Imports System.Collections
Imports ECCore
Imports System.IO 'C0304
Imports System.Web.Script.Serialization
Imports System.Linq

Namespace ECCore

    <Serializable()> Public Class clsNodesBelowForUser
        Public Property Nodes() As New List(Of clsNode)
        Public Property UserID() As Integer

        Public Sub New(ByVal UserID As Integer)
            Me.UserID = UserID
        End Sub
    End Class

    <Serializable()> Public Class clsAHPNodeData 'C0304
        Public StructuralAdjust As Boolean
        Public ProtectedJudgments As Boolean
        Public topp As Integer = UNDEFINED_INTEGER_VALUE 'C0306
        Public Leftt As Integer = UNDEFINED_INTEGER_VALUE 'C0306
        Public EnforceMode As Boolean

        Public Function FromStream(ByVal Stream As MemoryStream) As Boolean
            Dim BR As New BinaryReader(Stream)

            Dim res As Boolean = True
            Try
                StructuralAdjust = BR.ReadBoolean
                ProtectedJudgments = BR.ReadBoolean
                topp = BR.ReadInt32
                Leftt = BR.ReadInt32
                EnforceMode = BR.ReadBoolean
            Catch ex As Exception
                res = False
            Finally
                BR.Close()
            End Try

            Return res
        End Function

        Public Function ToStream() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(StructuralAdjust)
            BW.Write(ProtectedJudgments)
            BW.Write(topp)
            BW.Write(Leftt)
            BW.Write(EnforceMode)

            BW.Close()

            Return MS
        End Function
    End Class

    <Serializable()> Public Class clsAHPAltData 'C0304
        Public Infeasible As Boolean
        'Public IID As Integer = UNDEFINED_INTEGER_VALUE 'C0306
        Public MID As String = UNDEFINED_STRING_VALUE 'C0306
        Public Level As Integer = UNDEFINED_INTEGER_VALUE 'C0306
        Public IsLeaf As Boolean
        Public Selected As Boolean
        Public Period As Integer = UNDEFINED_INTEGER_VALUE 'C0306 'C0429
        Public BasePeriodAID As String = UNDEFINED_STRING_VALUE 'C0306 'C0429

        Public Function FromStream(ByVal Stream As MemoryStream) As Boolean
            Dim BR As New BinaryReader(Stream)

            Dim res As Boolean = True
            Try
                Infeasible = BR.ReadBoolean
                'IID = BR.ReadInt32
                MID = BR.ReadString
                Level = BR.ReadInt32
                IsLeaf = BR.ReadBoolean
                Selected = BR.ReadBoolean
                'Period = BR.ReadInt32
                'BasePeriodAID = BR.ReadString

                'C0429===
                If BR.BaseStream.Position <> BR.BaseStream.Length Then
                    Period = BR.ReadInt32
                    BasePeriodAID = BR.ReadString
                Else
                    Period = UNDEFINED_INTEGER_VALUE
                    BasePeriodAID = UNDEFINED_STRING_VALUE
                End If
                'C0429==
            Catch ex As Exception
                res = False
            Finally
                BR.Close()
            End Try

            Return res
        End Function

        Public Function ToStream() As MemoryStream
            Dim MS As New MemoryStream
            Dim BW As New BinaryWriter(MS)

            BW.Write(Infeasible)
            'BW.Write(IID)
            BW.Write(MID)
            BW.Write(Level)
            BW.Write(IsLeaf)
            BW.Write(Selected)
            'BW.Write(Period)
            'BW.Write(BasePeriodAID)

            'C0429===
            If Period <> UNDEFINED_INTEGER_VALUE Or BasePeriodAID <> UNDEFINED_STRING_VALUE Then
                If Period = UNDEFINED_INTEGER_VALUE Then
                    BW.Write(CInt(0))
                Else
                    BW.Write(Period)
                End If
                If BasePeriodAID = UNDEFINED_STRING_VALUE Then
                    BW.Write("")
                Else
                    BW.Write(BasePeriodAID)
                End If
            End If
            'C0429==

            BW.Close()

            Return MS
        End Function
    End Class

    <Serializable()> Public Class clsNode
        'Private _NodeID As Integer
        Public Property NodeID() As Integer
        '    Get
        '        Return _NodeID
        '    End Get
        '    Set(value As Integer)
        '        If _NodeID <> value Then
        '            If Me.Hierarchy IsNot Nothing AndAlso Me.Hierarchy.mNodesIntDict.ContainsKey(_NodeID) Then 
        '                Me.Hierarchy.mNodesIntDict.Remove(_NodeID)
        '            End If
        '            _NodeID = value
        '            If Me.Hierarchy IsNot Nothing AndAlso Not Me.Hierarchy.mNodesIntDict.ContainsKey(value) Then 
        '                Me.Hierarchy.mNodesIntDict.Add(value, Me)
        '            End If
        '        End If
        '    End Set
        'End Property
        Public Property NodeMappedID() As Integer 'AS/12323b

        'Public Property _NodeGuidID As Guid = Guid.NewGuid
        Public Property NodeGuidID() As Guid = Guid.NewGuid
        '    Get
        '        Return _NodeGuidID
        '    End Get
        '    Set(value As Guid)
        '        If _NodeGuidID <> value Then
        '            If Me.Hierarchy IsNot Nothing AndAlso Me.Hierarchy.mNodesGuidDict.ContainsKey(_NodeGuidID) Then 
        '                Me.Hierarchy.mNodesGuidDict.Remove(_NodeGuidID)
        '            End If
        '            _NodeGuidID = value
        '            If Me.Hierarchy IsNot Nothing AndAlso Not Me.Hierarchy.mNodesGuidDict.ContainsKey(value) Then 
        '                Me.Hierarchy.mNodesGuidDict.Add(value, Me)
        '            End If
        '        End If
        '    End Set
        'End Property

        Public Property OldNodeGuidID As Guid = Guid.Empty
        Public Property EventType As EventType = EventType.Risk
        Public Property Level() As Integer
        Public Property IsAlternative() As Boolean
        Public Property ParentNodeID() As Integer = -1
        Public Property NodeRank As Integer = 0
        Public Property UserRank As Integer = 0 ' D6671
        
        'A2103 ===
        Public Property Pros(ProjectManager As clsProjectManager) As List(Of AlternativeProAndCon)
            Get
                Dim attrVal As String = CStr(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNATIVE_PROS_ID, Me.NodeGuidID))
                Dim retVal As List(Of AlternativeProAndCon) = New List(Of AlternativeProAndCon)
                Try
                    retVal = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of AlternativeProAndCon))(attrVal)
                Catch ex As Exception

                End Try
                Return retVal
            End Get
            Set(value As List(Of AlternativeProAndCon))
                Dim strVal As String = Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None)
                With ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_ALTERNATIVE_PROS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, strVal, Me.NodeGuidID, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property

        Public Property Cons(ProjectManager As clsProjectManager) As List(Of AlternativeProAndCon)
            Get
                Dim attrVal As String = CStr(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNATIVE_CONS_ID, Me.NodeGuidID))
                Dim retVal As List(Of AlternativeProAndCon) = New List(Of AlternativeProAndCon)
                Try
                    retVal = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of AlternativeProAndCon))(attrVal)
                Catch ex As Exception

                End Try
                Return retVal
            End Get
            Set(value As List(Of AlternativeProAndCon))
                Dim strVal As String = Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None)
                With ProjectManager
                    .Attributes.SetAttributeValue(ATTRIBUTE_ALTERNATIVE_CONS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, strVal, Me.NodeGuidID, Guid.Empty)
                    .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
                End With
            End Set
        End Property 'A2103 ==

        Private mParentNode As clsNode
        Public Property ParentNode(Optional ByVal DeleteCalculatedValues As Boolean = True, Optional ByVal LeaveParentMeasurementType As Boolean = False) As clsNode 'C0383
            Get
                If mParentNode Is Nothing Then
                    If ParentNodesGuids.Count > 0 Then
                        Dim pNode As clsNode = Hierarchy.GetNodeByID(ParentNodesGuids(0))
                        Return pNode
                    Else
                        Return Nothing
                    End If
                Else
                    Return mParentNode
                End If
            End Get
            Set(ByVal value As clsNode)
                ' D0042 ===
                If Hierarchy.ProjectManager.IsRiskProject And Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                    LeaveParentMeasurementType = True
                End If

                If mParentNode IsNot value Then ' if we ARE changing parent node
                    If mParentNode IsNot Nothing Then ' if current parent node exists then remove all references from it
                        mParentNode.Children.Remove(Me)
                        mParentNode.DeleteJudgmentsWithChild(Me.NodeID)

                        'C0144===
                        If mParentNode.IsTerminalNode Then
                            mParentNode.MeasureType = Hierarchy.DefaultMeasurementTypeForCoveringObjectives 'C0778
                            'For Each U As clsUser In Hierarchy.ProjectManager.UsersList
                            '    'If U.UserID <> COMBINED_USER_ID Then 'C0450
                            '    If U.UserID >= 0 Then 'C0450
                            '        Hierarchy.ProjectManager.StorageManager.Writer.SaveUserPermissions(U.UserID, mParentNode)
                            '    End If
                            'Next
                        Else
                            mParentNode.MeasureType = Hierarchy.DefaultMeasurementTypeForNonCoveringObjectives
                        End If
                        'C0144==
                    End If

                    ParentNodesGuids.Clear()
                    If value Is Nothing Then
                        ParentNodeID = -1
                        Level = 0
                    Else
                        value.Children.Add(Me)
                        ParentNodeID = value.NodeID
                        ParentNodesGuids.Add(value.NodeGuidID)
                        Level = value.Level + 1

                        'If Not Me.IsAlternative Then 'C0144
                        If Not Me.IsAlternative And Me.Hierarchy.HierarchyType = ECHierarchyType.htModel Then 'C0144
                            'If value.MeasureType <> ECMeasureType.mtPairwise Then 'C0075 'C0383
                            If (value.MeasureType <> ECMeasureType.mtPairwise) And Not LeaveParentMeasurementType Then 'C0383
                                'value.MeasureType = ECMeasureType.mtPairwise 'C0075
                                If value.IsTerminalNode Or (Not value.IsTerminalNode And (value.MeasureType <> ECMeasureType.mtDirect)) Then 'C0667 'C0668
                                    value.MeasureType(True) = ECMeasureType.mtPairwise 'C0075
                                End If
                            End If
                        End If
                    End If

                    'C0176===
                    If mParentNode IsNot Nothing Then
                        'Hierarchy.ProjectManager.StorageManager.Writer.DeleteCalculatedWeights(mParentNode.NodeID) 'C0180
                        'C0180===
                        If DeleteCalculatedValues Then
                            'Hierarchy.ProjectManager.StorageManager.Writer.DeleteCalculatedWeights(mParentNode.NodeID) 'C0181
                            mParentNode.Judgments.Weights.ClearUserWeights() 'C0181
                        End If
                        'C0180==
                    End If

                    If value IsNot Nothing Then
                        'Hierarchy.ProjectManager.StorageManager.Writer.DeleteCalculatedWeights(value.NodeID) 'C0180
                        'C0180===
                        If DeleteCalculatedValues Then
                            'Hierarchy.ProjectManager.StorageManager.Writer.DeleteCalculatedWeights(value.NodeID) 'C0181
                            value.Judgments.Weights.ClearUserWeights() 'C0181
                        End If
                        'C0180==
                    End If
                    'C0176==

                    mParentNode = value
                End If
            End Set
        End Property

        Public Property ParentNodesGuids As New List(Of Guid)

        Public ReadOnly Property ParentNodes As List(Of clsNode)
            Get
                Dim res As New List(Of clsNode)
                If Hierarchy IsNot Nothing Then
                    For Each NodeGuid As Guid In ParentNodesGuids
                        Dim node As clsNode = Hierarchy.GetNodeByID(NodeGuid)
                        If node IsNot Nothing Then
                            res.Add(node)
                        End If
                    Next
                End If
                Return res
            End Get
        End Property

        Public Property Children As New List(Of clsNode)
        Public Property ChildrenAlts As New HashSet(Of Integer)

        Public Property Hierarchy() As clsHierarchy

        Public Property Comment() As String = ""

        Public Property InfoDoc() As String = ""

        Public Property Tag() As Object = Nothing

        Public Property DefaultDataInstance() As clsDataInstance

        Private mNodeName As String = ""
        Public Property NodeName() As String
            Get
                Return mNodeName
            End Get
            Set(ByVal value As String)
                If value.Length > NODE_NAME_MAX_LENGTH Then
                    mNodeName = value.Remove(NODE_NAME_MAX_LENGTH)
                Else
                    mNodeName = value
                End If
            End Set
        End Property

        Public Property Enabled() As Boolean = True
        Public Property DisabledUserIDsList() As New HashSet(Of Integer)
        Public Property DisabledForUser(ByVal UserID As Integer) As Boolean 'C0211
            Get
                If IsAlternative Then
                    Return DisabledUserIDsList.Contains(UserID)
                Else
                    If DisabledUserIDsList.Contains(UserID) Then
                        Return True
                    Else
                        Dim isDisabled As Boolean = False
                        Dim parent As clsNode = ParentNode
                        While Not isDisabled AndAlso (parent IsNot Nothing)
                            If parent.DisabledForUser(UserID) Then
                                isDisabled = True
                            End If
                            parent = parent.ParentNode
                        End While
                        Return isDisabled
                    End If
                End If
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    If Not DisabledUserIDsList.Contains(UserID) Then
                        DisabledUserIDsList.Add(UserID)
                    End If
                Else
                    DisabledUserIDsList.Remove(UserID)
                End If
            End Set
        End Property

        Public Property Judgments() As clsCustomJudgments = New clsPairwiseJudgments(Me)
        Public Property PWOutcomesJudgments As New clsPairwiseJudgments(Me)
        Public Property DirectJudgmentsForNoCause As New clsNonPairwiseJudgments(Me)
        Public Property EventsJudgments As New clsNonPairwiseJudgments(Me)
        Public Property FeedbackJudgments As clsCustomJudgments = New clsNonPairwiseJudgments(Me)

        Public Property TmpNodesBelow() As New List(Of clsNode)

        Private mRandomChildren As List(Of clsNode) 'C0385
        Private mUseRandom As Boolean 'C0205

        Public Property TempRatingScaleInfoDoc As String = ""

        Friend Property AppliedControl As Boolean

        Public Property SOrder As Integer

        Public Property Sorted As Boolean = False

        Public Property DataMappingGUID As Guid 'AS/12323xf

        Public Property CombinedLoaded() As New List(Of Integer)
        Public Property PipeCreated As Boolean = False
        Public Property SavedToStream As Boolean = False

        Public Function IsAllowed(ByVal UserID As Integer) As Boolean
            Return If(IsTerminalNode, True, Hierarchy.ProjectManager.UsersRoles.IsAllowedObjective(NodeGuidID, UserID))
        End Function

        Public Function AllowedNodesBelow(ByVal UserID As Integer) As List(Of clsNode)
            Dim altsList As New List(Of clsNode)

            If IsTerminalNode Then
                For Each alt As clsNode In Hierarchy.ProjectManager.AltsHierarchy(Hierarchy.ProjectManager.ActiveAltsHierarchy).TerminalNodes
                    If Hierarchy.ProjectManager.UsersRoles.IsAllowedAlternative(NodeGuidID, alt.NodeGuidID, UserID) Then
                        altsList.Add(alt)
                    End If
                Next
            End If

            Return altsList
        End Function

        Public Function RestrictedNodesBelow(ByVal UserID As Integer) As HashSet(Of clsNode) 'C0901
            Dim alts As New HashSet(Of clsNode)

            If IsTerminalNode Then
                Dim altsGuids As HashSet(Of Guid) = Hierarchy.ProjectManager.UsersRoles.GetRestrictedAlternatives(UserID, NodeGuidID)
                For Each altID As Guid In altsGuids
                    Dim alt As clsNode = Hierarchy.ProjectManager.ActiveAlternatives.GetNodeByID(altID)
                    alts.Add(alt)
                Next
            End If

            Return alts
        End Function

        ' D4213 ===
        Public Function isUncontributedAlternative() As Boolean 'A1408
            If IsAlternative AndAlso Hierarchy IsNot Nothing AndAlso Hierarchy.ProjectManager IsNot Nothing AndAlso Hierarchy.ProjectManager.IsRiskProject Then
                If Hierarchy.GetUncontributedAlternatives.Contains(Me) Then Return True
            End If
            Return False
        End Function

        Public ReadOnly Property Index As String
            Get
                Dim sIndex As String = ""
                If IsAlternative AndAlso Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes IsNot Nothing Then
                    ' D4323 ===
                    Dim tMode As IDColumnModes = IDColumnModes.IndexID
                    If Hierarchy.ProjectManager IsNot Nothing AndAlso TypeOf (Hierarchy.ProjectManager) Is clsProjectManager Then
                        With CType(Hierarchy.ProjectManager, clsProjectManager).Parameters
                            tMode = .NodeVisibleIndexMode
                            If .NodeIndexIsVisible Then 'A1342
                                sIndex = iIndex
                                ' D4323 ==
                                Dim tMax As Integer = 1
                                Select Case tMode
                                    Case IDColumnModes.IndexID, IDColumnModes.Rank
                                        tMax = Hierarchy.Nodes.Count
                                    Case IDColumnModes.UniqueID
                                        tMax = Hierarchy.GetNextNodeID()
                                    Case IDColumnModes.Rank
                                        tMax = Hierarchy.GetNextNodeID()
                                End Select
                                sIndex = sIndex.PadLeft(tMax.ToString.Length, CChar("0"))
                                ' D4323 ===
                                If clsProjectParametersWithDefaults.NodeIndexShowWithFormatting Then
                                    Select Case tMode
                                        Case IDColumnModes.IndexID
                                            'sIndex = String.Format("#{0}", sIndex)
                                            sIndex = String.Format("{0}", sIndex)
                                        Case IDColumnModes.UniqueID
                                            sIndex = String.Format("[{0}]", sIndex)
                                        Case IDColumnModes.Rank
                                            sIndex = String.Format("{0}", sIndex)
                                    End Select
                                End If
                            End If
                        End With
                        ' D4323 ==
                    End If
                End If
                Return sIndex
            End Get
        End Property

        Public ReadOnly Property iIndex As Integer 'A1390
            Get
                If IsAlternative AndAlso Hierarchy IsNot Nothing AndAlso Hierarchy.Nodes IsNot Nothing Then
                    Dim tMode As IDColumnModes = IDColumnModes.IndexID
                    If Hierarchy.ProjectManager IsNot Nothing AndAlso TypeOf (Hierarchy.ProjectManager) Is clsProjectManager Then
                        With CType(Hierarchy.ProjectManager, clsProjectManager).Parameters
                            tMode = .NodeVisibleIndexMode
                            If .NodeIndexIsVisible Then
                                Select Case tMode
                                    Case IDColumnModes.IndexID
                                        Return Hierarchy.Nodes.IndexOf(Me) + 1
                                    Case IDColumnModes.UniqueID
                                        Return NodeID + 1
                                    Case IDColumnModes.Rank
                                        Return If(NodeRank <= 0, Hierarchy.Nodes.IndexOf(Me) + 1, NodeRank)
                                End Select
                            End If
                        End With
                    End If
                End If
                Return -1
            End Get
        End Property

        Public ReadOnly Property EnabledChildren(Optional ByVal UserID As Integer = UNDEFINED_USER_ID) As List(Of clsNode) 'C0384
            Get
                Dim res As New List(Of clsNode) 'C0384
                For Each node As clsNode In Children
                    If node.Enabled AndAlso ((UserID = UNDEFINED_USER_ID) OrElse (UserID <> UNDEFINED_USER_ID) AndAlso Not node.DisabledForUser(UserID)) Then 'C0211
                        res.Add(node)
                    End If
                Next
                Return res
            End Get
        End Property

        Public Function GetChildIndexByID(ByVal NodeID As Integer, NodesList As List(Of clsNode), Optional ByVal UserID As Integer = UNDEFINED_USER_ID) As Integer 'C0450
            'Dim nodesList As List(Of clsNode) = GetNodesBelow(UserID)

            If NodesList IsNot Nothing Then
                For i As Integer = 0 To NodesList.Count - 1
                    If NodesList(i).NodeID = NodeID Then
                        Return i
                    End If
                Next
            End If

            Return -1
        End Function

        Public ReadOnly Property IsTerminalNode() As Boolean
            Get
                Return Children.Count = 0
            End Get
        End Property

        Public Property MeasureType(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As ECMeasureType 'C0075
            Get
                Return mMeasureType
            End Get
            Set(ByVal value As ECMeasureType)
                If mMeasureType <> value Then
                    If Not (mMeasureType = ECMeasureType.mtPairwise And value = ECMeasureType.mtPWAnalogous Or
                        mMeasureType = ECMeasureType.mtPWAnalogous And value = ECMeasureType.mtPairwise) Then

                        If ErasePreviousJudgmentsOnChange Then
                            Judgments.Weights.ClearUserWeights()
                            Judgments.ClearCombinedJudgments()
                        End If
                        Judgments = Nothing
                        DirectJudgmentsForNoCause = Nothing
                        If value = ECMeasureType.mtPairwise Or value = ECMeasureType.mtPWAnalogous Or value = ECMeasureType.mtPWOutcomes Then
                            Judgments = New clsPairwiseJudgments(Me)
                        Else
                            Judgments = New clsNonPairwiseJudgments(Me)
                            DirectJudgmentsForNoCause = New clsNonPairwiseJudgments(Me)
                        End If
                    End If


                    mMeasureType = value

                    Select Case mMeasureType
                        Case ECMeasureType.mtRatings
                            Dim rs As clsRatingScale
                            If Hierarchy.ProjectManager.IsRiskProject Then
                                'If IsAlternative OrElse Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                                If IsAlternative OrElse Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                                        rs = Hierarchy.ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stLikelihood)
                                        If rs Is Nothing Then
                                        rs = Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByName("WIDE LIKELIHOOD RATING SCALE")
                                    End If
                                Else
                                    rs = Hierarchy.ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stImpact)
                                End If
                            Else
                                rs = Hierarchy.ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stShared)
                            End If
                            If rs Is Nothing Then rs = Hierarchy.GetDefaultRatingScale()
                            If rs IsNot Nothing Then
                                RatingScaleID(False) = rs.ID
                            End If
                        Case ECMeasureType.mtStep
                            Dim sf As clsStepFunction
                            If Hierarchy.ProjectManager.IsRiskProject Then
                                If Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                                    sf = Hierarchy.ProjectManager.MeasureScales.GetDefaultStepFunction(ScaleType.stLikelihood)
                                Else
                                    sf = Hierarchy.ProjectManager.MeasureScales.GetDefaultStepFunction(ScaleType.stImpact)
                                End If
                            Else
                                sf = Hierarchy.ProjectManager.MeasureScales.GetDefaultStepFunction(ScaleType.stShared)
                            End If
                            If sf IsNot Nothing Then
                                StepFunctionID(False) = sf.ID
                            End If
                        Case ECMeasureType.mtRegularUtilityCurve
                            Dim uc As clsRegularUtilityCurve
                            If Hierarchy.ProjectManager.IsRiskProject Then
                                If Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                                    uc = Hierarchy.ProjectManager.MeasureScales.GetDefaultRegularUtilityCurve(ScaleType.stLikelihood)
                                Else
                                    uc = Hierarchy.ProjectManager.MeasureScales.GetDefaultRegularUtilityCurve(ScaleType.stImpact)
                                End If
                            Else
                                uc = Hierarchy.ProjectManager.MeasureScales.GetDefaultRegularUtilityCurve(ScaleType.stShared)
                            End If
                            If uc IsNot Nothing Then
                                RegularUtilityCurveID(False) = uc.ID
                            End If
                    End Select

                    ResetClusterPhrase()
                End If
                'C0450==
            End Set
        End Property

        Public Property FeedbackMeasureType(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As ECMeasureType 'C0075
            Get
                Return mFeedbackMeasureType
            End Get
            Set(ByVal value As ECMeasureType)
                If Not IsAlternative Then
                    mFeedbackMeasureType = value
                    Exit Property
                End If

                If mFeedbackMeasureType <> value Then
                    If Not (mFeedbackMeasureType = ECMeasureType.mtPairwise And value = ECMeasureType.mtPWAnalogous Or
                        mFeedbackMeasureType = ECMeasureType.mtPWAnalogous And value = ECMeasureType.mtPairwise) Then
                        FeedbackJudgments = Nothing
                        If value = ECMeasureType.mtPairwise Or value = ECMeasureType.mtPWAnalogous Or value = ECMeasureType.mtPWOutcomes Then
                            FeedbackJudgments = New clsPairwiseJudgments(Me)
                        Else
                            FeedbackJudgments = New clsNonPairwiseJudgments(Me)
                        End If
                    End If


                    mFeedbackMeasureType = value

                    ResetClusterPhrase()
                End If
            End Set
        End Property

        Public Sub ResetClusterPhrase()
            If Hierarchy.ProjectManager.Attributes IsNot Nothing AndAlso Hierarchy.ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_CLUSTER_PHRASE_ID) IsNot Nothing Then
                Dim attrValue As Object = Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_ID, NodeGuidID)
                If attrValue IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(attrValue)) Then
                    Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, Nothing, NodeGuidID, Guid.Empty)
                End If
            End If

            If Hierarchy.ProjectManager.Attributes IsNot Nothing AndAlso Hierarchy.ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID) IsNot Nothing Then
                Dim attrValue As Object = Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID, NodeGuidID)
                If attrValue IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(attrValue)) Then
                    Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_CLUSTER_PHRASE_MULTI_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, Nothing, NodeGuidID, Guid.Empty)
                End If
            End If

            ' D4133 ===
            If Hierarchy.ProjectManager.Attributes IsNot Nothing AndAlso Hierarchy.ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_ID) IsNot Nothing Then
                Dim attrValue As Object = Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_ID, NodeGuidID)
                If attrValue IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(attrValue)) Then
                    Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, Nothing, NodeGuidID, Guid.Empty)
                End If
            End If

            If Hierarchy.ProjectManager.Attributes IsNot Nothing AndAlso Hierarchy.ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_MULTI_ID) IsNot Nothing Then
                Dim attrValue As Object = Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_MULTI_ID, NodeGuidID)
                If attrValue IsNot Nothing AndAlso Not String.IsNullOrEmpty(CStr(attrValue)) Then
                    Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_CTRLS_CLUSTER_PHRASE_MULTI_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, Nothing, NodeGuidID, Guid.Empty)
                End If
            End If
            ' D4133 ==
        End Sub

        'A1086 ===
        Public Function GetElementsCountString() As String
            Dim retVal As String = ""

            If IsAlternative Then 'event with no sources
                If MeasureType = ECMeasureType.mtPWOutcomes Then
                    retVal = String.Format("{0}, {1}", 1, CType(MeasurementScale, ECCore.clsRatingScale).RatingSet.Count)
                Else
                    retVal = "1"
                End If
            Else
                Dim nodesBelow As List(Of ECCore.clsNode) = GetNodesBelow(UNDEFINED_USER_ID).Where(Function (tNode) tNode.RiskNodeType <> RiskNodeType.ntCategory).ToList
                If MeasureType = ECMeasureType.mtPWOutcomes Then
                    retVal = String.Format("{0}, {1}", nodesBelow.Count, CType(MeasurementScale, ECCore.clsRatingScale).RatingSet.Count)
                Else
                    retVal = CStr(nodesBelow.Count)
                End If
            End If

            Return If(retVal = "0", "", retVal)
        End Function

        Public Function GetJudgmentsCountString(ByVal DiagEval As ECCore.DiagonalsEvaluation, ByRef jc As Integer, ByRef tooltip As String) As String
            Dim string_representation As String = ""
            jc = 0 'judgment count

            Dim nodesBelow As List(Of ECCore.clsNode) = GetNodesBelow(UNDEFINED_USER_ID).Where(Function (tNode) tNode.RiskNodeType <> RiskNodeType.ntCategory).ToList

            Dim n As Integer = 0
            If IsAlternative Then 'event with no sources
                n = 1
            Else
                n = nodesBelow.Count()
            End If

            Dim nc As String = CStr(If(IsTerminalNode, "ne", "nc"))
            Dim nc_full As String = CStr(If(IsTerminalNode, "%%alternatives%%", "children"))

            Select Case MeasureType
                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                    Select Case DiagEval
                        Case DiagonalsEvaluation.deAll
                            tooltip = String.Format("{0}({0}-1)/2, where {0} = number of {1}", nc, nc_full)
                            jc = CInt(n * (n - 1) / 2)
                            string_representation = String.Format("{0}*({0}-1)/2 = {1}", n, jc)
                        Case DiagonalsEvaluation.deFirst
                            tooltip = String.Format("{0}-1, where {0} = number of {1}", nc, nc_full)
                            jc = n - 1
                            string_representation = String.Format("{0}-1 = {1}", n, jc)
                        Case DiagonalsEvaluation.deFirstAndSecond
                            tooltip = String.Format("({0}-1)+({0}-2), where {0} = number of {1}", nc, nc_full)
                            jc = (n - 1) + (n - 2)
                            string_representation = String.Format("({0}-1)+({0}-2) = {1}", n, jc)
                    End Select
                Case ECMeasureType.mtPWOutcomes
                    Dim rcount As Integer = CType(MeasurementScale, ECCore.clsRatingScale).RatingSet.Count
                    Select Case DiagEval
                        Case DiagonalsEvaluation.deAll
                            jc = CInt((rcount * (rcount - 1) / 2))
                            string_representation = String.Format("{0}*({0}-1)/2 * {1} = {2}", rcount, n, jc * n)
                            tooltip = String.Format("p(p-1)/2 * {0}, where p = number of probabilities; {0} = number of {1}", nc, nc_full)
                        Case DiagonalsEvaluation.deFirst
                            jc = rcount - 1
                            string_representation = String.Format("({0}-1) * {1} = {2}", rcount, n, jc * n)
                            tooltip = String.Format("(p - 1) * {0}, where p = number of probabilities; {0} = number of {1}", nc, nc_full)
                        Case DiagonalsEvaluation.deFirstAndSecond
                            jc = (rcount - 1) + (rcount - 2)
                            string_representation = String.Format("(({0}-1)+({0}-2)) * {1} = {2}", rcount, n, jc * n)
                            tooltip = String.Format("((p - 1) + (p - 2)) * {0}, where p = number of probabilities; {0} = number of {1}", nc, nc_full)
                    End Select
                    'string_representation = String.Format("{0} * {1}", retVal, n)
                    jc = n * jc
                Case Else
                    string_representation = String.Format("{0}", n)
                    tooltip = String.Format("{0}, where {0} = number of {1}", nc, nc_full)
                    jc = n
            End Select

            If jc <= 0 Then
                jc = 0
                string_representation = ""
                tooltip = ""
            End If

            Return string_representation
        End Function
        'A1086 ==

        Public Sub New(Optional ByVal Hierarchy As clsHierarchy = Nothing)
            Me.Hierarchy = Hierarchy
            mMeasureType = ECMeasureType.mtPairwise

            mRatingScaleID = -1
            mRegularUtilityCurveID = -1
            mAdvancedUtilityCurveID = -1

            mRandomChildren = New List(Of clsNode)
            mUseRandom = False

            mFeedbackMeasureType = ECMeasureType.mtDirect
        End Sub

        Public Function GetContributedAlternatives() As List(Of clsNode)
            Return If(IsTerminalNode, GetNodesBelow(UNDEFINED_USER_ID), New List(Of clsNode))

            If Hierarchy.HierarchyType <> ECHierarchyType.htModel Then Return Nothing

            If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso
               Hierarchy.Nodes.Count = 1 Then
                Return New List(Of clsNode)
            End If

            Dim usingDefaultContribution As Boolean = Hierarchy.IsUsingDefaultFullContribution

            If Hierarchy.AltsDefaultContribution = ECAltsDefaultContribution.adcFull And usingDefaultContribution Then
                Return Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes
            Else
                Return Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes.Where(Function(alt) ChildrenAlts.Contains(alt.NodeID)).ToList
            End If
        End Function

        Public Function GetVisibleNodesBelow(ByVal UserID As Integer) As List(Of clsNode) 'C0786
            Dim useCombined As Boolean = Hierarchy.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes

            If IsCombinedUserID(UserID) Then
                Return GetNodesBelow(UNDEFINED_USER_ID)
            Else
                Dim userNodes As List(Of clsNode) = GetNodesBelow(UserID)
                Dim allNodes As List(Of clsNode) = GetNodesBelow(UNDEFINED_USER_ID)

                If IsAllowed(UserID) Then
                    If IsTerminalNode Then
                        If useCombined And (userNodes.Count <> allNodes.Count) Then
                            Return allNodes
                        Else
                            Return userNodes
                        End If
                    Else
                        Return userNodes
                    End If
                Else
                    Return If(useCombined, allNodes, userNodes)
                End If
            End If
        End Function

        'Public Function GetNodesBelow(ByVal UserID As Integer, Optional ByVal Randomize As Boolean = False, Optional ByVal RandomSeed As Integer = -1, Optional OriginalUserID As Integer = -1, Optional CreateCopy As Boolean = False) As List(Of clsNode) 'C0450
        '    Dim NB As clsNodesBelowForUser = GetNodesBelowForUser(UserID)
        '    If False AndAlso (NB IsNot Nothing And OriginalUserID = -1) Then
        '        If CreateCopy Then
        '            Dim res As New List(Of clsNode)
        '            res.AddRange(NB.Nodes)
        '            Return res
        '        Else
        '            Return NB.Nodes
        '        End If
        '    Else
        '        If IsTerminalNode Then
        '            Dim alts As New List(Of clsNode)

        '            If Hierarchy.ProjectManager.IsRiskProject And Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood And
        '                Hierarchy.Nodes.Count = 1 Then
        '                Return alts
        '            End If

        '            Select Case Hierarchy.HierarchyType
        '                Case ECHierarchyType.htModel
        '                    Dim aAlts As List(Of clsNode) = If(OriginalUserID = -1, Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes, Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes.Where(Function(a) (Not a.DisabledForUser(OriginalUserID))).ToList).Where(Function(n) (n.Enabled)).ToList
        '                    Dim bAlts As List(Of clsNode) = If((Hierarchy.HierarchyID = ECHierarchyID.hidImpact AndAlso Hierarchy.Nodes.Count = 1) OrElse ((Hierarchy.AltsDefaultContribution = ECAltsDefaultContribution.adcFull) AndAlso Hierarchy.IsUsingDefaultFullContribution), aAlts, aAlts.Where(Function(a) ChildrenAlts.Contains(a.NodeID))).ToList
        '                    alts = If(UserID = UNDEFINED_USER_ID Or IsCombinedUserID(UserID), bAlts, bAlts.Where(Function(a) (Not a.DisabledForUser(UserID) AndAlso (Hierarchy.ProjectManager.UsersRoles.IsAllowedAlternative(NodeGuidID, a.NodeGuidID, UserID)))).ToList)
        '            End Select

        '            ClearCalculatedNodesBelow(UserID)

        '            NB = New clsNodesBelowForUser(UserID)
        '            NB.Nodes.AddRange(alts.ToArray)
        '            mNodesBelow.Add(NB)


        '            Dim res As New List(Of clsNode)
        '            res.AddRange(NB.Nodes)
        '            Return res
        '        Else
        '            Dim enChildren As List(Of clsNode)
        '            If OriginalUserID = -1 Then
        '                enChildren = EnabledChildren(UserID)
        '            Else
        '                enChildren = EnabledChildren(OriginalUserID)
        '            End If

        '            If mUseRandom Or Randomize And (enChildren.Count > 0) Then
        '                If (mRandomChildren.Count = enChildren.Count) Then
        '                    NB = New clsNodesBelowForUser(UserID)
        '                    NB.Nodes.AddRange(mRandomChildren.ToArray)
        '                    mNodesBelow.Add(NB)
        '                    Return NB.Nodes
        '                Else
        '                    Dim mRandom As New Random(RandomSeed)

        '                    Dim rInts As New ArrayList
        '                    Dim i As Integer

        '                    While rInts.Count < enChildren.Count
        '                        i = mRandom.Next(0, enChildren.Count)
        '                        If Not rInts.Contains(i) Then
        '                            rInts.Add(i)
        '                        End If
        '                    End While

        '                    mRandomChildren.Clear()
        '                    For i = 0 To rInts.Count - 1
        '                        mRandomChildren.Add(enChildren(rInts(i)))
        '                    Next

        '                    mUseRandom = True

        '                    'Return mRandomChildren.Clone 'C0384

        '                    'Return mRandomChildren 'C0384 'C0700

        '                    'C0700===
        '                    NB = New clsNodesBelowForUser(UserID)
        '                    NB.Nodes.AddRange(mRandomChildren.ToArray)
        '                    mNodesBelow.Add(NB)
        '                    Return NB.Nodes
        '                End If
        '            Else
        '                NB = New clsNodesBelowForUser(UserID)
        '                NB.Nodes.AddRange(enChildren.ToArray)
        '                mNodesBelow.Add(NB)
        '                Return NB.Nodes
        '            End If
        '        End If
        '    End If
        'End Function

        Public Overloads Function GetNodesBelowHS(ByVal UserID As Integer, Optional OriginalUserID As Integer = -1, Optional IncludeCategorical As Boolean = True) As HashSet(Of Integer)
            Dim res As New HashSet(Of Integer)

            Dim list As List(Of clsNode) = GetNodesBelow(UserID,,, OriginalUserID,, IncludeCategorical)
            For Each node As clsNode In list
                res.Add(node.NodeID)
            Next

            Return res
        End Function

        'Public Overloads Function GetNodesBelowHS(ByVal UserID As Integer, IncludeCategorical As Boolean) As HashSet(Of clsNode)
        '    If IncludeCategorical Then Return GetNodesBelowHS(UserID)
        '    Dim res As New HashSet(Of clsNode)

        '    Dim list As List(Of clsNode) = GetNodesBelow(UserID)
        '    For Each node As clsNode In list
        '        If IncludeCategorical OrElse (node.Hierarchy.ProjectManager.IsRiskProject AndAlso node.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso node.RiskNodeType <> RiskNodeType.ntCategory) Then
        '            res.Add(node)
        '        End If
        '    Next

        '    Return res
        'End Function

        Public Function GetNodesBelow(ByVal UserID As Integer, Optional ByVal Randomize As Boolean = False, Optional ByVal RandomSeed As Integer = -1, Optional OriginalUserID As Integer = -1, Optional CreateCopy As Boolean = False, Optional IncludeCategorical As Boolean = True) As List(Of clsNode) 'C0450
            Dim res As New List(Of clsNode)
            If IsTerminalNode Then
                Dim alts As New List(Of clsNode)

                If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood And
                    Hierarchy.Nodes.Count = 1 Then
                    Return alts
                End If

                Select Case Hierarchy.HierarchyType
                    Case ECHierarchyType.htModel
                        'Dim aAlts As List(Of clsNode) = If(OriginalUserID = -1, Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes, Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes.Where(Function(a) (Not a.DisabledForUser(OriginalUserID))).ToList).Where(Function(n) (n.Enabled)).ToList
                        Dim aAlts As List(Of clsNode) = If(OriginalUserID = -1, Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes, Hierarchy.ProjectManager.ActiveAlternatives.TerminalNodes.Where(Function(a) Not a.DisabledForUser(OriginalUserID) AndAlso a.Enabled).ToList)
                        Dim bAlts As List(Of clsNode) = If((Hierarchy.HierarchyID = ECHierarchyID.hidImpact AndAlso Hierarchy.Nodes.Count = 1) OrElse ((Hierarchy.AltsDefaultContribution = ECAltsDefaultContribution.adcFull) AndAlso Hierarchy.IsUsingDefaultFullContribution), aAlts, aAlts.Where(Function(a) ChildrenAlts.Contains(a.NodeID))).ToList
                        alts = If(UserID = UNDEFINED_USER_ID OrElse IsCombinedUserID(UserID), bAlts, bAlts.Where(Function(a) (Not a.DisabledForUser(UserID) AndAlso (Hierarchy.ProjectManager.UsersRoles.IsAllowedAlternative(NodeGuidID, a.NodeGuidID, UserID)))).ToList)
                End Select

                If Not IncludeCategorical AndAlso Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                    alts.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
                End If

                res.AddRange(alts.ToArray)
                Return res
            Else
                Dim enChildren As List(Of clsNode)
                If OriginalUserID = -1 Then
                    enChildren = EnabledChildren(UserID)
                Else
                    enChildren = EnabledChildren(OriginalUserID)
                End If

                If mUseRandom Or Randomize And (enChildren.Count > 0) Then
                    If (mRandomChildren.Count = enChildren.Count) Then
                        res.AddRange(mRandomChildren.ToArray)
                        Return res
                    Else
                        Dim mRandom As New Random(RandomSeed)

                        Dim rInts As New ArrayList
                        Dim i As Integer

                        While rInts.Count < enChildren.Count
                            i = mRandom.Next(0, enChildren.Count)
                            If Not rInts.Contains(i) Then
                                rInts.Add(i)
                            End If
                        End While

                        mRandomChildren.Clear()
                        For i = 0 To rInts.Count - 1
                            mRandomChildren.Add(enChildren(rInts(i)))
                        Next

                        mUseRandom = True

                        'Return mRandomChildren.Clone 'C0384

                        'Return mRandomChildren 'C0384 'C0700

                        res.AddRange(mRandomChildren.ToArray)
                        Return res
                    End If
                Else
                    If Not IncludeCategorical AndAlso Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                        enChildren.RemoveAll(Function(n) (n.RiskNodeType = RiskNodeType.ntCategory))
                    End If

                    res.AddRange(enChildren.ToArray)
                    Return res
                End If
            End If
        End Function

        Public Function GetNodeDescendants(ByVal node As clsNode, ByRef tDescendants As List(Of clsNode)) As List(Of clsNode) 'AS/4488c

            Dim res As New List(Of clsNode)
            If Not node.IsTerminalNode Then
                res = node.GetNodesBelow(UNDEFINED_USER_ID)
            End If

            For Each tNode As clsNode In res
                tDescendants.Add(tNode)
                If Not node.IsTerminalNode Then GetNodeDescendants(tNode, tDescendants)
            Next

            Return tDescendants
        End Function

        Public Sub DeleteJudgmentsWithChild(ByVal ChildID As Integer)
            Dim pwData As clsPairwiseMeasureData
            Dim nonpwData As clsNonPairwiseMeasureData

            'C0405===
            Dim uJudgments As List(Of clsCustomMeasureData)
            For Each U As clsUser In Hierarchy.ProjectManager.UsersList
                uJudgments = Judgments.UsersJudgments(U.UserID)
                For i As Integer = uJudgments.Count - 1 To 0 Step -1
                    Select Case MeasureType
                        Case ECMeasureType.mtPairwise
                            pwData = uJudgments(i)
                            If (pwData.FirstNodeID = ChildID) Or (pwData.SecondNodeID = ChildID) Then
                                uJudgments.RemoveAt(i)
                            End If
                            'TODO: (for future) Handle new measurement types here
                        Case ECMeasureType.mtRatings, ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtStep, ECMeasureType.mtDirect, ECMeasureType.mtAdvancedUtilityCurve 'C0026
                            nonpwData = uJudgments(i)
                            If (nonpwData.NodeID = ChildID) Then
                                uJudgments.RemoveAt(i)
                            End If
                    End Select
                Next
            Next
        End Sub

        'A1063 ===
        Public Function AllChildrenCategories() As Boolean
            Return Children IsNot Nothing AndAlso Children.Count > 0 AndAlso Children.Count = Children.FindAll(Function(node As clsNode) node.RiskNodeType = ECTypes.RiskNodeType.ntCategory).Count
        End Function
        'A1063 ==

        Public Property RiskNodeType() As RiskNodeType
            Get
                Return If(Hierarchy Is Nothing, RiskNodeType.ntUncertainty, If(Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood, Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_NODE_TYPE_ID, NodeGuidID), RiskNodeType.ntUncertainty))
            End Get
            Set(value As RiskNodeType)
                Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_NODE_TYPE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, CInt(value), NodeGuidID, Nothing)
            End Set
        End Property

        'A1339 ===
        Private _RiskEventSortOrder As Long = -1
        Public Property RiskEventSortOrder() As Long
            Get
                If _RiskEventSortOrder = -1 AndAlso Hierarchy IsNot Nothing Then
                    _RiskEventSortOrder = Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_EVENT_SORT_ORDER_ID, NodeGuidID)
                End If
                Return _RiskEventSortOrder
            End Get
            Set(value As Long)
                _RiskEventSortOrder = value
                Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_EVENT_SORT_ORDER_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, NodeGuidID, Guid.Empty)
            End Set
        End Property 'A1339 ==

        Public ReadOnly Property NodeSynthesisType() As Integer ' 0 - sum, 1 - max
            Get
                Return If(Hierarchy IsNot Nothing, CInt(Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_NODE_SYNTHESIS_TYPE_ID, NodeGuidID)), 0)
            End Get
        End Property

        Public ReadOnly Property StatisticalData As List(Of clsStatisticalDataItem)
            Get
                Dim retVal As List(Of clsStatisticalDataItem) = Nothing
                Dim sData As String = CStr(Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RISK_NODE_STATICTICAL_DATA_ID, NodeGuidID))
                If Not String.IsNullOrEmpty(sData) Then
                    Dim serializer As New JavaScriptSerializer
                    retVal = serializer.Deserialize(sData, GetType(List(Of clsStatisticalDataItem)))
                End If
                Return retVal
            End Get
        End Property

        Public Property IsStructuralAdjust() As Boolean?
            Get
                'Return If(Hierarchy IsNot Nothing, CBool(Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_IS_STRUCTURAL_ADJUST_NODE_ID, NodeGuidID)), False)
                Dim retVal As Boolean = CBool(Hierarchy.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_IS_STRUCTURAL_ADJUST_NODE_ID, NodeGuidID))
                Dim hasGrandChildren As Boolean = Children IsNot Nothing AndAlso Children.Count > 0 AndAlso Children.Where(Function(child) child.Children IsNot Nothing AndAlso child.Children.Count > 0).Count > 0
                If Not hasGrandChildren Then Return Nothing
                Return retVal
            End Get
            Set(value As Boolean?)
                Hierarchy.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_IS_STRUCTURAL_ADJUST_NODE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, value, NodeGuidID, Nothing)
            End Set
        End Property

        Public Function GetCategoriesCountInCluster() As Integer
            If Hierarchy.ProjectManager.IsRiskProject And Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood Then
                Return Children.LongCount(Function(child) (child.RiskNodeType = ECTypes.RiskNodeType.ntCategory))
            Else
                Return 0
            End If
        End Function

        Public Function NodePath(Optional ByVal IncludeGoalName As Boolean = False) As String
            Dim tPath As String = ""
            If ParentNode Is Nothing AndAlso Not IncludeGoalName Then Return ""
            If ParentNode IsNot Nothing Then tPath = ParentNode.NodePath
            Return If(String.IsNullOrEmpty(tPath), Me.NodeName, tPath + " \ " + Me.NodeName)
        End Function

        Public Function IsNodeIncludedInFilter(tCurrentFiltersList As List(Of clsFilterItem)) As Boolean
            Dim retVal As Boolean = True

            If tCurrentFiltersList IsNot Nothing AndAlso tCurrentFiltersList.Count > 0 Then
                retVal = False
                Dim PM As clsProjectManager = Hierarchy.ProjectManager

                Dim filterOp As FilterCombinations = FilterCombinations.fcAnd
                filterOp = tCurrentFiltersList(0).FilterCombination

                Dim tNoneRuleChecked As Boolean = True

                For Each cfi As clsFilterItem In tCurrentFiltersList
                    If cfi.FilterOperation <> FilterOperations.None AndAlso cfi.IsChecked Then
                        tNoneRuleChecked = False

                        Dim isCurrentRulePassed As Boolean = False

                        Dim tAttribute = PM.Attributes.GetAttributeByID(cfi.SelectedAttributeID)
                        Dim ObjValue As Object = PM.Attributes.GetAttributeValue(cfi.SelectedAttributeID, Me.NodeGuidID)

                        If ObjValue IsNot Nothing Then
                            Select Case tAttribute.ValueType
                                Case AttributeValueTypes.avtString
                                    Dim StrValue As String = CStr(ObjValue).ToLower
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Contains
                                            If StrValue.Contains(cfi.FilterText.ToString.ToLower) Then isCurrentRulePassed = True
                                        Case FilterOperations.Equal
                                            If StrValue.Trim = cfi.FilterText.ToString.ToLower.Trim Then isCurrentRulePassed = True
                                        Case FilterOperations.NotEqual
                                            If StrValue.Trim <> cfi.FilterText.ToString.ToLower.Trim Then isCurrentRulePassed = True
                                        Case FilterOperations.StartsWith
                                            If StrValue.Trim.StartsWith(cfi.FilterText.ToString.ToLower.Trim) Then isCurrentRulePassed = True
                                    End Select
                                Case AttributeValueTypes.avtBoolean
                                    Dim Value As Boolean = CBool(ObjValue)
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.IsTrue
                                            If Value Then isCurrentRulePassed = True
                                        Case FilterOperations.IsFalse
                                            If Not Value Then isCurrentRulePassed = True
                                    End Select
                                Case AttributeValueTypes.avtDouble
                                    Dim Value, FilterDouble As Double
                                    Value = CDbl(ObjValue)
                                    If ExpertChoice.Service.StringFuncs.String2Double(CStr(cfi.FilterText), FilterDouble) Then
                                        Select Case cfi.FilterOperation
                                            Case FilterOperations.Equal
                                                If Value = FilterDouble Then isCurrentRulePassed = True
                                            Case FilterOperations.NotEqual
                                                If Value <> FilterDouble Then isCurrentRulePassed = True
                                            Case FilterOperations.GreaterThan
                                                If Value > FilterDouble Then isCurrentRulePassed = True
                                            Case FilterOperations.GreaterThanOrEqual
                                                If Value >= FilterDouble Then isCurrentRulePassed = True
                                            Case FilterOperations.LessThan
                                                If Value < FilterDouble Then isCurrentRulePassed = True
                                            Case FilterOperations.LessThanOrequal
                                                If Value <= FilterDouble Then isCurrentRulePassed = True
                                        End Select
                                    Else
                                        If cfi.FilterOperation = FilterOperations.NotEqual Then isCurrentRulePassed = True
                                    End If
                                Case AttributeValueTypes.avtLong
                                    Dim Value, FilterLong As Long
                                    Value = CLng(ObjValue)
                                    If Long.TryParse(CStr(cfi.FilterText), FilterLong) Then
                                        Select Case cfi.FilterOperation
                                            Case FilterOperations.Equal
                                                If Value = FilterLong Then isCurrentRulePassed = True
                                            Case FilterOperations.NotEqual
                                                If Value <> FilterLong Then isCurrentRulePassed = True
                                            Case FilterOperations.GreaterThan
                                                If Value > FilterLong Then isCurrentRulePassed = True
                                            Case FilterOperations.GreaterThanOrEqual
                                                If Value >= FilterLong Then isCurrentRulePassed = True
                                            Case FilterOperations.LessThan
                                                If Value < FilterLong Then isCurrentRulePassed = True
                                            Case FilterOperations.LessThanOrequal
                                                If Value <= FilterLong Then isCurrentRulePassed = True
                                        End Select
                                    Else
                                        If cfi.FilterOperation = FilterOperations.NotEqual Then isCurrentRulePassed = True
                                    End If
                                Case AttributeValueTypes.avtEnumeration
                                    Dim tEnumID As Guid = CType(ObjValue, Guid)
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Equal
                                            If tEnumID.Equals(cfi.FilterEnumItemID) Then isCurrentRulePassed = True
                                        Case FilterOperations.NotEqual
                                            If Not tEnumID.Equals(cfi.FilterEnumItemID) Then isCurrentRulePassed = True
                                    End Select
                                Case AttributeValueTypes.avtEnumerationMulti
                                    Dim tEnumIDs As String = CStr(ObjValue)
                                    Select Case cfi.FilterOperation
                                        Case FilterOperations.Contains
                                            If Not String.IsNullOrEmpty(tEnumIDs) AndAlso cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                                Dim tContainsAll As Boolean = cfi.FilterEnumItemsIDs.Count > 0
                                                For Each value As Guid In cfi.FilterEnumItemsIDs
                                                    If Not tEnumIDs.Contains(value.ToString) Then
                                                        tContainsAll = False
                                                        Exit For
                                                    End If
                                                Next
                                                If tContainsAll Then isCurrentRulePassed = True
                                            End If
                                        Case FilterOperations.Equal
                                            If Not String.IsNullOrEmpty(tEnumIDs) AndAlso cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                                Dim tEqual As Boolean = cfi.FilterEnumItemsIDs.Count > 0 AndAlso cfi.FilterEnumItemsIDs.Count = tEnumIDs.Split(CChar(";")).Length
                                                For Each value As Guid In cfi.FilterEnumItemsIDs
                                                    If Not tEnumIDs.Contains(value.ToString) Then
                                                        tEqual = False
                                                        Exit For
                                                    End If
                                                Next
                                                If tEqual Then isCurrentRulePassed = True
                                            End If
                                        Case FilterOperations.NotEqual
                                            If cfi.FilterEnumItemsIDs IsNot Nothing AndAlso cfi.FilterEnumItemsIDs.Count > 0 Then
                                                If Not String.IsNullOrEmpty(tEnumIDs) Then
                                                    Dim tEqual As Boolean = cfi.FilterEnumItemsIDs.Count > 0 AndAlso cfi.FilterEnumItemsIDs.Count = tEnumIDs.Split(CChar(";")).Length
                                                    For Each value As Guid In cfi.FilterEnumItemsIDs
                                                        If Not tEnumIDs.Contains(value.ToString) Then
                                                            tEqual = False
                                                            Exit For
                                                        End If
                                                    Next
                                                    If Not tEqual Then isCurrentRulePassed = True
                                                Else
                                                    isCurrentRulePassed = True
                                                End If
                                            Else
                                                If Not String.IsNullOrEmpty(tEnumIDs) Then isCurrentRulePassed = True
                                            End If
                                    End Select
                            End Select
                        End If

                        'apply set
                        Dim indexOfFirstCheckedRule As Integer = 0
                        For i As Integer = 0 To tCurrentFiltersList.Count - 1
                            If tCurrentFiltersList(i).IsChecked Then
                                indexOfFirstCheckedRule = i
                                Exit For
                            End If
                        Next

                        If tCurrentFiltersList.IndexOf(cfi) = indexOfFirstCheckedRule Then
                            If isCurrentRulePassed Then retVal = True
                        Else
                            Select Case filterOp
                                Case FilterCombinations.fcAnd
                                    If Not isCurrentRulePassed Then retVal = False
                                Case FilterCombinations.fcOr
                                    If isCurrentRulePassed Then retVal = True
                            End Select
                        End If
                        filterOp = cfi.FilterCombination
                    End If
                Next

                If tNoneRuleChecked Then retVal = True
            End If

            Return retVal
        End Function
        'A1365 ==

