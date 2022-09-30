Imports ECCore
Imports System.Data.Common 'C0236
Imports System.IO 'C0271
Imports ECSecurity.ECSecurity 'C0380
Imports System.Linq

Namespace ECCore.MiscFuncs
    Public Module ECMiscFuncs

        Public Function DataExistsInProject_CanvasStreamDatabase(ByVal ConnectionString As String, ByVal ModelID As Integer, ByVal ProviderType As DBProviderType, Optional ByVal UserEMail As String = "") As Boolean
            Dim UserID As Integer = -1

            If UserEMail <> "" Then
                Dim UsersList As List(Of clsUser) = GetUsersList_CanvasStreamDatabase(ConnectionString, ProviderType, ModelID)

                Dim U As clsUser = GetUserByEmail(UsersList, UserEMail)
                If U IsNot Nothing Then
                    UserID = U.UserID
                Else
                    Return False
                End If
            End If

            Dim count As Integer = 0
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                If UserID <> -1 Then
                    oCommand.CommandText = "SELECT COUNT(*) FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                Else
                    oCommand.CommandText = "SELECT COUNT(*) FROM UserData WHERE ProjectID=? AND DataType=?"
                End If
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))

                If UserID <> -1 Then
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", UserID))
                End If

                Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                count = If(obj Is Nothing, 0, CType(obj, Integer))

            End Using

            Return count > 0
        End Function

        Public Function DataExistsInProject(ByVal StorageType As ECModelStorageType, ByVal ModelID As Integer, ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, Optional ByVal UserEMail As String = "") As Boolean 'C0274
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return DataExistsInProject_CanvasStreamDatabase(ConnectionString, ModelID, ProviderType, UserEMail)
            End Select

            Return False
        End Function

        Public Function GetCurrentSeparator() As Char
            Dim sngValue As Single = 0.5
            Return (sngValue.ToString())(1)
        End Function

        Public Function StringValueToSingle(ByVal SingleValueStr As String) As Single
            Dim dRes As Single
            If Single.TryParse(FixStringWithSingleValue(SingleValueStr), Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, dRes) Then
                Return dRes
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' WARNING: Unsafe function - depends on server Globalization settings, Not working for Russian Local settings
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        Public Function FixStringWithSingleValue(ByVal str As String) As String
            Dim separator As Char = GetCurrentSeparator()
            Dim sep2 As Char = CChar(If(separator = ".", ",", "."))
            Dim resStr As String = str.Trim.Replace("$", "")
            ' D7536 ===
            Dim idx_sep As Integer = str.IndexOf(separator)
            Dim idx_sep2 As Integer = str.IndexOf(sep2)
            If (idx_sep2 >= 0 AndAlso idx_sep2 < idx_sep) Then str = str.Replace(sep2, "")
            If (idx_sep < 0 AndAlso idx_sep2 >= 0) Then str = str.Replace(sep2, separator)
            ' D7536 ==
            If (resStr.IndexOf(separator) >= 0) Then
                If resStr(0) = separator Then resStr = "0" + resStr
                If resStr.Substring(0, 2) = "-" + separator Then resStr = "-0" + separator + resStr.Substring(2)
                Dim v As Double = Double.NaN
                If Double.TryParse(resStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.CreateSpecificCulture("en-US"), v) OrElse
                   Double.TryParse(resStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.CurrentCulture, v) OrElse
                   Double.TryParse(resStr, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, v) Then
                    If Not Double.IsNaN(v) Then resStr = v.ToString
                End If
            End If
            Return resStr
        End Function

        Public Function IsAHPSStream(ByVal InputStream As Stream, Optional ByVal PerformDecryption As Boolean = False, Optional ByVal Password As String = DEFAULT_ENCRYPTION_PASSWORD) As Boolean 'C0380
            If InputStream Is Nothing Then
                Return False
            End If

            If InputStream.Length = 0 Then
                Return False
            End If

            PerformDecryption = _DEBUG_ENCRYPTION_ENABLED 'C0380

            Dim oldPos As Integer = InputStream.Position
            InputStream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(InputStream)
            Dim chunk As Int32 = BR.ReadInt32

            Dim res As Boolean = (chunk = CHUNK_CANVAS_STREAMS_PROJECT) 'C0380

            'C0380===
            If Not res And PerformDecryption Then
                InputStream.Seek(0, SeekOrigin.Begin)
                'Dim nBytesLeft As Integer = BR.BaseStream.Length - BR.BaseStream.Position
                'Dim BlockOfBytes As Byte() = BR.ReadBytes(If(nBytesLeft >= 256, 256, nBytesLeft))
                'Dim decryptedBytes As Byte() = Decrypt(BlockOfBytes, Password)
                Dim AllBytes As Byte() = BR.ReadBytes(BR.BaseStream.Length)
                Dim decryptedBytes As Byte() = Decrypt(AllBytes, Password)
                Dim MS As New MemoryStream(decryptedBytes)
                Dim msBR As New BinaryReader(MS)
                MS.Seek(0, SeekOrigin.Begin)
                chunk = msBR.ReadInt32
                res = (chunk = CHUNK_CANVAS_STREAMS_PROJECT)
                msBR.Close()
            End If
            'C0380==

            InputStream.Seek(oldPos, SeekOrigin.Begin)

            'Return chunk = CHUNK_CANVAS_STREAMS_PROJECT 'C0380
            Return res 'C0380
        End Function

        Private Function GetUsersList_AHPDatabase(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) As List(Of clsUser)
            If Not CheckDBConnection(ProviderType, ConnectionString) Then
                Return Nothing
            End If

            Dim res As New List(Of clsUser)
            Dim AUser As clsUser 'C0048

            'Dim dbConnection As New odbc.odbcConnection(ConnectionString) 'C0236
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString) 'C0236
                dbConnection.Open() ' D0152
                'C0236===
                'Dim ocommand As odbc.odbcCommand
                'Dim dbreader As odbc.odbcDataReader
                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection
                Dim dbreader As DbDataReader
                'C0236==

                'ocommand = New odbc.odbcCommand("SELECT * FROM People", dbConnection) 'C0236
                oCommand.CommandText = "SELECT * FROM People" 'C0236
                dbreader = DBExecuteReader(ProviderType, oCommand)

                If Not dbreader Is Nothing Then
                    While dbreader.Read
                        If dbreader("PID") <> 1 Then ' if not combined
                            'C0048===
                            AUser = New clsUser
                            AUser.UserID = dbreader("PID")
                            AUser.Active = dbreader("Participating")
                            If Not (TypeOf (dbreader("PersonName")) Is DBNull) Then
                                AUser.UserName = dbreader("PersonName")
                            End If
                            If Not (TypeOf (dbreader("Email")) Is DBNull) Then
                                AUser.UserEMail = dbreader("Email")
                            End If
                            res.Add(AUser)
                            'C0048==
                        End If
                    End While
                End If

                oCommand = Nothing
                dbreader.Close()
                dbConnection.Close()
            End Using
            Return res
        End Function

        Public Function GetUsersList_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As List(Of clsUser)
            'Dim sReader As New clsStreamModelReader 'C0345

            'C0345===
            Dim dbVersion As ECCanvasDatabaseVersion = GetDBVersion(ECModelStorageType.mstCanvasStreamDatabase, Location, ProviderType, ModelID)
            Dim sReader As clsStreamModelReader = GetStreamModelReader(dbVersion)
            If sReader Is Nothing Then
                Return Nothing
            End If
            'C0345==

            Dim MS As New MemoryStream

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stModelStructure)))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                If dbReader.HasRows Then
                    dbReader.Read()

                    Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long                   ' The bytes returned from GetBytes.
                    Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    ' Continue reading and writing while there are bytes beyond the size of the buffer.
                    Do While retval = bufferSize
                        bw.Write(outbyte)
                        bw.Flush()

                        ' Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize
                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                    Loop

                    ' Write the remaining buffer.
                    bw.Write(outbyte, 0, retval)
                    bw.Flush()

                    'bw.Close()
                End If

                dbReader.Close()
            End Using

            sReader.BinaryStream = MS
            Return sReader.GetUsersList
        End Function

        Public Function GetStreamModelReader(ByVal CanvasDBVersion As ECCanvasDatabaseVersion) As clsStreamModelReader 'C0345
            Dim TReader As Type = Nothing

            Dim i As Integer = CanvasDBVersion.MinorVersion
            While (i >= 7) And TReader Is Nothing ' 7 is the first version we introduced current streams version control
                TReader = Type.GetType("clsStreamModelReader_v_1_1_" + i.ToString)
                If TReader Is Nothing Then i -= 1
            End While

            Return Activator.CreateInstance(TReader)
        End Function

        Public Function GetStreamModelWriter(ByVal CanvasDBVersion As ECCanvasDatabaseVersion) As clsStreamModelWriter 'C0345
            Dim TWriter As Type = Nothing

            Dim i As Integer = CanvasDBVersion.MinorVersion
            While (i >= 7) And TWriter Is Nothing ' 7 is the first version we introduced current streams version control
                TWriter = Type.GetType("clsStreamModelWriter_v_1_1_" + i.ToString)
                If TWriter Is Nothing Then i -= 1
            End While

            Return Activator.CreateInstance(TWriter)
        End Function

        Public Function GetUsersList(ByVal Location As String, ByVal StorageType As ECModelStorageType, ByVal ProviderType As DBProviderType, Optional ByVal ModelID As Integer = -1, Optional ByVal InputStream As Stream = Nothing) As List(Of clsUser)
            Select Case StorageType
                Case ECModelStorageType.mstAHPDatabase
                    Return GetUsersList_AHPDatabase(Location, ProviderType) 'C0236
                Case ECModelStorageType.mstCanvasStreamDatabase 'C0271
                    Return GetUsersList_CanvasStreamDatabase(Location, ProviderType, ModelID)
                Case ECModelStorageType.mstAHPSStream 'C0378
                    Return GetUsersList_AHPSStream(InputStream)
            End Select
            Return Nothing
        End Function

        'Private Function GetUsersList_AHPSStream(ByVal InputStream As Stream) As ArrayList 'C0378 'C0380
        Private Function GetUsersList_AHPSStream(ByVal InputStream As Stream, Optional ByVal PerformDecryption As Boolean = False, Optional ByVal Password As String = DEFAULT_ENCRYPTION_PASSWORD) As List(Of clsUser)
            If (InputStream Is Nothing) Then
                Return Nothing
            End If

            InputStream.Seek(0, SeekOrigin.Begin)
            Dim BR As New BinaryReader(InputStream) 'C0380

            'C0380===
            PerformDecryption = _DEBUG_ENCRYPTION_ENABLED

            If PerformDecryption Then
                Dim decryptedStream As MemoryStream
                Dim sBR As New BinaryReader(InputStream)
                Dim AllBytes As Byte() = sBR.ReadBytes(sBR.BaseStream.Length)
                Dim decryptedBytes As Byte() = Decrypt(AllBytes, Password)
                decryptedStream = New MemoryStream(decryptedBytes)
                InputStream.Seek(0, SeekOrigin.Begin)
                decryptedStream.Seek(0, SeekOrigin.Begin)
                BR = New BinaryReader(decryptedStream)
            End If
            'C0380==

            Dim HeadChunk As Int32 = BR.ReadInt32
            If HeadChunk <> CHUNK_CANVAS_STREAMS_PROJECT Then
                Return Nothing
            End If

            Dim chunk As Int32
            Dim chunkSize As Int32
            Dim UserID As Int32

            Dim dbVersion As ECCanvasDatabaseVersion = GetCurrentDBVersion()

            Dim StreamModel As MemoryStream = Nothing

            While (BR.BaseStream.Position < BR.BaseStream.Length - 1) And StreamModel Is Nothing
                chunk = BR.ReadInt32()
                Select Case chunk
                    Case CHUNK_CANVAS_STREAMS_PROJECT_BASE To CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE - 1
                        Dim byteArray As Byte()
                        chunkSize = BR.ReadInt32
                        byteArray = BR.ReadBytes(chunkSize)

                        If chunk = CHUNK_CANVAS_STREAMS_PROJECT_MODEL_STRUCTURE Then
                            StreamModel = New MemoryStream(byteArray)
                        End If

                        If chunk = CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION Then
                            Dim strVersion As String = ""
                            strVersion = ByteArrayToString(byteArray)

                            Dim version As New ECCanvasDatabaseVersion
                            If version.ParseString(strVersion) Then
                                dbVersion = version
                            End If
                        Else
                            BR.ReadInt64()
                        End If

                    Case CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE To 89999
                        Dim byteArray As Byte()
                        UserID = BR.ReadInt32()
                        chunkSize = BR.ReadInt32()
                        byteArray = BR.ReadBytes(chunkSize)
                        BR.ReadInt64()

                    Case CHUNK_CANVAS_STREAMS_SNAPSHOTS_STREAM
                        Dim byteArray As Byte()
                        chunkSize = BR.ReadInt32()
                        byteArray = BR.ReadBytes(chunkSize)
                End Select
            End While

            Dim sReader As clsStreamModelReader = GetStreamModelReader(dbVersion)
            If (sReader Is Nothing) Or (StreamModel Is Nothing) Then
                Return Nothing
            Else
                sReader.BinaryStream = StreamModel
                Return sReader.GetUsersList
            End If
        End Function

        Public Function GetProjectVersion_AHPSStream(ByVal InputStream As Stream, Optional ByVal PerformDecryption As Boolean = False, Optional ByVal Password As String = DEFAULT_ENCRYPTION_PASSWORD) As ECCanvasDatabaseVersion
            If (InputStream Is Nothing) Then
                Return Nothing
            End If

            InputStream.Seek(0, SeekOrigin.Begin)
            Dim BR As New BinaryReader(InputStream)

            PerformDecryption = _DEBUG_ENCRYPTION_ENABLED

            If PerformDecryption Then
                Dim decryptedStream As MemoryStream
                Dim sBR As New BinaryReader(InputStream)
                Dim AllBytes As Byte() = sBR.ReadBytes(sBR.BaseStream.Length)
                Dim decryptedBytes As Byte() = Decrypt(AllBytes, Password)
                decryptedStream = New MemoryStream(decryptedBytes)
                InputStream.Seek(0, SeekOrigin.Begin)
                decryptedStream.Seek(0, SeekOrigin.Begin)
                BR = New BinaryReader(decryptedStream)
            End If

            Dim HeadChunk As Int32 = BR.ReadInt32
            If HeadChunk <> CHUNK_CANVAS_STREAMS_PROJECT Then
                Return Nothing
            End If

            Dim chunk As Int32
            Dim chunkSize As Int32
            Dim UserID As Int32

            Dim dbVersion As ECCanvasDatabaseVersion = Nothing

            Dim StreamModel As MemoryStream = Nothing

            While (BR.BaseStream.Position < BR.BaseStream.Length - 1)
                chunk = BR.ReadInt32()
                Select Case chunk
                    Case CHUNK_CANVAS_STREAMS_PROJECT_BASE To CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE - 1
                        Dim byteArray As Byte()
                        chunkSize = BR.ReadInt32
                        byteArray = BR.ReadBytes(chunkSize)

                        If chunk = CHUNK_CANVAS_STREAMS_PROJECT_MODEL_STRUCTURE Then
                            StreamModel = New MemoryStream(byteArray)
                        End If

                        If chunk = CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION Then
                            Dim strVersion As String = ""
                            strVersion = ByteArrayToString(byteArray)

                            Dim version As New ECCanvasDatabaseVersion
                            If version.ParseString(strVersion) Then
                                dbVersion = version
                            End If

                            Return dbVersion
                        End If

                    Case CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE To 89999
                        Dim byteArray As Byte()
                        UserID = BR.ReadInt32()
                        chunkSize = BR.ReadInt32()
                        byteArray = BR.ReadBytes(chunkSize)

                    Case CHUNK_CANVAS_STREAMS_SNAPSHOTS_STREAM
                        Dim byteArray As Byte()
                        chunkSize = BR.ReadInt32()
                        byteArray = BR.ReadBytes(chunkSize)
                End Select
            End While

            Return Nothing
        End Function

        Public Function GetUserByEmail(ByVal UsersList As List(Of clsUser), ByVal email As String) As clsUser
            If UsersList Is Nothing Then Return Nothing
            Return UsersList.FirstOrDefault(Function(u) (u.UserEMail.ToLower = email.ToLower))
        End Function

        Public Function HasContribution(ByVal Alternative As clsNode, Hierarchy As clsHierarchy) As Boolean
            If Alternative Is Nothing Or Hierarchy Is Nothing Then Return Nothing
            Dim HasContributions As Boolean = False
            For Each node As clsNode In Hierarchy.TerminalNodes
                If node.GetNodesBelow(UNDEFINED_USER_ID).Contains(Alternative) Then
                    HasContributions = True
                    Exit For
                End If
            Next
            Return HasContributions
        End Function

        Public Function WriteSurveysToAHPFile(ByVal SourceConnectionString As String, ByVal SourceProviderType As DBProviderType, ByVal ModelID As Integer,
                                              ByVal AHPFileConnectionString As String, ByVal AHPProviderType As DBProviderType) As Boolean
            If Not CheckDBConnection(SourceProviderType, SourceConnectionString) Or Not CheckDBConnection(AHPProviderType, AHPFileConnectionString) Then
                Return False
            End If

            Dim dbConnection As DbConnection = GetDBConnection(SourceProviderType, SourceConnectionString)
            dbConnection.Open()
            Dim oCommand As DbCommand = GetDBCommand(SourceProviderType)
            oCommand.Connection = dbConnection

            Dim ahpdbConnection As DbConnection = GetDBConnection(AHPProviderType, AHPFileConnectionString)
            ahpdbConnection.Open()
            Dim ahpCommand As DbCommand = GetDBCommand(AHPProviderType)
            ahpCommand.Connection = ahpdbConnection

            oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND (StructureType=? OR StructureType=? OR StructureType=? OR StructureType=? OR StructureType=?)"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(SourceProviderType, "ProjectID", ModelID))
            oCommand.Parameters.Add(GetDBParameter(SourceProviderType, "Type1", StructureType.stSpyronStructureWelcome))
            oCommand.Parameters.Add(GetDBParameter(SourceProviderType, "Type2", StructureType.stSpyronStructureThankYou))
            oCommand.Parameters.Add(GetDBParameter(SourceProviderType, "Type3", StructureType.stSpyronModelVersion))
            oCommand.Parameters.Add(GetDBParameter(SourceProviderType, "Type4", StructureType.stSpyronStructureImpactWelcome))
            oCommand.Parameters.Add(GetDBParameter(SourceProviderType, "Type5", StructureType.stSpyronStructureImpactThankyou))

            Dim dbReader As DbDataReader
            dbReader = DBExecuteReader(SourceProviderType, oCommand)

            While dbReader.Read
                Dim MS As New MemoryStream

                Dim sType As StructureType
                sType = CType(dbReader("StructureType"), StructureType)

                Dim StreamSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                If StreamSize <= 0 Then StreamSize = 100
                Dim outbyte(StreamSize - 1) As Byte                         ' The BLOB byte() buffer to be filled by GetBytes.
                Dim retval As Long                                          ' The bytes returned from GetBytes.
                Dim startIndex As Long = 0                                  ' The starting position in the BLOB output.

                retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, StreamSize)

                Dim tmpBW As BinaryWriter = New BinaryWriter(MS)

                ' Continue reading and writing while there are bytes beyond the size of the buffer.
                Do While retval = StreamSize
                    tmpBW.Write(outbyte)
                    tmpBW.Flush()

                    ' Reposition the start index to the end of the last buffer and fill the buffer.
                    startIndex += StreamSize
                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, StreamSize)
                Loop

                tmpBW.Write(outbyte, 0, retval)
                tmpBW.Flush()

                ' write to ahp

                Dim affected As Integer
                If MS.ToArray.Length <> 0 Then
                    ahpCommand.CommandText = "INSERT INTO Surveys (SurveyType, StreamSize, Stream) VALUES (?, ?, ?)"
                    ahpCommand.Parameters.Clear()
                    ahpCommand.Parameters.Add(GetDBParameter(AHPProviderType, "SurveyType", sType))
                    ahpCommand.Parameters.Add(GetDBParameter(AHPProviderType, "StreamSize", MS.ToArray.Length))

                    If AHPProviderType = DBProviderType.dbptODBC Then
                        Dim streamParam As Odbc.OdbcParameter = ahpCommand.CreateParameter
                        streamParam.OdbcType = Odbc.OdbcType.Image
                        streamParam.ParameterName = "Stream"
                        streamParam.Size = MS.ToArray.Length
                        streamParam.Value = MS.ToArray
                        ahpCommand.Parameters.Add(streamParam)
                    Else
                        ahpCommand.Parameters.Add(GetDBParameter(AHPProviderType, "Stream", MS.ToArray))
                    End If

                    affected = DBExecuteNonQuery(AHPProviderType, ahpCommand)
                End If
            End While

            dbReader.Close()

            dbConnection.Close()
            ahpdbConnection.Close()
            Return True
        End Function

        Public Function LoadSurveysFromAHPFile(ByVal AHPFileConnectionString As String, ByVal AHPProviderType As DBProviderType,
                                               ByVal DestConnectionString As String, ByVal DestProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            If Not CheckDBConnection(DestProviderType, DestConnectionString) Or Not CheckDBConnection(AHPProviderType, AHPFileConnectionString) Then
                Return False
            End If

            If Not TableExists(AHPFileConnectionString, AHPProviderType, "Surveys") Then Return False

            Dim dbConnection As DbConnection = GetDBConnection(DestProviderType, DestConnectionString)
            dbConnection.Open()
            Dim oCommand As DbCommand = GetDBCommand(DestProviderType)
            oCommand.Connection = dbConnection

            Dim ahpdbConnection As DbConnection = GetDBConnection(AHPProviderType, AHPFileConnectionString)
            ahpdbConnection.Open()
            Dim ahpCommand As DbCommand = GetDBCommand(AHPProviderType)
            ahpCommand.Connection = ahpdbConnection

            ahpCommand.CommandText = "SELECT * FROM Surveys"

            Dim dbReader As DbDataReader
            dbReader = DBExecuteReader(AHPProviderType, ahpCommand)

            While dbReader.Read
                Dim MS As New MemoryStream

                Dim sType As StructureType
                sType = CType(dbReader("SurveyType"), StructureType)

                Dim StreamSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                If StreamSize <= 0 Then StreamSize = 100
                Dim outbyte(StreamSize - 1) As Byte                         ' The BLOB byte() buffer to be filled by GetBytes.
                Dim retval As Long                                          ' The bytes returned from GetBytes.
                Dim startIndex As Long = 0                                  ' The starting position in the BLOB output.

                retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, StreamSize)

                Dim tmpBW As BinaryWriter = New BinaryWriter(MS)

                ' Continue reading and writing while there are bytes beyond the size of the buffer.
                Do While retval = StreamSize
                    tmpBW.Write(outbyte)
                    tmpBW.Flush()

                    ' Reposition the start index to the end of the last buffer and fill the buffer.
                    startIndex += StreamSize
                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, StreamSize)
                Loop

                tmpBW.Write(outbyte, 0, retval)
                tmpBW.Flush()

                ' write to streams database

                Dim affected As Integer
                If MS.ToArray.Length <> 0 Then
                    oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(DestProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(DestProviderType, "StructureType", sType))
                    oCommand.Parameters.Add(GetDBParameter(DestProviderType, "StreamSize", MS.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(DestProviderType, "Stream", MS.ToArray))
                    affected = DBExecuteNonQuery(DestProviderType, oCommand)
                End If
            End While

            dbReader.Close()

            dbConnection.Close()
            ahpdbConnection.Close()
            Return True
        End Function

        Public Function GetMissingUsersList(ByVal ProjectManager As clsProjectManager) As List(Of clsUser)
            If ProjectManager Is Nothing Then Return Nothing
            Dim res As New List(Of clsUser)

            Dim usersFromStorage As List(Of clsUser)
            With ProjectManager.StorageManager
                usersFromStorage = GetUsersList(.ProjectLocation, .StorageType, .ProviderType, .ModelID)
            End With
            For Each user As clsUser In usersFromStorage
                If Not user Is Nothing AndAlso ProjectManager.GetUserByEMail(user.UserEMail) Is Nothing Then
                    res.Add(user)
                End If
            Next
            Return res
        End Function

        Public Function AddMissingUsersToProject(ByVal ProjectManager As clsProjectManager) As Boolean
            If ProjectManager Is Nothing Then Return False
            Dim newUsers As List(Of clsUser) = GetMissingUsersList(ProjectManager)
            If newUsers Is Nothing OrElse newUsers.Count = 0 Then Return False
            For Each user As clsUser In newUsers
                ProjectManager.AddUser(user)
            Next
            Return True
        End Function

        Public Function UpdateMPropertiesWithRAFlag(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType) As Boolean 'C0825 + D0950
            If Not CheckDBConnection(ProviderType, ConnectionString) Then
                Return False
            End If

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "UPDATE MProperties SET PValue=? WHERE PropertyName=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PValue", "1"))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "PropertyName", "LaunchRAforComparion"))
                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                If affected = 0 Then
                    oCommand.CommandText = "INSERT INTO MProperties (PValue,PropertyName) " +
                                                    "VALUES (?, ?)"
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                End If


                oCommand = Nothing
                dbConnection.Close()
            End Using

            Return True
        End Function

        Public Function DownloadProject_CanvasStreamDatabase(ByVal ConnectionString As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer, ByVal FilePath As String, isRisk As Boolean) As Boolean 'C0742 + D2256
            Debug.Print("Download project started: " + Now.ToString)
            Dim PM As New clsProjectManager(True, isRisk)   ' D2256
            PM.StorageManager.ProjectLocation = ConnectionString
            PM.StorageManager.ProviderType = ProviderType
            PM.StorageManager.ModelID = ModelID

            Dim res As Boolean = True

            Dim FS As IO.FileStream = Nothing
            Try
                FS = New IO.FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write)
                PM.StorageManager.Reader.LoadFullProjectStream(FS)
            Catch
                res = False
            Finally
                If FS IsNot Nothing Then
                    FS.Close()
                End If
            End Try

            Debug.Print("Download project ended: " + Now.ToString)
            Return res
        End Function

        Public Sub PrintDebugInfo(inputString As String, Optional forcePrint As Boolean = False)
            If False Or forcePrint Then ' set this to false if no need to show debug.print
                Debug.Print(inputString + "   ( " + Now.ToString("hh:mm:ss.ffff tt") + ")")
            End If
        End Sub

    End Module
End Namespace