<%@ Page Language="VB" Inherits="ChartsPage" title="Charts" Codebehind="Charts.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/drawMisc.js"></script>
    <script type="text/javascript" src="/Scripts/canvg.min.js"></script>
    <script type="text/javascript" src="/Scripts/ec.charts.js"></script>
    <script type="text/javascript" src="/Scripts/download.js"></script>
    <script type="text/javascript" src="/Scripts/jspdf.min.js"></script>
    <script type="text/javascript" src="/scripts/datatables_only.min.js"></script>
    <script type="text/javascript" src="/scripts/datatables.extra.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
    #toolbar {
        margin-left: 0px;
        padding-left: 8px;
        background: #ffffff;
        border: 1px solid #cccccc;
        margin-top: -1px;
        margin-bottom: 0px;
    }
    /*#content {
        border: 1px solid #cccccc;
        border-top: 0px;
    }*/

</style>
<script type="text/javascript">

    loadSVGLib();

    var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
    
    //{"synthmode": synthMode, "combinedmode": combinedMode, "usecis": useCIS, "useprty": usePriorities, "decimals": DecimalDigits, "normmode": normalizationMode};

    var synthMode = <%= CheckVar("synthmode", CInt(App.ActiveProject.ProjectManager.CalculationsManager.SynthesisMode)) %>;
    var combinedMode = <%= CheckVar("combinedmode", CInt(App.ActiveProject.ProjectManager.PipeParameters.CombinedMode)) %>;
    var useSimulated = <% = Bool2JS(PM.Parameters.Riskion_Use_Simulated_Values <> 0) %>;
    var useCIS = <%= Checkvar("usecis", CInt(App.ActiveProject.ProjectManager.PipeParameters.UseCISForIndividuals)) %>;
    var usePriorities = <%= CheckVar("useprty", CInt(App.ActiveProject.ProjectManager.PipeParameters.UseWeights)) %>;
    var includeIdealAlt = <% = Bool2JS(App.ActiveProject.ProjectManager.PipeParameters.IncludeIdealAlternative) %>;
    var DecimalDigits  = <% = CheckVar("decimals", CInt(PM.Parameters.DecimalDigits)) %>;
    var normalizationMode = "<% = CheckVar("normmode", NormalizeModeString) %>";
    var GridWRTNodeID = <% = Api.WRTNodeID %>;
    var GridWRTNodeIsTerminal = <% = Bool2JS(Api.GridWRTNodeIsTerminal) %>;
    var GridWRTNodePID = <% = Api.GridWRTNodePID %>;
    var WRTNodeParentGUID = "<% = Api.WRTNodeParentGUID%>";

    var total_users_and_groups_count = <%=PM.CombinedGroups.GroupsList.Count + PM.UsersList.Count%>;

    var tSplitterSize = sessionStorage.getItem("SynthesisSplitterSize<%=App.ProjectID%>");
    var treeSplitterSize = typeof tSplitterSize != "undefined" && tSplitterSize != null && tSplitterSize != "" ? tSplitterSize*1 : 200;

    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;
    var cmAIJ = <%=CInt(CombinedCalculationsMode.cmAIJ)%>;
    var cmAIP = <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;
    
    var users_data  = []; <%--<% = GetUsersData()%>;--%>
    var groups_data = []; <%--<% = GetGroupsData()%>;--%>

    var IDX_USER_CHECKED = 0;
    var IDX_USER_ID      = 1;
    var IDX_USER_NAME    = 2;
    var IDX_USER_EMAIL   = 3;
    var IDX_USER_HAS_DATA= 4;
    var IDX_GROUP_PARTICIPANTS = 5;
    var IDX_GROUP_NAME_EXTRA = 6;
    var IDX_GROUP_NAME_HINT = 7;

    var options = <% =CheckVar("options", "undefined") %>;
    var synthesize_data, objectives;

    var needReloadData = false;

    var initChartCompleted = false;
    var is_manual = false;

    var ACTION_ALTS_FILTER = "alts_filter";
    var AlternativesFilterValue  = <%=AlternativesFilterValue%>;
    var AlternativesAdvancedFilterUserID = <%=AlternativesAdvancedFilterUserID%>;
    var AlternativesAdvancedFilterValue  = <%=AlternativesAdvancedFilterValue%>;
    var old_filter_val = AlternativesFilterValue;
    var showLikelihoodsGivenSources = <% = Bool2JS(PM.Parameters.ShowLikelihoodsGivenSources)%>;
    var showHierarchyTree = <%=Bool2JS(PM.Parameters.Synthesis_ObjectivesVisibility)%>;
    var showTreePrioritiesL = <%=Bool2JS(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 2 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4)%>;
    var showTreePrioritiesG = <%=Bool2JS(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 3 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4)%>;

    /* Filtering by alt attributes vars area */
    <% = LoadAttribData() %>
    var aidx_id = 0;
    var aidx_name = 1;
    var aidx_type = 2;
    var aidx_vals = 3;
    
    var fidx_chk = 0;
    var fidx_id = 1;
    var fidx_oper = 2;
    var fidx_val = 3;

    var avtString = 0;
    var avtBoolean = 1;
    var avtLong = 2;
    var avtDouble = 3;
    var avtEnumeration = 4;
    var avtEnumerationMulti = 5;

    var flt_max_id = -1;
    var oper_list = ["Contains", "Equal", "Not Equal", "Starts With", "Greater Than", "Greater Than Or Equal", "Less Than", "Less Than Or Equal", "<%=ResString("lblYes")%>", "<%=ResString("lblNo")%>"];   
    var oper_available = [[0,1,2,3],
                          [8,9],
                          [1,2,4,5,6,7],
                          [1,2,4,5,6,7],
                          [1,2],
                          [0,1,2]];

    onSwitchAdvancedMode = function () { 
        setTimeout(function () {updateToolbar();}, 200);
    };

    <%--function onSetPage(pgid) {
        var tab = -1;
        switch (pgid % _pgid_max_mod) {
            case <% =_PGID_ANALYSIS_CHARTS_ALTS %>:
                tab = 0;
                break;
            case <% =_PGID_ANALYSIS_CHARTS_OBJS %>:
                tab = 1;
                break
        }
        if (tab>=0) {
            is_manual = true;
            if ($("#tabs").data("dxTabPanel")) $("#tabs").dxTabPanel("instance").option("selectedIndex", tab);
            is_manual = false;
            return true;
        }
        return false;
    }--%>

    /* Toolbar */

    function updateToolbar() {
        initToolbar();
        updateToolbarFlt();
        $("#toolbar").dxToolbar("instance").option("disabled", !hasData());
    }

    function updateInfobar() {
        var showAlts = getOption("showAlternatives");
        var lblWrt = document.getElementById("lblWrtNode");
        if ((lblWrt)) {
            var sNodeName = "";
            lblWrt.innerHTML = "";
            var wrtnodeid = (showAlts ? options.WRTNodeID : options.selectedNodeID);
            //if (objectives[0].id != wrtnodeid && wrtnodeid != -1) {
            var wrt_node;
            for (var i = 0; i < objectives.length; i++) {
                var obj = objectives[i];
                if (obj.id == (typeof wrtnodeid == "undefined" || wrtnodeid == -1 ? objectives[0].id : wrtnodeid)) { 
                    sNodeName = obj.name;
                    wrt_node = obj;
                };
            }
            if ((wrt_node) && wrt_node.id == objectives[0].id) {
                <%--lblWrt.innerHTML = "<%If PM.IsRiskProject Then%><%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%Likelihoods%%"), ParseString("%%Impacts%%"))%><%Else%><%End If%>";--%>
            } else {
                if (showLikelihoodsGivenSources) {
                    lblWrt.innerHTML = (showAlts ? "<%If PM.IsRiskProject Then%><%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%Likelihoods%% Due To"), ParseString("%%Alternative%% %%Impact%% On"))%><%Else%>with respect to<%End If%>" : "for nodes below") +  " " + sNodeName;
                } else {
                    lblWrt.innerHTML = (showAlts ? "<%If PM.IsRiskProject Then%><%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("Vulnerabilites Of %%Alternatives%% To"), ParseString("%%Alternative%% Consequences To"))%><%Else%>with respect to<%End If%>" : "for nodes below") +  " " + sNodeName;
                }
            }
            //}
        }
        
        //var lblUserNames = document.getElementById("lblTitleUserNames");
        //if ((lblUserNames)) {                
        //    var sUserNames = "";
        //    for (var i = 0; i < users_data.length; i++) {
        //        if (users_data[i][IDX_USER_CHECKED] > 0) {
        //            sUserNames += (sUserNames == "" ? "" : ", ") + users_data[i][IDX_USER_NAME];
        //        }
        //    }
        //    for (var i = 0; i < groups_data.length; i++) {
        //        if (groups_data[i][IDX_USER_CHECKED] > 0) {
        //            sUserNames += (sUserNames == "" ? "" : ", ") + groups_data[i][IDX_USER_NAME];
        //        }
        //    }
        //    lblUserNames.innerHTML = sUserNames == "" ? "" : " for " + sUserNames;
        //}

        var cb = $("#selNormalized").dxSelectBox("instance");
        if (typeof cb !== "undefined") {
            cb.option("items[0].disabled", synthMode == 0);
            if (synthMode == 0 && normalizationMode == "none") {
                normalizationMode = "normAll";
                cb.beginUpdate();
                cb.option("value", normalizationMode);
                cb.endUpdate();
            }
        }
        //cb.option("items[2].disabled", AlternativesFilterValue == -1);
    }

    var hideToolbarLabels = false;

    function getOptionProps(option) {
        var retVal = {text:"", icon:"", hint:""};
        var optVal = getOption(option);
        switch (option) {
            case "singleRow":             
                retVal.icon = (optVal ? "icon ec-multi-row" : "icon ec-single-row");
                retVal.text = (optVal ? "Grid View" : "Single Row View");
                retVal.hint = (optVal ? "Arrange charts in grid view (Multi rows/columns)" : "Arrange charts in single row/column");
                break;
            case "isRotated":
                retVal.icon = (optVal ? "icon ec-chart-rotate-ccw" : "icon ec-chart-rotate-cw");
                retVal.text = (optVal ? "Rotate CCW" : "Rotate CW");
                retVal.hint = (optVal ? "Rotate charts 90 degree CCW" : "Rotate charts 90 degree CW");
                break;
            case "showLabels":
                retVal.icon = "icon ec-chart-labels";
                retVal.text = (optVal ? "Hide Labels" : "Show Labels");
                retVal.hint = (optVal ? "Hide Labels" : "Show Labels");
                break;
            case "showLegend":
                retVal.icon = "icon ec-chart-legend";
                retVal.text = (optVal ? "Hide Legend" : "Show Legend");       
                retVal.hint = (optVal ? "Hide Legend" : "Show Legend");
                break;
            case "showLocal":
                retVal.icon = (!optVal ? "fas fa-map-marker-alt" : "fas fa-globe");
                retVal.text = (!optVal ? "Local Priorities" : "Global Priorities");
                retVal.hint = (!optVal ? "Local Priorities" : "Global Priorities");
                break;
            case "showComponents":
                retVal.icon = (optVal ? "icon ec-chart-columns" : "icon ec-chart-components");
                retVal.text = (optVal ? "Hide Components" : "Show Components");
                retVal.hint = (optVal ? "Hide Components" : "Show Components");
                break;
            case "groupByUsers":
                retVal.icon = (optVal ? "icon ec-group-nodes" : "icon ec-group-users");
                retVal.text = (optVal ? "Group by nodes" : "Group by users");
                retVal.hint = (optVal ? "Group charts by nodes" : "Group charts by users");
                break;
        };
        if (hideToolbarLabels) retVal.text = "";
        return retVal;
    }

    var syncTreeViewSelection = function(treeView, value){
        if (!value) {
            treeView.unselectAll();
        } else {
            treeView.selectItem(value);
        }
    }

    var getSelectedItemsKeys = function(items) {
        var result = [];
        items.forEach(function(item) {
            if(item.selected) {
                result.push(item.key);
            }
            if(item.items.length) {
                result = result.concat(getSelectedItemsKeys(item.items));
            }
        });
        return result;
    }

    /* Users and Groups */
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

    function showSelectUsersDialog() {
        if (users_data.length == 0 || groups_data.length == 0) {
            callAPI("pm/dashboard/?action=get_users_and_groups", {}, function (data) {
                users_data = data.users_data;
                groups_data = data.groups_data;
                selectUsersDialog();
            }, false, 2000);
        } else {
            selectUsersDialog();
        }
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
            var userIDs = [];
            for (var i = 0; i < users_data.length; i++) {                
                if (users_data[i][IDX_USER_CHECKED] > 0) { 
                    var uid = users_data[i][IDX_USER_ID];
                    params += (params == "" ? "" : ",") + uid; 
                    userIDs.push(uid);
                    selectedTotal += 1; } 
            };
            for (var i = 0; i < groups_data.length; i++) {                
                if (groups_data[i][IDX_USER_CHECKED] > 0) { 
                    var uid = groups_data[i][IDX_USER_ID];
                    params += (params == "" ? "" : ",") + uid; 
                    userIDs.push(uid);
                    selectedTotal += 1; } 
            };      
            if (selectedTotal == 0) { 
                getUserByID(<%=COMBINED_USER_ID%>)[IDX_USER_CHECKED] = 1;
                selectedTotal = 1;
                params = "<%=COMBINED_USER_ID%>";
                userIDs = [-1];
            };
            var cPP = getOption("chartsPerPage");
            if (selectedTotal > 10) { selectedTotal = 10; };
            if (cPP !== selectedTotal) {
                setOption("chartsPerPage", selectedTotal);
            };
            $("#chart").ecChart("option", "userIDs", userIDs);
            updateToolbar();
            callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=selected_users", { "name": "ids", "value" : params }, function () {
                loadChartData();
            }, false);
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
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' " + (!(data[IDX_USER_HAS_DATA] == 1 || useCIS), " disabled='disabled' ", "") + " "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" onclick='updateUsersWithDataInGroupCheckState();'>");
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
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+">");
                $("td:eq(1)", row).html((data[IDX_USER_ID] != -1 ? "<img id='imgGrp" + data[IDX_USER_ID] + "' src='<% =ImagePath%>old/plus.gif' width='9' height='9' border='0' style='margin-top:2px; margin-right:4px; cursor:pointer;' onclick='expandGroupUsers(" + data[IDX_USER_ID] + ");'>" : "") + htmlEscape(data[IDX_USER_NAME]) + "<div id='divExpandedUsers" + data[IDX_USER_ID] + "'></div>");
                $("td:eq(2)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : "<%=ResString("lblNo")%>"));
                $("td:eq(3)", row).html("<input type='checkbox' class='group_all_users_cb' uid='"+data[IDX_USER_ID]+"' " + (allGroupUsersWithDataChecked(data[IDX_USER_ID]) ? " checked ":"")+" onclick='checkUsersWithDataInGroup(\"" + data[IDX_USER_ID] + "\", this.checked);' " + (data[IDX_GROUP_PARTICIPANTS].length == 0 ? " disabled='disabled' " : "") + ">");
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

    var getNodeByLevel = function(node, level) {  
        if(!node.parent) {  
            return;  
        }  
  
        if (node.parent.level === level || node.parent.level === undefined) { // Remove the "|| node.parent.level === undefined" part after release  
            return node.parent;  
        } else {  
            return getNodeByLevel(node.parent, level);  
        }  
    }  

    var selectedButtonKeys = [<%=If(PM.Parameters.Synthesis_ObjectivesVisibility, "1,", "")%><%=If(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 2 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4, "2,", "")%><%=If(PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 3 OrElse PM.Parameters.Synthesis_ObjectivesPrioritiesVisibility = 4, "3", "")%>];

    function initToolbar() {
        if (!(objectives)) return false;
        var cv = $("#chart").ecChart("chartView");
        var sl = getOption("showLegend");
        var afs = getOption("adjustFontSize");
        var cpp = getOption("chartsPerPage");
        var showAlts = getOption("showAlternatives");
        //if (showAlts && !isAdvancedMode) { setOption("chartType", "altColumns") };
        var collapsedNodes = JSON.parse(localStorage.getItem('chartCollapsedNodes' + curPrjID));
        var expandedNodes = [];
        for (var i = 0; i < objectives.length; i++) {
            var obj = objectives[i];
            if (!(collapsedNodes !== null && collapsedNodes.indexOf(obj.key_id) >= 0)) {expandedNodes.push(obj.key_id)};
            obj.disabled = (!showAlts && obj.isTerminal);
        };
        if (collapsedNodes == null) {
            expandedNodes = null
        };
        
        var sel_id = showAlts ? options.WRTNodeID : options.selectedNodeID; //GridWRTNodeID; //(!showAlts && typeof options.selectedNodeID != "undefined" ? options.selectedNodeID : options.WRTNodeID);
        if (typeof sel_id == "undefined" || sel_id<0 && objectives.length) sel_id = objectives[0].id;

        var selectedObj;

        for (var i = 0; i < objectives.length; i++ ) {
            var obj = objectives[i];
            obj.tid = obj.id + 1;
            if (typeof obj.pid == "undefined") 
            {obj.tpid = 0} else 
            {obj.tpid = obj.pid + 1};

            if (obj.id == sel_id) {
                sel_id = obj.key_id;
                selectedObj = obj;
            }
        };

        initTitle(options.showAlternatives, selectedObj);

        var cellTemplateFunc = function(element, info) {
            var data = info.data[info.column.dataField];
            if (typeof data !== "undefined") {
                switch (data) {
                    case UNDEFINED_INTEGER_VALUE:
                        element.html(lblNA);
                        break;
                    case UNDEFINED_INTEGER_VALUE + 1:
                        element.html("");
                        break;
                    default:
                        var percent = roundTo(data * 100, DecimalDigits);
                        element.html(percent + "%");
                        break;
                }
            }
        };

        var cols = [{caption: "<%=ParseString("%%Objectives%%") %>", dataField: "name", "allowResizing" : true, "minWidth": 200}];

        if (showTreePrioritiesL || showTreePrioritiesG) { // if show priorities in hierarchy
            for (var i = 0; i < synthesize_data.users.length; i++ ) {
                var user = synthesize_data.users[i];
                if (user.checked) {
                    var columnsPriorities = { caption : user.name, "alignment" : "center", "allowSorting" : false, "allowSearch" : false, columns : [] };
                    if (showTreePrioritiesL) columnsPriorities.columns.push({ "caption" : "<% = ResString("optLocal")  %>", "alignment" : "right", "width": 65, "allowSorting" : false, "allowResizing" : true,  "allowSearch" : false, "dataField" : "l" + user.id, cellTemplate: cellTemplateFunc });
                    if (showTreePrioritiesG) columnsPriorities.columns.push({ "caption" : "<% = ResString("optGlobal") %>", "alignment" : "right", "width": 65, "allowSorting" : false, "allowResizing" : true,  "allowSearch" : false, "dataField" : "g" + user.id, cellTemplate: cellTemplateFunc });
                    cols.push(columnsPriorities);
                }
            };
        };

        if (!($("#treeList").data("dxTreeList"))) {
            $("#treeList").dxTreeList({
                autoExpandAll: expandedNodes == null,
                expandedRowKeys: (expandedNodes == null ? [] : expandedNodes),
                dataSource: objectives,
                dataStructure: "plain",
                focusedRowEnabled: true,
                hoverStateEnabled: true,
                keyExpr: "key_id",
                parentIdExpr: "key_pid",
                columns: cols,
                <%--showColumnHeaders: (showTreePrioritiesL || showTreePrioritiesG) && combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>,--%>
                showColumnHeaders: showTreePrioritiesL || showTreePrioritiesG,
                scrolling: {
                    showScrollbar: "always",
                    mode: "standard"
                },
                selection: {
                    allowSelectAll: false,
                    mode: "none",
                    recursive: false
                },
                focusedRowKey: sel_id,
                width: "auto",
                onFocusedRowChanged: function (e) { 
                    //alert(e.selectedRowKeys[0]);
                    if ((e.row)) {
                        options.showAlternatives = showAlts;
                        var selectedNode = e.row.data;
                        <% If PM.IsRiskProject Then %>
                        $("#cbSwitchShowPriorities").dxSelectBox("instance").option("visible", typeof selectedNode.pid !== "undefined");
                        <% End If %>
                        if (showAlts) {
                            if (options.WRTNodeID != selectedNode.id) {
                                options.WRTNodePID = selectedNode.pid;
                                options.WRTNodeID = selectedNode.id;
                                GridWRTNodeID = selectedNode.id;
                                //if (!selectedNode.isTerminal) {
                                //    var collapsedNodes = JSON.parse(localStorage.getItem('chartCollapsedNodes' + curPrjID));
                                //    if (collapsedNodes !== null) {
                                //        var idx = collapsedNodes.indexOf(e.row.key);
                                //        if (idx >= 0) {
                                //            collapsedNodes.splice(idx, 1);
                                //        } else {
                                //            collapsedNodes.push(e.row.key);
                                //        };
                                //        localStorage.setItem('chartCollapsedNodes' + curPrjID, JSON.stringify(collapsedNodes));
                                //        var expandedNodes = [];
                                //        for (var i = 0; i < objectives.length; i++) {
                                //            var obj = objectives[i];
                                //            if (!(collapsedNodes !== null && collapsedNodes.indexOf(obj.key_id) >= 0)) {expandedNodes.push(obj.key_id)};
                                //            obj.disabled = (!showAlts && obj.isTerminal);
                                //        };
                                //        $("#treeList").dxTreeList("option", "expandedRowKeys", expandedNodes);
                                //    };
                                //};

                                //setOption("selectedNodeID", options.WRTNodeID);
                                //$("#chart").ecChart("option", "selectedNodeID", options.WRTNodeID);
                                loadChartData({"selectedNodeID":options.WRTNodeID});
                                updateInfobar();
                            }
                        } 
                        else 
                        {                                 
                            if (!selectedNode.isTerminal) {
                                if (options.selectedNodeID !== selectedNode.id) {
                                    options.selectedNodeID = selectedNode.id; 
                                    options.WRTNodeID = e.component.option("dataSource")[0].id;  // objectives[0].id;                                    
                                    //$("#chart").ecChart("option", "selectedNodeID", options.selectedNodeID);
                                    //$("#chart").ecChart("redraw");
                                    GridWRTNodeID = options.selectedNodeID;
                                    setOption("selectedNodeID", options.selectedNodeID);
                                    updateInfobar();
                                };
                            }
                        }
                    }
                },
                //onCellPrepared: function(e) {
                //    if (e.rowType === "data" && e.data.isTerminal && !options.showAlternatives) {
                //        e.cellElement.css("color", "#999");
                //    }
                //},
                onCellPrepared: function(e) {  
                    if (e.rowType === "data" && e.data.isTerminal && !options.showAlternatives) {
                        e.cellElement.css("color", "#999");
                    }
                    if (e.rowType === "data" && e.data.isCategory) {
                        e.cellElement.addClass("categorical");
                    }
                    getDxTreeListNodeConnectingLinesOnCellPrepared(e);                    
                },
                onFocusedRowChanging: function (e) {
                    if ((e.rows[e.newRowIndex])) {
                        if (e.rows[e.newRowIndex].data.isTerminal && !showAlts)
                        {
                            e.cancel = true;
                        }
                    }
                },
                onRowExpanded: function (e) {
                    var collapsedNodes = JSON.parse(localStorage.getItem('chartCollapsedNodes' + curPrjID));
                    if (collapsedNodes !== null) {
                        var idx = collapsedNodes.indexOf(e.key);
                        collapsedNodes.splice(idx, 1);
                        localStorage.setItem('chartCollapsedNodes' + curPrjID, JSON.stringify(collapsedNodes));
                    };
                },
                onRowCollapsed: function (e) {
                    var collapsedNodes = JSON.parse(localStorage.getItem('chartCollapsedNodes' + curPrjID));
                    if (collapsedNodes == null) {
                        collapsedNodes = [];
                    };
                    collapsedNodes.push(e.key);
                    localStorage.setItem('chartCollapsedNodes' + curPrjID, JSON.stringify(collapsedNodes));
                },
            });
        } else {
            var tl = $("#treeList").dxTreeList("instance");
            tl.beginUpdate();
            tl.option("dataSource", objectives);
            tl.option("columns", cols);
            if (expandedNodes != null) tl.option("expandedRowKeys", expandedNodes); 
            tl.endUpdate();
        };

        var toolbarItems = [
            {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                disabled: false,
                options: {
                    text: "",
                    icon: "fas fa-users",
                    hint: "",
                    showText: false,
                    elementAttr: {id: "btnSelectUsers"},
                    onClick: function (e) {
                        showSelectUsersDialog();
                    },
                    onContentReady: function (e) {
                        e.element[0].style.textAlign = "left";
                    },
                    width: 28
                }
            },            
            {   location: 'before',
                locateInMenu: 'never',
                widget: 'dxButtonGroup',
                visible: true,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: selectedButtonKeys,
                    displayExpr: "text",
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        var sel = e.component.option("selectedItemKeys");

                        if (e.itemData.ID == 1 && sel.indexOf(1) > -1) showHierarchyTree = true;
                        if (e.itemData.ID == 1 && sel.indexOf(1) == -1) showHierarchyTree = false;
                        if (e.itemData.ID == 2 && sel.indexOf(2) > -1) showTreePrioritiesL =  combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;
                        if (e.itemData.ID == 2 && sel.indexOf(2) == -1) showTreePrioritiesL = false;
                        if (e.itemData.ID == 3 && sel.indexOf(3) > -1) showTreePrioritiesG =  combinedMode*1 !== <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;
                        if (e.itemData.ID == 3 && sel.indexOf(3) == -1) showTreePrioritiesG = false;
                        
                        if ((e.itemData.ID == 2 || e.itemData.ID == 3) && sel.indexOf(1) == -1) {                            
                            sel.push(1);
                            showHierarchyTree = true;
                            e.component.option("selectedItemKeys", sel);
                        }

                        $("#treeList").dxTreeList("instance").option("showColumnHeaders", showTreePrioritiesL || showTreePrioritiesG);
                        
                        selectedButtonKeys = sel;
                        updateToolbar();
                        $("#divTree").css("display", showHierarchyTree ? "" : "none");
                        resizePage();
                        e.itemElement.blur();

                        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=objectives_visibility", { "hierarchy" : showHierarchyTree, "local_priorities" :  showTreePrioritiesL, "global_priorities" : showTreePrioritiesG }, function () { 
                        }, true);
                    },
                    selectionMode: "multiple",
                    items: [{"ID" : 1, "text" : "", "hint" : "<%=ParseString("Hierarchy")%>", "icon" : "fas fa-sitemap" },
                            {"ID" : 2, "text" : "<% = ResString("optLocal")  %>", "hint" : "<%=ParseString("Local Priorities")%>", disabled : false, visible: true },
                            {"ID" : 3, "text" : "<% = ResString("optGlobal") %>", "hint" : "<%=ParseString("Global Priorities")%>", disabled : false, visible: true }]
                }
            }, 
            <% If PM.IsRiskProject Then %>
                { 
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxSelectBox',
                    visible: showAlts && typeof selectedObj.pid !== "undefined",
                    options: {
                        showText: true,
                        itemTemplate: function (data) {  
                            return "<div class='custom-dx-selectbox-item-with-tooltip' title='" + data.Text + "'>" + data.Text + "</div>";  
                        },
                        valueExpr: "ID",
                        displayExpr: "Text",
                        value: showLikelihoodsGivenSources ? 1 : 0,
                        elementAttr: {id: 'cbSwitchShowPriorities'},
                        onSelectionChanged: function (e) { 
                            if (showLikelihoodsGivenSources != e.selectedItem.ID && hasData()) {                           
                                showLikelihoodsGivenSources = e.selectedItem.ID == 1; 
                                switchShowPriorities(e.selectedItem.ID == 1);
                            }
                        },
                        items: [ 
                            {"ID": 0, "Text": "<%=ParseString(If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "%%Vulnerability%%", "Consequence Of %%Alternative%% On %%Objective(i)%%"))%>"},
                            {"ID": 1, "Text": "<%=ParseString(If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "%%Likelihood%%", "%%Impact%% Of %%Alternative%%"))%>"}
                        ]   
                    }
                },                        
                { location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: true,
                disabled: false,
                options: {
                    showText: true,
                    text: "Simulated Results",
                    value: useSimulated,
                    elementAttr: {id: 'cbSimulatedSwitch'},
                    hint: "",
                    onValueChanged: function (e) { 
                        //setOption('useSimulatedValues', e.value);
                        loadChartData({"useSimulatedValues": e.value});
                        useSimulated = e.value;
                        //sendCommand("action=show_sim_results&value=" + e.value, true);
                        //if (getOption("areaMode") != e.value) {
                        //}
                    }
                }
            }, {
                location: 'after',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: false,
                options: {
                    beginGroup: true,
                    icon: "fas fa-cog", text: "Preferences", hint: "Preferences",
                    template: function() {
                        return $('<i class="dx-icon fas fa-cog"></i><span class="dx-button-text nowide1200">Preferences</span>');
                    },
                    disabled: false,
                    elementAttr: {id: 'btn_settings'},
                    onClick: function (e) {
                        //settingsDialog();
                    }
                }
            }, 
            <% End If %>
            //{   location: 'before',
            //    locateInMenu: 'never',
            //    widget: 'dxButtonGroup',
            //    visible: true,
            //    options: {
            //        //searchEnabled: false,
            //        keyExpr: "ID",
            //        selectedItemKeys: [getOption("chartType")],
            //        displayExpr: "text",
            //        disabled: false,
            //        //hint: "Chart type",
            //        //width: 120,
            //        elementAttr: {id: "selChartType"},
            //        onItemClick: function (e) { 
            //            if (getOption("chartType") != e.itemData.ID && hasData()) {
            //                setOption("chartType", e.itemData.ID);
            //                updateToolbar();
            //            }
            //        },
            //        selectionMode: "single",
            //        items: (!showAlts ? (isAdvancedMode ? [{"ID": "objSunburst", "text": "", "hint": "Hierarchical pie", "icon":"icon ec-chart-sunburst"}, 
            //                {"ID": "objPie", "text": "", "hint": "Pie", "icon":"icon ec-chart-pie"},
            //                {"ID": "objDoughnut", "text": "", "hint": "Donut", "icon":"icon ec-chart-donut"},
            //                {"ID": "objColumns", "text": "", "hint": "Columns", "icon":"icon ec-chart-columns"},
            //                {"ID": "objStacked", "text": "", "hint": "Stacked bars", "icon":"icon ec-chart-stacked"}] : 
            //                [{"ID": "objSunburst", "text": "", "hint": "Hierarchical pie", "icon":"icon ec-chart-sunburst"}, 
            //                {"ID": "objColumns", "text": "", "hint": "Columns", "icon":"icon ec-chart-columns"}]) :
            //                (isAdvancedMode ? [{"ID": "altPie", "text": "", "hint": "Pie", "icon":"icon ec-chart-pie"},
            //                {"ID": "altDoughnut", "text": "", "hint": "Donut", "icon":"icon ec-chart-donut"},
            //                {"ID": "altColumns", "text": "", "hint": "Columns", "icon":"icon ec-chart-columns"},
            //                {"ID": "altStacked", "text": "", "hint": "Stacked bars", "icon":"icon ec-chart-stacked"}] : 
            //                []))
            //    }
            //},
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: isAdvancedMode,
                options: {
                    showText: true,
                    hint: "Chart Type",
                    focusStateEnabled:false,
                    valueExpr: "ID",
                    displayExpr: "Text",
                    value: getOption("chartType"),
                    elementAttr: {id: 'selChartType', class:("on_advanced")},
                    onSelectionChanged: function (e) { 
                        if (getOption("chartType") != e.selectedItem.ID && hasData()) {
                            setOption("chartType", e.selectedItem.ID);
                            updateToolbar();
                        }
                    },
                    items: (!showAlts ? [{"ID": "objSunburst", "Text": "Hierarchical pie", "hint": "Hierarchical pie", "icon":"icon ec-chart-sunburst"}, 
                            {"ID": "objPie", "Text": "Pie", "hint": "Pie", "icon":"icon ec-chart-pie"},
                            {"ID": "objDoughnut", "Text": "Donut", "hint": "Donut", "icon":"icon ec-chart-donut"},
                            {"ID": "objColumns", "Text": "Columns", "hint": "Columns", "icon":"icon ec-chart-columns"},
                            {"ID": "objStacked", "Text": "Stacked bars", "hint": "Stacked bars", "icon":"icon ec-chart-stacked"}]:
                            [{"ID": "altPie", "Text": "Pie", "hint": "Pie", "icon":"icon ec-chart-pie"},
                            {"ID": "altDoughnut", "Text": "Donut", "hint": "Donut", "icon":"icon ec-chart-donut"},
                            {"ID": "altColumns", "Text": "Columns", "hint": "Columns", "icon":"icon ec-chart-columns"},
                            {"ID": "altStacked", "Text": "Stacked bars", "hint": "Stacked bars", "icon":"icon ec-chart-stacked"}]) 
                }
            },
            <% If Not PM.IsRiskProject Then %>{
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: showAlts,
                options: {
                    showText: true,
                    hint: "Normalization mode",
                    disabled: false,
                    valueExpr: "ID",
                    displayExpr: "Text",
                    value: normalizationMode, //getOption("normalizationMode"),
                    elementAttr: {id: 'selNormalized'},
                    onSelectionChanged: function (e) { 
                        if (getOption("normalizationMode") != e.selectedItem.ID && hasData()) {
                            normalizationMode = e.selectedItem.ID;
                            setOption('normalizationMode', e.selectedItem.ID);
                            updateToolbar();
                        }
                    },
                    items: [ 
                        {"ID": "none", "Text": "Unnormalized"}, 
                        {"ID": "normAll", "Text": "Normalized"},
                        //{"ID": "normSelected", "Text": "Normalized for Selected"},
                        {"ID": "norm100", "Text": "% of Maximum"}
                    ]   
                }
            },<% End If %>
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: isAdvancedMode && showAlts,
                options: {
                    showText: true,
                    hint: "Filter <% = ParseString("%%Alternatives%%").ToLower %>",
                    itemTemplate: function (data) {  
                        return "<div class='custom-dx-selectbox-item-with-tooltip' title='" + data.Text + "'>" + data.Text + "</div>";  
                    },
                    valueExpr: "ID",
                    displayExpr: "Text",
                    value: AlternativesFilterValue,
                    elementAttr: {id: 'cbAltsFilter', class:"on_advanced"},
                    onSelectionChanged: function (e) { 
                        if (AlternativesFilterValue != e.selectedItem.ID && hasData()) {                            
                            applyAltsFilter(e.selectedItem.ID);
                        }
                    },
                    items: [ 
                        {"ID": -1, "Text": "Show all <%=ParseString("%%alternatives%%").ToLower%>"}, 
                        {"ID": 5, "Text": "Show top 5 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = If(PM.IsRiskProject, ParseString("%%risks%%").ToLower, "priorities") %>"}, 
                        {"ID": 10, "Text": "Show top 10 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = If(PM.IsRiskProject, ParseString("%%risks%%").ToLower, "priorities") %>"},
                        {"ID": 25, "Text": "Show top 25 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = If(PM.IsRiskProject, ParseString("%%risks%%").ToLower, "priorities") %>"},
                        {"ID": -2, "Text": "Advanced"},
                        {"ID": -105, "Text": "Show bottom 5 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = If(PM.IsRiskProject, ParseString("%%risks%%").ToLower, "priorities") %>"}, 
                        {"ID": -110, "Text": "Show bottom 10 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = If(PM.IsRiskProject, ParseString("%%risks%%").ToLower, "priorities") %>"},
                        {"ID": -125, "Text": "Show bottom 25 <%=ParseString("%%alternatives%%").ToLower%> based on All Participants <% = If(PM.IsRiskProject, ParseString("%%risks%%").ToLower, "priorities") %>"},                        
                        <%If App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ResourceAlignerEnabled, Nothing, True) Then%>
                        {"ID": -4, "Text": "Show funded <%=ParseString("%%alternatives%%").ToLower%> - <%=SafeFormString(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Name)%>", disabled: true, visible: false },
                        <%End If%>
                        {"ID": -3, "Text": "Select/deselect <%=ParseString("%%alternatives%%").ToLower%>"},
                        <%If ActiveProjectHasAlternativeAttributes Then%>
                        {"ID": -5, "Text": "Filter by <%=ParseString("%%alternative%%").ToLower%> attributes"},
                        <%End If%>
                        <%If PM.IsRiskProject AndAlso (PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel) Then%>
                        {"ID": -6, "Text": "Show <%=ParseString("%%risks%%")%> only"},
                        {"ID": -7, "Text": "Show <%=ParseString("%%opportunities%%")%> only"}
                        <%End If%>
                    ]   
                }
            },
            //</nobr><br><span class="text" id=""></span>            
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: isAdvancedMode && showAlts && (AlternativesFilterValue === -2 || AlternativesFilterValue === -3 || AlternativesFilterValue === -5),
                options: {
                    icon: "fas fa-pencil-alt",
                    hint: "Edit",
                    showText: false,
                    elementAttr: {id: "btnAdvEdit"},
                    onClick: function (e) {
                        editAdvClick();
                    }
                }
            },            
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: cv !== "stacked" && cpp > 1,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: [(getOption("singleRow")?"":"singleRow")],
                    displayExpr: "text",
                    elementAttr: {id: "selGrid"},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        if (e.itemData.ID == "singleRow") setOption("singleRow", !getOption("singleRow"));
                    },
                    selectionMode: "multiple",
                    items: ([{"ID": "singleRow", "text": "Grid View", "hint": "Grid View/Single Row", "icon":"icon ec-multi-row"}])
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                showText: "inMenu",
                visible: (cv == "sunburst" || cv == "pie") && isAdvancedMode,
                options: {
                    elementAttr: {id: 'btnDecRotate', class:"on_advanced"},
                    text: "Adjust Rotation CCW",
                    icon: "fas fa-angle-left",
                    hint: "Adjust Rotation CCW",
                    onClick: function (e) { 
                        adjustRotation(-5); 
                    }
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: cv !== "stacked",
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: [(getOption("isRotated")?"isRotated":"")],
                    displayExpr: "text",
                    elementAttr: {id: "selRotate"},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        if (e.itemData.ID == "isRotated") setOption('isRotated', !getOption("isRotated"));
                    },
                    selectionMode: "multiple",
                    items: ([{"ID": "isRotated", "text": "Rotate", "hint": "Rotate", "icon":"icon ec-chart-rotate-cw"}])
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                showText: "inMenu",
                visible: (cv == "sunburst" || cv == "pie") && isAdvancedMode,
                options: {
                    elementAttr: {id: 'btnDecRotate', class:"on_advanced"},
                    text: "Adjust Rotation CW",
                    icon: "fas fa-angle-right",
                    hint: "Adjust Rotation CW",
                    onClick: function (e) { 
                        adjustRotation(5); 
                    }
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                showText: "inMenu",
                visible: (cv == "sunburst") && isAdvancedMode,
                options: {
                    elementAttr: {id: 'btnZoomIn', class:"on_advanced"},
                    text: "Zoom In",
                    icon: "fas fa-search-plus",
                    hint: "Zoom In",
                    onClick: function (e) { 
                        adjustZoom(0.1); 
                    }
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                showText: "inMenu",
                visible: (cv == "sunburst") && isAdvancedMode,
                options: {
                    elementAttr: {id: 'btnZoomOut', class:"on_advanced"},
                    text: "Zoom Out",
                    icon: "fas fa-search-minus",
                    hint: "Zoom Out",
                    onClick: function (e) { 
                        adjustZoom(-0.1); 
                    }
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: true,
                options: {
                    showText: true,
                    hint: "Font Size",
                    focusStateEnabled:false,
                    valueExpr: "ID",
                    displayExpr: "Text",
                    value: (getOption("fontSize")|| ecChartDefOptions["fontSize"]),
                    disabled: cv == 'columns' && ((getOption("autoFontSize"))),
                    width: 100,
                    elementAttr: {id: 'selFontSize'},
                    onSelectionChanged: function (e) { 
                        var selFontSize = e.selectedItem.ID;
                        if (getOption("fontSize") !== selFontSize) {
                            setOption("fontSize", selFontSize);
                            setOption("adjustFontSize", 0);
                        }
                    },
                    items:  [{"ID": 8, "Text": "8"},
                            {"ID": 10, "Text": "10"},
                            {"ID": 12, "Text": "12"},
                            {"ID": 14, "Text": "14 - Default"},
                            {"ID": 16, "Text": "16"},
                            {"ID": 18, "Text": "18"},
                            {"ID": 20, "Text": "20"}]
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: (cv == 'columns'),
                options: {
                    text: "Auto font size",
                    hint: "Auto font size",
                    showText: true,
                    elementAttr: {id: 'cbAutoFontSize'},
                    value: (getOption("autoFontSize")),
                    onValueChanged: function (e) {
                        if (getOption("autoFontSize") !== e.value) {
                            setOption("autoFontSize", e.value);
                            updateToolbar();
                        }
                        
                    }
                }
            },
            //{   location: 'before',
            //    locateInMenu: 'auto',
            //    widget: 'dxButton',
            //    showText: "inMenu",
            //    visible: true,
            //    options: {
            //        elementAttr: {id: 'btnDecFont'},
            //        text: "Decrease font size",
            //        icon: "icon ec-font-dec",
            //        hint: "Decrease size",
            //        onClick: function (e) { 
            //            adjustFont(-2); 
            //            updateToolbar();
            //        }
            //    }
            //},
            //{   location: 'before',
            //    locateInMenu: 'auto',
            //    widget: 'dxButton',
            //    showText: "inMenu",
            //    visible: true,
            //    options: {
            //        elementAttr: {id: 'btnIncFont'},
            //        text: "Increase size",
            //        icon: "icon ec-font-inc",
            //        hint: "Increase font size",
            //        onClick: function (e) { 
            //            adjustFont(2); 
            //            updateToolbar();
            //        }
            //    }
            //},
            //{   location: 'before',
            //    locateInMenu: 'auto',
            //    widget: 'dxButton',
            //    showText: "inMenu",
            //    disabled: afs == 0,
            //    visible: true,
            //    options: {
            //        elementAttr: {id: 'btnResetFont'},
            //        icon: "fas fa-undo-alt",
            //        text: "Reset font",
            //        hint: "Reset font size",
            //        onClick: function (e) { 
            //            adjustFont(0); 
            //            updateToolbar();
            //        }
            //    }
            //},
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: true,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: (getOption("showLegend")?["showLegend"]:[]),
                    displayExpr: "text",
                    elementAttr: {id: 'cbLegend'},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        setOption('showLegend', !getOption("showLegend"));
                        updateToolbar();
                    },
                    selectionMode: "multiple",
                    items: [{"ID": "showLegend", "text": "Legend", "hint": getOptionProps("showLegend").hint, "icon":getOptionProps("showLegend").icon}]
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: cv == "columns",
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: (getOption("showComponents")?["showComponents"]:[]),
                    displayExpr: "text",
                    elementAttr: {id: 'cbComponents'},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        setOption('showComponents', !getOption('showComponents'));
                    },
                    selectionMode: "multiple",
                    items: [{"ID": "showComponents", "text": "Components", "hint": getOptionProps("showComponents").hint, "icon":"icon ec-chart-components"}]
                }
            },
            {   location: 'before',
                locateInMenu: 'always',
                widget: 'dxSelectBox',
                visible: true,
                disabled: !sl,
                showText: true,
                text: "Legend position",
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: getOption('legendPosition'),
                    width: 80,
                    displayExpr: "Text",
                    disabled: false,
                    elementAttr: {id: 'selLegend'},
                    hint: "Legend position",
                    onSelectionChanged: function (e) { 
                        if (getOption("legendPosition") != e.selectedItem.ID && hasData()) {
                            setOption('legendPosition', e.selectedItem.ID);
                        }
                    },
                    items: [{"ID": "auto", "Text": "Auto"}, 
                            {"ID": "right", "Text": "Right"}, 
                            {"ID": "bottom", "Text": "Bottom"}]
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: isAdvancedMode,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: (getOption("showLabels")?["showLabels"]:[]),
                    displayExpr: "text",
                    elementAttr: {id: 'cbLabels', class:"on_advanced"},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        setOption('showLabels', !getOption("showLabels"));
                    },
                    selectionMode: "multiple",
                    items: [{"ID": "showLabels", "text": "Labels", "hint": getOptionProps("showLabels").hint, "icon":getOptionProps("showLabels").icon}]
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: cv == "sunburst" && isAdvancedMode,
                options: {
                    text: "Area Mode",
                    showText: true,
                    hint: "The sum of the children's segments area is equal to their parent's area",
                    disabled: false,
                    value: getOption("areaMode"),
                    elementAttr: {id: 'cbAreaMode', class:"on_advanced"},
                    onValueChanged: function (e) {
                        if (getOption("areaMode") != e.value) {
                            setOption('areaMode', e.value);
                        }
                    }
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: !showAlts && isAdvancedMode,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: (getOption("showLocal")?["showLocal"]:[]),
                    displayExpr: "text",
                    elementAttr: {id: 'cbShowLocal', class:"on_advanced"},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        setOption('showLocal', !getOption('showLocal'));
                    },
                    selectionMode: "multiple",
                    items: [{"ID": "showLocal", "text": "Local Priorities", "hint": "Show Local Priorities", "icon": "fas fa-map-marker-alt"}],
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButtonGroup',
                visible: cv == "columns" && isAdvancedMode,
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: (getOption("groupByUsers")?["groupByUsers"]:[]),
                    displayExpr: "text",
                    elementAttr: {id: "cbGroupByUsers", class:"on_advanced"},
                    focusStateEnabled:false,
                    onItemClick: function (e) { 
                        setOption("groupByUsers", !getOption("groupByUsers"));
                    },
                    selectionMode: "multiple",
                    items: [{"ID": "groupByUsers", "text": "Group by Users", "hint": "Group charts by Users", "icon":"icon ec-group-users"}]
                }
            },
            {   location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                visible: true,
                showText: true,
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: getOption('sortBy'),
                    hint: "Sorting method",
                    width: 120,
                    displayExpr: "Text",
                    disabled: false,
                    elementAttr: {id: 'selSort'},
                    onSelectionChanged: function (e) { 
                        if (getOption("sortBy") != e.selectedItem.ID && hasData()) {
                            setOption('sortBy', e.selectedItem.ID);
                        }
                    },
                    items: [{"ID": "none", "Text": "Sort None", "icon":"icon ec-sort-none"}, 
                            {"ID": "name", "Text": "Sort by Name", "icon":"icon ec-sort-az"}, 
                            {"ID": "value", "Text": "Sort by <% = If(PM.IsRiskProject, ParseString("%%Risk%%"), "Priority") %>", "icon":"icon ec-sort-val"}]
                }
            },
            <%If Not PM.IsRiskProject Then%>
            {   location: 'before',
                locateInMenu: 'always',
                widget: 'dxSelectBox',
                visible: isAdvancedMode,
                showText: true,
                text: "Synthesis Mode",
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: synthMode,
                    width: 80,
                    displayExpr: "Text",
                    disabled: false,
                    elementAttr: {id: 'selSynthesisMode', class:"on_advanced"},
                    hint: "Synthesis Mode",
                    onSelectionChanged: function (e) { 
                        if (synthMode != e.selectedItem.ID && hasData()) {
                            synthMode = e.selectedItem.ID;
                            setPipeOption('SynthesisMode', e.selectedItem.ID);
                            updateInfobar();
                        };
                    },
                    items: [{"ID": <%=CInt(ECSynthesisMode.smIdeal)%>, "Text": "Ideal"}, 
                            {"ID": <%=CInt(ECSynthesisMode.smDistributive)%>, "Text": "Distributive"}]
                }
            },
            {   location: 'before',
                locateInMenu: 'always',
                widget: 'dxSelectBox',
                visible: showAlts && isAdvancedMode,
                showText: true,
                text: "Combined Mode",
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: combinedMode,
                    width: 80,
                    displayExpr: "Text",
                    disabled: false,
                    elementAttr: {id: 'selAIJAIPMode', class:"on_advanced"},
                    hint: "AIJ/AIP Mode",
                    onSelectionChanged: function (e) { 
                        if (combinedMode != e.selectedItem.ID && hasData()) {
                            combinedMode = e.selectedItem.ID;
                            menu_setOption(menu_option_noAIP, combinedMode == cmAIP);
                            $("#tabs").dxTabPanel("option","items[1].disabled", combinedMode == cmAIP);
                            setPipeOption('CombinedMode', e.selectedItem.ID);
                        };
                    },
                    items: [{"ID": cmAIJ, "Text": "AIJ"}, 
                            {"ID": cmAIP, "Text": "AIP"}]
                }
            },
            <%End If%>
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: isAdvancedMode,
                options: {
                    text: "CIS",
                    showText: true,
                    hint: "Combined Input Source mode",
                    value: useCIS,
                    elementAttr: {id: 'cbCIS', class:"on_advanced"},
                    onValueChanged: function (e) {
                        if (useCIS != e.value) {
                            useCIS = e.value;
                            setPipeOption('UseCISForIndividuals', e.value);
                        }
                    }
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: isAdvancedMode,
                options: {
                    text: "User priorities",
                    showText: true,
                    hint: "User priorities",
                    value: usePriorities,
                    elementAttr: {id: 'cbUserPriorities', class:"on_advanced"},
                    onValueChanged: function (e) {
                        if (usePriorities != e.value) {
                            usePriorities = e.value;
                            setPipeOption('UseWeights', e.value);
                        }
                    }
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: showAlts && isAdvancedMode,
                options: {
                    text: "Include ideal <% = ParseString("%%alternative%%") %>",
                    showText: true,
                    value: includeIdealAlt,
                    elementAttr: {id: 'cbIncludeIdeal', class:"on_advanced"},
                    onValueChanged: function (e) {
                        if (includeIdealAlt != e.value) {
                            includeIdealAlt = e.value;
                            setPipeOption('IncludeIdealAlternative', e.value);
                        }
                    }
                }
            },
            {   location: 'before',
                locateInMenu: 'always',
                widget: 'dxSelectBox',
                visible: true,
                text: "Decimals",
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: getOption('decimals'),
                    displayExpr: "Text",
                    disabled: false,
                    hint: "Show decimals",
                    width: 40,
                    elementAttr: {id: 'selDecimals'},
                    onSelectionChanged: function (e) { 
                        if (getOption("decimals") != e.selectedItem.ID && hasData()) {
                            DecimalDigits = e.selectedItem.ID;
                            setOption('decimals', e.selectedItem.ID);
                        }
                    },
                    items: [{"ID": 0, "Text": "0"}, 
                            {"ID": 1, "Text": "1"}, 
                            {"ID": 2, "Text": "2"}, 
                            {"ID": 3, "Text": "3"}, 
                            {"ID": 4, "Text": "4"}, 
                            {"ID": 5, "Text": "5"}]
                }
            },
            {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxButton',
                options: {
                    icon: "fas fa-file-export",
                    text: "<% = ResString("btnExport")%>",
                    hint: "<% = ResString("btnExport")%>",
                    elementAttr: { id: 'btnExportChart'},                            
                    onClick: function (e) {
                        $("#dlgChartExport").dxPopup({
                            height: 180,
                            title: "Export Chart Settings",
                            visible: true,
                            width: 400,
                        });
                        $("#btnChartExportPNG").dxButton({
                            text: "PNG",
                            onClick: function() {
                                exportChart("PNG");
                            },
                            width: 160,
                        });
                        $("#btnChartExportPDF").dxButton({
                            text: "PDF",
                            onClick: function() {
                                exportChart("PDF");
                            },
                            width: 160,
                        });
                        $("#btnChartExportSVG").dxButton({
                            text: "SVG",
                            onClick: function() {
                                exportChart("SVG");
                            },
                            width: 160,
                        });
                        $("#btnChartExportMultiPagePDF").dxButton({
                            text: "Multipage PDF",
                            onClick: function() {
                                exportChart("MPDF");
                            },
                            width: 160,
                        });
                        $("#btnChartExportCancel").dxButton({
                            text: "Cancel",
                            onClick: function() {
                                $("#dlgChartExport").dxPopup("hide");
                            },
                            width: 160,
                        });
                    }
                }
            }
        ];

        $("#toolbar").dxToolbar({
            items: toolbarItems,
            disabled: !hasData()
        });

        $("#tabs").dxTabPanel({
            items: [{id:0,title:"<% =JS_SafeString(ParseString("%%Alternatives%%")) %>",icon:"icon ec-alts"},
                    {id:1,title:"<% =JS_SafeString(ParseString("%%Objectives%%")) %>",icon:"icon ec-hierarchy", disabled: combinedMode == <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>}],
            selectedIndex: (showAlts ? 0 : 1),
            onSelectionChanged: function(e) {
                var ct = getOption("chartType");
                if (e.addedItems.length > 0) {
                    var tabID = e.addedItems[0].id;
                    var ctNew = ct;
                    if (tabID == 0) {
                        switch (ct) {
                            case "objSunburst":
                            case "objPie":
                                ctNew = (isAdvancedMode ? "altPie" : "altColumns");
                                break;
                            case "objDoughnut":
                                ctNew = "altDoughnut";
                                break;
                            case "objColumns":
                                ctNew = "altColumns";
                                break;
                            case "objStacked":
                                ctNew = "altStacked";
                                break;
                        }
                    }else{
                        switch (ct) {
                            case "altPie":
                                ctNew = "objPie";
                                break;
                            case "altDoughnut":
                                ctNew = "objDoughnut";
                                break;
                            case "altColumns":
                                ctNew = "objColumns";
                                break;
                            case "altStacked":
                                ctNew = "objStacked"
                                break;
                        }
                    };
                    if (ctNew !== ct) {
                        var do_reset = true;
                        options.showAlternatives = ctNew.indexOf("alt") > -1;
                        options.chartType = ctNew;
                        initTitle(options.showAlternatives);
                        if (options.showAlternatives) {
                            if (typeof options.selectedNodeID == "undefined") options.selectedNodeID = options.WRTNodeID;
                            if (options.selectedNodeID >= 0) {
                                options.WRTNodeID = options.selectedNodeID;
                                do_reset = false;
                            }
                        }
                        if (!options.showAlternatives && options.WRTNodeID>=0) {
                            var o = $("#chart").ecChart("getObjectiveByID", options.WRTNodeID);
                            if ((o)) {
                                if (o.isTerminal) o = $("#chart").ecChart("getObjectiveByID", o.pid);
                                if ((o) && !o.isTerminal) {
                                    do_reset = false;
                                    options.WRTNodeID = o.id;
                                    options.selectedNodeID = o.id;
                                }
                            }
                        }
                        if (do_reset) {
                            options.WRTNodeID = -1;
                            options.selectedNodeID = -1;
                        }
                        $("#chart").ecChart("option", "chartType", ctNew);
                        $("#chart").ecChart("option", "selectedNodeID", options.selectedNodeID);
                        //setOption("chartType", ctNew);
                        $("#treeList").dxTreeList("dispose");
                        $("#treeList").empty();
                        setTimeout(function () {updateToolbar()}, 200);
                        loadChartData({"chartType": ctNew});
                        if (!is_manual) {
                            var pg = -1;
                            if (ctNew == "altColumns") pg = <% =_PGID_ANALYSIS_CHARTS_ALTS %>; 
                            if (ctNew == "objColumns") pg = <% =_PGID_ANALYSIS_CHARTS_OBJS %>; 
                            if (typeof navOpenPage == "function" && (pg>0)) {
                                navOpenPage(pg, true);
                            }
                        }
                    }
                }
            }
        });
        updateInfobar();
    }

    function switchShowPriorities(value) {
        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=switch_show_priorities", { "value" : value }, function () {
            loadChartData();
        }, false);
    }

    /* Filter Alternatives */
    function setOldVal() {
        var cb = $("#cbAltsFilter").dxSelectBox("instance");
        if ((cb)) old_filter_val = cb.option("value");
    }

    function resetOldVal() {
        var cb = $("#cbAltsFilter").dxSelectBox("instance");
        if ((cb)) cb.option("value", old_filter_val);
        updateToolbar();
    }

    function applyAltsFilter(value) {        
        AlternativesFilterValue = value;
        options.AlternativesFilterValue = value;

        if (value == -4) {
            $("#tdScenarioName").show();
        } else {
            $("#tdScenarioName").hide();
        }

        switch (value) {
            case -2:
                filterAltsAdvanced();
                break;
            case -3:
                filterAltsCustom();
                break;
            case -5:
                filterAltsAttributes();
                break;
            default:
                callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=" + ACTION_ALTS_FILTER, { "name": "value", "value" : value }, function () {
                    $("#chart").ecChart("option", "AlternativesFilterValue", value);
                    loadChartData();
                }, false);
                //setOldVal();
                break;
        }        
        updateToolbar();
    }

    function editAdvClick() {
        var cb = $("#cbAltsFilter").dxSelectBox("instance");
        if ((cb)) {
            switch (cb.option("value")*1) {
                case -2:
                    filterAltsAdvanced();
                    break;
                case -3:
                    filterAltsCustom();
                    break;
                case -5:
                    filterAltsAttributes();
                    break;
            }
        }
    }

    /* Filter Alternatives - Advanced */
    var dlg_alts_advanced      = null;
    var dlg_alts_custom        = null;
    var dlg_alts_attributes    = null;

    function filterAltsAdvanced() {
        if ((dlg_alts_advanced)) dlg_alts_advanced = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_alts_advanced = $("#divAltsAdvanced").dialog({
            autoOpen: false,
            width: 600,
            height: 200,
            minHeight: 150,
            maxHeight: 850,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "Advanced",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_alts_advanced.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_alts_advanced.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
                document.body.style.cursor = "default";
                $("#cbAdvAltsNum").val(AlternativesAdvancedFilterValue);
                $("#cbAdvUsers").val(AlternativesAdvancedFilterUserID);
            },
            close: onAltsAdvancedDialogClose
        });
        dlg_alts_advanced.dialog('open');
    }

    function onAltsAdvancedDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            //sendCommand("action="+ACTION_ALTS_FILTER+"&value=-2&alts_num="+$("#cbAdvAltsNum").val()+"&user_id="+$("#cbAdvUsers").val(), true);
            AlternativesAdvancedFilterValue = $("#cbAdvAltsNum").val();
            AlternativesAdvancedFilterUserID = $("#cbAdvUsers").val();
            callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=" + ACTION_ALTS_FILTER, { "name": "value", "value" : -2, "alts_num" : $("#cbAdvAltsNum").val(), "user_id" : $("#cbAdvUsers").val() }, function () {
                options.AlternativesFilterValue = -2;
                $("#chart").ecChart("option", "AlternativesFilterValue", -2);
                loadChartData();
            }, false);
            setOldVal();
        } else {
            resetOldVal();
        }
        updateToolbar();
    }

    // Filter Alternatives - Custom
    function filterAltsCustom() {
        if ((dlg_alts_custom)) dlg_alts_custom = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_alts_custom = $("#divAltsCustom").dialog({
            autoOpen: false,
            width: 760,
            height: 435,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "Select/Deselect <%=ParseString("%%Alternatives%%")%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_alts_custom.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_alts_custom.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
                document.body.style.cursor = "default";
            },
            close: onAltsCustomDialogClose
        });
        dlg_alts_custom.dialog('open');
        $(".ui-dialog-buttonset").prepend($("#pnlAllOrNoneSelector"));
    }

    function onAltsCustomDialogClose() {
        $("body").css("overflow", "auto");
        $("#divAltsCustom").prepend($("#pnlAllOrNoneSelector"));
        if (dialog_result) {
            var params = "";
            var cb_arr = $("input:checkbox.cust_alt_cb");
            $.each(cb_arr, function(index, val) { var aid = val.value*1; if (val.checked) { params += (params == "" ? "" : ",") + aid; } });
            //sendCommand("action="+ACTION_ALTS_FILTER + "&value=-3&ids=" + params, true);
            callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=" + ACTION_ALTS_FILTER, { "name": "value", "value" : -3, "ids" : params }, function () {
                options.AlternativesFilterValue = -3;
                $("#chart").ecChart("option", "AlternativesFilterValue", -3);
                loadChartData();
            }, false);
            setOldVal();
        } else {
            resetOldVal();
        }
        updateToolbar();
    }

    function filterAltsCustomSelect(chk) {
        $("input:checkbox.cust_alt_cb").prop('checked', chk*1 == 1);
    }

    // Filter by alternative attributes
    function filterAltsAttributes() {
        if ((dlg_alts_attributes)) dlg_alts_attributes = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_alts_attributes = $("#divAltsAttributes").dialog({
            autoOpen: false,
            width: 560,
            height: 235,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "Filter by <%=ParseString("%%alternative%%")%> attributes",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_alts_attributes.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_alts_attributes.dialog( "close" ); }}],
            open:  function() { 
                $("body").css("overflow", "hidden");
                document.body.style.cursor = "default";
                InitFilterGrid();
            },
            close: onAltsAttributesDialogClose
        });
        dlg_alts_attributes.dialog('open');
    }

    function onAltsAttributesDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            ApplyFilter(false);            
            setOldVal();
        } else {
            resetOldVal();
        }
        updateToolbar();
    }

    function AddFilerRow(d, i) {
        var tbl = document.getElementById("tblFilters");
        if ((tbl))
        {
            if (i>flt_max_id) flt_max_id = i;

            var td = document.getElementById("flt_tr_" + i);
            if (!(td))
            {
                var r = tbl.rows.length;
                td = tbl.insertRow(r);
                td.id = "flt_tr_" + i;
                td.style.verticalAlign = "top";
                td.style.textAlign  = "left";
                td.className = "text";
                td.insertCell(0);
                td.insertCell(1);
                td.insertCell(2);
            }

            var attr = null;
                
            var sChk = "<input type='checkbox' id='flt_chk_" + i + "' value='1' onclick='IsAnyFilterChecked(); '" + (d[fidx_chk]=="1" ? " checked" : "") + ">";
            var sAttribs = "";            
            for (var j=0; j<attr_list.length; j++)
            {
                var act = (d[fidx_id] == attr_list[j][aidx_id]);
                sAttribs += "<option value='" + attr_list[j][aidx_id] + "'" + (act ? " selected" : "") + ">" + replaceString("'", "&#39;", attr_list[j][aidx_name]) + "</option>";
                if (act) attr = attr_list[j];
            }
            sAttribs = "<select id='flt_attr_" + i + "' style='width:24ex' onChange='ChangeFilterAttr(" + i + ", this.value);'>" + sAttribs + "</select>";

            sOpers = "";
            var sVal = "";

            if ((attr))
            {
                var o = oper_available[attr[aidx_type]];
                if (o) {
                    for (j=0; j<o.length; j++)
                    {
                        sOpers += "<option value='" + o[j] + "'" + (d[fidx_oper] == o[j] ? " selected" : "") + ">" + oper_list[o[j]] + "</option>";
                    }
                }
                sOpers = "<select id='flt_oper_" + i + "' style='width:15ex'>" + sOpers + "</select>";

                if ((attr[aidx_type] == avtEnumeration) || (attr[aidx_type] == avtEnumerationMulti)) //approach 1 - select multiple
                {
                    var v = attr[aidx_vals];
                    var vals = [d[fidx_val]];
                    if (attr[aidx_type] == avtEnumerationMulti) vals = d[fidx_val].split(";");                    
                    
                    for (j=0; j<v.length; j++)
                    {
                        var is_selected = false;
                        for (k=0; k<vals.length; k++)
                        {
                            if (vals[k] == v[j][0]) is_selected = true;
                        }
                        sVal += "<option value='" + v[j][0] + "'" + (is_selected ? " selected" : "") + ">" + replaceString("'", "&#39;", v[j][1]) + "</option>";
                    }
                    var multi = "";
                    if (attr[aidx_type] == avtEnumerationMulti) multi = "multiple='multiple'";
                    sVal = "<select " + multi + " id='flt_val_" + i + "' style='width:24ex; margin-top:3px;'>" + sVal + "</select>";

                    //approach 2 - check list box
                    if (attr[aidx_type] == avtEnumerationMulti)
                    {
                        sVal="";
                        for (j=0; j<v.length; j++)
                        {
                            var is_selected = false;
                            for (k=0; k<vals.length; k++)
                            {
                                if (vals[k] == v[j][0]) is_selected = true;                            
                            }
                            sVal += "<li style='margin:0; padding:0;' title='" + replaceString("'", "&#39;", v[j][1]) +"'><label for='chk" + j + "_" + k + "'><input type='checkbox' id='chk" + j + "_" + k + "' value='" + v[j][0] + "'" + (is_selected ? " checked" : "") + ">" + replaceString("'", "&#39;", v[j][1]) + "</label></li>";
                        }
                        sVal = "<ul id='flt_ul_" + i + "' style='height:12ex; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'>" + sVal + "</ul>";
                    }
                }
                else
                {
                    if (attr[aidx_type] != avtBoolean) {
                        sVal = "<input type='text' style='width:14ex; margin-top:2px;' id='flt_val_" + i + "' value='" + replaceString("'", "&#39;", d[fidx_val]) + "'>";
                    }
                }
            }

            td.cells[0].innerHTML = "<nobr>" + sChk + sAttribs + "&nbsp;" + sOpers + "</nobr>";
            td.cells[1].innerHTML = sVal;
            td.cells[2].innerHTML = "<input type=button class='button' style='width:22px;height:22px;' id='flt_del_" + i + "' value='X' onclick='onDeleteRule(" + i + "); IsAnyFilterChecked(); return false;'>";
        }
    }

    function ChangeFilterAttr(id, val) {
        var c = document.getElementById("flt_chk_" + id);
        var o = document.getElementById("flt_oper_" + id);
        var d = [((c) && c.checked ? 1 : 0), val, o.value, ''];
        AddFilerRow(d, id);
    }

    function onAddNewRule() {
        var d = [1, attr_list[0][aidx_id], 1, ''];
        AddFilerRow(d, flt_max_id+1);
    }

    function onDeleteRule(id) {
        dxConfirm(resString("msgAreYouSure"), "DeleteRule(" + id + ");");
    }

    function DeleteRule(id) {
        var td = document.getElementById("flt_tr_" + id);
        if ((td)) td.parentNode.removeChild(td);
    }

    function InitFilterGrid() {
        var d = document.getElementById("divFilters");
        if ((d)) d.innerHTML = "<table id='tblFilters' border='0' cellspacing='4' cellpadding='0'></table><div id='msgCheck' style='display: none; color: red;'>To activate a filter, check the box next to it</div>";
        if ((attr_flt) && (attr_list)) {
            for (var i=0; i<attr_flt.length; i++) {
                var d = attr_flt[i];
                AddFilerRow(d, i);
            }
        }
        IsAnyFilterChecked();
    }

    function ApplyFilter(do_reset) {
        var sData = (do_reset ? " " : getFilterString());
        //sendCommand("action="+ACTION_ALTS_FILTER + "&value=-5&filter=" + encodeURIComponent(sData) + "&combination=" + $("#cbFilterCombination").val(), true);
        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=" + ACTION_ALTS_FILTER, { "name": "value", "value" : -5, "filter" : sData, "combination" : $("#cbFilterCombination").val() }, function () {
            options.AlternativesFilterValue = -5;
            $("#chart").ecChart("option", "AlternativesFilterValue", -5);
            loadChartData();
        }, false);
    }

    function onFilterReset() {
        dxConfirm(resString("msgResetFilter"), "DoFilterReset();");
    }

    function DoFilterReset() {
        for (var i=0; i<=flt_max_id; i++) {
            var c = document.getElementById("flt_chk_" + i);
            if ((c)) c.checked = 0;
        }
        ApplyFilter(true);
    }

    function IsAnyFilterChecked() {
        var retVal = false;
        for (var i=0; i<=flt_max_id; i++) {            
            var c = document.getElementById("flt_chk_" + i);
            if ((c) && (c.checked)) {
                retVal = true;
            }
        }    

        $("#msgCheck").toggle(!retVal);                

        var btn = document.getElementById("btnApplyFilter");    
        if ((btn)) { btn.disabled = !retVal; }   
    }

    function getFilterString() {
        attr_flt = [];
        var sData = "";
        for (var i=0; i<=flt_max_id; i++) {
            var c = document.getElementById("flt_chk_" + i);
            var a = document.getElementById("flt_attr_" + i);
            var o = document.getElementById("flt_oper_" + i);
            var v = document.getElementById("flt_val_" + i);
            
            var selValue = ""            

            if (!(v)) {
                selValue = "";
                
                //approach 2: checkbox list
                var ul = document.getElementById("flt_ul_" + i);                
                if (ul) {
                    var items = ul.getElementsByTagName("input");
                    for (var k=0; k < items.length; k++)
                    {                    
                        if (items[k].checked) 
                        {
                            if (selValue.length > 0) selValue += ";"
                            selValue += items[k].value;
                        }
                    }
                }
            } else { selValue = v.value; }
            
            if ((c) && (a) && (o))
            {
                attr_flt.push([c.checked ? 1 : 0, a.value, o.value, selValue]);
                sData += (sData == "" ? "" : "\n") + (c.checked ? 1 : 0) + "<% =Flt_Separator %>" + a.value  + "<% =Flt_Separator %>" + o.value  + "<% =Flt_Separator %>" + selValue;
            }
        }
        return sData;
    }
    // End filtering by alternative attributes

    function updateToolbarFlt() {
        var lbl = document.getElementById("lblAdvFilter");
        if ((lbl)) lbl.style.display = "none";
        var cb = $("#cbAltsFilter").dxSelectBox("instance");
        if ((cb)) {
            if (cb.option("value")*1 < -1) {
                $("#btnAdvEdit").dxButton("instance").option("visible", true);
            } else {
                $("#btnAdvEdit").dxButton("instance").option("visible", false);
            }

            var cbN = document.getElementById("cbAdvAltsNum");
            var cbP = document.getElementById("cbAdvUsers");


            if (options.showAlternatives && (lbl) && (cbN) && (cbP) && cb.option("value")*1 == -2) {
                var direction = "top";
                if ([-105, -110, -125].indexOf(cb.option("value")*1) > -1) direction = "bottom";
                lbl.innerHTML = "<small>Showing " + direction + " " + cbN.options[cbN.selectedIndex].value + "&nbsp;<%=ParseString("%%alternatives%%").ToLower%> based on " + cbP.options[cbP.selectedIndex].text + " priorities</small>";
                lbl.style.display = "";
            }

            // enable or disable the "Normalize for Selected" depending on applied filter
            //var cbNO = document.getElementById("cbNormalizeOptions");
            //if (cbNO) {
            //    cbNO.options[2].disabled = cb.value*1 == -1;
            //    if (cb.value*1 == -1 && cbNO.selectedIndex == 2) cbNO.selectedIndex = 1;
            //}            
        }

        // update the Select Events icon
        var img = document.getElementById("imgFilterEvents");        
        if ((img)) {
            var event_list_len = event_list.length;
            var checkedEvents = 0;
            for (var k = 0; k < event_list_len; k++) {
                if (event_list[k][IDX_EVENT_ENABLED] == 1) checkedEvents += 1;
            }
            img.className = (checkedEvents == event_list_len ? "fas fa-filter fa-2x" : " fas fa-exclamation-circle fa-2x");
        }
    }
    /* Filter Alternatives */

    function hasData() {
        return (typeof synthesize_data != "undefined" && (synthesize_data)) && $("#chart").html()!="";
    }

    function setOption(option, val) {    
        $("#chart").ecChart("option", option, val);
        $("#chart").ecChart("redraw");
        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=set_option", { "name": option, "value" : val, "pid" : options.WRTNodePID }, function (res) {
            if (option == "selectedNodeID") {
                WRTNodeParentGUID = res.wrtNodeParent;
            }
        }, true);
    };

    function getWidgetParams() {
        var options = $("#chart").ecChart("getChangedOptions");
        options["userIDs"] = $("#chart").ecChart("option", "userIDs");
        var params= {"synthmode": synthMode, "combinedmode": combinedMode, "usecis": useCIS, "useprty": usePriorities, "decimals": DecimalDigits, "normmode": normalizationMode};
        var all = joinlists(params, options);
        return all;
    }

    function onGetDirectLink() {
        return list2params(getWidgetParams());
    };

    function onGetReportLink() {
        return (pgID == <% =_PGID_ANALYSIS_CHARTS_ALTS %> ? "<% = JS_SafeString(PageURL(_PGID_ANALYSIS_CHARTS_ALTS)) %>" : "<% = JS_SafeString(PageURL(_PGID_ANALYSIS_CHARTS_OBJS)) %>") + "&<% =_PARAM_ACTION %>=upload&" + onGetDirectLink();
    }

    function onGetReportTitle() {
        var t = "";
        //var h = $("#pgHeader span:visible");
        var h = $(".pgChartsTitle:visible");
        if ((h) && (h.length>0)) {
            for (var i =0; i<h.length; i++) {
                t += (t == "" ? "" : " ") + h[i].innerText;
            }
        }
        return t;
    }

    function onGetReport(addReportItem) {
        if (typeof addReportItem == "function") {
            localStorage.setItem(_sess_dash_alts_flt, (AlternativesFilterValue != -1));
            addReportItem({"name": onGetReportTitle(), "type": (pgID == <% =_PGID_ANALYSIS_CHARTS_ALTS %> ? "<% =JS_SafeString(ecReportItemType.AlternativesChart) %>" : "<% =JS_SafeString(ecReportItemType.ObjectivesChart) %>"), "edit": document.location.href, "export": onGetReportLink(), "ContentOptions": getWidgetParams()});
        }
    }

    function setPipeOption(option, val) {    
        callAPI("pm/params/?<% =_PARAM_ACTION %>=set_pipe_option", { "name": option, "value" : val }, onSetPipeOption);
    };

    function onSetPipeOption(data) {
        loadChartData();
    }

    function getOption(name) {
        var retVal = $("#chart").ecChart("option", name);
        return retVal;
    }

    function exportChart(format) {
        var mpage = false;
        if (format == "MPDF") {
            mpage = true;
            format = "PDF";
            //alert("This could take some time. You might want to grab a cup of coffee.");
            //setTimeout(function () {DevExpress.ui.dialog.alert("This could take some time. You might want to grab a cup of coffee.", "Please wait...") }, 200);
            showLoadingPanel("This could take some time. You might want to grab a cup of coffee.");
        };
        var mCoef = 6;
        setTimeout(function () {
            $("#chart").ecChart("export", "chart", 297*mCoef, 210*mCoef, format, mpage);
        }, 200);
    };

    function adjustFont(af) {
        var caf = $("#chart").ecChart("option", "adjustFontSize");
        if (af == 0) caf = 0;
        setOption('adjustFontSize', caf + af);
    };

    function adjustRotation(ar) {
        var cra = $("#chart").ecChart("option", "rotateAngle");
        setOption('rotateAngle', cra + ar);
    };

    function adjustZoom(az) {
        var cz = $("#chart").ecChart("option", "zoomRatio") * 1;
        setOption('zoomRatio', cz + az);
    };

    function resizePage() {
        var c = $("#content");
        var t = $("#toolbar");
        var i = $("#infobar");
        if ((c) && (c.length)) {
            var margin = 10;
            var p = c.parent();
            var tw = 0;
            $("#content").width(200).height(100);
            //var w = p.width() - margin;
            var w = t.width() + 14;
            var h = p.height() - 40;
            if ((t) && (t.length)) h -= t.height();
            if ((i) && (i.length)) h -= i.height();
            $("#content").width(w).height(h);    // AD: +14
            //$("#toolbar").dxToolbar("instance").repaint();
            if (hasData()) {
                $("#treeList").height(h-5);
                tw = $(".split_left").width();
                if (!showHierarchyTree) tw = 0;
                $("#chart").ecChart("resize", w - tw, h - 5);
            }
            $("#pgHeader").css("margin-left", tw);
        }
    }

    function loadChartData(extra) {
        //var nodeID = options.WRTNodeID;
        var nodeID = GridWRTNodeID;
        if (typeof options.showAlternatives == "undefined") {options.showAlternatives = true};
        if (!options.showAlternatives) {nodeID = <% = PM.ActiveObjectives.Nodes(0).NodeID %>};
        if (typeof extra == "undefined") extra = {};
        extra["wrt"] = nodeID;
        extra["users"] = "selected";
        callAPI("pm/dashboard/?action=synthesize", extra, onLoadChartData);
    }

    function onLoadChartData(data) {
        if (typeof data == "object") {
            synthesize_data = data; 
            fillHierarchyPriorities(synthesize_data);
            if (initChartCompleted) { 
                updateChart(); 
            } else { 
                initChart(); 
            };
            updateToolbar();
            $("#toolbar").dxToolbar("instance").option("disabled", !hasData());
            resizePage();
        }
    }

    function fillHierarchyPriorities(synthesize_data) {
        var prts = (synthesize_data.hpriorities.length == 0 ? synthesize_data.priorities : synthesize_data.hpriorities);
        for (var i = 0; i < prts.length; i++) {
            var userPriorities = prts[i];
            var objs = userPriorities.objs;
            for (var j = 0; j < objs.length; j++) {
                var objPrty = objs[j];
                var obj = getObjByID(objPrty.oid, synthesize_data);
                if ((obj) && !obj.isCategory) {
                    obj["l" + userPriorities.uid] = objPrty.lprty;
                    obj["g" + userPriorities.uid] = (normalizationMode == "none" ? objPrty.uprty : objPrty.prty);
                };
            };
        };
    }

    function getObjByID(oid, synthesize_data) {
        var objs = synthesize_data.objs;
        for (var i = 0; i < objs.length; i++) {
            var obj = objs[i];
            if (obj.id == oid) return obj;
        };
        return null;
    }

    function loadChartOptions() {
        callAPI("pm/dashboard/?action=options", {}, onLoadChartOptions);
    }

    function onLoadChartOptions(data) {
        if (typeof data == "object") {
            options = data;
            var param = "<% = If(CurrentPageID = _PGID_ANALYSIS_CHARTS_ALTS, "altColumns", If(CurrentPageID = _PGID_ANALYSIS_CHARTS_OBJS, "objColumns", "")) %>";
            if ((param) && (param != "")) {
                if (typeof options.chartType != "undefined" && param.substring(0,3) == options.chartType.substring(0,3)) {
                    param = {'chartType': options.chartType};
                } else {
                    options.chartType = param;
                    if (initChartCompleted) $("#chart").ecChart("option", "chartType", param);
                    param = {'chartType': param};
                }
            };
            options.showAlternatives = (options.chartType.indexOf("alt") == 0 ? true : false);
            if (!options.showAlternatives && GridWRTNodeID >= 0) {
                if (GridWRTNodeIsTerminal) {
                    GridWRTNodeID = GridWRTNodePID;
                    options.WRTNodeID = GridWRTNodePID;
                    options.selectedNodeID = GridWRTNodePID;
                }
            }
            loadChartData(param);
        }
    }

    function initChart() {
        objectives = JSON.parse(JSON.stringify(synthesize_data.objs)); //synthesize_data.objs.slice(0);
        options.AlternativesFilterValue = AlternativesFilterValue;
        options.dataSource = synthesize_data;
        options.decimals = DecimalDigits;
        options.selectedNodeID = GridWRTNodeID;
        options.WRTNodeID = GridWRTNodeID;
        options.normalizationMode = normalizationMode;
        options.onNodeSelected = function (nodeid) {
            //$("#ddWRTNode").dxDropDownBox("instance").option("value",nodeid);
        };
        $("#chart").ecChart(options);        
        initChartCompleted = true;
    }

    function updateChart() {
        objectives = JSON.parse(JSON.stringify(synthesize_data.objs));
        options.dataSource = synthesize_data;
        $("#chart").ecChart("option", "dataSource", synthesize_data);
        $("#chart").ecChart("option", "selectedNodeID", options.selectedNodeID);
        $("#chart").ecChart("redraw");
    }

    function updatePager(totalCharts, selectedPage, totalPages, chartsPerPage) {
        var sp = Number(selectedPage);
        $(".dx-page-size").each(function() {
            var pgs = Number(this.id.replace("psize",""));
            if (pgs > totalCharts) {$(this).hide()} else {$(this).show()};
        });
        $("#spanChartsCount").html("Charts: " + totalCharts + ". Page #" + (sp + 1) + " of " + totalPages);
        $(".dx-page-size").removeClass("dx-selection");
        $("#psize" + chartsPerPage).addClass("dx-selection");
        $(".dx-page,.dx-separator").remove();
        var sep1 = false;
        var sep2 = false;
        for (var i = 0; i < totalPages; i++) {
            var pn = (i + 1);
            if ((pn == 1)||(pn == totalPages)||(Math.abs(i - sp) < 2)||(i < 4 && sp < 4)||(totalPages - i < 5 && sp > totalPages - 4)) {
                $(".dx-next-button").before('<div id="pg' + i + '" class="dx-page'+ (i == sp ? ' dx-selection': '') + '" role="button" style="min-width:12px;text-align:center;" title="Page ' + pn + '">' + pn + '</div>');
            } else {
                if (i > selectedPage && !sep1) {
                    $(".dx-next-button").before('<div class="dx-separator">...</div>');
                    sep1 = true;
                }
                if (i < selectedPage && !sep2) {
                    $(".dx-next-button").before('<div class="dx-separator">...</div>');
                    sep2 = true;
                }
            }
        };
        jQuery(".dx-page").click(function () {
            var pg = Number(this.id.replace("pg",""));
            setOption('selectedPage', pg);
        });
        if (sp == 0) { $("div.dx-prev-button").addClass("dx-button-disable") } else {$("div.dx-prev-button").removeClass("dx-button-disable")};
        if (sp == totalPages - 1) { $("div.dx-next-button").addClass("dx-button-disable") } else {$("div.dx-next-button").removeClass("dx-button-disable")};
    } 

    function initTitle(is_alts, selectedObj) {
        if (is_alts) {
            $("#lblPageTitleObjs").hide();
            $("#lblPageTitleAlts").show();
        } else {
            $("#lblPageTitleAlts").hide();
            $("#lblPageTitleObjs").show();
        }        
        if (typeof selectedObj !== "undefined") {
            if (typeof selectedObj.pid !== "undefined") {
                $("#lblPageTitleAlts").hide();                
            }
        }
    }

    function initSplitters() {
        $(".split_left").resizable({
            autoHide: false,
            handles: 'e',
            maxWidth: 600,
            resize: function(e, ui) {
                $("#divContent").hide();
                var parent = ui.element.parent();
                var divTwo = ui.element.next();
                divTwo.width(10);
                var remainingSpace = parent.width() - ui.element.outerWidth();
                var divTwoWidth = remainingSpace - (divTwo.outerWidth() - divTwo.width());
                if (divTwoWidth > 20) divTwo.width(divTwoWidth);
            },
            stop: function(e, ui) {
                treeSplitterSize = ui.element.outerWidth();
                sessionStorage.setItem("SynthesisSplitterSize<%=App.ProjectID%>", treeSplitterSize);
                //callAPI("pm/dashboard/?action=v_splitter_size", { "value" : treeSplitterSize }, function () { }, true);
                $("#divContent").show();

                resizePage();
            }
        });
        $(".ui-resizable-e").addClass("splitter_v");
    }

    toggleScrollMain();
    resize_custom = resizePage;
    $(document).ready(function () {
        initTitle(<%=Bool2JS(CurrentPageID = _PGID_ANALYSIS_CHARTS_ALTS)%>);
        initSplitters();
        if (typeof options == "undefined" || options == {}) {
            loadChartOptions(); 
        } else {
            onLoadChartOptions(options);
        }
        jQuery("div.dx-page-size").click(function () {
            var psize = this.id.replace("psize","") * 1;
            setOption('chartsPerPage', psize);
            updateToolbar();
        });
        jQuery("div.dx-prev-button").click(function () {
            var selPage = getOption("selectedPage");
            if (selPage > 0) {selPage -= 1};
            setOption('selectedPage', selPage);
        });
        jQuery("div.dx-next-button").click(function () {
            var selPage = getOption("selectedPage");
            var totalPages = $("#chart").ecChart("getTotalPages");;        
            if (selPage < totalPages - 1) {selPage += 1};
            setOption('selectedPage', selPage);
        });
        $("#divTree").width(treeSplitterSize);
    });


