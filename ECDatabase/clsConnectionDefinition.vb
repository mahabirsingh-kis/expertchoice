Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Configuration
Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.Data.Odbc
Imports System.Data.OracleClient
Imports System.IO

Namespace ExpertChoice.Database

    ' -D0457: use DBProvideType from GenericDBAccess
    'Public Enum DBProvider
    '    Undefined
    '    SqlServer
    '    OleDb
    '    Oracle
    '    ODBC
    'End Enum

    <Serializable()> Public Class clsConnectionDefinition

        Implements ICloneable

        Private mConnectionString As String
        Private mProviderType As DBProviderType ' D0457

        Public Sub New(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) ' D0457
            mConnectionString = ConnectionString
            mProviderType = ProviderType
        End Sub

        Public Property ConnectionString() As String
            Set(ByVal value As String)
                mConnectionString = value
            End Set
            Get
                Return mConnectionString
            End Get
        End Property

        Public Property ProviderType() As DBProviderType ' D0457
            Set(ByVal value As DBProviderType) ' D0457
                mProviderType = value
            End Set
            Get
                Return mProviderType
            End Get
        End Property

        Public ReadOnly Property DBName() As String
            Get
                Return FindValue(csbDBName)
            End Get
        End Property

        Public ReadOnly Property UserName() As String
            Get
                Return FindValue(csbUserName)
            End Get
        End Property

        Public ReadOnly Property Password() As String
            Get
                Return FindValue(csbPassword)
            End Get
        End Property

        Public ReadOnly Property Server() As String
            Get
                Return FindValue(csbServer)
            End Get
        End Property

        Public ReadOnly Property Trusted() As String
            Get
                Try
                    Return FindValue(csbTrusted)
                Catch ex As Exception
                    Return False.ToString
                End Try
            End Get
        End Property

        Public ReadOnly Property Mirror() As String
            Get
                Return FindValue(csbMirror)
            End Get
        End Property

        Friend Shared csbDBName As String() = {"Database", "Initial Catalog"}
        Friend Shared csbUserName As String() = {"Uid", "User Id"}
        Friend Shared csbPassword As String() = {"Pwd", "Password"}
        Friend Shared csbServer As String() = {"Data Source", "Server"}
        Friend Shared csbTrusted As String() = {"Integrated Security", "Trusted_Connection"}
        Friend Shared csbMirror As String() = {"Failover Partner"}

        Public Function FindValue(ByVal searchArray() As String) As String

            Dim ConnectionStringBuilder As DbConnectionStringBuilder = getConnectionStringBuilder(mConnectionString, mProviderType)

            Dim retVal As String = ""

            For Each s As String In searchArray
                ' D0330 ===
                If ConnectionStringBuilder.ContainsKey(s) Then
                    retVal = ConnectionStringBuilder(s).ToString.Trim
                    Exit For
                    'If retVal.Length > 0 Then
                    '    Exit For
                    'End If
                End If
                ' D0330 ==
            Next

            Return retVal
        End Function

        Public Shared Function getConnectionStringBuilder(ByVal ConnectionString As String, ByVal Provider As DBProviderType) As DbConnectionStringBuilder   ' D0457

            Dim retVal As DbConnectionStringBuilder
            Select Case Provider
                Case DBProviderType.dbptSQLClient ' D0457
                    Dim dbcs As New SqlClient.SqlConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case DBProviderType.dbptOLEDB ' D0457
                    Dim dbcs As New OleDb.OleDbConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case DBProviderType.dbptODBC ' D0457
                    Dim dbcs As New Odbc.OdbcConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case DBProviderType.dbptOracle ' D0457
                    Dim dbcs As New OracleClient.OracleConnectionStringBuilder
                    dbcs.ConnectionString = ConnectionString
                    retVal = dbcs
                Case Else
                    Throw New Exception("not implemented")
            End Select

            Return retVal

        End Function

        Public Shared Function loadConnectionDefFromConfig(ByVal Name As String) As clsConnectionDefinition
            Try
                Dim p As DBProviderType = Nothing ' D0457
                p = getDBProviderType(ConfigurationManager.ConnectionStrings(Name).ProviderName)
                Dim cs As String = ConfigurationManager.ConnectionStrings(Name).ConnectionString
                Return New clsConnectionDefinition(cs, p)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Shared Function getProviderString(ByVal Provider As DBProviderType) As String ' D0457
            Dim p As String = ""
            Select Case Provider
                Case DBProviderType.dbptSQLClient ' D0457
                    p = "system.data.sqlclient"
                Case DBProviderType.dbptOLEDB ' D0457
                    p = "system.data.oledb"
                Case DBProviderType.dbptODBC ' D0457
                    p = "system.data.odbc"
                Case DBProviderType.dbptOracle ' D0457
                    p = "system.data.oracleclient"
            End Select
            Return p
        End Function

        Public Shared Function providerStringToFactory(ByVal ProviderInvariantName As String) As DbProviderFactory

            Dim retVal As DbProviderFactory = Nothing

            Try
                retVal = DbProviderFactories.GetFactory(ProviderInvariantName)
            Catch ex As Exception
            End Try

            Return retVal

        End Function

        Public Shared Function getDBProviderType(ByVal ProviderString As String) As DBProviderType  ' D0457
            Dim p As DBProviderType = Nothing    ' D0457
            Select Case ProviderString.Trim.ToLower
                Case "system.data.sqlclient"
                    p = DBProviderType.dbptSQLClient ' D0457
                Case "system.data.oledb"
                    p = DBProviderType.dbptOLEDB ' D0457
                Case "system.data.odbc"
                    p = DBProviderType.dbptODBC ' D0457
                Case "system.data.oracleclient"
                    p = DBProviderType.dbptOracle  ' D0457
            End Select
            Return p
        End Function

        Public Shared Function BuildJetConnectionDefinition(ByVal Filename As String, ByVal Provider As DBProviderType) As clsConnectionDefinition ' D0457

            Dim sConnString As String = ""
            Select Case Provider
                Case DBProviderType.dbptOLEDB ' D0457
                    sConnString += "Provider=Microsoft.Jet.OLEDB.4.0;"
                    sConnString += "Data Source=" + Filename + ";"
                Case DBProviderType.dbptODBC  ' D0457
                    sConnString += "Driver={Microsoft Access Driver (*.mdb)};"
                    sConnString += "DBQ=" + Filename + ";" + "DriverId=25;FIL=MS Access;"
                Case Else
                    Throw New Exception("Only supported for ODBC and OLEDB")
            End Select

            Dim retVal As New clsConnectionDefinition(sConnString, Provider)
            Return retVal

        End Function

        Public Shared Function BuildConnectionDefinitionFromSource(ByVal clsConnectionDefinition As clsConnectionDefinition, ByVal DBName As String) As clsConnectionDefinition
            If clsConnectionDefinition Is Nothing Then Return Nothing ' D0331
            Dim retVal As clsConnectionDefinition = CType(clsConnectionDefinition.Clone, clsConnectionDefinition)
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

        Public Function getDbProviderFactory() As DbProviderFactory

            Dim objFactory As DbProviderFactory = Nothing

            Select Case mProviderType
                Case DBProviderType.dbptSQLClient  ' D0457
                    objFactory = SqlClientFactory.Instance
                Case DBProviderType.dbptOLEDB  ' D0457
                    objFactory = OleDbFactory.Instance
                Case DBProviderType.dbptOracle  ' D0457
                    objFactory = OracleClientFactory.Instance
                Case DBProviderType.dbptODBC  ' D0457
                    objFactory = OdbcFactory.Instance
            End Select

            Return objFactory

        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim retVal As New clsConnectionDefinition(mConnectionString, mProviderType)
            Return retVal
        End Function

    End Class

End Namespace
