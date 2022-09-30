Partial Class HierarchyPage
    Inherits clsComparionCorePage

    Public Const IsCompleteHierarchyAllowed As Boolean = False
    Public Const IsMultiCategoricalAttributesAllowed As Boolean = True
    Public PagePrefix As String = ""
    Public PagePrefixAlts As String = ""

    ' actions
    Public Const ACTION_ADD_LEVEL_BELOW As String = "add_level_below"
    Public Const ACTION_ADD_SAME_LEVEL As String = "add_node"
    Public Const ACTION_ADD_FROM_DATASET As String = "add_from_dataset" 'SL16152
    Public Const ACTION_ADD_FROM_DATASET_ALTS As String = "add_from_dataset_alts" 'SL16152
    Public Const ACTION_EDIT_NODE As String = "edit_node"
    Public Const ACTION_DELETE_NODE As String = "delete_node"
    Public Const ACTION_CONVERT_HIERARCHY As String = "convert_to_non_complete_hierarchy"
    Public Const ACTION_EXPORT_OBJECTIVES As String = "export_objs_to_clipboard"
    Public Const ACTION_EXPORT_ALTERNATIVES As String = "export_alts_to_clipboard"
    Public Const ACTION_SHOW_ALTERNATIVES As String = "show_alternatives"
    Public Const ACTION_EXPORT_MODEL As String = "export_model_to_clipboard"
    Public Const ACTION_EXPORT_MODEL_FILE As String = "export_model_to_file"
    Public Const ACTION_ADD_ALTERNATIVES As String = "add_alternatives"
    Public Const ACTION_MOVE_NODE As String = "move_node"
    Public Const ACTION_ENABLE_NODE As String = "enable_node"
    Public Const ACTION_SWITCH_VIEW As String = "switch_view"
    Public Const ACTION_MOVE_TREE_VIEW_NODE As String = "move_tree_view_node"
    Public Const ACTION_DELETE_JUDGMENTS_FOR_NODE As String = "delete_judgments_for_node"
    Public Const ACTION_COPY_JUDGMENTS_TO_LOCATION As String = "copy_judgments_to_location"
    Public Const ACTION_PASTE_JUDGMENTS_FROM_LOCATION As String = "paste_judgments_from_location"
    Public Const ACTION_DISPLAY_PRIORITIES As String = "display_priorities"
    Public Const ACTION_SORT_CLUSTERS As String = "sort_clusters"
    Public Const ACTION_EDIT_ATTRIBUTES As String = "edit_attributes"


    Public Enum HierarchyViews
        hvGraph = 0
        hvTree = 1
        'hvAlts = 2
    End Enum

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property


    ReadOnly Property PM As clsProjectManager
        Get
            Return App.ActiveProject.ProjectManager
        End Get
    End Property

    ' -D6850
    ' moved to PM.Parameters.DecimalDigits
    'Public Property DecimalDigits As Integer
    '    Get
    '        Dim retVal As Integer = 2
    '        retVal = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
    '        If retVal < 0 Then retVal = 0
    '        If retVal > 5 Then retVal = 5
    '        Return retVal
    '    End Get
    '    Set(value As Integer)
    '        WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_DECIMALS_ID, AttributeValueTypes.avtLong, value)
    '    End Set
    'End Property

    Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
        With PM
            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

    Public Property ShowAlternatives As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_HIERARCHY_SHOW_ALTERNATIVES_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            SaveSetting(ATTRIBUTE_HIERARCHY_SHOW_ALTERNATIVES_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property ActiveView As Integer 'HierarchyViews.hvGraph, HierarchyViews.hvTree, HierarchyViews.hvAlts
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_HIERARCHY_ACTIVE_VIEW_ID, UNDEFINED_USER_ID))
            If retVal = HierarchyViews.hvTree AndAlso PM.ActiveObjectives.HasCompleteHierarchy Then retVal = CInt(HierarchyViews.hvGraph)
            Return retVal
        End Get
        Set(value As Integer)
            SaveSetting(ATTRIBUTE_HIERARCHY_ACTIVE_VIEW_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    ReadOnly Property SESSION_COPY_JUDGMENTS_SOURCE_NODE_ID As String
        Get
            Return String.Format("Hierarchy_Source_NodeId_{0}", App.ProjectID)
        End Get
    End Property

    Public Property JudgmentsSourceNodeID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s = SessVar(SESSION_COPY_JUDGMENTS_SOURCE_NODE_ID)
            If Not String.IsNullOrEmpty(s) AndAlso s.Length > 32 Then
                Dim tGuid As Guid = New Guid(s)
                If PM.ActiveObjectives.GetNodeByID(tGuid) IsNot Nothing OrElse PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(tGuid) IsNot Nothing Then retVal = tGuid
            End If
            Return retVal
        End Get
        Set(value As Guid)
            SessVar(SESSION_COPY_JUDGMENTS_SOURCE_NODE_ID) = value.ToString
        End Set
    End Property

    ReadOnly Property SESSION_COPY_JUDGMENTS_SOURCE_USER_ID As String
        Get
            Return String.Format("Hierarchy_Source_UserId_{0}", App.ProjectID)
        End Get
    End Property

    Public Property JudgmentsSourceUserID As Integer
        Get
            Dim retVal As Integer = 0
            Dim s = SessVar(SESSION_COPY_JUDGMENTS_SOURCE_USER_ID)
            If Not String.IsNullOrEmpty(s) Then Int32.TryParse(s, retVal)
            Return retVal
        End Get
        Set(value As Integer)
            SessVar(SESSION_COPY_JUDGMENTS_SOURCE_USER_ID) = value.ToString
        End Set
    End Property

    'ReadOnly Property SESSION_GOAL_CHECKED As String
    '    Get
    '        Return String.Format("Hierarchy_GoalNodeChecked_{0}", App.ProjectID)
    '    End Get
    'End Property

    'Public Property GoalNodeChecked As Boolean
    '    Get
    '        Dim s = SessVar(SESSION_GOAL_CHECKED)
    '        If Not String.IsNullOrEmpty(s) Then Return Str2Bool(s)
    '        Return False
    '    End Get
    '    Set(value As Boolean)
    '        SessVar(SESSION_GOAL_CHECKED) = value.ToString
    '    End Set
    'End Property

    ' D5054 ===
    Public Function GetHID() As ecNodeSetHierarchy
        Select Case CurrentPageID
            Case _PGID_STRUCTURE_HIERARCHY
                Return If(App.isRiskEnabled, If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, ecNodeSetHierarchy.hidImpact, ecNodeSetHierarchy.hidLikelihood), ecNodeSetHierarchy.hidObjectives)
            Case _PGID_STRUCTURE_ALTERNATIVES
                Return If(App.isRiskEnabled, ecNodeSetHierarchy.hidEvents, ecNodeSetHierarchy.hidAlternatives)
        End Select
        Return ecNodeSetHierarchy.hidObjectives
    End Function
    ' D5054 ==

    Public Sub New()
        MyBase.New(_PGID_STRUCTURE_HIERARCHY)
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As System.EventArgs) Handles Me.InitComplete
        ' D5056 ===
        Dim sAction = CheckVar(_PARAM_ACTION, "").Trim.ToLower
        Select Case sAction
            Case "download_nodeset"
                Dim ID As String = CheckVar("id", "").Trim
                Dim sError As String = ""
                If ID <> "" Then
                    Dim tSet As clsNodeSet = clsNodeSet.GetByName(App.NodeSets_GetList(GetHID, False, sError), ID)
                    If tSet IsNot Nothing Then
                        DownloadContent(tSet.Content, "text/plain", String.Format("{0}.txt", ID), dbObjectType.einfFile, App.ProjectID)   ' D6593 + D6852
                        'RawResponseStart()
                        'Response.ContentType = "text/plain"
                        'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.txt""",  HttpUtility.UrlEncode(SafeFileName(ID))))	' D6591
                        'Response.AppendHeader("Content-Length", CStr(tSet.Content.Length * Encoding.Default.GetBytes("a").Length))
                        'Response.Write(tSet.Content)
                        'RawResponseEnd()
                    End If
                End If
                ' D5056 ==
        End Select

        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Page_PreLoad(sender As Object, e As System.EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not isCallback Then
            'pnlLoadingPanel.Caption = ResString("msgLoading")
            'pnlLoadingPanel.Message = String.Format("<div style='margin:1em'><img src={1} alt='{0}' border=0/></div>&nbsp;&nbsp;{0}", ResString("lblPleaseWait"), ImagePath + "process.gif")
            PagePrefix = "Hierarchy: "
            PagePrefixAlts = ResString("lblAlternatives") + ": "
        End If
    End Sub


    Function GetNodesData() As String
        If PM.Parameters.Structure_DisplayPrioritiesMode <> 0 Then
            Dim CG As ECCore.clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup
            Dim CalcTarget As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
            PM.CalculationsManager.Calculate(CalcTarget, PM.ActiveObjectives.Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)
        End If

        Dim retVal As String = ""
        Dim dLocal As Double = 0
        Dim dGlobal As Double = 0
        ' get objectives
        Dim H As clsHierarchy = PM.ActiveObjectives
        H.SortNodesByLevels()
        PM.CreateHierarchyLevelValuesCH(H)
        For Each t As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder(True)
            Dim parentGuids As String = ""
            Dim node As clsNode = t.Item3
            Dim parent As clsNode = H.GetNodeByID(t.Item2)
            If parent IsNot Nothing Then parentGuids = String.Format("'{0}'", parent.NodeGuidID.ToString)
            If node.ParentNodesGuids IsNot Nothing Then
                For Each id As Guid In node.ParentNodesGuids
                    Dim sId As String = id.ToString
                    If parentGuids.IndexOf(sId) < 0 Then
                        parentGuids += CStr(IIf(parentGuids = "", "", ",")) + String.Format("'{0}'", sId)
                    End If
                Next
            End If
            parentGuids = String.Format("[{0}]", parentGuids)
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("[{0},'{1}','{2}',{3},{4},null,{5},0,{6},{7},1,{8},{9},{10},'{11}']", node.NodeID, node.NodeGuidID.ToString, JS_SafeString(node.NodeName), parentGuids, node.Level, Bool2JS(node.InfoDoc.Length > 0), If(node.IsTerminalNode, 1, 0), If(node.RiskNodeType = RiskNodeType.ntCategory, 1, 0), If(node.Enabled, 1, 0), dLocal, dGlobal, GetPrioritiesString(node))
        Next
        Dim maxLevel As Integer = H.Nodes.Max(Function(n) n.Level)
        ' get alternatives
        'If ShowAlternatives Then
        Dim AH As clsHierarchy = PM.ActiveAlternatives
        For Each alt As ECCore.clsNode In AH.Nodes
            Dim parentGuids As String = ""
            For Each covObj As ECCore.clsNode In H.TerminalNodes
                If covObj.GetContributedAlternatives.Contains(alt) Then
                    parentGuids += CStr(IIf(parentGuids = "", "", ",")) + String.Format("'{0}'", covObj.NodeGuidID.ToString)
                End If
            Next
            parentGuids = String.Format("[{0}]", parentGuids)
            Dim sAttributes As String = GetAltAttributesString(alt)
            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("[{0},'{1}','{2}',{3},{4},null,{5},1,1,0,0,{6},{7},{8},'{9}'{10}]", alt.NodeID, alt.NodeGuidID.ToString, JS_SafeString(alt.NodeName), parentGuids, maxLevel + 1, Bool2JS(alt.InfoDoc.Length > 0), CInt(IIf(alt.Enabled, 1, 0)), dLocal, dGlobal, GetPrioritiesString(alt), sAttributes)
        Next
        'End If
        Return String.Format("[{0}]", retVal)
    End Function

    Private Function GetAltAttributesString(ctrl As clsNode) As String
        Dim sAttributes As String = ""
        For Each attr As clsAttribute In AttributesList
            Dim clip_data As String = ""    'clipboard data
            Dim attrValue As Object = PM.Attributes.GetAttributeValue(attr.ID, ctrl.NodeGuidID)
            Dim sValue As String = ""

            Select Case attr.ValueType
                Case AttributeValueTypes.avtString '0
                    If attrValue Is Nothing OrElse Not TypeOf attrValue Is String Then attrValue = ""
                    clip_data = CStr(attrValue)
                    sValue = String.Format("'{0}'", JS_SafeString(CStr(attrValue)))
                Case AttributeValueTypes.avtBoolean '1
                    If attrValue Is Nothing OrElse Not TypeOf attrValue Is Boolean Then attrValue = False
                    clip_data = If(CBool(attrValue), ResString("lblYes"), ResString("lblNo"))
                    sValue = If(CBool(attrValue), "1", "0")
                Case AttributeValueTypes.avtLong '2
                    If attrValue Is Nothing OrElse Not (TypeOf attrValue Is Long OrElse TypeOf attrValue Is Integer) Then attrValue = ""
                    clip_data = CStr(attrValue).Replace("&#160;", " ")
                    sValue = ""
                    If CStr(attrValue) <> "" Then sValue = JS_SafeNumber(CStr(attrValue))
                Case AttributeValueTypes.avtDouble '3
                    If attrValue Is Nothing OrElse Not (TypeOf attrValue Is Double OrElse TypeOf attrValue Is Integer OrElse TypeOf attrValue Is Long OrElse TypeOf attrValue Is Single) Then attrValue = ""
                    clip_data = CStr(attrValue).Replace("&#160;", " ")
                    sValue = ""
                    If CStr(attrValue) <> "" Then sValue = JS_SafeNumber(CStr(attrValue))
                Case AttributeValueTypes.avtEnumeration '4
                    Dim attrValueEnumID As Guid = Guid.Empty

                    If attrValue IsNot Nothing AndAlso TypeOf attrValue Is Guid Then
                        attrValueEnumID = CType(attrValue, Guid)
                    End If

                    If attrValue IsNot Nothing AndAlso TypeOf attrValue Is String AndAlso CStr(attrValue) <> "" AndAlso CStr(attrValue).Trim.Length = 32 + 4 Then
                        attrValueEnumID = New Guid(CStr(attrValue))
                    End If

                    sValue = String.Format("'{0}'", attrValueEnumID.ToString)
                Case AttributeValueTypes.avtEnumerationMulti '5
                    Dim tVals As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If tVals IsNot Nothing AndAlso attrValue IsNot Nothing Then
                        Dim lst As String() = CStr(attrValue).Split(CChar(";"))
                        For k As Integer = 0 To lst.Length - 1
                            Dim sGuid As String = CStr(lst(k)).Trim
                            If Not String.IsNullOrEmpty(sGuid) AndAlso sGuid.Length = 36 Then
                                Dim itemID As Guid = New Guid(sGuid)
                                Dim tVal As clsAttributeEnumerationItem = tVals.GetItemByID(itemID)
                                If tVal IsNot Nothing Then sValue += If(sValue = "", "", ", ") + String.Format("'{0}'", sGuid)
                            End If
                        Next
                    End If
                    sValue = String.Format("[{0}]", sValue)
            End Select
            sAttributes += If(sAttributes = "", "", ", ") + sValue
        Next

        Dim fChk As Integer = If(ctrl.Enabled, 1, 0)
        If sAttributes <> "" Then sAttributes = "," + sAttributes

        Return sAttributes
    End Function

    Private _UsersList As List(Of clsApplicationUser) = Nothing
    Public ReadOnly Property UsersList As List(Of clsApplicationUser)
        Get
            If _UsersList Is Nothing Then _UsersList = App.DBUsersByProjectID(App.ProjectID)
            Return _UsersList
        End Get
    End Property

    Private Function GetIntegerList(sLst As String) As List(Of Integer)
        Dim iIDs As New List(Of Integer)
        If sLst = "all" Then
            For Each user As clsApplicationUser In UsersList
                iIDs.Add(user.UserID)
            Next
        Else
            Dim sIDs As String() = sLst.Split(CChar(","))
            For Each id As String In sIDs
                Dim i As Integer
                If Integer.TryParse(id, i) Then iIDs.Add(i)
            Next
        End If
        Return iIDs
    End Function

    Private Function GetGuidList(sLst As String) As List(Of Guid)
        Dim iIDs As New List(Of Guid)
        If sLst = "all" Then
            For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                iIDs.Add(alt.NodeGuidID)
            Next
        Else
            Dim sIDs As String() = sLst.Split(CChar(","))
            For Each id As String In sIDs
                Dim i As Guid
                If Guid.TryParse(id, i) Then iIDs.Add(i)
            Next
        End If
        Return iIDs
    End Function

    Public Function GetUsersData() As String
        Dim sRes As String = ""

        Dim tPrjUsers As List(Of ECTypes.clsUser) = PM.UsersList
        'Dim UsersData As List(Of Integer) = MiscFuncs.DataExistsForUsers_CanvasStreamDatabase(PRJ.ConnectionString, App.ProjectID, PRJ.ProviderType)
        'Dim UsersData As HashSet(Of Integer) = PM.StorageManager.Reader.DataExistsForUsersHashset(PM.ActiveHierarchy) 'PM.DataExistsForUsers()

        For Each tUser As clsApplicationUser In UsersList
            Dim sName As String = tUser.UserName
            Dim AHPUserID As Integer = -1
            Dim tAHPUser As clsUser = clsApplicationUser.AHPUserByUserEmail(tUser.UserEmail, tPrjUsers)
            If tAHPUser IsNot Nothing Then
                AHPUserID = tAHPUser.UserID
                sName = tAHPUser.UserName
            End If

            'Dim fHasData As Boolean = UsersData.Contains(AHPUserID)
            'sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}','{2}',{3},{4}]", tUser.UserID, JS_SafeString(tUser.UserEmail), JS_SafeString(sName), AHPUserID, Bool2Num(fHasData))
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("[{0},'{1}','{2}',{3}]", tUser.UserID, JS_SafeString(tUser.UserEmail), JS_SafeString(sName), AHPUserID)
        Next

        Return sRes
    End Function

    Private Function GetCallbackData(sAction As String, Optional MissingContributionsType As AlternativesMissingContributionsType = AlternativesMissingContributionsType.amcNone) As String
        Return String.Format("['{0}',{1},{2},{3},{4}]", sAction, GetNodesData(), GetTreeNodesData(), GetAlternativesData(), CInt(MissingContributionsType))
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, _PARAM_ACTION).ToLower
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction
            Case "refresh_full"
                tResult = GetCallbackData(sAction)
            Case ACTION_ADD_FROM_DATASET 'SL16152
                tResult = GetCallbackData(sAction)
            Case ACTION_ADD_SAME_LEVEL, ACTION_ADD_LEVEL_BELOW, ACTION_ADD_ALTERNATIVES, "add_alternatives_below"
                Dim s_parent_ids As String = GetParam(args, "parent_ids").Trim()
                Dim node_names As String = GetParam(args, "name")
                'Dim node_descr As String = GetParam(args, "descr").Trim()
                Dim parent_guids As New List(Of Guid)
                Dim amcType As AlternativesMissingContributionsType = AlternativesMissingContributionsType.amcNone
                Dim s_ids As String() = s_parent_ids.Split(CChar(";"))
                If s_ids.Count > 0 OrElse sAction = ACTION_ADD_ALTERNATIVES Then
                    For Each id As String In s_ids
                        Dim tGUID As Guid
                        If Not String.IsNullOrEmpty(id) AndAlso Guid.TryParse(id, tGUID) Then parent_guids.Add(tGUID)
                    Next
                    If sAction = ACTION_ADD_ALTERNATIVES Or sAction = "add_alternatives_below" Then
                        If Not ShowAlternatives Then ShowAlternatives = True
                        Dim insertBelowNodeID As Guid = Guid.Empty
                        If sAction = "add_alternatives_below" Then
                            If Not Guid.TryParse(GetParam(args, "upper_node_id").Trim(), insertBelowNodeID) Then insertBelowNodeID = Guid.Empty
                        End If
                        AddAlternatives(parent_guids, node_names, insertBelowNodeID)

                        amcType = GetAnyAlternativeMissingContribution()
                    Else
                        'Dim s_is_cat As String = GetParam(args, "is_cat").Trim()
                        'Dim is_cat As Boolean = Str2Bool(s_is_cat)
                        'AddNode(PM.ActiveHierarchy, parent_guids, node_name, node_descr)
                        Dim insertBelowNodeID As Guid = Guid.Empty
                        If sAction = ACTION_ADD_SAME_LEVEL Then
                            If Not Guid.TryParse(GetParam(args, "upper_node_id").Trim(), insertBelowNodeID) Then insertBelowNodeID = Guid.Empty
                        End If
                        AddNodes(CType(PM.ActiveHierarchy, ECHierarchyID), parent_guids, node_names, insertBelowNodeID)
                    End If
                End If

                PM.PipeBuilder.PipeCreated = False

                tResult = GetCallbackData(sAction, amcType)
            Case ACTION_EDIT_NODE
                Dim s_parent_ids As String = GetParam(args, "parent_ids").Trim()
                Dim s_is_alt As String = GetParam(args, "is_alt").Trim()
                Dim is_alt As Boolean = Str2Bool(s_is_alt)
                Dim s_is_cat As String = GetParam(args, "is_cat").Trim()
                Dim is_cat As Boolean = Str2Bool(s_is_cat)
                Dim node_id As Guid = New Guid(GetParam(args, "id").Trim())
                Dim node_name As String = GetParam(args, "name").Trim().Replace(Chr(10), " ").Replace(Chr(13), "").Replace(Chr(9), " ")
                'Dim node_descr As String = GetParam(args, "descr").Trim()
                Dim parent_guids As New List(Of Guid)
                Dim s_ids As String() = s_parent_ids.Split(CChar(";"))
                If s_ids.Count > 0 Then
                    For Each id As String In s_ids
                        If Not String.IsNullOrEmpty(id) Then parent_guids.Add(New Guid(id))
                    Next
                End If
                'EditNode(PM.ActiveHierarchy, node_id, parent_guids, node_name, node_descr)
                If is_alt Then
                    EditNode(PM.AltsHierarchy(PM.ActiveAltsHierarchy), node_id, Nothing, node_name, Nothing)
                    PM.UpdateContributions(node_id, parent_guids, CType(PM.ActiveHierarchy, ECHierarchyID))
                Else
                    EditNode(PM.ActiveObjectives, node_id, parent_guids, node_name, is_cat)
                End If

                PM.PipeBuilder.PipeCreated = False

                tResult = GetCallbackData(sAction)
            Case "set_node_type"
                Dim sID As String = GetParam(args, "id").Trim()
                Dim ID As Guid = New Guid(sID)
                Dim isCat As Boolean = Param2Bool(args, "is_cat")
                Dim node As clsNode = PM.ActiveObjectives.GetNodeByID(ID)
                If node IsNot Nothing Then
                    node.RiskNodeType = If(isCat, RiskNodeType.ntCategory, RiskNodeType.ntUncertainty)
                    PRJ.SaveStructure(PagePrefix + "Changed node type", True, True, node.NodeName)
                End If

                tResult = GetCallbackData(sAction)
            Case ACTION_DELETE_NODE
                If args.AllKeys.Contains("id") Then ' single node
                    Dim sID As String = GetParam(args, "id").Trim()
                    Dim ID As Guid = New Guid(sID)
                    Dim sIsAlt As String = GetParam(args, "is_alt")
                    Dim isAlt As Boolean = Str2Bool(sIsAlt)
                    PRJ.MakeSnapshot("Hierarchy", "Before deleting node")
                    DeleteNode(ID, isAlt)
                End If
                If args.AllKeys.Contains("ids") Then ' multiselect
                    Dim sIDs As String = GetParam(args, "ids").Trim()
                    Dim IDs As List(Of Guid) = Param2GuidList(sIDs)
                    PRJ.MakeSnapshot("Hierarchy", "Before deleting nodes")
                    DeleteNodes(IDs)
                End If

                PM.PipeBuilder.PipeCreated = False

                tResult = GetCallbackData(sAction)
            Case "delete_alts"
                Dim sIDs As String = GetParam(args, "ids").Trim()
                Dim IDs As List(Of Guid) = Param2GuidList(sIDs)
                PRJ.MakeSnapshot(ParseString("%%Alternatives%%"), ParseString("Before deleting %%alternatives%%"))
                DeleteNodes(IDs)

                PM.PipeBuilder.PipeCreated = False

                tResult = GetCallbackData(sAction)
            Case "duplicate_alt"
                Dim sIDs As String = GetParam(args, "ids").Trim()
                Dim IDs As List(Of Guid) = Param2GuidList(sIDs)
                Dim tDuplicateJudgments As Boolean = Str2Bool(GetParam(args, "judgments"))
                PRJ.MakeSnapshot(ParseString("%%Alternatives%%"), ParseString("Before duplicating %%alternative%%(s)"))
                For Each AltGUID As Guid In IDs
                    DuplicateNode(ECHierarchyType.htAlternative, AltGUID, "{0}", tDuplicateJudgments)
                Next

                PM.PipeBuilder.PipeCreated = False

                tResult = GetCallbackData(sAction)
            Case ACTION_ENABLE_NODE
                Dim sID As String = GetParam(args, "id").Trim()
                Dim ID As Guid = New Guid(sID)
                Dim isAlt As Boolean = Param2Bool(args, "is_alt")
                Dim isEnable As Boolean = Param2Bool(args, "chk")
                'PRJ.MakeSnapshot("Hierarchy", "Before enabling/disabling node")
                Dim node As clsNode = PM.ActiveObjectives.GetNodeByID(ID)
                If node Is Nothing Then
                    node = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(ID)
                End If
                If node IsNot Nothing Then
                    node.Enabled = isEnable
                    PRJ.SaveStructure(PagePrefix + "After enabling/disabling node", True, True, node.NodeName)
                End If

                tResult = GetCallbackData(sAction)
            Case ACTION_DELETE_JUDGMENTS_FOR_NODE
                Dim sID As String = GetParam(args, "id").Trim()
                Dim ID As Guid = New Guid(sID)
                Dim isAlt As Boolean = Param2Bool(args, "is_alt")
                Dim isPlex As Boolean = Param2Bool(args, "is_plex")
                PRJ.MakeSnapshot("Hierarchy", "Before deleting judgments")
                PM.DeleteJudgmentsForNode(ID, , isPlex)
                tResult = String.Format("['{0}','{1}']", sAction, "Done")
            Case ACTION_CONVERT_HIERARCHY
                Dim prjName As String = GetParam(args, "prj_name")
                PM.Hierarchy(PM.ActiveHierarchy).ConvertToIncompleteHierachy(Str2Bool(GetParam(args, "judgments")))   ' D3492
                Dim fCopyJudgments As Boolean = Str2Bool(GetParam(args, "judgments"))
                'PRJ.SaveStructure(PagePrefix + "Convert to incomplete hierarchy", True, True)
                PRJ.SaveStructure(PagePrefix + "Before converting to an incomplete hierarchy", True, True)
                Dim sError As String = ""
                Dim newPrjID As Integer = CopyProjectAndConvertFromCH(prjName, PRJ.ID, CType(PM.ActiveHierarchy, ECHierarchyID), fCopyJudgments, sError)
                'PM.ActiveObjectives.ConvertToIncompleteHierachy(Str2Bool(GetParam(args, "judgments")))   ' D3492
                'PM.StorageManager.Writer.SaveProject(False)
                'tResult = GetCallbackData(sAction)
                If newPrjID >= 0 AndAlso sError = "" Then 
                    App.ProjectID = newPrjID ' set new project id and reload the page
                End If

                PM.PipeBuilder.PipeCreated = False

                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(sError))
            Case "multiselect_switch"
                Dim sValue As String = GetParam(args, "value").Trim()
                PM.Parameters.Hierarchy_MultiselectEnabled = Str2Bool(sValue)
                PM.Parameters.Save()
                tResult = String.Format("['{0}','{1}']", sAction, "Done")            
            Case ACTION_SHOW_ALTERNATIVES
                Dim sValue As String = GetParam(args, "value").Trim()
                ShowAlternatives = Str2Bool(sValue)
                tResult = GetCallbackData(sAction)
            Case ACTION_MOVE_NODE
                Dim target_id As Guid = New Guid(GetParam(args, "target"))
                Dim source_id As Guid = New Guid(GetParam(args, "source"))
                Dim node_action As String = GetParam(args, "node_action").ToLower
                Dim copy_judgments As Boolean = Str2Bool(GetParam(args, "copy_judgments"))
                If Not target_id.Equals(Guid.Empty) AndAlso Not source_id.Equals(Guid.Empty) Then
                    If node_action = "move" Then
                        MoveNode(source_id, target_id)
                    Else
                        CopyNode(CType(PM.ActiveHierarchy, ECHierarchyID), source_id, target_id, NodeMoveAction.nmaAsChildOfNode, "{0}", copy_judgments)
                    End If
                End If
                PM.PipeBuilder.PipeCreated = False

                tResult = GetCallbackData(sAction)
            Case ACTION_MOVE_TREE_VIEW_NODE
                Dim goal_id As Guid = PM.ActiveObjectives.Nodes(0).NodeGuidID
                Dim target_id As Guid = Nothing
                Dim target_str As String = GetParam(args, "target").Trim()
                If Not String.IsNullOrEmpty(target_str) Then target_id = New Guid(target_str) Else target_id = goal_id
                Dim source_id As Guid = New Guid(GetParam(args, "source").Trim())
                Dim move_type As NodeMoveAction = CType(CInt(GetParam(args, "move_type")), NodeMoveAction)
                Dim node_action As String = GetParam(args, "node_action").ToLower
                Dim copy_judgments As Boolean = Str2Bool(GetParam(args, "copy_judgments"))
                If Not ((move_type = NodeMoveAction.nmaAfterNode OrElse move_type = NodeMoveAction.nmaBeforeNode) AndAlso target_id.Equals(goal_id)) AndAlso target_id <> source_id Then
                    If node_action = "move" Then
                        MoveNode(source_id, target_id, move_type)
                    Else
                        CopyNode(CType(PM.ActiveHierarchy, ECHierarchyID), source_id, target_id, move_type, "{0}", copy_judgments)
                    End If
                End If
                PM.PipeBuilder.PipeCreated = False
                tResult = GetCallbackData(sAction)
            Case ACTION_SWITCH_VIEW
                Dim tInt As Integer
                If Integer.TryParse(GetParam(args, "value"), tInt) Then ActiveView = tInt
                tResult = String.Format("['{0}']", sAction)
            Case ACTION_COPY_JUDGMENTS_TO_LOCATION
                Dim isAlt As Boolean = Param2Bool(args, "is_alt")
                JudgmentsSourceNodeID = New Guid((GetParam(args, "id")).Trim())
                JudgmentsSourceUserID = CInt((GetParam(args, "from")))    'Anti-XSS
                tResult = String.Format("['{0}']", sAction)
            Case ACTION_PASTE_JUDGMENTS_FROM_LOCATION
                Dim sMsg As String = ResString("errNoDetails")
                Dim isAlt As Boolean = Param2Bool(args, "is_alt")
                Dim toIDs = GetIntegerList((GetParam(args, "to")))    'Anti-XSS
                Dim toAltsIDs = GetGuidList((GetParam(args, "to_alts")))    'Anti-XSS
                'Dim JudgmentsTargetNodeID As Guid = New Guid((GetParam(args, "id")).Trim())
                Dim CopyMode As ECCore.ECTypes.CopyJudgmentsMode = CType(CInt((GetParam(args, "mode").Trim())), ECCore.ECTypes.CopyJudgmentsMode) 'Anti-XSS
                Dim pwOnly As Boolean = Str2Bool((GetParam(args, "pw_only").Trim()))   'Anti-XSS

                'If Not JudgmentsTargetNodeID.Equals(Guid.Empty) AndAlso toIDs.Count > 0 Then
                If toAltsIDs.Count > 0 AndAlso toIDs.Count > 0 Then
                    'A1505 === 
                    Dim destUsersList As New List(Of clsUser) 'AS/16216a===
                    Dim srcUserGuid As Guid = PM.GetUserByID(JudgmentsSourceUserID).UserGuidID
                    For Each item As Integer In toIDs
                        destUsersList.Add(PM.GetUserByID(item))
                    Next
                    If Not isAlt Then 'objective 'AS/11642d enclosed and added Else part
                        'If PM.CopyJudgmentsFromNodeToNodes(JudgmentsSourceNodeID, JudgmentsTargetNodeID, sMsg, srcUserGuid, destUsersList, CopyMode, pwOnly) Then 'AS/12323zh? 'AS/16059c
                        If PM.CopyJudgmentsFromNodeToNodes(JudgmentsSourceNodeID, toAltsIDs, sMsg, srcUserGuid, destUsersList, CopyMode, pwOnly) Then
                            sMsg = "Judgments were successfully copied."
                        Else 'AS/16216e
                            sMsg = "Cannot paste - source and destination measure types mismatch." 'AS/16216e
                        End If 'AS/16216a==
                    Else 'alternative 'AS/11642d===
                        'If PM.CopyJudgmentsFromAlternativeToAlternatives(JudgmentsSourceNodeID, JudgmentsTargetNodeID, sMsg, srcUserGuid, destUsersList, CopyMode, pwOnly) Then
                        If PM.CopyJudgmentsFromAlternativeToAlternatives(JudgmentsSourceNodeID, toAltsIDs, sMsg, srcUserGuid, destUsersList, CopyMode, pwOnly) Then
                            If sMsg = "" Then
                                sMsg = "Judgments were successfully rewritten."
                            Else
                                sMsg = sMsg
                            End If
                        Else
                            sMsg = ParseString("Cannot paste - source and destination %%alternatives%% settings are not the same.")
                        End If
                    End If 'AS/11642d==


                    'If Not isAlt Then 'AS/11593e===
                    '    PM.CopyJudgmentsFromClusterToCluster(JudgmentsSourceNodeID, JudgmentsTargetNodeID, sMsg)
                    'Else
                    '    PM.CopyJudgmentsFromAlternativeToAlternative(JudgmentsSourceNodeID, JudgmentsTargetNodeID, sMsg)
                    'End If 'AS/11593e==
                    ''A1505 == 

                    If sMsg = "" Then 'AS/16059c===
                        sMsg = "Judgments were successfully rewritten."
                    Else
                        sMsg = sMsg
                    End If 'AS/16059c==
                End If
                tResult = String.Format("['{0}','{1}']", sAction, sMsg)
            Case ACTION_EXPORT_OBJECTIVES
                Dim sExportedStructure As String = ""
                ExportHierarchyToText(PM.ActiveObjectives.Nodes(0), sExportedStructure, 0)
                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(sExportedStructure, True))
            Case ACTION_EXPORT_ALTERNATIVES
                Dim sExportedStructure As String = ExportAlternativesToText()
                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(sExportedStructure, True))
            Case ACTION_EXPORT_MODEL, ACTION_EXPORT_MODEL_FILE
                Dim sExportedStructure As String = clsTextModel.GetModelStructure(PRJ, PM.IsRiskProject, True)
                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(sExportedStructure, True))
            Case "sort_name_asc", "sort_name_desc", "sort_priority_asc", "sort_priority_desc"
                If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then
                    SortAlternatives(sAction = "sort_name_asc" Or sAction = "sort_name_desc", If(sAction = "sort_name_asc" Or sAction = "sort_priority_asc", 0, 1))
                Else
                    Dim sGuid As String = (GetParam(args, "id")).Trim()
                    Dim ID As Guid = Guid.Empty
                    If sGuid <> "" Then ID = New Guid(sGuid)
                    SortNodesBelow(ID, sAction = "sort_name_asc" Or sAction = "sort_name_desc", If(sAction = "sort_name_asc" Or sAction = "sort_priority_asc", 0, 1))
                End If
                tResult = GetCallbackData(sAction)
            Case "priorities_none", "priorities_local", "priorities_global", "priorities_both"
                Select Case sAction
                    Case "priorities_none"
                        PM.Parameters.Structure_DisplayPrioritiesMode = 0
                    Case "priorities_local"
                        PM.Parameters.Structure_DisplayPrioritiesMode = 1
                    Case "priorities_global"
                        PM.Parameters.Structure_DisplayPrioritiesMode = 2
                    Case "priorities_both"
                        PM.Parameters.Structure_DisplayPrioritiesMode = 3
                End Select
                PM.Parameters.Save()
                tResult = GetCallbackData(sAction)
            Case "priorities_update"
                tResult = GetCallbackData(sAction)
            Case "v_splitter_size"
                Dim sValue As String = (GetParam(args, "value")).Trim()
                Dim tValue As Double
                If String2Double(sValue, tValue) Then
                    PM.Parameters.Hierarchy_VerticalSplitterSize = tValue
                    PM.Parameters.Save()
                End If
                tResult = String.Format("['{0}']", sAction)                        
                                If Not PM.PipeBuilder.PipeCreated Then PM.ResourceAligner.Load()
                                If Not PM.PipeBuilder.PipeCreated Then PM.ResourceAligner.Load()
                    PM.PipeBuilder.PipeCreated = False
            Case "reorder_alts"
                Dim tFromIndex As Integer = CInt((GetParam(args, "from_index")))
                Dim tToIndex As Integer = CInt((GetParam(args, "to_index")))

                If PM.ActiveAlternatives.MoveAlternative(tFromIndex, tToIndex) Then
                    PRJ.SaveStructure(PagePrefix + ParseString("%%Alternatives%%: move %%alternative%%"), True, True, PM.ActiveAlternatives.Nodes.Item(tFromIndex).NodeName)
                End If

                PM.PipeBuilder.PipeCreated = False

                tResult = String.Format("['{0}','{1}',{2}]", sAction, JS_SafeString(""), GetAlternativesData())
            Case "edit_goal_name"
                Dim sNodeGUID As String = (GetParam(args, "nodeid")).Trim()
                Dim sValue As String = (GetParam(args, "value")).Trim()
                Dim sInfodoc As String = (GetParam(args, "infodoc")).Trim()
                'Dim fDontShow As Boolean = Str2bool((GetParam(args, "dont_show")).Trim())

                'If fDontShow Then GoalNodeChecked = True

                If sNodeGUID <> "" AndAlso sValue <> "" AndAlso App.HasActiveProject Then
                    Dim tPrj As clsProject = App.ActiveProject
                    Dim NodeGuid As Guid = New Guid(sNodeGUID)
                    Dim Node As clsNode = tPrj.ProjectManager.Hierarchy(tprj.ProjectManager.ActiveHierarchy).GetNodeByID(NodeGuid)
                    If Node IsNot Nothing Then
                        Node.NodeName = sValue.Trim()
                        Node.InfoDoc = sInfodoc.Trim()
                        tPrj.SaveStructure(PagePrefix + "After editing Goal", False, True, Node.NodeName)
                    End If
                End If

                tResult = GetCallbackData("refresh_full")
            Case "hierarchy_shown"
                If PRJ.ProjectStatus = ecProjectStatus.psActive AndAlso PM.Parameters.Hierarchy_WasShownToPM <> "" AndAlso Not Str2Bool(PM.Parameters.Hierarchy_WasShownToPM) Then
                    PM.Parameters.Hierarchy_WasShownToPM = Boolean.TrueString
                    PM.Parameters.Save()
                End If

                tResult = String.Format("['{0}']", sAction)

            'Case "dont_show"
            '    GoalNodeChecked = True
            '    tResult = String.Format("['{0}']", sAction)

            Case "save_settings"
                Dim sRectHeight As String = (GetParam(args, "rect_height"))
                Dim sRectMinWidth As String = (GetParam(args, "min_rect_width"))
                Dim sRectMaxWidth As String = (GetParam(args, "max_rect_width"))
                If Integer.TryParse(sRectHeight, PM.Parameters.Hierarchy_RectangleHeight) Or Integer.TryParse(sRectMinWidth, PM.Parameters.Hierarchy_RectangleMinWidth) Or Integer.TryParse(sRectMaxWidth, PM.Parameters.Hierarchy_RectangleMaxWidth) Then
                    PM.Parameters.Save()
                End If
                tResult = String.Format("['{0}']", sAction)

            Case "rename_column"
                Dim AttrIndex As Integer = CInt((GetParam(args, "clmn")))
                Dim NewName As String = (GetParam(args, "name")).Trim  ' Anti-XSS
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    AttributesList(AttrIndex).Name = NewName
                    Dim attr As clsAttribute = AttributesList(AttrIndex) 'PM.Attributes.GetAttributeByID(AttributesList(AttrIndex).ID)
                    If attr IsNot Nothing Then
                        attr.Name = NewName
                        SaveAttributes(String.Format("Rename attribute '{0}'", ShortString(NewName, 40, True)))    ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "add_column"
                Dim NewName As String = (GetParam(args, "name")).Trim  ' Anti-XSS
                Dim NewType As AttributeValueTypes = CType((GetParam(args, "type")), AttributeValueTypes)
                Dim attr As clsAttribute = PM.Attributes.AddAttribute(Guid.NewGuid(), NewName, AttributeTypes.atAlternative, NewType, Nothing, False, Guid.Empty)
                If attr IsNot Nothing Then
                    'try to assign the default value
                    If NewType = AttributeValueTypes.avtString OrElse NewType = AttributeValueTypes.avtLong OrElse NewType = AttributeValueTypes.avtDouble OrElse NewType = AttributeValueTypes.avtBoolean Then
                        Dim DefVal As String = (GetParam(args, "def_val")).Trim    ' Anti-XSS
                        If Not String.IsNullOrEmpty(DefVal) Then
                            Select Case NewType
                                Case AttributeValueTypes.avtString
                                    attr.DefaultValue = DefVal
                                Case AttributeValueTypes.avtLong
                                    Dim tIntVal As Integer
                                    If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal
                                Case AttributeValueTypes.avtDouble
                                    Dim tDblVal As Double
                                    If String2Double(DefVal, tDblVal) Then attr.DefaultValue = tDblVal
                                Case AttributeValueTypes.avtBoolean
                                    Dim tIntVal As Integer
                                    If Integer.TryParse(DefVal, tIntVal) Then attr.DefaultValue = tIntVal = 1
                            End Select
                        End If
                    End If

                    SaveAttributes(String.Format("Add attribute '{0}'", ShortString(NewName, 40)))    ' D3790
                End If

                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "del_column"
                Dim AttrIndex As Integer = CInt((GetParam(args, "clmn")))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim sName As String = AttributesList(AttrIndex).Name    ' D3790
                        PM.Attributes.RemoveAttribute(attr.ID)
                        SaveAttributes(String.Format("Delete attribute '{0}'", ShortString(sName, 40)))    ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "set_default_value"
                Dim AttrIndex As Integer = CInt((GetParam(args, "clmn")))
                Dim value As String = (GetParam(args, "def_val")).Trim ' Anti-XSS
                Dim fValueChanged As Boolean = False
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Select Case attr.ValueType
                            Case AttributeValueTypes.avtString
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                Else
                                    attr.DefaultValue = value
                                End If
                                fValueChanged = True
                            Case AttributeValueTypes.avtBoolean
                                If value = "1" OrElse value = "0" Then
                                    attr.DefaultValue = value = "1"
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtLong
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                Else
                                    Dim intValue As Integer
                                    If Integer.TryParse(value, intValue) Then
                                        attr.DefaultValue = intValue
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtDouble
                                If String.IsNullOrEmpty(value) Then
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                Else
                                    Dim dblValue As Double
                                    If String2Double(value, dblValue) Then
                                        attr.DefaultValue = dblValue
                                        fValueChanged = True
                                    End If
                                End If
                            Case AttributeValueTypes.avtEnumeration
                                Dim ItemIndex As Integer = CInt((GetParam(args, "item_index")))
                                If value = "1" Then
                                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                    If aEnum IsNot Nothing Then
                                        If ItemIndex >= 0 AndAlso ItemIndex < aEnum.Items.Count Then
                                            attr.DefaultValue = aEnum.Items(ItemIndex).ID
                                            fValueChanged = True
                                        End If
                                    End If
                                Else
                                    attr.DefaultValue = Nothing
                                    fValueChanged = True
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                Dim ItemIndex As Integer = CInt((GetParam(args, "item_index")))
                                Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                                If aEnum IsNot Nothing Then
                                    If value = "1" Then
                                        If ItemIndex >= 0 AndAlso ItemIndex < aEnum.Items.Count AndAlso (attr.DefaultValue Is Nothing OrElse Not CStr(attr.DefaultValue).Contains(aEnum.Items(ItemIndex).ID.ToString)) Then
                                            attr.DefaultValue = CStr(attr.DefaultValue) + CStr(If(attr.DefaultValue Is Nothing OrElse CStr(attr.DefaultValue) = "", "", ";") + aEnum.Items(ItemIndex).ID.ToString)
                                            fValueChanged = True
                                        End If
                                    Else
                                        attr.DefaultValue = CStr(attr.DefaultValue).Replace(";" + aEnum.Items(ItemIndex).ID.ToString, "").Replace(aEnum.Items(ItemIndex).ID.ToString, "")
                                        fValueChanged = True
                                    End If
                                End If
                        End Select
                        If fValueChanged Then SaveAttributes(String.Format("Set default value for '{0}'", ShortString(App.GetAttributeName(attr), 40))) ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "attributes_reorder"
                Dim fChanged As Boolean = False
                Dim lst As String = (GetParam(args, "lst"))    ' Anti-XSS
                If Not String.IsNullOrEmpty(lst) Then
                    Dim attrList As List(Of clsAttribute) = AttributesList 'PM.Attributes.GetAlternativesAttributes().Where(Function(a) (Not a.IsDefault) AndAlso (IsMultiCategoricalAttributesAllowed OrElse a.ValueType <> AttributeValueTypes.avtEnumerationMulti)).ToList
                    Dim globalI As New List(Of Integer)
                    For Each attr In attrList
                        globalI.Add(PM.Attributes.AttributesList.IndexOf(attr))
                    Next
                    Dim indices As String() = lst.Split(CChar(","))
                    Dim i As Integer = 0
                    For Each id As String In indices
                        Dim idx As Integer = CInt(id)
                        If idx >= 0 AndAlso idx < attrList.Count AndAlso idx <> i Then
                            Dim attrN As clsAttribute = attrList(idx)
                            PM.Attributes.AttributesList.RemoveAt(globalI(i))
                            PM.Attributes.AttributesList.Insert(globalI(i), attrN)
                            fChanged = True
                        End If
                        i += 1
                    Next
                End If
                If fChanged Then
                    SaveAttributes("Reorder attributes")    ' D3790
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "rename_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim ItemIndex As Integer = CInt(GetParam(args, "item"))
                Dim NewName As String = (GetParam(args, "name")).Trim  ' Anti-XSS
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                        If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex >= 0 AndAlso ItemIndex < items.Items.Count Then
                            items.Items(ItemIndex).Value = NewName
                            SaveAttributes(String.Format("Rename item '{0}'", ShortString(NewName, 40)))    ' D3790
                        End If
                    End If
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "add_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim NewName As String = (GetParam(args, "name")).Trim  ' Anti-XSS
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                        aEnum = New clsAttributeEnumeration()
                        aEnum.ID = Guid.NewGuid
                        aEnum.Name = attr.Name
                        attr.EnumID = aEnum.ID
                        PM.Attributes.Enumerations.Add(aEnum)
                    End If

                    Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(NewName)
                    SaveAttributes(String.Format("Add item '{0}'", ShortString(NewName, 40)))    ' D3790
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "del_item"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim ItemIndex As Integer = CInt(GetParam(args, "item"))
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim items As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If items IsNot Nothing AndAlso items.Items IsNot Nothing AndAlso items.Items.Count > 0 AndAlso ItemIndex >= 0 AndAlso ItemIndex < items.Items.Count Then
                        Dim sName As String = items.Items(ItemIndex).Value
                        items.Items.RemoveAt(ItemIndex)
                        SaveAttributes(String.Format("Delete item '{0}'", ShortString(sName, 40)))    ' D3790
                    End If
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "add_item_and_assign"
                Dim AttrIndex As Integer = CInt(GetParam(args, "clmn"))
                Dim NewName As String = (GetParam(args, "name")).Trim 
                Dim altID As String = (GetParam(args, "alt_id")).Trim 
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count AndAlso NewName.Length > 0 Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    Dim aEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(attr.EnumID)
                    If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                        aEnum = New clsAttributeEnumeration()
                        aEnum.ID = Guid.NewGuid
                        aEnum.Name = attr.Name
                        attr.EnumID = aEnum.ID
                        PM.Attributes.Enumerations.Add(aEnum)
                    End If

                    Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(NewName)

                    Dim alt As clsNode = pm.ActiveAlternatives.GetNodeByID(New Guid(altID))
                    PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, eItem.ID, alt.NodeGuidID, Guid.Empty)

                    SaveAttributes(String.Format("Add item '{0}'", ShortString(NewName, 40)))
                    SaveAttributesValues(String.Format("Assign category to '{0}'", ShortString(alt.NodeName, 40)))
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())
            Case "paste_attribute_data"
                Dim PasteAttributeData As String = ""
                Dim AttrIndex As Integer = CInt(GetParam(args, "attr_idx"))
                Dim sData As String = (GetParam(args, "data"))  ' Anti-XSS
                Dim cells As String() = sData.Split(Chr(10))
                Dim cells_count As Integer = cells.Count
                Dim fItemsChanged As Boolean = False
                Dim fValueChanged As Boolean = False
                Dim altsList As List(Of clsNode) = PM.ActiveAlternatives.TerminalNodes'.OrderBy(Function(a) a.iIndex).ToList
                If AttrIndex >= 0 AndAlso AttrIndex < AttributesList.Count Then
                    Dim attr As clsAttribute = AttributesList(AttrIndex)
                    If attr IsNot Nothing Then
                        Dim aEnum As clsAttributeEnumeration = Nothing
                        If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then aEnum = PM.Attributes.GetEnumByID(attr.EnumID)
                        Dim alts_count As Integer = altsList.Count
                        If alts_count < cells.Count Then cells_count = alts_count
                        For i As Integer = 0 To cells_count - 1                            
                            Dim value As String = cells(i).Trim
                            Dim alt As clsNode = altsList(i)
                            Select Case attr.ValueType
                                Case AttributeValueTypes.avtString
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, alt.NodeGuidID, Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtBoolean
                                    If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtBoolean, Str2Bool(value), alt.NodeGuidID, Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtLong
                                    Dim intValue As Integer
                                    If String.IsNullOrEmpty(value) Then value = Nothing
                                    If String.IsNullOrEmpty(value) OrElse Integer.TryParse(value, intValue) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, intValue, alt.NodeGuidID, Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtDouble
                                    Dim dblValue As Double
                                    If String.IsNullOrEmpty(value) Then value = Nothing
                                    If String.IsNullOrEmpty(value) OrElse String2Double(value, dblValue) AndAlso PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, dblValue, alt.NodeGuidID, Guid.Empty) Then fValueChanged = True
                                Case AttributeValueTypes.avtEnumeration
                                    If Not String.IsNullOrEmpty(value) Then
                                        ' check if enum item exists and create new if not
                                        Dim enumItem As clsAttributeEnumerationItem = Nothing

                                        If aEnum IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                                            enumItem = aEnum.GetItemByValue(value)
                                        Else
                                            aEnum = New clsAttributeEnumeration()
                                            aEnum.ID = Guid.NewGuid
                                            aEnum.Name = attr.Name
                                            attr.EnumID = aEnum.ID
                                            PM.Attributes.Enumerations.Add(aEnum)
                                        End If

                                        If enumItem Is Nothing Then
                                            enumItem = aEnum.AddItem(value)
                                            fItemsChanged = True
                                        End If
                                        ' assign attribute value
                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, enumItem.ID, alt.NodeGuidID, Guid.Empty) Then fValueChanged = True
                                    Else
                                        If PM.Attributes.SetAttributeValue(attr.ID, UNDEFINED_USER_ID, AttributeValueTypes.avtEnumeration, Nothing, alt.NodeGuidID, Guid.Empty) Then fValueChanged = True
                                    End If
                            End Select
                        Next
                        PasteAttributeData = GetAttributeItemsData(attr, aEnum)
                    End If
                    If fItemsChanged Then SaveAttributes(String.Format("Paste attrib data '{0}'", ShortString(attr.Name, 40)))
                    If fValueChanged Then SaveAttributesValues(CStr(IIf(fItemsChanged, "", String.Format("Paste attrib data '{0}'", ShortString(attr.Name, 40)))))
                    'If fItemsChanged OrElse fValueChanged AndAlso PM.ResourceAligner.Scenarios.GlobalSettings.isAutoSolve Then PM.ResourceAligner.Solve() ' D3900
                End If
                tResult = String.Format("['{0}',{1},{2},{3}]", sAction, GetAlternativesData(), GetAlternativesColumns(), GetAttributesData())

            Case "set_color"
                Dim sValue As String = GetParam(args, "value")
                Dim AltGuid As Guid

                Dim sMsg As String = ParseString("Set %%alternative%% color")

                if Guid.TryParse(GetParam(args, "guid"), AltGuid) Then
                    sValue = sValue.Replace("#", "")
                    'If sValue.Length = 6 Then sValue = "ff" + sValue
                    Dim tValue As Long = BrushToLong(sValue)

                    PM.Attributes.SetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, tValue, AltGuid, Guid.Empty)
                    WriteAttributeValues(PRJ, sMsg, sMsg)
                End If                
                tResult = String.Format("['{0}','{1}','{2}']", sAction, AltGuid.ToString, "#" + sValue)
            Case "event_id_mode"
                PM.Parameters.NodeVisibleIndexMode = CType(Param2Int(args, "value"), IDColumnModes)
                PM.Parameters.Save()
                tResult = String.Format("['{0}',{1},{2}]", sAction, GetAlternativesData(), GetAlternativesColumns())
            Case "obj_id_mode"
                PM.Parameters.ObjectiveIndexIsVisible = Param2bool(args, "value")
                PM.Parameters.Save()
                tResult = GetCallbackData(sAction)
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub    

    'Public Function AddNode(ByVal HierarchyType As ECCore.ECHierarchyID, ByVal ParentNodeIDs As List(Of Guid), ByVal NodeName As String, NodeDescription As String) As clsNode
    Public Function AddNodes(ByVal HierarchyID As ECCore.ECHierarchyID, ByVal ParentNodeIDs As List(Of Guid), ByVal tNodeNames As String, insertBelowNodeID As Guid) As Boolean
        Dim retVal As Boolean = False

        Dim H As ECCore.clsHierarchy = PM.Hierarchy(HierarchyID)
        If H IsNot Nothing AndAlso ParentNodeIDs.Count > 0 Then
            Dim ParentNode As ECCore.clsNode = H.GetNodeByID(ParentNodeIDs(0))

            Dim ID As Integer
            If ParentNode Is Nothing Then
                ID = -1
            Else
                ID = ParentNode.NodeID
            End If

            'Dim namesList As String() = NodeNames.Split(Environment.NewLine)
            Dim NodesNames As String() = tNodeNames.Split(Chr(10))

            Dim NamesList As New ArrayList
            For Each s As String In NodesNames
                NamesList.Add(s)
            Next

            Dim HP As New clsHierarchyStringsParser(H, ParentNode, HierarchyID = ECHierarchyID.hidLikelihood Or HierarchyID = ECHierarchyID.hidImpact)
            Dim NewNodesIDs As New List(Of Guid)
            HP.Parse(NamesList, NewNodesIDs, False)

            'Dim newNodes As New List(Of clsNode)

            'For Each NodeName As String In namesList
            '    Dim node As ECCore.clsNode = H.AddNode(ID)
            '    node.NodeName = NodeName.Trim
            '    'node.InfoDoc = NodeDescription
            '    node.ParentNodesGuids = Nothing

            '    node.ParentNodesGuids = New List(Of Guid)

            '    For Each gId As Guid In ParentNodeIDs
            '        node.ParentNodesGuids.Add(gId)
            '    Next

            '    If IsCategory IsNot Nothing AndAlso IsCategory.HasValue Then
            '        If IsCategory.Value Then
            '            H.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_NODE_TYPE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, CInt(RiskNodeType.ntCategory), node.NodeGuidID, Nothing)
            '        Else
            '            H.ProjectManager.Attributes.SetAttributeValue(ATTRIBUTE_RISK_NODE_TYPE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, CInt(RiskNodeType.ntUncertainty), node.NodeGuidID, Nothing)
            '        End If
            '    End If

            '    newNodes.Add(node)
            'Next

            'If Not insertBelowNodeID.Equals(Guid.Empty) Then
            '    Dim insertBelowNode As clsNode = H.GetNodeByID(insertBelowNodeID)
            '    If insertBelowNode IsNot Nothing Then 
            '        For i As Integer = newNodes.Count - 1 To 0 Step -1
            '            H.MoveNode(newNodes(i), insertBelowNode, NodeMoveAction.nmaAfterNode)
            '        Next
            '    End If
            'End If

            H.FixChildrenLinks()

            'PM.StorageManager.Writer.SaveProject(True)
            PRJ.SaveStructure(PagePrefix + "Hierarchy: added nodes", True, True, String.Join(", ", NodesNames))
            retVal = True
        End If

        PM.PipeBuilder.PipeCreated = False
        
        Return retVal
    End Function

    Public Function AddAlternatives(ByVal ContributedNodeIDs As List(Of Guid), ByVal NodeNames As String, insertBelowNodeID As Guid) As Boolean
        Dim retVal As Boolean = False

        Dim AH As ECCore.clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If AH IsNot Nothing Then
            Dim namesList As String() = NodeNames.Split(Chr(10))
            Dim altsGuids As List(Of Guid) = New List(Of Guid)
            For Each row As String In namesList
                Dim NodeName As String = ""
                Dim NodeDescription As String = ""
                Dim EventType As EventType = EventType.Risk
                Dim iEventType As Integer = 0
                Dim cols As String() = row.Split(CChar(vbTab))
                If cols.Length > 0 Then
                    Dim extra() As String = cols(0).Split(New String() {"--extra--"}, StringSplitOptions.RemoveEmptyEntries)
                    If extra.Length > 0 Then NodeName = extra(0) Else NodeName = cols(0)
                    If extra.Length > 2 AndAlso extra(2) <> "" Then
                        If (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) AndAlso Integer.TryParse(extra(2), iEventType) AndAlso [Enum].IsDefined(GetType(EventType), iEventType) Then    ' D6798
                            EventType = CType(iEventType, EventType)
                        End If
                    End If
                End If
                If cols.Length > 1 Then 
                    NodeDescription = cols(1)
                End If                
                If Not String.IsNullOrEmpty(NodeName.Trim) Then
                    Dim alt As ECCore.clsNode = AH.AddNode(-1)
                    alt.NodeName = NodeName.Trim
                    alt.InfoDoc = NodeDescription.Trim
                    alt.EventType = EventType
                    altsGuids.Add(alt.NodeGuidID)             
                End If   
            Next

            ' move nodes
            If insertBelowNodeID <> Guid.Empty Then 
                Dim upperNode As clsNode = AH.GetNodeByID(insertBelowNodeID)
                If upperNode IsNot Nothing Then 
                    For Each AltGuid As Guid In altsGuids 
                        Dim tAlt As clsNode = AH.GetNodeByID(AltGuid)
                        AH.MoveNode(tAlt, upperNode, NodeMoveAction.nmaAfterNode)
                        upperNode = tAlt
                    Next
                End If
            End If


            For Each altGuid As Guid In altsGuids
                PM.UpdateContributions(altGuid, ContributedNodeIDs, CType(PM.ActiveHierarchy, ECHierarchyID), False)
            Next
            'PM.StorageManager.Writer.SaveProject(True)
            PRJ.SaveStructure(ParseString("Hierarchy: added %%alternatives%%"), True, True, NodeNames.Replace(Chr(10), ", "))

            retVal = True
        End If

        PM.PipeBuilder.PipeCreated = False

        Return retVal
    End Function

    Public Enum AlternativesMissingContributionsType
        amcNone = 0
        amcLikelihood = 1
        amcImpact = 2
        amcBoth = 3
    End Enum

    Public Function GetAnyAlternativeMissingContribution() As AlternativesMissingContributionsType
        Dim res As AlternativesMissingContributionsType = AlternativesMissingContributionsType.amcNone
        Dim AnyLikelihoodContributionsMissing As Boolean = False
        Dim AnyImpactContributionsMissing As Boolean = False
        Dim ActiveProject = PRJ
        Dim AltH = ActiveProject.ProjectManager.AltsHierarchy(ActiveProject.ProjectManager.ActiveAltsHierarchy)
        If AltH IsNot Nothing AndAlso AltH.Nodes IsNot Nothing Then
            Dim AlternativesCount As Integer = AltH.Nodes.Count

            Dim HLikelihood = ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidLikelihood)
            If HLikelihood IsNot Nothing AndAlso (Not PM.IsRiskProject OrElse HLikelihood.Nodes.Count > 1) Then
                For Each node In HLikelihood.TerminalNodes
                    Dim contAlts = node.GetContributedAlternatives
                    If contAlts IsNot Nothing AndAlso contAlts.Count < AlternativesCount Then
                        AnyLikelihoodContributionsMissing = True
                        Exit For
                    End If
                Next
            End If

            Dim HImpact = ActiveProject.ProjectManager.Hierarchy(ECHierarchyID.hidImpact)
            If HImpact IsNot Nothing Then
                For Each node In HImpact.TerminalNodes
                    Dim contAlts = node.GetContributedAlternatives
                    If contAlts IsNot Nothing AndAlso contAlts.Count < AlternativesCount Then
                        AnyImpactContributionsMissing = True
                        Exit For
                    End If
                Next
            End If
        End If

        If AnyLikelihoodContributionsMissing AndAlso AnyImpactContributionsMissing Then
            res = AlternativesMissingContributionsType.amcBoth
        Else
            If AnyLikelihoodContributionsMissing Then res = AlternativesMissingContributionsType.amcLikelihood
            If AnyImpactContributionsMissing Then res = AlternativesMissingContributionsType.amcImpact
        End If

        Return res
    End Function

    'Public Function EditNode(ByVal HierarchyType As ECCore.ECHierarchyID, ByVal NodeID As Guid, ByVal ParentNodeIDs As List(Of Guid), ByVal NodeName As String, NodeDescription As String) As Boolean
    Public Function EditNode(ByVal H As ECCore.clsHierarchy, ByVal NodeID As Guid, ByVal ParentNodeIDs As List(Of Guid), ByVal NodeName As String, IsCategory As Boolean?) As Boolean
        Dim retVal As Boolean = False

        If H IsNot Nothing Then
            Dim ParentNode As ECCore.clsNode = Nothing
            If ParentNodeIDs IsNot Nothing AndAlso ParentNodeIDs.Count > 0 Then ParentNode = H.GetNodeByID(ParentNodeIDs(0))
            Dim Node As ECCore.clsNode = H.GetNodeByID(NodeID)

            If Node IsNot Nothing Then
                Dim ID As Integer
                If ParentNode Is Nothing Then
                    ID = -1
                Else
                    ID = ParentNode.NodeID
                End If

                Node.NodeName = NodeName
                Node.ParentNodeID = ID

                If IsCategory IsNot Nothing AndAlso IsCategory.HasValue Then
                    Node.RiskNodeType = If(IsCategory.Value, RiskNodeType.ntCategory, RiskNodeType.ntUncertainty)
                End If

                If Node.ParentNode IsNot Nothing Then
                    If ParentNodeIDs IsNot Nothing AndAlso ParentNodeIDs.IndexOf(Node.ParentNode.NodeGuidID) < 0 Then Node.ParentNode = ParentNode
                End If

                'If Not String.IsNullOrEmpty(NodeDescription) Then Node.InfoDoc = NodeDescription
                Node.ParentNodesGuids = Nothing

                Node.ParentNodesGuids = New List(Of Guid)

                If ParentNodeIDs IsNot Nothing Then
                    For Each gId As Guid In ParentNodeIDs
                        Node.ParentNodesGuids.Add(gId)
                    Next
                End If

                'PM.StorageManager.Writer.SaveProject(True)
                PRJ.SaveStructure(ParseString("Hierarchy: after editing node"), True, True, NodeName)
                retVal = True
            End If
        End If

        PM.PipeBuilder.PipeCreated = False

        Return retVal
    End Function

    Public Function DeleteNode(ByVal NodeID As Guid, IsAlternative As Boolean) As Boolean
        Dim retVal As Boolean = False
        Dim H As ECCore.clsHierarchy = Nothing
        If IsAlternative Then H = PM.AltsHierarchy(PM.ActiveAltsHierarchy) Else H = PM.ActiveObjectives

        If H IsNot Nothing Then
            Dim node As ECCore.clsNode = H.GetNodeByID(NodeID)
            If node IsNot Nothing AndAlso Not (Not node.IsAlternative And node.ParentNode Is Nothing) Then
                H.DeleteNode(node)

                Dim altBoardNode = PM.AntiguaDashboard.GetNodeByGuid(NodeID)
                If altBoardNode IsNot Nothing Then
                    PM.AntiguaDashboard.RemoveNodeByGuid(NodeID)
                    SaveAntiguaDashboard()
                End If

                'PM.StorageManager.Writer.SaveProject(True)
                PRJ.SaveStructure(ParseString(PagePrefix + If(IsAlternative, ParseString("after deleting %%alternative%%"), ParseString("after deleting %%objective%%"))), True, True, node.NodeName)

                retVal = True
            End If
        End If

        If IsAlternative Then PM.PipeBuilder.PipeCreated = False

        Return retVal
    End Function

    Public Function DeleteNodes(ByVal IDs As List(Of Guid)) As Boolean
        Dim retVal As Boolean = False
        Dim AH As ECCore.clsHierarchy = Nothing
        Dim H As ECCore.clsHierarchy = Nothing
        AH = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        H = PM.ActiveObjectives

        If AH IsNot Nothing And H IsNot Nothing Then

            Dim NodeNames As String = ""

            For Each NodeID As Guid In IDs
                Dim node As ECCore.clsNode = H.GetNodeByID(NodeID)
                If node IsNot Nothing AndAlso Not (Not node.IsAlternative And node.ParentNode Is Nothing) Then
                    H.DeleteNode(node)
                    NodeNames += If(NodeNames = "", "", ", ") + node.NodeName
                End If

                If node Is Nothing Then
                    node = AH.GetNodeByID(NodeID)
                    If node IsNot Nothing Then
                        AH.DeleteNode(node)
                        NodeNames += If(NodeNames = "", "", ", ") + node.NodeName
                    End If
                End If

                Dim altBoardNode = PM.AntiguaDashboard.GetNodeByGuid(NodeID)
                If altBoardNode IsNot Nothing Then
                    PM.AntiguaDashboard.RemoveNodeByGuid(NodeID)
                End If

                retVal = True
            Next

            SaveAntiguaDashboard()
            PRJ.SaveStructure(PagePrefix + "after deleting nodes", True, True, NodeNames)
        End If

        PM.PipeBuilder.PipeCreated = False

        Return retVal
    End Function

    Private Function HierarchyID(ByVal HierarchyType As ECCore.ECHierarchyType) As Guid
        Select Case HierarchyType
            Case ECHierarchyType.htModel
                Return PM.ActiveObjectives.HierarchyGuidID
            Case ECHierarchyType.htAlternative
                Return PM.AltsHierarchy(PM.ActiveAltsHierarchy).HierarchyGuidID
        End Select
    End Function


    Public Sub DuplicateNode(ByVal HierarchyType As ECCore.ECHierarchyType, ByVal SrcNodeID As Guid, ByVal DestNodeNameFormat As String, ByVal tDuplicateJudgments As Boolean)
        Dim res As List(Of clsNode) = Nothing
            Dim H As ECCore.clsHierarchy = PM.GetAnyHierarchyByID(HierarchyID(HierarchyType))
            If H IsNot Nothing Then
                PM.StorageManager.Reader.LoadInfoDocs()

                Dim srcNode As ECCore.clsNode = H.GetNodeByID(SrcNodeID)
                If srcNode IsNot Nothing Then
                    Dim resList As List(Of ECCore.clsNode) = H.CopyNode(srcNode, srcNode, ECCore.NodeMoveAction.nmaAfterNode, tDuplicateJudgments)

                    res = New List(Of clsNode)
                    If resList IsNot Nothing AndAlso resList.Count > 0 Then
                        For Each node In resList
                            node.NodeName = String.Format(DestNodeNameFormat, node.NodeName)

                            Dim altAttrList As List(Of clsAttribute) = Nothing
                            If PM.Attributes IsNot Nothing Then
                                altAttrList = PM.Attributes.GetAlternativesAttributes
                            End If

                            Dim likelihoodAttrList As List(Of clsAttribute) = Nothing
                            If PM.Attributes IsNot Nothing Then
                                likelihoodAttrList = PM.Attributes.GetLikelihodAttributes
                            End If

                            Dim impactAttrList As List(Of clsAttribute) = Nothing
                            If App.isRiskEnabled Then
                                If PM.Attributes IsNot Nothing Then
                                    impactAttrList = PM.Attributes.GetImpactAttributes
                                End If
                            End If

                        Next
                    End If
                    PRJ.SaveStructure("Duplicate node", , , GetNodeTypeAndName(srcNode))   ' D3571 + D3731
                    'PM.StorageManager.Writer.SaveProject(True)
                    PM.StorageManager.Writer.SaveInfoDocs() 'C0827                    
                End If
            End If

        PM.PipeBuilder.PipeCreated = False
    End Sub

    Private Function MoveNode(SourceNodeGuid As Guid, TargetNodeGuid As Guid, Optional MoveType As NodeMoveAction = NodeMoveAction.nmaAsChildOfNode) As Boolean
        Dim retVal As Boolean = False
        Dim H As clsHierarchy = PM.ActiveObjectives

        If H IsNot Nothing Then
            Dim SourceNode As ECCore.clsNode = H.GetNodeByID(SourceNodeGuid)
            Dim TargetNode As ECCore.clsNode = H.GetNodeByID(TargetNodeGuid)
            If SourceNode IsNot Nothing AndAlso TargetNode IsNot Nothing Then
                SourceNode.ParentNode = Nothing
                SourceNode.ParentNodesGuids.Clear()

                retVal = H.MoveNode(SourceNode, TargetNode, MoveType)

                'PM.StorageManager.Writer.SaveProject(True)
                PRJ.SaveStructure(PagePrefix + "after moving nodes", True, True, SourceNode.NodeName)
            End If
        End If

        PM.PipeBuilder.PipeCreated = False

        Return retVal
    End Function

    Public Function CopyNode(ByVal HierarchyID As ECCore.ECHierarchyID, ByVal SrcNodeID As Guid, ByVal DestNodeID As Guid, ByVal OperationType As ECCore.ECTypes.NodeMoveAction, ByVal DestNodeNameFormat As String, ByVal tDuplicateJudgments As Boolean) As Boolean
        Dim res As Boolean = False

        Dim H As ECCore.clsHierarchy = PM.Hierarchy(HierarchyID)
        If H IsNot Nothing Then
            PM.StorageManager.Reader.LoadInfoDocs() 'C0954

            Dim srcNode As ECCore.clsNode = H.GetNodeByID(SrcNodeID)
            Dim destNode As ECCore.clsNode = H.GetNodeByID(DestNodeID)
            If srcNode IsNot Nothing AndAlso destNode IsNot Nothing Then
                Dim resList As List(Of ECCore.clsNode) = H.CopyNode(srcNode, destNode, CType(OperationType, ECCore.NodeMoveAction), tDuplicateJudgments)

                If resList IsNot Nothing AndAlso resList.Count > 0 Then
                    For Each node In resList
                        node.NodeName = String.Format(DestNodeNameFormat, node.NodeName)
                        res = True
                    Next
                End If

                PRJ.SaveStructure(PagePrefix + "Copy node", , , String.Format("'{0}' -> '{1}' ({2})", srcNode.NodeName, destNode.NodeName, OperationType.ToString))   ' D3571 + D3731
                'PM.StorageManager.Writer.SaveProject(True)
                PM.StorageManager.Writer.SaveInfoDocs() 'C0827
            End If
        End If

        PM.PipeBuilder.PipeCreated = False

        Return res
    End Function


    Private Sub SaveAntiguaDashboard()
        Dim CanvasMasterDBName = ExpertChoice.Service.WebConfigOption(ExpertChoice.Web.WebOptions._OPT_CANVASMASTERDB, "CoreDB", True)
        Dim mConnectionString = ExpertChoice.Data.clsDatabaseAdvanced.GetConnectionString(CanvasMasterDBName, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient)
        PM.AntiguaDashboard.SavePanel(ECModelStorageType.mstCanvasStreamDatabase, mConnectionString, GenericDBAccess.ECGenericDatabaseAccess.DBProviderType.dbptSQLClient, App.ActiveProject.ID)
    End Sub

