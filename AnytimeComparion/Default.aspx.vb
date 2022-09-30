Imports System.Web.Services
Imports Canvas
Imports ECCore
Imports ExpertChoice.Data
Imports ExpertChoice.Service

Public Class About
    Inherits Page

    Private Const AutoAdvanceMaxJudgments As Integer = 5
    Private Const InconsistencySortingEnabled As String = "InconsistencySortingEnabled"
    Private Const InconsistencySortingOrder As String = "InconsistencySortingOrder"
    Private Const BestFit As String = "BestFit"
    Private Const Sess_WrtNode As String = "Sess_WrtNode"
    Private Const Sess_Recreate As String = "Recreate_Pipe"
    Private Const SessionIsFirstTime As String = "IsFirstTime"
    Private Const SessionAutoAdvanceJudgmentsCount As String = "AutoAdvanceJudgmentsCount"
    Private Const SessionAutoAdvance As String = "AutoAdvance"
    Private Const SessionIncreaseJudgmentsCount As String = "IncreaseJudgmentsCount"
    Private Const SessionIsJudgmentAlreadySaved As String = "IsJudgmentAlreadySaved"
    Private Const SessionMultiCollapse As String = "SessionMultiCollapse"
    Private Const SessionSinglePwCollapse As String = "SessionSinglePwCollapse"
    Private Const SessionParentNodeGuid As String = "SessionParentNodeGuid"
    Private _OriginalAHPUser As ECTypes.clsUser = Nothing

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
    End Sub

    Public Shared ReadOnly Property CurrentStep As Integer
        Get
            Dim context As HttpContext = HttpContext.Current
            Dim App = CType(context.Session("App"), clsComparionCore)
            If Not App Is Nothing Then
                Return App.ActiveWorkspace.ProjectStep(App.ActiveProject.isImpact)
            End If
            Return Nothing
        End Get
    End Property

    <WebMethod(EnableSession:=True)>
    Public Shared Function loadHierarchy() As Object
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("app"), clsComparionCore)
        If CurrentStep >= 0 Then
            Dim hierarchies = GeckoClass.NodeList(app.ActiveProject.HierarchyObjectives.GetLevelNodes(0), AnytimeClass.GetAction(CurrentStep))
            Dim output = New Dictionary(Of String, Object)() From
            {
                {"success", True},
                {"data", hierarchies}
            }
            Return output
        End If
    End Function


    <WebMethod(EnableSession:=True)>
    Public Shared Function loadStepList(ByVal first As Integer, ByVal last As Integer) As String
        Dim context As HttpContext = HttpContext.Current
        Dim app = CType(context.Session("app"), clsComparionCore)
        Dim steps = AnytimeClass.get_StepInformation(app, -1, first, last)
        Return steps
    End Function

    Public Shared Function CheckProject() As Boolean
        Dim context As HttpContext = HttpContext.Current
        Dim App = CType(context.Session("App"), clsComparionCore)
        If App.ActiveProject IsNot Nothing Then
            Dim project = App.DBProjectByID(App.ActiveProject.ID)
            If project.isTeamTime Then
                App.ActiveProjectsList = Nothing
            End If
            App.ActiveProject = project
            Return project.isTeamTime OrElse project.isTeamTimeLikelihood
        End If
        Return False
    End Function

End Class

