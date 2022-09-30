Public Class StructureRedirectPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_HIERARCHY)
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        Response.Redirect(PageURL(_PGID_STRUCTURE_HIERARCHY), True)
    End Sub

End Class