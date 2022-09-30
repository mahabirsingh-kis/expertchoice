Imports ECCore
Imports ECCore.MiscFuncs
Imports System.Reflection
Imports System.IO
Imports Canvas

<Serializable()> Public MustInherit Class clsStorage
    Public Overridable Property StorageType As ECModelStorageType = ECModelStorageType.mstCanvasStreamDatabase
    Public Overridable Property ModelID As Integer = -1
    Public Overridable Property Location() As String
    Public Overridable Property ProviderType() As DBProviderType = DBProviderType.dbptSQLClient
    Public Overridable Property CanvasDBVersion() As ECCanvasDatabaseVersion
    Public Overridable Property ProjectManager() As clsProjectManager

    Private _ModelStuctureStreamIDs As New HashSet(Of StructureType)
    Public ReadOnly Property ModelStuctureStreamIDs As HashSet(Of StructureType)
        Get
            Return _ModelStuctureStreamIDs
        End Get
    End Property

    Private _UserDataStreamIDs As New HashSet(Of UserDataType)
    Public ReadOnly Property UserDataStreamIDs As HashSet(Of UserDataType)
        Get
            Return _UserDataStreamIDs
        End Get
    End Property

    Private Sub InitModelStructureStreamIDs()
        Dim items As Array = System.Enum.GetValues(GetType(StructureType))
        For Each item As Object In items
            ModelStuctureStreamIDs.Add(CType(item, StructureType))
        Next
    End Sub

    Private Sub InitUserDataStreamIDs()
        Dim items As Array = System.Enum.GetValues(GetType(UserDataType))
        For Each item As Object In items
            UserDataStreamIDs.Add(CType(item, UserDataType))
        Next
    End Sub

    Public Sub New()
        InitModelStructureStreamIDs()
        InitUserDataStreamIDs()
    End Sub
End Class

