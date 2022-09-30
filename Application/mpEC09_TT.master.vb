' D0535
Imports SpyronControls.Spyron.Core

Partial Public Class clsComparionCoreEC09TTMasterPage
    Inherits MasterPage

    Public THEME_DEFAULT As String = "Default"

    Private Const MaxTitleLen As Integer = 60               ' D0071
    Private Const _ShowDraftMessage As Boolean = True       ' D0257

    Private Const sDraftMessage As String = "This is draft version. <nobr>Please, don't create extra cases in FogBugz for this page.</nobr>"    ' D0578

    Public ReadOnly Property _Page() As clsComparionCorePage
        Get
            Return CType(Page, clsComparionCorePage)
        End Get
    End Property

    ' D0144 ===
    Public ReadOnly Property HelpPage() As String
        Get
            Dim sHelpID As String = String.Format("help_{0}", _Page.CurrentPageID)
            Dim sHelp As String = _Page.ResString(sHelpID, True)
            If sHelp = sHelpID Then sHelp = ""
            Return sHelp
        End Get
    End Property
    ' D0144 ==

    ' D0309 ===
    Public Property WarningText() As String
        Get
            Return lblLockedProject.InnerHtml   ' D0578
        End Get
        Set(value As String)
            lblLockedProject.InnerHtml = value  ' D0578
            lblLockedProject.Visible = lblLockedProject.InnerHtml <> ""  ' D0578
        End Set
    End Property
    ' D0309 ==

    ' D0005 ===
    Protected Sub Page_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender

        If HideMasterHeader Then
            FindControl("master_header").Visible = False
        End If

        If SessionTimeout > 100 Then

        End If
        ' D0017 ===
        If Not Page.IsPostBack Then
            ' D0125 ===
            Dim sLang As String = EcSanitizer.GetSafeHtmlFragment(_Page.CheckVar("language", "")) ' Anti-XSS
            If sLang <> "" Then
                _Page.SetLanguage(sLang)
                _Page.App.ResetLanguages()  ' D0150
                Dim sURL As String = _Page.PageURL(_Page.CurrentPageID)
                If Not Request.UrlReferrer Is Nothing Then sURL = Request.UrlReferrer.AbsoluteUri
                If sURL = "" Then sURL = _URL_ROOT
                Response.Redirect(sURL, True)
            End If
            ' D0125 ==

            '' D0103 ===
            'Dim FavIconLink As New HtmlLink()
            'FavIconLink.Href = _URL_ROOT + "favicon.ico"
            'FavIconLink.Attributes.Add("rel", "shortcut icon")
            'Page.Header.Controls.Add(FavIconLink)
            'FavIconLink = New HtmlLink()
            'FavIconLink.Href = _URL_ROOT + "favicon.ico"
            'FavIconLink.Attributes.Add("rel", "icon")
            'Page.Header.Controls.Add(FavIconLink)
            '' D0103 ==

        End If

        ' D0257 ===
        MainModal.ProgressBlankImage = _Page.BlankImage ' D0355
        If _ShowDraftMessage And ShowDraftPages Then ' D0315
            If _Page.CurrentPageID <> _PGID_SILVERLIGHT_UI AndAlso _Page.isDraftPage(_Page.CurrentPageID) Then WarningText = sDraftMessage ' D0459 + D0695
        End If
        ' D0257 ==

        Page.Title = _Page.GetPageTitle ' D0135

        Dim fShowRoundCorners As Boolean = (_Page.CurrentPageID <> _PGID_SILVERLIGHT_UI)    ' D0680

        If fShowRoundCorners Then tdMainArea.Attributes.Add("style", String.Format("padding:{0}px 20px 12px 20px", 8)) ' D0680
        tdRoundTop.Visible = fShowRoundCorners ' D0680
        ' D0680 ===
        tdRoundBottom.Visible = fShowRoundCorners
        tdRoundLeft.Visible = fShowRoundCorners
        tdRoundRight.Visible = fShowRoundCorners
        ' D0680 ==

        If Not fShowRoundCorners Or Not _Page.AlignHorizontalCenter Or Not _Page.AlignVerticalCenter Then tableRound.Attributes.Add("style", "width:100%;height:100%") ' D0680

        ' D0061 ===
        tdMainContent.Attributes.Add("align", CStr(IIf(_Page.AlignHorizontalCenter, "center", "left")))   ' D0112
        tdMainContent.Attributes.Add("valign", CStr(IIf(_Page.AlignVerticalCenter, "middle", "top")))     ' D0112
        ' D0061 ==

        ' -D1359
        'Dim ctrls As ControlCollection = phIconsInRound.Controls

        'Dim imgHelp As New Image
        'imgHelp.SkinID = "HelpIconRound"
        'imgHelp.Style.Add("cursor", "help")

        'Dim imgVideoHelp As New Image
        'imgVideoHelp.SkinID = "HelpIconVideo"
        'imgVideoHelp.Style.Add("cursor", "help")
        'imgVideoHelp.Visible = False

        '' D0427 ===
        'If Not _Page.HasPermission(_PGID_HELP, Nothing) Then
        '    imgHelp.Visible = False
        'Else
        '    ' D0427 ==
        '    ' D0104 ===
        '    imgHelp.Attributes.Add("style", String.Format("position:absolute; margin-top:{0}px; margin-left:6px; cursor:help;", 8))
        '    tooltipHelp.Title = _Page.GetPageTitle  ' D0109
        '    tooltipHelp.Text = ""

        '    Dim sTooltipID As String = String.Format("tooltip_{0}", _Page.CurrentPageID)
        '    Dim sTooltip As String = _Page.ResString(sTooltipID, True)
        '    If sTooltip <> sTooltipID Then tooltipHelp.Text = sTooltip
        '    ' D0104 ==

        '    ' D0144 ===
        '    Dim sHelp As String = HelpPage
        '    If sHelp <> "" Then
        '        If tooltipHelp.Text <> "" Then
        '            tooltipHelp.Text += String.Format("<p><a href='#' onclick='return OpenHelp(""{0}"")' class='action'>{1}</a></p>", sHelp, _Page.ResString("lblMoreHelp"))
        '        Else
        '            tooltipHelp.Text = String.Format("<iframe width='700' height='450' src='{0}' border='0' frameborder='0' style='border:0px'></iframe>", _Page.HelpURL + sHelp)
        '        End If
        '    End If
        '    If tooltipHelp.Text = "" Then
        '        imgHelp.Visible = False ' D0320
        '        tooltipHelp.Text = String.Format("No help #{0} found for this page", _Page.CurrentPageID) ' D0109 + D0114
        '    End If
        '    ' D0144 ==
        '    ' D0427 ===
        'End If

        'If Not _Page.HasPermission(_PGID_VIDEO_HELP, Nothing) Then
        '    imgVideoHelp.Visible = False
        'Else
        '    ' D0427 ==
        '    ' D0340 ===
        '    Dim sHelpID As String = String.Format("video_{0}", _Page.CurrentPageID)
        '    Dim sHelpLink As String = _Page.ResString(sHelpID, True)
        '    Dim fHasVideoHelp As Boolean = sHelpLink <> sHelpID
        '    imgVideoHelp.Visible = fHasVideoHelp
        '    If fHasVideoHelp Then
        '        imgVideoHelp.Attributes.Add("style", String.Format("position:absolute; margin-top:{0}px; margin-left:{1}px; cursor:hand", 8, 26))
        '        imgVideoHelp.Attributes.Add("onclick", String.Format("return OpenVideoHelpItem('{0}')", JS_SafeString(sHelpLink)))
        '        imgVideoHelp.ToolTip = _Page.PageTitle(_PGID_VIDEO_HELP)
        '    End If
        '    ' D0340 ===
        '    ' D0427 ===
        'End If  ' D0427 ==

        'ctrls.Add(imgHelp)
        'ctrls.Add(imgVideoHelp)

        'tooltipHelp.TargetControlID = imgHelp.ClientID
        'tooltipHelp.IsClientID = True
        'tooltipHelp.Visible = imgHelp.Visible

        Dim sLockedMsg As String = ""

        '' D0052 ===
        Dim PrjName As String = ""
        If _Page.App.HasActiveProject AndAlso Not HasListPage(_PAGESLIST_SPYRON, _Page.CurrentPageID) Then
            ' D0071 ===
            Dim tPrj As clsProject = _Page.App.ActiveProject
            '            If _Page.App.CanUserDoProjectAction(ecActionType.at_mlManageProjectOptions, _Page.App.ActiveUser.UserID, tPrj.ID, _Page.App.ActiveUserWorkgroup) Then PrjName = String.Format(" [{0}]", tPrj.Passcode) ' D0071 + D0471 - D0867

            If PrjName.Length < MaxTitleLen - 5 Then PrjName = ShortString(tPrj.ProjectName, MaxTitleLen - PrjName.Length) + PrjName ' D0070 + D0071
            PrjName = PrjName.Trim
            Dim fShowProjectTitle As Boolean = False
            ' D0071 ==
            Dim pg As clsPageAction = _Page.PageByID(_Page.CurrentPageID)
            If Not pg Is Nothing Then
                ' D0134 ===
                'If pg.Permission = ecPagePermission.ppProjectNotEmptyWithLock OrElse pg.Permission = ecPagePermission.ppProjectWithLock OrElse pg.Permission = ecPagePermission.ppProjectWithStructuringLock OrElse pg.ID = _PGID_EVALUATION OrElse pg.ID = _PGID_EVALUATION OrElse pg.ID = _PGID_EVALUATE_READONLY OrElse pg.ID = _PGID_TEAMTIME OrElse pg.ID = _PGID_TEAMTIME_CV Then    ' D0194 + D0511 + D0589 + D1243 + D3563
                If pg.Permission = ecPagePermission.ppProjectWithLock OrElse pg.ID = _PGID_EVALUATION OrElse pg.ID = _PGID_EVALUATION OrElse pg.ID = _PGID_EVALUATE_READONLY OrElse pg.ID = _PGID_TEAMTIME OrElse pg.ID = _PGID_TEAMTIME_CV Then    ' D0194 + D0511 + D0589 + D1243 + D3563 + D4940
                    Dim isLocked As Boolean = Not _Page.App.ActiveProject.LockInfo.isLockAvailable(_Page.App.ActiveUser)    ' D0135 + D0474 + D0483 + D0589
                    If isLocked Or Not _Page.CanEditActiveProject Or _Page.App.ActiveProject.isTeamTime Then  ' D0135 + D1333
                        'lblLockedProject.Visible = True
                        ' D0194 + D0300 ===
                        Dim sRes As String = ""
                        Select Case _Page.App.ActiveProject.ProjectStatus
                            Case ecProjectStatus.psArchived
                                sRes = "lbl_ArchivedProject"
                            Case ecProjectStatus.psTemplate
                                sRes = "msgProjectIsTemplate"
                            Case ecProjectStatus.psMasterProject    ' D2479
                                sRes = "msgProjectIsTemplate"   ' D2479
                        End Select
                        If sRes <> "" Then sRes = _Page.ResString(sRes) ' D0491
                        If tPrj.isMarkedAsDeleted Then sRes = CStr(IIf(sRes = "", "", ", ")) + _Page.ResString("lblMarkedAsDeleted") ' D0789
                        If sRes = "" Then
                            ' D0417 ===
                            If isLocked AndAlso Not _Page.App.ActiveProject.isTeamTime Then ' D1178
                                sRes = _Page.ResString("msgProjectLocked")
                            Else
                                If _Page.App.ActiveProject.isTeamTime AndAlso _Page.App.ActiveProject.MeetingStatus(_Page.App.ActiveUser) = ECTeamTimeStatus.tsTeamTimeSessionOwner Then    ' D1178
                                    sRes = _Page.ResString("lblSynchronousProject")
                                    If _MEETING_ID_AVAILABLE Then
                                        WarningText = sRes  ' D0594
                                        Dim tmp As String = _Page.App.Options.EvalSiteURL   ' D4482
                                        _Page.App.Options.EvalSiteURL = ""  ' D4482
                                        sRes = String.Format("{0}. <span style='font-weight:normal; white-space:nowrap'>{1}: <a href='{4}' class='dashed' onclick=""CopyLink('{4}'); return false;"" title='{3}'>{2}</a></span>", sRes, _Page.ResString("lblMeetingID"), clsMeetingID.AsString(_Page.App.ActiveProject.MeetingID), "Copy link with MeetingID to Clipboard", JS_SafeString(_Page.ParseAllTemplates(_TEMPL_URL_MEETINGID, _Page.App.ActiveUser, _Page.App.ActiveProject))) ' D0420 + D0578 + D0594 + D1361 + D1705
                                        _Page.App.Options.EvalSiteURL = tmp ' D4482
                                    End If
                                End If
                            End If
                            If Not _Page.CanEditActiveProject AndAlso sRes = "" AndAlso Not _Page.App.ActiveProject.isTeamTime Then ' D1178
                                sRes = _Page.ResString("lblReadOnly")
                                If Not _Page.App.ActiveProject.isValidDBVersion Then sRes = _Page.ResString("msgWrongProjectDB") ' D0631
                            End If
                        End If
                        If Not sRes = "" Then sLockedMsg = sRes
                        'If Not sRes = "" Then lblLockedProject.Text = sRes
                        ' D0416 ==
                        ' D0300 ==
                        If Not _Page.App.ActiveProject.isTeamTime Then
                            'PrjName = String.Format("{0} [{1}]", PrjName, _Page.ResString(IIf(isLocked, "lblProjectLocked", "lbl_ArchivedProject"))) ' D0135
                            ' D0589 ===
                            WarningText = _Page.ResString(CStr(IIf(isLocked, "lblProjectLocked", "lbl_ArchivedProject")))
                            If _Page.App.ActiveProject.LockInfo.LockStatus = ECLockStatus.lsLockForAntigua Then
                                sLockedMsg = ""
                                WarningText = _Page.ResString("lblProjectLockedForAntigua")
                            End If
                            ' D0589 ==
                        Else
                            lblLockedProject.Attributes("class") = "top_warning_nofloat"  ' D0578
                        End If
                        ' D0194 ==
                    End If
                End If
                ' D0134 ==
                If pg.RoleLevel = ecRoleLevel.rlModelLevel And pg.ActionType <> ecActionType.atUnspecified Then
                    Page.Title = String.Format(_Page.ResString("titleWindow"), String.Format(_Page.ResString("titleProject"), Page.Title, PrjName), _Page.ApplicationName) ' D0088 + D0867
                    fShowProjectTitle = True    ' D0071
                End If
            End If
            If Not fShowProjectTitle Then
                PrjName = ""
                ' D1216 ===
            Else
                lblProjectName.Text = String.Format("<b>{0}</b>: {1}", _Page.ResString("tblProject"), PrjName)
                lblProjectName.Visible = True
            End If
        End If
        ' D1216 ==
        'If _Page.Title <> "" And _Page.Title = _Page.GetPageTitle Then Page.Title = String.Format(_Page.ResString("titleApplicationHeader"), Page.Title, _Page.ApplicationName) ' D0135
        'If _Page.Title = "" Then Page.Title = _Page.ApplicationName ' D0135
        ' D0052 ==

        ' D1333 ===
        If Not lblProjectName.Visible AndAlso HasListPage(_PAGESLIST_SPYRON, _Page.CurrentPageID) AndAlso _Page.App.ActiveSurveysList IsNot Nothing Then
            Dim sID As String = EcSanitizer.GetSafeHtmlFragment(_Page.CheckVar(_PARAM_ID, ""))    ' Anti-XSS
            Dim SurveyID As Integer = -1
            If Integer.TryParse(sID, SurveyID) Then
                Dim _Survey As clsSurveyInfo = _Page.App.SurveysManager.GetSurveyInfo(_Page.App.ActiveSurveysList, SurveyID)
                If _Survey IsNot Nothing Then
                    PrjName = _Survey.Title
                    Page.Title = String.Format(_Page.ResString("titleWindow"), String.Format(_Page.ResString("titleProject"), Page.Title, PrjName), _Page.ApplicationName)
                    lblProjectName.Text = String.Format("<b>{0}</b>: {1}", _Page.ResString("tblSurvey"), PrjName)
                    lblProjectName.Visible = True
                End If
            End If
        End If
        ' D1333 ==

        ' D0125 ===
        Dim Langs As List(Of clsLanguageResource) = _Page.App.Languages
        If Langs.Count < 2 Or Not ShowLanguages Then  ' D0211 + D0315
            lblLanguages.Visible = False
            RadToolTipLanguages.Visible = False ' D0127
        Else
            Dim Lst As New ArrayList
            For Each tLang As clsLanguageResource In _Page.App.Languages
                Dim N As String = tLang.LanguageName
                If tLang.LanguageCode.ToLower = _Page.App.LanguageCode.ToLower Then
                    N = String.Format("<b>{0}</b>", N)
                Else
                    N = String.Format("<a href='{0}' class='actions'>{1}</a>", URLParameter(Request.Url.PathAndQuery, "language", tLang.LanguageCode.ToLower), N)
                End If
                Lst.Add(N)
            Next
            Dim cLang As clsLanguageResource = clsLanguageResource.LanguageByCode(_Page.App.LanguageCode, Langs)
            If Not cLang Is Nothing Then lblLanguages.Text = cLang.LanguageName Else lblLanguages.Text = _Page.App.LanguageCode
            rptLanguages.DataSource = Lst
            rptLanguages.DataBind()
            lblLanguages.Visible = True         ' D0127
            RadToolTipLanguages.Visible = True  ' D0127
        End If
        ' D0125 ==

        lblAppVersion.Text = String.Format(_Page.ResString("lblVersion"), GetVersion(_Page.GetWebCoreVersion(), VersionFormat.Normal))   ' D0571 + D0576 + D4234
        'lblAppVersion.ToolTip = String.Format("<div>Business Core ver: {0}</div><div>Web Core ver: {3}</div><div>ECCore ver: {1}</div><div>Canvas Module ver: {2}</div>", GetVersion(_Page.App.GetCoreVersion()), GetVersion(GetCurrentECCoreVersion), GetVersion(GetCurrentCanvasModuleVersion), GetVersion(_Page.GetWebCoreVersion)) ' D0571 + D0576 -D0779

        'lblCurrentProject.Text = ""     ' D0071
        If _Page.App.isAuthorized Then
            ' D0048 ===
            Dim curUser As clsApplicationUser = _Page.App.ActiveUser
            Dim sUser As String = curUser.UserEmail
            Dim sUserEmail As String = curUser.UserEmail
            If curUser.UserName <> "" Then sUser = curUser.UserName
            lblSessionData.Text = String.Format("{0}: <a href='mailto:{2}'>{1}</a>", _Page.ResString("lblLoggedInAs"), sUser, sUserEmail) ' D0045 + D0049
            ' D2545 ===
            If _Page.App.Options.isSingleModeEvaluation OrElse _Page.App.Options.OnlyTeamTimeEvaluation Then    ' D2943
                lblLogout.Text = String.Format("&nbsp;&nbsp|&nbsp;&nbsp;<a href='{0}' class='actions' onclick='DoLogout(this); return false;'>{1}</a>", _Page.PageURL(+_PGID_LOGOUT), _Page.PageMenuItem(_PGID_LOGOUT))
                lblLogout.Visible = True
            End If
            ' D2545 ==
        End If

        ' D0189 + D0194 ===
        Dim fShowWarning As Boolean = sLockedMsg <> ""
        If fShowWarning And _Page.App.HasActiveProject Then
            If _Page.App.ActiveProject.isTeamTime Then fShowWarning = _Page.CurrentPageID <> _PGID_EVALUATION AndAlso _Page.CurrentPageID <> _PGID_EVALUATION AndAlso _Page.App.ActiveProject.MeetingStatus(_Page.App.ActiveUser) <> ECTeamTimeStatus.tsTeamTimeSessionOwner ' D0487 + D0511
            ' D0309 ===
            If _Page.CurrentPageID = _PGID_EVALUATE_READONLY Then
                fShowWarning = False
            End If
            ' D0309 ==
            ' D0525 ===
            If (_Page.App.ActiveProject.ProjectStatus = ecProjectStatus.psTemplate OrElse _Page.App.ActiveProject.ProjectStatus = ecProjectStatus.psMasterProject) AndAlso _Page.CurrentPageID = _PGID_PROJECT_PROPERTIES Then ' D2479
                fShowWarning = False
            End If
            ' D0525 ==
        End If
        ' D0336 ===
        If _Page.ExtraPageMessage <> "" Then
            sLockedMsg += _Page.ExtraPageMessage
        End If
        ' D0336 ==
        If fShowWarning Then
            ' D0194 ==
            pnlGlobalWarning.Visible = True
            pnlGlobalWarning.WarningShowOnLoad = True   ' D0190
            pnlGlobalWarning.Caption = _Page.ResString("lblWarning")
            pnlGlobalWarning.CloseCaption = _Page.ResString("btnClose")
            pnlGlobalWarning.Message = String.Format("<div class='error'>{0}</div>", sLockedMsg)
        Else
            If sLockedMsg <> "" Then WarningText = sLockedMsg
        End If
        ' D0189 ==

        ' D0917 ===
        If Not _Page.App.isCommercialUseEnabled AndAlso Not lblNonCommercial.Visible Then
            tdMainContent.Attributes("class") = "content nc"
            lblNonCommercial.InnerHtml = _Page.ResString("lblNonCommercialUseOnly")
            lblNonCommercial.Visible = True
        End If
        ' D0917 ==

        DebugInfo("Master page rendered")
    End Sub
    ' D0005 ==

    ' -D7607 Disabled due to an issues
    'Private _antiXsrfTokenValue As String
    'Private _antiXsrfSessionDebug As String = "SessionAntiXsrfDebug"

    'Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
    '    Dim antiXsrfDebugValue As String = ""
    '    If IsNothing(Session(_antiXsrfSessionDebug)) Then
    '        Session(_antiXsrfSessionDebug) = antiXsrfDebugValue
    '    Else
    '        antiXsrfDebugValue = CStr(Session(_antiXsrfSessionDebug))
    '    End If

    '    ' The code below helps to protect against XSRF attacks
    '    Dim requestCookie = Request.Cookies(AntiXsrfTokenKey)
    '    Dim requestCookieGuidValue As Guid
    '    Dim isGuid as Boolean = False

    '    If requestCookie IsNot Nothing Then
    '        Try
    '            requestCookieGuidValue = New Guid(requestCookie.Value)
    '            isGuid = True
    '        Catch ex As Exception
    '        End Try
    '    End If

    '    If isGuid Then
    '        ' Use the Anti-XSRF token from the cookie
    '        _antiXsrfTokenValue = requestCookie.Value
    '        Page.ViewStateUserKey = _antiXsrfTokenValue
    '    Else
    '        ' Generate a new Anti-XSRF token and save to the cookie
    '        _antiXsrfTokenValue = Guid.NewGuid().ToString("N")
    '        Page.ViewStateUserKey = _antiXsrfTokenValue

    '        Dim responseCookie = New HttpCookie(AntiXsrfTokenKey) With { .HttpOnly = True, .Value = _antiXsrfTokenValue, .Secure = Request.IsSecureConnection }
    '        'If FormsAuthentication.RequireSSL AndAlso Request.IsSecureConnection Then
    '        '    responseCookie.Secure = True
    '        'End If
    '        Response.Cookies.[Set](responseCookie)
    '    End If

    '    antiXsrfDebugValue += String.Format("G{0}", IIf(isGuid, 1, 0))
    '    Session(_antiXsrfSessionDebug) = antiXsrfDebugValue

    '    AddHandler Page.PreLoad, AddressOf master_Page_PreLoad
    'End Sub

    'Protected Sub master_Page_PreLoad(sender As Object, e As EventArgs)
    '    Dim antiXsrfDebugValue As String = CStr(Session(_antiXsrfSessionDebug))

    '    If Not IsPostBack Then
    '        ' Set Anti-XSRF token
    '        ViewState(AntiXsrfTokenKey) = Page.ViewStateUserKey
    '        ViewState(AntiXsrfUserNameKey) = If(Context.User.Identity.Name, [String].Empty)

    '        If antiXsrfDebugValue.Length > 2 Then
    '            antiXsrfDebugValue = antiXsrfDebugValue.Substring(antiXsrfDebugValue.Length - 2)
    '            Session(_antiXsrfSessionDebug) = antiXsrfDebugValue
    '        End If

    '        antiXsrfDebugValue += "P0"

    '    Else
    '        antiXsrfDebugValue += "P1"

    '        ' Set Anti-XSRF token
    '        If ((Page.IsCallback OrElse Page.IsAsync) AndAlso ViewState(AntiXsrfTokenKey) Is Nothing AndAlso Page.ViewStateUserKey IsNot Nothing) Then
    '            ViewState(AntiXsrfTokenKey) = Page.ViewStateUserKey
    '            ViewState(AntiXsrfUserNameKey) = If(Context.User.Identity.Name, [String].Empty)
    '            antiXsrfDebugValue += String.Format("X{0}{1}", IIf(Page.IsCallback, 1, 0), IIf(Page.IsAsync, 1, 0))
    '        End If

    '        ' Validate the Anti-XSRF token
    '        If ViewState(AntiXsrfTokenKey) Is Nothing OrElse (DirectCast(ViewState(AntiXsrfTokenKey), String) <> _antiXsrfTokenValue OrElse DirectCast(ViewState(AntiXsrfUserNameKey), String) <> (If(Context.User.Identity.Name, [String].Empty))) Then
    '            'If DirectCast(ViewState(AntiXsrfTokenKey), String) <> _antiXsrfTokenValue OrElse DirectCast(ViewState(AntiXsrfUserNameKey), String) <> (If(Context.User.Identity.Name, [String].Empty)) Then
    '            antiXsrfDebugValue += "V"
    '            antiXsrfDebugValue += If(ViewState(AntiXsrfTokenKey) Is Nothing, "1", "0")
    '            antiXsrfDebugValue += If(DirectCast(ViewState(AntiXsrfTokenKey), String) <> _antiXsrfTokenValue, "1", "0")
    '            antiXsrfDebugValue += If(DirectCast(ViewState(AntiXsrfUserNameKey), String) <> (If(Context.User.Identity.Name, [String].Empty)), "1", "0")

    '            antiXsrfDebugValue += If(String.IsNullOrEmpty(CStr(ViewState(AntiXsrfTokenKey))), "1", "0")
    '            antiXsrfDebugValue += If(String.IsNullOrEmpty(_antiXsrfTokenValue), "1", "0")
    '            antiXsrfDebugValue += If(String.IsNullOrEmpty(CStr(ViewState(AntiXsrfUserNameKey))), "1", "0")
    '            antiXsrfDebugValue += If(String.IsNullOrEmpty(If(Context.User.Identity.Name, [String].Empty)), "1", "0")

    '            Session(_antiXsrfSessionDebug) = Nothing
    '            Throw New InvalidOperationException("Validation of Anti-XSRF token failed. " + antiXsrfDebugValue)
    '        End If

    '        antiXsrfDebugValue += "V1"
    '    End If

    '    Session(_antiXsrfSessionDebug) = antiXsrfDebugValue
    'End Sub

End Class
