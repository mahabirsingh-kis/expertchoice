Imports Canvas
Imports ECCore
Imports ExpertChoice.Service

<Serializable()> _
Public Class clsTeamTimePipe  ' D1270

    <Serializable()> _
    Private Enum PriorityViewMode
        pvmNormalized = 0
        pvmUnnormalized = 1
        pvmPercentOfNormalized = 2
        pvmPercentOfUnnormalized = 3
    End Enum

    <Serializable()> _
    Private Class clsTempResults  ' D1270
        Public Priority As Single
        Public UnnormalizedPriority As Single
        Public PriorityPercentOfNormalized As Single
        Public PriorityPercentOfUnnormalized As Single
        Public ChildNodeID As Integer
        Public WRTNodeID As Integer
        Public IsAlternativeNode As Boolean

        Public Sub New(ByVal ChildID As Integer, ByVal WRTID As Integer, ByVal IsAlternative As Boolean, ByVal PriorityValue As Single, UnnormalizedValue As Single)
            Priority = PriorityValue
            ChildNodeID = ChildID
            WRTNodeID = WRTID
            IsAlternativeNode = IsAlternative
            UnnormalizedPriority = UnnormalizedValue
        End Sub
    End Class

    Public Property ProjectManager() As clsProjectManager
    Public Property SessionOwner() As clsUser
    Public Property CurrentStep() As Integer
    Public ReadOnly Property PipeBuilder() As clsPipeBuilder
    Public Property Users() As New Dictionary(Of String, clsUser)
    Public Property AllUsers() As New Dictionary(Of String, clsUser)    ' D4825

    Public Const Judgment_Delimeter As String = vbTab
    Public Override_ResultsMode As Boolean = True   ' D1797


    Private mTempResults As New Dictionary(Of Integer, List(Of clsTempResults))
    Private mInconsistencies As New Dictionary(Of Integer, Single)

    Private mMadeJudgments As New Dictionary(Of Integer, Integer)
    Private mTotalJudgments As New Dictionary(Of Integer, Integer)
    Private mJudgmentDateTime As New Dictionary(Of Integer, Nullable(Of DateTime))    ' D2577


    Private mWRTNodeID As Integer = -1
    Public PipeBuiltLastModifyTime As Nullable(Of DateTime) = Nothing ' D4542

    Public Property WRTNodeID() As Integer
        Get
            Return mWRTNodeID
        End Get
        Set(ByVal value As Integer)
            If value <> mWRTNodeID Then
                mWRTNodeID = value
                SetCurrentStep(CurrentStep, UsersList)
            End If
        End Set
    End Property


    Public ReadOnly Property UsersList As List(Of clsUser)
        Get
            Return Users.Values.ToList
        End Get
    End Property

    Public ReadOnly Property PipeParameters() As clsPipeParamaters
        Get
            Return ProjectManager.PipeParameters
        End Get
    End Property

    Public Property ResultsViewMode As ResultsView = ResultsView.rvBoth

    Public Function GetUserEvaluatedCount(ByVal user As clsUser) As Integer
        Return If(mMadeJudgments.ContainsKey(user.UserID), mMadeJudgments(user.UserID), -1)
    End Function

    Public Function GetUserJudgmentDateTime(ByVal user As clsUser) As Nullable(Of DateTime)
        Return If(mJudgmentDateTime.ContainsKey(user.UserID), mJudgmentDateTime(user.UserID), Nothing)
    End Function

    Public Function GetUserTotalCount(ByVal user As clsUser) As Integer
        Return If(mTotalJudgments.ContainsKey(user.UserID), mTotalJudgments(user.UserID), -1)
    End Function

    Public Function GetStepAvailabilityForUser(ByVal StepType As ActionType, ByVal Action As clsAction, ByVal user As clsUser) As Boolean
        Dim res As Boolean = False
        If user IsNot Nothing AndAlso Action IsNot Nothing Then
            Select Case StepType
                Case ActionType.atPairwise
                    Dim ParentNode As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(Action.ActionData.ParentNodeID)
                    If Not ParentNode.IsAllowed(user.UserID) Then
                        res = False
                    Else
                        Dim FirstNode As clsNode
                        Dim SecondNode As clsNode
                        If ParentNode.IsTerminalNode Then
                            FirstNode = ProjectManager.ActiveAlternatives.GetNodeByID(Action.ActionData.FirstNodeID)
                            SecondNode = ProjectManager.ActiveAlternatives.GetNodeByID(Action.ActionData.SecondNodeID)
                        Else
                            FirstNode = ProjectManager.ActiveObjectives.GetNodeByID(Action.ActionData.FirstNodeID)
                            SecondNode = ProjectManager.ActiveObjectives.GetNodeByID(Action.ActionData.SecondNodeID)
                        End If

                        Dim nodesBelow As List(Of clsNode) = ParentNode.GetNodesBelow(user.UserID)

                        res = nodesBelow.Contains(FirstNode) AndAlso nodesBelow.Contains(SecondNode) ' And Not FirstNode.DisabledForUser(user.UserID) And Not SecondNode.DisabledForUser(user.UserID)
                    End If
                Case ActionType.atPairwiseOutcomes
                    Dim ParentNode As clsNode = Action.PWONode
                    Return ParentNode.IsAllowed(user.UserID)
                Case ActionType.atNonPWOneAtATime
                    Dim ParentNode As clsNode
                    Dim ChildNode As clsNode

                    Dim nonpwData As clsNonPairwiseMeasureData = CType(Action.ActionData.Judgment, clsNonPairwiseMeasureData)

                    If Action.ActionData.Node.IsAlternative Then
                        ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
                        ChildNode = Action.ActionData.Node
                    Else
                        ParentNode = Action.ActionData.Node
                        If ParentNode.IsTerminalNode Then
                            ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                        Else
                            ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(nonpwData.NodeID)
                        End If
                    End If

                    If Not Action.ActionData.Node.IsAlternative AndAlso (Not ParentNode.IsAllowed(user.UserID) OrElse ParentNode.DisabledForUser(user.UserID)) Then
                        res = False
                    Else
                        If Action.ActionData.Node.IsAlternative OrElse ParentNode.IsTerminalNode Then
                            res = ProjectManager.UsersRoles.IsAllowedAlternative(ParentNode.NodeGuidID, ChildNode.NodeGuidID, user.UserID) AndAlso Not ChildNode.DisabledForUser(user.UserID)
                        Else
                            Return True
                        End If
                    End If
                Case ActionType.atShowLocalResults
                    res = CType(Action.ActionData, clsShowLocalResultsActionData).ParentNode.IsAllowed(user.UserID)
                Case ActionType.atShowGlobalResults
                    Dim tNode As clsNode = CType(Action.ActionData, clsShowGlobalResultsActionData).WRTNode
                    If tNode Is Nothing Then tNode = ProjectManager.ActiveObjectives.Nodes(0)
                    If tNode IsNot Nothing Then res = tNode.IsAllowed(user.UserID)
                Case Else
                    res = True
            End Select
        End If

        Return res
    End Function

    Private Function GetInconsistency(ByVal UserID As Integer) As Single
        Return If(mInconsistencies.ContainsKey(UserID), mInconsistencies(UserID), -1)
    End Function

    Private Function GetPriority(ByVal UserID As Integer, ByVal ChildNodeID As Integer, ByVal WRTNodeID As Integer, ByVal IsAlternative As Boolean, ViewMode As PriorityViewMode) As Single
        If mTempResults.ContainsKey(UserID) Then
            For Each res As clsTempResults In mTempResults.Item(UserID)
                If res.ChildNodeID = ChildNodeID And res.WRTNodeID = WRTNodeID And res.IsAlternativeNode = IsAlternative Then
                    Select Case ViewMode
                        Case PriorityViewMode.pvmNormalized
                            Return res.Priority
                        Case PriorityViewMode.pvmUnnormalized
                            Return res.UnnormalizedPriority
                        Case PriorityViewMode.pvmPercentOfNormalized
                            Return res.PriorityPercentOfNormalized
                        Case PriorityViewMode.pvmPercentOfUnnormalized
                            Return res.PriorityPercentOfUnnormalized
                    End Select
                End If
            Next
        End If
        Return -1
    End Function
    Public Function GetJSON_UsersList(ByVal tSessionUsers As List(Of clsUser)) As String
        VerifyUsers(tSessionUsers)
        Dim UsersString As String = ""  ' D1286
        For Each kvp As KeyValuePair(Of String, clsUser) In Users
            Dim UserStatus As Integer
            Select Case kvp.Value.SyncEvaluationMode
                Case SynchronousEvaluationMode.semVotingBox
                    UserStatus = kvp.Value.VotingBoxID
                Case SynchronousEvaluationMode.semOnline
                    UserStatus = 0
                Case Else
                    UserStatus = -1
            End Select
            UsersString += If(UsersString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4}]", kvp.Value.UserID, JS_SafeString(kvp.Value.UserEMail), JS_SafeString(kvp.Value.UserName), If(kvp.Value.Active, 1, 0), UserStatus)
        Next
        Return UsersString
    End Function
    Private Function GetJSON_LocalResults(ByVal Action As clsAction) As String 'C1002
        Dim ActionData As clsShowLocalResultsActionData = Action.ActionData
        Dim RowsString As String = ""
        Dim UsersString As String = ""  ' D1286
        Dim nodes As List(Of clsNode)

        If ActionData.ParentNode.IsAlternative AndAlso ProjectManager.ActiveObjectives.GetUncontributedAlternatives.Contains(ActionData.ParentNode) Then
            nodes = New List(Of clsNode)
            nodes.Add(ActionData.ParentNode)
        Else
            nodes = ActionData.ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
        End If

        For Each kvp As KeyValuePair(Of String, clsUser) In Users
            Dim incon As Single = -1
            If ActionData.ParentNode.MeasureType = ECMeasureType.mtPairwise AndAlso PipeParameters.ShowConsistencyRatio Then ' if pw    ' D1339
                incon = GetInconsistency(kvp.Value.UserID)
            End If

            Dim UserStatus As Integer
            If Not GetStepAvailabilityForUser(ActionType.atShowLocalResults, Action, kvp.Value) Then
                UserStatus = -2
            Else
                Select Case kvp.Value.SyncEvaluationMode
                    Case SynchronousEvaluationMode.semVotingBox
                        UserStatus = kvp.Value.VotingBoxID
                    Case SynchronousEvaluationMode.semOnline
                        UserStatus = 0
                    Case Else
                        UserStatus = -1
                End Select
            End If

            UsersString += If(UsersString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4},{5}]", kvp.Value.UserID, JS_SafeString(kvp.Value.UserEMail), JS_SafeString(kvp.Value.UserName), If(kvp.Value.Active, 1, 0), UserStatus, JS_SafeNumber(incon)) 'C1007 + D2585
        Next

        For i As Integer = 0 To nodes.Count - 1
            Dim node As clsNode = nodes(i)

            Dim PrtyString As String = ""
            Dim InconString As String = ""

            For Each kvp As KeyValuePair(Of String, clsUser) In Users
                Dim indivPrtyNormalized As Single = GetPriority(kvp.Value.UserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmNormalized)
                Dim indivPrtyUnnormalized As Single = GetPriority(kvp.Value.UserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmUnnormalized)
                Dim indivPrtyPercentOfNormalized As Single = GetPriority(kvp.Value.UserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmPercentOfNormalized)
                Dim indivPrtyPercentOfUnnormalized As Single = GetPriority(kvp.Value.UserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmPercentOfUnnormalized)
                PrtyString += String.Format("[{0},{1},{2},{3},{4}],", kvp.Value.UserID, JS_SafeNumber(indivPrtyNormalized), JS_SafeNumber(indivPrtyUnnormalized), JS_SafeNumber(indivPrtyPercentOfNormalized), JS_SafeNumber(indivPrtyPercentOfUnnormalized))
            Next

            Dim combinedPrtyNormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmNormalized)
            Dim combinedPrtyUnnormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmUnnormalized)
            Dim combinedPrtyPercentOfNormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmPercentOfNormalized)
            Dim combinedPrtyPercentOfUnnormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, node.NodeID, ActionData.ParentNode.NodeID, ActionData.ParentNode.IsTerminalNode, PriorityViewMode.pvmPercentOfUnnormalized)
            PrtyString += String.Format("[{0},{1},{2},{3},{4}]", ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, JS_SafeNumber(combinedPrtyNormalized), JS_SafeNumber(combinedPrtyUnnormalized), JS_SafeNumber(combinedPrtyPercentOfNormalized), JS_SafeNumber(combinedPrtyPercentOfUnnormalized))

            RowsString += If(RowsString = "", "", ",") + String.Format("[{0},'{1}',[{2}]]", i + 1, JS_SafeString(node.NodeName), PrtyString)
        Next

        Dim res As String = String.Format("['{0}',[{1}],[{2}],[{3},{4}],{5}]", JS_SafeString(ActionData.ParentNode.NodeName), UsersString, RowsString, CInt(If(Override_ResultsMode, ResultsViewMode, PipeParameters.LocalResultsView)), CInt(PipeParameters.LocalResultsSortMode), Bool2Num(ActionData.ParentNode.IsTerminalNode))    ' D1270 + D1282 'C1002 + D1286 + D1797 + D3810
        Return res
    End Function
    Private Function GetJSON_GlobalResults(ByVal Action As clsAction, ByVal WRTNodeID As Integer) As String 'C1002
        Dim ActionData As clsShowGlobalResultsActionData = Action.ActionData
        Dim RowsString As String = ""
        Dim UsersString As String = ""  ' D1287

        For Each kvp As KeyValuePair(Of String, clsUser) In Users
            Dim UserStatus As Integer
            If Not GetStepAvailabilityForUser(ActionType.atShowGlobalResults, Action, kvp.Value) Then
                UserStatus = -2
            Else
                Select Case kvp.Value.SyncEvaluationMode
                    Case SynchronousEvaluationMode.semVotingBox
                        UserStatus = kvp.Value.VotingBoxID
                    Case SynchronousEvaluationMode.semOnline
                        UserStatus = 0
                    Case Else
                        UserStatus = -1
                End Select
            End If

            UsersString += If(UsersString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4}]", kvp.Value.UserID, JS_SafeString(kvp.Value.UserEMail), JS_SafeString(kvp.Value.UserName), If(kvp.Value.Active, 1, 0), UserStatus) 'C1007 + D2585
        Next

        Dim WRTNode As clsNode
        If WRTNodeID = -1 Then
            WRTNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)
        Else
            WRTNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(WRTNodeID)
        End If

        Dim Alts As List(Of clsNode) = ProjectManager.UsersRoles.GetAllowedAlternatives(UNDEFINED_USER_ID, WRTNode)

        For i As Integer = 0 To Alts.Count - 1
            Dim alt As clsNode = Alts(i)

            Dim PrtyString As String = ""
            For Each kvp As KeyValuePair(Of String, clsUser) In Users
                Dim indivPrtyNormalized As Single = GetPriority(kvp.Value.UserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmNormalized)
                Dim indivPrtyUnormalized As Single = GetPriority(kvp.Value.UserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmUnnormalized)
                Dim indivPrtyPercentOfNormalized As Single = GetPriority(kvp.Value.UserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmPercentOfNormalized)
                Dim indivPrtyPercentOfUnnormalized As Single = GetPriority(kvp.Value.UserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmPercentOfUnnormalized)
                PrtyString += String.Format("[{0},{1},{2},{3},{4},{5}],", kvp.Value.UserID, If(indivPrtyNormalized = -1, 0, 1), JS_SafeNumber(indivPrtyNormalized), JS_SafeNumber(indivPrtyUnormalized), JS_SafeNumber(indivPrtyPercentOfNormalized), JS_SafeNumber(indivPrtyPercentOfUnnormalized))   ' D3030
            Next

            Dim combinedPrtyNormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmNormalized)
            Dim combinedPrtyUnnormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmUnnormalized) '/ 100    ' D4054
            Dim combinedPrtyPercentOfNormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmPercentOfNormalized)
            Dim combinedPrtyPercentOfUnnormalized As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmPercentOfUnnormalized)
            PrtyString += String.Format("[{0},{1},{2},{3},{4},{5}]", ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, If(combinedPrtyNormalized = -1, 0, 1), JS_SafeNumber(combinedPrtyNormalized), JS_SafeNumber(combinedPrtyUnnormalized), JS_SafeNumber(combinedPrtyPercentOfNormalized), JS_SafeNumber(combinedPrtyPercentOfUnnormalized))    ' D3030

            RowsString += If(RowsString = "", "", ",") + String.Format("[{0},'{1}',[{2}]]", i + 1, JS_SafeString(alt.NodeName), PrtyString)   ' D1287
        Next

        Dim CalcTarget As New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)   ' D3873
        ProjectManager.CalculationsManager.Calculate(CalcTarget, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)) ' D873
        Dim NodesString As String = GetNodesList(ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), "") ' D3483

        Dim res As String = String.Format("[{0},[{1}],[{2}],[{3}],[{4},{5}],{6}]", WRTNode.NodeID, UsersString, RowsString, NodesString, CInt(If(Override_ResultsMode, ResultsViewMode, PipeParameters.GlobalResultsView)), CInt(PipeParameters.GlobalResultsSortMode), Bool2Num(WRTNode.IsTerminalNode)) ' D1270 'C1002 + D1287 + D1797 + D3810
        Return res
    End Function
    Private Function GetNodesList(WRTNode As clsNode, sPID As String) As String
        Dim sNodes As String = ""
        If WRTNode IsNot Nothing Then
            Dim sPath = If(sPID = "", "n", sPID + "_") + WRTNode.NodeID.ToString
            ' D3873 ===
            Dim Prty As Double = -1
            If ProjectManager.IsRiskProject AndAlso ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso WRTNode.RiskNodeType = RiskNodeType.ntCategory Then
                ' no need to show prty (Case 6524)
            Else
                Prty = WRTNode.LocalPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID)
            End If
            sNodes = String.Format("[{0},{1},'{2}','{3}','{4}',{5}]", WRTNode.NodeID, WRTNode.ParentNodeID, JS_SafeString(WRTNode.NodeName), sPath, sPID, JS_SafeNumber(Prty))
            ' D3873 ==
            Dim nodes As List(Of clsNode) = WRTNode.GetNodesBelow(UNDEFINED_USER_ID)
            For i As Integer = 0 To nodes.Count - 1
                Dim tNode As clsNode = nodes(i)
                If (Not tNode.IsAlternative) Then
                    sNodes += If(sNodes = "", "", ",") + GetNodesList(tNode, sPath)
                End If
            Next
        End If
        Return sNodes
    End Function

    Private Function GetJSON_DSA(ByVal Action As clsAction, ByVal WRTNodeID As Integer) As String 'C1002 + D1414
        Dim ActionData As clsSensitivityAnalysisActionData = Action.ActionData

        Dim WRTNode As clsNode
        If WRTNodeID = -1 Then
            WRTNode = ProjectManager.ActiveObjectives.Nodes(0)
        Else
            WRTNode = ProjectManager.ActiveObjectives.GetNodeByID(WRTNodeID)
        End If

        Dim Alts As List(Of clsNode) = ProjectManager.UsersRoles.GetAllowedAlternatives(UNDEFINED_USER_ID, WRTNode)

        Dim RowsString As String = ""
        For i As Integer = 0 To Alts.Count - 1
            Dim alt As clsNode = Alts(i)

            Dim combinedPrty As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, alt.NodeID, WRTNode.NodeID, True, PriorityViewMode.pvmNormalized)

            RowsString += If(RowsString = "", "", ",") + String.Format("[{0},'{1}',{2}]", alt.NodeID, JS_SafeString(alt.NodeName), JS_SafeNumber(combinedPrty))
        Next

        Dim NodesString As String = ""
        Dim nodes As List(Of clsNode) = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes
        For i As Integer = 0 To nodes.Count - 1
            Dim node As clsNode = nodes(i)

            Dim combinedPrty As Single = GetPriority(ProjectManager.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID, node.NodeID, node.ParentNodeID, False, PriorityViewMode.pvmNormalized)

            NodesString += If(NodesString = "", "", ",") + String.Format("[{0},{1},'{2}',{3},{4}]", node.NodeID, node.ParentNodeID, JS_SafeString(node.NodeName), JS_SafeNumber(combinedPrty), JS_SafeNumber(combinedPrty))
        Next

        Dim res As String = String.Format("[{0},{1},[{2}],[{3}]]", 1, WRTNode.NodeID, RowsString, NodesString)
        Return res
    End Function

    Public Function SetDSAPriorities(WRTNodeID As Integer, Priorities As Dictionary(Of Integer, Single)) As Boolean
        Return True
    End Function

    Private Function GetJSON_Pairwise(ByVal Action As clsAction) As String
        Dim ActionData As clsPairwiseMeasureData = Action.ActionData

        Dim NodesString As String = ""

        Dim ParentNode As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(ActionData.ParentNodeID)
        Dim FirstNode As clsNode
        Dim SecondNode As clsNode
        If ParentNode.IsTerminalNode Then
            FirstNode = ProjectManager.ActiveAlternatives.GetNodeByID(ActionData.FirstNodeID)
            SecondNode = ProjectManager.ActiveAlternatives.GetNodeByID(ActionData.SecondNodeID)
        Else
            FirstNode = ProjectManager.ActiveObjectives.GetNodeByID(ActionData.FirstNodeID)
            SecondNode = ProjectManager.ActiveObjectives.GetNodeByID(ActionData.SecondNodeID)
        End If

        TeamTimeFuncs.GPW_Mode_Allowed = ParentNode.Hierarchy.ProjectManager.IsRiskProject
        TeamTimeFuncs.GPW_Mode_Strict = ParentNode.Hierarchy.ProjectManager.IsRiskProject

        Dim PWType As PairwiseType = ProjectManager.PipeBuilder.GetPairwiseTypeForNode(ParentNode) ' D2734
        If PWType = PairwiseType.ptGraphical Then
            Dim Mode As GraphicalPairwiseMode = TeamTimeFuncs.GPW_Mode_Default
            If TeamTimeFuncs.GPW_Mode_Allowed Then Mode = CType(If(ParentNode.IsTerminalNode, PipeParameters.GraphicalPWModeForAlternatives, PipeParameters.GraphicalPWMode), GraphicalPairwiseMode)
            Select Case Mode
                Case GraphicalPairwiseMode.gpwmLessThan9
                    PWType = 9
                Case GraphicalPairwiseMode.gpwmLessThan99
                    PWType = 99
                Case Else
                    PWType = PairwiseType.ptGraphical
            End Select
        End If

        Dim KnownLikelihood_A As Double = -1
        Dim KnownLikelihood_B As Double = -1

        If ParentNode IsNot Nothing AndAlso ProjectManager.IsRiskProject AndAlso ParentNode.MeasureType = ECMeasureType.mtPWAnalogous Then
            Dim L As List(Of KnownLikelihoodDataContract) = ParentNode.GetKnownLikelihoods()
            If L IsNot Nothing Then
                For Each tLikelihood As KnownLikelihoodDataContract In L
                    If tLikelihood.Value >= 0 Then
                        If tLikelihood.ID = ActionData.FirstNodeID Then KnownLikelihood_A = tLikelihood.Value
                        If tLikelihood.ID = ActionData.SecondNodeID Then KnownLikelihood_B = tLikelihood.Value
                    End If
                Next
            End If
        End If

        NodesString += String.Format("[{0},'{1}','{2}',{3},{4},0,''],", ParentNode.NodeID, ParentNode.NodeGuidID.ToString, JS_SafeString(ParentNode.NodeName), If(ParentNode.InfoDoc <> "", 1, 0), If(ParentNode.IsAlternative, 1, 0))  ' D1298 + D1301
        NodesString += String.Format("[{0},'{1}','{2}',{3},{4},{5},'{6}'],", FirstNode.NodeID, FirstNode.NodeGuidID.ToString, JS_SafeString(FirstNode.NodeName), If(FirstNode.InfoDoc <> "", 1, 0), If(FirstNode.IsAlternative, 1, 0), If(String.IsNullOrEmpty(ProjectManager.InfoDocs.GetNodeWRTInfoDoc(FirstNode.NodeGuidID, ParentNode.NodeGuidID)), 0, 1), If(KnownLikelihood_A >= 0, "%%known%% " + JS_SafeNumber(KnownLikelihood_A), ""))    ' D1298 + D1301
        NodesString += String.Format("[{0},'{1}','{2}',{3},{4},{5},'{6}']", SecondNode.NodeID, SecondNode.NodeGuidID.ToString, JS_SafeString(SecondNode.NodeName), If(SecondNode.InfoDoc <> "", 1, 0), If(SecondNode.IsAlternative, 1, 0), If(String.IsNullOrEmpty(ProjectManager.InfoDocs.GetNodeWRTInfoDoc(SecondNode.NodeGuidID, ParentNode.NodeGuidID)), 0, 1), If(KnownLikelihood_B >= 0, "%%known%% " + JS_SafeNumber(KnownLikelihood_B), ""))   ' D1298 + D1301

        Dim JudgmentsString As String = ""
        Dim JudgmentsList As New ArrayList

        Dim ListOfValues As New List(Of Double) 'C1009 + D1903

        For Each kvp As KeyValuePair(Of String, clsUser) In Users
            Dim user As clsUser = kvp.Value

            Dim pwData As clsPairwiseMeasureData = CType(ParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(FirstNode.NodeID, SecondNode.NodeID, user.UserID)
            If pwData IsNot Nothing Then
                pwData = New clsPairwiseMeasureData(pwData.FirstNodeID, pwData.SecondNodeID, pwData.Advantage, pwData.Value, pwData.ParentNodeID, pwData.UserID, pwData.IsUndefined, pwData.Comment)
                If PWType = PairwiseType.ptVerbal AndAlso pwData.Value > 9 Then pwData.Value = 9
            End If
            Dim value As Double ' D1903
            Dim comment As String = ""  ' D1281
            If pwData Is Nothing Then mJudgmentDateTime(user.UserID) = Nothing Else mJudgmentDateTime(user.UserID) = pwData.ModifyDate ' D2577

            If pwData Is Nothing OrElse pwData.IsUndefined Then
                value = TeamTimeFuncs.UndefinedValue ' D1903
            Else
                value = If(pwData.Value = 1, 0, -pwData.Advantage * (pwData.Value - 1))
            End If
            If pwData IsNot Nothing Then comment = pwData.Comment ' D1281 + D2757

            Dim UserStatus As Integer
            If Not GetStepAvailabilityForUser(ActionType.atPairwise, Action, user) Then 'And (user.UserEMail <> SessionOwner.UserEMail) Then ' TODO: if restricted 'C1007
                UserStatus = -2
            Else
                Select Case user.SyncEvaluationMode
                    Case SynchronousEvaluationMode.semVotingBox
                        UserStatus = user.VotingBoxID
                    Case SynchronousEvaluationMode.semOnline
                        UserStatus = 0
                    Case Else
                        UserStatus = -1
                End Select
                Dim fCanAdd As Boolean = UserStatus <> -1 OrElse PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess
                If fCanAdd Then JudgmentsList.Add(pwData)
                If pwData IsNot Nothing AndAlso Not pwData.IsUndefined AndAlso fCanAdd Then
                    ' D2605 ==
                    ListOfValues.Add(If(pwData.Advantage > 0, pwData.Value, 1 / pwData.Value))
                End If
            End If

            JudgmentsString += If(JudgmentsString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4},{5},'{6}']", user.UserID, JS_SafeString(user.UserEMail), JS_SafeString(user.UserName), If(user.Active, 1, 0), UserStatus, JS_SafeNumber(value), JS_SafeString(comment))    ' D1270
        Next

        ' group result
        Dim adv As Single
        Dim GroupResult As Double = TeamTimeFuncs.CombinePairwiseJudgments(JudgmentsList, adv)
        GroupResult = If(GroupResult = Single.NaN OrElse GroupResult = TeamTimeFuncs.UndefinedValue, TeamTimeFuncs.UndefinedValue, If(GroupResult = 1, 0, -adv * (GroupResult - 1))) 'C1002 + D1903 + D1909

        Dim ShowComment As Integer = If(PipeParameters.ShowComments, 1, 0)

        Dim Consensus As Single = 0 'C1003

        If ListOfValues.Count > 1 Then
            Dim variance As Double = 1  ' D1903
            Dim MaxValue As Double = 9  ' D1903
            For Each value As Double In ListOfValues    ' D1903
                If (If(value < 1, 1 / value, value)) > MaxValue Then
                    MaxValue = If(value < 1, 1 / value, value)
                End If
                variance *= value
            Next
            variance = Math.Pow(variance, 1 / ListOfValues.Count)
            For i As Integer = 0 To ListOfValues.Count - 1
                ListOfValues(i) /= variance
                If ListOfValues(i) < 1 Then
                    ListOfValues(i) = 1 / ListOfValues(i)
                End If
            Next
            variance = 1
            For Each value As Double In ListOfValues    ' D1903
                variance *= value
            Next
            If ListOfValues.Count > 0 Then variance = Math.Pow(variance, 1 / ListOfValues.Count) ' D1594
            If Math.Log(MaxValue) = 0 Then variance = 0 Else variance = Math.Log(variance) / Math.Log(MaxValue) ' D1594

            Consensus = variance
        End If

        Dim res As String = String.Format("[[{0}],[{1}],'{2}','{3}',{4}]", NodesString, JudgmentsString, JS_SafeNumber(GroupResult), JS_SafeNumber(Consensus), CInt(PWType))  ' D1270 'C1003 + D1478 + D1908
        Return res
    End Function

    Private Function GetJSON_PairwiseOutcomes(ByVal Action As clsAction) As String
        Dim ActionData As clsPairwiseMeasureData = Action.ActionData

        Dim NodesString As String = ""

        Dim ParentNode As clsNode = CType(Pipe(CurrentStep), clsAction).ParentNode
        Dim RS As clsRatingScale = CType(Pipe(CurrentStep), clsAction).PWONode.MeasurementScale
        Dim FirstRating As clsRating = RS.GetRatingByID(ActionData.FirstNodeID)
        Dim SecondRating As clsRating = RS.GetRatingByID(ActionData.SecondNodeID)

        TeamTimeFuncs.GPW_Mode_Allowed = ParentNode.Hierarchy.ProjectManager.IsRiskProject
        TeamTimeFuncs.GPW_Mode_Strict = ParentNode.Hierarchy.ProjectManager.IsRiskProject

        Dim PWType As PairwiseType = If(ParentNode.IsTerminalNode, PipeParameters.PairwiseTypeForAlternatives, PipeParameters.PairwiseType)
        If ((ParentNode.IsTerminalNode AndAlso PipeParameters.ForceGraphicalForAlternatives) Or (Not ParentNode.IsTerminalNode AndAlso PipeParameters.ForceGraphical)) Then
            If RS.RatingSet.Count > 0 AndAlso RS.RatingSet.Count <= 3 Then PWType = PairwiseType.ptGraphical
        End If

        If PWType = PairwiseType.ptGraphical Then
            Dim Mode As GraphicalPairwiseMode = TeamTimeFuncs.GPW_Mode_Default
            If TeamTimeFuncs.GPW_Mode_Allowed Then Mode = CType(If(ParentNode.IsTerminalNode, PipeParameters.GraphicalPWModeForAlternatives, PipeParameters.GraphicalPWMode), GraphicalPairwiseMode)
            Select Case Mode
                Case GraphicalPairwiseMode.gpwmLessThan9
                    PWType = 9
                Case GraphicalPairwiseMode.gpwmLessThan99
                    PWType = 99
                Case Else
                    PWType = PairwiseType.ptGraphical
            End Select
        End If

        ' D2999 ===
        Dim sNodeCommentA As String = ""
        Dim sNodeCommentB As String = ""
        If FirstRating IsNot Nothing Then sNodeCommentA = FirstRating.Comment
        If SecondRating IsNot Nothing Then sNodeCommentB = SecondRating.Comment

        NodesString += String.Format("[{0},'{1}','{2}',-1,{4},-1,''],", ParentNode.NodeID, ParentNode.NodeGuidID.ToString, JS_SafeString(ParentNode.NodeName), If(ParentNode.InfoDoc <> "", 1, 0), If(ParentNode.IsAlternative, 1, 0))  ' D1298 + D1301 + D2747
        NodesString += String.Format("[{0},'{1}','{2}',-1,0,-1,'{3}'],", FirstRating.ID, FirstRating.GuidID.ToString, JS_SafeString((FirstRating.Value * 100).ToString + "%"), JS_SafeString(sNodeCommentA))       ' D1298 + D1301 + D2747
        NodesString += String.Format("[{0},'{1}','{2}',-1,0,-1,'{3}']", SecondRating.ID, SecondRating.GuidID.ToString, JS_SafeString((SecondRating.Value * 100).ToString + "%"), JS_SafeString(sNodeCommentB))    ' D1298 + D1301 + D2747
        ' D2999 ==

        Dim JudgmentsString As String = ""
        Dim JudgmentsList As New ArrayList

        Dim ListOfValues As New List(Of Double)

        For Each kvp As KeyValuePair(Of String, clsUser) In Users
            Dim user As clsUser = kvp.Value

            Dim pwData As clsPairwiseMeasureData = ParentNode.PWOutcomesJudgments.PairwiseJudgment(FirstRating.ID, SecondRating.ID, user.UserID, , CType(Pipe(CurrentStep), clsAction).PWONode.NodeID)
            Dim value As Double
            Dim comment As String = ""
            If pwData Is Nothing Then mJudgmentDateTime(user.UserID) = Nothing Else mJudgmentDateTime(user.UserID) = pwData.ModifyDate

            If pwData Is Nothing OrElse pwData.IsUndefined Then
                value = TeamTimeFuncs.UndefinedValue
            Else
                value = If(pwData.Value = 1, 0, -pwData.Advantage * (pwData.Value - 1))
            End If
            If pwData IsNot Nothing Then comment = pwData.Comment ' D1281 + D2757

            Dim UserStatus As Integer
            If Not GetStepAvailabilityForUser(ActionType.atPairwiseOutcomes, CType(Pipe(CurrentStep), clsAction), user) Then
                UserStatus = -2
            Else
                Select Case user.SyncEvaluationMode
                    Case SynchronousEvaluationMode.semVotingBox
                        UserStatus = user.VotingBoxID
                    Case SynchronousEvaluationMode.semOnline
                        UserStatus = 0
                    Case Else
                        UserStatus = -1
                End Select
                ' D2605 ===
                Dim fCanAdd As Boolean = UserStatus <> -1 OrElse PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess
                If fCanAdd Then JudgmentsList.Add(pwData)
                If pwData IsNot Nothing AndAlso Not pwData.IsUndefined AndAlso fCanAdd Then
                    ' D2605 ==
                    ListOfValues.Add(If(pwData.Advantage > 0, pwData.Value, 1 / pwData.Value))
                End If
            End If

            JudgmentsString += If(JudgmentsString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4},{5},'{6}']", user.UserID, JS_SafeString(user.UserEMail), JS_SafeString(user.UserName), If(user.Active, 1, 0), UserStatus, JS_SafeNumber(value), JS_SafeString(comment))    ' D1270

            'JudgmentsList.Add(pwData)
        Next

        ' group result
        Dim adv As Single
        Dim GroupResult As Double = TeamTimeFuncs.CombinePairwiseJudgments(JudgmentsList, adv)
        GroupResult = If(GroupResult = Single.NaN OrElse GroupResult = TeamTimeFuncs.UndefinedValue, TeamTimeFuncs.UndefinedValue, If(GroupResult = 1, 0, -adv * (GroupResult - 1))) 'C1002 + D1903 + D1909

        Dim ShowComment As Integer = If(PipeParameters.ShowComments, 1, 0)

        Dim Consensus As Single = 0 'C1003

        'C1009===
        If ListOfValues.Count > 1 Then
            Dim variance As Double = 1  ' D1903
            Dim MaxValue As Double = 9  ' D1903
            For Each value As Double In ListOfValues    ' D1903
                If (If(value < 1, 1 / value, value)) > MaxValue Then
                    MaxValue = If(value < 1, 1 / value, value)
                End If
                variance *= value
            Next
            variance = Math.Pow(variance, 1 / ListOfValues.Count)
            For i As Integer = 0 To ListOfValues.Count - 1
                ListOfValues(i) /= variance
                If ListOfValues(i) < 1 Then
                    ListOfValues(i) = 1 / ListOfValues(i)
                End If
            Next
            variance = 1
            For Each value As Double In ListOfValues    ' D1903
                variance *= value
            Next
            If ListOfValues.Count > 0 Then variance = Math.Pow(variance, 1 / ListOfValues.Count) ' D1594
            If Math.Log(MaxValue) = 0 Then variance = 0 Else variance = Math.Log(variance) / Math.Log(MaxValue) ' D1594

            Consensus = variance
        End If
        'C1009==

        Dim res As String = String.Format("[[{0}],[{1}],'{2}','{3}',{4}]", NodesString, JudgmentsString, JS_SafeNumber(GroupResult), JS_SafeNumber(Consensus), CInt(PWType))  ' D1270 'C1003 + D1478 + D1908
        Return res
    End Function

    Private Function GetJSON_Ratings(ByVal Action As clsAction) As String
        Dim ActionData As clsOneAtATimeEvaluationActionData = Action.ActionData
        ' nodes
        Dim NodesString As String = ""
        Dim ParentNode As clsNode
        Dim ChildNode As clsNode
        Dim nonpwData As clsNonPairwiseMeasureData = CType(ActionData.Judgment, clsNonPairwiseMeasureData)
        Dim RS As clsRatingScale

        If ActionData.Node.IsAlternative Then
            ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
            ChildNode = ActionData.Node
            RS = ChildNode.MeasurementScale
        Else
            ParentNode = ActionData.Node
            If ParentNode.IsTerminalNode Then
                ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
            Else
                ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(nonpwData.NodeID)
            End If
            RS = ParentNode.MeasurementScale
        End If

        If nonpwData IsNot Nothing Then ' D3476

            NodesString += String.Format("[{0},'{1}','{2}',{3},{4},0],", ParentNode.NodeID, ParentNode.NodeGuidID.ToString, JS_SafeString(ParentNode.NodeName), If(ParentNode.InfoDoc <> "", 1, 0), If(ParentNode.IsAlternative, 1, 0), ParentNode.ParentNodeID)  ' D1298 + D1301
            NodesString += String.Format("[{0},'{1}','{2}',{3},{4},{5}]", ChildNode.NodeID, ChildNode.NodeGuidID.ToString, JS_SafeString(ChildNode.NodeName), If(ChildNode.InfoDoc <> "", 1, 0), If(ChildNode.IsAlternative, 1, 0), If(String.IsNullOrEmpty(ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID)), 0, 1))  ' D1298 + D1301

            Dim sPrecision As Integer = 0   ' D2121

            ' rating scale
            Dim ScaleString As String = ""
            For i As Integer = 0 To RS.RatingSet.Count - 1
                Dim intensity As clsRating = CType(RS.RatingSet(i), clsRating)
                ScaleString += If(ScaleString = "", "", ",") + String.Format("[{0},'{1}','{2}','{3}','{4}']", intensity.ID, JS_SafeString(intensity.Name), JS_SafeNumber(Double2String(intensity.Value, 2)), JS_SafeNumber(Double2String(intensity.Value * 100, 2, True)), JS_SafeString(intensity.Comment)) ' D1270 + D3419
                ' D2121 ===
                Dim tLen As Integer = intensity.Value.ToString.Length - 4
                If tLen > 3 Then tLen = 3
                If tLen > sPrecision Then sPrecision = tLen
                ' D2121 ==
            Next
            Dim ScaleInfo = String.Format("{0},'{1}',{2},{3}", RS.ID, RS.GuidID, If(RS.Comment = "", 0, 1), Bool2Num(Not ProjectManager.Parameters.RatingsUseDirectValue(RS.GuidID))) ' D3543 + D4174

            ' judgments
            Dim JudgmentsString As String = ""
            Dim JudgmentsList As New ArrayList
            Dim ListOfValues As New List(Of Double)

            For Each kvp As KeyValuePair(Of String, clsUser) In Users
                Dim user As clsUser = kvp.Value

                If ActionData.Node.IsAlternative Then
                    nonpwData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                Else
                    nonpwData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                End If
                Dim sValue As String    ' D3553 + D3557
                Dim comment As String = ""  ' D1281
                If nonpwData Is Nothing Then mJudgmentDateTime(user.UserID) = Nothing Else mJudgmentDateTime(user.UserID) = nonpwData.ModifyDate ' D2577

                'If Not PipeParameters.SynchStartInPollingMode Then ' -D1282
                If nonpwData Is Nothing OrElse nonpwData.IsUndefined Then
                    sValue = JS_SafeNumber(TeamTimeFuncs.UndefinedValue)  ' D1903 + D3553
                Else
                    ' D3557 ===
                    Dim RMD As clsRatingMeasureData = CType(nonpwData, clsRatingMeasureData)
                    If RMD.Rating IsNot Nothing Then
                        If RMD.Rating.ID < 0 Then
                            sValue = String.Format("{0}{1}", TeamTimeFuncs.RatingsDirectPrefix, JS_SafeNumber(RMD.Rating.Value))
                        Else
                            sValue = JS_SafeNumber(CType(nonpwData, clsRatingMeasureData).Rating.ID)    ' D3553
                        End If
                        ' D3557 ==
                    Else
                        ' Direct values starts with "*" when direct value (TeamTimeFuncs.RatingsDirectPrefix)
                        sValue = String.Format("{0}{1}", TeamTimeFuncs.RatingsDirectPrefix, JS_SafeNumber(nonpwData.SingleValue))   ' D3553
                    End If
                End If
                If nonpwData IsNot Nothing Then comment = nonpwData.Comment ' D1281 + D2757

                Dim UserStatus As Integer
                'If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, ActionData, user) And (user.UserEMail <> SessionOwner.UserEMail) Then ' TODO: if restricted 'C1007
                If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, Action, user) Then ' TODO: if restricted 'C1007 + D1566
                    UserStatus = -2
                Else
                    Select Case user.SyncEvaluationMode
                        Case SynchronousEvaluationMode.semVotingBox
                            UserStatus = user.VotingBoxID
                        Case SynchronousEvaluationMode.semOnline
                            UserStatus = 0
                        Case Else
                            UserStatus = -1
                    End Select

                    ' D2605 ===
                    Dim fCanAdd As Boolean = UserStatus <> -1 OrElse PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess
                    If fCanAdd Then JudgmentsList.Add(nonpwData)
                    If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined AndAlso fCanAdd Then
                        ' D2605 ==
                        ListOfValues.Add(nonpwData.SingleValue)
                    End If
                End If

                JudgmentsString += If(JudgmentsString = "", "", ",") + String.Format("[{0}, '{1}','{2}',{3},{4},'{5}','{6}']", user.UserID, JS_SafeString(user.UserEMail), JS_SafeString(user.UserName), If(user.Active, 1, 0), UserStatus, sValue, JS_SafeString(comment))    ' D1270 + D3553 + D3557
                'JudgmentsList.Add(nonpwData)
            Next

            ' group result
            Dim GroupResult As Single = TeamTimeFuncs.CombineNonPairwiseJudgments(JudgmentsList)

            'TODO: Is for TeamTime parameter set?
            Dim ShowComment As Integer = If(PipeParameters.ShowComments, 1, 0)

            Dim StdDeviation As Single
            Dim Variance As Single
            Dim bConsensus As Boolean = ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, Variance, StdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) 'C1003
            If Not bConsensus Then
                StdDeviation = 0
                Variance = 0
            End If

            Dim res As String = String.Format("[[{0}],[{1}],[{2}],{3},{4},{5},[{6}]]", NodesString, ScaleString, JudgmentsString, JS_SafeNumber(GroupResult), JS_SafeNumber(Variance), sPrecision, ScaleInfo)    ' D1270 'C1002 'C1003 + D1288 + D2121 + D3543

            Return res
        Else
            Return ""
        End If
    End Function

    Private Function GetJSON_Direct(ByVal Action As clsAction) As String
        Dim ActionData As clsOneAtATimeEvaluationActionData = Action.ActionData

        ' nodes
        Dim NodesString As String = ""
        Dim ParentNode As clsNode = ActionData.Node
        Dim ChildNode As clsNode
        Dim DirectData As clsDirectMeasureData = CType(ActionData.Judgment, clsDirectMeasureData)
        If ActionData.Node.IsAlternative Then
            If ActionData.IsEdge Then
                ParentNode = ActionData.Node
                ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(DirectData.NodeID)
            Else
                ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
                ChildNode = ActionData.Node
            End If
        Else
            ParentNode = ActionData.Node
            If ParentNode.IsTerminalNode Then
                ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(DirectData.NodeID)
            Else
                ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(DirectData.NodeID)
            End If
        End If

        If ActionData.Judgment IsNot Nothing Then ' D3476

            NodesString += String.Format("[{0},'{1}','{2}',{3},{4},0],", ParentNode.NodeID, ParentNode.NodeGuidID.ToString, JS_SafeString(ParentNode.NodeName), If(ParentNode.InfoDoc <> "", 1, 0), If(ParentNode.IsAlternative, 1, 0), ParentNode.ParentNodeID)
            If ChildNode IsNot Nothing AndAlso Not ParentNode.NodeGuidID.Equals(ChildNode.NodeGuidID) Then NodesString += String.Format("[{0},'{1}','{2}',{3},{4},{5}]", ChildNode.NodeID, ChildNode.NodeGuidID.ToString, JS_SafeString(ChildNode.NodeName), If(ChildNode.InfoDoc <> "", 1, 0), If(ChildNode.IsAlternative, 1, 0), If(String.IsNullOrEmpty(ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID)), 0, 1)) ' D3384

            ' judgments
            Dim JudgmentsString As String = ""
            Dim JudgmentsList As New ArrayList
            Dim ListOfValues As New List(Of Double)

            For Each kvp As KeyValuePair(Of String, clsUser) In Users
                Dim user As clsUser = kvp.Value

                If ActionData.Node.IsAlternative Then
                    If ActionData.IsEdge Then
                        DirectData = CType(ChildNode.EventsJudgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                    Else
                        DirectData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                    End If
                Else
                    DirectData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                End If

                Dim value As Single
                Dim comment As String = ""
                If DirectData Is Nothing Then mJudgmentDateTime(user.UserID) = Nothing Else mJudgmentDateTime(user.UserID) = DirectData.ModifyDate ' D2577

                If DirectData Is Nothing OrElse DirectData.IsUndefined Then
                    value = TeamTimeFuncs.UndefinedValue  ' D1903
                Else
                    value = CType(DirectData, clsDirectMeasureData).DirectData
                    'ListOfValues.Add(value)
                End If
                If DirectData IsNot Nothing Then comment = DirectData.Comment ' D2757

                Dim UserStatus As Integer
                'If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, ActionData, user) And (user.UserEMail <> SessionOwner.UserEMail) Then ' TODO: if restricted 'C1007
                If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, Action, user) Then ' D1569
                    UserStatus = -2
                Else
                    Select Case user.SyncEvaluationMode
                        Case SynchronousEvaluationMode.semVotingBox
                            UserStatus = user.VotingBoxID
                        Case SynchronousEvaluationMode.semOnline
                            UserStatus = 0
                        Case Else
                            UserStatus = -1
                    End Select
                    ' D2605 ===
                    Dim fCanAdd As Boolean = UserStatus <> -1 OrElse PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess
                    If fCanAdd Then JudgmentsList.Add(DirectData)
                    If value <> TeamTimeFuncs.UndefinedValue AndAlso fCanAdd Then
                        ' D2605 ==
                        ListOfValues.Add(value)
                    End If
                End If

                JudgmentsString += If(JudgmentsString = "", "", ",") + String.Format("[{0}, '{1}','{2}',{3},{4},{5},'{6}']", user.UserID, JS_SafeString(user.UserEMail), JS_SafeString(user.UserName), If(user.Active, 1, 0), UserStatus, JS_SafeNumber(value), JS_SafeString(comment))
                'JudgmentsList.Add(DirectData)
            Next

            ' group result
            Dim GroupResult As Single = TeamTimeFuncs.CombineNonPairwiseJudgments(JudgmentsList)

            'TODO: Is for TeamTime parameter set?
            Dim ShowComment As Integer = If(PipeParameters.ShowComments, 1, 0)

            Dim StdDeviation As Single
            Dim Variance As Single
            Dim bConsensus As Boolean = ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, Variance, StdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) 'C1003
            If Not bConsensus Then
                StdDeviation = 0
                Variance = 0
            End If

            Dim res As String = String.Format("[[{0}],[{1}],{2},{3}]", NodesString, JudgmentsString, JS_SafeNumber(GroupResult), JS_SafeNumber(Variance))

            Return res
        Else
            Return ""
        End If
    End Function

    Private Function GetJSON_StepFunction(ByVal Action As clsAction) As String
        Dim ActionData As clsOneAtATimeEvaluationActionData = Action.ActionData

        ' nodes
        Dim NodesString As String = ""
        Dim ParentNode As clsNode = ActionData.Node
        Dim ChildNode As clsNode
        Dim StepData As clsStepMeasureData = CType(ActionData.Judgment, clsStepMeasureData)
        Dim SF As clsStepFunction
        If ActionData.Node.IsAlternative Then
            ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
            ChildNode = ActionData.Node
            SF = ChildNode.MeasurementScale
        Else
            ParentNode = ActionData.Node
            If ParentNode.IsTerminalNode Then
                ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(StepData.NodeID)
            Else
                ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(StepData.NodeID)
            End If
            SF = ParentNode.MeasurementScale
        End If

        If StepData IsNot Nothing Then ' D3476
            NodesString += String.Format("[{0},'{1}','{2}',{3},{4},0],", ParentNode.NodeID, ParentNode.NodeGuidID.ToString, JS_SafeString(ParentNode.NodeName), If(ParentNode.InfoDoc <> "", 1, 0), If(ParentNode.IsAlternative, 1, 0), ParentNode.ParentNodeID)
            If ChildNode IsNot Nothing AndAlso Not ParentNode.NodeGuidID.Equals(ChildNode.NodeGuidID) Then NodesString += String.Format("[{0},'{1}','{2}',{3},{4},{5}]", ChildNode.NodeID, ChildNode.NodeGuidID.ToString, JS_SafeString(ChildNode.NodeName), If(ChildNode.InfoDoc <> "", 1, 0), If(ChildNode.IsAlternative, 1, 0), If(String.IsNullOrEmpty(ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID)), 0, 1)) ' D3384

            Dim tmp As Single = SF.GetValue(0)  ' D6333 // trick for fix issue with "high=0" on some scales
            Dim IntervalsString As String = ""
            For i As Integer = 0 To SF.Intervals.Count - 1
                Dim interval As clsStepInterval = CType(SF.Intervals(i), clsStepInterval)
                IntervalsString += If(IntervalsString = "", "", ",") + String.Format("[{0},'{1}',{2},{3},{4}]", interval.ID, JS_SafeString(interval.Name), JS_SafeNumber(interval.Low), JS_SafeNumber(If(i = SF.Intervals.Count - 1, Integer.MaxValue, interval.High)), JS_SafeNumber(interval.Value))  ' D7660
            Next

            ' judgments
            Dim JudgmentsString As String = ""
            Dim JudgmentsList As New ArrayList
            Dim ListOfValues As New List(Of Double)

            For Each kvp As KeyValuePair(Of String, clsUser) In Users
                Dim user As clsUser = kvp.Value

                If ActionData.Node.IsAlternative Then
                    StepData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                Else
                    StepData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                End If

                Dim value As Single
                Dim comment As String = ""
                If StepData Is Nothing Then mJudgmentDateTime(user.UserID) = Nothing Else mJudgmentDateTime(user.UserID) = StepData.ModifyDate ' D2577

                If StepData Is Nothing OrElse StepData.IsUndefined Then
                    value = TeamTimeFuncs.UndefinedValue  ' D1903
                Else
                    value = CType(StepData, clsStepMeasureData).Value
                    'ListOfValues.Add(value)
                End If
                If StepData IsNot Nothing Then comment = StepData.Comment ' D2757

                Dim UserStatus As Integer
                'If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, ActionData, user) And (user.UserEMail <> SessionOwner.UserEMail) Then ' TODO: if restricted 'C1007
                If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, Action, user) Then ' D1569
                    UserStatus = -2
                Else
                    Select Case user.SyncEvaluationMode
                        Case SynchronousEvaluationMode.semVotingBox
                            UserStatus = user.VotingBoxID
                        Case SynchronousEvaluationMode.semOnline
                            UserStatus = 0
                        Case Else
                            UserStatus = -1
                    End Select

                    ' D2605 ===
                    Dim fCanAdd As Boolean = UserStatus <> -1 OrElse PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess
                    If fCanAdd Then JudgmentsList.Add(StepData)
                    If value <> TeamTimeFuncs.UndefinedValue AndAlso fCanAdd Then ' D1903
                        ' D2605 ==
                        ListOfValues.Add(value)
                    End If

                End If

                JudgmentsString += If(JudgmentsString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4},{5},'{6}']", user.UserID, JS_SafeString(user.UserEMail), JS_SafeString(user.UserName), If(user.Active, 1, 0), UserStatus, JS_SafeNumber(value), JS_SafeString(comment))
                'JudgmentsList.Add(StepData)
            Next

            ' group result
            Dim GroupResult As Single = TeamTimeFuncs.CombineNonPairwiseJudgments(JudgmentsList)

            'TODO: Is for TeamTime parameter set?
            Dim ShowComment As Integer = If(PipeParameters.ShowComments, 1, 0)

            Dim StdDeviation As Single
            Dim Variance As Single
            Dim bConsensus As Boolean = ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, Variance, StdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) 'C1003
            If Not bConsensus Then
                StdDeviation = 0
                Variance = 0
            End If

            Dim res As String = String.Format("[[{0}],[{1}],[{2}],{3},{4},{5},'{6}',{7}]", NodesString, IntervalsString, JudgmentsString, JS_SafeNumber(GroupResult), JS_SafeNumber(Variance), If(SF.IsPiecewiseLinear, 1, 0), SF.GuidID, Bool2Num(SF.Comment <> ""))   ' D1548 + D7228

            Return res
        Else
            Return ""
        End If
    End Function

    Private Function GetJSON_RegularUtilityCurve(ByVal Action As clsAction) As String
        Dim ActionData As clsOneAtATimeEvaluationActionData = Action.ActionData

        ' nodes
        Dim NodesString As String = ""
        Dim ParentNode As clsNode = ActionData.Node
        Dim ChildNode As clsNode
        Dim UCData As clsUtilityCurveMeasureData = CType(ActionData.Judgment, clsUtilityCurveMeasureData)
        If ActionData.Node.IsAlternative Then
            ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
            ChildNode = ActionData.Node
        Else
            ParentNode = ActionData.Node
            If ParentNode.IsTerminalNode Then
                ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(UCData.NodeID)
            Else
                ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(UCData.NodeID)
            End If
        End If

        If UCData.UtilityCurve IsNot Nothing Then ' D3476
            NodesString += String.Format("[{0},'{1}','{2}',{3},{4},0],", ParentNode.NodeID, ParentNode.NodeGuidID.ToString, JS_SafeString(ParentNode.NodeName), If(ParentNode.InfoDoc <> "", 1, 0), If(ParentNode.IsAlternative, 1, 0), ParentNode.ParentNodeID)
            If ChildNode IsNot Nothing AndAlso Not ParentNode.NodeGuidID.Equals(ChildNode.NodeGuidID) Then NodesString += String.Format("[{0},'{1}','{2}',{3},{4},{5}]", ChildNode.NodeID, ChildNode.NodeGuidID.ToString, JS_SafeString(ChildNode.NodeName), If(ChildNode.InfoDoc <> "", 1, 0), If(ChildNode.IsAlternative, 1, 0), If(String.IsNullOrEmpty(ProjectManager.InfoDocs.GetNodeWRTInfoDoc(ChildNode.NodeGuidID, ParentNode.NodeGuidID)), 0, 1)) ' D3384

            Dim sUtilityCurve As String = ""
            With CType(UCData.UtilityCurve, clsRegularUtilityCurve)
                sUtilityCurve = String.Format("[{0},{1},{2},{3},{4},'{5}',{6}]", JS_SafeNumber(.Low), JS_SafeNumber(.High), JS_SafeNumber(.Curvature), If(.IsLinear, 1, 0), If(.IsIncreasing, 1, 0), .GuidID, Bool2Num(.Comment <> ""))  ' D7228
            End With

            ' judgments
            Dim JudgmentsString As String = ""
            Dim JudgmentsList As New ArrayList
            Dim ListOfValues As New List(Of Double)

            For Each kvp As KeyValuePair(Of String, clsUser) In Users
                Dim user As clsUser = kvp.Value

                If ActionData.Node.IsAlternative Then
                    UCData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                Else
                    UCData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, user.UserID)
                End If

                Dim value As Single
                Dim comment As String = ""
                If UCData Is Nothing Then mJudgmentDateTime(user.UserID) = Nothing Else mJudgmentDateTime(user.UserID) = UCData.ModifyDate ' D2577

                If UCData Is Nothing OrElse UCData.IsUndefined Then
                    value = TeamTimeFuncs.UndefinedValue  ' D1903
                Else
                    value = CType(UCData, clsUtilityCurveMeasureData).ObjectValue
                    'ListOfValues.Add(CType(UCData, clsUtilityCurveMeasureData).SingleValue)
                End If
                If UCData IsNot Nothing Then comment = UCData.Comment ' D2757

                Dim UserStatus As Integer
                'If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, ActionData, user) And (user.UserEMail <> SessionOwner.UserEMail) Then ' TODO: if restricted 'C1007
                If Not GetStepAvailabilityForUser(ActionType.atNonPWOneAtATime, Action, user) Then ' D1569
                    UserStatus = -2
                Else
                    Select Case user.SyncEvaluationMode
                        Case SynchronousEvaluationMode.semVotingBox
                            UserStatus = user.VotingBoxID
                        Case SynchronousEvaluationMode.semOnline
                            UserStatus = 0
                        Case Else
                            UserStatus = -1
                    End Select

                    ' D2605 ===
                    Dim fCanAdd As Boolean = UserStatus <> -1 OrElse PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess
                    If fCanAdd Then JudgmentsList.Add(UCData)
                    If value <> TeamTimeFuncs.UndefinedValue AndAlso fCanAdd Then
                        ' D2605 ==
                        ListOfValues.Add(value)
                    End If
                End If

                JudgmentsString += If(JudgmentsString = "", "", ",") + String.Format("[{0},'{1}','{2}',{3},{4},{5},'{6}']", user.UserID, JS_SafeString(user.UserEMail), JS_SafeString(user.UserName), If(user.Active, 1, 0), UserStatus, JS_SafeNumber(value), JS_SafeString(comment))
                'JudgmentsList.Add(UCData)
            Next

            ' group result
            Dim GroupResult As Single = TeamTimeFuncs.CombineNonPairwiseJudgments(JudgmentsList)

            'TODO: Is for TeamTime parameter set?
            Dim ShowComment As Integer = If(PipeParameters.ShowComments, 1, 0)

            Dim StdDeviation As Single
            Dim Variance As Single
            Dim bConsensus As Boolean = ECCore.MathFuncs.CalculateVarianceAndStandardDeviation(ListOfValues, Variance, StdDeviation, MathFuncs.StandardDeviationMode.sdmBiased) 'C1003
            If Not bConsensus Then
                StdDeviation = 0
                Variance = 0
            End If

            Dim res As String = String.Format("[[{0}],{1},[{2}],{3},{4}]", NodesString, sUtilityCurve, JudgmentsString, JS_SafeNumber(GroupResult), JS_SafeNumber(Variance))

            Return res
        Else
            Return ""
        End If
    End Function
    ' D1577 ==

    Public Sub VerifyUsers(ByVal UsersList As List(Of clsUser), Optional tDestLst As Dictionary(Of String, clsUser) = Nothing) ' D4825
        Dim tmpUsers As New Dictionary(Of String, clsUser)
        If tDestLst Is Nothing Then tDestLst = Users    ' D4825
        For Each user As clsUser In UsersList
            If user IsNot Nothing Then
                If tDestLst.ContainsKey(user.UserEMail) Then
                    tDestLst(user.UserEMail).Active = user.Active
                Else
                    tDestLst.Add(user.UserEMail, user)
                    AddUser(user)
                End If
                tmpUsers.Add(user.UserEMail, tDestLst(user.UserEMail))
            End If
        Next
        tDestLst = tmpUsers
    End Sub

    ''' <summary>
    ''' Get JSON string for current step in the pipe
    ''' </summary>
    ''' <param name="tSessionUsers">List of clsUser, who is participate in that meeting. All items are _clone_ from original items from ProjectManager, but with real names from WebCore and .Active property mean isOnline</param>
    ''' <returns>String with JSON</returns>
    ''' <remarks></remarks>
    Public Function GetJSON(ByVal tSessionUsers As List(Of clsUser), Optional tAllUsers As List(Of clsUser) = Nothing) As String ' D1270 + D1271 + D1290 + D4825
        Dim StepID As Integer = CurrentStep ' D1270
        If StepID < 0 Or StepID >= Pipe.Count Then Return ""

        VerifyUsers(tSessionUsers, Users)
        ' D4825 ===
        If tAllUsers Is Nothing OrElse tAllUsers.Count = 0 Then tAllUsers = tSessionUsers
        ' D6015 ===
        AllUsers = New Dictionary(Of String, clsUser)
        For Each K As String In Users.Keys
            AllUsers.Add(K, Users(K))
        Next
        ' D6015 ==
        VerifyUsers(tAllUsers, AllUsers)
        ' D4825 ==

        Dim sJSON As String = ""

        ' AD: temp only
        ' Use tSessionUsers for get list of users
        Dim User As clsUser = ProjectManager.User ' D1270

        Dim CurAction As clsAction = Pipe(StepID)
        Select Case CurAction.ActionType

            Case Canvas.ActionType.atPairwise
                sJSON = GetJSON_Pairwise(CurAction)

            Case ActionType.atPairwiseOutcomes
                sJSON = GetJSON_PairwiseOutcomes(CurAction)

            Case Canvas.ActionType.atNonPWOneAtATime

                Select Case CType(CurAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType
                    Case ECMeasureType.mtRatings
                        sJSON = GetJSON_Ratings(CurAction)

                    Case ECMeasureType.mtDirect
                        sJSON = GetJSON_Direct(CurAction)

                    Case ECMeasureType.mtStep
                        sJSON = GetJSON_StepFunction(CurAction)

                    Case ECMeasureType.mtRegularUtilityCurve
                        sJSON = GetJSON_RegularUtilityCurve(CurAction)  ' D1577

                End Select
                ' D1509 ==

            Case Canvas.ActionType.atShowLocalResults
                sJSON = GetJSON_LocalResults(CurAction) 'C1002

            Case Canvas.ActionType.atShowGlobalResults
                sJSON = GetJSON_GlobalResults(CurAction, WRTNodeID) 'C1002 + D1290

            Case Canvas.ActionType.atSensitivityAnalysis
                sJSON = GetJSON_DSA(CurAction, WRTNodeID)    ' D1414
                'CType(ActionData, clsSensitivityAnalysisActionData).WriteXML(writer)

        End Select

        Return sJSON
    End Function

    ' D4003 ===
    Private Sub UpdateCachedProgress(UID As Integer, isUndefined As Boolean, WasUndefined As Boolean)
        ' update cached progress
        If isUndefined Then
            ' this means we erased judgment
            If Not WasUndefined AndAlso mMadeJudgments.ContainsKey(UID) Then mMadeJudgments.Item(UID) -= 1
        Else
            If WasUndefined Then
                If mMadeJudgments.ContainsKey(UID) Then mMadeJudgments.Item(UID) += 1 Else mMadeJudgments.Add(UID, 1)
            End If
        End If
    End Sub
    ' D4003 ==

    'Private Function ParseJSONFromClient(ByVal UserID As Integer, ByVal JSON As String) As Boolean
    Public Function ParseStringFromClient(ByVal sData As String) As Boolean    ' D1275
        ' TODO: 
        ' verify step
        ' get data
        ' add to memory
        ' write to database
        ' update judgments count

        ' D1275 ===
        Dim fResult As Boolean = False

        Dim sSplitter As String() = {Judgment_Delimeter}
        Dim tData As String() = sData.Split(sSplitter, 6, StringSplitOptions.None)  ' D2578
        If tData.Length >= 4 Then
            Dim sUID As String = tData(0)
            Dim sStep As String = tData(1)
            Dim sGUID As String = tData(2)  ' D1281
            Dim sValue As String = tData(3)
            Dim sComment As String = ""
            If tData.Length = 5 Then sComment = Trim(tData(4)) ' D2578
            If tData.Length > 5 Then sComment = Trim(tData(5)) ' D2578

            Dim UID As Integer = -1
            Dim StepID As Integer = -1
            If Integer.TryParse(sUID, UID) AndAlso Integer.TryParse(sStep, StepID) Then

                Dim tUser As clsUser = ProjectManager.GetUserByID(UID)
                If StepID >= 0 AndAlso StepID < Pipe.Count AndAlso tUser IsNot Nothing Then

                    Dim CurAction As clsAction = Pipe(StepID)

                    If CurAction.StepGuid.ToString.ToLower = sGUID.ToLower Then

                        ' D2606 ===
                        Dim fCanMakeJudgment As Boolean = True
                        If tUser.SyncEvaluationMode = SynchronousEvaluationMode.semByFacilitatorOnly OrElse Not GetStepAvailabilityForUser(CurAction.ActionType, CurAction, tUser) Then fCanMakeJudgment = False

                        If fCanMakeJudgment Then
                            ' D2606 ==
                            Select Case CurAction.ActionType
                                Case Canvas.ActionType.atPairwise

                                    ' D1479 ===
                                    Dim Value As Double = TeamTimeFuncs.UndefinedValue   ' D1903 + D1908
                                    If String2Double(sValue, Value) Then    ' D1908
                                        Dim fUndef As Boolean = sValue = TeamTimeFuncs.UndefinedValue.ToString OrElse sValue = (TeamTimeFuncs.UndefinedValue + 1).ToString OrElse Value <= TeamTimeFuncs.UndefinedValue + 200  ' D1907 + D1908
                                        ' D1479 ==

                                        Dim pwData As clsPairwiseMeasureData = CType(CurAction.ActionData, clsPairwiseMeasureData)
                                        Dim ParentNode As clsNode = ProjectManager.ActiveObjectives.GetNodeByID(pwData.ParentNodeID)

                                        Dim FirstNode As clsNode = Nothing
                                        Dim SecondNode As clsNode = Nothing

                                        If ParentNode IsNot Nothing Then
                                            If ParentNode.IsTerminalNode Then
                                                FirstNode = ProjectManager.ActiveAlternatives.GetNodeByID(pwData.FirstNodeID)
                                                SecondNode = ProjectManager.ActiveAlternatives.GetNodeByID(pwData.SecondNodeID)
                                            Else
                                                FirstNode = ProjectManager.ActiveObjectives.GetNodeByID(pwData.FirstNodeID)
                                                SecondNode = ProjectManager.ActiveObjectives.GetNodeByID(pwData.SecondNodeID)
                                            End If
                                        End If

                                        If ParentNode IsNot Nothing And FirstNode IsNot Nothing And SecondNode IsNot Nothing Then
                                            Dim CurPWJ As clsPairwiseMeasureData = CType(ParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(FirstNode.NodeID, SecondNode.NodeID, UID)
                                            Dim NewPWJudgment As clsPairwiseMeasureData = Nothing

                                            If CurPWJ IsNot Nothing Then
                                                'If Not (CurPWJ.IsUndefined AndAlso fUndef) Then ' if was undefined and became undefined, no changes    ' -D2757
                                                Dim CurValue = If(CurPWJ.Value = 1, 0, -CurPWJ.Advantage * (CurPWJ.Value - 1))

                                                If (CurPWJ.IsUndefined <> fUndef) OrElse (CurValue <> Value) OrElse (CurPWJ.Comment <> sComment) Then ' if there's no changes, do nothing, otherwise change judgment 'C1002
                                                    Dim WasUndefined As Boolean = CurPWJ.IsUndefined
                                                    ' create new judgment
                                                    If fUndef Then
                                                        ' setting undefined
                                                        NewPWJudgment = New clsPairwiseMeasureData(FirstNode.NodeID, SecondNode.NodeID, 0, 0, ParentNode.NodeID, UID, True)
                                                    Else
                                                        NewPWJudgment = New clsPairwiseMeasureData(FirstNode.NodeID, SecondNode.NodeID, If(Value < 0, 1, -1), Math.Abs(Value) + 1, ParentNode.NodeID, UID, False)  ' D1281
                                                    End If
                                                    NewPWJudgment.Comment = sComment
                                                    NewPWJudgment.ModifyDate = Now  ' D2577
                                                    ParentNode.Judgments.AddMeasureData(NewPWJudgment, False)
                                                    ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                    UpdateCachedProgress(UID, NewPWJudgment.IsUndefined, WasUndefined)
                                                End If
                                                'End If
                                            Else
                                                'If Not fUndef Then
                                                ' create new judgment
                                                NewPWJudgment = New clsPairwiseMeasureData(FirstNode.NodeID, SecondNode.NodeID, If(Value < 0, 1, -1), Math.Abs(Value) + 1, ParentNode.NodeID, UID, fUndef, sComment)    ' D1281
                                                NewPWJudgment.ModifyDate = Now  ' D2577
                                                ParentNode.Judgments.AddMeasureData(NewPWJudgment, False)
                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                mMadeJudgments.Item(UID) += If(fUndef, 0, 1)
                                                'End If
                                            End If

                                            If UID = ProjectManager.User.UserID And NewPWJudgment IsNot Nothing Then
                                                CurAction.ActionData = NewPWJudgment
                                            End If
                                        End If

                                        fResult = True
                                    End If

                                Case Canvas.ActionType.atPairwiseOutcomes
                                    Dim Value As Double = TeamTimeFuncs.UndefinedValue
                                    If String2Double(sValue, Value) Then
                                        Dim fUndef As Boolean = sValue = TeamTimeFuncs.UndefinedValue.ToString OrElse sValue = (TeamTimeFuncs.UndefinedValue + 1).ToString OrElse Value <= TeamTimeFuncs.UndefinedValue + 200  ' D1907 + D1908
                                        ' D1479 ==

                                        Dim pwData As clsPairwiseMeasureData = CType(CurAction.ActionData, clsPairwiseMeasureData)
                                        Dim ParentNode As clsNode = CurAction.ParentNode
                                        Dim RS As clsRatingScale = CurAction.PWONode.MeasurementScale

                                        Dim FirstRating As clsRating = Nothing
                                        Dim SecondRating As clsRating = Nothing

                                        If ParentNode IsNot Nothing Then
                                            FirstRating = RS.GetRatingByID(pwData.FirstNodeID)
                                            SecondRating = RS.GetRatingByID(pwData.SecondNodeID)
                                        End If

                                        If ParentNode IsNot Nothing And FirstRating IsNot Nothing And SecondRating IsNot Nothing Then
                                            Dim CurPWJ As clsPairwiseMeasureData = ParentNode.PWOutcomesJudgments.PairwiseJudgment(FirstRating.ID, SecondRating.ID, UID, , CurAction.PWONode.NodeID)
                                            Dim NewPWJudgment As clsPairwiseMeasureData = Nothing

                                            If CurPWJ IsNot Nothing Then
                                                'If Not (CurPWJ.IsUndefined And fUndef) Then ' if was undefined and became undefined, no changes ' -D2757
                                                Dim CurValue = If(CurPWJ.Value = 1, 0, -CurPWJ.Advantage * (CurPWJ.Value - 1))

                                                If (CurPWJ.IsUndefined <> fUndef) Or (CurValue <> Value) Or (CurPWJ.Comment <> sComment) Then ' if there's no changes, do nothing, otherwise change judgment 'C1002
                                                    Dim WasUndefined As Boolean = CurPWJ.IsUndefined
                                                    ' create new judgment
                                                    If fUndef Then
                                                        ' setting undefined
                                                        NewPWJudgment = New clsPairwiseMeasureData(FirstRating.ID, SecondRating.ID, 0, 0, ParentNode.NodeID, UID, True)
                                                    Else
                                                        NewPWJudgment = New clsPairwiseMeasureData(FirstRating.ID, SecondRating.ID, If(Value < 0, 1, -1), Math.Abs(Value) + 1, ParentNode.NodeID, UID, False)
                                                    End If
                                                    NewPWJudgment.OutcomesNodeID = CurAction.PWONode.NodeID
                                                    NewPWJudgment.Comment = sComment
                                                    NewPWJudgment.ModifyDate = Now  ' D2577
                                                    ParentNode.PWOutcomesJudgments.AddMeasureData(NewPWJudgment, False)
                                                    ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                    UpdateCachedProgress(UID, NewPWJudgment.IsUndefined, WasUndefined)  ' D4003
                                                End If
                                                'End If
                                            Else
                                                If Not fUndef Then
                                                    ' create new judgment
                                                    NewPWJudgment = New clsPairwiseMeasureData(FirstRating.ID, SecondRating.ID, If(Value < 0, 1, -1), Math.Abs(Value) + 1, ParentNode.NodeID, UID, False, sComment)
                                                    NewPWJudgment.OutcomesNodeID = CurAction.PWONode.NodeID
                                                    NewPWJudgment.ModifyDate = Now  ' D2577
                                                    ParentNode.PWOutcomesJudgments.AddMeasureData(NewPWJudgment, False)
                                                    ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                    mMadeJudgments.Item(UID) += 1
                                                End If
                                            End If

                                            If UID = ProjectManager.User.UserID And NewPWJudgment IsNot Nothing Then
                                                CurAction.ActionData = NewPWJudgment
                                            End If
                                        End If

                                        fResult = True
                                    End If

                                Case Canvas.ActionType.atNonPWOneAtATime

                                    Select Case CType(CurAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType ' D1544

                                        Case ECMeasureType.mtRatings    ' D1544

                                            ' D2757 ===
                                            Dim tRatingID As Long = -1

                                            ' D3553 + D3555 ===
                                            ' Starts with "*" when direct value (TeamTimeFuncs.RatingsDirectPrefix)
                                            Dim fIsDirect As Boolean = sValue.StartsWith(TeamTimeFuncs.RatingsDirectPrefix)
                                            Dim tDirectVaue As Double = Integer.MinValue
                                            ' Try to parse string without leading "*", value must be in [0..1]
                                            If fIsDirect Then fIsDirect = String2Double(sValue.Substring(1), tDirectVaue) AndAlso (tDirectVaue >= 0 AndAlso tDirectVaue <= 1)
                                            ' D2757  + D3555 ==
                                            If fIsDirect OrElse Long.TryParse(sValue, tRatingID) Then
                                                Dim RatingID As Integer = -1
                                                If tRatingID > Integer.MinValue Then RatingID = CInt(tRatingID)
                                                Dim fUndef As Boolean = RatingID < 0 AndAlso Not fIsDirect
                                                ' D3553 ==

                                                Dim RatingsAction As clsOneAtATimeEvaluationActionData = CType(CurAction.ActionData, clsOneAtATimeEvaluationActionData)

                                                Dim ParentNode As clsNode
                                                Dim ChildNode As clsNode
                                                Dim nonpwData As clsNonPairwiseMeasureData = CType(RatingsAction.Judgment, clsNonPairwiseMeasureData)
                                                If RatingsAction.Node.IsAlternative Then
                                                    ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
                                                    ChildNode = RatingsAction.Node
                                                Else
                                                    ParentNode = RatingsAction.Node
                                                    If ParentNode.IsTerminalNode Then
                                                        ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                                                    Else
                                                        ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(nonpwData.NodeID)
                                                    End If
                                                End If

                                                If ParentNode IsNot Nothing And ChildNode IsNot Nothing Then
                                                    Dim MS As clsRatingScale
                                                    If RatingsAction.Node.IsAlternative Then
                                                        MS = ChildNode.MeasurementScale
                                                        nonpwData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    Else
                                                        MS = ParentNode.MeasurementScale
                                                        nonpwData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    End If

                                                    Dim CurRatingJ As clsRatingMeasureData = nonpwData ' CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    Dim NewRJudgment As clsRatingMeasureData = Nothing

                                                    If CurRatingJ IsNot Nothing Then
                                                        'If Not (CurRatingJ.IsUndefined And fUndef) Then ' if was undefined and became undefined, no changes    ' -D2757
                                                        'If CurRatingJ.Rating IsNot Nothing Then ' there might be ECD-like single values
                                                        If (CurRatingJ.IsUndefined <> fUndef) OrElse (CurRatingJ.IsUndefined = fUndef) AndAlso ((CurRatingJ.Rating IsNot Nothing AndAlso CurRatingJ.Rating.ID <> RatingID) OrElse fIsDirect OrElse (CurRatingJ.Comment <> sComment)) Then ' if there's no changes, do nothing, otherwise change judgment 'C1002 + D2757 + D3553
                                                            Dim WasUndefined As Boolean = CurRatingJ.IsUndefined
                                                            ' create new judgment
                                                            If fUndef Then
                                                                ' setting undefined
                                                                NewRJudgment = New clsRatingMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Nothing, MS, True)
                                                            Else
                                                                ' D3553 ===
                                                                If fIsDirect Then
                                                                    Dim R As New clsRating(-1, "Direct input from EC Core (TeamTime)", tDirectVaue, Nothing, sComment)
                                                                    NewRJudgment = New clsRatingMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, R, MS, False)
                                                                Else
                                                                    ' D3533 ==
                                                                    ' set new rating
                                                                    Dim R As clsRating = MS.GetRatingByID(RatingID)
                                                                    If R IsNot Nothing Then
                                                                        NewRJudgment = New clsRatingMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, R, MS, False)
                                                                    End If
                                                                End If
                                                            End If
                                                            If NewRJudgment IsNot Nothing Then
                                                                NewRJudgment.Comment = sComment
                                                                NewRJudgment.ModifyDate = Now   ' D2577
                                                                If RatingsAction.Node.IsAlternative Then
                                                                    ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewRJudgment, False)
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewRJudgment, False)
                                                                End If
                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                UpdateCachedProgress(UID, NewRJudgment.IsUndefined, WasUndefined)  ' D4003
                                                            End If
                                                        End If
                                                        'End If
                                                        'End If
                                                    Else
                                                        If Not fUndef Then
                                                            ' if there's no judgment, but user selected some rating, then add it
                                                            ' D3553 ===
                                                            If fIsDirect Then
                                                                Dim R As New clsRating(-1, "Direct input from EC Core (TeamTime)", tDirectVaue, Nothing, sComment)
                                                                NewRJudgment = New clsRatingMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, R, MS, False)
                                                            Else
                                                                ' D3553 ==
                                                                Dim R As clsRating = MS.GetRatingByID(RatingID)
                                                                If R IsNot Nothing Then
                                                                    NewRJudgment = New clsRatingMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, R, MS, False, sComment)
                                                                End If
                                                            End If
                                                            If NewRJudgment IsNot Nothing Then
                                                                NewRJudgment.ModifyDate = Now   ' D2577
                                                                If RatingsAction.Node.IsAlternative Then
                                                                    ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewRJudgment, False)
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewRJudgment, False)
                                                                End If

                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                mMadeJudgments.Item(UID) += 1
                                                            End If
                                                        End If
                                                    End If
                                                    If UID = ProjectManager.User.UserID And NewRJudgment IsNot Nothing Then
                                                        RatingsAction.Judgment = NewRJudgment
                                                    End If
                                                End If

                                                fResult = True
                                            End If

                                            ' D1544 ===
                                        Case ECMeasureType.mtDirect
                                            Dim Value As Double = -1    ' D1908

                                            If String2Double(sValue, Value) AndAlso Value <= 1 Then    ' D1908 + D2609
                                                Dim fUndef As Boolean = Value < 0

                                                Dim DirectAction As clsOneAtATimeEvaluationActionData = CType(CurAction.ActionData, clsOneAtATimeEvaluationActionData)
                                                Dim ParentNode As clsNode = DirectAction.Node
                                                Dim nonpwData As clsNonPairwiseMeasureData = CType(DirectAction.Judgment, clsNonPairwiseMeasureData)
                                                Dim ChildNode As clsNode

                                                If DirectAction.Node.IsAlternative Then
                                                    If DirectAction.IsEdge Then
                                                        ParentNode = DirectAction.Node
                                                        ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                                                    Else
                                                        ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
                                                        ChildNode = DirectAction.Node
                                                    End If
                                                Else
                                                    ParentNode = DirectAction.Node
                                                    If ParentNode.IsTerminalNode Then
                                                        ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                                                    Else
                                                        ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(nonpwData.NodeID)
                                                    End If
                                                End If

                                                If ParentNode IsNot Nothing And ChildNode IsNot Nothing Then
                                                    If DirectAction.Node.IsAlternative Then
                                                        If DirectAction.IsEdge Then
                                                            nonpwData = CType(ChildNode.EventsJudgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                        Else
                                                            nonpwData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                        End If
                                                    Else
                                                        nonpwData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    End If

                                                    Dim CurJudgment As clsDirectMeasureData = nonpwData
                                                    Dim NewJudgment As clsDirectMeasureData = Nothing

                                                    If CurJudgment IsNot Nothing Then
                                                        'If Not (CurJudgment.IsUndefined And fUndef) Then ' if was undefined and became undefined, no changes   ' -D2757
                                                        If (CurJudgment.IsUndefined <> fUndef) OrElse (CurJudgment.IsUndefined = fUndef) And ((CurJudgment.DirectData <> Value) Or (CurJudgment.Comment <> sComment)) Then ' if there's no changes, do nothing, otherwise change judgment 'C1002
                                                            Dim WasUndefined As Boolean = CurJudgment.IsUndefined
                                                            ' create new judgment
                                                            If fUndef Then
                                                                ' setting undefined
                                                                NewJudgment = New clsDirectMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, -1, True, sComment)    ' D2757
                                                            Else
                                                                ' set new rating
                                                                NewJudgment = New clsDirectMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Value, False, sComment)
                                                            End If
                                                            If NewJudgment IsNot Nothing Then
                                                                NewJudgment.ModifyDate = Now ' D2577

                                                                If DirectAction.Node.IsAlternative Then
                                                                    If DirectAction.IsEdge Then
                                                                        ChildNode.EventsJudgments.AddMeasureData(NewJudgment, False)
                                                                    Else
                                                                        ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewJudgment, False)
                                                                    End If
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewJudgment, False)
                                                                End If

                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                UpdateCachedProgress(UID, NewJudgment.IsUndefined, WasUndefined)  ' D4003
                                                            End If
                                                            'End If
                                                            'End If
                                                        End If
                                                    Else
                                                        If Not fUndef Then
                                                            ' if there's no judgment, but user add it
                                                            NewJudgment = New clsDirectMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Value, False, sComment)
                                                            If NewJudgment IsNot Nothing Then
                                                                NewJudgment.ModifyDate = Now    ' D2577

                                                                If DirectAction.Node.IsAlternative Then
                                                                    If DirectAction.IsEdge Then
                                                                        ChildNode.EventsJudgments.AddMeasureData(NewJudgment, False)
                                                                    Else
                                                                        ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewJudgment, False)
                                                                    End If
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewJudgment, False)
                                                                End If

                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                mMadeJudgments.Item(UID) += 1
                                                            End If
                                                        End If
                                                    End If

                                                    If UID = ProjectManager.UserID And NewJudgment IsNot Nothing Then
                                                        DirectAction.Judgment = NewJudgment
                                                    End If
                                                End If

                                                fResult = True
                                            End If


                                            ' D1549 ===
                                        Case ECMeasureType.mtStep
                                            Dim Value As Double = -1    ' D1908

                                            If String2Double(sValue, Value) Then    ' D1908
                                                Dim fUndef As Boolean = Value <= TeamTimeFuncs.UndefinedValue   ' D1579 + D1903

                                                Dim SFAction As clsOneAtATimeEvaluationActionData = CType(CurAction.ActionData, clsOneAtATimeEvaluationActionData)
                                                Dim ParentNode As clsNode = SFAction.Node
                                                Dim nonpwData As clsNonPairwiseMeasureData = CType(SFAction.Judgment, clsNonPairwiseMeasureData)
                                                Dim ChildNode As clsNode

                                                If SFAction.Node.IsAlternative Then
                                                    ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
                                                    ChildNode = SFAction.Node
                                                Else
                                                    ParentNode = SFAction.Node
                                                    If ParentNode.IsTerminalNode Then
                                                        ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                                                    Else
                                                        ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(nonpwData.NodeID)
                                                    End If
                                                End If

                                                If ParentNode IsNot Nothing And ChildNode IsNot Nothing Then
                                                    Dim MS As clsStepFunction
                                                    If SFAction.Node.IsAlternative Then
                                                        MS = ChildNode.MeasurementScale
                                                        nonpwData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    Else
                                                        MS = ParentNode.MeasurementScale
                                                        nonpwData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    End If

                                                    Dim CurJudgment As clsStepMeasureData = nonpwData
                                                    Dim NewJudgment As clsStepMeasureData = Nothing

                                                    If CurJudgment IsNot Nothing Then
                                                        'If Not (CurJudgment.IsUndefined And fUndef) Then ' if was undefined and became undefined, no changes   ' -D2757
                                                        If (CurJudgment.IsUndefined <> fUndef) OrElse (CurJudgment.IsUndefined = fUndef) And ((CurJudgment.Value <> Value) Or (CurJudgment.Comment <> sComment)) Then ' if there's no changes, do nothing, otherwise change judgment 'C1002
                                                            Dim WasUndefined As Boolean = CurJudgment.IsUndefined
                                                            ' create new judgment
                                                            If fUndef Then
                                                                ' setting undefined
                                                                NewJudgment = New clsStepMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, -1, CurJudgment.StepFunction, True, sComment)    ' D2757
                                                            Else
                                                                ' set new value
                                                                NewJudgment = New clsStepMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Value, CurJudgment.StepFunction, False, sComment)
                                                            End If
                                                            If NewJudgment IsNot Nothing Then
                                                                NewJudgment.ModifyDate = Now    ' D2577
                                                                If SFAction.Node.IsAlternative Then
                                                                    ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewJudgment, False)
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewJudgment, False)
                                                                End If
                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                UpdateCachedProgress(UID, NewJudgment.IsUndefined, WasUndefined)  ' D4003
                                                            End If
                                                            'End If
                                                            'End If
                                                        End If
                                                    Else
                                                        If Not fUndef Then
                                                            ' if there's no judgment, but user add it
                                                            NewJudgment = New clsStepMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Value, CType(SFAction.MeasurementScale, clsStepFunction), False, sComment)
                                                            If NewJudgment IsNot Nothing Then
                                                                NewJudgment.ModifyDate = Now    ' D2577
                                                                If SFAction.Node.IsAlternative Then
                                                                    ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewJudgment, False)
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewJudgment, False)
                                                                End If
                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                mMadeJudgments.Item(UID) += 1
                                                            End If
                                                        End If
                                                    End If

                                                    If UID = ProjectManager.UserID And NewJudgment IsNot Nothing Then
                                                        SFAction.Judgment = NewJudgment
                                                    End If
                                                End If

                                                fResult = True
                                            End If
                                            ' D1549 ==


                                            ' D1578 ===
                                        Case ECMeasureType.mtRegularUtilityCurve
                                            Dim Value As Double = -1    ' D1908

                                            If String2Double(sValue, Value) Then    ' D1908
                                                Dim fUndef As Boolean = Value <= TeamTimeFuncs.UndefinedValue ' D1579 + D1903

                                                Dim UCAction As clsOneAtATimeEvaluationActionData = CType(CurAction.ActionData, clsOneAtATimeEvaluationActionData)
                                                Dim ParentNode As clsNode = UCAction.Node
                                                Dim nonpwData As clsNonPairwiseMeasureData = CType(UCAction.Judgment, clsNonPairwiseMeasureData)
                                                Dim ChildNode As clsNode
                                                If UCAction.Node.IsAlternative Then
                                                    ParentNode = ProjectManager.ActiveObjectives.Nodes(0)
                                                    ChildNode = UCAction.Node
                                                Else
                                                    ParentNode = UCAction.Node
                                                    If ParentNode.IsTerminalNode Then
                                                        ChildNode = ProjectManager.ActiveAlternatives.GetNodeByID(nonpwData.NodeID)
                                                    Else
                                                        ChildNode = ProjectManager.ActiveObjectives.GetNodeByID(nonpwData.NodeID)
                                                    End If
                                                End If

                                                If ParentNode IsNot Nothing And ChildNode IsNot Nothing Then
                                                    Dim MS As clsRegularUtilityCurve
                                                    If UCAction.Node.IsAlternative Then
                                                        MS = ChildNode.MeasurementScale
                                                        nonpwData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    Else
                                                        MS = ParentNode.MeasurementScale
                                                        nonpwData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, UID)
                                                    End If

                                                    Dim CurJudgment As clsUtilityCurveMeasureData = nonpwData
                                                    Dim NewJudgment As clsUtilityCurveMeasureData = Nothing

                                                    If CurJudgment IsNot Nothing Then
                                                        'If Not (CurJudgment.IsUndefined And fUndef) Then ' if was undefined and became undefined, no changes   ' -D2757
                                                        If (CurJudgment.IsUndefined <> fUndef) OrElse (CurJudgment.IsUndefined = fUndef) And ((CurJudgment.ObjectValue <> Value) Or (CurJudgment.Comment <> sComment)) Then ' if there's no changes, do nothing, otherwise change judgment 'C1002
                                                            Dim WasUndefined As Boolean = CurJudgment.IsUndefined
                                                            ' create new judgment
                                                            If fUndef Then
                                                                ' setting undefined
                                                                NewJudgment = New clsUtilityCurveMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, -1, CurJudgment.UtilityCurve, True, sComment)    ' D2757
                                                            Else
                                                                ' set new value
                                                                NewJudgment = New clsUtilityCurveMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Value, CurJudgment.UtilityCurve, False, sComment)
                                                            End If
                                                            If NewJudgment IsNot Nothing Then
                                                                NewJudgment.ModifyDate = Now    ' D2577
                                                                If UCAction.Node.IsAlternative Then
                                                                    ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewJudgment, False)
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewJudgment, False)
                                                                End If
                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                UpdateCachedProgress(UID, NewJudgment.IsUndefined, WasUndefined)  ' D4003
                                                            End If
                                                            'End If
                                                            'End If
                                                        End If
                                                    Else
                                                        If Not fUndef Then
                                                            ' if there's no judgment, but user add it
                                                            NewJudgment = New clsUtilityCurveMeasureData(ChildNode.NodeID, ParentNode.NodeID, UID, Value, CType(UCAction.MeasurementScale, clsRegularUtilityCurve), False, sComment)
                                                            If NewJudgment IsNot Nothing Then
                                                                NewJudgment.ModifyDate = Now    ' D2577
                                                                If UCAction.Node.IsAlternative Then
                                                                    ChildNode.DirectJudgmentsForNoCause.AddMeasureData(NewJudgment, False)
                                                                Else
                                                                    ParentNode.Judgments.AddMeasureData(NewJudgment, False)
                                                                End If
                                                                ProjectManager.StorageManager.Writer.SaveUserJudgments(tUser)

                                                                mMadeJudgments.Item(UID) += 1
                                                            End If
                                                        End If
                                                    End If
                                                    If UID = ProjectManager.UserID And NewJudgment IsNot Nothing Then
                                                        UCAction.Judgment = NewJudgment
                                                    End If
                                                End If

                                                fResult = True
                                            End If
                                            ' D1578 ==
                                    End Select
                                    ' D1544 ==
                            End Select
                        End If
                    End If
                End If
            End If

        End If
        If fResult Then PipeBuiltLastModifyTime = ProjectManager.LastModifyTime ' D4542
        Return fResult
        ' D1275 ==
    End Function

    Public ReadOnly Property Pipe() As List(Of clsAction)
        Get
            Return PipeBuilder.Pipe
        End Get
    End Property


    Public Sub LoadUserData(ByVal user As clsUser)
        ProjectManager.StorageManager.Reader.LoadUserData(user)
    End Sub

    Public Sub AddUser(ByVal user As clsUser)
        If user IsNot Nothing AndAlso ProjectManager IsNot Nothing Then InitializeUserData(user, ProjectManager.User Is Nothing OrElse user.UserID <> ProjectManager.User.UserID) ' D4093 + D6445
    End Sub

    Private Sub InitializeUserData(ByVal user As clsUser, Optional DoLoad As Boolean = True)
        If DoLoad Then LoadUserData(user)
        CalculateTotalJudgments(user)
    End Sub

    Public Sub CreatePipe()
        PipeBuilder.IsSynchronousSession = True
        PipeBuilder.IsSynchronousSessionEvaluator = False
        PipeBuilder.SynchronousOwner = SessionOwner
        PipeBuilder.TeamTimePipe = Me

        ClearTempData(False)

        For Each user As clsUser In UsersList
            InitializeUserData(user, user.UserID <> ProjectManager.User.UserID)
        Next

        PipeBuilder.CreatePipe()
        If ProjectManager IsNot Nothing Then PipeBuiltLastModifyTime = ProjectManager.LastModifyTime ' D4542
    End Sub

    Public Sub ClearTempData(fResetUsers As Boolean)    ' D4548
        ClearStepData()
        mTotalJudgments.Clear()
        mMadeJudgments.Clear()
        mJudgmentDateTime.Clear()   ' D2577
        If fResetUsers Then Users.Clear() ' D4548
    End Sub

    Public Sub ClearStepData()
        mTempResults.Clear()
        mInconsistencies.Clear()
    End Sub

    Private Sub CalculateTotalJudgments(ByVal user As clsUser)
        Dim totalCount As Integer
        Dim madeCount As Integer

        Dim uList As New List(Of clsUser) From {user}
        Dim evalProgress As Dictionary(Of String, UserEvaluationProgressData) = ProjectManager.StorageManager.Reader.GetEvaluationProgress(uList, ProjectManager.ActiveHierarchy, madeCount, totalCount, False)

        If Not mTotalJudgments.ContainsKey(user.UserID) Then
            mTotalJudgments.Add(user.UserID, totalCount)
        Else
            mTotalJudgments(user.UserID) = totalCount
        End If

        If Not mMadeJudgments.ContainsKey(user.UserID) Then
            mMadeJudgments.Add(user.UserID, madeCount)
        Else
            mMadeJudgments(user.UserID) = madeCount
        End If
    End Sub

    Public Function SetCurrentStep(ByVal StepID As Integer, ByVal tSessionUsers As List(Of clsUser)) As Boolean
        If StepID < 0 Or StepID >= Pipe.Count Then
            Return False
        End If

        VerifyUsers(tSessionUsers)

        CurrentStep = StepID

        Dim CurAction As clsAction = Pipe(StepID)
        Select Case CurAction.ActionType
            Case Canvas.ActionType.atShowLocalResults
                ClearStepData()

                Dim WRTNode As clsNode = CType(CurAction.ActionData, clsShowLocalResultsActionData).ParentNode
                Dim children As List(Of clsNode) = WRTNode.GetNodesBelow(UNDEFINED_USER_ID)
                Dim IsAlternative As Boolean = WRTNode.IsTerminalNode

                Dim CanShowResults As Boolean

                Dim maxNormalized As Single
                Dim maxUnnormalized As Single

                For Each kvp As KeyValuePair(Of String, clsUser) In Users
                    Dim user As clsUser = kvp.Value
                    Dim UserTempRes As New List(Of clsTempResults)
                    mTempResults.Add(user.UserID, UserTempRes)

                    CanShowResults = CType(CurAction.ActionData, clsShowLocalResultsActionData).CanShowResultsForUser(user.UserID)

                    Dim UserCalcTarget As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)

                    If CanShowResults Then
                        WRTNode.CalculateLocal(user.UserID)
                    End If

                    maxNormalized = 0
                    maxUnnormalized = 0

                    For Each child As clsNode In children
                        Dim p As Single = 0
                        Dim up As Single = 0
                        If CanShowResults Then
                            If Not IsAlternative Then
                                If ProjectManager.HasCompleteHierarchy Then
                                    p = child.LocalPriority(UserCalcTarget, WRTNode)
                                    up = child.LocalPriorityUnnormalized(UserCalcTarget, WRTNode)
                                Else
                                    p = child.LocalPriority(user.UserID)
                                    up = child.LocalPriorityUnnormalized(user.UserID)
                                End If
                                UserTempRes.Add(New clsTempResults(child.NodeID, WRTNode.NodeID, IsAlternative, p, up))
                            Else
                                p = WRTNode.Judgments.Weights.GetUserWeights(user.UserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(child.NodeID)
                                up = WRTNode.Judgments.Weights.GetUserWeights(user.UserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(child.NodeID)
                                UserTempRes.Add(New clsTempResults(child.NodeID, WRTNode.NodeID, IsAlternative, p, up))
                            End If
                            If p > maxNormalized Then maxNormalized = p
                            If up > maxUnnormalized Then maxUnnormalized = up
                        Else
                            UserTempRes.Add(New clsTempResults(child.NodeID, WRTNode.NodeID, IsAlternative, -1, -1))
                        End If
                    Next

                    For Each tmpRes As clsTempResults In UserTempRes
                        If maxNormalized > 0 Then
                            tmpRes.PriorityPercentOfNormalized = tmpRes.Priority / maxNormalized
                        Else
                            tmpRes.PriorityPercentOfNormalized = 0
                        End If
                        If maxUnnormalized > 0 Then
                            tmpRes.PriorityPercentOfUnnormalized = tmpRes.UnnormalizedPriority / maxUnnormalized
                        Else
                            tmpRes.PriorityPercentOfUnnormalized = 0
                        End If
                    Next

                    If CanShowResults And WRTNode.MeasureType = ECMeasureType.mtPairwise Then
                        mInconsistencies.Add(user.UserID, CType(WRTNode.Judgments, clsPairwiseJudgments).EigenCalcs.InconIndex)
                    End If
                Next

                Dim CalcTarget As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
                'WRTNode.Judgments.CombineJudgments()
                ProjectManager.CalculationsManager.CreateCombinedJudgments(ProjectManager.CombinedGroups.GetDefaultCombinedGroup)

                CanShowResults = CType(CurAction.ActionData, clsShowLocalResultsActionData).CanShowResultsForUser(CalcTarget.GetUserID)

                If CanShowResults Then
                    WRTNode.CalculateLocal(CalcTarget)
                End If

                Dim CombinedTempRes As New List(Of clsTempResults)
                mTempResults.Add(CType(CalcTarget.Target, clsCombinedGroup).CombinedUserID, CombinedTempRes)

                maxNormalized = 0
                maxUnnormalized = 0
                For Each child As clsNode In children
                    Dim p As Single = 0
                    Dim up As Single = 0
                    If CanShowResults Then
                        If Not IsAlternative Then
                            If ProjectManager.HasCompleteHierarchy Then
                                p = child.LocalPriority(CalcTarget, WRTNode)
                                up = child.LocalPriorityUnnormalized(CalcTarget, WRTNode)
                            Else
                                p = child.LocalPriority(CalcTarget)
                                up = child.LocalPriorityUnnormalized(CalcTarget)
                            End If
                            CombinedTempRes.Add(New clsTempResults(child.NodeID, WRTNode.NodeID, IsAlternative, p, up))
                        Else
                            p = WRTNode.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(child.NodeID)
                            up = WRTNode.Judgments.Weights.GetUserWeights(CalcTarget.GetUserID, ProjectManager.CalculationsManager.SynthesisMode, ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(child.NodeID)
                            CombinedTempRes.Add(New clsTempResults(child.NodeID, WRTNode.NodeID, IsAlternative, p, up))
                        End If
                        If p > maxNormalized Then maxNormalized = p
                        If up > maxUnnormalized Then maxUnnormalized = up
                    Else
                        CombinedTempRes.Add(New clsTempResults(child.NodeID, WRTNode.NodeID, IsAlternative, -1, -1))
                    End If
                Next
                For Each tmpRes As clsTempResults In CombinedTempRes
                    If maxNormalized > 0 Then
                        tmpRes.PriorityPercentOfNormalized = tmpRes.Priority / maxNormalized
                    Else
                        tmpRes.PriorityPercentOfNormalized = 0
                    End If
                    If maxUnnormalized > 0 Then
                        tmpRes.PriorityPercentOfUnnormalized = tmpRes.UnnormalizedPriority / maxUnnormalized
                    Else
                        tmpRes.PriorityPercentOfUnnormalized = 0
                    End If
                Next
            Case Canvas.ActionType.atShowGlobalResults
                ClearStepData()

                Dim WRTNode As clsNode = If(WRTNodeID = -1, ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0), ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(WRTNodeID))
                Dim Alts As List(Of clsNode) = ProjectManager.UsersRoles.GetAllowedAlternatives(UNDEFINED_USER_ID, WRTNode)

                Dim CalcTarget As clsCalculationTarget

                Dim sum As Single
                Dim maxNormalized As Single = 0
                Dim maxUnnormalized As Single = 0

                For Each kvp As KeyValuePair(Of String, clsUser) In Users
                    Dim user As clsUser = kvp.Value
                    Dim UserTempRes As New List(Of clsTempResults)
                    mTempResults.Add(user.UserID, UserTempRes)

                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                    ProjectManager.CalculationsManager.Calculate(CalcTarget, WRTNode)

                    sum = 0
                    maxNormalized = 0
                    maxUnnormalized = 0

                    For Each alt As clsNode In Alts
                        'UserTempRes.Add(New clsTempResults(alt.NodeID, WRTNode.NodeID, True, alt.WRTGlobalPriority, alt.UnnormalizedPriority))
                        'sum += alt.WRTGlobalPriority
                        UserTempRes.Add(New clsTempResults(alt.NodeID, WRTNode.NodeID, True, alt.WRTGlobalPriority, alt.UnnormalizedPriority))
                        If alt.WRTGlobalPriority > maxNormalized Then maxNormalized = alt.WRTGlobalPriority
                        If alt.UnnormalizedPriority > maxUnnormalized Then maxUnnormalized = alt.UnnormalizedPriority
                        sum += alt.WRTGlobalPriority
                    Next

                    If sum <> 0 Then
                        For Each TempRes As clsTempResults In UserTempRes
                            TempRes.Priority /= sum
                            If maxNormalized > 0 Then
                                TempRes.PriorityPercentOfNormalized = TempRes.Priority / maxNormalized
                            Else
                                TempRes.PriorityPercentOfNormalized = 0
                            End If
                            If maxUnnormalized > 0 Then
                                TempRes.PriorityPercentOfUnnormalized = TempRes.UnnormalizedPriority / maxUnnormalized
                            Else
                                TempRes.PriorityPercentOfUnnormalized = 0
                            End If
                        Next
                    End If
                Next

                CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, ProjectManager.CombinedGroups.GetDefaultCombinedGroup)
                ProjectManager.CalculationsManager.Calculate(CalcTarget, WRTNode)

                Dim CombinedTempRes As New List(Of clsTempResults)
                mTempResults.Add(CType(CalcTarget.Target, clsCombinedGroup).CombinedUserID, CombinedTempRes)

                sum = 0
                maxNormalized = 0
                maxUnnormalized = 0

                For Each alt As clsNode In Alts
                    CombinedTempRes.Add(New clsTempResults(alt.NodeID, WRTNode.NodeID, True, alt.WRTGlobalPriority, alt.UnnormalizedPriority))
                    sum += alt.WRTGlobalPriority
                    If alt.WRTGlobalPriority > maxNormalized Then maxNormalized = alt.WRTGlobalPriority
                    If alt.UnnormalizedPriority > maxUnnormalized Then maxUnnormalized = alt.UnnormalizedPriority
                Next

                If sum <> 0 Then
                    For Each TempRes As clsTempResults In CombinedTempRes
                        TempRes.Priority /= sum
                        If maxNormalized > 0 Then
                            TempRes.PriorityPercentOfNormalized = TempRes.Priority / maxNormalized
                        Else
                            TempRes.PriorityPercentOfNormalized = 0
                        End If
                        If maxUnnormalized > 0 Then
                            TempRes.PriorityPercentOfUnnormalized = TempRes.UnnormalizedPriority / maxUnnormalized
                        Else
                            TempRes.PriorityPercentOfUnnormalized = 0
                        End If
                    Next
                End If
        End Select

        Return True
    End Function

    Public Function GetTeamTimeStep(ByVal CovObjID As Integer, ByVal ChildID As Integer) As Integer
        Dim action As clsAction
        For i As Integer = 0 To Pipe.Count - 1
            action = CType(Pipe(i), clsAction)
            Select Case action.ActionType
                Case ActionType.atNonPWOneAtATime
                    Dim J As clsNonPairwiseMeasureData = CType(CType(action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsNonPairwiseMeasureData)
                    Dim tNode As clsNode = CType(action.ActionData, clsOneAtATimeEvaluationActionData).Node ' D3366
                    If (J.ParentNodeID = CovObjID OrElse (CovObjID = -2 AndAlso tNode IsNot Nothing AndAlso tNode.IsAlternative)) AndAlso J.NodeID = ChildID Then    ' D3366
                        Return i
                    End If
                Case ActionType.atPairwise ' return the first pairwise step in the pipe for this cluster
                    Dim J As clsPairwiseMeasureData = CType(action.ActionData, clsPairwiseMeasureData)
                    If (J.ParentNodeID = CovObjID) AndAlso (J.FirstNodeID = ChildID Or J.SecondNodeID = ChildID) Then
                        Return i
                    End If
            End Select
        Next
        Return -1
    End Function

    Public Sub New(ByVal ProjectManager As clsProjectManager, ByVal SessionOwner As clsUser)
        Me.ProjectManager = ProjectManager
        Me.SessionOwner = SessionOwner
        Me.PipeBuilder = New clsPipeBuilder(ProjectManager)
        Me.PipeBuilder.PipeParameters = PipeParameters
        Me.PipeBuilder.ProjectParameters = ProjectManager.Parameters
    End Sub

End Class