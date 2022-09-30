Option Strict Off

Partial Class MaxOutReportPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_REPORT_MAXOUT)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RawResponseStart()
        'Response.ContentType = "text/xml"

        DebugInfo("Start create MaxOut report", _TRACE_INFO)

        Dim sContent As String = App.ActiveProject.ProjectManager.ProjectDataProvider.ReportDataSet(CanvasReportType.crtMaxOutPriorities).GetXml()

        Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""MaxOut{0}.xml""", Now().ToString("yyMMdd")))
        Response.ContentType = "application/xhtml+xml"
        Response.Charset = "utf-8"
        Response.AddHeader("Content-Length", sContent.Length)

        DebugInfo("Send content to user", _TRACE_INFO)
        Response.Write(sContent)
        RawResponseEnd()
    End Sub

End Class