Imports GenericDBAccess.ECGenericDatabaseAccess
Imports ExpertChoice.Database

Namespace ExpertChoice.Service

    ''' <summary>
    ''' Module with main common functions for database access: create and parse connection strings, check connections, etc.
    ''' </summary>
    ''' <remarks></remarks>
    Public Module JetDatabasesService

#Region "Compact and modify Jet databases"

        ' D0412 ===
        Public Function CompactJetDatabase(ByVal sFilename As String, Optional ByRef sError As String = Nothing) As Boolean
            Dim fResult As Boolean = False

            Dim sTmpFile As String = File_CreateTempName()
            Dim sSrcConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, DBProviderType.dbptOLEDB).ConnectionString ' D0457
            Dim sTmpConnString As String = clsConnectionDefinition.BuildJetConnectionDefinition(sTmpFile, DBProviderType.dbptOLEDB).ConnectionString ' D0457

            Try
                File_Erase(sTmpFile)
                Dim jetReplicationObject As New JRO.JetEngine
                jetReplicationObject.CompactDatabase(sSrcConnString, sTmpConnString)

                DebugInfo(String.Format("CompactJet from {0} to {1} bytes", My.Computer.FileSystem.GetFileInfo(sFilename).Length, My.Computer.FileSystem.GetFileInfo(sTmpFile).Length), _TRACE_INFO)

                If My.Computer.FileSystem.FileExists(sTmpFile) Then
                    My.Computer.FileSystem.MoveFile(sTmpFile, sFilename, True)
                End If

                fResult = True
            Catch ex As Exception
                If sError IsNot Nothing Then sError = ex.Message
            End Try
            File_Erase(sTmpFile)

            Return fResult
        End Function
        ' D0412 ==

        Public Function ChangeColumnNameInMDBFile(ByVal FilePath As String, ByVal TableName As String, ByVal OldColumnName As String, ByVal NewColumnName As String) As Boolean 'C0427

            'Dim OleDBConnectionString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FilePath + ";Jet OLEDB:Engine Type=5;"
            Dim OleDBConnectionString As String = clsConnectionDefinition.BuildJetConnectionDefinition(FilePath, DBProviderType.dbptOLEDB).ConnectionString() ' D0457

            If Not TableExists(OleDBConnectionString, DBProviderType.dbptOLEDB, TableName) Then
                Return False
            End If

            Dim ADOXCatalog As ADOX.Catalog
            Dim ADOConnection As ADODB.Connection

            ADOXCatalog = New ADOX.Catalog
            ADOConnection = New ADODB.Connection

            Try
                ADOConnection.Open(OleDBConnectionString)

                ADOXCatalog.ActiveConnection = ADOConnection
                ADOXCatalog.Tables(TableName).Columns(OldColumnName).Name = NewColumnName

            Catch ex As Exception
                'Debug.Print(ex.Message)
            Finally
                If ADOXCatalog.ActiveConnection IsNot Nothing Then
                    CType(ADOXCatalog.ActiveConnection, ADODB.Connection).Close()
                End If
                ADOXCatalog.ActiveConnection = Nothing
            End Try

            Return True
        End Function

        Public Function FixRAcontraintsTableBeforeUpload(ByVal FilePath As String) As Boolean 'C0427
            ChangeColumnNameInMDBFile(FilePath, "RAconstraints", "value", "value1")
        End Function

        Public Function FixRAcontraintsTableAfterDownload(ByVal FilePath As String) As Boolean 'C0427
            ChangeColumnNameInMDBFile(FilePath, "RAconstraints", "value1", "value")
        End Function
#End Region

    End Module

End Namespace