#Region "Measurement method function"
        Public Property MeasureMode() As ECMeasureMode

        Private mMeasureType As ECMeasureType

        Private mRegularUtilityCurveID As Integer
        Private mAdvancedUtilityCurveID As Integer 'C0026
        Private mRatingScaleID As Integer
        Private mStepFunctionID As Integer 'C0001

        Public Property RegularUtilityCurveID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0075
            Get
                Return mRegularUtilityCurveID
            End Get
            Set(ByVal value As Integer)
                If mRegularUtilityCurveID <> value Then
                    Judgments = Nothing
                    Judgments = New clsNonPairwiseJudgments(Me)
                    mRegularUtilityCurveID = value
                End If
            End Set
        End Property

        Public Property AdvancedUtilityCurveID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0026 'C0075
            Get
                Return mAdvancedUtilityCurveID
            End Get
            Set(ByVal value As Integer)
                If mAdvancedUtilityCurveID <> value Then
                    Judgments = Nothing
                    Judgments = New clsNonPairwiseJudgments(Me)
                    mAdvancedUtilityCurveID = value
                End If
            End Set
        End Property

        Public Property RatingScaleID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0075
            Get
                Return mRatingScaleID
            End Get
            Set(ByVal value As Integer)
                'C0016===
                If mRatingScaleID <> value Then
                    Judgments = Nothing
                    If MeasureType = ECMeasureType.mtPWOutcomes Then
                        Judgments = New clsPairwiseJudgments(Me)
                    Else
                        Judgments = New clsNonPairwiseJudgments(Me)
                    End If
                    mRatingScaleID = value
                End If
                'C0016==
            End Set
        End Property

        Public Property StepFunctionID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0001 'C0075
            Get
                Return mStepFunctionID
            End Get
            Set(ByVal value As Integer)
                'C0016===
                If mStepFunctionID <> value Then
                    Judgments = Nothing
                    Judgments = New clsNonPairwiseJudgments(Me)
                    mStepFunctionID = value
                End If
                'C0016==
            End Set
        End Property

        Public ReadOnly Property MeasurementScaleID() As Integer
            Get
                Select Case MeasureType
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes 'A0683
                        Return RatingScaleID
                    Case ECMeasureType.mtRegularUtilityCurve
                        Return RegularUtilityCurveID
                    Case ECMeasureType.mtStep 'C0026
                        Return StepFunctionID
                    Case ECMeasureType.mtAdvancedUtilityCurve 'C0026
                        Return AdvancedUtilityCurveID
                    Case Else
                        Return -1
                End Select
            End Get
        End Property

        Public ReadOnly Property MeasurementScale() As clsMeasurementScale
            Get
                If IsAlternative And RatingScaleID = -1 Then
                    Select Case MeasureType
                        Case ECMeasureType.mtRatings
                            Dim RS As clsRatingScale = Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByName("WIDE LIKELIHOOD RATING SCALE")
                            If RS IsNot Nothing Then
                                RatingScaleID = RS.ID
                                Return RS
                            End If
                            RS = Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByName(DEFAULT_RATING_SCALE_NAME_FOR_LIKELIHOOD)
                            If RS IsNot Nothing Then
                                RatingScaleID = RS.ID
                                Return RS
                            End If
                            RS = Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByName("Default Likelihood Rating Scale")
                            If RS IsNot Nothing Then
                                RatingScaleID = RS.ID
                                Return RS
                            End If
                        Case ECMeasureType.mtPWOutcomes
                            Dim RS As clsRatingScale = Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByName(DEFAULT_OUTCOMES_SCALE_NAME)
                            If RS IsNot Nothing Then
                                RatingScaleID = RS.ID
                                Return RS
                            End If
                            RS = Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByName(DEFAULT_PWOP_SCALE_NAME)
                            If RS IsNot Nothing Then
                                RatingScaleID = RS.ID
                                Return RS
                            End If
                    End Select

                End If

                Select Case MeasureType
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes 'A0683
                        Return Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByID(RatingScaleID)
                    Case ECMeasureType.mtRegularUtilityCurve
                        Return Hierarchy.ProjectManager.MeasureScales.GetRegularUtilityCurveByID(RegularUtilityCurveID)
                    Case ECMeasureType.mtStep
                        Return Hierarchy.ProjectManager.MeasureScales.GetStepFunctionByID(StepFunctionID)
                        'Case ECMeasureType.mtAdvancedUtilityCurve 'C0026
                        '    Return Hierarchy.ProjectManager.MeasureScales.GetAdvancedUtilityCurveByID(AdvancedUtilityCurveID)
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property

        Private mFeedbackMeasureType As ECMeasureType

        Private mFeedbackRegularUtilityCurveID As Integer
        Private mFeedbackAdvancedUtilityCurveID As Integer 'C0026
        Private mFeedbackRatingScaleID As Integer
        Private mFeedbackStepFunctionID As Integer 'C0001

        Public Property FeedbackRegularUtilityCurveID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0075
            Get
                Return mFeedbackRegularUtilityCurveID
            End Get
            Set(ByVal value As Integer)
                If mFeedbackRegularUtilityCurveID <> value Then
                    FeedbackJudgments = Nothing
                    FeedbackJudgments = New clsNonPairwiseJudgments(Me)
                    FeedbackRegularUtilityCurveID = value
                End If
            End Set
        End Property

        Public Property FeedbackAdvancedUtilityCurveID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0026 'C0075
            Get
                Return mFeedbackAdvancedUtilityCurveID
            End Get
            Set(ByVal value As Integer)
                If mFeedbackAdvancedUtilityCurveID <> value Then
                    FeedbackJudgments = Nothing
                    FeedbackJudgments = New clsNonPairwiseJudgments(Me)
                    mFeedbackAdvancedUtilityCurveID = value
                End If
            End Set
        End Property

        Public Property FeedbackRatingScaleID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0075
            Get
                Return mFeedbackRatingScaleID
            End Get
            Set(ByVal value As Integer)
                If mFeedbackRatingScaleID <> value Then
                    FeedbackJudgments = Nothing
                    If MeasureType = ECMeasureType.mtPWOutcomes Then
                        FeedbackJudgments = New clsPairwiseJudgments(Me)
                    Else
                        FeedbackJudgments = New clsNonPairwiseJudgments(Me)
                    End If
                    mFeedbackRatingScaleID = value
                End If
            End Set
        End Property

        Public Property FeedbackStepFunctionID(Optional ByVal ErasePreviousJudgmentsOnChange As Boolean = True) As Integer 'C0001 'C0075
            Get
                Return mFeedbackStepFunctionID
            End Get
            Set(ByVal value As Integer)
                If mFeedbackStepFunctionID <> value Then
                    FeedbackJudgments = Nothing
                    FeedbackJudgments = New clsNonPairwiseJudgments(Me)
                    mFeedbackStepFunctionID = value
                End If
            End Set
        End Property

        Public ReadOnly Property FeedbackMeasurementScaleID() As Integer
            Get
                Select Case MeasureType
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes 'A0683
                        Return FeedbackRatingScaleID
                    Case ECMeasureType.mtRegularUtilityCurve
                        Return FeedbackRegularUtilityCurveID
                    Case ECMeasureType.mtStep 'C0026
                        Return FeedbackStepFunctionID
                    Case ECMeasureType.mtAdvancedUtilityCurve 'C0026
                        Return FeedbackAdvancedUtilityCurveID
                    Case Else
                        Return -1
                End Select
            End Get
        End Property

        Public ReadOnly Property FeedbackMeasurementScale() As clsMeasurementScale
            Get
                Select Case FeedbackMeasureType
                    Case ECMeasureType.mtRatings, ECMeasureType.mtPWOutcomes 'A0683
                        Return Hierarchy.ProjectManager.MeasureScales.GetRatingScaleByID(FeedbackRatingScaleID)
                    Case ECMeasureType.mtRegularUtilityCurve
                        Return Hierarchy.ProjectManager.MeasureScales.GetRegularUtilityCurveByID(FeedbackRegularUtilityCurveID)
                    Case ECMeasureType.mtStep
                        Return Hierarchy.ProjectManager.MeasureScales.GetStepFunctionByID(FeedbackStepFunctionID)
                        'Case ECMeasureType.mtAdvancedUtilityCurve 'C0026
                        '    Return Hierarchy.ProjectManager.MeasureScales.GetAdvancedUtilityCurveByID(FeedbackAdvancedUtilityCurveID)
                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property
