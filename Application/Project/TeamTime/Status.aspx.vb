Partial Class ProjectTeamTimeStatusPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_TEAMTIME_STATUS)
    End Sub

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return PRJ.ProjectManager
        End Get
    End Property

    Public Function isTeamTimeActive() As Boolean
        Return (PM.ActiveHierarchy = ECHierarchyID.hidImpact AndAlso PRJ.isTeamTimeImpact) OrElse (PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso PRJ.isTeamTimeLikelihood)   'A0221 + D1951 + D1953 + D4030
    End Function

    Public Function isTeamTimeAvailable() As Boolean
        'Return App.isTeamTimeAvailable AndAlso PRJ.ProjectStatus = ecProjectStatus.psActive AndAlso CanEditActiveProject
        Return App.isTeamTimeAvailable AndAlso PRJ.ProjectStatus = ecProjectStatus.psActive ' D4310 for allow to stop TT session from another PM
    End Function

    ' D4030 ===
    Public Function isTeamTimeOwner() As Boolean
        Return PRJ.MeetingOwner IsNot Nothing AndAlso App.ActiveUser.UserID = PRJ.MeetingOwnerID
    End Function
    ' D4030 ==

    Public Function isObjectivesEvaluateWarning() As Boolean
        Return Not PM.PipeParameters.EvaluateObjectives ' D4030
    End Function

    Public Function isAlternativesEvaluateWarning() As Boolean
        Return Not PM.PipeParameters.EvaluateAlternatives   ' D4030
    End Function

#Region "Page events"

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack AndAlso Not isCallback Then
            AlignHorizontalCenter = True
            AlignVerticalCenter = True
            'pnlLoadingPanel.Caption = ResString("msgLoading")
            'pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")

            brdOptionsWarning.Visible = isObjectivesEvaluateWarning() OrElse isAlternativesEvaluateWarning()

            Dim fChk As Integer = CheckUserRolesAndEnabledItems(PRJ, App.ActiveUser.UserEmail)
            If fChk <> 3 Then
                ' check when only evaluation is enabled
                If (Not isObjectivesEvaluateWarning() AndAlso (fChk And 1) = 0) OrElse (Not isAlternativesEvaluateWarning() AndAlso (fChk And 2) = 0) Then
                    brdRolesWarning.Visible = True
                    brdRolesWarningTag.InnerText = fChk.ToString
                End If
            End If

            If App.isRiskEnabled AndAlso isTeamTimeActive() AndAlso (PRJ.isTeamTimeImpact <> PRJ.isImpact) Then    ' D4030
                lblMessage.InnerText = CStr(IIf(PRJ.isTeamTimeImpact, ResString("msgTeamTimeImpactIsActive"), ResString("msgTeamTimeLikelihoodIsActive")))
                lblMessage.Visible = True
                'DoAskOtherTT = True
            Else
                lblMessage.Visible = False
            End If

            ' D4030 ===
            If Not isTeamTimeAvailable() AndAlso Not lblMessage.Visible Then
                lblMessage.InnerText = ParseAllTemplates(ResString("msgTTCantBeStarted"), App.ActiveUser, PRJ, , True)
                lblMessage.Visible = True
            End If

            If isTeamTimeActive() AndAlso Not isTeamTimeOwner() AndAlso Not lblMessage.Visible Then
                Dim sPM As String = App.ResString("lblAnotherPM")
                If PRJ.MeetingOwner IsNot Nothing Then sPM = CStr(IIf(PRJ.MeetingOwner.UserName = "", PRJ.MeetingOwner.UserEmail, PRJ.MeetingOwner.UserName))
                lblMessage.InnerText = ParseAllTemplates(String.Format(ResString("msgTTAlreadyStarted"), sPM), App.ActiveUser, PRJ, , True)
                lblMessage.Visible = True
            End If
            ' D4030 ==

        End If
        If isAjax() Then Ajax_Callback(Request.Form.ToString) ' D4030
    End Sub

