Partial Class ViewLogsPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ADMIN_VIEWLOGS)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
    End Sub

    Public Function GetWorkgroups() As String
        Dim resVal As String = ""
        Dim AvailableWorkgroups As List(Of clsWorkgroup) = App.AvailableWorkgroupsAsWM()
        If AvailableWorkgroups.Count > 1 Then
            resVal = String.Format("{{ID: {0}, name:""{1}""}}", -1, "[All Workgroups]")
            For Each wg As clsWorkgroup In AvailableWorkgroups
                resVal += If(resVal = "", "", ",") + String.Format("{{ID: {0}, name:""{1}""}}", wg.ID, JS_SafeString(wg.Name))
            Next
        End If
        Return resVal
    End Function

End Class
