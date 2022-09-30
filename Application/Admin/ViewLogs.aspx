<%@ Page Language="VB" Inherits="ViewLogsPage" Title="View Logs" CodeBehind="ViewLogs.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
<script>
    var LogsDataSource = new DevExpress.data.CustomStore({});
    var workgroupsDataSource = [<%= GetWorkgroups() %>];
    var selectedWorkgroupID = Number((typeof localStorage['Logs_WID'] == "undefined" ? -1 : localStorage['Logs_WID']));
    var filterActionIDs = [<% =GetEnumIDsAsString(GetType(dbActionType)) %>];
    var filterTypeIDs = [<% =GetEnumIDsAsString(GetType(dbObjectType)) %>];

    function getWorkgroupByID(wID) {
        for (i = 1; i < workgroupsDataSource.length; i++) {
            if (workgroupsDataSource[i].ID == wID) return workgroupsDataSource[i];
        };
        return null;
    }

    function initDataSource(wkgID) {
        LogsDataSource = new DevExpress.data.CustomStore({
            //loadMode: "raw",
            //cacheRawData: false,
            key: "ID",
            load: function (loadOptions) {
                var deferred = $.Deferred();

                loadOptions["wkgid"] = wkgID;
                loadOptions["userData"] = null;
                var df = loadOptions["dataField"];
                if (typeof df !== "undefined" && df !== "DT") {
                    var filterList = [];
                    if (df == "WorkgroupID") {
                        for (i = 0; i < workgroupsDataSource.length; i++) {
                            var f = workgroupsDataSource[i];
                            filterList.push({ "WorkgroupID": f.ID });
                        };
                    };
                    if (df == "ActionID") {
                        for (i = 0; i < filterActionIDs.length; i++) {
                            filterList.push({ "ActionID": filterActionIDs[i] });
                        };
                    };
                    if (df == "TypeID") {
                        for (i = 0; i < filterTypeIDs.length; i++) {
                            filterList.push({ "TypeID": filterTypeIDs[i] });
                        };
                    };
                    deferred.resolve({ data: filterList });
                } else {
                    callAPI("manage/?<% =_PARAM_ACTION %>=logs_system", loadOptions, function (res) {
                        showBackgroundMessage("Processing data...");
                        if (isValidReply(res) && (res.Result == _res_Success)) {
                            //deferred.resolve(data, { totalCount: data.length });
                            deferred.resolve({ data: res.Data, totalCount: Number(res.ObjectID) });
                        } else {
                            showResMessage(res, true);
                        }
                    }); // , "Loading log events..."
                };
                return deferred.promise();
            },
            //onLoading: function (loadOptions) {
            //},
            onLoaded: function (request) {
                hideLoadingPanel();
            },
            errorHandler: function (error) {
                showErrorMessage(error.message, true);
            }
        });
    }

    function initGrid() {
        $("#gridLogs").dxDataGrid({
            dataSource: {
                key: "ID",
                store: LogsDataSource,
            },
            //width: "99%",
            //height: "95%",
            rowAlternationEnabled: true,
            columns: [
                {
                    dataField: "ID",
                    visible: true,
                    alignment: "right",
                    dataType: "number",
                    allowFiltering: false,
                    allowSearch: false,
                    allowHeaderFiltering: false,
                    allowSorting: true,
                }, {
                    dataField: "DT",
                    dataType: "datetime",
                    caption: "Date",
                    width: 150,
                    alignment: "center",
                    allowFiltering: true,
                    allowSearch: true,
                    allowHeaderFiltering: true,
                    allowSorting: true,
                    cellTemplate: function (element, info) {
                        if (typeof info.value != "undefined" && info.value != null) {
                            element.html(new Date(info.value).toLocaleString("en-US"));
                        }
                    }
                }, {
                    dataField: "WorkgroupID",
                    caption: "Workgroup",
                    visible: false,
                    alignment: "center",
                    allowFiltering: false,
                    allowSearch: false,
                    allowHeaderFiltering: true,
                    customizeText: function (cellInfo) {
                        var wgp = getWorkgroupByID(cellInfo.value);
                        var name = (wgp == null ? "" : wgp.name);
                        return name;
                    }
                }, {
                    dataField: "UserEmail",
                    caption: "User",
                    //cellTemplate: function (container, options) {
                    //    $("<a href='mailto:" + options.data.UserEmail + "'>" + options.data.UserEmail + "</a>").appendTo(container);
                    //},
                    width: 150,
                    alignment: "center",
                    allowFiltering: true,
                    allowSearch: true,
                    allowHeaderFiltering: false,
                }, {
                    dataField: "ActionID",
                    caption: "Action",
                    width: 100,
                    alignment: "center",
                    allowFiltering: false,
                    allowSearch: false,
                    allowHeaderFiltering: true,
                    customizeText: function (cellInfo) {
                        var newText = "";
                        if (cellInfo.value !== null) {
                            newText = resString("lblLogAction" + cellInfo.value);
                        };
                        return newText;
                    }
                }, {
                    dataField: "TypeID",
                    caption: "Type",
                    width: 100,
                    alignment: "center",
                    visible: true,
                    allowFiltering: false,
                    allowSearch: false,
                    allowHeaderFiltering: true,
                    customizeText: function (cellInfo) {
                        var newText = "";
                        if (cellInfo.value !== null) {
                            newText = resString("lblLogType" + cellInfo.value);
                        };
                        return newText;
                    }
                }, {
                    dataField: "ObjectName",
                    caption: "Object",
                    width: 100,
                    allowFiltering: true,
                    allowSearch: true,
                    allowHeaderFiltering: false,
                    customizeText: function (cellInfo) {
                        return (cellInfo.value == null || cellInfo.value == "-1") ? "" : cellInfo.value;
                    }
                }, {
                    dataField: "Comment",
                    caption: "Details",
                    width: 250,
                    allowFiltering: true,
                    allowSearch: true,
                    allowHeaderFiltering: false,
                }, {
                    dataField: "Result",
                    allowFiltering: true,
                    allowSearch: true,
                    allowHeaderFiltering: false,
                },
            ],
            remoteOperations: {
                filtering: true,
                paging: true,
                sorting: true,
                groupPaging: false,
                grouping: false,
                summary: false
            },
            pager: {
                allowedPageSizes: [20, 50, 100, 200],
                showInfo: true,
                showNavigationButtons: true,
                showPageSizeSelector: true,
                visible: true
            },
            paging: {
                enabled: true,
                pageSize: 100
            },
            scrolling: {
                //showScrollbar: "always",
                mode: "standard", // "virtual"
            },
            preloadEnabled: true,
            //rowRenderingMode: 'virtual',
            //columnRenderingMode: 'virtual',
            filterRow: {
                visible: true,
                applyFilter: "auto"
            },
            hoverStateEnabled: true,
            //columnHidingEnabled: false,
            columnAutoWidth: true,
            allowColumnResizing: true,
            allowColumnReordering: true,
            searchPanel: {
                visible: false,
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            headerFilter: {
                allowSearch: false,
                visible: true,
            },
            showBorders: false,
            //cacheEnabled: true,
            "export": {
                enabled: true,
                allowExportSelectedData: false,
                excelFilterEnabled: true,
                fileName: "Logs"
            },
            grouping: {
                //expandMode: "rowClick",
                contextMenuEnabled: false,
            },
            groupPanel: {
                visible: false,
            },
            columnFixing: {
                enabled: true
            },
            columnChooser: {
                enabled: true,
                mode: "select", //"dragAndDrop"
                title: resString("lblColumnChooser"),
                height: 400,
                width: 250,
                emptyPanelText: resString("lblColumnChooserPlace")
            },
            onContentReady: function (e) {
                hideLoadingPanel();
            },
            onInitialized: function (e) {
            },
            loadPanel: {
                enabled: false,
            },
        });
    }

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: [
                {
                    location: "before",
                    text: "Workgroup:",
                    locateInMenu: 'never',
                    visible: workgroupsDataSource.length > 1,
                },
                {
                    location: 'before',
                    locateInMenu: 'never',
                    widget: 'dxSelectBox',
                    visible: workgroupsDataSource.length > 1,
                    options: {
                        showText: true,
                        hint: "Select Workgroup",
                        focusStateEnabled: false,
                        valueExpr: "ID",
                        displayExpr: "name",
                        searchEnabled: true,
                        items: workgroupsDataSource,
                        value: selectedWorkgroupID,
                        width: "auto",
                        elementAttr: { id: 'selWorkgroup' },
                        onOpened: function (e) {
                            if (e.element) e.element.find("input[type=text]").select();
                        },
                        onClosed: function (e) {
                            if (e.component.option("text") == "") e.component.reset();
                        },
                        openOnFieldClick: true,
                        onSelectionChanged: function (e) {
                            initDataSource(e.selectedItem.ID);
                            selectedWorkgroupID = e.selectedItem.ID;
                            localStorage['Logs_WID'] = e.selectedItem.ID;
                            var dg = $("#gridLogs").dxDataGrid("instance");
                            dg.beginUpdate();
                            dg.option("dataSource.store", LogsDataSource);
                            dg.option("columns[2].visible", (e.selectedItem.ID == -1));
                            dg.endUpdate();
                        },
                        
                    }
                },
                {
                    widget: "dxButton",
                    location: "before",
                    locateInMenu: 'never',
                    options: {
                        icon: "fas fa-undo",
                        hint: "Reset",
                        text: "Reset Filters",
                        onClick: function () {
                            $("#gridLogs").dxDataGrid("clearFilter");
                        }
                    }
                }
            ]
        });
    }
    resize_custom = resizePage;

    function resizePage(force, w, h) {
        var th = $("#toolbar").height();
        var dg = $("#gridLogs").dxDataGrid("instance");
        dg.beginUpdate();
        dg.option({height: (h - th - 20), width: (w - 12)});
        dg.endUpdate();
    };

    $(document).ready(function () {
        initDataSource(selectedWorkgroupID);
        initGrid();
        initToolbar();
    });

</script>
<div id="toolbar" class="dxToolbar"></div>
<div id="gridLogs"></div>
</asp:Content>