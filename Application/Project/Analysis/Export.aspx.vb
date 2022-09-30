Imports System.Xml
Partial Class ExportChartPage
    Inherits clsComparionCorePage

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            If App.HasActiveProject Then Return App.ActiveProject.ProjectManager Else Return Nothing
        End Get
    End Property

    Public Sub New()
        MyBase.New(_PGID_EXPORT_CHARTS_ALTS)
    End Sub

    Private Sub ExportChartPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        AlignHorizontalCenter = True
        AlignVerticalCenter = True
    End Sub

End Class