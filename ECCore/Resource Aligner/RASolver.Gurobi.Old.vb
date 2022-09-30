Option Strict On

Imports ECCore
Imports Canvas
Imports Gurobi
Imports ECCore.MiscFuncs

Namespace Canvas

    Partial Public Class RASolver
        Private Function Solve_Gurobi_FromMPS(Optional OutputPath As String = "", Optional SolverParams As String() = Nothing) As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Try
                gEnv = New GRBEnv(OutputPath, OPT_GUROBI_ACCESS_ID, OPT_GUROBI_SECRETE_KEY, OPT_GUROBI_POOL_ID)   ' D3870 + D3894

                Dim s As String = "C:\gurobi.mps"
                gModel = New Gurobi.GRBModel(gEnv, s)

                gModel.Update()

                If OutputPath <> "" Then
                    OutputPath = OutputPath.Replace("\", "\\")
                    gModel.Write(OutputPath)
                End If
                gModel.Optimize()

                Dim status As Integer = gModel.Get(GRB.IntAttr.Status)

                Select Case status
                    Case GRB.Status.OPTIMAL
                        SolverState = raSolverState.raSolved
                    Case GRB.Status.INFEASIBLE, GRB.Status.INF_OR_UNBD
                        SolverState = raSolverState.raInfeasible
                End Select
                LastError = ""
                LastErrorReal = ""  ' D3880
            Catch ex As Exception
                ''Debug.Print("Error occured. Reason Code: " + Model.getXAExceptionCode().ToString + " Message: " + Model.getXAExceptionMessage())
                SolverState = raSolverState.raError
                ' D3880 ===
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
                If gEnv IsNot Nothing Then gEnv.Dispose() Else LastError = "Solver %%solver%% is not available. Contact EC Technical Support"
                ' D3880 ==
            Finally
                If gModel IsNot Nothing And Not KeepGurobiAlive Then gModel.Dispose()
                If gEnv IsNot Nothing And Not KeepGurobiAlive Then gEnv.Dispose()
            End Try

            Return True
        End Function

        Private Function Solve_GurobiOld(Optional OutputPath As String = "", Optional SolverParams As String() = Nothing) As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Try
                gEnv = New GRBEnv(OutputPath, OPT_GUROBI_ACCESS_ID, OPT_GUROBI_SECRETE_KEY, OPT_GUROBI_POOL_ID)   ' D3870 + D3894
                'gModel = New Gurobi.GRBModel(gEnv)
                'If SolverParams Is Nothing Then
                '    gEnv = New GRBEnv("gurobi.log", "a9ce3faa-185a-4419-a7ec-d3f4a19df18e", "fS2ENZd0TmeDbicgLnW9Ng", "134005-default")
                '    'gEnv = New Gurobi.GRBEnv()
                'Else
                '    'gEnv = New GRBEnv("gurobi.log", SolverParams(0), -1, SolverParams(1), 0, -1)
                '    gEnv = New GRBEnv("gurobi.log", "abd25fcf-6fbf-4006-ba18-605685999cd0", "CYuusDjwS+K+DHkkkSRf5Q", "")
                'End If
                'gEnv = New Gurobi.GRBEnv()
                'gEnv = New GRBEnv("gurobi.log", "ec2-54-210-210-235.compute-1.amazonaws.com", -1, "33f78777", 0, -1)

                gModel = New Gurobi.GRBModel(gEnv)

                Dim maxCost As Double = 0
                Dim maxBenefit As Double = 0
                Dim sumCost As Double = 0
                Dim sumBenefit As Double = 0
                Dim eps As Double = 0.0001
                For Each alt As RAAlternative In Alternatives
                    If alt.Cost > maxCost Then maxCost = alt.Cost
                    If alt.BenefitOriginal > maxBenefit Then maxBenefit = alt.BenefitOriginal
                    sumCost += alt.Cost
                    sumBenefit += alt.BenefitOriginal
                Next

                Dim adjustValue As Double = -eps * maxBenefit ' because approach with subtracting -0.00001 * maxCost from benefits to find optimal cost is not working
                adjustValue = 0

                ' objective coefficient for decision variables
                'For Each alt As RAAlternative In Alternatives
                '    Model.setColumnObjective(alt.ID, adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)))
                'Next

                Dim variables As New Dictionary(Of String, GRBVar)

                'Make decision variable alt1,alt2, .... binary
                Dim altVar As GRBVar
                For Each alt As RAAlternative In Alternatives
                    If alt.IsPartial Then
                        'Model.setColumnSemiContinuous(alt.ID, alt.MinPercent, 1)
                        altVar = gModel.AddVar(alt.MinPercent, 1, 0, GRB.SEMICONT, alt.ID)
                    Else
                        'Model.setColumnBinary(alt.ID)
                        altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, alt.ID)
                    End If
                    variables.Add(alt.ID, altVar)
                Next

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                Dim varName As String = "FP" + FP.ID.ToString + "_" + alt.ID
                                'Model.setColumnObjective("FP" + FP.ID.ToString + "_" + alt.ID, 0)
                                Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                Dim fpVar As GRBVar = gModel.AddVar(0, cellCost, 0, GRB.CONTINUOUS, varName)

                                variables.Add(varName, fpVar)
                                'Model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    'If Settings.TimePeriods Then
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            For k As Integer = 0 To apd.Duration - 1
                                altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString)
                                'model.setColumnObjective("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, 0)
                                'model.setColumnBinary("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString)
                                variables.Add("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, altVar)

                                If Not variables.ContainsKey("TPVar_" + alt.ID.ToString + j.ToString) Then
                                    If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                        altVar = gModel.AddVar(0, 1, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v), GRB.BINARY, "TPVar_" + alt.ID.ToString + j.ToString)
                                        'model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                                    Else
                                        altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "TPVar_" + alt.ID.ToString + j.ToString)
                                        'model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, 0)
                                        'model.setColumnBinary("TPVar_" + alt.ID.ToString + j.ToString)
                                    End If
                                    variables.Add("TPVar_" + alt.ID.ToString + j.ToString, altVar)
                                End If
                                v += 1
                            Next
                            i += 1
                        Next
                    Next

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            If resource.Enabled Then 'A1130
                                'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)  ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)  ' D3918

                                ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                                'If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                '    model.setColumnObjective("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString, 0)
                                '    model.setColumnBinary("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString)
                                'End If
                                'End If
                            End If
                        Next
                        t += 1
                    Next
                End If

                gModel.Update()

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                'Model.setRowMax("Cost", BudgetLimit)
                Dim objBenefits As GRBLinExpr = 0.0
                Dim constraintCost As GRBLinExpr = 0.0
                For Each alt As RAAlternative In Alternatives
                    objBenefits.AddTerm(adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)), variables(alt.ID))
                    constraintCost.AddTerm(alt.Cost, variables(alt.ID))
                    ''Model.setColumnObjective(alt.ID, adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)))
                    'Model.loadToCurrentRow(alt.ID, alt.Cost)
                Next
                gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, BudgetLimit, "cost")
                gModel.SetObjective(objBenefits, GRB.MAXIMIZE)


                'gModel.NumObj = 2
                'gModel.Parameters.ObjNumber = 1
                'gModel.ObjNPriority = 0
                'gModel.ObjNWeight = -1
                'Dim cCost As GRBLinExpr = 0.0
                'For Each alt As RAAlternative In Alternatives
                '    cCost.AddTerm(alt.Cost, variables(alt.ID))
                'Next
                'gModel.SetObjective(cCost, GRB.MAXIMIZE)


                'gModel.Parameters.ObjNumber = 0
                'gModel.ObjNPriority = 1


                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0
                For Each alt As RAAlternative In Alternatives
                    If alt.Must Then mustCount += 1
                    If alt.MustNot Then mustnotCount += 1
                Next

                Dim constraintMusts As GRBLinExpr = 0.0
                If Settings.Musts And (mustCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        constraintMusts.AddTerm(CDbl(If(alt.Must, 1, 0)), variables(alt.ID))
                        'Model.loadPoint("Must", alt.ID, CDbl(If(alt.Must, 1, 0)))
                    Next
                    gModel.AddConstr(constraintMusts, GRB.EQUAL, mustCount, "musts")
                    'Model.setRowFix("Must", mustCount)
                End If

                Dim constraintMustNots As GRBLinExpr = 0.0
                If Settings.MustNots And (mustnotCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        constraintMustNots.AddTerm(CDbl(If(alt.MustNot, 1, 0)), variables(alt.ID))
                        'Model.loadPoint("MustNot", alt.ID, CDbl(If(alt.MustNot, 1, 0)))
                    Next
                    gModel.AddConstr(constraintMustNots, GRB.EQUAL, 0, "mustnots")
                    'Model.setRowFix("MustNot", 0)
                End If

                If Settings.Groups Then
                    Dim i As Integer = 1
                    For Each group As RAGroup In Groups.Groups.Values
                        If group.Enabled Then
                            Dim gName As String = "Group_" + i.ToString
                            Dim constraintGroup As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                constraintGroup.AddTerm(CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)), variables(alt.ID))
                                'Model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
                            Next

                            Select Case group.Condition
                                Case RAGroupCondition.gcLessOrEqualsOne
                                    ' none of one alterantive from the group if funded
                                    'Model.setRowMax(gName, 1)
                                    gModel.AddConstr(constraintGroup, GRB.LESS_EQUAL, 1, gName)
                                Case RAGroupCondition.gcEqualsOne
                                    ' exactly one alterantive from the group is funded
                                    'Model.setRowFix(gName, 1)
                                    gModel.AddConstr(constraintGroup, GRB.EQUAL, 1, gName)
                                Case RAGroupCondition.gcGreaterOrEqualsOne
                                    ' at least one alternative from the group is funded
                                    'Model.setRowMin(gName, 1)
                                    gModel.AddConstr(constraintGroup, GRB.GREATER_EQUAL, 1, gName)
                            End Select
                            i += 1
                        End If
                    Next
                End If

                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In Dependencies.Dependencies
                        Dim constraintDependency As GRBLinExpr = 0.0
                        For Each alt As RAAlternative In Alternatives
                            Select Case Dependency.Value
                                Case RADependencyType.dtDependsOn
                                    ' first is not funded unless second is funded
                                    constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)), variables(alt.ID))
                                Case RADependencyType.dtMutuallyDependent
                                    constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)), variables(alt.ID))
                                Case RADependencyType.dtMutuallyExclusive
                                    constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) + CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)), variables(alt.ID))
                            End Select
                        Next
                        Dim dName As String
                        Select Case Dependency.Value
                            Case RADependencyType.dtDependsOn
                                dName = "Dependency" + i.ToString + "_DependsOn"
                                gModel.AddConstr(constraintDependency, GRB.GREATER_EQUAL, 0, dName)
                            Case RADependencyType.dtMutuallyDependent
                                dName = "Dependency" + i.ToString + "_MutuallyDependent"
                                gModel.AddConstr(constraintDependency, GRB.EQUAL, 0, dName)
                            Case RADependencyType.dtMutuallyExclusive
                                dName = "Dependency" + i.ToString + "_MutuallyExclusive"
                                gModel.AddConstr(constraintDependency, GRB.LESS_EQUAL, 1, dName)
                        End Select

                        i += 1
                    Next
                End If

                If Settings.CustomConstraints Then
                    Dim i As Integer = 1
                    For Each Constraint As RAConstraint In Constraints.Constraints.Values
                        If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
                            ' for each custom constraint we create a vector that holds custom constrain values in the field Cost
                            Dim cName As String = "CustomConstraint_" + i.ToString
                            Dim constraintCC As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                                If value <> UNDEFINED_INTEGER_VALUE Then
                                    'Model.loadPoint(cName, alt.ID, value)
                                    constraintCC.AddTerm(value, variables(alt.ID))
                                End If
                            Next

                            If Constraint.MinValueSet And Not Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Minimum Value
                                'Model.setRowMin(cName, Constraint.MinValue)
                                gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName)
                            End If
                            If Constraint.MaxValueSet And Not Constraint.MinValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                'Model.setRowMax(cName, Constraint.MaxValue)
                                gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName)
                            End If
                            If Constraint.MinValueSet And Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                'Model.setRowMinMax(cName, Constraint.MinValue, Constraint.MaxValue)
                                gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName)
                                gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName)
                            End If

                            i += 1
                        End If
                    Next
                End If

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        Dim constraintFPRow As GRBLinExpr = 0.0
                        Dim fprowName As String = "FProw_" + alt.ID
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                'Model.loadPoint("FProw_" + alt.ID, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                                constraintFPRow.AddTerm(1, variables("FP" + FP.ID.ToString + "_" + alt.ID))
                            End If
                            'model.setRowFix("FProw_" + alt.ID, 0)
                            'model.setRowMax("FProw_" + alt.ID, alt.Cost)
                        Next
                        'Model.loadPoint("FProw_" + alt.ID, alt.ID, -alt.Cost)
                        'Model.setRowFix("FProw_" + alt.ID, 0)
                        constraintFPRow.AddTerm(-alt.Cost, variables(alt.ID))
                        gModel.AddConstr(constraintFPRow, GRB.EQUAL, 0, fprowName)
                    Next

                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            Dim fprowName As String = "FProw_" + FP.ID.ToString
                            Dim constraintFPRow As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                'Model.loadPoint("FProw_" + FP.ID.ToString, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                                constraintFPRow.AddTerm(1, variables("FP" + FP.ID.ToString + "_" + alt.ID))
                            Next
                            'Model.setRowMax("FProw_" + FP.ID.ToString, FP.PoolLimit)
                            gModel.AddConstr(constraintFPRow, GRB.LESS_EQUAL, FP.PoolLimit, fprowName)
                        End If
                    Next
                End If

                If Settings.TimePeriods Then
                    Dim TPRowVariantCount As Integer = 0
                    Dim TPRowCount As Integer = 0
                    Dim tpResList As New Dictionary(Of String, GRBLinExpr)
                    Dim tpRowList As New Dictionary(Of String, GRBLinExpr)
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim cTPRowVariant As GRBLinExpr = 0.0

                        cTPRowVariant.AddTerm(-1, variables(alt.ID))
                        'model.loadPoint("TPRowVariant_" + alt.ID.ToString, alt.ID.ToString, -1)

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                'model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                                'gModel.ChgCoeffs()
                                'variables("TPVar_" + alt.ID.ToString + j.ToString).Set(, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                            End If
                            v += 1

                            'model.loadPoint("TPRowVariant_" + alt.ID.ToString, "TPVar_" + alt.ID.ToString + j.ToString, 1)
                            cTPRowVariant.AddTerm(1, variables("TPVar_" + alt.ID.ToString + j.ToString))

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        Dim cTPRow As GRBLinExpr = 0.0
                                        Dim TPRowExists As Boolean = False
                                        If tpRowList.ContainsKey("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString) Then
                                            TPRowExists = True
                                            cTPRow = tpRowList("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString)
                                        End If

                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                                            End If
                                            If Not TPRowExists Then
                                                cTPRow.AddTerm(-value, variables("TPVar_" + alt.ID.ToString + j.ToString))
                                            End If
                                            cTPRow.AddTerm(c, variables("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString))
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            ' D3918 ===
                                            'If resource.ID.Equals(RA_Cost_GUID) Then

                                            'model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                            Dim cTPRes As GRBLinExpr = 0.0
                                            If Not tpResList.ContainsKey("TPRes" + (j + k).ToString + "_" + resource.ID.ToString) Then
                                                tpResList.Add("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, cTPRes)
                                            Else
                                                cTPRes = tpResList("TPRes" + (j + k).ToString + "_" + resource.ID.ToString)
                                            End If
                                            cTPRes.AddTerm(c, variables("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString))


                                            'model.loadPoint("TPRes1" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                            'model.loadPoint("TPRes2" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            ' D3918 ==
                                            ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                                            'For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                                            '    'If resource.ID.Equals(RA_Cost_GUID) AndAlso period.ID = j + k Then  ' D3918
                                            '    If period.ID = j + k Then  ' D3918
                                            '        Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                            '        Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                            '        If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                            '            ' D3918 ===
                                            '            model.loadPoint("TPRes1" + (j + k).ToString + "_" + resource.ID.ToString, "TPResY_" + period.ID.ToString + "_" + resource.ID.ToString, -minValue)
                                            '            model.loadPoint("TPRes2" + (j + k).ToString + "_" + resource.ID.ToString, "TPResY_" + period.ID.ToString + "_" + resource.ID.ToString, -maxValue)

                                            '            model.setRowMin("TPRes1" + (j + k).ToString + "_" + resource.ID.ToString, 0)
                                            '            model.setRowMax("TPRes2" + (j + k).ToString + "_" + resource.ID.ToString, 0)
                                            '            ' D3918 ==
                                            '        End If
                                            '    End If
                                            'Next
                                            'End If

                                            'model.setRowFix("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            If Not tpRowList.ContainsKey("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString) Then
                                                tpRowList.Add("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, cTPRow)
                                                TPRowCount += 1
                                            End If
                                            'gModel.AddConstr(cTPRow, GRB.EQUAL, 0, "TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString)
                                        End If
                                    Next
                                End If

                                If k = 0 Then
                                    'model.loadPoint("TPR_ME_" + alt.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, 1)
                                End If
                            Next
                            '--- model.loadPoint("TPRow" + i.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -alt.Cost)
                            '---model.setRowFix("TPRow" + i.ToString, 0)
                            i += 1
                        Next
                        'model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        gModel.AddConstr(cTPRowVariant, GRB.EQUAL, 0, "TPRowVariant_" + alt.ID)
                        TPRowVariantCount += 1
                    Next

                    For Each cRow As KeyValuePair(Of String, GRBLinExpr) In tpRowList
                        gModel.AddConstr(cRow.Value, GRB.EQUAL, 0, cRow.Key)
                    Next

                    'Debug.Print("TPRowVariantCount = " + TPRowVariantCount.ToString)
                    'Debug.Print("TPRowCount = " + TPRowCount.ToString)

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes" + t.ToString + "_" + resource.ID.ToString) Then
                                        'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                        gModel.AddConstr(tpResList("TPRes" + t.ToString + "_" + resource.ID.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                        gModel.AddConstr(tpResList("TPRes" + t.ToString + "_" + resource.ID.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes" + t.ToString + "_" + resource.ID.ToString) Then
                                        'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                        gModel.AddConstr(tpResList("TPRes" + t.ToString + "_" + resource.ID.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString)
                                    End If
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes" + t.ToString + "_" + resource.ID.ToString) Then
                                        'model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
                                        gModel.AddConstr(tpResList("TPRes" + t.ToString + "_" + resource.ID.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString)
                                    End If
                                End If
                                'End If
                            End If
                        Next
                        t += 1
                    Next

                    If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim k As Integer = 1
                        For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                            If dependency.Enabled Then
                                Dim cDep As GRBLinExpr = 0.0
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    'model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + j.ToString, j + 1)
                                    cDep.AddTerm(j + 1, variables("TPVar_" + dependency.FirstAlternativeID + j.ToString))
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    'model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + j.ToString, -(j + 1))
                                    cDep.AddTerm(-(j + 1), variables("TPVar_" + dependency.SecondAlternativeID + j.ToString))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        'model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        'model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
                                        cDep.AddTerm(-(apd2.GetMaxPeriod + apd2.Duration), variables(dependency.FirstAlternativeID))
                                        gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration), "TPDependency_" + k.ToString)
                                    Case RADependencyType.dtConcurrent
                                        ' UNCOMMENT TO FORCE CONCURRENT:
                                        'Dim v As Double = Math.Min(apd1.Duration, apd2.Duration) - 0.5
                                        'model.setRowMinMax("TPDependency_" + k.ToString, -v, v)

                                        ' Can be concurrent:
                                        'model.setRowMax("TPDependency_" + k.ToString, 0)

                                        'model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -apd2.GetMaxPeriod)
                                        'model.setRowMin("TPDependency_" + k.ToString, -apd2.GetMaxPeriod)
                                        cDep.AddTerm(-apd2.GetMaxPeriod, variables(dependency.FirstAlternativeID))
                                        gModel.AddConstr(cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                End Select
                            End If
                            k += 1
                        Next
                    End If
                End If

                gModel.Update()
                If OutputPath <> "" Then
                    OutputPath = OutputPath.Replace("\", "\\")
                    gModel.Write(OutputPath)
                    'Model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    'Model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                End If
                gModel.Optimize()

                Dim status As Integer = gModel.Get(GRB.IntAttr.Status)

                Select Case status
                    Case GRB.Status.OPTIMAL
                        SolverState = raSolverState.raSolved

                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        's += "FP_'" + FP.Name + "' = " + Model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        'FP.SetAlternativeAllocatedValue(alt.ID, Model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                        FP.SetAlternativeAllocatedValue(alt.ID, variables("FP" + FP.ID.ToString + "_" + alt.ID).Get(GRB.DoubleAttr.X))
                                    End If
                                Next
                                ''Debug.Print(s)
                            Next

                            ''Debug.Print(vbNewLine)
                            ''Debug.Print("--- FUNDING POOLS VALUES ---")
                            'For Each alt As RAAlternative In Alternatives
                            '    'Debug.Print(vbNewLine)
                            '    Dim s As String = alt.Name + ": "
                            '    For Each FP As RAFundingPool In FundingPools.Pools.Values
                            '        If FP.Enabled Then
                            '            s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                            '        End If
                            '    Next
                            '    'Debug.Print(s)
                            'Next
                        End If

                        mFundedCost = 0
                        mFundedBenefit = 0
                        mFundedOriginalBenefit = 0

                        CurrentScenario.TimePeriods.TimePeriodResults.Clear()

                        Dim i As Integer = 1
                        For Each alt As RAAlternative In Alternatives
                            'alt.Funded = Model.getColumnPrimalActivity(alt.ID)
                            alt.Funded = variables(alt.ID).Get(GRB.DoubleAttr.X)

                            Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                            If alt.Funded > 0.00001 Then
                                mFundedCost += alt.Cost * alt.Funded

                                Dim dFactorPower As Integer = 0
                                Dim discount As Double = 1
                                If Settings.TimePeriods Then
                                    Dim fundedPeriod As Integer = -1
                                    For j As Integer = apd.MinPeriod To apd.MaxPeriod
                                        ''Debug.Print("TPVar_" + alt.ID.ToString + j.ToString + " = " + Model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + j.ToString).ToString)

                                        'If fundedPeriod = -1 AndAlso Model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + j.ToString) > 0 Then
                                        If fundedPeriod = -1 AndAlso variables("TPVar_" + alt.ID.ToString + j.ToString).Get(GRB.DoubleAttr.X) > 0 Then
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
                            'i += 1
                        Next
                    Case GRB.Status.INFEASIBLE, GRB.Status.INF_OR_UNBD
                        SolverState = raSolverState.raInfeasible
                End Select
                LastError = ""
                LastErrorReal = ""  ' D3880
            Catch ex As Exception
                ''Debug.Print("Error occured. Reason Code: " + Model.getXAExceptionCode().ToString + " Message: " + Model.getXAExceptionMessage())
                SolverState = raSolverState.raError
                ' D3880 ===
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
                If gEnv IsNot Nothing Then gEnv.Dispose() Else LastError = "Solver %%solver%% is not available. Contact EC Technical Support"
                ' D3880 ==
            Finally
                If gModel IsNot Nothing And Not KeepGurobiAlive Then gModel.Dispose()
                If gEnv IsNot Nothing And Not KeepGurobiAlive Then gEnv.Dispose()
            End Try

            Return True
        End Function

        Private Function Solve_Gurobi_Full(Optional OutputPath As String = "", Optional SolverParams As String() = Nothing, Optional isCloud As Boolean = False, Optional onAddLogFunction As onAddLogEvent = Nothing) As Boolean
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
                gModel = New Gurobi.GRBModel(gEnv)

                PrintDebugInfo("Gurobi environment created: ")

                Dim maxCost As Double = 0
                Dim maxBenefit As Double = 0
                Dim sumCost As Double = 0
                Dim sumBenefit As Double = 0
                Dim eps As Double = 0.0001
                For Each alt As RAAlternative In Alternatives
                    If alt.Cost > maxCost Then maxCost = alt.Cost
                    If alt.BenefitOriginal > maxBenefit Then maxBenefit = alt.BenefitOriginal
                    sumCost += alt.Cost
                    sumBenefit += alt.BenefitOriginal
                Next

                Dim variables As New Dictionary(Of String, GRBVar)

                'Make decision variable alt1,alt2, .... binary
                Dim altVar As GRBVar
                For Each alt As RAAlternative In Alternatives
                    If alt.IsPartial Then
                        altVar = gModel.AddVar(alt.MinPercent, 1, 0, GRB.SEMICONT, alt.ID)
                    Else
                        altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, alt.ID)
                    End If
                    variables.Add(alt.ID, altVar)
                Next

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                Dim varName As String = "FP" + FP.ID.ToString + "_" + alt.ID
                                Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                Dim fpVar As GRBVar = gModel.AddVar(0, cellCost, 0, GRB.CONTINUOUS, varName)
                                variables.Add(varName, fpVar)
                            End If
                        Next
                    Next
                End If

                Dim TPvariables As New Dictionary(Of String, GRBVar)
                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If Not variables.ContainsKey("TPVar_" + alt.ID.ToString + "_" + j.ToString) Then
                                If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                    altVar = gModel.AddVar(0, 1, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v), GRB.BINARY, "TPVar_" + alt.ID.ToString + "_" + j.ToString)
                                Else
                                    altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "TPVar_" + alt.ID.ToString + "_" + j.ToString)
                                End If
                                variables.Add("TPVar_" + alt.ID.ToString + "_" + j.ToString, altVar)
                                TPvariables.Add("TPVar_" + alt.ID.ToString + "_" + j.ToString, altVar)
                            End If
                            v += 1
                            i += 1
                        Next
                    Next
                End If

                gModel.Update()

                Dim objBenefits As GRBLinExpr = 0.0

                Dim objCost As GRBLinExpr = 0.0
                Dim constraintCost As GRBLinExpr = 0.0
                For Each alt As RAAlternative In Alternatives
                    objBenefits.AddTerm(CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)), variables(alt.ID))
                    constraintCost.AddTerm(alt.Cost, variables(alt.ID))
                    objCost.AddTerm(alt.Cost, variables(alt.ID))
                Next

                For Each tpv As GRBVar In TPvariables.Values
                    objBenefits.AddTerm(tpv.Obj, tpv)
                Next

                'gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, BudgetLimit, "cost")
                Dim costVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "costvar")
                variables.Add("costvar", costVar)
                constraintCost.AddTerm(-BudgetLimit, costVar)
                Dim COSTconstraint As GRBConstr = gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, 0, "cost")

                Dim constraintCostVar As GRBLinExpr = 0.0
                constraintCostVar.AddTerm(1, costVar)
                Dim constraintCVar As GRBConstr = gModel.AddConstr(constraintCostVar, GRB.EQUAL, 1, "costvar")


                Dim maxPriority As Integer = 100

                gModel.ModelSense = GRB.MAXIMIZE
                gModel.SetObjectiveN(objBenefits, 0, maxPriority, 1, 0, 0, "benefits")
                gModel.SetObjective(objBenefits, GRB.MAXIMIZE)

                Dim objIndex As Integer = 1

                Dim useAdditionalObjectives As Boolean = False

                Dim useCostObjective As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).InUse
                Dim costPrty As Integer = maxPriority - CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).Rank
                Dim costMaximize As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).Condition = RASolverPriority.raSolverCondition.raMaximize
                If useCostObjective Then
                    gModel.SetObjectiveN(objCost, objIndex, costPrty, CDbl(If(costMaximize, 1, -1)), 0, 0, "costs")
                    objIndex += 1
                End If

                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0
                For Each alt As RAAlternative In Alternatives
                    If alt.Must Then mustCount += 1
                    If alt.MustNot Then mustnotCount += 1
                Next

                Dim constraintMusts As GRBLinExpr = 0.0
                If Settings.Musts And (mustCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        constraintMusts.AddTerm(CDbl(If(alt.Must, 1, 0)), variables(alt.ID))
                    Next
                    gModel.AddConstr(constraintMusts, GRB.EQUAL, mustCount, "musts")
                End If

                Dim constraintMustNots As GRBLinExpr = 0.0
                If Settings.MustNots And (mustnotCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        constraintMustNots.AddTerm(CDbl(If(alt.MustNot, 1, 0)), variables(alt.ID))
                    Next
                    gModel.AddConstr(constraintMustNots, GRB.EQUAL, 0, "mustnots")
                End If

                If Settings.Groups Then
                    Dim i As Integer = 1
                    For Each group As RAGroup In Groups.Groups.Values
                        If group.Enabled Then
                            Dim gName As String = "Group_" + i.ToString
                            Dim constraintGroup As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                constraintGroup.AddTerm(CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)), variables(alt.ID))
                            Next

                            Select Case group.Condition
                                Case RAGroupCondition.gcLessOrEqualsOne
                                    gModel.AddConstr(constraintGroup, GRB.LESS_EQUAL, 1, gName)
                                Case RAGroupCondition.gcEqualsOne
                                    gModel.AddConstr(constraintGroup, GRB.EQUAL, 1, gName)
                                Case RAGroupCondition.gcGreaterOrEqualsOne
                                    gModel.AddConstr(constraintGroup, GRB.GREATER_EQUAL, 1, gName)
                            End Select
                            i += 1
                        End If
                    Next
                End If

                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In Dependencies.Dependencies
                        Dim constraintDependency As GRBLinExpr = 0.0
                        For Each alt As RAAlternative In Alternatives
                            Select Case Dependency.Value
                                Case RADependencyType.dtDependsOn
                                    ' first is not funded unless second is funded
                                    constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)), variables(alt.ID))
                                Case RADependencyType.dtMutuallyDependent
                                    constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)), variables(alt.ID))
                                Case RADependencyType.dtMutuallyExclusive
                                    constraintDependency.AddTerm(CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) + CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)), variables(alt.ID))
                            End Select
                        Next
                        Dim dName As String
                        Select Case Dependency.Value
                            Case RADependencyType.dtDependsOn
                                dName = "Dependency" + i.ToString + "_DependsOn"
                                gModel.AddConstr(constraintDependency, GRB.GREATER_EQUAL, 0, dName)
                            Case RADependencyType.dtMutuallyDependent
                                dName = "Dependency" + i.ToString + "_MutuallyDependent"
                                gModel.AddConstr(constraintDependency, GRB.EQUAL, 0, dName)
                            Case RADependencyType.dtMutuallyExclusive
                                dName = "Dependency" + i.ToString + "_MutuallyExclusive"
                                gModel.AddConstr(constraintDependency, GRB.LESS_EQUAL, 1, dName)
                        End Select

                        i += 1
                    Next
                End If

                If Settings.CustomConstraints Then
                    'Dim i As Integer = 1
                    For Each Constraint As RAConstraint In Constraints.Constraints.Values
                        If Constraint.Enabled Then
                            Dim useCCasObj As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID).InUse
                            Dim CCPriority As Integer = maxPriority - CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID).Rank
                            Dim CCIsMax As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(Constraint.ID).Condition = RASolverPriority.raSolverCondition.raMaximize
                            If useCCasObj Then
                                Dim constraintObj As GRBLinExpr = 0.0
                                For Each alt As RAAlternative In Alternatives
                                    Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                                    If value <> UNDEFINED_INTEGER_VALUE Then
                                        constraintObj.AddTerm(value, variables(alt.ID))
                                    End If
                                Next
                                gModel.SetObjectiveN(constraintObj, objIndex, CCPriority, CDbl(If(CCIsMax, 1, -1)), 0, 0, "CCObj_" + objIndex.ToString)
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
                                    constraintCC.AddTerm(value, variables(alt.ID))
                                End If
                            Next

                            If Constraint.MinValueSet And Not Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Minimum Value
                                gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName)
                            End If
                            If Constraint.MaxValueSet And Not Constraint.MinValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName)
                            End If
                            If Constraint.MinValueSet And Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName)
                                gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName)
                            End If

                            'i += 1
                        End If
                    Next
                End If

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        Dim constraintFPRow As GRBLinExpr = 0.0

                        Dim fprowName As String = "FProw_" + alt.ID
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                'Model.loadPoint("FProw_" + alt.ID, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                                constraintFPRow.AddTerm(1, variables("FP" + FP.ID.ToString + "_" + alt.ID))
                            End If
                        Next
                        constraintFPRow.AddTerm(-alt.Cost, variables(alt.ID))
                        gModel.AddConstr(constraintFPRow, GRB.EQUAL, 0, fprowName)
                    Next

                    Dim fpPrty As Integer = FundingPools.Pools.Count
                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            Dim fprowName As String = "FProw_" + FP.ID.ToString
                            Dim constraintFPRow As GRBLinExpr = 0.0
                            Dim constraintFPRowObj As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                constraintFPRow.AddTerm(1, variables("FP" + FP.ID.ToString + "_" + alt.ID))
                                constraintFPRowObj.AddTerm(1, variables("FP" + FP.ID.ToString + "_" + alt.ID))
                            Next
                            gModel.AddConstr(constraintFPRow, GRB.LESS_EQUAL, FP.PoolLimit, fprowName)
                            gModel.SetObjectiveN(constraintFPRowObj, objIndex, fpPrty, 1, 0, 0, "fpPrty_" + fpPrty.ToString)
                            objIndex += 1
                            fpPrty -= 1
                        End If
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim TPRowVariantCount As Integer = 0
                    Dim tpResList As New Dictionary(Of String, GRBLinExpr)
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim cTPRowVariant As GRBLinExpr = 0.0
                        cTPRowVariant.AddTerm(-1, variables(alt.ID))
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            v += 1
                            cTPRowVariant.AddTerm(1, variables("TPVar_" + alt.ID.ToString + "_" + j.ToString))
                            i += 1
                        Next
                        gModel.AddConstr(cTPRowVariant, GRB.EQUAL, 0, "TPRowVariant_" + alt.ID)
                        TPRowVariantCount += 1
                    Next

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            For Each alt As RAAlternative In Alternatives
                                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        If t - j < apd.Duration Then
                                            Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - j, alt.ID, resource.ID)
                                            If c <> UNDEFINED_INTEGER_VALUE Then
                                                Dim cTPRes As GRBLinExpr = 0.0
                                                If Not tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                                    tpResList.Add("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, cTPRes)
                                                Else
                                                    cTPRes = tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString)
                                                End If
                                                cTPRes.AddTerm(c, variables("TPVar_" + alt.ID + "_" + j.ToString))                                                'model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPVar_" + alt.ID + "_" + j.ToString, c)
                                            End If
                                        End If
                                    Next
                                End If
                            Next

                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        If Settings.ResourcesMax Then
                                            gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                        End If
                                        If Settings.ResourcesMin Then
                                            gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                        End If
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString)
                                    End If
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString)
                                    End If
                                End If
                            End If
                        Next
                        t += 1
                    Next

                    If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim k As Integer = 1
                        For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                            If dependency.Enabled Then
                                Dim cDep As GRBLinExpr = 0.0
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    cDep.AddTerm(j + 1, variables("TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString))
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    cDep.AddTerm(-(j + 1), variables("TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        cDep.AddTerm(-(apd2.GetMaxPeriod + apd2.Duration), variables(dependency.FirstAlternativeID))
                                        gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration), "TPDependency_" + k.ToString)
                                    Case RADependencyType.dtConcurrent
                                        cDep.AddTerm(-apd2.GetMaxPeriod, variables(dependency.FirstAlternativeID))
                                        gModel.AddConstr(cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                End Select
                            End If
                            k += 1
                        Next
                    End If
                End If

                gModel.Update()

                PrintDebugInfo("Gurobi model updated: ")

                If OutputPath <> "" Then
                    OutputPath = OutputPath.Replace("\", "\\")
                    gModel.Write(OutputPath)
                End If

                gModel.Parameters.TimeLimit = 60
                gModel.Parameters.MIPGap = 0.01

                'gModel.Tune()
                'PrintDebugInfo("Tuning model completed. Result count = " + gModel.TuneResultCount.ToString)
                'If gModel.TuneResultCount > 0 Then
                '    gModel.GetTuneResult(0)
                'End If

                If usePresolve Then
                    gModel = gModel.Presolve()
                End If

                Dim numSteps As Integer = 100

                For c As Integer = 1 To numSteps
                    Dim newBudget As Double = -BudgetLimit / numSteps * c
                    gModel.ChgCoeff(COSTconstraint, costVar, newBudget)
                    'PrintDebugInfo("budget = " + newBudget.ToString)

                    Dim singleUseViolation As Boolean = True
                    Dim otherException As Boolean = False

                    While Not otherException AndAlso singleUseViolation
                        Try
                            gModel.Optimize()
                            singleUseViolation = False
                        Catch ex As Exception
                            If ex.Message.Contains("Single") Then
                                Dim time As String = Now.ToString("hh:mm:ss.ffff tt")
                                singleUseViolation = True
                                If onAddLogFunction IsNot Nothing Then
                                    onAddLogFunction("Gurobi Single-Use license violaion occured ", time)
                                End If
                            Else
                                otherException = True
                            End If
                        End Try
                    End While

                    'PrintDebugInfo("Gurobi model solved: ")

                    Dim status As Integer = gModel.Get(GRB.IntAttr.Status)

                    Select Case status
                        Case GRB.Status.OPTIMAL
                            SolverState = raSolverState.raSolved

                            If Settings.FundingPools Then
                                For Each alt As RAAlternative In Alternatives
                                    Dim s As String = alt.Name + ": "
                                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                                        If FP.Enabled Then
                                            If usePresolve Then
                                                FP.SetAlternativeAllocatedValue(alt.ID, gModel.GetVarByName("FP" + FP.ID.ToString + "_" + alt.ID).Get(GRB.DoubleAttr.X))
                                            Else
                                                FP.SetAlternativeAllocatedValue(alt.ID, variables("FP" + FP.ID.ToString + "_" + alt.ID).Get(GRB.DoubleAttr.X))
                                            End If
                                        End If
                                    Next
                                Next
                            End If

                            mFundedCost = 0
                            mFundedBenefit = 0
                            mFundedOriginalBenefit = 0

                            CurrentScenario.TimePeriods.TimePeriodResults.Clear()

                            Dim i As Integer = 1
                            For Each alt As RAAlternative In Alternatives
                                If usePresolve Then
                                    alt.Funded = gModel.GetVarByName(alt.ID).Get(GRB.DoubleAttr.X)
                                Else
                                    alt.Funded = variables(alt.ID).Get(GRB.DoubleAttr.X)
                                End If

                                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                If alt.Funded > 0.00001 Then
                                    mFundedCost += alt.Cost * alt.Funded

                                    Dim dFactorPower As Integer = 0
                                    Dim discount As Double = 1
                                    If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                                        Dim fundedPeriod As Integer = -1
                                        For j As Integer = apd.MinPeriod To apd.MaxPeriod
                                            Dim TPVarFunded As Boolean
                                            If usePresolve Then
                                                TPVarFunded = gModel.GetVarByName("TPVar_" + alt.ID.ToString + "_" + j.ToString).Get(GRB.DoubleAttr.X) > 0
                                            Else
                                                TPVarFunded = variables("TPVar_" + alt.ID.ToString + "_" + j.ToString).Get(GRB.DoubleAttr.X) > 0
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
                                'i += 1
                            Next
                        Case GRB.Status.INFEASIBLE, GRB.Status.INF_OR_UNBD
                            SolverState = raSolverState.raInfeasible
                    End Select
                    LastError = ""
                    LastErrorReal = ""  ' D3880
                Next
            Catch ex As Exception
                SolverState = raSolverState.raError
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
                If gEnv IsNot Nothing Then gEnv.Dispose() Else LastError = "Solver %%solver%% is not available. Contact EC Technical Support"
            Finally
                If gModel IsNot Nothing And Not KeepGurobiAlive Then gModel.Dispose()
                If gEnv IsNot Nothing And Not KeepGurobiAlive Then gEnv.Dispose()
            End Try

            PrintDebugInfo("Gurobi solve function completed: ")

            Return True
        End Function

        Private Function GetGurobiModelWithInfeasibilities(gEnv As GRBEnv, IsForInfeasibilities As Boolean, ByRef gVariables As Dictionary(Of String, GRBVar), ByRef gConstraints As Dictionary(Of String, GRBConstr), ByRef gGenConstraints As Dictionary(Of String, GRBGenConstr)) As GRBModel
            If ProjectManager Is Nothing Then Return Nothing
            Try
                gModel = New Gurobi.GRBModel(gEnv)

                Dim ZConstraint As GRBLinExpr = 0.0

                Dim maxCost As Double = 0
                Dim maxBenefit As Double = 0
                Dim sumCost As Double = 0
                Dim sumBenefit As Double = 0
                Dim eps As Double = 0.0001
                For Each alt As RAAlternative In Alternatives
                    If alt.Cost > maxCost Then maxCost = alt.Cost
                    If alt.BenefitOriginal > maxBenefit Then maxBenefit = alt.BenefitOriginal
                    sumCost += alt.Cost
                    sumBenefit += alt.BenefitOriginal
                Next

                gVariables = New Dictionary(Of String, GRBVar)
                If IsForInfeasibilities Then
                    gGenConstraints = New Dictionary(Of String, GRBGenConstr)
                Else
                    gConstraints = New Dictionary(Of String, GRBConstr)
                End If

                'Make decision variable alt1,alt2, .... binary
                Dim altVar As GRBVar
                For Each alt As RAAlternative In Alternatives
                    If alt.IsPartial Then
                        altVar = gModel.AddVar(alt.MinPercent, 1, 0, GRB.SEMICONT, alt.ID)
                    Else
                        altVar = gModel.AddVar(0, 1, 0, GRB.BINARY, alt.ID)
                    End If
                    gVariables.Add(alt.ID, altVar)
                Next

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                Dim varName As String = "FP" + FP.ID.ToString + "_" + alt.ID
                                Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                Dim fpVar As GRBVar = gModel.AddVar(0, cellCost, 0, GRB.CONTINUOUS, varName)
                                gVariables.Add(varName, fpVar)
                            End If
                        Next
                    Next
                End If

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

                gModel.Update()

                Dim objBenefits As GRBLinExpr = 0.0

                Dim objCost As GRBLinExpr = 0.0
                Dim constraintCost As GRBLinExpr = 0.0
                For Each alt As RAAlternative In Alternatives
                    objBenefits.AddTerm(CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)), gVariables(alt.ID))
                    constraintCost.AddTerm(alt.Cost, gVariables(alt.ID))
                    objCost.AddTerm(alt.Cost, gVariables(alt.ID))
                Next

                For Each tpv As GRBVar In TPvariables.Values
                    objBenefits.AddTerm(tpv.Obj, tpv)
                Next

                'gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, BudgetLimit, "cost")
                Dim costVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "costvar")
                gVariables.Add("costvar", costVar)
                constraintCost.AddTerm(-BudgetLimit, costVar)
                Dim COSTconstraint As GRBConstr = gModel.AddConstr(constraintCost, GRB.LESS_EQUAL, 0, "cost")
                If Not IsForInfeasibilities Then
                    gConstraints.Add("cost", COSTconstraint)
                End If

                Dim constraintCostVar As GRBLinExpr = 0.0
                constraintCostVar.AddTerm(1, costVar)
                Dim constraintCVar As GRBConstr = gModel.AddConstr(constraintCostVar, GRB.EQUAL, 1, "costvar")
                If Not IsForInfeasibilities Then
                    gConstraints.Add("costvar", constraintCVar)
                End If

                Dim maxPriority As Integer = 100

                gModel.ModelSense = GRB.MAXIMIZE
                gModel.SetObjectiveN(objBenefits, 0, maxPriority, 1, 0, 0, "benefits")
                gModel.SetObjective(objBenefits, GRB.MAXIMIZE)

                Dim objIndex As Integer = 1

                Dim useAdditionalObjectives As Boolean = False

                Dim useCostObjective As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).InUse
                Dim costPrty As Integer = maxPriority - CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).Rank
                Dim costMaximize As Boolean = CurrentScenario.SolverPriorities.GetSolverPriorityByID(RA_FUNDED_COST_ID).Condition = RASolverPriority.raSolverCondition.raMaximize
                If useCostObjective Then
                    gModel.SetObjectiveN(objCost, objIndex, costPrty, CDbl(If(costMaximize, 1, -1)), 0, 0, "costs")
                    objIndex += 1
                End If

                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0
                For Each alt As RAAlternative In Alternatives
                    If alt.Must Then mustCount += 1
                    If alt.MustNot Then mustnotCount += 1
                Next

                'Dim constraintMusts As GRBLinExpr = 0.0
                If Settings.Musts And (mustCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        If alt.Must Then
                            Dim constraintMust As GRBLinExpr = 0.0
                            constraintMust.AddTerm(1, gVariables(alt.ID))
                            Dim zVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "zMust_" + alt.ID)
                            Dim czConstr As GRBGenConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintMust, GRB.EQUAL, 1, "zMustConstr_" + alt.ID)
                            gGenConstraints.Add("zMustConstr_" + alt.ID, czConstr)

                            ZConstraint.AddTerm(1, zVar)
                        End If

                        'constraintMusts.AddTerm(CDbl(If(alt.Must, 1, 0)), gVariables(alt.ID))
                    Next
                    'Dim mustConstr As GRBConstr = gModel.AddConstr(constraintMusts, GRB.EQUAL, mustCount, "musts")
                    'gConstraints.Add("musts", mustConstr)
                End If

                'Dim constraintMustNots As GRBLinExpr = 0.0
                If Settings.MustNots And (mustnotCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        If alt.MustNot Then
                            Dim constraintMustNots As GRBLinExpr = 0.0
                            'constraintMustNots.AddTerm(CDbl(If(alt.MustNot, 1, 0)), gVariables(alt.ID))
                            constraintMustNots.AddTerm(1, gVariables(alt.ID))
                            Dim zVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "zMustNot_" + alt.ID)
                            Dim czConstr As GRBGenConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintMustNots, GRB.EQUAL, 1, "zConstrMustNot_" + alt.ID)
                            gGenConstraints.Add("zConstrMustNot_" + alt.ID, czConstr)
                        End If
                    Next
                    'Dim mustnotConstr As GRBConstr = gModel.AddConstr(constraintMustNots, GRB.EQUAL, 0, "mustnots")
                    'gConstraints.Add("mustnots", mustnotConstr)
                End If

                If Settings.Groups Then
                    Dim i As Integer = 1
                    For Each group As RAGroup In Groups.Groups.Values
                        If group.Enabled Then
                            Dim gName As String = "Group_" + i.ToString
                            Dim constraintGroup As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                constraintGroup.AddTerm(CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)), gVariables(alt.ID))
                            Next

                            Dim zVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "zGroup_" + gName)

                            Dim groupConstr As GRBConstr = Nothing
                            Dim czgroupConstr As GRBGenConstr = Nothing
                            Select Case group.Condition
                                Case RAGroupCondition.gcLessOrEqualsOne
                                    'groupConstr = gModel.AddConstr(constraintGroup, GRB.LESS_EQUAL, 1, gName)
                                    czgroupConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintGroup, GRB.LESS_EQUAL, 1, "zConstrGroup_" + gName)
                                Case RAGroupCondition.gcEqualsOne
                                    'groupConstr = gModel.AddConstr(constraintGroup, GRB.EQUAL, 1, gName)
                                    czgroupConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintGroup, GRB.EQUAL, 1, "zConstrGroup_" + gName)
                                Case RAGroupCondition.gcGreaterOrEqualsOne
                                    'groupConstr = gModel.AddConstr(constraintGroup, GRB.GREATER_EQUAL, 1, gName)
                                    czgroupConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintGroup, GRB.GREATER_EQUAL, 1, "zConstrGroup_" + gName)
                            End Select
                            'If groupConstr IsNot Nothing Then gConstraints.Add(gName, groupConstr)
                            If czgroupConstr IsNot Nothing Then gGenConstraints.Add("zConstrGroup_" + gName, czgroupConstr)

                            i += 1
                        End If
                    Next
                End If

                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In Dependencies.Dependencies
                        Dim constraintDependency As GRBLinExpr = 0.0
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
                        Dim czdepConstr As GRBGenConstr = Nothing
                        Dim zVar As GRBVar = gModel.AddVar(0, 1, 0, GRB.BINARY, "zDependency_" + dName)
                        Select Case Dependency.Value
                            Case RADependencyType.dtDependsOn
                                dName = "Dependency" + i.ToString + "_DependsOn"
                                'depConstr = gModel.AddConstr(constraintDependency, GRB.GREATER_EQUAL, 0, dName)
                                czdepConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintDependency, GRB.GREATER_EQUAL, 0, "zConstrDependency_" + dName)
                            Case RADependencyType.dtMutuallyDependent
                                dName = "Dependency" + i.ToString + "_MutuallyDependent"
                                'depConstr = gModel.AddConstr(constraintDependency, GRB.EQUAL, 0, dName)
                                czdepConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintDependency, GRB.EQUAL, 0, "zConstrDependency_" + dName)
                            Case RADependencyType.dtMutuallyExclusive
                                dName = "Dependency" + i.ToString + "_MutuallyExclusive"
                                'depConstr = gModel.AddConstr(constraintDependency, GRB.LESS_EQUAL, 1, dName)
                                czdepConstr = gModel.AddGenConstrIndicator(zVar, 1, constraintDependency, GRB.LESS_EQUAL, 1, "zConstrDependency_" + dName)
                        End Select
                        'If depConstr IsNot Nothing Then gConstraints.Add(dName, depConstr)
                        If czdepConstr IsNot Nothing Then gGenConstraints.Add("zConstrDependency_" + dName, czdepConstr)
                        i += 1
                    Next
                End If

                If Settings.CustomConstraints Then
                    'Dim i As Integer = 1
                    For Each Constraint As RAConstraint In Constraints.Constraints.Values
                        If Constraint.Enabled Then
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
                                gModel.SetObjectiveN(constraintObj, objIndex, CCPriority, CDbl(If(CCIsMax, 1, -1)), 0, 0, "CCObj_" + objIndex.ToString)
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
                            If Constraint.MinValueSet And Not Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Minimum Value
                                ccConstr = gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName + "_Min")
                                gConstraints.Add(cName + "_Min", ccConstr)
                            End If
                            If Constraint.MaxValueSet And Not Constraint.MinValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                ccConstr = gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName + "_Max")
                                gConstraints.Add(cName + "_Max", ccConstr)
                            End If
                            If Constraint.MinValueSet And Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                ccConstr = gModel.AddConstr(constraintCC, GRB.GREATER_EQUAL, Constraint.MinValue, cName + "_Min")
                                gConstraints.Add(cName + "_Min", ccConstr)
                                ccConstr = gModel.AddConstr(constraintCC, GRB.LESS_EQUAL, Constraint.MaxValue, cName + "_Max")
                                gConstraints.Add(cName + "_Max", ccConstr)
                            End If

                            'i += 1
                        End If
                    Next
                End If

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        Dim constraintFPRow As GRBLinExpr = 0.0

                        Dim fprowName As String = "FProw_" + alt.ID
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                'Model.loadPoint("FProw_" + alt.ID, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                                constraintFPRow.AddTerm(1, gVariables("FP" + FP.ID.ToString + "_" + alt.ID))
                            End If
                        Next
                        constraintFPRow.AddTerm(-alt.Cost, gVariables(alt.ID))
                        Dim fpRowConstr As GRBConstr = gModel.AddConstr(constraintFPRow, GRB.EQUAL, 0, fprowName)
                        gConstraints.Add(fprowName, fpRowConstr)
                    Next

                    Dim fpPrty As Integer = FundingPools.Pools.Count
                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            Dim fprowName As String = "FProw_" + FP.ID.ToString
                            Dim constraintFPRow As GRBLinExpr = 0.0
                            Dim constraintFPRowObj As GRBLinExpr = 0.0
                            For Each alt As RAAlternative In Alternatives
                                constraintFPRow.AddTerm(1, gVariables("FP" + FP.ID.ToString + "_" + alt.ID))
                                constraintFPRowObj.AddTerm(1, gVariables("FP" + FP.ID.ToString + "_" + alt.ID))
                            Next
                            Dim fpConstr As GRBConstr = gModel.AddConstr(constraintFPRow, GRB.LESS_EQUAL, FP.PoolLimit, fprowName)
                            gConstraints.Add(fprowName, fpConstr)
                            gModel.SetObjectiveN(constraintFPRowObj, objIndex, fpPrty, 1, 0, 0, "fpPrty_" + fpPrty.ToString)
                            objIndex += 1
                            fpPrty -= 1
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
                            For Each alt As RAAlternative In Alternatives
                                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        If t - j < apd.Duration Then
                                            Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - j, alt.ID, resource.ID)
                                            If c <> UNDEFINED_INTEGER_VALUE Then
                                                Dim cTPRes As GRBLinExpr = 0.0
                                                If Not tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                                    tpResList.Add("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, cTPRes)
                                                Else
                                                    cTPRes = tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString)
                                                End If
                                                cTPRes.AddTerm(c, gVariables("TPVar_" + alt.ID + "_" + j.ToString))                                                'model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPVar_" + alt.ID + "_" + j.ToString, c)
                                            End If
                                        End If
                                    Next
                                End If
                            Next

                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        If Settings.ResourcesMax Then
                                            Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                            gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max", tpresMainConstr)
                                        End If
                                        If Settings.ResourcesMin Then
                                            Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                            gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min", tpresMainConstr)
                                        End If
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.LESS_EQUAL, maxValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max")
                                        gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Max", tpresMainConstr)
                                    End If
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    If tpResList.ContainsKey("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString) Then
                                        Dim tpresMainConstr As GRBConstr = gModel.AddConstr(tpResList("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString), GRB.GREATER_EQUAL, minValue, "TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min")
                                        gConstraints.Add("TPRes" + t.ToString + "_" + resource.ID.ToString + "_Min", tpresMainConstr)
                                    End If
                                End If
                            End If
                        Next
                        t += 1
                    Next

                    If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim k As Integer = 1
                        For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                            If dependency.Enabled Then
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
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        cDep.AddTerm(-(apd2.GetMaxPeriod + apd2.Duration), gVariables(dependency.FirstAlternativeID))
                                        tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration), "TPDependency_" + k.ToString)
                                    Case RADependencyType.dtConcurrent
                                        cDep.AddTerm(-apd2.GetMaxPeriod, gVariables(dependency.FirstAlternativeID))
                                        tpDepConstr = gModel.AddConstr(cDep, GRB.GREATER_EQUAL, -apd2.GetMaxPeriod, "TPDependency_" + k.ToString)
                                End Select
                                If tpDepConstr IsNot Nothing Then gConstraints.Add("TPDependency_" + k.ToString, tpDepConstr)
                            End If
                            k += 1
                        Next
                    End If
                End If

                Dim czConstraint As GRBConstr = gModel.AddConstr(ZConstraint, GRB.EQUAL, gGenConstraints.Values.Count, "czConstraint")

                gModel.Update()

                gModel.Parameters.TimeLimit = 60
                gModel.Parameters.MIPGap = 0.01

            Catch ex As Exception
                LastErrorReal = ex.Message
                LastError = ex.Message  ' D3628
                If gModel IsNot Nothing Then gModel.Dispose() Else LastError = "Error on create model for solver"
            End Try

            Return gModel
        End Function
    End Class

End Namespace