#Region "Tree View"

    Private Function AddTreeNodeData(tNode As clsNode) As String
        Dim sCaption = tNode.NodeName
        Dim iHasInfodoc As Integer = CInt(IIf(tNode.InfoDoc.Length > 0, 1, 0))
        Dim sInfodocIcon As String = ""

        Dim sClassName As String = "ztree_tnn znode_span"
        If tNode.RiskNodeType = RiskNodeType.ntCategory Then
            sClassName = "categorical znode_span"
        End If

        If Not tNode.Enabled Then
            sClassName = "ztree_node_disabled znode_span"
        End If

        'sInfodocIcon = "<img id='imgTreeInfodoc" + tNode.NodeGuidID.ToString.Replace("-", "") + "' src='" + ImagePath + "info12.png' width='12' height='12' title='' border='0' style='margin-right:3px;" + CStr(IIf(iHasInfodoc = 1, "", "display:none;")) + "'>"
        sInfodocIcon = "<i id='imgTreeInfodoc" + tNode.NodeGuidID.ToString.Replace("-", "") + "' class='fas fa-info-circle " + If(iHasInfodoc = 1, "toolbar-icon", "toolbar-icon-disabled") + "' style='margin-right: 3px;' onclick='setTimeout(""EditInfodoc(getSelectedNode());"",150);'></i>"

        If iHasInfodoc = 1 Then
            sClassName += " znode_has_infodoc"
        End If

        Dim sPriorities As String = GetPrioritiesString(tNode)

        If sPriorities <> "" Then sPriorities = String.Format("<span style='color:#0066cc;'>{0}</span>", sPriorities)

        Dim sName As String = sInfodocIcon + "<span class='" + sClassName + " text-overflow'>" + If(PM.Parameters.ObjectiveIndexIsVisible AndAlso tNode.NodeID > 0, String.Format("[{0}] ", tNode.NodeID), "") + SafeFormString(sCaption) + "</span>" + sPriorities
        Dim sID As String = tNode.NodeGuidID.ToString
        Dim sCanDrag As String = ""
        If tNode.ParentNode Is Nothing OrElse tNode.ParentNodesGuids Is Nothing OrElse tNode.ParentNodesGuids.Count = 0 Then sCanDrag = "drag:false,"

        Dim sNode As String = "{name:'" + JS_SafeString(sName) + "',title:'" + JS_SafeString(sCaption) + "',open:true,id:'" + sID + "',iid:" + tNode.NodeID.ToString + "," + sCanDrag + "infodoc:" + CStr(iHasInfodoc)

        Dim sChildrenList As String = ""
        If tNode.Children IsNot Nothing AndAlso tNode.Children.Count > 0 Then
            Dim sChildren As String = ""
            GetTreeNodeData(tNode.Children, sChildren)
            sChildrenList += sChildren
        End If
        sNode += If(sChildrenList <> "",  ",children:[" + sChildrenList + "]}", "}")

        Return sNode
    End Function

    Private Function GetPrioritiesString(tNode As clsNode) As String
        Dim sPriorities As String = ""

        Dim d As Integer = PM.Parameters.DecimalDigits
        Dim sf As String = "F" + CStr(d)

        Select Case PM.Parameters.Structure_DisplayPrioritiesMode
            Case 1
                sPriorities = String.Format(" [L:{0}%]", JS_SafeNumber((tNode.LocalPriorityUnnormalized(COMBINED_USER_ID) * 100).ToString(sf)))
            Case 2
                sPriorities = String.Format(" [G:{0}%]", JS_SafeNumber((tNode.WRTGlobalPriority * 100).ToString(sf)))
            Case 3
                sPriorities = String.Format(" [L:{0}%, G:{1}%]", JS_SafeNumber((tNode.LocalPriorityUnnormalized(COMBINED_USER_ID) * 100).ToString(sf)), JS_SafeNumber((tNode.WRTGlobalPriority * 100).ToString(sf)))
        End Select
        Return sPriorities
    End Function

    Private Sub GetTreeNodeData(ByVal Nodes As List(Of clsNode), ByRef retVal As String)
        If Nodes IsNot Nothing Then
            For Each tNode As clsNode In Nodes
                retVal += CStr(IIf(retVal = "", "", ",")) + AddTreeNodeData(tNode)
            Next
        End If
    End Sub

    Public Function GetTreeNodesData() As String
        Dim retVal As String = ""

        With PM
            '.CalculationsManager.GetRiskDataWRTNode(ECCore.COMBINED_USER_ID, "", HierarchyID, .Hierarchy(HierarchyID).Nodes(0).NodeGuidID, UseControlsReductionsHierarchy)
            GetTreeNodeData(.ActiveObjectives.GetLevelNodes(0), retVal)
        End With

        Return String.Format("[{0}]", retVal)
    End Function

