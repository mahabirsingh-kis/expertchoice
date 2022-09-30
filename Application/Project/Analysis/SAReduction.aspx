<%@ Page Language="VB" Inherits="SAReductionPage" Title="S.A. Reduction" CodeBehind="SAReduction.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script language="javascript" type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="ResultGroupsContent" ContentPlaceHolderID="PageContent" runat="Server">
    <style type="text/css">
        .dx-datagrid-rowsview .dx-data-row .dx-cell-modified .dx-highlight-outline::after {
            border-color: transparent;
        }
    </style>
    <script language="javascript" type="text/javascript">

        var CHAR_CURRENCY_SYMBOL = "<% = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol %>";
        var UNDEFINED_INTEGER_VALUE = <% = UNDEFINED_INTEGER_VALUE %>;

        var columns_ver = "_20210709__";

        var isReadOnly = <% = Bool2JS(App.IsActiveProjectReadOnly) %>;
        var ajaxMethod = _method_POST; // don't use method GET because of caching
        var please_wait_delay = 4000; //ms
        var load_n_at_a_time = 5;

        var ctUndefined = "<%=CInt(ControlType.ctUndefined)%>";
        var ctCause = <%=CInt(ControlType.ctCause)%>;
        var ctConsequenceOld = <%=CInt(ControlType.ctConsequence)%>; // OBSOLETE
        var ctCauseToEvent = <%=CInt(ControlType.ctCauseToEvent)%>;
        var ctConsequenceToEvent = <%=CInt(ControlType.ctConsequenceToEvent)%>;

        var ctUndefinedName = "<%=ControlTypeToString(ControlType.ctUndefined)%>";
        var ctCauseName = "<%=ControlTypeToString(ControlType.ctCause)%>";
        var ctCauseToEventName = "<%=ControlTypeToString(ControlType.ctCauseToEvent)%>";
        var ctConsequenceToEventName = "<%=ControlTypeToString(ControlType.ctConsequenceToEvent)%>";

        var controls = [];
        var loaded_count = 0;
        var is_running = false;
        var cancelled = false;
        var cf; // callback function

        var startTime;
        var endTime;

        /* Toolbar */
        var toolbarItems = [{
                location: 'before',
                widget: 'dxButton',
                locateInMenu: 'auto',
                options: {
                    icon: "fas fa-sync-alt",
                    disabled: true,
                    elementAttr: {id: 'btn_refresh'},
                    text: "Reload S.A. Reductions",
                    onClick: function() {
                        clearSAReductions();                        
                        loadSAReductions();
                    }
                }
            }, {
                location: 'before',
                widget: 'dxButton',
                locateInMenu: 'auto',
                options: {
                    icon: "fas fa-ban",
                    disabled: true,
                    elementAttr: {id: 'btn_cancel'},
                    text: "<% = ResString("btnCancel") %>",
                    onClick: function() {
                        cancelDialog();
                    }
                }
            }, {   
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: true,
                disabled: false,
                options: {
                    showText: true,
                    text: "Simulated Results",
                    value: <% = Bool2JS(PM.CalculationsManager.UseSimulatedValues) %>,
                    elementAttr: { id: 'cbSimulatedSwitch' },
                    hint: "",
                    onValueChanged: function (e) { 
                        callAPI("risk/?action=UseSimulations".toLowerCase(), { "value" : e.value }, function onUseSimulations(data) {
                            if (typeof data == "object") {
                                
                            }
                        }, false, please_wait_delay);
                    }
                }
            }, {
                location: 'right', locateInMenu: 'never',  template: '<span id="divTimeElapsed" style="display: none; padding-right: 10px;"></span>'
            }
        ];

        function updateToolbar() {
            var btn_cancel = $("#btn_cancel").dxButton("instance");
            if ((btn_cancel)) {
                btn_cancel.option("disabled", controls.length == loaded_count || cancelled);
            }            
            var btn_refresh = $("#btn_refresh").dxButton("instance");
            if ((btn_refresh)) {
                btn_refresh.option("disabled", controls.length === 0 || is_running);
            }

            var cbSimulatedSwitch = $("#cbSimulatedSwitch").dxCheckBox("instance");
            if ((cbSimulatedSwitch)) {
                cbSimulatedSwitch.option("disabled", is_running);
            }            
        }

        function initToolbar() {
            $("#toolbar").dxToolbar({
                items: toolbarItems
            });

            $("#toolbar").css("margin-bottom", 0);        
        }

        function cancelDialog() {
            cancelled = true;
            stopProgress();
            updateToolbar();
            DevExpress.ui.notify(resString("msgCancelled"), "error");
        }
        /* end Toolbar */

        /* Datagrid */
        var booleanCustomizeText = function (cellInfo) {
            return cellInfo.value ? "<% = ResString("lblYes") %>" : "";
        }

        function getControlTypeName(ctrlType) {
            var retVal = "";
            switch (ctrlType*1) {
                case ctCause:
                    retVal = ctCauseName;
                    break;
                case ctConsequenceOld:
                    retVal = ctConsequenceToEventName;
                    break;
                case ctCauseToEvent:
                    retVal = ctCauseToEventName;
                    break;
                case ctConsequenceToEvent:
                    retVal = ctConsequenceToEventName;
                    break;
            }
            return retVal;
        }

        function getColumns() {
            var columns = [];
            
            columns.push({ dataField: "index", caption: resString("tblID"), allowReordering: true, allowEditing: false, width: 60, allowGrouping: false });
            columns.push({ dataField: "name", caption: resString("tblControlName"), allowReordering: true, allowEditing: false, allowGrouping: false });
            columns.push({ dataField: "description", caption: resString("tblDescription"), allowReordering: true, allowEditing: false, width: "200px", visible: false, allowGrouping: false });
            columns.push({ dataField: "type", caption: "<% = ParseString("%%Control%% for") %>", allowReordering: true, allowEditing: false, width: 150, alignment: "left", allowGrouping: true, groupIndex: 0,
                customizeText: function(cellInfo) {
                    return getControlTypeName(cellInfo.value);
                }
            });
            columns.push({ dataField: "cost", caption: resString("tblCost"), allowReordering: true, allowEditing: false, width: 100, allowGrouping: false, 
                customizeText: function(cellInfo) {
                    return cellInfo.value === 0 ? "" : cellInfo.valueText;
                },
                format: {
                    "type": "currency"
                }
            });
            columns.push({ dataField: "selected", caption: "Selected", allowReordering: true, allowEditing: false, width: 75, alignment: "center", customizeText: booleanCustomizeText, dataType: "string", allowGrouping: true });
            columns.push({ dataField: "sa", caption: "S.A. Reduction, %", allowReordering: true, allowEditing: false, width: 120, allowGrouping: false, 
                customizeText: function(cellInfo) {
                    return cellInfo.value === UNDEFINED_INTEGER_VALUE ? "" : cellInfo.valueText;
                },
                format: {
                    "type": "percent", 
                    "precision": 2
                }
            });
            columns.push({ dataField: "sa_doll", caption: "S.A. Reduction, " + CHAR_CURRENCY_SYMBOL, allowReordering: true, allowEditing: false, width: 140, allowGrouping: false,
                customizeText: function(cellInfo) {
                    return cellInfo.value === UNDEFINED_INTEGER_VALUE ? "" : cellInfo.valueText;
                },
                format: {
                    "type": "currency"
                }
            });
            
            return columns;
        }

        var dataSource;

        function initDatagrid() {
            if ($("#divDatagrid").data("dxDataGrid")) {
                $("#divDatagrid").dxDataGrid("dispose");
            }

            var columns = getColumns();            

            dataSource = new DevExpress.data.ArrayStore({
                key: 'id',
                data: controls
            });

            $("#divDatagrid").dxDataGrid({
                allowColumnResizing: true,
                allowColumnReordering: false,
                autoNavigateToFocusedRow: false,
                columns: columns,
                columnResizingMode: "widget",
                columnAutoWidth: true,
                columnFixing: {
                    enabled: false
                },
                columnChooser: {                
                    height: function() { return Math.round($(window).height() * 0.8); },
                    mode: "select",
                    enabled: true
                },
                dataSource: dataSource,
                editing: {
                    allowUpdating: false,
                    refreshMode: "repaint"
                },
                "export": {
                    enabled: true,
                    fileName: "<% = SafeFileName(App.ActiveProject.ProjectName) %> - Standalone Risk Reduction"
                },
                grouping: {
                    autoExpandAll: true,
                },
                groupPanel: {
                    visible: true
                },
                highlightChanges: false,
                hoverStateEnabled: true,                
                focusedRowEnabled: true,
                keyExpr: "id",
                loadPanel: {
                    enabled: false
                },
                repaintChangesOnly: true,
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
                    visible: true
                },
                paging: {
                    enabled: true,
                    pageSize: 500
                },
                onInitialized: function (e) {
                    setTimeout(function () { resizePage(); }, 500);
                },
                onRowPrepared: function (e) {  
                    if (is_running && e.rowType !== "header" && e.data.sa !== UNDEFINED_INTEGER_VALUE && (e.cells) && e.cells.length >= 6) {
                        for (var i = 0; i < e.cells.length; i++) {
                            var cell = e.cells[i];
                            if (cell.column.dataField === "sa" || cell.column.dataField === "sa_doll") {
                                $(cell.cellElement).effect("highlight", {}, 800);
                                $(cell.cellElement).effect("highlight", {}, 800);
                            }
                        }
                    }
                },
                scrolling: {
                    mode: "standard"
                },
                showBorders: false,
                noDataText: "<% = GetEmptyMessage() %>",                
                onToolbarPreparing: function (e) {
                    var toolbarItems = e.toolbarOptions.items; 
                    toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: '<h6>Controls Standalone Risk Reductions</h6>' });
                },
                stateStoring: {
                    enabled: true,
                    type: "localStorage",
                    storageKey: "grid_sa_<% = CurrentPageID %>_PRJ_ID_<% = App.ProjectID %>" + columns_ver
                }, 
                wordWrapEnabled: false,
                width: "100%"
            });
        }
        /* end Datagrid */

        /* Progress Bar */
        var progress_inc = 0; // simulate progress bar
        var cur_progress = 0;

        function startProgress() {
            cur_progress = 0;
            setProgress(progress_inc);
            $("#tdProgressBar").show();

            progress_inc = Math.ceil(100 / Math.ceil(controls.length / load_n_at_a_time));
            if (progress_inc <= 0) progress_inc = 10;
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
        }
        /* end Progress Bar */

        function clearSAReductions() {
            var dg = $("#divDatagrid").dxDataGrid("instance");
            dg.beginUpdate();
            //var ds = dg.getDataSource();
            for (var i = 0; i < controls.length; i++) {                
                dataSource.update(controls[i].id, { "sa": UNDEFINED_INTEGER_VALUE });
                dataSource.update(controls[i].id, { "sa_doll": UNDEFINED_INTEGER_VALUE });
            }
            dg.endUpdate();
        }

        function loadSAReductionsByPortions() {
            var ids = "";
            for (var i = loaded_count; (i < controls.length) && (i < loaded_count + load_n_at_a_time); i++) {
                ids += (ids === "" ? "" : ",") + controls[i].id;
            }

            if (ids !== "") {
                is_running = true;
                callAPI("risk/?action=GetControlsRiskReduction".toLowerCase(), { "ids": ids }, function onGetControlsRiskReduction(data) {
                    if (typeof data == "object") {
                        var dg = $("#divDatagrid").dxDataGrid("instance");
                        //var ds = dg.getDataSource();
                        dg.beginUpdate();
                        for (var i = 0; i < data.Data.length; i++) {
                            //var rowIndex = dg.getRowIndexByKey(data.Data[i].id);
                            //dg.cellValue(rowIndex, "sa", data.Data[i].sa);
                            //dg.cellValue(rowIndex, "sa_doll", data.Data[i].sa_doll);
                            
                            dataSource.update(data.Data[i].id, { "sa": data.Data[i].sa });
                            dataSource.update(data.Data[i].id, { "sa_doll": data.Data[i].sa_doll });

                            loaded_count += 1;
                        }
                        
                        is_running = false;
                        
                        var bar = document.getElementById("progress_bar");
                        if (bar.style.display !== "none" && cur_progress < 100) {
                            setProgress(Math.round(cur_progress + progress_inc));
                        }
                        
                        endTime = new Date();
                        var seconds = endTime - startTime;
                        seconds = Math.round(seconds/1000);
                        
                        if (loaded_count < controls.length && !cancelled) {
                            $("#divTimeElapsed").text("Running " + seconds + " s.").show();
                            loadSAReductionsByPortions();
                        } else {
                            $("#divTimeElapsed").text(cancelled ? resString("msgCancelled") : "Calculated in " + seconds + " s.").show();
                            if (!cancelled) DevExpress.ui.notify("Calculated in " + seconds + " s.", "success");
                            if (cancelled && typeof cf === "function") cf();
                            stopProgress();
                        }
                        
                        setTimeout(function () { dg.saveEditData(); }, 1500);
                        dg.endUpdate();

                        updateToolbar();
                    }
                }, false, please_wait_delay);
            }
        }

        /* cancel on leaving the page */
        function onPageUnload(callback_func) {
            if (is_running) {
                cf = callback_func;
                cancelDialog();
            } else {
                if (typeof callback_func === "function") callback_func();
            }
        }

        $(window).on('beforeunload', function() {
            return onPageUnload();
        });

        logout_before = function(callback_func) {
            onPageUnload(callback_func);
        }

        /* Page resize */

        var grid_w_old = 0;
        var grid_h_old = 0;

        function resizeGrid() {
            var margin = 6;
            $("#divDatagrid").height(200).width(300);
            var td = $("#tdContent");
            var w = $("#divDatagrid").width(Math.round(td.innerWidth()) - margin).width();
            var h = $("#divDatagrid").height(Math.round(td.innerHeight()) - margin).height();
            if ((grid_w_old != w || grid_h_old != h)) {
                grid_w_old = w;
                grid_h_old = h;
            };
        }

        function checkResize(w_o, h_o) {
            var w = $(window).width();
            var h = $(window).height();
            if (!w || !h || !w_o || !h_o || (w == w_o && h == h_o)) {
                resizeGrid();
            }
        }

        function resizePage(force_redraw) {
            var w = $(window).innerWidth();
            var h = $(window).innerHeight();
            if (force_redraw) {
                grid_w_old = 0;
                grid_h_old = 0;
            }
            setTimeout("checkResize(" + (force_redraw ? 0 : w) + "," + (force_redraw ? 0 : h) +");", 50);
        }
       
        resize_custom = function (force_redraw) { 
            resizePage(force_redraw);
        };

        $(document).ready(function () {            
            initToolbar();
            loadControlsData();
        });

        function loadControlsData() {
            callAPI("risk/?action=GetControlsList".toLowerCase(), {}, function onLoadControlsList(data) {
                if (typeof data == "object") {
                    controls = data.Data;

                    initDatagrid();
                    updateToolbar();
                    
                    loadSAReductions();
                }
            }, false, please_wait_delay);
        }                

        function loadSAReductions() {
            $("#divTimeElapsed").hide();

            loaded_count = 0;
            cancelled = false;
            startTime = new Date();
            startProgress();
            loadSAReductionsByPortions();
        }
        
    </script>

    <table border='0' cellspacing="0" cellpadding="0" class="whole">
        <tr id="trProgressBar" valign="top" style="height: 8px;">
            <td id="tdProgressBar" style="display: none; vertical-align: top; padding: 2px 0px;" valign="top">
                <div id="progress_control" style="overflow: hidden; background-color: #d0d0d0; height: 4px; width: 100%; text-align: left;">
                    <div id="progress_bar" style="background-color: #8899cc; height: 4px; width: 100%; margin: 0px;"></div>
                </div>
            </td>
        </tr>
        <tr valign="top">
            <td valign="top">
                <div id="toolbar" style="background-color: transparent; padding-top: 5px;"></div>
            </td>
        </tr>
        <tr valign="top">
            <td id="tdContent" valign="top" style="overflow: hidden; text-align: left;" class="whole">
                <div id="divDatagrid" style="width: 100%; margin-top: 5px;"></div>
            </td>
        </tr>
    </table>

</asp:Content>