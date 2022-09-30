<%@ Page Language="VB" Inherits="ProjectParticipantsPage" title="Manage Participants" Codebehind="Users.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
    <script type="text/javascript" src="/Scripts/passfield.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">

    .pwd_input {
        width: 98%; 
        border: 0px; 
        margin: 3px 2px 2px 4px;
    }

    .pwd_warn {
        position: absolute;
        z-index: 3333;
    }

    .pwd_error {
    }

    .pwd_strength {
        height:2px; 
        width:100%; 
        margin-bottom:0px; 
        background: white;
        opacity: 0;
    }

    .dx-list .dx-empty-message {
        text-overflow: unset;
        white-space: normal;
    }

</style>
<script language="javascript" type="text/javascript">

    var pwd_id = "pwd";

    var isReadOnly = <% = Bool2JS(IsReadOnly)%>;
    var isWidget = <% = Bool2JS(IsWidget)%>;
    var isProjectUsers = <%=Bool2JS(CurrentPageID = _PGID_PROJECT_USERS)%>;
    var isWorkgroupUsers = <%=Bool2JS(CurrentPageID = _PGID_ADMIN_USERSLIST)%>;
    var isRiskUsers = <%=Bool2JS(CurrentPageID = _PGID_RISK_INVITE_USERS)%>;
    var ajaxMethod = _method_POST;
    //var evalProgressLoaded = <% = Bool2JS(evalProgress IsNot Nothing) %>;

    var wkg_admin = <%=App.ActiveWorkgroup.RoleGroupID(ecRoleGroupType.gtAdministrator, App.ActiveWorkgroup.RoleGroups) %>;

    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";

    var pagination = 20;

    //var last_click_cb = -1;
    var SEND_MAIL_MAX_USERS_COUNT = 100;
    var OPT_MESSAGE_HIGHLIGHT_TIMOUT = 3000;
    var is_context_menu_open = false;

    var IDX_ID = "id";
    var IDX_EMAIL = "email";
    var IDX_NAME = "name";
    var IDX_AHP_USER_ID = "ahp_id";
    var IDX_ROLE = "role";
    var IDX_PRIORITY = "priority";
    var IDX_HAS_DATA = "has_data";
    var IDX_PROGRESS = "progress";
    var IDX_DISABLED = "dis";
    var IDX_HAS_PSW = "has_psw";
    var IDX_LINK = "link";
    var IDX_GRP_ID = "grp_id";
    var IDX_PM = "pm";
    var IDX_CAN_EDIT = "can_edit";
    var IDX_IS_LINK_ENABLED = "link_enabled";
    var IDX_IS_LINKGO_ENABLED = "linkgo_enabled";
    var IDX_IS_PREVIEW_ENABLED = "preview_enabled";
    var IDX_LAST_JUDGMENT = "last_judg";
    var IDX_LAST_JUDGMENT_SORT = "last_judg_sort";
    var IDX_LOCKED = "locked";
    
    var TOTAL_DATA_FIELDS = 12; // starting from index 13 goes generated results groups data
    var OPT_BAR_WIDTH = 50;

    var COL_ACTIONS = 9;

    var IDX_RES_GRP_ID = 0;
    var IDX_RES_GRP_NAME = 1;
    var IDX_RES_GRP_USERS = 2;
    var IDX_RES_GRP_HAS_RULE = 3; // is group dynamic

    var table_main_id = "tableStructureUsers";
    var columns_ver = "220117";
    //var table_main;
    var table_wg;

    var IDX_WG_USER_ID = "id";
    var IDX_WG_USER_EMAIL = "email";
    var IDX_WG_USER_NAME = "name";
    var IDX_WG_USER_ROLE_ID = "role_id";
    var IDX_WG_USER_ROLE_NAME = "role_name";
    var IDX_WG_USER_DECISIONS = "decisions";
    var IDX_WG_USER_DISABLED = "dis";
    var IDX_WG_USER_LAST_VISITED = "last_visited";
    var IDX_WG_USER_CHECKED = "checked";
    var IDX_WG_HAS_PSW = "has_psw";

    var COL_WG_USER_CHK = 0;
    var COL_WG_USER_EMAIL = 1;
    var COL_WG_USER_NAME = 2;
    var COL_WG_USER_ROLE = 3;
    var COL_WG_USER_DECISIONS = 4;
    var COL_WG_USER_DISABLED = 5;
    var COL_WG_USER_LAST_VISITED = 6;
    var COL_WG_USER_ACTIONS = 7;

    //var cur_order = [IDX_NAME, "asc"];
    //var cur_edit = -1;

    var users_data = <%=GetUsersData()%>;   // participants 
    var groups_data = <%=GetGroupsData()%>; // permission groups
    var attr_user_flt = [<%=GetUsersFilterData()%>];    // current filters list
    var res_groups_data = <%=GetResultGroupsData()%>;  // combined (results) groups

    var activeUserID = <%=If(UsersList.Count > 0, UsersList.Where(Function (usr) usr.UserEmail.ToLower = App.ActiveUser.UserEmail.ToLower)(0).UserID, App.ActiveUser.UserID)%>;

    function users_count() { return users_data.length; }
    function res_groups_count() { return res_groups_data.length; }

    var aidx_id = 0;
    var aidx_guid = 1;
    var aidx_name = 2;
    var aidx_type = 3;
    var aidx_def_val = 4;
    var aidx_can_attr_be_deleted = 5;
    var aidx_attr_readonly = 6;

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

    var oper_list = ["Contains", "Equal", "Not Equal", "Starts With", "Greater Than", "Greater Than Or Equal", "Less Than", "Less Than Or Equal", "Is True", "Is False"];
    var oper_available = [[0,1,2,3],
                          [8,9],
                          [1,2,4,5,6,7],
                          [1,2,4,5,6,7],
                          [1,2],
                          [0,1,2]];

    var cancelled = false;
    var added_rows = 0;
    var user_link = "";
    var storedCheckedUsers = [];
    
    var COMBINED_ALL = -1;
    var DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID = "<%=DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID.ToString%>";

    var on_hit_enter = "";    
    var drag_index = -1;
    var drag_order = "";

    var idx_to_save = -1;
    var data_to_save = "";

    <% If CurrentPageID = _PGID_PROJECT_USERS Then %>
    function onGetReport(addReportItem) {
        if (typeof addReportItem == "function") {
            addReportItem({"name": "<% =JS_SafeString(PageTitle(CurrentPageID)) %>", "type": "<% =JS_SafeString(ecReportItemType.Participants.ToString) %>", "edit": document.location.href});
        }
    }<% End if %>

    //function cbUserClick(e, chk, uid) {
    //    // process Shift + Click
    //    if ((e.shiftKey) && e.shiftKey && last_click_cb >= 0 && last_click_cb != uid) {
    //        var start_id = "cb_user_" + last_click_cb;
    //        var stop_id = "cb_user_" + uid;
    //        var is_checking = false;
    //        $("input.cb_user").each(function() {
    //            var is_found = this.id == start_id || this.id == stop_id;
    //            if (is_found && !is_checking) {
    //                is_checking = true;
    //            } else {
    //                if (is_found && is_checking) {
    //                    this.checked = chk;
    //                    is_checking = false;
    //                }
    //            }
    //            if (is_checking) this.checked = chk;
    //        });            
    //    }
        
    //    last_click_cb = uid;

    //    updateToolbar();
    //}

    function getCheckedUsersCount() {        
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var checked_count = table_main.getSelectedRowKeys().length;
            //if (checked_count == 0) checked_count = table_main.option("focusedRowIndex") > -1 ? 1 : 0;
            return checked_count;
        } else {
            return 0;
        }
    }

    function getCheckedUsersEmailsArr() {        
        var retVal = [];
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var checked_rows = table_main.getSelectedRowKeys();
            var checked_count = checked_rows.length;
            //if (checked_count == 0) {
            //    if (table_main.option("focusedRowIndex") > -1) {
            //        retVal.push(table_main.option("focusedRowKey"));
            //    }
            //} else {
                retVal = checked_rows;
            //}
        }
        return retVal;
    }

    function updateToolbar() {
        var n = getCheckedUsersCount();
                       
        $("#btnEdit").prop("disabled", isReadOnly); //todo //$("#btnEdit").prop("disabled", n == 0);
        if ($("#btn_remove").data("dxDropDownButton")) $("#btn_remove").dxDropDownButton("instance").option("disabled", !users_data || users_data.length == 0 || isReadOnly);
        var btn_dis = $("#btn_disable"); if ((btn_dis) && btn_dis.data("dxButton")) btn_dis.dxButton("instance").option("disabled", (n == 0) || isReadOnly);
        var btn_en = $("#btn_enable"); if ((btn_en) && btn_en.data("dxButton")) btn_en.dxButton("instance").option("disabled", (n == 0) || isReadOnly);
        if ($("#btn_permission").data("dxDropDownButton")) $("#btn_permission").dxDropDownButton("instance").option("disabled", (n == 0) || isReadOnly); //$("#btnRoles").prop("disabled", n == 0);
        //$("#btnExport").prop("disabled", n == 0);        
        if ($("#btn_send_mail").data("dxButton")) $("#btn_send_mail").dxButton("instance").option("disabled", n == 0); //$("#btnSendMails").prop("disabled", n == 0);
        if ($("#btn_erase_psw").data("dxButton")) $("#btn_erase_psw").dxButton("instance").option("disabled", n == 0);

        //// set Check All checkbox state (on|off|indeterminate)
        //var chk_all = document.getElementById("chk_all");
        //if ((chk_all)) {
        //    var all_count = $("input.cb_user").length;
        //    var chk_count = n;
        //    if (chk_count == 0) {
        //        chk_all.checked = false;
        //        chk_all.indeterminate = false;
        //    } else {                
        //        if (all_count > 0 && all_count === chk_count) {
        //            chk_all.indeterminate = false;
        //            chk_all.checked = true;
        //        } else {
        //            chk_all.indeterminate = true;
        //        }
        //    }
        //}

    }

    function getProjectUsersColumns(hasState) {
        //var checkboxColumnCellTemplate = function (component, columnIndex, column) {
        //    return component.append(getCheckbox("chk_all", 0, 1, "cbAllClick(this.checked, 'cb_user');", "", "cb_all"));
        //};       
        var columns = [];
        //columns.push({ "caption": "", headerCellTemplate: checkboxColumnCellTemplate, "alignment": "center", "width": "32px", "allowSorting": false, "allowSearch": false, "allowExporting": false });
        columns.push({ "caption": "<%=ResString("tblUserEmail")%>", "alignment": "left", "allowSorting": true, "allowSearch": true, sortIndex: 0, sortOrder: 'asc', "dataField" : IDX_EMAIL, "allowEditing": false, "fixed" : true,
            <% If Not IsWidget Then %>                
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                $("<span></span>").addClass("nu" + (data[IDX_DISABLED] ? " grey-text" : "")).text(data[IDX_EMAIL]).appendTo(cellElement); 
            }
            <% End If %>
        });
        columns.push({ "caption": "<%=ResString("tblSyncUserName")%>", "alignment": "left", "allowSorting": true, "allowSearch": true, "dataField" : IDX_NAME, "allowEditing": true, "fixed" : false });
        columns.push({ "caption": "<%=ResString("tblUserRole")%>", "alignment": "center", "width": "120px", "dataType": "string", "allowSorting": true, "allowSearch": false, "dataField" : IDX_ROLE, "allowEditing": false,
            <% If Not IsWidget Then %>
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                $("<span></span>").prop("id", "usrRole" + data[IDX_ID]).html(htmlEscape(getRoleNameByID(data[IDX_GRP_ID]))).appendTo(cellElement); 
            }
            <% End If %>
        });
        <% If Not IsWidget Then %>
        columns.push({ "caption": "<%=ResString("tblUserWeight")%>", "alignment": "center", "width": (OPT_BAR_WIDTH * 2) + "px", "format": "percent", "dataType": "number", "allowSorting": true, "allowSearch": false, "dataField" : IDX_PRIORITY, "allowEditing": true,
            editorOptions: {
                disabled: isReadOnly,
                format: "percent",
                max: 1,
                min: 0,
                mode: "number",
                showClearButton: false,
                showSpinButtons: true,
                step: 0.1,
                useLargeSpinButtons: false
            }
        });
        if (!hasState) columns[columns.length - 1].visible = false;
        
        var eval_columns = [];
        eval_columns.push({ "caption": "<%=ResString("tblUserHasData")%>", "alignment": "center", "width": "85px", "dataType": "string", "allowSorting": true, "allowSearch": false, "dataField" : IDX_HAS_DATA, "allowEditing": false,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                <% If Not IsWidget Then %>
                $("<span></span>").html(data[IDX_HAS_DATA] ? "<% = JS_SafeString(ResString("lblYes")) %>" : "").appendTo(cellElement); 
                <% End If %>
            }
        });
        eval_columns.push({ "caption": "<%=ResString("tblEvaluationProgress")%>", "alignment": "left", "width": 160, "dataType": "string", "allowSorting": true, "allowSearch": false, "dataField" : IDX_PROGRESS, "allowEditing": false });
        eval_columns.push({ "caption" : "<%=ResString("tblLastJudgmentTime")%>", "alignment" : "left", "width" : "160px", "allowSorting" : true, "allowSearch" : false, "dataField" : IDX_LAST_JUDGMENT, "dataType": "date", format:'shortDateShortTime', "allowEditing": false });
        columns.push({"caption" : "Evaluation Status", "alignment" : "center", "columns": eval_columns, headerCellTemplate : function (columnHeader, headerInfo) {
            $("<span></span>").html(headerInfo.column.caption).appendTo(columnHeader);
            $("<div></div>").prop("id", "btnLoadEvalProgress").css({ "display" : "inline-block", "margin-left" : "10px" }).dxButton({
                //text: evalProgressLoaded ? "<% = JS_SafeString(ResString("btnReload")) %>" : "Load", 
                hint: "Reload evaluation progress",
                icon: "refresh",
                onClick : function (e) {
                    sendCommand("action=refresh_full&load_progress=true");
                    //evalProgressLoaded = true;
                }
            }).appendTo(columnHeader);
        }});

        columns.push({ "caption" : "Last Judgment Sort", "alignment" : "left", "width" : "0px", "allowSorting" : true, "allowSearch" : false, "dataField" : IDX_LAST_JUDGMENT_SORT, "visible" : false, "allowExporting": false, "allowEditing": false, "showInColumnChooser": false });
        columns.push({ "caption": "<%=ResString("tblUserDisabled")%>", "alignment": "center", "width": "90px", "dataType": "boolean", "allowSorting": true, "allowSearch": false, "dataField" : IDX_DISABLED, "allowEditing": true });
        columns.push({ "caption": "<%=ResString("tblActions")%>", "alignment": "center", "width": "100px", "dataType": "string", "allowSorting": false, "allowSearch": false, "allowExporting": false, "allowEditing": false,
            cellTemplate: function (cellElement, cellInfo) {
                var actions = "";
                var data = cellInfo.data;
                var isAdmin = ((data["adm"]));
                <% If Not IsWidget Then %>
                var clr = <% =If(App.isEvalURL_EvalSite, "((data[IDX_IS_PREVIEW_ENABLED]) ? ' style=\""color:#38761d\""' : '')", "''") %>;
                actions += ((data[IDX_LOCKED]) ? "<a href=''  onclick='userUnlock(\""+data[IDX_EMAIL]+"\"); return false;' style='margin:0px 2px;' title='<% = JS_SafeString(SafeFormString(ResString("btnUserUnlock"))) %>'><i class='fas fa-user-lock' style='color: darkred'></i></a>" : "");
                <% If Not isSSO() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %>actions += ((!isAdmin<% = If(App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.CannotBeDeleted, " || true", "")%>) ? "<a href='' onclick='resetPsw(\"" + data[IDX_ID] + "\"); return false;' style='margin:0px 2px;' title='<% = JS_SafeString(ResString("lblPasswordRestore")) %>'><i class='fas fa-key' " + ((data[IDX_HAS_PSW] ? "style='color: orange;'" : "")) + " id='imgPsw" + data[IDX_ID] + "' ></i></a>" : "<i class='fas fa-key' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");<% End if %>
                <% If CurrentPageID = _PGID_RISK_INVITE_USERS Then %>                    
                actions += (data[IDX_IS_LINK_ENABLED] ? "<a href='' onclick='getUserLink(\"" + data[IDX_EMAIL] + "\", \"get_controls_link\", \"iconCtrl" + data[IDX_ID] + "\", true, \"fas fa-link\"); return false;' style='margin:0px 2px;' title='<% = JS_SafeString(SafeFormString(ResString("btnGetAnytimeEvalLink"))) %>' id='iconCtrl" + data[IDX_ID] + "'><i class='fas fa-link'></i></a>" : "<i class='fas fa-link' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");
                <% Else %>
                <% if Not isSSO() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %>actions += (data[IDX_IS_LINK_ENABLED] ? "<a href=''  onclick='getUserLink(\"" + data[IDX_EMAIL] + "\",\"get_link\", \"iconLink" + data[IDX_ID] + "\"); return false;' style='margin:0px 2px;' title='<% = JS_SafeString(SafeFormString(ResString("btnGetAnytimeEvalLink"))) %>' id='iconLink" + data[IDX_ID] + "'><i class='fas fa-link'" + clr + "></i></a>" : "<i class='fas fa-link' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");
                actions += (data[IDX_IS_LINKGO_ENABLED] && !data[IDX_LOCKED] ? "<a href='' onclick='getUserLink(\"" + data[IDX_EMAIL] + "\",\"open_link\", \"iconOpen" + data[IDX_ID] + "\"); return false;' style='margin:0px 2px;' title='<%=SafeFormString(ResString("btnGetAndOpenAnytimeEvalLink"))%>' id='iconOpen" + data[IDX_ID] + "'><i class='fas fa-sign-in-alt'" + clr + "></i></a>" : "<i class='fas fa-sign-in-alt' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");<% End If %>
                actions += (data[IDX_IS_PREVIEW_ENABLED] ? "<a href='' onclick='" + (data[IDX_IS_LINKGO_ENABLED] ? "getPreview" : "getPreviewCore") + "(" + data[IDX_ID] + ", \"" + data[IDX_EMAIL] + "\", \"viewPipe" + data[IDX_ID] + "\"); return false;' style='margin:0px 2px;' title='<%=SafeFormString(ResString("lblEvalProgressHintUser"))%>' id='viewPipe" + data[IDX_ID] + "'><i class='fas fa-eye'></i></a>" : "<i class='fas fa-eye' style='filter:alpha(opacity=50); opacity:0.5;'></i>&nbsp;");
                <% End If %>
                <% End If %>
                $("<div></div>").html(actions).appendTo(cellElement); 
            }
        });

        // init column headers for attributes
        var attr_columns = [];
        for (var i = 0; i < attributes_data.length; i++) {
            var a = attributes_data[i];
            var s_align = "center";
            var s_data_type = "string";
            switch (a[aidx_type]) {
                case avtString:
                    s_align = "left";
                    s_data_type = "string";
                    break;
                case avtDouble:
                case avtLong:
                    s_align = "right";
                    s_data_type = "number";
                    break;
                case avtBoolean:
                    s_align = "center";
                    s_data_type = "boolean";
                    break;
            }
            attr_columns.push({ "caption": htmlEscape(a[aidx_name]), "alignment": s_align, "dataType": s_data_type, "dataField": "v" + i, "allowSorting": true, "allowSearch": false, "allowEditing": !isReadOnly && !a[aidx_attr_readonly], cssClass: a[aidx_attr_readonly] ? "attr_cell_readonly" : "", "width" : "110px", "allowResizing" : false });
            //for (var k = 0; k < users_data.length; k++ ) { users_data[k].push(""); }
        }
        if (attr_columns.length > 0) {
            columns.push({"caption" : "Attributes", "columns": attr_columns, "allowResizing" : false});
            if (!hasState) columns[columns.length - 1].visible = false;
        }

        // init column headers for results groups
        var groups_columns = [];
        for (var i = 0; i < res_groups_data.length; i++) {
            var g = res_groups_data[i];
            // getCheckbox("chk_all_grp_" + g[IDX_RES_GRP_ID], 0, 1, "cbAllClick(this.checked, 'cb_grp_'" + g[IDX_RES_GRP_ID] + "'); return false;", "", "cb_all_grp_" + g[IDX_RES_GRP_ID]) + 
            groups_columns.push({ "caption": htmlEscape(g[IDX_RES_GRP_NAME]), "alignment": "center", "dataType": "boolean", "dataField": "g" + i, "allowSorting": true, "allowSearch": false, "allowEditing": !isReadOnly && g[IDX_RES_GRP_HAS_RULE] == 0, cssClass : g[IDX_RES_GRP_HAS_RULE] == 0 ? "" : "group_cell_disabled", "width" : "110px", "allowResizing" : false });
            //for (var k = 0; k < users_data.length; k++ ) { users_data[k].push(""); }
        }
        
        columns.push({"caption" : "Groups", "columns": groups_columns, "allowResizing" : false});
        if (!hasState) columns[columns.length - 1].visible = true;

        <% End If %>
        return columns;
    }

    function getWorkgroupUsersColumns() {
        //var checkboxColumnCellTemplate = function (component, columnIndex, column) {
        //    return component.append(getCheckbox("chk_all", 0, 1, "cbAllClick(this.checked, 'cb_user');", "", "cb_all"));
        //};
        var columns = [];
        //columns.push({ "caption": "", headerCellTemplate: checkboxColumnCellTemplate, "alignment": "center", "width": "32px", "dataType": "string", "allowSorting": true, "allowSearch": false, "dataField" : IDX_WG_USER_CHECKED, "allowExporting" : false });
        columns.push({ "caption": resString("tblUserEmail"), "alignment": "left", "dataType": "string", "allowSorting": true, "allowSearch": true, sortIndex: 0, sortOrder: 'asc', "dataField" : IDX_WG_USER_EMAIL, "allowEditing": false, "fixed" : true,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                $("<span></span>").html(data[IDX_WG_USER_EMAIL]).css(data[IDX_WG_USER_DISABLED] ? { "color" : "#a0a0a0" } : {}).appendTo(cellElement); 
            }
        });
        columns.push({ "caption": resString("tblSyncUserName"), "alignment": "left", "dataType": "string", "allowSorting": true, "allowSearch": true, "dataField" : IDX_WG_USER_NAME, "allowEditing": true, "fixed" : false });
        columns.push({ "caption": resString("tblUserRole"), "alignment": "center", "width": "150px", "dataType": "string", "allowSorting": true, "allowSearch": false, "dataField" : IDX_WG_USER_ROLE_NAME, "allowEditing": false, "allowResizing": false,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                $("<span></span>").prop("id", "usrRole" + data[IDX_WG_USER_ID]).html(htmlEscape(data[IDX_WG_USER_ROLE_NAME])).appendTo(cellElement); 
            }
        });
        columns.push({ "caption": resString("lblUserProjectsCount"), "alignment": "center", "width": "100px", "type": "numeric", "allowSorting": true, "allowSearch": false, "dataField" : IDX_WG_USER_DECISIONS, "allowEditing": false, "allowResizing": false,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                $("<span></span>").html(data[IDX_WG_USER_DECISIONS] <= 0 ? data[IDX_WG_USER_DECISIONS] : "<a href='' onclick='showWGUserDecisions(" + data[IDX_WG_USER_ID] + "); return false;' class='actions' style='cursor: pointer;'>" + data[IDX_WG_USER_DECISIONS] + "</a>").appendTo(cellElement); 
            }
        });
        columns.push({ "caption": resString("tblUserDisabled"), "alignment": "center", "width": "90px", "dataType": "boolean", "allowSorting": true, "allowSearch": false, "dataField" : IDX_WG_USER_DISABLED, "allowEditing": true, "allowResizing": false });
        columns.push({ "caption" : resString("tblLastVisited"), "alignment" : "center", "width" : "170px", "allowSorting" : true, "allowSearch" : false, "dataField" : IDX_WG_USER_LAST_VISITED, "allowEditing": false, dataType: "datetime" });
        <% if Not isSSO() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %>columns.push({ "caption": resString("tblActions"), "alignment": "center", "width": "100px", "dataType": "string", "allowSorting": false, "allowSearch": false, "allowExporting": false, "allowEditing": false, "allowResizing": false,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                var actions = "";
                var isAdmin = data[IDX_WG_USER_ROLE_ID] == wkg_admin;
                <% if Not isSSO() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %>if (!isAdmin<% = If(App.ActiveUser IsNot Nothing AndAlso App.ActiveUser.CannotBeDeleted, " || true", "")%>) {
                    actions += "<a href='' onclick='resetPsw(" + data[IDX_WG_USER_ID] + "); return false;' style='margin:0px 2px;'><i class='fas fa-key' " + ((data[IDX_WG_HAS_PSW] ? "style='color: orange;'" : "")) + " id='imgPsw" + data[IDX_WG_USER_ID] + "'></i></a>";
                }
                if (!isAdmin) {
                    actions += "<a href='' onclick='getUserLink(\""+data[IDX_WG_USER_EMAIL]+"\",\"get_link\", this); return false;' style='margin:0px 2px;'><i class='fas fa-link'></i></a>";
                }<% End If %>
                $("<span></span>").html(actions).appendTo(cellElement); 
            }            
        });<% End if%>

        return columns;
    }

    function initTable(params) {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { table_main.dispose(); }

        var storageKey = (isWorkgroupUsers ? "WKG" : "PRJ") + "UsersList_" + (isWorkgroupUsers ? "" : "_PRJID_<% = App.ProjectID %>") + columns_ver;
        var hasState = typeof localStorage.getItem(storageKey) !== "undefined" && localStorage.getItem(storageKey) != null;

        //init columns headers                
        var columns;
        if (isProjectUsers || isRiskUsers) columns = getProjectUsersColumns(hasState);
        if (isWorkgroupUsers) columns = getWorkgroupUsersColumns();
        
        $("#" + table_main_id).dxDataGrid({
            dataSource: users_data,
            hoverStateEnabled: true,
            focusedRowEnabled: false,
            //focusedRowKey: users_data[0][IDX_EMAIL],
            keyExpr: "email",
            columns: columns,
            searchPanel: {
                visible: true,
                text: "",
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            columnHidingEnabled: false,
            allowColumnResizing: true,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            allowColumnReordering: false,
            columnFixing: {
                enabled: false
            },            
            columnChooser: {                
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: !isWorkgroupUsers
            },
            editing: {
                mode: "cell",
                //allowUpdating: (isProjectUsers || isRiskUsers) && !isReadOnly
                allowUpdating: !isReadOnly
            },            
            onCellPrepared: function(e) {
                if (e.rowType === "data" && e.column.dataField == IDX_DISABLED && !e.data[IDX_CAN_EDIT]) {
                    e.cellElement.empty();
                }
            //    if (e.rowType === "header" && e.column.dataField !== IDX_NAME && e.column.dataField !== IDX_EMAIL && e.column.dataField !== IDX_WG_USER_NAME && e.column.dataField !== IDX_WG_USER_EMAIL) {
            //        e.cellElement.on("click", function(args) {
            //            var sortOrder = e.column.sortOrder;
            //            if (!e.column.type && sortOrder == undefined) {
            //                e.component.columnOption(e.column.index, "sortOrder", "desc");
            //                args.preventDefault();
            //                args.stopPropagation();
            //            }
            //        });
            //    }
            },
            onFocusedRowChanged: function (e) {
                updateToolbar();
            },
            onRowUpdated: function (e) {
                idx_to_save += 1;
                var row_to_save = "key_" + idx_to_save + "=" + e.key;
                var vals = Object.keys(e.data);
                for (var i = 0; i < vals.length; i++) {
                    row_to_save += "&" + vals[i] + "_" + idx_to_save + "=" + encodeURIComponent(e.data[vals[i]]);
                }
                data_to_save += (data_to_save == "" ? "" : "&") + row_to_save;
                setTimeout("saveChanges(" + data_to_save.length + ");", 10);                
            },
            pager: {
                allowedPageSizes: [10, 15, 20, 50, 100, 200, 500],
                showInfo: true,
                showNavigationButtons: true,
                showPageSizeSelector: true,
                visible: true
            },
            paging: {
                enabled: true,
                //pageSize: 10
            },
            rowAlternationEnabled: false,
            showColumnLines: true,
            showRowLines: false,
            showBorders: true,
            "export": {
                enabled: true,
                fileName: isWorkgroupUsers ? "WorkgroupParticipants" : "ProjectParticipants"
            },
            selection: {
                mode: "multiple",
                allowSelectAll: true,
                showCheckBoxesMode: "always"
            },
            onEditingStart: function(e){
                
            },
            onSelectionChanged: function (e) {
                updateToolbar();
            },
            loadPanel: main_loadPanel_options,
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: storageKey
            },
            sorting: {
                mode: "multiple"
            },
            onEditorPrepared: function(options) {
                if (isWorkgroupUsers && (options.row) && (options.index) && (options.row.rowType=="data") && (options.row.data) && (typeof options.dataField!="undefined" && (options.dataField==IDX_WG_USER_DISABLED || options.row.data[IDX_WG_USER_ID] == activeUserID))) {
                    if (options.row.data[IDX_WG_USER_ROLE_ID] != wkg_admin && options.row.data[IDX_WG_USER_ID] != activeUserID) {
                        //options.editorElement.dxCheckBox("option", "onValueChanged", function(e) {
                        //    disableWGUser(options.row.data[IDX_WG_USER_EMAIL], e.value);
                        //});
                    } else {
                        //options.editorElement.dxCheckBox("option", "visible", false);
                        options.cancel = true;
                    }
                }
                if ((isProjectUsers || isRiskUsers) && (options.row) && (options.index) && (options.row.rowType=="data") && (options.row.data) && (typeof options.dataField!="undefined" && options.dataField==IDX_DISABLED)) {
                    if (!options.row.data[IDX_CAN_EDIT] || options.row.data[IDX_ID] == activeUserID) {                        
                        //options.editorElement.dxCheckBox("option", "visible", false);
                        options.cancel = true;
                    }
                }
            },
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items; 
                toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: '<h6 style="padding-top: 10px;">' + (isWorkgroupUsers ? 'Workgroup Participants' : 'Participants') + '</h6>' });
            },
            onRowPrepared: function (e) {
                if (e.rowType != "data") return;
                var row = e.rowElement;
                var data = e.data;
                if (isWorkgroupUsers) {
                    var isAdmin = data[IDX_WG_USER_ROLE_ID] == wkg_admin;
                    
                    if ((data[IDX_WG_USER_DISABLED])) { $('td:lt(' + COL_WG_USER_ACTIONS + ')', row).css({ 'color': '#a0a0a0' }); };
                    if ((data[IDX_WG_USER_ID] == activeUserID)) { $('td:lt(' + COL_WG_USER_ACTIONS + ')', row).css({ 'font-weight': 'bold' }); };                    
                }
                if (isProjectUsers || isRiskUsers) {
                    if ((data[IDX_DISABLED])) { 
                        $(row).css({ 'color': '#a0a0a0' }); 
                    }
                    if ((data[IDX_PM])) {
                        $('td', row).css({ 'font-weight': 'bold' }); 
                    }                    
                }
            }
        });

        resize_custom_(false);

        displayLoadingPanel(false);       
        updateToolbar();

        setTimeout(function () { 
            if (users_count() > 0 && !isWorkgroupUsers) {
                var dg = $("#" + table_main_id).dxDataGrid("instance");
                var cell = dg.getCellElement(0, 0);
                dg.focus(cell);  
            }
        }, 500);
    }

    function saveChanges(prev_data_to_save_length) {
        if (prev_data_to_save_length == data_to_save.length) {
            // save changes
            sendCommand("action=save_changes&n=" + (idx_to_save + 1) + "&" + data_to_save, false);
            data_to_save = "";
            idx_to_save = -1;
        }
    }

    function getRoleNameByID(gid) {
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][0] == gid) return groups_data[i][1];            
        }
        return "";
    }

    function getResGroupByID(gid) {
        for (var i = 0; i < res_groups_data.length; i++) {
            if (res_groups_data[i][IDX_RES_GRP_ID] == gid) return res_groups_data[i];            
        }
        return null;
    }

    //function isUserInResGroup(g, uid) {
    //    for (var i = 0; i < g[IDX_RES_GRP_USERS].length; i++) {
    //        if (g[IDX_RES_GRP_USERS][i] == uid) return true;
    //    }
    //    return false;
    //}

    //function getUserAttrValue(a, uid) {
    //    for (var i = 0; i < a[aidx_user_vals].length; i++) {
    //        if (a[aidx_user_vals][i][0] == uid) return a[aidx_user_vals][i][1];
    //    }
    //    var def_val = (a[aidx_vals].length > 0 ? a[aidx_vals][0] : "");
    //
    //    return def_val;
    //}

    function getUserById(uid) {
        for (var i = 0; i < users_data.length; i++) {
            if (users_data[i][IDX_ID] == uid) return users_data[i];
        }
        return null;
    }

    function getUserByEmail(email) {
        for (var i = 0; i < users_data.length; i++) {
            if (users_data[i][IDX_EMAIL] == email) return users_data[i];
        }
        return null;
    }

    //function cbResGroupChecked(gid, uid, chk) {
    //    var u = getUserById(uid*1);
    //    if ((u)) {
    //        for (var i = 0; i < res_groups_data.length; i++) {
    //            if (res_groups_data[i][IDX_RES_GRP_ID] == gid) {
    //                u[TOTAL_DATA_FIELDS + 1 + i] = chk ? "1" : "0";
    //            }
    //        }
    //    }
    //    _ajax_ok_code = "onResGroupChecked(" + gid + "," + uid + "," + chk + ", data);";
    //    _ajaxSend("action=user_res_group_chk&gid=" + gid + "&uid=" + uid + "&chk=" + (chk ? "1" : "0"));
    //}

    //function onResGroupChecked(gid, uid, chk, data) {
    //    if (data != 'error') {
    //        var g = getResGroupByID(gid);
    //        if ((g)) {
    //            // remove the uid
    //            var index = g[IDX_RES_GRP_USERS].indexOf(uid);
    //            if (index > -1) g[IDX_RES_GRP_USERS].splice(index, 1);
    //            // update the
    //            if (chk) g[IDX_RES_GRP_USERS].push(uid);
    //        }
    //    }
    //    refreshGrid();
    //}

    function refresh_full() {
        sendCommand("action=refresh_full");
    }

    function refreshGrid() {
        $("#" + table_main_id).dxDataGrid("instance").option("dataSource", users_data);
        $("#" + table_main_id).dxDataGrid("refresh"); 
    }

    function resetGrid() {
        $("#" + table_main_id).dxDataGrid("instance").state({});
    }

    //function onPriorityKeyUp(c) {
    //    var v = validFloat(c.value);
    //    if (v) { x = str2double(c.value); v = x >= 0 && x <= 1; }
    //    c.style.color = (c.value != "" && !v ? "#ff0000" : "");
    //    if (v) return true;
    //    return false;
    //}
    
    //var val_old = "";
    //var last_focus = "";
    //var on_priority_ok_code = "";

    //function onPriorityBlur(c, uid) {
    //    if ((c) && (((val_old == "" || c.value == "") && c.value != val_old) || (str2double(c.value) != str2double(val_old)))) {
    //        var v = validFloat(c.value);
    //        if (v) { x = str2double(c.value); v = x >= 0 && x <= 1; }
    //        if (v && (last_focus == c.id)) {
    //            //last_focus = ""; 
    //            on_priority_ok_code = "if (last_focus != '') { $('#" + last_focus + "').focus().select(); }"; 
    //            sendCommand("action=set_user_priority&uid=" + uid + "&lst=" + getCheckedUsersEmails(table_main_id, users_count()) + "&val=" + c.value);
    //        }
    //    }
    //}

    //function onPriorityFocus(id, val) {
    //    setTimeout('val_old = "' + val + '"; last_focus = "' + id + '"; $("#' + id + '").select();', 1);
    //}

    function addBar(tVal, fBarWidth) {
        if (tVal > 1) tVal = 1;
        if (tVal < 0) tVal = 0;
        var L = Math.floor(tVal * fBarWidth);
        var blnk = "<%=BlankImage%>";
        var sImg = "<img src='" + blnk + "' width='" + L + "' height='2' title='' border='0'>";
        var sImg2 = "<img src='" + blnk + "' width='" + (fBarWidth - L) + "' height='2' title='' border='0'>";
        return L <= 0 ? "" : "<span style='display:inline-block; line-height:2px; height:2px; width:" + L + "px; border-bottom:2px solid #8899cc; margin-top:-2px; vertical-align: top;'>" + sImg + "</span>" + (L < fBarWidth ? "<span style='display:inline-block; line-height:2px; height:2px; width:" + (fBarWidth - L) + "px; border-bottom:1px solid #d0d0d0; margin-top:-2px; vertical-align: top;'>" + sImg2 + "</span>" :"");
    }

    function onManageAttributesDlg() {
        notImplemented();
    }

    function resetPsw(uid) {
        var usr = getUserById(uid);
        if ((usr) && (usr[IDX_CAN_EDIT])) {

            $("#popupContainer").dxPopup({
                width: dlgMaxWidth(350),
                height: "auto",
                title: "<%=JS_SafeString(ResString("lblPasswordReset"))%>",
                toolbarItems: [{
                    widget: 'dxButton', 
                    options: {
                        elementAttr: {id: "btnResetPassOK"},
                        text: "<% =JS_SafeString(ResString("btnOK"))%>", 
                        //icon: "fas fa-check",
                        onClick : function () {
                            var psw = $("#" + pwd_id);
                            if ((psw) && (psw.length)) {
                                if (psw.val() == "") {
                                    var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgBlankPswConform")) %>", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
                                    result.done(function (dialogResult) {
                                        if (dialogResult) {
                                            confResetPsw(usr[IDX_EMAIL], uid, psw.val());
                                        }
                                    });
                                } else {
                                    confResetPsw(usr[IDX_EMAIL], uid, psw.val());
                                }
                            }
                            return false;
                        }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }, {
                    widget: 'dxButton', 
                    options: { 
                        width: 70,
                        text: "<%=JS_SafeString(ResString("btnClose"))%>",
                        onClick: function() {
                            closePopup(); 
                            return false; 
                        } 
                    },
                    toolbar: "bottom",
                    location: 'after'
                }],
                contentTemplate: function () {
                    return $("<div style='margin-bottom:1ex'>" + ("<% = ResString("msgCreatePassword") %>".format("<nobr><b>" + usr[IDX_EMAIL] + "</b></nobr>")) + ":</div><input type='hidden' id='inpEmail' value='" + usr[IDX_EMAIL] + "'><div class='dx-textbox dx-texteditor dx-editor-outlined dx-widget'><input type='password' id='" + pwd_id + "' class='pwd_input'/><div id='pwd_strength' class='pwd_strength'></div></div>");
                },
                onShown: function (){
                    $(".dx-overlay-content").css("overflow", "visible");
                    checkPswValue(); 
                    $("#" + pwd_id).focus();
                },
                onHidden: function(){
                    $(".dx-overlay-content").css("overflow", "");
                }
            }).dxPopup("instance");
            $("#popupContainer").dxPopup("show");

            var psw = initPasswordField($("#"+pwd_id), {
                pattern: "<% = JS_SafeString(App.GetPswComplexityPattern) %>",
                acceptRate: <% = JS_SafeNumber(If(_DEF_PASSWORD_COMPLEXITY, 1, 0.8)) %>,
                allowEmpty: <% =Bool2JS(AllowBlankPsw) %>,
                warnMsgClassName: "pwd_warn",
                errorWrapClassName: "text pwd_error",
                nonMatchField: "inpEmail",<% If _DEF_PASSWORD_COMPLEXITY Then %>
                length: {<% If _DEF_PASSWORD_MIN_LENGTH > 1 Then%>min: <%=_DEF_PASSWORD_MIN_LENGTH%><% End If %>,<% If _DEF_PASSWORD_MAX_LENGTH > 1 Then%>max: <%=_DEF_PASSWORD_MAX_LENGTH%><% End If %>},<% End If %>
                events: {
                    generated: function (pass) {
                        psw.togglePassMasking(true);
                    }
                }
            }, true, "pwd_strength", function (e) {
                checkPswValue();
            });
        }
    }

    function checkPswValue() {
        <% If Not WebOptions.AllowBlankPsw Then %>var psw = $("#" + pwd_id);
        var btn = $("#btnResetPassOK");
        if ((psw) && (psw.length) && (btn) && (btn.length)) {
            var dis = (psw.val()=="");
            <% If _DEF_PASSWORD_COMPLEXITY Then %>if (!dis) dis = !($("#"+pwd_id).validatePass());<% End If %>
            btn.dxButton("option", "disabled", dis);
        }<% End If%>
    }

    function confResetPsw(email, uid, psw) {
        closePopup(); 
        var dlg = DevExpress.ui.dialog.custom({
            title: resString("titleConfirmation"),
            messageHtml: "<div style='max-width:600px'><% = JS_SafeString(ResString("confUserPswUpdate")) %><p><div id='cbRemoveHashes'></div></div>".format(email),
            buttons: [{
                text: resString("btnContinue"),
                type: "default",
                onClick: function (e) {
                    var keep_hashes = true;
                    var cb = $("#cbRemoveHashes");
                    if ((cb) && cb.data("dxCheckBox")) {
                        keep_hashes = !(cb.dxCheckBox("instance").option("value"));
                    }
                    doResetPsw(uid, psw, keep_hashes);
                }
            }, {
                text: resString("btnCancel"),
                onClick: function (e) {
                }
            }]
        });
        dlg.show();
        $("#cbRemoveHashes").dxCheckBox({
            "text": "<% =JS_SafeString(ResString("lblRemoveHashes")) %>".format(email),
            value: false
        });
        return false;
    }

    function doResetPsw(uid, psw, keep_hashes) {
        closePopup(); 
        var usr = getUserById(uid);
        if ((usr)) {
            showLoadingPanel();
            callAjax("action=userpsw&id=" + uid + "&keep_hashes=" + ((keep_hashes) ? 1 : 0) + "&val=" + encodeURIComponent(psw), function (params) {
                hideLoadingPanel();
                if ((params)) {
                    var rd = eval(params);
                    hideLoadingPanel();
                    usr[IDX_HAS_PSW] = rd[1] !== 0;
                    $("#imgPsw" + uid).css('color', usr[IDX_HAS_PSW] ? "orange" : "");
                    DevExpress.ui.dialog.alert((typeof rd[2] != "undefined" && rd[2] !="" ? rd[2] : "<% = JS_SafeString(ResString("msgPswUpdated")) %>"), "<% = JS_SafeString(ResString("mnuSetPassword")) %>");
                }
            });
        }
    }

    function getUserLink(email, action, target_id, ctrls, icon) {<% If App.isRiskEnabled Then %>
        var items = [
            {
                text: "Likelihood",
                icon: (icon!="" ? icon : "fa fa-sitemap"),
                visible: true,
                onItemClick: function () {
                    sendCommand("action=" + action + "&hid=<% =CInt(ECHierarchyID.hidLikelihood) %>&email=" + encodeURIComponent(email));
                }
            },
            {
                text: "Impact",
                icon: (icon!="" ? icon : "fa fa-sitemap"),
                visible: true,
                onItemClick: function () {
                    sendCommand("action=" + action + "&hid=<% =CInt(ECHierarchyID.hidImpact) %>&email=" + encodeURIComponent(email));
                }
            },
            {
                text: "Controls",
                icon: (icon!="" ? icon : "fa fa-sitemap"),
                visible: ((ctrls)),
                onItemClick: function () {
                    sendCommand("action=" + action + "&controls=1&email=" + encodeURIComponent(email));
                }
            }
        ];
        onShowSubmenu($("#context-menu"), "link_menu_" + target_id, items, target_id);
        setTimeout(hideTooltips, 500);
        <% Else %>sendCommand("action=" + action + "&email=" + encodeURIComponent(email));<% End If %>
    }

    function userUnlock(email) {
        sendCommand("action=unlock&email=" + encodeURIComponent(email));
    }

    <% If CurrentPageID <> _PGID_ADMIN_USERSLIST Then %>function getPreview(id, email, target_id) {<% If App.isRiskEnabled Then %>
        var items = [
            {
                text: "Likelihood",
                icon: "fas fa-eye",
                visible: true,
                onItemClick: function () {
                    loadURL("<% =JS_SafeString(PageURL(_PGID_EVALUATION, "passcode=" + PRJ.PasscodeLikelihood + "&readonly=yes&id=", False)) %>" + id);
                }
            },
            {
                text: "Impact",
                icon: "fas fa-eye",
                visible: true,
                onItemClick: function () {
                    loadURL("<% =JS_SafeString(PageURL(_PGID_EVALUATION, "passcode=" + PRJ.PasscodeImpact + "&readonly=yes&id=", False)) %>" + id);
                }
            },
            {
                text: "Controls",
                icon: "fas fa-eye",
                visible: true,
                onItemClick: function () {
                    loadURL("<% =JS_SafeString(PageURL(_PGID_EVALUATE_RISK_CONTROLS_READONLY, "id=", False)) %>" + id);
                }
            }
        ];
        onShowSubmenu($("#context-menu"), "view_menu_" + target_id, items, target_id);
        setTimeout(hideTooltips, 500);
        <% Else %>showLoadingPanel();
        <% If App.isEvalURL_EvalSite AndAlso Not App.isRiskEnabled Then %>getUserLink(email, "view", "iconView" + id, true,"fas fa-eye");<% Else %>document.location.href = "<% =JS_SafeString(PageURL(_PGID_EVALUATION, "readonly=yes&id=", False)) %>" + id;<% End If %>
        <% End If %>
    }

    function getPreviewCore(id) {
        openProjectURLWhenGecko("<% = JS_SafeString(PageURL(_PGID_EVALUATION, "readonly=yes&id=", False)) %>" +id);
    }<% End If %>

    function ProceedLinkAction(link, cur_oper, is_resp) {
        if (cur_oper === "get_link" || cur_oper === "get_controls_link") copyDataToClipboard(link);
        if (cur_oper === "open_link") OpenLinkWithReturnUser(link, <%=App.ProjectID%>, <%=If(PM Is Nothing, ECHierarchyID.hidLikelihood, PM.ActiveHierarchy)%>, "<%=JS_SafeString(If(PM IsNot Nothing, If(App.HasActiveProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact, App.ActiveProject.PasscodeImpact, App.ActiveProject.PasscodeLikelihood), ""))%>", is_resp);
    }

    // Results Groups Editor
    function initResGroupsDlg() {        
        cancelled = false;
        on_hit_enter = "addResGroup();";
        $("#divResGroups").dxPopup({
            //animation: {
            //    show: { "duration": 0 },
            //    hide: { "duration": 0 }
            //},
              width: "auto",
              height: function() {
                  return window.innerHeight * 0.8;
              },
              title: "Manage Groups",
            onShown: function (e) {
                var h = window.innerHeight * 0.8;
                $("#pResGroups").height(h > 95 ? h - 95 : 95);
                $("#tblResGroups").css("max-height", (h > 95 ? h - 95 : 95) + "px");
                $("#tbName").focus();
              },
              onHiding: function() {                
                initResGroups();
                on_hit_enter = "";
                if (!cancelled && drag_order != "") {
                    drag_order = "";
                    for (var i = 0; i < res_groups_data.length; i++) {                    
                        drag_order += (drag_order == "" ? "" : ",") + res_groups_data[i][IDX_RES_GRP_ID];
                    }
                    sendCommand("action=res_groups_reorder&lst=" + encodeURIComponent(drag_order));
                    drag_order = "";
                }
              },
              visible: true
        });
        $("#btnResGroupsClose").dxButton({
            text: "<%=ResString("btnClose")%>",
            icon: "fas fa-times",
            onClick: function() {
                if (checkUnsavedData(document.getElementById("tbName"), "$('#divResGroups').dxPopup('hide');")) $("#divResGroups").dxPopup("hide");
            }
        });
    }

    function initResGroups() {
        var t = $("#tblResGroups tbody");
        
        if ((t)) {
            t.html("");        
            for (var i = 0; i < res_groups_data.length; i++) {                
                var v = res_groups_data[i];
                var n = htmlEscape(v[IDX_RES_GRP_NAME]);
                var id = v[IDX_RES_GRP_ID];
                sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "'>";
                sRow += "<td align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td id='tdName" + id + "' style='text-align: left;'>" + n + "</td>";                
                sRow += "<td id='tdActions" + id + "' align='center'><nobr><a href='' onclick='onEditResGroupName(" + id + ", false); return false;'><i class='fas fa-pencil-alt'></i></a>&nbsp;<a href='' onclick='deleteResGroup(" + id + "); return false;'><i class='fas fa-trash-alt'></i></a></nobr></td>";
                sRow +="</tr>";
                t.append(sRow);
            }

            sRow = "<tr class='text grid_footer' id='trNew'>";
            sRow += "<td>&nbsp;</td>";
            sRow += "<td><input type='text' class='input' style='width:100%' id='tbName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='if (event.keyCode == KEYCODE_ENTER) addResGroup();'></td>";
            sRow += "<td align='center'><a href='' onclick='addResGroup(); return false;'><i class='fas fa-plus-square'></i></a></td></tr>";
            t.append(sRow);
        }
    }

    function initDragDropGroups() {
        $(function () {
            $(".drag_drop_grid_groups").sortable({
                items: 'tr:not(tr:last-child)',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid_groups',
                axis: 'y',
                start: function( event, ui ) { drag_index = ui.item.index(); },
                stop: function ( event, ui) { setTimeout(function () { var tb = document.getElementById('tbName'); if ((tb)) tb.focus(); }, 50); },
                update: function( event, ui ) { onDragIndexGroups(ui.item.index()); }
            });
        });
    }

    function onDragIndexGroups(new_idx) {
        if (new_idx >= 0 && drag_index >= 0 && new_idx != drag_index) {
            
            // reorder groups
            var el = res_groups_data[drag_index];
            res_groups_data.splice(drag_index, 1);
            res_groups_data.splice(new_idx, 0, el);
            drag_order = "";
            for (var i = 0; i < res_groups_data.length; i++) {                    
                drag_order += (drag_order == "" ? "" : ",") + res_groups_data[i][IDX_GRP_ID];
            }            
            
            initResGroups();            

            drag_index = -1;
        }
    }

    function initDragDropAttributes() {
        $(function () {
            $(".drag_drop_grid_attrs").sortable({
                items: 'tr:not(tr:last-child)',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid_attrs',
                axis: 'y',
                start: function( event, ui ) { drag_index = ui.item.index(); },
                stop: function ( event, ui) { setTimeout(function () { var tb = document.getElementById('tbName'); if ((tb)) tb.focus(); }, 50); },
                update: function( event, ui ) { onDragIndexAttributes(ui.item.index()); }
            });
        });
    }

    function onDragIndexAttributes(new_idx) {
        if (new_idx >= 0 && drag_index >= 0 && new_idx != drag_index) {            
            // reorder groups
            var el = attributes_data[drag_index];
            attributes_data.splice(drag_index, 1);
            attributes_data.splice(new_idx, 0, el);
            drag_order = "";
            for (var i = 0; i < attributes_data.length; i++) {                    
                drag_order += (drag_order == "" ? "" : ",") + attributes_data[i][aidx_id];
            }            
            
            initAttributes();            

            drag_index = -1;
        }
    }

    /* Add-remove-rename results groups */
    function onEditResGroupName(id, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbName"), "onEditResGroupName(" + id + ", true)")) return false;
        initResGroups();
        var name = htmlEscape(getResGroupByID(id)[IDX_RES_GRP_NAME]);
        $("#tdName" + id).html("<input type='text' class='input' style='width:" + $("#tdName" + id).width()+ "' id='tbName' value='" + name + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdActions" + id).html("<a href='' onclick='editResGroupName(" + id + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='initResGroups(); document.getElementById(\"tbName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        $("#trNew").html("").hide();
        setTimeout(function () { document.getElementById('tbName').focus(); }, 50);
        on_hit_enter = "editResGroupName(" + id + ");";
    }

    function editResGroupName(id) {
        var n = document.getElementById("tbName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                DevExpress.ui.notify(resString("msgEmptyGroupName"), "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
            } else {
                getResGroupByID(id)[IDX_RES_GRP_NAME] = htmlEscape(n.value);
                sendCommand("action=rename_res_group&name=" + n.value + "&id=" + id);
            }
        }
    }

    function addResGroup() {
        var n = document.getElementById("tbName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value == '') {
                DevExpress.ui.notify(resString("msgCreateEmptyGroupName"), "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
            } else {
                sendCommand('action=add_res_group&name=' + encodeURIComponent(n.value));
            }
        }
    }

    function deleteResGroup(id) {
        dxConfirm(resString("msgSureDeleteGroup"), "sendCommand(\"action=del_res_group&id=" + id + "\");");
    }

    function createUserGroupFromAddUsersDlg(el_name_modifier) {
        initResGroups(); 
        initResGroupsDlg();
    }

    function removeUserGroupFromAddUsersDlg(el_name_modifier) {
        var cb = document.getElementById("cbUserGroups" + ((el_name_modifier) ? el_name_modifier : ""));
        if ((cb) && cb.selectedIndex > 0) {
            sendCommand('action=del_res_group&id=' + cb.options[cb.selectedIndex].value);
        }
    }

    function onUserGroupChanged(cb, el_name_modifier) {
        var btn = document.getElementById("btnRemoveGroup" + ((el_name_modifier) ? el_name_modifier : ""));
        if ((btn)) {
            btn.disabled = cb.selectedIndex == 0;
        }
    }


    /* helper function */
    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value != "")) {
            dxConfirm(resString("msgUnsavedData"), on_agree + ";");
            return false;
        }
        return true;
    }

    function checkUnsavedDataAddUsers(on_agree) {
        var has_emails = false;
        $("input.tb_mail").each(function() {
            if (this.value.trim() != "") {
                has_emails = true;
            }
        });
        if (has_emails) {
            dxConfirm(resString("msgUnsavedData"), on_agree + ";");
            return false;
        }
        return true;
    }    

    function onResize(force_redraw, w, h) {
        var td = $("#trUsersGrid");
        var tbl = $("#"+table_main_id);
        if ((td) && (td.length) && (tbl) && (tbl.length)) {
            tbl.hide();
            tbl.width(td.width() - 24).height(td.height() - 16);
            tbl.show();
        }
    }

    resize_custom = onResize;

    function addParticipants(view) {
        switch (view) {
            case 0:
                dlgAddParticipants();
                break;
            case 1: // Add project users from current workgroup
                sendCommand("action=get_workgroup_users", undefined, false);
                break;
            //case 2: // Add project users from Active Directory
            case 4: // Add workgroup users from Active Directory                
                dlgAddADParticipants();
                break;
            default:
                dlgAddWGParticipants();
                break;
        }
    }

    function dlgAddWGParticipants() {
        cancelled = true;

        $("#divAddWGUsers").dxPopup({
              width: "80%",
              height: "auto",
              maxHeight: function() {
                  return window.innerHeight * 0.9;
              },
              title: "Sign up multiple participants",
              onShowing: function() {
                  initGroupsCombobox("PRJ");
                  $("#tbAddWGUsers").val("");        
              },
              onHiding: function() {                
                  if (!cancelled) {
                      var data = $("#tbAddWGUsers").val();
                      if (data.trim() != "") {
                          var cbGenPass = document.getElementById("cbWGGenerateRandPass");
                          var cbGenPassChecked = false;
                          if ((cbGenPass)) cbGenPassChecked = cbGenPass.checked;
                          
                          var cbSendMail = document.getElementById("cbWGSendRegNotification");
                          var cbSendMailChecked = false;
                          if ((cbSendMail)) cbSendMailChecked = cbSendMail.checked;
                                                  
                          var addToGroupIdx = "<% = Integer.MinValue %>";
                          var cb = document.getElementById("cbUserGroupsPRJ");
                          if ((cb) && cb.selectedIndex > 0) addToGroupIdx = cb.options[cb.selectedIndex].value;
                          
                          var params = "&generate_pass=" + cbGenPassChecked + "&send_mail=" + cbSendMailChecked + "&add_to_group=" + addToGroupIdx;;
                          sendCommand("action=add_clipboard_users&data=" + encodeURIComponent(data) + params);
                      }
                  }
                  $("#tbAddWGUsers").val("");
              },
              visible: true
        });
        
        $("#btnAddWGUsersClose").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            onClick: function() {
                cancelled = false;
                $("#divAddWGUsers").dxPopup("hide");
            }
        });
        $("#btnAddWGUsersCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            onClick: function() {
                cancelled = true;
                $("#divAddWGUsers").dxPopup("hide");
            }
        });
    }

    function dlgAddParticipants() {
        cancelled = true;

        $("#divAddUsers").dxPopup({
            width: "80%",
            height: "auto",
            maxHeight: function() {
                return window.innerHeight * 0.9;
            },
            title: "Add Participants. Type or paste information below.",
            onShown: function() {
                added_rows = 0;
                $("#tblAddUsers tbody").html("");        
                addUserLine();
                initGroupsCombobox("");
                on_hit_enter = "addUserLine();";
                $("#tbMail" + added_rows).focus();
            },
            onHiding: function() {                
                if (!cancelled) {
                    var n = 0;
                    var data = "";
                    $("input.tb_mail").each(function() {
                        n += 1;
                        var mail1 = "";
                        var name1 = "";
                        if (this.value.trim() != "") {                              
                            mail1 = this.value.trim();
                            var tb = document.getElementById("tbName" + n);
                            if ((tb)) {
                                name1 = tb.value.trim();
                            }
                            data += "&mail" + n + "=" + encodeURIComponent(mail1);
                            data += "&name" + n + "=" + encodeURIComponent(name1);
                        }
                    });
                    if (n > 0) {
                        var cbGenPass = document.getElementById("cbGenerateRandPass");
                        var cbGenPassChecked = false;
                        if ((cbGenPass)) cbGenPassChecked = cbGenPass.checked;
                          
                        var cbSendMail = document.getElementById("cbSendRegNotification");
                        var cbSendMailChecked = false;
                        if ((cbSendMail)) cbSendMailChecked = cbSendMail.checked;
                          
                        var addToGroupId = "<% = Integer.MinValue %>";
                        var cb = document.getElementById("cbUserGroups");
                        if ((cb) && cb.selectedIndex > 0) addToGroupId = cb.options[cb.selectedIndex].value;
                          
                        var params = "&generate_pass=" + cbGenPassChecked + "&send_mail=" + cbSendMailChecked + "&add_to_group=" + addToGroupId;
                        sendCommand("action=add_users&count=" + n + params + data);
                    }
                }
                $("#tblAddUsers tbody").html("");
                on_hit_enter = "";
            },
            visible: true
        });
        
        $("#btnAddUsersClose").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            onClick: function() {
                cancelled = false;
                $("#divAddUsers").dxPopup("hide");
            }
        });
        $("#btnAddUsersCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            onClick: function() {
                cancelled = true;
                $("#divAddUsers").dxPopup("hide");
            }
        });
    }
        
    function getADUsers() {
        sendCommand("action=get_ldap_users&srv=" + $("#tbLDAPServer").val().trim() + "&dn=" + $("#tbLDAPDN").val().trim() + "&pw=" + $("#tbLDAPPW").val());
    }

    function dlgAddADParticipants() {
        $("#divAddADUsers").dxPopup({
              width: "auto",
              height: "auto",
              title: "Connect...",
              visible: true
        });

        $("#btnAddADUsersClose").dxButton({
            text: "<%=ResString("btnClose")%>",
            icon: "fas fa-times",
            onClick: function() {
                $("#divAddADUsers").dxPopup("hide");
            }
        });
    }

    function initGroupsCombobox(el_name_modifier) {
        var cb = document.getElementById("cbUserGroups" + ((el_name_modifier) ? el_name_modifier : ""));
        if ((cb)) {
            while (cb.firstChild) {
                cb.removeChild(cb.firstChild);
            }

            // add "No Group" option
            var opt = createOption("<% = Integer.MinValue %>", "No Group");
            cb.appendChild(opt);

            // add result groups options
            for (var i = 0; i < res_groups_data.length; i++) {                
                var v = res_groups_data[i];
                if (!v[IDX_RES_GRP_HAS_RULE]) {
                    var opt = createOption(v[IDX_RES_GRP_ID], v[IDX_RES_GRP_NAME]);
                    cb.appendChild(opt);
                }
            }
        }
    }

    function createOption(value, name) {
        var opt = document.createElement("option");
        opt.value = value;
        opt.innerHTML = ShortString(name, 75);
        return opt;
    }

    function removeUserLine(rowid) {
        var row = document.getElementById(rowid);
        row.parentNode.removeChild(row);
    }

    function addUserLine(email, name) {
        added_rows += 1;
        var t = $("#tblAddUsers tbody");
        
        if ((t)) {
            sRow = "<tr class='text' id='trRow" + added_rows + "'>";
            var sName = "";
            var sMail = "";
            if ((name)) sName = htmlEscape(name);
            if ((email)) sMail = htmlEscape(email);
            sRow += "<td><input type='text' class='input tb_mail' style='width:100%' id='tbMail" + added_rows + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value='" + sMail + "'></td>";
            sRow += "<td><input type='text' class='input tb_name' style='width:100%' id='tbName" + added_rows + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value='" + sName + "'></td>";
            sRow += "<td align='center'><a href='' onclick='removeUserLine(\"trRow" + added_rows + "\"); return false;'><i class='fas fa-minus-square'></i></a></td></tr>";
            t.append(sRow);
        }
    }

    function pasteWGUsers() {
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteWGData(data);
        } else {
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteWGData);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteWGChrome();", "cancelPasteWGChrome();", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }            
        }
    }

    function pasteUsers() {
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteData(data);
        } else {
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteData);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome();", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }               
        }                
    }

    function commitPasteChrome() {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(pasteBox.value);
        }
    }

    function pasteData(data) {
        $("#tblAddUsers tbody").html("");
        added_rows = 0;
        if ((data) && data != "") {
            var rows = data.split(String.fromCharCode(10));
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i].trim();
                if ((row) && row != "") {
                    var cells = row.split(String.fromCharCode(9));

                    if (cells.length > 1) { // cells are delimited with TAB

                    } else { // try parse as if cells were delimited with SPACE
                        var spaceIndex = row.indexOf(" ");
                        if (spaceIndex > 0 && spaceIndex < row.length) {
                            cells = [row.substring(0, spaceIndex), row.substring(spaceIndex + 1)];
                        } else {
                            cells = [row, ""];
                        }
                    }
                    
                    if (cells.length > 1) {
                        var email = cells[0].trim();
                        var name = cells[1].trim();

                        if (!(name)) name = "";
                        if ((email) && email != "") {
                            addUserLine(email, name);
                        }
                    }
                }
            }
        } else { 
            DevExpress.ui.notify(resString("msgUnableToPaste"), "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
        }
        // enable paste button
        var btn = document.getElementById("btnPasteUsers");
        if ((btn)) btn.disabled = false;
    }

    function commitPasteWGChrome() {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteWGData(pasteBox.value);
        }
        // enable paste button
        var btn = document.getElementById("btnPasteWGUsers");
        if ((btn)) btn.disabled = false;
    }

    function cancelPasteWGChrome() {
        // enable paste button
        var btn = document.getElementById("btnPasteWGUsers");
        if ((btn)) btn.disabled = false;
    }

    function pasteWGData(data) {
        if ((data) && data != "") {
            $("#tbAddWGUsers").val($("#tbAddWGUsers").val() + data);
        } else { 
            DevExpress.ui.notify(resString("msgUnableToPaste"), "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
        }
        // enable paste button
        var btn = document.getElementById("btnPasteWGUsers");
        if ((btn)) btn.disabled = false;
    }

    function dlgAddParticipantsFromAD(arr_ad_users) {
        cancelled = false;
        datatablesWGUsers(arr_ad_users, true);
        $("#divWGUsersDialog").dxPopup({
            width: "80%",
            height: "80%",
            maxHeight: function() {
                return window.innerHeight * 0.9;
            },
            title: "Select Active Directory members to include in the " + (isWorkgroupUsers ? "workgroup" : "project"),
            onShowing: function() {                   
                initGroupsCombobox("AD");
                if (isWorkgroupUsers) {
                    $("#lblWGADAsPMs").show();
                    $("#lblGroupsUI").hide();
                } else {
                    $("#lblWGADAsPMs").hide();
                    $("#lblGroupsUI").show();
                }
            },
            onHiding: function() {
                if (!cancelled) {
                    if (isWorkgroupUsers) {
                        addCheckedWGUsers(true);
                    } else {
                        var data = getCheckedUsersIds("tableWGUsers", wg_users.length);                          
                        if (data.length > 0) {
                            var cbGenPass = document.getElementById("cbWGADGenerateRandPass");
                            var cbGenPassChecked = false;
                            if ((cbGenPass)) cbGenPassChecked = cbGenPass.checked;
                          
                            var cbSendMail = document.getElementById("cbWGADSendRegNotification");
                            var cbSendMailChecked = false;
                            if ((cbSendMail)) cbSendMailChecked = cbSendMail.checked;
                          
                            var addToGroupId = "<% = Integer.MinValue %>";
                            var cb = document.getElementById("cbWGADUserGroups");
                            if ((cb) && cb.selectedIndex > 0) addToGroupId = cb.options[cb.selectedIndex].value;
                          
                            var params = "&generate_pass=" + cbGenPassChecked + "&send_mail=" + cbSendMailChecked + "&add_to_group=" + addToGroupId;
                            sendCommand("action=add_prj_users_from_ad&lst=" + data + params);
                        }
                    }
                }
            },
            visible: true
        });
        
        $("#btnWGUsersClose").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            onClick: function() {
                cancelled = false;
                $("#divWGUsersDialog").dxPopup("hide");
            }
        });
        $("#btnWGUsersCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            onClick: function() {
                cancelled = true;
                $("#divWGUsersDialog").dxPopup("hide");
            }
        });
    }

    function dlgAddParticipantsFromWorkgroup(arr_wg_users) {
        cancelled = false;
        datatablesWGUsers(arr_wg_users, false);
        $("#divWGUsersDialog").dxPopup({
            width: "80%",
            height: "80%",
            title: "Add participants from existing workgroup members",
            onShowing: function() {
                initGroupsCombobox("AD");
            },
            onHiding: function() {
                if (!cancelled) {
                    addCheckedWGUsers(false);
                }
            },
            visible: true
        });

        $("#btnWGUsersClose").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            onClick: function() {
                cancelled = false;
                $("#divWGUsersDialog").dxPopup("hide");
            }
        });
        $("#btnWGUsersCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            onClick: function() {
                cancelled = true;
                $("#divWGUsersDialog").dxPopup("hide");
            }
        });
    }

    var wg_users = [];

    function datatablesWGUsers(arr_wg_users, is_ad) {
        var table = (($('#tableWGUsers').data("dxDataGrid"))) ? $('#tableWGUsers').dxDataGrid("instance") : null;
        if (table !== null) { table.dispose(); }

        if (!is_ad) wg_users = arr_wg_users;

        var columns = [];
            
        //init columns headers        
        columns.push({ "caption" : "<%=ResString("tblEmailAddress")%>", "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "width": 200, "dataField" : "email" });
        columns.push({ "caption" : "<%=ResString("tblSyncUserName")%>", "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "dataField" : "name" });
        if (!is_ad) {
            columns.push({ "caption" : "<%=ResString("tblUserRole")%>", "alignment" : "left", "allowSorting" : true, "width": 200, "dataField" : "role" });
        }

        table_wg = $('#tableWGUsers').dxDataGrid( {
            dataSource: arr_wg_users,
            columns: columns,
            columnAutoWidth: true,
            height: "70%",
            //width: "96%",
            //keyExpr: "email",
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            scrolling: {
                rowRenderingMode: "virtual"
            },
            selection: {
                mode: "multiple",
                allowSelectAll: true,
                showCheckBoxesMode: "always"
            },
            searchPanel: {
                visible: true,
                text: "",
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            noDataText: "<% =GetEmptyMessage()%>"
        });

        $("a.dt-button").css({ padding:"4px 8px", width:"5em", margin:"5px 4px" });
        $("div.dt-buttons").css({"float":"none"});        
    }

    function addCheckedWGUsers(is_ad) {
        //if ($("input.cb_wg_user:checked" ).length > 0) { // any user checked
        //if (wg_users.length > 0) {
        var params = "";
        //if (is_ad) {
            var cbGenPass = document.getElementById("cbWGADGenerateRandPass");
            var cbGenPassChecked = false;
            if ((cbGenPass)) cbGenPassChecked = cbGenPass.checked;
                          
            var cbSendMail = document.getElementById("cbWGADSendRegNotification");
            var cbSendMailChecked = false;
            if ((cbSendMail)) cbSendMailChecked = cbSendMail.checked;
            
            var cbAllPMs = document.getElementById("cbWGADAsPMs");
            var cbAllPMsChecked = false;
            if ((cbAllPMs)) cbAllPMsChecked = cbAllPMs.checked;            

            var addToGroupId = "<% = Integer.MinValue %>";
            var cb = document.getElementById("cbUserGroupsAD");
            if ((cb) && cb.selectedIndex > 0) addToGroupId = cb.options[cb.selectedIndex].value;
                                                                          
            var params = "&generate_pass=" + cbGenPassChecked + "&send_mail=" + cbSendMailChecked + "&all_pms=" + cbAllPMsChecked + "&add_to_group=" + addToGroupId;;
        //}
            sendCommand('action='+(!is_ad? "add_from_wg" : "add_from_ad") + '&lst=' + getCheckedUsersIds("tableWGUsers", wg_users.length) + params);
        //}
    }

    var erase_options = "";

    function eraseJudgments() {
        erase_options = "";
        <%If PM IsNot Nothing AndAlso PM.IsRiskProject Then%>
        var dlg_content = "<h5>Erase judgments for:</h5>"; 
        dlg_content += "<div style='margin:2px auto; width:80%;'><label><input type='checkbox' id='cbL' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood AndAlso Not CurrentPageID = _PGID_RISK_INVITE_USERS, " checked='checked'", "")%> onclick='cbLClick(this.checked);'><%=ParseString("%%Likelihood%% hierarchy")%></label><br/>";
        dlg_content += "&nbsp;&nbsp;&nbsp;&nbsp;<label><input type='radio' id='rbLA' name='rbL' value='0' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "", " disabled='disabled'")%>><%=ParseString("For %%Alternatives%%")%></label><br/>";
        dlg_content += "&nbsp;&nbsp;&nbsp;&nbsp;<label><input type='radio' id='rbLO' name='rbL' value='1' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "", " disabled='disabled'")%>><%=ParseString("For %%Sources%%")%></label><br/>";
        dlg_content += "&nbsp;&nbsp;&nbsp;&nbsp;<label><input type='radio' id='rbLB' name='rbL' value='2' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, "", " disabled='disabled'")%> checked='checked'><%=ParseString("Both")%></label><br/>";
        dlg_content += "<label><input type='checkbox' id='cbI' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidImpact AndAlso Not CurrentPageID = _PGID_RISK_INVITE_USERS, " checked='checked'", "")%> onclick='cbIClick(this.checked);'><%=ParseString("%%Impact%% hierarchy")%></label><br/>";
        dlg_content += "&nbsp;&nbsp;&nbsp;&nbsp;<label><input type='radio' id='rbIA' name='rbI' value='0' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "", " disabled='disabled'")%>><%=ParseString("For %%Alternatives%%")%></label><br/>";
        dlg_content += "&nbsp;&nbsp;&nbsp;&nbsp;<label><input type='radio' id='rbIO' name='rbI' value='1' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "", " disabled='disabled'")%>><%=ParseString("For Objectives")%></label><br/>";
        dlg_content += "&nbsp;&nbsp;&nbsp;&nbsp;<label><input type='radio' id='rbIB' name='rbI' value='2' <%=If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "", " disabled='disabled'")%> checked='checked'><%=ParseString("Both")%></label><br/>";
        dlg_content += "<label><input type='checkbox' id='cbC' <%=If(CurrentPageID = _PGID_RISK_INVITE_USERS, " checked='checked'", "")%>><%=ParseString("%%Controls%%")%></label><br/>";
        dlg_content += "<br/><a href='' class='actions' onclick='selectAllEraseJudgmentsOptions(); return false;'><%=ResString("lblSelectAll")%></a><br/></div>";
        dxDialog(dlg_content, "erase_options=($('#cbL').prop('checked') ? '1' : '0') + $('input[name=rbL]:checked').val() + ($('#cbI').prop('checked') ? '1' : '0') + $('input[name=rbI]:checked').val() + ($('#cbC').prop('checked') ? '1' : '0'); proceedEraseJudgments();", ";", "Erase Judgments");
        dxDialogBtnFocus(true);
        <%Else%>
        //proceedEraseJudgments();
        var dlg_content = "<h5>Erase judgments for:</h5>"; 
        dlg_content += "<div style='margin:2px auto; width:80%;'>";
        dlg_content += "<label><input type='radio' id='rbA' name='rbErase' value='0'><%=ParseString("For %%Alternatives%%")%></label><br/>";
        dlg_content += "<label><input type='radio' id='rbO' name='rbErase' value='1'><%=ParseString("For %%Objectives%%")%></label><br/>";
        dlg_content += "<label><input type='radio' id='rbB' name='rbErase' value='2' checked='checked'><%=ParseString("Both")%></label><br/>";
        dlg_content += "</div>";
        dxDialog(dlg_content, "erase_options=$('input[name=rbErase]:checked').val(); proceedEraseJudgments();", ";", "Erase Judgments");
        dxDialogBtnFocus(true);
        <%End If%>
    }

    function cbLClick(chk) {
        $("#rbLA").prop("disabled", !chk);
        $("#rbLO").prop("disabled", !chk);
        $("#rbLB").prop("disabled", !chk);
    }

    function cbIClick(chk) {
        $("#rbIA").prop("disabled", !chk);
        $("#rbIO").prop("disabled", !chk);
        $("#rbIB").prop("disabled", !chk);
    }

    function selectAllEraseJudgmentsOptions() {
        $("#cbL").prop("checked", true);
        $("#cbI").prop("checked", true);
        $("#cbC").prop("checked", true);
        $("#rbLB").prop("checked", true);
        $("#rbIB").prop("checked", true);
        cbLClick(true);
        cbIClick(true);
    }

    function proceedEraseJudgments() {
        sendCommand('action=erase_judgments&lst=' + getCheckedUsersEmails(table_main_id, users_count()) + '&options=' + erase_options);
    }

    function copyJudgments() {
        $("#divCopyJudgments").dxPopup({
            width: "auto",
            height: "auto",
            maxHeight: function() {
                return window.innerHeight * 0.9;
            },
            title: "Copy Judgments...",
            onShowing: function() { 
                var users_data_len = users_count();
                // init UI features
                initSearch();

                var divSA = document.getElementById("divSelectAll");
                if ((divSA)) {
                    divSA.style.display = users_data_len > 1 ? "" : "none";
                }

                //$("#btnCopyJudgmentsOK").dxButton("instance").option("disabled", true);

                // fill in users UI (left and right panes)
                var divLeft = document.getElementById("divLeft");
                var divRight = document.getElementById("divRight");
                if ((divLeft) && (divRight)) {
                    divLeft.innerHTML = "";
                    divRight.innerHTML = "";
                    var sRadios = "";
                    var sCBs = "";
                    for (var i = 0; i < users_data_len; i++) {
                        var u = users_data[i];
                        var uname = htmlEscape(u[IDX_NAME]); //htmlEscape(u[IDX_NAME] == "" ? u[IDX_EMAIL] : u[IDX_NAME] + " (" + u[IDX_EMAIL] + ")");
                        sRadios += "<div class='divRadio'><label class='user_rb_lbl' search='" + uname.toLowerCase() + "'><input type='radio' class='user_radio_btn' id='inputRadio" + i + "' idx='" + i + "' name='inputRadio' value='" + u[IDX_AHP_USER_ID] + "' " + (i == activeUserID ? " checked='checked' " : "") + "/>" + uname + "</label></div>"; 
                        sCBs += "<div class='divCheckboxes' idx='" + i + "' " + (i == 0 ? " style='color:#ccc;' ": "") + "><label class='user_cb_lbl' search='" + uname.toLowerCase() + "'><input type='checkbox' class='user_cb_btn' id='inputCB" + i + "' idx='" + i + "' value='" + u[IDX_AHP_USER_ID] + "' " + (i == activeUserID ? " disabled='disabled' ": "") + " onclick='onUserCBClick();' />" + uname + "</label></div>"; 
                    }
                    divLeft.innerHTML = sRadios;
                    divRight.innerHTML = sCBs;
                    // click handlers
                    $('input.user_radio_btn').on('change',function(e) {
                        $('input.user_cb_btn').prop('disabled', false);
                        $('div.divCheckboxes').css("color", "black");

                        var idx = this.getAttribute('idx');
                        $('input.user_cb_btn[idx="' + idx + '"]').prop('disabled', true);
                        $('div.divCheckboxes[idx="' + idx + '"]').css("color", "#ccc");
                    });
                    $('input.user_cb_btn').on('change',function(e) {
                        
                    });
                }               
                $(".ui-widget-content").css({border:0});
                $(".ui-dialog .ui-dialog-buttonpane").css({marginTop:0});
            },
            onHiding: function() {
                // sendCommand
                if (!cancelled) {
                    var uids = "";
                    $('input.user_cb_btn:checked:enabled').each(function(e) {
                        uids += (uids == "" ? "" : ",") + this.value;
                    });
                    if (uids != "") {
                        var copy_from = $('input.user_radio_btn:checked').val();
                        sendCommand("action=copy_judgments&from=" + copy_from + "&to=" + uids + "&mode=" + $('input[name=rbMode]:checked').val() +"&pw_only=" + ($("#cbPWOnly").is(':checked') ? "1" : "0"));
                    }
                }
            },
            visible: true
        });

        $("#btnCopyJudgmentsOK").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            disabled: true,
            onClick: function() {
                cancelled = false;
                $("#divCopyJudgments").dxPopup("hide");
            }
        });
        $("#btnCopyJudgmentsCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            onClick: function() {
                cancelled = true;
                $("#divCopyJudgments").dxPopup("hide");
            }
        });
    }

    function onUserCBClick() {
        var any_checked = $("input.user_cb_btn:checked").length > 0;
        $("#btnCopyJudgmentsOK").dxButton("instance").option("disabled", !any_checked);
    }

    function selectAllUsersInDlg(chk) {
        $("input.user_cb_btn:enabled").prop("checked", chk);
        onUserCBClick();
    }
    
    function initSearch() {
        $("#tbSearchLeft").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
                    
            if (oldValue == "") return;

            setTimeout(function(){ if ($input.val() == "") onSearchLeft(""); }, 100);
        });
        $("#tbSearchRight").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
                    
            if (oldValue == "") return;

            setTimeout(function(){ if ($input.val() == "") onSearchRight(""); }, 100);
        });
    }

    function onSearchLeft(value) {
        value = value.toLowerCase();
        if (value == "") {
            $(".user_rb_lbl").show();
        } else {
            $(".user_rb_lbl").each(function (index, lbl) { 
                var attr = lbl.getAttribute("search");
                lbl.style.display = (attr) && (attr.indexOf(value) >= 0) ? "" : "none";
            });
        }
    }

    function onSearchRight(value) {
        value = value.toLowerCase();
        if (value == "") {
            $(".user_cb_lbl").show();
        } else {
            $(".user_cb_lbl").each(function (index, lbl) { 
                var attr = lbl.getAttribute("search");
                lbl.style.display = (attr) && (attr.indexOf(value) >= 0) ? "" : "none";
            });
        }
    }
    
    function toggleUserState(chk) {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var ids = getCheckedUsersEmailsArr();
            for (var i = 0; i < ids.length; i++) {
                var usr = getUserByEmail(ids[i]);
                if ((usr) && usr[IDX_CAN_EDIT]) usr[IDX_WG_USER_DISABLED] = !chk;
            }
            sendCommand('action=user_state&chk=' + (chk ? "1" : "0") + '&lst=' + getCheckedUsersEmails(table_main_id, users_count()));
        }
    }

    function eraseUserPsw() {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var ids = getCheckedUsersEmailsArr();
            var lst = [];
            for (var i = 0; i < ids.length; i++) {
                var usr = getUserByEmail(ids[i]);
                if ((usr) && usr[IDX_CAN_EDIT] && usr[IDX_HAS_PSW]) {
                    usr[IDX_HAS_PSW] = false;
                    lst.push(ids[i]);
                }
            }
            table_main.clearSelection();
            sendCommand('action=reset_psw&lst=' + lst.join());
        }
    }

    function setUserRole(value) {
        sendCommand('action=set_user_role&value=' + value + '&lst=' + getCheckedUsersEmails(table_main_id, users_count()));
    }

    function removeParticipantsByJudgments(params) {
        sendCommand('action=remove_users_by_judg&param=' + params);
    }

    function removeParticipants() {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var ids = getCheckedUsersEmailsArr();
        }
        var lst = getCheckedUsersEmails(table_main_id, users_count());
        sendCommand('action=remove_users&lst=' + lst);
        removeUsers(ids);
    }

    function removeUsers(ids) {
        for (var i = 0; i < ids.length; i++) {
            var usr = getUserByEmail(ids[i]);
            if ((usr)) {
                var index = users_data.indexOf(usr);
                if (index > -1 && usr[IDX_EMAIL]!="<% =JS_SafeString(App.ActiveUser.UserEmail) %>") {
                    users_data.splice(index, 1);
                }
            }
        }
    }

    //function doExport() {
    //    var lst = "";
    //    $("input.cb_user").each(function() {
    //        if (this.checked) {
    //            var u = getUserById(this.getAttribute('uid')*1);
    //            lst += u[IDX_NAME] + "\t" + u[IDX_EMAIL];
    //            lst += "\r\n";
    //        }
    //    });
    //    copyDataToClipboard(lst);
    //}

    //function storeCheckedUsers() {
    //    storedCheckedUsers = [];
    //    $("input.cb_user").each(function() {
    //        if (this.checked) storedCheckedUsers.push(this.getAttribute('uid') * 1);
    //    });            
    //}

    function getCheckedUsersEmails() {
        //if (cb_id == "cb_user") storeCheckedUsers();
        var lst = "";
        var chk_count = 0;
        var total_count = users_count();
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var ids = table_main.getSelectedRowKeys();
            for (var i = 0; i < ids.length; i++) {
                chk_count += 1;
                lst += (lst == "" ? "" : ",") +  encodeURIComponent(replaceString(",", "%2C", ids[i]));
            }           
            //if (ids.length == 0 && table_main.option("focusedRowIndex") > -1) {
            //    lst = encodeURIComponent(replaceString(",", "%2C", table_main.option("focusedRowKey")));
            //}
        }
        
        //if (chk_count > 0 && chk_count == total_count) lst = "all";
        return lst;
    }

    function getCheckedUsersIds(tbl_id, total_count) {
        //if (cb_id == "cb_user") storeCheckedUsers();
        var lst = "";
        var chk_count = 0;
        var table_main = (($("#" + tbl_id).data("dxDataGrid"))) ? $("#" + tbl_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var ids = table_main.getSelectedRowKeys();
            for (var i = 0; i < ids.length; i++) {
                chk_count += 1;
                lst += (lst == "" ? "" : ",") + ids[i]["id"];
            }           
        }
        
        //if (chk_count > 0 && chk_count == total_count) lst = "all";
        return lst;
    }

    function getCheckedUsers(tbl_id) {
        var lst = [];
        var table_main = (($("#" + tbl_id).data("dxDataGrid"))) ? $("#" + tbl_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            var ids = getCheckedUsersEmailsArr();
            for (var i = 0; i < ids.length; i++) {
                lst.push(getUserByEmail(ids[i]));
            }           
        }
        
        return lst;
    }

    /* Toolbar */
    var toolbarItems = [    
        {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxDropDownButton',
            options: {
                width: "110px",
                elementAttr: {id: 'btn_add'},
                disabled: <%=Bool2JS(IsReadOnly OrElse (PRJ IsNot Nothing AndAlso (PRJ.ProjectStatus = ecProjectStatus.psTemplate OrElse PRJ.ProjectStatus = ecProjectStatus.psArchived)))%>,
                displayExpr: "text",
                text: "Add New", 
                icon: "fas fa-user-plus",
                useSelectMode: false,
                wrapItemText: false,
                dropDownOptions: {
                    width: 300,
                },
                items: [{
                    id: "add_clipboard_users",
                    text: resString("btnAddFromClipboard"),
                    disabled: isReadOnly
                }, {
                    id: "add_from_workgroup_users",
                    text: resString("btnAddFromWorkgroup"),
                    visible: <% =Bool2JS(CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS) %>,
                    disabled: isReadOnly,
                }, {
                    id: "add_ad_users",
                    text: resString("btnAddUserFromAD"),
                    disabled: isReadOnly
                }],
                onItemClick: function (e) {
                    onClickToolbar(e.itemData.id);
                }
            }
        },
        <%If CurrentPageID = _PGID_PROJECT_USERS Or CurrentPageID = _PGID_RISK_INVITE_USERS Then %>
        {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxDropDownButton',
            visible: !isReadOnly,
            disabled: <%=Bool2JS(PRJ.ProjectStatus = ecProjectStatus.psTemplate OrElse PRJ.ProjectStatus = ecProjectStatus.psArchived)%>,
            options: {
                type: "normal",
                width: "78px",
                elementAttr: {id: 'btn_edit'},
                text: resString("btnEdit"), 
                icon: "fas fa-user-edit",
                displayExpr: "text",
                useSelectMode: false,
                wrapItemText: false,
                dropDownOptions: {
                    width: 120,
                },
                items: [],
                onButtonClick: function (e) {
                    var checked = (getCheckedUsersCount() != 0);
                    var items = [{
                        text: "Copy Judgments",
                        disabled: users_count() <= 1,
                        visible: !isRiskUsers,
                        onClick: function () {
                            copyJudgments();
                        }
                    }, {
                        text: "Erase Judgments",
                        disabled: !checked,
                        visible: !isRiskUsers,
                        onClick: function () {
                            eraseJudgments();
                        }
                    }, {
                        text: "Enable",
                        disabled: !checked,
                        onClick: function () {
                            toggleUserState(true);
                        }
                    }, {
                        text: "Disable",
                        disabled: !checked,
                        onClick: function () {
                            toggleUserState(false);
                        }
                    }, {
                        text: "Erase password",
                        disabled: !checked,
                        visible: <% =Bool2JS(AllowBlankPsw) %>,
                        onClick: function () {
                            dxConfirm(resString("msgConfirmResetUsersPsw"), function() { eraseUserPsw(); });
                        }
                    }];
                    e.component.option('items', items);
                },
            }
        },
        <%End If%>
        <%If CurrentPageID = _PGID_ADMIN_USERSLIST Then %>
        {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            options: {
                icon: "fas fa-user-times",
                text: "Remove",
                hint: "Remove",
                disabled: false,
                width: "105px",
                elementAttr: {id: 'btn_remove'},
                onClick: function () {
                    onClickToolbar("remove_wg_users");
               }
            }
        },
        <%End If%>
        <%If CurrentPageID = _PGID_ADMIN_USERSLIST Then %>
        {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            options: {
                icon: "fas fa-user-slash",
                text: "Disable",
                hint: "Disable",
                disabled: false,
                elementAttr: {id: 'btn_disable'},
                onClick: function () {
                    onClickToolbar("disable_wg_users");
                }
            }
        },
        {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            options: {
                icon: "fas fa-user-check",
                text: "Enable",
                hint: "Enable",
                disabled: false,
                elementAttr: {id: 'btn_enable'},
                onClick: function () {
                    onClickToolbar("enable_wg_users");
                }
            }
        },
        <%End If%>
        <%If CurrentPageID = _PGID_ADMIN_USERSLIST Then %>
        {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxDropDownButton',
            options: {
                type: "normal",
                width: "152px",
                elementAttr: {id: 'btn_permission'},
                text: "Set Permissions", 
                icon: "fas fa-users",
                displayExpr: "text",
                useSelectMode: false,
                wrapItemText: false,
                dropDownOptions: {
                    width: 280,
                },
                items: [<%=GetRoleGroupsMenu()%>],
            }
        },
        <%End If%>
        <%If CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS Then %>
        {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxDropDownButton',
            options: {
                type: "normal",
                width: "152px",
                elementAttr: {id: 'btn_permission'},
                text: "Set Permissions", 
                icon: "fas fa-users",
                displayExpr: "text",
                useSelectMode: false,
                wrapItemText: false,
                dropDownOptions: {
                    width: 280,
                },
                items: [],
                onButtonClick: function (e) {
                    var checked = (getCheckedUsersCount() != 0);
                    var items = [{
                        text: "Evaluator",
                        disabled: !checked,
                        onClick: function () {
                            setUserRole(0);
                        }
                    }, {
                        text: "Project Manager",
                        disabled: !checked,
                        onClick: function () {
                            setUserRole(1);
                        }
                    }, {
                        text: "Viewer",
                        disabled: !checked,
                        onClick: function () {
                            setUserRole(2);
                        }
                    }, {   
                        text: "Evaluator/Viewer",
                        disabled: !checked,
                        onClick: function () {
                            setUserRole(3);
                        }
                    }];
                    e.component.option('items', items);
                }
            }
        },
        <%End If%>
        <%If CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS Then %>
        {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxDropDownButton',
            options: {
                type: "normal",
                width: "105px",
                elementAttr: {id: 'btn_remove'},
                text: "Remove",  
                icon: "fas fa-user-times",
                displayExpr: "text",
                useSelectMode: false,
                wrapItemText: false,
                dropDownOptions: {
                    width: 250,
                },
                items: [],
                onButtonClick: function (e) {
                    var checked = (getCheckedUsersCount() != 0);
                    var items = [{
                        text: "Remove selected participants",
                            disabled: !checked,
                        onClick: function () {
                            dxConfirm(resString("msgConfirmDeleteSelectedUsers"), function() {removeParticipants();});                                
                        }
                    }, {
                        text: "Remove all participants who have not completed their judgments",
                        disabled: false,
                        visible: !isRiskUsers,
                        onClick: function () {
                            dxConfirm(resString("msgConfirmDeleteUsersNotCompJudg"), function() {removeParticipantsByJudgments("not_comp");});                                
                        }
                    }, {
                        text: "Remove all participants who have no judgments",
                        disabled: false,
                        visible: !isRiskUsers,
                        onClick: function () {
                            dxConfirm(resString("msgConfirmDeleteUsersWithNoJudg"), function () {removeParticipantsByJudgments("no_judgm");});
                        }
                    }];
                    e.component.option('items', items);
                }
            }
        }, {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            visible: !isReadOnly && !isRiskUsers && !isWidget,
            options: {
                icon: "fas fa-users-cog",
                text: "Manage Groups",
                hint: "Manage Participant Groups",
                type: "normal",
                elementAttr: {id: 'btn_manage_res_groups'},
                onClick: function () {
                    initResGroups(); initResGroupsDlg();
                }
            }
        }, {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            visible: !isReadOnly && !isRiskUsers && !isWidget,
            options: {
                icon: "fas fa-tasks",
                text: "Manage Attributes",
                hint: "Manage Participant Attributes",
                type: "normal",
                elementAttr: {id: 'btn_manage_attributes'},
                onClick: function () {
                    manageAttributes();
                }
            }
        }, {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            text: "",
            visible: !isReadOnly && !isRiskUsers && !isWidget,
            options: {
                icon: "fas fa-folder-plus",
                hint: "Add Attribute Using Survey Question",
                type: "normal",
                elementAttr: {id: 'btn_add_attribute_from_survey'},
                onClick: function () {
                    addAttributeUsingSurvey();
                }
            }
        },
        {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            //showText: "never",
            //text: "Filter...",
            text: "",
            visible: !isReadOnly && !isRiskUsers && !isWidget,
            options: {
                icon: "fas fa-filter",
                hint: "<%=JS_SafeString("Filter by participant attributes and create dynamic groups")%>",
                type: "normal",
                elementAttr: {id: 'btn_filter_users'},
                onClick: function () {
                    filterUsersDialog();
                }
            }
        }, 
        <%End If%>
        //{
        //    location: 'before',
        //    locateInMenu: 'auto',
        //    widget: 'dxButton',
        //    options: {
        //        icon: "fas fa-clipboard",
        //        text: "Export",
        //        hint: "Export",
        //        disabled: false,
        //        elementAttr: {id: 'btn_export'},
        //        onClick: function () {
        //            onClickToolbar("export_wg_users");
        //        }
        //    }
        //},
        <%If CurrentPageID = _PGID_ADMIN_USERSLIST Then %>
        {   
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxButton',
            options: {
                elementAttr: {id: 'btn_erase_psw'},
                text: "Erase Password", 
                disabled: (getCheckedUsersCount() == 0),
                icon: "fas fa-key",
                visible: <% =Bool2JS(AllowBlankPsw) %>,
                onClick: function () {
                    dxConfirm(resString("msgConfirmResetUsersPsw"), function() { eraseUserPsw(); });
                }            
            }
        },
        <%End If%>
        {
            location: 'before',
            //beginGroup: true,
            locateInMenu: 'auto',
            widget: 'dxButton',
            visible: false,
            options: {
                icon: "fas fa-envelope",
                text: "Send Mail",
                hint: "Send Mail",
                disabled: false,
                elementAttr: {id: 'btn_send_mail'},
                onClick: function () {
                    <%If CurrentPageID = _PGID_ADMIN_USERSLIST Then %>
                    onClickToolbar("send_mail_wg_users");
                    <%End If%>
                    <%If CurrentPageID = _PGID_PROJECT_USERS Or CurrentPageID = _PGID_RISK_INVITE_USERS Then %>
                    onClickToolbar("send_mail_prj_users");
                    <%End If%>
                }
            }
        }
    ];

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItems,
            //height: 47
        });
    }

    function onClickToolbar(btn_id, value) {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null) { 
            switch (btn_id + "") {
                case "add_clipboard_users":
                    addParticipants(<% =If(CurrentPageID = _PGID_ADMIN_USERSLIST, 3, 0) %>);
                    break;
                case "add_from_workgroup_users":
                    addParticipants(1);
                    break;
                case "add_ad_users":
                    addParticipants(4);
                    break;
                case "remove_wg_users":
                    dxConfirm(resString("msgConfirmDeleteSelectedUsers"), function() {                        
                        var ids = getCheckedUsersEmailsArr();
                        sendCommand('action=remove_wg_users&lst=' + getCheckedUsersEmails());
                        removeUsers(ids);
                    });
                    break;
                case "disable_wg_users":
                    var ids = getCheckedUsersEmailsArr();
                    for (var i = 0; i < ids.length; i++) {
                        var usr = getUserByEmail(ids[i]);
                        if ((usr) && usr[IDX_WG_USER_ROLE_ID] != wkg_admin) usr[IDX_WG_USER_DISABLED] = true;
                    }
                    sendCommand('action=disable_wg_users&lst=' + getCheckedUsersEmails());
                    break;
                case "enable_wg_users":
                    var ids = getCheckedUsersEmailsArr();
                    for (var i = 0; i < ids.length; i++) {
                        var usr = getUserByEmail(ids[i]);
                        if ((usr)) usr[IDX_WG_USER_DISABLED] = false;
                    }
                    sendCommand('action=enable_wg_users&lst=' + getCheckedUsersEmails());
                    break;
                case "permission_wg_user":
                    sendCommand('action=permission_wg_user&grp_id='+ value + '&lst=' + getCheckedUsersEmails());
                    break;
                    //case "export_wg_users":
                    //    break;
                case "send_mail_wg_users":
                    sendMails('wkg');
                    break;
                case "send_mail_prj_users":
                    sendMails('prj');
                    break;
                default:
                    notImplemented();
            }
        }
    }

    function disableWGUser(uid, chk) {
        if (chk) {
            sendCommand('action=disable_wg_users&lst=' + uid);
        } else {
            sendCommand('action=enable_wg_users&lst=' + uid);
        }
    }

    function showWGUserDecisions(uid) {
        sendCommand('action=wg_user_decisions&uid=' + uid);
    }
    /* end Toolbar*/

    function sendMails(param) {
        var lst = getCheckedUsers(table_main_id);
        var chk_count = getCheckedUsersCount();

        if (chk_count <= SEND_MAIL_MAX_USERS_COUNT) {
            var ids = "";
            for (var i = 0; i < lst.length; i ++) {
                ids += (ids == "" ? "" : ",") + lst[i]["id"];
            }
            OpenSendMail("mode=" + param + "&lst=" + ids);
        } else {
            DevExpress.ui.notify("We're sorry. The maximum number of selected participants is " + SEND_MAIL_MAX_USERS_COUNT + ". You selected " + chk_count + ".", "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
        }
    }

    function onShowHelp(params) {     
        if ((window.opener) && (typeof window.opener.onShowHelp)!="undefined") { window.opener.onShowHelp(params); return true; } 
        if ((window.parent) && (typeof window.parent.onShowHelp)!="undefined") { window.parent.onShowHelp(params); return true; } 
        return false;
    }

    function onShowLDAPHelp() {
         onShowHelp("<% =JS_SafeString(ResString("msgLDAPHelp"))%>");
    }

    function Hotkeys(event) {
        if (!document.getElementById) return false;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            //if (code == 13) {
            //    if (cur_edit > 0) {
            //        cancelEditUser(cur_edit, true);
            //        return false;
            //    }
            //}
            //if (code == 27) {
            //    if (cur_edit > 0) {
            //        cancelEditUser(cur_edit, false);
            //        return false;
            //    }
            //}
        }
    }

    function showResultUsers(count, sAction) {
        if (count > 0) {
            DevExpress.ui.notify(count + " participant" + (count > 1 ? "s were " : " was ") + sAction, "success", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
        }
    }

    /* Callback */
    var continue_cmd = "";
    var current_command_silent = undefined;

    function sendCommand(params, skipCheckingUnsavedData, silent) {       
        current_command_silent = silent;

        if (typeof skipCheckingUnsavedData != "undefined" && skipCheckingUnsavedData) {
            callAjax(params, syncReceived, ajaxMethod, current_command_silent);
        } else {
            continue_cmd = params;
            onBeforeToolbarClick();
        }
    }

    function continueCommand() {
        callAjax(continue_cmd, syncReceived, ajaxMethod, current_command_silent);
        continue_cmd = "";
    }

    function syncReceived(params) {
        displayLoadingPanel(false);

        if ((params)) {
            var rd = eval(params);
            //var rd = JSON.parse(params);

            if (rd[0] == "refresh_full") {
                users_data = rd[1];
                attributes_data = rd[2];                
                //refreshGrid();
                initTable();
            }

            if (rd[0] == "save_changes") {                
                if (rd[1] == '') {
                    //DevExpress.ui.notify("Changes Saved", "success", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                    users_data = rd[2];
                    res_groups_data = rd[3];
                    initTable();
                } else {
                    showErrorMessage(rd[1], true);
                    //DevExpress.ui.notify(rd[1], "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                }
            }

            if (rd[0] == "del_res_group" || rd[0] == "rename_res_group") {
                res_groups_data = rd[2];
                initTable();
                updateToolbar();
                initGroupsCombobox("");
                initGroupsCombobox("AD");
                initGroupsCombobox("PRJ");
            }

            if (rd[0] == "add_res_group" || rd[0] == "res_groups_reorder") {    
                users_data = rd[1];
                res_groups_data = rd[2];
                initTable();
                updateToolbar();
                initGroupsCombobox("");
                initGroupsCombobox("AD");
                initGroupsCombobox("PRJ");
            }
       
            if (rd[0] == "add_res_group" || rd[0] == "del_res_group" || rd[0] == "rename_res_group") {                
                initResGroups(); 
                var show_dlg = true;
                var cb = document.getElementById("cbUserGroups");
                if ((cb)) show_dlg = false;
                if (show_dlg) initResGroupsDlg(); else $("#tbName").focus();
                updateToolbar();
            }

            if (rd[0] == "filter_by_attr") {                
                users_data = rd[1];
                refreshGrid();
            }

            if (rd[0] == "add_users" || rd[0] == "add_from_wg" || rd[0] == "add_prj_users_from_ad") {                
                users_data = rd[1];
                res_groups_data = rd[2];
                if (rd[3] != '') dxDialog(rd[3], ";", null, resString("lblInformation"));
                refreshGrid();
                updateToolbar();

                if (rd[0] == "add_from_wg") {
                    var is_proceed_pms = (rd.length > 5) && rd[5];

                    if (is_proceed_pms) {
                        dxConfirm(rd[4], "sendCommand('action=set_pms');", ";");
                    }
                }
            }

            if (rd[0] == "remove_users") {
                //users_data = rd[1];
                showResultUsers(rd[2], "removed.");
                refreshGrid();
                updateToolbar();
            }

            if (rd[0] == "remove_users_by_judg") {
                users_data = rd[1];
                showResultUsers(rd[2], "removed.");
                refreshGrid();
                updateToolbar();
            }

            if (rd[0] == "unlock") {
                var user_email = rd[1];
                for (var i = 0; i < users_data.length; i++) {
                    if (users_data[i][IDX_EMAIL] == user_email) {
                        users_data[i][IDX_LOCKED] = (rd[2]);
                        refreshGrid();
                        DevExpress.ui.notify(user_email + " unlocked.", "success", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                        break;
                    }
                }
            }

            //if (rd[0] == "set_user_priority") {                
            //    //users_data = rd[1];
            //    refreshGrid();
            //    // eval(on_priority_ok_code); // - uncomment to highlight the edited weight field
            //}
        
            if (rd[0] == "get_link" || rd[0] == "get_controls_link" || rd[0] == "open_link" || rd[0] == "view") {
                var action = rd[0];
                if (rd[0] == "view") action ="open_link";
                user_link = rd[1];
                
                if (isWorkgroupUsers) {
                    ProceedLinkAction(user_link, action, false);
                } else {
                    var is_prj_online = rd[2] == "1";
                    var can_set_online = rd[3] == "1";
                    var is_resp = typeof rd[4] != "undefined" && (rd[4]);
                    if (!is_prj_online && can_set_online) {
                        var proceedCommand = function () {
                            ProceedLinkAction(user_link, action, is_resp);
                        }

                        var dlg = DevExpress.ui.dialog.custom({
                            title: resString("titleConfirmation"),
                            messageHtml: "<div style='max-width:600px'>" + replaceString("\n", "<br>", resString("msgSetProjectStatusOnline")) + "</div>",
                            buttons: [{
                                text: resString("btnSetPrjOnline"),
                                onClick: function(e) {
                                    sendCommand('action=set_prj_online');
                                    proceedCommand();
                                }
                            }, {
                                text: resString("btnLeavePrjOffline"),
                                onClick: function(e) {
                                    if (action != "open_link") proceedCommand();
                                }
                            }]
                        });
                        dlg.show();
                        //dxConfirm(resString("msgSetProjectStatusOnline"), "sendCommand('action=set_prj_online');" + proceedCommand, proceedCommand);
                    } else {
                        ProceedLinkAction(user_link, action, is_resp);
                    }       
                }     
            }

            if (rd[0] == "set_prj_online") {
                //proceedCopyLink();
            }

            if (rd[0] == "erase_judgments" || rd[0] == "copy_judgments" || rd[0] == "user_state" || rd[0] == "set_user_role" || rd[0] == "set_pms"|| rd[0] == "reset_psw") {
                var is_msg = (rd.length > 3) && (rd[3] == 1);
                var is_proceed_pms = (rd.length > 5) && rd[5];
                if (is_msg && !is_proceed_pms) {
                    showErrorMessage(rd[4], true);
                    //DevExpress.ui.notify(rd[4], "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                    //dxDialog(rd[4], ";", null, resString("lblInformation"));
                }

                if (is_msg && is_proceed_pms) {
                    dxConfirm(rd[4], "sendCommand('action=set_pms');", ";");
                }

                if (rd[0] == "erase_judgments" || rd[0] == "set_user_role" || rd[0] == "copy_judgments") users_data = rd[1];
                showResultUsers(rd[2], "updated.");
                refreshGrid();
            }

            if (rd[0] == "create_dyn_res_group") {                
                users_data = rd[1];
                res_groups_data = rd[2];
                initTable();
            }

            if (rd[0] == "get_workgroup_users") {
                dlgAddParticipantsFromWorkgroup(rd[1]);
            }

            if (rd[0] == "get_ldap_users") {
                displayLoadingPanel(false);
                if (rd[2].trim() == "") {
                    $("divAddADUsers").dxPopup("hide");
                    dlgAddParticipantsFromAD(rd[1]);
                } else {
                    showErrorMessage(rd[2], true);
                    //DevExpress.ui.notify(rd[2], "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                }
            }

            if (rd[0] == "add_clipboard_users" || rd[0] == "permission_wg_user" || rd[0] == "add_from_ad") {
                //if (rd[1] != "") DevExpress.ui.notify(rd[1], typeof rd[3] !== "undefined" && rd[3] ? "error" : "success", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                if (rd[1] != "") {
                    if (typeof rd[3] !== "undefined" && rd[3]) {
                        showErrorMessage(rd[1], true);
                    } else {
                        DevExpress.ui.notify(rd[1], "success", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                    }
                }
                users_data = rd[2];
                initTable();
            }

            if (rd[0] == "remove_wg_users" || rd[0] == "disable_wg_users" || rd[0] == "enable_wg_users") {
                if (rd[1] != "") DevExpress.ui.notify(rd[1], "success", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
                //users_data = rd[2];
                initTable();
            }

            if (rd[0] == "wg_user_decisions") {
                DevExpress.ui.dialog.alert(rd[1], "Models List");
            }

            //if (rd[0] == "password") {
            //    DevExpress.ui.dialog.alert(rd[2]=="" ? resString("msgSetPasswordDone") : rd[2], "Set Password");
            //}
            
            if (rd[0] == "get_filtered_users_data") {
                filtered_users_columns = rd[1];
                filtered_users_data = rd[2];
                drawUsersFilteredTable();
            }

            if (rd[0] == "add_column" || rd[0] == "del_column" || rd[0] == "rename_column") {
                attributes_data = rd[1];
                questions = rd[2];
                reopen_editor = true;
            }

            if (rd[0] == "attributes_reorder") {
                attributes_data = rd[1];
                users_data = rd[2];
                initTable();                
            }
            
            if (rd[0] == "set_default_value") {
                // do nothing
            }

            if (rd[0] == "create_attr_from_survey") {
                attributes_data = rd[1];
                users_data = rd[2];
                questions = rd[3];
                res_groups_data = rd[4];
                initTable();
                //refreshGrid();
                updateToolbar();
            }

            if (rd[0] == "open_project") {
                displayLoadingPanel(true);
                //window.location.reload(true); // reload this page OR
                navOpenPage(<%=_PGID_PROJECT_PROPERTIES%>);
            }

            if (continue_cmd != "") { continueCommand(); continue_cmd = ""; }
        }        

        if (reopen_editor) {
            manageAttributes();
        }
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
    }

    function openProject(prjId) {
        sendCommand("action=open_project&id=" + prjId);
    }
  
    $(document).ready(function () {
<%If App.HasActiveProject AndAlso App.ActiveProject.ProjectStatus = ecProjectStatus.psTemplate Then %>       setTimeout( function () { DevExpress.ui.notify("<% =JS_SafeString(ResString("lblProjectTemplateRO")) %>", "info"); }, 1000);<% End If %>
        displayLoadingPanel(true);
        initToolbar();
        initTable();
        initDragDropGroups();
        initDragDropAttributes();
        checkDatagridUnsavedData();

        //DevExpress.config({
        //    useLegacyVisibleIndex: true
        //});

        //setTimeout('$("#tableStructureUsers").dxDataGrid("instance").searchByText("");', 1000);
        <% If CheckVar("dlg", "").ToLower = "groups" Then %>setTimeout(function () {
            var btn = $("#btn_manage_res_groups");
            if ((btn) && (btn.data("dxButton")) && !(btn.dxButton("option", "disabled"))) btn.dxButton("option", "onClick")();
        }, 500);
        <% End If %>
    });
    
    document.onkeypress = Hotkeys;

    //window.onbeforeunload = function (event) {
    //    var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
    //    if (table_main !== null && table_main.hasEditData()) { 
    //        return resString("msgUnsavedData");
    //    }
    //};

    var e1;

    function onLeavePage(e) {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null && table_main.hasEditData()) { 
            e1 = e;
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", "onLeavePage = null; onMenuItemClick(e1);");
            return false;
        }
        return true;
    }    

    function checkDatagridUnsavedData() {
        setTimeout(function () { oncheckDatagridUnsavedData(); }, 1000*60*5);
    }

    function oncheckDatagridUnsavedData() {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null && table_main.hasEditData()) { 
            dxConfirm("<% = JS_SafeString(ResString("msgSaveUnsavedData")) %>", "saveEditData(); checkDatagridUnsavedData();", "checkDatagridUnsavedData();");
        } else { 
            checkDatagridUnsavedData(); 
        }
    }

    function saveEditData() {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null && table_main.hasEditData()) table_main.saveEditData();
    }

    function cancelEditData() {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null && table_main.hasEditData()) table_main.cancelEditData();
    }

    function onBeforeToolbarClick() {
        var table_main = (($("#" + table_main_id).data("dxDataGrid"))) ? $("#" + table_main_id).dxDataGrid("instance") : null;
        if (table_main !== null && table_main.hasEditData()) {             
            dxConfirm("<% = JS_SafeString(ResString("msgSaveUnsavedData")) %>", "saveEditData();", "cancelEditData();");
        } else {
            continueCommand();
            continue_cmd = "";
        }
    }

</script>

<table border="0" cellpadding="0" cellspacing="0" class="whole">
<tr style="height:32px">
    <td><div id='toolbar' class="dxToolbar"></div></td>
</tr>
<tr id="trUsersGrid">
    <td><div id="tableStructureUsers" class="whole" style="margin: 0px;"></div></td>
</tr>
</table>

<%If CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS Then%>
<%-- Filter Users By Attributes --%>
<div id="divUserAttributes" style="display:none;">
    <div style="padding:0px 5px 0px 5px; text-align:left; width: 100%;">
        <!-- Filter combinations toolbar -->
        <div style='text-align: left; margin-top:1ex' class='text'>
            <nobr><div id="cbUsersFiltersCombination" style="display: inline-block;"></div><%--<select id="cbUsersFiltersCombination">
                <option value='0'<% =If(FilterCombination=0, " selected", "") %>>AND</option>
                <option value='1'<% =If(FilterCombination=1, " selected", "") %>>OR</option>                
            </select>--%> &nbsp;          
            <%--<input type='button' class='button' style='width:13ex' id='btnUsersFilterAddRule' value='Add&nbsp;Rule' onclick='onUsersFiltersAddNewRule(); return false;' />--%>
            <a href="" id="btnAddNewRule" title="Add Rule" onclick="onUsersFiltersAddNewRule(); return false;"><i class="fas fa-plus fa-2x" style="color: MediumSeaGreen;"></i></a>&nbsp;
            <%--<input type='button' class='button' style='width:13ex' id='btnUsersFilterApplyFilter' value='Preview' onclick='initUsersFilteredTable(); return false;' />--%>
            <a href="" title="Preview" onclick="initUsersFilteredTable(); return false;"><i class="fas fa-check fa-2x" style="color: MediumSeaGreen;"></i></a>&nbsp;
            <%--<input type='button' class='button' style='width:13ex' id='btnUsersFilterResetFilter' value='Clear' onclick='onUsersFiltersReset(); return false;' /></nobr>--%>
            <a href="" title="Clear" onclick="onUsersFiltersReset(); return false;"><i class="fas fa-eraser fa-2x" style="color: LightCoral;"></i></a>
            <%--<input type='button' class='button' style='width:17ex' id='btnUsersFilterCreateResGroup' value='Create Group' onclick='onCreateDynamicResGroup(); return false;' />--%>
            </nobr>
        </div>

        <p align="left" id='divUsersFilters' style="margin:3px 3px 3px 20px;">Loading...</p>
    </div>
    <div style='width:100%;'>
        <%--<table id='tableFilteredUsers' class='text cell-border hover order-column' style='width:100%; vertical-align: top;'></table>--%>
        <div id="tableFilteredUsers"></div>
    </div>
</div>
<div id="divSurveyAttributes" style="display:none;">
    <h5>Select a survey question:</h5>
    <div style='width:100%;'>
        <div id="divQuestionsList" style="border: 1px solid grey;"></div>
    </div>
    <div style='width:100%; margin: 20px;'>
        Attribute name: <input id="tbAttributeName" type="text" style="width: 350px;" />
    </div>
    <br/>
    <label><input id="cbCreateGroups" type="checkbox" onclick="cbCreateGroupsChange(this.checked);" onchange="cbCreateGroupsChange(this.checked);" />Create <span id="lblDynGroupsCount"></span> dynamic groups using the answers of the selected survey question</label>
    <br/><br/>
    <div style='width:100%;'>
        <div id="divAnswersList" style="border: 1px solid grey;"></div>
    </div>
</div>
<%End If%>

<div id="divResGroups" style="display: none; text-align: center; height: 100%;">
<h6><%=JS_SafeString(ResString("lblGroups"))%>:</h6>
<div style="overflow: auto; width: 100%; height: 100px; display: block;" id="pResGroups"><table id="tblResGroups" border='0' cellspacing='1' cellpadding='2' class='grid drag_drop_grid_groups' style="display: block;">
    <thead>
      <tr class="text grid_header" align="center">
        <th style="width:1em">&nbsp;</th> <%--Drag-drop handle column--%>
        <th style="width:80%;"><%=JS_SafeString(ResString("lblGroupName"))%></th>
        <th style="width:80px;"><%=JS_SafeString(ResString("tblRAScenarioAction"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
<div style="height: 26px;"></div>
<div id="btnResGroupsClose" style="position: absolute; bottom: 5px; left: 50%; margin-left: -75px; width: 150px;"></div>
</div>

<div id="divAddWGUsers" style="display:none; text-align:center; padding-bottom: 35px;">
    <div style="overflow: auto;" id="pAddWGUsers">
        <center>
            <button id="btnPasteWGUsers" onclick="this.disabled = true; pasteWGUsers(); return false;" class='button' type="button" style="width:205px; height:25px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-paste"></i>&nbsp;<%=ResString("btnPasteUsers")%></button>
        </center>
        <p class="text" style="text-align: left;"><%=SafeFormString(ResString("msgSignUpParticipantsComment"))%></p>
        <textarea rows="7" id="tbAddWGUsers"  border='0' style='width: 96%;'>
        </textarea>
        <br/>
        <br/>
        <%If CurrentPageID = _PGID_PROJECT_USERS OrElse CurrentPageID = _PGID_RISK_INVITE_USERS Then %>
        <div style="border: 1px solid #d0d0d0;"><nobr>
            <span>All Participants added will be members of Group:</span><br/>
            <select id="cbUserGroupsPRJ" style="display: inline-block; margin-top: 1px; height: 24px; width: 200px;" onchange="onUserGroupChanged(this,'PRJ'); return false;">
                <option value="0">No Group</option>
            </select>
            <button id="btnCreateGroupPRJ" onclick="createUserGroupFromAddUsersDlg('PRJ'); return false;" class='button' type="button" style="width:125px; height:24px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-plus-circle"></i>Create Group</button>
            <button id="btnRemoveGroupPRJ" onclick="removeUserGroupFromAddUsersDlg('PRJ'); return false;" disabled="disabled" class='button' type="button" style="width:125px; height:24px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-minus-circle"></i>Remove Group</button>
        </nobr></div><br/>
        <%End If%>
        <div style="text-align: left; margin-left: 20px;">
            <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %><label id="lblWGGeneratePassword" class="text">
                <input type="checkbox" class="checkbox" id="cbWGGenerateRandPass" />&nbsp;Generate Random Password</label><br /><% End If %>
            <label id="lblWGSendRegNotification" class="text">
                <input type="checkbox" class="checkbox" id="cbWGSendRegNotification" />&nbsp;Send a registration notification to user</label>
        </div>
    </div>
    <div style="height: 26px;"></div>
    <div style="position: absolute; bottom: 5px; left: 50%; margin-left: -150px; width: 300px;">
        <div id="btnAddWGUsersClose" style="width: 148px;"></div>
        <div id="btnAddWGUsersCancel" style="width: 148px;"></div>
    </div>

</div>

<div id="divAddADUsers" style="display:none; text-align:center; padding-bottom: 35px;">
    <div style="overflow: auto;" id="pAddUsers">
        <div id="divADUsers" style="padding: 4px;"><br/>
            <div style="text-align: center;">LDAP Server:
                <input type="text" value="" id="tbLDAPServer" />&nbsp;Username:<input type="text" id="tbLDAPDN" />&nbsp;Password:<input type="text" id="tbLDAPPW" />&nbsp;
                <button type="button" onclick="getADUsers();">
                    <i class="fas fa-plug"></i>
                    Connect
                </button>
                <a href="" style="margin-left: 15px;" onclick="onShowLDAPHelp(); return false;">
                    <i class="fas fa-question-circle fa-2x" style="vertical-align: middle;"></i>
                </a>
            </div>
            <br />
        </div>
    </div>
    <div style="height: 26px;"></div>
    <div id="btnAddADUsersClose" style="position: absolute; bottom: 5px; left: 50%; margin-left: -75px; width: 150px;"></div>
</div>

<div id="divAddUsers" style="display:none; text-align:center; padding-bottom: 35px;">
    <div style="overflow: auto;" id="pAddUsers">
        <center>
            <button id="btnPasteUsers" onclick="this.disabled = true; pasteUsers(); return false;" class='button' type="button" style="width:205px; height:25px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-paste"></i>&nbsp;<%=ResString("btnPasteUsers")%></button>
        </center>
        <table id="tblAddUsers" border='0' cellspacing='1' cellpadding='2' style='width: 96%;' class='grid'>
            <thead>
                <tr class="text grid_header" align="center">
                    <th style="width: 45%;"><%=JS_SafeString(ResString("tblUserEmail"))%></th>
                    <th style="width: 45%;"><%=JS_SafeString(ResString("tblSyncUserName"))%></th>
                    <th style="width: 10%;"><%=JS_SafeString(ResString("tblActions"))%></th>
                </tr>
            </thead>
            <tbody>
            </tbody>
        </table>
        <a href='' onclick='addUserLine(); return false;'><i class="fas fa-plus"></i>Add Row</a>
        <div style="text-align: left;">
        <p class="text"><%=ResString("msgAddParticipantsComment")%></p>
        
        <div style="border: 1px solid #d0d0d0;"><nobr>
            <span><%=ResString("lblAddParticipantsToGroup")%></span><br/>
            <select id="cbUserGroups" style="display: inline-block; margin-top: 1px; height: 24px; width: 200px;" onchange="onUserGroupChanged(this,''); return false;">
                <option value="0">No Group</option>
            </select>
            <button id="btnCreateGroup" onclick="createUserGroupFromAddUsersDlg(''); return false;" class='button' type="button" style="width:125px; height:24px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-plus-circle"></i><%=ResString("btnAddGroup")%></button>
            <button id="btnRemoveGroup" onclick="removeUserGroupFromAddUsersDlg(''); return false;" disabled="disabled" class='button' type="button" style="width:125px; height:24px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-minus-circle"></i><%=ResString("btnRemoveGroup")%></button>
        </nobr></div><br/>
        <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %><label id="lblGeneratePassword" class="text">
            <input type="checkbox" class="checkbox" id="cbGenerateRandPass" />&nbsp;Generate Random Password</label><br /><% End If %>
        <label id="lblSendRegNotification" class="text">
            <input type="checkbox" class="checkbox" id="cbSendRegNotification" />&nbsp;Send a registration notification to user</label>
        </div>
    </div>
    <div style="height: 26px;"></div>
    <div style="position: absolute; bottom: 5px; left: 50%; margin-left: -150px; width: 300px;">
        <div id="btnAddUsersClose" style="width: 148px;"></div>
        <div id="btnAddUsersCancel" style="width: 148px;"></div>
    </div>

</div>

<div id="divWGUsersDialog" style="display:none; text-align:center; padding-bottom: 35px;">
    <h6 id='lblWGUsers'></h6>
    <div id="tableWGUsers"></div>
    <div style="text-align: left;">
        <label id="lblWGADAsPMs" class="text"><input type="checkbox" class="checkbox" id="cbWGADAsPMs" />&nbsp;Add All Participants As Project Managers</label><br />
        <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %><label id="lblWGADGeneratePassword" class="text"><input type="checkbox" class="checkbox" id="cbWGADGenerateRandPass" />&nbsp;Generate Random Password</label><br /><% End If %>
        <label id="lblWGADSendRegNotification" class="text"><input type="checkbox" class="checkbox" id="cbWGADSendRegNotification" />&nbsp;Send a registration notification to user</label>
    </div>
    <div id="lblGroupsUI" style="text-align: left; border: 1px solid #d0d0d0; padding: 2px;">
        <nobr>
            <span><%=ResString("lblAddParticipantsToGroup")%></span><br/>
            <select id="cbUserGroupsAD" style="display: inline-block; margin-top: 1px; height: 24px; width: 200px;" onchange="onUserGroupChanged(this,'AD'); return false;">
                <option value="0">No Group</option>
            </select>
            <button id="btnCreateGroupAD" onclick="createUserGroupFromAddUsersDlg('AD'); return false;" class='button' type="button" style="width:125px; height:24px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-plus-circle"></i><%=ResString("btnAddGroup")%></button>
            <button id="btnRemoveGroupAD" onclick="removeUserGroupFromAddUsersDlg('AD'); return false;" disabled="disabled" class='button' type="button" style="width:125px; height:24px; vertical-align:top; margin-bottom:5px;"><i class="fas fa-minus-circle"></i><%=ResString("btnRemoveGroup")%></button>
        </nobr><br/>
    </div>
    <div style="height: 26px;"></div>
    <div style="position: absolute; bottom: 5px; left: 50%; margin-left: -150px; width: 300px;">
        <div id="btnWGUsersClose" style="width: 148px;"></div>
        <div id="btnWGUsersCancel" style="width: 148px;"></div>
    </div>
</div>

<div id="divCopyJudgments" style="display:none; text-align:center; padding-bottom: 35px;">
    <table border="0" class="whole text">
    <tr>
        <td align="left" style="width:50%;"><h5 style='text-align:left;'>Copy FROM:</h5></td>
        <td align="left" style="width:50%;"><h5 style='text-align:left;'>Copy TO:</h5></td>
    </tr>
    <tr style="overflow:hidden;">
        <td align="left" valign="top" style="width:50%;">
            <div class="text" style="margin:0px 21px 2px 0px; text-align:right;"><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearchLeft" type="text" style="width:100px;" value="" onkeyup="onSearchLeft(this.value)" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' /></label></div>
            <div id="divLeft" style="text-align:left; padding:5px; width:90%; height:180px; background-color:white; overflow:auto;"></div>
        </td>
        <td align="left" valign="top" style="width:50%;">
            <div class="text" style="margin:0px 21px 2px 0px; text-align:right;"><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearchRight" type="text" style="width:100px;" value="" onkeyup="onSearchRight(this.value)" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' /></label></div>
            <div id="divRight" style="text-align:left;padding:5px; width:90%; height:180px; background-color:white; overflow:auto;"></div>
            <div id="divSelectAll" style='white-space:nowrap; text-align:center; display:block; margin:2px;' class='text'><a href='' onclick='selectAllUsersInDlg(true); return false;' class='actions'><%=ResString("lblAll")%></a> | <a href='' onclick='selectAllUsersInDlg(false); return false;' class='actions'><%=ResString("lblSelectNone")%></a></div>
        </td>
    </tr>
    <tr>
        <td colspan="2" align="center">
            <center>
                <fieldset class="legend" style="text-align: left; vertical-align: top;" id="pnlCopyJudgmentsOptions">
                    <legend class="text legend_title">&nbsp;Settings&nbsp;</legend>
                    <input type='radio' class='radio' name='rbMode' checked='checked' id='rbMode0' value="0" /><label for='rbMode0'>Fully rewrite (replace)</label><br/>
                    <input type='radio' class='radio' name='rbMode' id='rbMode1' value="1" /><label for='rbMode1'>Update existing judgments and copy where participant doesn't have judgments</label><br/>
                    <input type='radio' class='radio' name='rbMode' id='rbMode2' value="2" /><label for='rbMode2'>Copy only where participant doesn't already have judgments</label><br/><br/>
                    <input type='checkbox' class='checkbox' id='cbPWOnly' /><label for='cbPWOnly'>Pairwise judgments only</label>
                </fieldset>
            </center>
        </td>
    </tr>     
    </table>
    <div style="height: 26px;"></div>
    <div style="position: absolute; bottom: 5px; left: 50%; margin-left: -150px; width: 300px;">
        <div id="btnCopyJudgmentsOK" style="width: 148px;"></div>
        <div id="btnCopyJudgmentsCancel" style="width: 148px;"></div>
    </div>
</div>

<div id="divAttributes" style="display:none; text-align:center; height: 100%;">
<h6><%=JS_SafeString(ResString("lblRAColumns"))%>:</h6>
<div style="overflow: auto; width: 100%;" id="pAttributes"><table id="tblAttributes" border='0' cellspacing='1' cellpadding='2' class='grid drag_drop_grid_attrs'>
    <thead>
      <tr class="text grid_header" align="center">
        <th style="width:1em">&nbsp;</th> <%--Drag-drop handle column--%>
        <th style="width:40%" colspan="2"><%=JS_SafeString(ResString("tblRAAttributeName"))%></th>
        <th width="100"><%=JS_SafeString(ResString("tblAttributeType"))%></th>
        <th width="80" colspan="2"><%=JS_SafeString("Default Value")%></th>
        <th width="80"><%=JS_SafeString(ResString("tblRAScenarioAction"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
<div style="height: 26px;"></div>
<div id="btnAddAttributesClose" style="position: absolute; bottom: 5px; left: 50%; margin-left: -75px; width: 150px;"></div>
</div>

<script language='javascript' type='text/javascript'>

    // Manage Participant Attributes
    var attributes_data = <% =GetAttributesData() %>;

    var avtString       = 0;
    var avtBoolean      = 1;
    var avtLong         = 2;
    var avtDouble       = 3;
    var avtEnumeration  = 4;
    var avtEnumerationMulti = 5;

    var SelectedAttrIndex = 0;
    var SelectedItemIndex = 0;

    var item_name = "";
    var on_hit_enter_cat = "";
    
    var reopen_editor = false;

    function manageAttributes() {
        initAttributes();
        initAttributesDlg();
    }

    function initAttributes() {
        var t = $("#tblAttributes tbody");
        
        if ((t)) {
            t.html("");        
            for (var i=0; i<attributes_data.length; i++) {
                var v = attributes_data[i];
                var n = htmlEscape(v[aidx_name]);
                
                var vals = "&nbsp;";
                var attr_type = v[aidx_type]*1;

                switch (attr_type) {
                    case avtString:
                    case avtLong:
                    case avtDouble:
                        if ((v[aidx_def_val])) vals = htmlEscape(v[aidx_def_val]);
                        break;
                    case avtBoolean:
                        vals = (v[aidx_def_val] == "1" ? "[<%=ResString("lblYes")%>]" : "[<%=ResString("lblNo")%>]");
                        break;
                }

                var sHidden = "";
                
                sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "' " + sHidden + ">";
                sRow += "<td " + sHidden + " align='center' style='width:20px;'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td " + sHidden + " id='tdName" + i + "''>" + n + "</td>";                
                sRow += "<td " + sHidden + " id='tdEditAction" + i + "' align='right'><a href='' onclick='onEditAttribute(" + i + "); return false;'><i class='fas fa-pencil-alt'></i></a></td>";
                sRow += "<td " + sHidden + " id='tdType" + i + "' align='center'>" + getAttrTypeName(v[aidx_type]) + "</td>";
                sRow += "<td " + sHidden + " id='tdValues" + i + "'>" + vals + "</td>";
                sRow += "<td " + sHidden + " id='tdEditValues" + i + "' align='right'><a href='' onclick='onEditAttributeValues(" + i + ","+v[aidx_type]+"); return false;'><i class='fas fa-pencil-alt'></i></a></td>";
                sRow += "<td " + sHidden + " id='tdActions" + i + "' align='center'><a href='' onclick='DeleteAttribute(" + i + "); return false;'><i class='fas fa-trash-alt'></i></a></td>";
                sRow +="</tr>";
                t.append(sRow);
            }

            sRow = "<tr class='text grid_footer' id='trNew'>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width:100%; vertical-align: middle;' id='tbAttrName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='if (event.keyCode == KEYCODE_ENTER) AddAttribute();'></td>";
            sRow += "<td><select class='select' style='width:120px; vertical-align: middle;' id='tbType' onchange='cbNewAttrTypeChanged(this.value);' onkeydown='if (event.keyCode == KEYCODE_ENTER) AddAttribute();'>" + getAttrTypeOptions() + "</select></td>";
            //sRow += "<td>&nbsp;</td>"; // edit name icon
            sRow += "<td colspan='2'><nobr>&nbsp;<span id='lblDefaultValue' style='display:none; vertical-align: middle;'><%=ResString("lblDefaultAttrValue")%>:&nbsp;</span><input type='text' id='tbDefaultTextValue' onkeydown='if (event.keyCode == KEYCODE_ENTER) AddAttribute();' style='display:none; width:130px; vertical-align: middle;' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'><select id='tbDefaultBoolValue' class='select' style='display:none; width:80px; vertical-align: middle;' onkeydown='if (event.keyCode == KEYCODE_ENTER) AddAttribute();'><option value='1'><%=ResString("lblYes")%></option><option value='0'><%=ResString("lblNo") %></option></select></nobr></td>"; // values
            //sRow += "<td>&nbsp;</td>"; // edit values icon
            sRow += "<td align='center'><a href='' onclick='AddAttribute(); return false;' tite='<% = JS_SafeString(ResString("titleAddCC")) %>'><i class='fas fa-plus-square'></i></a></td></tr>";
            t.append(sRow);
            cbNewAttrTypeChanged(document.getElementById("tbType").value);
        }
    }

    function cbNewAttrTypeChanged(value) {
        $("#lblDefaultValue").hide();
        $("#tbDefaultTextValue").hide();
        $("#tbDefaultBoolValue").hide();
        
        if (value == avtString || value == avtLong || value == avtDouble) {
            $("#lblDefaultValue").show();
            $("#tbDefaultTextValue").show();
        }

        if (value == avtBoolean) {
            $("#lblDefaultValue").show();
            $("#tbDefaultBoolValue").show();
        }

        drag_index = -1;
    }

    function getAttrTypeOptions() {
        var retVal = "<option value='" + avtString + "'><%=ResString("optAttrTypeString")%></option>";
        retVal += "<option value='" + avtLong + "'><%=ResString("optAttrTypeInteger")%></option>";
        retVal += "<option value='" + avtDouble + "'><%=ResString("optAttrTypeDouble")%></option>";
        retVal += "<option value='" + avtBoolean + "'><%=ResString("optAttrTypeBoolean")%></option>";
        return retVal;
    }

    function getAttrTypeName(attr_type) {
        switch (attr_type*1) {
            case avtString: return "<%=ResString("optAttrTypeString") %>"; break;
            case avtBoolean: return "<%=ResString("optAttrTypeBoolean") %>"; break;
            case avtLong: return "<%=ResString("optAttrTypeInteger") %>"; break;
            case avtDouble: return "<%=ResString("optAttrTypeDouble") %>"; break;
            case avtEnumeration: return "<%=ResString("optAttrTypeEnum") %>"; break;
            case avtEnumerationMulti: return "<%=ResString("optAttrTypeMultiEnum") %>"; break;
        }
        return "";
    }

    function initAttributesDlg() {        
        $("#divAttributes").dxPopup({
            width: "auto",
            height: function() {
                return window.innerHeight * 0.8;
            },
            title: "<% = JS_SafeString(ResString("btnRAEditAttributes")) %>",
            onShown: function() {
                on_hit_enter = "AddAttribute();";
                $("#tbAttrName").focus();
            },
            onShown: function (e) {
                var h = window.innerHeight;
                $("#pAttributes").height(h > 95 ? h - 95 : 95);
                //$("#tblAttributes").css("max-height", (h > 95 ? h - 95 : 95) + "px");
                $("#tbAttrName").focus();
            },
            onHiding: function() {
                reopen_editor = false;
                initAttributes();
                on_hit_enter = "";
                if (drag_order!="") {
                    sendCommand("action=attributes_reorder&lst=" + encodeURIComponent(drag_order));
                    for (var i = 0; i < attributes_data.length; i++) {
                        attributes_data[i][aidx_id] = i; 
                    }
                    drag_order = "";
                } else {
                    refresh_full();
                }               
            },
            visible: true
        });

        $("#btnAddAttributesClose").dxButton({
            text: "<%=ResString("btnClose")%>",
            icon: "fas fa-times",
            onClick: function() {
                if (checkUnsavedData(document.getElementById("tbAttrName"), "$('#divAttributes').dxPopup('hide');")) $("#divAttributes").dxPopup("hide");                
            }
        });
    }

    function GetSelectedAttr() {
        var attr = null;
        if ((attributes_data) && (SelectedAttrIndex >= 0) && (SelectedAttrIndex < attributes_data.length)) {
            attr = attributes_data[SelectedAttrIndex];        
        }
        return attr;
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", on_agree + ";");
            return false;
        }
        return true;
    }

    function onEditAttribute(index, skip_check) {
        SelectedAttrIndex = index;
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbAttrName"), "onEditAttribute(" + index + ", true)")) return false;
        initAttributes();
        $("#tdName" + index).html("<input type='text' class='input' style='width:" + $("#tdName" + index).width()+ "; vertical-align: middle;' id='tbAttrName' value='" + replaceString("'", "&#39;", attributes_data[index][aidx_name]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdEditAction" + index).html("<a href='' onclick='EditAttribute(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbAttrName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        $("#trNew").html("").hide();
        setTimeout(function () { document.getElementById('tbAttrName').focus(); }, 50);
        on_hit_enter = "EditAttribute(" + index + ");";
    }

    <%--function onEditAttributeValue(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbCatName"), "onEditAttributeValue(" + index + ", true)")) return false;        
        $("#tdCatName" + index).html("<input type='text' class='input' style='width:" + $("#tdCatName" + index).width()+ "' id='tbCatName' value='" + replaceString("'", "&#39;", attributes_data[SelectedAttrIndex][IDX_ATTR_ENUM_ITEMS][index][aidx_name]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdCatActions" + index).html("<a href='' onclick='EditAttributeValue(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbCatName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        $("#trCatNew").html("").hide();
        setTimeout(function () { document.getElementById('tbCatName').focus(); }, 50);
        on_hit_enter_cat = "EditAttributeValue(" + index + ");";
    }--%>

    function onEditAttributeValues(index, attr_type) {
        SelectedAttrIndex = index;
        var attr = GetSelectedAttr();

        initAttributes();
            
        //if (!(skip_check) && !checkUnsavedData(document.getElementById("tbAttrName"), "onEditAttribute(" + index + ", true)")) return false;
        if (attr_type == avtBoolean) {
            $("#tdValues"   + index).html("");
            $("#tdValues"   + index).html("<select id='cbDefValue' style='width:80px; z-index: 1000;'><option value='1' " + (attr[aidx_def_val] == "1" ? " selected='selected' " : " ") + "><%=ResString("lblYes") %></option><option value='0' " + (attr[aidx_def_val] == "0" ? " selected='selected' " : " ") + "><%=ResString("lblNo") %></option></select>");
        } else {
            $("#tdValues"   + index).html("");
            $("#tdValues"   + index).html("<input type='text' class='input' style='width:" + $("#tdValues" + index).width()+ "' id='tbDefValue' value='" + htmlEscape(attr[aidx_def_val]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        }

        $("#tdEditValues" + index).html("<a href='' onclick='EditDefaultValue(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbAttrName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        $("#trNew").html("").hide();
        //if (attr_type == avtBoolean) { setTimeout(function () { document.getElementById('cbDefValue').focus(); }, 300) } else { setTimeout(function () { document.getElementById('tbDefValue').focus(); }, 150) };
        on_hit_enter = "EditDefaultValue(" + index + ");";
    }

    function EditDefaultValue(index) {
        SelectedItemIndex = index;
        var attr = GetSelectedAttr();
        
        var n = document.getElementById("tbDefValue");
        if (attr[aidx_type] == avtBoolean) n = document.getElementById("cbDefValue");

        if ((n) && (attr)) {
            var def_val = n.value.trim();                        
            if ((index >= 0) && (index < attributes_data.length)) {
                switch (attr[aidx_type]) {
                    case avtDouble:
                        if (validFloat(def_val)) {def_val = str2double(def_val); } else { def_val=""; }
                        break;
                    case avtLong:
                        if (validInteger(def_val)) { def_val = str2int(def_val); } else { def_val=""; }
                        break;
                }
                attr[aidx_def_val] = htmlEscape(def_val);
                reopen_editor = true;
                sendCommand("action=set_default_value&def_val=" + encodeURIComponent(def_val) + "&clmn=" + index);
            }
        }
    }

    /* Add-remove-rename columns */
    function EditAttribute(index) {
        var n = document.getElementById("tbAttrName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgEmptyCCName") %>", "setTimeout(function () { document.getElementById('tbAttrName').focus(); }, 150);", "undefined", "<%=ResString("lblError") %>");
            } else {
                if ((index >= 0) && (index < attributes_data.length)) {
                    attributes_data[index][aidx_name] = htmlEscape(n.value);
                    sendCommand("action=rename_column&name=" + n.value + "&clmn=" + index);
                }
            }
        }
    }

    function AddAttribute() {
        on_hit_enter = "";
        var n = document.getElementById("tbAttrName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgCreateEmptyCCName") %>", "setTimeout(function () { document.getElementById('tbAttrName').focus(); }, 150);", "undefined", "<%=ResString("lblError") %>");
            } else {
                var t = document.getElementById("tbType");
                var def_val = "";
                if ((t)) {
                    switch (t.value*1) {
                        case avtEnumeration:
                        case avtEnumerationMulti:
                            break;
                        case avtString:
                            var dv = document.getElementById("tbDefaultTextValue");
                            if ((dv)) def_val = dv.value.trim();
                            break;
                        case avtLong:
                            var dv = document.getElementById("tbDefaultTextValue");                            
                            if ((dv)) { def_val = dv.value.trim(); if (validInteger(def_val)) { def_val = str2int(def_val); } else { def_val=""; }}
                            break;
                        case avtDouble:
                            var dv = document.getElementById("tbDefaultTextValue");                            
                            if ((dv)) { def_val = dv.value.trim(); if (validFloat(def_val)) {def_val = str2double(def_val); } else { def_val=""; }}
                            break;
                        case avtBoolean:
                            var dv = document.getElementById("tbDefaultBoolValue");                            
                            if ((dv)) def_val = dv.value.trim();                            
                            break;
                    }
                    sendCommand('action=add_column&name='+encodeURIComponent(n.value)+'&type='+t.value+'&def_val='+encodeURIComponent(def_val));
                }
            }
        }
    }

    function DeleteAttribute(idx) {
        SelectedAttrIndex = idx;
        dxConfirm("<%=ResString("msgSureDeleteCC") %>", "sendCommand(\"action=del_column&clmn=" + idx + "\");", ";");
    }

    /* Add-remove-rename items */
    <%--function EditAttributeValue(index) {
        SelectedItemIndex = index;
        var n = document.getElementById("tbCatName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgEmptyCategoryName") %>", "setTimeout(\"document.getElementById('tbCatName').focus();\", 150);", "undefined", "<%=ResString("lblError") %>");
            } else {
                var attr = GetSelectedAttr();
                if ((index >= 0) && (index < attr[IDX_ATTR_ENUM_ITEMS].length)) {
                    var idx = attr[IDX_ATTR_ENUM_ITEMS][index][aidx_id];
                    attr[IDX_ATTR_ENUM_ITEMS][index][aidx_name] = htmlEscape(n.value);
                    sendCommand("action=rename_item&name=" + n.value + "&item=" + index + '&clmn=' + SelectedAttrIndex);
                }
            }
        }
    }    --%>
    /* end Participant Attributes */

    /* Filter Results By Participant Attributes */
    function filterUsersDialog() {
        $("#divUserAttributes").dxPopup({
            height: function() {
                return window.innerHeight * 0.8;
            },
            onContentReady: null,
            onInitialized: null,
            onShowing: null,
            onShown: function() {
                $("#divUsersFilters").html("");
                initUsersFiltersGrid();
                initUsersFilteredTable();
                on_hit_enter = ";";
                //var h = $(e.element).innerHeight();
                //$("#divUsersFilters").height(h > 95 ? h - 95 : 95);
            },
            resizeEnabled: true,
            shading: true,
            shadingColor: "",
            showCloseButton: false,
            showTitle: true,
            title: "<%=JS_SafeString("Filter by participant attributes and create dynamic groups") %>",
            visible: true,
            width: 800,
            onContentReady: function (e) {
                //$("#divUserAttributes").find(".dx-popup-content").css("overflow", "auto");
                $(".dx-popup-content").css("overflow", "auto");
            },
            toolbarItems: [
                {
                    toolbar: 'bottom',
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: true,
                    options: {
                        icon: "fas fa-plus-circle",
                        text: "Create Dynamic Group",
                        disabled: false,
                        onClick: function () {
                            onPopupCreateGroup();
                        }
                    }
                },
                {
                    toolbar: 'bottom',
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: true,
                    options: {
                        icon: "fas fa-ban",
                        text: "Cancel",
                        disabled: false,
                        onClick: function () {
                            onPopupClose();
                        }
                    }
                }
            ]
        });
    }

    function onPopupCreateGroup() {
        onCreateDynamicResGroup();        
        onPopupClose();      
    }
    
    function onPopupClose() {
        $("#divUserAttributes").dxPopup("instance").hide();
        //$("#divUserAttributes").dxPopup("dispose");
    }

    function onUsersFiltersAddFilerRow(d, i) {
        var tbl = document.getElementById("tblUsersFilters");
        if ((tbl)) {
            if (i>flt_max_id) flt_max_id = i;

            var td = document.getElementById("flt_tr_" + i);
            if (!(td))
            {
                var r = tbl.rows.length;
                td = tbl.insertRow(r);
                td.id = "flt_tr_" + i;
                td.style.verticalAlign = "middle";
                td.style.textAlign  = "left";
                td.className = "text";
                td.insertCell(0);
                td.insertCell(1);
                td.insertCell(2);
            }

            var attr = null;
            //var checkboxes_rules = [];
                
            //var sChk = "<input type='checkbox' id='flt_chk_" + i + "' onclick='onUsersFiltersIsAnyFilterChecked(); '" + (d[fidx_chk] == "1" ? " checked" : "") + ">";
            //var sChk = "<div id='flt_chk_" + i + "'></div>";
            //checkboxes_rules.push({"id": "flt_chk_" + i, "checked" : d[fidx_chk] == "1"});

            <%--var sAttribs = "";            
            for (var j = 0; j < attributes_data.length; j++) {
                var act = (d[fidx_id] == attributes_data[j][aidx_id]);
                sAttribs += "<option value='" + attributes_data[j][aidx_id] + "'" + (act ? " selected" : "") + ">" + htmlEscape(attributes_data[j][aidx_name]) + "</option>";
                if (act) attr = attributes_data[j];
            }
            // add "inconsistency" item
            if (d[fidx_id] == DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID) {
                attr = [DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID, htmlEscape("<%=ResString("optDynamicGroupInconsistencyAttribute")%>"), avtDouble, [],[]];
            }
            sAttribs += "<option value='" + DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID + "'" + (d[fidx_id] == DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID ? " selected" : "") + ">" + htmlEscape("<%=ResString("optDynamicGroupInconsistencyAttribute")%>") + "</option>";
            
            sAttribs = "<select id='flt_attr_" + i + "' style='width:24ex' onChange='onUsersFiltersChangeFilterAttr(" + i + ", this.value);'>" + sAttribs + "</select>";--%>

            var sAttribsValues = [];
            for (var j = 0; j < attributes_data.length; j++) {
                var act = (d[fidx_id] == attributes_data[j][aidx_guid]);
                sAttribsValues.push({"ID" : attributes_data[j][aidx_guid], "text" : htmlEscape(attributes_data[j][aidx_name])});
                if (act) attr = attributes_data[j];
            }
            <% If ShowDraftPages Then %>
            // add "inconsistency" item
            if (d[fidx_id] == DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID) {
                attr = [DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID, htmlEscape("<%=ResString("optDynamicGroupInconsistencyAttribute")%>"), avtDouble, [],[]];
            }
            sAttribsValues.push({"ID" : DYNAMIC_ATTRIBUTE_GROUP_INCONSISTENCY_ID, "text" : htmlEscape("<%=ResString("optDynamicGroupInconsistencyAttribute")%>")});
            <% End If %>
            
            sAttribs = "<div id='flt_attr_" + i + "' style='display: inline-block;'></div>";

            //var sBooleanOpersValues = [];
            //sBooleanOpersValues.push({"ID" : true, "text" : resString("lblYes")});
            //sBooleanOpersValues.push({"ID" : false, "text" : resString("lblNo")});

            sOpers = "";
            var sOpersValues = [];
            var sVal = "";

            if ((attr)) {
                var o = oper_available[attr[aidx_type]];
                if (o) {
                    for (j = 0; j < o.length; j++) {
                        //sOpers += "<option value='" + o[j] + "'" + (d[fidx_oper] == o[j] ? " selected" : "") + ">" + oper_list[o[j]] + "</option>";
                        sOpersValues.push({"ID" : o[j], "text" : oper_list[o[j]]});
                    }
                }
                //sOpers = "<select id='flt_oper_" + i + "' style='width:15ex'>" + sOpers + "</select>";
                sOpers = "<div id='flt_oper_" + i + "' style='display: inline-block;'></div>";

                //if ((attr[aidx_type] == avtEnumeration) || (attr[aidx_type] == avtEnumerationMulti)) //approach 1 - select multiple
                //{
                //    var v = attr[aidx_vals];
                //    var vals = [d[fidx_val]];
                //    if (attr[aidx_type] == avtEnumerationMulti) vals = d[fidx_val].split(";");                    
                    
                //    for (j = 0; j < v.length; j++) {
                //        var is_selected = false;
                //        for (k=0; k<vals.length; k++) {
                //            if (vals[k] == v[j][0]) is_selected = true;
                //        }
                //        sVal += "<option value='" + v[j][0] + "'" + (is_selected ? " selected" : "") + ">" + htmlEscape(v[j][1]) + "</option>";
                //    }
                //    var multi = "";
                //    if (attr[aidx_type] == avtEnumerationMulti) multi = "multiple='multiple'";
                //    sVal = "<select " + multi + " id='flt_val_" + i + "' style='width:24ex; margin-top:3px;'>" + sVal + "</select>";

                //    //approach 2 - check list box
                //    if (attr[aidx_type] == avtEnumerationMulti) {
                //        sVal="";
                //        for (j = 0; j < v.length; j++) {
                //            var is_selected = false;
                //            for (k = 0; k < vals.length; k++) {
                //                if (vals[k] == v[j][0]) is_selected = true;                            
                //            }
                //            sVal += "<li style='margin:0; padding:0;' title='" + htmlEscape(v[j][1]) +"'><label for='chk" + j + "_" + k + "'><input type='checkbox' id='chk" + j + "_" + k + "' value='" + v[j][0] + "'" + (is_selected ? " checked" : "") + ">" + htmlEscape(v[j][1]) + "</label></li>";
                //        }
                //        sVal = "<ul id='flt_ul_" + i + "' style='height:12ex; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'>" + sVal + "</ul>";
                //    }
                //} else {
                    if (attr[aidx_type] != avtBoolean) {
                        sVal = "<div id='flt_val_" + i + "' style='display: inline-block;'></div>";
                    }
                //}
            }

            //td.cells[0].innerHTML = "<nobr>" + sChk + sAttribs + "&nbsp;" + sOpers + "</nobr>";
            td.cells[0].innerHTML = "<a href='' onclick='onUsersFiltersDeleteRule(" + i + "); return false;'><i class='fas fa-times' style='color: LightCoral;'></i></a>";
            td.cells[1].innerHTML = "<nobr>" + sAttribs + "&nbsp;" + sOpers + "</nobr>";
            td.cells[2].innerHTML = sVal;
            //td.cells[2].innerHTML = "<input type='button' class='button' style='width:22px;height:22px;' id='flt_del_" + i + "' value='X' onclick='onUsersFiltersDeleteRule(" + i + "); onUsersFiltersIsAnyFilterChecked(); return false;'/>";

            // init dx checkboxes
            //for (var i = 0; i < checkboxes_rules.length; i++) {
            //    var c = checkboxes_rules[i];
            //    $("#" + c["id"]).dxCheckBox({
            //        text: '',
            //        value: c["checked"],
            //        onValueChanged: function(e){
            //            onUsersFiltersIsAnyFilterChecked();
            //        }
            //    });
            //}

            //init attributes dropdown
            $("#flt_attr_" + i).dxSelectBox({
                dataSource: sAttribsValues,
                displayExpr: "text",
                valueExpr: "ID",
                width: "200px",
                searchEnabled: false,
                value: d[fidx_id],
                elementAttr: {"data-idx": i},
                onValueChanged: function (e) {                    
                    onUsersFiltersChangeFilterAttr(e.element.attr("data-idx"), e.value);
                }
            });

            //init opers dropdown
            $("#flt_oper_" + i).dxSelectBox({
                dataSource: sOpersValues,
                displayExpr: "text",
                valueExpr: "ID",
                width: "200px",
                searchEnabled: false,
                value: d[fidx_oper],
                elementAttr: {"data-idx": i}
            });

            //init vals text box
            if ((attr) && (attr[aidx_type] == avtString || attr[aidx_type] == avtLong || attr[aidx_type] == avtDouble)) {
                $("#flt_val_" + i).dxTextBox({
                    width: "200px",
                    value: htmlEscape(d[fidx_val]),
                    elementAttr: {"data-idx": i}
                });
            }
            
            //init Boolean dropdown
            //if (attr[aidx_type] == avtBoolean) {
            //    $("#flt_val_" + i).dxSelectBox({
            //        dataSource: sBooleanOpersValues,
            //        displayExpr: "text",
            //        valueExpr: "ID",
            //        width: "90px",
            //        searchEnabled: false,
            //        value: d[fidx_oper],
            //        elementAttr: {"data-idx": i}
            //    });
            //}
        }
    }

    function onUsersFiltersChangeFilterAttr(id, val) {
        //var c = $("#" + "flt_chk_" + id).dxCheckBox("instance"); //document.getElementById("flt_chk_" + id);
        //var o = document.getElementById("flt_oper_" + id);
        var d = [1, val, $("#flt_oper_" + id).dxSelectBox("instance").option("value"), ''];
        onUsersFiltersAddFilerRow(d, id);
    }

    function onUsersFiltersAddNewRule() {
        if ((attributes_data) && attributes_data.length > 0) {
            var d = [1, attributes_data[0][aidx_guid], 1, ''];
            onUsersFiltersAddFilerRow(d, flt_max_id+1);
            //onUsersFiltersCheckDeleteRuleBtn();
            //var a = document.getElementById("flt_attr_" + flt_max_id);
            //if ((a)) a.focus();
        }
    }

    function onUsersFiltersDeleteRule(id) {
        //dxConfirm(resString("msgAreYouSure"), "UsersFiltersDeleteRule(" + id + ");");
        UsersFiltersDeleteRule(id);
    }

    function UsersFiltersDeleteRule(i) {
        if ($("#flt_attr_" + i).data("dxSelectBox")) $("#flt_attr_" + i).dxSelectBox("instance").dispose();
        if ($("#flt_oper_" + i).data("dxSelectBox")) $("#flt_oper_" + i).dxSelectBox("instance").dispose();      
        if (($("#flt_val_" + i).data("dxTextBox")))  $("#flt_val_" + i).dxTextBox("instance").dispose();
        
        var td = document.getElementById("flt_tr_" + i);
        if ((td)) td.parentNode.removeChild(td);
    }

    function initUsersFilteredTable() {
        sendCommand("action=get_filtered_users_data&filter=" + encodeURIComponent(getUsersFiltersString()) + "&combination=" + $("#cbUsersFiltersCombination").dxSelectBox("instance").option("value"));
    }

    var table_filter_users = null;
    var filtered_users_columns = [];
    var filtered_users_data = [];

    function drawUsersFilteredTable() {        
        //$('#tableFilteredUsers').empty();
        table_filter_users = (($("#tableFilteredUsers").data("dxDataGrid"))) ? $("#tableFilteredUsers").dxDataGrid("instance") : null;
        if (table_filter_users !== null) { 
            table_filter_users.dispose();
            //$("#tableFilteredUsers").dxDataGrid("refresh");
        }
        var columns = [];

        //init columns headers                
        for (var i = 0; i < filtered_users_columns.length; i++) {            
            columns.push({ "caption" : filtered_users_columns[i][0], "alignment" : filtered_users_columns[i][1], "allowSorting" : filtered_users_columns[i][2], "allowSearch" : filtered_users_columns[i][3], "dataField" : filtered_users_columns[i][4] });
        }

        table_filter_users = $("#tableFilteredUsers").dxDataGrid({
            dataSource: filtered_users_data,
            columns: columns,
            //height: 400,
            searchPanel: {
                visible: true,
                text: "",
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "FltUsrsDataGrid"
            },
            noDataText: '<% =GetEmptyMessage()%>'
        });
    }

    function initUsersFiltersGrid() {
        var btnAdd = document.getElementById("btnAddNewRule");
        if ((btnAdd)) {
            btnAdd.disabled = !(attributes_data) || attributes_data.length == 0;
        }
        var d = document.getElementById("divUsersFilters");
        if ((d)) d.innerHTML = "<table id='tblUsersFilters' border=0 cellspacing=4 cellpadding=0></table>";
        if ((attr_user_flt) && (attributes_data)) {
            for (var i = 0; i < attr_user_flt.length; i++) {
                var d = attr_user_flt[i];
                onUsersFiltersAddFilerRow(d, i);
            }
        }
        //onUsersFiltersCheckDeleteRuleBtn();
        //onUsersFiltersIsAnyFilterChecked();

        $("#cbUsersFiltersCombination").dxSelectBox({
            dataSource: [ {ID: 0, text: "AND"}, {ID: 1, text: "OR"} ],
            displayExpr: "text",
            valueExpr: "ID",
            width: "90px",
            searchEnabled: false,
            value: <%=CInt(FilterCombination)%>
        });
    }

    function onUsersFiltersApplyFilter(do_reset) {
        var sData = (do_reset ? " " : getUsersFiltersString());
        //sendCommand("action=filter_by_user_attr&filter=" + encodeURIComponent(sData) + "&combination=" + $("#cbUsersFiltersCombination").dxSelectBox("instance").option("value"));
        initUsersFilteredTable();
    }

    function onUsersFiltersReset() {
        dxConfirm(resString("msgResetFilter"), "DoUsersFiltersReset();");
    }

    function DoUsersFiltersReset() {
        for (var i = 0; i <= flt_max_id; i++) {
            onUsersFiltersDeleteRule(i);
            //var c = $("#" + "flt_chk_" + id).dxCheckBox("instance"); //document.getElementById("flt_chk_" + i);
            //if ((c)) c.option("value", false);
        }
        flt_max_id = 0;
        onUsersFiltersApplyFilter(true);
    }

    function onCreateDynamicResGroup() {
        var sDlgGroupName = "<span style='margin-left:15px;'><% = JS_SafeString(ResString("lblGroupName")) %>:&nbsp;</span><input id='tbDynResGroupName' type='text' class='text' style='text-align:left; width: 220px;' value='' />";
        var groupNameDialog = DevExpress.ui.dialog.custom({
            title: "<% = JS_SafeString(ResString("titleCreateGroup")) %>",
            message: sDlgGroupName,
            buttons: [{ text: "<% =JS_SafeString(ResString("btnOK"))%>", onClick : function () { return true; }}, {text: "<% =JS_SafeString(ResString("btnCancel")) %>", onClick : function () { return false; }}]
        });
        groupNameDialog.show().done(function(dialogResult){
            if (dialogResult) onSaveDynResGroup($('#tbDynResGroupName').val());
        });
        setTimeout(function () { $("#tbDynResGroupName").focus(); }, 500);
    }

    function onSaveDynResGroup(value) {        
        if (typeof value != 'undefined' && value.trim() != "") {
            value = value.trim();
            sendCommand("action=create_dyn_res_group&name=" + value + "&filter=" + encodeURIComponent(getUsersFiltersString()) + "&combination=" + $("#cbUsersFiltersCombination").dxSelectBox("instance").option("value"));
            //dlg_filter_users.dialog("close");
            onPopupClose();
        }
    }

    //function onUsersFiltersIsAnyFilterChecked() {
    //    var retVal = false;
    //    for (var i = 0; i <= flt_max_id; i++) {            
    //        var c = $("#" + "flt_chk_" + i).dxCheckBox("instance"); //document.getElementById("flt_chk_" + i);
    //        if ((c) && (c.option("value"))) {
    //            retVal = true;
    //        }
    //    }    
                
    //    var btn = document.getElementById("btnUsersFiltersApplyFilter");    
    //    if ((btn)) { btn.disabled = !retVal; }   
    //}

    function getUsersFiltersString() {
        attr_user_flt = [];
        var sData = "";
        for (var i = 0; i <= flt_max_id; i++) {
            //var c = $("#" + "flt_chk_" + i).dxCheckBox("instance"); //document.getElementById("flt_chk_" + i);
            //var a = document.getElementById("flt_attr_" + i);
            //var o = document.getElementById("flt_oper_" + i);
            //var v = document.getElementById("flt_val_" + i);
            var a = $("#flt_attr_" + i).dxSelectBox("instance");            
            var o = $("#flt_oper_" + i).dxSelectBox("instance");
            var el_v = $("#flt_val_" + i);
            var v;
            if (el_v.data("dxTextBox")) v = el_v.dxTextBox("instance");
            if (el_v.data("dxSelectBox")) v = el_v.dxSelectBox("instance");
            
            var selValue = ""            

            if (!(v)) {
            //if (v.multiple) {
                selValue = "";
                //Approach 1: <select multiple>
                //for (var k=0; k < v.options.length; k++)
                //{
                //    if (v.options[k].selected) 
                //    {
                //        if (selValue.length > 0) selValue += ";"
                //        selValue += v.options[k].value;
                //    }
                //}
                
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
            } else { selValue = v.option("value"); }
            
            if ((a) && (o))
            {
                attr_user_flt.push([1, a.option("value"), o.option("value"), selValue]);
                sData += (sData == "" ? "" : "\n") + 1 + "<% =Flt_Separator %>" + a.option("value")  + "<% =Flt_Separator %>" + o.option("value")  + "<% =Flt_Separator %>" + selValue;
            }
        }
        return sData;
    }
    /* end Filter Results By Participant Attributes */

    /* Add attributes using survey question */
    var questions = <% = Api.GetProjectSurveysQuestionsList() %>;
    var selectedQuestion = null;

    function addAttributeUsingSurvey() {
        selectedQuestion = questions.length > 0 ? questions[0] : null;

        $("#divSurveyAttributes").dxPopup({
            height: function() {
                return window.innerHeight * 0.8;
            },
            onContentReady: null,
            onInitialized: null,
            onShowing: null,
            onShown: function(e) {
                var h = $(e.element).innerHeight();
                $("#divQuestionsList").height(h > 95 ? h - 95 : 95);
            //    $("#tblAttributes").css("max-height", (h > 95 ? h - 95 : 95) + "px");
            },
            resizeEnabled: true,
            shading: true,
            shadingColor: "",
            showCloseButton: false,
            showTitle: true,
            title: "<%=JS_SafeString("Add attribute using survey question") %>",
            visible: true,
            width: 800,
            onContentReady: function (e) {
                $(".dx-popup-content").css({"overflow" : "auto", "overflow-x" : "hidden"});
            },
            toolbarItems: [
                {
                    toolbar: 'bottom',
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: true,
                    options: {
                        //icon: "fas fa-plus-circle",
                        text: "OK",
                        disabled: false,
                        onClick: function () {
                            onSurveyAttrPopupOk();
                        }
                    }
                },
                {
                    toolbar: 'bottom',
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    visible: true,
                    options: {
                        //icon: "fas fa-ban",
                        text: "Cancel",
                        disabled: false,
                        onClick: function () {
                            onSurveyAttrPopupClose();
                        }
                    }
                }
            ]
        });

        if ($("#divQuestionsList").data("dxList")) {
            $("#divQuestionsList").dxList("instance").dispose();
        }

        $("#divQuestionsList").dxList({
            dataSource: questions,
            focusStateEnabled: true,
            hoverStateEnabled: true,                
            keyExpr: "guid",
            displayExpr: "text",
            itemTemplate: function(data, _, element) {
                element.append(
                    $("<span id='ul" + data.id + "' style='" + (data.linked_attr_id == "" ? "" : "text-decoration:line-through;") + "'>").text(data.text)
                )
            },
            <% If Not IsReadOnly Then %>
            noDataText: "<% = ResString("msgNoSurveyQuestions") %>",
            <% End If %>
            onSelectionChanged: function (e) {
                $("#tbAttributeName").val(e.addedItems.length > 0 ? e.addedItems[0].text : "");
                
                selectedQuestion = e.addedItems.length > 0 ? e.addedItems[0] : null;

                initAnswersList(selectedQuestion);
            },
            searchEnabled: false,
            selectionMode: "single",
            selectedItemKeys: questions.length > 0 ? questions[0].guid : null
        });

        $("#tbAttributeName").val(questions.length > 0 ? questions[0].text : "");
        
        initAnswersList(selectedQuestion);

        cbCreateGroupsChange($("#cbCreateGroups").prop("checked"));
    }

    function openSurveySettings() {
        navOpenPage(<% = _PGID_PROJECT_OPTION_SURVEY %>);
    }

    function initAnswersList(selectedQuestion) {        
        $("#tbAttributeName").prop("disabled", selectedQuestion == null || selectedQuestion.linked_attr_id !== "");
        $("#cbCreateGroups").prop("disabled", selectedQuestion == null || selectedQuestion.linked_attr_id !== "");

        if ($("#divAnswersList").data("dxList")) {
            $("#divAnswersList").dxList("instance").dispose();
        }
        $("#divAnswersList").dxList({
            dataSource: (selectedQuestion) ? selectedQuestion.unique_answers : [],
            disabled: selectedQuestion == null || selectedQuestion.linked_attr_id !== "",
            focusStateEnabled: false,
            hoverStateEnabled: true,                
            itemTemplate: function(data, _, element) {
                element.append(
                    $("<span>").text(data)
                )
            },
            onSelectionChanged: function (e) {
                $("#lblDynGroupsCount").text(e.component.option("selectedItems").length);
            },
            showSelectionControls: true,
            searchEnabled: false,
            selectionMode: "all"//,
            //selectedItemKeys: questions.length > 0 ? questions[0].guid : null
        });
        var lst = $("#divAnswersList").dxList("instance");
        lst.selectAll();
        $("#lblDynGroupsCount").text(selectedQuestion == null ? "" : selectedQuestion.unique_answers.length);
        $("#lblDynGroupsCount").prop("disabled", selectedQuestion == null);
    }
    
    function onSurveyAttrPopupClose() {
        $("#divSurveyAttributes").dxPopup("instance").hide();
    }

    function onSurveyAttrPopupOk() {
        if (selectedQuestion != null && selectedQuestion.linked_attr_id == "" && $("#tbAttributeName").val().trim() != "") {
            var lst = $("#divAnswersList").dxList("instance");
            var sel_groups = ""
            var chk_groups = $("#cbCreateGroups").prop("checked");
            if (chk_groups) {
                var sel_items = lst.option("selectedItems");
                for (var i = 0; i < sel_items.length; i++) {
                    sel_groups += (sel_groups == "" ? "" : "\t") + sel_items[i];
                }
            }

            sendCommand("action=create_attr_from_survey&question_id=" + selectedQuestion.guid + "&name=" + $("#tbAttributeName").val().trim() + "&is_welcome_survey=" + !selectedQuestion.is_thankyou + "&create_groups=" + chk_groups + "&groups=" + sel_groups);
        }
        $("#divSurveyAttributes").dxPopup("instance").hide();
    }

    function cbCreateGroupsChange(chk) {
        $("#divAnswersList").dxList("instance").option("disabled", !chk);
    }

    /* end Add attributes using survey question */

</script>

</asp:Content>