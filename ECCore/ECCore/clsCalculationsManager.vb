Imports ECCore.MathFuncs
Imports System.Linq
Imports ECCore.MiscFuncs

Namespace ECCore
    <Serializable()> Public Class Priorities
        Public Property ObjectivesPriorities As New Dictionary(Of Integer, Tuple(Of Double, Double))
        Public Property AlternativesPriorities As New Dictionary(Of Integer, Tuple(Of Double, Double))
        Public Property ObjectivesPrioritiesByAlternative As New Dictionary(Of Integer, Dictionary(Of Integer, Tuple(Of Double, Double)))
        Public Property AlternativesPrioritiesByObjective As New Dictionary(Of Integer, Dictionary(Of Integer, Tuple(Of Double, Double)))
    End Class

    <Serializable()> Public Class Results
        Public Property UserID As Integer
        Public Property HierarchyID As Integer
        Public Property Priorities As New Priorities
        Public Property PrioritiesWithControls As New Priorities
    End Class
    <Serializable()> Public Class RiskResults
        Public Property UserID As Integer
        Public Property LikelihoodResults As New Results
        Public Property ImpactResults As New Results
        Public Property RiskValues As New Dictionary(Of Integer, Double)
        Public Property RiskValuesWithControls As New Dictionary(Of Integer, Double)
        Public Property TotalRisk As Double
        Public Property TotalRiskWithControls As Double
        'Public property BowTieData

        Public Sub New(UserID As Integer)
            Me.UserID = UserID
            LikelihoodResults.UserID = UserID
            LikelihoodResults.HierarchyID = ECHierarchyID.hidLikelihood
            ImpactResults.UserID = UserID
            ImpactResults.HierarchyID = ECHierarchyID.hidImpact
        End Sub
    End Class


    <Serializable()> Public Class ASAData
        Public Property UserID As Integer
        Public Property Alternatives As New Dictionary(Of Integer, String)
        Public Property Objectives As New Dictionary(Of Integer, String)
        Public Property AlternativesPriorities As New Dictionary(Of Integer, Tuple(Of Double, Double))
        Public Property ObjectivesPriorities As New Dictionary(Of Integer, Tuple(Of Double, Double))
        Public Property ObjectivesPrioritiesByAlternative As New Dictionary(Of Integer, Dictionary(Of Integer, Tuple(Of Double, Double)))
        Public Property AlternativesPrioritiesByObjective As New Dictionary(Of Integer, Dictionary(Of Integer, Tuple(Of Double, Double)))
        Public Property MaxAlternativePriorityUnnormalized As Double
        Public Property MaxAlternativePriorityNormalized As Double
    End Class

    <Serializable()> Public Enum ControlsUsageMode
        DoNotUse = 0
        UseAll = 1
        UseOnlyActive = 2
    End Enum

    Public Enum SimulatedValuesUsageMode 'A1387
        Computed = 0
        SimulatedInput = 1
        SimulatedOutput = 2
        SimulatedInputAndOutput = 3 'A1399
    End Enum

    <Serializable()> Public Class NodePriority
        Public Property NodeID As Integer
        Public Property ParentID As Integer
        Public Property LocalPriority As Double
        Public Property GlobalPriority As Double
        Public Property NormalizedGlobalPriority As Double
        Public Property CanShowResults As Boolean 'A1045
    End Class

    <Serializable>
    Public Class PrioritiesCacheManager
        Private mEnabled As Boolean = False
        Public Property Enabled As Boolean
            Get
                Return False
                Return mEnabled
            End Get
            Set(value As Boolean)
                mEnabled = value
            End Set
        End Property


        Private mCalculated As New Dictionary(Of Integer, HashSet(Of Integer))
        Public ReadOnly Property ProjectManager As clsProjectManager

        Private StructureTypes As New List(Of StructureType)
        Private StructureTypesRisk As New List(Of StructureType)
        Private UserDataTypes As New List(Of UserDataType)
        Private UserDataTypesRisk As New List(Of UserDataType)

        Private Sub InitStreamsLists()
            StructureTypes.Add(StructureType.stModelStructure)
            StructureTypes.Add(StructureType.stPipeOptions)

            StructureTypesRisk.Add(StructureType.stModelStructure)
            StructureTypesRisk.Add(StructureType.stPipeOptions)
            StructureTypesRisk.Add(StructureType.stControls)

            UserDataTypes.Add(UserDataType.udtJudgments)
            UserDataTypes.Add(UserDataType.udtPermissions)
            UserDataTypes.Add(UserDataType.udtAttributeValues)
            UserDataTypes.Add(UserDataType.udtDisabledNodes)

            UserDataTypesRisk.Add(UserDataType.udtJudgments)
            UserDataTypesRisk.Add(UserDataType.udtJudgmentsControls)
            UserDataTypesRisk.Add(UserDataType.udtPermissions)
            UserDataTypesRisk.Add(UserDataType.udtPermissionsControls)
            UserDataTypesRisk.Add(UserDataType.udtAttributeValues)
            UserDataTypesRisk.Add(UserDataType.udtDisabledNodes)
        End Sub

        Private Function NeedToReload(UserID As Integer) As Boolean ' we have some new data in storage that can affect results, so we need to recalculate
            Dim structureList As List(Of StructureType) = If(ProjectManager.IsRiskProject, StructureTypesRisk, StructureTypes)
            Dim userDataList As List(Of UserDataType) = If(ProjectManager.IsRiskProject, UserDataTypesRisk, UserDataTypes)

            ' check model structure streams
            Dim i As Integer = 0
            While (i < structureList.Count)
                Dim time As Nullable(Of DateTime) = ProjectManager.CacheManager.StructureLoaded(structureList(i))
                If time Is Nothing Then
                    Debug.Print("Stream " + structureList(i).ToString + " is not loaded yet")
                    Return True
                Else
                    Dim storedTime As Nullable(Of DateTime) = ProjectManager.StorageManager.Reader.GetModelStructureStreamTime(structureList(i))
                    If storedTime Is Nothing Then
                        Return True
                    Else
                        If storedTime > time Then
                            Debug.Print("Stream " + structureList(i).ToString + " has time " + CType(time, DateTime).ToString("hh:mm:ss.ffff tt") + ", but database has time " + CType(storedTime, DateTime).ToString("hh:mm:ss.ffff tt"))
                            Return True
                        End If
                    End If
                End If

                i += 1
            End While

            Debug.Print("All streams are up-to-date")
            Return False

            ' check user data streams
        End Function

        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
            InitStreamsLists()
        End Sub

        Public Sub SetCalculated(UserID As Integer, HierarchyID As Integer, Value As Boolean)
            If Value Then
                If Not mCalculated.ContainsKey(UserID) Then mCalculated.Add(UserID, New HashSet(Of Integer))
                mCalculated(UserID).Add(HierarchyID)
            Else
                If mCalculated.ContainsKey(UserID) AndAlso mCalculated(UserID).Contains(HierarchyID) Then
                    mCalculated(UserID).Remove(HierarchyID)
                    ProjectManager.Hierarchy(HierarchyID).ClearUserWeights(UserID)
                End If
            End If
        End Sub

        Public Sub ClearCache()
            mCalculated.Clear()
        End Sub

        Public ReadOnly Property IsCalculated(UserID As Integer, HierarchyID As Integer) As Boolean
            Get
                'If NeedToReload(UserID) Then SetCalculated(UserID, HierarchyID, False)
                Return mCalculated.ContainsKey(UserID) AndAlso mCalculated(UserID).Contains(HierarchyID)
            End Get
        End Property
    End Class

    <Serializable()> Public Enum LikelihoodsCalculationMode
        Regular = 0
        Probability = 1
    End Enum

    <Serializable()> Public Enum ConsequencesSimulationsMode
        Diluted = 0
        Undiluted = 1
    End Enum

    <Serializable()> Public Class clsCalculationsManager

        Shared CalcCount As Integer = 0
        Public Shared Property CalculateCount As Integer = 0
        Public Property ProjectManager() As clsProjectManager
        Public Property CalculationTarget() As clsCalculationTarget
        Public ReadOnly Property PrioritiesCacheManager As PrioritiesCacheManager

        Public Property HierarchyID As Integer = 0
        Public Property AltsHierarchyID As Integer = 1

        Public Property LikelihoodsCalculationMode As LikelihoodsCalculationMode
            Get
                Return ProjectManager.Parameters.RiskionCalculationsMode
            End Get
            Set(value As LikelihoodsCalculationMode)
                ProjectManager.Parameters.RiskionCalculationsMode = value
                ProjectManager.Parameters.Save()
            End Set
        End Property

        Public Property ConsequenceSimulationsMode As ConsequencesSimulationsMode
            Get
                Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISKION_CONSEQUENCES_CALC_ID, UNDEFINED_USER_ID))
            End Get
            Set(value As ConsequencesSimulationsMode)
                With ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_RISKION_CONSEQUENCES_CALC_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property        

        Private mSynthMode As ECSynthesisMode
        Public Property SynthesisMode() As ECSynthesisMode
            Get
                Return If(ProjectManager.IsRiskProject, ECSynthesisMode.smIdeal, mSynthMode)
            End Get
            Set(ByVal value As ECSynthesisMode)
                mSynthMode = value
            End Set
        End Property

        Public Property IncludeIdealAlternative() As Boolean = False
        Public Property ShowIdealAlternative() As Boolean = False
        Public Property ShowDueToPriorities As Boolean ' = True 'A1590 - show Likelihoods ("due to priority") instead of "WRT priorities" in Riskion
            Get
                Return ProjectManager.Parameters.ShowLikelihoodsGivenSources
            End Get
            Set(value As Boolean)
                ProjectManager.Parameters.ShowLikelihoodsGivenSources = value
                ProjectManager.Parameters.Save()
            End Set
        End Property
        Public Property CalcForSA() As Boolean
        Public Property FullCalculate() As Boolean
        Public Property WRTNode() As clsNode
        Public Property SANode() As clsNode


        Private mStartingPriority As Single = 1
        Private mIdealAlt As Single

        Public Property UseSimulatedValues As Boolean
            Get
                If Not ProjectManager.IsRiskProject Then Return False
                Return ProjectManager.Parameters.Riskion_Use_Simulated_Values > 0
            End Get
            Set(value As Boolean)
                ProjectManager.Parameters.Riskion_Use_Simulated_Values = If(value, 2, 0)
            End Set
        End Property

        Public Property SubtractOpportunityEvents As Boolean = False

        Public Property UseBayesianUpdating As Boolean = True

        Public Enum AfterSimulationSynthesisModes
            UseSimulationFunctions = 0
            SumPrioritiesBasedOnSimulatedValues = 1
        End Enum

        Public Property AfterSimulationSynthesisMode As AfterSimulationSynthesisModes = AfterSimulationSynthesisModes.UseSimulationFunctions
        Public Property UseUserWeights() As Boolean = False
        Public Property IncludeRestrictedJudgments() As Boolean = True
        Public Property ForceKeepJudgmentsUpToDate() As Boolean = True
        Public Property CombinedMode() As CombinedCalculationsMode = CombinedCalculationsMode.cmAIJ
        Public Property UseNormalizationForSA As Boolean = False

        Private PrioritiesFromOtherHierarchy As New Dictionary(Of Integer, Double)

        ' key1 = hierarchy ID, key2 = objective ID, value = dict (alt ID, priority)
        Private TerminalWRTPrioritiesByObjective As New Dictionary(Of Integer, Dictionary(Of Integer, Dictionary(Of Integer, Double)))
        ' key1 = hierarchy ID, key2 = alt ID, value = dict (obj ID, priority)
        Private TerminalWRTPrioritiesByAlternative As New Dictionary(Of Integer, Dictionary(Of Integer, Dictionary(Of Integer, Double)))

        Public Property UseReductions As Boolean
            Get
                Return ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive
            End Get
            Set(value As Boolean)
                If value Then
                    ControlsUsageMode = ControlsUsageMode.UseOnlyActive
                Else
                    ControlsUsageMode = ControlsUsageMode.DoNotUse
                End If
            End Set
        End Property
        Public Property ControlsUsageMode As ControlsUsageMode = ControlsUsageMode.DoNotUse

        'Private mIsOpportunity As Boolean = False
        'Public Property IsOpportunity() As Boolean
        '    Get
        '        Return mIsOpportunity
        '    End Get
        '    Set(value As Boolean)
        '        mIsOpportunity = value
        '    End Set
        'End Property

        Public ReadOnly Property IsOpportunity(node As clsNode) As Boolean
            Get
                If ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptOpportunities Then  ' D4978
                    'If ProjectManager.RiskionProjectType = RiskionProjectType.Opportunity Then
                    Return True
                Else
                    If ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptRegular Then  ' D4978
                        'If ProjectManager.RiskionProjectType = RiskionProjectType.Risk Then
                        Return False
                    Else
                        ' in case of mixed mode
                        Return node.EventType = EventType.Opportunity
                    End If
                End If
            End Get
        End Property

        Private mUseCombinedForRestrictedNodes As Boolean = False

        Public Property UseCombinedForRestrictedNodes() As Boolean 'C0785
            Get
                Return mUseCombinedForRestrictedNodes
            End Get
            Set(ByVal value As Boolean)
                If value <> mUseCombinedForRestrictedNodes Then
                    For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                        node.Judgments.Weights.ClearUserWeights()
                    Next
                End If

                mUseCombinedForRestrictedNodes = value
            End Set
        End Property

        Public Overloads Sub Calculate(ByVal CalcTarget As clsCalculationTarget, ByVal WRTNode As clsNode,
            Optional ByVal HierarchyID As Integer = -1, Optional ByVal AltsHierarchyID As Integer = -1,
            Optional ByVal CalcForSA As Boolean = False, Optional ByVal SANode As clsNode = Nothing,
            Optional ByVal FullCalculate As Boolean = True, Optional StartingPriority As Single = 1)

            If HierarchyID = -1 Then HierarchyID = ProjectManager.ActiveHierarchy

            If AltsHierarchyID = -1 Then AltsHierarchyID = ProjectManager.ActiveAltsHierarchy

            Me.CalculationTarget = CalcTarget
            Me.WRTNode = WRTNode
            Me.HierarchyID = HierarchyID
            Me.AltsHierarchyID = AltsHierarchyID
            Me.CalcForSA = CalcForSA
            Me.FullCalculate = FullCalculate

            mStartingPriority = StartingPriority

            Calculate()

            Me.CalcForSA = False
        End Sub

        Protected Overridable Sub NormalizeAlternatives(ObjectivesHierarchy As clsHierarchy)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)

            Dim sumGlobal As Single = 0
            Dim sumGlobalWRT As Single = 0
            Dim sumGlobalSA As Single = 0
            Dim sumUnnormalized As Single = 0

            For Each node As clsNode In AH.TerminalNodes
                sumGlobalWRT += node.WRTGlobalPriority
                sumGlobalSA += node.SAGlobalPriority
                sumUnnormalized += node.UnnormalizedPriority
            Next

            Dim idealPrty As Single = 0
            Dim idealPrtyUnnorm As Single = 0
            For Each CovObj As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                If (CovObj Is WRTNode) OrElse ProjectManager.ActiveObjectives.IsChildOf(CovObj, WRTNode) Then
                    Dim ideal As Double = GetIdealLocalPriority(CalculationTarget.GetUserID, CovObj.NodeID)
                    idealPrty += CovObj.WRTGlobalPriority * ideal
                    idealPrtyUnnorm += CovObj.UnnormalizedPriority * ideal
                End If
            Next

            If IncludeIdealAlternative Then
                sumGlobalWRT += idealPrty
                sumGlobalSA += idealPrty
                sumUnnormalized += idealPrtyUnnorm
            End If

            For Each node As clsNode In AH.TerminalNodes
                If sumGlobalWRT <> 0 Then node.NormalizeGlobalPriorityWRT(1 / sumGlobalWRT)
                If sumGlobalSA <> 0 Then node.NormalizeGlobalPrioritySA(1 / sumGlobalSA)
                If SynthesisMode = ECSynthesisMode.smDistributive Then
                    If sumUnnormalized <> 0 Then node.NormalizeUnnormalizedPriority(1 / sumUnnormalized)
                End If
            Next
            If sumGlobalWRT <> 0 Then mIdealAlt = idealPrty / sumGlobalWRT
        End Sub

        Private Function GetContributedCoveringObjectives(Alt As clsNode, WRTNode As clsNode, TerminalNode As List(Of clsNode)) As List(Of clsNode)
            If Alt Is Nothing Or WRTNode Is Nothing Then Return Nothing

            Dim res As New List(Of clsNode)

            Dim MH As clsHierarchy = WRTNode.Hierarchy

            For Each CovObj As clsNode In TerminalNode
                If (WRTNode.IsTerminalNode And (WRTNode Is CovObj)) Or
                    (Not WRTNode.IsTerminalNode And MH.IsChildOf(CovObj, WRTNode)) Then

                    If CovObj.GetNodesBelow(UNDEFINED_USER_ID).Contains(Alt) Then
                        res.Add(CovObj)
                    End If
                End If
            Next

            Return res
        End Function

        Private Sub ResetGlobalPriorities()
            Dim hList As New List(Of clsHierarchy)
            hList.Add(ProjectManager.Hierarchy(HierarchyID))
            hList.Add(ProjectManager.AltsHierarchy(AltsHierarchyID))

            For Each H As clsHierarchy In hList
                For Each node As clsNode In H.Nodes
                    node.WRTGlobalPriority = 0.0
                    node.UnnormalizedPriority = 0.0
                    node.UnnormalizedPriorityWithoutControls = 0.0

                    node.SAGlobalPriority = 0.0
                    node.SALocalPriority = 0.0

                    If HierarchyID = ECHierarchyID.hidImpact AndAlso WRTNode Is ProjectManager.Hierarchy(HierarchyID).Nodes(0) Then node.DollarValue = 0
                Next
            Next
        End Sub

        Protected Overridable Sub DetachAlternatives(CalculationTarget As clsCalculationTarget)
            Dim MH As clsHierarchy = ProjectManager.Hierarchy(HierarchyID)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)

            Dim userID As Integer = CalculationTarget.GetUserID()

            Dim hasDollarValuesForImpact As Boolean = False
            '-A1291 If ProjectManager.IsRiskProject AndAlso UseDollarValue AndAlso MH.HierarchyID = ECHierarchyID.hidImpact Then
            If ProjectManager.IsRiskProject AndAlso MH.HierarchyID = ECHierarchyID.hidImpact Then
                hasDollarValuesForImpact = HasDollarValueSet(MH)
                If hasDollarValuesForImpact AndAlso WRTNode Is MH.Nodes(0) Then
                    ResetDollarValues() 'A1319
                    CalculateImpactDollarValues()
                End If
            End If

            If ProjectManager.IsRiskProject Then
                For Each CovObj As clsNode In MH.Nodes
                    CovObj.AppliedControl = False
                Next
            End If

            Dim tEnabledControls As New List(Of clsControl)
            If ProjectManager.IsRiskProject Then
                tEnabledControls = ProjectManager.Controls.EnabledControls
            End If

            Dim alts As List(Of clsNode) = AH.TerminalNodes
            Dim isWRTTerminal As Boolean = WRTNode.IsTerminalNode

            For Each CovObj As clsNode In MH.TerminalNodes
                If CovObj.WRTGlobalPriority <> 0 Then
                    'Dim nodesBelow As List(Of clsNode) = CovObj.GetNodesBelow(userID)
                    Dim nodesBelow As List(Of clsNode) = CovObj.GetNodesBelow(UNDEFINED_USER_ID)
                    For Each alt As clsNode In nodesBelow
                        If (WRTNode Is MH.Nodes(0)) OrElse (isWRTTerminal AndAlso (WRTNode Is CovObj)) OrElse
                        (Not isWRTTerminal AndAlso MH.IsChildOf(CovObj, WRTNode)) Then

                            Dim CovObjReduction As Single = 1
                            Dim AltWRTReduction As Single = 1
                            If ProjectManager.IsRiskProject Then
                                If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                    For Each control As clsControl In tEnabledControls 'A1383
                                        If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                            For Each assignment As clsControlAssignment In control.Assignments
                                                If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                    If assignment.EventID.Equals(Guid.Empty) Then
                                                        If assignment.ObjectiveID.Equals(CovObj.NodeGuidID) Then
                                                            CovObjReduction *= 1 + If(IsOpportunity(CovObj), assignment.Value, -assignment.Value)
                                                        End If
                                                    Else
                                                        If assignment.ObjectiveID.Equals(CovObj.NodeGuidID) AndAlso assignment.EventID.Equals(alt.NodeGuidID) Then
                                                            AltWRTReduction *= 1 + If(IsOpportunity(alt), assignment.Value, -assignment.Value)
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                End If
                            End If

                            Dim OriginalObjWRTGlobalPriority As Single = CovObj.UnnormalizedPriority

                            If Not CovObj.AppliedControl Then
                                'CovObj.WRTGlobalPriority *= CovObjReduction
                                CovObj.UnnormalizedPriority *= CovObjReduction

                                CovObj.WRTRelativeBPriority *= CovObjReduction
                                CovObj.SimulatedPriority *= CovObjReduction
                                CovObj.AppliedControl = True
                            End If

                            Dim value As Double = CovObj.Judgments.Weights.GetUserWeights(userID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                            Dim unnormalizedValue As Double = CovObj.Judgments.Weights.GetUserWeights(userID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)

                            alt.WRTGlobalPriority += CovObj.WRTGlobalPriority * value * AltWRTReduction
                            alt.UnnormalizedPriority += CovObj.UnnormalizedPriority * unnormalizedValue * AltWRTReduction
                            alt.SAGlobalPriority += CovObj.SAGlobalPriority * value * AltWRTReduction

                            If ProjectManager.IsRiskProject AndAlso hasDollarValuesForImpact Then
                                alt.UnnormalizedPriorityWithoutControls += OriginalObjWRTGlobalPriority * unnormalizedValue
                                alt.SimulatedPriority += CovObj.SimulatedPriority * unnormalizedValue * AltWRTReduction
                                If hasDollarValuesForImpact Then
                                    alt.DollarValue += CovObj.DollarValue * value * AltWRTReduction
                                End If
                            End If
                        End If
                    Next
                End If
            Next

            If ProjectManager.IsRiskProject And MH.HierarchyID = ECHierarchyID.hidLikelihood Then
                For Each alt As clsNode In MH.GetUncontributedAlternatives
                    Dim CovObjReduction As Single = 1
                    Dim AltWRTReduction As Single = 1
                    If ControlsUsageMode = ControlsUsageMode.UseAll OrElse ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                        For Each control As clsControl In tEnabledControls 'A1383
                            If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                For Each assignment As clsControlAssignment In control.Assignments
                                    If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                        If assignment.EventID.Equals(Guid.Empty) Then
                                            If assignment.ObjectiveID.Equals(MH.Nodes(0).NodeGuidID) Then
                                                CovObjReduction *= 1 + If(IsOpportunity(MH.Nodes(0)), assignment.Value, -assignment.Value)
                                            End If
                                        Else
                                            If assignment.ObjectiveID.Equals(MH.Nodes(0).NodeGuidID) AndAlso assignment.EventID.Equals(alt.NodeGuidID) Then
                                                AltWRTReduction *= 1 + If(IsOpportunity(alt), assignment.Value, -assignment.Value)
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    End If

                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        alt.CalculateLocal(CalculationTarget)
                        'Dim value As Single = alt.PWOutcomesJudgments.GetWeight(CalculationTarget.GetUserID, alt.NodeID)
                        Dim valueWithoutControls As Double = alt.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, alt.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, alt.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                        Dim valueWithControls As Double = valueWithoutControls * CovObjReduction * AltWRTReduction
                        alt.WRTGlobalPriority = valueWithControls
                        alt.UnnormalizedPriority = valueWithControls
                        alt.UnnormalizedPriorityWithoutControls = valueWithoutControls
                        alt.SAGlobalPriority = valueWithControls
                    Else
                        Dim J As clsNonPairwiseMeasureData = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, MH.Nodes(0).NodeID, CalculationTarget.GetUserID)
                        Dim valueWithoutControls As Double = If(J IsNot Nothing, J.SingleValue, 0)
                        'Dim valueWithoutControls As Double = alt.DirectJudgmentsForNoCause.Weights.GetUserWeights(CalculationTarget.GetUserID, alt.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, alt.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                        Dim valueWithControls As Double = valueWithoutControls * CovObjReduction * AltWRTReduction
                        Dim normValue As Single = If(J IsNot Nothing, J.AltNormalizedValue, 0)

                        alt.WRTGlobalPriority = valueWithControls
                        alt.UnnormalizedPriority = valueWithControls
                        alt.UnnormalizedPriorityWithoutControls = valueWithoutControls
                        alt.SAGlobalPriority = valueWithControls
                    End If
                Next

                Dim sortedAlts As List(Of clsNode)
                Dim str As String
                If Not ProjectManager.Edges.TopologicalSort(sortedAlts, str) Then
                    For i As Integer = 0 To sortedAlts.Count - 1
                        For j As Integer = 0 To i - 1
                            If ProjectManager.Edges.Edges.ContainsKey(sortedAlts(j).NodeGuidID) Then
                                For Each edge As Edge In ProjectManager.Edges.Edges(sortedAlts(j).NodeGuidID)
                                    If edge.ToNode Is sortedAlts(i) Then
                                        Dim nonpwData As clsNonPairwiseMeasureData = edge.FromNode.EventsJudgments.GetJudgement(edge.ToNode.NodeID, edge.FromNode.NodeID, CalculationTarget.GetUserID)
                                        If nonpwData IsNot Nothing Then
                                            Dim value As Double = nonpwData.SingleValue

                                            Dim AltWRTReduction As Single = 1
                                            If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                                For Each control As clsControl In tEnabledControls 'A1383
                                                    If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                                        For Each assignment As clsControlAssignment In control.Assignments
                                                            If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                                If Not assignment.EventID.Equals(Guid.Empty) Then
                                                                    If assignment.ObjectiveID.Equals(sortedAlts(j).NodeGuidID) AndAlso assignment.EventID.Equals(edge.ToNode.NodeGuidID) Then
                                                                        AltWRTReduction *= 1 - assignment.Value
                                                                    End If
                                                                End If
                                                            End If
                                                        Next
                                                    End If
                                                Next
                                            End If

                                            value *= AltWRTReduction

                                            edge.ToNode.UnnormalizedPriority += value * edge.FromNode.UnnormalizedPriority
                                            edge.ToNode.WRTGlobalPriority += value * edge.FromNode.WRTGlobalPriority
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    Next
                End If
            End If

            '-A1291 If ProjectManager.IsRiskProject AndAlso UseDollarValue AndAlso MH.HierarchyID = ECHierarchyID.hidImpact Then
            If ProjectManager.IsRiskProject AndAlso MH.HierarchyID = ECHierarchyID.hidImpact Then
                Dim hasEventsDollarValue = HasDollarValueSet(AH)
                If hasEventsDollarValue Then CalculateEventsDollarValues()
            End If

            If ProjectManager.IsRiskProject Then
                For Each alt As clsNode In AH.Nodes
                    alt.UnnormalizedPriorityBeforeBayes = alt.UnnormalizedPriority
                    alt.UnnormalizedPriorityWithoutControlsBeforeBayes = alt.UnnormalizedPriorityWithoutControls
                Next
            End If

            If ProjectManager.IsRiskProject AndAlso UseBayesianUpdating Then
                ProjectManager.BayesianCalculationManager.Calculate()
            End If
        End Sub

        Public Overridable Sub CreateCombinedJudgments(ByVal CombinedGroup As clsCombinedGroup, Optional ByVal NodesToCombined As List(Of clsNode) = Nothing)
            MiscFuncs.PrintDebugInfo("Create combined judgments for group '" + CombinedGroup.Name + "' started - ")

            Dim watch As Stopwatch = System.Diagnostics.Stopwatch.StartNew()
            watch.Stop()

            Dim MH As clsHierarchy = ProjectManager.Hierarchy(HierarchyID)

            Dim dummyUser As New clsUser
            dummyUser.UserID = CombinedGroup.CombinedUserID
            ProjectManager.CleanUpUserDataFromMemory(HierarchyID, CombinedGroup.CombinedUserID)
            ProjectManager.AddEmptyMissingJudgments(HierarchyID, AltsHierarchyID, dummyUser, -1, True)

            Dim WeightsSum As Double = CombinedGroup.GetWeightsSum

            ProjectManager.StorageManager.Reader.AddJudgmentsToCombined(CombinedGroup, MH)
            For Each node As clsNode In MH.Nodes
                Select Case node.MeasureType
                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                        For Each pwData As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(CombinedGroup.CombinedUserID)
                            If pwData.UsersCount <> 0 Then
                                If UseUserWeights AndAlso WeightsSum > 0 Then
                                    Dim sum As Double = pwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                    If sum <> 0 Then
                                        pwData.Value = 1
                                        For Each ad As AggregatedData In pwData.AggregatedValues
                                            pwData.Value *= Math.Pow(ad.Value, ad.Weight / sum)
                                        Next
                                    End If
                                Else
                                    pwData.Value = Math.Pow(pwData.AggregatedValue, 1 / pwData.UsersCount)
                                    Dim value As Double = 1
                                    For Each v As Double In pwData.AggregatedValues2
                                        value *= Math.Pow(v, 1 / pwData.UsersCount)
                                    Next
                                    pwData.Value = value
                                End If
                                If pwData.Value > 1 Then
                                    pwData.Advantage = 1
                                Else
                                    pwData.Advantage = -1
                                    pwData.Value = 1 / pwData.Value
                                End If
                                pwData.IsUndefined = False
                            Else
                                pwData.IsUndefined = True
                            End If
                        Next
                    Case ECMeasureType.mtPWOutcomes
                        For Each child As clsNode In node.GetNodesBelow(UNDEFINED_USER_ID)
                            For Each pwData As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(CombinedGroup.CombinedUserID)
                                If pwData.UsersCount <> 0 Then
                                    If UseUserWeights AndAlso WeightsSum > 0 Then
                                        Dim sum As Double = pwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                        If sum <> 0 Then
                                            pwData.Value = 1
                                            For Each ad As AggregatedData In pwData.AggregatedValues
                                                pwData.Value *= Math.Pow(ad.Value, ad.Weight / sum)
                                            Next
                                        End If
                                    Else
                                        pwData.Value = Math.Pow(pwData.AggregatedValue, 1 / pwData.UsersCount)
                                        Dim value As Double = 1
                                        For Each v As Double In pwData.AggregatedValues2
                                            value *= Math.Pow(v, 1 / pwData.UsersCount)
                                        Next
                                        pwData.Value = value
                                    End If
                                    If pwData.Value > 1 Then
                                        pwData.Advantage = 1
                                    Else
                                        pwData.Advantage = -1
                                        pwData.Value = 1 / pwData.Value
                                    End If
                                    pwData.IsUndefined = False
                                Else
                                    pwData.IsUndefined = True
                                End If
                            Next
                        Next
                    Case Else
                        For Each nonpwData As clsNonPairwiseMeasureData In node.Judgments.JudgmentsFromUser(CombinedGroup.CombinedUserID)
                            If nonpwData.UsersCount <> 0 Then
                                Select Case node.MeasureType
                                    Case ECMeasureType.mtRatings
                                        Dim R As clsRating = New clsRating
                                        R.ID = -1
                                        R.Name = "Combined Rating"
                                        If UseUserWeights AndAlso WeightsSum > 0 Then
                                            R.Value = 0
                                            Dim sum As Double = nonpwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                            If sum <> 0 Then
                                                For Each ad As AggregatedData In nonpwData.AggregatedValues
                                                    R.Value += ad.Value * ad.Weight / sum
                                                Next
                                            End If
                                        Else
                                            R.Value = nonpwData.AggregatedValue / nonpwData.UsersCount
                                        End If

                                        nonpwData.ObjectValue = R
                                    Case Else
                                        If UseUserWeights AndAlso WeightsSum > 0 Then
                                            nonpwData.ObjectValue = 0
                                            Dim sum As Double = nonpwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                            If sum <> 0 Then
                                                For Each ad As AggregatedData In nonpwData.AggregatedValues
                                                    nonpwData.ObjectValue += ad.Value * ad.Weight / sum
                                                Next
                                            End If
                                        Else
                                            nonpwData.ObjectValue = nonpwData.AggregatedValue / nonpwData.UsersCount
                                        End If
                                End Select
                                nonpwData.IsUndefined = False
                            Else
                                nonpwData.IsUndefined = True
                            End If
                        Next
                End Select
            Next

            For Each alt As clsNode In MH.GetUncontributedAlternatives
                If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                    For Each pwData As clsPairwiseMeasureData In alt.PWOutcomesJudgments.JudgmentsFromUser(CombinedGroup.CombinedUserID)
                        If pwData.UsersCount <> 0 Then
                            If UseUserWeights AndAlso WeightsSum > 0 Then
                                Dim sum As Double = pwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                If sum <> 0 Then
                                    pwData.Value = 1
                                    For Each ad As AggregatedData In pwData.AggregatedValues
                                        pwData.Value *= Math.Pow(ad.Value, ad.Weight / sum)
                                    Next
                                End If
                            Else
                                pwData.Value = Math.Pow(pwData.AggregatedValue, 1 / pwData.UsersCount)
                                Dim value As Double = 1
                                For Each v As Double In pwData.AggregatedValues2
                                    value *= Math.Pow(v, 1 / pwData.UsersCount)
                                Next
                                pwData.Value = value
                            End If
                            If pwData.Value > 1 Then
                                pwData.Advantage = 1
                            Else
                                pwData.Advantage = -1
                                pwData.Value = 1 / pwData.Value
                            End If
                            pwData.IsUndefined = False
                        Else
                            pwData.IsUndefined = True
                        End If
                    Next
                Else
                    For Each nonpwData As clsNonPairwiseMeasureData In alt.DirectJudgmentsForNoCause.JudgmentsFromUser(CombinedGroup.CombinedUserID)
                        If nonpwData.UsersCount <> 0 Then
                            If alt.MeasureType = ECMeasureType.mtRatings Then
                                Dim R As clsRating = New clsRating
                                R.ID = -1
                                R.Name = "Combined Rating"
                                If UseUserWeights AndAlso WeightsSum > 0 Then
                                    R.Value = 0
                                    Dim sum As Double = nonpwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                    If sum <> 0 Then
                                        For Each ad As AggregatedData In nonpwData.AggregatedValues
                                            R.Value += ad.Value * ad.Weight / sum
                                        Next
                                    End If
                                Else
                                    R.Value = nonpwData.AggregatedValue / nonpwData.UsersCount
                                End If

                                nonpwData.ObjectValue = R
                            Else
                                If UseUserWeights AndAlso WeightsSum > 0 Then
                                    nonpwData.ObjectValue = 0
                                    Dim sum As Double = nonpwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                    If sum <> 0 Then
                                        For Each ad As AggregatedData In nonpwData.AggregatedValues
                                            nonpwData.ObjectValue += ad.Value * ad.Weight / sum
                                        Next
                                    End If
                                Else
                                    nonpwData.ObjectValue = nonpwData.AggregatedValue / nonpwData.UsersCount
                                End If
                            End If
                            nonpwData.IsUndefined = False
                        Else
                            nonpwData.IsUndefined = True
                        End If
                    Next
                End If
            Next

            For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                For Each nonpwData As clsNonPairwiseMeasureData In alt.EventsJudgments.JudgmentsFromUser(CombinedGroup.CombinedUserID)
                    If nonpwData.UsersCount <> 0 Then
                        Dim mt As ECMeasureType = ECMeasureType.mtDirect
                        'TODO: Nexteer - handle MTs
                        If mt = ECMeasureType.mtRatings Then
                            Dim R As clsRating = New clsRating
                            R.ID = -1
                            R.Name = "Combined Rating"
                            If UseUserWeights AndAlso WeightsSum > 0 Then
                                R.Value = 0
                                Dim sum As Double = nonpwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                If sum <> 0 Then
                                    For Each ad As AggregatedData In nonpwData.AggregatedValues
                                        R.Value += ad.Value * ad.Weight / sum
                                    Next
                                End If
                            Else
                                R.Value = nonpwData.AggregatedValue / nonpwData.UsersCount
                            End If

                            nonpwData.ObjectValue = R
                        Else
                            If UseUserWeights AndAlso WeightsSum > 0 Then
                                nonpwData.ObjectValue = 0
                                Dim sum As Double = nonpwData.AggregatedValues.Sum(Function(w) (w.Weight))
                                If sum <> 0 Then
                                    For Each ad As AggregatedData In nonpwData.AggregatedValues
                                        nonpwData.ObjectValue += ad.Value * ad.Weight / sum
                                    Next
                                End If
                            Else
                                nonpwData.ObjectValue = nonpwData.AggregatedValue / nonpwData.UsersCount
                            End If
                        End If
                        nonpwData.IsUndefined = False
                    Else
                        nonpwData.IsUndefined = True
                    End If

                    'Dim md As clsDirectMeasureData = alt.EventsJudgments.GetJudgement(nonpwData.ParentNodeID, nonpwData.NodeID, CombinedGroup.CombinedUserID)
                    'If md IsNot Nothing Then
                    '    md.DirectData = nonpwData.SingleValue
                    '    md.IsUndefined = nonpwData.IsUndefined
                    'Else
                    '    alt.EventsJudgments.AddMeasureData(New clsDirectMeasureData(nonpwData.NodeID, nonpwData.ParentNodeID, CombinedGroup.CombinedUserID, nonpwData.SingleValue, nonpwData.IsUndefined), False)
                    'End If
                Next
            Next

            MH.SetCombinedLoaded(CombinedGroup.CombinedUserID, True)
            MiscFuncs.PrintDebugInfo("Create combined judgments for group '" + CombinedGroup.Name + "' ended - ")

            Dim elapsedMs As Long = watch.ElapsedMilliseconds

            If ProjectManager.AddLog IsNot Nothing Then
                ProjectManager.AddLog("Project id=" + ProjectManager.StorageManager.ModelID.ToString + " created combined judgments in  " + elapsedMs.ToString + " ms")
            End If
        End Sub

        Private Function GetNodesWithRestrictedPermissions(ByVal UserID As Integer) As List(Of clsNode)
            Dim res As New List(Of clsNode)

            For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
                If Not node.IsAllowed(UserID) Then 'C0901
                    res.Add(node)
                Else
                    If node.IsTerminalNode Then
                        If node.GetNodesBelow(UserID).Count <> node.GetNodesBelow(UNDEFINED_USER_ID).Count Then
                            res.Add(node)
                        End If
                    End If
                End If
            Next

            Return res
        End Function

        Private Overloads Sub Calculate()
            Dim forcePrintDebug As Boolean = True
            MiscFuncs.PrintDebugInfo("Calculations started: ", forcePrintDebug)

            If CalculationTarget.TargetType = CalculationTargetType.cttCombinedGroup And CombinedMode = CombinedCalculationsMode.cmAIPTotals Then
                CalculateAIPTotals()
                Exit Sub
            End If

            Dim MH As clsHierarchy = ProjectManager.Hierarchy(HierarchyID)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)

            ResetGlobalPriorities()

            If FullCalculate AndAlso (Not PrioritiesCacheManager.Enabled OrElse PrioritiesCacheManager.Enabled AndAlso Not PrioritiesCacheManager.IsCalculated(CalculationTarget.GetUserID, MH.HierarchyID)) Then
                Dim CombinedCreatedForDefaultCombinedGroup As Boolean = False
                If CalculationTarget.TargetType = CalculationTargetType.cttCombinedGroup AndAlso CombinedMode = CombinedCalculationsMode.cmAIJ Then
                    CreateCombinedJudgments(CType(CalculationTarget.Target, clsCombinedGroup))
                    MiscFuncs.PrintDebugInfo("Combined created: ", forcePrintDebug)
                    CombinedCreatedForDefaultCombinedGroup = (CalculationTarget.GetUserID = COMBINED_USER_ID)
                End If

                If UseCombinedForRestrictedNodes And Not CombinedCreatedForDefaultCombinedGroup Then
                    If CalculationTarget.TargetType = CalculationTargetType.cttUser Then
                        ProjectManager.StorageManager.Reader.LoadUserPermissions(CType(CalculationTarget.Target, clsUser))
                    End If
                    Dim NodesWithRestricted As List(Of clsNode) = GetNodesWithRestrictedPermissions(CalculationTarget.GetUserID)
                    If NodesWithRestricted.Count > 0 Then
                        CreateCombinedJudgments(ProjectManager.CombinedGroups.GetDefaultCombinedGroup, NodesWithRestricted)
                    End If
                End If

                MH.CalculateLocal(CalculationTarget, WRTNode)
                MiscFuncs.PrintDebugInfo("CalculateLocal completed: ", forcePrintDebug)

                CalcCount += 1
                For Each alt As clsNode In MH.GetUncontributedAlternatives
                    If alt.MeasureType = ECMeasureType.mtPWOutcomes Then
                        alt.PWOutcomesJudgments.CalculateWeights(CalculationTarget)
                    End If
                Next
                If PrioritiesCacheManager.Enabled Then
                    PrioritiesCacheManager.SetCalculated(CalculationTarget.GetUserID, MH.HierarchyID, True)
                End If
            End If

            CalculateCount += 1
            MH.CalculateWRTGlobal(CalculationTarget, WRTNode, mStartingPriority)
            MiscFuncs.PrintDebugInfo("Calculate global completed: ", forcePrintDebug)

            If AH.Nodes.Count <> 0 Then
                DetachAlternatives(CalculationTarget)
                MiscFuncs.PrintDebugInfo("DetachAlternatives completed: ", forcePrintDebug)
                NormalizeAlternatives(MH)
                MiscFuncs.PrintDebugInfo("NormalizeAlternatives completed: ", forcePrintDebug)
            End If

            If ProjectManager.FeedbackOn Then
                CalculateFeedback(MH, AH, CalculationTarget.GetUserID)
            End If
            MiscFuncs.PrintDebugInfo("Calculations ended: ", forcePrintDebug)
        End Sub

        Private Sub CalculateAIPTotals()
            Dim MH As clsHierarchy = ProjectManager.Hierarchy(HierarchyID)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)

            Dim userCount As Integer = 0

            Dim altsValues As New List(Of clsWeight)
            For Each alt As clsNode In AH.TerminalNodes
                Dim w As clsWeight = New clsWeight(alt.NodeID, 0)
                w.UnnormValue = 0
                w.Value = 0
                altsValues.Add(w)
            Next

            Dim usersCount As Integer = 0

            Dim users As List(Of clsUser) = CType(CalculationTarget.Target, clsCombinedGroup).UsersList.ToList

            Dim JA As New clsJudgmentsAnalyzer(SynthesisMode, ProjectManager)

            For Each user As clsUser In users
                ProjectManager.StorageManager.Reader.LoadUserData(user)
                Dim userTarget As New clsCalculationTarget(CalculationTargetType.cttUser, user)
                Calculate(userTarget, WRTNode, HierarchyID, AltsHierarchyID)

                If JA.CanShowIndividualResults(user.UserID, WRTNode) Then
                    Dim sum As Single = 0
                    Dim sumUnnorm As Single = 0
                    For Each alt As clsNode In AH.TerminalNodes
                        sum += alt.WRTGlobalPriority
                        sumUnnorm += alt.UnnormalizedPriority
                    Next

                    For Each alt As clsNode In AH.TerminalNodes
                        For Each w As clsWeight In altsValues
                            If w.ChildNodeID = alt.NodeID Then
                                w.Value += alt.WRTGlobalPriority
                                w.UnnormValue += alt.UnnormalizedPriority
                                Exit For
                            End If
                        Next
                    Next
                    userCount += 1
                End If
            Next

            If userCount <> 0 Then
                Dim sum As Single = 0
                Dim sumUnnorm As Single = 0
                For Each w As clsWeight In altsValues
                    w.Value /= userCount
                    w.UnnormValue /= userCount
                    sum += w.Value
                    sumUnnorm += w.UnnormValue
                Next
                If sum <> 0 Then
                    For Each w As clsWeight In altsValues
                        w.Value /= sum
                        AH.GetNodeByID(w.ChildNodeID).WRTGlobalPriority = w.Value
                        AH.GetNodeByID(w.ChildNodeID).UnnormalizedPriority = w.UnnormValue
                    Next
                End If
            End If
        End Sub

        Public Function GetRiskDataWRTNode(ByVal UserID As Integer, UserEmail As String, ByVal WrtNodeGuid As Guid, Optional ByVal cUsageMode As ControlsUsageMode = ControlsUsageMode.DoNotUse, Optional EventsList As List(Of Guid) = Nothing) As RiskDataWRTNodeDataContract 'A1064
            Dim res As New RiskDataWRTNodeDataContract
            res.AlternativesData = New List(Of AlternativeRiskDataDataContract)

            Dim oldUsageMode As ControlsUsageMode = ControlsUsageMode
            ControlsUsageMode = cUsageMode

            With ProjectManager
                Dim CurrentHID As Integer = .ActiveHierarchy

                ' calculating for Likelihood
                .ActiveHierarchy = ECHierarchyID.hidLikelihood
                Dim LikelihoodWRTNode As ECCore.clsNode = .Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(WrtNodeGuid)
                If LikelihoodWRTNode Is Nothing Then LikelihoodWRTNode = .Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)

                Dim AltRiskData As AlternativeRiskDataDataContract

                Dim CalcTarget As clsCalculationTarget = Nothing
                If IsCombinedUserID(UserID) Then
                    Dim CG As ECCore.clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(UserID)
                    If CG IsNot Nothing Then CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                Else
                    Dim user As ECCore.clsUser = .GetUserByEMail(UserEmail)
                    If user IsNot Nothing Then
                        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                        If Not PrioritiesCacheManager.Enabled OrElse (PrioritiesCacheManager.Enabled AndAlso Not ProjectManager.CalculationsManager.PrioritiesCacheManager.IsCalculated(user.UserID, ECHierarchyID.hidLikelihood) AndAlso Not ProjectManager.CalculationsManager.PrioritiesCacheManager.IsCalculated(user.UserID, ECHierarchyID.hidImpact)) Then
                            .StorageManager.Reader.LoadUserData(user)
                        End If

                        If user Is .User Then .AddEmptyMissingJudgments(.ActiveHierarchy, .ActiveAltsHierarchy)
                    End If
                End If

                Calculate(CalcTarget, .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
                Dim LikelihoodWRTNodeUnnormalizedPriority As Double = CDbl(If(LikelihoodWRTNode Is .Hierarchy(.ActiveHierarchy).Nodes(0) OrElse Not ShowDueToPriorities, 1, LikelihoodWRTNode.UnnormalizedPriority))

                Calculate(CalcTarget, LikelihoodWRTNode, .ActiveHierarchy, .ActiveAltsHierarchy)

                'res.IsSOEG1 = SumOfEventsMoreThanOne()
                'res.IsSOSG1 = SumOfClusterMoreThanOne(CalculationTarget)
                'res.IsEG1 = AnyEventMoreThanOne()

                '-A1406 If .Hierarchy(.ActiveHierarchy).Nodes(0) IsNot LikelihoodWRTNode Then
                '    '-A1354 based on gtm with AC .Hierarchy(.ActiveHierarchy).CalculateWRTGlobal(CalcTarget, LikelihoodWRTNode)
                '    .CalculationsManager.Calculate(CalcTarget, LikelihoodWRTNode, .ActiveHierarchy, .ActiveAltsHierarchy) 'A1354 based on gtm with AC
                'End If

                Dim TotalLikelihood As Double = 0

                'Debug.Print("Likelihood values: ")

                Dim altsList As New List(Of clsNode)
                If EventsList Is Nothing Then
                    altsList = .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes
                Else
                    altsList = .AltsHierarchy(.ActiveAltsHierarchy).TerminalNodes.Where(Function(x) (EventsList.Contains(x.NodeGuidID))).Select(Function(x) (x)).ToList
                End If

                Dim uncontributedAlts As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives

                Dim EventsLikelihoods As New Dictionary(Of Integer, Double)

                For Each alt As ECCore.clsNode In altsList.Where(Function(a) a.Enabled)
                    AltRiskData = New AlternativeRiskDataDataContract
                    AltRiskData.AlternativeID = alt.NodeGuidID
                    AltRiskData.intID = alt.NodeID

                    If UseSimulatedValues OrElse LikelihoodsCalculationMode = LikelihoodsCalculationMode.Regular OrElse uncontributedAlts.Contains(alt) Then
                        AltRiskData.LikelihoodValue = If(UseSimulatedValues, alt.SimulatedAltLikelihood, alt.UnnormalizedPriority) * LikelihoodWRTNodeUnnormalizedPriority  'A1003
                        AltRiskData.WRTGlobalPriorityLikelihood = LikelihoodWRTNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID) 'Always 0
                    Else
                        Dim covObjs As List(Of clsNode) = ProjectManager.GetContributedCoveringObjectives(alt, .ActiveObjectives)
                        Dim covObjsIDs As List(Of Guid) = covObjs.Select(Function(x) x.NodeGuidID).ToList

                        ' list of CE groups with sources from contributed objectives
                        Dim MELists As New Dictionary(Of Guid, List(Of Guid))
                        Dim MESourcesInSomeGroups As New HashSet(Of Guid)

                        For Each group As EventsGroup In ProjectManager.SourceGroups.Groups
                            If group.Behaviour = GroupBehaviour.MutuallyExclusive Or group.Behaviour = GroupBehaviour.MutuallyExclusiveExhaustive Then
                                Dim MEGroupObjs As New List(Of Guid)
                                For Each data As EventGroupData In group.Events
                                    If covObjsIDs.Contains(data.EventGuidID) Then
                                        MEGroupObjs.Add(data.EventGuidID)
                                        MESourcesInSomeGroups.Add(data.EventGuidID)
                                    End If
                                Next
                                If MEGroupObjs.Count > 0 Then
                                    MELists.Add(group.ID, MEGroupObjs)
                                End If
                            End If
                        Next

                        Dim M As Double = 1


                        Dim UseControls As Boolean = ProjectManager.CalculationsManager.ControlsUsageMode <> ControlsUsageMode.DoNotUse
                        Dim tEnabledControls As List(Of clsControl) = ProjectManager.Controls.EnabledControls


                        Dim independentSources As New List(Of clsNode)
                        For Each obj As clsNode In covObjs
                            Dim CovObjReduction As Single = 1
                            Dim AltWRTReduction As Single = 1
                            If UseControls Then
                                If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                    For Each control As clsControl In tEnabledControls 'A1383
                                        If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                            For Each assignment As clsControlAssignment In control.Assignments
                                                If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                    If assignment.EventID.Equals(Guid.Empty) Then
                                                        If assignment.ObjectiveID.Equals(obj.NodeGuidID) Then
                                                            CovObjReduction *= 1 + If(IsOpportunity(obj), assignment.Value, -assignment.Value)
                                                        End If
                                                    Else
                                                        If assignment.ObjectiveID.Equals(obj.NodeGuidID) AndAlso assignment.EventID.Equals(alt.NodeGuidID) Then
                                                            AltWRTReduction *= 1 + If(IsOpportunity(alt), assignment.Value, -assignment.Value)
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                End If
                            End If

                            If Not MESourcesInSomeGroups.Contains(obj.NodeGuidID) Then
                                independentSources.Add(obj)
                                M *= (1 - obj.UnnormalizedPriority * CovObjReduction * obj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID) * AltWRTReduction)
                            End If
                        Next

                        Dim sortedAlts As List(Of clsNode)
                        Dim str As String
                        If Not ProjectManager.Edges.TopologicalSort(sortedAlts, str) Then
                            For i As Integer = 0 To sortedAlts.Count - 1
                                If sortedAlts(i).NodeID = alt.NodeID Then
                                    For j As Integer = 0 To i - 1
                                        If ProjectManager.Edges.Edges.ContainsKey(sortedAlts(j).NodeGuidID) Then
                                            For Each edge As Edge In ProjectManager.Edges.Edges(sortedAlts(j).NodeGuidID)
                                                If edge.ToNode Is sortedAlts(i) Then
                                                    Dim nonpwData As clsNonPairwiseMeasureData = edge.FromNode.EventsJudgments.GetJudgement(edge.ToNode.NodeID, edge.FromNode.NodeID, CalculationTarget.GetUserID)
                                                    If nonpwData IsNot Nothing Then
                                                        Dim value As Double = nonpwData.SingleValue

                                                        Dim AltWRTReduction As Single = 1
                                                        If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                                            For Each control As clsControl In tEnabledControls 'A1383
                                                                If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                                                    For Each assignment As clsControlAssignment In control.Assignments
                                                                        If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                                            If Not assignment.EventID.Equals(Guid.Empty) Then
                                                                                If assignment.ObjectiveID.Equals(sortedAlts(j).NodeGuidID) AndAlso assignment.EventID.Equals(edge.ToNode.NodeGuidID) Then
                                                                                    AltWRTReduction *= 1 - assignment.Value
                                                                                End If
                                                                            End If
                                                                        End If
                                                                    Next
                                                                End If
                                                            Next
                                                        End If

                                                        value *= AltWRTReduction

                                                        independentSources.Add(alt)
                                                        M *= (1 - alt.UnnormalizedPriority * value * AltWRTReduction)
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                End If
                            Next
                        End If

                        For Each MEsources As List(Of Guid) In MELists.Values
                            Dim s As Double = 0
                            For Each sourceID As Guid In MEsources
                                Dim node As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(sourceID)
                                s += node.UnnormalizedPriority * node.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            Next
                            M *= (1 - s)
                        Next

                        'For Each node As clsNode In covObjs
                        '    M *= (1 - node.UnnormalizedPriority * node.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID))
                        'Next
                        M = 1 - M

                        M *= LikelihoodWRTNodeUnnormalizedPriority

                        AltRiskData.LikelihoodValue = M
                        EventsLikelihoods.Add(alt.NodeID, M)
                        AltRiskData.WRTGlobalPriorityLikelihood = LikelihoodWRTNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID) 'Always 0
                    End If

                    TotalLikelihood += AltRiskData.LikelihoodValue
                    res.AlternativesData.Add(AltRiskData)
                Next

                'If Not res.IsSOEG1 Then res.IsSOEG1 = TotalLikelihood > 1

                ' calculating for Impact
                .ActiveHierarchy = ECHierarchyID.hidImpact
                Dim TotalImpact As Double = 0
                Dim ImpactWRTNode As ECCore.clsNode = .Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(WrtNodeGuid)
                If ImpactWRTNode Is Nothing Then ImpactWRTNode = .Hierarchy(ECHierarchyID.hidImpact).Nodes(0)

                Calculate(CalcTarget, .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)

                Dim ImpactWRTNodeUnnormalizedPriority As Double = CDbl(If(ImpactWRTNode Is .Hierarchy(.ActiveHierarchy).Nodes(0) OrElse Not ShowDueToPriorities, 1, ImpactWRTNode.UnnormalizedPriority))

                Calculate(CalcTarget, ImpactWRTNode, .ActiveHierarchy, .ActiveAltsHierarchy)
                '.Hierarchy(.ActiveHierarchy).CalculateWRTGlobal(CalcTarget, ImpactWRTNode)

                'res.IsSOIG1 = SumOfEventsMoreThanOne()
                'res.IsSOOG1 = SumOfClusterMoreThanOne(CalculationTarget)
                'res.IsIG1 = AnyEventMoreThanOne()

                'Debug.Print("Impact values: ")

                Dim computedSimulatedConsequences As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
                If Not UseSimulatedValues AndAlso LikelihoodsCalculationMode = LikelihoodsCalculationMode.Probability Then
                    ' create computed values

                    Dim computedPriorities As New Dictionary(Of Integer, Double)
                    Dim computedConsequences As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
                    Dim contributedEventsForObjectives As Dictionary(Of Integer, List(Of Integer)) = ProjectManager.RiskSimulations.GetTerminalNodesDictionary(ProjectManager.Hierarchy(ECHierarchyID.hidImpact))
                    Dim UseControls As Boolean = ProjectManager.CalculationsManager.ControlsUsageMode <> ControlsUsageMode.DoNotUse
                    Dim tEnabledControls As List(Of clsControl) = ProjectManager.Controls.EnabledControls

                    computedPriorities.Clear()
                    computedConsequences.Clear()
                    computedSimulatedConsequences.Clear()
                    For Each objective As clsNode In ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes
                        computedPriorities.Add(objective.NodeID, objective.UnnormalizedPriority)
                        computedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double))
                        computedSimulatedConsequences.Add(objective.NodeID, New Dictionary(Of Integer, Double))
                        For Each eventID As Integer In contributedEventsForObjectives(objective.NodeID)
                            Dim value As Double = objective.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(eventID)

                            Dim AltWRTReduction As Double = 1
                            If UseControls Then
                                For Each control As clsControl In tEnabledControls
                                    If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                        For Each assignment As clsControlAssignment In control.Assignments
                                            If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                If Not assignment.EventID.Equals(Guid.Empty) Then
                                                    If assignment.ObjectiveID.Equals(objective.NodeGuidID) AndAlso assignment.EventID.Equals(ProjectManager.ActiveAlternatives.GetNodeByID(eventID).NodeGuidID) Then
                                                        AltWRTReduction *= 1 + If(ProjectManager.CalculationsManager.IsOpportunity(ProjectManager.ActiveAlternatives.GetNodeByID(eventID)), assignment.Value, -assignment.Value)
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                Next
                            End If
                            value *= AltWRTReduction

                            computedConsequences(objective.NodeID).Add(eventID, value)
                            computedSimulatedConsequences(objective.NodeID).Add(eventID, 0)
                        Next
                    Next

                    For Each covObjID As Integer In contributedEventsForObjectives.Keys
                        'Debug.Print("Objective: " + ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(covObjID).NodeName)
                        ' N - number of contributed events for covObjID
                        Dim N As Integer = contributedEventsForObjectives(covObjID).Count
                        'Debug.Print("N = " + N.ToString)
                        Dim allContributedEvents(N - 1) As Integer
                        Dim t As Integer = 0
                        For Each eID As Integer In contributedEventsForObjectives(covObjID)
                            allContributedEvents(t) = eID
                            t += 1
                        Next

                        Dim count As Integer = 0
                        For Each eID As Integer In contributedEventsForObjectives(covObjID)
                            Debug.Print("Event: " + ProjectManager.ActiveAlternatives.GetNodeByID(eID).NodeName)
                            ' calculate simulated consequence (simCalcConsequence) for pair covObjID / eID
                            Dim simCalcConsequence As Double = 0

                            ' create list of remaining N-1 contributed events
                            Dim remainingEvents(N - 2) As Integer
                            t = 0
                            For Each id As Integer In allContributedEvents
                                If id <> eID Then
                                    remainingEvents(t) = id
                                    t += 1
                                End If
                            Next

                            ' calculate probability of only eID firing
                            Dim M As Double = 1
                            For Each reID As Integer In remainingEvents
                                If EventsLikelihoods.ContainsKey(reID) Then M *= (1 - EventsLikelihoods(reID)) 'todo AC
                            Next

                            ' consequence of only eID firing is simply computed consequence
                            simCalcConsequence += M * computedConsequences(covObjID)(eID)

                            ' for k = 1 to N-1 create all C(n, k) combinations
                            For k As Integer = 1 To N - 1
                                Dim c(k) As Integer ' this is where the combination is
                                For i As Integer = 0 To k
                                    c(i) = i
                                Next

                                Dim j As Integer = 1
                                While (j <> 0)
                                    count += 1
                                    ' array c contains current combination C(N-1, k), it is 1-based

                                    ' for each combination calculate propability M of this combination
                                    ' each event in combination should fire, all other events from remaining events should not fire
                                    M = 1
                                    For u As Integer = 0 To remainingEvents.Count - 1
                                        Dim inCombination As Boolean = False
                                        For v As Integer = 1 To k
                                            If c(v) - 1 = u Then
                                                inCombination = True
                                                Exit For
                                            End If
                                        Next
                                        If inCombination Then
                                            If EventsLikelihoods.ContainsKey(remainingEvents(u)) Then M *= EventsLikelihoods(remainingEvents(u)) 'todo AC
                                        Else
                                            If EventsLikelihoods.ContainsKey(remainingEvents(u)) Then M *= (1 - EventsLikelihoods(remainingEvents(u))) 'todo AC
                                        End If
                                    Next

                                    ' for each combination calculate step simulated consequence
                                    Dim mult As Double = 1
                                    Dim sum As Double = 0
                                    For v As Integer = 1 To k
                                        mult *= 1 - computedConsequences(covObjID)(remainingEvents(c(v) - 1))
                                        sum += computedConsequences(covObjID)(remainingEvents(c(v) - 1))
                                    Next
                                    mult *= 1 - computedConsequences(covObjID)(eID)
                                    sum += computedConsequences(covObjID)(eID)
                                    mult = 1 - mult
                                    Dim StepConsequence As Double = If(sum = 0, 1, computedConsequences(covObjID)(eID) * mult / sum)

                                    ' add to resulting simCalcConsequence
                                    simCalcConsequence += M * StepConsequence

                                    ' generate new combination
                                    Dim s As String = ""
                                    For i As Integer = 1 To k
                                        s += c(i).ToString + " "
                                    Next
                                    'Debug.Print(s + ": step consequence = " + StepConsequence.ToString + "; probability = " + M.ToString + "; sim step cons" + (M * StepConsequence).ToString + "; sum step consequence = " + simCalcConsequence.ToString)

                                    j = k
                                    If k <> N - 1 Then
                                        While (c(j) = (N - 1) - k + j)
                                            j = j - 1
                                        End While
                                        c(j) = c(j) + 1
                                        For i As Integer = j + 1 To k
                                            c(i) = c(i - 1) + 1
                                        Next
                                    Else
                                        j = 0
                                    End If
                                End While
                            Next
                            computedSimulatedConsequences(covObjID)(eID) = simCalcConsequence
                            'Debug.Print("count = " + count.ToString)
                        Next
                    Next

                    'Dim f As Boolean = False
                    'If f Then
                    '    For Each e As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                    '        For Each covObj As clsNode In ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes
                    '            Debug.Print(vbNewLine + e.NodeName + " vs. " + covObj.NodeName)
                    '            Debug.Print("Consequence: Calculated = " + computedConsequences(covObj.NodeID)(e.NodeID).ToString + "; Simulated = " + computedSimulatedConsequences(covObj.NodeID)(e.NodeID).ToString)
                    '        Next
                    '    Next
                    'End If

                End If

                Dim alts As List(Of clsNode) = altsList.Where(Function(a) a.Enabled).ToList
                For i As Integer = 0 To alts.Count - 1
                    AltRiskData = res.AlternativesData(i)
                    If UseSimulatedValues OrElse LikelihoodsCalculationMode = LikelihoodsCalculationMode.Regular Then
                        AltRiskData.ImpactValue = If(UseSimulatedValues, alts(i).SimulatedAltImpact, alts(i).UnnormalizedPriority) * ImpactWRTNodeUnnormalizedPriority
                        AltRiskData.WRTGlobalPriorityImpact = ImpactWRTNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alts(i).NodeID) 'Always 0
                    Else
                        ' probability calculations for consequences/impact
                        Dim contributedObjectives As List(Of clsNode) = GetContributedCoveringObjectives(ProjectManager.ActiveAlternatives.GetNodeByID(AltRiskData.AlternativeID), ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes(0), ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes)
                        Dim eImpact As Double = 0
                        For Each covObj As clsNode In contributedObjectives
                            eImpact += covObj.UnnormalizedPriority * computedSimulatedConsequences(covObj.NodeID)(AltRiskData.intID)
                        Next

                        eImpact *= ImpactWRTNodeUnnormalizedPriority

                        AltRiskData.ImpactValue = eImpact
                        AltRiskData.WRTGlobalPriorityImpact = eImpact
                    End If

                    TotalImpact += AltRiskData.ImpactValue
                    'Debug.Print(alts(i).NodeName + ": " + alts(i).UnnormalizedPriority.ToString)
                Next

                'If Not res.IsSOIG1 Then res.IsSOIG1 = TotalImpact > 1

                If TotalLikelihood = 0 Then TotalLikelihood = 1
                If TotalImpact = 0 Then TotalImpact = 1

                'Debug.Print("Risk values: ")
                For Each alt As AlternativeRiskDataDataContract In res.AlternativesData
                    alt.RiskValue = If(ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMixed OrElse ProjectManager.PipeParameters.ProjectType = Canvas.CanvasTypes.ProjectType.ptMyRiskReward, If(IsOpportunity(ProjectManager.ActiveAlternatives.GetNodeByID(alt.AlternativeID)), 1, -1), 1) * alt.LikelihoodValue * alt.ImpactValue ' D6798
                Next

                .ActiveHierarchy = CurrentHID
                ControlsUsageMode = oldUsageMode
            End With

            'Dim uncontributedEvents As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives

            'For Each e As clsNode In uncontributedEvents
            '    Dim V As Double = e.UnnormalizedPriority
            '    Dim vControls As List(Of clsControl) = ProjectManager.Controls.GetControlsForVulnerabilities(ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID, cUsageMode = ControlsUsageMode.UseOnlyActive)

            '    For Each control As clsControl In vControls
            '        Dim value As Double = ProjectManager.Controls.GetAssignmentValue(control.ID, ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID)
            '        If value <> 0 Then
            '        End If
            '    Next
            'Next


            Return res
        End Function

        Public Sub UpdateAlternativeRankValues(UserID As Integer)
            Dim nodes As IEnumerable(Of clsNode) = ProjectManager.ActiveAlternatives.Nodes.Where(Function(x) Not x.DisabledForUser(UserID)).OrderByDescending(Function (x) x.RiskValue)            
            For i As Integer = 1 to nodes.Count
                nodes(i - 1).NodeRank = i
            Next
        End Sub

        Public Function GetIsCancellationPending() As Boolean
            Return False
        End Function

        Public Function GetBowTieData2(ByVal UserID As Integer, ByVal UserEmail As String, ByVal tLoadPriorities As Boolean, Optional ControlsUsageMode As ControlsUsageMode = ControlsUsageMode.DoNotUse, Optional forObjective As Guid = Nothing, Optional RandSeed As Integer = 1, Optional isOpportunitymModel As Boolean = False) As clsBowTieDataContract2 'A1064
            PrintDebugInfo("GetBowTieData2 start: ")
            Dim res As clsBowTieDataContract2 = Nothing

            Dim PM As clsProjectManager = ProjectManager
            Dim wrtNodeGuid As Guid = Guid.Empty 'A1064
            Dim wrtNode As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(forObjective) 'A1064
            If wrtNode IsNot Nothing Then wrtNodeGuid = forObjective Else wrtNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0) 'A1064

            If UseSimulatedValues Then
                'simData = LEC.SimulateLossExceedance(UserID, UserEmail, PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(n) n.Enabled).Select(Function(x) x.NodeID).ToList, clsLEC._SIMULATE_SOURCES, False, ControlsUsageMode, dMaxValue, New Random(RandSeed), True, False, tSourcesType, Nothing, Guid.Empty, False, wrtNode.NodeGuidID, IsOpportunity)
                ProjectManager.RiskSimulations.SimulateCommon(UserID, ControlsUsageMode, wrtNode)
            End If

            res = New clsBowTieDataContract2
            res.UserID = UserID
            res.AlternativesTableData = ProjectManager.CalculationsManager.GetRiskDataWRTNode(UserID, UserEmail, forObjective, ControlsUsageMode) 'A1064
            res.BowTieData = New Dictionary(Of Guid, clsBowTieData2)

            Dim CalcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(UserID)

            Dim tmpActiveHierarchy As Integer = PM.ActiveHierarchy
            Dim tOldUsageMode As ControlsUsageMode = PM.CalculationsManager.ControlsUsageMode

            Dim tEnabledControls As List(Of clsControl) = ProjectManager.Controls.EnabledControls 'A1383 + A1392
            Dim LikelihoodWRTNodeUnnormalizedPriority As Double = 1
            PM.ActiveHierarchy = ECHierarchyID.hidLikelihood

            If tLoadPriorities Then
                'If CalcTarget.TargetType = CalculationTargetType.cttCombinedGroup Then
                '    PM.StorageManager.Reader.LoadUserData()
                'End If
                If CalcTarget.TargetType = CalculationTargetType.cttUser AndAlso CalcTarget.Target IsNot Nothing Then
                    'PM.StorageManager.Reader.LoadUserData(CType(CalcTarget.Target, ECCore.clsUser), PM.User Is CType(CalcTarget.Target, ECCore.clsUser))
                    PM.StorageManager.Reader.LoadUserData(CType(CalcTarget.Target, ECCore.clsUser))
                    If PM.User Is CType(CalcTarget.Target, ECCore.clsUser) Then
                        PM.AddEmptyMissingJudgments(PM.ActiveHierarchy, PM.ActiveAltsHierarchy, PM.User)
                    End If
                End If
                PM.CalculationsManager.ControlsUsageMode = ControlsUsageMode

                Calculate(CalcTarget, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
                LikelihoodWRTNodeUnnormalizedPriority = CDbl(If(wRTNode Is PM.Hierarchy(PM.ActiveHierarchy).Nodes(0) OrElse Not ShowDueToPriorities, 1, wRTNode.UnnormalizedPriority))

                PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy) 'A1064
            End If

            For Each alt As ECCore.clsNode In PM.ActiveAlternatives.TerminalNodes
                Dim btData As New clsBowTieData2

                btData.LikelihoodContributedNodeIDs = New List(Of Guid)
                btData.LikelihoodValues = New List(Of clsBowTiePriority)

                For Each node As ECCore.clsNode In PM.ActiveObjectives.TerminalNodes
                    'If wrtNode Is Nothing OrElse wrtNode Is node OrElse wrtNode.GetNodesBelow(UNDEFINED_USER_ID).Contains(node) Then 'A1064
                    Dim ContributedAlts As List(Of ECCore.clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)
                    If ContributedAlts.Contains(alt) Then
                        btData.LikelihoodContributedNodeIDs.Add(node.NodeGuidID)
                        If tLoadPriorities Then
                            'For Each childAlt As ECCore.clsNode In ContributedAlts
                            Dim BowTiePriority As New clsBowTiePriority

                            BowTiePriority.CovObjID = node.NodeGuidID

                            Dim CovObjReduction As Double = 1
                            Dim AltWRTReduction As Double = 1

                            If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                For Each control As clsControl In tEnabledControls 'A1383
                                    If ControlsUsageMode = ControlsUsageMode.UseAll Or (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                        For Each assignment As clsControlAssignment In control.Assignments
                                            If assignment.IsActive Then
                                                If assignment.EventID.Equals(Guid.Empty) Then
                                                    If assignment.ObjectiveID.Equals(node.NodeGuidID) Then
                                                        CovObjReduction *= 1 + If(IsOpportunity(node), assignment.Value, -assignment.Value)
                                                    End If
                                                Else
                                                    If assignment.ObjectiveID.Equals(node.NodeGuidID) And assignment.EventID.Equals(alt.NodeGuidID) Then
                                                        AltWRTReduction *= 1 + If(IsOpportunity(alt), assignment.Value, -assignment.Value)
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                Next
                            End If

                            'BowTiePriority.CovObjValueAbsolute = If(UseSimulatedValues, LEC.EventSimulatedLikelihood(simData, alt.NodeID, LEC.NumberOfSimulations), node.UnnormalizedPriority)
                            'BowTiePriority.AltWRTCovObjValueAbsolute = If(UseSimulatedValues, LEC.EventSimulatedVulnerability(simData, alt.NodeID, node.NodeID, LEC.NumberOfSimulations), node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)) * AltWRTReduction
                            'BowTiePriority.CovObjValueAbsolute = If(UseSimulatedValues, node.SimulatedPriority, node.UnnormalizedPriority)
                            'BowTiePriority.AltWRTCovObjValueAbsolute = If(UseSimulatedValues, if (node.SimulatedVulnerabilities.ContainsKey(alt.NodeID), node.SimulatedVulnerabilities(alt.NodeID), 0), node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)) * AltWRTReduction
                            BowTiePriority.CovObjValueAbsolute = If(UseSimulatedValues, node.SimulatedPriority, node.UnnormalizedPriority)
                            BowTiePriority.AltWRTCovObjValueAbsolute = If(UseSimulatedValues, If(node.SimulatedVulnerabilities.ContainsKey(alt.NodeID), node.SimulatedVulnerabilities(alt.NodeID), 0), node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID) * AltWRTReduction) * LikelihoodWRTNodeUnnormalizedPriority
                            BowTiePriority.MultipliedValueAbsolute = BowTiePriority.CovObjValueAbsolute * BowTiePriority.AltWRTCovObjValueAbsolute

                            ''BowTiePriority.CovObjValueRelative = node.WRTRelativeBPriority
                            'BowTiePriority.CovObjValueRelative = node.WRTRelativeAPriority 'A1000
                            'BowTiePriority.AltWRTCovObjValueRelative = node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) * AltWRTReduction
                            'BowTiePriority.MultipliedValueRelative = BowTiePriority.CovObjValueRelative * BowTiePriority.AltWRTCovObjValueRelative

                            btData.LikelihoodValues.Add(BowTiePriority)
                            'Next
                        End If
                    End If
                    'End If 'A1064
                Next

                res.BowTieData.Add(alt.NodeGuidID, btData)
            Next

            PM.ActiveHierarchy = ECHierarchyID.hidImpact

            wrtNodeGuid = Guid.Empty 'A1064
            wrtNode = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(forObjective) 'A1064
            If wrtNode IsNot Nothing Then wrtNodeGuid = forObjective Else wrtNode = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0) 'A1064
            Dim ImpactWRTNodeUnnormalizedPriority As Double = 1

            If tLoadPriorities Then
                Calculate(CalcTarget, PM.Hierarchy(PM.ActiveHierarchy).Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

                ImpactWRTNodeUnnormalizedPriority = CDbl(If(wrtNode Is PM.Hierarchy(PM.ActiveHierarchy).Nodes(0) OrElse Not ShowDueToPriorities, 1, wrtNode.UnnormalizedPriority))

                Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

                PM.CalculationsManager.Calculate(CalcTarget, wrtNode, PM.ActiveHierarchy) 'A1064
            End If

            For Each alt As ECCore.clsNode In PM.ActiveAlternatives.TerminalNodes
                Dim btData As clsBowTieData2 = res.BowTieData(alt.NodeGuidID)
                If btData IsNot Nothing Then
                    btData.ImpactContributedNodeIDs = New List(Of Guid)
                    btData.ImpactValues = New List(Of clsBowTiePriority)
                    For Each node As ECCore.clsNode In PM.ActiveObjectives.TerminalNodes
                        'If wrtNode Is Nothing OrElse wrtNode Is node OrElse wrtNode.GetNodesBelow(UNDEFINED_USER_ID).Contains(node) Then 'A1064
                        Dim ContributedAlts As List(Of ECCore.clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)
                        If ContributedAlts.Contains(alt) Then
                            btData.ImpactContributedNodeIDs.Add(node.NodeGuidID)
                            If tLoadPriorities Then
                                'For Each childAlt As ECCore.clsNode In ContributedAlts
                                Dim BowTiePriority As New clsBowTiePriority

                                BowTiePriority.CovObjID = node.NodeGuidID

                                Dim CovObjReduction As Double = 1
                                Dim AltWRTReduction As Double = 1

                                If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                    For Each control As clsControl In tEnabledControls 'A1383
                                        If ControlsUsageMode = ControlsUsageMode.UseAll Or (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                            For Each assignment As clsControlAssignment In control.Assignments
                                                If assignment.IsActive Then
                                                    If assignment.EventID.Equals(Guid.Empty) Then
                                                        If assignment.ObjectiveID.Equals(node.NodeGuidID) Then
                                                            CovObjReduction *= 1 + If(IsOpportunity(node), assignment.Value, -assignment.Value)
                                                        End If
                                                    Else
                                                        If assignment.ObjectiveID.Equals(node.NodeGuidID) And assignment.EventID.Equals(alt.NodeGuidID) Then
                                                            AltWRTReduction *= 1 + If(IsOpportunity(alt), assignment.Value, -assignment.Value)
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                End If

                                'BowTiePriority.CovObjValueAbsolute = If(UseSimulatedValues, LEC.EventSimulatedImpact(simData, alt.NodeID, LEC.NumberOfSimulations), node.UnnormalizedPriority) * CovObjReduction
                                'BowTiePriority.AltWRTCovObjValueAbsolute = If(UseSimulatedValues, LEC.EventSimulatedConsequence(simData, alt.NodeID, node.NodeID, LEC.NumberOfSimulations), node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)) * AltWRTReduction
                                'BowTiePriority.CovObjValueAbsolute = If(UseSimulatedValues, node.SimulatedPriority, node.UnnormalizedPriority) * CovObjReduction
                                'BowTiePriority.AltWRTCovObjValueAbsolute = If(UseSimulatedValues, If(PM.RiskSimulations.SimulatedConsequences.ContainsKey(node.NodeID) AndAlso PM.RiskSimulations.SimulatedConsequences(node.NodeID).ContainsKey(alt.NodeID), PM.RiskSimulations.SimulatedConsequences(node.NodeID)(alt.NodeID), 0), node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)) * AltWRTReduction
                                BowTiePriority.CovObjValueAbsolute = node.UnnormalizedPriority * CovObjReduction ' P is computed
                                BowTiePriority.AltWRTCovObjValueAbsolute = If(UseSimulatedValues, If(node.SimulatedConsequences.ContainsKey(alt.NodeID), node.SimulatedConsequences(alt.NodeID), 0), node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID) * AltWRTReduction) * ImpactWRTNodeUnnormalizedPriority
                                BowTiePriority.MultipliedValueAbsolute = BowTiePriority.CovObjValueAbsolute * BowTiePriority.AltWRTCovObjValueAbsolute

                                'BowTiePriority.CovObjValueRelative = node.WRTRelativeBPriority * CovObjReduction
                                'BowTiePriority.AltWRTCovObjValueRelative = node.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ECSynthesisMode.smIdeal, PM.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(alt.NodeID) * AltWRTReduction
                                'BowTiePriority.MultipliedValueRelative = BowTiePriority.CovObjValueRelative * BowTiePriority.AltWRTCovObjValueRelative

                                btData.ImpactValues.Add(BowTiePriority)
                                'Next
                            End If
                        End If
                        'End If 'A1064
                    Next
                End If
            Next

            PM.ActiveHierarchy = tmpActiveHierarchy
            If tLoadPriorities Then
                PM.CalculationsManager.ControlsUsageMode = tOldUsageMode
            End If

            PrintDebugInfo("GetBowTieData2 start: ")
            Return res
        End Function


        Public Function GetIdealGlobalPriority() As Single
            Dim sum As Single = 0

            If SynthesisMode = ECSynthesisMode.smDistributive Then
                For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                    sum += alt.WRTGlobalPriority
                Next
                Return 1 - sum
            Else
                Return mIdealAlt
            End If
        End Function

        Public Function GetIdealLocalPriority(ByVal UserID As Integer, ByVal CovObjectiveID As Integer) As Single 'C0159
            Dim node As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(CovObjectiveID)
            Return If(node Is Nothing, -1, node.GetIdealLocalPriority(UserID))
        End Function

        Public Function GetCalculationTargetByUserID(ByVal UserID As Integer) As clsCalculationTarget
            Dim calcTarget As clsCalculationTarget
            If IsCombinedUserID(UserID) Then
                calcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID))
            Else
                If UserID >= 0 Then
                    calcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, ProjectManager.GetUserByID(UserID))
                Else
                    calcTarget = New clsCalculationTarget(CalculationTargetType.cttDataInstance, ProjectManager.GetDataInstanceByUserID(UserID))
                End If
            End If
            Return calcTarget
        End Function

        Public Sub CalculateForSA(WRTNode As clsNode, CalcTarget As clsCalculationTarget)
            For Each node As clsNode In WRTNode.Hierarchy.Nodes
                node.GlobalCalculatedSA = False
                node.SAGlobalPriority = 0
            Next
            WRTNode.SAGlobalPriority = 1
            WRTNode.CalculateWRTGlobalSA(CalcTarget)

            For Each CovObj As clsNode In WRTNode.Hierarchy.TerminalNodes
                CovObj.AppliedControl = False
            Next

            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                alt.SAGlobalPriority = 0.0
            Next
            Dim tEnabledControls As List(Of clsControl) = ProjectManager.Controls.EnabledControls

            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                For Each CovObj As clsNode In WRTNode.Hierarchy.TerminalNodes
                    If (WRTNode.IsTerminalNode And (WRTNode Is CovObj)) Or
                        (Not WRTNode.IsTerminalNode And WRTNode.Hierarchy.IsChildOf(CovObj, WRTNode)) Then

                        If CovObj.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) Then
                            Dim CovObjReduction As Single = 1
                            Dim AltWRTReduction As Single = 1
                            If ProjectManager.IsRiskProject Then
                                If ControlsUsageMode = ControlsUsageMode.UseAll Or ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                    For Each control As clsControl In tEnabledControls 'A1383
                                        If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                            For Each assignment As clsControlAssignment In control.Assignments
                                                If (ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                    If assignment.EventID.Equals(Guid.Empty) Then
                                                        If assignment.ObjectiveID.Equals(CovObj.NodeGuidID) Then
                                                            CovObjReduction *= 1 + If(IsOpportunity(CovObj), assignment.Value, -assignment.Value)
                                                        End If
                                                    Else
                                                        If assignment.ObjectiveID.Equals(CovObj.NodeGuidID) AndAlso assignment.EventID.Equals(alt.NodeGuidID) Then
                                                            AltWRTReduction *= 1 + If(IsOpportunity(alt), assignment.Value, -assignment.Value)
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                End If
                            End If

                            'Dim OriginalObjWRTGlobalPriority As Single = CovObj.UnnormalizedPriority

                            If Not CovObj.AppliedControl Then
                                CovObj.SAGlobalPriority *= CovObjReduction
                                CovObj.AppliedControl = True
                            End If

                            'alt.WRTGlobalPriority = alt.WRTGlobalPriority + CovObj.WRTGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) * AltWRTReduction
                            'alt.SAGlobalPriority = alt.SAGlobalPriority + CovObj.SAGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                            'alt.SAGlobalPriority = alt.SAGlobalPriority + CovObj.SAGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                            'alt.SAGlobalPriority = alt.SAGlobalPriority + CovObj.SAGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)

                            'alt.WRTGlobalPriority = alt.WRTGlobalPriority + CovObj.WRTGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID) * AltWRTReduction

                            Dim forceSimulated As Boolean = False
                            If UseSimulatedValues Or forceSimulated Then
                                alt.SAGlobalPriority += CovObj.SAGlobalPriority * If(ProjectManager.RiskSimulations.SimulatedConsequences.ContainsKey(CovObj.NodeID) AndAlso ProjectManager.RiskSimulations.SimulatedConsequences(CovObj.NodeID).ContainsKey(alt.NodeID), ProjectManager.RiskSimulations.SimulatedConsequences(CovObj.NodeID)(alt.NodeID), 0)
                            Else
                                If UseNormalizationForSA Then
                                    alt.SAGlobalPriority = alt.SAGlobalPriority + CovObj.SAGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                                Else
                                    alt.SAGlobalPriority = alt.SAGlobalPriority + CovObj.SAGlobalPriority * CovObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                                End If
                            End If
                            alt.SAGlobalPriority *= AltWRTReduction
                        End If
                    End If
                Next
            Next

            'Dim sum As Double = 0
            'For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
            '    sum += alt.SAGlobalPriority
            'Next

            'If sum > 0 Then
            '    For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
            '        'alt.SAGlobalPriority /= sum
            '    Next
            'End If
            ''DetachAlternatives(CalcTarget)
            ''NormalizeAlternatives(WRTNode.Hierarchy)
        End Sub

        Public Function GetGradientData(WRTNodeID As Integer, CalcWRTGoal As Boolean, UserID As Integer, ObjectivesPriorities As Dictionary(Of Integer, Double), RiskSensitivityParameter as Integer, Optional AlternativesList As List(Of Integer) = Nothing) As Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Dim isRisk As Boolean = ProjectManager.IsRiskProject AndAlso (RiskSensitivityParameter = 1)
            Dim hID As ECHierarchyID = ProjectManager.ActiveObjectives.HierarchyID

            Dim res As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

            Dim MH As clsHierarchy = ProjectManager.Hierarchy(HierarchyID)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)

            Dim SAWRTNode As clsNode = MH.GetNodeByID(WRTNodeID)
            If SAWRTNode Is Nothing Then Return Nothing
            Dim CalcWRTNode As clsNode = If(CalcWRTGoal, MH.Nodes(0), SAWRTNode)

            SANode = SAWRTNode

            Dim CT As clsCalculationTarget = GetCalculationTargetByUserID(UserID)
            Calculate(CT, CalcWRTNode)

            Dim Children As List(Of clsNode) = SAWRTNode.GetNodesBelow(UserID)

            Dim Alternatives As New List(Of clsNode)
            If AlternativesList Is Nothing Then
                Alternatives = AH.TerminalNodes
            Else
                For Each AltID As Integer In AlternativesList
                    Dim alt As clsNode = AH.GetNodeByID(AltID)
                    If alt IsNot Nothing Then Alternatives.Add(alt)
                Next
            End If

            ' calculate in 0
            For Each child As clsNode In Children
                If ProjectManager.IsRiskProject AndAlso child.RiskNodeType = RiskNodeType.ntCategory Then
                    child.SALocalPriority = 1
                End If

                If Not ProjectManager.IsRiskProject OrElse ProjectManager.IsRiskProject AndAlso child.RiskNodeType <> RiskNodeType.ntCategory Then
                    Dim sum As Double = 0
                    For Each node As clsNode In Children
                        If Not ProjectManager.IsRiskProject OrElse ProjectManager.IsRiskProject AndAlso node.RiskNodeType <> RiskNodeType.ntCategory Then
                            If node IsNot child AndAlso ObjectivesPriorities.ContainsKey(node.NodeID) Then sum += ObjectivesPriorities(node.NodeID) ' D3471
                        End If
                    Next

                    Dim k As Double = 1
                    If sum <> 0 Then
                        k = 1 / sum
                    End If

                    For Each node As clsNode In Children
                        If Not ProjectManager.IsRiskProject OrElse ProjectManager.IsRiskProject AndAlso node.RiskNodeType <> RiskNodeType.ntCategory Then
                            If node IsNot child AndAlso ObjectivesPriorities.ContainsKey(node.NodeID) Then  ' D3471
                                node.SALocalPriority = ObjectivesPriorities(node.NodeID) * k
                            Else
                                node.SALocalPriority = 0
                            End If
                        End If
                    Next

                    CalculateForSA(CalcWRTNode, CT)

                    Dim AltData As New Dictionary(Of Integer, Double)
                    For Each alt As clsNode In Alternatives
                        If isRisk Then
                            AltData.Add(alt.NodeID, alt.SAGlobalPriority * PrioritiesFromOtherHierarchy(alt.NodeID))
                        Else
                            AltData.Add(alt.NodeID, alt.SAGlobalPriority)
                        End If
                        'AltData.Add(alt.NodeID, alt.UnnormalizedPriority)
                    Next
                    res.Add(child.NodeID, AltData)
                End If
            Next

            Return res
        End Function

        Public Sub InitializeSAGradient(WRTNodeID As Integer, CalcWRTGoal As Boolean, UserID As Integer, ByRef ObjectivesPriorities As Dictionary(Of Integer, Double), ByRef AlternativesValuesCurrent As Dictionary(Of Integer, Double), ByRef AlternativesValuesInOne As Dictionary(Of Integer, Dictionary(Of Integer, Double)), RiskSensitivityParameter As Integer, Optional AlternativesList As List(Of Integer) = Nothing)
            Dim isRisk As Boolean = ProjectManager.IsRiskProject AndAlso (RiskSensitivityParameter = 1)
            Dim hID As ECHierarchyID = ProjectManager.ActiveObjectives.HierarchyID

            Dim MH As clsHierarchy = ProjectManager.Hierarchy(HierarchyID)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(AltsHierarchyID)

            Dim SAWRTNode As clsNode = MH.GetNodeByID(WRTNodeID)
            If SAWRTNode Is Nothing Then Exit Sub
            Dim CalcWRTNode As clsNode = If(CalcWRTGoal, MH.Nodes(0), SAWRTNode)

            SANode = SAWRTNode

            ObjectivesPriorities = New Dictionary(Of Integer, Double)
            AlternativesValuesCurrent = New Dictionary(Of Integer, Double)
            AlternativesValuesInOne = New Dictionary(Of Integer, Dictionary(Of Integer, Double))

            Dim Alternatives As New List(Of clsNode)
            If AlternativesList Is Nothing Then
                Alternatives = AH.TerminalNodes
            Else
                For Each AltID As Integer In AlternativesList
                    Dim alt As clsNode = AH.GetNodeByID(AltID)
                    If alt IsNot Nothing Then Alternatives.Add(alt)
                Next
            End If

            Dim CT As clsCalculationTarget = GetCalculationTargetByUserID(UserID)

            Dim forceSimulated As Boolean = False

            If ProjectManager.IsRiskProject AndAlso (UseSimulatedValues OrElse forceSimulated) Then
                ProjectManager.RiskSimulations.Simulate()
            End If

            PrioritiesFromOtherHierarchy.Clear()
            If isRisk Then
                'Dim SimData As List(Of clsSimulationData) = Nothing
                'Dim UseSimulatedValues As Boolean = ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInput OrElse ProjectManager.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.SimulatedInputAndOutput
                'If UseSimulatedValues Then
                '    Dim dMaxLossValue As Double
                '    Dim LEC As New clsLEC(ProjectManager, AddressOf GetIsCancellationPending)
                '    SimData = LEC.SimulateLossExceedance(COMBINED_USER_ID, "", ProjectManager.ActiveAlternatives.TerminalNodes.Where(Function(a) a.Enabled).Select(Function(x) x.NodeID).ToList, clsLEC._SIMULATE_SOURCES, False, ControlsUsageMode.DoNotUse, dMaxLossValue, New Random(LEC.RandSeed), True, False, 0, Nothing, Guid.Empty, False, Guid.Empty, IsOpportunity)
                'End If

                Dim otherH As clsHierarchy = ProjectManager.GetAnyHierarchyByID(CInt(If(hID = ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact, ECHierarchyID.hidLikelihood)))
                Dim oldHID As Integer = ProjectManager.ActiveHierarchy
                ProjectManager.ActiveHierarchy = otherH.HierarchyID
                Calculate(CT, otherH.Nodes(0), otherH.HierarchyID)

                For Each alt As clsNode In Alternatives
                    PrioritiesFromOtherHierarchy.Add(alt.NodeID, If((UseSimulatedValues Or forceSimulated) AndAlso otherH.HierarchyID = ECHierarchyID.hidLikelihood, alt.SimulatedAltLikelihood, If(UseNormalizationForSA, alt.WRTGlobalPriority, alt.UnnormalizedPriority)))
                Next
                ProjectManager.ActiveHierarchy = oldHID
            End If

            Calculate(CT, CalcWRTNode)

            ' initial objectives priorities
            For Each node As clsNode In SAWRTNode.GetNodesBelow(CT.GetUserID)
                node.SALocalPriority = node.LocalPriorityUnnormalized(CT, SAWRTNode)
                If Not ProjectManager.IsRiskProject OrElse ProjectManager.IsRiskProject AndAlso node.RiskNodeType <> RiskNodeType.ntCategory Then
                    ObjectivesPriorities.Add(node.NodeID, node.SALocalPriority)
                End If
            Next

            CalculateForSA(CalcWRTNode, CT)

            ' initial alternatives priorities
            For Each alt As clsNode In Alternatives
                If isRisk Then
                    AlternativesValuesCurrent.Add(alt.NodeID, alt.SAGlobalPriority * PrioritiesFromOtherHierarchy(alt.NodeID))
                Else
                    AlternativesValuesCurrent.Add(alt.NodeID, alt.SAGlobalPriority)
                End If
            Next

            'Debug.Print(vbNewLine + "      SA Calculated in 1: " + vbNewLine)

            ' calculate in 1
            Dim Children As List(Of clsNode) = SAWRTNode.GetNodesBelow(UserID)

            For Each child As clsNode In Children
                If Not ProjectManager.IsRiskProject OrElse ProjectManager.IsRiskProject AndAlso child.RiskNodeType <> RiskNodeType.ntCategory Then
                    Dim AltData As New Dictionary(Of Integer, Double)
                    For Each node As clsNode In Children
                        If Not ProjectManager.IsRiskProject OrElse ProjectManager.IsRiskProject AndAlso node.RiskNodeType <> RiskNodeType.ntCategory Then
                            node.SALocalPriority = If(child Is node, 1, 0)
                        End If
                    Next

                    CalculateForSA(CalcWRTNode, CT)

                    For Each alt As clsNode In Alternatives
                        If isRisk Then
                            AltData.Add(alt.NodeID, alt.SAGlobalPriority * PrioritiesFromOtherHierarchy(alt.NodeID))
                        Else
                            AltData.Add(alt.NodeID, alt.SAGlobalPriority)
                        End If
                    Next

                    AlternativesValuesInOne.Add(child.NodeID, AltData)
                End If
            Next
        End Sub

        Public Sub GetSA2DData(WRTNodeID As Integer, UserID As Integer, ByRef ObjectivesPriorities As Dictionary(Of Integer, NodePriority), ByRef AlternativesPriorities As Dictionary(Of Integer, List(Of NodePriority)))
            Dim MH As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)

            Dim wrtNode As clsNode = MH.GetNodeByID(WRTNodeID)
            If wrtNode Is Nothing OrElse wrtNode.IsTerminalNode Then Exit Sub

            Dim calcTarget As clsCalculationTarget = GetCalculationTargetByUserID(UserID)
            Calculate(calcTarget, MH.Nodes(0))

            Dim Alternatives As List(Of clsNode) = AH.TerminalNodes

            ObjectivesPriorities = New Dictionary(Of Integer, NodePriority)
            AlternativesPriorities = New Dictionary(Of Integer, List(Of NodePriority))

            Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, ProjectManager)

            For Each node As clsNode In wrtNode.GetNodesBelow(UNDEFINED_USER_ID)
                Dim nPriority As New NodePriority
                nPriority.NodeID = node.NodeID
                nPriority.ParentID = node.ParentNode.NodeID
                nPriority.LocalPriority = node.LocalPriority(calcTarget, node.ParentNode)
                nPriority.GlobalPriority = node.WRTGlobalPriority
                nPriority.NormalizedGlobalPriority = node.WRTGlobalPriority
                nPriority.CanShowResults = JA.CanShowIndividualResults(calcTarget.GetUserID, node)

                ObjectivesPriorities.Add(node.NodeID, nPriority)

                Dim AltPriorities As New List(Of NodePriority)

                Calculate(calcTarget, node)
                'wrtNode = node
                'MH.CalculateWRTGlobal(calcTarget, node)
                'DetachAlternatives(calcTarget)

                For Each alt As clsNode In Alternatives
                    nPriority = New NodePriority
                    nPriority.NodeID = alt.NodeID
                    nPriority.ParentID = -1
                    nPriority.LocalPriority = -1
                    nPriority.GlobalPriority = alt.UnnormalizedPriority
                    nPriority.NormalizedGlobalPriority = alt.WRTGlobalPriority
                    nPriority.CanShowResults = True
                    AltPriorities.Add(nPriority)
                Next
                AlternativesPriorities.Add(node.NodeID, AltPriorities)
            Next
        End Sub

        Private Sub AddObjectives(WRTNode As clsNode, ByRef UserObjPriorities As List(Of Tuple(Of Integer, Integer, NodePriority)), CalcTarget As clsCalculationTarget, JA As clsJudgmentsAnalyzer)
            For Each node As clsNode In WRTNode.GetNodesBelow(UNDEFINED_USER_ID)
                Dim nPriority As New NodePriority
                nPriority.NodeID = node.NodeID
                nPriority.ParentID = WRTNode.NodeID
                nPriority.LocalPriority = If(ProjectManager.IsRiskProject, node.LocalPriorityUnnormalized(CalcTarget, WRTNode), node.LocalPriority(CalcTarget, WRTNode))
                nPriority.GlobalPriority = If(ProjectManager.IsRiskProject, node.UnnormalizedPriority, node.WRTGlobalPriority)
                nPriority.NormalizedGlobalPriority = node.WRTGlobalPriority
                Dim AlternativesEvaluated As Boolean = False
                'nPriority.CanShowResults = If(IsCombinedUserID(CalcTarget.GetUserID), JA.CanShowGroupResults(node, CType(CalcTarget.Target, clsCombinedGroup)), JA.CanShowIndividualResults(CalcTarget.GetUserID, node))
                nPriority.CanShowResults = Not (Not ProjectManager.UsersRoles.IsAllowedObjective(WRTNode.NodeGuidID, CalcTarget.GetUserID) AndAlso Not UseCombinedForRestrictedNodes)
                UserObjPriorities.Add(New Tuple(Of Integer, Integer, NodePriority)(node.NodeID, WRTNode.NodeID, nPriority))
                If Not node.IsTerminalNode Then
                    AddObjectives(node, UserObjPriorities, CalcTarget, JA)
                End If
            Next
        End Sub

        Public Sub GetAlternativesGrid(WRTNodeID As Integer, UserIDs As List(Of Integer), ByRef ObjectivesPriorities As Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority))), ByRef AlternativesPriorities As Dictionary(Of Integer, List(Of NodePriority)), Optional CalculateObjectives As Boolean = True, Optional CalculateAlternatives As Boolean = True)
            Dim MH As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)

            Dim wrtNode As clsNode = MH.GetNodeByID(WRTNodeID)
            If wrtNode Is Nothing Then Exit Sub

            Dim Alternatives As List(Of clsNode) = AH.TerminalNodes

            ObjectivesPriorities = New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
            AlternativesPriorities = New Dictionary(Of Integer, List(Of NodePriority))

            For Each userID As Integer In UserIDs
                Dim UserObjPriorities As New List(Of Tuple(Of Integer, Integer, NodePriority))
                Dim UserAltPriorities As New List(Of NodePriority)

                Dim calcTarget As clsCalculationTarget = GetCalculationTargetByUserID(userID)

                If Not IsCombinedUserID(userID) Then
                    ProjectManager.StorageManager.Reader.LoadUserData(ProjectManager.GetUserByID(userID))
                End If
                Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, ProjectManager) 'A1045

                If CalculateObjectives Then
                    Calculate(calcTarget, MH.Nodes(0))
                    AddObjectives(wrtNode, UserObjPriorities, calcTarget, JA)
                    ObjectivesPriorities.Add(userID, UserObjPriorities)
                End If

                If CalculateAlternatives Then
                    Dim WRTNodeUnnormalizedPriority As Double = wrtNode.UnnormalizedPriority

                    If wrtNode IsNot MH.Nodes(0) Then
                        Calculate(calcTarget, wrtNode, MH.HierarchyID, AH.HierarchyID)
                    End If

                    Dim CanShowResults As Boolean = If(IsCombinedUserID(calcTarget.GetUserID), JA.CanShowGroupResults(wrtNode, CType(calcTarget.Target, clsCombinedGroup)), JA.CanShowIndividualResults(calcTarget.GetUserID, wrtNode))
                    For Each alt As clsNode In AH.Nodes
                        Dim nPriority As New NodePriority
                        nPriority.NodeID = alt.NodeID
                        nPriority.ParentID = -1
                        nPriority.LocalPriority = -1
                        nPriority.GlobalPriority = alt.UnnormalizedPriority
                        nPriority.NormalizedGlobalPriority = alt.WRTGlobalPriority

                        If ProjectManager.IsRiskProject AndAlso ShowDueToPriorities Then 'A1590
                            nPriority.GlobalPriority = alt.UnnormalizedPriority * WRTNodeUnnormalizedPriority
                        End If

                        nPriority.CanShowResults = CanShowResults 'A1045

                        UserAltPriorities.Add(nPriority) 'A1009
                    Next
                    AlternativesPriorities.Add(userID, UserAltPriorities)
                End If
            Next
        End Sub

        Public Sub GetObjectivesGrid(WRTNodeID As Integer, UserIDs As List(Of Integer), ByRef ObjectivesPriorities As Dictionary(Of Integer, List(Of NodePriority)))
            Dim MH As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)

            Dim wrtNode As clsNode = MH.GetNodeByID(WRTNodeID)
            If wrtNode Is Nothing Then Exit Sub

            ObjectivesPriorities = New Dictionary(Of Integer, List(Of NodePriority))

            For Each userID As Integer In UserIDs
                Dim UserObjPriorities As New List(Of NodePriority)

                Dim calcTarget As clsCalculationTarget = GetCalculationTargetByUserID(userID)

                Calculate(calcTarget, wrtNode)
                For Each node As clsNode In MH.Nodes
                    For Each parent As clsNode In node.ParentNodes
                        Dim nPriority As New NodePriority
                        nPriority.NodeID = node.NodeID
                        nPriority.ParentID = parent.NodeID
                        nPriority.LocalPriority = node.LocalPriority(calcTarget, parent)
                        nPriority.GlobalPriority = node.WRTGlobalPriority
                    Next
                Next

                ObjectivesPriorities.Add(userID, UserObjPriorities)
            Next
        End Sub

        Public Function GetDSAComponents(WRTNodeID As Integer) As Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Dim res As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim wrtNode As clsNode = H.GetNodeByID(WRTNodeID)
            Dim nodesBelow As List(Of clsNode) = wrtNode.GetNodesBelow(UNDEFINED_USER_ID)
            Dim alternatives As List(Of clsNode) = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
            Calculate(CalculationTarget, H.Nodes(0))
            Dim parentGlobalPriority As Double = If(wrtNode.ParentNode Is Nothing, 1, wrtNode.ParentNode.WRTGlobalPriority)
            If parentGlobalPriority = 0 Then Return res

            For Each alt As clsNode In alternatives
                Dim altData As New Dictionary(Of Integer, Double)
                For Each node As clsNode In nodesBelow
                    Dim nodeCoeff As Double = parentGlobalPriority
                    Dim respectiveNodes As List(Of clsNode) = H.GetRespectiveTerminalNodes(node)
                    Dim sum As Double = 0
                    For Each covObj As clsNode In respectiveNodes
                        Dim rCoeff As Double = 1
                        If covObj.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) Then
                            rCoeff *= covObj.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            Dim pNode As clsNode = covObj
                            If pNode IsNot node Then
                                While (pNode IsNot node)
                                    rCoeff *= pNode.LocalPriority(CalculationTarget)
                                    pNode = pNode.ParentNode
                                End While
                            End If
                        End If
                        sum += rCoeff
                    Next
                    nodeCoeff *= sum
                    altData.Add(node.NodeID, nodeCoeff)
                Next
                res.Add(alt.NodeID, altData)
            Next
            Return res
        End Function


        Public Function GetDSAComponents(WRTNodeID As Integer, Optional CalculateFullComponentValue As Boolean = False, Optional UserID As Integer = COMBINED_USER_ID) As Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Dim res As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
            Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim wrtNode As clsNode = H.GetNodeByID(WRTNodeID)
            Dim nodesBelow As List(Of clsNode) = wrtNode.GetNodesBelow(If(IsCombinedUserID(UserID), UNDEFINED_USER_ID, UserID))
            Dim alternatives As List(Of clsNode) = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
            Dim CalcTarget As clsCalculationTarget = GetCalculationTargetByUserID(UserID)
            Calculate(CalcTarget, H.Nodes(0))
            Dim parentGlobalPriority As Double = If(wrtNode.ParentNode Is Nothing, 1, wrtNode.ParentNode.WRTGlobalPriority)
            If parentGlobalPriority = 0 Then Return res

            'Debug.Print("Components: " + vbNewLine)
            For Each alt As clsNode In alternatives
                Dim altData As New Dictionary(Of Integer, Double)
                For Each node As clsNode In nodesBelow
                    Dim nodeCoeff As Double = parentGlobalPriority
                    Dim respectiveNodes As List(Of clsNode) = H.GetRespectiveTerminalNodes(node)
                    Dim sum As Double = 0
                    For Each covObj As clsNode In respectiveNodes
                        If covObj.GetNodesBelow(If(UseCombinedForRestrictedNodes, UNDEFINED_USER_ID, CalcTarget.GetUserID)).Contains(alt) Then
                            Dim rCoeff As Double = 1
                            If UseNormalizationForSA Then
                                rCoeff *= covObj.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                            Else
                                rCoeff *= covObj.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)
                            End If
                            Dim pNode As clsNode = covObj
                            If pNode IsNot node Then
                                While (pNode IsNot node AndAlso pNode IsNot Nothing) 'A1740
                                    rCoeff *= If(UseNormalizationForSA, pNode.LocalPriority(CalcTarget), pNode.LocalPriorityUnnormalized(CalcTarget))
                                    pNode = pNode.ParentNode
                                End While
                            End If
                            If CalculateFullComponentValue AndAlso pNode IsNot Nothing Then 'A1740
                                rCoeff *= If(UseNormalizationForSA, pNode.LocalPriority(CalcTarget), pNode.LocalPriorityUnnormalized(CalcTarget))
                            End If
                            sum += rCoeff
                        End If
                    Next
                    nodeCoeff *= sum
                    altData.Add(node.NodeID, nodeCoeff)
                Next
                res.Add(alt.NodeID, altData)

                'Debug.Print(vbNewLine + alt.NodeName + ": ")
                'For Each kvp As KeyValuePair(Of Integer, Double) In altData
                '    'Debug.Print(kvp.Key.ToString + " = " + kvp.Value.ToString)
                '    Dim Node As clsNode = H.GetNodeByID(kvp.Key)
                '    Dim NodeName As String = If(Node IsNot Nothing, Node.NodeName, "")
                '    Debug.Print(kvp.Key.ToString + " " + NodeName + " = " + kvp.Value.ToString)
                'Next
                'Debug.Print("SUM = " + altData.Sum(Function(a) a.Value).ToString)
                'Debug.Print("ALT WRT PRIORITY = " + alt.WRTGlobalPriority.ToString)
                'Debug.Print("ALT UNNORM PRIORITY = " + alt.UnnormalizedPriority.ToString)
            Next
            Return res
        End Function

        Public Function GetASAData(UserID As Integer) As ASAData
            Dim result As New ASAData
            result.UserID = UserID

            If Not IsCombinedUserID(UserID) Then
                ProjectManager.StorageManager.Reader.LoadUserData(ProjectManager.GetUserByID(UserID))
            End If

            Dim CalcTarget As clsCalculationTarget = GetCalculationTargetByUserID(UserID)
            Calculate(CalcTarget, PM.ActiveObjectives.Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

            Dim maxAltValueUnnormalized As Double = 0
            Dim maxAltValueNormalized As Double = 0

            For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                result.Alternatives.Add(alt.NodeID, alt.NodeName)
                result.AlternativesPriorities.Add(alt.NodeID, New Tuple(Of Double, Double)(alt.WRTGlobalPriority, alt.UnnormalizedPriority))
            Next
            For Each node As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                result.Objectives.Add(node.NodeID, node.NodeName)
                result.ObjectivesPriorities.Add(node.NodeID, New Tuple(Of Double, Double)(node.WRTGlobalPriority, node.UnnormalizedPriority))
            Next

            For Each node As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                'Calculate(CalcTarget, node, ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, ,, False)
                Dim AltsPrtyByObj As New Dictionary(Of Integer, Tuple(Of Double, Double))
                For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                    Dim normValue As Double = node.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetWeightValueByNodeID(alt.NodeID)
                    Dim unnormValue As Double = node.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, SynthesisMode, IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(alt.NodeID)

                    AltsPrtyByObj.Add(alt.NodeID, New Tuple(Of Double, Double)(normValue, unnormValue))

                    If Not result.ObjectivesPrioritiesByAlternative.ContainsKey(alt.NodeID) Then
                        result.ObjectivesPrioritiesByAlternative.Add(alt.NodeID, New Dictionary(Of Integer, Tuple(Of Double, Double)))
                    End If

                    result.ObjectivesPrioritiesByAlternative(alt.NodeID).Add(node.NodeID, New Tuple(Of Double, Double)(normValue, unnormValue))

                    If normValue > maxAltValueNormalized Then maxAltValueNormalized = normValue
                    If unnormValue > maxAltValueUnnormalized Then maxAltValueUnnormalized = unnormValue
                Next
                result.AlternativesPrioritiesByObjective.Add(node.NodeID, AltsPrtyByObj)
            Next

            result.MaxAlternativePriorityUnnormalized = maxAltValueUnnormalized
            result.MaxAlternativePriorityNormalized = maxAltValueNormalized
            Return result
        End Function


        Public Sub TestGetASAData()
            Dim asa As ASAData = GetASAData(COMBINED_USER_ID)
            Dim b As Boolean = True
            PrintDebugInfo("Objectives priorities: ", b)
            For Each kvp As KeyValuePair(Of Integer, Tuple(Of Double, Double)) In asa.ObjectivesPriorities
                PrintDebugInfo(asa.Objectives(kvp.Key) + ": " + kvp.Value.Item1.ToString + "; " + kvp.Value.Item2.ToString, b)
            Next

            PrintDebugInfo("Alternatives priorities: ", b)
            For Each kvp As KeyValuePair(Of Integer, Tuple(Of Double, Double)) In asa.AlternativesPriorities
                PrintDebugInfo(asa.Alternatives(kvp.Key) + ": " + kvp.Value.Item1.ToString + "; " + kvp.Value.Item2.ToString, b)
            Next
        End Sub

#Region "Cost in Dollars functions" 'A1255 ===
        Private ReadOnly Property PM As clsProjectManager
            Get
                Return ProjectManager
            End Get
        End Property

        Public ReadOnly Property DollarValue As Double
            Get
                Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
            End Get
        End Property

        Public ReadOnly Property DollarValueTarget As String
            Get
                Dim retVal As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_DOLLAR_VALUE_TARGET_ID, UNDEFINED_USER_ID))
                If String.IsNullOrEmpty(retVal) Then Return ""

                Dim targetGuid As Guid
                If Guid.TryParse(retVal, targetGuid) Then
                    If Not targetGuid.Equals(Guid.Empty) Then
                        If PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(targetGuid) IsNot Nothing OrElse PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(targetGuid) IsNot Nothing Then
                            Return retVal
                        End If
                    End If
                End If
                Return UNDEFINED_STRING_VALUE
            End Get
        End Property

        Private Sub ResetDollarValues()
            If Not ProjectManager.IsRiskProject Then Exit Sub
            Dim H As clsHierarchy = ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
            If H Is Nothing Then Exit Sub
            Dim AH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
            If AH Is Nothing Then Exit Sub

            For Each node As clsNode In H.Nodes
                node.DollarValue = UNDEFINED_INTEGER_VALUE
            Next
            For Each node As clsNode In AH.Nodes
                node.DollarValue = UNDEFINED_INTEGER_VALUE
            Next
        End Sub

        Private Function HasDollarValueSet(H As clsHierarchy) As Boolean
            '-A1314 Return GetDollarValueNode(H) IsNot Nothing
            Return GetDollarValueNode(H) IsNot Nothing AndAlso DollarValue <> UNDEFINED_INTEGER_VALUE 'A1314
        End Function

        Private Function GetDollarValueNode(H As clsHierarchy) As clsNode
            Dim targetIDString As String = DollarValueTarget
            If targetIDString = "" AndAlso H.HierarchyType = ECHierarchyType.htModel Then
                Return H.Nodes(0)
            Else
                Dim targetID As Guid
                If Not Guid.TryParse(targetIDString, targetID) Then Return Nothing
                Return H.GetNodeByID(targetID)
            End If
        End Function

        Private Function GetDollarValueForImpactGoal() As Double
            Dim H As clsHierarchy = ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
            Dim dollarNode As clsNode = GetDollarValueNode(H)
            If dollarNode Is Nothing Then Return UNDEFINED_INTEGER_VALUE
            Dim goalNode As clsNode = H.Nodes(0)
            If DollarValue <> 0 AndAlso dollarNode.WRTGlobalPriority <> 0 Then
                Return DollarValue / dollarNode.WRTGlobalPriority
            Else
                Return 0
            End If
        End Function

        Private Sub CalculateImpactDollarValues()
            'ResetDollarValues()
            Dim goalDollarValue As Double = GetDollarValueForImpactGoal()
            If goalDollarValue = UNDEFINED_INTEGER_VALUE Then Exit Sub
            For Each node As clsNode In ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes
                node.DollarValue = goalDollarValue * node.WRTGlobalPriority
            Next
        End Sub

        Private Sub CalculateEventsDollarValues()
            'ResetDollarValues()
            Dim eventNode As clsNode = GetDollarValueNode(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy))
            If eventNode Is Nothing Then Exit Sub
            Dim value As Double = DollarValue
            If value = UNDEFINED_INTEGER_VALUE Then Exit Sub
            For Each node As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                node.DollarValue = value * node.WRTGlobalPriority / eventNode.WRTGlobalPriority
            Next
        End Sub