#End Region


#Region "Export to Clipboard, Sort"
    Public Sub ExportHierarchyToText(ByVal Node As clsNode, ByRef sText As String, ByVal Offset As Integer, Optional ByVal CheckedOnly As Boolean = False)
        If Node IsNot Nothing AndAlso Not Node.IsAlternative Then
            'If Not CheckedOnly OrElse Node.IsChecked Then
            For i As Integer = 0 To Offset - 1
                sText += vbTab
            Next
            sText += Node.NodeName + vbCrLf
            'End If
            If Node.Children IsNot Nothing AndAlso Node.Children.Count > 0 Then
                For Each child In Node.Children
                    ExportHierarchyToText(child, sText, Offset + 1, CheckedOnly)
                Next
            End If
        End If
    End Sub

    Public Function ExportAlternativesToText() As String
        Dim sText As String = ""
        For Each tAlt As clsNode In PM.ActiveAlternatives.TerminalNodes'.OrderBy(Function(a) a.iIndex)
            sText += tAlt.NodeName + vbCrLf
        Next
        Return sText
    End Function

    'A1496 ===
    Private Function SortNodesBelow(ByVal ParentNodeID As Guid, tSortByName As Boolean, tSortOrder As Integer) As Boolean

        Dim retVal As Boolean = False

        Dim H As clsHierarchy = PM.ActiveObjectives
        If H IsNot Nothing Then
            Dim ParentNode As ECCore.clsNode = H.GetNodeByID(ParentNodeID)
            If ParentNode IsNot Nothing Then

                If tSortByName Then
                    Select Case tSortOrder
                        Case 0
                            ParentNode.Children.Sort(New clsNodesByNameIncreasingComparer)
                        Case 1
                            ParentNode.Children.Sort(New clsNodesByNameDecreasingComparer)
                    End Select
                Else
                    Dim nodes As New List(Of ECCore.clsNode)
                    nodes.Add(ParentNode)
                    PM.CalculationsManager.CreateCombinedJudgments(PM.CombinedGroups.GetDefaultCombinedGroup, nodes)
                    ParentNode.CalculateLocal(COMBINED_USER_ID)

                    Select Case tSortOrder
                        Case 0
                            ParentNode.Children.Sort(New clsNodesByPriorityIncreasingComparer)
                        Case 1
                            ParentNode.Children.Sort(New clsNodesByPriorityDecreasingComparer)
                    End Select
                End If

                PRJ.SaveStructure(PagePrefix + "Sort nodes below", , , String.Format("'{0}' ({1})", GetNodeTypeAndName(ParentNode), tSortOrder.ToString))   ' D3571 + D3731
                'PM.StorageManager.Writer.SaveProject(True)

                ParentNode.Judgments.ClearCombinedJudgments()

                'For Each node As ECCore.clsNode In PM.ActiveObjectives.TerminalNodes
                '    node.ClearCalculatedNodesBelow()
                '    node.Judgments.ClearCombinedJudgments()
                'Next
                retVal = True
            End If
        End If
        Return retVal
    End Function
    'A1496 ==

