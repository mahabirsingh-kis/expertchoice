Imports ExpertChoice.Misc
Imports System.Drawing

Partial Class RAIncreasingBudgetsPage2
    Inherits clsComparionCorePage

    Private sampleResults As Boolean = False

    'GridAlternatives columns
    Public Const COL_GRID_MODE As Integer = 0
    Public Const COL_ID As Integer = 1
    Public Const COL_INDEX As Integer = 2
    Public Const COL_NAME As Integer = 3
    Public Const COL_BUDGET_START As Integer = 4

    Public Const ROW_BENEFIT As Integer = 0
    Public Const ROW_COST As Integer = 1
    Public Const ROW_PROJECTS As Integer = 2

    Public RA_OPT_PARETO_CURVE_DEF_INCREASE_MODE As EfficientFrontierDeltaType = EfficientFrontierDeltaType.MinBenefitIncrease  ' D6449 // will be assigned on .Init() // default increase mode
    Public Const RA_OPT_PARETO_CURVE_DEF_COST_PRECISION As Integer = 2 ' default precision
    Public Const RA_OPT_PARETO_CURVE_DEF_INCREMENTS As Integer = 50 '20 ' default number of increments
    Public Const RA_OPT_PARETO_CURVE_DEF_DECREMENT As Double = 1 ' default decrement

    Public Const OPT_GRIDVIEW_COL_WIDTH As Integer = 100   ' D2902
    Public Const OPT_PRECISION As Integer = 2
    Public Const OPT_SHOW_AS_IGNORES As Boolean = True
    Public Const OPT_SHOW_IGNORE_LINKS As Boolean = False
    Public Const _OPT_ALLOW_CHOSE_SOLVER As Boolean = False     ' D4075 + A1610

    Public Const OPT_NAME_COL_WIDTH As Integer = 200
    Public Const OPT_DATA_COL_WIDTH As Integer = 100

    Public Const OPT_SCENARIO_LEGEND_LEN As Integer = 45 'A0965    

    Private SUMMARY_HEADER_BACKGROUND As Color = Color.FromArgb(240, 245, 255)
    Private ALTERNATE_ROW_BACKGROUND As Color = Color.FromArgb(250, 250, 250) '"#fafafa"
    Private COLOR_MUST As Color = Color.FromArgb(0, 190, 0)
    Private COLOR_MUST_HEX As String = "#00be00"
    Private COLOR_MUST_NOT As Color = Color.IndianRed
    Private MUST_INDICATOR As String = "{3701C7FB-F2A7-44DF-941D-91A360DCAEF8}"
    Private PagePrefix As String = "Efficient Frontier (%%Controls%%)"
    Public lblCustomConstraintFunded As String = "Number Of Funded %%Alternatives%%"  

    Private Enum GridRowModes
        gmFundedMarks = 1
        gmAltNames = 2
    End Enum

    Public Property RA_Solver As raSolverLibrary
        Get
            ' D3924 ===
            If RA.Solver.SolverLibrary = raSolverLibrary.raGurobi AndAlso Not App.isGurobiAvailable Then
                If App.isXAAvailable Then   ' D4512
                    RA.Solver.SolverLibrary = raSolverLibrary.raXA
                    App.ActiveProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raXA
                Else
                    ' D7543 ===
                    If App.isBaronAvailable Then
                        RA.Solver.SolverLibrary = raSolverLibrary.raBaron
                        App.ActiveProject.ProjectManager.Parameters.RASolver = raSolverLibrary.raBaron
                    End If
                End If
                ' D7543 ==
            End If
            ' D3924 ==
            Return RA.Solver.SolverLibrary  ' D3877
        End Get
        Set(value As raSolverLibrary)
            ' D3877 ===
            If RA.Solver.SolverLibrary <> value Then
                RA.Solver.SolverLibrary = value
                App.ActiveProject.ProjectManager.Parameters.RASolver = value
            End If
            ' D3877 ==
        End Set
    End Property

    Public Property PlotTitle As String
        Get
           Return CStr(PM.Attributes.GetAttributeValue(ATTRIBUTE_RA_IB_PLOT_TITLE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_RA_IB_PLOT_TITLE_ID,  AttributeValueTypes.avtString, value)
        End Set
    End Property

    Public Property CalculateFrom As Double
        Get
            Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_FROM_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Double)
            Dim tObjValue As Object = Nothing
            If value <> UNDEFINED_INTEGER_VALUE Then tObjValue = Value
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_FROM_ID, AttributeValueTypes.avtDouble, tObjValue)
        End Set
    End Property

    Public Property CalculateTo As Double
        Get
            Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_TO_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Double)
            Dim tObjValue As Object = Nothing
            If value <> UNDEFINED_INTEGER_VALUE Then tObjValue = Value
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_TO_ID, AttributeValueTypes.avtDouble, tObjValue)
        End Set
    End Property

    Public Readonly Property IsCalculateFromDefined As Boolean
        Get
            Return CalculateFrom <> UNDEFINED_INTEGER_VALUE
        End Get
    End Property

    Public Readonly Property IsCalculateToDefined As Boolean
        Get
            Return CalculateTo <> UNDEFINED_INTEGER_VALUE
        End Get
    End Property

    Public Property AxisXTitle As String
        Get           
            Return ParseString(CStr(PM.Attributes.GetAttributeValue(If(PM.IsRiskProject, ATTRIBUTE_RO_IB_PLOT_XAXIS_TITLE_ID, ATTRIBUTE_RA_IB_PLOT_XAXIS_TITLE_ID), UNDEFINED_USER_ID)))
        End Get
        Set(value As string)
            WriteSetting(PRJ, If(PM.IsRiskProject, ATTRIBUTE_RO_IB_PLOT_XAXIS_TITLE_ID, ATTRIBUTE_RA_IB_PLOT_XAXIS_TITLE_ID), AttributeValueTypes.avtString, value)
        End Set
    End Property

    Public Property AxisYTitle As String
        Get
            Return ParseString(CStr(PM.Attributes.GetAttributeValue(If(PM.IsRiskProject, ATTRIBUTE_RO_IB_PLOT_YAXIS_TITLE_ID, ATTRIBUTE_RA_IB_PLOT_YAXIS_TITLE_ID), UNDEFINED_USER_ID)))
        End Get
        Set(value As string)
            WriteSetting(PRJ, If(PM.IsRiskProject, ATTRIBUTE_RO_IB_PLOT_YAXIS_TITLE_ID, ATTRIBUTE_RA_IB_PLOT_YAXIS_TITLE_ID), AttributeValueTypes.avtString, value)
        End Set
    End Property    

    Private ReadOnly Property SESS_TREATMENTS As String
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
            _Treatments = value.Where(Function (ctrl) ctrl.Type <> ControlType.ctUndefined).ToList()
            Session(SESS_TREATMENTS) = value
        End Set
    End Property

    Public Property gridModeShowProjects As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_GRID_MODE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_GRID_MODE_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property KeepFundedAlts As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_KEEP_FUNDED_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_KEEP_FUNDED_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property ZoomPlot As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_TREATMENTS_OPTIMIZE_IB_ZOOM_PLOT_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_RISK_TREATMENTS_OPTIMIZE_IB_ZOOM_PLOT_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property Mode As EfficientFrontierDeltaType
        Get
            'If Not isAdvancedMode Then Return EfficientFrontierDeltaType.MinBenefitIncrease
            Dim retVal As EfficientFrontierDeltaType = RA_OPT_PARETO_CURVE_DEF_INCREASE_MODE
            Dim tOpt As Integer = CInt(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_INCREASE_MODE_ID, UNDEFINED_USER_ID))
            If tOpt >= 0 Then retVal = CType(tOpt, EfficientFrontierDeltaType)
            Return retVal
        End Get
        Set(value As EfficientFrontierDeltaType)
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_INCREASE_MODE_ID, AttributeValueTypes.avtLong, CInt(value))
        End Set
    End Property

    Public Property DetailedTooltips As Boolean
        Get
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_IB_DETAILED_TOOLTIPS_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Boolean)
            WriteSetting(PRJ, ATTRIBUTE_IB_DETAILED_TOOLTIPS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Private _layout As Integer = -1
    Public Property LayoutMode As Integer '0 - Both, 1 - Grid only, 2 - Chart only
        Get
            If _layout < 0 Then _layout = CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_IB_LAYOUT_MODE_ID, UNDEFINED_USER_ID))
            If _layout < 0 OrElse _layout > 2 Then _layout = 0 ' Grid mode by deafult per Ernest's request
            Return _layout
        End Get
        Set(value As Integer)
            _layout = value
            WriteSetting(PRJ, ATTRIBUTE_IB_LAYOUT_MODE_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Property NumberOfIncrements As Integer
        Get
            Dim retVal As Integer = RA_OPT_PARETO_CURVE_DEF_INCREMENTS
            Dim tOpt As Integer = CInt(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_NUM_INCREMENTS_ID, UNDEFINED_USER_ID))
            If tOpt >= 0 Then retVal = tOpt
            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_NUM_INCREMENTS_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    Private Function GetDefaultIncrement() As Double
        'Dim tMinCost As Double
        'Dim tMinCostDiff As Double
        Dim tMinBudget As Double
        Dim tMaxBudget As Double
        'GetSolverParameters(tMinCost, tMinCostDiff, tMaxBudget, tMinBudget)
        GetSolverParameters(tMaxBudget, tMinBudget)
        Return Math.Round((tMaxBudget - tMinBudget) / RA_OPT_PARETO_CURVE_DEF_INCREMENTS)
    End Function

    Public Property SpecifiedIncrement As Double
        Get
            Dim retVal As Double = 0
            Dim tOpt As Double = CDbl(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_INCREMENT_ID, UNDEFINED_USER_ID))
            If tOpt >= 0 Then retVal = tOpt Else retVal = GetDefaultIncrement()
            Return retVal
        End Get
        Set(value As Double)
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_INCREMENT_ID, AttributeValueTypes.avtDouble, value)
        End Set
    End Property

    Public Property SpecifiedDecrement As Double
        Get
            Dim retVal As Double = 0
            Dim tOpt As Double = CDbl(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_RA_PARETO_CURVE_DECREMENT_ID, UNDEFINED_USER_ID))
            If tOpt >= 0 Then retVal = tOpt Else retVal = RA_OPT_PARETO_CURVE_DEF_DECREMENT
            Return retVal
        End Get
        Set(value As Double)
            WriteSetting(PRJ, ATTRIBUTE_RA_PARETO_CURVE_DECREMENT_ID, AttributeValueTypes.avtDouble, value)
        End Set
    End Property

    Public Property MinBenefitIncrease As Double
        Get
            Return PM.Parameters.EfficientFrontierMinBenefitIncrease
        End Get
        Set(value As Double)
            PM.Parameters.EfficientFrontierMinBenefitIncrease = value
            PM.Parameters.Save()
        End Set
    End Property

    'Public Property SelectedScenarioID As Integer
    '    Get
    '        Dim s = SessVar(SESS_RA_SCENARIO_ID)
    '        Dim retVal As Integer = 0
    '        If Not String.IsNullOrEmpty(s) Then Integer.TryParse(s, retVal)
    '        If Not RA.Scenarios.Scenarios.ContainsKey(retVal) Then retVal = 0
    '        Return retVal
    '    End Get
    '    Set(value As Integer)
    '        If RA.Scenarios.Scenarios.ContainsKey(value) Then
    '            RA.Scenarios.Scenarios(value).IsCheckedIB = True
    '            RA.Save()
    '        End If
    '        SessVar(SESS_RA_SCENARIO_ID) = value.ToString
    '    End Set
    'End Property

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

    Public Function GetRedLineValue As Double
        Return RedLineValue / 100
    End Function

    Public Property GreenLineValue As Double 'Max loss value specified by PM
        Get
            Dim retVal As Double = CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, UNDEFINED_USER_ID))
            'Dim isDollValue As Boolean = ShowDollarValue
            'If isDollValue Then
            '    If PM.DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE Then 
            '        retVal = UNDEFINED_INTEGER_VALUE 
            '    Else
            '        retVal /= 100
            '        retVal *= DollarValueOfEnterprise
            '    End If
            'End If
            Return If(retVal = UNDEFINED_INTEGER_VALUE, retVal, retVal / 100)
        End Get
        Set(value As Double)
            Dim isDollValue As Boolean = ShowDollarValue
            WriteSetting(PRJ, ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, AttributeValueTypes.avtDouble, If(isDollValue, If(PM.DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE Or PM.DollarValueOfEnterprise = 0, UNDEFINED_INTEGER_VALUE, (value / DollarValueOfEnterprise) * 100), value))
            'WriteSetting(PRJ, ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, AttributeValueTypes.avtDouble, value)
        End Set
    End Property

    'Public Function GetGreenLineValue As Double
    '    Return CDbl(PM.Attributes.GetAttributeValue(ATTRIBUTE_LEC_GREEN_LINE_VALUE_PRC_ID, UNDEFINED_USER_ID)) / 100
    'End Function

    Public Function IsSingleScenarioChecked() As Boolean
        Return GetCheckedScenarios.Count = 1
    End Function

    'Public Function GetFirstCheckedScenario() As RAScenario
    '    Dim checkedScenarios As IEnumerable(Of RAScenario) = GetCheckedScenarios
    '    If checkedScenarios.Count > 0 Then Return checkedScenarios(0)
    '    Return Nothing
    'End Function

    Public Function GetCheckedScenarios() As IEnumerable(Of RAScenario)
        Return RA.Scenarios.Scenarios.Values.Where(Function(sc) sc.IsCheckedIB)
    End Function

    Public Function GetCheckedScenariosToString() As String
        Dim retVal As String = ""
        Dim ColorPalette As String() = GetPalette(1)
        For each scenario As RAScenario In GetCheckedScenarios
            retVal += If(retVal = "", "", ",") + String.Format("[{0}, ""{1}"", ""{2}"", ""{3}""]", scenario.ID.ToString, JS_SafeString(scenario.Name), JS_SafeString(scenario.Description), ColorPalette(scenario.ID Mod ColorPalette.Count))
        Next
        Return retVal
    End Function

    Public Property HorizontalSplitterSize As Integer
        Get
            Return CInt(PM.Attributes.GetAttributeValue(ATTRIBUTE_IB_HORIZONTAL_SPLITTER_SIZE_ID, UNDEFINED_USER_ID))
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_IB_HORIZONTAL_SPLITTER_SIZE_ID, AttributeValueTypes.avtLong, value, "", "")
        End Set
    End Property

    ReadOnly Property SESS_SHOW_OPTIONS As String
        Get
            Return String.Format("RISK_SHOW_OPTIONS_{0}", App.ProjectID.ToString)
        End Get
    End Property

    Public Property ShowOptions As Boolean
        Get
            Dim tSessVar = Session(SESS_SHOW_OPTIONS)
            If tSessVar IsNot Nothing AndAlso TypeOf tSessVar Is Boolean Then
                Return CBool(tSessVar)
            End If
            Return True
        End Get
        Set(value As Boolean)
            Session(SESS_SHOW_OPTIONS) = value
        End Set
    End Property

    Public Property ShowDollarValue As Boolean
        Get
            Dim UseDollarValue As Boolean = CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID))
            Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_SHOW_DOLLAR_VALUE_ID, UNDEFINED_USER_ID)) AndAlso PM.CalculationsManager.DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso PM.CalculationsManager.DollarValueTarget <> UNDEFINED_STRING_VALUE
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
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_ID, AttributeValueTypes.avtDouble, value, ParseString(PagePrefix + ": Total Cost Changed to " + CostString(value)), "")
        End Set
    End Property

    Public Property DollarValueTarget As String
        Get
            Return PM.CalculationsManager.DollarValueTarget
        End Get
        Set(value As String)
            WriteSetting(PRJ, ATTRIBUTE_DOLLAR_VALUE_TARGET_ID, AttributeValueTypes.avtString, value, ParseString(PagePrefix + ": Cost Target Changed"), "")
        End Set
    End Property

    Public Function GetDollarValueFullString() As String
        If PM.IsRiskProject Then
            Dim tImpactGoalNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0)
            Dim retVal As String = ""
            retVal = String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), ResString("lblEnterprise"), CostString(PM.DollarValueOfEnterprise, 2, True))
            If DollarValueTarget <> "" AndAlso DollarValueTarget <> UNDEFINED_STRING_VALUE AndAlso Not tImpactGoalNode.NodeGuidID.Equals(New Guid(DollarValueTarget)) Then
                Dim tNode As clsNode = PM.Hierarchy(ECHierarchyID.hidImpact).GetNodeByID(New Guid(DollarValueTarget))
                Dim sDollvalue As String = ""
                If DollarValue <> UNDEFINED_INTEGER_VALUE Then sDollvalue = CostString(DollarValue, 2, True)
                retVal += If(retVal = "", "", ",") + String.Format("{0}&nbsp;{1}:&nbsp;{2}", ResString("lblValueOf"), GetTargetName(), sDollvalue)
            End If
            Return If(DollarValue <> UNDEFINED_INTEGER_VALUE AndAlso retVal <> "", " (" + retVal + ")", "")
        End If
        Return ""
    End Function
        
    ' Dollar value of Impact Goal node
    Public ReadOnly Property DollarValueOfEnterprise As Double
        Get
            If PM.IsRiskProject then
                Return PM.DollarValueOfEnterprise
            End If
            Return 0
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

    Dim _showCents As Boolean? = Nothing
    Public Property ShowCents As Boolean
        Get
            If Not _showCents.HasValue Then
                Return CBool(PM.Attributes.GetAttributeValue(ATTRIBUTE_RISK_SHOW_CENTS_ID, UNDEFINED_USER_ID))
            Else
                Return _showCents.Value
            End If
        End Get
        Set(value As Boolean)
            _showCents = value
            WriteSetting(PRJ, ATTRIBUTE_RISK_SHOW_CENTS_ID, AttributeValueTypes.avtBoolean, value)
        End Set
    End Property

    Public Sub New()
        MyBase.New(_PGID_RA_INC_BUDGETS)
    End Sub

    ' D2839 ===
    ReadOnly Property RA As ResourceAligner
        Get                        
            Return If(PM.IsRiskProject, PM.ResourceAlignerRisk, PM.ResourceAligner)
        End Get
    End Property
    ' D2839 ==

    ReadOnly Property PRJ As clsProject
        Get
            Return App.ActiveProject
        End Get
    End Property

    ReadOnly Property PM As clsProjectManager
        Get
            Return PRJ.ProjectManager
        End Get
    End Property

    Public ReadOnly Property BaseScenario As RAScenario
        Get
            Return RA.Scenarios.Scenarios(0)
        End Get
    End Property

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        RA_OPT_PARETO_CURVE_DEF_INCREASE_MODE = If(PM.IsRiskProject, EfficientFrontierDeltaType.MinBenefitIncrease, EfficientFrontierDeltaType.DeltaValue) ' D6449 default increase mode
        If PM.IsRiskProject Then
            Dim sPgid As String = CheckVar("pgid", "")
            Select Case sPgid
                Case "77996"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_OVERALL
                Case "77997"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES
                Case "77999"
                    CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS
            End Select
            If Not _OPT_ALLOW_CHOSE_SOLVER AndAlso RA.RiskOptimizer.SolverLibrary <> raSolverLibrary.raBaron Then RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raBaron ' D4075 + A1610
        End If

        If Not isCallback AndAlso Not IsPostBack Then
            RA.Load()
            If PM.IsRiskProject Then
                RA.Scenarios.CheckAlternatives()
            End If

            ControlsListReset()
            Treatments = ControlsList
        End If
    End Sub

    ' D2849 ===
    Protected Sub Page_InitComplete(sender As Object, e As EventArgs) Handles Me.InitComplete
        If Not IsPostBack AndAlso Not IsCallback Then
            RA.Scenarios.CheckModel() 'A1324

            'Make only Active Scenario Checked when entering Increasing Budgets
            For Each scenario As RAScenario In RA.Scenarios.Scenarios.Values
                scenario.IsCheckedIB = scenario.ID = RA.Scenarios.ActiveScenarioID
            Next
            If PM.IsRiskProject Then 
                'calculate, so that Impact Goal node's dollar value will be available
                RA.RiskOptimizer.GetComputedRisk(ControlsUsageMode.UseOnlyActive)
                'ReCalculate() 'A1319
            End If
        End If
        Ajax_Callback(Request.Form.ToString)
    End Sub
    ' D2849 ==

    'Private Sub ReCalculate() 'A1319
    '    With PM
    '        Dim CG As clsCombinedGroup = .CombinedGroups.GetDefaultCombinedGroup
    '        .CalculationsManager.Calculate(New clsCalculationTarget(CalculationTargetType.cttCombinedGroup, CG), .Hierarchy(.ActiveHierarchy).Nodes(0), .ActiveHierarchy, .ActiveAltsHierarchy)
    '    End With
    'End Sub

