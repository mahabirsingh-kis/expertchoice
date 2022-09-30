Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports ECCore
Imports System.Data
Imports System.Data.Common
Imports GenericDBAccess.ECGenericDatabaseAccess
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports Microsoft.SqlServer.Server

Public Class StaticDecoder
    <Microsoft.SqlServer.Server.SqlProcedure()> _
    Public Shared Sub StreamDecode(ByVal DestCatalog As String, ByVal MasterCatalog As String, ByVal ProjectId As Integer, ByVal NewSnapshot As Integer)
        Dim Decoder As New Reporting.StreamDecoder(GenericDB.DBProviderType.dbptSQLClient, "context connection=true", "context connection=true", DestCatalog, MasterCatalog)
        Decoder.StreamDecode(ProjectId, NewSnapshot)
    End Sub

    <Microsoft.SqlServer.Server.SqlProcedure()> _
    Public Shared Sub MasterCopy(ByVal DestCatalog As String, ByVal MasterCatalog As String, ByVal ProjectId As Integer, ByVal NewSnapshot As Integer)
        Dim Decoder As New Reporting.StreamDecoder(GenericDB.DBProviderType.dbptSQLClient, "context connection=true", "context connection=true", DestCatalog, MasterCatalog)
        Decoder.CopyMasterData(NewSnapshot)
    End Sub
End Class

