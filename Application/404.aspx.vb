Partial Class Error404Page
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ERROR_404)
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        ShowNavigation = False
        Dim sRef As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("aspxerrorpath", ""))   ' Anti-XSS
        If sRef = "" And Not Request.UrlReferrer Is Nothing Then
            If Not Request.UrlReferrer.AbsoluteUri Is Nothing Then sRef = Request.UrlReferrer.AbsoluteUri.Replace(ApplicationURL(False, False) + _URL_ROOT, "") ' D1663 + D1664 + D3494
        End If
        lblError.Text = ResString("msgError404") + CStr(IIf(sRef = "", "", String.Format("<div style='font-weight:normal; margin-top:1ex; color:#444444;'>(URL: {0})</div>", RemoveXssFromUrl(sRef, True))))    ' D1663 + D1664 + Anti-XSS
    End Sub

End Class
