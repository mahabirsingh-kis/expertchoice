Imports DevExpress.XtraReports.Web

Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlReport

        Inherits UserControl

        Private _ReportTitle As String = ""
        Public Property ReportTitle As String
            Get
                Return _ReportTitle
            End Get
            Set(value As String)
                _ReportTitle = value
            End Set
        End Property

        Public Property ReportViewer() As ReportViewer
            Get
                Return ReportViewerMain
                'Return _Report
            End Get
            Set(value As ReportViewer)
                ReportViewerMain = value
                '_Report = value
            End Set
        End Property

        'L0379 ===
        Private _ExportEnabled As Boolean = True
        Public Property ExportEnabled() As Boolean
            Get
                Return _ExportEnabled
            End Get
            Set(value As Boolean)
                _ExportEnabled = value
            End Set
        End Property
        'L0379 ==

        ' D3329 ===
        Protected Sub ReportToolbarMain_Load(sender As Object, e As EventArgs) Handles ReportToolbarMain.Load
            ' -D6300    // doesn't work for html5
            'If ExportEnabled AndAlso CType(ReportToolbarMain.Items(0), ReportToolbarButton).ItemKind <> ReportToolbarItemKind.PrintReport Then
            '    ReportToolbarMain.Items.Insert(0, New ReportToolbarSeparator())
            '    ReportToolbarMain.Items.Insert(0, New ReportToolbarButton(ReportToolbarItemKind.PrintPage, "Print the current page", "BtnPrintPage.gif"))
            '    ReportToolbarMain.Items.Insert(0, New ReportToolbarButton(ReportToolbarItemKind.PrintReport, "Print the report", "BtnPrint.gif"))
            '    ReportToolbarMain.Items.Add(New ReportToolbarSeparator())
            'End If
        End Sub
        ' D3329 ==

        ' D3702 ===
        Protected Sub ReportViewerMain_Init(sender As Object, e As EventArgs) Handles ReportViewerMain.Init
            Dim bypages As Boolean = True
            If Request.Cookies("rv") IsNot Nothing Then bypages = CBool(IIf(Request.Cookies("rv").Value = "0", False, True))
            ReportViewerMain.PageByPage = bypages
        End Sub
        ' D3702 ==

    End Class

End Namespace