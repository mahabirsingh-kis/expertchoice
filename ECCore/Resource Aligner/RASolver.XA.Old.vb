Option Strict On

Imports ECCore
Imports Canvas
Imports Optimizer
Imports System.Linq

Namespace Canvas
    Partial Public Class RASolver

        Private Function Solve_XA_Old(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            'Dim model As New Optimizer.XA.Optimize(100000)  ' Initialize XA
            Dim model As New Optimizer.XA.Optimize(100000, 0, 0, 0, 100, 100)  ' Initialize XA
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
                    model.setColumnObjective(alt.IDVar, adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)))
                Next

                'Make decision variable alt1,alt2, .... binary
                For Each alt As RAAlternative In Alternatives
                    If alt.IsPartial Then
                        model.setColumnSemiContinuous(alt.IDVar, alt.MinPercent, 1)
                    Else
                        model.setColumnBinary(alt.IDVar)
                    End If
                Next

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                model.setColumnObjective("FP" + FP.ID.ToString + "_" + alt.IDVar, 0)
                                Dim cellCost As Double = FP.GetAlternativeValue(alt.IDVar)
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.IDVar, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.IDVar)
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            For k As Integer = 0 To apd.Duration - 1
                                model.setColumnObjective("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, 0)
                                model.setColumnBinary("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString)

                                'model.setColumnObjective("TPVar_" + alt.IDVar.ToString + j.ToString, 0)
                                'model.setColumnBinary("TPVar_" + alt.IDVar.ToString + j.ToString)
                            Next
                            i += 1
                        Next
                    Next

                    'Dim t As Integer = 0
                    'For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                    '    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                    '        If resource.Enabled Then 'A1130
                    '            'If resource.IDVar.Equals(RA_Cost_GUID) Then    ' D3918
                    '            Dim minValue As Double = period.GetResourceMinValue(resource.IDVar)  ' D3918
                    '            Dim maxValue As Double = period.GetResourceMaxValue(resource.IDVar)  ' D3918

                    '            ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                    '            'If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                    '            '    model.setColumnObjective("TPResY_" + period.IDVar.ToString + "_" + resource.IDVar.ToString, 0)
                    '            '    model.setColumnBinary("TPResY_" + period.IDVar.ToString + "_" + resource.IDVar.ToString)
                    '            'End If
                    '            'End If
                    '        End If
                    '    Next
                    '    t += 1
                    'Next
                End If

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.IDVar, alt.Cost)
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
                        model.loadPoint("Must", alt.IDVar, CDbl(If(alt.Must, 1, 0)))
                    Next
                    model.setRowFix("Must", mustCount)
                End If

                If Settings.MustNots And (mustnotCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        model.loadPoint("MustNot", alt.IDVar, CDbl(If(alt.MustNot, 1, 0)))
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
                                'model.loadPoint(gName, alt.IDVar, CDbl(If(group.Alternatives.ContainsKey(alt.IDVar), 1, 0)))
                                model.loadPoint(gName, alt.IDVar, CDbl(If(filteredAlts.FirstOrDefault(Function(p) (p.IDVar.ToLower = alt.IDVar.ToLower)) IsNot Nothing, 1, 0)))
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
                                        model.loadPoint(dName, alt.IDVar, CDbl(If(alt.IDVar = Dependency.SecondAlternativeID, 1, 0)) - CDbl(If(alt.IDVar = Dependency.FirstAlternativeID, 1, 0)))
                                        model.setRowMin(dName, 0)
                                        'dTerm = model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p) - (Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p)))) >= 0
                                    Case RADependencyType.dtMutuallyDependent
                                        dName = "Dependency" + i.ToString + "_MutuallyDependent"
                                        model.loadPoint(dName, alt.IDVar, CDbl(If(alt.IDVar = Dependency.FirstAlternativeID, 1, 0)) - CDbl(If(alt.IDVar = Dependency.SecondAlternativeID, 1, 0)))
                                        model.setRowFix(dName, 0)
                                        'dTerm = model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) - (Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) = 0
                                    Case RADependencyType.dtMutuallyExclusive
                                        dName = "Dependency" + i.ToString + "_MutuallyExclusive"
                                        model.loadPoint(dName, alt.IDVar, CDbl(If(alt.IDVar = Dependency.FirstAlternativeID, 1, 0)) + CDbl(If(alt.IDVar = Dependency.SecondAlternativeID, 1, 0)))
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
                                Dim value As Double = Constraint.GetAlternativeValue(alt.IDVar)
                                If value <> UNDEFINED_INTEGER_VALUE Then
                                    model.loadPoint(cName, alt.IDVar, value)
                                End If
                                'model.loadPoint(cName, alt.IDVar, CDbl(If(value = UNDEFINED_INTEGER_VALUE, 0, value)))
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

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                model.loadPoint("FProw_" + alt.IDVar, "FP" + FP.ID.ToString + "_" + alt.IDVar, 1)
                            End If
                        Next
                        model.loadPoint("FProw_" + alt.IDVar, alt.IDVar, -alt.Cost)
                        model.setRowFix("FProw_" + alt.IDVar, 0)
                    Next

                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            For Each alt As RAAlternative In Alternatives
                                model.loadPoint("FProw_" + FP.ID.ToString, "FP" + FP.ID.ToString + "_" + alt.IDVar, 1)
                            Next
                            model.setRowMax("FProw_" + FP.ID.ToString, FP.PoolLimit)
                        End If
                    Next
                End If

                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.TimePeriods Then
                    Dim TPRowVariantCount As Integer = 0
                    Dim TPRowCount As Integer = 0
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        model.loadPoint("TPVariantNew_" + alt.IDVar.ToString, alt.IDVar.ToString, -1)

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.IDVar)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                'model.setColumnObjective("TPVar_" + alt.IDVar.ToString + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                            End If
                            v += 1

                            model.loadPoint("TPVariantNew_" + alt.IDVar.ToString, "TP" + i.ToString + "_" + j.ToString + "_" + alt.IDVar.ToString, 1)
                            'model.loadPoint("TPRowVariant_" + alt.IDVar.ToString, "TPVar_" + alt.IDVar.ToString + j.ToString, 1)

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.IDVar, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.IDVar, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.IDVar)
                                            End If
                                            'model.loadPoint("TPRow_" + alt.IDVar + "_" + resource.IDVar.ToString + "_" + j.ToString, "TPVar_" + alt.IDVar.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.IDVar + "_" + resource.IDVar.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, c)

                                            'model.loadPoint("TPRow_" + alt.IDVar + "_" + resource.IDVar.ToString + "_" + j.ToString, "TPVar_" + alt.IDVar.ToString + j.ToString, -apd.Duration)
                                            model.loadPoint("TPRow_" + alt.IDVar + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, CDbl(If(k = 0, (1 - apd.Duration), 1)))

                                            If j = 0 Then
                                                'model.loadPoint("TPVariantNew_" + alt.IDVar.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, 1)
                                            End If


                                            ' D3918 ===
                                            'If resource.IDVar.Equals(RA_Cost_GUID) Then
                                            model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, c)


                                            'model.loadPoint("TPRes1" + (j + k).ToString + "_" + resource.IDVar.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, c)
                                            'model.loadPoint("TPRes2" + (j + k).ToString + "_" + resource.IDVar.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, c)

                                            ' D3918 ==
                                            ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                                            'For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                                            '    'If resource.IDVar.Equals(RA_Cost_GUID) AndAlso period.IDVar = j + k Then  ' D3918
                                            '    If period.IDVar = j + k Then  ' D3918
                                            '        Dim minValue As Double = period.GetResourceMinValue(resource.IDVar)      ' D3918
                                            '        Dim maxValue As Double = period.GetResourceMaxValue(resource.IDVar)      ' D3918

                                            '        If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                            '            ' D3918 ===
                                            '            model.loadPoint("TPRes1" + (j + k).ToString + "_" + resource.IDVar.ToString, "TPResY_" + period.IDVar.ToString + "_" + resource.IDVar.ToString, -minValue)
                                            '            model.loadPoint("TPRes2" + (j + k).ToString + "_" + resource.IDVar.ToString, "TPResY_" + period.IDVar.ToString + "_" + resource.IDVar.ToString, -maxValue)

                                            '            model.setRowMin("TPRes1" + (j + k).ToString + "_" + resource.IDVar.ToString, 0)
                                            '            model.setRowMax("TPRes2" + (j + k).ToString + "_" + resource.IDVar.ToString, 0)
                                            '            ' D3918 ==
                                            '        End If
                                            '    End If
                                            'Next
                                            'End If
                                            model.setRowFix("TPRow_" + alt.IDVar + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            TPRowCount += 1

                                        End If
                                    Next
                                End If

                                If k = 0 Then
                                    'model.loadPoint("TPR_ME_" + alt.IDVar.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.IDVar.ToString, 1)
                                End If
                            Next
                            '--- model.loadPoint("TPRow" + i.ToString, "TPVar_" + alt.IDVar.ToString + j.ToString, -alt.Cost)
                            '---model.setRowFix("TPRow" + i.ToString, 0)
                            i += 1
                        Next
                        model.setRowFix("TPVariantNew_" + alt.IDVar.ToString, 0)
                        'model.setRowFix("TPRowVariant_" + alt.IDVar.ToString, 0)
                        'TPRowVariantCount += 1
                    Next

                    'Debug.Print("TPRowVariantCount = " + TPRowVariantCount.ToString)
                    'Debug.Print("TPRowCount = " + TPRowCount.ToString)

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                'If resource.IDVar.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
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
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + j.ToString, j + 1)
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + j.ToString, -(j + 1))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
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
                    ' D3488 ===
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                    ' D3488 ==
                End If

                'model.setCommand("Presolve  0 strategy 6")
                'model.setCommand("strategy 6    StopUnchange  1:00  StopAfter  2:00 ")
                Dim command As String = XACommand
                model.setCommand(command)
                'model.solveWithInfeasibleAnalysis()
                model.solve()

                'Debug.Print(model.getSolverStatus.ToString)

                Select Case model.getModelStatus
                    Case 1, 2
                        SolverState = raSolverState.raSolved


                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.IDVar).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.IDVar, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.IDVar))
                                    End If
                                Next
                                'Debug.Print(s)
                            Next

                            'Debug.Print(vbNewLine)
                            'Debug.Print("--- FUNDING POOLS VALUES ---")
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.IDVar).ToString + "; "
                                    End If
                                Next
                                'Debug.Print(s)
                            Next
                        End If

                        mFundedCost = 0
                        mFundedBenefit = 0
                        mFundedOriginalBenefit = 0

                        CurrentScenario.TimePeriods.TimePeriodResults.Clear()

                        Dim i As Integer = 1
                        For Each alt As RAAlternative In Alternatives
                            alt.Funded = model.getColumnPrimalActivity(alt.IDVar)

                            Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.IDVar)
                            If alt.Funded > 0.00001 Then
                                mFundedCost += alt.Cost * alt.Funded

                                Dim dFactorPower As Integer = 0
                                Dim discount As Double = 1
                                If Settings.TimePeriods Then
                                    Dim fundedPeriod As Integer = -1
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        ''Debug.Print("TP" + i.ToString + "_" + j.ToString + "_" + alt.IDVar.ToString + " = " + model.getColumnPrimalActivity("TP" + i.ToString + "_" + j.ToString + "_" + alt.IDVar.ToString).ToString)
                                        'Debug.Print("TPVar_" + alt.IDVar.ToString + j.ToString + " = " + model.getColumnPrimalActivity("TPVar_" + alt.IDVar.ToString + j.ToString).ToString)

                                        If fundedPeriod = -1 AndAlso model.getColumnPrimalActivity("TPVar_" + alt.IDVar.ToString + j.ToString) > 0 Then
                                            fundedPeriod = j
                                        End If
                                        i += 1
                                    Next
                                    If fundedPeriod <> -1 Then
                                        CurrentScenario.TimePeriods.TimePeriodResults.Add(alt.IDVar, fundedPeriod)
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
                'Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return True
        End Function

        Private Function Solve_XA3(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            'Dim model As New Optimizer.XA.Optimize(100000)  ' Initialize XA
            Dim model As New Optimizer.XA.Optimize(100000, 0, 0, 0, 100, 100)  ' Initialize XA
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
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            For k As Integer = 0 To apd.Duration - 1
                                model.setColumnObjective("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, 0)
                                model.setColumnBinary("TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString)

                                model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, 0)
                                model.setColumnBinary("TPVar_" + alt.ID.ToString + j.ToString)
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

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

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
                                'model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
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

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
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

                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.TimePeriods Then
                    Dim TPRowVariantCount As Integer = 0
                    Dim TPRowCount As Integer = 0
                    Dim i As Integer = 1
                    For Each alt As RAAlternative In Alternatives
                        model.loadPoint("TPRowVariant_" + alt.ID.ToString, alt.ID.ToString, -1)

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        Dim v As Integer = 0
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            If CurrentScenario.TimePeriods.UseDiscountFactor Then
                                model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                            End If
                            v += 1

                            model.loadPoint("TPRowVariant_" + alt.ID.ToString, "TPVar_" + alt.ID.ToString + j.ToString, 1)

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                                            End If
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -apd.Duration)
                                            model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, 1)


                                            ' D3918 ===
                                            'If resource.ID.Equals(RA_Cost_GUID) Then
                                            model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)


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
                                            model.setRowFix("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            TPRowCount += 1

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
                        model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        TPRowVariantCount += 1
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
                                    model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
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
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + j.ToString, j + 1)
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + j.ToString, -(j + 1))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
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
                    ' D3488 ===
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                    ' D3488 ==
                End If

                'model.setCommand("Presolve  0 strategy 6")
                'model.setCommand("strategy 6    StopUnchange  1:00  StopAfter  2:00 ")
                Dim command As String = XACommand
                model.setCommand(command)
                'model.solveWithInfeasibleAnalysis()
                model.solve()

                'Debug.Print(model.getSolverStatus.ToString)

                Select Case model.getModelStatus
                    Case 1, 2
                        SolverState = raSolverState.raSolved


                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                    End If
                                Next
                                'Debug.Print(s)
                            Next

                            'Debug.Print(vbNewLine)
                            'Debug.Print("--- FUNDING POOLS VALUES ---")
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                                    End If
                                Next
                                'Debug.Print(s)
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
                                        ''Debug.Print("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString + " = " + model.getColumnPrimalActivity("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString).ToString)
                                        'Debug.Print("TPVar_" + alt.ID.ToString + j.ToString + " = " + model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + j.ToString).ToString)

                                        If fundedPeriod = -1 AndAlso model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + j.ToString) > 0 Then
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
                'Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return True
        End Function

        Private Function Solve_XA4(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            'Dim model As New Optimizer.XA.Optimize(100000)  ' Initialize XA
            Dim model As New Optimizer.XA.Optimize(0, 0, 0, 0, 100, 100)  ' Initialize XA
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
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    For Each alt As RAAlternative In Alternatives
                        Dim i As Integer = 0
                        For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                            model.setColumnObjective("TP_" + alt.ID.ToString + "_" + i.ToString, 0)
                            model.setColumnBinary("TP_" + alt.ID.ToString + "_" + i.ToString)
                            i += 1
                        Next

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            model.setColumnObjective("TPVar_" + alt.ID.ToString + "_" + j.ToString, 0)
                            model.setColumnBinary("TPVar_" + alt.ID.ToString + "_" + j.ToString)
                        Next
                    Next

                    'Dim t As Integer = 0
                    'For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                    '    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                    '        If resource.Enabled Then 'A1130
                    '            'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                    '            Dim minValue As Double = period.GetResourceMinValue(resource.ID)  ' D3918
                    '            Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)  ' D3918

                    '            ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                    '            'If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                    '            '    model.setColumnObjective("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString, 0)
                    '            '    model.setColumnBinary("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString)
                    '            'End If
                    '            'End If
                    '        End If
                    '    Next
                    '    t += 1
                    'Next
                End If

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

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
                                'model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
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

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
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

                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.TimePeriods Then
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
                                model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                            End If
                            v += 1

                            model.loadPoint("TPRowVariant_" + alt.ID.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)

                            For tp As Integer = apd.GetMinPeriod To apd.GetMaxPeriod + apd.Duration - 1
                                If tp < j Or tp > j + apd.Duration - 1 Then
                                    model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)
                                    model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TP_" + alt.ID.ToString + "_" + tp.ToString, 1)
                                    model.setRowMax("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, 1)
                                End If
                            Next

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                                            End If
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, -value)
                                            model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            ' D3918 ===
                                            'If resource.ID.Equals(RA_Cost_GUID) Then

                                            ''model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                            ''model.loadPoint("TPRes_" + resource.ID.ToString + "_" + (j + k).ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            model.setRowMin("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            TPRowCount += 1
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
                        model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        TPRowVariantCount += 1
                        altPos += 1
                    Next

                    'Debug.Print("TPRowVariantCount = " + TPRowVariantCount.ToString)
                    'Debug.Print("TPRowCount = " + TPRowCount.ToString)

                    Dim pSum() As Double
                    ReDim pSum(CurrentScenario.TimePeriods.Periods.Count - 1)
                    For Each x As Double In pSum
                        x = 0
                    Next

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            For Each alt As RAAlternative In Alternatives
                                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - j, alt.ID, resource.ID)
                                        If c <> UNDEFINED_INTEGER_VALUE Then
                                            model.setColumnObjective("TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 0)
                                            model.setColumnBinary("TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString)

                                            model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, c)

                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", "TP_" + alt.ID.ToString + "_" + t.ToString, -1)
                                            model.setRowMax("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", 0)

                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", "TPVar_" + alt.ID.ToString + "_" + j.ToString, -1)
                                            model.setRowMax("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", 0)

                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TP_" + alt.ID.ToString + "_" + t.ToString, -1)
                                            model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TPVar_" + alt.ID.ToString + "_" + j.ToString, -1)
                                            model.setRowMin("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", -1)

                                            pSum(t) += c
                                            'model.loadPoint("TPRes_" + resource.ID.ToString + "_" + t.ToString, "TP_" + alt.ID.ToString + "_" + t.ToString, pSum(t))
                                            'model.loadPoint("TPRes_" + resource.ID.ToString + "_" + t.ToString, "TP_" + alt.ID.ToString + "_" + t.ToString, c)
                                        End If
                                    Next
                                End If
                            Next

                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                    'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                    model.setRowMinMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue, maxValue)  ' D3918
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, maxValue)  ' D3918
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
                                    model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue)  ' D3918
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
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + j.ToString, j + 1)
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + j.ToString, -(j + 1))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
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
                    ' D3488 ===
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                    ' D3488 ==
                End If

                'model.setCommand("Presolve  0 strategy 6")
                'model.setCommand("strategy 6    StopUnchange  1:00  StopAfter  2:00 ")
                Dim command As String = XACommand
                model.setCommand(command)
                'model.solveWithInfeasibleAnalysis()
                model.solve()

                'Debug.Print(model.getSolverStatus.ToString)

                Select Case model.getModelStatus
                    Case 1, 2
                        SolverState = raSolverState.raSolved


                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                    End If
                                Next
                                'Debug.Print(s)
                            Next

                            'Debug.Print(vbNewLine)
                            'Debug.Print("--- FUNDING POOLS VALUES ---")
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                                    End If
                                Next
                                'Debug.Print(s)
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
                                        ''Debug.Print("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString + " = " + model.getColumnPrimalActivity("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString).ToString)
                                        'Debug.Print("TPVar_" + alt.ID.ToString + "_" + j.ToString + " = " + model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + "_" + j.ToString).ToString)

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
                'Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return True
        End Function

        Private Function Solve_XA5(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            'Dim model As New Optimizer.XA.Optimize(100000)  ' Initialize XA
            Dim model As New Optimizer.XA.Optimize(0, 0, 0, 0, 100, 100)  ' Initialize XA
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
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    For Each alt As RAAlternative In Alternatives
                        Dim i As Integer = 0
                        For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                            model.setColumnObjective("TP_" + alt.ID.ToString + "_" + i.ToString, 0)
                            model.setColumnBinary("TP_" + alt.ID.ToString + "_" + i.ToString)
                            i += 1
                        Next

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            model.setColumnObjective("TPVar_" + alt.ID.ToString + "_" + j.ToString, 0)
                            model.setColumnBinary("TPVar_" + alt.ID.ToString + "_" + j.ToString)
                        Next
                    Next

                    'Dim t As Integer = 0
                    'For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                    '    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                    '        If resource.Enabled Then 'A1130
                    '            'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                    '            Dim minValue As Double = period.GetResourceMinValue(resource.ID)  ' D3918
                    '            Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)  ' D3918

                    '            ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                    '            'If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                    '            '    model.setColumnObjective("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString, 0)
                    '            '    model.setColumnBinary("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString)
                    '            'End If
                    '            'End If
                    '        End If
                    '    Next
                    '    t += 1
                    'Next
                End If

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

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
                                'model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
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

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
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

                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.TimePeriods Then
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
                                model.setColumnObjective("TPVar_" + alt.ID.ToString + j.ToString, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * (1 - CurrentScenario.TimePeriods.DiscountFactor * v))
                            End If
                            v += 1

                            model.loadPoint("TPRowVariant_" + alt.ID.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)

                            'For tp As Integer = apd.GetMinPeriod To apd.GetMaxPeriod + apd.Duration - 1
                            '    If tp < j Or tp > j + apd.Duration - 1
                            '        model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)
                            '        model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TP_" + alt.ID.ToString + "_" + tp.ToString, 1)
                            '        model.setRowMax("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, 1)
                            '    End If
                            'Next

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                                            End If
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, -value)
                                            model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            ' D3918 ===
                                            'If resource.ID.Equals(RA_Cost_GUID) Then

                                            ''model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                            ''model.loadPoint("TPRes_" + resource.ID.ToString + "_" + (j + k).ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            model.setRowMin("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            TPRowCount += 1
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
                        model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        TPRowVariantCount += 1
                        altPos += 1
                    Next

                    'Debug.Print("TPRowVariantCount = " + TPRowVariantCount.ToString)
                    'Debug.Print("TPRowCount = " + TPRowCount.ToString)

                    Dim pSum() As Double
                    ReDim pSum(CurrentScenario.TimePeriods.Periods.Count - 1)
                    For Each x As Double In pSum
                        x = 0
                    Next

                    Dim t As Integer = 0
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            For Each alt As RAAlternative In Alternatives
                                Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                                If t >= apd.MinPeriod AndAlso t <= apd.MaxPeriod + apd.Duration - 1 Then
                                    For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                                        Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(t - j, alt.ID, resource.ID)
                                        If c <> UNDEFINED_INTEGER_VALUE Then
                                            'model.setColumnObjective("TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 0)
                                            'model.setColumnBinary("TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString)

                                            'model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, c)
                                            model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPVar_" + alt.ID + "_" + j.ToString, c)

                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", "TP_" + alt.ID.ToString + "_" + t.ToString, -1)
                                            'model.setRowMax("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", 0)

                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", "TPVar_" + alt.ID.ToString + "_" + j.ToString, -1)
                                            'model.setRowMax("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", 0)

                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TP_" + alt.ID.ToString + "_" + t.ToString, -1)
                                            'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TPVar_" + alt.ID.ToString + "_" + j.ToString, -1)
                                            'model.setRowMin("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", -1)

                                            pSum(t) += c
                                            'model.loadPoint("TPRes_" + resource.ID.ToString + "_" + t.ToString, "TP_" + alt.ID.ToString + "_" + t.ToString, pSum(t))
                                            'model.loadPoint("TPRes_" + resource.ID.ToString + "_" + t.ToString, "TP_" + alt.ID.ToString + "_" + t.ToString, c)
                                        End If
                                    Next
                                End If
                            Next

                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                    'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                    model.setRowMinMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue, maxValue)  ' D3918
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, maxValue)  ' D3918
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
                                    model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue)  ' D3918
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
                                Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + j.ToString, j + 1)
                                Next
                                For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                    model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + j.ToString, -(j + 1))
                                Next
                                Select Case dependency.Value
                                    Case RADependencyType.dtSuccessive
                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
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
                    ' D3488 ===
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    'model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_mps_model    ToMPS Yes", OutputPath))
                    ' D3488 ==
                End If

                'model.setCommand("Presolve  0 strategy 6")
                'model.setCommand("strategy 6    StopUnchange  1:00  StopAfter  2:00 ")
                Dim command As String = XACommand
                model.setCommand(command)
                'model.solveWithInfeasibleAnalysis()
                model.solve()

                'Debug.Print(model.getSolverStatus.ToString)

                Select Case model.getModelStatus
                    Case 1, 2
                        SolverState = raSolverState.raSolved


                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                    End If
                                Next
                                'Debug.Print(s)
                            Next

                            'Debug.Print(vbNewLine)
                            'Debug.Print("--- FUNDING POOLS VALUES ---")
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                                    End If
                                Next
                                'Debug.Print(s)
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
                                        ''Debug.Print("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString + " = " + model.getColumnPrimalActivity("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString).ToString)
                                        'Debug.Print("TPVar_" + alt.ID.ToString + "_" + j.ToString + " = " + model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + "_" + j.ToString).ToString)

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
                'Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return True
        End Function

        Private Function Solve_XA_MinCost(Optional OutputPath As String = "") As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            'Dim model As New Optimizer.XA.Optimize(100000)  ' Initialize XA
            Dim model As New Optimizer.XA.Optimize(100000, 0, 0, 0, 100, 100)  ' Initialize XA
            model.setActivationCodes(Reserve1, Reserve2)    ' Activation codes
            model.setXAMessageWindowOff()                   ' Remove XA Message Window

            Try
                model.openConnection()                          ' Connect  

                model.setMaximizeObjective()                    ' Maximize the objective   

                Dim maxCost As Double = 0
                Dim maxBenefit As Double = 0
                Dim sumCost As Double = 0
                Dim sumBenefit As Double = 0
                Dim eps As Double = 0.00001
                For Each alt As RAAlternative In Alternatives
                    If alt.Cost > maxCost Then maxCost = alt.Cost
                    If alt.BenefitOriginal > maxBenefit Then maxBenefit = alt.BenefitOriginal
                    sumCost += alt.Cost
                    sumBenefit += alt.BenefitOriginal
                Next

                Dim adjustValue As Double = -eps ' because approach with subtracting -0.00001 * maxCost from benefits to find optimal cost is not working

                model.setColumnObjective("x1", 1)
                model.setColumnObjective("x2", -1)

                'model.setColumnObjective("x1", 0)
                'model.setColumnObjective("x2", 1)

                ' objective coefficient for decision variables
                For Each alt As RAAlternative In Alternatives
                    'model.setColumnObjective(alt.ID, adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal)))
                    model.setColumnObjective(alt.ID, 0)
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
                                'model.setColumnObjective("FP" + FP.ID.ToString + "_" + alt.ID, 1)
                                model.setColumnObjective("FP" + FP.ID.ToString + "_" + alt.ID, 0)
                                Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                ''model.setColumnMinMax("FP" + FP.ID.ToString + "_" + alt.ID, 0.0, cellCost)
                                'model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If


                For Each alt As RAAlternative In Alternatives
                    model.loadPoint("c_x1", alt.ID, CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)))
                    model.loadPoint("c_x1", "x1", -1)
                Next
                model.setRowFix("c_x1", 0)


                For Each alt As RAAlternative In Alternatives
                    If alt.Cost <> 0 Then
                        adjustValue = -1 / (100000 * alt.Cost)
                    Else
                        adjustValue = 0
                    End If
                    model.loadPoint("c_x2", alt.ID, adjustValue + CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)))
                    model.loadPoint("c_x2", "x2", -1)
                Next
                model.setRowFix("c_x2", 0)


                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

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
                    For Each group As RAGroup In Groups.Groups.Values
                        If group.Enabled Then
                            Dim gName As String = "Group_" + i.ToString
                            For Each alt As RAAlternative In Alternatives
                                model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
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
                            End Select
                            i += 1
                        End If
                    Next
                End If

                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In Dependencies.Dependencies
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

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
                    'For Each alt As RAAlternative In Alternatives
                    '    For Each FP As RAFundingPool In FundingPools.Pools.Values
                    '        If FP.Enabled Then
                    '            model.setColumnObjective("FP" + FP.ID.ToString + "_" + alt.ID, 0)
                    '            Dim cellCost As Double = FP.GetAlternativeValue(alt.ID)
                    '            If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                    '            model.setColumnMinMax("FP" + FP.ID.ToString + "_" + alt.ID, 0.0, cellCost)
                    '        End If
                    '    Next
                    'Next

                    For Each alt As RAAlternative In Alternatives
                        For Each FP As RAFundingPool In FundingPools.Pools.Values
                            If FP.Enabled Then
                                model.loadPoint("FProw_" + alt.ID, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                            End If
                            'model.setRowFix("FProw_" + alt.ID, 0)
                            'model.setRowMax("FProw_" + alt.ID, alt.Cost)
                        Next
                        model.loadPoint("FProw_" + alt.ID, alt.ID, -alt.Cost)
                        model.setRowFix("FProw_" + alt.ID, 0)
                        'model.setRowMax("FProw_" + alt.ID, alt.Cost)
                    Next

                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            For Each alt As RAAlternative In Alternatives
                                model.loadPoint("FProw_" + FP.ID.ToString, "FP" + FP.ID.ToString + "_" + alt.ID, 1)
                                'model.setRowMax("FProw_" + FP.ID.ToString, FP.PoolLimit)
                            Next
                            model.setRowMax("FProw_" + FP.ID.ToString, FP.PoolLimit)
                        End If
                    Next
                End If

                If OutputPath <> "" Then
                    ' D3488 ===
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                    ' D3488 ==
                End If

                'model.setCommand("Presolve  0")
                model.solve()

                Select Case model.getModelStatus
                    Case 1
                        SolverState = raSolverState.raSolved


                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                    End If
                                Next
                                'Debug.Print(s)
                            Next

                            'Debug.Print(vbNewLine)
                            'Debug.Print("--- FUNDING POOLS VALUES ---")
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                                    End If
                                Next
                                'Debug.Print(s)
                            Next
                        End If

                        mFundedCost = 0
                        mFundedBenefit = 0
                        mFundedOriginalBenefit = 0

                        CurrentScenario.TimePeriods.TimePeriodResults.Clear()

                        For Each alt As RAAlternative In Alternatives
                            alt.Funded = model.getColumnPrimalActivity(alt.ID)

                            If alt.Funded > 0.00001 Then
                                mFundedCost += alt.Cost * alt.Funded
                                mFundedBenefit += CDbl(If(Settings.Risks, alt.BenefitOriginal * (1 - alt.RiskOriginal), alt.BenefitOriginal)) * alt.Funded
                                mFundedOriginalBenefit += alt.BenefitOriginal * alt.Funded

                                CurrentScenario.TimePeriods.TimePeriodResults.Add(alt.ID, CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID).StartPeriod)
                            Else
                                alt.Funded = 0
                            End If
                        Next
                    Case 4
                        SolverState = raSolverState.raInfeasible
                End Select
                LastError = ""
            Catch ex As Exception
                'Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return True
        End Function

        Public Function GetInfeasibilityInfo(constraintsList As List(Of ConstraintInfo)) As String
            If ProjectManager Is Nothing Then Return ""
            'ResetFunded()

            Dim result As String = ""
            Dim ConstraintsCount As Long = constraintsList.LongCount(Function(c) c.Enabled)

            Dim cList As New List(Of ConstraintInfo)

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            Dim model As New Optimizer.XA.Optimize(0, 0, 0, 0, 100, 100)  ' Initialize XA
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
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
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

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0
                For Each alt As RAAlternative In Alternatives
                    If alt.Must Then mustCount += 1
                    If alt.MustNot Then mustnotCount += 1
                Next

                If Settings.Musts And (mustCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        If alt.Must Then
                            Dim cMustID As String = "C_Must_" + alt.ID
                            If constraintsList.FirstOrDefault(Function(c) (c.ID = cMustID)) IsNot Nothing AndAlso
                                constraintsList.First(Function(c) (c.ID = cMustID)).Enabled Then

                                model.setColumnObjective(cMustID, 0)
                                model.setColumnBinary(cMustID)

                                model.setColumnObjective("C_Z_Must_" + alt.ID, 0)
                                model.setColumnBinary("C_Z_Must_" + alt.ID)

                                model.loadPoint("Constraint_Must_" + alt.ID, "C_Z_Must_" + alt.ID, 1)
                                model.loadPoint("Constraint_Must_" + alt.ID, cMustID, -1)
                                model.setRowFix("Constraint_Must_" + alt.ID, 0)

                                Linearize(model, "C_Z_Must_" + alt.ID, cMustID, alt.ID)

                                model.loadPoint("TotalConstraints", cMustID, 1)
                            Else
                                model.loadPoint("Must_" + alt.ID, alt.ID, 1)
                                model.setRowFix("Must_" + alt.ID, 1)
                            End If
                        End If
                    Next
                End If

                If Settings.MustNots And (mustnotCount > 0) Then
                    For Each alt As RAAlternative In Alternatives
                        If alt.MustNot Then
                            Dim cMustNotID As String = "C_MustNot_" + alt.ID
                            If constraintsList.FirstOrDefault(Function(c) (c.ID = cMustNotID)) IsNot Nothing AndAlso
                                constraintsList.First(Function(c) (c.ID = cMustNotID)).Enabled Then

                                model.setColumnObjective(cMustNotID, 0)
                                model.setColumnBinary(cMustNotID)

                                model.setColumnObjective("C_Z_MustNot_" + alt.ID, 0)
                                model.setColumnBinary("C_Z_MustNot_" + alt.ID)

                                model.loadPoint("Constraint_MustNot_" + alt.ID, "C_Z_MustNot_" + alt.ID, 1)
                                model.setRowFix("Constraint_MustNot_" + alt.ID, 0)

                                Linearize(model, "C_Z_MustNot_" + alt.ID, cMustNotID, alt.ID)

                                model.loadPoint("TotalConstraints", cMustNotID, 1)
                            Else
                                model.loadPoint("MustNot_" + alt.ID, alt.ID, 1)
                                model.setRowFix("MustNot_" + alt.ID, 0)
                            End If
                        End If
                    Next
                End If

                If Settings.Groups Then
                    Dim i As Integer = 1
                    For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                        'For Each group As RAGroup In Groups.Groups.Values
                        Dim filteredAlts As List(Of RAAlternative) = group.GetFilteredAlternatives(CurrentScenario.Alternatives)
                        If group.Enabled And filteredAlts.Count > 1 Then
                            Dim gCID As String = "C_Group_" + group.ID.ToString
                            Dim gName As String = "Group_" + i.ToString
                            If constraintsList.FirstOrDefault(Function(c) (c.ID = gCID)) IsNot Nothing AndAlso
                                constraintsList.First(Function(c) (c.ID = gCID)).Enabled Then

                                model.setColumnObjective(gCID, 0)
                                model.setColumnBinary(gCID)

                                For Each alt As RAAlternative In group.Alternatives.Values
                                    Dim czgAlt As String = "C_Z_Group_" + group.ID + "_" + alt.ID
                                    model.setColumnObjective(czgAlt, 0)
                                    model.setColumnBinary(czgAlt)

                                    model.loadPoint(gName, czgAlt, CDbl(If(filteredAlts.FirstOrDefault(Function(p) (p.ID.ToLower = alt.ID.ToLower)) IsNot Nothing, 1, 0)))

                                    Linearize(model, czgAlt, gCID, alt.ID)

                                    'model.loadPoint(gName, alt.ID, CDbl(If(filteredAlts.FirstOrDefault(Function(p) (p.ID.ToLower = alt.ID.ToLower)) IsNot Nothing, 1, 0)))
                                Next

                                model.loadPoint(gName, gCID, -1)
                                Select Case group.Condition
                                    Case RAGroupCondition.gcLessOrEqualsOne
                                        ' none of one alterantive from the group if funded
                                        'model.setRowMax(gName, 1)
                                        model.setRowMax(gName, 0)
                                    Case RAGroupCondition.gcEqualsOne
                                        ' exactly one alterantive from the group is funded
                                        'model.setRowFix(gName, 1)
                                        model.setRowFix(gName, 0)
                                    Case RAGroupCondition.gcGreaterOrEqualsOne
                                        ' at least one alternative from the group is funded
                                        'model.setRowMin(gName, 1)
                                        model.setRowMin(gName, 0)
                                End Select

                                model.loadPoint("TotalConstraints", gCID, 1)
                            Else
                                For Each alt As RAAlternative In Alternatives
                                    'model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
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
                                End Select
                            End If
                            i += 1
                        End If
                    Next
                End If

                If Settings.Dependencies Then
                    Dim i As Integer = 1
                    For Each Dependency As RADependency In CurrentScenario.GetAvailableDependencies
                        If Dependency.Enabled Then
                            Dim dCID As String = "C_Dependency_" + i.ToString
                            If constraintsList.FirstOrDefault(Function(c) (c.ID = dCID)) IsNot Nothing AndAlso
                                constraintsList.First(Function(c) (c.ID = dCID)).Enabled Then

                                model.setColumnObjective(dCID, 0)
                                model.setColumnBinary(dCID)

                                Dim czdAlt1 As String = "C_Z_Dependency_" + i.ToString + "_" + Dependency.FirstAlternativeID
                                model.setColumnObjective(czdAlt1, 0)
                                model.setColumnBinary(czdAlt1)
                                Linearize(model, czdAlt1, dCID, Dependency.FirstAlternativeID)

                                Dim czdAlt2 As String = "C_Z_Dependency_" + i.ToString + "_" + Dependency.SecondAlternativeID
                                model.setColumnObjective(czdAlt2, 0)
                                model.setColumnBinary(czdAlt2)
                                Linearize(model, czdAlt2, dCID, Dependency.SecondAlternativeID)

                                Dim dName As String
                                Select Case Dependency.Value
                                    Case RADependencyType.dtDependsOn
                                        ' first is not funded unless second is funded
                                        dName = "Dependency" + i.ToString + "_DependsOn"
                                        'model.loadPoint(dName, alt.ID, CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)))

                                        model.loadPoint(dName, czdAlt2, 1)
                                        model.loadPoint(dName, czdAlt1, -1)

                                        model.setRowMin(dName, 0)
                                        'dTerm = model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p) - (Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p)))) >= 0
                                    Case RADependencyType.dtMutuallyDependent
                                        dName = "Dependency" + i.ToString + "_MutuallyDependent"

                                        model.loadPoint(dName, czdAlt1, 1)
                                        model.loadPoint(dName, czdAlt2, -1)

                                        model.setRowFix(dName, 0)

                                    'model.loadPoint(dName, alt.ID, CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) - CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)))
                                    'model.setRowFix(dName, 0)
                                    Case RADependencyType.dtMutuallyExclusive
                                        dName = "Dependency" + i.ToString + "_MutuallyExclusive"

                                        model.loadPoint(dName, czdAlt1, 1)
                                        model.loadPoint(dName, czdAlt2, 1)
                                        model.loadPoint(dName, dCID, -1)

                                        model.setRowMax(dName, 1)

                                        'model.loadPoint(dName, alt.ID, CDbl(If(alt.ID = Dependency.FirstAlternativeID, 1, 0)) + CDbl(If(alt.ID = Dependency.SecondAlternativeID, 1, 0)))
                                        'model.setRowMax(dName, 1)
                                End Select

                                model.loadPoint("TotalConstraints", dCID, 1)
                            Else
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
                        End If
                        i += 1
                    Next
                End If

                If Settings.CustomConstraints Then
                    Dim i As Integer = 1
                    For Each Constraint As RAConstraint In Constraints.Constraints.Values
                        If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
                            Dim cName As String = "CustomConstraint_" + i.ToString
                            Dim ccCID As String = "C_CC_" + i.ToString
                            If constraintsList.FirstOrDefault(Function(c) (c.ID = ccCID)) IsNot Nothing AndAlso
                                constraintsList.First(Function(c) (c.ID = ccCID)).Enabled Then

                                ' for each custom constraint we create a vector that holds custom constrain values in the field Cost
                                model.setColumnObjective(ccCID, 0)
                                model.setColumnBinary(ccCID)

                                For Each alt As RAAlternative In Alternatives
                                    Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                                    If value <> UNDEFINED_INTEGER_VALUE Then
                                        'model.loadPoint(cName, alt.ID, value)

                                        Dim czccAlt As String = "C_Z_CC_" + i.ToString + "_" + alt.ID
                                        model.setColumnObjective(czccAlt, 0)
                                        model.setColumnBinary(czccAlt)

                                        model.loadPoint(cName, czccAlt, value)

                                        Linearize(model, czccAlt, ccCID, alt.ID)
                                    End If
                                    'model.loadPoint(cName, alt.ID, CDbl(If(value = UNDEFINED_INTEGER_VALUE, 0, value)))
                                Next

                                If Constraint.MinValueSet And Not Constraint.MaxValueSet Then
                                    ' Adding constraint for custom constraint for Minimum Value
                                    'Dim ccTermMin As Term = model.GreaterEqual(model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MinValue)
                                    model.setRowMin(cName, Constraint.MinValue)

                                    model.loadPoint(cName, ccCID, -Constraint.MinValue)
                                    model.setRowMin(cName, 0)
                                End If
                                If Constraint.MaxValueSet And Not Constraint.MinValueSet Then
                                    ' Adding constraint for custom constraint for Maximum Value
                                    'Dim ccTermMax As Term = model.LessEqual(model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MaxValue)
                                    'model.setRowMax(cName, Constraint.MaxValue)

                                    model.loadPoint(cName, ccCID, -Constraint.MaxValue)
                                    model.setRowMax(cName, 0)
                                End If
                                If Constraint.MinValueSet And Constraint.MaxValueSet Then
                                    ' Adding constraint for custom constraint for Maximum Value
                                    'Dim ccTermMax As Term = model.LessEqual(model.Sum(model.ForEach(items, Function(p) ((Choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MaxValue)
                                    'model.setRowMinMax(cName, Constraint.MinValue, Constraint.MaxValue)

                                    model.loadPoint(cName, ccCID, -Constraint.MinValue)
                                    model.setRowMin(cName, 0)

                                    model.loadPoint(cName, ccCID, -Constraint.MaxValue)
                                    model.setRowMax(cName, 0)
                                End If

                                model.loadPoint("TotalConstraints", ccCID, 1)
                            Else
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
                            End If
                            i += 1
                        End If
                    Next
                End If

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
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

                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.TimePeriods Then
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

                            'For tp As Integer = apd.GetMinPeriod To apd.GetMaxPeriod + apd.Duration - 1
                            '    If tp < j Or tp > j + apd.Duration - 1
                            '        model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)
                            '        model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TP_" + alt.ID.ToString + "_" + tp.ToString, 1)
                            '        model.setRowMax("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, 1)
                            '    End If
                            'Next

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                                            End If
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            ''model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, -value)
                                            ''model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            ' D3918 ===
                                            'If resource.ID.Equals(RA_Cost_GUID) Then

                                            ''model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                            ''model.loadPoint("TPRes_" + resource.ID.ToString + "_" + (j + k).ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            ''model.setRowMin("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            TPRowCount += 1
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
                        model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        TPRowVariantCount += 1
                        altPos += 1
                    Next

                    Debug.Print("TPRowVariantCount = " + TPRowVariantCount.ToString)
                    Debug.Print("TPRowCount = " + TPRowCount.ToString)

                    Dim pSum() As Double
                    ReDim pSum(CurrentScenario.TimePeriods.Periods.Count - 1)
                    For Each x As Double In pSum
                        x = 0
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
                                                model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPVar_" + alt.ID + "_" + j.ToString, c)
                                                pSum(t) += c
                                            End If
                                        End If
                                    Next
                                End If
                            Next

                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    If Settings.ResourcesMin And Settings.ResourcesMax Then
                                        model.setRowMinMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue, maxValue)
                                    Else
                                        If Settings.ResourcesMin Then
                                            model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue)
                                        Else
                                            If Settings.ResourcesMax Then
                                                model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, maxValue)
                                            End If
                                        End If
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, maxValue)  ' D3918
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
                                    model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue)  ' D3918
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
                                Dim tpdCID As String = "C_TPDependency" + k.ToString

                                If constraintsList.FirstOrDefault(Function(c) (c.ID = tpdCID)) IsNot Nothing AndAlso
                                    constraintsList.First(Function(c) (c.ID = tpdCID)).Enabled Then

                                    model.setColumnObjective(tpdCID, 0)
                                    model.setColumnBinary(tpdCID)

                                    Dim apd1 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.FirstAlternativeID)
                                    Dim apd2 As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(dependency.SecondAlternativeID)
                                    For j As Integer = apd1.GetMinPeriod To apd1.GetMaxPeriod
                                        'model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString, j + 1)

                                        Dim cztpdAlt1 As String = "C_Z_TPDependency_" + k.ToString + "_" + "TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString
                                        model.setColumnObjective(cztpdAlt1, 0)
                                        model.setColumnBinary(cztpdAlt1)
                                        Linearize(model, cztpdAlt1, tpdCID, "TPVar_" + dependency.FirstAlternativeID + "_" + j.ToString)

                                        model.loadPoint("TPDependency_" + k.ToString, cztpdAlt1, j + 1)

                                    Next
                                    For j As Integer = apd2.GetMinPeriod To apd2.GetMaxPeriod
                                        'model.loadPoint("TPDependency_" + k.ToString, "TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString, -(j + 1))

                                        Dim cztpdAlt2 As String = "C_Z_TPDependency_" + k.ToString + "_" + "TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString
                                        model.setColumnObjective(cztpdAlt2, 0)
                                        model.setColumnBinary(cztpdAlt2)
                                        Linearize(model, cztpdAlt2, tpdCID, "TPVar_" + dependency.SecondAlternativeID + "_" + j.ToString)

                                        model.loadPoint("TPDependency_" + k.ToString, cztpdAlt2, j + 1)
                                    Next
                                    Select Case dependency.Value
                                        Case RADependencyType.dtSuccessive
                                            'model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))

                                            Dim cztpdAlt As String = "C_Z_TPDependency_" + k.ToString + "_" + dependency.FirstAlternativeID
                                            model.setColumnObjective(cztpdAlt, 0)
                                            model.setColumnBinary(cztpdAlt)
                                            Linearize(model, cztpdAlt, tpdCID, dependency.FirstAlternativeID)

                                            model.loadPoint("TPDependency_" + k.ToString, cztpdAlt, -(apd2.GetMaxPeriod + apd2.Duration))

                                            'model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
                                            model.loadPoint("TPDependency_" + k.ToString, tpdCID, -(apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration)))
                                            model.setRowMin("TPDependency_" + k.ToString, 0)
                                        Case RADependencyType.dtConcurrent
                                            ' UNCOMMENT TO FORCE CONCURRENT:
                                            'Dim v As Double = Math.Min(apd1.Duration, apd2.Duration) - 0.5
                                            'model.setRowMinMax("TPDependency_" + k.ToString, -v, v)

                                            ' Can be concurrent:
                                            'model.setRowMax("TPDependency_" + k.ToString, 0)

                                            'model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -apd2.GetMaxPeriod)

                                            Dim cztpdAlt As String = "C_Z_TPDependency_" + k.ToString + "_" + dependency.FirstAlternativeID
                                            model.setColumnObjective(cztpdAlt, 0)
                                            model.setColumnBinary(cztpdAlt)
                                            Linearize(model, cztpdAlt, tpdCID, dependency.FirstAlternativeID)

                                            model.loadPoint("TPDependency_" + k.ToString, cztpdAlt, -apd2.GetMaxPeriod)

                                            'model.setRowMin("TPDependency_" + k.ToString, -apd2.GetMaxPeriod)
                                            model.loadPoint("TPDependency_" + k.ToString, tpdCID, -(-apd2.GetMaxPeriod))
                                            model.setRowMin("TPDependency_" + k.ToString, 0)
                                    End Select
                                Else
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
                                            model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                            model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
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
                            End If
                            k += 1
                        Next
                    End If
                End If

                Dim command As String = XACommand
                model.setCommand(command)

                Dim numConstraints As Long = ConstraintsCount
                Dim isFeasible As Boolean = False
                While Not isFeasible And numConstraints >= 0
                    model.setRowFix("TotalConstraints", numConstraints)

                    model.solve()

                    Debug.Print(model.getSolverStatus.ToString)

                    Select Case model.getModelStatus
                        Case 1, 2
                            SolverState = raSolverState.raSolved
                            isFeasible = True

                            result = ""
                            result += "Removing " + (ConstraintsCount - numConstraints).ToString + " constraints will make the model feasbile" + vbNewLine

                            result += vbNewLine + "<ul type='square'><i>Constraints to remove</i>:"

                            For Each cInfo As ConstraintInfo In constraintsList.Where(Function(c) c.Enabled).ToList
                                If model.getColumnPrimalActivity(cInfo.ID) = 0 Then
                                    result += "<li>" + cInfo.Description + "</li>"
                                End If
                            Next
                            result += "</ul>"

                            If Settings.FundingPools Then
                                For Each alt As RAAlternative In Alternatives
                                    Debug.Print(vbNewLine)
                                    Dim s As String = alt.Name + ":   "
                                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                                        If FP.Enabled Then
                                            s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                            FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                        End If
                                    Next
                                    Debug.Print(s)
                                Next

                                Debug.Print(vbNewLine)
                                Debug.Print("--- FUNDING POOLS VALUES ---")
                                For Each alt As RAAlternative In Alternatives
                                    Debug.Print(vbNewLine)
                                    Dim s As String = alt.Name + ": "
                                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                                        If FP.Enabled Then
                                            s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                                        End If
                                    Next
                                    Debug.Print(s)
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
                                            'Debug.Print("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString + " = " + model.getColumnPrimalActivity("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString).ToString)
                                            Debug.Print("TPVar_" + alt.ID.ToString + "_" + j.ToString + " = " + model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + "_" + j.ToString).ToString)

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
                            result += vbNewLine + "<b>Funded benefit</b>: " + mFundedBenefit.ToString + vbNewLine
                            result += "<b>Funded original benefit</b>: " + mFundedOriginalBenefit.ToString + vbNewLine
                            result += "<b>Funded cost</b>: " + mFundedCost.ToString + vbNewLine
                        Case 4, 10
                            numConstraints -= 1
                            SolverState = raSolverState.raInfeasible
                    End Select
                    LastError = ""
                End While
            Catch ex As Exception
                Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return result
        End Function

        Private Sub Linearize(model As XA.Optimize, ZName As String, CName As String, XName As String)
            model.loadPoint(ZName + "_1", ZName, 1)
            model.loadPoint(ZName + "_1", XName, -1)
            model.setRowMax(ZName + "_1", 0)

            model.loadPoint(ZName + "_2", ZName, 1)
            model.loadPoint(ZName + "_2", CName, -1)
            model.setRowMax(ZName + "_2", 0)

            model.loadPoint(ZName + "_3", ZName, 1)
            model.loadPoint(ZName + "_3", XName, -1)
            model.loadPoint(ZName + "_3", CName, -1)
            model.setRowMin(ZName + "_3", -1)
        End Sub

        Private Function Solve_XA7(Optional OutputPath As String = "", Optional num As Integer = -1) As Boolean
            If ProjectManager Is Nothing Then Return False
            ResetFunded()

            'Dim cList As New List(Of ConstraintInfo)
            Dim result As Boolean = False

            Dim Reserve1 As Integer = 16222694  ' Confidential  Activation Codes
            Dim Reserve2 As Integer = 989284725

            'Dim model As New Optimizer.XA.Optimize(100000)  ' Initialize XA
            Dim model As New Optimizer.XA.Optimize(0, 0, 0, 0, 100, 100)  ' Initialize XA
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
                                If cellCost = UNDEFINED_INTEGER_VALUE Then cellCost = 0
                                model.setColumnSemiContinuous("FP" + FP.ID.ToString + "_" + alt.ID, 0, cellCost)
                            End If
                        Next
                    Next
                End If

                If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                    For Each alt As RAAlternative In Alternatives
                        'Dim i As Integer = 0
                        'For each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        '    model.setColumnObjective("TP_" + alt.ID.ToString + "_" + i.ToString, 0)
                        '    model.setColumnBinary("TP_" + alt.ID.ToString + "_" + i.ToString)
                        '    i += 1
                        'Next

                        Dim apd As AlternativePeriodsData = CurrentScenario.TimePeriods.PeriodsData.GetAlternativePeriodsData(alt.ID)
                        For j As Integer = apd.GetMinPeriod To apd.GetMaxPeriod
                            model.setColumnObjective("TPVar_" + alt.ID.ToString + "_" + j.ToString, 0)
                            model.setColumnBinary("TPVar_" + alt.ID.ToString + "_" + j.ToString)
                        Next
                    Next

                    'Dim t As Integer = 0
                    'For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                    '    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                    '        If resource.Enabled Then 'A1130
                    '            'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                    '            Dim minValue As Double = period.GetResourceMinValue(resource.ID)  ' D3918
                    '            Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)  ' D3918

                    '            ' CODE BELOW IS FOR CASES WHEN WE NEED TO HAVE THE ABILITY TO NOT FUND ALTERNATIVE IN CASE OF MIN/MAX
                    '            'If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                    '            '    model.setColumnObjective("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString, 0)
                    '            '    model.setColumnBinary("TPResY_" + period.ID.ToString + "_" + resource.ID.ToString)
                    '            'End If
                    '            'End If
                    '        End If
                    '    Next
                    '    t += 1
                    'Next
                End If

                ' Cost: alt1.Cost *  alt1   +  alt2.Cost * alt2 +  alt3.Cost * alt3   <=  BudgetLimit

                model.setRowMax("Cost", BudgetLimit)
                For Each alt As RAAlternative In Alternatives
                    model.loadToCurrentRow(alt.ID, alt.Cost)
                Next

                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0
                For Each alt As RAAlternative In Alternatives
                    If alt.Must Then mustCount += 1
                    If alt.MustNot Then mustnotCount += 1
                Next


                If Settings.Musts And (mustCount > 0) Then
                    'For Each alt As RAAlternative In Alternatives
                    '    model.loadPoint("Must", alt.ID, CDbl(If(alt.Must, 1, 0)))
                    'Next
                    'model.setRowFix("Must", mustCount)
                    For Each alt As RAAlternative In Alternatives
                        If alt.Must Then
                            If UseConstraintsAnalysis Then
                                model.setColumnObjective("C_Must_" + alt.ID, 0)
                                model.setColumnBinary("C_Must_" + alt.ID)

                                model.setColumnObjective("C_Z_Must_" + alt.ID, 0)
                                model.setColumnBinary("C_Z_Must_" + alt.ID)

                                model.loadPoint("Constrain_Must_" + alt.ID, "C_Must_" + alt.ID, -1)
                                model.loadPoint("Constrain_Must_" + alt.ID, "C_Z_Must_" + alt.ID, 1)
                                model.setRowFix("Constrain_Must_" + alt.ID, 0)

                                model.loadPoint("Constrain_Must_1_" + alt.ID, "C_Z_Must_" + alt.ID, 1)
                                model.loadPoint("Constrain_Must_1_" + alt.ID, "C_Must_" + alt.ID, -1)
                                model.setRowMax("Constrain_Must_1_" + alt.ID, 0)

                                model.loadPoint("Constrain_Must_2_" + alt.ID, "C_Z_Must_" + alt.ID, 1)
                                model.loadPoint("Constrain_Must_2_" + alt.ID, alt.ID, -1)
                                model.setRowMax("Constrain_Must_2_" + alt.ID, 0)

                                model.loadPoint("Constrain_Must_3_" + alt.ID, "C_Z_Must_" + alt.ID, 1)
                                model.loadPoint("Constrain_Must_3_" + alt.ID, "C_Must_" + alt.ID, -1)
                                model.loadPoint("Constrain_Must_3_" + alt.ID, alt.ID, -1)
                                model.setRowMin("Constrain_Must_3_" + alt.ID, -1)

                                model.loadPoint("TotalConstraints", "C_Must_" + alt.ID, 1)
                            Else
                                model.loadPoint("Must_" + alt.ID, alt.ID, 1)
                                model.setRowFix("Must_" + alt.ID, 1)
                            End If
                        End If
                    Next
                    If UseConstraintsAnalysis Then
                        model.setRowFix("TotalConstraints", num)
                    End If
                End If

                If Settings.MustNots And (mustnotCount > 0) Then
                    'For Each alt As RAAlternative In Alternatives
                    '    model.loadPoint("MustNot", alt.ID, CDbl(If(alt.MustNot, 1, 0)))
                    'Next
                    'model.setRowFix("MustNot", 0)
                    For Each alt As RAAlternative In Alternatives
                        If alt.MustNot Then
                            model.loadPoint("MustNot_" + alt.ID, alt.ID, 1)
                            model.setRowFix("MustNot_" + alt.ID, 0)
                        End If
                    Next
                End If

                If Settings.Groups Then
                    Dim i As Integer = 1
                    For Each group As RAGroup In CurrentScenario.GetAvailableGroups
                        'For Each group As RAGroup In Groups.Groups.Values
                        Dim filteredAlts As List(Of RAAlternative) = group.GetFilteredAlternatives(CurrentScenario.Alternatives)
                        If group.Enabled And filteredAlts.Count > 1 Then
                            Dim gName As String = "Group_" + i.ToString
                            For Each alt As RAAlternative In Alternatives
                                'model.loadPoint(gName, alt.ID, CDbl(If(group.Alternatives.ContainsKey(alt.ID), 1, 0)))
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

                If Settings.FundingPools And FundingPools.Pools.Count > 0 Then
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

                'If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.TimePeriods Then
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

                            'For tp As Integer = apd.GetMinPeriod To apd.GetMaxPeriod + apd.Duration - 1
                            '    If tp < j Or tp > j + apd.Duration - 1
                            '        model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, 1)
                            '        model.loadPoint("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, "TP_" + alt.ID.ToString + "_" + tp.ToString, 1)
                            '        model.setRowMax("TPRowVariant_Exclude_" + alt.ID.ToString + "_" + j.ToString + "_" + tp.ToString, 1)
                            '    End If
                            'Next

                            For k As Integer = 0 To apd.Duration - 1
                                Dim c As Double = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, RA_Cost_GUID) ' D3918
                                If c <> UNDEFINED_INTEGER_VALUE Then
                                    '--- model.loadPoint("TPRow" + i.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                    For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                                        c = CurrentScenario.TimePeriods.PeriodsData.GetResourceValue(k, alt.ID, resource.ID)
                                        If resource.Enabled AndAlso c <> UNDEFINED_INTEGER_VALUE Then
                                            ' +++
                                            Dim value As Double = alt.Cost
                                            If resource.ConstraintID >= 0 Then
                                                value = CurrentScenario.Constraints.GetConstraintValue(resource.ConstraintID, alt.ID)
                                            End If
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + j.ToString, -value)
                                            'model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)

                                            ''model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TPVar_" + alt.ID.ToString + "_" + j.ToString, -value)
                                            ''model.loadPoint("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            ' D3918 ===
                                            'If resource.ID.Equals(RA_Cost_GUID) Then

                                            ''model.loadPoint("TPRes" + (j + k).ToString + "_" + resource.ID.ToString, "TP" + i.ToString + "_" + (j + k).ToString + "_" + alt.ID.ToString, c)
                                            ''model.loadPoint("TPRes_" + resource.ID.ToString + "_" + (j + k).ToString, "TP_" + alt.ID.ToString + "_" + (j + k).ToString, c)

                                            ''model.setRowMin("TPRow_" + alt.ID + "_" + resource.ID.ToString + "_" + j.ToString, 0)
                                            TPRowCount += 1
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
                        model.setRowFix("TPRowVariant_" + alt.ID.ToString, 0)
                        TPRowVariantCount += 1
                        altPos += 1
                    Next

                    'Debug.Print("TPRowVariantCount = " + TPRowVariantCount.ToString)
                    'Debug.Print("TPRowCount = " + TPRowCount.ToString)

                    Dim pSum() As Double
                    ReDim pSum(CurrentScenario.TimePeriods.Periods.Count - 1)
                    For Each x As Double In pSum
                        x = 0
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
                                                'model.setColumnObjective("TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 0)
                                                'model.setColumnBinary("TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString)

                                                'model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, c)
                                                model.loadPoint("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, "TPVar_" + alt.ID + "_" + j.ToString, c)

                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", "TP_" + alt.ID.ToString + "_" + t.ToString, -1)
                                                'model.setRowMax("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_1", 0)

                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", "TPVar_" + alt.ID.ToString + "_" + j.ToString, -1)
                                                'model.setRowMax("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_2", 0)

                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TPResSubst_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString, 1)
                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TP_" + alt.ID.ToString + "_" + t.ToString, -1)
                                                'model.loadPoint("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", "TPVar_" + alt.ID.ToString + "_" + j.ToString, -1)
                                                'model.setRowMin("TPRes_" + alt.ID + "_" + resource.ID.ToString + "_" + t.ToString + "_" + j.ToString + "_3", -1)

                                                pSum(t) += c
                                                'model.loadPoint("TPRes_" + resource.ID.ToString + "_" + t.ToString, "TP_" + alt.ID.ToString + "_" + t.ToString, pSum(t))
                                                'model.loadPoint("TPRes_" + resource.ID.ToString + "_" + t.ToString, "TP_" + alt.ID.ToString + "_" + t.ToString, c)
                                            End If
                                        End If
                                    Next
                                End If
                            Next

                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                'If resource.ID.Equals(RA_Cost_GUID) Then    ' D3918
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)      ' D3918
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)      ' D3918

                                If minValue <> UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                    'model.setRowMinMax("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue, maxValue)
                                    If Settings.ResourcesMin And Settings.ResourcesMax Then
                                        model.setRowMinMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue, maxValue)
                                    Else
                                        If Settings.ResourcesMin Then
                                            model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue)
                                        Else
                                            If Settings.ResourcesMax Then
                                                model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, maxValue)
                                            End If
                                        End If
                                    End If
                                End If
                                If minValue = UNDEFINED_INTEGER_VALUE And maxValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMax Then
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    'model.setRowMax("TPRes" + t.ToString + "_" + resource.ID.ToString, maxValue)  ' D3918
                                    model.setRowMax("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, maxValue)  ' D3918
                                End If
                                If minValue <> UNDEFINED_INTEGER_VALUE And Settings.ResourcesMin And maxValue = UNDEFINED_INTEGER_VALUE Then
                                    'model.setRowMin("TPRes" + t.ToString + "_" + resource.ID.ToString, minValue)  ' D3918
                                    model.setRowMin("TPRes_Main_" + resource.ID.ToString + "_" + t.ToString, minValue)  ' D3918
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
                                        model.loadPoint("TPDependency_" + k.ToString, dependency.FirstAlternativeID, -(apd2.GetMaxPeriod + apd2.Duration))
                                        model.setRowMin("TPDependency_" + k.ToString, apd2.Duration - (apd2.GetMaxPeriod + apd2.Duration))
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
                    ' D3488 ===
                    OutputPath = OutputPath.Replace("\", "\\")
                    model.setCommand(String.Format("MatList Equ Output {0}xa_model.log Set CmprsName _   ListInput Yes ", OutputPath))
                    'model.setCommand(String.Format("FileName {0}xa_rcc_model    ToRcc Yes", OutputPath))
                    model.setCommand(String.Format("FileName {0}xa_mps_model    ToMPS Yes", OutputPath))
                    ' D3488 ==
                End If

                'model.setCommand("Presolve  0 strategy 6")
                'model.setCommand("strategy 6    StopUnchange  1:00  StopAfter  2:00 ")
                Dim command As String = XACommand
                model.setCommand(command)
                'model.solveWithInfeasibleAnalysis()

                model.solve()

                'Debug.Print(model.getSolverStatus.ToString)

                Select Case model.getModelStatus
                    Case 1, 2
                        result = True
                        SolverState = raSolverState.raSolved

                        ResourceAligner.Logs = ""
                        ResourceAligner.Logs = "Removing this constraints will make the model feasible: " + vbNewLine + vbNewLine
                        If UseConstraintsAnalysis Then
                            For Each alt As RAAlternative In Alternatives
                                If alt.Must AndAlso model.getColumnPrimalActivity("C_Must_" + alt.ID) = 0 Then
                                    ResourceAligner.Logs += "Must for alternative '" + alt.Name + "'" + vbNewLine
                                End If
                            Next
                        End If

                        If Settings.FundingPools Then
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ":   "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID).ToString + "; "
                                        FP.SetAlternativeAllocatedValue(alt.ID, model.getColumnPrimalActivity("FP" + FP.ID.ToString + "_" + alt.ID))
                                    End If
                                Next
                                'Debug.Print(s)
                            Next

                            'Debug.Print(vbNewLine)
                            'Debug.Print("--- FUNDING POOLS VALUES ---")
                            For Each alt As RAAlternative In Alternatives
                                'Debug.Print(vbNewLine)
                                Dim s As String = alt.Name + ": "
                                For Each FP As RAFundingPool In FundingPools.Pools.Values
                                    If FP.Enabled Then
                                        s += "FP_'" + FP.Name + "' = " + FP.GetAlternativeAllocatedValue(alt.ID).ToString + "; "
                                    End If
                                Next
                                'Debug.Print(s)
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
                                        ''Debug.Print("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString + " = " + model.getColumnPrimalActivity("TP" + i.ToString + "_" + j.ToString + "_" + alt.ID.ToString).ToString)
                                        'Debug.Print("TPVar_" + alt.ID.ToString + "_" + j.ToString + " = " + model.getColumnPrimalActivity("TPVar_" + alt.ID.ToString + "_" + j.ToString).ToString)

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
                        result = False
                        SolverState = raSolverState.raInfeasible

                End Select
                LastError = ""
            Catch ex As Exception
                'Debug.Print("Error occured. Reason Code: " + model.getXAExceptionCode().ToString + " Message: " + model.getXAExceptionMessage())
                'OnException(ex)    ' -D3488 due to overrides the real error msg and state
                SolverState = raSolverState.raError
                LastError = model.getXAExceptionMessage
            Finally
                model.closeConnection()
            End Try

            Return result
        End Function

        Public Sub RunInfeasibilityAnalysis()
            UseConstraintsAnalysis = True
            Dim isInfeasible As Boolean = True
            Dim num As Integer = 3
            While isInfeasible And num <> -1
                isInfeasible = Not Solve_XA7(, num)
                num -= 1
            End While
            'Debug.Print(ResourceAligner.Logs)
            UseConstraintsAnalysis = False
        End Sub

    End Class
End Namespace
