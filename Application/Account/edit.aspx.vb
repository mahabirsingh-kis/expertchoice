Partial Class EditAccountNewPage
    Inherits clsComparionCorePage

    ' D0043 ===
    Public Sub New()
        MyBase.New(_PGID_ACCOUNT_EDIT)
    End Sub
    ' D0043 ==

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
    End Sub

End Class
