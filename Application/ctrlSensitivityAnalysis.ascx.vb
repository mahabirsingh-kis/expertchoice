
Namespace ExpertChoice.Web.Controls

    Partial Public Class ctrlSensitivityAnalysis

        Inherits UserControl

        Public Const ACTION_DSA_UPDATE_VALUES As String = "dsa_update_values"
        Public Const ACTION_DSA_RESET As String = "dsa_reset"
        Public Const ACTION_SA_WRT_NODE_ID As String = "sa_wrt_node_id"

        Public msgNoEvaluationData As String = "no data for {0}"
        Public msgNoGroupData As String = "no group data"

        ' -D3778
        'Public msgSeeingCombined As String = "combined"
        'Public msgSeeingIndividual As String = "individual"
        'Public msgSeeingUser As String = "user"
        'Public msgHint As String = "drag bars"

        Public lblNormalization As String = "Normaization: "
        Public lblSelectNode As String = "Select: "
        Public lblRefreshCaption As String = "Refresh"
        Public lblKeepSortedAlts As String = "Keep sorted"  ' D3477
        Public lblShowLines As String = "Show lines"        ' D3477
        Public lblLineUp As String = "Line up"              ' D3719
        Public lblShowLegend As String = "Show Legend"      ' D3481

        Public pnlLoadingID As String = ""

        Public ProjectManager As clsProjectManager = Nothing
        Public NormalizationsList As New Dictionary(Of AlternativeNormalizationOptions, String)

        Public Opt_ShowMaxAltsCount As Integer = -1
        'Public Opt_ShowYouAreSeeing As Boolean = True  ' -D3778
        Public Opt_isMobile As Boolean = False
        Public opt_ShowNormalization As Boolean = True

        Public CanSeeResults As Boolean = False     ' D3857

        Private _Current_UserID As Integer = Integer.MinValue
        Private _SA_UserID As Integer = Integer.MinValue
        Private _SA_Data As clsSensitivityAnalysisActionData = Nothing  ' D2987
        Private _NormalizationMode As AlternativeNormalizationOptions = AlternativeNormalizationOptions.anoPercentOfMax  ' D2114
        Private _SortMode As Integer = 1
        Private _NodesList As Dictionary(Of Integer, String) = Nothing

        Public Const _SESS_SA_NORMALIZATION As String = "SANormMode"    ' D3473
        Public Const _SESS_SA_WRT_NODE As String = "SAWrtNode"          ' D3473
        Public Const _SESS_SA_SORT As String = "SASortMode"
        Public Const _OPT_IGNORE_CATEGORIES As Boolean = True           ' D2686


        ' D2987 ===
        Public Property Data As clsSensitivityAnalysisActionData
            Get
                Return _SA_Data
            End Get
            Set(value As clsSensitivityAnalysisActionData)
                _SA_Data = value
            End Set
        End Property
        ' D2987 ==

        ' D2988 ===
        Public ReadOnly Property SAType As SAType
            Get
                If Data Is Nothing Then Return SAType.satNone Else Return Data.SAType
            End Get
        End Property
        ' D2988 ==

        ' D3473 ===
        Public Function ModelID() As String
            If ProjectManager Is Nothing Then Return "" Else Return String.Format("_{0}", ProjectManager.StorageManager.ModelID)
        End Function
        ' D3473 ==

        ' D2114 ===
        Public Property NormalizationMode() As AlternativeNormalizationOptions
            Get
                If Session(_SESS_SA_NORMALIZATION + ModelID()) IsNot Nothing Then
                    _NormalizationMode = CType(Session(_SESS_SA_NORMALIZATION + ModelID()), AlternativeNormalizationOptions)
                End If
                Return _NormalizationMode
            End Get
            Set(value As AlternativeNormalizationOptions)
                _NormalizationMode = value
                Session.Remove(_SESS_SA_NORMALIZATION + ModelID())
                Session.Add(_SESS_SA_NORMALIZATION + ModelID(), _NormalizationMode)
            End Set
        End Property
        ' D2114 ==

        Public Property SortMode() As Integer
            Get
                If Session(_SESS_SA_SORT + ModelID()) IsNot Nothing Then
                    _SortMode = CType(Session(_SESS_SA_SORT + ModelID()), Integer)
                End If
                Return _SortMode
            End Get
            Set(value As Integer)
                _SortMode = value
                Session.Remove(_SESS_SA_SORT + ModelID())
                Session.Add(_SESS_SA_SORT + ModelID(), _SortMode)
            End Set
        End Property

        Public Property CurrentNode As clsNode
            Get
                ' D3473 ===
                Dim NodeID As Integer = -1
                If Session(_SESS_SA_WRT_NODE + ModelID()) IsNot Nothing Then
                    NodeID = CInt(Session(_SESS_SA_WRT_NODE + ModelID()))
                End If
                Dim tNode As clsNode = Nothing
                If ProjectManager IsNot Nothing Then
                    tNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(NodeID)
                    If tNode Is Nothing AndAlso ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes.Count > 0 Then tNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)
                End If
                ' D3473 ==
                Return tNode
            End Get
            Set(value As clsNode)
                If ProjectManager IsNot Nothing AndAlso value IsNot Nothing Then
                    Session.Remove(_SESS_SA_WRT_NODE + ModelID())   ' D3473
                    Session.Add(_SESS_SA_WRT_NODE + ModelID(), value.NodeID)    ' D3473
                End If
            End Set
        End Property

        Public Property SAUserID() As Integer
            Get
                If _SA_UserID = Integer.MinValue AndAlso ProjectManager IsNot Nothing Then
                    _SA_UserID = ProjectManager.UserID
                End If
                Return _SA_UserID
            End Get
            Set(value As Integer)
                _SA_UserID = value
            End Set
        End Property

        Public Property CurrentUserID() As Integer
            Get
                If _Current_UserID = Integer.MinValue AndAlso ProjectManager IsNot Nothing Then
                    _Current_UserID = ProjectManager.UserID
                End If
                Return _Current_UserID
            End Get
            Set(value As Integer)
                _Current_UserID = value
            End Set
        End Property
        ' D0374 ==

        Private Sub GetOverTerminalNodes(ByRef tNodesList As Dictionary(Of Integer, String), pNode As clsNode, margin As String)  ' D3000
            If pNode IsNot Nothing AndAlso Not pNode.IsTerminalNode AndAlso Not pNode.DisabledForUser(CurrentUserID) Then    ' D1905 + D2686
                'Dim fIgnoreCategories As Boolean = _OPT_IGNORE_CATEGORIES AndAlso Project.IsRisk  ' D2686
                'If (Not fIgnoreCategories OrElse pNode.RiskNodeType <> RiskNodeType.ntCategory) Then tNodesList.Add(pNode) ' D2686
                tNodesList.Add(pNode.NodeID, ShortString(CStr(IIf(margin = "", "", margin.Replace(" ", "&nbsp;") + "• ")) + SafeFormString(pNode.NodeName), 50 - margin.Length))    ' D3474
                For Each cNode As clsNode In pNode.GetNodesBelow(SAUserID)  ' D3000
                    GetOverTerminalNodes(tNodesList, cNode, margin + " ")   ' D3474
                Next
            End If
        End Sub

        Private Sub GetNodesWithParents(ByRef tNodesList As Dictionary(Of Integer, String), pNode As clsNode, margin As String)   ' D3000
            Dim fIgnoreCategories As Boolean = _OPT_IGNORE_CATEGORIES AndAlso ProjectManager.IsRiskProject  ' D2686
            For Each cNode As clsNode In pNode.GetNodesBelow(SAUserID)  ' D3000
                If Not cNode.DisabledForUser(CurrentUserID) AndAlso Not cNode.IsAlternative Then ' D1905
                    If (Not fIgnoreCategories OrElse cNode.RiskNodeType <> RiskNodeType.ntCategory) Then
                        tNodesList.Add(cNode.NodeID, margin + cNode.NodeName)
                    End If
                    If cNode.GetNodesBelow(SAUserID) IsNot Nothing Then GetNodesWithParents(tNodesList, cNode, margin + " • ")
                End If
            Next
        End Sub

        Public ReadOnly Property NodesList As Dictionary(Of Integer, String)
            Get
                If _NodesList Is Nothing Then
                    _NodesList = New Dictionary(Of Integer, String)
                    Dim tRoot As clsNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).Nodes(0)
                    Select Case SAType
                        'Case Canvas.SAType.satGradient
                        '    GetNodesWithParents(_NodesList, tRoot, " ")
                        Case Else
                            GetOverTerminalNodes(_NodesList, tRoot, "")
                    End Select
                End If
                Return _NodesList
            End Get
        End Property

        Public Function GetNodesList() As String
            Dim sList As String = ""
            If NodesList IsNot Nothing AndAlso NodesList.Count > 1 Then
                For Each ID As Integer In NodesList.Keys
                    Dim sActive As String = ""
                    If CurrentNode IsNot Nothing AndAlso CurrentNode.NodeID = ID Then sActive = " selected"
                    sList += String.Format("<option value='{0}'{1}>{2}</option>", ID, sActive, NodesList(ID))    ' D3473
                Next
                sList = String.Format("{0} <select id='nodes' onchange='onChangeNode(this.value);' style='width:150px'>{1}</select>", lblSelectNode, sList)
            End If
            Return sList
        End Function

        Public Function GetGSASubobjectives() As String
            Dim retVal As String = ""

            Dim wrtNode As clsNode = CurrentNode
            If wrtNode IsNot Nothing Then
                Dim i = 0
                For Each obj As clsNode In wrtNode.GetNodesBelow(SAUserID)
                    Dim sActive As String = ""
                    retVal += String.Format("<option value='{0}'{1}>{2}</option>", i, sActive, obj.NodeName)
                    i += 1
                Next
            End If
            Return retVal
        End Function

        Public Function GetNormalizationList() As String
            Dim sList As String = ""
            If NormalizationsList IsNot Nothing AndAlso NormalizationsList.Count > 1 Then
                For Each ID As AlternativeNormalizationOptions In NormalizationsList.Keys
                    Dim sActive As String = ""
                    'If NormalizationMode = ID Then sActive = " selected"
                    sList += String.Format("<option value='{0}'{1}>{2}</option>", CInt(ID), sActive, SafeFormString(NormalizationsList(ID)))
                Next
                sList = String.Format("<div><select id='norm_mode' onchange='onChangeNormalization(this.value);' style='width:150px'>{0}</select></div>", sList)
            End If
            Return sList
        End Function

        Public Function GetSortList() As String
            Dim sList As String = ""
            Dim SortList As New List(Of String)
            SortList.Add("No Sorting")
            SortList.Add("Sort by Priority")
            SortList.Add("Sort by Name")

            For i As Integer = 0 To 2
                Dim sActive As String = ""
                If SortMode = i Then sActive = " selected"
                sList += String.Format("<option value='{0}'{1}>{2}</option>", i, sActive, SortList(i))
            Next
            sList = String.Format("<div><select id='sort_mode' onchange='onChangeSorting(this.value);' style='width:150px'>{0}</select></div>", sList)
            Return sList
        End Function
        ' D3477 ===
        Public Function GetOptions() As String
            Dim sRes As String = ""

            If SAType = SAType.satDynamic Then
                If opt_ShowNormalization Then sRes += "<br/>" + GetNormalizationList() + "<br/>" + GetSortList() ' D7234
                'sRes += String.Format("<br/><div><label><input type=checkbox class='checkbox' name='cbKeepSorted' value='1' onclick='onKeepSorted(this.checked);' {0}>{1}</label></div>", "", lblKeepSortedAlts)
            End If

            If SAType = SAType.satPerformance Then
                If opt_ShowNormalization Then sRes += "<br/>" + GetNormalizationList() + "<br/>"    ' D7234
                sRes += String.Format("<br/><div><label><input type=checkbox class='checkbox' name='cbShowLines' value='1' onclick='onShowLines(this.checked);' {0}>{1}</label></div>", "checked", lblShowLines)
                sRes += String.Format("<br/><div><label><input type=checkbox class='checkbox' name='cbLineUp' value='1' onclick='onLineUp(this.checked);' {0}>{1}</label></div>", "checked", lblLineUp)   ' D3719
            End If

            ' D3481 ===
            If SAType = SAType.satGradient Then
                If opt_ShowNormalization Then sRes += "<br/>" + GetNormalizationList() + "<br/>" + GetSortList()    ' D7234
                sRes += String.Format("<br/><div><label><input type=checkbox class='checkbox' name='cbShowLegend' value='1' onclick='onShowLegend(this.checked);' {0}>{1}</label></div>", "checked", lblShowLegend)
            End If
            ' D3481 ==

            Return sRes
        End Function
        ' D3477 ==

        Protected Sub InitComponent(sender As Object, e As EventArgs) Handles Me.Init ' D3857
            Dim sMessage As String = ""
            If ProjectManager IsNot Nothing Then

                If NodesList.Count > 0 Then ' D0520

                    Dim fIsCombined As Boolean = IsCombinedUserID(SAUserID)
                    Dim fCanShowIndividual As Boolean = False
                    Dim fCanShowGlobal As Boolean = False

                    Dim PrjAnalyzer As New clsJudgmentsAnalyzer(ProjectManager.PipeParameters.SynthesisMode, ProjectManager)
                    If Not fIsCombined Then
                        fCanShowIndividual = PrjAnalyzer.CanShowIndividualResults(SAUserID, CurrentNode)
                    Else
                        fCanShowGlobal = PrjAnalyzer.CanShowGroupResults(CurrentNode)
                    End If
                    PrjAnalyzer = Nothing

                    If (fIsCombined AndAlso Not fCanShowGlobal) Or (Not fIsCombined AndAlso Not fCanShowIndividual) Then 'C0565 + D0521
                        sMessage = msgNoGroupData
                    Else
                        If CurrentNode IsNot Nothing Then  ' D0373
                            If Not fIsCombined AndAlso Not fCanShowIndividual Then  ' D3473
                                sMessage = String.Format(msgNoEvaluationData, CurrentNode.NodeName)
                            End If

                            ' -D3778
                            '' D0374 ===
                            'If Opt_ShowYouAreSeeing Then
                            '    Dim sSeeing As String = ""
                            '    Select Case SAUserID
                            '        Case COMBINED_USER_ID
                            '            sSeeing = msgSeeingCombined
                            '        Case CurrentUserID
                            '            sSeeing = msgSeeingIndividual
                            '        Case Else
                            '            If SAUserID <> Integer.MinValue AndAlso ProjectManager.User IsNot Nothing Then
                            '                ' D0401 ===
                            '                Dim sUserEmail As String = ""
                            '                Dim tUser As clsUser = ProjectManager.GetUserByID(SAUserID)
                            '                If Not tUser Is Nothing Then sUserEmail = tUser.UserEMail
                            '                sSeeing = String.Format(msgSeeingUser, sUserEmail)
                            '                ' D0401 ==
                            '            End If
                            '    End Select
                            '    If sSeeing <> "" Then lblSeeing.Text = sSeeing Else Opt_ShowYouAreSeeing = False
                            'End If
                            '' D0374 ==
                        End If
                        ' D0078 ==
                    End If
                    ' D0520 ===
                Else
                    sMessage = msgNoGroupData
                End If

                lblMessage.Text = sMessage
                If sMessage <> "" Then lblMessage.Visible = True
                ' D0520 ==
                CanSeeResults = sMessage = ""    ' D3857
            End If
            Ajax_Callback(Request.Form.ToString)    ' D3857
        End Sub

        Protected Sub Ajax_Callback(data As String)
            Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
            Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, _PARAM_ACTION)).Trim.ToLower   ' Anti-XSS
            Dim sResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

            Select Case sAction

                Case ACTION_SA_WRT_NODE_ID
                    Dim tNodeID As Integer = -1
                    If Integer.TryParse(GetParam(args, "node_id").ToLower, tNodeID) Then
                        CurrentNode = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy).GetNodeByID(tNodeID) ' D3473
                        sResult = GetSAData()
                    End If
                Case "normalization"
                    Dim tID As Integer = -1
                    If Integer.TryParse(GetParam(args, "norm_mode").ToLower, tID) Then
                        NormalizationMode = CType(tID, AlternativeNormalizationOptions)
                        sResult = GetSAData()
                    End If
                Case ACTION_DSA_UPDATE_VALUES
                    Dim s_values As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "values")).Trim() ' Anti-XSS
                    Dim values() As String = s_values.Split(CChar(","))
                    Dim s_ids As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "objids")).Trim()    ' Anti-XSS
                    Dim ids() As String = s_ids.Split(CChar(","))
                    Dim ANewObjPriorities As New Dictionary(Of Integer, Double)
                    Dim i = 0
                    For Each objID In ids
                        Dim APrty As Double = 0
                        String2Double(values(i), APrty)
                        ANewObjPriorities.Add(CInt(objID), APrty)
                        i += 1
                    Next
                    updateAltValuesinZero(ANewObjPriorities)
                    Dim ZeroValuesString As String = ""
                    For Each Objitem In AltValuesInZero
                        Dim ZeroAltValuesString As String = ""
                        For Each AltItem In Objitem.Value
                            ZeroAltValuesString += CStr(IIf(ZeroAltValuesString <> "", ",", "")) + String.Format("{{altID:{0},val:{1}}}", AltItem.Key, JS_SafeNumber(AltItem.Value))
                        Next
                        ZeroValuesString += CStr(IIf(ZeroValuesString <> "", ",", "")) + String.Format("[{0},[{1}]]", Objitem.Key, ZeroAltValuesString)
                    Next
                    sResult = String.Format("[[{0}]]", ZeroValuesString)
            End Select

            If sResult <> "" Then
                Response.Clear()
                Response.ContentType = "text/plain"
                Response.Write(sResult)
                Response.End()
            End If

        End Sub

        Function GetSATypeString() As String
            Select Case SAType
                Case SAType.satDynamic
                    Return "'DSA'"
                Case SAType.satGradient
                    Return "'GSA'"
                Case SAType.satPerformance
                    Return "'PSA'"
            End Select
            Return "'None'"
        End Function

        Public Function GetSAData() As String
            Return String.Format("[{0}, {1}]", GetSAObjectives, GetSAAlternatives)
        End Function

