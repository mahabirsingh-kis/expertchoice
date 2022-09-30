Option Strict Off

Imports System.Windows.Forms 'L0122
Imports System.Data

Public Class xrObjectivePriorities

    Inherits DevExpress.XtraReports.UI.XtraReport
    'L0032 ===
    Private fReportTitle As String = ""
    Private WithEvents xrRichText1 As DevExpress.XtraReports.UI.XRRichText
    Private WithEvents xrTableCell7 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell8 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrShape1 As DevExpress.XtraReports.UI.XRShape
    Private WithEvents xrShape2 As DevExpress.XtraReports.UI.XRShape
    Public Property ReportTitle() As String
        Get
            Return fReportTitle
        End Get
        Set(ByVal value As String)
            fReportTitle = value
        End Set
    End Property
    'L0032 ==
    'L0101 ===
    Private fReportOptions As String = "0000000000"
    Public Property ReportOptions() As String
        Get
            Return fReportOptions
        End Get
        Set(ByVal value As String)
            fReportOptions = value
        End Set
    End Property
    'L0101 ==
    Private Sub UpdateBinding(ByRef PM As clsProjectManager)
        Me.DataSource = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtObjectivesPriorities)
        Me.DataMember = "StructureWithPriorities"
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032
        'L0101 ===
        If ReportOptions(0) = "0" Then
            Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodeName")
        Else
            Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodePath")
        End If
        'L0101 ==
        Me.xrTableCell2.DataBindings.Add("Text", DataSource, "StructureWithPriorities.LocalPriority", "{0:P}")
        Me.xrTableCell3.DataBindings.Add("Text", DataSource, "StructureWithPriorities.GlobalPriority", "{0:P}")
        Me.Margins.Right = 50
    End Sub