#End Region

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim sResult As String = ""
        Dim sAction As String = GetParam(args, _PARAM_ACTION).ToLower()
        Dim tRes As New jActionResult    ' D4752

        Select Case sAction

            Case "set_param"
                Dim sParamID As String = GetParam(args, "id")
                Dim ParamID As Integer = -1
                If Integer.TryParse(sParamID, ParamID) Then
                    Dim Chk As Boolean = Param2Bool(args, "chk")
                    Dim sParamName As String = ""
                    With PRJ.PipeParameters
                        Select Case ParamID
                            Case 0
                                .TeamTimeDisplayUsersWithViewOnlyAccess = Chk
                                sParamName = ResString("lblTTDisplayViewOnlyUsers")
                            Case 1
                                .SynchStartInPollingMode = Chk
                                sParamName = ResString("lblTTStartPollingMode")
                            Case 2
                                .TeamTimeStartInAnonymousMode = Chk
                                sParamName = ResString("lblTTAnonymous")
                            Case 3
                                .TeamTimeHideProjectOwner = Chk
                                sParamName = ResString("lblTTAnonymous")
                            Case 4
                                PM.Parameters.TTHideOfflineUsers = Chk
                                sParamName = ResString("lblTTHideOfflineUsers")
                        End Select
                    End With
                    PRJ.SaveProjectOptions("Update TeamTime setting", , , String.Format("{0}: {1}", sParamName, Chk))
                    tRes.Result = ecActionResult.arSuccess  ' D4752
                End If

            Case "check_all"
                Dim Chk As Boolean = Param2Bool(args, "chk")
                With PRJ.PipeParameters
                    .TeamTimeDisplayUsersWithViewOnlyAccess = Chk
                    .SynchStartInPollingMode = Chk
                    .TeamTimeStartInAnonymousMode = Chk
                    .TeamTimeHideProjectOwner = Chk
                    PM.Parameters.TTHideOfflineUsers = Chk
                End With
                PRJ.SaveProjectOptions("Toggle all TeamTime settings", , , String.Format("Checked: {0}", Chk))
                tRes.Result = ecActionResult.arSuccess  ' D4752

                ' D4030 ===
            Case "start_tt"
                If Not isTeamTimeActive() AndAlso isTeamTimeAvailable() Then
                    Dim tUsersList As New List(Of clsApplicationUser)
                    Dim tWSList As New List(Of clsWorkspace)
                    Dim PrjUsers As List(Of clsApplicationUser) = App.DBUsersByProjectID(App.ProjectID)
                    Dim WSProject As List(Of clsWorkspace) = App.DBWorkspacesByProjectID(App.ProjectID)
                    For Each tWS As clsWorkspace In WSProject
                        Dim tUser As clsApplicationUser = clsApplicationUser.UserByUserID(tWS.UserID, PrjUsers)
                        If tWS.Status(PRJ.isImpact) <> ecWorkspaceStatus.wsDisabled AndAlso tUser IsNot Nothing Then
                            Dim tPrjUser As clsUser = PM.GetUserByEMail(tUser.UserEmail)
                            If tPrjUser IsNot Nothing Then
                                If (tUser.SyncEvaluationMode <> tPrjUser.SyncEvaluationMode OrElse tUser.VotingBoxID <> tPrjUser.VotingBoxID) Then
                                    tUser.VotingBoxID = tPrjUser.VotingBoxID
                                    tUser.SyncEvaluationMode = tPrjUser.SyncEvaluationMode
                                End If
                                Dim tStatus As ecWorkspaceStatus = tWS.TeamTimeStatus(PRJ.isImpact)
                                Select Case tUser.SyncEvaluationMode
                                    Case SynchronousEvaluationMode.semByFacilitatorOnly
                                        tStatus = ecWorkspaceStatus.wsSynhronousReadOnly
                                    Case SynchronousEvaluationMode.semOnline, SynchronousEvaluationMode.semVotingBox
                                        tStatus = ecWorkspaceStatus.wsSynhronousActive
                                    Case SynchronousEvaluationMode.semNone
                                        tStatus = ecWorkspaceStatus.wsUnknown
                                End Select
                                If tWS.TeamTimeStatus(PRJ.isImpact) <> tStatus Then
                                    tWS.TeamTimeStatus(PRJ.isImpact) = tStatus
                                    'App.DBWorkspaceUpdate(tWS, False, "Update User TeamTime status")
                                End If
                            End If
                            If tWS.isInTeamTime(PRJ.isImpact) OrElse tWS.UserID = App.ActiveUser.UserID Then
                                tUsersList.Add(tUser)
                                tWSList.Add(tWS)
                            End If
                        End If
                    Next
                    If App.TeamTimeStartSession(App.ActiveUser, PRJ, CType(PM.ActiveHierarchy, ECHierarchyID), tUsersList, tWSList, True, ecAppliationID.appComparion) Then   ' D4274
                        If isTeamTimeActive() Then tRes.Result = ecActionResult.arSuccess   ' D4752
                    End If
                End If

            Case "stop_tt"
                If isTeamTimeActive() AndAlso isTeamTimeAvailable() Then
                    App.TeamTimeEndSession(PRJ, False)
                    If Not isTeamTimeActive() Then tRes.Result = ecActionResult.arSuccess  ' D4752
                End If

            Case "status"
                If PRJ IsNot Nothing Then   ' D4316
                    With PRJ.PipeParameters
                        tRes.Data = String.Format("[{0},{1},{2},{3},{4},{5},{6}]", Bool2Num(isTeamTimeActive), Bool2Num(isTeamTimeOwner), Bool2Num(.TeamTimeDisplayUsersWithViewOnlyAccess), Bool2Num(.SynchStartInPollingMode), Bool2Num(.TeamTimeStartInAnonymousMode), Bool2Num(.TeamTimeHideProjectOwner), Bool2Num(PM.Parameters.TTHideOfflineUsers))  ' D4752
                        tRes.Result = ecActionResult.arSuccess
                    End With
                End If
                ' D4030 ==

        End Select

        sResult = JsonConvert.SerializeObject(tRes) ' D4752

        'sResult = String.Format("['{0}'{1}{2}]", JS_SafeString(sAction), IIf(sResult = "", "", ","), sResult)

        Response.Clear()
        Response.ContentType = "text/plain"
        Response.Write(sResult)
        RawResponseEnd()

    End Sub

End Class