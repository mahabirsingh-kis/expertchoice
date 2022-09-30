Partial Class ProjectManagerWebAPI
    Inherits clsComparionCorePage

    Public Sub New()
        MyBase.New(_PGID_WEBAPI)
    End Sub

    Private Function _Page() As mpWebAPI
        Return CType(Master, mpWebAPI)
    End Function

    Public Function Nodes_All() As List(Of jNode)   ' D5027
        Dim tLst As New List(Of jNode)
        For Each tH As clsHierarchy In App.ActiveProject.ProjectManager.Hierarchies
            If tH.HierarchyType <> ECHierarchyType.htMeasure Then
                For Each tNode As clsNode In tH.Nodes
                    tLst.Add(jNode.CreateFromBaseObject(tNode))
                Next
            End If
        Next
        For Each tNode As clsNode In App.ActiveProject.HierarchyAlternatives.Nodes
            tLst.Add(jNode.CreateFromBaseObject(tNode))
        Next
        Return tLst ' D5027
    End Function

    Public Function get_edges() As String
        Dim retVal As String = ""
        Dim PM As clsProjectManager = App.ActiveProject.ProjectManager
        Dim iEdges As Dictionary(Of Guid, List(Of Edge)) = PM.Edges.Edges
        For Each rowAlt As clsNode In PM.ActiveAlternatives.Nodes
            Dim sEdges As String = ""
            Dim hasEdges As Boolean = iEdges IsNot Nothing AndAlso iEdges.ContainsKey(rowAlt.NodeGuidID) AndAlso iEdges(rowAlt.NodeGuidID) IsNot Nothing AndAlso iEdges(rowAlt.NodeGuidID).Count > 0
            Dim di As Integer = 0
            For Each columnAlt As clsNode In PM.ActiveAlternatives.Nodes
                di = columnAlt.NodeID
                Dim edgeValue As Integer = If(rowAlt Is columnAlt, -1, 0)
                If hasEdges AndAlso edgeValue > -1 Then
                    For Each e As Edge In iEdges(rowAlt.NodeGuidID)
                        If e.FromNode IsNot Nothing AndAlso e.FromNode.NodeGuidID = rowAlt.NodeGuidID Then
                            If e.ToNode IsNot Nothing AndAlso e.ToNode.NodeGuidID = columnAlt.NodeGuidID Then
                                edgeValue = 1
                                Exit For
                            End If
                        End If
                    Next
                End If
                sEdges += If(sEdges = "", "", ",") + String.Format("""d{0}"":{1}", di, edgeValue)
            Next
            retVal += If(retVal = "", "", ",") + String.Format("{{""key"": {0}, ""id"": {1}, ""parentId"": {2}, ""text"": ""{3}"",{4}}}", rowAlt.NodeID + 1, rowAlt.NodeID, 0, JS_SafeString(rowAlt.NodeName), sEdges)
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function set_edge(tFromId As Integer, tToId As Integer, tMT As Integer) As String
        Dim tFromNode As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(tFromId)
        Dim tToNode As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(tToId)

        If tFromNode IsNot Nothing AndAlso tToNode IsNot Nothing Then
            If tMT = ECMeasureType.mtNone Then
                App.ActiveProject.ProjectManager.Edges.RemoveEdge(tFromNode.NodeGuidID, tToNode.NodeGuidID)
                App.ActiveProject.ProjectManager.Edges.Save()
            Else
                App.ActiveProject.ProjectManager.Edges.AddEdge(tFromNode.NodeGuidID, tToNode.NodeGuidID, CType(tMT, ECMeasureType))
                App.ActiveProject.ProjectManager.Edges.Save()
            End If
        End If

        Return "{}"
    End Function

    Public Function check_edges() As String
        Dim sortedList As New List(Of clsNode)
        Dim output As String = ""
        Dim hasCycles As Boolean = App.ActiveProject.ProjectManager.Edges.TopologicalSort(sortedList, output)
        Return String.Format("{{""message"":""{0}"", ""type"":""{1}""}}", If(hasCycles, "Circular reference detected: (" + output + ")", "No circular references detected"), If(hasCycles, "warning", "success"))
    End Function

    Public Function get_simulation_groups() As String
        Dim alts As String = ""
        Dim sources As String = ""
        Dim altgroups As String = ""
        Dim sourcegroups As String = ""

        For Each Alt As clsNode In App.ActiveProject.HierarchyAlternatives.Nodes
            alts += If(alts = "", "", ",") + String.Format("{{""id"":""{0}"",""name"":""{1}"",""parentId"":""-1""", Alt.NodeGuidID, JS_SafeString(Alt.NodeName))
            Dim groupIndex As Integer = 0
            For Each AltGroup As EventsGroup In App.ActiveProject.ProjectManager.EventsGroups.Groups
                Dim sGrpData As String = ",""g" + groupIndex.ToString + """:"
                Dim value = "false"
                For Each x In AltGroup.Events
                    If x.EventGuidID = Alt.NodeGuidID Then
                        value = Bool2JS(x.Precedence > 0)
                    End If
                Next
                alts += sGrpData + value
                groupIndex += 1
            Next
            alts += "}"
        Next

        For Each Node As Tuple(Of Integer, Integer, clsNode) In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).NodesInLinearOrder()
            sources += If(sources = "", "", ",") + String.Format("{{""id"":""{0}"",""name"":""{1}"",""parentId"":""{2}""", Node.Item3.NodeGuidID, JS_SafeString(Node.Item3.NodeName), If(Node.Item3.ParentNodesGuids IsNot Nothing AndAlso Node.Item3.ParentNodesGuids.Count > 0, Node.Item3.ParentNodesGuids(0).ToString, "-1"))
            Dim groupIndex As Integer = 0
            For Each Group As EventsGroup In App.ActiveProject.ProjectManager.SourceGroups.Groups
                Dim sGrpData As String = ",""g" + groupIndex.ToString + """:"
                Dim value = "false"
                For Each x In Group.Events
                    If x.EventGuidID = Node.Item3.NodeGuidID Then
                        value = Bool2JS(x.Precedence > 0)
                    End If
                Next
                sources += sGrpData + value
                groupIndex += 1
            Next
            sources += "}"
        Next

        'altgroups += String.Format("{{""guid"":""{0}"",""name"":""{1}""}}", -1, If(App.ActiveProject.ProjectManager.EventsGroups.Groups.Count = 0, "Click + to add new group", "Select a group ..."))
        For Each AltGroup As EventsGroup In App.ActiveProject.ProjectManager.EventsGroups.Groups
            altgroups += If(altgroups = "", "", ",") + String.Format("{{""guid"":""{0}"",""name"":""{1}"",""behavior"":{2},""enabled"":{3}}}", AltGroup.ID, JS_SafeString(AltGroup.Name), CInt(AltGroup.Behaviour), Bool2JS(AltGroup.Enabled))
        Next

        'sourcegroups += String.Format("{{""guid"":""{0}"",""name"":""{1}""}}", -1, If(App.ActiveProject.ProjectManager.EventsGroups.Groups.Count = 0, "Click + to add new group", "Select a group ..."))
        For Each SourceGroup As EventsGroup In App.ActiveProject.ProjectManager.SourceGroups.Groups
            sourcegroups += If(sourcegroups = "", "", ",") + String.Format("{{""guid"":""{0}"",""name"":""{1}"",""behavior"":{2},""enabled"":{3}}}", SourceGroup.ID, JS_SafeString(SourceGroup.Name), CInt(SourceGroup.Behaviour), Bool2JS(SourceGroup.Enabled))
        Next

        Return String.Format("{{""alternatives"":[{0}],""sources"":[{1}],""altgroups"":[{2}],""sourcegroups"":[{3}]}}", alts, sources, altgroups, sourcegroups)
    End Function

    Public Function add_simulation_group(tabID As Integer, sName As String, tBehavior As Integer, tEnabled As Boolean) As String
        Dim Groups As EventsGroups = If(tabID = 0, App.ActiveProject.ProjectManager.EventsGroups, App.ActiveProject.ProjectManager.SourceGroups)

        If Not String.IsNullOrEmpty(sName) Then
            Dim g As EventsGroup = Groups.AddGroup(sName)
            g.Behaviour = CType(tBehavior, GroupBehaviour)
            g.Enabled = tEnabled
            Groups.Save()
            App.ActiveProject.MakeSnapshot("Simulation group added", sName)
        End If

        Return "{}"
    End Function

    Public Function edit_simulation_group(sGuid As String, tabID As Integer, sName As String, tBehavior As Integer, tEnabled As Boolean) As String
        Dim tGuid As Guid

        Dim Groups As EventsGroups = If(tabID = 0, App.ActiveProject.ProjectManager.EventsGroups, App.ActiveProject.ProjectManager.SourceGroups)

        If Not String.IsNullOrEmpty(sName) AndAlso Guid.TryParse(sGuid, tGuid) Then
            Dim g As EventsGroup = Groups.GetGroup(tGuid)
            If g IsNot Nothing Then
                g.Name = sName
                g.Behaviour = CType(tBehavior, GroupBehaviour)
                g.Enabled = tEnabled
                Groups.Save()
                App.ActiveProject.MakeSnapshot("Simulation group edited", sName)
            End If
        End If

        Return "{}"
    End Function

    Public Function assign_simulation_group(tabID As Integer, groupIndex As Integer, precedence As Integer, nodeGuid As Guid) As String
        Dim Groups As EventsGroups = If(tabID = 0, App.ActiveProject.ProjectManager.EventsGroups, App.ActiveProject.ProjectManager.SourceGroups)
        Dim H As clsHierarchy = If(tabID = 0, App.ActiveProject.ProjectManager.ActiveAlternatives, App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood))
        Dim Node As clsNode = H.GetNodeByID(nodeGuid)
        If Node IsNot Nothing AndAlso groupIndex >= 0 AndAlso groupIndex < Groups.Groups.Count Then
            Dim g = Groups.Groups(groupIndex)
            If precedence >= 0 Then
                g.AddEvent(Node, precedence)
            Else
                g.DeleteEvent(Node)
            End If

            Groups.Save()

            App.ActiveProject.MakeSnapshot("Simulation group items changed", Node.NodeName)
        End If

        Return "{}"
    End Function

    ''' <summary>
    ''' Delete Simulation Group
    ''' </summary>
    ''' <param name="tabID"></param>
    ''' <param name="groupGuid"></param>
    ''' <returns></returns>
    Public Function delete_simulation_group(tabID As Integer, groupGuid As Guid) As String
        Dim Groups As EventsGroups = If(tabID = 0, App.ActiveProject.ProjectManager.EventsGroups, App.ActiveProject.ProjectManager.SourceGroups)
        Dim Group = Groups.GetGroup(groupGuid)
        If Group IsNot Nothing AndAlso Groups.DeleteGroup(groupGuid) Then
            Groups.Save()
            App.ActiveProject.MakeSnapshot("Simulation group deleted", Group.Name)
        End If

        Return "{}"
    End Function

    Public Function get_scenarios() As String
        Dim alts As String = ""
        Dim scenarios As String = ""

        'add default scenarios
        Dim H As clsHierarchy = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
        If H.Nodes.Count = 1 Then
            Dim Scenario1 As clsNode = H.AddNode(H.Nodes(0).NodeID)
            With Scenario1
                .NodeName = ParseString("%%Objective(l)%% 1")
                .RiskNodeType = RiskNodeType.ntCategory
            End With
            Dim Scenario2 As clsNode = H.AddNode(H.Nodes(0).NodeID)
            With Scenario2
                .NodeName = ParseString("%%Objective(l)%% 2")
                .RiskNodeType = RiskNodeType.ntCategory
            End With
        End If

        For Each scenario As clsNode In H.Nodes(0).Children
            If Not scenario.IsAlternative Then
                Dim alternatives As String = ""
                For Each alt As clsNode In scenario.GetContributedAlternatives()
                    alternatives += If(alternatives = "", "", ",") + EventToJSON(alt)
                Next
                scenarios += If(scenarios = "", "", ",") + String.Format("{{""id"":{0},""guid"":""{1}"",""name"":""{2}"",""description"":""{3}"",""events"":[{4}]", scenario.NodeID, scenario.NodeGuidID, JS_SafeString(scenario.NodeName), JS_SafeString(Infodoc2Text(App.ActiveProject, scenario.InfoDoc, False)), alternatives)
            End If
            scenarios += "}"
        Next

        Return String.Format("{{""scenarios"":[{0}]}}", scenarios)
    End Function

    Private Function EventToJSON(alt As clsNode) As String
        Dim objs As String = ""
        For Each obj As clsNode In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes
            If obj.GetContributedAlternatives.Contains(alt) Then 
                objs += If(objs = "", "", ",") + String.Format("{{""id"":{0},""guid"":""{1}"",""name"":""{2}"",""description"":""{3}""}}", obj.NodeID, obj.NodeGuidID, JS_SafeString(obj.NodeName), JS_SafeString(Infodoc2Text(App.ActiveProject, obj.InfoDoc, False)))
            End If
        Next
        Return String.Format("{{""id"":{0},""guid"":""{1}"",""name"":""{2}"",""description"":""{3}"",""type"":{4},""objectives"":[{5}]}}", alt.NodeID, alt.NodeGuidID, JS_SafeString(alt.NodeName), JS_SafeString(Infodoc2Text(App.ActiveProject, alt.InfoDoc, False)), CInt(alt.EventType), objs)
    End Function

    Public Function move_node(fromID As Integer, toID As Integer, HierarchyID As Integer, MoveAction As NodeMoveAction) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim H As clsHierarchy = If(HierarchyID = -1, App.ActiveProject.ProjectManager.ActiveAlternatives, App.ActiveProject.ProjectManager.Hierarchy(HierarchyID))
        Dim SourceNode As clsNode = H.GetNodeByID(fromID)
        Dim TargetNode As clsNode = H.GetNodeByID(toID)
        If SourceNode IsNot Nothing AndAlso TargetNode IsNot Nothing Then
            If H.MoveNode(SourceNode, TargetNode, MoveAction) Then
                App.ActiveProject.SaveStructure(ParseString("Moved %%Objective(l)%%"), False, True, SourceNode.NodeName)
                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function

    Public Function update_node_name(ID As Integer, Name As String, HierarchyID As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim H As clsHierarchy = If(HierarchyID = -1, App.ActiveProject.ProjectManager.ActiveAlternatives, App.ActiveProject.ProjectManager.Hierarchy(HierarchyID))
        Dim Node As clsNode = H.GetNodeByID(ID)
        If Node IsNot Nothing Then
            Node.NodeName = Name
            App.ActiveProject.SaveStructure("Renamed Node", False, True, Name)
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function update_node_description(ID As Integer, Description As String, HierarchyID As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim H As clsHierarchy = If(HierarchyID = -1, App.ActiveProject.ProjectManager.ActiveAlternatives, App.ActiveProject.ProjectManager.Hierarchy(HierarchyID))
        Dim Node As clsNode = H.GetNodeByID(ID)
        If Node IsNot Nothing Then
            Node.InfoDoc = Description
            'App.ActiveProject.ProjectManager.InfoDocs.SetNodeInfoDoc(Node.NodeGuidID, Description)
            App.ActiveProject.ProjectManager.StorageManager.Writer.SaveInfoDocs()
            App.ActiveProject.MakeSnapshot("Changed Node Description", Node.NodeName)
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function update_node_type(ID As Integer, EventType As Integer, HierarchyID As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Node As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(ID)
        If Node IsNot Nothing Then
            Node.EventType = CType(EventType, EventType)
            App.ActiveProject.SaveStructure(ParseString("Update %%Alternative%% Type"), False, True, Node.NodeName)
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function add_new_event_to_scenario(ScenarioID As Integer, EventName As String, EventType As EventType) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Scenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(ScenarioID)
        If Scenario IsNot Nothing Then
            Dim NewEvent As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.AddNode(-1)
            With NewEvent
                .NodeName = EventName
                .EventType = EventType
            End With

            Dim ContributedNodesGuids As New List(Of Guid)
            ContributedNodesGuids.Add(Scenario.NodeGuidID)
            App.ActiveProject.ProjectManager.UpdateContributions(NewEvent.NodeGuidID, ContributedNodesGuids, ECHierarchyID.hidLikelihood, False)

            App.ActiveProject.SaveStructure(ParseString("%%Alternative%% Added To %%Objective(l)%%"), False, True, String.Format("{0} added to {1}", EventName, Scenario.NodeName))
            Res.Result = ecActionResult.arSuccess
            Res.Data = New jEventData() With {.id = NewEvent.NodeID, .guid = NewEvent.NodeGuidID, .name = NewEvent.NodeName, .type = CInt(NewEvent.EventType)}
        End If

        Return Res
    End Function

    Public Function add_new_objective_to_event(EventID As Integer, ObjectiveName As String, Hid As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}
        
        Dim Alt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(EventID)
        If Alt IsNot Nothing Then
            Dim NewObjective As clsNode = App.ActiveProject.ProjectManager.Hierarchy(Hid).AddNode(App.ActiveProject.ProjectManager.Hierarchy(Hid).Nodes(0).NodeID)
            With NewObjective
                .NodeName = ObjectiveName
            End With

            Dim ContributedNodesGuids As New List(Of Guid)
            For Each obj In App.ActiveProject.ProjectManager.Hierarchy(Hid).TerminalNodes
                If obj.GetContributedAlternatives.Contains(Alt) AndAlso Not ContributedNodesGuids.Contains(obj.NodeGuidID) Then
                    ContributedNodesGuids.Add(obj.NodeGuidID)
                End If
            Next
            If Not ContributedNodesGuids.Contains(NewObjective.NodeGuidID) Then
                ContributedNodesGuids.Add(NewObjective.NodeGuidID)
            End If
            App.ActiveProject.ProjectManager.UpdateContributions(Alt.NodeGuidID, ContributedNodesGuids, CType(Hid, ECHierarchyID), False)

            App.ActiveProject.SaveStructure(ParseString("%%Objective(i)%% Added To %%Alternative%%"), False, True, String.Format("{0} added to {1}", NewObjective, Alt.NodeName))
            Res.Result = ecActionResult.arSuccess
            Res.Data = New jEventData() With {.id = NewObjective.NodeID, .guid = NewObjective.NodeGuidID, .name = NewObjective.NodeName}
        End If

        Return Res
    End Function

    Public Function add_existing_events_to_scenario(ScenarioID As Integer, EventIDs As List(Of Integer)) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Scenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(ScenarioID)
        If Scenario IsNot Nothing Then
            Dim ResAltsList As New List(Of jEventData)
            For Each AltId In EventIDs
                Dim Alt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(AltId)
                Dim ContributedNodesGuids As New List(Of Guid)
                For Each obj In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
                    If obj.GetContributedAlternatives.Contains(Alt) AndAlso Not ContributedNodesGuids.Contains(obj.NodeGuidID) Then
                        ContributedNodesGuids.Add(obj.NodeGuidID)
                    End If
                Next
                If Not ContributedNodesGuids.Contains(Scenario.NodeGuidID) Then
                    ContributedNodesGuids.Add(Scenario.NodeGuidID)
                End If
                App.ActiveProject.ProjectManager.UpdateContributions(Alt.NodeGuidID, ContributedNodesGuids, ECHierarchyID.hidLikelihood, False)
                ResAltsList.Add(New jEventData() With {.id = Alt.NodeID, .guid = Alt.NodeGuidID, .name = Alt.NodeName, .type = CInt(Alt.EventType)})
            Next

            App.ActiveProject.SaveStructure(ParseString("%%Alternatives%% Added To %%Objective(l)%%"), False, True, String.Format(ParseString("{0} %%alternatives%% added to {1}"), ResAltsList.Count, Scenario.NodeName))
            Res.Result = ecActionResult.arSuccess
            Res.Data = ResAltsList
        End If

        Return Res
    End Function
    
    Public Function add_existing_objectives_to_event(EventID As Integer, ObjIDs As List(Of Integer), Hid As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Alt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(EventID)
        If Alt IsNot Nothing Then
            Dim ResObjsList As New List(Of jEventData)
            Dim ContributedNodesGuids As New List(Of Guid)

            For Each obj In App.ActiveProject.ProjectManager.Hierarchy(Hid).TerminalNodes
                If ObjIDs.Contains(Obj.NodeID) OrElse obj.GetContributedAlternatives.Contains(Alt) Then
                    ContributedNodesGuids.Add(obj.NodeGuidID)
                End If
                If ObjIDs.Contains(Obj.NodeID) Then ResObjsList.Add(New jEventData() With {.id = Obj.NodeID, .guid = Obj.NodeGuidID, .name = Obj.NodeName})
            Next

            App.ActiveProject.ProjectManager.UpdateContributions(Alt.NodeGuidID, ContributedNodesGuids, CType(Hid, ECHierarchyID), False)

            App.ActiveProject.SaveStructure(ParseString("%%Objectives(i)%% Added To %%Alternative%%"), False, True, String.Format(ParseString("{0} %%objectives(i)%% added to {1}"), ResObjsList.Count, Alt.NodeName))
            Res.Result = ecActionResult.arSuccess
            Res.Data = ResObjsList
        End If

        Return Res
    End Function

    Public Function move_event_to_scenario(fromID As Integer, toID As Integer, fromScenarioID As Integer, toScenarioID As Integer, MoveAction As NodeMoveAction) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim fromScenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(fromScenarioID)
        Dim toScenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(toScenarioID)
        Dim fromAlt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(fromId)
        Dim toAlt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(toId)
        If fromScenario IsNot Nothing AndAlso toScenario IsNot Nothing AndAlso fromAlt IsNot Nothing Then
            'remove contribution with the first scenario and set up contribution with the second scenario
            Dim ContributedNodesGuids As New List(Of Guid)
            For Each obj In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
                If obj IsNot fromScenario AndAlso obj.GetContributedAlternatives.Contains(fromAlt) Then
                    ContributedNodesGuids.Add(obj.NodeGuidID)
                End If
            Next
            If Not ContributedNodesGuids.Contains(toScenario.NodeGuidID) Then ContributedNodesGuids.Add(toScenario.NodeGuidID)
            App.ActiveProject.ProjectManager.UpdateContributions(fromAlt.NodeGuidID, ContributedNodesGuids, ECHierarchyID.hidLikelihood, False)
            'move event
            If toAlt IsNot Nothing Then App.ActiveProject.ProjectManager.ActiveAlternatives.MoveNode(fromAlt, toAlt, MoveAction)
            'save snapshot
            App.ActiveProject.SaveStructure(ParseString("Moved %%Alternative%%"), False, True, String.Format(ParseString("Moved {0} to {1}"),fromAlt.NodeName, toScenario.NodeName))
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function copy_event_to_scenario(fromID As Integer, toID As Integer, fromScenarioID As Integer, toScenarioID As Integer, MoveAction As NodeMoveAction) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim fromScenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(fromScenarioID)
        Dim toScenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(toScenarioID)
        Dim fromAlt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(fromId)
        Dim toAlt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(toId)
        If fromScenario IsNot Nothing AndAlso toScenario IsNot Nothing AndAlso fromAlt IsNot Nothing Then
            'set up contribution with the second scenario
            Dim ContributedNodesGuids As New List(Of Guid)
            For Each obj In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
                If obj.GetContributedAlternatives.Contains(fromAlt) Then
                    ContributedNodesGuids.Add(obj.NodeGuidID)
                End If
            Next
            If Not ContributedNodesGuids.Contains(toScenario.NodeGuidID) Then ContributedNodesGuids.Add(toScenario.NodeGuidID)
            App.ActiveProject.ProjectManager.UpdateContributions(fromAlt.NodeGuidID, ContributedNodesGuids, ECHierarchyID.hidLikelihood, False)
            'move event
            If toAlt IsNot Nothing Then App.ActiveProject.ProjectManager.ActiveAlternatives.MoveNode(fromAlt, toAlt, MoveAction)
            'save snapshot
            App.ActiveProject.SaveStructure(ParseString("Copied %%Alternative%%"), False, True, String.Format(ParseString("Copied {0} to {1}"),fromAlt.NodeName, toScenario.NodeName))
            Res.Data = jEventData.FromNode(fromAlt, App)
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function delete_event_from_scenario(ScenarioID As Integer, EventID As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Scenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(ScenarioID)
        If Scenario IsNot Nothing Then
            Dim Alt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(EventID)

            Dim ContributedNodesGuids As New List(Of Guid)
            For Each obj In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
                If obj IsNot Scenario AndAlso obj.GetContributedAlternatives.Contains(Alt) Then
                    ContributedNodesGuids.Add(obj.NodeGuidID)
                End If
            Next
            If ContributedNodesGuids.Count > 0 Then
                App.ActiveProject.ProjectManager.UpdateContributions(Alt.NodeGuidID, ContributedNodesGuids, ECHierarchyID.hidLikelihood, False)
            Else
                App.ActiveProject.ProjectManager.ActiveAlternatives.DeleteNode(Alt, False)
            End If

            App.ActiveProject.SaveStructure(ParseString("%%Alternative%% Deleted From %%Objective(l)%%"), False, True, String.Format("{0} deleted from {1}", Alt.NodeName, Scenario.NodeName))
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function delete_objective_from_event(EventID As Integer, ObjID As Integer) As jActionResult 
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Alt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.GetNodeByID(EventID)
        Dim Obj As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(ObjID)
        If Alt IsNot Nothing AndAlso Obj IsNot Nothing Then
            'remove contribution with the objective
            Dim ContributedNodesGuids As New List(Of Guid)
            For Each tObjective In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes
                If tObjective IsNot Obj AndAlso tObjective.GetContributedAlternatives.Contains(Alt) AndAlso Not ContributedNodesGuids.Contains(tObjective.NodeGuidID) Then
                    ContributedNodesGuids.Add(tObjective.NodeGuidID)
                End If
            Next
            App.ActiveProject.ProjectManager.UpdateContributions(Alt.NodeGuidID, ContributedNodesGuids, ECHierarchyID.hidImpact, False)
            'delete objective if no contributions
            Dim ContributedAlts = Obj.GetContributedAlternatives
            If ContributedAlts Is Nothing OrElse ContributedAlts.Count = 0 Then App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).DeleteNode(Obj)
            'save snapshot
            App.ActiveProject.SaveStructure(ParseString("Deleted %%Objective(i)%% From %%Alternative%%"), False, True, String.Format(ParseString("Deleted {0} from {1}"), Obj.NodeName, Alt.NodeName))
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function add_scenario(ScenarioName As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Scenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).AddNode(App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeID)
        If Scenario IsNot Nothing Then
            Scenario.RiskNodeType = RiskNodeType.ntCategory
            Scenario.NodeName = ScenarioName

            App.ActiveProject.SaveStructure(ParseString("%%Objective(l)%% Added"), False, True, String.Format("{0} added", Scenario.NodeName))
            Res.Result = ecActionResult.arSuccess

            Res.Data = New jScenarioData() With {.id = Scenario.NodeID, .guid = Scenario.NodeGuidID, .name = Scenario.NodeName}
        End If

        Return Res
    End Function

    Public Function delete_scenario(ScenarioID As Integer) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        Dim Scenario As clsNode = App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(ScenarioID)
        If Scenario IsNot Nothing Then
            ' delete alternatives which belong to this scenario only
            Dim n As Integer = 0
            Dim i As Integer = 0
            While i < App.ActiveProject.ProjectManager.ActiveAlternatives.Nodes.Count
                Dim Alt As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.Nodes(i)

                Dim ContributedNodesGuids As New List(Of Guid)
                For Each obj In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
                    If obj IsNot Scenario AndAlso obj.GetContributedAlternatives.Contains(Alt) Then
                        ContributedNodesGuids.Add(obj.NodeGuidID)
                    End If
                Next

                If ContributedNodesGuids.Count = 0 Then
                    n += 1
                    App.ActiveProject.ProjectManager.ActiveAlternatives.DeleteNode(Alt, False)
                Else 
                    i += 1
                End If
            End While

            ' delete scenario node
            App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood).DeleteNode(Scenario, False)

            App.ActiveProject.SaveStructure(ParseString("%%Objective(l)%% Deleted"), False, True, String.Format(ParseString("{0} and {1} %%alternative%%(s) deleted"), Scenario.NodeName, n))
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function save_wizard_option(OptionID As String, OptionValue as String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}

        With App.ActiveProject.ProjectManager.Parameters
            Select Case OptionID 
                Case "eventsDisplayMode"
                    .MyRiskReward_ShowEventsMode = OptionValue
                    .Save()
                Case "showDescriptions"
                    .MyRiskReward_ShowDescriptions = Str2Bool(OptionValue)
                    .Save()
            End Select
            Res.Result = ecActionResult.arSuccess
        End With

        Return Res
    End Function

    Public Function DiagramNodeToJS(node As clsNode, Optional sNodePath As String = "", Optional sChecked As Boolean = False) As String
        Return String.Format("{{""id"":""{0}"", ""pid"":""{1}"", ""infodoc"":{2}, ""name"":""{3}"", ""iscategory"":{4}, ""isterminal"":{5}, ""path"":""{6}"", ""checked"":{7}{8}}}", node.NodeGuidID, If(node.ParentNodesGuids IsNot Nothing AndAlso node.ParentNodesGuids.Count > 0 , node.ParentNodesGuids(0).ToString, ""), Bool2JS(node.InfoDoc <> ""), JS_SafeString(node.NodeName), Bool2JS(node.RiskNodeType = RiskNodeType.ntCategory), Bool2JS(node.IsTerminalNode), JS_SafeString(sNodePath), Bool2JS(sChecked), If(node.IsAlternative AndAlso PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Contains(node), ",""is_ns"":true", ""))
    End Function

    Public Function get_hierarchy_nodes_data(nodes as clsHierarchy, Optional SelectedAltID? As Guid = Nothing) as String
        Dim retVal As String = ""
        For Each node As clsNode In If(nodes Is App.ActiveProject.ProjectManager.ActiveAlternatives, nodes.TerminalNodes, nodes.Nodes)
            Dim sChecked As Boolean = False
            If SelectedAltID IsNot Nothing AndAlso SelectedAltID.HasValue Then 
                Dim tAlt As clsNode = PM.ActiveAlternatives.GetNodeByID(SelectedAltID.Value)
                Dim tNodeContributedAlternatives = node.GetContributedAlternatives()
                sChecked = tAlt IsNot Nothing AndAlso tNodeContributedAlternatives IsNot Nothing AndAlso tNodeContributedAlternatives.Contains(tAlt)
            End If
            retVal += If(retVal <> "", ",", "") + DiagramNodeToJS(node, "", sChecked)
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    #Region "Measurement Scales"
    Private Function IsScaleVisible(scaleType As ScaleType) As Boolean
        Return scaleType = ScaleType.stShared OrElse (PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso scaleType = ScaleType.stLikelihood) OrElse (PM.ActiveHierarchy = ECHierarchyID.hidImpact AndAlso scaleType = ScaleType.stImpact)
    End Function

    Private Sub GetMeasureScalesDataByType(ByRef retVal As List(Of jMeasurementScale), sType As ECMeasureType, Optional sRatingScaleType As RatingScaleType = RatingScaleType.rsRegular)
        For Each Scale As clsMeasurementScale In PM.MeasureScales.AllScales
            Dim MT As Integer = CInt(ECMeasureType.mtNone)
            Dim scaleType As ScaleType = PM.MeasureScales.GetScaleType(Scale)
            If TypeOf (Scale) Is clsRatingScale Then MT = CInt(ECMeasureType.mtRatings)
            If TypeOf (Scale) Is clsStepFunction Then MT = CInt(ECMeasureType.mtStep)
            If TypeOf (Scale) Is clsRegularUtilityCurve Then MT = CInt(ECMeasureType.mtRegularUtilityCurve)
            Dim RST As Integer = PM.MeasureScales.GetRatingScaleType(Scale)
            If MT = sType AndAlso RST = sRatingScaleType AndAlso IsScaleVisible(scaleType) Then
                Dim sMT As String = JS_SafeString("None")
                Select Case MT
                    Case ECMeasureType.mtRatings
                        sMT = JS_SafeString("Ratings")
                        Select Case RST
                            Case RatingScaleType.rsExpectedValues
                                sMT = JS_SafeString("Expected Values")
                            Case RatingScaleType.rsOutcomes
                                sMT = JS_SafeString("Pairwise Of Probabilities")
                            Case RatingScaleType.rsPWOfPercentages
                                sMT = JS_SafeString("Pairwise Of Percentages")
                        End Select
                    Case ECMeasureType.mtStep
                        sMT = JS_SafeString("Step Function")
                    Case ECMeasureType.mtRegularUtilityCurve
                        sMT = JS_SafeString("Utility Curve")
                    Case ECMeasureType.mtAdvancedUtilityCurve
                        sMT = JS_SafeString("Advanced Utility Curve")
                End Select

                Dim tApplications As Integer = 0

                For Each node As clsNode In PM.Hierarchy(PM.ActiveHierarchy).Nodes
                    'If node.MeasureType = sType AndAlso (node.MeasurementScaleID = Scale.ID OrElse node.FeedbackMeasurementScaleID = Scale.ID) Then tApplications += 1
                    If (node.MeasureType = sType AndAlso node.MeasurementScaleID = Scale.ID) AndAlso (Not PM.IsRiskProject OrElse (Not node.RiskNodeType = RiskNodeType.ntCategory AndAlso Not node.AllChildrenCategories())) Then tApplications += 1
                Next

                If PM.IsRiskProject Then
                    Dim noSources As List(Of clsNode) = PM.Hierarchy(PM.ActiveHierarchy).GetUncontributedAlternatives
                    If noSources.Count > 0 Then
                        For Each node As clsNode In noSources
                            If node.MeasureType = sType AndAlso node.MeasurementScaleID = Scale.ID Then tApplications += 1
                        Next
                    End If
                End If

                retVal.Add(New jMeasurementScale With {.guid = Scale.GuidID, .mt = MT, .smt = sMT, .name = Scale.Name, .comment = If(False, Scale.Comment, ""), .rst = RST, .is_default = Scale.IsDefault, .apps = tApplications})
            End If
        Next
    End Sub

    Private Function IsScaleInUse(ScaleID As Integer) As String
        Dim sUses As String = ""
        For i As Integer = 0 To PM.Hierarchy(PM.ActiveHierarchy).Nodes.Count - 1
            If PM.Hierarchy(PM.ActiveHierarchy).Nodes(i).MeasurementScaleID = ScaleID Then
                If sUses.Length < 150 Then sUses += String.Format("{0}'{1}'", IIf(sUses = "", "", ", "), PM.Hierarchy(PM.ActiveHierarchy).Nodes(i).NodeName) Else Return sUses + ", ..."
            End If
        Next
        Return sUses
    End Function

    Private Sub SaveMeasurementScales()
        PM.StorageManager.Writer.SaveModelStructure()
        App.SaveProjectLogEvent(App.ActiveProject, String.Format("Save measurement scales"), False, "")
    End Sub

    Public Function GetMeasurementScales() As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone}
        Dim retVal As New List(Of jMeasurementScale)
        If App.isRiskEnabled Then
            ' Riskion Scales
            If PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then
                ' Riskion Likelihood scales
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings)
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtStep)
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtRegularUtilityCurve)
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings, RatingScaleType.rsOutcomes)
            Else
                ' Riskion Impact scales
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings)
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtRegularUtilityCurve)
                GetMeasureScalesDataByType(retVal, ECMeasureType.mtStep)
            End If
        Else
            ' Comparion Scales
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRatings)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtRegularUtilityCurve)
            GetMeasureScalesDataByType(retVal, ECMeasureType.mtStep)
        End If

        Res.Result = ecActionResult.arSuccess
        Res.Data = retVal

        Return Res
    End Function

    Public Function DeleteMeasurementScale(id As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}

        Dim tGuid As Guid = Guid.Empty
        If Guid.TryParse(id, tGuid) Then
            Dim ms As clsMeasurementScale = PM.MeasureScales.GetScaleByID(tGuid)
            Dim sUses As String = ""
            If ms IsNot Nothing Then sUses = IsScaleInUse(ms.ID)
            If Not String.IsNullOrEmpty(sUses) Then
                Res.Message = String.Format("[1,'{0}']", JS_SafeString(String.Format(ResString("msgScaleInUse"), sUses)))
            Else
                If ms IsNot Nothing Then
                    PM.MeasureScales.DeleteScaleByID(tGuid)
                    SaveMeasurementScales()
                    Res.Result = ecActionResult.arSuccess
                End If
            End If
        End If

        Return Res        
    End Function
    
    Public Function SetMeasurementScaleDefault(id As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}

        Dim tGuid As Guid = Guid.Empty
        If Guid.TryParse(id, tGuid) Then
            Dim ms As clsMeasurementScale = PM.MeasureScales.GetScaleByID(tGuid)
            If ms IsNot Nothing Then
                PM.MeasureScales.SetScaleDefault(tGuid, True)
                SaveMeasurementScales()
                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res        
    End Function

    Public Function MoveMeasurementScaleUp(id As String) As jActionResult
        Dim tGuid As Guid = Guid.Empty
        If Guid.TryParse(id, tGuid) Then
            PM.MeasureScales.MoveScale(tGuid, True)
            SaveMeasurementScales()
        End If

        Return GetMeasurementScales()        
    End Function

    Public Function MoveMeasurementScaleDown(id As String) As jActionResult
        Dim tGuid As Guid = Guid.Empty
        If Guid.TryParse(id, tGuid) Then
            PM.MeasureScales.MoveScale(tGuid, False)
            SaveMeasurementScales()
        End If

        Return GetMeasurementScales()        
    End Function

    #End Region

    #Region "Alexa Alternatives"

    Public Function GetAlternatives(Optional withProsCons As Boolean = False) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim tLst As New List(Of jNode)

        For Each tNode As clsNode In App.ActiveProject.ProjectManager.ActiveAlternatives.Nodes
            tLst.Add(jNode.CreateFromBaseObject(tNode, withProsCons))
        Next

        Res.Data = tLst
        Res.Result = ecActionResult.arSuccess

        Return Res
    End Function

    Public Function AddAlternative(name As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}

        Dim NewEvent As clsNode = App.ActiveProject.ProjectManager.ActiveAlternatives.AddNode(-1)
        With NewEvent
            .NodeName = Name
        End With
        AlexaWebAPI.CheckAlexaProjectMode(App)  ' D7584

        If App.ActiveProject.SaveStructure(ParseString("New %%Alternative%% Added"), False, True, String.Format("{0} added", Name)) Then
            Res.Result = ecActionResult.arSuccess
            Res.Data = jNode.CreateFromBaseObject(NewEvent)
        End If

        Return Res
    End Function

    Public Function GetAlternativeInfo(nodeid As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim tNode As clsNode = Nothing

        Dim tIntID As Integer
        If Integer.TryParse(nodeid, tIntID) Then
            tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
        Else
            Dim tGuidID As Guid
            If Guid.TryParse(nodeid, tGuidID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
            End If
        End If

        If tNode IsNot Nothing Then
            Res.Data = New jAlternativeInfo With {.name = tNode.NodeName, .id = tNode.NodeID, .guid = tNode.NodeGuidID, .pros = tNode.Pros(PM), .cons = tNode.Cons(PM)}
            Res.Result = ecActionResult.arSuccess
        End If

        Return Res
    End Function

    Public Function AddAlternativeInfo(nodeid As String, pros As String, cons As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim tNode As clsNode = Nothing

        Dim tIntID As Integer
        If Integer.TryParse(nodeid, tIntID) Then
            tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
        Else
            Dim tGuidID As Guid
            If Guid.TryParse(nodeid, tGuidID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
            End If
        End If

        If tNode IsNot Nothing Then
            If pros IsNot Nothing Then 
                Dim tPros As List(Of AlternativeProAndCon) = JsonConvert.DeserializeObject(Of List(Of AlternativeProAndCon))(pros)
                If tPros IsNot Nothing AndAlso tPros.Count > 0 Then 
                    Dim resPros As List(Of AlternativeProAndCon) = New List(Of AlternativeProAndCon)
                    resPros.AddRange(tNode.Pros(PM))
                    resPros.AddRange(tPros)
                    tNode.Pros(PM) = resPros 
                End If
            End If

            If cons IsNot Nothing Then
                Dim tCons As List(Of AlternativeProAndCon) = JsonConvert.DeserializeObject(Of List(Of AlternativeProAndCon))(cons)
                If tCons IsNot Nothing AndAlso tCons.Count > 0 Then
                    Dim resCons As List(Of AlternativeProAndCon) = New List(Of AlternativeProAndCon)
                    resCons.AddRange(tNode.Cons(PM))
                    resCons.AddRange(tCons)
                    tNode.Cons(PM) = resCons
                End If
            End If
            AlexaWebAPI.CheckAlexaProjectMode(App)  ' D7584

            Res.Result = ecActionResult.arSuccess
        End If
        Return Res
    End Function

    Private Sub SaveAntiguaDashboard()
        Dim CanvasMasterDBName = ExpertChoice.Service.WebConfigOption(ExpertChoice.Web.WebOptions._OPT_CANVASMASTERDB, "CoreDB", True)
        Dim mConnectionString = ExpertChoice.Data.clsDatabaseAdvanced.GetConnectionString(CanvasMasterDBName, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
        PM.AntiguaDashboard.SavePanel(ECModelStorageType.mstCanvasStreamDatabase, mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, App.ActiveProject.ID)
    End Sub

    Public Function DeleteAlternative(nodeid As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing Then
                H.DeleteNode(tNode)

                Dim altBoardNode = PM.AntiguaDashboard.GetNodeByGuid(tNode.NodeGuidID)
                If altBoardNode IsNot Nothing Then
                    PM.AntiguaDashboard.RemoveNodeByGuid(tNode.NodeGuidID)
                    SaveAntiguaDashboard()
                End If

                AlexaWebAPI.CheckAlexaProjectMode(App)  ' D7584
                If PRJ.SaveStructure(ParseString(ParseString("After deleting %%alternative%%")), True, True, tNode.NodeName) Then
                    Res.Result = ecActionResult.arSuccess
                End If

                PM.PipeBuilder.PipeCreated = False
            End If
        End If

        Return Res
    End Function

    Public Function RenameAlternative(nodeid As String, name As string) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing Then
                tNode.NodeName = name

                Dim altBoardNode = PM.AntiguaDashboard.GetNodeByGuid(tNode.NodeGuidID)
                If altBoardNode IsNot Nothing Then
                    altBoardNode.Text = name
                    SaveAntiguaDashboard()
                End If
                AlexaWebAPI.CheckAlexaProjectMode(App)  ' D7584

                If PRJ.SaveStructure(ParseString(ParseString("After renaming %%alternative%%")), True, True, tNode.NodeName) Then
                    Res.Result = ecActionResult.arSuccess
                End If
            End If
        End If

        Return Res
    End Function

    Public Function ResetProsAndCons(nodeid As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing Then
                tNode.Pros(PM) = New List(Of AlternativeProAndCon)
                tNode.Cons(PM) = New List(Of AlternativeProAndCon)
                AlexaWebAPI.CheckAlexaProjectMode(App)  ' D7584

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function

    Public Function RenameProByIndex(nodeid As String, index As String, newname As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim Idx As Integer
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing AndAlso Integer.TryParse(index, Idx) Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.Pros(PM) IsNot Nothing AndAlso Idx >= 0 AndAlso tNode.Pros(PM).Count < Idx  Then
                Dim Pros = tNode.Pros(PM)
                tNode.Pros(PM)(Idx).text = newname
                tNode.Pros(PM) = Pros ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function RenameProByName(nodeid As String, proname As String, newname As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.pros(PM) IsNot Nothing AndAlso tNode.pros(PM).Where(Function (p) p.text = proname).Count > 0 Then
                Dim pros = tNode.pros(PM)
                For Each pro In Pros.Where(Function (p) p.text = proname)
                    pro.text = newname
                Next
                tNode.Pros(PM) = pros ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function RenameConByIndex(nodeid As String, index As String, newname As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim Idx As Integer
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing AndAlso Integer.TryParse(index, Idx) Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.Cons(PM) IsNot Nothing AndAlso Idx >= 0 AndAlso tNode.Cons(PM).Count < Idx  Then
                Dim Cons = tNode.Cons(PM)
                tNode.Cons(PM)(Idx).text = newname
                tNode.Cons(PM) = Cons ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function RenameConByName(nodeid As String, conname As String, newname As String) As jActionResult
    Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.Cons(PM) IsNot Nothing AndAlso tNode.Cons(PM).Where(Function (p) p.text = conname).Count > 0 Then
                Dim Cons = tNode.Cons(PM)
                For Each con In Cons.Where(Function (p) p.text = conname)
                    con.text = newname
                Next
                tNode.Cons(PM) = Cons ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function DeleteProByIndex(nodeid As String, index As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim Idx As Integer
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing AndAlso Integer.TryParse(index, Idx) Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.Pros(PM) IsNot Nothing AndAlso Idx >= 0 AndAlso tNode.Pros(PM).Count < Idx  Then
                Dim Pros = tNode.Pros(PM)
                tNode.Pros(PM).RemoveAt(Idx)
                tNode.Pros(PM) = Pros ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function DeleteProByName(nodeid As String, proname As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.pros(PM) IsNot Nothing AndAlso tNode.pros(PM).Where(Function (p) p.text = proname).Count > 0 Then
                Dim pros = tNode.pros(PM)
                pros.RemoveAll(Function (p) p.text = proname)
                tNode.pros(PM) = pros ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function DeleteConByIndex(nodeid As String, index As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim Idx As Integer
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing AndAlso Integer.TryParse(index, Idx) Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.Cons(PM) IsNot Nothing AndAlso Idx >= 0 AndAlso tNode.Cons(PM).Count < Idx  Then
                Dim Cons = tNode.Cons(PM)
                tNode.Cons(PM).RemoveAt(Idx)
                tNode.Cons(PM) = Cons ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
    Public Function DeleteConByName(nodeid As String, conname As String) As jActionResult
        Dim Res As New jActionResult With {.Result = ecActionResult.arNone, .Message = ""}
        Dim H As ECCore.clsHierarchy = PM.ActiveAlternatives
        If H IsNot Nothing Then
            Dim tNode As clsNode = Nothing

            Dim tIntID As Integer
            If Integer.TryParse(nodeid, tIntID) Then
                tNode = PM.ActiveAlternatives.GetNodeByID(tIntID)
            Else
                Dim tGuidID As Guid
                If Guid.TryParse(nodeid, tGuidID) Then
                    tNode = PM.ActiveAlternatives.GetNodeByID(tGuidID)
                End If
            End If

            If tNode IsNot Nothing AndAlso tNode.Cons(PM) IsNot Nothing AndAlso tNode.Cons(PM).Where(Function (p) p.text = conname).Count > 0 Then
                Dim Cons = tNode.Cons(PM)
                Cons.RemoveAll(Function (p) p.text = conname)
                tNode.Cons(PM) = Cons ' Save

                Res.Result = ecActionResult.arSuccess
            End If
        End If

        Return Res
    End Function
#End Region

    Private ReadOnly Property AttributesList As List(Of clsAttribute)
        Get
            Return App.ActiveProject.ProjectManager.Attributes.AttributesList.Where(Function(a) a.Type = AttributeTypes.atAlternative AndAlso (Not a.IsDefault)).ToList
        End Get
    End Property

    ReadOnly Property RA As ResourceAligner
        Get
            Return If(App.ActiveProject.ProjectManager.IsRiskProject, App.ActiveProject.ProjectManager.ResourceAlignerRisk, App.ActiveProject.ProjectManager.ResourceAligner)
        End Get
    End Property

    Private Sub ProjectManagerWebAPI_Load(sender As Object, e As EventArgs) Handles Me.Load
        FetchIfNoActiveProject()

        RA.Load()

        Select Case _Page.Action

            Case "nodes_all"
                _Page.ResponseData = Nodes_All()    ' D5027

            Case "get_edges"
                _Page.ResponseData = get_edges()

            Case "set_edge"
                Dim sfrom_id As String = GetParam(_Page.Params, "from_id", True)
                Dim sto_id As String = GetParam(_Page.Params, "to_id", True)
                Dim smt As String = GetParam(_Page.Params, "mt", True)
                Dim from_id, to_id, mt As Integer
                Dim retVal As String = "{}"
                If Integer.TryParse(sfrom_id, from_id) AndAlso Integer.TryParse(sto_id, to_id) AndAlso Integer.TryParse(smt, mt) Then
                    retVal = set_edge(from_id, to_id, mt)
                End If
                _Page.ResponseData = retVal

            Case "check_edges"
                _Page.ResponseData = check_edges()

            Case "set_attribute_value"
                Dim fAttrChanged As Boolean = False
                Dim PM As clsProjectManager = App.ActiveProject.ProjectManager

                If _Page.Params.AllKeys.Contains("guid") AndAlso _Page.Params.AllKeys.Contains("dataField") Then
                    Dim tId As Guid = New Guid(GetParam(_Page.Params, "guid", True).Trim())
                    Dim sDataField As String = GetParam(_Page.Params, "dataField", True).Trim()
                    Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(tId)
                    If tAlt IsNot Nothing Then
                        Dim j As Integer = 0
                        For Each tAttr As clsAttribute In AttributesList
                            If Not tAttr.IsDefault AndAlso tAttr.Type = AttributeTypes.atAlternative Then
                                If sDataField = String.Format("v{0}", j) Then
                                    Dim sAttrVal As String = GetParam(_Page.Params, "value", True)
                                    PM.Attributes.SetAttributeValue(tAlt, tAttr, sAttrVal, fAttrChanged)
                                    Exit For
                                End If
                                j += 1
                            End If
                        Next

                        If fAttrChanged Then
                            ' write attribute values
                            Dim sMsg As String = ResString("msgEditAltAttrValues")
                            PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                        End If
                    End If
                End If

                _Page.ResponseData = String.Format("{{""success"":{0}}}", Bool2JS(fAttrChanged))
            Case "update_alternative"
                Dim fPrjChanged As Boolean = False
                Dim fRAChanged As Boolean = False
                Dim PM As clsProjectManager = App.ActiveProject.ProjectManager

                Dim tId As Guid = New Guid((GetParam(_Page.Params, "key", True)).Trim())
                Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(tId)
                If tAlt IsNot Nothing Then
                    If _Page.Params.AllKeys.Contains("name") Then
                        Dim sValue As String = GetParam(_Page.Params, "name", True)
                        If sValue <> tAlt.NodeName Then
                            tAlt.NodeName = sValue
                            fPrjChanged = True
                        End If
                    End If
                    If _Page.Params.AllKeys.Contains("event_type") Then
                        Dim sValue As String = GetParam(_Page.Params, "event_type", True)
                        Dim newType As EventType = CType(CInt(sValue), EventType)
                        If tAlt.EventType <> newType Then
                            tAlt.EventType = newType
                            fPrjChanged = True
                        End If
                    End If
                    If _Page.Params.AllKeys.Contains("cost") Then
                        Dim sValue As String = GetParam(_Page.Params, "cost", True)
                        Dim tNewValue As Double = UNDEFINED_INTEGER_VALUE
                        Dim tRAAlternative As RAAlternative = PM.ResourceAligner.Scenarios.Scenarios(0).GetAvailableAlternativeById(tAlt.NodeGuidID.ToString)
                        If String2Double(sValue, tNewValue) AndAlso tRAAlternative IsNot Nothing AndAlso tNewValue <> tRAAlternative.Cost Then
                            tRAAlternative.Cost = tNewValue
                            fRAChanged = True
                        End If
                    End If
                    If _Page.Params.AllKeys.Contains("risk") Then
                        Dim sValue As String = GetParam(_Page.Params, "risk", True)
                        Dim tNewValue As Double = UNDEFINED_INTEGER_VALUE
                        Dim tRAAlternative As RAAlternative = PM.ResourceAligner.Scenarios.Scenarios(0).GetAvailableAlternativeById(tAlt.NodeGuidID.ToString)
                        If String2Double(sValue, tNewValue) AndAlso tRAAlternative IsNot Nothing AndAlso tNewValue <> tRAAlternative.RiskOriginal Then
                            tRAAlternative.RiskOriginal = tNewValue
                            fRAChanged = True
                        End If
                    End If
                    If _Page.Params.AllKeys.Contains("dis") Then
                        Dim sValue As String = GetParam(_Page.Params, "dis", True)
                        tAlt.Enabled = Not Str2Bool(sValue)
                        fPrjChanged = True
                    End If
                End If

                If fRAChanged Then
                    App.ActiveProject.SaveRA(ResString("msgEditAltCost"), , , "Define structure")
                End If

                If fPrjChanged Then
                    ' save project users
                    PM.StorageManager.Writer.SaveModelStructure()
                End If

                'tResult = String.Format("['{0}','{1}',{2}]", sAction, JS_SafeString(""), GetAlternativesData())
                _Page.ResponseData = String.Format("{{""success"":{0}}}", Bool2JS(fRAChanged Or fPrjChanged))

            Case "get_simulation_groups"
                _Page.ResponseData = get_simulation_groups()

            Case "add_simulation_group"
                _Page.ResponseData = add_simulation_group(CInt(GetParam(_Page.Params, "tab", True)), GetParam(_Page.Params, "name", True), CInt(GetParam(_Page.Params, "behavior", True)), Str2Bool(GetParam(_Page.Params, "enabled", True)))

            Case "assign_simulation_group"
                _Page.ResponseData = assign_simulation_group(CInt(GetParam(_Page.Params, "tab", True)), CInt(GetParam(_Page.Params, "group_index", True)), CInt(GetParam(_Page.Params, "precedence", True)), New Guid(GetParam(_Page.Params, "node_guid", True)))

            Case "delete_simulation_group"
                _Page.ResponseData = delete_simulation_group(CInt(GetParam(_Page.Params, "tab", True)), New Guid(GetParam(_Page.Params, "guid", True)))

            Case "edit_simulation_group"
                _Page.ResponseData = edit_simulation_group(GetParam(_Page.Params, "guid", True), CInt(GetParam(_Page.Params, "tab", True)), GetParam(_Page.Params, "name", True), CInt(GetParam(_Page.Params, "behavior", True)), Str2Bool(GetParam(_Page.Params, "enabled", True)))
                ' Scenarios for MyRiskReward
            Case "get_scenarios"
                _Page.ResponseData = get_scenarios()
            Case "move_node"
                _Page.ResponseData = move_node(CInt(GetParam(_Page.Params, "fromID", True)), CInt(GetParam(_Page.Params, "toID", True)), CInt(GetParam(_Page.Params, "hid", True)), CType(GetParam(_Page.Params, "move_action", True), NodeMoveAction))
            Case "update_node_name"
                _Page.ResponseData = update_node_name(CInt(GetParam(_Page.Params, "id", True)), GetParam(_Page.Params, "name", True), CInt(GetParam(_Page.Params, "hid", True)))
            Case "update_node_description"
                _Page.ResponseData = update_node_description(CInt(GetParam(_Page.Params, "id", True)), GetParam(_Page.Params, "description", True), CInt(GetParam(_Page.Params, "hid", True)))
            Case "update_node_type"
                _Page.ResponseData = update_node_type(CInt(GetParam(_Page.Params, "id", True)), CInt(GetParam(_Page.Params, "type", True)), CInt(GetParam(_Page.Params, "hid", True)))
            Case "add_new_event_to_scenario"
                _Page.ResponseData = add_new_event_to_scenario(CInt(GetParam(_Page.Params, "id", True)), GetParam(_Page.Params, "name", True), CType(CInt(GetParam(_Page.Params, "type", True)), EventType))
            Case "add_existing_events_to_scenario"
                _Page.ResponseData = add_existing_events_to_scenario(CInt(GetParam(_Page.Params, "id", True)), Param2IntList(GetParam(_Page.Params, "event_ids", True)))
            Case "delete_event_from_scenario"
                _Page.ResponseData = delete_event_from_scenario(CInt(GetParam(_Page.Params, "id", True)), CInt(GetParam(_Page.Params, "event_id", True)))
            Case "add_scenario"
                _Page.ResponseData = add_scenario(GetParam(_Page.Params, "name", True))
            Case "delete_scenario"
                _Page.ResponseData = delete_scenario(CInt(GetParam(_Page.Params, "id", True)))
            Case "move_event_to_scenario"
                _Page.ResponseData = move_event_to_scenario(CInt(GetParam(_Page.Params, "fromID", True)), CInt(GetParam(_Page.Params, "toID", True)), CInt(GetParam(_Page.Params, "fromScenarioID", True)), CInt(GetParam(_Page.Params, "toScenarioID", True)), CType(GetParam(_Page.Params, "move_action", True), NodeMoveAction))
            Case "copy_event_to_scenario"
                _Page.ResponseData = copy_event_to_scenario(CInt(GetParam(_Page.Params, "fromID", True)), CInt(GetParam(_Page.Params, "toID", True)), CInt(GetParam(_Page.Params, "fromScenarioID", True)), CInt(GetParam(_Page.Params, "toScenarioID", True)), CType(GetParam(_Page.Params, "move_action", True), NodeMoveAction))
            Case "save_wizard_option"
                _Page.ResponseData = save_wizard_option(GetParam(_Page.Params, "id", True), GetParam(_Page.Params, "value", True))
            Case "add_new_objective_to_event"
                _Page.ResponseData = add_new_objective_to_event(CInt(GetParam(_Page.Params, "id", True)), GetParam(_Page.Params, "name", True), CInt(GetParam(_Page.Params, "hid", True)))
            Case "add_existing_objectives_to_event"
                _Page.ResponseData = add_existing_objectives_to_event(CInt(GetParam(_Page.Params, "id", True)), Param2IntList(GetParam(_Page.Params, "obj_ids", True)), CInt(GetParam(_Page.Params, "hid", True)))
            Case "delete_objective_from_event"
                _Page.ResponseData = delete_objective_from_event(CInt(GetParam(_Page.Params, "alt_id", True)), CInt(GetParam(_Page.Params, "obj_id", True)))
            Case "GetMeasurementScales".ToLower
                 _Page.ResponseData = GetMeasurementScales()
            Case "SetMeasurementScaleDefault".ToLower
                 _Page.ResponseData = SetMeasurementScaleDefault(GetParam(_Page.Params, "id", True))
            Case "DeleteMeasurementScale".ToLower
                 _Page.ResponseData = DeleteMeasurementScale(GetParam(_Page.Params, "id", True))
            Case "MoveMeasurementScaleUp".ToLower
                 _Page.ResponseData = MoveMeasurementScaleUp(GetParam(_Page.Params, "id", True))
            Case "MoveMeasurementScaleDown".ToLower
                 _Page.ResponseData = MoveMeasurementScaleDown(GetParam(_Page.Params, "id", True))
            Case "GetAlternatives".ToLower
                 _Page.ResponseData = GetAlternatives(If(HasParam(_Page.Params, "withProsCons", True), Str2Bool(GetParam(_Page.Params, "withProsCons", True)), False))
            Case "AddAlternative".ToLower
                 _Page.ResponseData = AddAlternative(GetParam(_Page.Params, "name", True))
            Case "GetAlternativeInfo".ToLower
                 _Page.ResponseData = GetAlternativeInfo(GetParam(_Page.Params, "nodeid", True))
            Case "AddAlternativeInfo".ToLower
                 _Page.ResponseData = AddAlternativeInfo(GetParam(_Page.Params, "nodeid", True), If(HasParam(_Page.Params, "pros", True), GetParam(_Page.Params, "pros", True), Nothing), If(HasParam(_Page.Params, "cons", True), GetParam(_Page.Params, "cons", True), Nothing))
            Case "DeleteAlternative".ToLower
                 _Page.ResponseData = DeleteAlternative(GetParam(_Page.Params, "nodeid", True))
            Case "RenameAlternative".ToLower
                 _Page.ResponseData = RenameAlternative(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "name", True))
            Case "ResetProsAndCons".ToLower
                 _Page.ResponseData = ResetProsAndCons(GetParam(_Page.Params, "nodeid", True))
            Case "RenameProByIndex".ToLower
                 _Page.ResponseData = RenameProByIndex(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proidx", True), GetParam(_Page.Params, "newname", True))
            Case "RenameProByName".ToLower
                 _Page.ResponseData = RenameProByName(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proname", True), GetParam(_Page.Params, "newname", True))
            Case "RenameConByIndex".ToLower
                 _Page.ResponseData = RenameConByIndex(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proidx", True), GetParam(_Page.Params, "newname", True))
            Case "RenameConByName".ToLower
                 _Page.ResponseData = RenameConByName(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proname", True), GetParam(_Page.Params, "newname", True))
            Case "DeleteProByIndex".ToLower
                 _Page.ResponseData = DeleteProByIndex(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proidx", True))
            Case "DeleteProByName".ToLower
                 _Page.ResponseData = DeleteProByName(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proname", True))
            Case "DeleteConByIndex".ToLower
                 _Page.ResponseData = DeleteConByIndex(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proidx", True))
            Case "DeleteConByName".ToLower
                 _Page.ResponseData = DeleteConByName(GetParam(_Page.Params, "nodeid", True), GetParam(_Page.Params, "proname", True))
            'Case "AddAlternativePro".ToLower
            '     _Page.ResponseData = AddAlternativePro(CInt(GetParam(_Page.Params, "nodeid", True)), GetParam(_Page.Params, "name", True))
            'Case "AddAlternativeCon".ToLower
            '     _Page.ResponseData = AddAlternativeCon(CInt(GetParam(_Page.Params, "nodeid", True)), GetParam(_Page.Params, "name", True))
        End Select
    End Sub

End Class

<Serializable> Public Class jScenarioData
    Inherits clsJsonObject

    Public Property id As Integer
    Public Property guid As Guid
    Public Property name As String = ""
    Public Property description As String = ""
    Public Property events As New List(Of jEventData)
End Class

<Serializable> Public Class jEventData
    Inherits clsJsonObject

    Public Property id As Integer
    Public Property guid As Guid
    Public Property name As String = ""
    Public Property description As String = ""
    Public Property type As Integer
    Public Property objectives As New List(Of jEventData)

    Public Shared Function FromNode(alt As clsNode, App As clsComparionCore) As jEventData
        Dim retVal As New jEventData With {.id = alt.NodeID, .guid = alt.NodeGuidID, .name = alt.NodeName, .description = Infodoc2Text(App.ActiveProject, alt.InfoDoc, False), .type = CInt(alt.EventType)}
        retVal.objectives = New List(Of jEventData)
        For Each obj As clsNode In App.ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact).TerminalNodes
            If obj.GetContributedAlternatives.Contains(alt) Then 
                retVal.objectives.Add(New jEventData With {.id = obj.NodeID, .guid = obj.NodeGuidID, .name = obj.NodeName, .description = Infodoc2Text(App.ActiveProject, obj.InfoDoc, False)})
            End If
        Next
        Return retVal
    End Function

End Class

<Serializable> Public Class jMeasurementScale
    Inherits clsJsonObject

    Public Property guid As Guid
    Public Property mt As Integer
    Public Property smt As String = ""
    Public Property name As String = ""
    Public Property comment As String = ""
    Public Property rst As Integer
    Public Property is_default As Boolean
    Public Property apps As Integer

End Class

<Serializable> Public Class jAlternativeInfo
    Inherits clsJsonObject

    Public Property guid As Guid
    Public Property id As Integer
    Public Property name As String = ""
    Public Property pros As List(Of AlternativeProAndCon)
    Public Property cons As List(Of AlternativeProAndCon)

End Class