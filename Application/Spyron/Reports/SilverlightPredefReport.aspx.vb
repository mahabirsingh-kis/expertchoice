Option Strict Off

'L0364
Imports System.Drawing
Imports DevExpress.XtraPrinting.Drawing
Imports DevExpress.XtraReports.UI
Imports ExpertChoice.Web.Controls

Partial Class SilverlightPredefReport
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_REPORT_CUSTOM)
    End Sub

    Private PM As clsProjectManager

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim Report As XtraReport = Nothing
        Dim ReportName As String = ""

        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        PM = App.ActiveProject.ProjectManager
        PM.ProjectDataProvider.FullAlternativePath = False

        Dim R As ctrlReport = Nothing
        'L0390 ===
        Dim NodeID As Integer = -1
        Dim sID As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("NodeID", ""))   ' Anti-XSS
        If sID <> "" Then Integer.TryParse(sID, NodeID)

        'L0473 ===
        Dim CIS, AIP, PRT As Boolean
        CIS = False
        AIP = False
        PRT = False
        Dim sBool As String = EcSanitizer.GetSafeHtmlFragment(CheckVar("CIS", ""))    ' Anti-XSS
        If sBool <> "" Then Boolean.TryParse(sBool, CIS)
        sBool = EcSanitizer.GetSafeHtmlFragment(CheckVar("AIP", ""))  ' Anti-XSS
        If sBool <> "" Then Boolean.TryParse(sBool, AIP)
        sBool = EcSanitizer.GetSafeHtmlFragment(CheckVar("PRT", ""))  ' Anti-XSS
        If sBool <> "" Then Boolean.TryParse(sBool, PRT)
        'L0473 ==
        Report = New xrSLOverallResults(PM, "Overall Results", ResString("dgAlt"), NodeID) 'A866
        ReportName = "xrSLOverallResults"
        'L0390 ==
        R = CtrlReport1

        Dim DefaultCommentType = App.ActiveProject.ProjectManager.ProjectDataProvider.CommentType
        If (R IsNot Nothing) AndAlso (Report IsNot Nothing) AndAlso (ReportName <> "") Then
            Dim AWatermark = Image.FromFile(Me.Server.MapPath("Watermark.png"))
            Report.Watermark.ImageViewMode = ImageViewMode.Stretch
            Report.Watermark.ShowBehind = True
            Report.Watermark.Image = AWatermark

            R.ReportViewer.Report = Report
            R.ReportViewer.ReportName = ReportName
            R.ReportViewer.DataBind()

        End If

        App.ActiveProject.ProjectManager.ProjectDataProvider.CommentType = DefaultCommentType

    End Sub
End Class
