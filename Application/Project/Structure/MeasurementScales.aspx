<%@ Page Language="VB" Inherits="MeasurementScalesPage" Title="Measurement Scales" CodeBehind="MeasurementScales.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <script language="javascript" type="text/javascript">
    
        var isReadOnly = <% = Bool2JS(App.IsActiveProjectStructureReadOnly) %>;
        var columns_ver = "210310";
        var storageKey = "MS_DG_<% = PRJ.ID %>_" + columns_ver;
    var OPT_PLEASE_WAIT_DELAY = 3000;
    var mscales, selected_row_guid;

    /* Data Table */
    function initTable() {
        var columns = [];
            
        //init columns headers                
        columns.push({ "caption" : "Measurement Type", "visible" : false, "alignment" : "left", "dataField": "smt", "groupIndex": 0, "allowEditing": false, "showInColumnChooser": false });
        columns.push({ "caption" : "Measurement Scale", "alignment" : "left", "width" : "100%", "dataField": "name", "allowEditing": false });
        //columns.push({ "caption" : "Description", "visible" : true, "alignment" : "left", "dataField": "comment", "allowEditing": false });
        columns.push({ "caption" : "Is Default", "alignment" : "center", "width" : "100px", "dataField": "is_default", "allowEditing": false, "trueText" : "<% = ResString("lblYes") %>", "falseText" : " ", "showEditorAlways": false });
        columns.push({ "caption" : "Applied to #", "alignment" : "center", "width" : "100px", "dataField": "apps", "allowEditing": false });
               
        $('#tableScales').dxDataGrid({
            allowColumnResizing: true,
            allowColumnReordering: true,
            dataSource: mscales,
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
                allowUpdating: false
            },
            focusedRowEnabled:true,
            hoverStateEnabled: true,
            grouping: {
                autoExpandAll: true,
            },
            groupPanel: {
                visible: false
            },
            keyExpr: "guid",
            onFocusedRowChanged: function (e) {
                updateToolbar();
            },
            onToolbarPreparing: function(e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.splice(0, 0, {
                    widget: 'dxButton', 
                    options: {
                        elementAttr: {id: "btnMoveUp"},
                        icon: 'chevronup', text: "Move Up",
                        onClick: function() { 
                            var dg = $('#tableScales').dxDataGrid("instance");
                            selected_row_guid = dg.option("focusedRowKey");        
                            var ms = getScaleById(selected_row_guid);
                            if ((ms) && !isReadOnly) {
                                $("#btnMoveUp").dxButton("instance").option("disabled", true);
                                callAPI("pm/hierarchy/?action=movemeasurementscaleup", { id: ms.guid }, function (res) {
                                    if (isValidReply(res) && res.Result == _res_Success) {
                                        mscales = res.Data;

                                        dg.option("dataSource", mscales);
                                        dg.refresh(true);
                                    }
                                }, false, OPT_PLEASE_WAIT_DELAY);
                            }
                        } 
                    },
                    locateInMenu: "never", 
                    location: 'before'
                });
                toolbarItems.splice(1, 0, {
                    widget: 'dxButton', 
                    options: { 
                        elementAttr: {id: "btnMoveDown"},
                        icon: 'chevrondown', text: "Move Down",
                        onClick: function() { 
                            var dg = $('#tableScales').dxDataGrid("instance");
                            selected_row_guid = dg.option("focusedRowKey");        
                            var ms = getScaleById(selected_row_guid);
                            if ((ms) && !isReadOnly) {
                                $("#btnMoveDown").dxButton("instance").option("disabled", true);
                                callAPI("pm/hierarchy/?action=movemeasurementscaledown", { id: ms.guid }, function (res) {
                                    if (isValidReply(res) && res.Result == _res_Success) {
                                        mscales = res.Data;

                                        dg.option("dataSource", mscales);
                                        dg.refresh(true);
                                    }
                                }, false, OPT_PLEASE_WAIT_DELAY);
                            }
                        } 
                    },
                    locateInMenu: "never", 
                    location: 'before'
                });
                toolbarItems.splice(2, 0, {
                    widget: 'dxButton',
                    options: {
                        elementAttr: {id: "btnDeleteScale"},
                        icon: 'remove', text: "Delete",
                        onClick: function() { 
                            var dg = $('#tableScales').dxDataGrid("instance");
                            selected_row_guid = dg.option("focusedRowKey");        
                            var ms = getScaleById(selected_row_guid);
                            if ((ms) && ms.apps === 0 && !isReadOnly) {
                                var msg = replaceString("{0}", ms.name, "<% = JS_SafeString(ResString("msgSureDeleteCommon")) %>");
                                dxConfirm(msg, function () {
                                    $("#btnDeleteScale").dxButton("instance").option("disabled", true);
                                    callAPI("pm/hierarchy/?action=deletemeasurementscale", { id: ms.guid }, function (res) {
                                        if (isValidReply(res) && res.Result == _res_Success) {
                                            mscales.splice(mscales.indexOf(ms), 1);

                                            dg.option("dataSource", mscales);
                                            dg.refresh(true);
                                        } else {
                                            if (res.Message !== "") {
                                                dxDialog(res.Message);
                                            }
                                        }
                                    }, false, OPT_PLEASE_WAIT_DELAY);
                                });
                            }
                        } 
                    },
                    locateInMenu: "never", 
                    location: 'before'
                });
                toolbarItems.splice(3, 0, {
                    widget: 'dxButton',
                    options: {
                        elementAttr: {id: "btnMakeDefault"},
                        icon: 'todo', text: "Make Default",
                        onClick: function() { 
                            var dg = $('#tableScales').dxDataGrid("instance");
                            selected_row_guid = dg.option("focusedRowKey");        
                            var ms = getScaleById(selected_row_guid);
                            if ((ms) && !ms.is_default && !isReadOnly) {
                                $("#btnMakeDefault").dxButton("instance").option("disabled", true);
                                callAPI("pm/hierarchy/?action=setmeasurementscaledefault", { id: ms.guid }, function (res) {
                                    if (isValidReply(res) && res.Result == _res_Success) {
                                        for (var i = 0; i < mscales.length; i++) {
                                            if (mscales[i].mt == ms.mt && mscales[i].rst == ms.rst) {
                                                mscales[i].is_default = false;
                                            }
                                        }
                                        ms.is_default = true;

                                        dg.option("dataSource", mscales);
                                        dg.refresh(true);

                                    }
                                }, false, OPT_PLEASE_WAIT_DELAY);
                            }
                        } 
                    },
                    locateInMenu: "never", 
                    location: 'before'
                });
            },
            pager: {
                visible: false
            },
            paging: {
                enabled: false
            },
            rowAlternationEnabled: false,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: "<% = ResString("btnDoSearch") %>..."
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: storageKey
            },
            sorting: {
                mode: 'none'
            },
            "export": {
                "enabled" : true
            },
            wordWrapEnabled: true                        
        });

        updateToolbar();
    }
    /* end DataTable */

    function updateToolbar() {
        var btnMoveUp = $("#btnMoveUp").dxButton("instance");
        btnMoveUp.option("disabled", true);
        var btnMoveDown = $("#btnMoveDown").dxButton("instance");
        btnMoveDown.option("disabled", true);
        var btnDeleteScale = $("#btnDeleteScale").dxButton("instance");
        btnDeleteScale.option("disabled", true);
        var btnMakeDefault = $("#btnMakeDefault").dxButton("instance");
        btnMakeDefault.option("disabled", true);
        selected_row_guid = $('#tableScales').dxDataGrid("instance").option("focusedRowKey");        
        var ms = getScaleById(selected_row_guid);
        if ((ms) && !isReadOnly) {
            var setting = { isFirst : false, isLast : false };
            var count = scalesOfSameTypeCount(ms, setting);
            if (count > 1) {
                btnDeleteScale.option("disabled", ms.is_default || ms.apps > 0);
                if (!setting.isFirst) btnMoveUp.option("disabled", false);
                if (!setting.isLast) btnMoveDown.option("disabled", false);
            }
            btnMakeDefault.option("disabled", ms.is_default);
        }
    }

    function scalesOfSameTypeCount(scale, setting) {
        var scales = [];
        for (var i = 0; i < mscales.length; i++) {
            if (mscales[i].mt == scale.mt && mscales[i].rst == scale.rst) {
                scales.push(mscales[i]);
            }
        }
        var scales_len = scales.length;
        if (scales_len > 0) {
            setting.isFirst = scales[0].guid == scale.guid;
            setting.isLast = scales[scales_len - 1].guid == scale.guid;
        }
        return scales_len;
    }

    function getScaleById(id) {
        for (var i = 0; i < mscales.length; i++) {
            if (mscales[i].guid == id) return mscales[i];
        }
        return null;
    }

    $(document).ready(function () {
        callAPI("pm/hierarchy/?action=getmeasurementscales", {}, function (res) {
            if (isValidReply(res) && res.Result == _res_Success) {
                mscales = res.Data;
                initTable();
            }
        }, false, OPT_PLEASE_WAIT_DELAY);
    });

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid() {
        $("#tableScales").height(200).width(300);
        var td = $("#tdContent");
        var w = $("#tableScales").width(Math.round(td.innerWidth())).width();
        var h = $("#tableScales").height(Math.round(td.innerHeight()) - 20).height();
        if ((grid_w_old !== w || grid_h_old !== h)) {
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
        $("#tdContent").height(0).height($("#tblMSMain").height() - $("#lblHeader").height());
        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        setTimeout("checkResize(" + (force_redraw ? 0 : w) + "," + (force_redraw ? 0 : h) +");", 50);
    }

    resize_custom  = resizePage;
    toggleScrollMain();

    </script>

    <div class="whole" id='tblMSMain'>
        <h5 id="lblHeader" style="margin-top: 15px;">Default Scales</h5>
        <div id='tdContent' style="width: 100%;">
            <div id='tableScales'></div>
        </div>
    </div>
</asp:Content>
