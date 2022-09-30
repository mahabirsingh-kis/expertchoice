Public Class ForgottenRedirectPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_START)
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        Response.Redirect(PageURL(_PGID_START, "remind=true"), True)
    End Sub

End Class