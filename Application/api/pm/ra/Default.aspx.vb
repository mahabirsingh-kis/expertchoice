Partial Public Class RAWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Private ReadOnly Property RA As ResourceAligner
        Get
            Return If(App.ActiveProject.ProjectManager.IsRiskProject, App.ActiveProject.ProjectManager.ResourceAlignerRisk, App.ActiveProject.ProjectManager.ResourceAligner)
        End Get
    End Property

    Private Property ActiveScenarioID As Integer
        Get
            Return RA.Scenarios.ActiveScenarioID
        End Get
        Set(value As Integer)
            If RA.Scenarios.ActiveScenarioID <> value Then
                RA.Scenarios.ActiveScenarioID = value
                RA.Solver.ResetSolver()
                PM.Parameters.RAActiveScenarioID = value
            End If
        End Set
    End Property

    Shared Function getScenarios(RA As ResourceAligner) As List(Of jRAScenario)
        Dim tRes As New List(Of jRAScenario)
        If RA IsNot Nothing Then
            For Each tScen As KeyValuePair(Of Integer, RAScenario) In RA.Scenarios.Scenarios
                tRes.Add(jRAScenario.CreateFromBaseObject(tScen.Value))
            Next
        End If
        Return tRes
    End Function

    Shared Function getScenariosAsJSON(RA As ResourceAligner) As String
        Return JsonConvert.SerializeObject(getScenarios(RA))
    End Function

    Public Function scenarios() As jActionResult
        Return New jActionResult With {
            .Result = ecActionResult.arSuccess,
            .Data = getScenarios(RA)
            }
    End Function

    Public Function set_active_scenario(ID As Integer) As jActionResult
        Dim tRes As New jActionResult
        If RA.Scenarios.Scenarios.ContainsKey(ID) Then
            ActiveScenarioID = ID
            PM.Parameters.Save()
            tRes.Result = ecActionResult.arSuccess
            tRes.ObjectID = ActiveScenarioID
        Else
            _Page.FetchWrongObject()
        End If
        Return tRes
    End Function

    Public Function get_portfolio_grid(Optional Solve As Boolean = False) As jActionResult
        Dim tRes As New jActionResult

        RA.Load()
        If (Solve OrElse RA.Solver.SolverState <> raSolverState.raSolved OrElse RA.Scenarios.ActiveScenario.Alternatives.Sum(Function(x) x.Funded) <= 0) Then   ' D7059 // count the "funded" alternatives: ideally this is the last one check when Solver reported that Solved but in fact no funded alts
            RA.Scenarios.CheckModel()
            RA.Solver.Solve(raSolverExport.raNone)
        End If

        tRes.Result = ecActionResult.arSuccess
        tRes.Message = RA.Solver.LastError
        tRes.Data = jRAGrid.CreateFromBaseObject(RA)
        tRes.ObjectID = ActiveScenarioID

        Return tRes
    End Function

    Private Sub RAWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()

        RA.Load()

        Select Case _Page.Action

            Case "scenarios"
                _Page.ResponseData = scenarios()

            Case "set_active_scenario"
                Dim ID As Integer
                If Not Integer.TryParse(GetParam(_Page.Params, "id"), ID) Then ID = ActiveScenarioID
                _Page.ResponseData = set_active_scenario(ID)

            Case "get_portfolio_grid"
                _Page.ResponseData = get_portfolio_grid(Str2Bool(GetParam(_Page.Params, "solve"), False))


        End Select
    End Sub

End Class