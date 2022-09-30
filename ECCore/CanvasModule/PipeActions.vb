Imports ECCore
Imports System.Xml 'C0464
Imports System.IO 'C0464
Imports system.Linq

Namespace Canvas
    Public Enum ActionType
        atNone = 0
        atPairwise = 1
        atNonPWOneAtATime = 2
        atNonPWAllChildren = 3
        atNonPWAllCovObjs = 4
        atShowLocalResults = 5
        atShowGlobalResults = 6
        atInformationPage = 7
        atSurvey = 8
        atSensitivityAnalysis = 9
        atSpyronSurvey = 10 'C0139
        atAllPairwise = 11 'C0959
        atPairwiseOutcomes = 12
        atAllPairwiseOutcomes = 13
        atAllEventsWithNoSource = 14
        atEmbeddedContent = 20
    End Enum

    Public Enum EmbeddedContentType
        None = -1
        RiskResults = 1
        HeatMap = 2     ' D6664
        AlternativesRank = 3   ' D6671
    End Enum

    Public Enum SAType 'C0078
        satNone = -1    ' D0182
        satDynamic = 0
        satPerformance = 1
        satGradient = 2
    End Enum

    ' D6928 ===
    Public Enum SASortMode
        AltsByIndex = 0
        AltsByPrty = 1
        AltsByName = 2
        ObjByIndex = 64
        ObjByPrty = 65
        ObjByname = 66
    End Enum
    ' D6928 ==

    Public Enum AlternativeNormalizationOptions
        anoPriority = 0
        anoPercentOfMax = 1
        anoMultipleOfMin = 2
        anoUnnormalized = 3
    End Enum

    <Serializable()> Public Class clsResultsItem
        Public Property Name() As String
        Public Property Value() As Single
        Public Property UnnormalizedValue() As Single
        Public Property ObjectID() As Integer

        Public Sub New(ByVal Name As String, ByVal Value As Single, ByVal UnnormalizedValue As Single, ByVal ObjectID As Integer)
            Me.Name = Name
            Me.Value = Value
            Me.UnnormalizedValue = UnnormalizedValue
            Me.ObjectID = ObjectID
        End Sub
    End Class

#Region "PIPE ACTIONS CLASSES"

    <Serializable()> Public Class clsAction
        Public Property ActionType() As ActionType
        Public Property ActionData() As Object
        Public Property ParentNode() As clsNode
        Public Property PWONode() As clsNode
        Public Property Available() As Boolean = True
        Public Property StepGuid() As Guid = Guid.NewGuid
        Public Property IsFeedback As Boolean
        Public Property EmbeddedContentType As EmbeddedContentType = EmbeddedContentType.None

        Public ReadOnly Property isEvaluation() As Boolean
            Get
                Select Case ActionType
                    Case ActionType.atPairwise, ActionType.atAllPairwise, ActionType.atNonPWOneAtATime, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atPairwiseOutcomes, ActionType.atAllPairwiseOutcomes, ActionType.atAllEventsWithNoSource
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Public ReadOnly Property isMultiEvaluation As Boolean
            Get
                Select Case ActionType
                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atAllEventsWithNoSource    ' D3250
                        Return True
                    Case Else
                        Return False
                End Select
                Return False
            End Get
        End Property
    End Class

