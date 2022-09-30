Option Strict Off

Imports System.Data
Imports System.Data.odbc
Imports clsDataProvider
Imports DevExpress.XtraReports.UI
Imports xrJudgements

Public Class xrJudgements
    Inherits DevExpress.XtraReports.UI.XtraReport
    Private PManager As New clsProjectManager
    Private WithEvents ReportFooter As DevExpress.XtraReports.UI.ReportFooterBand
    Private WithEvents xrShape1 As DevExpress.XtraReports.UI.XRShape
    Private WithEvents xrShape2 As DevExpress.XtraReports.UI.XRShape
    Private WithEvents xrLabel5 As DevExpress.XtraReports.UI.XRLabel
    Private NodeId As Integer = 0

    Private Property ProjectManager() As clsProjectManager
        Get
            Return PManager
        End Get
        Set(ByVal value As clsProjectManager)
            PManager = value
        End Set
    End Property
    Public Property CurrentNodeID() As Integer
        Get
            Return NodeId
        End Get
        Set(ByVal value As Integer)
            NodeId = value
        End Set
    End Property
    Private Sub UpdateBinding()
        ' Dim dp As New clsDataProvider(PM)
        'Me.DataSource = ProjectManager.ProjectDataProvider.ReportDataSet(CanvasReportType.crtAllJudgments) 'L0215
        Me.DataMember = "AllPairwiseJudgments"
        Dim grField As GroupField = New GroupField()
        grField.FieldName = "UserName"
        Me.GroupHeader2.GroupFields.Add(grField)
        ' grField = New GroupField()
        ' grField.FieldName = "NodeID"
        ' Me.GroupHeader1.GroupFields.Add(grField)

        Me.xrTableCell7.DataBindings.Add("Text", DataSource, "AllPairwiseJudgments.UserName")
        Me.xrLabel2.DataBindings.Add("Text", DataSource, "AllPairwiseJudgments.Child1Name")
        Me.xrLabel3.DataBindings.Add("Text", DataSource, "AllPairwiseJudgments.Child2Name")
        'Me.xrLabel4.DataBindings.Add("Text", DataSource, "AllPairwiseJudgments.Value", "{0:N2}")
        Me.xrLabel5.DataBindings.Add("Text", DataSource, "AllPairwiseJudgments.Comment") 'L0190 'L0209
        'Me.xrJudgementBar1.DataBindings.Add("Position", DataSource, "AllPairwiseJudgments.Value")
        Me.Margins.Right = 50
    End Sub
