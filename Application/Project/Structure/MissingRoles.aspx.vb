Partial Class MissingRolesReportPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_MISSING_ROLES_REPORT)
    End Sub

    Public ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean
        Get
            With App
                Return Not .CanUserModifyProject(.ActiveUser.UserID, PRJ.ID, .ActiveUserWorkgroup, .DBWorkspaceByUserIDProjectID(.ActiveUser.UserID, PRJ.ID), .ActiveWorkgroup)
            End With
        End Get
    End Property

    Private Sub MissingRolesReportPage_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Put your code here
    End Sub

    Public Function GetRolesStat() As String
        Dim rolesStat As List(Of RolesStatistics) = PM.GetRolesStatistics(RolesToSendType.rstAll)
        Dim missingRoles As List(Of MissingRole) = New List(Of MissingRole)()
        Dim i As Integer = 1
        For Each role As RolesStatistics In rolesStat
            If role.AllowedCount = 0 Then
                Dim missingRole = New MissingRole()
                missingRole.id = i
                Dim alt As clsNode = PM.ActiveAlternatives.GetNodeByID(role.AlternativeID)
                missingRole.Alternative = If(alt IsNot Nothing, alt.NodeName, "")
                Dim obj As clsNode = PM.ActiveObjectives.GetNodeByID(role.ObjectiveID)
                missingRole.Objective = If(obj IsNot Nothing, obj.NodeName, "")
                missingRole.AllowedCount = role.AllowedCount
                missingRole.RestrictedCount = role.RestrictedCount
                missingRole.EvaluatedCount = role.EvaluatedCount
                missingRoles.Add(missingRole)
                i += 1
            End If
        Next

        Return JsonConvert.SerializeObject(missingRoles)
    End Function

End Class

Friend Class MissingRole
    Property id As Integer
    Property Alternative As String
    Property Objective As String
    Property AllowedCount As Integer
    Property RestrictedCount As Integer
    Property EvaluatedCount As Integer
End Class