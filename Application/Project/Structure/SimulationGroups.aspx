<%@ Page Language="VB" Inherits="SimulationGroupsPage" Title="Groups" CodeBehind="SimulationGroups.aspx.vb" %>

<asp:Content ID="SimulationGroupsContent" ContentPlaceHolderID="PageContent" runat="Server">
    <style type="text/css">
        .dx-treelist-rowsview .dx-data-row .dx-cell-modified .dx-highlight-outline::after {
            border-color: transparent;
        }
    </style>
    <script language="javascript" type="text/javascript">

        var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
        var ajaxMethod = _method_POST; // don't use method GET because of caching

        var lblAltGroups = "<% = ParseString("%%Alternative%% Simulation Groups")%>";
        var lblSourceGroups = "<% = ParseString("%%Objective(l)%% Simulation Groups")%>";

        var alternatives = [];
        var sources = [];
        //var simGroups = [{ "guid" : "-1", "name" : "All" }];
        var simGroups = [];
        var altGroups = [];
        var sourceGroups = [];

        var tabAlts = 0;
        var tabSources = 1;

        var tabID = tabSources; // 0 - alternative groups, 1 - source groups

        var edit_current_group_guid;
        var current_group_guid = "-1";

        /* Toolbar */
        var toolbarItems = [{
                location: 'before',
                widget: 'dxButton',
                locateInMenu: 'auto',
                options: {
                    icon: "plus",
                    text: "Add Group",
                    onClick: function() {
                        formDialog();
                    }
                }
            }, {
                location: 'before',
                widget: 'dxButton',
                locateInMenu: 'auto',
                options: {
                    icon: "edit",
                    text: "Manage Groups",
                    onClick: function() {
                        manageGroups();
                    }
                }
            }
        ];

        function updateButtons() {
            var btn_groups_dd = $("#btn_groups_dropdown").dxList("instance");
            if ((btn_groups_dd)) {
                btn_groups_dd.beginUpdate();
                btn_groups_dd.option("dataSource", []);
                btn_groups_dd.option("dataSource", simGroups);
                btn_groups_dd.option("selectedItemKeys", [current_group_guid]);
                btn_groups_dd.endUpdate();
            }

            var btn_groups_edit = $("#btn_groups_edit").dxButton("instance");
            if ((btn_groups_edit)) {
                btn_groups_edit.option("disabled", simGroups.length == 0);
            }

            var btn_groups_delete = $("#btn_groups_delete").dxButton("instance");
            if ((btn_groups_delete)) {
                btn_groups_delete.option("disabled", simGroups.length == 0);
            }            
        }

        function updateToolbar() {                
            $("#toolbar").dxToolbar("instance").beginUpdate();            
            
            $("#toolbar").dxToolbar("instance").endUpdate();            
        }

        function initToolbar() {
            $("#toolbar").dxToolbar({
                items: toolbarItems
            });

            $("#toolbar").css("margin-bottom", 0);        
        }
        /* end Toolbar */

        /* Tabs */
        function initTabs() {
            $("#tabs").dxTabPanel({
                items: [{ id: tabSources, title: lblSourceGroups, icon: "icon ec-hierarchy" },
                        { id: tabAlts, title: lblAltGroups, icon: "icon ec-alts" }],
                selectedIndex: 0,
                onSelectionChanged: function(e) {
                    if (e.addedItems.length > 0) {
                        onSwitchTab(e.addedItems[0].id);
                    }
                }
            });
        }

        function onSwitchTab(idx) {
            tabID = idx;
            simGroups = tabID === tabAlts ? altGroups : sourceGroups;
            if (simGroups.length) current_group_guid = simGroups[0].guid;
            updateToolbar();
            initDatagrid();
        }
        /* end Tabs */

        /* Form Dialog */
        var popupForm = null, cancelled = false, popupFormOptions = {
            width: 550,
            height: "auto",
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    //$("<div class='dx-fieldset-header'>Header</div>"),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Group Name:</div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='tbGroupName'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Group Behavior:</div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='cbGroupBehavior'></div>"))),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Enabled:</div>"),
                        $("<div class='dx-field-value' style='float: left;'></div>").append(
                            $("<div id='cbGroupEnabled'></div>"))),
                    $("<center><div id='btnAddGroup' style='margin-top: 10px; min-width: 100px;'></div><div id='btnCancel' style='margin-top: 10px; min-width: 100px; margin-left: 10px;'></div></center>")
                );
            },
            showTitle: true,
            title: "Add/Edit Group",
            dragEnabled: true,
            closeOnOutsideClick: true
        };

        function initFormDialog() {
            if (popupForm) $(".popupForm").remove();
            var $popupContainer = $("<div></div>").addClass("popupForm").appendTo($("#popupForm"));
            popupForm = $popupContainer.dxPopup(popupFormOptions).dxPopup("instance");
            
            popupForm.show();
            
            $("#tbGroupName").dxTextBox({ width: "100%", value: "", placeholder: "Enter group name here" });

            $("#cbGroupBehavior").dxSelectBox({
                width: "100%",
                items: [ {"id" : <% = GroupBehaviour.Independent %>, "text" : "Independent"},
                         {"id" : <% = GroupBehaviour.MutuallyExclusive %>, "text" : "Mutually Exclusive"},
                         {"id" : <% = GroupBehaviour.MutuallyExclusiveExhaustive %>, "text" : "Mutually Exclusive Collectively Exhaustive"}
                ],
                valueExpr: "id",
                displayExpr: "text",
                value: <% = GroupBehaviour.Independent %>
            });

            $("#cbGroupEnabled").dxCheckBox({ value: true});
            
            $("#btnAddGroup").dxButton({
                text: "<%=ResString("btnSave")%>",
                icon: "fas fa-check",
                elementAttr: { "class": "button_enter" },
                onClick: function() {
                    if ((edit_current_group_guid)) {
                        var name = $("#tbGroupName").dxTextBox('instance').option("value");
                        if (name.trim() != "") { 
                            cancelled = false; 
                            popupForm.hide();
                            editGroup(edit_current_group_guid, name, $("#cbGroupBehavior").dxSelectBox("instance").option("value"), $("#cbGroupEnabled").dxCheckBox("instance").option("value"));
                        }
                    } else {
                        var name = $("#tbGroupName").dxTextBox('instance').option("value");
                        if (name.trim() != "") { 
                            cancelled = false; 
                            popupForm.hide();
                            addGroup(name, $("#cbGroupBehavior").dxSelectBox("instance").option("value"), $("#cbGroupEnabled").dxCheckBox("instance").option("value"));
                        }
                    }
                }
            });
            
            $("#btnCancel").dxButton({
                text: "<%=ResString("btnCancel")%>",
                icon: "fas fa-ban",
                elementAttr: { "class": "button_esc" },
                onClick: function() {
                    cancelled = true;
                    popupForm.hide();
                }
            });
        }

        function formDialog(guid, name, behavior, enabled) {
            cancelled = false;
            edit_current_group_guid = null;

            if (!($("#tbGroupName").data("dxTextBox"))) {
                initFormDialog();
            } else {
                popupForm.show();
            }

            if (typeof guid !== "undefined") edit_current_group_guid = guid;
            $("#tbGroupName").dxTextBox("instance").option("value", typeof name !== "undefined" ? name : "");
            $("#cbGroupBehavior").dxSelectBox("instance").option("value", typeof behavior !== "undefined" ? behavior : <% = GroupBehaviour.Independent %>);
            $("#cbGroupEnabled").dxCheckBox("instance").option("value", typeof enabled !== "undefined" ? enabled : true);
            
            setTimeout(function() {
                $("#tbGroupName").dxTextBox('instance').focus();
            }, 800);            
        }
        /* end form Dialog */

        /* Manage Groups */
        var popupManage = null, popupManageOptions = {
            width: 550,
            height: "auto",
            contentTemplate: function() {
                return  $("<div>").append(
                            $("<center>").append(
                                $("<div id='btnAddNewGroup' style='display: inline-block; margin: 2px;'></div>"),
                                $("<div id='btnEditGroup' style='display: inline-block; margin: 2px;'></div>"),
                                $("<div id='btnDeleteGroup' style='display: inline-block; margin: 2px;'></div>")
                            ),
                            $("<div id='cbGroupsList'></div>"),
                            $("<center><div id='btnClose' style='margin-top: 10px; min-width: 100px;'></div></center>")
                );
            },
            hasCloseButton: true,
            showTitle: true,
            title: "Manage Groups",
            dragEnabled: true,
            closeOnOutsideClick: true
        };

        function manageGroups() {
            if (popupManage) $(".popupManage").remove();
            var $popupContainer = $("<div></div>").addClass("popupManage").appendTo($("#popupManage"));
            popupManage = $popupContainer.dxPopup(popupManageOptions).dxPopup("instance");
            
            popupManage.show();
            
            $("#cbGroupsList").dxList({
                dataSource: simGroups,
                itemTemplate: function(data, _, element) {
                    element.text(data.name + getBehaviorChar(data.behavior));
                },
                elementAttr: {id: 'btn_groups_dropdown'},
                width: "100%",
                keyExpr: "guid",
                displayExpr: "name",
                selectionMode: "single",
                selectedItemKeys: [current_group_guid],
                onSelectionChanged: function(e) {
                    if ((e.addedItems) && e.addedItems.length) current_group_guid =  e.addedItems[0].guid;
                    //updateButtons();
                    //initDatagrid();
                }
            });

            $("#btnAddNewGroup").dxButton({
                icon: "plus",
                text: "Add Group",
                onClick: function() {
                    formDialog();
                }
            });

            $("#btnEditGroup").dxButton({
                icon: "fas fa-pencil-alt",
                text: "Edit Group",
                elementAttr: {id: 'btn_groups_edit'},
                disabled: true,
                onClick: function() {
                    var sel_guid = $("#btn_groups_dropdown").dxList("instance").option("selectedItemKeys")[0];
                    var group;
                    for (var i = 0; i < simGroups.length; i++) {
                        if (simGroups[i].guid == sel_guid) group = simGroups[i];
                    }
                    if ((group)) formDialog(group.guid, group.name, group.behavior, group.enabled);
                }
            });

            $("#btnDeleteGroup").dxButton({
                icon: "minus",
                text: "Delete Group",
                elementAttr: {id: 'btn_groups_delete'},
                disabled: true,
                onClick: function() {                        
                    var sel_guid = $("#btn_groups_dropdown").dxList("instance").option("selectedItemKeys")[0];
                    var sel_name = "";
                    for (var i = 0; i < simGroups.length; i++) {
                        if (simGroups[i].guid == sel_guid) sel_name = simGroups[i].name;
                    }
                    deleteGroup(sel_guid, sel_name);
                }
            });
            
            $("#btnClose").dxButton({
                text: "<%=ResString("btnClose")%>",
                icon: "fas fa-ban",
                elementAttr: { "class": "button_esc" },
                onClick: function() {
                    popupManage.hide();
                }
            });

            updateButtons();
        }
        /* end Manage Groups */

        /* Datagrid */
        function getBehaviorChar(behavior) {
            var behaviorChar = " [I] ";
            switch (behavior) {
                case <% = GroupBehaviour.MutuallyExclusive %>:
                    behaviorChar = " [ME] ";
                    break;
                case <% = GroupBehaviour.MutuallyExclusiveExhaustive %>:
                    behaviorChar = " [MECE] ";
                    break;
            }
            return behaviorChar;
        }

        function initDatagrid() {
            if ($("#divDatagrid").data("dxTreeList")) {
                $("#divDatagrid").dxTreeList("dispose");
            }

            $("#divDatagrid").removeClass().addClass("dx-treelist-withlines dx-treelist-compact");

            var columns = [];
            
            columns.push({ dataField: "name", caption: tabID === tabAlts ? "<%=ParseString("%%Alternative%% Name")%>" : "<%=ParseString("%%Objective(l)%% Name")%>", fixed: true, allowReordering: false, allowEditing: false });
            
            for (var i = 0; i < simGroups.length; i++) {
                var behaviorChar = getBehaviorChar(simGroups[i].behavior);
                columns.push({dataField: "g" + i, caption: simGroups[i].name + behaviorChar, alignment: "center", fixed: false, cssClass: simGroups[i].enabled ? "" : "ec-cell-disabled", dataType: "boolean", allowEditing: !isReadOnly //visible: current_group_guid == "-1" || current_group_guid == simGroups[i].guid,
                    <%--cellTemplate : function (element, info) {            
                        element.append(info.value >= 0 ? "<i class='ec-icon fas fa-check'></i>" + (info.value !== <%= Integer.MaxValue %> ? " #" + info.value : "") : "<i class='ec-icon'></i>");
                        element.addClass("cell_clickable").css({"vertical-align" : "middle"});
                    }--%>
                });
            }

            var dataSource = tabID === tabAlts ? alternatives : sources;

            $("#divDatagrid").dxTreeList({
                autoExpandAll: true,
                allowColumnResizing: true,
                allowColumnReordering: true,
                columns: columns,
                columnResizingMode: "widget",
                columnAutoWidth: true,
                columnFixing: {
                    enabled: true
                },
                dataSource: dataSource,
                dataStructure: "plain",
                editing: {
                    allowUpdating: !isReadOnly,
                    mode: "cell",
                    startEditAction: "click"
                },
                height: "auto",
                highlightChanges: false,
                loadPanel: {
                    enabled: false,
                },
                keyExpr: "id",
                parentIdExpr: "parentId",
                rootValue: "-1",
                rowAlternationEnabled: true,
                showColumnLines: true,
                showRowLines: false,
                columnAutoWidth: true, 
                paging: {
                    enabled: false
                },
                showBorders: true,
                noDataText: "<% = GetEmptyMessage() %>",
                onCellPrepared: getDxTreeListNodeConnectingLinesOnCellPrepared,
                onRowUpdating: function (e) {
                    if ((e.newData)) {
                        var keyNames = Object.keys(e.newData);
                        if (keyNames.length > 0) {
                            var NodeGuid = e.key;
                            var grpIndex = (keyNames[0] + "").substr(1);
                            //$(e.cellElement).find("i")[0].className = e.value < 0 ? "ec-icon fas fa-check" : "ec-icon";
                            callAPI("pm/hierarchy/?action=assign_simulation_group", { "tab" : tabID, "group_index" : grpIndex, "node_guid" : NodeGuid, "precedence" : e.newData[keyNames[0]] ? <%= Integer.MaxValue %> : -1}, function (data) {
                                if (typeof data == "object") {
                                    <%--e.value  = e.value < 0 ? <%= Integer.MaxValue %> : -1;
                                e.data[e.column.dataField] = e.value;
                                e.component.cellValue(e.row.rowIndex, e.column.dataField, e.value);--%>
                                } else {
                                    e.cancel = true;
                                }
                            }, false);
                        }
                    }
                },
                <%--onCellClick: function (e) {
                    if (e.column.index > 0 && e.rowType == "data") {
                        var NodeGuid = e.row.data.id;
                        var grpIndex = (e.column.dataField + "").substr(1);
                        $(e.cellElement).find("i")[0].className = e.value < 0 ? "ec-icon fas fa-check" : "ec-icon";
                        callAPI("pm/hierarchy/?action=assign_simulation_group", { "tab" : tabID, "group_index" : grpIndex, "node_guid" : NodeGuid, "precedence" : e.value < 0 ? <%= Integer.MaxValue %> : -1}, function (data) {
                            if (typeof data == "object") {
                                e.value  = e.value < 0 ? <%= Integer.MaxValue %> : -1;
                                e.data[e.column.dataField] = e.value;
                                e.component.cellValue(e.row.rowIndex, e.column.dataField, e.value);
                            }
                        }, false);
                    }
                },--%>
                //onCellPrepared: function(e) {
                //    if (e.rowType === "header" && e.column.dataField !== "name") {
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
                    $("#btnResetView").dxButton("instance").option("disabled", !e.component.getDataSource() || e.component.getDataSource().sort() == null);
                },
                onToolbarPreparing: function (e) {
                    var toolbarItems = e.toolbarOptions.items;
                    toolbarItems.splice(0, 0, {
                        widget: 'dxButton', 
                        options: { 
                            icon: 'fas fa-sync-alt', 
                            elementAttr: { "id" : "btnResetView" },
                            hint: "Reset",
                            showText: false,
                            visible: true,
                            disabled: false,
                            onClick: function() {
                                e.component.clearSorting();
                            } 
                        },
                        locateInMenu: "auto",
                        showText: "always",
                        location: 'after'
                    });
                    toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: "<h5 id='lblTitle'>" + (tabID === tabAlts ? lblAltGroups : lblSourceGroups) + "</h5>" });
                },
                stateStoring: {
                    enabled: true,
                    type: "localStorage",
                    storageKey: function () { return "SimGroups_<%=CurrentPageID%>_PrjId_<% = App.ProjectID %>_" + tabID; }
                }, 
                sorting: {
                    mode: "multiple"
                },
                wordWrapEnabled: true
            });

            resizePage();
        }
        /* end Datagrid */

        var grid_w_old = 0;
        var grid_h_old = 0;

        function resizeGrid() {
            var margin = 8;
            $("#divDatagrid").height(200).width(300);
            var td = $("#tdContent");
            var w = $("#divDatagrid").width(Math.round(td.innerWidth())-margin).width();
            var h = $("#divDatagrid").height(Math.round(td.innerHeight())-margin).height();
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

        function resizePage(force_redraw) {
            var w = $(window).innerWidth();
            var h = $(window).innerHeight();
            if (force_redraw) {
                grid_w_old = 0;
                grid_h_old = 0;
            }
            setTimeout("checkResize(" + (force_redraw ? 0 : w) + "," + (force_redraw ? 0 : h) +");", 50);
        }
       
        resize_custom = resizePage;

        $(document).ready(function () {            
            toggleScrollMain();
            initToolbar();
            updateToolbar();
            initTabs();
            loadGroupsData();
        });

        function loadGroupsData(is_added) {
            callAPI("pm/hierarchy/?action=get_simulation_groups", {}, function onLoadGroupsData(data) {
                if (typeof data == "object") {
                    alternatives = data.alternatives;
                    sources = data.sources;
                    altGroups = data.altgroups;
                    sourceGroups = data.sourcegroups;

                    simGroups = tabID === tabAlts ? altGroups : sourceGroups;
                    if (simGroups.length) current_group_guid = simGroups[0].guid;

                    if (typeof is_added !== "undefined" && is_added) {
                        current_group_guid = simGroups[simGroups.length - 1].guid;
                    }

                    initDatagrid();
                    updateToolbar();
                    updateButtons();
                }
                //DevExpress.ui.notify("Groups data loaded");
            });
        }                

        function addGroup(name, behavior, enabled) {
            if (name != "") callAPI("pm/hierarchy/?action=add_simulation_group", { "tab" : tabID, "name" : name, "behavior" : behavior, "enabled" : enabled}, onAddGroup);
        }
        
        function onAddGroup(data) {
            //DevExpress.ui.notify("Group added");
            loadGroupsData(true);
        }

        function editGroup(guid, name, behavior, enabled) {
            current_group_guid = guid;
            if (name != "") callAPI("pm/hierarchy/?action=edit_simulation_group", { "tab" : tabID, "guid" : guid, "name" : name, "behavior" : behavior, "enabled" : enabled}, onEditGroup);
        }
        
        function onEditGroup(data) {
            //DevExpress.ui.notify("Group changes saved");
            loadGroupsData();
        }

        function deleteGroup(guid, name) {
            dxConfirm(replaceString("{0}", name, resString("msgSureDeleteCommon")), function () {
                callAPI("pm/hierarchy/?action=delete_simulation_group", { "tab" : tabID, "guid" : guid}, function () {
                    //DevExpress.ui.notify(name + " deleted");
                    current_group_guid = "";
                    if (simGroups.length) current_group_guid = simGroups[0].guid;
                    loadGroupsData();
                });
            }); 
        }

    </script>

    <table border='0' cellspacing="0" cellpadding="0" class="whole">
        <tr valign="top">
            <td valign="top">
                <div id="tabs" class="ec_tabs ec_tabs_nocontent" style="margin-top: 8px; width: 99%;"></div>
            </td>
        </tr>
        <tr valign="top">
            <td valign="top">
                <div id="toolbar" style="border-left: 1px solid #cccccc; background-color: transparent; padding: 2px; padding-right: 8px;"></div>
            </td>
        </tr>
        <%--<tr valign="top">
            <td valign="top">
                <h5 id='lblTitle' style='padding-bottom: 3px;'></h5>
            </td>
        </tr>--%>
        <tr valign="top">
            <td id="tdContent" valign="top" style="overflow: hidden; text-align: left;" class="whole">
                <div id="divDatagrid" class="dx-treelist-withlines dx-treelist-compact"></div>
            </td>
        </tr>
    </table>

    <div id="popupForm" class="cs-popup"></div>
    <div id="popupSettings" class="cs-popup"></div>
    <div id="popupManage" class="cs-popup"></div>

</asp:Content>