Partial Class InfoPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_INFO_PAGE)
    End Sub

    Private Sub LandingPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        NavigationPageID = CheckVar(_PARAM_PAGE, CheckVar("pg", CheckVar(_PARAM_NAV_PAGE, CurrentPageID)))
    End Sub

End Class