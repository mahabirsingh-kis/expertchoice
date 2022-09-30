Public Class RiskRewardRedirectPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_UNKNOWN)
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        App.Options.RiskionRiskRewardMode = Not App.Options.RiskionRiskRewardMode
        Server.Transfer(PageURL(_PGID_START), True)
    End Sub

End Class