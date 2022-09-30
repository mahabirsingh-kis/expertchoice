<%@ Page Language="VB" Inherits="MissingRolesReportPage" title="Participants roles for Alternatives" Codebehind="MissingRoles.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <script language="javascript" type="text/javascript">

        function resizePage() {
        }

        //resize_custom = resizePage;
        resize_custom = onResize;

        var table_main_id = "tableMissingRoles";
        var gridMissingRoles = null;
        var grid_w_old = 0;
        var grid_h_old = 0;

        function getColumns() {
            return [
                {
                    dataField: "id",
                    caption: "id",
                    width: "36px"
                },
                {
                    dataField: "Objective",
                    caption: "Objective",
                    width: "42%"
                },
                {
                    dataField: "Alternative",
                    caption: "Alternative",
                    width: "42%"
                },
                {
                    dataField: "AllowedCount",
                    caption: "Allowed",
                    dataType: "number",
                    alignment: "right",
                    visible: false
                },
                {
                    dataField: "RestrictedCount",
                    caption: "Restricted",
                    dataType: "number",
                    alignment: "right"
                },
                {
                    dataField: "EvaluatedCount",
                    caption: "Evaluated",
                    dataType: "number",
                    alignment: "right"
                }
            ];
        }

        function hasGrid() {
            return ((gridMissingRoles) && $("#gridMissingRoles").html() != "");
        }

        function resizeGrid(id) {
            //return false;
            var d = ($("body").hasClass("fullscreen") ? 20 : 10);
            $("#" + id).height(300).width(400);
            var td = $("#trMissingGrid");
            var w = $("#" + id).width(Math.round(td[0].clientWidth-d)).width();
            var h = $("#" + id).height(Math.round(td[0].clientHeight-5)).height();
            //$("#grid").css("max-width", w);
            //$("#grid").show();
            if ((grid_w_old!=w || grid_h_old!=h)) {
                grid_w_old = w;
                grid_h_old = h;
                //onSetAutoPager();
            };
        }

        function checkResize(id, w_o, h_o) {
            var w = $(window).width();
            var h = $(window).height();
            if (!w || !h || !w_o || !h_o || (w==w_o && h==h_o)) {
                resizeGrid(id);
            }
        }

        function onResize(force_redraw) {
            var w = $(window).innerWidth();
            var h = $(window).innerHeight();
            if (force_redraw) {
                grid_w_old = 0;
                grid_h_old = 0;
            }
            checkResize(table_main_id, force_redraw ? 0 : w, force_redraw ? 0 : h);
            //checkResize(table_main_id,0,0);
        }

        function initGrid() {
            if (hasGrid()) {
                $("#" + table_main_id).dxDataGrid("dispose");
                $("#" + table_main_id).html("");
            }
            var gridColumns = getColumns();
            var missingRoles = <%= GetRolesStat() %>;

            $("#" + table_main_id).dxDataGrid({
                dataSource: missingRoles,
                allowColumnReordering: true,
                allowColumnResizing: true,
                columnAutoWidth: true,
                "export": {
                    enabled: true,
                    fileName: "MissingRoles"
                },
                showBorders: true,
                columns: gridColumns,
                onInitialized: function (e) {
                    setTimeout(function () { onResize(true); }, 300);
                },
                paging: {
                    enabled: false
                },
            });

            gridMissingRoles = $("#" + table_main_id).dxDataGrid('instance');
            onResize(false);
        }

        $(document).ready(function () {
            initGrid();
        });
    </script>    
    <div style="height:95%;margin-top:5px;margin-bottom:5px;">
    <div class="table" id="tblPageMain">
        <div class="tr" id="trMissingGrid">
            <div id="tableMissingRoles"></div>
        </div>
    </div>
    </div>
</asp:Content>