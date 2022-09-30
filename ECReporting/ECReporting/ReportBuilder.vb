Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Data
Imports System.Data.Common
Imports GenericDBAccess.ECGenericDatabaseAccess

Namespace Reporting

    Public Class ReportBuilder
        Protected mProvider As GenericDB.DBProviderType
        Protected mConnectionString As String
        Protected Conn As DbConnection

        Protected Function Connected() As Boolean
            If (Conn Is Nothing) Then
                Return False
            End If
            Return (Conn.State = ConnectionState.Open)
        End Function

        Protected Sub Connect()
            Disconnect()
            Conn = GenericDB.GetDBConnection(mProvider, mConnectionString)
            Conn.Open()
        End Sub

        Protected Sub Disconnect()
            If (Connected()) Then
                Conn.Close()
            End If
        End Sub

        Protected Function PrepareCommand(ByVal Cmd As String, ByVal Names As String(), ByVal Values As Object()) As DbCommand
            Dim Command As DbCommand = GenericDB.GetDBCommand(mProvider)
            Command.CommandText = Cmd
            Command.Connection = Conn
            For i = 0 To Names.Count() - 1
                Command.Parameters.Add(GenericDB.GetDBParameter(mProvider, Names(i), Values(i)))
            Next i
            GenericDB.ParseCommandQuery(mProvider, Command)
            Return Command
        End Function

        Protected Overridable Function ReportQuery(ByVal ReportId As Integer) As String
            Dim Cmd As DbCommand = PrepareCommand("select ReportQuery from ReportsQueries where ReportId = ?", _
                New String() {"ReportId"}, New Object() {ReportId})
            Dim Reader As DbDataReader = Cmd.ExecuteReader()
            Dim Query As String = ""
            While (Reader.Read())
                Query = Reader.GetString(0)
                Exit While
            End While
            Reader.Close()
            Return Query
        End Function

        Protected Overridable Function ReportQuery(ByVal ReportName As String) As String
            Dim Cmd As DbCommand = PrepareCommand("select ReportQuery from ReportsQueries where ReportName = ?", _
                New String() {"ReportName"}, New Object() {ReportName})
            Dim Reader As DbDataReader = Cmd.ExecuteReader()
            Dim Query As String = ""
            While (Reader.Read())
                Query = Reader.GetString(0)
                Exit While
            End While
            Reader.Close()
            Return Query
        End Function

        Protected Overridable Function PrepareReportCmd(ByVal Query As String, ByVal ProjectId As Integer, ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DbCommand
            If (Query Is Nothing) Then
                Return Nothing
            End If
            If (Query.Trim() = "") Then
                Return Nothing
            End If
            Dim Values As Object()
            If (ReportDate > New DateTime(1900, 1, 1)) Then
                Values = ParamValues.Concat(New Object() {ProjectId, ReportDate}).ToArray()
            Else
                Values = ParamValues.Concat(New Object() {ProjectId, DBNull.Value}).ToArray()
            End If
            Dim Names As String() = ParamNames.Concat(New String() {"@SYS$PROJECT_ID", "SYS$SNAPSNOT_TIME"}).ToArray()
            Return PrepareCommand(Query, Names, Values)
        End Function

        Protected Function PrepareReportCommand(ByVal ProjectId As Integer, ByVal ReportId As Integer, ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DbCommand
            Return PrepareReportCmd(ReportQuery(ReportId), ProjectId, ReportDate, ParamNames, ParamValues)
        End Function

        Protected Function PrepareReportCommand(ByVal ProjectId As Integer, ByVal ReportName As String, ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DbCommand
            Return PrepareReportCmd(ReportQuery(ReportName), ProjectId, ReportDate, ParamNames, ParamValues)
        End Function

        Protected Sub FillDataSet(ByVal Cmd As DbCommand, ByVal Ds As DataSet)
            If (Cmd IsNot Nothing) Then
                Dim Adapter As DbDataAdapter = CType(GenericDB.GetDBDataAdapter(mProvider, "", Conn), DbDataAdapter)
                Adapter.SelectCommand = Cmd
                Adapter.Fill(Ds)
            End If
        End Sub

        Public Overridable Function BuildReport(ByVal ProjectId As Integer, ByVal ReportId As Integer, ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DataSet
            Dim Result As DataSet = New DataSet()
            Connect()
            Try
                FillDataSet(PrepareReportCommand(ProjectId, ReportId, ReportDate, ParamNames, ParamValues), Result)
            Finally
                Disconnect()
            End Try
            Return Result
        End Function

        Public Overridable Function BuildReport(ByVal ProjectId As Integer, ByVal ReportName As String, ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DataSet
            Dim Result As DataSet = New DataSet()
            Connect()
            Try
                FillDataSet(PrepareReportCommand(ProjectId, ReportName, ReportDate, ParamNames, ParamValues), Result)
            Finally
                Disconnect()
            End Try
            Return Result
        End Function

        Public Overridable Function BuildReports(ByVal ProjectId As Integer, ByVal Reports As Integer(), ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DataSet()
            Dim Result(Reports.Count() - 1) As DataSet
            Connect()
            Try
                For i = 0 To Reports.Count - 1
                    Dim Ds As DataSet = New DataSet()
                    FillDataSet(PrepareReportCommand(ProjectId, Reports(i), ReportDate, ParamNames, ParamValues), Ds)
                    Result(i) = Ds
                Next i
            Finally
                Disconnect()
            End Try
            Return Result
        End Function

        Public Overridable Function BuildReports(ByVal ProjectId As Integer, ByVal Reports As String(), ByVal ReportDate As DateTime, ByVal ParamNames As String(), ByVal ParamValues As Object()) As DataSet()
            Dim Result(Reports.Count() - 1) As DataSet
            Connect()
            Try
                For i = 0 To Reports.Count() - 1
                    Dim Ds As DataSet = New DataSet()
                    FillDataSet(PrepareReportCommand(ProjectId, Reports(i), ReportDate, ParamNames, ParamValues), Ds)
                    Result(i) = Ds
                Next i
            Finally
                Disconnect()
            End Try
            Return Result
        End Function

        Public Sub New(ByVal DbProvider As GenericDB.DBProviderType, ByVal ConnStr As String)
            mProvider = DbProvider
            mConnectionString = ConnStr
        End Sub

        Public Property Provider() As GenericDB.DBProviderType
            Get
                Return mProvider
            End Get
            Set(ByVal value As GenericDB.DBProviderType)
                mProvider = value
            End Set
        End Property

        Public Property ConnectionString() As String
            Get
                Return mConnectionString
            End Get
            Set(ByVal value As String)
                mConnectionString = value
            End Set
        End Property

    End Class

End Namespace
