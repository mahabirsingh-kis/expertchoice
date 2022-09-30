Partial Class RAGroupsPage
    Inherits clsComparionCorePage

    Public Const CanShowCosts As Boolean = True

    Public Const OPT_ALLOW_ALL_OR_NONE_GROUP_TYPE As Boolean = True
    Public Const OPT_SCENARIO_LEN As Integer = 45       'A0965
    Public Const OPT_DESCRIPTION_LEN As Integer = 200   'A0965

    Public Sub New()
        MyBase.New(_PGID_RA_GROUPS)
    End Sub

    ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAligner
        End Get
    End Property

    ReadOnly Property Optimizer As RASolver
        Get
            Return RA.Solver
        End Get
    End Property

    ReadOnly Property IsPageForEventGroups As Boolean
        Get
            Return App.ActiveProject.IsRisk AndAlso CurrentPageID = _PGID_RISK_EVENT_GROUPS
        End Get
    End Property

    ReadOnly Property OptimizerGroups() As Dictionary(Of String, RAGroup)
        Get
            If IsPageForEventGroups Then
                Return Optimizer.EventGroups.Groups
            Else
                Return Optimizer.Groups.Groups
            End If
        End Get
    End Property

    ReadOnly Property OptimizerRAAlternatives As List(Of RAAlternative)
        Get
            If IsPageForEventGroups Then Return New List(Of RAAlternative)
            Return Optimizer.Alternatives
        End Get
    End Property

    ReadOnly Property OptimizerNodeAlternatives As List(Of clsNode)
        Get
            If Not IsPageForEventGroups Then Return New List(Of clsNode)
            Return App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).TerminalNodes.Where(Function (alt) alt.Enabled).ToList
        End Get
    End Property

    ReadOnly Property SESSION_VIEW_ALL_GROUPS As String
        Get
            Return String.Format("RA_Groups_ViewAllGroups_{0}", App.ProjectID)
        End Get
    End Property

    Public Property ViewAllGroups As Boolean
        Get
            Dim tSess As String = SessVar(SESSION_VIEW_ALL_GROUPS)
            Return String.IsNullOrEmpty(tSess) OrElse tSess = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_VIEW_ALL_GROUPS) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Private Sub RAGroupsPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        If App.isRiskEnabled Then
            If CheckVar("events", "0") = "1" Then
                CurrentPageID = _PGID_RISK_EVENT_GROUPS
            Else
                CurrentPageID = _PGID_RISK_OPTIMIZER_GROUPS
            End If
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not IsPostBack AndAlso Not IsCallback Then
            RA.Scenarios.CheckModel(False) 'A1324
            'btnViewAllGroups.Checked = ViewAllGroups
        End If

        Dim sAction = EcSanitizer.GetSafeHtmlFragment(CheckVar(_PARAM_ACTION, "")).Trim.ToLower   ' Anti-XSS
        Select Case sAction
            Case "scenario"
                Dim ID As Integer = CheckVar("sid", RA.Scenarios.ActiveScenarioID)
                If ID <> RA.Scenarios.ActiveScenarioID AndAlso RA.Scenarios.Scenarios.ContainsKey(ID) Then
                    RA.Scenarios.ActiveScenarioID = ID
                    Optimizer.ResetSolver()
                End If
                Response.Redirect(PageURL(CurrentPageID, GetTempThemeURI(False)), True)
        End Select

        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not IsCallback Then
            'If RA.Scenarios.Scenarios.Count = 0 Then RA.Load() ' -D4857
        End If
    End Sub

    Public Function LoadGroupsData() As String
        Dim tIndexVisible As Boolean = RA.Scenarios.GlobalSettings.IsIndexColumnVisible 'A1143

        Dim sList As String = ""
        Dim sAllAlts As String = ""

        Dim tOptimizerRAAlternatives As List(Of RAAlternative) = OptimizerRAAlternatives
        Dim tOptimizerNodeAlternatives As List(Of clsNode) = OptimizerNodeAlternatives
        For Each kvp As KeyValuePair(Of String, RAGroup) In OptimizerGroups
            Dim sAlts As String = ""
            If IsPageForEventGroups Then
                For Each sAltID As String In kvp.Value.Alternatives.Keys
                    Dim tAlt As clsNode = App.ActiveProject.ProjectManager.AltsHierarchy(App.ActiveProject.ProjectManager.ActiveAltsHierarchy).GetNodeByID(New Guid(sAltID))
                    If tAlt IsNot Nothing AndAlso tOptimizerNodeAlternatives.Contains(tAlt) Then
                        Dim tAltCost As Double = CDbl(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_COST_ID, tAlt.NodeGuidID))
                        If tAltCost = UNDEFINED_INTEGER_VALUE Then tAltCost = 0
                        sAlts += CStr(IIf(sAlts = "", "", ",")) + String.Format("['{0}','{1}','{2}']", tAlt.NodeGuidID.ToString, JS_SafeString(CStr(IIf(tIndexVisible, tAlt.Index.ToString + ". ", "")) + tAlt.NodeName), tAltCost)
                    End If
                Next
            Else
                For Each tAltKvp As KeyValuePair(Of String, RAAlternative) In kvp.Value.Alternatives
                    If tOptimizerRAAlternatives.Contains(tAltKvp.Value) Then
                        sAlts += CStr(IIf(sAlts = "", "", ",")) + String.Format("['{0}','{1}','{2}']", tAltKvp.Value.ID, JS_SafeString(CStr(IIf(tIndexVisible, tAltKvp.Value.SortOrder.ToString + ". ", "")) + tAltKvp.Value.Name), tAltKvp.Value.Cost) 'A1143
                    End If
                Next
            End If
            sList += CStr(IIf(sList = "", "", ",")) + String.Format("['{0}','{1}',{2},[{3}],'{4}']", kvp.Value.ID, JS_SafeString(kvp.Value.Name), CInt(kvp.Value.Condition), sAlts, CInt(IIf(kvp.Value.Enabled, 1, 0)))
        Next


        If IsPageForEventGroups Then
            For Each tAlt As clsNode In tOptimizerNodeAlternatives
                Dim tAltCost As Double = CDbl(App.ActiveProject.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_COST_ID, tAlt.NodeGuidID))
                If tAltCost = UNDEFINED_INTEGER_VALUE Then tAltCost = 0
                sAllAlts += CStr(IIf(sAllAlts = "", "", ",")) + String.Format("['{0}','{1}','{2}']", tAlt.NodeGuidID.ToString, JS_SafeString(CStr(IIf(tIndexVisible, tAlt.Index.ToString + ". ", "")) + tAlt.NodeName), tAltCost)
            Next
        Else
            For Each tAlt As RAAlternative In tOptimizerRAAlternatives
                sAllAlts += CStr(IIf(sAllAlts = "", "", ",")) + String.Format("['{0}','{1}','{2}']", tAlt.ID, JS_SafeString(CStr(IIf(tIndexVisible, tAlt.SortOrder.ToString + ". ", "")) + tAlt.Name), tAlt.Cost) 'A1143
            Next
        End If

        Return String.Format("var all_alts_list = [{0}]; {1} var groups_list = [{2}];", sAllAlts, vbCr, sList)
    End Function

    Private Function GetAlternativeByStringID(ID As String) As RAAlternative
        Dim retVal As RAAlternative = Nothing
        Dim tOptimizerRAAlternatives As List(Of RAAlternative) = OptimizerRAAlternatives
        Dim tOptimizerNodeAlternatives As List(Of clsNode) = OptimizerNodeAlternatives
        'If IsPageForEventGroups Then
        '    For Each alt As clsNode In tOptimizerNodeAlternatives
        '        If alt.NodeGuidID.ToString = ID Then retVal = alt
        '    Next
        'Else
            For Each alt As RAAlternative In tOptimizerRAAlternatives
                If alt.ID = ID Then retVal = alt
            Next
        'End If
        Return retVal
    End Function

    Public Function GetScenarios() As String
        Dim sRes As String = ""
        For Each tScen As Integer In RA.Scenarios.Scenarios.Keys
            sRes += String.Format("<option value='{0}'{2}>{1}</option>", tScen, ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Name), OPT_SCENARIO_LEN, True) + CStr(IIf(String.IsNullOrEmpty(RA.Scenarios.Scenarios(tScen).Description), "", String.Format(" ({0})", ShortString(SafeFormString(RA.Scenarios.Scenarios(tScen).Description), OPT_DESCRIPTION_LEN)))), IIf(tScen = RA.Scenarios.ActiveScenarioID, " selected", ""))
        Next
        Return String.Format("<select id='cbScenarios' style='width:130px; margin-top:3px; margin-right:2px;' onchange='onSetScenario(this.value);'>{0}</select>", sRes)
    End Function

    Private Function GetGroupCallbackData(sAction As String, group as RAGroup) As String
        Return String.Format("['{0}',['{1}','{2}',{3},{4},{5}]]", sAction, group.ID, JS_SafeString(group.Name), CInt(group.Condition), "[]", CInt(IIf(group.Enabled, 1, 0)))
    End Function

    Protected Sub Ajax_Callback(data As String)
        'If CurrentPageID = _PGID_RISK_OPTIMIZER_GROUPS Then Exit Sub

        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "action")).ToLower ' Anti-XSS
        Dim sComment As String = "" ' D3790
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", String.Format("['{0}']", sAction)))

        Select Case sAction

            Case "add_group"
                Dim sName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim    ' Anti-XSS
                If Not String.IsNullOrEmpty(sName) Then
                    Dim newGroup As RAGroup
                    If IsPageForEventGroups Then
                        newGroup = Optimizer.EventGroups.AddGroup()
                    Else
                        newGroup = Optimizer.Groups.AddGroup()
                    End If
                    Dim sType As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "type")).Trim    ' Anti-XSS
                    Dim sEnabled As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "enabled")).Trim  ' Anti-XSS
                    newGroup.Name = sName
                    newGroup.Condition = CType(CInt(sType), RAGroupCondition)
                    newGroup.Enabled = sEnabled = "1" OrElse sEnabled = "true"
                    tResult = GetGroupCallbackData(sAction, newGroup)
                    sComment = String.Format("Add group '{0}'", ShortString(sName, 40, True))   ' D3790
                End If

            Case "del_group"
                Dim GroupIndex As Integer = CInt(GetParam(args, "group_index").ToLower)
                If GroupIndex >= 0 AndAlso GroupIndex < OptimizerGroups.Count Then
                    sComment = String.Format("Add group '{0}'", ShortString(OptimizerGroups(OptimizerGroups.Keys(GroupIndex)).Name, 40, True))   ' D3790
                    OptimizerGroups.Remove(OptimizerGroups.Keys(GroupIndex))
                End If

            Case "del_all_groups"
                While OptimizerGroups.Count > 0
                    OptimizerGroups.Remove(OptimizerGroups.Keys(0))
                    sComment = String.Format("Delete all groups")   ' D3790
                End While

            Case "include", "exclude"
                Dim GroupIndex As Integer = CInt(GetParam(args, "group_index").ToLower)
                Dim isInclude As Boolean = sAction = "include"
                Dim sItems As String = EcSanitizer.GetSafeHtmlFragment(URLDecode(GetParam(args, "items")))    ' Anti-XSS
                Dim selGroup As RAGroup = OptimizerGroups.Values(GroupIndex)
                If sItems <> "" AndAlso selGroup IsNot Nothing Then
                    Dim sIDs() As String = sItems.Trim.Split(CChar(vbLf))
                    For Each ID As String In sIDs
                        If isInclude Then
                            If Not selGroup.Alternatives.ContainsKey(ID) Then
                                If IsPageForEventGroups Then 
                                    selGroup.Alternatives.Add(ID, new RAAlternative With {.ID = ID})
                                Else
                                    selGroup.Alternatives.Add(ID, GetAlternativeByStringID(ID))
                                End If
                            End If
                        Else
                            If selGroup.Alternatives.ContainsKey(ID) Then selGroup.Alternatives.Remove(ID)
                        End If
                    Next
                    sComment = String.Format("{0} group(s)", sAction)   ' D3790
                End If

            Case "set_type"
                Dim GroupIndex As Integer = CInt(GetParam(args, "group_index").ToLower)
                Dim TypeIndex As Integer = CInt(GetParam(args, "type").ToLower)
                If GroupIndex >= 0 AndAlso GroupIndex < OptimizerGroups.Count Then
                    OptimizerGroups(OptimizerGroups.Keys(GroupIndex)).Condition = CType(TypeIndex, RAGroupCondition)
                    sComment = String.Format("Set '{0}' as '{1}'", ShortString(OptimizerGroups(OptimizerGroups.Keys(GroupIndex)).Name, 40, True), ResString("optGroupType" + TypeIndex.ToString))   ' D3790
                End If

            Case "rename_group"
                Dim GroupIndex As Integer = CInt(GetParam(args, "group_index"))
                Dim NewName As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "name")).Trim  ' Anti-XSS
                Dim sType As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "type")).Trim    ' Anti-XSS
                Dim sEnabled As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "enabled")).Trim  ' Anti-XSS
                If GroupIndex >= 0 AndAlso GroupIndex < OptimizerGroups.Count AndAlso NewName.Length > 0 Then
                    Dim g As RAGroup = OptimizerGroups(OptimizerGroups.Keys(GroupIndex))
                    g.Name = NewName
                    g.Condition = CType(CInt(sType), RAGroupCondition)
                    g.Enabled = sEnabled = "1" OrElse sEnabled = "true"
                    tResult = GetGroupCallbackData(sAction, g)
                    sComment = String.Format("Rename group '{0}'", ShortString(NewName, 40, True))   ' D3790
                End If

            Case "copy_group"
                Dim GroupIndex As Integer = CInt(GetParam(args, "group_index"))
                If GroupIndex >= 0 AndAlso GroupIndex < OptimizerGroups.Count Then
                    Dim newGroup As RAGroup
                    If IsPageForEventGroups Then
                        newGroup = Optimizer.EventGroups.AddGroup()
                    Else
                        newGroup = Optimizer.Groups.AddGroup()
                    End If
                    Dim sourceGroup = OptimizerGroups(OptimizerGroups.Keys(GroupIndex))
                    newGroup.Name = String.Format(ResString("lblCopyOf"), sourceGroup.Name)
                    newGroup.Condition = sourceGroup.Condition
                    newGroup.Enabled = sourceGroup.Enabled
                    newGroup.Alternatives = New Dictionary(Of String, RAAlternative)
                    For Each kvp As KeyValuePair(Of String, RAAlternative) In sourceGroup.Alternatives
                        newGroup.Alternatives.Add(kvp.Key, kvp.Value)
                    Next
                    'newGroup.Events = New List(Of String)
                    'For Each sID As String In sourceGroup.Events
                    '    newGroup.Events.Add(sID)
                    'Next
                    tResult = GetGroupCallbackData(sAction, newGroup)
                    sComment = String.Format("Copy group '{0}'", ShortString(sourceGroup.Name, 40, True))   ' D3790
                End If

            Case "enable_group"
                Dim GroupIndex As Integer = CInt(GetParam(args, "group_index"))
                Dim isEnabled As Boolean = Param2Bool(args, "enabled")
                If GroupIndex >= 0 AndAlso GroupIndex < OptimizerGroups.Count Then
                    OptimizerGroups(OptimizerGroups.Keys(GroupIndex)).Enabled = isEnabled
                    sComment = String.Format("{1} group '{0}'", ShortString(OptimizerGroups(OptimizerGroups.Keys(GroupIndex)).Name, 40, True), IIf(isEnabled, "Enable", "Disable"))   ' D3790
                End If

            Case "view_all_groups"
                Dim sValue As String = EcSanitizer.GetSafeHtmlFragment(GetParam(args, "value")).ToLower   ' Anti-XSS
                ViewAllGroups = sValue = "1" OrElse sValue = "true"
                sAction = ""

        End Select

        If sAction <> "" Then App.ActiveProject.SaveRA("Edit Groups", , , sComment) ' D3790

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub RAGroupsPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA.Load()
    End Sub

End Class