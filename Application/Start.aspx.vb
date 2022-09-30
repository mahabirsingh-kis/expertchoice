Imports System.Threading

Partial Class StartSignUpPage
    Inherits clsComparionCorePage

    ' D1059 ===
    Public fEmail As Boolean = True
    Public fName As Boolean = True
    Public fPsw As Boolean = True
    Public fPhone As Boolean = False

    ' D1071 ===
    Public fReqEmail As Boolean = True
    Public fReqName As Boolean = True
    Public fReqPsw As Boolean = True
    Public fReqPhone As Boolean = False
    ' D1071 ==

    Public CustomContent As String = ""     ' D4650

    Private sRoleGoupID As String = ""      ' D1937
    Private sWkgRoleGroupID As String = ""  ' D2287
    Private sJoinAsPM As String = ""        ' D4332

    Private sMode As String = ""            ' D4513
    Private sReq As String = ""             ' D4513

    Dim sPasscode As String = ""

    'Public fForceUserPsw As Boolean = False    ' D1256

    Private Sub InitParams()
        Dim tParams As New NameValueCollection  ' D1535
        If Request IsNot Nothing AndAlso Request.Params IsNot Nothing Then tParams = Request.Params ' D1535

        Dim sKey As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(tParams, _PARAMS_KEY))   ' Anti-XSS
        Dim sTinyURL As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(tParams, _PARAMS_TINYURL))   ' Anti-XSS
        If sKey = "" AndAlso sTinyURL <> "" Then
            sKey = App.DecodeTinyURL(sTinyURL)
            If String.IsNullOrEmpty(sKey) Then sKey = ""
        End If
        If sKey <> "" Then
            sKey = DecodeURL(sKey, App.DatabaseID)
            If sKey <> "" Then
                Dim tNewParams As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(sKey))
                If tNewParams IsNot Nothing AndAlso tNewParams.Count > 0 Then   ' D1535
                    For Each sName As String In tParams
                        If sName IsNot Nothing AndAlso Array.IndexOf(_PARAMS_KEY, sName.ToLower) < 0 AndAlso Array.IndexOf(_PARAMS_TINYURL, sName.ToLower) < 0 AndAlso Array.IndexOf(tNewParams.AllKeys, sName.ToLower) < 0 Then tNewParams.Add(sName, tParams(sName))
                    Next
                    tParams = tNewParams
                End If
            End If
        End If

        sPasscode = EcSanitizer.GetSafeHtmlFragment(ParamByName(tParams, _PARAMS_PASSCODE))   ' Anti-XSS

        sMode = EcSanitizer.GetSafeHtmlFragment(ParamByName(tParams, _PARAMS_SIGNUP_MODE)).Trim.ToLower ' Anti-XSS + D4513

        fEmail = sMode = "" OrElse sMode.Contains("e")
        fName = sMode = "" OrElse sMode.Contains("n")
        'fPsw = sMode = "" Or sMode.Contains("p") Or fForceUserPsw   ' D1256
        fPsw = If(isSSO(), False, sMode = "" OrElse sMode.Contains("p")) ' D6552
        fPhone = sMode.Contains("t")    ' D4641

        If Not fEmail AndAlso Not fName AndAlso Not fPsw Then
            fEmail = True
            fName = True
            fPsw = Not isSSO()    ' D6552
        End If

        If fPsw AndAlso Not fName AndAlso Not fEmail Then fEmail = True

        ' D1071 ===
        sReq = EcSanitizer.GetSafeHtmlFragment(GetParam(tParams, "req")).Trim.ToLower   ' Anti-XSS + D4513

        fReqEmail = fEmail AndAlso sReq.Contains("e")
        fReqName = fName AndAlso sReq.Contains("n")
        fReqPsw = If(isSSO(), False, fPsw AndAlso (sReq.Contains("p") OrElse Not WebOptions.AllowBlankPsw)) ' D4495 + D6552
        fReqPhone = fPhone AndAlso sReq.Contains("t")   ' D4641
        ' D1071 ==

        ' D1512 ===
        If Not IsPostBack AndAlso Not IsCallback AndAlso CheckVar("f", False) AndAlso Not fReqPsw AndAlso Not fPsw Then
            If fEmail AndAlso fReqEmail AndAlso tbEmail.Text = "" Then tbEmail.Text = EcSanitizer.GetSafeHtmlFragment(ParamByName(tParams, _PARAMS_EMAIL))    ' Anti-XSS
            If fName AndAlso fReqName AndAlso tbFullName.Text = "" Then tbFullName.Text = EcSanitizer.GetSafeHtmlFragment(ParamByName(tParams, _PARAMS_USERNAME)) ' Anti-XSS
            If fPhone AndAlso fReqPhone AndAlso tbPhone.Text = "" Then tbPhone.Text = EcSanitizer.GetSafeHtmlFragment(GetParam(tParams, "phone")) ' D4641
        End If
        ' D1512 ==

        sRoleGoupID = EcSanitizer.GetSafeHtmlFragment(GetParam(tParams, _PARAM_ROLEGROUP))           ' D1937 + Anti-XSS
        sWkgRoleGroupID = EcSanitizer.GetSafeHtmlFragment(GetParam(tParams, _PARAM_WKG_ROLEGROUP))   ' D2287 + Anti-XSS
        sJoinAsPM = EcSanitizer.GetSafeHtmlFragment(GetParam(tParams, _PARAM_AS_PM))    ' D4332
        If sJoinAsPM = "1" Then sJoinAsPM = String.Format("&{0}=1", _PARAM_AS_PM) ' D4332
    End Sub
    ' D1059 ==

    ' D2531 ===
    Private Sub ShowErrorMessage(sMsg As String)
        lblFormError.Text = sMsg
        lblFormError.Visible = sMsg <> ""
    End Sub
    ' D2531 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If Not IsPostBack AndAlso Not isCallback Then   ' D1059
            If App.isAuthorized Then FetchAccess(_PGID_START) ' D1059

            AlignHorizontalCenter = True
            AlignVerticalCenter = True

            InitParams()    ' D1059

            lblError.Visible = False
            pnlSignupForm.Visible = False

            Dim tProject As clsProject = Nothing
            If sPasscode <> "" Then tProject = App.DBProjectByPasscode(sPasscode)
            If tProject Is Nothing Then
                lblError.Text = String.Format("<h4 class='error'>{0}</h4>", ResString("msgProjectNotFound")) ' D1092 + D1398
                lblError.Visible = True
            Else
                ' D1092 ===
                If tProject.ProjectStatus <> ecProjectStatus.psActive OrElse Not tProject.isOnline OrElse Not tProject.isPublic Then    ' D1098 + D1894 + D2019
                    Dim sErrMsg As String = If(tProject.ProjectStatus = ecProjectStatus.psActive, "msgProjectisUnavailable", "msgAuthProjectReadOnly")   ' D2019 + D4161
                    If Not tProject.isPublic Then sErrMsg = "msgDisabledPasscode" ' D2019
                    lblError.Text = String.Format("<h4>{0}</h4>", String.Format(ResString(sErrMsg), tProject.ProjectName)) ' D1398 + D2019
                    lblError.Visible = True
                Else
                    ' D1092 ==

                    ' D1059 ===
                    pnlSignupForm.Visible = True
                    ShowErrorMessage("")    ' D2531

                    hdr.InnerHtml = String.Format(ResString("msgSignUpEvaluate"), tProject.ProjectName) '   D1059
                    ' D4650 ===
                    If tProject.ProjectManager.Parameters.InvitationCustomTitle <> "" Then
                        hdr.InnerHtml = SafeFormString(ParseAllTemplates(tProject.ProjectManager.Parameters.InvitationCustomTitle, Nothing, tProject))
                        SetPageTitle(hdr.InnerHtml)
                    End If
                    If tProject.ProjectManager.Parameters.InvitationCustomText <> "" Then
                        Dim sHTML As String = tProject.ProjectManager.Parameters.InvitationCustomText.Trim
                        If sHTML <> "" Then
                            If isMHT(sHTML) Then sHTML = Infodoc_Unpack(tProject.ID, tProject.ProjectManager.ActiveHierarchy, reObjectType.ExtraInfo, _PARAM_INVITATION_CUSTOM, sHTML, True, True, -1)
                            CustomContent = HTML2TextWithSafeTags(sHTML, _DEF_SAFE_TAGS + "IMG;TABLE;TR;TH;TD;").Trim()
                            If CustomContent <> "" Then
                                CustomContent = String.Format("<div style='max-height:60%; oveflow:auto; padding:0px; margin:0px 20% 1em 20%; text-align:center;' class='text'>{0}</div>", ParseAllTemplates(CustomContent, Nothing, tProject))
                            End If
                        End If
                    End If
                    ' D4650 ==

                    trEmail.Visible = fEmail
                    trName.Visible = fName
                    trPhone.Visible = fPhone    ' D4641
                    trPsw1.Visible = fPsw
                    trPsw2.Visible = trPsw1.Visible
                    'trPsw2.Visible = trPsw1.Visible AndAlso Not fForceUserPsw ' D2014

                    btnOK.Text = ResString("btnSignup")
                    btnOKR.Text = ResString("btnLogin")    ' D2525

                    ' D1092 ===
                    Dim sFocus As String = ""
                    ' D2525 ===
                    Dim sEmailR As String = GetCookie(_COOKIE_EMAIL, "")
                    If Not String.IsNullOrEmpty(sEmailR) AndAlso Not isSSO() Then   ' D6552
                        tbEmailR.Text = sEmailR
                        sFocus = tbEmailR.ClientID
                    Else
                        If fPsw Then sFocus = tbPassword.ClientID
                        If fName Then sFocus = tbFullName.ClientID
                        If fEmail Then sFocus = tbEmail.ClientID
                    End If
                    ' D2813 ===
                    tbEmailR.Attributes("onfocus") = "frm_mode(1);"
                    tbPasswordR.Attributes("onfocus") = "frm_mode(1);"
                    tbEmail.Attributes("onfocus") = "frm_mode(0);"
                    tbFullName.Attributes("onfocus") = "frm_mode(0);"
                    tbPhone.Attributes("onfocus") = "frm_mode(0);"  ' D4641
                    tbPassword.Attributes("onfocus") = "frm_mode(0);"
                    tbPassword2.Attributes("onfocus") = "frm_mode(0);"
                    ' D2525 + D2813 ==
                    ScriptManager.RegisterStartupScript(Page, GetType(String), "Init", String.Format("setTimeout('theForm.{0}.focus();', 250);", sFocus), True)
                    ScriptManager.RegisterOnSubmitStatement(Page, GetType(String), "Check", "return CheckForm();")
                    ' D1092 ==

                    ' D1512 ===
                    If CheckVar("f", False) Then
                        Dim fPassed = Not fPsw
                        If fPhone AndAlso fReqPhone Then fPassed = fPassed AndAlso tbPhone.Text.Trim <> "" ' D4641
                        If fName AndAlso fReqName Then fPassed = fPassed AndAlso tbFullName.Text.Trim <> ""
                        If fEmail AndAlso fReqEmail Then fPassed = fPassed AndAlso tbEmail.Text.Trim <> ""
                        If fPassed Then btnOK_Click(Me, Nothing)
                    End If
                    ' D1512 ==
                    ' D1059 ==
                    If sMode = "" AndAlso sReq = "" AndAlso fName AndAlso Not fReqName AndAlso fEmail AndAlso Not fReqEmail AndAlso fPsw AndAlso Not fReqPsw AndAlso WebOptions.AllowBlankPsw Then btnOK_Click(Me, Nothing) ' D4513
                End If
            End If
        End If
    End Sub

    Public Sub New()
        MyBase.New(_PGID_START_WITH_SIGNUP)
    End Sub

    ' D1059 ===
    Protected Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If e IsNot Nothing Then InitParams() ' D1512
        PerformUserSignupOrLogin(False) ' D2525
    End Sub
    ' D1059 ==

    ' D2525 ===
    Protected Sub btnOKR_Click(sender As Object, e As EventArgs) Handles btnOKR.Click
        If e IsNot Nothing Then InitParams()
        PerformUserSignupOrLogin(True)
    End Sub
    ' D2525 ==

    Public Sub PerformUserSignupOrLogin(frmLogin As Boolean)    ' D2525
        If sPasscode = "" Then Exit Sub

        ' D2525 ===
        If frmLogin Then
            fReqEmail = True
            fReqName = False
            fReqPsw = False
        End If
        ' D2525 ==

        ' D1071 + D2531 ===
        Dim sMsg As String = ""
        If fEmail AndAlso fReqEmail AndAlso ((Not frmLogin AndAlso tbEmail.Text.Trim = "") OrElse (frmLogin AndAlso tbEmailR.Text.Trim = "")) Then sMsg += String.Format(ResString("lblValidatorField"), ResString("lblEmail")) + "<br>" ' D2525
        If fName AndAlso tbFullName.Text.Trim = "" AndAlso fReqName Then sMsg += String.Format(ResString("lblValidatorField"), ResString("lblFullName")) + "<br>"
        If fPhone AndAlso tbPhone.Text.Trim = "" AndAlso fReqPhone Then sMsg += String.Format(ResString("lblValidatorField"), ResString("lblUserPhone")) + "<br>" ' D4641
        If tbPassword.Visible AndAlso tbPassword.Text.Trim = "" AndAlso (fReqPsw OrElse Not WebOptions.AllowBlankPsw) Then sMsg += String.Format(ResString("lblValidatorField"), ResString("lblPassword")) ' D1256 + D4495
        If sMsg <> "" Then sMsg = ResString("msgMustEnterFields") + sMsg
        ShowErrorMessage(sMsg)    ' D2531
        ' D1071 + D2531 ==
        ' D1092 ===
        If Not frmLogin AndAlso tbPassword2.Visible AndAlso tbPassword.Text <> tbPassword2.Text Then  ' D1256 + D2014 + D2525
            ShowErrorMessage(ResString("msgPasswordsMustBeEqual"))  ' D2531
            Exit Sub
        End If
        ' D1092 ==

        Dim sEmail As String = tbEmail.Text.Trim
        Dim sName As String = tbFullName.Text.Trim
        Dim sPhone As String = tbPhone.Text.Trim    ' D4641
        Dim sPsw As String = tbPassword.Text

        ' D2525 ===
        If frmLogin Then
            sEmail = tbEmailR.Text.Trim
            sPsw = tbPasswordR.Text.Trim
        End If
        ' D2525 ==

        Dim fDoTerminate As Boolean = False ' D2014

        If frmLogin OrElse (fEmail AndAlso sEmail <> "") Then ' D1071 + D2525
            Dim tUser As clsApplicationUser = App.DBUserByEmail(sEmail)
            If tUser IsNot Nothing Then

                ' D2536 ===
                If tUser.PasswordStatus >= _DEF_PASSWORD_ATTEMPTS OrElse tUser.Status = ecUserStatus.usDisabled Then
                    Dim sLink As String = String.Format("<a href='' class='actions dashed' onclick=""onPswRemind(); return false;"">{0}</a>", ResString("mnuForgottenPassword")) ' D6074
                    ShowErrorMessage(String.Format(ResString(If(_DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK, "msgUserLockedByWrongPswTimeout", "msgUserLockedByWrongPsw")), sLink))   ' D6074
                    'ShowErrorMessage(String.Format(ResString("msgUserLockedByWrongPsw"), ResString("mnuForgottenPassword")))    ' D4866
                    ' D6068 ===
                    If App.CanvasMasterDBVersion >= "0.9996" AndAlso App.Database.Connect AndAlso sEmail <> "" Then CheckUserLockWhenInvalidPassword(tUser)    ' D6072 + D6346
                    ' D6068 ==
                    fDoTerminate = True
                End If
                ' D2536 ==

                ' D2531 ===
                If Not fDoTerminate AndAlso Not frmLogin Then
                    ' D6552 ====
                    If isSSO() Then
                        ShowErrorMessage(String.Format(ResString("errUseSSO4Login"), PageURL(_PGID_START, "action=sso_start")))
                    Else
                        ShowErrorMessage(ResString("errUseRegisteredForm"))
                    End If
                    ' D6552 ==
                    fDoTerminate = True
                End If
                If Not fDoTerminate AndAlso tUser.UserPassword <> sPsw Then  ' D2525
                    ' D2431 ==
                    'ShowErrorMessage(ResString(IIf(sPsw <> "", "errStartWrongPsw", "errStartNeedPsw")))  ' D2014 + D2525 + D2531
                    ShowErrorMessage(ResString("msgWrongEmailOrPasswordStart"))  ' D2014 + D2525 + D2531

                    ' D6346 ===
                    Dim tError As ecAuthenticateError = If(CheckUserLockWhenInvalidPassword(tUser), ecAuthenticateError.aeUserLockedByWrongPsw, ecAuthenticateError.aeWrongPassword)    ' D6065
                    Dim sErrorMessage As String = ParseAllTemplates(App.GetMessageByAuthErrorCode(tError, sPasscode), tUser, Nothing)
                    App.DBSaveLogonEvent(tUser.UserEmail, tError, ecAuthenticateWay.awRegular, Request, sErrorMessage + " [SignupForm]")
                    If tError = ecAuthenticateError.aeUserLockedByWrongPsw Then ShowErrorMessage(sErrorMessage)
                    ' D6346 ==

                    fDoTerminate = True
                End If
                ' D1061 ===
                Dim tProject As clsProject = App.DBProjectByPasscode(sPasscode)
                If tProject IsNot Nothing AndAlso tUser.CannotBeDeleted Then ' D1781 + D1783 + D2014
                    ShowErrorMessage(String.Format(ResString("errStartOnlyParticipants"), tProject.Passcode))  ' D2531
                    fDoTerminate = True ' D2014
                End If
                ' D1061 ==
            Else
                ' D2525 ===
                If frmLogin Then
                    ShowErrorMessage(ResString("msgWrongEmailOrPasswordStart")) ' D2531 + D2541
                    fDoTerminate = True
                Else
                    If Not fPsw AndAlso _Anonymous_RandomPsw Then sPsw = GetRandomString(8, True, False)
                End If
                ' D2525 ==
            End If
        Else
            sEmail = String.Format(_Anonymous_Template, sPasscode.ToLower, GetRandomString(8, True, True).ToLower)
            If sName = "" Then sName = ResString("defAnonymousName")   ' D6715
            If Not fPsw AndAlso (_Anonymous_RandomPsw OrElse Not WebOptions.AllowBlankPsw) Then sPsw = GetRandomString(8, True, False) ' D6715
        End If

        If Not fDoTerminate Then    ' D2014
            SetCookie(_COOKIE_START_EMAIL, sEmail,, True)    ' D7148
            SetCookie(_COOKIE_START_PWD, sPsw,, True)        ' D7148
            Dim sParams As String = String.Format("{0}=1&{1}={2}&{3}={4}&{5}={6}&{7}={8}&{9}={10}&{11}={12}{13}{14}{15}", _PARAMS_SIGNUP(0), _PARAMS_EMAIL(0), HttpUtility.UrlEncode(sEmail), _PARAMS_USERNAME(0), HttpUtility.UrlEncode(sName), _PARAMS_PASSWORD(0), HttpUtility.UrlEncode(sPsw), _PARAMS_PASSCODE(0), HttpUtility.UrlEncode(sPasscode), _PARAM_ROLEGROUP, sRoleGoupID, _PARAM_WKG_ROLEGROUP, sWkgRoleGroupID, sJoinAsPM, If(sJoinAsPM = "", "&pipe=" + CheckVar("pipe", "1"), ""), If(fPhone, "&phone=" + HttpUtility.UrlEncode(sPhone), "")) ' D1189 + D1937 + D2287 + D4332 + D4627
            Dim sURL As String = PageURL(_PGID_START, "?" + _PARAMS_KEY(0) + "=" + EncodeURL(sParams, App.DatabaseID))
            Response.Redirect(sURL, True)
        End If
    End Sub

End Class