Imports ECCore
Imports GenericDBAccess
Imports System.Data.Common
Imports System.IO
Imports System.Linq

Namespace ECCore

    ' D4067 ===
    Public Enum CommentStatus   ' must be as bits
        None = 0
        Edited = 1
        Deleted = 2
        BOT = 4
    End Enum
    ' D4067 ==

    <Serializable> Public Class Comment
        Public Property ID As Guid = New Guid
        Public Property UserID As Integer
        Public Property Text As String = 0
        Public Property Time As DateTime = VERY_OLD_DATE
        Public Property ResponseToID As Guid = Guid.Empty
        Public Property Status As Integer = 0

        Public Property ParentNodeID As Guid = Guid.Empty
        Public Property FirstNodeID As Guid = Guid.Empty
        Public Property SecondNodeID As Guid = Guid.Empty
    End Class

    <Serializable> Public Class Comments
        Public Property Comments As New List(Of Comment)
        Public Property User As clsUser

        Public Sub New(aUser As clsUser)
            User = aUser
        End Sub

        Public Function AddComment(Text As String, Time As DateTime, ParentNodeID As Guid, FirstNodeID As Guid, Optional SecondNodeID As Nullable(Of Guid) = Nothing) As Comment
            If SecondNodeID Is Nothing Then SecondNodeID = Guid.Empty

            Dim comment As New Comment
            comment.ID = Guid.NewGuid    ' D4010
            comment.Text = Text
            comment.Time = Time
            comment.ParentNodeID = ParentNodeID
            comment.FirstNodeID = FirstNodeID
            comment.SecondNodeID = SecondNodeID
            comment.UserID = User.UserID
            Comments.Add(comment)

            Return comment
        End Function

        Public Function GetStream() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Comments.Count)
            For Each comment As Comment In Comments
                BW.Write(comment.ID.ToByteArray)
                BW.Write(comment.UserID)
                BW.Write(comment.Text)
                BW.Write(comment.Time.ToBinary)
                BW.Write(comment.ResponseToID.ToByteArray)
                BW.Write(comment.Status)
                BW.Write(comment.ParentNodeID.ToByteArray)
                BW.Write(comment.FirstNodeID.ToByteArray)
                BW.Write(comment.SecondNodeID.ToByteArray)
            Next

            BW.Close()
            Return MS
        End Function

        Public Function ParseStream(Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)
            Dim BR As New BinaryReader(Stream)

            Comments.Clear()

            Dim commentsCount As Integer = BR.ReadInt32
            For i As Integer = 1 To commentsCount
                Dim comment As New Comment
                comment.ID = New Guid(BR.ReadBytes(16))
                comment.UserID = BR.ReadInt32
                comment.Text = BR.ReadString
                comment.Time = DateTime.FromBinary(BR.ReadInt64)
                comment.ResponseToID = New Guid(BR.ReadBytes(16))
                comment.Status = BR.ReadInt32
                comment.ParentNodeID = New Guid(BR.ReadBytes(16))
                comment.FirstNodeID = New Guid(BR.ReadBytes(16))
                comment.SecondNodeID = New Guid(BR.ReadBytes(16))
                If comment.ID.Equals(Guid.Empty) Then comment.ID = Guid.NewGuid ' D4010
                Comments.Add(comment)
            Next

            BR.Close()
            Return True
        End Function

        Public Function Save_CanvasStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim MS As MemoryStream = GetStream()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection
                Dim transaction As DbTransaction = Nothing

                Try
                    transaction = dbConnection.BeginTransaction
                    oCommand.Transaction = transaction

                    oCommand.CommandText = "DELETE FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtComments)))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                    Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                    Dim WriteTime As DateTime = Now

                    oCommand.CommandText = "INSERT INTO UserData (ProjectID, UserID, DataType, StreamSize, Stream, ModifyDate) VALUES (?, ?, ?, ?, ?, ?)"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", UserDataType.udtComments))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                    oCommand.Parameters.Add(GetDBParameter(ProviderType, "ModifyDate", WriteTime))
                    affected = DBExecuteNonQuery(ProviderType, oCommand)

                    transaction.Commit()
                Catch ex As Exception
                    If transaction IsNot Nothing Then
                        transaction.Rollback()
                    End If
                Finally
                    oCommand = Nothing
                    transaction.Dispose()
                    dbConnection.Close()
                End Try
            End Using

            Return True
        End Function

        Public Function Load_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Integer
            If User Is Nothing Then Return False
            Dim res As Boolean = False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=? AND UserID=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "DataType", CInt(UserDataType.udtComments)))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "UserID", User.UserID))
                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    Dim MS As New MemoryStream

                    Dim bufferSize As Integer = dbReader("StreamSize")      ' The size of the BLOB buffer.
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

                    res = ParseStream(MS)
                End While
                dbReader.Close()
            End Using

            Return res
        End Function
    End Class

    <Serializable> Public Class UsersComments
        Public Property UsersComments As New Dictionary(Of clsUser, Comments)
        Public ReadOnly ProjectManager As clsProjectManager

        Public Function LoadComments(Optional User As clsUser = Nothing) As Boolean
            If User Is Nothing Then Return LoadAllComments()
            If Not UsersComments.ContainsKey(User) Then UsersComments.Add(User, New Comments(User))
            UsersComments(User).Load_CanvasStreamDatabase(ProjectManager.StorageManager.ProjectLocation, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)
            If UsersComments(User).Comments.Count = 0 Then UsersComments.Remove(User)
            Return True
        End Function

        Private Function LoadAllComments() As Boolean
            With ProjectManager.StorageManager
                Using dbConnection As DbConnection = GetDBConnection(.ProviderType, .Location)
                    dbConnection.Open()

                    Dim oCommand As DbCommand = GetDBCommand(.ProviderType)
                    oCommand.Connection = dbConnection

                    Dim dbReader As DbDataReader

                    oCommand.CommandText = "SELECT * FROM UserData WHERE ProjectID=? AND DataType=?"
                    oCommand.Parameters.Clear()
                    oCommand.Parameters.Add(GetDBParameter(.ProviderType, "ProjectID", .ModelID))
                    oCommand.Parameters.Add(GetDBParameter(.ProviderType, "DataType", CInt(UserDataType.udtComments)))
                    dbReader = DBExecuteReader(.ProviderType, oCommand)

                    While dbReader.Read
                        Dim UserID As Integer = dbReader("UserID")
                        Dim u As clsUser = ProjectManager.GetUserByID(UserID)
                        If u IsNot Nothing Then
                            Dim MS As New MemoryStream

                            Dim bufferSize As Integer = dbReader("StreamSize")      ' The size of the BLOB buffer.
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

                            If Not UsersComments.ContainsKey(u) Then UsersComments.Add(u, New Comments(u))
                            UsersComments(u).ParseStream(MS)
                            If UsersComments(u).Comments.Count = 0 Then UsersComments.Remove(u)
                        End If

                    End While
                    dbReader.Close()
                End Using
            End With

            Return True
        End Function

        ' D4041 ===
        Public Function SaveComments(Optional User As clsUser = Nothing) As Boolean
            ' D4265 ===
            Dim fResult As Boolean = True

            Dim usersList As New List(Of clsUser)
            If User Is Nothing Then
                usersList = ProjectManager.UsersList
            Else
                usersList.Add(User)
            End If

            For Each u As clsUser In usersList
                If UsersComments.ContainsKey(u) AndAlso UsersComments(u).Comments.Count > 0 Then fResult = fResult AndAlso UsersComments(u).Save_CanvasStreamsDatabase(ProjectManager.StorageManager.ProjectLocation, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)
            Next
            Return fResult
            ' D4265 ==
        End Function
        ' D4041 ==

        Public Function GetComments(ParentNodeID As Guid, FirstNodeID As Guid, Optional SecondNodeID As Nullable(Of Guid) = Nothing) As List(Of Comment)
            If SecondNodeID Is Nothing Then SecondNodeID = Guid.Empty
            Dim res As New List(Of Comment)
            For Each userComments As Comments In UsersComments.Values
                For Each comment As Comment In userComments.Comments
                    If comment.ParentNodeID.Equals(ParentNodeID) AndAlso comment.FirstNodeID.Equals(FirstNodeID) AndAlso comment.SecondNodeID.Equals(SecondNodeID) Then
                        res.Add(comment)
                    End If
                Next
            Next
            Return res
        End Function

        Public Function AddComment(Text As String, Time As DateTime, User As clsUser, ParentNodeID As Guid, FirstNodeID As Guid, Optional SecondNodeID As Nullable(Of Guid) = Nothing, Optional sReplyID As String = "") As Comment
            If SecondNodeID Is Nothing Then SecondNodeID = Guid.Empty
            If Not UsersComments.ContainsKey(User) Then UsersComments.Add(User, New Comments(User))
            Dim res As Comment = UsersComments(User).AddComment(Text, Time, ParentNodeID, FirstNodeID, SecondNodeID)
            If Not String.IsNullOrEmpty(sReplyID) Then res.ResponseToID = New Guid(sReplyID) ' D4010
            SaveComments(User)  ' D4041
            Return res
        End Function

        Public Function DeleteComment(User As clsUser, CommentID As Guid) As Boolean
            If Not UsersComments.ContainsKey(User) Then Return False
            ' D4041 ===
            Dim Cnt As Integer = UsersComments(User).Comments.RemoveAll(Function(p) (p.ID.Equals(CommentID)))
            If Cnt > 0 Then SaveComments(User)
            Return Cnt > 0
            ' D4041 ==
        End Function

        Public Sub New(aProjectManager As clsProjectManager)
            ProjectManager = aProjectManager
        End Sub

    End Class

End Namespace