#Region " Designer generated code "

    Public Sub New(ByRef DS As DataSet)
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()
        Me.DataSource = DS
        'ProjectManager = CPM
        'Add any initialization after the InitializeComponent() call
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
    'Private WithEvents logsTableAdapter1 As dsLogsTableAdapters.LogsTableAdapter
    '  Private WithEvents dsLogs1 As dsLogs
    Private WithEvents xrTable2 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow2 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell7 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents GroupHeader2 As DevExpress.XtraReports.UI.GroupHeaderBand
    Private WithEvents xrLabel2 As DevExpress.XtraReports.UI.XRLabel
    Private WithEvents xrLabel3 As DevExpress.XtraReports.UI.XRLabel
    Private WithEvents xrLabel4 As DevExpress.XtraReports.UI.XRLabel

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrJudgements.resx"
        Dim shapeRectangle1 As DevExpress.XtraPrinting.Shape.ShapeRectangle = New DevExpress.XtraPrinting.Shape.ShapeRectangle
        Dim shapeRectangle2 As DevExpress.XtraPrinting.Shape.ShapeRectangle = New DevExpress.XtraPrinting.Shape.ShapeRectangle
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.xrShape2 = New DevExpress.XtraReports.UI.XRShape
        Me.xrLabel4 = New DevExpress.XtraReports.UI.XRLabel
        Me.xrLabel3 = New DevExpress.XtraReports.UI.XRLabel
        Me.xrLabel2 = New DevExpress.XtraReports.UI.XRLabel
        Me.xrShape1 = New DevExpress.XtraReports.UI.XRShape
        Me.xrTable2 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow2 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell7 = New DevExpress.XtraReports.UI.XRTableCell
        Me.GroupHeader2 = New DevExpress.XtraReports.UI.GroupHeaderBand
        Me.ReportFooter = New DevExpress.XtraReports.UI.ReportFooterBand
        Me.xrLabel5 = New DevExpress.XtraReports.UI.XRLabel
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrLabel5, Me.xrShape2, Me.xrLabel4, Me.xrLabel3, Me.xrLabel2, Me.xrShape1})
        Me.Detail.Height = 42
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrShape2
        '
        Me.xrShape2.BackColor = System.Drawing.Color.Blue
        Me.xrShape2.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.xrShape2.BorderWidth = 1
        Me.xrShape2.LineWidth = 0
        Me.xrShape2.Location = New System.Drawing.Point(200, 2)
        Me.xrShape2.Name = "xrShape2"
        Me.xrShape2.Shape = shapeRectangle1
        Me.xrShape2.Size = New System.Drawing.Size(100, 16)
        Me.xrShape2.StylePriority.UseBackColor = False
        Me.xrShape2.StylePriority.UseBorders = False
        Me.xrShape2.StylePriority.UseBorderWidth = False
        '
        'xrLabel4
        '
        Me.xrLabel4.BorderColor = System.Drawing.SystemColors.ButtonFace
        Me.xrLabel4.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrLabel4.BorderWidth = 1
        Me.xrLabel4.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrLabel4.Location = New System.Drawing.Point(300, 0)
        Me.xrLabel4.Name = "xrLabel4"
        Me.xrLabel4.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabel4.Size = New System.Drawing.Size(50, 20)
        Me.xrLabel4.StylePriority.UseBorderWidth = False
        Me.xrLabel4.StylePriority.UseTextAlignment = False
        Me.xrLabel4.Text = "xrLabel4"
        Me.xrLabel4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrLabel3
        '
        Me.xrLabel3.BorderColor = System.Drawing.SystemColors.ButtonFace
        Me.xrLabel3.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrLabel3.BorderWidth = 1
        Me.xrLabel3.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrLabel3.Location = New System.Drawing.Point(450, 0)
        Me.xrLabel3.Name = "xrLabel3"
        Me.xrLabel3.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabel3.Size = New System.Drawing.Size(200, 20)
        Me.xrLabel3.StylePriority.UseBorderWidth = False
        Me.xrLabel3.Text = "xrLabel3"
        Me.xrLabel3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        '
        'xrLabel2
        '
        Me.xrLabel2.BorderColor = System.Drawing.SystemColors.ButtonFace
        Me.xrLabel2.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrLabel2.BorderWidth = 1
        Me.xrLabel2.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrLabel2.Location = New System.Drawing.Point(0, 0)
        Me.xrLabel2.Name = "xrLabel2"
        Me.xrLabel2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabel2.Size = New System.Drawing.Size(200, 20)
        Me.xrLabel2.StylePriority.UseBorders = False
        Me.xrLabel2.StylePriority.UseBorderWidth = False
        Me.xrLabel2.Text = "xrLabel2"
        Me.xrLabel2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        '
        'xrShape1
        '
        Me.xrShape1.BackColor = System.Drawing.Color.Red
        Me.xrShape1.Borders = DevExpress.XtraPrinting.BorderSide.Left
        Me.xrShape1.LineWidth = 0
        Me.xrShape1.Location = New System.Drawing.Point(350, 2)
        Me.xrShape1.Name = "xrShape1"
        Me.xrShape1.Shape = shapeRectangle2
        Me.xrShape1.Size = New System.Drawing.Size(100, 16)
        Me.xrShape1.StylePriority.UseBackColor = False
        Me.xrShape1.StylePriority.UseBorders = False
        '
        'xrTable2
        '
        Me.xrTable2.BackColor = System.Drawing.Color.Gainsboro
        Me.xrTable2.BorderColor = System.Drawing.SystemColors.ControlDark
        Me.xrTable2.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTable2.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTable2.Location = New System.Drawing.Point(0, 0)
        Me.xrTable2.Name = "xrTable2"
        Me.xrTable2.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 96.0!)
        Me.xrTable2.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow2})
        Me.xrTable2.Size = New System.Drawing.Size(650, 20)
        Me.xrTable2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableRow2
        '
        Me.xrTableRow2.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell7})
        Me.xrTableRow2.Name = "xrTableRow2"
        Me.xrTableRow2.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 96.0!)
        Me.xrTableRow2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow2.Weight = 1
        '
        'xrTableCell7
        '
        Me.xrTableCell7.BackColor = System.Drawing.Color.Gainsboro
        Me.xrTableCell7.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell7.Name = "xrTableCell7"
        Me.xrTableCell7.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell7.Text = "UserName"
        Me.xrTableCell7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell7.Weight = 1
        '
        'GroupHeader2
        '
        Me.GroupHeader2.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrTable2})
        Me.GroupHeader2.Height = 20
        Me.GroupHeader2.Name = "GroupHeader2"
        Me.GroupHeader2.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.GroupHeader2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'ReportFooter
        '
        Me.ReportFooter.Height = 5
        Me.ReportFooter.Name = "ReportFooter"
        '
        'xrLabel5
        '
        Me.xrLabel5.BorderColor = System.Drawing.SystemColors.ButtonFace
        Me.xrLabel5.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrLabel5.BorderWidth = 1
        Me.xrLabel5.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrLabel5.Location = New System.Drawing.Point(0, 21)
        Me.xrLabel5.Multiline = True
        Me.xrLabel5.Name = "xrLabel5"
        Me.xrLabel5.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabel5.ProcessNullValues = DevExpress.XtraReports.UI.ValueSuppressType.SuppressAndShrink
        Me.xrLabel5.Size = New System.Drawing.Size(650, 20)
        Me.xrLabel5.StylePriority.UseBorders = False
        Me.xrLabel5.StylePriority.UseBorderWidth = False
        Me.xrLabel5.StylePriority.UseTextAlignment = False
        Me.xrLabel5.Text = "Comments"
        Me.xrLabel5.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrJudgements
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.GroupHeader2})
        Me.Borders = DevExpress.XtraPrinting.BorderSide.Left
        Me.BorderWidth = 0
        Me.Version = "9.1"
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand

