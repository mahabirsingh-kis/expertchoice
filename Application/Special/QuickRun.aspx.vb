
Partial Class QuickRunCore
    Inherits clsComparionCorePage

    Private Const _COOKIE_QR_EMAIL As String = "QR_UID"
    Private Const _COOKIE_QR_PWD As String = "QR_PWD"
    Private Const _COOKIE_QR_PASSCODE As String = "QR_CODE" ' D0349

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim isClear As Boolean = CheckVar("clear", False) OrElse CheckVar("c", False)   ' D4097

        Dim sUserEmail As String = GetCookie(_COOKIE_QR_EMAIL, "").Trim
        Dim sUserPassword As String = GetCookie(_COOKIE_QR_PWD, "").Trim
        Dim sPrevPassword As String = GetCookie(_COOKIE_QR_PASSCODE, "").Trim   ' D0349
        Dim sPasscode As String = EcSanitizer.GetSafeHtmlFragment(ParamByName(Request.Params, _PARAMS_PASSCODE)) ' D0488 + Anti-XSS

        If sPrevPassword <> "" And sPasscode <> "" And sPrevPassword.ToLower <> sPasscode.ToLower Then isClear = True ' D0349

        If isClear Or sUserEmail = "" Then sUserEmail = String.Format("{1}_{0}", GetRandomString(8, True, True).ToLower, IIf(sPasscode = "", "Anonymous", sPasscode.Trim.ToLower))
        If isClear Or sUserPassword = "" Then sUserPassword = GetRandomString(8, True, False)

        ' D0349 ===
        SetCookie(_COOKIE_QR_EMAIL, sUserEmail, True, True) ' D0755
        SetCookie(_COOKIE_QR_PWD, sUserPassword, True, True)    ' D0755
        SetCookie(_COOKIE_QR_PASSCODE, sPasscode, False)

        'Dim sNavigation As String = IIf(CheckVar(_ACTION_NAVIGATION, _ROLE_HIDE) = _ROLE_HIDE, String.Format("&{0}={1}", _ACTION_NAVIGATION, _ROLE_HIDE), "")
        If CheckVar(_ACTION_NAVIGATION, _ROLE_HIDE) = _ROLE_HIDE Then ShowNavigation = False ' D0352 + D0488

        Dim sURL As String = String.Format("{0}={1}&{2}={3}&{4}={5}&{6}={7}", _PARAMS_EMAIL(0), sUserEmail, _
                                           _PARAMS_PASSWORD(0), sUserPassword, _
                                           _PARAMS_PASSCODE(0), sPasscode, _
                                           _PARAMS_SIGNUP(0), 1)

        Dim sExtraParams As String = ""
        Dim tNewParams As NameValueCollection = Request.QueryString
        For Each sName As String In tNewParams
            If sName.ToLower <> "clear" AndAlso sName.ToLower <> "c" AndAlso sName.ToLower <> _ACTION_NAVIGATION AndAlso Array.IndexOf(_PARAMS_PASSCODE, sName) < 0 Then sExtraParams += String.Format("&{0}={1}", sName, Request(sName)) ' D4097
        Next

        sURL = String.Format("{0}?key={1}{2}", PageURL(_PGID_EVALUATENOW), EncodeURL(sURL, App.DatabaseID), sExtraParams)   ' D0826
        ' D0349 ==

        Response.Redirect(sURL, True)
    End Sub

    Public Sub New()
        MyBase.New(_PGID_UNKNOWN)
    End Sub

End Class
