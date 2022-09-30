Option Strict On

Partial Class AnytimeInvitePage
    Inherits clsComparionCorePage
    Private _ProjectUsers As List(Of clsApplicationUser) = Nothing
    Private _ProjectWorkspaces As List(Of clsWorkspace) = Nothing
    Private _ProjectUserWorkgroups As List(Of clsUserWorkgroup) = Nothing
    Private _AdminGroupID As Integer = -1
    Private CanManageWorkgroup As Boolean = False           ' D4965

    Public Sub New()
        MyBase.New(_PGID_EVALUATE_INVITE)
    End Sub

    Public ReadOnly Property ProjectUsers As List(Of clsApplicationUser)
        Get
            If _ProjectUsers Is Nothing Then
                _ProjectUsers = App.DBUsersByProjectID(App.ProjectID)
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

    ' D6263 ===
    Public ReadOnly Property isResponsiveOnly As Boolean
        Get
            Return App.Options.EvalSiteOnly ' D6359
        End Get
    End Property
    ' D6263 ==

    Public ReadOnly Property CanSeeLinks As Boolean
        Get
            Return Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY
        End Get
    End Property

    Public Function isAdmin(UserID As Integer) As Boolean
        Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups)
        Return tUW IsNot Nothing AndAlso tUW.RoleGroupID = AdminGroupID
    End Function

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

    Public Function GetParticipantsList() As String
        Dim retVal As String = ""
        Dim madeAllCount As Integer = 0
        Dim totalAllCount As Integer = 0
        Dim evalProgress As Dictionary(Of String, UserEvaluationProgressData) = PM.StorageManager.Reader.GetEvaluationProgress(PM.UsersList, PM.ActiveHierarchy, madeAllCount, totalAllCount)

        Dim defTotalCount As Integer = PM.GetDefaultTotalJudgmentCount(PM.ActiveHierarchy)

        For Each User As clsApplicationUser In ProjectUsers
            Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(User.UserID, App.ProjectID, ProjectWorkspaces) ' D4965
            Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(User.UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups) ' D4763 + D4965
            Dim tPrjUser As ECTypes.clsUser = clsApplicationUser.AHPUserByUserEmail(User.UserEmail, PM.UsersList)
            Dim tGroup As clsRoleGroup = Nothing
            Dim groupsList As String = ""
            Dim eProgress As Double = 0
            If tWS IsNot Nothing Then tGroup = App.ActiveWorkgroup.RoleGroup(tWS.GroupID, App.ActiveWorkgroup.RoleGroups)
            Dim madeCount As Integer = 0
            Dim totalCount As Integer = defTotalCount
            If tPrjUser IsNot Nothing Then
                User.UserName = tPrjUser.UserName
                For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
                    If group.CombinedUserID <> -1 AndAlso String.IsNullOrEmpty(group.Rule) Then ' D7094

                        If group.UsersList.Contains(tPrjUser) Then
                            groupsList += CStr(IIf(groupsList <> "", ", ", "")) + group.Name
                        End If
                    End If
                Next
                If evalProgress.ContainsKey(tPrjUser.UserEMail.ToLower) Then
                    With evalProgress(tPrjUser.UserEMail.ToLower)
                        madeCount = .EvaluatedCount
                        totalCount = .TotalCount
                    End With
                End If
                If totalCount > 0 Then
                    eProgress = madeCount / totalCount
                End If
            End If

            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{name:'{0}',email:'{1}',hasdata:{2},id:{3},groups:""{4}"",progress:{5}}}", JS_SafeString(User.UserName), JS_SafeString(User.UserEmail), If(madeCount > 0, 1, 0), User.UserID, groupsList, JS_SafeNumber(eProgress))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Private Sub InvitePage_Load(sender As Object, e As EventArgs) Handles Me.Load
        If isAJAX Then
            Dim tRes As New jActionResult

            Select Case CheckVar(_PARAM_ACTION, "").ToLower

                Case "reset"
                    App.ActiveProject.PipeParameters.EvaluateInvitationSubject2 = ""
                    App.ActiveProject.PipeParameters.EvaluateInvitationBody2 = ""
                    App.ActiveProject.SaveProjectOptions("Reset Anytime invitation")
                    tRes.Result = ecActionResult.arSuccess

                Case "subject"
                    Dim Val As String = CheckVar("val", "")
                    App.ActiveProject.PipeParameters.EvaluateInvitationSubject2 = Val
                    App.ActiveProject.SaveProjectOptions("Anytime invitation Subject changed")
                    tRes.Result = ecActionResult.arSuccess

                Case "signuptitle"
                    Dim Val As String = CheckVar("val", "")
                    PM.Parameters.InvitationCustomTitle = Val
                    App.ActiveProject.SaveProjectOptions("Signup Title changed")
                    tRes.Result = ecActionResult.arSuccess

                Case "sendlocal"
                    Dim uID As Integer = CheckVar("uid", -1)

                    Dim sText As String = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.ExtraInfo, _PARAM_INVITATION_EVAL, App.ActiveProject.PipeParameters.EvaluateInvitationBody2, True, True, -1)
                    If sText = "" Then sText = App.ResString(If(App.isRiskEnabled, "bodyDecisionInvitationsRisk", "bodyDecisionInvitations"))
                    Dim Subj As String = GetSubject()
                    For Each tUser As clsApplicationUser In ProjectUsers
                        If tUser.UserID = uID Then
                            Dim sSubject As String = ParseAllTemplates(Subj, tUser, App.ActiveProject)
                            Dim sBody As String = ParseAllTemplates(sText, tUser, App.ActiveProject)
                            If isHTML(sBody) Then sBody = HTML2Text(sBody)  ' D7617
                            tRes.Data = JS_SafeString(String.Format("mailto:{0}?subject={1}&body={2}", HttpUtility.UrlEncode(tUser.UserEmail), HttpUtility.UrlEncode(sSubject), HttpUtility.UrlEncode(sBody)))
                            Exit For
                        End If
                    Next
                    tRes.Result = ecActionResult.arSuccess

                Case "send"
                    Dim sResponsive As Boolean = Not CheckVar("ignoreval", False)  ' D4926
                    Dim strUsers As String() = CheckVar("users", "").Split(CChar(","))
                    Dim sErrors As String = ""
                    Dim sText As String = Infodoc_Unpack(App.ProjectID, App.ActiveProject.ProjectManager.ActiveHierarchy, reObjectType.ExtraInfo, _PARAM_INVITATION_EVAL, App.ActiveProject.PipeParameters.EvaluateInvitationBody2, True, True, -1)
                    If sText = "" Then sText = App.ResString(If(App.isRiskEnabled, "bodyDecisionInvitationsRisk", "bodyDecisionInvitations"))
                    Dim sEvalURL As String = App.Options.EvalSiteURL    ' D4926
                    If Not sResponsive Then App.Options.EvalSiteURL = ""    ' D4926
                    Dim Subj As String = GetSubject()
                    Dim sSummary As String = "" ' D6439
                    Dim Count As Integer = 0    ' D6439
                    For Each tUser As clsApplicationUser In ProjectUsers
                        If strUsers.Contains(tUser.UserID.ToString) Then
                            Dim sSubject As String = ParseAllTemplates(Subj, tUser, App.ActiveProject)
                            Dim sBody As String = ParseAllTemplates(sText, tUser, App.ActiveProject)
                            Dim sError As String = ""
                            If SendMail(WebOptions.SystemEmail, tUser.UserEmail, sSubject, sBody, sError, "", isHTML(sBody), SMTPSSL) Then  ' D6078
                                sSummary += String.Format("{0}: {2}" + vbNewLine, tUser.UserEmail, vbTab, ParseAllTemplates(_TEMPL_URL_EVALUATE, tUser, App.ActiveProject)) ' D6439
                                Count += 1  ' D6439
                            Else
                                sErrors += String.Format("{0}: {2}" + vbNewLine, tUser.UserEmail, vbTab, sError)   ' D6439
                            End If
                        End If
                    Next
                    App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfProject, App.ProjectID, "Send Anytime Invitations", SubString(String.Format("Sent: {0}" + vbCrLf + If(String.IsNullOrEmpty(sErrors), "", "; Error(s): {1}"), Count, sErrors), 990))  ' D7443
                    ' D6439 ===
                    If ProjectUsers.Count > 0 Then
                        Dim sResume As String = ParseAllTemplates(ResString("bodyAnyTimeInvitationSummary"), App.ActiveUser, App.ActiveProject)
                        Dim sResumeSubject As String = ParseAllTemplates(ResString("subjAnyTimeInvitationSummary"), App.ActiveUser, App.ActiveProject)
                        sResume = ParseTemplate(sResume, "%%count%%", Count.ToString)
                        sResume = ParseTemplate(sResume, "%%list%%", sSummary)
                        sResume = ParseTemplate(sResume, "%%errors%%", If(sErrors = "", "", "Errors:" + vbCrLf + sErrors))
                        Dim sError As String = ""
                        SendMail(WebOptions.SystemEmail, App.ActiveUser.UserEmail, sResumeSubject, sResume, sError, "", False, WebOptions.SMTPSSL)
                        App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfProject, App.ProjectID, "Send anytime invitations summary e-mail", sError)
                    End If
                    ' D6439 ==
                    App.Options.EvalSiteURL = sEvalURL      ' D4926
                    tRes.Data = Count
                    tRes.Message = sErrors
                    tRes.Result = If(Count > 0, ecActionResult.arSuccess, ecActionResult.arError)

                Case "getevalurl"
                    Dim isAnonymous As Boolean = CheckVar("isanonymous", False)
                    Dim AskFields As String = CheckVar("askfields", "")
                    Dim ReqFields As String = CheckVar("reqfields", "")
                    Dim GroupID As Integer = CheckVar("grpid", -1)
                    Dim RoleID As Integer = CheckVar("roleid", -1)
                    Dim JoinAsPM As Boolean = CheckVar("joinaspm", False)
                    Dim isResponsive As Boolean = CheckVar("responsive", False)
                    Dim DefViewerGrp As Integer = App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtViewer)
                    If isResponsiveOnly AndAlso isResponsive AndAlso RoleID = DefViewerGrp Then isResponsive = False    ' D6288
                    '"action=getevalurl&isanonymous=" + isAnonymous + "&askfields=" + askFields + "&reqfields=" + reqFields + "&grpid=" + groupID + "&roleid=" + roleGroupID + "&joinaspm=" + joinAsPM;
                    ' D6303 ==
                    Dim sRiskionBoth As String = ""
                    If App.isRiskEnabled Then
                        Dim HID As Integer = App.ActiveProject.ProjectManager.ActiveHierarchy
                        App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood
                        sRiskionBoth = String.Format(", '{0}'", GetEvaluationLink(isAnonymous, AskFields, ReqFields, GroupID, RoleID, JoinAsPM, isResponsive))
                        App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact
                        sRiskionBoth += String.Format(", '{0}'", GetEvaluationLink(isAnonymous, AskFields, ReqFields, GroupID, RoleID, JoinAsPM, isResponsive))
                        App.ActiveProject.ProjectManager.ActiveHierarchy = HID
                    End If
                    tRes.Data = "['" + GetEvaluationLink(isAnonymous, AskFields, ReqFields, GroupID, RoleID, JoinAsPM, isResponsive) + "','" + GetGroupLinks(isAnonymous, AskFields, ReqFields, RoleID, JoinAsPM, isResponsive) + "','" + GetUserLinks(isResponsive) + "'" + sRiskionBoth + "]"
                    ' D6303 ===
                    tRes.Tag = "getevalurl"
                    tRes.Result = ecActionResult.arSuccess
            End Select

            RawResponseStart()
            Response.ContentType = "text/plain"
            Response.Write(JsonConvert.SerializeObject(tRes))
            Response.End()
        End If

    End Sub

    Public Function GetSubject() As String
        Dim sValue As String = App.ActiveProject.PipeParameters.EvaluateInvitationSubject2
        If sValue = "" Then sValue = ResString("subjectAnyTimeInvitation")
        Return sValue
    End Function

    ' D4761 ===
    Public Function GetUserLinks(isResponsive As Boolean) As String
        Dim tResult As String = ""
        If CanSeeLinks Then ' D7420
            Dim sURL As String = ApplicationURL(isResponsive, False)
            For Each user As clsApplicationUser In ProjectUsers
                If CanShowUserLink(user) Then
                    Dim tUW As clsUserWorkgroup = clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(user.UserID, App.ActiveWorkgroup.ID, ProjectUserWorkgroups)
                    tResult += String.Format("{0}{1}{2}{3}", CreateLogonURL(user, App.ActiveProject, "pipe=yes", sURL, , tUW, True), vbTab, user.UserEmail, vbNewLine)
                End If
            Next
        End If
        Return JS_SafeString(tResult, True) ' D4797
    End Function

    ''' <summary>
    ''' Get the vealuation link depend on settings
    ''' </summary>
    ''' <param name="isAnonymous">true for Anonymous</param>
    ''' <param name="sAskFileds">[enpt]</param>
    ''' <param name="sRequiredFields">[enpt]</param>
    ''' <param name="tGroupID">GroupID inside the project (All Participants, etc)</param>
    ''' <param name="tWkgRoleGroupID">User permissions on attach (evaluator, PM, viewer, etc)</param>
    ''' <param name="JoinAsPM"></param>
    ''' <returns></returns>
    Public Function GetEvaluationLink(ByVal isAnonymous As Boolean, ByVal sAskFileds As String, ByVal sRequiredFields As String, tGroupID As Integer, tWkgRoleGroupID As Integer, JoinAsPM As Boolean, ByVal isResponsive As Boolean) As String
        ' D5078 ===
        If Not sAskFileds.Contains("e") Then sRequiredFields = sRequiredFields.Replace(CChar("e"), "")
        If Not sAskFileds.Contains("n") Then sRequiredFields = sRequiredFields.Replace(CChar("n"), "")
        If Not sAskFileds.Contains("t") Then sRequiredFields = sRequiredFields.Replace(CChar("t"), "")
        If Not sAskFileds.Contains("p") Then sRequiredFields = sRequiredFields.Replace(CChar("p"), "")
        If sRequiredFields <> "" AndAlso Not isAnonymous Then sRequiredFields = "req=" + sRequiredFields Else sRequiredFields = ""
        Dim DefGrp As clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup
        Dim sParams As String = sRequiredFields
        If tGroupID > 0 Then sParams += String.Format("&{0}={1}", _PARAM_ROLEGROUP, tGroupID)
        If tWkgRoleGroupID > 0 Then sParams += String.Format("&{0}={1}", _PARAM_WKG_ROLEGROUP, tWkgRoleGroupID)
        ' D5078 ==
        If Not isAnonymous AndAlso JoinAsPM Then sParams += String.Format("&{0}=1", _PARAM_AS_PM)
        Return String.Format("{0}{1}", ApplicationURL(isResponsive, False), CreateEvaluateSignupURL(App.ActiveProject, isAnonymous, sAskFileds, sParams.Replace("&&", "&"), _URL_ROOT))
    End Function

    ' D6732 ===
    Private Function GetLinkParams(ByVal isAnonymous As Boolean, ByVal sAskFileds As String, ByVal sRequiredFields As String, tWkgRoleGroupID As Integer, tGroupID As Integer, JoinAsPM As Boolean) As String
        Dim sRes As String = ""
        If isAnonymous Then
            sRes = ResString("lblSignupAnonymous")
        Else
            sRes = ResString("lblSignupOptions")
            Dim sFields As String = ""
            For Each sFld As Char In sAskFileds.ToLower
                Dim sName As String = ""
                Select Case sFld.ToString
                    Case "e"
                        sName = ResString("lblSignup_Email")
                    Case "n"
                        sName = ResString("lblSignup_Name")
                    Case "t"
                        sName = ResString("lblSignup_Phone")
                    Case "p"
                        sName = ResString("lblSignup_Password")
                End Select
                If sName <> "" AndAlso sRequiredFields.ToLower.Contains(sFld) Then sName += "*"
                If sName <> "" Then sFields += String.Format("{0}{1}", If(sFields = "", "", ", "), sName)
            Next
            If sFields <> "" Then sRes += String.Format("; {0}: {1}", "Sign-up form fields" + If(sFields.Contains("*"), " (* is required)", ""), sFields)
            If JoinAsPM Then sRes += String.Format("; {0}", "Assign as Project Manager")
        End If
        If tGroupID > 0 Then
            Dim G As clsGroup = App.ActiveProject.ProjectManager.CombinedGroups.GetGroupByID(tGroupID)
            If G IsNot Nothing Then
                sRes += String.Format("; {0} '{1}'", "Specify group", If(tGroupID = COMBINED_USER_ID, ResString("optNoGroup"), G.Name))
            End If
        End If
        If tWkgRoleGroupID > 0 Then
            Dim Wkg As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tWkgRoleGroupID)
            If Wkg IsNot Nothing Then sRes += String.Format("; {0}: '{1}'", "Assign the permissions", Wkg.Name)
        End If
        Return sRes
    End Function
    ' D6732 ==

    Public Function GetGroupLinks(ByVal isAnonymous As Boolean, ByVal sAskFileds As String, ByVal sRequiredFields As String, tWkgRoleGroupID As Integer, JoinAsPM As Boolean, isResponsive As Boolean) As String
        Dim Results As String = ""
        If CanSeeLinks Then ' D7420
            Dim DefGrp As clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup
            For Each tGrp As clsCombinedGroup In PM.CombinedGroups.GroupsList
                Dim isDef As Boolean = tGrp Is DefGrp
                If isDef OrElse String.IsNullOrWhiteSpace(tGrp.Rule) Then Results += String.Format("{2}{1}{0}\r\n", If(isDef, ResString("optNoGroup"), JS_SafeString(tGrp.Name)), vbTab, GetEvaluationLink(isAnonymous, sAskFileds, sRequiredFields, tGrp.ID, tWkgRoleGroupID, JoinAsPM, isResponsive))
            Next
            If Results <> "" Then Results = JS_SafeString(GetLinkParams(isAnonymous, sAskFileds, sRequiredFields, tWkgRoleGroupID, -9999, JoinAsPM)) + ":\r\n" + Results   ' D6732
        End If
        Return Results
    End Function

    Public Function GetUserRoles() As String
        Dim retVal As String = ""
        For Each tGrp As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
            'If tGrp.RoleLevel = ecRoleLevel.rlModelLevel AndAlso tGrp.ActionStatus(ecActionType.at_mlEvaluateModel) = ecActionStatus.asGranted Then ' D6291
            If tGrp.RoleLevel = ecRoleLevel.rlModelLevel Then ' D6291 + D6906
                retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{name:'{0}',id:{1}, can_eval:{2}}}", JS_SafeString(tGrp.Name), tGrp.ID, Bool2JS(tGrp.ActionStatus(ecActionType.at_mlEvaluateModel) = ecActionStatus.asGranted))    ' D6906
            End If
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetDefaultUserRolesID() As Integer
        Return App.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtEvaluator)  ' D5078
        'Dim retVal As String = ""
        'For Each tGrp As clsRoleGroup In App.ActiveWorkgroup.RoleGroups
        '    If tGrp.RoleLevel = ecRoleLevel.rlModelLevel Then
        '        Return tGrp.ID
        '    End If
        'Next
        'Return -1
    End Function

    Public Function GetGroupsList() As String
        Dim tResult As String = ""
        ' D6711 ===
        Dim DefGrp As clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            If String.IsNullOrEmpty(group.Rule) Then    ' D7094
                Dim isDef As Boolean = group Is DefGrp
                tResult += CStr(IIf(tResult <> "", ",", "")) + String.Format("{{name:'{0}',id:{1}}}", If(isDef, ResString("optNoGroup"), JS_SafeString(group.Name)), group.ID)
            End If
            ' D6711 ==
        Next
        Return tResult
    End Function

    ' D4965 ===
    Private Sub AnytimeInvitePage_Init(sender As Object, e As EventArgs) Handles Me.Init
        CanManageWorkgroup = App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup, App.ActiveWorkgroup)
    End Sub
    ' D4965 ==

End Class