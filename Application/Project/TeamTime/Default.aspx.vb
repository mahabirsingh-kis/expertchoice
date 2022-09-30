Option Strict Off

Imports DevExpress.Web.ASPxCallback
Imports Telerik.Web.UI

Partial Class TeamTimeEvaluationPage
    Inherits clsComparionCorePage

#Const OPT_USE_AJAX_INSTEAD_CALLBACK = False    ' D3998

    Private _SessionsList As List(Of clsOnlineUserSession) = Nothing
    Private _UsersList As List(Of clsApplicationUser) = Nothing ' D1243
    Private _TeamTimeUsersList As List(Of clsUser) = Nothing    ' D1271
    Private _TeamTimeUsersListAll As List(Of clsUser) = Nothing    ' D4835

    Private _TeamTimeRefreshTimeout As Integer = SynchronousRefresh()
    Private _TeamTimeRefreshTimeoutLoaded As Boolean = False

    Private _TeamTimePipeStep As Integer = -1       ' D1246
    Public _TeamTimeActive As Boolean = True        ' D1275
    Public Const ShowStepsCount As Integer = 11     ' D1246 + D1270 + D1653 + D1657 + D1690

    Public Const _OPT_SHOW_ONLINE_ONLY_PROJECT As Boolean = False   ' D3493

    'Private _TeamTimeStepsList As String() = Nothing                ' D2995
    'Private _TeamTimeProgressList As ecPipeStepStatus() = Nothing   ' D2995

    Public Const _SESS_VARIANCE As String = "tt_variance"           ' D1654
    Public Const _SESS_PW_SIDE As String = "tt_pw_side"             ' D1658
    Public Const _SESS_HIDE_COMBINED As String = "tt_no_combined"   ' D1799
    Public Const _SESS_STEPS_LIST As String = "tt_steps_list"       ' D2995
    Public Const _COOKIE_USERSPAGE As String = "tt_pg"              ' D2622
    Public Const _COOKIE_USERS_PAGES_USE As String = "tt_up"        ' D2622
    Public Const _COOKIE_USERS_PAGES_SIZE As String = "tt_ps"       ' D2622
    'Public Const _COOKIE_TT_NORM As String = "tt_res_norm"          ' D3026 // D7259 - use dynamic peroperty below

    Private Const _OPT_STEPS_PAGE_SIZE As Integer = 150 ' D3074
    Private Const _OPT_STEPS_SHOW_MAX As Integer = 100  ' D3074

    Private Const _OPT_PM_STEPS_COLOR As Boolean = False   ' D3287  '' AD: see D3290 for revert back ciolor right top corner triangle mark

    Private Const _TeamTimeSessionDataTimeoutCoeff As Integer = 10  ' D1254 + D1973
    Private _ForceSaveStepInfo As Boolean = False   ' D1353

    Private _TeamTime_Pipe As clsTeamTimePipe = Nothing         ' D1270 + D1287

    Private _TeamTimeOldPipeParams As ParameterSet = Nothing    ' D1974
    Private _Orignal_HID As ECHierarchyID = ECHierarchyID.hidImpact ' D1953

    Private sCallbackSave As String = ""    ' D2578


#Region "Enum"

    ' D2637 ===
    Public Enum ecPipeStepStatus
        psNoJudgments = 0
        psMissingJudgments = 1
        psAllJudgments = 2
    End Enum
    ' D2637 ==

#End Region

