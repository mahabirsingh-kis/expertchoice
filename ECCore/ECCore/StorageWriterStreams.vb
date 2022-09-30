Imports ECCore
Imports ECCore.MiscFuncs
Imports ECSecurity.ECSecurity
Imports System.Data.Common
Imports System.IO
Imports System.Text

Namespace ECCore
    <Serializable()> Public Class StorageWriterStreams
        Inherits clsStorageWriter

        Protected sWriter As clsStreamModelWriter
        Public ReadOnly Property StreamWriter() As clsStreamModelWriter
            Get
                If sWriter Is Nothing Then CreateStreamWriter
                Return sWriter
            End Get
        End Property

        Public Sub CreateStreamWriter()
            sWriter = GetStreamModelWriter(CanvasDBVersion)
        End Sub

        Public Function SaveJudgment(ByVal node As ECCore.clsNode, ByVal MeasureData As ECCore.clsCustomMeasureData) As Boolean
            Dim user As clsUser = ProjectManager.GetUserByID(MeasureData.UserID)
            SaveUserJudgments(user)
        End Function

        Public Function SaveModelStructureStream(StructureType As StructureType, MS As MemoryStream, Optional time As Nullable(Of DateTime) = Nothing) As Boolean
            If time Is Nothing Then time = Now

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim transaction As DbTransaction = Nothing

                Try
                    transaction = dbConnection.BeginTransaction
                    oCommand.Transaction = transaction

                    oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"

                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))

                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                    oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream, ModifyDate) VALUES (?, ?, ?, ?, ?)"
                    oCommand.Parameters.Clear()

                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)

                    transaction.Commit()
                Catch ex As Exception
                    If transaction IsNot Nothing Then transaction.Rollback()
                Finally
                    oCommand = Nothing
                    transaction.Dispose()
                    dbConnection.Close()
                End Try
            End Using

            Return True
        End Function

        Private Function SaveModelStructureOld() As Boolean
            Dim MS As New MemoryStream

            StreamWriter.ProjectManager = ProjectManager
            StreamWriter.BinaryStream = MS
            StreamWriter.WriteModelStructure()

            Dim time As Nullable(Of DateTime) = Now
            ProjectManager.LastModifyTime = time
            Dim res = SaveModelStructureStream(StructureType.stModelStructure, MS, time)
            Return res
        End Function

        Private Function SaveHasCompleteHierarchyStatus() As Boolean
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)
            If ProjectManager.HasCompleteHierarchy Then BW.Write(Encoding.Unicode.GetBytes("C")) ' D4747
            BW.Close()

            Dim res = SaveModelStructureStream(StructureType.stStatus, MS, ProjectManager.LastModifyTime)
            Return res
        End Function

        Public Function SaveModelStructure() As Boolean
            ' D4506 === // add return value
            Dim res As Boolean = SaveModelStructureOld()
            res = res Or SaveHasCompleteHierarchyStatus()
            ProjectManager.PipeBuilder.PipeCreated = False
            Return res
            ' D4506 ==
        End Function

        Public Function SaveDataMapping() As Boolean
            If Not ProjectManager.UseDataMapping Then Return False

            Dim MS As New MemoryStream

            StreamWriter.ProjectManager = ProjectManager
            StreamWriter.BinaryStream = MS
            StreamWriter.WriteDataMapping()

            Dim res = SaveModelStructureStream(StructureType.stDataMapping, MS)
            Return res
        End Function

        Public Function SaveEventsGroups() As Boolean
            Dim MS As New MemoryStream

            StreamWriter.ProjectManager = ProjectManager
            StreamWriter.BinaryStream = MS
            StreamWriter.WriteEventsGroups()

            Dim res = SaveModelStructureStream(StructureType.stEventsGroups, MS)
            Return res
        End Function

        Public Function SaveFullProjectStream(ByVal InputStream As Stream, Optional ByVal PerformDecryption As Boolean = False, Optional ByVal Password As String = DEFAULT_ENCRYPTION_PASSWORD, Optional ByRef SnapshotsStreamPosition As Integer = -1) As Boolean 'C0380 + D3892
            If Not CheckDBConnection(ProviderType, Location) Or (InputStream Is Nothing) Then
                Return False
            End If

            'Debug.Print("Upload project started: " + Now.ToString)

            PerformDecryption = _DEBUG_ENCRYPTION_ENABLED 'C0380

            'C0380===
            Dim decryptedStream As MemoryStream = Nothing

            If PerformDecryption Then
                Dim streamForDecryption As New MemoryStream
                InputStream.Seek(0, SeekOrigin.Begin)
                Dim decryptBR As New BinaryReader(InputStream)
                Dim encryptedBytes As Byte() = decryptBR.ReadBytes(InputStream.Length)
                Dim decryptedbytes As Byte() = Decrypt(encryptedBytes, Password)
                decryptedStream = New MemoryStream(decryptedbytes)
            End If

            Dim currentStream As Stream = If(PerformDecryption, decryptedStream, InputStream)

            If currentStream Is Nothing Then
                Return False
            End If
            'C0380==

            'C0380===
            'InputStream.Seek(0, SeekOrigin.Begin)
            'Dim BR As New BinaryReader(InputStream)
            currentStream.Seek(0, SeekOrigin.Begin)
            Dim BR As New BinaryReader(currentStream)
            'C0380==

            Dim HeadChunk As Int32 = BR.ReadInt32
            If HeadChunk <> CHUNK_CANVAS_STREAMS_PROJECT Then
                Return False
            End If

            Try
                Dim time As DateTime

                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    Dim affected As Integer

                    Dim chunk As Int32
                    Dim chunkSize As Int32
                    Dim UserID As Int32
                    Dim byteArray As Byte()
                    Dim res As Boolean = True

                    While (BR.BaseStream.Position < BR.BaseStream.Length - 1) And res
                        affected = 0 'C0379

                        chunk = BR.ReadInt32()

                        ' if this is a model structure stream
                        If ModelStuctureStreamIDs.Contains(CType(chunk - CHUNK_CANVAS_STREAMS_PROJECT_BASE, StructureType)) Then
                            chunkSize = BR.ReadInt32
                            byteArray = BR.ReadBytes(chunkSize)

                            If chunk <> CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION Then
                                time = DateTime.FromBinary(BR.ReadInt64)
                                If time = VERY_OLD_DATE Or time = UNDEFINED_DATE Or time.Year < 1900 Then time = Now
                            Else
                                time = Now
                            End If

                            If chunk = CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION Then
                                oCommand.CommandText = "UPDATE ModelStructure SET Stream=?, StreamSize=? WHERE ProjectID=? AND StructureType=?"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", byteArray))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", chunkSize))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(chunk - CHUNK_CANVAS_STREAMS_PROJECT_BASE)))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            End If

                            If affected = 0 Then
                                oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream, ModifyDate) VALUES (?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()

                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(chunk - CHUNK_CANVAS_STREAMS_PROJECT_BASE)))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", chunkSize))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", byteArray))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            End If
                        Else
                            ' if this is a user data stream
                            If UserDataStreamIDs.Contains(CType(chunk - CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE, UserDataType)) Then
                                UserID = BR.ReadInt32()
                                chunkSize = BR.ReadInt32()
                                byteArray = BR.ReadBytes(chunkSize)

                                time = DateTime.FromBinary(BR.ReadInt64)
                                If time = VERY_OLD_DATE Or time = UNDEFINED_DATE Then time = Now

                                oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) VALUES (?, ?, ?, ?, ?, ?)"
                                oCommand.Parameters.Clear()
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", UserID))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", chunk - CHUNK_CANVAS_STREAMS_PROJECT_USER_DATA_BASE))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", chunkSize))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", byteArray))
                                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", time))
                                affected = DBExecuteNonQuery(ProviderType, oCommand)
                            Else
                                ' otherwise just skiup unknown chunk
                                Dim S As Integer = BR.ReadInt32()
                                If chunk >= 80000 AndAlso chunk < 90000 Then S = BR.ReadInt32() + 8 ' read size since prev is UserID; +8 -- for skip datetime
                                If chunk = CHUNK_CANVAS_STREAMS_SNAPSHOTS_STREAM Then SnapshotsStreamPosition = BR.BaseStream.Position ' D3892
                                If BR.BaseStream.Position + S > BR.BaseStream.Length Then S = BR.BaseStream.Length - BR.BaseStream.Position ' for avoid to read outside the stream
                                BR.BaseStream.Seek(S, SeekOrigin.Current)
                            End If
                        End If
                    End While
                    BR.Close()

                    If decryptedStream IsNot Nothing Then
                        decryptedStream.Close()
                    End If

                    oCommand = Nothing
                End Using
            Finally
            End Try

            'Debug.Print("Upload project ended: " + Now.ToString)

            Return True
        End Function

        Public Function SaveUserJudgmentsControls(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing, Optional Time As Object = Nothing) As Boolean
            Dim success As Boolean = False

            While Not success
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim MS As MemoryStream

                    StreamWriter.ProjectManager = ProjectManager

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    Dim transaction As DbTransaction = Nothing

                    Try
                        transaction = dbConnection.BeginTransaction
                        oCommand.Transaction = transaction

                        Dim affected As Integer

                        If User Is Nothing Then
                            oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=?"
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgmentsControls)))
                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        End If

                        Dim users As New ArrayList
                        If User IsNot Nothing Then
                            users.Add(User)
                        Else
                            For Each U As clsUser In ProjectManager.UsersList
                                users.Add(U)
                            Next
                            For Each U As clsUser In ProjectManager.DataInstanceUsers
                                users.Add(U)
                            Next
                        End If

                        Dim WriteTime As DateTime = Now

                        For Each U As clsUser In users
                            If Not IsCombinedUserID(U.UserID) Then
                                U.LastJudgmentTime = WriteTime

                                MS = Nothing
                                MS = New MemoryStream

                                StreamWriter.BinaryStream = MS
                                Dim bHasJudgments As Boolean = StreamWriter.WriteUserJudgmentsControls(U, WriteTime)

                                If bHasJudgments Then
                                    Dim count As Integer
                                    If User IsNot Nothing Then
                                        oCommand.CommandText = "SELECT COUNT(*) FROM UserData WHERE ProjectID=? AND UserID=? AND DataType=?"
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgmentsControls)))

                                        Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                                        count = If(obj Is Nothing, 0, CType(obj, Integer))
                                    End If

                                    If (User Is Nothing) Or ((User IsNot Nothing) And (count = 0)) Then
                                        oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) VALUES (?, ?, ?, ?, ?, ?)" 'C0333
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtJudgmentsControls))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", If(Time Is Nothing, WriteTime, Time)))
                                    Else
                                        oCommand.CommandText = "UPDATE UserData SET StreamSize=?, Stream=?, ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", If(Time Is Nothing, WriteTime, Time)))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtJudgmentsControls))
                                    End If
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Else
                                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgmentsControls)))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                End If
                            End If
                        Next

                        transaction.Commit()

                        success = True
                    Catch ex As SqlClient.SqlException
                        If ex.Number = 1205 Then
                            If transaction IsNot Nothing Then
                                transaction.Rollback()
                            End If
                        End If
                    Finally
                        oCommand = Nothing
                        transaction.Dispose()
                        dbConnection.Close()
                    End Try
                End Using
            End While

            Return True
        End Function

        Public Function SaveUserControlsPermissions(UserID As Integer) As Boolean
            Dim U As clsUser = ProjectManager.GetUserByID(UserID)
            If U Is Nothing Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim MS As MemoryStream

                StreamWriter.ProjectManager = ProjectManager

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissionsControls)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", UserID))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                MS = Nothing
                MS = New MemoryStream

                StreamWriter.BinaryStream = MS
                StreamWriter.WriteControlsPermissions(UserID)

                oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream) VALUES (?, ?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtPermissionsControls))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            End Using

            Return True
        End Function

        Public Function SaveUserJudgments(UserID As Integer) As Boolean
            Return SaveUserJudgments(ProjectManager.GetUserByID(UserID))
        End Function

        Public Function SaveUserJudgments(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing, Optional Time As Object = Nothing) As Boolean 'C0661
            Dim success As Boolean = False 'C0662

            While Not success 'C0662
                Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                    dbConnection.Open()

                    Dim MS As MemoryStream

                    StreamWriter.ProjectManager = ProjectManager

                    Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                    oCommand.Connection = dbConnection

                    Dim transaction As DbTransaction = Nothing

                    Try
                        transaction = dbConnection.BeginTransaction
                        oCommand.Transaction = transaction

                        Dim affected As Integer

                        If User Is Nothing Then
                            oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=?"
                            oCommand.Parameters.Clear()
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                            oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                            affected = DBExecuteNonQuery(ProviderType, oCommand)
                        End If

                        Dim users As New ArrayList
                        If User IsNot Nothing Then
                            users.Add(User)
                        Else
                            For Each U As clsUser In ProjectManager.UsersList
                                users.Add(U)
                            Next
                            For Each U As clsUser In ProjectManager.DataInstanceUsers
                                users.Add(U)
                            Next
                        End If

                        Dim WriteTime As DateTime = Now 'C0333

                        For Each U As clsUser In users
                            If Not IsCombinedUserID(U.UserID) Then 'C0555
                                U.LastJudgmentTime = WriteTime 'C0333

                                MS = Nothing
                                MS = New MemoryStream

                                StreamWriter.BinaryStream = MS
                                Dim bHasJudgments As Boolean = StreamWriter.WriteUserJudgments(U, WriteTime)

                                If bHasJudgments Then 'C0412
                                    Dim count As Integer
                                    If User IsNot Nothing Then
                                        oCommand.CommandText = "SELECT COUNT(*) FROM UserData WHERE ProjectID=? AND UserID=? AND DataType=?"
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))

                                        Dim obj As Object = DBExecuteScalar(ProviderType, oCommand)
                                        count = If(obj Is Nothing, 0, CType(obj, Integer))
                                    End If

                                    If (User Is Nothing) Or ((User IsNot Nothing) And (count = 0)) Then
                                        oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) VALUES (?, ?, ?, ?, ?, ?)" 'C0333
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtJudgments))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", If(Time Is Nothing, WriteTime, Time))) 'C0333
                                    Else
                                        oCommand.CommandText = "UPDATE UserData SET StreamSize=?, Stream=?, ModifyDate=? WHERE ProjectID=? AND UserID=? AND DataType=?"
                                        oCommand.Parameters.Clear()
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", If(Time Is Nothing, WriteTime, Time))) 'C0333
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtJudgments))
                                    End If
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                Else
                                    'C1021===
                                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                                    oCommand.Parameters.Clear()
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                                    affected = DBExecuteNonQuery(ProviderType, oCommand)
                                    'C1021==
                                End If
                            End If
                        Next

                        transaction.Commit()

                        success = True 'C0662
                    Catch ex As SqlClient.SqlException 'C0662
                        If ex.Number = 1205 Then 'C0662
                            If transaction IsNot Nothing Then
                                transaction.Rollback()
                            End If
                        End If
                    Finally
                        oCommand = Nothing
                        transaction.Dispose()
                        dbConnection.Close()
                    End Try
                End Using
            End While 'C0662

            Return True
        End Function

        Public Function DeleteUserDisabledNodes(ByVal User As ECCore.ECTypes.clsUser) As Boolean 'C0450
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtDisabledNodes)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

            End Using

            Return True
        End Function

        Public Function SaveAHPExtraTables() As Boolean
            If ProjectManager.ExtraAHPTables Is Nothing Then Return True
            Dim res = SaveModelStructureStream(StructureType.stAHPExtraTables, ProjectManager.ExtraAHPTables)
            Return res
        End Function

        Public Function SaveVersion() As Boolean
            Dim MS As New MemoryStream(StringToByteArray(CanvasDBVersion.GetVersionString))
            Dim res = SaveModelStructureStream(StructureType.stModelVersion, MS)
            Return res
        End Function

        Public Function ClearTeamTimeData() As Boolean 'C0348
            'Debug.Print("ClearTeamTimeData")

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim affected As Integer

                oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtTeamTimeJudgment)))
                affected = DBExecuteNonQuery(ProviderType, oCommand)

                oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?" 'C0360
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stTeamTimeCombinedResults))) 'C0352 'C0360
                affected = DBExecuteNonQuery(ProviderType, oCommand)

            End Using

            Return True
        End Function

        Public Function DeleteUserJudgments(ByVal AUser As ECCore.ECTypes.clsUser) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtJudgments)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", AUser.UserID))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

            End Using

            Return True
        End Function

        Public Overloads Function DeleteUserData(ByVal AUser As ECCore.ECTypes.clsUser) As Boolean
            Dim affected As Integer
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", AUser.UserID))

                affected = DBExecuteNonQuery(ProviderType, oCommand)
            End Using

            Return affected > 0
        End Function

        Public Overloads Function DeleteUserData(users As List(Of Integer)) As Boolean
            If users Is Nothing OrElse users.Count = 0 Then Return False

            Dim BatchSize As Integer = 300
            Dim i As Integer = 0
            Dim n As Integer = users.Count
            While i < n
                Dim j As Integer = 0
                Dim UsersSet As String = ""
                While (j < BatchSize) AndAlso (i < n)
                    UsersSet += If(UsersSet = "", "", ",") + users(i).ToString
                    i += 1
                    j += 1
                End While
                If UsersSet <> "" Then
                    Dim affected As Integer
                    Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                        dbConnection.Open()

                        Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                        oCommand.Connection = dbConnection

                        oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND UserID IN (" + UsersSet + ")"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))

                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    End Using
                End If
            End While

            Return True
        End Function


        Public Function DeleteUserPermissions(ByVal AUser As ECCore.ECTypes.clsUser) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", AUser.UserID))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

            End Using

            Return True
        End Function

        Public Function SaveUserDisabledNodes(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim MS As MemoryStream

                StreamWriter.ProjectManager = ProjectManager

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                If User IsNot Nothing Then
                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                Else
                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=?"
                End If

                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtDisabledNodes)))

                If User IsNot Nothing Then
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                End If

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                Dim users As New List(Of clsUser)
                If User IsNot Nothing Then
                    users.Add(User)
                Else
                    users = ProjectManager.UsersList
                End If

                For Each U As clsUser In users
                    'If U.UserID <> COMBINED_USER_ID Then 'C0555
                    If Not IsCombinedUserID(U.UserID) Then 'C0555
                        MS = Nothing
                        MS = New MemoryStream

                        StreamWriter.BinaryStream = MS
                        StreamWriter.WriteUserDisabledNodes(U)

                        oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream) VALUES (?, ?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtDisabledNodes))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    End If
                Next

            End Using

            Return True
        End Function

        Public Function SaveUserPermissions(Optional ByVal User As ECCore.ECTypes.clsUser = Nothing) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim MS As MemoryStream

                StreamWriter.ProjectManager = ProjectManager

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                'oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=?" 'C0266 'C0279

                'C0279===
                If User IsNot Nothing Then
                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                Else
                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID >= 0"
                End If
                'C0279==

                'C0266===
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                'C0266==

                'C0279===
                If User IsNot Nothing Then
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                End If
                'C0279==

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                Dim users As New List(Of clsUser) 'C0384
                If User IsNot Nothing Then
                    users.Add(User)
                Else
                    users = ProjectManager.UsersList
                End If

                For Each U As clsUser In users
                    'If U.UserID <> COMBINED_USER_ID Then 'C0555
                    If Not IsCombinedUserID(U.UserID) Then 'C0555
                        MS = Nothing
                        MS = New MemoryStream

                        StreamWriter.BinaryStream = MS
                        StreamWriter.WriteUserPermissions(U)

                        'oCommand.CommandText = "INSERT INTO UserData (UserID, DataType, StreamSize, Stream) VALUES (?, ?, ?, ?)" 'C0266
                        oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream) VALUES (?, ?, ?, ?, ?)"
                        oCommand.Parameters.Clear()
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", U.UserID))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtPermissions))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                        oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                        affected = DBExecuteNonQuery(ProviderType, oCommand)
                    End If
                Next

            End Using

            Return True
        End Function

        Private Function SaveGroupPermissionsToDB(ByVal Group As clsCombinedGroup) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim MS As MemoryStream

                StreamWriter.ProjectManager = ProjectManager

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim transaction As DbTransaction = Nothing

                Try
                    transaction = dbConnection.BeginTransaction
                    oCommand.Transaction = transaction

                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"

                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtPermissions)))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", Group.CombinedUserID))

                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                    MS = Nothing
                    MS = New MemoryStream

                    StreamWriter.BinaryStream = MS
                    StreamWriter.WriteCombinedGroupPermissions(Group)

                    oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream) VALUES (?, ?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", Group.CombinedUserID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtPermissions))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)

                    transaction.Commit()
                Catch ex As Exception
                    If transaction IsNot Nothing Then transaction.Rollback()
                Finally
                    oCommand = Nothing
                    transaction.Dispose()
                    dbConnection.Close()
                End Try
            End Using

            Return True
        End Function

        Public Function SaveGroupPermissions(Optional ByVal Group As clsCombinedGroup = Nothing) As Boolean
            If ProjectManager Is Nothing Then Return False
            If Group IsNot Nothing Then
                Return SaveGroupPermissionsToDB(Group)
            Else
                For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                    SaveGroupPermissionsToDB(CG)
                Next
            End If

            Return False
        End Function

        Private Function SaveRegularInfoDocs() As Boolean
            Dim MS As New MemoryStream
            StreamWriter.ProjectManager = ProjectManager
            StreamWriter.BinaryStream = MS
            StreamWriter.WriteInfoDocs()

            Dim res = SaveModelStructureStream(StructureType.stInfoDocs, MS)
            Return res
        End Function

        Private Function SaveAdvancedInfoDocs() As Boolean
            Dim MS As New MemoryStream
            StreamWriter.ProjectManager = ProjectManager
            StreamWriter.BinaryStream = MS
            StreamWriter.WriteAdvancedInfoDocs()

            Dim res = SaveModelStructureStream(StructureType.stAdvancedInfoDocs, MS)
            Return res
        End Function

        Public Function SaveInfoDocs() As Boolean
            Return SaveRegularInfoDocs() AndAlso SaveAdvancedInfoDocs()
        End Function

        Public Function DeleteUser(UserEmail As String, Optional fSaveProject As Boolean = True) As Boolean
            With ProjectManager
                Dim U As clsUser = .GetUserByEMail(UserEmail)
                If U Is Nothing Then Return False
                DeleteUserData(U)
                .UsersList.Remove(U)
                If fSaveProject Then SaveProject(True)
            End With

            Return True
        End Function

        Public Overrides Function SaveProject(Optional ByVal StructureOnly As Boolean = False) As Boolean
            If ProjectManager Is Nothing Then Return False

            If ProjectManager.IsRiskProject AndAlso Not ProjectManager.IsValidHierarchyID(ECHierarchyID.hidImpact) Then
                ProjectManager.AddImpactHierarchy()
            End If

            SaveModelStructure()
            SaveDataMapping() 'AS/12323xp
            SaveVersion()
            SaveGroupPermissions()
            ProjectManager.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, Location, ProviderType, ModelID)
            SaveInfoDocs()
            SaveAHPExtraTables()
            SaveEventsGroups()

            If Not StructureOnly Then
                SaveUserJudgments()
                SaveUserPermissions()
                SaveUserDisabledNodes()
            End If

            ProjectManager.PipeBuilder.PipeCreated = False

            Return True
        End Function
    End Class
End Namespace