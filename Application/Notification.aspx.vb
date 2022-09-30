Partial Class NotificationPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_FEDRAMP_NOTIFICATION)
    End Sub

    Public Function GetRetPath() As String
        Dim sPath As String = SessVar("ReturnURL")
        If String.IsNullOrEmpty(sPath) Then sPath = PageURL(_PGID_START)
        Return sPath
    End Function

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
    End Sub

End Class