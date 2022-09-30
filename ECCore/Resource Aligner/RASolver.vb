Option Strict On    ' D2882

Imports ECCore

Namespace Canvas

    ' -D6475
    '<Serializable()> Public Class InfeasibilityResult
    '    Public Property RemovedConstraints As New List(Of ConstraintInfo)
    '    Public Property OptimalValue As Double
    '    Public Property Scenario As RAScenario
    'End Class

    <Serializable()> Public Enum AllocationMode
        amOptimization = 0
        amBenefitfCost = 1
        amFundMaxBenefit = 2
        amFundMinBenefit = 3
        amFundMaxCost = 4
        amFundMinCost = 5
        amFundMaxRanking = 6
        amFundMinRanking = 7
    End Enum

    <Serializable()> Public Class AllocationResultItem
        Public Property ID As String
        Public Property Name As String
        Public Property Funded As Double
        Public Property Benefit As Double
        Public Property Cost As Double
    End Class

    <Serializable()> Public Enum raSolverState
        raNone = 0
        raSolved = 1
        raInfeasible = 2
        raError = 3
        raExceedLimits = 4  ' D3228
    End Enum

    ' D3236 === 
    <Serializable()> Public Enum raSolverExport
        raNone = 0
        raReport = 1
        raOML = 2
        raMPS = 3
    End Enum
    ' D3236 ==

    <Serializable()> Public Enum raSolverLibrary
        'raMSF = 0  - D3888
        raXA = 1
        raGurobi = 2
        raBaron = 3
    End Enum

    <Serializable()> Public Enum ConstraintType
        ctMust = 0
        ctMustNot = 1
        ctGroup = 2
        ctDependency = 3
        ctCustomConstraintMin = 4
        ctCustomConstraintMax = 5
        ctTimePeriodsDependency = 6
        ctTimePeriodResourceMin = 7
        ctTimePeriodResourceMax = 8
        ctFundingPoolLimit = 9
        ctCost = 10
    End Enum

    <Serializable()> Public Class ConstraintInfo
        Public ID As String = ""
        Public ItemID As String = ""
        Public PeriodID As Integer
        Public Property Name As String = ""
        Public Property Description As String = ""
        Public Property Enabled As Boolean = True
        Public Property Selected As Boolean = True
        Public Property Type As ConstraintType
    End Class

    <Serializable()> Partial Public Class RASolver

        Public Property ResourceAligner As ResourceAligner
        Public Property LastError As String ' D2838
        Public Property LastErrorReal As String ' D3880

        Public Delegate Sub onAddLogEvent(sMessage As String, sComment As String)  ' D4526

        <NonSerialized>
        Public EfficientFrontierCallback As EfficientFrontierStepResultsSub = Nothing
        <NonSerialized>
        Public EfficientFrontierIsCancelledCallback As EfficientFrontierCancelSub = Nothing

        Public Property ForceSolver As Boolean = True   ' D7248
        Public Property ForcedSolver As raSolverLibrary = raSolverLibrary.raXA ' D7248

        Private mNumberOfSolutionsToFind As Integer = 10
        Private mPoolSearchMode As Integer = 0
        Private mSolutionsFound As Integer = 0

        Private mExcludeConstraintsSets As New List(Of List(Of ConstraintInfo))

        Private _SolverState As raSolverState = raSolverState.raNone     ' D2843

        Public Property SolverState As raSolverState
            Get
                Return _SolverState
            End Get
            Set(value As raSolverState)
                _SolverState = value
            End Set
        End Property

        Private _SolverLibrary As raSolverLibrary = raSolverLibrary.raBaron    ' D6464
        Public Property SolverLibrary As raSolverLibrary
            Get
                Return _SolverLibrary
            End Get
            Set(value As raSolverLibrary)
                _SolverLibrary = value
            End Set
        End Property

        Private mFundedCost As Double
        Public ReadOnly Property FundedCost As Double
            Get
                Return mFundedCost
            End Get
        End Property

        Private mFundedBenefit As Double
        Public ReadOnly Property FundedBenefit As Double
            Get
                Return mFundedBenefit
            End Get
        End Property

        ' D2941 ===
        Private mFundedOriginalBenefit As Double
        Public ReadOnly Property FundedOriginalBenefit As Double 'A0917 (typo)
            Get
                Return mFundedOriginalBenefit
            End Get
        End Property
        ' D2941 ==

        Public Property BaseCaseMaximum As Double = -1

        ' D2887 ===
        Public Function FundedConstraint(tConstrID As Integer) As Double
            Dim tConstr As RAConstraint = Constraints.GetConstraintByID(tConstrID)
            ' D2992 ===
            Dim tFunded As Double = 0
            If tConstr IsNot Nothing AndAlso (tConstr.Enabled OrElse RA_OPT_CC_EDIT_DISABLED) Then   ' D3540
                For Each tAlt As RAAlternative In Alternatives
                    Dim tVal As Double = tConstr.GetAlternativeValue(tAlt.ID)
                    If tVal <> UNDEFINED_INTEGER_VALUE Then tFunded += tAlt.Funded * tVal
                Next
            End If
            Return tFunded
            ' D2992 ==
        End Function
        ' D2887 ==

        Public ReadOnly Property TotalCost As Double
            Get
                Return Alternatives.Sum(Function(i) CDbl(If(i.Cost = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, 0, i.Cost))) 'A1381
            End Get
        End Property

        Public ReadOnly Property TotalBenefitOriginal As Double
            Get
                Return Alternatives.Sum(Function(i) i.BenefitOriginal)
            End Get
        End Property

        Public ReadOnly Property TotalBenefit As Double
            Get
                Return Alternatives.Sum(Function(i) i.Benefit)
            End Get
        End Property

        Public ReadOnly Property CurrentScenario As RAScenario
            Get
                Return ResourceAligner.Scenarios.ActiveScenario
            End Get
        End Property

        Public Property BudgetLimit As Double
            Get
                Return CurrentScenario.Budget
            End Get
            Set(value As Double)
                CurrentScenario.Budget = value
            End Set
        End Property

        Public ReadOnly Property Settings As RASettings
            Get
                Return CurrentScenario.Settings
            End Get
        End Property

        Public ReadOnly Property Constraints As RAConstraints
            Get
                Return CurrentScenario.Constraints
            End Get
        End Property

        Public ReadOnly Property Groups As RAGroups
            Get
                Return CurrentScenario.Groups
            End Get
        End Property

        Public ReadOnly Property EventGroups As RAGroups 'A1407
            Get
                Return CurrentScenario.EventGroups
            End Get
        End Property

        Public ReadOnly Property Dependencies As RADependencies
            Get
                Return CurrentScenario.Dependencies
            End Get
        End Property

        Public ReadOnly Property TimePeriodsDependencies As RADependencies 'A1161
            Get
                Return CurrentScenario.TimePeriodsDependencies
            End Get
        End Property

        Public ReadOnly Property FundingPools As RAFundingPools
            Get
                Return CurrentScenario.FundingPools
            End Get
        End Property

        Public ReadOnly Property ProjectManager As clsProjectManager
            Get
                Return ResourceAligner.ProjectManager
            End Get
        End Property

        Public ReadOnly Property Alternatives As List(Of RAAlternative)
            Get
                Return CurrentScenario.Alternatives.Where(Function(alt) alt.Enabled).ToList() 'A1411
            End Get
        End Property

        ' D2843 ===
        Public Sub ResetSolver()
            SolverState = raSolverState.raNone
            ResourceAligner.Logs = ""   ' D4516
        End Sub
        ' D2843 ==

        Public Function Solve(tSolverExport As raSolverExport, Optional tExportData As Object = Nothing, Optional LogsOutputPath As String = "", Optional CalculateBaseCase As Boolean = True) As Boolean ' D3193 + A0923 + D3236 + D7510
            ' calculate base case first
            If CalculateBaseCase Then
                ResetFunded()

                Dim oldLimit As Double = BudgetLimit
                Dim oldSettings As RASettings = Settings.Clone()

                CurrentScenario.Budget = TotalCost + 1

                Settings.CustomConstraints = False
                Settings.Dependencies = False
                Settings.FundingPools = False
                Settings.Groups = Settings.UseBaseCase AndAlso Settings.BaseCaseForGroups
                Settings.MustNots = False
                Settings.Musts = False
                'Settings.Risks = False         ' D4493
                Settings.TimePeriods = False
                Settings.ResourcesMax = False   ' D4493
                Settings.ResourcesMin = False   ' D4493

                Dim b As Boolean = SolveForInfeasibilityAnalysis
                SolveForInfeasibilityAnalysis = False
                Solve(raSolverExport.raNone, ,, False)   ' D3236 + D7510
                SolveForInfeasibilityAnalysis = b


                CurrentScenario.Budget = oldLimit

                Settings.CustomConstraints = oldSettings.CustomConstraints
                Settings.Dependencies = oldSettings.Dependencies
                Settings.FundingPools = oldSettings.FundingPools
                Settings.Groups = oldSettings.Groups
                Settings.MustNots = oldSettings.MustNots
                Settings.Musts = oldSettings.Musts
                'Settings.Risks = oldSettings.Risks ' D4493
                Settings.TimePeriods = oldSettings.TimePeriods
                Settings.ResourcesMin = oldSettings.ResourcesMin    ' D4493
                Settings.ResourcesMax = oldSettings.ResourcesMax    ' D4493

                BaseCaseMaximum = If(oldSettings.Risks, FundedBenefit, FundedOriginalBenefit) 'FundedOriginalBenefit 'A0918 + D4493 -Case 8021

                ResetFunded()
            End If

            CurrentScenario.TimePeriods.AllocateResourceValues()

            LastError = ""  ' D3880
            LastErrorReal = ""  ' D3880

            ' This example for adding the custom logs text to popup on RA Main (under Download/Export menu):
            'ResourceAligner.Logs += String.Format("Call 'Solve' with option '{0}'", SolverLibrary.ToString) ' D4516

            ' AC, see an example for write logs on RA Solve:
            'If onAddLogFunction IsNot Nothing Then onAddLogFunction("Just a test", "More details there")   ' D4526

            Dim library As raSolverLibrary = If(ForceSolver, ForcedSolver, SolverLibrary)

            Select Case library
                'Case raSolverLibrary.raMSF ' -D3888
                '    Return Solve_MSF(tSolverExport, tExportData)
                Case raSolverLibrary.raXA
                    Dim UseNewXA As Boolean = False
                    If UseNewXA Then
                        Return Solve_XA_MinCost(LogsOutputPath) ' version with minimum cost
                    Else
                        Return Solve_XA(LogsOutputPath)
                    End If
                Case raSolverLibrary.raGurobi
                    'Solve_Gurobi_Full(LogsOutputPath, , False, onAddLogFunction)
                    If OPT_GUROBI_USE_CLOUD Then   ' D3894
                        Return Solve_Gurobi(LogsOutputPath, , True) ' D4512
                    Else
                        Return Solve_Gurobi(LogsOutputPath, , False)    ' D7510
                        'Return Solve_Gurobi(LogsOutputPath) ' D3894
                    End If
                Case raSolverLibrary.raBaron
                    Solve_BARON(LogsOutputPath)
            End Select
            Return False
        End Function

        Public Function GetBaseCaseMaximum() As Double
            If ProjectManager Is Nothing Then Return -1
            ResetFunded()

            Dim oldLimit As Double = BudgetLimit
            Dim oldSettings As RASettings = Settings.Clone()

            CurrentScenario.Budget = TotalCost + 1

            Settings.CustomConstraints = False
            Settings.Dependencies = False
            Settings.FundingPools = False
            Settings.Groups = True
            Settings.MustNots = False
            Settings.Musts = False
            'Settings.Risks = False         ' D4493
            Settings.TimePeriods = False
            Settings.ResourcesMax = False   ' D4493
            Settings.ResourcesMin = False   ' D4493

            Solve(raSolverExport.raNone)   ' D3236

            CurrentScenario.Budget = oldLimit

            Settings.CustomConstraints = oldSettings.CustomConstraints
            Settings.Dependencies = oldSettings.Dependencies
            Settings.FundingPools = oldSettings.FundingPools
            Settings.Groups = oldSettings.Groups
            Settings.MustNots = oldSettings.MustNots
            Settings.Musts = oldSettings.Musts
            'Settings.Risks = oldSettings.Risks ' D4493
            Settings.TimePeriods = oldSettings.TimePeriods
            Settings.ResourcesMin = oldSettings.ResourcesMin    ' D4493
            Settings.ResourcesMax = oldSettings.ResourcesMax    ' D4493

            Dim res As Double = If(oldSettings.Risks, FundedBenefit, FundedOriginalBenefit) 'FundedOriginalBenefit 'A0918 + D4493 -Case 8021

            ResetFunded()

            Return res
        End Function

        Public Function GetMaxRelevantBudget() As Double
            If ProjectManager Is Nothing Then Return -1
            ResetFunded()

            Dim oldLimit As Double = BudgetLimit
            CurrentScenario.Budget = TotalCost + 1
            Solve(raSolverExport.raNone)   ' D3236
            CurrentScenario.Budget = oldLimit
            Dim res As Double = FundedCost

            ResetFunded()

            Return res
        End Function

        Public Sub ResetFunded()
            For Each alt As RAAlternative In Alternatives
                alt.Funded = 0
                alt.FundedPartial = 0
            Next
            For Each ResourcePool As RAFundingPools In CurrentScenario.ResourcePools.Values
                For Each FP As RAFundingPool In ResourcePool.Pools.Values
                    FP.ClearAllocatedValues()
                Next
            Next
            CurrentScenario.TimePeriods.TimePeriodResults.Clear()
            SolverState = raSolverState.raNone  ' D7033
        End Sub

        ' D3228 ===
        Private Sub OnException(ex As Exception)
            If ex.Message.Contains("The solver(s) threw an exception while solving the model") Then
                'Debug.Print("Exceed solver limit")
                SolverState = raSolverState.raExceedLimits
            Else
                'Debug.Print("Infeasible")
                SolverState = raSolverState.raInfeasible    ' D2843
            End If
            LastError = ex.Message  ' D2838
            If ex.InnerException IsNot Nothing AndAlso ex.InnerException.Message <> ex.Message Then LastError += "; " + ex.InnerException.Message
        End Sub
        ' D3228 ==

        Public Class AllocationResults
            Public Property AllocationMode As AllocationMode
            Public Property FundedBenefits As Double
            Public Property FundedCosts As Double
            Public Property Allocations As New List(Of AllocationResultItem)
        End Class

        Public Function GetAllocationsText() As String
            Dim allocations As List(Of AllocationResults) = CreateAllocations()
            Return PrintAllocationResults(allocations)
        End Function

        Public Function CreateAllocations() As List(Of AllocationResults)
            Dim Allocations As New List(Of AllocationResults)

            ' Mode = Optimization
            Dim resultsOptimized As New AllocationResults

            Dim oldSettings As RASettings = Settings.Clone()
            Settings.CustomConstraints = False
            Settings.Dependencies = False
            Settings.FundingPools = False
            Settings.Groups = False
            Settings.MustNots = False
            Settings.Musts = False
            Settings.TimePeriods = False
            Settings.ResourcesMax = False
            Settings.ResourcesMin = False
            Settings.Risks = False

            Solve(raSolverExport.raNone)

            Settings.CustomConstraints = oldSettings.CustomConstraints
            Settings.Dependencies = oldSettings.Dependencies
            Settings.FundingPools = oldSettings.FundingPools
            Settings.Groups = oldSettings.Groups
            Settings.MustNots = oldSettings.MustNots
            Settings.Musts = oldSettings.Musts
            Settings.TimePeriods = oldSettings.TimePeriods
            Settings.ResourcesMin = oldSettings.ResourcesMin
            Settings.ResourcesMax = oldSettings.ResourcesMax
            Settings.Risks = oldSettings.Risks

            Dim res As Double = If(oldSettings.Risks, FundedBenefit, FundedOriginalBenefit)

            For Each alt As RAAlternative In CurrentScenario.Alternatives
                Dim item As New AllocationResultItem
                item.ID = alt.ID
                item.Name = alt.Name
                item.Funded = alt.Funded
                item.Benefit = alt.BenefitOriginal
                item.Cost = alt.Cost
                resultsOptimized.Allocations.Add(item)
            Next

            resultsOptimized.AllocationMode = AllocationMode.amOptimization
            resultsOptimized.FundedBenefits = res
            resultsOptimized.FundedCosts = FundedCost
            Allocations.Add(resultsOptimized)

            ResetFunded()

            Allocations.Add(GetSortedAllocationResults(AllocationMode.amBenefitfCost))
            Allocations.Add(GetSortedAllocationResults(AllocationMode.amFundMaxBenefit))
            Allocations.Add(GetSortedAllocationResults(AllocationMode.amFundMinBenefit))
            Allocations.Add(GetSortedAllocationResults(AllocationMode.amFundMaxCost))
            Allocations.Add(GetSortedAllocationResults(AllocationMode.amFundMinCost))
            Allocations.Add(GetSortedAllocationResults(AllocationMode.amFundMaxRanking))
            Allocations.Add(GetSortedAllocationResults(AllocationMode.amFundMinRanking))

            PrintAllocationResults(Allocations)
            Return Allocations
        End Function

        Private Function PrintAllocationResults(results As List(Of AllocationResults)) As String
            Dim decimalsNumber As Integer = 4
            Dim sortedResults As List(Of AllocationResults) = results.OrderByDescending(Function(r) r.FundedBenefits).ToList
            Dim s As String = "=== Sub-optimal allocations ===" + vbNewLine + vbNewLine
            s += "Benefits:" + vbNewLine
            For Each result As AllocationResults In sortedResults
                s += AllocationModeToString(result.AllocationMode) + ": " + result.FundedBenefits.ToString("F" + decimalsNumber.ToString) + " (" + (result.FundedBenefits / sortedResults(0).FundedBenefits * 100).ToString("F2") + "%)" + vbNewLine
            Next

            s += vbNewLine + "=== Details ===" + vbNewLine
            For Each result As AllocationResults In sortedResults
                s += vbNewLine
                s += "Allocation results for mode '" + AllocationModeToString(result.AllocationMode) + "'" + vbNewLine
                s += "Funded benefits = " + result.FundedBenefits.ToString("F" + decimalsNumber.ToString) + vbNewLine
                s += "Funded costs = " + result.FundedCosts.ToString + vbNewLine
                s += "-- Allocations: " + vbNewLine
                For Each item As AllocationResultItem In result.Allocations
                    s += item.Name + ": " + "Funded = " + If(item.Funded > 0, "True", "False") + " (Benefit = " + item.Benefit.ToString("F" + decimalsNumber.ToString) + " ; Cost = " + item.Cost.ToString + If(result.AllocationMode = AllocationMode.amBenefitfCost, "; Benefit / Cost = " + (item.Benefit / item.Cost).ToString("F" + decimalsNumber.ToString) + ")", ")") + vbNewLine
                Next
                'Debug.Print(s)
            Next
            Return s
        End Function

        Public Function AllocationModeToString(Mode As AllocationMode) As String
            Select Case Mode
                Case AllocationMode.amOptimization
                    Return "Optimized"                          ' D4608 "Optimization"
                Case AllocationMode.amBenefitfCost
                    Return "Sort by Benefit/Cost ratio and Allocate"     ' D4608 "Benefit / Cost"
                Case AllocationMode.amFundMaxBenefit
                    Return "Sort by Benefits and Allocate"      ' D4608 "Fund maximum benefit first"
                Case AllocationMode.amFundMaxCost
                    Return "Sort by Costs and Allocate"         ' D4608 "Fund maximum cost first"
                Case AllocationMode.amFundMinBenefit
                    Return "Fund minimum benefit first"
                Case AllocationMode.amFundMinCost
                    Return "Fund minimum cost first"
                Case AllocationMode.amFundMaxRanking
                    Return "Fund maximum rank first"
                Case AllocationMode.amFundMinRanking
                    Return "Fund minimum rank first"
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function GetSortedAllocationResults(Mode As AllocationMode) As AllocationResults
            Dim result As New AllocationResults

            Dim sortedList As List(Of RAAlternative) = Nothing
            Select Case Mode
                Case AllocationMode.amBenefitfCost
                    sortedList = CurrentScenario.Alternatives.OrderByDescending(Function(a) (a.Benefit / a.Cost)).ToList
                Case AllocationMode.amFundMaxBenefit
                    sortedList = CurrentScenario.Alternatives.OrderByDescending(Function(a) (a.Benefit)).ToList
                Case AllocationMode.amFundMaxCost
                    sortedList = CurrentScenario.Alternatives.OrderByDescending(Function(a) (a.Cost)).ToList
                Case AllocationMode.amFundMinBenefit
                    sortedList = CurrentScenario.Alternatives.OrderBy(Function(a) (a.Benefit)).ToList
                Case AllocationMode.amFundMinCost
                    sortedList = CurrentScenario.Alternatives.OrderBy(Function(a) (a.Cost)).ToList
                Case AllocationMode.amFundMaxRanking
                    sortedList = CurrentScenario.Alternatives.OrderByDescending(Function(a) (a.SortOrder)).ToList
                Case AllocationMode.amFundMinRanking
                    sortedList = CurrentScenario.Alternatives.OrderBy(Function(a) (a.SortOrder)).ToList
                Case Else
                    Return Nothing
            End Select

            Dim fCost As Double = 0
            Dim fBenefit As Double = 0

            Dim i As Integer = 0
            Dim b As Boolean = False
            While i < sortedList.Count
                Dim item As New AllocationResultItem
                item.ID = sortedList(i).ID
                item.Name = sortedList(i).Name

                If fCost + sortedList(i).Cost <= CurrentScenario.Budget Then
                    If Not b Then
                        item.Funded = 1
                        fCost += sortedList(i).Cost
                        fBenefit += sortedList(i).BenefitOriginal
                    End If
                Else
                    item.Funded = 0
                    b = True
                End If

                item.Benefit = sortedList(i).BenefitOriginal
                item.Cost = sortedList(i).Cost
                result.Allocations.Add(item)
                i += 1
            End While
            result.AllocationMode = Mode
            result.FundedCosts = fCost
            result.FundedBenefits = fBenefit
            Return result
        End Function

        Public Function GetConstraintsList() As List(Of ConstraintInfo)
            Dim res As New List(Of ConstraintInfo)

            If Settings.Musts Then
                For Each alt As RAAlternative In CurrentScenario.Alternatives
                    If alt.Must Then
                        Dim C As New ConstraintInfo
                        C.ID = "mustVar_" + alt.ID
                        C.ItemID = alt.ID
                        C.Type = ConstraintType.ctMust
                        C.Name = "Must for '" + alt.Name + "'"
                        'C.Description = "'" + alt.Name + "' must be funded"    ' -D6475
                        res.Add(C)
                    End If
                Next
            End If

            If Settings.MustNots Then
                For Each alt As RAAlternative In CurrentScenario.Alternatives
                    If alt.MustNot Then
                        Dim C As New ConstraintInfo
                        C.ID = "mustNotVar_" + alt.ID
                        C.ItemID = alt.ID
                        C.Type = ConstraintType.ctMustNot
                        C.Name = "Must not for '" + alt.Name + "'"
                        'C.Description = "'" + alt.Name + "' must not be funded"    ' -D6475
                        res.Add(C)
                    End If
                Next
            End If

            If Settings.Groups Then
                For Each group As RAGroup In Groups.Groups.Values
                    Dim filteredAlts As List(Of RAAlternative) = group.GetFilteredAlternatives(CurrentScenario.Alternatives)
                    If group.Enabled And filteredAlts.Count > 1 Then
                        Dim C As New ConstraintInfo
                        C.ID = "gVar_" + group.ID.ToString
                        C.ItemID = group.ID
                        C.Type = ConstraintType.ctGroup
                        C.Name = "Group '" + group.Name + "'"
                        'C.Description = C.Name     ' -D6475
                        res.Add(C)
                    End If
                Next
            End If

            If Settings.Dependencies Then
                Dim i As Integer = 1
                For Each Dependency As RADependency In CurrentScenario.GetAvailableDependencies
                    If Dependency.Enabled Then
                        Dim C As New ConstraintInfo
                        C.ID = "dVar_" + i.ToString
                        C.ItemID = Dependency.ID.ToString
                        C.Type = ConstraintType.ctDependency
                        Dim cond As String = ""
                        Select Case Dependency.Value
                            Case RADependencyType.dtDependsOn
                                cond = " DEPENDS ON "
                            Case RADependencyType.dtMutuallyDependent
                                cond = " MULUTALLY DEPENDENT "
                            Case RADependencyType.dtMutuallyExclusive
                                cond = " MULUTALLY EXCLUSIVE "
                            Case RADependencyType.dtConcurrent
                                cond = " CONCURRENT "
                            Case RADependencyType.dtSuccessive
                                cond = " SUCCESSIVE "
                        End Select
                        C.Name = "Dependency #" + i.ToString
                        C.Description = CurrentScenario.Alternatives.First(Function(a) a.ID = Dependency.FirstAlternativeID).Name + cond + CurrentScenario.Alternatives.First(Function(a) a.ID = Dependency.SecondAlternativeID).Name
                        res.Add(C)
                    End If
                    i += 1
                Next
            End If

            If Settings.CustomConstraints Then
                For Each Constraint As RAConstraint In Constraints.Constraints.Values
                    If Constraint.Enabled And (Constraint.MinValueSet Or Constraint.MaxValueSet) Then
                        If Constraint.MinValueSet Then
                            Dim C As New ConstraintInfo
                            C.Type = ConstraintType.ctCustomConstraintMin
                            C.ID = "ccMinVar_" + Constraint.ID.ToString
                            C.ItemID = Constraint.ID.ToString
                            C.Name = String.Format("Min value for constraint {0}", Constraint.Name)    ' D6475 Constraint.Name + " (Min value)"
                            'C.Description = "Minimum value for constraint " + Constraint.Name   ' -D6475
                            res.Add(C)
                        End If
                        If Constraint.MaxValueSet Then
                            Dim C As New ConstraintInfo
                            C.Type = ConstraintType.ctCustomConstraintMax
                            C.ID = "ccMaxVar_" + Constraint.ID.ToString
                            C.ItemID = Constraint.ID.ToString
                            C.Name = String.Format("Max value for constraint {0}", Constraint.Name)    ' D6475 Constraint.Name + " (Max value)"
                            'C.Description = "Maximum value for constraint " + Constraint.Name  ' -D6475
                            res.Add(C)
                        End If
                    End If
                Next
            End If

            If Settings.FundingPools Then
                For Each FP As RAFundingPool In CurrentScenario.FundingPools.Pools.Values
                    If FP.Enabled Then
                        Dim C As New ConstraintInfo
                        C.Type = ConstraintType.ctFundingPoolLimit
                        C.ID = "fpPoolLimitVar_" + FP.ID.ToString
                        C.ItemID = FP.ID.ToString
                        C.Name = "Funding pool limit for '" + FP.Name + "'"
                        'C.Description = "Limit for funding pool '" + FP.Name + "' = " + FP.PoolLimit.ToString  ' -D6475
                        res.Add(C)
                    End If
                Next
            End If

            If Settings.Dependencies And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                Dim i As Integer = 1
                For Each dependency As RADependency In CurrentScenario.TimePeriodsDependencies.Dependencies
                    If dependency.Enabled AndAlso dependency.Value <> RADependencyType.dtConcurrent Then
                        Dim C As New ConstraintInfo
                        C.ID = "dTPVar_" + i.ToString
                        C.ItemID = dependency.ID.ToString
                        C.Type = ConstraintType.ctTimePeriodsDependency
                        Dim cond As String = ""
                        Select Case dependency.Value
                            Case RADependencyType.dtDependsOn
                                cond = " DEPENDS ON "
                            Case RADependencyType.dtMutuallyDependent
                                cond = " MULUTALLY DEPENDENT "
                            Case RADependencyType.dtMutuallyExclusive
                                cond = " MULUTALLY EXCLUSIVE "
                            Case RADependencyType.dtConcurrent
                                cond = " CONCURRENT "
                            Case RADependencyType.dtSuccessive
                                cond = " NON CONCURRENT "
                        End Select
                        C.Name = "Time period dependency #" + i.ToString
                        C.Description = CurrentScenario.Alternatives.First(Function(a) a.ID = dependency.FirstAlternativeID).Name + cond + CurrentScenario.Alternatives.First(Function(a) a.ID = dependency.SecondAlternativeID).Name
                        res.Add(C)
                    End If
                    i += 1
                Next
            End If

            If Settings.TimePeriods And CurrentScenario.TimePeriods.Periods.Count > 1 Then
                If Settings.ResourcesMin Or Settings.ResourcesMax Then
                    For Each period As RATimePeriod In CurrentScenario.TimePeriods.Periods
                        For Each resource As RAResource In CurrentScenario.TimePeriods.Resources.Values
                            Dim cc As RAConstraint = CurrentScenario.Constraints.GetConstraintByID(resource.ConstraintID)
                            If resource.ID.Equals(RA_Cost_GUID) Or (resource.Enabled AndAlso (resource.ConstraintID <> -1) AndAlso (cc Is Nothing Or cc IsNot Nothing AndAlso cc.Enabled) AndAlso Settings.CustomConstraints) Then 'A1130
                                Dim minValue As Double = period.GetResourceMinValue(resource.ID)
                                If Settings.ResourcesMin And minValue <> UNDEFINED_INTEGER_VALUE Then
                                    Dim C As New ConstraintInfo
                                    C.Type = ConstraintType.ctTimePeriodResourceMin
                                    C.ID = "tpResourceMinVar_" + period.ID.ToString + "_" + resource.ID.ToString
                                    C.ItemID = resource.ID.ToString
                                    C.PeriodID = period.ID
                                    C.Name = "Min value for resource " + resource.Name + " in period " + period.Name
                                    'C.Description = "Minimum value for resource " + resource.Name + " in period " + period.Name    ' -D6475
                                    res.Add(C)
                                End If
                                Dim maxValue As Double = period.GetResourceMaxValue(resource.ID)
                                If Settings.ResourcesMax And maxValue <> UNDEFINED_INTEGER_VALUE Then
                                    Dim C As New ConstraintInfo
                                    C.Type = ConstraintType.ctTimePeriodResourceMax
                                    C.ID = "tpResourceMaxVar_" + period.ID.ToString + "_" + resource.ID.ToString
                                    C.ItemID = resource.ID.ToString
                                    C.PeriodID = period.ID
                                    C.Name = "Max value for resource " + resource.Name + " in period " + period.Name
                                    'C.Description = "Maximum value for resource " + resource.Name + " in period " + period.Name    ' -D6475
                                    res.Add(C)
                                End If
                            End If
                        Next
                    Next
                End If
            End If

            Return res
        End Function

        Public Function GetInfeasibilityResults(ConstraintsToAnalyse As List(Of ConstraintInfo), NumberOfSolutions As Integer) As List(Of RAScenario)  ' D6475
            Dim result As New List(Of RAScenario)  ' D6475

            SolveForInfeasibilityAnalysis = True
            InfeasibilityConstraintsList = New List(Of ConstraintInfo)(ConstraintsToAnalyse)

            Dim isFeasible As Boolean = False
            Dim i As Integer = ConstraintsToAnalyse.Count

            While i >= 0 AndAlso Not isFeasible
                mInfeasibilityConstraintsCount = i
                Solve_Gurobi()

                If SolverState = raSolverState.raSolved Then
                    isFeasible = True
                Else
                    i -= 1
                End If
            End While

            Dim solutionsFound As Integer = 0

            If isFeasible Then
                While i >= 0 And solutionsFound < NumberOfSolutions
                    mInfeasibilityConstraintsCount = i
                    InfeasibilityConstraintsList = New List(Of ConstraintInfo)(ConstraintsToAnalyse)

                    mExcludeConstraintsSets.Clear()

                    While isFeasible And solutionsFound < NumberOfSolutions
                        Solve_Gurobi()
                        If SolverState = raSolverState.raSolved Then
                            isFeasible = True
                            solutionsFound += 1

                            Dim scenario As RAScenario = Me.ResourceAligner.Scenarios.AddScenario(Me.ResourceAligner.Scenarios.ActiveScenarioID)
                            scenario.IsInfeasibilityAnalysis = True
                            scenario.SolverPriorities.CheckAndSort()
                            scenario.InfeasibilityRemovedConstraints = New List(Of ConstraintInfo)  ' D6475
                            scenario.InfeasibilityOptimalValue = mFundedBenefit     ' D6475

                            Dim toRemove As New List(Of ConstraintInfo)
                            For Each c As ConstraintInfo In InfeasibilityConstraintsList
                                If Not c.Selected Then
                                    toRemove.Add(c)
                                    scenario.InfeasibilityRemovedConstraints.Add(c) ' D6475
                                    Select Case c.Type
                                        Case ConstraintType.ctMust
                                            Dim alt As RAAlternative = scenario.GetAvailableAlternativeById(c.ItemID)
                                            If alt IsNot Nothing Then alt.Must = False
                                        Case ConstraintType.ctMustNot
                                            Dim alt As RAAlternative = scenario.GetAvailableAlternativeById(c.ItemID)
                                            If alt IsNot Nothing Then alt.MustNot = False
                                        Case ConstraintType.ctGroup
                                            scenario.Groups.Groups.Remove(c.ItemID)
                                        Case ConstraintType.ctDependency
                                            Dim dep As RADependency = scenario.Dependencies.Dependencies.FirstOrDefault(Function(d) d.ID.ToString = c.ItemID)
                                            If dep IsNot Nothing Then
                                                scenario.Dependencies.Dependencies.Remove(dep)
                                                Dim depTP As RADependency = scenario.TimePeriodsDependencies.GetDependency(dep.FirstAlternativeID, dep.SecondAlternativeID)
                                                If depTP IsNot Nothing Then
                                                    scenario.TimePeriodsDependencies.DeleteDependency(depTP.FirstAlternativeID, depTP.SecondAlternativeID)
                                                End If
                                            End If
                                        Case ConstraintType.ctCustomConstraintMin
                                            'scenario.Constraints.GetConstraintByID(CInt(c.ItemID)).MinValue = UNDEFINED_INTEGER_VALUE
                                            scenario.Constraints.GetConstraintByID(CInt(c.ItemID)).MinValue = GetConstraintValueForFundedAlternatives(scenario, scenario.Constraints.GetConstraintByID(CInt(c.ItemID)))
                                        Case ConstraintType.ctCustomConstraintMax
                                            'scenario.Constraints.GetConstraintByID(CInt(c.ItemID)).MaxValue = UNDEFINED_INTEGER_VALUE
                                            scenario.Constraints.GetConstraintByID(CInt(c.ItemID)).MaxValue = GetConstraintValueForFundedAlternatives(scenario, scenario.Constraints.GetConstraintByID(CInt(c.ItemID)))
                                        Case ConstraintType.ctTimePeriodsDependency
                                            Dim depTP As RADependency = scenario.TimePeriodsDependencies.Dependencies.FirstOrDefault(Function(d) d.ID.ToString = c.ItemID)
                                            If depTP IsNot Nothing Then
                                                Dim depReg As RADependency = scenario.Dependencies.GetDependency(depTP.FirstAlternativeID, depTP.SecondAlternativeID)
                                                If depReg IsNot Nothing Then
                                                    scenario.Dependencies.DeleteDependency(depReg.FirstAlternativeID, depReg.SecondAlternativeID)
                                                End If
                                                scenario.TimePeriodsDependencies.Dependencies.Remove(depTP)
                                            End If
                                        Case ConstraintType.ctTimePeriodResourceMin
                                            Dim tp As RATimePeriod = scenario.TimePeriods.GetPeriod(c.PeriodID)
                                            If tp IsNot Nothing Then
                                                'tp.SetResourceMinValue(New Guid(c.ItemID), UNDEFINED_INTEGER_VALUE)
                                                tp.SetResourceMinValue(New Guid(c.ItemID), GetResourceValueForFundedAlternatives(scenario, c.PeriodID, New Guid(c.ItemID)))
                                            End If
                                        Case ConstraintType.ctTimePeriodResourceMax
                                            Dim tp As RATimePeriod = scenario.TimePeriods.GetPeriod(c.PeriodID)
                                            If tp IsNot Nothing Then
                                                'tp.SetResourceMaxValue(New Guid(c.ItemID), UNDEFINED_INTEGER_VALUE)
                                                tp.SetResourceMaxValue(New Guid(c.ItemID), GetResourceValueForFundedAlternatives(scenario, c.PeriodID, New Guid(c.ItemID)))
                                            End If
                                        Case ConstraintType.ctFundingPoolLimit
                                            scenario.FundingPools.GetPoolByID(CInt(c.ItemID)).PoolLimit = scenario.AlternativesFull.Sum(Function(x) x.Cost)
                                    End Select
                                End If
                            Next

                            Dim cList As New List(Of ConstraintInfo)
                            For Each c As ConstraintInfo In toRemove
                                cList.Add(c)
                            Next
                            mExcludeConstraintsSets.Add(cList)

                            result.Add(scenario)    ' D6475
                        Else
                            isFeasible = False
                        End If
                    End While

                    isFeasible = True
                    i -= 1
                End While
            End If

            mExcludeConstraintsSets.Clear()
            SolveForInfeasibilityAnalysis = False

            Return result
        End Function

        Public Function GetConstraintValueForFundedAlternatives(scenario As RAScenario, constraint As RAConstraint) As Double
            Dim res As Double = 0
            For Each alt As RAAlternative In scenario.Alternatives
                If alt.Funded > 0 Then
                    Dim v As Double = constraint.GetAlternativeValue(alt.ID)
                    If v <> UNDEFINED_INTEGER_VALUE Then
                        res += v
                    End If
                End If
            Next
            Return res
        End Function

        Public Function GetResourceValueForFundedAlternatives(scenario As RAScenario, periodID As Integer, resourceID As Guid) As Double
            Dim res As Double = UNDEFINED_INTEGER_VALUE
            For Each alt As RAAlternative In scenario.Alternatives
                If alt.Funded > 0 Then
                    Dim fundedPeriod As Integer = -1
                    If scenario.TimePeriods.TimePeriodResults.ContainsKey(alt.ID) Then
                        fundedPeriod = scenario.TimePeriods.TimePeriodResults(alt.ID)
                    End If
                    If fundedPeriod <> -1 Then
                        Dim v As Double = scenario.TimePeriods.PeriodsData.GetResourceValue(periodID - fundedPeriod, alt.ID, resourceID)
                        If v <> UNDEFINED_INTEGER_VALUE Then
                            res += v
                        End If
                    End If
                End If
            Next
            Return res
        End Function


        Public Sub New(RAligner As ResourceAligner)
            ResourceAligner = RAligner
        End Sub
    End Class

End Namespace
