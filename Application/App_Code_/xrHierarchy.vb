Option Strict Off

Imports System.Data
Imports System.Data.odbc
Imports ExpertChoice.Web
Imports System.Windows.Forms
Imports clsDataProvider



Public Class xrHierarchy
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

    Public Sub UpdateBinding(ByRef CPM As clsProjectManager)

        'L0001 ===
        Dim dp As New clsDataProvider(CPM)
        Me.subreport1.ReportSource = New xrReportTitle(ReportTitle) 'L0032

        Dim dr As DataTableReader = dp.dsStructureOverview.Tables("HierarchyStructure").CreateDataReader()
        Dim i As Integer = 0

        While dr.Read()
            If Not CBool(dr("IsAlternative").ToString) Then
                i += 1
                If CInt(dr("ParentNodeID").ToString) = -1 Then
                    Me.treeView1.Nodes.Add(dr("NodeID").ToString, dr("NodeName").ToString)
                Else
                    If Me.treeView1.Nodes.Find(dr("ParentNodeID").ToString, True).Length > 0 Then
                        Me.treeView1.Nodes.Find(dr("ParentNodeID").ToString, True)(0).Nodes.Add(dr("NodeID").ToString, dr("NodeName").ToString)
                    End If
                End If
            End If
        End While
        dr.Close()

        Me.treeView1.ExpandAll()
        Me.treeView1.Height = Me.treeView1.ItemHeight() * i

        'How to Draw Control on xrPictureBox

        'Me.xrPictureBox1.Height = Me.treeView1.Height
        'Me.xrPictureBox1.Image = New System.Drawing.Bitmap(Me.treeView1.Size.Width, Me.treeView1.Size.Height)
        'Dim bmp As System.Drawing.Bitmap = Me.xrPictureBox1.Image
        'Me.treeView1.DrawToBitmap(bmp, Me.treeView1.DisplayRectangle)

        dr = dp.dsStructureOverview.Tables("AlternativesList").CreateDataReader()
        i = 1
        'Me.treeView2.Nodes.Add("-1", "Alternatives") 'L0188
        While dr.Read()
            i += 1
            Me.treeView2.Nodes.Add(dr("AltID").ToString, dr("AltName").ToString) 'L0188
        End While
        dr.Close()

        Me.treeView2.ExpandAll()
        Me.treeView2.Height = Me.treeView2.ItemHeight() * i
        Me.Margins.Right = 50
        'L0001 ==
    End Sub

