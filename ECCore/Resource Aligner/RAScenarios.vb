Option Strict On    ' D2882

Imports ECCore
Imports Canvas
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.Serialization
Imports Newtonsoft.Json

<Serializable()> Public Class RAScenario
    Public Property ID As Integer

    Public ReadOnly Property Scenarios As RAScenarios

    Private mCombinedGroupUserD As Integer = UNDEFINED_INTEGER_VALUE
    Public Property CombinedGroupUserID() As Integer
        Get
            Return mCombinedGroupUserD
        End Get
        Set(value As Integer)
            If mCombinedGroupUserD <> value Then
                mCombinedGroupUserD = value
                If Not Scenarios.ResourceAligner.isLoading Then UpdateBenefits()
            End If
        End Set
    End Property

    Public Property Index As Integer = -1

    Private mName As String = "" 'A1416
    Public Property Name As String
        Get
            Return mName
        End Get
        Set(value As String)
            mName = value.Trim  ' D3251
        End Set
    End Property

    ' D2875 ===
    Private mDescription As String = ""
    Public Property Description As String
        Get
            If mDescription Is Nothing Then mDescription = ""
            Return mDescription
        End Get
        Set(value As String)
            mDescription = value.Trim   ' D3251
        End Set
    End Property
    ' D2875 ==

    Public Property Budget As Double = Double.MinValue

    Public Property MaxRisk As Double = 0
    Public Property MinReduction As Double = 0
    Public Property OptimizationType As RiskOptimizationType = RiskOptimizationType.BudgetLimit
    Public Property OriginalRiskValue As Double = 0
    Public Property OriginalRiskValueWithControls As Double = 0 'A1238
    'Public Property OriginalRiskDollarValue As Double = 0 'A1265
    'Public Property OriginalRiskDollarValueWithControls As Double = 0 'A1265
    Public Property OptimizedRiskValue As Double = 0 'A1265
    'Public Property OptimizedRiskDollarValue As Double = 0 'A1265
    Public Property OriginalAllControlsCost As Double = 0 'A1265
    Public Property OriginalAllControlsWithAssignmentsCost As Double = 0 'A1265
    Public Property OriginalSelectedControlsCost As Double = 0 'A1265
    Public Property OriginalSelectedControlsCount As Double = 0 'A1265

    Public Property SolverPriorities As New RASolverPriorities(Me)

    ''' <summary>
    ''' Is scenario checked in Increasing Budgets screen
    ''' </summary>
    ''' <remarks></remarks>
    Public Property IsCheckedIB As Boolean = False

    ''' <summary>
    ''' Is scenario checked in Scenario Comparison screen
    ''' </summary>
    ''' <remarks></remarks>
    Public Property IsCheckedCS As Boolean = False

    Public Property IsInfeasibilityAnalysis As Boolean = False  ' D4909
    Public Property InfeasibilityScenarioID As Integer = -1     ' D4909
    Public Property InfeasibilityScenarioIndex As Integer = 0   ' D6475
    Public Property InfeasibilityRemovedConstraints As List(Of ConstraintInfo) = Nothing ' D4912
    Public Property InfeasibilityOptimalValue As Double = 0     ' D6475


    Public Property Settings As New RASettings
    Public Property Constraints As New RAConstraints(Me)
    Public Property Groups As New RAGroups
    Public Property EventGroups As New RAGroups
    Public Property Dependencies As New RADependencies
    Public Property TimePeriodsDependencies As New RADependencies
    Public Property FundingPools As New RAFundingPools
    Public Property ResourcePools As New Dictionary(Of Guid, RAFundingPools)
    Public Property TimePeriods As New RATimePeriods(Me)

    Public AlternativesFull As New List(Of RAAlternative)
    Public Property Alternatives As List(Of RAAlternative)
        Get
            Return AlternativesFull.Where(Function(a) a.Enabled).ToList
        End Get
        Set(value As List(Of RAAlternative))
            AlternativesFull = value
        End Set
    End Property

    Private Sub UpdateBenefits()
        If Scenarios.ResourceAligner.IsRisk Then Exit Sub
        With Scenarios.ResourceAligner.ProjectManager
            Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(CInt(If(CombinedGroupUserID = UNDEFINED_INTEGER_VALUE, Scenarios.ResourceAligner.CombinedGroupUserID, CombinedGroupUserID)))
            If CG Is Nothing Then
                CG = .CombinedGroups.GetDefaultCombinedGroup()
            End If
            .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
            For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
                Dim raAlt As RAAlternative = AlternativesFull.FirstOrDefault(Function(a) (a.ID.ToString.ToLower = alt.NodeGuidID.ToString.ToLower))
                If raAlt IsNot Nothing Then
                    raAlt.BenefitOriginal = alt.UnnormalizedPriority
                    raAlt.Benefit = alt.UnnormalizedPriority
                    raAlt.Benefit = CDbl(If(Settings.Risks, raAlt.BenefitOriginal * (1 - raAlt.RiskOriginal), raAlt.BenefitOriginal))
                End If
            Next
        End With
    End Sub

    Public Function IsGroupAvailable(Group As RAGroup) As Boolean
        For Each alt As RAAlternative In Group.GetFilteredAlternatives(Alternatives)
            If Alternatives.FirstOrDefault(Function(a) (a.ID.ToLower = alt.ID.ToLower)) Is Nothing Then
                Return False
            End If
        Next
        Return True
    End Function

    Public Function GetAvailableGroups() As List(Of RAGroup)
        Return Groups.Groups.Values.Where(Function(g) IsGroupAvailable(g)).ToList
    End Function

    Public Function IsAvailableDependency(Dependency As RADependency) As Boolean
        If (Alternatives.FirstOrDefault(Function(a) (a.ID.ToLower = Dependency.FirstAlternativeID.ToLower)) Is Nothing AndAlso Groups.GetGroupByID(Dependency.FirstAlternativeID) Is Nothing) OrElse
           (Alternatives.FirstOrDefault(Function(a) (a.ID.ToLower = Dependency.SecondAlternativeID.ToLower)) Is Nothing AndAlso Groups.GetGroupByID(Dependency.SecondAlternativeID) Is Nothing) Then
            Return False
        End If
        Return True
    End Function

    Public Function GetAvailableDependencies() As List(Of RADependency)
        Return Dependencies.Dependencies.Where(Function(d) IsAvailableDependency(d)).ToList
    End Function

    Public Sub CheckDependencies(tDependencies As RADependencies)
        tDependencies.Dependencies.RemoveAll(Function(d) (GetAvailableAlternativeById(d.FirstAlternativeID) Is Nothing OrElse GetAvailableAlternativeById(d.SecondAlternativeID) Is Nothing))
    End Sub

    Public Function GetAvailableAlternativeById(altId As String) As RAAlternative
        Return Alternatives?.FirstOrDefault(Function(a) a.ID.Equals(altId))
    End Function

    Private Function GetMustsCost(Optional ByVal tSelectedConstraintID As Integer = -1) As Double
        Return Alternatives.Where(Function(a) a.Must).Sum(Function(x) x.Cost)
    End Function

    Private Function GetMustNotsCost(Optional ByVal tSelectedConstraintID As Integer = -1) As Double
        Return Alternatives.Where(Function(a) a.MustNot).Sum(Function(x) x.Cost)
    End Function

    Public Function GetMinCost(Optional ByVal tSelectedConstraintID As Integer = -1) As Double
        Dim alt As RAAlternative = Alternatives.OrderBy(Function(a) If(a.IsPartial, a.Cost * a.MinPercent, a.Cost)).FirstOrDefault()
        Dim res As Double = If(alt Is Nothing, 0, If(alt.IsPartial, alt.Cost * alt.MinPercent, alt.Cost))
        If Settings.Musts Then
            Dim mustCost As Double = GetMustsCost()
            If mustCost > res Then res = mustCost
        End If
        If tSelectedConstraintID >= 0 Then ' CC or Risk
            If tSelectedConstraintID = Integer.MaxValue Then
                'Risk
                If Alternatives IsNot Nothing AndAlso Alternatives.Count > 0 Then
                    res = Alternatives.Min(Function(a) a.RiskOriginal)
                End If
                If res > 0 Then res = 0 ' allow negative values or 0
            Else
                'Custom Constraint
                res = 0
            End If
        End If
        Return res
    End Function

    Public Function GetMaxCost(Optional ByVal tSelectedConstraintID As Integer = -1) As Double
        Dim res As Double = Alternatives.Sum(Function(a) a.Cost)
        If Settings.MustNots Then
            Dim mustNotsCost As Double = GetMustNotsCost()
            res -= mustNotsCost
        End If
        If tSelectedConstraintID >= 0 Then ' CC or Risk
            If tSelectedConstraintID = Integer.MaxValue Then
                'Risk
                res = Alternatives.Sum(Function(a) a.RiskOriginal)
            Else
                'Custom Constraint
                res = Alternatives.Sum(Function(a) If(a.Enabled, 1, 0)) ' Count of enabled alternatives
            End If
        End If
        Return res
    End Function

    Public Function GetDefaultEfficientFrontierInterval() As EfficientFrontierInterval
        Dim interval As New EfficientFrontierInterval
        interval.MinValue = GetMinCost()
        interval.MaxValue = GetMaxCost()
        interval.DeltaValue = (interval.MaxValue - interval.MinValue) / EFFICIENT_FRONTIER_DEFAULT_NUMBER_OF_STEPS
        Return interval
    End Function

    Public Function GetMaxBudget() As Double 'A1500
        Dim retVal As Double = Alternatives.Sum(Function(alt) If(alt.Cost <> UNDEFINED_INTEGER_VALUE And alt.Cost <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, alt.Cost, 0)) + 100
        Return retVal
    End Function

    ' D6478 ===
    Public Function GetInfeasibilityRemovedConstraint(sType As ConstraintType, ItemID As String) As ConstraintInfo
        If IsInfeasibilityAnalysis AndAlso InfeasibilityRemovedConstraints IsNot Nothing AndAlso InfeasibilityRemovedConstraints.Count > 0 Then
            For Each ConstrInfo As ConstraintInfo In InfeasibilityRemovedConstraints
                If ConstrInfo.Type = sType AndAlso ConstrInfo.ItemID = ItemID Then Return ConstrInfo
            Next
        End If
        Return Nothing
    End Function
    ' D6478 ==

    Public Sub New(Scenarios As RAScenarios)
        Me.Scenarios = Scenarios
    End Sub

