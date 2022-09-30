Imports ECCore
Imports System.Data.Common
Imports System.IO
Imports ECSecurity.ECSecurity
Imports ECCore.MiscFuncs
Imports Canvas

Namespace ECCore
    <Serializable()> Public Class StorageReaderStreams
        Inherits clsStorageReader

        Protected sReader As clsStreamModelReader
        Public ReadOnly Property StreamReader() As clsStreamModelReader
            Get
                If sReader Is Nothing Then CreateStreamReader
                Return sReader
            End Get
        End Property

        Public Sub CreateStreamReader()
            sReader = GetStreamModelReader(CanvasDBVersion)
        End Sub

        Private sub InitStreamReader(ByVal BinaryStream As Stream)
            BinaryStream.Seek(0, SeekOrigin.Begin)
            StreamReader.BinaryStream = BinaryStream
            StreamReader.ProjectManager = ProjectManager
        End sub

        Private Function AddJudgmentsToCombined_BinaryStream(ByVal BinaryStream As Stream, User As ECCore.ECTypes.clsUser, Group As ECCore.Groups.clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer
            InitStreamReader(BinaryStream)
            Return StreamReader.AddJudgmentsToCombined(User, Group)
        End Function

        Private Function GetMadeJudgementsCount_BinaryStream(BinaryStream As System.IO.Stream, User As ECCore.ECTypes.clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadMadeJudgementsCount(User, LastJudgmentTime, HierarchyID)
        End Function

        Private Function JudgmentsExist_BinaryStream(BinaryStream As System.IO.Stream, User As clsUser, Optional HierarchyID As Integer = -1) As Boolean
            InitStreamReader(BinaryStream)
            Return StreamReader.JudgmentsExists(User, HierarchyID)
        End Function

        Private Function LoadUserJudgments_BinaryStream(ByVal BinaryStream As Stream, ByVal User As ECCore.ECTypes.clsUser) As Integer 'C0765
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadUserJudgments(User)
        End Function

        Private Function LoadUserJudgmentsControls_BinaryStream(ByVal BinaryStream As Stream, ByVal User As ECCore.ECTypes.clsUser) As Integer 'C0765
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadUserJudgmentsControls(User)
        End Function

        Private Function LoadUserPermissions_BinaryStream(ByVal BinaryStream As Stream, ByVal User As ECCore.ECTypes.clsUser) As Boolean
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadUserPermissions(User)
        End Function

        Private Function LoadGroupPermissions_BinaryStream(ByVal BinaryStream As Stream, ByVal Group As clsCombinedGroup) As Boolean 'C1030
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadGroupPermissions(Group)
        End Function

        Private Function LoadUserDisabledNodes_BinaryStream(ByVal BinaryStream As Stream, ByVal User As ECCore.ECTypes.clsUser) As Boolean 'C0330
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadUserDisabledNodes(User)
        End Function

        Private Function LoadModelStructure_BinaryStream(ByVal BinaryStream As Stream) As Boolean
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadModelStructure()
        End Function

        Private Function LoadInfoDocs_BinaryStream(ByVal BinaryStream As Stream) As Boolean
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadInfoDocs
        End Function

        Private Function LoadAdvancedInfoDocs_BinaryStream(ByVal BinaryStream As Stream) As Boolean 'C0920
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadAdvancedInfoDocs
        End Function

        Private Function LoadDataMapping_BinaryStream(ByVal BinaryStream As Stream) As Boolean
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadDataMapping()
        End Function

        Private Function LoadEventsGroups_BinaryStream(ByVal BinaryStream As Stream) As Boolean
            InitStreamReader(BinaryStream)
            Return StreamReader.ReadEventsGroups()
        End Function

        Public Function AddJudgmentsToCombined(CombinedGroup As clsCombinedGroup, Optional Hierarchy As clsHierarchy = Nothing) As Integer
            PrintDebugInfo("AddJudgmentsToCombined_CanvasStreamDatabase start: ")
            If ProjectManager Is Nothing OrElse ProjectManager.User Is Nothing Then Return False

            Dim dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
            dbConnection.Open()

            Dim oCommand As DbCommand = GetDBCommand(ProviderType)
            oCommand.Connection = dbConnection

            Dim dbReader As DbDataReader

            Dim MS As MemoryStream

            Dim UserID As Integer
            Dim AUser As clsUser

            oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND UserID>=0 AND (DataType=? OR DataType=?) ORDER BY UserID, DataType DESC"
            oCommand.Parameters.Clear()
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType1", CInt(UserDataType.udtJudgments)))

            dbReader = DBExecuteReader(ProviderType, oCommand)

            Dim res As Integer = -1

            Dim UsersToUpdateTime As New List(Of clsUser)

            Dim prevUserID As Integer = Integer.MinValue

            While dbReader.Read
                UserID = dbReader("UserID")
                If UserID >= 0 Then
                    AUser = ProjectManager.GetUserByID(UserID)
                Else
                    AUser = Nothing
                End If
                If AUser IsNot Nothing AndAlso CombinedGroup.UsersList.Contains(AUser) Then
                    MS = New MemoryStream
                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long ' The bytes returned from GetBytes.

                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                    bw.Write(outbyte)
                    bw.Flush()

                    Select Case CType(CInt(dbReader("DataType")), UserDataType)
                        Case UserDataType.udtJudgments
                            res = AddJudgmentsToCombined_BinaryStream(MS, AUser, CombinedGroup, Hierarchy)

                            If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                                AUser.LastJudgmentTime = dbReader("ModifyDate")
                                If AUser.LastJudgmentTime = VERY_OLD_DATE Then
                                    AUser.LastJudgmentTime = Now
                                    UsersToUpdateTime.Add(AUser)
                                End If
                            Else
                                AUser.LastJudgmentTime = Now
                                UsersToUpdateTime.Add(AUser)
                            End If
                        Case UserDataType.udtPermissions
                            If prevUserID <> UserID Then
                                If prevUserID <> Integer.MinValue And prevUserID <> ProjectManager.User.UserID Then
                                    'If prevUserID <> Integer.MinValue Then
                                    If CombinedGroup.CombinedUserID = COMBINED_USER_ID Then
                                        ''ProjectManager.UsersRoles.CleanUpUserRoles(prevUserID)
                                        'ProjectManager.UsersRoles.ClearUserObjectivesRoles(DUMMY_PERMISSIONS_USER_ID)
                                        'ProjectManager.UsersRoles.ClearUserAlternativesRolesAll(DUMMY_PERMISSIONS_USER_ID)
                                    End If
                                End If
                                LoadUserPermissions_BinaryStream(MS, AUser)
                            End If
                            prevUserID = UserID
                    End Select
                End If
            End While
            dbReader.Close()

            If prevUserID <> Integer.MinValue And prevUserID <> ProjectManager.User.UserID Then
                If CombinedGroup.CombinedUserID = COMBINED_USER_ID Then
                    ''ProjectManager.UsersRoles.CleanUpUserRoles(prevUserID)
                    'ProjectManager.UsersRoles.CleanUpUserRoles(DUMMY_PERMISSIONS_USER_ID)
                End If
            End If

            Dim affected As Integer
            For Each U As clsUser In UsersToUpdateTime
                oCommand.CommandText = "UPDATE UserData SET ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", U.LastJudgmentTime))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            Next

            dbConnection.Close()

            PrintDebugInfo("AddJudgmentsToCombined_CanvasStreamDatabase end: ")
            Return res
        End Function

        'Public Function LoadFullProjectStream(ByVal OutputStream As Stream, Optional ByVal PerformEncryption As Boolean = False, Optional ByVal Password As String = DEFAULT_ENCRYPTION_PASSWORD, Optional ByRef SnapshotsStreamPosition As Integer = -1) As Boolean
        '    If (OutputStream Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return False

        '    PerformEncryption = _DEBUG_ENCRYPTION_ENABLED 'C0380

        '    ' If encryption is ON then we will write to the StreamForEncryption, after that encrypt the bytes from this stream and put them into the OutputStream

        '    Dim StreamForEncryption As New MemoryStream
        '    Dim encryptBW As New BinaryWriter(StreamForEncryption)

        '    OutputStream.Seek(0, SeekOrigin.Begin)
        '    Dim BW As New BinaryWriter(OutputStream)

        '    Dim currentWriter As BinaryWriter = If(PerformEncryption, encryptBW, BW) 'C0380

        '    ' First, write the CHUNK_CANVAS_STREAMS_PROJECT, so that we will know that this is a streams project
        '    currentWriter.Write(CHUNK_CANVAS_STREAMS_PROJECT) 'C0380

        '    ' Write the version number
        '    Dim version As ECCanvasDatabaseVersion = GetDBVersion_CanvasStreamDatabase(Location, ProviderType, ModelID)
        '    Dim VersionByteArray As Byte() = StringToByteArray(version.GetVersionString)

        '    currentWriter.Write(CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION)
        '    currentWriter.Write(VersionByteArray.Length)
        '    currentWriter.Write(VersionByteArray)

        '    Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
        '        dbConnection.Open()

        '        Dim oCommand As DbCommand = GetDBCommand(ProviderType)
        '        oCommand.Connection = dbConnection

        '        ' READ STREAMS FROM ModelStructure TABLE
        '        oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? ORDER BY StructureType"
        '        oCommand.Parameters.Clear()
        '        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))

        '        Dim dbReader As DbDataReader
        '        dbReader = DBExecuteReader(ProviderType, oCommand)

        '        Dim time As DateTime

        '        While dbReader.Read
        '            Dim MS As New MemoryStream

        '            ' Determine what kind of stream did we get
        '            Dim sType As StructureType
        '            sType = CType(dbReader("StructureType"), StructureType)

        '            If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
        '                time = dbReader("ModifyDate")
        '            Else
        '                time = VERY_OLD_DATE
        '            End If

        '            If time < VERY_OLD_DATE Then
        '                time = VERY_OLD_DATE
        '            End If

        '            Dim byteArray As Byte()

        '            Select Case sType

        '                Case StructureType.stModelVersion,
        '                StructureType.stModelStructure,
        '                StructureType.stInfoDocs,
        '                StructureType.stAHPExtraTables,
        '                StructureType.stPipeMessages,
        '                StructureType.stPipeOptions,
        '                StructureType.stProperties,
        '                StructureType.stAntiguaDashboard,
        '                StructureType.stAntiguaRecycleBin,
        '                StructureType.stAntiguaInfoDocs,
        '                StructureType.stAdvancedInfoDocs,
        '                StructureType.stSpyronStructureWelcome,
        '                StructureType.stSpyronStructureThankYou,
        '                StructureType.stSpyronStructureImpactWelcome,
        '                StructureType.stSpyronStructureImpactThankyou,
        '                StructureType.stSpyronModelVersion,
        '                StructureType.stAttributes,
        '                StructureType.stAntiguaDashboardImpact,
        '                StructureType.stAntiguaRecycleBinImpact,
        '                StructureType.stAntiguaInfoDocsImpact,
        '                StructureType.stControls,
        '                StructureType.stRegions,
        '                StructureType.stResourceAligner,
        '                StructureType.stResourceAlignerNew,
        '                StructureType.stResourceAlignerTimePeriods,
        '                StructureType.stProjectParameters,
        '                StructureType.stProjectParametersValues,
        '                StructureType.stStatus

        '                    If sType = StructureType.stModelVersion Then 'C0376
        '                        ' Write the structure stream to the result stream
        '                        currentWriter.Write(CInt(CHUNK_CANVAS_STREAMS_PROJECT_BASE + CInt(sType)))
        '                        byteArray = dbReader("Stream")
        '                        currentWriter.Write(byteArray.Length)
        '                        currentWriter.Write(byteArray)
        '                    Else
        '                        Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
        '                        'C0376===
        '                        If bufferSize <= 0 Then
        '                            bufferSize = 100
        '                        End If
        '                        Dim outbyte(bufferSize - 1) As Byte                         ' The BLOB byte() buffer to be filled by GetBytes.
        '                        Dim retval As Long                                          ' The bytes returned from GetBytes.
        '                        Dim startIndex As Long = 0                                  ' The starting position in the BLOB output.

        '                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

        '                        Dim tmpBW As BinaryWriter = New BinaryWriter(MS)

        '                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
        '                        Do While retval = bufferSize
        '                            tmpBW.Write(outbyte)
        '                            tmpBW.Flush()

        '                            ' Reposition the start index to the end of the last buffer and fill the buffer.
        '                            startIndex += bufferSize
        '                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
        '                        Loop

        '                        ' Write the remaining buffer.
        '                        tmpBW.Write(outbyte, 0, retval)
        '                        tmpBW.Flush()

        '                        ' Write the structure stream to the result stream
        '                        currentWriter.Write(CInt(CHUNK_CANVAS_STREAMS_PROJECT_BASE + CInt(sType)))
        '                        byteArray = MS.ToArray
        '                        currentWriter.Write(byteArray.Length)
        '                        currentWriter.Write(byteArray)

        '                        currentWriter.Write(time.ToBinary)
        '                    End If
        '            End Select
        '        End While

        '        dbReader.Close() 'C0377

        '        ' READ STREAMS FROM UserData TABLE
        '        oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? ORDER BY DataType"
        '        oCommand.Parameters.Clear()
        '        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))

        '        dbReader = DBExecuteReader(ProviderType, oCommand)

        '        While dbReader.Read
        '            Dim MS As New MemoryStream

        '            ' Determine what kind of stream did we get
        '            Dim udType As UserDataType
        '            udType = CType(dbReader("DataType"), UserDataType)

        '            If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
        '                time = dbReader("ModifyDate")
        '            Else
        '                time = VERY_OLD_DATE
        '            End If

        '            Dim byteArray As Byte()

        '            Select Case udType
        '                Case UserDataType.udtJudgments,
        '                UserDataType.udtPermissions,
        '                UserDataType.udtDisabledNodes,
        '                UserDataType.udtSpyronAnswersWelcome,
        '                UserDataType.udtSpyronAnswersThankYou,
        '                UserDataType.udtAttributeValues,
        '                UserDataType.udtJudgmentsControls,
        '                UserDataType.udtPermissionsControls,
        '                 UserDataType.udtComments

        '                    ' READ THE ACTUAL STREAM THEN
        '                    Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
        '                    Dim outbyte(bufferSize - 1) As Byte                         ' The BLOB byte() buffer to be filled by GetBytes.
        '                    Dim retval As Long                                          ' The bytes returned from GetBytes.
        '                    Dim startIndex As Long = 0                                  ' The starting position in the BLOB output.

        '                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

        '                    Dim tmpBW As BinaryWriter = New BinaryWriter(MS)

        '                    ' Continue reading and writing while there are bytes beyond the size of the buffer.
        '                    Do While retval = bufferSize
        '                        tmpBW.Write(outbyte)
        '                        tmpBW.Flush()

        '                        ' Reposition the start index to the end of the last buffer and fill the buffer.
        '                        startIndex += bufferSize
        '                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
        '                    Loop

        '                    ' Write the remaining buffer.
        '                    tmpBW.Write(outbyte, 0, retval)
        '                    tmpBW.Flush()

        '                    ' Write the user data stream to the result stream
        '                    currentWriter.Write(CInt(CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE + CInt(udType)))
        '                    currentWriter.Write(CInt(dbReader("UserID")))
        '                    byteArray = MS.ToArray
        '                    currentWriter.Write(byteArray.Length)
        '                    currentWriter.Write(byteArray)

        '                    currentWriter.Write(time.ToBinary)
        '                    'C0380==
        '            End Select
        '        End While

        '        dbReader.Close()
        '    End Using

        '    encryptBW.Close()
        '    If PerformEncryption Then
        '        Dim plainTextBytes As Byte() = StreamForEncryption.ToArray
        '        Dim encryptedBytes As Byte() = Encrypt(plainTextBytes, Password)
        '        BW.BaseStream.Seek(0, SeekOrigin.Begin)
        '        BW.Write(encryptedBytes)
        '    End If

        '    Return True
        'End Function

        Public Function LoadFullProjectStream(ByVal OutputStream As Stream, Optional ByVal PerformEncryption As Boolean = False, Optional ByVal Password As String = DEFAULT_ENCRYPTION_PASSWORD, Optional ByRef SnapshotsStreamPosition As Integer = -1) As Boolean
            If OutputStream Is Nothing Then Return False

            PerformEncryption = _DEBUG_ENCRYPTION_ENABLED 'C0380

            ' If encryption is ON then we will write to the StreamForEncryption, after that encrypt the bytes from this stream and put them into the OutputStream

            Dim StreamForEncryption As New MemoryStream
            Dim encryptBW As New BinaryWriter(StreamForEncryption)

            OutputStream.Seek(0, SeekOrigin.Begin)
            Dim BW As New BinaryWriter(OutputStream)

            Dim currentWriter As BinaryWriter = If(PerformEncryption, encryptBW, BW) 'C0380

            ' First, write the CHUNK_CANVAS_STREAMS_PROJECT, so that we will know that this is a streams project
            currentWriter.Write(CHUNK_CANVAS_STREAMS_PROJECT) 'C0380

            ' Write the version number
            Dim version As ECCanvasDatabaseVersion = GetDBVersion_CanvasStreamDatabase(Location, ProviderType, ModelID)
            Dim VersionByteArray As Byte() = StringToByteArray(version.GetVersionString)

            currentWriter.Write(CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION)
            currentWriter.Write(VersionByteArray.Length)
            currentWriter.Write(VersionByteArray)

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                ' READ STREAMS FROM ModelStructure TABLE
                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? ORDER BY StructureType"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim time As DateTime

                While dbReader.Read
                    Dim MS As New MemoryStream

                    ' Determine what kind of stream did we get
                    Dim sType As StructureType
                    sType = CType(dbReader("StructureType"), StructureType)

                    If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                        time = dbReader("ModifyDate")
                    Else
                        time = VERY_OLD_DATE
                    End If

                    If time < VERY_OLD_DATE Then
                        time = VERY_OLD_DATE
                    End If

                    Dim byteArray As Byte()

                    If ModelStuctureStreamIDs.Contains(sType) Then
                        If sType = StructureType.stModelVersion Then 'C0376
                            ' Write the structure stream to the result stream
                            currentWriter.Write(CInt(CHUNK_CANVAS_STREAMS_PROJECT_BASE + CInt(sType)))
                            byteArray = dbReader("Stream")
                            currentWriter.Write(byteArray.Length)
                            currentWriter.Write(byteArray)
                        Else
                            Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                            'C0376===
                            If bufferSize <= 0 Then
                                bufferSize = 100
                            End If
                            Dim outbyte(bufferSize - 1) As Byte                         ' The BLOB byte() buffer to be filled by GetBytes.
                            Dim retval As Long                                          ' The bytes returned from GetBytes.
                            Dim startIndex As Long = 0                                  ' The starting position in the BLOB output.

                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                            Dim tmpBW As BinaryWriter = New BinaryWriter(MS)

                            ' Continue reading and writing while there are bytes beyond the size of the buffer.
                            Do While retval = bufferSize
                                tmpBW.Write(outbyte)
                                tmpBW.Flush()

                                ' Reposition the start index to the end of the last buffer and fill the buffer.
                                startIndex += bufferSize
                                retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                            Loop

                            ' Write the remaining buffer.
                            tmpBW.Write(outbyte, 0, retval)
                            tmpBW.Flush()

                            ' Write the structure stream to the result stream
                            currentWriter.Write(CInt(CHUNK_CANVAS_STREAMS_PROJECT_BASE + CInt(sType)))
                            byteArray = MS.ToArray
                            currentWriter.Write(byteArray.Length)
                            currentWriter.Write(byteArray)

                            currentWriter.Write(time.ToBinary)
                        End If
                    End If
                End While

                dbReader.Close() 'C0377

                ' READ STREAMS FROM UserData TABLE
                oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? ORDER BY DataType"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))

                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    Dim MS As New MemoryStream

                    ' Determine what kind of stream did we get
                    Dim udType As UserDataType
                    udType = CType(dbReader("DataType"), UserDataType)

                    If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                        time = dbReader("ModifyDate")
                    Else
                        time = VERY_OLD_DATE
                    End If

                    Dim byteArray As Byte()

                    If UserDataStreamIDs.Contains(udType) Then
                        ' READ THE ACTUAL STREAM THEN
                        Dim bufferSize As Integer = CInt(dbReader("StreamSize"))    ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte                         ' The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long                                          ' The bytes returned from GetBytes.
                        Dim startIndex As Long = 0                                  ' The starting position in the BLOB output.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                        Dim tmpBW As BinaryWriter = New BinaryWriter(MS)

                        ' Continue reading and writing while there are bytes beyond the size of the buffer.
                        Do While retval = bufferSize
                            tmpBW.Write(outbyte)
                            tmpBW.Flush()

                            ' Reposition the start index to the end of the last buffer and fill the buffer.
                            startIndex += bufferSize
                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                        Loop

                        ' Write the remaining buffer.
                        tmpBW.Write(outbyte, 0, retval)
                        tmpBW.Flush()

                        ' Write the user data stream to the result stream
                        currentWriter.Write(CInt(CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE + CInt(udType)))
                        currentWriter.Write(CInt(dbReader("UserID")))
                        byteArray = MS.ToArray
                        currentWriter.Write(byteArray.Length)
                        currentWriter.Write(byteArray)

                        currentWriter.Write(time.ToBinary)
                    End If
                End While

                dbReader.Close()
            End Using

            encryptBW.Close()
            If PerformEncryption Then
                Dim plainTextBytes As Byte() = StreamForEncryption.ToArray
                Dim encryptedBytes As Byte() = Encrypt(plainTextBytes, Password)
                BW.BaseStream.Seek(0, SeekOrigin.Begin)
                BW.Write(encryptedBytes)
            End If

            Return True
        End Function

        Public Function LoadUserControlsPermissions(Optional UserID As Integer = -1) As Boolean
            If ProjectManager Is Nothing Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                Dim AUser As clsUser

                If UserID <> -1 Then
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissionsControls)))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", UserID))
                Else
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissionsControls)))
                End If
                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    UserID = dbReader("UserID")
                    AUser = ProjectManager.GetUserByID(UserID)

                    If AUser IsNot Nothing Then
                        MS = Nothing
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        If AUser IsNot Nothing Then
                            StreamReader.BinaryStream = MS
                            StreamReader.ProjectManager = ProjectManager
                            StreamReader.ReadControlsPermissions(UserID)
                        End If
                    End If
                End While
                dbReader.Close()
            End Using

            Return True
        End Function

        Public Overloads Function GetModelStructureStream(StructureType As StructureType, ByRef time As Nullable(Of DateTime)) As MemoryStream
            Dim MS As New MemoryStream
            'If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, Location) Then Return MS

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                If dbReader.HasRows Then
                    dbReader.Read()

                    If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                        time = dbReader("ModifyDate")
                    Else
                        time = Nothing
                    End If

                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize) ' The bytes returned from GetBytes.

                    bw.Write(outbyte)
                    bw.Flush()
                End If

                dbReader.Close()

                If time Is Nothing Then
                    time = Now

                    oCommand.CommandText = "UPDATE ModelStructure SET ModifyDate=? WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))
                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                End If
            End Using

            Return MS
        End Function

        Public Overloads Function GetModelStructureStream(StructureType As StructureType, ByRef time As Nullable(Of DateTime), ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As MemoryStream
            Dim MS As New MemoryStream

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))

                Dim dbReader As DbDataReader
                dbReader = DBExecuteReader(ProviderType, oCommand)

                If dbReader.HasRows Then
                    dbReader.Read()

                    If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                        time = dbReader("ModifyDate")
                    Else
                        time = Nothing
                    End If

                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize) ' The bytes returned from GetBytes.

                    bw.Write(outbyte)
                    bw.Flush()
                End If

                dbReader.Close()

                If time Is Nothing Then
                    time = Now

                    oCommand.CommandText = "UPDATE ModelStructure SET ModifyDate=? WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))
                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                End If
            End Using

            Return MS
        End Function

        Public Function GetModelStructureStreamTime(StructureType As StructureType) As Nullable(Of DateTime)
            Dim time As Nullable(Of DateTime) = Nothing
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT ModifyDate FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()

                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", (ModelID)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))

                Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)

                If Not TypeOf (obj) Is DBNull AndAlso obj IsNot Nothing Then
                    time = CType(obj, DateTime)
                End If

                If time Is Nothing Then
                    time = Now

                    oCommand.CommandText = "UPDATE ModelStructure SET ModifyDate=? WHERE ProjectID=? AND StructureType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))
                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                End If
            End Using

            Return time
        End Function

        Public Function GetUserDataStreamTime(UserID As Integer, DataType As UserDataType) As Nullable(Of DateTime)
            Dim time As Nullable(Of DateTime) = Nothing
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT ModifyDate FROM ModelStructure WHERE ProjectID=? AND UserID=? AND DataType=?"
                oCommand.Parameters.Clear()

                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", UserID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(DataType)))

                Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)

                If Not TypeOf (obj) Is DBNull AndAlso obj IsNot Nothing Then
                    time = CType(obj, DateTime)
                End If

                If time Is Nothing Then
                    time = Now

                    oCommand.CommandText = "UPDATE UserData SET ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", UserID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(DataType)))
                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)
                End If
            End Using

            Return time
        End Function

        Private Function NeedToLoadModelStructure(StructureType As StructureType) As Boolean
            Dim loadedTime As Nullable(Of DateTime) = ProjectManager.CacheManager.StructureLoaded(StructureType)
            If loadedTime IsNot Nothing Then
                PrintDebugInfo("Model structure already loaded at " + loadedTime.ToString)
                Dim storedTime As Nullable(Of DateTime) = GetModelStructureStreamTime(StructureType)
                If storedTime <= loadedTime Then Return False
            End If
            Return True
        End Function

        Private Function NeedToLoadUserData(UserID As Integer, DataType As UserDataType) As Boolean
            Dim loadedTime As Nullable(Of DateTime) = ProjectManager.CacheManager.UserDataLoaded(UserID, DataType)
            If loadedTime IsNot Nothing Then
                PrintDebugInfo("User data already loaded at " + loadedTime.ToString)
                Dim storedTime As Nullable(Of DateTime) = GetUserDataStreamTime(UserID, DataType)
                If storedTime <= loadedTime Then Return False
            End If
            Return True
        End Function

        Public Function LoadModelStructure() As Boolean
            If Not NeedToLoadModelStructure(StructureType.stModelStructure) Then Return True

            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = GetModelStructureStream(StructureType.stModelStructure, time)

            ProjectManager.LastModifyTime = time

            If MS.Length <> 0 Then
                Dim res As Boolean = LoadModelStructure_BinaryStream(MS)
                If res Then ProjectManager.CacheManager.StructureLoaded(StructureType.stModelStructure) = time
                'If res Then
                '    For Each DI As clsDataInstance In ProjectManager.DataInstances
                '        LoadUserJudgments(DI.User)
                '    Next
                'End If
                Return res
            Else
                Return False
            End If
        End Function

        Public Function LoadInfoDocsFromDB() As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = GetModelStructureStream(StructureType.stInfoDocs, time)

            If MS.Length <> 0 Then
                Return LoadInfoDocs_BinaryStream(MS)
            Else
                Return True
            End If
        End Function

        Public Function LoadAdvancedInfoDocs() As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = GetModelStructureStream(StructureType.stAdvancedInfoDocs, time)

            If MS.Length <> 0 Then
                Return LoadAdvancedInfoDocs_BinaryStream(MS)
            Else
                Return True
            End If
        End Function

        Public Function GetMadeJudgementsCount(ByVal User As clsUser, ByRef LastJudgmentTime As DateTime, Optional HierarchyID As Integer = -1) As Integer
            If ProjectManager Is Nothing Then Return -1

            Dim MS As New MemoryStream
            Dim res As Integer = 0

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim UserID As Integer
                Dim AUser As clsUser

                oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))

                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    MS = New MemoryStream

                    UserID = dbReader("UserID")

                    AUser = ProjectManager.GetUserByID(UserID)

                    If AUser IsNot Nothing Then
                        MS = Nothing
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        If AUser.UserID <> ProjectManager.UserID Then
                            'ProjectManager.CleanUpUserDataFromMemory(HierarchyID, AUser.UserID)
                        End If

                        res = GetMadeJudgementsCount_BinaryStream(MS, AUser, LastJudgmentTime, HierarchyID)
                        LastJudgmentTime = dbReader("ModifyDate")

                        'If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                        AUser.LastJudgmentTime = LastJudgmentTime
                        'End If
                    End If
                End While
                dbReader.Close()
            End Using
            Return res
        End Function

        Public Function GetEvaluationProgress(Users As List(Of clsUser), HierarchyID As Integer, ByRef MadeCount As Integer, ByRef TotalCount As Integer, Optional CleanUpRolesAfterLoad As Boolean = True) As Dictionary(Of String, UserEvaluationProgressData)
            If ProjectManager Is Nothing Then Return Nothing
            Dim res As New Dictionary(Of String, UserEvaluationProgressData)

            MadeCount = 0
            TotalCount = 0

            Dim uMadeCount As Integer = 0
            Dim uTotalCount As Integer = 0

            Dim isAllUsers As Boolean = Users.Count = ProjectManager.UsersList.Count
            Dim usersHS As New Dictionary(Of Integer, clsUser)
            For Each u As clsUser In Users
                usersHS.Add(u.UserID, u)
            Next

            Dim uList As New List(Of clsUser)
            If isAllUsers Then
                uList.Add(Users(0))
            Else
                uList = Users
            End If

            Dim defTotal As Integer = ProjectManager.GetDefaultTotalJudgmentCount(HierarchyID)

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As MemoryStream

                Dim UserID As Integer
                Dim AUser As clsUser

                Dim UsersToUpdateTime As New List(Of clsUser)

                Dim prevRolesID As Integer = Integer.MinValue
                Dim prevJudgmentsID As Integer = Integer.MinValue

                For Each user As clsUser In uList
                    If isAllUsers Then
                        oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND UserID>=0 AND (DataType=? OR DataType=? OR DataType=?) ORDER BY UserID, DataType DESC"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType1", CInt(UserDataType.udtDisabledNodes)))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType2", CInt(UserDataType.udtJudgments)))
                    Else
                        oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND UserID=? AND (DataType=? OR DataType=? OR DataType=?) ORDER BY UserID, DataType DESC"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", user.UserID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType1", CInt(UserDataType.udtDisabledNodes)))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType2", CInt(UserDataType.udtJudgments)))
                    End If

                    dbReader = DBExecuteReader(ProviderType, oCommand)

                    While dbReader.Read
                        UserID = dbReader("UserID")
                        If usersHS.ContainsKey(UserID) Then
                            AUser = usersHS(UserID)

                            MS = New MemoryStream

                            Dim bufferSize As Integer = dbReader("StreamSize")      ' The size of the BLOB buffer.
                            Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                            Dim retval As Long                   ' The bytes returned from GetBytes.

                            retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)
                            Dim bw As BinaryWriter = New BinaryWriter(MS)
                            bw.Write(outbyte)
                            bw.Flush()

                            Select Case CType(CInt(dbReader("DataType")), UserDataType)
                                Case UserDataType.udtJudgments
                                    Dim jTime As DateTime

                                    uMadeCount = GetMadeJudgementsCount_BinaryStream(MS, AUser, jTime, HierarchyID)

                                    If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                                        AUser.LastJudgmentTime = dbReader("ModifyDate")
                                        If AUser.LastJudgmentTime = VERY_OLD_DATE Then
                                            AUser.LastJudgmentTime = Now
                                            UsersToUpdateTime.Add(AUser)
                                        End If
                                    Else
                                        AUser.LastJudgmentTime = Now
                                        UsersToUpdateTime.Add(AUser)
                                    End If

                                    Dim hasTotalCount As Boolean = True
                                    If Not res.ContainsKey(AUser.UserEMail.ToLower) Then
                                        res.Add(AUser.UserEMail.ToLower, New UserEvaluationProgressData)
                                        hasTotalCount = False
                                    End If
                                    Dim eData As UserEvaluationProgressData = res(AUser.UserEMail.ToLower)
                                    eData.ID = UserID
                                    eData.Email = AUser.UserEMail
                                    eData.EvaluatedCount = uMadeCount
                                    MadeCount += uMadeCount
                                    If Not hasTotalCount Then
                                        eData.TotalCount = ProjectManager.GetTotalJudgmentCount(HierarchyID, UserID, False, defTotal)
                                        TotalCount += eData.TotalCount
                                    End If
                                    eData.LastJudgmentTime = AUser.LastJudgmentTime
                                Case UserDataType.udtPermissions
                                    If prevRolesID <> Integer.MinValue AndAlso prevRolesID <> ProjectManager.User.UserID Then
                                        ProjectManager.UsersRoles.CleanUpUserRoles(prevRolesID)
                                    End If
                                    LoadUserPermissions_BinaryStream(MS, AUser)
                                    If Not res.ContainsKey(AUser.UserEMail.ToLower) Then
                                        res.Add(AUser.UserEMail.ToLower, New UserEvaluationProgressData)
                                    End If
                                    Dim eData As UserEvaluationProgressData = res(AUser.UserEMail.ToLower)
                                    eData.ID = UserID
                                    eData.Email = AUser.UserEMail
                                    eData.TotalCount = ProjectManager.GetTotalJudgmentCount(HierarchyID, UserID, False, defTotal)
                                    TotalCount += eData.TotalCount

                                    prevRolesID = UserID
                                Case UserDataType.udtDisabledNodes
                                    LoadUserDisabledNodes_BinaryStream(MS, AUser)
                            End Select
                        End If
                    End While
                    dbReader.Close()
                Next


                If prevRolesID <> Integer.MinValue AndAlso prevRolesID <> ProjectManager.User.UserID Then
                    If CleanUpRolesAfterLoad Then
                        ProjectManager.UsersRoles.CleanUpUserRoles(prevRolesID)
                    End If
                End If

                Dim affected As Integer
                For Each U As clsUser In UsersToUpdateTime
                    oCommand.CommandText = "UPDATE UserData SET ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", U.LastJudgmentTime))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Next

            End Using

            For Each u As clsUser In Users
                If Not res.ContainsKey(u.UserEMail.ToLower) Then
                    Dim eData As New UserEvaluationProgressData
                    eData.ID = u.UserID
                    eData.Email = u.UserEMail
                    eData.EvaluatedCount = 0
                    eData.TotalCount = defTotal
                    eData.TotalCount = ProjectManager.GetTotalJudgmentCount(HierarchyID, u.UserID, defTotal)
                    TotalCount += eData.TotalCount
                    eData.LastJudgmentTime = VERY_OLD_DATE
                    res.Add(u.UserEMail.ToLower, eData)
                End If
            Next

            Return res
        End Function

        Public Function LoadUserJudgments(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Integer 'C0765
            If ProjectManager Is Nothing Then Return False

            Dim res As Integer = -1 'C0765

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                Dim UserID As Integer
                Dim AUser As clsUser

                ' Read judgments
                If User Is Nothing Then
                    'oCommand.CommandText = "SELECT * FROM UserData WHERE DataType=" + CInt(UserDataType.udtJudgments).ToString 'C0266
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                    'C0266===
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                    'C0266==
                Else
                    'oCommand.CommandText = "SELECT * FROM UserData WHERE DataType=" + CInt(UserDataType.udtJudgments).ToString + " AND UserID=?" 'C0266
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                    'C0266===
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                    'C0266==
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                End If
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim UsersToUpdateTime As New List(Of clsUser)

                While dbReader.Read
                    UserID = dbReader("UserID")

                    'AUser = ProjectManager.GetUserByID(UserID) 'C0277
                    'C0277===
                    If UserID >= 0 Then
                        AUser = ProjectManager.GetUserByID(UserID)
                    Else
                        AUser = ProjectManager.GetDataInstanceUserByID(UserID)
                    End If
                    'C0277==

                    If AUser IsNot Nothing Then 'C0275
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        ProjectManager.CleanUpUserDataFromMemory(ProjectManager.ActiveObjectives.HierarchyID, AUser.UserID, , True)

                        'LoadUserJudgments_BinaryStream(MS, AUser) 'C0765
                        res = LoadUserJudgments_BinaryStream(MS, AUser) 'C0765

                        'C0333===
                        If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                            AUser.LastJudgmentTime = dbReader("ModifyDate")
                            If AUser.LastJudgmentTime = VERY_OLD_DATE Then
                                AUser.LastJudgmentTime = Now
                                UsersToUpdateTime.Add(AUser)
                            End If
                        Else
                            AUser.LastJudgmentTime = Now
                            UsersToUpdateTime.Add(AUser)
                        End If
                        'C0333==
                    End If
                End While
                dbReader.Close()

                Dim affected As Integer
                For Each U As clsUser In UsersToUpdateTime
                    oCommand.CommandText = "UPDATE UserData SET ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", U.LastJudgmentTime))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Next

            End Using

            If User IsNot Nothing AndAlso User Is ProjectManager.User Then
                ProjectManager.AddEmptyMissingJudgments(ProjectManager.ActiveHierarchy, ProjectManager.ActiveAltsHierarchy, User)
            End If

            Return res 'C0765
        End Function

        Public Function LoadUserJudgmentsControls(ByRef LastJudgmentTime As DateTime, Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Integer
            If ProjectManager Is Nothing Then Return False

            Dim res As Integer = -1

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                Dim UserID As Integer
                Dim AUser As clsUser

                ' Read judgments
                If User Is Nothing Then
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgmentsControls)))
                Else
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgmentsControls)))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                End If
                dbReader = DBExecuteReader(ProviderType, oCommand)

                Dim UsersToUpdateTime As New List(Of clsUser)

                While dbReader.Read
                    UserID = dbReader("UserID")

                    If UserID >= 0 Then
                        AUser = ProjectManager.GetUserByID(UserID)
                    Else
                        AUser = ProjectManager.GetDataInstanceUserByID(UserID)
                    End If

                    If AUser IsNot Nothing Then 'C0275
                        MS = Nothing
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        ProjectManager.CleanUpControlsJudgmentsFromMemory(AUser.UserID)

                        'LoadUserJudgments_BinaryStream(MS, AUser) 'C0765
                        res = LoadUserJudgmentsControls_BinaryStream(MS, AUser) 'C0765

                        'C0333===
                        If Not TypeOf (dbReader("ModifyDate")) Is DBNull Then
                            LastJudgmentTime = dbReader("ModifyDate")
                            If LastJudgmentTime = VERY_OLD_DATE Then
                                LastJudgmentTime = Now
                                UsersToUpdateTime.Add(AUser)
                            End If
                        Else
                            LastJudgmentTime = Now
                            UsersToUpdateTime.Add(AUser)
                        End If
                        'C0333==
                    End If
                End While
                dbReader.Close()

                Dim affected As Integer
                For Each U As clsUser In UsersToUpdateTime
                    oCommand.CommandText = "UPDATE UserData SET ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", U.LastJudgmentTime))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgmentsControls)))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                Next

            End Using

            Return res 'C0765
        End Function

        Public Function LoadUserPermissions(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Boolean 'C0259
            If ProjectManager Is Nothing Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                Dim UserID As Integer
                Dim AUser As clsUser

                ' Read permissions
                If User Is Nothing Then
                    'oCommand.CommandText = "SELECT * FROM UserData WHERE DataType=" + CInt(UserDataType.udtPermissions).ToString 'C0266
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?" 'C0266
                    'C0266===
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                    'C0266==
                Else
                    'oCommand.CommandText = "SELECT * FROM UserData WHERE DataType=" + CInt(UserDataType.udtPermissions).ToString + " AND UserID=?" 'C0266
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?" 'C0266
                    oCommand.Parameters.Clear()
                    'C0266===
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                    'C0266==
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                End If
                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    UserID = dbReader("UserID")
                    AUser = ProjectManager.GetUserByID(UserID)

                    MS = Nothing
                    MS = New MemoryStream
                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long ' The bytes returned from GetBytes.

                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                    bw.Write(outbyte)
                    bw.Flush()

                    If AUser IsNot Nothing Then 'C0972
                        LoadUserPermissions_BinaryStream(MS, AUser)
                    End If
                End While
                dbReader.Close()
            End Using

            Return True
        End Function

        Public Function LoadGroupPermissions(Optional ByVal Group As clsCombinedGroup = Nothing) As Boolean 'C1030
            If ProjectManager Is Nothing Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                Dim UserID As Integer
                Dim AGroup As clsCombinedGroup

                ' Read permissions
                If Group Is Nothing Then
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID<0"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                Else
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?" 'C0266
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", Group.CombinedUserID))
                End If
                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    UserID = dbReader("UserID")
                    AGroup = ProjectManager.CombinedGroups.GetCombinedGroupByUserID(UserID)

                    If AGroup IsNot Nothing Then
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        LoadGroupPermissions_BinaryStream(MS, AGroup)
                    End If
                End While
                dbReader.Close()
            End Using
            Return True
        End Function

        Public Function LoadUserDisabledNodes(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Boolean 'C0330
            If ProjectManager Is Nothing Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                Dim UserID As Integer
                Dim AUser As clsUser

                ' Read disabled nodes
                If User Is Nothing Then
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtDisabledNodes)))
                Else
                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtDisabledNodes)))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                End If
                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    UserID = dbReader("UserID")
                    AUser = ProjectManager.GetUserByID(UserID)

                    MS = New MemoryStream
                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long ' The bytes returned from GetBytes.

                    retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                    bw.Write(outbyte)
                    bw.Flush()

                    LoadUserDisabledNodes_BinaryStream(MS, AUser)
                End While
                dbReader.Close()
            End Using

            Return True
        End Function

        Public Function JudgmentsMadeBefore(ByVal UserID As Integer, ByVal HierarchyID As Integer, ByVal NodeID As Integer, ByVal Time As Date) As Boolean 'C0333
            If ProjectManager Is Nothing Then Return False

            Dim res As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT MAX(ModifyDate) FROM UserData WHERE ProjectID=? AND UserID=? AND DataType=?"
                oCommand.Parameters.Clear()

                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", (ModelID)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", (UserID)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", (UserDataType.udtJudgments)))

                Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)

                Dim MaxJudgmentTime As DateTime
                If Not TypeOf (obj) Is DBNull Then
                    MaxJudgmentTime = If(obj Is Nothing, VERY_OLD_DATE, CType(obj, DateTime))
                Else
                    MaxJudgmentTime = VERY_OLD_DATE
                End If

                res = (Time >= MaxJudgmentTime) And (MaxJudgmentTime <> VERY_OLD_DATE)

            End Using
            Return res
        End Function

        Public Function LoadExtraAHPTables() As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = GetModelStructureStream(StructureType.stAHPExtraTables, time)

            If MS.Length <> 0 Then
                MS.Position = 0
                ProjectManager.ExtraAHPTables = MS
            End If

            Return True
        End Function

        Protected Function IsSurveyAvailable(SurveyType As StructureType) As Boolean
            Dim count As Integer = 0
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "SELECT COUNT(*) FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", SurveyType))
                Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                count = If(obj Is Nothing, 0, CType(obj, Integer))

                oCommand = Nothing
            End Using
            Return count <> 0
        End Function

        Public Function IsWelcomeSurveyAvailable(fIsImpact As Boolean) As Boolean
            Return IsSurveyAvailable(If(fIsImpact, StructureType.stSpyronStructureImpactWelcome, StructureType.stSpyronStructureWelcome))
        End Function

        Public Function IsThankYouSurveyAvailable(fIsImpact As Boolean) As Boolean
            Return IsSurveyAvailable(StructureType.stSpyronStructureThankYou)
        End Function

        Public Overrides Function LoadProject() As Boolean
            If ProjectManager Is Nothing Then Return False

            Dim watch As Stopwatch = System.Diagnostics.Stopwatch.StartNew()
            watch.Stop()

            PrintDebugInfo("----- Load Project Started ----- ")
            Dim res As Boolean = LoadModelStructure()

            If Not res Then
                PrintDebugInfo("----- Load Project Failed ----- ")
                Return False
            End If

            PrintDebugInfo("LoadModeStructure done - ")


            Dim NeedToFixRatings As Boolean = ProjectManager.MeasureScales.FixRatingScales
            'If NeedToFixRatings Then ProjectManager.StorageManager.Writer.SaveProject(True)

            ProjectManager.LoadPipeParameters(PipeStorageType.pstStreamsDatabase, ModelID)
            ProjectManager.CacheManager.StructureLoaded(StructureType.stPipeOptions) = ProjectManager.PipeParameters.LoadTime
            PrintDebugInfo("LoadPipeParameters done - ")

            res = ProjectManager.Parameters.Load()
            PrintDebugInfo("LoadParameters done - ")

            res = LoadInfoDocs()
            PrintDebugInfo("LoadInfoDocs done - ")

            res = LoadDataMapping() 'AS/12323xp
            res = LoadAdvancedInfoDocs()
            PrintDebugInfo("LoadAdvancedInfoDocs done - ")

            res = LoadExtraAHPTables()
            PrintDebugInfo("LoadExtraAHPTables done - ")

            res = LoadGroupPermissions(Nothing)
            PrintDebugInfo("LoadGroupPermissions done - ")


            If ProjectManager.IsRiskProject AndAlso ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) AndAlso ProjectManager.Hierarchy(ECHierarchyID.hidImpact).HierarchyType <> ECHierarchyType.htModel Then
                ProjectManager.Hierarchy(ECHierarchyID.hidImpact).HierarchyID = ProjectManager.GetNextHierarchyID
            End If


            If ProjectManager.IsRiskProject AndAlso Not ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then
                ProjectManager.AddImpactHierarchy()
                ProjectManager.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeName = DEFAULT_NODENAME_GOAL_IMPACT
                ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).HierarchyID = -1
                ProjectManager.Hierarchy(ECHierarchyID.hidImpact).HierarchyID = ECHierarchyID.hidLikelihood
                ProjectManager.Hierarchy(-1).HierarchyID = ECHierarchyID.hidImpact
            End If

            Dim pwOutcomesScaleExists As Boolean = ProjectManager.MeasureScales.RatingsScales.Exists(Function(s) s.IsOutcomes)
            Dim expValuesScaleExists As Boolean = ProjectManager.MeasureScales.RatingsScales.Exists(Function(s) s.IsExpectedValues)
            Dim pwofPercentagesScaleExists As Boolean = ProjectManager.MeasureScales.RatingsScales.Exists(Function(s) s.IsPWofPercentages)

            If Not pwOutcomesScaleExists Then ProjectManager.MeasureScales.AddDefaultOutcomesScale()
            If Not expValuesScaleExists Then ProjectManager.MeasureScales.AddDefaultExpectedValuesScale()
            If Not pwofPercentagesScaleExists Then ProjectManager.MeasureScales.AddDefaultPWofPercentagesScale()

            ProjectManager.MeasureScales.FixScalesIsDefaultProperty()

            ProjectManager.VerifyObjectivesHierarchies()

            ProjectManager.Controls.ReadControls(ECModelStorageType.mstCanvasStreamDatabase, Location, ProviderType, ModelID)
            PrintDebugInfo("ReadControls done - ")

            LoadUserControlsPermissions()
            PrintDebugInfo("LoadUserControlsPermissions done - ")

            Dim LJT As DateTime
            LoadUserJudgmentsControls(LJT)
            For Each control As clsControl In ProjectManager.Controls.Controls
                For Each assignment As clsControlAssignment In control.Assignments
                    assignment.Value = ProjectManager.Controls.GetCombinedEffectivenessValue(assignment.Judgments, control.ID, assignment.Value)
                Next
            Next

            'ProjectManager.Regions.ReadRegions(ECModelStorageType.mstCanvasStreamDatabase, Location, ProviderType, ModelID)

            ProjectManager.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, Location, ProviderType, ModelID)
            PrintDebugInfo("LoadAttributes done - ")

            ProjectManager.ResourceAligner.Load(ECModelStorageType.mstCanvasStreamDatabase, Location, ProviderType, ModelID)
            PrintDebugInfo("LoadResourceAligner done - ")


            ProjectManager.Comments.LoadComments()
            PrintDebugInfo("LoadComments done - ")

            ProjectManager.Edges.Load()

            LoadEventsGroups()

            ProjectManager.BayesianCalculationManager.Load()

            Dim elapsedMs As Long = watch.ElapsedMilliseconds

            If ProjectManager.AddLog IsNot Nothing Then
                ProjectManager.AddLog("Project id=" + ProjectManager.StorageManager.ModelID.ToString + " loaded in " + elapsedMs.ToString + " ms")
            End If

            PrintDebugInfo("----- Load Project Completed ----- ")
            Return True
        End Function

        Public Function LoadInfoDocs() As Boolean
            LoadInfoDocsFromDB()
            Return LoadAdvancedInfoDocs()
        End Function

        Public Function LoadUserData(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Boolean
            LoadUserJudgments(User)
            LoadUserPermissions(User)
            LoadUserDisabledNodes(User)

            Dim LJT As DateTime
            LoadUserJudgmentsControls(LJT, User)
            If User IsNot Nothing Then
                LoadUserControlsPermissions(User.UserID)
            End If

            Dim userIDs As New List(Of Integer)
            If User Is Nothing Then
                For Each U As clsUser In ProjectManager.UsersList
                    userIDs.Add(U.UserID)
                Next
            Else
                userIDs.Add(User.UserID)
            End If
        End Function

        Public Function LoadDataMapping() As Boolean
            If Not ProjectManager.UseDataMapping Then Return False

            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = GetModelStructureStream(StructureType.stDataMapping, time)

            If MS.Length <> 0 Then
                Dim res As Boolean = LoadDataMapping_BinaryStream(MS)
                Return res
            Else
                Return False
            End If
        End Function

        Public Function LoadEventsGroups() As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = GetModelStructureStream(StructureType.stEventsGroups, time)

            If MS.Length <> 0 Then
                Dim res As Boolean = LoadEventsGroups_BinaryStream(MS)
                Return res
            Else
                Return False
            End If
        End Function

        Public Function StoredUserJudgmentsUpToDate(User As clsUser) As Boolean
            Return False
        End Function

        Public Function DataExistsForUsers(Optional HierarchyID As Integer = -1) As List(Of Integer)
            If ProjectManager Is Nothing Then Return Nothing

            Dim MS As New MemoryStream
            Dim res As New List(Of Integer)

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim UserID As Integer
                Dim User As clsUser

                oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))

                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    MS = New MemoryStream

                    UserID = dbReader("UserID")
                    User = ProjectManager.GetUserByID(UserID)

                    If User IsNot Nothing Then
                        MS = Nothing
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        If JudgmentsExist_BinaryStream(MS, User, HierarchyID) Then
                            res.Add(UserID)
                        End If
                    End If
                End While
                dbReader.Close()
            End Using
            Return res
        End Function

        Public Function DataExistsForUsersHashset(Optional HierarchyID As Integer = -1) As HashSet(Of Integer)
            If ProjectManager Is Nothing Then Return Nothing

            Dim MS As New MemoryStream
            Dim res As New HashSet(Of Integer)

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim UserID As Integer
                Dim User As clsUser

                oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))

                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    MS = New MemoryStream

                    UserID = dbReader("UserID")
                    User = ProjectManager.GetUserByID(UserID)

                    If User IsNot Nothing Then
                        LoadUserPermissions(User)
                        LoadUserDisabledNodes(User)

                        MS = Nothing
                        MS = New MemoryStream
                        Dim bw As BinaryWriter = New BinaryWriter(MS)

                        Dim bufferSize As Integer = dbReader("StreamSize") ' The size of the BLOB buffer.
                        Dim outbyte(bufferSize - 1) As Byte 'The BLOB byte() buffer to be filled by GetBytes.
                        Dim retval As Long ' The bytes returned from GetBytes.

                        retval = dbReader.GetBytes(CInt(dbReader.GetOrdinal("Stream")), 0, outbyte, 0, bufferSize)

                        bw.Write(outbyte)
                        bw.Flush()

                        'If JudgmentsExist_BinaryStream(MS, User, HierarchyID) Then
                        Dim dt As DateTime
                        If GetMadeJudgementsCount_BinaryStream(MS, User, dt, HierarchyID) > 0 Then
                            res.Add(UserID)
                        End If
                    End If
                End While
                dbReader.Close()
            End Using
            Return res
        End Function
    End Class
End Namespace
