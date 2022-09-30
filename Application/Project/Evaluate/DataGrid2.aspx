<%@ Page Language="VB" Inherits="DataGrid2Page" title="Data Grid 2" Codebehind="DataGrid2.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">
    var toolbar = null;
    var groupsUsersList = <%=getGroupsUsersList() %>;
    var selectedUserID = <% =SelectedUserID %>;
    var lblDataGridUser = "<%=ResString("lblDataGridUser")%>";
    var pageURL = '<% =JS_SafeString(PageURL(CurrentPageID, _PARAM_ACTION + "=download")) %>';
    var data = <%= GetDataGridSource() %>;
    var columns = <%= GetDataGridColumns() %>;
    
   function loadDGData(extra) {
        //var nodeID = options.WRTNodeID;
        var nodeID = GridWRTNodeID;
        if (typeof options.showAlternatives == "undefined") {options.showAlternatives = true};
        if (!options.showAlternatives) {nodeID = <% = PM.ActiveObjectives.Nodes(0).NodeID %>};
        if (typeof extra == "undefined") extra = {};
        extra["wrt"] = nodeID;
        extra["users"] = "selected";
        callAPI("pm/dashboard/?action=synthesize", extra, onLoadDGData);
    }

    function onLoadDGData(data) {
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


    function syncReceived(params) {       
        if ((params)) {            
            var received_data = eval(params);
            if ((received_data)) {
                if (received_data[0] == 'rows'){
                    data = received_data[1];
                    if ($("#dataGridTable").hasClass("dx-treelist")) {                   
                        $("#dataGridTable").dxTreeList("option","dataSource", data);
                    } else {
                        var dgData = getDGRows(columns, data);
                        $("#dataGridTable").dxDataGrid("option","dataSource", dgData);
                    };
                };
            };
        } 
        hideLoadingPanel();
    }

    $(document).ready(function () {
        if (selectedUserID <= 0 && (groupsUsersList.length)) selectedUserID = groupsUsersList[0].id;
        initToolbar();
        resize_custom = function (force_redraw) { resize() };
        initDataGrid(false);
    });
    function resize() {
        $("#dataGridTable").hide();
        var gridHeight = $("#divContent").outerHeight()  - 4;
        $("#divContent").width('100%');
        var gridWidth = $("#divContent").outerWidth() - 20;   
        $("#divContent").width(gridWidth);
        //$("#dataGridTable").dxTreeList("option","width", gridWidth);
        $("#dataGridTable").show();
        if ($("#dataGridTable").hasClass("dx-treelist")) { 
            $("#dataGridTable").dxTreeList("option","height", gridHeight);
        } else {
            $("#dataGridTable").dxDataGrid("option","height", gridHeight);
        };
        
    }
    function getDGRows(cols, nodes){
        var rows = [];
        for (var i = 0; i < cols.length; i++) {
            var col = cols[i];
            if (col.dataField != "title") {
                var aRow = {title: col.caption};
                for (var r = 0; r < nodes.length; r++){
                    var aNode = nodes[r];
                    aRow["node" + aNode.nodeid] = aNode[col.dataField];       
                };
                rows.push(aRow);
            };  
        };
        return rows;
    }
    function getDGColumns(nodes) {
        var cols = [{dataField:"title",caption:"Node", columns:[], fixed: true, fixedPosition: 'left'}];
        for (var i = 0; i < nodes.length; i++) {
            var item = nodes[i];
            if (item.pid == 0) {
                var col = {dataField:'node'+item.nodeid,caption: item.title, nodeid: item.nodeid, pid: item.pid, columns: []};
                cols.push(col);
            };
        };
        for (var i = 0; i < nodes.length; i++) {
            var item = nodes[i];
            var col = {dataField:'node'+item.nodeid, caption: item.title, nodeid: item.nodeid, pid: item.pid, columns: []};
            var pCol = getNodeByNodeID(col.pid, cols);
            if (pCol != null) {
                pCol.columns.push(col);
            };
        };
        return cols;
    }
    function getNodeByNodeID(nodeid, nodes) {
        for (var i = 0; i < nodes.length; i++) { 
            var item = nodes[i];
            if (item.nodeid == nodeid) {return item};
            var childItem = getNodeByNodeID(nodeid, item.columns);
            if (childItem != null) { return childItem };
        };
        return null;
    }

    function exportToExcel() {
        var tv = $("#dataGridTable").dxTreeList("instance");
        var columns = tv.getVisibleColumns();
        var data = tv.getVisibleRows();

        var csvContent = "";

        for (i = 0; i < columns.length; i++) {            
            csvContent += columns[i].caption + ";";
        }       
        csvContent += "\r\n";

        for (i = 0; i < data.length; i++) { 
            row = data[i].values;
            row.forEach(function (item, index) {
                if (item === undefined || item === null) { csvContent += ";"; }
                else { csvContent += item + ";"; };
            }
            );
            csvContent += "\r\n";
        }            

        var blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });

        if (navigator.msSaveBlob) {
            navigator.msSaveBlob(blob, 'TreeList.csv')
        }
        else {
            var link = document.createElement("a");
            var url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "TreeList.csv");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);  
        };
    }

    function initDataGrid(isGridView) {
        if (isGridView) {
            var gridHeight = $("#divContent").outerHeight()  - 4;
            var dgNodes = getDGColumns(data);
            var dgData = getDGRows(columns, data);
            $("#dataGridTable").dxDataGrid({
                dataSource: dgData,
                columns: dgNodes,
                height: gridHeight,
                columnChooser: {
                    enabled: true,
                    mode: "select"
                },
                searchPanel: { visible: true },
                "export": {
                    enabled: true,
                    fileName: "DataGrid"
                },
                allowColumnResizing: false,
                columnHidingEnabled: false,
                columnAutoWidth: true,
                columnFixing: {
                    enabled: true
                },
                showRowLines: true,
                editing: {
                    mode: "cell",
                    allowUpdating: true
                },
            })
        } else {
            var expandedNodes = [];
            for (var i = 0; i < data.length; i++) { 
                var item = data[i];
                expandedNodes.push(item.nodeid);
            };
            var gridHeight = $("#divContent").outerHeight()  - 4;
            //var gridWidth = $("#divContent").outerWidth()  - 32;
            $("#dataGridTable").dxTreeList({
                dataSource: data,
                keyExpr: "nodeid",
                parentIdExpr: "pid",
                allowColumnResizing: false,
                columnHidingEnabled: false,
                columnAutoWidth: true,
                columnFixing: {
                    enabled: true
                },
                columnChooser: {
                    enabled: true,
                    mode: "select"
                },
                searchPanel: { visible: true },
                columns: columns,
                expandedRowKeys: expandedNodes,
                showRowLines: true,
                showBorders: true,
                height: gridHeight,
                onToolbarPreparing: function (e) {
                    var toolbarItems = e.toolbarOptions.items; 
                    toolbarItems.unshift({
                        widget: "dxButton", 
                        options: { icon: "export", onClick: exportToExcel },
                        location: "after"
                    });
                },
                editing: {
                    mode: "cell",
                    allowUpdating: true
                },
            });
        };
        $(document).on('focus', '#dataGridTable input', function() {
            $(this).select();
        });
    }

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: [{
                location: 'before',
                template: function() {
                    return $("<div>" + lblDataGridUser + "</div>");
                }
            }, {
                location: 'before',
                widget: 'dxSelectBox',
                locateInMenu: 'auto',
                options: {
                    width: 140,
                    items: groupsUsersList,
                    valueExpr: "id",
                    displayExpr: "text",
                    value: selectedUserID,
                    onValueChanged: function(args) {
                        var val = args.value;
                        selectedUserID = val;
                        showLoadingPanel();
                        //$("#btnUploadDataGrid").dxButton("instance").option("disabled", (val < 0));
                        sendCommand("action=userchanged&id=" + args.value);
                    }
                }
            },
            {
                location: 'before',
                widget: 'dxCheckBox',
                locateInMenu: 'auto',
                options: {
                    text: "Rotate Grid",
                    onValueChanged: function(args) {
                        if (args.value) {
                            $("#dataGridTable").dxTreeList("dispose");
                            $("#dataGridTable").empty();
                            initDataGrid(true);
                        } else {
                            $("#dataGridTable").dxDataGrid("dispose");
                            $("#dataGridTable").empty();
                            initDataGrid(false);
                        };
                    }
                }
            }
            //, {
            //    location: 'before',
            //    locateInMenu: 'auto',
            //    widget: 'dxButton',
            //    options: {
            //        text: resString("btnDownload"),
            //        icon: '',
            //        visible: true,
            //        elementAttr: { id: 'btnDownloadDataGrid' },
            //        onClick: function () {
            //            document.location.href = pageURL + "&id=" + selectedUserID;
            //        }
            //    }
            //}, {
            //    location: 'before',
            //    locateInMenu: 'auto',
            //    widget: 'dxButton',
            //    options: {
            //        disabled: (selectedUserID<=0),
            //        visible: true,
            //        text: resString("btnUpload"),
            //        icon: '',
            //        elementAttr: { id: 'btnUploadDataGrid' },
            //        onClick: function () {
            //                UploadDataGrid()
            //        }
            //    }
            //}
            ]
        });

        toolbar = $("#toolbar").dxToolbar("instance");
    }
    function sendCommand(params) {
        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }
    function syncError() {
        hideLoadingPanel();
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
        $(".ui-dialog").css("z-index", 9999);
    }
</script>
    <div class="table whole">
        <div class="tr" style="vertical-align: top; height: 24px;">
            <div class="td text" style="vertical-align: middle; padding-bottom: 6px;">
                <div id="toolbar" class="dxToolbar"></div>
            </div>
        </div>
        <div class="tr">
            <div class="td" id="tdContent">
                <div id="divContent" class="whole" style="text-align: left; vertical-align: top; overflow: auto;">
                    <div id="dataGridTable"></div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>