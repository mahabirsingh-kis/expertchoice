Partial Class RiskPlotPage
    Inherits clsComparionCorePage

    Public Const d As Integer = 4 ' decimal places
    Public Const _MAX_DATAPOINTS As Integer = 1000

    Public Sub New()
        MyBase.New(_PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT)
    End Sub

    Public Function GetDefaultMessage(isLoading As Boolean) As String
        Return String.Format("<table border=0 class='whole'><tr><td valign='middle' align='center'><h6 class='chart_loading_msg'>{0}</h6></td></tr></table>", If(isLoading, ResString("lblPleaseWait"), GetEmptyMessage())) ' D4080 + A1327
    End Function

    Public Function GetTitle(Optional forDataStep As Boolean = False) As String
        Dim sNodePath As String = ""
        Dim tSelectedHierarchyNodeID As Guid = SelectedHierarchyNodeID
        If Not tSelectedHierarchyNodeID.Equals(Guid.Empty) Then
            Dim wrtNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tSelectedHierarchyNodeID)
            If wrtNode Is Nothing Then wrtNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tSelectedHierarchyNodeID)
            If wrtNode IsNot Nothing Then 
                sNodePath = StringService.GetNodePathString(wrtNode, ecNodePathFormat.FullPath)
            End If
        End If
        If forDataStep Then Return String.Format("{0}<br/>", SafeFormString(PRJ.ProjectName)) + String.Format("Simulation{0}", If(sNodePath <> "", String.Format(" ({0})", sNodePath), "")) + CStr(IIf(SimulationUserID = COMBINED_USER_ID, String.Format(" for {0}", SimulationUserName), String.Format(" for <b>{0}</b>", SimulationUserName)))
        Return String.Format("Model '{0}'<br/>", SafeFormString(PRJ.ProjectName)) + String.Format(CStr(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel, ParseString("%%Risk%% Exceedance Curve{0}"), "Loss Exceedance Curve{0}")), IIf(sNodePath <> "", String.Format(" ({0})", sNodePath), "")) + CStr(IIf(SimulationUserID = COMBINED_USER_ID, String.Format(" for {0}", SimulationUserName), String.Format(" for <b>{0}</b>", SimulationUserName))) ' D6798
    End Function

    Public ReadOnly Property CanUseControls As Boolean 
        Get
            Return Not PRJ.isMyRiskRewardModel AndAlso CheckVar("can_use_controls", False)
        End Get
    End Property

    Public Function HowControlsSelected(Optional ShowFullInfo As Boolean = False) As String
        If Not PRJ.isMyRiskRewardModel Then
            Dim sCostOfControls As String = CostString(PM.Controls.CostOfFundedControls(), CostDecimalDigits, True) ' Total cost of all active controls 'A1303
            Dim sNumOfControls As String = CStr(PM.Controls.EnabledControls.Sum(Function(ctrl) If(ctrl.Active, 1, 0)))
            Dim sHowSelected As String = "Manually selected. "
            'If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 1 Then sHowSelected = "Optimized based on simulated input and computed output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, DecimalDigits, True)
            'If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 2 Then sHowSelected = "Optimized based on computed input and simulated output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, DecimalDigits, True)
            'If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 3 Then sHowSelected = "Optimized based on simulated input and output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, DecimalDigits, True)
            If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 Then sHowSelected = "Optimized with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, PM.Parameters.DecimalDigits, True) + ". "
            sHowSelected += sNumOfControls + ParseString(" %%controls%% selected.")
            If ShowFullInfo Then 
                Dim tbl As String = String.Format("<table class='text'><thead><tr><td class='th_detail'>{0}</td><td class='th_detail'>{1}</td><td class='th_detail'>{2}</td></tr></thead>", ParseString("# %%Controls%%"), ParseString("Cost of %%Controls%%"), ParseString("How Selected"))
                tbl += String.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", sNumOfControls, sCostOfControls, sHowSelected)
                Return String.Format(" ({0})", tbl + "</table>")
            Else
                Return String.Format(" ({0})", sHowSelected)
            End If
        End If
        Return ""
    End Function

    Public ReadOnly Property SimulationUserID As Integer
        Get
            Return COMBINED_USER_ID
        End Get
    End Property

    Public ReadOnly Property SimulationUserName As String
        Get
            Dim tUserID As Integer = SimulationUserID
            If IsCombinedUserID(tUserID) Then
                Dim CG As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(tUserID)
                If CG IsNot Nothing Then                     
                    If CG.CombinedUserID = COMBINED_USER_ID Then 
                        Return ResString("lblCombinedAll")
                    Else
                        Return CG.Name
                    End If
                End If
            Else
                For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                    If tAppUser.UserID = tUserID Then 
                        Return tAppUser.DisplayName
                    End If
                Next
            End If
            Return "undefined user"
        End Get
    End Property

    Public ReadOnly Property SimulationUserEmail As String
        Get
            Dim tUserID As Integer = SimulationUserID
            If IsCombinedUserID(tUserID) Then
                Return ""
            Else
                For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                    If tAppUser.UserID = tUserID Then 
                        Return tAppUser.UserEmail
                    End If
                Next
            End If
            Return ""
        End Get
    End Property

    ReadOnly Property SESS_SIMULATION_WRTNODE_GUID As String
        Get
            Return String.Format("RISK_EXTRA_PLOT_WRTNODE_GUID_{0}", App.ProjectID.ToString)
        End Get
    End Property

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
            If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS Then
                Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
    
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
            If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS Then
                HierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
            End If
            WriteSetting(PRJ, CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Dim _showCents As Boolean? = Nothing
    Public ReadOnly Property ShowCents As Boolean
        Get
            If Not _showCents.HasValue Then 
                _showCents = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_CENTS_ID, UNDEFINED_USER_ID))
            End If
            Return Not _showCents.HasValue OrElse _showCents.Value
        End Get
    End Property    

    Public ReadOnly Property CostDecimalDigits As Integer 
        Get
            If ShowCents Then Return 2
            Return 0
        End Get
    End Property

    Public Function GetEventsData() As String 
        Dim retVal As String = ""
        For Each tAlt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function (a) a.Enabled)
            retVal += If(retVal = "", "", ",") + String.Format("{{""id"":{0},""name"":'{1}',""grp"":'{2}',""prec"":{3}}}", tAlt.NodeID, JS_SafeString(tAlt.NodeName), JS_SafeString(PM.EventsGroups.GroupName(tAlt)), PM.EventsGroups.GroupPrecedence(tAlt))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetSourcesData() As String 
        Dim retVal As String = ""
        For each node As clsNode In PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes.Where(Function (n) n.Enabled)
            retVal += If(retVal = "", "", ",") + String.Format("{{""id"":{0},""name"":'{1}',""grp"":'{2}',""prec"":{3}}}", node.NodeID, JS_SafeString(node.NodeName), JS_SafeString(PM.SourceGroups.GroupName(node)), PM.SourceGroups.GroupPrecedence(node))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    ReadOnly Property SESS_SIMULATIONS_DATA As String
        Get
            Return String.Format("RISK_EXTRA_PLOT_DATA_{0}", App.ProjectID.ToString)
        End Get
    End Property

    ReadOnly Property SESS_SIMULATIONS_DATA_WC As String
        Get
            Return String.Format("RISK_EXTRA_PLOT_DATA_WC_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property SessionSimulationsData As Dictionary(Of Integer, SimulationStepInfo)
        Get
            Dim tSessVar = Session(SESS_SIMULATIONS_DATA)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Dictionary(Of Integer, SimulationStepInfo) Then
                Return CType(tSessVar, Dictionary(Of Integer, SimulationStepInfo))
            End If
            Return Nothing
        End Get
        Set(value As Dictionary(Of Integer, SimulationStepInfo))
            Session(SESS_SIMULATIONS_DATA) = value
        End Set
    End Property

    Public Property SessionSimulationsDataWC As Dictionary(Of Integer, SimulationStepInfo)
        Get
            Dim tSessVar = Session(SESS_SIMULATIONS_DATA_WC)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Dictionary(Of Integer, SimulationStepInfo) Then
                Return CType(tSessVar, Dictionary(Of Integer, SimulationStepInfo))
            End If
            Return Nothing
        End Get
        Set(value As Dictionary(Of Integer, SimulationStepInfo))
            Session(SESS_SIMULATIONS_DATA_WC) = value
        End Set
    End Property

    Private RiskOverallPageIDs As Integer() = {_PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID, _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART, _PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID, _PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART, _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS, _PGID_RISK_PLOT_OVERALL, _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS, _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, _PGID_RISK_BOW_TIE, _PGID_RISK_BOW_TIE_WITH_CONTROLS}

    Private ReadOnly Property RiskResultsPageID As Integer
        Get
            Dim tSess As String = SessVar("Sess_RRPGID_" + App.ProjectID.ToString)
            If String.IsNullOrEmpty(tSess) Then tSess = _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES.ToString 
            Return CInt(tSess)
        End Get
    End Property

    Private ReadOnly Property HierarchyID As Integer
        Get
            Dim retVal As Integer = ECHierarchyID.hidLikelihood
            Dim tSess As string = SessVar("Sess_HID_" + App.ProjectID.ToString)
            If Not String.IsNullOrEmpty (tSess) Then 
                retVal = CInt(tSess)
            End If
            Return retVal
        End Get
    End Property

    Private Function GetStepData(NumStep As Integer, isExclusiveEvents As Boolean, isControls As Boolean) As String
        Dim simData As Dictionary(Of Integer, SimulationStepInfo) = If(isControls, SessionSimulationsDataWC, SessionSimulationsData)
        Dim retVal As String = ""

        If simData IsNot Nothing AndAlso NumStep >= 1 AndAlso simData.Count >= NumStep Then 
            Dim s As SimulationStepInfo = simData(NumStep)
            Dim sEvents As String = ""
            For Each tAlt As clsNode In PM.ActiveAlternatives.TerminalNodes.Where(Function(a) a.Enabled)
                Dim tFiredEventsInfo As TupleIDD = Nothing
                Dim eSourceID As Integer = -2
                Dim eWRTLikelihood As Double = -1
                Dim eRand As Double = -1
                Dim eSimulatedImpact As Double = 0
                Dim eSimulatedRisk As Double = 0
                Dim eTooltip As String = tAlt.NodeName
                Dim eFired As Boolean = False
                If s.FiredEventsInfo IsNot Nothing AndAlso s.FiredEventsInfo.ContainsKey(tAlt.NodeID) Then
                    tFiredEventsInfo = s.FiredEventsInfo(tAlt.NodeID)
                    eSimulatedRisk = tFiredEventsInfo.Item6
                    eSourceID = tFiredEventsInfo.Item1
                    eWRTLikelihood = tFiredEventsInfo.Item2
                    eSimulatedImpact = If(PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel, -tFiredEventsInfo.Item4, tFiredEventsInfo.Item4)   ' D6798
                    eRand = tFiredEventsInfo.Item3
                    Dim tSource As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(eSourceID)
                    If tSource IsNot Nothing Then 
                        eTooltip = String.Format("{0} -> {1}", tSource.NodePath, tAlt.NodeName)
                    End If
                    eFired = tFiredEventsInfo.Item7
                End If
                '[{0},{1},{2},{3},{4},{5},{6},{7},{8},'{9}']
                sEvents += If(sEvents = "", "", ",") + String.Format("{{""id"":{0},""fired"":{1},""rand"":{2},""sourceId"":{3},""likelihood"":{4},""impact"":{5},""impact_doll"":{6},""risk"":{7},""risk_doll"":{8},""tooltip"":""{9}""}}", tAlt.NodeID, Bool2JS(eFired), JS_SafeNumber(Math.Round(eRand, d)), eSourceID, JS_SafeNumber(Math.Round(eWRTLikelihood, d)), JS_SafeNumber(Math.Round(eSimulatedImpact, d)), JS_SafeNumber(Math.Round(eSimulatedImpact * PM.DollarValueOfEnterprise, d)), JS_SafeNumber(Math.Round(eSimulatedRisk, d)), JS_SafeNumber(Math.Round(eSimulatedRisk * PM.DollarValueOfEnterprise, d)), JS_SafeString(eTooltip)) 'A1468
            Next
            Dim sSources As String = ""
            For each node As clsNode In PM.Hierarchy(ECHierarchyID.hidLikelihood).TerminalNodes.Where(Function (n) n.Enabled)
                Dim eFired As Boolean = False
                Dim eRand As Double = 0
                Dim ePriority As Double = 0
                Dim eTooltip As String = node.NodePath
                If s.SourcesInfo IsNot Nothing AndAlso s.SourcesInfo.ContainsKey(node.NodeID) Then
                    Dim tSourcesInfo = s.SourcesInfo(node.NodeID)
                    eFired = tSourcesInfo.Item1
                    ePriority = tSourcesInfo.Item2
                    eRand = tSourcesInfo.Item3
                End If                
                sSources += If(sSources = "", "", ",") + String.Format("{{""id"":{0},""fired"":{1},""rand"":{2},""priority"":{3},""tooltip"":""{4}""}}", node.NodeID, Bool2JS(eFired), JS_SafeNumber(Math.Round(eRand, d)), JS_SafeNumber(Math.Round(ePriority, d)), JS_SafeString(eTooltip))
            Next
            retVal = String.Format("{0},{1},[{2}],[{3}]", 0, JS_SafeNumber(If(ShowDollarValue, s.Impact * PM.DollarValueOfEnterprise, Math.Round(s.Impact, d))), sEvents, sSources)
        End If

        Return String.Format("{0}", retVal)
    End Function    

    Public AttributesList As New List(Of clsAttribute)

    Public Property SelectedAttributeID As Guid
        Get
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_SELECTED_CATEGORY_ID, UNDEFINED_USER_ID))
            Dim retVal As Guid = Guid.Empty
            If Not String.IsNullOrEmpty(s) Then retVal = New Guid(s)
            Return retVal
        End Get
        Set(value As Guid)
            Dim s As String = ""
            If Not value.Equals(Guid.Empty) Then s = value.ToString
            WriteSetting(PRJ, ATTRIBUTE_LEC_SELECTED_CATEGORY_ID, AttributeValueTypes.avtString, s)
        End Set
    End Property

    Private Sub InitAttributesList()
        PM.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        AttributesList.Clear()
        Dim HasAttributes As Boolean = PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing AndAlso PM.Attributes.AttributesList.Count > 0
        If HasAttributes Then
            'PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, -1)
            For Each tAttr In PM.Attributes.GetAlternativesAttributes(True).Where(Function(attr) attr.ValueType = AttributeValueTypes.avtEnumeration)
                AttributesList.Add(tAttr)
            Next
        End If
    End Sub

    Public Function GetCategories() As String
        Dim tSelectedCategoryID As Guid = SelectedAttributeID
        Dim sRes As String = String.Format("<option value='' {1}>{0}</option>", ResString("lblNone"), IIf(tSelectedCategoryID.Equals(Guid.Empty), " selected='selected' ", ""))
        For Each tAttr As clsAttribute In AttributesList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", tAttr.ID, ShortString(tAttr.Name, 40), IIf(tAttr.ID = tSelectedCategoryID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbCategories' onchange='onSetCategory(this.value);' {1}>{0}</select>", sRes, IIf(AttributesList.Count = 0, " style='display:none;' disabled='disabled' ", ""))
    End Function

    Public Property RedLineValue As Double
        Get
            Dim retVal As Double = CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_RED_LINE_VALUE_ID, UNDEFINED_USER_ID))
            If retVal = UNDEFINED_INTEGER_VALUE Then 
                If PRJ.isOpportunityModel Then retVal = 95 Else retVal = 5
            End If
            Return retVal
        End Get
        Set(value As Double)
            WriteSetting(PRJ, ATTRIBUTE_LEC_RED_LINE_VALUE_ID, AttributeValueTypes.avtDouble, value)
        End Set
    End Property    

    Public Property GreenLineValue As Double 'Max loss value specified by PM
        Get
            Dim retVal As Double = CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, UNDEFINED_USER_ID))
            Dim isDollValue As Boolean = UseDollarValue
            If retVal <> UNDEFINED_INTEGER_VALUE AndAlso isDollValue Then
                If PM.DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE Then 
                    retVal = UNDEFINED_INTEGER_VALUE 
                Else
                    retVal /= 100
                    retVal *= DollarValueOfEnterprise
                End If
            End If
            Return retVal
        End Get
        Set(value As Double)
            Dim isDollValue As Boolean = UseDollarValue
            WriteSetting(PRJ, ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, AttributeValueTypes.avtDouble, If(isDollValue, If(PM.DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE Or PM.DollarValueOfEnterprise = 0, UNDEFINED_INTEGER_VALUE, (value / DollarValueOfEnterprise) * 100), value))
        End Set
    End Property
    
    Private _SimulationMode As Integer = -1
    Public Property SimulationMode As Integer ' 0 - _SIMULATE_SOURCES, 1 - _SIMULATE_EVENTS
        Get
            Return 0
        End Get
        Set(value As Integer)
            _SimulationMode = value
            WriteSetting(PRJ, ATTRIBUTE_LEC_SIMULATION_MODE_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property NumDatapoints As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_NUM_DATAPOINTS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_LEC_NUM_DATAPOINTS_ID, AttributeValueTypes.avtLong, value)
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

    Public Property CurvesShown As Integer ' Without controls (0), with controls (1), both (2)
        Get
            Return 2
            If Not CanUseControls Then Return 0
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_CURVES_WITH_CONTROLS_SHOW_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_LEC_CURVES_WITH_CONTROLS_SHOW_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property ZoomPlot As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_ZOOM_PLOT_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_LEC_ZOOM_PLOT_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Private Sub RiskPlotPage_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
        Dim sMode = CheckVar("mode", "").Trim.ToLower

        If CheckVar("can_use_controls", False) Then
            CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS
            Select Case sMode
                Case "sources"
                    CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES
                Case "objectives"
                    CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS
            End Select
        Else
            Select Case sMode
                Case "sources"
                    CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES
                Case "objectives"
                    CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS
            End Select
        End If
    End Sub
   
    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        InitAttributesList()
        Dim sAction = CheckVar(_PARAM_ACTION, "")
        Ajax_Callback(RemoveXssFromUrl(Request.Form.ToString))  ' D6767
    End Sub

    'todo move to a get_hierarchy_data function to api, calculate results optionally
    Private Class userPriorities
        Public UserID As integer
        Public NodeID As integer
        Public l As Double
        Public g As Double
        Public lwc As Double
        Public gwc As Double
    End Class

    Public Const OPT_PRIORITY_COLUMN_WIDTH As Integer = 90

    Function IsUserChecked(UserID As Integer) As Boolean
        Return UserID = COMBINED_USER_ID
    End Function

    Function GetCheckedUserIDs() As List(Of Tuple(Of Integer, String))
        Dim usersList As New List(Of Tuple(Of Integer, String))

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

    Public Function GetHierarchyData() As String
        If Not (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
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
                'Dim users As List(Of Integer) = SelectedUsersAndGroupsIDs
                Dim users As New List(Of Integer)
                users.Add(COMBINED_USER_ID)
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

    Public Function GetHierarchyColumns(isWithControls As Boolean) As String
        If Not (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

        retVal = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 0, "ID", Bool2JS(False), "", "id", 0, "false", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 1, ParseString(If(HierarchyID = ECHierarchyID.hidLikelihood, "%%Objective(l)%% Name", "%%Objective(i)%% Name")), Bool2JS(True), "", "name", 100, "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 2, ParseString("Description"), Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs), "", "info", 100, "false", "false")

        Dim usersList = GetCheckedUserIDs()

        For Each t As Tuple(Of Integer, String) In usersList
            Dim CurrentUserID As Integer = t.Item1
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", UNDEFINED_INTEGER_VALUE, JS_SafeString(t.Item2), Bool2JS(True), "", "", 0, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 3, "Local", Bool2JS(True), "", String.Format("l{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 4, "Global", Bool2JS(True), "", String.Format("g{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            If isWithControls Then 
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 6, ParseString("Local w/%%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("lwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
                retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 7, ParseString("Global w/%%controls%%"), Bool2JS(CurrentPageID <> _PGID_RISK_REGISTER_DEFAULT), "", String.Format("gwc{0}", CurrentUserID), OPT_PRIORITY_COLUMN_WIDTH, "true", "false")
            End If
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Private Function GetFrequencyChartData(freqData As Dictionary(Of Double, Integer), valueField As String, argField As String) As String
        Dim retVal As String = ""
        Dim isDollValue As Boolean = ShowDollarValue
        Dim tDollarValueOfEnterprise As Double = PM.DollarValueOfEnterprise
        For each kvp As KeyValuePair(Of Double, Integer) in freqData
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{{""{0}"":{1},""{2}"":{3}}}", argField, JS_SafeNumber(If(isDollValue, kvp.Key * tDollarValueOfEnterprise, kvp.Key)), valueField, JS_SafeNumber(kvp.Value / PM.RiskSimulations.NumberOfTrials))
        Next         
        Return retVal
    End Function

    Private Function GetExceedanceCurveData(data As SortedDictionary(Of Double, Double), valueField As String, argField As String) As String
        Dim retVal As String = ""
        Dim isDollValue As Boolean = ShowDollarValue
        Dim tDollarValueOfEnterprise As Double = PM.DollarValueOfEnterprise
        For Each kvp As KeyValuePair(Of Double, Double) in data
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{{""{0}"":{1},""{2}"":{3}}}", argField, JS_SafeNumber(Math.Round(If(isDollValue, kvp.key * tDollarValueOfEnterprise, kvp.key), 4)), valueField, JS_SafeNumber(Math.Round(kvp.Value, 4)))
        Next

        Return retVal
    End Function

    Private Sub GetSimulationDataForSingleLine(isExclusiveEvents As Boolean, useControls As ControlsUsageMode, ByRef sCurveChartData As String, ByRef sFreqChartData As String)
        PM.RiskSimulations.StoreStepValues = True 
        PM.RiskSimulations.GenerateChartValues = True
        PM.RiskSimulations.DataPointsCount = NumDatapoints        
        PM.RiskSimulations.UserID = SimulationUserID
        PM.RiskSimulations.RedLineValue = RedLineValue / 100
        Dim GLV = CDBL(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, UNDEFINED_USER_ID))
        PM.RiskSimulations.GreenLineValue = If(GLV = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, GLV / 100) 'If(ShowDollarValue, GreenLineValue / DollarValueOfEnterprise, GreenLineValue / 100)
        Dim tOldUseReductions = PM.CalculationsManager.ControlsUsageMode
        PM.CalculationsManager.ControlsUsageMode = useControls
        'PM.RiskSimulations.RandomSeed = RandSeed
        PM.RiskSimulations.WRTNode = Nothing         
        Dim lkhWrtNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(SelectedHierarchyNodeID)
        Dim impWrtNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(SelectedHierarchyNodeID)
        If lkhWrtNode IsNot Nothing Then PM.RiskSimulations.WRTNode = lkhWrtNode
        If impWrtNode IsNot Nothing Then PM.RiskSimulations.WRTNode = impWrtNode

        PM.RiskSimulations.Simulate(AddressOf IsCancelled)
        
        sCurveChartData = GetExceedanceCurveData(PM.RiskSimulations.LossExceedanceValues, If(useControls <> ControlsUsageMode.DoNotUse, "lec_wc", "lec"), "loss")
        sFreqChartData = GetFrequencyChartData(PM.RiskSimulations.FrequencyValues, If(useControls <> ControlsUsageMode.DoNotUse, "freq_wc", "freq"), "loss")

        Dim tStepsInfo As Dictionary(Of Integer, SimulationStepInfo) = PM.RiskSimulations.StepsInfo
        If useControls <> ControlsUsageMode.DoNotUse Then
            SessionSimulationsDataWC = tStepsInfo
        Else
            SessionSimulationsData = tStepsInfo
        End If

        PM.RiskSimulations.StoreStepValues = False
        PM.RiskSimulations.GenerateChartValues = False
        PM.RiskSimulations.WRTNode = Nothing
        PM.CalculationsManager.ControlsUsageMode = tOldUseReductions
    End Sub

    Private Function DollarValueOfEnterprise As Double
        Return If(PM.DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE, 0, PM.DollarValueOfEnterprise)
    End Function

    Private Function GetSimulationDataForAllLines(ByRef sRedLineValues As String, ByRef sFreqLineValues As String, ByRef sCVARValues As String) As String
        Dim sDataIndp As String = ""
        Dim sDataIndpWC As String = ""

        Dim sFreqDataIndp As String = ""
        Dim sFreqDataIndpWC As String = ""

        Dim tOldConsequenceSimulationsMode As ConsequencesSimulationsMode = PM.CalculationsManager.ConsequenceSimulationsMode
        If PM.CalculationsManager.ConsequenceSimulationsMode <> ConsequencesSimulationsMode.Diluted Then 
            PM.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted
        End If

        Try            
            GetSimulationDataForSingleLine(False, ControlsUsageMode.DoNotUse, sDataIndp, sFreqDataIndp)
            dRedLineIndp = GetDisplayValue(PM.RiskSimulations.RedLineIntersection)
            dMaxValueIndp = PM.RiskSimulations.GreenLineIntersection
            tExpValueIndp = GetDisplayValue(PM.RiskSimulations.AverageLossValue)
            dCVARIndp = PM.RiskSimulations.CVARValue

            If CanUseControls Then 
                GetSimulationDataForSingleLine(False, ControlsUsageMode.UseOnlyActive, sDataIndpWC, sFreqDataIndpWC)
                dRedLineIndpWC = GetDisplayValue(PM.RiskSimulations.RedLineIntersection)
                dMaxValueIndpWC = PM.RiskSimulations.GreenLineIntersection
                tExpValueIndpWC = GetDisplayValue(PM.RiskSimulations.AverageLossValue)
                dCVARIndpWC = PM.RiskSimulations.CVARValue
            End If

        Catch e As OutOfMemoryException
        End Try

        If PM.CalculationsManager.ConsequenceSimulationsMode <> tOldConsequenceSimulationsMode Then
            PM.CalculationsManager.ConsequenceSimulationsMode = tOldConsequenceSimulationsMode
        End If

        sRedLineValues = String.Format("{0},{1},{2},{3},{4}", JS_SafeNumber(dRedLineIndp), JS_SafeNumber(dRedLineIndpWC), JS_SafeNumber(dMaxValueIndp), JS_SafeNumber(dMaxValueIndpWC), JS_SafeNumber(GreenLineValue))
        sFreqLineValues = String.Format("[{0}],[{1}],[{2}],[{3}]", sFreqDataIndp, sFreqDataIndpWC, "", "") 
        sCVARValues = String.Format("{0},{1},{2},{3}", JS_SafeNumber(dCVARIndp), JS_SafeNumber(dCVARIndpWC), JS_SafeNumber(dCVARExcl), JS_SafeNumber(dCVARExclWC))
        Return String.Format("[{0}],[{1}],[{2}],[{3}]", sDataIndp, sDataIndpWC, "", "") 
    End Function

    Private Function GetDisplayValue(value As Double) As Double
        Return If(value = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, value * If(ShowDollarValue, DollarValueOfEnterprise, 100))
    End Function

    Private tExpValueIndp As Double = UNDEFINED_INTEGER_VALUE
    Private tExpValueExcl As Double = UNDEFINED_INTEGER_VALUE
    Private tExpValueIndpWC As Double = UNDEFINED_INTEGER_VALUE
    Private tExpValueExclWC As Double = UNDEFINED_INTEGER_VALUE
    
    Private dRedLineIndp As Double = UNDEFINED_INTEGER_VALUE
    Private dRedLineIndpWC As Double = UNDEFINED_INTEGER_VALUE
    Private dRedLineExcl As Double = UNDEFINED_INTEGER_VALUE
    Private dRedLineExclWC As Double = UNDEFINED_INTEGER_VALUE

    Private dMaxValueIndp As Double = UNDEFINED_INTEGER_VALUE ' Green line calculated value
    Private dMaxValueExcl As Double = UNDEFINED_INTEGER_VALUE
    Private dMaxValueIndpWC As Double = UNDEFINED_INTEGER_VALUE
    Private dMaxValueExclWC As Double = UNDEFINED_INTEGER_VALUE

    Private dCVARIndp As Double = UNDEFINED_INTEGER_VALUE
    Private dCVARIndpWC As Double = UNDEFINED_INTEGER_VALUE
    Private dCVARExcl As Double = UNDEFINED_INTEGER_VALUE
    Private dCVARExclWC As Double = UNDEFINED_INTEGER_VALUE

    Private Function GetExpValueData() As String
        Dim maxValDecimals As Integer = CInt(IIf(Not ShowDollarValue, 1, 2))

        Dim sExpVal As String = String.Format("[{0},{1},{2},{3}]", JS_SafeNumber(Math.Round(tExpValueIndp, PM.Parameters.DecimalDigits)), JS_SafeNumber(Math.Round(tExpValueIndpWC, PM.Parameters.DecimalDigits)), JS_SafeNumber(Math.Round(tExpValueExcl, PM.Parameters.DecimalDigits)), JS_SafeNumber(Math.Round(tExpValueExclWC, PM.Parameters.DecimalDigits)))
        Dim sRedVal As String = String.Format("[{0},{1},{2},{3}]", JS_SafeNumber(Math.Round(dRedLineIndp, 4)), JS_SafeNumber(Math.Round(dRedLineIndpWC, 4)), JS_SafeNumber(Math.Round(dRedLineExcl, 4)), JS_SafeNumber(Math.Round(dRedLineExclWC, 4)))
        Dim sMaxVal As String = String.Format("[{0},{1},{2},{3}]", JS_SafeNumber(If(dMaxValueIndp = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, Math.Round(dMaxValueIndp * 100, maxValDecimals))), JS_SafeNumber(If(dMaxValueIndpWC = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, Math.Round(dMaxValueIndpWC * 100, maxValDecimals))), JS_SafeNumber(If(dMaxValueExcl = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, Math.Round(dMaxValueExcl * 100, maxValDecimals))), JS_SafeNumber(If(dMaxValueExclWC = UNDEFINED_INTEGER_VALUE, UNDEFINED_INTEGER_VALUE, Math.Round(dMaxValueExclWC * 100, maxValDecimals))))
        Return String.Format("{0},{1},{2}", sExpVal, sRedVal, sMaxVal)
    End Function

    Private Function AllCallbackData(sAction As String) As String 
        Dim sRedLineValues As String = ""
        Dim sFreqLineValues As String = ""
        Dim sCVARValues As String = ""
        Return String.Format("['{0}', [{1}], [{2}], [{3}], {4}, [{5}], [{6}], [{7}], [{8}], {9}, {10},[{11}]]", sAction, GetSimulationDataForAllLines(sRedLineValues, sFreqLineValues, sCVARValues), sFreqLineValues, GetExpValueData(), JS_SafeNumber(PM.RiskSimulations.RandomSeed), sRedLineValues, "", "", "", PM.RiskSimulations.NumberOfTrials, NumDatapoints, sCVARValues)
    End Function

    Private Function IsCancelled As Boolean
        Return Not Response.IsClientConnected
    End Function

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = CheckVar("action", "")
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction
            Case "simulate"
                'IsCancellationPending = False
                Dim sNumSimulations As String = GetParam(args, "simulations")
                Dim tNumSimulations As Integer = CInt(sNumSimulations)
                If Integer.TryParse(sNumSimulations, tNumSimulations) AndAlso PM.RiskSimulations.NumberOfTrials <> tNumSimulations Then
                    If tNumSimulations < 0 Then tNumSimulations = 0
                    If tNumSimulations > PM.RiskSimulations._MAX_NUM_SIMULATIONS Then tNumSimulations = PM.RiskSimulations._MAX_NUM_SIMULATIONS
                    PM.RiskSimulations.NumberOfTrials = tNumSimulations
                End If
                Dim sNumDatapoints As String = GetParam(args, "datapoints")
                Dim tNumDatapoints As Integer = CInt(sNumDatapoints)
                If Integer.TryParse(sNumDatapoints, tNumDatapoints) AndAlso NumDatapoints <> tNumDatapoints Then
                    If tNumDatapoints < 0 Then tNumDatapoints = 2
                    If tNumDatapoints > _MAX_DATAPOINTS Then tNumDatapoints = _MAX_DATAPOINTS
                    NumDatapoints = tNumDatapoints
                End If
                Dim sSeed As String = GetParam(args, "seed")
                Dim tSeed As Integer = PM.RiskSimulations.RandomSeed
                If integer.TryParse(sSeed, tSeed) AndAlso PM.RiskSimulations.RandomSeed <> tSeed Then PM.RiskSimulations.RandomSeed = tSeed

                tResult = AllCallbackData(sAction)
            Case "display_mode"
                ShowDollarValue = CInt(GetParam(args, "value")) = 1
                tResult = AllCallbackData(sAction)
            Case "with_controls_mode"
                CurvesShown = CInt(GetParam(args, "value"))
                tResult = AllCallbackData(sAction)
            Case "simulation_mode"
                SimulationMode = CInt(GetParam(args, "value"))
                tResult = AllCallbackData(sAction)
            Case "zoom_plot"
                ZoomPlot = Param2Bool(args, "val")
                tResult = AllCallbackData(sAction)
            Case "new_seed"
                dim tNewSeed As Integer = DateTime.Now.Millisecond
                PM.RiskSimulations.RandomSeed = tNewSeed
                App.ActiveProject.MakeSnapshot("Random seed changed", PM.RiskSimulations.RandomSeed.ToString)
                tResult = String.Format("['{0}', {1}]", sAction, JS_SafeNumber(tNewSeed))
            Case "keep_seed"
                PM.RiskSimulations.KeepRandomSeed = Param2Bool(args, "value")
                tResult = String.Format("['{0}', {1}]", sAction, Bool2JS(PM.RiskSimulations.KeepRandomSeed))
            Case "show_intersections"
                PM.Parameters.LEC_ShowIntersections = Param2Bool(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}', {1}]", sAction, Bool2JS(PM.Parameters.LEC_ShowIntersections))
            Case "show_freq_charts"
                PM.Parameters.LEC_ShowFrequencyCharts = Param2Bool(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}', {1}]", sAction, Bool2JS(PM.Parameters.LEC_ShowFrequencyCharts))
            Case "log_scale"
                PM.Parameters.LEC_LogarithmicScale = Param2Bool(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}', {1}]", sAction, Bool2JS(PM.Parameters.LEC_LogarithmicScale))
            Case "show_woc_line"
                PM.Parameters.LEC_ShowWithoutControls = Param2Bool(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}', {1}]", sAction, Bool2JS(PM.Parameters.LEC_ShowWithoutControls))
            Case "show_wc_line"
                PM.Parameters.LEC_ShowWithControls = Param2Bool(args, "value")
                PM.Parameters.Save()
                tResult = String.Format("['{0}', {1}]", sAction, Bool2JS(PM.Parameters.LEC_ShowWithControls))
            Case "use_source_groups"
                PM.RiskSimulations.UseSourceGroups = Param2Bool(args, "value")
                tResult = AllCallbackData(sAction)
            Case "use_event_groups"
                PM.RiskSimulations.UseEventsGroups = Param2Bool(args, "value")
                tResult = AllCallbackData(sAction)
            Case "view_step"
                Dim tNumStep As Integer = CInt(GetParam(args, "value"))
                Dim sStepDataIndp As String = ""
                Dim sStepDataIndpWC As String = ""
                Dim sStepDataExcl As String = ""
                Dim sStepDataExclWC As String = ""
                If CurvesShown = 0 OrElse CurvesShown = 2 Then sStepDataIndp = GetStepData(tNumStep, False, False)
                If CurvesShown = 1 OrElse CurvesShown = 2 Then sStepDataIndpWC = GetStepData(tNumStep, False, True)
                tResult = String.Format("['{0}', [[{1}],[{2}],[{3}],[{4}]]]", sAction, sStepDataIndp, sStepdataIndpWC, sStepDataExcl, sStepdataExclWC)
            Case "red_line"
                Dim sValue As String = GetParam(args, "value")
                Dim tValue As Double = 0
                If String2Double(sValue, tValue) AndAlso tValue <> RedLineValue Then 
                    RedLineValue = tValue
                Else
                    RedLineValue = UNDEFINED_INTEGER_VALUE
                End If
                tResult = AllCallbackData(sAction)
            Case "green_line"
                Dim sValue As String = GetParam(args, "value")
                Dim tValue As Double = 0
                If String2Double(sValue, tValue) AndAlso tValue <> GreenLineValue Then 
                    GreenLineValue = tValue
                Else
                    GreenLineValue = UNDEFINED_INTEGER_VALUE
                End If
                tResult = AllCallbackData(sAction)
            Case "select_category"
                Dim sId As String = GetParam(args, "cat_guid")
                Dim id As Guid = Guid.Empty 
                If Not String.IsNullOrEmpty(sID) Then id = New Guid(sId)
                SelectedAttributeID = id
                tResult = AllCallbackData(sAction)
            Case "intersection_options"
                PM.Parameters.LEC_ShowIntersectionsOptions  = CheckVar("value", "[]")
                PM.Parameters.Save()
                tResult = String.Format("[""{0}""]", sAction)
            Case "selected_node"
                Dim sGuid As String = GetParam(args, "value", True)
                SelectedHierarchyNodeID = New Guid(sGuid)
                tResult = String.Format("[""{0}""]", sAction)
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

End Class