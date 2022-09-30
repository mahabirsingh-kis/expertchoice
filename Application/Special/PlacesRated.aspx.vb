Partial Class PlacesRated
    Inherits clsComparionCorePage

    Private Const _COOKIE_PR_EMAIL As String = "PR_UID"
    Private Const _COOKIE_PR_PWD As String = "PR_PWD"

    Private Const _PR_PASSCODE As String = "placesrated"

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim isClear As Boolean = CheckVar("clear", "") <> ""

        Dim sUserEmail As String = GetCookie(_COOKIE_PR_EMAIL, "")
        Dim sUserPassword As String = GetCookie(_COOKIE_PR_PWD, "")
        If isClear Or sUserEmail = "" Then sUserEmail = String.Format("PR_{0}", GetRandomString(8, True, True).ToLower)
        If isClear Or sUserPassword = "" Then sUserPassword = GetRandomString(8, True, False)

        SetCookie(_COOKIE_PR_EMAIL, sUserEmail, False)
        SetCookie(_COOKIE_PR_PWD, sUserPassword, False)

        Dim sURL As String = String.Format("{0}={1}&{2}={3}&{4}={5}&{6}={7}", _PARAMS_EMAIL(0), sUserEmail, _
                                           _PARAMS_PASSWORD(0), sUserPassword, _
                                           _PARAMS_PASSCODE(0), _PR_PASSCODE, _
                                           _PARAMS_SIGNUP(0), 1)
        sURL = String.Format("{0}?key={1}&step=1", PageURL(_PGID_EVALUATENOW), EncodeURL(sURL, App.DatabaseID))

        Response.Redirect(sURL, True)
    End Sub

    Public Sub New()
        MyBase.New(_PGID_UNKNOWN)
    End Sub

End Class
