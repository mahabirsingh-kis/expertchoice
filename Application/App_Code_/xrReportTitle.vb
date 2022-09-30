Public Class xrReportTitle
    Inherits DevExpress.XtraReports.UI.XtraReport

#Region " Designer generated code "

    Public Sub New(ByVal Title As String)
        MyBase.New()

        'This call is required by the Designer.
        InitializeComponent()
        Me.xrLabelTitle.Text = Title
        'Add any initialization after the InitializeComponent() call

    End Sub

    'XtraReport overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub
    Private WithEvents xrLabelTitle As DevExpress.XtraReports.UI.XRLabel
    Private WithEvents xrPageInfo1 As DevExpress.XtraReports.UI.XRPageInfo

    'Required by the Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Designer
    'It can be modified using the Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resourceFileName As String = "xrReportTitle.resx"
        Me.Detail = New DevExpress.XtraReports.UI.DetailBand
        Me.PageHeader = New DevExpress.XtraReports.UI.PageHeaderBand
        Me.xrPageInfo1 = New DevExpress.XtraReports.UI.XRPageInfo
        Me.xrLabelTitle = New DevExpress.XtraReports.UI.XRLabel
        CType(Me, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'Detail
        '
        Me.Detail.Height = 14
        Me.Detail.Name = "Detail"
        Me.Detail.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'PageHeader
        '
        Me.PageHeader.Controls.AddRange(New DevExpress.XtraReports.UI.XRControl() {Me.xrPageInfo1, Me.xrLabelTitle})
        Me.PageHeader.Height = 61
        Me.PageHeader.Name = "PageHeader"
        Me.PageHeader.Padding = New DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100.0!)
        Me.PageHeader.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft
        '
        'xrPageInfo1
        '
        Me.xrPageInfo1.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(204, Byte))
        Me.xrPageInfo1.Format = "{0:yyyy'-'MM'-'dd HH':'mm':'ss}"
        Me.xrPageInfo1.Location = New System.Drawing.Point(442, 0)
        Me.xrPageInfo1.Name = "xrPageInfo1"
        Me.xrPageInfo1.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrPageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime
        Me.xrPageInfo1.Size = New System.Drawing.Size(204, 20)
        Me.xrPageInfo1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
        '
        'xrLabelTitle
        '
        Me.xrLabelTitle.Font = New System.Drawing.Font("Tahoma", 15.75!, System.Drawing.FontStyle.Bold)
        Me.xrLabelTitle.Location = New System.Drawing.Point(0, 21)
        Me.xrLabelTitle.Name = "xrLabelTitle"
        Me.xrLabelTitle.Padding = New DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100.0!)
        Me.xrLabelTitle.Size = New System.Drawing.Size(650, 40)
        Me.xrLabelTitle.StylePriority.UseFont = False
        Me.xrLabelTitle.StylePriority.UseTextAlignment = False
        Me.xrLabelTitle.Text = "xrLabelTitle"
        Me.xrLabelTitle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
        '
        'xrReportTitle
        '
        Me.Bands.AddRange(New DevExpress.XtraReports.UI.Band() {Me.Detail, Me.PageHeader})
        Me.Version = "9.1"
        CType(Me, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Public WithEvents Detail As DevExpress.XtraReports.UI.DetailBand
    Public WithEvents PageHeader As DevExpress.XtraReports.UI.PageHeaderBand

#End Region

End Class