#End Region 'A1255 ==

#Region "Feedback functions"
        Private Function GetAltPriorityWRTCovObj(Alt As clsNode, CovObj As clsNode, UserID As Integer) As Single
            Return CovObj.Judgments.Weights.GetUserWeights(UserID, ECSynthesisMode.smDistributive, False).GetNormalizedWeightValueByNodeID(Alt.NodeID)
        End Function

        Private Function GetCovObjPriorityWRTAlt(Alt As clsNode, CovObj As clsNode, UserID As Integer) As Single
            If Alt Is Nothing OrElse CovObj Is Nothing OrElse Alt.FeedbackJudgments Is Nothing Then Return 0
            Dim pwData As clsNonPairwiseMeasureData = CType(Alt.FeedbackJudgments, clsNonPairwiseJudgments).GetJudgement(CovObj.NodeID, Alt.NodeID, UserID)
            If pwData Is Nothing Then Return 0
            Return pwData.SingleValue
        End Function

        Private Function GetNextIteration(Alts As List(Of clsNode), CovObjs As List(Of clsNode), UserID As Integer) As Double()
            Dim res() As Double
            ReDim res(Alts.Count)

            Dim resObjs() As Double
            ReDim resObjs(CovObjs.Count)

            For Each node As clsNode In CovObjs
                node.WRTGlobalPriorityFeedback = 0
                For Each alt As clsNode In Alts
                    node.WRTGlobalPriorityFeedback += alt.WRTGlobalPriorityFeedback * GetCovObjPriorityWRTAlt(alt, node, UserID)
                Next
            Next

            For Each alt As clsNode In Alts
                alt.WRTGlobalPriorityFeedback = 0
                For Each node As clsNode In CovObjs
                    alt.WRTGlobalPriorityFeedback += node.WRTGlobalPriorityFeedback * GetAltPriorityWRTCovObj(alt, node, UserID)
                Next
            Next

            For j As Integer = 0 To Alts.Count - 1
                res(j) = Alts(j).WRTGlobalPriorityFeedback
            Next

            Return res
        End Function

        Private Sub CalculateFeedback(MH As clsHierarchy, AH As clsHierarchy, UserID As Integer)
            Exit Sub ' D3854 Approved by AC for avoid crashes and hangup

            Dim Priorities1() As Double
            Dim Priorities2() As Double

            Dim Alts As List(Of clsNode) = AH.TerminalNodes
            Dim CovObjs As List(Of clsNode) = MH.TerminalNodes

            ReDim Priorities1(Alts.Count)
            ReDim Priorities2(Alts.Count)

            For j As Integer = 0 To Alts.Count - 1
                Alts(j).WRTGlobalPriorityFeedback = 1 / Alts.Count
                Priorities1(j) = Alts(j).WRTGlobalPriorityFeedback
            Next

            For j As Integer = 0 To CovObjs.Count - 1
                CovObjs(j).WRTGlobalPriorityFeedback = CovObjs(j).WRTGlobalPriority
            Next

            Dim done As Boolean = False
            Dim b As Boolean
            Dim i As Integer

            Dim mEps As Single = 0.001

            While Not done
                Priorities2 = GetNextIteration(Alts, CovObjs, UserID)

                b = True
                i = 0
                While b And (i < Alts.Count)
                    b = Math.Abs(Priorities1(i) - Priorities2(i)) <= mEps
                    i += 1
                End While

                If Not b Then
                    CopyVector(Priorities2, Priorities1, Alts.Count)
                Else
                    done = True
                End If
            End While

            For j As Integer = 0 To Alts.Count - 1
                Alts(j).WRTGlobalPriority = Priorities2(j)
            Next
        End Sub
#End Region
        Public Sub New(ByVal ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
            PrioritiesCacheManager = New PrioritiesCacheManager(ProjectManager)
        End Sub
    End Class
End Namespace
