Imports ECCore
Imports Canvas
Imports System.IO

<Serializable()> Public Class clsProjectConverter
    Private mSourceFilePath As String
    Private mSourceFileFormat As ECModelStorageType

    Private mDestFilePath As String
    Private mDestFileFormat As ECModelStorageType

    <NonSerializedAttribute()> Private mWorker As ComponentModel.BackgroundWorker = Nothing

    'Public Event UserPrioritiesProgress(ByVal Progress As Integer, ByVal CurrentTask As String)

    Private mDefaultAHPFilePath As String
    Private mDefaultAHPXFilePath As String

    Public Property SourceFilePath() As String
        Get
            Return mSourceFilePath
        End Get
        Set(ByVal value As String)
            mSourceFilePath = value
        End Set
    End Property

    Public Property SourceFileFormat() As ECModelStorageType
        Get
            Return mSourceFileFormat
        End Get
        Set(ByVal value As ECModelStorageType)
            mSourceFileFormat = value
        End Set
    End Property

    Public Property DestinationFilePath() As String
        Get
            Return mDestFilePath
        End Get
        Set(ByVal value As String)
            mDestFilePath = value
        End Set
    End Property

    Public Property DestinationFileFormat() As ECModelStorageType
        Get
            Return mDestFileFormat
        End Get
        Set(ByVal value As ECModelStorageType)
            mDestFileFormat = value
        End Set
    End Property

    Public Property DefaultAHPFilePath() As String
        Get
            Return mDefaultAHPFilePath
        End Get
        Set(ByVal value As String)
            mDefaultAHPFilePath = value
        End Set
    End Property

    Public Property DefaultAHPXFilePath() As String
        Get
            Return mDefaultAHPXFilePath
        End Get
        Set(ByVal value As String)
            mDefaultAHPXFilePath = value
        End Set
    End Property

    Public Property Worker() As System.ComponentModel.BackgroundWorker
        Get
            Return mWorker
        End Get
        Set(ByVal value As System.ComponentModel.BackgroundWorker)
            mWorker = value
        End Set
    End Property

    Public Function Convert() As Boolean
        Dim ProjectManager As New clsProjectManager(,,)

        Dim origSourceFilePath As String = SourceFilePath
        Dim origDestFilePath As String = DestinationFilePath

        Select Case DestinationFileFormat
            Case ECModelStorageType.mstAHPDatabase
                If IO.File.Exists(DefaultAHPFilePath) Then
                    IO.File.Copy(DefaultAHPFilePath, DestinationFilePath, True)
                Else
                    Return False
                End If
                DestinationFilePath = "Driver={Microsoft Access Driver (*.mdb)};Dbq=" + DestinationFilePath + ";Uid=Admin;Pwd=;"
            Case ECModelStorageType.mstCanvasDatabase
                If IO.File.Exists(DefaultAHPXFilePath) Then
                    IO.File.Copy(DefaultAHPXFilePath, DestinationFilePath, True)
                Else
                    Return False
                End If
                DestinationFilePath = "Driver={Microsoft Access Driver (*.mdb)};Dbq=" + DestinationFilePath + ";Uid=Admin;Pwd=;"
        End Select

        Select Case SourceFileFormat
            Case ECModelStorageType.mstAHPDatabase, ECModelStorageType.mstCanvasDatabase
                SourceFilePath = "Driver={Microsoft Access Driver (*.mdb)};Dbq=" + SourceFilePath + ";Uid=Admin;Pwd=;"
        End Select


        ProjectManager.StorageManager.ProviderType = DBProviderType.dbptODBC 'for ahp/ahpx

        ProjectManager.StorageManager.StorageType = SourceFileFormat
        ProjectManager.StorageManager.ProjectLocation = SourceFilePath


        If Worker IsNot Nothing Then
            Worker.ReportProgress(-1, "Task 1/2: Loading model from source")
            Worker.ReportProgress(0, "Loading model from: " + origSourceFilePath)
        End If

        ProjectManager.StorageManager.Reader.LoadProject()

        ProjectManager.StorageManager.StorageType = DestinationFileFormat
        ProjectManager.StorageManager.ProjectLocation = DestinationFilePath

        If Worker IsNot Nothing Then
            Worker.ReportProgress(-1, "Task 2/2: Saving model to destination")
            Worker.ReportProgress(0, "Saving model to: " + origDestFilePath)
        End If

        Dim currentStep As Integer
        Dim totalSteps As Integer

        If ProjectManager.StorageManager.StorageType = ECModelStorageType.mstAHPSFile Then
            If ProjectManager.AHPSFileManager.Streams.Count = 0 Then
                ' Step 1: Writing model version
                ' Step 2: Writing model structure
                ' Step 3: Writing info docs
                ' Step 4 (usersCount * 3): users judgments, permissions, disabled nodes
                ' Step 5: Writing full stream to file
                ' Total steps: 4 + 4* usersCount

                currentStep = 0
                totalSteps = 4 + 4 * ProjectManager.UsersList.Count

                Worker.ReportProgress(currentStep / totalSteps * 100, "Creating model version stream")
                currentStep += 1
                ProjectManager.AHPSFileManager.AddStream(CHUNK_CANVAS_STREAMS_PROJECT_MODEL_VERSION, UNDEFINED_USER_ID, StringToByteArray(ProjectManager.StorageManager.CanvasDBVersion.GetVersionString))

                Dim MS As MemoryStream

                Worker.ReportProgress(currentStep / totalSteps * 100, "Creating model structure stream")
                currentStep += 1

                MS = New MemoryStream
                ProjectManager.StorageManager.Writer.StreamWriter.ProjectManager = ProjectManager
                ProjectManager.StorageManager.Writer.StreamWriter.BinaryStream = MS
                ProjectManager.StorageManager.Writer.StreamWriter.WriteModelStructure()
                ProjectManager.AHPSFileManager.AddStream(CHUNK_CANVAS_STREAMS_PROJECT_MODEL_STRUCTURE, UNDEFINED_USER_ID, MS.ToArray)

                Worker.ReportProgress(currentStep / totalSteps * 100, "Creating info docs stream")
                currentStep += 1

                MS = New MemoryStream
                ProjectManager.StorageManager.Writer.StreamWriter.ProjectManager = ProjectManager
                ProjectManager.StorageManager.Writer.StreamWriter.BinaryStream = MS
                ProjectManager.StorageManager.Writer.StreamWriter.WriteInfoDocs()
                ProjectManager.AHPSFileManager.AddStream(CHUNK_CANVAS_STREAMS_PROJECT_INFO_DOCS, UNDEFINED_USER_ID, MS.ToArray)

                For Each user As clsUser In ProjectManager.UsersList
                    Worker.ReportProgress(currentStep / totalSteps * 100, "Creating judgments stream for user: " + user.UserName + " (" + user.UserEMail + ")")
                    currentStep += 1

                    MS = New MemoryStream
                    ProjectManager.StorageManager.Writer.StreamWriter.ProjectManager = ProjectManager
                    ProjectManager.StorageManager.Writer.StreamWriter.BinaryStream = MS
                    ProjectManager.StorageManager.Writer.StreamWriter.WriteUserJudgments(user, Now)
                    ProjectManager.AHPSFileManager.AddStream(CHUNK_CANVAS_STREAMS_PROJECT_USER_JUDGMENTS, user.UserID, MS.ToArray)

                    Worker.ReportProgress(currentStep / totalSteps * 100, "Creating permissions stream for user: " + user.UserName + " (" + user.UserEMail + ")")
                    currentStep += 1

                    MS = New MemoryStream
                    ProjectManager.StorageManager.Writer.StreamWriter.ProjectManager = ProjectManager
                    ProjectManager.StorageManager.Writer.StreamWriter.BinaryStream = MS
                    ProjectManager.StorageManager.Writer.StreamWriter.WriteUserPermissions(user)
                    ProjectManager.AHPSFileManager.AddStream(CHUNK_CANVAS_STREAMS_PROJECT_USER_PERMISSIONS, user.UserID, MS.ToArray)

                    Worker.ReportProgress(currentStep / totalSteps * 100, "Creating disabled nodes stream for user: " + user.UserName + " (" + user.UserEMail + ")")
                    currentStep += 1

                    MS = New MemoryStream
                    ProjectManager.StorageManager.Writer.StreamWriter.ProjectManager = ProjectManager
                    ProjectManager.StorageManager.Writer.StreamWriter.BinaryStream = MS
                    ProjectManager.StorageManager.Writer.StreamWriter.WriteUserDisabledNodes(user)
                    ProjectManager.AHPSFileManager.AddStream(CHUNK_CANVAS_STREAMS_PROJECT_USER_DISABLED_NODES, user.UserID, MS.ToArray)
                Next
            End If
            Worker.ReportProgress(currentStep / totalSteps * 100, "Saving project to AHPS file: ")
            currentStep += 1
        End If

        ProjectManager.StorageManager.Writer.SaveProject(False)

        If ProjectManager.StorageManager.StorageType = ECModelStorageType.mstAHPSFile Then
            Worker.ReportProgress(100, "Done")
        End If

        Worker.ReportProgress(-1, "Completed")
    End Function

End Class
