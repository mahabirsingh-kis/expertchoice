Option Strict On

Imports ECCore
Imports ECCore.MiscFuncs 'A1708
Imports Canvas
Imports Optimizer
Imports System.Linq

Namespace Canvas


    Partial Public Class RASolver

        Public Property XA_STRATEGIES As Integer() = {1, 2, 3, 4, 6, 7, 8, 9, 10}
        Public Property XA_VARIATIONS As String() = {"No variation", "A", "B", "C", "D", "AC", "AD", "BC", "BD"}

        Public Property XAStrategy As Integer = 1
        Private mXAVariation As String = XA_VARIATIONS(0)   ' D3877

        Private UseConstraintsAnalysis As Boolean = False

        Public Property XAVariation As String
            Get
                If mXAVariation = XA_VARIATIONS(0) Then ' D3877
                    Return ""
                Else
                    Return mXAVariation
                End If
            End Get
            Set(value As String)
                If value = "" Then mXAVariation = XA_VARIATIONS(0) Else mXAVariation = value ' D3877
            End Set
        End Property

        Public Property XATimeOutGlobal As Integer = 150
        Public Property XATimeoutUnchanged As Integer = 30

        Public Property RiskLimitMin As Double = 0
        Public Property RiskLimitMax As Double = 0
        Public Property UseRiskLimit As Boolean = False

        Public ReadOnly Property XACommand As String
            Get
                Return "Presolve 1 " + "strategy " + XAStrategy.ToString + XAVariation + "  StopUnchange " + CInt(XATimeoutUnchanged \ 60).ToString + ":" + CInt(XATimeoutUnchanged Mod 60).ToString + "  Set TimeLimit " + CInt(XATimeOutGlobal \ 60).ToString + ":" + CInt(XATimeOutGlobal Mod 60).ToString
            End Get
        End Property

        Private Function GetRABaronModel() As String
            Dim res As String

            Return res
        End Function

        Private Sub ParseRABaronResults(Results As String)

        End Sub

        Private Function Solve_BARON(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()


            Return True
        End Function

        Private Function Solve_XA(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            Dim model As New Optimizer.XA.Optimize(64, 0, 0, 0, 100, 100)  ' Initialize XA
            model.setActivationCodes(Reserve1, Reserve2)    ' Activation codes
            model.setXAMessageWindowOff()                   ' Remove XA Message Window

            Try
                model.openConnection()                          ' Connect  

                model.setMaximizeObjective()                    ' Maximize the objective   

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
                For Each alt As RAAlternative In Alternatives
                    model.setColumnObjective(alt.ID, adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)))
                Next

                'Make decision variable alt1,alt2, .... binary
                For Each alt As RAAlternative In Alternatives
                    If alt.IsPartial Then
                        model.setColumnSemiContinuous(alt.ID, alt.MinPercent, 1)
                    Else
                        model.setColumnBinary(alt.ID)
                    End If
                Next

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                model.setColumnObjective("FP" + FP.ID.ToString + "_" + alt.ID, 0)
                                Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = Integer.MaxValue
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    For Each alt As RAAlternative In Alternatives
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            model.setColumnObjective("TPVar_" + alt.ID.ToString + "_" + j.ToString, 0)
                            model.setColumnBinary("TPVar_" + alt.ID.ToString + "_" + j.ToString)
                        Next
                    Next
                End If

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

                If UseRiskLimit Then
                    model.setRowMinMax("RiskLimit", RiskLimitMin, RiskLimitMax)
                    For Each alt As RAAlternative In Alternatives
                        model.loadToCurrentRow(alt.IDVar, alt.RiskOriginal)
                    Next
                End If

                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0
                For Each alt As RAAlternative In Alternatives
                    If alt.Must Then mustCount += 1
                    If alt.MustNot Then mustnotCount += 1
                Next


                If Settings.Musts And (mustCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        model.loadPoint("Must", alt.ID, CDbl(If(alt.Must, 1, 0)))
                    Next
                    model.setRowFix("Must", mustCount)
                End If

                If Settings.MustNots And (mustnotCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        model.loadPoint("MustNot", alt.ID, CDbl(If(alt.MustNot, 1, 0)))
                    Next
                    model.setRowFix("MustNot", 0)
                End If

                If Settings.Groups Then
                    Dim i As Integer = 1
                    For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                        'For Each group As RAGroup In Groups.Groups.Values
                        Dim filteredAlts As List(Of RAAlternative) = group.GetFilteredAlternatives(CurrentScenario.Alternatives)
                        If group.Enabled And filteredAlts.Count > 1 Then
                            Dim gName As String = "Group_" + i.ToString
                            For Each alt As RAAlternative In Alternatives
                                model.loadPoint(gName, alt.ID, CDbl(If(filteredAlts.FirstOrDefault(Function(p) (p.ID.ToLower = alt.ID.ToLower)) IsNot Nothing, 1, 0)))
                            Next

                            Select Case group.Condition
                                Case RAGroupCondition.gcLessOrEqualsOne
                                    ' none of one alterantive from the group if funded
                                    model.setRowMax(gName, 1)
                                Case RAGroupCondition.gcEqualsOne
                                    ' exactly one alterantive from the group is funded
                                    model.setRowFix(gName, 1)
                                Case RAGroupCondition.gcGreaterOrEqualsOne
                                    ' at least one alternative from the group is funded
                                    model.setRowMin(gName, 1)
                                Case RAGroupCondition.gcAllOrNothing
                                    model.setColumnObjective("g_" + group.ID.ToString, 0)
                                    model.setColumnBinary("g_" + group.ID.ToString)
                                    model.loadPoint(gName, "g_" + group.ID.ToString, -filteredAlts.Count)
                                    model.setRowFix(gName, 0)
                            End Select
                            i += 1
                        End If
                    Next
                End If

                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In CurrentScenario.GetAvailableDependencies
                        If Dependency.Enabled Then
                            'For Each Dependency As RADependency In Dependencies.Dependencies
                            For Each alt As RAAlternative In Alternatives
                                Dim dName As String
                                Select Case Dependency.Value
                                    Case RADependencyType.dtDependsOn
                                        ' first is not funded unless second is funded
                                        dName = "Dependency" + i.ToString + "_DependsOn"
                                        model.loadPoint(dName, alt.ID, CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)))
                                        model.setRowMin(dName, 0)
                                        'dTerm = model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p) - (Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p)))) >= 0
                                    Case RADependencyType.dtMutuallyDependent
                                        dName = "Dependency" + i.ToString + "_MutuallyDependent"
                                        model.loadPoint(dName, alt.ID, CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)))
                                        model.setRowFix(dName, 0)
                                        'dTerm = model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) - (Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) = 0
                                    Case RADependencyType.dtMutuallyExclusive
                                        dName = "Dependency" + i.ToString + "_MutuallyExclusive"
                                        model.loadPoint(dName, alt.ID, CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) + CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)))
                                        model.setRowMax(dName, 1)
                                        'dTerm = model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) + (Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) <= 1
                                End Select
                            Next
                        End If
                        i += 1
                    Next
                End If

                If Settings.CustomConstraints Then
                    Dim i As Integer = 1
                    For Each Constraint As RAConstraint In Constraints.Constraints.Values
                        If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
                            ' for each custom constraint we create a vector that holds custom constrain values in the field Cost
                            Dim cName As String = "CustomConstraint_" + i.ToString
                            For Each alt As RAAlternative In Alternatives
                                Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                                If value <> UNDEFINED_INTEGER_VALUE Then
                                    model.loadPoint(cName, alt.ID, value)
                                End If
                                'model.loadPoint(cName, alt.ID, CDbl(If(value = UNDEFINED_INTEGER_VALUE, 0, value)))
                            Next

                            If Constraint.MinValueSet And Not Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Minimum Value
                                'Dim ccTermMin As Term = model.GreaterEqual(model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MinValue)
                                model.setRowMin(cName, Constraint.MinValue)
                            End If
                            If Constraint.MaxValueSet And Not Constraint.MinValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                'Dim ccTermMax As Term = model.LessEqual(model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MaxValue)
                                model.setRowMax(cName, Constraint.MaxValue)
                            End If
                            If Constraint.MinValueSet And Constraint.MaxValueSet Then
                                ' Adding constraint for custom constraint for Maximum Value
                                'Dim ccTermMax As Term = model.LessEqual(model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MaxValue)
                                model.setRowMinMax(cName, Constraint.MinValue, Constraint.MaxValue)
                            End If

                            i += 1
                        End If
                    Next
                End If

                If Settings.FundingPools AndAlso FundingPools.Pools.Count > 0 AndAlso FundingPools.Pools.Values.FirstOrDefault(Function(p) p.Enabled) IsNot Nothing Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                model.loadPoint("FProw_" + alt.ID, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                            End If
                        Next
                        model.loadPoint("FProw_" + alt.ID, alt.ID, -alt.Cost)
                        model.setRowFix("FProw_" + alt.ID, 0)
                    Next

                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            For Each alt As RAAlternative In Alternatives
                                model.loadPoint("FProw_" + FP.ID.ToString, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                            Next
                            model.setRowMax("FProw_" + FP.ID.ToString, FP.PoolLimit)
                        End If
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim TPRowVariantCount As Integer = 0
                    Dim TPRowCount As Integer = 0
                    Dim i As Integer = 1
                    Dim altPos As Integer = 0
                    For Each alt As RAAlternative In Alternatives
                        model.loadPoint("TPRowVariant_" + alt.ID.ToString, alt.ID.ToString, -1)

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                model.setColumnObjective("TPVar_" + alt.ID.ToString + "_" + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                            End If
                            v += 1

                            model.loadPoint("TPRowVariant_" + alt.ID.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)

                            i += 1
                        Next
                        model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        TPRowVariantCount += 1
                        altPos += 1
                    Next

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                For Each alt As RAAlternative In Alternatives
                                    Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                    If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                        For k As Integer = Math.Max(apd.MinPeriod, t - apd.Duration + 1) To Math.Min(t, apd.MaxPeriod)
                                            Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - k, alt.ID, resource.ID)
                                            If c <> UNDEFINED_INTEGER_VALUE Then
                                                model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Min", "TPVar_" + alt.ID + "_" + k.ToString, c)
                                                model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Max", "TPVar_" + alt.ID + "_" + k.ToString, c)
                                            End If
                                        Next
                                    End If
                                Next
                            End If

                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130   
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If Settings.ResourcesMin And Settings.ResourcesMax Then
                                        model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Min", minValue)
                                        model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Max", maxValue)
                                    Else
                                        If Settings.ResourcesMin Then
                                            model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Min", minValue)
                                        Else
                                            If Settings.ResourcesMax Then
                                                model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Max", maxValue)
                                            End If
                                        End If
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                    model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Max", maxValue)
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString + "_Min", minValue)
                                End If
                            End If
                        Next
                        t += 1
                    Next

                    If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                        Dim k As Integer = 1
                        For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                            If dependency.Enabled And dependency.Value <> RADependencyType.dtConcurrent Then
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString, j + 1)
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString, -(j + 1))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        'model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        'model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))

                                        Dim bigM As Integer = 100
                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -bigM)
                                        model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - bigM)
                                    Case RADependencyType.dtConcurrent
                                        ' UNCOMMENT TO FORCE CONCURRENT:
                                        'Dim v As Double = Math.Min(apd1.Duration, apd2.Duration) - 0.5
                                        'model.setRowMinMax("TPDependency_" + k.ToString, -v, v)

                                        ' Can be concurrent:
                                        'model.setRowMax("TPDependency_" + k.ToString, 0)

                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -apd2.GetMaxPeriod)
                                        model.setRowMin("TPDependency_" + k.ToString, -apd2.GetMaxPeriod)
                                End Select
                            End If
                            k += 1
                        Next
                    End If
                End If

                If OutputPath <> "" Then
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_mps_model    ToMPS Yes", OutputPath))
                End If

                Dim command As String = XACommand
                model.setCommand(command)
                'model.solveWithInfeasibleAnalysis()

                model.solve()

                Select Case model.getModelStatus
                    Case 1, 2
                        SolverState = raSolverState.raSolved

                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
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
                            alt.Funded = model.getColumnPrimalActivity(alt.ID)

                            Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                            If alt.Funded > 0.00001 Then
                                mFundedCost += alt.Cost * alt.Funded

                                Dim dFactorPower As Integer = 0
                                Dim discount As Double = 1
                                If Settings.TimePeriods Then
                                    Dim fundedPeriod As Integer = -1
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        If fundedPeriod = -1 AndAlso model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + "_" + j.ToString) > 0 Then
                                            fundedPeriod = j
                                        End If
                                        i += 1
                                    Next
                                    If fundedPeriod <> -1 Then
                                        CurrentScenario.TimePeriods.TimePeriodResults.Add(alt.ID, fundedPeriod)
                                        If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                            dFactorPower = fundedPeriod - apd.GetMinPeriod
                                            discount = 1 / Math.Pow(1 + CurrentScenario.TimePeriods.DiscountFactor, dFactorPower)
                                        End If
                                    End If
                                End If

                                Dim RiskBenefit As Double = CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal))
                                mFundedBenefit += RiskBenefit * discount * alt.Funded
                                mFundedOriginalBenefit += alt.BenefitOriginal * discount * alt.Funded
                            Else
                                alt.Funded = 0
                                i += apd.GetMaxPeriod - apd.GetMinPeriod + 1
                            End If
                            'i += 1
                        Next
                    Case 4, 10
                        SolverState = raSolverState.raInfeasible
                End Select
                LastError = ""
            Catch ex As Exception
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return True
        End Function

        Public Sub Solve_XA_EfficientFrontier(ByRef Settings As EfficientFrontierSettings, Optional ConstraintType As EfficientFrontierConstraintType = EfficientFrontierConstraintType.BudgetLimit, Optional IsPointByPoint As Boolean = False, Optional KeepFunded As Boolean = False)
            If ProjectManager Is Nothing Then Exit Sub

            'A1701 ===
            Dim oldScenarioID As Integer = ResourceAligner.Scenarios.ActiveScenarioID
            ResourceAligner.Scenarios.ActiveScenarioID = Settings.ScenarioID
            'A1701 ==

            Dim IsSingleScenarioChecked As Boolean = ResourceAligner.Scenarios.Scenarios.Where(Function(scn As KeyValuePair(Of Integer, RAScenario)) scn.Value.IsCheckedIB).Count = 1

            PrintDebugInfo("Solve XA Efficient Frontier started: ")
            'ResetFunded()

            Dim BaseCaseMax As Double = GetBaseCaseMaximum()

            Dim isCancelled As Boolean = False

            For Each interval As EfficientFrontierInterval In Settings.Intervals
                Select Case interval.DeltaType
                    Case EfficientFrontierDeltaType.NumberOfSteps, EfficientFrontierDeltaType.DeltaValue, EfficientFrontierDeltaType.AllSolutions
                        Dim stepValue As Double = If(interval.DeltaType = EfficientFrontierDeltaType.DeltaValue, interval.DeltaValue, (interval.MaxValue - interval.MinValue) / interval.DeltaValue)
                        Dim i As Double = If((interval.DeltaType = EfficientFrontierDeltaType.DeltaValue Or interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps) And Settings.IsIncreasing, interval.MinValue, interval.MaxValue)
                        'While If(interval.DeltaType = EfficientFrontierDeltaType.DeltaValueIncreasing, i <= interval.MaxValue AndAlso (interval.Results.Count = 0 OrElse ((interval.Results.Count < 2 OrElse interval.Results(interval.Results.Count - 1).FundedBenefits <> interval.Results(interval.Results.Count - 2).FundedBenefits)), i >= interval.MinValue AndAlso (interval.Results.Count = 0 OrElse interval.Results(interval.Results.Count - 1).SolverState = raSolverState.raSolved))
                        While If((interval.DeltaType = EfficientFrontierDeltaType.DeltaValue Or interval.DeltaType = EfficientFrontierDeltaType.NumberOfSteps) And Settings.IsIncreasing, i <= interval.MaxValue, i >= interval.MinValue AndAlso (interval.Results.Count = 0 OrElse interval.Results(interval.Results.Count - 1).SolverState = raSolverState.raSolved))
                            If isCancelled Then Exit For

                            PrintDebugInfo("i = " + i.ToString)

                            Dim stepResults As EfficientFrontierResults = New EfficientFrontierResults
                            stepResults.SolveToken = Settings.SolveToken
                            SolveStep(Settings.ScenarioID, i, stepResults, ConstraintType = EfficientFrontierConstraintType.CustomConstraint, Settings.ConstraintID, IsSingleScenarioChecked, Settings.KeepFundedAlts)

                            Dim includeStep As Boolean = False
                            If interval.Results.Count > 0 Then
                                Dim lastAlts As Dictionary(Of String, Double) = interval.Results.Last().AlternativesData
                                If lastAlts.Count = stepResults.AlternativesData.Count Then
                                    For Each kvp As KeyValuePair(Of String, Double) In stepResults.AlternativesData
                                        If Not lastAlts.ContainsKey(kvp.Key) Then
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

                            PrintDebugInfo("XA datapoint (step) solved: ")

                            If includeStep Then
                                stepResults.ScenarioID = Settings.ScenarioID
                                stepResults.ScenarioIndex = Settings.ScenarioIndex 'A1708
                                interval.Results.Add(stepResults)
                            End If

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

            PrintDebugInfo("XA efficient frontier solve completed: ")

            ResourceAligner.Scenarios.ActiveScenarioID = oldScenarioID
        End Sub

        Private Sub SolveStep(scenarioID As Integer, curBudget As Double, retVal As EfficientFrontierResults, IsParameterCustomConstraint As Boolean, SelectedConstraintID As Integer, IsSingleScenarioChecked As Boolean, tKeepFundedAlts As Boolean)
            Dim RA As ResourceAligner = ResourceAligner
            Dim PM As clsProjectManager = ProjectManager
            Dim BaseScenario As RAScenario = RA.Scenarios.Scenarios(0)

            Dim scenario As RAScenario = RA.Scenarios.GetScenarioById(scenarioID)

            Dim oldActiveScenarioID As Integer = RA.Scenarios.ActiveScenarioID
            Dim oldSolverMode As RiskOptimizationType = RA.RiskOptimizer.CurrentScenario.OptimizationType

            Dim tSelectedConstraintID As Integer = -1
            If IsParameterCustomConstraint Then tSelectedConstraintID = SelectedConstraintID
            Dim IsParameterRiskConstraint As Boolean = tSelectedConstraintID = Integer.MaxValue

            retVal.SolverState = raSolverState.raNone
            retVal.ScenarioID = scenarioID

            If scenario IsNot Nothing Then
                If RA.Scenarios.ActiveScenarioID <> scenarioID Then RA.Scenarios.ActiveScenarioID = scenarioID
                If RA.RiskOptimizer.CurrentScenario.OptimizationType <> RiskOptimizationType.BudgetLimit Then RA.RiskOptimizer.CurrentScenario.OptimizationType = RiskOptimizationType.BudgetLimit
                RA.Solver.ResetSolver()

                Dim Alts0 As List(Of RAAlternative) = Nothing
                If Not PM.IsRiskProject Then Alts0 = BaseScenario.AlternativesFull

                Dim SelectedEventIDs As List(Of Guid) = Nothing
                If PM.IsRiskProject Then
                    SelectedEventIDs = New List(Of Guid)
                    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                        If alt.Enabled Then
                            SelectedEventIDs.Add(alt.NodeGuidID)
                        End If
                    Next
                End If

                Dim oldSettings As RASettings = New RASettings

                'store old Budget value
                Dim oldBudget As Double
                If PM.IsRiskProject Then
                    oldBudget = RA.RiskOptimizer.BudgetLimit
                    With PM.ResourceAligner.RiskOptimizer.Settings
                        oldSettings.Musts = .Musts
                        oldSettings.MustNots = .MustNots
                    End With
                Else
                    oldBudget = scenario.Budget

                    With RA.Scenarios.ActiveScenario.Settings
                        'save current settions
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
                        If Not IsSingleScenarioChecked AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions Then
                            .Musts = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Musts
                            .MustNots = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.MustNots
                            .CustomConstraints = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.CustomConstraints
                            .Dependencies = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Dependencies
                            .Groups = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Groups
                            .FundingPools = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.FundingPools
                            .Risks = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.Risks
                            .TimePeriods = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.TimePeriods 'A1137
                        End If

                        'use scenario comparison settings - base case
                        If Not IsSingleScenarioChecked AndAlso RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions Then
                            .UseBaseCase = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCase
                            .BaseCaseForGroups = RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.BaseCaseForGroups
                        End If

                    End With
                End If
                RA.UpdateBenefits()

                ' store the Musts/MustNots
                Dim oldMustsList As New Dictionary(Of String, Boolean)
                Dim oldMustNotList As New Dictionary(Of String, Boolean)

                For Each alt0 As RAAlternative In Alts0
                    Dim alt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(alt0.ID)
                    If alt IsNot Nothing Then
                        oldMustsList.Add(alt.ID, alt.Must)
                        oldMustNotList.Add(alt.ID, alt.MustNot)
                        If tKeepFundedAlts AndAlso Not RA.Scenarios.ActiveScenario.Settings.Musts Then
                            If alt.Must Then alt.Must = False
                        End If
                        If tKeepFundedAlts Then
                            alt.Must = alt.TmpMust
                            'alt.MustNot = alt.TmpMustNot
                        End If
                    End If
                Next

                If tKeepFundedAlts Then RA.Scenarios.ActiveScenario.Settings.Musts = True

                Dim oldCCMaxValue As Double = UNDEFINED_INTEGER_VALUE
                Dim tCC As RAConstraint = Nothing

                If IsParameterCustomConstraint Then
                    scenario.Budget = If(PM.Parameters.EfficientFrontierUseUnlimitedBudgetForCC, scenario.GetMaxBudget, If(PM.IsRiskProject, RA.RiskOptimizer.BudgetLimit, RA.Solver.BudgetLimit))

                    If IsParameterRiskConstraint Then
                        RA.Scenarios.ActiveScenario.Settings.Risks = True ' Allow risks
                        PM.ResourceAligner.Solver.UseRiskLimit = True
                        PM.ResourceAligner.Solver.RiskLimitMax = curBudget
                    Else
                        ' allow custom constraints
                        RA.Scenarios.ActiveScenario.Settings.CustomConstraints = True
                        ' set value
                        tCC = scenario.Constraints.GetConstraintByID(tSelectedConstraintID)
                        If tCC IsNot Nothing Then
                            oldCCMaxValue = tCC.MaxValue
                            tCC.MaxValue = curBudget
                        End If
                    End If
                Else
                    ' Budget Limit
                    If PM.IsRiskProject Then
                        RA.RiskOptimizer.CurrentScenario.Budget = curBudget
                    Else
                        scenario.Budget = curBudget
                    End If
                End If

                RA.Solver.ResetSolver()
                'calculate Base Case
                Dim BaseCaseMax As Double = -1
                If scenario.Settings.UseBaseCase AndAlso scenario.Settings.BaseCaseForGroups Then BaseCaseMax = RA.Solver.GetBaseCaseMaximum Else BaseCaseMax = -1

                Dim fundedControls As List(Of Guid) = Nothing
                Dim fundedCost As Double = 0
                Dim tCellBenefit As Double = 0

                'solve
                RA.Solve()

                retVal.SolverState = RA.Solver.SolverState

                If Not scenario.Settings.UseBaseCase OrElse BaseCaseMax < 0 Then BaseCaseMax = RA.Solver.TotalBenefitOriginal 'A0918 + A0922

                If BaseCaseMax > 0 Then tCellBenefit = RA.Solver.FundedBenefit / BaseCaseMax 'A0918
                fundedCost = RA.Solver.FundedCost
                If tCC IsNot Nothing Then
                    'For Custom constraint paramenter funded cost Is number funded of alternatives
                    fundedCost = RA.Scenarios.ActiveScenario.Alternatives.Where(Function(a) a.Funded > 0).Count
                    'fundedCost = RA.Scenarios.ActiveScenario.Alternatives.Sum(Function(a) a.Funded)
                    'fundedCost = 0
                    For Each a As RAAlternative In RA.Scenarios.ActiveScenario.Alternatives
                        If a.Funded > 0 Then
                            Dim ccVal As Double = RA.Scenarios.ActiveScenario.Constraints.GetConstraintValue(tSelectedConstraintID, a.ID)
                            fundedCost += If(ccVal <> UNDEFINED_INTEGER_VALUE, a.Funded * ccVal, 0)
                        End If
                    Next
                End If

                'sover output data
                'Dim tSolved As Boolean = True ' Always True for Risk Optimization
                Dim tSolved As Boolean = tCellBenefit <> UNDEFINED_INTEGER_VALUE 'Risk Optimization
                If Not PM.IsRiskProject Then
                    tSolved = RA.Solver.SolverState = raSolverState.raSolved
                End If

                Dim sFundedAlts As String = ""
                Dim tFundedAltsArray As String = ""

                retVal.AlternativesData = New Dictionary(Of String, Double)
                retVal.FundedCost = fundedCost
                retVal.FundedBenefits = tCellBenefit 'RA.Solver.FundedBenefit
                retVal.Value = curBudget

                Dim Alts As List(Of RAAlternative) = RA.Scenarios.ActiveScenario.Alternatives.OrderBy(Function(r) r.ID).ToList()
                If Alts IsNot Nothing Then
                    For Each alt As RAAlternative In Alts
                        Dim sDP As String = "0"
                        If alt.DisplayFunded > 0 Then
                            'sDP = JS_SafeNumber(alt.DisplayFunded)
                            If tKeepFundedAlts AndAlso Not alt.TmpMust Then alt.TmpMust = True
                            'tFundedAltsArray += If(tFundedAltsArray = "", "", ",") + String.Format("[""{0}"",{1}]", alt.ID, JS_SafeNumber(alt.DisplayFunded))
                            retVal.AlternativesData.Add(alt.ID, alt.DisplayFunded)
                        End If
                        sFundedAlts += If(sFundedAlts = "", "", ",") + sDP
                    Next
                End If

                'retVal = String.Format("[{0}, {1}, {2}, {3}, ""{4}"",[{5}],{6}]", Bool2JS(tSolved), JS_SafeNumber(Math.Round(If(IsParameterCustomConstraint, curBudget, fundedCost), tDecimalDigitsCost)), JS_SafeNumber(Math.Round(tCellBenefit, tDecimalDigits)), JS_SafeNumber(Math.Round(curBudget, tDecimalDigitsCost)), sFundedAlts, tFundedAltsArray, scenarioID)
                'retVal = String.Format("[{0}, {1}, {2}, {3}, ""{4}"",[{5}],{6}]", Bool2JS(tSolved), JS_SafeNumber(Math.Round(fundedCost, tDecimalDigitsCost)), JS_SafeNumber(Math.Round(tCellBenefit, tDecimalDigits)), JS_SafeNumber(Math.Round(curBudget, tDecimalDigitsCost)), sFundedAlts, tFundedAltsArray, scenarioID)

                'restore the Budget Limit
                If PM.IsRiskProject Then
                    RA.RiskOptimizer.CurrentScenario.Budget = oldBudget
                Else
                    scenario.Budget = oldBudget
                End If

                If IsParameterCustomConstraint AndAlso tCC IsNot Nothing Then
                    tCC.MaxValue = oldCCMaxValue
                End If

                If IsParameterRiskConstraint Then
                    PM.ResourceAligner.Solver.UseRiskLimit = False
                End If

                With RA.Scenarios.ActiveScenario.Settings
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

                RA.UpdateBenefits()

                ' restore the Musts/MustNots
                For Each alt0 As RAAlternative In Alts0
                    Dim alt As RAAlternative = RA.Scenarios.ActiveScenario.GetAvailableAlternativeById(alt0.ID)
                    If alt IsNot Nothing Then
                        Dim tstoredMust As Boolean = oldMustsList(alt.ID)
                        Dim tstoredMustNot As Boolean = oldMustNotList(alt.ID)
                        If alt.Must <> tstoredMust Then alt.Must = tstoredMust
                        If alt.MustNot <> tstoredMustNot Then alt.MustNot = tstoredMustNot
                    End If
                Next
            End If

            If RA.Scenarios.ActiveScenarioID <> oldActiveScenarioID Then RA.Scenarios.ActiveScenarioID = oldActiveScenarioID
            If RA.RiskOptimizer.CurrentScenario.OptimizationType <> oldSolverMode Then RA.RiskOptimizer.CurrentScenario.OptimizationType = oldSolverMode
            RA.Solver.ResetSolver()
        End Sub
        'A1708 ==

    End Class
End Namespace