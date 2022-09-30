Option Strict Off

Imports System.Data
Imports System.Data.odbc
Imports clsDataProvider


Public Class xrLogs
    Inherits DevExpress.XtraReports.UI.XtraReport
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
    Private Sub UpdateBinding()
        '' Create a connection.
        'Dim Connection As New OdbcConnection(CreateConnectionString(SQLServerName, SQLUserName, SQLUserPassword, SQLMasterDB, ecDBConnectionType.ODBC, ecDBServerType.MSSQL))
        '' Create a data adapter and a dataset.
        'Dim Adapter As New OdbcDataAdapter("SELECT * FROM Logs ORDER BY Logs.dt DESC", Connection)
        ''Dim Adapter As New OdbcDataAdapter("SELECT * FROM Logs ORDER BY Logs.dt DESC", App.Connection)
        'Dim DataSet1 As New DataSet()
        '' Specify the data adapter and the data source for the report.
        '' Note that you must fill the datasource with data because it is not bound directly to the specified data adapter.
        'Me.DataAdapter = Adapter
        'Me.DataAdapter.Fill(DataSet1, "Logs")
        'Me.DataSource = DataSet1
        Dim dp As New clsDataProvider(Nothing)
        Me.DataSource = dp.dsGlobalLogs
        'Me.DataMember = "Logs"
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032
        Me.xrTableCell1.DataBindings.Add("Text", DataSource, "Logs.id")
        Me.xrTableCell2.DataBindings.Add("Text", DataSource, "Logs.dt")
        Me.xrTableCell3.DataBindings.Add("Text", DataSource, "Logs.Object")
        Me.xrTableCell4.DataBindings.Add("Text", DataSource, "Logs.Action")
        Me.xrTableCell5.DataBindings.Add("Text", DataSource, "Logs.Comment")
        Me.xrTableCell6.DataBindings.Add("Text", DataSource, "Logs.UserEmail")
        Me.Margins.Right = 50
    End Sub
