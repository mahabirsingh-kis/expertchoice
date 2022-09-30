Partial Class SimulationGroupsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_RISK_SIMULATION_GROUPS)
    End Sub  

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        If App.ActiveProject Is Nothing Then FetchAccess()
    End Sub

End Class