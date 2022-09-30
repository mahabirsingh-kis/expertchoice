Imports Canvas.CanvasTypes
Imports System.Data.Common 'C0236
Imports System.IO 'C0271

Namespace ECCore.TeamTimeFuncs
    Public Module TeamTimeFuncs

        Public Const UndefinedValue As Long = CLng(Integer.MinValue) * 1000    ' D1903 + D1909
        Public Const RatingsDirectPrefix As Char = "*"  ' D3553
        Public GPW_Mode_Allowed As Boolean = False    ' D2156 + D2173
        Public GPW_Mode_Default As GraphicalPairwiseMode = GraphicalPairwiseMode.gpwmLessThan9   ' D2148
        Public GPW_Mode_Strict As Boolean = False  ' D2148

        Public Function CombinePairwiseJudgments(ByVal pwJudgments As ArrayList, ByRef Advantage As Integer) As Double  'C0088 'C0384 + D1991
            If pwJudgments Is Nothing Then Return UndefinedValue

            Dim s As Double = 0
            Dim userCount As Integer = 0

            For Each MD As clsCustomMeasureData In pwJudgments
                If TypeOf MD Is clsPairwiseMeasureData Then
                    Dim pwData As clsPairwiseMeasureData = CType(MD, clsPairwiseMeasureData)
                    If Not pwData Is Nothing Then
                        If Not pwData.IsUndefined Then
                            If s = 0 Then
                                s = 1
                            End If

                            userCount = userCount + 1
                            If pwData.Advantage = 1 Then
                                s = s * pwData.Value
                            Else
                                s = s * 1 / pwData.Value
                            End If
                        End If
                    End If
                End If
            Next
            'C0490==

            If userCount <> 0 Then
                s = Math.Pow(s, 1 / userCount)
                'C0089===
                If s > 1 Then
                    Advantage = 1
                Else
                    Advantage = -1
                    s = 1 / s 'C0089
                End If
                'C0089==
            Else
                Advantage = 0 'C0089
                s = UndefinedValue  'C0089
            End If

            Return s
        End Function

        Public Function CombineNonPairwiseJudgments(ByVal nonpwJudgments As ArrayList) As Single 'C0088 'C0384
            If nonpwJudgments Is Nothing Then
                Return -1
            End If

            Dim s As Single = 0
            Dim userCount As Integer = 0
            Dim value As Single = -1    ' D1288

            For Each MD As clsCustomMeasureData In nonpwJudgments
                If TypeOf MD Is clsNonPairwiseMeasureData Then
                    Dim nonpwData As clsNonPairwiseMeasureData = CType(MD, clsNonPairwiseMeasureData)
                    If nonpwData IsNot Nothing Then
                        If Not nonpwData.IsUndefined Then
                            value = nonpwData.SingleValue ' GetSingleValue(child.NodeID, Node.NodeID, U.UserID)
                            s = s + value
                            userCount += 1
                        End If
                    End If
                End If
            Next
            'C0490==

            If userCount <> 0 Then
                s = s / userCount
            Else
                's = Single.NaN
                s = -1  ' D1288
            End If

            Return s
        End Function

        Private Function ReadCombinedResultsFromStream(ByVal Stream As MemoryStream, ByVal WRTNode As clsNode, ByVal LocalPriorities As Boolean, ByVal NodesList As List(Of clsNode), ByRef NodesPriorities As List(Of Single), ByRef IdealAlternativePriority As Single) As Boolean 'C0384
            Dim BR As New BinaryReader(Stream)
            BR.BaseStream.Seek(0, SeekOrigin.Begin)

            If BR.BaseStream.Position <= BR.BaseStream.Length - 1 Then
                BR.Close()
                Return False
            End If

            Dim WRTNodeID As Integer = BR.ReadInt32

            If WRTNodeID <> WRTNode.NodeID Then
                BR.Close()
                Return False
            Else
                NodesPriorities = New List(Of Single) 'C0384
                For i As Integer = 1 To NodesList.Count
                    NodesPriorities.Add(0.0)
                Next

                Dim count As Integer = BR.ReadInt32

                Dim nodeID As Integer
                Dim nodePriority As Single

                For i As Integer = 1 To count
                    nodeID = BR.ReadInt32
                    nodePriority = BR.ReadSingle

                    If nodeID = -1 Then
                        IdealAlternativePriority = nodePriority
                    Else
                        'Dim index As Integer = NodesList.IndexOf(WRTNode.Hierarchy.GetNodeByID(nodeID)) 'C0362
                        'C0362===
                        Dim index As Integer
                        If WRTNode.IsTerminalNode Or Not LocalPriorities Then
                            index = NodesList.IndexOf(WRTNode.Hierarchy.ProjectManager.AltsHierarchy(WRTNode.Hierarchy.ProjectManager.ActiveAltsHierarchy).GetNodeByID(nodeID))
                        Else
                            index = NodesList.IndexOf(WRTNode.Hierarchy.GetNodeByID(nodeID))
                        End If
                        'C0362==
                        If index <> -1 Then
                            NodesPriorities(index) = nodePriority
                        End If
                    End If
                Next
            End If

            BR.Close()
            Return True
        End Function

        Public Function ReadCombinedResultsFromCanvasStreamDatabase(ByVal ConnectionString As String, ByVal ModelID As Integer, ByVal ProviderType As DBProviderType, ByVal WRTNode As clsNode, ByVal NodesList As List(Of clsNode), ByVal LocalPriorities As Boolean, ByRef NodesPriorities As List(Of Single), ByRef IdealAlternativePriority As Single) As Boolean 'C0384
            If WRTNode Is Nothing OrElse NodesList Is Nothing OrElse NodesList.Count = 0 Then Return False

            Dim MS As New MemoryStream

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)
                dbConnection.Open()

                Dim ocommand As DbCommand = GetDBCommand(ProviderType)
                ocommand.Connection = dbConnection

                Dim dbreader As DbDataReader = Nothing

                ocommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                ocommand.Parameters.Clear()
                ocommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                ocommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stTeamTimeCombinedResults)))
                dbreader = DBExecuteReader(ProviderType, ocommand)

                If dbreader.HasRows Then
                    dbreader.Read()

                    Dim bufferSize As Integer = CInt(dbreader("StreamSize"))    ' The size of the BLOB buffer.
                    Dim outbyte(bufferSize - 1) As Byte  ' The BLOB byte() buffer to be filled by GetBytes.
                    Dim retval As Long                   ' The bytes returned from GetBytes.
                    Dim startIndex As Long = 0           ' The starting position in the BLOB output.

                    retval = dbreader.GetBytes(CInt(dbreader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)

                    Dim bw As BinaryWriter = New BinaryWriter(MS)

                    ' Continue reading and writing while there are bytes beyond the size of the buffer.
                    Do While retval = bufferSize
                        bw.Write(outbyte)
                        bw.Flush()

                        ' Reposition the start index to the end of the last buffer and fill the buffer.
                        startIndex += bufferSize
                        retval = dbreader.GetBytes(CInt(dbreader.GetOrdinal("Stream")), startIndex, outbyte, 0, bufferSize)
                    Loop

                    ' Write the remaining buffer.
                    bw.Write(outbyte, 0, retval)
                    bw.Flush()

                    'bw.Close()
                End If
                dbreader.Close()
            End Using

            Return ReadCombinedResultsFromStream(MS, WRTNode, LocalPriorities, NodesList, NodesPriorities, IdealAlternativePriority)
        End Function


        Public Function PrecalculatedCombinedResultsExistInCanvasStreamDatabase(ByVal ConnectionString As String, ByVal ModelID As Integer, ByVal ProviderType As DBProviderType) As Boolean 'C0352
            Dim count As Integer = 0
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, ConnectionString)
                dbConnection.Open()

                Dim ocommand As DbCommand = GetDBCommand(ProviderType)
                ocommand.Connection = dbConnection

                'Dim dbreader As DbDataReader = Nothing 'C0360

                ocommand.CommandText = "SELECT COUNT(*) FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                ocommand.Parameters.Clear()
                ocommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                ocommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stTeamTimeCombinedResults)))

                Dim obj As Object = DBExecuteScalar(ProviderType, ocommand)
                count = If(obj Is Nothing, 0, CType(obj, Integer))

                'dbreader.Close() 'C0360
            End Using

            Return count > 0
        End Function

    End Module
End Namespace
