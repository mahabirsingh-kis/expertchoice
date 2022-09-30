Public Class ProjectsListRedirectPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_PROJECTSLIST)
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        Response.Redirect(PageURL(_PGID_PROJECTSLIST), True)
    End Sub

End Class