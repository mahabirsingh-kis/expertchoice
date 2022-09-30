Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.Common
Imports GenericDBAccess.ECGenericDatabaseAccess
Imports ExpertChoice.Service
Imports ExpertChoice.Database

Namespace ExpertChoice.Data

    ''' <summary>
    ''' Class for work with Database server.
    ''' </summary>
    ''' <remarks>This is ODBC.Net version. Dependencies: System.Data.*</remarks>
    Public Class clsDatabaseAdvanced
        Implements IDisposable

        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                _Connection.Close()
            Catch ex As Exception
            End Try
        End Sub

        Public Const _DB_CREATE_TIMEOUT As Integer = 2000   ' D0490
        Public Const _DB_CREATE_TRIES As Integer = 20       ' D0490

        Public Const _DB_CLEAR_POOL As Boolean = False      ' D2235

        ''' <summary>
        ''' Current connection
        ''' </summary>
        ''' <remarks>Should be defined and not be Nothing</remarks>
        Private _Connection As DbConnection = Nothing   ' D0372

        Private _ProviderType As DBProviderType         ' D0372

        ''' <summary>
        ''' String with last error details
        ''' </summary>
        ''' <remarks>Empty while all operations is O.K.</remarks>
        Private sError As String

        ''' <summary>
        ''' Readonly property for getting status of current connection
        ''' </summary>
        ''' <value></value>
        ''' <returns>True, when current connection is active and SQL server is connected.</returns>
        ''' <remarks>Connection must be exists.</remarks>
        Private ReadOnly Property isConnected() As Boolean
            Get
                Return Not Connection Is Nothing AndAlso CBool(Connection.State And System.Data.ConnectionState.Open)  ' D0327 + D0464
            End Get
        End Property

        ''' <summary>
        ''' Current connection string.
        ''' </summary>
        ''' <value>New connection string for existed connection.</value>
        ''' <returns>Full connection string for current connection</returns>
        ''' <remarks>Current (active) connection will be closed, if new connection string is providing.</remarks>
        Public Property ConnectionString() As String
            Get
                If Connection Is Nothing Then Return "" Else Return Connection.ConnectionString ' D0372
            End Get
            Set(ByVal value As String)
                If value <> ConnectionString Then   ' D0372
                    If isConnected Then Close()
                    Try 'AS/15285s enclosed and added Catch
                        Connection.ConnectionString = value
                    Catch ex As Exception
                        ' MsgBox("Connection error occured: " & ex.Message,, "AS debug message from Property ConnectionString()")
                        Debug.Print(ex.Message)
                    End Try
                End If
            End Set
        End Property

        ''' <summary>
        ''' Get reference to current connection
        ''' </summary>
        ''' <value></value>
        ''' <returns>Current connection</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Connection() As DbConnection
            Get
                If _Connection Is Nothing Then
                    'TODO: MF, Usage place
                    _Connection = GetDBConnection(_ProviderType, ConnectionString)
                End If
                Return _Connection
            End Get
        End Property

        ' D0329 ===
        Public ReadOnly Property ProviderType() As DBProviderType
            Get
                Return _ProviderType
            End Get
        End Property
        ' D0329 ==

        ''' <summary>
        ''' Get error details.
        ''' </summary>
        ''' <value></value>
        ''' <returns>String with error descriptions.</returns>
        ''' <remarks>Is empty while don't have any errors. Storing details only for the latest error.</remarks>
        Public Property LastError() As String   ' D2157
            Get
                Return sError
            End Get
            Set(value As String)    ' D2157
                sError = value
            End Set
        End Property

        ''' <summary>
        ''' Get current server name (DataSource of Current Connection)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Connection must be exists.</remarks>
        ReadOnly Property ServerName() As String
            Get
                Return Connection.DataSource
            End Get
        End Property

        ''' <summary>
        ''' Get name for current Database
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Connection must be exists.</remarks>
        Public Property DatabaseName() As String    ' D0372
            Get
                If Connection Is Nothing Then Return "" Else Return Connection.Database
            End Get
            ' D0372 ===
            Set(ByVal value As String)
                If DatabaseName <> value Then
                    ConnectionString = GetConnectionString(value, ProviderType) ' D0457
                End If
            End Set
            ' D0372 ==
        End Property

        ''' <summary>
        ''' Method for reset Error details.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ResetError()
            sError = ""
        End Sub

        ''' <summary>
        ''' Perform SQL connection with specified params.
        ''' </summary>
        ''' <returns>True, if connection was successfully</returns>
        ''' <remarks>LastError have a message while ConnectionString not been provided.
        ''' When RTE occurred, LastError also have a details about connection error.
        ''' </remarks>
        Public Overloads Function Connect() As Boolean
            Try
                If ConnectionString = "" Then
                    sError = "Connection string is empty."
                    Return False
                End If
                ' D0329 ===
                If Not isConnected Then
                    Connection.Open()
                    DebugInfo(String.Format("Connect to DB with connection string '{0}'", ShortString(ConnectionString, 200, True)))
                End If
                Return isConnected
                ' D0329 ==
            Catch ex As System.Exception
                sError = "Error: """ + ex.Message + """"
                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Close current connection.
        ''' </summary>
        ''' <remarks>Connection must be exists.</remarks>
        Public Sub Close()
            If isConnected Then
                DebugInfo("Close DB connection")
                If Connection.State = System.Data.ConnectionState.Open Then
                    Try
                        Connection.Close() ' D0464
                        If _DB_CLEAR_POOL AndAlso TypeOf (Connection) Is SqlClient.SqlConnection Then System.Data.SqlClient.SqlConnection.ClearPool(CType(Connection, SqlClient.SqlConnection)) ' D2235
                    Catch ex As Exception
                    End Try
                End If
            End If
        End Sub

        ''' <summary>
        ''' Create SQL Command for specified SQL-command(s).
        ''' </summary>
        ''' <param name="sSQL">String with SQL-command(s).</param>
        ''' <returns>Command object with type of current connection.</returns>
        ''' <remarks>Connection must be exists and shouldn't be connected. When calling, connection will try to open if not connected.</remarks>
        Public Function SQL(ByVal sSQL As String) As DbCommand
            Dim oCommand As DbCommand = Nothing
            If Connect() Then   ' D0464
                ' D0329 ===
                oCommand = GetDBCommand(ProviderType)
                oCommand.Connection = Connection
                oCommand.CommandText = sSQL ' D0331
                ' D0329 ==
                'DebugInfo(String.Format("Create SQL '{0}'", ShortString(sSQL, 200, True))) ' -D0830
            End If
            Return oCommand
        End Function

        ''' <summary>
        ''' Create new database object with database connection.
        ''' </summary>
        ''' <remarks>ConnectionString is empty, connection is closed, LastError is also empty.</remarks>
        Public Sub New(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType)
            sError = ""
            _ProviderType = ProviderType
            'TODO: MF, Usage place
            _Connection = GetDBConnection(_ProviderType, ConnectionString)
        End Sub


#Region "Common DB-operations"
        ' D0010 ===
        'http://www.megalib.com/books/923/77739.htm
        ''' <summary>
        ''' Get last inserted ID (for tables with auto incremented field)
        ''' </summary>
        ''' <param name="DB">Active dbConnection object, should be available and connected</param>
        ''' <returns>Integer value for last inserted ID. Returns -1 when not found</returns>
        ''' <remarks>Supported only for MSSQL 7+</remarks>
        Public Shared Function GetLastIdentity(ByVal DB As clsDatabaseAdvanced) As Integer ' D0045 + D0329 + D0464
            Dim id As Integer = -1
            If DB.Connect Then
                Dim sSQL As String = "SELECT @@IDENTITY"    ' D0045
                Dim oCommand As DbCommand = DB.SQL(sSQL)    ' D0329
                Dim Res As Object = oCommand.ExecuteScalar()    ' D0045
                If Not TypeOf (Res) Is DBNull Then id = CInt(Res) ' D0045
                DebugInfo(String.Format("Last SQL Identity: {0}", id))  ' D0045
            End If
            Return id
        End Function
        ' D0010 ==

        ''' <summary>
        ''' Check connection for specified ODBC connection string.
        ''' </summary>
        ''' <param name="sConnString">Connection string</param>
        ''' <returns>True, when connection is successful.</returns>
        ''' <remarks></remarks>
        Public Shared Function CheckDBConnection(ByVal sConnString As String, ByVal ProviderType As DBProviderType) As Boolean    ' D0329
            Dim fAvailable As Boolean = False
            Using fTempDB As DbConnection = GetDBConnection(ProviderType, sConnString)  ' D0329 + D2227
                Try
                    fTempDB.Open()
                    fAvailable = fTempDB.State = System.Data.ConnectionState.Open ' D0329 + M0332 
                Catch ex As Exception
                    DebugInfo("Can't connect to " + sConnString, _TRACE_RTE)   ' D0330
                    fAvailable = False
                Finally
                    fTempDB.Close()
                End Try
            End Using
            Return fAvailable
        End Function

        Public Shared Function GetConnectionString(ByVal DatabaseName As String, Optional ByVal Provider As DBProviderType = DBProviderType.dbptUnspecified) As String  ' D0457
            Return getConnectionDefinition(DatabaseName, Provider).ConnectionString
        End Function

        Public Shared Function DatabaseNameFromConnectionString(ByVal sConnectionString As String, ByVal Provider As DBProviderType) As String  ' D0466
            Return New clsConnectionDefinition(sConnectionString, Provider).DBName
        End Function

        ' D0464 ===
        Public Shared Function ReadRowValues(ByVal RS As DbDataReader) As Dictionary(Of String, Object)
            Dim tRow As New Dictionary(Of String, Object)   ' D0463
            For i As Integer = 0 To RS.FieldCount - 1
                ' D0483 ===
                Dim sKey As String = RS.GetName(i)
                If tRow.ContainsKey(sKey) Then sKey += "_"
                If tRow.ContainsKey(sKey) Then sKey += "_"
                tRow.Add(sKey, RS.GetValue(i)) ' D0463
                ' D0483 ==
            Next
            Return tRow
        End Function

        Public Shared Function ReadRows(ByVal RS As DbDataReader) As List(Of Dictionary(Of String, Object))
            Dim tData As New List(Of Dictionary(Of String, Object))
            While (Not RS Is Nothing And RS.Read)
                tData.Add(ReadRowValues(RS))
            End While
            Return tData
        End Function
        ' D0464 ==

        Public Function SelectFromTable(ByVal sTableName As String, Optional ByVal sParamName As String = Nothing, Optional ByVal sParamValue As Object = Nothing, Optional ByVal sReturnList As String = "*", Optional ByVal sOrderBy As String = "") As List(Of Dictionary(Of String, Object))   ' D0463 + D0464 + D0491 + D0497
            Dim tData As New List(Of Dictionary(Of String, Object)) ' D0463
            If Connect() AndAlso sTableName <> "" Then
                ' D0464 ===
                Dim sSQL As String = String.Format("SELECT {1} FROM {0}", sTableName, sReturnList)
                Dim tParams As List(Of Object) = Nothing
                If Not String.IsNullOrEmpty(sParamName) Then
                    sSQL += String.Format(" WHERE {0}=?", sParamName)
                    tParams = New List(Of Object)
                    tParams.Add(sParamValue)
                End If
                If sOrderBy <> "" Then sSQL += " ORDER BY " + sOrderBy ' D0497
                DebugInfo(String.Format("Get data From table '{0}'", sTableName))
                tData = SelectBySQL(sSQL, tParams)
                ' D0464 ==
            End If
            Return tData
        End Function

        ' D0473 ===
        Private Sub AddDBParameters(ByRef oCommand As DbCommand, ByVal ParamsList As List(Of Object))
            If Not ParamsList Is Nothing Then
                For i As Integer = 0 To ParamsList.Count - 1
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Param" + i.ToString, ParamsList(i)))
                Next
            End If
        End Sub
        ' D0473 ==

        ' D0464 ===
        Public Function SelectBySQL(ByVal sSQL As String, Optional ByVal ParamsList As List(Of Object) = Nothing) As List(Of Dictionary(Of String, Object))
            Dim tData As New List(Of Dictionary(Of String, Object))
            If Connect() AndAlso sSQL <> "" Then
                Dim oCommand As DbCommand = SQL(sSQL)
                AddDBParameters(oCommand, ParamsList)   ' D0473
                DebugInfo(String.Format("Get data by SQL '{0}'", sSQL))
                Dim RS As DbDataReader = DBExecuteReader(ProviderType, oCommand)
                DebugInfo("Read data from DataReader...")
                tData = ReadRows(RS)
                DebugInfo(String.Format("Loaded rows: {0}", tData.Count))
                RS.Close()
                RS = Nothing
                oCommand = Nothing
            End If
            Return tData
        End Function

        Public Function ExecuteSQL(ByVal sSQL As String, Optional ByVal ParamsList As List(Of Object) = Nothing) As Integer
            Dim tFetched As Integer = -1
            If Connect() AndAlso sSQL <> "" Then
                Dim oCommand As DbCommand = SQL(sSQL)
                AddDBParameters(oCommand, ParamsList)   ' D0473
                tFetched = DBExecuteNonQuery(ProviderType, oCommand)
                DebugInfo(String.Format("Executed non-query SQL '{0}'. Fetched: {1}", sSQL, tFetched))
                oCommand = Nothing
            End If
            Return tFetched
        End Function
        ' D0464 ==

        ' D0465 ===
        Public Function ExecuteScalarSQL(ByVal sSQL As String, Optional ByVal ParamsList As List(Of Object) = Nothing) As Object
            Dim tResult As Object = Nothing
            If Connect() AndAlso sSQL <> "" Then
                Dim oCommand As DbCommand = SQL(sSQL)
                AddDBParameters(oCommand, ParamsList)   ' D0473
                tResult = DBExecuteScalar(ProviderType, oCommand)
                DebugInfo(String.Format("Executed scalar SQL '{0}'", sSQL))
                oCommand = Nothing
            End If
            Return tResult
        End Function
        ' D0465 ==

        ' moved in 0456
        Public Shared Function ExecuteSQLScript(ByVal SQL As String, ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, Optional ByRef sRes As String = Nothing, Optional ByVal fUseTransaction As Boolean = True) As Boolean    'D0035 + D0329 + D0337
            Dim fResult As Boolean = False
            Using DB As New clsDatabaseAdvanced(ConnectionString, ProviderType)   ' D0329 + D2235
                Dim transaction As DbTransaction = Nothing    ' D0041
                Try
                    If DB.Connect Then
                        DebugInfo("Execute SQL script...")

                        ' D0045 ===
                        Dim sSeparator() As String = {"GO" + vbCrLf + vbCrLf}   ' D0420
                        Dim SQLList() As String = SQL.Split(sSeparator, StringSplitOptions.None)

                        Dim oCommand As DbCommand = GetDBCommand(ProviderType)  ' D0329
                        oCommand.Connection = DB.Connection
                        oCommand.CommandTimeout = 300
                        ' D0337 ===
                        If fUseTransaction Then
                            transaction = DB.Connection.BeginTransaction()
                            oCommand.Transaction = transaction
                        End If
                        ' D0337 ==

                        For Each sSQLPart As String In SQLList
                            ' D0341 ===
                            While sSQLPart > "" AndAlso sSQLPart.StartsWith(vbCrLf)
                                sSQLPart = sSQLPart.Remove(0, vbCrLf.Length)
                            End While
                            While sSQLPart > "" AndAlso sSQLPart.EndsWith(vbCrLf)
                                sSQLPart = sSQLPart.Remove(sSQLPart.Length - vbCrLf.Length)
                            End While
                            sSQLPart = sSQLPart.Trim
                            If sSQLPart <> "" Then
                                oCommand.CommandText = sSQLPart
                                oCommand.ExecuteNonQuery()    ' D0035
                                DebugInfo(ShortString(String.Format("SQL: {0}", sSQLPart), 60), _TRACE_INFO)  ' D0341
                            End If
                            ' D0341==
                        Next

                        If fUseTransaction Then transaction.Commit() ' D0337
                        ' D0045 ==
                        oCommand = Nothing
                        fResult = True
                    Else
                        If Not sRes Is Nothing Then sRes = DB.LastError ' D0366
                    End If
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)
                    If Not sRes Is Nothing Then sRes = ex.Message ' D0035
                    Try ' D0193 
                        If fUseTransaction And Not transaction Is Nothing Then transaction.Rollback() ' D0041 + D0337
                    Catch
                        If Not sRes Is Nothing Then sRes += vbCrLf + ex.Message ' D0193
                    End Try
                    DebugInfo("RTE occurred for SQL script: " + ex.Message, _TRACE_RTE)  ' D0341
                    fResult = False
                Finally
                    DB.Close()
                End Try
                transaction = Nothing   ' D0041

            End Using
            Return fResult
        End Function

        Public Shared Function CreateDatabase(ByVal sDatabaseName As String, ByVal fProviderType As DBProviderType, Optional ByRef sError As String = Nothing) As Boolean ' D0046 + D0341 + D0465 + D4138
            ' D0339 ===
            Dim sMainConnString As String = clsDatabaseAdvanced.GetConnectionString("master")   ' D0315  + D0329 + D0330 + D0341 + D0458
            DebugInfo(String.Format("Create Database '{0}'", sDatabaseName))
            ' D0341 ===
            Dim CreateSQL As String = String.Format("CREATE DATABASE {0}", sDatabaseName)
            If ExecuteSQLScript(CreateSQL, sMainConnString, fProviderType, sError, False) Then
                Threading.Thread.Sleep(_DB_CREATE_TIMEOUT)  ' D0490
                Using DB As New clsDatabaseAdvanced(clsDatabaseAdvanced.GetConnectionString(sDatabaseName), fProviderType)    ' D0458 + D2235
                    Dim cnt As Integer = 0
                    While cnt < _DB_CREATE_TRIES And Not DB.Connect   ' D0366 + D0490
                        System.Threading.Thread.Sleep(1000)
                        cnt += 1
                        DebugInfo(String.Format("Delayed for {0} sec before next DB connection", cnt), _TRACE_WARNING)  ' D0341
                    End While
                    DB.Close()
                End Using
                Return True ' D0345
            End If
            Return False
        End Function

        Public Shared Function DeleteDatabase(ByVal ConnString As String, ByVal ProviderType As DBProviderType) As Boolean    ' D0144
            Dim fresult As Boolean = False  ' D0144
            If ConnString = "" Then Exit Function
            Dim sDBName As String = New clsConnectionDefinition(ConnString, ProviderType).DBName    ' D0330 + MF0332 + D0342 + D0458
            Using DB As New clsDatabaseAdvanced(ConnString, ProviderType)   ' D0329 + D0330 + D0465 + D2235
                Try
                    DebugInfo(String.Format("Drop Database '{0}'", sDBName))
                    DB.Connect()
                    ' D0315 ===
                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)  ' D0329
                    oCommand.Connection = DB.Connection
                    oCommand.CommandText = String.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", sDBName)
                    DBExecuteNonQuery(DB.ProviderType, oCommand)
                    ' D0315 ==
                    oCommand.CommandText = String.Format("DROP DATABASE {0}", sDBName)
                    DBExecuteNonQuery(DB.ProviderType, oCommand)
                    oCommand = Nothing
                    fresult = True  ' D0144
                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_WARNING)
                Finally
                    DB.Close()
                End Try
            End Using
            Return fresult  ' D0144
        End Function

        ' D0130 ===
        Public Shared Function CopyJetToDatabase(ByVal sFilename As String, ByVal DestConnStringODBC As String, ByRef sErrorMessage As String) As Boolean ' D0329
            Dim fResult As Boolean = False

            Dim tFileProvider As DBProviderType = DBProviderType.dbptOLEDB  ' D0329
            Dim sFileConnectionString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, tFileProvider).ConnectionString     ' D0329 + D0330 + MF0332 + D0342 + D0348
            Using Connection As DbConnection = GetDBConnection(tFileProvider, sFileConnectionString)  ' D0329 + D2227

                Dim oCommand As DbCommand = GetDBCommand(tFileProvider)
                oCommand.Connection = Connection

                Try
                    ' Open uploaded projects as file
                    Connection.Open()
                    oCommand.Transaction = Connection.BeginTransaction()    ' D0330

                    ' open schema to read table names
                    Dim schemaTable As System.Data.DataTable
                    schemaTable = GetDBSchemaTables(tFileProvider, sFileConnectionString)   ' D0329
                    'schemaTable = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})

                    Dim SQL As String
                    Dim i As Integer
                    ' iterate through all tables and make import
                    For i = 0 To schemaTable.Rows.Count - 1
                        Dim sTable As String = schemaTable.Rows(i)!TABLE_NAME.ToString  ' D0330
                        If schemaTable.Rows(i)!TABLE_TYPE.ToString.ToLower = "table" And Not sTable.ToLower.StartsWith("sys") Then   ' D0065 + D0330
                            SQL = String.Format("SELECT * INTO [ODBC;{0}].{1} FROM {1}", DestConnStringODBC, sTable)    ' D0330
                            oCommand.CommandText = SQL
                            DBExecuteNonQuery(tFileProvider, oCommand)
                        End If
                    Next

                    oCommand.Transaction.Commit()   ' D0330
                    oCommand = Nothing
                    Connection.Close()

                    fResult = True
                    sErrorMessage = ""

                Catch ex As Exception
                    ' Rollback if error occurred.
                    If Not oCommand Is Nothing Then ' D0330
                        If Not oCommand.Transaction Is Nothing Then oCommand.Transaction.Commit() ' D0330
                    End If
                    If Connection.State <> System.Data.ConnectionState.Closed Then Connection.Close() 'MF0332
                    DeleteDatabase(DestConnStringODBC, DBProviderType.dbptODBC)  ' D0329
                    If Not sErrorMessage Is Nothing Then sErrorMessage = ex.Message
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
            End Using

            Return fResult
        End Function

        Public Shared Function CopyDatabaseToJet(ByVal SrcConnectionStringODBC As String, ByVal sFilename As String, ByRef sErrorMessage As String) As Boolean  ' D0329
            Dim fResult As Boolean = False

            Dim transaction As DbTransaction = Nothing   ' D0062
            ' D0329 ===
            Dim tFileProvider As DBProviderType = DBProviderType.dbptOLEDB
            Dim sFileConnectionString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, tFileProvider).ConnectionString   ' D0330 + MF0332 + D0342 + D0348
            Using Connection As DbConnection = GetDBConnection(tFileProvider, sFileConnectionString)    ' D2227
                ' D0329 ==

                Try
                    Connection.Open()

                    ' open schema to read table names
                    Using dbConnection As DbConnection = GetDBConnection(DBProviderType.dbptODBC, SrcConnectionStringODBC)  ' D0108 + D0315 + D0329 + D2227
                        dbConnection.Open()

                        Dim schemaTable As System.Data.DataTable = GetDBSchemaTables(DBProviderType.dbptODBC, SrcConnectionStringODBC)  ' D0329
                        'schemaTable = dbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})

                        Dim oCommand As DbCommand = GetDBCommand(tFileProvider) ' D0329
                        oCommand.Connection = Connection

                        transaction = Connection.BeginTransaction()
                        oCommand.Transaction = transaction

                        ' iterate through all tables and make import
                        For i As Integer = 0 To schemaTable.Rows.Count - 1
                            Dim sTable As String = schemaTable.Rows(i)!TABLE_NAME.ToString  ' D0330
                            If schemaTable.Rows(i)!TABLE_TYPE.ToString.ToLower = "table" And Not sTable.ToLower.StartsWith("sys") Then   ' D0065 + D0330
                                oCommand.CommandText = String.Format("SELECT * INTO {0} FROM [ODBC;{1}].{0}", sTable, SrcConnectionStringODBC) ' D0329 + D0330
                                DBExecuteNonQuery(tFileProvider, oCommand)
                            End If
                        Next

                        transaction.Commit()
                        oCommand = Nothing
                        Connection.Close()
                        dbConnection.Close()
                        fResult = True
                        sErrorMessage = ""
                    End Using

                Catch ex As Exception
                    ' Rollback if error occurred.
                    If Not transaction Is Nothing Then transaction.Commit()
                    If Connection.State <> System.Data.ConnectionState.Closed Then Connection.Close() 'MF0332
                    If Not sErrorMessage Is Nothing Then sErrorMessage = ex.Message
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                End Try
            End Using

            Return fResult
        End Function
        ' D0130 ==

        ' D0166 + D0479 ===
        Shared Function CopyCustomTablesFromFileToDatabase(ByVal sDatabaseName As String, ByVal sFileName As String, ByVal sTablesList() As String, ByVal sTablesPrefix As String) As Boolean
            Dim fResult As Boolean = False
            Dim sOleDB_FileConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFileName, DBProviderType.dbptOLEDB).ConnectionString   ' D0315 + D0330 + MF0332 + D0458
            Dim sODBC_DBConnString As String = clsDatabaseAdvanced.GetConnectionString(sDatabaseName, DBProviderType.dbptODBC)   ' D0315 + D0330 + D0458

            Using Connection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, sOleDB_FileConnString)  ' D0329 + D2227

                Try
                    Dim oCommand As DbCommand = GetDBCommand(DBProviderType.dbptOLEDB)  ' D0329
                    oCommand.Connection = Connection
                    Connection.Open()

                    For Each sTableName As String In sTablesList
                        Try
                            oCommand.CommandText = String.Format("SELECT * INTO [ODBC;{0}].{2}{1} FROM {1}", sODBC_DBConnString, sTableName, sTablesPrefix)
                            DBExecuteNonQuery(DBProviderType.dbptOLEDB, oCommand)   ' D0331
                        Catch ex As Exception
                        End Try
                    Next

                    Connection.Close()
                    oCommand = Nothing
                    fResult = True

                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Finally
                    If Connection.State <> System.Data.ConnectionState.Closed Then Connection.Close() 'MF0332
                End Try
            End Using

            Return fResult
        End Function

        Shared Function CopyCustomTablesFromDatabaseToFile(ByVal sDatabase As String, ByVal sFileName As String, ByVal sTablesList() As String, ByVal sTablesPrefix As String) As Boolean
            Dim fResult As Boolean = False

            Dim sOleDB_FileConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFileName, DBProviderType.dbptOLEDB).ConnectionString  ' D0315 + D0329 + D0330 + MF0332 + D0458
            Dim sODBC_DBConnString As String = clsDatabaseAdvanced.GetConnectionString(sDatabase, DBProviderType.dbptODBC)   ' D0315 + D0329 + D0330 + D0458

            Using Connection As DbConnection = GetDBConnection(DBProviderType.dbptOLEDB, sOleDB_FileConnString)   ' D0329 + D2227

                Try
                    Connection.Open()
                    Dim oCommand As DbCommand = GetDBCommand(DBProviderType.dbptOLEDB)  ' D0329
                    oCommand.Connection = Connection

                    For Each sTableName As String In sTablesList
                        If TableExists(sODBC_DBConnString, DBProviderType.dbptODBC, String.Format("{0}{1}", sTablesPrefix, sTableName)) Then 'C0247
                            Dim affected As Integer
                            Try
                                Try
                                    oCommand.CommandText = String.Format("DROP TABLE {0}", sTableName)
                                    DBExecuteNonQuery(DBProviderType.dbptOLEDB, oCommand)   ' D0331
                                Catch ex As Exception
                                End Try
                                oCommand.CommandText = String.Format("SELECT * INTO {0} FROM {2}{0} IN '' [ODBC;{1}]", sTableName, sODBC_DBConnString, sTablesPrefix)
                                affected = DBExecuteNonQuery(DBProviderType.dbptOLEDB, oCommand)    ' D0331
                            Catch ex As Exception
                                DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                            End Try
                        End If
                    Next

                    oCommand = Nothing
                    Connection.Close()
                    fResult = True

                Catch ex As Exception
                    DebugInfo(ex.Message, _TRACE_RTE)   ' D0330
                Finally
                    If Connection.State <> System.Data.ConnectionState.Closed Then Connection.Close() 'MF0332
                End Try
            End Using

            Return fResult
        End Function
        ' D0166 + 0479 ==

#End Region

    End Class

End Namespace