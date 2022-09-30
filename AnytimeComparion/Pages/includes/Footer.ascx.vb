Imports System.Web.Services
Imports ExpertChoice.Data

Public Class Footer
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    'Protected Sub btnCurrentCluster_click(sender As Object, e As EventArgs)
    '    Dim context As HttpContext = HttpContext.Current
    '    Dim app = CType(context.Session("app"), clsComparionCore)
    '    Dim hierarchies = GeckoClass.NodeList(app.ActiveProject.HierarchyObjectives.GetLevelNodes(0), AnytimeClass.GetAction(CurrentStep))
    '    Dim output = New Dictionary(Of String, Object)() From
    '    {
    '        {"success", True},
    '        {"data", hierarchies}
    '    }
    '    'Return output
    'End Sub

    'Private Sub Button1_Click(sender As Object, e As System.EventArgs) Handles Button1.Click
    '    'do stuff
    '    Me.Button2.PerformClick()
    'End Sub

    'Public Shared ReadOnly Property CurrentStep As Integer
    '    Get
    '        Dim context As HttpContext = HttpContext.Current
    '        Dim App = CType(context.Session("App"), clsComparionCore)
    '        Return App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
    '    End Get
    'End Property



End Class