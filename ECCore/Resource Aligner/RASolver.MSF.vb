Option Strict On

Imports ECCore
Imports Canvas
Imports Microsoft.SolverFoundation.Services
Imports System.IO

Namespace Canvas

    Partial Public Class RASolver

        Private Function Solve_MSF(tSolverExport As raSolverExport, Optional ByRef tExportData As Object = Nothing) As Boolean ' D3193 + A0923 + D3236
            If ProjectManager Is Nothing Then Return False

            ResetFunded()

            Dim Alts As New List(Of RAAlternative)
            Dim AltsPartial As New List(Of RAAlternative)
            Dim AltsPartialY As New List(Of RAAlternative)
            Dim AltsIsPartial As New List(Of RAAlternative)
            Dim AltsIsInteger As New List(Of RAAlternative)

            For Each alt As RAAlternative In Alternatives
                ' if we are ignoring risks, we should use BenefitOriginal value
                ' if we are using risks, we should use adjusted value
                alt.Benefit = CDbl(IIf(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal))

                ' alternative cannot be partial if it is included in one of the dependencies
                Dim isPartial As Boolean = (alt.IsPartial And Not Dependencies.IsAlwaysInteger(alt.ID))
                If (alt.Must And Settings.Musts) Then
                    isPartial = False
                End If

                Dim newAlt As RAAlternative = alt.Clone

                If newAlt.MustNot And Settings.MustNots Then
                    newAlt.Benefit = 0
                    newAlt.BenefitPartial = 0
                    newAlt.Cost = 0
                    newAlt.CostPartial = 0
                    newAlt.MinPercent = 0
                Else
                    ' for integer alternatives we will use Benefit and Cost fields. BenefitPartial and CostPartial fields will be zeros.
                    ' for partial alternatives we will use BenefitPartial and CostPartial fields. Benefit and Cost fields will be zeros.
                    newAlt.Benefit = CDbl(IIf(alt.Must And Settings.Musts, alt.Benefit, IIf(isPartial, 0, alt.Benefit)))
                    newAlt.BenefitPartial = CDbl(IIf(alt.Must And Settings.Musts, 0, IIf(isPartial, alt.Benefit, 0)))
                    newAlt.Cost = CDbl(IIf(alt.Must And Settings.Musts, alt.Cost, IIf(isPartial, 0, alt.Cost)))
                    newAlt.CostPartial = CDbl(IIf(alt.Must And Settings.Musts, 0, IIf(isPartial, alt.Cost, 0)))
                End If

                ' Funded field is a place where result for integers will be stored
                ' FundedPartial - result for partial
                newAlt.Funded = 0
                newAlt.FundedPartial = 0

                Dim IsPartialAlt As RAAlternative = alt.Clone
                IsPartialAlt.Funded = CInt(IIf(isPartial, 1, 0))
                AltsIsPartial.Add(IsPartialAlt)

                Dim IsIntegerAlt As RAAlternative = alt.Clone
                IsIntegerAlt.Funded = CInt(IIf(isPartial, 0, 1))
                AltsIsInteger.Add(IsIntegerAlt)

                Alts.Add(newAlt.Clone)
                AltsPartial.Add(newAlt.Clone)
                AltsPartialY.Add(newAlt.Clone)
            Next

            Dim budget As Double = BudgetLimit

            Dim context As SolverContext = SolverContext.GetContext()
            Dim model As Model = context.CurrentModel
            If model IsNot Nothing Then context.ClearModel()
            model = context.CreateModel()

            Dim items As Microsoft.SolverFoundation.Services.Set = New Microsoft.SolverFoundation.Services.Set(Domain.Any, "items")

            ' Creating parameter for priority values and binding it to Benefit field
            Dim priority As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "priority", items)
            priority.SetBinding(Alts, "Benefit", "ID")
            model.AddParameter(priority)

            ' Creating parameter for partial priority values and binding it to BenefitPartial field
            Dim priorityPartial As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "priorityPartial", items)
            priorityPartial.SetBinding(AltsPartial, "BenefitPartial", "ID")
            model.AddParameter(priorityPartial)

            ' Creating parameter for cost values and binding it to Cost field
            Dim cost As New Microsoft.SolverFoundation.Services.Parameter(Domain.Real, "cost", items)
            cost.SetBinding(Alts, "Cost", "ID")
            model.AddParameter(cost)

            ' Creating parameter for cost partial values and binding it to CostPartial field
            Dim costPartial As New Microsoft.SolverFoundation.Services.Parameter(Domain.Real, "costPartial", items)
            costPartial.SetBinding(AltsPartial, "CostPartial", "ID")
            model.AddParameter(costPartial)

            ' Creating parameter for minimum percent values for partials and binding it to MinPercent field
            Dim minPercent As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "minPercent", items)
            minPercent.SetBinding(Alts, "MinPercent", "ID")
            model.AddParameter(minPercent)

            ' Creating parameter to determine whether alternative is integer or partial
            Dim IsPartialParam As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "IsPartialParam", items)
            IsPartialParam.SetBinding(AltsIsPartial, "Funded", "ID")
            model.AddParameter(IsPartialParam)

            Dim IsIntegerParam As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "IsIntegerParam", items)
            IsIntegerParam.SetBinding(AltsIsInteger, "Funded", "ID")
            model.AddParameter(IsIntegerParam)

            ' Creating 2 Decisions (basically, vectors that will hold result values) - one for integers, one for partials
            Dim choose As New Decision(Domain.IntegerRange(0, 1), "choose", items)
            choose.SetBinding(Alts, "Funded", "ID")
            model.AddDecision(choose)

            Dim choosePartial As New Decision(Domain.RealRange(0, 1), "choosePartial", items)
            choosePartial.SetBinding(AltsPartial, "FundedPartial", "ID")
            model.AddDecision(choosePartial)

            ' This is additional parameter for partials
            ' We need it because partial alternative can be either not funded at all or funded higher than min percent value
            ' So it is not continuous variable
            Dim chooseY As New Decision(Domain.IntegerRange(0, 1), "chooseY", items)
            chooseY.SetBinding(AltsPartialY, "FundedPartial", "ID")
            model.AddDecision(chooseY)


            ' Adding min partial constraints
            Dim cMinPercent1 As Constraint = model.AddConstraint("MinimumPercent1", model.ForEach(items, Function(p) (choosePartial(p) <= chooseY(p))))
            Dim cMinPercent2 As Constraint = model.AddConstraint("MinimumPercent2", model.ForEach(items, Function(p) (choosePartial(p) >= minPercent(p) * chooseY(p))))

            If Settings.Musts Or Settings.MustNots Then
                Dim mustAlts As New List(Of RAAlternative)
                Dim mustnotAlts As New List(Of RAAlternative)
                Dim mustCount As Integer = 0
                Dim mustnotCount As Integer = 0

                ' Creating a vector of MUST alternatives (mustAlts). Funded property = 1 if alternative is a MUST and 0 otherwise
                ' The same for MUSTNOT alternatives (mustnotAlts)
                For Each alt As RAAlternative In Alts
                    Dim newAltMust As RAAlternative = alt.Clone
                    newAltMust.Funded = CInt(IIf(alt.Must And Settings.Musts, 1, 0))
                    mustAlts.Add(newAltMust)
                    If alt.Must And Settings.Musts Then mustCount += 1

                    Dim newAltMustNot As RAAlternative = alt.Clone
                    newAltMustNot.Funded = CInt(IIf(alt.MustNot And Settings.MustNots, 1, 0))
                    mustnotAlts.Add(newAltMustNot)
                    If alt.MustNot And Settings.MustNots Then
                        mustnotCount += 1
                    End If
                Next

                If Settings.Musts And (mustCount > 0) Then
                    ' Creating parameter for MUST alternatives and binding it to our list of MUST alts
                    Dim chooseMust As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "MustParam", items)
                    chooseMust.SetBinding(mustAlts, "Funded", "ID")
                    model.AddParameter(chooseMust)

                    ' Here we say that all must alternatives should be fully funded
                    Dim MustTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * IsIntegerParam(p) * chooseMust(p)))) = mustCount
                    Dim mustConstraint As Constraint = model.AddConstraint("MustConstraint", MustTerm)
                End If

                If Settings.MustNots And (mustnotCount > 0) Then
                    ' Creating parameter for MUST alternatives and binding it to our list of MUSTNOT alts
                    Dim chooseMustNot As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "MustNotParam", items)
                    chooseMustNot.SetBinding(mustnotAlts, "Funded", "ID")
                    model.AddParameter(chooseMustNot)

                    ' Here we say that all mustnot alternatives should NOT be funded
                    ' We should have written "= 0" instead of "<= 0.00001", but for some reason it doesn't work with MSF
                    Dim MustNotTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseMustNot(p)))) <= 0.00001
                    Dim mustnotConstraint As Constraint = model.AddConstraint("MustNotConstraint", MustNotTerm)

                    Dim MustNotTermPartial As Term = model.Sum(model.ForEach(items, Function(p) (choosePartial(p) * chooseMustNot(p)))) <= 0.00001
                    Dim mustnotConstraintPartial As Constraint = model.AddConstraint("MustNotConstraintPartial", MustNotTermPartial)
                End If
            End If

            If Settings.Groups Then
                Dim i As Integer = 1
                For Each group As RAGroup In Groups.Groups.Values
                    If group.Enabled Then
                        Dim groupTerm As Term = Nothing

                        ' For each groups we create a vector with alternatives from that group
                        ' If alternative is included in this group, Funded value will be = 1, otherwise it will be 0
                        Dim groupAlts As New List(Of RAAlternative)
                        For Each alt As RAAlternative In Alts
                            Dim newAlt As RAAlternative = alt.Clone
                            newAlt.Funded = CDbl(IIf(group.Alternatives.ContainsKey(alt.ID), 1, 0))
                            newAlt.FundedPartial = newAlt.Funded
                            groupAlts.Add(newAlt)
                        Next

                        ' Creating parameter for the list of group alternatives
                        Dim chooseGroup As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Group" + i.ToString, items)
                        chooseGroup.SetBinding(groupAlts, "Funded", "ID")
                        model.AddParameter(chooseGroup)

                        ' Adding constraints for groups
                        Select Case group.Condition
                            Case RAGroupCondition.gcLessOrEqualsOne
                                ' none of one alterantive from the group if funded
                                groupTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseGroup(p)))) <= 1
                            Case RAGroupCondition.gcEqualsOne
                                ' exactly one alterantive from the group is funded
                                groupTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseGroup(p)))) = 1
                            Case RAGroupCondition.gcGreaterOrEqualsOne
                                ' at least one alternative from the group is funded
                                groupTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseGroup(p)))) >= 1
                        End Select
                        Dim groupConstraint As Constraint = model.AddConstraint("Constraint_Group" + i.ToString, groupTerm)
                        i += 1
                    End If
                Next
            End If

            If Settings.CustomConstraints Then
                Dim i As Integer = 1
                For Each Constraint As RAConstraint In Constraints.Constraints.Values
                    If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
                        ' for each custom constraint we create a vector that holds custom constrain values in the field Cost
                        Dim ccAlts As New List(Of RAAlternative)
                        For Each alt As RAAlternative In Alts
                            Dim newAlt As RAAlternative = alt.Clone
                            Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
                            newAlt.Cost = CDbl(IIf(value = UNDEFINED_INTEGER_VALUE, 0, value))
                            ccAlts.Add(newAlt)
                        Next

                        ' Creating parameter for custom constraint and binding it to Cost field
                        Dim chooseCC As New Microsoft.SolverFoundation.Services.Parameter(Domain.Real, "CC" + i.ToString, items)
                        chooseCC.SetBinding(ccAlts, "Cost", "ID")
                        model.AddParameter(chooseCC)

                        If Constraint.MinValueSet Then
                            ' Adding constraint for custom constraint for Minimum Value
                            Dim ccTermMin As Term = model.GreaterEqual(model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MinValue)
                            Dim ccConstraintMin As Constraint = model.AddConstraint("Constraint_Min_" + i.ToString, ccTermMin)
                        End If
                        If Constraint.MaxValueSet Then
                            ' Adding constraint for custom constraint for Maximum Value
                            Dim ccTermMax As Term = model.LessEqual(model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MaxValue)
                            Dim ccConstraintMax As Constraint = model.AddConstraint("Constraint_Max_" + i.ToString, ccTermMax)
                        End If

                        ' Please note that we were using additional parameter IsPartialParam:
                        ' (choose(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)
                        ' without this "* IsPartialParam(p)" it didn't work for some reason
                        ' there were some non-zero values in some of choosePartial items that shouldn't have been there (for integer alternatives)

                        i += 1
                    End If
                Next
            End If

            If Settings.Dependencies Then
                Dim i As Integer = 1
                For Each Dependency As RADependency In Dependencies.Dependencies
                    Dim dAltsFirst As New List(Of RAAlternative)
                    Dim dAltsSecond As New List(Of RAAlternative)

                    ' Dependency requires 2 alternatives
                    ' Creating 2 vectors of alternatives for dependency
                    For Each alt As RAAlternative In Alts
                        Dim newAlt1 As RAAlternative = alt.Clone
                        Dim newAlt2 As RAAlternative = alt.Clone
                        newAlt1.Funded = CDbl(IIf(alt.ID = Dependency.FirstAlternativeID, 1, 0))
                        newAlt1.FundedPartial = newAlt1.Funded
                        newAlt2.Funded = CDbl(IIf(alt.ID = Dependency.SecondAlternativeID, 1, 0))
                        newAlt2.FundedPartial = newAlt2.Funded
                        dAltsFirst.Add(newAlt1)
                        dAltsSecond.Add(newAlt2)
                    Next

                    'Creating parameters for dependency alterantives vectors

                    Dim chooseD1 As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Dependency" + i.ToString + "_1", items)
                    chooseD1.SetBinding(dAltsFirst, "Funded", "ID")
                    model.AddParameter(chooseD1)

                    Dim chooseD2 As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Dependency" + i.ToString + "_2", items)
                    chooseD2.SetBinding(dAltsSecond, "Funded", "ID")
                    model.AddParameter(chooseD2)

                    ' Adding constraints for dependencies
                    Dim dTerm As Term
                    Select Case Dependency.Value
                        Case RADependencyType.dtDependsOn
                            ' first is not funded unless second is funded
                            dTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p) - (choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p)))) >= 0
                            'dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD2(p) - choose(p) * chooseD1(p)))) >= 0
                        Case RADependencyType.dtMutuallyDependent
                            dTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) - (choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) = 0
                            'dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD1(p) - choose(p) * chooseD2(p)))) = 0
                        Case Else   'RADependencyType.dtMutuallyExclusive
                            dTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) + (choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) <= 1
                            'dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD1(p) + choose(p) * chooseD2(p)))) <= 1
                    End Select
                    Dim dependencyConstraint As Constraint = model.AddConstraint("Constraint_Dependency_" + i.ToString, dTerm)
                    i += 1
                Next
            End If

            If Settings.FundingPools Then
                If False Then
                    Dim PoolsList As New List(Of Decision)

                    For Each FP As RAFundingPool In FundingPools.Pools.Values
                        If FP.Enabled Then
                            ' for each funding pool we create a vector of alternatives that hold funding pool values for specific alt (Cost field)
                            ' Funded field will be the place for result - it will hold the amount of funded money from this pool
                            Dim fpAlts As New List(Of RAAlternative)
                            For Each raAlt As RAAlternative In Alternatives
                                Dim newAlt As RAAlternative = raAlt.Clone
                                newAlt.Cost = FP.GetAlternativeValue(newAlt.ID)
                                If newAlt.Cost = UNDEFINED_INTEGER_VALUE Then
                                    newAlt.Cost = 0
                                End If
                                newAlt.Funded = 0

                                fpAlts.Add(newAlt)
                            Next

                            ' Creating parameter for funding pool costs - vector that will store cost values for alternatives from this pool
                            Dim FPDCosts As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealNonnegative, "FPCosts" + FP.ID.ToString, items)
                            FPDCosts.SetBinding(fpAlts, "Cost", "ID")
                            model.AddParameter(FPDCosts)

                            ' Creating Decision for alternatives of the funding pool and binding it to Funded field
                            ' This is where our funding pool results will be stored
                            Dim FPD As New Decision(Domain.RealNonnegative, "FP" + FP.ID.ToString, items)
                            FPD.SetBinding(fpAlts, "Funded", "ID")
                            model.AddDecision(FPD)

                            ' Creating a list of funding pool decisions
                            PoolsList.Add(FPD)

                            ' Adding constraint - the amount of money that is used to fund alternatives from this pool cannot exceed pool limit
                            Dim fpLimitTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * FPD(p)))) <= FP.PoolLimit
                            Dim fpLimitConstraint As Constraint = model.AddConstraint("FPLimit_" + FP.ID.ToString, fpLimitTerm)

                            ' Each alternative cannot take from the funded pool more that allowed
                            ' In other words, for each alternative Funded should be less or equals funding pool cost value
                            ' Please note that we should include only Funded global altenatives - this is why we have to multiply by choose(p)
                            Dim fpIndividualLimitsTerm As Term = model.ForEach(items, Function(p) (choose(p) * FPD(p) <= FPDCosts(p)))
                            Dim fpIndividualLimitsConstraint As Constraint = model.AddConstraint("FPIndividualLimit_" + FP.ID.ToString, fpIndividualLimitsTerm)
                        End If
                    Next

                    ' Now the most difficult place

                    ' We need to set up additional constraint for funding pools:
                    ' For funded alternatives the sum of funded values from funding pools should be equal the cost of alternative
                    ' In other words, the cost of alternative is being split between funding pools
                    ' So, the sum of Funded in each row of the table should be equal Cost

                    ' When setting up constraints in MSF we need to operate with its types, like Terms, Decisions, etc.
                    ' and use internal iterators and aggregators like model.ForEach, model.Sum, etc.
                    ' Funding pools count is a dynamic value, so we have to iterate through it to somehow aggregate values from funding pools,
                    ' but there is no way to do that naturally in MSF
                    ' After trying many things I found a special class in MSF called SumTermBuilder
                    ' This class allows to aggregate values in such situations
                    If PoolsList.Count > 0 Then
                        ' Setting up SumTermBuilder and saying how many values will be aggregated
                        Dim stb As New SumTermBuilder(PoolsList.Count)
                        For i As Integer = 0 To Alternatives.Count - 1
                            ' Summing up all funded values for each row
                            For Each fpd As Decision In PoolsList
                                stb.Add(fpd(i) * choose(i))
                            Next
                            ' Adding constraint - the sum of funded should be equal alternative cost
                            model.AddConstraint("FPAltLimit_" + i.ToString, stb.ToTerm = Alternatives(i).Cost)
                            stb.Clear()
                        Next
                    End If
                End If
            End If

            ' Adding constraint - Below budget limit
            Dim cTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * cost(p) + choosePartial(p) * costPartial(p)))) <= budget
            Dim cBudget As Constraint = model.AddConstraint("budgetlimit", cTerm)

            ' Setting up the goal - Maximize priority
            Dim gTerm As Term = model.Sum(model.ForEach(items, Function(i) (choose(i) * priority(i) + choosePartial(i) * priorityPartial(i))))
            Dim goal As Goal = model.AddGoal("goal", GoalKind.Maximize, gTerm)


            Dim solved As Boolean = True

            If tSolverExport = raSolverExport.raNone OrElse tSolverExport = raSolverExport.raReport Then
                Try
                    ' This is a call to solve the model
                    Dim solution As Microsoft.SolverFoundation.Services.Solution = context.Solve()
                    context.PropagateDecisions()
                    SolverState = raSolverState.raSolved
                    LastError = ""

                    If tSolverExport = raSolverExport.raReport AndAlso tExportData IsNot Nothing Then
                        Dim report As Microsoft.SolverFoundation.Services.Report = solution.GetReport(ReportVerbosity.All)
                        tExportData = report.ToString
                    End If

                Catch ex As Exception
                    OnException(ex)
                    solved = False
                End Try
            End If

            Try
                If tSolverExport = raSolverExport.raOML AndAlso tExportData IsNot Nothing Then context.SaveModel(FileFormat.OML, CType(tExportData, StreamWriter))
                If tSolverExport = raSolverExport.raMPS AndAlso tExportData IsNot Nothing Then context.SaveModel(FileFormat.MPS, CType(tExportData, StreamWriter))
            Catch ex As Exception
                Debug.Print(String.Format("Unable to save file {0}: {1}" + tSolverExport.ToString, ex.Message)) ' D3236
                LastError = ex.Message
            End Try

            Debug.Print("RA Solved: ")

            mFundedCost = 0
            mFundedBenefit = 0
            mFundedOriginalBenefit = 0

            ' Getting the results, calculating funded priorities, funded cost, etc.
            For Each alt As RAAlternative In Alternatives
                Dim altID As String = alt.ID
                Dim i As Integer = Alts.FindIndex(Function(a) (a.ID = altID))
                If i <> -1 Then
                    alt.Funded = CDbl(IIf(solved, IIf(alt.Must And Settings.Musts, 1, IIf(alt.IsPartial And Not Dependencies.IsAlwaysInteger(alt.ID), AltsPartial(i).FundedPartial, Alts(i).Funded)), 0))
                End If

                If alt.Funded > 0.00001 Then
                    mFundedCost += alt.Cost * alt.Funded
                    mFundedBenefit += alt.Benefit * alt.Funded
                    mFundedOriginalBenefit += alt.BenefitOriginal * alt.Funded
                Else
                    alt.Funded = 0
                End If

                Debug.Print(alt.Name + ": " + alt.Funded.ToString)
            Next
            Debug.Print("Funded: " + FundedCost.ToString)
            Debug.Print("Benefits: " + FundedBenefit.ToString)

            Debug.Print("Alts:")
            For Each alt As RAAlternative In Alts
                Debug.Print(alt.Name + ": " + alt.Funded.ToString + " - " + alt.FundedPartial.ToString)
            Next

            Debug.Print("Alts partial:")
            For Each alt As RAAlternative In AltsPartial
                Debug.Print(alt.Name + ": " + alt.Funded.ToString + " - " + alt.FundedPartial.ToString)
            Next

            Debug.Print("Is partial param:")
            For Each alt As RAAlternative In AltsIsPartial
                Debug.Print(alt.Name + ": " + alt.Funded.ToString)
            Next


            Return True
        End Function

        'Private Function Solve_MSF2(tSolverExport As raSolverExport, Optional ByRef tExportData As Object = Nothing) As Boolean ' D3193 + A0923 + D3236
        '    If ProjectManager Is Nothing Then Return False

        '    ResetFunded()

        '    Dim Alts As New List(Of RAAlternative)
        '    Dim AltsPartial As New List(Of RAAlternative)
        '    Dim AltsPartialY As New List(Of RAAlternative)
        '    Dim AltsIsPartial As New List(Of RAAlternative)
        '    Dim AltsIsInteger As New List(Of RAAlternative)

        '    For Each alt As RAAlternative In Alternatives
        '        ' if we are ignoring risks, we should use BenefitOriginal value
        '        ' if we are using risks, we should use adjusted value
        '        alt.Benefit = CDbl(IIf(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal))

        '        ' alternative cannot be partial if it is included in one of the dependencies
        '        Dim isPartial As Boolean = (alt.IsPartial And Not Dependencies.IsAlwaysInteger(alt.ID))
        '        If (alt.Must And Settings.Musts) Then
        '            isPartial = False
        '        End If

        '        Dim newAlt As RAAlternative = alt.Clone

        '        If newAlt.MustNot And Settings.MustNots Then
        '            newAlt.Benefit = 0
        '            newAlt.BenefitPartial = 0
        '            newAlt.Cost = 0
        '            newAlt.CostPartial = 0
        '            newAlt.MinPercent = 0
        '        Else
        '            ' for integer alternatives we will use Benefit and Cost fields. BenefitPartial and CostPartial fields will be zeros.
        '            ' for partial alternatives we will use BenefitPartial and CostPartial fields. Benefit and Cost fields will be zeros.
        '            newAlt.Benefit = CDbl(IIf(alt.Must And Settings.Musts, alt.Benefit, IIf(isPartial, 0, alt.Benefit)))
        '            newAlt.BenefitPartial = CDbl(IIf(alt.Must And Settings.Musts, 0, IIf(isPartial, alt.Benefit, 0)))
        '            newAlt.Cost = CDbl(IIf(alt.Must And Settings.Musts, alt.Cost, IIf(isPartial, 0, alt.Cost)))
        '            newAlt.CostPartial = CDbl(IIf(alt.Must And Settings.Musts, 0, IIf(isPartial, alt.Cost, 0)))
        '        End If

        '        ' Funded field is a place where result for integers will be stored
        '        ' FundedPartial - result for partial
        '        newAlt.Funded = 0
        '        newAlt.FundedPartial = 0

        '        Dim IsPartialAlt As RAAlternative = alt.Clone
        '        IsPartialAlt.Funded = CInt(IIf(isPartial, 1, 0))
        '        AltsIsPartial.Add(IsPartialAlt)

        '        Dim IsIntegerAlt As RAAlternative = alt.Clone
        '        IsIntegerAlt.Funded = CInt(IIf(isPartial, 0, 1))
        '        AltsIsInteger.Add(IsIntegerAlt)

        '        Alts.Add(newAlt.Clone)
        '        AltsPartial.Add(newAlt.Clone)
        '        AltsPartialY.Add(newAlt.Clone)
        '    Next

        '    Dim budget As Double = BudgetLimit

        '    Dim context As SolverContext = SolverContext.GetContext()
        '    Dim model As Model = context.CurrentModel
        '    If model IsNot Nothing Then context.ClearModel()
        '    model = context.CreateModel()

        '    Dim items As Microsoft.SolverFoundation.Services.Set = New Microsoft.SolverFoundation.Services.Set(Domain.Any, "items")

        '    ' Creating parameter for priority values and binding it to Benefit field
        '    Dim priority As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "priority", items)
        '    priority.SetBinding(Alts, "Benefit", "ID")
        '    model.AddParameter(priority)

        '    ' Creating parameter for partial priority values and binding it to BenefitPartial field
        '    Dim priorityPartial As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "priorityPartial", items)
        '    priorityPartial.SetBinding(AltsPartial, "BenefitPartial", "ID")
        '    model.AddParameter(priorityPartial)

        '    ' Creating parameter for cost values and binding it to Cost field
        '    Dim cost As New Microsoft.SolverFoundation.Services.Parameter(Domain.Real, "cost", items)
        '    cost.SetBinding(Alts, "Cost", "ID")
        '    model.AddParameter(cost)

        '    ' Creating parameter for cost partial values and binding it to CostPartial field
        '    Dim costPartial As New Microsoft.SolverFoundation.Services.Parameter(Domain.Real, "costPartial", items)
        '    costPartial.SetBinding(AltsPartial, "CostPartial", "ID")
        '    model.AddParameter(costPartial)

        '    ' Creating parameter for minimum percent values for partials and binding it to MinPercent field
        '    Dim minPercent As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "minPercent", items)
        '    minPercent.SetBinding(Alts, "MinPercent", "ID")
        '    model.AddParameter(minPercent)

        '    ' Creating parameter to determine whether alternative is integer or partial
        '    Dim IsPartialParam As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "IsPartialParam", items)
        '    IsPartialParam.SetBinding(AltsIsPartial, "Funded", "ID")
        '    model.AddParameter(IsPartialParam)

        '    Dim IsIntegerParam As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "IsIntegerParam", items)
        '    IsIntegerParam.SetBinding(AltsIsInteger, "Funded", "ID")
        '    model.AddParameter(IsIntegerParam)

        '    ' Creating 2 Decisions (basically, vectors that will hold result values) - one for integers, one for partials
        '    Dim choose As New Decision(Domain.IntegerRange(0, 1), "choose", items)
        '    choose.SetBinding(Alts, "Funded", "ID")
        '    model.AddDecision(choose)

        '    Dim choosePartial As New Decision(Domain.RealRange(0, 1), "choosePartial", items)
        '    choosePartial.SetBinding(AltsPartial, "FundedPartial", "ID")
        '    model.AddDecision(choosePartial)

        '    ' This is additional parameter for partials
        '    ' We need it because partial alternative can be either not funded at all or funded higher than min percent value
        '    ' So it is not continuous variable
        '    Dim chooseY As New Decision(Domain.IntegerRange(0, 1), "chooseY", items)
        '    chooseY.SetBinding(AltsPartialY, "FundedPartial", "ID")
        '    model.AddDecision(chooseY)


        '    ' Adding min partial constraints
        '    Dim cMinPercent1 As Constraint = model.AddConstraint("MinimumPercent1", model.ForEach(items, Function(p) (choosePartial(p) <= chooseY(p))))
        '    Dim cMinPercent2 As Constraint = model.AddConstraint("MinimumPercent2", model.ForEach(items, Function(p) (choosePartial(p) >= minPercent(p) * chooseY(p))))

        '    If Settings.Musts Or Settings.MustNots Then
        '        Dim mustAlts As New List(Of RAAlternative)
        '        Dim mustnotAlts As New List(Of RAAlternative)
        '        Dim mustCount As Integer = 0
        '        Dim mustnotCount As Integer = 0

        '        ' Creating a vector of MUST alternatives (mustAlts). Funded property = 1 if alternative is a MUST and 0 otherwise
        '        ' The same for MUSTNOT alternatives (mustnotAlts)
        '        For Each alt As RAAlternative In Alts
        '            Dim newAltMust As RAAlternative = alt.Clone
        '            newAltMust.Funded = CInt(IIf(alt.Must And Settings.Musts, 1, 0))
        '            mustAlts.Add(newAltMust)
        '            If alt.Must And Settings.Musts Then mustCount += 1

        '            Dim newAltMustNot As RAAlternative = alt.Clone
        '            newAltMustNot.Funded = CInt(IIf(alt.MustNot And Settings.MustNots, 1, 0))
        '            mustnotAlts.Add(newAltMustNot)
        '            If alt.MustNot And Settings.MustNots Then
        '                mustnotCount += 1
        '            End If
        '        Next

        '        If Settings.Musts And (mustCount > 0) Then
        '            ' Creating parameter for MUST alternatives and binding it to our list of MUST alts
        '            Dim chooseMust As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "MustParam", items)
        '            chooseMust.SetBinding(mustAlts, "Funded", "ID")
        '            model.AddParameter(chooseMust)

        '            ' Here we say that all must alternatives should be fully funded
        '            Dim MustTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * IsIntegerParam(p) * chooseMust(p)))) = mustCount
        '            Dim mustConstraint As Constraint = model.AddConstraint("MustConstraint", MustTerm)
        '        End If

        '        If Settings.MustNots And (mustnotCount > 0) Then
        '            ' Creating parameter for MUST alternatives and binding it to our list of MUSTNOT alts
        '            Dim chooseMustNot As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "MustNotParam", items)
        '            chooseMustNot.SetBinding(mustnotAlts, "Funded", "ID")
        '            model.AddParameter(chooseMustNot)

        '            ' Here we say that all mustnot alternatives should NOT be funded
        '            ' We should have written "= 0" instead of "<= 0.00001", but for some reason it doesn't work with MSF
        '            Dim MustNotTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseMustNot(p)))) <= 0.00001
        '            Dim mustnotConstraint As Constraint = model.AddConstraint("MustNotConstraint", MustNotTerm)

        '            Dim MustNotTermPartial As Term = model.Sum(model.ForEach(items, Function(p) (choosePartial(p) * chooseMustNot(p)))) <= 0.00001
        '            Dim mustnotConstraintPartial As Constraint = model.AddConstraint("MustNotConstraintPartial", MustNotTermPartial)
        '        End If
        '    End If

        '    If Settings.Groups Then
        '        Dim i As Integer = 1
        '        For Each group As RAGroup In Groups.Groups.Values
        '            If group.Enabled Then
        '                Dim groupTerm As Term = Nothing

        '                ' For each groups we create a vector with alternatives from that group
        '                ' If alternative is included in this group, Funded value will be = 1, otherwise it will be 0
        '                Dim groupAlts As New List(Of RAAlternative)
        '                For Each alt As RAAlternative In Alts
        '                    Dim newAlt As RAAlternative = alt.Clone
        '                    newAlt.Funded = CDbl(IIf(group.Alternatives.ContainsKey(alt.ID), 1, 0))
        '                    newAlt.FundedPartial = newAlt.Funded
        '                    groupAlts.Add(newAlt)
        '                Next

        '                ' Creating parameter for the list of group alternatives
        '                Dim chooseGroup As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Group" + i.ToString, items)
        '                chooseGroup.SetBinding(groupAlts, "Funded", "ID")
        '                model.AddParameter(chooseGroup)

        '                ' Adding constraints for groups
        '                Select Case group.Condition
        '                    Case RAGroupCondition.gcLessOrEqualsOne
        '                        ' none of one alterantive from the group if funded
        '                        groupTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseGroup(p)))) <= 1
        '                    Case RAGroupCondition.gcEqualsOne
        '                        ' exactly one alterantive from the group is funded
        '                        groupTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseGroup(p)))) = 1
        '                    Case RAGroupCondition.gcGreaterOrEqualsOne
        '                        ' at least one alternative from the group is funded
        '                        groupTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseGroup(p)))) >= 1
        '                End Select
        '                Dim groupConstraint As Constraint = model.AddConstraint("Constraint_Group" + i.ToString, groupTerm)
        '                i += 1
        '            End If
        '        Next
        '    End If

        '    If Settings.CustomConstraints Then
        '        Dim i As Integer = 1
        '        For Each Constraint As RAConstraint In Constraints.Constraints.Values
        '            If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
        '                ' for each custom constraint we create a vector that holds custom constrain values in the field Cost
        '                Dim ccAlts As New List(Of RAAlternative)
        '                For Each alt As RAAlternative In Alts
        '                    Dim newAlt As RAAlternative = alt.Clone
        '                    Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
        '                    newAlt.Cost = CDbl(IIf(value = UNDEFINED_INTEGER_VALUE, 0, value))
        '                    ccAlts.Add(newAlt)
        '                Next

        '                ' Creating parameter for custom constraint and binding it to Cost field
        '                Dim chooseCC As New Microsoft.SolverFoundation.Services.Parameter(Domain.Real, "CC" + i.ToString, items)
        '                chooseCC.SetBinding(ccAlts, "Cost", "ID")
        '                model.AddParameter(chooseCC)

        '                If Constraint.MinValueSet Then
        '                    ' Adding constraint for custom constraint for Minimum Value
        '                    Dim ccTermMin As Term = model.GreaterEqual(model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MinValue)
        '                    Dim ccConstraintMin As Constraint = model.AddConstraint("Constraint_Min_" + i.ToString, ccTermMin)
        '                End If
        '                If Constraint.MaxValueSet Then
        '                    ' Adding constraint for custom constraint for Maximum Value
        '                    Dim ccTermMax As Term = model.LessEqual(model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)))), Constraint.MaxValue)
        '                    Dim ccConstraintMax As Constraint = model.AddConstraint("Constraint_Max_" + i.ToString, ccTermMax)
        '                End If

        '                ' Please note that we were using additional parameter IsPartialParam:
        '                ' (choose(p) + choosePartial(p) * IsPartialParam(p)) * chooseCC(p)
        '                ' without this "* IsPartialParam(p)" it didn't work for some reason
        '                ' there were some non-zero values in some of choosePartial items that shouldn't have been there (for integer alternatives)

        '                i += 1
        '            End If
        '        Next
        '    End If

        '    If Settings.Dependencies Then
        '        Dim i As Integer = 1
        '        For Each Dependency As RADependency In Dependencies.Dependencies
        '            Dim dAltsFirst As New List(Of RAAlternative)
        '            Dim dAltsSecond As New List(Of RAAlternative)

        '            ' Dependency requires 2 alternatives
        '            ' Creating 2 vectors of alternatives for dependency
        '            For Each alt As RAAlternative In Alts
        '                Dim newAlt1 As RAAlternative = alt.Clone
        '                Dim newAlt2 As RAAlternative = alt.Clone
        '                newAlt1.Funded = CDbl(IIf(alt.ID = Dependency.FirstAlternativeID, 1, 0))
        '                newAlt1.FundedPartial = newAlt1.Funded
        '                newAlt2.Funded = CDbl(IIf(alt.ID = Dependency.SecondAlternativeID, 1, 0))
        '                newAlt2.FundedPartial = newAlt2.Funded
        '                dAltsFirst.Add(newAlt1)
        '                dAltsSecond.Add(newAlt2)
        '            Next

        '            'Creating parameters for dependency alterantives vectors

        '            Dim chooseD1 As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Dependency" + i.ToString + "_1", items)
        '            chooseD1.SetBinding(dAltsFirst, "Funded", "ID")
        '            model.AddParameter(chooseD1)

        '            Dim chooseD2 As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Dependency" + i.ToString + "_2", items)
        '            chooseD2.SetBinding(dAltsSecond, "Funded", "ID")
        '            model.AddParameter(chooseD2)

        '            ' Adding constraints for dependencies
        '            Dim dTerm As Term
        '            Select Case Dependency.Value
        '                Case RADependencyType.dtDependsOn
        '                    ' first is not funded unless second is funded
        '                    dTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p) - (choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p)))) >= 0
        '                    'dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD2(p) - choose(p) * chooseD1(p)))) >= 0
        '                Case RADependencyType.dtMutuallyDependent
        '                    dTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) - (choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) = 0
        '                    'dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD1(p) - choose(p) * chooseD2(p)))) = 0
        '                Case Else   'RADependencyType.dtMutuallyExclusive
        '                    dTerm = model.Sum(model.ForEach(items, Function(p) ((choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD1(p) + (choose(p) * IsIntegerParam(p) + choosePartial(p) * IsPartialParam(p)) * chooseD2(p)))) <= 1
        '                    'dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD1(p) + choose(p) * chooseD2(p)))) <= 1
        '            End Select
        '            Dim dependencyConstraint As Constraint = model.AddConstraint("Constraint_Dependency_" + i.ToString, dTerm)
        '            i += 1
        '        Next
        '    End If

        '    If Settings.FundingPools Then
        '        If False Then
        '            Dim PoolsList As New List(Of Decision)

        '            For Each FP As RAFundingPool In FundingPools.Pools.Values
        '                If FP.Enabled Then
        '                    ' for each funding pool we create a vector of alternatives that holds funding pool values for specific alt (Cost field)
        '                    ' Funded field will be the place for result - it will hold the amount of funded money from this pool
        '                    Dim fpAlts As New List(Of RAAlternative)
        '                    For Each raAlt As RAAlternative In Alternatives
        '                        Dim newAlt As RAAlternative = raAlt.Clone
        '                        newAlt.Cost = FP.GetAlternativeValue(newAlt.ID)
        '                        If newAlt.Cost = UNDEFINED_INTEGER_VALUE Then
        '                            newAlt.Cost = 0
        '                        End If
        '                        newAlt.Funded = 0

        '                        fpAlts.Add(newAlt)
        '                    Next

        '                    ' Creating parameter for funding pool costs - vector that will store cost values for alternatives from this pool
        '                    Dim FPDCosts As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealNonnegative, "FPCosts" + FP.ID.ToString, items)
        '                    FPDCosts.SetBinding(fpAlts, "Cost", "ID")
        '                    model.AddParameter(FPDCosts)

        '                    ' Creating Decision for alternatives of the funding pool and binding it to Funded field
        '                    ' This is where our funding pool results will be stored
        '                    Dim FPD As New Decision(Domain.RealNonnegative, "FP" + FP.ID.ToString, items)
        '                    FPD.SetBinding(fpAlts, "Funded", "ID")
        '                    model.AddDecision(FPD)

        '                    ' Creating a list of funding pool decisions
        '                    PoolsList.Add(FPD)

        '                    ' Adding constraint - the amount of money that is used to fund alternatives from this pool cannot exceed pool limit
        '                    Dim fpLimitTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * FPD(p)))) <= FP.PoolLimit
        '                    Dim fpLimitConstraint As Constraint = model.AddConstraint("FPLimit_" + FP.ID.ToString, fpLimitTerm)

        '                    ' Each alternative cannot take from the funded pool more that allowed
        '                    ' In other words, for each alternative Funded should be less or equals funding pool cost value
        '                    ' Please note that we should include only Funded global altenatives - this is why we have to multiply by choose(p)
        '                    Dim fpIndividualLimitsTerm As Term = model.ForEach(items, Function(p) (choose(p) * FPD(p) <= FPDCosts(p)))
        '                    Dim fpIndividualLimitsConstraint As Constraint = model.AddConstraint("FPIndividualLimit_" + FP.ID.ToString, fpIndividualLimitsTerm)
        '                End If
        '            Next

        '            ' Now the most difficult place

        '            ' We need to set up additional constraint for funding pools:
        '            ' For funded alternatives the sum of funded values from funding pools should be equal the cost of alternative
        '            ' In other words, the cost of alternative is being split between funding pools
        '            ' So, the sum of Funded in each row of the table should be equal Cost

        '            ' When setting up constraints in MSF we need to operate with its types, like Terms, Decisions, etc.
        '            ' and use internal iterators and aggregators like model.ForEach, model.Sum, etc.
        '            ' Funding pools count is a dynamic value, so we have to iterate through it to somehow aggregate values from funding pools,
        '            ' but there is no way to do that naturally in MSF
        '            ' After trying many things I found a special class in MSF called SumTermBuilder
        '            ' This class allows to aggregate values in such situations

        '            If PoolsList.Count > 0 Then
        '                ' Setting up SumTermBuilder and saying how many values will be aggregated
        '                Dim stb As New SumTermBuilder(PoolsList.Count)
        '                For i As Integer = 0 To Alternatives.Count - 1
        '                    ' Summing up all funded values for each row
        '                    For Each fpd As Decision In PoolsList
        '                        stb.Add(fpd(i))
        '                    Next
        '                    ' Adding constraint - the sum of funded should be equal alternative cost
        '                    model.AddConstraint("FPAltLimit_" + i.ToString, stb.ToTerm = Alternatives(i).Cost)
        '                    stb.Clear()
        '                Next
        '            End If
        '        End If
        '    End If

        '    ' Adding constraint - Below budget limit
        '    Dim cTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * cost(p) + choosePartial(p) * costPartial(p)))) <= budget
        '    Dim cBudget As Constraint = model.AddConstraint("budgetlimit", cTerm)

        '    ' Setting up the goal - Maximize priority
        '    Dim gTerm As Term = model.Sum(model.ForEach(items, Function(i) (choose(i) * priority(i) + choosePartial(i) * priorityPartial(i))))
        '    Dim goal As Goal = model.AddGoal("goal", GoalKind.Maximize, gTerm)


        '    Dim solved As Boolean = True

        '    If tSolverExport = raSolverExport.raNone OrElse tSolverExport = raSolverExport.raReport Then
        '        Try
        '            ' This is a call to solve the model
        '            Dim solution As Microsoft.SolverFoundation.Services.Solution = context.Solve()
        '            context.PropagateDecisions()
        '            SolverState = raSolverState.raSolved
        '            LastError = ""

        '            If tSolverExport = raSolverExport.raReport AndAlso tExportData IsNot Nothing Then
        '                Dim report As Microsoft.SolverFoundation.Services.Report = solution.GetReport(ReportVerbosity.All)
        '                tExportData = report.ToString
        '            End If

        '        Catch ex As Exception
        '            OnException(ex)
        '            solved = False
        '        End Try
        '    End If

        '    Try
        '        If tSolverExport = raSolverExport.raOML AndAlso tExportData IsNot Nothing Then context.SaveModel(FileFormat.OML, CType(tExportData, StreamWriter))
        '        If tSolverExport = raSolverExport.raMPS AndAlso tExportData IsNot Nothing Then context.SaveModel(FileFormat.MPS, CType(tExportData, StreamWriter))
        '    Catch ex As Exception
        '        Debug.Print(String.Format("Unable to save file {0}: {1}" + tSolverExport.ToString, ex.Message)) ' D3236
        '        LastError = ex.Message
        '    End Try

        '    Debug.Print("RA Solved: ")

        '    mFundedCost = 0
        '    mFundedBenefit = 0
        '    mFundedOriginalBenefit = 0

        '    ' Getting the results, calculating funded priorities, funded cost, etc.
        '    For Each alt As RAAlternative In Alternatives
        '        Dim altID As String = alt.ID
        '        Dim i As Integer = Alts.FindIndex(Function(a) (a.ID = altID))
        '        If i <> -1 Then
        '            alt.Funded = CDbl(IIf(solved, IIf(alt.Must And Settings.Musts, 1, IIf(alt.IsPartial And Not Dependencies.IsAlwaysInteger(alt.ID), AltsPartial(i).FundedPartial, Alts(i).Funded)), 0))
        '        End If

        '        If alt.Funded > 0.00001 Then
        '            mFundedCost += alt.Cost * alt.Funded
        '            mFundedBenefit += alt.Benefit * alt.Funded
        '            mFundedOriginalBenefit += alt.BenefitOriginal * alt.Funded
        '        Else
        '            alt.Funded = 0
        '        End If

        '        Debug.Print(alt.Name + ": " + alt.Funded.ToString)
        '    Next
        '    Debug.Print("Funded: " + FundedCost.ToString)
        '    Debug.Print("Benefits: " + FundedBenefit.ToString)

        '    Debug.Print("Alts:")
        '    For Each alt As RAAlternative In Alts
        '        Debug.Print(alt.Name + ": " + alt.Funded.ToString + " - " + alt.FundedPartial.ToString)
        '    Next

        '    Debug.Print("Alts partial:")
        '    For Each alt As RAAlternative In AltsPartial
        '        Debug.Print(alt.Name + ": " + alt.Funded.ToString + " - " + alt.FundedPartial.ToString)
        '    Next

        '    Debug.Print("Is partial param:")
        '    For Each alt As RAAlternative In AltsIsPartial
        '        Debug.Print(alt.Name + ": " + alt.Funded.ToString)
        '    Next


        '    Return True
        'End Function

        'Public Function SolveOld() As Boolean
        '    If ProjectManager Is Nothing Then Return False

        '    ResetFunded()

        '    Dim Alts As New List(Of RAAlternative)
        '    Dim AltsPartial As New List(Of RAAlternative)

        '    ' process must/mustnots, setting cost, costpartial
        '    For Each alt As RAAlternative In Alternatives
        '        If Not (alt.MustNot And Settings.MustNots Or alt.Must And Settings.Musts) Then
        '            alt.Benefit = CDbl(IIf(Settings.Risks, alt.BenefitOriginal * (1 - alt.Risk), alt.BenefitOriginal))  ' D2882

        '            Dim newAlt As RAAlternative = alt.Clone
        '            newAlt.Benefit = CDbl(IIf(alt.IsPartial, 0, alt.Benefit))   ' D2882
        '            newAlt.BenefitPartial = CDbl(IIf(alt.IsPartial, alt.Benefit, 0))    ' D2882
        '            newAlt.Cost = CDbl(IIf(alt.IsPartial, 0, alt.Cost)) ' D2882
        '            newAlt.CostPartial = CDbl(IIf(alt.IsPartial, alt.Cost, 0))  ' D2882

        '            newAlt.Funded = 0
        '            newAlt.FundedPartial = 0

        '            Alts.Add(newAlt)
        '            AltsPartial.Add(newAlt)
        '        End If
        '    Next

        '    ' adjust budget
        '    Dim budget As Double = BudgetLimit
        '    For Each alt As RAAlternative In Alternatives
        '        If alt.Must And Settings.Musts Then budget -= alt.Cost
        '    Next

        '    Dim context As SolverContext = SolverContext.GetContext()
        '    Dim model As Model = context.CurrentModel
        '    If model IsNot Nothing Then context.ClearModel()
        '    model = context.CreateModel()

        '    Dim items As Microsoft.SolverFoundation.Services.Set = New Microsoft.SolverFoundation.Services.Set(Domain.Any, "items")

        '    ' Benefit (priority) of each alternative
        '    Dim priority As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealNonnegative, "priority", items)
        '    priority.SetBinding(Alts, "Benefit", "ID")
        '    model.AddParameter(priority)

        '    Dim priorityPartial As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealNonnegative, "priorityPartial", items)
        '    priorityPartial.SetBinding(AltsPartial, "BenefitPartial", "ID")
        '    model.AddParameter(priorityPartial)

        '    ' Cost for each alternative
        '    Dim cost As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealNonnegative, "cost", items)
        '    cost.SetBinding(Alts, "Cost", "ID")
        '    model.AddParameter(cost)

        '    Dim costPartial As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealNonnegative, "costPartial", items)
        '    costPartial.SetBinding(AltsPartial, "CostPartial", "ID")
        '    model.AddParameter(costPartial)

        '    ' min percent
        '    Dim minPercent As New Microsoft.SolverFoundation.Services.Parameter(Domain.RealRange(0, 1), "minPercent", items)
        '    minPercent.SetBinding(Alts, "MinPercent", "ID")
        '    model.AddParameter(minPercent)


        '    Dim choose As New Decision(Domain.IntegerRange(0, 1), "choose", items)
        '    choose.SetBinding(Alts, "Funded", "ID")
        '    model.AddDecision(choose)

        '    Dim choosePartial As New Decision(Domain.RealRange(0, 1), "choosePartial", items)
        '    choosePartial.SetBinding(AltsPartial, "FundedPartial", "ID")
        '    model.AddDecision(choosePartial)

        '    ' Below budget limit.
        '    Dim cTerm As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * cost(p) + choosePartial(p) * costPartial(p)))) <= budget
        '    Dim cBudget As Constraint = model.AddConstraint("budgetlimit", cTerm)

        '    ' Add min partial constraints
        '    Dim cMinPercent As Constraint = model.AddConstraint("MinimumPercent", model.ForEach(items, Function(p) (choosePartial(p) >= minPercent(p))))

        '    If Settings.Groups Then
        '        Dim i As Integer = 1
        '        For Each group As RAGroup In Groups.Groups.Values
        '            If group.Enabled Then
        '                Dim groupTerm As Term = Nothing

        '                Dim groupAlts As New List(Of RAAlternative)
        '                For Each alt As RAAlternative In Alts
        '                    Dim newAlt As RAAlternative = alt.Clone
        '                    newAlt.Funded = CDbl(IIf(group.Alternatives.ContainsKey(alt.ID), 1, 0)) ' D2882
        '                    newAlt.FundedPartial = newAlt.Funded
        '                    groupAlts.Add(newAlt)
        '                Next

        '                Dim chooseGroup As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Group" + i.ToString, items)
        '                chooseGroup.SetBinding(groupAlts, "Funded", "ID")
        '                model.AddParameter(chooseGroup)

        '                Select Case group.Condition
        '                    Case RAGroupCondition.gcLessOrEqualsOne
        '                        groupTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseGroup(p)))) <= 1
        '                    Case RAGroupCondition.gcEqualsOne
        '                        groupTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseGroup(p)))) = 1
        '                    Case RAGroupCondition.gcGreaterOrEqualsOne
        '                        groupTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseGroup(p)))) >= 1
        '                End Select
        '                Dim groupConstraint As Constraint = model.AddConstraint("Constraint_Group" + i.ToString, groupTerm)
        '                i += 1
        '            End If
        '        Next
        '    End If

        '    If Settings.CustomConstraints Then
        '        Dim i As Integer = 1
        '        For Each Constraint As RAConstraint In Constraints.Constraints.Values
        '            If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
        '                Dim ccAlts As New List(Of RAAlternative)
        '                For Each alt As RAAlternative In Alts
        '                    Dim newAlt As RAAlternative = alt.Clone
        '                    Dim value As Double = Constraint.GetAlternativeValue(alt.ID)
        '                    newAlt.Cost = CDbl(IIf(value = UNDEFINED_INTEGER_VALUE, 0, value))  ' D2882
        '                    ccAlts.Add(newAlt)
        '                Next

        '                Dim chooseCC As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "CC" + i.ToString, items)
        '                chooseCC.SetBinding(ccAlts, "Cost", "ID")
        '                model.AddParameter(chooseCC)

        '                If Constraint.MinValueSet Then
        '                    Dim ccTermMin As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseCC(p)))) >= Constraint.MinValue
        '                    Dim ccConstraintMin As Constraint = model.AddConstraint("Constraint_Min_" + i.ToString, ccTermMin)
        '                End If
        '                If Constraint.MaxValueSet Then
        '                    Dim ccTermMax As Term = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseCC(p)))) <= Constraint.MaxValue
        '                    Dim ccConstraintMax As Constraint = model.AddConstraint("Constraint_Max_" + i.ToString, ccTermMax)
        '                End If

        '                i += 1
        '            End If
        '        Next
        '    End If

        '    If Settings.Dependencies Then
        '        Dim i As Integer = 1
        '        For Each Dependency As RADependency In Dependencies.Dependencies
        '            Dim dAltsFirst As New List(Of RAAlternative)
        '            Dim dAltsSecond As New List(Of RAAlternative)
        '            For Each alt As RAAlternative In Alts
        '                Dim newAlt1 As RAAlternative = alt.Clone
        '                Dim newAlt2 As RAAlternative = alt.Clone
        '                newAlt1.Funded = CDbl(IIf(alt.ID = Dependency.FirstAlternativeID, 1, 0))    ' D2882
        '                newAlt1.FundedPartial = newAlt1.Funded
        '                newAlt2.Funded = CDbl(IIf(alt.ID = Dependency.SecondAlternativeID, 1, 0))   ' D2882
        '                newAlt2.FundedPartial = newAlt2.Funded
        '                dAltsFirst.Add(newAlt1)
        '                dAltsSecond.Add(newAlt2)
        '            Next

        '            Dim chooseD1 As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Dependency" + i.ToString + "_1", items)
        '            chooseD1.SetBinding(dAltsFirst, "Funded", "ID")
        '            model.AddParameter(chooseD1)

        '            Dim chooseD2 As New Microsoft.SolverFoundation.Services.Parameter(Domain.IntegerRange(0, 1), "Dependency" + i.ToString + "_2", items)
        '            chooseD2.SetBinding(dAltsSecond, "Funded", "ID")
        '            model.AddParameter(chooseD2)

        '            Dim dTerm As Term
        '            Select Case Dependency.Value
        '                Case RADependencyType.dtDependsOn
        '                    ' first is not funded unless second is funded
        '                    dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD2(p) - choose(p) * chooseD1(p)))) >= 0
        '                Case RADependencyType.dtMutuallyDependent
        '                    dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD1(p) - choose(p) * chooseD2(p)))) = 0
        '                Case Else   ' RADependencyType.dtMutuallyExclusive
        '                    dTerm = model.Sum(model.ForEach(items, Function(p) (choose(p) * chooseD1(p) + choose(p) * chooseD2(p)))) <= 1
        '            End Select
        '            Dim dependencyConstraint As Constraint = model.AddConstraint("Constraint_Dependency_" + i.ToString, dTerm)
        '            i += 1
        '        Next
        '    End If

        '    ' Maximum priority
        '    Dim gTerm As Term = model.Sum(model.ForEach(items, Function(i) (choose(i) * priority(i) + choosePartial(i) * priorityPartial(i))))
        '    Dim goal As Goal = model.AddGoal("goal", GoalKind.Maximize, gTerm)

        '    Dim solved As Boolean = True

        '    Try
        '        Dim solution As Microsoft.SolverFoundation.Services.Solution = context.Solve()  ' D2882
        '        context.PropagateDecisions()
        '        SolverState = raSolverState.raSolved
        '        LastError = ""  ' D2838
        '    Catch ex As Exception
        '        OnException(ex) ' D3228
        '        solved = False
        '    End Try

        '    'Dim selected = From d In Alts
        '    '               Where d.Funded = 1
        '    '               Select d

        '    Debug.Print("RA Solved: ")

        '    mFundedCost = 0
        '    mFundedBenefit = 0
        '    mFundedOriginalBenefit = 0  ' D2941
        '    For Each alt As RAAlternative In Alternatives
        '        If alt.Must And Settings.Musts Then
        '            alt.Funded = CDbl(IIf(solved, 1, 0))    ' D2882
        '        Else
        '            If alt.MustNot And Settings.MustNots Then
        '                alt.Funded = 0
        '            Else
        '                Dim altID As String = alt.ID
        '                Dim i As Integer = Alts.FindIndex(Function(a) (a.ID = altID))
        '                If i <> -1 Then
        '                    alt.Funded = CDbl(IIf(solved, IIf(alt.IsPartial, AltsPartial(i).FundedPartial, Alts(i).Funded), 0)) ' D2882
        '                End If
        '            End If
        '        End If
        '        If alt.Funded > 0 Then
        '            mFundedCost += alt.Cost * alt.Funded
        '            mFundedBenefit += alt.Benefit * alt.Funded
        '            mFundedOriginalBenefit += alt.BenefitOriginal * alt.Funded  ' D2941
        '        End If

        '        Debug.Print(alt.Name + ": " + alt.Funded.ToString)
        '    Next
        '    Debug.Print("Funded: " + FundedCost.ToString)
        '    Debug.Print("Benefits: " + FundedBenefit.ToString)

        '    Return True
        'End Function

    End Class

End Namespace
