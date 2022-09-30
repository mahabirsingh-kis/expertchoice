<%@ Page Language="VB" Inherits="ExportChartPage" title="Export" MasterPageFile="~/mpEmpty.master"  Codebehind="Export.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/drawMisc.js"></script>
    <script type="text/javascript" src="/Scripts/canvg.min.js"></script>
    <script type="text/javascript" src="/Scripts/svg.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    loadSVGLib();

    var cdata = <% = New DashboardWebAPI().Synthesize(1) %>;

    $(document).ready(function () {
        <% If CheckVar(_PARAM_ACTION, "") = "upload" Then %>
            setTimeout(function () {
            uploadChartPNG("<% = JS_SafeString(CheckVar("filename", "chart.png")) %>", function (fn, res) {
                if (typeof onUploadSuccess == "function") {
                    onUploadSuccess(fn, res, "<% =JS_SafeString(CheckVar("format", "")) %>");
                } else {
                    if ((window.opener) && typeof window.opener.onUploadSuccess == "function") {
                        window.opener.onUploadSuccess(fn, res);
                    } else {
                        if ((window.parent) && typeof window.parent.onUploadSuccess == "function") {
                            window.parent.onUploadSuccess(fn, res, "<% =JS_SafeString(CheckVar("format", "")) %>");
                        }
                    }
                }
            });
            }, 2500);
        <% End If %>
    });

    var options = {
        //dataSource: cdata,
        WRTNodeID: 1,
        exportMode: true,
        width: 600,
        height: 400,
        sortBy: "value", 
        debugMode: true,
        isRotated: true,
        decimals: 1,
        chartsPerPage: 1,
        singleRow: false,
        showLegend: false,
        showLabels: true
    }

    function uploadChartPNG(filename, on_upload) {
        var mCoef = 6;
        options.dataSource = cdata;
        $("#chart").ecChart(options);
        $("#chart").ecChart("export", filename, 297 * mCoef, 210 * mCoef, "PNG", false, on_upload);
    }

    function updatePager() {};
</script>

    <div id="chart">
        <h6 style="margin-bottom:2em;"><% =PageMenuItem(CurrentPageID) %></h6>
        <img src="/Images/loader.gif" width="60" />
    </div>

</asp:Content>