#Region "SA"
        'Data example for sa plugin
        'alts: [{ id: 0, name: 'Alt1', value: 0.7, initValue: 0.7, color: '#95c5f0' },
        '       { id: 1, name: 'Alt2', value: 0.3, initValue: 0.3, color: '#fa7000'}],
        'objs: [{ id: 1, name: 'Obj1', value: 0.3, initValue: 0.3, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}], gradientInitMinValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.8}] },
        '   { id: 2, name: 'Obj2', value: 0.5, initValue: 0.5, gradientMaxValues: [{ altID: 0, val: 0.3 }, { altID: 1, val: 0.5}], gradientMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}], gradientInitMinValues: [{ altID: 0, val: 0.8 }, { altID: 1, val: 0.7}] },
        '   { id: 3, name: 'Obj3', value: 0.2, initValue: 0.2, gradientMaxValues: [{ altID: 0, val: 0.1 }, { altID: 1, val: 0.3}], gradientMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}], gradientInitMinValues: [{ altID: 0, val: 0.6 }, { altID: 1, val: 0.1}]}]
        Dim AltColors As String() = {"#344d94", "#cb3034", "#9d27a8", "#e3e112", "#00687d", "#407000", "#f24961", "#663d2e", "#9600fa", "#ffbde6", "#00c49f", "#7280c4", "#009180", "#e33000", "#80bdff", "#a10040", "#0affe3", "#00523c", "#919100", "#5c00f7", "#a15f00", "#cce6ff", "#00465c", "#adff69", "#f24ba0", "#0dff87", "#ff8c47", "#349400", "#b3b3a1", "#a10067", "#ba544a", "#edc2d1", "#00e8c3", "#3f0073", "#5ec1f7", "#6e00b8", "#f5f5c4", "#e33000", "#52ba00", "#ff943b", "#0079db", "#f0e6c0", "#ffb517", "#cf0076", "#e8cfc9"}

        Dim ObjPriorities As New Dictionary(Of Integer, Double)
        Dim AltValues As New Dictionary(Of Integer, Double)
        Dim AltValuesInOne As New Dictionary(Of Integer, Dictionary(Of Integer, Double))
        Dim AltValuesInZero As New Dictionary(Of Integer, Dictionary(Of Integer, Double))

        Public Sub initSAData()
            If ObjPriorities.Count = 0 Then
                Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
                If H IsNot Nothing Then
                    Dim wrtNode As clsNode = CurrentNode
                    If wrtNode IsNot Nothing Then
                        ObjPriorities.Clear()
                        AltValues.Clear()
                        AltValuesInOne.Clear()
                        ProjectManager.CalculationsManager.InitializeSAGradient(wrtNode.NodeID, False, SAUserID, ObjPriorities, AltValues, AltValuesInOne, 0)
                        AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(wrtNode.NodeID, False, SAUserID, ObjPriorities, 0)
                    End If
                End If
            End If
        End Sub

        Public Sub updateAltValuesinZero(NewObjPriorities As Dictionary(Of Integer, Double))
            Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            If H IsNot Nothing Then
                Dim wrtNode As clsNode = CurrentNode
                If wrtNode IsNot Nothing Then
                    AltValuesInZero = ProjectManager.CalculationsManager.GetGradientData(wrtNode.NodeID, False, SAUserID, NewObjPriorities, 0)
                End If
            End If
        End Sub

        Public Function GetSAObjectives() As String
            Dim retVal As String = ""

            Dim H As clsHierarchy = ProjectManager.Hierarchy(ProjectManager.ActiveHierarchy)
            Dim altH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
            initSAData()
            If ObjPriorities.Count > 0 And altH IsNot Nothing And H IsNot Nothing Then
                Dim wrtNode As clsNode = CurrentNode
                If wrtNode IsNot Nothing Then
                    Dim i = 0
                    For Each obj As clsNode In wrtNode.GetNodesBelow(SAUserID)
                        Dim AttrGuid As Guid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_OBJECTIVE_ID
                        If ProjectManager.ActiveHierarchy = ECHierarchyID.hidImpact Then
                            AttrGuid = ATTRIBUTE_DEFAULT_BRUSH_COLOR_IMPACT_ID
                        End If

                        Dim sNodeColor As String = ""
                        Dim tNodeColor As Long = CLng(ProjectManager.Attributes.GetAttributeValue(AttrGuid, obj.NodeGuidID))
                        sNodeColor = If(tNodeColor > 0, LongToBrush(tNodeColor), GetPaletteColor(CurrentPaletteID(ProjectManager), obj.NodeID))

                        Dim gradientMaxValues As String = ""
                        Dim gradientMinValues As String = ""
                        If AltValuesInOne.ContainsKey(obj.NodeID) AndAlso AltValuesInZero.ContainsKey(obj.NodeID) Then  ' D7497
                            For Each altID In AltValuesInOne(obj.NodeID).Keys
                                gradientMaxValues += CStr(IIf(gradientMaxValues <> "", ",", "")) + String.Format("{{altID:{0},val:{1}}}",
                                                                                       altID, JS_SafeNumber(AltValuesInOne(obj.NodeID)(altID)))
                            Next
                            For Each altID In AltValuesInZero(obj.NodeID).Keys
                                gradientMinValues += CStr(IIf(gradientMinValues <> "", ",", "")) + String.Format("{{altID:{0},val:{1}}}",
                                                                                       altID, JS_SafeNumber(AltValuesInZero(obj.NodeID)(altID)))
                            Next
                        End If

                        retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{id:{0},idx:{1},name:'{2}',value:{3},initValue:{4},gradientMaxValues:[{5}],gradientMinValues:[{6}],gradientInitMinValues:[{7}],color:'{8}'}}",
                                                                                   obj.NodeID, i,
                                                                                   JS_SafeString(obj.NodeName),
                                                                                   JS_SafeNumber(ObjPriorities(obj.NodeID)),
                                                                                   JS_SafeNumber(ObjPriorities(obj.NodeID)),
                                                                                   gradientMaxValues, gradientMinValues, gradientMinValues, sNodeColor)
                        i += 1

                    Next
                End If
            End If
            retVal = String.Format("[{0}]", retVal)
            Return retVal
        End Function

        Public Function GetSAAlternatives() As String
            Dim retVal As String = ""
            Dim altH As clsHierarchy = ProjectManager.AltsHierarchy(ProjectManager.ActiveAltsHierarchy)
            initSAData()
            If AltValues.Count > 0 And altH IsNot Nothing Then
                Dim i = 0
                For Each alt As clsNode In altH.TerminalNodes
                    Dim sAltColor As String = ""
                    Dim altColor As Long = CLng(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_DEFAULT_BRUSH_COLOR_ID, alt.NodeGuidID))
                    sAltColor = If(altColor > 0, LongToBrush(altColor), GetPaletteColor(CurrentPaletteID(ProjectManager), alt.NodeID, True))
                    retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("{{id:{0},idx:{1},name:'{2}',value:{3},initValue:{4},color:'{5}',visible:1,""event_type"":{6}}}", alt.NodeID, i, JS_SafeString(alt.NodeName), JS_SafeNumber(AltValues(alt.NodeID)), JS_SafeNumber(AltValues(alt.NodeID)), sAltColor, CInt(alt.EventType))
                    i += 1
                Next
            End If
            retVal = String.Format("[{0}]", retVal)
            Return retVal
        End Function

        Public Function GetDecimalsValue() As Integer
            Return CInt(ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_DECIMALS_ID, UNDEFINED_USER_ID))
        End Function
#End Region

    End Class

End Namespace