#Region " Designer generated code "

    Public Sub New(ByRef CPM As clsProjectManager, ByVal ReportTitle As String, ByVal ROptions As String, sObjName As String, sLocalPrty As String, sGlobalPrty As String) 'L0032 'L0101 + D2465 + D2475
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle 'L0032
        Me.ReportOptions = ROptions 'L0101
        xrTableCell4.Text = sObjName    ' D2465
        xrTableCell5.Text = sLocalPrty  ' D2475
        xrTableCell6.Text = sGlobalPrty ' D2475
        UpdateBinding(CPM)
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
    Private WithEvents xrTableCell2 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrPageInfo1 As DevExpress.XtraReports.UI.XRPageInfo
    Private WithEvents xrTableCell3 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand
    Private WithEvents xrTable2 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow2 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell4 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell5 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell6 As DevExpress.XtraReports.UI.XRTableCell

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrObjectivePriorities.resx"
        Dim shapeRectangle1 As DevExpress.XtraPrinting.Shape.ShapeRectangle = New DevExpress.XtraPrinting.Shape.ShapeRectangle
        Dim shapeRectangle2 As DevExpress.XtraPrinting.Shape.ShapeRectangle = New DevExpress.XtraPrinting.Shape.ShapeRectangle
        Dim xrSummary1 As DevExpress.XtraReports.UI.XRSummary = New DevExpress.XtraReports.UI.XRSummary
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.xrRichText1 = New DevExpress.XtraReports.UI.XRRichText
        Me.xrTable1 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow1 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell1 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell7 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrShape1 = New DevExpress.XtraReports.UI.XRShape
        Me.xrTableCell2 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell8 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrShape2 = New DevExpress.XtraReports.UI.XRShape
        Me.xrTableCell3 = New DevExpress.XtraReports.UI.XRTableCell
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.xrPageInfo1 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.xrTable2 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow2 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell4 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell5 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell6 = New DevExpress.XtraReports.UI.XRTableCell
        CType(Me.xrRichText1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrRichText1, Me.xrTable1})
        Me.Detail.Height = 42
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrRichText1
        '
        Me.xrRichText1.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrRichText1.Location = New System.Drawing.Point(0, 20)
        Me.xrRichText1.Name = "xrRichText1"
        Me.xrRichText1.SerializableRtfString = ""
        Me.xrRichText1.Size = New System.Drawing.Size(650, 20)
        Me.xrRichText1.StylePriority.UseBorders = False
        '
        'xrTable1
        '
        Me.xrTable1.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrTable1.Location = New System.Drawing.Point(0, 0)
        Me.xrTable1.Name = "xrTable1"
        Me.xrTable1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTable1.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow1})
        Me.xrTable1.Size = New System.Drawing.Size(650, 20)
        Me.xrTable1.StylePriority.UseBackColor = False
        Me.xrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTableRow1
        '
        Me.xrTableRow1.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrTableRow1.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell1, Me.xrTableCell7, Me.xrTableCell2, Me.xrTableCell8, Me.xrTableCell3})
        Me.xrTableRow1.Name = "xrTableRow1"
        Me.xrTableRow1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTableRow1.StylePriority.UseBackColor = False
        Me.xrTableRow1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow1.Weight = 1
        '
        'xrTableCell1
        '
        Me.xrTableCell1.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrTableCell1.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell1.Name = "xrTableCell1"
        Me.xrTableCell1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell1.StylePriority.UseBackColor = False
        Me.xrTableCell1.StylePriority.UseBorders = False
        Me.xrTableCell1.Text = "xrTableCell1"
        Me.xrTableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell1.Weight = 0.55076923076923079
        '
        'xrTableCell7
        '
        Me.xrTableCell7.BackColor = System.Drawing.Color.Transparent
        Me.xrTableCell7.BorderColor = System.Drawing.Color.Silver
        Me.xrTableCell7.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell7.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrShape1})
        Me.xrTableCell7.Name = "xrTableCell7"
        Me.xrTableCell7.StylePriority.UseBackColor = False
        Me.xrTableCell7.StylePriority.UseBorderColor = False
        Me.xrTableCell7.StylePriority.UseBorders = False
        Me.xrTableCell7.Weight = 0.12923076923076926
        '
        'xrShape1
        '
        Me.xrShape1.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrShape1.BorderWidth = 0
        Me.xrShape1.FillColor = System.Drawing.Color.FromArgb(CType(CType(19, Byte), Integer), CType(CType(141, Byte), Integer), CType(CType(164, Byte), Integer))
        Me.xrShape1.LineWidth = 0
        Me.xrShape1.Location = New System.Drawing.Point(2, 2)
        Me.xrShape1.Name = "xrShape1"
        Me.xrShape1.Shape = shapeRectangle1
        Me.xrShape1.Size = New System.Drawing.Size(80, 16)
        Me.xrShape1.StylePriority.UseBackColor = False
        Me.xrShape1.StylePriority.UseBorderWidth = False
        '
        'xrTableCell2
        '
        Me.xrTableCell2.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrTableCell2.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell2.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell2.Name = "xrTableCell2"
        Me.xrTableCell2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell2.StylePriority.UseBackColor = False
        Me.xrTableCell2.StylePriority.UseBorders = False
        Me.xrTableCell2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        Me.xrTableCell2.Weight = 0.089230769230769238
        '
        'xrTableCell8
        '
        Me.xrTableCell8.BackColor = System.Drawing.Color.Transparent
        Me.xrTableCell8.BorderColor = System.Drawing.Color.Silver
        Me.xrTableCell8.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell8.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrShape2})
        Me.xrTableCell8.Name = "xrTableCell8"
        Me.xrTableCell8.StylePriority.UseBackColor = False
        Me.xrTableCell8.StylePriority.UseBorderColor = False
        Me.xrTableCell8.StylePriority.UseBorders = False
        Me.xrTableCell8.Weight = 0.12923076923076926
        '
        'xrShape2
        '
        Me.xrShape2.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrShape2.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrShape2.BorderWidth = 0
        Me.xrShape2.FillColor = System.Drawing.Color.FromArgb(CType(CType(19, Byte), Integer), CType(CType(141, Byte), Integer), CType(CType(164, Byte), Integer))
        Me.xrShape2.LineWidth = 0
        Me.xrShape2.Location = New System.Drawing.Point(2, 2)
        Me.xrShape2.Name = "xrShape2"
        Me.xrShape2.Shape = shapeRectangle2
        Me.xrShape2.Size = New System.Drawing.Size(80, 16)
        Me.xrShape2.StylePriority.UseBackColor = False
        Me.xrShape2.StylePriority.UseBorders = False
        Me.xrShape2.StylePriority.UseBorderWidth = False
        '
        'xrTableCell3
        '
        Me.xrTableCell3.BackColor = System.Drawing.Color.FromArgb(CType(CType(222, Byte), Integer), CType(CType(231, Byte), Integer), CType(CType(209, Byte), Integer))
        Me.xrTableCell3.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell3.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell3.Name = "xrTableCell3"
        Me.xrTableCell3.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell3.StylePriority.UseBackColor = False
        Me.xrTableCell3.StylePriority.UseBorders = False
        xrSummary1.FormatString = "{0:0.00%}"
        Me.xrTableCell3.Summary = xrSummary1
        Me.xrTableCell3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        Me.xrTableCell3.Weight = 0.10153846153846154
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
        Me.xrPageInfo1.Location = New System.Drawing.Point(592, 0)
        Me.xrPageInfo1.Name = "xrPageInfo1"
        Me.xrPageInfo1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrPageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.Number
        Me.xrPageInfo1.Size = New System.Drawing.Size(58, 25)
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
        Me.ReportHeader.Height = 33
        Me.ReportHeader.Name = "ReportHeader"
        Me.ReportHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.ReportHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'PageHeader
        '
        Me.PageHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrTable2})
        Me.PageHeader.Height = 25
        Me.PageHeader.Name = "PageHeader"
        Me.PageHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.PageHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTable2
        '
        Me.xrTable2.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTable2.Location = New System.Drawing.Point(0, 0)
        Me.xrTable2.Name = "xrTable2"
        Me.xrTable2.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTable2.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow2})
        Me.xrTable2.Size = New System.Drawing.Size(650, 25)
        Me.xrTable2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTableRow2
        '
        Me.xrTableRow2.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell4, Me.xrTableCell5, Me.xrTableCell6})
        Me.xrTableRow2.Name = "xrTableRow2"
        Me.xrTableRow2.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTableRow2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow2.Weight = 1
        '
        'xrTableCell4
        '
        Me.xrTableCell4.BackColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.xrTableCell4.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell4.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell4.ForeColor = System.Drawing.Color.White
        Me.xrTableCell4.Name = "xrTableCell4"
        Me.xrTableCell4.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell4.StylePriority.UseBackColor = False
        Me.xrTableCell4.StylePriority.UseFont = False
        Me.xrTableCell4.StylePriority.UseForeColor = False
        Me.xrTableCell4.StylePriority.UseTextAlignment = False
        Me.xrTableCell4.Text = "Objectives"
        Me.xrTableCell4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell4.Weight = 0.55076923076923068
        '
        'xrTableCell5
        '
        Me.xrTableCell5.BackColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.xrTableCell5.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Top Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell5.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell5.ForeColor = System.Drawing.Color.White
        Me.xrTableCell5.Name = "xrTableCell5"
        Me.xrTableCell5.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell5.StylePriority.UseBackColor = False
        Me.xrTableCell5.StylePriority.UseFont = False
        Me.xrTableCell5.StylePriority.UseForeColor = False
        Me.xrTableCell5.StylePriority.UseTextAlignment = False
        Me.xrTableCell5.Text = "Local Priority"
        Me.xrTableCell5.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell5.Weight = 0.21846153846153846
        '
        'xrTableCell6
        '
        Me.xrTableCell6.BackColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.xrTableCell6.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Top Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell6.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell6.ForeColor = System.Drawing.Color.White
        Me.xrTableCell6.Name = "xrTableCell6"
        Me.xrTableCell6.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell6.StylePriority.UseBackColor = False
        Me.xrTableCell6.StylePriority.UseFont = False
        Me.xrTableCell6.StylePriority.UseForeColor = False
        Me.xrTableCell6.StylePriority.UseTextAlignment = False
        Me.xrTableCell6.Text = "Global Priority"
        Me.xrTableCell6.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell6.Weight = 0.23076923076923075
        '
        'xrObjectivePriorities
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageFooter, Me.ReportHeader, Me.PageHeader})
        Me.Margins = New System.Drawing.Printing.Margins(100, 50, 100, 100)
        Me.Version = "8.3"
        CType(Me.xrRichText1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region

    Dim LineSwitch As Boolean = True 'L0191
    Dim AColor As System.Drawing.Color = System.Drawing.Color.FromArgb(239, 243, 234) 'L0191

    Private Sub xrTable1_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles xrTable1.BeforePrint
        'L0191 ==
        If LineSwitch Then
            AColor = System.Drawing.Color.FromArgb(222, 231, 209)
        Else
            AColor = System.Drawing.Color.FromArgb(239, 243, 234)
        End If
        LineSwitch = Not LineSwitch

        xrTableCell1.BackColor = AColor
        xrTableCell2.BackColor = AColor
        xrTableCell3.BackColor = AColor
        xrTableCell7.BackColor = AColor
        xrTableCell8.BackColor = AColor

        'L0389 ===
        Dim LocalPrty As Double = CDbl(CType(Me.GetCurrentRow(), DataRowView).Item("LocalPriority")) * 80
        Dim GlobalPrty As Double = CDbl(CType(Me.GetCurrentRow(), DataRowView).Item("GlobalPriority")) * 80
        xrShape1.Width = LocalPrty
        xrShape2.Width = GlobalPrty
        'L0389 ==
        'L0191 ===
        'L0101 ===
        If ReportOptions(0) = "0" Then
            Dim SpaceCount = CStr(CType(Me.GetCurrentRow(), DataRowView).Item("NodePath")).Split("|").Count()
            For i As Integer = 1 To SpaceCount - 1
                xrTableCell1.Text = "        " + xrTableCell1.Text
            Next
        End If
        'L0101 ==
        'L0210 ===
        If ReportOptions(5) = "0" Then
            xrTableCell7.BorderWidth = 0
            xrTableCell8.BorderWidth = 0
            xrShape1.Visible = False
            xrShape2.Visible = False
        Else
            xrTableCell7.BorderWidth = 1
            xrTableCell8.BorderWidth = 1
            xrShape1.Visible = True
            xrShape2.Visible = True
        End If
        'L0210 ==
        'L0389 ===
        If LocalPrty = 0 Then xrShape1.Visible = False
        If GlobalPrty = 0 Then xrShape2.Visible = False
        'L0389 ==
        'L0194 ===
        xrRichText1.Visible = False
        xrRichText1.Top = 0
        Me.Detail.Height = 20
        'L0194 ==
    End Sub

End Class