End Class

<Serializable()> Public Class RAScenarios
    Private mActiveScenarioID As Integer
    Public Scenarios As New Dictionary(Of Integer, RAScenario)
    Public Property GlobalSettings As RAGlobalSettings

    Public Property ResourceAligner As ResourceAligner

    Public Property ActiveScenarioID As Integer
        Get
            Return mActiveScenarioID
        End Get
        Set(value As Integer)
            If value <> mActiveScenarioID AndAlso Scenarios.ContainsKey(value) Then   ' D3213
                mActiveScenarioID = value
                If ResourceAligner.ProjectManager.StorageManager.ProviderType = DBProviderType.dbptSQLClient Then CheckModel(, True) 'A1324 + D4834
                If Not ResourceAligner.isLoading Then
                    If ResourceAligner.ProjectManager.Parameters.Parameters.Parameters.Count < 2 Then ResourceAligner.ProjectManager.Parameters.Load() ' D4265
                    ResourceAligner.ProjectManager.Parameters.RAActiveScenarioID = value ' D3858
                    ResourceAligner.ProjectManager.Parameters.Save()
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property ActiveScenario As RAScenario
        Get
            Return Scenarios(ActiveScenarioID)
        End Get
    End Property

    Public Sub CheckModel(Optional ByVal tUpdateBenefits As Boolean = True, Optional fKeepInfeasibilityScenarios As Boolean = False) 'A1324 + D4929
        ' D4929 ===
        Dim toRemove As New List(Of Integer)
        Dim AID As Integer = ActiveScenarioID
        For Each tScen As KeyValuePair(Of Integer, RAScenario) In Scenarios ' check all infeasibility scenarios and wipe out in cae of an empty or when don't need to keep it
            If tScen.Value.IsInfeasibilityAnalysis AndAlso (tScen.Value.Name = "" OrElse Not fKeepInfeasibilityScenarios) Then
                If tScen.Key = AID Then AID = If(tScen.Value.InfeasibilityScenarioID > 0, tScen.Value.InfeasibilityScenarioID, 0)
                toRemove.Insert(0, tScen.Key)
            End If
        Next
        For Each ID As Integer In toRemove
            DeleteScenario(ID)
        Next
        If AID <> ActiveScenarioID Then ActiveScenarioID = AID  ' for avoid calling recursive that function we are set ActiveScenarioID at the end
        ' D4929 ==
        If tUpdateBenefits Then UpdateScenarioBenefits()
        CheckAlternatives()
        UpdateSortOrder()
        CheckDependencies() 'A1416
    End Sub

    public Sub CheckAlternatives() 'A1324
        Dim BaseScenario As RAScenario = Scenarios(0)

        If ResourceAligner.ProjectManager.IsRiskProject Then
            ResourceAligner.ProjectManager.Controls.ReadControls(ResourceAligner.ProjectManager.StorageManager.StorageType, ResourceAligner.ProjectManager.StorageManager.Location, ResourceAligner.ProjectManager.StorageManager.ProviderType, ResourceAligner.ProjectManager.StorageManager.ModelID)
            Dim LJT As DateTime
            ResourceAligner.ProjectManager.StorageManager.Reader.LoadUserJudgmentsControls(LJT)
            For Each control As clsControl In ResourceAligner.ProjectManager.Controls.EnabledControls
                For Each assignment As clsControlAssignment In control.Assignments
                    assignment.Value = ResourceAligner.ProjectManager.Controls.GetCombinedEffectivenessValue(assignment.Judgments, control.ID, assignment.Value)
                Next
            Next
        End If

        Dim fNeedSave As Boolean = False    ' D3097
        For Each scenario As RAScenario In Scenarios.Values
            If scenario.ID <> 0 Then
                For i As Integer = 0 To BaseScenario.AlternativesFull.Count - 1
                    Dim alt As RAAlternative = BaseScenario.AlternativesFull(i)
                    If Not scenario.AlternativesFull.Exists(Function(a) (a.ID.ToLower = alt.ID.ToLower)) Then
                        scenario.AlternativesFull.Add(alt.Clone)
                        fNeedSave = True    ' D3097
                    Else
                        ' D3369 ===
                        Dim curAlt As RAAlternative = scenario.AlternativesFull.Where(Function(a) (a.ID.ToLower = alt.ID.ToLower)).First
                        If curAlt.Name <> alt.Name Then
                            curAlt.Name = alt.Name
                            ' D3369 ==
                            fNeedSave = True
                        End If
                    End If
                Next

                For i As Integer = scenario.AlternativesFull.Count - 1 To 0 Step -1
                    Dim alt As RAAlternative = scenario.AlternativesFull(i)
                    If Not BaseScenario.AlternativesFull.Exists(Function(a) (a.ID.ToLower = alt.ID.ToLower)) Then
                        scenario.AlternativesFull.RemoveAt(i)
                        fNeedSave = True    ' D3097
                    End If
                Next
            End If

            If ResourceAligner.ProjectManager.IsRiskProject Then
                For i As Integer = scenario.AlternativesFull.Count - 1 To 0 Step -1
                    Dim alt As RAAlternative = scenario.AlternativesFull(i)
                    Dim control As clsControl = ResourceAligner.ProjectManager.Controls.GetControlByID(New Guid(alt.ID))
                    If control Is Nothing OrElse Not control.Enabled OrElse control.Type = ControlType.ctUndefined Then
                        scenario.AlternativesFull.RemoveAt(i)
                        fNeedSave = True
                    End If
                Next

                For Each control As clsControl In ResourceAligner.ProjectManager.Controls.EnabledControls
                    Dim newAlt As RAAlternative = Nothing
                    Dim aid As String = control.ID.ToString.ToLower
                    newAlt = scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = aid))
                    If newAlt Is Nothing Then
                        newAlt = New RAAlternative
                        scenario.AlternativesFull.Add(newAlt)
                    End If
                    newAlt.ID = control.ID.ToString
                    newAlt.Enabled = control.Enabled
                    newAlt.SortOrder = ResourceAligner.ProjectManager.Controls.Controls.IndexOf(control) + 1
                    'newAlt.SortOrder = CInt(ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_ALT_SORT_ID, control.ID))
                    newAlt.Name = control.Name
                    newAlt.BenefitOriginal = control.Effectiveness
                    newAlt.Benefit = control.Effectiveness
                    If newAlt.Cost <> control.Cost Then fNeedSave = True
                    newAlt.Cost = control.Cost
                    '-A1412 newAlt.Enabled = control.Enabled 'A1411
                Next
            End If

            'A0923 ===
            'clean-up the non-existing alternatives from Dependencies
            Dim k As Integer = 0
            While k < scenario.Dependencies.Dependencies.Count
                Dim dep As RADependency = scenario.Dependencies.Dependencies(k)
                Dim alt0 As Object = ResourceAligner.GetAlternativeById(scenario, dep.FirstAlternativeID)
                If alt0 Is Nothing Then alt0 = ResourceAligner.Solver.Groups.GetGroupByID(dep.FirstAlternativeID)
                Dim alt1 As Object = ResourceAligner.GetAlternativeById(scenario, dep.SecondAlternativeID)
                If alt1 Is Nothing Then alt1 = ResourceAligner.Solver.Groups.GetGroupByID(dep.SecondAlternativeID)
                If alt0 Is Nothing OrElse alt1 Is Nothing Then
                    scenario.Dependencies.Dependencies.Remove(dep)
                    fNeedSave = True
                Else
                    k += 1
                End If
            End While

            'clean-up the non-existing alternatives from Groups
            For Each kvp As KeyValuePair(Of String, RAGroup) In scenario.Groups.Groups
                Dim grp As RAGroup = kvp.Value
                If grp.Alternatives IsNot Nothing Then
                    k = 0
                    While k < grp.Alternatives.Count
                        Dim alt As String = grp.Alternatives.Keys(k)
                        If ResourceAligner.GetAlternativeById(scenario, alt) Is Nothing Then
                            grp.Alternatives.Remove(alt)
                            fNeedSave = True
                        Else
                            k += 1
                        End If
                    End While
                End If
            Next
            'clean-up the non-existing alternatives from Custom Constraints
            For Each kvp As KeyValuePair(Of Integer, RAConstraint) In scenario.Constraints.Constraints
                Dim cc As RAConstraint = kvp.Value
                If cc.AlternativesData IsNot Nothing Then
                    k = 0
                    While k < cc.AlternativesData.Count
                        Dim alt As String = cc.AlternativesData.Keys(k)
                        If ResourceAligner.GetAlternativeById(scenario, alt) Is Nothing Then
                            cc.AlternativesData.Remove(alt)
                            fNeedSave = True
                        Else
                            k += 1
                        End If
                    End While
                End If
            Next
            'clean-up the non-existing alternatives from Funding Pools
            For Each kvp As KeyValuePair(Of Integer, RAFundingPool) In scenario.FundingPools.Pools
                Dim fp As RAFundingPool = kvp.Value
                If fp.Values IsNot Nothing Then
                    k = 0
                    While k < fp.Values.Count
                        Dim alt As String = fp.Values.Keys(k)
                        If ResourceAligner.GetAlternativeById(scenario, alt) Is Nothing Then
                            fp.Values.Remove(alt)
                            fNeedSave = True
                        Else
                            k += 1
                        End If
                    End While
                End If
            Next
            'A0923 ==
        Next

        If fNeedSave AndAlso ResourceAligner IsNot Nothing Then ResourceAligner.Save() ' D3097
    End Sub

    Private Sub UpdateSortOrder() 'A1324
        If ResourceAligner.IsRisk Then Exit Sub

        Dim alts As List(Of clsNode) = ResourceAligner.ProjectManager.AltsHierarchy(ResourceAligner.ProjectManager.ActiveAltsHierarchy).TerminalNodes
        For i As Integer = 0 To alts.Count - 1
            Dim alt As clsNode = alts(i)
            For Each scenario As RAScenario In Scenarios.Values
                Dim raAlt As RAAlternative = scenario.AlternativesFull.FirstOrDefault(Function(p) (p.ID.ToLower = alt.NodeGuidID.ToString.ToLower))
                If raAlt IsNot Nothing Then
                    raAlt.SortOrder = i + 1
                End If
            Next
        Next
        CheckAndSortScenarios() ' D3213
    End Sub

    'A1416 ===
    Private Sub CheckDependencies()
        Dim fNeedSave As Boolean = False
        For Each scenario As RAScenario In Scenarios.Values
            ' check regular dependencies
            For Each tDep As RADependency In scenario.Dependencies.Dependencies
                If tDep.Value = RADependencyType.dtDependsOn Then
                    Dim tExistDep As RADependency = scenario.TimePeriodsDependencies.GetDependency(tDep.FirstAlternativeID, tDep.SecondAlternativeID)
                    If tExistDep Is Nothing OrElse (tExistDep.Value <> RADependencyType.dtConcurrent AndAlso tExistDep.Value <> RADependencyType.dtSuccessive AndAlso tExistDep.Value <> RADependencyType.dtLag) Then
                        scenario.TimePeriodsDependencies.DeleteDependencyOneDirection(tDep.FirstAlternativeID, tDep.SecondAlternativeID)
                        scenario.TimePeriodsDependencies.SetDependency(tDep.FirstAlternativeID, tDep.SecondAlternativeID, RADependencyType.dtSuccessive)
                        fNeedSave = True
                    End If
                End If
            Next

            ' check Time Periods dependencies
            For Each tDep As RADependency In scenario.TimePeriodsDependencies.Dependencies
                If tDep.Value = RADependencyType.dtConcurrent OrElse tDep.Value = RADependencyType.dtSuccessive OrElse tDep.Value = RADependencyType.dtLag Then
                    Dim tExistDep As RADependency = scenario.Dependencies.GetDependency(tDep.FirstAlternativeID, tDep.SecondAlternativeID)
                    If tExistDep Is Nothing OrElse tExistDep.Value <> RADependencyType.dtDependsOn Then
                        scenario.Dependencies.DeleteDependencyOneDirection(tDep.FirstAlternativeID, tDep.SecondAlternativeID)
                        scenario.Dependencies.SetDependency(tDep.FirstAlternativeID, tDep.SecondAlternativeID, RADependencyType.dtDependsOn)
                        fNeedSave = True
                    End If
                End If
            Next

            ' check duplicates
            Dim i As Integer = 0
            While i < scenario.Dependencies.Dependencies.Count
                Dim tDep As RADependency = scenario.Dependencies.Dependencies(i)
                If tDep.Value = RADependencyType.dtMutuallyDependent OrElse tDep.Value = RADependencyType.dtMutuallyExclusive Then
                    Dim tExistDep As RADependency = scenario.Dependencies.GetDependency(tDep.SecondAlternativeID, tDep.FirstAlternativeID)
                    If tExistDep IsNot Nothing Then
                        scenario.Dependencies.DeleteDependencyOneDirection(tDep.SecondAlternativeID, tDep.FirstAlternativeID)
                        fNeedSave = True
                    End If
                End If
                i += 1
            End While
        Next
        If fNeedSave AndAlso ResourceAligner IsNot Nothing Then ResourceAligner.Save()
    End Sub
    'A1416 ==

    ' D3340 ===
    Public Function SyncLinkedConstraintsValues(Optional tConstraintsList As List(Of RAConstraint) = Nothing) As Boolean  ' D3346
        If ResourceAligner.ProjectManager.IsRiskProject Then Return True
        Dim fRes As Boolean = False
        Dim ConstrList As New List(Of RAConstraint)

        If tConstraintsList IsNot Nothing Then
            ConstrList = tConstraintsList   ' D3346
        Else
            For Each ID As Integer In ActiveScenario.Constraints.Constraints.Keys
                If ActiveScenario.Constraints.Constraints(ID).IsLinked AndAlso ActiveScenario.Constraints.Constraints(ID).IsReadOnly Then   ' D3346
                    ConstrList.Add(ActiveScenario.Constraints.Constraints(ID))
                End If
            Next
        End If

        If ConstrList IsNot Nothing Then

            Dim AttributesList As New List(Of clsAttribute)
            With ResourceAligner.ProjectManager
                '.Controls.ReadControls(.StorageManager.StorageType, .StorageManager.Location, .StorageManager.ProviderType, .StorageManager.ModelID)

                '.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                If .Attributes IsNot Nothing AndAlso .Attributes.AttributesList IsNot Nothing AndAlso .Attributes.AttributesList.Count > 0 Then
                    '.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
                    For Each attr As clsAttribute In If(.IsRiskProject, .Attributes.GetControlsAttributes, .Attributes.GetAlternativesAttributes) ' D6049
                        If Not attr.IsDefault AndAlso (attr.ValueType = AttributeValueTypes.avtLong OrElse attr.ValueType = AttributeValueTypes.avtDouble) OrElse attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                            AttributesList.Add(attr)
                        End If
                    Next
                End If
            End With

            For Each CC As RAConstraint In ConstrList
                If CC.Enabled OrElse RA_OPT_CC_EDIT_DISABLED Then  ' D3539

                    'If CC.IsLinked AndAlso CC.IsReadOnly Then  ' -D3346
                    Dim tAttr As clsAttribute = Nothing
                    For Each tmpAttr As clsAttribute In AttributesList
                        If tmpAttr.ID.Equals(CC.LinkedAttributeID) Then
                            tAttr = tmpAttr
                            Exit For
                        End If
                    Next
                    If tAttr IsNot Nothing Then
                        Dim isDollarValues As Boolean = CC.isDollarValue    ' D6464
                        For Each tAlt As RAAlternative In ActiveScenario.AlternativesFull
                            Dim objAttrValue As Object = ResourceAligner.ProjectManager.Attributes.GetAttributeValue(tAttr.ID, New Guid(tAlt.ID))
                            If objAttrValue Is Nothing AndAlso tAttr.DefaultValue IsNot Nothing Then objAttrValue = tAttr.DefaultValue ' D3379
                            'If objAttrValue IsNot Nothing Then ' -D3379
                            Select Case tAttr.ValueType

                                Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                    Dim tAttributeValue As String = ""
                                    If objAttrValue IsNot Nothing Then  ' D3379
                                        If TypeOf (objAttrValue) Is Guid Then tAttributeValue = objAttrValue.ToString Else tAttributeValue = CStr(objAttrValue)
                                    End If
                                    'If Not String.IsNullOrEmpty(tAttributeValue) Then  ' -D3379
                                    Dim tVal As Double = UNDEFINED_INTEGER_VALUE
                                    tAttributeValue = (tAttributeValue + ";").Replace(";;", ";").Trim(CChar(";"))
                                    Dim tParams() As String = tAttributeValue.Split(CChar(";"))
                                    For Each sParam As String In tParams
                                        If sParam <> "" Then
                                            If CC.LinkedEnumID.ToString = sParam Then
                                                tVal = If(isDollarValues AndAlso tAlt.Cost <> UNDEFINED_INTEGER_VALUE, tAlt.Cost, 1) ' D6464
                                            End If
                                        End If
                                    Next
                                    fRes = ActiveScenario.Constraints.SetConstraintValue(CC.ID, tAlt.ID, tVal)  ' D3802
                                    'End If

                                Case AttributeValueTypes.avtDouble, AttributeValueTypes.avtLong
                                    fRes = ActiveScenario.Constraints.SetConstraintValue(CC.ID, tAlt.ID, CDbl(objAttrValue))    ' D3802

                            End Select
                            'End If
                        Next
                    End If
                    'End If
                End If
            Next
        End If
        Return fRes
    End Function
    ' D3340 ==

    Public Function SyncLinkedConstraintsToResources() As Boolean
        Dim HasChanged As Boolean = False
        If RA_OPT_FORCE_CC_USE_IN_TIMEPERIODS Then
            For Each tScen As KeyValuePair(Of Integer, RAScenario) In Scenarios
                For Each tCID As Integer In tScen.Value.Constraints.Constraints.Keys
                    Dim tConstr As RAConstraint = tScen.Value.Constraints.Constraints(tCID)
                    If Not tConstr.IsLinkedToResource Then
                        tConstr.IsLinkedToResource = True
                        HasChanged = True
                    End If
                Next
            Next
        End If
        Return HasChanged
    End Function
    ' D4913 ==

    Public Function AddScenario(Optional BaseScenarioID As Integer = 0, Optional CheckAndSort As Boolean = True) As RAScenario
        Dim res As New RAScenario(Me)
        Dim newId As Integer = Scenarios.Max(Function(i) (i.Key)) + 1
        res.ID = newId 'AS/6-22-15
        Scenarios.Add(newId, res)
        If res.Index < 0 Then res.Index = Scenarios.Count + 1 ' D3209 + D3215

        If BaseScenarioID >= 0 Then  ' D3098
            Dim BaseScenario As RAScenario = Scenarios(BaseScenarioID)
            For Each alt As RAAlternative In BaseScenario.AlternativesFull
                res.AlternativesFull.Add(alt.Clone)
            Next
            res.Budget = BaseScenario.Budget
            ' D3123 ===
            res.Constraints = BaseScenario.Constraints.Clone(res)
            res.Dependencies = BaseScenario.Dependencies.Clone
            res.TimePeriodsDependencies = BaseScenario.TimePeriodsDependencies.Clone 'A1225
            res.FundingPools = BaseScenario.FundingPools.Clone
            res.Groups = BaseScenario.Groups.Clone(res.AlternativesFull)
            res.EventGroups = BaseScenario.Groups.Clone() 'A1407
            res.TimePeriods = BaseScenario.TimePeriods.Clone(res)  ' D3840
            res.IsCheckedIB = BaseScenario.IsCheckedIB 'A0915
            res.IsCheckedCS = BaseScenario.IsCheckedCS 'A0915
            ' D3123 ==
            res.Settings = BaseScenario.Settings.Clone
            ' D3098 ===
        Else
            If Scenarios.Count > 0 Then
                Dim BaseScenario As RAScenario = Scenarios(0)
                For Each alt As RAAlternative In BaseScenario.AlternativesFull
                    res.AlternativesFull.Add(alt.Clone)
                Next
                res.Budget = 0
            End If
        End If
        ' D3098 ==

        If ResourceAligner IsNot Nothing AndAlso Not ResourceAligner.isLoading Then ' D3240
            CheckAndSortScenarios()     ' D3213
        End If

        Return (res)
    End Function

    Public Function DeleteScenario(ScenarioID As Integer) As Boolean
        If Not Scenarios.ContainsKey(ScenarioID) OrElse ScenarioID = 0 Then Return False        
        Return Scenarios.Remove(ScenarioID)
    End Function

    Public Function AddDefaultScenario() As Integer
        If ResourceAligner Is Nothing OrElse ResourceAligner.ProjectManager Is Nothing Then Return -1 ' D2839

        Dim Scenario As RAScenario
        Dim exists As Boolean = Scenarios.ContainsKey(0)
        If exists Then
            Scenario = Scenarios(0)
        Else
            Scenario = New RAScenario(Me)
            Scenario.ID = 0
            Scenario.Name = "Default Scenario"
            Scenario.Budget = 0
            Scenarios.Add(0, Scenario)
        End If
        If Scenario.Settings Is Nothing Then Scenario.Settings = New RASettings
        If Scenario.SolverPriorities Is Nothing Then Scenario.SolverPriorities = New RASolverPriorities(Scenario)

        If Scenario.AlternativesFull Is Nothing Then Scenario.AlternativesFull = New List(Of RAAlternative)

        With ResourceAligner.ProjectManager
            If Not ResourceAligner.IsRisk Then
                If Not ResourceAligner.isLoading Then
                    Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(ResourceAligner.CombinedGroupUserID)
                    If CG Is Nothing Then
                        CG = .CombinedGroups.GetDefaultCombinedGroup()
                    End If
                    .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
                End If
                Dim NeedToSaveAttributes As Boolean = False
                For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
                    Dim newAlt As RAAlternative = Nothing
                    If exists Then
                        Dim aid As String = alt.NodeGuidID.ToString.ToLower
                        newAlt = Scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = aid))
                    End If
                    If newAlt Is Nothing Then
                        newAlt = New RAAlternative
                        Scenario.AlternativesFull.Add(newAlt)
                    End If
                    newAlt.ID = alt.NodeGuidID.ToString
                    'newAlt.SortOrder = alt.SOrder
                    newAlt.SortOrder = CInt(ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_ALT_SORT_ID, alt.NodeGuidID))
                    newAlt.Name = alt.NodeName
                    If Not exists Or newAlt Is Nothing Then
                        newAlt.Must = False
                        newAlt.MustNot = False
                        newAlt.IsPartial = False
                        newAlt.MinPercent = 0
                    End If
                    newAlt.BenefitOriginal = alt.UnnormalizedPriority
                    newAlt.Benefit = alt.UnnormalizedPriority
                    ' D4619 ===
                    Dim costValue As Double = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE
                    Dim sValue As String = CStr(.Attributes.GetAttributeValue(ATTRIBUTE_COST_ID, alt.NodeGuidID))    ' D2282
                    If Double.TryParse(sValue, costValue) Then
                        If costValue <> UNDEFINED_INTEGER_VALUE And costValue <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                            newAlt.Cost = costValue
                            .Attributes.SetAttributeValue(ATTRIBUTE_COST_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, Nothing, alt.NodeGuidID, Guid.Empty)
                            NeedToSaveAttributes = True
                        End If
                    End If
                    Dim riskValue As Double = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE
                    sValue = CStr(.Attributes.GetAttributeValue(ATTRIBUTE_RISK_ID, alt.NodeGuidID)) ' D2882
                    If Double.TryParse(sValue, riskValue) Then
                        If riskValue <> UNDEFINED_INTEGER_VALUE And riskValue <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE Then
                            newAlt.RiskOriginal = riskValue
                            .Attributes.SetAttributeValue(ATTRIBUTE_RISK_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, Nothing, alt.NodeGuidID, Guid.Empty)
                            NeedToSaveAttributes = True
                        End If
                    End If
                    ' D4619 ==
                    newAlt.Benefit = CDbl(If(Scenario.Settings.Risks, newAlt.BenefitOriginal * (1 - newAlt.RiskOriginal), newAlt.BenefitOriginal)) ' D2882
                Next

                For i As Integer = Scenario.AlternativesFull.Count - 1 To 0 Step -1
                    If .AltsHierarchy(.ActiveAltsHierarchy).GetNodeByID(New Guid(Scenario.AlternativesFull(i).ID)) Is Nothing Then
                        Scenario.AlternativesFull.RemoveAt(i)
                    End If
                Next

                If NeedToSaveAttributes AndAlso .StorageManager.StorageType = ECModelStorageType.mstCanvasStreamDatabase Then   ' D4927 // for avoid crashing on upload AHP while adding default scenario
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.Location, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End If

                'Dim max As Double = 0
                'For Each raAlt As RAAlternative In Scenario.Alternatives
                '    If raAlt.BenefitOriginal > max Then
                '        max = raAlt.BenefitOriginal
                '    End If
                'Next

                'If max > 0 Then
                '    For Each raAlt As RAAlternative In Scenario.Alternatives
                '        raAlt.Benefit /= max
                '        raAlt.BenefitOriginal /= max
                '    Next
                'End If
            Else
                For Each control As clsControl In ResourceAligner.ProjectManager.Controls.EnabledControls
                    Dim newAlt As RAAlternative = Nothing
                    If exists Then
                        Dim aid As String = control.ID.ToString.ToLower
                        newAlt = Scenario.AlternativesFull.Find(Function(a) (a.ID.ToLower = aid))
                    End If
                    If newAlt Is Nothing Then
                        newAlt = New RAAlternative
                        Scenario.AlternativesFull.Add(newAlt)
                    End If
                    newAlt.ID = control.ID.ToString
                    newAlt.SortOrder = ResourceAligner.ProjectManager.Controls.EnabledControls.IndexOf(control) + 1
                    'newAlt.SortOrder = CInt(ResourceAligner.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_ALT_SORT_ID, control.ID))
                    newAlt.Name = control.Name
                    If Not exists Or newAlt Is Nothing Then
                        newAlt.Must = False
                        newAlt.MustNot = False
                        newAlt.IsPartial = False
                        newAlt.MinPercent = 0
                    End If
                    newAlt.BenefitOriginal = control.Effectiveness
                    newAlt.Benefit = control.Effectiveness
                    newAlt.Cost = control.Cost
                Next

                'For i As Integer = Scenario.AlternativesFull.Count - 1 To 0 Step -1
                '    If .AltsHierarchy(.ActiveAltsHierarchy).GetNodeByID(New Guid(Scenario.AlternativesFull(i).ID)) Is Nothing Then
                '        Scenario.AlternativesFull.RemoveAt(i)
                '    End If
                'Next
            End If
        End With

        Return Scenario.ID
    End Function

    Private Sub UpdateScenarioBenefits() 'A1324
        If ResourceAligner.IsRisk Then Exit Sub

        With ResourceAligner.ProjectManager
            .LoadPipeParameters(PipeStorageType.pstStreamsDatabase, .StorageManager.ModelID)
            For Each scenario As RAScenario In Scenarios.Values
                'Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(ResourceAligner.CombinedGroupUserID)
                Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(scenario.CombinedGroupUserID)
                If CG Is Nothing Then
                    CG = .CombinedGroups.GetDefaultCombinedGroup()
                End If
                .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)

                Dim max As Double = 0
                For Each alt As clsNode In .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
                    Dim raAlt As RAAlternative = Nothing
                    Dim aid As String = alt.NodeGuidID.ToString.ToLower
                    raAlt = scenario.AlternativesFull.FirstOrDefault(Function(a) (a.ID.ToLower = aid))
                    If raAlt IsNot Nothing Then
                        raAlt.BenefitOriginal = alt.UnnormalizedPriority
                        raAlt.Benefit = alt.UnnormalizedPriority
                        If alt.UnnormalizedPriority > max Then
                            max = alt.UnnormalizedPriority
                        End If
                    End If
                Next
                If max > 0 Then
                    For Each raAlt As RAAlternative In scenario.AlternativesFull
                        raAlt.BenefitOriginal /= max
                        raAlt.Benefit = CDbl(If(scenario.Settings.Risks, raAlt.BenefitOriginal * (1 - raAlt.RiskOriginal), raAlt.BenefitOriginal))
                    Next
                End If
            Next
        End With
    End Sub

    ' D3213 ===
    Public Sub CheckAndSortScenarios()
        If Scenarios IsNot Nothing AndAlso Scenarios.Count > 1 Then
            Dim fHasIDs As Boolean = False  ' D3652
            Dim Lst As New List(Of RAScenario)
            For Each ID As Integer In Scenarios.Keys
                Lst.Add(Scenarios(ID))
                If Scenarios(ID).ID > 0 Then fHasIDs = True ' D3652
            Next

            If fHasIDs Then Lst.Sort(New RAScenarios_Comparer(RAGlobalSettings.raScenarioField.Index, False)) ' D3652

            Scenarios.Clear()
            Dim Idx As Integer = 1
            For Each tScen As RAScenario In Lst
                tScen.Index = Idx
                If Not fHasIDs Then tScen.ID = Idx - 1 ' D3652
                Scenarios.Add(tScen.ID, tScen)
                Idx += 1
            Next

            Lst = Nothing
        End If
    End Sub
    ' D3213 ==

    'A1227 ===
    Public Function GetScenarioById(ScenarioId As Integer) As RAScenario
        For Each scenario As RAScenario In Scenarios.Values
            If scenario.ID = ScenarioId Then Return scenario
        Next
        Return Nothing
    End Function
    'A1227 ==

    Class ScenarioData
        Public id As Integer
        Public name As String
    End Class

    Public Function ToJSON() As String
        Dim ScenariosData As New List(Of ScenarioData)
        For each kvp As KeyValuePair(Of Integer, RAScenario) In Scenarios
            ScenariosData.Add(New ScenarioData With {.id = kvp.Key, .name = kvp.Value.Name})
        Next
        Return JsonConvert.SerializeObject(ScenariosData)
    End Function

    Public Sub Clear()
        Scenarios.Clear()
    End Sub

    Public Sub New(ResourceAligner As ResourceAligner)
        Me.ResourceAligner = ResourceAligner
        mActiveScenarioID = 0
        GlobalSettings = New RAGlobalSettings(ResourceAligner)
    End Sub

End Class

