<%@ Page Language="VB" Inherits="RiskRegisterPage" title="Risk Register" Codebehind="RiskRegister.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script language="javascript" type="text/javascript" src="/Scripts/jszip.min.js"></script>
    <script language="javascript" type="text/javascript" src="/scripts/datatables_only.min.js"></script>
    <script language="javascript" type="text/javascript" src="/scripts/datatables.extra.js"></script>
    <script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/html2canvas.min.js"></script>
    <script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/download.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
    .dx-datagrid .dx-row > td {
        padding: 1px 5px;
    }
    /*.dx-datagrid-table :not(.dx-selection):not(.dx-row-focused):not(.dx-edit-row):not(.dx-row-removed):not(.dx-row-inserted).dx-state-hover.dx-data-row > td:not(.dx-focused) {
        background-color: #b0b0b0;
        color: #222;
    }*/
    .dx-fieldset-header {
        border-bottom: 1px solid #cfcfcf;
    }

</style>
<script language="javascript" type="text/javascript">
    
    var isReadOnly = <%=Bool2JS(App.IsActiveProjectStructureReadOnly OrElse PRJ.ProjectStatus = ecProjectStatus.psMasterProject)%>;
    var isWidget = <% = Bool2JS(IsWidget) %>;
    
    var columns_ver = "210208";

    var rvRiskRegister      = <%=CInt(RegisterViews.rvRiskRegister)%>;
    var rvRiskRegisterWithControls = <%=CInt(RegisterViews.rvRiskRegisterWithControls)%>;
    var rvTreatmentRegister = <%=CInt(RegisterViews.rvTreatmentRegister)%>;
    var rvAcceptanceRegister= <%=CInt(RegisterViews.rvAcceptanceRegister)%>;

    var RegisterView   = <%=CInt(RegisterView)%>;
    var DecimalDigits  = <%=PM.Parameters.DecimalDigits%>;

    var showDollarValue = <%=Bool2JS(ShowDollarValue)%>;
    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    var showCents = <% = Bool2JS(ShowCents) %>;
    var showDescriptions = <%=Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs)%>;
    var showTimestamp = <%=Bool2JS(PM.Parameters.Riskion_Register_Timestamp)%>;
    var showTotalRisk = <% = Bool2JS(ShowTotalRisk) %>;
    var selectedHierarchyNodeID = "<% = SelectedHierarchyNodeID.ToString %>";
    var probability_calculation = <% = CInt(PM.CalculationsManager.LikelihoodsCalculationMode) %>;
    var dilutedMode = <% = CInt(PM.CalculationsManager.ConsequenceSimulationsMode) %>;
    var wrt_calculation = <% = CInt(If(PM.CalculationsManager.ShowDueToPriorities, 1, 0)) %>;
    var isHierarchyMultiSelectMode = false;
    var gridPrintStyle = false;

    var hasInit = false;
    var table_treatments   = null;
    var table_acceptance   = null;

    var coveringSources = <% = GetCoveringSources() %>;
    var all_obj_data = <% = GetAllObjsData() %>;

    var DollarValue = <%=JS_SafeNumber(DollarValue)%>; // Value of Enterprise
    var DollarValueTarget = "<%=DollarValueTarget%>"; // Target Of Value of Enterprise
    var DollarValueOfEnterprise = <%=JS_SafeNumber(DollarValueOfEnterprise)%>;
    var DollarValueDefined = <%=Bool2JS(Not (DollarValue = UNDEFINED_INTEGER_VALUE OrElse DollarValueTarget = UNDEFINED_STRING_VALUE))%>;

    var events_data     = <% = GetEventsData() %>[0];
    var event_list = events_data;
    var events_columns  = <% = GetEventsColumns() %>;
    var treatments_data = <% = GetControlsData() %>;
    var treatments_columns = <% = GetTreatmentsColumns() %>;
    var acceptance_data = <% = GetAcceptanceData() %>;
    var acceptance_columns = <% = GetAcceptanceColumns() %>;
    var hierarchy_data = <% = GetHierarchyData() %>;
    var hierarchy_columns = <% = GetHierarchyColumns() %>;

    var NormMaxValues = <% = NormMaxValues %>    

    var numSimulations = <% = PM.RiskSimulations.NumberOfTrials %>;
    var randSeed       = <% = PM.RiskSimulations.RandomSeed %>;
    var keepSeed       = <% = Bool2JS(PM.RiskSimulations.KeepRandomSeed) %>;
    var use_event_groups = <% = Bool2JS(PM.RiskSimulations.UseEventsGroups) %>;
    var use_source_groups = <% = Bool2JS(PM.RiskSimulations.UseSourceGroups) %>;

    var UNDEFINED_INTEGER_VALUE = <% = UNDEFINED_INTEGER_VALUE %>;

    var img_path = '<% =ImagePath %>';
    var img_qh = new Image; img_qh.src = img_path + 'help/icon-question.png';
    var img_qh_ = new Image; img_qh_.src = img_path + 'help/icon-question_dis.png';
    
    var cur_order = [3, "desc"];

    var COL_CTRL_ID = 'id';
    var COL_CTRL_NAME = 'name';
    var COL_CTRL_INFODOC = 'info';
    var COL_CTRL_IS_ACTIVE = 'selected';
    var COL_CTRL_FOR = 'type';
    var COL_CTRL_COST = 'cost';
    var COL_CTRL_APPL_COUNT = 'apps_count';
    var COL_CTRL_APPLICATIONS = 'apps';
    var COL_CTLR_EFFECTIVENESS = 'value';

    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_CLASS = 3;
    var COL_COLUMN_DATA_FIELD = 4;
    var COL_COLUMN_WIDTH = 5;
    var COL_COLUMN_ALLOW_FILTERING = 6;
    var COL_COLUMN_ALLOW_GROUPING = 7;

    var BAR_DEFAULT_COLOR;
    var BAR_LIKELIHOOD_COLOR;
    var BAR_IMPACT_COLOR;
    var BAR_RISK_COLOR;
    var BAR_OPPORTUNITY_COLOR;

    var lblNA = "<%=ResString("lblNA")%>";

    // actions
    var ACTION_DECIMALS          = "<%=ACTION_DECIMALS%>";  
    var ACTION_CREATE_DEFAULT_ATTRIBUTE = "<%=ACTION_CREATE_DEFAULT_ATTRIBUTE%>";

    /* jQuery Ajax */
    function syncReceived(params) {
        if ((params)) {
            var rd = eval(params);
            if ((rd) && rd.length > 0) {
                if (rd[0] == ACTION_DECIMALS) {
                    switch (RegisterView) {
                        case rvRiskRegister:
                        case rvRiskRegisterWithControls:
                            events_data = rd[1][0];
                            NormMaxValues = rd[1][1];
                            refreshDataGrid("tableContent", events_data);
                            break;
                        case rvTreatmentRegister:
                            treatments_data = rd[1];
                            refreshDataGrid("tableContent", treatments_data);
                            break;
                    }
                    //initDataGrid();
                }
                if (rd[0] == ACTION_CREATE_DEFAULT_ATTRIBUTE) {
                    // reload project in Shell

                }
                if (rd[0] == "timestamp") {
                    $("#lblTimestamp").html(" " + rd[1]);
                }
                if (rd[0] == "show_sim_results" || rd[0] == "probability_calculation_mode" || rd[0] == "diluted_mode" || rd[0] == "wrt_calculation_mode" || rd[0] == "select_events") {
                    events_data = rd[1][0];
                    NormMaxValues = rd[1][1];
                    initDataGrid();
                }
                if (rd[0] == "show_cents") {
                    events_data = rd[1][0];
                    NormMaxValues = rd[1][1];
                    initDataGrid();
                }
                if (rd[0] == "get_optimal_unmber_of_trials") {
                    numSimulations = rd[1] * 1;
                    $("#tbNumSimulations").dxNumberBox("instance").option("value", numSimulations)
                    $("#tbNumSimulations").effect("highlight", {}, 900);
                }
                if (rd[0] == "event_id_mode") {
                    events_data = rd[1][0];
                    events_columns = rd[2];
                    NormMaxValues = rd[1][1];
                    initDataGrid();
                }
                //if (rd[0] == "show_dollar_value" || rd[0] == "show_total_risk") { }
                if (rd[0] == "selected_users") {
                    events_data = rd[1][0];
                    NormMaxValues = rd[1][1];
                    events_columns = rd[2];
                    hierarchy_data = rd[3];
                    hierarchy_columns = rd[4];
                    initDataGrid();
                    initTreeList();
                }
                if (rd[0] == "selected_node") {
                    events_data = rd[1][0];
                    NormMaxValues = rd[1][1];
                    refreshDataGrid("tableContent", events_data);
                }
                if (rd[0] == "selected_multiple_nodes") {
                    events_data = rd[1][0];
                    NormMaxValues = rd[1][1];
                    events_columns = rd[2];
                    initDataGrid();
                }
                if (rd[0] == "set_dollar_value") {                    
                    all_obj_data = rd[1];
                    DollarValue = rd[2];
                    DollarValueTarget = rd[3];
                    DollarValueOfEnterprise = rd[4];
                    DollarValueDefined = rd[5];
                    initDataGrid();
                    //todo initTreeList();
                }
            }
        }        

        updateToolbar();
        hideLoadingPanel();
    }
    
    function syncError() {
        hideLoadingPanel();
        dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error", 350, 280);
    }

    function sendCommand(params, showPleaseWait) {
        if (showPleaseWait) showLoadingPanel();

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function refreshDataGrid(grid_id, ds) {
        showLoadingPanel();
        if ($("#" + grid_id).data("dxDataGrid")) {
            var dg = $("#" + grid_id).dxDataGrid("instance");
            dg.option("dataSource", ds.filter(function(item) { return item.enabled; }));
            dg.refresh();
        }        
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(grid_id, parent_id) {
        var margin = 4;
        $("#" + grid_id).height(0).width(0);
        var td = $("#" + parent_id);
        var w = $("#" + grid_id).width(Math.round(td.innerWidth()) - 3*margin).width();
        var h = $("#" + grid_id).height(Math.round(td.innerHeight() > 0 ? td.innerHeight() - 10 : td.parent().innerHeight() - 10)).height();
        if ((grid_w_old!=w || grid_h_old!=h)) {
            grid_w_old = w;
            grid_h_old = h;
        };
    }

    function checkResize(w_o, h_o) {
        var w = $(window).width();
        var h = $(window).height();
        if (!w || !h || !w_o || !h_o || (w==w_o && h==h_o)) {
            $("#tableContent").height(0).width(0);
            $("#divHierarchy").height(0).width(0);
            resizeGrid("tableContent", "divContent");
            resizeGrid("divHierarchy", "divSplitter");
        }
    }

    var splitters_init = false;

    function resizePage(force_redraw) {
        if (!splitters_init || force_redraw) {
            splitters_init = initSplitter();        
        }

        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        checkResize(force_redraw ? 0 : w, force_redraw ? 0 : h);
    }

    function createEventColumn(events_column, isFixed, hasState, isPercent) {
        var clmn = { "caption" : events_column[COL_COLUMN_NAME], "dataField" : events_column[COL_COLUMN_DATA_FIELD], "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowFiltering" : events_column[COL_COLUMN_ALLOW_FILTERING], "allowGrouping" : events_column[COL_COLUMN_ALLOW_GROUPING], "allowEditing" : false, "encodeHtml" : events_column[COL_COLUMN_DATA_FIELD] == "name" }
        if (events_column[COL_COLUMN_DATA_FIELD] == "info") clmn.type = "html";
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
            clmn.format = {"type" : "percent", "precision" : 2};
        }
        if (events_column[COL_COLUMN_DATA_FIELD] == "color") {
            clmn.type = "html";
            clmn.alignment = "center";
            clmn.cellTemplate = function(element, info) {            
                var $img = $('<img src="<% = ImagePath %>legend-10.png" width="10" height="10" border="0" style="background:' + info.value + '">');
                element.append($img);
            }
        }
        if (clmn.dataField == "event_type") {
            <% If PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then %>
            clmn.groupIndex = 1;
            <% End If %>
            //clmn.showEditorAlways = true;
            //clmn.showWhenGrouped = true;
            clmn.customizeText = function(cellInfo) {
                return cellInfo.value == 0 ? "<% = ParseString("%%Risk%%") %>" : "<% = ParseString("%%Opportunity%%") %>";
            };
            clmn.allowEditing = !isReadOnly;
            clmn.editCellTemplate = function (cellElement, cellInfo) {
                    $("<div></div>").dxSelectBox({
                        displayExpr: "text",
                        valueExpr: "key",
                        items: [{"key" : 0, "text" : "<% = ParseString("%%Risk%%") %>"}, {"key" : 1, "text" : "<% = ParseString("%%Opportunity%%") %>"}],
                        value: cellInfo.value * 1,
                        onValueChanged: function(e) {
                            if (cellInfo.data[cellInfo.column.dataField] != e.value) {
                                cellInfo.component.option("value", e.value);
                                cellInfo.data[cellInfo.column.dataField] = e.value;
                                cellInfo.setValue(e.value);
                            }
                        }
                    }).appendTo(cellElement);                    
                };
        }
        if (clmn.dataField == "likelihood_contributions") {
            <% If PRJ.isMyRiskRewardModel Then %>
            clmn.groupIndex = 0;
            <% End If %>
            clmn.customizeText = function(cellInfo) {
                var retVal = "";
                if (typeof cellInfo.value === "object") cellInfo.value.forEach(function(item) {
                    retVal += (retVal == "" ? "" : ", ") + coveringSources[item].name;
                });
                return retVal;
            };
        }
        return clmn;
    }

    function getPriorityCellTemplate(bar_color, maxValue, canShowCost, isRiskCell) {
        return function(element, info) {            
            var sVal = lblNA;
            if (info.value + "" != lblNA) {
                var dVal = info.value * 100;
                var fVal = canShowCost && showDollarValue ? showCost(info.value * DollarValueOfEnterprise, true, !showCents) : jsDouble2Fixed(dVal, DecimalDigits, true);
                sVal = htmlGraphBarWithValue(dVal, NormMaxValues[maxValue] * 100, fVal, "<% = ImagePath %>", 75, 10, typeof isRiskCell !== "undefined" && isRiskCell && <% = Bool2JS(PRJ.isMixedModel OrElse PRJ.isOpportunityModel OrElse PRJ.isMyRiskRewardModel) %> ? (info.data.event_type != <% = CInt(EventType.Risk) %> ? BAR_OPPORTUNITY_COLOR : bar_color) : bar_color);
            }            
            element.append(sVal);
        }
    }

    var sortStorageKey = "RiskReg_Sort_<% = PRJ.ID %>";
    var sortByGroupSummaryInfo = typeof localStorage.getItem(sortStorageKey) !== "undefined" && localStorage.getItem(sortStorageKey) !== null ? JSON.parse(localStorage.getItem(sortStorageKey)) : undefined;

    function initDataGrid() {
        var tbl = (($("#tableContent").data("dxDataGrid"))) ? $("#tableContent").dxDataGrid("instance") : null;
        if (tbl !== null) { tbl.dispose(); }

        var storageKey = "RReg_DG_<%=PRJ.ID%>_<% = Bool2JS(IsWidget) %>_" + columns_ver;
        var hasState = typeof localStorage.getItem(storageKey) !== "undefined" && localStorage.getItem(storageKey) != null;

        switch (RegisterView) {
            case rvRiskRegister:
            case rvRiskRegisterWithControls:
                var summaryOptions = { groupItems : [], totalItems: [] };
                var likelihoodFormat = { type: "percent", precision: DecimalDigits };
                var impactFormat = showDollarValue ? function (value) { return showCost(value * DollarValueOfEnterprise, true, !showCents); } : { type: "percent", precision: DecimalDigits };
                //init columns headers                
                var columns = [];
                for (var i = 0; i < events_columns.length; i++) {
                    if (events_columns[i][COL_COLUMN_ID] == UNDEFINED_INTEGER_VALUE) {
                        var clmn = { "caption" : events_columns[i][COL_COLUMN_NAME], "alignment" : "center", "columns" : [] };
                        var childColumns;
                        <% If PagesWithControlsList.Contains(CurrentPageID) Then %>
                        var woClmn = { "caption" : "<% = ParseString("W.O. %%Controls%%") %>", "alignment" : "center", "columns" : [] };
                        var wcClmn = { "caption" : "<% = ParseString("With %%Controls%%") %>", "alignment" : "center", "columns" : [] };
                        clmn.columns.push(woClmn);
                        clmn.columns.push(wcClmn);
                        childColumns = woClmn.columns;
                        <% Else %>
                        childColumns = clmn.columns;
                        <% End If %>
                        childColumns.push(createEventColumn(events_columns[i + 1], false, hasState, true)); // l
                        childColumns[childColumns.length - 1].cellTemplate = getPriorityCellTemplate(BAR_LIKELIHOOD_COLOR, "maxL", false, false);
                        //summaryOptions.groupItems.push({ column: events_columns[i + 1][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: likelihoodFormat, showInGroupFooter: true, alignByColumn: true, cssClass : "align-right"});
                        childColumns.push(createEventColumn(events_columns[i + 2], false, hasState, true)); // i
                        childColumns[childColumns.length - 1].cellTemplate = getPriorityCellTemplate(BAR_IMPACT_COLOR, "maxI", true, false);
                        if (showDollarValue) childColumns[childColumns.length - 1].customizeText = function (info) { return typeof info.value !== "undefined" ? showCost(info.value * DollarValueOfEnterprise, true, !showCents) : info.valueText; };
                        //summaryOptions.groupItems.push({ column: events_columns[i + 2][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: impactFormat, showInGroupFooter: true, alignByColumn: true, cssClass : "align-right"});
                        childColumns.push(createEventColumn(events_columns[i + 3], false, hasState, true)); // r
                        childColumns[childColumns.length - 1].isRiskColumn = true;
                        childColumns[childColumns.length - 1].cellTemplate = getPriorityCellTemplate(<% If PRJ.isOpportunityModel Then %>BAR_OPPORTUNITY_COLOR<% Else %>BAR_RISK_COLOR<% End If %>, "maxR", true, true);
                        if (showDollarValue) childColumns[childColumns.length - 1].customizeText = function (info) { return typeof info.value !== "undefined" ? showCost(info.value * DollarValueOfEnterprise, true, !showCents) : info.valueText; };
                        summaryOptions.groupItems.push({ column: events_columns[i + 3][COL_COLUMN_DATA_FIELD], name: events_columns[i + 3][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: impactFormat, showInGroupFooter: false, alignByColumn: true, cssClass : "align-right"});
                        if (showTotalRisk && dilutedMode == 0) summaryOptions.totalItems.push({ column: events_columns[i + 3][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: impactFormat, cssClass : "align-right" }); // Grid Total

                        <% If PagesWithControlsList.Contains(CurrentPageID) Then %>
                        childColumns = wcClmn.columns;
                        childColumns.push(createEventColumn(events_columns[i + 4], false, hasState, true)); // lwc
                        childColumns[childColumns.length - 1].cellTemplate = getPriorityCellTemplate(BAR_LIKELIHOOD_COLOR, "maxL", false, false);
                        //summaryOptions.groupItems.push({ column: events_columns[i + 4][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: likelihoodFormat, showInGroupFooter: true, alignByColumn: true, cssClass : "align-right"});
                        childColumns.push(createEventColumn(events_columns[i + 5], false, hasState, true)); // iwc
                        childColumns[childColumns.length - 1].cellTemplate = getPriorityCellTemplate(BAR_IMPACT_COLOR, "maxI", true, false);
                        //summaryOptions.groupItems.push({ column: events_columns[i + 5][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: impactFormat, showInGroupFooter: true, alignByColumn: true, cssClass : "align-right"});
                        childColumns.push(createEventColumn(events_columns[i + 6], false, hasState, true)); // rwc
                        childColumns[childColumns.length - 1].isRiskColumn = true;
                        childColumns[childColumns.length - 1].cellTemplate = getPriorityCellTemplate(<% If PRJ.isOpportunityModel Then %>BAR_OPPORTUNITY_COLOR<% Else %>BAR_RISK_COLOR<% End If %>, "maxR", true, true);
                        summaryOptions.groupItems.push({ column: events_columns[i + 6][COL_COLUMN_DATA_FIELD], name: events_columns[i + 6][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: impactFormat, showInGroupFooter: false, alignByColumn: true, cssClass : "align-right"});
                        if (showTotalRisk && dilutedMode == 0) summaryOptions.totalItems.push({ column: events_columns[i + 6][COL_COLUMN_DATA_FIELD], summaryType : "sum", displayFormat: "{0}", valueFormat: impactFormat, cssClass : "align-right" }); // Grid Total
                        <% End If %>
                        columns.push(clmn);
                        i += 6;
                    } else {                    
                        columns.push(createEventColumn(events_columns[i], i < 2, hasState));
                        if (events_columns[i][COL_COLUMN_DATA_FIELD] == "name" && showTotalRisk && dilutedMode == 0) {
                            summaryOptions.totalItems.push({ column: events_columns[i][COL_COLUMN_DATA_FIELD], summaryType : "custom", displayFormat: "<% = If (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel, "Net " + RiskHeader, "Total " + RiskHeader) %>:", cssClass : "align-right" }); // Grid Total caption
                        }               
                        if (events_columns[i][COL_COLUMN_DATA_FIELD] == "likelihood_contributions") {
                            columns[columns.length - 1].groupCellTemplate = function (element, info) {
                                var retVal = "";
                                info.value.forEach(function(item) {
                                    retVal += (retVal == "" ? "" : ", ") + coveringSources[item].name + (coveringSources[item].info == "" ? "" : " – " + coveringSources[item].info);
                                });
                                if (retVal == "") retVal = "<% = ParseString("No %%Objective(l)%%") %>";
                                info.displayValue = retVal;
                                element.append(retVal);
                            }
                        }
                        
                    }
                }                 
                
                var ds = events_data.filter(function(item) { return item.enabled; });

                $("#tableContent").dxDataGrid({
                    allowColumnResizing: true,
                    allowColumnReordering: true,
                    dataSource: ds,
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
                    editing: {
                        mode: "cell",
                        useIcons: true,
                        allowUpdating: !isReadOnly
                    },
                    filterPanel: {
                        visible: true
                    },
                    filterRow: {
                        visible: true
                    },
                    hoverStateEnabled: true,
                    grouping: {
                        autoExpandAll: true,
                    },
                    groupPanel: {
                        visible: true
                    },
                    onCellPrepared: function(e) {
                        //if (e.rowType === "header" && e.column.dataField !== COL_CTRL_ID && e.column.dataField !== COL_CTRL_NAME && e.column.dataField !== COL_CTRL_INFODOC) {
                        //    e.cellElement.on("click", function(args) {
                        //        var sortOrder = e.column.sortOrder;
                        //        if (!e.column.type && sortOrder == undefined) {
                        //            e.component.columnOption(e.column.index, "sortOrder", "desc");
                        //            args.preventDefault();
                        //            args.stopPropagation();
                        //        }
                        //    });
                        //}
                        if (e.rowType == "data" && e.column.dataField == COL_CTRL_NAME) {
                            e.cellElement.addClass("wrt_link");
                        }
                    },
                    onCellClick: function (e) {
                        if ((e.column) && e.column.isRiskColumn && e.rowType == "header") {
                            sortByGroupSummaryInfo = [{ summaryItem: e.column.dataField, sortOrder: e.column.sortOrder == "asc" ? "desc" : "asc" }];
                            localStorage.setItem(sortStorageKey, JSON.stringify(sortByGroupSummaryInfo));
                            $("#tableContent").dxDataGrid("instance").option("sortByGroupSummaryInfo", sortByGroupSummaryInfo);
                        }
                        if ((e.column) && (e.row) && e.column.dataField == "name" && e.row.rowType == "data") {
                            var nav_to_pgid = 70008;
                            var pg = <%  =CurrentPageID %>;
                            switch (pg*1) {
                                case <% = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS %>:
                                case <% = _PGID_RISK_PLOT_OVERALL %>:
                                    nav_to_pgid = <% = _PGID_RISK_BOW_TIE %>;
                                    break;
                                case <%  = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES %>:
                                case <% = _PGID_RISK_PLOT_CAUSES%>:
                                    nav_to_pgid = <% = _PGID_RISK_BOW_TIE_CAUSES %>;
                                    break;
                                case <% = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_OBJS %>:
                                case <% = _PGID_RISK_PLOT_OBJECTIVES %>:
                                    nav_to_pgid = <% = _PGID_RISK_BOW_TIE_OBJS %>;
                                    break;
                            }        
                            if (<% = Bool2JS(PagesWithControlsList.Contains(CurrentPageID)) %>) {
                                switch (pg*1) {
                                    case <% = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL %>:
                                    case <% = _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS %>:
                                    case <% = _PGID_RISK_REGISTER_WITH_CONTROLS %>:
                                        nav_to_pgid = <% = _PGID_RISK_BOW_TIE_WITH_CONTROLS %>;
                                        break;
                                    case <% = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES %>:
                                    case <% = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS %>:
                                        nav_to_pgid = <% = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES %>;
                                        break;
                                    case <% = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OBJS %>:
                                    case <% = _PGID_RISK_PLOT_OBJECTIVES_WITH_CONTROLS %>:
                                        nav_to_pgid = <% = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS %>;
                                        break;
                                }   
                            }
                            <%--onShowIFrame(pageByID(nav_json, nav_to_pgid).url + "&selected_event=" + e.data.guid + "&selected_node=" + e.data.wrt_node_guid + "&temptheme=sl", "<% = JS_SafeString(PageTitle(_PGID_RISK_BOW_TIE)) %>");--%>
                            onShowIFrame(pageByID(nav_json, nav_to_pgid).url + "&selected_event=" + e.data.guid + "&selected_node=" + e.data.wrt_node_guid + "&temptheme=sl", "<% = JS_SafeString(ParseString("%%Likelihoods%%, %%Impacts%% and %%Risks%%")) %>");
                        }
                    },
                    onContentReady: function (e) {
                        //if (!hasInit) {
                        //    hasInit = true;
                        //    setTimeout(function () { 
                        //        e.component.repaint(); resizePage(true); 
                        //    }, 100);
                        //}
                        $(".dx-icon-decreaselinespacing,.dx-icon-increaselinespacing").css({"font-size": "14pt", "margin-left" : "-5px"});
                        $(".dx-toolbar-after").css({"z-index": 0});
                        $(".dx-toolbar-before").css({"z-index": 1});
                        var has_groups = e.component.getVisibleRows().some(function(row) {return row.rowType == "group";});
                        $(".toolbar-button-toggle-grouping-collapse-state").each(function() {
                            $(this).dxButton("instance").option("visible", has_groups);
                        });
                        hideLoadingPanel();
                    },
                    onRowUpdating: function (e) {
                        callAPI("pm/hierarchy/?action=update_alternative", {"key": e.key.guid, "event_type" : e.newData.event_type}, function (data) {
                            if (!data.success) {
                                e.cancel = true;
                            } else {
                                sendCommand('action=probability_calculation_mode&value=' + probability_calculation); //reload events priorities
                            }
                            DevExpress.ui.notify(data.success ? "Changes Saved" : "Error", data.success ? "success" : "error");
                            e.component.option("validationStatus", data.success ? "valid" : "invalid");
                        }, true);
                    },
                    onToolbarPreparing: function(e) {
                        var toolbarItems = e.toolbarOptions.items;
                        toolbarItems.splice(0, 0, {
                            widget: 'dxButton', 
                            options: {
                                elementAttr: {class: "grid-toolbar toolbar-button-toggle-grouping-collapse-state"},
                                icon: 'decreaselinespacing', visible: false, width: 30,
                                onClick: function() { e.component.collapseAll(); } 
                            },
                            locateInMenu: "never", 
                            location: 'before'
                        });
                        toolbarItems.splice(1, 0, {
                            widget: 'dxButton', 
                            options: { 
                                elementAttr: {class: "grid-toolbar toolbar-button-toggle-grouping-collapse-state"},
                                icon: 'increaselinespacing', visible: false, width: 30,
                                onClick: function() { e.component.expandAll(); } 
                            },
                            locateInMenu: "never", 
                            location: 'before'
                        });
                        toolbarItems.splice(2, 0, {
                            widget: 'dxButton',
                            visible: !is_ie,
                            options: {
                                elementAttr: {class: "grid-toolbar"},
                                icon: 'fas fa-camera', visible: true, width: 30,
                                onClick: function() { 
                                    //gridPrintStyle = !gridPrintStyle;
                                    var switchPrintStyle = function (gridPrintStyle) {
                                        e.component.option("rowAlternationEnabled", !gridPrintStyle);
                                        e.component.option("showRowLines", !gridPrintStyle);
                                        e.component.option("showColumnLines", !gridPrintStyle);
                                        e.component.option("groupPanel.visible", !gridPrintStyle);
                                        e.component.option("searchPanel.visible", !gridPrintStyle);
                                        e.component.option("columnFixing", { "enabled" : !gridPrintStyle });
                                        e.component.option("columns[0].fixed", !gridPrintStyle);
                                        e.component.option("columns[1].fixed", !gridPrintStyle);
                                        e.component.option("columns[2].fixed", !gridPrintStyle);
                                        e.component.repaint();
                                        $(e.element).find(".dx-datagrid-headers td").css({ "background-color": gridPrintStyle ? "white" : "#F0F0F0" });
                                        //$("i.dx-icon.fas.fa-camera").toggleClass("toolbar-button-inactive", !gridPrintStyle);
                                    }
                                    switchPrintStyle(true);

                                    if (!is_ie) {
                                        //e.toolbarOptions.visible = false;
                                        $("#tableContent").find(".dx-toolbar").hide();
                                        $("#trToolbar").hide();
                                        resizePage();
                                        showLoadingPanel();
                                        html2canvas(document.querySelector('#tableRiskRegisterMain'), {
                                            logging: false,
                                            scrollX: 0,
                                            scrollY: -5
                                        }).then(function(canvas) {
                                            //e.toolbarOptions.visible = true;
                                            $("#tableContent").find(".dx-toolbar").show();
                                            $("#trToolbar").show();
                                            resizePage();
                                            hideLoadingPanel();                                            
                                            download(canvas.toDataURL(), "<% = SafeFileName(App.ActiveProject.ProjectName) %> - Risk Register.png", "image/png");
                                            switchPrintStyle(false);
                                        });
                                    }
                                }                               
                            },
                            locateInMenu: "never", 
                            location: 'after'
                        });
                    },
                    pager: {
                        allowedPageSizes: <% = _OPT_PAGINATION_PAGE_SIZES %>,
                        showPageSizeSelector: true,
                        showNavigationButtons: true,
                        visible: ds.length > <% = _OPT_PAGINATION_LIMIT %>
                    },
                    paging: {
                        enabled: ds.length > <% = _OPT_PAGINATION_LIMIT %>
                        },
                    rowAlternationEnabled: true,
                    showColumnLines: true,
                    showBorders: false,
                    showRowLines: true,
                    searchPanel: {
                        visible: true,
                        width: 240,
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
                    sortByGroupSummaryInfo : sortByGroupSummaryInfo,
                    summary: summaryOptions,
                    noDataText: "<% = GetEmptyMessage() %>",
                    "export": {
                        "enabled" : true
                    },
                    wordWrapEnabled: true
                        
                });
                break;
            
            case rvTreatmentRegister:
                //init columns headers                
                var columns = [];
                for (var i = 0; i < treatments_columns.length; i++) {
                    var clmn = { "caption" : treatments_columns[i][COL_COLUMN_NAME], "dataField" : treatments_columns[i][COL_COLUMN_DATA_FIELD], "alignment" : treatments_columns[i][COL_COLUMN_CLASS], "allowSorting" : i < treatments_columns.length - 1, "allowSearch" : true, "allowFiltering" : true, "encodeHtml" : i < 2 };
                    columns.push(clmn);
                    if (!hasState) {
                        clmn.visible = !showDescriptions && i == "info" ? false : treatments_columns[i][COL_COLUMN_VISIBILITY];
                    }
                    if (treatments_columns[i][COL_COLUMN_DATA_FIELD] == "info") clmn.type = "html";
                    if (treatments_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_APPLICATIONS) {
                        clmn.class += " text-overflow td-line-height-15";
                        clmn["encodeHtml"] = false;
                        clmn["cellTemplate"] = function(element, info) {
                            var valuesList = info.value.split("\n");
                            var sVal = "";
                            for (var j = 0; j < valuesList.length; j++) {                        
                                if (valuesList[j] + "" != "") {                            
                                    sVal += (sVal == "" ? "" : "<br>") + "<span title='" + valuesList[j] + "'>" + ShortString(valuesList[j], 50) + "</span>";
                                }
                            }
                            element.append($("<span class='no_wrap'></span>").append(sVal));
                        };
                    }
                    if (treatments_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTLR_EFFECTIVENESS) {
                        clmn["encodeHtml"] = false;
                        clmn["cellTemplate"] = function(element, info) {            
                            var valuesList = info.value.split("\n");
                            var sVal = "";
                            for (var j = 0; j < valuesList.length; j++) {                        
                                if (valuesList[j] + "" != "N/A") {
                                    var fVal = jsDouble2Fixed(valuesList[j] * 100, DecimalDigits, true);
                                    sVal += htmlGraphBarWithValue(valuesList[j] * 100, 100, fVal, "<% =ImagePath %>", 75, 10, BAR_DEFAULT_COLOR);
                                } else {
                                    sVal += (sVal == "" ? "" : "<br>") + "<%=ResString("lblNA")%>";
                                }
                            }
                            element.append($("<span class='no_wrap'></span>").append(sVal));
                        };
                    }
                }

                table_treatments = $("#tableContent").dxDataGrid({
                    allowColumnResizing: true,
                    dataSource: treatments_data,
                    columnAutoWidth: true,
                    columnResizingMode: 'widget',
                    columns: columns,
                    columnChooser: {
                        mode: "select",
                        enabled: true
                    },
                    searchPanel: {
                        visible: true,
                        width: 240,
                        placeholder: "<%=ResString("btnDoSearch")%>..."
                    },
                    noDataText: "<% = GetEmptyMessage() %>",
                    "export": {
                        "enabled" : true,
                        customizeExcelCell: function(options) {
                            var gridCell = options.gridCell;
                            if(!gridCell) {
                                return;
                            }

                            if(gridCell.rowType === "data") {
                                if (gridCell.column.dataField == COL_CTRL_APPLICATIONS || gridCell.column.dataField == COL_CTLR_EFFECTIVENESS) {
                                    options.wrapTextEnabled = true;
                                }
                            }
                        }
                    },
                    showRowLines: true,
                    stateStoring: {
                        enabled: true,
                        type: "localStorage",
                        storageKey: "TreatmentsRegister_DxDatagrid_<%=PRJ.ID%>_<%=App.ActiveUser.UserID%>_<%=CurrentPageID%>"
                    },
                    onContentReady: function (e) {
                    //    setTimeout(function () { resizePage(true); }, 400);
                        hideLoadingPanel();
                    },
                    wordWrapEnabled: true
                });
                //resizePage(true);
                break;

            case rvAcceptanceRegister:
                //init columns headers                
                var columns = [];
                for (var i = 0; i < acceptance_columns.length; i++) {
                    columns.push({ "caption" : acceptance_columns[i][COL_COLUMN_NAME], "dataField" : acceptance_columns[i][COL_COLUMN_DATA_FIELD], "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowFiltering" : true });
                }
                
                table_acceptance = $("#tableContent").dxDataGrid({
                    allowColumnResizing: true,
                    dataSource: acceptance_data,
                    columnAutoWidth: true,
                    columnResizingMode: 'widget',
                    columns: columns,
                    columnChooser: {
                        mode: "select",
                        enabled: true
                    },
                    searchPanel: {
                        visible: true,
                        width: 240,
                        placeholder: "<%=ResString("btnDoSearch")%>..."
                    },
                    noDataText: "<% = GetEmptyMessage() %>",
                    "export": {
                        "enabled" : true
                    },
                    stateStoring: {
                        enabled: true,
                        type: "localStorage",
                        storageKey: "AcceptanceRegister_Datagrid_<%=PRJ.ID%>_<%=App.ActiveUser.UserID%>_<%=CurrentPageID%>"
                    },
                    onContentReady: function (e) {
                        //    setTimeout(function () { resizePage(true); }, 400);
                        hideLoadingPanel();
                    },
                    wordWrapEnabled: true
                });
                break;
        }        
        resizePage(true);
    }


    /* Hierarchy TreeList */
    function initTreeList() {
        <% If PagesGridWrtObjective.Contains(CurrentPageID) Then %>
        var storageKey = "RiskRegister_TL_<%=PRJ.ID%>_<% = Bool2JS(IsWidget) %>_" + columns_ver;
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
                <% If PagesWithControlsList.Contains(CurrentPageID) AndAlso Not PagesToObjectivesWithControls.Contains(CurrentPageID) Then %>
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
                        element.append("<span" + (info.data["is_cat"] ? " class='categorical' " : "") + ">" + info.data["name"] + "</span>");
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
            height: "100%",
            hoverStateEnabled: true,
            focusedRowEnabled: true,
            focusedStateEnabled: true,
            focusedRowKey: selectedHierarchyNodeID,
            onFocusedRowChanging: function (e) {
                if (isHierarchyMultiSelectMode || isLoadingTreeListSessionSettings) e.cancel = true;
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
                    e.component.option("focusedRowKey", isHierarchyMultiSelectMode ? undefined : selectedHierarchyNodeID);
                }, 10);
            },
            onSelectionChanged: function (e) {
                if (isHierarchyMultiSelectMode) {
                    var selectedRowKeys = "";
                    e.selectedRowKeys.forEach(function(item) { selectedRowKeys += (selectedRowKeys == "" ? "" : ",") + item; });
                    sendCommand("<% =_PARAM_ACTION %>=selected_multiple_nodes&value=" + selectedRowKeys, true);
                }
            },
            onToolbarPreparing: function(e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.splice(0, 0, {
                    widget: 'dxButton', 
                    locateInMenu: "never", 
                    showText: "inMenu",
                    location: 'before',
                    options: {
                        elementAttr: {class: "toolbar-button-toggle-selection-state"},
                        icon: 'selectall', visible: true, disabled: false, width: 30,
                        hint: "Selection Mode",
                        onClick: function() { 
                            isHierarchyMultiSelectMode = !isHierarchyMultiSelectMode;
                            e.component.option("selection.mode", isHierarchyMultiSelectMode ? "multiple" : "none");
                            e.component.option("focusedRowKey", isHierarchyMultiSelectMode ? undefined : selectedHierarchyNodeID);
                            if (isHierarchyMultiSelectMode) {
                                e.component.beginUpdate();
                                e.component.selectRows([selectedHierarchyNodeID], false);
                                e.component.endUpdate();
                            } else {
                                sendCommand("<% =_PARAM_ACTION %>=selected_multiple_nodes&value=undefined", true);
                            }
                        } 
                    }
                });
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

    /* Users and Groups */
    var users_data  = <% = GetUsersData()%>;
    var groups_data = <% = GetGroupsData()%>;
    var useCIS = <%=Bool2JS(App.ActiveProject.ProjectManager.PipeParameters.UseCISForIndividuals)%>;

    var IDX_USER_CHECKED = 0;
    var IDX_USER_ID      = 1;
    var IDX_USER_NAME    = 2;
    var IDX_USER_EMAIL   = 3;
    var IDX_USER_HAS_DATA= 4;
    var IDX_GROUP_PARTICIPANTS = 5;
    var IDX_GROUP_NAME_EXTRA = 6;
    var IDX_GROUP_NAME_HINT = 7;

    var dialog_result = false;
    var dlg_users;

    function SelectAllUsers(action) {
        var cb_arr = $("input:checkbox.user_cb");
        $.each(cb_arr, function(index, val) { 
            if (action == 0) val.checked = false; 
            if (action == 1) val.checked = val.getAttribute("has_data") == "1";
        });
        updateUsersWithDataInGroupCheckState();
        return false;
    }

    function getUserByID(id) {
        for (var u = 0; u < users_data.length; u++) {
            if (users_data[u][IDX_USER_ID] == id) return users_data[u];
        }
        for (var u = 0; u < groups_data.length; u++) {
            if (groups_data[u][IDX_USER_ID] == id) return groups_data[u];
        }
        return false;
    }

    function selectUsersDialog() {
        if ((dlg_users)) dlg_users = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_users = $("#divUsersAndGroups").dialog({
            autoOpen: false,
            width: 880,
            height: 435,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "<%=SafeFormString(ResString("btnParticipantsAndGroups"))%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_users.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_users.dialog( "close" ); }}],
            open:  function() { $("body").css("overflow", "hidden"); initUsersTable(); initGroupsTable(); document.body.style.cursor = "default";},
            close: onUsersDialogClose
        });
        dlg_users.dialog('open');
    }

    function onUsersDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            var params = "";
            var cb_arr = $("input:checkbox.user_cb");
            $.each(cb_arr, function(index, val) { var uid = val.getAttribute("uid")*1; var u = getUserByID(uid); if ((u)) u[IDX_USER_CHECKED] = (val.checked ? 1 : 0); });
            var selectedTotal = 0;
            for (var i = 0; i < users_data.length; i++) {                
                if (users_data[i][IDX_USER_CHECKED] > 0) { params += (params == "" ? "" : ",") + users_data[i][IDX_USER_ID]; selectedTotal += 1; } 
            };
            for (var i = 0; i < groups_data.length; i++) {                
                if (groups_data[i][IDX_USER_CHECKED] > 0) { params += (params == "" ? "" : ",") + groups_data[i][IDX_USER_ID]; selectedTotal += 1; } 
            };      
            if (selectedTotal == 0) { 
                getUserByID(<%=COMBINED_USER_ID%>)[IDX_USER_CHECKED] = 1;
                selectedTotal = 1;
                params = "<%=COMBINED_USER_ID%>";
            };
            <%--callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=selected_users", { "name": "ids", "value" : params }, function () {
                
            }, false);--%>

            sendCommand("<% =_PARAM_ACTION %>=selected_users&value=" + params, true);
        }        
    }

    function initUsersTable() {        
        var columns = [];

        //init columns headers                
        columns.push({ "title" : "", "class" : "td_center", "sortable" : true, "searchable" : false });
        columns.push({ "title" : "UserID", "bVisible" : false, "searchable" : false });
        columns.push({ "title" : "<%=ResString("tblSyncUserName")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });
        columns.push({ "title" : "<%=ResString("tblEmailAddress")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });        
        columns.push({ "title" : "<nobr><%=ResString("lblHasData")%>&nbsp;&nbsp;</nobr>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });

        $.each(columns, function(index, val) { val.title += "&nbsp;&nbsp;"; });
        $('#tableUsers').empty();
        table_users = $('#tableUsers').DataTable( {
            dom: 'frti',
            data: users_data,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  true,
            "order": [[ 2, 'asc' ]],
            scrollY: 245,
            //scrollX: true,
            stateSave: false,
            searching: true,
            info:      false,
            "rowCallback": function( row, data, index ) {
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' " + (!(data[IDX_USER_HAS_DATA] == 1 || useCIS), " disabled='disabled' ", "") + " "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" onclick='updateUsersWithDataInGroupCheckState();' >");
                $("td:eq(1)", row).html(htmlEscape(data[IDX_USER_NAME]));
                $("td:eq(3)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : ""));
                if (!(data[IDX_USER_HAS_DATA] == 1 || useCIS)) { $("td", row).css("color", "#909090"); <%--$("td:eq(0)", row).children().first().attr("disabled","disabled");--%> }
            },
            "language" : {"emptyTable" : "<h6 style='margin:2em 10em'><nobr><% =GetEmptyMessage()%></nobr></h6>"}
        });
        
        setTimeout(function () { $(".dataTables_filter").css({"float":"left", "padding-bottom":"10px"}); }, 100);
        setTimeout(function () { $("input[type=search]").focus(); }, 1000);

        // search in groups table when typing in a search field of the users table
        table_users.on('search.dt', function () {
            table_groups.search(table_users.search()).draw();
        } );
    }

    function allGroupUsersWithDataChecked(grp_id) {
        var retVal = true;

        var g = [];
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][IDX_USER_ID] == grp_id) g = groups_data[i];
        }

        if (g.length > 0) {
            if (g[IDX_GROUP_PARTICIPANTS].length == 0) retVal = false;
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);                                
                if ((u)) {
                    if (u[IDX_USER_HAS_DATA] == 1 && u[IDX_USER_CHECKED] != 1) retVal = false;
                }
            }
        }

        return retVal;
    }

    function checkUsersWithDataInGroup(grp_id, chk) {
        var g = [];
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][IDX_USER_ID] == grp_id) g = groups_data[i];
        }

        if (g.length > 0) {
            if (g[IDX_GROUP_PARTICIPANTS].length == 0) retVal = false;
            var cb_arr = $("input:checkbox.user_cb");
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);                                
                if ((u) && u[IDX_USER_HAS_DATA] == 1) {
                    $.each(cb_arr, function(index, val) { var uid = val.getAttribute("uid")*1; if (uid == g[IDX_GROUP_PARTICIPANTS][i]) val.checked = chk; });
                }
            }
        }
    }

    function updateUsersWithDataInGroupCheckState() {        
        var cb_arr = $("input:checkbox.group_all_users_cb");
        $.each(cb_arr, function(index, val) { 
            var gid = val.getAttribute("uid")*1; 
            var g = [];
            for (var i = 0; i < groups_data.length; i++) {
                if (groups_data[i][IDX_USER_ID] == gid) g = groups_data[i];
            }
            if (g.length > 0) {                
                var u_arr = $("input:checkbox.user_cb");
                var all_checked = g[IDX_GROUP_PARTICIPANTS].length > 0;
                $.each(u_arr, function(u_index, u_val) { 
                    var uid = u_val.getAttribute("uid")*1; 
                    var u = getUserByID(uid);
                    if ($.inArray(uid, g[IDX_GROUP_PARTICIPANTS]) >= 0 && u[IDX_USER_HAS_DATA] == 1 && !u_val.checked) all_checked = false; 
                });
                val.checked = all_checked;
            }
        });
    }

    function initGroupsTable() {        
        var columns = [];

        //init columns headers                
        columns.push({ "title" : "", "class" : "td_center", "sortable" : true, "searchable" : false });
        columns.push({ "title" : "GroupID", "bVisible" : false, "searchable" : false });
        columns.push({ "title" : "<%=ResString("lblGroupName")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });
        columns.push({ "title" : "<nobr><%=ResString("lblHasData")%>&nbsp;&nbsp;</nobr>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });        
        columns.push({ "title" : "<small>Select all users with data</small>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });

        $.each(columns, function(index, val) { val.title += "&nbsp;&nbsp;"; });
        $('#tableGroups').empty();

        table_groups = $('#tableGroups').DataTable( {
            dom: 'rti',
            data: groups_data,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  true,
            "order": [[ 0, 'desc' ]],
            scrollY: 245,
            //scrollX: true,
            stateSave: false,
            searching: true,
            info:      false,
            "rowCallback": function( row, data, index ) {
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" >");
                $("td:eq(1)", row).html((data[IDX_USER_ID] != -1 ? "<img id='imgGrp" + data[IDX_USER_ID] + "' src='<% =ImagePath%>old/plus.gif' width='9' height='9' border='0' style='margin-top:2px; margin-right:4px; cursor:pointer;' onclick='expandGroupUsers(" + data[IDX_USER_ID] + ");' >" : "") + htmlEscape(data[IDX_USER_NAME]) + "<div id='divExpandedUsers" + data[IDX_USER_ID] + "'></div>");
                $("td:eq(2)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : "<%=ResString("lblNo")%>"));
                $("td:eq(3)", row).html("<input type='checkbox' class='group_all_users_cb' uid='"+data[IDX_USER_ID]+"' " + (allGroupUsersWithDataChecked(data[IDX_USER_ID]) ? " checked ":"")+" onclick='checkUsersWithDataInGroup(\"" + data[IDX_USER_ID] + "\", this.checked);' " + (data[IDX_GROUP_PARTICIPANTS].length == 0 ? " disabled='disabled' " : "") + " >");
                if (data[IDX_USER_HAS_DATA] != 1) { $("td", row).css("color", "#909090"); $("td:eq(0)", row).children().first().attr("disabled","disabled"); }
            },
        });
    }

    function expandGroupUsers(grp_id) {
        var s = "";
        var g = getUserByID(grp_id);
        if ((g)) {
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);
                if ((u)) {
                    var hd = u[IDX_USER_HAS_DATA];
                    s += (s == "" ? "" : "<br>") + "<nobr><small " + (hd != 1 ? "style='color:#909090;'" : "") +">&nbsp;&#8226;&nbsp;" + htmlEscape(u[IDX_USER_NAME]) + "</small></nobr>";
                }
            }
            $("#divExpandedUsers" + grp_id).html(s);
            var img = document.getElementById("imgGrp" + g[IDX_USER_ID]);
            if ((img)) {
                img.src = "<%=ImagePath%>old/minus.gif";
                img.onclick = function () { collapseGroupUsers(g[IDX_USER_ID]); };
            }
        }
    }

    function collapseGroupUsers(grp_id) {
        $("#divExpandedUsers" + grp_id).html("");
        var img = document.getElementById("imgGrp" + grp_id);
        if ((img)) {
            img.src = "<%=ImagePath%>old/plus.gif";
            img.onclick = function () { expandGroupUsers(grp_id); };
        }
    }
    /* --- Users and Groups */

    function initToolbar() {
        if (isWidget) return;

        var toolbarItems = [
<%If RegisterView <> RegisterViews.rvAcceptanceRegister Then%>
            {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                disabled: false,
                options: {
                    text: " ",
                    icon: "fas fa-users",
                    hint: "<% = ResString("btnParticipantsAndGroups")%>",
                    showText: false,
                    elementAttr: {id: "btnSelectUsers"},
                    onClick: function (e) {
                        selectUsersDialog();
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                disabled: false,
                options: {
                    text: "Loss Exceedance…",
                    icon: "fas fa-chart-area",
                    hint: "<%=ResString(CStr(IIf(PRJ.isOpportunityModel, "titleRiskExtraPlotOpp", "titleRiskExtraPlot")))%>",
                    showText: true,
                    elementAttr: {id: "btnRiskExtraPlot"},
                    onClick: function (e) {
                        showExtraPlot();
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                disabled: false,
                options: {
                    text: "<% = ParseString("Filter %%Alternatives%%") %>",
                    icon: "fas fa-filter <% = If(PM.ActiveAlternatives.RootNodes.Count > 0 AndAlso PM.ActiveAlternatives.RootNodes.Where(Function(a) Not a.Enabled).Count > 0, "attention", "") %>",
                    hint: "<% = ParseString("Filter %%Alternatives%%") %>",
                    showText: true,
                    elementAttr: {id: "btnFilterEvents"},
                    onClick: function (e) {
                        onSelectEventsClick();
                    }
                }
            }, {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: true,
                disabled: false,
                options: {
                    showText: true,
                    text: "Simulated Results",
                    value: <% = Bool2JS(PM.CalculationsManager.UseSimulatedValues) %>,
                    elementAttr: {id: 'cbSimulatedSwitch'},
                    hint: "",
                    onValueChanged: function (e) { 
                        sendCommand("action=show_sim_results&value=" + e.value, true);
                    }
                }
            }, {   location: 'before',
                locateInMenu: 'never',
                widget: 'dxCheckBox',
                visible: <% = Bool2JS(CurrentPageID <> _PGID_RISK_TREATMENTS_DICTIONARY AndAlso CurrentPageID <> _PGID_RISK_TREATMENTS_REGISTER) %>,
                disabled: false,
                options: {
                    showText: true,
                    text: "<% = ResString("lblShowDollarValue") %>",
                    value: <% = Bool2JS(ShowDollarValue) %>,
                    elementAttr: { 
                        id: 'cbDollarValue',
                        title: "<% = ResString("lblShowDollarValue") + GetDollarValueFullString() %>"
                        },
                    hint: "",
                    onValueChanged: function (e) {
                        if (showDollarValue !== e.value) {
                            showDollarValue = e.value;
                            initDataGrid();
                            sendCommand("action=show_dollar_value&chk=" + (e.value ? "1" : "0"));
                        }
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: <% = Bool2JS(CurrentPageID <> _PGID_RISK_TREATMENTS_DICTIONARY AndAlso CurrentPageID <> _PGID_RISK_TREATMENTS_REGISTER) %>,
                disabled: false,
                options: {
                    text: "",
                    icon: "fas fa-hand-holding-usd",
                    hint: "Edit Value of Enterprise",
                    showText: false,
                    elementAttr: {id: "btnSelectUsers"},
                    onClick: function (e) {
                        settingsDollarValueDialog();
                    }
                }
            },
<%End If%>
            {
                location: 'after',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: true,
                options: {
                    beginGroup: true,
                    icon: "fas fa-cog", text: "Preferences", hint: "Preferences",
                    template: function() {
                        return $('<i class="dx-icon fas fa-cog"></i><span class="dx-button-text nowide1200">Preferences</span>');
                    },
                    disabled: false,
                    elementAttr: {id: 'btn_settings'},
                    onClick: function (e) {
                        settingsDialog();
                    }
                }
            }
        ];
    
        $("#divToolbar").dxToolbar({
            items: toolbarItems,
            disabled: false
        });
    }

    function cbDecimalsChange(value) {
        DecimalDigits = value*1;
        sendCommand("action=" + ACTION_DECIMALS + "&value=" + value);
    }

    function updateToolbar() {
        if (isWidget) return;

        $("#divToolbar").dxToolbar("instance").beginUpdate();

        if ($("#cbDollarValue").data("dxCheckBox")) {
            $("#cbDollarValue").dxCheckBox("instance").option("disabled", !DollarValueDefined);
            $("#cbDollarValue").dxCheckBox("instance").option("value", showDollarValue);
        }

        $(".on_advanced").toggle(isAdvancedMode);

        $("#divToolbar").dxToolbar("instance").endUpdate();
    }

    /* Settings Dialog */

    function getOptimalNumberOfTrials() {
        sendCommand('action=get_optimal_unmber_of_trials');
    }

    var popupSettings = null, 
        popupOptions = {
            animation: null,
            width: 610,
            height: "auto",
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    $("<div class='dx-fieldset-header'><%=ParseString("%%Alternatives%% Display")%></div>"),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'><%=ParseString("%%Alternative%% Numbers")%></div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbEventIDMode'></div>"))),
                        <%--$("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'><%=ParseString("Show %%Alternative%% Descriptions")%></div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbShowEventDescriptions'></div>"))),--%>
                    $("<br >"),
                    $("<div class='dx-fieldset-header'>Display Settings</div>"),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Timestamp</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbTimestamp'></div>"))),
                        $("<div class='dx-field' style='display: none;'></div>").append(
                            $("<div class='dx-field-label'>Probability Calculation</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbProbabilityCalculation'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Consequences Simulation Mode</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbDilutedMode'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>WRT Calculation</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbWRTCalculation'></div>"))),
                        //$("<div class='dx-field'></div>").append(
                        //    $("<div class='dx-field-label'>CIS</div>"),
                        //    $("<div class='dx-field-value'></div>").append(
                        //        $("<div id='cbCIS'></div>"))),
                        //$("<div class='dx-field'></div>").append(
                        //    $("<div class='dx-field-label'>User Priorities</div>"),
                        //    $("<div class='dx-field-value'></div>").append(
                        //        $("<div id='cbWeights'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'><%=ParseString("Show Total %%Risk%% (Total Average %%Loss%%)")%></div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbShowTotalRisk'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Decimals</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbDecimals'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Show cents for monetary values</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbShowCents'></div>"))),
                    $("<br >"),
                    $("<div class='dx-fieldset-header'>Simulations Settings</div>"),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Number Of Trials</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='tbNumSimulations'></div>"),$('<a href="" onclick="getOptimalNumberOfTrials(); return false;"><i class="fas fa-magic"></i>&nbsp;Get Optimal Number Of Trials</a>'))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Seed</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='tbRandSeed'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Keep Seed</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbKeepRandSeed'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'><% = ParseString("Use %%Objective(l)%% Groups") %></div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbUseSourceGroups'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'><% = ParseString("Use %%Alternative%% Groups") %></div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='cbUseEventGroups'></div>"))),
                    $("<div id='btnSettingsClose' style='margin-top: 10px; margin-bottom: 10px; float: right;'></div>")
                );
            },
            showTitle: true,
            title: "Preferences",
            dragEnabled: true,
            closeOnOutsideClick: true
        };

    function settingsDialog() {
        if (popupSettings) $(".popupSettings").remove();
        var $popupContainer = $("<div></div>").addClass("popupSettings").appendTo($("#popupSettings"));
        popupSettings = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
        popupSettings.show();
        
        $("#btnSettingsClose").dxButton({
            text: "Close",
            icon: "fas fa-times",
            onClick: function() {
                popupSettings.hide();
            }
        });

        $("#cbProbabilityCalculation").dxSelectBox({ 
            width: "100%",
            items: [ 
                        {"ID": <% = CInt(LikelihoodsCalculationMode.Regular) %>, "Text": "Sum Product"}, 
                        {"ID": <% = CInt(LikelihoodsCalculationMode.Probability) %>, "Text": "Combinatorial"}
                    ],
            hint: "Probability Calculation",
            disabled: false,
            displayExpr: "Text",
            searchEnabled: false,
            value: probability_calculation,
            valueExpr: "ID",
            onSelectionChanged: function (e) { 
                if (probability_calculation !== e.selectedItem.ID * 1) {
                    probability_calculation = e.selectedItem.ID * 1;
                    sendCommand('action=probability_calculation_mode&value=' + probability_calculation);
                }
            }
        });   
        
        $("#cbDilutedMode").dxSelectBox({ 
            width: "100%",
            items: [ 
                        {"ID": <% = CInt(ConsequencesSimulationsMode.Diluted) %>, "Text": "Diluted"}, 
                        {"ID": <% = CInt(ConsequencesSimulationsMode.Undiluted) %>, "Text": "Undiluted"}
                    ],
            hint: "Consequences Simulation Mode",
            disabled: false,
            displayExpr: "Text",
            searchEnabled: false,
            value: dilutedMode,
            valueExpr: "ID",
            onSelectionChanged: function (e) { 
                if (dilutedMode !== e.selectedItem.ID * 1) {
                    dilutedMode = e.selectedItem.ID * 1;
                    $("#cbShowTotalRisk").dxCheckBox("instance").option("disabled", dilutedMode !== 0);
                    sendCommand('action=diluted_mode&value=' + dilutedMode);
                }
            }
        });

        $("#cbWRTCalculation").dxSelectBox({ 
            width: "100%",
            items: [ 
                        {"ID": 0, "Text": "<% = ParseString("Show likelihoods and priority of %%objectives(i)%%") %>"}, 
                        {"ID": 1, "Text": "<% = ParseString("Show vulnerabilities and consequences of %%objectives(i)%%") %>"}
                    ],
            hint: "'With Respect To' Calculation Mode",
            disabled: false,
            displayExpr: "Text",
            searchEnabled: false,
            value: wrt_calculation,
            valueExpr: "ID",
            onSelectionChanged: function (e) { 
                if (wrt_calculation !== e.selectedItem.ID * 1) {
                    wrt_calculation = e.selectedItem.ID * 1;
                    sendCommand('action=wrt_calculation_mode&value=' + wrt_calculation);
                }
            }
        });   

        $("#cbTimestamp").dxCheckBox({
            width: "100%",
            //showText: true,
            //text: "Timestamp",
            value: showTimestamp,
            hint: "Timestamp",
            onValueChanged: function (e) { 
                showTimestamp = e.value; 
                sendCommand("action=timestamp&chk=" + showTimestamp);
            }
        });

        $("#cbShowTotalRisk").dxCheckBox({ 
            width: "100%",
            value: showTotalRisk,
            disabled: dilutedMode !== 0,
            onValueChanged: function (e) {
                showTotalRisk = e.value;
                initDataGrid();
                sendCommand("action=show_total_risk&value=" + e.value, false);
            }
        });

        $("#cbEventIDMode").dxSelectBox({ 
            width: "100%",
            dataSource: [
                //{ "id" : -1, "text" : "None" }, 
                { "id" : <% = CInt(IDColumnModes.UniqueID) %>, "text" : "<%=ResString("optUniqueID")%>" }, 
                { "id" : <% = CInt(IDColumnModes.IndexID) %>, "text" : "<%=ResString("optIndexID")%>" },
                { "id" : <% = CInt(IDColumnModes.Rank) %>, "text" : "<%=ResString("optRank")%>" }
            ],
            disabled: false,
            displayExpr: "text",
            searchEnabled: false,
            <%--value: <% = CInt(If(PM.Parameters.NodeIndexIsVisible, PM.Parameters.NodeVisibleIndexMode, -1)) %>,--%>
            value: <% = CInt(PM.Parameters.NodeVisibleIndexMode) %>,
            valueExpr: "id",
            onValueChanged: function (e) {
                sendCommand("action=event_id_mode&value=" + e.value);
            }
        });    
        
        $("#cbDecimals").dxSelectBox({
            searchEnabled: false,
            valueExpr: "ID",
            value: DecimalDigits,
            displayExpr: "Text",
            disabled: false,
            hint: "Decimals",
            onSelectionChanged: function (e) { 
                if (DecimalDigits != e.selectedItem.ID*1) {
                    cbDecimalsChange(e.selectedItem.ID*1);
                }
            },
            items: [ 
                {"ID": 0, "Text": "0"}, 
                {"ID": 1, "Text": "1"}, 
                {"ID": 2, "Text": "2"}, 
                {"ID": 3, "Text": "3"}, 
                {"ID": 4, "Text": "4"}, 
                {"ID": 5, "Text": "5"}
            ]
        });

        $("#cbShowCents").dxCheckBox({ 
            width: "100%",
            value: showCents,
            disabled: isReadOnly,
            onValueChanged: function (e) {
                if (showCents !== e.value) {
                    showCents = e.value;
                    sendCommand("action=show_cents&value=" + e.value, false);
                }
            }
        });

        // simulation settings
        $("#tbNumSimulations").dxNumberBox({ 
            width: "100%",
            value: numSimulations,
            min: 0,
            disabled: isReadOnly,
            onValueChanged: function (e) {
                if (e.value !== numSimulations) {
                    numSimulations = e.value;
                    sendCommand("action=num_sim&value=" + e.value, false);
                }
            }
        });

        $("#tbRandSeed").dxNumberBox({ 
            width: "100%",
            value: randSeed,
            min: 0,
            disabled: isReadOnly || keepSeed,
            onValueChanged: function (e) {
                if (e.value !== randSeed) {
                    randSeed = e.value;
                    sendCommand("action=rand_seed&value=" + e.value, false);
                }
            }
        });

        $("#cbKeepRandSeed").dxCheckBox({ 
            width: "100%",
            value: keepSeed,
            disabled: isReadOnly,
            onValueChanged: function (e) {
                if (keepSeed !== e.value) {
                    keepSeed = e.value;
                    $("#tbRandSeed").dxNumberBox("instance").option("disabled", isReadOnly || keepSeed);
                    sendCommand("action=keep_rand_seed&value=" + e.value, false);
                }
            }
        });

        $("#cbUseSourceGroups").dxCheckBox({ 
            width: "100%",
            value: use_source_groups,
            disabled: isReadOnly,
            onValueChanged: function (e) {
                if (use_source_groups !== e.value) {
                    use_source_groups = e.value;
                    sendCommand("action=use_source_groups&value=" + e.value, false);
                }
            }
        });

        $("#cbUseEventGroups").dxCheckBox({ 
            width: "100%",
            value: use_event_groups,
            disabled: isReadOnly,
            onValueChanged: function (e) {
                if (use_event_groups !== e.value) {
                    use_event_groups = e.value;
                    sendCommand("action=use_event_groups&value=" + e.value, false);
                }
            }
        });        
        
    }
    /* end Settings Dialog */

    /* Dollar Value dialog */
    var popupDollarValueSettings = null, 
        popupDollarValueOptions = {
            animation: null,
            width: 750,
            height: "auto",
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    $("<div class='dx-fieldset-header'><%=ResString("msgEnterDollarValue")%></div>"),
                    $("<br>"),
                        $("<div class='dx-field' style='text-align: center;'></div>").append(
                            $("<span style='display: inline-block; vertical-align: middle; margin-top: -22px; margin-right: 6px;'><% = JS_SafeString(ResString("lblValueOf")) %></span>"),
                            $("<div id='cbDollarValueTarget' style='display: inline-block; margin: 0px 2px 2px 2px;'></div>"), 
                            $("<div id='tbDollarValue' style='display: inline-block; margin: 0px 2px 2px 2px;'></div>")),
                    $("<br>"),
                    $("<center>").append($("<div id='btnDollarValueSettingsOK' style='margin: 10px 2px 2px 2px;'></div>"), $("<div id='btnDollarValueSettingsReset' style='margin: 10px 2px 2px 2px;'></div>"), $("<div id='btnDollarValueSettingsCancel' style='margin: 10px 2px 2px 2px;'></div>"))
                );
            },
            showTitle: true,
            title: "<% = JS_SafeString(ResString("titleEditUSD")) %>",
            dragEnabled: true,
            closeOnOutsideClick: false
        };

    function settingsDollarValueDialog() {
        if (popupDollarValueSettings) $(".popupDollarValueSettings").remove();
        var $popupDollarValueContainer = $("<div></div>").addClass("popupDollarValueSettings").appendTo($("#popupDollarValueSettings"));
        popupDollarValueSettings = $popupDollarValueContainer.dxPopup(popupDollarValueOptions).dxPopup("instance");
        popupDollarValueSettings.show();
        
        $("#btnDollarValueSettingsOK").dxButton({
            text: "<% = ResString("btnOK") %>",
            icon: "fas fa-check",
            width: 100,
            onClick: function() {
                onSaveDollarValue($("#tbDollarValue").dxNumberBox("instance").option("value"), $("#cbDollarValueTarget").dxSelectBox("instance").option("value"));
                popupDollarValueSettings.hide();
            }
        });

        $("#btnDollarValueSettingsCancel").dxButton({
            text: "<% = ResString("btnCancel") %>",
            icon: "fas fa-ban",
            width: 100,
            onClick: function() {
                popupDollarValueSettings.hide();
            }
        });

        $("#btnDollarValueSettingsReset").dxButton({
            text: "<% = ResString("btnReset") %>",
            icon: "fas fa-eraser",
            width: 100,
            onClick: function() {
                onResetDollarValue();
                $("#cbDollarValueTarget").dxSelectBox("instance").option("selectedIndex", 0);
                $("#tbDollarValue").dxNumberBox("instance").option("value", "");
                popupDollarValueSettings.hide();
            }
        });

        $("#tbDollarValue").dxNumberBox({ 
            width: 150,
            format: "currency",
            showSpinButtons: true,
            value: DollarValue == UNDEFINED_INTEGER_VALUE ? "" : DollarValue
        });

        $("#cbDollarValueTarget").dxSelectBox({ 
            width: 450,
            dataSource: all_obj_data,
            disabled: false,
            displayExpr: "text",
            searchEnabled: false,
            value: DollarValueTarget == "<% = UNDEFINED_STRING_VALUE %>" || DollarValueTarget == "" ? "<% = PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID.ToString %>" : DollarValueTarget,
            valueExpr: "key",
            itemTemplate: function (itemData, itemIndex, itemElement) {
                return $("<div></div>").append(
                    $("<span></span>").html(itemData.html)
                ).prop("title", itemData.title);
            },
            onSelectionChanged: function (e) {
                $("#tbDollarValue").dxNumberBox("instance").option("value", e.selectedItem.dollar_value);                
            }
        });            
    }

    function onSaveDollarValue(value, target) {        
        if (typeof value !== 'undefined' && validFloat(value)) {
            var dValue = str2double(value);            
            DollarValue = dValue;
            DollarValueTarget = target;
            DollarValueDefined = true;
            if (!showDollarValue) {
                showDollarValue = true;                
            }
            updateToolbar();
            sendCommand("action=set_dollar_value&value=" + value + "&target=" + target);
        }
    }
    
    function onResetDollarValue() {
        showDollarValue = false;
        DollarValue = UNDEFINED_INTEGER_VALUE;
        DollarValueTarget = "<% = UNDEFINED_STRING_VALUE %>";
        DollarValueDefined = false;
        sendCommand("action=set_dollar_value&value=" + UNDEFINED_INTEGER_VALUE + "&target=" + DollarValueTarget);
    }
    /* end Dollar Value dialog */

    // Select Events Dialog
    var dlg_select_events;

    function onSelectEventsClick() {
        if (event_list.length > 0) {
            initSelectEventsForm("Select <%=ParseString("%%Alternatives%%")%>");
            dlg_select_events.dialog("open");
            dxDialogBtnDisable(true, true);
        }
    }

    function initSelectEventsForm(_title) {
        cancelled = false;

        var labels = "";

        // generate list of events
        var event_list_len = event_list.length;
        for (var k = 0; k < event_list_len; k++) {
            var checked = event_list[k].enabled;
            labels += "<div class='divCheckbox'><label><input type='checkbox' class='select_event_cb' value='" + event_list[k].guid + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(event_list[k].name) + "</label></div>";
        }

        $("#divSelectEvents").html(labels);

        dlg_select_events = $("#selectEventsForm").dialog({
            autoOpen: false,
            modal: true,
            width: 420,
            minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: 500,
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
                        event_list[index].enabled = val.checked;
                    });
                    $("#btnFilterEvents").dxButton("instance").option("icon", (chk_count !== event_list.length) && event_list.length ? "fas fa-filter attention" : "fas fa-filter");
                    sendCommand('action=select_events&event_ids=' + sEventIDs); // save the selected events via ajax
                }
            }
        });
        $(".ui-dialog").css("z-index", 9999);
    }

    function filterSelectAllEvents(chk) {
        $("input:checkbox.select_event_cb").prop('checked', chk * 1 == 1);
        dxDialogBtnDisable(true, false);
    }
    // end Select Events Dialog

    var plot_wnd = null;

    function showExtraPlot() {
        var _PGURL_EXTRA_PLOT = '<% = GetLecUrl() %>';
        var event_list = _PGURL_EXTRA_PLOT.indexOf("?") !== -1 ? "&ids=" : "?ids=";
        plot_wnd = CreatePopup(_PGURL_EXTRA_PLOT + event_list + "&<% =_PARAM_TEMP_THEME + "=" + _THEME_SL %>", 'RiskPlot', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=1300,height=700');
        setTimeout("if ((plot_wnd)) plot_wnd.focus();", 100);
    }

    function onRichEditorRefreshQH(empty, infodoc, callback)
    { 
        window.focus();
        $get("imgQH").src = ((empty=="1") ? img_qh_ : img_qh).src;
        if ((callback) && (callback.length>1) && (callback[0]!="") && (callback[0])) {
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("confQHPreview")) %>", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    setTimeout('$(\"#lnkQHVew\")[0].click();', 350);
                }
            });
        }
    }

    function editQuickHelp(cmd) {
        OpenRichEditor("?type=11&" + cmd);
    }

    function ShowIdleQuickHelp() {
        showQuickHelp("<% =JS_SafeString(QuickHelpParams) %>", true, <% =Bool2JS(isPM) %>);
    }

    function initSplitter() {
        <% If PagesGridWrtObjective.Contains(CurrentPageID) Then %>
        var splitterStorageKey = "RiskRegister_DxDatagrid_Splitter_<%=PRJ.ID%>_<% = Bool2JS(IsWidget) %>";
        var splitterStorageValue = localStorage.getItem(splitterStorageKey);
        var hasState = typeof splitterStorageValue !== "undefined" && splitterStorageValue != null;

        $("#divSplitter").width(0);
        $("#divContent").width(0);
        $("#tableContent").width(0);

        var totalWidth = $("#tableRiskRegisterMain").innerWidth();

        $("#divSplitter").width(hasState ? splitterStorageValue * 1: totalWidth * 0.2 - 6);
        $("#divContent").width(hasState ? totalWidth - splitterStorageValue * 1 - 6: totalWidth * 0.8);
        $("#tableContent").width($("#divContent").width());

        $("#divSplitter").dxResizable({
            handles: 'right',
            width: hasState ? splitterStorageValue * 1 : "19%",
            onResize: function (e) { 
                $("#divContent").width($("#tableRiskRegisterMain").innerWidth() - e.width - 6);
                $("#divHierarchy").width(e.width - 5);
            },
            onResizeEnd: function (e) {
                localStorage.setItem(splitterStorageKey, e.width);
                //var totalWidth = $("#tableRiskRegisterMain").innerWidth();
                //$("#divSplitter").width(e.width);
                //$("#divContent").width(totalWidth - e.width - 10);
                resizePage();
            }
        });//.addClass("splitter_v"); //.css("background-color", "#ccc");
        <%End If%>
        return true;
    }
    
    $(document).ready(function () { 
        BAR_DEFAULT_COLOR = $("#div-default-bar-color").css("color");
        BAR_LIKELIHOOD_COLOR = $("#div-likelihood-bar-color").css("color");
        BAR_IMPACT_COLOR = $("#div-impact-bar-color").css("color");
        BAR_RISK_COLOR = $("#div-risk-bar-color").css("color");
        BAR_OPPORTUNITY_COLOR = $("#div-opportunity-bar-color").css("color");   
        
        initTreeList();
        initDataGrid();
        initToolbar();
        updateToolbar();

        resizePage(true);
    });

    onSwitchAdvancedMode = function (value) { 
        updateToolbar();
    };

    toggleScrollMain();
    resize_custom = resizePage;

