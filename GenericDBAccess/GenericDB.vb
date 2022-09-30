Imports System.Data.Common

Namespace ECGenericDatabaseAccess

    Public Module GenericDB

        Public Const _DB_COMMAND_TIMEOUT As Integer = 180   ' D6007

        Public Enum DBProviderType 'C0235
            dbptUnspecified = -1 ' D0457
            dbptSQLClient = 0
            dbptOLEDB = 1
            dbptOracle = 2
            dbptODBC = 3
        End Enum

        'Public Function CheckDBConnection(ProviderType, ByVal sConnString As String) As Boolean 'C0236
        Public Function CheckDBConnection(ByVal ProviderType As DBProviderType, ByVal sConnString As String) As Boolean 'C0236
            Dim fAvailable As Boolean = True
            Using fTempDB As DbConnection = GetDBConnection(ProviderType, sConnString) 'C0236 + D2227
                Try
                    'fTempDB.ConnectionString = sConnString 'C0236
                    fTempDB.Open()
                Catch ex As Exception
                    fAvailable = False
                Finally
                    fTempDB.Close()
                End Try
            End Using
            Return fAvailable
        End Function

        'Public Function TableExists(ByVal ConnectionString As String, ByVal TableName As String) As Boolean 'C0236
        Public Function TableExists(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, ByVal TableName As String) As Boolean 'C0236
            If Not CheckDBConnection(ProviderType, ConnectionString) Then
                Return False
            End If

            ' open schema to read table names
            'Dim dbConnection As New Data.odbc.odbcConnection(ConnectionString) 'C0236
            Dim dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString) 'C0236

            dbConnection.Open()

            Dim schemaTable As System.Data.DataTable
            'schemaTable = dbConnection.GetSchema(Data.odbc.odbcMetaDataCollectionNames.Tables)    ' D0065 'C0236
            schemaTable = GetDBSchemaTables(ProviderType, ConnectionString) 'C0236

            For i As Integer = 0 To schemaTable.Rows.Count - 1
                If schemaTable.Rows(i)!TABLE_NAME.ToString = TableName Then
                    dbConnection.Close()    'C0058
                    dbConnection = Nothing  'C0058
                    Return True
                End If
            Next

            dbConnection.Close()    ' D0166
            dbConnection = Nothing  ' D0166

            Return False
        End Function

        Public Function GetDBConnection(ByVal ProviderType As DBProviderType, ByVal ConnectionString As String) As Common.DbConnection 'C0235
            Select Case ProviderType
                Case DBProviderType.dbptODBC
                    Return New Odbc.OdbcConnection(ConnectionString) 'C0830
                Case DBProviderType.dbptOLEDB 'C0309
                    Return New OleDb.OleDbConnection(ConnectionString)
                    'Return New OleDb.OleDbConnection("Provider=sqloledb;Data Source=AlexSony;Initial Catalog=CanvasProject3;User Id=sa;Password=ecionly;")
                Case DBProviderType.dbptSQLClient
                    Return New SqlClient.SqlConnection(ConnectionString)
                    'Return New SqlClient.SqlConnection("Data Source=AlexSony;Initial Catalog=CanvasProject3;User Id=sa;Password=ecionly;")
                Case Else
                    ' return odbc by default
                    Return New Odbc.OdbcConnection(ConnectionString)
            End Select
        End Function

        Public Function GetDBDataAdapter(ByVal ProviderType As DBProviderType, ByVal Query As String, ByVal Connection As DbConnection) As Common.DataAdapter 'C0253
            Select Case ProviderType
                Case DBProviderType.dbptODBC
                    Return New Odbc.OdbcDataAdapter(Query, Connection)
                Case DBProviderType.dbptOLEDB
                    Return New OleDb.OleDbDataAdapter(Query, Connection)
                    'Return New OleDb.OleDbConnection("Provider=sqloledb;Data Source=AlexSony;Initial Catalog=CanvasProject3;User Id=sa;Password=ecionly;")
                Case DBProviderType.dbptSQLClient
                    Return New SqlClient.SqlDataAdapter(Query, Connection)
                    'Return New SqlClient.SqlConnection("Data Source=AlexSony;Initial Catalog=CanvasProject3;User Id=sa;Password=ecionly;")
                Case Else
                    ' return odbc by default
                    Return New Odbc.OdbcDataAdapter(Query, Connection)
            End Select
        End Function

        Public Function GetDBCommand(ByVal ProviderType As DBProviderType) As Common.DbCommand 'C0235
            Select Case ProviderType
                Case DBProviderType.dbptODBC
                    Return New Odbc.OdbcCommand()
                Case DBProviderType.dbptOLEDB
                    Return New OleDb.OleDbCommand()
                Case DBProviderType.dbptSQLClient
                    Return New SqlClient.SqlCommand() With {
                        .CommandTimeout = _DB_COMMAND_TIMEOUT   ' D6007
                    }
                Case Else
                    ' return odbc by default
                    Return New Odbc.OdbcCommand()
            End Select
        End Function

        'Public Function GetDBParameter(ByVal ProviderType As DBProviderType, Optional ByVal Value As Object = Nothing) As Common.DbParameter 'C0235 'C0237
        Public Function GetDBParameter(ByVal ProviderType As DBProviderType, ByVal Name As String, ByVal Value As Object) As Common.DbParameter 'C0235 'C0237
            Dim param As Common.DbParameter
            Select Case ProviderType
                Case DBProviderType.dbptODBC
                    param = New Odbc.OdbcParameter
                Case DBProviderType.dbptOLEDB
                    param = New OleDb.OleDbParameter
                Case DBProviderType.dbptSQLClient
                    param = New SqlClient.SqlParameter
                Case Else
                    ' return odbc by default
                    param = New Odbc.OdbcParameter
            End Select

            If param IsNot Nothing Then
                param.ParameterName = Name 'C0237
                param.Value = Value
            End If
            Return param
        End Function

        Public Function GetDBSchemaTables(ByVal ProviderType As DBProviderType, ByVal ConnectionString As String) As System.Data.DataTable
            If Not CheckDBConnection(ProviderType, ConnectionString) Then
                Return Nothing
            End If

            ' open schema to read table names
            Dim dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString) 'C0236
            dbConnection.Open()

            Dim schemaTable As System.Data.DataTable = Nothing

            Select Case ProviderType
                Case DBProviderType.dbptODBC
                    schemaTable = dbConnection.GetSchema(Data.Odbc.OdbcMetaDataCollectionNames.Tables)
                Case DBProviderType.dbptOLEDB
                    schemaTable = dbConnection.GetSchema(Data.OleDb.OleDbMetaDataCollectionNames.Tables)
                Case DBProviderType.dbptSQLClient
                    schemaTable = dbConnection.GetSchema(Data.SqlClient.SqlClientMetaDataCollectionNames.Tables)
            End Select

            dbConnection.Close()
            dbConnection = Nothing

            Return schemaTable
        End Function

        Public Sub ParseCommandQuery(ByVal ProviderType As DBProviderType, ByVal oCommand As DbCommand) 'C0238
            Dim query As String = oCommand.CommandText
            Select Case ProviderType
                Case DBProviderType.dbptSQLClient
                    Dim qPos As Integer = -1
                    For Each p As DbParameter In oCommand.Parameters
                        qPos = query.IndexOf("?")
                        If qPos < 0 Then Exit For
                        query = query.Substring(0, qPos) & "@" & p.ParameterName & query.Substring(qPos + 1)
                    Next
            End Select
            oCommand.CommandText = query
        End Sub

        Public Function DBExecuteNonQuery(ByVal ProviderType As DBProviderType, ByVal oCommand As DbCommand) As Integer 'C0238
            ParseCommandQuery(ProviderType, oCommand)
            ''Debug.Print(oCommand.CommandText) 'C0425
            Return oCommand.ExecuteNonQuery()   ' D0331 DBExecuteNonQuery(ProviderType, oCommand)
        End Function

        Public Function DBExecuteScalar(ByVal ProviderType As DBProviderType, ByVal oCommand As DbCommand) As Object 'C0238
            ParseCommandQuery(ProviderType, oCommand)
            ''Debug.Print(oCommand.CommandText) 'C0425
            Return oCommand.ExecuteScalar() ' D0331 DBExecuteScalar(ProviderType, oCommand)
        End Function

        Public Function DBExecuteReader(ByVal ProviderType As DBProviderType, ByVal oCommand As DbCommand) As DbDataReader 'C0238
            ParseCommandQuery(ProviderType, oCommand)
            ''Debug.Print(oCommand.CommandText) 'C0425
            Return oCommand.ExecuteReader
        End Function

        Public Function GetDBCommandBuilder(ByVal ProviderType As DBProviderType, ByVal DataAdapter As DbDataAdapter) As Common.DbCommandBuilder 'C0342
            Select Case ProviderType
                Case DBProviderType.dbptODBC
                    Return New Odbc.OdbcCommandBuilder(DataAdapter)
                Case DBProviderType.dbptOLEDB
                    Return New OleDb.OleDbCommandBuilder(DataAdapter)
                Case DBProviderType.dbptSQLClient
                    Return New SqlClient.SqlCommandBuilder(DataAdapter)
                Case Else
                    Return New Odbc.OdbcCommandBuilder(DataAdapter)
            End Select
        End Function
    End Module

End Namespace