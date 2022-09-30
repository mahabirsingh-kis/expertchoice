Imports Telerik.Web.UI
Imports System.Drawing
Imports System.IO

Partial Class RiskResultsPage
    Inherits clsComparionCorePage

    ' D2313 ===
    Private PageIDs As Integer() = {_PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID, _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART, _PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID, _PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART, _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS, _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS, _PGID_RISK_PLOT_OVERALL, _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, _PGID_RISK_PLOT_CAUSES, _PGID_RISK_PLOT_OBJECTIVES, _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS, _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, _PGID_RISK_BOW_TIE, _PGID_RISK_BOW_TIE_CAUSES, _PGID_RISK_BOW_TIE_OBJS, _PGID_RISK_BOW_TIE_DEFINE_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES, _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS}
    'Private PageTypes As String() = {"likelihood", "chart", "imp_grid", "chart2", "all_events", "spec_causes", "spec_objs", "lkh_grid_with_controls", "impact_grid_with_controls", "controls_all_events", "all_causes", "all_objs", "all_causes_with_controls", "all_objs_with_controls"}

    Public PageTitles As String() = {"%%Likelihood%%, %%Impact%%, and %%Risk%% by %%Sources%%", "%%Risk%% by %%Sources%%", "%%Likelihood%%, %%Impact%%, and %%Risk%% by %%Objectives(i)%%", "%%Risk%% by %%Objectives(i)%%", "Overall %%Likelihoods%%, %%Impacts%%, and %%Risks%%", "%%Likelihoods%%, %%Impacts%%, and %%Risks%% from %%Source%% {0}", "%%Likelihoods%%, %%Impacts%% and %%Risks%% to {0} %%Objective(i)%%", "%%Likelihood%%, %%Impact%%, and %%Risk%% from %%Source%% {0} <span id='lblWC'>(With %%Controls%%)</span>", "%%Likelihood%%, %%Impact%%, and %%Risk%% to {0} %%Objective(i)%% <span id='lblWC'>(With %%Controls%%)</span>", "Overall %%Likelihoods%%, %%Impacts%%, and %%Risks%% <span id='lblWC'>(With %%Controls%%)</span>", "%%Event%% {0} %%Likelihoods%% from all %%Sources%% ", "%%Event%% {0} %%Impact%% to all %%Objectives(i)%%", "%%Likelihood%% of the %%Event%% WRT %%Sources%% <span id='lblWC'>(with %%Controls%%)</span>", "%%Impact%% of the %%Event%% WRT %%Objectives(i)%% <span id='lblWC'>(with %%Controls%%)</span>", "%%Risk%% Map - Overall", "%%Risk%% Map <span id='lblWC'>(with %%controls%%)</span> - Overall", "%%Risk%% Map - From %%Sources%%", "%%Risk%% Map - To %%Objectives(i)%%", "%%Risk%% Map <span id='lblWC'>(with %%controls%%)</span> - From %%Sources%%", "%%Risk%% Map <span id='lblWC'>(with %%controls%%)</span> - To %%Objectives(i)%%", "Bow-Tie", "%%Likelihoods%%, %%Impacts%%, and %%Risks%% from %%Source%% {0}", "%%Likelihoods%%, %%Impacts%% and %%Risks%% to {0} %%Objective(i)%%", "Bow-Tie", "Bow-Tie <span id='lblWC'>(with %%controls%%)</span>", "%%Likelihoods%%, %%Impacts%%, and %%Risks%% from %%Source%% {0} <span id='lblWC'>(with %%controls%%)</span>", "%%Likelihoods%%, %%Impacts%% and %%Risks%% to {0} %%Objective(i)%% <span id='lblWC'>(with %%controls%%)</span>"}
    Public PagesWithControlsList() As Integer = {_PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS, _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS, _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS, _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS} 'A1139
    ' D2313 ==

    Public IsWidget As Boolean = False
    'Public Const IsMixedEventsAllowed As Boolean = True

    Public Const _COLOR_SIMULATED As String = "#ffa792"
    Public Const _MAX_NUM_SIMULATIONS As Integer = 1000000 ' 1M

    ' D6662 ===
    Public AutoShowQuickHelp As Boolean = False
    Public QuickHelpParams As String = ""
    Public QuickHelpEmpty As Boolean = True
    ' D6662 ==
    Public isPM As Boolean = False  ' D6672

    Public MapUserEmail As String = ""      ' D6667
    Public MapUserName As String = ""      'A2003

    Public Const _OPT_SHOW_HEATMAP_IN_PIPE As Boolean = True ' D6663

    Private _HierarchyID As ECHierarchyID = ECHierarchyID.hidLikelihood
    Private Property HierarchyID As ECHierarchyID
        Get
            Return _HierarchyID
        End Get
        Set(value As ECHierarchyID)
            _HierarchyID = value
            SessVar("Sess_HID_" + App.ProjectID.ToString) = CInt(value).ToString
        End Set
    End Property

    Private WriteOnly Property RiskResultsPageID As Integer
        Set(value As Integer)
            SessVar("Sess_RRPGID_" + App.ProjectID.ToString) = CInt(value).ToString
        End Set
    End Property

    ' D2534 ===
    Private UsersList As New List(Of String)

    Private Const COL_ID As Integer = 0
    Private Const COL_GUID As Integer = 1
    Private Const COL_NAME As Integer = 2
    Private Const COL_WORST_CASE As Integer = 3
    Private Const COL_TAG As Integer = 4 ' used to store the index of selected category (for IsGroupByAttribute = True)
    Private Const COL_IS_SUMMARY As Integer = 5 ' used to differentiate regular rows and group summary rows
    Private Const COL_ATTR_START As Integer = 6
    Private COL_USERS_START As Integer = 6

    Private SUMMARY_HEADER_COLOR As Color = Color.Teal
    Private SUMMARY_HEADER_BACKGROUND As Color = Color.FromArgb(240, 245, 255)

    Public Const ACTION_SELECTED_USER As String = "sa_selected_user"
    Public Const ACTION_SELECTED_USERS As String = "selected_users"
    Public Const ACTION_GET_TREATMENTS As String = "get_treatments"
    Public Const ACTION_SET_BUBBLE_SIZE As String = "set_bubble_size"
    Public Const ACTION_SHOW_DOLLAR_VALUE As String = "show_dollar_value"

    'Public Const AutoSwitchNormalization As Boolean = False
    Public Const Flt_Separator As String = "|***|"    ' D2666 + A1551
    Public Const Flt_Row_Separator As Char = CChar(vbTab)    'A1551
    Public Const IsObjectiveAttributesAvailable As Boolean = False
    Public Const MAX_DECIMALS_FOR_CONTROLS As Integer = 3
    Public Const MAX_SHAPES_COUNT As Integer = 11

    Public LogMessagePrefix As String = "%%Risk%% Results: "    ' D4181
    Public Const ShowEventCategoriesOnBowTie As Boolean = True

    Private IsExport As Boolean = False ' D2923

    Private IsLikelihoodRelative1Flag As Byte = 1 << 0 ' 1
    Private IsImpactRelative1Flag As Byte = 1 << 1 ' 2
    Private IsRiskRelative1Flag As Byte = 1 << 2 ' 4
    Private IsOverallRelative1Flag As Byte = 1 << 3 ' 8  'for All Participants only

    Public ReadOnly Property RA As ResourceAligner
        Get
            Return App.ActiveProject.ProjectManager.ResourceAlignerRisk
        End Get
    End Property

    Public Function GetIsCancellationPending() As Boolean
        Return False
    End Function

    Public ReadOnly Property GoalName As String
        Get
            Return PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeName
        End Get
    End Property

    Public ReadOnly Property RiskPlotMode As Integer
        Get
            Dim retVal As Integer = PM.Parameters.Riskion_RiskPlotMode
            If Treatments.Count = 0 OrElse Not IsRiskMapWithControlsPage() Then Return 0
            Return retVal 
        End Get
    End Property

    ReadOnly Property SESS_PLOT_EVENT_TYPE As String
        Get
            Return String.Format("RISK_PLOT_EVENT_TYPE_{0}", App.ProjectID.ToString)
        End Get
    End Property
    Public Property RiskPlotEventType As Integer
        Get
            Dim tSessVar = Session(SESS_PLOT_EVENT_TYPE)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Integer Then
                Return CInt(tSessVar)
            End If
            Return EventType.Risk
        End Get
        Set(value As Integer)
            Session(SESS_PLOT_EVENT_TYPE) = value
        End Set
    End Property

#Region "Risk Optimization"
    Public ReadOnly Property IsOptimizationAllowed() As Boolean
        Get
            Return (IsRiskWithControlsPage AndAlso Not IsRiskResultsByEventPage) OrElse (CurrentPageID = _PGID_RISK_BOW_TIE_DEFINE_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS)
        End Get
    End Property

    'ReadOnly Property SESS_SHOW_HIDDEN_SETTINGS As String
    '    Get
    '        Return String.Format("RISK_EXTRA_PLOT_SHOW_HIDDEN_SETTINGS_{0}", App.ProjectID.ToString)
    '    End Get
    'End Property

    'Public Property ShowHiddenSettings As Boolean
    '    Get
    '        Dim tSessVar = Session(SESS_SHOW_HIDDEN_SETTINGS)
    '        If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Boolean Then
    '            Return CBool(tSessVar)
    '        End If
    '        Return False
    '    End Get
    '    Set(value As Boolean)
    '        Session(SESS_SHOW_HIDDEN_SETTINGS) = value
    '    End Set
    'End Property

    Public Readonly Property StringSelectedEventIDs As String
        Get
            Dim retVal As String = ""
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                If alt.Enabled Then
                    retVal += CStr(IIf(retVal = "", "", ",")) + alt.NodeGuidID.ToString
                End If
            Next
            Return retVal
        End Get
    End Property

    Public ReadOnly Property SelectedEventIDs As List(Of Guid)
        Get
            Dim EventsIDs As New List(Of Guid)
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                If alt.Enabled Then
                    EventsIDs.Add(alt.NodeGuidID)
                End If
            Next
            Return EventsIDs
        End Get
    End Property

    Public ReadOnly Property StringSelectedControlIDs As String
        Get
            Dim retVal As String = ""
            For Each ctrl As clsControl In PM.Controls.EnabledControls
                retVal += CStr(IIf(retVal = "", "", ",")) + ctrl.ID.ToString
            Next
            Return retVal
        End Get
    End Property

    Public ReadOnly Property SelectedControlIDs As List(Of Guid)
        Get
            Dim IDs As New List(Of Guid)
            For Each ctrl As clsControl In PM.Controls.EnabledControls
                IDs.Add(ctrl.ID)
            Next
            Return IDs
        End Get
    End Property

    Public Property OptimizationTypeAttribute As RiskOptimizationType
        Get
            Return Ctype(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_TYPE_ID, UNDEFINED_USER_ID), RiskOptimizationType)
        End Get
        Set(value As RiskOptimizationType)
            RA.RiskOptimizer.CurrentScenario.OptimizationType = value
            WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_TYPE_ID, AttributeValueTypes.avtLong, CInt(value))', ParseString("%%Controls%% optimization - optimization type changed (" + [Enum].GetName(GetType(RiskOptimizationType), value) + ")"))
        End Set
    End Property

    'Public Property ControlsSelectedManually As Boolean
    '    Get
    '        Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_CONTROLS_MANUAL_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Boolean)
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_CONTROLS_MANUAL_ID, AttributeValueTypes.avtBoolean, value, "", "")
    '    End Set
    'End Property

    Public Property ShowSolverPane As Boolean 
        Get
            Return False
            Return PM.Controls.DefinedControls.Count > 0 AndAlso CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_SHOW_SOLVER_PANE_RISK_RESULTS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_SHOW_SOLVER_PANE_RISK_RESULTS_ID, AttributeValueTypes.avtBoolean, value, "", "")
        End Set
    End Property
    
    'Public Property BudgetLimitAttribute As Double
    '    Get
    '        Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_BUDGET_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        RA.RiskOptimizer.CurrentScenario.Budget = value
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_BUDGET_ID, AttributeValueTypes.avtDouble, value)', ParseString("%%Controls%% optimization - budget limit changed (" + CostString(value) + ")"))
    '    End Set
    'End Property
    '
    'Public Property MaxRiskAttribute As Double
    '    Get
    '        Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_MAX_RISK_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        RA.RiskOptimizer.CurrentScenario.MaxRisk = value
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_MAX_RISK_ID, AttributeValueTypes.avtDouble, value)', ParseString("%%Controls%% optimization - max %%risk%% changed (" + value.ToString + ")"))
    '    End Set
    'End Property
    '
    'Public Property MinReductionAttribute As Double
    '    Get
    '        Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_OPTIMIZER_MIN_REDUCTION_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        RA.RiskOptimizer.CurrentScenario.MinReduction = value
    '        WriteSetting(PRJ, ATTRIBUTE_RISK_OPTIMIZER_MIN_REDUCTION_ID, AttributeValueTypes.avtDouble, value)', ParseString("%%Controls%% optimization - %%risk%% reduction changed (" + value.ToString + ")"))
    '    End Set
    'End Property

    Public Function getInitialOptimizedRisk() As String
        Dim tShowDollarValue As Boolean = ShowDollarValue
        Dim riskWithControls As Double = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseOnlyActive)
        Dim sRiskValue As String = ""
        sRiskValue = (riskWithControls * 100).ToString("F2") + "%" 
        If tShowDollarValue Then
            sRiskValue += DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * DollarValueOfEnterprise, riskWithControls * DollarValueOfEnterprise, CostDecimalDigits, tShowDollarValue)
        Else 
            sRiskValue += DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue, riskWithControls, CostDecimalDigits, tShowDollarValue)
        End If
        Return sRiskValue
    End Function

    Function GetCurRiskReduction(tShowDollarValue As Boolean) As Double
        With RA.RiskOptimizer.CurrentScenario
            Dim tRiskReduction As Double = 0
            If .OriginalRiskValueWithControls <> 0 Then
                tRiskReduction = 1 - .OriginalRiskValueWithControls / .OriginalRiskValue
            End If
            If tShowDollarValue Then tRiskReduction *= DollarValueOfEnterprise
            Return tRiskReduction
        End With
    End Function

    Private Sub RiskResultsPage_PreInit()
        Dim sSimMode As String = (CheckVar("sim", "")).ToLower
        PM.CalculationsManager.AfterSimulationSynthesisMode = If(sSimMode = "" OrElse sSimMode.Trim = "0", clsCalculationsManager.AfterSimulationSynthesisModes.UseSimulationFunctions, clsCalculationsManager.AfterSimulationSynthesisModes.SumPrioritiesBasedOnSimulatedValues)

        PM.CalculationsManager.PrioritiesCacheManager.ClearCache()
        PM.CalculationsManager.PrioritiesCacheManager.Enabled = True
        If Not App.HasActiveProject Then FetchAccess() ' D4396
        If Not isCallback AndAlso Not IsPostBack Then
            RA.RiskOptimizer.isOpportunityModel = PRJ.isOpportunityModel

            With RA.RiskOptimizer.CurrentScenario
                .OptimizationType = OptimizationTypeAttribute
                '.Budget = BudgetLimitAttribute
                '.MaxRisk = MaxRiskAttribute
                '.MinReduction = MinReductionAttribute
                .OriginalRiskValue = RA.RiskOptimizer.GetRisk(ControlsUsageMode.DoNotUse)
                .OriginalRiskValueWithControls = RA.RiskOptimizer.GetRisk(ControlsUsageMode.UseAll)
                .OriginalAllControlsCost = 0
                .OriginalSelectedControlsCost = 0
                .OriginalSelectedControlsCount = 0
                For Each ctrl As clsControl In PM.Controls.EnabledControls 'A1392
                    .OriginalAllControlsCost += If(ctrl.IsCostDefined, ctrl.Cost, 0)
                    If ctrl.Active Then
                        .OriginalSelectedControlsCost += If(ctrl.IsCostDefined, ctrl.Cost, 0)
                        .OriginalSelectedControlsCount += 1
                    End If
                Next
            End With
        End If

        'If CanUserEditControls Then
            Treatments = GetTreatments()
            TreatmentsSortList = GetTreatmentsSortAttributes()
        'End If
    End Sub

    Private Function OptimizeTreatments(ByRef totalCost As Double, ByRef optimizedRiskValue As Double, Optional OutputPath As String = "") As List(Of Guid)
        Dim EventsIDs As New List(Of Guid)
        For Each alt As clsNode In OrderedEvents
            EventsIDs.Add(alt.NodeGuidID)
        Next

        Dim fundedControls As New List(Of Guid)

        If EventsIDs.Count > 0 Then
            optimizedRiskValue = RA.RiskOptimizer.Optimize(EventsIDs, If(SelectedHierarchyNodeID = Guid.Empty, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, SelectedHierarchyNodeID), fundedControls, totalCost, OutputPath)
            ' D4072 ===
            If ShowDraftPages() AndAlso RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raBaron AndAlso Baron_Error_Code <> BaronErrorCode.None Then
                App.DBSaveLog(dbActionType.actRASolveModel, dbObjectType.einfProject, App.ProjectID, "Riskion optimization", String.Format("Solver '{0}' error: {1}", ResString("lblRASolverBaron"), GetMessageByBaronError())) ' D4078
            End If
            ' D4072
        End If

        Return fundedControls
    End Function

#End Region 

    Public ReadOnly Property IsRiskWithControlsPage() As Boolean
        Get
            Return CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS
        End Get
    End Property

    Public ReadOnly Property IsRiskWithoutControlsGridPage() As Boolean
        Get
            Return CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS
        End Get
    End Property

    Public ReadOnly Property IsRiskResultsByEventPage() As Boolean
        Get
            Return CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS
        End Get
    End Property

    Public ReadOnly Property IsRiskMapPage() As Boolean
        Get
            Return CurrentPageID = _PGID_RISK_PLOT_OVERALL OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS
        End Get
    End Property

    Public ReadOnly Property IsRiskMapByObjectivePage() As Boolean
        Get
            Return CurrentPageID = _PGID_RISK_PLOT_CAUSES OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS
        End Get
    End Property

    Public ReadOnly Property IsRiskMapWithControlsPage() As Boolean
        Get
            Return CurrentPageID = _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS
        End Get
    End Property

    Public ReadOnly Property IsBowTiePage() As Boolean
        Get
            Return CurrentPageID = _PGID_RISK_BOW_TIE OrElse CurrentPageID = _PGID_RISK_BOW_TIE_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_DEFINE_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS
        End Get
    End Property

    Public ReadOnly Property IsBowTieByObjectivePage() As Boolean
        Get
            Return CurrentPageID = _PGID_RISK_BOW_TIE_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS
        End Get
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
            Return PM.CalculationsManager.DollarValueTarget
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_TARGET_ID, AttributeValueTypes.avtString, value, ParseString(LogMessagePrefix + "Cost Target Changed"), "")
        End Set
    End Property

    ' Dollar value of Impact Goal node
    'Private _DollarValueOfEnterprise As Double = UNDEFINED_INTEGER_VALUE
    Public ReadOnly Property DollarValueOfEnterprise As Double
        Get
    '        If _DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE Then
    '            _DollarValueOfEnterprise = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).DollarValue
    '            Dim wrtDollarNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(SelectedHierarchyNodeID)
    '            If wrtDollarNode IsNot Nothing AndAlso wrtDollarNode IsNot PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0) Then

    '                Dim CalcTarget As clsCalculationTarget = Nothing
    '                Dim CG As clsCombinedGroup = PM.CombinedGroups.GetCombinedGroupByUserID(COMBINED_USER_ID)
    '                If CG IsNot Nothing Then
    '                    CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
    '                End If

    '                PM.CalculationsManager.Calculate(CalcTarget, wrtDollarNode, ECHierarchyID.hidImpact)
    '                _DollarValueOfEnterprise = wrtDollarNode.DollarValue
    '            End If
    '        End If
            Return If(PM.DollarValueOfEnterprise <> UNDEFINED_INTEGER_VALUE, PM.DollarValueOfEnterprise, 0)
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
            If DollarValue <> UNDEFINED_INTEGER_VALUE Then sDollValue = CostString(DollarValue, CostDecimalDigits, True)
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), GetTargetName(), sDollvalue)
        End If        
        Return CStr(IIf(DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso retVal <> "", " (" + retVal + ")", ""))
    End Function  

    ReadOnly Property SESSION_FILTER_RULES As String
        Get
            If Not IsRiskResultsByEventPage AndAlso Not IsBowTiePage Then
                Return String.Format("RiskResultsFltRulesAlts_{0}", App.ProjectID.ToString)
            Else
                Return String.Format("RiskResultsFltRulesObjs_{0}", App.ProjectID.ToString)
            End If
        End Get
    End Property

    'ReadOnly Property SESSION_RISK_NORMALIZATION_MODE As String '-1 - Undefined, 2 - Absolute, 4 - RelativeB
    '    Get
    '        Return String.Format("SLRiskSynthMode_{0}", App.ProjectID)
    '    End Get
    'End Property

    'ReadOnly Property SESSION_RISK_SYNTHESIS_MODE_IMPACT As String '-1 - Undefined, 2 - Absolute, 4 - RelativeB
    '    Get
    '        Return String.Format("SLRiskSynthImp_{0}", App.ProjectID)
    '    End Get
    'End Property

    'ReadOnly Property SESSION_RISK_NORM_MODE As String '3 - Unnormalized, 0 - Normalized, 1 - mul100, 2 - Sum100, 3 - % of Maximum
    '    Get
    '        Return String.Format("SLRiskNormMode_{0}", App.ProjectID)
    '    End Get
    'End Property

    'ReadOnly Property SESSION_RISK_NORM_MODE_IMPACT As String '3 - Unnormalized, 0 - Normalized, 1 - mul100, 2 - Sum100, 3 - % of Maximum
    '    Get
    '        Return String.Format("SLRiskNormImp_{0}", App.ProjectID)
    '    End Get
    'End Property

    ReadOnly Property SESSION_WORST_CASE_VISIBLE As String
        Get
            Return String.Format("RiskResultsWorstCaseVisible_{0}", App.ProjectID)
        End Get
    End Property

    ReadOnly Property SESSION_FILTER_APPLIED As String
        Get
            Return String.Format("RiskResultsFilterApplied_{0}", App.ProjectID)
        End Get
    End Property

    ReadOnly Property SESSION_USE_CONTROLS_REDUCTIONS As String
        Get
            Return String.Format("RiskUseControlsReductions_{0}_{1}", App.ProjectID, IsRiskWithControlsPage)
        End Get
    End Property

    ReadOnly Property SESSION_USE_CONTROLS_REDUCTIONS_HIERARCHY As String
        Get
            Return String.Format("RiskUseControlsReductionsHierarchy_{0}{1}", App.ProjectID, IsRiskWithControlsPage)
        End Get
    End Property

    ReadOnly Property SESSION_RISK_USE_CIS As String
        Get
            Return String.Format("RiskUseCIS_{0}", App.ProjectID)
        End Get
    End Property

    ReadOnly Property SESSION_RISK_USE_WEIGHTS As String
        Get
            Return String.Format("RiskUseWEIGHTS_{0}", App.ProjectID)
        End Get
    End Property

    ReadOnly Property SESS_EVENTS_DATA As String
        Get
            Return String.Format("RISK_EXTRA_PLOT_EVENTS_DATA_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property SimulationEventsData As List(Of Integer)
        Get
            Dim tSessVar = Session(SESS_EVENTS_DATA)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is List(Of Integer) Then
                Return CType(tSessVar, List(Of Integer))
            End If
            Return Nothing
        End Get
        Set(value As List(Of Integer))
            Session(SESS_EVENTS_DATA) = value
        End Set
    End Property

    ReadOnly Property SESS_SIMULATION_USER_ID As String
        Get
            Return String.Format("RISK_EXTRA_PLOT_USER_ID_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property SimulationUserID As Integer
        Get
            Dim tSessVar = Session(SESS_SIMULATION_USER_ID)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Integer Then
                Return CInt(tSessVar)
            End If
            Return COMBINED_USER_ID
        End Get
        Set(value As Integer)
            Session(SESS_SIMULATION_USER_ID) = value
        End Set
    End Property

    ReadOnly Property SESS_SIMULATION_WRTNODE_GUID As String
        Get
            Return String.Format("RISK_EXTRA_PLOT_WRTNODE_GUID_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public WriteOnly Property SimulationWRTNodeGuid As Guid
        Set(value As Guid)
            Session(SESS_SIMULATION_WRTNODE_GUID) = value
        End Set
    End Property    

    Dim _showCents As Boolean? = Nothing
    Public Property ShowCents As Boolean
        Get
            If Not _showCents.HasValue Then 
                _showCents = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_CENTS_ID, UNDEFINED_USER_ID))
            End If
            Return Not _showCents.HasValue OrElse _showCents.Value
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

    <Serializable()> _
    Public Enum SynthesisMode
        smIdeal = 0
        smDistributive = 1
        smAbsolute = 2
        smRelative = 3
        smRelativeB = 4
        smUndefined = -1
    End Enum

    'These variable is used only when ShowBarsRelativeToOne = false
    'Private MaxPriorityOutOfAll As Double = Double.MinValue
    Private MaxPriorityL As Double = Double.MinValue
    Private MaxPriorityI As Double = Double.MinValue
    Private MaxPriorityR As Double = Double.MinValue

    Private MaxLikelihood As Double = 0
    Private MaxImpact As Double = 0    

    Private ColumnsUsersIDs As New Dictionary(Of Integer, String)
    Private CanShowGridLegend As Boolean = False

    Public ShowOverallRow As Boolean = False

    ' D2517 ===
    Private Const BAR_W As Integer = 100
    Private Const BAR_H As Integer = 12
    ' D2517 ==

    Private TotalRisk As New List(Of Double)
    Private AverageLoss As New List(Of Double)
    Private TotalMaxRisk As Double
    Private InvestmentLeverage As New List(Of Double)

    Private AverageLossWithoutControls As New List(Of Double)
    Private TotalRiskWithoutControls As New List(Of Double)

    Public ShowGridLegend As Boolean = False    ' D2188

    Public Property ShowTotalRisk As Boolean 'A0938
        Get
            If IsWidget Then Return False
            Return PM.Parameters.Riskion_Show_Total_Risk
        End Get
        Set(value As Boolean)
            PM.Parameters.Riskion_Show_Total_Risk = value
            PM.Parameters.Save()
        End Set
    End Property

    'Public Property ShowResultsMode As ShowResultsModes
    '    Get
    '        If IsWidget Then Return ShowResultsModes.rmSimulated
    '        If PM.Parameters.Riskion_Use_Simulated_Values = 0 Then Return ShowResultsModes.rmComputed Else Return ShowResultsModes.rmSimulated
    '
    '        'Dim tSess As Object = PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_SIMULATED_RESULTS_ID, UNDEFINED_USER_ID)
    '        'If Not TypeOf tSess Is Integer AndAlso Not TypeOf tSess Is Long Then Return ShowResultsModes.rmSimulated
    '        'Return Ctype(tSess, ShowResultsModes)
    '    End Get
    '    Set(value As ShowResultsModes)
    '        Select Case value
    '            Case ShowResultsModes.rmComputed
    '                PM.Parameters.Riskion_Use_Simulated_Values = 0
    '            Case Else 'ShowResultsModes.rmSimulated, ShowResultsModes.rmBoth
    '                PM.Parameters.Riskion_Use_Simulated_Values = 3
    '        End Select
    '        PM.Parameters.Save()
    '        'WriteSetting(PRJ, ATTRIBUTE_RISK_SHOW_SIMULATED_RESULTS_ID, AttributeValueTypes.avtLong, CInt(value))
    '    End Set
    'End Property

    ' D2517 ===
    Private Const BAR_W_SMALL As Integer = 75
    Private Const BAR_H_SMALL As Integer = 10
    Private Const BAR_W_BIG As Integer = 100
    Private Const BAR_H_BIG As Integer = 16
    ' D2517 ==

    Public Sub New()
        MyBase.New(_PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS)  ' D2313
    End Sub

    Private selected_bow_tie_wrt_node As Guid = Guid.Empty

    ' D2313 ===
    Protected Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        IsWidget = CheckVar("widget", False)    ' D6664
        isPM = CanEditActiveProject AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace, App.ActiveWorkgroup)   ' D6672

        Dim sSelectedEventGUID As String = CheckVar("selected_event", "")
        If sSelectedEventGUID <> "" Then Guid.TryParse(sSelectedEventGUID, SelectedEventID)

        selected_bow_tie_wrt_node = CheckVar("selected_node", Guid.Empty)

        RiskResultsPage_PreInit()

        Dim sPgid As String = (CheckVar("pgid", "")).ToLower
        For i As Integer = 0 To PageIDs.Length - 1
            If PageIDs(i).ToString.ToLower = sPgid Then
                CurrentPageID = PageIDs(i)
                RiskResultsPageID = CurrentPageID
                Exit For
            End If
        Next
        'If IsWidget OrElse selected_bow_tie_wrt_node = Guid.Empty Then
        If IsWidget Then
            RadPaneHierarchy.Collapsed = True
            RadSplitRight.Visible = False
        End If
        If IsRiskMapPage AndAlso Not IsWidget Then divHeader.Visible = False
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_BY_EVENT OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES OrElse CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS Then HierarchyID = ECHierarchyID.hidImpact Else HierarchyID = ECHierarchyID.hidLikelihood
        'UseControlsReductions = CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL
        'UseControlsReductionsHierarchy = UseControlsReductions
        If IsRiskMapPage OrElse IsBowTiePage Then
            RadPaneGrid.Scrolling = SplitterPaneScrolling.Both
        Else
            RadPaneGrid.Scrolling = SplitterPaneScrolling.X
        End If
        If IsRiskMapPage Then btnResetZoom.Text = ResString("btnResetZoom")
        btnExport.Visible = App.isExportAvailable   ' D3324
        '_redBrush = PM.Parameters.DefaultRedBrush 'Red
        '_whiteBrush = PM.Parameters.DefaultWhiteBrush 'Yellow
        '_greenBrush = PM.Parameters.DefaultGreenBrush 'Green
        ' D6662 ===
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
        ' D6662 ==
    End Sub
    ' D2313 ==

    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        'If Not isAJAX AndAlso Not isCallback AndAlso Not IsPostBack Then InitAttributesList()   ' D6666

        'jQuery Ajax
        Dim sAction = (CheckVar(_PARAM_ACTION, "")).Trim.ToLower
        Select Case sAction
            Case ""
        End Select
        Ajax_Callback(Request.Form.ToString)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' D2143 ===
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_IMPACT_CHART Then
            If Not IsPostBack AndAlso Not isCallback Then
                ClientScript.RegisterStartupScript(GetType(String), "InitChart", "InitChart();", True)
            End If
        End If
        ' D2143 ==
        ' D2188 ===
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_IMPACT_GRID OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL OrElse IsRiskResultsByEventPage Then
            If Not IsPostBack AndAlso Not isCallback Then
                'ClientScript.RegisterStartupScript(GetType(String), "InitGrid", "InitGrid();", True)   ' -D2313
                'GridResults.Visible = False    ' -D2313
            Else
                GridResults.Visible = True
            End If
        End If
        ' D2188 ==
        If Not IsPostBack AndAlso Not isCallback Then
            With PM
                Dim HasAttributes As Boolean = .Attributes IsNot Nothing AndAlso .Attributes.AttributesList IsNot Nothing AndAlso .Attributes.AttributesList.Count > 0
                If HasAttributes Then
                    .Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, -1)
                End If
            End With
            If CurrentPageID = _PGID_RISK_BOW_TIE OrElse CurrentPageID = _PGID_RISK_BOW_TIE_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_OBJS Then UseControlsReductions = False
            If CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS Then UseControlsReductions = True
            'RadColorPickerHigh.Columns = 9
            'RadColorPickerMid.Columns = 9
            'RadColorPickerLow.Columns = 9
            'Dim colors As ColorPickerItemCollection = GetStandardColors()
            'RadColorPickerHigh.Items.AddRange(colors)
            'RadColorPickerMid.Items.AddRange(colors)
            'RadColorPickerLow.Items.AddRange(colors)
        End If
    End Sub

    ' D2179 ===
    Public Property ShowGlobalPrty As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SHOW_GLOBAL_PRTY_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SHOW_GLOBAL_PRTY_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property ShowLocalPrty As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SHOW_LOCAL_PRTY_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SHOW_LOCAL_PRTY_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property
    ' D2179 ==

    Public Property ShowTreatmentNames As Boolean 'A1179
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SHOW_CONTROLS_NAMES_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SHOW_CONTROLS_NAMES_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property ShowTreatmentIndices As Boolean
        Get
            Return CBool(PM.Parameters.Riskion_Show_Control_Indices)
        End Get
        Set(value As Boolean)
            PM.Parameters.Riskion_Show_Control_Indices = value
            PM.Parameters.Save()
        End Set
    End Property

    Public Property ShowGroupTotals As Boolean 'A1179
        Get
            Return  False
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SHOW_GROUPS_TOTALS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SHOW_GROUPS_TOTALS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public ReadOnly Property NormalizedLkhd As SynthesisMode
        Get
            Return SynthesisMode.smAbsolute
            '    'If Not ShowHiddenSettings Then Return SynthesisMode.smAbsolute 'A1405
            '    Dim retVal As Integer = SynthesisMode.smAbsolute
            '    Dim s = SessVar(SESSION_RISK_NORMALIZATION_MODE)
            '    Dim iVal As Integer = 2
            '    If Not String.IsNullOrEmpty(s) AndAlso Integer.TryParse(s, iVal) Then
            '        retVal = iVal
            '    End If
            '    Return CType(retVal, SynthesisMode)
        End Get
        'Set(value As SynthesisMode)
        '    SessVar(SESSION_RISK_NORMALIZATION_MODE) = CInt(value).ToString
        'End Set
    End Property

    'Public Property NormalizedLocalLkhd As LocalNormalizationType
    '    Get
    '        'If Not ShowHiddenSettings Then Return LocalNormalizationType.ntUnnormalized 'A1405
    '        Dim retVal As LocalNormalizationType = LocalNormalizationType.ntUnnormalized
    '        Dim s = SessVar(SESSION_RISK_NORM_MODE)
    '        If String.IsNullOrEmpty(s) Then retVal = LocalNormalizationType.ntUnnormalized Else retVal = CType(CInt(s), LocalNormalizationType)
    '        Return retVal
    '    End Get
    '    Set(value As LocalNormalizationType)
    '        SessVar(SESSION_RISK_NORM_MODE) = CInt(value).ToString
    '    End Set
    'End Property

    Public ReadOnly Property NormalizedLocalImpact As LocalNormalizationType
        Get
            Return LocalNormalizationType.ntUnnormalized
            ''If Not ShowHiddenSettings Then Return LocalNormalizationType.ntUnnormalized 'A1405
            'Dim retVal As LocalNormalizationType = LocalNormalizationType.ntUnnormalized
            'Dim s = SessVar(SESSION_RISK_NORM_MODE_IMPACT)
            'If String.IsNullOrEmpty(s) Then retVal = LocalNormalizationType.ntUnnormalized Else retVal = CType(CInt(s), LocalNormalizationType)
            'Return retVal
        End Get
        'Set(value As LocalNormalizationType)
        '    SessVar(SESSION_RISK_NORM_MODE_IMPACT) = CInt(value).ToString
        'End Set
    End Property

    Public Property IsWorstCaseVisible As Boolean
        Get
            Return SessVar(SESSION_WORST_CASE_VISIBLE) = "1" AndAlso CheckedUsersCount() > 1
        End Get
        Set(value As Boolean)
            SessVar(SESSION_WORST_CASE_VISIBLE) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Public Property ShowAttributes As Boolean
        Get
            If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS OrElse IsRiskMapPage Then
                Dim tShowAttributes As Boolean = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_ATTRIBUTES_ID, UNDEFINED_USER_ID))
                If tShowAttributes Then IsGroupByAttribute = False
                Return tShowAttributes
            End If
            Return False
        End Get
        Set(value As Boolean)
            If value Then IsGroupByAttribute = False
            WriteSetting(PRJ, ATTRIBUTE_RISK_SHOW_ATTRIBUTES_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property IsGroupByAttribute As Boolean
        Get
            If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS Then
                Dim tGroupByAttribute As Boolean = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_GROUP_BY_ATTRIBUTE_ID, UNDEFINED_USER_ID))
                If tGroupByAttribute Then ShowAttributes = False
                Return tGroupByAttribute
            End If
            Return False
        End Get
        Set(value As Boolean)
            If value Then ShowAttributes = False
            WriteSetting(PRJ, ATTRIBUTE_RISK_GROUP_BY_ATTRIBUTE_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property GroupByAttributeGUID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim sRes As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_GROUP_BY_ATTR_GUID_ID, UNDEFINED_USER_ID))
            If sRes <> "" Then retVal = New Guid(sRes) Else If AttributesList.Count > 0 Then retVal = AttributesList(0).ID
            If (AttributesList.Count > 0 AndAlso AttributesList.Where(Function(attr) attr.ID = retVal).Count = 0) AndAlso Not PRJ.isMixedModel AndAlso Not PRJ.isMyRiskRewardModel Then retVal = AttributesList(0).ID  ' D6798
            If retVal = Guid.Empty AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) Then retVal = GroupByRiskOpportunity ' D6798
            Return retVal
        End Get
        Set(value As Guid)
            WriteSetting(PRJ, ATTRIBUTE_RISK_GROUP_BY_ATTR_GUID_ID, AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Public Property GroupByRiskOpportunity As Guid = New Guid("{E1B79080-2754-4C5F-B6B3-723323FF9CC7}")

    Public Property IsFilterApplied As Boolean
        Get
            Return SessVar(SESSION_FILTER_APPLIED) = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_FILTER_APPLIED) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Public Property UseControlsReductions As Boolean
        Get
            If IsRiskWithoutControlsGridPage OrElse (Not (CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES)) OrElse Treatments.Count = 0 Then Return False
            Dim v As String = SessVar(SESSION_USE_CONTROLS_REDUCTIONS)
            If String.IsNullOrEmpty(v) Then
                If IsRiskWithControlsPage Then Return True Else Return False
            End If
            Return v = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_USE_CONTROLS_REDUCTIONS) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Public Property UseControlsReductionsHierarchy As Boolean
        Get
            Dim v As String = SessVar(SESSION_USE_CONTROLS_REDUCTIONS_HIERARCHY)
            If String.IsNullOrEmpty(v) Then
                If IsRiskWithControlsPage Then Return True Else Return False
            End If
            Return v = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_USE_CONTROLS_REDUCTIONS_HIERARCHY) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Public Property ShowRiskReduction As Boolean
        Get
            If IsWidget Then Return False
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_RISK_REDUCTION_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_SHOW_RISK_REDUCTION_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    'Private Sub SaveSetting(ID As Guid, valueType As AttributeValueTypes, value As Object)
    '    If Not App.Options.isSingleModeEvaluation Then
    '        With PM
    '            .Attributes.SetAttributeValue(ID, UNDEFINED_USER_ID, valueType, value, Guid.Empty, Guid.Empty)
    '            .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
    '        End With
    '    End If
    'End Sub

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

    Public Property SelectedUserOrGroupID As Integer
        Get
            ' D6667 ===
            Dim UID As Integer = UNDEFINED_USER_ID
            If Not App.Options.isSingleModeEvaluation AndAlso Not IsWidget Then
                UID = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SELECTED_USER_ID, UNDEFINED_USER_ID))
            Else
                UID = PM.UserID
            End If
            Return UID
            ' D6667 ==
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SELECTED_USER_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

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
            If selected_bow_tie_wrt_node <> Guid.Empty Then Return selected_bow_tie_wrt_node ' for Jump To Bow-Tie feature

            'A1064 ===
            If (IsBowTiePage AndAlso Not IsBowTieByObjectivePage) OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS Then
                Return PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID
            End If
            'A1064 ==

            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) AndAlso s.Length > 16 Then retVal = New Guid(s)
            Return retVal
        End Get
        Set(value As Guid)
            ' D6664 ===
            If IsWidget Then
                PM.Attributes.SetAttributeValue(CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), UNDEFINED_USER_ID, AttributeValueTypes.avtString, value.ToString, Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), AttributeValueTypes.avtString, value.ToString)
            End If
            ' D6664 ==
        End Set
    End Property

    Public ReadOnly Property SelectedHierarchyNodeName(Optional IsRiskMap As Boolean = False) As String
        Get
            Dim _id As Guid = SelectedHierarchyNodeID
            Dim node As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(_id)
            If node Is Nothing Then node = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(_id)
            Dim retVal As String = ""
            If node IsNot Nothing Then 'AndAlso node IsNot PM.Hierarchy(ECHierarchyID.hidLikelihood).nodes(0) AndAlso node IsNot PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0) Then 
                'retVal = JS_SafeString(String.Format("{0} &quot;{1}&quot;", ResString("lblRiskRegionsFor"), node.NodeName))
                'retVal = JS_SafeString(If(IsRiskMap AndAlso node.ParentNode Is Nothing, "Overall", node.NodeName))
                retVal = JS_SafeString(node.NodeName)
            End If
            Return retVal
        End Get
    End Property

    Public Property SelectedEventID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SELECTED_EVENT_ID, UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) AndAlso s.Length > 16 Then retVal = New Guid(s)

            If retVal = Guid.Empty AndAlso OrderedEvents.Count > 0 Then retVal = OrderedEvents()(0).NodeGuidID

            Dim tSelectedHierarchyNodeID As Guid = SelectedHierarchyNodeID
            Dim lkhGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)
            Dim wrtNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tSelectedHierarchyNodeID)
            If wrtNode Is Nothing Then wrtNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tSelectedHierarchyNodeID)
            If wrtNode Is Nothing Then wrtNode = lkhGoalNode

            If wrtNode.Hierarchy.HierarchyID <> ECHierarchyID.hidLikelihood Then 
                Dim ContributedAltsList As List(Of clsNode) = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(alt) AnySubObjectiveContributes(wrtNode, alt.NodeID)).ToList
                If ContributedAltsList.Where(Function (alt) alt.NodeGuidID = retVal).Count = 0 Then retVal = If(ContributedAltsList.Count > 0, ContributedAltsList(0).NodeGuidID, Guid.Empty)
            End If

            Return retVal
        End Get
        Set(value As Guid)
            If value <> Guid.Empty Then WriteSetting(PRJ, ATTRIBUTE_RISK_SELECTED_EVENT_ID, AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Public ReadOnly Property SelectedEventName As String
        Get
            Dim retVal As String = ""

            Dim _id As Guid = SelectedEventID
            Dim alt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(_id)

            If alt IsNot Nothing Then
                retVal = JS_SafeString(alt.NodeName)
            End If
            Return retVal
        End Get
    End Property

    Public Function GetSAParticipants() As String
        Dim sRes As String = ""
        For Each group As clsCombinedGroup In PM.CombinedGroups.GroupsList
            group.ApplyRules()
            sRes += String.Format("<option value='{0}' {2}>[{1}]</option>", group.CombinedUserID, SafeFormString(group.Name), IIf(group.CombinedUserID = SelectedUserOrGroupID, " selected='selected' ", ""))
        Next
        sRes += "<option disabled='disabled'>-----------------------------</option>"
        For Each user As clsUser In PM.UsersList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", user.UserID, SafeFormString(CStr(IIf(user.UserName = "", user.UserEMail, user.UserName))), IIf(user.UserID = SelectedUserOrGroupID, " selected='selected' ", ""))
        Next
        Return String.Format("<select class='select' name='cbSAUser' style='width:120px;' onchange='showLoadingPanel(); sendCommand(""action=" + ACTION_SELECTED_USER + "&id=""+this.value);'>{0}</select>", sRes)
    End Function

    Public ReadOnly Property OrderedEvents(Optional EnabledOnly As Boolean = True) As IOrderedEnumerable(Of clsNode)
        Get
            Return PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function (alt) Not EnabledOnly OrElse alt.Enabled).OrderBy(Function(a) a.RiskEventSortOrder)
        End Get
    End Property

    Public Function GetEventsData() As String
        Dim retVal As String = ""
        For Each Alt As clsNode In OrderedEvents(False)
           'Dim sDollValueString As String = ""
           'If Alt.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso Alt.DollarValue <> 0 Then sDollValueString = String.Format(" ({0})", CostString(Alt.DollarValue, CostDecimalDigits, True))

            Dim fChk As Integer = CInt(IIf(Alt.Enabled, 1, 0))
            'retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("['{0}', '{1}{2}', {3}, {4}]", Alt.NodeGuidID, ShortString(JS_SafeString(Alt.NodeName), 50, True), SafeFormString(sDollValueString), fChk, JS_SafeNumber(Alt.DollarValue))
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("['{0}', '{1}{2}', {3}, {4}]", Alt.NodeGuidID, ShortString(JS_SafeString(Alt.NodeName), 50, True), "", fChk, JS_SafeNumber(Alt.DollarValue))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetAllObjsData() As String
        Dim retVal As String = ""
        For Each Node As clsNode In PM.Hierarchy(ECHierarchyID.hidImpact).Nodes
            Dim sDollValue As String = ""
            If Node.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso Node.DollarValue <> 0 Then sDollValue = String.Format(" ({0})", CostString(Node.DollarValue, CostDecimalDigits, True))

            Dim margin As String = New String(CChar(" "), Node.Level)
            margin = CStr(IIf(margin = "", "", margin.Replace(" ", "&nbsp;") + "•&nbsp;"))
            retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("['{0}', '{1}{2}{3}', {4}]", Node.NodeGuidID, margin, ShortString(JS_SafeString(Node.NodeName), 50), SafeFormString(sDollValue), JS_SafeNumber(Node.DollarValue))
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetEvents(ComboBoxWidth As Integer) As String
        Dim tSelectedHierarchyNodeID As Guid = SelectedHierarchyNodeID
        Dim lkhGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)
        Dim wrtNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(tSelectedHierarchyNodeID)
        If wrtNode Is Nothing Then wrtNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(tSelectedHierarchyNodeID)
        If wrtNode Is Nothing Then wrtNode = lkhGoalNode

        Dim tSelectedEventID As Guid = SelectedEventID
        
        Dim ContributedAltsList As List(Of clsNode) = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(alt) AnySubObjectiveContributes(wrtNode, alt.NodeID)).ToList
        If ContributedAltsList.Where(Function (alt) alt.NodeGuidID = tSelectedEventID).Count = 0 Then tSelectedEventID = If(ContributedAltsList.Count > 0, ContributedAltsList(0).NodeGuidID, Guid.Empty)

        Dim sRes As String = ""
        For Each Alt As clsNode In OrderedEvents
            If ContributedAltsList.Contains(Alt) Then
                sRes += String.Format("<option value='{0}' {2}>{1}</option>", Alt.NodeGuidID, ShortString(JS_SafeString(Alt.NodeName), 40), IIf(Alt.NodeGuidID = tSelectedEventID, " selected='selected' ", ""))
            End If
        Next

        Return String.Format("<select id='cbBowTieEvents' style='width:{2}px; margin-bottom:-1px;' onchange='sendCommand(""selected_event=""+this.value);' {1}>{0}</select>", sRes, IIf(ContributedAltsList.Count = 0 OrElse tSelectedEventID = Guid.Empty, " disabled='disabled' ", ""), ComboBoxWidth)
    End Function

    Public Property ZoomPlot As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_ZOOM_PLOT_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_ZOOM_PLOT_ID, AttributeValueTypes.avtBoolean, value)
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

    Public Property BarsRelativeTo1State As Byte
        Get
            Return CByte(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_BARS_RELATIVE_TO_1_STATE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Byte)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_BARS_RELATIVE_TO_1_STATE_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    ''' <summary>
    ''' String, containing visible attributes GUIDs delimited with ","
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectedColumns As String
        Get
            Return CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SELECTED_COLUMNS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SELECTED_COLUMNS_ID, AttributeValueTypes.avtString, value)
        End Set
    End Property

    Public Property ShowEventDescriptions As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_SHOW_EVENT_DESCRIPTIONS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_SHOW_EVENT_DESCRIPTIONS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property _Rh As Double
        Get
            Dim retVal As Double = PM.Parameters.DefaultRh
            If SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID OrElse SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID Then
                retVal = PM.Parameters.Riskion_Regions_Rh
            Else
                Dim attrVal As Double = CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_WRT_RISK_HIGH_ID, SelectedHierarchyNodeID))
                If attrVal >= 0 Then retVal = attrVal
            End If
            Return retVal
        End Get
        Set(value As Double)
            If SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID OrElse SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID Then
                PM.Parameters.Riskion_Regions_Rh = value
                PM.Parameters.Save()
            Else
                PM.Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_WRT_RISK_HIGH_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, value, SelectedHierarchyNodeID, Guid.Empty)
                PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
            End If
        End Set
    End Property

    Public Property _Rl As Double
        Get
            Dim retVal As Double = PM.Parameters.DefaultRl
            If SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID OrElse SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID Then
                retVal = PM.Parameters.Riskion_Regions_Rl
            Else
                Dim attrVal As Double = CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_WRT_RISK_LOW_ID, SelectedHierarchyNodeID))
                If attrVal >= 0 Then retVal = attrVal
            End If
            Return retVal
        End Get
        Set(value As Double)
            If SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID OrElse SelectedHierarchyNodeID = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID Then
                PM.Parameters.Riskion_Regions_Rl = value
                PM.Parameters.Save()
            Else
                PM.Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_WRT_RISK_LOW_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtDouble, value, SelectedHierarchyNodeID, Guid.Empty)
                PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
            End If
        End Set
    End Property

    Private Function AddRadTreeNode(tNode As clsNode, ByRef RadNodes As RadTreeNodeCollection, Level As Integer) As RadTreeNode ' D2179
        Dim rNode As New RadTreeNode
        ' D2179 ===
        Dim sCaption = tNode.NodeName
        'If tNode.ParentNode Is Nothing Then sCaption = "Overall"
        Dim MaxLen As Integer = 60
        Dim sPrty As String = ""
        If ShowLocalPrty AndAlso tNode.ParentNode IsNot Nothing Then
            MaxLen -= 4
            sPrty = String.Format("[L:{0}]", Double2String(CDbl(IIf(HierarchyID = ECHierarchyID.hidLikelihood, tNode.LocalPriorityUnnormalized(COMBINED_USER_ID, UseControlsReductionsHierarchy) * 100, tNode.LocalPriority(COMBINED_USER_ID, UseControlsReductionsHierarchy) * 100)), PM.Parameters.DecimalDigits))
            'sPrty = String.Format("[L:{0}]", (tNode.UnnormalizedPriority * 100).ToString("F2"))
        End If
        If ShowGlobalPrty AndAlso tNode.ParentNode IsNot Nothing Then
            MaxLen -= 4
            sPrty += String.Format("[G:{0}]", Double2String(tNode.UnnormalizedPriority * 100, PM.Parameters.DecimalDigits))
        End If
        If sPrty <> "" Then sPrty = String.Format(" <span class='TreeInner'>{0}</span>", sPrty)
        rNode.Text = JS_SafeHTML(ShortString(sCaption, MaxLen - Level * 3, True)) + sPrty    ' D0042
        ' D2179 ==
        rNode.Value = tNode.NodeGuidID.ToString
        Dim asRoot As Boolean = False 'Level = 0
        rNode.CssClass = CStr(IIf(asRoot, "tree_root", "tree_alt"))
        rNode.HoveredCssClass = CStr(IIf(asRoot, "tree_root tree_root_hover", "tree_node tree_node_hover"))
        rNode.SelectedCssClass = CStr(IIf(asRoot, "tree_root tree_root_sel", "tree_node tree_node_sel"))

        RadNodes.Add(rNode)
        Return rNode
    End Function

    'for RiskByEvent
    Private Function AddRadTreeNode(tNode As clsNode, ByRef RadNodes As RadTreeNodeCollection, asRoot As Boolean, SelectedNodeID As Guid, Optional Prefix As String = "") As RadTreeNode
        Dim rNode As New RadTreeNode
        ' D2179 ===
        Dim sCaption = Prefix + tNode.NodeName
        If tNode.NodeID = -1 Then sCaption = ResString("lblRiskResultsEvents") '"Events"
        Dim MaxLen As Integer = 60
        rNode.Text = JS_SafeHTML(ShortString(sCaption, MaxLen, True))
        ' D2179 ==
        rNode.Value = String.Format("{0}", tNode.NodeGuidID.ToString)
        If Not SelectedNodeID.Equals(Guid.Empty) AndAlso tNode.NodeGuidID.Equals(SelectedNodeID) Then
            rNode.Selected = True
        End If
        rNode.CssClass = CStr(IIf(asRoot, "tree_root", "tree_alt"))
        rNode.HoveredCssClass = CStr(IIf(asRoot, "tree_root tree_root_hover", "tree_node tree_node_hover"))
        rNode.SelectedCssClass = CStr(IIf(asRoot, "tree_root tree_root_sel", "tree_node tree_node_sel"))
        RadNodes.Add(rNode)
        Return rNode
    End Function

    Private Sub AddNodesToRadTree(Nodes As List(Of clsNode), ByRef RadNodes As RadTreeNodeCollection, SelectedNodeID As Guid)
        If Nodes Is Nothing Or RadNodes Is Nothing Then Exit Sub
        For Each tNode As clsNode In Nodes
            Dim rNode As RadTreeNode = AddRadTreeNode(tNode, RadNodes, tNode.Level) ' D2179
            If tNode.Children IsNot Nothing AndAlso tNode.Children.Count > 0 Then
                AddNodesToRadTree(tNode.Children, rNode.Nodes, SelectedNodeID)
                rNode.Expanded = True
            End If
            Dim isOverallNode As Boolean = (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS) AndAlso rNode.ParentNode Is Nothing
            If isOverallNode Then rNode.Enabled = False
            If rNode.Enabled Then rNode.Style.Add("cursor", "pointer") ' D2313
            If rNode.Enabled AndAlso Not SelectedNodeID.Equals(Guid.Empty) AndAlso tNode.NodeGuidID.Equals(SelectedNodeID) Then rNode.Selected = True
            'Don't disable the "Overall" node by EF's request
            If isOverallNode Then rNode.Enabled = True
            If tNode.RiskNodeType = RiskNodeType.ntCategory Then
                rNode.Attributes.Add("style", "font-style: italic;")
            End If
        Next
    End Sub

    Private Sub LoadHierarchy()
        InitAlternatives()

        If RadTreeHierarchy.Nodes.Count = 0 Then
            Dim tSelNode = CType(IIf(Not IsRiskResultsByEventPage AndAlso (Not IsBowTiePage OrElse IsBowTieByObjectivePage), SelectedHierarchyNodeID, SelectedEventID), Guid)
            If Not IsRiskResultsByEventPage AndAlso (Not IsBowTiePage OrElse IsBowTieByObjectivePage) Then
                If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL Then
                    tSelNode = PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID
                End If

                With PM
                    '.StorageManager.Reader.LoadUserData()
                    If ShowLocalPrty OrElse ShowGlobalPrty Then
                        .CalculationsManager.GetRiskDataWRTNode(COMBINED_USER_ID, "", .Hierarchy(HierarchyID).Nodes(0).NodeGuidID, CType(IIf(UseControlsReductionsHierarchy, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode)) 'A1064
                    End If
                    AddNodesToRadTree(.Hierarchy(HierarchyID).GetLevelNodes(0), RadTreeHierarchy.Nodes, tSelNode)
                End With
                If RadTreeHierarchy.SelectedNode Is Nothing Then
                    If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS OrElse IsBowTieByObjectivePage OrElse IsRiskMapByObjectivePage Then
                        If RadTreeHierarchy.Nodes.Count > 0 Then
                            If RadTreeHierarchy.Nodes(0).Nodes.Count > 0 Then
                                RadTreeHierarchy.Nodes(0).Nodes(0).Selected = True
                                SelectedHierarchyNodeID = PM.Hierarchy(HierarchyID).Nodes(0).Children(0).NodeGuidID
                            Else
                                RadTreeHierarchy.Nodes(0).Selected = True
                                SelectedHierarchyNodeID = PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID
                            End If
                        End If
                    Else
                        If RadTreeHierarchy.Nodes.Count > 0 Then
                            RadTreeHierarchy.Nodes(0).Selected = True
                            SelectedHierarchyNodeID = PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID
                        Else
                            RadTreeHierarchy.Nodes(0).Selected = True
                            SelectedHierarchyNodeID = PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID
                        End If
                    End If
                End If
            Else
                Dim tOrderedEvents = OrderedEvents
                With PM
                    .StorageManager.Reader.LoadUserData()
                    If ShowLocalPrty OrElse ShowGlobalPrty Then
                        CalculateOverall()
                    End If
                    Dim rootNode As RadTreeNode = AddRadTreeNode(New clsNode With {.NodeID = -1, .NodeGuidID = ALTERNATIVES_HEADER_ITEM_ID}, RadTreeHierarchy.Nodes, True, tSelNode)
                    rootNode.Expanded = True
                    Dim tShowEventIds As Boolean = PM.Parameters.NodeIndexIsVisible
                    Dim SelectedWRTNode = .Hierarchy(HierarchyID).Nodes(0)
                    Dim isForGoalLikelihood As Boolean = HierarchyID = ECHierarchyID.hidLikelihood And SelectedWRTNode.ParentNodeID = -1
                    For Each alt As clsNode In tOrderedEvents
                        If isForGoalLikelihood OrElse AnySubObjectiveContributes(SelectedWRTNode, alt.NodeID) Then
                            Dim rNode As RadTreeNode = AddRadTreeNode(alt, rootNode.Nodes, False, tSelNode, CStr(IIf(tShowEventIds, alt.Index + " ", "")))
                        End If
                    Next
                End With
                If RadTreeHierarchy.SelectedNode Is Nothing AndAlso RadTreeHierarchy.Nodes.Count > 0 AndAlso RadTreeHierarchy.Nodes(0).Nodes IsNot Nothing AndAlso RadTreeHierarchy.Nodes(0).Nodes.Count > 0 AndAlso tOrderedEvents.Count > 0 Then
                    RadTreeHierarchy.Nodes(0).Nodes(0).Selected = True
                    SelectedEventID = tOrderedEvents(0).NodeGuidID
                End If
                If RadTreeHierarchy.Nodes.Count > 0 Then RadTreeHierarchy.Nodes(0).Enabled = False
            End If
            RadTreeHierarchy.DataBind()
        End If
    End Sub

    Protected Sub RadAjaxPanelGrid_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxManagerMain.AjaxRequest
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(e.Argument)

        Dim sSelectedEventGUID As String = (GetParam(args, "selected_event"))   
        If sSelectedEventGUID <> "" Then
            Guid.TryParse(sSelectedEventGUID, SelectedEventID)
            Exit Sub
        End If

        ' D2666 ===
        Dim sFilter As String = URLDecode(GetParam(args, "filter")).Trim()
        If sFilter <> "" Then

            ' D2668 ===
            Dim sComb As String = (GetParam(args, "combination"))   
            If sComb <> "" Then
                If sComb = "0" Then FilterCombination = FilterCombinations.fcAnd
                If sComb = "1" Then FilterCombination = FilterCombinations.fcOr
            End If
            ' D2668 ==

            CurrentFiltersList = ParseFilterString(sFilter)
            IsFilterApplied = isAnyFilterItemChecked(CurrentFiltersList)
            'Else
            '    CurrentFiltersList = Nothing
            '    IsFilterApplied = False
        End If
        ' D2666 ==

        Dim sAction As String = (GetParam(args, "action")).ToLower.Trim 
        'Dim sNormLkhd As String = (GetParam(args, "norm_lkhd")).ToLower.Trim
        Dim sNormLkhd As String = (GetParam(args, "norm_lkhd_local")).ToLower.Trim 
        Dim sNormImpact As String = (GetParam(args, "norm_impact")).ToLower.Trim
        Dim sUseControlsReductions As String = (GetParam(args, "use_controls")).ToLower.Trim
        Dim sShowRiskReduction As String = (GetParam(args, "show_risk_reduction")).ToLower.Trim 
        Dim sWorstCase As String = (GetParam(args, "worst_case")).ToLower.Trim          
        Dim sShowBarsRelativeToOne As String = (GetParam(args, "bars_relative_to_one")).ToLower.Trim
        Dim sShowBarsLRelative As String = (GetParam(args, "bars_l")).ToLower.Trim  
        Dim sShowBarsIRelative As String = (GetParam(args, "bars_i")).ToLower.Trim  
        Dim sShowBarsRRelative As String = (GetParam(args, "bars_r")).ToLower.Trim  
        Dim sShowBowTieResults As String = (GetParam(args, "show_bowtie_results")).ToLower.Trim 
        Dim sRiskPlotMode As String = (GetParam(args, "riskplot_controls_mode")).ToLower.Trim 
        Dim sRiskPlotEventType As String = GetParam(args, "riskplot_event_type")
        Dim sShowUpperLevelNodes As String = (GetParam(args, "show_upper_level_nodes")).ToLower.Trim
        Dim sShowBowTieBackground As String = (GetParam(args, "show_bowtie_background")).ToLower.Trim   
        Dim sShowBowTieAttributes As String = (GetParam(args, "show_bowtie_attributes")).ToLower.Trim   
        Dim sBowTieLikelihoodSort As String = (GetParam(args, "bowtie_likelihood_sort")).ToLower.Trim   
        Dim sBowTieImpactSort As String = (GetParam(args, "bowtie_impact_sort")).ToLower.Trim   
        'Dim sCanUserEditControls As String = (GetParam(args, "show_controls")).ToLower.Trim 
        Dim sShowControlNames As String = (GetParam(args, "show_control_names")).ToLower.Trim   
        Dim sShowControlIndices As String = (GetParam(args, "show_control_indices")).ToLower.Trim   
        Dim sSwitchAxes As String = (GetParam(args, "switch_axes")).ToLower.Trim
        Dim sShowRegions As String = (GetParam(args, "show_regions")).ToLower.Trim  
        Dim sShowLabels As String = (GetParam(args, "show_labels")).ToLower.Trim
        Dim sJitter As String = (GetParam(args, "jitter_datapoints")).ToLower.Trim  
        Dim sShowLegends As String = (GetParam(args, "show_legends")).ToLower.Trim  
        Dim sAxisXTitle As String = (GetParam(args, "axis_x_name")).Trim
        Dim sAxisYTitle As String = (GetParam(args, "axis_y_name")).Trim
        Dim sRegionsParams As String = (GetParam(args, "regions_params")).Trim  
        Dim sZoomPlot As String = (GetParam(args, "zoom_plot")).Trim
        Dim sBubbleSize As String = (GetParam(args, "risk_bubble_size")).Trim   
        Dim sUseCIS As String = (GetParam(args, "use_cis")).Trim
        Dim sUseWeights As String = (GetParam(args, "use_weights")).Trim
        Dim sShowTotalRisk As String = (GetParam(args, "show_total_risk")).Trim 
        Dim sShowSimulatedResults As String = (GetParam(args, "show_sim_results")).Trim 
        Dim sLikelihoodCalcMode As String = GetParam(args, "probability_calculation_mode")
        Dim sWRTCalcMode As String = GetParam(args, "wrt_calculation_mode")
        Dim sUseShuffling As String = (GetParam(args, "use_shuffling")).Trim 
        Dim sDilutedMode As String = (GetParam(args, "diluted_mode")).Trim 

        'If sNormLkhd <> "" Then
        '    NormalizedLkhd = CType(IIf(sNormLkhd = "1" OrElse sNormLkhd = "true", SynthesisMode.smRelativeB, SynthesisMode.smAbsolute), SynthesisMode)
        '    'If sNormLkhd = "0" Then NormalizedLkhd = SynthesisMode.smRelativeB
        '    'If sNormLkhd = "2" Then NormalizedLkhd = SynthesisMode.smAbsolute
        'End If

        'If sNormLkhdLocal <> "" Then NormalizedLocalLkhd = CType(IIf(sNormLkhdLocal = "1" OrElse sNormLkhdLocal = "true", LocalNormalizationType.ntNormalizedForAll, LocalNormalizationType.ntUnnormalized), LocalNormalizationType)
        'If sNormImpact <> "" Then NormalizedLocalImpact = CType(IIf((sNormImpact = "3" OrElse sNormImpact = "false"), LocalNormalizationType.ntUnnormalized, LocalNormalizationType.ntNormalizedForAll), LocalNormalizationType)

        If Not String.IsNullOrEmpty(sUseControlsReductions) Then Str2Bool(sUseControlsReductions, UseControlsReductions)
        If Not String.IsNullOrEmpty(sShowRiskReduction) Then Str2Bool(sShowRiskReduction, ShowRiskReduction)
        If Not String.IsNullOrEmpty(sWorstCase) Then Str2Bool(sSwitchAxes, IsWorstCaseVisible)
        If Not String.IsNullOrEmpty(sShowBarsRelativeToOne) Then Str2Bool(sShowBarsRelativeToOne, IsShowBarsRelativeToOne)
        If Not String.IsNullOrEmpty(sShowBarsLRelative) Then Str2Bool(sShowBarsLRelative, BarsLRelativeTo1)
        If Not String.IsNullOrEmpty(sShowBarsIRelative) Then Str2Bool(sShowBarsIRelative, BarsIRelativeTo1)
        If Not String.IsNullOrEmpty(sShowBarsRRelative) Then Str2Bool(sShowBarsRRelative, BarsRRelativeTo1)
        If Not String.IsNullOrEmpty(sShowBowTieResults) Then Str2Bool(sShowBowTieResults, CanShowAllPriorities)
        If Not String.IsNullOrEmpty(sRiskPlotMode) AndAlso Integer.TryParse(sRiskPlotMode, PM.Parameters.Riskion_RiskPlotMode) Then
            PM.Parameters.Save()
        End If
        If Not String.IsNullOrEmpty(sRiskPlotEventType) Then Integer.TryParse(sRiskPlotEventType, RiskPlotEventType)
        If Not String.IsNullOrEmpty(sShowUpperLevelNodes) Then Str2Bool(sShowUpperLevelNodes, CanShowUpperLevelNodes)
        If Not String.IsNullOrEmpty(sShowBowTieBackground) Then Str2Bool(sShowBowTieBackground, CanShowBowTieBackground)
        If Not String.IsNullOrEmpty(sShowBowTieAttributes) Then Str2Bool(sShowBowTieAttributes, CanShowBowTieAttributes)
        'If Not String.IsNullOrEmpty(sCanUserEditControls) Then Str2Bool(sCanUserEditControls, CanUserEditControls)
        If Not String.IsNullOrEmpty(sShowControlNames) Then Str2Bool(sShowControlNames, ShowTreatmentNames)
        If Not String.IsNullOrEmpty(sShowControlIndices) Then Str2Bool(sShowControlIndices, ShowTreatmentIndices)
        If Not String.IsNullOrEmpty(sSwitchAxes) Then Str2Bool(sSwitchAxes, RiskMapSwitchAxes)
        If Not String.IsNullOrEmpty(sShowRegions) Then RiskMapShowRegions = CInt(sShowRegions)
        If Not String.IsNullOrEmpty(sShowLabels) Then Str2Bool(sShowLabels, RiskMapShowLabels)
        If Not String.IsNullOrEmpty(sJitter) Then Str2Bool(sJitter, RiskMapJitterDatapoints)
        If Not String.IsNullOrEmpty(sShowLegends) Then Str2Bool(sShowLegends, RiskMapShowLegends)
        If Not String.IsNullOrEmpty(sBubbleSize) Then Str2Bool(sBubbleSize, RiskMapBubbleSizeWithRisk)
        If Not String.IsNullOrEmpty(sAxisXTitle) Then AxisXTitle = sAxisXTitle
        If Not String.IsNullOrEmpty(sAxisYTitle) Then AxisYTitle = sAxisYTitle
        If Not String.IsNullOrEmpty(sZoomPlot) Then Str2Bool(sZoomPlot, ZoomPlot)
        If Not String.IsNullOrEmpty(sShowTotalRisk) Then Str2Bool(sShowTotalRisk, ShowTotalRisk)
        If Not String.IsNullOrEmpty(sShowSimulatedResults) Then
            PM.CalculationsManager.UseSimulatedValues = Str2Bool(sShowSimulatedResults)
            PM.Parameters.Save()
        End If
        If Not String.IsNullOrEmpty(sLikelihoodCalcMode) Then
            PM.CalculationsManager.LikelihoodsCalculationMode = CType(CInt(sLikelihoodCalcMode), LikelihoodsCalculationMode)
            App.ActiveProject.MakeSnapshot("Likelihoods calculation mode changed", PM.CalculationsManager.LikelihoodsCalculationMode.ToString)
        End If
        If Not String.IsNullOrEmpty(sWRTCalcMode) Then
            PM.CalculationsManager.ShowDueToPriorities = CInt(sWRTCalcMode) = 1
            App.ActiveProject.MakeSnapshot("Show due to priorities mode changed", PM.CalculationsManager.ShowDueToPriorities.ToString)
        End If
        If Not String.IsNullOrEmpty(sDilutedMode) Then
            PM.CalculationsManager.ConsequenceSimulationsMode = CType(CInt(sDilutedMode), ConsequencesSimulationsMode)
            App.ActiveProject.MakeSnapshot("Consequence simulation mode changed", PM.CalculationsManager.ConsequenceSimulationsMode.ToString)
        End If
        If Not String.IsNullOrEmpty(sUseShuffling) Then
            PM.RiskSimulations.UseShuffling = Str2Bool(sUseShuffling)
            App.ActiveProject.MakeSnapshot("Use shuffling changed", PM.RiskSimulations.UseShuffling.ToString)
        End If

        If sUseCIS <> "" Then
            Dim UseCIS As Boolean = (sUseCIS = "1" OrElse sUseCIS = "true")
            PM.CalculationsManager.UseCombinedForRestrictedNodes = UseCIS
            PM.PipeParameters.UseCISForIndividuals = UseCIS
            App.ActiveProject.SaveProjectOptions("Use CIS option", , , sUseCIS)
        End If

        If sUseWeights <> "" Then
            PM.CalculationsManager.UseUserWeights = (sUseWeights = "1" OrElse sUseWeights = "true")
            App.ActiveProject.SaveProjectOptions("Update 'UseUserWeights' option", , , sUseWeights)
        End If

        If sAction <> "" Then
            Select Case sAction
                Case ACTION_SELECTED_USER
                    Dim sID As String = (GetParam(args, "id")).Trim   
                    SelectedUserOrGroupID = CInt(sID)
                Case ACTION_SELECTED_USERS
                    StringSelectedUsersAndGroupsIDs = (GetParam(args, "ids")).Trim()
                Case "show_cents"
                    ShowCents = Param2Bool(args, "chk")
                ' color by alt attribute:
                Case "color_by_category"
                    ColorCodingByCategory = Param2Bool(args, "chk")
                Case "select_category"
                    Dim sId As String = (GetParam(args, "cat_guid")).Trim   
                    SelectedCategoryID = New Guid(sId)
                    ' shape by alt attribute:
                Case "shape_by_category"
                    ShapeBubblesByCategory = Param2Bool(args, "chk")
                Case "select_shape_category"
                    Dim sId As String = (GetParam(args, "cat_guid")).Trim   
                    SelectedShapeCategoryID = New Guid(sId)
                Case ACTION_SET_BUBBLE_SIZE
                    RiskMapBubbleSize = CInt((GetParam(args, "val").Trim()))
                    ' number of decimal places
                Case "decimals"
                    PM.Parameters.DecimalDigits = CInt((GetParam(args, "value")))
                    ' risk map - show as an event cloud
                Case "event_cloud"
                    RiskMapEventCloud = Param2Bool(args, "value")
                    'risk map - sort legend by clicking the header row
                Case "risk_legend_sort_click"
                    Dim col_idx As Integer = CInt((GetParam(args, "col_idx")))
                    If col_idx <> RiskMapLegendSortColumn Then
                        RiskMapLegendSortColumn = col_idx
                    Else
                        RiskMapLegendSortDirection = CInt(IIf(RiskMapLegendSortDirection = 0, 1, 0))
                    End If
                Case "is_grouping"
                    Dim tParam As String = (GetParam(args, "chk"))  
                    IsGroupByAttribute = tParam = "1" OrElse tParam.ToLower = "true"
                Case "group_totals"
                    Dim tParam As String = (GetParam(args, "chk"))  
                    ShowGroupTotals = tParam = "1" OrElse tParam.ToLower = "true"
                Case "solver_pane"
                    ShowSolverPane = Param2Bool(args, "chk")
                Case "grouping_category"
                    Dim tID As String = (GetParam(args, "value"))   
                    GroupByAttributeGUID = New Guid(tID)
                Case "addcontrol"
                    Dim name As String = (GetParam(args, "name")).Trim  
                    'Dim infodoc As String = (GetParam(args, "descr")).Trim    -D4347
                    Dim categories As String = (GetParam(args, "cat")).Trim 
                    Dim obj_id As Guid = Guid.Empty
                    Dim tOID As String = (GetParam(args, "obj_id")) 
                    If tOID <> "" AndAlso tOID <> "undefined" Then obj_id = New Guid(tOID)
                    Dim ctrl_type As String = (GetParam(args, "ctype")).Trim
                    Dim ControlType As ControlType = CType(ctrl_type, ControlType)
                    Dim cost As Double = UNDEFINED_INTEGER_VALUE
                    If Not String2Double((GetParam(args, "cost")).Trim(), cost) Then cost = UNDEFINED_INTEGER_VALUE

                    'ControlsSelectedManually = True
                    If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                        PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                        PM.Parameters.Save()
                    End If

                    'Dim control As clsControl = PM.Controls.AddControl(Guid.NewGuid, name, ControlType, infodoc)
                    Dim control As clsControl = PM.Controls.AddControl(Guid.NewGuid, name, ControlType) ' D4347
                    control.Cost = cost
                    control.Categories = categories
                    Dim checked As Boolean = True
                    If Not obj_id.Equals(Guid.Empty) Then
                        If ControlType = ControlType.ctCause Then
                            If checked Then
                                AddControlAssignment(control.ID, obj_id)
                            Else
                                DeleteControlAssignment(control.ID, obj_id)
                            End If
                        Else
                            Dim obj_ids As New List(Of Guid)
                            obj_ids.Add(obj_id)
                            Dim event_ids As List(Of Guid) = New List(Of Guid)
                            event_ids.Add(SelectedEventID)
                            Dim control_ids = New List(Of Guid)
                            control_ids.Add(control.ID)
                            If checked Then
                                AddControlsAssignments2(ControlType, control_ids, obj_ids, event_ids)
                            Else
                                DeleteControlsAssignments2(control_ids, obj_ids, event_ids)
                            End If
                        End If
                    End If
                    WriteControls(PRJ)
                    WriteAttributeValues(PRJ, ParseString(LogMessagePrefix + "%%Control%% added"), control.Name)    ' D3731
                Case "editcontrol"
                    Dim control_id As Guid = New Guid((GetParam(args, "control_id")))
                    Dim name As String = (GetParam(args, "name")).Trim  
                    'Dim infodoc As String = (GetParam(args, "descr")).Trim    '-D4347
                    Dim categories As String = (GetParam(args, "cat")).Trim 
                    Dim cost As Double = 0
                    String2Double((GetParam(args, "cost")).Trim(), cost)
                    Dim control As clsControl = PM.Controls.GetControlByID(control_id)
                    If control IsNot Nothing Then
                        control.Name = name
                        'control.InfoDoc = infodoc
                        control.Cost = cost
                        control.Categories = categories
                        'SetControlInfodoc(PRJ, control, infodoc, String.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Host), ParseString(LogMessagePrefix + "%%Control%% edited"))    ' D4345 -D4347
                        WriteControls(PRJ)
                        WriteAttributeValues(PRJ, ParseString(LogMessagePrefix + "%%Control%% edited"), control.Name)  ' D3731
                    End If
                Case "deletecontrol"
                    Dim control_id As Guid = New Guid((GetParam(args, "control_id")))
                    Dim res As Boolean = PM.Controls.DeleteControl(control_id)
                    If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% deleted"))
                    'ControlsSelectedManually = True
                    If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                        PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                        PM.Parameters.Save()
                    End If
                Case "select_controls_bt"
                    Dim obj_id As Guid = Guid.Empty
                    Dim tOID As String = (GetParam(args, "obj_id")) 
                    If tOID <> "" AndAlso tOID <> "undefined" Then obj_id = New Guid(tOID)
                    Dim ctrl_type As String = (GetParam(args, "ctype")).Trim
                    Dim ControlType As ControlType = CType(ctrl_type, ControlType)
                    Dim control_ids As String = (GetParam(args, "control_ids")).Trim
                    Dim IDs As New List(Of Guid)
                    If control_ids.Trim <> "" Then
                        Dim ctrlIDs = control_ids.Split(CChar(","))
                        For Each ctr_id As String In ctrlIDs
                            If ctr_id.Trim <> "" Then
                                IDs.Add(New Guid(ctr_id))
                            End If
                        Next
                    End If
                    'ControlsSelectedManually = True
                    If PM.Parameters.Riskion_ControlsActualSelectionMode <> 0 Then
                        PM.Parameters.Riskion_ControlsActualSelectionMode = 0
                        PM.Parameters.Save()
                    End If
                    UpdateControlsAssignments(ControlType, IDs, obj_id, SelectedEventID)
                Case "remove_contribution"
                    Dim obj_id As Guid = Guid.Empty
                    Dim tOID As String = (GetParam(args, "node_id"))
                    If tOID <> "" AndAlso tOID <> "undefined" Then obj_id = New Guid(tOID)
                    Dim tHid As ECHierarchyID = CType(CInt((GetParam(args, "hid"))), ECHierarchyID)
                    Dim Alt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(SelectedEventID)
                    If Alt IsNot Nothing AndAlso Not obj_id.Equals(Guid.Empty) AndAlso Not SelectedEventID.Equals(Guid.Empty) Then
                        Dim AltAsSource As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(obj_id)
                        If AltAsSource IsNot Nothing Then
                            'remove edge
                            PM.Edges.RemoveEdge(obj_id, SelectedEventID)
                            PM.Edges.Save()
                        Else
                            'remove contribution
                            Dim CovObjIDs As New List(Of Guid)
                            For Each Node As clsNode In PM.Hierarchy(tHid).TerminalNodes
                                If Node.GetContributedAlternatives.Contains(Alt) AndAlso Not Node.NodeGuidID.Equals(obj_id) Then
                                    CovObjIDs.Add(Node.NodeGuidID)
                                End If
                            Next
                            PM.UpdateContributions(SelectedEventID, CovObjIDs, tHid)
                        End If
                    End If
                Case "delete_event"
                    Dim sAltID As String = (GetParam(args, "event_id"))  ' Anti-XSS                
                    Dim AltID As Integer = CInt(sAltID)
                    Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                    Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                    If Alt IsNot Nothing Then
                        Dim sSnapshotMessage = "Delete " + ParseString("%%alternative%%")
                        Dim sSnapshotComment = String.Format("Delete ""{0}""", Alt.NodeName)
                        AltH.DeleteNode(Alt, False)
                        App.ActiveProject.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment)
                    End If
                'Case "event_type"
                '    Dim sAltID As String = (GetParam(args, "event_id"))  ' Anti-XSS                
                '    Dim isOppEvent As Boolean = Str2Bool((GetParam(args, "is_opportunity")))  ' Anti-XSS                
                '    Dim AltID As Integer = CInt(sAltID)
                '    Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                '    Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                '    If Alt IsNot Nothing Then
                '        PM.Attributes.SetAttributeValue(ATTRIBUTE_RISK_EVENT_TYPE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, If(isOppEvent, 1, 0), Alt.NodeGuidID, Guid.Empty)
                '        WriteAttributeValues(PRJ, "", "")
                '    End If
                Case "risk_event_type"
                    Dim sValue As String = (GetParam(args, "value")).Trim   
                    Dim sID As String = (GetParam(args, "id")).Trim   
                    Dim tEventID As Integer = 0
                    Dim tEventType As Integer = 0
                    If Integer.TryParse(sID, tEventID) AndAlso Integer.TryParse(sValue, tEventType) Then
                        Dim Alt As clsNode = PM.ActiveAlternatives.GetNodeByID(tEventID)
                        If Alt IsNot Nothing AndAlso [Enum].IsDefined(GetType(EventType), tEventType) Then
                            Dim tNewEventType As EventType = EventType.Risk
                            tNewEventType = CType([Enum].Parse(GetType(EventType), sValue), EventType)
                            Alt.EventType = tNewEventType
                            Dim sSnapshotMessage = ParseString("Change %%alternative%% type")
                            Dim sSnapshotComment = String.Format("Change ""{0}"" type to {1}", Alt.NodeName, tNewEventType)
                            PRJ.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment)
                        End If
                    End If
                Case "update_event_name"
                    Dim sAltID As String = (GetParam(args, "event_id"))  ' Anti-XSS                
                    Dim AltID As Integer = CInt(sAltID)
                    Dim sAltName As String = (GetParam(args, "val"))  ' Anti-XSS                
                    Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
                    Dim Alt As clsNode = AltH.GetNodeByID(AltID)
                    If Alt IsNot Nothing Then
                        Dim sSnapshotMessage = "Update " + ParseString("%%alternative%%") + " name"
                        Dim sSnapshotComment = String.Format("Update ""{0}"" name", Alt.NodeName)
                        Alt.NodeName = sAltName
                        App.ActiveProject.SaveStructure(sSnapshotMessage, True, True, sSnapshotComment)
                    End If
                Case "select_columns"
                    Dim tColumnsIDs As String = (GetParam(args, "column_ids"))  
                    SelectedColumns = tColumnsIDs
                Case ACTION_SHOW_DOLLAR_VALUE
                    ShowDollarValue = (GetParam(args, "chk")).Trim = "1"
                    App.ActiveProject.MakeSnapshot("Show monetary values setting changed", ShowDollarValue.ToString)
                'Case "set_dollar_value"
                '    Dim sValue As String = (GetParam(args, "value")).Trim   
                '    Dim sTarget As String = (GetParam(args, "target")).Trim   
                '    Dim dValue As Double
                '    If Not String.IsNullOrEmpty(sValue) AndAlso String2Double(sValue, dValue) Then
                '        DollarValue = dValue
                '    End If
                '    If DollarValueTarget <> sTarget Then DollarValueTarget = sTarget
                '    RA.RiskOptimizer.InitOriginalValues()
                '    'tResult = String.Format("['OK',{0},{1}]", GetAllObjsData(), GetEventsData())
                Case "event_id_mode"
                    PM.Parameters.NodeVisibleIndexMode = CType(Param2Int(args, "value"), IDColumnModes)
                    PM.Parameters.Save()
                Case "show_event_numbers"
                    PM.Parameters.NodeIndexIsVisible = Param2Bool(args, "value")
                    PM.Parameters.Save()
                Case "show_event_descriptions"
                    ShowEventDescriptions = Param2Bool(args, "value")
                Case "solve"
                    'ControlsSelectedManually = False

                    Dim tDblValue As Double = 0
                    Select Case RA.RiskOptimizer.CurrentScenario.OptimizationType
                        Case RiskOptimizationType.BudgetLimit
                            Dim sBudget As String = (GetParam(args, "value")).ToLower ' Anti-XSS
                            If Not String.IsNullOrEmpty(sBudget) AndAlso String2Double(sBudget, tDblValue) AndAlso RA.RiskOptimizer.BudgetLimit <> tDblValue Then
                                RA.RiskOptimizer.BudgetLimit = tDblValue
                                RA.Save()
                            End If
                        Case RiskOptimizationType.MaxRisk
                            Dim sMaxRisk As String = (GetParam(args, "risk")).ToLower ' Anti-XSS
                            If String2Double(sMaxRisk, tDblValue) Then
                                If ShowDollarValue AndAlso DollarValueOfEnterprise <> 0 Then
                                    tDblValue = Math.Round(tDblValue * 100 / DollarValueOfEnterprise, 5)
                                End If
                                tDblValue /= 100
                                If RA.RiskOptimizer.CurrentScenario.MaxRisk <> tDblValue Then
                                    RA.RiskOptimizer.CurrentScenario.MaxRisk = tDblValue
                                    RA.Save()
                                End If
                            End If
                        Case RiskOptimizationType.MinReduction
                            Dim sMinReduction As String = (GetParam(args, "reduction")).ToLower ' Anti-XSS
                            If String2Double(sMinReduction, tDblValue) Then
                                If ShowDollarValue AndAlso DollarValueOfEnterprise <> 0 Then
                                    tDblValue = Math.Round(tDblValue * 100 / DollarValueOfEnterprise, 5)
                                End If
                                tDblValue /= 100
                                If RA.RiskOptimizer.CurrentScenario.MinReduction <> tDblValue Then
                                    RA.RiskOptimizer.CurrentScenario.MinReduction = tDblValue
                                    RA.Save()
                                End If
                            End If
                    End Select

                    'ActivateAllControls() 'A1232

                    Dim tFundedCost As Double = 0
                    Dim tOptimizedRisk As Double = 0
                    Dim tOptimizedRiskDollarValue As Double = 0
                    Dim retVal As List(Of Guid) = OptimizeTreatments(tFundedCost, tOptimizedRisk)
                    Dim sIDs As String = ""
                    For Each Id As Guid In retVal
                        sIDs += CStr(IIf(sIDs = "", "", ",")) + String.Format("'{0}'", Id.ToString)
                    Next
                    ' A1232 - make only funded controls active
                    Dim res As Boolean = False
                    For Each ctrl As clsControl In PM.Controls.EnabledControls 'A1392
                        Dim newIsActive As Boolean = retVal.Contains(ctrl.ID)
                        If PM.Controls.SetControlActive(ctrl.ID, newIsActive) Then
                            res = True
                        End If
                    Next
                    If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%%(s) activated by optimizer")) ' D4181
                    With RA.RiskOptimizer.CurrentScenario
                        'RA.Solver.GetComputedRisk(ControlsUsageMode.UseOnlyActive, SelectedEventIDs)
                        .OptimizedRiskValue = tOptimizedRisk
                        .OriginalSelectedControlsCount = PM.Controls.EnabledControls.Where(Function(ctrl) ctrl.Active).Count
                        .OriginalSelectedControlsCost = PM.Controls.EnabledControls.Sum(Function(ctrl) CDbl(IIf(ctrl.Active AndAlso ctrl.IsCostDefined, ctrl.Cost, 0)))
                    End With
                    ' A1232 ==
                    Dim tMode As Integer = 1 ' Optimized
                    If tMode <> PM.Parameters.Riskion_ControlsActualSelectionMode Then
                        PM.Parameters.Riskion_ControlsActualSelectionMode = tMode
                        PM.Parameters.Save()
                    End If
                    ' D4078 ===
                    'Dim sError As String = GetMessageByBaronError()
                    'tResult = String.Format("['solve', [{0}], {1}, {2}, '{3}', '{4}']", sIDs, JS_SafeNumber(RA.RiskOptimizer.BudgetLimit), JS_SafeNumber(tFundedCost), CStr(IIf(tOptimizedRisk >= 0, JS_SafeNumber(Math.Round(tOptimizedRisk, 4)), JS_SafeString("<span class='error'>Error</span>"))), JS_SafeString(sError))
                    ' D4078 ==
                'Case "save_event_group"
                '    Dim eventID As Integer = CInt((GetParam(args, "event_id")).Trim)   
                '    Dim groupName As String = (GetParam(args, "group_name")).Trim   
                '    Dim sPrecedence As String = (GetParam(args, "group_value")).Trim   
                '    Dim iPrecedence As Integer = Integer.MaxValue
                '    If Not String.IsNullOrEmpty(sPrecedence) Then
                '        Integer.TryParse(sPrecedence, iPrecedence)
                '    End If
                '    'If Not String.IsNullOrEmpty(groupName) Then 
                '    'Dim eventGuid As Guid = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(eventId).NodeGuidID
                '    Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(eventID)
                '    PM.EventsGroups.GroupName(tAlt) = groupName
                '    PM.EventsGroups.GroupPrecedence(tAlt) = iPrecedence
                '    'End If
                '    PRJ.MakeSnapshot(ParseString("Edit %%event%% group"), groupName)
                Case "num_sim"
                    Dim sVal As String = (GetParam(args, "val")).Trim   
                    Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                    If Not String.IsNullOrEmpty(sVal) Then
                        If Integer.TryParse(sVal, iVal) Then 'AndAlso LEC.NumberOfSimulations <> iVal AndAlso iVal > 0 AndAlso iVal <= _MAX_NUM_SIMULATIONS Then 
                            PM.RiskSimulations.NumberOfTrials = iVal
                        End If
                    End If
                    App.ActiveProject.MakeSnapshot("Number of simulations changed", PM.RiskSimulations.NumberOfTrials.ToString)
                Case "rand_seed"
                    Dim sVal As String = (GetParam(args, "val")).Trim   
                    Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                    If Not String.IsNullOrEmpty(sVal) Then
                        If Integer.TryParse(sVal, iVal) AndAlso PM.RiskSimulations.RandomSeed <> iVal AndAlso iVal > 0 Then
                            PM.RiskSimulations.RandomSeed = iVal
                        End If
                    End If
                    App.ActiveProject.MakeSnapshot("Random seed changed", PM.RiskSimulations.RandomSeed.ToString)
                Case "keep_rand_seed"
                    Dim bVal As Boolean = Param2Bool(args, "val")
                    PM.RiskSimulations.KeepRandomSeed = bVal
                    App.ActiveProject.MakeSnapshot("Keep random seed seeting changed", PM.RiskSimulations.KeepRandomSeed.ToString)
                Case "use_source_groups"
                    PM.RiskSimulations.UseSourceGroups = Param2Bool(args, "value")
                    App.ActiveProject.MakeSnapshot("Use source groups changed", PM.RiskSimulations.UseSourceGroups.ToString)
                Case "use_event_groups"
                    PM.RiskSimulations.UseEventsGroups = Param2Bool(args, "value")
                    App.ActiveProject.MakeSnapshot("Use event groups changed", PM.RiskSimulations.UseEventsGroups.ToString)
                Case "select_events", "select_controls"
                    If sAction = "select_events" Then
                        Dim StringSelectedEventIDs = (GetParam(args, "event_ids")).ToLower ' Anti-XSS
                        For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                            alt.Enabled = StringSelectedEventIDs.Contains(alt.NodeGuidID.ToString)
                        Next
                        PRJ.SaveStructure("Save active alternatives")
                    End If
                    If sAction = "select_controls" Then
                        Dim tStringSelectedControlIDs As String = (GetParam(args, "control_ids")).ToLower ' Anti-XSS
                        For Each ctrl As clsControl In PM.Controls.DefinedControls
                            ctrl.Enabled = tStringSelectedControlIDs.Contains(ctrl.ID.ToString)
                        Next
                        WriteControls(PRJ, ParseString(LogMessagePrefix + ": %%Control%%(s) enabled"), ParseString(LogMessagePrefix + ": %%Control%%(s) enabled"))
                        WriteAttributeValues(PRJ, ParseString(LogMessagePrefix + ": %%Control%%(s) enabled"), ParseString(LogMessagePrefix + ": %%Control%%(s) enabled")) 'TODO: 
                    End If
            End Select
        End If

        If sBowTieLikelihoodSort <> "" Then
            'don't change these two lines, otherwise IDE will show an error
            Dim newValue As BowTieSortState() = {CType([Enum].Parse(GetType(BowTieSortState), sBowTieLikelihoodSort), BowTieSortState), BowTieSort(1)}
            BowTieSort = newValue
        End If

        If sBowTieImpactSort <> "" Then
            'don't change these two lines, otherwise IDE will show an error
            Dim newValue As BowTieSortState() = {BowTieSort(0), CType([Enum].Parse(GetType(BowTieSortState), sBowTieImpactSort), BowTieSortState)}
            BowTieSort = newValue
        End If

        Dim sActivateControl As String = (GetParam(args, "activate_control")).ToLower.Trim  
        If sActivateControl <> "" Then
            Dim sCtrlID As String = (GetParam(args, "ctrl_id")).ToLower.Trim
            If sCtrlID <> "" Then
                ActivateControl(New Guid(sCtrlID), CInt(sActivateControl))
            End If
        End If

        If sRegionsParams <> "" AndAlso (sRegionsParams = "1" OrElse sRegionsParams = "true") Then
            Dim h As Double = 0
            Dim l As Double = 0
            If String2Double((GetParam(args, "h")).Trim, h) AndAlso String2Double((GetParam(args, "l")).Trim, l) AndAlso l <= h Then
                _Rh = h / 100
                _Rl = l / 100

                PM.Parameters.Riskion_Regions_Rh_Color = (GetParam(args, "r")).Trim  
                PM.Parameters.Riskion_Regions_Rm_Color = (GetParam(args, "w")).Trim
                PM.Parameters.Riskion_Regions_Rl_Color = (GetParam(args, "g")).Trim

                PM.Parameters.Save()

                'With PM
                '    .Regions.Cells = New List(Of Long)
                '    .Regions.ColumnCount = 6
                '    .Regions.RowCount = 1
                '
                '    .Regions.Cells.Add(0) 'Size
                '    .Regions.Cells.Add(CLng(h * 100000)) 'Rh *100 000
                '    .Regions.Cells.Add(CLng(l * 100000)) 'Rl *100 000
                '
                '    .Regions.Cells.Add(BrushToLong(_redBrush.Replace("#", ""))) 'Red
                '    .Regions.Cells.Add(BrushToLong(_whiteBrush.Replace("#", ""))) 'Yellow
                '    .Regions.Cells.Add(BrushToLong(_greenBrush.Replace("#", ""))) 'Green
                '
                '    .Regions.WriteRegions(.StorageManager.StorageType, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                'End With
            End If
        End If

        If RadTreeHierarchy.SelectedNode IsNot Nothing Then
            If Not IsRiskResultsByEventPage AndAlso (Not IsBowTiePage OrElse IsBowTieByObjectivePage) Then
                selected_bow_tie_wrt_node = New Guid(RadTreeHierarchy.SelectedNode.Value)
                SelectedHierarchyNodeID = selected_bow_tie_wrt_node
            Else
                SelectedEventID = New Guid(RadTreeHierarchy.SelectedNode.Value)
            End If
        End If

        Dim sShowAttrs As String = (GetParam(args, "showattributes")).ToLower.Trim  
        If sShowAttrs <> "" Then
            ShowAttributes = sShowAttrs = "1"
        End If

        UpdateControl()
    End Sub

    Protected Sub RadTreeHierarchy_Load(sender As Object, e As EventArgs) Handles RadTreeHierarchy.Load
        If RadTreeHierarchy.Nodes.Count = 0 Then
            LoadHierarchy()
            UpdateControl()
        End If
    End Sub

    Private Function IsUserChecked(UserID As Integer) As Boolean
        Return SelectedUsersAndGroupsIDs.Contains(UserID)
    End Function

    Private Function GetNewColumnName(tDataTable As DataTable, tSuggestedName As String, tUniqueID As String, Optional LIRformat As Boolean = True) As String 'A1201
        If LIRformat AndAlso Not IsRiskResultsByEventPage Then tSuggestedName = String.Format("L, I, R ({0})", tSuggestedName) 'A1201
        If tDataTable.Columns IsNot Nothing AndAlso tDataTable.Columns.Count > 0 Then
            For Each clmn As DataColumn In tDataTable.Columns
                If clmn.ColumnName = tSuggestedName Then Return String.Format("{0} ({1})", tSuggestedName, tUniqueID)
            Next
        End If
        Return tSuggestedName
    End Function

    Private Sub AddRealValuesColumns(ByRef dtvals As DataTable)
        Dim idx As String = UsersList.Count.ToString
        dtvals.Columns.Add(New DataColumn("L" + idx, GetType(Double)))
        dtvals.Columns.Add(New DataColumn("I" + idx, GetType(Double)))
        dtvals.Columns.Add(New DataColumn("R" + idx, GetType(Double)))
    End Sub

    ''Sum of sources >1
    'Private _IsSOSG1 As Boolean = False
    'Public Property IsSOSG1 As Boolean
    '    Get
    '        Return _IsSOSG1
    '    End Get
    '    Set(value As Boolean)
    '        _IsSOSG1 = value
    '    End Set
    'End Property
    '
    ''Sum of events likelihoods >1
    'Private _IsSOEG1 As Boolean = False
    'Public Property IsSOEG1 As Boolean
    '    Get
    '        Return _IsSOEG1
    '    End Get
    '    Set(value As Boolean)
    '        _IsSOEG1 = value
    '    End Set
    'End Property
    '
    ''Sum of objectives (impacts) >1
    'Private _IsSOOG1 As Boolean = False
    'Public Property IsSOOG1 As Boolean
    '    Get
    '        Return _IsSOOG1
    '    End Get
    '    Set(value As Boolean)
    '        _IsSOOG1 = value
    '    End Set
    'End Property
    '
    ''Sum of event impacts >1
    'Private _IsSOIG1 As Boolean = False
    'Public Property IsSOIG1 As Boolean
    '    Get
    '        Return _IsSOIG1
    '    End Get
    '    Set(value As Boolean)
    '        _IsSOIG1 = value
    '    End Set
    'End Property
    '
    ''One of events likelihoods >1
    'Private _IsEG1 As Boolean = False
    'Public Property IsEG1 As Boolean
    '    Get
    '        Return _IsEG1
    '    End Get
    '    Set(value As Boolean)
    '        _IsEG1 = value
    '    End Set
    'End Property
    '
    ''One of events impacts >1
    'Private _IsIG1 As Boolean = False
    'Public Property IsIG1 As Boolean
    '    Get
    '        Return _IsIG1
    '    End Get
    '    Set(value As Boolean)
    '        _IsIG1 = value
    '    End Set
    'End Property
    '
    'Private Sub InitIndicators(riskData As RiskDataWRTNodeDataContract)
    '    If Not _IsSOSG1 Then _IsSOSG1 = riskData.IsSOSG1
    '    If Not _IsSOEG1 Then _IsSOEG1 = riskData.IsSOEG1
    '    If Not _IsSOOG1 Then _IsSOOG1 = riskData.IsSOOG1
    '    If Not _IsSOIG1 Then _IsSOIG1 = riskData.IsSOIG1
    '    If Not _IsEG1 Then _IsEG1 = riskData.IsEG1
    '    If Not _IsIG1 Then _IsIG1 = riskData.IsIG1
    'End Sub

    Private Sub CalculateSumAndMaxPriorities(ByRef Dict As Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract)), VisibleAlternatives As List(Of clsNode))
        For Each tUser In Dict
            Dim maxLikelihood As Double = 0
            Dim maxImpact As Double = 0
            Dim maxRisk As Double = 0

            For Each alt In VisibleAlternatives
                Dim altVal As AlternativeRiskDataDataContract = Nothing
                For Each item In tUser.Value
                    If item.AlternativeID.Equals(alt.NodeGuidID) Then altVal = item
                Next
                If altVal IsNot Nothing Then
                    If maxLikelihood < altVal.LikelihoodValue Then maxLikelihood = altVal.LikelihoodValue
                    If maxImpact < altVal.ImpactValue Then maxImpact = altVal.ImpactValue
                    If maxRisk < altVal.RiskValue Then maxRisk = altVal.RiskValue
                End If
            Next

            If Math.Abs(maxLikelihood) < 0.0001 Then maxLikelihood = 1
            If Math.Abs(maxImpact) < 0.0001 Then maxImpact = 1

            If Math.Abs(maxRisk) < 0.0001 Then maxRisk = 1

            For Each altVal In tUser.Value
                altVal.MaxLikelihood = maxLikelihood
                altVal.MaxImpact = maxImpact
                altVal.MaxRisk = maxRisk
            Next
        Next
    End Sub

    Private Sub InitAlternatives()
        Dim tProjectAltsList = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
        For Each alt As clsNode In tProjectAltsList
            If alt.SOrder = 0 Then alt.SOrder = tProjectAltsList.IndexOf(alt) + 1
        Next
    End Sub

    Private SimulationEventsDataForAllUsers As Dictionary(Of Integer, List(Of Integer))
    'Private SimulationResults As Dictionary(Of Integer, List(Of clsSimulationData)) = New Dictionary(Of Integer, List(Of clsSimulationData))
    'Private SimulationResultsWithoutControls As Dictionary(Of Integer, List(Of clsSimulationData)) = New Dictionary(Of Integer, List(Of clsSimulationData))
    Private DataDict As Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))
    Private DataDictWithoutControls As Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))

    Function CreateDataSource() As DataView 'ICollection
        Dim dt As New DataTable()
        Dim dr As DataRow

        MaxPriorityL = Double.MinValue
        MaxPriorityI = Double.MinValue
        MaxPriorityR = Double.MinValue

        TotalMaxRisk = 0

        SimulationEventsDataForAllUsers = New Dictionary(Of Integer, List(Of Integer))
        'SimulationResults = New Dictionary(Of Integer, List(Of clsSimulationData))
        'SimulationResultsWithoutControls = New Dictionary(Of Integer, List(Of clsSimulationData))

        Dim UserRankAttr As clsAttribute = PM.Attributes.GetAttributeByID(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID)

        Dim tShowDollarValue As Boolean = ShowDollarValue
        Dim tSimulationWRTNodeGuid As Guid = Guid.Empty
        Dim SelectedWRTNode As clsNode = Nothing

        'Dim tShowResultsMode As ShowResultsModes = ShowResultsMode

        DataDict = New Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))
        DataDictWithoutControls = New Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))

        ' simulate loss exceedance for showing average loss        
        Dim tSourcesType As Integer = 0 ' Independent
        Dim dMaxValue As Double = 0

        AverageLoss.Clear()
        AverageLossWithoutControls.Clear()
        InvestmentLeverage.Clear()

        If RadTreeHierarchy.SelectedNode IsNot Nothing AndAlso RadTreeHierarchy.Nodes.Count > 0 Then
            Dim SelectedNodeGuid As Guid = New Guid(RadTreeHierarchy.SelectedNode.Value)
            SelectedWRTNode = PM.Hierarchy(HierarchyID).GetNodeByID(SelectedNodeGuid)
            Dim isForGoalLikelihood As Boolean = HierarchyID = ECHierarchyID.hidLikelihood AndAlso SelectedWRTNode.ParentNodeID = -1
            Dim tCurrentFiltersList = CurrentFiltersList
            Dim tIsGroupByAttribute As Boolean = IsGroupByAttribute
            Dim tShowGroupTotals As Boolean = ShowGroupTotals
            Dim VisibleAlternatives As New List(Of clsNode)
            Dim tDecimalDigits As Integer = PM.Parameters.DecimalDigits
            Dim tProjectAltsList = OrderedEvents

            For Each alt As clsNode In tProjectAltsList
                If isForGoalLikelihood OrElse AnySubObjectiveContributes(SelectedWRTNode, alt.NodeID) Then
                    If Not SkipAlternative(alt, tCurrentFiltersList) Then
                        VisibleAlternatives.Add(alt)
                    End If
                End If
            Next

            'grouping by attribute values:
            Dim dictAvailableCategories As New Dictionary(Of String, Char)

            If IsGroupByAttribute Then
                Dim ID As Guid = GroupByAttributeGUID

                If Not ID.Equals(Guid.Empty) AndAlso (AttributesList.Where(Function(attr) attr.ID.Equals(ID)).Count > 0 OrElse (ID = GroupByRiskOpportunity AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel))) Then  ' D6798
                    If Not dictAvailableCategories.ContainsKey(Guid.Empty.ToString) Then dictAvailableCategories.Add(Guid.Empty.ToString, Char.MaxValue) 'the event with no category assigned will be last in the list

                    ' first sort
                    For Each alt In VisibleAlternatives
                        Dim sObjValue As String
                        If (ID = GroupByRiskOpportunity AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel)) Then   ' D6798
                            sObjValue = (alt.EventType).ToString
                        Else
                            sObjValue = PM.Attributes.GetAttributeValueString(ID, alt.NodeGuidID)
                        End If

                        If Not String.IsNullOrEmpty(sObjValue) Then
                            alt.Tag = sObjValue
                        Else
                            alt.Tag = Char.MaxValue.ToString
                        End If
                    Next

                    VisibleAlternatives = VisibleAlternatives.OrderBy(Function(alt) CStr(alt.Tag)).ToList

                    For Each alt In VisibleAlternatives
                        Dim tObjValue As Object
                        If (ID = GroupByRiskOpportunity AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel)) Then   ' D6798
                            tObjValue = alt.EventType
                        Else
                            tObjValue = PM.Attributes.GetAttributeValue(ID, alt.NodeGuidID)
                        End If

                        If tObjValue IsNot Nothing Then
                            Dim tKey As String = Guid.Empty.ToString
                            If TypeOf tObjValue Is Guid Then tKey = CType(tObjValue, Guid).ToString
                            If TypeOf tObjValue Is String Then tKey = CStr(tObjValue)
                            If TypeOf tObjValue Is EventType Then tKey = CStr(tObjValue)
                            If Not dictAvailableCategories.ContainsKey(tKey) Then dictAvailableCategories.Add(tKey, Chr(dictAvailableCategories.Keys.Count))
                            alt.Tag = dictAvailableCategories(tKey)
                        Else
                            alt.Tag = dictAvailableCategories(Guid.Empty.ToString)
                        End If
                    Next
                    VisibleAlternatives = VisibleAlternatives.OrderBy(Function(alt) Asc(CChar(alt.Tag))).ToList
                End If
            End If

            dt.Columns.Add(New DataColumn(If(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank, ResString(If(IsWidget, "tblRiskResultsRank", "optRank")), ResString("tblNo_")), GetType(Int32)))
            dt.Columns.Add(New DataColumn("colGUID", GetType(String)))
            dt.Columns.Add(New DataColumn(ResString("lblRiskResultsEvents"), GetType(String)))

            'If IsWorstCaseVisible Then
            'dt.Columns.Add(New DataColumn("Worst Avg.", GetType(Double)))
            dt.Columns.Add(New DataColumn("Max " + ResString("lblRisk"), GetType(Double)))
            'End If
            'If IsGroupByAttribute Then
            dt.Columns.Add(New DataColumn("ALT_TAG", GetType(Int32)))
            dt.Columns.Add(New DataColumn("IS_SUMMARY", GetType(String)))
            'End If
            ' show attributes
            Dim tSelectedColumns As String = SelectedColumns
            With PM
                For Each tAttr As clsAttribute In .Attributes.AttributesList
                    If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAttributesType Then
                        If SelectedColumns.Contains(tAttr.ID.ToString) Then
                            dt.Columns.Add(New DataColumn(GetNewColumnName(dt, App.GetAttributeName(tAttr), tAttr.ID.ToString, False), GetType(String)))
                            COL_USERS_START += 1
                        End If
                    End If
                Next
            End With

            '_IsSOSG1 = False
            '_IsSOEG1 = False
            '_IsSOOG1 = False
            '_IsSOIG1 = False
            '_IsEG1 = False
            '_IsIG1 = False

            'calculate
            'StartColumnIdx = dt.Columns.Count   ' D2534
            UsersList.Clear()     ' D2534
            With PM
                If .CombinedGroups IsNot Nothing AndAlso .CombinedGroups.GroupsList IsNot Nothing Then
                    For Each tGroup As clsCombinedGroup In .CombinedGroups.GroupsList
                        If IsUserChecked(tGroup.CombinedUserID) AndAlso Not App.Options.isSingleModeEvaluation AndAlso Not IsWidget OrElse IsRiskMapPage AndAlso tGroup.CombinedUserID = SelectedUserOrGroupID Then    ' D6048
                            ' D2534 ===
                            Dim tColumnName As String = ShortString(CStr(IIf(tGroup.Rule = "", tGroup.Name, tGroup.Rule)), 20, True)
                            UsersList.Add(tColumnName)  ' D2534
                            tColumnName = GetNewColumnName(dt, tColumnName, tGroup.CombinedUserID.ToString)
                            dt.Columns.Add(New DataColumn(tColumnName, GetType(String)))
                            AddRealValuesColumns(dt)
                            ' D2534 ==
                            Dim riskData As RiskDataWRTNodeDataContract = .CalculationsManager.GetRiskDataWRTNode(tGroup.CombinedUserID, "", SelectedNodeGuid, ControlsUsageMode.UseOnlyActive) 'A1064
                            DataDict.Add(tGroup.CombinedUserID, riskData.AlternativesData)
                            'InitIndicators(riskData)

                            Dim fSimulatedRiskWithoutControls As Double = 0
                            Dim fSimulatedRiskWithControls As Double = 0

                            If PM.CalculationsManager.UseSimulatedValues Then 'OrElse ShowResultsMode = ShowResultsModes.rmBoth Then                                
                                'LEC.InitSimulatedValues(If(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), PRJ.isOpportunityModel)
                                'Dim simData As List(Of clsSimulationData) = LEC.SimulateLossExceedance(UserID, UserEmail, SimulationEventsDataForAllUsers.Values(i), clsLEC._SIMULATE_SOURCES, False, CType(IIf(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode), dMaxValue, New Random(RandSeed), True, False, tSourcesType, AttributesList, Guid.Empty, False, tSimulationWRTNodeGuid, PRJ.isOpportunityModel)
                                PM.RiskSimulations.SimulateCommon(tGroup.CombinedUserID, If(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), SelectedWRTNode, NormalizedLkhd <> SynthesisMode.smAbsolute, True)
                                fSimulatedRiskWithControls = PM.RiskSimulations.SimulatedRisk
                                'TODO
                                For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                                    Dim list = DataDict(tGroup.CombinedUserID)
                                    For Each item In list
                                        If item.AlternativeID = alt.NodeGuidID Then
                                            item.LikelihoodValue = alt.SimulatedAltLikelihood
                                            item.ImpactValue = alt.SimulatedAltImpact
                                            item.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                                        End If
                                    Next
                                Next
                                'SimulationResults(UserID) = simData
                                AverageLoss.Add(PM.RiskSimulations.AverageLossValue)
                                AverageLoss.Add(0)
                                AverageLoss.Add(0)
                                AverageLoss.Add(0)
                            End If

                            'If ShowRiskReduction AndAlso UseControlsReductions Then
                            Dim riskDataWithoutControls As RiskDataWRTNodeDataContract = .CalculationsManager.GetRiskDataWRTNode(tGroup.CombinedUserID, "", SelectedNodeGuid, ControlsUsageMode.DoNotUse) 'A1064
                            DataDictWithoutControls.Add(tGroup.CombinedUserID, riskDataWithoutControls.AlternativesData)
                            'End If

                            If PM.CalculationsManager.UseSimulatedValues Then 'OrElse ShowResultsMode = ShowResultsModes.rmBoth Then                                
                                'simulate for without controls
                                If UseControlsReductions OrElse ShowTotalRisk OrElse Not IsRiskWithControlsPage Then
                                    'Dim simDataWC As List(Of clsSimulationData) = LEC.SimulateLossExceedance(UserID, UserEmail, SimulationEventsDataForAllUsers.Values(i), clsLEC._SIMULATE_SOURCES, False, ControlsUsageMode.DoNotUse, dMaxValue, New Random(RandSeed), True, False, tSourcesType, AttributesList, Guid.Empty, False, tSimulationWRTNodeGuid, PRJ.isOpportunityModel)                    
                                    PM.RiskSimulations.SimulateCommon(tGroup.CombinedUserID, ControlsUsageMode.DoNotUse, SelectedWRTNode, NormalizedLkhd <> SynthesisMode.smAbsolute)
                                    fSimulatedRiskWithoutControls = PM.RiskSimulations.SimulatedRisk
                                    For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                                        Dim list = DataDictWithoutControls(tGroup.CombinedUserID)
                                        For Each item In list
                                            If item.AlternativeID = alt.NodeGuidID Then
                                                item.LikelihoodValue = alt.SimulatedAltLikelihood
                                                item.ImpactValue = alt.SimulatedAltImpact
                                                item.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                                            End If
                                        Next
                                    Next
                                    'TODO
                                    'SimulationResultsWithoutControls(UserID) = simDataWC
                                    'AverageLossWithoutControls.Add(0) 'simDataWC.Average(Function(simItem) simItem.Total))
                                    AverageLossWithoutControls.Add(PM.RiskSimulations.AverageLossValue)
                                    AverageLossWithoutControls.Add(0)
                                    AverageLossWithoutControls.Add(0)
                                    AverageLossWithoutControls.Add(0)
                                End If
                            End If

                            Dim tSimulatedRiskReduction As Double = (fSimulatedRiskWithoutControls - fSimulatedRiskWithControls) * DollarValueOfEnterprise

                            InvestmentLeverage.Add(If(PM.Controls.CostOfFundedControls() = 0, 0, tSimulatedRiskReduction / PM.Controls.CostOfFundedControls()))
                            InvestmentLeverage.Add(0)
                            InvestmentLeverage.Add(0)
                            InvestmentLeverage.Add(0)
                        End If
                    Next
                End If

                For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                    Dim tAHPUser As clsUser = .GetUserByEMail(tAppUser.UserEmail)
                    If tAHPUser IsNot Nothing Then tAppUser.UserName = tAHPUser.UserName
                    If tAHPUser IsNot Nothing AndAlso (IsUserChecked(tAHPUser.UserID) OrElse ((App.Options.isSingleModeEvaluation OrElse IsWidget) AndAlso .User.UserEMail.ToLower = tAppUser.UserEmail.ToLower)) OrElse IsRiskMapPage AndAlso tAHPUser.UserID = SelectedUserOrGroupID Then  ' D6048 + D6058
                        ' D2534 ===
                        Dim sUsername As String = ShortString(CStr(IIf(Not String.IsNullOrEmpty(tAppUser.UserName), tAppUser.UserName, tAppUser.UserEmail)), 20, True)
                        Dim tColumnName As String = GetNewColumnName(dt, sUsername, tAHPUser.UserGuidID.ToString)
                        UsersList.Add(sUsername)
                        dt.Columns.Add(New DataColumn(tColumnName, GetType(String)))
                        AddRealValuesColumns(dt)

                        Dim fSimulatedRiskWithoutControls As Double = 0
                        Dim fSimulatedRiskWithControls As Double = 0

                        ' D2534 ==
                        Dim riskData As RiskDataWRTNodeDataContract = .CalculationsManager.GetRiskDataWRTNode(tAHPUser.UserID, tAppUser.UserEmail, SelectedNodeGuid, CType(IIf(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode)) 'A1064
                        DataDict.Add(tAppUser.UserID, riskData.AlternativesData)
                        'InitIndicators(riskData)
                        If PM.CalculationsManager.UseSimulatedValues Then 'OrElse ShowResultsMode = ShowResultsModes.rmBoth Then                                

                            'LEC.InitSimulatedValues(If(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), PRJ.isOpportunityModel)
                            'Dim simData As List(Of clsSimulationData) = LEC.SimulateLossExceedance(UserID, UserEmail, SimulationEventsDataForAllUsers.Values(i), clsLEC._SIMULATE_SOURCES, False, CType(IIf(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode), dMaxValue, New Random(RandSeed), True, False, tSourcesType, AttributesList, Guid.Empty, False, tSimulationWRTNodeGuid, PRJ.isOpportunityModel)
                            PM.RiskSimulations.SimulateCommon(tAHPUser.UserID, If(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), SelectedWRTNode, NormalizedLkhd <> SynthesisMode.smAbsolute, True)
                            fSimulatedRiskWithControls = PM.RiskSimulations.SimulatedRisk
                            'TODO
                            For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                                Dim list = DataDict(tAppUser.UserID)
                                For Each item In list
                                    If item.AlternativeID = alt.NodeGuidID Then
                                        item.LikelihoodValue = alt.SimulatedAltLikelihood
                                        item.ImpactValue = alt.SimulatedAltImpact
                                        item.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                                    End If
                                Next
                            Next
                            'SimulationResults(UserID) = simData
                            AverageLoss.Add(PM.RiskSimulations.AverageLossValue)
                            AverageLoss.Add(0)
                            AverageLoss.Add(0)
                            AverageLoss.Add(0)
                        End If

                        'If ShowRiskReduction AndAlso UseControlsReductions Then
                        Dim riskDataWithoutControls As RiskDataWRTNodeDataContract = .CalculationsManager.GetRiskDataWRTNode(tAHPUser.UserID, tAppUser.UserEmail, SelectedNodeGuid, ControlsUsageMode.DoNotUse) 'A1064
                        DataDictWithoutControls.Add(tAppUser.UserID, riskDataWithoutControls.AlternativesData)
                        'End If

                        If PM.CalculationsManager.UseSimulatedValues Then 'OrElse ShowResultsMode = ShowResultsModes.rmBoth Then                                
                            'simulate for without controls
                            If UseControlsReductions OrElse ShowTotalRisk OrElse Not IsRiskWithControlsPage Then
                                'Dim simDataWC As List(Of clsSimulationData) = LEC.SimulateLossExceedance(UserID, UserEmail, SimulationEventsDataForAllUsers.Values(i), clsLEC._SIMULATE_SOURCES, False, ControlsUsageMode.DoNotUse, dMaxValue, New Random(RandSeed), True, False, tSourcesType, AttributesList, Guid.Empty, False, tSimulationWRTNodeGuid, PRJ.isOpportunityModel)                    
                                PM.RiskSimulations.SimulateCommon(tAHPUser.UserID, ControlsUsageMode.DoNotUse, SelectedWRTNode, NormalizedLkhd <> SynthesisMode.smAbsolute)
                                fSimulatedRiskWithoutControls = PM.RiskSimulations.SimulatedRisk
                                For Each alt As clsNode In PM.ActiveAlternatives.TerminalNodes
                                    Dim list = DataDictWithoutControls(tAppUser.UserID)
                                    For Each item In list
                                        If item.AlternativeID = alt.NodeGuidID Then
                                            item.LikelihoodValue = alt.SimulatedAltLikelihood
                                            item.ImpactValue = alt.SimulatedAltImpact
                                            item.RiskValue = alt.SimulatedAltLikelihood * alt.SimulatedAltImpact
                                        End If
                                    Next
                                Next
                                'TODO
                                'SimulationResultsWithoutControls(UserID) = simDataWC
                                'AverageLossWithoutControls.Add(0) 'simDataWC.Average(Function(simItem) simItem.Total))
                                AverageLossWithoutControls.Add(PM.RiskSimulations.AverageLossValue)
                                AverageLossWithoutControls.Add(0)
                                AverageLossWithoutControls.Add(0)
                                AverageLossWithoutControls.Add(0)
                            End If
                        End If

                        Dim tSimulatedRiskReduction As Double = (fSimulatedRiskWithoutControls - fSimulatedRiskWithControls) * DollarValueOfEnterprise

                        InvestmentLeverage.Add(If(PM.Controls.CostOfFundedControls() = 0, 0, tSimulatedRiskReduction / PM.Controls.CostOfFundedControls()))
                        InvestmentLeverage.Add(0)
                        InvestmentLeverage.Add(0)
                        InvestmentLeverage.Add(0)
                    End If
                Next
            End With

            IsSingleUserSelected = DataDict.Count = 1
            ShowGridLegend = DataDict.Count > 0    ' D2188
            SimulationUserID = If(DataDict.Count > 0, DataDict.Keys.First(), UNDEFINED_USER_ID)
            tSimulationWRTNodeGuid = CType(IIf(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, SelectedHierarchyNodeID), Guid)
            SimulationWRTNodeGuid = tSimulationWRTNodeGuid

            'Dim tNormalizedLkhd As SynthesisMode = NormalizedLkhd
            'Dim tNormalizedLkhdLocal As LocalNormalizationType = NormalizedLocalLkhd
            'Dim tNormalizedImpcLocal As LocalNormalizationType = NormalizedLocalImpact

            'If Not IsPostBack AndAlso Not IsCallback Then
            '    'IsFilterApplied = False
            '    If tNormalizedLkhd = SynthesisMode.smUndefined Then
            '        tNormalizedLkhd = SynthesisMode.smAbsolute
            '        NormalizedLkhd = tNormalizedLkhd
            '        tNormalizedLkhdLocal = LocalNormalizationType.ntUnnormalized
            '        NormalizedLocalLkhd = tNormalizedLkhdLocal
            '        If AutoSwitchNormalization Then
            '            If _IsSOEG1 OrElse _IsSOSG1 Then
            '                tNormalizedLkhd = SynthesisMode.smRelativeB
            '                NormalizedLkhd = tNormalizedLkhd
            '            End If
            '        End If
            '    End If
            'End If

            CalculateSumAndMaxPriorities(DataDict, VisibleAlternatives)
            'If ShowRiskReduction AndAlso UseControlsReductions Then
            CalculateSumAndMaxPriorities(DataDictWithoutControls, VisibleAlternatives)
            'End If

            'IsGroupByCategory store variables for current category
            Dim CurType As Integer = -1
            Dim sr As DataRow = Nothing 'summary row
            Dim TotalRiskByCategory As List(Of Double) = Nothing
            Dim TotalRiskReductionByCategory As List(Of Double) = Nothing
            Dim TotalMaxRiskByCategory As Double = -1

            For Each item In DataDict
                SimulationEventsDataForAllUsers.Add(item.Key, New List(Of Integer))
                'SimulationResults.Add(item.Key, New List(Of clsSimulationData))
                'SimulationResultsWithoutControls.Add(item.Key, New List(Of clsSimulationData))
            Next

            For Each alt As clsNode In VisibleAlternatives
                alt.UserRank = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, alt.NodeGuidID, UserRankAttr, PM.User.UserID))
                'add summary row
                If tIsGroupByAttribute AndAlso ((Not GroupByAttributeGUID.Equals(Guid.Empty) AndAlso alt.Tag IsNot Nothing AndAlso TypeOf alt.Tag Is Char AndAlso CurType <> Asc(CChar(alt.Tag)))) Then
                    CurType = Asc(CChar(alt.Tag))

                    If IsWorstCaseVisible AndAlso TotalMaxRiskByCategory >= 0 Then sr(COL_WORST_CASE) = TotalMaxRiskByCategory.ToString
                    If TotalRiskByCategory IsNot Nothing AndAlso tShowGroupTotals Then
                        For t As Integer = 0 To TotalRiskByCategory.Count - 1
                            If Not PM.CalculationsManager.UseSimulatedValues Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                                If tShowDollarValue Then
                                    sr(COL_USERS_START + t * 4) = CostString(TotalRiskByCategory(t), CostDecimalDigits, True)
                                Else
                                    sr(COL_USERS_START + t * 4) = Double2String(TotalRiskByCategory(t) * 100, tDecimalDigits, True)
                                End If
                                If ShowRiskReduction AndAlso UseControlsReductions Then
                                    Dim sRiskREductionByCat As String
                                    If tShowDollarValue Then
                                        sRiskREductionByCat = CostString(TotalRiskReductionByCategory(t), CostDecimalDigits, True)
                                    Else
                                        sRiskREductionByCat = Double2String(TotalRiskReductionByCategory(t) * 100, tDecimalDigits, True)
                                    End If
                                    sr(COL_USERS_START + t * 4) = CStr(sr(COL_USERS_START + t * 4)) + String.Format(ParseString(" (%%risk%% reduction: {0})"), sRiskREductionByCat)
                                End If
                            End If
                            'If tShowResultsMode = ShowResultsModes.rmSimulated OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                            '    Dim userIndex As Integer = t
                            '    Dim SimulatedRiskByCategory As Double = GetSimulatedRiskByCategory(CurType, userIndex)
                            '    'Dim SimulatedRiskReductionByCategory As Double = 0
                            '    Dim s As String = ""
                            '    If Not IsDBNull(sr(COL_USERS_START + t * 4)) Then s = CStr(sr(COL_USERS_START + t * 4))
                            '    If tShowDollarValue Then
                            '        sr(COL_USERS_START + t * 4) = s + ParseString(" %%Loss%%: ") + CostString(SimulatedRiskByCategory, CostDecimalDigits, True)
                            '    Else
                            '        sr(COL_USERS_START + t * 4) = s + ParseString(" %%Loss%%: ") + Double2String(SimulatedRiskByCategory * 100, tDecimalDigits, True)
                            '    End If
                            '    'If ShowRiskReduction AndAlso UseControlsReductions Then
                            '    '    Dim sRiskREductionByCat As String
                            '    '    If tShowDollarValue
                            '    '        sRiskREductionByCat = CostString(SimulatedRiskReductionByCategory, CostDecimalDigits, True)
                            '    '    Else
                            '    '        sRiskREductionByCat = Double2String(SimulatedRiskReductionByCategory * 100, tDecimalDigits, True)
                            '    '    End If
                            '    '    sr(COL_USERS_START + t * 4) = CStr(sr(COL_USERS_START + t * 4)) + String.Format(ParseString(" (%%risk%% reduction: {0})"), sRiskReductionByCat)
                            '    'End If
                            'End If
                        Next
                    End If

                    TotalRiskByCategory = New List(Of Double)
                    TotalRiskReductionByCategory = New List(Of Double)
                    TotalMaxRiskByCategory = 0

                    For Each tUser In DataDict
                        TotalRiskByCategory.Add(0)
                        TotalRiskReductionByCategory.Add(0)
                    Next

                    Dim tCategoryName As String = ResString("lblNoCategory")
                    Dim ID As Guid = GroupByAttributeGUID
                    Dim tAttr = PM.Attributes.GetAttributeByID(ID)
                    If Not ID.Equals(Guid.Empty) AndAlso (tAttr IsNot Nothing OrElse (GroupByAttributeGUID = GroupByRiskOpportunity AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel))) Then    ' D6798
                        Dim key = dictAvailableCategories.Where(Function(kvp) Asc(CChar(kvp.Value)) = CurType)(0).Key
                        If GroupByAttributeGUID = GroupByRiskOpportunity AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) Then
                            If alt.EventType = EventType.Opportunity Then tCategoryName = ParseString("%%Opportunity%%")
                            If alt.EventType = EventType.Risk Then tCategoryName = ParseString("%%Risk%%")
                        Else
                            Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                            Dim tEnumItem As clsAttributeEnumerationItem = Nothing
                            If tEnum IsNot Nothing Then
                                If tAttr.ValueType = AttributeValueTypes.avtEnumeration Then
                                    tEnumItem = tEnum.GetItemByID(New Guid(dictAvailableCategories.Where(Function(kvp) Asc(CChar(kvp.Value)) = CurType)(0).Key))
                                    If tEnumItem IsNot Nothing Then tCategoryName = tEnumItem.Value
                                End If
                                If tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                                    Dim sKey As String = dictAvailableCategories.Where(Function(kvp) Asc(CChar(kvp.Value)) = CurType)(0).Key
                                    Dim sName As String = ""
                                    For Each tEnumItem In tEnum.Items
                                        If sKey.Contains(tEnumItem.ID.ToString) Then
                                            sName += CStr(IIf(sName = "", "", ",")) + tEnumItem.Value
                                        End If
                                    Next
                                    If sName <> "" Then tCategoryName = sName
                                End If
                            End If
                        End If
                    End If

                    ' Category Header
                    sr = dt.NewRow()
                    sr(COL_IS_SUMMARY) = (CurType - 1).ToString + " visible header summary"
                    sr(COL_NAME) = tCategoryName  'category name
                    sr(COL_WORST_CASE) = 0
                    sr(COL_TAG) = CurType
                    dt.Rows.Add(sr)

                    If tShowGroupTotals Then
                        ' Category Footer
                        sr = dt.NewRow()
                        sr(COL_IS_SUMMARY) = CurType.ToString + " summary"
                        sr(COL_NAME) = tCategoryName  'category name
                        sr(COL_WORST_CASE) = 0
                        sr(COL_TAG) = CurType
                        dt.Rows.Add(sr)
                    End If
                End If

                dr = dt.NewRow()
                dr(COL_GUID) = alt.NodeGuidID.ToString
                'dr(COL_ID) = alt.SOrder
                dr(COL_ID) = alt.iIndex 'A1390
                dr(COL_NAME) = String.Format("{0}", alt.NodeName)
                dr(COL_IS_SUMMARY) = CurType.ToString

                ' add attributes values if columns visible
                With PM
                    Dim idc As Integer = 0
                    For Each tAttr As clsAttribute In .Attributes.AttributesList
                        If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAttributesType Then
                            If SelectedColumns.Contains(tAttr.ID.ToString) Then
                                Dim SValue As String = .Attributes.GetAttributeValueString(tAttr.ID, alt.NodeGuidID)
                                dr(COL_ATTR_START + idc) = SValue
                                idc += 1
                            End If
                        End If
                    Next
                End With

                If alt.Tag IsNot Nothing AndAlso TypeOf alt.Tag Is Char Then dr(COL_TAG) = alt.Tag

                Dim tWorstValue As Double = 0

                'Dim tWRTNodeGlobalPriority As Double = 1
                'Dim wrtNode As ECCore.clsNode = PM.Hierarchy(PM.ActiveHierarchy).GetNodeByID(SelectedHierarchyNodeID)
                'If wrtNode IsNot Nothing Then tWRTNodeGlobalPriority = wrtNode.WRTGlobalPriorityOriginal

                Dim j As Integer = 0  ' D2534
                For Each tUser In DataDict
                    Dim tUserValue As New AlternativeRiskDataDataContract With {.AlternativeID = alt.NodeGuidID, .LikelihoodValue = 0, .ImpactValue = 0, .RiskValue = 0}
                    Dim tUserValueWithoutControls As New AlternativeRiskDataDataContract With {.AlternativeID = alt.NodeGuidID, .LikelihoodValue = 0, .ImpactValue = 0, .RiskValue = 0}
                    For Each altVal In tUser.Value
                        If altVal.AlternativeID.Equals(alt.NodeGuidID) Then
                            tUserValue.SumImpact = altVal.SumImpact
                            tUserValue.MaxImpact = altVal.MaxImpact

                            tUserValue.LikelihoodValue = altVal.LikelihoodValue
                            tUserValue.ImpactValue = altVal.ImpactValue
                            tUserValue.RiskValue = altVal.RiskValue
                        End If
                    Next

                    'If tNormalizedLkhd <> SynthesisMode.smRelativeB Then
                    'If ShowRiskReduction AndAlso UseControlsReductions Then
                    For Each altVal In DataDictWithoutControls(tUser.Key)
                        If altVal.AlternativeID.Equals(alt.NodeGuidID) Then
                            'Select Case tNormalizedLkhd
                            '    Case SynthesisMode.smAbsolute
                            tUserValueWithoutControls.LikelihoodValue = altVal.LikelihoodValue '* tWRTNodeGlobalPriority
                            '    Case Else 'SynthesisMode.smRelative, SynthesisMode.smRelativeB
                            'tUserValueWithoutControls.LikelihoodValue = altVal.LikelihoodRelativeBValue '* tWRTNodeGlobalPriority
                            'Case SynthesisMode.smRelativeB
                            '    tUserValueWithoutControls.LikelihoodValue = altVal.NormalizedLikelihoodRelativeB
                            'End Select

                            'tUserValueWithoutControls.SumLikelihood = altVal.SumLikelihood
                            'tUserValueWithoutControls.MaxLikelihood = altVal.MaxLikelihood

                            tUserValueWithoutControls.SumImpact = altVal.SumImpact
                            tUserValueWithoutControls.MaxImpact = altVal.MaxImpact

                            tUserValueWithoutControls.ImpactValue = altVal.ImpactValue '* tWRTNodeGlobalPriority
                            tUserValueWithoutControls.RiskValue = altVal.RiskValue

                            'tUserValueWithoutControls.DollarValueImpact = altVal.DollarValueImpact
                            'tUserValueWithoutControls.DollarValueRisk = altVal.DollarValueRisk
                            '
                            ''A1313 ===
                            'If tNormalizedLkhdLocal <> LocalNormalizationType.ntUnnormalized Then 
                            '    tUserValueWithoutControls.DollarValueRisk = altVal.DollarValueRiskRelativeB
                            'End If
                            ''A1313 ==
                        End If
                    Next
                    'End If

                    'Normalization Local                    
                    'Select Case tNormalizedLkhdLocal
                    '    Case LocalNormalizationType.ntNormalizedForAll
                    '        tUserValue.LikelihoodValue = tUserValue.LikelihoodValue / tUserValue.SumLikelihood
                    '        'If ShowRiskReduction AndAlso UseControlsReductions Then
                    '        tUserValueWithoutControls.LikelihoodValue = tUserValueWithoutControls.LikelihoodValue / tUserValueWithoutControls.SumLikelihood
                    '        'End If
                    '    Case LocalNormalizationType.ntNormalizedMul100
                    '        tUserValue.LikelihoodValue = tUserValue.LikelihoodValue / tUserValue.MaxLikelihood
                    '        'If ShowRiskReduction AndAlso UseControlsReductions Then
                    '        tUserValueWithoutControls.LikelihoodValue = tUserValueWithoutControls.LikelihoodValue / tUserValueWithoutControls.MaxLikelihood
                    '        'End If
                    'End Select
                    'End If

                    'If tNormalizedImpcLocal <> LocalNormalizationType.ntUnnormalized Then
                    '    tUserValue.ImpactValue = tUserValue.ImpactValue / tUserValue.SumImpact
                    '    'If ShowRiskReduction AndAlso UseControlsReductions Then
                    '    tUserValueWithoutControls.ImpactValue = tUserValueWithoutControls.ImpactValue / tUserValueWithoutControls.SumImpact
                    '    'End If
                    'End If

                    'tUserValue.RiskValue = tUserValue.LikelihoodValue * tUserValue.ImpactValue
                    'If ShowRiskReduction AndAlso UseControlsReductions Then
                    'tUserValueWithoutControls.RiskValue = tUserValueWithoutControls.LikelihoodValue * tUserValueWithoutControls.ImpactValue
                    'End If

                    If tUserValue.RiskValue > tWorstValue Then tWorstValue = tUserValue.RiskValue

                    'tSimulationEventsData.Add (New clsEventData With {.ID = alt.NodeID, .Name = alt.NodeName, .Likelihood = tUserValue.LikelihoodValue, .Impact = tUserValue.ImpactValue, .Risk = tUserValue.RiskValue, .CostImpact = IMP, .CostRisk = R})
                    'if j = 0 Then
                    If alt.Enabled Then
                        'SimulationEventsDataForAllUsers(tUser.key).Add(New clsEventData With {.ID = alt.NodeID, .GuID = alt.NodeGuidID, .Name = alt.NodeName, .Priorities = New clsEventPriorities With {.Likelihood = tUserValueWithoutControls.LikelihoodValue, .Impact = tUserValueWithoutControls.ImpactValue, .Risk = tUserValueWithoutControls.RiskValue, .CostImpact = tUserValueWithoutControls.DollarValueImpact, .CostRisk = tUserValueWithoutControls.DollarValueRisk }, .PrioritiesWithControls = New clsEventPriorities With {.Likelihood = tUserValue.LikelihoodValue, .Impact = tUserValue.ImpactValue, .Risk = tUserValue.RiskValue, .CostImpact = tUserValue.DollarValueImpact, .CostRisk = tUserValue.DollarValueRisk}})
                        'SimulationEventsDataForAllUsers(tUser.key).Add(New clsEventData With {.ID = alt.NodeID, .GuID = alt.NodeGuidID, .Name = alt.NodeName})
                        SimulationEventsDataForAllUsers(tUser.Key).Add(alt.NodeID)
                    End If
                    'End If

                    If Not UseControlsReductions Then
                        tUserValue = tUserValueWithoutControls
                    End If

                    If tShowDollarValue Then
                        dr(COL_USERS_START + j * 4) = String.Format("{0}&{1}&{2}&{3}&{4}", Double2String(tUserValue.LikelihoodValue, 14), Double2String(tUserValue.ImpactValue * DollarValueOfEnterprise, 14), Double2String(tUserValue.RiskValue, 14), Double2String(tUserValue.RiskValue * DollarValueOfEnterprise, 14), Double2String(tUserValueWithoutControls.RiskValue * DollarValueOfEnterprise, 14))
                    Else
                        dr(COL_USERS_START + j * 4) = String.Format("{0}&{1}&{2}&{3}&{4}", Double2String(tUserValue.LikelihoodValue, 14), Double2String(tUserValue.ImpactValue, 14), Double2String(tUserValue.RiskValue, 14), Double2String(tUserValue.RiskValue, 14), Double2String(tUserValueWithoutControls.RiskValue, 14))
                    End If
                    dr(COL_USERS_START + j * 4 + 1) = tUserValue.LikelihoodValue
                    dr(COL_USERS_START + j * 4 + 2) = tUserValue.ImpactValue
                    dr(COL_USERS_START + j * 4 + 3) = tUserValue.RiskValue

                    alt.RiskValue = tUserValue.RiskValue 'todo

                    If Not BarsLRelativeTo1 AndAlso MaxPriorityL < tUserValue.LikelihoodValue Then MaxPriorityL = tUserValue.LikelihoodValue
                    If Not BarsIRelativeTo1 AndAlso MaxPriorityI < Math.Abs(tUserValue.ImpactValue) Then MaxPriorityI = Math.Abs(tUserValue.ImpactValue)
                    If Not BarsRRelativeTo1 AndAlso MaxPriorityR < Math.Abs(tUserValue.RiskValue) Then MaxPriorityR = Math.Abs(tUserValue.RiskValue)

                    If IsGroupByAttribute AndAlso Not GroupByAttributeGUID.Equals(Guid.Empty) AndAlso TotalRiskByCategory IsNot Nothing AndAlso TotalRiskReductionByCategory IsNot Nothing Then
                        If tShowDollarValue Then
                            TotalRiskByCategory(j) += tUserValue.RiskValue * DollarValueOfEnterprise
                            TotalRiskReductionByCategory(j) += (tUserValueWithoutControls.RiskValue - tUserValue.RiskValue) * DollarValueOfEnterprise
                        Else
                            TotalRiskByCategory(j) += tUserValue.RiskValue
                            TotalRiskReductionByCategory(j) += tUserValueWithoutControls.RiskValue - tUserValue.RiskValue
                        End If
                    End If

                    j += 1
                Next

                If IsWorstCaseVisible Then
                    dr(COL_WORST_CASE) = tWorstValue
                    'If Not IsShowBarsRelativeToOne AndAlso MaxPriorityOutOfAll < tWorstValue Then MaxPriorityOutOfAll = tWorstValue
                    If Not BarsRRelativeTo1 AndAlso MaxPriorityR < tWorstValue Then MaxPriorityR = tWorstValue
                    TotalMaxRisk += tWorstValue
                End If

                dt.Rows.Add(dr)
            Next

            If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then
                PM.CalculationsManager.UpdateAlternativeRankValues(PM.UserID)

                For i As Integer = 0 To VisibleAlternatives.Count - 1
                    Dim alt As clsNode = VisibleAlternatives(i)
                    CType(dt.Rows(i), DataRow)(COL_ID) = alt.iIndex
                Next
            End If

            If sr IsNot Nothing AndAlso IsGroupByAttribute AndAlso tShowGroupTotals Then
                If IsWorstCaseVisible AndAlso TotalMaxRiskByCategory >= 0 Then sr(COL_WORST_CASE) = TotalMaxRiskByCategory.ToString
                If TotalRiskByCategory IsNot Nothing Then
                    For t As Integer = 0 To TotalRiskByCategory.Count - 1
                        If Not PM.CalculationsManager.UseSimulatedValues Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                            If ShowTotalRisk Then
                                If tShowDollarValue Then
                                    sr(COL_USERS_START + t * 4) = CostString(TotalRiskByCategory(t), tDecimalDigits, True)
                                Else
                                    sr(COL_USERS_START + t * 4) = Double2String(TotalRiskByCategory(t) * 100, tDecimalDigits, True)
                                End If
                            End If
                            If ShowRiskReduction AndAlso UseControlsReductions Then
                                Dim sRiskReductionByCat As String = ""
                                If tShowDollarValue Then
                                    sRiskReductionByCat = CostString(TotalRiskReductionByCategory(t), CostDecimalDigits, False)
                                Else
                                    sRiskReductionByCat = Double2String(TotalRiskReductionByCategory(t) * 100, tDecimalDigits, False)
                                End If
                                sr(COL_USERS_START + t * 4) = CStr(sr(COL_USERS_START + t * 4)) + String.Format(ParseString(" (%%risk%% reduction: {0})"), sRiskReductionByCat)
                            End If
                        End If
                        'If tShowResultsMode = ShowResultsModes.rmSimulated OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                        'Dim userIndex As Integer = t
                        'Dim SimulatedRiskByCategory As Double = GetSimulatedRiskByCategory(CurType, userIndex)
                        ''Dim SimulatedRiskReductionByCategory As Double = 0
                        'If ShowTotalRisk Then
                        '    Dim s As String = ""
                        '    If Not IsDBNull(sr(COL_USERS_START + t * 4)) Then s = CStr(sr(COL_USERS_START + t * 4))
                        '    If tShowDollarValue Then
                        '        sr(COL_USERS_START + t * 4) = s + ParseString(" %%Loss%%: ") + CostString(SimulatedRiskByCategory, tDecimalDigits, True)
                        '    Else
                        '        sr(COL_USERS_START + t * 4) = s + ParseString(" %%Loss%%: ") + Double2String(SimulatedRiskByCategory * 100, tDecimalDigits, True)
                        '    End If
                        'End If
                        'If ShowRiskReduction AndAlso UseControlsReductions Then
                        '    Dim sRiskReductionByCat As String = ""
                        '    If tShowDollarValue 
                        '        sRiskReductionByCat = CostString(SimulatedRiskReductionByCategory, CostDecimalDigits, False)
                        '    Else
                        '        sRiskReductionByCat = Double2String(SimulatedRiskReductionByCategory * 100, tDecimalDigits, False)
                        '    End If
                        '    sr(COL_USERS_START + t * 4) = CStr(sr(COL_USERS_START + t * 4)) + String.Format(ParseString(" (%%loss%% reduction: {0})"), sRiskReductionByCat)
                        'End If
                        'End If
                    Next
                End If
            End If

        End If

        If Not BarsLRelativeTo1 AndAlso Math.Abs(MaxPriorityL) < 0.00000001 Then MaxPriorityL = 1
        If Not BarsIRelativeTo1 AndAlso Math.Abs(MaxPriorityI) < 0.00000001 Then MaxPriorityI = 1
        If Not BarsRRelativeTo1 AndAlso Math.Abs(MaxPriorityR) < 0.00000001 Then MaxPriorityR = 1

        If SimulationEventsDataForAllUsers.Count > 0 Then
            SimulationEventsData = SimulationEventsDataForAllUsers.Values.First ' write events data for simulations to session
        End If

        Dim dv As New DataView(dt)
        Return dv
    End Function

    Function CreateDataSourceByEvent() As DataView 'ICollection
        Dim dt As New DataTable()
        Dim dr As DataRow
        ColumnsUsersIDs.Clear()

        If RadTreeHierarchy.SelectedNode IsNot Nothing AndAlso RadTreeHierarchy.Nodes.Count > 0 Then
            Dim tDecimalDigits As Integer = PM.Parameters.DecimalDigits
            Dim SelectedNodeGuid As Guid = New Guid(RadTreeHierarchy.SelectedNode.Value)
            If Not SelectedNodeGuid.Equals(ALTERNATIVES_HEADER_ITEM_ID) Then
                dt.Columns.Add(New DataColumn(ResString("tblNo_"), GetType(Int32)))
                ColumnsUsersIDs.Add(Integer.MinValue, ResString("tblNo_"))
                dt.Columns.Add(New DataColumn("colGUID", GetType(String)))
                ColumnsUsersIDs.Add(Integer.MinValue + 1, "colGUID")
                dt.Columns.Add(New DataColumn(CStr(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ParseAllTemplates("%%Sources%% (%%Likelihood%% of %%Source%%)", App.ActiveUser, App.ActiveProject), ParseAllTemplates("%%Objectives(i)%% (%%Objective(i)%% priorities)", App.ActiveUser, App.ActiveProject))), GetType(String)))
                ColumnsUsersIDs.Add(Integer.MinValue + 2, "Title")
                Dim DataDict As New Dictionary(Of Integer, List(Of AlternativeRiskDataDataContract))

                MaxLikelihood = 0
                MaxImpact = 0

                If PM.CombinedGroups IsNot Nothing AndAlso PM.CombinedGroups.GroupsList IsNot Nothing Then
                    For Each tGroup As clsCombinedGroup In PM.CombinedGroups.GroupsList
                        If IsUserChecked(tGroup.CombinedUserID) Then
                            Dim tColumnName As String = GetNewColumnName(dt, ShortString(CStr(IIf(tGroup.Rule = "", tGroup.Name, tGroup.Rule)), 20, True), tGroup.CombinedUserID.ToString)
                            dt.Columns.Add(New DataColumn(tColumnName, GetType(String)))
                            DataDict.Add(tGroup.CombinedUserID, GetRiskDataWRTEvent(tGroup.CombinedUserID, "", HierarchyID, SelectedNodeGuid))
                            ColumnsUsersIDs.Add(tGroup.CombinedUserID, "")
                        End If
                    Next
                End If

                For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                    If IsUserChecked(tAppUser.UserID) Then
                        Dim tAHPUser As clsUser = PM.GetUserByEMail(tAppUser.UserEmail)
                        If tAHPUser IsNot Nothing Then tAppUser.UserName = tAHPUser.UserName
                        Dim tColumnName As String = GetNewColumnName(dt, ShortString(CStr(IIf(Not String.IsNullOrEmpty(tAppUser.UserName), tAppUser.UserName, tAppUser.UserEmail)), 20, True), tAHPUser.UserGuidID.ToString)
                        dt.Columns.Add(New DataColumn(tColumnName, GetType(String)))
                        DataDict.Add(tAppUser.UserID, GetRiskDataWRTEvent(tAHPUser.UserID, tAppUser.UserEmail, HierarchyID, SelectedNodeGuid))
                        ColumnsUsersIDs.Add(tAppUser.UserID, tAppUser.UserEmail)
                    End If
                Next

                ShowGridLegend = CanShowGridLegend AndAlso DataDict.Count > 0
                IsSingleUserSelected = DataDict.Count = 1
                Dim SelectedEvent = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(SelectedNodeGuid) 'A0778

                CalculateOverall(UseControlsReductions)
                Dim tCurrentFiltersList = CurrentFiltersList

                Dim i As Integer = 0
                For Each node As clsNode In PM.Hierarchy(HierarchyID).Nodes
                    If SelectedEvent IsNot Nothing AndAlso node.NodeID > 0 AndAlso node.IsTerminalNode AndAlso node.GetContributedAlternatives().Contains(SelectedEvent) AndAlso node.IsNodeIncludedInFilter(tCurrentFiltersList) Then
                        dr = dt.NewRow()
                        i += 1
                        dr(0) = i
                        dr(1) = node.NodeGuidID.ToString
                        dr(2) = String.Format("{0}    ({1})", node.NodeName, Double2String(node.UnnormalizedPriority * 100, tDecimalDigits, True))

                        Dim j As Integer = 3
                        For Each tUser In DataDict
                            Dim tUserValue As New AlternativeRiskDataDataContract With {.AlternativeID = node.NodeGuidID, .LikelihoodValue = 0, .ImpactValue = 0}
                            For Each altVal In tUser.Value
                                If altVal.AlternativeID.Equals(node.NodeGuidID) Then
                                    tUserValue.LikelihoodValue = altVal.LikelihoodValue * node.UnnormalizedPriority
                                    tUserValue.ImpactValue = altVal.ImpactValue * node.UnnormalizedPriority
                                End If
                            Next
                            dr(j) = String.Format("{0}", CStr(IIf(HierarchyID = ECHierarchyID.hidLikelihood, Double2String(tUserValue.LikelihoodValue, 14), Double2String(tUserValue.ImpactValue, 14))))
                            j += 1
                        Next

                        dt.Rows.Add(dr)
                    End If
                Next
            End If
        End If

        Dim dv As New DataView(dt)
        Return dv
    End Function

    Public Function GetLecUrl() As String
        Dim LecPgID As Integer = If(PagesWithControlsList.Contains(CurrentPageID), _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS, _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT)
        If CurrentPageID = _PGID_RISK_PLOT_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_CAUSES Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES
        If CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_OBJS Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS
        If CurrentPageID = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES
        If CurrentPageID = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS Then LecPgID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS        
        Return PageURL(LecPgID)
    End Function

    'Private Function GetSimulatedRiskByCategory(CurType As Integer, UserIndex As Integer) As Double
    '    Dim retVal As Double = 0

    '    'Dim tData = SimulationResults(DataDict.Keys(userIndex))

    '    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
    '        If Asc(CChar(alt.Tag)) = CurType Then
    '            'Dim tLikelihood As Double = LEC.EventSimulatedLikelihood(tData, alt.NodeID, LEC.NumberOfSimulations)
    '            Dim tLikelihood As Double = alt.SimulatedAltLikelihood
    '            'Dim tImpact As Double = LEC.EventSimulatedImpact(tData, alt.NodeID, LEC.NumberOfSimulations)
    '            Dim tImpact As Double = alt.SimulatedAltImpact
    '            Dim tRisk As Double = tLikelihood * tImpact
    '            retVal += tRisk
    '        End If
    '    Next

    '    Return retVal
    'End Function

    'Private Function GetSimulatedRiskReductionByCategory(CurType As Integer, UserIndex As Integer) As Double
    '    Dim retVal As Double = 0

    '    'Dim tData = SimulationResults(DataDict.Keys(userIndex))
    '    'Dim tDataWC = SimulationResultsWithoutControls(DataDict.Keys(userIndex))
    '    Dim tRisk As Double = 0
    '    Dim tRiskWC As Double = 0

    '    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
    '        If Asc(CChar(alt.Tag)) = CurType Then
    '            'Dim tLikelihood As Double = LEC.EventSimulatedLikelihood(tData, alt.NodeID, LEC.NumberOfSimulations)
    '            'Dim tImpact As Double = LEC.EventSimulatedImpact(tData, alt.NodeID, LEC.NumberOfSimulations)
    '            Dim tLikelihood As Double = alt.SimulatedAltLikelihood
    '            Dim tImpact As Double = alt.SimulatedAltImpact
    '            tRisk += tLikelihood * tImpact

    '            'Dim tLikelihoodWC As Double = LEC.EventSimulatedLikelihood(tDataWC, alt.NodeID, LEC.NumberOfSimulations)
    '            'Dim tImpactWC As Double = LEC.EventSimulatedImpact(tDataWC, alt.NodeID, LEC.NumberOfSimulations)
    '            Dim tLikelihoodWC As Double = alt.SimulatedAltLikelihood
    '            Dim tImpactWC As Double = alt.SimulatedAltImpact
    '            tRiskWC += tLikelihoodWC * tImpactWC
    '        End If
    '    Next

    '    retVal = tRiskWC - tRisk

    '    Return retVal
    'End Function

    Private Function AnySubObjectiveContributes(Node As clsNode, AltID As Integer) As Boolean
        If Node IsNot Nothing Then
            ' If likelihood and "Sources" is selected then cound all nodes
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

    Private Sub UpdateControl()
        If IsBowTiePage OrElse IsRiskMapPage Then Exit Sub
        BuildGridViewControl(SortExpression, SortDirection)
    End Sub

    Private Sub BuildGridViewControl(tSortExpression As String, tSortDirection As SortDirection)
        Dim DS As DataView
        If Not IsRiskResultsByEventPage AndAlso Not IsBowTiePage Then DS = CreateDataSource() Else DS = CreateDataSourceByEvent()

        If Not GridResults.Visible OrElse IsBowTiePage OrElse IsRiskMapPage Then Exit Sub ' D2188

        If Not String.IsNullOrEmpty(tSortExpression) Then
            Dim tColumnExists As Boolean = False
            ' D2385 ===
            Dim ColNames As New List(Of String)
            For Each clmn As DataColumn In DS.Table.Columns
                If clmn.ColumnName = tSortExpression Then tColumnExists = True
                If Not IsRiskResultsByEventPage Then
                    ColNames.Add(clmn.ColumnName)
                    clmn.ColumnName = clmn.ColumnName.Replace(",", "")
                End If
            Next
            ' D2385 ==
            If DS.Count > 0 Then 'A1120
                If tColumnExists Then
                    If Not IsRiskResultsByEventPage Then
                        DS.Sort = String.Format("{2}{0} {1}", tSortExpression.Replace(",", ""), IIf(tSortDirection = SortDirection.Ascending, "ASC", "DESC"), IIf(IsGroupByAttribute, "IS_SUMMARY ASC, ALT_TAG ASC, ", ""))
                    Else
                        DS.Sort = String.Format("{0} {1}", tSortExpression, IIf(tSortDirection = SortDirection.Ascending, "ASC", "DESC"))
                    End If
                Else
                    If Not IsRiskResultsByEventPage AndAlso IsGroupByAttribute Then
                        DS.Sort = String.Format("IS_SUMMARY ASC, ALT_TAG ASC, {0} ASC", ResString("tblNo_"))
                    End If
                End If
            End If
            If Not IsRiskResultsByEventPage Then
                ' D2385 ===
                For i As Integer = 0 To DS.Table.Columns.Count - 1
                    DS.Table.Columns(i).ColumnName = ColNames(i)
                Next
                ' D2385 ==
            End If
        End If

        'update sort order for Bow-Tie
        If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL Then
            Dim index As Integer = 0
            Dim altH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
            For Each row As DataRow In DS.Table.Select("", DS.Sort)
                If Not IsDBNull(row(COL_GUID)) Then
                    Dim eventGuid As Guid = New Guid(CStr(row(COL_GUID)))
                    Dim alt As clsNode = altH.GetNodeByID(eventGuid)
                    If alt IsNot Nothing Then
                        alt.RiskEventSortOrder = index
                        index += 1
                    End If
                End If
            Next
            PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
        End If

        If Not IsRiskResultsByEventPage Then
            TotalRisk.Clear()
            TotalRiskWithoutControls.Clear()
            For Each tUser In DS.Table.Columns
                TotalRisk.Add(0)
                TotalRiskWithoutControls.Add(0)
            Next
        End If

        GridResults.DataSource = DS
        GridResults.DataBind()
    End Sub

    Public Function GetRiskDataWRTEvent(UserID As Integer, UserEmail As String, tHierarchyID As ECHierarchyID, EventID As Guid, Optional UseControls As Boolean = False) As List(Of AlternativeRiskDataDataContract)
        If App IsNot Nothing AndAlso App.ActiveProject IsNot Nothing Then
            Dim res As New List(Of AlternativeRiskDataDataContract)

            With PM
                Dim oldHierarchyID As ECHierarchyID = tHierarchyID
                .ActiveHierarchy = tHierarchyID

                .StorageManager.Reader.LoadUserData()
                'TODO: AC - calculate Causes or Objectives priorities WRT the Event
                Dim CalcTarget As clsCalculationTarget = Nothing

                If IsCombinedUserID(UserID) Then
                    Dim CG As clsCombinedGroup = .CombinedGroups.GetCombinedGroupByUserID(UserID)
                    If CG IsNot Nothing Then
                        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG)
                    End If
                Else
                    Dim user As clsUser = .GetUserByEMail(UserEmail)
                    If user IsNot Nothing Then
                        CalcTarget = New clsCalculationTarget(CalculationTargetType.cttUser, user)
                        '.StorageManager.Reader.LoadUserData(user)
                    End If
                End If

                PM.CalculationsManager.GetRiskDataWRTNode(UserID, UserEmail, .Hierarchy(HierarchyID).Nodes(0).NodeGuidID) 'A1064

                Dim oldUseReduction = .CalculationsManager.UseReductions
                .CalculationsManager.UseReductions = UseControlsReductions

                For Each node In .Hierarchy(tHierarchyID).Nodes
                    If node.NodeID > 0 Then 'Skip Goal node
                        .CalculationsManager.Calculate(CalcTarget, node, tHierarchyID)
                        Dim tEventData As AlternativeRiskDataDataContract = New AlternativeRiskDataDataContract
                        tEventData.AlternativeID = node.NodeGuidID

                        For Each alt As clsNode In OrderedEvents
                            If alt.NodeGuidID.Equals(EventID) Then
                                If tHierarchyID = ECHierarchyID.hidLikelihood Then
                                    tEventData.LikelihoodValue = alt.UnnormalizedPriority
                                Else
                                    tEventData.ImpactValue = alt.UnnormalizedPriority
                                End If
                            End If
                        Next
                        res.Add(tEventData)
                        If tHierarchyID = ECHierarchyID.hidLikelihood Then
                            If MaxLikelihood < tEventData.LikelihoodValue Then MaxLikelihood = tEventData.LikelihoodValue
                        Else
                            If MaxImpact < tEventData.ImpactValue Then MaxImpact = tEventData.ImpactValue
                        End If
                    End If
                Next
                .ActiveHierarchy = oldHierarchyID
                .CalculationsManager.UseReductions = oldUseReduction
            End With

            Return res
        End If
        Return Nothing
    End Function

    Public ReadOnly Property GetTotalRiskText(riskValue As Double) As String
        Get
            Dim retVal As String = ""

            If Not PM.CalculationsManager.UseSimulatedValues Then
                retVal = ParseString("Total %%Risk%%")
            Else
                retVal = ParseString("Total %%Risk%% (Average %%Loss%%)")
            End If

            'If PRJ.RiskionProjectType = ProjectType.ptMixed OrElse PRJ.RiskionProjectType = ProjectType.ptOpportunities Then
            '    If riskValue < 0 Then
            '        retVal = ParseString("Net %%Risk%%")
            '    Else
            '        retVal = ParseString("Net Opportunity")
            '    End If
            'End If

            ' D6798 ===
            Select Case PRJ.RiskionProjectType
                Case ProjectType.ptMixed
                    retVal = ParseString("Net %%Opportunity%%/%%Risk%%")
                Case ProjectType.ptMyRiskReward
                    retVal = ParseString("Net %%Opportunity%%/%%Risk%%")    ' ?
                Case ProjectType.ptOpportunities
                    retVal = ParseString("Net %%Opportunity%%")
            End Select
            ' D6798 ==

            Return retVal
        End Get
    End Property

    Public Function IsCombinedUserID(UserID As Integer) As Boolean
        Return (UserID <= COMBINED_GROUPS_USERS_START_ID) AndAlso (UserID > Integer.MinValue) OrElse (UserID = COMBINED_USER_ID)
    End Function

    Private IsSingleUserSelected As Boolean = True

    ' D2534 ===
    Private Function CreateSortLink(sColName As String, sName As String, Idx As Integer, Optional Tooltip As String = "", Optional TooltipClass As String = "") As LinkButton 'A0927
        Dim btn As New LinkButton
        btn.ID = "Sort" + Idx.ToString
        btn.Text = sName ' If(IsWidget, "", sName)  ' D6666
        btn.Width = BAR_W_SMALL
        'A0927 ===
        If Not String.IsNullOrEmpty(Tooltip) Then
            btn.Attributes.Add("class", TooltipClass)
        End If 'A0927 ==
        btn.CommandName = "Sort"
        btn.CommandArgument = sColName + Idx.ToString
        If btn.CommandArgument = SortExpression Then btn.Text += CStr(IIf(SortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC))

        btn.Attributes.Add("style", "margin:2px 2px 0px 2px;")
        btn.Attributes.Add("onclick", String.Format("showLoadingPanel(); __doPostBack('{0}', '{1}${2}'); return false;", GridResults.UniqueID, btn.CommandName, btn.CommandArgument))
        Return btn
    End Function

    Private Function CreateStaticText(Name As String, Optional Width As Integer = 0) As HtmlGenericControl
        Dim lbl As New HtmlGenericControl
        lbl.InnerHtml = Name
        If Width > 0 Then
            lbl.Attributes.Add("style", "display: inline-block; width: " + Width.ToString + "px;")
        End If
        Return lbl
    End Function
    ' D2534 ==

    ' D2533 ===
    Protected Sub GridResults_PreRender(sender As Object, e As EventArgs) Handles GridResults.PreRender
        'GridResults.Style.Add("margin", "3px 6px 6px 6px")
        If GridResults.HeaderRow IsNot Nothing AndAlso GridResults.HeaderRow.Cells.Count > 2 Then
            Dim lblRisk As String = ResString("lblRisk")
            Dim HasOppEvent As Boolean = PM.ActiveAlternatives.Nodes.Where(Function(alt) alt.EventType = EventType.Opportunity).Count > 0
            Dim HasRiskEvent As Boolean = PM.ActiveAlternatives.Nodes.Where(Function(alt) alt.EventType = EventType.Risk).Count > 0
            If HasOppEvent Then
                If HasRiskEvent Then
                    lblRisk = ParseString("%%Risk%%/%%Opportunity%%")
                Else
                    lblRisk = ParseString("%%Opportunity%%")
                End If
            End If
            For i As Integer = 0 To GridResults.HeaderRow.Cells.Count - 1
                Dim tCol As DataControlFieldHeaderCell = CType(GridResults.HeaderRow.Cells(i), DataControlFieldHeaderCell)
                If tCol.Controls IsNot Nothing AndAlso tCol.Controls.Count > 0 Then
                    Dim lbl As LinkButton = CType(tCol.Controls(0), LinkButton)
                    lbl.OnClientClick = "showLoadingPanel();"
                    If Not IsRiskResultsByEventPage Then
                        If i = COL_ID AndAlso PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then
                            lbl.Text = ResString(If(IsWidget, "tblRiskResultsRank", "optRank"))
                        End If
                        If i < COL_USERS_START AndAlso lbl.CommandArgument = SortExpression Then
                            lbl.Text += CStr(IIf(SortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC))
                        End If
                        If i >= COL_USERS_START AndAlso (i - COL_USERS_START) Mod 4 = 0 Then
                            Dim idx As Integer = CInt((i - COL_USERS_START) / 4)
                            Dim sExportUserName As String = ""

                            If Not IsExport Then
                                tCol.Controls.Add(CreateStaticText(String.Format("{0}<br/>", UsersList(idx))))
                            Else
                                sExportUserName = String.Format(" &ndash; {0}", UsersList(idx))
                            End If

                            Dim TopW As Integer = BAR_W_SMALL
                            'If tShowResultsMode = ShowResultsModes.rmBoth Then TopW = BAR_W_SMALL * 2 + 10

                            Dim sImpactHeader As String = ResString("lblImpact") + CStr(IIf(ShowDollarValue, ", " + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, ""))
                            Dim sRiskHeader As String = lblRisk + CStr(IIf(ShowDollarValue, ", " + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, ""))

                            ' D6666 ===
                            'If IsWidget Then
                            tCol.Controls.Add(CreateSortLink("L", String.Format("{0}{1}", ParseString("%%Likelihood%%"), sExportUserName), idx + 1, ResString("hintSortByLikelihood"), "sort_link_likelihood"))
                            tCol.Controls.Add(CreateSortLink("I", String.Format("{0}{1}", sImpactHeader, sExportUserName), idx + 1, ResString("hintSortByImpact"), "sort_link_impact"))
                            tCol.Controls.Add(CreateSortLink("R", String.Format("{0}{1}", sRiskHeader, sExportUserName), idx + 1, ResString("hintSortByRisk"), "sort_link_risk"))
                            'Else
                            '    tCol.Controls.Add(CreateStaticText(String.Format("{0}{1}", ParseString("%%Likelihood%%"), sExportUserName), TopW))
                            '    tCol.Controls.Add(CreateStaticText(String.Format("{0}{1}", sImpactHeader, sExportUserName), TopW))
                            '    tCol.Controls.Add(CreateStaticText(String.Format("{0}{1}", sRiskHeader, sExportUserName), TopW))
                            'End If
                            'tCol.Controls.Add(CreateStaticText("<br/>"))

                            'If Not IsWidget Then
                            '    ' D6666 ==
                            '    If tShowResultsMode = ShowResultsModes.rmComputed Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then                                
                            '        Dim btn As LinkButton = CreateSortLink("L", "Computed", idx + 1, ResString("hintSortByLikelihood"), "sort_link_likelihood")
                            '        tCol.Controls.Add(btn)
                            '    End If
                            '    If tShowResultsMode = ShowResultsModes.rmSimulated Then
                            '        Dim btn As LinkButton = CreateSortLink("L", "Simulated", idx + 1, ResString("hintSortByLikelihood"), "sort_link_likelihood")
                            '        tCol.Controls.Add(btn)
                            '    End If
                            '    'If tShowResultsMode = ShowResultsModes.rmBoth Then                                
                            '    '    tCol.Controls.Add(CreateStaticText("Simulated", BAR_W_SMALL))
                            '    'End If

                            '    If tShowResultsMode = ShowResultsModes.rmComputed Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                            '        Dim btn As LinkButton = CreateSortLink("I", "Computed", idx + 1, ResString("hintSortByImpact"), "sort_link_impact")
                            '        tCol.Controls.Add(btn) ' D4181
                            '    End If
                            '    If tShowResultsMode = ShowResultsModes.rmSimulated Then
                            '        Dim btn As LinkButton = CreateSortLink("I", "Simulated", idx + 1, ResString("hintSortByImpact"), "sort_link_impact")
                            '        tCol.Controls.Add(btn)
                            '    End If
                            '    'If tShowResultsMode = ShowResultsModes.rmBoth Then                                
                            '    '    tCol.Controls.Add(CreateStaticText("Simulated", BAR_W_SMALL))
                            '    'End If

                            '    If tShowResultsMode = ShowResultsModes.rmComputed Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                            '        Dim btn As LinkButton = CreateSortLink("R", "Computed", idx + 1, ResString("hintSortByRisk"), "sort_link_risk")
                            '        tCol.Controls.Add(btn) ' D4181
                            '    End If
                            '    If tShowResultsMode = ShowResultsModes.rmSimulated Then
                            '        Dim btn As LinkButton = CreateSortLink("R", "Simulated", idx + 1, ResString("hintSortByRisk"), "sort_link_risk")
                            '        tCol.Controls.Add(btn)
                            '    End If
                            '    'If tShowResultsMode = ShowResultsModes.rmBoth Then                                
                            '    '    tCol.Controls.Add(CreateStaticText("Simulated", BAR_W_SMALL))
                            '    'End If
                            'End If

                            lbl.Visible = False
                        End If
                    Else
                        If lbl IsNot Nothing AndAlso lbl.CommandArgument = SortExpression Then
                            lbl.Text += CStr(IIf(SortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC))
                        End If
                    End If
                End If
            Next
        End If
    End Sub
    ' D2533 ==

    Private tSelectedCategoryMultiValues As List(Of String)
    Private sGridColorLegend As String

    Protected Sub GridResults_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles GridResults.RowDataBound
        If e.Row.DataItem IsNot Nothing AndAlso e.Row.RowType = DataControlRowType.Header AndAlso e.Row.Cells.Count > COL_ID Then
            e.Row.Cells(COL_ID).Text = ResString("tblRiskResultsRank")
        End If

        Dim tDecimalDigits As Integer = PM.Parameters.DecimalDigits

        If e.Row.RowType = DataControlRowType.Header Then
            e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Left
            e.Row.Cells(COL_NAME).Style.Add("padding-left", "5px")
            tSelectedCategoryMultiValues = New List(Of String)
            sGridColorLegend = ""
        End If

        If Not IsRiskResultsByEventPage Then
            ' D2534, A0818  ===
            Dim cellsCount As Integer = e.Row.Cells.Count
            For i As Integer = 0 To UsersList.Count - 1
                Dim CellIndex As Integer = COL_USERS_START + i * 4
                If CellIndex + 1 < cellsCount Then e.Row.Cells(CellIndex + 1).Visible = False
                If CellIndex + 2 < cellsCount Then e.Row.Cells(CellIndex + 2).Visible = False
                If CellIndex + 3 < cellsCount Then e.Row.Cells(CellIndex + 3).Visible = False
            Next

            If e.Row.Cells.Count > COL_GUID Then e.Row.Cells(COL_GUID).Visible = False
            If e.Row.Cells.Count > COL_TAG Then e.Row.Cells(COL_TAG).Visible = False
            If e.Row.Cells.Count > COL_IS_SUMMARY Then e.Row.Cells(COL_IS_SUMMARY).Visible = False
            If e.Row.Cells.Count > COL_ID Then e.Row.Cells(COL_ID).Visible = PM.Parameters.NodeIndexIsVisible
            If Not IsWorstCaseVisible AndAlso e.Row.Cells.Count > COL_WORST_CASE Then e.Row.Cells(COL_WORST_CASE).Visible = False

            Dim tShowDollarValue As Boolean = ShowDollarValue
            'Dim tDollarValueOfEnterprise As Double = DollarValueOfEnterprise

            ' D2534, A0818 ==
            If e.Row.DataItem IsNot Nothing AndAlso e.Row.RowType <> DataControlRowType.Footer AndAlso e.Row.RowType <> DataControlRowType.Header Then
                Dim tRow As DataRowView = CType(e.Row.DataItem, DataRowView)
                Dim tColor As String = ""
                'color rows by category
                'A1211 ===
                Dim tSelectedCategoryID As Guid = SelectedCategoryID
                Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
                If tSelectedCategoryID.Equals(Guid.Empty) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = AttributesList(0).ID

                Dim tSelectedAttr As clsAttribute = Nothing
                For Each attr In AttributesList
                    If attr.ID = tSelectedCategoryID Then tSelectedAttr = attr
                Next

                Dim AltID As Integer = -1
                Dim eventGuid As Guid = Guid.Empty
                Dim eAlt As clsNode = Nothing
                If Not IsDBNull(tRow.Item(COL_GUID)) Then
                    eventGuid = New Guid(CStr(tRow.Item(COL_GUID)))
                    eAlt = PM.ActiveAlternatives.GetNodeByID(eventGuid)
                    AltID = eAlt.NodeID

                    e.Row.Cells(COL_ID).Text = eAlt.Index
                    If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank AndAlso IsWidget Then
                        Dim NodeRank As Integer = eAlt.iIndex
                        'e.Row.Cells(COL_ID).Text = String.Format("{0}<sup>{3}{1}{2}{4}</sup> ({5})", NodeRank, If(NodeRank < eAlt.UserRank, "<i class='fas fa-long-arrow-alt-up'></i>", "<i class='fas fa-long-arrow-alt-down'></i>"), Math.Abs(NodeRank - eAlt.UserRank), If(NodeRank < eAlt.UserRank, "<span style='color:green;'>", "<span style='color:red;'>"), "</span>", eAlt.UserRank)
                        'e.Row.Cells(COL_ID).Text = String.Format("{0}&nbsp;{1}", NodeRank, If(NodeRank < eAlt.UserRank, "<i class='fas fa-long-arrow-alt-up online_marker'></i>", "<i class='fas fa-long-arrow-alt-down error'></i>"))
                        'e.Row.Cells(COL_ID).Text = String.Format("{0}&nbsp;<i class='{1}' style='cursor: default;' title='{2}'></i>", NodeRank, If(NodeRank < eAlt.UserRank, "fas fa-long-arrow-alt-up online_marker", "fas fa-long-arrow-alt-down error"), If(NodeRank < eAlt.UserRank, "+", "-") + Math.Abs(NodeRank - eAlt.UserRank).ToString)

                        Dim sCompRank As String = NodeRank.ToString
                        Dim sUserRank As String = If(eAlt.UserRank > 0, eAlt.UserRank.ToString, "")
                        Dim sRankDelta As String = If(eAlt.UserRank > 0, String.Format("<i class='{1}' style='cursor: default;'></i>{0}", If(NodeRank <> eAlt.UserRank, Math.Abs(NodeRank - eAlt.UserRank).ToString, ""), If(NodeRank < eAlt.UserRank, "fas fa-long-arrow-alt-up online_marker", If(NodeRank > eAlt.UserRank, "fas fa-long-arrow-alt-down error", "fas fa-equals ec-icon"))), "")
                        e.Row.Cells(COL_ID).Text = String.Format("<nobr><span style='display:inline-block;width:40px;text-align:right;'>{0}</span>&nbsp;&nbsp;<span style='display:inline-block;width:40px;text-align:right;'>{1}</span>&nbsp;&nbsp;<span style='display:inline-block;width:40px;text-align:right; padding-right:1ex;'>{2}</span></nobr>", sCompRank, sUserRank, sRankDelta)

                    End If

                    If eAlt.Enabled Then e.Row.Attributes.Remove("style") Else e.Row.Attributes("style") = "opacity: 0.4"
                End If

                If tSelectedAttr IsNot Nothing AndAlso Not tSelectedAttr.EnumID.Equals(Guid.Empty) Then tSelectedCategoryEnum = PM.Attributes.GetEnumByID(tSelectedAttr.EnumID)
                If ColorCodingByCategory AndAlso tSelectedAttr IsNot Nothing AndAlso AttributesList.Count > 0 AndAlso Not IsDBNull(tRow.Item(COL_ID)) Then
                    tColor = AlternativeUniformColor
                    Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, eAlt.NodeGuidID)
                    If attrVal IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                        Select Case tSelectedAttr.ValueType
                            Case AttributeValueTypes.avtEnumeration
                                Dim tLegendItemAdded As Boolean = False
                                Dim sVal = CType(attrVal, Guid).ToString
                                If Not tSelectedCategoryMultiValues.Contains(sVal) Then
                                    tSelectedCategoryMultiValues.Add(sVal)
                                    tLegendItemAdded = True
                                End If
                                Dim item As clsAttributeEnumerationItem = tSelectedCategoryEnum.GetItemByID(CType(attrVal, Guid))
                                If item IsNot Nothing Then
                                    Dim index As Integer = tSelectedCategoryEnum.Items.IndexOf(item)
                                    If index >= 0 Then
                                        tColor = GetPaletteColor(CurrentPaletteID(PM), index)
                                    End If
                                    If tLegendItemAdded Then
                                        sGridColorLegend += CStr(IIf(sGridColorLegend = "", "", ", ")) + String.Format("<span style='color:{0};'>[{1}]</span>", tColor, SafeFormString(item.Value))
                                    End If
                                End If
                            Case AttributeValueTypes.avtEnumerationMulti
                                Dim sVal As String = CStr(attrVal)
                                If Not String.IsNullOrEmpty(sVal) Then
                                    Dim tLegendItemAdded As Boolean = False
                                    If Not tSelectedCategoryMultiValues.Contains(sVal) Then
                                        tSelectedCategoryMultiValues.Add(sVal)
                                        tLegendItemAdded = True
                                    End If
                                    Dim index As Integer = tSelectedCategoryMultiValues.IndexOf(sVal)
                                    tColor = GetPaletteColor(CurrentPaletteID(PM), index)
                                    If tLegendItemAdded Then
                                        sGridColorLegend += CStr(IIf(sGridColorLegend = "", "", ", ")) + String.Format("<span style='color:{0};'>[{1}]</span>", tColor, SafeFormString(PM.Attributes.GetAttributeValueString(tSelectedCategoryID, eAlt.NodeGuidID)))
                                    End If
                                End If
                        End Select
                    End If
                    If eAlt.Enabled Then
                        e.Row.Attributes("style") = "color:" + tColor
                    Else
                        e.Row.Attributes("style") = "opacity: 0.4; color:" + tColor
                        'e.Row.Attributes("style") = "opacity: 0.4"
                    End If
                    'If e.Row.Cells.Count > COL_USERS_START - 1 Then 
                    '    For i As Integer = 0 To COL_USERS_START - 1
                    '        e.Row.Cells(i).Attributes("style") = "color:" + tColor
                    '    Next
                    'End If
                End If
                'A1211 ==

                If e.Row.Cells.Count >= COL_NAME Then
                    Dim tEventRecord As String = CStr(tRow.Item(COL_NAME))
                    If eAlt IsNot Nothing Then
                        If Not IsExport AndAlso Not App.Options.isSingleModeEvaluation AndAlso Not IsWidget Then    ' D6048
                            '-A1201 e.Row.Cells(COL_NAME).Text = String.Format("<a href="""" onclick=""return OnEventName_Click('{1}',{2})"" class=""actions"">{0}</a>", SafeFormString(tStrValues(0)), tStrValues(1), IIf(HasListPage(PagesWithControlsList, CurrentPageID), 1, 0)) ' D3365
                            Dim sTitle As String = JS_SafeString(tEventRecord)
                            'Dim eventGuid As Guid = New Guid(CStr(tRow.Item(COL_GUID)))
                            'Dim eAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(eventGuid)
                            'Dim sDescription As String = SafeFormString(PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(New Guid(sEventID)).Comment)
                            Dim sDescription As String = ""
                            If ShowEventDescriptions Then   ' D4155
                                sDescription = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(eventGuid).InfoDoc   ' D4154
                                sDescription = Infodoc2Text(App.ActiveProject, sDescription, True)        ' D4154 + D4155
                            End If
                            'Dim sContent As String = String.Format("<a href="""" onclick=""return OnEventName_Click('{1}',{2})"" class=""actions"" {3}>{0}</a>", sTitle, sEventID, IIf(HasListPage(PagesWithControlsList, CurrentPageID), 1, 0), IIf(tColor = "", "", "style=""color:" + tColor + """")) ' D3365 + A1201 + A1211
                            'e.Row.Cells(COL_NAME).Text = "<table id='lblName" + eAlt.NodeID.ToString + "' cellpadding='0' cellspacing='0' class='text' style='width:100%;'><tr><td align='left' style='padding-left:3px;' title='" + sTitle + "'>" + sContent + "</td><td align='right' rowspan='2'><img align='right' style='vertical-align:top;cursor:pointer;' src='" + ImagePath + "menu_dots.png' alt='' onclick='showEventMenu(""" + eAlt.NodeID.ToString + """, """ + sTitle + """);' /></td></tr><tr><td align='left' style='padding-top:4px; padding-left:3px;'><small title='" + sDescription + "'>" + sDescription + "</small></td></tr></table>"
                            'e.Row.Cells(COL_NAME).Text += "<input type='text' id='tbName" + eAlt.NodeID.ToString + "' style='width:99%; display:none;' value='" + eAlt.NodeName + "' onblur='switchAltNameEditor(" + eAlt.NodeID.ToString + ",0);'>"
                            Dim sContent As String = String.Format("<a href='' id='lblName{4}' onclick=""return OnEventName_Click('{1}',{2})"" class='actions' style=""padding-left:3px;{3}"" title='{5}'>{0}</a>", SafeFormString(tEventRecord), eventGuid.ToString, IIf(HasListPage(PagesWithControlsList, CurrentPageID), 1, 0), IIf(tColor = "", "", "color:" + tColor + ";"), eAlt.NodeID, sTitle) ' D3365 + A1201 + A1211 + A1262
                            e.Row.Cells(COL_NAME).Text = sContent
                            If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then  ' D6798
                                e.Row.Cells(COL_NAME).Text += "<select align='right' style='float:right;vertical-align:top; width: 80px;' onchange='sendCommand(""action=risk_event_type&value=""+this.value+""&id=" + eAlt.NodeID.ToString + """);'><option value='0' " + If(eAlt.EventType = EventType.Risk, "selected='selected'", "") + ">Risk</option><option value='1' " + If(eAlt.EventType = EventType.Opportunity, "selected='selected'", "") + ">" + ParseString("%%Opportunity%%") + "</option></select>"
                            End If
                            e.Row.Cells(COL_NAME).Text += "<i align='right' style='float:right;vertical-align:top;cursor:pointer;' class='fas fa-ellipsis-h' onclick='showEventMenu(""" + eAlt.NodeID.ToString + """, """ + SafeFormString(JS_SafeString(tEventRecord)) + """);'></i>"
                            e.Row.Cells(COL_NAME).Text += "<input type='text' id='tbName" + eAlt.NodeID.ToString + "' style='width:99%; display:none;' value='" + eAlt.NodeName + "' onblur='switchAltNameEditor(" + eAlt.NodeID.ToString + ",0);'>"
                            e.Row.Cells(COL_NAME).Text += CStr(IIf(sDescription <> "", "<br/><small style='padding-top:4px; padding-left:3px;' title='" + sDescription + "'>" + sDescription + "</small>", "")) ' D4155

                            Dim groupName As String = PM.EventsGroups.GroupName(eAlt)
                            Dim eventPrecedence As Integer = PM.EventsGroups.GroupPrecedence(eAlt)
                            If Not String.IsNullOrEmpty(groupName) Then
                                groupName = String.Format("Group Name: {0}", groupName)
                                If eventPrecedence <> Integer.MaxValue Then
                                    groupName += String.Format(", Precedence: {0}", eventPrecedence)
                                End If
                                e.Row.Cells(COL_NAME).Text += "<br/><small style='padding-top:4px; padding-left:3px; color:#1fad93; text-align:right;' title='" + ParseString("%%Event%% Group") + "'>" + groupName + "</small>"
                            End If
                        End If
                    Else
                        e.Row.Cells(COL_NAME).Text = tEventRecord
                    End If
                    e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Left
                    e.Row.Cells(COL_NAME).Text = String.Format("<div style='min-width:250px; padding:3px 0px;'>{0}</div>", e.Row.Cells(COL_NAME).Text)
                    'e.Row.Attributes.Remove("onmouseover")
                    'e.Row.Attributes.Add("onmouseover", String.Format("RowHover(this,1,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                    'e.Row.Attributes.Remove("onmouseout")
                    'e.Row.Attributes.Add("onmouseout", String.Format("RowHover(this,0,{0});", IIf(e.Row.RowState = DataControlRowState.Alternate, 1, 0)))
                    'e.Row.CssClass = String.Format("text grid_row{0}{1}", IIf(e.Row.RowIndex Mod 2 = 0, "", "_alt"), "")
                End If

                If Not tRow.Item(COL_IS_SUMMARY).ToString.EndsWith("summary") Then
                    If IsWorstCaseVisible Then
                        Dim tWorstValue As Double = CDbl(tRow.Item(COL_WORST_CASE))
                        Dim tBarTextColor As String = "#000000"
                        e.Row.Cells(COL_WORST_CASE).Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:134px'><tr valign=middle><td>{0}</td></tr></table>", HTMLCreateGraphBarWithValues(CSng(IIf(BarsRRelativeTo1, tWorstValue * 100, (tWorstValue / MaxPriorityR) * 100)), 100, BAR_W_SMALL, BAR_H_SMALL, "#ffa792", tBarTextColor, ImagePath, Double2String(tWorstValue * 100, tDecimalDigits, True), True)) ' D2417 #E68E79    #FFA500 (orange) + A0927
                    End If

                    For i As Integer = COL_USERS_START To e.Row.Cells.Count - 1
                        'For i As Integer = 2 To tRow.Row.ItemArray.Count - 1
                        If TypeOf (tRow.Item(i)) Is String Then
                            Dim tDataString As String = CStr(tRow.Item(i))
                            Dim tValues As String() = tDataString.Split(CChar("&"))
                            Dim tLVal As Double = 0
                            Dim tIVal As Double = 0
                            Dim tRVal As Double = 0
                            If tValues.Count > 0 Then
                                tLVal = CDbl(tRow.Item(i + 1)) * 100
                                tIVal = CDbl(tRow.Item(i + 2)) * 100
                                Dim Risk = CDbl(tRow.Item(i + 3))
                                tRVal = Risk * 100 'CDbl(tValues(0))

                                Dim RiskGraphHint As String = tValues(3)
                                If Not tShowDollarValue Then
                                    RiskGraphHint = Double2String(CDbl(tValues(3)) * 100, PM.Parameters.DecimalDigits, True)
                                Else
                                    RiskGraphHint = CostString(CDbl(tValues(3)))
                                End If

                                Dim ImpactDollarValue As Double = CDbl(tValues(1))
                                Dim RiskDollarValue As Double = CDbl(tValues(3))
                                Dim RiskWOControls As Double = CDbl(tValues(4))

                                Dim tLikelihood As String = ""
                                Dim tImpact As String = ""
                                Dim tRisk As String = ""

                                Dim tBarTextColor As String = "#000000"

                                'If tShowResultsMode = ShowResultsModes.rmComputed Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                                tLikelihood = HTMLCreateGraphBarWithValues(CSng(IIf(BarsLRelativeTo1, tLVal, tLVal / MaxPriorityL)), 100, BAR_W_SMALL, BAR_H_SMALL, "#bdeb34", tBarTextColor, ImagePath, Double2String(tLVal, tDecimalDigits, True), True) ' D2417
                                tImpact = HTMLCreateGraphBarWithValues(CSng(IIf(BarsIRelativeTo1, tIVal, tIVal / MaxPriorityI)), 100, BAR_W_SMALL, BAR_H_SMALL, "#abccff", tBarTextColor, ImagePath, CStr(IIf(tShowDollarValue, CostString(ImpactDollarValue, CostDecimalDigits), Double2String(tIVal, tDecimalDigits, True))), True) ' D2417
                                tRisk = HTMLCreateGraphBarWithValues(CSng(IIf(BarsRRelativeTo1, tRVal, tRVal / MaxPriorityR)), 100, BAR_W_SMALL, BAR_H_SMALL, If((PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) AndAlso tRVal > 0, "#f4ab5e", "#ffa792"), tBarTextColor, ImagePath, CStr(IIf(tShowDollarValue, CostString(RiskDollarValue, CostDecimalDigits), RiskGraphHint)), True) 'A0927 + A1112 + D6798
                                'End If

                                'Dim tRiskSimulated As String = ""
                                'Dim tLikelihoodSimulated As String = ""
                                'Dim tImpactSimulated As String = ""

                                'If tShowResultsMode = ShowResultsModes.rmSimulated Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                                '    Dim userIndex As Integer = CInt((i - COL_USERS_START) / 4)
                                '    Dim cVal As Double = 0
                                '    Dim iVal As Double = 0
                                '    If DataDict.Count > userIndex Then
                                '        'Dim tData = SimulationResults(DataDict.Keys(userIndex))
                                '        'cVal = LEC.EventSimulatedLikelihood(tData, AltID, LEC.NumberOfSimulations) * 100
                                '        cVal = eAlt.SimulatedAltLikelihood * 100
                                '        tLikelihoodSimulated = HTMLCreateGraphBarWithValues(CSng(IIf(BarsLRelativeTo1, cVal, cVal / MaxPriorityL)), 100, BAR_W_SMALL, BAR_H_SMALL, "#bdeb34", tBarTextColor, ImagePath, Double2String(cVal, tDecimalDigits, True), True)

                                '        'iVal = LEC.EventSimulatedImpact(tData, AltID, LEC.NumberOfSimulations) * 100
                                '        iVal = eAlt.SimulatedAltImpact * 100
                                '        Dim iValDoll = iVal * DollarValueOfEnterprise / 100
                                '        tImpactSimulated = HTMLCreateGraphBarWithValues(CSng(IIf(BarsLRelativeTo1, iVal, iVal / MaxPriorityI)), 100, BAR_W_SMALL, BAR_H_SMALL, "#abccff", tBarTextColor, ImagePath, CStr(IIf(tShowDollarValue, CostString(iValDoll, CostDecimalDigits), Double2String(iVal, tDecimalDigits, True))), True)

                                '        'Simulated Risk                                    
                                '        Dim tSimRiskVal As Double = iVal * cVal / 100
                                '        Dim tSimRiskValDoll As Double = 0
                                '        if tShowDollarValue Then
                                '            tSimRiskValDoll = tSimRiskVal * DollarValueOfEnterprise / 100
                                '        End If
                                '        tRiskSimulated = HTMLCreateGraphBarWithValues(CSng(IIf(BarsRRelativeTo1, tSimRiskVal, tSimRiskVal / MaxPriorityR)), 100, BAR_W_SMALL, BAR_H_SMALL, If(PM.PipeParameters.ProjectType = ProjectType.ptMixed AndAlso tSimRiskVal > 0, "#f4ab5e", "#ffa792"), tBarTextColor, ImagePath, CStr(IIf(tShowDollarValue, CostString(tSimRiskValDoll, CostDecimalDigits), Double2String(tSimRiskVal, tDecimalDigits, True))), True)

                                '        ' for sorting
                                '        'tRow.Item(i + 1) = cVal
                                '        'tRow.Item(i + 2) = iVal
                                '        'tRow.Item(i + 3) = tSimRiskVal

                                '        'AverageLoss(i - COL_USERS_START) += CDbl(IIf(tShowDollarValue, tSimRiskValDoll, tSimRiskVal))
                                '        If (ShowRiskReduction AndAlso UseControlsReductions) Or ShowTotalRisk Then
                                '            'tData = SimulationResultsWithoutControls(DataDict.Keys(userIndex))
                                '            'cVal = LEC.EventSimulatedLikelihood(tData, AltID, LEC.NumberOfSimulations) * 100
                                '            'iVal = LEC.EventSimulatedImpact(tData, AltID, LEC.NumberOfSimulations) * 100
                                '            cVal = eAlt.SimulatedAltLikelihood * 100
                                '            iVal = eAlt.SimulatedAltImpact * 100
                                '            'iValDoll = iVal * DollarValueOfEnterprise / 100

                                '            'Simulated Risk                                    
                                '            tSimRiskVal = iVal * cVal / 100
                                '            tSimRiskValDoll = 0
                                '            if tShowDollarValue Then
                                '                tSimRiskValDoll = tSimRiskVal * DollarValueOfEnterprise / 100
                                '            End If

                                '            'AverageLossWithoutControls(i - COL_USERS_START) += CDbl(IIf(tShowDollarValue, tSimRiskValDoll, tSimRiskVal))
                                '        End If
                                '     End If
                                'End If

                                'If ShowTotalRisk Then

                                TotalRisk(i - COL_USERS_START) += CDbl(IIf(tShowDollarValue, RiskDollarValue, Risk))
                                TotalRiskWithoutControls(i - COL_USERS_START) += RiskWOControls
                                Dim tRiskSimulated As Double = If(AverageLoss.Count > i - COL_USERS_START, AverageLoss(i - COL_USERS_START), 0)
                                Dim tRiskSimulatedWithoutControls As Double = If(AverageLossWithoutControls.Count > i - COL_USERS_START, AverageLossWithoutControls(i - COL_USERS_START), 0)
                                Dim sRiskSimulated = HTMLCreateGraphBarWithValues(CSng(IIf(BarsRRelativeTo1, tRiskSimulated, tRiskSimulated / MaxPriorityR)), 100, BAR_W_SMALL, BAR_H_SMALL, If((PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) AndAlso tRiskSimulated > 0, "#f4ab5e", "#ffa792"), tBarTextColor, ImagePath, CStr(IIf(tShowDollarValue, CostString(tRiskSimulated * DollarValueOfEnterprise, CostDecimalDigits), Double2String(tRiskSimulated, tDecimalDigits, True))), True)  ' D6798

                                e.Row.Cells(i).Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:184px'><tr valign=middle>{2}{4}{3}{5}{0}</tr></table>", CStr(IIf(tRisk <> "", String.Format("<td>{0}</td>", tRisk), "")), CStr(IIf(sRiskSimulated <> "", String.Format("<td>{0}</td>", tRiskSimulated), "")), CStr(IIf(tLikelihood <> "", String.Format("<td>{0}</td>", tLikelihood), "")), CStr(IIf(tImpact <> "", String.Format("<td>{0}</td>", tImpact), "")), "", "")  ' D2417 + A0927 + A1371
                                'End If
                                e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                            End If
                        End If
                    Next

                    'If CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_EVENT_TYPE_ID, eAlt.NodeGuidID, Guid.Empty)) > 0 Then 
                    '    e.Row.Attributes("style") = "font-weight: bold;"
                    'End If
                Else
                    'summary row style and adjustment
                    Dim IsCategoryHeaderRow As Boolean = tRow.Item(COL_IS_SUMMARY).ToString.EndsWith("visible header summary")
                    If IsCategoryHeaderRow Then
                        e.Row.Cells(COL_NAME).Font.Size = FontUnit.Larger
                        e.Row.Cells(COL_NAME).VerticalAlign = VerticalAlign.Bottom
                        e.Row.Height = Unit.Pixel(32)
                        If CInt(tRow.Item(COL_TAG)) = Integer.MaxValue Then
                            e.Row.Cells(COL_NAME).Text = String.Format("{0}", tRow(COL_NAME).ToString)
                        Else
                            e.Row.Cells(COL_NAME).Text = String.Format("Category ""{0}""", tRow(COL_NAME).ToString)
                        End If
                    Else
                        If Not PM.CalculationsManager.UseSimulatedValues Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                            If CInt(tRow.Item(COL_TAG)) = Integer.MaxValue Then
                                e.Row.Cells(COL_NAME).Text = String.Format(ParseString("Total %%Risk%% of {0}:"), tRow(COL_NAME).ToString)
                            Else
                                e.Row.Cells(COL_NAME).Text = String.Format(ParseString("Total %%Risk%% of category ""{0}"":"), tRow(COL_NAME).ToString)
                            End If
                        Else
                            If CInt(tRow.Item(COL_TAG)) = Integer.MaxValue Then
                                e.Row.Cells(COL_NAME).Text += String.Format(ParseString("<br/>Total %%Loss%% of {0}:"), tRow(COL_NAME).ToString)
                            Else
                                e.Row.Cells(COL_NAME).Text += String.Format(ParseString("<br/>Total %%Loss%% of category ""{0}"":"), tRow(COL_NAME).ToString)
                            End If
                        End If
                        e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Right
                        'e.Row.RowState = DataControlRowState.Normal
                    End If
                    For Each cell As TableCell In e.Row.Cells
                        cell.ForeColor = SUMMARY_HEADER_COLOR
                        cell.HorizontalAlign = HorizontalAlign.Right
                        'cell.BackColor = SUMMARY_HEADER_BACKGROUND
                    Next
                    e.Row.Cells(COL_NAME).Height = Unit.Pixel(22)
                    If IsCategoryHeaderRow Then
                        e.Row.Height = Unit.Pixel(28)
                        e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Center
                    End If
                    If IsWorstCaseVisible Then e.Row.Cells(COL_WORST_CASE).Text = ""
                    For i As Integer = COL_USERS_START To e.Row.Cells.Count - 1
                        'e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Right
                        'e.Row.Cells(i).Style.Add("padding-right", "30px")
                        If e.Row.Cells(i).Text <> "" Then
                            e.Row.Cells(i).Text = String.Format("<span>{0}</span>", e.Row.Cells(i).Text)
                        End If
                        'If tShowResultsMode = ShowResultsModes.rmSimulated Then 'OrElse tShowResultsMode = ShowResultsModes.rmBoth Then
                        '    If Not IsCategoryHeaderRow AndAlso CInt(tRow.Item(COL_TAG)) <> Integer.MaxValue AndAlso ((i - COL_USERS_START) Mod 4 = 0) Then
                        '        Dim userIndex As Integer = CInt((i - COL_USERS_START) / 4)
                        '        Dim CurType As Integer = CInt(tRow.Item(COL_TAG))
                        '        Dim SimulatedRiskByCategory As Double = GetSimulatedRiskByCategory(CurType, userIndex)
                        '        If tShowDollarValue Then
                        '            e.Row.Cells(i).Text = e.Row.Cells(i).Text + "<br/>" + CostString(SimulatedRiskByCategory, CostDecimalDigits, True)
                        '        Else
                        '            e.Row.Cells(i).Text = e.Row.Cells(i).Text + "<br/>" + Double2String(SimulatedRiskByCategory * 100, tDecimalDigits, True)
                        '        End If
                        '        If ShowRiskReduction AndAlso UseControlsReductions Then
                        '            Dim SimulatedRiskReductionByCategory As Double = GetSimulatedRiskReductionByCategory(CurType, userIndex)
                        '            Dim sRiskREductionByCat As String
                        '            If tShowDollarValue
                        '                sRiskREductionByCat = CostString(SimulatedRiskReductionByCategory, CostDecimalDigits, True)
                        '            Else
                        '                sRiskREductionByCat = Double2String(SimulatedRiskReductionByCategory * 100, tDecimalDigits, True)
                        '            End If
                        '            e.Row.Cells(i).Text = e.Row.Cells(i).Text + String.Format(ParseString(" (%%loss%% reduction: {0})"), sRiskReductionByCat)
                        '        End If
                        '    End If
                        'End If
                    Next
                End If
            End If

            If e.Row.RowType = DataControlRowType.Footer Then 'AndAlso ShowTotalRisk Then                
                e.Row.Cells(COL_NAME).HorizontalAlign = HorizontalAlign.Right
                For i As Integer = COL_USERS_START To e.Row.Cells.Count - 1  ' D2534
                    'If i Mod 4 = 0 then
                    e.Row.Cells(COL_NAME).Text = ""
                    e.Row.Cells(i).Text = ""
                    e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Right
                    'e.Row.Cells(i).Text = String.Format("<table border=0 cellspacing=0 cellpadding=0 style='width:184px'><tr valign=middle><td><p align=""center""><small>Total Risk: {0}</small></p></td></tr></table>", Convert(TotalRisk(i - tUsersIndex)))

                    'Dim tblRisk As String = String.Format("<table class='text'><thead><tr><td class='th_detail'>{0}</td><td class='th_detail'>{1}</td><td class='th_detail'>{2}</td></tr></thead>", "&nbsp;", If(tShowResultsMode = ShowResultsModes.rmComputed, ParseString("Computed"), ""), If(tShowResultsMode = ShowResultsModes.rmSimulated, ParseString("Simulated"), ""))
                    Dim tblRisk As String = "<table class='text'>"
                    Dim sRisk As String = ""
                    Dim sLoss As String = ""
                    Dim sRiskReduction As String = ""
                    Dim sLossReduction As String = ""
                    Dim sRiskWithControls As String = ""
                    Dim sLossWithControls As String = ""
                    Dim sInvestmentLeverage As String = ""

                    If UseControlsReductions Then
                        Dim sCostOfControls As String = CostString(PM.Controls.CostOfFundedControls(), CostDecimalDigits, True) ' Total cost of all active controls 'A1303
                        Dim sNumOfControls As String = CStr(PM.Controls.EnabledControls.Sum(Function(ctrl) If(ctrl.Active, 1, 0)))
                        Dim sHowSelected As String = "Manually selected"
                        If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 1 Then sHowSelected = "Optimized based on simulated input and computed output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, PM.Parameters.DecimalDigits, True)
                        If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 2 Then sHowSelected = "Optimized based on computed input and simulated output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, PM.Parameters.DecimalDigits, True)
                        If PM.Parameters.Riskion_ControlsActualSelectionMode = 1 And PM.Parameters.Riskion_Use_Simulated_Values = 3 Then sHowSelected = "Optimized based on simulated input and output with budget of " + CostString(PM.ResourceAlignerRisk.RiskOptimizer.BudgetLimit, PM.Parameters.DecimalDigits, True)
                        Dim tbl As String = String.Format("<table class='text'><thead><tr><td class='th_detail'>{0}</td><td class='th_detail'>{1}</td><td class='th_detail'>{2}</td></tr></thead>", ParseString("# %%Controls%%"), ParseString("Cost of %%Controls%%"), ParseString("How Selected"))
                        tbl += String.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", sNumOfControls, sCostOfControls, sHowSelected)
                        e.Row.Cells(COL_NAME).Text = tbl + "</table>"
                    End If

                    If Not PM.CalculationsManager.UseSimulatedValues Then 'OrElse ShowResultsMode = ShowResultsModes.rmBoth Then
                        If ShowRiskReduction AndAlso UseControlsReductions Then
                            sRiskReduction = If(tShowDollarValue, CostString(TotalRiskWithoutControls(i - COL_USERS_START) - TotalRisk(i - COL_USERS_START), CostDecimalDigits, True), Math.Round((TotalRiskWithoutControls(i - COL_USERS_START) - TotalRisk(i - COL_USERS_START)) * 100, tDecimalDigits).ToString + "%")
                        End If
                        If ShowTotalRisk Then
                            sRiskWithControls = If(tShowDollarValue, CostString(TotalRisk(i - COL_USERS_START), CostDecimalDigits, True), Double2String(TotalRisk(i - COL_USERS_START) * 100, tDecimalDigits, True))
                            sRisk = If(tShowDollarValue, CostString(TotalRiskWithoutControls(i - COL_USERS_START), CostDecimalDigits, True), Math.Round(TotalRiskWithoutControls(i - COL_USERS_START) * 100, tDecimalDigits).ToString + "%")
                        End If
                    End If

                    If PM.CalculationsManager.UseSimulatedValues AndAlso AverageLossWithoutControls.Count > i - COL_USERS_START AndAlso AverageLoss.Count > i - COL_USERS_START Then 'OrElse ShowResultsMode = ShowResultsModes.rmBoth Then
                        ' show simulated average loss                        
                        If ShowRiskReduction AndAlso UseControlsReductions AndAlso GetHasControls() Then
                            sLossReduction = If(tShowDollarValue, CostString((AverageLossWithoutControls(i - COL_USERS_START) - AverageLoss(i - COL_USERS_START)) * DollarValueOfEnterprise, CostDecimalDigits, True), Math.Round(((AverageLossWithoutControls(i - COL_USERS_START) - AverageLoss(i - COL_USERS_START)) * 100), tDecimalDigits).ToString + "%")
                        End If
                        If ShowTotalRisk Then
                            sLossWithControls = If(tShowDollarValue, CostString(AverageLoss(i - COL_USERS_START) * DollarValueOfEnterprise, CostDecimalDigits, True), Double2String(AverageLoss(i - COL_USERS_START) * 100, tDecimalDigits, True))
                            sLoss = If(tShowDollarValue, CostString(AverageLossWithoutControls(i - COL_USERS_START) * DollarValueOfEnterprise, CostDecimalDigits, True), Math.Round(AverageLossWithoutControls(i - COL_USERS_START) * 100, tDecimalDigits).ToString + "%")
                        End If
                        If IsRiskWithControlsPage Then sInvestmentLeverage = If(InvestmentLeverage(i - COL_USERS_START) <> UNDEFINED_INTEGER_VALUE, Double2String(InvestmentLeverage(i - COL_USERS_START)), "")
                    End If

                    Dim tblRiskRows As String = ""
                    If sRisk <> "" Or sLoss <> "" Then tblRiskRows += String.Format("<tr><td>{0}</td><td>{1}</td><td class='td_risk_extra'>{2}</td></tr>", GetTotalRiskText(TotalRisk(i - COL_USERS_START)), sRisk, sLoss)
                    If GetHasControls() Then
                        If sRiskReduction <> "" Or sLossReduction <> "" Then tblRiskRows += String.Format("<tr><td>{0}</td><td>{1}</td><td class='td_risk_extra'>{2}</td></tr>", ParseString("%%Risk%% Reduction"), sRiskReduction, sLossReduction)
                        If IsRiskWithControlsPage AndAlso (sRiskWithControls <> "" Or sLossWithControls <> "") Then tblRiskRows += String.Format("<tr><td>{0}</td><td>{1}</td><td class='td_risk_extra'>{2}</td></tr>", ParseString("Residual %%Risk%%"), sRiskWithControls, sLossWithControls)
                    End If
                    If sInvestmentLeverage <> "" Then tblRiskRows += String.Format("<tr><td>{0}</td><td>{1}</td><td class='td_risk_extra'>{2}</td></tr>", ParseString("Investment Leverage"), "", sInvestmentLeverage)
                    e.Row.Cells(i).Text = If(tblRiskRows <> "", tblRisk + tblRiskRows + "</table>", "")
                Next
                If IsWorstCaseVisible Then
                    e.Row.Cells(2).Text = String.Format("{1}: {0}", Double2String(TotalMaxRisk * 100, tDecimalDigits, True), ResString("lblRiskTotalMax")) 'A0938
                    e.Row.Cells(2).HorizontalAlign = HorizontalAlign.Center
                End If
            End If
        Else
            Dim tRow As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim tUsersIndex As Integer = 3
            If e.Row.Cells.Count > COL_GUID Then e.Row.Cells(COL_GUID).Visible = False

            If e.Row.DataItem IsNot Nothing AndAlso e.Row.RowType <> DataControlRowType.Footer Then

                If e.Row.Cells.Count >= tUsersIndex Then
                    Dim tEventRecord As String = CStr(tRow.Item(COL_NAME))
                    e.Row.Cells(1).Text = tEventRecord
                    e.Row.Cells(1).HorizontalAlign = HorizontalAlign.Left
                End If

                For i As Integer = tUsersIndex To e.Row.Cells.Count - 1
                    If TypeOf (tRow.Item(i)) Is String Then
                        Dim tDataString As String = CStr(tRow.Item(i))
                        Dim tLikelihood As String = ""
                        Dim tImpact As String = ""
                        Dim tPriority As Double = CDbl(tDataString)
                        If HierarchyID = ECHierarchyID.hidLikelihood Then
                            tLikelihood = HTMLCreateGraphBarWithValues(CSng((tPriority / MaxLikelihood) * 100), 100, BAR_W, BAR_H, "#bdeb34", "", ImagePath, Double2String(tPriority * 100, tDecimalDigits, True), True) ' D2417
                            e.Row.Cells(i).Text = tLikelihood
                        Else
                            tImpact = HTMLCreateGraphBarWithValues(CSng((tPriority / MaxImpact) * 100), 100, BAR_W, BAR_H, "#abccff", "", ImagePath, Double2String(tPriority * 100, tDecimalDigits, True), True) ' D2417
                            e.Row.Cells(i).Text = tImpact
                        End If
                        e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                    End If
                Next
            End If
            If ShowOverallRow AndAlso e.Row.RowType = DataControlRowType.Footer Then
                e.Row.Cells(1).Text = "Overall:"
                e.Row.Cells(1).HorizontalAlign = HorizontalAlign.Left
                Dim tDataColumnsIndex As Integer = tUsersIndex
                Dim SelectedNodeGuid As Guid = New Guid(RadTreeHierarchy.SelectedNode.Value)
                For i As Integer = tDataColumnsIndex To e.Row.Cells.Count - 1
                    Dim tData = PM.CalculationsManager.GetRiskDataWRTNode(ColumnsUsersIDs.Keys(i), ColumnsUsersIDs.Values(i), PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID) 'A1064
                    Dim TotalPriority As Double = 0
                    For Each alt In tData.AlternativesData
                        If alt.AlternativeID = SelectedNodeGuid Then
                            TotalPriority = CDbl(IIf(HierarchyID = ECHierarchyID.hidLikelihood, alt.LikelihoodValue, alt.ImpactValue))
                        End If
                    Next
                    e.Row.Cells(i).Text = Double2String(TotalPriority * 100, tDecimalDigits, True)
                    e.Row.Cells(i).HorizontalAlign = HorizontalAlign.Center
                Next
            End If
        End If
    End Sub

    Public Function HTMLCreateGraphBarWithValues(Value As Single, MaxValue As Single, BWidth As Integer, BHeight As Integer, barColor As String, textColor As String, ImagesPath As String, Optional ByVal sGraphHint As String = "", Optional fShowValueOverride As Boolean = False) As String    ' D0136 + D0323 + D2188
        Const tMargin As Integer = 2

        Dim sFill As String = ""    ' D2923

        If MaxValue < 0 Then Return "&nbsp;"
        If MaxValue = 0 Then Value = 0 Else Value = CSng(Math.Abs(Value) / (MaxValue + 0.00000001))

        Dim FillWidth As Integer = -1
        If Math.Abs(Value) >= 0 And Math.Abs(Value) <= 100 Then FillWidth = CInt(Math.Round((BWidth - tMargin) * Math.Abs(Value))) - 1 ' D0147 'A0756
        If FillWidth < 0 Then FillWidth = 0
        If FillWidth > BWidth - tMargin Then FillWidth = BWidth - tMargin

        If Not IsExport Then    ' D2923
            Dim sLH As String = CStr(Math.Floor(100 * BHeight / 9))   ' D2417
            Dim sBG As String = CStr(IIf(FillWidth > 0 AndAlso FillWidth < BWidth - tMargin, " background: url(" + ImagesPath + "prg_bg_white.gif) repeat-y " + (FillWidth).ToString + "px", ""))
            If FillWidth > 0 Then sBG += CStr(IIf(sBG = "", " background:", "")) + " " + barColor + ";"
            Dim sVal As String = CStr(IIf(fShowValueOverride, sGraphHint, "&nbsp;"))
            Dim sBar As String = String.Format("<div class='bar_value' style='line-height:{0}%; width:100%; height:100%;{1}{4}' title='{3}'>{2}</div>", sLH, sBG, sVal, SafeFormString(sGraphHint), IIf(textColor = "", "", "color:" + textColor + ";"))
            sFill = String.Format("<div class='progress' style='height:{1}px;width:{2}px;padding:1px;'>{0}</div>", sBar, BHeight, BWidth)
        Else
            sFill = String.Format("<div><nobr><div style='display:inline;line-height:{2}px; height:{2}px; font-size:1px; width:{0}px; background:{3}; border:1px solid #d0d0d0; border-right:0px;'></div><div style='display:inline;line-height:{2}px; height:{2}px; font-size:1px; width:{1}px; background:{4}; border:1px solid #d0d0d0; border-left:0px;'></div><div class='text' style='display:inline;width:9ex; text-align:right'>{5}</div></nobr></div>", FillWidth, BWidth - FillWidth - tMargin, BHeight - tMargin, barColor, "#eaeaea", SafeFormString(sGraphHint), BWidth + 4)
        End If
        Return sFill
    End Function

    ' D2179 ===
    Protected Sub RadAjaxPanelTree_AjaxRequest(sender As Object, e As AjaxRequestEventArgs) Handles RadAjaxPanelTree.AjaxRequest
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(URLDecode(e.Argument))
        Dim sAction As String = (GetParam(args, "action")).ToLower.Trim 
        If sAction = "event_id_mode" Then
            PM.Parameters.NodeVisibleIndexMode = CType(Param2Int(args, "value"), IDColumnModes)
            PM.Parameters.Save()
            RadTreeHierarchy.Nodes.Clear()
            LoadHierarchy()
        End If
        If sAction = "show_event_numbers" Then
            PM.Parameters.NodeIndexIsVisible = Param2Bool(args, "value")
            PM.Parameters.Save()
            RadTreeHierarchy.Nodes.Clear()
            LoadHierarchy()
        End If
        Dim sGlobal As String = (GetParam(args, "global")).ToLower.Trim 
        Dim sLocal As String = (GetParam(args, "local")).ToLower.Trim   
        Dim sUseControls As String = (GetParam(args, "use_controls")).ToLower.Trim  
        If sGlobal <> "" OrElse sLocal <> "" OrElse sUseControls <> "" Then
            If sGlobal <> "" Then ShowGlobalPrty = (sGlobal = "1" OrElse sGlobal = "true")
            If sLocal <> "" Then ShowLocalPrty = (sLocal = "1" OrElse sLocal = "true")
            If sUseControls <> "" Then UseControlsReductionsHierarchy = (sUseControls = "1" OrElse sUseControls = "true")
            RadTreeHierarchy.Nodes.Clear()
            LoadHierarchy()
        End If
    End Sub
    ' D2179 ==

    Public Function GetGroupingCategories(ComboBoxWidth As Integer) As String
        Dim sRes As String = ""
        Dim tGroupByAttributeGUID As Guid = GroupByAttributeGUID
        If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then  ' D6798
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", GroupByRiskOpportunity, ParseString("%%Opportunity%% & %%Risk%%"), If(GroupByRiskOpportunity = tGroupByAttributeGUID, " selected='selected' ", ""))
        End If
        For Each tAttr As clsAttribute In AttributesList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", tAttr.ID, ShortString(App.GetAttributeName(tAttr), 40), If(tAttr.ID = tGroupByAttributeGUID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbGroupingCategories' style='width:{2}px; margin-bottom:-1px;' onchange='onSetGroupingCategory(this.value);' {1}>{0}</select>", sRes, IIf(IsGroupByAttribute, "", " disabled='disabled' "), ComboBoxWidth)
    End Function

    Protected Sub GridResults_Sorting(sender As Object, e As GridViewSortEventArgs) Handles GridResults.Sorting
        If SortDirection = SortDirection.Ascending Then SortDirection = SortDirection.Descending Else SortDirection = SortDirection.Ascending
        SortExpression = e.SortExpression
        BuildGridViewControl(e.SortExpression, SortDirection)
    End Sub

    Public Property SortDirection() As SortDirection
        Get
            Return CType(PM.Attributes.GetAttributeValue(CType(IIf(IsRiskResultsByEventPage, ATTRIBUTE_RISK_GRID_SORT_DIRECTION2_ID, ATTRIBUTE_RISK_GRID_SORT_DIRECTION_ID), Guid), UNDEFINED_USER_ID), SortDirection)
        End Get
        Set(value As SortDirection)
            ' D6664 ===
            If IsWidget Then
                PM.Attributes.SetAttributeValue(CType(IIf(IsRiskResultsByEventPage, ATTRIBUTE_RISK_GRID_SORT_DIRECTION2_ID, ATTRIBUTE_RISK_GRID_SORT_DIRECTION_ID), Guid), UNDEFINED_USER_ID, AttributeValueTypes.avtLong, CInt(value), Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, CType(IIf(IsRiskResultsByEventPage, ATTRIBUTE_RISK_GRID_SORT_DIRECTION2_ID, ATTRIBUTE_RISK_GRID_SORT_DIRECTION_ID), Guid), AttributeValueTypes.avtLong, CInt(value))
            End If
            ' D6664 ==
        End Set
    End Property

    Public Property SortExpression As String
        Get
            Dim retVal As String = ""
            retVal = CStr(PM.Attributes.GetAttributeValue(CType(IIf(IsRiskResultsByEventPage, ATTRIBUTE_RISK_GRID_SORT_EXPRESSION2_ID, ATTRIBUTE_RISK_GRID_SORT_EXPRESSION_ID), Guid), UNDEFINED_USER_ID))
            If String.IsNullOrEmpty(retVal) Then retVal = ResString("tblNo_")
            Return retVal
        End Get
        Set(value As String)
            ' D6664 ===
            If IsWidget Then
                PM.Attributes.SetAttributeValue(CType(IIf(IsRiskResultsByEventPage, ATTRIBUTE_RISK_GRID_SORT_EXPRESSION2_ID, ATTRIBUTE_RISK_GRID_SORT_EXPRESSION_ID), Guid), UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, CType(IIf(IsRiskResultsByEventPage, ATTRIBUTE_RISK_GRID_SORT_EXPRESSION2_ID, ATTRIBUTE_RISK_GRID_SORT_EXPRESSION_ID), Guid), AttributeValueTypes.avtString, value)
            End If
            ' D6664 ==
        End Set
    End Property

    Private ReadOnly Property IsGoalNodeSelected As Boolean
        Get
            If RadTreeHierarchy.SelectedNode IsNot Nothing AndAlso RadTreeHierarchy.Nodes.Count > 0 Then
                Dim SelectedNodeGuid As Guid = New Guid(RadTreeHierarchy.SelectedNode.Value)
                If SelectedNodeGuid.Equals(PM.Hierarchy(HierarchyID).Nodes(0).NodeGuidID) Then Return True
            End If
            Return False
        End Get
    End Property

    Private Function SkipAlternative(Alt As clsNode, tCurrentFiltersList As List(Of clsFilterItem)) As Boolean
        Dim retVal As Boolean = False
        With PM
            If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES Then
                If HierarchyID = ECHierarchyID.hidLikelihood AndAlso Not IsGoalNodeSelected Then
                    If Not MiscFuncs.HasContribution(Alt, .Hierarchy(.ActiveHierarchy)) Then
                        retVal = True
                    End If
                End If
            End If

            If Not retVal AndAlso ShowAttributes AndAlso IsFilterApplied Then
                If tCurrentFiltersList IsNot Nothing AndAlso CurrentFiltersList.Count > 0 Then
                    retVal = Not Alt.IsNodeIncludedInFilter(tCurrentFiltersList)
                End If
            End If
        End With

        If Not retVal AndAlso IsWidget AndAlso Alt.DisabledForUser(PM.User.UserID) Then retVal = True
        Return retVal
    End Function

    Public ReadOnly Property ActiveProjectHasAlternativeAttributes As Boolean
        Get
            If App IsNot Nothing AndAlso App.ActiveProject IsNot Nothing AndAlso PM IsNot Nothing AndAlso PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing Then
                For Each attr In PM.Attributes.AttributesList
                    If attr.Type = CurrentAttributesType AndAlso Not attr.IsDefault Then Return True
                Next
            End If
            Return False
        End Get
    End Property

    Private _FilterCombination As FilterCombinations = FilterCombinations.fcAnd
    Public Property FilterCombination As FilterCombinations
        Get
            Return _FilterCombination
        End Get
        Set(value As FilterCombinations)
            _FilterCombination = value
        End Set
    End Property

    Private _CurrentFiltersList As List(Of clsFilterItem) = Nothing
    Public Property CurrentFiltersList As List(Of clsFilterItem)
        Get
            If _CurrentFiltersList Is Nothing Then
                ' D2668 ===
                Dim tSessVar = Session(SESSION_FILTER_RULES)
                If tSessVar Is Nothing OrElse Not (TypeOf tSessVar Is List(Of clsFilterItem)) Then
                    _CurrentFiltersList = New List(Of clsFilterItem)
                    Session.Add(SESSION_FILTER_RULES, _CurrentFiltersList)
                Else
                    _CurrentFiltersList = CType(tSessVar, List(Of clsFilterItem))
                End If
                ' D2668 ==
            End If
            Return _CurrentFiltersList
        End Get
        Set(value As List(Of clsFilterItem))
            _CurrentFiltersList = value
            Session(SESSION_FILTER_RULES) = value
        End Set
    End Property

    Private ReadOnly Property CurrentAttributesType As AttributeTypes
        Get
            Dim tCurrentAttrType As AttributeTypes = AttributeTypes.atAlternative
            If IsRiskResultsByEventPage Then
                If HierarchyID = ECHierarchyID.hidLikelihood Then
                    tCurrentAttrType = AttributeTypes.atLikelihood
                Else
                    tCurrentAttrType = AttributeTypes.atImpact
                End If
            End If
            Return tCurrentAttrType
        End Get
    End Property

    Private sFilterData As String = ""

    ' D2666 ===
    Public Function LoadAttribData() As String
        Dim sList As String = ""
        With PM
            Dim fIsEmpty As Boolean = CurrentFiltersList.Count = 0
            Dim tSelectedColumns As String = SelectedColumns
            For Each tAttr As clsAttribute In .Attributes.AttributesList
                If Not tAttr.IsDefault AndAlso tAttr.Type = CurrentAttributesType Then
                    Dim sVals As String = ""
                    If tAttr.ValueType = AttributeValueTypes.avtEnumeration OrElse tAttr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                        Dim tVals As clsAttributeEnumeration = .Attributes.GetEnumByID(tAttr.EnumID)
                        If tVals IsNot Nothing Then
                            For Each tVal As clsAttributeEnumerationItem In tVals.Items
                                sVals += CStr(IIf(sVals = "", "", ",")) + String.Format("['{0}','{1}']", tVal.ID, tVal.Value)
                            Next
                        End If
                    End If
                    sList += CStr(IIf(sList = "", "", ",")) + String.Format("['{0}','{1}',{2},[{3}],{4}]", tAttr.ID, JS_SafeString(App.GetAttributeName(tAttr)), CInt(tAttr.ValueType), sVals, CStr(IIf(SelectedColumns.Contains(tAttr.ID.ToString), "1", "0")))
                    If fIsEmpty Then CurrentFiltersList.Add(New clsFilterItem With {.IsChecked = False, .SelectedAttributeID = tAttr.ID, .FilterOperation = FilterOperations.Equal})
                End If
            Next
        End With
        sFilterData = GetFilterData(CurrentFiltersList)
        'Return String.Format("var attr_list = [{0}];{1} var attr_flt = [{2}];", sList, vbCrLf, sFlt)
        Return String.Format("[{0}]", sList)
    End Function
    ' D2666 ==

    Private Function isAnyFilterItemChecked(ByVal fList As List(Of clsFilterItem)) As Boolean
        If fList IsNot Nothing AndAlso fList.Count > 0 Then
            For Each item As clsFilterItem In fList
                If item.IsChecked Then Return True
            Next
        End If
        Return False
    End Function

    Private Function ParseFilterString(sFilter As String) As List(Of clsFilterItem)
        Dim retVal As List(Of clsFilterItem) = New List(Of clsFilterItem)

        Dim sRows() As String = sFilter.Trim.Split(Flt_Row_Separator)
        For Each sRow As String In sRows
            Dim tSeparators As String() = New String(0) {}
            tSeparators(0) = Flt_Separator
            Dim tVals As String() = sRow.Split(tSeparators, StringSplitOptions.None)
            If tVals.Length > 2 Then
                Dim sChecked As String = (tVals(0)) 
                Dim sAttrID As String = (tVals(1))  
                Dim sOperID As String = (tVals(2))  
                Dim sFilterText As String = ""
                If tVals.Length = 4 Then
                    sFilterText = (tVals(3))  
                End If
                'If sChecked = "1" Then
                Dim tFilterItem As New clsFilterItem With {.FilterCombination = FilterCombination}
                Dim tAttrID As Guid = New Guid(sAttrID)

                With PM
                    For Each tAttr As clsAttribute In .Attributes.AttributesList
                        If tAttr.Type = CurrentAttributesType AndAlso tAttr.ID.Equals(tAttrID) Then
                            tFilterItem.SelectedAttributeID = tAttr.ID
                            Exit For ' D2668
                        End If
                    Next
                End With

                tFilterItem.IsChecked = (sChecked = "1")  ' D2668
                tFilterItem.FilterOperation = CType(CInt(sOperID), FilterOperations)
                tFilterItem.FilterText = sFilterText

                Dim tAttribute = PM.Attributes.GetAttributeByID(tFilterItem.SelectedAttributeID)
                If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumeration Then
                    tFilterItem.FilterEnumItemID = New Guid(sFilterText)
                End If
                If tAttribute IsNot Nothing AndAlso tAttribute.Type = CurrentAttributesType AndAlso tAttribute.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                    Dim sGuids As String() = sFilterText.Split(CChar(";"))
                    For Each sGuid In sGuids
                        If sGuid.Length > 0 Then
                            If tFilterItem.FilterEnumItemsIDs Is Nothing Then tFilterItem.FilterEnumItemsIDs = New List(Of Guid)
                            tFilterItem.FilterEnumItemsIDs.Add(New Guid(sGuid))
                        End If
                    Next
                End If

                retVal.Add(tFilterItem)
            End If
        Next

        Return retVal
    End Function

    Public Function GetFilterData(ByRef fList As List(Of clsFilterItem)) As String
        Dim sFlt As String = ""
        Dim i As Integer = 0
        While i < fList.Count
            Dim tVal As clsFilterItem = fList(i)
            Dim sVal As String = ""
            Dim tAttr = PM.Attributes.GetAttributeByID(tVal.SelectedAttributeID)
            If tAttr IsNot Nothing Then
                Select Case tAttr.ValueType
                    Case AttributeValueTypes.avtEnumeration
                        sVal = CStr(IIf(tVal.FilterEnumItemID.Equals(Guid.Empty), "", tVal.FilterEnumItemID.ToString))
                    Case AttributeValueTypes.avtEnumerationMulti
                        sVal = ""
                        If tVal.FilterEnumItemsIDs IsNot Nothing Then
                            For Each ID As Guid In tVal.FilterEnumItemsIDs
                                If sVal.Length > 0 Then sVal += ";"
                                sVal += ID.ToString
                            Next
                        End If

                    Case Else
                        sVal = tVal.FilterText
                End Select
                sFlt += CStr(IIf(sFlt = "", "", ",")) + String.Format("['{0}','{1}',{2},'{3}']", IIf(tVal.IsChecked, 1, 0), tAttr.ID, CInt(tVal.FilterOperation), JS_SafeString(sVal))
                i += 1
            Else
                fList.RemoveAt(i)
            End If
        End While
        Return sFlt
    End Function

    Public Function LoadFilterData() As String
        Return String.Format("[{0}]", sFilterData)
    End Function

    Private Sub CalculateOverall(Optional UseControls As Boolean = False)
        With PM
            .StorageManager.Reader.LoadUserData()
            Dim CurrentHID As Integer = .ActiveHierarchy ' saving current hierarchy id

            Dim oldUseReductions As Boolean = .CalculationsManager.UseReductions
            .CalculationsManager.UseReductions = UseControls

            .ActiveHierarchy = HierarchyID
            .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, .CombinedGroups.GetDefaultCombinedGroup), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy)

            .ActiveHierarchy = CurrentHID ' restoring current hierarchy id
            .CalculationsManager.UseReductions = oldUseReductions
        End With
    End Sub

    Public Overrides Sub VerifyRenderingInServerForm(ctrl As Control)
        'Verifies that the control is rendered
    End Sub

    Protected Sub Export()
        Try
            RawResponseStart()
            Dim utf16 As New UTF8Encoding()
            Dim sWriter As New StringWriter()
            Dim hWriter As New HtmlTextWriter(sWriter)
            ' D2923 ===
            IsExport = True
            Dim UseSorting As Boolean = GridResults.AllowSorting
            GridResults.AllowSorting = False
            Dim tWidth As Unit = GridResults.Width
            GridResults.Width = Unit.Empty
            GridResults.BorderWidth = 1
            UpdateControl()
            GridResults.RenderControl(hWriter)
            GridResults.AllowSorting = UseSorting
            GridResults.Width = tWidth
            GridResults.BorderWidth = 0
            IsExport = False

            Dim sContent As String = File_GetContent(MapPath("Export_template.xls"), "")
            If Not sContent.Contains("%%") Then
                sContent = sWriter.ToString
            Else
                sContent = ParseTemplate(ParseTemplate(sContent, "%%title%%", Page.Title, False), "%%content%%", sWriter.ToString, False)
            End If

            DownloadContent(sContent, "application/vnd.ms-excel", GetProjectFileName(App.ActiveProject.ProjectName, "Risk Results", "", ".xls"), dbObjectType.einfFile, App.ProjectID)  ' D6593

            'Dim sDownloadName As String = SafeFileName(App.ActiveProject.ProjectName)
            'Response.AddHeader("Content-Length", utf16.GetByteCount(sContent).ToString)
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.xls""", HttpUtility.UrlEncode(sDownloadName)))   ' D6591
            'Response.ContentType = "application/vnd.ms-excel"
            'Response.Write(sContent)
            'If Response.IsClientConnected Then RawResponseEnd()
            ' D2923 ==

        Catch ex As Exception
        End Try
    End Sub

    Protected Sub ExportBowTie()
        Try
            RawResponseStart()
            Dim utf16 As New UTF8Encoding()
            Dim sWriter As New StringWriter()
            Dim hWriter As New HtmlTextWriter(sWriter)

            IsExport = True
            Dim expModel As New ExportModelRisk(GetExportDataItems)
            Dim sContent As String = File_GetContent(MapPath("Export_template_bowtie.xml"), "")
            sContent = expModel.ExportToExcelXML(sContent)
            sContent = ParseString(sContent.Replace("%%lblObjectivesSources%%", "%%Sources%%"))
            sContent = sContent.Replace("%%lblRiskLegendA%%", ResString("lblRiskLegendA"))
            sContent = sContent.Replace("%%lblRiskLegendB%%", ResString("lblRiskLegendB"))
            sContent = sContent.Replace("%%optSortByCauseLkh%%", ResString("optSortByCauseLkh"))
            sContent = sContent.Replace("%%optSortByConsequenceImp%%", ResString("optSortByConsequenceImp"))
            sContent = sContent.Replace("%%lblRiskLegendC%%", ResString("lblRiskLegendC"))
            sContent = sContent.Replace("%%lblRiskLegendD%%", ResString("lblRiskLegendD"))
            sContent = ParseString(sContent.Replace("%%lblConsequences%%", "%%Objectives(i)%%"))
            IsExport = False


            DownloadContent(sContent, "application/vnd.ms-excel", GetProjectFileName(App.ActiveProject.ProjectName, "Bow Tie", "", ".xls"), dbObjectType.einfFile, App.ProjectID)   ' D6593

            'Response.ContentType = "application/vnd.ms-excel"
            'Response.AddHeader("Content-Length", utf16.GetByteCount(sContent).ToString)
            'Dim sDownloadName As String = SafeFileName(App.ActiveProject.ProjectName)
            'Response.AppendHeader("Content-Disposition", String.Format("attachment; filename=""{0}.xml""", HttpUtility.UrlEncode(sDownloadName)))	' D6591
            'Response.Write(sContent)
            'If Response.IsClientConnected Then RawResponseEnd()

        Catch ex As Exception
        End Try
    End Sub

    Protected Sub Page_PreRender1(sender As Object, e As EventArgs) Handles Me.PreRender
        Dim sPgid As String = (CheckVar("pgid", "")).ToLower
        For i As Integer = 0 To PageIDs.Length - 1
            If PageIDs(i).ToString.ToLower = sPgid Then
                Dim ttl As String = String.Format(ParseAllTemplates(PageTitles(i), App.ActiveUser, App.ActiveProject), "<span id='divNodeName'></span>")
                If ttl.Contains("id='lblWC'") Then
                    ttl = ttl.Insert(ttl.IndexOf("id='lblWC'") + "id='lblWC'".Length, If((IsRiskWithControlsPage OrElse IsBowTiePage OrElse IsRiskMapPage) AndAlso Not UseControlsReductions, " style='display: none;' ", ""))
                End If

                ttl += String.Format(" for <u>{0}</u>", SafeFormString(PRJ.ProjectName))
                ttl += String.Format("<small id='lblSimMode'></small>")

                If IsWidget Then ttl = ParseString(String.Format("%%Likelihoods%%, %%Impacts%% and %%Risks%% for {0}'s {1}", PM.User.HTMLDisplayName, SafeFormString(App.ActiveProject.ProjectName)))

                divHeader.InnerHtml = ttl + If(PM.CalculationsManager.UseSimulatedValues, ", simulated", "")
                Exit For
            End If
        Next
        If CheckVar(_PARAM_ACTION, "").ToLower = "download" Then
            If IsBowTiePage Then ExportBowTie() Else Export()
        End If
    End Sub

    Protected Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)
        Dim sAction As String = (GetParam(args, "action")).ToLower  
        Dim tResult As String = CStr(IIf(String.IsNullOrEmpty(sAction), "", sAction))

        Select Case sAction
            Case ACTION_GET_TREATMENTS
                Dim NodeID As Guid = New Guid((GetParam(args, "node").Trim()))
                Dim CtrlType As ControlType = CType(CInt((GetParam(args, "ctrl_type").Trim())), ControlType)

                Dim ctrls As String = ""

                If Treatments IsNot Nothing Then
                    For Each ctrl As clsControl In _Treatments
                        If ctrl.Type = CtrlType Then
                            For Each assignment As clsControlAssignment In ctrl.Assignments
                                If (assignment.ObjectiveID = NodeID) AndAlso (CtrlType = ControlType.ctCause OrElse assignment.EventID = SelectedEventID) AndAlso assignment.IsActive Then
                                    ctrls += CStr(IIf(ctrls <> "", ",", "")) + String.Format("['{0}', '{1}', {2}]", ctrl.ID, JS_SafeString(ctrl.Name), JS_SafeNumber(Math.Round(assignment.Value * 100, PM.Parameters.DecimalDigits)))
                                End If
                            Next
                        End If
                    Next
                End If

                Dim EventNode As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(SelectedEventID)
                Dim EventName As String = EventNode.NodeName

                Dim ObjName As String = ""
                Dim ObjType As String = ""
                If CtrlType = ControlType.ctCause OrElse CtrlType = ControlType.ctCauseToEvent Then
                    Dim ObjNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(NodeID)
                    ObjName = ObjNode.NodeName
                    ObjType = "0"
                    If EventNode IsNot Nothing AndAlso CtrlType = ControlType.ctCauseToEvent AndAlso Not ObjNode.GetContributedAlternatives.Contains(EventNode) Then
                        ObjType = "-1"
                    End If
                Else
                    ObjName = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(NodeID).NodeName
                    ObjType = "2"
                End If

                If CtrlType = ControlType.ctCause Then EventName = "" 'ParseString("%%Alternatives%%")
                tResult = String.Format("['{0}',{1},[{2}],'{3}']", JS_SafeString(SafeFormString(ObjName)), ObjType, ctrls, JS_SafeString(SafeFormString(EventName)))
            Case ACTION_SET_BUBBLE_SIZE
                RiskMapBubbleSize = CInt((GetParam(args, "val").Trim()))
                tResult = String.Format("['{0}']", sAction)
            Case "add_category" 'add a new treatment category
                Dim tCatName As Object = (GetParam(args, "name")).Trim  
                Dim catID As Guid = Guid.NewGuid()
                AddEnumAttributeItem(ATTRIBUTE_CONTROL_CATEGORY_ID, AttributeTypes.atControl, catID, CStr(tCatName), True)
                tResult = String.Format("['OK','{0}']", catID.ToString)
            Case "delete_category"
                Dim catID As Guid = New Guid((GetParam(args, "cat_id")))
                RemoveCategoryItem(catID)
                tResult = "['OK']"
            Case "rename_category"
                Dim catID As Guid = New Guid((GetParam(args, "cat_id")))
                Dim catName As String = (GetParam(args, "cat_name"))
                RenameCategoryItem(catID, catName)
                tResult = "['OK']"
                ' Edit Event Attributes on Bow-Tie
            Case "set_checked_value"
                Dim chk As String = (GetParam(args, "chk"))
                Dim attr As String = (GetParam(args, "attr"))
                Dim item_index As Integer = CInt((GetParam(args, "item_index")))
                Dim tAttr As clsAttribute = PM.Attributes.GetAttributeByID(New Guid(attr))
                Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(SelectedEventID)
                If tAttr IsNot Nothing AndAlso tAlt IsNot Nothing Then
                    Dim tValue As Object = Nothing
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration
                            If chk = "1" Then
                                Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                                If tEnum IsNot Nothing AndAlso tEnum.Items.Count > item_index Then
                                    tValue = tEnum.Items(item_index).ID
                                End If
                            End If
                        Case AttributeValueTypes.avtEnumerationMulti
                            Dim sValue As String = CStr(PM.Attributes.GetAttributeValue(tAttr.ID, tAlt.NodeGuidID))
                            Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                            If tEnum IsNot Nothing AndAlso tEnum.Items.Count > item_index Then
                                If String.IsNullOrEmpty(sValue) Then sValue = ""
                                Dim gID = tEnum.Items(item_index).ID
                                Dim sPos As Integer = sValue.IndexOf(gID.ToString)
                                If sPos <> -1 Then
                                    sValue = sValue.Remove(sPos, gID.ToString.Length)
                                    If sPos > 0 AndAlso sValue(sPos - 1) = ";" Then sValue = sValue.Remove(sPos - 1, 1)
                                End If

                                If chk = "1" Then
                                    If tEnum.Items.Count > CInt(item_index) Then
                                        sValue += CStr(IIf(sValue = "", "", ";")) + tEnum.Items(item_index).ID.ToString
                                    End If
                                End If
                            End If
                            tValue = sValue
                    End Select
                    PM.Attributes.SetAttributeValue(tAttr.ID, UNDEFINED_USER_ID, tAttr.ValueType, tValue, tAlt.NodeGuidID, Guid.Empty)
                    PM.Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, UNDEFINED_USER_ID)
                    tResult = GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
                End If
            Case "set_default_value"
                Dim chk As String = (GetParam(args, "chk"))
                Dim attr As String = (GetParam(args, "attr"))
                Dim item_index As Integer = CInt((GetParam(args, "item_index")))
                Dim tAttr As clsAttribute = PM.Attributes.GetAttributeByID(New Guid(attr))
                If tAttr IsNot Nothing Then
                    Dim tValue As Object = Nothing
                    Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                    If tEnum IsNot Nothing AndAlso tEnum.Items.Count > item_index Then
                        tValue = tEnum.Items(item_index).ID
                    End If
                    tAttr.DefaultValue = tValue
                    PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
                    tResult = GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
                End If
            Case "delete_enum_attr_value"
                Dim attr As String = (GetParam(args, "attr"))
                Dim item_index As Integer = CInt((GetParam(args, "item_index")))
                Dim tAttr As clsAttribute = PM.Attributes.GetAttributeByID(New Guid(attr))
                If tAttr IsNot Nothing Then
                    Dim fChanged As Boolean = False
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                            Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                            If tEnum IsNot Nothing AndAlso tEnum.Items.Count > item_index Then
                                tEnum.Items.RemoveAt(item_index)
                                fChanged = True
                            End If
                    End Select
                    If fChanged Then
                        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
                    End If
                    tResult = GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
                End If
            Case "edit_enum_attr_value"
                Dim attr As String = (GetParam(args, "attr"))
                Dim item_index As Integer = CInt((GetParam(args, "item_index")))
                Dim value As String = (GetParam(args, "value"))
                Dim tAttr As clsAttribute = PM.Attributes.GetAttributeByID(New Guid(attr))
                If tAttr IsNot Nothing Then
                    Dim fChanged As Boolean = False
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                            Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                            If tEnum IsNot Nothing AndAlso tEnum.Items.Count > item_index Then
                                tEnum.Items(item_index).Value = value
                                fChanged = True
                            End If
                    End Select
                    If fChanged Then
                        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
                    End If
                    tResult = GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
                End If
            Case "add_enum_attr_value"
                Dim attr As String = (GetParam(args, "attr"))
                Dim value As String = (GetParam(args, "value"))
                Dim tAttr As clsAttribute = PM.Attributes.GetAttributeByID(New Guid(attr))
                If tAttr IsNot Nothing Then
                    Dim fChanged As Boolean = False
                    Select Case tAttr.ValueType
                        Case AttributeValueTypes.avtEnumeration, AttributeValueTypes.avtEnumerationMulti
                            Dim tEnum As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                            If tEnum IsNot Nothing Then
                                tEnum.AddItem(value)
                                fChanged = True
                            End If
                    End Select
                    If fChanged Then
                        PM.Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
                    End If
                    tResult = GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
                End If
                ' End edit Event Attributes on Bow-Tie
            Case "edit_budget"
                Dim sValue As String = (GetParam(args, "value")).ToLower   ' Anti-XSS
                Dim dValue As Double = 0
                Dim res As Boolean = False
                If String.IsNullOrEmpty(sValue) Then
                    RA.RiskOptimizer.CurrentScenario.Budget = 0
                    res = True
                Else
                    If String2Double(sValue, dValue) Then
                        RA.RiskOptimizer.CurrentScenario.Budget = dValue
                        res = True
                    End If
                End If
                If res Then
                    RA.Save()
                End If
                tResult = String.Format("['{0}']", sAction)
            Case "optimizer_type"
                OptimizationTypeAttribute = CType((GetParam(args, "value")), RiskOptimizationType)  ' Anti-XSS                
                tResult = String.Format("['{0}']", sAction)
            Case "event_group_data"
                Dim eventId As Integer = CInt((GetParam(args, "event_id")))
                'Dim eventGuid As Guid = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(eventId).NodeGuidID
                Dim tAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(eventId)
                Dim sGroupName As String = PM.EventsGroups.GroupName(tAlt)
                Dim sEventPrecedence As String = PM.EventsGroups.GroupPrecedence(tAlt).ToString
                If sEventPrecedence = Integer.MaxValue.ToString Then sEventPrecedence = ""
                tResult = String.Format("['{0}',{1},'{2}','{3}']", sAction, eventId, JS_SafeString(sGroupName), sEventPrecedence)
            Case "selected_dollar_value_target"
                Dim sTarget As String = (GetParam(args, "value")).Trim   
                If DollarValueTarget <> sTarget Then DollarValueTarget = sTarget
                Dim sValue As String = (GetParam(args, "doll_value")).Trim   
                Dim tValue As Double
                If Not String.IsNullOrEmpty(sValue) AndAlso String2Double(sValue, tValue) AndAlso DollarValue <> tValue Then
                    DollarValue = tValue
                End If
                tResult = String.Format("['selected_dollar_value_target',{0},{1},{2},'{3}',{4}]", GetAllObjsData(), GetEventsData(), JS_SafeNumber(DollarValue), DollarValueTarget, JS_SafeNumber(DollarValueOfEnterprise))
            Case "set_dollar_value"
                Dim sTarget As String = (GetParam(args, "target")).Trim   
                Dim sValue As String = (GetParam(args, "value")).Trim   
                Dim dValue As Double
                If Not String.IsNullOrEmpty(sValue) AndAlso String2Double(sValue, dValue) Then
                    DollarValue = dValue
                End If
                If DollarValueTarget <> sTarget Then DollarValueTarget = sTarget
                RA.RiskOptimizer.InitOriginalValues()

                tResult = String.Format("['set_dollar_value',{0},{1},{2},'{3}',{4}]", GetAllObjsData(), GetEventsData(), JS_SafeNumber(DollarValue), DollarValueTarget, JS_SafeNumber(DollarValueOfEnterprise))
            Case "get_optimal_unmber_of_trials"
                PM.RiskSimulations.NumberOfTrials = PM.RiskSimulations.GetEstimatedNumberOfSimulations()
                tResult = String.Format("['get_optimal_unmber_of_trials',{0}]", JS_SafeNumber(PM.RiskSimulations.NumberOfTrials))
            Case "add_filter"
                Dim sName As String = (GetParam(args, "name")).Trim   
                Dim sValue As String = (GetParam(args, "value")).Trim   
                Dim sComb As String = (GetParam(args, "combination")).Trim   
                If (sName <> "") Then
                    Dim sFilter As String = String.Format("['{0}','{1}',{2}]", JS_SafeString(sName), JS_SafeString(sValue), sComb)
                    PM.Parameters.AlternativeFilters += CStr(IIf(PM.Parameters.AlternativeFilters = "", "", ",")) + sFilter
                    PM.Parameters.Save()
                End If
                tResult = String.Format("['{0}',[{1}]]", sAction, PM.Parameters.AlternativeFilters)
            Case "get_filters"
                tResult = String.Format("['{0}',[{1}]]", sAction, PM.Parameters.AlternativeFilters)
            Case "update_filters"
                Dim sValue As String = (GetParam(args, "value")).Trim                   
                PM.Parameters.AlternativeFilters = sValue
                PM.Parameters.Save()
                tResult = String.Format("['{0}',[{1}]]", sAction, PM.Parameters.AlternativeFilters)
            Case "clear_filters"
                PM.Parameters.AlternativeFilters = ""
                PM.Parameters.Save()
                tResult = String.Format("['{0}',[{1}]]", sAction, PM.Parameters.AlternativeFilters)
            Case "parse_filter"
                Dim sValue As String = (GetParam(args, "value")).Trim   
                Dim sComb As String = (GetParam(args, "combination")).Trim   
                Dim fList As List(Of clsFilterItem) = ParseFilterString(sValue)
                Dim sList As String = GetFilterData(fList)
                tResult = String.Format("['{0}',[{1}],{2}]", sAction, sList, sComb)
            Case "manual_zoom"
                Dim sValue As String = (GetParam(args, "params")).ToLower   ' Anti-XSS
                PM.Parameters.Riskion_RiskPlotManualZoomParams = sValue
                PM.Parameters.Save()
                tResult = String.Format("['{0}','{1}']", sAction, JS_SafeString(sValue))
            Case "reverse_zoom"
                Dim sValue As String = (GetParam(args, "value")).ToLower   ' Anti-XSS
                Dim sDir As String = (GetParam(args, "dir")).ToLower   ' Anti-XSS
                Dim dValue As Integer = 0
                If String.IsNullOrEmpty(sValue) Then
                    Select Case sDir
                        Case "top"
                            RiskMapReverseZoomValueTop = 0
                        Case "left"
                            RiskMapReverseZoomValueLeft = 0
                        Case "right"
                            RiskMapReverseZoomValueRight = 0
                        Case "bottom"
                            RiskMapReverseZoomValueBottom = 0
                    End Select
                Else
                    If Integer.TryParse(sValue, dValue) Then
                        Select Case sDir
                            Case "top"
                                RiskMapReverseZoomValueTop = dValue
                            Case "left"
                                RiskMapReverseZoomValueLeft = dValue
                            Case "right"
                                RiskMapReverseZoomValueRight = dValue
                            Case "bottom"
                                RiskMapReverseZoomValueBottom = dValue
                        End Select
                    End If
                End If
                tResult = String.Format("['{0}']", sAction)
            Case "get_rand_seed"
                tResult = String.Format("['{0}', {1}]", sAction, JS_SafeNumber(PM.RiskSimulations.RandomSeed))
                'Case "show_hidden_settings"
                '    ShowHiddenSettings = Param2Bool(args, "val")
                '    tResult = String.Format("['{0}']", sAction)            
        End Select

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
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
                sButtons += String.Format("<input name='btnBackToEval' class='button' style='width:6em; margin-left:1ex;' onclick='loadURL(""{0}""); return false;' type='button' value='{1}**'/>", PageURL(_PGID_RISK_PLOT_OVERALL, sParams), SafeFormString(ResString("btnEvaluationNext")))
            End If
            sButtons += String.Format("<div class='text small gray' style='text-align:center'>*{0}{1}{2}</div>", ResString(If(isRiskResults, If(isImpact, "lblPrevLikelihood", "lblNextImpact"), "lblNextRiskResults")), If(isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE, "<br>**", ""), If(isRiskResults AndAlso _OPT_SHOW_HEATMAP_IN_PIPE, ResString("lblNextHeatMap"), ""))
            sButtons = String.Format("<div style='display:inline-block; float:right; text-align:right; margin:4px 0px; text-align:center' id='divPipeButtons'>{0}</div>", sButtons)
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
                If Not SkipAlternative(alt, CurrentFiltersList) Then
                    Dim Rank As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_ALTERNATIVE_USER_RANK_ID, alt.NodeGuidID, UserRankAttr, PM.UserID))
                    Sum += (If(Rank > 0, Math.Abs(Idx - Rank), 0))
                    Idx += 1
                End If
            Next
            Score = Sum / (Idx - 1)
        End If
        'Return String.Format("<h6 style='max-width:600px; text-align:center;'>{0}</h6>", String.Format(ResString(If(Score < 2, "lblRiskResultScore0", "lblRiskResultScore2")), Score.ToString("F1")))
        'Return String.Format("<h6 class='error'>{0}: {1}</h6><div class='note' style='max-width:600px; margin:0px auto 1em auto; text-align:center;'>{2}</div>", ResString("lblRiskResultScore"), Score.ToString("F1"), ResString(If(Score < 2, "lblRiskResultScore0", "lblRiskResultScore2")))
        Return String.Format("<h6 class='error'>{0}: {1}</h6><div class='note' style='max-width:600px; margin:0px auto 1em auto; text-align:center;'>{2}</div>", ResString("lblRiskResultScore"), Score.ToString("F1"), ResString(If(Score = 2, "lblRiskResultScoreAverage", If(Score < 2, "lblRiskResultScore0", "lblRiskResultScore2"))))
    End Function
    ' D6702 ==

#Region "Risk Map (Risk Plot)"

    'Public ReadOnly DefaultRh As Double = 15
    'Public ReadOnly DefaultRl As Double = 2

    'Public _Rh As Double = DefaultRh
    'Public _Rl As Double = DefaultRl

    'Public ReadOnly Property DefaultRedBrush As String
    '    Get
    '        If PM.PipeParameters.ProjectType = ProjectType.ptOpportunities Then Return "#a0ff6a" '"#ffffff" 'Green
    '        Return "#FF7D7B" '"#ff6347" 'Red
    '    End Get
    'End Property

    'Public ReadOnly Property DefaultWhiteBrush As String
    '    Get
    '        Return "#FFEDCE" '"#ffff00" 'Yellow
    '    End Get
    'End Property

    'Public ReadOnly Property DefaultGreenBrush As String
    '    Get
    '        If PM.PipeParameters.ProjectType = ProjectType.ptOpportunities Then Return "#FF7D7B" 'Red
    '        Return "#a0ff6a" '"#E1ECC0" '"#00Ff7f" 'Green
    '    End Get
    'End Property

    'Public _redBrush As String = "" 'Red
    'Public _whiteBrush As String = "" 'Yellow
    'Public _greenBrush As String = "" 'Green

    Public _grayBrush As String = "#fcfcee" 'alt with no sources

    Private mAttributesList As List(Of clsAttribute)
    Public ReadOnly Property AttributesList As List(Of clsAttribute)
        Get
            If mAttributesList Is Nothing Then
                mAttributesList = New List(Of clsAttribute)
                InitAttributesList()
            End If
            Return mAttributesList
        End Get
    End Property

    Private gridStep As Integer = 100

    Public Property RiskMapSwitchAxes As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SWITCH_AXES_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SWITCH_AXES_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    ''' <summary>
    ''' 0 - hide regions, 1 - show exact regions, 2 - heat map
    ''' </summary>
    ''' <returns></returns>
    Public Property RiskMapShowRegions As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SHOW_REGIONS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            ' D6664 ===
            If IsWidget Then    ' avoid saving when just a user and a pipe step
                PM.Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_SHOW_REGIONS_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SHOW_REGIONS_ID, AttributeValueTypes.avtLong, value)
            End If
            ' D6664 ==
        End Set
    End Property

    Public Property RiskMapEventCloud As Boolean
        Get
            Return False
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_EVENT_CLOUD_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_EVENT_CLOUD_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property RiskMapShowLabels As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SHOW_LABELS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SHOW_LABELS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property RiskMapJitterDatapoints As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_JITTER_DATAPOINTS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_JITTER_DATAPOINTS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property RiskMapBubbleSizeWithRisk As Boolean ' show bubble size proportional to the risk
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_BUBBLE_SIZE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_BUBBLE_SIZE_ID, AttributeValueTypes.avtBoolean, value, "", "", IsWidget)
        End Set
    End Property

    Public Property RiskMapBubbleSize As Integer ' bubble size from the dropdown
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_BUBBLE_SIZE_VALUE_ID, UNDEFINED_USER_ID))
            If retVal <= 0 Then retVal = CInt(PM.Attributes.GetAttributeByID(ATTRIBUTE_RISK_MAP_BUBBLE_SIZE_VALUE_ID).DefaultValue)
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_BUBBLE_SIZE_VALUE_ID, AttributeValueTypes.avtLong, value, "", "", IsWidget)
        End Set
    End Property

    Public Property RiskMapReverseZoomValueTop As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_TOP_ID, UNDEFINED_USER_ID))
            If retVal < 0 Then retVal = 0
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_TOP_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property RiskMapReverseZoomValueLeft As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_LEFT_ID, UNDEFINED_USER_ID))
            If retVal < 0 Then retVal = 0
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_LEFT_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property RiskMapReverseZoomValueRight As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_RIGHT_ID, UNDEFINED_USER_ID))
            If retVal < 0 Then retVal = 0
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_RIGHT_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property RiskMapReverseZoomValueBottom As Integer
        Get
            Dim retVal As Integer = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_BOTTOM_ID, UNDEFINED_USER_ID))
            If retVal < 0 Then retVal = 0
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_REVERSE_ZOOM_BOTTOM_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Public Property RiskMapShowLegends As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SHOW_LEGENDS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SHOW_LEGENDS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property AxisXTitle As String
        Get
            Dim tValue As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_XAXIS_TITLE_ID, UNDEFINED_USER_ID))
            If String.IsNullOrEmpty(tValue) Then tValue = ParseString("%%Likelihood%%")
            Return JS_SafeString(tValue)
        End Get
        Set(value As String)
            With PM
                .Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_XAXIS_TITLE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, Guid.Empty, Guid.Empty)
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End Set
    End Property

    Public Property AxisYTitle As String
        Get
            Dim tValue As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_YAXIS_TITLE_ID, UNDEFINED_USER_ID))
            If String.IsNullOrEmpty(tValue) Then tValue = ParseString("%%Impact%%")
            Return JS_SafeString(tValue)
        End Get
        Set(value As String)
            With PM
                .Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_YAXIS_TITLE_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtString, value, Guid.Empty, Guid.Empty)
                .Attributes.WriteAttributeValues(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID, UNDEFINED_USER_ID)
            End With
        End Set
    End Property

    Public Property RiskMapLegendSortDirection As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SORT_LEGEND_DIR_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            ' D6664 ===
            If IsWidget Then
                PM.Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_SORT_LEGEND_DIR_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SORT_LEGEND_DIR_ID, AttributeValueTypes.avtLong, value)
            End If
            ' D6664 ==
        End Set
    End Property

    Public Property RiskMapLegendSortColumn As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SORT_LEGEND_COL_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            ' D6664 ===
            If IsWidget Then
                PM.Attributes.SetAttributeValue(ATTRIBUTE_RISK_MAP_SORT_LEGEND_COL_ID, UNDEFINED_USER_ID, AttributeValueTypes.avtLong, value, Guid.Empty, Guid.Empty)
            Else
                WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SORT_LEGEND_COL_ID, AttributeValueTypes.avtLong, value)
            End If
            ' D6664 ==
        End Set
    End Property

    Private Function GetStandardColors() As ColorPickerItemCollection
        Dim retVal As ColorPickerItemCollection = New ColorPickerItemCollection
        For Each item As String In GetPalette(3)
            Dim cpi As ColorPickerItem = New ColorPickerItem(ColorTranslator.FromHtml(item))
            retVal.Add(cpi)
        Next
        Return retVal
    End Function

    Private Function f(i As Double, a As Double, maxY As Double) As Double
        If i <= 0 Then Return maxY * 5
        Dim retVal = (a / i) * 100
        If retVal > maxY * 5 Then Return maxY * 5
        Return retVal
    End Function

    Public Function GetRiskFunctionData(level As Integer, arrayOnly As Boolean, minX As Double, minY As Double, maxX As Double, maxY As Double) As String
        Dim retVal As String = "[0,0]"
        If IsRiskMapPage AndAlso RiskMapShowRegions > 0 Then
            retVal = ""
            Dim a As Double = _Rl * 100
            If level = 1 Then a = _Rh * 100
            Select Case level
                Case 0, 1 'low level 'med level
                    Dim increment As Double = (maxX - minX) / gridStep
                    Dim mValue As Double = minX
                    While mValue < maxX
                        retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1}]", JS_SafeNumber(Math.Round(mValue, 3)), JS_SafeNumber(Math.Round(f(mValue, a, maxY), 3)))
                        mValue += increment
                    End While
                    mValue = maxX
                    retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1}]", JS_SafeNumber(mValue), JS_SafeNumber(Math.Round(f(mValue, a, maxY), 3)))
                Case 2 'top level
                    Dim mValue As Double = (_Rh * 100 / maxY) * 100
                    If mValue < minX Then mValue = minX
                    If mValue > maxX Then mValue = maxX
                    retVal = String.Format("[{2},  {1}], [{0},  {1}]", JS_SafeNumber(maxX), JS_SafeNumber(maxY * 5), JS_SafeNumber(minX))
            End Select
        End If
        Return String.Format(CStr(IIf(arrayOnly, "", "var l{0} = ")) + "[{1}]", level, retVal)
    End Function

    Private Class eventCloudDataItem
        Public id As Guid
        Public name As String
        Public index As String
        Public l As Double
        Public i As Double
        Public r As Double
        Public i_s As Double
        Public r_s As Double
        Public color As String
    End Class

    Private Function GetRiskPlotSeries(ByRef minX As Double, ByRef minY As Double, ByRef maxX As Double, ByRef maxY As Double, riskData As RiskDataWRTNodeDataContract, bwtData As clsBowTieDataContract2, VisibleAlternatives As List(Of clsNode), ByRef tEventCloudItems As List(Of eventCloudDataItem), id As Integer, email As String, withControls As Boolean, ByRef tColorLegendVisible As Boolean, ByRef tShapeLegendVisible As Boolean) As String
        Dim retVal As String = ""

        Dim tSelectedCategoryID As Guid = SelectedCategoryID
        Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
        If tSelectedCategoryID.Equals(Guid.Empty) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = AttributesList(0).ID

        Dim tSelectedShapeCategoryID As Guid = SelectedShapeCategoryID
        Dim tSelectedShapeCategoryEnum As clsAttributeEnumeration = Nothing
        If tSelectedShapeCategoryID.Equals(Guid.Empty) AndAlso AttributesList.Count > 0 Then tSelectedShapeCategoryID = AttributesList(0).ID

        Dim tSelectedAttr As clsAttribute = Nothing
        Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String)
        Dim tSelectedCategoryMultiNames As List(Of String) = New List(Of String)

        For Each attr In AttributesList
            If attr.ID = tSelectedCategoryID Then tSelectedAttr = attr
        Next
        If tSelectedAttr IsNot Nothing AndAlso Not tSelectedAttr.EnumID.Equals(Guid.Empty) Then tSelectedCategoryEnum = PM.Attributes.GetEnumByID(tSelectedAttr.EnumID)

        Dim tSelectedShapeAttr As clsAttribute = Nothing
        Dim tSelectedShapeCategoryMultiValues As List(Of String) = New List(Of String)
        Dim tSelectedShapeCategoryMultiNames As List(Of String) = New List(Of String)

        For Each attr In AttributesList
            If attr.ID = tSelectedShapeCategoryID Then tSelectedShapeAttr = attr
        Next
        If tSelectedShapeAttr IsNot Nothing AndAlso Not tSelectedShapeAttr.EnumID.Equals(Guid.Empty) Then tSelectedShapeCategoryEnum = PM.Attributes.GetEnumByID(tSelectedShapeAttr.EnumID)

        If riskData IsNot Nothing AndAlso riskData.AlternativesData IsNot Nothing AndAlso VisibleAlternatives IsNot Nothing Then
            InitAlternatives(riskData.AlternativesData, AlternativesData, Nothing)
            'Dim AltIndex As Integer = 1
            Dim SortedAlternatives As New List(Of clsNode)
            SortedAlternatives.AddRange(VisibleAlternatives)
            Dim sWithControls As String = ParseString(" (With %%controls%%)")

            'Dim simData As List(Of clsSimulationData) = Nothing

            'If ShowResultsMode <> ShowResultsModes.rmComputed Then
            '    Dim tSourcesType As Integer = 0 ' Independent
            '    Dim dMaxValue As Double = 0
            '    Dim tSimulationWRTNodeGuid As Guid = CType(IIf(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID, SelectedHierarchyNodeID), Guid)
            '    'SimulationEventsDataForAllUsers.Add(id, New List(Of clsEventData))
            '    'For each alt In SortedAlternatives 
            '    '    If alt.Enabled Then 
            '    '        SimulationEventsDataForAllUsers(id).Add(New clsEventData With {.ID = alt.NodeID, .GuID = alt.NodeGuidID, .Name = alt.NodeName, .Priorities = New clsEventPriorities With {.Likelihood = tUserValueWithoutControls.LikelihoodValue, .Impact = tUserValueWithoutControls.ImpactValue, .Risk = tUserValueWithoutControls.RiskValue, .CostImpact = tUserValueWithoutControls.DollarValueImpact, .CostRisk = tUserValueWithoutControls.DollarValueRisk }, .PrioritiesWithControls = New clsEventPriorities With {.Likelihood = tUserValue.LikelihoodValue, .Impact = tUserValue.ImpactValue, .Risk = tUserValue.RiskValue, .CostImpact = tUserValue.DollarValueImpact, .CostRisk = tUserValue.DollarValueRisk}})
            '    '    End If
            '    'Next
            '    'simData = LEC.SimulateLossExceedance(id, email, SimulationEventsDataForAllUsers.Values.First, clsLEC._SIMULATE_SOURCES, False, CType(IIf(withControls, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode), dMaxValue, New Random(RandSeed), True, False, tSourcesType, AttributesList, Guid.Empty, False, tSimulationWRTNodeGuid, PRJ.isOpportunityModel)
            '    PM.RiskSimulations.SimulateCommon(id, CType(IIf(withControls, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode), SelectedHierarchyNode, NormalizedLkhd <> SynthesisMode.smAbsolute)
            'End If            

            Dim tRiskMapBubbleSizeWithRisk As Boolean = RiskMapBubbleSizeWithRisk
            Dim tShapeBubblesByCategory As Boolean = ShapeBubblesByCategory
            Dim tColorCodingByCategory As Boolean = ColorCodingByCategory

            Dim OverlappingDelta As Double = (maxX - minX) / 5000

            Dim SelectedNodeGuid As Guid = New Guid(RadTreeHierarchy.SelectedNode.Value)
            Dim rand As New Random(DateTime.Now.Millisecond)

            Dim wrtNodeL As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(SelectedNodeGuid)
            If wrtNodeL Is Nothing Then wrtNodeL = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)

            Dim wrtNodeI As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(SelectedNodeGuid)
            If wrtNodeI Is Nothing Then wrtNodeI = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0)

            If PM.CalculationsManager.UseSimulatedValues Then
                Dim wrtObjective As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(SelectedNodeGuid)
                If wrtObjective Is Nothing Then
                    wrtObjective = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(SelectedNodeGuid)
                End If
                PM.RiskSimulations.SimulateCommon(bwtData.UserID, If(withControls, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), wrtObjective)
            End If

            ' Calculate
            If PM.Edges.Edges IsNot Nothing Then
                Dim wrtCalcNode As clsNode = If(wrtNodeL Is Nothing, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0), wrtNodeL)
                PM.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetDefaultCombinedGroup), wrtCalcNode, ECHierarchyID.hidLikelihood)
            End If

            For Each tAlt As clsNode In VisibleAlternatives
                If IsWidget AndAlso tAlt.DisabledForUser(PM.User.UserID) Then Continue For
                For Each data As clsRiskData In AlternativesData
                    If data.ID.Equals(tAlt.NodeGuidID) Then

                        Dim tEventData As clsBowTieData2 = Nothing
                        For Each tEvent In bwtData.BowTieData
                            If tEvent.Key.Equals(tAlt.NodeGuidID) Then tEventData = tEvent.Value
                        Next

                        If tAlt IsNot Nothing AndAlso tEventData IsNot Nothing AndAlso Not PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Contains(tAlt) Then 'A1408
                            data.LikelihoodValue = 0
                            data.ImpactValue = 0

                            If PM.CalculationsManager.UseSimulatedValues Then
                                data.LikelihoodValue = tAlt.SimulatedAltLikelihood
                                data.ImpactValue = tAlt.SimulatedAltImpact
                            Else
                                For Each lk As clsBowTiePriority In tEventData.LikelihoodValues
                                    If PM.Hierarchy(ECHierarchyID.hidLikelihood).GetRespectiveTerminalNodes(wrtNodeL).Where(Function(node) node.NodeGuidID = lk.CovObjID).Count > 0 Then
                                        data.LikelihoodValue += lk.MultipliedValueAbsolute
                                    End If
                                Next

                                If PM.Edges.Edges IsNot Nothing Then
                                    For Each kvp As KeyValuePair(Of Guid, List(Of Edge)) In PM.Edges.Edges
                                        For Each ToAlt As Edge In kvp.Value
                                            If ToAlt.ToNode.NodeGuidID = tAlt.NodeGuidID Then
                                                Dim FromAlt As clsNode = PM.ActiveAlternatives.GetNodeByID(kvp.Key)
                                                If FromAlt IsNot Nothing AndAlso ToAlt.ToNode IsNot Nothing Then
                                                    Dim nonpwData As clsNonPairwiseMeasureData = FromAlt.EventsJudgments.GetJudgement(ToAlt.ToNode.NodeID, FromAlt.NodeID, PM.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID)
                                                    Dim value As Double = 0
                                                    If nonpwData IsNot Nothing Then
                                                        value = nonpwData.SingleValue
                                                    End If
                                                    data.LikelihoodValue += FromAlt.UnnormalizedPriority * value
                                                End If
                                            End If
                                        Next
                                    Next
                                End If

                                For Each lk As clsBowTiePriority In tEventData.ImpactValues
                                    If PM.Hierarchy(ECHierarchyID.hidImpact).GetRespectiveTerminalNodes(wrtNodeI).Where(Function(node) node.NodeGuidID = lk.CovObjID).Count > 0 Then
                                        data.ImpactValue += lk.MultipliedValueAbsolute
                                    End If
                                Next
                            End If                                                        
                        Else
                            If PM.CalculationsManager.UseSimulatedValues Then
                                data.LikelihoodValue = tAlt.SimulatedAltLikelihood
                                data.ImpactValue = tAlt.SimulatedAltImpact
                                data.RiskValue = tAlt.SimulatedAltLikelihood * tAlt.SimulatedAltImpact
                            End If
                        End If

                        If Not PM.CalculationsManager.UseSimulatedValues Then
                            data.RiskValue = data.LikelihoodValue * data.ImpactValue
                        End If
                    Else
                    End If
                Next
            Next

            ' Sort
            Select Case RiskMapLegendSortColumn
                Case 0
                    'No.
                    If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.IndexID Then
                        'If RiskMapLegendSortDirection = SortDirection.Descending Then SortedAlternatives.Reverse()
                        If RiskMapLegendSortDirection = SortDirection.Descending Then SortedAlternatives = VisibleAlternatives.OrderByDescending(Function(x) x.iIndex).ToList Else SortedAlternatives = VisibleAlternatives.OrderBy(Function(x) x.iIndex).ToList
                    End If
                    If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.UniqueID Then
                        If RiskMapLegendSortDirection = SortDirection.Descending Then SortedAlternatives = VisibleAlternatives.OrderByDescending(Function(x) x.NodeID).ToList Else SortedAlternatives = VisibleAlternatives.OrderBy(Function(x) x.NodeID).ToList
                    End If
                    If PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then
                        If RiskMapLegendSortDirection = SortDirection.Descending Then SortedAlternatives = VisibleAlternatives.OrderByDescending(Function(x) x.RiskValue).ToList Else SortedAlternatives = VisibleAlternatives.OrderBy(Function(x) x.RiskValue).ToList
                    End If
                Case 1
                    'Name
                    If RiskMapLegendSortDirection = SortDirection.Descending Then SortedAlternatives = VisibleAlternatives.OrderByDescending(Function(x) x.NodeName).ToList Else SortedAlternatives = VisibleAlternatives.OrderBy(Function(x) x.NodeName).ToList
                Case 2, 3, 4
                    'Likelihood, Impact, Risk
                    For Each alt In VisibleAlternatives
                        For Each data As clsRiskData In AlternativesData
                            If data.ID.Equals(alt.NodeGuidID) Then
                                Select Case RiskMapLegendSortColumn
                                    Case 2
                                        alt.Tag = data.LikelihoodValue
                                    Case 3
                                        alt.Tag = data.ImpactValue
                                    Case 4
                                        alt.Tag = data.RiskValue
                                End Select

                            End If
                        Next
                    Next
                    If RiskMapLegendSortDirection = SortDirection.Descending Then SortedAlternatives = VisibleAlternatives.OrderByDescending(Function(x) CDbl(x.Tag)).ToList Else SortedAlternatives = VisibleAlternatives.OrderBy(Function(x) CDbl(x.Tag)).ToList
            End Select

            For Each tAlt As clsNode In SortedAlternatives
                If IsWidget AndAlso tAlt.DisabledForUser(PM.User.UserID) Then Continue For
                For Each data As clsRiskData In AlternativesData
                    If data.ID.Equals(tAlt.NodeGuidID) Then
                                                            
                        Dim dLikelihood As Double = data.LikelihoodValue * 100
                        Dim dImpact As Double = data.ImpactValue * 100

                        Dim plotLikelihood As Double = dLikelihood
                        Dim plotImpact As Double = Math.Abs(dImpact)

                        If RiskMapJitterDatapoints Then
                            For i As Integer = 0 To AlternativesData.IndexOf(data) - 1
                                Dim tmpAlt As clsRiskData = AlternativesData(i)
                                If tmpAlt.LikelihoodValue > data.LikelihoodValue - OverlappingDelta AndAlso tmpAlt.LikelihoodValue < data.LikelihoodValue + OverlappingDelta AndAlso tmpAlt.ImpactValue > data.ImpactValue - OverlappingDelta AndAlso tmpAlt.ImpactValue < data.ImpactValue + OverlappingDelta Then
                                    plotLikelihood = Jitter(rand, dLikelihood)
                                    plotImpact = Jitter(rand, dImpact)
                                End If
                            Next
                        End If

                        Dim likelihood As String = JS_SafeNumber(Double2String(dLikelihood, PM.Parameters.DecimalDigits))
                        Dim impact As String = JS_SafeNumber(Double2String(dImpact, PM.Parameters.DecimalDigits))

                        Dim sPlotLikelihood As String = JS_SafeNumber(Double2String(plotLikelihood, PM.Parameters.DecimalDigits))
                        Dim sPlotImpact As String = JS_SafeNumber(Double2String(Math.Abs(plotImpact), PM.Parameters.DecimalDigits))
                       
                        Dim risk As String = JS_SafeNumber(Double2String(Math.Sqrt(Math.Abs(data.riskValue) / Math.PI) * 100 * (RiskMapBubbleSize / 7), 6))

                        If Not tRiskMapBubbleSizeWithRisk OrElse tShapeBubblesByCategory Then risk = "100"
                        If PM.Parameters.Riskion_RiskPlotManualZoomParams = "" Then
                            If RiskMapSwitchAxes Then
                                'max
                                If dLikelihood > maxY Then maxY = dLikelihood
                                If dImpact > maxX Then maxX = dImpact
                                'min
                                If dLikelihood < minY Then minY = dLikelihood
                                If dImpact < minX Then minX = dImpact
                            Else
                                'max
                                If dLikelihood > maxX Then maxX = dLikelihood
                                If dImpact > maxY Then maxY = dImpact
                                'min
                                If dLikelihood < minX Then minX = dLikelihood
                                If dImpact < minY Then minY = dImpact
                            End If
                        End If
                        'A0888 ===
                        Dim tColor As String = ""
                        Dim tColorCategory As String = "0"
                        If tColorCodingByCategory AndAlso tSelectedAttr IsNot Nothing AndAlso AttributesList.Count > 0 Then
                            tColorLegendVisible = True
                            tColor = AlternativeUniformColor
                            Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedCategoryID, tAlt.NodeGuidID)
                            If attrVal IsNot Nothing AndAlso tSelectedCategoryEnum IsNot Nothing AndAlso tSelectedCategoryEnum.Items.Count > 0 Then
                                Select Case tSelectedAttr.ValueType
                                    Case AttributeValueTypes.avtEnumeration
                                        Dim item As clsAttributeEnumerationItem = tSelectedCategoryEnum.GetItemByID(CType(attrVal, Guid))
                                        If item IsNot Nothing Then
                                            Dim index As Integer = tSelectedCategoryEnum.Items.IndexOf(item)
                                            If index >= 0 Then
                                                tColorCategory = (index + 1).ToString
                                                tColor = GetPaletteColor(CurrentPaletteID(PM), index)
                                            End If
                                        End If
                                    Case AttributeValueTypes.avtEnumerationMulti
                                        Dim sVal As String = CStr(attrVal)
                                        If Not String.IsNullOrEmpty(sVal) Then
                                            If Not tSelectedCategoryMultiValues.Contains(sVal) Then
                                                tSelectedCategoryMultiValues.Add(sVal)
                                                Dim sName As String = ""
                                                For Each item As clsAttributeEnumerationItem In tSelectedCategoryEnum.Items
                                                    If sVal.Contains(item.ID.ToString) Then
                                                        sName += CStr(IIf(sName = "", "", " & ")) + item.Value
                                                    End If
                                                Next
                                                tSelectedCategoryMultiNames.Add(sName)
                                            End If
                                            Dim index As Integer = tSelectedCategoryMultiValues.IndexOf(sVal)
                                            tColorCategory = (index + 1).ToString
                                            tColor = GetPaletteColor(CurrentPaletteID(PM), index)
                                        End If
                                End Select
                            End If
                        Else
                            tColor = GetPaletteColor(CurrentPaletteID(PM), AlternativesData.IndexOf(data), True)
                        End If 'A0888 ==
                        'A0942 ===
                        Dim tShape As Integer = -1 ' default shape (bubble)
                        Dim tShapeCategory As String = "0"
                        If tShapeBubblesByCategory AndAlso tSelectedShapeAttr IsNot Nothing AndAlso AttributesList.Count > 0 Then
                            tShapeLegendVisible = True
                            tShape = 0 ' undefined attribute value
                            Dim attrVal As Object = PM.Attributes.GetAttributeValue(tSelectedShapeCategoryID, tAlt.NodeGuidID)
                            If attrVal IsNot Nothing AndAlso tSelectedShapeCategoryEnum IsNot Nothing AndAlso tSelectedShapeCategoryEnum.Items.Count > 0 Then
                                Select Case tSelectedAttr.ValueType
                                    Case AttributeValueTypes.avtEnumeration
                                        Dim item As clsAttributeEnumerationItem = tSelectedShapeCategoryEnum.GetItemByID(CType(attrVal, Guid))
                                        If item IsNot Nothing Then
                                            Dim index As Integer = tSelectedShapeCategoryEnum.Items.IndexOf(item)
                                            If index >= 0 Then
                                                tShapeCategory = (index + 1).ToString
                                                tShape = (index Mod (MAX_SHAPES_COUNT - 1)) + 1
                                            End If
                                        End If
                                    Case AttributeValueTypes.avtEnumerationMulti
                                        Dim sVal As String = ""
                                        If TypeOf attrVal Is Guid Then sVal = CType(attrVal, Guid).ToString Else sVal = CStr(attrVal)
                                        If Not String.IsNullOrEmpty(sVal) Then
                                            If Not tSelectedShapeCategoryMultiValues.Contains(sVal) Then
                                                tSelectedShapeCategoryMultiValues.Add(sVal)
                                                Dim sName As String = ""
                                                For Each item As clsAttributeEnumerationItem In tSelectedShapeCategoryEnum.Items
                                                    If sVal.Contains(item.ID.ToString) Then
                                                        sName += CStr(IIf(sName = "", "", " & ")) + item.Value
                                                    End If
                                                Next
                                                tSelectedShapeCategoryMultiNames.Add(sName)
                                            End If
                                            Dim index As Integer = tSelectedShapeCategoryMultiValues.IndexOf(sVal)
                                            tShapeCategory = (index + 1).ToString
                                            tShape = (index Mod (MAX_SHAPES_COUNT - 1)) + 1
                                        End If
                                End Select
                            End If
                        End If
                        'A0942 ==
                        tEventCloudItems.Add(New eventCloudDataItem With {.id = tAlt.NodeGuidID, .name = tAlt.NodeName, .index = tAlt.Index, .l = data.LikelihoodValue, .i = data.ImpactValue, .r = data.riskValue, .color = tColor, .i_s = data.ImpactValue * DollarValueOfEnterprise, .r_s = data.RiskValue * DollarValueOfEnterprise})
                        retVal += CStr(IIf(retVal = "", "", ",")) + String.Format("[{0},{1},{2},{{guid:'{11}', event_type: ""{16}"", label:'{3}',color:'{6}',shape:{7}, actual_likelihood:'{12}', actual_impact:'{13}', actual_risk:{8}, dollar_impact:{14}, dollar_risk:{15},color_cat:{9}, shape_cat:{10}}},'{4}','{5}','{11}']", CStr(IIf(RiskMapSwitchAxes, sPlotImpact, sPlotLikelihood)), CStr(IIf(RiskMapSwitchAxes, sPlotLikelihood, sPlotImpact)), risk, JS_SafeString(tAlt.Index), JS_SafeString(tAlt.NodeName + If(withControls, sWithControls, "")), Double2String(Math.Abs(data.riskValue) * 100, PM.Parameters.DecimalDigits, True), tColor, tShape.ToString, JS_SafeNumber(Math.Abs(data.riskValue)), tColorCategory, tShapeCategory, If(withControls, "-", "") + tAlt.NodeGuidID.ToString, likelihood, impact, JS_SafeNumber(Math.Abs(data.ImpactValue) * DollarValueOfEnterprise), JS_SafeNumber(Math.Abs(data.RiskValue) * DollarValueOfEnterprise), CInt(tAlt.EventType))
                    End If
                Next
            Next
        End If

        Return retVal
    End Function

    Public Function GetRiskPlotData(ByRef minX As Double, ByRef minY As Double, ByRef maxX As Double, ByRef maxY As Double, ByRef ColorsLegend As String, ByRef ShapesLegend As String, UseControls As Boolean, ByRef eventCloudData As String) As String
        eventCloudData = "[]"
        If Not IsRiskMapPage Then Return "[]"
        Dim retVal As String = ""
        Dim tEventCloudItems As New List(Of eventCloudDataItem)
        Dim tDecimalDigits As Integer = PM.Parameters.DecimalDigits

        If IsRiskMapPage Then
            '_IsSOSG1 = False
            '_IsSOEG1 = False
            '_IsSOOG1 = False
            '_IsSOIG1 = False
            '_IsEG1 = False
            '_IsIG1 = False

            Dim SelectedNodeGuid As Guid = New Guid(RadTreeHierarchy.SelectedNode.Value)
            Dim SelectedWRTNode = PM.Hierarchy(HierarchyID).GetNodeByID(SelectedNodeGuid)
            Dim isForGoalLikelihood As Boolean = HierarchyID = ECHierarchyID.hidLikelihood AndAlso SelectedWRTNode.ParentNodeID = -1
            Dim tCurrentFiltersList = CurrentFiltersList
            Dim VisibleAlternatives As New List(Of clsNode)

            'LoadBowTieData(CanShowAllPriorities, SelectedNodeGuid, SelectedNodeGuid)

            'A0888 ===
            Dim tSelectedCategoryID As Guid = SelectedCategoryID
            Dim tSelectedCategoryEnum As clsAttributeEnumeration = Nothing
            If tSelectedCategoryID.Equals(Guid.Empty) AndAlso AttributesList.Count > 0 Then tSelectedCategoryID = AttributesList(0).ID

            Dim tSelectedShapeCategoryID As Guid = SelectedShapeCategoryID
            Dim tSelectedShapeCategoryEnum As clsAttributeEnumeration = Nothing
            If tSelectedShapeCategoryID.Equals(Guid.Empty) AndAlso AttributesList.Count > 0 Then tSelectedShapeCategoryID = AttributesList(0).ID

            Dim tSelectedAttr As clsAttribute = Nothing
            Dim tSelectedCategoryMultiValues As List(Of String) = New List(Of String)
            Dim tSelectedCategoryMultiNames As List(Of String) = New List(Of String)

            For Each attr In AttributesList
                If attr.ID = tSelectedCategoryID Then tSelectedAttr = attr
            Next
            If tSelectedAttr IsNot Nothing AndAlso Not tSelectedAttr.EnumID.Equals(Guid.Empty) Then tSelectedCategoryEnum = PM.Attributes.GetEnumByID(tSelectedAttr.EnumID)

            Dim tSelectedShapeAttr As clsAttribute = Nothing
            Dim tSelectedShapeCategoryMultiValues As List(Of String) = New List(Of String)
            Dim tSelectedShapeCategoryMultiNames As List(Of String) = New List(Of String)

            For Each attr In AttributesList
                If attr.ID = tSelectedShapeCategoryID Then tSelectedShapeAttr = attr
            Next
            If tSelectedShapeAttr IsNot Nothing AndAlso Not tSelectedShapeAttr.EnumID.Equals(Guid.Empty) Then tSelectedShapeCategoryEnum = PM.Attributes.GetEnumByID(tSelectedShapeAttr.EnumID)

            ColorsLegend = String.Format("['{0}','{1}']", AlternativeUniformColor, "No Category")
            Dim tColorLegendVisible As Boolean = False

            ShapesLegend = String.Format("['{0}','{1}']", 0, "No Category")
            Dim tShapeLegendVisible As Boolean = False
            'A0888 ==

            Dim tShowDollarValue As Boolean = ShowDollarValue
            'Dim tDollarValueOfEnterprise As Double = PM.DollarValueOfEnterprise

            With PM
                For Each alt As clsNode In OrderedEvents
                    If isForGoalLikelihood OrElse AnySubObjectiveContributes(SelectedWRTNode, alt.NodeID) Then
                        If Not SkipAlternative(alt, tCurrentFiltersList) Then
                            VisibleAlternatives.Add(alt)
                        End If
                    End If
                Next

                Dim id As Integer = COMBINED_USER_ID
                Dim email As String = ""
                Dim tSelectedUserOrGroupID As Integer = SelectedUserOrGroupID

                If ECCore.IsCombinedUserID(tSelectedUserOrGroupID) AndAlso PM.CombinedGroups.GetCombinedGroupByUserID(tSelectedUserOrGroupID) IsNot Nothing Then
                    id = tSelectedUserOrGroupID
                Else
                    For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                        Dim tAHPUser As clsUser = .GetUserByEMail(tAppUser.UserEmail)
                        If tAHPUser IsNot Nothing AndAlso tAHPUser.UserID = tSelectedUserOrGroupID Then
                            id = .GetUserByEMail(tAppUser.UserEmail).UserID
                            email = tAppUser.UserEmail
                            MapUserName = tAppUser.HTMLDisplayName 'A2003
                        End If
                    Next
                End If

                MapUserEmail = email    ' D6667

                Dim s1 As String = ""
                Dim s2 As String = ""

                Dim bwtDataWOControls As clsBowTieDataContract2 = Nothing
                'Dim riskDataWOControls As RiskDataWRTNodeDataContract = Nothing
                'if RiskPlotMode = 0 Or RiskPlotMode = 2 Or RiskPlotMode = 3 Then
                '    'riskDataWOControls = .CalculationsManager.GetRiskDataWRTNode(id, email, SelectedNodeGuid, ControlsUsageMode.DoNotUse)
                'End If
                bwtDataWOControls = LoadBowTieData(True, SelectedNodeGuid, SelectedNodeGuid, False)

                If Not RiskMapEventCloud Then s1 = GetRiskPlotSeries(minX, minY, maxX, maxY, bwtDataWOControls.AlternativesTableData, bwtDataWOControls, VisibleAlternatives, tEventCloudItems, id, email, False, tColorLegendVisible, tShapeLegendVisible)
                If RiskMapEventCloud And Not UseControls Then retVal = GetRiskPlotSeries(minX, minY, maxX, maxY, bwtDataWOControls.AlternativesTableData, bwtDataWOControls, VisibleAlternatives, tEventCloudItems, id, email, False, tColorLegendVisible, tShapeLegendVisible)

                Dim bwtDataWithControls As clsBowTieDataContract2 = Nothing
                'Dim riskDataWithControls As RiskDataWRTNodeDataContract = Nothing
                'if RiskPlotMode = 1 Or RiskPlotMode = 2 Or RiskPlotMode = 3 Then
                'riskDataWithControls = .CalculationsManager.GetRiskDataWRTNode(id, email, SelectedNodeGuid, ControlsUsageMode.UseOnlyActive)
                bwtDataWithControls = LoadBowTieData(True, SelectedNodeGuid, SelectedNodeGuid, True)
                'End If

                If Not RiskMapEventCloud Then s2 = GetRiskPlotSeries(minX, minY, maxX, maxY, bwtDataWithControls.AlternativesTableData, bwtDataWithControls, VisibleAlternatives, tEventCloudItems, id, email, True, tColorLegendVisible, tShapeLegendVisible)
                If RiskMapEventCloud And UseControls Then retVal = GetRiskPlotSeries(minX, minY, maxX, maxY, bwtDataWithControls.AlternativesTableData, bwtDataWithControls, VisibleAlternatives, tEventCloudItems, id, email, True, tColorLegendVisible, tShapeLegendVisible)

                If Not RiskMapEventCloud Then
                    Select Case RiskPlotMode
                        Case 0 'w/o controls
                            retVal = s1
                        Case 1 'with controls
                            retVal = s2
                        Case 2, 3 'split, combine
                            retVal = s1 + If(s1 = "" Or s2 = "", "", ",") + s2
                    End Select
                End If
            End With

            If tColorLegendVisible AndAlso tSelectedCategoryEnum IsNot Nothing Then
                Select Case tSelectedAttr.ValueType
                    Case AttributeValueTypes.avtEnumeration
                        For index As Integer = 0 To tSelectedCategoryEnum.Items.Count - 1
                            ColorsLegend += String.Format(",['{0}','{1}']", GetPaletteColor(CurrentPaletteID(PM), index), JS_SafeString(tSelectedCategoryEnum.Items(index).Value))
                        Next
                    Case AttributeValueTypes.avtEnumerationMulti
                        For index As Integer = 0 To tSelectedCategoryMultiNames.Count - 1
                            ColorsLegend += String.Format(",['{0}','{1}']", GetPaletteColor(CurrentPaletteID(PM), index), JS_SafeString(tSelectedCategoryMultiNames(index)))
                        Next
                End Select
            End If

            If tShapeLegendVisible Then
                Select Case tSelectedShapeAttr.ValueType
                    Case AttributeValueTypes.avtEnumeration
                        For index As Integer = 0 To tSelectedShapeCategoryEnum.Items.Count - 1
                            ShapesLegend += String.Format(",['{0}','{1}']", (index Mod (MAX_SHAPES_COUNT - 1)) + 1, JS_SafeString(tSelectedShapeCategoryEnum.Items(index).Value))
                        Next
                    Case AttributeValueTypes.avtEnumerationMulti
                        For index As Integer = 0 To tSelectedShapeCategoryMultiNames.Count - 1
                            ShapesLegend += String.Format(",['{0}','{1}']", (index Mod (MAX_SHAPES_COUNT - 1)) + 1, JS_SafeString(tSelectedShapeCategoryMultiNames(index)))
                        Next
                End Select
            End If
        End If

        Dim tEventCloudItemsSorted As IEnumerable(Of eventCloudDataItem) = tEventCloudItems.OrderByDescending(Function(e) e.l)
        eventCloudData = ""
        For Each e As eventCloudDataItem In tEventCloudItemsSorted
            eventCloudData += CStr(IIf(eventCloudData = "", "", ",")) + String.Format("({{ guid:'{0}', name:'{1}', index:'{2}', color: '{3}', l:{4}, i:{5}, r:{6}, i_s:{7}, r_s:{8} }})", e.id, JS_SafeString(e.name), e.index, e.color, JS_SafeNumber(Math.Round(e.l, tDecimalDigits + 2)), JS_SafeNumber(Math.Round(e.i, tDecimalDigits + 2)), JS_SafeNumber(Math.Round(e.r, tDecimalDigits + 2)), JS_SafeNumber(e.i_s), JS_SafeNumber(e.r_s))
        Next

        eventCloudData = String.Format("[{0}]", eventCloudData)

        Return String.Format("[{0}]", retVal)
    End Function

    ''' <summary>
    ''' Jitter a value from 0 to 1
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="jitterOffsetPrc">Percentage</param>
    ''' <returns></returns>
    Private Function Jitter(rand As Random, value As Double, Optional jitterOffsetPrc As Integer = 3) As Double
        Dim xRand As Double = CDbl(rand.Next(jitterOffsetPrc * 100) / 100)
        Dim yRand As Double = CDbl(rand.Next(jitterOffsetPrc * 100) / 100)

        If value - xRand < 0 Then
            value += xRand
        Else
            If value + xRand > 1 Then
                value -= xRand
            Else
                value += CDbl(IIf(rand.Next(2) > 0, xRand, -xRand))
            End If
        End If
        Return value
    End Function

    'Private Sub InitRegions()
    '    If PM.Regions IsNot Nothing Then
    '        With PM.Regions
    '            If .ColumnCount = 6 AndAlso .RowCount = 1 AndAlso .Cells IsNot Nothing AndAlso .Cells.Count = 6 Then
    '                _Rh = .Cells(1) / 100000 'Rh *100 000
    '                _Rl = .Cells(2) / 100000 'Rl *100 000
    '
    '                _redBrush = LongToBrush(.Cells(3)) 'Red
    '                _whiteBrush = LongToBrush(.Cells(4)) 'Yellow
    '                _greenBrush = LongToBrush(.Cells(5)) 'Green
    '            End If
    '        End With
    '    End If
    'End Sub

    Public Property ColorCodingByCategory As Boolean 'A0888
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_RESULTS_COLOR_BY_CATEGORY_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_RESULTS_COLOR_BY_CATEGORY_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property ShapeBubblesByCategory As Boolean 'A0942
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SHAPE_BY_CATEGORY_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SHAPE_BY_CATEGORY_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property SelectedCategoryID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SELECTED_COLOR_CATEGORY_ID, UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) Then retVal = New Guid(s)
            Return retVal
        End Get
        Set(value As Guid)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SELECTED_COLOR_CATEGORY_ID, AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Public Property SelectedShapeCategoryID As Guid
        Get
            Dim retVal As Guid = Guid.Empty
            Dim s As String = CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_MAP_SELECTED_SHAPE_CATEGORY_ID, UNDEFINED_USER_ID))
            If Not String.IsNullOrEmpty(s) Then retVal = New Guid(s)
            Return retVal
        End Get
        Set(value As Guid)
            WriteSetting(PRJ, ATTRIBUTE_RISK_MAP_SELECTED_SHAPE_CATEGORY_ID, AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    Public Function GetCategories(ComboBoxWidth As Integer) As String
        Dim sRes As String = ""
        Dim tSelectedCategoryID As Guid = SelectedCategoryID
        For Each tAttr As clsAttribute In AttributesList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", tAttr.ID, ShortString(App.GetAttributeName(tAttr), 40), IIf(tAttr.ID = tSelectedCategoryID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbCategories' style='width:{2}px; margin-bottom:-1px;' onchange='onSetCategory(this.value);' {1}>{0}</select>", sRes, IIf(AttributesList.Count = 0 OrElse Not ColorCodingByCategory, " disabled='disabled' ", ""), ComboBoxWidth)
    End Function

    Public Function GetCategoriesShapes(ComboBoxWidth As Integer) As String
        Dim sRes As String = ""
        Dim tSelectedShapeCategoryID As Guid = SelectedShapeCategoryID
        For Each tAttr As clsAttribute In AttributesList
            sRes += String.Format("<option value='{0}' {2}>{1}</option>", tAttr.ID, ShortString(App.GetAttributeName(tAttr), 40), IIf(tAttr.ID = tSelectedShapeCategoryID, " selected='selected' ", ""))
        Next
        Return String.Format("<select id='cbCategoriesShapes' style='width:{2}px; margin-bottom:-1px;' onchange='onSetCategoryShape(this.value);' {1}>{0}</select>", sRes, IIf(AttributesList.Count = 0 OrElse Not ShapeBubblesByCategory, " disabled='disabled' ", ""), ComboBoxWidth)
    End Function

    Private Sub InitAttributesList()
        PM.Attributes.ReadAttributes(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID)
        AttributesList.Clear()
        Dim HasAttributes As Boolean = PM.Attributes IsNot Nothing AndAlso PM.Attributes.AttributesList IsNot Nothing AndAlso PM.Attributes.AttributesList.Count > 0
        If HasAttributes Then
            PM.Attributes.ReadAttributeValues(AttributesStorageType.astStreamsDatabase, PM.StorageManager.ProjectLocation, PM.StorageManager.ProviderType, PM.StorageManager.ModelID, -1)
            For Each tAttr In PM.Attributes.GetAlternativesAttributes(True).Where(Function(attr) attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti)
                'If attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti Then
                AttributesList.Add(tAttr)
                'End If
            Next
        End If
    End Sub

    Protected Sub DivRiskMapData_PreRender(sender As Object, e As EventArgs) Handles DivRiskMapData.PreRender
        If Not IsRiskMapPage Then Exit Sub
        'InitRegions()
        Dim tZoomPlot As Boolean = ZoomPlot
        Dim tUseControlsReductions As Boolean = UseControlsReductions
        Dim minX As Double = CDbl(IIf(tZoomPlot, Double.MaxValue, 0))
        Dim minY As Double = CDbl(IIf(tZoomPlot, Double.MaxValue, 0))
        Dim maxX As Double = CDbl(IIf(tZoomPlot, Double.MinValue, 100))
        Dim maxY As Double = CDbl(IIf(tZoomPlot, Double.MinValue, 100))

        If PM.Parameters.Riskion_RiskPlotManualZoomParams <> "" Then
            Dim sParams As String() = PM.Parameters.Riskion_RiskPlotManualZoomParams.Split(CChar(","))
            If sParams.Count = 4 Then
                String2Double(sParams(0), minX)
                String2Double(sParams(1), maxX)
                String2Double(sParams(2), minY)
                String2Double(sParams(3), maxY)
            End If
        End If

        Dim sColorsLegend As String = ""
        Dim sShapesLegend As String = ""
        Dim sEventCloudData As String = ""
        Dim sPlotData As String = GetRiskPlotData(minX, minY, maxX, maxY, sColorsLegend, sShapesLegend, tUseControlsReductions, sEventCloudData)

        'If tZoomPlot Then
        Dim minX_wc As Double = CDbl(IIf(tZoomPlot, Double.MaxValue, 0))
        Dim minY_wc As Double = CDbl(IIf(tZoomPlot, Double.MaxValue, 0))
        Dim maxX_wc As Double = CDbl(IIf(tZoomPlot, Double.MinValue, 100))
        Dim maxY_wc As Double = CDbl(IIf(tZoomPlot, Double.MinValue, 100))

        If PM.Parameters.Riskion_RiskPlotManualZoomParams <> "" Then
            minX_wc = minX
            maxX_wc = maxX
            minY_wc = minY
            maxY_wc = maxY
        End If

        Dim tmpColorsLegend As String = ""
        Dim tmpShapesLegend As String = ""
        Dim tmpEventCloudData As String = ""
        Dim tmpPlotData As String = GetRiskPlotData(minX_wc, minY_wc, maxX_wc, maxY_wc, tmpColorsLegend, tmpShapesLegend, Not tUseControlsReductions, tmpEventCloudData)
        minX = Math.Min(minX, minX_wc)
        minY = Math.Min(minY, minY_wc)
        maxX = Math.Max(maxX, maxX_wc)
        maxY = Math.Max(maxY, maxY_wc)
        'End If

        sColorsLegend = String.Format("[{0}]", sColorsLegend)
        sShapesLegend = String.Format("[{0}]", sShapesLegend)

        If PM.Parameters.Riskion_RiskPlotManualZoomParams = "" Then
            Dim coef As Double = 0.1 '10%
            If tZoomPlot Then
                Dim orig_minX As Double = minX
                minX = Math.Floor(minX) - (maxX - minX) * coef
                maxX = Math.Floor(maxX + 1) + (maxX - orig_minX) * coef
                Dim orig_minY As Double = minY
                minY = Math.Floor(minY) - (maxY - minY) * coef
                maxY = Math.Floor(maxY + 1) + (maxY - orig_minY) * coef
            End If
            If maxX < 100 AndAlso Not tZoomPlot Then maxX = 100
            If maxY < 100 AndAlso Not tZoomPlot Then maxY = 100
            If minX > 0 AndAlso Not tZoomPlot Then minX = 0
            If minY > 0 AndAlso Not tZoomPlot Then minY = 0
            'If minX < 0 AndAlso tZoomPlot Then minX = 0 ' by EF request
            'If minY < 0 AndAlso tZoomPlot Then minY = 0 ' by EF request
            If minX > maxX Then
                minX = 0
                maxX = 100
            End If
            If minY > maxY Then
                minY = 0
                maxY = 100
            End If
        End If

        DivRiskMapData.InnerHtml = String.Format("[{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},'{11}',{12},{13}]", GetRiskFunctionData(0, True, minX, minY, maxX, maxY), GetRiskFunctionData(1, True, minX, minY, maxX, maxY), GetRiskFunctionData(2, True, minX, minY, maxX, maxY), sPlotData, JS_SafeNumber(maxX), JS_SafeNumber(maxY), JS_SafeNumber(minX), JS_SafeNumber(minY), sColorsLegend, sShapesLegend, sEventCloudData, If(UseControlsReductions, ParseString("(with %%Controls%%)"), ""), JS_SafeNumber(_Rh, _OPT_MaxDecimalPlaces), JS_SafeNumber(_Rl, _OPT_MaxDecimalPlaces))
    End Sub

#End Region

#Region "Bow-Tie"

    Public Const Eps As Double = 0.00001
    Public Const _COST_FORMAT As String = "f2"

    ReadOnly Property SESS_CAN_EDIT_TREATMENTS As String
        Get
            Return String.Format("RISK_CAN_EDIT_TREATMENTS_{0}_{1}", App.ProjectID.ToString, IsBowTiePage)
        End Get
    End Property

    ReadOnly Property SESS_TREATMENTS As String
        Get
            Return String.Format("RISK_TREATMENTS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    ReadOnly Property SESSION_SHOW_BOWTIE_RESULTS As String
        Get
            Return String.Format("RiskShowBowTieResults_{0}_{1}", App.ProjectID, IsBowTiePage)
        End Get
    End Property

    ReadOnly Property SESSION_BOWTIE_SORT_STATE As String
        Get
            Return String.Format("RiskBowTieSortState_{0}_{1}", App.ProjectID, IsBowTiePage)
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

    Public Property CanShowAllPriorities As Boolean ' Show priorities in Bow-Tie view
        Get
            Dim v As String = SessVar(SESSION_SHOW_BOWTIE_RESULTS)
            If String.IsNullOrEmpty(v) Then
                Return True
            End If
            Return v = "1"
        End Get
        Set(value As Boolean)
            SessVar(SESSION_SHOW_BOWTIE_RESULTS) = CStr(IIf(value, "1", "0"))
        End Set
    End Property

    Public Property CanShowUpperLevelNodes As Boolean ' Show upper-level nodes in Bow-Tie view
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_BOWTIE_SHOW_FULL_PATHS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_BOWTIE_SHOW_FULL_PATHS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property CanShowBowTieAttributes As Boolean ' Show Event Attributes UI below the event circle
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_BOWTIE_SHOW_EVENT_ATTR_UI_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_BOWTIE_SHOW_EVENT_ATTR_UI_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property CanShowBowTieBackground As Boolean ' Show background in Bow-Tie view
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_BOWTIE_SHOW_BACKGROUND_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_BOWTIE_SHOW_BACKGROUND_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    'Public Property CanUserEditControls As Boolean ' Show controls in Bow-Tie view
    '    Get
    '        If Not (CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES) Then Return False
    '        Dim v As String = SessVar(SESS_CAN_EDIT_TREATMENTS)
    '        If String.IsNullOrEmpty(v) Then
    '            Return True
    '        End If
    '        Return v = "1"
    '    End Get
    '    Set(value As Boolean)
    '        SessVar(SESS_CAN_EDIT_TREATMENTS) = CStr(IIf(value, "1", "0"))
    '    End Set
    'End Property

    Public ReadOnly Property LikelihoodGoalGuid As Guid
        Get
            Return PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID
        End Get
    End Property

    Public ReadOnly Property ImpactGoalGuid As Guid
        Get
            Return PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID
        End Get
    End Property

    Public Enum BowTieSortState
        sCausesNone = 0
        sCauses = 1
        sCausesLkl = 2
        sCausesToEvents = 3
        sConsequences = 4
        sConsequencesImp = 5
        sConsequencesToEvents = 6
        sConsequencesNone = 7
    End Enum

    Public Property BowTieSort As BowTieSortState()
        Get
            Dim defaultValue As BowTieSortState() = {BowTieSortState.sCausesNone, BowTieSortState.sConsequencesNone}
            Dim v As String = SessVar(SESSION_BOWTIE_SORT_STATE)
            If Not String.IsNullOrEmpty(v) Then
                Dim sValue As String() = v.Split(CChar(","))
                If sValue.Count = 2 Then
                    defaultValue(0) = CType(CInt(sValue(0)), BowTieSortState)
                    defaultValue(1) = CType(CInt(sValue(1)), BowTieSortState)
                End If
            End If
            Return defaultValue
        End Get
        Set(value As BowTieSortState())
            Dim sValue As String = CStr(CInt(value(0))) + "," + CStr(CInt(value(1)))
            SessVar(SESSION_BOWTIE_SORT_STATE) = sValue
        End Set
    End Property

    Private TreatmentsSortList As List(Of ControlSortAttribute)

    Private BowTieLoadedData As clsBowTieDataContract2
    Private AlternativesData As New List(Of clsRiskData)
    Private LikelihoodNodes As List(Of clsPriorityData) = Nothing
    Private ImpactNodes As List(Of clsPriorityData) = Nothing

    Public Function LoadBowTieData(isLoadAndShowPriorities As Boolean, forEvent As Guid, forObjective As Guid, useControls As Boolean) As clsBowTieDataContract2
        'forObjective = Nothing 
        'CanShowAllPriorities = isLoadAndShowPriorities

        Dim tSelectedUserOrGroupID As Integer = SelectedUserOrGroupID
        Dim id As Integer = COMBINED_USER_ID
        Dim email As String = ""
        If ECCore.IsCombinedUserID(tSelectedUserOrGroupID) AndAlso PM.CombinedGroups.GetCombinedGroupByUserID(tSelectedUserOrGroupID) IsNot Nothing Then
            id = tSelectedUserOrGroupID
        Else
            For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                Dim tAHPUser As clsUser = PM.GetUserByEMail(tAppUser.UserEmail)
                If tAHPUser IsNot Nothing AndAlso tAHPUser.UserID = tSelectedUserOrGroupID Then
                    id = tAHPUser.UserID
                    email = tAppUser.UserEmail
                    Exit For
                End If
            Next
        End If

        'PM.CalculationsManager.UseSimulatedValues = ShowResultsMode = ShowResultsModes.rmSimulated
        Dim retVal As clsBowTieDataContract2 = PM.CalculationsManager.GetBowTieData2(id, email, isLoadAndShowPriorities, CType(IIf(useControls, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), ControlsUsageMode), forObjective)

        'If BowTieLoadedData IsNot Nothing Then
        '    InitIndicators(BowTieLoadedData.AlternativesTableData)
        'End If

        'If CanUserEditControls Then
        '    Treatments = GetTreatments()
        '    TreatmentsSortList = GetTreatmentsSortAttributes()
        'End If

        Dim goalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)
        Dim wrtNode As clsNode = goalNode

        If forObjective <> Nothing Then
            wrtNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(forObjective)
            If wrtNode Is Nothing Then wrtNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(forObjective)
            If wrtNode Is Nothing Then wrtNode = goalNode
        End If

        InitAlternatives(retVal.AlternativesTableData.AlternativesData, AlternativesData, wrtNode)

        PM.CalculationsManager.UpdateAlternativeRankValues(PM.UserID)

        CopyNodes(LikelihoodNodes, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes)
        CopyNodes(ImpactNodes, PM.Hierarchy(ECHierarchyID.hidImpact).Nodes)

        Return retVal
    End Function

    Public Sub CopyNodes(ByRef nodes As List(Of clsPriorityData), ByRef Source As List(Of clsNode))
        If nodes Is Nothing Then nodes = New List(Of clsPriorityData)
        nodes.Clear()
        If Source IsNot Nothing Then
            If Source.Count > 0 Then
                Dim GoalID = Source(0).NodeID
                For Each node In Source
                    nodes.Add(New clsPriorityData(node.NodeGuidID, node.NodeName))
                Next
            End If
        End If
    End Sub

    Public Sub InitAlternatives(AlternativesData As List(Of AlternativeRiskDataDataContract), ByRef Alternatives As List(Of clsRiskData), WRTNode As clsNode)
        Alternatives.Clear()
        For Each alt As AlternativeRiskDataDataContract In AlternativesData
            Dim altData As New clsRiskData With {.ID = alt.AlternativeID, .LikelihoodValue = alt.LikelihoodValue, .ImpactValue = alt.ImpactValue, .RiskValue = alt.RiskValue, .OriginalImpactValue = alt.ImpactValue, .OriginalLikelihoodValue = alt.LikelihoodValue, .OriginalRiskValue = alt.RiskValue, .WRTGlobalPriorityLikelihood = alt.WRTGlobalPriorityLikelihood, .WRTGlobalPriorityImpact = alt.WRTGlobalPriorityImpact}
            Dim modelAlt = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(alt.intID)

            Dim wrtNodeL As clsNode = Nothing
            If WRTNode IsNot Nothing Then wrtNodeL = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(WRTNode.NodeGuidID)
            If wrtNodeL Is Nothing Then wrtNodeL = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)

            Dim wrtNodeI As clsNode = Nothing
            If WRTNode IsNot Nothing Then wrtNodeI = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(WRTNode.NodeGuidID)
            If wrtNodeI Is Nothing Then wrtNodeI = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0)

            altData.HasContributionsL = AnySubObjectiveContributes(wrtNodeL, modelAlt.NodeID)
            altData.HasContributionsI = AnySubObjectiveContributes(wrtNodeI, modelAlt.NodeID)

            Dim intAltID As Integer = 0
            If modelAlt IsNot Nothing Then
                altData.Name = modelAlt.NodeName
                intAltID = modelAlt.NodeID
                altData.intID = intAltID
            End If

            'altData.DollarValueImpact = alt.DollarValueImpact
            'altData.DollarValueRisk = alt.DollarValueRisk
            '
            ''A1313 ===
            'If NormalizedLocalLkhd <> LocalNormalizationType.ntUnnormalized Then 
            '    altData.DollarValueRisk = alt.DollarValueRiskRelativeB
            'End If
            ''A1313 ==

            Alternatives.Add(altData)
        Next
    End Sub

    Public Function GetTreatments() As List(Of clsControl)
        Dim res As New List(Of clsControl)
        With PM
            .Controls.ReadControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)

            For Each user As clsUser In .UsersList
                .StorageManager.Reader.LoadUserControlsPermissions(user.UserID)
            Next

            Dim LJT As DateTime
            .StorageManager.Reader.LoadUserJudgmentsControls(LJT)
            For Each control As clsControl In .Controls.Controls
                For Each assignment As clsControlAssignment In control.Assignments
                    assignment.Value = .Controls.GetCombinedEffectivenessValue(assignment.Judgments, control.ID, assignment.Value)
                Next
            Next

            For Each control As clsControl In .Controls.Controls
                If Not control.IsCostDefined Then control.Cost = 0
                res.Add(control)
            Next
        End With

        Return res
    End Function

    Public Function GetTreatmentsSortAttributes() As List(Of ControlSortAttribute)
        Dim res As List(Of ControlSortAttribute) = New List(Of ControlSortAttribute)
        With PM
            If .Attributes.GetAttributeByID(ATTRIBUTE_RISK_SORT_CONTROLS_ID) IsNot Nothing Then
                Dim tAllValues = .Attributes.GetNodesAttributesValues(ATTRIBUTE_RISK_SORT_CONTROLS_ID)
                For Each value In tAllValues
                    Dim sValue As String = ""
                    If Not String.IsNullOrEmpty(CStr(value.Value)) Then sValue = CStr(value.Value)
                    res.Add(New ControlSortAttribute With {.ObjectiveID = value.ObjectID, .EventID = value.AdditionalID, .SortString = sValue})
                Next
            End If
        End With
        Return res
    End Function

    Public Function GetCategoriesData(Optional arrayOnly As Boolean = False) As String
        Dim retVal As String = ""

        With PM
            If .Attributes IsNot Nothing Then
                Dim attr As clsAttribute = .Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
                If attr IsNot Nothing AndAlso Not attr.EnumID.Equals(Guid.Empty) Then
                    Dim cat_attr_vals As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                    If cat_attr_vals IsNot Nothing Then
                        For Each item As clsAttributeEnumerationItem In cat_attr_vals.Items
                            retVal += CStr(IIf(retVal <> "", ",", "")) + String.Format("['{0}','{1}']", item.ID, JS_SafeString(item.Value))
                        Next
                    End If
                End If
            End If
        End With

        Return CStr(IIf(arrayOnly, "", "var cat_data = ")) + String.Format("[{0}]", retVal) + CStr(IIf(arrayOnly, "", ";"))
    End Function

    Public Function GetEventAttrViewData(tAttr As clsAttribute, AltID As Guid) As String
        Return String.Format("['{0}','{1}','{2}']", tAttr.ID.ToString, JS_SafeString(ShortString(App.GetAttributeName(tAttr), 15)), JS_SafeString(ShortString(PM.Attributes.GetAttributeValueString(tAttr.ID, AltID), 30)))
    End Function

    Public Function GetBowTieData(SelectedAltGuid As Guid, forObjective As Guid) As String
        Dim altVal As String = ""
        Dim lkhVal As String = ""
        Dim impVal As String = ""

        Dim wrtObjective As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(forObjective)
        If wrtObjective Is Nothing Then
            wrtObjective = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(forObjective)
        End If
        Dim cAlt As clsNode = PM.AltsHierarchy(PM.ActiveAltsHierarchy).GetNodeByID(SelectedAltGuid)

        Dim ContributedAltsList As List(Of clsNode) = PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Where(Function(alt) AnySubObjectiveContributes(wrtObjective, alt.NodeID)).ToList

        If ContributedAltsList.Contains(cAlt) OrElse wrtObjective Is PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0) Then
            If IsBowTiePage AndAlso Not SelectedAltGuid.Equals(Guid.Empty) AndAlso Not SelectedAltGuid.Equals(ALTERNATIVES_HEADER_ITEM_ID) Then
                'InitRegions()
                BowTieLoadedData = LoadBowTieData(CanShowAllPriorities, SelectedAltGuid, forObjective, UseControlsReductions)

                Dim tShowDollarValue As Boolean = ShowDollarValue
                'Dim tDollarValueOfEnterprise As Double = PM.DollarValueOfEnterprise

                Dim alt As clsRiskData = Nothing
                For Each a In AlternativesData
                    If a.ID.Equals(SelectedAltGuid) Then alt = a
                Next

                Dim tEventData As clsBowTieData2 = Nothing
                For Each tEvent In BowTieLoadedData.BowTieData
                    If tEvent.Key.Equals(SelectedAltGuid) Then tEventData = tEvent.Value
                Next

                'A1406 ===
                Dim wrtNodeL As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(forObjective)
                If wrtNodeL Is Nothing Then wrtNodeL = PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0)

                Dim wrtNodeI As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(forObjective)
                If wrtNodeI Is Nothing Then wrtNodeI = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0)

                If alt IsNot Nothing AndAlso tEventData IsNot Nothing Then 'A1408
                    If Not PM.Hierarchy(ECHierarchyID.hidLikelihood).GetUncontributedAlternatives.Contains(cAlt) Then
                        alt.LikelihoodValue = 0
                        For Each lk As clsBowTiePriority In tEventData.LikelihoodValues
                            If PM.Hierarchy(ECHierarchyID.hidLikelihood).GetRespectiveTerminalNodes(wrtNodeL).Where(Function(node) node.NodeGuidID = lk.CovObjID).Count > 0 Then
                                alt.LikelihoodValue += lk.MultipliedValueAbsolute
                            End If
                        Next
                    End If

                    alt.ImpactValue = 0
                    For Each lk As clsBowTiePriority In tEventData.ImpactValues
                        If PM.Hierarchy(ECHierarchyID.hidImpact).GetRespectiveTerminalNodes(wrtNodeI).Where(Function(node) node.NodeGuidID = lk.CovObjID).Count > 0 Then
                            alt.ImpactValue += lk.MultipliedValueAbsolute
                        End If
                    Next

                    If PM.CalculationsManager.UseSimulatedValues Then
                        PM.RiskSimulations.SimulateCommon(BowTieLoadedData.UserID, If(UseControlsReductions, ControlsUsageMode.UseOnlyActive, ControlsUsageMode.DoNotUse), wrtObjective)

                        alt.LikelihoodValue = cAlt.SimulatedAltLikelihood
                        alt.ImpactValue = cAlt.SimulatedAltImpact
                        alt.RiskValue = cAlt.SimulatedAltLikelihood * cAlt.SimulatedAltImpact
                    Else
                        alt.RiskValue = alt.LikelihoodValue * alt.ImpactValue
                    End If

                End If
                'A1406 ==

                Dim wrtNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(forObjective)
                If wrtNode Is PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0) Then wrtNode = Nothing

                If PM.Edges.Edges IsNot Nothing Then
                    Dim wrtCalcNode As clsNode = If(wrtNode Is Nothing, PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0), wrtNode)
                    PM.CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, PM.CombinedGroups.GetDefaultCombinedGroup), wrtCalcNode, ECHierarchyID.hidLikelihood)
                    For Each kvp As KeyValuePair(Of Guid, List(Of Edge)) In PM.Edges.Edges
                        For Each ToAlt As Edge In kvp.Value
                            If ToAlt.ToNode.NodeGuidID = SelectedAltGuid Then
                                Dim FromAlt As clsNode = PM.ActiveAlternatives.GetNodeByID(kvp.Key)
                                If FromAlt IsNot Nothing AndAlso ToAlt.ToNode IsNot Nothing Then
                                    Dim nonpwData As clsNonPairwiseMeasureData = FromAlt.EventsJudgments.GetJudgement(ToAlt.ToNode.NodeID, FromAlt.NodeID, PM.CombinedGroups.GetDefaultCombinedGroup.CombinedUserID)
                                    Dim value As Double = 0
                                    If nonpwData IsNot Nothing Then
                                        value = nonpwData.SingleValue
                                    End If
                                    alt.LikelihoodValue += If(Not PM.CalculationsManager.UseSimulatedValues, FromAlt.UnnormalizedPriority, FromAlt.SimulatedAltLikelihood) * value
                                    lkhVal += CStr(IIf(lkhVal = "", "", ",")) + String.Format("['{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},{8},{9},{10}]", FromAlt.NodeGuidID, JS_SafeString(FromAlt.NodeName), Double2String(If(Not PM.CalculationsManager.UseSimulatedValues, FromAlt.UnnormalizedPriority * 100, FromAlt.SimulatedAltLikelihood * 100), PM.Parameters.DecimalDigits, True), Double2String(value * 100, PM.Parameters.DecimalDigits, True), Double2String(FromAlt.UnnormalizedPriority * value * 100, PM.Parameters.DecimalDigits, True), JS_SafeString(""), CInt(RiskNodeType.ntUncertainty), 1, IIf(Not String.IsNullOrEmpty(FromAlt.InfoDoc), 1, 0), "[]", "{}") ' {7} - edge event (source event)
                                End If
                            End If
                        Next
                    Next

                    If PM.CalculationsManager.UseSimulatedValues Then
                        alt.LikelihoodValue = cAlt.SimulatedAltLikelihood
                    End If
                End If

                Dim alt_color As String = _grayBrush
                If alt IsNot Nothing Then
                    alt_color = PM.Parameters.Riskion_Regions_Rl_Color '_greenBrush
                    'Dim risk_value As Double = alt.RiskValue * 100
                    If alt.RiskValue >= _Rl AndAlso alt.RiskValue <= _Rh Then alt_color = PM.Parameters.Riskion_Regions_Rm_Color
                    If alt.RiskValue > _Rh Then alt_color = PM.Parameters.Riskion_Regions_Rh_Color
                    ' get event attributes data
                    Dim sEventViewData As String = ""
                    Dim sEventFullData As String = ""
                    If ShowEventCategoriesOnBowTie Then
                        For Each tAttr As clsAttribute In AttributesList
                            sEventViewData += CStr(IIf(sEventViewData = "", "", ",")) + GetEventAttrViewData(tAttr, alt.ID)
                            Dim tValue As Object = PM.Attributes.GetAttributeValue(tAttr.ID, alt.ID)
                            Dim sValue As String = ""
                            If tValue IsNot Nothing Then sValue = tValue.ToString
                            Dim sValues As String = ""
                            Dim e As clsAttributeEnumeration = PM.Attributes.GetEnumByID(tAttr.EnumID)
                            If e IsNot Nothing Then
                                For Each item In e.Items
                                    Dim iIsDefValue As Integer = 0
                                    If tAttr.DefaultValue IsNot Nothing Then iIsDefValue = If(item.ID.ToString = tAttr.DefaultValue.ToString, 1, 0)
                                    sValues += CStr(IIf(sValues = "", "", ",")) + String.Format("['{0}','{1}',{2}, {3}]", item.ID.ToString, JS_SafeString(item.Value), iIsDefValue, IIf(sValue.Contains(item.ID.ToString), 1, 0))
                                Next
                            End If
                            sEventFullData += CStr(IIf(sEventFullData = "", "", ",")) + String.Format("['{0}','{1}',[{2}],{3}]", tAttr.ID.ToString, JS_SafeString(App.GetAttributeName(tAttr)), sValues, CInt(tAttr.ValueType))
                        Next
                    End If
                    ' end attributes
                    altVal = String.Format("'{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},{8},[{9}],[{10}]", SelectedEventID, JS_SafeString(alt.Name), Double2String(alt.LikelihoodValue * 100, PM.Parameters.DecimalDigits, True), Double2String(If(tShowDollarValue, alt.ImpactValue * DollarValueOfEnterprise, alt.ImpactValue * 100), PM.Parameters.DecimalDigits, Not tShowDollarValue), Double2String(If(tShowDollarValue, alt.RiskValue * DollarValueOfEnterprise, alt.RiskValue * 100), PM.Parameters.DecimalDigits, Not tShowDollarValue), alt_color, IIf(alt.HasContributionsL, 1, 0), If(alt.HasContributionsI, 1, 0), If(Not String.IsNullOrEmpty(cAlt.InfoDoc), 1, 0), sEventViewData, sEventFullData)
                End If

                Dim isEventWithNoSpecificCause As Boolean = False

                For Each node As clsPriorityData In LikelihoodNodes
                    Dim modelNode As clsNode = PM.Hierarchy(ECHierarchyID.hidLikelihood).GetNodeByID(node.GuidID)
                    If modelNode IsNot Nothing AndAlso tEventData IsNot Nothing AndAlso modelNode.IsTerminalNode AndAlso (wrtNode Is Nothing OrElse wrtNode Is modelNode OrElse wrtNode.GetNodesBelow(UNDEFINED_USER_ID).Contains(modelNode)) Then
                        If HasContribution(tEventData, node.GuidID) Then 'OrElse (Not isEventWithNoSpecificCause AndAlso (tEventData.LikelihoodContributedNodeIDs.Count = 0 OrElse AllContributionsCategories(tEventData))) Then
                            isEventWithNoSpecificCause = (tEventData.LikelihoodContributedNodeIDs.Count = 0) AndAlso wrtNode Is Nothing
                            node.NodePath = modelNode.ParentNode.NodePath
                            node.IsVisible = Not isEventWithNoSpecificCause
                            node.RiskNodeType = modelNode.RiskNodeType
                            node.InfoDoc = modelNode.InfoDoc
                            If node.IsVisible Then InitNodePriorities(node, tEventData, True)
                        End If
                    End If
                Next

                SortBowTieNodes(LikelihoodNodes, True)

                Dim sources_val As String = ""
                For Each node As clsPriorityData In LikelihoodNodes
                    If node.IsVisible Then sources_val += CStr(IIf(sources_val = "", "", ",")) + String.Format("['{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},{8},{9},{10}]", node.GuidID, JS_SafeString(node.Title), Double2String(node.PriorityL * 100, PM.Parameters.DecimalDigits, True), Double2String(node.PriorityV * 100, PM.Parameters.DecimalDigits, True), Double2String(node.PriorityLV * 100, PM.Parameters.DecimalDigits, True), JS_SafeString(node.NodePath), CInt(node.RiskNodeType), 0, IIf(Not String.IsNullOrEmpty(node.InfoDoc), 1, 0), "[]", "{}") ' {7} - unused field, can be used for any data
                Next

                lkhVal = sources_val + CStr(IIf(sources_val = "", "", ",")) + lkhVal

                wrtNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(forObjective)
                If wrtNode Is PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0) Then wrtNode = Nothing

                For Each node As clsPriorityData In ImpactNodes
                    Dim modelNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(node.GuidID)
                    If modelNode IsNot Nothing AndAlso tEventData IsNot Nothing AndAlso modelNode.IsTerminalNode AndAlso (wrtNode Is Nothing OrElse wrtNode Is modelNode OrElse wrtNode.GetNodesBelow(UNDEFINED_USER_ID).Contains(modelNode)) Then
                        If HasContribution(tEventData, node.GuidID) Then
                            node.NodePath = ""
                            If modelNode.ParentNode IsNot Nothing Then node.NodePath = modelNode.ParentNode.NodePath
                            node.IsVisible = True
                            node.InfoDoc = modelNode.InfoDoc
                            node.RiskNodeType = modelNode.RiskNodeType
                            If node.IsVisible Then InitNodePriorities(node, tEventData, False)
                        End If
                    End If
                Next

                SortBowTieNodes(ImpactNodes, False)
                For Each node As clsPriorityData In ImpactNodes
                    If node.IsVisible Then impVal += CStr(IIf(impVal = "", "", ",")) + String.Format("['{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}',{8}]", node.GuidID, JS_SafeString(node.Title), Double2String(node.PriorityL * 100, PM.Parameters.DecimalDigits, True), Double2String(node.PriorityV * 100, PM.Parameters.DecimalDigits, True), If(tShowDollarValue, CostString(node.PriorityLV * DollarValueOfEnterprise, CostDecimalDigits, True), Double2String(node.PriorityLV * 100, PM.Parameters.DecimalDigits, True)), JS_SafeString(node.NodePath), CInt(node.RiskNodeType), "", IIf(Not String.IsNullOrEmpty(node.InfoDoc), 1, 0))
                Next

                If alt IsNot Nothing Then
                    alt.Causes.Clear()
                    For Each node In LikelihoodNodes
                        If node.IsVisible Then alt.Causes.Add(node)
                    Next

                    alt.Consequences.Clear()
                    For Each node In ImpactNodes
                        If node.IsVisible Then alt.Consequences.Add(node)
                    Next
                End If
            End If
        End If
        'Return String.Format("var selected_alt_data = [{0}]; ", altVal) + Environment.NewLine + String.Format("var likelihood_data = [{0}]; ", lkhVal) + Environment.NewLine + String.Format("var impact_data = [{0}]; ", impVal)
        Return String.Format("[[{0}],[{1}],[{2}],{3},{4}]", altVal, lkhVal, impVal, GetBTControlsData(), GetCategoriesData(True))
    End Function

    Private Sub InitNodePriorities(node As clsPriorityData, tEvent As clsBowTieData2, IsLeft As Boolean)
        Dim tObjValue As Double = 0
        Dim tEventWRTObjValue As Double = 0
        Dim tObjMultipliedValue As Double = 0
        Dim tBowTieNodeValues = tEvent.LikelihoodValues
        If Not IsLeft Then
            tBowTieNodeValues = tEvent.ImpactValues
        End If
        For Each value In tBowTieNodeValues
            If value.CovObjID.Equals(node.GuidID) Then
                If (IsLeft AndAlso NormalizedLkhd = SynthesisMode.smAbsolute) OrElse ((Not IsLeft) AndAlso (NormalizedLocalImpact = LocalNormalizationType.ntUnnormalized)) Then
                    tObjValue = value.CovObjValueAbsolute
                    tEventWRTObjValue = value.AltWRTCovObjValueAbsolute
                    tObjMultipliedValue = value.MultipliedValueAbsolute
                Else
                    tObjValue = value.CovObjValueRelative
                    tEventWRTObjValue = value.AltWRTCovObjValueRelative
                    tObjMultipliedValue = value.MultipliedValueRelative
                End If
            End If
        Next

        With node
            .PriorityL = tObjValue
            .PriorityLV = tObjMultipliedValue
            .PriorityV = tEventWRTObjValue
        End With
    End Sub

    Private Sub SortBowTieNodes(ByRef tNodes As List(Of clsPriorityData), isLikelihood As Boolean)
        Dim tmpSortedNodes As IOrderedEnumerable(Of clsPriorityData) = Nothing

        If BowTieSort IsNot Nothing AndAlso BowTieSort.Count > 1 Then
            If isLikelihood Then
                Dim tState = BowTieSort(0)
                Select Case tState
                    Case BowTieSortState.sCauses
                        If tmpSortedNodes Is Nothing Then
                            tmpSortedNodes = tNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityL))
                        Else
                            tmpSortedNodes = tmpSortedNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityL))
                        End If
                    Case BowTieSortState.sCausesLkl
                        If tmpSortedNodes Is Nothing Then
                            tmpSortedNodes = tNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityLV))
                        Else
                            tmpSortedNodes = tmpSortedNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityLV))
                        End If
                    Case BowTieSortState.sCausesToEvents
                        If tmpSortedNodes Is Nothing Then
                            tmpSortedNodes = tNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityV))
                        Else
                            tmpSortedNodes = tmpSortedNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityV))
                        End If
                End Select
            Else
                Dim tState = BowTieSort(1)
                Select Case tState
                    Case BowTieSortState.sConsequences
                        If tmpSortedNodes Is Nothing Then
                            tmpSortedNodes = tNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityL))
                        Else
                            tmpSortedNodes = tmpSortedNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityL))
                        End If
                    Case BowTieSortState.sConsequencesImp
                        If tmpSortedNodes Is Nothing Then
                            tmpSortedNodes = tNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityLV))
                        Else
                            tmpSortedNodes = tmpSortedNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityLV))
                        End If
                    Case BowTieSortState.sConsequencesToEvents
                        If tmpSortedNodes Is Nothing Then
                            tmpSortedNodes = tNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityV))
                        Else
                            tmpSortedNodes = tmpSortedNodes.OrderByDescending(Function(tDataItem) (tDataItem.PriorityV))
                        End If
                End Select
            End If
        End If

        If tmpSortedNodes IsNot Nothing Then
            tNodes = tmpSortedNodes.ToList
        Else
            tNodes = tNodes.ToList
        End If
    End Sub

    'Public Function GetNodePath(Node As clsNode) As String
    '    If Node IsNot Nothing AndAlso Node.ParentNode IsNot Nothing Then
    '        Dim tPath As String = GetNodePath(Node.ParentNode)
    '        If Not String.IsNullOrEmpty(tPath) Then
    '            Return tPath + " / " + Node.NodeName
    '        Else
    '            Return Node.NodeName
    '        End If
    '    Else
    '        Return ""
    '    End If
    'End Function

    Private Function HasContribution(tEventData As clsBowTieData2, NodeGuid As Guid) As Boolean
        Return tEventData.LikelihoodContributedNodeIDs.Contains(NodeGuid) OrElse tEventData.ImpactContributedNodeIDs.Contains(NodeGuid)
    End Function

    Private Function GetExportDataItems() As List(Of ExportRiskData)
        Dim res As New List(Of ExportRiskData)
        Dim oldSelectedEventID As Guid = SelectedEventID
        For Each mAlt In OrderedEvents
            SelectedEventID = mAlt.NodeGuidID
            GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
            Dim alt As clsRiskData = Nothing
            For Each a In AlternativesData
                If a.ID.Equals(mAlt.NodeGuidID) Then
                    alt = a
                End If
            Next
            If alt IsNot Nothing Then
                Dim dataItem As New ExportRiskData
                dataItem.EventName = alt.Name
                dataItem.RiskPriority = alt.RiskValue
                dataItem.SumCausesPriority = alt.LikelihoodValue
                dataItem.SumConsequencesPriority = alt.ImpactValue
                For Each item In alt.Causes
                    dataItem.Causes.Add(item)
                Next
                For Each item In alt.Consequences
                    dataItem.Consequences.Add(item)
                Next
                res.Add(dataItem)
            End If
        Next
        SelectedEventID = oldSelectedEventID
        Return res
    End Function

    Function GetBTControlData(ctrl As clsControl) As String
        Dim apps As String = ""
        Dim ctrlIndex As Integer = _Treatments.IndexOf(ctrl) + 1
        For Each a As clsControlAssignment In ctrl.Assignments
            If ctrl.Type = ControlType.ctCause OrElse a.EventID.Equals(SelectedEventID) Then
                apps += CStr(IIf(apps <> "", ",", "")) + String.Format("['{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8}]", a.ID, JS_SafeString(a.Comment), Double2String(a.Value, MAX_DECIMALS_FOR_CONTROLS), a.ObjectiveID, a.EventID, CInt(a.MeasurementType), a.MeasurementScaleGuid, IIf(a.IsActive, "1", "0"), ctrlIndex)
            End If
        Next
        Return String.Format("['{0}','{1}', [{2}], '{3}', '{4}', '{5}', '{6}', {7}]", ctrl.ID, JS_SafeString(ctrl.Name), apps, CostString(ctrl.Cost), JS_SafeString(SafeFormString(GetControlInfodoc(App.ActiveProject, ctrl, True))), CStr(IIf(ctrl.Categories Is Nothing, "", JS_SafeString(ctrl.Categories))), CInt(ctrl.Type), Bool2Num(ctrl.Enabled))   ' D4346
    End Function

    Public Function GetBTControlsData() As String
        Dim retVal As String = ""

        ControlsListReset()
        Treatments = ControlsList

        If IsBowTiePage AndAlso UseControlsReductions AndAlso _Treatments IsNot Nothing Then
            For Each ctrl As clsControl In _Treatments.Where(Function(c) c.Enabled)
                retVal += CStr(IIf(retVal <> "", ",", "")) + GetBTControlData(ctrl)
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    'Public TotalAssignmentsCount As Integer = 0

    Public Function GetHasControls() As Boolean
        Return PM.Controls.Controls.Where(Function(ctrl) ctrl.Type <> ControlType.ctUndefined AndAlso ctrl.Active).Count > 0
    End Function

    Public Function GetControlData(ctrl As clsControl) As String
        Dim appl_count As Integer
        'GetApplicationsData(ctrl.ID, appl_count)
        'TotalAssignmentsCount += appl_count

        Dim attrCategory As clsAttribute = PM.Attributes.GetAttributeByID(ATTRIBUTE_CONTROL_CATEGORY_ID)
        Dim tCategories As clsAttributeEnumeration = Nothing
        If attrCategory IsNot Nothing AndAlso Not attrCategory.EnumID.Equals(Guid.Empty) Then
            tCategories = PM.Attributes.GetEnumByID(attrCategory.EnumID)
        End If

        Dim sCategroies As String = ""
        If tCategories IsNot Nothing AndAlso TypeOf ctrl.Categories Is String Then
            Dim sItems As String = CStr(ctrl.Categories)
            If Not String.IsNullOrEmpty(sItems) Then
                For Each item In tCategories.Items
                    If sItems.Contains(item.ID.ToString) Then
                        sCategroies += CStr(IIf(sCategroies = "", "", ", ")) + item.Value
                    End If
                Next
            End If
        End If

        Dim fChk As Integer = CInt(IIf(ctrl.Enabled, 1, 0))

        Return String.Format("[{0},{1},'{2}',{3},{4},{5},{6},'{7}',{8},{9},{10},'{11}','{12}','{13}',{14}]", Treatments.IndexOf(ctrl) + 1, IIf(ctrl.Active, "true", "false"), JS_SafeString(ctrl.Name), CInt(ctrl.Type), 0, JS_SafeNumber(CDbl(IIf(ctrl.IsCostDefined, ctrl.Cost, 0))), appl_count, JS_SafeString(sCategroies), 0, Bool2JS(ctrl.Must), Bool2JS(ctrl.MustNot), ctrl.ID.ToString, JS_SafeString(GetControlInfodoc(PRJ, ctrl, True)), CStr(IIf(ctrl.Categories Is Nothing, "", JS_SafeString(ctrl.Categories))), fChk) ' D4345
    End Function

    Public Function GetControlsData() As String
        Dim retVal As String = ""
        'TotalAssignmentsCount = 0

        ControlsListReset()
        Treatments = ControlsList

        For Each ctrl As clsControl In Treatments
            retVal += CStr(IIf(retVal = "", "", ",")) + GetControlData(ctrl)
        Next
        Return String.Format("[{0}]", retVal)
    End Function

    Private Function GetControlByID(ID As Guid) As clsControl
        For Each ctrl In Treatments
            If ctrl.ID.Equals(ID) Then Return ctrl
        Next
        Return Nothing
    End Function

    Private Sub ActivateControl(ControlID As Guid, isActivate As Integer)
        With PM
            Dim ctrl As clsControl = GetControlByID(ControlID)
            If ctrl IsNot Nothing Then
                Dim res As Boolean = .Controls.SetControlActive(ctrl.ID, CBool(IIf(isActivate = 1, True, False)))
                If res Then .Controls.WriteControls(ECModelStorageType.mstCanvasStreamDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
            End If
        End With
    End Sub

    Protected Sub DivBowTieData_PreRender(sender As Object, e As EventArgs) Handles DivBowTieData.PreRender
        'InitRegions()
        DivBowTieData.InnerHtml = GetBowTieData(SelectedEventID, SelectedHierarchyNodeID)
    End Sub

    Public ReadOnly Property SourcesGoalNodeID As String
        Get
            Return PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID.ToString
        End Get
    End Property

    Public Function CheckedUsersCount() As Integer
        Dim retVal As Integer = 0
        With PM
            If .CombinedGroups IsNot Nothing AndAlso .CombinedGroups.GroupsList IsNot Nothing Then
                For Each tGroup As clsCombinedGroup In .CombinedGroups.GroupsList
                    If IsUserChecked(tGroup.CombinedUserID) Then retVal += 1
                Next
            End If
            For Each tAppUser As clsApplicationUser In App.DBUsersByProjectID(App.ProjectID)
                If IsUserChecked(tAppUser.UserID) Then retVal += 1
            Next
        End With
        Return retVal
    End Function

    ' Show Sources/Consequences hierarchy in a modal window
    Protected Sub RadTreeViewLikelihood_Load(sender As Object, e As EventArgs) Handles RadTreeViewLikelihood.Load
        If RadTreeViewLikelihood.Nodes.Count = 0 Then
            LoadBTHierarchy(ECHierarchyID.hidLikelihood, RadTreeViewLikelihood)
            RadTreeViewLikelihood.Attributes.Add("style", "overflow:hidden;")
        End If
    End Sub

    Protected Sub RadTreeViewImpact_Load(sender As Object, e As EventArgs) Handles RadTreeViewImpact.Load
        If RadTreeViewImpact.Nodes.Count = 0 Then
            LoadBTHierarchy(ECHierarchyID.hidImpact, RadTreeViewImpact)
            RadTreeViewImpact.Attributes.Add("style", "overflow:hidden;")
        End If
    End Sub

    Private Sub LoadBTHierarchy(HID As ECHierarchyID, TreeControl As RadTreeView)
        AddNodesToRadTree(PM.Hierarchy(HID).GetLevelNodes(0), TreeControl.Nodes, 0)
    End Sub

    Private Function AddRadTreeNode(sCaption As String, ID As Integer, ByRef RadNodes As RadTreeNodeCollection, Level As Integer, fHasChilds As Boolean) As RadTreeNode
        Dim rNode As New RadTreeNode
        rNode.Text = JS_SafeHTML(ShortString(sCaption, CInt(IIf(Level = 0, 65, 80 - 4 * Level)), True))
        Dim asRoot As Boolean = Level = 0
        rNode.Text = String.Format("<span class='{1}'>{0}</span>", rNode.Text, IIf(asRoot, "tree_root_popup", "tree_node_popup"))
        rNode.Value = CStr(ID)
        rNode.CssClass = "text"
        RadNodes.Add(rNode)
        Return rNode
    End Function

    Private Sub AddNodesToRadTree(Nodes As List(Of clsNode), ByRef RadNodes As RadTreeNodeCollection, LevelOffset As Integer)
        If Nodes Is Nothing Or RadNodes Is Nothing Then Exit Sub
        For Each tNode As clsNode In Nodes
            Dim Childs As List(Of clsNode) = tNode.Children
            'Dim rNode As RadTreeNode = AddRadTreeNode(If(tNode.ParentNode Is Nothing, "Overall", tNode.NodeName), tNode.NodeID, RadNodes, tNode.Level + LevelOffset, Childs.Count > 0)
            Dim rNode As RadTreeNode = AddRadTreeNode(tNode.NodeName, tNode.NodeID, RadNodes, tNode.Level + LevelOffset, Childs.Count > 0)
            If Childs.Count > 0 Then
                AddNodesToRadTree(Childs, rNode.Nodes, LevelOffset)
                rNode.Expanded = True
            End If
        Next
    End Sub

#End Region

#Region "Operations with Treatments"

    Public Sub AddControlAssignment(ControlID As Guid, Object1ID As Guid)
        Dim controlAssignment As clsControlAssignment = PM.Controls.AddControlAssignment(ControlID, Object1ID)
        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignment added"))
    End Sub

    Public Sub DeleteControlAssignment(ControlID As Guid, Object1ID As Guid)
        Dim res As Boolean = False
        Dim AssignmentID As Guid = Guid.Empty
        Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
        If ctrl IsNot Nothing Then
            For Each appl As clsControlAssignment In ctrl.Assignments
                If appl.ObjectiveID.Equals(Object1ID) Then AssignmentID = appl.ID
            Next
        End If

        If Not AssignmentID.Equals(Guid.Empty) Then
            If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                res = True
            End If
        End If
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignment deleted"))
    End Sub

    Public Sub DeleteControlAssignment2(ControlID As Guid, Object1ID As Guid, EventID As Guid)
        Dim res As Boolean = False
        Dim AssignmentID As Guid = Guid.Empty
        Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
        If ctrl IsNot Nothing Then
            For Each appl As clsControlAssignment In ctrl.Assignments
                If appl.ObjectiveID.Equals(Object1ID) AndAlso appl.EventID.Equals(EventID) Then AssignmentID = appl.ID
            Next
        End If

        If Not AssignmentID.Equals(Guid.Empty) Then
            If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                res = True
            End If
        End If
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assigment deleted"))
    End Sub

    Public Sub DeleteControlsAssignments(ControlIDs As List(Of Guid), AssignmentIDs As List(Of Guid))
        Dim res As Boolean = False
        For Each ControlID In ControlIDs
            For Each AssignmentID In AssignmentIDs
                If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                    res = True
                End If
            Next
        Next
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments deleted"))
    End Sub

    Public Sub DeleteControlsAssignments2(ControlIDs As List(Of Guid), ObjIDs As List(Of Guid), EventIDs As List(Of Guid))
        Dim res As Boolean = False
        For Each ControlID In ControlIDs
            Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
            If ctrl IsNot Nothing Then
                For Each ObjID In ObjIDs
                    For Each EventID In EventIDs
                        Dim AssignmentID As Guid = Guid.Empty

                        For Each appl As clsControlAssignment In ctrl.Assignments
                            If appl.ObjectiveID.Equals(ObjID) AndAlso appl.EventID.Equals(EventID) Then AssignmentID = appl.ID
                        Next

                        If Not AssignmentID.Equals(Guid.Empty) Then
                            If PM.Controls.DeleteControlAssignment(ControlID, AssignmentID) Then
                                res = True
                            End If
                        End If
                    Next
                Next
            End If
        Next
        If res Then WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments deleted"))
    End Sub

    Public Sub AddControlsAssignments(ControlIDs As List(Of Guid), Object1IDs As List(Of Guid))
        For Each ControlID In ControlIDs
            Dim tControl = PM.Controls.GetControlByID(ControlID)
            For Each Object1ID In Object1IDs
                PM.Controls.AddControlAssignment(ControlID, Object1ID)
            Next
        Next
        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments added"))
    End Sub

    Public Sub AddControlsAssignments2(ControlType As ControlType, ControlIDs As List(Of Guid), Object1IDs As List(Of Guid), Object2IDs As List(Of Guid))
        Dim Objectives As clsHierarchy = Nothing
        If ControlType = ControlType.ctConsequenceToEvent Then
            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
        Else
            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
        End If

        For Each ControlID In ControlIDs
            Dim tControl = PM.Controls.GetControlByID(ControlID)
            For Each Object1ID In Object1IDs
                Dim node As clsNode = Objectives.GetNodeByID(Object1ID)
                Dim contAlts As List(Of clsNode) = node.GetContributedAlternatives
                Dim contAltsIDs As List(Of Guid) = New List(Of Guid)
                If contAlts IsNot Nothing Then
                    For Each alt As clsNode In contAlts
                        contAltsIDs.Add(alt.NodeGuidID)
                    Next
                End If
                If node.ParentNode Is Nothing Then 'no specific source
                    contAltsIDs.Clear()
                    For Each obj2 In Object2IDs
                        contAltsIDs.Add(obj2)
                    Next
                End If
                For Each Object2ID As Guid In Object2IDs
                    For Each altID As Guid In contAltsIDs
                        If altID.Equals(Object2ID) Then
                            PM.Controls.AddControlAssignment(ControlID, Object1ID, Object2ID)
                        End If
                    Next
                Next
            Next
        Next
        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments added"))
    End Sub

    Public Sub UpdateControlsAssignments(ControlType As ControlType, ControlIDs As List(Of Guid), Object1ID As Guid, Object2ID As Guid)
        Dim Objectives As clsHierarchy = Nothing

        If ControlType = ControlType.ctConsequenceToEvent Then
            Objectives = PM.Hierarchy(ECHierarchyID.hidImpact)
        Else
            Objectives = PM.Hierarchy(ECHierarchyID.hidLikelihood)
        End If

        For Each ctrl As clsControl In PM.Controls.Controls
            If ControlType = ctrl.Type AndAlso Not ControlIDs.Contains(ctrl.ID) Then
                Dim AssignmentID As Guid = Guid.Empty
                For Each appl As clsControlAssignment In ctrl.Assignments
                    If appl.ObjectiveID = Object1ID AndAlso (ControlType = ControlType.ctCause OrElse appl.EventID = Object2ID) Then
                        AssignmentID = appl.ID
                    End If
                Next
                If Not AssignmentID.Equals(Guid.Empty) Then
                    PM.Controls.DeleteControlAssignment(ctrl.ID, AssignmentID)
                End If
            End If
        Next

        For Each ControlID In ControlIDs
            Dim ctrl As clsControl = PM.Controls.GetControlByID(ControlID)
            If ctrl IsNot Nothing AndAlso ControlType = ctrl.Type Then
                Dim AssignmentExists As Boolean = False
                For Each appl As clsControlAssignment In ctrl.Assignments
                    If appl.ObjectiveID.Equals(Object1ID) AndAlso (ControlType = ControlType.ctCause OrElse appl.EventID.Equals(Object2ID)) Then
                        AssignmentExists = True
                    End If
                Next
                If Not AssignmentExists Then
                    PM.Controls.AddControlAssignment(ControlID, Object1ID, Object2ID)
                End If
            End If
        Next

        WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% assignments updated"))
    End Sub

    Public Sub SetControlMeasurementScale(ControlID As Guid, ControlAssignmentID As Guid, MeasurementType As ECMeasureType, MeasurementScaleID As Guid)
        Dim control As clsControl = PM.Controls.GetControlByID(ControlID)
        If control IsNot Nothing Then
            Dim controlAssignment As clsControlAssignment = control.GetAssignmentByID(ControlAssignmentID)
            If controlAssignment IsNot Nothing Then
                controlAssignment.MeasurementType = MeasurementType
                controlAssignment.MeasurementScaleGuid = MeasurementScaleID
                WriteControls(PRJ, ParseString(LogMessagePrefix + "%%Control%% measurement scale set"))
            End If
        End If
    End Sub

#End Region

#Region "Categories/Enum"

    Public Sub AddEnumAttributeItem(AttributeID As Guid, AttributeType As AttributeTypes, ItemID As Guid, ItemName As String, Optional saveAttributes As Boolean = True)
        Dim attr As clsAttribute = PM.Attributes.GetAttributeByID(AttributeID)
        If attr IsNot Nothing AndAlso (attr.ValueType = AttributeValueTypes.avtEnumeration OrElse attr.ValueType = AttributeValueTypes.avtEnumerationMulti) Then
            With PM
                Dim aEnum As clsAttributeEnumeration = .Attributes.GetEnumByID(attr.EnumID)
                If aEnum Is Nothing OrElse attr.EnumID.Equals(Guid.Empty) Then
                    aEnum = New clsAttributeEnumeration
                    aEnum.ID = ATTRIBUTE_CONTROL_CATEGORY_ENUM_ID
                    aEnum.Name = App.GetAttributeName(attr)
                    aEnum.Items = New List(Of clsAttributeEnumerationItem)
                    attr.EnumID = aEnum.ID
                    .Attributes.Enumerations.Add(aEnum)
                End If

                Dim eItem As clsAttributeEnumerationItem = aEnum.AddItem(ItemName)
                eItem.ID = ItemID

                If saveAttributes Then
                    .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
                End If
            End With
        End If
    End Sub

    Public Sub RemoveCategoryItem(ItemID As Guid)
        With PM
            Dim aEnum As clsAttributeEnumeration = .Attributes.GetEnumByID(ATTRIBUTE_CONTROL_CATEGORY_ENUM_ID)
            If aEnum IsNot Nothing Then
                aEnum.DeleteItem(ItemID)
            End If

            .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With
    End Sub

    Public Sub RenameCategoryItem(ItemID As Guid, ItemName As String)
        With PM
            Dim tSuccess As Boolean = False
            Dim aEnum As clsAttributeEnumeration = .Attributes.GetEnumByID(ATTRIBUTE_CONTROL_CATEGORY_ENUM_ID)
            If aEnum IsNot Nothing Then
                ItemName = ItemName.Trim
                Dim item As clsAttributeEnumerationItem = aEnum.GetItemByID(ItemID)
                If item IsNot Nothing AndAlso ItemName <> "" Then
                    item.Value = ItemName
                    tSuccess = True
                End If
            End If

            If tSuccess Then .Attributes.WriteAttributes(AttributesStorageType.astStreamsDatabase, .StorageManager.ProjectLocation, .StorageManager.ProviderType, .StorageManager.ModelID)
        End With
    End Sub

    Private Sub DivGridColorLegend_PreRender(sender As Object, e As EventArgs) Handles DivGridColorLegend.PreRender
        DivGridColorLegend.InnerHtml = "Color Legend: " + sGridColorLegend
    End Sub

    Private Sub DivFilterData_PreRender(sender As Object, e As EventArgs) Handles divFilterData.PreRender
        divFilterData.InnerHtml = String.Format("[{0},{1}]", LoadAttribData, LoadFilterData)
    End Sub

    Private Sub divNodesDataWithDollars_PreRender(sender As Object, e As EventArgs) Handles divNodesDataWithDollars.PreRender
        divNodesDataWithDollars.InnerHtml = String.Format("[{0},{1}]", GetAllObjsData(), GetEventsData())
    End Sub

    Private Sub RiskResultsPage_LoadComplete(sender As Object, e As EventArgs) Handles Me.LoadComplete
        PM.CalculationsManager.PrioritiesCacheManager.Enabled = False
        PM.PipeBuilder.PipeCreated = False  ' D6687
    End Sub

#End Region

End Class