</script>

<div id="div-default-bar-color" class="color-bar-default" style="display: none; width: 0; height: 0;"></div>
<div id="div-likelihood-bar-color" class="color-likelihood" style="display: none; width: 0; height: 0;"></div>
<div id="div-impact-bar-color" class="color-impact" style="display: none; width: 0; height: 0;"></div>
<div id="div-risk-bar-color" class="color-risk" style="display: none; width: 0; height: 0;"></div>
<div id="div-opportunity-bar-color" class="color-opportunity" style="display: none; width: 0; height: 0;"></div>

<table id="tableRiskRegisterMain" border='0' cellspacing="0" cellpadding="0" class="whole">
    <tr id="trToolbar" valign="top">
        <td valign="top" style="padding-bottom:4px" style="text-align: left;">
            <% If IsWidget Then %>
            <% = GetQuickHelpIcons() %>
            <% End if%>
            <% If Not IsWidget AndAlso Not App.Options.isSingleModeEvaluation Then %>
            <div id='divToolbar' class="dxToolbar"></div>
            <% End if%>
        </td>
    </tr>
    <tr id="trHeader">
        <td class='text'  id="tdHeader" valign="top">
            <h5 id="lblPageTitle" style="padding-top: 10px;">
                <% = GetRegisterTitle()%>
                <% If IsWidget AndAlso CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS AndAlso PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then%><% = GetScore() %><% End If %>
                <%If RegisterView = RegisterViews.rvAcceptanceRegister Then%>
                    <center><span class="text"><%=ParseString("(All the %%Objectives(l)%%, %%Vulnerabilities%% and %%Impacts%% that have no %%controls%%)")%></span></center>
                <%End If%>
            </h5>            
        </td>
    </tr>
    <%If RegisterView <> RegisterViews.rvAcceptanceRegister AndAlso PagesWithControlsList.Contains(CurrentPageID) Then%>
    <tr id="trResults">
        <td valign="top" align="center">
            <div id='divResults' class='text' style="margin: 0px 0px 6px 0px; text-align: center;">            
                <table class="text" cellpadding="0" cellspacing="0" style="width: 100%;">
                    <tr>
                        <td><span id="lblActiveCountLabel" style="display:inline-block;text-align:right;white-space:nowrap;"><b>Selected <% =ParseString("%%controls%%") %>:&nbsp;</b></span></td>
                        <td><span class="number_result" id="lblActiveCount" style="color:darkorange;"><%=PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCount%></span></td>
                        <td><span style="display:inline-block;padding-left:10px;white-space:nowrap;text-align:right;"><b><%=ParseString("Cost Of Selected %%Controls%%")%>:&nbsp;</b></span></td>
                        <td><span class="number_result" id="tbFundedCost" style="white-space: nowrap;"><%=CostString(PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True) + DeltaString(PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True, ResString("lblUnfunded") + ":")%></span></td>
                        <td><span style="display:inline-block;padding-left:10px;white-space:nowrap;text-align:right;"><b><%=ParseString("Total Cost Of All %%Controls%%")%>:&nbsp;</b></span></td>
                        <td><span class="number_result" id="tbAllControlsCost" style="white-space: nowrap;"><%=CostString(PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, CostDecimalDigits, True)%></span></td>
                        <td><span style="display:inline-block;padding-left:10px;white-space:nowrap;text-align:right;"><b><%=ParseString("How Selected")%>:&nbsp;</b></span></td>
                        <td style="text-align: left;"><span class="number_result"><% = HowControlsSelected(PRJ, AddressOf ParseString) %></span></td>
                    </tr>
                </table>                                   
            </div>
        </td>
    </tr>
    <%End If%>
    <tr id="trContent" valign="top" class="whole" style="text-align: left;">
        <td id="tdContent" class="whole" valign="top" style="text-align: left; padding: 0px; white-space: nowrap;">
            <% If PagesGridWrtObjective.Contains(CurrentPageID) Then %>
            <div id="divSplitter" style="vertical-align: top; display: inline-block; height: 100%;">
                <div id="divHierarchy" style="background-color: white;" class="whole"></div>
            </div>
            <%End If%>
            <div id="divContent" class="whole" style="vertical-align: top; display: inline-block; height: 100%; overflow: hidden;">
                <div id="tableContent" style="padding: 5px;"></div>
            </div>
        </td>        
    </tr>
    <%If RegisterView = RegisterViews.rvRiskRegister OrElse RegisterView = RegisterViews.rvRiskRegisterWithControls Then%>
    <tr>
        <td>
            <% If False Then %>
            <center>
                <div class="text small" style="margin-top: 0; margin-bottom:3px;">
                    <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_likelihood'/>&nbsp;<% = ResString("lblLikelihood")%>&nbsp;(<% = ResString("lblLikelihood")(0)%>)&nbsp; &nbsp;
                    <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_impact'>&nbsp;<% = ResString("lblImpact")%>&nbsp;(<% = ResString("lblImpact")(0)%>)&nbsp; &nbsp;
                    <%If PRJ.RiskionProjectType <> ProjectType.ptOpportunities Then%>
                    <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_risk' >&nbsp;<% = ResString("lblRisk")%>&nbsp;(<% = ResString("lblRisk")(0)%>)
                    <%End If%>
                     <%If PRJ.isMixedModel OrElse PRJ.isOpportunityModel OrElse PRJ.isMyRiskRewardModel Then%>
                    &nbsp; &nbsp;<img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_opportunity' >&nbsp;<% = ResString("lblOpportunityModel")%>&nbsp;(<% = ResString("lblOpportunityModel")(0)%>)
                    <%End If%>
                </div>
            </center>
            <% End If %>
            <% = GetPipeButtons() %>
        </td>
    </tr>
    <%End If%>
</table>

<%-- Users --%>
<div id="divUsersAndGroups" style="display:none;position:relative;">
    <table border="0" cellpadding="0" cellspacing="0" style="width:100%;">
        <tr valign="top">
            <td valign="bottom" style="width:450px;"><table id='tableUsers' class='text cell-border hover order-column' style='width:450px;'></table></td>
            <td valign="bottom" style="padding-left:5px;width:300px;"><table id='tableGroups' class='text cell-border hover order-column' style='width:100%;'></table></td>
        </tr>
    </table>

    <div style='text-align:center; margin-top:1ex; width:100%;'>
        <a href="" onclick="return SelectAllUsers(1)" class="actions"><% =ResString("lblSelectAll")%></a> |
        <a href="" onclick="return SelectAllUsers(0)" class="actions"><% =ResString("lblDeselectAll")%></a>
    </div>
</div>

<div id="popupSettings" class="cs-popup"></div>
<div id="popupDollarValueSettings" class="cs-popup"></div>

<%-- Events with Checkboxes dialog --%>
<div id='selectEventsForm' style='display: none; overflow:hidden;'>
    <div id="divSelectEvents" style="padding: 5px; text-align: left; overflow:auto; height:300px;"></div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllEvents(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllEvents(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

</asp:Content>