Option Strict Off

Imports Microsoft.VisualBasic
Imports ExpertChoice.Data
Imports ECCore
Imports System.Data
Imports SpyronControls.Spyron.Core
Imports System.Data.Common
Imports ExpertChoice.Database

Public Class clsDataProvider
    Private _ConnectionString As String

    'Private AppConnection As New OdbcConnection(CreateConnectionString(WebOptions.SQLServerName, WebOptions.SQLUserName, WebOptions.SQLUserPassword, WebOptions.SQLMasterDB, ecDBConnectionType.ODBC, ecDBServerType.MSSQL))    ' D0315
    'Private AppConnection As OdbcConnection = Nothing   ' D0329 'L0010
    'Private ProjectConnection As OdbcConnection = Nothing   ' D0329 'L0010
    Private ProjectManager As New clsProjectManager()
    Private fMasterConnDef As clsConnectionDefinition 'L0002
    Private fDBProviderType As DBProviderType ' L0002

    ' L0002 ===
    Public Property MasterConnectionDefinition() As clsConnectionDefinition
        Get
            Return fMasterConnDef
        End Get
        Set(ByVal value As clsConnectionDefinition)
            fMasterConnDef = value
            fDBProviderType = value.ProviderType    ' D0499
        End Set
    End Property

    Public ReadOnly Property MasterDBProviderType() As DBProviderType
        Get
            Return fDBProviderType
        End Get
    End Property
    ' L0002 ==

    Public Sub New(ByRef PM As clsProjectManager, Optional ByVal AppConnectionString As String = "") ' D0329
        If Not PM Is Nothing Then
            ProjectManager = PM
        End If
        'AppConnection = New OdbcConnection()   ' D0329
        'If AppConnectionString <> "" Then AppConnection.ConnectionString = AppConnectionString ' D0329
    End Sub

    Public ReadOnly Property dsGlobalLogs() As DataSet
        Get
            Dim DataSet1 As New DataSet()
            Dim Adapter As DataAdapter
            'L0010 ===
            Adapter = GetDBDataAdapter(MasterDBProviderType, "SELECT * FROM Logs ORDER BY Logs.dt DESC", GetDBConnection(MasterDBProviderType, MasterConnectionDefinition.ConnectionString))
            'Dim Adapter As New OdbcDataAdapter("SELECT * FROM Logs ORDER BY Logs.dt DESC", AppConnection)
            Adapter.Fill(DataSet1)
            'L0010 ==
            Return DataSet1
        End Get
    End Property
    Public ReadOnly Property dsGlobalViewLogs(ByVal sWorkgroup As String, ByVal StartDate As Date, ByVal EndDate As Date) As DataSet    ' D0296
        Get
            'einfSession = 1
            'einfUser = 10
            'einfProject = 20
            'einfRoleAction = 50
            'einfRoleGroup = 51
            'einfUserWorkgroup = 52
            'einfWorkgroup = 53
            'einfWorkspace = 54
            'einfOther = 99
            Dim strSQL As String = ""
            Dim DataSet1 As New DataSet()

            'strSQL += "SELECT id as 'ID', dt as 'Date', (CAST(DatePart(hh, dt) AS nvarchar(2))+':'+CAST(DatePart(mi, dt)as nvarchar(2))+':'+CAST(DatePart(ss, dt) as nvarchar(2))) AS 'Time', " 'L0030
            strSQL += "SELECT id as 'ID', dt as 'Date', convert(varchar(10), dt, 108) AS 'Time', " 'L0030
            strSQL += "'Extra Type' = CASE WHEN ExtraType = 1 THEN 'Session' "
            strSQL += "WHEN ExtraType = 10 THEN 'User' "
            strSQL += "WHEN ExtraType = 20 THEN 'Project' "
            strSQL += "WHEN ExtraType = 21 THEN 'Project Report' "  ' D2290
            strSQL += "WHEN ExtraType = 50 THEN 'Role Action' "
            strSQL += "WHEN ExtraType = 51 THEN 'Role Group' "
            strSQL += "WHEN ExtraType = 52 THEN 'User Workgroup' "
            strSQL += "WHEN ExtraType = 53 THEN 'Workgroup' "
            strSQL += "WHEN ExtraType = 54 THEN 'Workspace' "
            strSQL += "WHEN ExtraType = 55 THEN 'Option Sets' " ' D2290
            strSQL += "WHEN ExtraType = 56 THEN 'Workgroup Params' "    ' D2290
            strSQL += "WHEN ExtraType = 60 THEN 'File' "        ' D2290
            strSQL += "WHEN ExtraType = 61 THEN 'Database' "    ' D2290
            strSQL += "WHEN ExtraType = 80 THEN 'WebService' "  ' D0348
            strSQL += "WHEN ExtraType = 97 THEN 'Message' "     ' D0184
            strSQL += "WHEN ExtraType = 98 THEN 'RTE' "         ' D0184
            strSQL += "WHEN ExtraType = 99 THEN 'Other' "
            strSQL += "ELSE 'Unknown' END, "
            ' D0296 ===
            strSQL += "Object, Action, Comment, UserEmail, Workgroup, SessionID "
            strSQL += "FROM Logs "
            If sWorkgroup <> "" Then
                strSQL += String.Format("WHERE Workgroup='{0}' ", sWorkgroup.Replace("'", "''"))
            Else
                Dim strDateFilter As String = ""
                If StartDate <> Date.MinValue Then strDateFilter += String.Format(" ('{0}-{1}-{2}' <= dt) ", StartDate.Year, StartDate.Month, StartDate.Day)
                If EndDate <> Date.MinValue Then
                    If strDateFilter <> "" Then strDateFilter += " AND "
                    strDateFilter += String.Format(" ('{0}-{1}-{2}' >= dt) ", EndDate.Year, EndDate.Month, EndDate.Day)
                End If
                If strDateFilter <> "" Then strSQL += " WHERE " + strDateFilter
            End If

            strSQL += "ORDER BY Logs.id DESC" 'L0030
            ' D0296 ==
            ' L0002 ===
            Dim Adapter As DataAdapter
            Adapter = GetDBDataAdapter(MasterDBProviderType, strSQL, GetDBConnection(MasterDBProviderType, MasterConnectionDefinition.ConnectionString))
            'Dim Adapter As New OdbcDataAdapter(strSQL, AppConnection)
            'Adapter.Fill(DataSet1, "Logs")
            Adapter.Fill(DataSet1)
            ' L0002 ==

            Return DataSet1
        End Get
    End Property


    Public ReadOnly Property dsStructureOverview() As DataSet
        Get
            Dim ds As New DataSet()
            ds = ProjectManager.ProjectDataProvider.ReportDataSet(CanvasReportType.crtHierarchyObjectivesAndAlternatives2) 'L0001
            Return ds
        End Get
    End Property

    Public ReadOnly Property dsJudgementsOverview() As DataSet
        Get
            Dim ds As New DataSet()
            ds.Tables.Add("Participants")
            ds.Tables("Participants").Columns.Add("Name", System.Type.GetType("System.String"))
            ds.Tables("Participants").Columns.Add("UserType", System.Type.GetType("System.String"))
            Dim Row As DataRow
            For Each User As clsUser In ProjectManager.UsersList
                Row = ds.Tables("Participants").NewRow()
                Row("Name") = User.UserName
                Row("UserType") = "Facilitator"
                ds.Tables("Participants").Rows.Add(Row)
            Next
            For Each User As clsUser In ProjectManager.UsersList
                Row = ds.Tables("Participants").NewRow()
                Row("Name") = User.UserName
                Row("UserType") = "Participant"
                ds.Tables("Participants").Rows.Add(Row)
            Next
            Return ds
        End Get
    End Property
End Class