<Serializable()> Public Class clsStorageManager
    Inherits clsStorage

    Private mDBVersion As ECCanvasDatabaseVersion
    Public Property Reader() As StorageReaderStreams
    Public Property Writer() As StorageWriterStreams

    Public Property AHPReader() As New StorageReaderAHP
    Public Property AHPWriter() As New StorageWriterAHP

    Public Overrides Property ProviderType() As DBProviderType
        Get
            Return MyBase.ProviderType
        End Get
        Set(ByVal value As DBProviderType)
            MyBase.ProviderType = value
            If Reader IsNot Nothing Then Reader.ProviderType = value
            If Writer IsNot Nothing Then Writer.ProviderType = value

            If AHPReader IsNot Nothing Then AHPReader.ProviderType = value
            If AHPWriter IsNot Nothing Then AHPWriter.ProviderType = value
        End Set
    End Property

    Public Overrides Property ProjectManager() As clsProjectManager
        Get
            Return MyBase.ProjectManager
        End Get
        Set(ByVal value As clsProjectManager)
            MyBase.ProjectManager = value
            If Reader IsNot Nothing Then Reader.ProjectManager = value
            If Writer IsNot Nothing Then Writer.ProjectManager = value

            If AHPReader IsNot Nothing Then AHPReader.ProjectManager = value
            If AHPWriter IsNot Nothing Then AHPWriter.ProjectManager = value
        End Set
    End Property

    Public Overrides Property StorageType() As ECModelStorageType
        Get
            Return MyBase.StorageType
        End Get
        Set(ByVal value As ECModelStorageType)
            MyBase.StorageType = value
            If Reader IsNot Nothing Then Reader.StorageType = value
            If Writer IsNot Nothing Then Writer.StorageType = value

            If AHPReader IsNot Nothing Then AHPReader.StorageType = value
            If AHPWriter IsNot Nothing Then AHPWriter.StorageType = value
        End Set
    End Property

    Public Property ProjectLocation() As String
        Get
            Return MyBase.Location
        End Get
        Set(ByVal value As String)
            MyBase.Location = value
            If Reader IsNot Nothing Then Reader.Location = value
            If Writer IsNot Nothing Then Writer.Location = value

            If AHPReader IsNot Nothing Then AHPReader.Location = value
            If AHPWriter IsNot Nothing Then AHPWriter.Location = value
        End Set
    End Property

    Public Overrides Property ModelID() As Integer
        Get
            Return MyBase.ModelID
        End Get
        Set(ByVal value As Integer)
            MyBase.ModelID = value
            If Reader IsNot Nothing Then Reader.ModelID = value
            If Writer IsNot Nothing Then Writer.ModelID = value

            If AHPReader IsNot Nothing Then AHPReader.ModelID = value
            If AHPWriter IsNot Nothing Then AHPWriter.ModelID = value
        End Set
    End Property

    Public Overrides Property CanvasDBVersion() As ECCanvasDatabaseVersion
        Get
            Return mDBVersion
        End Get
        Set(ByVal value As ECCanvasDatabaseVersion)
            mDBVersion = value
            CreateReaderAndWriter()
        End Set
    End Property

    Public ReadOnly Property CurrentCanvasDBVersion() As ECCanvasDatabaseVersion
        Get
            Return GetCurrentDBVersion()
        End Get
    End Property

    Protected Function ReadDBVersion_CanvasStreamDatabase() As Boolean
        If (ProjectManager Is Nothing) OrElse Not CheckDBConnection(ProviderType, ProjectLocation) Then Return False
        mDBVersion = GetDBVersion_CanvasStreamDatabase(ProjectLocation, ProviderType, ModelID)
        Return mDBVersion IsNot Nothing
    End Function

    Protected Function ReadDBVersion_AHPSFile() As Boolean
        If ProjectManager Is Nothing Then Return False
        mDBVersion = GetDBVersion_AHPSFile(ProjectLocation)
        Return mDBVersion IsNot Nothing
    End Function

    ''' <summary>
    ''' Reads storage version from current location and puts it in CanvasDBVersion property.
    ''' Also creates appropriate reader and writer for this version.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReadDBVersion()
        Dim res As Boolean = False
        Select Case StorageType
            Case ECModelStorageType.mstCanvasStreamDatabase
                res = ReadDBVersion_CanvasStreamDatabase()
            Case ECModelStorageType.mstAHPSFile
                res = ReadDBVersion_AHPSFile()
        End Select

        If Not res Then mDBVersion = GetCurrentDBVersion()

        If mDBVersion.MinorVersion <= CurrentCanvasDBVersion.MinorVersion Then
            CreateReaderAndWriter()
        End If
    End Sub

    ''' <summary>
    ''' Creates reader and writer for current storage version
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CreateReaderAndWriter()
        Reader = Nothing
        Writer = Nothing

        Select Case StorageType
            Case ECModelStorageType.mstCanvasStreamDatabase, ECModelStorageType.mstAHPSFile, ECModelStorageType.mstAHPSStream
                Reader = New StorageReaderStreams
                Writer = New StorageWriterStreams
            Case ECModelStorageType.mstAHPDatabase
                ' Reader = 
                'Writer = New StorageWriterAHP()
        End Select

        If Reader IsNot Nothing Then
            Reader.ProjectManager = ProjectManager
            Reader.Location = ProjectLocation
            Reader.StorageType = StorageType
            Reader.CanvasDBVersion = CanvasDBVersion
            Reader.ProviderType = ProviderType
            Reader.ModelID = ModelID
            Reader.CreateStreamReader()
        End If

        If Writer IsNot Nothing Then
            Writer.ProjectManager = ProjectManager
            Writer.Location = ProjectLocation
            Writer.StorageType = StorageType
            Writer.CanvasDBVersion = CanvasDBVersion
            Writer.ProviderType = ProviderType
            Writer.ModelID = ModelID
            Writer.CreateStreamWriter()
        End If

        If AHPReader IsNot Nothing Then AHPReader.ProjectManager = ProjectManager
        If AHPWriter IsNot Nothing Then AHPWriter.ProjectManager = ProjectManager
    End Sub

    Public Function LoadModelStream_AHPSFile() As Boolean 'C0772
        If Not File.Exists(Location) Then
            Return False
        End If

        Dim FS As FileStream = New FileStream(Location, FileMode.Open, FileAccess.Read)

        FS.Seek(0, SeekOrigin.Begin)
        Dim BR As New BinaryReader(FS)

        Dim HeadChunk As Int32 = BR.ReadInt32
        If HeadChunk <> CHUNK_CANVAS_STREAMS_PROJECT Then
            Return False
        End If

        Dim chunk As Int32
        Dim chunkSize As Int32
        Dim UserID As Int32
        'Dim byteArray As Byte()
        Dim res As Boolean = True

        ProjectManager.AHPSFileManager.CloseProject()

        Dim NextChunkPosition As Integer

        While (BR.BaseStream.Position < BR.BaseStream.Length - 1) And res

            chunk = BR.ReadInt32()

            Select Case chunk
                Case CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION
                    chunkSize = BR.ReadInt32

                    Dim byteArray As Byte()
                    byteArray = BR.ReadBytes(chunkSize)

                    Dim strVersion As String = ByteArrayToString(byteArray)

                    Dim dbversion As New ECCanvasDatabaseVersion
                    If dbversion.ParseString(strVersion) Then
                        CanvasDBVersion = dbversion
                    End If
                Case CHUNK_CANVAS_STREAMS_PROJECT_MODEL_STRUCTURE
                    chunkSize = BR.ReadInt32

                    Dim byteArray As Byte()
                    byteArray = BR.ReadBytes(chunkSize)
                    BR.ReadUInt64()

                    Dim ModelReader As clsStreamModelReader = GetStreamModelReader(CanvasDBVersion)
                    Dim stream As New MemoryStream(byteArray)
                    ModelReader.BinaryStream = stream
                    ModelReader.ProjectManager = ProjectManager
                    Dim b As Boolean = ModelReader.ReadModelStructure()
                Case CHUNK_CANVAS_STREAMS_PROJECT_RESOURCE_ALIGNER_NEW
                    chunkSize = BR.ReadInt32

                    Dim byteArray As Byte()
                    byteArray = BR.ReadBytes(chunkSize)
                    BR.ReadUInt64()

                    Dim stream As New MemoryStream(byteArray)
                    Dim b As Boolean = ProjectManager.ResourceAligner.ParseRAStream(stream)
                    ProjectManager.ResourceAligner.Scenarios.ResourceAligner = ProjectManager.ResourceAligner
                Case CHUNK_CANVAS_STREAMS_PROJECT_PIPE_OPTIONS
                    chunkSize = BR.ReadInt32

                    Dim byteArray As Byte()
                    byteArray = BR.ReadBytes(chunkSize)
                    BR.ReadUInt64()

                    Dim stream As New MemoryStream(byteArray)

                    Dim PP As clsPipeParamaters = ProjectManager.PipeParameters
                    PP.ProjectVersion = CanvasDBVersion
                    Dim b As Boolean = PP.ReadFromStream(stream)
                    ' D4834 ===
                Case CHUNK_CANVAS_STREAMS_SNAPSHOTS_STREAM
                    chunkSize = BR.ReadInt32
                    NextChunkPosition = BR.BaseStream.Position + chunkSize
                    BR.BaseStream.Seek(NextChunkPosition, SeekOrigin.Begin)
                    ' D4834 ==
                Case Else
                    If chunk >= 70000 And chunk < 80000 Then
                        ' if model structure
                        chunkSize = BR.ReadInt32
                        NextChunkPosition = BR.BaseStream.Position + chunkSize
                        BR.BaseStream.Seek(NextChunkPosition, SeekOrigin.Begin)
                    Else
                        ' if user data
                        UserID = BR.ReadInt32()
                        chunkSize = BR.ReadInt32
                        NextChunkPosition = BR.BaseStream.Position + chunkSize
                        BR.BaseStream.Seek(NextChunkPosition, SeekOrigin.Begin)
                    End If
                    BR.ReadUInt64()
            End Select
        End While

        BR.Close()

        Return res
    End Function

    Public Sub New(ByVal ProjectManager As clsProjectManager)
        MyBase.ProjectManager = ProjectManager
        mDBVersion = CurrentCanvasDBVersion
        CreateReaderAndWriter()
    End Sub
End Class