
Partial Class CreatePasswordPage
    Inherits clsComparionCorePage

    Private _tUser As clsApplicationUser = Nothing

    Public isFormEnabled As Boolean = False
    Public byToken As Boolean = False

    Public Sub New()
        MyBase.New(_PGID_CREATE_PASSWORD)
    End Sub

    Public ReadOnly Property CurrentUser() As clsApplicationUser    ' D6454
        Get
            If _tUser Is Nothing Then
                If App.ActiveUser IsNot Nothing Then _tUser = App.ActiveUser
            End If
            Return _tUser
        End Get
    End Property

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        If isSSO() Then FetchAccess() ' D6552
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        ShowNavigation = False  ' D6322
        ShowTopNavigation = False   ' D6322
        CheckParams()
        If isAJAX Then onAJAX()
    End Sub

    Private Sub CheckParams()
        Dim sError As String = ""
        Dim sKey As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(Request.Params, _PARAMS_TINYURL)).Trim   ' Anti-XSS
        If sKey <> "" Then
            sKey = App.DecodeTinyURL(sKey)
            If Not String.IsNullOrEmpty(sKey) Then
                sKey = DecodeURL(sKey, App.DatabaseID)
                If Not String.IsNullOrEmpty(sKey) Then
                    Dim tParams As NameValueCollection = HttpUtility.ParseQueryString(sKey)
                    If tParams(_PARAM_ACTION) = "resetpsw" Then
                        Dim ue As String = tParams("ue")
                        Dim up As String = tParams("up")
                        If ue <> "" Then
                            Dim tUser As clsApplicationUser = App.DBUserByEmail(ue)
                            If tUser IsNot Nothing AndAlso tUser.UserPassword = up Then
                                If App.CanvasMasterDBVersion >= "0.99" Then
                                    Dim SQL As String = "SELECT Created FROM PrivateURLs WHERE hash = ?"
                                    Dim tSQLParam As New List(Of Object)
                                    tSQLParam.Add(EcSanitizer.GetSafeHtmlFragment(ParamByName(Request.Params, _PARAMS_TINYURL)))  ' Anti-XSS
                                    Dim t As Object = App.Database.ExecuteScalarSQL(SQL, tSQLParam)
                                    If t IsNot Nothing Then
                                        Dim dt As DateTime = CType(t, DateTime)
                                        If dt < Now.AddSeconds(-_DEF_PASSWORD_LINK_TIMEOUT) Then     ' D2798
                                            sError = ResString("errResetPswExpired")
                                            App.DBTinyURLDelete(-1, -2, tUser.UserID)
                                        End If
                                    End If
                                End If
                                If sError = "" Then
                                    _tUser = tUser
                                    byToken = True
                                End If
                            End If
                        End If
                    End If
                End If
            Else
                sError = ResString("errTokenNotAvailable")
            End If
        End If
        If sError = "" AndAlso CurrentUser Is Nothing OrElse (Not byToken AndAlso CurrentUser IsNot Nothing AndAlso (CurrentUser.PasswordStatus >= 0 AndAlso (CurrentUser.UserPassword <> "" OrElse WebOptions.AllowBlankPsw)) AndAlso CurrentUser.PasswordStatus < _DEF_PASSWORD_ATTEMPTS) Then FetchAccess(CInt(IIf(CurrentUser Is Nothing, -1, _PGID_PROJECTSLIST))) ' D4484
        If (CurrentUser IsNot Nothing AndAlso CurrentUser.CannotBeDeleted AndAlso App.AvailableWorkgroups(CurrentUser).Count > 1) Then sError = ResString("errResetPswNoAllowed")

        If sError <> "" Then
            isFormEnabled = False
            lblMsg.Visible = True
            lblMsg.Text = sError
        Else
            isFormEnabled = CurrentUser IsNot Nothing ' D7178
        End If
    End Sub

    Private Sub onAJAX()
        Dim tRes As New jActionResult With {.Result = ecActionResult.arError}

        Dim args = ParseJSONParams(CheckVar("params", ""))
        If HasParam(args, "value", True) Then
            Dim fJustNewPsw As Boolean = CurrentUser.PasswordStatus = -1 OrElse CurrentUser.UserPassword = ""   ' D4494
            Dim sOldPsw As String = CurrentUser.UserPassword                ' D7114
            Dim sNewPsw = GetParam(args, "value", True)

            Dim fKeepHashes As Boolean = Str2Bool(GetParam(args, "keep_hashes", True))
            If App.SetUserPassword(CurrentUser, sNewPsw, fKeepHashes, AllowBlankPsw, tRes.Message) Then
                If fKeepHashes Then
                    TinyURLUpdateUserPsw(CurrentUser.UserID, sOldPsw, CurrentUser.UserPassword)
                End If
                If Str2Bool(GetCookie(_COOKIE_REMEMBER, "0")) Then SetCookie(_COOKIE_PASSWORD, CurrentUser.UserPassword, False, True)  ' D6446
                'Dim tDTSess As DateTime? = App.CheckSessionTerminate(App.ActiveUser)   ' D7357
                'If tDTSess.HasValue Then SessVar(_SESS_CMD_TERMINATE) = Date2ULong(tDTSess.Value).ToString    ' D7357
            End If

            If tRes.Message = "" Then
                ' D7193 ===
                Dim SQL As String = "DELETE FROM PrivateURLs WHERE hash = ?"
                Dim tSQLParam As New List(Of Object)
                tSQLParam.Add(EcSanitizer.GetSafeHtmlFragment(ParamByName(Request.Params, _PARAMS_TINYURL)))  ' Anti-XSS
                App.Database.ExecuteSQL(SQL, tSQLParam)
                ' D7193 ==
                App.DBUserUnlock(CurrentUser, sNewPsw, If(CurrentUser.PasswordStatus <= 0 OrElse fJustNewPsw, "Set a user new password", "Unlock the user when creating a new password"))    ' D6086 + D6460
                Dim sURL As String = PageURL(CInt(IIf(fJustNewPsw OrElse App.ActiveUser IsNot Nothing, _PGID_PROJECTSLIST, _PGID_START)))   ' D2219
                If CurrentUser.CannotBeDeleted Then sURL = PageURL(_PGID_ADMIN_WORKGROUPS) ' D2220
                ' D2799 + D4916 ===
                Dim sRetURL = SessVar(_SESS_RET_URL)   ' D3412
                SessVar(_SESS_RET_URL) = Nothing
                If Not String.IsNullOrEmpty(sRetURL) Then sURL = sRetURL ' D3412
                ' D4916 ==
                If sURL <> "" AndAlso Not sURL.ToLower.Contains("http") Then sURL = ApplicationURL(False, False) + sURL ' D3309 + D3494
                If sURL = "" Then sURL = PageURL(CInt(IIf(fJustNewPsw OrElse App.ActiveUser IsNot Nothing, _PGID_PROJECTSLIST, _PGID_START))) ' D3412
                ' D2799 ==
                If Not fJustNewPsw OrElse App.ActiveUser Is Nothing Then
                    App.Logout()
                    isFirstRun = False  ' D2762
                    SetCookie("first_run", "1", True, False)
                    sURL = PageURL(_PGID_START)
                End If
                tRes.Result = ecActionResult.arSuccess
                tRes.URL = sURL
            End If
        Else
            tRes.Message = "Invalid request parameters"
        End If
        If tRes.Message <> "" Then tRes.Result = ecActionResult.arError

        SendResponseJSON(tRes)
    End Sub

End Class
