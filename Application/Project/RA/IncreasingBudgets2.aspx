<%@ Page Language="VB" Codebehind="IncreasingBudgets2.aspx.vb" Inherits="RAIncreasingBudgetsPage2" title="Resource Aligner - Efficient Frontier" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
<script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/dataTables.buttons.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/dataTables.fixedColumns.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/buttons.html5.min.js"></script><%--
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/buttons.print.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/buttons.colVis.min.js"></script>--%>
<style type="text/css">
    table.dataTable tbody th, table.dataTable tbody td {
        border-right: 1px solid #ccc;
        /*max-width: 100px;*/
    }
</style>
<script language="javascript" type="text/javascript">

    ping_active = false;

    var optionsVisible = true;

    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    var decimals = <% = PM.Parameters.DecimalDigits %>;
    var showCents = <% = Bool2JS(ShowCents) %>;
    var plot_point_by_point = <% = Bool2JS(Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue OrElse (PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value)) %>;
    var isRisk = <% = Bool2JS(PM.IsRiskProject) %>;
    var msgSolving = "<% = ResString(If(PRJ.IsRisk, "msgEfficientFrontierRunningRiskion", "msgEfficientFrontierRunningComparion")) %>";
    var solve_token;

    var imNumOfIncrements = 0;
    var imSpecifiedIncrement = 1;
    var imMinCost = 2;
    var imMinDifferenceOfCosts = 3;
    var imAllSolutions = 4;
    var imIncreasing = 5;
    var imMinBenefitIncrease = 7;

    // GridRowModes
    var gmFundedMarks = 1;
    var gmAltNames = 2;

    var is_decreasing = false;

    var OPT_SCENARIO_LEN     = 45;
    var OPT_DESCRIPTION_LEN = 200;

    //var scenario_index = -1;
    var ccRiskID = <%=JS_SafeNumber(Integer.MaxValue)%>;
    
    var settings = <%=GetSolverParams%>;
    var gridModeShowProjects = <%=Bool2JS(gridModeShowProjects)%>;    
    var detailedTooltips = <%=Bool2JS(DetailedTooltips)%>;
    var layoutMode = 0; <% = CInt(LayoutMode) %>; //0 - Both, 1 - Grid only, 2 - Chart only
    var tooltipMaxZIndex = 100;
    var sEventIDs    = "<% = StringSelectedEventIDs%>";
    var dilutedMode = <% = CInt(PM.CalculationsManager.ConsequenceSimulationsMode)%>;

    var scenarios = [<% = GetScenariosData() %>];
    var IDX_ID   = 0;
    var IDX_NAME = 1;
    var IDX_CHK  = 2;
    var IDX_DESCRIPTION = 3; 
    var IDX_COLOR = 4; 

    <%--var scenario_id    = <%= SelectedScenarioID() %>;--%>     //selected scenario Id

    var chart_data = [];
    var last_pointIndex = -1;
    //var lastFundedAlts = "";
    //var is_solving = false;

    var pagination = 10;

    var calculateFrom = <%=JS_SafeNumber(CalculateFrom)%>;
    var calculateTo = <%=JS_SafeNumber(CalculateTo)%>;
    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;

    var cur_increment = "<% = Math.Round(SpecifiedIncrement, 2)%>";
    var cur_decrement = "<% = Math.Round(SpecifiedDecrement, 2)%>";
    var cur_num_inc   = "<% = NumberOfIncrements%>";
    var cur_min_benefit_increase = <% = JS_SafeNumber(MinBenefitIncrease)%>;
    var cur_cmd = "";
    var objectiveFunctionType = <%=CInt(PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType)%>;
    
    var is_context_menu_open = false;
    var scenarios_checked = "";

    var dlg_select_events = null;
    var cancelled = false;
    
    var event_list = <%=GetEventsData()%>;
    var IDX_EVENT_ID = 0;
    var IDX_EVENT_NAME = 1;
    var IDX_EVENT_ENABLED = 2;

    var controls_data = <%=GetControlsData()%>;
    var IDX_CONTROL_ID = 0;
    var IDX_CONTROL_NAME = 1;
    var IDX_CONTROL_INDEX = 2;

    var hierarchy_data = <% = GetHierarchyData() %>;
    var hierarchy_columns = <% = GetHierarchyColumns() %>;
    var selectedHierarchyNodeID = "<% = SelectedHierarchyNodeID.ToString %>";

    var originalDatapoints = [];

    /* Datatables datagrid */
    var datagrid   = null;
    //var cur_order  = [0, "asc"];
    var datagrid_data = [];
    var datagrid_data_marks = [];
    var datagrid_data_names = [];
    var datagrid_columns = [];
    //var visibleColumnsCount = 0;
    
    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_CLASS = 3;
    var COL_COLUMN_TYPE = 4;

    /* chart options */
    var showLines  = true;
    var showLabels = true;
    var chartTitle = "";
    var xTitle = "";
    var yTitle = "";
    var checked_scenarios = [];

    var chart_settings = <%=GetChartSettings()%>;
    var zoom_plot = false; <%--<%=Bool2JS(ZoomPlot)%>;--%>
    var is_risk = <%=Bool2JS(PM.IsRiskProject)%>;
    var Use_Simulated_Values = <% = CInt(PM.Parameters.Riskion_Use_Simulated_Values) %>;
    //var canShowTooltip = true;

    var solverMode = <% = CInt(Mode) %>;

    var IDX_CHART_SETTINGS = 0; // == chart_settings
    var IDX_CHART_DATA     = 1;
    var IDX_CHART_LABELS   = 2;
    var IDX_CHART_LABELS_WITH_CHECKBOXES  = 5;

    var IDX_SOLVER_SOLVED = 0;
    var IDX_SOLVER_FUNDED_COST = 1;
    var IDX_SOLVER_BENEFIT = 2;
    var IDX_SOLVER_BUDGET = 3;
    var IDX_SOLVER_FUNDED_ALTS_HASH = 4;
    var IDX_SOLVER_FUNDED_ALTS = 5;
    var IDX_SOLVER_CUR_SCENARIO_ID = 6;
    var IDX_SOLVER_DELTA_RISK = 7;
    var IDX_SOLVER_DELTA_RISK_MONETARY = 8;
    var IDX_SOLVER_DELTA_COST = 9;
    var IDX_SOLVER_RISK_DELTA_LEVERAGE = 10;
    var IDX_SOLVER_UNDEFINED_CAN_BE_REASSIGNED = 11; // 0 cell, can be reused
    var IDX_SOLVER_RISK_REDUCTION_GLOBAL = 12;
    var IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY = 13;
    var IDX_SOLVER_RISK_LEVERAGE_GLOBAL = 14;
    var IDX_SOLVER_RISK_EXPECTED_SAVINGS = 15;
    var IDX_SOLVER_RISK_DELTA_EXPECTED_SAVINGS = 16;
    var IDX_SOLVER_LOSS_EXCEEDANCE = 17;
    var IDX_SOLVER_LIKELIHOOD_OF_EXCEEDING = 18;
    var IDX_SOLVER_CUR_SCENARIO_INDEX = 19;
    var IDX_SOLVER_BENEFIT_MONETARY = 20;
    var IDX_SOLVER_OVERALL_POINT_INDEX = 21;

    var overall_points_count = 0;
    var showDollarValue = <%=Bool2JS(ShowDollarValue)%>;
    var dollarValueOfEnterprise = <%=JS_SafeNumber(DollarValueOfEnterprise)%>;
    var CHAR_CURRENCY = "<%=JS_SafeString(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol)%>";
    
    <%--var showRiskDeltas = true; <%=Bool2JS(PM.Parameters.EfficientFrontierShowDelta)%>;--%>
    var plotParameter = "<%=If(PM.IsRiskProject, PM.Parameters.EfficientFrontierPlotParameter, "priority")%>";
    var showLECValues = <%=Bool2JS(PM.IsRiskProject AndAlso PM.Parameters.EfficientFrontierCalculateLECValues)%>;
    var numSimulations = <%=PM.RiskSimulations.NumberOfTrials%>;
    var keepSeed = <% = Bool2JS(PM.RiskSimulations.KeepRandomSeed) %>;
    var red_line_value = <%=JS_SafeNumber(RedLineValue)%>;    
    var green_line_value = <%=JS_SafeNumber(GreenLineValue)%>;
    var horizontalSplitterSize = <%=JS_SafeNumber(HorizontalSplitterSize)%>;
    var isPleaseWait = false;

    var startTime;
    var endTime;

    /* Callback */
    function sendCommand(params, show_please_wait) {
        if (show_please_wait) showLoadingPanel();
        cmd = params;

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function syncReceived(params) {
        var received_data = ["",[]];
        
        if ((params)) {
            received_data = JSON.parse(params);
            if (received_data[0] == "update") {
                InitScenarios();
                drawChart();
                //switchIgnores();
                //switchBaseCase();                
            }            

            //if (received_data[0] == "scenario_checked" || received_data[0] == "scenarios_load" || received_data[0] == "grid_scenario_changed" || received_data[0] == "grid_mode" || received_data[0] == "decimals" || received_data[0] == "keep" || received_data[0] == "solve0" || received_data[0] == "solve1" || received_data[0] == "solve4" || received_data[0] == "solvermode") {
            if (received_data[0] == "scenario_checked" || received_data[0] == "scenarios_load" || received_data[0] == "grid_mode" || received_data[0] == "decimals" || received_data[0] == "keep_funded" || received_data[0] == "solve0" || received_data[0] == "solve1" || received_data[0] == "solve4"  || received_data[0] == "solve7" || received_data[0] == "solvermode") {
                settings = received_data[1];                

                updateKeepFundedSwitch();
                
                // update the custom constraints dropdown
                cc_dropdown = received_data[2];
                $("#divCCDropdown").html(cc_dropdown);
                if (document.getElementById("cbParameter")) toggleExtraBudgetVisibility(document.getElementById("cbParameter").options[document.getElementById("cbParameter").selectedIndex].value);

                // show or hide the active scenario dropdown
                //toggleScenariosVisibility();

                // update Ignores checked state
                initIgnores();
                initBaseCase();
            }

            if (received_data[0] == "title_plot" || received_data[0] == "title_x" || received_data[0] == "title_y") {
                chart_settings = received_data[1];
            }

            if (received_data[0] == "zoom_plot") {
                updateChart();
            }

            if (received_data[0] == "green_line") {
                solveStart();
                return;
            }
            
            if (received_data[0] == "show_dollar_value" || received_data[0] == "show_lec_values" || received_data[0] == "show_cents" || received_data[0] == "decimals") {                
                if (showLECValues) {
                    //solveStart();
                    //return;
                } else {
                    refreshGridAndChart();
                    refreshLEClabels();
                }
            }

            if (received_data[0] == "solvermode") {
                $("#cbDecreasing").prop("disabled", !(solverMode == 0 || solverMode == 1));
            }

            if (received_data[0] == "new_seed") {
                $("#tbSeed").val(received_data[1]);
            }

        }

        var controls_len = controls_data.length;
        
        if (received_data[0] == "solve_start" && !plot_point_by_point) {
            chart_settings = received_data[2];
            var dp = received_data[1];
            //if (isRevertedOrderOfDatapoints() && dp[IDX_SOLVER_SOLVED]) {
            //    chart_settings[5] = dp[IDX_SOLVER_BENEFIT] * 0.95; // min Y when zooming is On - solving with maxBudget and getting the best benefit (minimum risk)
            //    if (chart_settings[5] < 0) chart_settings[5] = 0;
            //}
            
            //preInitGrid();
        
            //}
            //
            //if (received_data[0] == "solve_start" || received_data[0] == "solve_step") {

            var scenarios = received_data[1];
            curScenarioIndex = -1;
            for (var k = 0; k < scenarios.length; k++) {
                curScenarioIndex += 1;
                var datapoints = scenarios[k];
                for (var v = 0; v < datapoints.length; v++) {
                    var dp = datapoints[v];
                    <%--if (dp[IDX_SOLVER_SOLVED] && (lastFundedAlts !== dp[IDX_SOLVER_FUNDED_ALTS_HASH] <% If App.isRiskEnabled Then %>|| dp[IDX_SOLVER_FUNDED_COST] == 0<%End If%>)) {--%>
                    if (dp[2] >= 0 && dp[21] == solve_token) {
                        originalDatapoints.push(dp);
                        addDatapoint(dp);
                    }
                }
            }
            
      
            //if (dp[IDX_SOLVER_SOLVED] && settings.mode == imAllSolutions) {
            //    curBudget = dp[IDX_SOLVER_FUNDED_COST];
            //}

            //$("input.cb_scenario_shown").prop("disabled", true);
           
            //$("#imgLoading").hide();
            solveStop(false);
            //if (is_solving) {                
            //    proceedSolve();
            //} else {
            //    $("#btnSolve").prop("disabled", true);
            //    solveStop(true);
            //}
        }

        if (received_data[0] == "message") {
            solveStop(false);
            dxDialog(received_data[1], function(){}, null, "Error");
        }

        if (received_data[0] == "solve_start") {           
            if (!plot_point_by_point) {
                updateDatagrid();
                updateChart();

                // todo: 
                dataTablesInit(datagrid);
                setTimeout("resizePage();", 100);
            }

            $("#tdPinAll").css("display", "block");

            endTime = new Date();
            var timeDiff = endTime - startTime;
            timeDiff /= 1000;
            var seconds = timeDiff; //Math.round(timeDiff);
            $("#divTimeElapsed").html("<%=ResString("lblRASolvedFor")%>" + " " + seconds + " s.");
            solveStop(false);
        }

        if (received_data[0] == "constraint_id") { //|| received_data[0] == "save_cc_limits") {
            ccId = received_data[1]*1;
            ccName = received_data[2];
            //tMin = received_data[3]*1;
            //tMax = received_data[4]*1;
            settings = received_data[3];
            chart_settings = received_data[4];

            isParameterCC = ccId >= 0;

            // show editor window if changed ccId
            //if (received_data[0] == "constraint_id" && ccId >= 0) {
            //    onEditCCLimits(ccId, ccName, tMin, tMax);                
            //}
        }

        if (received_data[0] == "show_options") {

        }

        if (received_data[0] == "selected_node") {
            solveStart();
            return;
        }

        if (received_data[0] == "plot_parameter") {
            refreshGridAndChart();
        }

        updateButtons();
        updateToolbar();
        hideLoadingPanel();
    }

    function updateKeepFundedSwitch() {
        $("#cbKeepFunded").attr("disabled", isRevertedOrderOfDatapoints());
    }

    function isRevertedOrderOfDatapoints() {
        return plot_point_by_point && settings.mode == imAllSolutions;
        return false;
    }

    var summaryRowsCount = 0;

    function preInitGridAndChart() {
        var controls_len = controls_data.length;

        overall_points_count = 0;   
        chart_data = [];
        
        createChart();

        // init datagrid
        summaryRowsCount = 0;
        datagrid_data = [];
        datagrid_data_marks = [];
        datagrid_data_names = [];

        // 1
        datagrid_data_marks.push(["", isParameterCC ? "<%=ParseString(lblCustomConstraintFunded)%>" : (is_risk ? "<%=ResString("tblRowRiskGlobal")%>" + (showDollarValue ? ", $" : ", %") : "<%=ResString("tblRowFundedCost")%>")]);
        datagrid_data_names.push(["", isParameterCC ? "<%=ParseString(lblCustomConstraintFunded)%>" : (is_risk ? "<%=ResString("tblRowRiskGlobal")%>" + (showDollarValue ? ", $" : ", %") : "<%=ResString("tblRowFundedCost")%>")]);
        summaryRowsCount += 1;

        if (is_risk) {                        
            // 4
            datagrid_data_marks.push(["", "<%=ResString("tblRowFundedCost")%>"]);
            datagrid_data_names.push(["", "<%=ResString("tblRowFundedCost")%>"]);
            summaryRowsCount += 1;
            
            // 3
            if (showDollarValue) {
                datagrid_data_marks.push(["","<%=ResString("tblRowSavings")%>, $"]);
                datagrid_data_names.push(["","<%=ResString("tblRowSavings")%>, $"]);
                summaryRowsCount += 1;
            }

            // 7
            datagrid_data_marks.push(["","<%=ResString("tblRowLeverageGlobal")%>"]);
            datagrid_data_names.push(["","<%=ResString("tblRowLeverageGlobal")%>"]);
            summaryRowsCount += 1;

            if (true || dilutedMode !== 1) { // show only if "Diluted"
                // 2
                datagrid_data_marks.push(["", "<%=ResString("tblRowRiskWithCtrls")%>" + (showDollarValue ? ", $" : ", %")]);
                datagrid_data_names.push(["", "<%=ResString("tblRowRiskWithCtrls")%>" + (showDollarValue ? ", $" : ", %")]);
                summaryRowsCount += 1;
            }
        } else {
            // 4
            datagrid_data_marks.push(["", "<%=ResString("tblRowBenefit")%>"]);
            datagrid_data_names.push(["", "<%=ResString("tblRowBenefit")%>"]);
            summaryRowsCount += 1;
        }        

        if (is_risk) {
            if (showLECValues && (Use_Simulated_Values > 0)) {
                // 5
                datagrid_data_marks.push(["","<span class='lblPercentLEC'>5% loss exceedance</span>"]);
                datagrid_data_names.push(["","<span class='lblPercentLEC'>5% loss exceedance</span>"]);
                summaryRowsCount += 1;

                // 6
                var green_line_text = green_line_value == UNDEFINED_INTEGER_VALUE ? "" : showDollarValue ? Math.round(green_line_value * dollarValueOfEnterprise) : Math.round(green_line_value * 100);
                datagrid_data_marks.push(["","<span class='lblLikelihoodLEC'>Likelihood of exceeding $" + green_line_text + "</span>"]);
                datagrid_data_names.push(["","<span class='lblLikelihoodLEC'>Likelihood of exceeding $" + green_line_text + "</span>"]);                    
                summaryRowsCount += 1;
            }            
            
            if (showAllRows) {
                // 8
                datagrid_data_marks.push(["","<%=ResString("tblRowDeltaLeverage")%>"]);
                datagrid_data_names.push(["","<%=ResString("tblRowDeltaLeverage")%>"]);                    
                summaryRowsCount += 1;

                // 9
                datagrid_data_marks.push(["","<%=ResString("tblRowDeltaRisk")%>" + (showDollarValue ? ", $" : ", %")]);
                datagrid_data_names.push(["","<%=ResString("tblRowDeltaRisk")%>" + (showDollarValue ? ", $" : ", %")]);
                summaryRowsCount += 1;

                // 10
                datagrid_data_marks.push(["","<%=ResString("tblRowDeltaCost")%>, $"]);
                datagrid_data_names.push(["","<%=ResString("tblRowDeltaCost")%>, $"]);
                summaryRowsCount += 1;

                // 11
                if (showDollarValue) {
                    datagrid_data_marks.push(["","<%=ResString("tblRowDeltaSavings")%>, $"]);
                    datagrid_data_names.push(["","<%=ResString("tblRowDeltaSavings")%>, $"]);
                    summaryRowsCount += 1;
                }
            }
        }

        datagrid_data_names.push(["","<%=If(PM.IsRiskProject, ResString("tblRowFundedControls"), ResString("tblRowFundedAlts"))%>"]); // last row
        
        for (var i = 0; i < controls_len; i++) {
            datagrid_data_marks.push([controls_data[i][IDX_CONTROL_INDEX], controls_data[i][IDX_CONTROL_NAME]]);
        }
        
        datagrid_columns = [];
        datagrid_columns.push([0, "<%=If(PM.IsRiskProject OrElse PM.Parameters.NodeVisibleIndexMode = IDColumnModes.IndexID, ResString("optIndexID"), ResString("optUniqueID"))%>", true, "text_bold dt-body-right", ""]);
        datagrid_columns.push([1, (isParameterCC ? "<%=ParseString(If(PM.IsRiskProject, "%%Controls%%", "%%Alternatives%%") + "/Custom Constraint Upper Bound")%>" : "<%=If(PM.IsRiskProject, ParseString("%%Controls%%/Budget"), ResString("tblAlternativesBudget"))%>"), true, "text_bold dt-body-left", "html"]);

        //is_solving = false;
    }

    var showAllRows = false;

    function addDatapoint(dp) {        
        var controls_len = controls_data.length;
        var idx = 0;
        curScenarioIndex = dp[IDX_SOLVER_CUR_SCENARIO_INDEX];

        if (dp[IDX_SOLVER_SOLVED]) {
            if (dp.length <= IDX_SOLVER_OVERALL_POINT_INDEX) {
                dp.push(overall_points_count);
            }
            overall_points_count += 1;            

            var cd = [];
            cd.push(dp[IDX_SOLVER_FUNDED_COST]);
            switch (plotParameter) {
                case "priority":
                    cd.push(dp[IDX_SOLVER_BENEFIT]);
                    break;
                case "leverage":
                    cd.push(dp[IDX_SOLVER_RISK_LEVERAGE_GLOBAL]);
                    break;
                case "delta_leverage":
                    cd.push(dp[IDX_SOLVER_RISK_DELTA_LEVERAGE]);
                    break;
                case "savings":
                    cd.push(dp[IDX_SOLVER_RISK_EXPECTED_SAVINGS]);
                    break;
                case "delta_savings":
                    cd.push(dp[IDX_SOLVER_RISK_DELTA_EXPECTED_SAVINGS]);
                    break;
                case "percent_lec":
                    cd.push(dp[IDX_SOLVER_LOSS_EXCEEDANCE]);
                    break;
                case "likelihood_lec":
                    cd.push(dp[IDX_SOLVER_LIKELIHOOD_OF_EXCEEDING]);
                    break;
            } 
            cd.push(showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents));
            cd.push(dp);

            var chart_dp = { "cost" : dp[IDX_SOLVER_FUNDED_COST], "dcost" : cd[2], "budget" : dp[IDX_SOLVER_BUDGET], "overall_point_index" : overall_points_count - 1 };
            chart_dp["s" + curScenarioIndex] = cd[1];
            chart_dp["d" + curScenarioIndex] = cd[1] * dollarValueOfEnterprise;            
            chart_data.push(chart_dp);
           
            if (isRevertedOrderOfDatapoints()) {
                // add column to datagrid
                datagrid_columns.splice(summaryRowsCount, 0, [datagrid_columns.length, isParameterCC ? dp[IDX_SOLVER_BUDGET] : showCost(dp[IDX_SOLVER_BUDGET], true, !showCents), true, "dt-right", ""]);

                // add datapoint to datagrid
                idx = 0;

                // 1
                datagrid_data_marks[idx].splice(summaryRowsCount, 0, isParameterCC ? dp[IDX_SOLVER_FUNDED_COST] : (is_risk ? (showDollarValue ? showCost(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL], decimals)) : showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents)));
                datagrid_data_names[idx].splice(summaryRowsCount, 0, isParameterCC ? dp[IDX_SOLVER_FUNDED_COST] : (is_risk ? (showDollarValue ? showCost(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL], decimals)) : showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents)));
                idx += 1;
               
                if (is_risk) {                                        
                    // 2
                    datagrid_data_marks[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents));
                    datagrid_data_names[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents));
                    idx += 1;

                    if (showDollarValue) {
                        // 3
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_RISK_EXPECTED_SAVINGS], true, !showCents));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_RISK_EXPECTED_SAVINGS], true, !showCents));
                        idx += 1;
                    }

                    // 7
                    datagrid_data_marks[idx].splice(summaryRowsCount, 0, dp[IDX_SOLVER_RISK_LEVERAGE_GLOBAL].toFixed(decimals) + " : 1");
                    datagrid_data_names[idx].splice(summaryRowsCount, 0, dp[IDX_SOLVER_RISK_LEVERAGE_GLOBAL].toFixed(decimals) + " : 1");
                    idx += 1;
                    
                    if (true || dilutedMode !== 1) { // show only if "Diluted"
                        // 4
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, showDollarValue ? showCost(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, showDollarValue ? showCost(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                        idx += 1;
                    }
                } else {
                    // 4
                    datagrid_data_marks[idx].splice(summaryRowsCount, 0, num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                    datagrid_data_names[idx].splice(summaryRowsCount, 0, num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                    idx += 1;
                }

                if (is_risk) {                               
                    if (showLECValues && (Use_Simulated_Values > 0)) {
                        // 5
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, showDollarValue ? showCost(dp[IDX_SOLVER_LOSS_EXCEEDANCE] * dollarValueOfEnterprise, true, !showCents) : num2prc(dp[IDX_SOLVER_LOSS_EXCEEDANCE], decimals));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, showDollarValue ? showCost(dp[IDX_SOLVER_LOSS_EXCEEDANCE] * dollarValueOfEnterprise, true, !showCents) : num2prc(dp[IDX_SOLVER_LOSS_EXCEEDANCE], decimals));
                        idx += 1;

                        // 6
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, num2prc(dp[IDX_SOLVER_LIKELIHOOD_OF_EXCEEDING], decimals, true));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, num2prc(dp[IDX_SOLVER_LIKELIHOOD_OF_EXCEEDING], decimals, true));
                        idx += 1;
                    }                    
                    
                    if (showAllRows) {
                        // 8
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, dp[IDX_SOLVER_RISK_DELTA_LEVERAGE].toFixed(decimals));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, dp[IDX_SOLVER_RISK_DELTA_LEVERAGE].toFixed(decimals));
                        idx += 1;

                        // 9
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, showDollarValue ? showCost(dp[IDX_SOLVER_BENEFIT_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_DELTA_RISK], decimals));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, showDollarValue ? showCost(dp[IDX_SOLVER_BENEFIT_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_DELTA_RISK], decimals));
                        idx += 1;

                        // 10
                        datagrid_data_marks[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_DELTA_COST], true, !showCents));
                        datagrid_data_names[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_DELTA_COST], true, !showCents));
                        idx += 1;

                        if (showDollarValue) {
                            // 11
                            datagrid_data_marks[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_RISK_DELTA_EXPECTED_SAVINGS], true, !showCents));
                            datagrid_data_names[idx].splice(summaryRowsCount, 0, showCost(dp[IDX_SOLVER_RISK_DELTA_EXPECTED_SAVINGS], true, !showCents));
                            idx += 1;
                        }                 
                    }
                }
            } else {
                // add column to datagrid
                datagrid_columns.push([datagrid_columns.length, isParameterCC ? dp[IDX_SOLVER_BUDGET] : showCost(dp[IDX_SOLVER_BUDGET], true, !showCents), true, "dt-right", ""]);
                
                // add datapoint to datagrid
                idx = 0;

                // 1
                datagrid_data_marks[idx].push(isParameterCC ? dp[IDX_SOLVER_FUNDED_COST] : (is_risk ? (showDollarValue ? showCost(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL], decimals)) : showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents)));
                datagrid_data_names[idx].push(isParameterCC ? dp[IDX_SOLVER_FUNDED_COST] : (is_risk ? (showDollarValue ? showCost(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_RISK_REDUCTION_GLOBAL], decimals)) : showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents)));
                idx += 1; 

                if (is_risk) {                                        
                    // 4
                    datagrid_data_marks[idx].push(showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents));
                    datagrid_data_names[idx].push(showCost(dp[IDX_SOLVER_FUNDED_COST], true, !showCents));
                    idx += 1; 

                    if (showDollarValue) {
                        // 3
                        datagrid_data_marks[idx].push(showCost(dp[IDX_SOLVER_RISK_EXPECTED_SAVINGS], true, !showCents));
                        datagrid_data_names[idx].push(showCost(dp[IDX_SOLVER_RISK_EXPECTED_SAVINGS], true, !showCents));
                        idx += 1;
                    }

                    // 7
                    datagrid_data_marks[idx].push(dp[IDX_SOLVER_RISK_LEVERAGE_GLOBAL].toFixed(decimals) + " : 1");
                    datagrid_data_names[idx].push(dp[IDX_SOLVER_RISK_LEVERAGE_GLOBAL].toFixed(decimals) + " : 1");
                    idx += 1;                    
                                        
                    if (true || dilutedMode !== 1) { // show only if "Diluted"
                        // 4
                        datagrid_data_marks[idx].push(showDollarValue ? showCost(dp[IDX_SOLVER_BENEFIT_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                        datagrid_data_names[idx].push(showDollarValue ? showCost(dp[IDX_SOLVER_BENEFIT_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                        idx += 1;
                    }
                } else {
                    // 4
                    datagrid_data_marks[idx].push(num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                    datagrid_data_names[idx].push(num2prc(dp[IDX_SOLVER_BENEFIT], decimals));
                    idx += 1;                    
                }
                                                
                if (is_risk) {                                        

                    if (showLECValues && (Use_Simulated_Values > 0)) {                        
                        // 5
                        datagrid_data_marks[idx].push(showDollarValue ? showCost(dp[IDX_SOLVER_LOSS_EXCEEDANCE] * dollarValueOfEnterprise, true, !showCents) : num2prc(dp[IDX_SOLVER_LOSS_EXCEEDANCE], decimals));
                        datagrid_data_names[idx].push(showDollarValue ? showCost(dp[IDX_SOLVER_LOSS_EXCEEDANCE] * dollarValueOfEnterprise, true, !showCents) : num2prc(dp[IDX_SOLVER_LOSS_EXCEEDANCE], decimals));
                        idx += 1;

                        // 6
                        datagrid_data_marks[idx].push(num2prc(dp[IDX_SOLVER_LIKELIHOOD_OF_EXCEEDING], decimals, true));
                        datagrid_data_names[idx].push(num2prc(dp[IDX_SOLVER_LIKELIHOOD_OF_EXCEEDING], decimals, true));
                        idx += 1;
                    }                    

                    if (showAllRows) {
                        // 8
                        datagrid_data_marks[idx].push(dp[IDX_SOLVER_RISK_DELTA_LEVERAGE].toFixed(decimals));
                        datagrid_data_names[idx].push(dp[IDX_SOLVER_RISK_DELTA_LEVERAGE].toFixed(decimals));
                        idx += 1;

                        // 9
                        datagrid_data_marks[idx].push(showDollarValue ? showCost(dp[IDX_SOLVER_DELTA_RISK_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_DELTA_RISK], decimals));
                        datagrid_data_names[idx].push(showDollarValue ? showCost(dp[IDX_SOLVER_DELTA_RISK_MONETARY], true, !showCents) : num2prc(dp[IDX_SOLVER_DELTA_RISK], decimals));
                        idx += 1;

                        // 10
                        datagrid_data_marks[idx].push(showCost(dp[IDX_SOLVER_DELTA_COST], true, !showCents));
                        datagrid_data_names[idx].push(showCost(dp[IDX_SOLVER_DELTA_COST], true, !showCents));
                        idx += 1;

                        if (showDollarValue) {
                            // 11
                            datagrid_data_marks[idx].push(showCost(dp[IDX_SOLVER_RISK_DELTA_EXPECTED_SAVINGS], true, !showCents));
                            datagrid_data_names[idx].push(showCost(dp[IDX_SOLVER_RISK_DELTA_EXPECTED_SAVINGS], true, !showCents));
                            idx += 1;
                        }
                    }
                }
            }
                
            // funded alts names
            var sFundedAlts = "";
            for (var i = 0; i < dp[IDX_SOLVER_FUNDED_ALTS].length; i++) {
                var ctrl = getControlByID(dp[IDX_SOLVER_FUNDED_ALTS][i][0]);
                if ((ctrl)) {
                    //sFundedAlts += (sFundedAlts == "" ? "" : "<br>") + "<nobr>" + ShortString(ctrl[IDX_CONTROL_INDEX]+ " " + ctrl[IDX_CONTROL_NAME], 70) + "</nobr>";
                    sFundedAlts += (sFundedAlts == "" ? "" : "<br>") + "<span title='" + ctrl[IDX_CONTROL_INDEX]+ " " + ctrl[IDX_CONTROL_NAME] + "'>" + ShortString(ctrl[IDX_CONTROL_INDEX]+ " " + ctrl[IDX_CONTROL_NAME], 40) + "</span>";
                }
            }
            if (isRevertedOrderOfDatapoints()) {
                datagrid_data_names[idx].splice(summaryRowsCount, 0, sFundedAlts);       
                datagrid_data.splice(0, 0, dp);
            } else {
                datagrid_data_names[idx].push(sFundedAlts);       
                datagrid_data.push(dp);
            }

            // funded alts marks
            for (var i = 0; i < controls_len; i++) {
                var fundedMark = "";
                for (var j = 0; j < dp[IDX_SOLVER_FUNDED_ALTS].length; j++) {
                    if (controls_data[i][IDX_CONTROL_ID] == dp[IDX_SOLVER_FUNDED_ALTS][j][0]) fundedMark = "<%=ResString("tblFUNDED")%>";
                }
                if (isRevertedOrderOfDatapoints()) {
                    datagrid_data_marks[summaryRowsCount + i].splice(summaryRowsCount, 0, fundedMark);
                } else {
                    datagrid_data_marks[summaryRowsCount + i].push(fundedMark);
                }
            }
        }
    }

    function updateToolbar() {
        if (is_solving) return;

        $(".on_advanced").toggle(isAdvancedMode);

        // show or hide the Custom Constraints panel and settings
        //var pnl = $("#pnlCCSettings");
        //if ((pnl)) {
        //    if (isParameterCC) {
        //        $("#tdCCDetails").show();
        //        $("#tbMin").html(tMin);
        //        $("#tbMax").html(tMax);
        //    } else {
        //        $("#tdCCDetails").hide();
        //    }
        //}
        //$("#cbGridView").prop("disabled", true);
        //$("#cbChartView").prop("disabled", true);

        switchIgnores();
        switchBaseCase();

        var opacity = keepSeed ? 1 : 0.5;
        $("#btnNewSeed").fadeTo(opacity).css({"cursor" : keepSeed ? "pointer" : "default", "opacity" : opacity});
        $("#lblSeed").css("color", keepSeed ? "black" : "#909090;");
        $("#tbSeed").prop("disabled", !keepSeed);
        $("#btnCancel").toggle(plot_point_by_point);

        <%If PM.IsRiskProject Then%>
        $("#cbShowLECValues").prop("disabled", Use_Simulated_Values == 0).prop("checked", showLECValues);
        var cbChart = document.getElementById("cbChartType");
        if ((cbChart)) {
            cbChart.options[5].disabled = Use_Simulated_Values == 0;
            cbChart.options[6].disabled = Use_Simulated_Values == 0;
        }
        <%End If%>
    }

    var ccId = -1;
    var ccName = "undefined";
    <%--var tMin = <%=JS_SafeNumber(SelectedConstraintMin)%>;
    var tMax = <%=JS_SafeNumber(SelectedConstraintMax)%>;--%>

    //function onEditCCLimits(ccId, ccName, tMin, tMax) {
    //    var sInfo = "Enter " + ccName + " limits:<br><br>";
    //    dxDialog(sInfo + "<center><br><span>Min:&nbsp;</span><input id='tbMinBound' type='input text' style='text-align:right;' value='" + tMin + "' > &nbsp;&nbsp; <span>Max:&nbsp;</span><input id='tbMaxBound' type='input text' style='text-align:right;' value='" + tMax + "' ></center>", "onSaveCCLimits($('#tbMinBound').val(), $('#tbMaxBound').val());", ";",  "Settings");
    //}

    //function onSaveCCLimits(tMin, tMax) {
    //    sendCommand("action=save_cc_limits&min=" + tMin + "&max=" + tMax, true); 
    //}

    function onPlottingOptionDlg() {
        var sDlg = "<br><br><div style='text-indent:-20px;padding-left:20px;'><label><input type='radio' class='radio' id='rbPlottingSetting' name='rbPlotModesSetting' >Display one point at a time (takes longer)</label></div>";
        sDlg += "<div style='text-indent:-20px;padding-left:20px;'><label><input type='radio' class='radio' name='rbPlotModesSetting' checked='checked' >Display all points at once (delay until plot appears)</label></div><br><br>";
        dxDialog(sDlg, "rbPlotModesClick(document.getElementById('rbPlottingSetting').checked);", ";",  "Plotting mode");
    }

    function syncError() {
        hideLoadingPanel();
        dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error");
    }
    /* end Callback */

    function getControlByID(control_id) {
        var controls_len = controls_data.length;
        for (var i = 0; i < controls_len; i++) {
            if (controls_data[i][IDX_CONTROL_ID] == control_id) return controls_data[i];
        }
        return null;
    }

    var tickCostFormatter = function (format, val) {        
        if (val < 0) return " ";
        return isParameterCC ? Math.round(val * 100) / 100 : showCost(Math.round(val), true, !showCents);
    }

    var tickDollarValueFormatter = function (format, val) {        
        if (val < 0) return " ";
        return showCost(Math.round(val * dollarValueOfEnterprise), true, !showCents);
    }

    var tickPercentFormatter = function (format, val) {        
        if (val < 0.1 || val > 1.1) return " ";
        return num2prc(val, decimals);
    }

    function createChart() {             
        if ($("#divChart").data("dxChart")) $("#divChart").dxChart("instance").dispose();
        $("#divChart").empty().show();
        $("#divChart").removeData(); 

        var has_data = chart_data.length;
        var data_len = 0;

        if (!has_data) {
            $("#divChart").html("<h6 style='margin: 8em 2em;'><% = GetEmptyMessage() %></h6>");
            return false;
        }

        var series = [];
        var seriesColors = [];
        var seriesLabels = [];
        for (var i = 0; i < settings.chk_scenarios.length; i++) {
            seriesColors.push(settings.chk_scenarios[i][3]);
            seriesLabels.push("<input type='checkbox' class='checkbox cb_scenario_shown' checked='checked' id='cbScenarioShown" + i + "' value='" + i + "' onclick='showScenario(this.value, this.checked);' >&nbsp;" + settings.chk_scenarios[i][1]);
            series.push({ valueField: showDollarValue ? "d" + i : "s" + i, name: settings.chk_scenarios[i][1], point: { size: 8 }, color: settings.chk_scenarios[i][3] });
        }

        var axis_y_min = zoom_plot ? chart_settings[5] : (is_risk ? (chart_settings[5] > 0 ? 0 : chart_settings[5]) : 0);
        var numberTicks = 10;

        var yAxisTitle = yTitle.length > 0 ? yTitle : chart_settings[2] + ", " + (showDollarValue ? CHAR_CURRENCY : "%");
        if (plotParameter != "priority") yAxisTitle = getChartParameterName(plotParameter);
        
        $('#divChart').dxChart({
            animation: { enabled: false },
            palette: "bright",
            dataSource: chart_data,
            crosshair: {
                enabled: true,
                color: "#777777"
            },
            commonSeriesSettings: {
                type: "line",
                argumentField: "cost"
            },
            commonAxisSettings: {
                grid: {
                    visible: false
                }
            },
            height: "100%",
            series: series,
            tooltip: {
                enabled: true,
                format: "percent",
                //argumentFormat: displayCostValue ? "currency" : "percent"
                contentTemplate: function(info, container) {
                    //var c = $("#" + chart_id).dxChart("instance");
                    //var xLabel = c.option("argumentAxis.title.text")
                    //var yLabel = c.option("valueAxis.title.text")
                    //yLabel = yLabel.substr(0, yLabel.length - 3);
                    //$("<div>" +
                    //    yLabel + ": " + info.valueText + "<br>" +
                    //    xLabel + ": " + (displayCostValue ? showCost(info.argument, true, skipNonZeroCents) : num2prc(info.argument, 2, true)) + "</div>").appendTo(container);
                    //var cd = info.data[3];

                    // init data for tooltip
                    //var sFundedAlts = "";
                    //if (detailedTooltips) {
                    //sFundedAlts = "<br>";
                    //sFundedAlts = "<div class='detailed_tooltip_info text' style='display: " + (detailedTooltips ? "" : "none") + ";'>";
                    //for (var i = 0; i < cd[IDX_SOLVER_FUNDED_ALTS].length; i++) {
                    //    var ctrl = getControlByID(cd[IDX_SOLVER_FUNDED_ALTS][i][0]);
                    //    if ((ctrl)) {
                    //        sFundedAlts += (sFundedAlts == "" ? "" : "<br>") + "<nobr>" + ShortString(ctrl[IDX_CONTROL_INDEX]+ " " + ctrl[IDX_CONTROL_NAME], 70) + "</nobr>";
                    //    }
                    //}
                    //sFundedAlts += "</div>";
                    //}
        
                    // generate tooltip text content
                    var ttText = "<b>" + info.seriesName + "</b><br><br>";

                    if (isParameterCC) {
                        var cbParameter = document.getElementById("cbParameter");
                        var isRISK = false;
                        if ((cbParameter)) isRISK = cbParameter.options[cbParameter.selectedIndex].value == <% = JS_SafeNumber(Integer.MaxValue) %>;
                        ttText += "Upper Bound" + ': <b>' + info.argumentText + '</b><br>';
                        ttText += isRISK ? "<% = ResString("lblRARisk")%>" + ': <b>' + info.argumentText + '</b>' : "";
                        ttText += "<% = ParseString("Number Of Funded %%Alternatives%%")%>: <b>" + info.argument + "</b>";
                        if (!isRisk) {
                            ttText += '<br>' + (isParameterCC ? "Benefit" : "<%=Capitalize(If(App.isRiskEnabled, ParseString("%%Risk%%"), ResString("titlePercent")))%>");
                            ttText += ': <b>' + info.valueText + '</b>';
                        } else {
                            var fieldName = getChartParameterName(plotParameter);
                            ttText += '<br>' + (plotParameter != "priority" ? fieldName : (isParameterCC ? "Benefit" : "<%=Capitalize(If(App.isRiskEnabled, ParseString("%%Risk%%"), ResString("titlePercent")))%>"));
                            ttText += ': <b>' + info.valueText + '</b>';
                        }
                    } else {
                        ttText += "<%=ResString("titleBudget")%>" + ': <b>' + showCost(info.point.data.budget, true, !showCents) + '</b><br>' + (isParameterCC ? "Benefit" : showDollarValue ? "Dollar Value" : "<% = Capitalize(ResString("titlePercent"))%>") + ': <b>' + (showDollarValue ? showCost(info.value * dollarValueOfEnterprise, true, !showCents) : num2prc(info.value, decimals)) +'</b><br>' + "<%=ResString("lblFundedCost")%>" + ': <b>' + showCost(info.argument, true, !showCents) + '</b>';
                    }
                    $("<div>").html(ttText).appendTo(container);
                }
            },
            legend: {
                verticalAlignment: "bottom",
                horizontalAlignment: "center",
                visible: <% = Bool2JS(Not PM.IsRiskProject) %>
            },
            onLegendClick: function(e) {
                var series = e.component.option('series');
                if (series.length <= 0) return;
                for (var i = 0; i < series.length; i++) {
                    var s = series[i];
                    if (s.name == e.target.name) {
                        s.visible = !e.target.isVisible();
                        //toggle datagrid column
                        var scenario_id = getCheckedScenarios()[s.valueField.substr(1, s.valueField.length-1)*1][IDX_ID];
                        $("tr").children("td.scenario_id_" + scenario_id + "_hdr,td.scenario_id_" + scenario_id).css({"display": s.visible ? "" : "none"});
                        $("tr").children("th.scenario_id_" + scenario_id + "_clm").css({"display": s.visible ? "" : "none"});
                        dataTablesRefreshHeaders("tableContent");
                        break;
                    }
                }
                e.component.option('series', series);
            },
            onPointHoverChanged: function (e) {
                var cd = e.target.data;
                //var td = document.getElementById("tdCol" + cd.overall_point_index).previousSibling;
                //if ((td)) td.scrollIntoView({ block: "center", behavior: "smooth" });
                if (e.target.isHovered()) {
                    $(".funded" + cd.overall_point_index).each(function(i, obj) { this.setAttribute("bkg", obj.style.backgroundColor.toString()); });
                    $(".funded" + cd.overall_point_index).css("background-color", "#ffff99")[0].scrollIntoView({"inline" : "end"}); //"#d1e6b5"
                    
                } else {
                    $(".funded" + cd.overall_point_index).each(function(i, obj) { this.style.backgroundColor = this.getAttribute("bkg"); });
                }
            },
            "export": {
                enabled: true
            },            
            argumentAxis: {
                label:{
                    format: {
                        type: isParameterCC ? "number" : "currency"
                    },
                    customizeText: function () {
                        return isParameterCC ? this.value : showCost(this.value, true, !showCents);
                    }
                },
                //allowDecimals: false,
                title: {
                    text: xTitle.length > 0 ? xTitle : chart_settings[1]
                },
                visualRange: {
                    startValue: minX
                },
                valueMarginsEnabled: false
            },
            valueAxis: {
                label: {
                    //customizeText: function () {
                    //    return displayCostValue ? showCost(this.value, true, skipNonZeroCents) : this.valueText;
                    //},
                    format: {
                        type: showDollarValue ? "currency" : "percent"
                    }
                },
                title: {
                    text: yAxisTitle
                },
                visualRange: {
                    startValue: showDollarValue ? axis_y_min * dollarValueOfEnterprise : axis_y_min
                },
                //tickInterval: 0.1,
                allowDecimals: true,
                valueMarginsEnabled: false
            },
            onDone: function (e) {
                
            },
            title: {
                text: chartTitle.length > 0 ? htmlEscape(chartTitle) : htmlEscape(chart_settings[0]),
                font: {size: 17}
            }           
        }); 

        //put the scenarios descriptions as the plot legend items tooltips
        //if ((chart_data) && (chart_data.length > IDX_DESCRIPTION)) {
        //    var legend_items = $("tr.jqplot-table-legend");
        //    var data_len = chart_data[IDX_DESCRIPTION].length;
        //    for (var t = 0; t < data_len; t++) {
        //        var a = chart_data[IDX_DESCRIPTION][t];
        //        if (legend_items.length > t) {
        //            legend_items.eq(t).prop("title", a);
        //        }
        //    }
        //}        

        initAllNoneLegendSelector();
    }

    function getChartParameterName(param) {
        var retVal = "";
        switch (param) {
            case "priority":
                retVal = "<%=ResString("tblRowBenefit")%>";
                break;
            case "leverage":
                retVal = "<%=ResString("tblRowLeverageGlobal")%>";
                break;
            case "delta_leverage":
                fieldName = "<%=ResString("tblRowDeltaLeverage")%>";
                break;
            case "savings":
                retVal = "<%=ResString("tblRowSavings")%>";
                break;
            case "delta_savings":
                retVal = "<%=ResString("tblRowDeltaSavings")%>";
                break;
            case "savings":
                retVal = "<%=ResString("tblRowSavings")%>";
                break;
            case "delta_savings":
                retVal = "<%=ResString("tblRowDeltaSavings")%>";
                break;
            case "percent_lec":
                retVal = "Loss Exceedance";
                break;
            case "likelihood_lec":
                retVal = "<%=ParseString("%%Likelihood%% of exceeeding")%>";
                break;
        }
        return retVal;
    }

    function updateChart() {
        createChart();

        //if ($("#divChart").data("dxChart")) {
        //    $("#divChart").dxChart("instance").option("dataSource", chart_data);
        //} else {
        //    createChart();
        //}
    }

    function initAllNoneLegendSelector() {
        <%--$('table.jqplot-table-legend > tbody:last-child').append('<tr class="jqplot-table-legend"><td class="jqplot-table-legend" colspan="3" style="text-align: center;"><a href="" class="actions" onclick="onScenariosSelected(1); return false;"><%=ResString("lblAll")%></a>&nbsp;|&nbsp;<a href="" class="actions" onclick="onScenariosSelected(0); return false;"><%=ResString("lblNone")%></a></td></tr>');--%>
    }

    function onScenariosSelected(is_all) {
        $("input.cb_scenario_shown").each( function (i, obj) {             
            $(this).prop("checked", is_all == 1);
            showScenario(this.value, this.checked);
        });
    }

    function hidePinnedTooltips() {
        $('.bubble_tooltip_pinned').each(function(i, obj) { obj.parentElement.removeChild(this) });        
    }

    <%--function showChartTooltip(ev, seriesIndex, pointIndex, data, gridColumnHighlight) {
        //if (!canShowTooltip) return true;
        if ((jqplot_chart) && jqplot_chart.series[seriesIndex].canvas._elem[0].style.display == "none") return true;
        
        // create new tooltip div and insert after pinned div
        var newDiv = document.createElement("div");
        newDiv.id = "bubble_tooltip";
        newDiv.style.display = "none";
        newDiv.className = "bubble_tooltip text";
        insertAfter(newDiv, document.getElementById("tblEfficientFrontierMain"));

        var cd = data[3];

        // init data for tooltip
        var sFundedAlts = "";
        //if (detailedTooltips) {
            //sFundedAlts = "<br><br>";
            sFundedAlts = "<div class='detailed_tooltip_info text' style='display: " + (detailedTooltips ? "" : "none") + ";'>";
            for (var i = 0; i < cd[IDX_SOLVER_FUNDED_ALTS].length; i++) {
                var ctrl = getControlByID(cd[IDX_SOLVER_FUNDED_ALTS][i][0]);
                if ((ctrl)) {
                    sFundedAlts += (sFundedAlts == "" ? "" : "<br>") + "<nobr>" + ShortString(ctrl[IDX_CONTROL_INDEX]+ " " + ctrl[IDX_CONTROL_NAME], 70) + "</nobr>";
                }
            }
            sFundedAlts += "</div>";
        //}
        
        // generate tooltip text content
        var ttText = ""
        if (isParameterCC) {
            var cbParameter = document.getElementById("cbParameter");
            var isRISK = false;
            if ((cbParameter)) isRISK = cbParameter.options[cbParameter.selectedIndex].value == <%=JS_SafeNumber(Integer.MaxValue)%>;
            ttText = "Upper Bound" + ': <b>' + cd[IDX_SOLVER_BUDGET] + '</b><br>' + (isRISK ? "<%=ResString("lblRARisk")%>" + ': <b>' + cd[IDX_SOLVER_FUNDED_COST] + '</b>': "<%=ResString("lblFundedCost")%>" + ': <b>' + showCost(cd[IDX_SOLVER_FUNDED_COST], true, !showCents) + '</b>')+'<br>' + "<%=Capitalize(ParseString(_TPL_RISK_RISK))%>" + ': <b>' + data[1].toFixed(2) + '%</b><br><br>' + sFundedAlts;
        } else {
            ttText = "<%=ResString("titleBudget")%>" + ': <b>' + showCost(cd[IDX_SOLVER_BUDGET], true, !showCents) + '</b><br>' + "<%= Capitalize(If(PM.IsRiskProject, ParseString(_TPL_RISK_RISK), ResString("titlePercent")))%>" + ': <b>' + (showDollarValue ? showCost(cd[IDX_SOLVER_BENEFIT] * dollarValueOfEnterprise, true, !showCents) : data[1].toFixed(2) + '%') +'</b><br>' + "<%=ResString("lblFundedCost")%>" + ': <b>' + showCost(cd[IDX_SOLVER_FUNDED_COST], true, !showCents) + '</b>' + sFundedAlts;
        }
        var pin = "<a href='' style='cursor: pointer; float: right;' onclick='togglePinTooltip(event, this.parentNode, this.childNodes[0]); return false;'><i class='fas fa-thumbtack pinned' style='margin: -2px -10px 0px 10px' ></i></a>";
        $('#bubble_tooltip').html(pin + ttText);

        // detect tooltip size
        var tw = $('#bubble_tooltip').width();
        var th = $('#bubble_tooltip').height();

        // detect tooltip coordinates and position
        var chart_left = $('#jqplot_chart').offset().left,
        chart_top = $('#jqplot_chart').offset().top,        
        x = jqplot_chart.axes.xaxis.u2p(data[0]),  // convert x axis units to pixels
        y = jqplot_chart.axes.yaxis.u2p(data[1]);  // convert y axis units to pixels
        var pos_x = chart_left + x - 25;
        var w = $('#jqplot_chart').width();
        if ((w) && w > 100 && pos_x > (w - tw)) { pos_x = chart_left + x - tw - 30; if (pos_x < 30) pos_x = 30; }
        var pos_y = chart_top + y + 12;
        var h = $('#jqplot_chart').height();
        if ((h) && h > 100 && pos_y > (h - th)) { pos_y = chart_top + y - th - 25; if (pos_y < 20) pos_y = 20; }
        $('#bubble_tooltip').css({ left: pos_x, top: pos_y });
        $('#bubble_tooltip').show();
        
        last_pointIndex = cd[IDX_SOLVER_OVERALL_POINT_INDEX];
        //if (gridColumnHighlight && seriesIndex == scenario_index) {
        if (gridColumnHighlight) {
            $(".funded" + cd[IDX_SOLVER_OVERALL_POINT_INDEX]).each(function(i, obj) { this.setAttribute("bkg", obj.style.backgroundColor.toString()); });
            $(".funded" + cd[IDX_SOLVER_OVERALL_POINT_INDEX]).css("background-color", "#d1e6b5");
            var td = document.getElementById("tdCol" + cd[IDX_SOLVER_OVERALL_POINT_INDEX]).previousSibling;
            if ((td)) td.scrollIntoView();
        }
    }--%>

    var allTooltipsShown = false;

    function togglePinAllTooltips(e, tooltipDiv, iconImg) {
        //allTooltipsShown = !allTooltipsShown;

        //if (allTooltipsShown) {
        //    //tooltipDiv.className = "bubble_tooltip_pinned text";
        //    iconImg.className = "fas fa-thumbtack pinned";

        //    if ((jqplot_chart) && (jqplot_chart.series)) {
        //        for (var seriesIndex = 0; seriesIndex < jqplot_chart.series.length; seriesIndex++) {
        //            for (var pointIndex = 0; pointIndex < jqplot_chart.series[seriesIndex].data.length; pointIndex++) {
        //                var data = jqplot_chart.series[seriesIndex].data[pointIndex];
        //                scenario_index = seriesIndex;
        //                showChartTooltip(event, seriesIndex, pointIndex, data, false);
        //                togglePinTooltip(event, $("#bubble_tooltip")[0], $("#bubble_tooltip").find("i")[0]);
        //            }
        //        }
        //    }
        //} else {
        //    //tooltipDiv.className = "bubble_tooltip text";
        //    iconImg.className = "fas fa-thumbtack unpinned";

        //    hidePinnedTooltips();
        //}

    }

    function togglePinTooltip(e, tooltipDiv, iconImg) {
        e.stopPropagation();
        // pin this div (change its class to "bubble_tooltip_pinned) and change the icon"
        if (tooltipDiv.className.indexOf("bubble_tooltip_pinned") >= 0) {
            // unpin
            tooltipDiv.parentElement.removeChild(tooltipDiv);
        } else {
            // pin
            tooltipDiv.id = "bubble_tooltip_pinned_" + tooltipDiv.parentElement.childNodes.length;
            tooltipDiv.className = "bubble_tooltip_pinned text";
            if ((iconImg)) iconImg.className = "fas fa-thumbtack pinned";
        }
    }

    function showScenario(series_index, checked) {        
        //if ((jqplot_chart) && series_index >= 0 && series_index < jqplot_chart.series.length) {
        //    if (checked) {
        //        jqplot_chart.series[series_index].canvas._elem.show();
        //        jqplot_chart.series[series_index].showMarker = true;
        //        $(".jqplot-point-label.jqplot-series-"+series_index).show();
        //    } else {
        //        jqplot_chart.series[series_index].canvas._elem.hide();
        //        jqplot_chart.series[series_index].showMarker = false;
        //        $(".jqplot-point-label.jqplot-series-"+series_index).hide();
        //    }           
        //    if (series_index >= 0 && series_index < checked_scenarios.length) checked_scenarios[series_index] = checked;
        //}
        //// show/hide grid columns        
        ////// show all columns
        ////updateDatagrid();
        
        //var scenarios_len = scenarios.length;
        //var ser_id = -1;
        //for (var i = 0; i < scenarios_len; i ++) {
        //    if (scenarios[i][IDX_CHK] == 1) ser_id += 1;
        //    if (scenarios[i][IDX_CHK] == 1 && series_index == ser_id) {
        //        $("tr").children("td.scenario_id_" + scenarios[i][IDX_ID] + "_hdr,td.scenario_id_" + scenarios[i][IDX_ID]).css({"display": checked ? "" : "none"});
        //        $("tr").children("th.scenario_id_" + scenarios[i][IDX_ID] + "_clm").css({"display": checked ? "" : "none"});
        //        //var cell_indices = [];
        //        //$("tr.firstHeaderRow").children("td.scenario_id_" + scenarios[i][IDX_ID] + "_hdr").each(function(i, obj) {
        //        //    cell_indices.push(this.cellIndex);
        //        //});
        //        //$.each(cell_indices, function(i, obj) {
        //        //    dataTablesColumnVisibility("tableContent", obj, checked);
        //        //});
        //        dataTablesRefreshHeaders("tableContent");
        //    }
        //}
    }

    function drawChart() {
        updateChart();
    }

    function optionKeyDown(event, tb, value, orig_value, op) {  
       if (window.event) event = window.event;
       if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            if (code == 13) {
                switch (op) {
                    case 0:
                    case 1:
                    case 4:
                    case 7:
                        setIncrement(tb, value);
                    case 10:
                        setTitle(value);
                        break;
                    case 11:
                        setXLabel(value);
                        break;
                    case 12:
                        setYLabel(value);
                        break;
                }               
            }
            if (code == 27) tb.value = orig_value;
       }
    }

    //function setNumInc(tb, value, orig_value) {
    //    if (validFloat(value) && (str2double(value) != str2double(orig_value))) {
    //        cur_num_inc = value;
    //        sendCommand("action=solve0&value=" + value, true); 
    //    } else { tb.value = orig_value }
    //}

    function setIncrement(tb, value) {        
        var orig_value = 0;
        switch (solverMode) {
            case <% = CInt(EfficientFrontierDeltaType.MinBenefitIncrease)%>:
                orig_value = cur_min_benefit_increase;
                break;
            case <% = CInt(EfficientFrontierDeltaType.DeltaValue)%>:
                orig_value = cur_increment;
                break;
            case <% = CInt(EfficientFrontierDeltaType.NumberOfSteps)%>:
                orig_value = cur_num_inc;
                break;
            case <% = CInt(EfficientFrontierDeltaType.AllSolutions)%>:
                orig_value = cur_decrement;
                break;
        }
        value = replaceString(" ", "", value + "");
        if (validFloat(value) && (str2double(value) != str2double(orig_value))) {            
            switch (solverMode) {
                case <% = CInt(EfficientFrontierDeltaType.MinBenefitIncrease)%>:
                    cur_min_benefit_increase = value;
                    break;
                case <% = CInt(EfficientFrontierDeltaType.DeltaValue)%>:
                    cur_increment = value;
                    break;
                case <% = CInt(EfficientFrontierDeltaType.NumberOfSteps)%>:
                    cur_num_inc = value;
                    break;
                case <% = CInt(EfficientFrontierDeltaType.AllSolutions)%>:
                    cur_decrement = value;
                    break;
            }
            $("#tbIncrement").val(number2locale(value, true));
            sendCommand("action=solve" + solverMode + "&value=" + value, true); 
        } else { tb.value = orig_value }
    }

    //function setDecrement(tb, value, orig_value) {        
    //    value = replaceString(" ", "", value + "");
    //    if (validFloat(value) && (str2double(value) != str2double(orig_value))) {            
    //        cur_decrement = value;
    //        $("#tbDecrement").val(number2locale(cur_decrement, true));
    //        sendCommand("action=solve4&value=" + value, true); 
    //    } else { tb.value = orig_value }
    //}

    //function setMinBenefitIncrease(tb, value, orig_value) {
    //    value = replaceString(" ", "", value + "");
    //    if (validFloat(value) && (str2double(value)!=str2double(orig_value))) {            
    //        cur_min_benefit_increase = value;
    //        $("#tbMinBenefitIncrease").val(number2locale(cur_min_benefit_increase, true));
    //        sendCommand("action=solve7&value=" + value, true); 
    //    } else { tb.value = orig_value }        
    //}

    //function onCbScenarioClick(value) {
    //    scenario_id = value;
    //    $(".cb_scenario[value=" + value + "]").prop('checked', true);
    //    sendCommand("action=grid_scenario_changed&id=" + value, true);
    //}

    function getCheckedScenarios() {
        var retVal = [];
        for (i = 0; i < scenarios.length; i++) {
            if (scenarios[i][IDX_CHK] == 1) retVal.push(scenarios[i]);
        }
        return retVal;
    }

    function getCheckedScenariosCount() {
        var retVal = 0;
        for (i = 0; i < scenarios.length; i++) {
            if (scenarios[i][IDX_CHK] == 1) retVal += 1;
        }
        return retVal;
    }

    function getCheckedScenariosString() {
        var ids = "";
        for (i = 0; i < scenarios.length; i++) {
            ids += (scenarios[i][IDX_CHK] == 1 ? (ids == "" ? "" : "-") + scenarios[i][IDX_ID] + "" : "");
        }
        return ids;
    }

    function onToolbarDropdownOpened(sender, args) {
        scenarios_checked = getCheckedScenariosString();
    }

    function onToolbarDropdownClosed(sender, args) {
        onScenariosOK_Click();
    } 

    function onScenariosOK_Click() {        
        var ids = getCheckedScenariosString();
        if (ids != scenarios_checked) sendCommand("action=scenarios_load&ids=" + ids, true);
    }

    function getScenarioById(id) {
        var scenarios_len = scenarios.length;
        for (i=0;i<scenarios_len;i++) {
            if (scenarios[i][IDX_ID] == id) return scenarios[i];
        }
        return null;
    }

    function onScenarioClick(id, chk) {
        is_context_menu_open = false; 
        var sc = getScenarioById(id);
        if ((sc)) sc[IDX_CHK] = (chk ? 1 : 0);
        UpdateCheckedScenarios();
        UpdateScenariosButtons();     
        resizePage();
    }

    function checkAllScenarios(chk) {
        for (i=0; i < scenarios.length; i++) {scenarios[i][IDX_CHK] = (chk ? 1 : 0)};
        $("input:checkbox.cb_scenario").prop("checked", chk);
        UpdateScenariosButtons();
        UpdateCheckedScenarios();
    }    

    function UpdateCheckedScenarios() {
        ids = "";
        $('input:checkbox.cb_scenario:checked').each(function() { ids += (ids == "" ? "" : ",") + this.value});
        sendCommand("action=scenario_checked&ids=" + ids);
    }
    function UpdateScenariosButtons() {
        var scen_len = scenarios.length;        
        var checked_len = $("input:checkbox.cb_scenario:checked").length;

        $("#btnAll"  ).prop("disabled", (scen_len == 0) || (checked_len == scen_len));
        $("#btnNone" ).prop("disabled", (scen_len == 0) || (checked_len == 0));              
    }

    function updateButtons() {
        var scen_len = scenarios.length;        
        var checked_len = $("input:checkbox.cb_scenario:checked").length;

        UpdateScenariosButtons();
        $("#cbFields").prop("disabled", checked_len == 0);
        $("#cbViewMode").prop("disabled", checked_len == 0);
        //$("#pnlIgnore").prop("disabled", !is_risk && checked_len == 0);
        //$("#pnlBaseCase").prop("disabled", checked_len == 0);
        
        <%--var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var btn =  toolbar.findItemByValue("export");
            if ((btn)) btn.set_enabled(checked_len != 0);
            
        }--%>
        
        var lblScenCount =  document.getElementById("lbl_scen_count");
        if ((lblScenCount)) {
            var scenAll = scenarios.length;
            var scenSel = 0;
            for (var i = 0; i < scenAll; i++) { 
                if (scenarios[i][IDX_CHK]*1 == 1) scenSel += 1;
            }
            lblScenCount.innerHTML = "<small>" + scenSel + "/" + scenAll + "</small>";
        }

        <%--document.getElementById("btnSelectScenarios").value = "<%=ResString("lblSelectScenarios")%>&nbsp;(" + scenSel + "/" + scenAll + ")&#x25BE;";--%>

        //$(".empty_gridview").parents("table").css("border-width", "0px").prop("border", "0");

        $("#lblMinCost").html(showCost(settings.minCost)); // CostString(MinCost)
        $("#lblMinCostDiff").html(showCost(settings.minCostDiff)); // CostString(MinCost)        
        //todo:
        //$("#btnMode3").prop("disabled", settings.minCost == 0);
        //$("#btnMode4").prop("disabled", settings.minCostDiff == 0);

        updateFilterImg();
    }

    // Scenarios toolbar drop-down
    function btnSelectScenariosDropdown(sender) {
        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
        var scenarios_len = scenarios.length;
        for (var i = 0; i < scenarios_len; i++) {                
            var a = scenarios[i];
            sMenu += "<div class='divCheckbox'><label style='cursor: default;' onclick='is_context_menu_open = false;'><input type='checkbox' class='cb_scenario' id='tpl" + i + "' name='tpl" + i + "' value='" + a[IDX_ID] + "' " + (a[IDX_CHK] == 1 ? " checked='checked' " : "") +" onclick='onScenarioClick(this.value, this.checked);' onkeydown='onScenarioClick(this.value, this.checked);' title='" + a[IDX_NAME] + "' >&nbsp;&nbsp;" + ShortString(a[IDX_NAME], OPT_SCENARIO_LEN) + "</label>" + (a[IDX_DESCRIPTION] == "" ? "" : " (" + ShortString(a[IDX_DESCRIPTION], OPT_DESCRIPTION_LEN) + ")")+"</div>"; 
        }

        sMenu += "  <div style='text-align:left; padding:10px 0px 2px 35px;'><nobr>";
        sMenu += "      <input type='button' id='btnAll'  value='<%=ResString("lblAll") %>'  class='button' style='width:75px' onclick='is_context_menu_open = false; checkAllScenarios(true);' >";
        sMenu += "      <input type='button' id='btnNone' value='<%=ResString("lblNone") %>' class='button' style='width:75px' onclick='is_context_menu_open = false; checkAllScenarios(false);' >";
        sMenu += "      <input type='button' id='btnOK'   value='<%=ResString("btnOK") %>'   class='button' style='width:75px' onclick='is_context_menu_open = true;  UpdateCheckedScenarios();' ></nobr>";
        sMenu += "  </div>";
        sMenu += "</div>";
        dropdownMenu(sender, sMenu);
    }

    function dropdownMenu(sender, sMenu) {
        if (is_context_menu_open == true) { 
            $("div.context-menu").hide(200); 
            is_context_menu_open = false; 
            if (last_dropdown_btn == sender.id) return false;
        }
        last_dropdown_btn = sender.id;
        is_context_menu_open = false;
        $("div.context-menu").hide().remove();
                        
        if ((sender)) {
            var rect = sender.getBoundingClientRect();
            var x = rect.left   + $(window).scrollLeft() + 0;
            var y = rect.bottom + $(window).scrollTop()  + 1;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});                        
            //if ((s)) { var w = s.width();var pw = $("#tblEfficientFrontierMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

            $("div.context-menu").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }
    }

    function canCloseMenu() {
        is_context_menu_open = true;
    }


    function InitScenarios() {                        
        // Scenarios - selected scenario dropdown

        //init minCost and minCostDiff
        <%--var pr = document.getElementById("<% =divParamsData.ClientID %>");
        if ((pr)) {
            var params = eval(pr[text]);
            
            var lblMinCost = document.getElementById("lblMinCost");
            if ((lblMinCost)) lblMinCost.innerHTML = params[0];
            
            var lblMinCostDiff = document.getElementById("lblMinCostDiff");
            if ((lblMinCostDiff)) lblMinCostDiff.innerHTML = params[1];

            var IsCancelled = params[2] == "1";
            if (IsCancelled) alert('Cancelled!');
        }--%>

        // init combobox
        //var cb = document.getElementById("cbScenarios");
        //if ((cb) && (scenarios) && (scenarios.length > 1)) {            
        //    while (cb.firstChild) {
        //        cb.removeChild(cb.firstChild);
        //    }
        //                        
        //    for (var j = 0; j < scenarios.length; j++) {
        //        var opt = document.createElement("option");
        //        opt.value= scenarios[j][IDX_ID];
        //        opt.innerHTML = ShortString(scenarios[j][IDX_NAME], 40);
        //        cb.appendChild(opt);
        //        if ((scenario_id) && (scenarios[j][IDX_ID] == scenario_id)) cb.selectedIndex = j;
        //    }
        //}
        
        //toggleScenariosVisibility();
       
        updateButtons();
        onGridModeClick(gridModeShowProjects, true);
    }

    //function toggleScenariosVisibility() {
    //    if ((scenarios) && (scenarios.length > 1)) { //&& (chart_data) && (chart_data.length > 1)) { 
    //        $("#cbScenarios").show();            
    //    } else { 
    //        $("#cbScenarios").hide(); 
    //    }
    //}

    function setTitle(value) {
        chartTitle = value.trim(); 
        if ($("#divChart").data("dxChart")) $("#divChart").dxChart("instance").option("title.text", chartTitle);
        sendCommand("action=title_plot&value=" + encodeURIComponent(value), false); 
    }
    
    function setXLabel(value) {
        xTitle = value;
        if ($("#divChart").data("dxChart")) $("#divChart").dxChart("instance").option("argumentAxis.title.text", xTitle);
        sendCommand("action=title_x&value=" + encodeURIComponent(value), false); 
    }

    function setYLabel(value) {
        yTitle = value;
        if ($("#divChart").data("dxChart")) $("#divChart").dxChart("instance").option("valueAxis.title.text", yTitle);
        sendCommand("action=title_y&value=" + encodeURIComponent(value), false);
    }
    
    function setShowLines(value) {
        //showLines = value;
        //var tmp = checked_scenarios;
        //if ((jqplot_chart) && (jqplot_chart.series) && (jqplot_chart.series.length >0)) {
        //    for (var i = 0; i < jqplot_chart.series.length; i++) {
        //        jqplot_chart.series[i].showLine = value;                
        //    }
        //    jqplot_chart.replot({ resetAxes: true });
        //    initAllNoneLegendSelector();
        //}
        //checked_scenarios = tmp;
    }
    
    function setShowLabels(value) {
        showLabels = value;
        //redraw completely because "jqplot_chart.series[i].pointLabels.show = value;" not working
        var tmp = checked_scenarios;
        drawChart();
        checked_scenarios = tmp;
    }

    function SetOptAll(chk) {
        var cb = theForm.cbIgnoreOptions;
        if (settings.chk_scenarios.length != 1 && (cb) && (!cb.checked)) return false;
        theForm.cbOptMusts.checked=chk;
        theForm.cbOptMustNots.checked=chk;
        theForm.cbOptDependencies.checked=chk;
        theForm.cbOptGroups.checked=chk;
        <% If Not App.isRiskEnabled Then %>
        theForm.cbOptConstraints.checked=chk;
        theForm.cbOptFundingPools.checked=chk;
        theForm.cbOptRisks.checked = chk;
        if ((theForm.cbOptTimePeriods)) theForm.cbOptTimePeriods.checked=chk;
        <% End If %>
        sendCommand("action=opt_all&val=" + ((chk) ? 1 : 0), true);
        return false;
    }

    function initIgnores() { // m, mn, cc, dp, gr, fp, r, tp
        var isSingleScenario = settings.chk_scenarios.length == 1;        
        if (isSingleScenario) {
            $("div.multi-scenario-ui").hide();
            $("div.single-scenario-ui").show();
        } else {
            $("div.single-scenario-ui").hide();
            $("div.multi-scenario-ui").show();
        }
        var values = settings.ignores;
        if ((theForm.cbOptMusts)) theForm.cbOptMusts.checked = !values[0];
        if ((theForm.cbOptMustNots)) theForm.cbOptMustNots.checked = !values[1];
        if ((theForm.cbOptConstraints)) theForm.cbOptConstraints.checked=!values[2];
        if ((theForm.cbOptDependencies)) theForm.cbOptDependencies.checked = !values[3];
        if ((theForm.cbOptGroups)) theForm.cbOptGroups.checked = !values[4];
        if ((theForm.cbOptFundingPools)) theForm.cbOptFundingPools.checked=!values[5];
        if ((theForm.cbOptRisks)) theForm.cbOptRisks.checked = !values[6];
        if ((theForm.cbOptTimePeriods)) theForm.cbOptTimePeriods.checked = !values[7];
    }

    function initBaseCase() {        
        var values = settings.basecase;
        if ((theForm.cbBaseCase)) theForm.cbBaseCase.checked = values[0];
        if ((theForm.cbBaseCaseIncludesM)) theForm.cbBaseCaseIncludesM.checked = values[1];
        if ((theForm.cbBaseCaseIncludesS)) theForm.cbBaseCaseIncludesS.checked = values[1];
        if ((theForm.cbBCGroups)) theForm.cbBCGroups.checked = values[2];
    }

    function switchIgnores() {
        if (settings.chk_scenarios.length == 1) {
            //$("#tblIgnores").find("input").prop("disabled", false); 
            $("#btnUseAll").prop("disabled", false);
            $("#btnIgnoreAll").prop("disabled", false);
        } else {
            var cb = theForm.cbIgnoreOptions;
            if ((cb)) { 
                //$("#tblIgnores").find("input").prop("disabled", !cb.checked);
                $("#btnUseAll").prop("disabled", !cb.checked);
                $("#btnIgnoreAll").prop("disabled", !cb.checked);
            }
        }        
    }

    function switchBaseCase() {
        var bc  = theForm.cbBaseCase;
        var bc1 = theForm.cbBaseCaseIncludes;
        var isSingleScenario = settings.chk_scenarios.length == 1;
        if ((bc1)) bc1.disabled = false;
        if ((bc)) {
            if (!isSingleScenario && (bc1)) { 
                $("#tblBaseCase").find("input").prop("disabled", !(bc.checked && bc1.checked));
                bc1.disabled = !bc.checked;
            }
        } else {
            if (!isSingleScenario && (bc1)) {
                $("#tblBaseCase").find("input").prop("disabled", !bc1.checked);
                bc1.disabled = false;
            }
        }
        var disabled_ignores = settings.chk_scenarios.length == 0;
        var icb = theForm.cbIgnoreOptions;
        if ((icb)) icb.disabled = disabled_ignores;
        icb = theForm.cbBaseCase;
        if ((icb)) icb.disabled = disabled_ignores;

        $("#pnlIgnore").find("input").prop("disabled", disabled_ignores);
        $("#tblIgnore").find("input").prop("disabled", disabled_ignores);
        $("#pnlBaseCase").find("input").prop("disabled", disabled_ignores);
        $("#tblBaseCase").find("input").prop("disabled", disabled_ignores);
    }

    function onOptionClick(name, checked) {
        sendCommand("action=option&name=" + name + "&val=" + ((checked) ? 1 : 0), true);
    }

    function onIgnoreOptionsClick(checked) {
        sendCommand("action=use_ignore_options&val=" + ((checked) ? 1 : 0), true);
    }

    function onBaseCaseOptionsClick(checked) {
        switchBaseCase();
        sendCommand("action=use_base_case_options&val=" + ((checked) ? 1 : 0), true);
    }

    function onGridModeClick(gm, is_init) {
        gridModeShowProjects = gm;
        //if (gm == gmFundedMarks) {
        //    $("#tableContent tr.tr_alt_names").hide();
        //    $("#tableContent tr.tr_funded_marks").show();
        //} else {
        //    $("#tableContent tr.tr_funded_marks").hide();
        //    $("#tableContent tr.tr_alt_names").show();            
        //}
        if (!is_init) {
            updateDatagrid();
            //initFixedColumn();
            sendCommand("action=grid_mode&mode=" + gm, false);
        }
    }

    function onDetailedTooltipsClick(chk) {
        detailedTooltips = chk;
        if (chk) { 
            $("div.detailed_tooltip_info").each(function (index, el) {
                el.style.display = "";
                var topVal = parseInt(el.parentElement.style.top, 10);
                el.parentElement.style.top = (topVal - $(el).height()) + "px";
            }); 
        } else { 
            $("div.detailed_tooltip_info").each(function (index, el) {
                el.style.display = "none";
                var topVal = parseInt(el.parentElement.style.top, 10);
                el.parentElement.style.top = (topVal + $(el).height()) + "px";
            }); 
        }
        sendCommand("action=detailed_tooltips&value=" + chk, true);
    }

    function onUnlimBudgetClick(chk) {        
        if (chk) { $("#tbBudgetLimit").prop("disabled", true); } else { $("#tbBudgetLimit").prop("disabled", false); }
        sendCommand("action=unlim_budget&value=" + chk, true);
    }

    function cbDecimalsChange(value) {
        decimals = value * 1;
        sendCommand("action=decimals&value=" + value, true);
    }

    function ignoreOptionsMouseOver(is_hover) {
        var d = document.getElementById("divIgnoreBtns");
        if ((d)) {
            if (is_hover == 1) {
                d.style.display = "";
            } else {
                d.style.display = "none";
            }
        }
    }

    function setLayoutModeClick(target, chk) {
        var cbGrid = document.getElementById("cbGridView");
        var cbChart = document.getElementById("cbChartView");
        if ((cbGrid) && (cbChart)) {
            if (target == "grid") {
                layoutMode = chk ? 0 : 2;
            }
            if (target == "chart") {
                layoutMode = chk ? 0 : 1;
            }
        }
        cbGrid.checked = layoutMode == 0 || layoutMode == 1; 
        cbChart.checked = layoutMode == 0 || layoutMode == 2; 
        
        if (layoutMode == 0) { $("#tdChart").find(".dx-resizable-handle").addClass("splitter_v"); } else { $("#tdChart").find(".dx-resizable-handle").removeClass("splitter_v"); }
        $("#tdChart").dxResizable("instance").option("width", layoutMode == 2 ? "100%" : (horizontalSplitterSize == UNDEFINED_INTEGER_VALUE ? "50%" : horizontalSplitterSize));

        var tdGrid = document.getElementById("tdGrid");
        var tdChart = document.getElementById("tdChart");
        if ((tdGrid) && (tdChart)) {
            tdGrid.style.display = cbGrid.checked ? "" : "none";
            tdChart.style.display = cbChart.checked ? "" : "none";
        }        
        if ((typeof is_init) == 'undefined' || !is_init) sendCommand("action=layout_mode&value=" + layoutMode, true);
        setTimeout(function () {
            resizePage(); 
            if (cbChart.checked) {
                updateChart();
            }
        }, 500);        
    }

    /* Tree View */
    function initTreeList() {
        <% If PagesWrtObjective.Contains(CurrentPageID) Then %>
        var storageKey = "EfficientFrontier_TreeList_<%=PRJ.ID%>";
        var hasState = typeof localStorage.getItem(storageKey) !== "undefined" && localStorage.getItem(storageKey) != null;

        //init columns headers                
        var columns = [];
        for (var i = 0; i < hierarchy_columns.length; i++) {
            var clmn = { "caption" : hierarchy_columns[i][1], "dataField" : hierarchy_columns[i][4], "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "encodeHtml" : hierarchy_columns[i][4] == "name" || hierarchy_columns[i][4] == "info" }
            columns.push(clmn);
            if (!hasState) {
                clmn.visible = hierarchy_columns[i][2];
            }
        }     

        var isLoadingTreeListSessionSettings = true;

        $("#divHierarchy").dxTreeList({
            allowColumnResizing: true,
            allowColumnReordering: true,
            autoExpandAll: true,
            dataSource: hierarchy_data,
            columns: columns,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            columnChooser: {
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: true
            },
            columnFixing: {
                enabled: true
            },
            hoverStateEnabled: true,
            focusedRowEnabled: true,
            focusedStateEnabled: true,
            focusedRowKey: selectedHierarchyNodeID,
            onFocusedRowChanged: function (e) {
                if ((e.row) && (selectedHierarchyNodeID !== e.row.key) && !isLoadingTreeListSessionSettings) {
                    selectedHierarchyNodeID = e.row.key;
                    sendCommand("<% =_PARAM_ACTION %>=selected_node&value=" + e.row.key, true);
                }
            },
            keyExpr: "guid",
            parentIdExpr: "pguid",
            rootValue: "",
            //onCellPrepared: function(e) {
            //    if (e.rowType === "header" && e.column.dataField !== "id" && e.column.dataField !== "name" && e.column.dataField !== "info") {
            //        e.cellElement.on("click", function(args) {
            //            var sortOrder = e.column.sortOrder;
            //            if (!e.column.type && sortOrder == undefined) {
            //                e.component.columnOption(e.column.index, "sortOrder", "desc");
            //                args.preventDefault();
            //                args.stopPropagation();
            //            }
            //        });
            //    }
            //},
            onContentReady: function (e) {
                $(e.element).find(".dx-toolbar").css("background-color", "transparent");
                setTimeout(function () {
                    isLoadingTreeListSessionSettings = false;
                    e.component.option("focusedRowKey", selectedHierarchyNodeID);
                }, 10);
            },
            onToolbarPreparing: function(e) {
                
            },
            paging: {
                enabled: false
            },
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            searchPanel: {
                visible: true,
                //width: 240,
                placeholder: "<%=ResString("btnDoSearch")%>..."
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: storageKey
            },
            sorting: {
                mode: 'multiple'
            },
            noDataText: "<% = GetEmptyMessage() %>",
            "export": {
                "enabled" : true
            },
            wordWrapEnabled: false                        
        });
        
        <%End If%>
    }
    /* end Tree View */

    // Select Events Dialog
    function onSelectEventsClick() {        
        initSelectEventsForm("Select <%=ParseString("%%Alternatives%%")%>");
        dlg_select_events.dialog("open");
        dxDialogBtnDisable(true, true);
    }

    function initSelectEventsForm(_title) {
        cancelled = false;

        var labels = "";

        // generate list of events
        var event_list_len = event_list.length;
        for (var k = 0; k < event_list_len; k++) {
            var checked = event_list[k][IDX_EVENT_ENABLED] == 1;
            labels += "<div class='divCheckbox'><label><input type='checkbox' class='select_event_cb' value='" + event_list[k][IDX_EVENT_ID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(event_list[k][IDX_EVENT_NAME]) + "</label></div>";
        }

        $("#divSelectEvents").html(labels);

        dlg_select_events = $("#selectEventsForm").dialog({
            autoOpen: false,
            modal: true,
            width: 420,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: _title,
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: {
                    id: 'jDialog_btnOK', text: "OK", click: function () {
                        dlg_select_events.dialog("close");
                    }
                },
                Cancel: function () {
                    cancelled = true;
                    dlg_select_events.dialog("close");
                }
            },
            open: function () {
                $("body").css("overflow", "hidden");
            },
            close: function () {
                $("body").css("overflow", "auto");
                if (!cancelled) {
                    sEventIDs = "";
                    var cb_arr = $("input:checkbox.select_event_cb");
                    var chk_count = 0;
                    $.each(cb_arr, function (index, val) { 
                        var cid = val.value + ""; 
                        if (val.checked) { 
                            sEventIDs += (sEventIDs == "" ? "" : ",") + cid; 
                            chk_count += 1;
                        } 
                        event_list[index][IDX_EVENT_ENABLED] = (val.checked ? 1 : 0);
                    });
                    //if ((chk_count == event_list.length) && (chk_count > 0)) sEventIDs = "all";
                    //if (chk_count == 0) sEventIDs = "none";
                    updateFilterImg(sEventIDs);
                    sendCommand('action=select_events&event_ids=' + sEventIDs); // save the selected events via ajax                    
                }
            }
        });
        $(".ui-dialog").css("z-index", 9999);
    }

    function updateFilterImg() {
        var img = document.getElementById("imgFilterEvents");
        if ((img)) {
            var event_list_len = event_list.length;
            var checkedEvents = 0;
            for (var k = 0; k < event_list_len; k++) {
                if (event_list[k][IDX_EVENT_ENABLED] == 1) checkedEvents += 1;
            }
            img.src = "../../Images/ra/" + (checkedEvents == event_list_len ? "filter20.png" : "filter20_red.png");            
        }        
    }

    function filterSelectAllEvents(chk) {
        $("input:checkbox.select_event_cb").prop('checked', chk * 1 == 1);
        dxDialogBtnDisable(true, false);
    }

    // end Select Events Dialog

    // Datatables datagrid
    function initDatagrid() {

        // reset variables
        // if ((datagrid) && (datagrid != null) && ((typeof datagrid.destroy) != 'undefined')) {
        //     datagrid.clear();
        //     datagrid.destroy();        
        //     datagrid = null;
        //     $("#tableContent").html("");
        // }

        dataTablesDestroy(datagrid, "tableContent");
        //dataTablesDestroy(datagrid);

        var btn_styles = "ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only";
                
        //init columns headers                
        var columns = [];
        //visibleColumnsCount = 0;
        for (var i = 0; i < datagrid_columns.length; i++) {
            columns.push({ "title" : datagrid_columns[i][COL_COLUMN_NAME] + "", "class" : datagrid_columns[i][COL_COLUMN_CLASS], "type" : datagrid_columns[i][COL_COLUMN_TYPE], "sortable" : true, "searchable" : false, "bVisible" : i == 0 && gridModeShowProjects ? false : datagrid_columns[i][COL_COLUMN_VISIBILITY] });
            //visibleColumnsCount += datagrid_columns[i][COL_COLUMN_VISIBILITY] ? 1 : 0;
        }

        if (datagrid_columns.length == 0) return false;
                
        datagrid = $("#tableContent").DataTable({
            destroy: true,
            dom: 'Bfprti',
            data: gridModeShowProjects ? datagrid_data_names : datagrid_data_marks,
            columns: columns.length > 0 ? columns : undefined,
            fixedColumns: {
                leftColumns: columns.length > 6 ? 2 : 0
            },
            paging: false, // pagination > 0 && (gridModeShowProjects ? datagrid_data_names : datagrid_data_marks).length > pagination,
            "pageLength": pagination > 0 ? pagination : 10,
            deferRender: true,
            ordering:  false,
            //order: cur_order,
            colReorder: false,
            scrollY: 245,
            scrollX: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "bScrollCollapse": true,
            stateSave: false,
            searching: false,
            info:      false,
            "headerCallback": function( thead, data, start, end, display ) {
                $("th", thead).each( function(i, obj) { 
                    if (i >= (gridModeShowProjects ? 1: 2) && datagrid_data.length) { 
                        var idx = i - (gridModeShowProjects ? 1: 2); 
                        obj.className = "sorting_disabled dt-right scenario_id_" + datagrid_data[isRevertedOrderOfDatapoints() ? overall_points_count - idx - 1 : idx][IDX_SOLVER_CUR_SCENARIO_ID] + "_clm";
                    } 
                });    
            },
            "rowCallback": function( row, data, index ) {
                //if (gridModeShowProjects) $("td:eq(1)", row).html(htmlEscape(data[1]));
                if (index == 0) { // add column ids for highlighting and scrolling
                    row.className = "odd firstHeaderRow";
                    $("td", row).each( function(i, obj) { if (i >= (gridModeShowProjects ? 1: 2)) { var idx = i - (gridModeShowProjects ? 1: 2); obj.id = "tdCol" + (settings.mode == imAllSolutions ? overall_points_count - idx - 1 : idx); } });    
                }
                //add classes for highlighting
                $("td", row).each( function(i, obj) { 
                    if (i >= (gridModeShowProjects ? 1: 2)) { 
                        var idx = i - (gridModeShowProjects ? 1: 2); 
                        obj.className = "highlighteablecolumn funded" + (settings.mode == imAllSolutions ? overall_points_count - idx - 1 : idx) + " scenario_id_" + datagrid_data[settings.mode == imAllSolutions ? overall_points_count - idx - 1 : idx][IDX_SOLVER_CUR_SCENARIO_ID] + (index < summaryRowsCount ? "_hdr" : "");
                    } 
                });
                if (index < summaryRowsCount) { // summary header rows
                    $("td", row).css({"background-color": BKG_SUMMARY});
                } else {
                    $("td", row).css({"background-color": "white"});
                }
                if (gridModeShowProjects && index == summaryRowsCount) { // alt names row
                    $("td", row).css({"vertical-align": "top", "text-align": "left"});
                }
                if (!gridModeShowProjects && index >= summaryRowsCount) { // alt funded marks rows
                    $("td:gt(1)", row).css({"text-align": "center"});
                }

            },
            "fnDrawCallback": function() {
                // setTimeout('$("input[type=search]").focus();', 500);
                setTimeout("colorizeColumnsByScenarios();", 100);

                refreshLEClabels();
            },
            "language" : {"emptyTable" : "<h6 style='margin:2em 10em'><nobr><% =GetEmptyMessage()%></nobr></h6>"},
            <% If App.isExportAvailable Then %>
            buttons: [
                //{ extend: 'colvis',     className : btn_styles, text : "Columns" },
                { extend: "copyHtml5",  className : btn_styles }, 
                //{ extend: "csvHtml5",   className : btn_styles },
                //{ extend: "excelHtml5", className : btn_styles },
                //{ extend: "print",      className : btn_styles, title:"", customize : function (win) { $(win.document.body).prepend("<h5>"+$("#lblPageTitle").html()+"</h5>");}}
            ],
            initComplete: function() {
                $('.dt-buttons').hide();
                $("#excel").prop("disabled", false);
            } 
            <% End If %>
        });
       
        // TODO:
        //dataTablesDisableAlternateRowColor(datagrid);
        resizeDatatable();
        
        $("a.dt-button").css({ padding:"2px 4px", width:"7em", margin:"2px", fontSize: "8pt" });
    }    

    function updateRedLabelValue() {
        var text = red_line_value + "";
        var result = prompt('Enter value (0 - 100):', text);
        if (validFloat(result)) {
            var tmp = str2double(result);
            if (tmp >= 0 && tmp <= 100) {
                red_line_value = tmp;
                refreshLEClabels();
                sendCommand("action=red_line&value=" + red_line_value, false);
            }
        }                
    }

    function updateGreenLabelValue() {
        var text = green_line_value == UNDEFINED_INTEGER_VALUE ? "" : showDollarValue ? Math.round(green_line_value * dollarValueOfEnterprise) : Math.round(green_line_value * 100);
        var result = prompt('Enter value:', text);
        if (result !== null) {
            result = result.trim();
            if (result == "") {
                green_line_value = UNDEFINED_INTEGER_VALUE;
                sendCommand("action=green_line&value=", false);
            }
            if (validFloat(result)) {
                var tmp = str2double(result);
                green_line_value = tmp;
                sendCommand("action=green_line&value=" + green_line_value, false);
                if (showDollarValue) { 
                    green_line_value = green_line_value / dollarValueOfEnterprise;
                } else {
                    green_line_value = green_line_value / 100;
                }
            }
            refreshLEClabels();
        }
    }

    function refreshLEClabels() {        
        var s = "<a href='' class='actions' onclick='updateRedLabelValue(); return false;'>" + red_line_value + "%</a> <%=ParseString("%%loss%%")%> exceedance";
        $(".lblPercentLEC").html(s);

        if (green_line_value != UNDEFINED_INTEGER_VALUE) {
            var cur_cost = showDollarValue ? showCost(green_line_value * dollarValueOfEnterprise, true, !showCents) : num2prc(green_line_value, decimals);
            gBtn = "<a href='' class='actions' onclick='updateGreenLabelValue(); return false;'>" + (cur_cost == "" ? "undefined" : cur_cost) + "</a>";
        } else {
            gBtn = "<a href='' class='actions' onclick='updateGreenLabelValue(); return false;'>undefined</a>";
        }
        s = "<%=ParseString("%%Likelihood%%")%> " + (<%=Bool2JS(PRJ.isOpportunityModel)%> ? "of gaining more than " : "of losing more than ") + gBtn;
        $(".lblLikelihoodLEC").html(s);
    }

    function colorizeColumnsByScenarios() {
        var colors_len = ColorPalette.length;
        var scenarios_len = scenarios.length;
        for (var i = 0; i < scenarios_len; i++) {
            if (scenarios[i][IDX_CHK] == 1) {
                //var hdr_color = colorBrightness(ColorPalette[scenarios[i][IDX_ID] % colors_len], 55);
                //var row_color = colorBrightness(ColorPalette[scenarios[i][IDX_ID] % colors_len], 85);
                var hdr_color = i == 0 ? BKG_SUMMARY : colorBrightness(scenarios[i][IDX_COLOR], 55);
                var row_color = i == 0 ? "white" : colorBrightness(scenarios[i][IDX_COLOR], 85);
                $("td.scenario_id_" + scenarios[i][IDX_ID] + "_hdr").css( {"background-color" : hdr_color, "text-align" : "right"} );
                $("td.scenario_id_" + scenarios[i][IDX_ID]).css( {"background-color" : row_color} );
            }
        }
    }

    function updateDatagrid() {
        initDatagrid(); // for now rebuilding totally, later do refresh
    }
    
    function resizeDatatable() {       
        if ((datagrid)) {
            $("#tableContent").height(0);
            $("#divChartContainer").hide();
            dataTablesResize('tableContent', 'divGridContainer', 0);
            dataTablesRefreshHeaders('tableContent');
            $("#divChartContainer").show();
        }
    }
    // end Datatables datagrid

    function setSimulationMode() {        
        //var SimIn = false; //$("#cbUseSimulatedInput").prop("checked");
        var SimOut = $("#cbUseSimulatedOutput").prop("checked");
        Use_Simulated_Values = SimOut ? 2 : 0;

        $("#tbNumSimulations").prop("disabled", Use_Simulated_Values == 0 ? "disabled" : "");
        updateToolbar();

        sendCommand("action=use_simulated_values&value=" + Use_Simulated_Values);
    }

    var iterationIncrement = 0;
    var progressIncrement = 0;
    var progressValue = 0;
    var curBudget = 0;
    var curScenarioIndex = 0;
    var is_solving = false;
    var isParameterCC = <%=Bool2JS(SelectedConstraintID >= 0)%>;
    var minX = settings.minBudget;
    var maxX = settings.maxBudget;

    function solveCancel() {
        $("#btnSolve").prop("disabled", false);
        $("#btnCancel").prop("disabled", true);
        //if (plot_point_by_point)
        cancelPolling();
    }

    function solveStart() {
        $("#divNoDataTitle").hide();

        if (layoutMode == 0 && !$("#tdChart").find(".dx-resizable-handle").hasClass("splitter_v")) $("#tdChart").find(".dx-resizable-handle").addClass("splitter_v");

        //$("#btnSelectScenarios").prop("disabled", true);
        $("#btnSolve").prop("disabled", true);
        $("#btnCancel").prop("disabled", false); // !plot_point_by_point

        originalDatapoints = [];

        sCalculateFrom = $("#tbCalculateFrom").val().trim();
        sCalculateTo = $("#tbCalculateTo").val().trim();

        if (sCalculateFrom != "") calculateFrom = str2cost(sCalculateFrom + "");
        if (sCalculateTo != "") calculateTo = str2cost(sCalculateTo + "");

        minX = (sCalculateFrom == "" || isNaN(calculateFrom)) ? settings.minBudget : calculateFrom;
        maxX = (sCalculateTo == "" || isNaN(calculateTo)) ? settings.maxBudget : calculateTo;

        // settings for CC
        isParameterCC = false;
        var cbParameter = document.getElementById("cbParameter");
        if ((cbParameter)) isParameterCC = cbParameter.selectedIndex > 0;
        
        switch (settings.mode) {
            case imNumOfIncrements:
                if (settings.numberOfIncrements > 0) {
                    iterationIncrement = ((maxX - minX) / settings.numberOfIncrements);
                } else {
                    iterationIncrement = 0;
                }
                break;
            case imSpecifiedIncrement:
            case imIncreasing:
                iterationIncrement = settings.specifiedIncrement;
                break;
            case imAllSolutions:
                curBudget = maxX;
                iterationIncrement = -settings.specifiedDecrement;
                break;
            case imMinCost:
                iterationIncrement = settings.minCost;
                break;
            case imMinDifferenceOfCosts:
                iterationIncrement = settings.minCostDiff;
                break;
        }

        startProgress();

        progressValue = 0;
        //if (iterationIncrement > 0  && settings.chk_scenarios.length > 0) {
        //    progressIncrement = (100 / ((maxX - minX) / iterationIncrement)) / settings.chk_scenarios.length;
        //}

        //curScenarioIndex = 0;            

        //if (settings.chk_scenarios.length > curScenarioIndex && (iterationIncrement > 0 || (iterationIncrement < 0 && settings.mode == imAllSolutions)) && minX < maxX) {
        //    var curScenarioID = settings.chk_scenarios[curScenarioIndex][0];
        //    for (var i = 0; i < settings.chk_scenarios.length; i++) {
        //        chart_data[IDX_CHART_DATA].push([]);
        //    }
        //    if (canProceed()) {   
        //        sendCommand("action=solve_start&scenario_id=" + curScenarioID + "&budget=" + curBudget + "&cc=" + isParameterCC + "&from=" + (sCalculateFrom == "" ? "" : calculateFrom) +  "&to=" + (sCalculateTo == "" ? "" : calculateTo), true);
        //    }
        //} else {
        //    solveStop(false);
        //}

        preInitGridAndChart();

       
        if ((settings.chk_scenarios.length > 0) && ((iterationIncrement > 0 && (settings.mode == imSpecifiedIncrement || settings.mode == imIncreasing)) || (settings.numberOfIncrements > 0 && settings.mode == imNumOfIncrements) || (iterationIncrement < 0 && settings.mode == imAllSolutions) || (settings.mode == imMinBenefitIncrease && cur_min_benefit_increase > 0)) && (minX < maxX)) {
            is_solving = true;
            solve_token = Math.round(Math.random() * 1000);
            chart_data = [];
            sendCommand("action=solve_start&budget=" + curBudget + "&cc=" + isParameterCC + "&ccBudgetLimit=" + $("#tbBudgetLimit").val() + "&from=" + (sCalculateFrom == "" ? "" : calculateFrom) +  "&to=" + (sCalculateTo == "" ? "" : calculateTo) + "&solve_token=" + solve_token, false); //!plot_point_by_point);
            //$(".divMsgRunning").show();
            showLoadingPanel(msgSolving);
            //if (!plot_point_by_point) $("#imgLoading").show();
            //if (!plot_point_by_point) $("#imgLoading").show();
            $("#divTimeElapsed").html("");
            startTime = new Date();
            if (plot_point_by_point) {
                $("#msgs").html("");
                chart_settings[5] = 0; // Axis Y min
                chart_settings[6] = zoom_plot ? 0 : 1; // Axis Y max
                initPolling();
            }
        } else {
            solveStop(false);
        }        
    }

    //function canProceed() {
    //    if (settings.mode == imAllSolutions) {
    //        return curBudget > 0;
    //    } else {
    //        return (curBudget <= maxX) || (curBudget - iterationIncrement < maxX && curBudget > maxX);
    //    }
    //}

    //function proceedSolve() {
    //    var oldBudget = curBudget;
    //    curBudget += iterationIncrement;
    //    
    //    if (settings.mode == imAllSolutions) {
    //        if (maxX > 0) progressValue = 100 - (curBudget * 100 / maxX);
    //        if (progressValue > 100) progressValue = 100;
    //    } else {
    //        progressValue += progressIncrement;
    //    }
    //    setProgress(progressValue);
    //
    //    if (canProceed() || (settings.mode == imAllSolutions && curBudget < minX && oldBudget > 0)) {
    //        if (settings.mode == imAllSolutions && curBudget < minX) curBudget = 0;
    //        sendCommand("action=solve_step&scenario_id=" + settings.chk_scenarios[curScenarioIndex][0] + "&budget=" + curBudget + "&cc=" + isParameterCC, false);
    //    } else {
    //        curScenarioIndex += 1;
    //        lastFundedAlts = "";
    //        if (!isParameterCC && settings.chk_scenarios.length > curScenarioIndex) {
    //            curBudget = minX - iterationIncrement;
    //            proceedSolve();
    //        } else {
    //            solveStop(true);
    //        }
    //    }
    //}

    function refreshGridAndChart() {
        preInitGridAndChart();

        for (var i = 0; i < originalDatapoints.length; i++) {
            addDatapoint(originalDatapoints[i]);
        }

        updateDatagrid();
        updateChart();
    }

    function solveStop(notify) {
        is_solving = false;
        stopProgress();
        //$(".divMsgRunning").hide();
        hideLoadingPanel();
        if (settings.mode == imMinBenefitIncrease) {
            function compare(a, b) {
                if (a[IDX_SOLVER_BUDGET] < b[IDX_SOLVER_BUDGET]) {
                    return -1;
                }
                if (a[IDX_SOLVER_BUDGET] > b[IDX_SOLVER_BUDGET]) {
                    return 1;
                }
                return 0;
            }
            originalDatapoints.sort(compare);
            refreshGridAndChart();
        }

        //if (notify) sendCommand("action=solve_stop", true);
        //$("#btnSelectScenarios").prop("disabled", false);
    }
    
    function startProgress() {
        $("#tblEfficientFrontierMain").find("input,button,select:not(#btnSolve,#btnCancel)").each(function () {
            this.setAttribute("data-dis", this.disabled ? "1" : "0");
            this.disabled = true;
        }); 
        $("#btnSolve").prop("disabled", true);
        $("#btnCancel").prop("disabled", !plot_point_by_point);
        $("#divModeDropdown").dxSelectBox("instance").option("disabled", true);
    }

    function stopProgress() {        
        $("#tblEfficientFrontierMain").find("input,button,select:not(#btnSolve,#btnCancel)").each(function () {
            this.disabled = this.getAttribute("data-dis") == "1";
        });
        $("#btnSolve").prop("disabled", false);
        $("#btnCancel").prop("disabled", true);
        $("#divModeDropdown").dxSelectBox("instance").option("disabled", false);
    }

    function toggleExtraBudgetVisibility(value) {
        if ((value*1 == -1) || (value*1 == ccRiskID)) {
            //$("#divUnlimBudget").hide();
            $("#divBudgetLimit").hide();
        } else {
            //$("#divUnlimBudget").show();
            $("#divBudgetLimit").show();
        }
    }

    function cbParameterChange(value) {
        toggleExtraBudgetVisibility(value);

        sendCommand("action=constraint_id&value=" + value, true);
    }

    function rbPlotModesClick(value) {
        plot_point_by_point = value;
        sendCommand("action=plot_point_by_point&value=" + value, true);
    }

    function ShowDollarValueClick(value) {
        showDollarValue = value;
        sendCommand("action=show_dollar_value&value=" + value, true);
    }

    function onPlotParameter(value) {
        plotParameter = value;
        sendCommand("action=plot_parameter&value=" + value, true);
    }

    //function onShowRiskDeltas(value) {
    //    showRiskDeltas = value;
    //    sendCommand("action=show_deltas&value=" + value, true);
    //}

    function onShowLECValuesClick(value) {
        showLECValues = value;
        sendCommand("action=show_lec_values&value=" + value);
    }

    function inputKeyDown(e, tb) {
        if (e.keyCode == KEYCODE_ENTER) tb.blur();
    }

    function inputChanged(value) {
        randSeed = value*1;
        sendCommand('action=save_seed&value=' + value, true);
    }

    function newSeed() {
        if (keepSeed) {
            sendCommand('action=new_seed', true);
        }
    }

    function onKeepSeed(value) {
        keepSeed = value; 
        sendCommand("action=keep_seed&value=" + value, true);
    }

    function saveNumerOfSimulations(value) {
        if (validInteger(value)) {
            var v = str2int(value);
            if (v !== numSimulations) {
                numSimulations = v;
                sendCommand("action=num_sim&val=" + v);
            }
        }
    }

    function initSplitters() {
        $("#tdChart").dxResizable({
            handles: 'right',
            width: layoutMode == 2 ? "100%" : (horizontalSplitterSize == UNDEFINED_INTEGER_VALUE ? "100%" : horizontalSplitterSize),
            onResize: function (e) {
                //document.getElementById("tdGrid").style.display = "none";
                //document.getElementById("tdChart").style.display = "none";
                //resizePage();
            },
            onResizeEnd: function (e) {
                horizontalSplitterSize = e.width;
                //document.getElementById("tdGrid").style.display = document.getElementById("cbGridView").checked ? "" : "none";
                //document.getElementById("tdChart").style.display = document.getElementById("cbChartView").checked ? "" : "none";
                resizePage();
                sendCommand("action=h_splitter_size&value=" + e.width, false);
            }
        });
        //if (layoutMode == 0) $("#tdChart").find(".dx-resizable-handle").addClass("splitter_v");
    }

    function hotkeys(event) {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            switch (code) {
                case KEYCODE_LEFT:
                case KEYCODE_RIGHT:
                    // switch between Chart and Data views on left and right arrow keys pressed
                    //var cbView = document.getElementById("cbView")
                    //viewChange(cbView.checked ? 0 : 1);
                    break;
                case KEYCODE_ESCAPE:
                    //allTooltipsShown = false;
                    //document.getElementById("imgPinAll").className = "fas fa-thumbtack unpinned";
                    //hidePinnedTooltips();
                    break;
            }
        }
    }

    function togglePinOptions() {
        optionsVisible = !optionsVisible;
        document.getElementById("iconToggleOptions").className = optionsVisible ? "fas fa-angle-double-up fa-lg" : "fas fa-angle-double-down fa-lg";
        initPinOptions();
        sendCommand('action=show_options&val=' + optionsVisible);
        setTimeout("resizePage();", 600);
    }

    function initPinOptions() {
        if (optionsVisible) {
            $("#trIgnores").slideDown();
            //$(".ui-button").slideDown();
        } else {
            $("#trIgnores").slideUp();
            //$(".ui-button").slideUp();
        }
    }

    function ExpandSL(expand) {
        var w = ((window.opener) ? window.opener : window.parent);
        if ((w) && ((typeof w.GetSLObject) == "function")) {
            var s = w.GetSLObject();
            if ((s)) s.Content.Shell.SetPrintVersion(expand);
        }
    }
    
    var is_expanded = false;
    
    function btnExpandClick() {
        is_expanded = !is_expanded;
        var img = document.getElementById("imgExpand");
        if ((img)) {
            img.title = (is_expanded ? "Normal View" : "Full View");
            img.src= (is_expanded ? '../../Images/ra/nofullscreen.png' : '../../Images/ra/fullscreen.png');
        }
        ExpandSL(is_expanded);
    }

    var dlg_settings;

    function onSettingsClick() {
        InitSettingsDlg();
        dlg_settings.dialog("open");
    }

    function InitSettingsDlg() {                
        dlg_settings = $("#divSettings").dialog({
            autoOpen: false,
            width: 430,
            height: "auto",
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: true,
            title: "Settings",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"Close", click: function() { dlg_settings.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
            },
            close: function() { $("body").css("overflow", "auto"); }
        });
    }

    function initIncrementInput() {
        switch (solverMode) {
            case <% = CInt(EfficientFrontierDeltaType.MinBenefitIncrease)%>:
                $("#tbIncrement").val(cur_min_benefit_increase);
                break;
            case <% = CInt(EfficientFrontierDeltaType.DeltaValue)%>:
                $("#tbIncrement").val(cur_increment);
                break;
            case <% = CInt(EfficientFrontierDeltaType.NumberOfSteps)%>:
                $("#tbIncrement").val(cur_num_inc);
                break;
            case <% = CInt(EfficientFrontierDeltaType.AllSolutions)%>:
                $("#tbIncrement").val(cur_decrement);
                break;
        }
    }

    function initMode() {
        $("#divModeDropdown").dxSelectBox({
            valueExpr: "value",
            displayExpr: "text",
            value: <% = CInt(Mode) %>,
            items: [
                    <%If PM.IsRiskProject Then %>
                    {value : <% = CInt(EfficientFrontierDeltaType.MinBenefitIncrease)%>, text : "<% = ResString("lblMinBenefitIncrease")%>" },
                    <%End If%>
                    {value : <% = CInt(EfficientFrontierDeltaType.DeltaValue)%>, text : "<% = ResString("lblSpecAmount")%>"},
                    {value : <% = CInt(EfficientFrontierDeltaType.NumberOfSteps)%>, text : "<% = ResString("lblNumIncrements")%>" },
                    {value : <% = CInt(EfficientFrontierDeltaType.AllSolutions)%>, text : "<% = ResString("lblAllSolutions")%>" }
            ],
            onValueChanged: function (e) {
                if (solverMode !== e.value) {
                    solverMode = e.value;
                    initIncrementInput();
                    sendCommand("action=solvermode&value=" + solverMode + "&num=" + $("#tbIncrement").val(), true);
                }
            }
        });
    }

    function initWidgetsWidth() {
        $("#divGridContainer").width(10);
        $("#divChartContainer").width(10);

        var w = $("#tblEfficientFrontierMain").width() - ($("#tdTree").is(":visible") ? $("#divHierarchy").width() : 0);
        layoutMode = layoutMode * 1;

        var margins = 10;

        switch (layoutMode) {
            case 0:
                if (horizontalSplitterSize == UNDEFINED_INTEGER_VALUE || horizontalSplitterSize <= 0) {
                    $("#divGridContainer").width(w / 2);
                    $("#divChartContainer").width(w / 2 - margins - 5);
                } else {
                    $("#divGridContainer").width(w - horizontalSplitterSize);
                    $("#divChartContainer").width(horizontalSplitterSize - margins - 5);
                }
                break;
            case 1: // Grid
                $("#divGridContainer").width(w);
                break;
            case 2: // Chart
                $("#divChartContainer").width(w - margins * 2 - 5);
                break;        
        }
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(grid_id, parent_id) {
        var margin = 4;
        $("#" + grid_id).height(0).width(0);
        var td = $("#" + parent_id);
        var w = $("#" + grid_id).width(Math.round(td.innerWidth())-margin).width();
        var h = $("#" + grid_id).height(Math.round(td.innerHeight())).height();
        if ((grid_w_old!=w || grid_h_old!=h)) {
            grid_w_old = w;
            grid_h_old = h;
        };
    }

    function resizePage(e) {
        initWidgetsWidth();
        
        $("divChart").hide();
        resizeDatatable();
        
        <% If PagesWrtObjective.Contains(CurrentPageID) Then %>
        resizeGrid("divHierarchy", "divTreeContainer");
        <% End If %>        

        resizeNowideCaptions();
        $("divChart").show();
    }

    function resizeNowideCaptions() {
        if ($("#tblEfficientFrontierMain").width() < 900) { $(".lbl-nowide1200").hide(); } else { $(".lbl-nowide1200").show(); }
    }

    $(document).ready(function () {                
        InitScenarios();
        initSplitters();
        initWidgetsWidth();
        initIgnores();
        initBaseCase();
        initMode();
        initIncrementInput();
        $('#tbIncrement').focus();
        updateButtons();
        updateToolbar();
        initSelectEventsForm();        
        initPinOptions();
        <%If CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES Or CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS Then%>
        initTreeList();
        <%End If%>
        $('#tbBudgetLimit').on('blur',function(e){
            setTimeout("saveBudget();", 500);
        });
        <%If Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue Then%>
        onPlottingOptionDlg();
        <%End If%>
        <%If PM.IsRiskProject Then%>
        var cb = document.getElementById("cbChartType");
        if ((cb)) { cb.options[0].innerText = chart_settings[2]; }
        <%End If%>
        updateToolbar();
        resizePage();
    });
    
    onSwitchAdvancedMode = function (value) { 
        updateToolbar();
        resizePage();
    };

    function saveBudget() {
        var value = "";
        var sval = str2cost($('#tbBudgetLimit').val());
        if (sval !== 'undefined') value = sval;
        sendCommand('action=edit_budget&value=' + value, false)
    }

    document.onclick = function () { if (is_context_menu_open == true) { $("div.context-menu").hide(200); UpdateCheckedScenarios(); is_context_menu_open = false; } };
    document.onkeypress = hotkeys;

    resize_custom = resizePage;
    toggleScrollMain();

    /* cancel on leaving the page */
    function onPageUnload(callback_func) {
        if (is_solving) {
            cf = callback_func;
            solveCancel();
        } else {
            callback_func();
        }
    }

    $(window).on('beforeunload', function() {
        return onPageUnload();
    });

    logout_before = function(callback_func) {
        onPageUnload(callback_func);
    }

</script>

<table id="tblEfficientFrontierMain" border='0' cellspacing='0' cellpadding='0' class='whole'>
<tr valign='top' id="trToolbar" class='text'>
<td valign="top" style="min-height: 24px;" class="text" colspan='3'>
    <div id='divToolbar' class='text ec_toolbar' style="margin:0px;"><table border="0" cellpadding="2" cellspacing="2" class="text">
        <tr>
            <td class="ec_toolbar_td_separator text" style="white-space: nowrap;">
                <button type="button" class="button" style='height: 28px; padding: 5px; vertical-align:middle; text-overflow:ellipsis;' id='btnSolve' onclick="solveStart();"><img src="<% =ImagePath %>assembly-16.png" width=16 height=16 style="vertical-align:middle; padding:0px; margin-top:-4px" >&nbsp;&nbsp;<%=ResString("btnRASolve")%></button>
                <button type="button" class="button" style='height: 28px; padding: 5px; vertical-align:middle; text-overflow:ellipsis; <% = If(PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value, "", " display: none; ") %>' id='btnCancel' onclick="solveCancel();" disabled="disabled"><i class="fas fa-ban"></i>&nbsp;&nbsp;<%=ResString("btnCancel")%></button>
                <%If Not PM.IsRiskProject Then%>
                <button type="button" class="button" style='width: 22ex; height: 28px; padding: 5px; vertical-align:middle; text-overflow:ellipsis;' id='btnSelectScenarios' onclick="btnSelectScenariosDropdown(this);"><img src="<% =ImagePath %>scenarios-20.png" width=16 height=16 style="vertical-align:middle; padding:0px; margin-top:-2px" />&nbsp;<%=ResString("lblSelectScenarios")%>&nbsp;&#40;<span id="lbl_scen_count"></span>&#41;&nbsp;&#x25BE;</button>
                <%End If%>
                <button type="button" class="button" style='height: 28px; padding: 5px; vertical-align:middle; text-overflow:ellipsis;' id="btnSettings" onclick="onSettingsClick();"><i class="fas fa-cog"></i>&nbsp;<%=ResString("lblRASettings") %></button>
            </td>
            <td class="ec_toolbar_td_separator text">
                <%If App.isRiskEnabled Then%>
                <div style="display: inline-block;">
                <nobr>
                    <label>
                        <input type='checkbox' id='cbUseSimulatedOutput' <%=If(PM.Parameters.Riskion_Use_Simulated_Values <> SimulatedValuesUsageMode.Computed, " checked='checked' ", "") %>  onclick='setSimulationMode();' />                    
                        <span class="text">Simulated</span>
                    </label>
                </nobr>
                </div>
                <nobr><label><input type='checkbox' id="cbShowLECValues" onclick='onShowLECValuesClick(this.checked);' <% = If(PM.Parameters.EfficientFrontierCalculateLECValues, " checked='checked' ", "")%> <% = If(PM.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.Computed, " disabled='disabled' ", "") %>/>LEC Values</label></nobr>
                &nbsp;|&nbsp;
                <div style="display: inline-block;" class="on_advanced">
                    <span style="white-space: nowrap;">Number of trials:</span>
                    <input type="text" id="tbNumSimulations" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=PM.RiskSimulations.NumberOfTrials.ToString%>" style="width:70px; margin-top:2px; text-align:right;" onblur="saveNumerOfSimulations(this.value);" <%=If(PM.Parameters.Riskion_Use_Simulated_Values = SimulatedValuesUsageMode.Computed, " disabled='disabled' ", "")%> />
                </div>
                <% If False Then %>
                <div style="display: inline-block;" class="on_advanced">
                    <span style="white-space: nowrap;">Probability Calculation:</span>
                    <select class="select" id='cbObjectiveFunctionType' onclick="if (this.value*1 != objectiveFunctionType) { objectiveFunctionType = this.value*1; sendCommand('action=objective_function_type&value='+this.value); }">
                        <option value="0" <%=If(PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType = ObjectiveFunctionType.SumProduct, " selected='selected'", "") %>>Sum Product</option>
                        <option value="1" <%=If(PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType = ObjectiveFunctionType.Combinatorial, " selected='selected'", "") %>>Combinatorial</option>
                    </select>
                </div>
                <%End If%>
                <%End If%>
            <select onchange="rbPlotModesClick(this.value == 1);">
                <option title="Display one point at a time (takes longer)" value="1" <% = If(Not PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue OrElse (PM.Parameters.EfficientFrontierPlotPointByPoint.HasValue AndAlso PM.Parameters.EfficientFrontierPlotPointByPoint.Value), " selected", "")%>>One point at a time</option>
                <option title="Display all points at once (delay until plot appears)" value="0" <% = If(Not PM.Parameters.EfficientFrontierPlotPointByPoint, " selected", "")%>>All points at once</option>
            </select>
            <%If PM.IsRiskProject And DollarValueOfEnterprise <> UNDEFINED_INTEGER_VALUE Then%>
            <select onchange="ShowDollarValueClick(this.value == 1);">
                <option value="0" <% = If(Not ShowDollarValue, " selected ", "") %>>Percentages</option>
                <option value="1" <% = If(ShowDollarValue, " selected ", "") %> <% = If(DollarValue = UNDEFINED_INTEGER_VALUE, " disabled='disabled' ", "") %> title="<% = GetDollarValueFullString() %>">Monetary Values</option>
            </select>
            <% End If %>
            <%If Not PM.IsRiskProject Then%>
            <div id="pnlCCSettings" class="ec_toolbar_td" style="padding-right: 0px; display: inline-block;">
                <div style="display: inline-block;" class="lbl-nowide1200"><label class='toolbar-label' title='(Budget and Cost only)' style='cursor: default;'>X-Axis:&nbsp;</label></div>
                <div id="divCCDropdown" style="display: inline-block;">
                <%=GetParameterDropdown() %>
                </div>                
            </div>
            <div style="display: inline-block;">
                <div id="divBudgetLimit" style="background-color:#f5f5f5; padding-bottom: 3px; border: 1px solid #c0c0c0;<%=If(SelectedConstraintID = Integer.MaxValue OrElse SelectedConstraintID = -1, "display:none;","display:block;")%>">
                <div style='text-indent:-20px;padding-left:20px;' id="divUnlimBudget"><nobr><label><input type='checkbox' id="cbUnlimBudget" class='checkbox' <% = If(PM.Parameters.EfficientFrontierUseUnlimitedBudgetForCC, " checked", "") %> onclick='onUnlimBudgetClick(this.checked);' /><%=ParseString("Unlimited Budget")%></label></nobr></div>
                <div class="small" style='padding-right:3px; text-align: left;'>
                    <nobr><span><%=ResString("lblROBudgetLimit")%>:</span>&nbsp;<%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%> 
                    <input type="text" id="tbBudgetLimit" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=CostString(If(PM.IsRiskProject, RA.RiskOptimizer.BudgetLimit, RA.Solver.BudgetLimit))%>" <%=If(PM.Parameters.EfficientFrontierUseUnlimitedBudgetForCC, " disabled='disabled' ", " ") %> style="width:80px; height:16px; padding:0px; margin-top:2px; text-align:right; font-size: 8pt;" />
                    </nobr>
                </div>                        
                </div>
            </div>            
            <% End If %>
            <div style="text-align: right; display: inline-block;">
                <% If App.isExportAvailable Then %>
                <a href='' style='cursor: pointer; background-color: white; width: 25px; display: inline-block; text-align: center; border: 1px solid #ccc; padding: 5px; border-radius: 2px;' id="excel" onclick="$('.dt-buttons').find('.buttons-copy').click(); return false;" aria-disabled="disabled"><i class="fas fa-paste fa-lg"></i></a>
                <% End If %>
                <a href='' style='cursor: pointer; background-color: white; width: 25px; display: inline-block; text-align: center; border: 1px solid #ccc; padding: 5px; border-radius: 2px;' onclick='togglePinOptions(); return false;'><i id="iconToggleOptions" class="fas fa-angle-double-up fa-lg"></i></a>
            </div>
        </td>
        </tr>
    </table>
 </div>
    <div id="divTimeElapsed" style="padding-top: 2px; padding-left: 15px; display: inline-block; text-align: right; position: absolute; right: 10px;" class="text gray xsmall"></div>
</td>
</tr>
<tr id="trTitle" valign='top' align='left' class='text'>
<td style="height:24px; padding:4px 4px 0px 4px;" class="text" colspan='3'>
    <h5 id="lblPageTitle" style='margin: 0px 30px; padding: 5px 0px 0px 0px;'><%=ResString("titleIncreasingBudgets")%> for &quot;<% = ShortString(SafeFormString(App.ActiveProject.ProjectName), 85)%>&quot;</h5>    
</td>
</tr>

<!-- Ignore options and Base Case options -->

<tr id="trIgnores" valign='top'>
    <!-- Ignore -->
    <td align='center' valign='top' style='text-align:center; overflow:hidden; padding-top:0px;' colspan='3'>
<center><div id='divIgnores' runat='server' style='overflow:hidden; padding: 4px 4px 10px 4px;'><table border='0' cellpadding='0' cellspacing='0'>
    <tr><td valign='top'> 
    <div class='text' style='overflow:hidden;margin:0px 5px 0px 0px;'>
        <fieldset class="legend" runat="server" id="pnlOptions" style='padding-top: 2px; padding-bottom: 2px; min-width: 280px;'>
        <legend class="text legend_title" style="white-space:normal; display:table;"><span>&nbsp;<%=ResString("titleIncBudgetsPnl")%>:</span></legend>
            <nobr><div id="divModeDropdown" style="margin: 2px; display: inline-block; vertical-align: middle;"></div>:&nbsp;<input type='text' id="tbIncrement" class='input' style='width: 90px; text-align: right;' onchange='setIncrement(this, this.value);' /></nobr>
        </fieldset>    
    </div>
    </td>
    <td valign='top'>
        <div style="overflow: hidden; margin-left: 5px;">
            <%--<fieldset class="legend" style="display:inline-block; width:auto; padding-bottom:3px; padding-top:0px;" runat="server" id="pnlBaseCase">
            <legend class="text legend_title">&nbsp;<label><input type='checkbox' name='cbBaseCase' value='1' <% =GetOptionValue("cbBaseCase")%> onclick='switchBaseCase(); onOptionClick(this.name, this.checked);'/><%=ResString("lblBaseCasePnl")%>:</label>&nbsp;</legend>--%>
            <fieldset class="legend text" runat="server" id="pnlSettings" style='padding-top: 2px; padding-bottom: 2px; height:auto;'>
                <legend class="text legend_title">&nbsp;Grid Options:</legend>
                <label><input type='checkbox' id="cbKeepFunded" class='checkbox' <%=If(Mode = EfficientFrontierDeltaType.AllSolutions OrElse Not PM.Parameters.EfficientFrontierIsIncreasing," disabled='disabled' ", "") %> <%=If(KeepFundedAlts, " checked", "") %> onclick='sendCommand("action=keep_funded&checked=" + this.checked, true);' /><%=If(App.isRiskEnabled, ResString("lblEnsureFundedControls"), ResString("lblEnsureFundedAlts"))%></label><br/>
                <label><input type='checkbox' id="cbGridMode" class='checkbox' <% = If(gridModeShowProjects, " checked", "") %> onclick='onGridModeClick(this.checked, false);' /><% = ResString(If(PM.IsRiskProject, "lblGridViewModeECR", "lblGridViewModeECC"))%></label>
                <%--<%If False And PM.IsRiskProject Then%><nobr><label title="Show Δ" style="vertical-align: middle;"><input type="checkbox" id="cbShowRiskDeltas" class="checkbox" <%=If(PM.Parameters.EfficientFrontierShowDelta, " checked='checked' ", "") %> onclick="onShowRiskDeltas(this.checked);" />Show Δ</label></nobr><%End If%>--%>
            </fieldset>
        </div>    
    </td>
    <td valign='top'>
        <div style="overflow: hidden; margin-left: 5px;">
            <fieldset class="legend on_advanced" style='display: block; margin-top:6px; padding-top: 2px; padding-bottom: 2px; width:190px; text-align:left; height:auto;' id="pnlBaseCase">&nbsp;<div class="multi-scenario-ui" style="display: inline-block;"><legend class="text legend_title" style="white-space:normal; display:table;">&nbsp;<label><input type='checkbox' name='cbBaseCase' value='1' <% =If(RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions, "checked='checked'", "")%> onclick='onBaseCaseOptionsClick(this.checked);'/><%=ResString("lblBaseCasePnlTemp")%>:</label>&nbsp;</legend>
            <label class='text'><input type='checkbox' class='checkbox' name='cbBaseCaseIncludesM' value='1' <% =GetOptionValue("cbBaseCase")%> onclick='switchBaseCase(); onOptionClick("cbBaseCase", this.checked);'/><%=ResString("lblBaseCasePnl")%>:</label>&nbsp;
            </div>
            <div class="single-scenario-ui" style="display: inline-block;">
            <legend class="text legend_title"><label><input type='checkbox' class='checkbox' name='cbBaseCaseIncludesS' value='1' <% =GetOptionValue("cbBaseCase")%> onclick='switchBaseCase(); onOptionClick("cbBaseCase", this.checked);'/><%=ResString("lblBaseCasePnl")%>:</label>&nbsp;</legend>
            </div>
            <table id='tblBaseCase' border='0' cellspacing='0' cellpadding='0' style='padding-left:24px;'>    
            <tr valign="top"><td class='text' align='left'><div id='optBaseCase'>
            <label id='lblBSGroups' <% =GetOptionStyle("cbBCGroups")%>><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbBCGroups")%> name='cbBCGroups'/> <%=ResString("optBCGroups")%></label> &nbsp;<%--<a href='<% =PageURL(_PGID_RA_GROUPS, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a>--%><br />
            </div></td></tr></table>
            </fieldset>        
        </div>    
    </td>
    </tr>
    <tr>
        <td valign='top' colspan="3">        
    <div style='overflow: hidden;'>
    <fieldset class="legend" style='padding-top: 2px; padding-bottom: 2px;' id="pnlIgnore"  onmouseover="ignoreOptionsMouseOver(1);" onmouseout="ignoreOptionsMouseOver(0);">
    <legend class="text legend_title">&nbsp;<div class="single-scenario-ui" style="display: inline-block;"><%=ResString("lblIgnorePnl")%>:</div><div class="multi-scenario-ui" style="display: inline-block;"><label id='lblIgnoreOptions'><input type='checkbox' name='cbIgnoreOptions' value='1' <% =If(RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, "checked='checked'", "")%> onclick='onIgnoreOptionsClick(this.checked);'/><%=ResString("lblIgnorePnlTemp")%>:</label></div>&nbsp;</legend>
    <table id='tblIgnores' border='0' cellspacing='0' cellpadding='0'>
    <tr valign='top'><td class='text' align='left' style='text-align:left;'>
    <nobr><label id='lblcbOptMusts'        <% =GetOptionStyle("cbOptMusts")%>       ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptMusts")%>        name='cbOptMusts' id='cbMusts' /> <%=ResString("optIgnoreMusts")%></label></nobr>
    <nobr><label id='lblcbOptMustNots'     <% =GetOptionStyle("cbOptMustNots")%>    ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptMustNots")%>     name='cbOptMustNots' /> <%=ResString("optIgnoreMustNot")%></label></nobr>
        <nobr><label id='lblcbOptGroups'       <% =GetOptionStyle("cbOptGroups")%>      ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptGroups")%>       name='cbOptGroups' /> <%=ResString("optIgnoreGroups")%></label><% If OPT_SHOW_IGNORE_LINKS Then%> &nbsp;<a href='<% =PageURL(If(PRJ.IsRisk, _PGID_RISK_THREAT_GROUPS, _PGID_RA_GROUPS), GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a><%End If%></nobr>
    <% If Not App.isRiskEnabled Then %>
    <nobr><label id='lblcbOptConstraints'  <% =GetOptionStyle("cbOptConstraints")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptConstraints")%>  name='cbOptConstraints' /> <%=ResString("optIgnoreCC")%></label></nobr>
    <% End If %>
    <nobr><label id='lblcbOptDependencies' <% =GetOptionStyle("cbOptDependencies")%>><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptDependencies")%> name='cbOptDependencies' /> <%=ResString("optIgnoreDependencies")%></label><% If OPT_SHOW_IGNORE_LINKS Then%>&nbsp;<a href='<% =PageURL(_PGID_RA_DEPENDENCIES, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a><%End If%></nobr>
    <%--</td><td class='text' align='left' style='padding-left:1em'>--%>    
    <nobr><label id='lblcbOptFundingPools' <% =GetOptionStyle("cbOptFundingPools")%>><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptFundingPools")%> name='cbOptFundingPools' /> <%=ResString("optIgnoreFP")%></label></nobr><% If OPT_SHOW_IGNORE_LINKS Then%> &nbsp;<a href='<% =PageURL(_PGID_RA_FUNDINGPOOLS, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a><%End If%>
    <% If Not App.isRiskEnabled Then %>
    <nobr><label id='lblcbOptRisks'        <% =GetOptionStyle("cbOptRisks")%>       ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptRisks")%>        name='cbOptRisks' /> <%=ResString("optIgnoreRisks")%></label></nobr>
    <% If ShowDraftPages() OrElse Not isDraftPage(_PGID_RA_TIMEPERIODS_SETTINGS) Then%><nobr><label id='lblcbOptTimePeriods'  <% =GetOptionStyle("cbOptTimePeriods")%> ><input type='checkbox' class='checkbox' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptTimePeriods")%>  name='cbOptTimePeriods' /> <%=ResString("optTimePeriods")%></label></nobr><% end if %>
    <% Else %>
    <label id='lblcbOptTimePeriods'  <%=If(RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0, "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.TimePeriods, "checked='checked'", "")%> id='cbOptTimePeriods' /> <%=ResString("optTimePeriods")%></label>
    <% End If %>
    </td></tr></table>
    <div id="divIgnoreBtns" style="display:none; position: absolute; margin-bottom: 10px; z-index: 9999;">
        <input type="button" class='button button_small' id='btnUseAll' value='<%=ResString("btnUseAll")%>' style='width:70px' onclick="return SetOptAll(<%=If(OPT_SHOW_AS_IGNORES, 0, 1)%>);" />
        <input type='button' id='btnIgnoreAll' value='<%=ResString("btnIgnoreAll")%>' class="button button_small" style='width:70px;' onclick="    return SetOptAll(<% =If(OPT_SHOW_AS_IGNORES, 1, 0) %>);" />
    </div>
    </fieldset></div>
</td>
    </tr>
</table></div></center>   
</td>
</tr>
<tr valign='top' style='height:100%;'>
<% If CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_EFFICIENT_FRONTIER_TO_OBJS Then %>
<td id="tdTree">
    <div id="divTreeContainer" valign='top' class="whole" style="border-spacing: 0px; width: 250px;">
        <div id='divHierarchy'>
    </div>
</td>
<% End If %>
<!-- Chart -->
<td id='tdChart' style="width: 50%; height:100%; text-align: left; <%=If(LayoutMode = 0 OrElse LayoutMode = 2, "display: ;", "display: none;")%>" valign='top' align="left">
    <div id="divChartContainer" class="whole" style="position: relative;">
        <div id="divChart" class="whole" style="margin: 0;"></div>        
    </div>
    <!-- Splitter here -->
</td>
<!-- Grid -->
<td id='tdGrid' valign='top' align="left" style="width: 50%; height:100%; overflow: hidden; text-align: left; <% = If(LayoutMode = 0 OrElse LayoutMode = 1, "display: ;", "display: none;")%>">
    <div id='divGridContainer' class="whole">
        <table id='tableContent' class="row-border"></table>
    </div>
</td>
</tr>

</table>

<div id="divNoDataTitle" style="position: absolute; background-color: white; margin-left: -5%; text-align: center; left: 50%; top: 50%;"><h6><%=ResString("msgRAClickSolveIB") %></h6></div>

<% If App.isRiskEnabled Then %>
<!-- Events with Checkboxes dialog -->
<div id='selectEventsForm' style='display: none; position: relative;'>
    <div id="divSelectEvents" style="padding: 5px; text-align: left;"></div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllEvents(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllEvents(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>
<% End If%>

<div id="divSettings" style="display: none;">
    <center>
    <fieldset class="legend text" runat="server" style="width: 380px; text-align: left; display: block;">
        <legend class="text legend_title">&nbsp;Display Settings:</legend>
        <input type='checkbox' class='input checkbox' id='cbShowCents' <% = If(ShowCents, " checked", "") %> onclick='showCents = this.checked; sendCommand("action=show_cents&chk=" + this.checked);' /><label for='cbShowCents'><%=ParseString("Show cents for monetary values")%></label><br/>
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'><%=ResString("lblDecimals") + ":"%>&nbsp;</label>&nbsp;&nbsp;
        <select class='select' name='cbDecimals' style='width:12ex;' onchange='cbDecimalsChange(this.value);'>
            <option value='0'<% = If(PM.Parameters.DecimalDigits = 0, " selected", "") %>>0</option>
            <option value='1'<% = If(PM.Parameters.DecimalDigits = 1, " selected", "") %>>1</option>
            <option value='2'<% = If(PM.Parameters.DecimalDigits = 2, " selected", "") %>>2</option>
            <option value='3'<% = If(PM.Parameters.DecimalDigits = 3, " selected", "") %>>3</option>
            <option value='4'<% = If(PM.Parameters.DecimalDigits = 4, " selected", "") %>>4</option>
            <option value='5'<% = If(PM.Parameters.DecimalDigits = 5, " selected", "") %>>5</option>
        </select>&nbsp;</nobr><br/>
    </fieldset>
    <br/>
    <fieldset class="legend text" runat="server" style="width: 380px; text-align: left;">
        <legend class="text legend_title">&nbsp;Chart Settings:</legend>
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'><%=ResString("lblTitle")%>:&nbsp;</label><input type='text' id="tbTitle" style='width:180px; text-align:Left; margin:2px;' onchange='setTitle(this.value);' onkeydown='optionKeyDown(event,this,this.value,"<%=PlotTitle%>",10);' value='<%=PlotTitle%>' /></nobr>
        <br />
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'><%=ResString("lblXAxisLabel")%>:&nbsp;</label><input type='text' id="tbXLabel" style='width:180px; text-align:Left; margin:2px;' onchange='setXLabel(this.value);' onkeydown='optionKeyDown(event,this,this.value,"<%=AxisXTitle%>",11);' value='<%=AxisXTitle%>' /></nobr>
        <br />
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'><%=ResString("lblYAxisLabel")%>:&nbsp;</label><input type='text' id="tbYLabel" style='width:180px; text-align:Left; margin:2px;' onchange='setYLabel(this.value);' onkeydown='optionKeyDown(event,this,this.value,"<%=AxisYTitle%>",12);' value='<%=AxisYTitle%>' /></nobr>
        <br /><br />

        <!--<legend class="text legend_title">&nbsp;X-axis range:</legend>-->
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'>X-axis <%=ResString("lblEfficientFrontierFrom")%>:&nbsp;</label>
        <input type='text' id="tbCalculateFrom" style="width:180px;text-align:right;margin:2px;" value="<%=If(IsCalculateFromDefined, CostString(CalculateFrom), "") %>" /></nobr>
        <br />
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'>X-axis <%=ResString("lblEfficientFrontierTo")%>:&nbsp;</label>
        <input type='text' id="tbCalculateTo" style="width:180px;text-align:right;margin:2px;" value="<%=If(IsCalculateToDefined, CostString(CalculateTo), "") %>"/></nobr>
        <%If PM.IsRiskProject Then%>
        <br><br/>
        <nobr><label class='toolbar-label' style='cursor: default; display: inline-block; width: 90px; text-align: right;'>Y-axis parameter:&nbsp;</label>
        <div style="display: inline-block; margin-left: 4px;">
            <select id="cbChartType" onchange="onPlotParameter(this.value);" style="min-width: 230px;">
                <option value="priority" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "priority", " selected", "")%>>Optimized Risk</option>
                <option value="leverage" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "leverage", " selected", "")%>><%=ResString("tblRowLeverageGlobal")%></option>
                <option value="delta_leverage" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "delta_leverage", " selected", "")%>><%=ResString("tblRowDeltaLeverage")%></option>
                <option value="savings" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "savings", " selected", "")%>><%=ResString("tblRowSavings")%></option>
                <option value="delta_savings" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "delta_savings", " selected", "")%>><%=ResString("tblRowDeltaSavings")%></option>
                <option value="percent_lec" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "percent_lec", " selected", "")%>>Loss Exceedance</option>
                <option value="likelihood_lec" <%=If(PM.Parameters.EfficientFrontierPlotParameter = "likelihood_lec", " selected", "")%>><%=ParseString("%%Likelihood%% of Exceeding")%></option>
            </select>
        </div></nobr>
        <%End If%>
    </fieldset>
    <% If App.isRiskEnabled Then %>
    <br/>
    <fieldset class="legend text" runat="server" style="width: 380px; text-align: left;"><legend class="text legend_title">&nbsp;Simulation Settings:</legend>
        <div style="width: 100%; text-align: center;">
        <%--<nobr class="on_advanced">--%>
        <nobr style="display: none;">
            Consequences simulation mode:&nbsp;
            <select class="select" id='cbDilutedMode' onclick="if (this.value != dilutedMode) { dilutedMode = this.value; sendCommand('action=diluted_mode&value='+this.value); }">
                <option value="0" <%=If(PM.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted, " selected='selected'", "") %>>Diluted</option>
                <option value="1" <%=If(PM.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Undiluted, " selected='selected'", "") %>>Undiluted</option>
            </select>
        </nobr>
        </div>
        <div>
        <span id="lblSeed" style="display: inline-block; width: 90px; text-align: right;">Seed:</span>
        <input type="number" id="tbSeed" class="input number" step='1' min="1" max="<%=Integer.MaxValue%>" value="<% = PM.RiskSimulations.RandomSeed %>" style="width:40px; height:20px; margin-top:2px; text-align:right;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown="inputKeyDown(event, this);" onchange="inputChanged(this.value);" />
        <i class="fas fa-sync-alt ec-icon" id="btnNewSeed" style="vertical-align: middle; padding-bottom: 1px; cursor: pointer;" title="New Seed" onclick="newSeed();"></i>
        <label title="<%=ResString("lblKeepRandSeed")%>" style="margin-left: 40px;"><input type="checkbox" id="cbKeepSeed" class="checkbox" <% = If(PM.RiskSimulations.KeepRandomSeed, " checked='checked' ", "") %> onclick="onKeepSeed(this.checked);" /><%=ResString("lblKeepRandSeed")%></label>
        </div>
    </fieldset>
    <% End If%>
    </center>
