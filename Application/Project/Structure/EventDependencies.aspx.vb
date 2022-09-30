Partial Class EventDependenciesPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_RISK_EVENT_DEPENDENCIES)
    End Sub  

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        If App.ActiveProject Is Nothing Then FetchAccess()
    End Sub

End Class