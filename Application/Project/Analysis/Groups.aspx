<%@ Page Language="VB" Inherits="ResultGroupsPage" Title="Groups" CodeBehind="Groups.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="ResultGroupsContent" ContentPlaceHolderID="PageContent" runat="Server">
    <style type="text/css">
        .dx-datagrid-rowsview .dx-data-row .dx-cell-modified .dx-highlight-outline::after {
            border-color: transparent;
        }
    </style>
    <script language="javascript" type="text/javascript">

        var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
        var ajaxMethod = _method_POST; // don't use method GET because of caching
        var please_wait_delay = 4000; //ms

        var participants = [];
        var groups = [];
        var attributes = [];

        var edit_current_group_id;
        var current_group_id = "-1";
        var columns_ver = "_20201215";

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

        function updateToolbar() {
            var btn_groups_dd = $("#btn_groups_dropdown").dxList("instance");
            if ((btn_groups_dd)) {
                btn_groups_dd.beginUpdate();
                btn_groups_dd.option("dataSource", []);
                btn_groups_dd.option("dataSource", groups);
                btn_groups_dd.option("selectedItemKeys", [current_group_id]);
                btn_groups_dd.endUpdate();
            }

            var btn_groups_edit = $("#btn_groups_edit").dxButton("instance");
            if ((btn_groups_edit)) {
                btn_groups_edit.option("disabled", groups.length == 0);
            }

            var btn_groups_delete = $("#btn_groups_delete").dxButton("instance");
            if ((btn_groups_delete)) {
                btn_groups_delete.option("disabled", groups.length == 0);
            }            
        }

        function initToolbar() {
            $("#toolbar").dxToolbar({
                items: toolbarItems
            });

            $("#toolbar").css("margin-bottom", 0);        
        }
        /* end Toolbar */

        /* Form Dialog */
        var popupForm = null, cancelled = false, popupFormOptions = {
            width: 550,
            height: "auto",
            maxHeight: function() {
                return window.innerHeight * 0.9;
            },
            dragEnabled: true,
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    //$("<div class='dx-fieldset-header'>Header</div>"),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'>Group Name:</div>"),
                        $("<div class='dx-field-value' style='width: 100%;'></div>").append(
                            $("<div id='tbGroupName'></div>"))),                    
                    $("<center><div id='btnAddGroup' style='margin-top: 25px; min-width: 100px;'></div><div id='btnCancel' style='margin-top: 25px; min-width: 100px; margin-left: 10px;'></div></center>")
                );
            },
            showTitle: true,
            title: "Add/Edit Group",
            closeOnOutsideClick: true
        };

        function initFormDialog() {
            if (popupForm) $(".popupForm").remove();
            var $popupContainer = $("<div></div>").addClass("popupForm").appendTo($("#popupForm"));
            popupForm = $popupContainer.dxPopup(popupFormOptions).dxPopup("instance");
            
            popupForm.show();                      
            
            $("#tbGroupName").dxTextBox({ width: "100%", value: "", placeholder: "Enter group name here" });
           
            $("#btnAddGroup").dxButton({
                text: "<%=ResString("btnSave")%>",
                icon: "fas fa-check",
                elementAttr: { "class": "button_enter" },
                onClick: function() {
                    if ((edit_current_group_id)) {
                        var name = $("#tbGroupName").dxTextBox('instance').option("value");
                        if (name.trim() != "") { 
                            cancelled = false; 
                            popupForm.hide();
                            editGroup(edit_current_group_id, name);
                        }
                    } else {
                        var name = $("#tbGroupName").dxTextBox('instance').option("value");
                        if (name.trim() != "") { 
                            cancelled = false; 
                            popupForm.hide();
                            addGroup(name);
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
            
            popupForm.show();
        }

        function formDialog(id, name) {
            cancelled = false;
            edit_current_group_id = null;

            if (!($("#tbGroupName").data("dxTextBox"))) {
                initFormDialog();
            } else {
                popupForm.show();
            }

            if (typeof id !== "undefined") edit_current_group_id = id;
            $("#tbGroupName").dxTextBox("instance").option("value", typeof name !== "undefined" ? name : "");
            
            setTimeout(function() {
                $("#tbGroupName").dxTextBox('instance').focus();
            }, 800);            
        }
        /* end form Dialog */

        /* Manage Groups */        
        var popupManage = null;

        function manageGroups() {
            if (popupManage) $(".popupManage").remove();
            var $popupContainer = $("<div></div>").addClass("popupManage").appendTo($("#popupManage"));
            var groupsReordered = false;

            var popupManageOptions = {
                width: 550,
                height: function() {  
                    return window.innerHeight * 0.9;
                },
                maxHeight: function() {
                    return window.innerHeight * 0.9;
                },
                onHidden: function (e) {
                    if (groupsReordered) {
                        //$("#divDatagrid").dxDataGrid("instance").state({});
                        var dg = $("#divDatagrid").dxDataGrid("instance");
                        dg.beginUpdate();
                        dg.option("columns", getColumns());
                        dg.endUpdate();
                    }
                },
                dragEnabled: true,
                contentTemplate: function(container) {
                    var divTopButtons = $("<div>").css("text-align", "center");
                    var scrollGroups = $("<div>").prop("id", "scrollGroups");
                    var btnAddNewGroup = $("<div id='btnAddNewGroup' style='display: inline-block; margin: 2px;'></div>");
                    var btnEditGroup = $("<div id='btnEditGroup' style='display: inline-block; margin: 2px;'></div>");
                    var btnDeleteGroup = $("<div id='btnDeleteGroup' style='display: inline-block; margin: 2px;'></div>");
                    var cbGroupsList = $("<div id='cbGroupsList' style='margin-top: 15px;'></div>");
                    var btnCloseGroups = $("<div id='btnClose' style='margin-top: 10px; min-width: 100px;'></div>");
                    var $div = $("<div>").css("height", "calc(100% - 60px)").append(
                                divTopButtons.append(
                                    btnAddNewGroup,
                                    btnEditGroup,
                                    btnDeleteGroup
                                ),
                                scrollGroups.append(cbGroupsList),
                                $("<center></center>").append(btnCloseGroups)
                    );

                    scrollGroups.dxScrollView({
                        //height: '100%',
                        //width: '100%'          
                    });

                    cbGroupsList.dxList({
                        dataSource: groups,
                        //new DevExpress.data.DataSource({
                        //    store: new DevExpress.data.ArrayStore({
                        //        key: 'id',
                        //        data: groups
                        //    }),
                        //    paginate: false
                        //}),
                        elementAttr: {id: 'btn_groups_dropdown'},
                        itemDragging: {
                            allowDropInsideItem: false,
                            allowReordering: !isReadOnly,
                            moveItemOnDrop: true,
                            onReorder: function (e) {
                                groupsReordered = true;
                                var item = groups[e.fromIndex];
                                groups.splice(e.fromIndex, 1);
                                groups.splice(e.toIndex, 0, item);
                                callAPI("pm/dashboard/?action=move_result_group", { "fromindex" : e.fromIndex + 1, "toIndex" : e.toIndex + 1}, function (data) { // +1 because the first results group is "all participants"
                                    if (typeof data == "object") {
                                        
                                    }
                                }, false, please_wait_delay);
                            }
                        },
                        width: "100%",               
                        keyExpr: "id",
                        displayExpr: "name",
                        pageLoadMode: "scrollBottom",
                        selectionMode: "single",
                        selectedItemKeys: [current_group_id],
                        onContentReady: function(e) {
                            $(e.element).find(".dx-empty-message").css("text-align", "center");
                        },
                        onSelectionChanged: function(e) {
                            if ((e.addedItems) && e.addedItems.length) current_group_id =  e.addedItems[0].id;
                        }
                    });

                    btnAddNewGroup.dxButton({
                        icon: "plus",
                        text: "Add Group",
                        onClick: function() {
                            formDialog();
                        }
                    });

                    btnEditGroup.dxButton({
                        icon: "fas fa-pencil-alt",
                        text: "Edit Group",
                        elementAttr: {id: 'btn_groups_edit'},
                        disabled: true,
                        onClick: function() {
                            var sel_id = $("#btn_groups_dropdown").dxList("instance").option("selectedItemKeys")[0];
                            var group;
                            for (var i = 0; i < groups.length; i++) {
                                if (groups[i].id == sel_id) group = groups[i];
                            }
                            if ((group)) formDialog(group.id, group.name);
                        }
                    });

                    btnDeleteGroup.dxButton({
                        icon: "minus",
                        text: "Delete Group",
                        elementAttr: {id: 'btn_groups_delete'},
                        disabled: true,
                        onClick: function() {                        
                            var sel_id = $("#btn_groups_dropdown").dxList("instance").option("selectedItemKeys")[0];
                            var sel_name = "";
                            for (var i = 0; i < groups.length; i++) {
                                if (groups[i].id == sel_id) sel_name = groups[i].name;
                            }
                            deleteGroup(sel_id, sel_name);
                        }
                    });
                    
                    btnCloseGroups.dxButton({
                        text: "<%=ResString("btnClose")%>",
                        icon: "fas fa-ban",
                        elementAttr: { "class": "button_esc" },
                        onClick: function() {
                            popupManage.hide();
                        }
                    });

                    container.append($div);

                    return container;
                },
                hasCloseButton: true,
                showTitle: true,
                title: "Manage Groups",
                visible: true,
                closeOnOutsideClick: true
            };

            popupManage = $popupContainer.dxPopup(popupManageOptions).dxPopup("instance");

            updateToolbar();
        }
        /* end Manage Groups */

        /* Datagrid */
        function getColumns() {
            var columns = [];
            
            columns.push({ dataField: "email", caption: resString("tblUserEmail"), fixed: true, allowReordering: true, allowEditing: false, visibleIndex: 0 });
            columns.push({ dataField: "name", caption: resString("tblSyncUserName"), fixed: true, allowReordering: true, allowEditing: false, visibleIndex: 1 });
            columns.push({ dataField: "role", caption: resString("tblUserRole"), fixed: true, allowReordering: true, allowEditing: false, visibleIndex: 2 });
            //columns.push({ dataField: "is_disabled", caption: resString("tblUserDisabled"), fixed: true, allowReordering: true, allowEditing: false, width: 80 });
            
            var lastColumnVisibleIndex = columns[columns.length - 1].visibleIndex;
            var groupsColumn = { caption: "Groups", alignment: "center", columns: [] };
            for (var i = 0; i < groups.length; i++) {
                lastColumnVisibleIndex += 1;
                groupsColumn.columns.push({ dataField: "g" + groups[i].id, caption: groups[i].name, alignment: "center", fixed: false, dataType: "boolean", allowEditing: !isReadOnly && !groups[i].is_dynamic, cssClass: groups[i].is_dynamic ? "group_cell_disabled" : "", customizeText : function (e) { return e.value ? "Yes" : ""; }, visibleIndex: lastColumnVisibleIndex });
            }
            columns.push(groupsColumn);

            if (attributes.length) {
                var attrsColumn = { caption: "Attributes", alignment: "center", columns: [] };
                for (var i = 0; i < attributes.length; i++) {
                    lastColumnVisibleIndex += 1;
                    attrsColumn.columns.push({ dataField: "a" + i, caption: attributes[i].name, alignment: "center", fixed: false, allowEditing: false, cssClass: attributes[i].readonly ? "attr_cell_readonly" : "", visibleIndex: lastColumnVisibleIndex });
                }
                columns.push(attrsColumn);
            }
            return columns;
        }

        var dataSource;

        function initDatagrid() {
            if ($("#divDatagrid").data("dxDataGrid")) {
                $("#divDatagrid").dxDataGrid("dispose");
            }

            var columns = getColumns();            

            //var dataSource = participants;

            dataSource = new DevExpress.data.ArrayStore({
                key: 'email',
                data: participants
            });

            $("#divDatagrid").dxDataGrid({
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
                    enabled: true
                },
                dataSource: dataSource,
                dataStructure: "plain",
                editing: {
                    allowUpdating: !isReadOnly,
                    mode: "cell",
                    startEditAction: "click"
                },
                "export": {
                    enabled: true,
                    fileName: "ParticipantGroups"
                },
                highlightChanges: false,
                hoverStateEnabled: true,                
                focusedRowEnabled: true,
                keyExpr: "email",
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
                    visible: true
                },
                paging: {
                    enabled: true,
                    //pageSize: 10
                },
                //scrolling: {
                //    mode: "virtual"
                //},
                showBorders: true,
                noDataText: "<% =GetEmptyMessage()%>",
                onRowUpdating: function (e) {
                    if ((e.newData)) {
                        var keyNames = Object.keys(e.newData);
                        if (keyNames.length > 0) {
                            var email = e.key;
                            var grpID = (keyNames[0] + "").substr(1);
                            callAPI("pm/dashboard/?action=assign_result_group", { "group_id" : grpID, "user_email" : email, "value" : e.newData[keyNames[0]] }, function (data) {
                                if (typeof data !== "object") {                                    
                                    e.cancel = true;
                                }
                            }, false, please_wait_delay);
                        }
                    }
                },
                onToolbarPreparing: function (e) {
                    var toolbarItems = e.toolbarOptions.items; 
                    toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: '<h6 style="padding-top: 10px;">Participant Groups</h6>' });
                },
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
                stateStoring: {
                    enabled: true,
                    type: "localStorage",
                    storageKey: function () { return "res_groups_<% = CurrentPageID %>_PRJ_ID_<% = App.ProjectID %>" + columns_ver; }
                }, 
                wordWrapEnabled: true
            });

            resizePage();
        }
        /* end Datagrid */

        var grid_w_old = 0;
        var grid_h_old = 0;

        function resizeGrid() {
            var margin = 16;
            $("#divDatagrid").height(200).width(300);
            var td = $("#tdContent");
            var w = $("#divDatagrid").width(Math.round(td.innerWidth()) - margin - 8).width();
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
       
        resize_custom = resizePage;

        $(document).ready(function () {            
            initToolbar();
            loadGroupsData();
        });

        function loadGroupsData(is_added) {
            callAPI("pm/dashboard/?action=get_result_groups", {}, function onLoadGroupsData(data) {
                if (typeof data == "object") {
                    participants = data.participants;
                    groups = data.groups;
                    attributes = data.attributes;
                    if (groups.length) current_group_id = groups[0].id;

                    if (typeof is_added !== "undefined" && is_added) {
                        current_group_id = groups[groups.length - 1].id;
                    }

                    initDatagrid();
                    updateToolbar();
                }
                //DevExpress.ui.notify("Groups data loaded");
            }, false, please_wait_delay);
        }                

        function addGroup(name) {
            if (name != "") callAPI("pm/dashboard/?action=add_result_group", { "name" : name }, onAddGroup, false, please_wait_delay);
        }
        
        function onAddGroup(data) {
            //DevExpress.ui.notify("Group added");
            loadGroupsData(true);
        }

        function editGroup(id, name) {
            current_group_id = id;
            if (name != "") callAPI("pm/dashboard/?action=edit_result_group", { "id" : id, "name" : name }, onEditGroup, false, please_wait_delay);
        }
        
        function onEditGroup(data) {
            //DevExpress.ui.notify("Group changes saved");
            loadGroupsData();
        }

        function deleteGroup(id, name) {
            dxConfirm(replaceString("{0}", name, resString("msgSureDeleteCommon")), function () {
                callAPI("pm/dashboard/?action=delete_result_group", { "id" : id}, function () {
                    //DevExpress.ui.notify(name + " deleted");
                    current_group_id = "";
                    if (groups.length) current_group_id = groups[0].id;
                    loadGroupsData();
                }, false, please_wait_delay);
            }); 
        }

    </script>

    <table border='0' cellspacing="0" cellpadding="0" class="whole">
        <tr valign="top">
            <td valign="top">
                <div id="toolbar" style="background-color: transparent; padding-top: 5px;"></div>
            </td>
        </tr>
        <%--<tr valign="top">
            <td valign="top">
                <h5 id='lblTitle' style='padding-bottom: 3px;'>Participant Groups</h5>
            </td>
        </tr>--%>
        <tr valign="top">
            <td id="tdContent" valign="top" style="overflow: hidden; text-align: left;" class="whole">
                <div id="divDatagrid" style="overflow: hidden;"></div>
            </td>
        </tr>
    </table>

    <div id="popupForm" class="cs-popup"></div>
    <div id="popupSettings" class="cs-popup"></div>
    <div id="popupManage" class="cs-popup"></div>

</asp:Content>