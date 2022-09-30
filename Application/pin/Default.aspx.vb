Public Class PinCodePage
    Inherits clsComparionCorePage

    Public Pin As String = "0000"
    Public Timeout As Integer = 0

    Public Sub New()
        MyBase.New(_PGID_PINCODE)
    End Sub

    Protected Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit
        If Not _PINCODE_ALLOWED Then Response.Redirect(PageURL(_PGID_START), True)  ' D7249
        If Not App.isAuthorized Then Response.Redirect(PageURL(_PGID_START, "?pin"), True)
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        If Not String.IsNullOrEmpty(SessVar(_SESS_RET_URL)) Then
            ShowNavigation = False
            ShowTopNavigation = False
        End If

        If Not isAJAX Then
            Dim fDoRefresh As Boolean = CheckVar("action", "").ToLower = "refresh"
            Dim Data As Tuple(Of Integer, Integer) = App.GetUserPin(ecPinCodeType.mfaAlexa, App.ActiveUser.UserID, App.ProjectID, , fDoRefresh)    ' D7501 + D7502
            Pin = padWithZeros(Data.Item1.ToString, 4)
            Timeout = Data.Item2
            If fDoRefresh Then Response.Redirect(PageURL(CurrentPageID), True)
        End If
    End Sub

End Class