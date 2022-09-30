<%@ Page Language="VB" Inherits="RiskPlotPage" title="RiskPlot" Codebehind="RiskPlot.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script language="javascript" type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
    g.dxc-val-constant-lines > text {
        fill: black !important;
    }
    g.dxc-arg-constant-lines > text {
        fill: black !important;
    }
</style>
<script language="javascript" type="text/javascript">
    
    var columns_ver = "210726";
    var canUseControls = <% = Bool2JS(CanUseControls) %>;

    var please_wait_delay = 2000; //ms
    var decimals = <% = PM.Parameters.DecimalDigits %>;
    var skipNonZeroCents = <% = Bool2JS(CostDecimalDigits = 0)%>;
    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    var DollarValueOfEnterprise = <%=JS_SafeNumber(If(PM.DollarValueOfEnterprise = UNDEFINED_INTEGER_VALUE, 0, PM.DollarValueOfEnterprise))%>;
    var costOfControls = <% = JS_SafeNumber(PM.Controls.EnabledControls.Sum(Function (ctrl) If(ctrl.Active, ctrl.Cost, 0)))%>;
    var intersectionOptions = <% = PM.Parameters.LEC_ShowIntersectionsOptions %>; // {red_line: true, green_line: true, red_line_intersection: true, green_line_intersection: false, red_line_intersection_wc: true, green_line_intersection_wc: false}

    var events_data = <% = GetEventsData() %>;
    var sources_data = <% = GetSourcesData() %>;

    var IDX_ID = "id";
    var IDX_NAME = "name";

    var COLOR_RED_LINE = '#ff2727'; // $steel
    var COLOR_GREEN_LINE = '#ff2727'; // $ec-blue-normal
    var COLOR_EVENTS_LINE = "#073763";  //$ec-blue-dark
    var COLOR_EVENTS_LINE_WC = "#6aa84f"; // with controls //#c4e2f2

    var percentages_data_events = [];
    var percentages_data_sources = [];
    var percentages_data_impacts = [];

    window.loss_chart = {};    
    window.freq_chart = {};    
    window.revr_chart = {};    

    var displayCostValue = <%=Bool2JS(ShowDollarValue)%>;
    var curView = 0; // 0 - chart, 1 - datatable
    
    var simulationMode = <%=CInt(SimulationMode)%>; // 0 - _SIMULATE_SOURCES, 1 - _SIMULATE_EVENTS
    var _SIMULATE_SOURCES = <%=RiskSimulations._SIMULATE_SOURCES%>;
    var _SIMULATE_EVENTS = <%=RiskSimulations._SIMULATE_EVENTS%>;

    var keepSeed = <% = Bool2JS(PM.RiskSimulations.KeepRandomSeed)%>;
    <%--var showIntersections = <%=Bool2JS(PM.Parameters.LEC_ShowIntersections)%>;--%>
    var showFreqCharts = <%=Bool2JS(PM.Parameters.LEC_ShowFrequencyCharts)%>;
    var logScale = <% = Bool2JS(PM.Parameters.LEC_LogarithmicScale) %>;
    var showWithoutControlsLine = <% = Bool2JS(PM.Parameters.LEC_ShowWithoutControls) %>;
    var showWithControlsLine = <% = Bool2JS(PM.Parameters.LEC_ShowWithControls) %>;

    var isOpportunityModel = <%=Bool2JS(PRJ.isOpportunityModel)%>;

    var use_event_groups = <%=Bool2JS(PM.RiskSimulations.UseEventsGroups)%>;
    var use_source_groups = <%=Bool2JS(PM.RiskSimulations.UseSourceGroups)%>;

    var events_filter_name = unescape("<%=JS_SafeString(GetCookie("events_filter_name", ""))%>");
    var red_line_value = <%=JS_SafeNumber(RedLineValue)%>;    
    var red_line_data = [];

    var green_line_value = <%=JS_SafeNumber(GreenLineValue)%>;    

    var lblLoss = "<% = If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "gain", "loss")%>";
    var lblLosing = "<% = If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "gaining", "losing")%>";

    var sim_start = 0;
    var sim_end = 0;

    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_CLASS = 3;
    var COL_COLUMN_DATA_FIELD = 4;
    var COL_COLUMN_WIDTH = 5;
    var COL_COLUMN_ALLOW_FILTERING = 6;
    var COL_COLUMN_ALLOW_GROUPING = 7;

    var selectedHierarchyNodeID = "<% = SelectedHierarchyNodeID.ToString %>";
    var hierarchy_data = <% = GetHierarchyData() %>;
    var hierarchy_columns = <% = GetHierarchyColumns(CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS) %>;

    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;

    var last_exec_time_ms = 1000;

    var sessAutoPlotYAxisMin = localStorage.getItem("LEC_Y_MIN_<% = PRJ.ID %>");
    var autoPlotYAxisMin = !(sessAutoPlotYAxisMin) || typeof sessAutoPlotYAxisMin == "undefined" ? false : str2bool(sessAutoPlotYAxisMin);

    var sessShowFiredOnlySources = localStorage.getItem("LEC_FIRED_ONLY_SOURCES_<% = PRJ.ID %>");
    var showFiredOnlySources = !(sessShowFiredOnlySources) || typeof sessShowFiredOnlySources == "undefined" ? true : str2bool(sessShowFiredOnlySources);

    var sessShowFiredOnlyEvents = localStorage.getItem("LEC_FIRED_ONLY_EVENTS_<% = PRJ.ID %>");
    var showFiredOnlyEvents = !(sessShowFiredOnlyEvents) || typeof sessShowFiredOnlyEvents == "undefined" ? true : str2bool(sessShowFiredOnlyEvents);

    var exp_val_data;
    
    /* jQuery Ajax */
    var orig_received_data_string = "";

    function syncReceived(params) {
        current_ajax_call_completed = true;
        hideLoadingPanel();

        if ((params)) {                        
            var received_data = eval(params);
            if ((received_data)) {
                var can_update_exec_time = false;

                //if (received_data[0] == "simulate" || received_data[0] == "simulation_mode" || received_data[0] == "display_mode" || received_data[0] == "with_controls_mode" || received_data[0] == "truncate_impacts" || received_data[0] == "limit_impacts" || received_data[0] == "select_category" || received_data[0] == "use_event_groups" || received_data[0] == "allow_events_fire_multiple_times" || received_data[0] == "red_line" || received_data[0] == "green_line") {
                if (received_data[0] == "simulate" || received_data[0] == "simulation_mode" || received_data[0] == "display_mode" || received_data[0] == "with_controls_mode" || received_data[0] == "limit_impacts" || received_data[0] == "select_category" || received_data[0] == "use_source_groups" || received_data[0] == "use_event_groups" || received_data[0] == "allow_events_fire_multiple_times" || received_data[0] == "red_line" || received_data[0] == "green_line") {
                    orig_received_data_string = params;

                    var main_chart_data = received_data[1];
                    var freq_chart_data = received_data[2];
                    var cumu_chart_data = eval(params)[1]; // copy of data

                    red_line_data = received_data[5];
                    green_line_value = received_data[5][4];

                    percentages_data_events = received_data[6];
                    percentages_data_sources = received_data[7];
                    percentages_data_impacts = received_data[8];

                    $('#tbSimulations').val(received_data[9]);
                    $('#tbDatapoints').val(received_data[10]);

                    exp_val_data = received_data[3];   // expected value independent, expected value mutually exclusive
                    var cvar_data = received_data[11];
                    showExpectedValue(exp_val_data, cvar_data);

                    var seed = received_data[4];
                    $("#tbSeed").val(seed);

                    $(".lec_chart").hide();

                    createChart("loss_chart", main_chart_data, 'divChart', isOpportunityModel, red_line_data, true); // reversed cumulative frequency chart
                    if (showFreqCharts) {
                        freqChart("freq_chart", freq_chart_data, "divChart1"); // frequency chart
                        createChart("revr_chart", cumu_chart_data, "divChart2", !isOpportunityModel, [], false);  // cumulative frequency chart
                    }
                    createSteps();

                    if ($('#tbSimulations').val()*1 > 0) getStepData(1);
                    viewChange(curView);

                    can_update_exec_time = true;
                }
                if (can_update_exec_time || received_data[0] == "ajax_cancel") {
                    setTimeout("stopProgress();", 200);
                    sim_end = performance.now();
                    var isCancelled = received_data[0] == "ajax_cancel";
                    last_exec_time_ms = sim_end - sim_start;
                    if ($("#txtTime").length) $("#txtTime").text("Total execution time" + (isCancelled ? " (<% = ResString("msgCancelled").ToLower() %>)" : "" ) + ": " + (last_exec_time_ms / 1000).toFixed(3) + " s");
                }
                if (received_data[0] == "view_step") {
                    showAllStepsData(received_data[1]);
                    var tbSteps = document.getElementById("tbSteps");
                    if ((tbSteps)) tbSteps.disabled = false;
                    // stopProgress();
                    // scroll to top
                    // window.scrollTo(0, 0);
                }
                if (received_data[0] == "new_seed") {
                    $("#tbSeed").val(received_data[1]);
                    runSimulation();
                }
                if (received_data[0] == "keep_seed") {
                    keepSeed = received_data[1];
                }
                if (received_data[0] == "show_intersections" || received_data[0] == "show_freq_charts" || received_data[0] == "selected_node") {
                    runSimulation();
                }
                if (received_data[0] == "log_scale") {
                    $(".lec_chart").each(function () {
                        if ($(this).data("dxChart")) {
                            var c = $(this).dxChart("instance");
                            c.option("argumentAxis.type", logScale ? 'logarithmic' : 'continuous');  
                        }
                    });
                }                
                if (received_data[0] == "ajax_cancel") {
                    $(".lec_chart").html("<% = GetDefaultMessage(False) %>");
                    $(".chart_loading_msg").html("<% = ResString("msgCancelled") %>");
                }
            }

        }        
        
        restoreControls();
        $("#btnRun").prop("disabled", false);
        $("#btnCancel").prop("disabled", true);

        updateToolbar();
        initChartsLayout();
    }

    function syncError() {
        current_ajax_call_completed = true;
        hideLoadingPanel();
        if (_ajax_show_error) {
            dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error");
            $(".ui-dialog").css("z-index", 9999);
        }
    }

    var current_ajax_call_completed = false;
    function sendCommand(params, showPleaseWait) {
        if (typeof showPleaseWait == "undefined" || showPleaseWait) showLoadingPanel();
        //current_ajax_call_completed = false;
        //setTimeout(function () {
        //    if (!current_ajax_call_completed) showLoadingPanel();
        //}, please_wait_delay * 1);

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function cancelCommand() {
        _ajaxCancel();
        syncReceived('["ajax_cancel"]');
        //setTimeout("sendCommand('action=ajax_cancel', true);", 100);
    }
    // end jQuery Ajax

    function getRiskLabel(v) {
        var riskLabel = "";
        <%If PRJ.RiskionProjectType = ProjectType.ptMixed Then%>
        if (v < 0) {
            riskLabel = "<%=ParseString("Net %%risk%%")%>";
        } else {
            riskLabel = "<%=ParseString("Net opportunity")%>";
        }
        <%Else%>
        riskLabel = "<%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "Average gain", "Average %%loss%%"))%>";
        <%End If%>
        return riskLabel;
    }

    function updateExtraInfo(exp_val_data, index) {
        var expValTitle = ""; <%--'<%=ResString("lblExpectedValueIndivEvents",,,True)%>';--%>
        
        var h0 = "", c0 = "", c1 = "", c2 = "", c3 = "";

        var cell_color = COLOR_EVENTS_LINE;
        switch (index) {
            case 0:
                expValTitle += "<input type='checkbox' style='vertical-align: middle;' " + (showWithoutControlsLine ? "checked='checked'" : "") + " onclick='onSwitchLine(0, this.checked);' /><div style='height: 12px; width: 12px; background-color: #915514; display: inline-block; margin: 5px; vertical-align: middle;'></div>" + "<% = If(CanUseControls, ParseString("Without %%controls%%"), "")%>";
                cell_color = COLOR_EVENTS_LINE;
                break;
            case 1:
                expValTitle += "<input type='checkbox' style='vertical-align: middle;' " + (showWithControlsLine ? "checked='checked'" : "") + " onclick='onSwitchLine(1, this.checked);' /><div style='height: 12px; width: 12px; background-color: #6aa84f; display: inline-block; margin: 5px; vertical-align: middle;'></div>" + "<% = If(CanUseControls, ParseString("With %%controls%%"), "") %>";
                cell_color = COLOR_EVENTS_LINE_WC;
                break;
        }
        
        var cbDisplay = document.getElementById("cbDisplay");
        if ((cbDisplay)) displayCostValue = ((cbDisplay)) && (cbDisplay.value == "1");

        h0 = expValTitle;
        var extraInfo = "";

        //var cell = $("#tblExpectedValue").find("tr:eq(0)>td:eq(" + index + ")");
        if (exp_val_data[0][index] != UNDEFINED_INTEGER_VALUE) {
            var expValue = displayCostValue ? showCost(exp_val_data[0][index], true, skipNonZeroCents) : exp_val_data[0][index] + "%";
            if (d1 == "undefined") d1 = exp_val_data[0][index]; else d1 -= exp_val_data[0][index];

            extraInfo += expValTitle + "<br>" + getRiskLabel(expValue) + ":&nbsp;" + expValue;

            c0 = ""; //"<span style='color: " + cell_color + "; font-size: medium; font-weight: bold;'>" + (index == 1 || index == 3 ? "────────────────" : "----------------------------") + "</span>";
            c1 = expValue;
        
            if (exp_val_data[1][index] != UNDEFINED_INTEGER_VALUE) {
                extraInfo += "probability that <%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "gain", "%%loss%%"))%> will exceed " + (displayCostValue ? showCost(exp_val_data[1][index], true, skipNonZeroCents) : Math.round(exp_val_data[1][index] * 100)/100 + "%");
                c2 = displayCostValue ? showCost(exp_val_data[1][index], true, skipNonZeroCents) : Math.round(exp_val_data[1][index] * 100)/100 + "%";
                if (d2 == "undefined") d2 = exp_val_data[1][index]; else d2 -= exp_val_data[1][index];
            }
            extraInfo += "</span>";
            extraInfo += "<br><span style='color:" + COLOR_GREEN_LINE + ";'>VAR,&nbsp;<%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "gain", "%%loss%%"))%>:&nbsp;";
            if (green_line_value != UNDEFINED_INTEGER_VALUE) {
                extraInfo += (exp_val_data[2][index] == UNDEFINED_INTEGER_VALUE || exp_val_data[2][index] == 0 ? "~0" : Math.round(exp_val_data[2][index] * 100)/100) + (isOpportunityModel ? "% chance of gaining more than " : "% chance of losing more than "); // + " of funds";
                c3 = (exp_val_data[2][index] == UNDEFINED_INTEGER_VALUE || exp_val_data[2][index] == 0 ? "~0" : Math.round(exp_val_data[2][index] * 100)/100) + "%";
                if (d3 == "undefined") { d3 = exp_val_data[2][index] == UNDEFINED_INTEGER_VALUE || exp_val_data[2][index] == 0 ? 0 : Math.round(exp_val_data[2][index] * 100)/100; } else { d3 = exp_val_data[2][index] == UNDEFINED_INTEGER_VALUE || exp_val_data[2][index] == 0 ? 0 : Math.round(exp_val_data[2][index] * 100)/100; }
            }
            extraInfo += "</span>";
        }
        if (extraInfo != "") {
            var dataField = index === 0 ? "result" : "result_wc";
            totalsColumns.push({ "caption" : index == 0 ? "<% = If(CanUseControls, ParseString("Without %%controls%%"), "Value") %>" : "<% = ParseString("With %%controls%%") %>", headerCellTemplate : function (columnHeader, headerInfo) {                    
                return columnHeader.append(h0);
            }, "dataField" : dataField, "alignment" : "right", "encodeHtml" : false, "width" : 120 });
            totalsData[0][dataField] = c1 + "<input type='checkbox' style='opacity:0;margin-left: 5px;'/>";
            totalsData[1][dataField] = c2 + "<input class='cb_red_line " + (index === 0 ? "cb_woc" : "cb_wc") + "' type='checkbox' style='margin-left: 5px;" + (index === 0  && showWithoutControlsLine || index !== 0  && showWithControlsLine ? "opacity:1;" : "opacity:0;") + "' " + ((index == 0 ? intersectionOptions.red_line_intersection : intersectionOptions.red_line_intersection_wc) ? "checked='checked'" : "") + " onclick='" + (index == 0 ? "intersectionOptions.red_line_intersection" : "intersectionOptions.red_line_intersection_wc")+" = this.checked; updateIntersectionOption(true);' " + (intersectionOptions.red_line ? "" : "disabled='disabled' ") + "/>"; //red line
            totalsData[2][dataField] = c3 + "<input class='cb_green_line " + (index === 0 ? "cb_woc" : "cb_wc") + "' type='checkbox' style='margin-left: 5px;" + (index === 0  && showWithoutControlsLine || index !== 0  && showWithControlsLine ? "opacity:1;" : "opacity:0;") + "' " + ((index == 0 ? intersectionOptions.green_line_intersection : intersectionOptions.green_line_intersection_wc) ? "checked='checked'" : "") + " onclick='"+ (index == 0 ? "intersectionOptions.green_line_intersection" : "intersectionOptions.green_line_intersection_wc")+" = this.checked; updateIntersectionOption(true);' " + (intersectionOptions.green_line ? "" : "disabled='disabled' ") + "/>"; //green line
        }
    }

    var tableTotals;
    var totalsColumns = [];
    var totalsData = [];
    var d1, d2, d3;

    function showExpectedValue(exp_val_data, cvar_data) { // expected value independent, expected value mutually exclusive
        var tbl = (($("#tblExpectedValue").data("dxDataGrid"))) ? $("#tblExpectedValue").dxDataGrid("instance") : null;
        if (tbl !== null) { tbl.dispose(); }

        totalsColumns = [];
        totalsData = [];

        totalsData.push({});
        totalsData.push({});
        totalsData.push({});
        totalsData.push({});
        //TODO CVAR totalsData.push([]);

        totalsColumns.push({ "caption" : "<% = If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, ParseString("%%Risk%% Impacts"), ParseString("%%Risk%% Impacts")) %>", "dataField": "name", "alignment" : "left", "encodeHtml" : false });
        totalsData[0].name = "<input type='checkbox' style='opacity:0;'/>" + getRiskLabel(exp_val_data[0]) + (exp_val_data[0]*exp_val_data[1]<0 ? "<span style='color:"+COLOR_EVENTS_LINE_WC+"'/> / " + getRiskLabel(exp_val_data[1]) + "</span>": "");
        totalsData[1].name = "<input type='checkbox' " + (intersectionOptions.red_line ? "checked='checked'" : "") + " onclick='intersectionOptions.red_line = this.checked; $(\".cb_red_line\").prop(\"disabled\", !this.checked); updateIntersectionOption(true);' />VAR,&nbsp;probability: <a href='' class='actions' onclick='updateRedLabelValue(); return false;'>" + red_line_value + "%</a> probability that <%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "gain", "%%loss%%"))%> will exceed";
        var gBtn;
        if (green_line_value != UNDEFINED_INTEGER_VALUE) {
            var cur_cost = displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : green_line_value;
            gBtn = "<a href='' class='actions' onclick='updateGreenLabelValue(); return false;' style='color:" + COLOR_GREEN_LINE + "'>" + (cur_cost == "" ? "undefined" : (displayCostValue ? cur_cost : num2prc(cur_cost/100, 2))) + "</a>";
        } else {
            gBtn = "<a href='' class='actions' onclick='updateGreenLabelValue(); return false;'  style='color:" + COLOR_GREEN_LINE + "'>undefined</a>";
        }
        totalsData[2].name = "<input type='checkbox' " + (intersectionOptions.green_line ? "checked='checked'" : "") + " onclick='intersectionOptions.green_line = this.checked; $(\".cb_green_line\").prop(\"disabled\", !this.checked); updateIntersectionOption(true);' />VAR,&nbsp;<%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "gain", "%%loss%%"))%>: " + (<%=Bool2JS(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed)%> ? "Percent chance of gaining more than " : "% chance of losing more than ") + gBtn;
        totalsData[3].name = "<input type='checkbox' style='opacity:0;'/>" + "<% = ParseString("Cost of %%Controls%%") + HowControlsSelected() %>";

        d1 = "undefined"; d2 = "undefined"; d3 = "undefined";

        updateExtraInfo(exp_val_data, 0);
        updateExtraInfo(exp_val_data, 1);
        
        totalsData[3].result = showCost(0) + "<input type='checkbox' style='opacity:0;margin-left: 5px;'/>";
        totalsData[3].result_wc = showCost(costOfControls) + "<input type='checkbox' style='opacity:0;margin-left: 5px;'/>"; // cost of active controls
        
        if (<% = Bool2JS(CanUseControls) %>) {
            totalsColumns.push({ "caption" : "Δ", "dataField": "delta", "alignment" : "right", "encodeHtml" : false, "width" : 120 });
            totalsData[0].delta = d1 == "undefined" ? "" : (displayCostValue ? showCost(d1, true, skipNonZeroCents) : Math.round(d1*100)/100 + "%");


            totalsData[1].delta = d2 == "undefined" ? "" : (displayCostValue ? showCost(d2, true, skipNonZeroCents) : Math.round(d2*100)/100 + "%");
            totalsData[2].delta = d3 == "undefined" ? "" : (Math.round(d3*100)/100 + "%");

            totalsData[3].delta = showCost(-costOfControls); // cost of controls delta
        }

        if (<% = Bool2JS(Not CanUseControls) %>) {
            totalsData.splice(3, 1);
        }

        var storageKey = "LEC_Total_<%=PRJ.ID%>_" + columns_ver;
        
        tableTotals = $("#tblExpectedValue").dxDataGrid({
            allowColumnResizing: true,
            columns: totalsColumns,
            dataSource: totalsData,
            "export": {
                enabled: true,
                customizeExcelCell: function(options) {
                    var gridCell = options.gridCell;
                    if(!gridCell) {
                        return;
                    }

                    if (gridCell.rowType === "data") {
                        options.value = htmlStrip(gridCell.value);
                    }
                },
                fileName: "<% = SafeFileName(GetTitle(False)) %>"
            },
            onRowPrepared: function (e) {
                if (e.rowType === "data" && e.rowIndex === 0) {
                    e.rowElement.css({'font-weight' : 'bold', 'font-size' : 'medium', 'color' : 'black'});
                }
            },
            sorting: {
                mode: "none"
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: storageKey
            },
        });

        resizeGrid("tblExpectedValue", "divExpectedValue");
    }

    function updateIntersectionOption(fSave) {
        var lineWidth = 2;
        var c = $("#divChart").dxChart("instance");
        
        c.option("valueAxis.constantLines[0].width", intersectionOptions.red_line ? lineWidth : 0);
        c.option("valueAxis.constantLines[0].label.text", intersectionOptions.red_line ? c.option("valueAxis.constantLines[0].label.textOrig") : "");
        
        c.option("argumentAxis.constantLines[0].width", intersectionOptions.green_line ? lineWidth : 0);        
        c.option("argumentAxis.constantLines[0].label.text", intersectionOptions.red_line ? c.option("argumentAxis.constantLines[0].label.textOrig") : "");
        
        c.option("annotations[0].text", showWithoutControlsLine && intersectionOptions.red_line && intersectionOptions.red_line_intersection ? c.option("annotations[0].textOrig") : "");
        c.option("annotations[0].opacity", showWithoutControlsLine && intersectionOptions.red_line && intersectionOptions.red_line_intersection ? 1 : 0);
        c.option("annotations[1].text", showWithControlsLine && intersectionOptions.red_line && intersectionOptions.red_line_intersection_wc ? c.option("annotations[1].textOrig") : "");
        c.option("annotations[1].opacity", showWithControlsLine && intersectionOptions.red_line && intersectionOptions.red_line_intersection_wc ? 1 : 0);

        c.option("annotations[2].text", showWithoutControlsLine && intersectionOptions.green_line && intersectionOptions.green_line_intersection ? c.option("annotations[2].textOrig") : "");
        c.option("annotations[2].opacity", showWithoutControlsLine && intersectionOptions.green_line && intersectionOptions.green_line_intersection ? 1 : 0);
        c.option("annotations[3].text", showWithControlsLine && intersectionOptions.green_line && intersectionOptions.green_line_intersection_wc ? c.option("annotations[3].textOrig") : "");
        c.option("annotations[3].opacity", showWithControlsLine && intersectionOptions.green_line && intersectionOptions.green_line_intersection_wc ? 1 : 0);

        if (fSave) {
            sendCommand('action=intersection_options&value=' + JSON.stringify(intersectionOptions), false);
        }
    }

    // Simulation Chart
    function createChart(plot_object, chart_data, chart_id, is_cumulative, red_line_data, showAnnotations) {
        if ($("#" + chart_id).data("dxChart")) $("#" + chart_id).dxChart("instance").dispose();
        $("#" + chart_id).show();

        var cbDisplay = document.getElementById("cbDisplay");
        if ((cbDisplay)) displayCostValue = ((cbDisplay)) && (cbDisplay.value == "1");

        if ((chart_data)) {
            //check if data array is empty or all array elements are empty
            var chart_data_len = chart_data.length;
            var has_data = chart_data_len > 0;
                           
            if (!has_data) {
                $("#" + chart_id).html("<%=GetDefaultMessage(False)%>");
                return false;
            }
            
            //if (chart_data[0].length == 0) chart_data[0].push([]);

            var dataSource = [];
            var maxVal = 0;

            for (var i = 0; i < chart_data[0].length; i++) {
                dataSource.push({"loss" : chart_data[0][i].loss, "lec" : is_cumulative ? 1 - chart_data[0][i].lec : chart_data[0][i].lec});
            }

            for (var i = 0; i < chart_data[1].length; i++) {
                dataSource.push({"loss" : chart_data[1][i].loss, "lec_wc" : is_cumulative ? 1 - chart_data[1][i].lec_wc : chart_data[1][i].lec_wc});
            }

            for (var i = 0; i < dataSource.length; i++) {
                if (maxVal < dataSource[i].lec) maxVal = dataSource[i].lec;
                if (maxVal < dataSource[i].lec_wc) maxVal = dataSource[i].lec_wc;
            }

            var avg_loss = "Average " + lblLoss + " (Risk)" + (showWithControlsLine && canUseControls ? ", without <% = ParseString("%%controls%%") %>" : "") + " = " + (displayCostValue ? showCost(exp_val_data[0][0], true, skipNonZeroCents) : num2prc(exp_val_data[0][0] / 100, 2));
            if (showWithControlsLine && canUseControls) {
                avg_loss += "\nAverage " + lblLoss + " (Risk), with <% = ParseString("%%controls%%") %> = " + (displayCostValue ? showCost(exp_val_data[0][1], true, skipNonZeroCents) : num2prc(exp_val_data[0][1] / 100, 2));
            }

            window[plot_object] = $("#" + chart_id).dxChart({
                palette: "bright",
                dataSource: dataSource,
                crosshair: {
                    enabled: true,
                    color: "#777777"
                },
                commonSeriesSettings: {
                    type: "spline",
                    argumentField: "loss"
                },
                commonAxisSettings: {
                    grid: {
                        visible: false
                    }
                },
                series: [
                    { valueField: "lec", name: "<% = ParseString("Without %%Controls%%") %>", point: {size: 8}, color: "#915514", visible: showWithoutControlsLine /*DevExpress.viz.getPalette("bright").simpleSet[1]*/ },
                    { valueField: "lec_wc", name: "<% = ParseString("With %%Controls%%") %>", point: {size: 8}, color: "#6aa84f", visible: showWithControlsLine /*DevExpress.viz.getPalette("bright").simpleSet[0]*/ }
                ],
                tooltip: {
                    enabled: true,
                    format: "percent",
                    precision: decimals, //maxVal > 0.5 ? 0 : (maxVal < 0.2 ? 2 : 1),
                    //argumentFormat: displayCostValue ? "currency" : "percent"
                    contentTemplate: function(info, container) {
                        var c = $("#" + chart_id).dxChart("instance");
                        var xLabel = c.option("argumentAxis.title.text")
                        var yLabel = c.option("valueAxis.title.text")
                        yLabel = yLabel.substr(0, yLabel.length - 3);
                        $("<div>" +
                            yLabel + ": " + num2prc(info.value, 2, true) + "<br>" +
                            xLabel + " <= " + (displayCostValue ? showCost(info.argument, true, skipNonZeroCents) : num2prc(info.argument, 2, true)) + "</div>").appendTo(container);
                    }
                },
                legend: {
                    verticalAlignment: "bottom",
                    horizontalAlignment: "center",
                    visible: false
                },
                "export": {
                    enabled: true
                },
                annotations: !showAnnotations ? null : [{
                    allowDragging: true,
                    type: "text",
                    //text: red_line_value + "% avg loss probability<br/>WITHOUT controls = " + (displayCostValue ? showCost(red_line_data[0], true, skipNonZeroCents) : num2prc(red_line_data[0], 2, true)),
                    //textOrig: red_line_value + "% avg loss probability<br/>WITHOUT controls = " + (displayCostValue ? showCost(red_line_data[0], true, skipNonZeroCents) : num2prc(red_line_data[0], 2, true)),
                    text: "<% = If(CanUseControls, ParseString("WITHOUT %%controls%%<br/>"), "") %>" + (red_line_value) + "% chance of " + lblLosing + " more than " + (displayCostValue ? showCost(red_line_data[0], true, skipNonZeroCents) : num2prc(red_line_data[0] / 100, 2, true)),
                    textOrig: "<% = If(CanUseControls, ParseString("WITHOUT %%controls%%<br/>"), "") %>" + (red_line_value) + "% chance of " + lblLosing + " more than " + (displayCostValue ? showCost(red_line_data[0], true, skipNonZeroCents) : num2prc(red_line_data[0] / 100, 2, true)),
                    argument: red_line_data[0] / (displayCostValue ? 1 : 100),
                    value: red_line_value / 100,
                    opacity: 1
                }, {
                    allowDragging: true,
                    type: "text",
                    text: "<% = If(CanUseControls, ParseString("WITH %%controls%%<br/>"), "") %>" + (red_line_value) + "% chance of " + lblLosing + " more than " + (displayCostValue ? showCost(red_line_data[1], true, skipNonZeroCents) : num2prc(red_line_data[1] / 100, 2, true)),
                    textOrig: "<% = If(CanUseControls, ParseString("WITH %%controls%%<br/>"), "") %>" + (red_line_value) + "% chance of " + lblLosing + " more than " + (displayCostValue ? showCost(red_line_data[1], true, skipNonZeroCents) : num2prc(red_line_data[1] / 100, 2, true)),
                    argument: red_line_data[1] / (displayCostValue ? 1 : 100),
                    value: red_line_value / 100,
                    opacity: 1
                }, {
                    allowDragging: true,
                    type: "text",
                    text:  "<% = If(CanUseControls, ParseString("WITHOUT %%controls%%<br>"), "") %>" + num2prc(red_line_data[2], 2, true) + " chance of " + lblLosing + "<br/>more than " + (displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : num2prc(green_line_value / 100, 2, true)),
                    textOrig:  "<% = If(CanUseControls, ParseString("WITHOUT %%controls%%<br>"), "") %>" + num2prc(red_line_data[2], 2, true) + " chance of " + lblLosing + "<br/>more than " + (displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : num2prc(green_line_value / 100, 2, true)),
                    argument: displayCostValue ? green_line_value : green_line_value / 100,
                    value: red_line_data[2],
                    opacity: 1
                }, {
                    allowDragging: true,
                    type: "text",
                    text: "<% = If(CanUseControls, ParseString("WITH %%controls%%<br>"), "") %>" + num2prc(red_line_data[3], 2, true) + " chance of " + lblLosing + "<br>more than " + (displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : num2prc(green_line_value / 100, 2, true)),
                    textOrig: "<% = If(CanUseControls, ParseString("WITH %%controls%%<br>"), "") %>" + num2prc(red_line_data[3], 2, true) + " chance of " + lblLosing + "<br>more than " + (displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : num2prc(green_line_value / 100, 2, true)),
                    argument: displayCostValue ? green_line_value : green_line_value / 100,
                    value: red_line_data[3],
                    opacity: 1
                }],
                argumentAxis: {
                    label:{
                        format: {
                            type: displayCostValue ? "currency" : "percent"
                        },
                        customizeText: function () {
                            return displayCostValue ? showCost(this.value, true, skipNonZeroCents) : this.valueText;
                        }
                    },
                    //allowDecimals: false,
                    title: {
                        text: ((cbDisplay)) ? cbDisplay.options[cbDisplay.selectedIndex].innerHTML : "<%=CStr(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, ParseString("Gain"), "Loss"))%>"
                    },
                    type: logScale ? 'logarithmic' : 'continuous',
                    constantLines: !showAnnotations ? null : [{
                        label: {
                            text: "Probability of " + lblLosing + " more than " + (displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : num2prc(green_line_value / 100, 2)),
                            textOrig: "Probability of " + lblLosing + " more than " + (displayCostValue ? showCost(green_line_value, true, skipNonZeroCents) : num2prc(green_line_value / 100, 2))
                        },
                        width: 2,
                        value: displayCostValue ? green_line_value : green_line_value / 100,
                        color: COLOR_GREEN_LINE,
                        //dashStyle: "dash"
                    }, {
                        label: {
                            font: {
                                size: "medium",
                                weight: "bold"
                            },
                            text: avg_loss,
                            textOrig: avg_loss,
                            verticalAlignment: "center",
                            horizontalAlignment: "left"
                        },
                        width: 2,
                        value: displayCostValue ? green_line_value : green_line_value / 100,
                        color: COLOR_GREEN_LINE,
                        //dashStyle: "dash"
                    }],
                    valueMarginsEnabled: false
                    //axisDivisionFactor: 60
                },
                valueAxis: {
                    //endOnTick: true,
                    label: {
                        //customizeText: function () {
                        //    return displayCostValue ? showCost(this.value, true, skipNonZeroCents) : this.valueText;
                        //},
                        format: {
                            type: "percent",
                            precision: decimals //maxVal > 0.5 ? 0 : (maxVal < 0.2 ? 2 : 1) 
                        }
                    },
                    title: {
                        text: is_cumulative ? "<%=ParseString("Probability, %")%>" : "<%=CStr(IIf(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, ParseString("%%Risk%% Exceedance Probability, %"), "Loss Exceedance Probability, %"))%>"
                    },
                    constantLines: !showAnnotations ? null : [{
                        label: {
                            text: "VAR, " + lblLoss + ": " + red_line_value + "%",
                            textOrig: "VAR, " + lblLoss + ": " + red_line_value + "%"
                        },
                        width: 2,
                        value: red_line_value / 100,
                        color: COLOR_RED_LINE,
                        //dashStyle: "dash"
                    }],
                    visualRange: { startValue: autoPlotYAxisMin ? undefined : 0 },
                    //wholeRange: [0, 1]
                    valueMarginsEnabled: false
                },
                onDone: function (e) {
                    
                },
                title: {
                    text: !showAnnotations ? "<% = If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "Cumulative Gain Frequency Chart", "Cumulative Loss Frequency Chart") %>" : "<% = GetTitle() %>",
                    font: {size: 17}
                }
            }).dxChart("instance"); 
        }
        updateIntersectionOption(false);
    }

    function updateRedLabelValue() {
        var text = red_line_value + "";
        var result = prompt('Enter value (0 - 100):', text);
        if (validFloat(result)) {
            var tmp = str2double(result);
            if (tmp >= 0 && tmp <= 100) {
                red_line_value = tmp;
                $("#lblRedLine").html(result + "%");               
                initSimulationStart();
                sendCommand("action=red_line&value=" + red_line_value, true);
            }
        }                
    }

    function updateGreenLabelValue() {
        var text = green_line_value == UNDEFINED_INTEGER_VALUE ? "" : green_line_value;
        var result = prompt('Enter value ' + (displayCostValue ? "in dollars" : "as a percentage, for example \"50\" for 50%") + ':', text);
        if (result !== null) {
            result = result.trim();
            if (result == "") {
                green_line_value = UNDEFINED_INTEGER_VALUE;
                sendCommand("action=green_line&value=", true);
                initSimulationStart();
            }
            if (validFloat(result)) {
                var tmp = str2double(result);
                green_line_value = tmp;
                sendCommand("action=green_line&value=" + green_line_value, true);
                initSimulationStart();
            }                
        }
    }
    // end Simulation Chart

    // Frequency Chart    
    function freqChart(plot_object, chart_data, chart_id) {
        if ($("#" + chart_id).data("dxChart")) $("#" + chart_id).dxChart("instance").dispose();
        $("#" + chart_id).show();

        var cbDisplay = document.getElementById("cbDisplay");
        if ((cbDisplay)) displayCostValue = ((cbDisplay)) && (cbDisplay.value == "1");

        if ((chart_data)) {
            //check if data array is empty or all array elements are empty
            var chart_data_len = chart_data.length;
            var has_data = chart_data_len > 0;
                           
            if (!has_data) {
                $("#" + chart_id).html("<%=GetDefaultMessage(False)%>");
                return false;
            }
            
            var dataSource = [];
            var maxVal = 0;

            for (var i = 0; i < chart_data[0].length; i++) {
                dataSource.push({"loss" : chart_data[0][i].loss, "freq" : chart_data[0][i].freq});
            }
            for (var i = 0; i < chart_data[1].length; i++) {
                dataSource.push({"loss_wc" : chart_data[1][i].loss, "freq_wc" : chart_data[1][i].freq_wc});
            }

            for (var i = 0; i < dataSource.length; i++) {
                if (maxVal < dataSource[i].lec) maxVal = dataSource[i].lec;
                if (maxVal < dataSource[i].lec_wc) maxVal = dataSource[i].lec_wc;
            }

            window[plot_object] = $("#" + chart_id).dxChart({
                palette: "bright",
                dataSource: dataSource,
                crosshair: {
                    enabled: true,
                    color: "#777777"
                },
                commonSeriesSettings: {
                    type: "bar",
                    argumentField: "loss"
                },
                commonAxisSettings: {
                    grid: {
                        visible: false
                    }
                },
                series: [
                    { argumentField: "loss", valueField: "freq", name: "<% = ParseString("Without %%Controls%%") %>",    point: {size: 5}, color: "#915514", visible: showWithoutControlsLine /*DevExpress.viz.getPalette("bright").simpleSet[1]*/ },
                    { argumentField: "loss_wc", valueField: "freq_wc", name: "<% = ParseString("With %%Controls%%") %>", point: {size: 5}, color: "#6aa84f", visible: showWithControlsLine /*DevExpress.viz.getPalette("bright").simpleSet[0]*/ }
                ],
                tooltip: {
                    enabled: true,
                    format: "percent",
                    precision: decimals, //maxVal > 0.5 ? 0 : (maxVal < 0.2 ? 2 : 1),
                    //argumentFormat: displayCostValue ? "currency" : "percent"
                    contentTemplate: function(info, container) {
                        var c = $("#" + chart_id).dxChart("instance");
                        var xLabel = c.option("argumentAxis.title.text")
                        var yLabel = c.option("valueAxis.title.text")
                        yLabel = yLabel.substr(0, yLabel.length - 3);
                        $("<div>" +
                            yLabel + ": " + num2prc(info.value, 2, true) + "<br>" +
                            xLabel + ": " + (displayCostValue ? showCost(info.argument, true, skipNonZeroCents) : num2prc(info.argument, 2, true)) + "</div>").appendTo(container);
                    }
                },
                legend: {
                    verticalAlignment: "bottom",
                    horizontalAlignment: "center",
                    visible: false
                },
                "export": {
                    enabled: true
                },
                argumentAxis: {
                    label:{
                        format: {
                            type: displayCostValue ? "currency" : "percent"
                        },
                        customizeText: function () {
                            return displayCostValue ? showCost(this.value, true, skipNonZeroCents) : this.valueText;
                        }
                    },
                    //allowDecimals: false,
                    title: {
                        text: ((cbDisplay)) ? cbDisplay.options[cbDisplay.selectedIndex].innerHTML : "<% = CStr(IIf(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, ParseString("%%Risk%%"), "Loss"))%>"                        
                    },
                    type: logScale ? 'logarithmic' : 'continuous',
                    valueMarginsEnabled: false
                },
                valueAxis: {
                    //endOnTick: true,
                    label: {
                        //customizeText: function () {
                        //    return displayCostValue ? showCost(this.value, true, skipNonZeroCents) : this.valueText;
                        //},
                        format: {
                            type: "percent",
                            precision: decimals //maxVal > 0.5 ? 0 : (maxVal < 0.2 ? 2 : 1) 
                        }
                    },
                    title: {
                        text: "Frequency, %"
                    },
                    visualRange: { startValue: autoPlotYAxisMin ? undefined : 0 },
                    valueMarginsEnabled: false
                },
                onDone: function (e) {
                    
                },
                title: {text: "<% = If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "Gain Frequency Chart", "Loss Frequency Chart") %>", font: {size: 17}}
            }).dxChart("instance");                  
        }
    }
    // end Frequency Chart

    // Simulation Data
    function createSteps() {        
        var tbSteps = document.getElementById("tbSteps");
        if ((tbSteps)) {
            //while (cbSteps.firstChild) {
            //    cbSteps.removeChild(cbSteps.firstChild);
            //}
            var numSimulations = $('#tbSimulations').val() * 1;
            //for (var i = 0; i < numSimulations; i++) {
            //    var opt = document.createElement("option");
            //    opt.value = i;
            //    opt.innerHTML = (i + 1) + "";
            //    cbSteps.appendChild(opt);
            //}
            tbSteps.disabled = true;
            if (numSimulations > 0) {
                tbSteps.value = 1;
                tbSteps.disabled = false;
            }
            tbSteps.style.display = "";
        }
        var cbFiredOnlySources = document.getElementById("cbFiredOnlySources");
        if ((cbFiredOnlySources)) cbFiredOnlySources.checked = showFiredOnlySources;        
        var cbFiredOnlyEvents = document.getElementById("cbFiredOnlyEvents");
        if ((cbFiredOnlyEvents)) cbFiredOnlyEvents.checked = showFiredOnlyEvents;        
    }

    function onShowFiredOnlySources(value) {
        showFiredOnlySources = value;
        localStorage.setItem("LEC_FIRED_ONLY_SOURCES_<% = PRJ.ID %>", value);
        showAllStepsData(cur_steps_data);
    }

    function onShowFiredOnlyEvents(value) {
        showFiredOnlyEvents = value;
        localStorage.setItem("LEC_FIRED_ONLY_EVENTS_<% = PRJ.ID %>", value);
        showAllStepsData(cur_steps_data);
    }

    function getStepData(step_num) {
        var tbSteps = document.getElementById("tbSteps");
        if ((tbSteps)) tbSteps.disabled = true;
        var numSimulations = $('#tbSimulations').val() * 1;
        if (step_num*1 <= <%=PM.RiskSimulations.NumberOfStepsToStore%> && step_num*1 <= numSimulations) {
            sendCommand('action=view_step&value=' + step_num, true);
        } else {
            if ((tbSteps)) tbSteps.disabled = false;
        }
    }

    var cur_steps_data;

    function showAllStepsData(steps_data) {
        cur_steps_data = steps_data;
        //// colors for sources
        //$("#tdStepSources").css("color", COLOR_EVENTS_LINE);
        //$("#tdStepWCSources").css("color", COLOR_EVENTS_LINE_WC);
        
        //$("#tblDataSources").css("color", COLOR_EVENTS_LINE);
        //$("#tblDataWCSources").css("color", COLOR_EVENTS_LINE_WC);
        
        //// colors for events
        //$("#tdStepIndp").css("color", COLOR_EVENTS_LINE);
        //$("#tdStepIndpWC").css("color", COLOR_EVENTS_LINE_WC);
        
        //$("#tblDataIndp").css("color", COLOR_EVENTS_LINE);
        //$("#tblDataIndpWC").css("color", COLOR_EVENTS_LINE_WC);

        //$("#divStepSources").css("width", canUseControls ? "14%" : "29%");
        //$("#divStepSourcesWC").css("width", canUseControls ? "14%" : "0%");
        //$("#divStepEvents").css("width", canUseControls ? "34%" : "69%");
        //$("#divStepEventsWC").css("width", canUseControls ? "34%" : "0%");

        // fill in sources columns
        if (simulationMode == _SIMULATE_SOURCES && (steps_data[0].length > 0 || steps_data[2].length > 0)) { // without controls
            createSourcesStepData(steps_data[0].length > 0 ? steps_data[0] : steps_data[2], "lblTotalsSources", "tblDataSources", "<% = If(CanUseControls, ParseString("%%Sources%% (without %%controls%%)"), ParseString("%%Sources%%")) %>");
            $("#divStepSources").show();
        } else { $("#divStepSources").hide(); }
        if (simulationMode == _SIMULATE_SOURCES && (steps_data[1].length > 0 || steps_data[3].length > 0)) { // with controls
            createSourcesStepData(steps_data[1].length > 0 ? steps_data[1] : steps_data[3], "lblTotalsSourcesWC", "tblDataSourcesWC", "<% = If(CanUseControls, ParseString("%%Sources%% (with %%controls%%)"), ParseString("%%Sources%%")) %>");
            $("#divStepSourcesWC").show();
        } else { $("#divStepSourcesWC").hide(); }        

        if (simulationMode == _SIMULATE_SOURCES || simulationMode == _SIMULATE_EVENTS) {
            // fill in events columns
            if (steps_data[0].length > 0) { // independend data step            
                createStepData(steps_data[0], "lblTotalsEvents", "tblDataEvents");
                $("#divStepSources").show();
            } else { $("#divStepSources").hide(); }
            if (steps_data[1].length > 0 && <% = Bool2JS(CanUseControls) %>) { // independend with controls data step
                createStepData(steps_data[1], "lblTotalsEventsWC", "tblDataEventsWC");
                $("#divStepSourcesWC").show();
            } else { $("#divStepSourcesWC").hide(); }
        }
    }

    function resizeGrid(grid_id, parent_id) {
        var margin = 4;
        $("#" + grid_id).height(0).width(0);
        var td = $("#" + parent_id);
        var w = $("#" + grid_id).width(Math.round(td.innerWidth())-margin).width();
        var h = $("#" + grid_id).height(Math.round(td.innerHeight() > 0 ? td.innerHeight() - 10 : td.parent().innerHeight() - 10)).height();
    }
    
    function createSourcesStepData(step_data, idTotals, idTbl, header) {
        //"<b>Name</b>  <b>Priority</b> <b>Random()</b>
        //$("#" + idTbl).css("background-color", "#e2f7df");
        //$("#" + idTbl).hide();
        //$("#" + idTbl).find("tr:gt(0)").remove();

        if ($("#" + idTbl).data("dxDataGrid")) {
            $("#" + idTbl).dxDataGrid("dispose");
        }

        //$("#" + idTbl).css("width", canUseControls ? "25%" : "50%");

        var step_data_sources = step_data[3];
        var step_data_sources_len = step_data_sources.length;
        var sources_data_len = sources_data.length;
        var fired_sources_count = 0;
        var ds = [];

        $.each(sources_data, function(i, e) {
            var sClass = "legend_row_normal";
            var se;
            for (var k = 0; k < step_data_sources_len; k++) {
                if (step_data_sources[k].id == e.id) {
                    se = step_data_sources[k];
                    k = step_data_sources_len;
                }
            }
            if ((se)) {
                if (se.id === e.id && se.fired) {
                    sClass = "legend_row_bold ra_funded";
                    fired_sources_count += 1;
                }
                
                //var source_name_td = '<td class="col0"></td>';
                
                var nameCell = "";
                //nameCell = "[" + e.id + "] " + ShortString(e.name, 45, true);
                nameCell = "[" + e.id + "] " + e.name;
                
                se.class = sClass;
                se.name = nameCell;
                //se.rand = se.rand >= 0 ? se.rand : "";
                //$('#' + idTbl).append('<tr class="' + sClass + '" valign="top" title="' + se[4] + '">' + source_name_td + '<td style="border:1px solid #ccc;text-align:right;">' + se[3] + '</td><td style="border:1px solid #ccc;text-align:right;">' + (se[2] >= 0 ? se[2] : "") + '</td></tr>');
                if (!showFiredOnlySources || se.fired) ds.push(se);
            }
        });

        var columns = [];            
        
        columns.push({ dataField: "name", caption: "<% = ParseString("%%Source%%") %>", allowReordering: true, allowEditing: false, visibleIndex: 0, encodeHtml: false });
        columns.push({ dataField: "priority", caption: "Probability", allowReordering: true, allowEditing: false, visibleIndex: 1,
            format: {
                type: "fixedPoint",
                precision: 4
            }
        });
        columns.push({ dataField: "rand", caption: "<% = ParseString("%%Source%% Random()") %>", allowReordering: true, allowEditing: false, visibleIndex: 2,
            format: {
                type: "fixedPoint",
                precision: 4
            }
            //customizeText: function(cellInfo) {
            //        return cellInfo.value > 0 ? cellInfo.value : "";
            //}
        });

        var dataSource = new DevExpress.data.ArrayStore({
            key: 'id',
            data: ds
        });

        $("#" + idTbl).dxDataGrid({
            allowColumnResizing: true,
            allowColumnReordering: false,
            columns: columns,
            columnResizingMode: "widget",
            columnAutoWidth: true,
            columnFixing: {
                enabled: true
            },
            columnChooser: {                
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: false
            },
            dataSource: dataSource,
            dataStructure: "plain",
            editing: {
                allowUpdating: false
            },
            "export": {
                enabled: true,
            },
            height: "auto",
            highlightChanges: false,
            hoverStateEnabled: true,                
            focusedRowEnabled: false,
            keyExpr: "id",
            loadPanel: {
                enabled: false,
            },
            rowAlternationEnabled: false,
            showColumnLines: true,
            showRowLines: false,
            sorting: {
                mode: "multiple"
            },
            columnAutoWidth: true, 
            pager: {
                allowedPageSizes: [10, 15, 20, 50, 100, 200, 500],
                showInfo: true,
                showNavigationButtons: true,
                showPageSizeSelector: true,
                visible: false
            },
            paging: {
                enabled: false,
                //pageSize: 10
            },
            onRowPrepared: function (e) {
                if (e.rowType === "data") {
                    e.rowElement.addClass(e.data.class);
                    if (e.data.fired) {
                        e.rowElement.css({'font-weight' : 'bold'});
                    }
                }
            },
            //scrolling: {
            //    mode: "virtual"
            //},
            showBorders: true,
            noDataText: "<% = GetEmptyMessage() %>",            
            //onToolbarPreparing: function (e) {
            //    var toolbarItems = e.toolbarOptions.items; 
            //    toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: '<h6 style="padding-top: 10px;">Participant Groups</h6>' });
            //},
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: function () { return "LEC_2_<% = CurrentPageID %>_PRJ_ID_<% = App.ProjectID %>"; }
            }, 
            wordWrapEnabled: false,
            width: "100%"
        });


        var lblTotals = document.getElementById(idTotals);
        //lblTotals.innerHTML = "Total <%=ParseString("%%impact%%")%> of simulation = " + (displayCostValue ? showCost(step_data[1], true, skipNonZeroCents) : step_data[1]);
        lblTotals.innerHTML = "&nbsp;" + header + "&nbsp;<br><small>Number of <%=ParseString("%%sources%%")%> that occured: " + fired_sources_count + "</small>";
        //} else {
        //    lblTotals.innerHTML += "<br><br><span class='warning'>Error</span>"; 
        //}       

        $("#" + idTbl).show();     
        setTimeout(function() { resizeGrid(idTbl, idTbl + "Parent"); }, 100);
    }

    function createStepData(step_data, idTotals, idTbl) {
        //"<b>Name</b>  <b>Likelihood</b> <b>Impact"+ (displayCostValue ? ", $" : "") + "</b> <b>Risk"+ (displayCostValue ? ", $" : "") + "</b>";
        var lblTotals = document.getElementById(idTotals);
        //lblTotals.innerHTML = "Expected Value = " + step_data[0] + "<br>" + "Sum of Impacts = " + (displayCostValue ? showCost(step_data[1], true, skipNonZeroCents) : step_data[1]);
        lblTotals.innerHTML = "Total <% = ParseString ("%%impact%%")%> of simulation: " + (displayCostValue ? showCost(step_data[1], true, skipNonZeroCents) : step_data[1]);
        
        //$("#" + idTbl).hide();
        //$("#" + idTbl).find("tr:gt(0)").remove();
        if ($("#" + idTbl).data("dxDataGrid")) {
            $("#" + idTbl).dxDataGrid("dispose");
        }

        //$("#" + idTbl).css("width", canUseControls ? "25%" : "50%");

        var ds = [];
        var step_data_events = step_data[2];
        var step_data_events_len = step_data_events.length;
        var events_data_len = events_data.length;
        var fired_events_count = 0;

        //if (step_data_events_len == events_data_len) {
        $.each(step_data_events, function(i, se) {
            var sClass = "legend_row_normal";
            var e = [];
            for (var k = 0; k < events_data_len; k++) {
                if (events_data[k].id == se.id) {
                    e = events_data[k];
                    k = events_data_len;
                }
            }
            if ((se)) {
                if (se.id === e.id && se.fired) {
                    sClass = "legend_row_bold ra_funded";
                    fired_events_count += 1;
                }
                //var eventNameCell = ShortString(e.name, 45, true);
                var eventNameCell = e.name;
                
                if (simulationMode == _SIMULATE_SOURCES && se.sourceId >= 0 && se.fired) {
                    eventNameCell += " <span style='color:#008000;'>&nbsp;<% = ParseString("%%Sources%%")(0).ToString.ToUpper %><sub>ID</sub>=[" + se.sourceId + "]</span>";
                }

                if (simulationMode == _SIMULATE_SOURCES) {
                    if (se.sourceId < -1) {
                        //sClass += " strike";
                        eventNameCell += " <small><span style='color:#008000;'>[<%=ParseString("No contribution with the %%sources%% that occured")%>]</span></small>";
                    } else {
                        if (se.sourceId < 0) {
                            eventNameCell += " <small><span style='color:#008000;'>[<%=ParseString("No %%Sources%%")%>]</span></small>";
                        }
                    }
                }                

                se.class = sClass;
                se.name = eventNameCell;
                if (!showFiredOnlyEvents || se.fired) ds.push(se);

                //var event_name_td = nameColumnExists ? '<td class="col0"></td>' : '<td class="col0" style="border:1px solid #ccc; white-space:nowrap; text-align:left;">' + eventNameCell + '</td>';
                //$('#' + idTbl).append('<tr class="' + sClass + '" valign="top" title="' + se[9] + '">' + event_name_td + '<td style="border:1px solid #ccc;text-align:right;">' + (simulationMode == _SIMULATE_SOURCES ? (se[4] >=0 ? se[4] : "") : e[2]) + '</td><td style="border:1px solid #ccc;text-align:right;">' + (se[2] >= 0 ? se[2] : "") + '</td><td style="border:1px solid #ccc;text-align:right;">' + (se[1] == 1 ? (displayCostValue ? showCost(se[6], true, skipNonZeroCents) : se[5]) : "") + '</td><td style="display:none;border:1px solid #ccc;text-align:right;">' + (se[1] == 1 ? (displayCostValue ? showCost(se[8], true, skipNonZeroCents) : se[7]) : "") + '</td></tr>');
            }
        });

        var columns = [];            
        
        columns.push({ dataField: "name", caption: "<% = ParseString("%%Alternative%%") %>", allowReordering: true, allowEditing: false, visibleIndex: 0, encodeHtml: false });
        columns.push({ dataField: "likelihood", caption: "<% = ParseString("%%Likelihood%%") %>", allowReordering: true, allowEditing: false, visibleIndex: 1,
            customizeText: function(cellInfo) {
                return cellInfo.value >= 0 ? cellInfo.valueText : "";
            },
            format: {
                type: "fixedPoint",
                precision: 4
            }
        });
        columns.push({ dataField: "rand", caption: "Random()", allowReordering: true, allowEditing: false, visibleIndex: 2,
            customizeText: function(cellInfo) {
                return cellInfo.value >= 0 ? cellInfo.valueText : "";
            },
            format: {
                type: "fixedPoint",
                precision: 4
            }
        });
        columns.push({ dataField: displayCostValue ? "impact_doll" : "impact", caption: "<% = ParseString("%%Impact%%") %>", allowReordering: true, allowEditing: false, visibleIndex: 3,
            format: {
                type: displayCostValue ? "currency" : "fixedPoint",
                precision: displayCostValue ? 2 : 4
            }
        });

        lblTotals.innerHTML += "<br><small>Number of <%=ParseString("%%alternatives%%")%> that fired: " + fired_events_count + "</small>";
        var dataSource = new DevExpress.data.ArrayStore({
            key: 'id',
            data: ds
        });

        $("#" + idTbl).dxDataGrid({
            allowColumnResizing: true,
            allowColumnReordering: false,
            columns: columns,
            columnResizingMode: "widget",
            columnAutoWidth: true,
            columnFixing: {
                enabled: true
            },
            columnChooser: {                
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: false
            },
            dataSource: dataSource,
            dataStructure: "plain",
            editing: {
                allowUpdating: false
            },
            "export": {
                enabled: true,
            },
            height: "auto",
            highlightChanges: false,
            hoverStateEnabled: true,                
            focusedRowEnabled: false,
            keyExpr: "id",
            loadPanel: {
                enabled: false,
            },
            rowAlternationEnabled: false,
            showColumnLines: true,
            showRowLines: false,
            sorting: {
                mode: "multiple"
            },
            columnAutoWidth: true, 
            pager: {
                allowedPageSizes: [10, 15, 20, 50, 100, 200, 500],
                showInfo: true,
                showNavigationButtons: true,
                showPageSizeSelector: true,
                visible: false
            },
            paging: {
                enabled: false,
                //pageSize: 10
            },
            onRowPrepared: function (e) {
                if (e.rowType === "data") {
                    e.rowElement.addClass(e.data.class);
                    if (e.data.fired) {
                        e.rowElement.css({'font-weight' : 'bold'});
                    }
                }
            },
            //scrolling: {
            //    mode: "virtual"
            //},
            showBorders: true,
            noDataText: "<% = GetEmptyMessage() %>",            
            //onToolbarPreparing: function (e) {
            //    var toolbarItems = e.toolbarOptions.items; 
            //    toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: '<h6 style="padding-top: 10px;">Participant Groups</h6>' });
            //},
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: function () { return "LEC_1_<% = CurrentPageID %>_PRJ_ID_<% = App.ProjectID %>"; }
            }, 
            wordWrapEnabled: false,
            width: "100%"
        });

        lblTotals.innerHTML += "<br/><small>Number of <%=ParseString("%%alternatives%%")%> that occured: " + fired_events_count + "</small>";
        $("#" + idTbl).show();     
        setTimeout(function() { resizeGrid(idTbl, idTbl + "Parent"); }, 100);
    }
    // end Simulation Data

    function inputKeyDown(e, tb) {
        if (e.keyCode == KEYCODE_ENTER) tb.blur();
    }

    function inputChanged(value) {
        $("#lblStepsMaxLabel").text($('#tbSimulations').val()*1);
        runSimulation();
    }
    
    function runSimulation() {
        showLoadingPanel();
        initSimulationStart();
        sendCommand('action=simulate&simulations=' + $('#tbSimulations').val() + '&datapoints=' + $('#tbDatapoints').val() + '&seed=' + $("#tbSeed").val(), false);
    }

    function initSimulationStart() {
        $("#divLECMain").find("input,button,select:not(#btnRun,#btnCancel)").each(function () {
            this.setAttribute("data-dis", this.disabled ? "1" : "0");
            this.disabled = true;
        });
        $("#btnRun").prop("disabled", true);
        $("#btnCancel").prop("disabled", false);
        if ($(".chart_loading_msg").length > 0) $(".chart_loading_msg").html("<%=ResString("lblPleaseWait")%>");
        sim_start = performance.now();        
        
        curView = getCurView();
        viewChange(0);
        
        startProgress();
    }

    function restoreControls() {
        $("#divLECMain").find("input,button,select:not(#btnRun,#btnCancel)").each(function () {
            this.disabled = this.getAttribute("data-dis") == "1";
        });
    }

    function getCurView() {
        //var cbView = document.getElementById("cbView");
        //return cbView.checked ? 1 : 0;
        return $("#cbView").dxSwitch("instance").option("value") ? 1 : 0;
    }

    function simulationModeChange(value) {
        updateToolbar();
        simulationMode = value * 1;
        initSimulationStart();
        sendCommand('action=simulation_mode&value=' + value, true);
    }

    function displayChange(value) {
        displayCostValue = (value*1) == 1;
        initSimulationStart();
        sendCommand('action=display_mode&value=' + value, true);
    }

    function viewChange(value) {
        //var cbView = document.getElementById("cbView")
        //if ((cbView)) cbView.checked = value == 1;
        $("#cbView").dxSwitch("instance").option("value", value == 1);
        //$("#cbView").val(value);
        switch (value) {
            case 0:
                document.getElementById("tdData").style.display = "none";
                document.getElementById("tdChart").style.display = "table";
                //document.getElementById("lblMainTitle").style.display = "";
                document.getElementById("lblDataTitle").style.display = "none";
                $("#trExpectedValues").show();
                //resizeChart();
                break;
            case 1:
                document.getElementById("tdData").style.display = "block";
                document.getElementById("tdChart").style.display = "none";
                //document.getElementById("lblMainTitle").style.display = "none";
                document.getElementById("lblDataTitle").style.display = "";
                $("#trExpectedValues").hide();
                showAllStepsData(cur_steps_data);
                //resizeData();
                break;
        }
        resizePage();
    }

    function onKeepSeed(value) {
        keepSeed = value; 
        sendCommand("action=keep_seed&value=" + value, true);
    }

    //function onShowIntersections(value) {
    //    showIntersections = value;
    //    sendCommand("action=show_intersections&value=" + value, true);
    //}

    function onShowFreqCharts(value) {
        showFreqCharts = value;
        sendCommand("action=show_freq_charts&value=" + value, true);
    }
    
    function onSwitchLogScale(value) {
        logScale = value;
        sendCommand("action=log_scale&value=" + value, false);
    }

    function onForceYAxis0(value) {
        autoPlotYAxisMin = !value;
        localStorage.setItem("LEC_Y_MIN_<% = PRJ.ID %>", autoPlotYAxisMin);
        reloadPage();
        //$(".lec_chart").each(function () {
        //    if ($(this).data("dxChart")) {
        //        var c = $(this).dxChart("instance");
        //        c.option("valueAxis.visualRange.startValue", autoPlotYAxisMin ? undefined : 0);  
        //        c.refresh();
        //    }
        //});
    }

    function onSwitchLine(param, value) {
        if (param === 0) {
            showWithoutControlsLine = value;
            $(".cb_woc").css("opacity", showWithoutControlsLine ? 1 : 0);
            sendCommand("action=show_woc_line&value=" + value, false);
        } else {
            showWithControlsLine = value;
            $(".cb_wc").css("opacity", showWithControlsLine ? 1 : 0);
            sendCommand("action=show_wc_line&value=" + value, false);
        }
        $(".lec_chart").each(function () {
            if ($(this).data("dxChart")) {
                var c = $(this).dxChart("instance");
                c.option("series[" + param + "].visible", param === 0 ? showWithoutControlsLine : showWithControlsLine);  
            }
        });
        updateIntersectionOption(false);
    }

    function onLimitImpactsTo100(value) {
        initSimulationStart();
        sendCommand("action=limit_impacts&value=" + value, true);
    }

    function onUseSourceGroups(value) {
        use_source_groups = value;
        initSimulationStart();
        sendCommand("action=use_source_groups&value=" + value, true);
    }

    function onUseEventGroups(value) {
        use_event_groups = value;
        initSimulationStart();
        sendCommand("action=use_event_groups&value=" + value, true);
    }

    var cur_object_id = "";

    function showGroupEditor(object_id, group_name, group_precedence, is_source_group) {
        var cur_object_id = object_id;
        if (is_source_group) {
            var sContent = "<br><br><table class='text' cellpadding= cellspacing=0 border=0><tr><td align='right'><%=JS_SafeString(ParseString("%%Source%% group name:"))%>&nbsp;</td><td><input type='text' id='tbGroupName' value='" + group_name + "' ></td></tr><tr><td align='right'><%=JS_SafeString(ParseString("%%Source%% precedence:"))%>&nbsp;</td><td><input type='text' id='tbGroupValue' style='text-align:right;' value='" + (group_precedence == <%=Integer.MaxValue%> ? "" : group_precedence) + "' ></td></tr></table>"
            dxDialog(sContent, "onSaveSourceGroup(" + object_id + ", $('#tbGroupName').val(), $('#tbGroupValue').val());", ";", "<% = JS_SafeString(ParseString("Edit %%source%% group")) %>");
        } else {
            var sContent = "<br><br><table class='text' cellpadding= cellspacing=0 border=0><tr><td align='right'><%=JS_SafeString(ParseString("%%Event%% group name:"))%>&nbsp;</td><td><input type='text' id='tbGroupName' value='" + group_name + "' ></td></tr><tr><td align='right'><%=JS_SafeString(ParseString("%%Event%% precedence:"))%>&nbsp;</td><td><input type='text' id='tbGroupValue' style='text-align:right;' value='" + (group_precedence == <%=Integer.MaxValue%> ? "" : group_precedence) + "' ></td></tr></table>"
            dxDialog(sContent, "onSaveEventGroup(" + object_id + ", $('#tbGroupName').val(), $('#tbGroupValue').val());", ";", "<% = JS_SafeString(ParseString("Edit %%event%% group")) %>");
        }
    }

    function newSeed() {
        if (keepSeed) {
            sendCommand('action=new_seed', true);
        }
    }

    function onSetCategory(cat_guid) {
        initSimulationStart();
        sendCommand("action=select_category&cat_guid="+ cat_guid);
    }

    /* Progress Bar */
    var X_ms = 500;
    var progress_inc_in_X_ms = last_exec_time_ms / X_ms; // simulate progress bar

    function startProgress() {
        setProgress(2);
        $("#tdProgressBar").show();

        progress_inc_in_X_ms = Math.round(100 / (last_exec_time_ms / X_ms));
        if (progress_inc_in_X_ms <= 0) progress_inc_in_X_ms = 33;
        setTimeout("setProgress(" + progress_inc_in_X_ms + ");", X_ms);
    }

    function stopProgress() {        
        setProgress(100);
        $("#tdProgressBar").hide("slow");
    }

    function setProgress(value) {
        cur_progress = value;
        if (cur_progress < 0) cur_progress = 0;
        if (cur_progress > 100) cur_progress = 100;
        var bar = document.getElementById("progress_bar");            
        bar.style.width = value + "%";
        if (bar.style.display != "none" && cur_progress < 100) {
            var progress_inc = Math.round(cur_progress + progress_inc_in_X_ms);
            setTimeout("setProgress(" + progress_inc + ");", X_ms);
        }
    }
    /* end Progress Bar */

    /* Hierarchy TreeList */
    function createEventColumn(events_column, isFixed, hasState, isPercent) {
        var clmn = { "caption" : events_column[COL_COLUMN_NAME], "dataField" : events_column[COL_COLUMN_DATA_FIELD], "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowFiltering" : events_column[COL_COLUMN_ALLOW_FILTERING], "allowGrouping" : events_column[COL_COLUMN_ALLOW_GROUPING], "allowEditing" : false, "encodeHtml" : events_column[COL_COLUMN_DATA_FIELD] == "name" }
        if (!hasState) {
            clmn.visible = events_column[COL_COLUMN_VISIBILITY];
        }
        if (events_column[COL_COLUMN_WIDTH] > 0) {
            clmn.width = events_column[COL_COLUMN_WIDTH];
        }
        clmn.fixed = isFixed;
        if (typeof isPercent !== "undefined" && isPercent) {
            clmn.dataType = "number";
            clmn.format = "percent";
            clmn.format = {"type" : "percent", "precision" : decimals};
        }
        return clmn;
    }

    function initTreeList() {
        <% If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS Then %>
        var storageKey = "RiskPlot_TreeList_<% = PRJ.ID %>";
        var hasState = typeof localStorage.getItem(storageKey) !== "undefined" && localStorage.getItem(storageKey) != null;

        //init columns headers                
        var columns = [];
        for (var i = 0; i < hierarchy_columns.length; i++) {
            if (hierarchy_columns[i][COL_COLUMN_ID] == UNDEFINED_INTEGER_VALUE) {
                var clmn = { "caption" : hierarchy_columns[i][COL_COLUMN_NAME], "alignment" : "center", "columns" : [] };
                clmn.columns.push(createEventColumn(hierarchy_columns[i + 1], false, hasState, true)); // l                
                clmn.columns[clmn.columns.length - 1].dataType = "number";
                clmn.columns[clmn.columns.length - 1].alignment = "right";
                clmn.columns.push(createEventColumn(hierarchy_columns[i + 2], false, hasState, true)); // g
                clmn.columns[clmn.columns.length - 1].dataType = "number";
                clmn.columns[clmn.columns.length - 1].alignment = "right";
                i += 2;
                <% If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS Then %>
                clmn.columns.push(createEventColumn(hierarchy_columns[i + 1], false, hasState, true)); // lwc
                clmn.columns[clmn.columns.length - 1].dataType = "number";
                clmn.columns[clmn.columns.length - 1].alignment = "right";
                clmn.columns.push(createEventColumn(hierarchy_columns[i + 2], false, hasState, true)); // gwc
                clmn.columns[clmn.columns.length - 1].dataType = "number";
                clmn.columns[clmn.columns.length - 1].alignment = "right";
                i += 2;
                <% End If %>
                columns.push(clmn);
            } else {                    
                columns.push(createEventColumn(hierarchy_columns[i], i < 2, hasState));
                if (columns[columns.length - 1].dataField == "name") {
                    columns[columns.length - 1].cellTemplate = function(element, info) {            
                        element.append("<span>" + (info.data["is_cat"] ? "<i>" : "") + info.data["name"] + (info.data["is_cat"] ? "</i>" : "") + "</span>");
                    };
                }

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
            onFocusedRowChanging: function (e) {
                if (isLoadingTreeListSessionSettings) e.cancel = true;
            },
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
            selection: {
                mode: "none",
                recursive: false
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
    /* end Hierarchy TreeList */
    
    function initPage() {
        resizeChart();
        if ((theForm.btnRun) && !theForm.btnRun.disabled) theForm.btnRun.focus();
    }

    function initChartsLayout() {
        var chartMain = document.getElementById("divChartMain");
        var chartExtra = document.getElementById("divChartExtra");
        chartMain.style.width = "0%";
        chartExtra.style.width = "0%";
        if (!showFreqCharts) {
            chartMain.style.width = "100%";
            chartExtra.style.width = "0%";
        } else {
            chartMain.style.width = "55%";
            chartExtra.style.width = "45%";
        }        
    }

    function resizeChart() {
        $(".lec_chart").width(10);
        $(".lec_chart").each(function () {
            $(this).width($(this).parent().width() - 10);
            if ($(this).data("dxChart")) {
                var c = $(this).dxChart("instance");
                c.render();  
            }
        });
    }

    function resizeData() {
        $("#divStepDataTable").height(100).width(100);
        var h = document.getElementById("tdData").clientHeight - 50;
        if (h > 100) $("#divStepDataTable").height(h);
        var w = document.getElementById("tdData").clientWidth;
        if (w > 100) $("#divStepDataTable").width(w);
        $(".grids").each(function () {
            resizeGrid(this.id, $(this).parent()[0].id);
        });
    }

    function resizePage() {          
        if (getCurView() == 0) setTimeout('resizeChart();', 25);
        if (getCurView() == 1) setTimeout('resizeData();', 25);        
    }

    $(document).ready(function () {        
        toggleScrollMain();

        initTreeList();
        initPage();
        initChartsLayout();
        updateToolbar();

        $("#cbView").dxSwitch({
            value: curView == 1,
            onValueChanged: function(e) {
                var newView = e.value ? 1 : 0;
                if (curView !== newView) {
                    curView = newView; 
                    viewChange(curView);
                }
            }
        });
        $("#divFilterWarning").html(events_filter_name);
        if (events_filter_name != "") { $("#divFilterWarning").show(); } else { $("#divFilterWarning").hide(); }

        $("#cbForceYAxis0").prop("checked", !autoPlotYAxisMin);

        //$("#divExpectedValue").dxResizable({
        //    handles: "top",
        //    onResize: function (e) {
        //        resizeChart();
        //    }
        //});

        if (<% = Bool2JS(PRJ.isMyRiskRewardModel OrElse Not CanUseControls) %>) {
            $("#divStepSources").dxResizable({
                handles: "right",
                onResize: function (e) {
                    $("#divStepSources").width(e.width);
                    $("#divStepEvents").width($("#divStepDataTable").width() - e.width);
                    resizeData();
                }
            });
        }


        runSimulation();        
    });

    function updateToolbar() {
        var cb = document.getElementById("cbSimulationMode");
        if ((cb)) {
            cb.style.backgroundColor = cb.options[cb.selectedIndex].style.backgroundColor;
        }
        
        var opacity = keepSeed ? 1 : 0.5;
        $("#btnNewSeed").fadeTo(opacity).css({"cursor" : keepSeed ? "pointer" : "default", "opacity" : opacity});
        $("#lblSeed").css("color", keepSeed ? "black" : "#909090;");
        $("#tbSeed").prop("disabled", !keepSeed);

        $("#cbAllowEventFireMultipleTimes").prop("disabled", simulationMode == _SIMULATE_EVENTS);
        if (simulationMode == _SIMULATE_EVENTS) {
            $("#divAllowEventFireMultipleTimes").hide();
            $("#btnPercentageSources").hide();
        } else {
            //if (showHiddenSettings) {
            //    $("#divAllowEventFireMultipleTimes").show();
            //} else {
            //    $("#divAllowEventFireMultipleTimes").hide();
            //}
            $("#btnPercentageSources").show();
        }

        //if (showHiddenSettings) {
        //    $("#divLimitImpacts100").show();
        //    $("#divAllowEventFireMultipleTimes").show();
        //} else {
        //    $("#divLimitImpacts100").hide();
        //    $("#divAllowEventFireMultipleTimes").hide();
        //}
    }

    function hotkeys(event) {
        if (!document.getElementById) return;
        if ($("#tbSimulations").is(':focus') || $("#tbDatapoints").is(':focus') || $("#tbSeed").is(':focus')) return;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            switch (code) {
                case KEYCODE_LEFT:
                case KEYCODE_RIGHT:
                    // switch between Chart and Data views on left and right arrow keys pressed
                    
                    //var cbView = document.getElementById("cbView")
                    curView = getCurView() == 0 ? 1 : 0;
                    viewChange(curView);
                    break;
                case KEYCODE_DOWN:
                case KEYCODE_UP:
                    // switch between Data steps on up and down arrow keys pressed
                    curView = getCurView();
                    if (curView == 1) {
                        // prevent default handling (scrolling)
                        if (event.preventDefault) event.preventDefault();
                        if (event.stopPropagation) event.stopPropagation();
                        
                        // go to next/prev step
                        var tbSteps = document.getElementById("tbSteps");
                        var numSimulations = $('#tbSimulations').val() * 1;
                        if ((tbSteps) && !tbSteps.disabled && numSimulations > 0) {
                            var curStep = tbSteps.value * 1;
                            curStep += code == KEYCODE_UP ? -1 : 1;
                            if (curStep >= 1 && curStep <= numSimulations && curStep <= <%= PM.RiskSimulations.NumberOfStepsToStore%>) {
                                tbSteps.value = curStep;
                                getStepData(curStep);
                            }
                        }
                    }
                    break;
                case KEYCODE_PERIOD:
                    if (event.ctrlKey) {
                        onToolbarDblClick();
                    }
                    break;

            }
        }
    }

    document.onkeydown = hotkeys;
    resize_custom  = resizePage;

</script>

<%-- Page content --%>
<div id="divLECMain" class="whole">
<table class="whole" border='0' cellspacing='0' cellpadding='0'>
    <tr id="trToolbar" valign="top">
        <td colspan="3" valign="top">
            <div>
                 <div style="white-space: nowrap; display: inline-block;">
                     <button id="btnRun" onclick="runSimulation(); return false;" type='button' class='button' style="width:75px; height:24px; line-height:17px;" disabled="disabled"><i class="fas fa-play"></i>&nbsp;<% = ResString("btnRun") %></button>
                     <button id="btnCancel" onclick="this.disabled = true; cancelCommand(); return false;" type='button' class='button' style="width:75px; height:24px; line-height:17px;" disabled="disabled"><i class="fas fa-ban"></i><% = ResString("btnCancel") %></button>
                 </div>
                 <div style="white-space: nowrap; display: inline-block;">
                     <nobr><span>Trials:</span>
                     <input type="number" id="tbSimulations" class="input number" step='1' min="0" value="<%=PM.RiskSimulations.NumberOfTrials%>" style="width:70px; height:20px; margin-top:2px; text-align:right;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown="inputKeyDown(event, this);" onchange="inputChanged(this.value);" >&nbsp;</nobr>
                     <nobr><span>Datapoints:</span>
                     <input type="number" id="tbDatapoints" class="input number" step='1' min="2" value="<%=NumDatapoints%>" style="width:40px; height:20px; margin-top:2px; text-align:right;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown="inputKeyDown(event, this);" onchange="inputChanged(this.value);">&nbsp;</nobr>
                 </div>
                 <div style="white-space: nowrap; display: inline-block;">
                     <nobr><span id="lblSeed">Seed:</span>
                     <input type="number" id="tbSeed" class="input number" step='1' min="1" max="<%=Integer.MaxValue%>" value="<% = PM.RiskSimulations.RandomSeed %>" style="width:40px; height:20px; margin-top:2px; text-align:right;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown="inputKeyDown(event, this);" onchange="inputChanged(this.value);" >
                     <i class="fas fa-sync-alt ec-icon" id="btnNewSeed" style="vertical-align:middle; padding-bottom:1px; cursor:pointer;" title="New Seed" onclick="newSeed();"></i>
                     </nobr>
                     <nobr><label title="<%=ResString("lblKeepRandSeed")%>"><input type="checkbox" id="cbKeepSeed" class="checkbox" <% = If(PM.RiskSimulations.KeepRandomSeed, " checked='checked' ", "") %> onclick="onKeepSeed(this.checked);" ><%=ResString("lblKeepRandSeed")%></label></nobr>
                 </div>
                 <div style="white-space: nowrap; display: inline-block;">
                     <%If PM.CalculationsManager.DollarValue <> UNDEFINED_INTEGER_VALUE Then%>
                     <nobr><span>Display:</span>                 
                     <select id='cbDisplay' onchange="displayChange(this.value);">
                         <option value="1" <%=IIf(ShowDollarValue, "selected='selected'", "")%>><%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "Monetary Gain", "Monetary %%Loss%%"))%></option>
                         <option value="0" <%=IIf(Not ShowDollarValue, "selected='selected'", "")%>><%=ParseString(If(PRJ.RiskionProjectType = ProjectType.ptOpportunities OrElse PRJ.RiskionProjectType = ProjectType.ptMixed, "Percent Gain", "Percent %%Loss%%"))%></option>
                     </select></nobr>                        
                     <%End If%>                            
                     <div style="display: inline-block; background-color: #ffa792; padding: 1px 5px; border: 1px solid #ccc; line-height: 0;"><nobr><label title="<%=ResString("lblLECUseSourceGroups")%>"><input type="checkbox" id="cbUseSourceGroups" class="checkbox" <% = IIf(PM.RiskSimulations.UseSourceGroups, " checked='checked' ", "") %> onclick="onUseSourceGroups(this.checked);" /><%=ResString("lblLECUseSourceGroups")%></label></nobr></div>
                     <div style="display: inline-block; background-color: #ffa792; padding: 1px 5px; border: 1px solid #ccc; line-height: 0;"><nobr><label title="<%=ResString("lblLECUseEventGroups")%>"><input type="checkbox" id="cbUseEventGroups" class="checkbox" <% = If(PM.RiskSimulations.UseEventsGroups, " checked='checked' ", "") %> onclick="onUseEventGroups(this.checked);" /><%=ResString("lblLECUseEventGroups")%></label></nobr></div>
                 </div>
                 <div style="white-space: nowrap; display: inline-block;">
                     <%--<nobr><label title="Show Intersections"><input type="checkbox" id="cbShowIntersections" class="checkbox" <%=If(PM.Parameters.LEC_ShowIntersections, " checked='checked' ", "") %> onclick="onShowIntersections(this.checked);" />Show Intersections</label></nobr>--%>
                     <nobr><label title="Show Frequency Charts"><input type="checkbox" id="cbShowFreqCharts" class="checkbox" <%=If(PM.Parameters.LEC_ShowFrequencyCharts, " checked='checked' ", "") %> onclick="onShowFreqCharts(this.checked);" />Show Frequency Charts</label></nobr>
                 </div>
                 <div style="white-space: nowrap; display: inline-block;">
                     <nobr><label title="Logarithmic scale for X-axis"><input type="checkbox" id="cbLogXScale" class="checkbox" <% = If(PM.Parameters.LEC_LogarithmicScale, " checked='checked' ", "") %> onclick="onSwitchLogScale(this.checked);" />Logarithmic Scale</label></nobr>
                     <nobr><label><input type="checkbox" id="cbForceYAxis0" class="checkbox" onclick="onForceYAxis0(this.checked);" />Force 0 for Y-axis</label></nobr>
                 </div>                                        
                 <div style="white-space: nowrap; display: none;">
                     <button id="btnClose" onclick="window.close(); return false;" class='button' style="width:95px; height:24px; line-height:17px;"><i class="fas fa-times" style="vertical-align:middle; padding-bottom:1px;"></i><% =ResString("btnClose")%></button> 
                 </div>
                 <div style="display:inline-block; margin: 8px; text-align: center; vertical-align: middle;">
                     <span>Trials: </span><div style="display: inline-block;" id="cbView"></div>
                 </div>
            </div>
        </td>
    </tr>
    <tr id="trProgressBar" valign="top" style="height:4px;">
        <td id="tdProgressBar" colspan="3" style="display:none; vertical-align:top;" valign="top">
            <div id="progress_control" style="overflow:hidden; background-color:#d0d0d0; height:4px; width:100%; text-align:left;">
                <div id="progress_bar" style="background-color:#8899cc; height:4px; width:100%; margin:0px;"></div>
            </div>
        </td>
    </tr>
    <tr id="trExtraInfo" valign="top">
        <td colspan="3" valign="top">
            <div class="whole" style="display:table; border-collapse:collapse;">
                <div style="display:table-row;">
            		<div style="display:table-cell; text-align: center;">
                        <div id="divFilterWarning" class="note" style="display:none;"></div>                        
                    </div>
                </div>
            </div>                       
        </td>
    </tr>
    <tr id="trTitle">
        <td colspan="3" valign="top">
            <h5 id="lblMainTitle" style="margin: 8px 0px 0px 0px; display: none;">
                <% = GetTitle()%>
            </h5>
            <h5 id="lblDataTitle" style="margin: 8px 0px 0px 0px;">
                <% = GetTitle(True)%>
            </h5>
        </td>
    </tr>        
    <tr id='trContent' class='whole' valign='top'>
        <% If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_TO_OBJS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_FROM_SOURCES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_EXTRA_PLOT_WITH_CONTROLS_TO_OBJS Then %>
        <td id="tdHierarchy" style="height: 100%;">
            <div id="divHierarchy" style="background-color: white;"></div>
        </td>
        <td colspan="2" class="whole" align="center" valign="middle">
        <% Else %>
        <td colspan="3" class="whole" align="center" valign="middle">
        <%End If%>
            <div id="tdChart" class="whole" style="display:table; border-collapse:collapse; margin-bottom: -40px; margin-top: -40px;">
            	<div class="whole" style="display: table-row;">
            		<div id="divChartMain" style="width: 70%; height: 100%; display:table-cell;">
                        <div id="divChart" class='whole lec_chart'></div>
            		</div>
            		<div id="divChartExtra" style="width: 30%; height: 100%; display: table-cell; vertical-align: top;">
            			<div class="whole" style="display: table; border-collapse: collapse;">
                            <div style="height: 50%; display:table-row;">
            		        	<div id="divChart1Container" style="display: table-cell; padding-bottom:5px;">
                                    <div id="divChart1" class='whole lec_chart'></div>
            		        	</div>
            		        </div>
                            <div style="height: 50%; display:table-row;">
            		        	<div id="divChart2Container" style="display:table-cell; padding-top:5px;">
                                    <div id="divChart2" class='whole lec_chart'></div>
            		        	</div>
            		        </div>
            			</div>
            		</div>
            	</div>
            </div>    
        
            <div id="tdData" colspan="3" class="whole" style="display: none; text-align: center;">
                <nobr>
                    <span>Step:&nbsp;[1..<span id="lblStepsMaxLabel"><% = PM.RiskSimulations.NumberOfTrials %></span>]</span>
                    <%--<select id='cbSteps' dir="rtl" style="display:none;" onchange="getStepData(this.value);">
                    </select>--%>
                    <input type="number" id="tbSteps" class="input number" step='1' min="1" max="<% = PM.RiskSimulations.NumberOfStepsToStore%>" value="1" style="width:40px; height:20px; margin-top:2px; text-align:right;" onchange="if (validFloat(this.value)) getStepData(this.value*1); else return false;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'/>
                    <button id="btnBack" onclick="viewChange(0); return false;" class='button' style="width: 120px; height: 26px; margin-left: 20px; margin-bottom: 4px;">Back to Chart</button>
                    <br/>
                    <nobr>
                        <span>Show occured only:</span>&nbsp;
                        <label><input id='cbFiredOnlySources' type='checkbox' style='vertical-align: middle;' onclick='onShowFiredOnlySources(this.checked);' /><% = ParseString("%%Objectives(l)%%") %></label>&nbsp;
                        <label><input id='cbFiredOnlyEvents' type='checkbox' style='vertical-align: middle;' onclick='onShowFiredOnlyEvents(this.checked);' /><% = ParseString("%%Alternatives%%") %></label>&nbsp;
                    </nobr>
                </nobr><br/>
                <div id="divStepDataTable" style="text-align: center; height: 300px; width: 100%; display: inline-block; white-space: nowrap;">
                    <div id="divStepSources" class="divWithoutControls" style="width: 40%; height: 100%; display: inline-block; vertical-align: top;">
                        <p id="lblTotalsSources" style="text-align: center; margin-bottom: -25px;"></p>
                        <div id="tblDataSourcesParent" class="whole">
                            <div id="tblDataSources" class="grids" style="width: 100%;"></div>
                        </div>
                    </div>
                    <div id="divStepSourcesWC" class="divWithControls" style="height: 100%; display: inline-block; vertical-align: top;">
                        <p id="lblTotalsSourcesWC" style="text-align: center; margin-bottom: -25px;"></p>
                        <div id="tblDataSourcesWCParent" class="whole">
                            <div id="tblDataSourcesWC" class="grids" style="width: 100%;"></div>
                        </div>
                    </div>
                    <div id="divStepEvents" class="divWithoutControls" style="width: 60%; height: 100%; display: inline-block; vertical-align: top;">
                        <p id="lblTotalsEvents" style="text-align: center; margin-bottom: -25px;"></p>
                        <div id="tblDataEventsParent" class="whole">
                            <div id="tblDataEvents" class="grids" style="width: 100%;"></div>
                        </div>
                    </div>
                    <div id="divStepEventsWC" class="divWithControls" style="height: 100%; display: inline-block; vertical-align: top;">
                        <p id="lblTotalsEventsWC" style="text-align: center; margin-bottom: -25px;"></p>
                        <div id="tblDataEventsWCParent" class="whole">
                            <div id="tblDataEventsWC" class="grids" style="width: 100%;"></div>
                        </div>
                    </div>                   
                </div>
            </div>    
        </td>
    </tr>
    <tr id="trExpectedValues">
        <td colspan="3" valign="top">
            <div id="divExpectedValue" style="margin-top:3px; width: 100%; height: <% = If(CanUseControls, "195px", "165px") %>; background-color: white;">
                <div id="tblExpectedValue" class="whole grids"></div>
            </div>
        </td>
    </tr>
    <tr>
        <td colspan="2" align="left" style="padding:2px 2px 0px 100px;">
            <span class="note text small" style="text-align: left; color:#777; display:inline-block;">
                Hint: use Left and Right arrow keys for switching the Chart/Data view and Up/Down keys for looping the steps when in Data view
            </span>
        </td>
        <td align="right" style="padding:2px 12px 0px 10px;">
            <%If ShowDraftPages() Then %>
            <span id="txtTime" class="text small" style="text-align: right; color:#b0b0b0;"></span>
            <%End If%>
        </td>
    </tr>
</table>
</div>
</asp:Content>

