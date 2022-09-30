<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.UtilityCurve" CodeBehind="UtilityCurve.ascx.vb" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%--<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>--%>
<style type='text/css'>
    .ui-slider .ui-slider-handle {
        width: 11px;
        height: 14px;
        background: url(<% =ImagePath%>slider_handler_top.gif) center no-repeat;
        border: 0px;
    }

    .ui-slider-horizontal {
        margin-top: 4px;
        height: 5px;
        background: #e0e0e0;
        padding: 0px;
    }

        .ui-slider-horizontal .ui-slider-handle {
            top: -8px;
            margin-left: -4px;
        }
</style>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.jqplot.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasOverlay.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasTextRenderer.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisLabelRenderer.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.ui.touch-punch.min.js"></script>
<!--[if lt IE 9]><script language="javascript" type="text/javascript" src="../../js/excanvas.min.js"></script><![endif]-->

<script>
    var StepsX = [<% = StepsX %>];
    var StepsY = [<% = StepsY %>];
    var XMin = <% = XMin %>;
    var XMax = <% = XMax %>;
    var UCtype = '<% = UCtype %>';
    var Decreasing = <% = Decreasing %>;
    var Curvature = <% = Curvature %>;
    var XValue = <% = JS_SafeNumber(iIf(XValueString = "", UNDEFINED_SINGLE_VALUE, XValueString)) %>;
    var PWL = '<% = PWLString %>';
    var is_manual = false;
    var optSmooth = true;
    var optShowMarker = false;
    var plot1 = null;

    $(document).ready(function () {
        drawUCChart();
    });

    function onResizeChart() {
        //var td = $get("tdContent");
        var c = $("ChartRUC");
        if (c) {
            //c.style.height = 1;
            $('#ChartRUC').empty();
            //var pnl = $get("tdInfodcos");
            //if ((pnl)) pnl.style.display = 'none';
            //var h_all = 200; // td.clientHeight;
            //var h_chart = h_all - 38;
            //if (h_chart < 200) h_chart = 200;
            //if (h_chart > 450) h_chart = 450;
            //var w_chart = Math.round(h_chart / 3 * 4);
            //c.style.height = h_chart;
            //c.style.width = w_chart;
            if ((plot1)) drawUCChart();
            //var s = $("divSlider");
            //if ((s)) s.style.width = (w_chart - 90);
            //if ((pnl)) pnl.style.display = 'block';
        }
    }

    function drawUCChart() {
        var sineRenderer = function () {
            var data = [[]];
            for (var i = 0; i < 10; i += 0.4) {
                data[0].push([i, Math.sin(i)]);
            }
            return data;
        };
        var UCRenderer = function () {
            var data = [[]];
            if (UCtype == "Step") {
                var Shift = (XMax - XMin) / 100;
                data[0].push([XMin, StepsY[0] * 100]);
                for (var i = 0; i < StepsX.length - 1; i++) {
                    data[0].push([StepsX[i], StepsY[i] * 100]);
                };
                data[0].push([XMax, StepsY[StepsY.length - 1] * 100]);
            } else {
                var Shift = (XMax - XMin) / 10;
                for (var i = 0; i < 11; i++) {
                    data[0].push([XMin + i * Shift, UCFunction(XMin + i * Shift) * 100]);
                };
            };


            return data;
        };

        // we have an empty data array here, but use the "dataRenderer"
        // option to tell the plot to get data from our renderer.
        plot1 = $.jqplot('ChartRUC', [], {
            dataRenderer: UCRenderer,
            seriesDefaults: {
                showMarker: optShowMarker,
                rendererOptions: {
                    smooth: optSmooth
                }
            },
            axes: {
                xaxis: {
                    min: XMin,
                    max: XMax,
                    label: 'Data'
                },
                yaxis: {
                    min: 0,
                    max: 100,
                    label: "<%=YAxisCaption%>",
                    labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                    tickOptions: {
                        suffix: ' %',
                        formatString: "%#.1f"
                    }
                }
            },
            canvasOverlay: {
                show: true,
                objects: [
                    {
                        verticalLine: {
                            name: 'xValLine',
                            x: XValue,
                            lineWidth: 3,
                            yOffset: 0,
                            lineCap: 'butt',
                            color: 'rgb(89, 198, 154)',
                            shadow: false
                        }
                    }
                ]
            }
        });
    };

</script>
<%--<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>--%>

<asp:Label runat="server" ID="lblMessage" Visible="false" CssClass="text error" />
            <div runat="server" id="pnlRUC" style="text-align: center">
                <div>
                    <div id="ChartRUC" style="width: 450px; height: 350px"></div>
                    <div id="divSlider" style="margin-left: 45px; margin-right: 10px;"></div>
                </div>
                <div style='text-align: center; margin-top: 5px;'>
                    <asp:Image runat="server" ID="imgScaleInfodoc" SkinID="InfoIcon15" Visible="false" />
                    <input type='text' class='as_number' id="SliderValue" name="SliderValue" value="<% =XValueString %>" onkeyup='onChangeText();' onmousedown='focusOnClick(this);' style='width: 6em; margin-right: 6px' /><a href='' onclick='eraseValue(); return false;'><img src='<% =ImagePath %>nodelete_tiny.gif' id='imgErase' width='10' height='10' title='Erase judgment' border='0'></a>
                </div>
                <input type='hidden' name='UCValue' value='<% =XValue %>' /><telerik:RadToolTip runat="server" ID="tooltipScaleInfodoc" Visible="false" TargetControlID="imgScaleInfodoc" EnableEmbeddedBaseStylesheet="true" HideEvent="ManualClose" Position="BottomLeft" RelativeTo="Element" ShowEvent="OnMouseOver" IsClientID="false" OnClientBeforeShow="OnToolTipShowHandler" />
            </div>
