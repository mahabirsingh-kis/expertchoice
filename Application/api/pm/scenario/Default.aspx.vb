Partial Class ScenarioWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Private Readonly Property RA As ResourceAligner
        Get
            Return If(PM.IsRiskProject, PM.ResourceAlignerRisk, PM.ResourceAligner)
        End Get
    End Property

    Public Function list() As String
        Return RA.Scenarios.ToJSON()
    End Function

    Public Function set_active(id As Integer) As String
        RA.Scenarios.ActiveScenarioID = id  ' saved inside
        Return String.Format("{{""id"":{0}}}", RA.Scenarios.ActiveScenarioID)
    End Function

    Public Function add(name As String, desc As string) As String
        Dim NewScenario As RAScenario = Nothing
        If name <> "" Then
            NewScenario = RA.Scenarios.AddScenario()
            NewScenario.Name = name
            NewScenario.Description = desc
            RA.Save()
        End If
        Return list()
    End Function

    Public Function delete(id As Integer) As String
        If RA.Scenarios.DeleteScenario(id) Then RA.Save()
        Return list()
    End Function

    Private Sub ProjectManagerWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()

        Dim retVal As String = ""

        Select Case _Page.Action

            Case "list"
                _Page.ResponseData = list()

            Case "set_active"
                _Page.ResponseData = String.Format("{{""id"":{0}}}", RA.Scenarios.ActiveScenarioID)

                Dim ID As Integer = -1
                If Integer.TryParse(GetParam(_Page.Params, "id", True), ID) Then 
                    _Page.ResponseData = set_active(ID)
                End If

            Case "add"
                Dim sName As String = GetParam(_Page.Params, "name", True).Trim
                Dim sDesc As String = GetParam(_Page.Params, "desc", True).Trim
                
                _Page.ResponseData = add(sName, sDesc)

            Case "delete"
                _Page.ResponseData = "[]"
                Dim ID As Integer = -1
                Integer.TryParse(GetParam(_Page.Params, "id", True), ID)
                _Page.ResponseData = delete(ID)                
        End Select
    End Sub

End Class