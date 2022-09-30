Option Strict On

Partial Class TeamTimeInvitePage
    Inherits clsComparionCorePage

    ' D7561 ===
    Private _ProjectUsers As List(Of clsApplicationUser) = Nothing
    Private _ProjectWorkspaces As List(Of clsWorkspace) = Nothing
    Private _ProjectUserWorkgroups As List(Of clsUserWorkgroup) = Nothing
    Private _AdminGroupID As Integer = -1
    ' D7516 ==
    Private CanManageWorkgroup As Boolean = False           ' D4965

    Public Sub New()
        MyBase.New(_PGID_TEAMTIME_INVITE)
    End Sub

    ' D7670 ===
    Private Function isUserSelected(tUser As clsApplicationUser) As Boolean
        Dim tWs As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ProjectID, ProjectWorkspaces)
        Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.GetUserByEMail(tUser.UserEmail)
        Return tUser.Status <> ecUserStatus.usDisabled AndAlso tWs.Status(App.ActiveProject.isImpact) <> ecWorkspaceStatus.wsDisabled AndAlso CanEvaluate(tUser.UserID) AndAlso CanShowUserLink(tUser) AndAlso
               ((App.isAuthorized AndAlso tUser.UserID = App.ActiveUser.UserID) OrElse (tPrjUser IsNot Nothing AndAlso tPrjUser.SyncEvaluationMode <> SynchronousEvaluationMode.semNone))
    End Function
    ' D7670 ==

    ' D7561 ===
    Public ReadOnly Property ProjectUsers As List(Of clsApplicationUser)
        Get
            If _ProjectUsers Is Nothing Then
                _ProjectUsers = App.DBUsersByProjectID(App.ProjectID)
                _ProjectUsers.Sort(New clsApplicationUserComparer(ecApplicationUserSort.usName, SortDirection.Ascending))  ' D7101
            End If
            Return _ProjectUsers
        End Get
    End Property

    Public ReadOnly Property ProjectWorkspaces As List(Of clsWorkspace)
        Get
            If _ProjectWorkspaces Is Nothing Then
                _ProjectWorkspaces = App.DBWorkspacesByProjectID(App.ProjectID)
            End If
            Return _ProjectWorkspaces
        End Get
    End Property

    Public ReadOnly Property ProjectUserWorkgroups As List(Of clsUserWorkgroup)
        Get
            If _ProjectUserWorkgroups Is Nothing Then
                _ProjectUserWorkgroups = App.DBUserWorkgroupsByProjectIDWorkgroupID(App.ProjectID, App.ActiveWorkgroup.ID)
            End If
            Return _ProjectUserWorkgroups
        End Get
    End Property

    Public ReadOnly Property AdminGroupID As Integer
        Get
            If _AdminGroupID <= 0 Then _AdminGroupID = App.ActiveWorkgroup.RoleGroupID(ecRoleGroupType.gtAdministrator, App.ActiveWorkgroup.RoleGroups)
            Return _AdminGroupID
        End Get
    End Property

    Public Function isAdmin(UserID As Integer) As Boolean
        Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups)
        Return tUW IsNot Nothing AndAlso tUW.RoleGroupID = AdminGroupID
    End Function

    ' D7111 ===
    Public Function CanEvaluate(UserID As Integer) As Boolean
        Return App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, UserID, App.ProjectID, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups), App.ActiveWorkgroup)
    End Function
    ' D7111 ==

    Public Function CanShowUserLink(tUser As clsApplicationUser) As Boolean
        ' D4965 ===
        If tUser IsNot Nothing AndAlso tUser.Status <> ecUserStatus.usDisabled AndAlso Not isAdmin(tUser.UserID) Then
            If CanManageWorkgroup OrElse App.ActiveUser.UserID = tUser.UserID OrElse Not App.CanUserModifyProject(tUser.UserID, App.ProjectID, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups), clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ProjectID, ProjectWorkspaces), App.ActiveWorkgroup) Then
                Return True
            End If
        End If
        Return False
        ' D4965 ==
    End Function
    ' D7516 ==

    Private Sub TeamTimeInvitePage_Load(sender As Object, e As EventArgs) Handles Me.Load
        If isAJAX Then
            Dim tRes As New jActionResult

            Select Case CheckVar(_PARAM_ACTION, "").ToLower

                Case "reset"
                    App.ActiveProject.PipeParameters.TeamTimeInvitationSubject2 = ""
                    App.ActiveProject.PipeParameters.TeamTimeInvitationBody2 = ""
                    App.ActiveProject.SaveProjectOptions("Reset TeamTime invitation")
                    tRes.Result = ecActionResult.arSuccess

                Case "subject"
                    Dim Val As String = CheckVar("val", "")
                    App.ActiveProject.PipeParameters.TeamTimeInvitationSubject2 = Val
                    App.ActiveProject.SaveProjectOptions("TeamTime invitation Subject changed")
                    tRes.Result = ecActionResult.arSuccess

                    ' D4789 ===
                Case "send"
                    tRes.ObjectID = 0
                    Dim sErrors As String = ""
                    Dim sResponsive As Boolean = Not CheckVar("ignoreval", False)  ' D4926
                    Dim sText As String = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.ExtraInfo, _PARAM_INVITATION_TT, App.ActiveProject.PipeParameters.TeamTimeInvitationBody2, True, True, -1)
                    If sText = "" Then sText = App.ResString("bodySynchronousInvitations")
                    Dim sEvalURL As String = App.Options.EvalSiteURL    ' D4926
                    If Not sResponsive Then App.Options.EvalSiteURL = ""    ' D4926
                    Dim Subj As String = GetSubject()
                    For Each tUser As clsApplicationUser In ProjectUsers
                        ' D7670 ===
                        'Dim tWs As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ProjectID, ProjectWorkspaces)
                        'Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.Users.GetUserByEMail(tUser.UserEmail)
                        'If tUser.Status <> ecUserStatus.usDisabled AndAlso tWs.Status(App.ActiveProject.isImpact) <> ecWorkspaceStatus.wsDisabled AndAlso CanEvaluate(tUser.UserID) AndAlso (CanShowUserLink(tUser) AndAlso ((App.isAuthorized AndAlso tUser.UserID = App.ActiveUser.UserID) OrElse (tWs IsNot Nothing AndAlso tWs.isInTeamTime(App.ActiveProject.isImpact)) OrElse (tPrjUser IsNot Nothing AndAlso tPrjUser.SyncEvaluationMode <> SynchronousEvaluationMode.semNone))) Then   ' D7101 + D7111
                        If isUserSelected(tUser) Then
                            ' D7670 ==
                            Dim sSubject As String = ParseAllTemplates(Subj, tUser, App.ActiveProject)
                            Dim sBody As String = ParseAllTemplates(sText, tUser, App.ActiveProject)
                            Dim sError As String = ""
                            If SendMail(WebOptions.SystemEmail, tUser.UserEmail, sSubject, sBody, sError, "", isHTML(sBody), SMTPSSL) Then   ' D6078
                                tRes.ObjectID += 1
                            Else
                                sErrors += String.Format("{0}: {1}{2}", tUser.UserEmail, sError, vbNewLine)
                            End If
                        End If
                    Next
                    App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfProject, App.ProjectID, "Send TeamTime Invitations", SubString(String.Format("Sent: {0}" + vbCrLf, "Error(s): {1}", tRes.ObjectID, tRes.Data), 990))
                    App.Options.EvalSiteURL = sEvalURL      ' D4926
                    tRes.Data = sErrors
                    tRes.Result = ecActionResult.arSuccess
                    ' D4789 ==

            End Select

            RawResponseStart()
            Response.ContentType = "text/plain"
            Response.Write(JsonConvert.SerializeObject(tRes))
            Response.End()
        End If

    End Sub

    Public Function GetSubject() As String
        Dim sValue As String = App.ActiveProject.PipeParameters.TeamTimeInvitationSubject2
        If sValue = "" Then sValue = ResString("subjectTeamTimeInvitation")
        Return sValue
    End Function

    ' D4761 ===
    Public Function GetSelectedUserIDs() As String
        Dim sLst As String = ""
        For Each tUser As clsApplicationUser In ProjectUsers
            ' D7670 ===
            'Dim tWs As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ProjectID, ProjectWorkspaces)
            'Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.Users.GetUserByEMail(tUser.UserEmail)
            'Dim Mode As SynchronousEvaluationMode = clsWorkspace.TeamTimStatusAsSyncMode(tWs.TeamTimeStatus(App.ActiveProject.isImpact))    ' D7130
            'If tUser.Status <> ecUserStatus.usDisabled AndAlso (Mode <> SynchronousEvaluationMode.semNone OrElse tPrjUser.SyncEvaluationMode <> SynchronousEvaluationMode.semNone OrElse (App.isAuthorized AndAlso tUser.UserID = App.ActiveUser.UserID)) AndAlso CanEvaluate(tUser.UserID) AndAlso CanShowUserLink(tUser) Then   ' D7130
            If isUserSelected(tUser) Then
                ' D7670 ==
                sLst += String.Format("{0}{1}", If(sLst = "", "", ","), tUser.UserID)
            End If
        Next
        Return sLst
    End Function

    ' D7101 ===
    Public Function GetSelectedUsers() As String
        Dim sLst As String = ""
        For Each tUser As clsApplicationUser In ProjectUsers
            ' D7670 ===
            'Dim tWs As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tUser.UserID, App.ProjectID, ProjectWorkspaces)
            'Dim tPrjUser As clsUser = App.ActiveProject.ProjectManager.Users.GetUserByEMail(tUser.UserEmail)
            'Dim Mode As SynchronousEvaluationMode = clsWorkspace.TeamTimStatusAsSyncMode(tWs.TeamTimeStatus(App.ActiveProject.isImpact))    ' D7130
            'If tUser.Status <> ecUserStatus.usDisabled AndAlso (Mode <> SynchronousEvaluationMode.semNone OrElse tPrjUser.SyncEvaluationMode <> SynchronousEvaluationMode.semNone OrElse (App.isAuthorized AndAlso tUser.UserID = App.ActiveUser.UserID)) AndAlso CanEvaluate(tUser.UserID) AndAlso CanShowUserLink(tUser) Then   ' D7130
            If isUserSelected(tUser) Then
                Dim sName As String = If(String.IsNullOrEmpty(tUser.UserName), tUser.UserName, tUser.UserName)
                ' D7670 ==
                sLst += String.Format("<li>{0}&lt;{1}&gt;</li>", JS_SafeString(If(String.IsNullOrEmpty(sName) OrElse sName = tUser.UserEmail, "", sName + " ")), JS_SafeString(HTMLEmailLink(tUser.UserEmail, tUser.UserEmail))) ' D7111 + D7113
            End If
        Next
        Return sLst
    End Function
    '  D7101 ==

    Public Function GetUserLinks() As String
        Dim tResult As String = ""
        Dim sURL As String = ApplicationURL(False, True)
        For Each user As clsApplicationUser In ProjectUsers
            If CanShowUserLink(user) AndAlso CanEvaluate(user.UserID) Then  ' D7111
                Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(user.UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups)
                tResult += String.Format("{0}{1}{2}{3}", CreateLogonURL(user, App.ActiveProject, "TTOnly=1", sURL, , tUW, True), vbTab, user.UserEmail, vbNewLine)
            End If
        Next
        Return JS_SafeString(tResult, True) ' D4797
    End Function
    ' D4761 ==

    ' D4965 ===
    Private Sub TeamTimeInvitePage_Init(sender As Object, e As EventArgs) Handles Me.Init
        CanManageWorkgroup = App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup, App.ActiveWorkgroup)
    End Sub
    ' D4965 ==

End Class