</div>

<span id="msgs" style="position: absolute; top: 75px; display: none;"></span>

<script language="javascript" type="text/javascript">

    /* Long Polling */

    var _method_POST = "POST";
    var ajaxMethod = _method_POST; // don't use method GET because of cacheing

    function initPolling() {
        //showLoadingIcon();
        callAjax("<%=_PARAM_ACTION %>=poll&prjid=<%=PRJ.ID%>&polling_user_id=<%=App.ActiveUser.UserID%>", onPollCallback, ajaxMethod, true, "IncreasingBudgetsService.aspx");
    }

    function cancelPolling() {
        //showLoadingIcon();
        callAjax("<%=_PARAM_ACTION %>=cancel&prjid=<%=PRJ.ID%>&polling_user_id=<%=App.ActiveUser.UserID%>", onPollCancel, ajaxMethod, true, "IncreasingBudgetsService.aspx");
    }

    function onPollCancel(data) {
        hideLoadingPanel();
        solveStop(false);
        stopProgress();
    }

    function onPollCallback(data) {
        var res = JSON.parse(data);
        if (res && res.length > 0) {
            if (res[0] !== "poll_completed") {
                var continue_polling = true;

                $("#msgs").html($("#msgs").html() + "<br/><small>" + "Solved " + res.length + " datapoints" + "</small>");
                var dp_to_draw = 0;


                for (var i = 0; i < res.length; i++) {
                    var dp = res[i][2];
                    var dp_token = res[i][3];
                                   
                    if (dp[0] !== "finished") {
                        //add a datapoint to the plot and grid

                        curScenarioIndex = dp[IDX_SOLVER_CUR_SCENARIO_INDEX];

                        if (dp[2] >= 0 && dp_token == solve_token) {
                            if (dp[IDX_SOLVER_BENEFIT] > chart_settings[6]) chart_settings[6] = dp[IDX_SOLVER_BENEFIT] * 1.01;
                            originalDatapoints.push(dp);
                            addDatapoint(dp);
                            dp_to_draw += 1;
                        }
                    } else {
                        if (curScenarioIndex == getCheckedScenariosCount() - 1) {
                            continue_polling = false;
                            sendCommand("action=release_solver", false);
                            hideLoadingPanel();
                            solveStop(false);
                            stopProgress();
                            setTimeout("resizePage();", 100);
                        }
                    }
                }

                if (dp_to_draw > 0) {
                    updateDatagrid();
                    updateChart();
                }
                if (continue_polling) setTimeout("initPolling();", 50); // 60 * 5 seconds passed or update received, restart polling with a micro-delay
            } 
        }
    }

    </script>

</asp:Content>