#End Region

    Private Sub xrJudgements_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles Me.BeforePrint
        UpdateBinding()
        Dim dv As DataView
        Dim ds As DataSet = Me.DataSource
        dv = ds.Tables("AllPairwiseJudgments").DefaultView
        dv.RowFilter = "NodeID=" + NodeId.ToString
        Me.DataSource = dv
        Me.Visible = dv.Count <> 0
    End Sub

    'L0190 ==
    Private Sub xrLabel2_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles Detail.BeforePrint
        Dim AValue As Double = Me.GetCurrentColumnValue("Value")
        Dim AValueText As String = ""

        'L0371 ===
        If Math.Abs(AValue) > 99 Then AValue = AValue / Math.Abs(AValue) * 99 'Temporary

        Dim a, b As Double
        a = (100 * Math.Abs(AValue)) / (Math.Abs(AValue) + 1)
        b = 100 / (Math.Abs(AValue) + 1)

        AValueText = Math.Abs(AValue).ToString("F02")
        If AValue < 0 Then
            Dim c As Double
            c = a
            a = b
            b = c
            AValueText = "1/" + AValueText
        End If
        xrShape2.Width = a
        xrShape2.Left = 299 - a
        xrShape2.Visible = True

        xrShape1.Width = b
        xrShape1.Visible = True

        If AValue = 0 Then
            xrShape2.Width = 50
            xrShape2.Left = 299 - 50
            xrShape2.Visible = True

            xrShape1.Width = 50
            xrShape1.Visible = True
            AValueText = "1/1"
            'xrShape1.Visible = False
            'xrShape2.Visible = False
        End If

        'If AValue > 0 Then 'L0211
        '    'AValueText = "1/" + Math.Abs(AValue).ToString("F02") 'L0202
        '    AValueText = Math.Abs(AValue).ToString("F02") 'L0202
        '    xrShape1.Width = 0
        '    xrShape1.Visible = False 'L0202
        '    xrShape2.Visible = True 'L0202

        '    'xrShape2.Width = 100 * Math.Abs(AValue) / 10 'L0371
        '    'xrShape2.Left = 299 - 100 * Math.Abs(AValue) / 10 'L0371
        '    xrShape2.Width = a 'L0371
        '    xrShape2.Left = 299 - a 'L0371
        'Else
        '    AValueText = Math.Abs(AValue).ToString("F02") 'L0211
        '    'xrShape1.Width = 100 * Math.Abs(AValue) / 10 'L0211 'L0371
        '    xrShape1.Width = b 'L0371
        '    xrShape1.Visible = True 'L0202
        '    xrShape2.Visible = False 'L0202
        '    xrShape2.Left = 299
        '    xrShape2.Width = 0
        'End If
        'L0371 ==

        xrLabel4.Text = AValueText
    End Sub
    'L0190 ===
End Class