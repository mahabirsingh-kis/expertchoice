Option Strict Off

Imports System.Windows.Forms 'L0122
Imports System.Data
Imports Canvas

Public Class xrHierarchyObjectivesAndAlternatives
    Inherits DevExpress.XtraReports.UI.XtraReport
    'L0032 ===
    Private fReportTitle As String = ""
    Private WithEvents xrRichText1 As DevExpress.XtraReports.UI.XRRichText
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
        PM.ProjectDataProvider.FullAlternativePath = False
        Me.DataSource = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtHierarchyObjectivesAndAlternatives)
        Me.DataMember = "HierarchyStructure"
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032
        'L0101 ===
        If ReportOptions(0) = "0" Then
            Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodeName")
        Else
            Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodePath")
        End If
        'L0101 ==
        'Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodePath") 'L0101
        Me.xrTableCell2.DataBindings.Add("Text", DataSource, "HierarchyStructure.MeasurementType")
        'Me.xrRichText1.DataBindings.Add("Rtf", DataSource, "HierarchyStructure.Comment") 'L0110 'L0194
        Me.Margins.Right = 50
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodePath", System.Type.GetType("System.String"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("Comment", System.Type.GetType("System.String"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("MeasurementType", System.Type.GetType("System.String"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("IsAlternative", System.Type.GetType("System.Boolean"))
    End Sub

