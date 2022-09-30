Imports Microsoft.VisualBasic
Imports System.Configuration

Namespace ExpertChoice.Database

    Public Module ConnectionStringSection

        Private mGlobalDefaultProvider As String = ""

        'This value can be set from an external routine to change default provider
        Public Property GlobalDefaultProvider() As String
            Get
                Return mGlobalDefaultProvider
            End Get
            Set(ByVal value As String)
                mGlobalDefaultProvider = value
            End Set
        End Property

        'Returns the connection from the connection strings section 
        Public Function getConnectionStringSectionItem(Optional ByVal Provider As DBProviderType = Nothing) As clsConnectionDefinition ' D0457
            Dim ProviderString As String = FindProvider(Provider)
            Dim Connection As clsConnectionDefinition = clsConnectionDefinition.loadConnectionDefFromConfig(ProviderString)
            Return Connection
        End Function

        'returns the connection whose database name = DatabaseName and based upon the specified/default provider
        Public Function getConnectionDefinition(ByVal DatabaseName As String, Optional ByVal Provider As DBProviderType = DBProviderType.dbptUnspecified) As clsConnectionDefinition    ' D0330 + D0457
            Dim ProviderString As String = FindProvider(Provider)
            Dim Connection As clsConnectionDefinition = clsConnectionDefinition.loadConnectionDefFromConfig(ProviderString)
            Connection = clsConnectionDefinition.BuildConnectionDefinitionFromSource(Connection, DatabaseName)
            Return Connection
        End Function

        'M0332 Moved to 
        'Public Function getConnectionString(ByVal DatabaseName As String, Optional ByVal Provider As DBProvider = Nothing) As String
        '    Return getConnectionDefinition(DatabaseName, Provider).ConnectionString
        'End Function

        Private Function FindProvider(ByVal Provider As DBProviderType) As String ' D0457
            Dim ProviderString As String = ""
            If Provider = DBProviderType.dbptUnspecified Then ' D0457 
                Dim Connection As clsConnectionDefinition = clsConnectionDefinition.loadConnectionDefFromConfig(GlobalDefaultProvider)
                If Not Connection Is Nothing Then
                    ProviderString = Connection.ProviderType.ToString ' D0330
                End If
            Else
                ProviderString = Provider.ToString
            End If
            ' D0459 ===
            Select Case ProviderString
                Case DBProviderType.dbptODBC.ToString
                    ProviderString = "ODBC"
                Case DBProviderType.dbptOLEDB.ToString
                    ProviderString = "OleDB"
                Case DBProviderType.dbptSQLClient.ToString
                    ProviderString = "SQLServer"
                Case DBProviderType.dbptOracle.ToString
                    ProviderString = "Oracle"
                Case Else
                    ProviderString = "undefined"
            End Select
            ' D0459 ==

            Return ProviderString
        End Function

    End Module

End Namespace
