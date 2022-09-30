Partial Class ServicePage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_SERVICEPAGE)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        ShowNavigation = False
        StorePageID = False ' D1574

        tdMessage.InnerHtml = String.Format("<div style='text-align:center; padding:2em;'><img src='/Images/loader.gif' width=60 title='Please wait' border=0></div><h6>{1}</h6>", ImagePath, ResString("lblPleaseWait")) ' D3255

        If CheckVar("msg", "").ToLower = "no_report" AndAlso Not App.HasActiveProject Then Response.Redirect(PageURL(_PGID_PROJECTSLIST), True)   ' D7170

        ' D0781 ===
        Select Case CheckVar("action", "").ToLower
            'Case "html"
            '    Dim sRefreshURL As String = PageURL(_PGID_PROJECTSLIST)
            '    App.ActiveProjectsList = Nothing
            '    App.UserWorkgroups = Nothing
            '    App.Workspaces = Nothing
            '    'DefaultMasterPage = _MASTER_EC09
            '    DefaultMasterPage = _MASTER_EC2018  ' D4961
            '    ' D6061 ===
            '    Dim PgID As Integer = CurrentPageID
            '    Dim DefPgID As Integer = If(App.isRiskEnabled, _DEF_PGID_ONSELECTPROJECT_RISK, _DEF_PGID_ONSELECTPROJECT)
            '    If Not Integer.TryParse(CheckVar("pgid", ""), PgID) Then PgID = DefPgID
            '    If PgID = _PGID_SILVERLIGHT_UI Then PgID = DefPgID
            '    ' D6061 ==
            '    'App.Options.ShowSilverlightShellOnLogon = False
            '    'If CheckVar(_ACTION_SAVE, False) Then SetCookie(_COOKIE_FORCE_SL, App.Options.ShowSilverlightShellOnLogon.ToString, False, False) ' D0853
            '    Dim tPG As clsPageAction = PageByID(PgID)
            '    If tPG Is Nothing AndAlso PgID = _PGID_SILVERLIGHT_UI Then tPG = PageByID(DefPgID)  ' D6061
            '    If tPG IsNot Nothing Then sRefreshURL = tPG.URL
            '    'SetCookie(_COOKIE_USED_SL, "", True, False) ' D1105
            '    Response.Redirect(sRefreshURL)

            Case "nosurvey"
                tdMessage.InnerHtml = "<h6>Survey not found</h6>Page will be closed in 5 seconds."

                ' D1624 ===
            Case "msg"
                Dim sRes As String = ResString("errNoAccess")
                Dim sType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("type", "")).ToLower   ' Anti-XSS
                If Not App.isAuthorized AndAlso sType <> "help" Then  ' D4022
                    sRes = ResString("msgRestricted")
                    sRes += String.Format("<p align=center><input type='button' class='button' style='width:12em' value='{0}' onclick='Redirect(""{1}""); return false'></p>", ResString("btnClick4Login"), PageURL(_PGID_START)) ' D3331
                    'If CheckVar("_pgid", "") <> CInt(_PGID_EVALUATE_INFODOC).ToString Then ClientScript.RegisterStartupScript(GetType(String), "CallReloadShell", "AskSLReload();", True) ' D1625
                    ClientScript.RegisterStartupScript(GetType(String), "CallReloadShell", "AskSLReload();", True)
                Else
                    Select Case sType
                        Case "err"
                            Dim sPgID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("pg", "")).ToLower ' Anti-XSS
                            Select Case sPgID
                                Case _PGID_ERROR_403.ToString
                                    sRes = ResString("titleError403")
                                Case _PGID_ERROR_404.ToString
                                    sRes = ResString("titleError404")
                                Case _PGID_ERROR_500.ToString
                                    sRes = ResString("msgApplicationError")
                                Case _PGID_ERROR_503.ToString
                                    If Not String.IsNullOrEmpty(App.ApplicationError.Message) Then sRes = App.ApplicationError.Message Else sRes = ResString("errLicenseOver")
                                Case Else
                                    ClientScript.RegisterStartupScript(GetType(String), "CallReloadShell", "setTimeout('RefreshPage();', 60000);", True)
                            End Select
                            ' D3197 ===
                        Case "fetch"
                            Dim tPgID As Integer = CheckVar("_pgid", CurrentPageID)
                            ' D4269 ===
                            Dim sName As String = ""
                            Dim sLnk As String = ""
                            If (tPgID >= _PGID_RA_BASE AndAlso tPgID < 77200) OrElse (tPgID = 771055) Then
                                sName = ResString("titleResourceAligner")
                                sLnk = ResString("lnkRAHelp")
                            End If
                            If (tPgID >= _PGID_SURVEY_EDIT_PRE AndAlso tPgID < 79100) OrElse tPgID = 20304 OrElse tPgID = 92016 OrElse tPgID = 920016 Then
                                sName = ResString("titleApplicationSurvey")
                                sLnk = ResString("lnkSurveyHelp")    ' Measure/Creating_and_Editing_an_Insight_Survey.html
                            End If
                            If tPgID = _PGID_TEAMTIME OrElse tPgID = _PGID_TEAMTIME_CV OrElse tPgID = 70533 OrElse (tPgID >= _PGID_TEAMTIME_INVITE AndAlso tPgID < 30600) Then
                                sName = ResString("titleApplicationTeamTime")
                                sLnk = ResString("lnkTTHelp")    ' Measure/Measure_Teamtime_Instructions.html
                            End If
                            If sName <> "" Then sRes = String.Format("<h3>{0}</h3>", String.Format(ResString("msgNoRALicense"), sName, "openChat();", sLnk)) ' D4267
                            ' D3197 + D4269 ==
                            ' D3241 + D3244 ===
                        Case "fp_comingsoon"
                            sRes = ResString("msgRA_FP_ComingSoon")
                            ' D3241 + D3244 ==
                            ' D4023 ===
                        Case "help"
                            sRes = ResString("msgPleaseAuthorize")
                            sRes += String.Format("<p align=center><input type='button' class='button' style='width:10em; margin-top:2em;' value='{0}' onclick='tryLogin();'></p>", ResString("btnLogIn")) ' D4025
                            ' D4023 ==
                            ' D4872 ===
                        Case "no_result", "no_report"   ' D7170
                            If Not App.HasActiveProject Then Response.Redirect(PageURL(_PGID_PROJECTSLIST), True)   ' D7170
                            sRes = "Sorry, but this part of application is not avilable to display on that instance."
                            SessVar(SESSION_NO_REPORTS) = "1"
                            ' D4872 ==
                    End Select
                End If
                If sRes <> "" Then
                    tdMessage.InnerHtml = String.Format("<h5 class='error'>{0}</h5>", sRes)
                    SetPageTitle(ShortString(HTML2Text(sRes).Replace(vbLf, "").Replace(vbCr, " "), 80))   ' D3241 + D4267
                End If
                ' D1624 ===

                ' D2119 ===
            Case "redirect"
                Dim sURL As String = HttpUtility.UrlDecode(EcSanitizer.GetSafeHtmlFragment(CheckVar("url", _URL_ROOT)))   ' Anti-XSS
                If sURL = "" Then sURL = _URL_ROOT
                ClientScript.RegisterStartupScript(GetType(String), "DoRedirect", String.Format("Redirect('{0}');", JS_SafeString(sURL)), True)
                ' D2119 ==

                ' D3253 ===
            Case "flow"
                Dim sType As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("type", ""))   ' Anti-XSS
                ' D3256 ===
                Select Case sType.Trim.ToLower
                    Case "riskresults"
                        ClientScript.RegisterStartupScript(GetType(String), "RiskResults", "RiskResults();", True)
                    Case "impact"
                        ClientScript.RegisterStartupScript(GetType(String), "EvalImpact", "StartEvalImpact();", True)
                    Case Else
                        ClientScript.RegisterStartupScript(GetType(String), "EvalImpact", "StartEvalLikelihood();", True)
                End Select
                ' D3253 + D3256 ==

                ' D4143 ===
            Case "navigate"
                Dim sPgID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("pgid", ""))
                ClientScript.RegisterStartupScript(GetType(String), "Navigate", String.Format("setTimeout('navOpenPage({0});', 150);", sPgID), True)
                ' D4143 ==

                ' D3561 ===
            Case "eval_teamtime", "eval_anytime"
                Dim sMode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("action", "")).ToLower ' Anti-XSS
                If App.ActiveUser Is Nothing OrElse Not App.HasActiveProject Then
                    tdMessage.InnerHtml = String.Format("<h5 class='error'>Sorry, but this action is not allowed.</h5>")
                Else
                    Dim fCanOpen As Boolean = True  ' D4315
                    Dim sRoot As String = ApplicationURL(True, sMode <> "eval_anytime")
                    Dim sURL As String = ""
                    ' D6610 ===
                    Dim sExtra As NameValueCollection = HttpUtility.ParseQueryString(GetParamsWithoutAuthKeys(Request.QueryString))
                    If GetParam(sExtra, _PARAM_ACTION, False) <> "" Then sExtra.Remove(_PARAM_ACTION)
                    If GetParam(sExtra, "redirect", False) <> "" Then sExtra.Remove("redirect")
                    If GetParam(sExtra, "from", False) <> "" Then sExtra.Remove("from")
                    If GetParam(sExtra, "pipe", False) <> "" Then sExtra.Remove("pipe")
                    Dim sExtraParams As String = sExtra.ToString
                    If sExtraParams <> "" Then sExtraParams = "&" + sExtraParams
                    ' D6610 ==
                    If sRoot = "" AndAlso App.Options.EvalSiteURL = "" Then
                        sURL = PageURL(CInt(IIf(sMode = "eval_anytime", _PGID_EVALUATION, _PGID_TEAMTIME)), "redirect=no" + GetTempThemeURI(True)) + "&from=comparion" + sExtraParams   ' D4184 + D6610
                    Else
                        ' D4315 ===
                        sURL = CreateLogonURL(App.ActiveUser, App.ActiveProject, "pipe=yes", sRoot, App.ActiveProject.Passcode(App.ActiveProject.isImpact), , True) + "&redirect=no&action=" + sMode + "&from=comparion" + sExtraParams     ' D4184 + D4315 + D4616 = D6610
                        If sMode.ToLower = "eval_anytime" AndAlso App.HasActiveProject AndAlso (App.ActiveProject.isTeamTimeImpact OrElse App.ActiveProject.isTeamTimeLikelihood) Then
                            If Not ShowDraftPages() Then sURL = PageURL(_PGID_TEAMTIME) Else sURL = sURL.Replace("eval_anytime", "eval_teamtime")
                            If (Not App.isRiskEnabled AndAlso Not App.Options.EvalURL4TeamTime) OrElse (App.isRiskEnabled AndAlso Not App.Options.EvalURL4TeamTime_Riskion) Then sURL = PageURL(_PGID_TEAMTIME) ' D6705
                            fCanOpen = False
                            ClientScript.RegisterStartupScript(GetType(String), "AskRedirect", String.Format("setTimeout('dxDialog(""{0}"", ""document.location.href = \'{1}\';"", ""window.open(\'\', \'_self\', \'\'); window.close();"", ""{2}"", ""{3}"", ""{4}"");', 100);", JS_SafeString(ResString("msgNoAnytimeWhileTeamTime")), JS_SafeString(sURL), JS_SafeString(ResString("titleConfirmation")), JS_SafeString(ResString("btnYes")), JS_SafeString(ResString("btnNo"))), True) ' D4185
                        End If
                        ' D4315 ==
                    End If
                    'If sURL <> "" Then Response.Redirect(sURL, True)
                    If fCanOpen Then
                        Response.Redirect(sURL, True)
                        'ClientScript.RegisterStartupScript(GetType(String), "DoRedirect", String.Format("document.location.href = '{0}';", JS_SafeString(sURL)), True) ' D4185 + D4315
                    End If
                End If
                ' D3561 ==

                ' D3594 ===
            Case "cs"
                Dim sMode As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("mode", "")).ToLower   ' Anti-XSS
                tdMessage.InnerHtml = String.Format("<p align='center'><input type='button' class='button' style='width:24em; padding:6px; font-size:11pt; color:#005292; font-weight: bold;' value='{0}' onclick='document.location.href=""{1}{2}""; return false'></p>", SafeFormString(ResString(CStr(IIf(sMode.Contains("individual"), "btnStartCS_Indiv", "btnStartCS_TT")))), PageURL(_PGID_ANTIGUA_MEETING), JS_SafeString(Request.Url.Query.Replace("action=cs", "").Replace("?&", "?").Replace("&&", "&")))
                ' D3594 ==

                ' D4687 ===
            Case "intensities"
                SessVar(SESSION_ORIGINAL_HID) = Nothing
                SessVar(SESSION_SCALE_ID) = Nothing
                SessVar(SESSION_EXCLUDE_GUIDS) = Nothing    ' D2044
                App.ActiveProject.PipeParameters.CurrentParameterSet = App.ActiveProject.PipeParameters.DefaultParameterSet
                ' D4687 ==

        End Select
        ' D0781 ==

    End Sub

    '' D1624 ===
    'Protected Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
    '    Page.MasterPageFile = _URL_ROOT + _MASTER_EMPTY ' D4961
    'End Sub
    '' D1624 ==

End Class