</script>
<div id="tabs" class="ec_tabs ec_tabs_nocontent" style="margin-top: 8px; width: 99%; display: none;"></div>
<div id="toolbar" class="dxToolbar" style="width:99%; margin-bottom:4px;"></div>
<div id='infobar' style="margin-bottom:-10px; margin-top: 5px; margin-left: 0px; margin-right: 0px;">
    <h5 id="pgHeader"><% = GetTitle() %>
        <div id="lblAdvFilter" style="text-align: center;"></div>
    </h5>
    <%If App.ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.ResourceAlignerEnabled, Nothing, True) Then%>
    <div id="tdScenarioName" class="note" <%=CStr(IIf(AlternativesFilterValue = -4, "", "style='display:none;'")) %>>
        <span>Funded <%=ParseString("%%Alternatives%%") %> of </span>
        <span><%=SafeFormString(App.ActiveProject.ProjectManager.ResourceAligner.Scenarios.ActiveScenario.Name)%></span>
    </div>
    <%End If%>
</div>
<div id="content" style="width:99%;">
    <table class="whole">
        <tr>
            <td style="vertical-align:top;">
                <div id="divTree" class="split_left splitter_left" style="float: left; border-spacing: 0px; height: 100%; display:<%=If(PM.Parameters.Synthesis_ObjectivesVisibility, "", "none")%>; width: 200px;">
                    <div id="treeList" class="dx-treelist-withlines dx-treelist-compact" style='margin-right: 2px;'></div>
                </div>
            </td>
            <td style="vertical-align:top;">
                <div id="chart"></div>
            </td>
        </tr>
    </table>
    <div class="dx-datagrid-pager dx-pager">
        <div class="dx-page-sizes">
            Charts per page:
            <div class="dx-page-size" role="button" title="Display 1 chart on page" id="psize1">1</div>
            <div class="dx-page-size" role="button" title="Display 2 charts on page" id="psize2">2</div>
            <div class="dx-page-size" role="button" title="Display 3 charts on page" id="psize3">3</div>
            <div class="dx-page-size" role="button" title="Display 4 charts on page" id="psize4">4</div>
            <div class="dx-page-size" role="button" title="Display 5 charts on page" id="psize5">5</div>
            <div class="dx-page-size" role="button" title="Display 6 charts on page" id="psize6">6</div>
            <div class="dx-page-size" role="button" title="Display 7 charts on page" id="psize7">7</div>
            <div class="dx-page-size" role="button" title="Display 8 charts on page" id="psize8">8</div>
            <div class="dx-page-size" role="button" title="Display 9 charts on page" id="psize9">9</div>
            <div class="dx-page-size" role="button" title="Display 10 charts on page" id="psize10">10</div>
        </div>
        <div class="dx-pages">
            <div class="dx-info">
                <span id="spanChartsCount"></span>
            </div>
            <div class="dx-navigate-button dx-prev-button" role="button" title="Previous page"></div>
