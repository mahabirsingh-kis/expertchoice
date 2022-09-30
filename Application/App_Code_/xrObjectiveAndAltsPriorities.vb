Option Strict Off

Imports System.Windows.Forms
Imports System.Data

Public Class xrObjectiveAndAltsPriorities

    Inherits DevExpress.XtraReports.UI.XtraReport
    'L0032 ===
    Private fReportTitle As String = ""
    Private WithEvents xrTableCell7 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell8 As DevExpress.XtraReports.UI.XRTableCell
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
        Me.DataSource = PM.ProjectDataProvider.ReportDataSet(CanvasReportType.crtObjectivesAndAlternativesPriorities)
        Me.DataMember = "StructureWithPriorities"
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032
        'L0101 ===
        If ReportOptions(0) = "0" Then
            Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodeName")
        Else
            Me.xrTableCell1.DataBindings.Add("Text", DataSource, "HierarchyStructure.NodePath")
        End If
        'L0101 ==
        'Me.xrTableCell1.DataBindings.Add("Text", DataSource, "StructureWithPriorities.NodePath")
        'Me.xrTableCell2.DataBindings.Add("Text", DataSource, "StructureWithPriorities.LocalPriority", "{0:P}") 'L0191
        'Me.xrTableCell3.DataBindings.Add("Text", DataSource, "StructureWithPriorities.GlobalPriority", "{0:P}") 'L0191
        Me.xrTableCell8.DataBindings.Add("Text", DataSource, "StructureWithPriorities.LocalPriority", "{0:P}") 'L0191
        'Me.xrRichText1.DataBindings.Add("Rtf", DataSource, "StructureWithPriorities.Comment") 'L0110 'L0194
        Me.Margins.Right = 50
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodeID", System.Type.GetType("System.Int32"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("NodePath", System.Type.GetType("System.String"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("Comment", System.Type.GetType("System.String"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("MeasurementType", System.Type.GetType("System.String"))
        'mDataSet.Tables(TABLE_NAME_HIERARCHY_STRUCTURE).Columns.Add("IsAlternative", System.Type.GetType("System.Boolean"))
    End Sub

