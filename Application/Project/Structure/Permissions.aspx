<%@ Page Language="VB" Inherits="Permission4AltsPage" title="Participants roles for Alternatives" Codebehind="Permissions.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/ec.dg.js"></script>
    <script language="javascript" type="text/javascript">
        
        var toolbar = null;
        var roles_for = <%=PermissionsFor%>;
        var checkedUsers = <%=GetCheckedUsersList(0)%>;
        var checkedGroups = <%=GetCheckedUsersList(1)%>;
        var copyFromUserID = null;
        var readonly = <% =Bool2JS(App.IsActiveProjectStructureReadOnly)%>;
        
        function initToolbar() {
            $("#toolbar").dxToolbar({
                items: [
                    {
                        location: 'before',
                        locateInMenu: 'auto',
                        visible: !readonly,
                        widget: 'dxButton',
                        options: {
                            text: "Copy Roles",
                            icon: "fa fa-copy",
                            elementAttr: { id: 'btnCopyRoles' },
                            onClick: function () {
                                onCopyRoles();
                            }
                        }
                    },
                    {
                        location: 'before',
                        locateInMenu: 'auto',
                        visible: !readonly,
                        widget: 'dxButton',
                        options: {
                            text: "Paste Roles",
                            icon: "fa fa-paste",
                            elementAttr: { id: 'btnPasteRoles' },
                            disabled: true,
                            onClick: function () {
                                onPasteRoles();
                            }
                        }
                    },
                    {
                        location: 'before',
                        locateInMenu: 'auto',
                        widget: 'dxButton',
                        visible: !readonly,
                        options: {
                            text: "Drop all",
                            icon: "<% =JS_SafeString(ImagePath) %>" + "rolesDrop.png",
                            elementAttr: { id: 'btnDropAll' },
                            onClick: function () {
                                //0 - restricted
                                //1 - allowed
                                //2 - "undefined"
                                $('#dataGridTable').datagrid('setAllRoles', 2);
                                var rolesMode = $('#dataGridTable').datagrid('option', 'rolesMode');
                                sendCommand('action=setallroles&val=2&rm='+ rolesMode +'&chkusers=' + (roles_for == 0 ?  checkedUsers : checkedGroups), true);
                            }
                        }
                    },
                    {
                        location: 'before',
                        locateInMenu: 'auto',
                        visible: !readonly,
                        widget: 'dxButton',
                        options: {
                            text: "Allow all",
                            icon: "<% =JS_SafeString(ImagePath) %>" + "rolesAllow.png",
                            elementAttr: { id: 'btnAllowAll' },
                            onClick: function () {
                                $('#dataGridTable').datagrid('setAllRoles', 1);
                                var rolesMode = $('#dataGridTable').datagrid('option', 'rolesMode');
                                sendCommand('action=setallroles&val=1&rm='+ rolesMode +'&chkusers=' + (roles_for == 0 ?  checkedUsers : checkedGroups), true);                                
                            }
                        }
                    },
                    {
                        location: 'before',
                        locateInMenu: 'auto',
                        visible: !readonly,
                        widget: 'dxButton',
                        options: {
                            text: "Restrict All",
                            icon: "<% =JS_SafeString(ImagePath) %>" + "rolesRestrict.png",
                            elementAttr: { id: 'btnRestrictAll' },
                            onClick: function () {
                                $('#dataGridTable').datagrid('setAllRoles', 0);
                                var rolesMode = $('#dataGridTable').datagrid('option', 'rolesMode');
                                sendCommand('action=setallroles&val=0&rm='+ rolesMode +'&chkusers=' + (roles_for == 0 ?  checkedUsers : checkedGroups), true);
                            }
                        }
                    },
                    {
                        location: 'before',
                        locateInMenu: 'auto',
                        widget: 'dxButton',
                        options: {
                            text: "Select Columns",
                            icon: "fas fa-columns",
                            elementAttr: { id: 'btnSelectColumns' },
                            onClick: function () {
                                onSelectColumnsClick();
                            }
                        }
                    },    
                    {
                    location: 'before',
                    widget: 'dxSelectBox',
                    locateInMenu: 'auto',
                    visible: !readonly,
                    options: {
                        width: 140,
                        items: [{id:0,text:"Edit Mode"},{id:1,text:"View Mode"}],
                        valueExpr: "id",
                        displayExpr: "text",
                        value: (readonly ? 1 : 0),
                        onValueChanged: function(args) {
                            onViewMode(args.value);
                        }
                    }
                },
                {
                    location: 'before',
                    widget: 'dxCheckBox',
                    locateInMenu: 'auto',
                    options: {
                        //hint: "Shows N/M for each nodes, where:<br><br>N = No. of Participants who made judgments<br>M = No. of Participants with 'Allowed' role",
                        hint: "Shows count of Participants with 'Allowed' role",
                        text: "Show statistics",
                        onValueChanged: function(args) {
                            onStatisticClick(args.value);
                        }
                    }
                },
                {
                    location: 'before',
                    widget: 'dxCheckBox',
                    locateInMenu: 'auto',
                    visible: false,
                    options: {
                        text: "Show missing roles",
                        elementAttr: { id: 'cbMissingRoles' },
                        onValueChanged: function(args) {
                            onShowMissedRoles(args.value);
                        }
                    }
                }
                ]
            });

            toolbar = $("#toolbar").dxToolbar("instance");
        }

        var continue_ajax = false;

        function syncReceived(params) {
            if ((params)) {            
                var received_data = JSON.parse(params);
                if ((received_data)) {
                    if (received_data.action == 'reload'){
                        document.location.href = '<% =PageURL(CurrentPageID) %>?<% =GetTempThemeURI(true) %>';
                    };

                    if (received_data.action == 'select_columns') {
                        // TODO: refresh the widget - show attribute columns

                        // $('#dataGridTable').datagrid('redraw');  // - ?
                    }
                    if (received_data.action == 'setcellrole') {
                        var colID = received_data.colid;
                        var rowID = received_data.rowid;
                        var cellstat = received_data.cellstat;
                        $('#dataGridTable').datagrid('setCellRoleStat', colID, rowID, cellstat);
                        $('#dataGridTable').datagrid('updateRoles', true);
                        $('#dataGridTable').datagrid('redraw'); 
                        // $('#dataGridTable').datagrid('redraw');  // - ?
                    }
                    if (received_data.action == 'activate_users' || received_data.action == 'showstatistic' || received_data.action == 'setallroles' || received_data.action == 'setrolerange') { //A1362
                        // TODO: refresh the widget - users/groups were checked or unchecked
                        //var curType = received_data.tfor * 1; // 0 - for users, 1 - for groups
                        //if (curType == 0) {
                        if (received_data.action == 'activate_users' || received_data.action == 'showstatistic') {
                            if (received_data.is_for == 0) checkedUsers = received_data.checkedusers;
                            if (received_data.is_for == 1) checkedGroups = received_data.checkedusers;
                        }

                        $('#btnCopyRoles').dxButton('option','disabled', received_data.checkedusers.length > 1);
                        //if (received_data.checkedusers.length > 1) {
                        //    $('#btnCopyRoles').dxButton('option','disabled', received_data.checkedusers.length > 1);
                        //} else {
                        //    $('#btnCopyRoles').dxButton('option','disabled', false);
                        //};
                        $('#dataGridTable').datagrid('option', 'checkedUsers', received_data.checkedusers);
                        //} else {
                        //    checkedGroups = received_data.checkedusers;
                        //    $('#dataGridTable').datagrid('option', 'checkedGroups', checkedGroups);
                        //}
                        $('#dataGridTable').datagrid('option', 'columns', received_data.cols);
                        $('#dataGridTable').datagrid('option', 'rows', received_data.rows);
                        $('#dataGridTable').datagrid('updateRoles', true); 
                        $('#dataGridTable').datagrid('updateObjCheckboxState');
                        if (received_data.action == 'showstatistic') {
                            $('#dataGridTable').datagrid('option', 'showRolesStats', true);
                        };
                        $('#dataGridTable').datagrid('redraw');  
                    }

                    if (received_data.action == 'permissions_for_changed') { //A1362
                        // TODO: refresh the widget
                        var curType = received_data.value * 1; // 0 - for users, 1 - for groups
                        //if (curType == 0) {
                        $('#dataGridTable').datagrid('option', 'checkedUsers', curType == 0 ? checkedUsers : checkedGroups);
                        //} else {
                        //    $('#dataGridTable').datagrid('option', 'checkedUsers', checkedGroups);
                        //}
                        $('#dataGridTable').datagrid('option', 'isForGroups', curType == 1);
                        // $('#dataGridTable').datagrid('redraw');  // - ?
                        onSelectedParticipantsChanged(curType, 'activate_users');
                        
                        continue_ajax = true;
                    }
                    resizePage();
                }
            }        
            
            if (!continue_ajax) { hideLoadingPanel(); } else { showLoadingPanel(); }

            continue_ajax = false;
        }

        function sendCommand(params, showPleaseWait) {
            callAjax(params, syncReceived, _method_POST, !showPleaseWait);
            //if (showPleaseWait) showLoadingPanel();
            //
            //_ajax_ok_code = syncReceived;
            //_ajax_error_code = syncError;
            //_ajaxSend(params);
        }
        
        //function syncError() {
        //    hideLoadingPanel();
        //    DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
        //    $(".ui-dialog").css("z-index", 9999);
        //}

        var showAttributes = '<%=SelectedColumns(App.ActiveProject)%>';
        var attr_list = <%= GetAttributes() %>;

        function onCopyRoles() {
            var list = roles_for == 0 ?  checkedUsers : checkedGroups;
            if (list.length == 1) {
                copyFromUserID = list[0];
                $('#btnPasteRoles').dxButton('option','disabled', false);
            };
        }

        function onPasteRoles() {
            var list = roles_for == 0 ?  checkedUsers : checkedGroups;
            if ((copyFromUserID !== null) && (list.length > 0)) {
                var rolesMode = $('#dataGridTable').datagrid('option', 'rolesMode');
                sendCommand('action=pasteroles&rm='+ rolesMode +'&copyfromuserid=' + copyFromUserID + '&chkusers=' + list, true);
            };
        }

        function onSelectColumnsClick() {
            initSelectColumnsForm("Select Columns");
            dlg_select_columns.dialog("open");
            dxDialogBtnDisable(true, true);
        }

        function onStatisticClick(checkState) {
            if (checkState) {
                var curType = ($('#dataGridTable').datagrid('option', 'isForGroups') ? 1 : 0);
                $("#divTabs").dxTabPanel("option", "disabled", true);
                $("#tabs").dxTabPanel("option", "disabled", true);
                $("#divList0").dxList("option", "disabled", true);
                $("#divList1").dxList("option", "disabled", true);
                onSelectedParticipantsChanged(curType, 'showstatistic');
            }else{
                $("#divTabs").dxTabPanel("option", "disabled", false);
                $("#tabs").dxTabPanel("option", "disabled", false);
                $("#divList0").dxList("option", "disabled", false);
                $("#divList1").dxList("option", "disabled", false);
                $('#dataGridTable').datagrid('option', 'showRolesStats', false);
            }            
        }

        function onShowMissedRoles(checkState) {
            $('#dataGridTable').datagrid('option', 'showMissedRoles', checkState);
            $('#dataGridTable').datagrid('redraw');
        }

        function onViewMode(mode) {
            $('#dataGridTable').datagrid('option', 'showRolesMode', mode);
            $('#dataGridTable').datagrid('redraw');

        }

        function initSelectColumnsForm(_title) {
            cancelled = false;

            var labels = "";

            // generate list of attributes
            var attr_list_len = attr_list.length;
            for (var k = 0; k < attr_list_len; k++) {
                var checked = showAttributes.indexOf(attr_list[k].guid) != -1;
                labels += "<label><input type='checkbox' class='select_clmn_cb' value='" + attr_list[k].guid + "' " + (checked ? " checked='checked' " : " " ) + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + attr_list[k].name + "</label><br>";
            }    

            $("#divSelectColumns").html(labels);

            dlg_select_columns = $("#selectColumnsForm").dialog({
                autoOpen: false,
                modal: true,
                width: 420,
                dialogClass: "no-close",
                closeOnEscape: true,
                bgiframe: true,
                title: _title,
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: {
                    Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                        dlg_select_columns.dialog( "close" );
                    }},
                    Cancel: function() {
                        cancelled = true;
                        dlg_select_columns.dialog( "close" );
                    }
                },
                open: function() {
                    $("body").css("overflow", "hidden");
                },
                close: function() {
                    $("body").css("overflow", "auto");                
                    if (!cancelled) {
                        var clmn_ids = "";
                        var cb_arr = $("input:checkbox.select_clmn_cb");
                        $.each(cb_arr, function(index, val) { var cid = val.value + ""; if (val.checked) { clmn_ids += (clmn_ids == "" ? "" : ",") + cid; } });
                        showAttributes = clmn_ids;
                        _ajaxSend('action=select_columns&column_ids=' + clmn_ids); // save the selected columns via ajax
                        options.showAttributes = showAttributes;  // update the visible columns option                        
                        $('#dataGridTable').datagrid('option', 'showAttributes', showAttributes);    // update the datagrid option
                        $('#dataGridTable').datagrid('redraw'); // redraw datagrid
                    }
                }
            });
            $(".ui-dialog").css("z-index", 9999);
        }

        function filterColumnsCustomSelect(chk) {
            $("input:checkbox.select_clmn_cb").prop('checked', chk*1 == 1);
            dxDialogBtnDisable(true, false);
        }

        //function openTab(evt, tab_id) { //A1362
        //    var i, tabcontent, tablinks;

        //    // Get all elements with class="ec_toolbar_tabcontent" and hide them
        //    tabcontent = document.getElementsByClassName("ec_toolbar_tabcontent");
        //    for (i = 0; i < tabcontent.length; i++) {
        //        tabcontent[i].style.display = "none";
        //    }

        //    // Get all elements with class="ec_toolbar_tablink" and remove the class "active"
        //    tablinks = document.getElementsByClassName("ec_toolbar_tablink");
        //    for (i = 0; i < tablinks.length; i++) {
        //        tablinks[i].className = tablinks[i].className.replace(" active", "");
        //    }

        //    // Show the current tab, and add an "active" class to the link that opened the tab
        //    document.getElementById("tabContent" + tab_id).style.display = "block";
        //    evt.currentTarget.className += " active";

        //    // ajax
        //    sendCommand("action=permissions_for_changed&value=" + tab_id, true);
        //}

        //var last_click_id = ""; //A1362

        //function onParticipantCheckboxChecked(e, cb, user_id, cb_for) { //A1362
        //    var chk = cb.checked;

        //    var cb_class = "";
        //    if (cb_for == 0) cb_class = "cb_multiselect_users";
        //    if (cb_for == 1) cb_class = "cb_multiselect_groups";
            
        //    var ids = "";

        //    // process Shift + Click
        //    if ((e.shiftKey) && e.shiftKey && (last_click_id != "") && (last_click_id != user_id)) {
        //        var start_id = last_click_id;
        //        var stop_id = user_id;
        //        var is_checking = false;
                
        //        $("input." + cb_class).each(function() {
        //            var this_id = this.getAttribute("data-id") * 1;
        //            var is_found = this_id == start_id || this_id == stop_id;
        //            if (is_found && !is_checking) {
        //                is_checking = true;
        //                this.checked = chk;
        //            } else {
        //                if (is_found && is_checking) {
        //                    is_checking = false;
        //                    this.checked = chk;
        //                }
        //            }
        //            if (is_checking) {
        //                this.checked = chk;
        //            }
        //        });            
        //    }

        //    last_click_id = user_id;

        //    $("input." + cb_class).each(function() {
        //        if (this.checked) ids += (ids == "" ? "" : ",") + this.getAttribute("data-id");
        //    });
        
        //    sendCommand('action=activate_users&ids=' + ids + '&chk=' + chk + '&for=' + cb_for);
        //}

        function onSelectedParticipantsChanged(cb_for, act) { //A1407
            if (typeof act == 'undefined') {act = 'activate_users'};
            var idsArr = [];
            
            var list = (($("#" + (cb_for == 0 ? "divList0" : "divList1")).data("dxList"))) ? $("#" + (cb_for == 0 ? "divList0" : "divList1")).dxList("instance") : null;
            if (list !== null) {
                idsArr = list.option("selectedItemKeys");    
            }         
            
            sendCommand('action=' + act + '&ids=' + idsArr.join(",") + '&for=' + cb_for, true);
        }

        var autoSelect = false;

        function initSplitters() { //A1362
            $(".split_left").dxResizable({
                handles: 'right',
                width: 170,
                minWidth: 150,
                maxWidth: 1000,
                onResizeStart: function (e) {
                    if (is_firefox) $("#dataGridTable").hide();
                },                
                onResizeEnd: function (e) {
                    $("#dataGridTable").show();
                    resizePage();
                }
            });

            $(".dx-resizable-handle").addClass("splitter_v");
        }

        var is_tpl = <% =Bool2JS(App.ActiveProject.ProjectStatus = ecProjectStatus.psTemplate) %>;
        if (is_tpl) {roles_for = 1};
        var tabItems = [{ title: "<%=ResString("lblUsers")%>", visible: !is_tpl }, { title: "<%=ResString("lblGroups")%>"}];
        var users = <%=GetCheckList(0)%>;
        var groups = <%=GetCheckList(1)%>;
        var lastSelectedKey;

        function initTabs() {
            $("#divTabs").dxTabPanel({
                height: "100%",
                items: tabItems,
                selectedIndex: (is_tpl ? 1 : <%=PermissionsFor%>),
                onSelectionChanged: function (e) {
                    roles_for = e.component.option("selectedIndex");
                    initList(roles_for);
                    if (!autoSelect) sendCommand("action=permissions_for_changed&value=" + roles_for, true);
                },
                itemTemplate: function (itemData, itemIndex, itemElement) {
                    var mainContainer = $("<div id='divList" + itemIndex + "' style='margin:1px;'>");
                    itemElement.append(mainContainer);
                }
            });

            initList(0);
            initList(1);
        }

        function initList(cb_for) {
            if (!$("#divList" + cb_for).data("dxList")) {
                var data = cb_for == 0 ? users : groups;
                var checkedData = cb_for == 0 ? checkedUsers : checkedGroups;
                
                var columns = [{ "caption" : "", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "text", "allowEditing": false}];

                $("#divList" + cb_for).dxList({
                    columns: columns,
                    dataSource: { store : new DevExpress.data.ArrayStore({
                            key: 'key',
                            data: data
                        }),
                        paginate: false 
                    },
                    keyExpr: "key",
                    displayExpr: "text",
                    showColumnHeaders: false,
                    selectionMode: "all",
                    showSelectionControls: true,
                    selectedItemKeys: checkedData,
                    onSelectionChanged: function(e) {
                        if (!autoSelect) {
                            if ((window.event) && window.event.ctrlKey) {
                                var isChecked = e.addedItems.length > 0;
                                lastSelectedKey = isChecked ? e.addedItems[0].key : e.removedItems[0].key;
                                onSelectedParticipantsChanged(cb_for);
                            } else {
                                if ((window.event) && window.event.shiftKey) {
                                    resetPageSelection();
                                    autoSelect = true;
                                    var isChecked = e.addedItems.length > 0;
                                    var nextSelectedKey = isChecked ? e.addedItems[0].key : e.removedItems[0].key;
                                    var selUsersKeys = selectAllUsersBetween(data, lastSelectedKey, nextSelectedKey, isChecked);
                                    lastSelectedKey = nextSelectedKey;
                                    var curSelUsersKeys = e.component.option("selectedItemKeys");
                                    for (var i = 0; i < selUsersKeys.length; i++) {
                                        if (isChecked) {
                                            if (curSelUsersKeys.indexOf(selUsersKeys[i]) == -1) curSelUsersKeys.push(selUsersKeys[i]);
                                        } else {
                                            var idx = curSelUsersKeys.indexOf(selUsersKeys[i]);
                                            if (idx > -1) curSelUsersKeys.splice(idx, 1);
                                        }
                                    }
                                    //e.component.option("selectedItemKeys", []);
                                    e.component.option("selectedItemKeys", curSelUsersKeys);
                                    onSelectedParticipantsChanged(cb_for);
                                    setTimeout(function() { autoSelect = false; }, 200);
                                } else {
                                    var isChecked = e.addedItems.length > 0;
                                    lastSelectedKey = isChecked ? e.addedItems[0].key : e.removedItems[0].key;
                                    if (e.addedItems.length == 1) {
                                        autoSelect = true;
                                        e.component.option("selectedItemKeys", []);
                                        e.component.option("selectedItemKeys", [e.addedItems[0].key]);
                                        setTimeout(function() { autoSelect = false; }, 200);
                                    }
                                    onSelectedParticipantsChanged(cb_for);
                                }
                            }
                        }
                        checkedData = e.component.option("selectedItemKeys");
                    },
                    searchEnabled: true,
                    searchExpr: "text",
                    useNativeScrolling: false,
                    pageLoadMode: "scrollBottom"
                });
            }
        }

        function selectAllUsersBetween(Items, User1, User2, IsChecked) {
            var retVal = [];
            if ((User1) && (User2)) {
                var user1Index = -1;
                var user2Index = -1;
                for (var i = 0; i < Items.length; i++) {
                    if (Items[i].key == User1) user1Index = i;
                    if (Items[i].key == User2) user2Index = i;
                }
                if (user1Index >= 0 && user2Index >= 0 && user1Index !== user2Index) {
                    if (user1Index > user2Index) {
                        var tmpIndex = user1Index;
                        user1Index = user2Index;
                        user2Index = tmpIndex;
                    }

                    for (var i = user1Index; i <= user2Index; i++) {
                        if (Items[i].key !== -1) retVal.push(Items[i].key);
                    }
                }
            }
            return retVal;
        }

        var options = {
                rows: <%= GetRows(GetCheckedUsersIDs(PermissionsFor), False) %>,
                columns: <%= GetColumns(GetCheckedUsersIDs(PermissionsFor), False) %>,
                rowsTitle: "<%=ParseString("%%Alternatives%%")%>",
                viewMode: 'roles',
                showAttributes: showAttributes, //A1201
                attributes: attr_list,          //A1201
                hierarchyNodes: <%= GetHierarchyColumns() %>,
                imgPath: '<%=ImagePath%>',
                contextMenu: true,
                showRolesMode: (readonly ? 1 : 0),
                hid: <%=IIf(App.isRiskEnabled, CInt(PM.ActiveHierarchy), -1)%>,
                checkedUsers: (roles_for == 0 ?  checkedUsers : checkedGroups), //A1362
                isForGroups: roles_for == 1
            };

        $(document).ready(function () {
            $("#tabs").dxTabPanel({
                items: [{id:0,title:"<% =JS_SafeString(ParseString(If(PM.IsRiskProject, If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "For %%Alternative%% Consequences", "For %%Alternative%% Vulnerabilities"), "For %%Alternatives%%"))) %>",icon:"icon ec-alts"},
                        {id:1,title:"<% =JS_SafeString(ParseString(If(PM.IsRiskProject, If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, "For %%Objective(i)%% Priorities", "For %%Objective(l)%% Likelihoods"), "For %%Objectives%%"))) %>",icon:"icon ec-hierarchy"}],
                selectedIndex: 0,
                onSelectionChanged: function(e) {
                    if (e.addedItems.length > 0) {
                        var tabID = e.addedItems[0].id;
                        if (tabID == 0) {
                            $('#dataGridTable').datagrid('option', 'rolesMode', 'alts');
                            $('#cbMissingRoles').dxCheckBox('option','disabled', false);
                        }else{
                            $('#dataGridTable').datagrid('option', 'rolesMode', 'objs');
                            $('#cbMissingRoles').dxCheckBox('option','disabled', true);
                            $('#cbMissingRoles').dxCheckBox('option','value', false);
                            onShowMissedRoles(false);
                        };
                        $('#dataGridTable').datagrid('redraw');
                    }
                }
            });

            initSplitters(); //A1362
            initTabs(); //A1710;
            resetDivContent();
            options.height = $("#divContent").height() - 40;
            options.width = $("#divContent").width() - 15; 
            $("#dataGridTable").datagrid(options);
            initToolbar();
            setTimeout(function () { resizePage(); }, 100);
        });

        resize_custom = resizePage;

        function resetDivContent (){
            $(".content_pane").height(100);
            $("#divContent").width(100);
            var h = $("#divMain").height() - $("#divToolbar").height() - 5;
            var w = $("#divMain").width() - $("#divUsers").width();
            h = (h > 400 ? h : 400);
            w = (w > 400 ? w : 400);
            $(".content_pane").height(h);
            $("#divContent").width(w);
            //$(".ec_toolbar_tabcontent").height(h - 50);
        }

        function resizePage(force_redraw, w1, h1) {
            resetDivContent();
            var w = $("#divContent").width();
            var h = $("#divContent").height();
            $('#dataGridTable').datagrid('resize', w - 16, h - 38);
            $('#dataGridTable').datagrid('redraw');
        }

        function showLegend() {
            var legendText = "<ul><li><table><tr><td><div style='background-color: grey; width: 16px; height:16px;'></div></td><td> Grey - no information on whether participant or group has a role</td></tr></table></li>"
            legendText += "<li><table><tr><td><div style='background-color: green; width: 16px; height:16px;'></div></td><td>Green - participant or group has a role</td></tr></table></li>"
            legendText += "<li><table><tr><td><div style='background-color: red; width: 16px; height:16px;'></div></td><td>Red - participant or group is not allowed a role</td></tr></table></li>"
            legendText += "<li><table><tr><td><div style='background-color: yellow; width: 16px; height:16px;'></div></td><td>Yellow - roles in the row or column vary</td></tr></table></li>"
            legendText += "<li><div><table><tr><td><div style='background-color: #ff7e80; width: 16px; height:16px; padding:2px;'><div style='background-color: green; width: 12px; height:12px;'></div></div></td><td>Different inside and outside colors - the outside color indicates the participant's role based on their group membership;</td></tr></table></div>"
            legendText += "<div>when the inside color differs from the outside color this means the participant has an override to their role based on group membership.</div>"
            legendText += "<div>Ultimately, the inside color indicates whether the participant has a role or not.</div></li>"
            legendText += "<li><table><tr><td><div style='background-color: transparent; width: 16px; height:16px;'></div></td><td>No colored box(blank) means the alternative does not have a contribution to the objective</td></tr></table></li>"
            legendText += "</ul><p>For more information, please consult the <a href='' target='_blank' onclick='onShowHelp(help_uri); return false;'>help</a></p>"
            showMessagePopup(legendText, "Legend");
        }

    </script>
    
    <div id="divMain" class="whole" style="text-align:left;vertical-align:top;overflow:hidden;">
        <div id='divToolbar'>
            <div class="table">
                <div class="tr">
                    <div class="td">
                        <div id="toolbar" class="dxToolbar"></div>
                    </div>
                </div>
 <%--               <div class="tr">
                    <td class="ec_toolbar_td_separator">
                        <button type='button' class='button' onclick='onCopyRoles();' title='Copy Roles' id="btnCopyRoles"><img src="<% =ImagePath %>copy2-16.png" width="16" height="16" />&nbsp;Copy Roles</button>
                        <button type='button' class='button' onclick='onPasteRoles();' disabled="disabled" title='Paste Roles' id="btnPasteRoles"><img src="<% =ImagePath %>paste-16.png" width="16" height="16" />&nbsp;Paste Roles</button>
                    </td>
                    <td class="ec_toolbar_td_separator">
                        <button type='button' class='button' style='width:140px;' onclick='return false;' disabled="disabled" title='Manage Groups'><img src="<% =ImagePath %>edit_group.gif" width="16" height="16" />&nbsp;Manage Groups</button>
                    </td>
                    <%If PM.Attributes.GetAlternativesAttributes(True).Count > 0 Then%>
                    <td class="ec_toolbar_td_separator">
                        <button type="button" class="button" style='width:16ex' id='hbSelectColumns' onclick="onSelectColumnsClick();"><img src="<% =ImagePath %>columns-20.png" width=16 height=16 />&nbsp;<%=ResString("lblRAOptColumns")%>&hellip;&nbsp;</button>
                    </td>
                    <%End If%>
                    <td class="ec_toolbar_td">
                        <select>
                            <option selected="selected">Edit Mode</option>
                            <option>View Mode</option>
                        </select>
                    </td>
                    <td class="ec_toolbar_td"><input type="checkbox"  onclick="onStatisticClick(this.checked);" />Roles Statistic</td>
                </div>
 --%>           </div>
        </div>
        <div id="divMainPermissions" style="white-space: nowrap;">
            <div id="divUsers" class="split_left content_pane" style="height: 100%; display: inline-block; border: 0; padding-right: 4px;">
                <div id="divTabs" class="ec_tabs" style="display:block; vertical-align: top;"></div>
                    <%--<div class="ec_toolbar_tabs" style="display:inline-block; width:200px;">
                        <a href="#" class="ec_toolbar_tablink<%=CStr(IIf(PermissionsFor = 0, " active",""))%>" onclick="openTab(event, 0)" title="<%=ResString("lblUsers")%>"><%=ResString("lblUsers")%></a>
                        <a href="#" class="ec_toolbar_tablink<%=CStr(IIf(PermissionsFor = 1, " active",""))%>" onclick="openTab(event, 1)" title="<%=ResString("lblGroups")%>"><%=ResString("lblGroups")%></a>
                    </div>
                    <div class="ec_toolbar_tabcontent" id="tabContent0" style="overflow-x: hidden; overflow-y: auto; <%=CStr(IIf(PermissionsFor = 0, "display:block;","display:none;"))%>">
                        <%=GetCheckboxList(0)%>
                    </div>                        
                    <div class="ec_toolbar_tabcontent" id="tabContent1" style="overflow-x: hidden; overflow-y: auto; <%=CStr(IIf(PermissionsFor = 1, "display:block;","display:none;"))%>">
                        <%=GetCheckboxList(1)%>
                    </div>                        
                </div>--%>
                <%--<div id="divGridResizeHandle" style="font-weight: bold; position: absolute; right: -5px; width: 7px; height: 95%; text-align: center;"><b>.<br/>.<br/>.</b></div>--%>
            </div>
            <div id="divContent" class="content_pane" style="display: inline-block; text-align: left; vertical-align: top; overflow: auto; height: 100%; width: 300px; border: 0;">
                <table border="0" cellpadding="0" cellspacing="0"><tbody><tr>
                    <td><div id="tabs" class="ec_tabs ec_tabs_nocontent"></div></td>
                    <td><a onclick="showLegend();return false;" style="cursor:pointer;"><i class="fas fa-info-circle" style="font-size:16px; margin:5px;" title="Legend"></i>Legend</a></td>
                </tr></tbody></table> 
                <canvas id="dataGridTable">HTML5 is not supported</canvas>
            </div>
        </div>
    </div>

    <div id='selectColumnsForm' style='display: none; position: relative;'>
        <div id="divSelectColumns" style="padding: 5px; text-align: left;"></div>
        <div style='text-align: center; margin-top: 1ex; width: 100%;'>
            <a href="" onclick="filterColumnsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
            <a href="" onclick="filterColumnsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
        </div>
    </div>
</asp:Content>