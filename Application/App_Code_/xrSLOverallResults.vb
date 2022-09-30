Imports System.Data
Imports Canvas

Public Class xrSLOverallResults
    Inherits DevExpress.XtraReports.UI.XtraReport

    Private fReportTitle As String = ""
    Public Property ReportTitle() As String
        Get
            Return fReportTitle
        End Get
        Set(ByVal value As String)
            fReportTitle = value
        End Set
    End Property

    Private Sub UpdateBinding(ByRef PM As clsProjectManager, ByVal NodeID As Integer) 'L0390
        PM.ProjectDataProvider.FullAlternativePath = False
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032
        PM.ProjectDataProvider.WRTNodeID = NodeID 'L0390
        Me.xrPivotGrid1.DataSource = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtPivotAlternativesPriorities)
        Me.xrPivotGrid1.DataMember = "PivotAlternativesPriorities"
        Me.Margins.Right = 50
    End Sub

#Region " Designer generated code "

    Public Sub New(ByRef CPM As clsProjectManager, ByVal ReportTitle As String, sAltName As String, Optional ByVal NodeID As Integer = 0) ' D2463
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle
        xrAlternativeField.Caption = sAltName   ' D2463
        UpdateBinding(CPM, NodeID) 'L0390
    End Sub

    'XtraReport overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub
    Private WithEvents subreport1 As DevExpress.XtraReports.UI.XRSubreport  ' D0233
    Private WithEvents ReportHeader As DevExpress.XtraReports.UI.ReportHeaderBand
    Private WithEvents xrPageInfo1 As DevExpress.XtraReports.UI.XRPageInfo
    Private WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand
    Private WithEvents xrPivotGrid1 As DevExpress.XtraReports.UI.XRPivotGrid
    Private WithEvents xrAlternativeField As DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField
    Private WithEvents xrUserField As DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField
    Private WithEvents xrPriorityField As DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField
    Private WithEvents PageHeader1 As DevExpress.XtraReports.UI.PageHeaderBand

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrSLOverallResults.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.xrPageInfo1 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        Me.xrPivotGrid1 = New DevExpress.XtraReports.UI.XRPivotGrid
        Me.xrAlternativeField = New DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField
        Me.xrUserField = New DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField
        Me.xrPriorityField = New DevExpress.XtraReports.UI.PivotGrid.XRPivotGridField
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.PageHeader1 = New DevExpress.XtraReports.UI.PageHeaderBand
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Height = 0
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'PageFooter
        '
        Me.PageFooter.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrPageInfo1})
        Me.PageFooter.Height = 25
        Me.PageFooter.Name = "PageFooter"
        Me.PageFooter.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.PageFooter.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrPageInfo1
        '
        Me.xrPageInfo1.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrPageInfo1.Location = New System.Drawing.Point(533, 0)
        Me.xrPageInfo1.Name = "xrPageInfo1"
        Me.xrPageInfo1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrPageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.Number
        Me.xrPageInfo1.Size = New System.Drawing.Size(108, 25)
        Me.xrPageInfo1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        '
        'subreport1
        '
        Me.subreport1.Location = New System.Drawing.Point(0, 0)
        Me.subreport1.Name = "subreport1"
        Me.subreport1.Size = New System.Drawing.Size(100, 23)
        '
        'ReportHeader
        '
        Me.ReportHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.subreport1})
        Me.ReportHeader.Height = 38
        Me.ReportHeader.Name = "ReportHeader"
        Me.ReportHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.ReportHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrPivotGrid1
        '
        Me.xrPivotGrid1.Fields.AddRange(New DevExpress.XtraPivotGrid.PivotGridField() {Me.xrAlternativeField, Me.xrUserField, Me.xrPriorityField})
        Me.xrPivotGrid1.Location = New System.Drawing.Point(0, 0)
        Me.xrPivotGrid1.Name = "xrPivotGrid1"
        Me.xrPivotGrid1.OptionsView.ShowColumnHeaders = False
        Me.xrPivotGrid1.OptionsView.ShowColumnTotals = False
        Me.xrPivotGrid1.OptionsView.ShowDataHeaders = False
        Me.xrPivotGrid1.OptionsView.ShowFilterHeaders = False
        Me.xrPivotGrid1.Size = New System.Drawing.Size(900, 40)
        '
        'xrAlternativeField
        '
        Me.xrAlternativeField.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea
        Me.xrAlternativeField.AreaIndex = 0
        Me.xrAlternativeField.Caption = "Alternative"
        Me.xrAlternativeField.FieldName = "AltName"
        Me.xrAlternativeField.MinWidth = 50
        Me.xrAlternativeField.Name = "xrAlternativeField"
        Me.xrAlternativeField.Options.ShowCustomTotals = False
        Me.xrAlternativeField.Options.ShowGrandTotal = False
        Me.xrAlternativeField.Options.ShowTotals = False
        Me.xrAlternativeField.Width = 400
        '
        'xrUserField
        '
        Me.xrUserField.Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea
        Me.xrUserField.AreaIndex = 0
        Me.xrUserField.Caption = "User"
        Me.xrUserField.FieldName = "UserName"
        Me.xrUserField.MinWidth = 50
        Me.xrUserField.Name = "xrUserField"
        Me.xrUserField.Options.AllowDrag = DevExpress.Utils.DefaultBoolean.[True]
        Me.xrUserField.Options.AllowExpand = DevExpress.Utils.DefaultBoolean.[True]
        Me.xrUserField.Options.AllowSort = DevExpress.Utils.DefaultBoolean.[True]
        Me.xrUserField.Options.ShowCustomTotals = False
        Me.xrUserField.Options.ShowGrandTotal = False
        Me.xrUserField.Options.ShowTotals = False
        Me.xrUserField.Width = 143
        '
        'xrPriorityField
        '
        Me.xrPriorityField.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea
        Me.xrPriorityField.AreaIndex = 0
        Me.xrPriorityField.CellFormat.FormatString = "p2"
        Me.xrPriorityField.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric
        Me.xrPriorityField.FieldName = "AltGlobalPriority"
        Me.xrPriorityField.MinWidth = 50
        Me.xrPriorityField.Name = "xrPriorityField"
        Me.xrPriorityField.Options.ShowCustomTotals = False
        Me.xrPriorityField.Options.ShowGrandTotal = False
        Me.xrPriorityField.Options.ShowTotals = False
        Me.xrPriorityField.Width = 143
        '
        'PageHeader
        '
        Me.PageHeader.Height = 20
        Me.PageHeader.Name = "PageHeader"
        '
        'PageHeader1
        '
        Me.PageHeader1.BorderWidth = 0
        Me.PageHeader1.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrPivotGrid1})
        Me.PageHeader1.Height = 40
        Me.PageHeader1.Name = "PageHeader1"
        Me.PageHeader1.StylePriority.UseBorderWidth = False
        '
        'xrSLOverallResults
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageFooter, Me.ReportHeader, Me.PageHeader1})
        Me.BorderWidth = 0
        Me.Margins = New System.Drawing.Printing.Margins(50, 50, 100, 100)
        Me.Version = "8.3"
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region
    'L0390 ===
    Private Sub xrPivotGrid1_CustomCellDisplayText(ByVal sender As Object, ByVal e As DevExpress.XtraPivotGrid.PivotCellDisplayTextEventArgs) Handles xrPivotGrid1.CustomCellDisplayText
        If e.Value.ToString = "-1" Then e.DisplayText = "N/A"
    End Sub
    'L0390 ==
End Class