#Region "Hierarchy Tree View"

    Public PagesWrtObjective As Integer() = {_PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES, _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS}

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
            If PagesWrtObjective.Contains(CurrentPageID)
                Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
    
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
            If PagesWrtObjective.Contains(CurrentPageID)
                HierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)
            End If
            WriteSetting(PRJ, CType(IIf(HierarchyID = ECHierarchyID.hidLikelihood, ATTRIBUTE_RISK_SELECTED_LIKELIHOOD_NODE_ID, ATTRIBUTE_RISK_SELECTED_IMPACT_NODE_ID), Guid), AttributeValueTypes.avtString, value.ToString)
        End Set
    End Property

    'todo use a get_hierarchy_data function from api
    Public Function GetHierarchyData() As String
        If Not PagesWrtObjective.Contains(CurrentPageID) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

        Dim H As clsHierarchy = PM.Hierarchy(HierarchyID)
        For Each nodeTuple As Tuple(Of Integer, Integer, clsNode) In H.NodesInLinearOrder
            Dim node As clsNode = nodeTuple.Item3
            retVal += CStr(If(retVal = "", "", ",")) + String.Format("{{""id"":{0},""guid"":""{1}"",""name"":""{2}"",""pguid"":""{3}""", node.NodeID, node.NodeGuidID, JS_SafeString(node.NodeName), If(node.ParentNode Is Nothing, "", node.ParentNode.NodeGuidID.ToString))
            retVal += String.Format(",""info"":""{0}""", JS_SafeString(Infodoc2Text(PRJ, node.InfoDoc, True)))

            retVal += "}"
        Next

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetHierarchyColumns() As String
        If Not PagesWrtObjective.Contains(CurrentPageID) Then Return "[]"

        Dim retVal As String = ""

        Dim HierarchyID As ECHierarchyID = If(CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES, ECHierarchyID.hidLikelihood, ECHierarchyID.hidImpact)

        retVal = String.Format("[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 0, "ID", Bool2JS(False), "", "id", 0, "false", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 1, ParseString(If(HierarchyID = ECHierarchyID.hidLikelihood, "%%Objective(l)%% Name", "%%Objective(i)%% Name")), Bool2JS(True), "", "name", 0, "true", "false")
        retVal += String.Format(",[{0},'{1}',{2},'{3}','{4}',{5},{6},{7}]", 2, ParseString("Description"), Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs), "", "info", 0, "false", "false")

        Return String.Format("[{0}]", retVal)
    End Function


