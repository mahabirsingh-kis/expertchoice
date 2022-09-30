Imports ECCore
Imports System.IO
Imports System.Data.Common
Imports System.Linq
Imports Canvas

Namespace ECCore
    <Serializable()> _
    Public Enum ControlType
        ctCause = 0
        ctConsequence = 1
        ctEvent = 2
        ctCauseToEvent = 3
        ctConsequenceToEvent = 4
        ctUndefined = 5
    End Enum

    <Serializable()> _
    Public Class clsRegions
        Public Property RowCount As Integer
        Public Property ColumnCount As Integer
        Public Cells As New List(Of Long)

        Private Function ReadRegionsStream(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            RowCount = BR.ReadInt32
            ColumnCount = BR.ReadInt32
            Dim cellsCount As Integer = BR.ReadInt32
            Dim cell As Long

            Cells.Clear()
            For i As Integer = 1 To cellsCount
                cell = BR.ReadInt64
                Cells.Add(cell)
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadRegions_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            If Not CheckDBConnection(ProviderType, Location) Then Return False

            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                Dim dbReader As DbDataReader

                Dim MS As New MemoryStream

                oCommand.CommandText = "SELECT * FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stRegions)))

                dbReader = DBExecuteReader(ProviderType, oCommand)

                While dbReader.Read
                    MS = Nothing
                    MS = New MemoryStream

                    Dim bufferSize As Integer = CInt(dbReader("StreamSize"))      ' The size of the BLOB buffer.
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

                    ReadRegionsStream(MS)
                End While
                dbReader.Close()
            End Using
            Return True
        End Function

        Public Function ReadRegions(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Cells.Clear()

            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    ReadRegions_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select

            Return True
        End Function

        Private Function WriteRegionsStream() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(RowCount)
            BW.Write(ColumnCount)
            BW.Write(Cells.Count)

            For Each cell As Long In Cells
                BW.Write(cell)
            Next

            BW.Close()

            Return MS
        End Function

        Protected Function WriteRegions_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Using dbConnection As DbConnection = GetDBConnection(ProviderType, Location)
                dbConnection.Open()

                Dim MS As MemoryStream

                Dim oCommand As DbCommand = GetDBCommand(ProviderType)
                oCommand.Connection = dbConnection

                oCommand.CommandText = "DELETE FROM ModelStructure WHERE ProjectID=? AND StructureType=?"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", CInt(StructureType.stRegions)))

                Dim affected As Integer = DBExecuteNonQuery(ProviderType, oCommand)

                MS = Nothing
                MS = New MemoryStream

                MS = WriteRegionsStream()

                oCommand.CommandText = "INSERT INTO ModelStructure (ProjectID, StructureType, StreamSize, Stream) VALUES (?, ?, ?, ?)"
                oCommand.Parameters.Clear()
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "ProjectID", ModelID))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StructureType", StructureType.stRegions))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "StreamSize", MS.ToArray.Length))
                oCommand.Parameters.Add(GetDBParameter(ProviderType, "Stream", MS.ToArray))
                affected = DBExecuteNonQuery(ProviderType, oCommand)

            End Using

            Return True
        End Function

        Public Function WriteRegions(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    WriteRegions_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select
            Return True
        End Function
    End Class


    <Serializable()> _
    Public Class clsControlAssignment
        Private mMeasurementType As ECMeasureType

        Public Property MeasurementType As ECMeasureType
            Get
                If mMeasurementType = ECMeasureType.mtPWOutcomes Then
                    mMeasurementType = ECMeasureType.mtDirect
                End If
                Return mMeasurementType
            End Get
            Set(value As ECMeasureType)
                If value = ECMeasureType.mtPWOutcomes Then
                    mMeasurementType = ECMeasureType.mtDirect
                Else
                    mMeasurementType = value
                End If
            End Set
        End Property

        Public Property MeasurementScaleID As Integer
        Public Property MeasurementScaleGuid As Guid
        Public Property IsActive As Boolean
        Public Property Judgments As clsNonPairwiseJudgments
        Public Property ID As Guid
        Public Property Comment As String = ""
        Public Overloads Property Value As Double = 0
        Public Overloads ReadOnly Property Value(UserID As Integer) As Double
            Get
                If UserID = COMBINED_USER_ID Then
                    Return Value
                Else
                    If Judgments IsNot Nothing Then
                        Dim J As clsNonPairwiseMeasureData = Judgments.GetJudgement(ObjectiveID, EventID, UserID)
                        If J IsNot Nothing AndAlso Not J.IsUndefined Then
                            Return J.SingleValue
                        Else
                            Return -1
                        End If
                    End If
                End If
            End Get
        End Property

        Public Property ObjectiveID As Guid
        Public Property EventID As Guid

        Public Sub New(Control As clsControl)
            MeasurementType = ECMeasureType.mtDirect
            Judgments = New clsNonPairwiseJudgments(Nothing, Control.ProjectManager)
        End Sub
    End Class

    <Serializable()>
    Public Class clsControl
        Public Property IsDecreasing As Boolean = True
        Private mCost As Double
        Private mMust As Boolean
        Public Property Must As Boolean
            Get
                If ProjectManager Is Nothing OrElse ProjectManager.ResourceAlignerRisk Is Nothing Then Return False
                Dim raAlt As RAAlternative = ProjectManager.ResourceAlignerRisk.Scenarios.ActiveScenario.GetAvailableAlternativeById(Me.ID.ToString.ToLower)
                If raAlt Is Nothing Then
                    Return False
                Else
                    Return raAlt.Must
                End If
            End Get
            Set(value As Boolean)
                If ProjectManager IsNot Nothing AndAlso ProjectManager.ResourceAlignerRisk IsNot Nothing Then
                    Dim raAlt As RAAlternative = ProjectManager.ResourceAlignerRisk.Scenarios.ActiveScenario.GetAvailableAlternativeById(Me.ID.ToString.ToLower)
                    If raAlt IsNot Nothing Then
                        raAlt.Must = value
                    End If
                End If
            End Set
        End Property

        Private mMustNot As Boolean
        Public Property MustNot As Boolean
            Get
                If ProjectManager Is Nothing OrElse ProjectManager.ResourceAlignerRisk Is Nothing Then Return False
                Dim raAlt As RAAlternative = ProjectManager.ResourceAlignerRisk.Scenarios.ActiveScenario.GetAvailableAlternativeById(Me.ID.ToString.ToLower)
                If raAlt Is Nothing Then
                    Return False
                Else
                    Return raAlt.MustNot
                End If
            End Get
            Set(value As Boolean)
                If ProjectManager IsNot Nothing AndAlso ProjectManager.ResourceAlignerRisk IsNot Nothing Then
                    Dim raAlt As RAAlternative = ProjectManager.ResourceAlignerRisk.Scenarios.ActiveScenario.GetAvailableAlternativeById(Me.ID.ToString.ToLower)
                    If raAlt IsNot Nothing Then
                        raAlt.MustNot = value
                    End If
                End If
            End Set
        End Property

        Public Property ProjectManager As clsProjectManager
        ''' <summary>
        ''' TmpMust is used for temporarily storing the Must value for Efficient Frontier
        ''' </summary>
        <NonSerialized> Public TmpMust As Boolean
        ''' <summary>
        ''' TmpMustNot is used for temporarily storing the MustNot value for Efficient Frontier
        ''' </summary>
        <NonSerialized> Public TmpMustNot As Boolean

        Private mActive As Boolean = False
        Public Property Active As Boolean
            Get
                Return mActive
                'If Not Enabled Then Return False
                'For Each ctrlAssignment As clsControlAssignment In Assignments
                '    If ctrlAssignment.IsActive Then Return True
                'Next
                'Return False
            End Get
            Set(value As Boolean)
                mActive = value
                For Each ctrlAssignment As clsControlAssignment In Assignments
                    SetAssignmentActive(ctrlAssignment.ID, value)
                Next
            End Set
        End Property

        Public Property ActiveOriginal As Boolean = False

        Public Property Enabled As Boolean = True

        Friend VarID As Integer

        ''' <summary>
        ''' OBSOLETE
        ''' </summary>
        ''' <returns></returns>
        Public Property Effectiveness As Double

        Public Property RiskReduction As Double

        Public Property SARiskReduction As Double = UNDEFINED_INTEGER_VALUE
        Public Property SARiskReductionMonetary As Double = UNDEFINED_INTEGER_VALUE

        Public Property Type As ControlType

        Public Property ID As Guid

        Public Property Name As String = ""

        Public Property InfoDoc As String = ""

        Public Property Cost As Double
            Get
                Return ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CONTROL_COST_ID, ID)
            End Get
            Set(value As Double)
                ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_CONTROL_COST_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, value, ID, Guid.Empty)
            End Set
        End Property

        Public ReadOnly Property IsCostDefined As Boolean
            Get
                Dim tCost As Double = Cost
                Return tCost <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE AndAlso tCost <> UNDEFINED_INTEGER_VALUE
            End Get
        End Property

        Public Property Categories As Object 'A0885
            Get
                Return ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CONTROL_CATEGORY_ID, ID)
            End Get
            Set(value As Object)
                ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_CONTROL_CATEGORY_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumerationMulti, value, ID, Guid.Empty)
            End Set
        End Property

        Public Property Assignments As New List(Of clsControlAssignment)

        Public Function GetAssignmentByID(AssignmentID As Guid) As clsControlAssignment
            Return Assignments.FirstOrDefault(Function(a) (a.ID.Equals(AssignmentID)))
        End Function

        Public Function GetAssignment(ObjID As Guid, EventID As Guid) As clsControlAssignment
            Return Assignments.FirstOrDefault(Function(a) (a.ObjectiveID.Equals(ObjID) AndAlso a.EventID.Equals(EventID)))
        End Function

        Public Function SetAssignmentValue(AssignmentID As Guid, Value As Double) As Boolean
            Dim assignment As clsControlAssignment = GetAssignmentByID(AssignmentID)
            If assignment IsNot Nothing Then
                assignment.Value = Value
                Return True
            Else
                Return False
            End If
        End Function

        Public Function SetAssignmentActive(AssignmentID As Guid, Value As Boolean) As Boolean
            Dim assignment As clsControlAssignment = GetAssignmentByID(AssignmentID)
            If assignment IsNot Nothing Then
                assignment.IsActive = Value
                Return True
            Else
                Return False
            End If
        End Function

        Public Function SetAssignmentComment(AssignmentID As Guid, Comment As String) As Boolean
            Dim assignment As clsControlAssignment = GetAssignmentByID(AssignmentID)
            If assignment IsNot Nothing Then
                assignment.Comment = Comment
                Return True 'A0722
            Else
                Return False 'A0722
            End If
        End Function

        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub
    End Class

    <Serializable()> _
    Public Class clsControls
        Public Property Options As New Canvas.RASettings

        Public Property Controls As New List(Of clsControl)

        Public ReadOnly Property DefinedControls As List(Of clsControl) 'A1373 + A1383
            Get
                Return Controls.Where(Function(ctrl) ctrl.Type <> ControlType.ctUndefined).ToList
            End Get
        End Property
        
        'A1392 ==
        Public ReadOnly Property EnabledControls As List(Of clsControl)
            Get
                Return DefinedControls.Where(Function(ctrl) ctrl.Enabled And ctrl.Type <> ControlType.ctUndefined).ToList
            End Get
        End Property
        'A1392 ===

        Public Function CostOfFundedControls() As Double
            Return CDbl(ProjectManager.Controls.EnabledControls.Sum(Function(ctrl) If(ctrl.Active AndAlso ctrl.IsCostDefined, ctrl.Cost, 0)))
        End Function

        Public Property ProjectManager As clsProjectManager

        Public ReadOnly Property GetControlID(tControl As clsControl) As Integer 'A1259
            Get
                Return Controls.FindAll(Function(ctrl) ctrl.Type = tControl.Type).IndexOf(tControl) + 1
            End Get
        End Property

        Public Function GetCombinedEffectivenessValue(Judgments As clsNonPairwiseJudgments, ControlID As Guid, Optional defaultValue As Double = 0) As Double
            Dim WeightsSum As Double = 0

            Dim uCount As Integer = 0
            Dim value As Double = 0

            Dim useWeights As Boolean = ProjectManager.CalculationsManager.UseUserWeights
            Dim aData As New List(Of AggregatedData)

            For Each J As clsNonPairwiseMeasureData In Judgments.JudgmentsFromAllUsers
                If Not J.IsUndefined Then
                    If ProjectManager.ControlsRoles.IsAllowedObjective(ControlID, J.UserID) Then
                        If useWeights Then
                            aData.Add(New AggregatedData(J.SingleValue, ProjectManager.GetUserByID(J.UserID).Weight))
                        Else
                            value += J.SingleValue
                        End If
                        uCount += 1
                    End If
                End If
            Next
            If uCount >= 1 Then
                If useWeights Then
                    Dim sum As Double = aData.Sum(Function(w) (w.Weight))
                    If sum <> 0 Then
                        For Each ad As AggregatedData In aData
                            value += ad.Value * ad.Weight / sum
                        Next
                    End If
                Else
                    value = value / uCount
                End If
            Else
                value = defaultValue
            End If

            Return value
        End Function

        Public Overloads Function AddControl(ID As Guid, Name As String, Type As ControlType, Optional InfoDoc As String = Nothing) As clsControl 'A0640 'A0820 + D4344
            Dim newControl As New clsControl(ProjectManager)
            newControl.ID = ID
            newControl.Name = Name
            If InfoDoc IsNot Nothing Then newControl.InfoDoc = InfoDoc 'A0640 + D4344
            newControl.Type = Type
            Controls.Add(newControl)
            Return newControl
        End Function

        Public Function GetControlByName(Name As String) As clsControl
            Return Controls.FirstOrDefault(Function(c) (c.Name.ToLower = Name.ToLower))
        End Function

        Private Function GetControlsOfType(ControlType As ControlType, ObjectiveID As Guid, EventID As Guid, Optional OnlyActive As Boolean = False, Optional OnlyEnabled As Boolean = True) As List(Of clsControl) 'A1392
            Dim res As New List(Of clsControl)
            For Each control As clsControl In Controls
                If control.Type = ControlType AndAlso (Not OnlyEnabled OrElse control.Enabled) Then 'A1392
                    For Each assignment As clsControlAssignment In control.Assignments
                        If (Not OnlyActive OrElse (OnlyActive AndAlso assignment.IsActive)) Then
                            If assignment.ObjectiveID.Equals(ObjectiveID) AndAlso assignment.EventID.Equals(EventID) Then
                                res.Add(control)
                                Exit For
                            Else
                                If ControlType = ControlType.ctCause Then
                                    Dim CovObj As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(ObjectiveID)
                                    Dim controlSource As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(assignment.ObjectiveID)
                                    'TODO: check wrt calculations
                                    If ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).IsChildOf(CovObj, controlSource) Then
                                        res.Add(control)
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                        If (Not OnlyActive OrElse (OnlyActive AndAlso assignment.IsActive)) AndAlso assignment.ObjectiveID.Equals(ObjectiveID) AndAlso assignment.EventID.Equals(EventID) Then
                            res.Add(control)
                            Exit For
                        End If
                    Next
                End If
            Next
            Return res
        End Function

        Public Function GetControlsForSource(SourceID As Guid, Optional OnlyActive As Boolean = False, Optional OnlyEnabled As Boolean = True) As List(Of clsControl) 'A1392
            Return GetControlsOfType(ControlType.ctCause, SourceID, Guid.Empty, OnlyActive, OnlyEnabled) 'A1392
        End Function

        Public Function GetControlsForVulnerabilities(SourceID As Guid, EventID As Guid, Optional OnlyActive As Boolean = False, Optional OnlyEnabled As Boolean = True) As List(Of clsControl) 'A1392
            Return GetControlsOfType(ControlType.ctCauseToEvent, SourceID, EventID, OnlyActive, OnlyEnabled) 'A1392
        End Function

        Public Function GetControlsForConsequences(ObjectiveID As Guid, EventID As Guid, Optional OnlyActive As Boolean = False, Optional OnlyEnabled As Boolean = True) As List(Of clsControl) 'A1392
            Return GetControlsOfType(ControlType.ctConsequenceToEvent, ObjectiveID, EventID, OnlyActive, OnlyEnabled) 'A1392
        End Function

        Public Function GetControlByID(ControlID As Guid) As clsControl
            Return Controls.FirstOrDefault(Function(c) (c.ID.Equals(ControlID)))
        End Function

        Public Function GetControlByVarID(ControlVarID As Integer) As clsControl
            Return Controls.FirstOrDefault(Function(c) (c.VarID = ControlVarID))
        End Function

        Public Overloads Function AddControlAssignment(ControlID As Guid, ObjectID As Guid) As clsControlAssignment
            Dim control As clsControl = GetControlByID(ControlID)
            If control Is Nothing Then Return Nothing

            Dim newControlAssignment As New clsControlAssignment(control)
            newControlAssignment.ID = Guid.NewGuid()
            newControlAssignment.ObjectiveID = ObjectID
            newControlAssignment.IsActive = True
            Dim a As clsControlAssignment = control.GetAssignmentByID(newControlAssignment.ID)
            If a Is Nothing Then
                For Each assignment As clsControlAssignment In control.Assignments
                    If assignment.ObjectiveID.Equals(ObjectID) Then
                        a = assignment
                    End If
                Next
            End If
            If a Is Nothing Then
                control.Assignments.Add(newControlAssignment)
                Return newControlAssignment
            Else
                Return a
            End If
        End Function

        Public Overloads Function AddControlAssignment(ControlID As Guid, ObjectiveID As Guid, EventID As Guid) As clsControlAssignment
            Dim control As clsControl = GetControlByID(ControlID)
            If control Is Nothing Then Return Nothing

            Dim newControlAssignment As New clsControlAssignment(control)
            newControlAssignment.ID = Guid.NewGuid()
            newControlAssignment.ObjectiveID = ObjectiveID
            newControlAssignment.EventID = EventID
            newControlAssignment.IsActive = True

            Dim a As clsControlAssignment = control.GetAssignmentByID(newControlAssignment.ID)
            If a Is Nothing Then
                For Each assignment As clsControlAssignment In control.Assignments
                    If assignment.ObjectiveID.Equals(ObjectiveID) AndAlso assignment.EventID.Equals(EventID) Then
                        a = assignment
                    End If
                Next
            End If
            If a Is Nothing Then
                control.Assignments.Add(newControlAssignment)
                Return newControlAssignment
            Else
                Return a
            End If
        End Function

        Public Function SetControlEffectiveness(ControlID As Guid, Value As Double) As Boolean
            Dim control As clsControl = GetControlByID(ControlID)
            If control Is Nothing Then Return False
            control.Effectiveness = Value
            Return True
        End Function

        Public Function DeleteControl(ID As Guid) As Boolean
            For i As Integer = Controls.Count - 1 To 0 Step -1
                If Controls(i).ID.Equals(ID) Then
                    Dim res As Boolean = True
                    If Controls(i).Assignments IsNot Nothing Then
                        While Controls(i).Assignments.Count > 0 AndAlso res
                            Dim ctrlAssignment As ECCore.clsControlAssignment = Controls(i).Assignments(0)
                            res = DeleteControlAssignment(Controls(i).ID, ctrlAssignment.ID)
                        End While
                    End If
                    Controls.RemoveAt(i)
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function DeleteControlAssignment(ControlID As Guid, AssignmentID As Guid) As Boolean
            Dim control As clsControl = GetControlByID(ControlID)
            If control Is Nothing Then Return False
            Return control.Assignments.RemoveAll(Function(a) (a.ID.Equals(AssignmentID)))
        End Function

        Public Function SetAssignmentComment(ControlID As Guid, ControlAssignmentID As Guid, Comment As String) As Boolean
            Dim control As clsControl = GetControlByID(ControlID)
            If control Is Nothing Then Return False
            Return control.SetAssignmentValue(ControlAssignmentID, Comment)
        End Function

        Public Function SetControlValue(ControlID As Guid, ControlAssignmentID As Guid, Value As Double) As Boolean
            Dim control As clsControl = GetControlByID(ControlID)
            If control Is Nothing Then Return False
            Return control.SetAssignmentValue(ControlAssignmentID, Value)
        End Function

        Public Function SetControlActive(ControlID As Guid, Value As Boolean) As Boolean
            Dim control As clsControl = GetControlByID(ControlID)
            If control IsNot Nothing AndAlso control.Enabled Then
                control.Active = value
            End If
            Return True
        End Function

        Public Function SetControlsActive(Type As ControlType, Value As Boolean) As Boolean
            For Each control As clsControl In Controls
                If control.Enabled AndAlso control.Type = Type Then
                    control.Active = value
                End If
            Next
            Return True
        End Function

        Public Function SetControlName(ID As Guid, Name As String) As Boolean
            Dim control As clsControl = GetControlByID(ID)
            If control Is Nothing Then
                Return False
            Else
                control.Name = Name
                Return True
            End If
        End Function

        Public Function SetControlInfodoc(ID As Guid, InfoDoc As String) As Boolean
            Dim control As clsControl = GetControlByID(ID)
            If control Is Nothing Then
                Return False
            Else
                control.InfoDoc = InfoDoc
                Return True
            End If
        End Function

        Public Function SetControlCost(ID As Guid, Cost As Double) As Boolean
            Dim control As clsControl = GetControlByID(ID)
            If control Is Nothing Then
                Return False
            Else
                control.Cost = Cost
                Return True
            End If
        End Function

        Public Function UpdateControl(ID As Guid, Name As String, InfoDoc As String) As Boolean 'A0640
            Dim control As clsControl = GetControlByID(ID)
            If control Is Nothing Then
                Return False
            Else
                control.Name = Name
                control.InfoDoc = InfoDoc
                Return True
            End If
        End Function

        Public Function GetAssignmentValue(ControlID As Guid, ObjectiveID As Guid, EventID As Guid) As Double
            Dim res As Double = -1
            For Each control As clsControl In Controls
                If control.ID.Equals(ControlID) Then
                    For Each assignment As clsControlAssignment In control.Assignments
                        If assignment.ObjectiveID.Equals(ObjectiveID) AndAlso assignment.EventID.Equals(EventID) Then
                            res = assignment.Value
                        End If
                    Next
                    If res = -1 AndAlso control.Type = ControlType.ctCause AndAlso EventID.Equals(Guid.Empty) Then
                        Dim sourceNode As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(ObjectiveID)
                        For Each assignment As clsControlAssignment In control.Assignments
                            Dim parentNode As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(assignment.ObjectiveID)
                            If ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).IsChildOf(sourceNode, parentNode) Then
                                res = assignment.Value
                            End If
                        Next
                    End If
                    Exit For
                End If
            Next
            Return res
        End Function

        Public Function SetControlsVars(EventIDs As List(Of Guid)) As Integer 'A1368
            For Each control As clsControl In Controls
                control.VarID = 0
            Next

            Dim i As Integer = 1

            Dim includeAll As Boolean = True
            If includeAll Then
                For Each control As clsControl In Controls
                    control.VarID = i
                    i += 1
                Next
                Return i - 1
            End If


            For Each eventID As Guid In EventIDs
                Dim sources As List(Of clsNode) = ProjectManager.GetContributedCoveringObjectives(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID), ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood))
                For Each source As clsNode In sources
                    Dim sControls As List(Of clsControl) = GetControlsForSource(source.NodeGuidID, False)
                    For Each control As clsControl In sControls
                        If control.VarID = 0 Then
                            control.VarID = i
                            i += 1
                        End If
                    Next
                Next

                For Each source As clsNode In sources
                    Dim vControls As List(Of clsControl) = GetControlsForVulnerabilities(source.NodeGuidID, eventID, False)
                    For Each control As clsControl In vControls
                        If control.VarID = 0 Then
                            control.VarID = i
                            i += 1
                        End If
                    Next
                Next

                Dim objectives As List(Of clsNode) = ProjectManager.GetContributedCoveringObjectives(ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(eventID), ProjectManager.Hierarchy(ECHierarchyID.hidImpact))
                For Each obj As clsNode In objectives
                    Dim cControls As List(Of clsControl) = GetControlsForConsequences(obj.NodeGuidID, eventID, False)
                    For Each control As clsControl In cControls
                        If control.VarID = 0 Then
                            control.VarID = i
                            i += 1
                        End If
                    Next
                Next
            Next

            Dim uncontributedEvents As List(Of clsNode) = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives
            For Each e As clsNode In uncontributedEvents
                Dim vControls As List(Of clsControl) = GetControlsForVulnerabilities(ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, e.NodeGuidID, False)
                For Each control As clsControl In vControls
                    If control.VarID = 0 Then
                        control.VarID = i
                        i += 1
                    End If
                Next
            Next

            Return i - 1 'A1368
        End Function

        Public Function IsValidControlAssignment(tControl As clsControl, tAssignment As clsControlAssignment) As Boolean
            Dim retVal As Boolean = False
            If ProjectManager IsNot Nothing Then 
                Dim node As clsNode = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tAssignment.ObjectiveID)
                If node Is Nothing Then node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tAssignment.ObjectiveID)
                If node Is Nothing AndAlso tControl.Type = ControlType.ctCauseToEvent Then
                    node = ProjectManager.ActiveAlternatives.GetNodeByID(tAssignment.ObjectiveID)
                End If
                If node IsNot Nothing then
                    Dim HierarchyID As ECHierarchyID = node.Hierarchy.HierarchyID
                    retVal = node IsNot Nothing ' AndAlso (node.IsTerminalNode OrElse node.ParentNode Is Nothing)
                    If retVal AndAlso (tControl.Type = ControlType.ctCauseToEvent OrElse tControl.Type = ControlType.ctConsequenceToEvent) Then
                        Dim alt As clsNode = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(tAssignment.EventID)
                        'If node.IsTerminalNode AndAlso (HierarchyID = ECHierarchyID.hidLikelihood And ProjectManager.Hierarchy(HierarchyID).Nodes.Count > 1 Or HierarchyID = ECHierarchyID.hidImpact) AndAlso Not node.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) OrElse (Not node.IsTerminalNode OrElse ProjectManager.Hierarchy(HierarchyID).Nodes.Count = 1) AndAlso ProjectManager.GetContributedCoveringObjectives(alt, node.Hierarchy).Count > 0 Then
                        If node.IsTerminalNode AndAlso (HierarchyID = ECHierarchyID.hidLikelihood AndAlso ProjectManager.Hierarchy(HierarchyID).Nodes.Count > 1 OrElse HierarchyID = ECHierarchyID.hidImpact) AndAlso Not node.GetNodesBelow(UNDEFINED_USER_ID).Contains(alt) OrElse Not node.IsTerminalNode AndAlso ProjectManager.GetContributedCoveringObjectives(alt, node.Hierarchy).Count > 0 AndAlso alt.Enabled Then 'A1371 + A1403
                            retVal = False
                        End If
                    End If
                End If
            End If
                
            Return retVal
        End Function

        Private Function WriteControlsStream_1_1_16() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Assignments.Count)
                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_17() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)
                BW.Write(control.Assignments.Count)
                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_20() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)
                BW.Write(control.Assignments.Count)
                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(CInt(assignment.MeasurementType))
                    BW.Write(CInt(assignment.MeasurementScaleID))
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_21() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)
                BW.Write(control.Assignments.Count)
                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(CInt(assignment.MeasurementType))
                    BW.Write(assignment.MeasurementScaleID)
                    BW.Write(assignment.MeasurementScaleGuid.ToByteArray)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_23() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)
                BW.Write(control.Assignments.Count)
                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(CInt(assignment.MeasurementType))
                    BW.Write(assignment.MeasurementScaleID)
                    BW.Write(assignment.MeasurementScaleGuid.ToByteArray)
                    BW.Write(assignment.IsActive)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_41() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)

                ' new to v.1.1.41
                BW.Write(control.Must)
                BW.Write(control.MustNot)

                BW.Write(control.Assignments.Count)

                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(CInt(assignment.MeasurementType))
                    BW.Write(assignment.MeasurementScaleID)
                    BW.Write(assignment.MeasurementScaleGuid.ToByteArray)
                    BW.Write(assignment.IsActive)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            ' settings
            BW.Write(Options.BaseCaseForConstraints)
            BW.Write(Options.BaseCaseForDependencies)
            BW.Write(Options.BaseCaseForFundingPools)
            BW.Write(Options.BaseCaseForGroups)
            BW.Write(Options.BaseCaseForMustNots)
            BW.Write(Options.BaseCaseForMusts)
            BW.Write(Options.CustomConstraints)
            BW.Write(Options.Dependencies)
            BW.Write(Options.FundingPools)
            BW.Write(Options.Groups)
            BW.Write(Options.MustNots)
            BW.Write(Options.Musts)
            BW.Write(Options.Risks)
            BW.Write(Options.UseBaseCase)
            BW.Write(Options.UseBaseCaseOptions)
            BW.Write(Options.UseIgnoreOptions)
            BW.Write(Options.TimePeriods)

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_42() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)
                BW.Write(control.Enabled)

                '' new to v.1.1.41
                'BW.Write(control.Must)
                'BW.Write(control.MustNot)

                BW.Write(True)
                BW.Write(True)

                BW.Write(control.Assignments.Count)

                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(CInt(assignment.MeasurementType))
                    BW.Write(assignment.MeasurementScaleID)
                    BW.Write(assignment.MeasurementScaleGuid.ToByteArray)
                    BW.Write(assignment.IsActive)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            ' settings
            BW.Write(Options.BaseCaseForConstraints)
            BW.Write(Options.BaseCaseForDependencies)
            BW.Write(Options.BaseCaseForFundingPools)
            BW.Write(Options.BaseCaseForGroups)
            BW.Write(Options.BaseCaseForMustNots)
            BW.Write(Options.BaseCaseForMusts)
            BW.Write(Options.CustomConstraints)
            BW.Write(Options.Dependencies)
            BW.Write(Options.FundingPools)
            BW.Write(Options.Groups)
            BW.Write(Options.MustNots)
            BW.Write(Options.Musts)
            BW.Write(Options.Risks)
            BW.Write(Options.UseBaseCase)
            BW.Write(Options.UseBaseCaseOptions)
            BW.Write(Options.UseIgnoreOptions)
            BW.Write(Options.TimePeriods)

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream_1_1_46() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Controls.Count)

            For Each control As clsControl In Controls
                BW.Write(control.ID.ToByteArray)
                BW.Write(control.Name)
                BW.Write(control.InfoDoc)
                BW.Write(CInt(control.Type))
                BW.Write(control.Effectiveness)
                BW.Write(control.RiskReduction)
                BW.Write(control.Enabled)

                '' new to v.1.1.41
                'BW.Write(control.Must)
                'BW.Write(control.MustNot)

                BW.Write(False)
                BW.Write(control.Active)

                BW.Write(control.IsDecreasing)

                BW.Write(control.Assignments.Count)

                For Each assignment As clsControlAssignment In control.Assignments
                    BW.Write(assignment.ID.ToByteArray)
                    BW.Write(CStr(assignment.Comment)) 'A0640
                    BW.Write(assignment.Value)
                    BW.Write(CInt(assignment.MeasurementType))
                    BW.Write(assignment.MeasurementScaleID)
                    BW.Write(assignment.MeasurementScaleGuid.ToByteArray)
                    BW.Write(assignment.IsActive)
                    BW.Write(assignment.ObjectiveID.ToByteArray)
                    If control.Type = ControlType.ctCauseToEvent Or control.Type = ControlType.ctConsequenceToEvent Then
                        BW.Write(assignment.EventID.ToByteArray)
                    End If
                Next
            Next

            ' settings
            BW.Write(Options.BaseCaseForConstraints)
            BW.Write(Options.BaseCaseForDependencies)
            BW.Write(Options.BaseCaseForFundingPools)
            BW.Write(Options.BaseCaseForGroups)
            BW.Write(Options.BaseCaseForMustNots)
            BW.Write(Options.BaseCaseForMusts)
            BW.Write(Options.CustomConstraints)
            BW.Write(Options.Dependencies)
            BW.Write(Options.FundingPools)
            BW.Write(Options.Groups)
            BW.Write(Options.MustNots)
            BW.Write(Options.Musts)
            BW.Write(Options.Risks)
            BW.Write(Options.UseBaseCase)
            BW.Write(Options.UseBaseCaseOptions)
            BW.Write(Options.UseIgnoreOptions)
            BW.Write(Options.TimePeriods)

            BW.Close()

            Return MS
        End Function

        Private Function WriteControlsStream() As MemoryStream
            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 16
                    Return WriteControlsStream_1_1_16()
                Case 17 To 19
                    Return WriteControlsStream_1_1_17()
                Case 20
                    Return WriteControlsStream_1_1_20()
                Case 21, 22
                    Return WriteControlsStream_1_1_21()
                Case 23 to 40
                    Return WriteControlsStream_1_1_23()
                Case 41
                    Return WriteControlsStream_1_1_41()
                Case 42 To 45
                    Return WriteControlsStream_1_1_42()
                Case Else
                    Return WriteControlsStream_1_1_46()
            End Select

            Return Nothing
        End Function

        Private Function ReadControlsStream_1_1_16(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                control.Type = Type

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble
                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Select Case Type
                        Case ControlType.ctCause
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                        Case (ControlType.ctConsequence)
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctEvent
                            isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                    End Select

                    If isValid Then
                        If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                            assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                        Else
                            assignment = AddControlAssignment(control.ID, Object1ID)
                        End If
                        assignment.ID = assignmentID
                        assignment.Comment = Comment
                        assignment.Value = Value
                    End If
                Next
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_17(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble
                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Select Case Type
                        Case ControlType.ctCause
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                        Case (ControlType.ctConsequence)
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctEvent
                            isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                    End Select

                    If isValid Then
                        If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                            assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                        Else
                            assignment = AddControlAssignment(control.ID, Object1ID)
                        End If
                        assignment.ID = assignmentID
                        assignment.Comment = Comment
                        assignment.Value = Value
                    End If
                Next
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_20(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble

                    Dim MT As ECMeasureType = CType(BR.ReadInt32, ECMeasureType)
                    Dim MTID As Integer = BR.ReadInt32

                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Select Case Type
                        Case ControlType.ctCause
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                        Case (ControlType.ctConsequence)
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctEvent
                            isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                    End Select

                    If isValid Then
                        If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                            assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                        Else
                            assignment = AddControlAssignment(control.ID, Object1ID)
                        End If
                        assignment.ID = assignmentID
                        assignment.Comment = Comment
                        assignment.Value = Value
                        assignment.MeasurementType = MT
                        assignment.MeasurementScaleID = MTID
                    End If
                Next
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_21(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble

                    Dim MT As ECMeasureType = CType(BR.ReadInt32, ECMeasureType)
                    Dim MTID As Integer = BR.ReadInt32
                    Dim MTGuidID As Guid = New Guid(BR.ReadBytes(16))

                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Select Case Type
                        Case ControlType.ctCause
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                        Case (ControlType.ctConsequence)
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctEvent
                            isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                    End Select

                    If isValid Then
                        If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                            assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                        Else
                            assignment = AddControlAssignment(control.ID, Object1ID)
                        End If
                        assignment.ID = assignmentID
                        assignment.Comment = Comment
                        assignment.Value = Value
                        assignment.MeasurementType = MT
                        assignment.MeasurementScaleID = MTID
                        assignment.MeasurementScaleGuid = MTGuidID
                    End If
                Next
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_23(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble

                    Dim MT As ECMeasureType = CType(BR.ReadInt32, ECMeasureType)
                    Dim MTID As Integer = BR.ReadInt32
                    Dim MTGuidID As Guid = New Guid(BR.ReadBytes(16))
                    Dim isActive As Boolean = BR.ReadBoolean

                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Select Case Type
                        Case ControlType.ctCause
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                        Case (ControlType.ctConsequence)
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctEvent
                            isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID) IsNot Nothing
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            isValid = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID) IsNot Nothing
                            If isValid Then
                                isValid = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID) IsNot Nothing
                            End If
                    End Select

                    If isValid Then
                        If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                            assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                        Else
                            assignment = AddControlAssignment(control.ID, Object1ID)
                        End If
                        assignment.ID = assignmentID
                        assignment.Comment = Comment
                        assignment.Value = Value
                        assignment.MeasurementType = MT
                        assignment.MeasurementScaleID = MTID
                        assignment.MeasurementScaleGuid = MTGuidID
                        assignment.IsActive = isActive
                    End If
                Next
            Next

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_41(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = GetControlByID(ID)

                Dim isNewControl As Boolean = False
                If control Is Nothing Then
                    control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                    isNewControl = True
                End If
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                'control.Must = BR.ReadBoolean
                'control.MustNot = BR.ReadBoolean
                BR.ReadBoolean()
                BR.ReadBoolean()

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble

                    Dim MT As ECMeasureType = CType(BR.ReadInt32, ECMeasureType)
                    Dim MTID As Integer = BR.ReadInt32
                    Dim MTGuidID As Guid = New Guid(BR.ReadBytes(16))
                    Dim isActive As Boolean = BR.ReadBoolean

                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Dim node As clsNode = Nothing
                    Dim alt As clsNode = Nothing
                    Select Case Type
                        Case ControlType.ctCause
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID)
                            isValid = node IsNot Nothing AndAlso node.IsTerminalNode
                        Case (ControlType.ctConsequence)
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID)
                            isValid = node IsNot Nothing AndAlso node.IsTerminalNode
                        Case ControlType.ctEvent
                            alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID)
                            isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID)
                            If node IsNot Nothing AndAlso (node.IsTerminalNode OrElse (node Is ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0))) Then 'A1352
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID)
                                isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID)
                            If node IsNot Nothing AndAlso node.IsTerminalNode Then
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID)
                                isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                            End If
                    End Select

                    If isValid Then
                        If isNewControl Then
                            If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                                assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                            Else
                                assignment = AddControlAssignment(control.ID, Object1ID)
                            End If
                            If assignment IsNot Nothing Then
                                assignment.ID = assignmentID
                                assignment.Comment = Comment
                                assignment.Value = Value
                                assignment.MeasurementType = MT
                                assignment.MeasurementScaleID = MTID
                                assignment.MeasurementScaleGuid = MTGuidID
                                assignment.IsActive = isActive
                            End If
                        End If
                    End If
                Next
            Next

            ' settings
            Options.BaseCaseForConstraints = BR.ReadBoolean
            Options.BaseCaseForDependencies = BR.ReadBoolean
            Options.BaseCaseForFundingPools = BR.ReadBoolean
            Options.BaseCaseForGroups = BR.ReadBoolean
            Options.BaseCaseForMustNots = BR.ReadBoolean
            Options.BaseCaseForMusts = BR.ReadBoolean

            Options.CustomConstraints = BR.ReadBoolean
            Options.Dependencies = BR.ReadBoolean
            Options.FundingPools = BR.ReadBoolean
            Options.Groups = BR.ReadBoolean
            Options.MustNots = BR.ReadBoolean
            Options.Musts = BR.ReadBoolean
            Options.Risks = BR.ReadBoolean

            Options.UseBaseCase = BR.ReadBoolean
            Options.UseBaseCaseOptions = BR.ReadBoolean
            Options.UseIgnoreOptions = BR.ReadBoolean

            Options.TimePeriods = BR.ReadBoolean

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_42(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = GetControlByID(ID)

                Dim isNewControl As Boolean = False
                If control Is Nothing Then
                    control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                    isNewControl = True
                End If
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                control.Enabled = BR.ReadBoolean

                'control.Must = BR.ReadBoolean
                'control.MustNot = BR.ReadBoolean
                BR.ReadBoolean()
                BR.ReadBoolean()

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble

                    Dim MT As ECMeasureType = CType(BR.ReadInt32, ECMeasureType)
                    Dim MTID As Integer = BR.ReadInt32
                    Dim MTGuidID As Guid = New Guid(BR.ReadBytes(16))
                    Dim isActive As Boolean = BR.ReadBoolean

                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Dim node As clsNode = Nothing
                    Dim alt As clsNode = Nothing
                    Select Case Type
                        Case ControlType.ctCause
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID)
                            isValid = node IsNot Nothing AndAlso node.IsTerminalNode
                        Case (ControlType.ctConsequence)
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID)
                            isValid = node IsNot Nothing AndAlso node.IsTerminalNode
                        Case ControlType.ctEvent
                            alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID)
                            isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID)
                            If node IsNot Nothing AndAlso (node.IsTerminalNode OrElse (node Is ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0))) Then 'A1352
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID)
                                isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID)
                            If node IsNot Nothing AndAlso node.IsTerminalNode Then
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID)
                                isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                            End If
                    End Select

                    If isValid Then
                        If isNewControl Then
                            If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                                assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                            Else
                                assignment = AddControlAssignment(control.ID, Object1ID)
                            End If
                            If assignment IsNot Nothing Then
                                assignment.ID = assignmentID
                                assignment.Comment = Comment
                                assignment.Value = Value
                                assignment.MeasurementType = MT
                                assignment.MeasurementScaleID = MTID
                                assignment.MeasurementScaleGuid = MTGuidID
                                assignment.IsActive = isActive
                            End If
                        End If
                    End If
                Next
            Next

            ' settings
            Options.BaseCaseForConstraints = BR.ReadBoolean
            Options.BaseCaseForDependencies = BR.ReadBoolean
            Options.BaseCaseForFundingPools = BR.ReadBoolean
            Options.BaseCaseForGroups = BR.ReadBoolean
            Options.BaseCaseForMustNots = BR.ReadBoolean
            Options.BaseCaseForMusts = BR.ReadBoolean

            Options.CustomConstraints = BR.ReadBoolean
            Options.Dependencies = BR.ReadBoolean
            Options.FundingPools = BR.ReadBoolean
            Options.Groups = BR.ReadBoolean
            Options.MustNots = BR.ReadBoolean
            Options.Musts = BR.ReadBoolean
            Options.Risks = BR.ReadBoolean

            Options.UseBaseCase = BR.ReadBoolean
            Options.UseBaseCaseOptions = BR.ReadBoolean
            Options.UseIgnoreOptions = BR.ReadBoolean

            Options.TimePeriods = BR.ReadBoolean

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream_1_1_46(ByVal Stream As MemoryStream) As Boolean
            Stream.Seek(0, SeekOrigin.Begin)

            Dim BR As New BinaryReader(Stream)

            Dim count As Integer = BR.ReadInt32


            Dim ID As Guid
            Dim assignmentID As Guid
            Dim Name As String
            Dim Comment As String
            Dim InfoDoc As String
            Dim Type As ControlType
            Dim Effectiveness As Double
            Dim RiskReduction As Double
            Dim Value As Double
            Dim Object1ID As Guid
            Dim Object2ID As Guid
            Dim assignmentsCount As Integer

            Dim control As clsControl
            Dim assignment As clsControlAssignment

            For i As Integer = 1 To count
                control = Nothing

                ID = New Guid(BR.ReadBytes(16))
                Name = BR.ReadString
                InfoDoc = BR.ReadString
                Type = CType(BR.ReadInt32, ControlType)

                Effectiveness = BR.ReadDouble
                RiskReduction = BR.ReadDouble

                control = GetControlByID(ID)

                Dim isNewControl As Boolean = False
                If control Is Nothing Then
                    control = ProjectManager.Controls.AddControl(ID, Name, Type, InfoDoc) 'A0640 'A0820
                    isNewControl = True
                End If
                control.Type = Type
                control.Effectiveness = Effectiveness
                control.RiskReduction = RiskReduction

                control.Enabled = BR.ReadBoolean

                'control.Must = BR.ReadBoolean
                'control.MustNot = BR.ReadBoolean

                'BR.ReadBoolean()
                'BR.ReadBoolean()
                Dim isOldDummyBool As Boolean = BR.ReadBoolean
                Dim isActiveControl As Boolean = BR.ReadBoolean

                control.IsDecreasing = BR.ReadBoolean

                assignmentsCount = BR.ReadInt32

                For j As Integer = 1 To assignmentsCount
                    assignmentID = New Guid(BR.ReadBytes(16))
                    Comment = BR.ReadString
                    Value = BR.ReadDouble

                    Dim MT As ECMeasureType = CType(BR.ReadInt32, ECMeasureType)
                    Dim MTID As Integer = BR.ReadInt32
                    Dim MTGuidID As Guid = New Guid(BR.ReadBytes(16))
                    Dim isActive As Boolean = BR.ReadBoolean

                    Object1ID = New Guid(BR.ReadBytes(16))

                    Dim isValid As Boolean = False
                    Dim node As clsNode = Nothing
                    Dim alt As clsNode = Nothing
                    Select Case Type
                        Case ControlType.ctCause
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID)
                            'isValid = node IsNot Nothing AndAlso node.IsTerminalNode
                            isValid = node IsNot Nothing
                        Case (ControlType.ctConsequence)
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID)
                            isValid = node IsNot Nothing AndAlso node.IsTerminalNode
                        Case ControlType.ctEvent
                            alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object1ID)
                            isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                        Case ControlType.ctCauseToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(Object1ID)
                            If node Is Nothing Then
                                node = ProjectManager.ActiveAlternatives.GetNodeByID(Object1ID)
                            End If
                            If node IsNot Nothing AndAlso (node.IsTerminalNode OrElse (node Is ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0))) Then 'A1352
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID)
                                isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                            End If
                        Case ControlType.ctConsequenceToEvent
                            Object2ID = New Guid(BR.ReadBytes(16))
                            node = ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(Object1ID)
                            If node IsNot Nothing AndAlso node.IsTerminalNode Then
                                alt = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).GetNodeByID(Object2ID)
                                isValid = alt IsNot Nothing AndAlso alt.IsTerminalNode
                            End If
                    End Select

                    If isValid Then
                        If isNewControl Then
                            If Type = ControlType.ctCauseToEvent Or Type = ControlType.ctConsequenceToEvent Then
                                assignment = AddControlAssignment(control.ID, Object1ID, Object2ID)
                            Else
                                assignment = AddControlAssignment(control.ID, Object1ID)
                            End If
                            If assignment IsNot Nothing Then
                                assignment.ID = assignmentID
                                assignment.Comment = Comment
                                assignment.Value = Value
                                assignment.MeasurementType = MT
                                assignment.MeasurementScaleID = MTID
                                assignment.MeasurementScaleGuid = MTGuidID
                                assignment.IsActive = isActive
                            End If
                        End If
                    End If
                Next
                If Not isOldDummyBool Then
                    control.Active = isActiveControl
                End If
            Next

            ' settings
            Options.BaseCaseForConstraints = BR.ReadBoolean
            Options.BaseCaseForDependencies = BR.ReadBoolean
            Options.BaseCaseForFundingPools = BR.ReadBoolean
            Options.BaseCaseForGroups = BR.ReadBoolean
            Options.BaseCaseForMustNots = BR.ReadBoolean
            Options.BaseCaseForMusts = BR.ReadBoolean

            Options.CustomConstraints = BR.ReadBoolean
            Options.Dependencies = BR.ReadBoolean
            Options.FundingPools = BR.ReadBoolean
            Options.Groups = BR.ReadBoolean
            Options.MustNots = BR.ReadBoolean
            Options.Musts = BR.ReadBoolean
            Options.Risks = BR.ReadBoolean

            Options.UseBaseCase = BR.ReadBoolean
            Options.UseBaseCaseOptions = BR.ReadBoolean
            Options.UseIgnoreOptions = BR.ReadBoolean

            Options.TimePeriods = BR.ReadBoolean

            BR.Close()

            Return True
        End Function

        Private Function ReadControlsStream(ByVal Stream As MemoryStream) As Boolean
            If Stream Is Nothing OrElse Stream.Length = 0 Then Return False

            Select Case ProjectManager.StorageManager.CanvasDBVersion.MinorVersion
                Case 16
                    ReadControlsStream_1_1_16(Stream)
                Case 17 To 19
                    ReadControlsStream_1_1_17(Stream)
                Case 20
                    ReadControlsStream_1_1_20(Stream)
                Case 21, 22
                    Try
                        ReadControlsStream_1_1_21(Stream)
                    Catch ex As Exception
                        ReadControlsStream_1_1_20(Stream)
                    End Try
                Case 23 To 40
                    ReadControlsStream_1_1_23(Stream)
                Case 41
                    ReadControlsStream_1_1_41(Stream)
                Case 42 To 45
                    ReadControlsStream_1_1_42(Stream)
                Case Else
                    ReadControlsStream_1_1_46(Stream)
            End Select
        End Function

        Private Function ReadControls_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim time As Nullable(Of DateTime)
            Dim MS As MemoryStream = ProjectManager.StorageManager.Reader.GetModelStructureStream(StructureType.stControls, time, Location, ProviderType, ModelID)
            Dim res As Boolean = ReadControlsStream(MS)
            If res Then ProjectManager.CacheManager.StructureLoaded(StructureType.stControls) = time
            Return res
        End Function
        Public Function ReadControls(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Controls.Clear()

            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    ReadControls_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select

            Return True
        End Function

        Protected Function WriteControls_CanvasStreamDatabase(ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Dim MS As MemoryStream = WriteControlsStream()
            Dim res = ProjectManager.StorageManager.Writer.SaveModelStructureStream(StructureType.stControls, MS)
            Return res
        End Function

        Public Function WriteControls(ByVal StorageType As ECModelStorageType, ByVal Location As String, ByVal ProviderType As DBProviderType, ByVal ModelID As Integer) As Boolean
            Select Case StorageType
                Case ECModelStorageType.mstCanvasStreamDatabase
                    WriteControls_CanvasStreamDatabase(Location, ProviderType, ModelID)
            End Select
            Return True
        End Function

        Public Sub New(ProjectManager As clsProjectManager)
            Me.ProjectManager = ProjectManager
        End Sub
    End Class
End Namespace