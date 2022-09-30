Partial Class DiagramViewPage
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_MAP_DIAGRAM_VIEW_RISK)
    End Sub

    Public Function GetTitle() As String
        Dim retVal As String = If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, ResString("titleRiskDiagramViewByObj"), ResString("titleRiskDiagramView"))
        If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then
            Dim Node As clsNode = PM.ActiveObjectives.GetNodeByID(SelectedNodeID)
            If Node IsNot Nothing Then retVal = String.Format(ResString(If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "headerRiskDiagramViewByObjImpact", "headerRiskDiagramViewByObjLikelihood")), SafeFormString(Node.NodeName))
        Else
            Dim Alt As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedEventID)
            If Alt IsNot Nothing Then retVal = String.Format(ResString(If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "headerRiskDiagramViewImpact", "headerRiskDiagramViewLikelihood")), SafeFormString(Alt.NodeName))
        End If
        Return retVal '+ " for model " + SafeFormString(App.ActiveProject.ProjectName)
    End Function

    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim sPgid As String = CheckVar("pgid", "").ToLower
        Integer.TryParse(sPgid, CurrentPageID)
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        If isAJAX Then Ajax_Callback(Request.Form.ToString)
    End Sub

#Region "Diagram View"
    Public Property CanShowUpperLevelNodes As Boolean ' Show upper-level nodes in Bow-Tie view
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_BOWTIE_SHOW_FULL_PATHS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_BOWTIE_SHOW_FULL_PATHS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property    

    Public Function GetNodesData() As String
        If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then
            Return GetContributedAltsData()
        Else
            Dim retVal As String = ""
            Dim Nodes As List(Of clsNode) = PM.ActiveObjectives.TerminalNodes
            Dim Alts = PM.ActiveAlternatives
            If Alts.TerminalNodes.Count > 0 Then
                Dim Alt As clsNode = Alts.GetNodeByID(SelectedEventID)
                For Each node As clsNode In Nodes
                    If node.GetContributedAlternatives.Contains(Alt) Then
                        Dim sNodePath As String = ""
                        If node.ParentNode IsNot Nothing Then
                            sNodePath = node.ParentNode.NodePath
                        End If
                        retVal += If(retVal <> "", ",", "") + Api.DiagramNodeToJS(node, sNodePath)
                    End If
                Next
            End If
            Return String.Format("[{0}]", retVal)
        End If
    End Function

    Public Function GetContributedAltsData() As String
        Dim retVal As String = ""
        Dim tNode As clsNode = PM.ActiveObjectives.GetNodeByID(SelectedNodeID)
        If tNode IsNot Nothing Then
            Dim ContrAlts As List(Of clsNode) = tNode.GetContributedAlternatives
            For Each node As clsNode In ContrAlts
                retVal += If(retVal <> "", ",", "") + Api.DiagramNodeToJS(node)
            Next
        End If
        Return String.Format("[{0}]", retVal)
    End Function

    Public Property SelectedEventID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SELECTED_EVENT_ID, UNDEFINED_USER_ID))
            If Guid.TryParse(s, retVal) Then
                Dim Alts = PM.ActiveAlternatives
                If Alts.GetNodeByID(retVal) Is Nothing Then
                    retVal = Guid.Empty
                    If Alts.TerminalNodes.Count > 0 Then retVal = Alts.TerminalNodes()(0).NodeGuidID
                End If
            End If
            Return retVal
        End Get
        Set(value As Guid)
            WriteSetting(PRJ, ATTRIBUTE_RISK_SELECTED_EVENT_ID, AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Public Property SelectedNodeID As Guid
        Get
            Dim Nodes = PM.ActiveObjectives
            Dim retVal As Guid = Guid.Empty
            Dim AttrID As Guid = If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID)
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(AttrID, UNDEFINED_USER_ID))
            If Guid.TryParse(s, retVal) Then
                Dim tNode As clsNode = Nodes.GetNodeByID(retVal)
                If tNode Is Nothing OrElse Not tNode.IsTerminalNode Then
                    retVal = Guid.Empty
                    If Nodes.TerminalNodes.Count > 0 Then retVal = Nodes.TerminalNodes()(0).NodeGuidID
                End If
            Else
                retVal = Nodes.Nodes(0).NodeGuidID
            End If
            Return retVal
        End Get
        Set(value As Guid)
            Dim AttrID As Guid = If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID)
            WriteSetting(PRJ, AttrID, AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Public Function GetSelectedEventData() As String
        If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then
            Dim tNode As clsNode = PM.ActiveObjectives.GetNodeByID(SelectedNodeID)
            Return Api.DiagramNodeToJS(tNode)
        Else
            Dim tAlt As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedEventID)
            If tAlt IsNot Nothing Then
                Return Api.DiagramNodeToJS(tAlt)
            End If
        End If
        Return "{}"
    End Function

    Private Function AnySubObjectiveContributes(Node As clsNode, AltID As Integer) As Boolean
        If Node IsNot Nothing Then
            ' If likelihood and "Sources" is selected then count all nodes
            Dim lkhGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)
            If Node Is lkhGoalNode Then Return True

            ' otherwise check contributions
            Dim ContributedAlternatives = Node.GetContributedAlternatives
            If ContributedAlternatives IsNot Nothing Then
                For Each cAlt In ContributedAlternatives
                    If cAlt.NodeID = AltID Then Return True
                Next
            End If
            If Node.Children IsNot Nothing AndAlso Node.Children.Count > 0 Then
                For Each child In Node.Children
                    Dim tRes As Boolean = False
                    tRes = AnySubObjectiveContributes(child, AltID)
                    If tRes Then Return True
                Next
            End If
        End If
        Return False
    End Function

    Public Function AddAlternatives(ByVal ContributedNodeIDs As List(Of Guid), ByVal NodeNames As String) As Boolean
        Dim retVal As Boolean = False

        Dim AH As ECCore.clsHierarchy = PM.ActiveAlternatives
        If AH IsNot Nothing Then
            Dim namesList As String() = NodeNames.Split(Chr(10))
            Dim altsGuids As List(Of Guid) = New List(Of Guid)
            For Each NodeName As String In namesList                
                If Not String.IsNullOrWhiteSpace(NodeName) Then
                    Dim alt As ECCore.clsNode = AH.AddNode(-1)
                    alt.NodeName = NodeName.Trim
                    'node.InfoDoc = NodeDescription
                    altsGuids.Add(alt.NodeGuidID)
                End If
            Next

            PRJ.SaveStructure("Diagram View - Add Nodes")

            If ContributedNodeIDs IsNot Nothing AndAlso ContributedNodeIDs.Count > 0 Then
                For Each altGuid As Guid In altsGuids
                    PM.UpdateContributions(altGuid, ContributedNodeIDs, CType(PM.ActiveHierarchy, ECHierarchyID))
                Next
            End If

            retVal = True
        End If

        Return retVal
    End Function

    Public Function AddNodes(ByVal HierarchyID As ECCore.ECHierarchyID, ByVal ParentNodeID As Guid, ByVal NodeNames As String) As Boolean
        Dim retVal As Boolean = False

        Dim H As ECCore.clsHierarchy = PM.Hierarchy(HierarchyID)
        If H IsNot Nothing Then
            Dim ParentNode As ECCore.clsNode = H.GetNodeByID(ParentNodeID)

            Dim ParentID As Integer
            If ParentNode Is Nothing Then
                ParentID = -1
            Else
                ParentID = ParentNode.NodeID
            End If

            Dim namesList As String() = NodeNames.Split(Chr(10))

            Dim nodesList As List(Of clsNode) = New List(Of clsNode)

            For Each NodeName As String In namesList
                If Not String.IsNullOrWhiteSpace(NodeName) Then
                    Dim node As ECCore.clsNode = H.AddNode(ParentID)
                    node.NodeName = NodeName.Trim
                    nodesList.Add(node)
                End If
            Next

            H.FixChildrenLinks()

            PRJ.SaveStructure()
            retVal = True

            ' set contributions with newly created nodes
            If SelectedEventID <> Guid.Empty Then
                Dim Alt As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedEventID)

                Dim CovObjIDs As New List(Of Guid)
                For Each Node As clsNode In PM.ActiveObjectives.TerminalNodes
                    If Node.GetContributedAlternatives.Contains(Alt) Then
                        CovObjIDs.Add(Node.NodeGuidID)
                    End If
                Next

                For Each Obj In nodesList
                    If Not CovObjIDs.Contains(Obj.NodeGuidID) Then
                        CovObjIDs.Add(Obj.NodeGuidID)
                    End If
                Next
                PM.UpdateContributions(SelectedEventID, CovObjIDs, CType(PM.ActiveHierarchy, ECHierarchyID), True)
            End If
        End If

        Return retVal
    End Function