#End Region

    Private Function HasOption(sID As String, tScenario As RAScenario) As Boolean
        Dim has As Boolean = False
        With tScenario
            Select Case sID
                Case "cbOptMusts"
                    If PM.IsRiskProject Then 
                        has = Treatments.Where(Function(a) a.Must).Count > 0
                    Else
                        has = .Alternatives.Sum(Function(a) If(a.Must, 1, 0)) > 0
                    End If
                Case "cbOptMustNots"
                    If PM.IsRiskProject Then 
                        has = Treatments.Where(Function(a) a.MustNot).Count > 0
                    Else
                        has = .Alternatives.Sum(Function(a) If(a.MustNot, 1, 0)) > 0
                    End If
                Case "cbOptConstraints"
                    If PM.IsRiskProject Then 
                        has = RA.Solver.Constraints.Constraints.Count > 0
                    Else
                        has = .Constraints.Constraints.Count > 0
                    End If
                Case "cbOptDependencies"
                    'has = .Dependencies.Dependencies.Count > 0
                    If PM.IsRiskProject Then 
                        has = RA.Solver.Dependencies.HasData(.Alternatives, .Groups)
                    Else
                        has = .Dependencies.HasData(.Alternatives, .Groups)  ' D3881
                    End If
                Case "cbOptGroups", "cbBCGroups"
                    'has = .Groups.Groups.Count > 0
                    If PM.IsRiskProject Then 
                        has = RA.Solver.Groups.HasData(.Alternatives)
                    Else
                        has = .Groups.HasData(.Alternatives)    ' D3881
                    End If
                Case "cbOptFundingPools"
                    If PM.IsRiskProject Then 
                        has = RA.Solver.FundingPools.Pools.Count > 0
                    Else
                        has = .FundingPools.Pools.Count > 0
                    End If
                Case "cbOptRisks"
                    has = .Alternatives.Sum(Function(a) a.RiskOriginal) > 0
                Case "cbOptTimePeriods" ' D3824
                    'has = RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0     ' D3824 + D3841 + A1137 + D3943
                    has = tScenario.TimePeriods.Periods.Count > 0 'A1455
            End Select
        End With
        Return has
    End Function

    Public Function GetOptionStyle(sID As String) As String
        Dim has As Boolean = False
        If IsSingleScenarioChecked() Then
            has = HasOption(sID, GetCheckedScenarios(0))
        Else
            For Each scenario As RAScenario In GetCheckedScenarios
                If Not has Then has = HasOption(sID, scenario)
            Next
        End If
        If has Then Return "style='font-weight:bold;'" Else Return "style='color:#999;'"
    End Function

    Public Function IsOptionChecked(sID As String, settings As RASettings) As Boolean
        Dim chk As Boolean = False
        With If(PM.IsRiskProject, RA.RiskOptimizer.Settings, settings)
            Select Case sID
                Case "cbOptMusts"
                    chk = .Musts
                Case "cbOptMustNots"
                    chk = .MustNots
                Case "cbOptConstraints"
                    chk = .CustomConstraints
                Case "cbOptDependencies"
                    chk = .Dependencies
                Case "cbOptGroups"
                    chk = .Groups
                Case "cbOptFundingPools"
                    chk = .FundingPools
                Case "cbOptRisks"
                    chk = .Risks
                Case "cbOptTimePeriods" ' D3824
                    chk = .TimePeriods  ' D3824
                    ' D3078 ===
                Case "cbBaseCase"
                    chk = If(OPT_SHOW_AS_IGNORES, Not .UseBaseCase, .UseBaseCase)
                Case "cbBCGroups"
                    chk = If(OPT_SHOW_AS_IGNORES, Not .BaseCaseForGroups, .BaseCaseForGroups)
                    ' D3078 ==
            End Select
        End With
        Return chk
    End Function

    Public Function GetOptionValue(sID As String) As String
        Dim chk As Boolean = False
        If Not chk Then chk = IsOptionChecked(sID, If(IsSingleScenarioChecked(), GetCheckedScenarios(0).Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings))
        If OPT_SHOW_AS_IGNORES Then chk = Not chk ' D2931
        If chk Then Return " checked='checked' " Else Return ""
    End Function

    Public Readonly Property StringSelectedEventIDs As String
        Get
            Dim retVal As String = ""
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                If alt.Enabled Then
                    retVal += If(retVal = "", "", ",") + alt.NodeGuidID.ToString
                End If
            Next
            Return retVal
         End Get
     End Property

    Public Property SelectedConstraintID As Integer ' -1 = Budget Limit, Integer.MaxValue = Risk,  Integer.MaxValue > custom Constraint > 0
        Get
            Dim retVal As Integer = -1
            If Not PM.IsRiskProject Then
                Dim tOpt As Integer = CInt(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_IB_PLOT_CONSTRAINT_ID_ID, UNDEFINED_USER_ID))
                Dim tScenario As RAScenario = Nothing
                If GetCheckedScenarios.Count > 0 Then tScenario = GetCheckedScenarios(0)
                If tScenario IsNot Nothing Then
                    If tOpt = Integer.MaxValue Then 'Risk
                        retVal = tOpt
                        Dim HasRisks As Boolean = tScenario.Alternatives.Sum(Function(a) a.RiskOriginal) > 0
                        If Not HasRisks Then retVal = -1
                    Else ' Custom constraint
                        Dim tCC As RAConstraint = tScenario.Constraints.GetConstraintByID(tOpt)
                        If tCC IsNot Nothing Then
                            retVal = tOpt
                        End If
                    End If
                End If
            End If

            Return retVal
        End Get
        Set(value As Integer)
            WriteSetting(PRJ, ATTRIBUTE_IB_PLOT_CONSTRAINT_ID_ID, AttributeValueTypes.avtLong, value)
        End Set
    End Property

    'Public Property SelectedConstraintMin As Double
    '    Get
    '        Return CDbl(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_IB_PLOT_CONSTRAINT_MIN_ID, UNDEFINED_USER_ID))
    '    End Get
    '    Set(value As Double)
    '        WriteSetting(PRJ, ATTRIBUTE_IB_PLOT_CONSTRAINT_MIN_ID, AttributeValueTypes.avtDouble, value)
    '    End Set
    'End Property

    'Public Property SelectedConstraintMax As Double
    '    Get
    '        Dim retVal As Double = CDbl(RA.ProjectManager.Attributes.GetAttributeValue(ATTRIBUTE_IB_PLOT_CONSTRAINT_MAX_ID, UNDEFINED_USER_ID))
    '        If SelectedConstraintID = Integer.MaxValue Then 'Risk
    '            If retVal = UNDEFINED_INTEGER_VALUE Then
    '                Dim tScenario As RAScenario = Nothing
    '                If GetCheckedScenarios.Count > 0 Then tScenario = GetCheckedScenarios(0)
    '                If tScenario IsNot Nothing Then
    '                    retVal = tScenario.Alternatives.Sum(Function(a) a.Risk)
    '                End If
    '            End If
    '        End If
    '        Return retVal
    '    End Get
    '    Set(value As Double)
    '        WriteSetting(PRJ, ATTRIBUTE_IB_PLOT_CONSTRAINT_MAX_ID, AttributeValueTypes.avtDouble, value)
    '    End Set
    'End Property

    Public Function GetParameterDropdown() As String
        Dim tScenario As RAScenario = Nothing
        If GetCheckedScenarios.Count > 0 Then tScenario = GetCheckedScenarios(0)

        Dim retVal As String = "<select class='select' id='cbParameter' style='margin-right:0px; margin-top:1px; max-width:110px;' onchange='cbParameterChange(this.value);'>"
        retVal += String.Format("<option value='{0}' {1}>{2}</option>", -1, If(SelectedConstraintID = -1 OrElse tScenario Is Nothing, " selected='selected' ", ""), JS_SafeString(ResString("lblBudgetLimit")))

        If tScenario IsNot Nothing Then
            ' risk item
            'Dim HasRisks As Boolean = tScenario.Alternatives.Sum(Function(a) a.RiskOriginal) > 0
            'retVal += "<option value='' disabled='disabled'>------------------------</option>"
            'retVal += String.Format("<option value='{0}' {1} {2}>{3}</option>", Integer.MaxValue, If(SelectedConstraintID = Integer.MaxValue, " selected='selected' ", ""), If(Not HasRisks, " disabled='disabled' ", ""), JS_SafeString(ResString("lblRARisk")))

            ' separator
            If tScenario.Constraints.Constraints.Values.Count > 0 Then
                retVal += "<option value='' disabled='disabled'>------------------------</option>"
            End If

            For Each cc As RAConstraint In tScenario.Constraints.Constraints.Values
                retVal += String.Format("<option value='{0}' {1}>{2}</option>", cc.ID, If(SelectedConstraintID = cc.ID, " selected='selected' ", ""), JS_SafeString(cc.Name))
            Next
        End If

        retVal += "</select>&nbsp;"
        Return retVal
    End Function

    Public Function GetEventsData(Optional arrayOnly As Boolean = False) As String
        Dim retVal As String = ""

        'If PM.IsRiskProject Then
        Dim AltH As clsHierarchy = PM.AltsHierarchy(PM.ActiveAltsHierarchy)
        If AltH IsNot Nothing Then
            For Each alt As clsNode In AltH.TerminalNodes
                Dim fChk As Integer = 1
                If Not alt.Enabled Then fChk = 0
                retVal += If(retVal <> "", ",", "") + String.Format("['{0}','{1}',{2}]", alt.NodeGuidID, JS_SafeString(alt.NodeName), fChk)
            Next
        End If
        'End If

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetControlsData(Optional arrayOnly As Boolean = False) As String
        Dim retVal As String = ""

        If PM.IsRiskProject Then
            Dim index As Integer = 0
            ' get Controls data
            'For Each ctrl As clsControl In Treatments
            '    index += 1
            '    retVal += If(retVal <> "", ",", "") + String.Format("['{0}','{1}','{2}.']", ctrl.ID.ToString, JS_SafeString(ctrl.Name), index)
            'Next
            For Each ctrl As RAAlternative In RA.Solver.Alternatives.OrderBy(Function (c) c.SortOrder)
                index += 1
                retVal += If(retVal <> "", ",", "") + String.Format("['{0}','{1}','{2}.']", ctrl.ID, JS_SafeString(ctrl.Name), ctrl.SortOrder)
            Next
        Else
            ' get Alternatives data
            For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                retVal += If(retVal <> "", ",", "") + String.Format("['{0}','{1}','{2}']", alt.NodeGuidID.ToString, JS_SafeString(alt.NodeName), alt.Index)
            Next
        End If

        Return String.Format("[{0}]", retVal)
    End Function

    Public Function GetChartSettings(Optional minX As Double = Integer.MaxValue, Optional minY As Double = Integer.MaxValue, Optional maxX As Double = Integer.MinValue, Optional maxY As Double = Integer.MinValue) As String
        ' Plot title, X title, Y title, minX, maxX, minY, maxY
        If PM.IsRiskProject Then RA.RiskOptimizer.InitOriginalValues()
        If Not ZoomPlot Then 
            minY = 0
            maxY = 1

            'If PM.IsRiskProject Then
            '    minY = Math.Floor(RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * 100)
            '    If minY < 0 Then minY = 0
            '    maxY = Math.Ceiling(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * 100)
            '    If maxY < 0 Then maxY = 100
            '    If minY > maxY Then ' swap minY and maxY
            '        Dim tmpV As Double = minY
            '        minY = maxY
            '        maxY = tmpV
            '    End If
            'Else
            '    ' Comparion - Budget Limit, Risk Constraint, Custom Constraints
            '
            'End If

            minX = MinBudget
            maxX = MaxBudget

        End If

        If IsCalculateFromDefined() Then minX = Math.Max(CalculateFrom, minX)
        If IsCalculateToDefined() Then maxX = Math.Min(CalculateTo, maxX)

        Dim IsParameterRiskConstraint As Boolean = Not PM.IsRiskProject AndAlso SelectedConstraintID = Integer.MaxValue
        Dim tCCName As String = ParseString(lblCustomConstraintFunded)
        Dim tCC = RA.Scenarios.ActiveScenario.Constraints.GetConstraintByID(SelectedConstraintID)
        If tCC IsNot Nothing Then
            tCCName = tCC.Name
        End If

        'Return String.Format("['{0}', '{1}', '{2}', {3}, {4}, {5}, {6}]", JS_SafeString(PlotTitle), If(IsParameterRiskConstraint, ResString("lblRARisk"), AxisXTitle), String.Format("{0}, {1}", AxisYTitle, If(ShowDollarValue, System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, "%")), JS_SafeNumber(minX), JS_SafeNumber(maxX), JS_SafeNumber(minY), JS_SafeNumber(maxY))
        Return String.Format("[""{0}"", ""{1}"", ""{2}"", {3}, {4}, {5}, {6}]", JS_SafeString(PlotTitle), If(IsParameterRiskConstraint, ResString("lblRARisk"), If(SelectedConstraintID >= 0, tCCName, AxisXTitle)), AxisYTitle, JS_SafeNumber(minX), JS_SafeNumber(maxX), JS_SafeNumber(minY), JS_SafeNumber(maxY))
    End Function

    Public MinBudget As Double
    Public MaxBudget As Double

    'Public Sub GetSolverParameters(ByRef tMinCost As Double, ByRef tMinCostDiff As Double, ByRef tMaxBudget As Double, ByRef tMinBudget As Double)
    Public Sub GetSolverParameters(ByRef tMaxBudget As Double, ByRef tMinBudget As Double)
        tMinBudget = 0
        tMaxBudget = 0

        'tMinCost = Double.MaxValue
        'tMinCostDiff = Double.MaxValue

        Dim SumScenarioCost As List(Of Double) = New List(Of Double)
        Dim MinScenarioCost As List(Of Double) = New List(Of Double)

        If Not PM.IsRiskProject Then
            'Resource Aligner            
            Dim tSelectedConstraintID As Integer = SelectedConstraintID
            If tSelectedConstraintID >= 0 Then ' CC or Risk
                Dim tScenarios = GetCheckedScenarios()
                If tScenarios.Count > 0 Then
                    Dim tScenario As RAScenario = tScenarios(0)
                    If tSelectedConstraintID = Integer.MaxValue Then
                        'Risk
                        If tScenario.Alternatives IsNot Nothing AndAlso tScenario.Alternatives.Count > 0 Then 
                            tMinBudget = tScenario.Alternatives.Min(Function(a) a.RiskOriginal)
                        End If
                        If tMinBudget > 0 Then tMinBudget = 0 ' allow negative values or 0
                        tMaxBudget = tScenario.Alternatives.Sum(Function(a) a.RiskOriginal)
                    Else
                        'Custom Constraint
                        tMinBudget = 0
                        tMaxBudget = tScenario.Alternatives.Sum(Function (a) If(a.Enabled, 1, 0)) ' Count of enabled alternatives
                        'Dim tCC As RAConstraint = tScenario.Constraints.GetConstraintByID(tSelectedConstraintID)
                        'If tCC IsNot Nothing Then
                        '    'min
                        '    If tCC.MinValueSet Then
                        '        tMinBudget = tCC.MinValue
                        '    Else
                        '        If tScenario.Alternatives IsNot Nothing AndAlso tScenario.Alternatives.Count > 0 Then                                 
                        '            tMinBudget = tScenario.Alternatives.Min(Function(a) If(tCC.GetAlternativeValue(a.ID) = UNDEFINED_INTEGER_VALUE, Integer.MaxValue, tCC.GetAlternativeValue(a.ID)))
                        '        End If
                        '        If tMinBudget = Integer.MaxValue Then tMinBudget = 0
                        '    End If
                        '    'max
                        '    If tCC.MaxValueSet Then
                        '        tMaxBudget = tCC.MaxValue
                        '    Else
                        '        tMaxBudget = tScenario.Alternatives.Sum(Function(a) If(tCC.GetAlternativeValue(a.ID) = UNDEFINED_INTEGER_VALUE, 0, tCC.GetAlternativeValue(a.ID)))
                        '    End If
                        'End If
                    End If
                End If
            Else
                'Budget Limit
                For Each scenario As RAScenario In GetCheckedScenarios()
                    Dim Alts As List(Of RAAlternative) = scenario.Alternatives
                    If Alts IsNot Nothing AndAlso Alts.Count > 0 Then
                        ' max budget
                        SumScenarioCost.Add(Alts.Sum(Function(alt) If(alt.Enabled AndAlso alt.Cost <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, alt.Cost, 0)))
                        MinScenarioCost.Add(Alts.Min(Function(alt) If(alt.Enabled AndAlso alt.Cost <> UNDEFINED_ATTRIBUTE_DEFAULT_DOUBLE_VALUE, If(alt.IsPartial, alt.Cost * alt.MinPercent, alt.Cost), 0)))
                        If MinScenarioCost.Last > 0 Then MinScenarioCost(MinScenarioCost.Count - 1) = 0 ' this is a fix for negative costs - if there is a negative cost we use it, otherwise we use 0
                    End If
                    ' min cost and diff
                    'If Alts.Count > 0 Then
                    '    Dim tCurMin = Alts.Min(Function(alt) If(alt.Cost > 0, alt.Cost, Double.MaxValue))
                    '    If tCurMin > 0 AndAlso tCurMin < tMinCost Then tMinCost = tCurMin
                    '    For i As Integer = 0 To Alts.Count - 2
                    '        For j As Integer = i + 1 To Alts.Count - 1
                    '            Dim tDiff As Double = Math.Abs(Alts(i).Cost - Alts(j).Cost)
                    '            If tDiff > 0 AndAlso tDiff < tMinCostDiff Then tMinCostDiff = tDiff
                    '        Next
                    '    Next
                    'End If
                Next
            End If
        Else
            'Riskion            
            'Dim Controls As List(Of clsControl) = Treatments
            ' max budget
            If Treatments IsNot Nothing AndAlso Treatments.Count > 0 Then 
                SumScenarioCost.Add(Treatments.Sum(Function(ctrl) If(Not ctrl.Enabled OrElse Not ctrl.IsCostDefined, 0, ctrl.Cost)))
            Else
                SumScenarioCost.Add(0)
            End If
            'MinScenarioCost.Add(Controls.Min(Function(ctrl) If(ctrl.Cost = UNDEFINED_INTEGER_VALUE, 0, ctrl.Cost)))
            ' min cost and diff
            'If Controls.Count > 0 Then
            '    Dim tCurMin = Controls.Min(Function(alt) If(alt.Cost > 0, alt.Cost, Double.MaxValue))
            '    If tCurMin > 0 AndAlso tCurMin < tMinCost Then tMinCost = tCurMin
            '    For i As Integer = 0 To Controls.Count - 2
            '        For j As Integer = i + 1 To Controls.Count - 1
            '            Dim tDiff As Double = Math.Abs(Controls(i).Cost - Controls(j).Cost)
            '            If tDiff > 0 AndAlso tDiff < tMinCostDiff Then tMinCostDiff = tDiff
            '        Next
            '    Next
            'End If
        End If

        ' max budget
        If SumScenarioCost.Count > 0 Then
            tMaxBudget = SumScenarioCost.Max()
        End If

        ' min budget
        If MinScenarioCost.Count > 0 Then
            tMinBudget = MinScenarioCost.Min()
        End If

        ' min cost and diff
        'If tMinCost = Double.MaxValue Then tMinCost = 0
        'If tMinCostDiff = Double.MaxValue Then tMinCostDiff = 0

        MinBudget = tMinBudget
        MaxBudget = tMaxBudget
    End Sub

    Private Sub StoreFundedAlts()
        For Each Scenario As RAScenario In RA.Scenarios.Scenarios.Values
            For Each tAlt As RAAlternative In Scenario.Alternatives
                tAlt.FundedOriginal = tAlt.Funded
                tAlt.TmpMust = False
            Next
        Next
    End Sub

    Private Sub RestoreFundedAlts()
        For Each Scenario As RAScenario In RA.Scenarios.Scenarios.Values
            For Each tAlt As RAAlternative In Scenario.Alternatives
                tAlt.Funded = tAlt.FundedOriginal
            Next
        Next
        RA.Save()
    End Sub

    Private Sub StoreActiveControls()
        For Each tControl As clsControl In PM.Controls.Controls
            tControl.ActiveOriginal = tControl.Active
        Next
    End Sub

    Private Sub RestoreActiveControls()
        For Each tControl As clsControl In PM.Controls.Controls
            tControl.Active = tControl.ActiveOriginal
        Next        
    End Sub

    Public Function GetIsCancellationPending() As Boolean
        Return False
    End Function

    Private Sub Ajax_Callback(data As String)
        Dim args As NameValueCollection = HttpUtility.ParseQueryString(data)

        Dim tAction As String = GetParam(args, "action")
        Dim tResult As String = tAction

        If Not String.IsNullOrEmpty(tAction) Then
            Select Case tAction
                Case "grid_mode"
                    gridModeShowProjects = Param2Bool(args, "mode")
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "detailed_tooltips"
                    DetailedTooltips = Param2Bool(args, "value")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "layout_mode"
                    LayoutMode = Param2Int(args, "value")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "title_plot"
                    PlotTitle = GetParam(args, "value")
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetChartSettings())
                Case "title_x"
                    AxisXTitle = GetParam(args, "value")
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetChartSettings())
                Case "title_y"
                    AxisYTitle = GetParam(args, "value")
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetChartSettings())
                Case "zoom_plot"
                    ZoomPlot = Param2Bool(args, "val")
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "keep_funded"
                    KeepFundedAlts = Param2Bool(args, "checked")
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "solve0" ' Mode = 0 - Approx Num of Increments
                    Dim sValue As String = GetParam(args, "value")
                    If String.IsNullOrEmpty(sValue) OrElse Not Integer.TryParse(sValue, NumberOfIncrements) OrElse NumberOfIncrements <= 0 Then NumberOfIncrements = RA_OPT_PARETO_CURVE_DEF_INCREMENTS
                    Mode = EfficientFrontierDeltaType.NumberOfSteps
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "solve1" ' Mode = 1 - Specific amount
                    Dim sValue As String = GetParam(args, "value")
                    If String.IsNullOrEmpty(sValue) OrElse Not String2Double(sValue, SpecifiedIncrement) OrElse SpecifiedIncrement <= 0 Then SpecifiedIncrement = GetDefaultIncrement()
                    Mode = EfficientFrontierDeltaType.DeltaValue
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "solve4" ' Mode = 4 - Specific Decrement (All Solutions)
                    Dim sValue As String = GetParam(args, "value")
                    If String.IsNullOrEmpty(sValue) OrElse Not String2Double(sValue, SpecifiedDecrement) OrElse SpecifiedDecrement <= 0 Then
                        If SpecifiedDecrement <> RA_OPT_PARETO_CURVE_DEF_DECREMENT Then
                            SpecifiedDecrement = RA_OPT_PARETO_CURVE_DEF_DECREMENT
                        End If
                    End If
                    Mode = EfficientFrontierDeltaType.AllSolutions
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "solve7" ' Mode = 7 - Min Benefit Increase
                    Dim sValue As String = GetParam(args, "value")
                    If String.IsNullOrEmpty(sValue) OrElse Not String2Double(sValue, MinBenefitIncrease) OrElse MinBenefitIncrease <= 0 Then MinBenefitIncrease = clsProjectParametersWithDefaults.EfficientFrontierMinBenefitIncreaseDefaultValue
                    Mode = EfficientFrontierDeltaType.MinBenefitIncrease
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "solvermode"
                    Mode = CType(CInt(GetParam(args, "value")), EfficientFrontierDeltaType)
                    Select Case Mode
                        Case EfficientFrontierDeltaType.NumberOfSteps
                            Dim sValue As String = GetParam(args, "num")
                            If String.IsNullOrEmpty(sValue) OrElse Not Integer.TryParse(sValue, NumberOfIncrements) OrElse NumberOfIncrements <= 0 Then NumberOfIncrements = RA_OPT_PARETO_CURVE_DEF_INCREMENTS
                        Case EfficientFrontierDeltaType.DeltaValue
                            Dim sValue As String = GetParam(args, "inc")
                            If String.IsNullOrEmpty(sValue) OrElse Not String2Double(sValue, SpecifiedIncrement) OrElse SpecifiedIncrement <= 0 Then SpecifiedIncrement = GetDefaultIncrement()
                        Case EfficientFrontierDeltaType.AllSolutions
                            Dim sValue As String = GetParam(args, "dec")
                            If String.IsNullOrEmpty(sValue) OrElse Not String2Double(sValue, SpecifiedDecrement) OrElse SpecifiedDecrement <= 0 Then
                                If SpecifiedDecrement <> RA_OPT_PARETO_CURVE_DEF_DECREMENT Then
                                    SpecifiedDecrement = RA_OPT_PARETO_CURVE_DEF_DECREMENT
                                End If
                            End If
                        Case EfficientFrontierDeltaType.MinBenefitIncrease
                            Dim sValue As String = GetParam(args, "delta")
                            If String.IsNullOrEmpty(sValue) OrElse Not String2Double(sValue, MinBenefitIncrease) OrElse MinBenefitIncrease <= 0 Then MinBenefitIncrease = clsProjectParametersWithDefaults.EfficientFrontierMinBenefitIncreaseDefaultValue
                    End Select
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "solve_start" ', "solve_step"
                    'If PM.IsRiskProject AndAlso (CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS) AndAlso Not RA.RiskOptimizer.HasControlsForOptimization(SelectedHierarchyNode) Then 
                    '    tResult = String.Format("[""message"",""{0}""]", JS_SafeString(ParseString("No %%controls%% available for specified " + If(SelectedHierarchyNode.Hierarchy.HierarchyID = ECHierarchyID.hidLikelihood, "%%objectives(l)%%", "%%objectives(i)%%"))))
                    '    Exit Select
                    'End If
                    'Dim tScenarioId As Integer = CInt(GetParam(args, "scenario_id"))
                    Dim oldCacheValue As Boolean = PM.CalculationsManager.PrioritiesCacheManager.Enabled
                    RA.Solver.isEfficientFrontierRunning = True

                    If Not oldCacheValue Then
                        PM.CalculationsManager.PrioritiesCacheManager.ClearCache()
                    End If
                    PM.CalculationsManager.PrioritiesCacheManager.Enabled = True

                    If tAction = "solve_start" Then

                        ' read and clear DB data for this UserID
                        'App.DBTeamTimeDataReadAll(PRJ.ID, ecExtraProperty.IncreasingBudgetsJsonData, App.ActiveUser.UserID, True)
                        'App.DBTeamTimeDataReadAll(PRJ.ID, ecExtraProperty.IncreasingBudgetsJsonData, -App.ActiveUser.UserID, True)

                        RA.Solver.KeepGurobiAlive = True
                        ' Activate all controls
                        'Dim res As Boolean = PM.Controls.SetControlsActive(ControlType.ctCause, True) And PM.Controls.SetControlsActive(ControlType.ctCauseToEvent, True) And PM.Controls.SetControlsActive(ControlType.ctConsequenceToEvent, True)
                        'If res Then WriteControls(PRJ, "")
                        RA.Solver.RiskLimitMin = 0
                        'Dim tScenario = RA.Scenarios.GetScenarioById(tScenarioId)
                        'If tScenario IsNot Nothing AndAlso tScenario.Alternatives IsNot Nothing AndAlso tScenario.Alternatives.Count > 0 Then
                        '    RA.Solver.RiskLimitMin = tScenario.Alternatives.Min(Function(a) a.RiskOriginal)
                        '    if RA.Solver.RiskLimitMin > 0 Then RA.Solver.RiskLimitMin = 0
                        '    For Each alt In tScenario.Alternatives 
                        '        alt.TmpMust = False
                        '        alt.TmpMustNot = False
                        '    Next
                        'End If
                    End If
                    Dim sBudget As String = GetParam(args, "budget")
                    Dim tParameterCustomConstraint As Boolean = Param2Bool(args, "cc")
                    Dim sCalcFrom As String = GetParam(args, "from")
                    Dim solveToken As Integer = CInt(GetParam(args, "solve_token"))
                    If tAction = "solve_start" Then
                        If tParameterCustomConstraint Then 
                            Dim ccBudgetLimit As String = GetParam(args, "ccBudgetLimit")
                            Dim tBudgetLimit As Double
                            If String2Double(ccBudgetLimit, tBudgetLimit) Then
                                If PM.IsRiskProject Then
                                    if RA.RiskOptimizer.BudgetLimit <> tBudgetLimit Then 
                                        RA.RiskOptimizer.BudgetLimit = tBudgetLimit
                                        RA.Save()
                                    End If
                                Else
                                    if RA.Solver.BudgetLimit <> tBudgetLimit Then 
                                        RA.Solver.BudgetLimit = tBudgetLimit
                                        RA.Save()
                                    End If
                                End If
                            End If
                        End If

                        If String.IsNullOrEmpty(sCalcFrom) AndAlso IsCalculateFromDefined Then
                            CalculateFrom = UNDEFINED_INTEGER_VALUE
                        Else
                            Dim tCalcFrom As Double
                            If Not String.IsNullOrEmpty(sCalcFrom) AndAlso String2Double(sCalcFrom, tCalcFrom) Then
                                CalculateFrom = tCalcFrom
                            End If
                        End If
                        Dim sCalcTo As String = GetParam(args, "to")
                        If String.IsNullOrEmpty(sCalcTo) AndAlso IsCalculateToDefined Then
                            CalculateTo = UNDEFINED_INTEGER_VALUE
                        Else
                            Dim tCalcTo As Double
                            If Not String.IsNullOrEmpty(sCalcTo) AndAlso String2Double(sCalcTo, tCalcTo) Then
                                CalculateTo = tCalcTo
                            End If
                        End If
                        'Dim tCC As RAConstraint = Nothing
                        'If tParameterCustomConstraint Then 
                        '    tCC = RA.Scenarios.Scenarios(0).Constraints.GetConstraintByID(SelectedConstraintID)
                        'End If
                        'If tCC IsNot Nothing Then 
                        '    if IsCalculateFromDefined AndAlso tCC.MinValueSet Then CalculateFrom = Math.Max(CalculateFrom, tCC.MinValue)
                        '    if IsCalculateToDefined AndAlso tCC.MaxValueSet Then CalculateTo = Math.Min(CalculateTo, tCC.MaxValue)
                        'End If
                    End If
                    Dim tBudget As Double
                    If String2Double(sBudget, tBudget) Then
                        StoreFundedAlts()
                        StoreActiveControls()

                        Dim retVal As String = ""

                        Dim rand As New Random(DateTime.Now.Millisecond)
                        Dim minX As Double = Integer.MaxValue
                        Dim minY As Double = Integer.MaxValue
                        Dim maxX As Double = Integer.MinValue
                        Dim maxY As Double = Integer.MinValue

                        Dim CheckedScenariosIDs As New List(Of Integer)

                        For Each kvp As KeyValuePair(Of Integer, RAScenario) In RA.Scenarios.Scenarios
                            Dim scenario As RAScenario = kvp.Value
                            If scenario.IsCheckedIB Then
                                CheckedScenariosIDs.Add(kvp.Key)
                            End If
                        Next

                        For Each ScenarioID As Integer In CheckedScenariosIDs
                            Dim scenario As RAScenario = RA.Scenarios.Scenarios(ScenarioID)
                            Dim scVal As String = ""
                            Dim Intervals As New List(Of EfficientFrontierInterval)
                            Dim DeltaValue As Double = SpecifiedIncrement
                            If Mode = EfficientFrontierDeltaType.NumberOfSteps Then DeltaValue = NumberOfIncrements
                            If Mode = EfficientFrontierDeltaType.AllSolutions Then DeltaValue = SpecifiedDecrement
                            If Mode = EfficientFrontierDeltaType.MinBenefitIncrease Then DeltaValue = MinBenefitIncrease
                            Intervals.Add(New EfficientFrontierInterval With {.DeltaType = Mode, .DeltaValue = DeltaValue, .MinValue = If(IsCalculateFromDefined, CalculateFrom, scenario.GetMinCost(SelectedConstraintID)), .MaxValue = If(IsCalculateToDefined, CalculateTo, scenario.GetMaxCost(SelectedConstraintID))})
                            Dim settings As EfficientFrontierSettings = New EfficientFrontierSettings With {.WRTNodeGuid = SelectedHierarchyNodeID, .ConstraintID = SelectedConstraintID, .ConstraintType = If(SelectedConstraintID = -1, EfficientFrontierConstraintType.BudgetLimit, If(SelectedConstraintID = Integer.MaxValue, EfficientFrontierConstraintType.Risk, EfficientFrontierConstraintType.CustomConstraint)), .Intervals = Intervals, .ScenarioID = scenario.ID, .IsIncreasing = PM.Parameters.EfficientFrontierIsIncreasing, .CalculateLECValues = PM.Parameters.EfficientFrontierCalculateLECValues, .RedLineValue = GetRedLineValue(), .GreenLineValue = GreenLineValue, .ScenarioIndex = GetScenarioIndex(scenario.ID), .KeepFundedAlts = KeepFundedAlts AndAlso Mode <> EfficientFrontierDeltaType.AllSolutions AndAlso PM.Parameters.EfficientFrontierIsIncreasing, .SolveToken = solveToken}

                            RA.Solver.EfficientFrontierCallback = AddressOf EfficientFrontierCallback
                            RA.Solver.EfficientFrontierIsCancelledCallback = AddressOf EfficientFrontierGetIsCancelledCallback

                            If Not sampleResults Then
                                If PM.IsRiskProject Then
                                    Dim AllSubObjectivesContributedAlternatives As New HashSet(Of Guid)
                                    If SelectedHierarchyNode IsNot PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0) Then GetAllSubObjectivesContributedAlternativesGuids(SelectedHierarchyNode, AllSubObjectivesContributedAlternatives)
                                    RA.Solver.Solve_Baron_EfficientFrontier(settings, PM.ActiveAlternatives.TerminalNodes.Where(Function(a) a.Enabled AndAlso (SelectedHierarchyNode Is PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0) OrElse AllSubObjectivesContributedAlternatives.Contains(a.NodeGuidID))).Select(Function(x) x.NodeGuidID).ToList, SelectedHierarchyNodeID, Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue OrElse (PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value))
                                Else
                                    Select Case RA_Solver
                                        Case raSolverLibrary.raGurobi
                                            RA.Solver.Solve_Gurobi_EfficientFrontier(settings, , , , , settings.ConstraintType, Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue OrElse (PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value))
                                        Case raSolverLibrary.raXA
                                            RA.Solver.Solve_XA_EfficientFrontier(settings, settings.ConstraintType, Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue OrElse (PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value))
                                        Case raSolverLibrary.raBaron
                                            RA.Solver.Solve_XA_EfficientFrontier(settings, settings.ConstraintType, Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue OrElse (PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value))
                                    End Select
                                End If
                            End If

                            For Each Interval In settings.Intervals
                                If sampleResults Then
                                    Interval.Results = New List(Of EfficientFrontierResults)
                                    Dim x As Double = Interval.MinValue
                                    For i As Integer = 0 To 15
                                        x = ((Interval.MaxValue - Interval.MinValue) / 15) * i
                                        Dim alts As New Dictionary(Of String, Double)
                                        For Each alt As RAAlternative In scenario.Alternatives
                                            alts.Add(alt.ID, If(rand.Next(1000) > 500, 1, 0))
                                        Next
                                        Interval.Results.Add(New EfficientFrontierResults With {.SolverState = raSolverState.raSolved, .FundedCost = rand.NextDouble() * Interval.MaxValue, .FundedBenefits = rand.NextDouble(), .FundedBenefitsOriginal = rand.NextDouble(), .Value = x, .AlternativesData = alts, .ScenarioID = scenario.ID, .ScenarioIndex = GetScenarioIndex(scenario.ID)})
                                    Next
                                End If
                                For Each DP In Interval.Results

                                    If DP.SolverState = raSolverState.raSolved Then
                                        If DP.FundedCost < minX Then minX = DP.FundedCost
                                        If DP.FundedCost > maxX Then maxX = DP.FundedCost
                                        If DP.FundedBenefits < minY Then minY = DP.FundedBenefits
                                        If DP.FundedBenefits > maxY Then maxY = DP.FundedBenefits
                                    End If

                                    scVal += If(scVal = "", "", ",") + EfficientFrontierResults_GetJSON(DP)
                                Next
                            Next
                            retVal += If(retVal = "", "", ",") + String.Format("[{0}]", scVal)
                        Next

                        Dim tSettings As String = "[]"
                        tSettings = GetChartSettings(minX, minY, maxX, maxY)

                        'SolveStep(tScenarioId, tBudget, tParameterCustomConstraint)
                        tResult = String.Format("[""{0}"",[{1}],{2}]", tAction, retVal, tSettings)
                    End If

                    PM.CalculationsManager.PrioritiesCacheManager.Enabled = oldCacheValue
                    If Not oldCacheValue Then
                        PM.CalculationsManager.PrioritiesCacheManager.ClearCache()
                    End If

                    If Not PM.Parameters.EfficientFrontierPlotPointByPoint Then RA.Solver.isEfficientFrontierRunning = False

                    RestoreActiveControls()
                Case "release_solver"
                    RA.Solver.isEfficientFrontierRunning = False
                    tResult = String.Format("[""{0}""]", tAction)
                Case "edit_budget"
                    Dim sValue As String = GetParam(args, "value")
                    Dim dValue As Double = 0
                    Dim res As Boolean = False
                    If String2Double(sValue, dValue) Then
                        If PM.IsRiskProject Then
                            If RA.RiskOptimizer.BudgetLimit <> dValue Then
                                RA.RiskOptimizer.BudgetLimit = dValue
                                RA.Save()
                            End If
                        Else
                            If RA.Solver.BudgetLimit <> dValue Then
                                RA.Solver.BudgetLimit = dValue
                                RA.Save()
                            End If
                        End If
                    End If
                    tResult = String.Format("[""{0}""]", tAction)
                    RA.Solver.KeepGurobiAlive = False
                'Case "solve_stop"
                '    RA.Solver.KeepGurobiAlive = False
                '    RestoreFundedAlts()
                '    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "scenarios_load"
                    Dim isChanged As Boolean = False
                    Dim param_ids = GetParam(args, "ids")
                    Dim ids As String() = param_ids.Split(CChar("-"))
                    If ids.Length > 0 Then
                        Dim cur_ids As String = ""
                        For Each scenario As RAScenario In RA.Scenarios.Scenarios.Values
                            If scenario.IsCheckedIB Then
                                cur_ids += If(cur_ids = "", "", "-") + scenario.ID.ToString
                            End If
                        Next
                        isChanged = param_ids <> cur_ids
                    End If

                    If isChanged Then
                        For Each scenario As RAScenario In RA.Scenarios.Scenarios.Values
                            scenario.IsCheckedIB = False
                        Next

                        If ids.Length > 0 Then
                            For Each id As String In ids
                                Dim tID As Integer = 0
                                If Integer.TryParse(id, tID) AndAlso RA.Scenarios.Scenarios.ContainsKey(tID) Then
                                    RA.Scenarios.Scenarios(tID).IsCheckedIB = True
                                End If
                            Next
                        End If
                        ''if only one scenario is checked make it selected (display in grid) 
                        'Dim chkScenarios = GetCheckedScenarios
                        'If chkScenarios.Count > 0 AndAlso RA.Scenarios.Scenarios.Values.Where(Function(sc) sc.IsCheckedIB AndAlso sc.ID = SelectedScenarioID).Count = 0 Then
                        '    SelectedScenarioID = chkScenarios(0).ID
                        'End If
                        RA.Save()
                    End If
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "scenario_checked"
                    Dim ids As List(Of Integer) = Param2IntList(GetParam(args, "ids"))
                    For each scenario As RAScenario In RA.Scenarios.Scenarios.Values                        
                        scenario.IsCheckedIB = ids.Contains(scenario.ID)
                    Next
                    RA.Save()
                    tResult = String.Format("[""{0}"",{1},""{2}""]", tAction, GetSolverParams(), GetParameterDropdown())
                'Case "grid_scenario_changed"
                '    SelectedScenarioID = CInt(GetParam(args, "id"))
                '    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "option"   ' Solver options
                    Dim sName As String = GetParam(args, "name")
                    Dim sVal As String = GetParam(args, "val")
                    Dim tVal As Integer = -1
                    If sName <> "" AndAlso Integer.TryParse(sVal, tVal) Then
                        Dim chk As Boolean = tVal <> 0
                        If OPT_SHOW_AS_IGNORES Then chk = Not chk
                        SetOptionValue(sName, chk)
                        RA.Save()
                    End If
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "opt_all"
                    SetOptionsAll(Param2Bool(args, "val"))
                    RA.Save()
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "use_ignore_options"
                    Dim chk As Boolean = Param2Bool(args, "val")
                    If PM.IsRiskProject Then 
                        RA.RiskOptimizer.Settings.Musts = Not chk
                        RA.RiskOptimizer.Settings.MustNots = Not chk
                    End If
                    RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions = chk
                    RA.Save()   ' D3240
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "use_base_case_options"
                    RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions = Param2Bool(args, "val")
                    RA.Save()   ' D3240
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "decimals"
                    PM.Parameters.DecimalDigits = CInt(GetParam(args, "value"))
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}"",{1}]", tAction, GetSolverParams())
                Case "select_events"
                    Dim StringSelectedEventIDs As String = GetParam(args, "event_ids")
                    For Each alt As clsNode In PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes
                        alt.Enabled = StringSelectedEventIDs.Contains(alt.NodeGuidID.ToString)
                    Next
                    PRJ.SaveStructure("Save active alternatives")
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "plot_point_by_point"
                    Dim sPlotMode As String = GetParam(args, "value")
                    PM.Parameters.EfficientFrontierPlotPointByPoint = Str2Bool(sPlotMode)
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "is_inc"
                    PM.Parameters.EfficientFrontierIsIncreasing = Str2Bool(GetParam(args, "value"))
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}"",[]]", tAction)
                Case "constraint_id"
                    Dim tValue As Integer = CInt(GetParam(args, "value"))
                    SelectedConstraintID = tValue
                    Dim tCC As RAConstraint = RA.Scenarios.Scenarios(0).Constraints.GetConstraintByID(tValue)
                    Dim tCCName As String = "undefined"
                    'Dim tMin As Double = 0
                    'Dim tMax As Double = 0
                    If tCC IsNot Nothing Then
                        tCCName = tCC.Name
                        'If tCC.MinValueSet Then tMin = tCC.MinValue 
                        'If tCC.MaxValueSet Then tMax = tCC.MaxValue 
                    End If
                    If tValue = Integer.MaxValue Then 'Risk
                        tCCName = ResString("lblRARisk")
                        'tMin = SelectedConstraintMin
                        'tMax = SelectedConstraintMax
                    End If
                    'tResult = String.Format("[""{0}"", {1}, '{2}', {3}, {4}, {5}, {6}]", tAction, SelectedConstraintID, JS_SafeString(tCCName), JS_SafeNumber(tMin), JS_SafeNumber(tMax), GetSolverParams(), GetChartSettings())
                    tResult = String.Format("[""{0}"", {1}, ""{2}"", {3}, {4}]", tAction, SelectedConstraintID, JS_SafeString(tCCName),  GetSolverParams(), GetChartSettings())
                'Case "save_cc_limits"
                '    Dim sMin As String = GetParam(args, "min")
                '    Dim sMax As String = GetParam(args, "max")
                '    Dim tMin As Double = 0
                '    Dim tMax As Double = 0
                '    If String2Double(sMin, tMin) Then 
                '        SelectedConstraintMin = tMin
                '    End If
                '    If String2Double(sMax, tMax) Then 
                '        SelectedConstraintMax = tMax
                '    End If
                '    Dim tCC As RAConstraint = RA.Scenarios.Scenarios(0).Constraints.GetConstraintByID(SelectedConstraintID)
                '    Dim tCCName As String = "undefined"
                '    If tCC IsNot Nothing Then tCCName = tCC.Name
                '    If SelectedConstraintID = Integer.MaxValue Then tCCName = ResString("lblRARisk")
                '    tResult = String.Format("[""{0}"", {1}, '{2}', {3}, {4}, {5}, {6}]", tAction, SelectedConstraintID, JS_SafeString(tCCName), JS_SafeNumber(SelectedConstraintMin), JS_SafeNumber(SelectedConstraintMax), GetSolverParams(), GetChartSettings())
                Case "show_dollar_value"
                    ShowDollarValue = Param2Bool(args, "value")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "plot_parameter"
                    PM.Parameters.EfficientFrontierPlotParameter = GetParam(args, "value").Trim()
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}""]", tAction)
                'Case "show_deltas"
                '    PM.Parameters.EfficientFrontierShowDelta = Param2Bool(args, "value")
                '    PM.Parameters.Save()
                '    tResult = String.Format("[""{0}""]", tAction)
                Case "save_seed"
                    PM.RiskSimulations.RandomSeed = Param2Int(args, "value")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "new_seed"
                    dim tNewSeed As Integer = DateTime.Now.Millisecond
                    PM.RiskSimulations.RandomSeed = tNewSeed
                    tResult = String.Format("[""{0}"", {1}]", tAction, JS_SafeNumber(tNewSeed))
                Case "keep_seed"
                    PM.RiskSimulations.KeepRandomSeed = Param2Bool(args, "value")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "show_cents"
                    ShowCents = Param2Bool(args, "chk")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "num_sim"
                    Dim sVal As String = GetParam(args, "val")
                    Dim iVal As Integer = UNDEFINED_INTEGER_VALUE
                    If Not String.IsNullOrEmpty(sVal) Then
                        If Integer.TryParse(sVal, iVal) Then 'AndAlso LEC.NumberOfSimulations <> iVal AndAlso iVal > 0 AndAlso iVal <= _MAX_NUM_SIMULATIONS Then 
                            PM.RiskSimulations.NumberOfTrials = iVal
                        End If
                    End If
                    tResult = String.Format("[""{0}""]", tAction)
                Case "h_splitter_size"
                    Dim sValue As String = GetParam(args, "value")
                    Dim tValue As Double
                    If String2Double(sValue, tValue) AndAlso HorizontalSplitterSize <> CInt(tValue) Then
                        HorizontalSplitterSize = Convert.ToInt32(tValue)
                    End If
                    tResult = String.Format("[""{0}""]", tAction)
                Case "show_options"
                    ShowOptions = Param2Bool(args, "val")
                    tResult = String.Format("[""{0}""]", tAction)
                Case "use_simulated_values"
                    PM.Parameters.Riskion_Use_Simulated_Values = Param2Int(args, "value")
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}""]", tAction)
                Case "show_lec_values"
                    PM.Parameters.EfficientFrontierCalculateLECValues = Param2Bool(args, "value")
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}""]", tAction)
                Case "red_line"
                    Dim sValue As String = GetParam(args, "value")
                    Dim tValue As Double = 0
                    If String2Double(sValue, tValue) Then 
                        If tValue <> RedLineValue Then RedLineValue = tValue
                    Else
                        RedLineValue = UNDEFINED_INTEGER_VALUE
                    End If
                    tResult = String.Format("[""{0}""]", tAction)
                Case "green_line"
                    Dim sValue As String = GetParam(args, "value")
                    Dim tValue As Double = 0
                    If String2Double(sValue, tValue) Then 
                        If tValue <> GreenLineValue Then GreenLineValue = tValue
                    Else
                        GreenLineValue = UNDEFINED_INTEGER_VALUE
                    End If
                    tResult = String.Format("[""{0}""]", tAction)
                Case "unlim_budget"
                    PM.Parameters.EfficientFrontierUseUnlimitedBudgetForCC = Param2Bool(args, "value")
                    PM.Parameters.Save()
                    tResult = String.Format("[""{0}""]", tAction)                
                Case "objective_function_type"
                    PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType = CType(Param2Int(args, "value"), ObjectiveFunctionType)
                    PM.ResourceAlignerRisk.Save()
                    tResult = String.Format("[""{0}""]", tAction)
                Case "selected_node"
                    Dim sGuid As String = GetParam(args, "value", True)
                    SelectedHierarchyNodeID = New Guid(sGuid)
                    tResult = String.Format("[""{0}""]", tAction)
                Case "diluted_mode"
                    Dim sDilutedMode As String = GetParam(args, "value")
                    If Not String.IsNullOrEmpty(sDilutedMode) Then
                        PM.CalculationsManager.ConsequenceSimulationsMode = CType(CInt(sDilutedMode), ConsequencesSimulationsMode)
                        App.ActiveProject.MakeSnapshot("Consequence simulation mode changed", PM.CalculationsManager.ConsequenceSimulationsMode.ToString)
                    End If
                    tResult = String.Format("[""{0}""]", tAction)
            End Select
        End If

        If tResult <> "" Then
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write(tResult)
            Response.End()
        End If
    End Sub

    Private Sub GetAllSubObjectivesContributedAlternativesGuids(Node As clsNode, ByRef retVal As HashSet(Of Guid))
        If Node IsNot Nothing Then
            Dim ContributedAlternatives As List(Of clsNode) = If(Node.IsTerminalNode, Node.GetContributedAlternatives, Nothing)
            If ContributedAlternatives IsNot Nothing Then
                For Each cAlt In ContributedAlternatives
                    If Not retVal.Contains(cAlt.NodeGuidID) Then retVal.Add(cAlt.NodeGuidID)
                Next
            End If
            If Node.Children IsNot Nothing AndAlso Node.Children.Count > 0 Then
                For Each child In Node.Children
                    GetAllSubObjectivesContributedAlternativesGuids(child, retVal)
                Next
            End If
        End If
    End Sub

    Private Sub SetOptionsAll(chk As Boolean)
        If OPT_SHOW_AS_IGNORES Then chk = Not chk
        Dim settings As RASettings = If(IsSingleScenarioChecked, GetCheckedScenarios(0).Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings)
        With settings
            .Dependencies = chk
            .Groups = chk
            .Musts = chk
            .MustNots = chk
            If Not PM.IsRiskProject Then
                .Risks = chk
                .CustomConstraints = chk
                .FundingPools = chk
                .TimePeriods = chk  ' D3824
            End If
        End With
        RA.Save()   ' D3240
    End Sub

    Private Sub SetOptionValue(sID As String, chk As Boolean)
        Dim settings As RASettings = If(IsSingleScenarioChecked, GetCheckedScenarios(0).Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings)
        With If(PM.IsRiskProject, RA.RiskOptimizer.Settings, settings)
            Select Case sID
                Case "cbOptMusts"
                    .Musts = chk
                Case "cbOptMustNots"
                    .MustNots = chk
                Case "cbOptConstraints"
                    .CustomConstraints = chk
                Case "cbOptDependencies"
                    .Dependencies = chk
                Case "cbOptGroups"
                    .Groups = chk
                Case "cbOptFundingPools"
                    .FundingPools = chk
                Case "cbOptRisks"
                    .Risks = chk
                Case "cbOptTimePeriods" ' D3824
                    .TimePeriods = chk  ' D3824
                Case "cbBaseCase"
                    .UseBaseCase = If(OPT_SHOW_AS_IGNORES, Not chk, chk)
                Case "cbBCGroups"
                    .BaseCaseForGroups = If(OPT_SHOW_AS_IGNORES, Not chk, chk)
            End Select
            RA.Save() ' D3240
        End With
    End Sub

    Public Function GetScenarioIndex(ID As Integer) As Integer 
        Dim retVal = -1
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            If RA.Scenarios.Scenarios(tID).IsCheckedIB Then
                retVal += 1
                If tID = ID Then Return retVal
            End If
        Next
        Return -1
    End Function

    Public Function GetScenariosData() As String
        Dim sRes As String = ""
        Dim ColorPalette As String() = GetPalette(1)
        For Each tID As Integer In RA.Scenarios.Scenarios.Keys
            Dim sc As RAScenario = RA.Scenarios.Scenarios(tID)
            sRes += If(sRes = "", "", ",") + String.Format("[{0},'{1}',{2},'{3}','{4}']", tID, JS_SafeString(sc.Name), JS_SafeNumber(If(sc.IsCheckedIB, 1, 0)), If(String.IsNullOrEmpty(sc.Description), "", JS_SafeString(sc.Description)), ColorPalette(sc.ID Mod ColorPalette.Count))
        Next
        Return sRes
    End Function

    Private Function B(value As Boolean) As String
        Return If(value, "1", "0")
    End Function

    Public Function GetSolverParams() As String
        'Dim tMinCost As Double
        'Dim tMinCostDiff As Double
        Dim tMinBudget As Double
        Dim tMaxBudget As Double
        'GetSolverParameters(tMinCost, tMinCostDiff, tMaxBudget, tMinBudget)
        GetSolverParameters(tMaxBudget, tMinBudget)
        'Return String.Format("{{chk_scenarios : [{0}], minBudget : {1}, maxBudget : {2}, minCost : {3}, maxCostDiff : {4}, mode : {5}, specifiedIncrement : {6}, decimals : {7}, numberOfIncrements : {8}, specifiedDecrement : {9}}}", GetCheckedScenariosToString(), JS_SafeNumber(Math.Round(tMinBudget, tDecimalDigitsCost)), JS_SafeNumber(Math.Round(tMaxBudget, tDecimalDigitsCost)), JS_SafeNumber(Math.Round(tMinCost, tDecimalDigitsCost)), JS_SafeNumber(Math.Round(tMinCostDiff, tDecimalDigitsCost)), CInt(Mode).ToString, JS_SafeNumber(SpecifiedIncrement), tDecimalDigits.ToString, NumberOfIncrements.ToString, JS_SafeNumber(SpecifiedDecrement))
        Dim sMinBudget As String = JS_SafeNumber(Math.Round(tMinBudget, 4))
        Dim sMaxBudget As String = JS_SafeNumber(Math.Round(tMaxBudget, 4))
        If SelectedConstraintID >= 0 Then ' If Risk or CC is selected then no rounding needed
            sMinBudget = JS_SafeNumber(tMinBudget)
            sMaxBudget = JS_SafeNumber(tMaxBudget)
        End If
        Dim sIgnores As String = ""
        Dim sBasecase As String = ""
        Dim settings As RASettings  = If(IsSingleScenarioChecked(), GetCheckedScenarios(0).Settings, RA.Scenarios.GlobalSettings.ScenarioComparisonSettings)
        With Settings 'm, mn, cc, dp, gr, fp, r, tp
            sIgnores = String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", B(.Musts), B(.MustNots), B(.CustomConstraints), B(.Dependencies), B(.Groups), B(.FundingPools), B(.Risks), B(.TimePeriods))
            sBasecase = String.Format("{0},{1},{2}", B(.UseBaseCaseOptions), B(.UseBaseCase), B(.BaseCaseForGroups))
        End With
        Return String.Format("{{""chk_scenarios"" : [{0}], ""minBudget"" : {1}, ""maxBudget"" : {2}, ""mode"" : {3}, ""specifiedIncrement"" : {4}, ""decimals"" : {5}, ""numberOfIncrements"" : {6}, ""specifiedDecrement"" : {7}, ""ignores"": [{8}], ""basecase"":[{9}]}}", GetCheckedScenariosToString(), sMinBudget, sMaxBudget, CInt(Mode).ToString, JS_SafeNumber(SpecifiedIncrement), PM.Parameters.DecimalDigits, NumberOfIncrements.ToString, JS_SafeNumber(SpecifiedDecrement), sIgnores, sBasecase)
    End Function

    Public function EfficientFrontierResults_GetJSON(DP As EfficientFrontierResults) As String
        Dim sFundedAltsHash As String = "" ' not needed with current approach
        Dim sFundedAltsArray As String = ""
        For Each alt As KeyValuePair(Of String, Double) In DP.AlternativesData
            If alt.Value > 0 Then
                sFundedAltsArray += If(sFundedAltsArray = "", "", ",") + String.Format("[""{0}"",{1}]", alt.Key, JS_SafeNumber(alt.Value))
            End If
        Next
        sFundedAltsArray = String.Format("[{0}]", sFundedAltsArray)

        Return String.Format("[{0}, {1}, {2}, {3}, ""{4}"",{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}]", Bool2JS(If(PM.IsRiskProject, True, DP.SolverState = raSolverState.raSolved)), JS_SafeNumber(Math.Round(If(SelectedConstraintID = -1, DP.FundedCost, DP.AlternativesData.Where(Function (a) a.Value > 0).Count), 4)), JS_SafeNumber(Math.Round(DP.FundedBenefits, PM.Parameters.DecimalDigits + 4)), JS_SafeNumber(Math.Round(DP.Value, PM.Parameters.DecimalDigits + 4)), sFundedAltsHash, sFundedAltsArray, DP.ScenarioID, JS_SafeNumber(Math.Round(DP.DeltaValue, PM.Parameters.DecimalDigits + 4)), JS_SafeNumber(Math.Round(DP.DeltaMonetaryValue, 4)), JS_SafeNumber(Math.Round(DP.DeltaCost, 4)), JS_SafeNumber(Math.Round(DP.DeltaLeverage, PM.Parameters.DecimalDigits + 4)), 0, JS_SafeNumber(Math.Round(DP.RiskReductionGlobal, PM.Parameters.DecimalDigits + 4)), JS_SafeNumber(Math.Round(DP.RiskReductionMonetaryGlobal, 4)), JS_SafeNumber(Math.Round(DP.LeverageGlobal, PM.Parameters.DecimalDigits + 4)), JS_SafeNumber(Math.Round(DP.ExpectedSavings, 4)), JS_SafeNumber(Math.Round(DP.DeltaExpectedSavings, 4)), JS_SafeNumber(Math.Round(DP.RedLineIntersection, PM.Parameters.DecimalDigits + 4)), JS_SafeNumber(Math.Round(DP.GreenLineIntersection, PM.Parameters.DecimalDigits + 4)), DP.ScenarioIndex, JS_SafeNumber(Math.Round(DP.FundedBenefits * DollarValueOfEnterprise, 4)), DP.SolveToken)
    End Function

    Private Sub DBAppendAction(DP As EfficientFrontierResults, Progress As Integer)
        Dim curDT As Long = DateTime.Now.Ticks
        Dim sData As String = String.Format("[""{0}"",{1},{2},{3}]", curDT, Progress, If(DP Is Nothing, "[""finished""]", EfficientFrontierResults_GetJSON(DP)), If(DP IsNot Nothing, DP.SolveToken, 0))
        App.DBTeamTimeDataWrite(App.ProjectID, App.ActiveUser.UserID, ecExtraProperty.IncreasingBudgetsJsonData, sData, False)
    End Sub

    Private Sub EfficientFrontierCallback(StepResult As EfficientFrontierResults, Progress As Integer, ByRef RefIsCancelled As Boolean)
        'Write a datapoint data to DB
        DBAppendAction(StepResult, Progress)

        'Check DB for a "Cancel" command
        Dim sData As List(Of String) = App.DBTeamTimeDataReadAll(PRJ.ID, ecExtraProperty.IncreasingBudgetsJsonData, -App.ActiveUser.UserID, True)
        Dim isCancelled As Boolean = sData IsNot Nothing AndAlso sData.Count > 0
        If isCancelled Then
            RA.Solver.isEfficientFrontierRunning = False
        End If
        If Not RefIsCancelled Then RefIsCancelled = isCancelled
    End Sub

    Private Sub EfficientFrontierGetIsCancelledCallback(ByRef RefIsCancelled As Boolean)
        'Check DB for a "Cancel" command
        Dim sData As List(Of String) = App.DBTeamTimeDataReadAll(PRJ.ID, ecExtraProperty.IncreasingBudgetsJsonData, -App.ActiveUser.UserID, True)
        Dim isCancelled As Boolean = sData IsNot Nothing AndAlso sData.Count > 0
        If Not RefIsCancelled Then RefIsCancelled = isCancelled
    End Sub

End Class