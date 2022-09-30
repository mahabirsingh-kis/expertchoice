Partial Class Error503Page
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_ERROR_503)
    End Sub

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
        'ShowNavigation = False
        If App.ApplicationError.Status = ecErrorStatus.errWrongLicense Then    ' D0261
            Dim sReport As String = App.ApplicationError.Message
            Dim CurWG As clsWorkgroup = App.ActiveWorkgroup ' D0261
            If sReport = "" AndAlso CurWG IsNot Nothing Then sReport = ResString(CStr(IIf(CurWG.License.isValidLicense, "errLicenseUnknown", "errLicenseFile"))) '  D0262
            sReport = String.Format("<h3 class='error' style='padding:1em 4em'>{0}</h3><h6 style='margin:3em'><b>{1}</b></h6>", sReport, String.Format(ResString("msgLicenseContact"), SystemEmail))  ' D0315 + D1152 + D4630
            If App.ApplicationError.DoFetch Then sReport = "<h4 class='error' style='margin-top:6em;'>" + ResString("errLicenseOver") + "</h4>" + sReport
            lblMessage.Text = sReport
        Else
            FetchAccess(_PGID_START)    ' D0262
        End If
    End Sub

End Class
