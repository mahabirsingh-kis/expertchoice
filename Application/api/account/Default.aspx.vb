''' <summary>
''' User account functionality
''' </summary>
''' <remarks>There are listed the functions related to the user account which is logging to the system.</remarks>
Partial Class AccountWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    ' D7223 ===
    ''' <summary>
    ''' This is a common function for perform user authentication
    ''' <remark>
    ''' There are available a different login ways, which can be used to start a session for a specific user/credentials. You can force specific method with passing 'mode' [int] parameter (optional <seealso cref="ecAuthenticateWay"/> parameter, since the system can recognize the most cases, especially when hash parameter used).
    ''' Logout can be performed via <see cref="AccountWebAPI.Logout()"/> method.
    ''' </remark>
    ''' <example>?action=logon&amp;email=user&amp;password=psw&amp;params={""rememberme":1}, ?action=logon&amp;params={"hash":"1234567890abcdefgh..."}</example>
    ''' </summary>
    ''' <example>Login by plain credentials
    ''' <code>?action=logon&amp;email=user&amp;password=psw&amp;params={""rememberme":1}</code>
    ''' </example>
    ''' <example>Login by hash code
    ''' <code>{"hash":"1234567890abcdefgh..."}</code>
    ''' </example>
    ''' <param name="email">Unique user e-mail</param>
    ''' <param name="password">User password</param>
    ''' <param name="passcode">Access code (if exists/required) for specify model</param>
    ''' <param name="meetingID">The MeetingID (if required) for join to TeamTime or Brainstorming session</param>
    ''' <param name="hash">The unique hash code that encrypt he user/model/extra credentials</param>
    ''' <param name="params">Required (the listed above) or any custom parameters, which can be used during the autorization. List as a common named array {"name":"value"}</param>
    ''' <returns>It returns the jActionResult with a code of ecAuthenticateError and user/project data in case of success or an information message in case of error.
    ''' <example>{"Result" 2,  "ObjectID": -1,  "Message": "",  "URL": "",  "Data": "aeUnknown",  "Tag": null}</example>
    ''' </returns>
    Public Function Logon(Optional email As String = "", Optional password As String = "", Optional passcode As String = "", Optional meetingID As String = "", Optional hash As String = "", Optional params As NameValueCollection = Nothing) As jActionResult
        If params Is Nothing Then params = New NameValueCollection()
        If Not String.IsNullOrEmpty(email) AndAlso ParamByName(params, _PARAMS_EMAIL) = "" Then params.Add(_PARAM_EMAIL, email)
        If Not String.IsNullOrEmpty(password) AndAlso ParamByName(params, _PARAMS_PASSWORD) = "" Then params.Add(_PARAM_PASSWORD, password)
        If Not String.IsNullOrEmpty(passcode) AndAlso ParamByName(params, _PARAMS_PASSCODE) = "" Then params.Add(_PARAM_PASSCODE, passcode)
        If Not String.IsNullOrEmpty(meetingID) AndAlso ParamByName(params, _PARAMS_MEETING_ID) = "" Then params.Add(_PARAM_MEETING_ID, meetingID)
        If Not String.IsNullOrEmpty(hash) AndAlso ParamByName(params, {"hash"}) = "" Then params.Add("hash", hash)
        ' D7223 ==
        If ParamByName(params, _PARAMS_PASSWORD) = _DEF_PASSWORD_FAKE Then
            Dim sOrigPsw As String = GetParam(params, "po", True)
            If sOrigPsw <> "" Then sOrigPsw = DecodeURL(sOrigPsw, App.Options.SessionID)
            params(_PARAM_PASSWORD) = sOrigPsw
        End If
        Dim AuthMode As ecAuthenticateWay = ecAuthenticateWay.awRegular
        Select Case GetParam(params, "mode", True)
            Case ecAuthenticateWay.awJoinMeeting.ToString, CInt(ecAuthenticateWay.awJoinMeeting).ToString
                AuthMode = ecAuthenticateWay.awJoinMeeting
            Case ecAuthenticateWay.awCredentials.ToString, CInt(ecAuthenticateWay.awCredentials).ToString
                AuthMode = ecAuthenticateWay.awCredentials
            Case ecAuthenticateWay.awTokenizedURL.ToString, CInt(ecAuthenticateWay.awTokenizedURL).ToString
                AuthMode = ecAuthenticateWay.awTokenizedURL
            Case Else
                AuthMode = ecAuthenticateWay.awRegular
        End Select
        Dim Res As New jActionResult
        ' D7503 ===
        Dim AuthRes As ecAuthenticateError = Authenticate(params, Res.Message, Res.URL, AuthMode, False)    ' D6532
        Res.Data = AuthRes.ToString
        ' D6067 ===
        Select Case AuthRes
            Case ecAuthenticateError.aeNoErrors
                ' D7503 ==
                Res.Result = ecActionResult.arSuccess
                Res.Tag = jSessionStatus.CreateFromBaseObject(App, Session)  ' D7198 + D7209
                ' D7503 ===
            Case Else
                If AuthRes = ecAuthenticateError.aeMFA_Required AndAlso App.isAuthorized AndAlso App.MFA_Requested Then
                    Res.Result = ecActionResult.arWarning
                    Res.Message = App.GetMessageByAuthErrorCode(AuthRes)
                    Dim tCode As Tuple(Of Integer, Integer) = App.GetUserPin(ecPinCodeType.mfaEmail, App.ActiveUser.UserID, App.ProjectID,, False)
                    Res.Tag = App.GetUserPinTimeout(tCode)    ' D7504
                    SessVar("ReturnURL") = Res.URL      ' D7504
                Else
                    Res.Result = ecActionResult.arError
                    If String.IsNullOrEmpty(Res.Message) Then Res.Message = App.GetMessageByAuthErrorCode(AuthRes)
                    Thread.Sleep(1000)  ' D5026
                End If
                ' D7503 ==
        End Select
        ' D6067 ==
        Return Res
    End Function

    ''' <summary>
    ''' Perform user logout
    ''' </summary>
    ''' <returns>Returns jActionResult with success code and URL as start page</returns>
    Public Function Logout() As jActionResult
        UserLogout()    ' D5027
        Return New jActionResult With {.Result = ecActionResult.arSuccess, .URL = PageURL(_PGID_START)}
    End Function

    ' D7187 ===
    Public Function ConnectWithPin(pin As Integer, Optional PinType As ecPinCodeType = ecPinCodeType.mfaAlexa) As jActionResult    ' D7501 + D7502
        Dim tRes As New jActionResult

        Dim tUser As clsApplicationUser = Nothing
        Dim tPrj As clsProject = Nothing
        Dim sExtra As String = ""

        App.GetUserByPin(PinType, pin, tUser, tPrj, sExtra) ' D7501 + D7502

        If tUser IsNot Nothing Then
            tRes.Result = ecActionResult.arSuccess
            tRes.Data = jAppUserShort.CreateFromBaseObject(tUser)
            tRes.ObjectID = If(tPrj Is Nothing, -1, tPrj.ID)
            tRes.Tag = sExtra
        Else
            tRes.Result = ecActionResult.arError
            tRes.Message = "PIN code is invalid or expired"
        End If

        Return tRes
    End Function
    ' D7187 ==

    ''' <summary>
    ''' Accept the EULA for current user
    ''' <remarks>User must be logged. Active workgroup is not required. Saved in the DB the EULA name and revision (for masterDB v. 0.0092+). Also pushed the row to the global Logs with user IP.</remarks>
    ''' </summary>
    ''' <param name="EULA">EULA file name, must be non-empty</param>
    ''' <param name="Version">EULA revision, usually came from _EULA_REVISION (like "200128")</param>
    ''' <returns>Returns jActionResult with success/error code and optional message</returns>
    Public Function EULA_Accept(EULA As String, Version As String) As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult With {.Result = ecActionResult.arError, .Message = "No valid EULA data"}
        If EULA <> "" Then
            Dim DBParams As New List(Of Object)
            DBParams.Add(Version)
            DBParams.Add(App.ActiveUser.UserID)
            If App.CanvasMasterDBVersion >= "0.992" AndAlso Version <> _EULA_REVISION AndAlso App.ActiveWorkgroup IsNot Nothing Then
                DBParams.Add(App.ActiveWorkgroup.ID)
                App.Database.ExecuteSQL(String.Format("UPDATE {0} SET EULAversion=? WHERE {1}=? AND {2}=?", clsComparionCore._TABLE_USERWORKGROUPS, clsComparionCore._FLD_USERWRKG_USERID, clsComparionCore._FLD_USERWRKG_WRKGID), DBParams)
                Res.ObjectID = App.ActiveWorkgroup.ID
            Else
                App.Database.ExecuteSQL(String.Format("UPDATE {0} SET EULAversion=? WHERE {1}=?", clsComparionCore._TABLE_USERS, clsComparionCore._FLD_USERS_ID), DBParams)
                Res.ObjectID = -1
            End If
            Dim sComment As String = ""
            If Request IsNot Nothing Then sComment = String.Format("IP: {0}", Request.UserHostAddress)
            App.DBSaveLog(dbActionType.actAcceptEULA, dbObjectType.einfUser, App.ActiveUser.UserID, EULA, "")
            App._EULA_checked = False
            Res.Message = ""
            Res.Result = ecActionResult.arSuccess
        End If
        Return Res
    End Function

    ' D5033 ===
    ''' <summary>
    ''' Set user option value by name
    ''' <remarks>use this method for set some account/UI settings, specific for the active user (must be logged).</remarks>
    ''' </summary>
    ''' <param name="Name">UI or Account option name ('show_splash', 'advancedmode', 'autologon', 'user_name', etc).</param>
    ''' <param name="Value">Option value as string</param>
    ''' <returns>Returns jActionResult with success/error code and optional message</returns>
    Public Function [Option](Name As String, Value As String) As jActionResult
        FetchIfNotAuthorized()
        Dim Res As New jActionResult
        Dim sError As String = ""
        Select Case Name.ToString.ToLower

            Case "show_splash"
                PMShowInstruction(App, App.ActiveUser.UserID) = CBool(Value)
                Res.Data = PMShowInstruction(App, App.ActiveUser.UserID)

                ' D6034 ===
            Case "show_issues"
                ShowKnownIssues(App) = Str2Bool(Value)
                ' D6034 ==

                ' D5083 ===
            Case "advancedmode"
                isAdvancedMode = Str2Bool(Value)
                Res.Data = isAdvancedMode
                ' D5083 ==

                ' D6027 ===
            Case "showlanding"
                ShowLandingPages(App, App.ActiveUser.UserID) = Str2Bool(Value)
                ' D6027 ==

                ' D6117 ===
            Case "autologon"
                DebugAutoLogon = Str2Bool(Value)

            Case "restorelastpage"
                DebugRestoreLastPage = Str2Bool(Value)

            Case "autocomplete"
                DebugDisableAutoComplete4Logon = Str2Bool(Value)

            Case "reviewaccount"
                App.ReviewAccountEnabled(ReviewAccount() <> "") = Str2Bool(Value)

            Case "wipeout_timeout"
                Dim D As Integer
                If Integer.TryParse(Value, D) Then App.WipeoutProjectsTimeout = D

            Case "user_name"
                Dim sOld As String = App.ActiveUser.UserName
                Value = Value.Trim
                If sOld <> Value Then
                    App.ActiveUser.UserName = Value
                    App.DBUserUpdate(App.ActiveUser, False, String.Format("Update User Name ('{0}' > '{1}')", sOld, Value))
                Else
                    sError = ResString("msgNoChanges")  ' D6446
                End If

            Case "user_password"
                If isSSO() Then
                    _Page.FetchNotAllowedSSO()    ' D6552
                Else
                    Dim sID As String = GetParam(_Page.Params, "id", True)
                    Dim sOld As String = GetParam(_Page.Params, "old", True)
                Dim fKeepHashes As Boolean = Str2Bool(GetParam(_Page.Params, "keep_hashes", True))   ' D7719
                    If sID <> App.Options.SessionID Then
                        sError = ResString("msgWrongAPIParams") ' D6446
                    Else
                        If sOld = App.ActiveUser.UserPassword Then
                        ' D6659 ===
                        Dim sOldPsw As String = App.ActiveUser.UserPassword ' D7719
                        ' D7719 ===
                        If App.SetUserPassword(App.ActiveUser, Value, fKeepHashes, AllowBlankPsw, sError) Then
                            If fKeepHashes Then
                                TinyURLUpdateUserPsw(App.ActiveUser.UserID, sOldPsw, App.ActiveUser.UserPassword)
                            End If
                            ' D7719 ==
                            'Dim tDTSess As DateTime? = App.CheckSessionTerminate(App.ActiveUser)   ' D7357
                            'If tDTSess.HasValue Then SessVar(_SESS_CMD_TERMINATE) = Date2ULong(tDTSess.Value).ToString    ' D7357
                            End If
                        ' D6659 ==
                    Else
                            sError = ResString("msgWrongCurPsw")    ' D6446
                        End If
                    End If
                End If
                ' D6117 ==

            Case Else
                sError = String.Format("Unknown option '{0}'", RemoveHTMLTags(Name))
        End Select

        If sError = "" Then
            Res.Result = ecActionResult.arSuccess
        Else
            Res.Result = ecActionResult.arError
            Res.Message = sError
        End If
        Return Res
    End Function
    ' D5033 ==

    ' D5062 ===
    ''' <summary>
    ''' Get pages statistic information
    ''' </summary>
    ''' <returns>Html string containing information about counts of Total pages, html5 enabled pages, drafts and missing pages</returns>
    Public Function Pages_Statistic() As jActionResult
        If Not App.isAuthorized OrElse Not App.CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) Then FetchNoPermissions()
        Dim sPages As String = _Page.GetPagesStatistic
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = sPages
        }
    End Function
    ' D5062 ==

    ' D5039 ===
    Public Function Allowed_Pages(Layout As String, Optional GetNext As Boolean = False) As jActionResult    ' D6758
        If _Page._LAYOUTS_ALLOWED.Length > 1 Then
            Dim Idx As Integer = _Page._LAYOUTS_ALLOWED.ToList().IndexOf(Layout)
            If Idx < 0 Then Idx = 0
            If GetNext Then ' D6758
                If Idx >= _Page._LAYOUTS_ALLOWED.Length - 1 Then Idx = 0 Else Idx += 1
            Else
                If Idx > _Page._LAYOUTS_ALLOWED.Length - 1 Then Idx = 0 ' D6758
            End If
            _Page.LayoutName() = _Page._LAYOUTS_ALLOWED(Idx)
        End If
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Tag = _Page.LayoutName(),
            .Data = String.Format("[{0}]", _Page.GetSitemapJSON())
            }
    End Function
    ' D5039 ==

    ' D6075 ===
    ''' <summary>
    ''' Send Password reminder email
    ''' </summary>
    ''' <param name="Email">user email</param>
    ''' <returns></returns>
    Public Function PasswordRemind(Email As String) As jActionResult
        Dim fRes As New jActionResult
        fRes.Result = ecActionResult.arError
        If Email <> "" Then
            Dim tUser As clsApplicationUser = App.DBUserByEmail(Email)
            If tUser Is Nothing Then
                'fRes.Message = String.Format(ResString("errUserEmailNotExists"), Email)
                fRes.Message = ResString("msgOnPswReminder")    ' D7158
                fRes.Result = ecActionResult.arSuccess  ' D7182 // for avoid show "Error" in popup header
            Else
                Dim sBody As String = ParseAllTemplates(App.GetPswReminderBodyText(ResString("bodyReminder", False, False), False, False), tUser, Nothing)
                If tUser.CannotBeDeleted OrElse Not sBody.ToLower.Contains("://") Then
                    fRes.Message = String.Format(ResString("errResetPswNoAllowed"), Email)
                Else
                    Dim sError As String = ""
                    Dim fSent As Boolean = SendMail(SystemEmail, Email, ParseAllTemplates(ResString("subjReminder", False, False), tUser, Nothing), sBody, sError, "", False, SMTPSSL)
                    App.DBSaveLog(dbActionType.actSendEmail, dbObjectType.einfUser, tUser.UserID, "Request for forgotten password", sError)
                    App.DBExtraWrite(clsExtra.Params2Extra(tUser.UserID, ecExtraType.User, ecExtraProperty.UserSessionTerminate, Date2ULong(Now).ToString())) ' D7356
                    'fRes.Message = String.Format(ResString(CType(IIf(fSent, "msgReminderOK", "msgReminderError"), String)), Email, SystemEmail)
                    fRes.Message = ResString("msgOnPswReminder")    ' D7158
                    'If fSent Then fRes.Result = ecActionResult.arSuccess
                    fRes.Result = ecActionResult.arSuccess  ' D7182 // Force to show "Information" for any case
                End If
            End If
        Else
            fRes.Message = ResString("msgEmptyEmail")
        End If
        Return fRes
    End Function
    ' D6075 ==

    ' D7544 ===
    Public Function Language(Value As String) As jActionResult
        Dim Res As New jActionResult
        If Value <> "" AndAlso Value.ToLower <> App.LanguageCode.ToLower Then
            If SetLanguage(Value) Then
                App.ResetLanguages()
            End If
        End If
        Res.Data = App.LanguageCode
        Res.Result = ecActionResult.arSuccess
        Return Res
    End Function
    ' D7544 ==

    ' D7504 ===
    Public Function MFASendCode() As jActionResult
        FetchIfNotAuthorized()
        Dim tRes As New jActionResult With {.Result = ecActionResult.arError, .Message = "MFA is not available"}
        If _MFA_REQUIRED AndAlso App.MFA_Requested Then
            Dim PinType As ecPinCodeType = App.GetMFA_Mode(App.ActiveUser)
            Select Case PinType
                Case ecPinCodeType.mfaEmail
                    Dim tCode As Tuple(Of Integer, Integer) = App.GetUserPin(PinType, App.ActiveUser.UserID, App.ProjectID, "", False)
                    Dim PinTimeout As Integer = App.GetUserPinTimeout(tCode)
                    If PinTimeout + _MFA_CODE_RESEND_TIMEOUT > _DEF_MFA_EMAIL_TIMEOUT Then
                        tRes.Message = "Timeout for getting new code is not reached yet"
                    Else
                        tCode = App.GetUserPin(PinType, App.ActiveUser.UserID, App.ProjectID, "", True)
                        tRes.Result = If(tCode IsNot Nothing AndAlso SendMFA_Code(PinType, App.ActiveUser, tRes.Message), ecActionResult.arSuccess, ecActionResult.arError)
                        PinTimeout = App.GetUserPinTimeout(tCode)
                    End If
                    If tCode IsNot Nothing Then tRes.Tag = PinTimeout
            End Select
        End If
        Return tRes
    End Function

    Public Function MFACheckCode(Code As String) As jActionResult
        FetchIfNotAuthorized()
        Dim tRes As New jActionResult With {.Result = ecActionResult.arError, .Message = "MFA is not available"}
        If _MFA_REQUIRED AndAlso App.MFA_Requested Then
            tRes.Message = ResString("errMFAWrongCode")
            Dim PinType As ecPinCodeType = App.GetMFA_Mode(App.ActiveUser)
            Select Case PinType
                Case ecPinCodeType.mfaEmail
                    Dim tCode As Tuple(Of Integer, Integer) = App.GetUserPin(PinType, App.ActiveUser.UserID, App.ProjectID, "", False)
                    If tCode IsNot Nothing AndAlso Not String.IsNullOrEmpty(Code) Then
                        tRes.Tag = tCode.Item2
                        If tCode.Item1.ToString <> Code Then
                            If _DEF_MFA_EMAIL_TIMEOUT - tCode.Item2 < 5 Then
                                SendMFA_Code(PinType, App.ActiveUser, tRes.Message)
                                tRes.Message = ResString("errMFACodeExpired")
                            End If
                            Thread.Sleep(1000)  ' D7508
                        Else
                            App.DeleteUserPin(PinType, App.ActiveUser.UserID)
                            App.MFA_Requested = False
                            tRes.Result = ecActionResult.arSuccess
                            tRes.Message = ""
                            tRes.URL = SessVar("ReturnURL")
                            If String.IsNullOrEmpty(tRes.URL) Then tRes.URL = GetDefaultPath(True)
                        End If
                    End If
            End Select
        End If
        Return tRes
    End Function
    ' D7504 ==

    Private Sub AuthWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        Select Case _Page.Action

            Case "logon"
                ' D6030 + D7223 ===
                Dim Params As NameValueCollection = _Page.Params
                For Each sName As String In Request.Params
                    If Params(sName) Is Nothing Then Params.Add(sName, Request(sName))
                Next
                _Page.ResponseData = Logon(GetParam(_Page.Params, _PARAM_EMAIL, True), GetParam(_Page.Params, _PARAM_PASSWORD, True), GetParam(_Page.Params, _PARAM_PASSCODE, True), GetParam(_Page.Params, _PARAM_MEETING_ID, True), GetParam(_Page.Params, "hash", True), Params)
                ' D6030 + D7223 ==

            Case "logout"
                _Page.ResponseData = Logout()

                ' D7187 ===
            Case "connectwithpin"
                Dim pin As Integer = 0
                Integer.TryParse(GetParam(_Page.Params, "pin", True), pin)
                ' D7502 ===
                Dim pinType As Integer = 0
                Integer.TryParse(GetParam(_Page.Params, "type", True), pin)
                Select Case pinType
                    Case CInt(ecPinCodeType.mfaEmail), CInt(ecPinCodeType.mfaAlexa)
                    Case Else
                        pinType = CInt(ecPinCodeType.mfaAlexa)
                End Select
                _Page.ResponseData = ConnectWithPin(pin, CType(pinType, ecPinCodeType))
                ' D7187 + D7502 ==

            Case "eula_accept"
                ' D5033 ===
                _Page.ResponseData = EULA_Accept(GetParam(_Page.Params, "EULA", True), GetParam(_Page.Params, "Version", True))

            Case "option"
                _Page.ResponseData = [Option](GetParam(_Page.Params, "name", True), GetParam(_Page.Params, "value", True))
                ' D5033 ==

                ' D5039 ===
            Case "allowed_pages"
                _Page.ResponseData = Allowed_Pages(GetParam(_Page.Params, "layout", True).ToLower, Str2Bool(GetParam(_Page.Params, "getnext", True)))   ' D6758
                ' D5039 ==

                ' D5062 ===
            Case "pages_statistic"
                _Page.ResponseData = Pages_Statistic()
                ' D5062 ==

                ' D6075 ===
            Case "passwordremind"
                _Page.ResponseData = PasswordRemind(GetParam(_Page.Params, "email", True))
                ' D6075 ==

            Case "language" ' D7544
                _Page.ResponseData = Language(GetParam(_Page.Params, "value", True))    ' D7544

                ' D7504 ===
            Case "mfasendcode"
                _Page.ResponseData = MFASendCode()

            Case "mfacheckcode"
                _Page.ResponseData = MFACheckCode(GetParam(_Page.Params, "code", True))
                ' D7504 ==

        End Select
    End Sub

End Class