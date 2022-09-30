Option Strict Off

Imports System.Data

Public Class xrJudgementsOverview
    Inherits DevExpress.XtraReports.UI.XtraReport

    Private ProjectManager As New clsProjectManager
    Private ReportType As CanvasReportType = CanvasReportType.crtAllJudgments   ' D3703

    'L0032 ===
    Private fReportTitle As String = ""
    Public Property ReportTitle() As String
        Get
            Return fReportTitle
        End Get
        Set(ByVal value As String)
            fReportTitle = value
        End Set
    End Property
    'L0032 ==

    'L0215 ===
    Private fAllJudgementsDataSet As DataSet
    Public Property AllJudgemnetsDataSet() As DataSet
        Get
            Return fAllJudgementsDataSet
        End Get
        Set(ByVal value As DataSet)
            fAllJudgementsDataSet = value
        End Set
    End Property
    'L0215 ==

    Private Sub UpdateBinding()
        Me.DataSource = ProjectManager.ProjectDataProvider.ReportDataSet(CanvasReportType.crtHierarchyObjectives)
        Me.DataMember = "HierarchyStructure"
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032
        Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodePath")
        Me.Margins.Right = 50
        AllJudgemnetsDataSet = ProjectManager.ProjectDataProvider.ReportDataSet(ReportType) ' D3703
    End Sub

#Region " Designer generated code "

    Public Sub New(ByRef CPM As clsProjectManager, ByVal ReportTitle As String, tReportType As CanvasReportType) 'L0032 + D3703
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()
        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle 'L0032
        ReportType = tReportType    ' D3703
        ProjectManager = CPM
        UpdateBinding()
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
    Private WithEvents xrTable1 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow1 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell1 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrPageInfo1 As DevExpress.XtraReports.UI.XRPageInfo
    Private WithEvents subreport2 As DevExpress.XtraReports.UI.XRSubreport  ' D0233

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrJudgementsOverview.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.subreport2 = New DevExpress.XtraReports.UI.XRSubreport
        Me.xrTable1 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow1 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell1 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrPageInfo1 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.subreport2, Me.xrTable1})
        Me.Detail.Height = 44
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'subreport2
        '
        Me.subreport2.Location = New System.Drawing.Point(0, 21)
        Me.subreport2.Name = "subreport2"
        Me.subreport2.Size = New System.Drawing.Size(100, 23)
        '
        'xrTable1
        '
        Me.xrTable1.BackColor = System.Drawing.Color.LightBlue
        Me.xrTable1.Location = New System.Drawing.Point(0, 0)
        Me.xrTable1.Name = "xrTable1"
        Me.xrTable1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 96.0!)
        Me.xrTable1.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow1})
        Me.xrTable1.Size = New System.Drawing.Size(650, 20)
        Me.xrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTableRow1
        '
        Me.xrTableRow1.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell1})
        Me.xrTableRow1.Name = "xrTableRow1"
        Me.xrTableRow1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 96.0!)
        Me.xrTableRow1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow1.Weight = 1
        '
        'xrTableCell1
        '
        Me.xrTableCell1.BackColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.xrTableCell1.BookmarkParent = Me.xrPageInfo1
        Me.xrTableCell1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.xrTableCell1.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell1.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell1.ForeColor = System.Drawing.Color.White
        Me.xrTableCell1.Name = "xrTableCell1"
        Me.xrTableCell1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell1.StylePriority.UseBackColor = False
        Me.xrTableCell1.StylePriority.UseBorderColor = False
        Me.xrTableCell1.StylePriority.UseFont = False
        Me.xrTableCell1.StylePriority.UseForeColor = False
        Me.xrTableCell1.Text = "xrTableCell1"
        Me.xrTableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell1.Weight = 1
        '
        'xrPageInfo1
        '
        Me.xrPageInfo1.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrPageInfo1.Location = New System.Drawing.Point(550, 0)
        Me.xrPageInfo1.Name = "xrPageInfo1"
        Me.xrPageInfo1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrPageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.Number
        Me.xrPageInfo1.Size = New System.Drawing.Size(100, 25)
        Me.xrPageInfo1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        '
        'PageFooter
        '
        Me.PageFooter.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrPageInfo1})
        Me.PageFooter.Height = 25
        Me.PageFooter.Name = "PageFooter"
        Me.PageFooter.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.PageFooter.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
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
        Me.ReportHeader.Height = 33
        Me.ReportHeader.Name = "ReportHeader"
        Me.ReportHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.ReportHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrJudgementsOverview
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageFooter, Me.ReportHeader})
        Me.Margins = New System.Drawing.Printing.Margins(100, 100, 100, 120)
        Me.Version = "9.1"
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region

    Private Sub Detail_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles Detail.BeforePrint 'L0380
        Dim fnt1 As New Drawing.Font("Tahoma", 10, Drawing.FontStyle.Regular)
        Dim fnt2 As New Drawing.Font("Tahoma", 10, Drawing.FontStyle.Bold)
        'L0380 + D3709 ===
        Me.subreport2.Visible = False
        Me.xrTableCell1.Visible = False
        Me.xrTable1.Height = 0
        Me.Detail.Height = 0
        Me.subreport2.Top = 0
        Me.subreport2.Height = 0

        If ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).Nodes.Count = 0 _
        And Me.GetCurrentColumnValue("IsCoveringObjective") = True Then

            Me.subreport2.ReportSource = New xrNonRatingJudgements(AllJudgemnetsDataSet)

        Else
            'L0380 ==

            Dim fVisible As Boolean = False

            If (Me.GetCurrentColumnValue("MeasurementType") <> "Pairwise") And (Me.GetCurrentColumnValue("MeasurementType") <> "Ratings") Then
                Me.subreport2.ReportSource = New xrNonRatingJudgements(AllJudgemnetsDataSet) 'L0215
                Dim subJudgments As xrNonRatingJudgements
                subJudgments = Me.subreport2.ReportSource
                fVisible = subJudgments.RowCount > 0    ' D3709
                subJudgments.CurrentNodeID = Me.GetCurrentColumnValue("NodeID")
            End If

            If Me.GetCurrentColumnValue("MeasurementType") = "Pairwise" Then
                Me.subreport2.ReportSource = New xrJudgements(AllJudgemnetsDataSet) 'L0215
                Dim subJudgments As xrJudgements
                subJudgments = Me.subreport2.ReportSource
                'fVisible = True
                subJudgments.CurrentNodeID = Me.GetCurrentColumnValue("NodeID")
                fVisible = subJudgments.RowCount > 0    ' D3709
            End If

            If Me.GetCurrentColumnValue("MeasurementType") = "Ratings" Then
                Me.subreport2.ReportSource = New xrRatingJudgements(AllJudgemnetsDataSet) 'L0215
                Dim subJudgments As xrRatingJudgements
                subJudgments = Me.subreport2.ReportSource
                fVisible = subJudgments.RowCount > 0    ' D3709
                subJudgments.CurrentNodeID = Me.GetCurrentColumnValue("NodeID")
            End If

            If True Then 'fVisible Then    ' D3709
                'L0380 ===
                Me.xrTable1.Height = 20
                Me.subreport2.Top = 21
                Me.subreport2.Visible = True
                Me.xrTableCell1.Visible = True
                Me.Detail.Height = 44
                'L0380 ==
            End If
        End If
    End Sub

End Class