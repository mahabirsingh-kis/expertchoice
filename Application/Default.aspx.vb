Imports System.Web.Configuration

Partial Class LoginV6Page
    Inherits clsComparionCorePage

    Public sMessage As String = ""
    Public sAlert As String = ""
    Public sEmail As String = ""
    Public sPsw As String = ""
    Public sPswOrig As String = ""
    Public sAccessCode As String = ""
    Public sMeetingID As String = ""
    Public sName As String = ""
    Public fRemember As Boolean = False
    Public fDB_OK As Boolean = True
    Public fTTAvailable As Boolean = False

    Public CodeTimeout As Integer = _MFA_CODE_RESEND_TIMEOUT    ' D7504

    Public _ShowLogonForm As Boolean = True     ' D6550
    Public _ShowRegularLogon As Boolean = True  ' D6550
    Public _ShowMeetingForm As Boolean = True   ' D7438

    Private Const _OPT_SSO_MEETING_SHOW_FORM As Boolean = False ' D7445

    Public Sub New()
        MyBase.New(_PGID_START)
    End Sub

    Public Function GetURLParam() As String
        Dim sLst As String = ""
        For Each sName As String In Request.Params
            sName = RemoveXssFromParameter(sName)   ' D6767
            If Not String.IsNullOrEmpty(sName) AndAlso Array.IndexOf(_PARAMS_LOGON, sName.ToLower) >= 0 Then sLst += String.Format("{0}""{1}"":""{2}""", If(sLst = "", "", ", "), sName, HttpUtility.UrlEncode(Request.Params(sName)))
        Next
        Return sLst
    End Function

    Private Sub DefaultPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        isFirstRun = GetCookie("first_run", "") = ""    ' D6785
        If Not isCallback AndAlso Not IsPostBack AndAlso Not isAJAX Then
            SetCookie("first_run", "1", True, False)   ' D6785
        End If

        ' D6689 ===
        If Not ECSecurity.ECSecurity.FIPS_MODE Then
            Try
                Dim alg As New System.Security.Cryptography.RijndaelManaged
            Catch ex As Exception
                ShowError(ResString("errFIPSEnabled"), False)
                fDB_OK = False
            End Try
        End If
        ' D6689 ==

        If Not CheckVar("auto", True) OrElse isSSO_Only() Then   ' D7420
            DebugAutoLogon = False
            DebugRestoreLastPage = False
        End If

        Dim sAction As String = CheckVar(_PARAM_ACTION, "").ToLower
        Select Case sAction
            Case _ACTION_LOGOUT
                Session(_SESS_TT_Pipe) = Nothing
                Session(_SESS_TT_Passcode) = Nothing    ' D6789
                If App.isAuthorized Then
                    LogoutAndCheckReturnUser()
                End If
                SetCookie(_COOKIE_FEDRAMP_NOTIFICATION, "") ' D6644
                Response.Redirect(PageURL(_PGID_START), True)
                ' D6689 ===
            Case "fips"
                App = Nothing
                Session.Abandon()
                LoadComparionCoreOptions(App)
                ECSecurity.ECSecurity.FIPS_MODE = True
                Response.Redirect(PageURL(_PGID_START), True)
                ' D6689 ==
            ' D6442 ===
            Case "sso_start"    ' D5462
                ' D6532 ===
                Dim sError As String = ""
                OpenSSO(sError)
                If Not String.IsNullOrEmpty(sError) Then
                    ShowError(sError, False)
                End If
                ' D6532 ==
                ' D6550 ===
            Case "sso_success"
                ' D7424 ===
                Dim sOrigParams = CheckVar("orig_params", "")
                If Not String.IsNullOrEmpty(sOrigParams) Then sOrigParams = "orig_params=" + HttpUtility.UrlEncode(sOrigParams)
                Server.Transfer(PageURL(_PGID_SSO_ASSERT, sOrigParams), True)
                ' D6550 + D7424 ==
        End Select

        If sMessage = "" Then   ' D6689
            'If isAJAX() Then onAjaxRequest()   ' -D6075
            If _MFA_REQUIRED AndAlso App.isAuthorized AndAlso App.MFA_Requested AndAlso App.GetMFA_Mode(App.ActiveUser) = ecPinCodeType.mfaEmail Then   ' D7503
                Dim tCode As Tuple(Of Integer, Integer) = App.GetUserPin(ecPinCodeType.mfaEmail, App.ActiveUser.UserID, App.ProjectID, , False) ' D7503
                ' D7504 ===
                CodeTimeout = App.GetUserPinTimeout(tCode)
                If CodeTimeout > (_DEF_MFA_EMAIL_TIMEOUT - 3) Then    ' in case when pin has been changed a monent ago (refreshed): send e-mail with code
                    SendMFA_Email(App.ActiveUser, tCode.Item1.ToString, Nothing)
                End If
                ' D7504 ==
            Else
                ' D4871 ===
                If App.isAuthorized AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso Not IsPostBack AndAlso Not isCallback AndAlso Not isAJAX AndAlso Request.QueryString.Count = 0 Then   ' D4925
                    Dim pgID As Integer = _PGID_PROJECTSLIST
                    If App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem Then
                        pgID = _PGID_ADMIN_WORKGROUPS
                    Else
                        If App.HasActiveProject AndAlso Not App.isEvalURL_EvalSite AndAlso (App.ActiveProjectsList.Count = 1 OrElse App.Options.isSingleModeEvaluation) Then pgID = _PGID_EVALUATION    ' D6359
                        ' D4955 ===
                        Dim sMSg As String = ""
                        If CurrentPageID <> _PGID_ERROR_503 AndAlso Not App.CheckLicense(App.ActiveWorkgroup, sMSg, True) Then
                            If Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then FetchAccessByWrongLicense(sMSg, True)
                        End If
                        ' D4955 ==
                    End If
                    ' D7675 ===
                    Try
                        Server.Transfer(PageURL(pgID,, True), True)    ' D6359 + D7413
                    Catch ex As Exception
                        Response.Redirect(PageURL(pgID,, True), True)
                    End Try
                    ' D7675 ==
                End If
            End If
            ' D4871 ==
        End If

        InitLogonForm()
        SetPageTitle(ParseString(_TEMPL_APPNAME))
        _ShowRegularLogon = Not isSSO() OrElse Not isSSO_Only() OrElse (Not fDB_OK) OrElse CheckVar("mode", "") = "regular" OrElse Request.IsLocal ' D6550 + D6552
        _ShowMeetingForm = Not isSSO_Only() OrElse (_OPT_SSO_MEETING_SHOW_FORM AndAlso (App.isTeamTimeAvailable AndAlso ParamByName(Request.Params, _PARAMS_MEETING_ID) <> "")) ' D7438 + D7445
        _ShowLogonForm = _ShowRegularLogon OrElse _ShowMeetingForm OrElse Not isFirstRun   ' D6550 + D7438
        ' D6532 ===
        If isSSO() AndAlso isSSO_Only() AndAlso Not _ShowLogonForm AndAlso fDB_OK Then  ' D6550
            If CurrentPageID = _PGID_ERROR_403 OrElse (String.IsNullOrEmpty(sAlert) AndAlso String.IsNullOrEmpty(sMessage) AndAlso CurrentPageID = _PGID_START AndAlso App.ApplicationError.Status = ecErrorStatus.errNone) Then  ' D6550
                Dim sError As String = ""
                OpenSSO(sError)
                If Not String.IsNullOrEmpty(sError) Then
                    ShowError(sError, False)
                End If
            End If
        End If
        ' D6532 ==
    End Sub

    Private Sub ShowError(sError As String, fShowContactInfo As Boolean, Optional sHint As String = "") ' D6550
        Dim sMsg As String = sError.Replace(vbNewLine, " <br>") ' D6550
        If fShowContactInfo AndAlso sMsg <> "" AndAlso sEmail.ToLower <> _DB_DEFAULT_ADMIN_LOGIN.ToLower Then sMsg += "<br>" + ResString("msgAuthContact")  ' D6446
        If sMsg <> "" Then sMsg = "<b class='error_dark'>" + sMsg + "</b>"
        sAlert += sMsg
    End Sub

    Private Sub InitLogonForm()
        Dim fMasterExists As Boolean = App.isCanvasMasterDBValid
        Dim fProjectsExists As Boolean = App.isCanvasProjectsDBValid
        fDB_OK = fDB_OK AndAlso fMasterExists AndAlso fProjectsExists   ' D6689

        If Not Page.IsPostBack Then sEmail = RemoveHTMLTags(EcSanitizer.GetSafeHtmlFragment(GetCookie(_COOKIE_EMAIL)))  ' D6446 + D7148

        If fDB_OK AndAlso sMessage = "" AndAlso App.Options.CheckLicense AndAlso App.ApplicationError.Status = ecErrorStatus.errNone Then
            Dim sLicError As String = ""
            If Not App.CheckLicense(App.SystemWorkgroup, sLicError, True) Then ShowError(String.Format("{0} [System Workgroup]", sLicError), True) ' D0811 + D6446
            If sLicError = "" AndAlso App.SystemWorkgroup IsNot Nothing AndAlso App.SystemWorkgroup.License.GetParameterMaxByID(ecLicenseParameter.MaxWorkgroupsTotal) > 0 AndAlso Not App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxWorkgroupsTotal) Then ShowError(App.LicenseErrorMessage(App.SystemWorkgroup.License, ecLicenseParameter.MaxWorkgroupsTotal), True) ' D4982 + D4984
        End If
        ' D0262 ==

        If sMessage = "" Then   ' D6689
            Select Case App.ApplicationError.Status
                Case ecErrorStatus.errAccessDenied
                    ShowError403()  ' D6645

                Case ecErrorStatus.errWrongLicense
                    If App.ApplicationError.Message <> "" Then ShowError(App.ApplicationError.Message, False) Else Response.Redirect(PageURL(_PGID_ERROR_503), True)

                ' D0389 ===
                        'Case ecAuthenticateError.aeProjectLocked
                        '    lblWhyICantLogon.Visible = True

                Case ecErrorStatus.errMessage
                    CurrentPageID = _PGID_START
                    ShowError(App.ApplicationError.Message, False)
                    App.ApplicationError.Reset()
                    ' D0389 ==

                Case Else
                    CurrentPageID = _PGID_START   ' D0045

            End Select
        End If

        If fDB_OK Then

        Else    ' DB can't be read properly of DB is outdated

            Dim sErrorDB As String = ""
            If Not fProjectsExists Then sErrorDB = App.Options.CanvasProjectsDBName
            If Not fMasterExists Then sErrorDB = App.Options.CanvasMasterDBName
            If sErrorDB <> "" Then  ' D6689
                sMessage = String.Format(ResString("msgErrorDBConnection"), sErrorDB)
                sMessage = String.Format("<div title='{2}' class='error'><i class='fa fa-exclamation-circle' style='float:left; margin-right:6px; font-size:1.5em; color:#bf2b1b;'></i><div style='display:inline-block; padding-top:3px;'><b>{1}</b></div></div>", SafeFormString(ResString("lblError")), sMessage, SafeFormString(App.Database.LastError))
                App.ResetDBChecks()
            End If

        End If

        If Not Page.IsPostBack Then
            sMeetingID = RemoveHTMLTags(Uri.UnescapeDataString(GetCookie(_COOKIE_MEETING_ID)))  ' D4920 + D7148
            sName = RemoveHTMLTags(Uri.UnescapeDataString(GetCookie(_COOKIE_MEETING_LOGIN)))    ' D4920 + D7148
            sPsw = If(isSSO(), "", GetCookie(_COOKIE_PASSWORD))   ' D6552
            If sName = sEmail Then sName = "" ' D6651
            fRemember = If(isSSO(), False, GetCookie(_COOKIE_REMEMBER, "1") = "1")   ' D6552

            If fRemember AndAlso sEmail <> "" Then
                sPswOrig = EncodeURL(sPsw, App.Options.SessionID)
                sPsw = EcSanitizer.GetSafeHtmlFragment(_DEF_PASSWORD_FAKE)
            End If

            If DebugDisableAutoComplete4Logon Then
                sEmail = ""
                sPsw = ""
                sPswOrig = ""
                fRemember = False
            End If
            sAccessCode = ""    ' SafeFormString(GetCookie(_COOKIE_PASSCODE, ""))   ' D6287
            Dim sPasscodeURI As String = RemoveHTMLTags(EcSanitizer.GetSafeHtmlFragment(ParamByName(Request.Params, _PARAMS_PASSCODE))) ' D7148
            If sPasscodeURI <> "" Then sAccessCode = SafeFormString(sPasscodeURI)
            Dim sMID As String = RemoveHTMLTags(EcSanitizer.GetSafeHtmlFragment(ParamByName(Request.Params, _PARAMS_MEETING_ID)))   ' D7148
            If sMID <> "" Then sMeetingID = sMID

            If fDB_OK Then
                Dim sDBVersion As String = App.CanvasMasterDBVersion
                If sDBVersion = "" Then sDBVersion = "?"
                If sDBVersion = "?" OrElse sDBVersion < _DB_MINVER_MASTERDB OrElse sDBVersion = _DB_VERSION_UNKNOWN Then
                    If Request.IsLocal OrElse sEmail.ToLower = _DB_DEFAULT_ADMIN_LOGIN.ToLower Then ShowError(String.Format(ResString("msgWrongDBVersion"), sDBVersion, _DB_MINVER_MASTERDB) + "<br>" + vbCrLf, True)
                End If
            End If

            If Not Infodoc_Prepare(0, 0, reObjectType.Unspecified, "test") Then ShowError(ResString("errAccessDeniedDocMedia"), True)

            fTTAvailable = App.SystemWorkgroup IsNot Nothing AndAlso App.SystemWorkgroup.License IsNot Nothing AndAlso App.SystemWorkgroup.License.isValidLicense AndAlso App.SystemWorkgroup.License.CheckParameterByID(ecLicenseParameter.TeamTimeEnabled, Nothing, True)

            ' D6446 ===
            If sMessage = "" AndAlso sAlert <> "" AndAlso sEmail.ToLower = _DB_DEFAULT_ADMIN_LOGIN.ToLower Then
                sMessage = sAlert
                sAlert = ""
            End If
            ' D6446 ==

            Dim sFName As String = _FILE_ROOT + "default.txt"
            If IO.File.Exists(sFName) Then
                Dim sContent As String = File_GetContent(sFName)
                If sContent <> "" AndAlso HTML2TextWithSafeTags(sContent).Replace(vbCr, "").Replace(vbLf, "").Trim <> "" Then sMessage += sContent
            End If

            Dim tExtra As clsExtra = App.DBExtraRead(clsExtra.Params2Extra(-1, ecExtraType.Common, ecExtraProperty.Message))
            If tExtra IsNot Nothing AndAlso tExtra.Value IsNot Nothing AndAlso CStr(tExtra.Value).Trim <> "" Then
                If sMessage <> "" Then sMessage += String.Format("<div style='padding:6px'><div class='warning'>{0}</div></div>", tExtra.Value) Else sMessage = CStr(tExtra.Value)
            End If

            If sMessage = "" AndAlso sAlert = "" AndAlso CheckVar("error", "") = "403" Then ShowError403()  ' D6645
        End If
    End Sub

    ' D6645 ===
    Private Sub ShowError403()
        'App.ResetError()
        App.ApplicationError.Status = ecErrorStatus.errNone
        CurrentPageID = _PGID_ERROR_403
        ShowError(CStr(IIf(App.ApplicationError.Message = "", ResString("msgRestricted"), App.ApplicationError.Message)), False)
        ' D6640 ===
        If _FEDRAMP_MODE Then
            SetCookie(_COOKIE_FEDRAMP_NOTIFICATION, "")
            Response.Redirect(PageURL(_PGID_FEDRAMP_NOTIFICATION), True)
        End If
        ' D6640 ==
    End Sub
    ' D6645 ==
End Class