Namespace Reporting

    Public MustInherit Class BaseDecoder

        Shared BatchSize As Integer = 500000
        Protected SrcConn As String
        Protected DstConn As String
        Protected Catalog As String
        Protected Timings As String
        Protected MasterCatalog As String

        Protected Provider As GenericDB.DBProviderType
        Protected Conn As DbConnection
        Protected Trans As DbTransaction
        Protected pm As clsProjectManager = New clsProjectManager(True)

        'project manager-related virtuals
        Protected Overridable Sub PMLoadProjectData(ByVal ProjectId As Integer)
            pm.StorageManager.ProviderType = Provider
            pm.StorageManager.ProjectLocation = SrcConn
            pm.StorageManager.ModelID = ProjectId
            pm.StorageManager.ReadDBVersion()
            pm.StorageManager.Reader.LoadProject(Nothing)
        End Sub

        Protected Overridable Sub PMLoadUserData(ByVal UserId As Integer?)
            If (UserId IsNot Nothing) Then
                pm.StorageManager.Reader.LoadUserData(pm.UsersList(UserId), False)
            Else
                pm.StorageManager.Reader.LoadUserData(Nothing, False)
            End If
        End Sub

        Protected Overridable Function MakeDataTable(ByVal ColumnNames As String(), ByVal DataTypes As Type()) As DataTable
            Dim Table As DataTable = New DataTable()
            For i = 0 To ColumnNames.Count() - 1
                Table.Columns.Add(New DataColumn(ColumnNames(i), DataTypes(i)))
            Next i
            Return Table
        End Function

        'data table abstracts (for bulk copy)
        Protected MustOverride Function MakeNodesDataTable() As DataTable
        Protected MustOverride Function MakeAltsContributionsDataTable() As DataTable
        Protected MustOverride Function MakeDisabledNodesDataTable() As DataTable
        Protected MustOverride Function MakeNodesRestrictionsDataTable() As DataTable
        Protected MustOverride Function MakeAltsRestrictionsDataTable() As DataTable
        Protected MustOverride Function MakePairwiseDataTable() As DataTable
        Protected MustOverride Function MakeNonPairwiseDataTable() As DataTable

        'fill data table abstracts (for bulk copy)
        Protected MustOverride Sub HierarchyNodesFillTable(ByVal Hierarchy As clsHierarchy, ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub FillAltsContributionsDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub FillDisabledHierarchyNodes(ByVal Table As DataTable, ByVal Hierarchy As clsHierarchy, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub FillNodesRestrictionsDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub FillAltsRestrictionsDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub FillPairwiseDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub FillNonPairwiseDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)

        'fill data table virtuals (for bulk copy)
        Protected Overridable Sub FillNodesDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            HierarchyNodesFillTable(pm.Hierarchies(pm.ActiveHierarchy), Table, SnapshotId, ProjectId)
            HierarchyNodesFillTable(pm.AltsHierarchy(pm.ActiveAltsHierarchy), Table, SnapshotId, ProjectId)
        End Sub

        Protected Overridable Sub FillDisabledNodesDataTable(ByVal Table As DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            FillDisabledHierarchyNodes(Table, pm.Hierarchies(pm.ActiveHierarchy), SnapshotId, ProjectId, UserId)
            FillDisabledHierarchyNodes(Table, pm.AltsHierarchy(pm.ActiveAltsHierarchy), SnapshotId, ProjectId, UserId)
        End Sub

        'bulk insert virtuals: generic purpose
        Public Overridable Function BulkCopyAllowed() As Boolean
            Return ((Provider = GenericDB.DBProviderType.dbptSQLClient) And (Not SqlContext.IsAvailable))
        End Function

        Protected Overridable Sub DoBulkCopy(ByVal Table As DataTable, ByVal TargetTable As String, ByVal Threshold As Integer)
            If (Table.Rows.Count > Threshold) Then
                Using cpy As SqlBulkCopy = New SqlBulkCopy(CType(Conn, SqlConnection), SqlBulkCopyOptions.Default, CType(Trans, SqlTransaction))
                    cpy.DestinationTableName = Catalog + TargetTable
                    cpy.BatchSize = BatchSize
                    cpy.WriteToServer(Table)
                    Table.Rows.Clear()
                End Using
            End If
        End Sub

        'bulk insert virtuals
        Protected Overridable Sub BulkInsertNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Table As DataTable = MakeNodesDataTable()
            FillNodesDataTable(Table, SnapshotId, ProjectId)
            DoBulkCopy(Table, "Nodes", 0)
        End Sub

        Protected Overridable Sub BulkInsertAltsContributions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Table As DataTable = MakeAltsContributionsDataTable()
            FillAltsContributionsDataTable(Table, SnapshotId, ProjectId)
            DoBulkCopy(Table, "AltsContributions", 0)
        End Sub

        Protected Overridable Sub BulkInsertDisabledNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            Dim Table As DataTable = MakeDisabledNodesDataTable()
            If (UserId IsNot Nothing) Then
                FillDisabledNodesDataTable(Table, SnapshotId, ProjectId, UserId)
            Else
                For Each User As ECTypes.clsUser In pm.UsersList
                    If (User.UserID >= 0) Then
                        FillDisabledNodesDataTable(Table, SnapshotId, ProjectId, User.UserID)
                        DoBulkCopy(Table, "UserDisabledNodes", BatchSize)
                    End If
                Next
            End If
            DoBulkCopy(Table, "UserDisabledNodes", 0)
        End Sub


        Protected Overridable Sub BulkInsertNodesRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            Dim Table As DataTable = MakeNodesRestrictionsDataTable()
            If (UserId IsNot Nothing) Then
                FillNodesRestrictionsDataTable(Table, SnapshotId, ProjectId, UserId)
            Else
                For Each User As ECTypes.clsUser In pm.UsersList
                    If (User.UserID >= 0) Then
                        FillNodesRestrictionsDataTable(Table, SnapshotId, ProjectId, User.UserID)
                        DoBulkCopy(Table, "NodesRestrictions", BatchSize)
                    End If
                Next
            End If
            DoBulkCopy(Table, "NodesRestrictions", 0)
        End Sub

        Protected Overridable Sub BulkInsertAltsRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            Dim Table As DataTable = MakeAltsRestrictionsDataTable()
            If (UserId IsNot Nothing) Then
                FillAltsRestrictionsDataTable(Table, SnapshotId, ProjectId, UserId)
            Else
                For Each User As ECTypes.clsUser In pm.UsersList
                    If (User.UserID >= 0) Then
                        FillAltsRestrictionsDataTable(Table, SnapshotId, ProjectId, User.UserID)
                        DoBulkCopy(Table, "AltsRestrictions", BatchSize)
                    End If
                Next
            End If
            DoBulkCopy(Table, "AltsRestrictions", 0)
        End Sub

        Protected Overridable Sub BulkInsertUserPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            Dim Table As DataTable = MakePairwiseDataTable()
            If (UserId IsNot Nothing) Then
                FillPairwiseDataTable(Table, SnapshotId, ProjectId, UserId)
            Else
                For Each User As ECTypes.clsUser In pm.UsersList
                    If (User.UserID >= 0) Then
                        FillPairwiseDataTable(Table, SnapshotId, ProjectId, User.UserID)
                        DoBulkCopy(Table, "PairwiseData", BatchSize)
                    End If
                Next
            End If
            DoBulkCopy(Table, "PairwiseData", 0)
        End Sub

        Protected Overridable Sub BulkInsertUserNonPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            Dim Table As DataTable = MakeNonPairwiseDataTable()
            If (UserId IsNot Nothing) Then
                FillNonPairwiseDataTable(Table, SnapshotId, ProjectId, UserId)
            Else
                For Each User As ECTypes.clsUser In pm.UsersList
                    If (User.UserID >= 0) Then
                        FillNonPairwiseDataTable(Table, SnapshotId, ProjectId, User.UserID)
                        DoBulkCopy(Table, "NonPairwiseData", BatchSize)
                    End If
                Next
            End If
            DoBulkCopy(Table, "NonPairwiseData", 0)
        End Sub

        Protected Overridable Sub BulkInsertUserData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            BulkInsertDisabledNodes(SnapshotId, ProjectId, UserId)
            BulkInsertNodesRestrictions(SnapshotId, ProjectId, UserId)
            BulkInsertAltsRestrictions(SnapshotId, ProjectId, UserId)
            BulkInsertUserPairwiseData(SnapshotId, ProjectId, UserId)
            BulkInsertUserNonPairwiseData(SnapshotId, ProjectId, UserId)
        End Sub

        Public Overridable Function PrepareCommand(ByVal Cmd As String, ByVal Names As String(), ByVal Values As Object()) As DbCommand
            Dim Command As DbCommand = GenericDB.GetDBCommand(Provider)
            Command.CommandText = Cmd
            Command.Connection = Conn
            Command.Transaction = Trans
            For i = 0 To Names.Count() - 1
                Command.Parameters.Add(GenericDB.GetDBParameter(Provider, Names(i), Values(i)))
            Next i
            GenericDB.ParseCommandQuery(Provider, Command)
            Return Command
        End Function

        Public Overridable Sub ExecuteNonQuery(ByVal Cmd As String, ByVal Names As String(), ByVal Values As Object())
            PrepareCommand(Cmd, Names, Values).ExecuteNonQuery()
        End Sub

        Protected Overridable Function ProjectWorkgroupId(ByVal ProjectId As Integer) As Integer?
            Dim Cmd As DbCommand = PrepareCommand("select WorkgroupID from " + MasterCatalog + _
                "Projects where ID = ?", _
                New String() {"ProjectID"}, New Object() {ProjectId})
            Dim Reader As DbDataReader = Cmd.ExecuteReader()
            Dim WorkgroupId As Integer? = Nothing
            While (Reader.Read())
                WorkgroupId = Reader.GetInt32(0)
                Exit While
            End While
            Reader.Close()
            Return WorkgroupId
        End Function

        'generic operations abstracts
        Protected MustOverride Sub DeleteMasterData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
        Protected MustOverride Sub DeleteProjectData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)

        Protected MustOverride Sub InsertWorkgroups(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
        Protected MustOverride Sub InsertRoleGroups(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
        Protected MustOverride Sub InsertRoleActions(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
        Protected MustOverride Sub InsertProjectData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
        Protected MustOverride Sub InsertUsers(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
        Protected MustOverride Sub InsertWorkspace(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
        Protected MustOverride Sub InsertUserWorkgroups(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)

        Protected MustOverride Sub InsertHierarchyNodes(ByVal Hierarchy As clsHierarchy, ByVal Cmd As DbCommand)
        Protected MustOverride Sub GenericInsertNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub GenericInsertAltsContributions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertRatingScales(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertRatingIntensities(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertUtilityCurves(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertAdvancedUtilityCurves(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertCurvesPoints(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertStepFunctions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertStepIntervals(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertUsers(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertDataInstances(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
        Protected MustOverride Sub InsertDisabledHierarchyNodes(ByVal Hierarchy As clsHierarchy, ByVal Cmd As DbCommand, ByVal UserId As Integer)
        Protected MustOverride Sub GenericInsertDisabledNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub GenericInsertNodesRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub GenericInsertAltsRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub GenericInsertUserPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
        Protected MustOverride Sub GenericInsertUserNonPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)

        Protected Overridable Sub InsertMasterData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
            Dim WorkgroupId As Integer? = Nothing
            If (ProjectId IsNot Nothing) Then
                WorkgroupId = ProjectWorkgroupId(ProjectId)
            End If
            InsertProjectData(SnapshotId, ProjectId)
            InsertWorkgroups(SnapshotId, WorkgroupId)
            InsertRoleGroups(SnapshotId, WorkgroupId)
            InsertRoleActions(SnapshotId, WorkgroupId)
            InsertUsers(SnapshotId, ProjectId)
            InsertWorkspace(SnapshotId, ProjectId)
            InsertUserWorkgroups(SnapshotId, WorkgroupId)
        End Sub

        Protected Overridable Sub InsertNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertNodes(SnapshotId, ProjectId)
            Else
                GenericInsertNodes(SnapshotId, ProjectId)
            End If
        End Sub

        Protected Overridable Sub InsertAltsContributions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertAltsContributions(SnapshotId, ProjectId)
            Else
                GenericInsertAltsContributions(SnapshotId, ProjectId)
            End If
        End Sub

        Protected Overridable Sub StreamDecodeModelStructure(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            'pm.StorageManager.ProviderType = Provider
            'pm.StorageManager.ProjectLocation = SrcConn
            'pm.StorageManager.ModelID = ProjectId
            'pm.StorageManager.ReadDBVersion()
            'pm.StorageManager.Reader.LoadProject(Nothing)
            'DeleteProjectData(SnapshotId, ProjectId)
            DeleteMasterData(SnapshotId, ProjectId)
            InsertMasterData(SnapshotId, ProjectId)
            'InsertProjectData(SnapshotId, ProjectId)

            InsertNodes(SnapshotId, ProjectId)
            InsertAltsContributions(SnapshotId, ProjectId)
            InsertRatingScales(SnapshotId, ProjectId)
            InsertRatingIntensities(SnapshotId, ProjectId)
            InsertUtilityCurves(SnapshotId, ProjectId)
            InsertAdvancedUtilityCurves(SnapshotId, ProjectId)
            InsertCurvesPoints(SnapshotId, ProjectId)
            InsertStepFunctions(SnapshotId, ProjectId)
            InsertStepIntervals(SnapshotId, ProjectId)
            InsertUsers(SnapshotId, ProjectId)
            InsertDataInstances(SnapshotId, ProjectId)
        End Sub

        Protected MustOverride Sub DeleteUserData(ByVal TableName As String, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)

        Protected Overridable Sub DeleteAllUserData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            DeleteUserData("UserDisabledNodes", SnapshotId, ProjectId, UserId)
            DeleteUserData("PairwiseData", SnapshotId, ProjectId, UserId)
            DeleteUserData("NonPairwiseData", SnapshotId, ProjectId, UserId)
            DeleteUserData("NodesRestrictions", SnapshotId, ProjectId, UserId)
            DeleteUserData("AltsRestrictions", SnapshotId, ProjectId, UserId)
        End Sub

        Protected Overridable Sub InsertUserDisabledNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertDisabledNodes(SnapshotId, ProjectId, UserId)
            Else
                GenericInsertDisabledNodes(SnapshotId, ProjectId, UserId)
            End If
        End Sub

        Protected Overridable Sub InsertUserNodesRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertNodesRestrictions(SnapshotId, ProjectId, UserId)
            Else
                GenericInsertNodesRestrictions(SnapshotId, ProjectId, UserId)
            End If
        End Sub

        Protected Overridable Sub InsertUserAltsRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertAltsRestrictions(SnapshotId, ProjectId, UserId)
            Else
                GenericInsertAltsRestrictions(SnapshotId, ProjectId, UserId)
            End If
        End Sub

        Protected Overridable Sub InsertUserPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertUserPairwiseData(SnapshotId, ProjectId, UserId)
            Else
                GenericInsertUserPairwiseData(SnapshotId, ProjectId, UserId)
            End If
        End Sub

        Protected Overridable Sub InsertUserNonPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            If (BulkCopyAllowed()) Then
                BulkInsertUserNonPairwiseData(SnapshotId, ProjectId, UserId)
            Else
                GenericInsertUserNonPairwiseData(SnapshotId, ProjectId, UserId)
            End If
        End Sub

        Protected Overridable Sub InsertUserData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            'pm.StorageManager.Reader.LoadUserData(pm.UsersList[UserId], false)
            InsertUserDisabledNodes(SnapshotId, ProjectId, UserId)
            InsertUserNodesRestrictions(SnapshotId, ProjectId, UserId)
            InsertUserAltsRestrictions(SnapshotId, ProjectId, UserId)
            InsertUserPairwiseData(SnapshotId, ProjectId, UserId)
            InsertUserNonPairwiseData(SnapshotId, ProjectId, UserId)
        End Sub

        Protected Overridable Sub StreamDecodeUserJudgments(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            'pm.StorageManager.ProviderType = Provider
            'pm.StorageManager.ProjectLocation = SrcConn
            'pm.StorageManager.ModelID = ProjectId
            'pm.StorageManager.ReadDBVersion()
            'pm.StorageManager.Reader.LoadProject(Nothing)
            DeleteAllUserData(SnapshotId, ProjectId, UserId)
            If (BulkCopyAllowed()) Then
                BulkInsertUserData(SnapshotId, ProjectId, UserId)
                Return
            End If
            If (UserId IsNot Nothing) Then
                InsertUserData(SnapshotId, ProjectId, UserId)
            Else
                For Each User As ECTypes.clsUser In pm.UsersList
                    If (User.UserID >= 0) Then
                        InsertUserData(SnapshotId, ProjectId, User.UserID)
                    End If
                Next
            End If
        End Sub

        'snapshot management virtuals
        Protected Overridable Function LatestSnapshotId(ByVal ProjectId As Integer) As Integer?
            Dim Cmd As DbCommand = GenericDB.GetDBCommand(Provider)
            Cmd.Connection = Conn
            Cmd.CommandText = "SELECT a.SnapshotId FROM " + Catalog + _
                "Snapshots a WHERE a.ProjectId = ? AND a.SnapshotTime = (SELECT MAX(SnapshotTime) FROM " + _
                Catalog + "Snapshots b WHERE b.ProjectId = a.ProjectId)"
            Cmd.Parameters.Add(GenericDB.GetDBParameter(Provider, "ProjectId", ProjectId))
            GenericDB.ParseCommandQuery(Provider, Cmd)
            Cmd.Transaction = Trans
            Dim Reader As DbDataReader = Cmd.ExecuteReader()
            While (Reader.Read())
                Dim Id As Integer = Reader.GetInt32(0)
                Reader.Close()
                Return Id
            End While
            Reader.Close()
            Return Nothing
        End Function

        Protected Overridable Function NewSnapshotId(ByVal ProjectId As Integer) As Integer
            Dim LatestId As Integer? = LatestSnapshotId(ProjectId)
            Dim SnapshotTime As DateTime = DateTime.Now
            If (LatestId Is Nothing) Then
                SnapshotTime = New DateTime(1900, 1, 1)
            End If
            ExecuteNonQuery("INSERT INTO " + Catalog + "SNAPSHOTS(ProjectId, SnapshotTime) VALUES (?, ?)", _
                New String() {"ProjectId", "SnapshotTime"}, New Object() {ProjectId, SnapshotTime})
            Return LatestSnapshotId(ProjectId)
        End Function

        'public generic-purpose virtuals
        Public Overridable Sub OpenConnection()
            Conn = GenericDB.GetDBConnection(Provider, DstConn)
            Conn.Open()
        End Sub

        Public Overridable Sub CloseConnection()
            Conn.Close()
        End Sub

        Public Overridable Sub BeginTransaction()
            Trans = Conn.BeginTransaction()
        End Sub

        Public Overridable Sub Commit()
            Trans.Commit()
        End Sub

        Public Overridable Sub Rollback()
            Trans.Rollback()
        End Sub

        'main decode routine
        Public Overridable Sub StreamDecode(ByVal ProjectId As Integer, ByVal NewSnapshot As Integer)
            Dim t0 As DateTime = DateTime.Now
            PMLoadProjectData(ProjectId)
            PMLoadUserData(Nothing)
            Dim t1 As DateTime = DateTime.Now
            OpenConnection()
            BeginTransaction()
            Try
                Dim SnapshotId As Integer
                Dim SnapId As Integer? = LatestSnapshotId(ProjectId)
                If ((SnapId Is Nothing) Or (NewSnapshot <> 0)) Then
                    SnapshotId = NewSnapshotId(ProjectId)
                Else
                    SnapshotId = SnapId
                End If
                StreamDecodeModelStructure(SnapshotId, ProjectId)
                StreamDecodeUserJudgments(SnapshotId, ProjectId, Nothing)
                Commit()
                CloseConnection()
            Catch
                Rollback()
                CloseConnection()
                Throw
            End Try
            Dim t2 As DateTime = DateTime.Now
            Dim ts1 As TimeSpan = t1 - t0
            Dim ts2 As TimeSpan = t2 - t1
            Timings = String.Format("Streams loaded in: {0}, db refreshed in: {1}", ts1.ToString(), ts2.ToString())
            If (SqlContext.IsAvailable) Then
                SqlContext.Pipe.Send(Timings)
            End If
        End Sub

        Public Overridable Sub CopyMasterData(ByVal NewSnapshot As Integer)
            Dim MasterProjectId As Integer = -1
            Dim t0 As DateTime = DateTime.Now
            OpenConnection()
            BeginTransaction()
            Try
                Dim SnapshotId As Integer
                Dim SnapId As Integer? = LatestSnapshotId(MasterProjectId)
                If ((SnapId Is Nothing) Or (NewSnapshot <> 0)) Then
                    SnapshotId = NewSnapshotId(MasterProjectId)
                Else
                    SnapshotId = SnapId
                End If
                DeleteMasterData(SnapshotId, Nothing)
                InsertMasterData(SnapshotId, Nothing)
                Commit()
                CloseConnection()
            Catch
                Rollback()
                CloseConnection()
                Throw
            End Try
            Dim t1 As DateTime = DateTime.Now
            Dim ts1 As TimeSpan = t1 - t0
            Timings = String.Format("Db refreshed in: {0}", ts1.ToString())
            If (SqlContext.IsAvailable) Then
                SqlContext.Pipe.Send(Timings)
            End If
        End Sub

        Public Property StreamConnection() As String
            Get
                Return SrcConn
            End Get
            Set(ByVal Value As String)
                SrcConn = Value
            End Set
        End Property
        Public Property DestConnection() As String
            Get
                Return DstConn
            End Get
            Set(ByVal value As String)
                DstConn = value
            End Set
        End Property
        Public Property DestCatalog() As String
            Get
                Return Catalog
            End Get
            Set(ByVal value As String)
                Catalog = value
            End Set
        End Property
        Public Property MasterDbCatalog() As String
            Get
                Return MasterCatalog
            End Get
            Set(ByVal value As String)
                MasterCatalog = value
            End Set
        End Property
        Public ReadOnly Property LastTimings() As String
            Get
                Return Timings
            End Get
        End Property
        Public Property ProviderType() As GenericDB.DBProviderType
            Get
                Return Provider
            End Get
            Set(ByVal value As GenericDB.DBProviderType)
                Provider = value
            End Set
        End Property

    End Class

    Public Class StreamDecoder
        Inherits BaseDecoder

        Protected _DBNull As Object = DBNull.Value
        Protected _Int32 As Type = GetType(Int32)
        Protected _Single As Type = GetType(Single)
        Protected _String As Type = GetType(String)
        Protected _DateTime As Type = GetType(DateTime)

        Protected Overrides Sub DeleteMasterData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
            ExecuteNonQuery("DELETE FROM " + Catalog + "Users WHERE SnapshotID = ?", _
                New String() {"SnapshotID"}, New Object() {SnapshotId})

            ExecuteNonQuery("DELETE FROM " + Catalog + "Workgroups WHERE SnapshotID = ?", _
                New String() {"SnapshotID"}, New Object() {SnapshotId})

            DeleteProjectData(SnapshotId, ProjectId)
        End Sub

        Protected Overrides Sub DeleteProjectData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
            Dim Sql As String = "DELETE FROM " + Catalog + "Projects WHERE SnapshotId=?"
            If (ProjectId IsNot Nothing) Then
                ExecuteNonQuery(Sql + " AND ProjectId=?", _
                    New String() {"SnapshotId", "ProjectId"}, New Object() {SnapshotId, ProjectId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub DeleteUserData(ByVal TableName As String, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer?)
            Dim Cmd As DbCommand = GenericDB.GetDBCommand(Provider)
            Cmd.Connection = Conn
            Cmd.CommandText = "DELETE FROM " + Catalog + TableName + " WHERE SnapshotId = ? AND ProjectId = ?"
            If (UserId IsNot Nothing) Then
                Cmd.CommandText = Cmd.CommandText + " AND UserId = ?"
            End If
            Cmd.Parameters.Add(GenericDB.GetDBParameter(Provider, "SnapshotId", SnapshotId))
            Cmd.Parameters.Add(GenericDB.GetDBParameter(Provider, "ProjectId", ProjectId))
            If (UserId IsNot Nothing) Then

                Cmd.Parameters.Add(GenericDB.GetDBParameter(Provider, "UserId", UserId))
            End If
            GenericDB.ParseCommandQuery(Provider, Cmd)
            Cmd.Transaction = Trans
            Cmd.ExecuteNonQuery()
        End Sub

        Protected Overrides Sub FillAltsContributionsDataTable(ByVal Table As System.Data.DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For i = 0 To Hierarchy.Nodes.Count - 1
                Dim Node As clsNode = Hierarchy.Nodes(i)
                If (Node.IsTerminalNode) Then
                    Dim Alts As List(Of clsNode) = Node.GetNodesBelow(ECTypes.UNDEFINED_USER_ID, False, 0)
                    For j = 0 To Alts.Count - 1
                        Dim Alt As clsNode = Alts(j)
                        Dim Row As DataRow = Table.NewRow()
                        Row("SNAPSHOTID") = SnapshotId
                        Row("PROJECTID") = ProjectId
                        Row("COVOBJNODEID") = Node.NodeID
                        Row("ALTNODEID") = -(Alt.NodeID + 100)
                        Table.Rows.Add(Row)
                    Next j
                End If
            Next i
        End Sub

        Protected Overrides Sub FillAltsRestrictionsDataTable(ByVal Table As System.Data.DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.IsTerminalNode) Then
                    For Each Alt As clsNode In CType(Node.UserPermissions(UserId).SpecialPermissions, clsNodePermissions).RestrictedNodesBelow
                        Dim Row As DataRow = Table.NewRow()
                        Row("SNAPSHOTID") = SnapshotId
                        Row("PROJECTID") = ProjectId
                        Row("USERID") = UserId
                        Row("NODEID") = Node.NodeID
                        Row("ALTNODEID") = -(Alt.NodeID + 100)
                        Table.Rows.Add(Row)
                    Next
                End If
            Next
        End Sub

        Protected Overrides Sub FillDisabledHierarchyNodes(ByVal Table As System.Data.DataTable, ByVal Hierarchy As ECCore.clsHierarchy, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.DisabledForUser(UserId)) Then
                    Dim Row As DataRow = Table.NewRow()
                    Row("SNAPSHOTID") = SnapshotId
                    Row("PROJECTID") = ProjectId
                    Row("USERID") = UserId
                    If (Node.IsAlternative) Then
                        Row("NODEID") = -(Node.NodeID + 100)
                    Else
                        Row("NODEID") = Node.NodeID
                    End If
                    Table.Rows.Add(Row)
                End If
            Next
        End Sub

        Protected Overrides Sub FillNodesRestrictionsDataTable(ByVal Table As System.Data.DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Not Node.UserPermissions(UserId).Write) Then
                    Dim Row As DataRow = Table.NewRow()
                    Row("SNAPSHOTID") = SnapshotId
                    Row("PROJECTID") = ProjectId
                    Row("USERID") = UserId
                    Row("NODEID") = Node.NodeID
                    Table.Rows.Add(Row)
                End If
            Next
        End Sub

        Protected Overrides Sub FillNonPairwiseDataTable(ByVal Table As System.Data.DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.MeasureType() <> ECMeasureType.mtPairwise) Then
                    For Each Data As clsNonPairwiseMeasureData In CType(Node.Judgments, clsNonPairwiseJudgments).JudgmentsFromUser(UserId)
                        If (Not Data.IsUndefined) Then
                            Dim Row As DataRow = Table.NewRow()
                            Row("SNAPSHOTID") = SnapshotId
                            Row("PROJECTID") = ProjectId
                            Row("USERID") = UserId
                            Dim NodeId As Integer
                            If (Node.IsTerminalNode) Then
                                NodeId = -(Data.NodeID + 100)
                            Else
                                NodeId = Data.NodeID
                            End If
                            Row("PARENTNODEID") = Data.ParentNodeID
                            Row("NODEID") = NodeId
                            If (Node.MeasureType() = ECMeasureType.mtRatings) Then
                                Row("INPUTVALUE") = CType(Data, clsRatingMeasureData).Rating.ID
                            Else
                                Row("INPUTVALUE") = CType(Data.ObjectValue, Single)
                            End If
                            Row("VALUE") = Data.SingleValue
                            Row("COMMENT") = Data.Comment
                            Row("MODIFYTIME") = Data.ModifyDate
                            Table.Rows.Add(Row)
                        End If
                    Next
                End If
            Next
        End Sub

        Protected Overrides Sub FillPairwiseDataTable(ByVal Table As System.Data.DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.MeasureType() = ECMeasureType.mtPairwise) Then
                    For Each Data As clsPairwiseMeasureData In CType(Node.Judgments, clsPairwiseJudgments).JudgmentsFromUser(UserId)
                        If (Not Data.IsUndefined) Then
                            Dim Row As DataRow = Table.NewRow()
                            Row("SNAPSHOTID") = SnapshotId
                            Row("PROJECTID") = ProjectId
                            Row("USERID") = UserId
                            Dim FirstNode As Integer
                            Dim SecondNode As Integer
                            If Node.IsTerminalNode Then
                                FirstNode = -(Data.FirstNodeID + 100)
                                SecondNode = -(Data.SecondNodeID + 100)
                            Else
                                FirstNode = Data.FirstNodeID
                                SecondNode = Data.SecondNodeID
                            End If
                            Row("WRTNODEID") = Data.ParentNodeID
                            Row("FIRSTNODEID") = FirstNode
                            Row("SECONDNODEID") = SecondNode
                            Row("ADVANTAGE") = Data.Advantage
                            Row("PWVALUE") = Data.Value
                            Row("COMMENT") = Data.Comment
                            Row("MODIFYTIME") = Data.ModifyDate
                            Table.Rows.Add(Row)
                        End If
                    Next
                End If
            Next
        End Sub

        Protected Overrides Sub GenericInsertAltsContributions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "AltsContributions(SnapshotId, ProjectId, AltNodeId, CovObjNodeId) VALUES(?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "AltNodeId", "CovObjNodeId"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull})

            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For i = 0 To Hierarchy.Nodes.Count - 1
                Dim Node As clsNode = Hierarchy.Nodes(i)
                If (Node.IsTerminalNode) Then
                    Cmd.Parameters("CovObjNodeId").Value = Node.NodeID
                    Dim Alts As List(Of clsNode) = Node.GetNodesBelow(ECTypes.UNDEFINED_USER_ID, False, 0)
                    For j = 0 To Alts.Count - 1
                        Dim Alt As clsNode = Alts(j)
                        Cmd.Parameters("AltNodeId").Value = -(Alt.NodeID + 100)
                        Cmd.ExecuteNonQuery()
                    Next j
                End If
            Next i
        End Sub

        Protected Overrides Sub GenericInsertAltsRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "AltsRestrictions(SnapshotId, ProjectId, UserId, NodeId, AltNodeId) VALUES(?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "UserId", "NodeId", "AltNodeId"}, _
                New Object() {SnapshotId, ProjectId, UserId, _DBNull, _DBNull})

            GenericDB.ParseCommandQuery(Provider, Cmd)
            Cmd.Transaction = Trans
            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.IsTerminalNode) Then
                    Cmd.Parameters("NodeId").Value = Node.NodeID
                    For Each Alt As clsNode In CType(Node.UserPermissions(UserId).SpecialPermissions, clsNodePermissions).RestrictedNodesBelow
                        Cmd.Parameters("AltNodeId").Value = -(Alt.NodeID + 100)
                        Cmd.ExecuteNonQuery()
                    Next
                End If
            Next
        End Sub

        Protected Overrides Sub GenericInsertDisabledNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "UserDisabledNodes(SnapshotId, ProjectId, UserId, NodeId) VALUES(?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "UserId", "NodeId"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull})

            InsertDisabledHierarchyNodes(pm.Hierarchies(pm.ActiveHierarchy), Cmd, UserId)
            InsertDisabledHierarchyNodes(pm.AltsHierarchy(pm.ActiveAltsHierarchy), Cmd, UserId)
        End Sub

        Protected Overrides Sub GenericInsertNodes(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "Nodes(SnapshotId, ProjectId, NodeId, OriginalNodeId, Guid, Name, ParentId, IsEnabled, IsAlternative, MType, MMode, Comment, RUCurveID, AUCurveID, RScaleID, SFuncID, DefaultDataInstanceID) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "NodeId", "OriginalNodeId", "Guid", "Name", "ParentId", "IsEnabled", "IsAlternative", "MType", "MMode", "Comment", "RUCurveID", "AUCurveID", "RScaleID", "SFuncID", "DefaultDataInstanceID"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            InsertHierarchyNodes(pm.Hierarchies(pm.ActiveHierarchy), Cmd)
            InsertHierarchyNodes(pm.AltsHierarchy(pm.ActiveAltsHierarchy), Cmd)
        End Sub

        Protected Overrides Sub GenericInsertNodesRestrictions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "NodesRestrictions(SnapshotId, ProjectId, UserId, NodeId) VALUES(?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "UserId", "NodeId"}, _
                New Object() {SnapshotId, ProjectId, UserId, _DBNull})

            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Not Node.UserPermissions(UserId).Write) Then
                    Cmd.Parameters("NodeId").Value = Node.NodeID
                    Cmd.ExecuteNonQuery()
                End If
            Next
        End Sub

        Protected Overrides Sub GenericInsertUserNonPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "NonPairwiseData(SnapshotId, ProjectId, UserId, ParentNodeId, NodeId, InputValue, Value, Comment, ModifyTime) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                    New String() {"SnapshotId", "ProjectId", "UserId", "ParentNodeId", "NodeId", "InputValue", "Value", "Comment", "ModifyTime"}, _
                    New Object() {SnapshotId, ProjectId, UserId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.MeasureType() <> ECMeasureType.mtPairwise) Then
                    For Each Data As clsNonPairwiseMeasureData In CType(Node.Judgments, clsNonPairwiseJudgments).JudgmentsFromUser(UserId)
                        If (Not Data.IsUndefined) Then
                            Dim NodeId As Integer
                            If (Node.IsTerminalNode) Then
                                NodeId = -(Data.NodeID + 100)
                            Else
                                NodeId = Data.NodeID
                            End If
                            Cmd.Parameters("ParentNodeId").Value = Data.ParentNodeID
                            Cmd.Parameters("NodeId").Value = NodeId
                            If (Node.MeasureType() = ECMeasureType.mtRatings) Then
                                Cmd.Parameters("InputValue").Value = CType(Data, clsRatingMeasureData).Rating.ID
                            Else
                                Cmd.Parameters("InputValue").Value = CType(Data.ObjectValue, Single)
                            End If
                            Cmd.Parameters("Value").Value = Data.SingleValue
                            Cmd.Parameters("Comment").Value = Data.Comment
                            Cmd.Parameters("ModifyTime").Value = Data.ModifyDate
                            Cmd.ExecuteNonQuery()
                        End If
                    Next
                End If
            Next
        End Sub

        Protected Overrides Sub GenericInsertUserPairwiseData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer, ByVal UserId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "PairwiseData(SnapshotId, ProjectId, UserId, WRTNodeId, FirstNodeId, SecondNodeId, Advantage, PWValue, Comment, ModifyTime) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "UserId", "WRTNodeId", "FirstNodeId", "SecondNodeId", "Advantage", "PWValue", "Comment", "ModifyTime"}, _
                New Object() {SnapshotId, ProjectId, UserId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            Dim Hierarchy As clsHierarchy = pm.Hierarchies(pm.ActiveHierarchy)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.MeasureType() = ECMeasureType.mtPairwise) Then
                    For Each Data As clsPairwiseMeasureData In CType(Node.Judgments, clsPairwiseJudgments).JudgmentsFromUser(UserId)
                        If (Not Data.IsUndefined) Then
                            Dim FirstNode As Integer
                            Dim SecondNode As Integer
                            If (Node.IsTerminalNode) Then
                                FirstNode = -(Data.FirstNodeID + 100)
                                SecondNode = -(Data.SecondNodeID + 100)
                            Else
                                FirstNode = Data.FirstNodeID
                                SecondNode = Data.SecondNodeID
                            End If
                            Cmd.Parameters("WRTNodeId").Value = Data.ParentNodeID
                            Cmd.Parameters("FirstNodeId").Value = FirstNode
                            Cmd.Parameters("SecondNodeId").Value = SecondNode
                            Cmd.Parameters("Advantage").Value = Data.Advantage
                            Cmd.Parameters("PWValue").Value = Data.Value
                            Cmd.Parameters("Comment").Value = Data.Comment
                            Cmd.Parameters("ModifyTime").Value = Data.ModifyDate
                            Cmd.ExecuteNonQuery()
                        End If
                    Next
                End If
            Next
        End Sub

        Protected Overrides Sub HierarchyNodesFillTable(ByVal Hierarchy As ECCore.clsHierarchy, ByVal Table As System.Data.DataTable, ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            For i = 0 To Hierarchy.Nodes.Count - 1
                Dim Row As DataRow = Table.NewRow()
                Dim Node As clsNode = Hierarchy.Nodes(i)
                Dim mType As ECMeasureType = Node.MeasureType()
                Dim ScaleId As Integer = Node.MeasurementScaleID
                Row("SNAPSHOTID") = SnapshotId
                Row("PROJECTID") = ProjectId
                If (Node.IsAlternative) Then
                    Row("NODEID") = -(Node.NodeID + 100)
                Else
                    Row("NODEID") = Node.NodeID
                End If
                Row("ORIGINALNODEID") = Node.NodeID
                Row("GUID") = Node.NodeGuidID.ToString()
                Row("NAME") = Node.NodeName
                Row("PARENTID") = Node.ParentNodeID
                Row("ISENABLED") = System.Convert.ToInt16(Node.Enabled)
                Row("ISALTERNATIVE") = System.Convert.ToInt16(Node.IsAlternative)
                Row("MTYPE") = mType
                Row("MMODE") = Node.MeasureMode
                Row("COMMENT") = Node.Comment

                Select Case mType
                    Case ECMeasureType.mtRegularUtilityCurve
                        Row("RUCURVEID") = ScaleId
                    Case ECMeasureType.mtAdvancedUtilityCurve
                        Row("AUCURVEID") = ScaleId
                    Case ECMeasureType.mtRatings
                        Row("RSCALEID") = ScaleId
                    Case ECMeasureType.mtStep
                        Row("SFUNCID") = ScaleId
                End Select

                If (Node.DefaultDataInstance() IsNot Nothing) Then
                    Row("DEFAULTDATAINSTANCEID") = Node.DefaultDataInstance.ID
                End If
                Table.Rows.Add(Row)
            Next i
        End Sub

        Protected Overrides Sub InsertAdvancedUtilityCurves(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "AdvancedUtilityCurves(SnapshotId, ProjectId, CurveId, Guid, Name, Low, High, InterpolationMethod, Comment) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "CurveId", "Guid", "Name", "Low", "High", "InterpolationMethod", "Comment"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.AdvancedUtilityCurves.Count - 1
                Dim Curve As clsAdvancedUtilityCurve = pm.MeasureScales.AdvancedUtilityCurves(i)
                Cmd.Parameters("CurveId").Value = Curve.ID
                Cmd.Parameters("Guid").Value = Curve.GuidID.ToString()
                Cmd.Parameters("Name").Value = Curve.Name
                Cmd.Parameters("Low").Value = Curve.Low
                Cmd.Parameters("High").Value = Curve.High
                Cmd.Parameters("InterpolationMethod").Value = Curve.InterpolationMethod
                Cmd.Parameters("Comment").Value = Curve.Comment
                Cmd.ExecuteNonQuery()
            Next i
        End Sub

        Protected Overrides Sub InsertCurvesPoints(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "CurvesPoints(SnapshotId, ProjectId, CurveId, XValue, YValue) VALUES(?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "CurveId", "XValue", "YValue"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.AdvancedUtilityCurves.Count - 1
                Dim Curve As clsAdvancedUtilityCurve = pm.MeasureScales.AdvancedUtilityCurves(i)
                Cmd.Parameters("CurveId").Value = Curve.ID
                For j = 0 To Curve.Points.Count - 1
                    Dim p As clsUCPoint = Curve.Points(j)
                    Cmd.Parameters("XValue").Value = p.X
                    Cmd.Parameters("YValue").Value = p.Y
                    Cmd.ExecuteNonQuery()
                Next j
            Next i
        End Sub

        Protected Overrides Sub InsertDataInstances(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "DataInstances(SnapshotId, ProjectId, Id, UserId, Name, Comment) VALUES (?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "Id", "UserId", "Name", "Comment"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull})

            For Each Data As ECTypes.clsDataInstance In pm.DataInstances
                Cmd.Parameters("Id").Value = Data.ID
                Cmd.Parameters("UserId").Value = Data.User.UserID
                Cmd.Parameters("Name").Value = Data.Name
                Cmd.Parameters("Comment").Value = Data.Comment
                Cmd.ExecuteNonQuery()
            Next
        End Sub

        Protected Overrides Sub InsertDisabledHierarchyNodes(ByVal Hierarchy As ECCore.clsHierarchy, ByVal Cmd As System.Data.Common.DbCommand, ByVal UserId As Integer)
            For Each Node As clsNode In Hierarchy.Nodes
                If (Node.DisabledForUser(UserId)) Then
                    Cmd.Parameters("UserId").Value = UserId
                    If (Node.IsAlternative) Then
                        Cmd.Parameters("NodeId").Value = -(Node.NodeID + 100)
                    Else
                        Cmd.Parameters("NodeId").Value = Node.NodeID
                    End If
                    Cmd.ExecuteNonQuery()
                End If
            Next
        End Sub

        Protected Overrides Sub InsertHierarchyNodes(ByVal Hierarchy As ECCore.clsHierarchy, ByVal Cmd As System.Data.Common.DbCommand)
            For i = 0 To Hierarchy.Nodes.Count - 1
                Dim Node As clsNode = Hierarchy.Nodes(i)
                Dim mType As ECMeasureType = Node.MeasureType()
                Dim ScaleId As Integer = Node.MeasurementScaleID
                If (Node.IsAlternative) Then
                    Cmd.Parameters("NodeId").Value = -(Node.NodeID + 100)
                Else
                    Cmd.Parameters("NodeId").Value = Node.NodeID
                End If
                Cmd.Parameters("OriginalNodeId").Value = Node.NodeID
                Cmd.Parameters("Guid").Value = Node.NodeGuidID.ToString()
                Cmd.Parameters("Name").Value = Node.NodeName
                Cmd.Parameters("ParentId").Value = Node.ParentNodeID
                Cmd.Parameters("IsEnabled").Value = System.Convert.ToInt16(Node.Enabled)
                Cmd.Parameters("IsAlternative").Value = System.Convert.ToInt16(Node.IsAlternative)
                Cmd.Parameters("MType").Value = mType
                Cmd.Parameters("MMode").Value = Node.MeasureMode
                Cmd.Parameters("Comment").Value = Node.Comment
                Cmd.Parameters("RUCurveID").Value = _DBNull
                Cmd.Parameters("AUCurveID").Value = _DBNull
                Cmd.Parameters("RScaleID").Value = _DBNull
                Cmd.Parameters("SFuncID").Value = _DBNull
                Select Case mType
                    Case ECMeasureType.mtRegularUtilityCurve
                        Cmd.Parameters("RUCurveID").Value = ScaleId
                    Case ECMeasureType.mtAdvancedUtilityCurve
                        Cmd.Parameters("AUCurveID").Value = ScaleId
                    Case ECMeasureType.mtRatings
                        Cmd.Parameters("RScaleID").Value = ScaleId
                    Case ECMeasureType.mtStep
                        Cmd.Parameters("SFuncID").Value = ScaleId
                End Select
                If (Node.DefaultDataInstance IsNot Nothing) Then
                    Cmd.Parameters("DefaultDataInstanceID").Value = Node.DefaultDataInstance.ID
                End If
                Cmd.ExecuteNonQuery()
            Next i
        End Sub

        Protected Overrides Sub InsertProjectData(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
            Dim Sql As String = _
                "insert into " + Catalog + "Projects(SnapshotID, WorkgroupID, ProjectID, OwnerID, Passcode, FileName, ProjectName, Status, Comment, MeetingID, MeetingOwnerID, LockStatus, LockedByUserID, LockExpiration, Created, LastModified, LastVisited) " + _
                "select ?, WorkgroupID, ID, OwnerID, Passcode, FileName, ProjectName, Status, Comment, MeetingID, MeetingOwnerID, LockStatus, LockedByUserID, LockExpiration, Created, LastModify, LastVisited from " + _
                MasterCatalog + "Projects"
            If (ProjectId IsNot Nothing) Then
                ExecuteNonQuery(Sql + " where ID = ?", New String() {"SnapshotId", "ProjectId"}, New Object() {SnapshotId, ProjectId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub InsertRatingIntensities(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "RatingIntensities(SnapshotId, ProjectId, RScaleId, IntensityId, Guid, Name, IntensityValue, Comment) VALUES (?, ?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "RScaleId", "IntensityId", "Guid", "Name", "IntensityValue", "Comment"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.RatingsScales.Count - 1
                Dim Scale As clsRatingScale = pm.MeasureScales.RatingsScales(i)
                Cmd.Parameters("RScaleId").Value = Scale.ID
                For j = 0 To Scale.RatingSet.Count - 1
                    Dim r As clsRating = Scale.RatingSet(j)
                    Cmd.Parameters("IntensityId").Value = r.ID
                    Cmd.Parameters("Guid").Value = r.GuidID.ToString()
                    Cmd.Parameters("Name").Value = r.Name
                    Cmd.Parameters("IntensityValue").Value = r.Value
                    Cmd.Parameters("Comment").Value = r.Comment
                    Cmd.ExecuteNonQuery()
                Next j
            Next i
        End Sub

        Protected Overrides Sub InsertRatingScales(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "RatingScales(SnapshotId, ProjectId, RScaleId, Guid, Name, IsExplicitlySet, Comment) VALUES (?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "RScaleId", "Guid", "Name", "IsExplicitlySet", "Comment"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.RatingsScales.Count - 1
                Dim Scale As clsRatingScale = pm.MeasureScales.RatingsScales(i)
                Cmd.Parameters("RScaleId").Value = Scale.ID
                Cmd.Parameters("Guid").Value = Scale.GuidID.ToString()
                Cmd.Parameters("Name").Value = Scale.Name
                Cmd.Parameters("IsExplicitlySet").Value = 0 'check with AC
                Cmd.Parameters("Comment").Value = Scale.Comment
                Cmd.ExecuteNonQuery()
            Next i
        End Sub

        Protected Overrides Sub InsertRoleActions(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
            Dim Sql As String = _
                    "insert into " + Catalog + "RoleActions(SnapshotID, RoleGroupID, ActionID, ActionType, Status, Comment) " + _
                    "select ?, a.RoleGroupID, a.ID, a.ActionType, a.Status, a.Comment from " + MasterCatalog + "RoleActions a"
            If (WorkgroupId IsNot Nothing) Then
                ExecuteNonQuery(Sql + ", " + MasterCatalog + _
                    "RoleGroups b where a.RoleGroupID = b.ID and b.WorkgroupID = ?", _
                    New String() {"SnapshotId", "WorkgroupId"}, New Object() {SnapshotId, WorkgroupId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub InsertRoleGroups(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
            Dim Sql As String = _
                    "insert into " + Catalog + "RoleGroups(SnapshotID, RoleGroupID, WorkgroupID, RoleLevel, GroupType, Status, Name, Comment, Created, LastModified) " + _
                    "select ?, ID, WorkgroupID, RoleLevel, GroupType, Status, Name, Comment, Created, LastModify from " + MasterCatalog + "RoleGroups"
            If (WorkgroupId IsNot Nothing) Then
                ExecuteNonQuery(Sql + " where WorkgroupID = ?", New String() {"SnapshotId", "WorkgroupId"}, New Object() {SnapshotId, WorkgroupId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub InsertStepFunctions(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "StepFunctions(SnapshotId, ProjectId, FunctionId, Guid, Name, Comment, IsPiecewiseLinear) VALUES (?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "FunctionId", "Guid", "Name", "Comment", "IsPiecewiseLinear"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.StepFunctions.Count - 1
                Dim f As clsStepFunction = pm.MeasureScales.StepFunctions(i)
                Cmd.Parameters("FunctionId").Value = f.ID
                Cmd.Parameters("Guid").Value = f.GuidID.ToString()
                Cmd.Parameters("Name").Value = f.Name
                Cmd.Parameters("Comment").Value = f.Comment
                Cmd.Parameters("IsPiecewiseLinear").Value = f.IsPiecewiseLinear
                Cmd.ExecuteNonQuery()
            Next i
        End Sub

        Protected Overrides Sub InsertStepIntervals(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "StepIntervals(SnapshotId, ProjectId, FunctionId, IntervalId, Name, LowX, HighX, IntervalValue, Comment) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                    New String() {"SnapshotId", "ProjectId", "FunctionId", "IntervalId", "Name", "LowX", "HighX", "IntervalValue", "Comment"}, _
                    New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.StepFunctions.Count - 1
                Dim f As clsStepFunction = pm.MeasureScales.StepFunctions(i)
                Cmd.Parameters("FunctionId").Value = f.ID
                For j = 0 To f.Intervals.Count - 1
                    Dim si As clsStepInterval = f.Intervals(j)
                    Cmd.Parameters("IntervalId").Value = si.ID
                    Cmd.Parameters("Name").Value = si.Name
                    Cmd.Parameters("LowX").Value = si.Low
                    Cmd.Parameters("HighX").Value = si.High
                    Cmd.Parameters("IntervalValue").Value = si.Value
                    Cmd.Parameters("Comment").Value = si.Comment
                    Cmd.ExecuteNonQuery()
                Next j
            Next i
        End Sub

        Protected Overloads Overrides Sub InsertUsers(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "ProjectUsers(SnapshotId, ProjectId, UserId, Guid, Email, FullName, Active, Comment, LastJudgmentTime, DataInstanceID, GroupID, VotingBoxID, IncludedInSynchronous, SyncEvalMode) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "UserId", "Guid", "Email", "FullName", "Active", "Comment", "LastJudgmentTime", "DataInstanceID", "GroupID", "VotingBoxID", "IncludedInSynchronous", "SyncEvalMode"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For Each User As ECTypes.clsUser In pm.UsersList
                Cmd.Parameters("UserId").Value = User.UserID
                Cmd.Parameters("Guid").Value = User.UserGuidID
                Cmd.Parameters("Email").Value = User.UserEMail
                Cmd.Parameters("FullName").Value = User.UserName
                Cmd.Parameters("Active").Value = System.Convert.ToInt16(User.Active)
                Cmd.Parameters("LastJudgmentTime").Value = User.LastJudgmentTime
                Cmd.Parameters("DataInstanceID").Value = User.DataInstanceID
                If (User.EvaluationGroup IsNot Nothing) Then
                    Cmd.Parameters("GroupID").Value = User.EvaluationGroup.ID
                Else
                    Cmd.Parameters("GroupID").Value = _DBNull
                End If
                Cmd.Parameters("VotingBoxID").Value = User.VotingBoxID
                Cmd.Parameters("IncludedInSynchronous").Value = System.Convert.ToInt16(User.IncludedInSynchronous)
                Cmd.Parameters("SyncEvalMode").Value = User.SyncEvaluationMode
                Cmd.ExecuteNonQuery()
            Next
        End Sub

        Protected Overloads Overrides Sub InsertUsers(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
            Dim Sql As String = _
                    "insert into " + Catalog + "Users(SnapshotID, UserID, OwnerID, EMail, Password, Status, FullName, Comment, DefaultWGID, Created, LastModified, LastVisited, LastPageID, LastURL, LastWorkgroupID, IsOnline, EULAVersion, SessionID) " + _
                    "select ?, a.ID, a.OwnerID, a.EMail, a.Password, a.Status, a.FullName, a.Comment, a.DefaultWGID, a.Created, a.LastModify, a.LastVisited, a.LastPageID, a.LastURL, a.LastWorkgroupID, a.IsOnline, a.EULAVersion, a.SessionID from " + _
                    MasterCatalog + "Users a"
            If (ProjectId IsNot Nothing) Then
                ExecuteNonQuery(Sql + ", " + MasterCatalog + _
                    "Workspace b where b.ProjectID = ? and a.ID = b.UserID", _
                    New String() {"SnapshotId", "ProjectId"}, New Object() {SnapshotId, ProjectId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub InsertUserWorkgroups(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
            Dim Sql As String = _
                    "insert into " + Catalog + "UsersWorkgroups(SnapshotID, UserID, WorkgroupID, RoleGroupID, Status, Comment, ExpirationDate, Created, LastVisited, LastProjectID) " + _
                    "select ?, a.UserID, a.WorkgroupID, a.RoleGroupID, a.Status, a.Comment, a.ExpirationDate, a.Created, a.LastVisited, a.LastProjectID from " + MasterCatalog + "UserWorkgroups a"
            If (WorkgroupId IsNot Nothing) Then
                ExecuteNonQuery(Sql + ", Users b where a.WorkgroupID = ? and a.UserID = b.UserID and b.SnapshotID = ?", _
                        New String() {"SnapshotId", "WorkgroupId", "SnapId"}, New Object() {SnapshotId, WorkgroupId, SnapshotId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub InsertUtilityCurves(ByVal SnapshotId As Integer, ByVal ProjectId As Integer)
            Dim Cmd As DbCommand = PrepareCommand("INSERT INTO " + Catalog + "UtilityCurves(SnapshotId, ProjectId, CurveId, Guid, Name, Low, High, Curvature, Comment) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?)", _
                New String() {"SnapshotId", "ProjectId", "CurveId", "Guid", "Name", "Low", "High", "Curvature", "Comment"}, _
                New Object() {SnapshotId, ProjectId, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull, _DBNull})

            For i = 0 To pm.MeasureScales.RegularUtilityCurves.Count - 1
                Dim Curve As clsRegularUtilityCurve = pm.MeasureScales.RegularUtilityCurves(i)
                Cmd.Parameters("CurveId").Value = Curve.ID
                Cmd.Parameters("Guid").Value = Curve.GuidID.ToString()
                Cmd.Parameters("Name").Value = Curve.Name
                Cmd.Parameters("Low").Value = Curve.Low
                Cmd.Parameters("High").Value = Curve.High
                Cmd.Parameters("Curvature").Value = Curve.Curvature
                Cmd.Parameters("Comment").Value = Curve.Comment
                Cmd.ExecuteNonQuery()
            Next i
        End Sub

        Protected Overrides Sub InsertWorkgroups(ByVal SnapshotId As Integer, ByVal WorkgroupId As Integer?)
            Dim Sql As String = _
                    "insert into " + Catalog + "Workgroups(SnapshotID, WorkgroupID, OwnerID, ECAMID, Name, Status, LicenseData, LicenseKey, Comment, Created, LastModified, LastVisited) " + _
                    "select ?, ID, OwnerID, ECAMID, Name, Status, LicenseData, LicenseKey, Comment, Created, LastModify, LastVisited from " + MasterCatalog + "Workgroups"
            If (WorkgroupId IsNot Nothing) Then
                ExecuteNonQuery(Sql + " where ID = ?", New String() {"SnapshotId", "WorkgroupId"}, New Object() {SnapshotId, WorkgroupId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Sub InsertWorkspace(ByVal SnapshotId As Integer, ByVal ProjectId As Integer?)
            Dim Sql As String = _
                "insert into " + Catalog + "Workspace(SnapshotID, UserID, ProjectID, GroupID, Status, Comment, Step, Created, LastModified) " + _
                "select ?, UserID, ProjectID, GroupID, Status, Comment, Step, Created, LastModify from " + MasterCatalog + "Workspace"
            If (ProjectId IsNot Nothing) Then
                ExecuteNonQuery(Sql + " where ProjectID = ?", _
                        New String() {"SnapshotId", "ProjectId"}, New Object() {SnapshotId, ProjectId})
            Else
                ExecuteNonQuery(Sql, New String() {"SnapshotId"}, New Object() {SnapshotId})
            End If
        End Sub

        Protected Overrides Function MakeAltsContributionsDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                    New String() {"SNAPSHOTID", "PROJECTID", "ALTNODEID", "COVOBJNODEID"}, _
                    New Type() {_Int32, _Int32, _Int32, _Int32})
        End Function

        Protected Overrides Function MakeAltsRestrictionsDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                New String() {"SNAPSHOTID", "PROJECTID", "USERID", "NODEID", "ALTNODEID"}, _
                New Type() {_Int32, _Int32, _Int32, _Int32, _Int32})
        End Function

        Protected Overrides Function MakeDisabledNodesDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                New String() {"SNAPSHOTID", "PROJECTID", "USERID", "NODEID"}, _
                New Type() {_Int32, _Int32, _Int32, _Int32})
        End Function

        Protected Overrides Function MakeNodesDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                New String() {"SNAPSHOTID", "PROJECTID", "NODEID", "ORIGINALNODEID", "GUID", "NAME", "PARENTID", "ISENABLED", "ISALTERNATIVE", "MTYPE", "MMODE", "COMMENT", "RUCURVEID", "AUCURVEID", "RSCALEID", "SFUNCID", "DEFAULTDATAINSTANCEID"}, _
                New Type() {_Int32, _Int32, _Int32, _Int32, _String, _String, _Int32, _Int32, _Int32, _Int32, _Int32, _String, _Int32, _Int32, _Int32, _Int32, _Int32})
        End Function

        Protected Overrides Function MakeNodesRestrictionsDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                New String() {"SNAPSHOTID", "PROJECTID", "USERID", "NODEID"}, _
                New Type() {_Int32, _Int32, _Int32, _Int32})
        End Function

        Protected Overrides Function MakeNonPairwiseDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                New String() {"SNAPSHOTID", "PROJECTID", "USERID", "PARENTNODEID", "NODEID", "INPUTVALUE", "VALUE", "COMMENT", "MODIFYTIME"}, _
                New Type() {_Int32, _Int32, _Int32, _Int32, _Int32, _Single, _Single, _String, _DateTime})
        End Function

        Protected Overrides Function MakePairwiseDataTable() As System.Data.DataTable
            Return MakeDataTable( _
                New String() {"SNAPSHOTID", "PROJECTID", "USERID", "WRTNODEID", "FIRSTNODEID", "SECONDNODEID", "ADVANTAGE", "PWVALUE", "COMMENT", "MODIFYTIME"}, _
                New Type() {_Int32, _Int32, _Int32, _Int32, _Int32, _Int32, _Int32, _Single, _String, _DateTime})
        End Function

        Public Sub New(ByVal DbProvider As GenericDB.DBProviderType, ByVal StreamDb As String, ByVal DestDb As String, ByVal DestCat As String, ByVal MasterCat As String)
            Provider = DbProvider
            SrcConn = StreamDb
            DstConn = DestDb
            Catalog = DestCat
            MasterCatalog = MasterCat
            pm.StorageManager.StorageType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase
        End Sub

    End Class

End Namespace