#End Region

#Region "Calculations"
        Public Property WRTRelativeAPriority() As Single
        Public Property WRTRelativeBPriority() As Single
        Public Property GlobalCalculated As Boolean = False
        Public Property GlobalCalculatedSA As Boolean = False
        Public Property WRTGlobalPriorityFeedback As Single
        Public Property WRTGlobalPriority() As Single
        Public Property DollarValue As Double
        Public Property UnnormalizedPriorityWithoutControls As Single
        Public Property UnnormalizedPriority As Single
        Public Property SALocalPriority() As Single
        Public Property SAGlobalPriority() As Single
        Public Property SimulatedPriority As Double
        Public Property SimulatedAltLikelihood As Double
        Public Property SimulatedAltImpact As Double
        Public Property RiskValue As Double = 0
        Public Property NumFired As Integer = 0
        Public Property SimulatedConsequences As New Dictionary(Of Integer, Double)
        Public Property SimulatedVulnerabilities As New Dictionary(Of Integer, Double)

        Public Property UnnormalizedPriorityBeforeBayes As Double
        Public Property UnnormalizedPriorityWithoutControlsBeforeBayes As Double

        Public ReadOnly Property LocalPriority(ByVal UserID As Integer, Optional UseReductions As Boolean = False) As Single
            Get
                If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then
                    ' rolled back per Ernest's request
                    Return 1
                    'If ParentNode IsNot Nothing Then
                    '    Dim count As Integer = 0
                    '    For Each child As clsNode In ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
                    '        If child.RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then count += 1
                    '    Next
                    '    Return 1 / count
                    'Else
                    '    Return 1
                    'End If
                End If

                Dim res As Single = LocalPriority(Hierarchy.ProjectManager.CalculationsManager.GetCalculationTargetByUserID(UserID))

                If UseReductions Then ApplyControlsReductions(res) 'A0828
                Return res
            End Get
        End Property

        Private Sub ApplyControlsReductions(ByRef Res As Double) 'A0828
            For Each control As clsControl In Hierarchy.ProjectManager.Controls.EnabledControls 'A1383 + A1392
                For Each assignment As clsControlAssignment In control.Assignments
                    If assignment.IsActive Then
                        If assignment.EventID.Equals(Guid.Empty) Then
                            If assignment.ObjectiveID.Equals(NodeGuidID) Then
                                Res *= 1 + If(Hierarchy.ProjectManager.CalculationsManager.IsOpportunity(Me), assignment.Value, -assignment.Value)
                            End If
                        End If
                    End If
                Next
            Next
        End Sub

        Public ReadOnly Property LocalPriority(ByVal CalculationTarget As clsCalculationTarget, Optional WRTParentNode As clsNode = Nothing) As Single 'C0159
            Get
                If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then
                    ' rolled back per Ernest's request
                    Return 1
                    'If ParentNode IsNot Nothing Then
                    '    Dim count As Integer = 0
                    '    For Each child As clsNode In ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
                    '        If child.RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then count += 1
                    '    Next
                    '    Return 1 / count
                    'Else
                    '    Return 1
                    'End If
                End If

                If WRTParentNode Is Nothing Then
                    If mParentNode Is Nothing Then
                        Return 1
                    Else
                        Return mParentNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(NodeID)
                    End If
                Else
                    Return WRTParentNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(NodeID)
                End If
            End Get
        End Property

        Public ReadOnly Property LocalPriorityNormalized(ByVal CalculationTarget As clsCalculationTarget, Optional WRTParentNode As clsNode = Nothing) As Single 'C0159
            Get
                If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then
                    ' rolled back per Ernest's request
                    Return 1
                    'If ParentNode IsNot Nothing Then
                    '    Dim count As Integer = 0
                    '    For Each child As clsNode In ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
                    '        If child.RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then count += 1
                    '    Next
                    '    Return 1 / count
                    'Else
                    '    Return 1
                    'End If
                End If

                If WRTParentNode Is Nothing Then
                    If mParentNode Is Nothing Then
                        Return 1
                    Else
                        Return mParentNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(NodeID)
                    End If
                Else
                    Return WRTParentNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetNormalizedWeightValueByNodeID(NodeID)
                End If
            End Get
        End Property

        Public ReadOnly Property LocalPriorityUnnormalized(ByVal UserID As Integer, Optional UseReductions As Boolean = False, Optional WRTParentNode As clsNode = Nothing) As Single
            Get
                If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then
                    ' rolled back per Ernest's request
                    Return 1
                    'If ParentNode IsNot Nothing Then
                    '    Dim count As Integer = 0
                    '    For Each child As clsNode In ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
                    '        If child.RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then count += 1
                    '    Next
                    '    Return 1 / count
                    'Else
                    '    Return 1
                    'End If
                End If

                Dim res As Single = LocalPriorityUnnormalized(Hierarchy.ProjectManager.CalculationsManager.GetCalculationTargetByUserID(UserID), WRTParentNode)
                If UseReductions Then ApplyControlsReductions(res) 'A0828
                Return res
            End Get
        End Property


        Public ReadOnly Property LocalPriorityUnnormalized(ByVal CalculationTarget As clsCalculationTarget, Optional WRTParentNode As clsNode = Nothing) As Single 'C0159
            Get
                If Hierarchy.ProjectManager.IsRiskProject AndAlso Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood AndAlso RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then
                    ' rolled back per Ernest's request
                    Return 1
                    'If ParentNode IsNot Nothing Then
                    '    Dim count As Integer = 0
                    '    For Each child As clsNode In ParentNode.GetNodesBelow(UNDEFINED_USER_ID)
                    '        If child.RiskNodeType() = ECTypes.RiskNodeType.ntCategory Then count += 1
                    '    Next
                    '    Return 1 / count
                    'Else
                    '    Return 1
                    'End If
                End If

                If WRTParentNode Is Nothing Then
                    If mParentNode Is Nothing Then
                        Return 1
                    Else
                        Return mParentNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(NodeID) 'C0338
                    End If
                Else
                    Return WRTParentNode.Judgments.Weights.GetUserWeights(CalculationTarget.GetUserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetUnnormalizedWeightValueByNodeID(NodeID) 'C0338
                End If
            End Get
        End Property

        Public Function GetIdealLocalPriority(ByVal UserID As Integer) As Single 'C0159
            Return Judgments.Weights.GetUserWeights(UserID, Hierarchy.ProjectManager.CalculationsManager.SynthesisMode, Hierarchy.ProjectManager.CalculationsManager.IncludeIdealAlternative).GetWeightValueByNodeID(IDEAL_ALTERNATIVE_ID) 'C0338
        End Function

        Public Function IsAvailableForCalculations(ByVal CalculationTarget As clsCalculationTarget) As Boolean
            Return Enabled AndAlso Not DisabledForUser(CalculationTarget.GetUserID)
        End Function

        Public Overloads Sub CalculateLocal(ByVal UserID As Integer) 'C0551
            Dim calcTarget As clsCalculationTarget = Hierarchy.ProjectManager.CalculationsManager.GetCalculationTargetByUserID(UserID)
            CalculateLocal(calcTarget)
        End Sub

        Public Overloads Sub CalculateLocal(ByVal CalculationTarget As clsCalculationTarget)
            If Not Hierarchy.ProjectManager.CalculationsManager.CalcForSA Then
                If IsAvailableForCalculations(CalculationTarget) Then
                    Judgments.CalculateWeights(CalculationTarget)
                End If
            End If
        End Sub

        Public Sub CalculateWRTGlobalSA(ByVal CalculationTarget As clsCalculationTarget)
            For Each node As clsNode In Children
                If Not node.GlobalCalculatedSA Then
                    If node.IsAvailableForCalculations(CalculationTarget) Then
                        Dim parents As New List(Of clsNode)
                        If ParentNodesGuids.Count = 0 Then
                            parents.Add(Me)
                        Else
                            For Each parentGuid As Guid In node.ParentNodesGuids
                                Dim parent As clsNode = Hierarchy.GetNodeByID(parentGuid)
                                If parent IsNot Nothing Then parents.Add(parent)
                            Next
                        End If

                        For Each parent As clsNode In parents
                            'node.WRTGlobalPriority += parent.WRTGlobalPriority * node.LocalPriority(CalculationTarget, parent)

                            If parent Is Hierarchy.ProjectManager.CalculationsManager.SANode Then
                                node.SAGlobalPriority += parent.SAGlobalPriority * node.SALocalPriority
                            Else
                                'node.SAGlobalPriority += parent.SAGlobalPriority * node.LocalPriority(CalculationTarget, parent)
                                node.SAGlobalPriority += parent.SAGlobalPriority * node.LocalPriorityUnnormalized(CalculationTarget, parent)
                            End If
                        Next
                        node.GlobalCalculatedSA = True
                    End If
                End If
            Next

            For Each node As clsNode In Children
                If node.IsAvailableForCalculations(CalculationTarget) Then
                    node.CalculateWRTGlobalSA(CalculationTarget)
                End If
            Next
        End Sub

        Public Sub CalculateWRTGlobal(ByVal CalculationTarget As clsCalculationTarget)
            Dim k As Single = GetMultiplierForCategories()

            For Each node As clsNode In Children
                If Not node.GlobalCalculated Then
                    If node.IsAvailableForCalculations(CalculationTarget) Then
                        Dim parents As New List(Of clsNode)
                        If ParentNodesGuids.Count = 0 Then
                            parents.Add(Me)
                        Else
                            For Each parentGuid As Guid In node.ParentNodesGuids
                                Dim parent As clsNode = Hierarchy.GetNodeByID(parentGuid)
                                If parent IsNot Nothing Then parents.Add(parent)
                            Next
                        End If

                        For Each parent As clsNode In parents
                            node.WRTGlobalPriority += parent.WRTGlobalPriority * node.LocalPriority(CalculationTarget, parent)

                            node.UnnormalizedPriority += parent.UnnormalizedPriority * node.LocalPriorityUnnormalized(CalculationTarget, parent)

                            'node.SAGlobalPriority += parent.SAGlobalPriority * node.SALocalPriority
                            If parent Is Hierarchy.ProjectManager.CalculationsManager.SANode Then
                                node.SAGlobalPriority += parent.SAGlobalPriority * node.SALocalPriority
                            Else
                                node.SAGlobalPriority += parent.SAGlobalPriority * node.LocalPriority(CalculationTarget, parent)
                            End If
                        Next

                        If Not IsTerminalNode Then
                            With Hierarchy.ProjectManager.CalculationsManager
                                If .ControlsUsageMode = ControlsUsageMode.UseAll Or .ControlsUsageMode = ControlsUsageMode.UseOnlyActive Then
                                    Dim CovObjReduction As Single = 1
                                    Dim tEnabledControls As List(Of clsControl) = Hierarchy.ProjectManager.Controls.EnabledControls
                                    For Each control As clsControl In tEnabledControls
                                        If (.ControlsUsageMode = ControlsUsageMode.UseAll) OrElse (.ControlsUsageMode = ControlsUsageMode.UseOnlyActive AndAlso control.Active) Then
                                            For Each assignment As clsControlAssignment In control.Assignments
                                                If (.ControlsUsageMode = ControlsUsageMode.UseAll) OrElse assignment.IsActive Then
                                                    If assignment.EventID.Equals(Guid.Empty) Then
                                                        If assignment.ObjectiveID.Equals(NodeGuidID) Then
                                                            CovObjReduction *= 1 + If(.IsOpportunity(Me), assignment.Value, -assignment.Value)
                                                        End If
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Next
                                    If Not AppliedControl Then
                                        UnnormalizedPriority *= CovObjReduction
                                        WRTRelativeBPriority *= CovObjReduction
                                        SimulatedPriority *= CovObjReduction
                                        AppliedControl = True
                                    End If
                                End If
                            End With
                        End If

                        node.GlobalCalculated = True

                        'node.CalculateWRTGlobal(CalculationTarget) 'C0159
                    End If
                End If
            Next

            For Each node As clsNode In Children
                If node.IsAvailableForCalculations(CalculationTarget) Then
                    node.CalculateWRTGlobal(CalculationTarget)
                End If
            Next
        End Sub

        Public Function GetKnownLikelihoods() As List(Of KnownLikelihoodDataContract) 'A0847
            Dim res As List(Of KnownLikelihoodDataContract) = Nothing
            Dim attr As ECCore.clsAttribute = Hierarchy.ProjectManager.Attributes.GetAttributeByID(ATTRIBUTE_KNOWN_VALUE_ID)
            If attr IsNot Nothing Then
                Dim nodes As List(Of ECCore.clsNode) = Me.GetNodesBelow(UNDEFINED_USER_ID)

                res = New List(Of KnownLikelihoodDataContract)
                For Each nd As ECCore.clsNode In nodes
                    If nd.RiskNodeType <> ECTypes.RiskNodeType.ntCategory Then
                        Dim item As New KnownLikelihoodDataContract
                        item.ID = nd.NodeID
                        item.GuidID = nd.NodeGuidID
                        item.NodeName = nd.NodeName
                        Dim Value As Double = CType(Hierarchy.ProjectManager.Attributes.GetAttributeValue(attr.ID, nd.NodeGuidID, Me.NodeGuidID), Double)
                        item.Value = CDbl(If(Value = UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, -1, Value))
                        item.NewValue = item.Value
                        res.Add(item)
                    End If
                Next
            End If
            Return res
        End Function

        Public Sub NormalizeGlobalPriorityWRT(ByVal NormalizeFactor As Single)
            If (Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
                WRTGlobalPriority *= NormalizeFactor
            End If
        End Sub

        Public Sub NormalizeGlobalPrioritySA(ByVal NormalizeFactor As Single)
            If (Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
                SAGlobalPriority *= NormalizeFactor
            End If
        End Sub

        Public Sub NormalizeUnnormalizedPriority(ByVal NormalizeFactor As Single)
            If (Hierarchy.HierarchyType = ECHierarchyType.htAlternative) Then
                UnnormalizedPriority *= NormalizeFactor
            End If
        End Sub
        Public Function GetMultiplierForCategories() As Single
            ' rolled back per Ernest's request
            Return 1
            'Dim count As Integer = GetCategoriesCountInCluster()
            'Dim k As Single = 1
            'If count = Children.Count AndAlso Children.Count <> 0 Then
            '    k = 1 / count
            'Else
            '    k = 1 / (count + 1)
            'End If
            'Return k
        End Function

#End Region

#Region "AHP properties"
        Public Property AHPNodeData() As New clsAHPNodeData
        Public Property AHPAltData() As New clsAHPAltData
        Public Property AHPTag() As Object
#End Region

        Public Function Clone() As clsNode 'A0107
            Return CType(Me.MemberwiseClone(), clsNode)
        End Function
    End Class

    <Serializable> Public Class AlternativeProAndCon 'A2103
        Public Property text() As String
        Public Property author() As String
        Public Property created() As Date
    End Class

End Namespace