#End Region


#Region "Alternatives View"

    Private ReadOnly Property AttributesList As List(Of clsAttribute)
        Get
            Return PM.Attributes.AttributesList.Where(Function(a) a.Type = AttributeTypes.atAlternative AndAlso (Not a.IsDefault) AndAlso (IsMultiCategoricalAttributesAllowed OrElse a.ValueType <> AttributeValueTypes.avtEnumerationMulti)).ToList
        End Get
    End Property

    Public Function GetAttributeItemsData(attr As clsAttribute, items As clsAttributeEnumeration) As String
        Dim sItems As String = ""
        If items IsNot Nothing Then

            Dim defaultItemsGuids As New List(Of Guid)
            If attr.DefaultValue IsNot Nothing Then
                If attr.ValueType = AttributeValueTypes.avtEnumeration Then
                    If TypeOf attr.DefaultValue Is Guid Then defaultItemsGuids.Add(CType(attr.DefaultValue, Guid))
                    If TypeOf attr.DefaultValue Is String Then defaultItemsGuids.Add(New Guid(CStr(attr.DefaultValue)))
                End If
                If attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    If TypeOf attr.DefaultValue Is Guid Then defaultItemsGuids.Add(CType(attr.DefaultValue, Guid))
                    If TypeOf attr.DefaultValue Is String Then
                        Dim sGuids As String() = CStr(attr.DefaultValue).Split(CChar(";"))
                        For Each sGuid As String In sGuids
                            Dim tGuid As Guid
                            If Guid.TryParse(sGuid, tGuid) Then 
                                defaultItemsGuids.Add(tGuid)
                            End If
                        Next
                    End If
                End If
            End If

            For Each item As clsAttributeEnumerationItem In items.Items
                Dim isDefault As Boolean = defaultItemsGuids.Contains(item.ID)
                sItems += If(sItems = "", "", ",") + String.Format("['{0}','{1}',{2}]", item.ID, JS_SafeString(item.Value), Bool2Num(isDefault))
            Next
        End If
        Return sItems
    End Function

    Public Function GetAttributesData() As String
        Dim sAttrs As String = ""
        Dim attrIndex As Integer = 0
        For Each attr As clsAttribute In AttributesList
            Dim sValue As String = "''" ' enum attribute items or string/int/double/bool default value
            If attr.DefaultValue IsNot Nothing Then sValue = attr.DefaultValue.ToString()

            Select Case attr.ValueType
                Case AttributeValueTypes.avtString '0
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("'{0}'", JS_SafeString(attr.DefaultValue))
                Case AttributeValueTypes.avtBoolean '1
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", If(CBool(attr.DefaultValue), 1, 0))
                Case AttributeValueTypes.avtLong '2
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                Case AttributeValueTypes.avtDouble '3
                    If attr.DefaultValue IsNot Nothing Then sValue = String.Format("{0}", JS_SafeNumber(attr.DefaultValue))
                Case AttributeValueTypes.avtEnumeration '4
                    sValue = String.Format("[{0}]", GetAttributeItemsData(attr, PM.Attributes.GetEnumByID(attr.EnumID)))
                Case AttributeValueTypes.avtEnumerationMulti '5
                    sValue = String.Format("[{0}]", GetAttributeItemsData(attr, PM.Attributes.GetEnumByID(attr.EnumID)))
            End Select

            sAttrs += If(sAttrs = "", "", ",") + String.Format("[{0},'{1}',{2},{3},{4}]", attrIndex, JS_SafeString(App.GetAttributeName(attr)), sValue, CInt(attr.ValueType), Bool2JS(Not attr.ID.Equals(ATTRIBUTE_CONTROL_CATEGORY_ID))) ' Index, Name, Default Value (or available enum values), ValueType, Can attribute be removed
            attrIndex += 1
        Next
        Return String.Format("[{0}]", sAttrs)
    End Function

    Private Sub SaveAttributes(sComment As String)
        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        If sComment <> "" Then App.ActiveProject.SaveRA(PagePrefixAlts + ParseString("Edit %%alternative%% attributes"), , , sComment)
    End Sub

    Private Sub SaveAttributesValues(sComment As String)
        PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
        PM.ResourceAligner.Scenarios.SyncLinkedConstraintsValues(Nothing)
        If sComment <> "" Then App.ActiveProject.SaveRA("Edit/paste attributes", , , sComment)
    End Sub                 

    Public Function GetAlternativesColumns() As String
        Dim lblIndex As String = ResString("optIndexID")
        If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then lblIndex = ResString("optRank")
        If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.UniqueID Then lblIndex = ResString("optUniqueID")
        Dim retVal As String = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 0, lblIndex, "true", "right", "idx", "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 1, "Information Document", "true", "center", "infodoc", "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 2, ResString("lblAlternatives"), "true", "left", "name", "true", "true")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 3, ResString("tblAlternativeStatus"), "false", "center", "dis", "true", "true")
        If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then  ' D6798
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 3, ParseString("%%Alternative%% Type"), "true", "left", "event_type", "true", "true")
        End If
        Dim hasAlexaProsCons As Boolean = PM.ActiveAlternatives.Nodes.Where(Function (tAlt) String.Join("", tAlt.Pros(PM).Select(function(c) c.text)) + String.Join("", tAlt.Cons(PM).Select(function(c) c.text)) <> "").Count > 0
        If PM.Parameters.SpecialMode = _OPT_MODE_ALEXA_PROJECT Then ' D7485
            If hasAlexaProsCons Then
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 3, "Pros", "true", "center", "pros", "true", "false")
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 3, "Cons", "true", "center", "cons", "true", "false")
            End If        
        End If
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 3, "Color", "false", "center", "color", "false", "true")
        'retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 4, ParseString("Priority (All Participants, unnormalized)"), Bool2JS(Not PRJ.IsRisk), "right", "priority", "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 4, ResString("tblCost"), Bool2JS(False), "right", "cost", "true", "true")
        If Not PM.IsRiskProject Then 
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},-1]", 5, ResString("tblProbFailure"), Bool2JS(False), "right", "risk", "true", "true")
        End If

        Dim index As Integer = 0
        For Each attr As clsAttribute In AttributesList
            If attr.Type = AttributeTypes.atAlternative AndAlso Not attr.IsDefault Then
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7},{8}]", index + 6, JS_SafeString(App.GetAttributeName(attr)), "true", "left", "v" + index.ToString, "true", "false", CInt(attr.ValueType), index)
                index += 1
            End If
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    ReadOnly Property RA As ResourceAligner
        Get
            Return If(App.ActiveProject.ProjectManager.IsRiskProject, App.ActiveProject.ProjectManager.ResourceAlignerRisk, App.ActiveProject.ProjectManager.ResourceAligner)
        End Get
    End Property

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Dim tHid As Integer = CheckVar("hid", PM.ActiveHierarchy)
        If tHid <> PM.ActiveHierarchy Then
            PM.ActiveHierarchy = tHid
            PM.PipeParameters.CurrentParameterSet = If(tHid = ECHierarchyID.hidLikelihood, PM.PipeParameters.DefaultParameterSet, PM.PipeParameters.ImpactParameterSet)
        End If

        ' D5056 ===
        If CheckVar(_PARAM_TAB, "").ToLower = "alternatives" Then
            'ActiveView = HierarchyViews.hvAlts
            CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES
        End If
        ' D5056 ==

        'Dim tPgid As Integer = CheckVar("pgid", 0)
        'Select Case tPgid
        '    Case _PGID_STRUCTURE_SOURCES, _PGID_STRUCTURE_OBJECTIVES, _PGID_STRUCTURE_SOURCES_HIERARCHY, _PGID_STRUCTURE_OBJECTIVES_HIERARCHY
        '        NavigationPageID = tPgid
        'End Select

        RA.Load()
    End Sub

    Public Function GetNodeColor(NodeID As Integer, NodeGUID As Guid) As String
        Dim altColor As Long = CLng(PM.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, NodeGUID))
        Return If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(PM), NodeID, True))
    End Function

    Public Function GetAlternativesData() As String
        Dim sRes As String = ""
        If PM Is Nothing Then Return "[]"

        Dim AttributesList = PM.Attributes.GetAlternativesAttributes(True)

        'Dim calcTarget As clsCalculationTarget = PM.CalculationsManager.GetCalculationTargetByUserID(COMBINED_USER_ID)
        Dim wrtNode As clsNode = PM.ActiveObjectives.Nodes(0)
        'PM.CalculationsManager.Calculate(calcTarget, wrtNode)
        'Dim JA As clsJudgmentsAnalyzer = New clsJudgmentsAnalyzer(ECSynthesisMode.smDistributive, PM)
        'Dim CanShowResults = JA.CanShowIndividualResults(calcTarget.GetUserID, wrtNode)
        Dim lblYes As String = ResString("lblYes")

        For Each tAlt As clsNode In PM.ActiveAlternatives.TerminalNodes'.OrderBy(Function(a) a.iIndex)

            Dim sName As String = tAlt.NodeName

            Dim sAttrVals As String = ""
            Dim i As Integer = 0
            For Each tAttr As clsAttribute In AttributesList
                If Not tAttr.IsDefault AndAlso tAttr.Type = AttributeTypes.atAlternative AndAlso (IsMultiCategoricalAttributesAllowed OrElse tAttr.ValueType <> AttributeValueTypes.avtEnumerationMulti) Then
                    Dim sVal As String = ""
                    Dim tAttrVal As Object = PM.Attributes.GetAttributeValue(tAttr.ID, tAlt.NodeGuidID)
                    If tAttrVal IsNot Nothing Then
                        Select Case tAttr.ValueType
                            Case AttributeValueTypes.avtBoolean
                                sVal = CStr(IIf(CBool(tAttrVal), "true", "false"))
                            Case AttributeValueTypes.avtDouble
                                sVal = JS_SafeNumber(CDbl(tAttrVal))
                            Case AttributeValueTypes.avtString
                                sVal = "'" + JS_SafeString(CStr(tAttrVal)) + "'"
                            Case AttributeValueTypes.avtLong
                                sVal = JS_SafeNumber(CLng(tAttrVal))
                            Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                                sVal = "'" + JS_SafeString(PM.Attributes.GetAttributeValueString(tAttr.ID, tAlt.NodeGuidID)) + "'"
                                sAttrVals += String.Format(",'vguid{0}':'{1}'", i, if (sVal = "", "", tAttrVal.ToString))
                        End Select
                    End If
                    sAttrVals += String.Format(",'v{0}':{1}", i, if (sVal = "", "''", sVal))
                    i += 1
                End If
            Next

            Dim tDefaultScenarioCost As Double = UNDEFINED_INTEGER_VALUE
            Dim tDefaultScenarioRisk As Double = UNDEFINED_INTEGER_VALUE

            Dim tRAAlternative As RAAlternative = PM.ResourceAligner.Scenarios.Scenarios(0).GetAvailableAlternativeById(tAlt.NodeGuidID.ToString)
            If tRAAlternative IsNot Nothing Then 
                tDefaultScenarioCost = tRAAlternative.Cost
                tDefaultScenarioRisk = tRAAlternative.RiskOriginal
            End If

            'sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("{{""iid"":{0},""guid"":""{1}"",""idx"":""{2}"",""vidx"":{3},""name"":""{4}"",""infodoc"":""{5}"",""priority"":{6},""disabled"":{7},""cost"":{8},""risk"":{9}{10}}}", talt.NodeID, talt.NodeGuidID.ToString, tAlt.Index, 0, JS_SafeString(tAlt.NodeName), If(tAlt.InfoDoc <> "", lblYes, ""), If(CanShowResults, JS_SafeNumber(Math.Round(tAlt.UnnormalizedPriority * 100, 2)), UNDEFINED_INTEGER_VALUE.ToString), Bool2JS(Not tAlt.Enabled), If(tDefaultScenarioCost = UNDEFINED_INTEGER_VALUE, "''", JS_SafeString(tDefaultScenarioCost)), If(tDefaultScenarioRisk = UNDEFINED_INTEGER_VALUE, "''", JS_SafeString(tDefaultScenarioRisk)), sAttrVals)
            sRes += CStr(IIf(sRes = "", "", ",")) + String.Format("{{""iid"":{0},""guid"":""{1}"",""idx"":""{2}"",""vidx"":{3},""name"":""{4}"",""infodoc"":""{5}"",""priority"":{6},""dis"":{7},""cost"":{8},""risk"":{9},""color"":""{10}"",""event_type"":{11},""pros"":""{12}"",""cons"":""{13}""{14}}}", talt.NodeID, talt.NodeGuidID.ToString, tAlt.Index, 0, JS_SafeString(tAlt.NodeName), If(tAlt.InfoDoc <> "", lblYes, ""), 100, Bool2JS(Not tAlt.Enabled), If(tDefaultScenarioCost = UNDEFINED_INTEGER_VALUE, "''", JS_SafeString(tDefaultScenarioCost)), If(tDefaultScenarioRisk = UNDEFINED_INTEGER_VALUE, "''", JS_SafeString(tDefaultScenarioRisk)), GetNodeColor(tAlt.NodeID, tAlt.NodeGuidID), CInt(tAlt.EventType), JS_SafeString(String.Join(", ", tAlt.Pros(PM).Select(function(c) c.text))), JS_SafeString(String.Join(", ", tAlt.Cons(PM).Select(function(c) c.text))), sAttrVals)
        Next

        Return String.Format("[{0}]", sRes)
    End Function

    Public Function SortAlternatives(tSortByName As Boolean, tSortOrder As Integer) As Boolean

        Dim retVal As Boolean = False

        Dim AH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            If AH IsNot Nothing Then
                If tSortByName Then
                    Select Case tSortOrder
                        Case 0
                            AH.Nodes.Sort(New clsNodesByNameIncreasingComparer)
                        Case 1
                            AH.Nodes.Sort(New clsNodesByNameDecreasingComparer)
                    End Select
                Else
                    If Not PM.IsRiskProject Then
                        Dim CG As ECCore.clsCombinedGroup = PM.CombinedGroups.GetDefaultCombinedGroup
                        Dim CalcTarget As clsCalculationTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                        PM.CalculationsManager.Calculate(CalcTarget, PM.ActiveObjectives.Nodes(0), PM.ActiveHierarchy, PM.ActiveAltsHierarchy)

                        Select Case tSortOrder
                            Case 0
                                AH.Nodes.Sort(New clsAlternativesByPriorityIncreasingComparer)
                            Case 1
                                AH.Nodes.Sort(New clsAlternativesByPriorityDecreasingComparer)
                        End Select
                    Else
                        Dim tAltsList = PM.CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", Guid.Empty, ControlsUsageMode.DoNotUse) 'A1064
                        If tAltsList IsNot Nothing AndAlso AH IsNot Nothing AndAlso AH.Nodes IsNot Nothing Then
                            For Each node In AH.Nodes
                                node.RiskValue = 0
                            Next
                            For Each alt In tAltsList.AlternativesData
                                Dim node As ECCore.clsNode = AH.GetNodeByID(alt.intID)
                                If node IsNot Nothing Then node.RiskValue = alt.RiskValue
                            Next
                            Select Case tSortOrder
                                Case 0
                                    AH.Nodes.Sort(New clsAlternativesByRiskIncreasingComparer)
                                Case 1
                                    AH.Nodes.Sort(New clsAlternativesByRiskDecreasingComparer)
                            End Select
                        End If
                    End If
                End If
                PRJ.SaveStructure("Sort alternatives", , , String.Format("Order: {0}; by name: {1})", tSortOrder, tSortByName.ToString))   ' D3571 + D3731
                'ActiveProject.ProjectManager.StorageManager.Writer.SaveProject(True)

                For Each node As ECCore.clsNode In PM.ActiveObjectives.TerminalNodes
                node.Judgments.ClearCombinedJudgments()
            Next

                retVal = True
            End If
        Return retVal
    End Function

