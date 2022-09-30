Option Strict Off

Imports System.Data
Imports System.Data.odbc
Imports clsDataProvider
Imports DevExpress.XtraReports.UI
Imports xrJudgements

Public Class xrRatingJudgements
    Inherits DevExpress.XtraReports.UI.XtraReport

    Private PManager As New clsProjectManager
    Private WithEvents xrTable1 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow1 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell2 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell3 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell5 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrLabel1 As DevExpress.XtraReports.UI.XRLabel
    Private NodeId As Integer
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
        Me.DataMember = "AltsRatingsJudgments"
        Dim grField As GroupField = New GroupField()
        grField.FieldName = "UserName"
        Me.GroupHeader2.GroupFields.Add(grField)

        Me.xrTableCell7.DataBindings.Add("Text", DataSource, "AltsRatingsJudgments.UserName")
        Me.xrTableCell2.DataBindings.Add("Text", DataSource, "AltsRatingsJudgments.AltName")
        Me.xrTableCell3.DataBindings.Add("Text", DataSource, "AltsRatingsJudgments.RatingName")
        Me.xrTableCell5.DataBindings.Add("Text", DataSource, "AltsRatingsJudgments.Value", "{0:N2}")
        'Me.xrTableCell4.DataBindings.Add("Text", DataSource, "AltsRatingsJudgments.Comment") 'L0190
        Me.xrLabel1.DataBindings.Add("Text", DataSource, "AltsRatingsJudgments.Comment") 'L0209
        Me.Margins.Right = 50

    End Sub
#Region " Designer generated code "

    Public Sub New(ByRef DS As DataSet)
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()
        'ProjectManager = CPM
        Me.DataSource = DS
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

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrRatingJudgements.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.xrTable1 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow1 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell2 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell3 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell5 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTable2 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow2 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell7 = New DevExpress.XtraReports.UI.XRTableCell
        Me.GroupHeader2 = New DevExpress.XtraReports.UI.GroupHeaderBand
        Me.xrLabel1 = New DevExpress.XtraReports.UI.XRLabel
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Borders = CType((((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Top) _
                    Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrLabel1, Me.xrTable1})
        Me.Detail.Height = 41
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTable1
        '
        Me.xrTable1.BorderColor = System.Drawing.SystemColors.ButtonFace
        Me.xrTable1.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTable1.Location = New System.Drawing.Point(0, 0)
        Me.xrTable1.Name = "xrTable1"
        Me.xrTable1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 96.0!)
        Me.xrTable1.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow1})
        Me.xrTable1.Size = New System.Drawing.Size(650, 20)
        Me.xrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrTableRow1
        '
        Me.xrTableRow1.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell2, Me.xrTableCell3, Me.xrTableCell5})
        Me.xrTableRow1.Name = "xrTableRow1"
        Me.xrTableRow1.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 96.0!)
        Me.xrTableRow1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        Me.xrTableRow1.Weight = 1
        '
        'xrTableCell2
        '
        Me.xrTableCell2.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell2.Name = "xrTableCell2"
        Me.xrTableCell2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell2.Text = "xrTableCell2"
        Me.xrTableCell2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell2.Weight = 0.5075550295857989
        '
        'xrTableCell3
        '
        Me.xrTableCell3.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell3.Name = "xrTableCell3"
        Me.xrTableCell3.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell3.Text = "xrTableCell3"
        Me.xrTableCell3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell3.Weight = 0.35136568047337291
        '
        'xrTableCell5
        '
        Me.xrTableCell5.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell5.Name = "xrTableCell5"
        Me.xrTableCell5.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell5.Text = "xrTableCell5"
        Me.xrTableCell5.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft
        Me.xrTableCell5.Weight = 0.1549254437869822
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
        'xrLabel1
        '
        Me.xrLabel1.BorderColor = System.Drawing.SystemColors.ButtonFace
        Me.xrLabel1.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrLabel1.Font = New System.Drawing.Font("Tahoma", 8.25!)
        Me.xrLabel1.Location = New System.Drawing.Point(0, 21)
        Me.xrLabel1.Multiline = True
        Me.xrLabel1.Name = "xrLabel1"
        Me.xrLabel1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96.0!)
        Me.xrLabel1.ProcessNullValues = DevExpress.XtraReports.UI.ValueSuppressType.SuppressAndShrink
        Me.xrLabel1.Size = New System.Drawing.Size(650, 20)
        Me.xrLabel1.StylePriority.UseBorderColor = False
        Me.xrLabel1.StylePriority.UseBorders = False
        Me.xrLabel1.StylePriority.UseFont = False
        Me.xrLabel1.Text = "Comments"
        '
        'xrRatingJudgements
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.GroupHeader2})
        Me.Version = "9.1"
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand

#End Region

    Private Sub xrJudgements_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs) Handles Me.BeforePrint
        UpdateBinding()
        Dim dv As DataView
        Dim ds As DataSet = Me.DataSource
        dv = ds.Tables("AltsRatingsJudgments").DefaultView
        dv.RowFilter = "CovObjID=" + NodeId.ToString
        Me.DataSource = dv
    End Sub
End Class