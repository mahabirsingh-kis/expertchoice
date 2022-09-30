Imports ECCore
Imports Canvas
Imports System.Linq

Namespace Canvas
    Public Delegate Function IsStaticSurveyFunction(ByVal SurveyGuid As String, ByRef IsStatic As Boolean) As Boolean
    Public Delegate Function GetSurveyStepsCountFunction(ByVal ProjectID As Integer, ByVal ASurveyType As Integer, ByRef StepsCount As Integer, ByRef HasSelectAlternatives As Boolean) As Boolean  ' D6671

    <Serializable()> Public Enum ecPipeType
        ptAnytime = 0
        ptTeamTime = 1
        ptRiskWithControls = 2
    End Enum
    ' D2505 ==

    <Serializable()> Public Class clsPipeBuilder
        Public Property ProjectManager() As clsProjectManager
        Public Property PipeParameters() As clsPipeParamaters
        Public Property ProjectParameters() As clsProjectParametersWithDefaults = Nothing

        Private mPipe As List(Of clsAction)

        Public Property IsSynchronousSessionEvaluator() As Boolean = False
        Public Property IsSynchronousSession() As Boolean = False
        Public Property SynchronousOwner() As clsUser = Nothing

        Private mPipeCreated As Boolean 'C0167
        Public Property PipeCreated As Boolean
            Get
                Return mPipeCreated And (mPipeUserID = ProjectManager.UserID)
            End Get
            Set(ByVal value As Boolean)
                mPipeCreated = value
                If ProjectManager.ResourceAligner.isLoaded And Not value Then ProjectManager.ResourceAligner.isLoaded = False
            End Set
        End Property

        Private mPipeUserID As Integer

        Private mRandomSeed As Integer

        Private mPipeType As ecPipeType = ecPipeType.ptAnytime
        Public ReadOnly Property PipeType As ecPipeType
            Get
                Return mPipeType
            End Get
        End Property

        Private mHierarchyID As Integer = -1

        Public GetSurveyInfo As GetSurveyStepsCountFunction

        Public OPT_RISKION_JOIN_PIPE As Boolean = False    ' D3253
        Friend Const _OPT_CUSTOM_PHASE_NON_EVAL As Boolean = True    ' D4053

        Public ReadOnly Property Objectives As clsHierarchy
            Get
                Return ProjectManager?.ActiveObjectives
            End Get
        End Property

        Public ReadOnly Property Alternatives As clsHierarchy
            Get
                Return ProjectManager?.ActiveAlternatives
            End Get
        End Property

        Public Property TeamTimePipe() As clsTeamTimePipe

        Private Function GetNodeIndexByID(ByVal NodesList As List(Of clsNode), ByVal NodeID As Integer) As Integer
            If NodesList Is Nothing Then Return -1
            Return NodesList.FindIndex(Function(n) (n.NodeID = NodeID))
        End Function

        Private Function GetRatingIndexByID(ByVal RatingsList As List(Of clsRating), ByVal RatingID As Integer) As Integer
            If RatingsList Is Nothing Then Return -1
            Return RatingsList.FindIndex(Function(r) (r.ID = RatingID))
        End Function

        Public Function GetNodesBelowForPipe(ByVal node As clsNode) As List(Of clsNode)
            Dim res As New List(Of clsNode)
            If Not IsSynchronousSession Then
                res = node.GetNodesBelow(ProjectManager.UserID)
            Else
                If Not node.IsTerminalNode Then
                    res = node.GetNodesBelow(UNDEFINED_USER_ID)
                Else
                    Dim allAlts As List(Of clsNode) = node.GetNodesBelow(UNDEFINED_USER_ID)
                    For Each alt As clsNode In allAlts
                        Dim CanEval As Boolean = False
                        Dim j As Integer = 0
                        While Not CanEval And (j < TeamTimePipe.Users.Count)
                            CanEval = Not node.DisabledForUser(TeamTimePipe.UsersList(j).UserID) And ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, TeamTimePipe.UsersList(j).UserID)
                            j += 1
                        End While
                        If CanEval Then
                            res.Add(alt)
                        End If
                    Next
                End If
            End If

            If Not node.IsTerminalNode And ProjectManager.IsRiskProject AndAlso Objectives.HierarchyID = ECHierarchyID.hidLikelihood Then
                res.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
            End If

            Return res
        End Function

        Private Sub AddPairwiseData(ByVal node As clsNode, Optional ByVal Recursive As Boolean = True, Optional ByVal AllowTerminalNodes As Boolean = False, Optional AddingEventsWithNoSource As Boolean = False) 'C0959
            Dim HasOnlyGoal As Boolean = Objectives.Nodes.Count = 1
            If HasOnlyGoal And Not AddingEventsWithNoSource Then Exit Sub

            If node Is Nothing OrElse node.PipeCreated Then Exit Sub ' D7237
            If node.Judgments Is Nothing OrElse node.Judgments.JudgmentsFromUser(ProjectManager.UserID) Is Nothing Then Exit Sub ' D7237

            Dim pwAction As clsAction ' pairwise action
            Dim lrAction As clsAction ' local results action

            Dim NodesList As List(Of clsNode) 'C0384

            Dim CanEvaluateNode As Boolean
            If Not IsSynchronousSession Then
                CanEvaluateNode = Not node.DisabledForUser(ProjectManager.UserID) And node.IsAllowed(ProjectManager.UserID)
            Else
                Dim SomeUserCanEvaluate As Boolean = False
                Dim i As Integer = 0
                While Not SomeUserCanEvaluate And (i < TeamTimePipe.Users.Count)
                    SomeUserCanEvaluate = Not node.DisabledForUser(TeamTimePipe.UsersList(i).UserID) And node.IsAllowed(TeamTimePipe.UsersList(i).UserID)
                    i += 1
                End While
                CanEvaluateNode = SomeUserCanEvaluate
            End If

            Dim JudgmentsCount As Integer = 0

            Dim isTerminalNode As Boolean = node.IsTerminalNode

            If CanEvaluateNode AndAlso Not isTerminalNode OrElse (isTerminalNode And AllowTerminalNodes) Then
                If ProjectManager.IsRiskProject AndAlso (Objectives.HierarchyID = ECHierarchyID.hidLikelihood) AndAlso
                    (node.MeasureType = ECMeasureType.mtPWOutcomes) AndAlso node.IsAlternative AndAlso Objectives.GetUncontributedAlternatives.Contains(node) Then
                    ' part for pw of probabilities for event with no source
                    NodesList = New List(Of clsNode)
                    NodesList.Add(node)
                Else
                    NodesList = GetNodesBelowForPipe(node)
                End If

                Dim nodesLookup As New Dictionary(Of Integer, Integer)
                For k As Integer = 0 To NodesList.Count - 1
                    nodesLookup.Add(NodesList(k).NodeID, k)
                Next

                If node.Judgments Is Nothing OrElse node.Judgments.JudgmentsFromUser(ProjectManager.UserID) Is Nothing Then Exit Sub

                Select Case node.MeasureType
                    Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                        Dim IsOneAtATime As Boolean = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DISPLAY_OPTION_PAIRWISE_ID, node.NodeGuidID) = 1
                        Dim IsAllPW As Boolean = Not IsOneAtATime And Not IsSynchronousSession

                        Dim PWList As New List(Of clsPairwiseMeasureData)

                        Dim comparer As clsPairwiseComparer = New clsPairwiseComparer
                        comparer.Node = node
                        node.Judgments.JudgmentsFromUser(ProjectManager.UserID).Sort(comparer)

                        Dim DiagonalsEvaluationMode As DiagonalsEvaluation = PipeParameters.EvaluateDiagonals   ' D6123
                        If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID) Then
                            DiagonalsEvaluationMode = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                        Else
                            If node.IsTerminalNode Then
                                If PipeParameters.ForceAllDiagonalsForAlternatives And (NodesList.Count < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                    DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                End If
                            Else
                                If PipeParameters.ForceAllDiagonals And (NodesList.Count < PipeParameters.ForceAllDiagonalsLimit) Then
                                    DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                                End If
                            End If
                        End If

                        Dim Node1 As clsNode
                        Dim Node2 As clsNode

                        Dim pwSequenceOriginal As New ArrayList
                        Dim pwSequenceRandom As New ArrayList


                        Select Case DiagonalsEvaluationMode
                            Case DiagonalsEvaluation.deAll
                                ' add all diagonals
                                Select Case PipeParameters.PairwiseMatrixEvaluationOrder
                                    Case PairwiseEvaluationOrder.peoDiagonals
                                        For i As Integer = 1 To NodesList.Count - 1
                                            For Each pd As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(ProjectManager.UserID)
                                                'If node.IsTerminalNode Then
                                                '    Node1 = Alternatives.GetNodeByID(pd.FirstNodeID)
                                                '    Node2 = Alternatives.GetNodeByID(pd.SecondNodeID)
                                                'Else
                                                '    Node1 = Objectives.GetNodeByID(pd.FirstNodeID)
                                                '    Node2 = Objectives.GetNodeByID(pd.SecondNodeID)
                                                'End If

                                                If nodesLookup.ContainsKey(pd.FirstNodeID) AndAlso nodesLookup.ContainsKey(pd.SecondNodeID) Then
                                                    'If NodesList.Contains(Node1) AndAlso NodesList.Contains(Node2) Then
                                                    If Math.Abs(nodesLookup(pd.FirstNodeID) - nodesLookup(pd.SecondNodeID)) = i Then
                                                        'If Math.Abs(GetNodeIndexByID(NodesList, pd.FirstNodeID) - GetNodeIndexByID(NodesList, pd.SecondNodeID)) = i Then
                                                        If IsAllPW Then
                                                            PWList.Add(pd)
                                                        Else
                                                            pwAction = New clsAction
                                                            pwAction.ActionType = ActionType.atPairwise
                                                            pwAction.ActionData = pd

                                                            mPipe.Add(pwAction)
                                                            JudgmentsCount += 1
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        Next

                                    Case PairwiseEvaluationOrder.peoRows
                                        For i As Integer = 0 To NodesList.Count - 2
                                            For j As Integer = i + 1 To NodesList.Count - 1
                                                For Each pd As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(ProjectManager.UserID)
                                                    If node.IsTerminalNode Then
                                                        Node1 = Alternatives.GetNodeByID(pd.FirstNodeID)
                                                        Node2 = Alternatives.GetNodeByID(pd.SecondNodeID)
                                                    Else
                                                        Node1 = Objectives.GetNodeByID(pd.FirstNodeID)
                                                        Node2 = Objectives.GetNodeByID(pd.SecondNodeID)
                                                    End If

                                                    If NodesList.Contains(Node1) AndAlso NodesList.Contains(Node2) Then
                                                        If (GetNodeIndexByID(NodesList, pd.FirstNodeID) = i) AndAlso (GetNodeIndexByID(NodesList, pd.SecondNodeID) = j) OrElse
                                                            (GetNodeIndexByID(NodesList, pd.FirstNodeID) = j) AndAlso (GetNodeIndexByID(NodesList, pd.SecondNodeID) = i) Then

                                                            If IsAllPW Then
                                                                PWList.Add(pd)
                                                            Else
                                                                pwAction = New clsAction
                                                                pwAction.ActionType = ActionType.atPairwise
                                                                pwAction.ActionData = pd

                                                                mPipe.Add(pwAction)
                                                                JudgmentsCount += 1
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                            Next
                                        Next

                                    Case PairwiseEvaluationOrder.peoColumns
                                        For i As Integer = 1 To NodesList.Count - 1
                                            For j As Integer = 0 To i
                                                For Each pd As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(ProjectManager.UserID)
                                                    If node.IsTerminalNode Then
                                                        Node1 = Alternatives.GetNodeByID(pd.FirstNodeID)
                                                        Node2 = Alternatives.GetNodeByID(pd.SecondNodeID)
                                                    Else
                                                        Node1 = Objectives.GetNodeByID(pd.FirstNodeID)
                                                        Node2 = Objectives.GetNodeByID(pd.SecondNodeID)
                                                    End If

                                                    If NodesList.Contains(Node1) AndAlso NodesList.Contains(Node2) Then
                                                        If (GetNodeIndexByID(NodesList, pd.FirstNodeID) = i) AndAlso (GetNodeIndexByID(NodesList, pd.SecondNodeID) = j) OrElse
                                                            (GetNodeIndexByID(NodesList, pd.FirstNodeID) = j) AndAlso (GetNodeIndexByID(NodesList, pd.SecondNodeID) = i) Then

                                                            If IsAllPW Then
                                                                PWList.Add(pd)
                                                            Else
                                                                pwAction = New clsAction
                                                                pwAction.ActionType = ActionType.atPairwise
                                                                pwAction.ActionData = pd

                                                                mPipe.Add(pwAction)
                                                                JudgmentsCount += 1
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                            Next
                                        Next
                                End Select

                            Case DiagonalsEvaluation.deFirst
                                ' add first diagonal
                                For Each pd As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(ProjectManager.UserID)
                                    If node.IsTerminalNode Then
                                        Node1 = Alternatives.GetNodeByID(pd.FirstNodeID)
                                        Node2 = Alternatives.GetNodeByID(pd.SecondNodeID)
                                    Else
                                        Node1 = Objectives.GetNodeByID(pd.FirstNodeID)
                                        Node2 = Objectives.GetNodeByID(pd.SecondNodeID)
                                    End If

                                    If NodesList.Contains(Node1) AndAlso NodesList.Contains(Node2) Then
                                        If Math.Abs(GetNodeIndexByID(NodesList, pd.FirstNodeID) - GetNodeIndexByID(NodesList, pd.SecondNodeID)) = 1 Then
                                            If IsAllPW Then
                                                PWList.Add(pd)
                                            Else
                                                pwAction = New clsAction
                                                pwAction.ActionType = ActionType.atPairwise
                                                pwAction.ActionData = pd

                                                mPipe.Add(pwAction)
                                                JudgmentsCount += 1
                                            End If
                                        End If
                                    End If
                                Next

                            Case DiagonalsEvaluation.deFirstAndSecond
                                ' add first diagonal
                                For Each pd As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(ProjectManager.UserID)
                                    If node.IsTerminalNode Then
                                        Node1 = Alternatives.GetNodeByID(pd.FirstNodeID)
                                        Node2 = Alternatives.GetNodeByID(pd.SecondNodeID)
                                    Else
                                        Node1 = Objectives.GetNodeByID(pd.FirstNodeID)
                                        Node2 = Objectives.GetNodeByID(pd.SecondNodeID)
                                    End If

                                    If NodesList.Contains(Node1) AndAlso NodesList.Contains(Node2) Then
                                        If Math.Abs(GetNodeIndexByID(NodesList, pd.FirstNodeID) - GetNodeIndexByID(NodesList, pd.SecondNodeID)) = 1 Then
                                            If IsAllPW Then
                                                PWList.Add(pd)
                                            Else
                                                pwAction = New clsAction
                                                pwAction.ActionType = ActionType.atPairwise
                                                pwAction.ActionData = pd

                                                mPipe.Add(pwAction)
                                                JudgmentsCount += 1
                                            End If
                                        End If
                                    End If
                                Next

                                ' add second diagonal
                                For Each pd As clsPairwiseMeasureData In node.Judgments.JudgmentsFromUser(ProjectManager.UserID)
                                    If node.IsTerminalNode Then
                                        Node1 = Alternatives.GetNodeByID(pd.FirstNodeID)
                                        Node2 = Alternatives.GetNodeByID(pd.SecondNodeID)
                                    Else
                                        Node1 = Objectives.GetNodeByID(pd.FirstNodeID)
                                        Node2 = Objectives.GetNodeByID(pd.SecondNodeID)
                                    End If

                                    If NodesList.Contains(Node1) AndAlso NodesList.Contains(Node2) Then
                                        If Math.Abs(GetNodeIndexByID(NodesList, pd.FirstNodeID) - GetNodeIndexByID(NodesList, pd.SecondNodeID)) = 2 Then
                                            If IsAllPW Then
                                                PWList.Add(pd)
                                            Else
                                                pwAction = New clsAction
                                                pwAction.ActionType = ActionType.atPairwise
                                                pwAction.ActionData = pd

                                                mPipe.Add(pwAction)
                                                JudgmentsCount += 1
                                            End If
                                        End If
                                    End If
                                Next
                        End Select

                        If IsAllPW And PWList.Count > 0 Then
                            If PWList.Count > 1 Then
                                pwAction = New clsAction
                                pwAction.ActionType = ActionType.atAllPairwise
                                pwAction.ActionData = New clsAllPairwiseEvaluationActionData(ProjectManager.UserID, node, NodesList, PWList)

                                mPipe.Add(pwAction)
                                JudgmentsCount += PWList.Count
                            Else
                                pwAction = New clsAction
                                pwAction.ActionType = ActionType.atPairwise
                                pwAction.ActionData = PWList(0)

                                mPipe.Add(pwAction)
                                JudgmentsCount += 1
                            End If
                        End If
                    Case ECMeasureType.mtPWOutcomes
                        Dim IsOneAtATime As Boolean = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DISPLAY_OPTION_PAIRWISE_ID, node.NodeGuidID) = 1
                        Dim IsAllPW As Boolean = Not IsOneAtATime And Not IsSynchronousSession
                        Dim PWOS As clsRatingScale = CType(node.MeasurementScale, clsRatingScale)
                        Dim RatingsList As List(Of clsRating) = PWOS.RatingSet
                        Dim comparer As New clsPairwiseOutcomesComparer
                        comparer.RatingScale = PWOS

                        Dim DiagonalsEvaluationMode As DiagonalsEvaluation
                        If Objectives.HierarchyType = ECHierarchyType.htMeasure Then
                            DiagonalsEvaluationMode = ProjectManager.MeasureScales.GetRatingScaleDiagonalsEvaluation(CType(node.MeasurementScale, clsRatingScale))
                        Else
                            DiagonalsEvaluationMode = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATE_DIAGONALS_MODE_ID, node.NodeGuidID)
                        End If

                        If node.IsTerminalNode Then
                            If PipeParameters.ForceAllDiagonalsForAlternatives And (RatingsList.Count < PipeParameters.ForceAllDiagonalsLimitForAlternatives) Then
                                DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                            End If
                        Else
                            If PipeParameters.ForceAllDiagonals And (RatingsList.Count < PipeParameters.ForceAllDiagonalsLimit) Then
                                DiagonalsEvaluationMode = DiagonalsEvaluation.deAll
                            End If
                        End If

                        Dim R1 As clsRating
                        Dim R2 As clsRating

                        For Each child As clsNode In NodesList
                            child.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID).Sort(comparer)

                            Dim PWList As New List(Of clsPairwiseMeasureData)

                            Dim pwSequenceOriginal As New ArrayList
                            Dim pwSequenceRandom As New ArrayList

                            Select Case DiagonalsEvaluationMode
                                Case DiagonalsEvaluation.deAll
                                    ' add all diagonals
                                    Select Case PipeParameters.PairwiseMatrixEvaluationOrder
                                        Case PairwiseEvaluationOrder.peoDiagonals
                                            For i As Integer = 1 To RatingsList.Count - 1
                                                For Each pd As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID, node.NodeID)
                                                    If pd.ParentNodeID = child.NodeID Then
                                                        R1 = PWOS.GetRatingByID(pd.FirstNodeID)
                                                        R2 = PWOS.GetRatingByID(pd.SecondNodeID)

                                                        If R1 IsNot Nothing AndAlso R2 IsNot Nothing Then
                                                            If Math.Abs(GetRatingIndexByID(RatingsList, pd.FirstNodeID) - GetRatingIndexByID(RatingsList, pd.SecondNodeID)) = i Then
                                                                If IsAllPW Then
                                                                    PWList.Add(pd)
                                                                Else
                                                                    pwAction = New clsAction
                                                                    pwAction.ActionType = ActionType.atPairwiseOutcomes
                                                                    pwAction.ActionData = pd
                                                                    pwAction.ParentNode = child
                                                                    pwAction.PWONode = node

                                                                    mPipe.Add(pwAction)
                                                                    JudgmentsCount += 1
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                            Next

                                        Case PairwiseEvaluationOrder.peoRows
                                            For i As Integer = 0 To RatingsList.Count - 2
                                                For j As Integer = i + 1 To RatingsList.Count - 1
                                                    For Each pd As clsPairwiseMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID, node.NodeID)
                                                        If pd.ParentNodeID = child.NodeID Then
                                                            R1 = PWOS.GetRatingByID(pd.FirstNodeID)
                                                            R2 = PWOS.GetRatingByID(pd.SecondNodeID)

                                                            If R1 IsNot Nothing AndAlso R2 IsNot Nothing Then
                                                                If (GetRatingIndexByID(RatingsList, pd.FirstNodeID) = i) And (GetRatingIndexByID(RatingsList, pd.SecondNodeID) = j) Or
                                                                        (GetRatingIndexByID(RatingsList, pd.FirstNodeID) = j) And (GetRatingIndexByID(RatingsList, pd.SecondNodeID) = i) Then

                                                                    If IsAllPW Then
                                                                        PWList.Add(pd)
                                                                    Else
                                                                        pwAction = New clsAction
                                                                        pwAction.ActionType = ActionType.atPairwiseOutcomes
                                                                        pwAction.ActionData = pd
                                                                        pwAction.ParentNode = child
                                                                        pwAction.PWONode = node

                                                                        mPipe.Add(pwAction)
                                                                        JudgmentsCount += 1
                                                                    End If
                                                                End If
                                                            End If
                                                        End If
                                                    Next
                                                Next
                                            Next

                                        Case PairwiseEvaluationOrder.peoColumns 'C0199
                                            For i As Integer = 1 To RatingsList.Count - 1
                                                For j As Integer = 0 To i
                                                    For Each pd As clsPairwiseMeasureData In node.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID, node.NodeID)
                                                        If pd.ParentNodeID = child.NodeID Then
                                                            R1 = PWOS.GetRatingByID(pd.FirstNodeID)
                                                            R2 = PWOS.GetRatingByID(pd.SecondNodeID)

                                                            If R1 IsNot Nothing AndAlso R2 IsNot Nothing Then
                                                                If (GetRatingIndexByID(RatingsList, pd.FirstNodeID) = i) And (GetRatingIndexByID(RatingsList, pd.SecondNodeID) = j) Or
                                                                    (GetRatingIndexByID(RatingsList, pd.FirstNodeID) = j) And (GetRatingIndexByID(RatingsList, pd.SecondNodeID) = i) Then

                                                                    If IsAllPW Then
                                                                        PWList.Add(pd)
                                                                    Else
                                                                        pwAction = New clsAction
                                                                        pwAction.ActionType = ActionType.atPairwiseOutcomes
                                                                        pwAction.ActionData = pd
                                                                        pwAction.ParentNode = child
                                                                        pwAction.PWONode = node

                                                                        mPipe.Add(pwAction)
                                                                        JudgmentsCount += 1
                                                                    End If
                                                                End If
                                                            End If
                                                        End If
                                                    Next
                                                Next
                                            Next
                                    End Select

                                Case DiagonalsEvaluation.deFirst
                                    ' add first diagonal
                                    For Each pd As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID, node.NodeID)
                                        If pd.ParentNodeID = child.NodeID Then
                                            R1 = PWOS.GetRatingByID(pd.FirstNodeID)
                                            R2 = PWOS.GetRatingByID(pd.SecondNodeID)

                                            If R1 IsNot Nothing AndAlso R2 IsNot Nothing Then
                                                If Math.Abs(GetRatingIndexByID(RatingsList, pd.FirstNodeID) - GetRatingIndexByID(RatingsList, pd.SecondNodeID)) = 1 Then
                                                    If IsAllPW Then
                                                        PWList.Add(pd)
                                                    Else
                                                        pwAction = New clsAction
                                                        pwAction.ActionType = ActionType.atPairwiseOutcomes
                                                        pwAction.ActionData = pd
                                                        pwAction.ParentNode = child
                                                        pwAction.PWONode = node

                                                        mPipe.Add(pwAction)
                                                        JudgmentsCount += 1
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next

                                Case DiagonalsEvaluation.deFirstAndSecond
                                    ' add first diagonal
                                    For Each pd As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID, node.NodeID)
                                        If pd.ParentNodeID = child.NodeID Then
                                            R1 = PWOS.GetRatingByID(pd.FirstNodeID)
                                            R2 = PWOS.GetRatingByID(pd.SecondNodeID)

                                            If R1 IsNot Nothing AndAlso R2 IsNot Nothing Then
                                                If Math.Abs(GetRatingIndexByID(RatingsList, pd.FirstNodeID) - GetRatingIndexByID(RatingsList, pd.SecondNodeID)) = 1 Then
                                                    If IsAllPW Then
                                                        PWList.Add(pd)
                                                    Else
                                                        pwAction = New clsAction
                                                        pwAction.ActionType = ActionType.atPairwiseOutcomes
                                                        pwAction.ActionData = pd
                                                        pwAction.ParentNode = child
                                                        pwAction.PWONode = node

                                                        mPipe.Add(pwAction)
                                                        JudgmentsCount += 1
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next
                                    ' add second diagonal
                                    For Each pd As clsPairwiseMeasureData In child.PWOutcomesJudgments.JudgmentsFromUser(ProjectManager.UserID, node.NodeID)
                                        If pd.ParentNodeID = child.NodeID Then
                                            R1 = PWOS.GetRatingByID(pd.FirstNodeID)
                                            R2 = PWOS.GetRatingByID(pd.SecondNodeID)

                                            If R1 IsNot Nothing AndAlso R2 IsNot Nothing Then
                                                If Math.Abs(GetRatingIndexByID(RatingsList, pd.FirstNodeID) - GetRatingIndexByID(RatingsList, pd.SecondNodeID)) = 2 Then
                                                    If IsAllPW Then
                                                        PWList.Add(pd)
                                                    Else
                                                        pwAction = New clsAction
                                                        pwAction.ActionType = ActionType.atPairwiseOutcomes
                                                        pwAction.ActionData = pd
                                                        pwAction.ParentNode = child
                                                        pwAction.PWONode = node

                                                        mPipe.Add(pwAction)
                                                        JudgmentsCount += 1
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next
                            End Select

                            If IsAllPW And PWList.Count > 0 Then
                                If PWList.Count > 1 Then
                                    pwAction = New clsAction
                                    pwAction.ActionType = ActionType.atAllPairwiseOutcomes
                                    pwAction.ActionData = New clsAllPairwiseEvaluationActionData(ProjectManager.UserID, node, NodesList, PWList)
                                    pwAction.ParentNode = child
                                    pwAction.PWONode = node

                                    mPipe.Add(pwAction)
                                    JudgmentsCount += PWList.Count
                                Else
                                    pwAction = New clsAction
                                    pwAction.ActionType = ActionType.atPairwiseOutcomes
                                    pwAction.ActionData = PWList(0)
                                    pwAction.ParentNode = child
                                    pwAction.PWONode = node

                                    mPipe.Add(pwAction)
                                    JudgmentsCount += 1
                                End If
                            End If
                            If (PipeParameters.LocalResultsView <> ResultsView.rvNone) Then
                                lrAction = New clsAction
                                lrAction.ActionType = ActionType.atShowLocalResults
                                lrAction.ActionData = New clsShowLocalResultsActionData(node, PipeParameters.LocalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID, child)
                                mPipe.Add(lrAction)
                            End If
                        Next
                    Case Else
                        If IsSynchronousSession OrElse node.MeasureType = ECMeasureType.mtStep OrElse node.MeasureType = ECMeasureType.mtRegularUtilityCurve OrElse node.MeasureType = ECMeasureType.mtAdvancedUtilityCurve Then
                            ' one-at-a-time
                            For Each child As clsNode In NodesList
                                Dim childID As Integer = child.NodeID

                                Dim J As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(childID, node.NodeID, ProjectManager.UserID)

                                If J Is Nothing Then
                                    Select Case node.MeasureType
                                        Case ECMeasureType.mtRatings
                                            J = New clsRatingMeasureData(childID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            J = New clsUtilityCurveMeasureData(childID, node.NodeID, ProjectManager.UserID, Single.NaN, node.MeasurementScale, True)
                                        Case ECMeasureType.mtStep
                                            J = New clsStepMeasureData(childID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                        Case ECMeasureType.mtDirect
                                            J = New clsDirectMeasureData(childID, node.NodeID, ProjectManager.UserID, Single.NaN, True)
                                    End Select
                                    node.Judgments.AddMeasureData(J, True)
                                End If

                                If J IsNot Nothing Then
                                    Dim OneAtATimeAction As New clsOneAtATimeEvaluationActionData(node.MeasureType, ProjectManager.UserID)
                                    OneAtATimeAction.Node = node

                                    OneAtATimeAction.Judgment = J

                                    Dim action As New clsAction
                                    action.ActionType = ActionType.atNonPWOneAtATime
                                    action.ActionData = OneAtATimeAction

                                    mPipe.Add(action)
                                    JudgmentsCount += 1
                                End If
                            Next
                        Else
                            'All non-pairwise children vs 1 parent objective
                            Dim children As New List(Of clsNode)
                            Dim J As clsNonPairwiseMeasureData

                            For Each child As clsNode In NodesList
                                J = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(child.NodeID, node.NodeID, ProjectManager.UserID)
                                If Not J Is Nothing Then children.Add(child)
                            Next

                            If children.Count <> 0 Then
                                Dim AllChildrenActionData As clsAllChildrenEvaluationActionData
                                AllChildrenActionData = New clsAllChildrenEvaluationActionData(node.MeasureType, ProjectManager.UserID, node, children)

                                Dim AllChildrenAction As clsAction = New clsAction
                                AllChildrenAction.ActionType = ActionType.atNonPWAllChildren
                                AllChildrenAction.ActionData = AllChildrenActionData
                                mPipe.Add(AllChildrenAction)
                            End If
                            JudgmentsCount += children.Count
                        End If
                End Select

                If ((node.MeasureType = ECMeasureType.mtDirect) Or (node.MeasureType = ECMeasureType.mtRatings) Or ((Not ProjectManager.IsRiskProject Or ProjectManager.IsRiskProject And (((Objectives.HierarchyID <> ECHierarchyID.hidLikelihood) Or (((node.MeasureType = ECMeasureType.mtPairwise) Or (node.MeasureType = ECMeasureType.mtPWAnalogous) Or (node.MeasureType = ECMeasureType.mtPWOutcomes)) And Objectives.HierarchyID = ECHierarchyID.hidLikelihood)))))) Then
                    If (NodesList.Count > 0) And (PipeParameters.LocalResultsView <> ResultsView.rvNone) Then
                        Dim fAddLocalRes As Boolean = Not HasOnlyGoal OrElse HasOnlyGoal AndAlso Not IsSynchronousSession AndAlso (PipeParameters.GlobalResultsView = ResultsView.rvNone Or node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous)
                        'Dim fAddLocalRes As Boolean = Not HasOnlyGoal OrElse HasOnlyGoal AndAlso (PipeParameters.GlobalResultsView = ResultsView.rvNone OrElse node.MeasureType = ECMeasureType.mtPairwise OrElse node.MeasureType = ECMeasureType.mtPWOutcomes OrElse node.MeasureType = ECMeasureType.mtPWAnalogous)  ' D4545
                        If fAddLocalRes AndAlso HasOnlyGoal AndAlso PipeParameters.GlobalResultsView <> ResultsView.rvNone AndAlso node.ParentNodeID <= 0 AndAlso Not node.IsAlternative Then fAddLocalRes = False
                        If fAddLocalRes AndAlso NodesList.Count > 1 Then
                            lrAction = New clsAction
                            lrAction.ActionType = ActionType.atShowLocalResults
                            lrAction.ActionData = New clsShowLocalResultsActionData(node, PipeParameters.LocalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID) 'C0733
                            mPipe.Add(lrAction)
                        End If
                    End If
                End If
            End If

            node.PipeCreated = True

            If Recursive Then
                For Each nd As clsNode In node.Children
                    If Not nd.IsTerminalNode Then
                        AddPairwiseData(nd)
                    End If
                Next
            End If
        End Sub

        Private Sub AddObjectivePairs_FromTop()
            AddPairwiseData(Objectives.Nodes(0), True, True)
        End Sub

        Private Sub AddObjectivePairs_FromBottom()
            Dim MaxLevel As Integer = Objectives.GetMaxLevel
            Dim LevelNodes As List(Of clsNode)
            For i As Integer = MaxLevel - 1 To 0 Step -1
                LevelNodes = Objectives.GetLevelNodes(i)
                For Each node As clsNode In LevelNodes
                    If Not node.IsTerminalNode Then
                        AddPairwiseData(node, False, True)
                    End If
                Next
            Next
        End Sub

        Private Sub AddObjectivesPairs()
            If PipeParameters.ObjectivesEvalDirection = ObjectivesEvaluationDirection.oedTopToBottom Then
                AddObjectivePairs_FromTop()
            Else
                AddObjectivePairs_FromBottom()
            End If
        End Sub

        Private Function MultipleMeasurementTypesForCovObjs() As Boolean
            Dim termNodes As List(Of clsNode) = Objectives.TerminalNodes
            Dim mType As ECMeasureType
            If termNodes.Count <> 0 Then
                mType = termNodes(0).MeasureType
            Else
                Return False
            End If
            Return termNodes.Exists(Function(n) (n.MeasureType <> mType))
        End Function

        Private Sub AddAlternativesPairs() 'C0959
            If Objectives.HierarchyType = ECHierarchyType.htMeasure OrElse Alternatives.Nodes.Count = 0 Then Exit Sub
            Dim HasOnlyGoal As Boolean = Objectives.Nodes.Count = 1
            If HasOnlyGoal And Objectives.Nodes(0).PipeCreated Then Exit Sub

            Dim action As clsAction

            Dim OneAtATimeAction As clsOneAtATimeEvaluationActionData
            Dim AllAltsAction As clsAllChildrenEvaluationActionData
            Dim AllCovObjsAction As clsAllCoveringObjectivesEvaluationActionData

            Dim lrAction As clsAction ' local results action

            Dim alt As clsNode

            Dim J As clsNonPairwiseMeasureData

            Dim terminalNodes As New List(Of clsNode)
            For Each node As clsNode In Objectives.TerminalNodes
                If node.MeasureType = ECMeasureType.mtRatings Then
                    node.Judgments.JudgmentsFromUser(ProjectManager.UserID).Sort(New clsRatingMeasureDataComparer)
                End If
                terminalNodes.Add(node)
            Next

            If Not (ProjectManager.IsRiskProject And Objectives.HierarchyID = ECHierarchyID.hidLikelihood And Objectives.Nodes.Count = 1) Then
                If (IsSynchronousSession And PipeParameters.AlternativesEvalMode <> AlternativesEvaluationMode.aemOnePairAtATimeWRTAlt) Or (((PipeParameters.AlternativesEvalMode <> AlternativesEvaluationMode.aemAllCoveringObjectives) And
                    (PipeParameters.AlternativesEvalMode <> AlternativesEvaluationMode.aemOnePairAtATimeWRTAlt)) Or
                    ((PipeParameters.AlternativesEvalMode = AlternativesEvaluationMode.aemAllCoveringObjectives) And (MultipleMeasurementTypesForCovObjs() Or HasOnlyGoal))) Then

                    ' One-at-a-Time or All Alternatives vs 1 covering objective
                    For Each node As clsNode In terminalNodes
                        Dim CanEvaluateNode As Boolean
                        If Not IsSynchronousSession Then
                            CanEvaluateNode = Not node.DisabledForUser(ProjectManager.UserID) And node.IsAllowed(ProjectManager.UserID)
                        Else
                            Dim SomeUserCanEvaluate As Boolean = False
                            Dim i As Integer = 0
                            While Not SomeUserCanEvaluate And (i < TeamTimePipe.UsersList.Count)
                                SomeUserCanEvaluate = Not node.DisabledForUser(TeamTimePipe.UsersList(i).UserID) And node.IsAllowed(TeamTimePipe.UsersList(i).UserID)
                                i += 1
                            End While
                            CanEvaluateNode = SomeUserCanEvaluate
                        End If

                        Dim JudgmentsCount As Integer = 0

                        Dim alts As List(Of clsNode) = GetNodesBelowForPipe(node)
                        If CanEvaluateNode Then
                            Select Case node.MeasureType
                                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                                    AddPairwiseData(node, False, True, True)
                                Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtStep, ECMeasureType.mtDirect, ECMeasureType.mtAdvancedUtilityCurve
                                    If IsSynchronousSession Or (PipeParameters.AlternativesEvalMode <> AlternativesEvaluationMode.aemAllAlternatives) Or
                                           ((PipeParameters.AlternativesEvalMode = AlternativesEvaluationMode.aemAllAlternatives) And ((node.MeasureType = ECMeasureType.mtRegularUtilityCurve) Or (node.MeasureType = ECMeasureType.mtAdvancedUtilityCurve) Or (node.MeasureType = ECMeasureType.mtStep))) Then
                                        ' One-at-a-Time
                                        ' (all covering objectives vs 1 alt when multiple measurement types will go here as well)
                                        Dim altID As Integer
                                        For Each alt In alts
                                            altID = alt.NodeID
                                            J = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(altID, node.NodeID, ProjectManager.UserID)
                                            If J Is Nothing And IsSynchronousSession Then
                                                Select Case node.MeasureType
                                                    Case ECMeasureType.mtRatings
                                                        J = New clsRatingMeasureData(altID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                                    Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                        J = New clsUtilityCurveMeasureData(altID, node.NodeID, ProjectManager.UserID, Single.NaN, node.MeasurementScale, True)
                                                    Case ECMeasureType.mtStep
                                                        J = New clsStepMeasureData(altID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                                    Case ECMeasureType.mtDirect
                                                        J = New clsDirectMeasureData(altID, node.NodeID, ProjectManager.UserID, Single.NaN, True)
                                                End Select
                                                node.Judgments.AddMeasureData(J, True)
                                            End If

                                            If Not J Is Nothing Then
                                                Dim CanEvaluateAlt As Boolean
                                                If Not IsSynchronousSession Then
                                                    CanEvaluateAlt = ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, ProjectManager.UserID)
                                                Else
                                                    Dim SomeUserCanEvaluate As Boolean = False
                                                    Dim i As Integer = 0
                                                    While Not SomeUserCanEvaluate And (i < TeamTimePipe.UsersList.Count)
                                                        SomeUserCanEvaluate = ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, TeamTimePipe.UsersList(i).UserID)
                                                        i += 1
                                                    End While
                                                    CanEvaluateAlt = SomeUserCanEvaluate
                                                End If

                                                'If ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, ProjectManager.UserID) Or IsSynchronousSession Then
                                                If CanEvaluateAlt Then
                                                    OneAtATimeAction = New clsOneAtATimeEvaluationActionData(node.MeasureType, ProjectManager.UserID) 'C0464
                                                    OneAtATimeAction.Node = node

                                                    OneAtATimeAction.Judgment = J

                                                    OneAtATimeAction.EventType = alt.EventType

                                                    action = New clsAction
                                                    action.ActionType = ActionType.atNonPWOneAtATime
                                                    action.ActionData = OneAtATimeAction

                                                    mPipe.Add(action)
                                                    JudgmentsCount += 1
                                                End If
                                            End If
                                        Next

                                        If (PipeParameters.LocalResultsView <> ResultsView.rvNone) Then
                                            If Not HasOnlyGoal Or HasOnlyGoal And Not IsSynchronousSession And (PipeParameters.GlobalResultsView = ResultsView.rvNone Or node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                                If alts.Count > 1 Then
                                                    lrAction = New clsAction
                                                    lrAction.ActionType = ActionType.atShowLocalResults
                                                    lrAction.ActionData = New clsShowLocalResultsActionData(node, PipeParameters.LocalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID) 'C0733

                                                    mPipe.Add(lrAction)
                                                End If
                                            End If
                                        End If
                                    Else
                                        'All Alternatives vs 1 covering objective
                                        Dim alternatives As New List(Of clsNode) 'C0667

                                        Dim riskAlts As New List(Of clsNode)
                                        Dim opportunityAlts As New List(Of clsNode)

                                        For Each alt In alts 'C0450
                                            If ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, ProjectManager.UserID) Then
                                                J = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, node.NodeID, ProjectManager.UserID)

                                                If J Is Nothing And IsSynchronousSession Then
                                                    Select Case node.MeasureType
                                                        Case ECMeasureType.mtRatings
                                                            J = New clsRatingMeasureData(alt.NodeID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                            J = New clsUtilityCurveMeasureData(alt.NodeID, node.NodeID, ProjectManager.UserID, Single.NaN, node.MeasurementScale, True)
                                                        Case ECMeasureType.mtStep
                                                            J = New clsStepMeasureData(alt.NodeID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                                        Case ECMeasureType.mtDirect
                                                            J = New clsDirectMeasureData(alt.NodeID, node.NodeID, ProjectManager.UserID, Single.NaN, True)
                                                    End Select
                                                    node.Judgments.AddMeasureData(J, True)
                                                End If

                                                If Not J Is Nothing Then
                                                    If ProjectManager.IsRiskProject Then
                                                        If alt.EventType = EventType.Risk Then
                                                            riskAlts.Add(alt)
                                                        Else
                                                            opportunityAlts.Add(alt)
                                                        End If
                                                    Else
                                                        alternatives.Add(alt)
                                                    End If
                                                End If
                                            End If
                                        Next

                                        If ProjectManager.IsRiskProject Then
                                            JudgmentsCount += riskAlts.Count + opportunityAlts.Count
                                        Else
                                            JudgmentsCount += alts.Count
                                        End If

                                        If ProjectManager.IsRiskProject Then
                                            If riskAlts.Count <> 0 Then
                                                AllAltsAction = New clsAllChildrenEvaluationActionData(node.MeasureType, ProjectManager.UserID, node, riskAlts)
                                                AllAltsAction.EventType = EventType.Risk
                                                action = New clsAction
                                                action.ActionType = ActionType.atNonPWAllChildren
                                                action.ActionData = AllAltsAction
                                                mPipe.Add(action)

                                                If (PipeParameters.LocalResultsView <> ResultsView.rvNone) Then
                                                    If Not HasOnlyGoal Or HasOnlyGoal And (PipeParameters.GlobalResultsView = ResultsView.rvNone Or node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                                        If riskAlts.Count > 1 Then
                                                            lrAction = New clsAction
                                                            lrAction.ActionType = ActionType.atShowLocalResults
                                                            lrAction.ActionData = New clsShowLocalResultsActionData(node, PipeParameters.LocalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID) 'C0733

                                                            mPipe.Add(lrAction)
                                                        End If
                                                    End If
                                                End If
                                            End If
                                            If opportunityAlts.Count <> 0 Then
                                                AllAltsAction = New clsAllChildrenEvaluationActionData(node.MeasureType, ProjectManager.UserID, node, opportunityAlts)
                                                AllAltsAction.EventType = EventType.Opportunity
                                                action = New clsAction
                                                action.ActionType = ActionType.atNonPWAllChildren
                                                action.ActionData = AllAltsAction
                                                mPipe.Add(action)

                                                If (PipeParameters.LocalResultsView <> ResultsView.rvNone) Then
                                                    If Not HasOnlyGoal Or HasOnlyGoal And (PipeParameters.GlobalResultsView = ResultsView.rvNone Or node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                                        If opportunityAlts.Count > 1 Then
                                                            lrAction = New clsAction
                                                            lrAction.ActionType = ActionType.atShowLocalResults
                                                            lrAction.ActionData = New clsShowLocalResultsActionData(node, PipeParameters.LocalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID) 'C0733

                                                            mPipe.Add(lrAction)
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Else
                                            If alternatives.Count <> 0 Then
                                                AllAltsAction = New clsAllChildrenEvaluationActionData(node.MeasureType, ProjectManager.UserID, node, alternatives) 'C0464

                                                action = New clsAction
                                                action.ActionType = ActionType.atNonPWAllChildren
                                                action.ActionData = AllAltsAction
                                                mPipe.Add(action)

                                                If (PipeParameters.LocalResultsView <> ResultsView.rvNone) Then
                                                    If Not HasOnlyGoal Or HasOnlyGoal And (PipeParameters.GlobalResultsView = ResultsView.rvNone Or node.MeasureType = ECMeasureType.mtPairwise Or node.MeasureType = ECMeasureType.mtPWOutcomes Or node.MeasureType = ECMeasureType.mtPWAnalogous) Then
                                                        If alternatives.Count > 1 Then
                                                            lrAction = New clsAction
                                                            lrAction.ActionType = ActionType.atShowLocalResults
                                                            lrAction.ActionData = New clsShowLocalResultsActionData(node, PipeParameters.LocalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID) 'C0733

                                                            mPipe.Add(lrAction)
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                            End Select
                        End If
                    Next
                Else
                    Dim CovObjs As New List(Of clsNode)
                    Dim CovObjsMeasureTypes As New List(Of ECMeasureType)
                    Dim MeasureTypeExists As Boolean

                    Dim pwNodes As New List(Of clsNode)

                    For Each node As clsNode In Objectives.TerminalNodes
                        Select Case node.MeasureType
                            Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous, ECMeasureType.mtPWOutcomes
                                pwNodes.Add(node)
                            Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtStep, ECMeasureType.mtDirect, ECMeasureType.mtAdvancedUtilityCurve 'C0026
                                CovObjs.Add(node)
                                If Not CovObjsMeasureTypes.Contains(node.MeasureType) Then
                                    CovObjsMeasureTypes.Add(node.MeasureType)
                                End If
                        End Select
                    Next

                    Dim tmpObjsList As List(Of clsNode)
                    If CovObjs.Count <> 0 Then
                        For Each alt In Alternatives.TerminalNodes
                            tmpObjsList = New List(Of clsNode)
                            For Each node As clsNode In CovObjs
                                Dim AltID As Integer
                                For Each alt1 As clsNode In GetNodesBelowForPipe(node)
                                    Dim CanEvaluateAlt As Boolean
                                    If Not IsSynchronousSession Then
                                        CanEvaluateAlt = ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, ProjectManager.UserID)
                                    Else
                                        Dim SomeUserCanEvaluate As Boolean = False
                                        Dim i As Integer = 0
                                        While Not SomeUserCanEvaluate And (i < TeamTimePipe.UsersList.Count)
                                            SomeUserCanEvaluate = ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt.NodeGuidID, TeamTimePipe.UsersList(i).UserID)
                                            i += 1
                                        End While
                                        CanEvaluateAlt = SomeUserCanEvaluate
                                    End If

                                    'If ProjectManager.UsersRoles.IsAllowedAlternative(node.NodeGuidID, alt1.NodeGuidID, ProjectManager.UserID) Then
                                    If CanEvaluateAlt Then
                                        AltID = alt1.NodeID
                                        If AltID = alt.NodeID Then
                                            J = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(AltID, node.NodeID, ProjectManager.UserID)
                                            If J Is Nothing And IsSynchronousSession Then
                                                J = New clsRatingMeasureData(AltID, node.NodeID, ProjectManager.UserID, Nothing, node.MeasurementScale, True)
                                                node.Judgments.AddMeasureData(J, True)
                                            End If
                                            tmpObjsList.Add(node)
                                        End If
                                    End If
                                Next
                            Next

                            If tmpObjsList.Count <> 0 Then
                                If (PipeParameters.AlternativesEvalMode = AlternativesEvaluationMode.aemAllCoveringObjectives) And Not IsSynchronousSession And ((tmpObjsList(0).MeasureType = ECMeasureType.mtDirect) Or (tmpObjsList(0).MeasureType = ECMeasureType.mtRatings)) Then
                                    'TODO: pass here the list of MTs
                                    AllCovObjsAction = New clsAllCoveringObjectivesEvaluationActionData(CType(tmpObjsList(0), clsNode).MeasureType, ProjectManager.UserID, alt, tmpObjsList) 'C0965

                                    action = New clsAction
                                    action.ActionType = ActionType.atNonPWAllCovObjs
                                    action.ActionData = AllCovObjsAction
                                    mPipe.Add(action)
                                Else
                                    For Each CO As clsNode In tmpObjsList
                                        OneAtATimeAction = New clsOneAtATimeEvaluationActionData(CO.MeasureType, ProjectManager.UserID) 'C0464
                                        OneAtATimeAction.Node = CO
                                        OneAtATimeAction.Judgment = CType(CO.Judgments, clsNonPairwiseJudgments).GetJudgement(alt.NodeID, CO.NodeID, ProjectManager.UserID)

                                        action = New clsAction
                                        action.ActionType = ActionType.atNonPWOneAtATime
                                        action.ActionData = OneAtATimeAction

                                        mPipe.Add(action)
                                    Next
                                End If
                            End If
                        Next
                    End If

                    For Each node As clsNode In pwNodes
                        AddPairwiseData(node, False, True)
                    Next
                End If
            End If

            Dim H As clsHierarchy = Objectives
            If ProjectManager.IsRiskProject And (H.HierarchyID = ECHierarchyID.hidLikelihood) Then
                Dim AllEventsRatings As New List(Of clsNode)
                Dim AllEventsDirect As New List(Of clsNode)

                Dim RatingsCount As Integer = 0
                Dim DirectCount As Integer = 0
                For Each alt In H.GetUncontributedAlternatives
                    If alt.MeasureType = ECMeasureType.mtRatings Then RatingsCount += 1
                    If alt.MeasureType = ECMeasureType.mtDirect Then DirectCount += 1
                Next

                For Each alt In H.GetUncontributedAlternatives
                    Dim CanEvaluateNode As Boolean
                    If Not IsSynchronousSession Then
                        CanEvaluateNode = ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, ProjectManager.UserID) And Not alt.DisabledForUser(ProjectManager.UserID)
                    Else
                        Dim SomeUserCanEvaluate As Boolean = False
                        Dim i As Integer = 0
                        While Not SomeUserCanEvaluate And (i < TeamTimePipe.UsersList.Count)
                            SomeUserCanEvaluate = ProjectManager.UsersRoles.IsAllowedAlternative(H.Nodes(0).NodeGuidID, alt.NodeGuidID, TeamTimePipe.UsersList(i).UserID) And Not alt.DisabledForUser(TeamTimePipe.UsersList(i).UserID)
                            i += 1
                        End While
                        CanEvaluateNode = SomeUserCanEvaluate
                    End If

                    If CanEvaluateNode Then
                        If (alt.MeasureType = ECMeasureType.mtPWOutcomes) Then
                            AddPairwiseData(alt, False, True, True)
                        Else
                            If (PipeParameters.AlternativesEvalMode = AlternativesEvaluationMode.aemAllAlternatives) And Not IsSynchronousSession _
                                And ((alt.MeasureType = ECMeasureType.mtRatings) And (RatingsCount > 1) Or (alt.MeasureType = ECMeasureType.mtDirect) And (DirectCount > 1)) Then
                                Select Case alt.MeasureType
                                    Case ECMeasureType.mtRatings
                                        AllEventsRatings.Add(alt)
                                    Case ECMeasureType.mtDirect
                                        AllEventsDirect.Add(alt)
                                End Select
                            Else
                                OneAtATimeAction = New clsOneAtATimeEvaluationActionData(alt.MeasureType, ProjectManager.UserID)

                                If alt.MeasureType = ECMeasureType.mtRatings And alt.MeasurementScale IsNot Nothing Then
                                    alt.RatingScaleID(False) = alt.MeasurementScale.ID
                                End If

                                OneAtATimeAction.Node = alt

                                J = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, H.Nodes(0).NodeID, ProjectManager.UserID)

                                OneAtATimeAction.Judgment = J

                                action = New clsAction
                                action.ActionType = ActionType.atNonPWOneAtATime
                                action.ActionData = OneAtATimeAction

                                mPipe.Add(action)
                            End If
                        End If
                    End If
                Next
                If (PipeParameters.AlternativesEvalMode = AlternativesEvaluationMode.aemAllAlternatives) And Not IsSynchronousSession Then
                    If AllEventsRatings.Count = 1 Then
                        alt = AllEventsRatings(0)
                        OneAtATimeAction = New clsOneAtATimeEvaluationActionData(alt.MeasureType, ProjectManager.UserID)

                        If alt.MeasurementScale IsNot Nothing Then
                            alt.RatingScaleID(False) = alt.MeasurementScale.ID
                        End If

                        OneAtATimeAction.Node = alt

                        J = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, H.Nodes(0).NodeID, ProjectManager.UserID)

                        OneAtATimeAction.Judgment = J

                        action = New clsAction
                        action.ActionType = ActionType.atNonPWOneAtATime
                        action.ActionData = OneAtATimeAction

                        mPipe.Add(action)
                    Else
                        If AllEventsRatings.Count > 1 Then
                            Dim AllEventsWithNoSource As New clsAllEventsWithNoSourceEvaluationActionData(ECMeasureType.mtRatings, ProjectManager.UserID, AllEventsRatings)
                            action = New clsAction
                            action.ActionType = ActionType.atAllEventsWithNoSource
                            action.ActionData = AllEventsWithNoSource
                            mPipe.Add(action)
                        End If
                    End If

                    If AllEventsDirect.Count = 1 Then
                        alt = AllEventsDirect(0)
                        OneAtATimeAction = New clsOneAtATimeEvaluationActionData(alt.MeasureType, ProjectManager.UserID)

                        OneAtATimeAction.Node = alt

                        J = alt.DirectJudgmentsForNoCause.GetJudgement(alt.NodeID, H.Nodes(0).NodeID, ProjectManager.UserID)

                        OneAtATimeAction.Judgment = J

                        action = New clsAction
                        action.ActionType = ActionType.atNonPWOneAtATime
                        action.ActionData = OneAtATimeAction

                        mPipe.Add(action)
                    Else
                        If AllEventsDirect.Count > 1 Then
                            Dim AllEventsWithNoSource As New clsAllEventsWithNoSourceEvaluationActionData(ECMeasureType.mtDirect, ProjectManager.UserID, AllEventsDirect)  ' D3250
                            action = New clsAction
                            action.ActionType = ActionType.atAllEventsWithNoSource
                            action.ActionData = AllEventsWithNoSource
                            mPipe.Add(action)
                        End If
                    End If
                End If

                ' add events links
                For Each alt In ProjectManager.ActiveAlternatives.TerminalNodes
                    If ProjectManager.Edges.Edges.ContainsKey(alt.NodeGuidID) Then
                        For Each edge As Edge In ProjectManager.Edges.Edges(alt.NodeGuidID)
                            Dim ToEvent As clsNode = edge.ToNode
                            If ToEvent IsNot Nothing Then
                                OneAtATimeAction = New clsOneAtATimeEvaluationActionData(edge.MeasurementType, ProjectManager.UserID)
                                OneAtATimeAction.EdgeMeasurementScale = edge.MeasurementScale
                                OneAtATimeAction.IsEdge = True

                                OneAtATimeAction.Node = alt

                                J = alt.EventsJudgments.GetJudgement(ToEvent.NodeID, alt.NodeID, ProjectManager.UserID)

                                OneAtATimeAction.Judgment = J

                                action = New clsAction
                                action.ActionType = ActionType.atNonPWOneAtATime
                                action.ActionData = OneAtATimeAction

                                mPipe.Add(action)
                            End If
                        Next
                    End If
                Next
            End If
        End Sub

        Public Function GetFirstEvalPipeStepForNode(ByVal node As clsNode, Optional ByVal CurrentStep As Integer = 0) As Integer
            'current step is needed to get back to this step in case of failure
            If node Is Nothing Then Return CurrentStep

            Dim action As clsAction
            For i As Integer = 0 To Pipe.Count - 1
                action = Pipe(i)
                Select Case action.ActionType
                    Case ActionType.atPairwise
                        Dim pwd As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)
                        If pwd.ParentNodeID = node.NodeID Then
                            Return i
                        End If
                    Case ActionType.atPairwiseOutcomes
                        If action.PWONode IsNot Nothing AndAlso action.PWONode Is node Then ' D4399
                            Return i
                        End If
                        If action.ParentNode Is node Then  ' D4095
                            Return i
                        End If
                    Case ActionType.atNonPWOneAtATime
                        Dim Data As clsOneAtATimeEvaluationActionData = CType(action.ActionData, clsOneAtATimeEvaluationActionData)
                        If Data IsNot Nothing Then
                            Dim ParentNode As clsNode = Nothing
                            If Data.Assignment IsNot Nothing AndAlso Data.Control IsNot Nothing Then    ' Risk with controls
                                Select Case Data.Control.Type
                                    Case ControlType.ctCause, ControlType.ctCauseToEvent
                                        If Not Guid.Equals(Data.Assignment.ObjectiveID, Guid.Empty) Then
                                            ParentNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Data.Assignment.ObjectiveID)
                                        End If
                                    Case ControlType.ctConsequenceToEvent
                                        If Not Guid.Equals(Data.Assignment.ObjectiveID, Guid.Empty) Then
                                            ParentNode = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Data.Assignment.ObjectiveID)
                                        End If
                                    Case Else
                                        ParentNode = Data.Node
                                End Select
                            Else    ' regular
                                ParentNode = Data.Node
                            End If
                            If ParentNode Is node Then Return i
                        End If
                    Case ActionType.atNonPWAllChildren
                        If CType(action.ActionData, clsAllChildrenEvaluationActionData).ParentNode Is node Then 'C0464 'C0667
                            Return i
                        End If
                    Case ActionType.atNonPWAllCovObjs
                        If CType(action.ActionData, clsAllCoveringObjectivesEvaluationActionData).CoveringObjectives.Contains(node) Then
                            Return i
                        End If
                    Case ActionType.atAllPairwise   ' D4095
                        If CType(action.ActionData, clsAllPairwiseEvaluationActionData).ParentNode Is node Then 'C0961
                            Return i
                        End If
                    Case ActionType.atAllPairwiseOutcomes
                        If CType(action.ActionData, clsAllPairwiseEvaluationActionData).ParentNode Is node OrElse action.ParentNode Is node Then    ' D7285
                            Return i
                        End If
                    Case ActionType.atAllEventsWithNoSource
                        If CType(action.ActionData, clsAllEventsWithNoSourceEvaluationActionData).Alternatives.Contains(node) Then
                            Return i
                        End If
                End Select
            Next
            Return CurrentStep
        End Function

        Public Function AddPairToPipe(ByVal WRTNodeID As Integer, ByVal FirstNodeID As Integer, ByVal SecondNodeID As Integer, ByVal StepNumber As Integer) As Boolean 'C0966
            If mPipe.Count = 0 Or mPipe.Count < StepNumber Then
                Return False
            End If

            Dim ParentNode As clsNode = Objectives.GetNodeByID(WRTNodeID)
            If ParentNode Is Nothing Then
                Return False
            End If

            Dim pd As clsPairwiseMeasureData = CType(ParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(FirstNodeID, SecondNodeID, ProjectManager.UserID)
            If pd Is Nothing Then
                ' Return False 'C0975
                pd = New clsPairwiseMeasureData(FirstNodeID, SecondNodeID, 0, 0, ParentNode.NodeID, ProjectManager.UserID, True) 'C0975
                CType(ParentNode.Judgments, clsPairwiseJudgments).AddMeasureData(pd) 'C0975
            End If

            Dim pwAction As New clsAction
            pwAction.ActionType = ActionType.atPairwise
            pwAction.ActionData = pd
            pwAction.ParentNode = ParentNode ' D7546

            mPipe.Insert(StepNumber, pwAction)
            Return True
        End Function

        Public Function AddPairToPipePWOutcomes(Node As clsNode, PWONode As clsNode, ByVal FirstRatingID As Integer, ByVal SecondRatingID As Integer, ByVal StepNumber As Integer) As Boolean
            If mPipe.Count = 0 Or mPipe.Count < StepNumber Then
                Return False
            End If

            Dim pd As clsPairwiseMeasureData = CType(Node.PWOutcomesJudgments, clsPairwiseJudgments).PairwiseJudgment(FirstRatingID, SecondRatingID, ProjectManager.UserID)
            If pd Is Nothing Then
                pd = New clsPairwiseMeasureData(FirstRatingID, SecondRatingID, 0, 0, Node.NodeID, ProjectManager.UserID, True)
                CType(Node.PWOutcomesJudgments, clsPairwiseJudgments).AddMeasureData(pd)
            End If

            Dim pwAction As New clsAction
            pwAction.ActionType = ActionType.atPairwiseOutcomes
            pwAction.ActionData = pd
            pwAction.ParentNode = Node
            pwAction.PWONode = PWONode

            mPipe.Insert(StepNumber, pwAction)
            Return True
        End Function

        Public Sub CreatePipe(Optional ByVal IsForTTAssistant As Boolean = False)
            If PipeCreated And mHierarchyID = Objectives.HierarchyID Or PipeParameters Is Nothing Then
                Exit Sub
            End If

            ProjectManager.FeedbackOn = PipeParameters.FeedbackOn   ' D7279

            'ProjectManager.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, ProjectManager.StorageManager.ProjectLocation, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)
            'ProjectManager.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, ProjectManager.StorageManager.ProjectLocation, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID, -1)
            ProjectManager.CombinedGroups.UpdateDynamicGroups()

            ResetPipeCreatedForNodes()

            If IsSynchronousSession And Not IsSynchronousSessionEvaluator And Not IsForTTAssistant Then
                ProjectManager.StorageManager.Writer.ClearTeamTimeData()
            End If

            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                RS.Sort()
            Next

            If PipeParameters.RandomSequence Then
                mRandomSeed = Now.Millisecond
                For Each node As clsNode In Objectives.Nodes
                    If Not node.IsTerminalNode Then
                        node.GetNodesBelow(True, mRandomSeed)
                    End If
                Next
            End If

            ProjectManager.StorageManager.Reader.LoadUserData(ProjectManager.User)
            ProjectManager.AddEmptyMissingJudgments(ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, ProjectManager.User, -1, Not IsSynchronousSession)

            Dim action As clsAction
            mPipe = New List(Of clsAction)

            If Not IsSynchronousSession And PipeParameters.ShowWelcomeSurvey And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then
                Dim SurveyStepsCount As Integer = 1
                If GetSurveyInfo IsNot Nothing Then
                    Dim HasAltsSel As Boolean = False   ' D6671
                    If GetSurveyInfo(ProjectManager.StorageManager.ModelID, If(Objectives.HierarchyID = ECHierarchyID.hidLikelihood, 1, 3), SurveyStepsCount, HasAltsSel) Then  ' D6671
                        For i As Integer = 1 To SurveyStepsCount
                            action = New clsAction
                            action.ActionType = ActionType.atSpyronSurvey
                            action.ActionData = New clsSpyronSurveyAction("", If(Objectives.HierarchyID = ECHierarchyID.hidLikelihood, 1, 3), i)
                            mPipe.Add(action)
                        Next
                    End If
                    ' -D6726 // disable due to not use it in production but in COVID-19 branch only
                    '' D6671 ===
                    'If HasAltsSel Then
                    '    action = New clsAction
                    '    action.ActionType = ActionType.atEmbeddedContent
                    '    action.EmbeddedContentType = EmbeddedContentType.AlternativesRank
                    '    action.ActionData = New List(Of clsNode)
                    '    Dim attr As clsAttribute = ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID)
                    '    For Each Alt As clsNode In ProjectManager.ActiveAlternatives.Nodes
                    '        If Not Alt.DisabledForUser(ProjectManager.UserID) Then
                    '            Alt.UserRank = CInt(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, Alt.NodeGuidID, attr, ProjectManager.User.UserID))
                    '            action.ActionData.Add(Alt)
                    '        End If
                    '    Next
                    '    mPipe.Add(action)
                    'End If
                    '' D6671 ==
                End If
            End If

            ' add welcome screen
            If PipeParameters.ShowWelcomeScreen AndAlso (Objectives.HierarchyType <> ECHierarchyType.htMeasure) AndAlso (Not ProjectManager.IsRiskProject OrElse Not OPT_RISKION_JOIN_PIPE OrElse ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood) Then    ' D3253
                action = New clsAction
                action.ActionType = ActionType.atInformationPage
                action.ActionData = New clsInformationPageActionData("Welcome")
                CType(action.ActionData, clsInformationPageActionData).Text = PipeParameters.PipeMessages.GetWelcomeText(PipeMessageKind.pmkText, ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy) 'C0139 'C0464
                If ProjectParameters IsNot Nothing AndAlso ProjectParameters.EvalWelcomeSurveyFirst Then mPipe.Add(action) Else mPipe.Insert(0, action) ' D3977
            End If

            Dim CheckOwnerPermissions As Boolean = IsSynchronousSession And IsSynchronousSessionEvaluator And (SynchronousOwner IsNot Nothing)
            If CheckOwnerPermissions Then
                ProjectManager.StorageManager.Reader.LoadUserPermissions(ProjectManager.GetUserByID(SynchronousOwner.UserID))
            End If

            If Objectives.Nodes.Count <> 0 Then
                If PipeParameters.EvaluateObjectives Then
                    If PipeParameters.ModelEvalOrder = ModelEvaluationOrder.meoObjectivesFirst Then
                        AddObjectivesPairs()
                        If PipeParameters.EvaluateAlternatives Then
                            AddAlternativesPairs()
                        End If
                    Else
                        If PipeParameters.EvaluateAlternatives Then
                            AddAlternativesPairs()
                        End If
                        AddObjectivesPairs()
                    End If
                Else
                    If PipeParameters.EvaluateAlternatives Then
                        AddAlternativesPairs()
                    End If
                End If
            End If

            If ProjectManager.FeedbackOn And Not IsSynchronousSession Then
                For Each alt As clsNode In Alternatives.TerminalNodes
                    For Each CovObj As clsNode In ProjectManager.GetContributedCoveringObjectives(alt, Objectives)
                        Dim J As clsNonPairwiseMeasureData = CType(alt.FeedbackJudgments, clsNonPairwiseJudgments).GetJudgement(CovObj.NodeID, alt.NodeID, ProjectManager.UserID)

                        If Not J Is Nothing Then
                            Dim OneAtATimeAction As New clsOneAtATimeEvaluationActionData(alt.FeedbackMeasureType, ProjectManager.UserID)
                            OneAtATimeAction.Node = alt

                            OneAtATimeAction.Judgment = J

                            action = New clsAction
                            action.IsFeedback = True
                            action.ActionType = ActionType.atNonPWOneAtATime
                            action.ActionData = OneAtATimeAction

                            mPipe.Add(action)
                        End If
                    Next
                Next
            End If

            If (Alternatives.Nodes.Count <> 0) And
                (PipeParameters.GlobalResultsView <> ResultsView.rvNone) And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then
                ' if there are alts in the model, then add global results
                action = New clsAction
                ' D4587 + D4593 ===
                Dim fIsGoalOnlyWithPW As Boolean = Not ProjectManager.IsRiskProject AndAlso ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes.Count = 1 AndAlso PipeParameters.ShowConsistencyRatio
                If fIsGoalOnlyWithPW Then
                    Select Case ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0).MeasureType
                        Case ECMeasureType.mtPairwise, ECMeasureType.mtPWOutcomes, ECMeasureType.mtPWAnalogous
                        Case Else
                            fIsGoalOnlyWithPW = False
                    End Select
                End If
                If fIsGoalOnlyWithPW Then
                    ' D4593 ==
                    action.ActionType = ActionType.atShowLocalResults
                    action.ActionData = New clsShowLocalResultsActionData(ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), PipeParameters.GlobalResultsView, PipeParameters.ShowConsistencyRatio, IsSynchronousSessionEvaluator, ProjectManager.UserID)
                Else
                    ' D4587 ==
                    action.ActionType = ActionType.atShowGlobalResults
                    'action.ActionData = New clsShowGlobalResultsAction(PipeParameters.SynthesisMode, PipeParameters.GlobalResultsView, mPrjManager) 'C0100
                    action.ActionData = New clsShowGlobalResultsActionData(PipeParameters.SynthesisMode, PipeParameters.GlobalResultsView, ProjectManager, IsSynchronousSessionEvaluator) 'C0100 'C0464
                End If
                mPipe.Add(action)
            End If

            Dim sPages As String() = ProjectManager.Parameters.EvalEmbeddedContent.Trim.Split(vbNewLine)
            For Each sPage As String In sPages
                ' Store as: "{EmbedTypeID};{Enabled}" [+ NewLine + ...] Where Enabled "1" for active items;
                Dim sParams As String() = sPage.Trim.Split(CChar(";"))
                If sParams.Count > 1 Then
                    Dim Pg As Integer
                    If Integer.TryParse(sParams(0), Pg) Then
                        If ([Enum].IsDefined(GetType(EmbeddedContentType), CType(Pg, EmbeddedContentType))) Then
                            Dim Cont As EmbeddedContentType = CType(Pg, EmbeddedContentType)
                            Dim fCanAdd As Boolean = False
                            Select Case Cont
                                Case EmbeddedContentType.RiskResults
                                    fCanAdd = ProjectManager.IsRiskProject AndAlso Objectives.HierarchyID = ECHierarchyID.hidImpact
                                Case EmbeddedContentType.HeatMap    ' D6664
                                    fCanAdd = ProjectManager.IsRiskProject AndAlso Objectives.HierarchyID = ECHierarchyID.hidImpact ' D6664
                                Case Else
                                    fCanAdd = False
                            End Select
                            If fCanAdd AndAlso sParams(1) = "1" Then
                                action = New clsAction
                                action.ActionType = ActionType.atEmbeddedContent
                                action.EmbeddedContentType = Cont
                                mPipe.Add(action)
                            End If
                        End If
                    End If
                End If
            Next

            ' add sensitivity analysis
            If Not IsSynchronousSession And (Alternatives.Nodes.Count <> 0) And
                PipeParameters.ShowSensitivityAnalysis And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then
                action = New clsAction
                action.ActionType = ActionType.atSensitivityAnalysis
                action.ActionData = New clsSensitivityAnalysisActionData(ProjectManager, SAType.satDynamic, IsSynchronousSessionEvaluator) 'C0102 'C0464
                mPipe.Add(action)
            End If

            ' add performance sensitivity analysis
            If Not IsSynchronousSession And (Alternatives.Nodes.Count <> 0) And
                PipeParameters.ShowSensitivityAnalysisPerformance And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then 'C0639

                action = New clsAction
                action.ActionType = ActionType.atSensitivityAnalysis
                'action.ActionData = New clsSensitivityAnalysisAction(ProjectManager, SAType.satPerformance) 'C0102
                action.ActionData = New clsSensitivityAnalysisActionData(ProjectManager, SAType.satPerformance, IsSynchronousSessionEvaluator) 'C0102 'C0464
                mPipe.Add(action)
            End If

            ' add gradient sensitivity analysis
            If Not IsSynchronousSession And (Alternatives.Nodes.Count <> 0) And
                PipeParameters.ShowSensitivityAnalysisGradient And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then 'C0639
                action = New clsAction
                action.ActionType = ActionType.atSensitivityAnalysis
                action.ActionData = New clsSensitivityAnalysisActionData(ProjectManager, SAType.satGradient, IsSynchronousSessionEvaluator) 'C0102 'C0464
                mPipe.Add(action)
            End If
            'C0078==

            ' add thank you survey
            Dim ThankYouStep As Integer = mPipe.Count
            If Not IsSynchronousSession And PipeParameters.ShowThankYouSurvey And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then 'C0639
                Dim SurveyStepsCount As Integer = 1
                If GetSurveyInfo IsNot Nothing Then
                    Dim HasAltsSel As Boolean = False   ' D6671
                    If GetSurveyInfo(ProjectManager.StorageManager.ModelID, If(Objectives.HierarchyID = ECHierarchyID.hidLikelihood, 2, 4), SurveyStepsCount, HasAltsSel) Then  ' D4712
                        For i As Integer = 1 To SurveyStepsCount
                            action = New clsAction
                            action.ActionType = ActionType.atSpyronSurvey
                            action.ActionData = New clsSpyronSurveyAction("", If(Objectives.HierarchyID = ECHierarchyID.hidLikelihood, 2, 4), i)
                            mPipe.Add(action)
                        Next
                    End If
                End If
            End If

            ' -D4712
            '' add survey screen
            'If PipeParameters.ShowSurvey And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then
            '    action = New clsAction
            '    action.ActionType = ActionType.atSurvey
            '    action.ActionData = New clsSurveyActionData("Survey") 'C0464
            '    mPipe.Add(action)
            'End If
            ' -D4712
            '' add survey screen
            'If PipeParameters.ShowSurvey And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then
            '    action = New clsAction
            '    action.ActionType = ActionType.atSurvey
            '    action.ActionData = New clsSurveyActionData("Survey") 'C0464
            '    mPipe.Add(action)
            'End If

            ' add thank you screen
            If PipeParameters.ShowThankYouScreen AndAlso Objectives.HierarchyType <> ECHierarchyType.htMeasure AndAlso (Not ProjectManager.IsRiskProject OrElse Not OPT_RISKION_JOIN_PIPE OrElse ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact) Then    ' D3253 + D3259
                action = New clsAction
                action.ActionType = ActionType.atInformationPage
                action.ActionData = New clsInformationPageActionData("ThankYou") 'C0464
                CType(action.ActionData, clsInformationPageActionData).Text = PipeParameters.PipeMessages.GetThankYouText(PipeMessageKind.pmkText, ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy) 'C0139 'C0464
                If ProjectParameters IsNot Nothing AndAlso ProjectParameters.EvalThankYouSurveyLast Then mPipe.Insert(ThankYouStep, action) Else mPipe.Add(action) ' D3977
            End If

            mPipeUserID = ProjectManager.UserID 'C0168
            mPipeType = ecPipeType.ptAnytime    ' D2505
            mPipeCreated = True 'C0167
            mHierarchyID = Objectives.HierarchyID
        End Sub

        Public Sub CreatePipeForControls(ByRef LastJudgmentTime As DateTime, Optional fSkipSA As Boolean = False) ' D2973
            CreatePipeForControlsSorted(LastJudgmentTime)
        End Sub

        Private Sub AddControlsOfType(HierarchyID As ECHierarchyID, type As ControlType, UserID As Integer)
            Dim tNodes As New List(Of clsNode)
            For Each node As clsNode In ProjectManager.Hierarchy(HierarchyID).Nodes
                tNodes.Add(node)
            Next
            For Each node As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                tNodes.Add(node)
            Next
            For Each CovObj As clsNode In tNodes
                For Each control As clsControl In ProjectManager.Controls.EnabledControls
                    If control.Type = type And ProjectManager.ControlsRoles.IsAllowedObjective(control.ID, UserID) Then
                        For Each assignment As clsControlAssignment In control.Assignments
                            '-A1374 If isValid Then
                            If ProjectManager.Controls.IsValidControlAssignment(control, assignment) AndAlso CovObj.NodeGuidID.Equals(assignment.ObjectiveID) Then 'A1374
                                Dim action As New clsAction
                                action.ActionType = ActionType.atNonPWOneAtATime

                                Dim OneAtATimeAction As New clsOneAtATimeEvaluationActionData(assignment.MeasurementType, UserID)
                                OneAtATimeAction.Assignment = assignment
                                OneAtATimeAction.Control = control

                                Dim J As clsNonPairwiseMeasureData = assignment.Judgments.GetJudgement(assignment.ObjectiveID, UserID)
                                Select Case assignment.MeasurementType
                                    Case ECMeasureType.mtRatings
                                        Dim rs As clsRatingScale = ProjectManager.MeasureScales.GetRatingScaleByID(assignment.MeasurementScaleGuid)
                                        OneAtATimeAction.ControlMeasurementScale = rs
                                        If J Is Nothing Then
                                            J = New clsRatingMeasureData(-1, -1, UserID, Nothing, rs, True)
                                            OneAtATimeAction.ControlMeasurementScale = rs
                                            assignment.Judgments.AddMeasureData(J, True)
                                        End If
                                    Case ECMeasureType.mtRegularUtilityCurve
                                        Dim uc As clsRegularUtilityCurve = ProjectManager.MeasureScales.GetRegularUtilityCurveByID(assignment.MeasurementScaleGuid)
                                        OneAtATimeAction.ControlMeasurementScale = uc
                                        If J Is Nothing Then
                                            J = New clsUtilityCurveMeasureData(-1, -1, UserID, Single.NaN, uc, True)
                                            assignment.Judgments.AddMeasureData(J, True)
                                        End If
                                    Case ECMeasureType.mtStep
                                        Dim sf As clsStepFunction = ProjectManager.MeasureScales.GetStepFunctionByID(assignment.MeasurementScaleGuid)
                                        OneAtATimeAction.ControlMeasurementScale = sf
                                        If J Is Nothing Then
                                            J = New clsStepMeasureData(-1, -1, UserID, Nothing, sf, True)
                                            OneAtATimeAction.ControlMeasurementScale = sf
                                            assignment.Judgments.AddMeasureData(J, True)
                                        End If
                                    Case ECMeasureType.mtDirect
                                        If J Is Nothing Then
                                            J = New clsDirectMeasureData(-1, -1, UserID, Single.NaN, True)
                                            assignment.Judgments.AddMeasureData(J, True)
                                        End If
                                End Select
                                J.CtrlObjectiveID = assignment.ObjectiveID
                                J.CtrlEventID = assignment.EventID

                                OneAtATimeAction.Judgment = J

                                action.ActionData = OneAtATimeAction
                                mPipe.Add(action)
                            End If
                            'End If
                        Next
                    End If
                Next
            Next
        End Sub

        Public Function GetControlsTotalJudgmentsCount() As Integer
            Return Pipe.LongCount(Function(a) (a.ActionType = ActionType.atNonPWOneAtATime))
        End Function

        Public Sub GetControlsEvaluationProgress(UserID As Integer, ByRef MadeCount As Integer, ByRef TotalCount As Integer, ByRef LastJudgmentTime As DateTime)
            PipeCreated = False
            Dim user As clsUser = ProjectManager.GetUserByID(UserID)
            CreatePipeForControlsSorted(LastJudgmentTime, user)
            MadeCount = GetControlsMadeJudgmentsCount()
            TotalCount = GetControlsTotalJudgmentsCount()
            PipeCreated = False
        End Sub

        Public Function GetControlsMadeJudgmentsCount() As Integer
            Return Pipe.LongCount(Function(a) ((a.ActionType = ActionType.atNonPWOneAtATime) AndAlso (Not CType(CType(a.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsNonPairwiseMeasureData).IsUndefined)))
        End Function

        Public Sub CreatePipeForControlsSorted(ByRef LastJudgmentTime As DateTime, Optional User As clsUser = Nothing)
            If PipeCreated Then Exit Sub

            ProjectManager.StorageManager.Reader.LoadUserJudgmentsControls(LastJudgmentTime, User)

            For Each RS As clsRatingScale In ProjectManager.MeasureScales.RatingsScales
                RS.Sort()
            Next

            Dim action As clsAction
            mPipe = New List(Of clsAction)

            If PipeParameters.ShowWelcomeScreen And (Objectives.HierarchyType <> ECHierarchyType.htMeasure) Then
                action = New clsAction
                action.ActionType = ActionType.atInformationPage
                action.ActionData = New clsInformationPageActionData("Welcome")
                CType(action.ActionData, clsInformationPageActionData).Text = PipeParameters.PipeMessages.GetWelcomeText(PipeMessageKind.pmkText, ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy)
                mPipe.Add(action)
            End If

            Dim UserID As Integer = ProjectManager.UserID
            If User IsNot Nothing Then UserID = User.UserID

            AddControlsOfType(ECHierarchyID.hidLikelihood, ControlType.ctCause, UserID)
            AddControlsOfType(ECHierarchyID.hidLikelihood, ControlType.ctCauseToEvent, UserID)
            AddControlsOfType(ECHierarchyID.hidImpact, ControlType.ctConsequenceToEvent, UserID)

            If PipeParameters.ShowThankYouScreen And Objectives.HierarchyType <> ECHierarchyType.htMeasure Then
                action = New clsAction
                action.ActionType = ActionType.atInformationPage
                action.ActionData = New clsInformationPageActionData("ThankYou")
                CType(action.ActionData, clsInformationPageActionData).Text = PipeParameters.PipeMessages.GetThankYouText(PipeMessageKind.pmkText, ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy)
                mPipe.Add(action)
            End If

            If PipeParameters Is Nothing Then Exit Sub

            mPipeUserID = ProjectManager.UserID
            mPipeType = ecPipeType.ptRiskWithControls
            mPipeCreated = True
        End Sub

        Public Sub SaveControlJudgment(action As clsAction)
            Dim OneAtATimeActionData As clsOneAtATimeEvaluationActionData = CType(action.ActionData, clsOneAtATimeEvaluationActionData)
            OneAtATimeActionData.Assignment.Value = ProjectManager.Controls.GetCombinedEffectivenessValue(OneAtATimeActionData.Assignment.Judgments, OneAtATimeActionData.Control.ID, OneAtATimeActionData.Assignment.Value)
            With ProjectManager
                .Controls.WriteControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                .StorageManager.Writer.SaveUserJudgmentsControls(ProjectManager.User)
            End With
        End Sub

        Public ReadOnly Property Pipe() As List(Of clsAction)
            Get
                If mPipe Is Nothing Then
                    CreatePipe()
                End If
                Return mPipe
            End Get
        End Property

        Public ReadOnly Property ShowInfoDocs As Boolean
            Get
                Return If(Objectives.HierarchyType = ECHierarchyType.htMeasure, False, PipeParameters.ShowInfoDocs)
            End Get
        End Property

        Public ReadOnly Property ShowComments As Boolean
            Get
                Return If(Objectives.HierarchyType = ECHierarchyType.htMeasure, False, PipeParameters.ShowComments)
            End Get
        End Property

        Public Function GetPWNodes(Action As clsAction, pwData As clsPairwiseMeasureData, ByRef FirstNode As clsNode, ByRef SecondNode As clsNode) As Boolean
            If pwData Is Nothing Or ((Action.ActionType <> ActionType.atPairwiseOutcomes) And (Action.ActionType <> ActionType.atAllPairwiseOutcomes)) Then Return False

            Dim H As clsHierarchy = Objectives
            Dim AH As clsHierarchy = Alternatives

            Dim ParentNode As clsNode

            Select Case Action.ActionType
                Case ActionType.atPairwise, ActionType.atAllPairwise
                    ParentNode = H.GetNodeByID(pwData.ParentNodeID)
                    If ParentNode.IsTerminalNode Then
                        FirstNode = AH.GetNodeByID(pwData.FirstNodeID)
                        SecondNode = AH.GetNodeByID(pwData.SecondNodeID)
                    Else
                        FirstNode = H.GetNodeByID(pwData.FirstNodeID)
                        SecondNode = H.GetNodeByID(pwData.SecondNodeID)
                    End If
                Case ActionType.atPairwiseOutcomes, ActionType.atAllPairwiseOutcomes
                    ParentNode = Action.ParentNode
                    Dim R1 As clsRating = CType(Action.PWONode.MeasurementScale, clsRatingScale).GetRatingByID(pwData.FirstNodeID)
                    Dim R2 As clsRating = CType(Action.PWONode.MeasurementScale, clsRatingScale).GetRatingByID(pwData.SecondNodeID)

                    If R1 IsNot Nothing Then
                        FirstNode = New clsNode
                        FirstNode.NodeID = R1.ID
                        FirstNode.NodeGuidID = R1.GuidID
                        FirstNode.NodeName = ExpertChoice.Service.Double2String(R1.Value)   ' D0357
                        FirstNode.InfoDoc = ""
                        FirstNode.Comment = ""
                    Else
                        FirstNode = Nothing
                    End If

                    If R2 IsNot Nothing Then
                        SecondNode = New clsNode
                        SecondNode.NodeID = R2.ID
                        SecondNode.NodeGuidID = R2.GuidID
                        SecondNode.NodeName = ExpertChoice.Service.Double2String(R2.Value)   ' D3057
                        SecondNode.InfoDoc = ""
                        SecondNode.Comment = ""
                    Else
                        SecondNode = Nothing
                    End If
                Case Else
                    Return False
            End Select
            Return True
        End Function

        ' DA - more accurate version of GetPairwiseTypeForNode function
        'Public Function GetPairwiseTypeForNode(Node As clsNode) As PairwiseType
        '    Dim res As PairwiseType

        '    If Node Is Nothing Then Return PairwiseType.ptGraphical ' D3462

        '    Dim forceGraphical As Boolean
        '    If Node.IsTerminalNode Then
        '        res = PipeParameters.PairwiseTypeForAlternatives
        '        forceGraphical = PipeParameters.ForceGraphicalForAlternatives
        '    Else
        '        res = PipeParameters.PairwiseType
        '        forceGraphical = PipeParameters.ForceGraphical
        '    End If

        '    If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_PAIRWISE_TYPE_ID, Node.NodeGuidID) Then
        '        Dim attrValue As Object = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_PAIRWISE_TYPE_ID, Node.NodeGuidID)
        '        If attrValue IsNot Nothing Then
        '            res = CType(attrValue, PairwiseType)
        '        End If
        '    End If
            
        '    If res <> PairwiseType.ptGraphical Then    
        '        If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, Node.NodeGuidID) Then
        '            forceGraphical = CBool(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, Node.NodeGuidID))
        '        End If

        '        If forceGraphical Then
        '            Dim n As Integer
        '            If Node.MeasureType = ECMeasureType.mtPWOutcomes Then
        '                Dim RS As clsRatingScale = CType(Node.MeasurementScale, clsRatingScale)
        '                n = RS.RatingSet.Count
        '            Else
        '                n = GetNodesBelowForPipe(Node).Count
        '            End If

        '            If (n = 2 Or n = 3) AndAlso forceGraphical Then
        '                res = PairwiseType.ptGraphical
        '            End If
        '        End If
        '    End If

        '    Return res
        'End Function

        Public Function GetPairwiseTypeForNode(Node As clsNode) As PairwiseType
            Dim res As PairwiseType

            If Node Is Nothing Then Return PairwiseType.ptGraphical ' D3462

            If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_PAIRWISE_TYPE_ID, Node.NodeGuidID) Then
                Dim attrValue As Object = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_PAIRWISE_TYPE_ID, Node.NodeGuidID)
                If attrValue IsNot Nothing Then
                    res = CType(attrValue, PairwiseType)
                    Return res
                End If
            Else
                Dim forceGraphical As Boolean
                If Node.IsTerminalNode Then
                    res = PipeParameters.PairwiseTypeForAlternatives
                    forceGraphical = PipeParameters.ForceGraphicalForAlternatives
                Else
                    res = PipeParameters.PairwiseType
                    forceGraphical = PipeParameters.ForceGraphical
                End If

                If ProjectManager.Attributes.IsValueSet(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, Node.NodeGuidID) Then forceGraphical = CBool(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_PAIRWISE_FORCE_GRAPHICAL_ID, Node.NodeGuidID)) 'A1507

                Dim n As Integer
                If Node.MeasureType = ECMeasureType.mtPWOutcomes Then
                    Dim RS As clsRatingScale = CType(Node.MeasurementScale, clsRatingScale)
                    n = RS.RatingSet.Count
                Else
                    n = GetNodesBelowForPipe(Node).Count
                End If

                If (n = 2 Or n = 3) And forceGraphical Then 'A1507
                    res = PairwiseType.ptGraphical
                End If
            End If

            Return res
        End Function

        Public Function GetClusterPhraseForNode(Node As clsNode, IsMulti As Boolean, AdditionGUID As Guid, isRiskWithControls As Boolean) As String   ' D4053 + D4133
            Dim res As String = ""
            If Node.IsTerminalNode Then
                If IsMulti Then
                    res = PipeParameters.JudgementAltsPromtMulti
                Else
                    res = PipeParameters.JudgementAltsPromt
                End If
            Else
                If IsMulti Then
                    res = PipeParameters.JudgementPromtMulti
                Else
                    res = PipeParameters.JudgementPromt
                End If
            End If

            Dim AttrGUID As Guid
            If IsMulti Then
                If isRiskWithControls Then AttrGUID = ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_MULTI_ID Else AttrGUID = ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID
            Else
                If isRiskWithControls Then AttrGUID = ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_ID Else AttrGUID = ATTRIBUTE_CLUSTER_PHRASE_ID
            End If
            If ProjectManager.Attributes.GetAttributeByID(AttrGUID) IsNot Nothing Then
                Dim attrValue As Object = ProjectManager.Attributes.GetAttributeValue(AttrGUID, Node.NodeGuidID, AdditionGUID)  ' D4053 + D4329
                If attrValue IsNot Nothing Then
                    res = CType(attrValue, String)
                End If
            End If
            Return res
        End Function

        ' D7677 ===
        Public Function SetClusterPhraseForNode(NodeID As Guid, sClusterPhrase As String, IsMulti As Boolean, tAdditionlGuid As Guid, isRiskWithControls As Boolean) As Boolean   ' D2751 + D4053
            Dim fResult As Boolean = False  ' D3912
            With ProjectManager
                Dim attr As clsAttribute
                ' D4133 ===
                Dim AttrGUID As Guid
                If IsMulti Then
                    If isRiskWithControls Then AttrGUID = ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_MULTI_ID Else AttrGUID = ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID
                Else
                    If isRiskWithControls Then AttrGUID = ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_ID Else AttrGUID = ATTRIBUTE_CLUSTER_PHRASE_ID
                End If
                attr = .Attributes.GetAttributeByID(AttrGUID)
                If attr IsNot Nothing Then
                    .Attributes.SetAttributeValue(AttrGUID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, sClusterPhrase, NodeID, tAdditionlGuid) ' D4053
                    fResult = .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End If
                ' D4133 ==
            End With
            Return fResult  ' D3912
        End Function
        ' D7677 ==

        Public Function GetClusterTitleForResults(tNodeGUID As Guid, tAdditionalGUID As Guid) As String
            Dim res As String = ""
            If ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_CLUSTER_PHRASE_LOCAL_RES_ID) IsNot Nothing Then
                Dim attrValue As Object = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_LOCAL_RES_ID, tNodeGUID, tAdditionalGUID)
                If attrValue IsNot Nothing Then res = CType(attrValue, String)
            End If
            Return res
        End Function

        ' D7677 ===
        Public Function SetClusterTitleForNode(NodeID As Guid, sClusterPhrase As String, tAdditionalGUID As Guid) As Boolean
            Dim fResult As Boolean = False
            With ProjectManager
                Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CLUSTER_PHRASE_LOCAL_RES_ID)
                If attr IsNot Nothing Then
                    .Attributes.SetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_LOCAL_RES_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, sClusterPhrase, NodeID, tAdditionalGUID)
                    fResult = .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End If
            End With
            Return fResult
        End Function
        ' D7677 ==

        Public Sub ResetPipeCreatedForNodes()
            For Each node As clsNode In Objectives.Nodes
                node.PipeCreated = False
            Next
            For Each node As clsNode In Alternatives.Nodes
                node.PipeCreated = False
            Next
        End Sub

        Private Sub MakeAllNodesDirect()
            ' FOR DEBUG PURPOSES ONLY
            For Each node As clsNode In Objectives.Nodes
                node.MeasureType = ECMeasureType.mtDirect
            Next
        End Sub

        Public Function GetPipeActionNode(ByVal Action As clsAction) As clsNode
            If Action Is Nothing Then Return Nothing
            Dim ParentNode As clsNode = Nothing
            Dim FirstNode As clsNode = Nothing
            Dim SecondNode As clsNode = Nothing
            GetPipeActionNodes(Action, ParentNode, FirstNode, SecondNode)
            Return ParentNode
        End Function

        Public Function CanEditClusterPhrase(tAction As clsAction) As Boolean
            Dim fResult As Boolean = False
            If tAction IsNot Nothing Then
                fResult = tAction.isEvaluation
                If Not fResult AndAlso _OPT_CUSTOM_PHASE_NON_EVAL AndAlso Not fResult Then
                    If GetPipeActionNode(tAction) IsNot Nothing Then fResult = True
                End If
            End If
            Return fResult
        End Function

        Public Function GetPipeStepClusters(tCurStep As Integer, ByRef HasCurStepCluster As Boolean, ByRef NodeStepsList As List(Of Integer)) As List(Of clsNode)
            Dim ApplyNodes As New List(Of clsNode)
            ' D4176 ===
            Dim tCurAction As clsAction = Nothing
            If tCurStep < Pipe.Count - 1 Then tCurAction = CType(Pipe(tCurStep - 1), clsAction)
            If tCurAction IsNot Nothing Then
                ' D4176 ==
                Dim tCurNode As clsNode = GetPipeActionNode(tCurAction)
                Dim CurHelpID As ecEvaluationStepType = GetPipeActionStepType(tCurAction)
                HasCurStepCluster = False
                If tCurNode IsNot Nothing Then
                    For i As Integer = 1 To Pipe.Count
                        Dim tAction As clsAction = CType(Pipe(i - 1), clsAction)
                        Dim tNode As clsNode = Nothing
                        If i = tCurStep Then tNode = tCurNode Else tNode = GetPipeActionNode(tAction)
                        If CanEditClusterPhrase(tAction) AndAlso tNode IsNot Nothing Then
                            If tCurAction.ActionType = tAction.ActionType AndAlso (Not tAction.isEvaluation OrElse
                                (tAction.isEvaluation AndAlso (tAction.ActionData.GetType().GetProperty("Judgment") Is Nothing OrElse
                                                               tAction.ActionData.GetType().GetProperty("Judgment").GetType Is tCurAction.ActionData.GetType().GetProperty("Judgment").GetType))) Then
                                If ApplyNodes.Contains(tNode) Then
                                    If tNode Is tCurNode Then HasCurStepCluster = True
                                Else
                                    ApplyNodes.Add(tNode)
                                    NodeStepsList.Add(i)
                                End If
                            End If
                        End If
                    Next
                End If
            End If
            Return ApplyNodes
        End Function

        Public Sub GetPipeActionNodes(Action As clsAction, ByRef ParentNode As clsNode, ByRef FirstNode As clsNode, ByRef SecondNode As clsNode)
            Dim ParentGUID As Guid = Guid.Empty
            Dim ParentID As Integer = -1
            Dim sName As String = ""  ' D4111

            If Action IsNot Nothing AndAlso Action.ActionData IsNot Nothing AndAlso ProjectManager IsNot Nothing Then  ' D4041
                Dim ObjHierarchy As clsHierarchy = Objectives
                Dim AltsHierarchy As clsHierarchy = Alternatives

                Select Case Action.ActionType
                    Case ActionType.atInformationPage
                        If CType(Action.ActionData, clsInformationPageActionData).Description.ToLower = "welcome" Then
                            ParentGUID = New Guid("{AE9324D7-1836-40F5-8B9F-5CA62F6C37AA}") ' welcome
                            sName = "Welcome screen"  ' D4111
                        Else
                            ParentGUID = New Guid("{6D9ABC73-0C27-42D7-A551-301ADAD28AB6}") ' thank you
                            sName = "Thank You screen"  ' D4111
                        End If

                    Case ActionType.atShowLocalResults
                        ParentNode = CType(Action.ActionData, clsShowLocalResultsActionData).ParentNode

                    Case ActionType.atShowGlobalResults
                        ParentNode = ObjHierarchy.Nodes(0)

                    Case ActionType.atSensitivityAnalysis
                        Dim tAction As clsSensitivityAnalysisActionData = CType(Action.ActionData, clsSensitivityAnalysisActionData)
                        Select Case tAction.SAType
                            Case SAType.satDynamic
                                ParentGUID = New Guid("{E37300EB-BF4E-4D95-BB95-45E4B3675DB5}")
                                sName = "Dynamic sensitivity"  ' D4111
                            Case SAType.satGradient
                                ParentGUID = New Guid("{023A5530-6BAB-45A5-A51B-823824FF143C}")
                                sName = "Gradient Sensitivity"  ' D4111
                            Case SAType.satPerformance
                                ParentGUID = New Guid("{6B2D9EF9-8947-49A0-AA5F-27D9AB66F509}")
                                sName = "Performance Sensitivity"  ' D4111
                        End Select

                    Case ActionType.atSpyronSurvey
                        ParentGUID = New Guid("{D359E52B-DBB4-4140-860A-054210A9CFD7}")
                        sName = "Insight survey"  ' D4111

                    Case ActionType.atSurvey
                        ParentGUID = New Guid("{072C7938-2213-4B61-8D29-DCAD00B18320}")
                        sName = "Insight survey"  ' D4111

                    Case ActionType.atPairwise
                        Dim Data As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                        ParentNode = ObjHierarchy.GetNodeByID(Data.ParentNodeID)
                        Dim Hierarchy As clsHierarchy = CType(If(ParentNode.IsTerminalNode, AltsHierarchy, ObjHierarchy), clsHierarchy)
                        FirstNode = Hierarchy.GetNodeByID(Data.FirstNodeID)
                        SecondNode = Hierarchy.GetNodeByID(Data.SecondNodeID)

                    Case ActionType.atPairwiseOutcomes
                        Dim Data As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                        ParentNode = Action.ParentNode
                        If ParentNode IsNot Nothing Then GetPWNodes(Action, Data, FirstNode, SecondNode)

                    Case ActionType.atNonPWOneAtATime
                        Dim data As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)
                        If data IsNot Nothing Then

                            If data.Assignment IsNot Nothing AndAlso data.Control IsNot Nothing Then    ' Risk with controls

                                Select Case data.Control.Type
                                    Case ControlType.ctCause
                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            ParentNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                        End If
                                    Case ControlType.ctCauseToEvent
                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            ParentNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(data.Assignment.ObjectiveID)
                                        End If
                                        If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then
                                            FirstNode = AltsHierarchy.GetNodeByID(data.Assignment.EventID)
                                        End If
                                    Case ControlType.ctConsequenceToEvent
                                        If Not Guid.Equals(data.Assignment.ObjectiveID, Guid.Empty) Then
                                            ParentNode = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(data.Assignment.ObjectiveID)
                                        End If
                                        If Not Guid.Equals(data.Assignment.EventID, Guid.Empty) Then
                                            FirstNode = AltsHierarchy.GetNodeByID(data.Assignment.EventID)
                                        End If
                                End Select
                                If SecondNode Is Nothing Then
                                    SecondNode = New clsNode()
                                    SecondNode.NodeGuidID = data.Control.ID
                                    SecondNode.NodeName = data.Control.Name
                                End If
                            Else    ' regular

                                ParentNode = data.Node
                                Select Case data.MeasurementType
                                    Case ECMeasureType.mtRatings

                                        Dim tData As clsNonPairwiseMeasureData = CType(data.Judgment, clsNonPairwiseMeasureData)
                                        Dim isAlt As Boolean = data.Node IsNot Nothing AndAlso (data.Node.IsAlternative OrElse data.Node.IsTerminalNode)
                                        If isAlt Then
                                            FirstNode = AltsHierarchy.GetNodeByID(tData.NodeID)
                                        Else
                                            FirstNode = ObjHierarchy.GetNodeByID(tData.NodeID)
                                            If FirstNode Is Nothing Then
                                                FirstNode = AltsHierarchy.GetNodeByID(tData.NodeID)
                                                isAlt = True
                                            End If
                                        End If

                                    Case ECMeasureType.mtStep
                                        Dim tStep As clsStepMeasureData = CType(data.Judgment, clsStepMeasureData)
                                        If ParentNode IsNot Nothing AndAlso ParentNode.IsTerminalNode Then
                                            FirstNode = AltsHierarchy.GetNodeByID(tStep.NodeID)
                                        Else
                                            FirstNode = ObjHierarchy.GetNodeByID(tStep.NodeID)
                                        End If

                                    Case ECMeasureType.mtDirect
                                        Dim tDirect As clsDirectMeasureData = CType(data.Judgment, clsDirectMeasureData)
                                        Dim Hierarchy As clsHierarchy
                                        If Action.IsFeedback AndAlso ProjectManager.FeedbackOn Then
                                            Hierarchy = ObjHierarchy
                                        Else
                                            If data.Node.IsTerminalNode Then Hierarchy = AltsHierarchy Else Hierarchy = ObjHierarchy
                                        End If
                                        FirstNode = Hierarchy.GetNodeByID(tDirect.NodeID)

                                    Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtCustomUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                        Dim tData As clsUtilityCurveMeasureData = CType(data.Judgment, clsUtilityCurveMeasureData)
                                        ParentNode = CType(ObjHierarchy.GetNodeByID(tData.ParentNodeID), clsNode)
                                        If ParentNode.IsTerminalNode Then
                                            FirstNode = AltsHierarchy.GetNodeByID(tData.NodeID)
                                        Else
                                            FirstNode = ObjHierarchy.GetNodeByID(tData.NodeID)
                                        End If

                                End Select
                            End If
                        End If

                    Case ActionType.atNonPWAllCovObjs
                        Dim data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData) 'Cv2 'C0464
                        If Not data Is Nothing Then
                            ParentNode = data.Alternative
                        End If

                    Case ActionType.atNonPWAllChildren
                        Dim data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)
                        If data IsNot Nothing AndAlso data.ParentNode IsNot Nothing Then
                            ParentNode = data.ParentNode
                        End If

                    Case ActionType.atAllEventsWithNoSource
                        ParentNode = Action.ParentNode
                        If ParentNode Is Nothing Then
                            ParentGUID = New Guid("{C06A0C9C-C01E-41EB-BD23-3CE82A6BA361}")
                        End If

                    Case ActionType.atAllPairwise
                        ParentNode = Action.ParentNode
                        If ParentNode Is Nothing Then
                            Dim Data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
                            ParentNode = Data.ParentNode
                        End If

                        ' D7285 ===
                    Case ActionType.atAllPairwiseOutcomes
                        Dim Data As clsAllPairwiseEvaluationActionData = CType(Action.ActionData, clsAllPairwiseEvaluationActionData)
                        ParentNode = Data.ParentNode
                        If ParentNode Is Nothing Then
                            ParentNode = Action.ParentNode
                        End If
                        ' D7285 ==


                        ' D6673 ===
                    Case ActionType.atEmbeddedContent
                        Select Case Action.EmbeddedContentType
                            Case EmbeddedContentType.AlternativesRank
                                ParentGUID = New Guid("{7E9C5513-64FF-4667-8594-AF30DCB442B8}")
                                sName = "Alternatives Rank"
                        End Select
                        ' D6673 ==
                End Select
            End If

            ' Check if it's No Source object
            If ParentNode IsNot Nothing AndAlso FirstNode IsNot Nothing AndAlso FirstNode.IsAlternative AndAlso SecondNode Is Nothing Then
                'If FirstNode.isUncontributedAlternative Then
                If ProjectManager.IsRiskProject AndAlso ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetUncontributedAlternatives().Contains(FirstNode) Then 'A1457
                    ParentNode = FirstNode
                    FirstNode = Nothing
                End If
            End If

            If Not ParentGUID.Equals(Guid.Empty) AndAlso ParentNode Is Nothing Then ' for cases, when parent node can't be specified (non-evaluation steps usually)
                ParentNode = New clsNode()
                ParentNode.Tag = Action
                ParentNode.NodeGuidID = ParentGUID
                If ParentID = -1 Then ParentID = ECSecurity.ECSecurity.CRC32.ComputeAsInt(ParentGUID.ToString) ' D4079
                ParentNode.NodeID = ParentID
                ParentNode.NodeName = If(sName = "", Action.ActionType.ToString, sName)    ' D4111
                ParentNode.Comment = "Virtual node"
            End If
        End Sub

        Public Sub GetPipeActionNodeGUIDs(Action As clsAction, ByRef ParentGUID As Guid, ByRef FirstGUID As Guid, ByRef SecondGUID As Guid)
            Dim ParentNode As clsNode = Nothing
            Dim FirstNode As clsNode = Nothing
            Dim SecondNode As clsNode = Nothing

            GetPipeActionNodes(Action, ParentNode, FirstNode, SecondNode)

            If ParentNode IsNot Nothing Then ParentGUID = ParentNode.NodeGuidID Else ParentGUID = Guid.Empty
            If FirstNode IsNot Nothing Then FirstGUID = FirstNode.NodeGuidID Else FirstGUID = Guid.Empty
            If SecondNode IsNot Nothing Then SecondGUID = SecondNode.NodeGuidID Else SecondGUID = Guid.Empty
        End Sub

        Public Sub GetPipeActionNodeIDs(Action As clsAction, ByRef ParentID As Integer, ByRef FirstID As Integer, ByRef SecondID As Integer)
            Dim ParentNode As clsNode = Nothing
            Dim FirstNode As clsNode = Nothing
            Dim SecondNode As clsNode = Nothing

            GetPipeActionNodes(Action, ParentNode, FirstNode, SecondNode)

            If ParentNode IsNot Nothing Then ParentID = ParentNode.NodeID Else ParentID = -1
            If FirstNode IsNot Nothing Then FirstID = FirstNode.NodeID Else FirstID = -1
            If SecondNode IsNot Nothing Then SecondID = SecondNode.NodeID Else SecondID = -1
        End Sub

        Public Function GetPipeActionStepType(tAction As clsAction) As ecEvaluationStepType
            Dim HelpID As ecEvaluationStepType = ecEvaluationStepType.Other
            If tAction IsNot Nothing Then
                Select Case tAction.ActionType
                    Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                        Dim tNode As clsNode = Objectives.GetNodeByID(CType(tAction.ActionData, clsPairwiseMeasureData).ParentNodeID)
                        If tNode IsNot Nothing Then HelpID = If(GetPairwiseTypeForNode(tNode) = PairwiseType.ptVerbal, ecEvaluationStepType.VerbalPW, ecEvaluationStepType.GraphicalPW) ' D3693

                    Case ActionType.atNonPWOneAtATime
                        Select Case CType(tAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType
                            Case ECMeasureType.mtRatings
                                HelpID = ecEvaluationStepType.Ratings
                            Case ECMeasureType.mtAdvancedUtilityCurve, ECMeasureType.mtRegularUtilityCurve
                                HelpID = ecEvaluationStepType.UtilityCurve
                            Case ECMeasureType.mtDirect
                                HelpID = ecEvaluationStepType.DirectInput
                            Case ECMeasureType.mtStep
                                HelpID = ecEvaluationStepType.StepFunction
                        End Select

                    Case ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs
                        Select Case CType(tAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType
                            Case ECMeasureType.mtRatings    ' MultiRatings
                                HelpID = ecEvaluationStepType.MultiRatings
                            Case ECMeasureType.mtDirect     ' MultiDirect
                                HelpID = ecEvaluationStepType.MultiDirectInput
                        End Select

                        ' D1032 ===
                    Case ActionType.atSensitivityAnalysis
                        Select Case CType(tAction.ActionData, clsSensitivityAnalysisActionData).SAType
                            Case SAType.satDynamic
                                HelpID = ecEvaluationStepType.DynamicSA
                            Case SAType.satGradient
                                HelpID = ecEvaluationStepType.GradientSA
                            Case SAType.satPerformance
                                HelpID = ecEvaluationStepType.PerformanceSA
                        End Select

                    Case ActionType.atInformationPage
                        If CType(tAction.ActionData, clsInformationPageActionData).Description.ToLower = "welcome" Then HelpID = ecEvaluationStepType.Welcome Else HelpID = ecEvaluationStepType.ThankYou ' D1040

                    Case ActionType.atSpyronSurvey
                        HelpID = ecEvaluationStepType.Survey

                    Case ActionType.atShowLocalResults
                        HelpID = ecEvaluationStepType.IntermediateResults

                    Case ActionType.atShowGlobalResults
                        HelpID = ecEvaluationStepType.OverallResults
                        ' D1032 ==

                    Case ActionType.atAllPairwise, ActionType.atAllPairwiseOutcomes   ' D1155     // Multi PW
                        Dim tNode As clsNode = CType(tAction.ActionData, clsAllPairwiseEvaluationActionData).ParentNode ' D1924
                        If tNode IsNot Nothing Then HelpID = If(GetPairwiseTypeForNode(tNode) = PairwiseType.ptVerbal, ecEvaluationStepType.MultiVerbalPW, ecEvaluationStepType.MultiGraphicalPW) ' D1365 + D1924 + D6257
                        ' D3693 ==

                    Case ActionType.atAllEventsWithNoSource ' D4711
                        HelpID = ecEvaluationStepType.AllEventsWithNoSources    ' D4711

                        ' D6674 ===
                    Case ActionType.atEmbeddedContent
                        Select Case tAction.EmbeddedContentType
                            Case EmbeddedContentType.AlternativesRank
                                HelpID = ecEvaluationStepType.AlternativesRank
                                ' AD: keep it commented since the RiskResults has own links for QuickHelp view/edit; You can see duplicate QH buttons when uncommented.
                                'Case EmbeddedContentType.HeatMap
                                '    HelpID = ecEvaluationStepType.HeatMap
                                'Case EmbeddedContentType.RiskResults
                                '    HelpID = ecEvaluationStepType.RiskResults
                        End Select
                        ' D6674 ==

                End Select
            End If
            Return HelpID
        End Function

        Public Sub New(Optional ByVal ProjectManager As clsProjectManager = Nothing, Optional ByVal PipeParameters As clsPipeParamaters = Nothing, Optional ProjectParameters As clsProjectParametersWithDefaults = Nothing) 'C0100 + D3977
            Me.ProjectManager = ProjectManager
            Me.PipeParameters = PipeParameters
            Me.ProjectParameters = ProjectParameters
            mPipeCreated = False
            mPipeUserID = Integer.MinValue
            mRandomSeed = Now.Millisecond
        End Sub

    End Class

End Namespace
