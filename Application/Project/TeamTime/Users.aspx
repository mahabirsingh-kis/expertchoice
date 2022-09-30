<%@ Page Language="vb" CodeBehind="Users.aspx.vb" Inherits="TeamTimeUsersPage" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server">
    <script type="text/javascript">

        var mode_none = <% =CInt(SynchronousEvaluationMode.semNone) %>;
        var mode_online = <% =CInt(SynchronousEvaluationMode.semOnline) %>;
        var mode_keypad = <% =CInt(SynchronousEvaluationMode.semVotingBox) %>;
        var mode_viewonly = <% =CInt(SynchronousEvaluationMode.semByFacilitatorOnly) %>;
        var keypads_max  = 255;

        var grid = null;
        var cb_all = null;
        var ignore_saving = true;
        var userslist = null;
        var groups = [<% =CombinedGroupsWebAPI.List_JSON(App.ActiveProject.ProjectManager) %>];
        var activeUserID = <%= App.ActiveUser.UserID %>;
        var fileNameExport = "<% =JS_SafeString(String.Format("{1} - {0}", App.ActiveProject.ProjectName, PageMenuItem(CurrentPageID))) %>";
        var opt_UseKeypads = <% =Bool2JS(App.ActiveProject.PipeParameters.SynchUseVotingBoxes AndAlso App.Options.KeypadsAvailable) %>;
        var opt_ShowKeypadNums = <% =Bool2JS(App.ActiveProject.PipeParameters.TeamTimeShowKeypadNumbers) %>;
        var opt_MeetingID_allow = <% =Bool2JS(App.ActiveProject.PipeParameters.TeamTimeAllowMeetingID) %>;

        function accessModes() {
            var lst = [{"value": mode_online, "text": "<% =JS_SafeString(ResString("lblSynchronousActive")) %>"}];
            <% If App.Options.KeypadsAvailable Then %>if (opt_UseKeypads) lst.push({"value": mode_keypad, "text": "<% =JS_SafeString(ResString("lblSynchronousKeypad")) %>"});<% End if %>
            lst.push({"value": mode_viewonly, "text": "<% =JS_SafeString(ResString("lblSynchronousViewOnly")) %>"});
            return lst;
        }

        function getSelected() {
            var lst = [];
            if ((grid)) {
                var selected = grid.getSelectedRowsData();
                for (var i=0; i<selected.length; i++) {
                    var row = selected[i];
                    if ((row.AccessMode !== mode_none && row.CanEvaluate) || row.ID == activeUserID) { lst.push(row.ID); }
                }
            }
            return lst;
        }
        
        function updateSelection() {
            if ((grid)) {
                var rowsToSelect = [];
                for (i = 0; i < userslist.length; i ++) {
                    var row = userslist[i];
                    if ((row.AccessMode !== mode_none && row.CanEvaluate) || row.ID == activeUserID) { rowsToSelect.push(row.ID) };
                };
                ignore_saving = true;
                grid.selectRows(rowsToSelect, false);
                updateSelectAll();
                setTimeout(function () { ignore_saving = false; }, 30);
            }
        }

        function updateSelectAll() {
            if ((grid) && (cb_all)) {
                var has_sel = false;
                var has_unsel = false;
                for (i = 0; i < userslist.length; i ++) {
                    var row = userslist[i];
                    if (row.CanEvaluate && row.ID != activeUserID) {
                        if (row.AccessMode !== mode_none) { 
                            has_sel = true;
                        } else {
                            has_unsel = true;
                        }
                    }
                }
                cb_all.dxCheckBox("option", "value", (has_sel && has_unsel ? undefined : (has_sel)));
            }
        }

        function checkKeypadsForSelected() {
            var users = [];
            for (i = 0; i < userslist.length; i ++) {
                var u = userslist[i];
                if (u.AccessMode == mode_keypad) {<% If App.Options.KeypadsAvailable Then %>
                    var old = u.Keypad;
                    var k = canSetKeypad(u);
                    if (k<1 || k!=old)
                    {
                        if (k<1) {
                            u.AccessMode = mode_online;
                        } else {
                            u.Keypad = k;
                        }
                        initAMOption(u);
                        users.push(getUserOptions(u));
                    }<% Else %>
                    u.AccessMode = mode_online;
                    initAMOption(u);
                    users.push(getUserOptions(u));
                    <% End If %>
                }
            }
            if (users.length) {
                updateUsers(users);
                DevExpress.ui.notify("<% = JS_SafeString(ResString("msgKeypadsFixed")) %>", "warning");
            }
            return ((users.length));
        }

        function getUserByID(id) {
            id = Number(id);
            if ((userslist.length) && (id>0)) {
                for (var i=0; i<userslist.length; i++) {
                    if (userslist[i].ID == id) return userslist[i];
                }
            }
            return null;
        }

        function getNextFreeKeypad() {
            if ((userslist.length)) {
                for (var k=1; k<=keypads_max; k++) {
                    var used = false;
                    for (var u=0; u<userslist.length; u++) {
                        if (userslist[u].Keypad == k) {
                            used = true;
                            break;
                        }
                    }
                    if (!used) return k;
                }
            }
            return -1;
        }

        function isKeypadInUse(idx, ignore_uid) {
            if ((userslist.length)) {
                for (var u=0; u<userslist.length; u++) {
                    if (userslist[u].Keypad == idx && userslist[u].ID != ignore_uid) {
                        return userslist[u].ID;
                    }
                }
            }
            return -1;
        }

        function setKeypadsForSelection(reset) {
            ignore_saving = true;
            var users = [];
            for (var i=0; i<userslist.length; i++) {
                var u = userslist[i];
                if (u.AccessMode != mode_none) {
                    if ((reset && u.AccessMode == mode_keypad) || (!reset && (u.AccessMode != mode_keypad || u.Keypad<1 || u.Keypad>keypads_max))) {
                        if (changeMode(u.ID, (reset ? mode_online : mode_keypad), false)) users.push(getUserOptions(u));
                    }
                }
            }
            ignore_saving = false;
            if ((users.length)) updateUsers(users); else if (!ignore_saving) DevExpress.ui.notify("Nothing to change", "warning");
        }

        // return the Keypad# if possible, or <1 otherwise
        function canSetKeypad(user) {
            var keypad = -1;
            if ((user)) {
                var keypad = user.Keypad;
                var inuse = isKeypadInUse(keypad, user.ID);
                var renew = (user.Keypad<1 || user.Keypad>keypads_max);
                if ((renew) || (inuse>=0)) { 
                    if (!(renew) && (inuse>=0)) {
                        var u_used = getUserByID(inuse);
                        if ((u_used) && (u_used.AccessMode != mode_keypad)) {
                            keypad = u_used.Keypad;
                            u_used.Keypad = getNextFreeKeypad();
                            renew = false;
                        } else {
                            renew = true;
                        }
                    }
                    if ((renew)) keypad = getNextFreeKeypad();
                }
            }
            return keypad;
        }

        function setKeypad(u, keypad) {
            if ((u)) {
                u.Keypad = keypad;
                var k = canSetKeypad(u);
                if (k == keypad || keypad<1 || keypad>keypads_max) {
                    u.Keypad = k;
                    return true;
                } else {
                    initAMOption(u);
                }
            }
            return false;
        }

        function changeMode(id, val, save_data) {
            var u = getUserByID(id);
            if ((u) && u.AccessMode != val) {
                var isKeypad = (val == mode_keypad);
                if (isKeypad) {
                    if (!setKeypad(u, u.Keypad)) {
                        if ((save_data)) DevExpress.ui.dialog.alert(resString("msgCantSetKeypad"), resString("msgWarning"));
                        return false;
                    }
                } 
                u.AccessMode = val;
                if ((save_data)) {
                    updateUser(u);
                }
                initAMOption(u);
                return true;
            }
        }

        function changeKeypad(id, val, save_data) {
            var u = getUserByID(id);
            if ((u) && val>0 && val<=keypads_max && (u.Keypad != val)) {
                var old = u.Keypad;
                if (setKeypad(u, val)) {
                    if (u.Keypad == val && (save_data)) {
                        updateUser(u);
                    }
                } else {
                    u.Keypad = old;
                    DevExpress.ui.dialog.alert(resString("msgCantChangeKeypad"), resString("msgWarning"));
                }
                initAMOption(u);
            }
        }

        function enableUser(user) {
            if ((user) && (user.AccessMode == mode_none)) {
                if (changeMode(user.ID, mode_online, false)) return true;
            }
            return false;
        }

        function disableUser(user) {
            if ((user)) {
                user.AccessMode = mode_none;
                return true;
            }
            return false;
        }

        function selectUsers(sel_add, sel_del) {
            var users = [];
            var new_del = 0;
            var new_add = 0;
            var update_checks = false;
            for (var i=0; i<sel_add.length; i++) {
                var u = getUserByID(sel_add[i]);
                if ((u) && enableUser(u)) {
                    users.push(getUserOptions(u)); 
                    initAMOption(u);
                    new_add++;
                } else {
                    update_checks = true;
                }
            }
            for (var i=0; i<sel_del.length; i++) {
                var u = getUserByID(sel_del[i]);
                if ((u) && (disableUser(u)) && (sel_del[i]!=activeUserID)) {
                    users.push(getUserOptions(u)); 
                    initAMOption(u);
                    new_del++;
                } else { 
                    update_checks = true;
                }
            }
            if (update_checks) {
                updateSelection();
                ignore_saving = true;
                setTimeout(function () {
                    ignore_saving = false;
                }, 150);
            }
            if (users.length) {
                updateUsers(users); 
                var msg = "";
                if (new_add>0) msg = " Included " + new_add + " user(s);";
                if (new_del>0) msg = " Excluded " + new_del + " user(s);";
                //if (msg!="") DevExpress.ui.notify(msg, "info");
            } else {
                if (!ignore_saving) DevExpress.ui.notify("Nothing to change", "warning");
            }
        }

        function getUserOptions(user) {
            if ((user)) return {"Email": user.Email, "AccessMode": user.AccessMode, "Keypad": user.Keypad}; else return {};
        }

        function updateUsers(users) {
            if ((users) && users.length) {
                callAPI("pm/user/?<% =_PARAM_ACTION %>=teamtime_userslist_update", {"users": users}, onUpdate, "Saving...");
            }
        }

        function updateUser(user) {
            if ((user)) {
                callAPI("pm/user/?<% =_PARAM_ACTION %>=teamtime_user_update", getUserOptions(user), onUpdate, "Saving...");
            }
        }

        function onUpdate(res) {
            var err = (res.Result != _res_Success);
            if (err || res.Message != "" ) {
                showResMessage(res, err);
            } else {
                if (typeof res.Data != "undefined" && (res.Data)) {
                    DevExpress.ui.notify("<% =JS_SafeString(ResString("msgSaved")) %>", "info");
                }
            }
        }

        function getColumns() {
            return [{
                dataField: "Email",
                caption: "<% =JS_SafeString(ResString("lblEmail")) %>",
                fixed: true,
                fixedPosition: "left",
                sortIndex: 0, 
                sortOrder: 'asc',
                cellTemplate: function(element, info) {
                    var color = (info.data.isOnline ? "online_marker" : "offline_marker");
                    var tooltip = (info.data.isOnline ? "<% =JS_SafeString(ResString("lblOnline")) %>" : "<% =JS_SafeString(ResString("lblOffline")) %>");
                    var actionLink = <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %>(info.data.Link !== "" && info.data.CanEvaluate ? "<a style='margin-right:5px;float:right;' href='#' onclick='copyDataToClipboard(\""+ info.data.Link +"\"); return false;' title='Copy user link to clipboard'><i class='fas fa-link fa-xs'></i></a>" : "")<% Else %>""<% End if %>;
                    var onlineIcon = "<span style='margin-right:5px;' title='" + tooltip + "' class='fa fa-circle fa-xs " + color + "'></span>";
                    var email = info.text;
                    if (info.text.indexOf("@") > -1) email = "<a href='mailto:" + email + "'>" + email + "</a>";
                    element.append(onlineIcon + actionLink + email);
                }
            }, {
                dataField: "Name",
                caption: "<% =JS_SafeString(ResString("lblTTSortByName")) %>"
            }, {
                dataField: "HasData",
                calculateCellValue: function(rowData) {
                    return ((rowData.HasData)) ? "<% =JS_SafeString(ResString("lblYes")) %>" : "<% =JS_SafeString(ResString("lblNo")) %>";
                },
                caption: "<% =JS_SafeString(ResString("tblUserHasData")) %>",
                minWidth: 90,
                maxWidth: 150,
                width: 110,
                alignment: "center", 
            }, {
                dataField: "isOnline",
                headerFilter: {
                    dataSource: [{"value": false, "text": "<% =JS_SafeString(ResString("lblNo")) %>"}, {"value": true, "text": "<% =JS_SafeString(ResString("lblYes")) %>"}]
                },
                caption: "<% =JS_SafeString(ResString("lblOnline")) %>",
                minWidth: 70,
                maxWidth: 120,
                width: 110,
                visible: false
            }, {
                dataField: "AccessMode",
                caption: "<% =JS_SafeString(ResString("lblTTAccessMode")) %>",
                alignment: "left",
                minWidth: <% =If(App.Options.KeypadsAvailable, 200, 150) %>,
                width: <% =If(App.Options.KeypadsAvailable, 200, 150) %>,
                maxWidth: 220,
                headerFilter: {
                    dataSource: accessModes()
                },
                customizeText: function(cellInfo) {
                    switch (cellInfo.value) {
                        case mode_online: return "<% =JS_SafeString(ResString("lblSynchronousActive")) %>";
                        case mode_keypad: return "<% =JS_SafeString(ResString("lblSynchronousKeypad")) %>";
                        case mode_viewonly: return "<% =JS_SafeString(ResString("lblSynchronousViewOnly")) %>";
                    }
                    return "<% =JS_SafeString(ResString("lblNone")) %>";
                },
                cellTemplate: function(element, info) {
                    element[0].style.paddingTop = "3px";
                    element[0].style.paddingBottom = "0px";
                    element[0].style.verticalAlign = "top";
                    element[0].style.align = "center";
                    $("<nobr><div id='mode_" + info.data.ID + "' style='width:<% =If(App.Options.KeypadsAvailable, "135px; float:left;", "100%") %>; height:22px; margin:0px 2px; display:inline-block;'></div><% If App.Options.KeypadsAvailable Then %><div id='keypad_" + info.data.ID + "' class='keypad_num' style='float:right;'></div><% End If %></nobr>").appendTo(element);
                }
            }, {
                dataField: "Groups",
                caption: "<% =JS_SafeString(ResString("lblGroups")) %>",
                visible: false
            }];
        }

        function initGrid() {
            if ((grid)) {
                $("#grid").dxDataGrid("dispose");
                $("#grid").html("");
            }
            $("#grid").dxDataGrid({
                dataSource: new DevExpress.data.ArrayStore({
                    key: "ID",
                    data: userslist
                }),
                columns: getColumns(),
                stateStoring: {
                    enabled: true,
                    type: "localStorage",   //"sessionStorage",
                    storageKey: "TTUsersLst_<% =App.ProjectID %>"
                },
                rowAlternationEnabled: true,
                onCellPrepared: function(e) {             
                    if (e.rowType === "data" && e.column.command === 'select') {
                        if (!(e.data.CanEvaluate) || e.data.ID == activeUserID) {
                            var cb = e.cellElement.find('.dx-select-checkbox').dxCheckBox("instance");
                            cb.option("disabled", true);
                            e.cellElement.off();
                        };
                    }
                    if (e.rowType === "header" && e.column.command === 'select') {
                        //cb_all = e.cellElement.find('.dx-select-checkbox').dxCheckBox("instance");
                        //cb_all.value = false;
                        e.cellElement.find('.dx-select-checkbox').hide()
                        cb_all = $("<div id='cbSelectAll'></div>").appendTo(e.cellElement);
                        cb_all.dxCheckBox({
                            value: undefined,
                            onValueChanged: function(data) {
                                if (!ignore_saving) {
                                    var do_sel = (data.value == true);
                                    var sel_add = [];
                                    var sel_del = [];
                                    if ((grid)) {
                                        var visrows = grid.getVisibleRows();
                                        for (var i=0; i<visrows.length; i++) {
                                            var u = visrows[i].data;
                                            if (u.CanEvaluate && u.ID != activeUserID) {
                                                if (do_sel && u.AccessMode == mode_none) sel_add.push(u.ID);
                                                if (!do_sel && u.AccessMode != mode_none) sel_del.push(u.ID);
                                            }
                                        }
                                    }
                                    //for (var i=0; i<userslist.length; i++) {
                                    //    var u = userslist[i];
                                    //    if (u.CanEvaluate && u.ID != activeUserID) {
                                    //        if (do_sel && u.AccessMode == mode_none) sel_add.push(u.ID);
                                    //        if (!do_sel && u.AccessMode != mode_none) sel_del.push(u.ID);
                                    //    }
                                    //}
                                    ignore_saving = true;
                                    if (sel_add.length || sel_del.length) selectUsers(sel_add, sel_del);
                                    ignore_saving = false;
                                }
                            }
                        });
                    }
                },
                onSelectionChanged: function(e) {
                    if (!ignore_saving) {
                        selectUsers(e.currentSelectedRowKeys, e.currentDeselectedRowKeys);
                        setTimeout(function () {
                            ignore_saving = true;
                            updateSelectAll();
                            ignore_saving = false;
                        }, 100);
                    }
                },
                pager: {
                    allowedPageSizes: [10, 15, 20, 50, 100, 200],
                    showInfo: true,
                    showNavigationButtons: true,
                    showPageSizeSelector: true,
                    visible: true
                },
                paging: {
                    pageSize: 50
                },
                scrolling: {
                    showScrollbar: "always",
                    mode: "standard",
                },
                filterRow: {
                    visible: false,
                    applyFilter: "auto"
                },
                hoverStateEnabled: true,
                columnHidingEnabled: <% =Bool2JS(App.isMobileBrowser) %>,
                columnAutoWidth: true,
                allowColumnResizing: true,
                allowColumnReordering: true,
                searchPanel: {
                    visible: true,
                    width: 240,
                    placeholder: resString("btnDoSearch") + "..."
                },
                headerFilter: {
                    allowSearch: true,
                    visible: true
                },
                showBorders: false,
                "export": {
                    enabled: true,
                    allowExportSelectedData: true,
                    excelFilterEnabled : true,
                    fileName: fileNameExport
                },
                grouping: {
                    contextMenuEnabled: true
                },
                groupPanel: {
                    visible: "auto"
                },
                columnFixing: {
                    enabled: true
                },
                selection: {
                    mode: "multiple",
                    allowSelectAll: true,
                    showCheckBoxesMode: "always"
                },
                columnChooser: {
                    enabled: true,
                    mode: "select", 
                    title: resString("lblColumnChooser"),
                    height: 400,
                    width: 250,
                    emptyPanelText: resString("lblColumnChooserPlace")
                },
                onToolbarPreparing: onCreateGridToolbar,
                loadPanel: main_loadPanel_options,
                onContentReady: function (e) {
                    initAMOptions();
                    updateSelection();
                    checkKeypadsForSelected();
                },
            });
            grid = $("#grid").dxDataGrid("instance");
            resizeGrid();
        }

        function initAMOptions() {
            if ((userslist) && (userslist.length)) {
                for (var i=0; i<userslist.length; i++) {
                    initAMOption(userslist[i]);
                }
            }
        }

        function initAMOption(user) {
            if ((user)) {
                ignore_saving = true;
                var isKeypad = <% If App.Options.KeypadsAvailable Then %>(user.AccessMode == mode_keypad)<% Else %>false<% End If %>;
                var isEnabled = (user.AccessMode > mode_none && user.CanEvaluate);
                var am = $("#mode_" + user.ID);
                if ((am) && (am.length)) {
                    if (am.html()=="") {
                        am.dxSelectBox({
                            dataSource: new DevExpress.data.ArrayStore({ 
                                data: accessModes(),
                                key: "value"
                            }),
                            elementAttr: {"data-uid": user.ID},
                            displayExpr: "text",
                            valueExpr: "value",
                            value: user.AccessMode,
                            onValueChanged: function(e) {
                                if (!ignore_saving) changeMode($(e.element).data("uid"), e.value, true);
                            }
                        }); 
                    } else {
                        am.dxSelectBox("option", "value", user.AccessMode);
                    }
                    am.toggle(isEnabled);
                }<% If App.Options.KeypadsAvailable Then %>
                var kp = $("#keypad_" + user.ID);
                if ((kp) && (kp.length)) {
                    if (kp.html()=="") {
                        kp.dxButton({
                            text: "" + user.Keypad,
                            elementAttr: {"data-uid": user.ID},
                            onClick: function() {
                                showLoadingPanel();
                                setTimeout(function () { showKeypads(user); }, 50);
                            }
                        })
                        //kp.dxNumberBox({
                        //    min: 1,
                        //    max: keypads_max,
                        //    value: user.Keypad,
                        //    mode: "number",
                        //    showSpinButtons: true,
                        //    elementAttr: {"data-uid": user.ID},
                        //    onValueChanged: function(e) {
                        //        if (!ignore_saving) changeKeypad($(e.element).data("uid"), e.value, true);
                        //    }
                        //});
                    } else {
                        //kp.dxNumberBox("option", "value", user.Keypad);
                        kp.dxButton("option", "text", "" + user.Keypad);
                    }
                    kp.toggle(isEnabled  && isKeypad);
                }<% End If %>
                ignore_saving = false;
            }
        }

        function showKeypads(user) {
            if ((user)) {
                var btns = "";
                for (var i=1; i<=keypads_max; i++) {
                    btns += "<div id='h_keypad_" + i + "' style='display:inline-block'><div id='_keypad_" + i + "' class='keypad_num' style='margin-bottom:4px;'></div></div> ";
                }

                $("#popupContainer").dxPopup({
                    width: dlgMaxWidth(830),
                    height: dlgMaxWidth(550),
                    contentTemplate: function() {
                        return $("<div style='text-align:center;' id='divKeypads'>" + btns + "</div>");
                    },
                    showTitle: true,
                    title: "<% = JS_SafeString(ResString("lblSelectKeypad")) %>",
                    dragEnabled: true,
                    shading: true,
                    closeOnOutsideClick: true,
                    resizeEnabled: true,
                    showCloseButton: false,
                    toolbarItems: [{
                        widget: 'dxButton',
                        options: {
                            text: "<% = JS_SafeString(ResString("btnClose")) %>",
                            onClick: function (e) {
                                closePopup();
                            }
                        },
                        toolbar: "bottom",
                        location: 'center'
                    }]
                });
                $("#popupContainer").dxPopup("show");
                for (var i=1; i<=keypads_max; i++) {
                    var isActive = false;
                    var inUse = false;
                    var checked = false;
                    var tooltip = "";
                    if (user.Keypad == i) {
                        isActive = true;
                        checked = true;
                    } else {
                        var u = isKeypadInUse(i, -1);
                        if (u>0) {
                            inUse = true;
                            var usr = getUserByID(u);
                            if ((usr)) {
                                tooltip = usr.Email;
                                checked = (usr.AccessMode != mode_none);
                            }
                        }
                    }
                    $("#_keypad_" + i).dxButton({
                        text: "" + i,
                        type: (isActive ? "default" : ""),
                        //elementAttr: {"title": tooltip},
                        elementAttr: {"data-keypad": i},
                        disabled: (!isActive && inUse && checked),
                        //hint: tooltip,
                        onClick: function(e) {
                            var k = $(e.element).data("keypad");
                            if ((k) && k != user.Keypad) {
                                if (setKeypad(user, k)) {
                                    closePopup();
                                    user.Keypad = k;
                                    initAMOption(user);
                                    updateUser(user);
                                } else {
                                    DevExpress.ui.dialog.alert(resString("msgCantSetKeypad"), resString("msgWarning"));
                                }
                            }
                        }
                    });
                    if (inUse && !checked) $("#_keypad_" + i).css("font-style", "italic");
                    if (inUse || isActive) $("#_keypad_" + i).css("font-weight", "bold");
                    if (tooltip!="") $("#h_keypad_" + i).prop("title", tooltip);
                }
                hideLoadingPanel();
            }
        }

        function onCreateGridToolbar(e) {
            var toolbarItems = e.toolbarOptions.items;

            <%If App.Options.KeypadsAvailable Then %>toolbarItems.push({
                widget: 'dxButton', 
                options: { 
                    icon: "ion ion-md-keypad", 
                    text: resString("btnSetKeypadNumbers"), 
                    hint: resString("btnSetKeypadNumbers"), 
                    disabled: !(opt_UseKeypads),
                    elementAttr: {
                        id: "btnSetAllKeypads",
                    },
                    onClick: function() {
                        var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgSetAllKeypads")) %>", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
                        result.done(function (dialogResult) {
                            if (dialogResult) setKeypadsForSelection(false); 
                        });
                    }
                },
                locateInMenu: "auto",
                showText: "always",
                location: 'center'
            });<% End If %>

            //toolbarItems.splice(0, 0, {
            toolbarItems.push({
                widget: 'dxButton', 
                options: { 
                    icon: 'fa fa-user-check', 
                    elementAttr: {id: 'btnSelectGroup'},
                    text: resString("btnTTSelectGroup"), 
                    hint: resString("hintTTSelectGroup"), 
                    disabled: !groups.length,
                    onClick: function() {
                        selectByGroup();
                    }
                },
                locateInMenu: "auto",
                showText: "inMenu",
                location: 'center'
            });
            toolbarItems.push({
                widget: 'dxButton', 
                options: { 
                    icon: "fa fa-user-cog",
                    elementAttr: {id: 'btnManageGroups'},
                    text: "Manage Participant Groups...",
                    hint: "Manage Groups [Navigate to other page]",
                    onClick: function() {
                        loadURL('<%=JS_SafeString(PageURL(_PGID_PROJECT_USERS, "dlg=groups")) %>');
                    }
                },
                locateInMenu: "auto",
                showText: "inMenu",
                location: 'center'
            });

            toolbarItems.push({
                widget: 'dxButton', 
                options: { 
                    icon: 'fa fa-sync', 
                    elementAttr: {id: 'btnGridReload'},
                    text: resString("btnRefresh"), 
                    hint: resString("btnRefresh"), 
                    onClick: function() {
                        loadUsersList();
                    } 
                },
                locateInMenu: "auto",
                showText: "inMenu",
                location: 'after'
            });
        }

        function selectByGroup() {
            ignore_saving = true;
            var ml = 15;
            for (var i=0; i<groups.length; i++) {
                if (groups[i].title.length > ml) ml = groups[i].title.length;
            }
            $("#popupContainer").dxPopup({
                contentTemplate: function() {
                    return $("<div id='divGroups' style='max-height:" + dlgMaxHeight(500) + "px; max-width:" + Math.round(ml*0.65+3) + "em;overflow:auto;'></div>");
                },
                showTitle: true,
                width: "auto",
                height: "auto",
                title: "<% = JS_SafeString(ResString("btnTTSelectGroup")) %>",
                dragEnabled: true,
                shading: true,
                closeOnOutsideClick: true,
                resizeEnabled: true,
                showCloseButton: false,
                toolbarItems: [{
                    widget: 'dxButton',
                    options: {
                        text: "<% = JS_SafeString(ResString("btnClose")) %>",
                        onClick: function (e) {
                            closePopup();
                        }
                    },
                    toolbar: "bottom",
                    location: 'center'
                }]
            });
            $("#popupContainer").dxPopup("show");
            var lst = $("#divGroups").dxList({
                dataSource: groups,
                keyExpr: "id",
                displayExpr: "title",
                selectedItemKeys: [],
                allowItemDeleting: false,
                showSelectionControls: true,
                selectionMode: "multiple",
                onSelectionChanged: function (e) {
                    if (!ignore_saving) checkByGroup(e.addedItems, e.removedItems, e.component.option("selectedItemKeys"));
                },
                onItemRendered: function (e) {
                    if (e.itemElement) {
                        var c = $(e.itemElement).find(".dx-checkbox");
                        if ((c) && (c.data("dxCheckBox"))) {
                            var grp_id = e.itemData.id;
                            var total = 0;
                            var select = 0;
                            if ((userslist) && (userslist.length) && (groups) && (groups.length)) {
                                for (k=0; k<userslist.length; k++) {
                                    var u = userslist[k];
                                    if (u.GroupIDs.indexOf(grp_id)>=0) {
                                        total++;
                                        if (u.AccessMode != mode_none) {
                                            select++;
                                        }
                                    }
                                }
                            }
                            c.dxCheckBox("instance").option({value: (total==select ? true : (select>0 ? undefined: false))});
                        }
                    }
                }
            }).dxList("instance");
            $("#popupContainer").dxPopup("hide");
            $("#popupContainer").dxPopup("show");
            ignore_saving = false;
        }

        function checkByGroup(sel_add, sel_del, sel_cur) {
            if ((groups) && (groups.length) && (userslist) && (userslist.length)) {
                var new_add = [];
                var new_del = [];
                if (typeof sel_cur == "undefined") sel_cur = [];
                for (var i=0; i<sel_del.length; i++) {
                    var d = sel_del[i].id;
                    for (k=0; k<userslist.length; k++) {
                        var u = userslist[k];
                        if (typeof u.GroupIDs != "undefined" && u.AccessMode != mode_none && u.ID!=activeUserID && u.CanEvaluate && u.GroupIDs.indexOf(d)>=0) {
                            // need to check is other groups is checked for this user
                            var have_other = false;
                            for (var j=0; j<sel_cur.length; j++) {
                                var g = sel_cur[j];
                                if (g != d && u.GroupIDs.indexOf(g)>=0) {
                                    have_other = true;
                                    break;
                                }
                            }
                            if (!have_other) new_del.push(u.ID);
                        }
                    }
                }
                for (var i=0; i<sel_add.length; i++) {
                    var a = sel_add[i].id;
                    for (k=0; k<userslist.length; k++) {
                        var u = userslist[k];
                        if (typeof u.GroupIDs != "undefined" && u.AccessMode == mode_none && u.ID!=activeUserID && u.CanEvaluate && u.GroupIDs.indexOf(a)>=0) new_add.push(u.ID);
                    }
                }
                if (new_add.length || new_del.length) {
                    selectUsers(new_add, new_del);
                    updateSelection();
                }
            }
        }

        function loadUsersList() {
            callAPI("pm/user/?<% =_PARAM_ACTION %>=teamtime_userslist", {}, onLoadUsersList);
        }

        function onLoadUsersList(res) {
            if (res.Result == _res_Success) {
                userslist = res.Data;
                initGrid();
                setTimeout(function () {
                    var tbSearch = $("input", ".dx-searchbox");
                    if ((tbSearch) && (tbSearch.length)) { tbSearch.select(); tbSearch.focus(); }
                }, 500);
            } else {
                showResMessage(res, true);
            }
        }

        function resizeGrid() {
            var d = ($("body").hasClass("fullscreen") ? 12 : 8);
            $("#grid").height(300).width(400);
            var td = $("#trGrid");
            var w = $("#grid").width(Math.round(td.width() - d - 12)).width();
            var h = $("#grid").height(Math.round(td.height() - d)).height();
        }

        resize_custom = resizeGrid;
        loadUsersList();

        $(document).ready(function () {
            <% If App.Options.KeypadsAvailable Then %>$("#cbUseKeypads").dxCheckBox({
                text: resString("lblUseKeypads"),
                value: opt_UseKeypads,
                onValueChanged: function(data) {
                    var keypads = ((data.value));
                    opt_UseKeypads = keypads;
                    callAPI("pm/params/?<% =_PARAM_ACTION %>=set_pipe_option", {"name": "SynchUseVotingBoxes", "value": opt_UseKeypads}, onUpdate);
                    $("#cbShowKeypadNumbers").dxCheckBox("instance").option("disabled", !keypads);
                    $("#btnSetAllKeypads").dxButton("instance").option("disabled", !keypads);
                    if (!data.value) {
                        setKeypadsForSelection(true);
                    };
                    setTimeout(function () { 
                        initAMOptions(); 
                        grid.columnOption(4, "headerFilter", { dataSource: accessModes() });
                    }, 300);
                }
            });
            $("#cbShowKeypadNumbers").dxCheckBox({
                text: resString("lblSyncOption_ShowKeypadNumbers"),
                value: opt_ShowKeypadNums,
                onValueChanged: function(data) {
                    opt_ShowKeypadNums = data.value;
                    callAPI("pm/params/?<% =_PARAM_ACTION %>=set_pipe_option", {"name": "TeamTimeShowKeypadNumbers", "value": opt_ShowKeypadNums}, onUpdate);
                }
            });<% End If %>
            $("#cbAllowDirectLinks").dxCheckBox({
                text: resString("lblSyncOption_OnlyEmails"),
                value: opt_MeetingID_allow,
                onValueChanged: function(data) {
                    opt_MeetingID_allow = data.value;
                    callAPI("pm/params/?<% =_PARAM_ACTION %>=set_pipe_option", {"name": "TeamTimeAllowMeetingID", "value": opt_MeetingID_allow}, onUpdate);
                }
            });
        });

    </script>
    <div class="table">
        <div class="tr" style="height:1em;" id="tdOptions">
            <div class="td" style="padding:0px 16px 0px 0px">
                <h5 style="padding:4px"><% =SafeFormString(PageTitle(CurrentPageID)) %></h5>
                <table width="100%">
                    <tr valign="middle">
                        <td style="text-align:left; padding:0px 1em 1ex 0px;" class="text">
                            <% If App.Options.KeypadsAvailable Then %><div><nobr><div id="cbUseKeypads"></div></nobr></div>
                            <div><nobr><div style="margin-left:20px;" id="cbShowKeypadNumbers"></div></nobr></div><% End If %>
                            <div><nobr><div id="cbAllowDirectLinks"></div></nobr></div>
                        </td>
                        <td style="text-align:right;">
                            <div style="font-size:110%;"><nobr><h6 style='display: inline-block; color:black;'><% =ResString("lblMeetingID") %></h6>:</nobr> <nobr><a href='' title='<% =SafeFormString(ResString("titleCopyToClipboard")) %>' onclick='copyDataToClipboard("<% =JS_SafeString(ApplicationURL(False, True) + "/?" + _PARAM_MEETING_ID + "=" + clsMeetingID.AsString(App.ActiveProject.MeetingID)) %>"); return false;' class='actions dashed'><% =SafeFormString(clsMeetingID.AsString(App.ActiveProject.MeetingID)) %></a></nobr></div>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <div class="tr" id="trGrid">
            <div class="td" id="tdGrid">
                <div id="grid" class="whole"><div class="whole gray" style="text-align:center"><img src="/Images/process_64.gif" width="64" height="64" style="margin-top:25%" /><br /> &nbsp; Loading...</div></div>
            </div>
        </div>
    </div>
</asp:Content>