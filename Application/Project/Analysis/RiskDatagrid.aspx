<%@ Page Language="VB" Inherits="RiskDatagridPage" title="Data Grid" Codebehind="RiskDatagrid.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">
    
    var datagrid_columns = <% = GetDatagridColumns() %>;
    var datagrid_data    = <% = GetDatagridData()%>;

    var groupsUsersList = <%=getGroupsUsersList() %>;
    var selectedUserID = <% =SelectedUserID %>;

    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_CLASS = 3;
    var COL_COLUMN_TYPE = 4;
    var COL_COLUMN_DATA_FIELD = 5;

    /* jQuery Ajax */
    function syncReceived(params) {
        if ((params)) {            
            var received_data = eval(params);
            if ((received_data) && received_data.length > 2) {
                if (received_data[0] == "select_user") {
                    datagrid_columns = received_data[1];
                    datagrid_data = received_data[2];
                    initDatatable();
                }
            }
        }
        hideLoadingPanel();
    }

    function syncError() {
        hideLoadingPanel();
        dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", true, ";", undefined, "Error", 350, 280);
        $(".ui-dialog").css("z-index", 9999);
    }

    function sendCommand(params) {
        showLoadingPanel();

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }
    // end jQuery Ajax

    // Toolbar
    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: [{
                location: 'before',
                template: function() {
                    return $("<div class='nowide900'><%=ResString("lblDataGridUser")%></div>");
                }
            }, {
                location: 'before',
                widget: 'dxSelectBox',
                locateInMenu: 'auto',
                options: {
                    acceptCustomValue: false,
                    width: 340,
                    items: groupsUsersList,
                    valueExpr: "id",
                    displayExpr: "text",
                    value: selectedUserID,
                    onOpened: function (e) {
                        $(e.element).find(".dx-texteditor-input").select();
                    },
                    onValueChanged: function(e) {
                        var val = e.value;
                        if (typeof val == "undefined" || !(val)) {
                            e.component.option("value", e.previousValue);
                        } else {
                            if (selectedUserID !== val) {
                                selectedUserID = val;
                                showLoadingPanel();
                                //$("#btnUploadDataGrid").dxButton("instance").option("disabled", (val < 0));
                                sendCommand("action=select_user&id=" + val, true);
                            }
                        }
                    },
                    searchEnabled: true
                }
            }]
        });
    }
    // end Toolbar

    function initPage() {
        initToolbar();
        initDatatable();
    }

    function resizeGrid() {
        var margin = 8;
        $("#tableContent").height(200).width(300);
        var td = $("#tdContent");
        $("#tableContent").width(Math.round(td.innerWidth()) - margin);
        $("#tableContent").height(Math.round(td.innerHeight()) - margin);
    }

    // DataTables
    
    function initDatatable() {
        if ($("#tableContent").data("dxTreeList")) {
            $("#tableContent").dxTreeList("dispose");
        }

        //init columns headers         
        var columns = [];
        for (var i = 0; i < datagrid_columns.length; i++) {
            columns.push({ "caption" : datagrid_columns[i][COL_COLUMN_NAME], "alignment" : datagrid_columns[i][COL_COLUMN_CLASS], "dataType" : datagrid_columns[i][COL_COLUMN_TYPE],"allowSorting" : true, "allowSearch" : i == 0 || i == 1, "visible" : datagrid_columns[i][COL_COLUMN_VISIBILITY], "fixed" :  i < 2, "dataField" : datagrid_columns[i][COL_COLUMN_DATA_FIELD] });
        }

        $("#tableContent").dxTreeList({
            allowColumnResizing: true,
            columnFixing: {
                enabled: true
            },
            columns: columns,
            columnAutoWidth: true,
            columnFixing: {
                enabled: true
            },
            filterPanel: {
                visible: false
            },
            filterRow: {
                visible: false
            },
            headerFilter: {
                visible: true
            },
            searchPanel: {
                visible: true,
                text: "",
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            "export": {
                enabled: true
            },
            rowAlternationEnabled: true,
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            dataSource: datagrid_data,
            dataStructure: "plain",
            height: "auto",
            highlightChanges: false,
            keyExpr: "idx",
            pager: {
                allowedPageSizes: <% = _OPT_PAGINATION_PAGE_SIZES %>,
                showPageSizeSelector: true,
                showNavigationButtons: true,
                visible: datagrid_data.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            paging: {
                enabled: datagrid_data.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            showColumnLines: true,
            showRowLines: true,
            noDataText: "<% = GetEmptyMessage() %>",
            wordWrapEnabled: true
        });
                
        setTimeout(resizeGrid, 250);
    }
    // end DataGrid

    $(document).ready(function () {
        initPage();
    });

    resize_custom = resizeGrid;

</script>

<%-- Page content --%>
<table id='tblMain' style='overflow:hidden;' border='0' cellspacing='0' cellpadding='0' class='whole'>
    <tr id="trToolbar" valign="top">
        <td class='text' valign="middle" style_="padding-bottom:6px;">
            <div id="toolbar" class="dxToolbar"></div>
    </tr>
    <tr style='height:2em'><td valign="top"><h5 id='lblPageTitle' style='margin:6px 6px 0px 6px;'><%=GetTitle()%></h5></td></tr>
    <tr id='trContent' class='whole' valign='top'>
        <td id="tdContent" class="whole" colspan="2">
            <div id="divContent" class='whole'><div id='tableContent'></div></div>
        </td>
    </tr>
</table>

</asp:Content>

