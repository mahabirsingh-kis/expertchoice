<%@ Page Language="VB" Inherits="EvaluatorsPage"  title="Evaluation Progress" Codebehind="Evaluators.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">
    
    var cmd = "";
    var sel_group_id = <%=EvaluationGroupID()%>;
    var cur_link  = "";
    var cur_oper  = "";

    var users_data  = <%=GetEvaluationProgressData()%>;
    var store;

    var IDX_ID = "id";
    var IDX_NAME = "name";
    var IDX_EMAIL = "email";
    var IDX_PROGRESS = "progress";
    var IDX_PROGRESS_PLAIN = "progress_plain";
    var IDX_PERCENT_EVAL = "percent_eval";
    var IDX_EVALUATED = "evaluated";
    var IDX_TOTAL = "total";
    var IDX_LAST_JUDGMENT = "last_judg";
    var IDX_ACTIONS = "actions";
    var IDX_IS_ONLINE = "is_online";
    var IDX_IS_DISABLED = "is_disabled";
    var IDX_CAN_EDIT_PROJECT = "can_edit";
    var IDX_CAN_EVALUATE = "can_eval";
    var IDX_IS_LINK_ENABLED = "link_enabled";
    var IDX_IS_LINKGO_ENABLED = "linkgo_enabled";
    var IDX_IS_PREVIEW_ENABLED = "preview_enabled";

    var COL_NAME = 0;
    var COL_EMAIL = 1;
    var COL_EVAL_PROGRESS = 2;
    var COL_LAST_JUDGMENT = 3;
    var COL_ACTIONS = 4;

    var isOnline = <%=Bool2JS(App.ActiveProject.isOnline)%>;
    var isReadOnly = <%=Bool2JS(App.IsActiveProjectStructureReadOnly)%>;

    function updateOptions() {
        var rb0 = document.getElementById("rbOffline");
        var rb1 = document.getElementById("rbOnline");
        if ((rb0) && (rb1)) {
            rb0.checked = !isOnline;
            rb1.checked = isOnline;
        }
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }
    
    function sendCommand(params) {
        cmd = params;
        displayLoadingPanel(true);

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function syncReceived(params) {
        displayLoadingPanel(false);
        if ((params) && (isAction("update", cmd) || isAction("filter_by_group", cmd))) {    
            var arr = eval(params);
            if (arr.length > 5 && (arr[5] == "update" || arr[5] == "filter_by_group")) {
                users_data = arr[0];
                var graphBar = arr[1];
                var graphBarHint = arr[2];
                var isTT = arr[3];
                var lblHeader = arr[4];
                $("#tdOverallGraphBar").html(graphBar);
                $("#lblGlobalProgress").html(graphBarHint);
                $("#lblListOfEvaluators").html(lblHeader);
                if (isTT) {$("#<%=barWarning.ClientID%>").show()} else {$("#<%=barWarning.ClientID%>").hide()};
                refreshGrid();
            }
        }

        if ((params) && isAction("stop_tt", cmd)) {    
            var val = eval(params);
            if (!(val)) reloadPage(0);
        }

        if ((params) && (isAction("copy", cmd) || isAction("open", cmd) || isAction("view", cmd))) {    
            if (isAction("copy", cmd)) cur_oper = "copy";
            if (isAction("open", cmd) || isAction("view", cmd)) cur_oper = "open";            
            
            var res = eval(params);

            cur_link = res[2];
            var is_resp = typeof res[3] != "undefined" && (res[3]);
            
            switch (res[0]) {
                case "proceed":
                    ProceedLinkAction(cur_link, is_resp);
                    break;
                case "error":
                    var linkDialog = DevExpress.ui.dialog.custom({
                        title: resString("lblWarning"),
                        messageHtml: res[1],
                        buttons: [{ text: "<% =JS_SafeString(ResString("btnYes"))%>", onClick : function () { return true; }}, {text: "<% =JS_SafeString(ResString("btnNo")) %>", onClick : function () { return false; }}]
                    });
                    linkDialog.show().done(function(dialogResult){
                        if (dialogResult) {
                            if (cur_oper == "copy") copyDataToClipboard(cur_link);
                        }
                    });
                    break;
                case "confirm":
                    var linkDialog = DevExpress.ui.dialog.custom({
                        title: resString("lblWarning"),
                        messageHtml: replaceString("\n", "<br>", res[1]),
                        buttons: [{ text: "<% =JS_SafeString(ResString("btnSetPrjOnline"))%>", onClick : function () { return true; }}, {text: "<% =JS_SafeString(ResString("btnLeavePrjOffline")) %>", onClick : function () { return false; }}]
                    });
                    linkDialog.show().done(function(dialogResult){
                        if (dialogResult) {
                            sendCommand('action=make_online');
                        } else {
                            if (!isAction("open", cmd) && ("open" != cur_oper)) ProceedLinkAction(cur_link, is_resp);
                        }
                    });
                    break;
            }
        }

        if ((params) && (isAction("make_online", cmd))) {    
            if (params === 'true') {
                ProceedLinkAction(cur_link, false);
            }
        }

        if ((params) && (isAction("set_status", cmd))) {
            var res = eval(params);
            var success = (res[0] + "").toLowerCase() == "ok";
            if (success) {
                isOnline = res[1];
            } else {
                updateOptions();
            }
            if (typeof onChangeActiveProject == "function") {
                if (typeof curPrjData != "undefined") curPrjData.isOnline = isOnline;
                if (typeof updateProjectActions == "function") updateProjectActions();
                onChangeActiveProject();
            }
            if ((typeof res[2] != "undefined") && res[2]!="") {
                DevExpress.ui.notify(res[2], "error", 5000);
            }
        }

        cmd = "";
    }

    function ProceedLinkAction(link, is_resp) {
        if (link!="") {
            if (cur_oper === "copy") copyDataToClipboard(link);
            if (cur_oper === "open") OpenLinkWithReturnUser(link, <%=App.ProjectID%>, <%=PM.ActiveHierarchy%>, "<%=JS_SafeString(If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, App.ActiveProject.PasscodeImpact, App.ActiveProject.PasscodeLikelihood))%>", is_resp);
        } else {
            dxDialog("Link is not available", false, ";");
        }
    }

    function onCreateGridToolbar(e) {
        var toolbarItems = e.toolbarOptions.items;

        toolbarItems.splice(1, 0, {
            widget: 'dxButton', 
            options: { icon: "fas fa-sync-alt", text: "<%=ResString("btnProgressUpdate")%>", hint: "<%=ResString("btnProgressUpdate")%>", onClick: function() { sendCommand("action=update&id=" + sel_group_id); } },
            locateInMenu: "never",
            //showText: "inMenu",
            location: 'after'
        });
    }

    function loadDataTable() {       
        var columns = [];
            
        //init columns headers                
        columns.push({ "caption" : "<%=ResString("tblSyncUserName")%>", "alignment" : "left", "allowSorting" : true, "dataField" : IDX_NAME, "allowSearch" : true });
        columns.push({ "caption" : "<%=ResString("tblEmailAddress")%>", "alignment" : "left", "allowSorting" : true, "dataField" : IDX_EMAIL, "allowSearch" : true });
        columns.push({ "caption" : "<%=ResString("tblEvaluationProgress")%>", "alignment" : "left", "width" : "300px", "allowSorting" : true, "sortOrder" : "desc", "dataField" : IDX_PERCENT_EVAL, "allowSearch" : false,
            cellTemplate : function (cellElement, cellInfo) {
                var data = cellInfo.data;
                cellElement.append(data[IDX_PROGRESS]);
                if (!data[IDX_CAN_EVALUATE] || data[IDX_IS_DISABLED]) { 
                    $(cellElement).find('.graph_progress').removeClass("graph_progress").addClass("graph_progress_disabled");  
                };
            } 
        });
        columns.push({ "caption" : "<%=ResString("tblLastJudgmentTime")%>", "alignment" : "left", "width" : "200px", "allowSorting" : true, "dataField" : IDX_LAST_JUDGMENT, "dataType": "date", "allowSearch" : false, format:'shortDateShortTime' });
        columns.push({ "caption" : "<%=ResString("tblActions")%>", "alignment" : "center", "width" : "120px", "allowSorting" : false, "allowSearch" : false, "allowExporting" : false, 
            cellTemplate : function (cellElement, cellInfo) {
                var data = cellInfo.data;
                if (!isReadOnly && (data[IDX_IS_LINK_ENABLED] || data[IDX_IS_PREVIEW_ENABLED])) {
                    var clr = <% =If(App.isEvalURL_EvalSite, "((data[IDX_IS_PREVIEW_ENABLED]) ? ' style=\""color:#38761d\""' : '')", "''") %>;
                    <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %>cellElement.append(data[IDX_IS_LINK_ENABLED] ? "<a href='' onclick='getUserLink(\""+data[IDX_EMAIL]+"\",\"copy\"); return false;' style='margin:0px 2px;' title='<%=SafeFormString(ResString("btnGetAnytimeEvalLink"))%>'><i class='fas fa-link'" + clr + "></i></a>" : "<i class='fas fa-link' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");
                    cellElement.append(data[IDX_IS_LINKGO_ENABLED] ? "<a href='' onclick='getUserLink(\""+data[IDX_EMAIL]+"\",\"open\"); return false;' style='margin:0px 2px;' title='<%=SafeFormString(ResString("btnGetAndOpenAnytimeEvalLink"))%>'><i class='fas fa-sign-in-alt'" + clr + "></i></a>" : "<i class='fas fa-sign-in-alt' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");<% End If %>
                    cellElement.append(data[IDX_IS_PREVIEW_ENABLED] ? "<a href='' onclick='" + (data[IDX_IS_LINKGO_ENABLED] ? "getPreview" : "getPreviewCore") + "("+data[IDX_ID]+",\""+data[IDX_EMAIL]+"\"); return false;' style='margin:0px 2px;' title='<%=SafeFormString(ResString("lblEvalProgressHintUser"))%>'><i class='fas fa-eye'></i></a>" : "<i class='fas fa-eye' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");
                }
            } 
        });

        store = new DevExpress.data.ArrayStore({
            key: IDX_ID,
            data: users_data
        });
        
        var table = (($("#tableEvaluationProgressData").data("dxDataGrid"))) ? $("#tableEvaluationProgressData").dxDataGrid("instance") : null;
        if (table !== null) { table.dispose() };

        $('#tableEvaluationProgressData').dxDataGrid( {
            allowColumnResizing : true,
            allowColumnReordering: true,
            dataSource: store, //: users_data,            
            columns: columns,
            columnResizingMode: 'widget',
            columnChooser: {
                mode: "select",
                enabled: true
            },  
            customizeExportData: function (columns, rows) {
                rows.forEach(function (row) {
                    var rowValues = row.values;
                    if (rowValues.length > 2) rowValues[2] = row.data[IDX_PROGRESS_PLAIN];
                })
            },          
            pager: {
                allowedPageSizes: [10, 15, 20, 50, 100, 200],
                showInfo: true,
                showNavigationButtons: true,
                showPageSizeSelector: true,
                visible: true
            },
            paging: {
                enabled: true,
                pageSize: 50
            },
            scrolling: {
                mode: "standard",
            },
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            hoverStateEnabled: true,
            "export": {
                enabled: true
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "EvalStatus_" + "_PRJID_<%=App.ProjectID%>"
            },
            sorting: {
                mode: "multiple"
            },
            //onCellPrepared: function(e) {
            //    if (e.rowType === "header" && e.column.dataField !== IDX_NAME && e.column.dataField !== IDX_EMAIL) {
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
                setTimeout(function () { onResize(true); }, 400);
            },
            onToolbarPreparing: onCreateGridToolbar,
            onRowPrepared: function (e) {
                if (e.rowType != "data") return;
                var row = e.rowElement;
                var data = e.data;
                $('td', row).css({ "padding" : "2px", "font-size" : "9pt" });
                if (data[IDX_IS_ONLINE]) { $('td', row).css({'color':'#009933'}); }
                if (data[IDX_CAN_EDIT_PROJECT] || data[IDX_IS_ONLINE]) { $('td', row).css({'font-weight':'bold'}); }                
            }
        });        
    }

    function refreshGrid() {
        $("#tableEvaluationProgressData").dxDataGrid("instance").option("dataSource", users_data);
        $("#tableEvaluationProgressData").dxDataGrid("refresh"); 
    }

    function getUserLink(email, action) {        
        sendCommand("action=" + action + "&email=" + email);
    }
    
    function getPreview(id, email) {
        showLoadingPanel();
        <% If App.isEvalURL_EvalSite AndAlso Not App.isRiskEnabled Then %>getUserLink(email, "view");<% Else %>document.location.href = "<% =JS_SafeString(PageURL(If(IsEvaluationProgressForTreatments, _PGID_EVALUATE_RISK_CONTROLS, _PGID_EVALUATION), "readonly=yes&id=")) %>" + id;<% End If %>
    }

    function getPreviewCore(id, email) {
        openProjectURLWhenGecko("<% = JS_SafeString(PageURL(If(IsEvaluationProgressForTreatments, _PGID_EVALUATE_RISK_CONTROLS, _PGID_EVALUATION), "readonly=yes&id=", False)) %>" +id);
    }

    function onChangeActiveProject() {
        if (typeof curPrjData != "undefined") {
            isOnline = curPrjData.isOnline;
            updateOptions();
        }
    }

    function stopTT() {
        var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("confTTStop")) %>", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
        result.done(function (dialogResult) {
            if (dialogResult) {
                onStopTT();
            }
        });
    }

    function onStopTT() {
        sendCommand("<% =_PARAM_ACTION %>=stop_tt");
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid() {
        var margin = 8;
        $("#tableEvaluationProgressData").height(200).width(400);
        var td = $("#tdContent");
        var w = $("#tableEvaluationProgressData").width(Math.round(td.innerWidth())-margin).width();
        var h = $("#tableEvaluationProgressData").height(Math.round(td.innerHeight())-margin).height();
        if ((grid_w_old!=w || grid_h_old!=h)) {
            grid_w_old = w;
            grid_h_old = h;
        };
    }

    function checkResize(w_o, h_o) {
        var w = $(window).width();
        var h = $(window).height();
        if (!w || !h || !w_o || !h_o || (w==w_o && h==h_o)) {
            resizeGrid();
        }
    }
    
    function onResize(force_redraw) {
        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        setTimeout("checkResize(" + (force_redraw ? 0 : w) + "," + (force_redraw ? 0 : h) +");", 100);
        //checkResize(0,0);
    }

    resize_custom = function (force_redraw) { onResize(force_redraw); };
    
    $(document).ready( function () {
        loadDataTable();
    });
    
</script>

<table cellpadding="0" cellspacing="0" border="0" style="width:100%; height:100%; overflow: hidden;">
<tr valign="top" align="center">
    <td class='text'  id="tdHeader" valign="top" style="padding-top: 10px;">
        <h5 id="lblPageTitle" style="padding: 0; margin: 0;"><%= String.Format(ResString(If(IsEvaluationProgressForTreatments, "lblEvalProgressControls", "lblEvalProgress")), ShortString(App.ActiveProject.ProjectName, 70)) %></h5>
        
        <table border="0" cellpadding="0" cellspacing="0">
            <%If Not PM.IsRiskProject Then%>
            <tr>
                <td colspan="3">
                    <div style="margin-top:1em;"><b><% = ResString("titleProjectStatus") %>:</b></div>
                    <div style="margin:0px 0px 1ex">
                        <div style="text-align:left;">
                            <input type='radio' class='radio' id='rbOnline'  name='radioStatus' value='1' onclick='sendCommand("action=set_status&online=" + this.value);' <%=If(CanChange, "", " disabled='disabled' ")%> <%=If(App.ActiveProject.isOnline, "checked='checked'", "")%>><label for="rbOnline"><%=ResString("optProjectOnline")%></label><br />
                            <input type='radio' class='radio' id='rbOffline' name='radioStatus' value='0' onclick='sendCommand("action=set_status&online=" + this.value);' <%=If(CanChange, "", " disabled='disabled' ")%> <%=If(Not App.ActiveProject.isOnline, "checked='checked'", "")%><label for="rbOffline"><%=ResString("optProjectOffline")%></label>
                        </div>
                    </div>
                    <% = GetMessage() %>
                </td>
            </tr>
            <%End If%>
            <tr>
                <td><p id="lblGlobalProgressTitle" class="header_sl" style="display:inline-block; margin: 4px 8px 4px 8px;"><%=IIf(App.isRiskEnabled, IIf(CurrentPageID <> _PGID_CONTROLS_EVAL_PROGRESS, IIf(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ResString("lblOverallEvalProgressLikelihood"), ResString("lblOverallEvalProgressImpact")), ParseString("%%Controls%%")), ResString("lblOverallEvalProgress"))%></p>
                <div id="tdOverallGraphBar" style="display:inline-block;"><%=GetOverallGraphBar()%></div><div id="lblGlobalProgress" class="text" style="display:inline-block;">&nbsp;<%=CStr(OverallProgress)%>&#37;</div></td>
            </tr>
            <tr>
                <td colspan="3" align="center">
                    <div id="lblUsersTitle" class="header_sl" style="text-align:center; margin-top: 0;">
                        <span id="lblListOfEvaluators"><%=String.Format(ResString("lblListOfEvaluators"), PM.CombinedGroups.GetCombinedGroupByUserID(EvaluationGroupID).UsersList.Count)%></span>
                        <%If PM.CombinedGroups.GroupsList.Count > 1 Then%><span>&ndash;&nbsp;<% =ResString("lblGroup") %>&nbsp;</span><span class='text'><%=GetEvaluationGroups()%></span><%End If%>
                    </div>                                        
                </td>
            </tr>
        </table>

        <div id="barWarning" runat="server" class="warning" visible="false" style="text-align:center;">
            <p id="lblWarning"><%=SafeFormString(ResString("msgViewPipeTT"))%>&nbsp; <input type="button" class="button" value="<% =SafeFormString(ResString("btnTTStopSession")) %>" id="btnTTStop" onclick="stopTT(); return false;" /></p>                 
        </div>
    </td>
</tr>
<tr valign="top" align="center" style="height: 100%; width: 100%;">
    <td class='whole' id='tdContent' valign="top" style="min-width: 450px;">
        <div id='tableEvaluationProgressData' class='whole'></div>
    </td>
</tr>
</table>

</asp:Content>