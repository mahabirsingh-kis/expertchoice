Partial Class RiskRegisterPage
    Inherits clsComparionCorePage

    Public Const OPT_PRIORITY_COLUMN_WIDTH As Integer = 90
    Public Const _OPT_SHOW_HEATMAP_IN_PIPE As Boolean = True ' D6663

    Public PageIDs As Integer() = {_PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID, _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART, _PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID, _PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART, _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS, _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS, _PGID_RISK_PLOT_OVERALL, _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, _PGID_RISK_PLOT_CAUSES, _PGID_RISK_PLOT_OBJECTIVES, _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS, _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, _PGID_RISK_BOW_TIE, _PGID_RISK_BOW_TIE_CAUSES, _PGID_RISK_BOW_TIE_OBJS, _PGID_RISK_BOW_TIE_DEFINE_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES, _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS}
    Private PageTitles As String() = {"%%Likelihood%%, %%Impact%%, and %%Risk%% by %%Objectives(l)%%", "%%Risk%% by %%Objectives(l)%%", "%%Likelihood%%, %%Impact%%, and %%Risk%% by %%Objectives(i)%%", "%%Risk%% by %%Objectives(i)%%", "Overall %%Likelihoods%%, %%Impacts%% and %%Risks%%", "%%Likelihoods%%, %%Impacts%% and %%Risks%% from %%Source%% {0}", "%%Likelihoods%%, %%Impacts%% and %%Risks%% to {0} %%Objective(i)%%", "%%Likelihood%%, %%Impact%%, and %%Risk%% from %%Source%% {0} <span id='lblWC'>(With %%Controls%%)</span>", "%%Likelihood%%, %%Impact%%, and %%Risk%% to {0} %%Objective(i)%% <span id='lblWC'>(With %%Controls%%)</span>", "Overall %%Likelihoods%%, %%Impacts%% and %%Risks%% <span id='lblWC'>(With %%Controls%%)</span>", "%%Event%% {0} %%Likelihoods%% from all %%Objectives(l)%% ", "%%Event%% {0} %%Impact%% to all %%Objectives(i)%%", "%%Likelihood%% of the %%Event%% WRT %%Objectives(l)%% <span id='lblWC'>(with %%Controls%%)</span>", "%%Impact%% of the %%Event%% WRT %%Objectives(i)%% <span id='lblWC'>(with %%Controls%%)</span>", "%%Risk%% Map - Overall", "%%Risk%% Map <span id='lblWC'>(with %%controls%%)</span> - Overall", "%%Risk%% Map - From %%Objectives(l)%%", "%%Risk%% Map - To %%Objectives(i)%%", "%%Risk%% Map <span id='lblWC'>(with %%controls%%)</span> - From %%Objectives(l)%%", "%%Risk%% Map <span id='lblWC'>(with %%controls%%)</span> - To %%Objectives(i)%%", "Bow-Tie", "%%Likelihoods%%, %%Impacts%% and %%Risks%% from %%Source%% {0}", "%%Likelihoods%%, %%Impacts%% and %%Risks%% to {0} %%Objective(i)%%", "Bow-Tie", "Bow-Tie <span id='lblWC'>(with %%controls%%)</span>", "%%Likelihoods%%, %%Impacts%% and %%Risks%% from %%Source%% {0} <span id='lblWC'>(with %%controls%%)</span>", "%%Likelihoods%%, %%Impacts%% and %%Risks%% to {0} %%Objective(i)%% <span id='lblWC'>(with %%controls%%)</span>"}
    Public PagesWithControlsList() As Integer = {_PGID_RISK_REGISTER_WITH_CONTROLS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS} 'A1139
    Public PagesGridWrtObjective As Integer() = {_PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS}
    Public PagesToObjectivesWithControls As Integer() = {_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS}

    Public IsWidget As Boolean = False
    Public isPM As Boolean = False  ' D6672

    Public Property RiskHeader As String
    Public Property RisksHeader As String

    ' D6662 ===
    Public AutoShowQuickHelp As Boolean = False
    Public QuickHelpParams As String = ""
    Public QuickHelpEmpty As Boolean = True
    ' D6662 ==

    Public ReadOnly Property LogMessagePrefix As String
        Get
            Return String.Format("{0}: ", Me.Title)
        End Get
    End Property

    Public Enum RegisterViews
        rvRiskRegister = 0
        rvRiskRegisterWithControls = 1
        rvTreatmentRegister = 2
        rvAcceptanceRegister = 3
    End Enum

    Public RegisterView As RegisterViews = RegisterViews.rvRiskRegister

    ' actions
    Public Const ACTION_DECIMALS As String = "decimals"
    Public Const ACTION_CREATE_DEFAULT_ATTRIBUTE As String = "create_default_column"

    Dim _showCents As Boolean? = Nothing
    Public Property ShowCents As Boolean
        Get
            If Not _showCents.HasValue Then
                _showCents = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_CENTS_ID, UNDEFINED_USER_ID))
            End If
            Return _showCents.Value
        End Get
        Set(value As Boolean)
            _showCents = value
            WriteSetting(PRJ, ATTRIBUTE_RISK_SHOW_CENTS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public ReadOnly Property CostDecimalDigits As Integer
        Get
            If ShowCents Then Return 2
            Return 0
        End Get
    End Property

    Public Function CanAddControls() As Boolean
        Return CurrentPageID = _PGID_RISK_TREATMENTS_DICTIONARY
    End Function    

    Public ReadOnly Property SelectedHierarchyNode As clsNode
        Get
            Dim tGuid As Guid = SelectedHierarchyNodeID
            Dim retVal As clsNode = Nothing
            If tGuid <> Guid.Empty Then
                retVal = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tGuid)
                If retVal Is Nothing Then retVal = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tGuid)
            End If
            Return retVal
        End Get
    End Property

    Public Property SelectedHierarchyNodeID As Guid
        Get
            If PagesGridWrtObjective.Contains(CurrentPageID)
                Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
    
                Dim retVal As Guid = Guid.Empty
                Dim s As String = CStr(PM.Attributes.GetAttributeValue(If(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), UNDEFINED_USER_ID))
                If Not String.IsNullOrEmpty(s) AndAlso s.Length > 16 Then retVal = New Guid(s)

                If PM.Hierarchy(HierarchyID).GetNodeByID(retVal) Is Nothing Then retVal = PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID
                Return retVal
            Else            
                Return PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID
            End If
        End Get
        Set(value As Guid)
            Dim HierarchyID As ECHierarchyID = CType(PM.ActiveHierarchy, ECHierarchyID)
            If PagesGridWrtObjective.Contains(CurrentPageID)
                HierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
            End If
            If IsWidget Then
                PM.Attributes.SetAttributeValue(CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), UNDEFINED_USER_ID, AttributeValueTypes.avtString, value.ToString, Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), AttributeValueTypes.avtString, value.ToString)
            End If
        End Set
    End Property

    Public Function GetRegisterTitle() As String
        Dim retVal As String = ""
        Select Case RegisterView
            Case RegisterViews.rvRiskRegister, RegisterViews.rvRiskRegisterWithControls
                retVal = ParseString("%%Risk%% Register")
            Case RegisterViews.rvTreatmentRegister
                retVal = ParseString("%%Control%% Register")
            Case RegisterViews.rvAcceptanceRegister
                retVal = ParseString("Acceptance Register")
        End Select

        For i As Integer = 0 To PageIDs.Length - 1
            If PageIDs(i) = CurrentPageID Then
                Dim sTitle = PageTitles(i)
                If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then  ' D6798
                    sTitle = sTitle.Replace("%%Risk%%", RiskHeader).Replace("%%Risks%%", RisksHeader)
                End If
                retVal = String.Format(ParseAllTemplates(sTitle, App.ActiveUser, App.ActiveProject), "<span id='divNodeName'></span>")
                'If retVal.Contains("id='lblWC'") Then
                '    retVal = retVal.Insert(retVal.IndexOf("id='lblWC'") + "id='lblWC'".Length, If((PagesWithControlsList.Contains(CurrentPageID)) AndAlso Not UseControlsReductions, " style='display: none;' ", ""))
                'End If                

                'If IsWidget Then retVal = ParseString(String.Format("%%Likelihoods%%, %%Impacts%% and %%Risks%% for {0}'s {1}", PM.User.HTMLDisplayName, SafeFormString(App.ActiveProject.ProjectName)))
            End If
        Next
        'retVal += "<span style='color:#DC6200;'>" + If(PM.Parameters.Riskion_Use_Simulated_Values = 0 , " (Computed)", " (Simulated)") + "</span>"
        'retVal += If(PM.Parameters.Riskion_Use_Simulated_Values = 0, " (computed)", " (with simulations)")
        'retVal += String.Format("<br/>{0} <span id='lblTimestamp'>{1}</span>", SafeFormString(PRJ.ProjectName), If(PM.Parameters.Riskion_Register_Timestamp, String.Format("({0})", DateTime.Now.ToString), ""))
        retVal += String.Format(" for <u>{0}</u><span id='lblTimestamp'>{1}</span>", SafeFormString(PRJ.ProjectName), If(PM.Parameters.Riskion_Register_Timestamp, String.Format(" (As of: {0})", DateTime.Now.ToString), ""))
        retVal += String.Format("<small id='lblSimMode'></small>")

        If IsWidget Then retVal = ParseString(String.Format("%%Likelihoods%%, %%Impacts%% and %%Risks%% for {0}'s {1}", PM.User.HTMLDisplayName, SafeFormString(App.ActiveProject.ProjectName)))

        Return retVal
    End Function

    Private IsLikelihoodRelative1Flag As Byte = 1 << 0 ' 1
    Private IsImpactRelative1Flag As Byte = 1 << 1 ' 2
    Private IsRiskRelative1Flag As Byte = 1 << 2 ' 4
    Private IsOverallRelative1Flag As Byte = 1 << 3 ' 8  'for All Participants only

    Private MaxPriorityL As Double = Double.MinValue
    Private MaxPriorityI As Double = Double.MinValue
    Private MaxPriorityR As Double = Double.MinValue

    Public NormMaxValues As String = "{}"

    Public Property BarsRelativeTo1State As Byte
        Get
            Return CByte(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_BARS_RELATIVE_TO_1_STATE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Byte)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_BARS_RELATIVE_TO_1_STATE_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property BarsLRelativeTo1 As Boolean
        Get
            Return (BarsRelativeTo1State And IsLikelihoodRelative1Flag) > 0
        End Get
        Set(value As Boolean)
            If value Then
                BarsRelativeTo1State = BarsRelativeTo1State Or IsLikelihoodRelative1Flag
            Else
                BarsRelativeTo1State = BarsRelativeTo1State And Not IsLikelihoodRelative1Flag
            End If
        End Set
    End Property

    Public Property BarsIRelativeTo1 As Boolean
        Get
            Return (BarsRelativeTo1State And IsImpactRelative1Flag) > 0
        End Get
        Set(value As Boolean)
            If value Then
                BarsRelativeTo1State = BarsRelativeTo1State Or IsImpactRelative1Flag
            Else
                BarsRelativeTo1State = BarsRelativeTo1State And Not IsImpactRelative1Flag
            End If
        End Set
    End Property

    Public Property BarsRRelativeTo1 As Boolean
        Get
            Return (BarsRelativeTo1State And IsRiskRelative1Flag) > 0
        End Get
        Set(value As Boolean)
            If value Then
                BarsRelativeTo1State = BarsRelativeTo1State Or IsRiskRelative1Flag
            Else
                BarsRelativeTo1State = BarsRelativeTo1State And Not IsRiskRelative1Flag
            End If
        End Set
    End Property

    Public Property IsShowBarsRelativeToOne As Boolean
        Get
            Return (BarsRelativeTo1State And IsOverallRelative1Flag) > 0
        End Get
        Set(value As Boolean)
            If value Then
                BarsRelativeTo1State = BarsRelativeTo1State Or IsOverallRelative1Flag
            Else
                BarsRelativeTo1State = BarsRelativeTo1State And Not IsOverallRelative1Flag
            End If
        End Set
    End Property

    Private _MultipleSelectedHierarchyNodeGUIDs As List(Of Guid) = Nothing
    Private Property MultipleSelectedHierarchyNodeGUIDs As List(Of Guid)
        Get
            If _MultipleSelectedHierarchyNodeGUIDs Is Nothing Then 
                _MultipleSelectedHierarchyNodeGUIDs = CType(Session(String.Format("risk_results_multiselect_guids_{0}", App.ProjectID)), List(Of Guid))
            End If
            Return _MultipleSelectedHierarchyNodeGUIDs
        End Get
        Set(value As List(Of Guid))
            _MultipleSelectedHierarchyNodeGUIDs = value
            Session(String.Format("risk_results_multiselect_guids_{0}", App.ProjectID)) = value
        End Set
    End Property

    ReadOnly Property SESS_TREATMENTS As String
        Get
            Return String.Format("RISK_TREATMENTS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public _Treatments As List(Of clsControl) = Nothing
    Public Property Treatments As List(Of clsControl)
        Get
            If _Treatments Is Nothing Then
                Dim tSessVar = Session(SESS_TREATMENTS)
                If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of clsControl) Then
                    _Treatments = CType(tSessVar, List(Of clsControl))
                End If
            End If
            Return _Treatments
        End Get
        Set(value As List(Of clsControl))
            _Treatments = value
            Session(SESS_TREATMENTS) = value
        End Set
    End Property

    Public Function GetTreatmentByName(ctrlName As String, ctrlType As ControlType) As clsControl
        ctrlName = ctrlName.Trim
        Dim retVal As clsControl = Nothing
        With PM
            For Each control As clsControl In Treatments
                If control.Type = ctrlType AndAlso control.Name = ctrlName Then
                    retVal = control
                    Exit For
                End If
            Next
        End With

        Return retVal
    End Function

    Public Property ShowTotalRisk As Boolean
        Get
            If IsWidget Then Return False
            Return PM.Parameters.Riskion_Show_Total_Risk
        End Get
        Set(value As Boolean)
            PM.Parameters.Riskion_Show_Total_Risk = value
            PM.Parameters.Save()
        End Set
    End Property

    Public ReadOnly Property UseDollarValue As Boolean
        Get
            Dim retVal As Boolean = False
            If PM.CalculationsManager.DollarValue <> UNDEFINED_INTEGER_VALUE Then
                retVal = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
            End If
            Return retVal
        End Get
    End Property

    Public Property ShowDollarValue As Boolean
        Get
            Return UseDollarValue AndAlso PM.CalculationsManager.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso PM.CalculationsManager.DollarValueTarget <> UNDEFINED_STRING_VALUE
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, AttributeValueTypes.avtBoolean, value, "", "")
        End Set
    End Property

    ' dollar value of DollarValueTarget (which can be an impact objective or event)
    Public Property DollarValue As Double
        Get
            Return PM.CalculationsManager.DollarValue
        End Get
        Set(value As Double)
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_ID, AttributeValueTypes.avtDouble, value, ParseString(LogMessagePrefix + "Total Cost Changed to " + CostString(value)), "")
        End Set
    End Property

    Public Property DollarValueTarget As String
        Get
            If PM.CalculationsManager.DollarValueTarget = "" Then Return PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID.ToString
            Return PM.CalculationsManager.DollarValueTarget
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_TARGET_ID, AttributeValueTypes.avtString, value, ParseString(LogMessagePrefix + "Cost Target Changed"), "")
        End Set
    End Property

    ' Dollar value of Impact Goal node
    Private _DollarValueOfEnterprise As Double = UNDEFINED_INTEGER_VALUE
    Public ReadOnly Property DollarValueOfEnterprise As Double
        Get
            If _DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE Then 
                _DollarValueOfEnterprise = If(PM.DollarValueOfEnterprise <> UNDEFINED_INTEGER_VALUE, PM.DollarValueOfEnterprise, 0)
            End If
            Return _DollarValueOfEnterprise
        End Get
    End Property

    Public Function GetTargetName() As String
        Dim retVal As String = ResString("lblEnterprise")
        If DollarValueTarget <> "" AndAlso DollarValueTarget <> UNDEFINED_STRING_VALUE Then
            Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(New Guid(DollarValueTarget))
            If tAlt IsNot Nothing Then retVal = tAlt.NodeName
            Dim tNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(New Guid(DollarValueTarget))
            If tNode IsNot Nothing Then retVal = tNode.NodeName
            Return "&quot;" + ShortString(retVal, 40) + "&quot;"
        End If
        Return retVal
    End Function

    Public Function GetDollarValueFullString() As String
        Dim tImpactGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0)
        Dim retVal As String = ""
        retVal = String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), ResString("lblEnterprise"), CostString(DollarValueOfEnterprise, CostDecimalDigits, True))
        If DollarValueTarget <> "" AndAlso DollarValueTarget <> UNDEFINED_STRING_VALUE AndAlso Not tImpactGoalNode.NodeGuidID.Equals(New Guid(DollarValueTarget)) Then
            Dim tNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(New Guid(DollarValueTarget))
            Dim sDollvalue As String = ""
            If DollarValue <> UNDEFINED_INTEGER_VALUE Then sDollvalue = CostString(DollarValue, CostDecimalDigits, True)
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), GetTargetName(), sDollvalue)
        End If
        Return CStr(IIf(DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso retVal <> "", " (" + retVal + ")", ""))
    End Function

    Public Sub New()
        MyBase.New(_PGID_RISK_REGISTER_DEFAULT)
    End Sub

    Public ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAlignerRisk
        End Get
    End Property

    Public Property SelectedUsersAndGroupsIDs As List(Of Integer)
        Get
            Dim sUsers As String = UNDEFINED_USER_ID.ToString   ' D3377
            If Not App.Options.isSingleModeEvaluation AndAlso Not IsWidget Then sUsers = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_USERS_ID, UNDEFINED_USER_ID)) ' D3377 + D6048
            If String.IsNullOrEmpty(sUsers) Then Return Nothing
            Dim tList As String() = sUsers.Split(CChar(","))
            If tList.Count > 0 Then
                Dim res As New List(Of Integer)
                For Each item In tList
                    Dim tID As Integer
                    If Integer.TryParse(item, tID) AndAlso (PM.UserExists(tID) OrElse PM.CombinedGroups.CombinedGroupExists(tID)) Then
                        res.Add(tID)
                    End If
                Next
                Return res
            End If
            Return Nothing
        End Get
        Set(value As List(Of Integer))
            Dim sUsers As String = ""
            For i As Integer = 0 To value.Count - 1
                sUsers = sUsers + value(i).ToString
                If i <> value.Count - 1 Then sUsers += ","
            Next
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, sUsers)
        End Set
    End Property

    Public Property StringSelectedUsersAndGroupsIDs As String
        Get
            Return CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_SYNTHESIS_USERS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_SYNTHESIS_USERS_ID, AttributeValueTypes.avtString, value)
        End Set
    End Property

    Private Function IsUserChecked(UserID As Integer) As Boolean
        Return SelectedUsersAndGroupsIDs.Contains(UserID)
    End Function

    Private UserIDsWithData As List(Of Integer) = New List(Of Integer)

    Public Function GetUsersData() As String
        Dim retVal As String = ""
        Dim userList As List(Of Integer) = SelectedUsersAndGroupsIDs
        UserIDsWithData.Clear()
        ' get users data
        For Each usr As clsUser In PM.UsersList
            Dim IsChecked As Boolean = userList.Contains(usr.UserID)
            Dim HasData As Boolean = MiscFuncs.DataExistsInProject(ECModelStorageType.mstCanvasStreamDatabase, App.ActiveProject.ID, App.ActiveProject.ConnectionString, App.ActiveProject.ProviderType, usr.UserEMail)
            If HasData Then UserIDsWithData.Add(usr.UserID)
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1},'{2}','{3}',{4}]", CStr(IIf(IsChecked, 1, 0)), usr.UserID, JS_SafeString(usr.UserName), JS_SafeString(usr.UserEMail), IIf(HasData, 1, 0)) ' ID, name, email, isChecked, HasData
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetGroupsData() As String
        Dim retVal As String = ""

        Dim userList As List(Of Integer) = SelectedUsersAndGroupsIDs

        ' apply rules for dynamic groups
        With PM.StorageManager
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .ProjectLocation, .ProviderType, .ModelID, -1)
        End With

        ' get groups data
        If PM.CombinedGroups IsNot Nothing AndAlso PM.CombinedGroups.GroupsList IsNot Nothing Then
            For Each cg As clsCombinedGroup In PM.CombinedGroups.GroupsList
                cg.ApplyRules()
            Next

            For Each grp As clsCombinedGroup In PM.CombinedGroups.GroupsList
                Dim IsChecked As Boolean = userList.Contains(grp.CombinedUserID) 'grp.CombinedUserID = COMBINED_USER_ID
                Dim GroupHasData As Boolean = IsGroupHasData(grp)
                Dim sUsers As String = ""
                For Each u As clsUser In grp.UsersList
                    sUsers += CStr(IIf(sUsers = "", "", ",")) + u.UserID.ToString
                Next
                retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1},'{2}','{3}',{4},[{5}]]", CStr(IIf(IsChecked, 1, 0)), grp.CombinedUserID, JS_SafeString(grp.Name), "", IIf(GroupHasData, 1, 0), sUsers) ' isChecked, ID, name, 'email - obsolete', HasData (1/0), Participants
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Private Function IsGroupHasData(grp As clsCombinedGroup) As Boolean
        Dim retVal As Boolean = False
        For Each user As clsUser In grp.UsersList
            If UserIDsWithData.Contains(user.UserID) Then Return True
        Next
        Return retVal
    End Function

    Private Sub RiskRegisterPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        IsWidget = CheckVar("widget", False)
        Dim sMode = CheckVar("mode", "").Trim.ToLower

        Select Case sMode
            Case "", "overall", "sources", "objectives"
                RegisterView = RegisterViews.rvRiskRegister
            Case "withcontrols", "overall_wc", "sources_wc", "objectives_wc"
                RegisterView = RegisterViews.rvRiskRegisterWithControls
            Case "treatments"
                RegisterView = RegisterViews.rvTreatmentRegister
            Case "acc"
                RegisterView = RegisterViews.rvAcceptanceRegister
        End Select

        Select Case RegisterView
            Case RegisterViews.rvRiskRegister
                Select Case sMode
                    Case "overall"
                        CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS
                    Case "sources"
                        CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES
                    Case "objectives"
                        CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS
                    Case Else
                        CurrentPageID = _PGID_RISK_REGISTER_DEFAULT
                End Select
            Case RegisterViews.rvRiskRegisterWithControls
                Select Case sMode
                    Case "overall_wc"
                        CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL
                    Case "sources_wc"
                        CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES
                    Case "objectives_wc"
                        CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS
                    Case "withcontrols"
                        CurrentPageID = _PGID_RISK_REGISTER_WITH_CONTROLS
                    Case Else
                        CurrentPageID = _PGID_RISK_REGISTER_WITH_CONTROLS
                End Select
            Case RegisterViews.rvTreatmentRegister
                CurrentPageID = _PGID_RISK_TREATMENTS_REGISTER
            Case RegisterViews.rvAcceptanceRegister
                CurrentPageID = _PGID_RISK_ACCEPTANCE_REGISTER
            Case Else
                CurrentPageID = _PGID_RISK_REGISTER_DEFAULT
        End Select

        If IsWidget Then CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS

        If Not IsPostBack AndAlso Not isCallback Then 
            MultipleSelectedHierarchyNodeGUIDs = Nothing
        End If
    End Sub

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        Dim sAction = CheckVar(_PARAM_ACTION, "").Trim.ToLower
        Select Case sAction
            Case ""
        End Select

        Treatments = ControlsList

        If Not isCallback AndAlso Not IsPostBack Then
            RA.RiskOptimizer.isOpportunityModel = PRJ.isOpportunityModel

            With RA.RiskOptimizer.CurrentScenario
                .OriginalAllControlsCost = 0
                .OriginalSelectedControlsCost = 0
                .OriginalSelectedControlsCount = 0
                For Each ctrl As clsControl In Treatments
                    If ctrl.Enabled AndAlso ctrl.IsCostDefined Then
                        .OriginalAllControlsCost += ctrl.Cost
                    End If
                    If ctrl.Enabled AndAlso ctrl.Active Then
                        If ctrl.IsCostDefined Then
                            .OriginalSelectedControlsCost += ctrl.Cost
                        End If
                        .OriginalSelectedControlsCount += 1
                    End If
                    ctrl.SARiskReduction = UNDEFINED_INTEGER_VALUE
                    ctrl.SARiskReductionMonetary = UNDEFINED_INTEGER_VALUE
                Next

            End With
        End If

        Dim hasRisks As Boolean = PRJ.RiskionProjectType = ProjectType.ptRegular OrElse ((PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) AndAlso PM.ActiveAlternatives.Nodes.Where(Function(alt) alt.EventType = EventType.Risk).Count > 0) ' D6798
        Dim hasOpportunities As Boolean = PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse ((PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) AndAlso PM.ActiveAlternatives.Nodes.Where(Function(alt) alt.EventType = EventType.Opportunity).Count > 0)    ' D6798
        RiskHeader = ParseString("%%Risk%%")
        RisksHeader = ParseString("%%Risks%%")
        If hasRisks AndAlso hasOpportunities Then
            RiskHeader = ParseString("%%Risk%%/%%Opportunity%%")
            RisksHeader = ParseString("%%Risks%%/%%Opportunities%%")
        End If
        If Not hasRisks AndAlso hasOpportunities Then
            RiskHeader = ParseString("%%Opportunity%%")
            RiskHeader = ParseString("%%Opportunities%%")
        End If

        Ajax_Callback(Request.Form.ToString)
    End Sub

    Public Function GetLecUrl() As String
        Dim LecPgID As Integer = If(PagesWithControlsList.Contains(CurrentPageID), _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS, _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT)
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS        
        Return PageURL(LecPgID)
    End Function

    Protected Sub Page_PreLoad(sender As Object, e As EventArgs) Handles Me.PreLoad
        If Not IsPostBack AndAlso Not isCallback Then
            With PM.ResourceAligner.RiskOptimizer
                .InitOriginalValues()
            End With
        End If
    End Sub

    Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
        With PM
            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
        End With
    End Sub

    Private Function GetAltData(riskData As RiskDataWRTNodeDataContract, altGuid As Guid) As AlternativeRiskDataDataContract
        For Each alt In riskData.AlternativesData
            If alt.AlternativeID.Equals(altGuid) Then
                Return alt
            End If
        Next
        Return Nothing
    End Function

    Private _HierarchyTerminalNodes As List(Of clsNode) = Nothing
    Public ReadOnly Property HierarchyTerminalNodes As List(Of clsNode) 
    Get
            If _HierarchyTerminalNodes Is Nothing Then 
                _HierarchyTerminalNodes = PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes
            End If
        Return _HierarchyTerminalNodes
    End Get
    End Property

    Public Function GetCoveringSources() As String
        Dim retVal As String = ""
        For Each node As clsNode In HierarchyTerminalNodes
            retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""name"":""{1}"",""info"":""{2}""}}", node.NodeID, JS_SafeString(node.NodeName), JS_SafeString(ShortString(Infodoc2Text(PRJ, node.InfoDoc, True), 350, True)))
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetEventsData() As String
        If RegisterView <> RegisterViews.rvRiskRegister AndAlso RegisterView <> RegisterViews.rvRiskRegisterWithControls Then Return "[]"

        Dim retVal As String = ""

        MaxPriorityL = Double.MinValue
        MaxPriorityI = Double.MinValue
        MaxPriorityR = Double.MinValue

        Dim WRTNodesGuids As List(Of Guid) = Nothing
        If IsPostBack OrElse isCallback AndAlso MultipleSelectedHierarchyNodeGUIDs IsNot Nothing Then 
            WRTNodesGuids = MultipleSelectedHierarchyNodeGUIDs
        Else
            WRTNodesGuids = New List(Of Guid)
            WRTNodesGuids.Add(SelectedHierarchyNodeID)
        End If
        
        Dim oldSelectedHierarchyNodeGuid As Guid = SelectedHierarchyNodeID

        'Dim OldUseSimulatedValues As Boolean = PM.CalculationsManager.UseSimulatedValues
        'PM.CalculationsManager.UseSimulatedValues = PM.Parameters.Riskion_Use_Simulated_Values <> 0 OrElse IsWidget

        Dim usersList = GetCheckedUserIDs()        

        For Each WRTNodeGuid As Guid In WRTNodesGuids 
            SelectedHierarchyNodeID = WRTNodeGuid

            Dim usersRiskData As New Dictionary(Of Integer, RiskDataWRTNodeDataContract)
            Dim usersRiskDataWC As New Dictionary(Of Integer, RiskDataWRTNodeDataContract)

            With PM
                For Each CurrentUser In usersList
                    Dim CurrentUserID As Integer = CurrentUser.Item1
                    Dim riskDataWC As RiskDataWRTNodeDataContract = .CalculationsManager.GetRiskDataWRTNode(CurrentUserID, If(CurrentUserID >= 0, PM.GetUserByID(CurrentUserID).UserEMail, ""), SelectedHierarchyNode.NodeGuidID, ControlsUsageMode.UseOnlyActive)
                    usersRiskDataWC.Add(CurrentUserID, riskDataWC)

                    If PM.CalculationsManager.UseSimulatedValues Then
                        PM.RiskSimulations.SimulateCommon(CurrentUserID, ControlsUsageMode.UseOnlyActive, SelectedHierarchyNode)
                        For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                            For Each item In riskDataWC.AlternativesData
                                If item.AlternativeID = alt.NodeGuidID Then
                                    item.LikelihoodValue = alt.SimulatedAltLikelihood
                                    item.ImpactValue = alt.SimulatedAltImpact
                                    item.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                                End If
                            Next
                        Next
                    End If

                    Dim riskDataWithoutControls As RiskDataWRTNodeDataContract = .CalculationsManager.GetRiskDataWRTNode(CurrentUserID, If(CurrentUserID >= 0, PM.GetUserByID(CurrentUserID).UserEMail, ""), SelectedHierarchyNode.NodeGuidID, ControlsUsageMode.DoNotUse)
                    usersRiskData.Add(CurrentUserID, riskDataWithoutControls)
                    If .CalculationsManager.UseSimulatedValues Then
                        PM.RiskSimulations.SimulateCommon(CurrentUserID, ControlsUsageMode.DoNotUse, SelectedHierarchyNode)
                        For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                            For Each item In riskDataWithoutControls.AlternativesData
                                If item.AlternativeID = alt.NodeGuidID Then
                                    item.LikelihoodValue = alt.SimulatedAltLikelihood
                                    item.ImpactValue = alt.SimulatedAltImpact
                                    item.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                                End If
                            Next
                        Next
                    End If
                Next
            End With

            Dim H As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            Dim tSelectedHierarchyNode = SelectedHierarchyNode
            Dim ContributedAltsList As List(Of clsNode) = If(tSelectedHierarchyNode.ParentNode Is Nothing, PM.ActiveAlternatives.TerminalNodes, PM.ActiveAlternatives.TerminalNodes.Where(Function(alt) AnySubObjectiveContributes(tSelectedHierarchyNode, alt.NodeID)).ToList)
            If H IsNot Nothing AndAlso H.Nodes.Count > 0 Then
                For Each alt As clsNode In ContributedAltsList
                    retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""idx"":""{1}"",""guid"":""{2}"",""name"":""{3}"",""event_type"":""{4}"",""enabled"":{5}", alt.iIndex, alt.Index, alt.NodeGuidID, JS_SafeString(alt.NodeName), CInt(alt.EventType), Bool2JS(alt.Enabled))
                    retVal += String.Format(",""info"":""{0}""", JS_SafeString(Infodoc2Text(PRJ, alt.InfoDoc, True)))
                    retVal += String.Format(",""wrt_node"":""{0}"",""wrt_node_guid"":""{1}""", JS_SafeString(SelectedHierarchyNode.NodePath(True)), SelectedHierarchyNode.NodeGuidID)

                    Dim tColor As String = GetPaletteColor(CurrentPaletteID(PM), H.Nodes.IndexOf(alt), True)
                    retVal += String.Format(",""color"":""{0}""", tColor)

                    If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then
                        Dim covObjs As New List(Of clsNode)
                        Dim covObjIDs As String = ""
                        For Each node In HierarchyTerminalNodes
                            Dim contributedAlternatives = node.GetContributedAlternatives
                            If contributedAlternatives IsNot Nothing AndAlso contributedAlternatives.Contains(alt) AndAlso Not covObjs.contains(node) Then covObjs.Add(node)
                        Next
                        For Each node In CovObjs
                            covObjIDs += If(covObjIDs = "", "", ",") + HierarchyTerminalNodes.IndexOf(node).ToString
                        Next
                        retVal += String.Format(",""likelihood_contributions"":[{0}]", covObjIDs)
                    End If

                    retVal += String.Format(",""groups"":""{0}""", JS_SafeString(App.ActiveProject.ProjectManager.EventsGroups.GroupName(alt)))

                    For Each CurrentUser In usersList
                        Dim CurrentUserID As Integer = CurrentUser.Item1
                        Dim riskData = usersRiskData(CurrentUserID)
                        Dim riskDataWC = usersRiskDataWC(CurrentUserID)

                        Dim altData As AlternativeRiskDataDataContract = GetAltData(riskData, alt.NodeGuidID)
                        Dim altDataWC As AlternativeRiskDataDataContract = GetAltData(riskDataWC, alt.NodeGuidID)
                        Dim l As String = "N/A"
                        Dim i As String = "N/A"
                        Dim r As String = "N/A"
                        Dim lwc As String = "N/A"
                        Dim iwc As String = "N/A"
                        Dim rwc As String = "N/A"
                        If altData IsNot Nothing Then
                            l = JS_SafeNumber(Math.Abs(altData.LikelihoodValue), _OPT_JSMaxDecimalPlaces)
                            i = JS_SafeNumber(Math.Abs(altData.ImpactValue), _OPT_JSMaxDecimalPlaces)
                            r = JS_SafeNumber(Math.Abs(altData.RiskValue), _OPT_JSMaxDecimalPlaces)

                            If Not BarsLRelativeTo1 AndAlso MaxPriorityL < Math.Abs(altData.LikelihoodValue) Then MaxPriorityL = Math.Abs(altData.LikelihoodValue)
                            If Not BarsIRelativeTo1 AndAlso MaxPriorityI < Math.Abs(altData.ImpactValue) Then MaxPriorityI = Math.Abs(altData.ImpactValue)
                            If Not BarsRRelativeTo1 AndAlso MaxPriorityR < Math.Abs(altData.RiskValue) Then MaxPriorityR = Math.Abs(altData.RiskValue)
                        End If
                        If altDataWC IsNot Nothing Then
                            lwc = JS_SafeNumber(Math.Abs(altDataWC.LikelihoodValue), _OPT_JSMaxDecimalPlaces)
                            iwc = JS_SafeNumber(Math.Abs(altDataWC.ImpactValue), _OPT_JSMaxDecimalPlaces)
                            rwc = JS_SafeNumber(Math.Abs(altDataWC.RiskValue), _OPT_JSMaxDecimalPlaces)

                            If Not BarsLRelativeTo1 AndAlso MaxPriorityL < Math.Abs(altDataWC.LikelihoodValue) Then MaxPriorityL = Math.Abs(altDataWC.LikelihoodValue)
                            If Not BarsIRelativeTo1 AndAlso MaxPriorityI < Math.Abs(altDataWC.ImpactValue) Then MaxPriorityI = Math.Abs(altDataWC.ImpactValue)
                            If Not BarsRRelativeTo1 AndAlso MaxPriorityR < Math.Abs(altDataWC.RiskValue) Then MaxPriorityR = Math.Abs(altDataWC.RiskValue)
                        End If

                        retVal += String.Format(",""l{1}"":""{0}""", l, CurrentUserID)
                        retVal += String.Format(",""i{1}"":""{0}""", i, CurrentUserID)
                        retVal += String.Format(",""r{1}"":""{0}""", r, CurrentUserID)
                        retVal += String.Format(",""lwc{1}"":""{0}""", lwc, CurrentUserID)
                        retVal += String.Format(",""iwc{1}"":""{0}""", iwc, CurrentUserID)
                        retVal += String.Format(",""rwc{1}"":""{0}""", rwc, CurrentUserID)
                    Next

                    Dim Attributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)
                    Dim attrIndex As Integer = 0

                    For Each attr As clsAttribute In Attributes
                        Dim attrStr As String = PM.Attributes.GetAttributeValueString(attr.ID, alt.NodeGuidID)
                        retVal += String.Format(",""a{0}"":""{1}""", attrIndex, JS_SafeString(attrStr))
                        attrIndex += 1
                    Next

                    'A1103 ===
                    For Each id As Guid In RiskRegisterAttributesIDs
                        If False AndAlso PM.Attributes.GetAttributeByID(id) Is Nothing Then
                            retVal += String.Format(",""a{0}"":""""", attrIndex)
                            attrIndex += 1
                        End If
                    Next
                    'A1103 ==

                    retVal += String.Format(",""attr_count"":{0}", attrIndex) + "}"
                Next
            End If

        Next

        If BarsLRelativeTo1 Then MaxPriorityL = 1
        If BarsIRelativeTo1 Then MaxPriorityI = 1
        If BarsRRelativeTo1 Then MaxPriorityR = 1

        'PM.CalculationsManager.UseSimulatedValues = OldUseSimulatedValues
        SelectedHierarchyNodeID = oldSelectedHierarchyNodeGuid

        NormMaxValues = String.Format("{{maxL : {0}, maxI : {1}, maxR : {2}}}", JS_SafeString(MaxPriorityL), JS_SafeString(MaxPriorityI), JS_SafeString(MaxPriorityR))

        Return String.Format("[[{0}],{1}]", retVal, NormMaxValues)
    End Function

    Public Function GetEventsColumns() As String
        If RegisterView <> RegisterViews.rvRiskRegister AndAlso RegisterView <> RegisterViews.rvRiskRegisterWithControls Then Return "[]"

        Dim retVal As String = ""
        Dim lblIndex As String = ResString("optIndexID")
        If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then lblIndex = ResString("optRank")
        If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.UniqueID Then lblIndex = ResString("optUniqueID")
        retVal = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 0, lblIndex, Bool2JS(True), "", "idx", 0, "false", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 0, "Color", Bool2JS(False), "", "color", 0, "false", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 1, ParseString("%%Alternative%% Name"), Bool2JS(True), "", "name", 0, "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 2, ParseString("Description"), Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs), "", "info", 0, "true", "false")
        
        If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then  ' D6798
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 3, ParseString("%%Objective(l)%%"), Bool2JS(True), "", "likelihood_contributions", 0, "true", "true")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 3, ResString("colEventType"), Bool2JS(True), "", "event_type", 0, "true", "true")
        End If

        Dim EventsWithSimulationGroupCount As Integer = 0
        App.ActiveProject.ProjectManager.ActiveAlternatives.Nodes.ForEach(Sub (a)
                                                                              Dim g = App.ActiveProject.ProjectManager.EventsGroups.GetGroupsWithEvent(a.NodeGuidID)
                                                                              EventsWithSimulationGroupCount += If(g IsNot Nothing, g.Count, 0)
                                                                          End Sub)

        If EventsWithSimulationGroupCount > 0 Then retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 4, "Simulation Group", Bool2JS(True), "", "groups", 0, "true", "true")

        ' if Multi-select is ON
        If (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS) AndAlso MultipleSelectedHierarchyNodeGUIDs IsNot Nothing Then 
            Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 2, If(HierarchyID = ECHierarchyID.hidLikelihood, ParseString("WRT %%Objective(l)%%"), ParseString("WRT %%Objective(i)%%")), "true", "", "wrt_node", 0, "true", "true")
        End If

        Dim i As Integer = 3
        Dim attrIndex As Integer = 0
        For Each attr As clsAttribute In PM.Attributes.GetAlternativesAttributes(True)
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", i, attr.Name, Bool2JS(False), "", "a" + attrIndex.ToString, 0, "true", "true")
            i += 1
            attrIndex += 1
        Next
        'A1103 ===
        Dim j As Integer = 0
        For Each id As Guid In RiskRegisterAttributesIDs
            If False AndAlso PM.Attributes.GetAttributeByID(id) Is Nothing Then
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", i, ParseString(RiskRegisterAttributesNames(j)), Bool2JS(False), "", "a" + i.ToString, 0, "true", "true")
                i += 1
            End If
            j += 1
        Next
        'A1103 ==

        Dim usersList As New List(Of Tuple(Of Integer, String))

        For Each tGroup As clsCombinedGroup In PM.CombinedGroups.GroupsList
            If IsUserChecked(tGroup.CombinedUserID) Then
                usersList.Add(New Tuple(Of Integer, String)(tGroup.CombinedUserID, ShortString(tGroup.Name, 40, True)))
            End If
        Next

        For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
            Dim tAHPUser As clsUser = PM.GetUserByEMail(tAppUser.UserEmail)
            If tAHPUser IsNot Nothing Then tAppUser.UserName = tAHPUser.UserName
            If tAHPUser IsNot Nothing AndAlso IsUserChecked(tAHPUser.UserID) Then
                usersList.Add(New Tuple(Of Integer, String)(tAHPUser.UserID, ShortString(If(Not String.IsNullOrEmpty(tAppUser.UserName), tAppUser.UserName, tAppUser.UserEmail), 40, True)))
            End If
        Next

        If App.Options.isSingleModeEvaluation OrElse IsWidget Then
            usersList.Clear()
            Dim tUsr = PM.GetUserByID(PM.UserID)
            usersList.Add(New Tuple(Of Integer, String)(PM.UserID, ShortString(If(Not String.IsNullOrEmpty(tUsr.UserName), tUsr.UserName, tUsr.UserEMail), 40, True)))
            'usersList.Add(COMBINED_USER_ID)
        End If

        For Each t As Tuple(Of Integer, String) In usersList
            Dim CurrentUserID As Integer = t.Item1
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", UNDEFINED_INTEGER_VALUE, JS_SafeString(t.Item2), Bool2JS(True), "", "", 0, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 3, ParseString("%%Likelihood%%"), Bool2JS(True), "", String.Format("l{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 4, ParseString("%%Impact%%"), Bool2JS(True), "", String.Format("i{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 5, RiskHeader, Bool2JS(True), "", String.Format("r{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            'retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 6, ParseString("%%Likelihood%% with %%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("lwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            'retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 7, ParseString("%%Impact%% with %%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("iwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            'retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 8, RiskHeader + ParseString(" with %%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("rwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 6, ParseString("%%Likelihood%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("lwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 7, ParseString("%%Impact%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("iwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 8, RiskHeader, Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("rwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Private Sub CalculateOverall(HierarchyID As ECHierarchyID, UserID As Integer, UseControls As Boolean)
        With PM
            .StorageManager.Reader.LoadUserData()
            Dim CurrentHID As Integer = .ActiveHierarchy ' saving current hierarchy id

            Dim oldUseReductions As Boolean = .CalculationsManager.UseReductions
            .CalculationsManager.UseReductions = UseControls

            .ActiveHierarchy = HierarchyID
            .CalculationsManager.Calculate(.CalculationsManager.GetCalculationTargetByUserID(UserID), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy)

            .ActiveHierarchy = CurrentHID ' restoring current hierarchy id
            .CalculationsManager.UseReductions = oldUseReductions
        End With
    End Sub

    Private Class userPriorities
        Public UserID As integer
        Public NodeID As integer
        Public l As Double
        Public g As Double
        Public lwc As Double
        Public gwc As Double
    End Class

    'todo move to a get_hierarchy_data function to api, calculate results optionally
    Public Function GetHierarchyData() As String
        If Not PagesGridWrtObjective.Contains(CurrentPageID) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
        Dim H As clsHierarchy = PM.Hierarchy(HierarchyID)

        Dim usersList = GetCheckedUserIDs()
        Dim usersPrioritiesDict As New Dictionary(Of Integer, Dictionary(Of Integer, userPriorities)) ' user ID -> usersPriorities (obj id - priorities)

        With PM
            .StorageManager.Reader.LoadUserData()

            Dim oldUseControls As Boolean = PM.CalculationsManager.UseReductions
            Dim oldActiveHierarchy As Integer = PM.ActiveHierarchy
            PM.ActiveHierarchy = HierarchyID

            For Each CurrentUserID As Tuple(Of Integer, String) In usersList

                Dim objs As New Dictionary(Of Integer, List(Of Tuple(Of Integer, Integer, NodePriority)))
                Dim alts As New Dictionary(Of Integer, List(Of NodePriority))
                Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs
                Dim wrtParentNode = PM.Hierarchy(HierarchyID).Nodes(0)

                PM.CalculationsManager.UseReductions = False
                PM.CalculationsManager.GetAlternativesGrid(wrtParentNode.NodeID, users, objs, alts)

                Dim list = New Dictionary(Of Integer, userPriorities)
                usersPrioritiesDict.Add(CurrentUserID.Item1, list)

                For Each nodeTuple As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder
                    Dim node = nodeTuple.Item3
                    Dim obj As NodePriority 
                    If node IsNot wrtParentNode Then
                        Dim v = objs(CurrentUserID.item1).Find(Function(x) x.Item1 = node.NodeID AndAlso x.Item2 = node.ParentNodeID)
                        If v IsNot Nothing Then 
                            obj = v.Item3
                        Else
                            obj = New NodePriority With {.CanShowResults = True, .LocalPriority = UNDEFINED_INTEGER_VALUE, .GlobalPriority = UNDEFINED_INTEGER_VALUE}
                        End If
                    Else
                        obj = New NodePriority With {.CanShowResults = True, .LocalPriority = UNDEFINED_INTEGER_VALUE, .GlobalPriority = UNDEFINED_INTEGER_VALUE}
                    End If
                    
                    list.Add(node.NodeID, New userPriorities With {.NodeID = node.NodeID, .UserID = CurrentUserID.item1, .l = If(obj.CanShowResults AndAlso node.RiskNodeType <> RiskNodeType.ntCategory, obj.LocalPriority, UNDEFINED_INTEGER_VALUE), .g = If(obj.CanShowResults AndAlso node.RiskNodeType <> RiskNodeType.ntCategory, obj.GlobalPriority, UNDEFINED_INTEGER_VALUE)})
                Next

                PM.CalculationsManager.UseReductions = True
                PM.CalculationsManager.GetAlternativesGrid(wrtParentNode.NodeID, users, objs, alts)

                For Each nodeTuple As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder
                    Dim node = nodeTuple.Item3
                    Dim obj As NodePriority 
                    If node IsNot wrtParentNode Then
                        Dim v = objs(CurrentUserID.item1).Find(Function(x) x.Item1 = node.NodeID AndAlso x.Item2 = node.ParentNodeID)
                        If v IsNot Nothing Then 
                            obj = v.Item3
                        Else
                            obj = New NodePriority With {.CanShowResults = True, .LocalPriority = UNDEFINED_INTEGER_VALUE, .GlobalPriority = UNDEFINED_INTEGER_VALUE}
                        End If
                    Else
                        obj = New NodePriority With {.CanShowResults = True, .LocalPriority = UNDEFINED_INTEGER_VALUE, .GlobalPriority = UNDEFINED_INTEGER_VALUE}
                    End If
                    list(node.NodeID).lwc = If(obj.CanShowResults AndAlso node.RiskNodeType <> RiskNodeType.ntCategory, obj.LocalPriority, UNDEFINED_INTEGER_VALUE)
                    list(node.NodeID).gwc = If(obj.CanShowResults AndAlso node.RiskNodeType <> RiskNodeType.ntCategory, obj.GlobalPriority, UNDEFINED_INTEGER_VALUE)
                Next
            Next

            PM.CalculationsManager.UseReductions = oldUseControls
            PM.ActiveHierarchy = oldActiveHierarchy
        End With

        MaxPriorityL = Double.MinValue
        MaxPriorityI = Double.MinValue
        MaxPriorityR = Double.MinValue

        For Each nodeTuple As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder
            Dim node As clsNode = nodeTuple.Item3
            retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""guid"":""{1}"",""name"":""{2}"",""pguid"":""{3}"",""is_cat"":{4}", node.NodeID, node.NodeGuidID, JS_SafeString(node.NodeName), If(node.ParentNode Is Nothing, "", node.ParentNode.NodeGuidID.ToString), Bool2JS(node.RiskNodeType = RiskNodeType.ntCategory))
            retVal += String.Format(",""info"":""{0}""", JS_SafeString(Infodoc2Text(PRJ, node.InfoDoc, True)))

            For Each CurrentUserID As Tuple(Of Integer, String) In usersList
                Dim nPriorities = usersPrioritiesDict(CurrentUserID.Item1)
                Dim prty = nPriorities(node.NodeID)
                retVal += String.Format(",""l{0}"":{1},""g{0}"":{2},""lwc{0}"":{3},""gwc{0}"":{4}", CurrentUserID.Item1, If(prty.l <> UNDEFINED_INTEGER_VALUE, JS_SafeNumber(prty.l), "undefined"), If(prty.g <> UNDEFINED_INTEGER_VALUE, JS_SafeNumber(prty.g), "undefined"), If(prty.lwc <> UNDEFINED_INTEGER_VALUE, JS_SafeNumber(prty.lwc), "undefined"), If(prty.gwc <> UNDEFINED_INTEGER_VALUE, JS_SafeNumber(prty.gwc), "undefined"))
            Next

            retVal += "}"
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetHierarchyColumns() As String
        If Not PagesGridWrtObjective.Contains(CurrentPageID) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

        retVal += String.Format("[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 1, ParseString(If(HierarchyID = ECHierarchyID.hidLikelihood, "%%Objective(l)%% Name", "%%Objective(i)%% Name")), Bool2JS(True), "", "name", 0, "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 0, "ID", Bool2JS(False), "", "id", 0, "false", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 2, ParseString("Description"), Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs), "", "info", 0, "false", "false")

        Dim usersList = GetCheckedUserIDs()

        For Each t As Tuple(Of Integer, String) In usersList
            Dim CurrentUserID As Integer = t.Item1
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", UNDEFINED_INTEGER_VALUE, JS_SafeString(t.Item2), Bool2JS(True), "", "", 0, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 3, "Local", Bool2JS(True), "", String.Format("l{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 4, "Global", Bool2JS(True), "", String.Format("g{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            If PagesWithControlsList.Contains(CurrentPageID) AndAlso Not PagesToObjectivesWithControls.Contains(CurrentPageID) Then 
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 6, ParseString("Local w/%%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("lwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 7, ParseString("Global w/%%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("gwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            End If
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Private Function AnySubObjectiveContributes(Node As clsNode, AltID As Integer) As Boolean
        If Node IsNot Nothing Then
            ' If likelihood and "Sources" is selected then count all nodes
            Dim lkhGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)
            If Node Is lkhGoalNode Then Return True

            ' otherwise check contributions
            Dim ContributedAlternatives As List(Of clsNode) = If(Node.IsTerminalNode, Node.GetContributedAlternatives, Nothing)
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

    Function GetCheckedUserIDs() As List(Of Tuple(Of Integer, String))
        Dim usersList As New List(Of Tuple(Of Integer, String))

        If App.Options.isSingleModeEvaluation OrElse IsWidget Then
            usersList.Clear()
            usersList.Add(new Tuple(Of Integer, String)(PM.UserID, ""))
            Return usersList
        End If

        For Each tGroup As clsCombinedGroup In PM.CombinedGroups.GroupsList
            If IsUserChecked(tGroup.CombinedUserID) Then
                usersList.Add(New Tuple(Of Integer, String)(tGroup.CombinedUserID, ShortString(tGroup.Name, 40, True)))
            End If
        Next

        For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
            Dim tAHPUser As clsUser = PM.GetUserByEMail(tAppUser.UserEmail)
            If tAHPUser IsNot Nothing AndAlso IsUserChecked(tAHPUser.UserID) Then
                usersList.Add(New Tuple(Of Integer, String)(tAHPUser.UserID, ShortString(If(Not String.IsNullOrEmpty(tAppUser.UserName), tAppUser.UserName, tAppUser.UserEmail), 40, True)))
            End If
        Next

        Return usersList
    End Function

    Public Function GetCategoriesData() As String
        Dim retVal As String = ""

        With PM
            If .Attributes IsNot Nothing Then
                Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
                If attr IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                    Dim cat_attr_vals As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                    If cat_attr_vals IsNot Nothing Then
                        For Each item As clsAttributeEnumerationItem In cat_attr_vals.Items
                            retVal += CStr(If(retVal <> "", ",", "")) + String.Format("['{0}','{1}']", item.ID, JS_SafeString(item.Value))
                        Next
                    End If
                End If
            End If
        End With

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetCategoryByName(CategoryName As String) As clsAttributeEnumerationItem
        Dim retVal As clsAttributeEnumerationItem = Nothing

        With PM
            If .Attributes IsNot Nothing Then
                Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
                If attr IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                    Dim cat_attr_vals As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                    If cat_attr_vals IsNot Nothing Then
                        For Each item As clsAttributeEnumerationItem In cat_attr_vals.Items
                            If item.Value = CategoryName Then retVal = item
                        Next
                    End If
                End If
            End If
        End With

        Return retVal
    End Function

    Public Function ControlTypeToString(ctrlType As ControlType) As String
        Dim retVal As String = ""
        Select Case ctrlType
            Case ControlType.ctUndefined
                retVal = ParseString("Undefined")
            Case ControlType.ctCause
                retVal = ParseString("%%Source%%")
            Case ControlType.ctCauseToEvent
                retVal = ParseString("%%Vulnerability%%")
            Case ControlType.ctConsequence ' OBSOLETE
                retVal = ParseString("Consequence")
            Case ControlType.ctConsequenceToEvent
                retVal = ParseString("Consequence")
        End Select
        Return retVal
    End Function

    Public Function GetControlData(ctrl As clsControl) As String
        Dim retVal As String = ""
        Dim lblYes As String = ResString("lblYes")

        Dim ctrl_type As String = ControlTypeToString(ctrl.Type)

        'Dim dd As Integer = DecimalDigits

        Dim appl_count As String = "0"
        Dim appl_names As String = ""
        Dim appl_values As String = ""
        Dim nlSeparator As String = "\n"
        If ctrl.Assignments IsNot Nothing Then
            appl_count = CStr(ctrl.Assignments.Count)
            For Each item In ctrl.Assignments
                Select Case ctrl.Type
                    Case ControlType.ctCause
                        Dim node As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(item.ObjectiveID)
                        If node IsNot Nothing Then
                            appl_names += CStr(If(appl_names = "", "", nlSeparator)) + JS_SafeString(node.NodeName)
                            'appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(Double2String(item.Value, dd))
                            appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(item.Value)
                        End If
                    Case ControlType.ctConsequence
                        Dim node As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(item.ObjectiveID)
                        If node IsNot Nothing Then
                            appl_names += CStr(If(appl_names = "", "", nlSeparator)) + JS_SafeString(node.NodeName)
                            'appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(Double2String(item.Value, dd))
                            appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(item.Value)
                        End If
                    Case ControlType.ctCauseToEvent
                        Dim e As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(item.EventID)
                        Dim node As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(item.ObjectiveID)
                        If e IsNot Nothing AndAlso node IsNot Nothing Then
                            appl_names += CStr(If(appl_names = "", "", nlSeparator)) + JS_SafeString(node.NodeName) + " / " + JS_SafeString(e.NodeName)
                            'appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(Double2String(item.Value, dd))
                            appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(item.Value)
                        End If
                    Case ControlType.ctConsequenceToEvent
                        Dim e As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(item.EventID)
                        Dim node As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(item.ObjectiveID)
                        If e IsNot Nothing AndAlso node IsNot Nothing Then
                            appl_names += CStr(If(appl_names = "", "", nlSeparator)) + JS_SafeString(node.NodeName) + " / " + JS_SafeString(e.NodeName)
                            'appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(Double2String(item.Value, dd))
                            appl_values += CStr(If(appl_values = "", "", nlSeparator)) + JS_SafeNumber(item.Value)
                        End If
                End Select
            Next
        End If
        'retVal = String.Format("[{0},'{1}','{2}','{3}','{4}',{5},'{6}','{7}','{8}','{9}']", Treatments.IndexOf(ctrl) + 1, JS_SafeString(ctrl.Name), CStr(If(ctrl.Active, lblYes, "")), ctrl_type, CostString(CDbl(If(ctrl.Cost <> UNDEFINED_INTEGER_VALUE, ctrl.Cost, 0)), CostDecimalDigits), appl_count, appl_names, appl_values, JS_SafeString(GetControlInfodoc(PRJ, ctrl, True)), ctrl.ID.ToString)
        retVal = String.Format("'id':{0},'name':'{1}','info':'{2}','selected':'{3}','type':'{4}','cost':'{5}','apps_count':{6},'apps':'{7}','value':'{8}'", Treatments.IndexOf(ctrl) + 1, JS_SafeString(ctrl.Name), JS_SafeString(Infodoc2Text(PRJ, ctrl.InfoDoc, True)), CStr(If(ctrl.Active, lblYes, "")), ctrl_type, CostString(CDbl(If(ctrl.Cost <> UNDEFINED_INTEGER_VALUE, ctrl.Cost, 0)), CostDecimalDigits), appl_count, appl_names, appl_values)

        Dim Attributes As List(Of clsAttribute) = PM.Attributes.GetControlsAttributes(True)
        Dim attrIndex As Integer = 0

        For Each attr As clsAttribute In Attributes
            Dim attrStr As String = PM.Attributes.GetAttributeValueString(attr.ID, ctrl.ID)
            retVal += String.Format(",'a{0}':'{1}'", attrIndex, attrStr)
            attrIndex += 1
        Next

        Return String.Format("{{{0}}}", retVal)
    End Function

    Public Function GetControlsData() As String
        If RegisterView <> RegisterViews.rvTreatmentRegister Then Return "[]"
        Dim retVal As String = ""

        ControlsListReset()
        Treatments = ControlsList

        For Each ctrl As clsControl In Treatments
            retVal += CStr(If(retVal = "", "", ",")) + GetControlData(ctrl)
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetTreatmentsColumns() As String
        If RegisterView <> RegisterViews.rvTreatmentRegister Then Return "[]"
        Dim retVal As String = ""
        retVal = String.Format("[{0},'{1}',{2},'{3}','{4}']", 0, "ID", Bool2JS(True), "left", "id")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 1, ParseString("%%Control%% Name"), Bool2JS(True), "left", "name")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 2, ParseString("Description"), Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs), "left", "info")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 3, ParseString("Selected"), Bool2JS(True), "center", "selected")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 4, ParseString("%%Control%% for"), Bool2JS(True), "center", "type")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 5, ParseString("Cost"), Bool2JS(True), "right", "cost")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 6, ParseString("Application count"), Bool2JS(True), "center", "apps_count")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 7, ParseString("Application"), Bool2JS(True), "left", "apps")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 8, ParseString("Effectiveness"), Bool2JS(True), "left", "value")

        Dim i As Integer = 9
        Dim attrIndex As Integer = 0
        For Each attr As clsAttribute In PM.Attributes.GetControlsAttributes(True)
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", i, attr.Name, Bool2JS(False), "left", "a" + attrIndex.ToString)
            i += 1
            attrIndex += 1
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetAcceptanceData() As String
        If RegisterView <> RegisterViews.rvAcceptanceRegister Then Return "[]"

        Dim retVal As String = ""
        Dim index As Integer = 1
        Dim H As clsHierarchy = PM.Hierarchy(ECHierarchyID.hidLikelihood)

        ' Treatments for Sources
        Dim ctrl_type As String = ParseString("%%Source%%")
        For Each source As clsNode In H.Nodes
            If source.IsTerminalNode OrElse (source.ParentNode Is Nothing AndAlso (source.ParentNodesGuids Is Nothing OrElse source.ParentNodesGuids.Count = 0)) Then
                Dim hasTreatments As Boolean = False

                'check controls for Sources
                For Each ctrl As clsControl In Treatments.Where(Function(c) c.Type = ControlType.ctCause)
                    If Not hasTreatments Then
                        For Each item As clsControlAssignment In ctrl.Assignments
                            If item.ObjectiveID.Equals(source.NodeGuidID) Then
                                hasTreatments = True
                                Exit For
                            End If
                        Next
                    End If
                Next

                If Not hasTreatments Then
                    retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""type"":""{1}"",""app"":""{2}""}}", index, ctrl_type, JS_SafeString(source.NodeName))
                    index += 1
                End If

            End If
        Next

        'Treatments for Vulnerabilities
        ctrl_type = ParseString("%%Vulnerability%%")
        For Each source As clsNode In H.Nodes

            If source.IsTerminalNode OrElse (source.ParentNode Is Nothing AndAlso (source.ParentNodesGuids Is Nothing OrElse source.ParentNodesGuids.Count = 0)) Then
                For Each alt As clsNode In source.GetContributedAlternatives

                    Dim hasTreatments As Boolean = False
                    'check controls for Sources
                    For Each ctrl As clsControl In Treatments.Where(Function(c) c.Type = ControlType.ctCauseToEvent)
                        If Not hasTreatments Then
                            For Each item As clsControlAssignment In ctrl.Assignments
                                If item.ObjectiveID.Equals(source.NodeGuidID) AndAlso item.EventID.Equals(alt.NodeGuidID) Then
                                    hasTreatments = True
                                    Exit For
                                End If
                            Next
                        End If
                    Next

                    If Not hasTreatments Then
                        retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""type"":""{1}"",""app"":""{2}""}}", index, ctrl_type, JS_SafeString(source.NodeName) + " / " + JS_SafeString(alt.NodeName))
                        index += 1
                    End If

                Next
            End If

        Next

        'Treatments for Vulnerabilities
        ctrl_type = ParseString("%%Impact%%")
        H = PM.Hierarchy(ECHierarchyID.hidImpact)
        For Each obj As clsNode In H.Nodes

            If obj.IsTerminalNode Then
                For Each alt As clsNode In obj.GetContributedAlternatives

                    Dim hasTreatments As Boolean = False
                    'check controls for Sources
                    For Each ctrl As clsControl In Treatments.Where(Function(c) c.Type = ControlType.ctConsequenceToEvent)
                        If Not hasTreatments Then
                            For Each item As clsControlAssignment In ctrl.Assignments
                                If item.ObjectiveID.Equals(obj.NodeGuidID) AndAlso item.EventID.Equals(alt.NodeGuidID) Then
                                    hasTreatments = True
                                    Exit For
                                End If
                            Next
                        End If
                    Next

                    If Not hasTreatments Then
                        retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""type"":""{1}"",""app"":""{2}""}}", index, ctrl_type, JS_SafeString(obj.NodeName) + " / " + JS_SafeString(alt.NodeName))
                        index += 1
                    End If

                Next
            End If

        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetAcceptanceColumns() As String
        If RegisterView <> RegisterViews.rvAcceptanceRegister Then Return "[]"

        Dim retVal As String = ""
        retVal = String.Format("[{0},'{1}',{2},'{3}','{4}']", 0, "ID", Bool2JS(True), "left", "id")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 1, ParseString("%%Control%% Type"), Bool2JS(True), "center", "type")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}']", 2, ParseString("Application"), Bool2JS(True), "left", "app")
        Return String.Format("[{0}]", retVal)
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = GetParam(args, "action").ToLower
        Dim tResult As String = CStr(If(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction
            Case ACTION_DECIMALS
                PM.Parameters.DecimalDigits = CInt(GetParam(args, "value").Trim())
                tResult = String.Format("['{0}',{1}]", sAction, AllCallbackData())
            Case ACTION_CREATE_DEFAULT_ATTRIBUTE
                Dim isVisible As Boolean = Str2Bool(GetParam(args, "state"))
                Dim ColIndex As Integer = CInt(GetParam(args, "col_index"))
                'Dim i As Integer = COL_ATTRIBUTES_START
                Dim Attributes As List(Of clsAttribute) = PM.Attributes.GetAlternativesAttributes(True)
                'i += Attributes.Count
                Dim gID As String = ""

                ' create column if possible  
                If RegisterView = RegisterViews.rvRiskRegister OrElse RegisterView = RegisterViews.rvRiskRegisterWithControls Then
                    For Each id As Guid In RiskRegisterAttributesIDs
                        If PM.Attributes.GetAttributeByID(id) Is Nothing Then
                            'If ColIndex = i Then
                            gID = id.ToString
                            If isVisible Then
                                ' create an attribute
                                Dim sName As String = ""
                                Select Case id
                                    Case ATTRIBUTE_RISK_REGISTER_EVENT_HISTORY_ID
                                        sName = ParseString(ATTRIBUTE_RISK_REGISTER_EVENT_HISTORY_NAME)
                                        PM.Attributes.AddAttribute(ATTRIBUTE_RISK_REGISTER_EVENT_HISTORY_ID, sName, AttributeTypes.atAlternative, AttributeValueTypes.avtString, "", False, Nothing)
                                    Case ATTRIBUTE_RISK_REGISTER_EVENT_RECOMMENDATIONS_ID
                                        sName = ParseString(ATTRIBUTE_RISK_REGISTER_EVENT_RECOMMENDATIONS_NAME)
                                        PM.Attributes.AddAttribute(ATTRIBUTE_RISK_REGISTER_EVENT_RECOMMENDATIONS_ID, sName, AttributeTypes.atAlternative, AttributeValueTypes.avtString, , False, Nothing)
                                    Case ATTRIBUTE_RISK_REGISTER_OWNER_ID
                                        sName = ParseString(ATTRIBUTE_RISK_REGISTER_OWNER_NAME)
                                        PM.Attributes.AddAttribute(ATTRIBUTE_RISK_REGISTER_OWNER_ID, sName, AttributeTypes.atAlternative, AttributeValueTypes.avtEnumeration, , False, Nothing)
                                    Case ATTRIBUTE_RISK_REGISTER_RISK_CATEGORIES_ID
                                        sName = ParseString(ATTRIBUTE_RISK_REGISTER_RISK_CATEGORIES_NAME)
                                        PM.Attributes.AddAttribute(ATTRIBUTE_RISK_REGISTER_RISK_CATEGORIES_ID, sName, AttributeTypes.atAlternative, AttributeValueTypes.avtEnumerationMulti, , False, Nothing)
                                    Case ATTRIBUTE_RISK_REGISTER_FUNCTIONAL_AREAS_ID
                                        sName = ParseString(ATTRIBUTE_RISK_REGISTER_FUNCTIONAL_AREAS_NAME)
                                        PM.Attributes.AddAttribute(ATTRIBUTE_RISK_REGISTER_FUNCTIONAL_AREAS_ID, sName, AttributeTypes.atAlternative, AttributeValueTypes.avtEnumerationMulti, , False, Nothing)
                                    Case ATTRIBUTE_RISK_REGISTER_STAKEHOLDERS_ID
                                        sName = ParseString(ATTRIBUTE_RISK_REGISTER_STAKEHOLDERS_NAME)
                                        PM.Attributes.AddAttribute(ATTRIBUTE_RISK_REGISTER_STAKEHOLDERS_ID, sName, AttributeTypes.atAlternative, AttributeValueTypes.avtEnumerationMulti, , False, Nothing)
                                End Select
                                WriteAttributes(PRJ, "Added predefined attribute", sName)   ' D3731
                                Exit For
                            End If
                            'End If
                            'i += 1
                        End If
                    Next
                End If
                tResult = String.Format("['{0}']", sAction)
            Case "timestamp"
                PM.Parameters.Riskion_Register_Timestamp = Param2Bool(args, "chk")
                PM.Parameters.Save()
                tResult = String.Format("['{0}','{1}']", sAction, If(PM.Parameters.Riskion_Register_Timestamp, JS_SafeString(String.Format("(As of: {0})", DateTime.Now.ToString)), ""))
            Case "control_type"
                Dim ctrlID As Guid = New Guid(GetParam(args, "id"))
                Dim value As ControlType = CType(CInt(GetParam(args, "value")), ControlType)
            Case "show_sim_results"
                'PM.Parameters.Riskion_Use_Simulated_Values = If(Str2Bool(GetParam(args, "value", True)), 3, 0)
                PM.CalculationsManager.UseSimulatedValues = Str2Bool(GetParam(args, "value", True))
                PM.Parameters.Save()
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())
            Case "selected_users"
                Dim sIDs As String = GetParam(args, "value")
                If sIDs = "" Then sIDs = UNDEFINED_USER_ID.ToString
                StringSelectedUsersAndGroupsIDs = sIDs
                tResult = String.Format("['{0}',{1},{2},{3},{4}]", sAction, GetEventsData(), GetEventsColumns(), GetHierarchyData(), GetHierarchyColumns())
            Case "selected_node"
                Dim sGuid As String = GetParam(args, "value", True)
                SelectedHierarchyNodeID = New Guid(sGuid)
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())
            Case "selected_multiple_nodes"
                Dim sGuids As String = GetParam(args, "value", True)
                If sGuids = "undefined" Then 
                    MultipleSelectedHierarchyNodeGUIDS = Nothing
                    tResult = String.Format("['{0}',{1}]", "selected_node", GetEventsData()) ' we reload the data for a single SelectedHierarchyNodeID
                Else
                    MultipleSelectedHierarchyNodeGUIDS = Param2GuidList(sGuids)
                    tResult = String.Format("['{0}',{1},{2}]", sAction, GetEventsData(), GetEventsColumns())
                End If
            Case "probability_calculation_mode"
                PM.CalculationsManager.LikelihoodsCalculationMode = CType(CInt(GetParam(args, "value", True)), LikelihoodsCalculationMode)
                App.ActiveProject.MakeSnapshot("Likelihoods calculation mode changed", PM.CalculationsManager.LikelihoodsCalculationMode.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())
            Case "diluted_mode"
                PM.CalculationsManager.ConsequenceSimulationsMode = CType(CInt(GetParam(args, "value", True)), ConsequencesSimulationsMode)
                App.ActiveProject.MakeSnapshot("Consequence simulation mode changed", PM.CalculationsManager.ConsequenceSimulationsMode.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())
            Case "wrt_calculation_mode"
                PM.CalculationsManager.ShowDueToPriorities = CInt(GetParam(args, "value", True)) = 1
                App.ActiveProject.MakeSnapshot("Show due to priorities mode changed", PM.CalculationsManager.ShowDueToPriorities.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())
            Case "show_dollar_value"
                ShowDollarValue = Str2Bool(GetParam(args, "chk", True))
                App.ActiveProject.MakeSnapshot("Show monetary values setting changed", ShowDollarValue.ToString)
                tResult = String.Format("['{0}']", sAction)
            Case "show_total_risk"
                ShowTotalRisk = Str2Bool(GetParam(args, "value", True))
                App.ActiveProject.MakeSnapshot("Show total risk setting changed", ShowTotalRisk.ToString)
                tResult = String.Format("['{0}']", sAction)
            Case "show_cents"
                ShowCents = Str2Bool(GetParam(args, "value", True))
                App.ActiveProject.MakeSnapshot("Show cents for monetary values setting changed", ShowCents.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())
            Case "event_id_mode"
                PM.Parameters.NodeVisibleIndexMode = CType(Param2Int(args, "value"), IDColumnModes)
                PM.Parameters.Save()
                tResult = String.Format("['{0}',{1},{2}]", sAction, GetEventsData(), GetEventsColumns())
            Case "set_dollar_value"
                Dim sTarget As String = GetParam(args, "target", True)
                Dim sValue As String = GetParam(args, "value", True)
                Dim dValue As Double
                If Not String.IsNullOrEmpty(sValue) AndAlso String2Double(sValue, dValue) Then
                    DollarValue = dValue
                End If
                If DollarValueTarget <> sTarget Then DollarValueTarget = sTarget

                RA.RiskOptimizer.InitOriginalValues()

                Dim DollarValueDefined As Boolean = Not (DollarValue = UNDEFINED_INTEGER_VALUE OrElse DollarValueTarget = UNDEFINED_STRING_VALUE)
                If DollarValueDefined Then
                    ShowDollarValue = True
                End If
                _DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE

                App.ActiveProject.MakeSnapshot("Dollar value of enterprise changed", (If(DollarValueDefined, DollarValue.ToString, "")).ToString)

                tResult = String.Format("['{0}',{1},{2},'{3}',{4},{5}]", sAction, GetAllObjsData(), JS_SafeNumber(DollarValue), DollarValueTarget, JS_SafeNumber(DollarValueOfEnterprise), Bool2JS(DollarValueDefined))
            Case "select_events"
                Dim StringSelectedEventIDs = GetParam(args, "event_ids")
                For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                    alt.Enabled = StringSelectedEventIDs.Contains(alt.NodeGuidID.ToString)
                Next
                PRJ.SaveStructure("Save active alternatives")
                tResult = String.Format("['{0}',{1}]", sAction, GetEventsData())

            Case "num_sim"
                Dim sVal As String = GetParam(args, "value")
                Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                If Not String.IsNullOrEmpty(sVal) Then
                    If Integer.TryParse(sVal, iVal) Then 'AndAlso LEC.NumberOfSimulations <> iVal AndAlso iVal > 0 AndAlso iVal <= _MAX_NUM_SIMULATIONS Then 
                        PM.RiskSimulations.NumberOfTrials = iVal
                    End If
                End If
                App.ActiveProject.MakeSnapshot("Number of simulations changed", PM.RiskSimulations.NumberOfTrials.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, iVal)
            Case "rand_seed"
                Dim sVal As String = GetParam(args, "value")
                Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                If Not String.IsNullOrEmpty(sVal) Then
                    If Integer.TryParse(sVal, iVal) AndAlso PM.RiskSimulations.RandomSeed <> iVal AndAlso iVal > 0 Then
                        PM.RiskSimulations.RandomSeed = iVal
                    End If
                End If
                App.ActiveProject.MakeSnapshot("Random seed changed", PM.RiskSimulations.RandomSeed.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, iVal)
            Case "keep_rand_seed"
                Dim bVal As Boolean = Param2Bool(args, "value")
                PM.RiskSimulations.KeepRandomSeed = bVal
                App.ActiveProject.MakeSnapshot("Keep random seed seeting changed", PM.RiskSimulations.KeepRandomSeed.ToString)
                tResult = String.Format("['{0}',{1}]", sAction, Bool2JS(bVal))
            Case "use_source_groups"
                PM.RiskSimulations.UseSourceGroups = Param2Bool(args, "value")
                App.ActiveProject.MakeSnapshot("Use source groups changed", PM.RiskSimulations.UseSourceGroups.ToString)
                tResult = String.Format("['{0}']", sAction)
            Case "use_event_groups"
                PM.RiskSimulations.UseEventsGroups = Param2Bool(args, "value")
                App.ActiveProject.MakeSnapshot("Use event groups changed", PM.RiskSimulations.UseEventsGroups.ToString)
                tResult = String.Format("['{0}']", sAction)
            Case "get_optimal_unmber_of_trials"
                PM.RiskSimulations.NumberOfTrials = PM.RiskSimulations.GetEstimatedNumberOfSimulations()
                tResult = String.Format("['get_optimal_unmber_of_trials',{0}]", JS_SafeNumber(PM.RiskSimulations.NumberOfTrials))
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Public Function GetAllObjsData() As String
        Dim retVal As String = ""
        For Each Node As clsNode In PM.Hierarchy(ECHierarchyID.hidImpact).Nodes
            Dim sDollValue As String = ""
            If Node.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso Node.DollarValue <> 0 Then sDollValue = String.Format(" ({0})", CostString(Node.DollarValue, CostDecimalDigits, True))

            Dim margin As String = ""
            For i As Integer = 0 To Node.Level 
                margin += "&nbsp;&nbsp;"
            Next
            margin = If(margin = "", "", margin + "• ")
            retVal += If(retVal = "", "", ",") + String.Format("{{""key"":""{0}"",""html"":""{1}{2}{3}"",""title"":""{5}{3}"",""text"":""{6}{3}"", ""dollar_value"":{4}}}", Node.NodeGuidID, margin, ShortString(JS_SafeString(If(Node.ParentNode Is Nothing, ResString("lblEnterprise"), Node.NodeName)), 45, False), SafeFormString(sDollValue), JS_SafeNumber(Node.DollarValue), JS_SafeString(Node.NodeName), ShortString(JS_SafeString(If(Node.ParentNode Is Nothing, ResString("lblEnterprise"), Node.NodeName)), 45, True))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Private Function AllCallbackData() As String
        Select Case RegisterView
            Case RegisterViews.rvRiskRegister, RegisterViews.rvRiskRegisterWithControls
                Return GetEventsData()
            Case RegisterViews.rvTreatmentRegister
                Return GetControlsData()
            Case RegisterViews.rvAcceptanceRegister
                Return GetAcceptanceData()
            Case Else
                Return ""
        End Select
    End Function

    Private Sub RiskRegisterPage_Init(sender As Object, e As EventArgs) Handles Me.Init
        isPM = CanEditActiveProject AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)   ' D6672

        If (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL) AndAlso Not isAJAX Then
            Dim tEvalType As ecEvaluationStepType = ecEvaluationStepType.RiskResults
            Select Case CurrentPageID
                Case _PGID_RISK_PLOT_OVERALL
                    tEvalType = ecEvaluationStepType.HeatMap
            End Select
            QuickHelpParams = String.Format("&qh={0}", CInt(tEvalType))
            Dim QuickHelpContent As String = PM.PipeParameters.PipeMessages.GetEvaluationQuickHelpForCustom(PM, tEvalType, -1, AutoShowQuickHelp).Trim
            QuickHelpEmpty = QuickHelpContent = ""
            If Not isCallback AndAlso Not IsPostBack AndAlso clsPipeMessages.OPT_QUICK_HELP_AVAILABLE AndAlso AutoShowQuickHelp AndAlso GetCookie(_COOKIE_QH_AUTOSHOW + App.ProjectID.ToString, "") <> "0" AndAlso Not QuickHelpEmpty AndAlso (IsWidget OrElse CheckVar("back") <> "") Then
                If CheckShow_QuickHelp(QuickHelpContent) Then ClientScript.RegisterStartupScript(GetType(String), "PWHelp", "setTimeout(function () { ShowIdleQuickHelp(); }, 500);", True) ' D6672
            End If
        End If
    End Sub

    ' D6663 ===
    Public Function GetPipeButtons() As String
        Dim sButtons As String = ""
        Dim sBack As String = CheckVar("back", "")
        If Not IsWidget AndAlso (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL) AndAlso sBack <> "" Then
            Dim isRiskResults = CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS
            Dim sHID As String = CheckVar("h", "")
            Dim sParams = If(sBack = "" AndAlso isRiskResults, "", "back=" + sBack)
            If sHID <> "" Then sParams += If(sParams = "", "", "&") + "h=" + sHID
            Dim isImpact As Boolean = sHID = CInt(ECHierarchyID.hidImpact).ToString
            sButtons = String.Format("<input name='btnBackToEval' class='button' style='width:6em;' onclick='loadURL(""{0}""); return false;' type='button' value='{1}*'/>", PageURL(If(isRiskResults, _PGID_EVALUATION, _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS), If(isRiskResults, "passcode=" + App.ActiveProject.Passcode(isImpact), sParams)), SafeFormString(ResString("btnEvaluationPrev")))
            If isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE Then
                sButtons += String.Format("<input name='btnNextToPlot' class='button' style='width:6em; margin-left:1ex;' onclick='loadURL(""{0}""); return false;' type='button' value='{1}**'/>", PageURL(_PGID_RISK_PLOT_OVERALL, sParams), SafeFormString(ResString("btnEvaluationNext")))
            End If
            'sButtons += String.Format("<div class='text small gray' style='text-align:center'>*{0}{1}{2}</div>", ResString(If(isRiskResults, If(isImpact, "lblPrevLikelihood", "lblNextImpact"), "lblNextRiskResults")), If(isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE, "<br>**", ""), If(isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE, ResString("lblNextHeatMap"), ""))
            sButtons += String.Format("<div class='text small gray' style='text-align:center'>*{0}{1}{2}</div>", ResString(If(isRiskResults, "lblNextImpact", "lblNextRiskResults")), If(isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE, "<br>**", ""), If(isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE, ResString("lblNextHeatMap"), ""))  ' D7349
            sButtons = String.Format("<div style='display:inline-block; float:right; text-align:right; margin:4px 1ex; text-align:center' id='divPipeButtons'>{0}</div>", sButtons)
        End If
        Return sButtons
    End Function

    Public Function GetQuickHelpIcons() As String
        Dim sIcons As String = ""
        If clsPipeMessages.OPT_QUICK_HELP_AVAILABLE AndAlso QuickHelpParams <> "" Then
            ' D6665 ===
            If isPM Then sIcons += String.Format("<a href='' onclick=""editQuickHelp('{1}'); return false;""><img src='{0}edit_small.gif' width='16' height='16' border='0' title='Edit Quick Help' style='margin-left:2px; float:right'/></a>", ImagePath, JS_SafeString(QuickHelpParams)) ' D6674
            If isPM OrElse Not QuickHelpEmpty Then sIcons += String.Format("<a href='' id='lnkQHVew' onclick=""showQuickHelp('{0}', false, {1}); return false;""><img src='{2}help/{3}' id='imgQH' width='16' height='16' border='0' title='Show Quick Help' style='float:right'/></a>", JS_SafeString(QuickHelpParams), Bool2JS(isPM), ImagePath, If(QuickHelpEmpty, "icon-question_dis.png", "icon-question.png")) ' D6668 + D6672
            If sIcons <> "" Then
                sIcons = String.Format("<div style='float:right;{1}' id='divQHIcons'>{0}</div>", sIcons, If(IsWidget, " margin-top:4px;", ""))  ' D6674
                'If IsWidget Then divHeader.Attributes("style") = "margin-left:34px; margin-top: 1ex;"
            End If
            ' D6665 ==
        End If
        Return sIcons
    End Function
    ' D6663 ==

    ' D6702 ===
    Public Function GetScore() As String
        Dim Score As Double = 0
        Dim Sum As Integer = 0
        Dim Idx As Integer = 1
        Dim UserRankAttr As clsAttribute = PM.Attributes.GetAttributeByID(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID)
        Dim Nodes As IEnumerable(Of clsNode) = PM.ActiveAlternatives.TerminalNodes.OrderByDescending(Function(n) n.RiskValue)
        If Nodes.Count > 0 Then
            For Each alt As clsNode In Nodes
                'If Not SkipAlternative(alt, CurrentFiltersList) Then
                    Dim Rank As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, alt.NodeGuidID, UserRankAttr, PM.UserID))
                    Sum += (If(Rank > 0, Math.Abs(Idx - Rank), 0))
                    Idx += 1
                'End If
            Next
            Score = Sum / (Idx - 1)
        End If
        'Return String.Format("<h6 style='max-width:600px; text-align:center;'>{0}</h6>", String.Format(ResString(If(Score < 2, "lblRiskResultScore0", "lblRiskResultScore2")), Score.ToString("F1")))
        'Return String.Format("<h6 class='error'>{0}: {1}</h6><div class='note' style='max-width:600px; margin:0px auto 1em auto; text-align:center;'>{2}</div>", ResString("lblRiskResultScore"), Score.ToString("F1"), ResString(If(Score < 2, "lblRiskResultScore0", "lblRiskResultScore2")))
        Return String.Format("<h6 class='error'>{0}: {1}</h6><div class='note' style='max-width:600px; margin:0px auto 1em auto; text-align:center;'>{2}</div>", ResString("lblRiskResultScore"), Score.ToString("F1"), ResString(If(Score = 2, "lblRiskResultScoreAverage", If(Score < 2, "lblRiskResultScore0", "lblRiskResultScore2"))))
    End Function
    ' D6702 ==

End Class