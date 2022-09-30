<%@ Page Language="VB" Inherits="RiskTreatmentsGridPage" title="Controls DataGrid" Codebehind="RiskTreatmentsGrid.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script language="javascript" type="text/javascript" src="/Scripts/jszip.min.js"></script>    
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
    .dx-tabpanel .dx-multiview-wrapper {
        border-bottom: 0px;
    }
    .dx-tabs {
        border:0px;
    }
    .dx-tabpanel .dx-tabs {
        background-color: transparent !important;
        border-bottom: 0px;
        border-left: 1px solid #cccccc;
    }
    .dx-tabpanel :not(.dx-tab-selected):not(.dx-state-hover).dx-tab {
        background: #eaeaea;
    }
    .dx-tabpanel .dx-tab {
        border: 1px solid #cccccc;
        border-left: 0px;
    }
    .dx-tabpanel .dx-tab-selected::before {
        border-bottom: 0px;
    }
    .dx-tab.dx-tab-selected {
        background-color: #ffffff;
        border-bottom: 2px solid #ffffff;
    }
    .dx-tab.dx-tab-selected .dx-item-content.dx-tab-content {
        font-weight: bold;
    }
    .dx-tabpanel.dx-state-focused .dx-tab-selected::after {
        border-color: #cccccc;
    }
    .dx-tab-selected::before, .dx-tab-selected::after {
        border: 0px;
    }
    .dx-tabpanel-container {
        display: none;
    }
</style>
<script language="javascript" type="text/javascript">

    var isReadOnly = <%=Bool2JS(IsReadOnly)%>;
    var ajaxMethod = _method_POST;
    var controls_type = <%=ControlType.ctCause%>;
    var only_active = true;
    var dataSource = [];

    /* Toolbar */
    var toolbarItems = [    
        {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxCheckBox',
            visible: true,
            options: {
                text: "Only Active",
                showText: true,
                hint: "Show only active <%=ParseString("%%controls%%")%>",
                disabled: false,
                value: only_active,
                elementAttr: {id: 'cbOnlyActive'},
                onValueChanged: function (e) {
                    if (only_active !== e.value) {
                        only_active = e.value;
                        getControlsData();
                    }                    
                }
            }
        },
        {
            location: 'before',
            locateInMenu: 'never',
            widget: 'dxButton',
            visible: true,
            options: {
                icon: "fas fa-sync", text: "", hint: "Refresh",
                elementAttr: {id: 'btn_refresh'},
                onClick: function (e) {
                    getControlsData();
                }
            }
        }
    ];

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItems
        });
    }
    /* end Toolbar*/

    function initTabs() {
        $("#divTabs").dxTabs({
            items: [ {"ID" : <%=ControlType.ctCause%>, "text" : "<% = ParseString("%%Controls%% for %%Objectives(l)%%")%>"}, {"ID" : <%=ControlType.ctCauseToEvent%>, "text" : "<% = ParseString("%%Controls%% for %%Vulnerabilities%%")%>"}, {"ID" : <%=ControlType.ctConsequenceToEvent%>, "text" : "<% = ParseString("%%Controls%% for %%Objectives(i)%%")%>"} ],
            keyExpr: "ID",
            selectedItemKeys: [<%=ControlType.ctCause%>],
            onSelectionChanged: function (e) {
                var new_type = e.component.option("selectedItem")["ID"];
                if (controls_type !== new_type) {
                    controls_type = new_type;
                    getControlsData();
                }
            }
        });
    }

    function initDataGrid() {
        if ($("#tableControlsDataGrid").data("dxDataGrid")) $("#tableControlsDataGrid").dxDataGrid("instance").dispose();

        $("#tableControlsDataGrid").dxDataGrid({
            allowColumnResizing: true,
            columnAutoWidth: true,  
            columnResizingMode: 'widget',
            columns: [ 
                { dataField: "ControlID", caption: "<%=ParseString("%%Control%%")%> ID", visible: false },
                { dataField: "ControlName", caption: "<%=ParseString("%%Control%%")%> Name" },
                { dataField: "ObjID", caption: controls_type == <%=ControlType.ctConsequenceToEvent%> ? "<%=ParseString("%%Objective(i)%%")%> ID" : "<%=ParseString("%%Objective(l)%%")%> ID", visible: false },
                { dataField: "ObjName", caption: controls_type == <%=ControlType.ctConsequenceToEvent%> ? "<%=ParseString("%%Objective(i)%%")%> Name" : "<%=ParseString("%%Objective(l)%%")%> Name" },
                { dataField: "EventID", caption: "<%=ParseString("%%Alternative%%")%> ID", visible: false && controls_type !== <%=ControlType.ctCause%> },
                { dataField: "EventName", caption: "<%=ParseString("%%Alternative%%")%> Name", visible: controls_type !== <%=ControlType.ctCause%> },
                { dataField: "Value", caption: "Effectiveness" },
            ],
            dataSource: dataSource,
            onContentReady: function (e) {
                setTimeout(function () {
                    resizePage(true);
                }, 100);
            },
            width: "100%",
            "export": {
                enabled: true
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "ControlsDataGrid_<% = App.ActiveProject.ID.ToString %>_" + controls_type
            },
            loadPanel: {
                enabled: false,
            }
        });
        resizePage(true);
    }
    
    function getControlsData() {
        callAPI("risk/?action=loadcontrolsdatagrid&type=" + controls_type + "&only_active=" + only_active, {}, function (res) {
            if (isValidReply(res) && res.Result == _res_Success) {
                dataSource = res.Data.rows;
                initDataGrid();
            }
        });
    }

    $(document).ready(function () {
        $("#divMainContent").css("overflow", "hidden");
        $(".mainContentContainer").css({ "overflow" : "hidden"});

        displayLoadingPanel(true);
        initTabs();
        initToolbar();
        getControlsData();
    });

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(id) {
        var d = ($("body").hasClass("fullscreen") ? 20 : 5);
        $("#" + id).height(300).width(400);
        var td = $("#trControlsDataGrid");
        var w = $("#" + id).width(Math.round(td.innerWidth() - d)).width();
        var h = $("#" + id).height(Math.round(td.innerHeight() - $("#divTabs").outerHeight())).height();
        if ((grid_w_old != w || grid_h_old != h)) {
            grid_w_old = w;
            grid_h_old = h;
        };
    }

    function checkResize(id, w_o, h_o) {
        var w = $(window).width();
        var h = $(window).height();
        if (!w || !h || !w_o || !h_o || (w == w_o && h == h_o)) {
            resizeGrid(id);
        }
    }

    function resizeDatatable(force_redraw) {
        var table_main = (($("#tableControlsDataGrid").data("dxDataGrid"))) ? $("#tableControlsDataGrid").dxDataGrid("instance") : null;
        if ((table_main) &&table_main !== null) {
            table_main.updateDimensions();
        }
        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        checkResize("tableControlsDataGrid", force_redraw ? 0 : w, force_redraw ? 0 : h);
    }

    function resizePage(force_redraw) {
        resizeDatatable(force_redraw);
    }

    resize_custom = resizePage;
    
</script>

<div class="whole">
    <div class="table" id="tblPageMain">
        <div class="tr">
            <div class="td">
                <div id='toolbar' class="dxToolbar"></div>
            </div>
       </div>
        <div class="tr whole" id="trControlsDataGrid">
            <div id="divTabs"></div>
            <div id="tableControlsDataGrid"></div>
        </div>
    </div>
</div>

</asp:Content>