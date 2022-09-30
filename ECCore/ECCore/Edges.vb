Imports System.Web.Script.Serialization
Imports System.Linq
Imports System.IO

Namespace ECCore
    <Serializable()> Public Class Edge
        Public Property FromNode As clsNode
        Public Property FromHierarchy As clsHierarchy
        Public Property ToNode As clsNode
        Public Property ToHierarchy As clsHierarchy
        Public Property MeasurementType As ECMeasureType = ECMeasureType.mtDirect
        Public Property MeasurementScale As clsMeasurementScale = Nothing

        Public Sub New(FromNode As clsNode, FromHierarchy As clsHierarchy, ToNode As clsNode, ToHierarchy As clsHierarchy)
            Me.FromNode = FromNode
            Me.FromHierarchy = FromHierarchy
            Me.ToNode = ToNode
            Me.ToHierarchy = ToHierarchy
        End Sub
    End Class

    <Serializable()> Public Class Edges
        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub

        Public Property Edges As New Dictionary(Of Guid, List(Of Edge))
        Public ReadOnly Property ProjectManager As clsProjectManager

        Private mWhites As New HashSet(Of clsNode)
        Private mGreys As New HashSet(Of clsNode)
        Private mBlacks As New List(Of clsNode)
        Private mHasCycle As Boolean = False
        Private mStack As New Stack(Of clsNode)
        Private mCycleVertex As clsNode

        Public Sub AddEdge(edge As Edge)
            If Not Edges.ContainsKey(edge.FromNode.NodeGuidID) Then
                Edges.Add(edge.FromNode.NodeGuidID, New List(Of Edge))
                Edges(edge.FromNode.NodeGuidID).Add(edge)
            Else
                Dim e As Edge = Edges(edge.FromNode.NodeGuidID).FirstOrDefault(Function(x) (x.ToNode Is edge.ToNode) AndAlso (x.FromHierarchy Is edge.FromHierarchy) AndAlso (x.ToHierarchy Is edge.ToHierarchy))
                If e Is Nothing Then
                    Edges(edge.FromNode.NodeGuidID).Add(edge)
                Else
                    e.ToNode = edge.ToNode
                    e.FromHierarchy = edge.FromHierarchy
                    e.ToHierarchy = edge.ToHierarchy
                    e.MeasurementType = edge.MeasurementType
                    e.MeasurementScale = edge.MeasurementScale
                End If
            End If
        End Sub

        Public Sub AddEdge(FromNodeID As Guid, ToNodeID As Guid, Optional MeasurementType As ECMeasureType = ECMeasureType.mtDirect, Optional MeasurementScale As clsMeasurementScale = Nothing)
            Dim AH As clsHierarchy = ProjectManager.ActiveAlternatives
            Dim edge As New Edge(AH.GetNodeByID(FromNodeID), AH, AH.GetNodeByID(ToNodeID), AH)
            edge.MeasurementType = MeasurementType
            edge.MeasurementScale = MeasurementScale
            AddEdge(edge)
        End Sub

        Public Sub RemoveEdge(edge As Edge)
            If Edges.ContainsKey(edge.FromNode.NodeGuidID) Then
                Edges(edge.FromNode.NodeGuidID).RemoveAll(Function(x) (x.ToNode Is edge.ToNode) AndAlso (x.FromHierarchy Is edge.FromHierarchy) AndAlso (x.ToHierarchy Is edge.ToHierarchy))
            End If
        End Sub

        Public Sub RemoveEdge(FromNodeID As Guid, ToNodeID As Guid)
            Dim AH As clsHierarchy = ProjectManager.ActiveAlternatives
            Dim edge As New Edge(AH.GetNodeByID(FromNodeID), AH, AH.GetNodeByID(ToNodeID), AH)
            RemoveEdge(edge)
        End Sub

        Public Sub Save()
            WriteEdges(ProjectManager.StorageManager.StorageType, ProjectManager.StorageManager.Location, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)
        End Sub

        Private Function WriteEdgesStream_1_1_46() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Edges.Count)

            For Each kvp As KeyValuePair(Of Guid, List(Of Edge)) In Edges
                BW.Write(kvp.Key.ToByteArray)
                BW.Write(kvp.Value.Count)

                For Each edge As Edge In kvp.Value
                    BW.Write(edge.FromHierarchy.HierarchyGuidID.ToByteArray)
                    BW.Write(edge.FromNode.NodeGuidID.ToByteArray)
                    BW.Write(edge.ToHierarchy.HierarchyGuidID.ToByteArray)
                    BW.Write(edge.ToNode.NodeGuidID.ToByteArray)
                    BW.Write(CInt(edge.MeasurementType))
                    If edge.MeasurementScale Is Nothing Then
                        BW.Write(Guid.Empty.ToByteArray)
                    Else
                        BW.Write(edge.MeasurementScale.GuidID.ToByteArray)
                    End If
                Next
            Next

            BW.Close()

            Return MS
        End Function

        Private Function WriteEdgesStream() As MemoryStream
            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 46
                    Return WriteEdgesStream_1_1_46()
            End Select

            Return Nothing
        End Function

        Protected Function WriteEdges_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim MS As MemoryStream = WriteEdgesStream()
            Dim res = ProjectManager.StorageManager.Writer.SaveModelStructureStream(StructureType.stEdges, MS)
            Return res
        End Function

        Public Function WriteEdges(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    WriteEdges_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select
            Return True
        End Function

        Private Function ReadEdgesStream_1_1_46(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Edges.Clear()

            Dim count As Integer = BR.ReadInt32

            Dim FromHierarchyID As Guid
            Dim FromNodeID As Guid
            Dim ToHierarchyID As Guid
            Dim ToNodeID As Guid
            Dim MT As ECMeasureType
            Dim Scale As clsMeasurementScale

            For i As Integer = 1 To count
                FromNodeID = New Guid(BR.ReadBytes(16))
                Dim list As New List(Of Edge)
                Edges.Add(FromNodeID, list)
                Dim n As Integer = BR.ReadInt32
                For j As Integer = 1 To n
                    FromHierarchyID = New Guid(BR.ReadBytes(16))
                    FromNodeID = New Guid(BR.ReadBytes(16))
                    ToHierarchyID = New Guid(BR.ReadBytes(16))
                    ToNodeID = New Guid(BR.ReadBytes(16))
                    MT = CType(BR.ReadInt32, ECMeasureType)
                    Dim g As Guid = New Guid(BR.ReadBytes(16))
                    If g = Guid.Empty Then
                        Scale = Nothing
                    Else
                        Scale = ProjectManager.MeasureScales.GetScaleByID(g)
                    End If
                    Dim FromH As clsHierarchy = ProjectManager.GetAnyHierarchyByID(FromHierarchyID)
                    Dim FromNode As clsNode = If(FromH Is Nothing, Nothing, FromH.GetNodeByID(FromNodeID))
                    Dim ToH As clsHierarchy = ProjectManager.GetAnyHierarchyByID(ToHierarchyID)
                    Dim ToNode As clsNode = If(ToH Is Nothing, Nothing, ToH.GetNodeByID(ToNodeID))
                    If FromNode IsNot Nothing AndAlso ToNode IsNot Nothing Then
                        Dim edge As New Edge(FromNode, FromH, ToNode, ToH)
                        edge.MeasurementType = MT
                        edge.MeasurementScale = Scale
                        list.Add(edge)
                    End If
                Next
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadEdgesStream(ByVal Stream As MemoryStream) As Boolean
            If Stream Is Nothing OrElse Stream.Length = 0 Then Return False

            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 46
                    ReadEdgesStream_1_1_46(Stream)
            End Select
        End Function

        Private Function ReadEdges_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = ProjectManager.StorageManager.Reader.GetModelStructureStream(StructureType.stEdges, time, Location, ProviderType, ModelID)
            Dim res As Boolean = ReadEdgesStream(MS)
            If res Then ProjectManager.CacheManager.StructureLoaded(StructureType.stEdges) = time
            Return res
        End Function
        Public Function ReadEdges(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    ReadEdges_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select

            Return True
        End Function

        Public Sub Load()
            ReadEdges(ProjectManager.StorageManager.StorageType, ProjectManager.StorageManager.Location, ProjectManager.StorageManager.ProviderType, ProjectManager.StorageManager.ModelID)
        End Sub

        Private Function SortStep(alt As clsNode) As Boolean
            If mHasCycle Then Return False
            If mBlacks.Contains(alt) Then Return True
            If mGreys.Contains(alt) Then
                mHasCycle = True
                mCycleVertex = alt
                Return False ' cycle detected
            End If
            If mWhites.Contains(alt) Then
                mWhites.Remove(alt)
                mGreys.Add(alt)
                mStack.Push(alt)
                If Edges.ContainsKey(alt.NodeGuidID) Then
                    For Each edge As Edge In Edges(alt.NodeGuidID)
                        If Not mHasCycle Then
                            mHasCycle = Not SortStep(edge.ToNode)
                        End If
                    Next
                End If
                If Not mHasCycle Then
                    mGreys.Remove(alt)
                    mBlacks.Insert(0, alt)
                    mStack.Pop()
                End If
                Return Not mHasCycle
            End If
        End Function

        Public Function TopologicalSort(ByRef result As List(Of clsNode), ByRef output As String) As Boolean
            Dim alts As List(Of clsNode) = ProjectManager.ActiveAlternatives.TerminalNodes
            result = New List(Of clsNode)
            mHasCycle = False

            mWhites.Clear()
            mGreys.Clear()
            mBlacks.Clear()
            mStack.Clear()
            mCycleVertex = Nothing

            For Each alt As clsNode In alts
                mWhites.Add(alt)
            Next

            For Each alt As clsNode In alts
                If Not mHasCycle AndAlso Not SortStep(alt) Then
                    mHasCycle = True
                End If
            Next

            Dim sortedOrder As String = ""
            Dim cycleString As String = ""
            If Not mHasCycle Then
                For Each alt As clsNode In mBlacks
                    result.Add(alt)
                    sortedOrder += If(sortedOrder = "", alt.NodeName, " -> " + alt.NodeName)
                Next
                Debug.Print("Sorted order: " + sortedOrder)
                output = sortedOrder
            Else
                Dim cycle As New List(Of clsNode)
                Dim b As Boolean = False
                While Not b And mStack.Count > 0
                    cycle.Insert(0, mStack.Pop)
                    b = cycle(0) Is mCycleVertex
                End While

                For Each alt As clsNode In cycle
                    cycleString += If(cycleString = "", alt.NodeName, " -> " + alt.NodeName)
                Next
                If cycle.Count > 0 Then
                    cycleString += " -> " + cycle(0).NodeName
                End If
                Debug.Print("Cycle: " + cycleString)
                output = cycleString
            End If
            Return mHasCycle
        End Function

    End Class
End Namespace