Imports ECCore
Imports System.Linq
Imports System.IO
Imports System.Data.Common

Namespace ECCore
    <Serializable> Public Enum BayesianMode
        bmProbability = 0
        bmOdds = 1
    End Enum

    <Serializable> Public Class BayesianDataSource
        Public Property EventGuid As Guid = Guid.Empty
        Public Property EventIndex As String = ""
        Public Property PriorOdds As Double = UNDEFINED_INTEGER_VALUE
        Public Property Prior As Double = UNDEFINED_INTEGER_VALUE
        Public Property SourceOfPriorGuid As Guid = Guid.Empty
        Public Property Probability_E As Double = UNDEFINED_INTEGER_VALUE
        Public Property Probability_E_Comment As String = "" 'TODO save to stream
        Public Property Probability_Not_E As Double = UNDEFINED_INTEGER_VALUE
        Public Property Probability_Not_E_Comment As String = "" 'TODO save to stream
        Public Property LR As Double = UNDEFINED_INTEGER_VALUE
        Public Property DataGuid As Guid = Guid.Empty
        Public Property DataName As String = "" 'TODO save to stream
        Public Property DataComment As String = ""
        Public Property Prior_E_Odds As Double = UNDEFINED_INTEGER_VALUE
        Public Property Posterior_E_Odds As Double = UNDEFINED_INTEGER_VALUE
        Public Property Posterior_E As Double = UNDEFINED_INTEGER_VALUE
    End Class

    Public Class BayesPageDataSource
        Public Property id As String
        Public Property alt_idx As Integer
        Public Property alt_event_type As Integer
        Public Property name As String
        Public Property prior_odds As Double
        Public Property prior As Double
        Public Property source_prior As String
        Public Property prob_e As Double
        Public Property prob_e_comment As String
        Public Property prob_not_e As Double
        Public Property prob_not_e_comment As String
        Public Property lr As Double
        Public Property data_guid As String
        Public Property data_name As String
        Public Property data_comment As String
        Public Property actions As String
        Public Property prior_e_odds As Double
        Public Property post_e_odds As Double
        Public Property post_e As Double
    End Class

    <Serializable> Public Class BayesianUpdating

        Private _ProjectManager As clsProjectManager = Nothing
        Public ReadOnly Property ProjectManager As clsProjectManager
            Get
                Return _ProjectManager
            End Get
        End Property

        Public Property BayesData As New List(Of BayesianDataSource)
        Public Property Mode As BayesianMode = BayesianMode.bmProbability
        Public Property CalculateInBothModes As Boolean = True
        Public Property UseUpdateGlobally As Boolean = False

        Public Sub Calculate() 'A1722
            Dim EventGuid As Guid = Guid.Empty
            Dim PrevEvent As BayesianDataSource = Nothing
            Dim Alt As clsNode = Nothing

            For Each Data As BayesianDataSource In BayesData
                If EventGuid = Guid.Empty Then EventGuid = Data.EventGuid

                If Alt Is Nothing OrElse Data.EventGuid <> EventGuid Then Alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Data.EventGuid)

                If Data.EventGuid <> EventGuid Then
                    PrevEvent = Nothing
                    EventGuid = Data.EventGuid
                End If


                If PrevEvent IsNot Nothing Then
                    Data.Prior = PrevEvent.Posterior_E
                Else
                    Data.Prior = Alt.UnnormalizedPriorityBeforeBayes
                End If

                Data.Posterior_E = UNDEFINED_INTEGER_VALUE

                If Data.Probability_E <> UNDEFINED_INTEGER_VALUE AndAlso Data.Probability_Not_E <> UNDEFINED_INTEGER_VALUE Then
                    Dim B As Double = Data.Probability_E * Data.Prior + Data.Probability_Not_E * (1 - Data.Prior)
                    If B <> 0 Then
                        Data.Posterior_E = (Data.Probability_E * Data.Prior) / B
                    End If
                End If

                PrevEvent = Data
            Next

            For Each Data As BayesianDataSource In BayesData
                Alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Data.EventGuid)
                If Alt IsNot Nothing Then
                    Alt.UnnormalizedPriority = Data.Posterior_E
                End If
            Next
        End Sub

        Private Function Load_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = ProjectManager.StorageManager.Reader.GetModelStructureStream(StructureType.stBayesianData, time, Location, ProviderType, ModelID)
            Dim res As Boolean = ParseStream(MS)
            If res Then ProjectManager.CacheManager.StructureLoaded(StructureType.stBayesianData) = time
            Return res
        End Function

        Private Function ParseStream_1_1_46(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32

            BayesData.Clear()

            For i As Integer = 1 To count
                Dim ds As New BayesianDataSource

                ds.DataGuid = New Guid(BR.ReadBytes(16))
                ds.DataName = BR.ReadString
                ds.DataComment = BR.ReadString
                ds.EventGuid = New Guid(BR.ReadBytes(16))
                ds.EventIndex = BR.ReadInt32
                ds.LR = BR.ReadDouble
                ds.Prior = BR.ReadDouble
                ds.Prior_E_Odds = BR.ReadDouble
                ds.Probability_E = BR.ReadDouble
                ds.Probability_E_Comment = BR.ReadString
                ds.Probability_Not_E = BR.ReadDouble
                ds.Probability_Not_E_Comment = BR.ReadString
                ds.SourceOfPriorGuid = New Guid(BR.ReadBytes(16))

                BayesData.Add(ds)
            Next

            BR.Close()

            Return True
        End Function

        Private Function ParseStream(ByVal Stream As MemoryStream) As Boolean
            If Stream Is Nothing OrElse Stream.Length = 0 Then Return False

            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 46 To _DB_VERSION_MINOR
                    ParseStream_1_1_46(Stream)
            End Select
        End Function

        Public Function Load() As Boolean
            BayesData.Clear()

            With ProjectManager.StorageManager
                Select Case ProjectManager.StorageManager.StorageType
                    Case ECModelStorageType.mstCanvasStreamDatabase
                        Load_CanvasStreamDatabase(.Location, .ProviderType, .ModelID)
                End Select
            End With

            Return True
        End Function


        Public Function Save() As Boolean
            With ProjectManager.StorageManager
                Select Case .StorageType
                    Case ECModelStorageType.mstCanvasStreamDatabase
                        Return Save_CanvasStreamsDatabase(.ProjectLocation, .ProviderType, .ModelID)
                End Select
            End With
            Return False
        End Function

        Public Function Save(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    Return Save_CanvasStreamsDatabase(Location, ProviderType, ModelID)
            End Select
            Return False
        End Function

        Private Function GetStream() As MemoryStream
            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 46 To _DB_VERSION_MINOR
                    Return GetStream_1_1_46()
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function GetStream_1_1_46() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(BayesData.Count)
            For Each ds As BayesianDataSource In BayesData
                BW.Write(ds.DataGuid.ToByteArray)
                BW.Write(ds.DataName)
                BW.Write(ds.DataComment)
                BW.Write(ds.EventGuid.ToByteArray)
                BW.Write(ds.EventIndex)
                BW.Write(ds.LR)
                BW.Write(ds.Prior)
                BW.Write(ds.Prior_E_Odds)
                BW.Write(ds.Probability_E)
                BW.Write(ds.Probability_E_Comment)
                BW.Write(ds.Probability_Not_E)
                BW.Write(ds.Probability_Not_E_Comment)
                BW.Write(ds.SourceOfPriorGuid.ToByteArray)
            Next

            BW.Close()
            Return MS
        End Function


        Private Function Save_CanvasStreamsDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim MS As MemoryStream = GetStream()

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"

                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stBayesianData))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stBayesianData))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                affected = DBExecuteNonQuery(ProviderType, oCommand)
            End Using

            Return True
        End Function

        Public Sub New(ProjectManager As clsProjectManager)
            Me._ProjectManager = ProjectManager
        End Sub
    End Class

End Namespace
