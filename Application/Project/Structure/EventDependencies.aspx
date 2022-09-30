<%@ Page Language="VB" Inherits="EventDependenciesPage" Title="Event Dependencies" CodeBehind="EventDependencies.aspx.vb" %>

<asp:Content ID="DependenciesContent" ContentPlaceHolderID="PageContent" runat="Server">
    <style type="text/css">
        .dx-treelist-rowsview .dx-data-row .dx-cell-modified .dx-highlight-outline::after {
            border-color: transparent;
        }
    </style>
    <script language="javascript" type="text/javascript">

        var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
        var ajaxMethod = _method_POST; // don't use method GET because of caching

        var alternatives = [];

        /* Toolbar */
        var toolbarItems = [
            {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-sync", text: "", hint: "Refresh",
                    elementAttr: {id: 'btn_refresh_anon'},
                    onClick: function (e) {
                        loadAlternativesEdgesData();
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: false,
                options: {
                    icon: "fas fa-cog", text: "", hint: resString("btnRASettings"),
                    disabled: isReadOnly,
                    elementAttr: {id: 'btn_settings'},
                    onClick: function (e) {
                        settingsDialog();
                    }
                }
            }, {
                location: 'center',
                locateInMenu: 'never',
                widget: 'dxButton',
                visible: true,
                options: {
                    icon: "fas fa-infinity", text: "Check circular references", hint: "Check circular references",
                    disabled: false,
                    elementAttr: {id: 'btn_check_edges'},
                    onClick: function (e) {
                        callAPI("pm/hierarchy/?action=check_edges", { }, function (data) {
                            if (typeof data == "object") {
                                //DevExpress.ui.notify(data.message, data.type);
                                DevExpress.ui.dialog.alert(data.message, data.type);
                            }
                        }); 
                    }
                }
            }
        ];

    function updateToolbar() {     
        $("#toolbar").dxToolbar("instance").beginUpdate();

        //if ($("#btn_zoom").data("dxSelectBox")) {
        //    var btn_zoom = $("#btn_zoom").dxSelectBox("instance");
        //    btn_zoom.option("disabled", meetingState != msActive);
        //}

        $("#toolbar").dxToolbar("instance").endUpdate();
    }

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItems
        });

        $("#toolbar").css("margin-bottom", 0);

        DevExpress.config({
            floatingActionButtonConfig: {
                position: {
                    of: "#tdContent",
                    my: "right top",
                    at: "right top",
                    offset: "-25 15"
                }
            }
        });

    }
    /* end Toolbar */

    /* Settings Dialog */
    var popupSettings = null, 
        popupOptions = {
            width: 550,
            height: "auto",
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    $("<div class='dx-fieldset-header'>Header</div>"),
                    $("<div class='dx-field'></div>").append(
                        $("<div class='dx-field-label'><%=ParseString("Param")%></div>"),
                        $("<div class='dx-field-value'></div>").append(
                            $("<div id='tbParam1Value'>Value</div>"))),

                    $("<div id='btnSettingsClose' style='margin-top: 10px; margin-bottom: 10px; float: right;'></div>")
                );
            },
            showTitle: true,
            title: "Settings",
            dragEnabled: true,
            closeOnOutsideClick: true
        };

        function settingsDialog() {
            if (popupSettings) $(".popupSettings").remove();
            var $popupContainer = $("<div></div>").addClass("popupSettings").appendTo($("#popupSettings"));
            popupSettings = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
            popupSettings.show();

        }
        /* end Settings Dialog */

        /* Datagrid */
        function initDatagrid() {
            if ($("#divDatagrid").data("dxTreeList")) {
                $("#divDatagrid").dxTreeList("dispose");
            }

            var columns = [];
            
            columns.push({caption: "<%=ParseString("%%Alternative%% As %%Objective(l)%%")%>", dataField: "text", fixed: true, "minWidth" : 200 });
            var event_columns = {caption: "<%=ParseString("Target %%Alternative%%")%>", "columns": []};

            for (var i = 0; i < alternatives.length; i++) {
                event_columns.columns.push({dataField: "d" + alternatives[i].id, caption: alternatives[i].text, alignment: "center", fixed: false,
                    cellTemplate : function (element, info) {            
                        if (info.value == -1) {
                            element.css({"background-color" : "#f0f0f0", "cursor" : "not-allowed"});
                        } else {
                            element.append(info.value > 0 ? "<i class='ec-icon fas fa-check'></i>" : "<i class='ec-icon fas fa-check color_transparent'></i>");
                            element.addClass("cell_clickable").css({"vertical-align" : "middle"});
                        }
                    }
                });
            }

            columns.push(event_columns);
            
            $("#divDatagrid").dxTreeList({
                allowColumnResizing: true,
                columns: columns,
                columnAutoWidth: true,
                columnFixing: {
                    enabled: true
                },
                dataSource: alternatives,
                dataStructure: "plain",
                height: "auto",
                highlightChanges: false,
                keyExpr: "key",
                showColumnLines: true,
                showRowLines: true,
                onCellClick: function (e) {
                    if (e.column.index > 0 && (e.row) && e.row.rowType == "data") {
                        var fromNodeId = e.row.data.id;
                        var toNodeId = (e.column.dataField + "").substr(1);
                        //$(e.cellElement).find(".ec-icon")[0].className = e.value == 0 ? "ec-icon fas fa-check" : "ec-icon  fas fa-check color_transparent";
                        callAPI("pm/hierarchy/?action=set_edge", { "from_id" : fromNodeId, "to_id" : toNodeId, "mt" : e.value > 0 ? <% = CInt(ECMeasureType.mtNone)%> : <% = CInt(ECMeasureType.mtDirect)%> }, function (data) {                    
                            if (typeof data == "object") {
                                e.value  = e.value > 0 ? 0 : 1;
                                e.component.cellValue(e.row.rowIndex, e.column.dataField, e.value);
                                //$(e.cellElement).removeClass("dx-highlight-outline");
                                //var ds = e.component.getDataSource();
                                //ds.items()[e.row.rowIndex][e.column.dataField] = e.value;
                                //$(e.cellElement).find(".ec-icon")[0].className = e.value > 0 ? "ec-icon fas fa-check" : "ec-icon";
                            }
                        }, false);
                    }
                },
                onCellHoverChanged: function (e) {
                    if ((e.row) && e.rowType == "data" && e.eventType == "mouseover") {
                        var fromNodeId = e.row.data.id * 1;
                        var toNodeId = (e.column.dataField + "").substr(1) * 1;
                        var t = "";
                        if (fromNodeId !== toNodeId) {
                            for (var i = 0; i < alternatives.length; i++) {
                                if (alternatives[i].id == fromNodeId) t = alternatives[i].text + " / " + t;
                                if (alternatives[i].id == toNodeId) t += alternatives[i].text;
                            }
                        }
                        e.element.prop("title",  t);
                    }
                },
                sorting: {
                    mode: "none"
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
       
        function hotkeys(event) {
            if (!document.getElementById) return;
            if (window.event) event = window.event;
            if (event) {
                var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
                switch (code) {
                    case KEYCODE_ESCAPE:
                        
                        break;
                }
            }
        }

        document.onkeydown = hotkeys;
        
        resize_custom = resizePage;

        $(document).ready(function () {            
            initToolbar();
            loadAlternativesEdgesData();
        });

        function loadAlternativesEdgesData() {
            callAPI("pm/hierarchy/?action=get_edges", {}, onLoadAlternativesEdgesData);
        }
        
        function onLoadAlternativesEdgesData(data) {
            if (typeof data == "object") {
                alternatives = data;
                initDatagrid();
            }
        }

    </script>

    <table border='0' cellspacing="0" cellpadding="0" class="whole">
        <tr valign="top">
            <td valign="top">
                <div id="toolbar" class="dxToolbar cs-toolbar"></div>
            </td>
        </tr>
        <tr valign="top">
            <td id="tdContent" valign="top" style="overflow: hidden; text-align: left;" class="whole">
                <div id="divDatagrid" style="overflow: hidden;"></div>
            </td>
        </tr>
    </table>

    <div id="popupSettings" class="cs-popup"></div>

</asp:Content>