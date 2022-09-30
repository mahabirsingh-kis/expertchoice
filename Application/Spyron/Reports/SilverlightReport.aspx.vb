Option Strict Off

'L0364
Imports Telerik.Web.UI

Partial Class SilverlightCustomReport
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_REPORT_CUSTOM)
    End Sub

    Private PM As clsProjectManager

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        AlignHorizontalCenter = False
        AlignVerticalCenter = False
        PM = App.ActiveProject.ProjectManager
        PM.ProjectDataProvider.FullAlternativePath = False
        'If Session("DSADHOC") Is Nothing Then
        '    Session("DSADHOC") = App.ActiveProject.Passcode
        'End If
    End Sub

    Protected Sub tbExport_ButtonClick(sender As Object, e As RadToolBarEventArgs) Handles tbExport.ButtonClick
        Dim FileName As String = "PairwiseJudgements"
        Select Case CType(e.Item, RadToolBarButton).CommandName
            Case "ExporttoRTF"
                ASPxGridViewExporter.WriteRtfToResponse(FileName, True)
            Case "ExporttoPDF"
                ASPxGridViewExporter.WritePdfToResponse(FileName, True)
            Case "ExporttoXLS"
                ASPxGridViewExporter.WriteXlsToResponse(FileName, True)
            Case "Generate"

                btnPDF.Enabled = True
                btnRTF.Enabled = True
                btnXLS.Enabled = True

                ASPxGridView.Visible = True
                ASPxGridView_Load(sender, e)
        End Select
    End Sub

    Protected Sub ASPxGridView_Load(sender As Object, e As EventArgs) Handles ASPxGridView.Load
        If ASPxGridView.Visible Then
            If Session("DS1") Is Nothing Or Session("DSADHOC") <> App.ActiveProject.Passcode Then
                Session("DS1") = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtAllJudgments)
            End If
            ASPxGridView.DataSource = Session("DS1")
            ASPxGridView.DataMember = "AllPairwiseJudgments"
            ASPxGridView.DataBind()
        End If
    End Sub
End Class