#Region "Non-pairwise evaluation actions"
    <Serializable()> Public MustInherit Class clsNonPairwiseEvaluationActionData 'C0464
        Public Property MeasurementType() As ECMeasureType
        Public Property UserID() As Integer
    End Class

    <Serializable()> Public Class clsOneAtATimeEvaluationActionData 'C0464
        Inherits clsNonPairwiseEvaluationActionData

        Public Property Node() As clsNode
        Public Property ControlMeasurementScale As clsMeasurementScale
        Public Property EdgeMeasurementScale As clsMeasurementScale
        Public Property IsEdge As Boolean = False
        Public Property EventType As EventType = EventType.Risk

        Public ReadOnly Property MeasurementScale() As clsMeasurementScale
            Get
                If EdgeMeasurementScale IsNot Nothing Then Return EdgeMeasurementScale
                If ControlMeasurementScale IsNot Nothing Then Return ControlMeasurementScale
                If Node Is Nothing Then Return Nothing Else Return Node.MeasurementScale
            End Get
        End Property

        Public Property Assignment As clsControlAssignment
        Public Property Control As clsControl

        Public Property Judgment() As Object
        Public Sub New(ByVal MeasureType As ECMeasureType, ByVal nUserID As Integer)
            Me.MeasurementType = MeasureType
            Me.UserID = UserID
        End Sub
    End Class

    <Serializable()> Public Class clsAllChildrenEvaluationActionData 'C0667
        Inherits clsNonPairwiseEvaluationActionData

        Public Property ParentNode() As clsNode
        Public Property Children() As New List(Of clsNode)
        Public Property EventType As EventType
        Public ReadOnly Property MeasurementScale() As clsMeasurementScale
            Get
                Return ParentNode.MeasurementScale
            End Get
        End Property

        Public Sub SetData(ByVal Child As clsNode, ByVal data As Object, Optional ByVal Comment As String = "")
            Dim MD As clsNonPairwiseMeasureData = Nothing
            Select Case MeasurementType
                Case ECMeasureType.mtRatings
                    MD = New clsRatingMeasureData(Child.NodeID, ParentNode.NodeID, UserID, CType(data, clsRating), MeasurementScale, data Is Nothing, Comment)
                Case ECMeasureType.mtRegularUtilityCurve
                    MD = New clsUtilityCurveMeasureData(Child.NodeID, ParentNode.NodeID, UserID, CSng(data), MeasurementScale, data Is Nothing, Comment)
                Case ECMeasureType.mtAdvancedUtilityCurve 'C0026
                    MD = New clsUtilityCurveMeasureData(Child.NodeID, ParentNode.NodeID, UserID, CSng(data), MeasurementScale, data Is Nothing, Comment)
                Case ECMeasureType.mtStep
                    MD = New clsStepMeasureData(Child.NodeID, ParentNode.NodeID, UserID, CSng(data), MeasurementScale, Single.IsNaN(data), Comment) 'C0156
                Case ECMeasureType.mtDirect
                    MD = New clsDirectMeasureData(Child.NodeID, ParentNode.NodeID, UserID, CSng(data), data Is Nothing, Comment)
            End Select
            ParentNode.Judgments.AddMeasureData(MD)
        End Sub

        Public Function GetJudgment(ByVal Child As clsNode) As clsNonPairwiseMeasureData
            Return CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(Child.NodeID, ParentNode.NodeID, UserID)
        End Function

        Public Sub New(ByVal MeasurementType As ECMeasureType, ByVal UserID As Integer, ByVal ParentNode As clsNode, ByVal Children As List(Of clsNode))
            Me.MeasurementType = MeasurementType
            Me.ParentNode = ParentNode
            Me.Children = Children
            Me.UserID = UserID
        End Sub
    End Class

    <Serializable()> Public Class clsAllPairwiseEvaluationActionData 'C0959
        Inherits clsNonPairwiseEvaluationActionData

        Public Property ParentNode() As clsNode
        Public Property Children() As New List(Of clsNode)
        Public Property Judgments() As New List(Of clsPairwiseMeasureData)

        Public Sub SetData(ByVal Child1 As clsNode, ByVal Child2 As clsNode, ByVal advantage As Integer, ByVal value As Nullable(Of Single), Optional ByVal Comment As String = "")
            Dim MD As New clsPairwiseMeasureData(Child1.NodeID, Child2.NodeID, advantage, If(value Is Nothing, 0, value), ParentNode.NodeID, UserID, value Is Nothing, Comment)
            ParentNode.Judgments.AddMeasureData(MD)
        End Sub

        Public Function GetJudgment(ByVal Child1 As clsNode, ByVal Child2 As clsNode) As clsPairwiseMeasureData
            Return CType(ParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(Child1.NodeID, Child2.NodeID, UserID)
        End Function

        Public Sub New(ByVal UserID As Integer, ByVal ParentNode As clsNode, ByVal Children As List(Of clsNode), ByVal Judgments As List(Of clsPairwiseMeasureData))
            Me.ParentNode = ParentNode
            Me.Children = Children
            Me.UserID = UserID
            Me.Judgments = Judgments
        End Sub
    End Class

    <Serializable()> Public Class clsAllCoveringObjectivesEvaluationActionData
        Inherits clsNonPairwiseEvaluationActionData

        Public Property Alternative() As clsNode
        Public Property CoveringObjectives() As New List(Of clsNode)

        Public Sub SetData(ByRef CoveringObjective As clsNode, ByVal data As Object, Optional ByVal Comment As String = "")
            Dim MD As clsNonPairwiseMeasureData = Nothing
            Select Case MeasurementType
                Case ECMeasureType.mtRatings
                    MD = New clsRatingMeasureData(Alternative.NodeID, CoveringObjective.NodeID, UserID, CType(data, clsRating), CoveringObjective.MeasurementScale, data Is Nothing, Comment)
                Case ECMeasureType.mtRegularUtilityCurve
                    MD = New clsUtilityCurveMeasureData(Alternative.NodeID, CoveringObjective.NodeID, UserID, CSng(data), CoveringObjective.MeasurementScale, data Is Nothing, Comment)
                Case ECMeasureType.mtStep
                    MD = New clsStepMeasureData(Alternative.NodeID, CoveringObjective.NodeID, UserID, CSng(data), CoveringObjective.MeasurementScale, Single.IsNaN(data), Comment) 'C0156
                Case ECMeasureType.mtDirect
                    MD = New clsDirectMeasureData(Alternative.NodeID, CoveringObjective.NodeID, UserID, CSng(data), data Is Nothing, Comment)
                Case ECMeasureType.mtAdvancedUtilityCurve
                    MD = New clsUtilityCurveMeasureData(Alternative.NodeID, CoveringObjective.NodeID, UserID, CSng(data), CoveringObjective.MeasurementScale, data Is Nothing, Comment)
            End Select
            CoveringObjective.Judgments.AddMeasureData(MD)
        End Sub

        Public Function GetJudgment(ByVal CoveringObjective As clsNode) As clsNonPairwiseMeasureData
            Return CType(CoveringObjective.Judgments, clsNonPairwiseJudgments).GetJudgement(Alternative.NodeID, CoveringObjective.NodeID, UserID) 'C0007
        End Function

        Public Sub New(ByVal MeasurementType As ECMeasureType, ByVal UserID As Integer, ByVal Alternative As clsNode, ByVal CoveringObjectives As List(Of clsNode))
            Me.MeasurementType = MeasurementType
            Me.UserID = UserID
            Me.Alternative = Alternative
            Me.CoveringObjectives = CoveringObjectives
        End Sub
    End Class

    <Serializable()> Public Class clsAllEventsWithNoSourceEvaluationActionData
        Inherits clsNonPairwiseEvaluationActionData

        Public Property Alternatives() As List(Of clsNode)

        Public Sub SetData(ByRef Alternative As clsNode, ByVal data As Object, Optional ByVal Comment As String = "")
            Dim MD As clsNonPairwiseMeasureData = Nothing
            Select Case MeasurementType
                Case ECMeasureType.mtRatings
                    MD = New clsRatingMeasureData(Alternative.NodeID, Alternative.Hierarchy.ProjectManager.Hierarchy(Alternative.Hierarchy.ProjectManager.ActiveHierarchy).Nodes(0).NodeID, UserID, CType(data, clsRating), Alternative.MeasurementScale, data Is Nothing, Comment)
                Case ECMeasureType.mtDirect
                    MD = New clsDirectMeasureData(Alternative.NodeID, Alternative.Hierarchy.ProjectManager.Hierarchy(Alternative.Hierarchy.ProjectManager.ActiveHierarchy).Nodes(0).NodeID, UserID, CSng(data), data Is Nothing, Comment)
            End Select
            Alternative.DirectJudgmentsForNoCause.AddMeasureData(MD)
        End Sub

        Public Function GetJudgment(ByVal Alternative As clsNode) As clsNonPairwiseMeasureData
            Return Alternative.DirectJudgmentsForNoCause.GetJudgement(Alternative.NodeID, Alternative.Hierarchy.ProjectManager.Hierarchy(Alternative.Hierarchy.ProjectManager.ActiveHierarchy).Nodes(0).NodeID, UserID)
        End Function

        Public Sub New(ByVal MeasureType As ECMeasureType, ByVal nUserID As Integer, ByVal Alternatives As List(Of clsNode))
            MeasurementType = MeasureType
            UserID = nUserID
            Me.Alternatives = Alternatives
        End Sub
    End Class
#End Region

#Region "Results actions"
    <Serializable()> Public Class clsShowLocalResultsActionData
        Public Property ProjectManager As clsProjectManager
        Public Property ParentNode() As clsNode
        Public Property PWOutcomesNode() As clsNode = Nothing
        Public Property ResultsViewMode() As ResultsView
        Public Property ShowConsistency() As Boolean
        Public Property IsSynchronous() As Boolean 'C0100


        Private mInconsistencyIndexIndividual As Single
        Private mInconsistencyRatioIndividual As Single
        Private mInconsistencyIndexCombined As Single
        Private mInconsistencyRatioCombined As Single

        Private mExpectedIndividual As Single
        Private mExpectedCombined As Single

        Private mResultsList As New ArrayList
        Public ReadOnly Property ResultsList(ByVal UserID As Integer, ByVal IndividualUserID As Integer) As ArrayList
            Get
                PrepareResultsList(UserID, IndividualUserID) 'C0092
                Return mResultsList
            End Get
        End Property

        Private mUserID As Integer
        Public Property UserID() As Integer
            Get
                If mUserID = UNDEFINED_USER_ID Then
                    Return ProjectManager.UserID
                Else
                    Return mUserID
                End If
            End Get
            Set(ByVal value As Integer)
                mUserID = value
            End Set
        End Property

        Public Function CanShowResultsForUser(ByVal UserID As Integer) As Boolean
            Dim JA As New clsJudgmentsAnalyzer(ProjectManager.CalculationsManager.SynthesisMode, ProjectManager)
            Return JA.CanShowResultsForNode(UserID, ParentNode, False, False)
        End Function

        Public ReadOnly Property CanShowIndividualResults() As Boolean
            Get
                Dim JA As New clsJudgmentsAnalyzer(ProjectManager.CalculationsManager.SynthesisMode, ProjectManager)
                Return JA.CanShowResultsForNode(UserID, ParentNode, False, False)
            End Get
        End Property

        Public ReadOnly Property CanShowGroupResults(Optional ByVal UsersList As ArrayList = Nothing) As Boolean 'C0094
            Get
                Dim JA As New clsJudgmentsAnalyzer(ProjectManager.CalculationsManager.SynthesisMode, ProjectManager)
                Return JA.CanShowResultsForNode(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, ParentNode, False, False)
            End Get
        End Property

        Public ReadOnly Property InconsistencyRatio() As Double
            Get
                Return If(ECCore.IsPWMeasurementType(ParentNode.MeasureType), CType(ParentNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconRatio, 0)
            End Get
        End Property

        Public ReadOnly Property InconsistencyIndex() As Double
            Get
                Return If(ECCore.IsPWMeasurementType(ParentNode.MeasureType), CType(ParentNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex, 0)
            End Get
        End Property

        Public ReadOnly Property InconsistencyIndividual() As Single
            Get
                Return mInconsistencyIndexIndividual
            End Get
        End Property

        Public ReadOnly Property InconsistencyCombined() As Single
            Get
                Return mInconsistencyIndexCombined
            End Get
        End Property

        Public Function GetNodesList(ByVal IndividualUserID As Integer) As List(Of clsNode)
            If ParentNode Is Nothing Then Return Nothing

            Dim NodesList As New List(Of clsNode)

            If ParentNode.MeasureType = ECMeasureType.mtPWOutcomes And PWOutcomesNode IsNot Nothing Then
                Dim RS As clsRatingScale = CType(ParentNode.MeasurementScale, clsRatingScale)
                For Each R As clsRating In RS.RatingSet
                    Dim node As clsNode = New clsNode
                    node.NodeID = R.ID
                    node.NodeGuidID = R.GuidID
                    node.NodeName = ExpertChoice.Service.Double2String(R.Value * 100, , True)   ' D3057
                    node.InfoDoc = ""
                    node.Comment = ""
                    NodesList.Add(node)
                Next
            Else
                NodesList = ParentNode.GetNodesBelow(IndividualUserID)
                For i As Integer = NodesList.Count - 1 To 0 Step -1
                    If NodesList(i).RiskNodeType = RiskNodeType.ntCategory Then NodesList.RemoveAt(i)
                Next
            End If

            Return NodesList
        End Function

        Public ReadOnly Property ShowExpectedValue As Boolean
            Get
                If ParentNode.MeasureType = ECMeasureType.mtPWOutcomes And PWOutcomesNode IsNot Nothing Then Return True
            End Get
        End Property

        Public ReadOnly Property ExpectedValue(UserID As Integer) As Single
            Get
                If PWOutcomesNode Is Nothing Then Return 0
                Return If(IsCombinedUserID(UserID), mExpectedCombined, mExpectedIndividual)
            End Get
        End Property

        Private Sub PrepareResultsList(ByVal UserID As Integer, ByVal IndividualUserID As Integer)
            Dim oldUseCIS As Boolean = ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes
            ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes = False

            mResultsList.Clear()

            If Not IsCombinedUserID(UserID) Or IsCombinedUserID(UserID) And (Not IsSynchronous) Then
                If IsCombinedUserID(UserID) Then
                    Dim CG As clsCombinedGroup = ProjectManager.CombinedGroups.GetDefaultCombinedGroup

                    Dim nodes As New List(Of clsNode)
                    nodes.Add(ParentNode)
                    ProjectManager.CalculationsManager.CreateCombinedJudgments(CG, nodes)
                    ParentNode.CalculateLocal(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG))
                Else
                    Dim tUser As clsUser = ProjectManager.GetUserByID(UserID)
                    ProjectManager.StorageManager.Reader.LoadUserData(tUser)
                    ParentNode.CalculateLocal(New clsCalculationTarget(CalculationTargetType.cttUser, tUser))
                End If

                If ParentNode.MeasureType = ECMeasureType.mtPWOutcomes And PWOutcomesNode IsNot Nothing Then
                    Dim value As Double = ParentNode.Judgments.Weights.GetUserWeights(UserID, ParentNode.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, ParentNode.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(PWOutcomesNode.NodeID)
                    If IsCombinedUserID(UserID) Then
                        mExpectedCombined = value
                    Else
                        mExpectedIndividual = value
                    End If
                End If

                Select Case ParentNode.MeasureType
                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                        If IsCombinedUserID(UserID) Then
                            mInconsistencyIndexCombined = CType(ParentNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                            mInconsistencyRatioCombined = CType(ParentNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                        Else
                            mInconsistencyIndexIndividual = CType(ParentNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                            mInconsistencyRatioIndividual = CType(ParentNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                        End If
                    Case ECMeasureType.mtPWOutcomes
                        If PWOutcomesNode IsNot Nothing Then
                            If IsCombinedUserID(UserID) Then
                                mInconsistencyIndexCombined = CType(PWOutcomesNode.PWOutcomesJudgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                                mInconsistencyRatioCombined = CType(PWOutcomesNode.PWOutcomesJudgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                            Else
                                mInconsistencyIndexIndividual = CType(PWOutcomesNode.PWOutcomesJudgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                                mInconsistencyRatioIndividual = CType(PWOutcomesNode.PWOutcomesJudgments, clsPairwiseJudgments).EigenCalcs.InconIndex
                            End If
                        Else
                            mInconsistencyIndexIndividual = 0
                            mInconsistencyRatioIndividual = 0
                            mInconsistencyIndexCombined = 0
                            mInconsistencyRatioCombined = 0
                        End If
                    Case Else
                        mInconsistencyIndexIndividual = 0
                        mInconsistencyRatioIndividual = 0
                        mInconsistencyIndexCombined = 0
                        mInconsistencyRatioCombined = 0
                End Select

            End If

            Dim NodesList As List(Of clsNode) = GetNodesList(IndividualUserID)

            Dim idealPriority As Single
            If Not IsCombinedUserID(UserID) Or IsCombinedUserID(UserID) And Not IsSynchronous Then
                For i As Integer = 0 To NodesList.Count - 1
                    Dim nd As clsNode = CType(NodesList(i), clsNode)
                    If ParentNode.MeasureType = ECMeasureType.mtPWOutcomes And PWOutcomesNode IsNot Nothing Then
                        mResultsList.Add(New clsResultsItem(nd.NodeName, PWOutcomesNode.PWOutcomesJudgments.Weights.GetUserWeights(UserID, ParentNode.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, ParentNode.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(nd.NodeID), PWOutcomesNode.PWOutcomesJudgments.Weights.GetUserWeights(UserID, ParentNode.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, ParentNode.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(nd.NodeID), nd.NodeID))
                    Else
                        mResultsList.Add(New clsResultsItem(nd.NodeName, ParentNode.Judgments.Weights.GetUserWeights(UserID, ParentNode.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, ParentNode.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(nd.NodeID), ParentNode.Judgments.Weights.GetUserWeights(UserID, ParentNode.Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, ParentNode.Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(nd.NodeID), nd.NodeID))
                    End If
                Next
            Else
                'TODO: pw outcomes!
                Dim NodesPriorities As New List(Of Single)
                Select Case ParentNode.Hierarchy.ProjectManager.StorageManager.StorageType
                    Case ECModelStorageType.mstCanvasStreamDatabase
                        TeamTimeFuncs.ReadCombinedResultsFromCanvasStreamDatabase(ProjectManager.ProjectLocation, ProjectManager.StorageManager.ModelID, ParentNode.Hierarchy.ProjectManager.StorageManager.ProviderType, ParentNode, NodesList, True, NodesPriorities, idealPriority)
                End Select

                If NodesList.Count <= NodesPriorities.Count Then
                    For i As Integer = 0 To NodesList.Count - 1
                        mResultsList.Add(New clsResultsItem(CType(NodesList(i), clsNode).NodeName, CSng(NodesPriorities(i)), CSng(NodesPriorities(i)), CType(NodesList(i), clsNode).NodeID)) 'C0582
                    Next
                End If
            End If

            If ParentNode.Hierarchy.HierarchyType = ECHierarchyType.htMeasure Or CType(ParentNode.Hierarchy.ProjectManager, clsProjectManager).PipeParameters.IdealViewType = IdealViewType.ivtMaxIsOne Then
                Dim max As Single = 0
                For Each RI As clsResultsItem In mResultsList
                    If RI.Value > max Then
                        max = RI.Value
                    End If
                Next
                If max <> 0 Then
                    For Each RI As clsResultsItem In mResultsList
                        RI.Value /= max
                    Next
                End If
            Else
                'If ProjectManager.CalculationsManager.IncludeIdealAlternative And Not ProjectManager.CalculationsManager.ShowIdealAlternative And (ParentNode.MeasureType = ECMeasureType.mtPairwise Or ParentNode.MeasureType = ECMeasureType.mtPWOutcomes) Then
                If ParentNode.MeasureType <> ECMeasureType.mtPWAnalogous AndAlso ProjectManager.CalculationsManager.IncludeIdealAlternative And Not ProjectManager.CalculationsManager.ShowIdealAlternative Then
                    Dim sum As Single = 0
                    Dim sumUnnorm As Single = 0
                    For Each RI As clsResultsItem In mResultsList
                        sum += RI.Value
                        sumUnnorm += RI.UnnormalizedValue
                    Next
                    If sum <> 0 Then
                        For Each RI As clsResultsItem In mResultsList
                            RI.Value /= sum
                            'RI.UnnormalizedValue /= sumUnnorm
                        Next
                    End If
                End If
            End If

            ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes = oldUseCIS
        End Sub

        Public Function OnlyMainDiagonalEvaluated() As Boolean
            Select Case ParentNode.MeasureType
                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                    Return CType(ParentNode.Judgments, clsPairwiseJudgments).OnlyMainDiagonalEvaluated(UserID)
                Case ECMeasureType.mtPWOutcomes
                    If PWOutcomesNode IsNot Nothing Then
                        Return PWOutcomesNode.PWOutcomesJudgments.OnlyMainDiagonalEvaluated(UserID)
                    End If
            End Select
        End Function

        Public Sub New(ByVal ParentNode As clsNode, ByVal ResultsViewMode As ResultsView, ByVal ShowConsistency As Boolean, Optional ByVal IsSynchronousSessionEvaluator As Boolean = False, Optional ByVal UserID As Integer = UNDEFINED_USER_ID, Optional PWOutcomesNode As clsNode = Nothing)
            Me.IsSynchronous = IsSynchronousSessionEvaluator
            Me.ParentNode = ParentNode
            Me.PWOutcomesNode = PWOutcomesNode
            Me.ResultsViewMode = ResultsViewMode
            Me.ShowConsistency = ShowConsistency
            Me.UserID = UserID
            If ParentNode IsNot Nothing Then ProjectManager = ParentNode.Hierarchy.ProjectManager
        End Sub
    End Class

    <Serializable()> Public Class clsShowGlobalResultsActionData 'C0464
        Private mSynthesisMode As ECSynthesisMode
        Private mPrjManager As clsProjectManager

        Private mResultsList As New ArrayList

        Private mJudgmentsAnalyzer As clsJudgmentsAnalyzer

        Public Property SynthesisMode() As ECSynthesisMode
            Get
                Return mSynthesisMode
            End Get
            Set(ByVal value As ECSynthesisMode)
                mSynthesisMode = value
                If mJudgmentsAnalyzer IsNot Nothing Then
                    mJudgmentsAnalyzer.SynthesisMode = mSynthesisMode
                End If
            End Set
        End Property

        Public Property IsSynchronous() As Boolean

        Public Property ResultsViewMode() As ResultsView

        Public Property ProjectManager() As clsProjectManager 'C0230
            Get
                Return mPrjManager
            End Get
            Set(ByVal value As clsProjectManager)
                mPrjManager = value
                If mJudgmentsAnalyzer IsNot Nothing Then
                    mJudgmentsAnalyzer = Nothing
                    mJudgmentsAnalyzer = New clsJudgmentsAnalyzer(mSynthesisMode, mPrjManager)
                End If
            End Set
        End Property

        Public Property WRTNode() As clsNode = Nothing
        Public ReadOnly Property ResultsList(ByVal UserID As Integer, ByVal IndividualUserID As Integer, Optional ByVal NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPriority) As ArrayList 'C0647
            Get
                PrepareResultsList(UserID, IndividualUserID, NormalizationMode) 'C0647
                Return mResultsList
            End Get
        End Property

        Public ReadOnly Property CanShowIndividualResults(ByVal UserID As Integer, Optional ByVal WRTNode As clsNode = Nothing) As Boolean
            Get
                Return mJudgmentsAnalyzer.CanShowIndividualResults(UserID, WRTNode)
            End Get
        End Property

        Public ReadOnly Property CanShowGroupResults(Optional ByVal CombinedGroup As clsCombinedGroup = Nothing) As Boolean 'C0555
            Get
                If IsSynchronous Then
                    Dim CalcWRTNode As clsNode = If(WRTNode Is Nothing, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), WRTNode)
                    Select Case ProjectManager.StorageManager.StorageType
                        Case ECModelStorageType.mstCanvasStreamDatabase
                            Return TeamTimeFuncs.PrecalculatedCombinedResultsExistInCanvasStreamDatabase(ProjectManager.ProjectLocation, ProjectManager.StorageManager.ModelID, ProjectManager.StorageManager.ProviderType) 'C0362
                        Case Else
                            Return False
                    End Select
                Else
                    Return mJudgmentsAnalyzer.CanShowGroupResults(WRTNode, CombinedGroup)
                End If
            End Get
        End Property

        Public Function GetAlternativesList(ByVal IndividualUserID As Integer) As List(Of clsNode)
            If mPrjManager Is Nothing Then Return Nothing

            Dim CalcWRTNode As clsNode
            CalcWRTNode = If(WRTNode Is Nothing, mPrjManager.ActiveObjectives.Nodes(0), WRTNode)

            Dim AltsList As New List(Of clsNode)
            AltsList = mPrjManager.UsersRoles.GetAllowedAlternatives(IndividualUserID, CalcWRTNode)
            Return AltsList
        End Function

        Private Sub PrepareResultsList(ByVal UserID As Integer, ByVal IndividualUserID As Integer, Optional ByVal NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPriority) 'C0647
            'Dim oldSynthMode As ECSynthesisMode = mPrjManager.CalculationsManager.SynthesisMode
            mPrjManager.CalculationsManager.SynthesisMode = ProjectManager.PipeParameters.SynthesisMode

            mResultsList.Clear()

            Dim CalcWRTNode As clsNode
            CalcWRTNode = If(WRTNode Is Nothing, mPrjManager.Hierarchy(mPrjManager.ActiveHierarchy).Nodes(0), WRTNode)

            Dim AltsList As New List(Of clsNode)
            AltsList = mPrjManager.UsersRoles.GetAllowedAlternatives(UserID, CalcWRTNode)

            Dim NodesPriorities As New List(Of Single)
            Dim idealPriority As Single

            If Not IsCombinedUserID(UserID) Then
                mPrjManager.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttUser, mPrjManager.GetUserByID(UserID)), CalcWRTNode, mPrjManager.ActiveHierarchy)
            Else
                If IsSynchronous Then
                    Select Case ProjectManager.StorageManager.StorageType
                        Case ECModelStorageType.mstCanvasStreamDatabase
                            TeamTimeFuncs.ReadCombinedResultsFromCanvasStreamDatabase(ProjectManager.ProjectLocation, ProjectManager.StorageManager.ModelID, ProjectManager.StorageManager.ProviderType, CalcWRTNode, AltsList, False, NodesPriorities, idealPriority) 'C0362
                    End Select
                Else
                    mPrjManager.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, mPrjManager.CombinedGroups.GetDefaultCombinedGroup), CalcWRTNode, mPrjManager.ActiveHierarchy) 'C0551
                End If
            End If

            If IsSynchronous And IsCombinedUserID(UserID) Then
                If NodesPriorities.Count >= AltsList.Count Then
                    For i As Integer = 0 To AltsList.Count - 1
                        mResultsList.Add(New clsResultsItem(CType(AltsList(i), clsNode).NodeName, CSng(NodesPriorities(i)), CSng(NodesPriorities(i)), CType(AltsList(i), clsNode).NodeID)) 'C0582
                    Next
                End If
            Else
                For Each alt As clsNode In AltsList 'C0823
                    mResultsList.Add(New clsResultsItem(alt.NodeName, alt.WRTGlobalPriority, alt.UnnormalizedPriority, alt.NodeID)) 'C0582
                Next
            End If

            If ProjectManager.PipeParameters.IdealViewType = IdealViewType.ivtMaxIsOne Then
                Dim max As Single = 0
                For Each RI As clsResultsItem In mResultsList
                    If RI.Value > max Then
                        max = RI.Value
                    End If
                Next
                If max <> 0 Then
                    For Each RI As clsResultsItem In mResultsList
                        RI.Value /= max
                    Next
                End If
            End If

            'normalize using normalization options
            Select Case NormalizationMode
                Case AlternativeNormalizationOptions.anoPriority
                    Dim sum As Single = 0
                    For Each RI As clsResultsItem In mResultsList
                        sum += RI.Value
                    Next
                    If sum <> 0 Then
                        For Each RI As clsResultsItem In mResultsList
                            RI.Value /= sum
                        Next
                    End If
                Case AlternativeNormalizationOptions.anoMultipleOfMin
                    Dim min As Single = 2
                    For Each RI As clsResultsItem In mResultsList
                        If RI.Value < min Then
                            min = RI.Value
                        End If
                    Next
                    If min <> 2 And min <> 0 Then
                        For Each RI As clsResultsItem In mResultsList
                            RI.Value /= min
                        Next
                    End If
                    'Case AlternativeNormalizationOptions.anoPercentOfMax, AlternativeNormalizationOptions.anoUnnormalized
                Case AlternativeNormalizationOptions.anoPercentOfMax
                    Dim max As Single = -1
                    For Each RI As clsResultsItem In mResultsList
                        If RI.Value > max Then
                            max = RI.Value
                        End If
                    Next
                    If max <> -1 And max <> 0 Then
                        For Each RI As clsResultsItem In mResultsList
                            RI.Value /= max
                        Next
                    End If
            End Select

            If mPrjManager.CalculationsManager.SynthesisMode = ECSynthesisMode.smDistributive Then
                Dim sum As Single = 0
                For Each RI As clsResultsItem In mResultsList
                    sum += RI.UnnormalizedValue
                Next
                If sum <> 0 Then
                    For Each RI As clsResultsItem In mResultsList
                        RI.UnnormalizedValue /= sum
                    Next
                End If
            End If

            'mPrjManager.CalculationsManager.SynthesisMode = oldSynthMode
        End Sub

        Public Sub New(ByVal SynthesisMode As ECSynthesisMode, ByVal ResultsViewMode As ResultsView, ByVal ProjectManager As clsProjectManager, Optional ByVal IsSynchronousSessionEvaluator As Boolean = False) 'C0100
            Me.IsSynchronous = IsSynchronousSessionEvaluator 'C0100
            Me.SynthesisMode = SynthesisMode
            Me.ResultsViewMode = ResultsViewMode
            Me.ProjectManager = ProjectManager

            mJudgmentsAnalyzer = New clsJudgmentsAnalyzer(mSynthesisMode, mPrjManager)
        End Sub
    End Class

#End Region

    <Serializable()> Public Class clsSensitivityAnalysisActionData
        Public Property SAType() As SAType = SAType.satDynamic
        Public Property IsSynchronousSessionEvaluator() As Boolean

        Public Sub New(ByVal ProjectManager As clsProjectManager, Optional ByVal SAType As SAType = Canvas.SAType.satDynamic, Optional ByVal IsSynchronousSessionEvaluator As Boolean = False)
            Me.SAType = SAType
            Me.IsSynchronousSessionEvaluator = IsSynchronousSessionEvaluator
        End Sub
    End Class

    <Serializable()> Public Class clsInformationPageActionData
        Public Property Description() As String
        Public Property Text() As String

        Public Sub New(ByVal Description As String)
            Me.Description = Description
        End Sub
    End Class

    <Serializable()> Public Class clsSurveyActionData
        Public Property SurveyURL() As String

        Public Sub New(ByVal URL As String)
            Me.SurveyURL = URL
        End Sub
    End Class

    <Serializable()> Public Class clsSpyronSurveyAction
        Public Property SurveyID() As String
        Public Property StepNumber() As Integer
        Public Property SurveyType As Integer = 1

        Public Sub New(ByVal SurveyID As String, SurveyType As Integer, Optional ByVal StepNumber As Integer = 1)
            Me.SurveyID = SurveyID
            Me.StepNumber = StepNumber
            Me.SurveyType = SurveyType
        End Sub

    End Class

#End Region
End Namespace