<%--            <div class="dx-page" role="button" title="Page 1">1</div>
            <div class="dx-separator">. . .</div>
            <div class="dx-page" role="button" title="Page 16">16</div>
            <div class="dx-page" role="button" title="Page 17">17</div>
            <div class="dx-page" role="button" title="Page 18">18</div>
            <div class="dx-page dx-selection" role="button" title="Page 19">19</div>
            <div class="dx-page" role="button" title="Page 20">20</div>--%>
            <div class="dx-navigate-button dx-next-button" role="button" title="Next page"></div>
        </div></div>
</div>


<%-- Chart Export settings dialog --%>
<div id="dlgChartExport" style="display:none;">
    <div style="text-align: center; height: 100%; width: 100%;">
        <div style="text-align: center; width:100%;">
            <table style="height: 120px; width: 100%">
                <tr>
                    <td>
                        <div id="btnChartExportPNG"></div>
                    </td>
                    <td>
                        <div id="btnChartExportSVG"></div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div id="btnChartExportPDF"></div>
                    </td>
                    <td>
                        <div id="btnChartExportMultiPagePDF"></div>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <div id="btnChartExportCancel"></div>
                    </td>
                </tr>
            </table>
        </div>
    </div>
</div>

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

<%-- Filter Alternatives - Advanced --%>
<div id="divAltsAdvanced" style="display:none;position:relative; text-align:center; padding-top:30px; vertical-align:middle;">
    Select top <% = GetAltsNumDropdown()%>&nbsp;<%=ParseString("%%alternatives%%")%> based on <% = GetUsersDropdown()%> priorities
