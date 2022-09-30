Imports ECCore
Imports System.IO

<Serializable()> Public Class clsStreamChunk 'C0772
    Private mChunkID As Integer
    Private mUserID As Integer
    Private mStream As Byte()

    Public Property ChunkID() As Integer
        Get
            Return mChunkID
        End Get
        Set(ByVal value As Integer)
            mChunkID = value
        End Set
    End Property

    Public Property UserID() As Integer
        Get
            Return mUserID
        End Get
        Set(ByVal value As Integer)
            mUserID = value
        End Set
    End Property

    Public Property Stream() As Byte()
        Get
            Return mStream
        End Get
        Set(ByVal value As Byte())
            mStream = value
        End Set
    End Property

    Public Sub New(ByVal ChunkID As Integer, ByVal UserID As Integer, ByVal Stream As Byte())
        mChunkID = ChunkID
        mUserID = UserID
        mStream = Stream
    End Sub
End Class

<Serializable()> Public Class clsAHPSFileManager 'C0772
    Private mFullStream As MemoryStream

    Private mStreams As List(Of clsStreamChunk)

    Public Property FullStream() As MemoryStream
        Get
            Return mFullStream
        End Get
        Set(ByVal value As MemoryStream)
            mFullStream = value
        End Set
    End Property

    Public Property Streams() As List(Of clsStreamChunk)
        Get
            Return mStreams
        End Get
        Set(ByVal value As List(Of clsStreamChunk))
            mStreams = value
        End Set
    End Property

    Public Function GetStream(ByVal ChunkID As Integer, Optional ByVal UserID As Integer = UNDEFINED_USER_ID) As MemoryStream
        For Each S As clsStreamChunk In Streams
            If S.ChunkID = ChunkID And S.UserID = UserID Then
                Dim MS As New MemoryStream(S.Stream)
                Return MS
            End If
        Next
        Return Nothing
    End Function

    Public Function DeleteUser(ByVal UserID As Integer) As Boolean
        For i As Integer = mStreams.Count - 1 To 0 Step -1
            If mStreams(i).UserID = UserID Then
                mStreams.RemoveAt(i)
            End If
        Next

        Return True
    End Function

    Public Function AddStream(ByVal ChunkID As Integer, ByVal UserID As Integer, ByVal Stream As Byte()) As Boolean
        For Each S As clsStreamChunk In mStreams
            If S.ChunkID = ChunkID And S.UserID = UserID Then
                S.Stream = Stream
                Return True
            End If
        Next

        Dim NewStreamChunk As New clsStreamChunk(ChunkID, UserID, Stream)
        mStreams.Add(NewStreamChunk)
        Return True
    End Function

    Public Sub CloseProject()
        mStreams.Clear()
    End Sub

    Public Sub New()
        mStreams = New List(Of clsStreamChunk)
    End Sub
End Class