Partial Class ResultGroupsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_RESULTGROUPS)
    End Sub  

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        If App.ActiveProject Is Nothing Then FetchAccess()
    End Sub

End Class