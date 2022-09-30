Partial Class UpdateDecisionPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_PROJECT_UPDATE)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        ShowNavigation = False
        StorePageID = False
    End Sub

End Class