#Region " Designer generated code "

    Public Sub New(ByRef CPM As clsProjectManager, ByVal ReportTitle As String, ByVal ROptions As String, sObjName As String, sAltName As String, sPrty As String) 'L0032 + L0101 + D2463 + D2590
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle 'L0032
        Me.ReportOptions = ROptions 'L0101
        xrTableCell4.Text = String.Format("{0} / {1}", sObjName, sAltName)  ' D2463
        xrTableCell7.Text = String.Format("{0} {1}", sAltName, sPrty) ' D2463 + D2590
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
    Private WithEvents xrPageInfo1 As DevExpress.XtraReports.UI.XRPageInfo
    Private WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand
    Private WithEvents xrTable2 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow2 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell4 As DevExpress.XtraReports.UI.XRTableCell

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrObjectiveAndAltsPriorities.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.xrRichText1 = New DevExpress.XtraReports.UI.XRRichText
        Me.xrTable1 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow1 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell1 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell8 = New DevExpress.XtraReports.UI.XRTableCell
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.xrPageInfo1 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.xrTable2 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow2 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell4 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell7 = New DevExpress.XtraReports.UI.XRTableCell
        CType(Me.xrRichText1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrRichText1, Me.xrTable1})
        Me.Detail.Height = 40
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
        Me.xrTable1.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTable1.Location = New System.Drawing.Point(0, 0)
        Me.xrTable1.Name = "xrTable1"
        Me.xrTable1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTable1.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow1})
        Me.xrTable1.Size = New System.Drawing.Size(650, 20)
        Me.xrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTableRow1
        '
        Me.xrTableRow1.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell1, Me.xrTableCell8})
        Me.xrTableRow1.Name = "xrTableRow1"
        Me.xrTableRow1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.xrTableRow1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow1.Weight = 1
        '
        'xrTableCell1
        '
        Me.xrTableCell1.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell1.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell1.Name = "xrTableCell1"
        Me.xrTableCell1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell1.StylePriority.UseBorders = False
        Me.xrTableCell1.Text = "xrTableCell1"
        Me.xrTableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell1.Weight = 0.70461538461538464
        '
        'xrTableCell8
        '
        Me.xrTableCell8.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrTableCell8.Font = New System.Drawing.Font("Tahoma", 8.25!)
        Me.xrTableCell8.Multiline = True
        Me.xrTableCell8.Name = "xrTableCell8"
        Me.xrTableCell8.StylePriority.UseBorders = False
        Me.xrTableCell8.StylePriority.UseFont = False
        Me.xrTableCell8.StylePriority.UseTextAlignment = False
        Me.xrTableCell8.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        Me.xrTableCell8.Weight = 0.29538461538461536
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
        Me.xrPageInfo1.Location = New System.Drawing.Point(558, 0)
        Me.xrPageInfo1.Name = "xrPageInfo1"
        Me.xrPageInfo1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrPageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.Number
        Me.xrPageInfo1.Size = New System.Drawing.Size(92, 25)
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
        Me.xrTableRow2.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell4, Me.xrTableCell7})
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
        Me.xrTableCell4.Text = "Objective / Alternative"
        Me.xrTableCell4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell4.Weight = 0.70461538461538464
        '
        'xrTableCell7
        '
        Me.xrTableCell7.BackColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(190, Byte), Integer), CType(CType(69, Byte), Integer))
        Me.xrTableCell7.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Top Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTableCell7.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell7.ForeColor = System.Drawing.Color.White
        Me.xrTableCell7.Name = "xrTableCell7"
        Me.xrTableCell7.StylePriority.UseBackColor = False
        Me.xrTableCell7.StylePriority.UseBorders = False
        Me.xrTableCell7.StylePriority.UseFont = False
        Me.xrTableCell7.StylePriority.UseForeColor = False
        Me.xrTableCell7.StylePriority.UseTextAlignment = False
        Me.xrTableCell7.Text = "Alternative Priority"
        Me.xrTableCell7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        Me.xrTableCell7.Weight = 0.29538461538461536
        '
        'xrObjectiveAndAltsPriorities
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageFooter, Me.ReportHeader, Me.PageHeader})
        Me.Version = "8.3"
        CType(Me.xrRichText1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Private WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region

    Dim LineSwitch As Boolean = True 'L0191
    Dim AColor As System.Drawing.Color = System.Drawing.Color.FromArgb(239, 243, 234) 'L0191
    Dim LastSpaceCount As Integer = 0 'L0191

    Private Sub xrTable1_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles xrTable1.BeforePrint
        'L0191 ===
        If LineSwitch Then
            AColor = System.Drawing.Color.FromArgb(222, 231, 209)
        Else
            AColor = System.Drawing.Color.FromArgb(239, 243, 234)
        End If
        LineSwitch = Not LineSwitch

        xrTableCell1.BackColor = AColor
        xrTableCell8.BackColor = AColor
        'L0191 ==

        'xrTableCell8.Text = "" 'L0119
        'L0101 ===
        Dim SpaceCount = CStr(CType(Me.GetCurrentRow(), DataRowView).Item("NodePath")).Split("|").Count()

        If ReportOptions(0) = "0" Then
            For i As Integer = 1 To SpaceCount - 1
                xrTableCell1.Text = "        " + xrTableCell1.Text
            Next
        End If
        'L0101 ==
        'L0107 ===
        If ReportOptions(1) = "0" Then
            'xrTableCell2.Visible = False 'L0191
        End If
        If ReportOptions(1) = "0" And ReportOptions(3) = "0" Then
            'xrTableCell1.Width += xrTableCell2.Width + xrTableCell7.Width 'L0191
            'xrTableCell4.Width += xrTableCell5.Width + xrTableCell8.Width 'L0191
            'xrTableCell5.Visible = False 'L0191
            'L0108 ===
        Else
            If ReportOptions(3) = "0" Then
                xrTableCell8.Visible = False
                xrTableCell7.Visible = False
                xrTableCell1.Width += xrTableCell7.Width
                xrTableCell4.Width += xrTableCell8.Width
            End If
            'L0108 ==
        End If
        If ReportOptions(2) = "0" Then
            'xrTableCell3.Visible = False 'L0191
            'xrTableCell6.Visible = False 'L0191
        End If
        'L0107 ==

        'L0191 ===
        If Me.GetCurrentColumnValue("IsAlternative") Then
            'Me.xrTableCell1.BackColor = Drawing.Color.Honeydew 
            'Me.xrTableCell2.BackColor = Drawing.Color.Honeydew 
            'L0108 ===
            'xrTableCell8.Text = xrTableCell2.Text 
            'xrTableCell2.Visible = False 

            'L0108 ==
            'Me.xrTableCell3.Visible = False 
            For i As Integer = 1 To LastSpaceCount
                xrTableCell1.Text = "        " + xrTableCell1.Text
            Next
        Else
            LastSpaceCount = SpaceCount
            xrTableCell8.Text = ""
            xrTableCell1.Text += "   "
            If ReportOptions(1) = "1" Then
                xrTableCell1.Text += String.Format("[L:{0:P2}]", Me.GetCurrentColumnValue("LocalPriority"))
            End If
            If ReportOptions(2) = "1" Then
                xrTableCell1.Text += String.Format(" [G:{0:P2}]", Me.GetCurrentColumnValue("GlobalPriority"))
            End If

            'Me.xrTableCell1.BackColor = Drawing.Color.AliceBlue 
            'Me.xrTableCell2.BackColor = Drawing.Color.AliceBlue 
            'If ReportOptions(1) = "1" Then xrTableCell2.Visible = True 'L0107 
            'If ReportOptions(2) = "1" Then xrTableCell3.Visible = True 'L0107 
        End If
        'L0191 ==

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

        'If ARtfText = "" Or ReportOptions(4) = "0" Then 'L0119 'L0122 'L0157
        '    Me.xrRichText1.Visible = False 'L0110
        '    Me.xrRichText1.Top = 0 'L0110
        '    Me.Detail.Height = 20
        'Else
        '    Me.Detail.Height = 40
        '    Me.xrRichText1.Visible = True 'L0110
        '    Me.xrRichText1.Top = 20 'L0110
        'End If
        Me.xrRichText1.Visible = False
        Me.xrRichText1.Top = 0
        Me.Detail.Height = 20
        'L0194 ==
    End Sub

End Class