#End Region

    ' D3392 ===
    Public Function CopyProjectAndConvertFromCH(ByVal sNewPrjName As String, ByVal SourceProjectID As Integer, HID As ECHierarchyID, fCopyJudgments As Boolean, ByRef sError As String) As Integer   ' D3489 + D3492
        Dim WebCore = App
        Dim tRes As Integer
        If WebCore.CanUserDoAction(ecActionType.at_alCreateNewModel, WebCore.ActiveUserWorkgroup, WebCore.ActiveWorkgroup) Then
            If WebCore.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxLifetimeProjects, Nothing, False) Then
                If WebCore.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsTotal, Nothing, False) Then
                    Dim tSrcPrj As clsProject = clsProject.ProjectByID(SourceProjectID, WebCore.ActiveProjectsList) ' D3439
                    If tSrcPrj Is Nothing Then tSrcPrj = WebCore.DBProjectByID(SourceProjectID) ' D3439
                    Dim tDestPrj As Integer = CopyProject(sNewPrjName, SourceProjectID, ecProjectStatus.psActive, True, Nothing, Nothing, Nothing, False, False, -1, CType(IIf(tSrcPrj IsNot Nothing, tSrcPrj.RiskionProjectType, CanvasTypes.ProjectType.ptRegular), ProjectType), True) ' D3439 + A1205
                    If tDestPrj >= 0 Then
                        Dim tNewPrj As clsProject = WebCore.DBProjectByID(tDestPrj)
                        If tNewPrj IsNot Nothing Then
                            'If tNewPrj.isOnline AndAlso Not WebCore.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxProjectsOnline, Nothing, False) Then tNewPrj.isOnline = False

                            tNewPrj.ProjectManager.Hierarchy(HID).ConvertToIncompleteHierachy(fCopyJudgments)     ' D3489 + D3492

                            If WebCore.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxObjectives, tNewPrj, True) AndAlso WebCore.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxAlternatives, tNewPrj, True) AndAlso WebCore.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.MaxLevelsBelowGoal, tNewPrj, True) Then
                                tNewPrj.ProjectManager.StorageManager.Writer.SaveProject(False)
                                tRes = tNewPrj.ID
                            Else
                                WebCore.DBProjectDelete(tNewPrj, True)
                                sError = "You can't convert this project due to over license limits for Max Objectives count"
                            End If

                        Else
                            sError = "Specified project can't be found"
                        End If
                    Else
                        sError = "Can't copy project"
                    End If
                Else
                    sError = WebCore.LicenseErrorMessage(WebCore.ActiveWorkgroup.License, ecLicenseParameter.MaxProjectsTotal, True)
                End If
            Else
                sError = WebCore.LicenseErrorMessage(WebCore.ActiveWorkgroup.License, ecLicenseParameter.MaxLifetimeProjects, True)
            End If
        Else
            sError = "You don't have permissions for create model"
        End If

        Return tRes
    End Function
    ' D3392 ==

    Public Function CopyProject(ByVal sTitle As String, ByVal SourceProjectID As Integer, DestProjectStatus As ecProjectStatus, ByVal fCopyUsers As Boolean, sObjCleanUpName As String, sAltCleanUpName As String, sObjImpactCleanUpName As String, ByVal fPaticipantsEmailCleanUp As Boolean, ByVal fPaticipantsNameCleanUp As Boolean, CopyMinorVersion As Integer, ByVal fProjectType As ProjectType, CopySnapshots As Boolean) As Integer  ' D0760 + D1182 + D1186 + D2145 + A1010 + D3432 + D3945
        Dim WebCore = App
        Dim tPrjID As Integer = -1 ' D0719
        Dim tSourcePrj As clsProject = WebCore.DBProjectByID(SourceProjectID)
        If tSourcePrj IsNot Nothing AndAlso (tSourcePrj.isValidDBVersion OrElse tSourcePrj.isDBVersionCanBeUpdated) Then ' D1627 + D1995
            Dim tPrj As clsProject = New clsProject(WebCore.Options.ProjectLoadOnDemand, WebCore.Options.ProjectForceAllowedAlts, WebCore.ActiveUser.UserEmail, tSourcePrj.IsRisk, AddressOf WebCore.onProjectSavingEvent, AddressOf WebCore.onProjectSavedEvent, WebCore.Options.ProjectUseDataMapping, AddressOf WebCore.onProjectUpdateLastModifyEvent)   ' D0714 + D0719 + D2255 + D3571 + D4465 + D4535
            tPrj.Created = DateTime.Now ' D0821
            tPrj.ProjectName = sTitle
            tPrj.ProviderType = WebCore.DefaultProvider
            tPrj.WorkgroupID = WebCore.ActiveWorkgroup.ID
            tPrj.OwnerID = WebCore.ActiveUser.UserID
            tPrj.PasscodeLikelihood = WebCore.ProjectUniquePasscode("", -1) ' D1709
            tPrj.PasscodeImpact = WebCore.ProjectUniquePasscode("", -1)     ' D1709
            tPrj.isOnline = False   ' D0748
            tPrj.isPublic = True    ' D3021
            tPrj.Comment = tSourcePrj.Comment   ' D1727            

            ' D4040 ===
            Dim fSaveasTemplate As Boolean = DestProjectStatus = ecProjectStatus.psMasterProject OrElse DestProjectStatus = ecProjectStatus.psTemplate  ' D2490

            ' D0720 ===
            Dim fNeedUpdateProject As Boolean = False
            If Not fSaveasTemplate AndAlso tPrj.ProjectStatus <> ecProjectStatus.psActive Then
                tPrj.ProjectStatus = ecProjectStatus.psActive
                fNeedUpdateProject = True
            End If
            ' D0720 ==

            ' D3451 ===
            If tPrj.ProjectStatus <> DestProjectStatus OrElse tPrj.RiskionProjectType <> fProjectType Then
                tPrj.ProjectStatus = DestProjectStatus  ' D2490
                tPrj.RiskionProjectType = fProjectType
                fNeedUpdateProject = True
            End If
            ' D3451 + D4040 ==

            If WebCore.DBProjectCreate(tPrj, CStr(IIf(tSourcePrj.ProjectStatus = ecProjectStatus.psTemplate OrElse tSourcePrj.ProjectStatus = ecProjectStatus.psMasterProject, String.Format("Create project from {0} '{1}'", IIf(tSourcePrj.ProjectStatus = ecProjectStatus.psMasterProject, "Default option sets", "Template"), ShortString(tSourcePrj.ProjectName, 45)), "Copy decision from '" + tSourcePrj.ProjectName + "'"))) Then ' D2479 + D4123

                'Dim fCopySnapshots As Boolean = DestProjectStatus = ecProjectStatus.psActive AndAlso fCopyUsers    ' D3774 - D3945
                If DestProjectStatus <> ecProjectStatus.psActive OrElse Not fCopyUsers OrElse sObjCleanUpName <> "" OrElse sAltCleanUpName <> "" OrElse sObjImpactCleanUpName <> "" OrElse fPaticipantsEmailCleanUp OrElse fPaticipantsNameCleanUp Then CopySnapshots = False ' D3945

                If WebCore.DBProjectCopy(tSourcePrj, ECModelStorageType.mstCanvasStreamDatabase, tPrj.ConnectionString, tPrj.ProviderType, tPrj.ID, CopySnapshots, sObjCleanUpName, sAltCleanUpName, sObjImpactCleanUpName, fPaticipantsEmailCleanUp, fPaticipantsNameCleanUp, CopyMinorVersion) Then  ' D0760 + D1182 + D1186 + D2145 + D3432 + D3774 + D3945
                    tPrj.CheckGUID()    ' D3687
                    tPrj.ResetProject()

                    If fSaveasTemplate OrElse Not fCopyUsers OrElse (tPrj.ProjectStatus = ecProjectStatus.psMasterProject OrElse tPrj.ProjectStatus = ecProjectStatus.psTemplate) Then   ' D2544
                        Dim UsersOrig As List(Of ECTypes.clsUser) = tPrj.ProjectManager.UsersList
                        Dim Users As New List(Of ECTypes.clsUser)
                        Users.AddRange(UsersOrig.ToArray)
                        For Each tUser As ECTypes.clsUser In Users
                            tPrj.ProjectManager.DeleteUser(tUser.UserEMail, False)
                        Next
                        tPrj.SaveStructure(, , False)    ' D4562

                        ' D2490 ===
                        If DestProjectStatus = ecProjectStatus.psMasterProject Then
                            For Each tHier As clsHierarchy In tPrj.ProjectManager.Hierarchies
                                If tHier.HierarchyType = ECHierarchyType.htModel AndAlso tHier.Nodes.Count > 1 Then
                                    Dim tGoal As ECCore.clsNode = tHier.Nodes(0)
                                    For i As Integer = tGoal.Children.Count - 1 To 0 Step -1
                                        tHier.DeleteNode(tGoal.Children(i), False)
                                        fNeedUpdateProject = True
                                    Next
                                End If
                            Next
                            If tPrj.HierarchyAlternatives.Nodes.Count > 0 Then
                                For i As Integer = tPrj.HierarchyAlternatives.Nodes.Count - 1 To 0 Step -1
                                    tPrj.HierarchyAlternatives.DeleteNode(tPrj.HierarchyAlternatives.Nodes(i), False)
                                    fNeedUpdateProject = True
                                Next
                            End If
                            If fNeedUpdateProject Then tPrj.SaveStructure(, , False) ' D3731
                        End If
                        ' D2490 ==

                        If fSaveasTemplate Then
                            tPrj.isOnline = False   ' D0748
                            tPrj.isPublic = False   ' D0748
                            fNeedUpdateProject = True
                        End If

                        WebCore.AttachProject(WebCore.ActiveUser, tPrj, False, WebCore.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager), "", False)  ' D1114 + D2287 + D2644 + D2780

                    Else

                        If fPaticipantsEmailCleanUp Then    ' D0760 + D1182 + D1186

                            WebCore.ImportProjectUsers(tPrj, WebCore.ActiveUser, True, False)   ' D0760

                        Else

                            tPrj.Created = tSourcePrj.Created   ' D0821
                            tPrj.LastModify = tSourcePrj.LastModify ' D0821
                            fNeedUpdateProject = True   ' D0821

                            ' copy participants
                            For Each tWS As clsWorkspace In WebCore.DBWorkspacesByProjectID(SourceProjectID)
                                If tWS.UserID = WebCore.ActiveUser.UserID Or fCopyUsers Then    ' D0714
                                    Dim tNewWS As clsWorkspace = tWS.Clone
                                    tNewWS.ProjectID = tPrj.ID
                                    Dim sMsg As String = String.Format("Copy user to project '{0}'", tPrj.Passcode)
                                    If tWS.UserID = WebCore.ActiveUser.UserID Then
                                        tNewWS.GroupID = WebCore.ActiveWorkgroup.GetDefaultRoleGroupID(ecRoleLevel.rlModelLevel, ecRoleGroupType.gtProjectManager)  ' D2780
                                        sMsg = String.Format("Attach Project Owner '{0}' to project '{1}'", WebCore.ActiveUser.UserEmail, tPrj.Passcode)
                                    End If
                                    WebCore.DBWorkspaceUpdate(tNewWS, True, sMsg)    ' D0479
                                    WebCore.DBSaveLog(dbActionType.actCreate, dbObjectType.einfWorkspace, tNewWS.ID, sMsg, "", WebCore.ActiveUser.UserID, WebCore.ActiveWorkgroup.ID)
                                End If
                            Next
                        End If

                    End If

                    If fNeedUpdateProject Then WebCore.DBProjectUpdate(tPrj, False, "Update project after copy") ' D0721

                    'A1010 ===
                    If tPrj.ProjectManager.PipeParameters.ProjectType <> fProjectType Then
                        tPrj.ProjectManager.PipeParameters.ProjectType = fProjectType
                        tPrj.SaveProjectOptions("Init on copy project", False, False)   ' D3571
                        'tPrj.ProjectManager.SavePipeParameters(PipeStorageType.pstStreamsDatabase, tPrj.ProjectManager.StorageManager.ModelID)
                    End If
                    'A1010 ==

                    ' D0719 ===
                    WebCore.ActiveProjectsList = Nothing
                    'tPrj.ResetProject()   ' D4562
                    WebCore.Workspaces = Nothing
                    tPrjID = tPrj.ID

                    ' D1114 ==
                Else
                    WebCore.DBProjectDelete(tPrj, True) ' can't copy
                End If
            End If
        End If

        Return tPrjID
    End Function
    ' D0713 ==

End Class