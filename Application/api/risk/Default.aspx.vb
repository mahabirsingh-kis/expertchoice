Partial Class RiskionWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Public Function LoadControlsDatagrid(ctrlType As ControlType, OnlyActive As Boolean) As jActionResult
        Dim rows As New List(Of jControlAssignment)
        Dim cols As New List(Of jDataGridColumn)

        Dim EventNames As New Dictionary(Of Guid, String)
        Dim ObjNames As New Dictionary(Of Guid, String)

        For Each alt As clsNode In App.ActiveProject.ProjectManager.ActiveAlternatives.Nodes
            EventNames.Add(alt.NodeGuidID, alt.NodeName)
        Next

        For Each node As clsNode In If(ctrlType = ControlType.ctConsequenceToEvent, App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes, App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes)
            ObjNames.Add(node.NodeGuidID, node.NodeName)
        Next

        For Each ctrl As clsControl In App.ActiveProject.ProjectManager.Controls.DefinedControls
            If ctrl.Type = ctrlType Then 
                 For Each assignment As clsControlAssignment In ctrl.Assignments
                     If (Not OnlyActive OrElse (OnlyActive AndAlso assignment.IsActive)) Then

                        Dim item As jControlAssignment = New jControlAssignment With {.ControlID = ctrl.ID, .ControlName = ctrl.Name, .ObjID = assignment.ObjectiveID, .ObjName = If(ObjNames.ContainsKey(assignment.ObjectiveID), ObjNames(assignment.ObjectiveID), ""), .EventID = assignment.EventID, .EventName = If(EventNames.ContainsKey(assignment.EventID), EventNames(assignment.EventID), ""), .Value = Math.Round(assignment.Value, 6)}
                        rows.Add(item)
                     End If
                 Next
            End If
        Next

        Return New jActionResult With {.Result = ecActionResult.arSuccess, .Data = New jDataGridData With {.rows = rows, .columns = cols }}
    End Function

    Public Function GetControlsList() As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim list As New List(Of jControl)
        Dim index As Integer = 1
        For Each ctrl As clsControl In App.ActiveProject.ProjectManager.Controls.DefinedControls.OrderBy(Function(x) x.Type)
            Dim item As jControl = New jControl With {.id = ctrl.ID, .index = index, .name = ctrl.Name, .description = GetControlInfodoc(PRJ, ctrl, True), .cost = If(ctrl.IsCostDefined, ctrl.Cost, 0), .type = ctrl.Type, .selected = ctrl.Active, .sa = UNDEFINED_INTEGER_VALUE, .sa_doll = UNDEFINED_INTEGER_VALUE}
            list.Add(item)
            index += 1
        Next

        Res.Data = list
        Res.Result = ecActionResult.arSuccess

        Return Res
    End Function

    Public Function GetControlsRiskReduction(ids As List(Of Guid)) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}
        Dim SelectedEventIDs As List(Of Guid) = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function (x) x.Enabled).Select(Of Guid)(Function (y) y.NodeGuidID).ToList

        Dim list As New List(Of jControl)
        For i As Integer = 0 To ids.Count - 1
            Dim sa As Double = App.ActiveProject.ProjectManager.ResourceAlignerRisk.Solver.GetControlRiskReductions(ids(i), SelectedEventIDs)
            Dim sa_doll As Double = If(sa = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, sa * PM.DollarValueOfEnterprise)
            Dim item As jControl = New jControl With {.id = ids(i), .sa = sa, .sa_doll = sa_doll}
            list.Add(item)
        Next

        Res.Data = list
        Res.Result = ecActionResult.arSuccess

        Return Res
    End Function

    Public Function UseSimulations(value As Boolean) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        PM.CalculationsManager.UseSimulatedValues = value
        PM.Parameters.Save()

        Res.Result = ecActionResult.arSuccess

        Return Res
    End Function

    Private Sub ProjectsWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNotAuthorized()

        Select Case _Page.Action

            Case "LoadControlsDatagrid".ToLower
                Dim ctrlType As ControlType = ControlType.ctCause
                ControlType.TryParse(GetParam(_Page.Params, "type", True), ctrlType)
                Dim OnlyActive As Boolean = Str2Bool(GetParam(_Page.Params, "only_active", True))                
                _Page.ResponseData = LoadControlsDatagrid(ctrlType, OnlyActive)
            
            Case "GetControlsList".ToLower
                _Page.ResponseData = GetControlsList()

            Case "GetControlsRiskReduction".ToLower
                Dim ids As List(Of Guid) = Param2GuidList(GetParam(_Page.Params, "ids", True))
                _Page.ResponseData = GetControlsRiskReduction(ids)

            Case "UseSimulations".ToLower
                Dim value As Boolean = Str2Bool(GetParam(_Page.Params, "value", True))
                _Page.ResponseData = UseSimulations(value)

        End Select
    End Sub

    <Serializable> Public Class jControl
        Inherits clsJsonObject

        Public Property id As Guid
        Public Property index As Integer
        Public Property name As String = ""
        Public Property description As String = ""
        Public Property type As ControlType
        Public Property selected As Boolean
        Public Property cost As Double
        Public Property sa As Double
        Public Property sa_doll As Double

    End Class

    <Serializable> Public Class jControlAssignment
        Inherits clsJsonObject

        Public Property ControlID As Guid
        Public Property ControlName As String = ""
        Public Property ObjID As Guid
        Public Property ObjName As String = ""
        Public Property EventID As Guid
        Public Property EventName As String = ""
        Public Property Value As Double

    End Class

    <Serializable> Public Class jDataGridColumn
        Inherits clsJsonObject

        Public Property dataField As String = ""
        Public Property caption As String = ""
    End Class

    <Serializable> Public Class jDataGridData
        Inherits clsJsonObject

        Public Property rows As List(Of jControlAssignment)
        Public Property columns As List(Of jDataGridColumn)
    End Class

End Class