#Region "Properties"

    Public ReadOnly Property isTeamTime() As Boolean
        Get
            Return App.ActiveProject.isTeamTimeLikelihood OrElse App.ActiveProject.isTeamTimeImpact ' D1953
        End Get
    End Property

    ' D1926 ===
    Public ReadOnly Property isImpact() As Boolean
        Get
            Return PM.ActiveHierarchy = ECHierarchyID.hidImpact ' D3066 + D4093
        End Get
    End Property

    Public ReadOnly Property isLikelihood() As Boolean
        Get
            Return PM.ActiveHierarchy = ECHierarchyID.hidLikelihood ' D3066 + D4093
        End Get
    End Property
    ' D1926 ==

    ' D4093 ===
    Public ReadOnly Property PM As clsProjectManager
        Get
            If isTeamTimeOwner Then Return TeamTime.ProjectManager Else Return App.ActiveProject.ProjectManager
        End Get
    End Property
    ' D4093 ==

    Public ReadOnly Property isTeamTimeOwner() As Boolean
        Get
            If isTeamTime Then
                Return App.ActiveProject.MeetingStatus(App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner
            Else
                If _LOGIN_WITH_MEETINGID_TO_INACTIVE_MEETING AndAlso (App.Options.isLoggedInWithMeetingID OrElse App.Options.OnlyTeamTimeEvaluation) Then Return False
            End If
            Return False    ' D1247
        End Get
    End Property

    Public ReadOnly Property UsingTeamTimeKeypads() As Boolean
        Get
            Return TeamTimePipeParams.SynchUseVotingBoxes AndAlso App.Options.KeypadsAvailable    ' D1561 + D6418 + D6429
        End Get
    End Property


    Public ReadOnly Property isTeamTimeOwnerOnline() As Boolean
        Get
            Dim fAllowed As Boolean = False
            If Not App.ActiveProject.MeetingOwner Is Nothing Then
                fAllowed = Not clsOnlineUserSession.OnlineSessionByUserIDProjectID(App.ActiveProject.MeetingOwnerID, App.ProjectID, SessionsList) Is Nothing
            End If
            Return fAllowed
        End Get
    End Property

    Public ReadOnly Property SessionsList() As List(Of clsOnlineUserSession)
        Get
            If _SessionsList Is Nothing Then
                DebugInfo("Get sessions list")
                ' D1243 ===
                _SessionsList = New List(Of clsOnlineUserSession)
                Dim _AllOnline As List(Of clsOnlineUserSession) = App.DBOnlineSessions
                For Each tSess As clsOnlineUserSession In _AllOnline
                    If clsApplicationUser.UserByUserID(tSess.UserID, UsersList) IsNot Nothing AndAlso (Not _OPT_SHOW_ONLINE_ONLY_PROJECT OrElse tSess.ProjectID = App.ProjectID) Then _SessionsList.Add(tSess) ' D3493
                Next
                ' D1243 ==
            End If
            Return _SessionsList
        End Get
    End Property

    ' D1243 ===
    Private ReadOnly Property UsersList() As List(Of clsApplicationUser)
        Get
            If _UsersList Is Nothing Then
                DebugInfo("Get users list from DB")
                _UsersList = App.DBUsersByProjectID(App.ProjectID)
            End If
            Return _UsersList
        End Get
    End Property
    ' D1243 ==

    ' D1271 ===
    Private ReadOnly Property TeamTimeUsersList() As List(Of clsUser)
        Get
            If _TeamTimeUsersList Is Nothing Then
                DebugInfo("Prepare users list")
                _TeamTimeUsersList = New List(Of clsUser)
                _TeamTimeUsersListAll = New List(Of clsUser)    ' D4825
                Dim tSessOwner As clsUser = Nothing ' D4002
                Dim tWSList As List(Of clsWorkspace) = App.DBWorkspacesByProjectID(App.ProjectID)
                ' D4369 ===
                Dim EvalGrpIDs As New List(Of Integer)
                For Each tGrp As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
                    If tGrp.ActionStatus(ecActionType.at_mlEvaluateModel) = ecActionStatus.asGranted Then EvalGrpIDs.Add(tGrp.ID)
                Next
                ' D4369 ==
                For Each tAppUser As clsApplicationUser In UsersList
                    Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, App.ProjectID, tWSList)
                    If tWS IsNot Nothing AndAlso clsOnlineUserSession.OnlineSessionByUserID(tWS.UserID, SessionsList) IsNot Nothing Then tWS.Status(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive ' D1432 + D1945
                    If tWS IsNot Nothing AndAlso (IsConsensus OrElse tWS.isInTeamTime(App.ActiveProject.isImpact)) Then  ' D1945 + D4455 + D4635
                        ' D1288 + D4369 ===
                        Dim fCanParticipate As Boolean = EvalGrpIDs.Contains(tWS.GroupID)
                        If fCanParticipate AndAlso Not TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess AndAlso tWS.TeamTimeStatus(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousReadOnly Then fCanParticipate = False ' D1945
                        If fCanParticipate AndAlso TeamTimePipeParams.TeamTimeHideProjectOwner AndAlso App.ActiveProject.MeetingOwnerID = tAppUser.UserID Then fCanParticipate = False
                        ' D4369 ==
                        Dim fUpdate As Boolean = False
                        If fCanParticipate OrElse App.ActiveProject.MeetingOwnerID = tAppUser.UserID Then   ' D4002
                            ' D1288 ==
                            Dim tPrjUser As clsUser = PM.GetUserByEMail(tAppUser.UserEmail)
                            If tPrjUser Is Nothing Then
                                tPrjUser = PM.AddUser(tAppUser.UserEmail, True, tAppUser.UserName) ' D1325
                                tPrjUser.IncludedInSynchronous = True
                                tPrjUser.SyncEvaluationMode = SynchronousEvaluationMode.semOnline
                                fUpdate = True
                            Else
                                If Not tPrjUser.IncludedInSynchronous OrElse tPrjUser.SyncEvaluationMode = SynchronousEvaluationMode.semNone Then   ' D1362
                                    tPrjUser.IncludedInSynchronous = True
                                    If tPrjUser.SyncEvaluationMode = SynchronousEvaluationMode.semNone Then tPrjUser.SyncEvaluationMode = SynchronousEvaluationMode.semOnline ' D1362
                                    fUpdate = True
                                End If
                            End If
                            If tPrjUser IsNot Nothing AndAlso App.ActiveProject.MeetingOwnerID = tAppUser.UserID Then tSessOwner = tPrjUser.Clone ' D4002
                            If fCanParticipate Then ' D4002
                                If fUpdate Then PM.StorageManager.Writer.SaveModelStructure() ' D1325
                                Dim tAddUser As clsUser = tPrjUser.Clone
                                tAddUser.UserName = tPrjUser.UserName   ' D1639                            
                                tAddUser.Active = clsOnlineUserSession.OnlineSessionByUserID(tAppUser.UserID, SessionsList) IsNot Nothing
                                If Not (Not TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess AndAlso tPrjUser IsNot Nothing AndAlso tPrjUser.SyncEvaluationMode = SynchronousEvaluationMode.semByFacilitatorOnly) Then ' D1605
                                    If Not PM.Parameters.TTHideOfflineUsers OrElse tAddUser.Active OrElse tAddUser.UserID = App.ActiveUser.UserID Then    ' D3929
                                        _TeamTimeUsersList.Add(tAddUser)
                                    End If
                                    _TeamTimeUsersListAll.Add(tAddUser) ' D4825
                                End If
                            End If
                        End If
                    End If
                Next

                'If _TeamTimeUsersList.Count = 0 AndAlso tSessOwner IsNot Nothing Then _TeamTimeUsersList.Add(tSessOwner) ' D4002

                ' D1561 ===
                DebugInfo("Sort users list")
                Dim Fld As ecUserSort = ecUserSort.usEmail
                Select Case TeamTimePipeParams.TeamTimeUsersSorting
                    Case TTUsersSorting.ttusName
                        Fld = ecUserSort.usName
                    Case TTUsersSorting.ttusKeypadID
                        Fld = ecUserSort.usKeyPad
                    Case Else
                        Fld = ecUserSort.usEmail
                End Select
                Dim cmp As New clsUserComparer(Fld, SortDirection.Ascending)
                _TeamTimeUsersList.Sort(cmp)
                _TeamTimeUsersListAll.Sort(cmp) ' D4825
                ' D1561 ==

                ' D4001 ===
                For Each tUser As clsUser In PM.UsersList
                    If tUser.UserID <> PM.UserID AndAlso clsApplicationUser.AHPUserByUserID(tUser.UserID, _TeamTimeUsersList) Is Nothing Then
                        PM.CleanUpUserDataFromMemory(PM.ActiveObjectives.HierarchyID, tUser.UserID, , True)
                    End If
                Next
                PM.CleanUpUserDataFromMemory(PM.ActiveObjectives.HierarchyID, COMBINED_USER_ID, , True)
                ' D4001 ==

            End If
            Return _TeamTimeUsersList
        End Get
    End Property

    Private ReadOnly Property TeamTimeUsersListNonEmpty As List(Of clsUser)
        Get
            Dim tList As List(Of clsUser) = TeamTimeUsersList
            If TeamTimeUsersList.Count = 0 Then
                Dim tAppUser As clsApplicationUser = clsApplicationUser.UserByUserID(App.ActiveProject.MeetingOwnerID, UsersList)
                If tAppUser IsNot Nothing Then tList.Add(PM.GetUserByEMail(tAppUser.UserEmail))
                Return tList
            End If
            Return tList
        End Get
    End Property
    ' D1271 ==

    ' D1654 ===
    Public Property TeamTimeShowVarianceInPollingMode As Boolean
        Set(value As Boolean)
            SessVar(_SESS_VARIANCE) = CStr(IIf(value, "1", "0"))
        End Set
        Get
            Return SessVar(_SESS_VARIANCE) = "1"
        End Get
    End Property
    ' D1654 ==

    ' D1658 ===
    Public Property TeamTimeShowPWSideKeypadsInPollingMode As Boolean
        Set(value As Boolean)
            SessVar(_SESS_PW_SIDE) = CStr(IIf(value, "1", "0"))
        End Set
        Get
            Return SessVar(_SESS_PW_SIDE) = "1"
        End Get
    End Property
    ' D1658 ==

    ' D1799 ===
    Public Property TeamTimeHideCombined As Boolean
        Set(value As Boolean)
            SessVar(_SESS_HIDE_COMBINED) = CStr(IIf(value, "1", "0"))
        End Set
        Get
            Return SessVar(_SESS_HIDE_COMBINED) = "1"
        End Get
    End Property
    ' D1799 ==

    ' D1973 ===
    Public ReadOnly Property TeamTimeMinimumRefreshTimeout() As Integer
        Get
            If isTeamTimeOwner AndAlso TeamTimeUsersList IsNot Nothing AndAlso TeamTimeUsersList.Count > 0 Then Return CInt(Math.Min(Math.Ceiling(TeamTimeUsersList.Count / 20), 5) + 1) ' D2622 + D3066
            Return 2
        End Get
    End Property
    ' D1973 ==

    '' D2966 ===
    'Public ReadOnly Property TeamTimeInfodocMode As Integer
    '    Get
    '        If TeamTimePipeParams.ShowInfoDocs Then
    '            Dim tVal As Integer = CInt(IIf(isMobileBrowser, ShowInfoDocsMode.sidmPopup, TeamTimePipeParams.ShowInfoDocsMode))
    '            Dim sVal As String = GetCookie("tt_infodocmode", CStr(tVal))
    '            Integer.TryParse(sVal, tVal)
    '            Return tVal
    '        Else
    '            Return -1
    '        End If
    '    End Get
    'End Property
    '' D2966 ==

    Public Property TeamTimeRefreshTimeout() As Integer
        Get
            If Not _TeamTimeRefreshTimeoutLoaded Then
                DebugInfo("Get TT timeout")
                ' D1275 ===
                Dim DT As Nullable(Of DateTime)
                Dim sRefresh As String = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.RefreshTimeout, DT)
                If Not Integer.TryParse(sRefresh, _TeamTimeRefreshTimeout) Then _TeamTimeRefreshTimeout = SynchronousRefresh
                ' D1275 ==
                If _TeamTimeRefreshTimeout < TeamTimeMinimumRefreshTimeout Then _TeamTimeRefreshTimeout = TeamTimeMinimumRefreshTimeout ' D1973
                _TeamTimeRefreshTimeoutLoaded = True
            End If
            Return _TeamTimeRefreshTimeout
        End Get
        Set(value As Integer)
            If value < 1 Then value = 1
            If value > 30 Then value = 30
            If value <> _TeamTimeRefreshTimeout AndAlso isTeamTimeOwner Then
                DebugInfo("Set TT timeout")
                _TeamTimeRefreshTimeout = value
                App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.RefreshTimeout, CStr(value))   ' D1275
                _TeamTimeRefreshTimeoutLoaded = True
            End If
        End Set
    End Property

    ' D1246 ===
    Public Property TeamTimePipeStep() As Integer
        Get
            If _TeamTimePipeStep < 1 Then
                If isTeamTimeOwner Then
                    DebugInfo("Get TT step")
                    _TeamTimePipeStep = App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact) ' D1945
                    ' D1289 ===
                    If _TeamTimePipeStep < 1 Then _TeamTimePipeStep = 1
                    If _TeamTimePipeStep > TeamTimePipe.Count Then _TeamTimePipeStep = TeamTimePipe.Count
                    If _TeamTimePipeStep <> App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact) Then App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact) = _TeamTimePipeStep ' D1945
                    ' D1289 ==
                    'TeamTimeUpdateStepInfo(_TeamTimePipeStep)   ' D1348
                    TeamTime.SetCurrentStep(_TeamTimePipeStep - 1, TeamTimeUsersListNonEmpty)  ' D1270 + D4002
                    ' D1353 ===
                    If _ForceSaveStepInfo Then
                        TeamTimeUpdateStepInfo(_TeamTimePipeStep)
                        _ForceSaveStepInfo = False
                    End If
                    ' D1353 ==
                Else
                    Dim tWS As clsWorkspace = App.DBWorkspaceByUserIDProjectID(App.ActiveProject.MeetingOwnerID, App.ProjectID)
                    If tWS IsNot Nothing Then _TeamTimePipeStep = tWS.ProjectStep(App.ActiveProject.isImpact) ' D1945
                End If
                If _TeamTimePipeStep < 1 Then _TeamTimePipeStep = 1
            End If
            Return _TeamTimePipeStep
        End Get
        Set(value As Integer)
            If isTeamTimeOwner AndAlso _TeamTimePipeStep <> value Then
                DebugInfo("Set TT step")
                _TeamTimePipeStep = value
                App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact) = value ' D1945
                TeamTimeUpdateStepInfo(_TeamTimePipeStep)   ' D1348
                'App.DBWorkspaceUpdate(App.ActiveWorkspace, False, "")
                'TeamTime.SetCurrentStep(_TeamTimePipeStep - 1, TeamTimeUsersList)
            End If
        End Set
    End Property

    Public ReadOnly Property TeamTimePipe() As List(Of clsAction)
        Get
            Return TeamTime.Pipe    ' D1269
        End Get
    End Property

    ' D1276 ===
    Public ReadOnly Property TeamTimePipeParams() As clsPipeParamaters
        Get
            'If isTeamTimeOwner Then Return TeamTime.PipeParameters Else Return App.ActiveProject.PipeParameters ' D3066
            Return App.ActiveProject.PipeParameters ' D3066
        End Get
    End Property
    ' D1276 ==

    ' D1269 ===
    Public Property TeamTime() As clsTeamTimePipe
        Get
            If _TeamTime_Pipe Is Nothing Then
                If Session(_SESS_TT_Pipe) IsNot Nothing Then
                    DebugInfo("Get TT object")
                    _TeamTime_Pipe = CType(Session(_SESS_TT_Pipe), clsTeamTimePipe)
                    ' D6789 ===
                    Dim sPipePasscode As String = ""
                    If Session(_SESS_TT_Passcode) IsNot Nothing Then sPipePasscode = CStr(Session(_SESS_TT_Passcode))
                    If _TeamTime_Pipe.ProjectManager.StorageManager.ModelID <> App.ProjectID OrElse _TeamTime_Pipe.ProjectManager.ActiveHierarchy <> App.ActiveProject.ProjectManager.ActiveHierarchy OrElse sPipePasscode <> App.ActiveProject.Passcode(App.ActiveProject.isImpact) Then _TeamTime_Pipe = Nothing
                    ' D6789 ==
                End If
                If _TeamTime_Pipe Is Nothing Then
                    DebugInfo("Create TT object")
                    ' D6445 ===
                    If App.ActiveProject.ProjectManager.User Is Nothing Then
                        App.ActiveProject.ProjectManager.User = App.ActiveProject.ProjectManager.GetUserByEMail(App.ActiveUser.UserEmail)
                    End If
                    ' D6445 ==
                    _TeamTime_Pipe = New clsTeamTimePipe(App.ActiveProject.ProjectManager, App.ActiveProject.ProjectManager.User)
                    _TeamTime_Pipe.Override_ResultsMode = True  ' D1797
                    _TeamTime_Pipe.ResultsViewMode = ResultsView.rvBoth ' D1797
                    _TeamTime_Pipe.VerifyUsers(TeamTimeUsersListNonEmpty)
                    _TeamTime_Pipe.CreatePipe()
                    _ForceSaveStepInfo = True
                    '_TeamTimeStepsList = Nothing ' D2995
                    Session.Remove(_SESS_TT_Pipe)  ' D3066
                    Session.Remove(_SESS_TT_Passcode)   ' D6789
                    Session.Add(_SESS_TT_Pipe, _TeamTime_Pipe)
                    Session(_SESS_TT_Passcode) = App.ActiveProject.Passcode(App.ActiveProject.isImpact) ' D6789
                    Session.Remove(_SESS_STEPS_LIST)    ' D2995
                End If
            End If
            Return _TeamTime_Pipe
        End Get
        Set(value As clsTeamTimePipe)
            DebugInfo("Set TT object")
            _TeamTime_Pipe = value
            Session(_SESS_TT_Pipe) = _TeamTime_Pipe
            Session(_SESS_TT_Passcode) = If(value Is Nothing, "", App.ActiveProject.Passcode(App.ActiveProject.isImpact))   ' D6789
            Session.Remove(_SESS_STEPS_LIST)    ' D3066
        End Set
    End Property
    ' D1269 ==

    ' D3151 ===
    Public ReadOnly Property IsConsensus As Boolean
        Get
            Return CheckVar(_PARAM_ACTION, "").ToLower = "consensus" OrElse CheckVar("mode", "").ToLower = "consensus"
        End Get
    End Property
    ' D3151 ==

    ' D7259 ===
    Public ReadOnly Property _COOKIE_TT_NORM As String
        Get
            Return If(App.isRiskEnabled, "tt_res_norm_risk", "tt_res_norm")
        End Get
    End Property
    ' D7259 ==

    ' D4554 ===
    Public Property TeamTimeEvalProgressUser As String
        Get
            Dim User As String = TeamTime.ProjectManager.Parameters.TTEvaluationProgressUser
            If String.IsNullOrEmpty(User) OrElse clsApplicationUser.AHPUserByUserEmail(User, PM.UsersList) Is Nothing Then  ' D4591
                User = App.ActiveUser.UserEmail
                TeamTime.ProjectManager.Parameters.TTEvaluationProgressUser = User
            End If
            Return User
        End Get
        Set(value As String)
            If TeamTime.ProjectManager.Parameters.TTEvaluationProgressUser <> value Then
                TeamTime.ProjectManager.Parameters.TTEvaluationProgressUser = value
                TeamTime.ProjectManager.Parameters.Save()
            End If
        End Set
    End Property
    ' D4554 ==

    ' D4583 ===
    Private Function GetTeamTimeEvalProgressUserName() As String
        Dim sName As String = ""
        If (isTeamTimeOwner) Then
            Dim tUser As clsUser = PM.GetUserByEMail(TeamTimeEvalProgressUser)
            If tUser IsNot Nothing Then Return tUser.UserName
        End If
        Return sName
    End Function
    ' D4583 ==

    Public Function GetAction(stepNumber As Integer) As clsAction
        If stepNumber < 1 Or stepNumber > TeamTimePipe.Count Or TeamTimePipe.Count < 1 Then Return Nothing Else Return CType(TeamTimePipe(stepNumber - 1), clsAction)
    End Function

    Public Function isEvaluation(Action As clsAction) As Boolean  ' D1276
        If Action Is Nothing Then Return False
        Select Case Action.ActionType
            Case ActionType.atPairwise, ActionType.atNonPWOneAtATime, ActionType.atNonPWAllChildren, ActionType.atNonPWAllCovObjs, ActionType.atPairwiseOutcomes
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Function isUndefined(Action As clsAction) As Boolean
        If Action IsNot Nothing AndAlso Action.ActionData IsNot Nothing Then    ' D1306
            If _OPT_PM_STEPS_COLOR Then ' D3286

                Select Case Action.ActionType
                    Case ActionType.atPairwise
                        Return CType(Action.ActionData, clsPairwiseMeasureData).IsUndefined
                        ' D2652 ===
                    Case ActionType.atPairwiseOutcomes
                        Dim RS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                        Dim FirstRating As clsRating = RS.GetRatingByID(Action.ActionData.FirstNodeID)
                        Dim SecondRating As clsRating = RS.GetRatingByID(Action.ActionData.SecondNodeID)
                        Dim pwData As clsPairwiseMeasureData = Action.ParentNode.PWOutcomesJudgments.PairwiseJudgment(FirstRating.ID, SecondRating.ID, PM.UserID, , Action.PWONode.NodeID)
                        Return pwData Is Nothing OrElse pwData.IsUndefined
                        ' D2652 ==
                    Case ActionType.atNonPWOneAtATime
                        Return CType(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).Judgment, clsNonPairwiseMeasureData).IsUndefined
                    Case ActionType.atNonPWAllChildren
                        Dim fHasUndef As Boolean = False
                        Dim Data As clsAllChildrenEvaluationActionData = CType(Action.ActionData, clsAllChildrenEvaluationActionData)
                        If Data IsNot Nothing Then  ' D1306
                            For Each tAlt As clsNode In Data.Children
                                Dim MD As clsNonPairwiseMeasureData = CType(Data.GetJudgment(tAlt), clsNonPairwiseMeasureData)
                                If MD IsNot Nothing AndAlso MD.IsUndefined Then fHasUndef = True ' D1306
                            Next
                        End If
                        Return fHasUndef
                    Case ActionType.atNonPWAllCovObjs
                        Dim fHasUndef As Boolean = False
                        Dim Data As clsAllCoveringObjectivesEvaluationActionData = CType(Action.ActionData, clsAllCoveringObjectivesEvaluationActionData)
                        If Data IsNot Nothing Then  ' D1306
                            For Each tAlt As clsNode In Data.CoveringObjectives
                                Dim MD As clsNonPairwiseMeasureData = CType(Data.GetJudgment(tAlt), clsNonPairwiseMeasureData)
                                If MD IsNot Nothing AndAlso MD.IsUndefined Then fHasUndef = True ' D1306
                            Next
                        End If
                        Return fHasUndef
                End Select

                ' D3286 ===
            Else

                Return GetStepStatus(Action, TeamTimeUsersList) <> ecPipeStepStatus.psAllJudgments  ' D4554

            End If
            ' D3286 ==

        End If
        Return False
    End Function

    ' D2637 ===
    Private Function IsUserJudgmentAllowed(Action As clsAction, tUser As clsUser) As Boolean
        Return tUser IsNot Nothing AndAlso (tUser.SyncEvaluationMode = SynchronousEvaluationMode.semOnline OrElse tUser.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox) AndAlso _
               Action IsNot Nothing AndAlso TeamTime.GetStepAvailabilityForUser(Action.ActionType, Action, tUser)    ' D2638
        'Action IsNot Nothing AndAlso TeamTime.GetStepAvailabilityForUser(Action.ActionType, IIf(Action.ActionType = ActionType.atPairwiseOutcomes, Action, Action.ActionData), tUser)    ' D2638
    End Function

    Private Function GetStepStatus(Action As clsAction, UsersList As List(Of clsUser)) As ecPipeStepStatus  ' D4554
        Dim fStatus As ecPipeStepStatus = ecPipeStepStatus.psNoJudgments
        'Return fStatus

        Dim ParentNode As clsNode = Nothing
        Dim FirstNode As clsNode = Nothing
        Dim SecondNode As clsNode = Nothing
        Dim FirstRating As clsRating = Nothing
        Dim SecondRating As clsRating = Nothing
        Dim ChildNode As clsNode = Nothing
        Dim ObjHierarchy As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)
        Dim AltsHierarchy As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)

        If Action IsNot Nothing AndAlso Action.ActionData IsNot Nothing AndAlso UsersList IsNot Nothing Then

            Select Case Action.ActionType

                Case ActionType.atPairwise
                    Dim ActionData As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                    ParentNode = ObjHierarchy.GetNodeByID(ActionData.ParentNodeID)
                    If ParentNode IsNot Nothing Then    ' D2946
                        If ParentNode.IsTerminalNode Then
                            FirstNode = AltsHierarchy.GetNodeByID(ActionData.FirstNodeID)
                            SecondNode = AltsHierarchy.GetNodeByID(ActionData.SecondNodeID)
                        Else
                            FirstNode = ObjHierarchy.GetNodeByID(ActionData.FirstNodeID)
                            SecondNode = ObjHierarchy.GetNodeByID(ActionData.SecondNodeID)
                        End If
                    End If
                Case ActionType.atPairwiseOutcomes
                    Dim ActionData As clsPairwiseMeasureData = CType(Action.ActionData, clsPairwiseMeasureData)
                    If Action.PWONode IsNot Nothing Then    ' D2946
                        Dim RS As clsRatingScale = CType(Action.PWONode.MeasurementScale, clsRatingScale)
                        If RS IsNot Nothing Then    ' D2946
                            ParentNode = Action.ParentNode
                            FirstRating = RS.GetRatingByID(ActionData.FirstNodeID)
                            SecondRating = RS.GetRatingByID(ActionData.SecondNodeID)
                        End If
                    End If

                Case ActionType.atNonPWOneAtATime
                    Dim ActionData As clsOneAtATimeEvaluationActionData = CType(Action.ActionData, clsOneAtATimeEvaluationActionData)

                    If ActionData.Node IsNot Nothing AndAlso ActionData.Judgment IsNot Nothing Then    ' D2946
                        Select Case CType(Action.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType
                            Case ECMeasureType.mtRatings
                                Dim nonpwData As clsNonPairwiseMeasureData = CType(ActionData.Judgment, clsNonPairwiseMeasureData)
                                If ActionData.Node.IsAlternative Then
                                    ParentNode = ObjHierarchy.Nodes(0)
                                    ChildNode = ActionData.Node
                                Else
                                    ParentNode = ActionData.Node
                                    If ParentNode IsNot Nothing AndAlso ParentNode.IsTerminalNode Then ChildNode = AltsHierarchy.GetNodeByID(nonpwData.NodeID) Else ChildNode = ObjHierarchy.GetNodeByID(nonpwData.NodeID) ' D2946
                                End If

                            Case ECMeasureType.mtDirect
                                ParentNode = ActionData.Node
                                Dim DirectData As clsDirectMeasureData = CType(ActionData.Judgment, clsDirectMeasureData)
                                If ParentNode.IsTerminalNode Then ChildNode = AltsHierarchy.GetNodeByID(DirectData.NodeID) Else ChildNode = ObjHierarchy.GetNodeByID(DirectData.NodeID)

                            Case ECMeasureType.mtStep
                                ParentNode = ActionData.Node
                                Dim StepData As clsStepMeasureData = CType(ActionData.Judgment, clsStepMeasureData)
                                If ParentNode.IsTerminalNode Then ChildNode = AltsHierarchy.GetNodeByID(StepData.NodeID) Else ChildNode = ObjHierarchy.GetNodeByID(StepData.NodeID)

                            Case ECMeasureType.mtRegularUtilityCurve
                                ParentNode = ActionData.Node
                                Dim UCData As clsUtilityCurveMeasureData = CType(ActionData.Judgment, clsUtilityCurveMeasureData)
                                If ParentNode.IsTerminalNode Then ChildNode = AltsHierarchy.GetNodeByID(UCData.NodeID) Else ChildNode = ObjHierarchy.GetNodeByID(UCData.NodeID)

                        End Select
                    End If

            End Select

            Dim fHasJudgments As Boolean = False
            Dim fHasUndefined As Boolean = False
            For Each tUser As clsUser In UsersList
                If tUser IsNot Nothing AndAlso IsUserJudgmentAllowed(Action, tUser) Then    ' D2946
                    Select Case Action.ActionType

                        Case ActionType.atPairwise
                            If ParentNode IsNot Nothing AndAlso FirstNode IsNot Nothing AndAlso SecondNode IsNot Nothing Then
                                Dim pwData As clsPairwiseMeasureData = CType(ParentNode.Judgments, clsPairwiseJudgments).PairwiseJudgment(FirstNode.NodeID, SecondNode.NodeID, tUser.UserID)
                                If pwData Is Nothing OrElse pwData.IsUndefined Then fHasUndefined = True Else fHasJudgments = True
                            End If

                        Case ActionType.atPairwiseOutcomes
                            If ParentNode IsNot Nothing AndAlso FirstRating IsNot Nothing AndAlso SecondRating IsNot Nothing AndAlso ParentNode.PWOutcomesJudgments IsNot Nothing Then ' D2946
                                Dim pwData As clsPairwiseMeasureData = ParentNode.PWOutcomesJudgments.PairwiseJudgment(FirstRating.ID, SecondRating.ID, tUser.UserID, , Action.PWONode.NodeID)
                                If pwData Is Nothing OrElse pwData.IsUndefined Then fHasUndefined = True Else fHasJudgments = True
                            End If

                        Case ActionType.atNonPWOneAtATime
                            If ParentNode IsNot Nothing AndAlso ChildNode IsNot Nothing AndAlso Action.ActionData IsNot Nothing Then   ' D2946
                                Dim judgments As clsNonPairwiseJudgments = If(CType(Action.ActionData, clsOneAtATimeEvaluationActionData).IsEdge, ChildNode.EventsJudgments, ChildNode.DirectJudgmentsForNoCause)
                                Select Case CType(Action.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType

                                Case ECMeasureType.mtRatings
                                        Dim nonpwData As clsNonPairwiseMeasureData = Nothing
                                        If Action.ActionData.Node.IsAlternative Then
                                            'If ChildNode.DirectJudgmentsForNoCause IsNot Nothing Then nonpwData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                            If judgments IsNot Nothing Then nonpwData = judgments.GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                        Else
                                            If ParentNode.Judgments IsNot Nothing Then nonpwData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                        End If
                                        If nonpwData Is Nothing OrElse nonpwData.IsUndefined Then fHasUndefined = True Else fHasJudgments = True

                                    Case ECMeasureType.mtDirect
                                        Dim DirectData As clsDirectMeasureData = Nothing
                                        ' D3323 ===
                                        If ParentNode.Judgments IsNot Nothing Then
                                            If Action.ActionData.Node.IsAlternative Then
                                                'DirectData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                                DirectData = judgments.GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                            Else
                                                DirectData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                            End If
                                        End If
                                        'DirectData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                        ' D3323 ==
                                        If DirectData Is Nothing OrElse DirectData.IsUndefined Then fHasUndefined = True Else fHasJudgments = True

                                    Case ECMeasureType.mtStep
                                        Dim StepData As clsStepMeasureData = Nothing
                                        ' D3323 ===
                                        If ParentNode.Judgments IsNot Nothing Then
                                            If Action.ActionData.Node.IsAlternative Then
                                                'StepData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                                StepData = judgments.GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                            Else
                                                StepData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                            End If
                                        End If
                                        'StepData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                        ' D3323 ==
                                        If StepData Is Nothing OrElse StepData.IsUndefined Then fHasUndefined = True Else fHasJudgments = True

                                    Case ECMeasureType.mtRegularUtilityCurve
                                        Dim UCData As clsUtilityCurveMeasureData = Nothing
                                        ' D3323 ===
                                        If ParentNode.Judgments IsNot Nothing Then
                                            If Action.ActionData.Node.IsAlternative Then
                                                'UCData = CType(ChildNode.DirectJudgmentsForNoCause, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                                UCData = judgments.GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                            Else
                                                UCData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID)
                                            End If
                                        End If
                                        'UCData = CType(ParentNode.Judgments, clsNonPairwiseJudgments).GetJudgement(ChildNode.NodeID, ParentNode.NodeID, tUser.UserID) ' D2946
                                        ' D3323 ==
                                        If UCData Is Nothing OrElse UCData.IsUndefined Then fHasUndefined = True Else fHasJudgments = True

                                End Select
                            End If

                    End Select
                End If

            Next

            If fHasJudgments AndAlso Not fHasUndefined Then fStatus = ecPipeStepStatus.psAllJudgments
            If fHasJudgments AndAlso fHasUndefined Then fStatus = ecPipeStepStatus.psMissingJudgments
            If Not fHasJudgments AndAlso fHasUndefined Then fStatus = ecPipeStepStatus.psNoJudgments

        End If
        Return fStatus
    End Function
    ' D2637 ==

    Public Function GetPipeSteps() As String
        If IsCallback Then Return ""
        DebugInfo("Get Evaluation Steps")
        Dim StepsCount As Integer = TeamTimePipe.Count
        If StepsCount < 1 Then Return "&nbsp;"

        Dim Terminals As Integer = 2

        Dim btnStart As Integer = TeamTimePipeStep - (ShowStepsCount - Terminals) \ 2
        If btnStart < 2 Then
            btnStart = 2
        Else
            Terminals += 1
            btnStart += 1
        End If

        Dim btnEnd As Integer = btnStart + (ShowStepsCount - Terminals - 1)
        If btnEnd >= StepsCount Then
            btnEnd = StepsCount - 1
            btnStart = btnEnd - (ShowStepsCount - Terminals - 1)
            If btnStart < 2 Then btnStart = 2
        Else
            Terminals += 1
            btnEnd -= 1
        End If

        Dim sSteps As String = ""
        If StepsCount > 0 Then sSteps += GetStepButton(1)
        If btnStart > 2 Then
            Dim St As Integer = btnStart - (ShowStepsCount - 3) \ 2
            If St < 2 Then St = 2
            If btnStart = 3 Then St = -2
            sSteps += GetStepButton(-St)
        End If
        For i As Integer = btnStart To btnEnd
            sSteps += GetStepButton(i)
        Next
        If btnEnd < StepsCount - 1 Then
            Dim St As Integer = btnEnd + (ShowStepsCount - 3) \ 2
            If St > StepsCount - 1 Then St = StepsCount - 1
            If btnEnd = StepsCount - 2 Then St = -(StepsCount - 1)
            sSteps += GetStepButton(-St)
        End If
        If StepsCount > 1 Then sSteps += GetStepButton(StepsCount)

        Dim sEvaluated = ""
        ' D1287 + D4554 ===
        Dim tPrjUser As clsUser = Nothing
        ' D4583 ===
        If isTeamTimeOwner Then tPrjUser = PM.GetUserByEMail(TeamTimeEvalProgressUser)
        If tPrjUser Is Nothing Then tPrjUser = PM.GetUserByEMail(App.ActiveUser.UserEmail)
        Dim fHasEvalPrg As Boolean = False  ' D4586
        ' D4554 ==
        If tPrjUser IsNot Nothing AndAlso clsApplicationUser.AHPUserByUserEmail(tPrjUser.UserEMail, TeamTimeUsersList) IsNot Nothing Then
            ' D4583 ==
            Dim jTotal As Integer = TeamTime.GetUserTotalCount(tPrjUser)
            Dim jMade As Integer = TeamTime.GetUserEvaluatedCount(tPrjUser)
            If jTotal > 0 Then
                sEvaluated = String.Format("<span id='divEvalProgress'>" + ResString("lblEvaluationStatus") + "</span>", String.Format("<b>{0}</b>", jMade), String.Format("<b>{0}</b>", jTotal)) ' D1289
                fHasEvalPrg = True  ' D4586
            End If
        End If
        ' D1287 ==
        If Not fHasEvalPrg Then sEvaluated = "<span id='divEvalProgress'></span>" ' D4586

        sSteps = String.Format("<img src='{2}TreeView.gif' width=16 height=16 title='{3}' border=0 name='imgWhereAmI' id='imgWhereAmI' class='aslink'>&nbsp;<b>{4}</b>:&nbsp;{0}<img src='{2}pane-list.png' width=16 height=16 border=0 alt='{5}' ID='imgStepsList' class='aslink'/><span class='text' style='margin-left:1ex'>{1}</span>", sSteps, sEvaluated, ImagePath, JS_SafeHTML(ResString("lblWhereAmI")), ResString("lblEvaluationSteps"), ResString("lblStepsList"))    ' D1291 + D1653 + D1657
        Return sSteps
    End Function

    ' D2577 ===
    Private Function GetDateTimeTicks(Dt As DateTime) As String
        Return Dt.ToString("yyyy'/'MM'/'dd HH':'mm':'ss")
    End Function
    ' D2577 ==

    ' D1653 ===
    Public Function GetStepsList() As String
        If IsCallback Then Return ""
        DebugInfo("Get Steps List")
        Dim StepsCount As Integer = TeamTimePipe.Count

        Dim sSteps As String = ""
        Dim sMainStyle As String = ""   ' D2637

        If StepsCount <= _OPT_STEPS_SHOW_MAX Then   ' D3074

            For i As Integer = 1 To StepsCount
                Dim Action As clsAction = TeamTimePipe(i - 1)
                Dim sClass As String = ""
                sMainStyle = "button_c" ' D2637
                If isEvaluation(Action) Then
                    'sClass += CStr(IIf(isUndefined(Action), " button_evaluate_undefined", " button_evaluated"))
                    ' -D3290
                    '' D2637 ===
                    'Select Case GetStepStatus(Action)
                    '    Case ecPipeStepStatus.psAllJudgments
                    '        sMainStyle = "button_green_c"
                    '    Case ecPipeStepStatus.psMissingJudgments
                    '        sMainStyle = "button_yellow_c"
                    '    Case ecPipeStepStatus.psNoJudgments
                    '        sMainStyle = "button_red_c"
                    'End Select
                    '' D2637 ==

                    ' -D3293
                    '' D3025 ===
                    'If Not TeamTime.GetStepAvailabilityForUser(Action.ActionType, Action, clsApplicationUser.AHPUserByUserEmail(App.ActiveUser.UserEmail, TeamTimeUsersList)) Then
                    '    sMainStyle += " button_evaluate_restr"
                    'Else
                    '    sMainStyle += CStr(IIf(isUndefined(Action), " button_evaluate_undefined", " button_evaluated"))
                    'End If
                    '' D3025 ==
                    ' D3323 ===
                    'sMainStyle += CStr(IIf(isUndefined(Action), " button_evaluate_undefined", " button_evaluated")) ' D3293
                    Select Case GetStepStatus(Action, TeamTimeUsersList)   ' D4554
                        Case ecPipeStepStatus.psAllJudgments
                            sMainStyle += " button_evaluated"
                        Case ecPipeStepStatus.psMissingJudgments
                            sMainStyle += " button_evaluate_missing"
                        Case ecPipeStepStatus.psNoJudgments
                            sMainStyle += " button_evaluate_undefined"
                    End Select
                    ' D3323 ==
                Else
                    sClass += " button_no_evaluate"
                End If
                If TeamTimePipeStep = i Then sClass += " button_active_list" ' D1693
                Dim sCaption As String = Action.ActionType.ToString
                sCaption = String.Format(ResString("btnEvaluationStepHint"), i, GetPipeStepHint(TeamTimePipe(i - 1)).Replace("'", "&#39;"))  ' D2995 + D3074
                sCaption = HTML2Text(sCaption).Replace(vbLf, " ").Replace(vbCr, "").Replace("  ", " ").Trim ' D3567
                sSteps += String.Format("<div style='margin:2px' id='divStep{0}'><input type='button' value='{1}' class='{3} button_small {2}' onclick='SyncStep({0}); return false;' style='width:100%; text-align:left; padding:0px 2px; font-weight:normal;' onmouseover='this.title=""{4}"";'/></div>", i, ShortString(sCaption, 140 - i.ToString.Length, True), JS_SafeString(sClass), JS_SafeString(sMainStyle), JS_SafeString(IIf(sCaption.Length > 100, sCaption, "")))  ' D2637 + D3567 + D3872
            Next
            ' D3074 ===
        Else
            sSteps = String.Format("<p align=center style='margin-top:12em;'><img src='{0}devex_loading.gif' width=16 height=16 style='margin-bottom:1em;' ><br>{1}</p>", ImagePath, ResString("lblPleaseWait"))
            ScriptManager.RegisterStartupScript(Me, GetType(String), "onLoadSteps", "load_steps = 1;", True)
        End If
        ' D3074 ==

        sSteps = String.Format("<div style='margin:4px; padding:2px;{0}' id='divStepsList'>{1}</div>", IIf(StepsCount > 18, "height:30em; overflow-y:scroll; margin-top:8px;", ""), sSteps)

        Return sSteps
    End Function
    ' D1653 ==

    '' D2995 ===
    'Public ReadOnly Property TeamTimeStepHint(tStep As Integer) As String
    '    Get
    '        If Session(_SESS_STEPS_LIST) IsNot Nothing Then _TeamTimeStepsList = CType(Session(_SESS_STEPS_LIST), String())
    '        If _TeamTimeStepsList Is Nothing Then
    '            Array.Resize(_TeamTimeStepsList, TeamTime.Pipe.Count)
    '            For i As Integer = 0 To TeamTime.Pipe.Count - 1
    '                _TeamTimeStepsList(i) = GetPipeStepHint(TeamTime.Pipe(i))
    '            Next
    '            Session(_SESS_STEPS_LIST) = _TeamTimeStepsList
    '        End If
    '        Return _TeamTimeStepsList(tStep)
    '    End Get
    'End Property
    '' D2995 ==

    Private Function GetStepButton(mStep As Integer) As String
        Dim btnWidth As Integer = 4
        If mStep > 0 Then
            btnWidth = TeamTimePipe.Count.ToString.Length
            If btnWidth > 3 Then btnWidth -= 1
            If btnWidth < 2 Then btnWidth = 2
            btnWidth += 4
        End If
        Dim sBtnStyle As String = ""
        Dim sMainStyle As String = "button" ' D2637
        Dim sJudgHint As String = ""        ' D2367 - D3290 + D3323
        Dim tAction As clsAction = Nothing  ' D1653
        Dim sHint As String = ""    ' D1653
        If mStep < 0 Then
            sBtnStyle += " button_ellipse"
        Else
            If TeamTimePipeStep = mStep Then sBtnStyle += " button_active"
            tAction = GetAction(mStep)  ' D1653
            If tAction IsNot Nothing Then
                If isEvaluation(tAction) Then
                    ' -D3290
                    '' D2637 ===
                    'Select Case GetStepStatus(tAction)
                    '    Case ecPipeStepStatus.psAllJudgments
                    '        sMainStyle = "button_green"
                    '        sJudgHint = ResString("lblTTAllJudgments")
                    '    Case ecPipeStepStatus.psMissingJudgments
                    '        sMainStyle = "button_yellow"
                    '        sJudgHint = ResString("lblTTMissingJudgments")
                    '    Case ecPipeStepStatus.psNoJudgments
                    '        sMainStyle = "button_red"
                    '        sJudgHint = ResString("lblTTNoJudgments")
                    'End Select
                    '' D2637 ==

                    ' -D3293
                    '' D3025 ===
                    'If Not TeamTime.GetStepAvailabilityForUser(tAction.ActionType, tAction, clsApplicationUser.AHPUserByUserEmail(App.ActiveUser.UserEmail, TeamTimeUsersList)) Then
                    '    sBtnStyle += " button_evaluate_restr"
                    'Else
                    '    sBtnStyle += CStr(IIf(isUndefined(tAction), " button_evaluate_undefined", " button_evaluated"))
                    'End If
                    '' D3025 ==
                    ' D3323 ===
                    'sMainStyle += CStr(IIf(isUndefined(Action), " button_evaluate_undefined", " button_evaluated")) ' D3293
                    Select Case GetStepStatus(tAction, TeamTimeUsersList)   ' D4554
                        Case ecPipeStepStatus.psAllJudgments
                            sMainStyle += " button_evaluated"
                            sJudgHint = ResString("lblTTAllJudgments")
                        Case ecPipeStepStatus.psMissingJudgments
                            sMainStyle += " button_evaluate_missing"
                            sJudgHint = ResString("lblTTMissingJudgments")
                        Case ecPipeStepStatus.psNoJudgments
                            sMainStyle += " button_evaluate_undefined"
                            sJudgHint = ResString("lblTTNoJudgments")
                    End Select
                    ' D3323 ==
                Else
                    sBtnStyle += " button_no_evaluate"
                End If
            End If
            sHint = String.Format("#{0}: {1}", mStep, GetPipeStepHint(TeamTimePipe(mStep - 1)).Replace("'", "&#39;"))    ' D1653 + D2995 + D3074
        End If
        Return String.Format("<input type='button' id='btn{0}' name='btn{0}' value='{1}' class='{7} button_small{2}' onClick='{5}' style='border-width:{3}px; padding:0px; width:{4}ex;' title='{6}{8}'>", Math.Abs(mStep), IIf(mStep < 0, "…", mStep), sBtnStyle, IIf(mStep < 0, 0, 1), btnWidth, String.Format(CStr(IIf(mStep < 0, "ShowJumpTooltip(""btn{0}""); return false;", "return SyncStep({0});")), Math.Abs(mStep)), SafeFormString(sHint), sMainStyle, IIf(sJudgHint <> "" AndAlso TeamTimePipeStep <> mStep, "<br><br><i>" + SafeFormString(sJudgHint) + "</i>", "")) ' D1653 + D2637 + D3323
        'Return String.Format("<input type='button' id='btn{0}' name='btn{0}' value='{1}' class='{7} button_small{2}' onClick='{5}' style='border-width:{3}px; padding:0px; width:{4}ex;' title='{6}'>", Math.Abs(mStep), IIf(mStep < 0, "…", mStep), sBtnStyle, IIf(mStep < 0, 0, 1), btnWidth, String.Format(IIf(mStep < 0, "ShowJumpTooltip(""btn{0}""); return false;", "return SyncStep({0});"), Math.Abs(mStep)), sHint, sMainStyle) ' D1653 + D2637 + D3290
    End Function
    ' D1246 ==

    ' D2460 ===
    Public Function GetHelpRoot() As String
        Return CStr(IIf(App.isRiskEnabled, ResString("navHelpBaseRisk") + ResString("navHelpStartRisk"), ResString("navHelpBase") + ResString("navHelpStart")))
    End Function
    ' D2460 ==

    ' D2622 ===
    Public Function GetUsersPageActive() As Integer
        Dim tPG As Integer = 1
        If Not Integer.TryParse(GetCookie(_COOKIE_USERSPAGE, "1"), tPG) Then tPG = 1
        Return tPG
    End Function
    ' D2622 ==

    ' D2630 ===
    Public Function GetUsersPageEnabled() As Integer
        Dim tPG As Integer = 0  ' D6504
        If Not Integer.TryParse(GetCookie(_COOKIE_USERS_PAGES_USE, tPG.ToString), tPG) Then tPG = 0 ' D6504
        Return tPG
    End Function

    Public Function GetUsersPageSize() As Integer
        Dim tPG As Integer = -15
        If Not Integer.TryParse(GetCookie(_COOKIE_USERS_PAGES_SIZE, "-15"), tPG) Then tPG = -15
        Return tPG
    End Function
    ' D2630 ==

    ' D4555 ===
    Private Function GetNextUnasStep(tFrom As Integer, tTo As Integer) As Integer
        Dim UsrList As New List(Of clsUser)
        Dim tUser As clsUser = clsApplicationUser.AHPUserByUserEmail(TeamTimeEvalProgressUser, TeamTimeUsersList)
        If tUser IsNot Nothing Then
            UsrList.Add(tUser)
            For i As Integer = tFrom To tTo Step 1
                Dim tAction As clsAction = GetAction(i)
                If isEvaluation(tAction) AndAlso IsUserJudgmentAllowed(tAction, tUser) AndAlso GetStepStatus(tAction, UsrList) <> ecPipeStepStatus.psAllJudgments Then Return i ' D4615
            Next
        End If
        Return -1
    End Function

    Public Function GetNextUnassessed() As Integer
        Dim NU As Integer = GetNextUnasStep(TeamTimePipeStep + 1, TeamTimePipe.Count)
        If NU < 0 Then NU = GetNextUnasStep(1, TeamTimePipeStep - 1)
        Return NU
    End Function
    ' D4555 ==

    '' D2701 ===
    'Public Function CanUseKeypads() As Boolean
    '    If _TeamTimeActive AndAlso TeamTimePipeParams.SynchUseVotingBoxes Then
    '        If isTeamTimeOwner Then Return True
    '        'Dim tUser As clsUser = PM.GetUserByEMail(App.ActiveUser.UserEmail)
    '        'If tUser IsNot Nothing AndAlso tUser.SyncEvaluationMode = SynchronousEvaluationMode.semVotingBox Then Return True
    '    End If
    '    Return False
    'End Function
    '' D2701 ==

#End Region

#Region "Page Events"

    Public Sub New()
        MyBase.New(_PGID_TEAMTIME_CV)  ' D3563
        'ExpertChoice.Service.isTraceEnabled = True
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If App.ActiveProject.ProjectStatus <> ecProjectStatus.psActive Then FetchAccess(_DEF_PGID_ONPROJECTS) ' D1246
        CheckProjectStatus()
        NoMobileView = True ' D4906
        If Not IsPostBack AndAlso Not isCallback Then
            AlignHorizontalCenter = False
            AlignVerticalCenter = False

            App.DBTeamTimeSessionAppID(App.ProjectID)

            imgNavHelp.Attributes.Add("style", "top:3px; position:relative;")   ' D4558

            If App.CanvasMasterDBVersion < "0.996" Then
                _TeamTimeActive = False
            Else
                ' D1271 ===
                Dim tWs As clsWorkspace = App.ActiveWorkspace
                If tWs Is Nothing Then tWs = App.AttachProject(App.ActiveUser, App.ActiveProject, False, App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator), "Attach user to project", False) ' D2644
                If tWs IsNot Nothing AndAlso Not tWs.isInTeamTime(App.ActiveProject.isImpact) AndAlso isTeamTime Then   ' D1945
                    tWs.TeamTimeStatus(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive   ' D1945
                    tWs.Status(App.ActiveProject.isImpact) = ecWorkspaceStatus.wsSynhronousActive   ' D1945
                    App.DBWorkspaceUpdate(tWs, False, "Enable TeamTime for user")
                End If
                ' D1271 ==

                If tWs Is Nothing AndAlso App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then FetchAccessByWrongLicense("", False) ' D1490

                ' D1440 ===
                If Not App.ActiveProject.isTeamTime OrElse isTeamTimeOwner Then ' D1953
                    'If CheckVar(_PARAM_ACTION, "").ToLower = "consensus" Then
                    If IsConsensus AndAlso CheckVar("objid", "") <> "" Then ' D3565
                        DebugInfo("Start TT from consensus")
                        ' D3151 ===
                        Dim ObjID As Integer = CheckVar("objid", -1)
                        Dim AltID As Integer = CheckVar("altid", -1)
                        Dim TTStep As Integer = TeamTime.GetTeamTimeStep(ObjID, AltID)
                        Dim fCanEditPrj As Boolean = App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)
                        ' D3151 ==
                        If fCanEditPrj Then   ' D3154 + D3239
                            If App.TeamTimeResumeSession(App.ActiveUser, App.ActiveProject, CType(PM.ActiveHierarchy, ECHierarchyID), Nothing, Nothing, SessionsList, True, 0) Then
                                TeamTime = Nothing ' D1734 + D3151
                                _TeamTimeUsersList = Nothing    ' D3239
                            End If
                        End If
                        If isTeamTime AndAlso fCanEditPrj Then  ' D3239
                            ' D3235 ===
                            Dim tRealStep As Integer = TeamTime.GetTeamTimeStep(ObjID, AltID)
                            If tRealStep <> TTStep AndAlso tRealStep > 0 Then
                                TTStep = tRealStep
                                App.TeamTimeResumeSession(App.ActiveUser, App.ActiveProject, CType(PM.ActiveHierarchy, ECHierarchyID), Nothing, Nothing, SessionsList, True, -1)
                            End If
                            ' D3235 ==
                            ' D1453 ===
                            Dim sURL As String = PageURL(CurrentPageID)
                            sURL += CStr(IIf(sURL.Contains("?"), "", "?")) + GetTempThemeURI(True)  ' D3563
                            If TTStep >= 0 Then
                                'sURL = sURL.Replace("mode=consensus", "").Replace("action=consensus", "") + String.Format("&{0}={1}&mode=consensus", _PARAM_STEP, TTStep + 1)   ' D3563
                                sURL += String.Format("&{0}={1}", _PARAM_STEP, TTStep + 1)   ' D3565
                                TeamTimePipeStep = TTStep + 1
                            Else
                                sURL += "&msg=nostep"   ' D3235
                            End If
                            TeamTime = Nothing  ' D3151
                            Response.Redirect(sURL, True)
                            ' D1453 ==
                        End If
                    End If
                End If
                ' D1440 ==

                ' D1246 ===
                If isTeamTimeOwner Then
                    Dim NewStep As Integer = CheckVar(_PARAM_STEP, TeamTimePipeStep)
                    If NewStep <> TeamTimePipeStep Then
                        TeamTimePipeStep = NewStep
                        Dim sTheme As String = GetTempThemeURI(False)   ' D1325
                        Response.Redirect(PageURL(CurrentPageID) + CStr(IIf(sTheme = "", "", "?" + sTheme)), True)  ' D1292 + D1325
                    End If
                    ClientScript.RegisterOnSubmitStatement(GetType(String), "OnPostData", "SyncStop('');")  ' D2538
                Else
                    ClientScript.RegisterOnSubmitStatement(GetType(String), "OnPostData", "return false;")  ' D2538
                End If
                ' D1246 ==
                ClientScript.RegisterStartupScript(GetType(String), "SyncInit", " SyncInit(''); ", True)
                ' D3235 ===
                If CheckVar("msg", "").Trim.ToLower = "nostep" Then
                    ClientScript.RegisterStartupScript(GetType(String), "MsgCV", String.Format("dxDialog('{0}', true, ';', '');", JS_SafeString(ResString("msgTTCantFindCVStep"))), True)
                End If
                ' D3235 ==
            End If
            pnlLoadingNextStep.Caption = ResString("msgLoading")    ' D1291
            'pnlLoadingNextStep.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "devex_loading.gif") ' D1291 ' "process.gif"
            pnlLoadingNextStep.Message = String.Format("<img src={1} alt='{0}' border=0 width=16 height=16/>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "devex_loading.gif") ' D1291 ' "process.gif"
        End If

        ' D3565 ===
        If isTeamTimeOwner AndAlso isTeamTime AndAlso CheckVar(_PARAM_ACTION, "").Trim.ToLower = "stop" Then
            StopMeeting()
            'CStr(iif(GetTempTheme()="tt" OrElse GetTempTheme()="sl", "&close=yes&temptheme=sl" , "")))
            Dim sURL As String = PageURL(_PGID_PROJECTSLIST)
            If isSLTheme() OrElse GetTempTheme() = "tt" Then sURL = PageURL(CurrentPageID, "close=yes&teamtime=yes" + GetTempThemeURI(True))
            If App.Options.OnlyTeamTimeEvaluation Then sURL = PageURL(_PGID_LOGOUT) ' D3767
            Response.Redirect(sURL, True)
        End If
        ' D3565 ==

        ' D3074 ===
        If isTeamTimeOwner AndAlso CheckVar(_PARAM_ACTION, "") = "stepslist" Then

            Dim sContent As String = ""
            Dim s As Integer = CheckVar("from", 1)
            If s < 1 Then s = 1
            Dim f As Integer = s + _OPT_STEPS_PAGE_SIZE
            If f > TeamTimePipe.Count Then f = TeamTimePipe.Count
            For i As Integer = s To f

                Dim St As Integer = 3   ' 0: no judgments, 1: missing, 2: all judgments, 3: non-evaluation  // was: 0 = evaluated, 1 = undefined, 2: non-evaluation ' D3323
                Dim Action As clsAction = GetAction(i)
                'Dim Col = 3
                If isEvaluation(Action) Then
                    'If isUndefined(Action) Then St = 1 Else St = 0 ' -D3323
                    St = CInt(GetStepStatus(Action, TeamTimeUsersList))    ' D3323 + D4554
                End If

                Dim sName As String = String.Format(ResString("btnEvaluationStepHint"), i, GetPipeStepHint(Action))
                sName = ShortString(HTML2Text(sName).Replace(vbLf, " ").Replace(vbCr, "").Replace("  ", " ").Trim, 140 - ID.ToString.Length, True)
                'sContent += String.Format("{0}[{1},{2},'{3}',{4}]", IIf(sContent = "", "", ","), i, St, JS_SafeString(sName.Replace("'", "&#39;")), Col)
                sContent += String.Format("{0}[{1},{2},'{3}']", IIf(sContent = "", "", ","), i, St, JS_SafeString(sName.Replace("'", "&#39;"))) ' D3323 + D3567
            Next

            sContent = "[" + sContent + "]"

            RawResponseStart()
            Response.ContentType = "text/plain"
            Response.Write(sContent)
            Response.End()  ' D3567
        End If
        ' D3074 ==

        DebugInfo("Init TT finished")

        ' D3998 ===
#If OPT_USE_AJAX_INSTEAD_CALLBACK Then
        If isAjax() Then
            Dim sResult As String = onCallback(Request.Params)
            RawResponseStart()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If
#End If
        ' D3998 ==

    End Sub

    ' D1953 ===
    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not IsConsensus AndAlso Not App.isTeamTimeAvailable Then FetchAccessByWrongLicense(ResString("errNoTTAllowed"), True) ' D3565
        If Not IsConsensus Then CurrentPageID = _PGID_TEAMTIME Else StorePageID = False  ' D3563 + D4755
        If App.HasActiveProject Then
            ' D4958 ===
            If PM.StorageManager.ModelID <> App.ProjectID Then  ' project has been changed, need to reset everything
                TeamTime = Nothing
                If Not isCallback AndAlso Not IsPostBack AndAlso Not isAJAX Then Response.Redirect(PageURL(CurrentPageID) + CStr(IIf(GetTempThemeURI(False) = "", "", "?" + GetTempThemeURI(False))), True)
            End If
            ' D4958 ==
            DebugInfo("TT init…")
            _Orignal_HID = CType(PM.ActiveHierarchy, ECHierarchyID)
            ' D1734 ===
            Dim sPasscode As String = App.DBTeamTimeDataRead(App.ActiveProject.ID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionPasscode, Nothing)
            If Not String.IsNullOrEmpty(sPasscode) Then
                If App.ActiveProject.PasscodeLikelihood.ToLower = sPasscode Then PM.ActiveHierarchy = ECHierarchyID.hidLikelihood
                If App.isRiskEnabled AndAlso App.ActiveProject.PasscodeImpact.ToLower = sPasscode Then PM.ActiveHierarchy = ECHierarchyID.hidImpact ' D2898
                If App.isRiskEnabled Then
                    ' D1857 ===
                    If App.ActiveProject.isImpact Then
                        PM.PipeParameters.CurrentParameterSet = PM.PipeParameters.GetParameterSetByID(PARAMETER_SET_IMPACT)
                    Else
                        PM.PipeParameters.CurrentParameterSet = PM.PipeParameters.GetParameterSetByID(PARAMETER_SET_DEFAULT)
                    End If
                Else
                    ' D1974 ===
                    If Not App.ActiveProject.PipeParameters.ForceDefaultParameters Then
                        If App.ActiveProject.PipeParameters.CurrentParameterSet IsNot App.ActiveProject.PipeParameters.TeamTimeParameterSet Then _TeamTimeOldPipeParams = App.ActiveProject.PipeParameters.CurrentParameterSet
                        App.ActiveProject.PipeParameters.CurrentParameterSet = App.ActiveProject.PipeParameters.TeamTimeParameterSet
                    End If
                    ' D1974 ==
                End If
                ' D1857 ==
                If isTeamTimeOwner AndAlso Not IsPostBack AndAlso Not isCallback AndAlso PM.ActiveHierarchy <> PM.ActiveHierarchy Then PM.ActiveHierarchy = PM.ActiveHierarchy ' D3066
            End If
            ' D1734 ==
        End If
    End Sub

    Protected Sub Page_PreRenderComplete(sender As Object, e As EventArgs) Handles Me.PreRenderComplete
        If App.HasActiveProject Then PM.ActiveHierarchy = _Orignal_HID
        If Not App.isRiskEnabled AndAlso _TeamTimeOldPipeParams IsNot Nothing AndAlso Not App.ActiveProject.PipeParameters.ForceDefaultParameters Then
            App.ActiveProject.PipeParameters.CurrentParameterSet = _TeamTimeOldPipeParams
        End If
        DebugInfo("Page rendered")
    End Sub
    ' D1953 ==

    '' D1261 ===
    'Protected Sub btnStart_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnStart.Click
    '    App.TeamTimeResumeSession(App.ActiveUser, App.ActiveProject, PM.ActiveHierarchy, Nothing, Nothing, SessionsList)  ' D1734
    '    If Request IsNot Nothing AndAlso Request.Url IsNot Nothing Then Response.Redirect(Request.Url.AbsoluteUri, True) Else Response.Redirect(PageURL(CurrentPageID) + "?" + GetTempThemeURI(False), True)
    'End Sub
    '' D1261 ==

    ' D1291 ===
    Protected Sub RadTreeViewHierarchy_Load(sender As Object, e As EventArgs) Handles RadTreeViewHierarchy.Load
        If isTeamTime AndAlso isTeamTimeOwner AndAlso RadTreeViewHierarchy.Visible AndAlso RadTreeViewHierarchy.Nodes IsNot Nothing AndAlso RadTreeViewHierarchy.Nodes.Count = 0 AndAlso PM IsNot Nothing Then  ' D1628 + D3066
            DebugInfo("Load Hierarchy")
            Dim tHieararchy As clsHierarchy = PM.Hierarchy(PM.ActiveHierarchy)    ' D1325
            If tHieararchy IsNot Nothing Then   ' D1628
                AddNodesToRadTree(tHieararchy.GetLevelNodes(0), RadTreeViewHierarchy.Nodes, False, 0, -1)    ' D1325
                If tHieararchy.Nodes.Count > 15 Then divTree.Attributes("style") += "height:30em; overflow-y:scroll; margin-top:16px;" ' D1316 + D1325
            End If
        End If
    End Sub
    ' D1291 ==

#End Region

#Region "Service functions"

    ' D1288 ===
    Private Sub ResetProject()
        DebugInfo("Reset project")
        TeamTime = Nothing
        _TeamTimeUsersList = Nothing
        App.ActiveProject.ResetProject()
        'Infodoc_CleanProject(App.ProjectID) ' D1301 -D1600 // case 2771
    End Sub
    ' D1288 ==

    ' D3565 ===
    Private Sub StopMeeting()
        If App.ActiveProject.PipeParameters.SynchUseVotingBoxes AndAlso App.Options.KeypadsAvailable Then    ' D6418 + D6429
            'TODO: TTA
            'Try
            '    Dim AdminProxy As New KeypadPrivillegedClient
            '    If AdminProxy IsNot Nothing Then
            '        AdminProxy.EndMeeting(App.ActiveProject.MeetingID)
            '        AdminProxy.ClearTokens(clsMeetingID.AsString(App.ActiveProject.MeetingID))
            '        AdminProxy.Close()
            '    End If
            'Catch ex As Exception
            '    App.DBSaveLog(dbActionType.actShowRTE, dbObjectType.einfRTE, App.ActiveProject.ID, "End TeamTime meeting", "RTE: " + ex.Message)
            'End Try
        End If
        App.TeamTimeEndSession(App.ActiveProject, False)
    End Sub
    ' D3565 ==

    ' D1348 ===
    Private Sub TeamTimeUpdateStepInfo(StepID As Integer)
        DebugInfo("Update step info")
        TeamTime.SetCurrentStep(StepID - 1, TeamTimeUsersList)
        App.DBWorkspaceUpdate(App.ActiveWorkspace, False, "")
        Dim tAction As clsAction = GetAction(StepID)
        If tAction IsNot Nothing Then
            ' D1364 ===
            Dim sExtra As String = ""
            If tAction.ActionType = ActionType.atNonPWOneAtATime AndAlso TypeOf (tAction.ActionData) Is clsOneAtATimeEvaluationActionData Then
                ' D1403 ===
                Dim tActionData As clsOneAtATimeEvaluationActionData = CType(tAction.ActionData, clsOneAtATimeEvaluationActionData)
                If tActionData.MeasurementType = ECMeasureType.mtRatings Then
                    ' D1403 ==
                    Dim RS As clsRatingScale = CType(tActionData.Node.MeasurementScale, clsRatingScale)
                    For i As Integer = RS.RatingSet.Count - 1 To 0 Step -1
                        Dim intensity As clsRating = CType(RS.RatingSet(i), clsRating)
                        sExtra += CStr(IIf(sExtra = "", "", ",")) + intensity.ID.ToString
                    Next
                End If
            End If
            Dim sStepData As String = String.Format("{1}{0}{2}{0}{3}{0}{4}", clsTeamTimePipe.Judgment_Delimeter, StepID - 1, CInt(tAction.ActionType), tAction.StepGuid, sExtra)  ' D1353
            ' D1364 ==
            App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeStepData, sStepData)
        Else
            App.DBTeamTimeDataDelete(App.ProjectID, Nothing, ecExtraProperty.TeamTimeStepData)
        End If
    End Sub
    ' D1348 ==

    Private Sub CheckProjectStatus()
        DebugInfo("Check project status")
        Dim tmpPrj As clsProject = App.DBProjectByID(App.ProjectID)
        If tmpPrj IsNot Nothing Then
            ' D1246 ===
            If isTeamTimeOwner Then
                If Not tmpPrj.isOnline AndAlso Not IsConsensus Then ' D3207
                    tmpPrj.isOnline = True
                    App.ActiveProject.isOnline = True
                    App.DBProjectUpdate(App.ActiveProject, False, "Make project on-line")
                    App.ActiveProject.LastModify = Now
                End If
                If tmpPrj.LockInfo IsNot Nothing AndAlso (tmpPrj.LockInfo.LockStatus <> ECLockStatus.lsLockForTeamTime AndAlso tmpPrj.LockInfo.LockStatus <> ECLockStatus.lsLockForSystem) Then ' D6821
                    If App.DBProjectLockInfoWrite(ECLockStatus.lsLockForTeamTime, tmpPrj.LockInfo, App.ActiveUser, Now.AddSeconds(If(IsConsensus, _DEF_LOCK_TT_CONSENSUS_VIEW, _DEF_LOCK_TT_SESSION_TIMEOUT))) Then App.ActiveProject.LockInfo = tmpPrj.LockInfo    ' D6553
                End If
                '' D4535 ===
                'Dim dt As Object = App.Database.ExecuteScalarSQL(String.Format("SELECT MAX(ModifyDate) as dt FROM ModelStructure WHERE ProjectID = {0}", App.ProjectID))
                'If dt IsNot Nothing AndAlso Not IsDBNull(dt) Then
                '    Dim PM_dt As DateTime = CDate(dt)
                '    If Not tmpPrj.LastModify.HasValue OrElse tmpPrj.LastModify.Value < PM_dt Then
                '        tmpPrj.LastModify = PM_dt
                '        PM.LastModifyTime = PM_dt
                '        TeamTime = Nothing
                '    End If
                'End If
                '' D4535 ==
            End If
            ' D1246 ==
            If tmpPrj.StatusDataLikelihood <> App.ActiveProject.StatusDataLikelihood Then App.ActiveProject.StatusDataLikelihood = tmpPrj.StatusDataLikelihood ' D1944
            If tmpPrj.StatusDataImpact <> App.ActiveProject.StatusDataImpact Then App.ActiveProject.StatusDataImpact = tmpPrj.StatusDataImpact ' D1944
            If tmpPrj.LastModify.HasValue AndAlso tmpPrj.LastModify.Value.AddSeconds(-2) > App.ActiveProject.LastModify Then    ' D4535
                If Not isTeamTimeOwner Then ResetProject()
                App.ActiveProject.LastModify = tmpPrj.LastModify
            End If
        End If
        ' D1284 ===
        If isTeamTime AndAlso isTeamTimeOwner Then
            ' check timestamps for possible chnages outside
            ' check ModelID for case when another TT project loaded;
            ' check LastModfy for Project, ProjectManager, TeamTime pipe
            If TeamTime IsNot Nothing AndAlso (PM.StorageManager.ModelID <> App.ActiveProject.ProjectManager.StorageManager.ModelID OrElse _
               (PM.LastModifyTime.HasValue AndAlso App.ActiveProject.ProjectManager.LastModifyTime.HasValue AndAlso PM.LastModifyTime.Value.AddSeconds(1) < App.ActiveProject.ProjectManager.LastModifyTime) OrElse _
               (TeamTime.PipeBuiltLastModifyTime.HasValue AndAlso App.ActiveProject.ProjectManager.LastModifyTime.HasValue AndAlso TeamTime.PipeBuiltLastModifyTime.Value.AddSeconds(1) < App.ActiveProject.ProjectManager.LastModifyTime) OrElse _
               (TeamTime.PipeBuiltLastModifyTime.HasValue AndAlso PM.LastModifyTime.HasValue AndAlso TeamTime.PipeBuiltLastModifyTime.Value.AddSeconds(1) < PM.LastModifyTime)) Then    ' D3066 + D4535 + D4542
                TeamTime = Nothing
            End If
        End If
        ' D1284 ==
    End Sub

    ' D1291 ===
    Private Function AddRadTreeNode(sCaption As String, ID As Integer, ByRef RadNodes As RadTreeNodeCollection, Level As Integer, isAlts As Boolean, fHasChilds As Boolean, isCategory As Boolean) As RadTreeNode ' D3604
        Dim rNode As New RadTreeNode
        rNode.Text = JS_SafeHTML(ShortString(sCaption, If(Level = 0, 65, 80 - 4 * Level), True))
        'rNode.Value = CStr(IIf(isAlts, -ID, ID))
        If isAlts Then Level += 1
        Dim asRoot As Boolean = Level = 0
        Dim isCurrent As Boolean = False
        Dim StepNode As clsNode = PM.PipeBuilder.GetPipeActionNode(GetAction(TeamTimePipeStep)) ' D4079 + D4116 + D6445
        If StepNode IsNot Nothing Then
            isCurrent = (ID = StepNode.NodeID)
            If isCurrent Then rNode.ExpandParentNodes()
        End If
        rNode.Text = String.Format("<span class='{1}'>{0}</span>", rNode.Text, If(asRoot, "tree_root_popup", "tree_node_popup") + If(isCurrent, " tree_current", If(isCategory, " tree_cat", "")))   ' D3604
        rNode.CssClass = "aslink"
        RadNodes.Add(rNode)
        Return rNode
    End Function

    Private Sub AddNodesToRadTree(Nodes As List(Of clsNode), ByRef RadNodes As RadTreeNodeCollection, isAlts As Boolean, LevelOffset As Integer, NextUnass As Integer)     ' D0074 + D0125 + C0384 + D1274
        If Nodes Is Nothing Or RadNodes Is Nothing Then Exit Sub
        For Each tNode As clsNode In Nodes
            Dim Childs As List(Of clsNode) = tNode.Children
            Dim rNode As RadTreeNode = AddRadTreeNode(tNode.NodeName, tNode.NodeID, RadNodes, tNode.Level + LevelOffset, isAlts, Childs.Count > 0, tNode.RiskNodeType = RiskNodeType.ntCategory)    ' D3604
            'Dim NodeStep As Integer = PM.PipeBuilder.GetFirstEvalPipeStepForNode(tNode, -1)    ' D1325 'C1020
            Dim NodeStep As Integer = TeamTime.PipeBuilder.GetFirstEvalPipeStepForNode(tNode, -1) 'C1020
            If NextUnass > 0 AndAlso Not TeamTimePipeParams.AllowMissingJudgments AndAlso NodeStep > NextUnass Then NodeStep = -1 ' D1325
            If NodeStep >= 0 And (Not isTeamTime Or isTeamTimeOwner) Then
                rNode.Value = (NodeStep + 1).ToString
            Else
                rNode.Value = -1
                rNode.CssClass = "gray"
            End If
            If Not String.IsNullOrEmpty(rNode.Value) AndAlso rNode.Value.Length > 0 AndAlso rNode.Value(0) <> "-" Then rNode.Text += String.Format("<span class='text small gray'>&nbsp;(#{0})</span>", rNode.Value) ' D1653
            If Childs.Count > 0 Then
                AddNodesToRadTree(Childs, rNode.Nodes, tNode.IsTerminalNode, LevelOffset, NextUnass)
                rNode.Expanded = True
            End If
        Next
    End Sub
    ' D1291 ==

    ' D3482 ===
    Public Function CanStartTTMeeting() As Boolean
        Dim fRes As Boolean = False
        If Not isTeamTime AndAlso _TeamTimeActive AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) Then
            fRes = True
        End If
        Return fRes
    End Function
    ' D3482 ==

    Public Function GetStartupMessage() As String
        Dim sMsg As String = ""
        If _TeamTimeActive Then ' D1275
            If Not isTeamTime OrElse (Not isTeamTimeOwner AndAlso Not isTeamTimeOwnerOnline) Then
                sMsg = String.Format("<h5>{0}</h5>", ResString("msgSynchronousWaitOwner"))
                If CanStartTTMeeting() AndAlso Not IsConsensus AndAlso App.ProjectID = PM.StorageManager.ModelID Then sMsg += String.Format("<div style='text-align:center; margin-top:2em'>{2}<br><input type=button class='button' style='width:14em; padding:3px' value='{1}' onclick='SyncAddCommand(""{0}=do_start"", 0, """");'/></div>", _PARAM_ACTION, ResString("btnStartTT"), "") 'ResString("msgStartTT")  ' D4455 + D4958 + D6086
            Else
                sMsg = "Loading. Please wait…"
            End If
        Else
            sMsg = "<span class='h5 error'>TeamTime can't work on that instance due to have wrong MasterDB version.<br>Please contact with System Administrator for upgrade database.</span>" ' D1275
        End If
        'ShowWarning("TeamTime not started yet. Go to <a href='Default.aspx' class='actions'>AnyTime evaluation</a>.")
        Return sMsg ' D1254
    End Function

    ' D4382 ===
    Public Function GetCombinedName() As String
        Return ResString("lblEvaluationLegendCombined")
        'TeamTime.ProjectManager.CheckCustomCombined()   ' D4382
        'If TeamTime.ProjectManager.Parameters.ResultsCustomCombinedUID >= 0 AndAlso TeamTime.ProjectManager.Parameters.ResultsCustomCombinedName <> "" Then
        '    Return TeamTime.ProjectManager.Parameters.ResultsCustomCombinedName
        'Else
        '    Return ResString("lblEvaluationLegendCombined")
        'End If
    End Function
    ' D4382 ==

    ' D1252 ===
    Private Function GetWRTName(tNode As clsNode) As String

        Dim sName = ""
        If tNode IsNot Nothing Then
            sName = tNode.NodeName
            If TeamTimePipeParams.ShowFullObjectivePath <> ecShowObjectivePath.DontShowPath Then    ' D1276 + D4100
                Dim sDivider As String = ResString("lblObjectivePathDivider")
                While tNode.ParentNode IsNot Nothing
                    sName = tNode.ParentNode.NodeName + sDivider + sName
                    tNode = tNode.ParentNode
                End While
            End If
        End If
        Return sName
    End Function

    'If TaskRes = "" Then TaskRes = ResString(IIf(ParentNode.IsTerminalNode, "task_Pairwise_Alternatives", "task_Pairwise_Objectives")) ' D1852
    'sTask = PrepareTask(ParseStringTemplates(TaskRes, tParams))  ' D1852
    ' D1924 ==
    Private Function GetPipeData() As String
        DebugInfo("Get evaluation data")
        Dim sData As String = ""
        If isTeamTime Then
            If isTeamTimeOwner Then

                Dim tAction As clsAction = GetAction(TeamTimePipeStep)
                sCallbackSave = ""  ' D2578

                ' D1257 ===
                Dim Dt As DateTime = Now
                Dim tRows As List(Of String) = App.DBTeamTimeDataReadAll(App.ProjectID, ecExtraProperty.TeamTimeJudgment)
                If tRows IsNot Nothing Then
                    For Each sRow As String In tRows
                        TeamTime.ParseStringFromClient(sRow)
                        ' D2578 ===
                        Dim sParams As String() = sRow.Split(clsTeamTimePipe.Judgment_Delimeter)
                        If sParams.Count > 4 Then
                            sCallbackSave += String.Format("{0}'{1}'", CStr(IIf(sCallbackSave = "", "", ",")), sParams(4).Trim)
                        End If
                        ' D2578 ==
                    Next
                    If tRows.Count > 0 Then App.DBTeamTimeDataDelete(App.ProjectID, Dt, ecExtraProperty.TeamTimeJudgment)
                    If sCallbackSave <> "" Then sCallbackSave = String.Format("['pended',[{0}]],", sCallbackSave)
                End If
                ' D1257 ==

                Dim sType As String = ""
                Dim sTask As String = ""
                Dim sValue As String = ""

                If tAction IsNot Nothing Then

                    Select Case tAction.ActionType
                        Case ActionType.atInformationPage
                            sType = "message"
                            sValue = String.Format("['{0}',[{1}]]", JS_SafeString(URLProjectID(PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&md5={3}", CInt(reObjectType.PipeMessage), _PARAM_ID, CType(tAction.ActionData, clsInformationPageActionData).Description.ToLower, GetMD5(CType(tAction.ActionData, clsInformationPageActionData).Text))), App.ProjectID)), TeamTime.GetJSON_UsersList(TeamTimeUsersList)) ' D1270 + D1289 + D2585

                        Case ActionType.atPairwise, ActionType.atPairwiseOutcomes
                            sType = "pairwise"
                            sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll) ' D1270 + D1271 + D4825
                            If App.isRiskEnabled AndAlso sValue.IndexOf("%%known%%") > 0 Then sValue = sValue.Replace("%%known%%", JS_SafeString(ResString(CStr(IIf(App.isRiskEnabled AndAlso isImpact, "lblKnownImpactPW", "lblKnownLikelihoodPW"))))) ' D2999 + D3051 + D3647 + D4410 // for parse known likelihood text

                            'Case ActionType.atSpyronSurvey
                            '    sType = "survey"

                        Case ActionType.atNonPWOneAtATime
                            Select Case CType(tAction.ActionData, clsNonPairwiseEvaluationActionData).MeasurementType

                                Case ECMeasureType.mtRatings
                                    sType = "rating"
                                    sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll) ' D1270 + D1271 + D4825

                                Case ECMeasureType.mtRegularUtilityCurve
                                    sType = "ruc"
                                    sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll)  ' D1577 + D4825

                                Case ECMeasureType.mtDirect
                                    sType = "direct"
                                    sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll)  ' D1509 + D4825

                                Case ECMeasureType.mtStep
                                    sType = "step"
                                    sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll)  ' D1546 + D4825

                            End Select

                        Case ActionType.atSensitivityAnalysis
                            '' -D3476 due to current SA doesn't work properly
                            '' D1326 + D1414 ===
                            'sType = "dsa"
                            'Dim tSens As clsSensitivityAnalysisActionData = CType(tAction.ActionData, clsSensitivityAnalysisActionData)
                            'Select Case tSens.SAType
                            '    Case SAType.satDynamic
                            '        sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll)
                            '    Case SAType.satGradient
                            '    Case SAType.satPerformance
                            'End Select
                            ''If sValue <> "" Then sTask = GetTask_SensitivityAnalysis(tSens)
                            '' D1326 + D1414 ==

                        Case ActionType.atShowLocalResults
                            sType = "localresults"
                            ' D1799 ===
                            If TeamTime.Override_ResultsMode Then
                                TeamTime.ResultsViewMode = IIf(TeamTimeHideCombined, ResultsView.rvIndividual, ResultsView.rvBoth)
                            End If
                            ' D1799 ===
                            sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll) ' D1270 + D1271 + D4825

                        Case ActionType.atShowGlobalResults
                            sType = "globalresults"
                            ' D1799 ===
                            If TeamTime.Override_ResultsMode Then
                                TeamTime.ResultsViewMode = IIf(TeamTimeHideCombined, ResultsView.rvIndividual, ResultsView.rvBoth)
                            End If
                            ' D1799 ===
                            sValue = TeamTime.GetJSON(TeamTimeUsersList(), _TeamTimeUsersListAll)  ' D1270 + D1271 + D4825

                    End Select

                    If sType <> "" Then sTask = GetPipeStepTask(tAction, Nothing, False, False, False, (TeamTimePipeParams.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame OrElse sType = "pairwise" OrElse sType.Contains("results"))) ' D2316 + D2972 + D3487

                    ' D3476 ===
                    If sType <> "" AndAlso sValue = "" Then
                        sType = "info"
                        sValue = String.Format("'<h6 class=\'gray\'>{0}</h6>'", JS_SafeString(ResString("errLoadingStepData")))
                    End If
                    ' D3476 ==

                    If sValue = "" Then
                        sType = "info"
                        sValue = String.Format(String.Format("'<h6 class=\'gray\'>{0}</h6>'", JS_SafeString(ResString("msgTeamTimeNotSupported"))), tAction.ActionType.ToString) ' D1270 + D2105
                    End If

                    ' D1289 ===
                    Dim sEvalProgress As String = ""
                    If isTeamTime Then
                        For Each tUser As clsUser In TeamTimeUsersList
                            ' D2577 ===
                            Dim sTime As String = ""
                            Dim tDateTime As Nullable(Of DateTime) = TeamTime.GetUserJudgmentDateTime(tUser)
                            If tDateTime.HasValue Then sTime = GetDateTimeTicks(tDateTime.Value)
                            sEvalProgress += CStr(IIf(sEvalProgress = "", "", ",")) + String.Format("['{0}',{1},{2},'{3}']", JS_SafeString(tUser.UserEMail), TeamTime.GetUserEvaluatedCount(tUser), TeamTime.GetUserTotalCount(tUser), sTime)
                            ' D2577 ==
                        Next
                    End If
                    ' D1289 ==

                    DebugInfo("Prepare JSON")

                    ' Prepare and save to DB from session owner
                    With TeamTimePipeParams ' D1276
                        sData = String.Format("['active',{0}],['progress',{1},{2},[{3}],'{22}','{23}',{24}],['options',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{19},{20},{21},{25}],['data','{15}','{16}','{17}',{18}]",
                                              IIf(isTeamTime, 1, 0),
                                              TeamTimePipeStep, TeamTimePipe.Count, sEvalProgress,
                                              TeamTimeRefreshTimeout, IIf(.ShowComments, 1, 0), IIf(.ShowInfoDocs, CInt(.ShowInfoDocsMode), -1),
                                              IIf(.SynchStartInPollingMode, 1, 0), IIf(.TeamTimeStartInAnonymousMode, 1, 0),
                                              IIf(.SynchUseVotingBoxes AndAlso App.Options.KeypadsAvailable, 1, 0), CInt(.TeamTimeUsersSorting),
                                              IIf(TeamTimeShowVarianceInPollingMode, 1, 0),
                                              IIf(TeamTimeShowPWSideKeypadsInPollingMode, 1, 0),
                                              IIf(.TeamTimeDisplayUsersWithViewOnlyAccess, 1, 0),
                                              IIf(.TeamTimeHideProjectOwner, 1, 0),
                                              sType, JS_SafeString(PrepareTask(sTask)),
                                              tAction.StepGuid.ToString, sValue, App.ActiveProject.MeetingOwnerID,
                                              IIf(PM.Parameters.TTHideOfflineUsers, 1, 0), IIf(PM.Parameters.EvalHideInfodocCaptions, 1, 0),
                                              JS_SafeString(TeamTimeEvalProgressUser.ToLower), JS_SafeString(GetTeamTimeEvalProgressUserName()), GetNextUnassessed, App.ProjectID)  ' D1276 + D1281 +  D1298 + D1527 + D1561 + D1654 + D1658 + D1976 + D3506 + D3929 + D4555 + D4583 + D4807 + D4958 + D6418 + D6429
                    End With

                    App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, sCallbackSave + sData)    ' D1275 + D2578
                    'TeamTimeUpdateStepInfo(TeamTimePipeStep)
                    ' D1289 ===
                Else
                    sData = "['action', 'refresh']"
                    ' D1289 ==
                End If

            Else

                ' Get from DB for participants
                Dim fProjectAvailable As Boolean = Not (App.ActiveProject.LockInfo IsNot Nothing AndAlso App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForModify)
                Dim sMsg As String = ""

                If fProjectAvailable Then
                    ' Check date time for last session data
                    ' D1275 ===
                    Dim DT As Nullable(Of DateTime) = Nothing
                    sData = App.DBTeamTimeDataRead(App.ProjectID, App.ActiveProject.MeetingOwnerID, ecExtraProperty.TeamTimeSessionData, DT)    ' D1280
                    If Not DT.HasValue OrElse DT.Value.AddSeconds(TeamTimeRefreshTimeout * _TeamTimeSessionDataTimeoutCoeff) < Now Then sData = "" ' D1280
                    ' D1275 ==

                    If String.IsNullOrEmpty(sData) Then sMsg = ResString("msgSynchronousWaitOwner") ' D1280
                Else
                    sMsg = "Project is temporary locked for update. Please wait session owner…"
                End If
                If sData = "" AndAlso sMsg <> "" Then sData = String.Format("['warning','{0}']", JS_SafeString(sMsg))
            End If

        Else
            'If Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) Then sData = String.Format("['active',0],['warning','{0}']", GetStartupMessage) ' D1289
            'sData = String.Format("['active',0],['warning','{0}']", GetStartupMessage) ' D1289

            'If App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) Then sData = "['active',0],['action','stop']" Else sData = String.Format("['active',0],['warning','{0}']", GetStartupMessage) ' D1289 + D1604 -D2043
            ' Always show message for avoid to show "Meeting is ended" when loggen into inactive TT session.

            ' -D2943
            'If App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) Then
            '    sData = "['active',0],['action','stop']"
            'Else
            sData = String.Format("['active',0],['warning','{0}']", JS_SafeString(GetStartupMessage)) ' D1289 + D1604 + D2043 + D3504
            'End If

        End If
        DebugInfo("TT Step Data completed")

        ' D2538 ===
        If App.ActiveUser.Session IsNot Nothing Then
            If App.ActiveUser.Session.LastAccess.HasValue AndAlso App.ActiveUser.Session.LastAccess.Value.AddSeconds(Math.Round(_DEF_SESS_TIMEOUT / 4)) < Now Then
                App.ActiveUser.Session.LastAccess = Now
                App.DBUpdateDateTime(clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_LASTVISITED, App.ActiveUser.UserID, clsComparionCore._FLD_USERS_ID)
            End If
        End If
        ' D2538 ==

        Return sData
    End Function
    ' D1252 ==

#End Region

#Region "AJAX section"

#If Not OPT_USE_AJAX_INSTEAD_CALLBACK Then
    Protected Sub ASPxCallbackTeamTime_Callback(source As Object, e As CallbackEventArgs) Handles ASPxCallbackTeamTime.Callback
        DebugInfo("Start callback: " + e.Parameter)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(e.Parameter))
        e.Result = onCallback(args) ' D3998
    End Sub
#End If

    Protected Function onCallback(args As NameValueCollection) As String    ' D3998
        Dim sResult As String = ""
        Dim sAction As String = GetParam(args, _PARAM_ACTION).ToLower

        If _TeamTimeActive Then ' D1275
            Select Case sAction

                Case "refresh"
                    Dim sRefresh As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")) ' Anti-XSS
                    Dim tRefresh As Integer = TeamTimeRefreshTimeout
                    If isTeamTimeOwner AndAlso Integer.TryParse(sRefresh, tRefresh) Then TeamTimeRefreshTimeout = tRefresh ' D1280

                    ' D1280 ===
                Case "pause"
                    If isTeamTimeOwner Then
                        App.DBTeamTimeDataDelete(App.ProjectID, Nothing, ecExtraProperty.TeamTimeSessionData, -1)
                        App.DBTeamTimeDataDelete(App.ProjectID, Nothing, ecExtraProperty.TeamTimeStepData)  ' D1348
                    End If
                    ' D1280 ==

                    ' D1280 ===
                Case "start"
                    If isTeamTimeOwner Then
                        'ResetProject()  ' D1288 - D2611
                    End If
                    ' D1280 ==

                    ' D3482 ===
                Case "do_start"
                    If CanStartTTMeeting() Then
                        App.TeamTimeResumeSession(App.ActiveUser, App.ActiveProject, PM.ActiveHierarchy, Nothing, Nothing, SessionsList, IsConsensus) ' D6553
                    End If
                    ' D3482 ==

                    ' D1276 ===
                Case "hidejudgments"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then    ' D1280
                        TeamTimePipeParams.SynchStartInPollingMode = (sValue.ToLower = True.ToString.ToLower Or sValue.ToLower = "true" Or sValue = "1" Or sValue.ToLower = "yes")
                        App.ActiveProject.SaveProjectOptions("Switch TT polling mode", , False)  ' D1279
                    End If

                Case "anonymous"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then    ' D1280
                        TeamTimePipeParams.TeamTimeStartInAnonymousMode = (sValue.ToLower = True.ToString.ToLower Or sValue.ToLower = "true" Or sValue = "1" Or sValue.ToLower = "yes")
                        App.ActiveProject.SaveProjectOptions("Switch TT Anonymous mode", , False)  ' D1279
                    End If
                    ' D1276 ==

                    ' D4554 ===
                Case "eval_prg_user"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        TeamTimeEvalProgressUser = sValue
                        'App.ActiveProject.SaveProjectOptions("Set TT evaluation progress user", , False)  ' D1279
                    End If
                    ' D4554 ==

                    ' D1516 ===
                Case "sorting"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then    ' D1280
                        Dim idx As Integer = CInt(TeamTimePipeParams.TeamTimeUsersSorting)
                        If Integer.TryParse(sValue, idx) AndAlso idx >= 0 AndAlso idx <= 2 AndAlso idx <> CInt(TeamTimePipeParams.TeamTimeUsersSorting) Then
                            TeamTimePipeParams.TeamTimeUsersSorting = CType(idx, TTUsersSorting)
                            App.ActiveProject.SaveProjectOptions("Change TT sorting mode", , False)
                            'TeamTime.Users.Clear()
                            _TeamTimeUsersList = Nothing
                        End If
                    End If
                    ' D1561 ==

                    ' D1654 ===
                Case "variance"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        TeamTimeShowVarianceInPollingMode = (sValue = "1")
                    End If

                    ' D1658 ===
                Case "pwside"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        TeamTimeShowPWSideKeypadsInPollingMode = (sValue = "1")
                    End If
                    ' D1658 ==

                    ' D1799 ===
                Case "nocombined"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        TeamTimeHideCombined = (sValue = "1")
                    End If
                    ' D1799 ==

                Case "viewonly"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        TeamTimePipeParams.TeamTimeDisplayUsersWithViewOnlyAccess = (sValue.ToLower = True.ToString.ToLower Or sValue.ToLower = "true" Or sValue = "1" Or sValue.ToLower = "yes")
                        App.ActiveProject.SaveProjectOptions("Change TT option for ViewOnly", , False)
                    End If

                Case "hideowner"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        TeamTimePipeParams.TeamTimeHideProjectOwner = (sValue.ToLower = True.ToString.ToLower Or sValue.ToLower = "true" Or sValue = "1" Or sValue.ToLower = "yes")
                        App.ActiveProject.SaveProjectOptions("Switch TT 'Hide Owner' option", , False)
                    End If
                    ' D1654 ==

                    ' D3929 ===
                Case "hideoffline"
                    Dim sValue As String = GetParam(args, "value")
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        PM.Parameters.TTHideOfflineUsers = Str2Bool(sValue)
                        PM.Parameters.Save()
                        _TeamTimeUsersList = Nothing    ' D3931
                        ' D4001 ===
                        TeamTime.ClearStepData()
                        TeamTime.ClearTempData(True)    ' D4548
                        If PM.Parameters.TTHideOfflineUsers Then TeamTime.SetCurrentStep(TeamTimePipeStep - 1, TeamTimeUsersList)
                        ' D4001 ==
                        'App.SaveProjectLogEvent(App.ActiveProject, "Switch TT 'Hide off-line users' option", False, sValue)
                    End If
                    ' D3929 ==

                    ' D2966 ===
                Case "infodocmode"
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    If isTeamTimeOwner AndAlso sValue IsNot Nothing Then
                        If (sValue = "1") Then TeamTimePipeParams.ShowInfoDocsMode = ShowInfoDocsMode.sidmFrame Else TeamTimePipeParams.ShowInfoDocsMode = ShowInfoDocsMode.sidmPopup
                        App.ActiveProject.SaveProjectOptions("Switch infodocs mode", , False)
                    End If
                    ' D2966 ==

                Case "save"
                    Dim sStep As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "step")) ' Anti-XSS
                    Dim sGUID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "guid"))    ' D1281 + Anti-XSS
                    Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value"))   ' Anti-XSS
                    Dim sUID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id"))    ' Anti-XSS
                    Dim sComment As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "comment"))   ' Anti-XSS
                    Dim sPendingID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "pending"))    ' D2578 + Anti-XSS
                    Dim UID As Integer = -1
                    Dim St As Integer = -1
                    If Integer.TryParse(sStep, St) AndAlso Integer.TryParse(sUID, UID) AndAlso sGUID <> "" Then
                        Dim tUser As clsUser = PM.GetUserByID(UID)
                        'If tUser IsNot Nothing AndAlso St > 0 AndAlso St <= TeamTimePipe.Count Then
                        If tUser IsNot Nothing Then ' D3066
                            If isTeamTimeOwner Or tUser.UserEMail.ToLower = App.ActiveUser.UserEmail.ToLower Then   ' D1280
                                Dim sJudgment As String = String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{6}{0}{5}", clsTeamTimePipe.Judgment_Delimeter, UID, St - 1, sGUID, sValue, sComment, sPendingID)    ' D1281
                                App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveUser.UserID, ecExtraProperty.TeamTimeJudgment, sJudgment, False)
                                If App.ActiveProject.LockInfo IsNot Nothing Then
                                    Dim tOwner As clsApplicationUser = App.ActiveProject.MeetingOwner
                                    If tOwner Is Nothing Then tOwner = clsApplicationUser.UserByUserID(App.ActiveProject.MeetingOwnerID, UsersList)
                                    If tOwner IsNot Nothing Then App.DBProjectLockInfoWrite(ECLockStatus.lsLockForTeamTime, App.ActiveProject.LockInfo, App.ActiveProject.MeetingOwner, Now.AddSeconds(If(IsConsensus, _DEF_LOCK_TT_CONSENSUS_VIEW, _DEF_LOCK_TT_JUDGMENT_TIMEOUT)))    ' D6553
                                End If
                            End If
                        End If
                    End If

                    ' D1290 ===
                Case "global_wrt"
                    Dim sID As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "id")) ' Anti-XSS
                    Dim ID As Integer = -1
                    If Integer.TryParse(sID, ID) Then
                        Dim tNode As clsNode = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(ID)
                        If tNode IsNot Nothing Then TeamTime.WRTNodeID = ID
                    End If
                    ' D1290 ==

                    '    ' D1417 ===
                    'Case "dsa_recalc"
                    '    Dim sCnt As String = GetParam(args, "cnt")
                    '    Dim Cnt As Integer = 0
                    '    If Integer.TryParse(sCnt, Cnt) Then
                    '        Dim Nodes As New Dictionary(Of Integer, Single)
                    '        For i As Integer = 0 To Cnt - 1
                    '            Dim sID As String = GetParam(args, "id" + CStr(i))
                    '            Dim sVal As String = GetParam(args, "val" + CStr(i))
                    '            Dim nID As Integer = -1
                    '            Dim tVal As Single = -1
                    '            If Integer.TryParse(sID, nID) AndAlso Single.TryParse(sVal, tVal) Then
                    '                Nodes.Add(nID, tVal)
                    '            End If
                    '        Next
                    '        If Nodes.Count > 0 Then TeamTime.SetDSAPriorities(TeamTime.WRTNodeID, Nodes)
                    '    End If
                    '    ' D1417 ==

                    '    ' D1418 ===
                    'Case "dsa_wrtnode"
                    '    Dim sID As String = GetParam(args, "id")
                    '    Dim ID As Integer = 0
                    '    If Integer.TryParse(sID, ID) Then
                    '        Dim tNode As clsNode = App.ActiveProject.HierarchyObjectives.GetNodeByID(ID)
                    '        If tNode IsNot Nothing AndAlso Not tNode.IsTerminalNode Then TeamTime.WRTNodeID = ID
                    '    End If
                    '    ' D1418 ==

            End Select

            Dim sData As String = ""
            If sAction <> "pause" Then sData = GetPipeData().Trim ' D1280
            Dim sHash As String = ""

            If sData > "" Then
                Dim sData4Hash As String = sData
                Dim pIDx As Integer = -1
                If sData.StartsWith("['pended',['") Then pIDx = sData.IndexOf("']],[")
                If pIDx > 0 Then sData4Hash = sData4Hash.Substring(pIDx + 4)
                sHash = GetMD5(sData4Hash)   ' D1261
                Dim sClientHash As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "hash")).ToLower   ' Anti-XSS
                If sClientHash <> "" AndAlso sClientHash = sHash Then sData = ""
                ' D4958 ===
                Dim pid As String = GetParam(args, "pid")
                If pid <> "" AndAlso pid <> App.ProjectID.ToString Then
                    sData = "['action', 'refresh']"
                    sHash = GetMD5(sData)
                End If
                ' D4958 ==
            End If

            If sData <> "" Then sData = "," + sData

            sResult = String.Format("[['time','{0}'],{3}['hash','{1}']{2}]", GetDateTimeTicks(Now), sHash, sData, sCallbackSave)    ' D1246 + D1247 + D1251 + D1252 + D2577 + D2578
        End If
        DebugInfo("Callback processed")

        Return sResult  ' D3998
    End Function

#End Region

End Class