#Region " Designer generated code "

    Public Sub New(ByRef CPM As clsProjectManager, ByVal ReportTitle As String, sObjName As String, sAltName As String) 'L0032 + D2463
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ReportTitle = ReportTitle 'L0032
        xrLabel1.Text = sObjName    ' D2463
        xrLabel2.Text = sAltName    ' D2463
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
    Private WithEvents winControlContainer1 As DevExpress.XtraReports.UI.WinControlContainer
    Private WithEvents treeView1 As TreeView
    Private WithEvents winControlContainer2 As DevExpress.XtraReports.UI.WinControlContainer
    Private WithEvents treeView2 As TreeView
    Private WithEvents ReportHeader As DevExpress.XtraReports.UI.ReportHeaderBand
    Private WithEvents subreport1 As DevExpress.XtraReports.UI.XRSubreport
    Private WithEvents xrLabel1 As DevExpress.XtraReports.UI.XRLabel
    Private WithEvents xrLabel2 As DevExpress.XtraReports.UI.XRLabel
    ' D0233

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrHierarchy.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.winControlContainer2 = New DevExpress.XtraReports.UI.WinControlContainer
        Me.treeView2 = New TreeView
        Me.winControlContainer1 = New DevExpress.XtraReports.UI.WinControlContainer
        Me.treeView1 = New TreeView
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.PageFooter = New DevExpress.XtraReports.UI.PageFooterBand
        Me.ReportHeader = New DevExpress.XtraReports.UI.ReportHeaderBand
        Me.xrLabel2 = New DevExpress.XtraReports.UI.XRLabel
        Me.xrLabel1 = New DevExpress.XtraReports.UI.XRLabel
        Me.subreport1 = New DevExpress.XtraReports.UI.XRSubreport
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.winControlContainer2, Me.winControlContainer1})
        Me.Detail.Height = 134
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'winControlContainer2
        '
        Me.winControlContainer2.BackColor = System.Drawing.Color.Transparent
        Me.winControlContainer2.BorderColor = System.Drawing.SystemColors.ControlText
        Me.winControlContainer2.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.winControlContainer2.BorderWidth = 1
        Me.winControlContainer2.DrawMethod = DevExpress.XtraReports.UI.WinControlDrawMethod.UseWMPrint
        Me.winControlContainer2.Font = New System.Drawing.Font("Times New Roman", 9.75!)
        Me.winControlContainer2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.winControlContainer2.ImageType = DevExpress.XtraReports.UI.WinControlImageType.Bitmap
        Me.winControlContainer2.Location = New System.Drawing.Point(333, 8)
        Me.winControlContainer2.Name = "winControlContainer2"
        Me.winControlContainer2.Size = New System.Drawing.Size(317, 117)
        Me.winControlContainer2.WinControl = Me.treeView2
        '
        'treeView2
        '
        Me.treeView2.BackColor = System.Drawing.Color.White
        Me.treeView2.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.treeView2.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.treeView2.Location = New System.Drawing.Point(0, 0)
        Me.treeView2.Name = "treeView2"
        Me.treeView2.Scrollable = False
        Me.treeView2.ShowPlusMinus = False
        Me.treeView2.ShowRootLines = False
        Me.treeView2.Size = New System.Drawing.Size(304, 112)
        Me.treeView2.TabIndex = 1
        '
        'winControlContainer1
        '
        Me.winControlContainer1.BackColor = System.Drawing.Color.Transparent
        Me.winControlContainer1.BorderColor = System.Drawing.SystemColors.ControlText
        Me.winControlContainer1.Borders = DevExpress.XtraPrinting.BorderSide.None
        Me.winControlContainer1.BorderWidth = 1
        Me.winControlContainer1.DrawMethod = DevExpress.XtraReports.UI.WinControlDrawMethod.UseWMPaintRecursive
        Me.winControlContainer1.Font = New System.Drawing.Font("Times New Roman", 9.75!)
        Me.winControlContainer1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.winControlContainer1.Location = New System.Drawing.Point(0, 8)
        Me.winControlContainer1.Name = "winControlContainer1"
        Me.winControlContainer1.Size = New System.Drawing.Size(308, 117)
        Me.winControlContainer1.WinControl = Me.treeView1
        '
        'treeView1
        '
        Me.treeView1.BackColor = System.Drawing.Color.White
        Me.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.treeView1.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.treeView1.LineColor = System.Drawing.Color.DarkBlue
        Me.treeView1.Location = New System.Drawing.Point(0, 0)
        Me.treeView1.Name = "treeView1"
        Me.treeView1.Scrollable = False
        Me.treeView1.ShowNodeToolTips = True
        Me.treeView1.ShowPlusMinus = False
        Me.treeView1.ShowRootLines = False
        Me.treeView1.Size = New System.Drawing.Size(296, 112)
        Me.treeView1.TabIndex = 0
        '
        'PageHeader
        '
        Me.PageHeader.BorderWidth = 1
        Me.PageHeader.Height = 10
        Me.PageHeader.Name = "PageHeader"
        Me.PageHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.PageHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'PageFooter
        '
        Me.PageFooter.Height = 19
        Me.PageFooter.Name = "PageFooter"
        Me.PageFooter.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.PageFooter.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'ReportHeader
        '
        Me.ReportHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.subreport1, Me.xrLabel1, Me.xrLabel2})
        Me.ReportHeader.Height = 75
        Me.ReportHeader.Name = "ReportHeader"
        Me.ReportHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.ReportHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrLabel2
        '
        Me.xrLabel2.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Underline)
        Me.xrLabel2.Location = New System.Drawing.Point(333, 42)
        Me.xrLabel2.Name = "xrLabel2"
        Me.xrLabel2.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabel2.Size = New System.Drawing.Size(317, 25)
        Me.xrLabel2.StylePriority.UseFont = False
        Me.xrLabel2.Text = "Alternatives"
        '
        'xrLabel1
        '
        Me.xrLabel1.Font = New System.Drawing.Font("Tahoma", 14.0!, System.Drawing.FontStyle.Underline)
        Me.xrLabel1.Location = New System.Drawing.Point(0, 42)
        Me.xrLabel1.Name = "xrLabel1"
        Me.xrLabel1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabel1.Size = New System.Drawing.Size(308, 25)
        Me.xrLabel1.StylePriority.UseFont = False
        Me.xrLabel1.Text = "Objectives"
        '
        'subreport1
        '
        Me.subreport1.Location = New System.Drawing.Point(0, 0)
        Me.subreport1.Name = "subreport1"
        Me.subreport1.Size = New System.Drawing.Size(100, 23)
        '
        'xrHierarchy
        '
        Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Detail, PageHeader, PageFooter, ReportHeader})
        Me.Version = "8.3"
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand
    Public WithEvents PageFooter As DevExpress.XtraReports.UI.PageFooterBand

#End Region

End Class