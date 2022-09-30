'====================================================================
' Copyright (C) 2005 BinaryIntellect Consulting. All rights reserved.
' Visit us at www.binaryintellect.com
'====================================================================
'Helper code found from the following site.
'http://www.binaryintellect.net/articles/e0d28f9c-238e-43ae-a00b-59ddc33bfa87.aspx

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Data
Imports System.Configuration
Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.Data.Odbc
Imports System.Data.OracleClient
Imports System.IO

Namespace ExpertChoice.Database

    Public Class clsDatabaseHelper
        Implements IDisposable

        Public Sub Dispose() Implements System.IDisposable.Dispose
            Try
                objConnection.Close()
            Catch
            End Try
            Try
                objConnection.Dispose()
                objCommand.Dispose()
            Catch
            End Try
        End Sub

        Private objConnection As DbConnection
        Private objCommand As DbCommand
        Private objFactory As DbProviderFactory = Nothing
        Private boolHandleErrors As Boolean = True
        Private strLastError As String
        Private boolLogError As Boolean
        Private strLogFile As String

        Private mConnection As clsConnectionDefinition
        'Private mDefaultConnectionstringSettings As ConnectionStringSettings


        Public Shared Function getConnectionStringBuilder(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) As DbConnectionStringBuilder  ' D0333 + D0457

            Dim retVal As DbConnectionStringBuilder
            Select Case ProviderType
                Case DBProviderType.dbptSQLClient ' D0457
                    Dim dbcs As New SqlClient.SqlConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case DBProviderType.dbptOLEDB  ' D0457
                    Dim dbcs As New OleDb.OleDbConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case DBProviderType.dbptODBC  ' D0457
                    Dim dbcs As New Odbc.OdbcConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case DBProviderType.dbptOracle  ' D0457
                    Dim dbcs As New OracleClient.OracleConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case Else
                    Throw New Exception("Provider not supported")
            End Select

            Return retVal

        End Function

        Public Sub New(ByVal clsConnectionDefinition As clsConnectionDefinition)
            If clsConnectionDefinition Is Nothing Then
                Exit Sub
            End If

            'Try
            '    mDefaultConnectionstringSettings = ConfigurationManager.ConnectionStrings(1)
            'Catch
            '    mDefaultConnectionstringSettings = Nothing
            'End Try

            mConnection = CType(clsConnectionDefinition.Clone, clsConnectionDefinition) ' D0333

            Select Case clsConnectionDefinition.ProviderType
                Case DBProviderType.dbptSQLClient  ' D0457
                    objFactory = SqlClientFactory.Instance
                Case DBProviderType.dbptOLEDB  ' D0457
                    objFactory = OleDbFactory.Instance
                Case DBProviderType.dbptOracle  ' D0457
                    objFactory = OracleClientFactory.Instance
                Case DBProviderType.dbptODBC  ' D0457
                    objFactory = OdbcFactory.Instance
                Case Else
                    objFactory = Nothing
            End Select

            Try
                objConnection = objFactory.CreateConnection
                objCommand = objFactory.CreateCommand
                objConnection.ConnectionString = clsConnectionDefinition.ConnectionString
                objCommand.Connection = objConnection
            Catch ex As Exception
                HandleExceptions(ex)
            End Try
        End Sub

        Public Sub New()
            'MyClass.New(New clsConnectionDefinition(ConfigurationManager.ConnectionStrings(1).ConnectionString, DBProvider.ConfigDefined))
        End Sub

        Public Property HandleErrors() As Boolean
            Get
                Return boolHandleErrors
            End Get
            Set(ByVal value As Boolean)
                boolHandleErrors = value
            End Set
        End Property

        Public ReadOnly Property LastError() As String
            Get
                Return strLastError
            End Get
        End Property

        Public Property LogErrors() As Boolean
            Get
                Return boolLogError
            End Get
            Set(ByVal value As Boolean)
                boolLogError = value
            End Set
        End Property

        Public Property LogFile() As String
            Get
                Return strLogFile
            End Get
            Set(ByVal value As String)
                strLogFile = value
            End Set
        End Property

        Public Function AddParameter(ByVal name As String, ByVal value As Object) As Integer
            Dim p As DbParameter = objFactory.CreateParameter
            p.ParameterName = name
            p.Value = value
            Return objCommand.Parameters.Add(p)
        End Function

        Public Function AddParameter(ByVal parameter As DbParameter) As Integer
            Return objCommand.Parameters.Add(parameter)
        End Function

        Public ReadOnly Property Command() As DbCommand
            Get
                Return objCommand
            End Get
        End Property

        Public ReadOnly Property DbConnection() As DbConnection
            Get
                Return objConnection
            End Get
        End Property

        Public ReadOnly Property Factory() As DbProviderFactory
            Get
                Return objFactory
            End Get
        End Property

        Public ReadOnly Property isConnected() As Boolean
            Get
                Return objConnection.State = System.Data.ConnectionState.Open
            End Get
        End Property

        Public ReadOnly Property clsConnectionDefinition() As clsConnectionDefinition
            Get
                Return CType(mConnection.Clone, clsConnectionDefinition)    ' D0333
            End Get
        End Property

        Public Function Connect() As Boolean
            Try
                If objConnection.State <> System.Data.ConnectionState.Open Then
                    objConnection.Open()
                End If
                Return objConnection.State = System.Data.ConnectionState.Open
            Catch ex As Exception
                HandleExceptions(ex)
                Return False
            Finally
            End Try
        End Function

        Public Function CreateDataAdapter(ByVal query As String) As DbDataAdapter
            Dim da As DbDataAdapter = objFactory.CreateDataAdapter
            da.SelectCommand = objConnection.CreateCommand
            da.SelectCommand.CommandText = parseQuery(query)
            Return da
        End Function

        Public Shared Function TableExists(ByVal clsConnectionDefinition As clsConnectionDefinition, ByVal TableName As String) As Boolean

            Using db As New clsDatabaseHelper(clsConnectionDefinition)

                If Not db.Connect() Then
                    Return False
                End If

                ' open schema to read table names

                Dim schemaTable As System.Data.DataTable
                Select Case clsConnectionDefinition.ProviderType
                    Case DBProviderType.dbptSQLClient, DBProviderType.dbptOLEDB  ' D0457
                        schemaTable = db.DbConnection.GetSchema("Tables")
                    Case Else
                        Throw New Exception("not implemented")
                End Select

                For i As Integer = 0 To schemaTable.Rows.Count - 1
                    If schemaTable.Rows(i)!TABLE_NAME.ToString = TableName Then
                        db.Dispose()
                        Return True
                    End If
                Next

                db.Dispose()
            End Using
            Return False
        End Function

        Public Function ExecuteSQLScript(ByVal SQL As String) As Boolean

            Dim success As Boolean = False

            If Connect() Then
                Dim myRegex As Regex = New Regex("^GO", RegexOptions.IgnoreCase Or RegexOptions.Multiline)
                Dim lines As String() = myRegex.Split(SQL)

                Dim transaction As DbTransaction = DbConnection.BeginTransaction

                Using cmd As DbCommand = DbConnection.CreateCommand

                    cmd.Connection = DbConnection
                    cmd.Transaction = transaction

                    success = True
                    For Each line As String In lines
                        If line.Length > 0 Then
                            cmd.CommandText = line
                            cmd.CommandType = CommandType.Text
                            Try
                                cmd.ExecuteNonQuery()
                            Catch ex As Exception
                                transaction.Rollback()
                                success = False
                                HandleExceptions(ex)
                                Exit For
                            End Try
                        End If
                    Next
                End Using

                transaction.Commit()

            End If
            Return success

        End Function

        Public Sub BeginTransaction()
            If objConnection.State = System.Data.ConnectionState.Closed Then
                objConnection.Open()
            End If
            objCommand.Transaction = objConnection.BeginTransaction
        End Sub

        Public Sub CommitTransaction()
            Try
                objCommand.Transaction.Commit()
                objConnection.Close()
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Public Sub RollbackTransaction()
            objCommand.Transaction.Rollback()
            objConnection.Close()
        End Sub

        Public Function ExecuteNonQuery(ByVal query As String) As Integer
            Return ExecuteNonQuery(query, CommandType.Text, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteNonQuery(ByVal query As String, ByVal commandtype As CommandType) As Integer
            Return ExecuteNonQuery(query, commandtype, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteNonQuery(ByVal query As String, ByVal connectionstate As ConnectionState) As Integer
            Return ExecuteNonQuery(query, CommandType.Text, connectionstate)
        End Function

        Public Function ExecuteNonQuery(ByVal query As String, ByVal commandtype As CommandType, ByVal connectionstate As ConnectionState) As Integer
            objCommand.CommandText = parseQuery(query)
            objCommand.CommandType = commandtype
            Dim i As Integer = -1
            Try
                If objConnection.State = System.Data.ConnectionState.Closed Then
                    objConnection.Open()
                End If
                i = objCommand.ExecuteNonQuery
            Catch ex As Exception
                HandleExceptions(ex)
            Finally
                objCommand.Parameters.Clear()
                If connectionstate = connectionstate.CloseOnExit Then
                    objConnection.Close()
                End If
            End Try
            Return i
        End Function

        Private Function parseQuery(ByVal query As String) As String
            Dim retVal As String = query
            Select Case mConnection.ProviderType
                Case DBProviderType.dbptSQLClient  ' D0457
                    Dim qPos As Integer = -1
                    For Each p As DbParameter In objCommand.Parameters
                        qPos = retVal.IndexOf("?")
                        If qPos < 0 Then Exit For
                        retVal = retVal.Substring(0, qPos) & "@" & p.ParameterName & retVal.Substring(qPos + 1)
                    Next
                Case Else
            End Select
            Return retVal
        End Function

        Public Function ExecuteScalar(ByVal query As String) As Object
            Return ExecuteScalar(query, CommandType.Text, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteScalar(ByVal query As String, ByVal commandtype As CommandType) As Object
            Return ExecuteScalar(query, commandtype, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteScalar(ByVal query As String, ByVal connectionstate As ConnectionState) As Object
            Return ExecuteScalar(query, CommandType.Text, connectionstate)
        End Function

        Public Function ExecuteScalar(ByVal query As String, ByVal commandtype As CommandType, ByVal connectionstate As ConnectionState) As Object

            objCommand.CommandText = parseQuery(query)
            objCommand.CommandType = commandtype
            Dim o As Object = Nothing
            Try
                If objConnection.State = System.Data.ConnectionState.Closed Then
                    objConnection.Open()
                End If
                o = objCommand.ExecuteScalar
            Catch ex As Exception
                HandleExceptions(ex)
            Finally
                objCommand.Parameters.Clear()
                If connectionstate = connectionstate.CloseOnExit Then
                    objConnection.Close()
                End If
            End Try
            Return o
        End Function

        Public Function ExecuteReader(ByVal query As String) As DbDataReader
            Return ExecuteReader(query, CommandType.Text, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteReader(ByVal query As String, ByVal commandtype As CommandType) As DbDataReader
            Return ExecuteReader(query, commandtype, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteReader(ByVal query As String, ByVal connectionstate As ConnectionState) As DbDataReader
            Return ExecuteReader(query, CommandType.Text, connectionstate)
        End Function

        Public Function ExecuteReader(ByVal query As String, ByVal commandtype As CommandType, ByVal connectionstate As ConnectionState) As DbDataReader
            objCommand.CommandText = parseQuery(query)
            objCommand.CommandType = commandtype
            Dim reader As DbDataReader = Nothing
            Try
                If objConnection.State = System.Data.ConnectionState.Closed Then
                    objConnection.Open()
                End If
                If connectionstate = connectionstate.CloseOnExit Then
                    reader = objCommand.ExecuteReader(CommandBehavior.CloseConnection)
                Else
                    reader = objCommand.ExecuteReader
                End If
            Catch ex As Exception
                HandleExceptions(ex)
            Finally
                objCommand.Parameters.Clear()
            End Try
            Return reader
        End Function

        Public Function ExecuteDataSet(ByVal query As String) As DataSet
            Return ExecuteDataSet(query, CommandType.Text, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteDataSet(ByVal query As String, ByVal commandtype As CommandType) As DataSet
            Return ExecuteDataSet(query, commandtype, ConnectionState.CloseOnExit)
        End Function

        Public Function ExecuteDataSet(ByVal query As String, ByVal connectionstate As ConnectionState) As DataSet
            Return ExecuteDataSet(query, CommandType.Text, connectionstate)
        End Function

        Public Function ExecuteDataSet(ByVal query As String, ByVal commandtype As CommandType, ByVal connectionstate As ConnectionState) As DataSet
            Dim adapter As DbDataAdapter = objFactory.CreateDataAdapter
            objCommand.CommandText = parseQuery(query)
            objCommand.CommandType = commandtype
            adapter.SelectCommand = objCommand
            Dim ds As DataSet = New DataSet
            Try
                adapter.Fill(ds)
            Catch ex As Exception
                HandleExceptions(ex)
            Finally
                objCommand.Parameters.Clear()
                If connectionstate = connectionstate.CloseOnExit Then
                    If objConnection.State = System.Data.ConnectionState.Open Then
                        objConnection.Close()
                    End If
                End If
            End Try
            Return ds
        End Function

        Public Function DeleteDatabase(ByVal DatabaseName As String) As Boolean

            Dim fresult As Boolean = False

            Try
                Connect()
                If ExecuteNonQuery(String.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", DatabaseName)) <> 0 Then
                    If ExecuteNonQuery(String.Format("DROP DATABASE {0}", DatabaseName)) <> 0 Then
                        fresult = True
                    End If
                End If
            Catch ex As Exception
                fresult = False
                HandleExceptions(ex)
            Finally
            End Try
            Return fresult

        End Function

        Public Function CreateDatabase(ByVal DatabaseName As String) As Boolean

            Dim success As Boolean = False
            Try
                If Connect() Then
                    Dim SQL As String = String.Format("CREATE DATABASE {0}", DatabaseName)
                    success = ExecuteNonQuery(SQL) <> 0

                    Dim CI As clsConnectionDefinition = mConnection
                    Using dbTest As New clsDatabaseHelper(BuildConnectionDefinitionFromSource(CI, DatabaseName))
                        If success Then 'keep trying to connect.... prevents error from other code immediately trying to connect
                            Dim connectTries As Integer = 0
                            Dim connect As Boolean = False
                            Do
                                Try
                                    connectTries += 1
                                    connect = dbTest.Connect()
                                    If connect Then dbTest.DbConnection.Close()
                                    'connect = dbTest.clsConnectionDefinition.State = System.Data.ConnectionState.Open
                                Catch ex As Exception
                                    'System.Diagnostics.Debug.WriteLine(String.Format("{0}  {1}", connectTries, ex.Message))
                                    System.Threading.Thread.Sleep(250)
                                End Try
                            Loop While Not connect
                        End If
                    End Using
                End If
            Catch ex As Exception
                success = False
                HandleExceptions(ex)
            End Try

            Return success

        End Function

        Private Sub HandleExceptions(ByVal ex As Exception)
            If LogErrors Then
                WriteToLog(ex.Message)
            End If
            If HandleErrors Then
                strLastError = ex.Message
            Else
                Throw ex
            End If
        End Sub

        Private Sub WriteToLog(ByVal msg As String)
            Dim writer As StreamWriter = File.AppendText(LogFile)
            writer.WriteLine(DateTime.Now.ToString + " - " + msg)
            writer.Close()
        End Sub

        <Obsolete("This routine is not reliable.  Please get the provider type from the config and modify that instead.")> _
        Public Shared Function ConvertConnectionDefinition(ByVal clsConnectionDefinition As clsConnectionDefinition, ByVal TargetProviderType As DBProviderType) As clsConnectionDefinition ' D0457

            Dim ServerName As String = ""
            Dim DatabaseName As String = ""
            Dim UID As String = ""
            Dim PWD As String = ""
            Dim Trusted As Boolean = False

            Select Case clsConnectionDefinition.ProviderType
                Case DBProviderType.dbptSQLClient  ' D0457
                    Dim csb As New SqlConnectionStringBuilder
                    csb.ConnectionString = clsConnectionDefinition.ConnectionString
                    ServerName = csb.DataSource
                    DatabaseName = csb.InitialCatalog
                    UID = csb.UserID
                    PWD = csb.Password
                    Trusted = csb.IntegratedSecurity
                Case Else
                    Throw New Exception("not implemented")
            End Select

            Dim cstring As String = ""
            Select Case TargetProviderType
                Case DBProviderType.dbptODBC  ' D0457 
                    cstring = "Driver={{SQL Server}};Server={0};Database={1};UID={2};PWD={3};Trusted_Connection={4}"
                    cstring = String.Format(cstring, ServerName, DatabaseName, UID, PWD, IIf(Trusted, "yes", "no"))
                Case DBProviderType.dbptOLEDB  ' D0457
                    cstring = "Provider=sqloledb;Data Source={0};Initial Catalog={1};User Id={2};Password={3};{4}"
                    cstring = String.Format(cstring, ServerName, DatabaseName, UID, PWD, IIf(Trusted, "Integrated Security=SSPI;", ""))
                Case Else
                    Throw New Exception("not implemented")
            End Select

            Return New clsConnectionDefinition(cstring, TargetProviderType)
        End Function

        Public Shared Function getJetConnectionFromDefinition(ByVal clsConnectionDefinition As clsConnectionDefinition) As clsConnectionDefinition

            Dim sConnString As String = ""
            Select Case clsConnectionDefinition.ProviderType
                Case DBProviderType.dbptOLEDB  ' D0457
                    sConnString += "Provider=Microsoft.Jet.OLEDB.4.0;"
                    sConnString += "Data Source=" + clsConnectionDefinition.ConnectionString + ";"
                Case DBProviderType.dbptODBC  ' D0457
                    sConnString += "Driver={Microsoft Access Driver (*.mdb)};"
                    sConnString += "DBQ=" + clsConnectionDefinition.ConnectionString + ";" + "DriverId=25;FIL=MS Access;"
            End Select

            Dim retVal As clsConnectionDefinition
            retVal = CType(clsConnectionDefinition.Clone, clsConnectionDefinition) ' D0333
            retVal.ConnectionString = sConnString
            Return retVal

        End Function

        Public Shared Function BuildConnectionDefinitionFromSource(ByVal clsConnectionDefinition As clsConnectionDefinition, ByVal DBName As String) As clsConnectionDefinition
            Dim retVal As clsConnectionDefinition = CType(clsConnectionDefinition.Clone, clsConnectionDefinition)   ' D0333
            Select Case clsConnectionDefinition.ProviderType
                Case DBProviderType.dbptODBC  ' D0457
                    Dim csb As New Odbc.OdbcConnectionStringBuilder
                    csb.ConnectionString = clsConnectionDefinition.ConnectionString
                    csb("database") = DBName
                    retVal.ConnectionString = csb.ConnectionString
                Case DBProviderType.dbptOLEDB  ' D0457
                    Dim csb As New OleDb.OleDbConnectionStringBuilder
                    csb.ConnectionString = clsConnectionDefinition.ConnectionString
                    csb("Initial Catalog") = DBName
                    retVal.ConnectionString = csb.ConnectionString
                Case DBProviderType.dbptSQLClient  ' D0457 
                    Dim csb As New SqlClient.SqlConnectionStringBuilder
                    csb.ConnectionString = clsConnectionDefinition.ConnectionString
                    csb.InitialCatalog = DBName
                    retVal.ConnectionString = csb.ConnectionString
                Case Else
                    Throw New Exception("not implemented")
            End Select

            Return retVal
        End Function

        Public Shared Function loadConnectionDefFromConfig(ByVal Name As String) As clsConnectionDefinition
            If ConfigurationManager.ConnectionStrings(Name) IsNot Nothing Then
                Dim p As DBProviderType = Nothing ' D0457
                p = getDBProviderType(ConfigurationManager.ConnectionStrings(Name).ProviderName)
                Dim cs As String = ConfigurationManager.ConnectionStrings(Name).ConnectionString
                Return New clsConnectionDefinition(cs, p)
            Else
                Return Nothing
            End If
        End Function

        Public Shared Function getDBProviderType(ByVal ProviderString As String) As DBProviderType  ' D0457
            Dim p As DBProviderType = Nothing ' D0457
            Select Case ProviderString.Trim.ToLower
                Case "system.data.sqlclient"
                    p = DBProviderType.dbptSQLClient  ' D0457
                Case "system.data.oledb"
                    p = DBProviderType.dbptOLEDB  ' D0457
                Case "system.data.odbc"
                    p = DBProviderType.dbptODBC  ' D0457
                Case "system.data.oracleclient"
                    p = DBProviderType.dbptOracle  ' D0457
            End Select
            Return p
        End Function

        Public Shared Function getProviderString(ByVal Provider As DBProviderType) As String ' D0457
            Dim p As String = ""
            Select Case Provider
                Case DBProviderType.dbptSQLClient  ' D0457
                    p = "system.data.sqlclient"
                Case DBProviderType.dbptOLEDB  ' D0457
                    p = "system.data.oledb"
                Case DBProviderType.dbptODBC  ' D0457
                    p = "system.data.odbc"
                Case DBProviderType.dbptOracle  ' D0457
                    p = "system.data.oracleclient"
            End Select
            Return p
        End Function

    End Class

    Public Enum ConnectionState
        KeepOpen
        CloseOnExit
    End Enum

End Namespace