#End Region

    Private Function AllCallbackData(sAction As String) As String
        Return String.Format("[""{0}"",{1},{2},{3},""{4}""]", sAction, GetSelectedEventData(), GetNodesData(), Api.get_hierarchy_nodes_data(If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, PM.ActiveObjectives, PM.ActiveAlternatives)), JS_SafeString(GetTitle))
    End Function

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim sResult As String = ""
        Dim sAction As String = GetParam(args, _PARAM_ACTION).ToLower

        If Not String.IsNullOrEmpty(sAction) Then

            Select Case sAction
                Case "show_upper_level_nodes"
                    CanShowUpperLevelNodes = Param2Bool(args, "chk")
                    sResult = String.Format("[""{0}""]", sAction)
                Case "selected_node"
                    Dim tGuid As Guid = New Guid(GetParam(args, "id"))
                    If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then
                        SelectedNodeID = tGuid
                    Else
                        SelectedEventID = tGuid
                    End If
                    sResult = AllCallbackData(sAction)
                Case "add_events"
                    Dim sNames = GetParam(args, "names")
                    Dim ContributedNodesIDs As New List(Of Guid)
                    If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then
                        ContributedNodesIDs.Add(SelectedNodeID)
                    End If
                    AddAlternatives(ContributedNodesIDs, sNames)
                    sResult = AllCallbackData(sAction)
                Case "add_objectives"
                    ' update contributions
                    Dim sIDs As List(Of Guid) = Param2GuidList(GetParam(args, "contributed_nodes"))
                    Dim sGuIDs As New List(Of Guid)
                    For Each intID As Guid In sIDs
                        Dim tNode As clsNode = PM.ActiveObjectives.GetNodeByID(intID)
                        If tNode IsNot Nothing AndAlso tNode.IsTerminalNode AndAlso Not sGuIDs.Contains(tNode.NodeGuidID) Then sGuIDs.Add(tNode.NodeGuidID)
                    Next
                    PM.UpdateContributions(SelectedEventID, sGuIDs, CType(PM.ActiveHierarchy, ECHierarchyID))

                    ' add new nodes
                    Dim sNames As String = GetParam(args, "names")
                    If sNames <> "" Then AddNodes(CType(PM.ActiveHierarchy, ECHierarchyID), PM.ActiveObjectives.Nodes(0).NodeGuidID, sNames)
                    sResult = AllCallbackData(sAction)
                Case "remove_contribution"
                    If Not CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then
                        Dim obj_id As Guid = Guid.Empty
                        Dim tOID As String = GetParam(args, "node_id")
                        If tOID <> "" AndAlso tOID <> "undefined" Then obj_id = New Guid(tOID)
                        Dim tHid As ECHierarchyID = CType(PM.ActiveHierarchy, ECHierarchyID)
                        Dim Alt As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedEventID)
                        If Alt IsNot Nothing AndAlso Not obj_id.Equals(Guid.Empty) AndAlso Not SelectedEventID.Equals(Guid.Empty) Then
                            Dim CovObjIDs As New List(Of Guid)
                            For Each Node As clsNode In PM.Hierarchy(tHid).TerminalNodes
                                If Node.GetContributedAlternatives.Contains(Alt) AndAlso Not Node.NodeGuidID.Equals(obj_id) Then
                                    CovObjIDs.Add(Node.NodeGuidID)
                                End If
                            Next
                            PM.UpdateContributions(SelectedEventID, CovObjIDs, tHid)
                        End If
                    Else
                        Dim alt_id As Guid = Guid.Empty
                        Dim tAID As String = GetParam(args, "node_id")
                        If tAID <> "" AndAlso tAID <> "undefined" Then alt_id = New Guid(tAID)
                        Dim tHid As ECHierarchyID = CType(PM.ActiveHierarchy, ECHierarchyID)
                        Dim Alt As clsNode = PM.ActiveAlternatives.GetNodeByID(alt_id)
                        If Alt IsNot Nothing AndAlso SelectedNodeID.Equals(Guid.Empty) Then
                            Dim CovObjIDs As New List(Of Guid)
                            CovObjIDs.Add(SelectedNodeID)
                            PM.UpdateContributions(alt_id, CovObjIDs, tHid)
                        End If
                    End If
                    sResult = AllCallbackData(sAction)
                Case "get_contributed_nodes"
                    Dim retVal As String = Api.get_hierarchy_nodes_data(PM.ActiveObjectives, SelectedEventID)
                    sResult = String.Format("[""{0}"", {1}]", sAction, retVal)
                'Case "v_splitter_size"
                '    Dim sValue As String = GetParam(args, "value", True)
                '    Dim tValue As Double
                '    If String2Double(sValue, tValue) Then
                '        PM.Parameters.Synthesis_ObjectivesSplitterSize = Convert.ToInt32(tValue)
                '        PM.Parameters.Save()
                '    End If
                '    sResult = String.Format("[""{0}""]", sAction)
            End Select

        End If

        If sResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(sResult)
            Response.End()
        End If
    End Sub

    Private _Api As ProjectManagerWebAPI = Nothing
    Public ReadOnly Property Api As ProjectManagerWebAPI
        Get
            If _Api Is Nothing Then _Api = New ProjectManagerWebAPI
            Return _Api
        End Get
    End Property

End Class