#Region " Designer generated code "

    Public Sub New(ByVal ReportTitle As String) 'L0032
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle 'L0032
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
    Private WithEvents xrTable1 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow1 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell1 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell2 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell3 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell4 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell5 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell6 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrPageInfo2 As DevExpress.XtraReports.UI.XRPageInfo
    Private WithEvents ReportHeader As DevExpress.XtraReports.UI.ReportHeaderBand
    Private WithEvents xrTable2 As DevExpress.XtraReports.UI.XRTable
    Private WithEvents xrTableRow2 As DevExpress.XtraReports.UI.XRTableRow
    Private WithEvents xrTableCell7 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell8 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell9 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell10 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell11 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents xrTableCell12 As DevExpress.XtraReports.UI.XRTableCell
    Private WithEvents subreport1 As DevExpress.XtraReports.UI.XRSubreport  ' D0233



    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrLogs.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.xrTable1 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow1 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell1 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell2 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell3 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell4 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell5 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell6 = New DevExpress.XtraReports.UI.XRTableCell
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.xrTable2 = New DevExpress.XtraReports.UI.XRTable
        Me.xrTableRow2 = New DevExpress.XtraReports.UI.XRTableRow
        Me.xrTableCell7 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell8 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell9 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell10 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell11 = New DevExpress.XtraReports.UI.XRTableCell
        Me.xrTableCell12 = New DevExpress.XtraReports.UI.XRTableCell
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.xrPageInfo2 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport   ' D0233
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrTable1})
        Me.Detail.Height = 20
        Me.Detail.Name = "Detail"
        '
        'xrTable1
        '
        Me.xrTable1.BorderColor = System.Drawing.SystemColors.ControlDark
        Me.xrTable1.Borders = CType(((DevExpress.XtraPrinting.BorderSide.Left Or DevExpress.XtraPrinting.BorderSide.Right) _
                    Or DevExpress.XtraPrinting.BorderSide.Bottom), DevExpress.XtraPrinting.BorderSide)
        Me.xrTable1.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTable1.Location = New System.Drawing.Point(0, 0)
        Me.xrTable1.Name = "xrTable1"
        Me.xrTable1.ParentStyleUsing.UseBorderColor = False
        Me.xrTable1.ParentStyleUsing.UseBorders = False
        Me.xrTable1.ParentStyleUsing.UseFont = False
        Me.xrTable1.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow1})
        Me.xrTable1.Size = New System.Drawing.Size(650, 20)
        '
        'xrTableRow1
        '
        Me.xrTableRow1.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell1, Me.xrTableCell2, Me.xrTableCell3, Me.xrTableCell4, Me.xrTableCell5, Me.xrTableCell6})
        Me.xrTableRow1.Name = "xrTableRow1"
        Me.xrTableRow1.Size = New System.Drawing.Size(646, 23)
        '
        'xrTableCell1
        '
        Me.xrTableCell1.Location = New System.Drawing.Point(0, 0)
        Me.xrTableCell1.Name = "xrTableCell1"
        Me.xrTableCell1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell1.Size = New System.Drawing.Size(33, 23)
        Me.xrTableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
        '
        'xrTableCell2
        '
        Me.xrTableCell2.Location = New System.Drawing.Point(33, 0)
        Me.xrTableCell2.Name = "xrTableCell2"
        Me.xrTableCell2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell2.Size = New System.Drawing.Size(75, 23)
        '
        'xrTableCell3
        '
        Me.xrTableCell3.Location = New System.Drawing.Point(108, 0)
        Me.xrTableCell3.Name = "xrTableCell3"
        Me.xrTableCell3.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell3.Size = New System.Drawing.Size(134, 23)
        '
        'xrTableCell4
        '
        Me.xrTableCell4.Location = New System.Drawing.Point(242, 0)
        Me.xrTableCell4.Name = "xrTableCell4"
        Me.xrTableCell4.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell4.Size = New System.Drawing.Size(141, 23)
        '
        'xrTableCell5
        '
        Me.xrTableCell5.Location = New System.Drawing.Point(383, 0)
        Me.xrTableCell5.Name = "xrTableCell5"
        Me.xrTableCell5.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell5.Size = New System.Drawing.Size(134, 23)
        '
        'xrTableCell6
        '
        Me.xrTableCell6.Location = New System.Drawing.Point(517, 0)
        Me.xrTableCell6.Name = "xrTableCell6"
        Me.xrTableCell6.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell6.Size = New System.Drawing.Size(129, 23)
        '
        'PageHeader
        '
        Me.PageHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrTable2})
        Me.PageHeader.Height = 20
        Me.PageHeader.Name = "PageHeader"
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
        Me.xrTable2.ParentStyleUsing.UseBackColor = False
        Me.xrTable2.ParentStyleUsing.UseBorderColor = False
        Me.xrTable2.ParentStyleUsing.UseBorders = False
        Me.xrTable2.ParentStyleUsing.UseFont = False
        Me.xrTable2.Rows.AddRange(New DevExpress.XtraReports.UI.XRTableRow() {Me.xrTableRow2})
        Me.xrTable2.Size = New System.Drawing.Size(650, 20)
        Me.xrTable2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableRow2
        '
        Me.xrTableRow2.Cells.AddRange(New DevExpress.XtraReports.UI.XRTableCell() {Me.xrTableCell7, Me.xrTableCell8, Me.xrTableCell9, Me.xrTableCell10, Me.xrTableCell11, Me.xrTableCell12})
        Me.xrTableRow2.Name = "xrTableRow2"
        Me.xrTableRow2.Size = New System.Drawing.Size(646, 23)
        '
        'xrTableCell7
        '
        Me.xrTableCell7.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell7.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell7.Location = New System.Drawing.Point(0, 0)
        Me.xrTableCell7.Name = "xrTableCell7"
        Me.xrTableCell7.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell7.ParentStyleUsing.UseBackColor = False
        Me.xrTableCell7.ParentStyleUsing.UseFont = False
        Me.xrTableCell7.Size = New System.Drawing.Size(33, 23)
        Me.xrTableCell7.Text = "ID"
        Me.xrTableCell7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableCell8
        '
        Me.xrTableCell8.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell8.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell8.Location = New System.Drawing.Point(33, 0)
        Me.xrTableCell8.Name = "xrTableCell8"
        Me.xrTableCell8.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell8.ParentStyleUsing.UseBackColor = False
        Me.xrTableCell8.ParentStyleUsing.UseFont = False
        Me.xrTableCell8.Size = New System.Drawing.Size(75, 23)
        Me.xrTableCell8.Text = "Date Time"
        Me.xrTableCell8.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableCell9
        '
        Me.xrTableCell9.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell9.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell9.Location = New System.Drawing.Point(108, 0)
        Me.xrTableCell9.Name = "xrTableCell9"
        Me.xrTableCell9.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell9.ParentStyleUsing.UseBackColor = False
        Me.xrTableCell9.ParentStyleUsing.UseFont = False
        Me.xrTableCell9.Size = New System.Drawing.Size(134, 23)
        Me.xrTableCell9.Text = "Object"
        Me.xrTableCell9.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableCell10
        '
        Me.xrTableCell10.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell10.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell10.Location = New System.Drawing.Point(242, 0)
        Me.xrTableCell10.Name = "xrTableCell10"
        Me.xrTableCell10.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell10.ParentStyleUsing.UseBackColor = False
        Me.xrTableCell10.ParentStyleUsing.UseFont = False
        Me.xrTableCell10.Size = New System.Drawing.Size(141, 23)
        Me.xrTableCell10.Text = "Action"
        Me.xrTableCell10.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableCell11
        '
        Me.xrTableCell11.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell11.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell11.Location = New System.Drawing.Point(383, 0)
        Me.xrTableCell11.Name = "xrTableCell11"
        Me.xrTableCell11.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell11.ParentStyleUsing.UseBackColor = False
        Me.xrTableCell11.ParentStyleUsing.UseFont = False
        Me.xrTableCell11.Size = New System.Drawing.Size(134, 23)
        Me.xrTableCell11.Text = "Comments"
        Me.xrTableCell11.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrTableCell12
        '
        Me.xrTableCell12.BackColor = System.Drawing.Color.AliceBlue
        Me.xrTableCell12.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrTableCell12.Location = New System.Drawing.Point(517, 0)
        Me.xrTableCell12.Name = "xrTableCell12"
        Me.xrTableCell12.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrTableCell12.ParentStyleUsing.UseBackColor = False
        Me.xrTableCell12.ParentStyleUsing.UseFont = False
        Me.xrTableCell12.Size = New System.Drawing.Size(129, 23)
        Me.xrTableCell12.Text = "User E-mail"
        Me.xrTableCell12.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'PageFooter
        '
        Me.PageFooter.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrPageInfo2})
        Me.PageFooter.Height = 25
        Me.PageFooter.Name = "PageFooter"
        '
        'xrPageInfo2
        '
        Me.xrPageInfo2.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrPageInfo2.Location = New System.Drawing.Point(520, 0)
        Me.xrPageInfo2.Name = "xrPageInfo2"
        Me.xrPageInfo2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrPageInfo2.PageInfo = DevExpress.XtraPrinting.PageInfo.Number
        Me.xrPageInfo2.ParentStyleUsing.UseFont = False
        Me.xrPageInfo2.Size = New System.Drawing.Size(130, 25)
        Me.xrPageInfo2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight
        '
        'ReportHeader
        '
        Me.ReportHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.subreport1})
        Me.ReportHeader.Height = 33
        Me.ReportHeader.Name = "ReportHeader"
        '
        'subreport1
        '
        Me.subreport1.Location = New System.Drawing.Point(0, 0)
        Me.subreport1.Name = "subreport1"
        '
        'xrLogs
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageHeader, Me.PageFooter, Me.ReportHeader})
        CType(Me.xrTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.xrTable2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region

End Class