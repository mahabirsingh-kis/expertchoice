Imports System.Collections
Imports ECCore
Imports System.Linq

Namespace ECCore

    <Serializable()> Public Class clsHierarchy
        Private Const UseFlatAlternativesList As Boolean = True
        Public Property ProjectManager() As clsProjectManager
        Public Property HierarchyID() As Integer
        Public Property HierarchyGuidID() As Guid

        Private mHierarchyName As String = ""
        Public Property HierarchyName() As String
            Get
                Return mHierarchyName
            End Get
            Set(ByVal value As String)
                If value.Length > HIERARCHY_NAME_MAX_LENGTH Then
                    mHierarchyName = value.Remove(HIERARCHY_NAME_MAX_LENGTH)
                Else
                    mHierarchyName = value
                End If
            End Set
        End Property
        Public Property Comment() As String = ""
        Public Property Author() As String = ""
        Public Property HierarchyType() As ECHierarchyType

        Friend mNodesIntDict As New Dictionary(Of Integer, clsNode)
        Friend mNodesGuidDict As New Dictionary(Of Guid, clsNode)

        Public Property Nodes() As New List(Of clsNode)
        Public Property AltsDefaultContribution() As ECAltsDefaultContribution = ECAltsDefaultContribution.adcFull
        Public Property DefaultMeasurementTypeForNonCoveringObjectives() As ECMeasureType = ECMeasureType.mtPairwise

        Private mDefaultMeasurementType As ECMeasureType = ECMeasureType.mtRatings
        Public Property DefaultMeasurementTypeForCoveringObjectives() As ECMeasureType
            Get
                Return If(ProjectManager.IsRiskProject, ECMeasureType.mtRatings, mDefaultMeasurementType)
            End Get
            Set(ByVal value As ECMeasureType)
                mDefaultMeasurementType = value
            End Set
        End Property

        ''' <summary>
        ''' List of all nodes and create duplicate nodes if there are more than 1 parent (ParentNodesGuids.Count>1)
        ''' </summary>
        ''' <returns></returns>
        Public Function GetOrderedExpandedHierarchyFromCompleteHierarchy() As List(Of clsNode)
            Dim retVal As New List(Of clsNode)
            If Nodes.Count > 0 Then
                Dim H As New clsHierarchy(New clsProjectManager)
                GetExpandedOrderedChildren(H, Nodes(0), Nothing, retVal)
            End If
            Return retVal
        End Function

        ''' <summary>
        ''' Get the list of nodes with only single Parent node and no other properties
        ''' </summary>
        ''' <param name="node"></param>
        ''' <param name="parentNode"></param>
        ''' <param name="retVal"></param>
        Private Sub GetExpandedOrderedChildren(H As clsHierarchy, ByVal node As clsNode, ByRef parentNode As clsNode, ByRef retVal As List(Of clsNode))
            Dim resNode As clsNode = New clsNode(H) With {.NodeID = node.NodeID, .NodeGuidID = node.NodeGuidID, .NodeName = node.NodeName}
            resNode.ParentNode = parentNode
            'If parentNode IsNot Nothing Then resNode.ParentNode.Children.Add(resNode)

            retVal.Add(resNode)

            For Each child As clsNode In node.Children
                GetExpandedOrderedChildren(H, child, resNode, retVal)
            Next
        End Sub

        Public Sub New(ByVal ProjectManager As clsProjectManager, Optional ByVal HierarchyType As ECHierarchyType = ECHierarchyType.htModel)
            Me.ProjectManager = ProjectManager
            Me.HierarchyType = HierarchyType
            Me.HierarchyGuidID = Guid.NewGuid
        End Sub

        Protected Overridable Sub CalculateLocalRecursive(ByVal CalculationTarget As clsCalculationTarget, ByVal node As clsNode) 'C0159
            If Not node.IsAlternative Then
                node.CalculateLocal(CalculationTarget)

                Dim nd As clsNode
                For Each nd In node.Children
                    CalculateLocalRecursive(CalculationTarget, nd) 'C0159
                Next
            End If
        End Sub

        Public Overridable Sub CalculateLocal(ByVal CalculationTarget As clsCalculationTarget)
            If Nodes Is Nothing OrElse Nodes.Count = 0 Then Exit Sub
            If Nodes.Item(0) IsNot Nothing Then CalculateLocalRecursive(CalculationTarget, Nodes.Item(0))
        End Sub

        Public Overridable Sub CalculateLocal(ByVal CalculationTarget As clsCalculationTarget, ByVal Node As clsNode)
            If Node Is Nothing Then Exit Sub
            CalculateLocalRecursive(CalculationTarget, Node)
        End Sub

        Public Sub CalculateWRTGlobal(ByVal CalculationTarget As clsCalculationTarget, ByVal node As clsNode, Optional StartingPriority As Single = 1) 'C0159
            If node Is Nothing Then Exit Sub

            For Each nd As clsNode In Nodes
                nd.GlobalCalculated = False
            Next

            node.WRTGlobalPriority = StartingPriority
            node.UnnormalizedPriority = StartingPriority
            node.SAGlobalPriority = StartingPriority
            node.CalculateWRTGlobal(CalculationTarget)
        End Sub

        Friend ReadOnly Property TerminalNodesGuidHashset As HashSet(Of Guid)
            Get
                Dim nodes As List(Of clsNode) = TerminalNodes
                Dim res As New HashSet(Of Guid)
                For Each node As clsNode In nodes
                    res.Add(node.NodeGuidID)
                Next
                Return res
            End Get
        End Property

        Public ReadOnly Property TerminalNodes(Optional LinearOrder As Boolean = False) As List(Of clsNode)
            Get
                If HierarchyType = ECHierarchyType.htAlternative AndAlso UseFlatAlternativesList Then
                    Return Nodes
                Else
                    Return Nodes.Where(Function(n) (n.IsTerminalNode)).ToList
                End If
            End Get
        End Property

        ''' <summary>
        ''' Checks if one node is a descendant of another node in the hierarchy
        ''' </summary>
        ''' <param name="ChildNode">Node</param>
        ''' <param name="ParentNode">Parent node</param>
        ''' <returns>Returns True is Node is a descendant of a ParentNode, otherwise returns False</returns>
        ''' <remarks></remarks>
        Public Function IsChildOf(ByVal ChildNode As clsNode, ByVal ParentNode As clsNode) As Boolean 'C0255
            If ChildNode.Level < ParentNode.Level Then Return False
            If ChildNode.Level = ParentNode.Level Then
                Return ChildNode Is ParentNode
            Else
                For Each parentGuid As Guid In ChildNode.ParentNodesGuids
                    Dim parent As clsNode = GetNodeByID(parentGuid)
                    If parent IsNot Nothing Then
                        If IsChildOf(parent, ParentNode) Then
                            Return True
                        End If
                    End If
                Next
                Return IsChildOf(ChildNode.ParentNode, ParentNode)
            End If
            Return False
        End Function

        ''' <summary>
        ''' Return a list of nodes at specified level
        ''' </summary>
        ''' <param name="Level">Node level</param>
        ''' <returns>Return a list of nodes at specified level</returns>
        ''' <remarks></remarks>
        Public Function GetLevelNodes(ByVal Level As Integer) As List(Of clsNode)
            Return Nodes.Where(Function(n) (n.Level = Level)).ToList
        End Function

        ''' <summary>
        ''' Returns maximum level in the hierarchy
        ''' </summary>
        ''' <returns>Returns maximum level in the hierarchy</returns>
        ''' <remarks></remarks>
        Public Function GetMaxLevel() As Integer
            Return Nodes.Select(Function(n) (n.Level)).DefaultIfEmpty(0).Max
        End Function

        ''' <summary>
        ''' Returns terminal nodes of part of the hierarchy that starts from specified node
        ''' </summary>
        ''' <param name="Node">Node that is a root for a subtree</param>
        ''' <returns>Returns terminal nodes of part of the hierarchy that starts from specified node</returns>
        ''' <remarks></remarks>
        Public Function GetRespectiveTerminalNodes(ByVal Node As clsNode) As List(Of clsNode)
            Return Nodes.Where(Function(n) (n.IsTerminalNode And IsChildOf(n, Node))).ToList
        End Function

        ''' <summary>
        ''' Returns node by ID
        ''' </summary>
        ''' <param name="ID">ID of a node</param>
        ''' <returns>Returns node by ID. If node with such ID does not exist returns Nothing.</returns>
        ''' <remarks></remarks>
        Public Overloads Function GetNodeByID(ByVal ID As Integer) As clsNode
            Return If(mNodesIntDict.ContainsKey(ID), mNodesIntDict(ID), Nothing)
            'Return Nodes.FirstOrDefault(Function(n) (n.NodeID = ID))
        End Function

        Public Overloads Function GetNodeByID(ByVal GuidID As Guid) As clsNode
            Return If(mNodesGuidDict.ContainsKey(GuidID), mNodesGuidDict(GuidID), Nothing)
            'Return Nodes.FirstOrDefault(Function(n) (n.NodeGuidID.Equals(GuidID)))
        End Function

        Public Function GetAlternativeByMapkey(ByVal sMapkey As String) As clsNode 'AS/14406

            For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).TerminalNodes
                Dim attrValue As Object = ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_MAPKEY_ID, alt.NodeGuidID)
                If attrValue IsNot Nothing Then
                    If attrValue.ToString = sMapkey Then
                        Return alt
                    End If
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns next free node ID in the hierarchy
        ''' </summary>
        ''' <returns>Returns next free node ID in the hierarchy</returns>
        ''' <remarks></remarks>
        Public Function GetNextNodeID() As Integer
            Return Nodes.Select(Function(n) (n.NodeID)).DefaultIfEmpty(-1).Max + 1
        End Function

        ' D2615 ===
        Public Function GetDefaultRatingScale() As clsRatingScale
            Dim rs As clsRatingScale = ProjectManager.MeasureScales.GetRatingScaleByName(If(ProjectManager.IsRiskProject, If(HierarchyID = ECHierarchyID.hidLikelihood, If(ProjectManager.IsRiskProject, DEFAULT_RATING_SCALE_NAME_FOR_LIKELIHOOD, DEFAULT_RATING_SCALE_NAME), DEFAULT_RATING_SCALE_NAME_FOR_IMPACT), DEFAULT_RATING_SCALE_NAME))
            If rs Is Nothing Then
                rs = ProjectManager.MeasureScales.GetRatingScaleByName("Scale for Project Risk")
            End If
            Return rs
        End Function
        ' D2615 ==

        ''' <summary>
        ''' Adds new node to the hierarchy to specific parent node
        ''' </summary>
        ''' <param name="ParentNodeID">ID of parent node. New node will be inserted as a child of this node.</param>
        ''' <returns>Newly created node</returns>
        ''' <remarks>If ParentNodeID = -1 then new node will be added as a root node</remarks>
        Public Overloads Function AddNode(ByVal ParentNodeID As Integer, Optional ByVal LeaveParentMeasurementType As Boolean = False, Optional NodeGuidID As Nullable(Of Guid) = Nothing) As clsNode
            'Public Overloads Function AddNode(ByVal ParentNodeID As Integer) As clsNode 'C0383
            Dim resNode As New clsNode(Me)
            resNode.NodeGuidID = If(NodeGuidID Is Nothing, resNode.NodeGuidID, NodeGuidID)
            resNode.NodeID = GetNextNodeID()
            resNode.ParentNodeID = ParentNodeID
            'resNode.NodeName = "New Node " + resNode.NodeID.ToString 'C0014
            'C0014===
            Select Case HierarchyType
                Case ECHierarchyType.htModel
                    resNode.NodeName = "New Objective " + resNode.NodeID.ToString
                    resNode.IsAlternative = False
                Case ECHierarchyType.htAlternative
                    resNode.NodeName = "New Alternative " + resNode.NodeID.ToString
                    resNode.IsAlternative = True
                Case ECHierarchyType.htMeasure
                    resNode.NodeName = "New Node " + resNode.NodeID.ToString
                    resNode.IsAlternative = False
            End Select
            'C0014==

            If ParentNodeID = -1 Then
                Nodes.Add(resNode)

                mNodesIntDict.Add(resNode.NodeID, resNode)
                mNodesGuidDict.Add(resNode.NodeGuidID, resNode)
            Else
                Dim pNode As clsNode = GetNodeByID(ParentNodeID)
                If pNode Is Nothing Then
                    Return Nothing
                Else
                    Nodes.Add(resNode)

                    mNodesIntDict.Add(resNode.NodeID, resNode)
                    mNodesGuidDict.Add(resNode.NodeGuidID, resNode)

                    resNode.ParentNode(True, LeaveParentMeasurementType) = pNode
                End If
            End If

            If HierarchyType = ECHierarchyType.htModel Then
                If resNode.IsTerminalNode Then
                    resNode.MeasureType(True) = DefaultMeasurementTypeForCoveringObjectives 'C0075
                    If resNode.MeasureType = ECMeasureType.mtRatings Then
                        Dim rs As clsRatingScale = Nothing
                        If ProjectManager.IsRiskProject Then
                            If HierarchyID = ECHierarchyID.hidLikelihood Then
                                rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stLikelihood)
                                If rs Is Nothing Then
                                    rs = ProjectManager.MeasureScales.GetRatingScaleByName("WIDE LIKELIHOOD RATING SCALE")
                                End If
                            Else
                                rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stImpact)
                            End If
                        Else
                            rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stShared)
                        End If

                        If rs Is Nothing Then rs = GetDefaultRatingScale()

                        If rs IsNot Nothing Then
                            resNode.RatingScaleID(False) = rs.ID 'C0075
                        Else
                            resNode.MeasureType(True) = ECMeasureType.mtPairwise 'C0075
                        End If
                    End If
                End If

                If resNode.ParentNode IsNot Nothing Then
                    If (resNode.ParentNode.MeasureType <> ECMeasureType.mtPairwise) And Not LeaveParentMeasurementType Then 'C0383
                        If resNode.ParentNode.MeasureType <> ECMeasureType.mtDirect Then 'C0667
                            If ProjectManager.IsRiskProject And HierarchyID = ECHierarchyID.hidLikelihood Then
                                resNode.ParentNode.MeasureType(True) = ECMeasureType.mtRatings
                            Else
                                resNode.ParentNode.MeasureType(True) = ECMeasureType.mtPairwise 'C0075
                            End If
                        End If
                    End If
                    resNode.ParentNode.Judgments.Weights.ClearUserWeights() 'C0450
                End If
            End If

            Dim nodesBelow As List(Of clsNode)

            If HierarchyType = ECHierarchyType.htAlternative Then 'C0450
                For Each node As clsNode In ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).TerminalNodes 'C0385
                    nodesBelow = node.GetNodesBelow(UNDEFINED_USER_ID) 'C0450
                    If nodesBelow.Contains(resNode) Then
                        node.Judgments.Weights.ClearUserWeights() 'C0181
                    End If
                Next
                resNode.MeasureType = ECMeasureType.mtRatings
                Dim rs As clsRatingScale = Nothing
                If ProjectManager.IsRiskProject Then
                    Select Case HierarchyID
                        Case ECHierarchyID.hidLikelihood, 1
                            rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stLikelihood)
                            If rs Is Nothing Then
                                rs = ProjectManager.MeasureScales.GetRatingScaleByName("WIDE LIKELIHOOD RATING SCALE")
                            End If
                        Case ECHierarchyID.hidImpact
                            rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stImpact)
                    End Select
                Else
                    rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stShared)
                End If
                If rs Is Nothing Then rs = GetDefaultRatingScale()

                If rs IsNot Nothing Then
                    resNode.RatingScaleID = rs.ID
                End If
            End If

            Return resNode
        End Function

        ''' <summary>
        ''' Add existing node to specific parent node
        ''' </summary>
        ''' <param name="node">Existing node to add to the hierarchy</param>
        ''' <param name="ParentNodeID">ID of parent node. New node will be inserted as a child of this node.</param>
        ''' <returns>Returns index of added node in Nodes collection</returns>
        ''' <remarks>If ParentNodeID = -1 then new node will be added as a root node</remarks>
        Public Overloads Function AddNode(ByVal node As clsNode, ByVal ParentNodeID As Integer, Optional ByVal LeaveParentMeasurementType As Boolean = False) As Integer 'C0383
            If node Is Nothing Then Return -1
            node.Hierarchy = Me

            Dim res As Integer

            If ParentNodeID = -1 Then
                Nodes.Add(node)
                mNodesIntDict.Add(node.NodeID, node)
                mNodesGuidDict.Add(node.NodeGuidID, node)
                res = Nodes.IndexOf(node)
            Else
                Dim pNode As clsNode = GetNodeByID(ParentNodeID)
                If pNode Is Nothing Then
                    Return -1
                Else
                    Nodes.Add(node)
                    mNodesIntDict.Add(node.NodeID, node)
                    mNodesGuidDict.Add(node.NodeGuidID, node)
                    res = Nodes.IndexOf(node)
                    node.ParentNode(False, LeaveParentMeasurementType) = pNode 'C0383
                End If
            End If

            Select Case HierarchyType
                Case ECHierarchyType.htModel, ECHierarchyType.htMeasure
                    node.IsAlternative = False
                Case ECHierarchyType.htAlternative
                    node.IsAlternative = True
            End Select

            If HierarchyType = ECHierarchyType.htModel Then
                If node.IsTerminalNode Then
                    node.MeasureType(False) = DefaultMeasurementTypeForCoveringObjectives 'C0075
                    If node.MeasureType = ECMeasureType.mtRatings Then
                        Dim rs As clsRatingScale
                        If ProjectManager.IsRiskProject Then
                            If HierarchyID = ECHierarchyID.hidLikelihood Then
                                rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stLikelihood)
                                If rs Is Nothing Then
                                    rs = ProjectManager.MeasureScales.GetRatingScaleByName("WIDE LIKELIHOOD RATING SCALE")
                                End If
                            Else
                                rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stImpact)
                            End If
                        Else
                            rs = ProjectManager.MeasureScales.GetDefaultRatingScale(ScaleType.stShared)
                        End If
                        If rs Is Nothing Then rs = GetDefaultRatingScale()
                        If rs IsNot Nothing Then
                            node.RatingScaleID(False) = rs.ID 'C0075
                        Else
                            node.MeasureType(False) = ECMeasureType.mtPairwise 'C0077
                        End If
                    End If
                End If

                If node.ParentNode IsNot Nothing Then
                    If (node.ParentNode.MeasureType <> ECMeasureType.mtPairwise) And Not LeaveParentMeasurementType Then 'C0383
                        If node.ParentNode.MeasureType <> ECMeasureType.mtDirect Then 'C0667
                            If ProjectManager.IsRiskProject And HierarchyID = ECHierarchyID.hidLikelihood Then
                                node.ParentNode.MeasureType(True) = ECMeasureType.mtRatings
                            Else
                                node.ParentNode.MeasureType(True) = ECMeasureType.mtPairwise 'C0075
                            End If
                        End If
                    End If
                End If
            End If

            Return res
        End Function

        Private Sub DeleteTerminalAltNode(ByVal alt As clsNode)
            If alt Is Nothing OrElse Not alt.IsAlternative Then Exit Sub

            For Each H As clsHierarchy In ProjectManager.Hierarchies
                Dim nodesBelow As List(Of clsNode)
                For Each node As clsNode In H.TerminalNodes
                    nodesBelow = node.GetNodesBelow(UNDEFINED_USER_ID)
                    If nodesBelow.Contains(alt) Then
                        node.DeleteJudgmentsWithChild(alt.NodeID)
                        node.ChildrenAlts.Remove(alt.NodeID)
                        node.Judgments.Weights.ClearUserWeights()
                        node.Judgments.ClearCombinedJudgments()
                    End If
                Next
            Next

            ProjectManager.Attributes.DeleteMapkeyForAlternative(alt.NodeGuidID) 'AS/14611
        End Sub

        Public Sub DeleteNode(ByVal node As clsNode, Optional ByVal UpdateStorage As Boolean = True) 'C0723
            If node.Children.Count <> 0 Then
                Dim child As clsNode
                If Not HasCompleteHierarchy() Then 'AS/9335 enclosed and added Else part
                    For i As Integer = node.Children.Count - 1 To 0 Step -1
                        child = node.Children(i)
                        DeleteNode(child)
                    Next
                Else 'AS/9335===
                    For Each child In node.Children
                        child.ParentNodesGuids.Remove(node.NodeGuidID)
                    Next
                End If 'AS/9335==
            End If

            If (HierarchyType = ECHierarchyType.htAlternative) And node.IsTerminalNode Then
                'DeleteTerminalAltNode(node) 'C0723
                DeleteTerminalAltNode(node)
            End If

            If node.ParentNode IsNot Nothing Then
                node.ParentNode.Children.Remove(node)
                node.ParentNode.DeleteJudgmentsWithChild(node.NodeID)

                If HierarchyType = ECHierarchyType.htModel Then
                    If node.ParentNode.IsTerminalNode Then
                        node.ParentNode.MeasureType = DefaultMeasurementTypeForCoveringObjectives
                        If node.ParentNode.MeasureType = ECMeasureType.mtRatings Then
                            Dim rs As clsRatingScale = GetDefaultRatingScale()    ' D2615
                            If rs IsNot Nothing Then
                                node.ParentNode.RatingScaleID = rs.ID
                            Else
                                node.ParentNode.MeasureType = ECMeasureType.mtPairwise
                            End If
                        End If
                    End If
                End If
            End If

            node.Judgments.ClearCombinedJudgments() 'C0184
            node.Judgments.Weights.ClearUserWeights()
            If node.ParentNode IsNot Nothing Then
                node.ParentNode.Judgments.ClearCombinedJudgments() 'C0184
                node.ParentNode.Judgments.Weights.ClearUserWeights() 'C0181
            End If
            'C0176==

            Nodes.Remove(node)

            mNodesIntDict.Remove(node.NodeID)
            mNodesGuidDict.Remove(node.NodeGuidID)

            If UpdateStorage Then 'C0723
                ProjectManager.StorageManager.Writer.SaveModelStructure()
            End If
        End Sub

        Friend mIsUsingDefaultContribution As Nullable(Of Boolean) = Nothing

        Public ReadOnly Property IsUsingDefaultFullContribution() As Boolean
            Get
                If mIsUsingDefaultContribution Is Nothing Then
                    mIsUsingDefaultContribution = TerminalNodes.FirstOrDefault(Function(n) (n.ChildrenAlts.Count <> 0)) Is Nothing
                End If
                Return mIsUsingDefaultContribution
            End Get
        End Property

        Public Function MoveNode(ByVal SourceNode As clsNode, ByVal DestinationNode As clsNode, ByVal MoveAction As NodeMoveAction) As Boolean 'C0621
            If (SourceNode Is DestinationNode) Or (SourceNode Is Nothing) Or (DestinationNode Is Nothing) Then
                Return False
            End If

            Select Case MoveAction
                Case NodeMoveAction.nmaAsChildOfNode
                    'SourceNode.ParentNode = DestinationNode 'C0827
                    'C0827===
                    If SourceNode.ParentNode Is DestinationNode Then
                        DestinationNode.Children.Remove(SourceNode)
                        DestinationNode.Children.Add(SourceNode)
                    Else
                        SourceNode.ParentNode = DestinationNode
                        If Not SourceNode.ParentNodesGuids.Contains(DestinationNode.NodeGuidID) Then SourceNode.ParentNodesGuids.Add(DestinationNode.NodeGuidID) 'A1034
                    End If
                    'C0827==
                Case NodeMoveAction.nmaAfterNode, NodeMoveAction.nmaBeforeNode
                    If DestinationNode.ParentNode IsNot Nothing AndAlso SourceNode.ParentNode IsNot DestinationNode.ParentNode Then
                        SourceNode.ParentNode = DestinationNode.ParentNode
                        If Not SourceNode.ParentNodesGuids.Contains(DestinationNode.ParentNode.NodeGuidID) Then SourceNode.ParentNodesGuids.Add(DestinationNode.ParentNode.NodeGuidID) 'A1044
                    End If

                    'C0677===
                    Select Case HierarchyType
                        Case ECHierarchyType.htModel, ECHierarchyType.htMeasure
                            If DestinationNode.ParentNode Is Nothing Then
                                SourceNode.ParentNode.Children.Remove(SourceNode)
                                SourceNode.ParentNode = DestinationNode
                                DestinationNode.Children.Insert(0, SourceNode)
                            Else
                                DestinationNode.ParentNode.Children.Remove(SourceNode)
                                If MoveAction = NodeMoveAction.nmaAfterNode Then
                                    DestinationNode.ParentNode.Children.Insert(DestinationNode.ParentNode.Children.IndexOf(DestinationNode) + 1, SourceNode)
                                Else
                                    DestinationNode.ParentNode.Children.Insert(DestinationNode.ParentNode.Children.IndexOf(DestinationNode), SourceNode)
                                End If
                            End If
                        Case ECHierarchyType.htAlternative
                            If DestinationNode.ParentNode Is Nothing Then
                                Nodes.Remove(SourceNode)
                                If MoveAction = NodeMoveAction.nmaAfterNode Then
                                    Nodes.Insert(Nodes.IndexOf(DestinationNode) + 1, SourceNode)
                                Else
                                    Nodes.Insert(Nodes.IndexOf(DestinationNode), SourceNode)
                                End If
                            Else
                                DestinationNode.ParentNode.Children.Remove(SourceNode)
                                If MoveAction = NodeMoveAction.nmaAfterNode Then
                                    DestinationNode.ParentNode.Children.Insert(DestinationNode.ParentNode.Children.IndexOf(DestinationNode) + 1, SourceNode)
                                Else
                                    DestinationNode.ParentNode.Children.Insert(DestinationNode.ParentNode.Children.IndexOf(DestinationNode), SourceNode)
                                End If
                            End If
                    End Select
                    'C0677==
            End Select


            If DestinationNode.ParentNode IsNot Nothing Then
                DestinationNode.ParentNode.Judgments.ClearCombinedJudgments()
            End If

            Return True
        End Function

        Public Function MoveAlternative(ByVal SourceIndex As Integer, ByVal DestinationIndex As Integer) As Boolean
            Dim retVal As Boolean = False
            If (SourceIndex >= 0 AndAlso DestinationIndex >= 0 AndAlso SourceIndex < Nodes.Count AndAlso DestinationIndex < Nodes.Count) Then
                Dim tSourceGuid As Guid = Nodes.Item(SourceIndex).NodeGuidID
                Dim tTargetGuid As Guid = Nodes.Item(DestinationIndex).NodeGuidID

                Dim SourceNode As clsNode = Nodes.Item(SourceIndex)
                Dim DestinationNode As clsNode = Nodes.Item(DestinationIndex)

                Nodes.RemoveAt(SourceIndex)
                Nodes.Insert(DestinationIndex, SourceNode)

                retVal = True

                If DestinationNode.ParentNode IsNot Nothing Then
                    DestinationNode.ParentNode.Judgments.ClearCombinedJudgments()
                End If
            End If

            Return retVal
        End Function

        Public Function CopyNode2(ByVal SourceNode As ECCore.clsNode, ByVal DestinationNode As ECCore.clsNode, ByVal MoveAction As NodeMoveAction) As List(Of ECCore.clsNode)
            If (SourceNode Is DestinationNode) Or (SourceNode Is Nothing) Or (DestinationNode Is Nothing) Then
                Return Nothing
            End If

            Dim resList As New List(Of ECCore.clsNode)

            Dim CopiedNode As ECCore.clsNode = Nothing
            Dim ParentNode As ECCore.clsNode = Nothing

            Select Case MoveAction
                Case NodeMoveAction.nmaAsChildOfNode
                    ParentNode = DestinationNode
                Case NodeMoveAction.nmaAfterNode, NodeMoveAction.nmaBeforeNode
                    ParentNode = DestinationNode.ParentNode
            End Select

            CopiedNode = SourceNode.Hierarchy.AddNode(ParentNode.NodeID) 'C0827
            resList.Add(CopiedNode)
            CopyNode(SourceNode, CopiedNode, ParentNode, resList)

            Select Case MoveAction
                Case NodeMoveAction.nmaAsChildOfNode
                    CopiedNode.ParentNode = DestinationNode 'C0827
                Case NodeMoveAction.nmaAfterNode, NodeMoveAction.nmaBeforeNode
                    MoveNode(CopiedNode, DestinationNode, MoveAction) 'C0827
            End Select
            Return resList
        End Function

        'A0340 ===
        Public Function CopyNode(ByVal SourceNode As ECCore.clsNode, ByVal DestinationNode As ECCore.clsNode, ByVal MoveAction As NodeMoveAction, ByVal tDuplicateJudgments As Boolean) As List(Of ECCore.clsNode) 'A1316
            If (SourceNode Is DestinationNode AndAlso MoveAction = NodeMoveAction.nmaAsChildOfNode) Or (SourceNode Is Nothing) Or (DestinationNode Is Nothing) Then 'A0717
                Return Nothing
            End If

            Dim resList As New List(Of ECCore.clsNode)

            Dim CopiedNode As ECCore.clsNode = Nothing
            Dim ParentNode As ECCore.clsNode = Nothing

            Select Case MoveAction
                Case NodeMoveAction.nmaAsChildOfNode
                    ParentNode = DestinationNode
                Case NodeMoveAction.nmaAfterNode, NodeMoveAction.nmaBeforeNode
                    ParentNode = DestinationNode.ParentNode
            End Select

            'A0717 ===
            Select Case HierarchyType
                Case ECHierarchyType.htModel
                    CopiedNode = SourceNode.Hierarchy.AddNode(ParentNode.NodeID) 'C0827
                Case ECHierarchyType.htAlternative
                    CopiedNode = SourceNode.Hierarchy.AddNode(-1)
            End Select
            'A0717 ==

            resList.Add(CopiedNode)
            CopyNode(SourceNode, CopiedNode, ParentNode, resList)

            Select Case MoveAction
                Case NodeMoveAction.nmaAsChildOfNode
                    CopiedNode.ParentNode = DestinationNode 'C0827
                Case NodeMoveAction.nmaAfterNode, NodeMoveAction.nmaBeforeNode
                    MoveNode(CopiedNode, DestinationNode, MoveAction) 'C0827
            End Select

            If tDuplicateJudgments Then
                ' duplicate contributions
                If CopiedNode.IsAlternative Then
                    For Each node As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                        If node.ChildrenAlts.Contains(SourceNode.NodeID) Then
                            node.ChildrenAlts.Add(CopiedNode.NodeID)
                        End If
                    Next
                Else
                    If CopiedNode.IsTerminalNode Then
                        CopiedNode.ChildrenAlts.Clear()
                        For Each childID As Integer In SourceNode.ChildrenAlts
                            CopiedNode.ChildrenAlts.Add(childID)
                        Next
                    End If
                End If

                For Each user As clsUser In ProjectManager.UsersList
                    ProjectManager.StorageManager.Reader.LoadUserData(user)
                    Dim UR As clsUserRoles = ProjectManager.UsersRoles.GetUserRolesByID(user.UserID)

                    If CopiedNode.IsAlternative Then
                        For Each node As clsNode In ProjectManager.ActiveObjectives.TerminalNodes
                            ' duplicate permissions
                            If UR IsNot Nothing Then
                                If UR.AlternativesRoles.ContainsKey(node.NodeGuidID) Then
                                    If UR.AlternativesRoles(node.NodeGuidID).AllowedAlternativesList.Contains(SourceNode.NodeGuidID) Then
                                        UR.AlternativesRoles(node.NodeGuidID).AllowedAlternativesList.Add(CopiedNode.NodeGuidID)
                                    End If
                                    If UR.AlternativesRoles(node.NodeGuidID).RestrictedAlternativesList.Contains(SourceNode.NodeGuidID) Then
                                        UR.AlternativesRoles(node.NodeGuidID).RestrictedAlternativesList.Add(CopiedNode.NodeGuidID)
                                    End If
                                    If UR.AlternativesRoles(node.NodeGuidID).UndefinedAlternativesList.Contains(SourceNode.NodeGuidID) Then
                                        UR.AlternativesRoles(node.NodeGuidID).UndefinedAlternativesList.Add(CopiedNode.NodeGuidID)
                                    End If
                                End If
                            End If

                            For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                                Dim cgUR As clsUserRoles = ProjectManager.UsersRoles.GetUserRolesByID(CG.CombinedUserID)
                                If cgUR IsNot Nothing Then
                                    If cgUR.AlternativesRoles.ContainsKey(node.NodeGuidID) Then
                                        If cgUR.AlternativesRoles(node.NodeGuidID).AllowedAlternativesList.Contains(SourceNode.NodeGuidID) Then
                                            cgUR.AlternativesRoles(node.NodeGuidID).AllowedAlternativesList.Add(CopiedNode.NodeGuidID)
                                        End If
                                        If cgUR.AlternativesRoles(node.NodeGuidID).RestrictedAlternativesList.Contains(SourceNode.NodeGuidID) Then
                                            cgUR.AlternativesRoles(node.NodeGuidID).RestrictedAlternativesList.Add(CopiedNode.NodeGuidID)
                                        End If
                                        If cgUR.AlternativesRoles(node.NodeGuidID).UndefinedAlternativesList.Contains(SourceNode.NodeGuidID) Then
                                            cgUR.AlternativesRoles(node.NodeGuidID).UndefinedAlternativesList.Add(CopiedNode.NodeGuidID)
                                        End If
                                    End If
                                End If
                            Next

                            ' duplicate judgments
                            Select Case node.MeasureType
                                Case ECMeasureType.mtPairwise, ECMeasureType.mtPWAnalogous
                                    For Each alt As clsNode In SourceNode.Hierarchy.TerminalNodes
                                        Dim pwData As clsPairwiseMeasureData = CType(node.Judgments, clsPairwiseJudgments).PairwiseJudgment(SourceNode.NodeID, alt.NodeID, user.UserID)
                                        If pwData IsNot Nothing AndAlso Not pwData.IsUndefined Then
                                            If SourceNode.NodeID = pwData.FirstNodeID Then
                                                node.Judgments.AddMeasureData(New clsPairwiseMeasureData(CopiedNode.NodeID, pwData.SecondNodeID, pwData.Advantage, pwData.Value, node.NodeID, user.UserID, pwData.IsUndefined, pwData.Comment), False)
                                            Else
                                                node.Judgments.AddMeasureData(New clsPairwiseMeasureData(pwData.FirstNodeID, CopiedNode.NodeID, pwData.Advantage, pwData.Value, node.NodeID, user.UserID, pwData.IsUndefined, pwData.Comment), False)
                                            End If
                                        End If
                                    Next
                                Case Else
                                    Dim nonpwData As clsNonPairwiseMeasureData = CType(node.Judgments, clsNonPairwiseJudgments).GetJudgement(SourceNode.NodeID, node.NodeID, user.UserID)
                                    If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                node.Judgments.AddMeasureData(New clsRatingMeasureData(CopiedNode.NodeID, node.NodeID, user.UserID, CType(nonpwData, clsRatingMeasureData).Rating, node.MeasurementScale, nonpwData.IsUndefined), False)
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                node.Judgments.AddMeasureData(New clsUtilityCurveMeasureData(CopiedNode.NodeID, node.NodeID, user.UserID, nonpwData.ObjectValue, node.MeasurementScale, nonpwData.IsUndefined), False)
                                            Case ECMeasureType.mtStep
                                                node.Judgments.AddMeasureData(New clsStepMeasureData(CopiedNode.NodeID, node.NodeID, user.UserID, nonpwData.ObjectValue, node.MeasurementScale, nonpwData.IsUndefined), False)
                                            Case ECMeasureType.mtDirect
                                                node.Judgments.AddMeasureData(New clsDirectMeasureData(CopiedNode.NodeID, node.NodeID, user.UserID, nonpwData.ObjectValue, nonpwData.IsUndefined), False)
                                        End Select
                                    End If
                            End Select
                        Next
                    Else
                        If UR IsNot Nothing Then
                            If UR.ObjectivesRoles.Allowed.Contains(SourceNode.NodeGuidID) Then
                                UR.ObjectivesRoles.Allowed.Add(CopiedNode.NodeGuidID)
                            End If
                            If UR.ObjectivesRoles.Restricted.Contains(SourceNode.NodeGuidID) Then
                                UR.ObjectivesRoles.Restricted.Add(CopiedNode.NodeGuidID)
                            End If
                            If UR.ObjectivesRoles.Undefined.Contains(SourceNode.NodeGuidID) Then
                                UR.ObjectivesRoles.Undefined.Add(CopiedNode.NodeGuidID)
                            End If

                            If UR.AlternativesRoles.ContainsKey(SourceNode.NodeGuidID) Then
                                For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                                    If Not UR.AlternativesRoles.ContainsKey(CopiedNode.NodeGuidID) Then
                                        UR.AlternativesRoles.Add(CopiedNode.NodeGuidID, New clsAlternativesRoles)
                                    End If
                                    If UR.AlternativesRoles(SourceNode.NodeGuidID).AllowedAlternativesList.Contains(alt.NodeGuidID) Then
                                        UR.AlternativesRoles(CopiedNode.NodeGuidID).AllowedAlternativesList.Add(alt.NodeGuidID)
                                    End If
                                    If UR.AlternativesRoles(SourceNode.NodeGuidID).RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
                                        UR.AlternativesRoles(CopiedNode.NodeGuidID).RestrictedAlternativesList.Add(alt.NodeGuidID)
                                    End If
                                    If UR.AlternativesRoles(SourceNode.NodeGuidID).UndefinedAlternativesList.Contains(alt.NodeGuidID) Then
                                        UR.AlternativesRoles(CopiedNode.NodeGuidID).UndefinedAlternativesList.Add(alt.NodeGuidID)
                                    End If
                                Next
                            End If
                        End If

                        For Each CG As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                            Dim cgUR As clsUserRoles = ProjectManager.UsersRoles.GetUserRolesByID(CG.CombinedUserID)
                            If cgUR IsNot Nothing Then
                                If cgUR.AlternativesRoles.ContainsKey(SourceNode.NodeGuidID) Then
                                    For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                                        If Not cgUR.AlternativesRoles.ContainsKey(CopiedNode.NodeGuidID) Then
                                            cgUR.AlternativesRoles.Add(CopiedNode.NodeGuidID, New clsAlternativesRoles)
                                        End If
                                        If cgUR.AlternativesRoles(SourceNode.NodeGuidID).AllowedAlternativesList.Contains(alt.NodeGuidID) Then
                                            cgUR.AlternativesRoles(CopiedNode.NodeGuidID).AllowedAlternativesList.Add(alt.NodeGuidID)
                                        End If
                                        If cgUR.AlternativesRoles(SourceNode.NodeGuidID).RestrictedAlternativesList.Contains(alt.NodeGuidID) Then
                                            cgUR.AlternativesRoles(CopiedNode.NodeGuidID).RestrictedAlternativesList.Add(alt.NodeGuidID)
                                        End If
                                        If cgUR.AlternativesRoles(SourceNode.NodeGuidID).UndefinedAlternativesList.Contains(alt.NodeGuidID) Then
                                            cgUR.AlternativesRoles(CopiedNode.NodeGuidID).UndefinedAlternativesList.Add(alt.NodeGuidID)
                                        End If
                                    Next
                                End If
                            End If
                        Next

                        If SourceNode.ParentNode IsNot Nothing AndAlso (SourceNode.ParentNode Is CopiedNode.ParentNode) Then
                            If IsPWMeasurementType(SourceNode.ParentNode.MeasureType) Then
                                For Each pwData As clsPairwiseMeasureData In CType(SourceNode.ParentNode.Judgments, clsPairwiseJudgments).JudgmentsFromUser(user.UserID)
                                    If pwData IsNot Nothing AndAlso Not pwData.IsUndefined Then
                                        If SourceNode.NodeID = pwData.FirstNodeID Then
                                            SourceNode.ParentNode.Judgments.AddMeasureData(New clsPairwiseMeasureData(CopiedNode.NodeID, pwData.SecondNodeID, pwData.Advantage, pwData.Value, SourceNode.ParentNode.NodeID, user.UserID, pwData.IsUndefined, pwData.Comment), False)
                                        Else
                                            If SourceNode.NodeID = pwData.SecondNodeID Then
                                                SourceNode.ParentNode.Judgments.AddMeasureData(New clsPairwiseMeasureData(pwData.FirstNodeID, CopiedNode.NodeID, pwData.Advantage, pwData.Value, SourceNode.ParentNode.NodeID, user.UserID, pwData.IsUndefined, pwData.Comment), False)
                                            End If
                                        End If
                                    End If
                                Next
                            Else
                                For Each nonpwData As clsNonPairwiseMeasureData In CType(SourceNode.ParentNode.Judgments, clsNonPairwiseJudgments).JudgmentsFromUser(user.UserID)
                                    If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                        Select Case SourceNode.ParentNode.MeasureType
                                            Case ECMeasureType.mtRatings
                                                SourceNode.ParentNode.Judgments.AddMeasureData(New clsRatingMeasureData(CopiedNode.NodeID, SourceNode.ParentNode.NodeID, user.UserID, CType(nonpwData, clsRatingMeasureData).Rating, SourceNode.ParentNode.MeasurementScale, nonpwData.IsUndefined), False)
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                SourceNode.ParentNode.Judgments.AddMeasureData(New clsUtilityCurveMeasureData(CopiedNode.NodeID, SourceNode.ParentNode.NodeID, user.UserID, nonpwData.ObjectValue, SourceNode.ParentNode.MeasurementScale, nonpwData.IsUndefined), False)
                                            Case ECMeasureType.mtStep
                                                SourceNode.ParentNode.Judgments.AddMeasureData(New clsStepMeasureData(CopiedNode.NodeID, SourceNode.ParentNode.NodeID, user.UserID, nonpwData.ObjectValue, SourceNode.ParentNode.MeasurementScale, nonpwData.IsUndefined), False)
                                            Case ECMeasureType.mtDirect
                                                SourceNode.ParentNode.Judgments.AddMeasureData(New clsDirectMeasureData(CopiedNode.NodeID, SourceNode.ParentNode.NodeID, user.UserID, nonpwData.ObjectValue, nonpwData.IsUndefined), False)
                                        End Select
                                    End If
                                Next
                            End If
                        End If

                        If IsPWMeasurementType(SourceNode.MeasureType) Then
                            For Each pwData As clsPairwiseMeasureData In CType(SourceNode.Judgments, clsPairwiseJudgments).JudgmentsFromUser(user.UserID)
                                If pwData IsNot Nothing AndAlso Not pwData.IsUndefined Then
                                    CopiedNode.Judgments.AddMeasureData(New clsPairwiseMeasureData(pwData.FirstNodeID, pwData.SecondNodeID, pwData.Advantage, pwData.Value, CopiedNode.NodeID, user.UserID, pwData.IsUndefined, pwData.Comment), False)
                                End If
                            Next
                        Else
                            For Each nonpwData As clsNonPairwiseMeasureData In CType(SourceNode.Judgments, clsNonPairwiseJudgments).JudgmentsFromUser(user.UserID)
                                If nonpwData IsNot Nothing AndAlso Not nonpwData.IsUndefined Then
                                    Select Case SourceNode.MeasureType
                                        Case ECMeasureType.mtRatings
                                            CopiedNode.Judgments.AddMeasureData(New clsRatingMeasureData(nonpwData.NodeID, CopiedNode.NodeID, user.UserID, CType(nonpwData, clsRatingMeasureData).Rating, SourceNode.MeasurementScale, nonpwData.IsUndefined), False)
                                        Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                            CopiedNode.Judgments.AddMeasureData(New clsUtilityCurveMeasureData(nonpwData.NodeID, CopiedNode.NodeID, user.UserID, nonpwData.ObjectValue, SourceNode.MeasurementScale, nonpwData.IsUndefined), False)
                                        Case ECMeasureType.mtStep
                                            CopiedNode.Judgments.AddMeasureData(New clsStepMeasureData(nonpwData.NodeID, CopiedNode.NodeID, user.UserID, nonpwData.ObjectValue, SourceNode.MeasurementScale, nonpwData.IsUndefined), False)
                                        Case ECMeasureType.mtDirect
                                            CopiedNode.Judgments.AddMeasureData(New clsDirectMeasureData(nonpwData.NodeID, CopiedNode.NodeID, user.UserID, nonpwData.ObjectValue, nonpwData.IsUndefined), False)
                                    End Select
                                End If
                            Next
                        End If
                    End If

                    ProjectManager.StorageManager.Writer.SaveUserJudgments(user)
                    ProjectManager.StorageManager.Writer.SaveUserPermissions(user)
                Next
            End If

            Return resList
        End Function

        'copy node saving the hierarchy
        Private Sub CopyNode(ByVal SourceNode As ECCore.clsNode, ByVal CopiedNode As ECCore.clsNode, ByVal ParentNode As ECCore.clsNode, Optional ByVal resList As List(Of ECCore.clsNode) = Nothing)
            If SourceNode IsNot Nothing Then
                CloneNode(SourceNode, CopiedNode, ParentNode)

                If SourceNode.Children IsNot Nothing AndAlso SourceNode.Children.Count > 0 Then

                    CopiedNode.Children = New List(Of ECCore.clsNode)

                    For Each child As ECCore.clsNode In SourceNode.Children
                        'C0827===
                        'Dim newChild As New ECCore.clsNode(child.Hierarchy)
                        'CopiedNode.Children.Add(newChild)
                        'C0827==
                        Dim newChild As clsNode = AddNode(CopiedNode.NodeID) 'C0827

                        If resList IsNot Nothing Then resList.Add(newChild)

                        CopyNode(child, newChild, CopiedNode, resList)
                    Next
                End If
            End If
        End Sub

        Private Sub CloneNode(ByVal FromNode As ECCore.clsNode, ByVal ToNode As ECCore.clsNode, ByVal ParentNode As ECCore.clsNode)
            ToNode.NodeName = FromNode.NodeName
            ToNode.MeasureMode = FromNode.MeasureMode
            ToNode.MeasureType = FromNode.MeasureType
            ToNode.RatingScaleID = FromNode.RatingScaleID 'C0827
            ToNode.AdvancedUtilityCurveID = FromNode.AdvancedUtilityCurveID
            ToNode.RegularUtilityCurveID = FromNode.RegularUtilityCurveID
            ToNode.StepFunctionID = FromNode.StepFunctionID
            ToNode.IsAlternative = FromNode.IsAlternative
            ToNode.ParentNode = ParentNode
            If ParentNode IsNot Nothing Then ToNode.ParentNodeID = ParentNode.NodeID 'A0717
            ToNode.InfoDoc = FromNode.InfoDoc 'C0827
            ToNode.Comment = FromNode.Comment 'C0827
            If ToNode.MeasureType = ECMeasureType.mtPairwise Then
                ToNode.Judgments = New clsPairwiseJudgments(ToNode)
            Else
                ToNode.Judgments = New clsNonPairwiseJudgments(ToNode)
            End If

        End Sub
        'A0340 ==

        Public ReadOnly Property RootNodes() As List(Of clsNode) 'C0642
            Get
                Return Nodes.Where(Function(n) (n.ParentNode Is Nothing)).ToList
            End Get
        End Property

        Public Function GetUncontributedAlternatives() As List(Of clsNode)
            Dim res As New List(Of clsNode)
            For Each alt As clsNode In ProjectManager.ActiveAlternatives.TerminalNodes
                Dim HasContributions As Boolean
                If (HierarchyID = ECHierarchyID.hidLikelihood) And (Nodes.Count = 1) Then
                    HasContributions = False
                Else
                    HasContributions = MiscFuncs.HasContribution(alt, Me)
                End If
                If Not HasContributions Then res.Add(alt)
            Next
            Return res
        End Function

        Public Sub FixChildrenLinks()
            For Each node As clsNode In Nodes
                If node.ParentNodesGuids.Count > 0 Then
                    For Each pGuid As Guid In node.ParentNodesGuids
                        Dim pNode As clsNode = GetNodeByID(pGuid)
                        If pNode IsNot Nothing Then
                            If Not pNode.Children.Contains(node) Then
                                pNode.Children.Add(node)
                            End If
                        End If
                    Next
                Else
                    If node.ParentNode IsNot Nothing Then
                        node.ParentNodesGuids.Add(node.ParentNode.NodeGuidID)
                    End If
                End If
            Next
        End Sub

        Public Function HasCompleteHierarchy() As Boolean
            For Each node As clsNode In Nodes
                If node.ParentNodesGuids.Count > 1 Then
                    Dim count As Integer = 0
                    For Each parentGuid As Guid In node.ParentNodesGuids
                        Dim parent As clsNode = GetNodeByID(parentGuid)
                        If parent IsNot Nothing Then count += 1
                        If count > 1 Then Return True
                    Next
                End If
            Next

            Return False
        End Function

        Private Function GetNodesByOldGuidID(H As clsHierarchy, ByVal ID As Guid) As List(Of clsNode)
            Return H.Nodes.Where(Function(n) n.OldNodeGuidID.Equals(ID)).ToList
        End Function

        Private Sub FillIncompleteHierarchy(SourceNode As clsNode, DestNode As clsNode)
            CloneNode(SourceNode, DestNode, DestNode.ParentNode)
            DestNode.OldNodeGuidID = SourceNode.NodeGuidID

            For Each child As clsNode In SourceNode.Children
                Dim newChild As clsNode = DestNode.Hierarchy.AddNode(DestNode.NodeID, True)
                FillIncompleteHierarchy(child, newChild)
            Next
        End Sub

        Private Function GetNewNodeID(ChildID As Guid, NewNode As clsNode) As Integer
            For Each child As clsNode In NewNode.Children
                If child.OldNodeGuidID.Equals(ChildID) Then
                    Return child.NodeID
                End If
            Next
            Return -2
        End Function

        Public Sub ConvertToIncompleteHierachy(fCopyJudgments As Boolean)   ' D3492
            ProjectManager.StorageManager.Reader.LoadUserData()

            Dim NewH As clsHierarchy = ProjectManager.AddHierarchy
            Dim NewHID As Integer = NewH.HierarchyID

            FillIncompleteHierarchy(Nodes(0), NewH.Nodes(0))
            NewH.FixChildrenLinks()

            If fCopyJudgments Then
                For Each node As clsNode In NewH.Nodes
                    Dim oldNode As clsNode = GetNodeByID(node.OldNodeGuidID)
                    If oldNode IsNot Nothing Then
                        Dim nodesBelow As List(Of clsNode) = oldNode.GetNodesBelow(UNDEFINED_USER_ID)
                        If node.IsTerminalNode Then
                            If IsPWMeasurementType(node.MeasureType) Then
                                For Each pwData As clsPairwiseMeasureData In GetNodeByID(node.OldNodeGuidID).Judgments.JudgmentsFromAllUsers
                                    If nodesBelow.FirstOrDefault(Function(n) (n.NodeID = pwData.FirstNodeID)) IsNot Nothing AndAlso nodesBelow.FirstOrDefault(Function(n) (n.NodeID = pwData.SecondNodeID)) IsNot Nothing Then
                                        Dim newpwData As New clsPairwiseMeasureData(pwData.FirstNodeID, pwData.SecondNodeID, pwData.Advantage, pwData.Value, node.NodeID, pwData.UserID, pwData.IsUndefined, pwData.Comment)
                                        newpwData.ModifyDate = pwData.ModifyDate
                                        Dim outcomesNode As clsNode = GetNodeByID(pwData.OutcomesNodeID)
                                        If outcomesNode IsNot Nothing Then
                                            newpwData.OutcomesNodeID = GetNewNodeID(outcomesNode.NodeGuidID, node)
                                        End If
                                        node.Judgments.AddMeasureData(newpwData, True)
                                    End If
                                Next
                            Else
                                For Each nonpwData As clsNonPairwiseMeasureData In GetNodeByID(node.OldNodeGuidID).Judgments.JudgmentsFromAllUsers
                                    If nodesBelow.FirstOrDefault(Function(n) (n.NodeID = nonpwData.NodeID)) IsNot Nothing Then
                                        Dim newnonpwData As clsNonPairwiseMeasureData = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                newnonpwData = New clsRatingMeasureData(nonpwData.NodeID, node.NodeID, nonpwData.UserID, CType(nonpwData, clsRatingMeasureData).Rating, node.MeasurementScale, nonpwData.IsUndefined, nonpwData.Comment)
                                            Case (ECMeasureType.mtDirect)
                                                newnonpwData = New clsDirectMeasureData(nonpwData.NodeID, node.NodeID, nonpwData.UserID, nonpwData.ObjectValue, nonpwData.IsUndefined, nonpwData.Comment)
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                newnonpwData = New clsUtilityCurveMeasureData(nonpwData.NodeID, node.NodeID, nonpwData.UserID, nonpwData.ObjectValue, node.MeasurementScale, nonpwData.IsUndefined, nonpwData.Comment)
                                            Case ECMeasureType.mtStep
                                                newnonpwData = New clsStepMeasureData(nonpwData.NodeID, node.NodeID, nonpwData.UserID, nonpwData.ObjectValue, node.MeasurementScale, nonpwData.IsUndefined, nonpwData.Comment)
                                        End Select
                                        If newnonpwData IsNot Nothing Then
                                            newnonpwData.ModifyDate = nonpwData.ModifyDate
                                            'newnonpwData.ParentNodeID = node.NodeID
                                            'newnonpwData.ObjectValue = nonpwData.ObjectValue
                                            'newnonpwData.NodeID = GetNewNodeID(GetNodeByID(nonpwData.NodeID).NodeGuidID, node)
                                            'newnonpwData.NodeID = nonpwData.NodeID
                                            'newnonpwData.Comment = nonpwData.Comment
                                            'newnonpwData.UserID = nonpwData.UserID
                                            'newnonpwData.IsUndefined = nonpwData.IsUndefined
                                            node.Judgments.AddMeasureData(newnonpwData, True)
                                        End If
                                    End If
                                Next
                            End If

                            For Each AltID As Integer In GetNodeByID(node.OldNodeGuidID).ChildrenAlts
                                node.ChildrenAlts.Add(AltID)
                            Next
                        Else
                            If IsPWMeasurementType(node.MeasureType) Then
                                For Each pwData As clsPairwiseMeasureData In GetNodeByID(node.OldNodeGuidID).Judgments.JudgmentsFromAllUsers
                                    If nodesBelow.FirstOrDefault(Function(n) (n.NodeID = pwData.FirstNodeID)) IsNot Nothing AndAlso nodesBelow.FirstOrDefault(Function(n) (n.NodeID = pwData.SecondNodeID)) IsNot Nothing Then
                                        Dim newpwData As New clsPairwiseMeasureData(GetNewNodeID(GetNodeByID(pwData.FirstNodeID).NodeGuidID, node), GetNewNodeID(GetNodeByID(pwData.SecondNodeID).NodeGuidID, node), pwData.Advantage, pwData.Value, node.NodeID, pwData.UserID, pwData.IsUndefined, pwData.Comment)
                                        newpwData.ModifyDate = pwData.ModifyDate
                                        Dim outcomesNode As clsNode = GetNodeByID(pwData.OutcomesNodeID)
                                        If outcomesNode IsNot Nothing Then
                                            newpwData.OutcomesNodeID = GetNewNodeID(outcomesNode.NodeGuidID, node)
                                        End If
                                        node.Judgments.AddMeasureData(newpwData, True)
                                    End If
                                Next
                            Else
                                For Each nonpwData As clsNonPairwiseMeasureData In GetNodeByID(node.OldNodeGuidID).Judgments.JudgmentsFromAllUsers
                                    If nodesBelow.FirstOrDefault(Function(n) (n.NodeID = nonpwData.NodeID)) IsNot Nothing Then
                                        Dim newnonpwData As clsNonPairwiseMeasureData = Nothing
                                        Select Case node.MeasureType
                                            Case ECMeasureType.mtRatings
                                                newnonpwData = New clsRatingMeasureData(GetNewNodeID(GetNodeByID(nonpwData.NodeID).NodeGuidID, node), node.NodeID, nonpwData.UserID, CType(nonpwData, clsRatingMeasureData).Rating, node.MeasurementScale, nonpwData.IsUndefined, nonpwData.Comment)
                                            Case (ECMeasureType.mtDirect)
                                                newnonpwData = New clsDirectMeasureData(GetNewNodeID(GetNodeByID(nonpwData.NodeID).NodeGuidID, node), node.NodeID, nonpwData.UserID, nonpwData.ObjectValue, nonpwData.IsUndefined, nonpwData.Comment)
                                            Case ECMeasureType.mtRegularUtilityCurve, ECMeasureType.mtAdvancedUtilityCurve
                                                newnonpwData = New clsUtilityCurveMeasureData(GetNewNodeID(GetNodeByID(nonpwData.NodeID).NodeGuidID, node), node.NodeID, nonpwData.UserID, nonpwData.ObjectValue, node.MeasurementScale, nonpwData.IsUndefined, nonpwData.Comment)
                                            Case ECMeasureType.mtStep
                                                newnonpwData = New clsStepMeasureData(GetNewNodeID(GetNodeByID(nonpwData.NodeID).NodeGuidID, node), node.NodeID, nonpwData.UserID, nonpwData.ObjectValue, node.MeasurementScale, nonpwData.IsUndefined, nonpwData.Comment)
                                        End Select
                                        If newnonpwData IsNot Nothing Then
                                            newnonpwData.ModifyDate = nonpwData.ModifyDate
                                            'newnonpwData.ParentNodeID = node.NodeID
                                            'newnonpwData.ObjectValue = nonpwData.ObjectValue
                                            'newnonpwData.NodeID = GetNewNodeID(GetNodeByID(nonpwData.NodeID).NodeGuidID, node)
                                            'newnonpwData.NodeID = nonpwData.NodeID
                                            'newnonpwData.Comment = nonpwData.Comment
                                            'newnonpwData.UserID = nonpwData.UserID
                                            'newnonpwData.IsUndefined = nonpwData.IsUndefined

                                            node.Judgments.AddMeasureData(newnonpwData, True)
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                Next
            End If

            Dim userIDs As New List(Of Integer)
            For Each user As clsUser In ProjectManager.UsersList
                userIDs.Add(user.UserID)
            Next
            For Each group As clsCombinedGroup In ProjectManager.CombinedGroups.GroupsList
                userIDs.Add(group.CombinedUserID)
            Next

            For Each id As Integer In userIDs
                Dim UR As clsUserRoles = ProjectManager.UsersRoles.GetUserRolesByID(id)

                If UR IsNot Nothing Then
                    ' allowed
                    Dim allowed As New List(Of Guid)
                    For Each nodeGuid As Guid In UR.ObjectivesRoles.Allowed
                        Dim nodesList As List(Of clsNode) = NewH.GetNodesByOldGuidID(NewH, nodeGuid)
                        For Each node As clsNode In nodesList
                            allowed.Add(node.NodeGuidID)
                        Next
                    Next
                    UR.ObjectivesRoles.Allowed = Nothing
                    UR.ObjectivesRoles.Allowed = allowed

                    ' restricted
                    Dim restricted As New List(Of Guid)
                    For Each nodeGuid As Guid In UR.ObjectivesRoles.Restricted
                        Dim nodesList As List(Of clsNode) = NewH.GetNodesByOldGuidID(NewH, nodeGuid)
                        For Each node As clsNode In nodesList
                            restricted.Add(node.NodeGuidID)
                        Next
                    Next
                    UR.ObjectivesRoles.Restricted = Nothing
                    UR.ObjectivesRoles.Restricted = restricted

                    ' undefined
                    Dim undefined As New List(Of Guid)
                    For Each nodeGuid As Guid In UR.ObjectivesRoles.Undefined
                        Dim nodesList As List(Of clsNode) = NewH.GetNodesByOldGuidID(NewH, nodeGuid)
                        For Each node As clsNode In nodesList
                            undefined.Add(node.NodeGuidID)
                        Next
                    Next
                    UR.ObjectivesRoles.Undefined = Nothing
                    UR.ObjectivesRoles.Undefined = undefined

                    ' alternatives
                    Dim AltRoles As New Dictionary(Of Guid, clsAlternativesRoles)
                    For Each kvp As KeyValuePair(Of Guid, clsAlternativesRoles) In UR.AlternativesRoles
                        Dim nodesList As List(Of clsNode) = NewH.GetNodesByOldGuidID(NewH, kvp.Key)
                        For Each node As clsNode In nodesList
                            Dim roles As New clsAlternativesRoles
                            roles.CoveringObjectiveID = node.NodeGuidID
                            roles.AllowedAlternativesList = kvp.Value.AllowedAlternativesList
                            roles.RestrictedAlternativesList = kvp.Value.RestrictedAlternativesList
                            roles.UndefinedAlternativesList = kvp.Value.UndefinedAlternativesList
                            AltRoles.Add(node.NodeGuidID, roles)
                        Next
                    Next
                    UR.AlternativesRoles = AltRoles
                End If
            Next

            For Each node As clsNode In Nodes
                Dim nodeslist As List(Of clsNode) = GetNodesByOldGuidID(NewH, node.NodeGuidID)

                ' user disabled nodes
                For Each newNode As clsNode In nodeslist
                    newNode.DisabledUserIDsList = node.DisabledUserIDsList
                Next

                ' attributes
                For Each attrValue As clsAttributeValue In ProjectManager.Attributes.GetObjectAttributesValues(node.NodeGuidID)
                    For Each newNode As clsNode In nodeslist
                        ProjectManager.Attributes.SetAttributeValue(attrValue.AttributeID, attrValue.UserID, attrValue.ValueType, attrValue.Value, newNode.NodeGuidID, Guid.Empty)
                    Next
                    ProjectManager.Attributes.SetAttributeValue(attrValue.AttributeID, attrValue.UserID, attrValue.ValueType, Nothing, node.NodeGuidID, Guid.Empty)
                Next
            Next

            NewH.HierarchyID = HierarchyID
            HierarchyID = NewHID
            ProjectManager.Hierarchies.Remove(Me)
        End Sub

        Private Sub ReorderByLevels(node As clsNode, ByRef result As List(Of clsNode))
            For Each child As clsNode In node.Children
                If Not child.Sorted Then
                    result.Add(child)
                    child.Sorted = True
                End If
            Next
            For Each child As clsNode In node.Children
                ReorderByLevels(child, result)
            Next
        End Sub

        Public Sub SortNodesByLevels()
            For Each node As clsNode In Nodes
                node.Sorted = False
            Next

            Dim res As New List(Of clsNode)
            Nodes(0).Sorted = True
            res.Add(Nodes(0))
            ReorderByLevels(Nodes(0), res)
            Nodes = res
        End Sub

        'A1118 ===
        Public ReadOnly Property NodesInLinearOrder(Optional HideDuplicateNodes As Boolean = False) As List(Of Tuple(Of Integer, Integer, clsNode))
            Get
                If ProjectManager IsNot Nothing Then
                    SortNodesByLevels()
                    ProjectManager.CreateHierarchyLevelValuesCH(Me)
                End If
                Dim retVal As New List(Of Tuple(Of Integer, Integer, clsNode))
                GetLinearOrder(Nodes(0), Nothing, retVal, HideDuplicateNodes)
                Return retVal
            End Get
        End Property

        Private Sub GetLinearOrder(Node As clsNode, WRTNode As clsNode, retVal As List(Of Tuple(Of Integer, Integer, clsNode)), HideDuplicateNodes As Boolean)
            If Node IsNot Nothing AndAlso Not Node.IsAlternative AndAlso (Not HideDuplicateNodes OrElse retVal.Find(Function(x) x.Item1 = Node.NodeID) Is Nothing) Then
                retVal.Add(New Tuple(Of Integer, Integer, clsNode)(Node.NodeID, If(WRTNode Is Nothing, -1, WRTNode.NodeID), Node))
                For Each child As clsNode In Node.Children
                    GetLinearOrder(child, Node, retVal, HideDuplicateNodes)
                Next
            End If
        End Sub
        'A1118 ==

        Public Sub SetCombinedLoaded(CombinedUserID As Integer, Value As Boolean)
            For Each node As clsNode In Nodes
                If Value Then
                    If Not node.CombinedLoaded.Contains(CombinedUserID) Then node.CombinedLoaded.Add(CombinedUserID)
                Else
                    If node.CombinedLoaded.Contains(CombinedUserID) Then node.CombinedLoaded.Remove(CombinedUserID)
                End If
            Next
        End Sub

        Public Sub ClearUserWeights(UserID As Integer)
            For Each node As clsNode In Nodes
                node.Judgments.Weights.ClearUserWeights(UserID)
            Next

            If ProjectManager.IsRiskProject AndAlso HierarchyID = ECHierarchyID.hidLikelihood Then
                For Each alt As clsNode In ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy).Nodes
                    alt.DirectJudgmentsForNoCause.Weights.ClearUserWeights(UserID)
                    alt.PWOutcomesJudgments.Weights.ClearUserWeights(UserID)
                Next
            End If
        End Sub

        Public Sub ResetNodesDictionaries()
            mNodesIntDict.Clear()
            mNodesGuidDict.Clear()
            For Each node As clsNode In Nodes
                mNodesIntDict.Add(node.NodeID, node)
                mNodesGuidDict.Add(node.NodeGuidID, node)
            Next
        End Sub
    End Class
End Namespace