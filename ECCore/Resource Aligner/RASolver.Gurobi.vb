Option Strict On

Imports ECCore
Imports ECCore.MiscFuncs
Imports Gurobi

Namespace Canvas

    Partial Public Class RASolver

        <NonSerialized> Private gModel As Gurobi.GRBModel = Nothing
        <NonSerialized> Private gModelForBaseCase As Gurobi.GRBModel = Nothing
        <NonSerialized> Public gEnv As Gurobi.GRBEnv = Nothing

        <NonSerialized> Private gVariables As New Dictionary(Of String, GRBVar)
        <NonSerialized> Private gConstraints As New Dictionary(Of String, GRBConstr)


        ' D3894 ===
        Public OPT_GUROBI_USE_CLOUD As Boolean = False  ' D4512
        Public OPT_GUROBI_ACCESS_ID As String = "a9ce3faa-185a-4419-a7ec-d3f4a19df18e"
        Public OPT_GUROBI_SECRETE_KEY As String = "fS2ENZd0TmeDbicgLnW9Ng"
        Public OPT_GUROBI_ID As String = "134005"
        Public OPT_GUROBI_POOL_ID As String = "134005-default"

        Public OPT_GUROBI_REST_URL_BASE As String = "https://cloud.gurobi.com/api/v2/"
        Public OPT_GUROBI_REST_LICENSES As String = "licenses"
        Public OPT_GUROBI_REST_MACHINES As String = "machines"
        ' D3894 ==

        Private mKeepGurobiAlive As Boolean = False

        Private MIPGap As Double = 0.0

        Public UseFundingPoolsPriorities As Boolean = False ' D7098

        Public Property FlexibleTimePeriodsResources As Boolean = False

        Public Property InfeasibilityConstraintsList As New List(Of ConstraintInfo)
        Private mInfeasibilityConstraintsCount As Integer = 0
        Public Property SolveForInfeasibilityAnalysis As Boolean = False

        <NonSerialized> Private gIndVariables As New Dictionary(Of String, GRBVar)

        Public Property KeepGurobiAlive As Boolean
            Get
                'Return False
                Return mKeepGurobiAlive
            End Get
            Set(value As Boolean)
                mKeepGurobiAlive = value
                If Not mKeepGurobiAlive Then
                    If gModel IsNot Nothing Then gModel.Dispose()
                    If gEnv IsNot Nothing Then gEnv.Dispose()
                End If
            End Set
        End Property

        Private Function Solve_Gurobi(Optional OutputPath As String = "", Optional SolverParams As String() = Nothing, Optional isCloud As Boolean = False, Optional SolutionNumber As Integer = 0) As Boolean
            If ProjectManager Is Nothing Then Return False
            PrintDebugInfo("Solve Gurobi started: ")
            ResetFunded()

            Dim usePresolve As Boolean = False

            Try
                Dim gEnv As GRBEnv

                If isCloud Then
                    gEnv = New GRBEnv(OutputPath, OPT_GUROBI_ACCESS_ID, OPT_GUROBI_SECRETE_KEY, OPT_GUROBI_POOL_ID)   ' D3870 + D3894
                Else
                    gEnv = New GRBEnv()
                End If
                PrintDebugInfo("Gurobi environment created: ")

                gVariables = New Dictionary(Of String, GRBVar)
                gConstraints = New Dictionary(Of String, GRBConstr)

                gModel = GetGurobiModel(gEnv, gVariables, gConstraints)

                PrintDebugInfo("Gurobi model created: ")

                gModel.Parameters.TimeLimit = 60
                gModel.Parameters.MIPGap = MIPGap

                If usePresolve Then
                    gModel = gModel.Presolve()
                End If

                gModel.Optimize()

                PrintDebugInfo("Gurobi model solved: ")

                Dim status As Integer = gModel.Get(GRB.IntAttr.Status)

                Select Case status
                    Case GRB.Status.OPTIMAL
                        SolverState = raSolverState.raSolved
                        Dim results As Dictionary(Of String, Double) = GetGurobiResults(gModel, gVariables, usePresolve, SolutionNumber)
                        SetGurobiResults(results)
                        mSolutionsFound = gModel.SolCount
                    Case GRB.Status.INFEASIBLE, GRB.Status.INF_OR_UNBD
                        SolverState = raSolverState.raInfeasible

                End Select

                ' D7078 ===
                If OutputPath <> "" Then
                    ' v9: The name of the file to be written. The file type is encoded in the file name suffix. Valid suffixes are .mps, .rew, .lp, or .rlp for writing the model itself, 
                    '     .ilp for writing just the IIS associated with an infeasible model (see GRBModel.ComputeIIS for further information), 
                    '     .sol for writing the current solution, .mst For writing a start vector, .hnt For writing a hint file, .bas For writing an LP basis, 
                    '     .prm For writing modified parameter settings, .attr For writing model attributes, Or .json for writing solution information in JSON format
                    OutputPath = OutputPath.Replace("\", "\\")
                    gModel.Write(String.Format("{0}.mps", OutputPath))
                    gModel.Write(String.Format("{0}.rew", OutputPath))
                    gModel.Write(String.Format("{0}.lp", OutputPath))
                    gModel.Write(String.Format("{0}.rlp", OutputPath))
                    gModel.Write(String.Format("{0}.sol", OutputPath))
                End If
                ' D7078 ==
                LastError = ""
                LastErrorReal = ""  ' D3880
            Catch ex As Exception
                SolverState = raSolverState.raError
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
                If gEnv IsNot Nothing Then gEnv.Dispose() Else LastError = "Solver %%solver%% is not available. Contact EC Technical Support"
            Finally
                If gModel IsNot Nothing And Not KeepGurobiAlive Then gModel.Dispose()
                If gEnv IsNot Nothing And Not KeepGurobiAlive Then gEnv.Dispose()
                gModel = Nothing
                gEnv = Nothing
            End Try

            PrintDebugInfo("Gurobi solve function completed: ")

            Return True
        End Function

        Public Sub Solve_Gurobi_EfficientFrontier(ByRef Settings As EfficientFrontierSettings, Optional OutputPath As String = "", Optional SolverParams As String() = Nothing, Optional isCloud As Boolean = False, Optional onAddLogFunction As onAddLogEvent = Nothing, Optional ConstraintType As EfficientFrontierConstraintType = EfficientFrontierConstraintType.BudgetLimit, Optional IsPointByPoint As Boolean = False)
            If ProjectManager Is Nothing Then Exit Sub

            'A1701 ===
            Dim oldScenarioID As Integer = ResourceAligner.Scenarios.ActiveScenarioID
            ResourceAligner.Scenarios.ActiveScenarioID = Settings.ScenarioID
            'A1701 ==

            PrintDebugInfo("Solve Gurobi Efficient Frontier started: ")
            'ResetFunded()

            Dim BaseCaseMax As Double = GetBaseCaseMaximum()

            Dim usePresolve As Boolean = False

            Dim IsSingleScenarioChecked As Boolean = ResourceAligner.Scenarios.Scenarios.Where(Function(scn As KeyValuePair(Of Integer, RAScenario)) scn.Value.IsCheckedIB).Count = 1
            Dim oldSettings As RASettings = New RASettings
            Try
                Dim gEnv As GRBEnv
                If isCloud Then
                    gEnv = New GRBEnv(OutputPath, OPT_GUROBI_ACCESS_ID, OPT_GUROBI_SECRETE_KEY, OPT_GUROBI_POOL_ID)   ' D3870 + D3894
                Else
                    gEnv = New GRBEnv()
                End If

                With CurrentScenario.Settings
                    'save current settings
                    oldSettings.Musts = .Musts
                    oldSettings.MustNots = .MustNots
                    oldSettings.CustomConstraints = .CustomConstraints
                    oldSettings.Dependencies = .Dependencies
                    oldSettings.Groups = .Groups
                    oldSettings.FundingPools = .FundingPools
                    oldSettings.Risks = .Risks
                    oldSettings.UseBaseCase = .UseBaseCase
                    oldSettings.BaseCaseForGroups = .BaseCaseForGroups
                    oldSettings.TimePeriods = .TimePeriods 'A1137

                    'use scenario comparison settings - ignores
                    If Not IsSingleScenarioChecked AndAlso ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions Then
                        .Musts = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.Musts
                        .MustNots = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.MustNots
                        .CustomConstraints = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.CustomConstraints
                        .Dependencies = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.Dependencies
                        .Groups = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.Groups
                        .FundingPools = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.FundingPools
                        .Risks = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.Risks
                        .TimePeriods = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.TimePeriods 'A1137
                    End If

                    'use scenario comparison settings - base case
                    If Not IsSingleScenarioChecked AndAlso ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions Then
                        .UseBaseCase = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCase
                        .BaseCaseForGroups = ResourceAligner.Scenarios.GlobalSettings.ScenarioComparisonSettings.BaseCaseForGroups
                    End If

                End With

                PrintDebugInfo("Gurobi environment created: ")

                Dim gVariables As New Dictionary(Of String, GRBVar)
                Dim gConstraints As New Dictionary(Of String, GRBConstr)

                gModel = GetGurobiModel(gEnv, gVariables, gConstraints)

                If ConstraintType = EfficientFrontierConstraintType.Risk Then
                    Dim constraintRisk As GRBLinExpr = 0.0
                    For Each alt As RAAlternative In Alternatives
                        constraintRisk.AddTerm(alt.RiskOriginal, gVariables(alt.ID))
                    Next

                    Dim riskVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "riskvar")
                    gVariables.Add("riskvar", riskVar)
                    constraintRisk.AddTerm(0, riskVar)
                    Dim RISKconstraint As GRBConstr = gModel.AddConstr(constraintRisk, GRB.LESS_EQUAL, 0, "risk")
                    gConstraints.Add("risk", RISKconstraint)

                    Dim constraintRiskVar As GRBLinExpr = 0.0
                    constraintRiskVar.AddTerm(1, riskVar)
                    Dim constraintRVar As GRBConstr = gModel.AddConstr(constraintRiskVar, GRB.EQUAL, 1, "riskvar")
                    gConstraints.Add("riskvar", constraintRVar)

                    gModel.Update()
                End If

                PrintDebugInfo("Gurobi model created: ")

                If OutputPath <> "" Then
                    OutputPath = OutputPath.Replace("\", "\\")
                    gModel.Write(OutputPath)
                End If

                gModel.Parameters.TimeLimit = 60
                gModel.Parameters.MIPGap = MIPGap

                'gModel.Tune()
                'PrintDebugInfo("Tuning model completed. Result count = " + gModel.TuneResultCount.ToString)
                'If gModel.TuneResultCount > 0 Then
                '    gModel.GetTuneResult(0)
                'End If

                If usePresolve Then
                    gModel = gModel.Presolve()
                End If

                Dim isCancelled As Boolean = False

                Dim CurrentFunded As New HashSet(Of String)
                Dim KeepFunded As Boolean = Settings.KeepFundedAlts

                For Each interval As EfficientFrontierInterval In Settings.Intervals
                    If Not Settings.IsIncreasing OrElse interval.DeltaType = EfficientFrontierDeltaType.AllSolutions Then KeepFunded = False
                    Select Case interval.DeltaType
                        Case EfficientFrontierDeltaType.NumberOfSteps, EfficientFrontierDeltaType.DeltaValue, EfficientFrontierDeltaType.AllSolutions
                            Dim stepValue As Double = If(interval.DeltaType = EfficientFrontierDeltaType.DeltaValue, interval.DeltaValue, (interval.MaxValue - interval.MinValue) / interval.DeltaValue)
                            Dim i As Double = If((interval.DeltaType = EfficientFrontierDeltaType.DeltaValue Or interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps) And Settings.IsIncreasing, interval.MinValue, interval.MaxValue)
                            While If((interval.DeltaType = EfficientFrontierDeltaType.DeltaValue Or interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps) And Settings.IsIncreasing, i <= interval.MaxValue, i >= interval.MinValue AndAlso (interval.Results.Count = 0 OrElse interval.Results(interval.Results.Count - 1).SolverState = raSolverState.raSolved))
                                If isCancelled Then Exit For

                                PrintDebugInfo("i = " + i.ToString)
                                Select Case ConstraintType
                                    Case EfficientFrontierConstraintType.BudgetLimit
                                        gModel.ChgCoeff(gConstraints("cost"), gVariables("costvar"), -i)
                                    Case EfficientFrontierConstraintType.Risk
                                        gModel.ChgCoeff(gConstraints("risk"), gVariables("riskvar"), -i)
                                    Case EfficientFrontierConstraintType.CustomConstraint
                                        If Not gConstraints.ContainsKey("CustomConstraint_" + Settings.ConstraintID.ToString + "_Min") Then
                                            Dim constraintCCMin As GRBLinExpr = 0.0
                                            For Each alt As RAAlternative In Alternatives
                                                Dim value As Double = Constraints.GetConstraintByID(Settings.ConstraintID).GetAlternativeValue(alt.ID)
                                                If value <> UNDEFINED_INTEGER_VALUE Then
                                                    constraintCCMin.AddTerm(value, gVariables(alt.ID))
                                                End If
                                            Next

                                            Dim ccminVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "ccminvar")
                                            gVariables.Add("ccminvar", ccminVar)
                                            constraintCCMin.AddTerm(-interval.MinValue, ccminVar)

                                            Dim termccMinVar As GRBLinExpr = 0.0
                                            termccMinVar.AddTerm(1, ccminVar)
                                            Dim constraintccminVar As GRBConstr = gModel.AddConstr(termccMinVar, GRB.EQUAL, 1, "ccminvarconstraint")
                                            gConstraints.Add("ccminvarconstraint", constraintccminVar)

                                            Dim ccConstr As GRBConstr = gModel.AddConstr(constraintCCMin, GRB.GREATER_EQUAL, 0, "CustomConstraint_" + Settings.ConstraintID.ToString + "_Min")
                                            gConstraints.Add("CustomConstraint_" + Settings.ConstraintID.ToString + "_Min", ccConstr)
                                        Else
                                            gModel.ChgCoeff(gConstraints("CustomConstraint_" + Settings.ConstraintID.ToString + "_Min"), gVariables("ccminvar"), -interval.MinValue)
                                        End If

                                        If Not gConstraints.ContainsKey("CustomConstraint_" + Settings.ConstraintID.ToString + "_Max") Then
                                            Dim constraintCCMax As GRBLinExpr = 0.0
                                            For Each alt As RAAlternative In Alternatives
                                                Dim value As Double = Constraints.GetConstraintByID(Settings.ConstraintID).GetAlternativeValue(alt.ID)
                                                If value <> UNDEFINED_INTEGER_VALUE Then
                                                    constraintCCMax.AddTerm(value, gVariables(alt.ID))
                                                End If
                                            Next

                                            Dim ccmaxVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "ccmaxvar")
                                            gVariables.Add("ccmaxvar", ccmaxVar)
                                            constraintCCMax.AddTerm(-i, ccmaxVar)

                                            Dim termccMaxVar As GRBLinExpr = 0.0
                                            termccMaxVar.AddTerm(1, ccmaxVar)
                                            Dim constraintccmaxVar As GRBConstr = gModel.AddConstr(termccMaxVar, GRB.EQUAL, 1, "ccmaxvarconstraint")
                                            gConstraints.Add("ccmaxvarconstraint", constraintccmaxVar)

                                            Dim ccConstr As GRBConstr = gModel.AddConstr(constraintCCMax, GRB.LESS_EQUAL, 0, "CustomConstraint_" + Settings.ConstraintID.ToString + "_Max")
                                            gConstraints.Add("CustomConstraint_" + Settings.ConstraintID.ToString + "_Max", ccConstr)
                                        Else
                                            gModel.ChgCoeff(gConstraints("CustomConstraint_" + Settings.ConstraintID.ToString + "_Max"), gVariables("ccmaxvar"), -i)
                                        End If
                                End Select

                                If KeepFunded Then
                                    ' make sure that previously funded alts are still funded
                                    If CurrentFunded.Count > 0 Then
                                        If gConstraints.ContainsKey("KeepFunded") Then
                                            gModel.Remove(gConstraints("KeepFunded"))
                                            gConstraints.Remove("KeepFunded")
                                        End If
                                        gModel.Update()
                                        Dim keepFundedConstraint As GRBLinExpr = 0.0
                                        For Each id As String In CurrentFunded
                                            keepFundedConstraint.AddTerm(1, gVariables(id))
                                        Next
                                        Dim constr As GRBConstr = gModel.AddConstr(keepFundedConstraint, GRB.EQUAL, CurrentFunded.Count, "KeepFunded")
                                        gConstraints.Add("KeepFunded", constr)
                                    End If
                                End If

                                gModel.Update()
                                gModel.Optimize()

                                PrintDebugInfo("Gurobi model solved: ")

                                Dim status As Integer = gModel.Get(GRB.IntAttr.Status)
                                Dim stepResults As EfficientFrontierResults = New EfficientFrontierResults
                                stepResults.SolveToken = Settings.SolveToken
                                Select Case status
                                    Case GRB.Status.OPTIMAL
                                        Dim gResults As Dictionary(Of String, Double) = GetGurobiResults(gModel, gVariables, usePresolve)
                                        stepResults = SetGurobiResultsEfficientFrontier(gResults)
                                        stepResults.SolveToken = Settings.SolveToken
                                        stepResults.Value = i
                                        stepResults.SolverState = raSolverState.raSolved
                                        stepResults.FundedBenefits /= If(BaseCaseMax = 0, 1, BaseCaseMax)
                                        Debug.Print("Solved")

                                        If KeepFunded Then
                                            For Each kvp As KeyValuePair(Of String, Double) In stepResults.AlternativesData
                                                If kvp.Value > 0.00001 Then
                                                    CurrentFunded.Add(kvp.Key)
                                                End If
                                            Next
                                        End If
                                    Case GRB.Status.INFEASIBLE, GRB.Status.INF_OR_UNBD
                                        stepResults.SolverState = raSolverState.raInfeasible
                                        Debug.Print("Infeasible")
                                End Select
                                stepResults.ScenarioID = Settings.ScenarioID
                                stepResults.ScenarioIndex = Settings.ScenarioIndex 'A1708

                                Dim includeStep As Boolean = False
                                If interval.Results.Count > 0 Then
                                    If interval.Results(interval.Results.Count - 1).AlternativesData.Count = stepResults.AlternativesData.Count Then
                                        For Each id As String In stepResults.AlternativesData.Keys
                                            If Not interval.Results(interval.Results.Count - 1).AlternativesData.Keys.Contains(id) Then
                                                includeStep = True
                                                Exit For
                                            End If
                                        Next
                                    Else
                                        includeStep = True
                                    End If
                                Else
                                    includeStep = True
                                End If

                                If includeStep Then
                                    interval.Results.Add(stepResults)
                                End If

                                interval.Results.Add(stepResults)

                                LastError = ""
                                LastErrorReal = ""  ' D3880

                                If interval.DeltaType = EfficientFrontierDeltaType.AllSolutions Then
                                    i = stepResults.FundedCost - interval.DeltaValue
                                Else
                                    Dim prevStepBudget As Double = i ' A1611
                                    If Settings.IsIncreasing Then
                                        i += stepValue
                                        If prevStepBudget < interval.MaxValue And i > interval.MaxValue Then i = interval.MaxValue 'A1611
                                    Else
                                        i -= stepValue
                                        If prevStepBudget > interval.MinValue And i < interval.MinValue Then i = interval.MinValue 'A1611
                                    End If
                                End If

                                If IsPointByPoint Then
                                    EfficientFrontierCallback(stepResults, -1, isCancelled)
                                End If
                            End While
                    End Select
                    If IsPointByPoint Then
                        EfficientFrontierCallback(Nothing, -1, isCancelled)
                    End If
                    interval.Results = interval.Results.OrderBy(Function(a) a.Value).ToList
                Next
            Catch ex As Exception
                SolverState = raSolverState.raError
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
                If gEnv IsNot Nothing Then gEnv.Dispose() Else LastError = "Solver %%solver%% is not available. Contact EC Technical Support"
            Finally
                With ResourceAligner.Scenarios.ActiveScenario.Settings
                    'restore current settions
                    .Musts = oldSettings.Musts
                    .MustNots = oldSettings.MustNots
                    .CustomConstraints = oldSettings.CustomConstraints
                    .Dependencies = oldSettings.Dependencies
                    .Groups = oldSettings.Groups
                    .FundingPools = oldSettings.FundingPools
                    .Risks = oldSettings.Risks
                    .UseBaseCase = oldSettings.UseBaseCase
                    .BaseCaseForGroups = oldSettings.BaseCaseForGroups
                    .TimePeriods = oldSettings.TimePeriods 'A1137
                End With

                If gModel IsNot Nothing And Not KeepGurobiAlive Then gModel.Dispose()
                If gEnv IsNot Nothing And Not KeepGurobiAlive Then gEnv.Dispose()
            End Try

            PrintDebugInfo("Gurobi efficient frontier solve completed: ")
            'PrintEfficientFrontierResults(Settings)

            ResourceAligner.Scenarios.ActiveScenarioID = oldScenarioID
        End Sub

        Private Function GetGurobiResults(gModel As GRBModel, variables As Dictionary(Of String, GRBVar), usePresolve As Boolean, Optional SolutionNumber As Integer = 0) As Dictionary(Of String, Double)
            Dim results As New Dictionary(Of String, Double)
            If Settings.FundingPools Then
                For Each FPools As RAFundingPools In CurrentScenario.ResourcePools.Values
                    For Each alt As RAAlternative In Alternatives
                        Dim s As String = alt.Name + ": "
                        For Each FP As RAFundingPool In FPools.Pools.Values
                            If FP.Enabled Then
                                Dim varName As String = "RP_" + FPools.ResourceID.ToString + "_FP" + FP.ID.ToString + "_" + alt.ID
                                'If usePresolve Then
                                '    results.Add("FP" + FP.ID.ToString + "_" + alt.ID, gModel.GetVarByName("FP" + FP.ID.ToString + "_" + alt.ID).Get(GRB.DoubleAttr.Xn))
                                'Else
                                '    results.Add("FP" + FP.ID.ToString + "_" + alt.ID, variables("FP" + FP.ID.ToString + "_" + alt.ID).Get(GRB.DoubleAttr.Xn))
                                'End If
                                If usePresolve Then
                                    results.Add(varName, gModel.GetVarByName(varName).Get(GRB.DoubleAttr.Xn))
                                Else
                                    results.Add(varName, variables(varName).Get(GRB.DoubleAttr.Xn))
                                    'results.Add("FP" + FP.ID.ToString + "_" + alt.ID, variables(varName).Get(GRB.DoubleAttr.Xn))
                                End If
                            End If
                        Next
                    Next
                Next
            End If

            Debug.Print("Solution = " + gModel.ObjVal.ToString)
            Debug.Print("Status = " + gModel.Status.ToString)
            Debug.Print("NumBinVars = " + gModel.NumBinVars.ToString)
            Debug.Print("NumVars = " + gModel.NumVars.ToString)
            Debug.Print("NumConstr = " + gModel.NumConstrs.ToString)

            For Each alt As RAAlternative In Alternatives
                Dim funded As Double
                If usePresolve Then
                    funded = gModel.GetVarByName(alt.ID).Get(GRB.DoubleAttr.Xn)
                Else
                    funded = variables(alt.ID).Get(GRB.DoubleAttr.Xn)
                End If
                results.Add(alt.ID, funded)
                Debug.Print(alt.Name + ": " + funded.ToString)
                If Settings.CostTolerance AndAlso alt.AllowCostTolerance Then
                    If variables.ContainsKey("soft_" + alt.ID) Then
                        'Debug.Print("soft = " + variables("soft_" + alt.ID).Get(GRB.DoubleAttr.Xn).ToString)
                        alt.FundedCost = variables("soft_" + alt.ID).Get(GRB.DoubleAttr.Xn)
                    End If
                    'If variables.ContainsKey("softC_" + alt.ID) Then
                    '    Debug.Print("softC = " + variables("softC_" + alt.ID).Get(GRB.DoubleAttr.Xn).ToString)
                    'End If
                End If

                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                'Debug.Print("Alt: " + alt.Name + "; funded = " + funded.ToString)
                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                '    For j As Integer = apd.MinPeriod To apd.MaxPeriod
                '        If usePresolve Then
                '            Debug.Print("Period " + j.ToString + ": " + gModel.GetVarByName("TPVar_" + alt.ID.ToString + "_" + j.ToString).Get(GRB.DoubleAttr.Xn).ToString)
                '        Else
                '            Debug.Print("Period " + j.ToString + ": " + variables("TPVar_" + alt.ID.ToString + "_" + j.ToString).Get(GRB.DoubleAttr.Xn).ToString)
                '        End If
                '    Next
                'End If
                If funded > 0.00001 Then
                    If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If usePresolve Then
                                results.Add("TPVar_" + alt.ID.ToString + "_" + j.ToString, gModel.GetVarByName("TPVar_" + alt.ID.ToString + "_" + j.ToString).Get(GRB.DoubleAttr.Xn))
                            Else
                                results.Add("TPVar_" + alt.ID.ToString + "_" + j.ToString, variables("TPVar_" + alt.ID.ToString + "_" + j.ToString).Get(GRB.DoubleAttr.Xn))
                            End If
                        Next
                    End If
                End If
            Next

            If SolveForInfeasibilityAnalysis Then
                For Each c As ConstraintInfo In InfeasibilityConstraintsList
                    If usePresolve Then
                        results.Add(c.ID, gModel.GetVarByName(c.ID).Get(GRB.DoubleAttr.Xn))
                    Else
                        results.Add(c.ID, gIndVariables(c.ID).Get(GRB.DoubleAttr.Xn))
                    End If
                    Dim value As Double = results(c.ID)
                    c.Selected = (value = 1)
                    Debug.Print(c.Name + ": " + value.ToString)
                Next
            End If

            'For Each vName As String In gIndVariables.Keys
            '    Debug.Print(vName + ": " + gIndVariables(vName).Get(GRB.DoubleAttr.Xn).ToString)
            'Next

            Return results
        End Function

        Private Function SetGurobiResultsEfficientFrontier(results As Dictionary(Of String, Double)) As EfficientFrontierResults
            Dim efResults As New EfficientFrontierResults

            Dim value As Double
            Dim altValue As Double
            'If Settings.FundingPools Then
            '    For Each alt As RAAlternative In Alternatives
            '        Dim s As String = alt.Name + ": "
            '        For Each FP As RAFundingPool In FundingPools.Pools.Values
            '            If FP.Enabled Then
            '                If results.TryGetValue("FP" + FP.ID.ToString + "_" + alt.ID, value) Then
            '                    FP.SetAlternativeAllocatedValue(alt.ID, value)
            '                End If
            '            End If
            '        Next
            '    Next
            'End If

            'mFundedCost = 0
            'mFundedBenefit = 0
            'mFundedOriginalBenefit = 0

            'CurrentScenario.TimePeriods.TimePeriodResults.Clear()

            Dim i As Integer = 1
            For Each alt As RAAlternative In Alternatives
                If results.TryGetValue(alt.ID, altValue) Then
                    efResults.AlternativesData.Add(alt.ID, altValue)
                    'alt.Funded = value
                End If

                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                If altValue > 0.00001 Then
                    'mFundedCost += alt.Cost * alt.Funded
                    efResults.FundedCost += alt.Cost * altValue

                    Dim dFactorPower As Integer = 0
                    Dim discount As Double = 1
                    If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim fundedPeriod As Integer = -1
                        For j As Integer = apd.MinPeriod To apd.MaxPeriod
                            Dim TPVarFunded As Boolean
                            If results.TryGetValue("TPVar_" + alt.ID.ToString + "_" + j.ToString, value) Then
                                TPVarFunded = value > 0
                            End If

                            If fundedPeriod = -1 AndAlso TPVarFunded Then
                                fundedPeriod = j
                            End If
                            i += 1
                        Next
                        If fundedPeriod <> -1 Then
                            'CurrentScenario.TimePeriods.TimePeriodResults.Add(alt.ID, fundedPeriod)
                            If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                dFactorPower = fundedPeriod - apd.MinPeriod
                                discount = 1 / Math.Pow(1 + CurrentScenario.TimePeriods.DiscountFactor, dFactorPower)
                            End If
                        End If
                    End If

                    Dim RiskBenefit As Double = CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal))
                    'mFundedBenefit += RiskBenefit * discount * alt.Funded
                    'mFundedOriginalBenefit += alt.BenefitOriginal * discount * alt.Funded
                    efResults.FundedBenefits += RiskBenefit * discount * altValue
                    efResults.FundedBenefitsOriginal += alt.BenefitOriginal * discount * altValue
                Else
                    'alt.Funded = 0
                    i += apd.MaxPeriod - apd.MinPeriod + 1
                End If
            Next

            Return efResults
        End Function

        Private Sub SetGurobiResults(results As Dictionary(Of String, Double))
            Dim value As Double
            If Settings.FundingPools Then
                For Each ResourcePool As RAFundingPools In CurrentScenario.ResourcePools.Values
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In ResourcePool.Pools.Values
                            If FP.Enabled Then
                                If results.TryGetValue("RP_" + ResourcePool.ResourceID.ToString + "_FP" + FP.ID.ToString + "_" + alt.ID, value) Then
                                    FP.SetAlternativeAllocatedValue(alt.ID, value)
                                End If
                            End If
                        Next
                    Next
                Next
            End If

            mFundedCost = 0
            mFundedBenefit = 0
            mFundedOriginalBenefit = 0

            CurrentScenario.TimePeriods.TimePeriodResults.Clear()

            Dim i As Integer = 1
            For Each alt As RAAlternative In Alternatives
                If results.TryGetValue(alt.ID, value) Then
                    alt.Funded = value
                End If

                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                If alt.Funded > 0.00001 Then
                    mFundedCost += alt.Funded * If(Settings.CostTolerance AndAlso alt.AllowCostTolerance, alt.FundedCost, alt.Cost)

                    Dim dFactorPower As Integer = 0
                    Dim discount As Double = 1
                    If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim fundedPeriod As Integer = -1
                        For j As Integer = apd.MinPeriod To apd.MaxPeriod
                            Dim TPVarFunded As Boolean
                            If results.TryGetValue("TPVar_" + alt.ID.ToString + "_" + j.ToString, value) Then
                                TPVarFunded = value > 0
                            End If

                            If fundedPeriod = -1 AndAlso TPVarFunded Then
                                fundedPeriod = j
                            End If
                            i += 1
                        Next
                        If fundedPeriod <> -1 Then
                            CurrentScenario.TimePeriods.TimePeriodResults.Add(alt.ID, fundedPeriod)
                            If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                dFactorPower = fundedPeriod - apd.MinPeriod
                                discount = 1 / Math.Pow(1 + CurrentScenario.TimePeriods.DiscountFactor, dFactorPower)
                            End If
                        End If
                    End If

                    Dim RiskBenefit As Double = CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal))
                    mFundedBenefit += RiskBenefit * discount * alt.Funded
                    mFundedOriginalBenefit += alt.BenefitOriginal * discount * alt.Funded
                Else
                    alt.Funded = 0
                    i += apd.MaxPeriod - apd.MinPeriod + 1
                End If
            Next
        End Sub

        Private Function GetGurobiModel(gEnv As GRBEnv, ByRef gVariables As Dictionary(Of String, GRBVar), ByRef gConstraints As Dictionary(Of String, GRBConstr)) As GRBModel
            If ProjectManager Is Nothing Then Return Nothing
            Try
                gModel = New Gurobi.GRBModel(gEnv)

                Dim maxCost As Double = Alternatives.Max(Function(x) x.Cost)
                Dim maxBenefit As Double = Alternatives.Max(Function(x) x.BenefitOriginal)
                Dim sumCost As Double = Alternatives.Sum(Function(x) x.Cost)
                Dim sumBenefit As Double = Alternatives.Sum(Function(x) x.BenefitOriginal)
                Dim eps As Double = 0.0001

                gVariables = New Dictionary(Of String, GRBVar)
                gIndVariables = New Dictionary(Of String, GRBVar)
                gConstraints = New Dictionary(Of String, GRBConstr)

                ' ========= DECLARING GENERAL VARIABLES =========

                ' DECLARE VARIABLES FOR ALTERNATIVES
                Dim altVar As GRBVar
                For Each alt As RAAlternative In Alternatives
                    If alt.IsPartial Then
                        altVar = gModel.AddVar(alt.MinPercent, 1, 0, GRB.SEMICONT, alt.ID)
                    Else
                        altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, alt.ID)
                    End If
                    gVariables.Add(alt.ID, altVar)
                    If Settings.CostTolerance AndAlso alt.AllowCostTolerance Then
                        Dim softCostVar As GRBVar = gModel.AddVar(0, alt.Cost, 0, GRB.CONTINUOUS, "soft_" + alt.ID)
                        gVariables.Add("soft_" + alt.ID, softCostVar)

                        Dim softCostVarC As GRBVar = gModel.AddVar(alt.Cost - alt.CostDelta, alt.Cost, 0, GRB.CONTINUOUS, "softC_" + alt.ID)
                        gVariables.Add("softC_" + alt.ID, softCostVarC)
                    End If
                Next

                ' DECLARE VARIABLES FOR FUNDING POOLS
                'If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                If Settings.FundingPools Then
                    'For Each alt As RAAlternative In Alternatives
                    '    For Each FP As RAFundingPool In FundingPools.Pools.Values
                    '        If FP.Enabled Then
                    '            Dim varName As String = "FP" + FP.ID.ToString + "_" + alt.ID
                    '            Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                    '            If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = Integer.MaxValue
                    '            Dim fpVar As GRBVar = gModel.AddVar(0, cellCost, 0, GRB.CONTINUOUS, varName)
                    '            gVariables.Add(varName, fpVar)
                    '        End If
                    '    Next
                    'Next

                    For Each alt As RAAlternative In Alternatives
                        ' for each resource
                        For Each FPools As RAFundingPools In CurrentScenario.ResourcePools.Values
                            Dim resource As RAResource = If(CurrentScenario.TimePeriods.Resources.ContainsKey(FPools.ResourceID), CurrentScenario.TimePeriods.Resources(FPools.ResourceID), Nothing)
                            Dim cc As RAConstraint = If(resource IsNot Nothing, CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID), Nothing)

                            Dim rValue As Double
                            If FPools.ResourceID.Equals(RA_Cost_GUID) Then
                                rValue = alt.Cost
                            Else
                                rValue = If(cc IsNot Nothing, cc.GetAlternativeValue(alt.ID), 0)
                            End If
                            If rValue <> UNDEFINED_INTEGER_VALUE Then
                            End If

                            ' for each resource pool
                            For Each FP As RAFundingPool In FPools.Pools.Values
                                If FP.Enabled Then

                                    Dim varName As String = "RP_" + FPools.ResourceID.ToString + "_FP" + FP.ID.ToString + "_" + alt.ID
                                    Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                                    If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = Integer.MaxValue
                                    If rValue = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                    Dim fpVar As GRBVar = gModel.AddVar(0, cellCost, 0, GRB.CONTINUOUS, varName)
                                    gVariables.Add(varName, fpVar)
                                End If
                            Next
                        Next
                    Next
                End If

                ' DECLARE VARIABLES FOR TIME PERIODS
                Dim TPvariables As New Dictionary(Of String, GRBVar)
                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If Not gVariables.ContainsKey("TPVar_" + alt.ID.ToString + "_" + j.ToString) Then
                                If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                    altVar = gModel.AddVar(0, 1, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v), GRB.BINARY, "TPVar_" + alt.ID.ToString + "_" + j.ToString)
                                Else
                                    altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "TPVar_" + alt.ID.ToString + "_" + j.ToString)
                                End If
                                gVariables.Add("TPVar_" + alt.ID.ToString + "_" + j.ToString, altVar)
                                TPvariables.Add("TPVar_" + alt.ID.ToString + "_" + j.ToString, altVar)
                            End If
                            v += 1
                            i += 1
                        Next
                    Next
                End If

                If FlexibleTimePeriodsResources AndAlso Settings.TimePeriods AndAlso CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                For Each alt As RAAlternative In Alternatives

                                    Dim ccValue As Double = UNDEFINED_INTEGER_VALUE
                                    If resource.ID.Equals(RA_Cost_GUID) Then
                                        ccValue = alt.Cost
                                    Else
                                        If cc IsNot Nothing Then
                                            ccValue = cc.GetAlternativeValue(alt.ID)
                                        End If
                                    End If

                                    Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                    If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                        For k As Integer = Math.Max(apd.MinPeriod, t - apd.Duration + 1) To Math.Min(t, apd.MaxPeriod)
                                            Dim sName As String = "TPVarFlex_" + alt.ID.ToString + "_" + resource.ID.ToString + "_" + t.ToString
                                            If Not gVariables.ContainsKey(sName) Then
                                                altVar = gModel.AddVar(0, ccValue, 0, GRB.CONTINUOUS, sName)
                                                gVariables.Add(sName, altVar)
                                                TPvariables.Add(sName, altVar)
                                            End If
                                        Next
                                    End If
                                Next
                            End If
                        Next
                        t += 1
                    Next

                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                        Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                        If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                            For Each alt As RAAlternative In Alternatives
                                Dim ccValue As Double = UNDEFINED_INTEGER_VALUE
                                If resource.ID.Equals(RA_Cost_GUID) Then
                                    ccValue = alt.Cost
                                Else
                                    If cc IsNot Nothing Then
                                        ccValue = cc.GetAlternativeValue(alt.ID)
                                    End If
                                End If
                                If ccValue <> UNDEFINED_INTEGER_VALUE Then
                                    Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        Dim cTPResFlex As GRBLinExpr = 0.0
                                        For k As Integer = 0 To apd.Duration - 1
                                            cTPResFlex.AddTerm(1, gVariables("TPVarFlex_" + alt.ID.ToString + "_" + resource.ID.ToString + "_" + (j + k).ToString))
                                        Next
                                        'Dim indFlex As GRBGenConstr = gModel.AddGenConstrIndicator(gVariables("TPVar_" + alt.ID.ToString + "_" + j.ToString), 1, cTPResFlex, GRB.EQUAL, ccValue, "TPResSum" + alt.ID.ToString + "_" + j.ToString + "_" + resource.ID.ToString)
                                    Next
                                End If
                            Next
                        End If
                    Next
                End If

                ' DECLARE VARIABLES FOR GROUPS WITH CONDITION "ALL OR NOTHING"
                If Settings.Groups Then
                    For Each group As RAGroup In Groups.Groups.Values
                        Dim filteredAlts As List(Of RAAlternative) = group.GetFilteredAlternatives(CurrentScenario.Alternatives)
                        If group.Enabled AndAlso filteredAlts.Count > 1 AndAlso group.Condition = RAGroupCondition.gcAllOrNothing Then
                            Dim gVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "g_" + group.ID.ToString)
                            gVariables.Add("g_" + group.ID.ToString, gVar)
                        End If
                    Next
                End If

                'UPDATE THE MODEL TO MAKE VARIABLES AVAILABLE
                gModel.Update()


                ' ========= SETTING MAIN AND COST ObJECTIVE FUNCTIONS =========

                Dim objBenefits As GRBLinExpr = 0.0
                Dim objCost As GRBLinExpr = 0.0
                Dim objToleranceCost As GRBLinExpr = 0.0
                Dim hasToleranceObjective As Boolean = False
                Dim constraintCost As GRBLinExpr = 0.0
                For Each alt As RAAlternative In Alternatives
                    If UseFundingPoolsPriorities AndAlso Settings.FundingPools AndAlso FundingPools.Pools.Count > 0 Then
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                Dim varName As String = "FP" + FP.ID.ToString + "_" + alt.ID
                                Dim fpVar As GRBVar = gVariables(varName)
                                Dim cellPrty As Double = FP.GetAlternativePriority(alt.ID)
                                If cellPrty <> UNDEFINED_INTEGER_VALUE Then
                                    objBenefits.AddTerm((CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal))) * cellPrty, fpVar)
                                End If
                            End If
                        Next
                    Else
                        objBenefits.AddTerm(CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)), gVariables(alt.ID))
                    End If

                    If Settings.CostTolerance AndAlso alt.AllowCostTolerance Then
                        objCost.AddTerm(1, gVariables("soft_" + alt.ID))
                        objToleranceCost.AddTerm(1, gVariables("soft_" + alt.ID))
                        hasToleranceObjective = True
                        constraintCost.AddTerm(1, gVariables("soft_" + alt.ID))

                        ' Cmin * x <= z <= Cmax * x
                        Dim z1 As GRBLinExpr = 0.0
                        z1.AddTerm(alt.Cost - alt.CostDelta, gVariables(alt.ID))
                        z1.AddTerm(-1, gVariables("soft_" + alt.ID))
                        Dim constrZ1 As GRBConstr = gModel.AddConstr(z1, GRB.LESS_EQUAL, 0, "z1_" + alt.ID)
                        gConstraints.Add("z1_" + alt.ID, constrZ1)

                        Dim z2 As GRBLinExpr = 0.0
                        z2.AddTerm(alt.Cost, gVariables(alt.ID))
                        z2.AddTerm(-1, gVariables("soft_" + alt.ID))
                        Dim constrZ2 As GRBConstr = gModel.AddConstr(z2, GRB.GREATER_EQUAL, 0, "z2_" + alt.ID)
                        gConstraints.Add("z2_" + alt.ID, constrZ2)

                        ' C - (1 - x) * Cmax <= z <= C - (1 - x) * Cmin
                        ' Cmax * x - z + C <= Cmax
                        Dim z3 As GRBLinExpr = 0.0
                        z3.AddTerm(alt.Cost, gVariables(alt.ID))
                        z3.AddTerm(-1, gVariables("soft_" + alt.ID))
                        z3.AddTerm(1, gVariables("softC_" + alt.ID))
                        Dim constrZ3 As GRBConstr = gModel.AddConstr(z3, GRB.LESS_EQUAL, alt.Cost, "z3_" + alt.ID)
                        gConstraints.Add("z3_" + alt.ID, constrZ3)

                        ' Cmin * x - z + C >= 1
                        Dim z4 As GRBLinExpr = 0.0
                        z4.AddTerm(alt.Cost - alt.CostDelta, gVariables(alt.ID))
                        z4.AddTerm(-1, gVariables("soft_" + alt.ID))
                        z4.AddTerm(1, gVariables("softC_" + alt.ID))
                        Dim constrZ4 As GRBConstr = gModel.AddConstr(z4, GRB.GREATER_EQUAL, alt.Cost - alt.CostDelta, "z4_" + alt.ID)
                        gConstraints.Add("z4_" + alt.ID, constrZ4)

                        ' z <= C + (1 - x) * Cmax
                        Dim z5 As GRBLinExpr = 0.0
                        z5.AddTerm(-alt.Cost, gVariables(alt.ID))
                        z5.AddTerm(-1, gVariables("soft_" + alt.ID))
                        z5.AddTerm(1, gVariables("softC_" + alt.ID))
                        Dim constrZ5 As GRBConstr = gModel.AddConstr(z5, GRB.GREATER_EQUAL, -alt.Cost, "z5_" + alt.ID)
                        gConstraints.Add("z5_" + alt.ID, constrZ5)
                    Else
                        objCost.AddTerm(alt.Cost, gVariables(alt.ID))
                        constraintCost.AddTerm(alt.Cost, gVariables(alt.ID))
                    End If
                Next

                For Each tpv As GRBVar In TPvariables.Values
                    objBenefits.AddTerm(tpv.Obj, tpv)
                Next

                Dim constraintInfeasibilityIndicators As GRBLinExpr = 0.0
                Dim numOfConstraintsForInfeasibilities As Integer = 0
                Dim IndicatorConstraints As New Dictionary(Of String, GRBGenConstr)

                'gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, BudgetLimit, "cost")
                Dim costVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "costvar")
                gVariables.Add("costvar", costVar)
                constraintCost.AddTerm(-BudgetLimit, costVar)

                If Not (UseFundingPoolsPriorities AndAlso Settings.FundingPools AndAlso FundingPools.Pools.Count > 0) Then
                    Dim COSTconstraint As GRBConstr = gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, 0, "cost")
                    gConstraints.Add("cost", COSTconstraint)
                End If

                'If SolveForInfeasibilityAnalysis Then
                '    If InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("cost")) IsNot Nothing Then
                '        Dim costVarIndicator As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "costVarIndicator")
                '        gVariables.Add("costVarIndicator", costVarIndicator)
                '        gIndVariables.Add("costVarIndicator", costVarIndicator)
                '        constraintInfeasibilityIndicators.AddTerm(1, costVarIndicator)

                '        Dim costConstr As GRBGenConstr = gModel.AddGenConstrIndicator(costVarIndicator, 1, constraintCost, GRB.EQUAL, 1, "Cost_")
                '        IndicatorConstraints.Add("Cost_", costConstr)
                '        numOfConstraintsForInfeasibilities += 1
                '    End If
                'Else
                '    Dim COSTconstraint As GRBConstr = gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, 0, "cost")
                '    gConstraints.Add("cost", COSTconstraint)
                'End If

                Dim maxPriority As Integer = 1000

                gModel.ModelSense = GRB.MAXIMIZE
                'gModel.SetObjectiveN(objBenefits, 0, maxPriority, 1, 0, 0, "benefits")
                'gModel.SetObjective(objBenefits, GRB.MAXIMIZE)

                Dim objIndex As Integer = 1
                If hasToleranceObjective Then
                    gModel.SetObjectiveN(objToleranceCost, objIndex, maxPriority - 1, 1, 0, 0, "costTolerance")
                    maxPriority -= 1
                    objIndex += 1
                End If

                Dim useAdditionalObjectives As Boolean = True

                If CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID) IsNot Nothing Then
                    Dim useCostObjective As Boolean = useAdditionalObjectives AndAlso CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).InUse
                    Dim costPrty As Integer = maxPriority - CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).Rank
                    Dim costMaximize As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).Condition = RASolverPriority.raSolverCondition.raMaximize
                    If useCostObjective Then
                        gModel.SetObjectiveN(If(costMaximize, 1, -1) * objCost, objIndex, costPrty, 1, 0, 0, "costs")
                        objIndex += 1
                    End If
                End If

                ' ========= SETTING CONSTRAINTS =========
                Dim mustCount As Integer = Alternatives.Where(Function(x) x.Must).Count
                Dim mustnotCount As Integer = Alternatives.Where(Function(x) x.MustNot).Count

                ' HANDLE MUSTS
                Dim constraintMusts As GRBLinExpr = 0.0
                If Settings.Musts And (mustCount > 0) Then
                    Dim remainingMusts As Integer = 0
                    For Each alt As RAAlternative In Alternatives
                        If SolveForInfeasibilityAnalysis AndAlso alt.Must AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("mustVar_" + alt.ID)) IsNot Nothing Then
                            Dim mustVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "mustVar_" + alt.ID)
                            gVariables.Add("mustVar_" + alt.ID, mustVar)
                            gIndVariables.Add("mustVar_" + alt.ID, mustVar)
                            constraintInfeasibilityIndicators.AddTerm(1, mustVar)

                            Dim constraintMustIndividual As GRBLinExpr = 0.0
                            constraintMustIndividual.AddTerm(1, gVariables(alt.ID))
                            Dim mustConstrIndividual As GRBGenConstr = gModel.AddGenConstrIndicator(mustVar, 1, constraintMustIndividual, GRB.EQUAL, 1, "Must_" + alt.ID)
                            IndicatorConstraints.Add("Must_" + alt.ID, mustConstrIndividual)
                            numOfConstraintsForInfeasibilities += 1
                        Else
                            remainingMusts += If(alt.Must, 1, 0)
                            constraintMusts.AddTerm(CDbl(If(alt.Must, 1, 0)), gVariables(alt.ID))
                        End If
                    Next

                    If remainingMusts > 0 Then
                        Dim mustConstr As GRBConstr = gModel.AddConstr(constraintMusts, GRB.EQUAL, remainingMusts, "musts")
                        gConstraints.Add("musts", mustConstr)
                    End If
                End If

                ' HANDLE MUSTNOTS
                Dim constraintMustNots As GRBLinExpr = 0.0
                If Settings.MustNots And (mustnotCount > 0) Then
                    Dim remainingMustNots As Integer = 0
                    For Each alt As RAAlternative In Alternatives
                        If SolveForInfeasibilityAnalysis AndAlso alt.MustNot AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("mustNotVar_" + alt.ID)) IsNot Nothing Then
                            Dim mustNotVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "mustNotVar_" + alt.ID)
                            gVariables.Add("mustNotVar_" + alt.ID, mustNotVar)
                            gIndVariables.Add("mustNotVar_" + alt.ID, mustNotVar)
                            constraintInfeasibilityIndicators.AddTerm(1, mustNotVar)

                            Dim constraintMustNotIndividual As GRBLinExpr = 0.0
                            constraintMustNotIndividual.AddTerm(1, gVariables(alt.ID))
                            Dim mustNotConstrIndividual As GRBGenConstr = gModel.AddGenConstrIndicator(mustNotVar, 1, constraintMustNotIndividual, GRB.EQUAL, 0, "MustNot_" + alt.ID)
                            IndicatorConstraints.Add("Must_" + alt.ID, mustNotConstrIndividual)
                            numOfConstraintsForInfeasibilities += 1
                        Else
                            remainingMustNots += If(alt.MustNot, 1, 0)
                            constraintMustNots.AddTerm(CDbl(If(alt.MustNot, 1, 0)), gVariables(alt.ID))
                        End If
                    Next

                    If remainingMustNots > 0 Then
                        Dim mustNotConstr As GRBConstr = gModel.AddConstr(constraintMustNots, GRB.EQUAL, 0, "mustnots")
                        gConstraints.Add("mustnots", mustNotConstr)
                    End If
                End If

                ' HANDLE GROUPS
                If Settings.Groups Then
                    Dim i As Integer = 1

                    For Each group As RAGroup In Groups.Groups.Values
                        Dim filteredAlts As List(Of RAAlternative) = group.GetFilteredAlternatives(CurrentScenario.Alternatives)
                        If group.Enabled AndAlso filteredAlts.Count > 1 Then
                            Dim gName As String = "Group_" + i.ToString
                            Dim constraintGroup As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                constraintGroup.AddTerm(CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)), gVariables(alt.ID))
                            Next

                            Dim groupVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "gVar_" + group.ID.ToString)
                            gVariables.Add("gVar_" + group.ID.ToString, groupVar)
                            gIndVariables.Add("gVar_" + group.ID.ToString, groupVar)
                            constraintInfeasibilityIndicators.AddTerm(1, groupVar)

                            Dim groupConstr As GRBConstr = Nothing
                            Dim groupIndConstr As GRBGenConstr = Nothing
                            Select Case group.Condition
                                Case RAGroupCondition.gcLessOrEqualsOne
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("gVar_" + group.ID.ToString)) IsNot Nothing Then
                                        groupIndConstr = gModel.AddGenConstrIndicator(groupVar, 1, constraintGroup, GRB.LESS_EQUAL, 1, gName)
                                    Else
                                        groupConstr = gModel.AddConstr(constraintGroup, GRB.LESS_EQUAL, 1, gName)
                                    End If
                                Case RAGroupCondition.gcEqualsOne
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("gVar_" + group.ID.ToString)) IsNot Nothing Then
                                        groupIndConstr = gModel.AddGenConstrIndicator(groupVar, 1, constraintGroup, GRB.EQUAL, 1, gName)
                                    Else
                                        groupConstr = gModel.AddConstr(constraintGroup, GRB.EQUAL, 1, gName)
                                    End If
                                Case RAGroupCondition.gcGreaterOrEqualsOne
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("gVar_" + group.ID.ToString)) IsNot Nothing Then
                                        groupIndConstr = gModel.AddGenConstrIndicator(groupVar, 1, constraintGroup, GRB.GREATER_EQUAL, 1, gName)
                                    Else
                                        groupConstr = gModel.AddConstr(constraintGroup, GRB.GREATER_EQUAL, 1, gName)
                                    End If
                                Case RAGroupCondition.gcAllOrNothing
                                    constraintGroup.AddTerm(-filteredAlts.Count, gVariables("g_" + group.ID.ToString))
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("gVar_" + group.ID.ToString)) IsNot Nothing Then
                                        groupConstr = gModel.AddConstr(constraintGroup, GRB.EQUAL, 0, gName)
                                    Else
                                        groupIndConstr = gModel.AddGenConstrIndicator(groupVar, 1, constraintGroup, GRB.EQUAL, 0, gName)
                                    End If
                            End Select
                            If groupConstr IsNot Nothing Then gConstraints.Add(gName, groupConstr)
                            If groupIndConstr IsNot Nothing Then
                                IndicatorConstraints.Add(gName, groupIndConstr)
                                numOfConstraintsForInfeasibilities += 1
                            End If
                            i += 1
                        End If
                    Next
                End If

                ' HANDLE DEPENDENCIES
                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In Dependencies.Dependencies
                        If Dependency.Enabled Then
                            Dim dependencyIndConstr As GRBGenConstr = Nothing

                            Dim constraintDependency As GRBLinExpr = 0.0
                            Dim dependencyVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "dVar_" + i.ToString)
                            constraintInfeasibilityIndicators.AddTerm(1, dependencyVar)
                            gVariables.Add("dVar_" + i.ToString, dependencyVar)
                            gIndVariables.Add("dVar_" + i.ToString, dependencyVar)

                            'Dim dependencyIndConstr As GRBGenConstr = Nothing
                            For Each alt As RAAlternative In Alternatives
                                Select Case Dependency.Value
                                    Case RADependencyType.dtDependsOn
                                        ' first is not funded unless second is funded
                                        constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)), gVariables(alt.ID))
                                    Case RADependencyType.dtMutuallyDependent
                                        constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)), gVariables(alt.ID))
                                    Case RADependencyType.dtMutuallyExclusive
                                        constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) + CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)), gVariables(alt.ID))
                                End Select
                            Next
                            Dim dName As String = ""
                            Dim depConstr As GRBConstr = Nothing
                            Select Case Dependency.Value
                                Case RADependencyType.dtDependsOn
                                    dName = "Dependency" + i.ToString + "_DependsOn"
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("dVar_" + i.ToString)) IsNot Nothing Then
                                        dependencyIndConstr = gModel.AddGenConstrIndicator(dependencyVar, 1, constraintDependency, GRB.GREATER_EQUAL, 0, dName)
                                    Else
                                        depConstr = gModel.AddConstr(constraintDependency, GRB.GREATER_EQUAL, 0, dName)
                                    End If
                                Case RADependencyType.dtMutuallyDependent
                                    dName = "Dependency" + i.ToString + "_MutuallyDependent"
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("dVar_" + i.ToString)) IsNot Nothing Then
                                        dependencyIndConstr = gModel.AddGenConstrIndicator(dependencyVar, 1, constraintDependency, GRB.EQUAL, 0, dName)
                                    Else
                                        depConstr = gModel.AddConstr(constraintDependency, GRB.EQUAL, 0, dName)
                                    End If
                                Case RADependencyType.dtMutuallyExclusive
                                    dName = "Dependency" + i.ToString + "_MutuallyExclusive"
                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("dVar_" + i.ToString)) IsNot Nothing Then
                                        dependencyIndConstr = gModel.AddGenConstrIndicator(dependencyVar, 1, constraintDependency, GRB.LESS_EQUAL, 1, dName)
                                    Else
                                        depConstr = gModel.AddConstr(constraintDependency, GRB.LESS_EQUAL, 1, dName)
                                    End If
                            End Select
                            If depConstr IsNot Nothing Then gConstraints.Add(dName, depConstr)
                            If dependencyIndConstr IsNot Nothing Then
                                IndicatorConstraints.Add(dName, dependencyIndConstr)
                                numOfConstraintsForInfeasibilities += 1
                            End If
                            i += 1
                        End If
                    Next
                End If

                If Settings.CustomConstraints Then
                    'Dim i As Integer = 1
                    For Each Constraint As RAConstraint In Constraints.Constraints.Values
                        If Constraint.Enabled AndAlso CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID) IsNot Nothing Then
                            Dim useCCasObj As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID).InUse
                            Dim CCPriority As Integer = maxPriority - CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID).Rank
                            Dim CCIsMax As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID).Condition = RASolverPriority.raSolverCondition.raMaximize
                            If useCCasObj Then
                                Dim constraintObj As GRBLinExpr = 0.0
                                For Each alt As RAAlternative In Alternatives
                                    Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                                    If value <> UNDEFINED_INTEGER_VALUE Then
                                        constraintObj.AddTerm(value, gVariables(alt.ID))
                                    End If
                                Next
                                'gModel.SetObjectiveN(constraintObj, objIndex, CCPriority, CDbl(If(CCIsMax, 1, -1)), 0, 0, "CCObj_" + objIndex.ToString)
                                gModel.SetObjectiveN(CInt(If(CCIsMax, 1, -1)) * constraintObj, objIndex, CCPriority, 1, 0, 0, "CCObj_" + objIndex.ToString)
                                objIndex += 1
                            End If
                        End If

                        If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
                            ' for each custom constraint we create a vector that holds custom constrain values in the field Cost
                            'Dim cName As String = "CustomConstraint_" + i.ToString
                            Dim cName As String = "CustomConstraint_" + Constraint.ID.ToString
                            Dim constraintCC As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                                If value <> UNDEFINED_INTEGER_VALUE Then
                                    constraintCC.AddTerm(value, gVariables(alt.ID))
                                End If
                            Next

                            Dim ccConstr As GRBConstr = Nothing
                            Dim ccIndConstr As GRBGenConstr = Nothing
                            If Constraint.MinValueSet Then
                                ' Adding constraint for custom constraint for Minimum Value
                                If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("ccMinVar_" + Constraint.ID.ToString)) IsNot Nothing Then
                                    Dim ccMinVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "ccMinVar_" + Constraint.ID.ToString)
                                    gVariables.Add("ccMinVar_" + Constraint.ID.ToString, ccMinVar)
                                    gIndVariables.Add("ccMinVar_" + Constraint.ID.ToString, ccMinVar)
                                    constraintInfeasibilityIndicators.AddTerm(1, ccMinVar)

                                    ccIndConstr = gModel.AddGenConstrIndicator(ccMinVar, 1, constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName + "_Min")
                                    IndicatorConstraints.Add(cName + "_Min", ccIndConstr)

                                    numOfConstraintsForInfeasibilities += 1

                                Else
                                    ccConstr = gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName + "_Min")
                                    gConstraints.Add(cName + "_Min", ccConstr)
                                End If
                            End If
                            If Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("ccMaxVar_" + Constraint.ID.ToString)) IsNot Nothing Then
                                    Dim ccMaxVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "ccMaxVar_" + Constraint.ID.ToString)
                                    gVariables.Add("ccMaxVar_" + Constraint.ID.ToString, ccMaxVar)
                                    gIndVariables.Add("ccMaxVar_" + Constraint.ID.ToString, ccMaxVar)
                                    constraintInfeasibilityIndicators.AddTerm(1, ccMaxVar)

                                    ccIndConstr = gModel.AddGenConstrIndicator(ccMaxVar, 1, constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName + "_Max")
                                    IndicatorConstraints.Add(cName + "_Max", ccIndConstr)
                                    numOfConstraintsForInfeasibilities += 1
                                Else
                                    ccConstr = gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName + "_Max")
                                    gConstraints.Add(cName + "_Max", ccConstr)

                                End If
                            End If
                            'i += 1
                        End If
                    Next
                End If

                If Settings.FundingPools Then
                    For Each FPools As RAFundingPools In CurrentScenario.ResourcePools.Values
                        If FPools.Pools.Count > 0 AndAlso FPools.Pools.Values.FirstOrDefault(Function(p) p.Enabled) IsNot Nothing Then
                            ' sum of the row should be equal to the value of alternative resource
                            For Each alt As RAAlternative In Alternatives
                                Dim constraintFPRow As GRBLinExpr = 0.0

                                'Dim fprowName As String = "FProw_" + alt.ID
                                Dim fprowName As String = "RP_" + FPools.ResourceID.ToString + "_FProw_" + alt.ID
                                For Each FP As RAFundingPool In FPools.Pools.Values
                                    If FP.Enabled Then
                                        'Model.loadPoint("FProw_" + alt.ID, "FP" + FP.ID.ToString + "_" + alt.ID, 1)

                                        'constraintFPRow.AddTerm(1, gVariables("FP" + FP.ID.ToString + "_" + alt.ID))
                                        constraintFPRow.AddTerm(1, gVariables("RP_" + FPools.ResourceID.ToString + "_FP" + FP.ID.ToString + "_" + alt.ID))
                                    End If
                                Next
                                'constraintFPRow.AddTerm(-alt.Cost, gVariables(alt.ID))
                                Dim resource As RAResource = If(CurrentScenario.TimePeriods.Resources.ContainsKey(FPools.ResourceID), CurrentScenario.TimePeriods.Resources(FPools.ResourceID), Nothing)
                                Dim cc As RAConstraint = If(resource IsNot Nothing, CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID), Nothing)

                                Dim rValue As Double
                                If FPools.ResourceID.Equals(RA_Cost_GUID) Then
                                    rValue = alt.Cost
                                Else
                                    rValue = If(cc IsNot Nothing, cc.GetAlternativeValue(alt.ID), 0)
                                End If
                                If rValue <> UNDEFINED_INTEGER_VALUE Then
                                    constraintFPRow.AddTerm(-rValue, gVariables(alt.ID))

                                    Dim fpRowConstr As GRBConstr = gModel.AddConstr(constraintFPRow, GRB.EQUAL, 0, fprowName)
                                    gConstraints.Add(fprowName, fpRowConstr)
                                End If
                            Next

                            ' funding pools limits
                            Dim fpPrty As Integer = FPools.Pools.Count
                            For Each FP As RAFundingPool In FPools.Pools.Values
                                If FP.Enabled Then
                                    'Dim fprowName As String = "FProw_" + FP.ID.ToString
                                    Dim fprowName As String = "RPool_" + FPools.ResourceID.ToString + "_FProw_" + FP.ID.ToString

                                    Dim constraintFPRow As GRBLinExpr = 0.0
                                    Dim constraintFPRowObj As GRBLinExpr = 0.0
                                    For Each alt As RAAlternative In Alternatives
                                        'constraintFPRow.AddTerm(1, gVariables("FP" + FP.ID.ToString + "_" + alt.ID))
                                        'constraintFPRowObj.AddTerm(1, gVariables("FP" + FP.ID.ToString + "_" + alt.ID))
                                        constraintFPRow.AddTerm(1, gVariables("RP_" + FPools.ResourceID.ToString + "_FP" + FP.ID.ToString + "_" + alt.ID))
                                        constraintFPRowObj.AddTerm(1, gVariables("RP_" + FPools.ResourceID.ToString + "_FP" + FP.ID.ToString + "_" + alt.ID))
                                    Next

                                    If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("fpPoolLimitVar_" + FP.ID.ToString)) IsNot Nothing Then
                                        Dim fpPoolLimitVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "fpPoolLimitVar_" + FP.ID.ToString)
                                        gVariables.Add("fpPoolLimitVar_" + FP.ID.ToString, fpPoolLimitVar)
                                        gIndVariables.Add("fpPoolLimitVar_" + FP.ID.ToString, fpPoolLimitVar)
                                        constraintInfeasibilityIndicators.AddTerm(1, fpPoolLimitVar)

                                        Dim fpPoolLimitIndConstr As GRBGenConstr = Nothing
                                        fpPoolLimitIndConstr = gModel.AddGenConstrIndicator(fpPoolLimitVar, 1, constraintFPRow, GRB.LESS_EQUAL, FP.PoolLimit, fprowName)
                                        IndicatorConstraints.Add(fprowName, fpPoolLimitIndConstr)
                                        numOfConstraintsForInfeasibilities += 1
                                    Else
                                        Dim fpConstr As GRBConstr = gModel.AddConstr(constraintFPRow, GRB.LESS_EQUAL, FP.PoolLimit, fprowName)
                                        gConstraints.Add(fprowName, fpConstr)
                                    End If

                                    gModel.SetObjectiveN(constraintFPRowObj, objIndex, fpPrty, 1, 0, 0, "fpPrty_" + fpPrty.ToString)
                                    objIndex += 1
                                    fpPrty -= 1
                                End If
                            Next
                        End If
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim TPRowVariantCount As Integer = 0
                    Dim tpResList As New Dictionary(Of String, GRBLinExpr)
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim cTPRowVariant As GRBLinExpr = 0.0
                        cTPRowVariant.AddTerm(-1, gVariables(alt.ID))
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            v += 1
                            cTPRowVariant.AddTerm(1, gVariables("TPVar_" + alt.ID.ToString + "_" + j.ToString))
                            i += 1
                        Next
                        Dim tpRowVariant As GRBConstr = gModel.AddConstr(cTPRowVariant, GRB.EQUAL, 0, "TPRowVariant_" + alt.ID)
                        gConstraints.Add("TPRowVariant_" + alt.ID, tpRowVariant)
                        TPRowVariantCount += 1
                    Next

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                For Each alt As RAAlternative In Alternatives
                                    Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                    If t >= apd.GetMinPeriod AndAlso t <= apd.GetMaxPeriod + apd.Duration - 1 Then
                                        If Not FlexibleTimePeriodsResources Then
                                            For k As Integer = Math.Max(apd.GetMinPeriod, t - apd.Duration + 1) To Math.Min(t, apd.GetMaxPeriod)
                                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - k, alt.ID, resource.ID)
                                                If c <> UNDEFINED_INTEGER_VALUE Then
                                                    Dim cTPRes As GRBLinExpr = 0.0
                                                    If Not tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                                        tpResList.Add("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, cTPRes)
                                                    Else
                                                        cTPRes = tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString)
                                                    End If
                                                    cTPRes.AddTerm(c, gVariables("TPVar_" + alt.ID + "_" + k.ToString))
                                                End If
                                            Next
                                        Else
                                            Dim cTPRes As GRBLinExpr = 0.0
                                            If Not tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                                tpResList.Add("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, cTPRes)
                                            Else
                                                cTPRes = tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString)
                                            End If
                                            cTPRes.AddTerm(1, gVariables("TPVarFlex_" + alt.ID.ToString + "_" + resource.ID.ToString + "_" + t.ToString))
                                        End If


                                        'For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        '    If t - j < apd.Duration Then
                                        '        Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - j, alt.ID, resource.ID)
                                        '        'Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t, alt.ID, resource.ID)
                                        '        If c <> UNDEFINED_INTEGER_VALUE Then
                                        '            Dim cTPRes As GRBLinExpr = 0.0
                                        '            If Not tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        '                tpResList.Add("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, cTPRes)
                                        '            Else
                                        '                cTPRes = tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString)
                                        '            End If
                                        '            cTPRes.AddTerm(c, gVariables("TPVar_" + alt.ID + "_" + j.ToString))                                                'model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPVar_" + alt.ID + "_" + j.ToString, c)
                                        '        End If
                                        '    End If
                                        'Next
                                    End If
                                Next
                            End If

                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        If Settings.ResourcesMax Then
                                            If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString)) IsNot Nothing Then
                                                Dim tpResourceMaxVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString)
                                                gVariables.Add("tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMaxVar)
                                                gIndVariables.Add("tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMaxVar)
                                                constraintInfeasibilityIndicators.AddTerm(1, tpResourceMaxVar)

                                                Dim tpResourceMaxIndConstr As GRBGenConstr = gModel.AddGenConstrIndicator(tpResourceMaxVar, 1, tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                                IndicatorConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max", tpResourceMaxIndConstr)

                                                numOfConstraintsForInfeasibilities += 1
                                            Else
                                                Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                                gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max", tpresMainConstr)
                                            End If
                                        End If
                                        If Settings.ResourcesMin Then
                                            If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString)) IsNot Nothing Then
                                                Dim tpResourceMinVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString)
                                                gVariables.Add("tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMinVar)
                                                gIndVariables.Add("tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMinVar)
                                                constraintInfeasibilityIndicators.AddTerm(1, tpResourceMinVar)

                                                Dim tpResourceMinIndConstr As GRBGenConstr = gModel.AddGenConstrIndicator(tpResourceMinVar, 1, tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                                IndicatorConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min", tpResourceMinIndConstr)

                                                numOfConstraintsForInfeasibilities += 1
                                            Else
                                                Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                                gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min", tpresMainConstr)
                                            End If
                                        End If
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString)) IsNot Nothing Then
                                            Dim tpResourceMaxVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString)
                                            gVariables.Add("tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMaxVar)
                                            gIndVariables.Add("tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMaxVar)
                                            constraintInfeasibilityIndicators.AddTerm(1, tpResourceMaxVar)

                                            Dim tpResourceMaxIndConstr As GRBGenConstr = gModel.AddGenConstrIndicator(tpResourceMaxVar, 1, tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                            IndicatorConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max", tpResourceMaxIndConstr)
                                            numOfConstraintsForInfeasibilities += 1
                                        Else
                                            Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                            gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max", tpresMainConstr)
                                        End If
                                    End If
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString)) IsNot Nothing Then
                                            Dim tpResourceMinVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString)
                                            gVariables.Add("tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMinVar)
                                            gIndVariables.Add("tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString, tpResourceMinVar)
                                            constraintInfeasibilityIndicators.AddTerm(1, tpResourceMinVar)

                                            Dim tpResourceMinIndConstr As GRBGenConstr = gModel.AddGenConstrIndicator(tpResourceMinVar, 1, tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                            IndicatorConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min", tpResourceMinIndConstr)
                                            numOfConstraintsForInfeasibilities += 1
                                        Else
                                            Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                            gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min", tpresMainConstr)
                                        End If
                                    End If
                                End If
                            End If
                        Next
                        t += 1
                    Next

                    If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim k As Integer = 1
                        For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                            If dependency.Enabled And dependency.Value <> RADependencyType.dtConcurrent Then
                                Dim cDep As GRBLinExpr = 0.0
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    cDep.AddTerm(j + 1, gVariables("TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString))
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    cDep.AddTerm(-(j + 1), gVariables("TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString))
                                Next

                                Dim tpDepConstr As GRBConstr = Nothing
                                Dim tpIndDepConstr As GRBGenConstr = Nothing

                                Dim dtpVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "dTPVar_" + k.ToString)
                                constraintInfeasibilityIndicators.AddTerm(1, dtpVar)
                                gVariables.Add("dTPVar_" + k.ToString, dtpVar)
                                gIndVariables.Add("dTPVar_" + k.ToString, dtpVar)


                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        'cDep.AddTerm(-(apd2.GetMaxPeriod + apd2.Duration), gVariables(dependency.FirstAlternativeID))

                                        If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("dTPVar_" + k.ToString)) IsNot Nothing Then
                                            'tpIndDepConstr = gModel.AddGenConstrIndicator(dtpVar, 1, cDep, GRB.GREATER_EQUAL, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration), "TPDependency_" + k.ToString)
                                            tpIndDepConstr = gModel.AddGenConstrIndicator(gVariables(dependency.FirstAlternativeID), 1, cDep, GRB.GREATER_EQUAL, apd2.Duration, "TPDependency_" + k.ToString)
                                        Else
                                            'tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration), "TPDependency_" + k.ToString)
                                            'tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration, "TPDependency_" + k.ToString)

                                            ' formulation using indicator constraints
                                            tpIndDepConstr = gModel.AddGenConstrIndicator(gVariables(dependency.FirstAlternativeID), 1, cDep, GRB.GREATER_EQUAL, apd2.Duration, "TPDependency_" + k.ToString)

                                            ' formulation using bigM approach:
                                            'Dim bigM As Integer = 100
                                            'cDep.AddTerm(-bigM, gVariables(dependency.FirstAlternativeID))
                                            'tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - bigM, "TPDependency_" + k.ToString)
                                        End If
                                    Case RADependencyType.dtConcurrent
                                        cDep.AddTerm(-apd2.GetMaxPeriod, gVariables(dependency.FirstAlternativeID))
                                        If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("dTPVar_" + k.ToString)) IsNot Nothing Then
                                            tpIndDepConstr = gModel.AddGenConstrIndicator(dtpVar, 1, cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                        Else
                                            tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                        End If
                                    Case RADependencyType.dtLag
                                        cDep.AddTerm(-apd2.GetMaxPeriod, gVariables(dependency.FirstAlternativeID))
                                        If SolveForInfeasibilityAnalysis AndAlso InfeasibilityConstraintsList.FirstOrDefault(Function(x) x.ID = ("dTPVar_" + k.ToString)) IsNot Nothing Then
                                            tpIndDepConstr = gModel.AddGenConstrIndicator(dtpVar, 1, cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                        Else
                                            tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                        End If
                                End Select
                                If tpDepConstr IsNot Nothing Then gConstraints.Add("TPDependency_" + k.ToString, tpDepConstr)
                                If tpDepConstr IsNot Nothing Then
                                    IndicatorConstraints.Add("TPDependency_" + k.ToString, tpIndDepConstr)
                                    numOfConstraintsForInfeasibilities += 1
                                End If

                            End If
                            k += 1
                        Next
                    End If
                End If

                If SolveForInfeasibilityAnalysis Then
                    'Dim IndicatorConstr As GRBConstr = gModel.AddConstr(constraintInfeasibilityIndicators, GRB.EQUAL, numOfConstraintsForInfeasibilities, "InfeasibilityIndicators")
                    Dim IndicatorConstr As GRBConstr = gModel.AddConstr(constraintInfeasibilityIndicators, GRB.EQUAL, mInfeasibilityConstraintsCount, "InfeasibilityIndicators")
                    If IndicatorConstr IsNot Nothing Then gConstraints.Add("InfeasibilityIndicators", IndicatorConstr)

                    Dim k As Integer = 1
                    For Each cList As List(Of ConstraintInfo) In mExcludeConstraintsSets
                        If cList.Count > 0 Then
                            Dim listExpr As GRBLinExpr = 0.0
                            For Each c As ConstraintInfo In cList
                                listExpr.AddTerm(1, gIndVariables(c.ID))
                            Next
                            Dim listConstr As GRBConstr = gModel.AddConstr(listExpr, GRB.GREATER_EQUAL, 1, "ExcludeList_" + k.ToString)
                            k += 1
                        End If
                    Next
                End If

                gModel.SetObjectiveN(objBenefits, 0, maxPriority, 1, 0, 0, "benefits")

                gModel.Update()

                gModel.Parameters.TimeLimit = 60
                gModel.Parameters.MIPGap = 0.001

                gModel.Parameters.PoolSolutions = mNumberOfSolutionsToFind
                gModel.Parameters.PoolSearchMode = mPoolSearchMode

            Catch ex As Exception
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
            End Try

            Return gModel
        End Function

#Region "Debug functions"
        Private Sub TestEfficientFrontier()
            Dim interval As New EfficientFrontierInterval
            interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps
            interval.DeltaValue = 20
            interval.MinValue = CurrentScenario.GetMinCost
            interval.MaxValue = CurrentScenario.GetMaxCost

            Dim settings As New EfficientFrontierSettings
            settings.ConstraintType = EfficientFrontierConstraintType.BudgetLimit
            settings.Intervals.Add(interval)

            Solve_Gurobi_EfficientFrontier(settings)
        End Sub
        Private Sub PrintEfficientFrontierResults(Settings As EfficientFrontierSettings)
            Debug.Print(vbNewLine)
            Dim i As Integer = 1
            For Each interval As EfficientFrontierInterval In Settings.Intervals
                Debug.Print("Interval " + i.ToString + ":")
                Debug.Print("  Delta type = " + interval.DeltaType.ToString)
                Debug.Print("  Delta value = " + interval.DeltaValue.ToString)
                Debug.Print("  Min value " + interval.MinValue.ToString)
                Debug.Print("  Max value " + interval.MaxValue.ToString)
                Debug.Print("  Interval data: ")
                For Each result As EfficientFrontierResults In interval.Results
                    Debug.Print("    Funded cost: " + result.FundedCost.ToString)
                    Debug.Print("    Funded benefits: " + result.FundedBenefits.ToString)
                    Debug.Print("    Funded benefits original: " + result.FundedBenefitsOriginal.ToString)
                    Debug.Print("    SolverState" + result.SolverState.ToString)
                    Debug.Print("    Value: " + result.Value.ToString)
                    If True Then
                        Debug.Print("    Aleternatives data: ")
                        For Each kvp As KeyValuePair(Of String, Double) In result.AlternativesData
                            Debug.Print("      " + kvp.Key + "; funded  = " + kvp.Value.ToString)
                        Next
                    End If
                Next
                i += 1
            Next

        End Sub
#End Region
    End Class

#Region "Gurobi JSON"

    ' D3894 ===
    <Serializable> Public Class clsGurobiLicense
        Public id As String = ""
        Public cloudKey As String = ""
        Public credit As Double = 0
        Public expiration As Date
        Public ratePlan As String = ""
        Public ratePlanName As String = ""
        Public licenseTypes As String() = {}
        Public invalid As String = ""
        Public invalidState As String = ""
        Public userPassword As String = ""
        Public adminPassword As String = ""
    End Class

    <Serializable> Public Class clsGurobiMachine
        Public id As String = ""
        Public state As String = ""
        Public DNSName As String = ""
        Public machineType As String = ""
        Public createdAt As String = ""
        Public region As String = ""
        Public zone As String = ""
        Public licenseId As String = ""
        Public cloudKey As String = ""
        Public adminPassword As String = ""
        Public userPassword As String = ""
        Public licenseType As String = ""
        Public idleShutdown As Integer = 0
        Public jobLimit As Integer = 0
        Public nbDistributedWorkers As Integer = 0
        Public GRBVersion As String = ""
        Public masterServer As String = ""
        Public poolName As String = ""
        Public poolId As String = ""
        'Public error As String()
        Public status As clsGurobiMachineStatus = Nothing
    End Class

    <Serializable> Public Class clsGurobiMachineStatus
        Public GRBVersion As String = ""
        Public idleTime As Integer = 0
        Public running As Integer = 0
        Public jobLimit As Integer = 0
        Public services As String() = {}
    End Class
    ' D3894 ==

#End Region

End Namespace
