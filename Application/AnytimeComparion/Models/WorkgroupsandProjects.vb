Public Class WorkgroupsandProjects
    Public Property workgroups() As List(Of Object())
    Public Property projects() As List(Of String())
    Public Property clsProjects() As List(Of Projects)
    Public Property active_workgroup_id() As Object()
    Public Property active_project_id() As Integer
    Public Property combined_group_id() As Integer
    Public Property role_workgroup_id() As Integer
    Public Property users() As List(Of Integer)
    Public Property currentUserID() As Integer
    Public Property isPM() As Boolean
    Public Property pageSize() As String
    Public Property sort() As Object()
    Public Property totalProjects() As Integer
    Public Property debugprojects() As Integer
    Public Property ProjectStatus() As Integer
    Public Property project_status() As Boolean
    Public Property project_access() As Boolean
    Public Property last_access() As Boolean
    Public Property last_modified() As Boolean
    Public Property date_created() As Boolean
    Public Property overal_judgment_process() As Boolean
    Public Property hideBrowserWarning() As Boolean
End Class

Public Class Projects
    Public Property ID() As Integer
    Public Property ProjectName() As String
    Public Property UserName() As String
    Public Property isTeamTime() As Boolean
    Public Property isOnline() As Boolean
    Public Property MeetingOwnerID() As Integer
    Public Property meetingOwner() As String
    Public Property LastModify() As String
    Public Property userscount() As Integer
    Public Property LastVisited() As Decimal
    Public Property DateCreated() As Decimal
    Public Property fCanModifyProject() As Boolean
    Public Property isValidDBVersion() As Boolean
    Public Property ProjectStatus() As String
End Class