</div>

<%-- Filter Alternatives - Select/Deselect --%>
<div id="divAltsCustom" style="display:none;position:relative;">
    <div style="padding:5px; text-align:left;">
        <% = GetAltsCheckList()%>
    </div>
    <div id="pnlAllOrNoneSelector" style='float:left; margin-top:2ex; margin-right:250px;'>
        <a href="" onclick="filterAltsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterAltsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

<%-- Filter by alternative attributes --%>
<div id="divAltsAttributes" style="display:none;position:relative;">
    <div style="padding:5px; text-align:left;">
        <!-- Filter combinations toolbar -->
        <div style='text-align:center; margin-top:1ex' class='text'>
            <nobr>Use: <select id="cbFilterCombination">
                <option value='0'<% =IIf(FilterCombination = 0, " selected", "") %>>AND</option>
                <option value='1'<% =IIf(FilterCombination = 1, " selected", "") %>>OR</option>                
            </select> &nbsp;          
            <input type='button' class='button' style='width:11ex' id='btnAddRule' value='Add&nbsp;Rule' onclick='onAddNewRule(); return false;' />
            <input type='button' class='button' style='width:11ex' id='btnRest' value='Reset' onclick='onFilterReset(); return false;' /></nobr>
        </div>

        <p align="center" id='divFilters'>Loading...</p>
    </div>    
</div>

</asp:Content>