#Region " Designer generated code "

    Public Sub New(ByRef CPM As clsProjectManager, ByVal ReportTitle As String, ByVal ROptions As String) 'L0032 'L0101
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle 'L0032
        Me.ReportOptions = ROptions 'L0101
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
    Private WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand
    Private WithEvents xrTable2 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow2 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell3 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell4 As DevExpress.XtraReports.UI.XRTableCell

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrHierarchyObjectivesAndAlternatives.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.xrRichText1 = New DevExpress.XtraReports.UI.XRRichText
        Me.xrTable1 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow1 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell1 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell2 = New DevExpress.XtraReports.UI.XRTableCell
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.xrPageInfo1 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.xrTable2 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow2 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell3 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell4 = New DevExpress.XtraReports.UI.XRTableCell
        CType(Me.xrRichText1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrRichText1, Me.xrTable1})
        Me.Detail.Height = 45
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
        Me.xrTable1.BackColor = System.Drawing.Color.LightBlue
        Me.xrTable1.Location = New System.Drawing.Point(0, 0)
        Me.xrTable1.Name = "xrTable1"
        Me.xrTable1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTable1.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow1})
        Me.xrTable1.Size = New System.Drawing.Size(650, 20)
        Me.xrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTableRow1
        '
        Me.xrTableRow1.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell1, Me.xrTableCell2})
        Me.xrTableRow1.Name = "xrTableRow1"
        Me.xrTableRow1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTableRow1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow1.Weight = 1
        '
        'xrTableCell1
        '
        Me.xrTableCell1.BackColor = System.Drawing.Color.Transparent
        Me.xrTableCell1.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell1.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell1.Name = "xrTableCell1"
        Me.xrTableCell1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell1.StylePriority.UseBackColor = False
        Me.xrTableCell1.StylePriority.UseBorders = False
        Me.xrTableCell1.StylePriority.UseFont = False
        Me.xrTableCell1.Text = "xrTableCell1"
        Me.xrTableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell1.Weight = 0.8338461538461539
        '
        'xrTableCell2
        '
        Me.xrTableCell2.BackColor = System.Drawing.Color.Transparent
        Me.xrTableCell2.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell2.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell2.Name = "xrTableCell2"
        Me.xrTableCell2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell2.StylePriority.UseBackColor = False
        Me.xrTableCell2.StylePriority.UseBorders = False
        Me.xrTableCell2.StylePriority.UseFont = False
        Me.xrTableCell2.Text = "xrTableCell2"
        Me.xrTableCell2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell2.Weight = 0.16615384615384615
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
        Me.xrPageInfo1.Location = New System.Drawing.Point(542, 0)
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
        Me.ReportHeader.Height = 33
        Me.ReportHeader.Name = "ReportHeader"
        Me.ReportHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.ReportHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'PageHeader
        '
        Me.PageHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrTable2})
        Me.PageHeader.Height = 20
        Me.PageHeader.Name = "PageHeader"
        '
        'xrTable2
        '
        Me.xrTable2.BackColor = System.Drawing.Color.LightBlue
        Me.xrTable2.Location = New System.Drawing.Point(0, 0)
        Me.xrTable2.Name = "xrTable2"
        Me.xrTable2.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow2})
        Me.xrTable2.Size = New System.Drawing.Size(650, 20)
        '
        'xrTableRow2
        '
        Me.xrTableRow2.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell3, Me.xrTableCell4})
        Me.xrTableRow2.Name = "xrTableRow2"
        Me.xrTableRow2.Weight = 1
        '
        'xrTableCell3
        '
        Me.xrTableCell3.BackColor = System.Drawing.Color.LightBlue
        Me.xrTableCell3.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell3.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell3.Name = "xrTableCell3"
        Me.xrTableCell3.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell3.Text = "Node Path"
        Me.xrTableCell3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell3.Weight = 0.8584615384615385
        '
        'xrTableCell4
        '
        Me.xrTableCell4.BackColor = System.Drawing.Color.LightBlue
        Me.xrTableCell4.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Top Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell4.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell4.Name = "xrTableCell4"
        Me.xrTableCell4.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell4.Text = "Type"
        Me.xrTableCell4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell4.Weight = 0.14153846153846153
        '
        'xrHierarchyObjectivesAndAlternatives
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageFooter, Me.ReportHeader})
        Me.Version = "8.3"
        CType(Me.xrRichText1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region

    Dim LastSpaceCount As Integer = 0 'L0190

    Private Sub xrTable1_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles xrTable1.BeforePrint
        Dim fnt1 As New Drawing.Font("Tahoma", 12, Drawing.FontStyle.Regular) 'L0190
        Dim fnt2 As New Drawing.Font("Tahoma", 12, Drawing.FontStyle.Bold) 'L0190
        'L0101 ===

        Dim SpaceCount = CStr(CType(Me.GetCurrentRow(), DataRowView).Item("NodePath")).Split("|").Count() 'L0190
        If ReportOptions(0) = "0" Then

            xrTable1.Left = (SpaceCount - 1) * 20
            xrTable1.Width = 650 - (SpaceCount - 1) * 20.0
            xrTableCell1.Width = 542 - (SpaceCount - 1) * 20.0 'L0202
            xrTableCell2.Width = 108
            'xrRichText1.Left = (SpaceCount - 1) * 20 'L0194
            'xrRichText1.Width = 650 - (SpaceCount - 1) * 20.0 'L0194

            'For i As Integer = 1 To SpaceCount - 1
            '    xrTableCell1.Text = "        " + xrTableCell1.Text
            'Next
        End If
        'L0101 ==
        If Me.GetCurrentColumnValue("IsAlternative") Then
            'L0190 ==
            'Me.xrTableCell1.BackColor = Drawing.Color.Honeydew
            'Me.xrTableCell2.BackColor = Drawing.Color.Honeydew
            If ReportOptions(0) = "0" Then
                xrTable1.Left = (LastSpaceCount) * 20
                xrTable1.Width = 650 - (LastSpaceCount) * 20.0
                'xrRichText1.Left = (LastSpaceCount) * 20 'L0194
                'xrRichText1.Width = 650 - (LastSpaceCount) * 20.0 'L0194
            End If

            'L0190 ===
            Me.xrTableCell1.Font = fnt1
            Me.xrTableCell1.ForeColor = Drawing.Color.Blue 'L0202
        Else
            'L0190 ==
            'Me.xrTableCell1.BackColor = Drawing.Color.AliceBlue
            'Me.xrTableCell2.BackColor = Drawing.Color.AliceBlue
            LastSpaceCount = SpaceCount
            'L0190 ===
            Me.xrTableCell1.Font = fnt2
            Me.xrTableCell1.ForeColor = Drawing.Color.Black 'L0202
        End If

        'L0194 ===
        ''L0157 ===
        'Dim ARtfText As String = Me.GetCurrentColumnValue("Comment")
        'If ARtfText.Contains("rtf") Then
        '    'L0122 ===
        '    Dim ARTBox As New RichTextBox
        '    ARTBox.Rtf = ARtfText
        '    ARtfText = ARTBox.Text
        '    ARtfText = ARtfText.Trim()
        '    'L0122 ==
        'Else
        '    ARtfText = ""
        'End If
        ''L0157 ==

        'If ARtfText = "" Or ReportOptions(4) = "0" Then 'L0119 'L0122
        '    Me.xrRichText1.Visible = False 'L0110
        '    Me.xrRichText1.Top = 0 'L0110
        '    Me.Detail.Height = 20
        'Else
        '    Me.xrRichText1.Visible = True 'L0110
        '    Me.Detail.Height = 40
        '    Me.xrRichText1.Top = 20 'L0110
        'End If

        Me.xrRichText1.Visible = False
        Me.xrRichText1.Top = 0
        Me.Detail.Height = 20
        'L0194 ==
    End Sub

End Class