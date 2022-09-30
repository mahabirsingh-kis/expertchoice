<%@ Page Language="VB" Inherits="StatisticPage" title="Statistic" Codebehind="Statistic.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    var wg_id = <% =CurrentWkgID %>;
    var dataset = [];

    function loadData() {
        callAjax("<% =_PARAM_ACTION %>=load&wkg=" + wg_id, onLoadData, _method_POST);
    }

    function onLoadData(res) {
        if (isValidReply(res) && res.Result == _res_Success) {
            dataset = res.Data;
            initGrid();
            if (res.Message != "") $("#divSummary").html(res.Message);
            if (res.URL != "") $("#divTitle").html(res.URL);
        }
    }

    function onSetWkg(id) {
        wg_id = id;
        loadData();
        return false;
    }

    function initGrid() {
        if ((dataset) && dataset != []) {
            showLoadingPanel();
            if ($("#divGrid").data("dxDataGrid")) {
                $("#divGrid").dxDataGrid("instance").option("dataSource", dataset);
            } else {
                $("#divGrid").dxDataGrid({
                    dataSource: dataset,
                    keyExpr: "idx",
                    columns: [{
                        dataField: "idx",
                        caption: "#",
                        //width: 70,
                        sortIndex: 0,
                    }, {
                        dataField: "name",
                        dataType: "string",
                        //width: 250,
                        caption: "Counter"
                    }, {
                        dataField: "1m",
                        dataType: "number",
                        caption: "1 month"
                    }, {
                        dataField: "3m",
                        dataType: "number",
                        caption: "3 months"
                    }, {
                        dataField: "6m",
                        dataType: "number",
                        caption: "6 months"
                    }, {
                        dataField: "1y",
                        dataType: "number",
                        caption: "1 year"
                    }, {
                        dataField: "all",
                        dataType: "number",
                        caption: "All time"
                    }],
                    showBorders: true,
                    allowColumnReordering: true,
                    rowAlternationEnabled: true,
                    hoverStateEnabled: true,
                    columnAutoWidth: true,
                    allowColumnResizing: true,
                    allowColumnReordering: true,
                    headerFilter: {
                        allowSearch: true,
                        visible: true
                    },
                    columnChooser: {
                        enabled: true,
                        mode: "select",
                        title: resString("lblColumnChooser"),
                        height: 400,
                        width: 250,
                        emptyPanelText: resString("lblColumnChooserPlace")
                    },
                    searchPanel: {
                        visible: true,
                        width: 240,
                        searchVisibleColumnsOnly: true,
                        placeholder: resString("btnDoSearch") + "..."
                    },
                    export: {
                        enabled: true
                    },
                    onContentReady: function (e) {
                        hideLoadingPanel();
                    }
                    //loadPanel: main_loadPanel_options,
                });        
            }
        }
    }

    $(document).ready(function () {
        loadData();
    });

</script>
<h3 style="margin-top:1ex" id="divTitle"><% =PageTitle(CurrentPageID)%></h3>
<div style="text-align:center" class="text">
    <% =GetWorkgroups%>
    <div style="max-width:800px; margin:1em auto 1em auto"><div id="divGrid"><div style="margin:3em auto 3em auto">Loading...</div></div></div>
    <div id="divSummary"></div>
</div>
</asp:Content>