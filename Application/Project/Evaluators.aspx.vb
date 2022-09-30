Imports System.Diagnostics

Partial Class EvaluatorsPage
    Inherits clsComparionCorePage

    Public Class clsUserEvaluationProgressData
        Public UserID As Integer = -1
        Public Name As String = ""
        Public Email As String = ""
        Public EvaluatedCount As Integer = 0
        Public TotalCount As Integer = 0
        Public LastJudgmentTime As Nullable(Of Date) = Nothing
        Public LastJudgmentTimeUTC As String = ""
        Public IsDisabled As Boolean = False
        Public IsOnline As Boolean = False
        Public CanEditProject As Boolean = False
        Public CanEvaluateProject As Boolean = False
        Public IsLinkEnabled As Boolean = True
        Public IsLinkGoEnabled As Boolean = True
        Public IsPreviewLinkEnabled As Boolean = True
    End Class

    Public OverallProgress As Integer = 0
    Private _EvalSiteURL As String = ""             ' D3836
    Public CanChange As Boolean = False
    Public sMessage As String = ""

    Public Sub New()
        MyBase.New(_PGID_PROJECT_EVAL_PROGRESS)
    End Sub

    ' D4948 ===
    Public Function GetMessage() As String
        If sMessage <> "" Then Return String.Format("<span class='note text'>{0}</span>", sMessage) Else Return ""
    End Function
    ' D4948 ==

    Public ReadOnly Property IsEvaluationProgressForTreatments As Boolean
        Get
            Return CurrentPageID = _PGID_CONTROLS_EVAL_PROGRESS
        End Get
    End Property

    Private ReadOnly Property IsTTRunning() As Boolean
        Get
            Return App.ActiveProject IsNot Nothing AndAlso App.ActiveUser IsNot Nothing AndAlso (App.ActiveProject.isTeamTimeLikelihood OrElse App.ActiveProject.isTeamTimeImpact) AndAlso (App.ActiveProject.MeetingOwnerID = App.ActiveUser.UserID)
        End Get
    End Property

    ' -D6363
    'Public Function CanSetProjectOnline() As Boolean
    '    If App.ActiveProject.isOnline Then Return True
    '    If App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.License.Parameters IsNot Nothing Then
    '        Dim Limit As Long = App.ActiveWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxProjectsOnline)
    '        If (Limit = -1 Or Limit > GetOnlineProjectsCount()) Then Return True
    '    End If
    '    Return False
    'End Function

    Public Function GetOnlineProjectsCount() As Integer
        Dim tOnlineProjects As Integer = 0
        If App.ActiveProjectsList IsNot Nothing Then
            For Each tPrj In App.ActiveProjectsList
                If tPrj.isOnline AndAlso tPrj.ProjectStatus = ecProjectStatus.psActive AndAlso Not tPrj.isMarkedAsDeleted Then tOnlineProjects += 1
            Next
        End If
        Return tOnlineProjects
    End Function

    Public Property EvaluationGroupID As Integer
        Get
            If PM.CombinedGroups.GroupsList.Count > 1 Then
                Dim tEvalGroupID As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_EVALUATION_PROGRESS_FILTER_BY_GROUP_ID, UNDEFINED_USER_ID))
                If PM.CombinedGroups.CombinedGroupExists(tEvalGroupID) Then Return tEvalGroupID
            End If
            Return COMBINED_USER_ID
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_EVALUATION_PROGRESS_FILTER_BY_GROUP_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Function GetEvaluationGroups() As String
        Dim sRes As String = ""
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            group.ApplyRules()
            sRes += String.Format("<option value='{0}' {2}>[{1}]</option>", group.CombinedUserID, JS_SafeString(group.Name), IIf(group.CombinedUserID = EvaluationGroupID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbGroups' class='select' style='min-width:130px;' onchange='sel_group_id = this.value * 1; sendCommand(""action=filter_by_group&id=""+this.value, true);'>{0}</select>", sRes)
    End Function

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        ' D3836 ===
        If Not App.isEvalURL_EvalSite Then ' D6269
            _EvalSiteURL = App.Options.EvalSiteURL
            App.Options.EvalSiteURL = ""
        End If
        ' D3836 ==
        Dim sPgid As String = CheckVar("pgid", "").ToLower
        Dim iPgid As Integer = _PGID_PROJECT_EVAL_PROGRESS
        If Integer.TryParse(sPgid, iPgid) Then CurrentPageID = iPgid
        ' D5091 ===
        If App.HasActiveProject Then
            CanChange = App.CanChangeProjectOnlineStatus(App.ActiveProject, sMessage) AndAlso Not IsTTRunning  ' D4948
            If sMessage <> "" Then sMessage = ParseString(sMessage) ' D4948
        End If
        ' D5091 ==
        If Not isCallback AndAlso Not IsPostBack Then
            If IsTTRunning() Then barWarning.Visible = True
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_PAGE_ACTION, "")).Trim().ToLower()  ' Anti-XSS
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender
        If Not App.isEvalURL_EvalSite Then App.Options.EvalSiteURL = _EvalSiteURL ' D3836 + D6269
    End Sub

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tResult As String = ""
        Dim tAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).ToLower()  ' Anti-XSS

        If Not String.IsNullOrEmpty(tAction) Then

            Select Case tAction

                Case "update", "filter_by_group"
                    Dim id As Integer = CInt(EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ID)).Trim())   ' Anti-XSS
                    If EvaluationGroupID <> id Then EvaluationGroupID = id
                    tResult = String.Format("[{0},'{1}','&nbsp;{2}&#37;',{3},'{4}','{5}']", GetEvaluationProgressData(), JS_SafeString(GetOverallGraphBar()), OverallProgress, Bool2JS(IsTTRunning), String.Format(ResString("lblListOfEvaluators"), PM.CombinedGroups.GetCombinedGroupByUserID(EvaluationGroupID).UsersList.Count), tAction)

                Case "copy", "open", "view" ' D6512
                    Dim sCommand As String = "proceed"
                    Dim sWarning As String = ""
                    Dim isOpen As Boolean = tAction.ToLower <> "copy" ' D6364 + D6512

                    Dim sEmail As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_EMAIL)).Trim()   ' Anti-XSS
                    Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)

                    If tUser IsNot Nothing Then
                        ' D6312 ===
                        Dim isEvalSite As Boolean = False
                        If App.Options.EvalSiteURL <> "" AndAlso App.isEvalURL_EvalSite Then
                            Dim tmpUW As clsUserWorkgroup = App.DBUserWorkgroupByUserIDWorkgroupID(tUser.UserID, App.ActiveWorkgroup.ID)
                            Dim tmpWS As clsWorkspace = App.DBWorkspaceByUserIDProjectID(tUser.UserID, App.ProjectID)
                            'isEvalSite = Not App.CanUserModifyProject(tUser.UserID, App.ProjectID, tmpUW, tmpWS, App.ActiveWorkgroup) AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, App.ProjectID, tmpUW, App.ActiveWorkgroup)
                            isEvalSite = Not tUser.CannotBeDeleted AndAlso App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tUser.UserID, App.ProjectID, tmpUW, App.ActiveWorkgroup) ' D6318
                        End If
                        Dim _sEvalURL As String = App.Options.EvalSiteURL
                        If App.Options.EvalSiteURL <> "" AndAlso Not isEvalSite Then
                            App.Options.EvalSiteURL = ""
                        End If

                        If App.Options.EvalSiteURL <> "" AndAlso isOpen Then PrepareProjectForOpenPipe(App.ActiveProject)   ' D6363 + D6364 + D6512

                        If Not isOpen AndAlso (Not App.ActiveProject.isOnline OrElse Not App.ActiveProject.isPublic) Then   ' D6364
                            Dim tPrj = App.ActiveProject
                            If tPrj.ProjectStatus = ecProjectStatus.psActive OrElse Not App.CanChangeProjectOnlineStatus(App.ActiveProject, sWarning) Then  ' D6363
                                If Not tPrj.isMarkedAsDeleted AndAlso Not tPrj.isTeamTimeImpact AndAlso Not tPrj.isTeamTimeLikelihood AndAlso
                               App.CanUserModifyProject(App.ActiveUser.UserID, tPrj.ID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup) Then    ' D6363
                                    sCommand = "confirm"
                                    sWarning = JS_SafeString(ResString("msgSetProjectStatusOnline"))
                                Else
                                    sCommand = "error"
                                    sWarning = JS_SafeString(ResString("msgCantMakeProjectOnline"))
                                End If
                            Else
                                sCommand = "error"
                                sWarning = JS_SafeString(ResString("msgCantMakeReadOnlyProjectOnline"))
                            End If
                        End If

                        If isOpen Then App.Options.EvalURLPostfix = "&ignoreoffline=yes&ignorepsw=yes"  ' D6364
                        Dim sTPL As String = CStr(IIf(IsEvaluationProgressForTreatments, _TEMPL_URL_EVALUATE_CONTROLS, IIf(App.isRiskEnabled, IIf(App.ActiveProject.isImpact, _TEMPL_URL_EVALUATE_IMPACT, _TEMPL_URL_EVALUATE_LIKELIHOOD), _TEMPL_URL_EVALUATE)))   ' D4281
                        tResult = String.Format("['{0}','{1}','{2}{4}',{3}]", sCommand, sWarning, JS_SafeString(ParseAllTemplates(sTPL, tUser, App.ActiveProject)), Bool2JS(isEvalSite), If(tAction.ToLower = "view" AndAlso Not App.isRiskEnabled, String.Format("&{0}=yes", _PARAM_READONLY), ""))   ' D4281 + D6518
                        If isOpen Then App.Options.EvalURLPostfix = ""  ' D6364
                        If _sEvalURL <> "" AndAlso Not isEvalSite Then
                            App.Options.EvalSiteURL = _sEvalURL
                        End If
                    End If
                    ' D6312 ==

                Case "make_online"
                    App.ActiveProject.isOnline = True
                    If Not App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False) Then App.ActiveProject.isOnline = False

                    If App.ActiveProject.isOnline Then
                        App.DBProjectUpdate(App.ActiveProject, False, Trim("Update Project info. Online (Eval Progress)"))
                    End If

                    tResult = Bool2JS(App.ActiveProject.isOnline)

                    ' D5091 ===
                Case "set_status"
                    tResult = "['error']"
                    Dim tOnline As Boolean = CheckVar("online", App.ActiveProject.isOnline)
                    ' D4743 ===
                    If App.ActiveProject.isOnline <> tOnline AndAlso CanChange Then ' D4948
                        App.ActiveProject.isOnline = tOnline
                        App.DBProjectUpdate(App.ActiveProject, False, String.Format("Set project as {0}", If(tOnline, "on-line", "off-line")))  ' D4948
                        tResult = String.Format("['OK', {0}, '{1}']", If(tOnline, "true", "false"), JS_SafeString(sMessage))    ' D4948
                    End If
                    ' D5091 ==

                    ' D6334 ===
                Case "stop_tt"
                    If IsTTRunning Then
                        App.TeamTimeEndSession(App.ActiveProject, False)
                    End If
                    tResult = Bool2JS(IsTTRunning())
                    ' D6334 ==

            End Select

        End If

        If Not App.isEvalURL_EvalSite Then App.Options.EvalSiteURL = _EvalSiteURL ' D4592 + D6269

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Function GetEvaluationProgress() As List(Of clsUserEvaluationProgressData)
        ' from CoreWS_OperationContracts.vb:DoGetUsersEvaluationProgress()
        Dim res As List(Of clsUserEvaluationProgressData) = New List(Of clsUserEvaluationProgressData)

        Dim group As clsCombinedGroup = PM.CombinedGroups.GetGroupByCombinedID(EvaluationGroupID)

        Debug.WriteLine(Now.ToLongTimeString + " Start getting eval progress for " + group.Name)
        Dim startTime As DateTime = Now

        Dim tAllUsers As Dictionary(Of String, clsApplicationUser) = App.DBUsersByProjectIDDictionary(App.ProjectID)
        Dim tWSList As List(Of clsWorkspace) = App.DBWorkspacesByProjectID(App.ProjectID)
        Dim tUWList As List(Of clsUserWorkgroup) = App.DBUserWorkgroupsByProjectIDWorkgroupID(App.ProjectID, App.ActiveWorkgroup.ID)    ' D4965

        Dim fCanManageWkg As Boolean = App.CanUserDoAction(ecActionType.at_alManageWorkgroupUsers, App.ActiveUserWorkgroup, App.ActiveWorkgroup)    ' D4965

        Dim madeAllCount As Integer
        Dim totalAllCount As Integer
        Dim evalProgress As New Dictionary(Of String, UserEvaluationProgressData)
        If Not IsEvaluationProgressForTreatments Then
            evalProgress = PM.StorageManager.Reader.GetEvaluationProgress(group.UsersList, PM.ActiveHierarchy, madeAllCount, totalAllCount)
        End If

        For Each user As clsUser In group.UsersList
            If tAllUsers.ContainsKey(user.UserEMail.ToLower) Then   ' D6496
                Dim tAppUser As clsApplicationUser = tAllUsers(user.UserEMail.ToLower)  ' D6496
                If tAppUser IsNot Nothing Then
                    Dim email As String = tAppUser.UserEmail
                    Dim userName As String = If(user.UserName = "", tAppUser.UserName, user.UserName)   ' D6496

                    Dim progress As New clsUserEvaluationProgressData With {
                            .UserID = tAppUser.UserID,
                            .Email = email,
                            .Name = If(String.IsNullOrEmpty(userName), email, userName),
                            .IsOnline = tAppUser.isOnline
                    }

                    Dim tWS As clsWorkspace = clsWorkspace.WorkspaceByUserIDAndProjectID(tAppUser.UserID, App.ProjectID, tWSList)
                    If tWS IsNot Nothing Then
                        Dim tGroup As clsRoleGroup = App.ActiveWorkgroup.RoleGroup(tWS.GroupID, App.ActiveWorkgroup.RoleGroups)
                        If tGroup IsNot Nothing Then
                            progress.CanEditProject = tGroup.ActionStatus(ecActionType.at_mlManageProjectOptions) = ecActionStatus.asGranted OrElse tGroup.ActionStatus(ecActionType.at_mlModifyModelHierarchy) = ecActionStatus.asGranted
                            progress.CanEvaluateProject = App.CanUserDoProjectAction(ecActionType.at_mlEvaluateModel, tAppUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkgroup)
                        End If

                        progress.IsDisabled = tWS.Status(PM.ActiveHierarchy = ECHierarchyID.hidImpact) = ecWorkspaceStatus.wsDisabled

                        Dim isAdmin As Boolean = tAppUser.CannotBeDeleted() OrElse App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, tAppUser.UserID)    ' D4921
                        ' User Link Enabled/Disabled
                        If isAdmin Then progress.IsLinkEnabled = False  ' D4921
                        'If progress.IsLinkEnabled AndAlso Not App.CanUserModifyProject(tAppUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, tWS) Then progress.IsLinkEnabled = False   ' -D4392

                        ' User Open Link Enabled/Disabled
                        If isAdmin Then progress.IsLinkGoEnabled = False    ' D4921
                        'If progress.IsLinkGoEnabled AndAlso Not App.CanUserModifyProject(tAppUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, tWS, App.ActiveWorkgroup) Then progress.IsLinkGoEnabled = False  ' -D4392
                        If progress.IsLinkGoEnabled AndAlso App.ActiveProject IsNot Nothing AndAlso (App.ActiveProject.isTeamTimeImpact OrElse App.ActiveProject.isTeamTimeLikelihood) AndAlso App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.UserID = App.ActiveProject.MeetingOwnerID Then progress.IsLinkGoEnabled = False
                        If progress.IsLinkGoEnabled AndAlso progress.IsDisabled Then progress.IsLinkGoEnabled = False
                        ' D4965 ===
                        If progress.IsLinkGoEnabled AndAlso Not fCanManageWkg AndAlso App.ActiveUser.UserID <> tAppUser.UserID AndAlso App.CanUserModifyProject(tAppUser.UserID, App.ProjectID, clsUserWorkgroup.UserWorkgroupByUserIDAndWorkgroupID(tAppUser.UserID, App.ActiveWorkgroup.ID, tUWList), tWS, App.ActiveWorkgroup) Then
                            progress.IsLinkGoEnabled = False
                        End If
                        If Not progress.IsLinkGoEnabled Then progress.IsLinkEnabled = False
                        '  D4965 ==

                        ' User Preview Enabled/Disabled
                        'If Not progress.CanEvaluateProject OrElse (isAdmin AndAlso App.isEvalURL_EvalSite) Then progress.IsPreviewLinkEnabled = False   ' D6518
                        If Not progress.CanEvaluateProject Then progress.IsPreviewLinkEnabled = False   ' D6518 + D6700
                        If progress.IsPreviewLinkEnabled AndAlso (App.ActiveProject.isTeamTimeImpact OrElse App.ActiveProject.isTeamTimeLikelihood) Then progress.IsPreviewLinkEnabled = False
                    End If

                    Dim userLastJudgmentTime As DateTime = VERY_OLD_DATE

                    Dim madeCount As Integer
                    Dim totalCount As Integer
                    If IsEvaluationProgressForTreatments Then
                        ' Evaluation progress for Riskion Treatments
                        PM.PipeBuilder.GetControlsEvaluationProgress(user.UserID, madeCount, totalCount, userLastJudgmentTime)
                    Else
                        ' Evaluation progress for Anytime
                        If evalProgress.ContainsKey(user.UserEMail.ToLower) Then
                            With evalProgress(user.UserEMail.ToLower)
                                madeCount = .EvaluatedCount
                                totalCount = .TotalCount
                                If .LastJudgmentTime.HasValue Then
                                    userLastJudgmentTime = .LastJudgmentTime.Value
                                End If
                            End With
                        End If
                    End If

                    If madeCount > totalCount Then madeCount = totalCount

                    progress.EvaluatedCount = madeCount
                    progress.TotalCount = totalCount

                    If madeCount > 0 Then
                        progress.LastJudgmentTime = userLastJudgmentTime
                        progress.LastJudgmentTimeUTC = userLastJudgmentTime.ToString()
                    Else
                        progress.LastJudgmentTime = Nothing
                        progress.LastJudgmentTimeUTC = ""
                    End If

                    res.Add(progress)
                End If
            End If
        Next
        Return res
    End Function

    Public Function GetEvaluationProgressData() As String
        Dim retVal As String = ""
        Dim data As List(Of clsUserEvaluationProgressData) = GetEvaluationProgress() '.OrderByDescending(Function(p) p.EvaluatedCount / p.TotalCount).ToList        
        Dim Total As Integer = 0
        Dim Made As Integer = 0

        For Each item As clsUserEvaluationProgressData In data
            Dim fPrc As Single = 0
            Dim sPrc As String = 0.ToString("F1")

            If item.TotalCount > 0 AndAlso item.EvaluatedCount > 0 Then
                fPrc = CSng(Math.Round(item.EvaluatedCount * 100 / item.TotalCount, 1))
                sPrc = fPrc.ToString("F1")
            End If

            If item.TotalCount > 0 Then
                Total += item.TotalCount                
                Made += item.EvaluatedCount
            End If

            Dim sProg As String = JS_SafeString(String.Format("<nobr><div style='display:inline-block; vertical-align:middle;'>{0}</div><span style='width:3em; text-align:right; display:inline-block;'>{1}&#37;</span><span style='margin-left:5px; display:inline-block;'>({2}/{3})</span></nobr>", HTMLCreateGraphBar(fPrc, 100, 150, 9, "graph_progress", ThemePath + _FILE_IMAGE_BLANK, sPrc + "&#37"), sPrc, item.EvaluatedCount, item.TotalCount))
            Dim sProgPlain As String = JS_SafeString(String.Format("{0}% ({1}/{2})", sPrc, item.EvaluatedCount, item.TotalCount))
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{{""id"":{0},""name"":'{1}',""email"":'{2}',""progress"":'{3}',""progress_plain"":'{4}',""percent_eval"":{5},""evaluated"":{6},""total"":{7},""last_judg"":'{8}',""actions"":'{9}',""is_online"":{10},""is_disabled"":{11},""can_edit"":{12},""can_eval"":{13},""link_enabled"":{14},""linkgo_enabled"":{15},""preview_enabled"":{16}}}", item.UserID, JS_SafeString(item.Name), JS_SafeString(item.Email), sProg, sProgPlain, JS_SafeNumber(fPrc), JS_SafeNumber(item.EvaluatedCount), JS_SafeNumber(item.TotalCount), JS_SafeString(item.LastJudgmentTimeUTC), "", Bool2JS(item.IsOnline), Bool2JS(item.IsDisabled), Bool2JS(item.CanEditProject), Bool2JS(item.CanEvaluateProject), Bool2JS(item.IsLinkEnabled), Bool2JS(item.IsLinkGoEnabled), Bool2JS(item.IsPreviewLinkEnabled))
        Next

        If Total = 0 Then OverallProgress = 0 Else OverallProgress = CInt(100 * Made / Total)
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetOverallGraphBar() As String
        Return HTMLCreateGraphBar(OverallProgress, 100, 300, 12, "graph_global_progress", ThemePath + _FILE_IMAGE_BLANK, CStr(OverallProgress) + "&#37;")
    End Function

End Class