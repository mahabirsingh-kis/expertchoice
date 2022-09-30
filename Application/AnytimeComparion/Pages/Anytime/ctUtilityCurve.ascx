<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="ctUtilityCurve.ascx.vb" Inherits=".ctUtilityCurve" %>
<%@ Reference Control="~/ctrlEvaluationControlBase.ascx" %>
<%@ Register TagPrefix="EC" TagName="FramedInfodoc" Src="~/ctrlFramedInfodoc.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%--<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>--%>
<style type='text/css'>
    .ui-slider .ui-slider-handle {
        width: 11px;
        height: 14px;
        /*background: url(../../../App_Themes/EC2018/images/slider_handler_top.gif) center no-repeat;*/
        border: 0px;
        cursor: e-resize;
    }

   .ui-slider-horizontal {
    margin-top: 4px;
    height: 10px;
    background: #e0e0e0;
    padding: 0px;
    margin-bottom: 15px;
}

        .ui-slider-horizontal .ui-slider-handle {
            top: -8px;
            margin-left: 0px;
        }

    .empty_space {
        margin-top: 90px;
    }

    @media (max-width:992px) {
        .empty_space {
            margin-top: 20px;
        }
    }
    #lblTask {
        display: block;
        margin: 0 !important;
        padding: 0 !important;
    }
        .tooltip_item {
        position: absolute;
        width: max-content;
        bottom: 30px;
        font-size: 11px !important;
        background: var(--White);
        text-decoration: none;
        padding: 5px 8px;
        border: 1px solid var(--bs-gray);
        border-radius: 5px;
        margin: 0;
        transform: scale(0) translateX(calc(-50% + -15px));
        transition: all .5s;
        color: var(--Black);
    }
    .tooltip_item:after {
        content: "";
        width: 10px;
        height: 10px;
        background: var(--White);
        border-left: 1px solid var(--bs-gray);
        border-bottom: 1px solid var(--bs-gray);
        border-right: 1px solid transparent;
        border-top: 1px solid transparent;
        left: calc(50% - 0px);
        position: absolute;
        bottom: -5px;
        transform: rotate(-45deg);
    }
    .newclass {
        position: relative;
    }

    .newclass span.tooltip_item {
        text-decoration: none;
        /*text-transform: none;*/
    }

    .chart_wrapper_outer .close_icon span {
        display: inherit;
    }

    .newclass:hover span.tooltip_item {
        transform: scale(1) translateX(calc(-50% + -15px));
    }
</style>

<script src="../../../Scripts/jquery.jqplot.min.js"></script>
<script src="../../../Scripts/jqplot.canvasOverlay.min.js"></script>
<script src="../../../Scripts/jqplot.canvasTextRenderer.min.js"></script>
<script src="../../../Scripts/jqplot.canvasAxisLabelRenderer.min.js"></script>
<script src="../../../Scripts/jquery.ui.touch-punch.min.js"></script>
<script src="../../CustomScripts/utilitycurve.js"></script>
<script src="https://cdn.jsdelivr.net/npm/chart.js@2.9.4/dist/Chart.min.js"></script>
<%--<script src="../../CustomScripts/anytime.js"></script>--%>

<%--<% If ShowFramedInfodocs Then Response.Write(frmInfodocGoal.Script_Init(sRootPath, AllowSaveSize, SaveSizeMessage))%>--%>

<asp:HiddenField ID="hdnXMin" runat="server" Value="0" />
<asp:HiddenField ID="hdnXMax" runat="server" Value="0" />
<asp:HiddenField ID="hdnDecreasing" runat="server" Value="0" />
<asp:HiddenField ID="hdnCurvature" runat="server" Value="0" />
<asp:HiddenField ID="hdnXValue" runat="server" Value="0" />

<script>
    $(document).ready(function () {
        setTimeout(function () {
            var phtml = $('.removeptagd').html().replaceAll('<p></p>', '');

            $('.removeptagd').html('');
            $('.removeptagd').html(phtml);

            var whtml = $('.removeptagw').html().replaceAll('<p></p>', '');

            $('.removeptagw').html('');
            $('.removeptagw').html(whtml);
        }, 500);
    });
</script>

<script type="text/javascript"><!--
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
    var img_path = "../../../App_Themes/EC09/images/"
    var img_delete = new Image; img_delete.src = img_path + 'delete_tiny.gif';
    var img_delete_ = new Image; img_delete_.src = img_path + 'nodelete_tiny.gif';
    var output = eval("(" + $('#MainContent_hdnOutput').val() + ")");
    var label = output.child_node + " for " + output.parent_node;

    $('#dvUtilityfunction').css('display', 'none');
    $('#dvStepfunction').css('display', 'none');

    var UCFunction = function (x) {
        if (UCtype == "Step") {
            if (x < StepsX[0]) { return Number(StepsY[0]) };
            if (x >= StepsX[StepsX.length - 1]) { return Number(StepsY[StepsY.length - 1]) };
            for (var i = 0; i < StepsX.length - 1; i++) {
                if ((StepsX[i] <= x) && (StepsX[i + 1] > x)) {
                    if (PWL == "1") {
                        var k = (StepsY[i + 1] - StepsY[i]) / (StepsX[i + 1] - StepsX[i]);
                        var b = StepsY[i] - k * StepsX[i];
                        return (k * x + b);
                    } else {
                        return StepsY[i]
                    };
                };
            };
        } else {
            if (XMin >= XMax) { return 0 };
            var n = new Number(0.0);
            if (!Decreasing) {
                if (x < XMin) { return 0 };
                if (x > XMax) { return 1 };
                n = (XMax - XMin) * Curvature;
                if (n == 0) { return (x - XMin) / (XMax - XMin) }
                else { return (1 - Math.exp(-(x - XMin) / n)) / (1 - Math.exp(-(XMax - XMin) / n)) };
            }
            else {
                if (x < XMin) { return 1 };
                if (x > XMax) { return 0 };
                n = (XMax - XMin) * Curvature;
                if (n == 0) { return (XMax - x) / (XMax - XMin) }
                else { return (1 - Math.exp(-(XMax - x) / n)) / (1 - Math.exp(-(XMax - XMin) / n)) };
            };
        };
    };

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
                    gridThickness: 0,
                    label: "<%=YAxisCaption%>",
                    labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                    tickOptions: {
                        suffix: ' %',
                        formatString: "%#.1f"
                    },
                    stripLines: [
                        {
                            value: 0,
                            showOnTop: true,
                            color: "gray",
                            thickness: 2
                        }
                    ]
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


    function onSliderChange(event, ui) {
        // if (!is_manual) {    
        var tbVal = $('#SliderValue');
        if ((ui) && (tbVal)) {
            $('#SliderValue').val(ui.value);
            onChangeValue();
            setTimeout(function () {
                save_utility_curve(8, null, 0, '<% =UCType %>')
            }, 500);
        }
        //  }
    }



    function eraseValue() {
        if (UCtype == "Step")
            $('#steps-functionInput').val("");
        else
            $('#uCurveInput').val("");
        if (output.pipeOptions.dontAllowMissingJudgment) {
            EnableDisablePagination(true);
        }
        onChangeText();

        $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
        $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
    }

    var stepChart = null;
    var chart = null;
    function onChangeText() {
        if (UCtype == "Step") {
            var val = $('#steps-functionInput').val();
            if (!isNaN(val)) {
                //if (val != null) {
                val = (val == null ? -2147483648000 : parseFloat(val));
                if (isNaN(val))
                    val = -2147483648000;
                is_erase = (val == -2147483648000);
                step_input = val;
                isValueChanged = output.current_value != val;
                //console.log("val: " + val + ", is_erase: " + is_erase);
                setTimeout(function () {
                    disableNext = output.pipeOptions.dontAllowMissingJudgment && is_erase;
                    output.IsUndefined = is_erase;
                    output.IsUndefined = is_erase;
                }, 50);

                if (val < output.PipeParameters.min) val = output.PipeParameters.min;
                if (val > output.PipeParameters.max) val = output.PipeParameters.max;

                if (val < lowest_value) {
                    var min = $(".tt-steps-slider").slider("option", "min");
                    $(".tt-steps-slider").slider("value", min);
                }

                $(".tt-steps-slider").slider("value", val);
                var stepLabel = getXandYofPriority(val, stepChart.data.datasets[0].data);

                stepChart.options.priorityLabel[0].x = val == 0 ? 0.00000001 : val;
                stepChart.options.priorityLabel[0].y = stepLabel[0];
                stepChart.options.priorityLabel[0].reverse = stepLabel[1];
                stepChart.options.priorityLabel[0].bottom = stepLabel[2];
                stepChart.options.priorityLabel[0].text = (stepLabel[0]).toFixed(2) + "%";

                if (output.pipeOptions.dontAllowMissingJudgment) {
                    if (is_erase) {
                        EnableDisablePagination(true);
                    }
                    else { EnableDisablePagination(true, false); }

                }
                if (is_erase) {
                    $(".next_step").removeClass("back-pulse");
                    step_input = "";
                    $("#steps-functionInput").val("");
                    stepChart.options.priorityLabel[0].text = "";
                    set_slider_gray(".tt-steps-slider");
                } else {
                    stepChart.options.priorityLabel[0].text = stepLabel[0].toFixed(2) + "%";
                    set_slider_blue(".tt-steps-slider");
                    $(".next_step").addClass("back-pulse");
                }
                if ($('#steps-functionInput').val() == '') {
                    $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
                }
                else{
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');;
                }
                //else {
                //    $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                //    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
                //}

                stepChart.update();
            }
        }
        else {
            var value = $('#uCurveInput').val();
            if (value != "-") {
                value = (value == null || value == "NaN" ? -2147483648000 : parseFloat(value));


                if (isNaN(value))
                    value = -2147483648000;

                is_erase = (value == -2147483648000);
                utilityCurveInput = value;
                isValueChanged = output.UCData.XValue != value;

                if (value < output.UCData.XMinValue) value = output.UCData.XMinValue;
                if (value > output.UCData.XMaxValue) value = output.UCData.XMaxValue;
                //$(".load-canvas-gif").show();
                //var loadingMessage = stringResources.yellowLoadingIcon.save;

                setTimeout(function () {
                    disableNext = output.pipeOptions.dontAllowMissingJudgment && is_erase;
                    output.IsUndefined = is_erase;
                    output.IsUndefined = is_erase;
                }, 50);

                if (is_erase) {
                    chart.options.ucpriorityLabel[0].text = "";
                    utilityCurveInput = "";
                    chart.update();
                }
                else {
                    var steplabel = getXandYofPriority(value, chart.data.datasets[0].data);
                    steplabel[0] = (UCFunction(value) * 100);
                    chart.options.ucpriorityLabel[0].x = value;
                    chart.options.ucpriorityLabel[0].y = value >= output.UCData.XMaxValue ? 0.1 : steplabel[0];
                    chart.options.ucpriorityLabel[0].reverse = steplabel[1];
                    chart.options.ucpriorityLabel[0].bottom = steplabel[2];
                    chart.options.ucpriorityLabel[0].text = (steplabel[0]).toFixed(2) + "%";
                    chart.update();
                }

                $(".dsBarClr").slider({ value: value });
                var handlePos = $(".tt-utility-curve-wrap .ui-slider-handle").position();
                var dragHandle = $(".tt-utility-curve-wrap .ui-slider-handle");

                if (is_erase) {
                    $("#uCurveInput").val("");
                    set_slider_gray("#sliderCurve");
                }
                else {
                    set_slider_blue("#sliderCurve");
                }

                $("#sliderCurve .ui-slider-handle").fadeIn();

                if (output.pipeOptions.dontAllowMissingJudgment) {
                    if (is_erase) {
                        EnableDisablePagination(true);
                    }
                    else { EnableDisablePagination(true, false); }

                }
                if (is_erase) {
                    $(".next_step").removeClass("back-pulse");
                } else {
                    $(".next_step").addClass("back-pulse");
                }

                if ($('#uCurveInput').val() != '') {
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
                }
                else {
                    $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
                }

                //angular.element(".load-canvas-gif").hide();
            }
        }
        save_utility_curve(8, null, 0, UCtype)
    <%--    save_utility_curve(8, null, 0, '<% =UCType %>')--%>
    }

    var lowest_value = -999999999999;
    $(document).ready(function () {
        //  setTimeout(function () {      
        var step_input = XValue; // <%= IIf(XValueString = "0", "", XValueString) %>; 
        var sessionValue = '<%=Session("XValue") %>';
        UCtype = output.non_pw_type == 'mtStep' ? 'Step' : '';
        setTimeout(function () {
            if (output.pipeOptions.dontAllowMissingJudgment) { if (XValue > 0 || XValue < 0) { EnableDisablePagination(true, false); } else { EnableDisablePagination(true); } }
        },
            1500);
        if (UCtype == "Step") {
            $('#dvUtilityfunction').css('display', 'none');
            var minE = output.PipeParameters.min;
            var maxE = output.PipeParameters.max;
            var stepX = output.step_intervals[0];
            var stepY = output.step_intervals[1];
            render_step_function(minE, maxE, stepX, stepY);

            updateSliderSteps(0, minE, maxE);
            $('#dvStepfunction').css('display', 'block');
            dragHandle = $(".tt-steps-slider .ui-slider-handle");
            setTimeout(function () {
                if (output.current_value < lowest_value) {
                    $("#steps-functionInput").val("");
                    set_slider_gray(".tt-steps-slider");
                    $(".tt-steps-slider .ui-slider-handle").addClass("back-pulse");
                    setTimeout(function () {
                        $(".tt-steps-slider .ui-slider-handle").removeClass("back-pulse");
                    },
                        1500);
                    $("#stepsFunctionSlider").slider("option", "value", minE);
                }

                if (output.current_value >= lowest_value) {
                    //resizeStepFunctionHandle($dragHandle);
                    set_slider_blue(".tt-steps-slider");
                    $("#stepsFunctionSlider").slider("option", "value", step_input);
                    $("#steps-functionInput").val(step_input);
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
                }
                if (XValue < lowest_value || sessionValue == '') {
                    $("#steps-functionInput").val("");
                    $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
                    eraseValue();
                }
                else {
                    $("#steps-functionInput").val(XValue);
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)')
                }
                $(".ui-slider-handle").removeClass('ui-state-default');
                $(".ui-slider-handle").height($('#sFunctionCanvas').height() - 60);

                $(".tt-steps-slider .ui-slider-handle").fadeIn();
                var handlePos = $(".tt-steps-slider .ui-slider-handle").position();
                var dragHandle = $(".tt-steps-slider .ui-slider-handle");
                resizeStepFunctionHandle(dragHandle, handlePos);
                execute_onresize();

            }, 1000);

        } else {
            $('#dvStepfunction').css('display', 'none');
            render_utility_curve();
            setTimeout(function () {

                var sessionValue = '<%=Session("XValue") %>';
                  $(".ui-slider-handle").removeClass('ui-state-default');
                $(".ui-slider-handle").height(chart.chart.height - 63);
                if (output.isPipeViewOnly) {
                    $('#sliderCurve').css('pointer-events', 'none');
                }
                if (XValue < lowest_value && sessionValue == '') {
                    $("#steps-functionInput").val("");
                    $('.imgClose').c$('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)'); eraseValue();
                }
                else {
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                }
            }, 200);
        }
        //   }, 1000);
    });
//-->
</script>

<script>
    var chart = null;

    function getXandYofPriority(val, dataset) {
        //if (val < lowests_value || val === "") {

        //    return ["", false, false];
        //}     
        var firstData;
        var secondData;
        //console.log(dataset);
        var lowestY = 100;
        var highestY = 0;
        var lowestX = Number.MAX_VALUE; // XMin; // Number.MAX_VALUE;
        var highestX = Number.MIN_VALUE;
        var reverse = false;

        for (i = 0; i < dataset.length; i++) {

            if (dataset[i].y < lowestY)
                lowestY = dataset[i].y;
            if (dataset[i].y > highestY)
                highestY = dataset[i].y;
            if (dataset[i].x < lowestX)
                lowestX = parseFloat(dataset[i].x);
            if (dataset[i].x > lowestX)
                highestX = parseFloat(dataset[i].x);

            if (secondData == null || typeof secondData == 'undefined') {
                if (firstData != undefined) {
                    if ((parseFloat(val)).toFixed(2) <= parseFloat(dataset[i].x)) {
                        secondData = dataset[i];
                    }
                    else {
                        firstData = dataset[i];
                    }
                } else {
                    if (parseFloat(dataset[i].x) <= parseFloat(val)) {
                        firstData = dataset[i];
                    }
                    else {
                        firstData = dataset[0];
                        secondData = dataset[0];
                    }
                }
            }
        }

        if (firstData != undefined && secondData != undefined) {
            if (firstData.x > secondData.x) {
                var tempFirstData = firstData;
                firstData = secondData;
                secondData = tempFirstData;
            }
        }

        var temp = secondData.x - firstData.x;
        var tVal = val - firstData.x;
        var percentage = tVal / temp;
        var xAndy = firstData.y + ((secondData.y - firstData.y) * percentage);

        if (parseFloat(secondData.y) < parseFloat(firstData.y)) {
            xAndy = secondData.y + ((firstData.y - secondData.y) * (1 - percentage));
            reverse = false;
        }

        var bottom = false;
        // console.log(lowestX);
        if (highestY <= xAndy + (highestY * 0.4)) {
            bottom = true;
        }

        if (highestY <= xAndy + (highestY * 0.4))
            reverse = false;

        if (((highestX * 0.2) + lowestX) > val) {
            reverse = true;
            //console.log([val, xAndy, reverse, bottom]);
        }

        return [xAndy, reverse, bottom];
    }


    function render_utility_curve() {
        utilityCurveInput = (parseFloat(output.UCData.XValue.toFixed(2)));
        var XValue = utilityCurveInput;

        if (utilityCurveInput < XMin) XValue = XMin;
        if (utilityCurveInput > XMax) XValue = XMax;
        var uc_points = [];

        var Shift = (XMax - XMin) / 10;

        for (var i = 0; i < 11; i++) {
            var x_point = (XMin + i * Shift);
            var y_point = UCFunction(XMin + i * Shift) * 100;
            uc_points.push({ x: x_point, y: y_point });
        };

        var label = output.child_node + " for " + output.parent_node;

        var canvasData = getXandYofPriority(XValue, uc_points);
        //console.log("============");
        if (canvasData[0] != "") {
            canvasData[0] = (UCFunction(XValue) * 100);
            canvasData[0] = (canvasData[0]).toFixed(2);
        }

        if (utilityCurveInput == -2147483648000) {
            $("#uCurveInput").val("");
        }

        $('#dvUtilityfunction').css('display', 'block');

        var min = output.UCData.XMinValue;
        var max = output.UCData.XMaxValue;
        var value = utilityCurveInput;
        $(".tt-curve-slider").slider({
            step: 0.01,
            value: value,
            min: min,
            max: max,
            //animate: "fast",
            //disabled: $(this).attr("data-disable"),
            slide: function (event, ui) {

                var val = ui.value;
                var handlePos = $('.tt-utility-curve-wrap .ui-slider-handle').position();
                var dragHandle = $('.tt-utility-curve-wrap .ui-slider-handle');
                $("#uCurveInput").val(val);
                if ($("#uCurveInput").val() != '' && val >= 0) {
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
                }
                else {
                    $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
                }
                resizeCurveHandle(dragHandle, handlePos);

                var steplabel = getXandYofPriority(val, chart.data.datasets[0].data);
                if (parseInt(steplabel[0]) == 0) {
                    onChangeText();
                }
                else {
                    steplabel[0] = (UCFunction(val) * 100);
                    chart.options.ucpriorityLabel[0].x = parseFloat(val);
                    chart.options.ucpriorityLabel[0].y = steplabel[0];
                    chart.options.ucpriorityLabel[0].reverse = steplabel[1];
                    chart.options.ucpriorityLabel[0].bottom = steplabel[2];
                    chart.options.ucpriorityLabel[0].text = (steplabel[0]).toFixed(2) + "%";
                    chart.update();
                }
                set_slider_blue('#sliderCurve');
            },
            stop: function (event, ui) {
                $("#uCurveInput").val(ui.value);
                if (output.pipeOptions.dontAllowMissingJudgment) {
                    if (ui.value >= 0) { EnableDisablePagination(true, false); } else { EnableDisablePagination(true); }
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
                }
                save_utility_curve(8, null, 0, UCtype)
            }
        });

        var priorityLabel = canvasData[0] + "%";

        if (utilityCurveInput == -2147483648000) {
            priorityLabel = "";
        }

        Chart.pluginService.register({
            beforeDraw: function (chart, easing) {
                if (chart.config.options.chartArea && chart.config.options.chartArea.backgroundColor) {
                    var helpers = Chart.helpers;
                    var ctx = chart.chart.ctx;
                    var chartArea = chart.chartArea;

                    ctx.save();
                    ctx.fillStyle = chart.config.options.chartArea.backgroundColor;
                    ctx.fillRect(chartArea.left, chartArea.top, chartArea.right - chartArea.left, chartArea.bottom - chartArea.top);
                    ctx.restore();
                }
            }
        });

        UCInitPriorityLabel();
        var data = {
            datasets: [{
                label: label,
                borderColor: "#0058a3",
                fill: false,
                data: uc_points,
                pointBackgroundColor: "#ecfaff"
            }]
        };

        var options = {
            responsive: true,
            //maintainAspectRatio: false,
            scales: {
                xAxes: [{
                    type: "linear",
                    position: "bottom",
                    scaleLabel: {
                        display: true,
                        labelString: "Data",
                        fontSize: 16
                    },
                    ticks: {
                        min: XMin,
                        max: XMax,
                        callback: function (label, index, labels) {

                            if (is_float(label)) {
                                label = parseFloat(label).toFixed(2);
                            }
                            return label;
                        }
                    }
                }],
                yAxes: [{

                    scaleLabel: {
                        display: true,
                        labelString: "Priority",
                        fontSize: 16
                    },
                    ticks: {
                        callback: function (label, index, labels) {
                            if (is_float(label)) {
                                label = parseFloat(label).toFixed(2);
                            }
                            return label + ".00%";
                        }
                    }
                }]
            },
            //chartArea: {
            //    backgroundColor: 'white'
            //},
            ucpriorityLabel: [{
                "x": XValue,
                "y": XValue >= output.UCData.XMaxValue ? 0.1 : canvasData[0],
                "style": "white",
                "text": priorityLabel,
                "reverse": canvasData[1],
                "bottom": canvasData[2],
                "glowonce": true
            }]
        };   
        var canvas = document.getElementById("uCurve");
        var ctx = canvas.getContext("2d");
        //new Chart(ctx, options);
        Chart.defaults.global.legend.display = false; //remove label at the top of graph
        Chart.defaults.global.tooltips.enabled = false;
        var newchart = Chart.Line(ctx, {
            data: data,
            options: options
        });

        chart = newchart;
        chart.update();
        //console.log(chart);
        setTimeout(function () {
            resize_UC_canvas();
            //$(".tt-utility-curve-wrap .ui-slider-handle").hide();
        }, 10);


        $(".tt-curve-slider").slider("value", XValue);
        //     $("#sliderCurve .ui-slider-handle").           resizeCurveHandle($dragHandle, $handlePos);
        var sessionValue = '<%=Session("XValue") %>';
        
        if (utilityCurveInput == -2147483648000) {
            $("#uCurveInput").val("");
            $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
            priorityLabel = "";
        }
        if (utilityCurveInput < XMin || sessionValue == '') {
            set_slider_gray("#sliderCurve");
            $("#sliderCurve .ui-slider-handle").addClass("back-pulse");

            $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');

            setTimeout(function () {
                if (utilityCurveInput == -2147483648000 || (utilityCurveInput < XMin && sessionValue == '')) {
                    $("#sliderCurve .ui-slider-handle").addClass("back-pulse");

                    $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
                    $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
                }
                else if (utilityCurveInput < XMin) {
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $("#uCurveInput").val(utilityCurveInput);
                }
                else {
                    $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                    $("#uCurveInput").val(utilityCurveInput);
                    $("#sliderCurve .ui-slider-handle").removeClass("back-pulse");
                }
            }, 1000);
        }
        else {
            $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
            set_slider_blue("#sliderCurve .ui-slider-handle");
        }

        $("#sliderCurve .ui-slider-handle").fadeIn();
        chart.options.ucpriorityLabel[0].text = priorityLabel;
        //console.log(canvasData[0] == "" ? + "" : canvasData[0] + " %");
        chart.update();


        $(".load-canvas-gif").hide();
        $(".tt-utility-curve-wrap").show();


    }

    var fUCPriorityloaded = false;
    function UCInitPriorityLabel() {
        if (!fUCPriorityloaded) {
            fUCPriorityloaded = true;
            var ucPriorityLabelPlugin = {
                afterDraw: function (chartInstance) {
                    var yScale = chartInstance.scales["y-axis-0"];
                    var xScale = chartInstance.scales["x-axis-0"];
                    var canvas = chartInstance.chart;
                    var ctx = canvas.ctx;
                    var index;
                    var line;
                    var style;
                    //ctx.shadowColor = "rgba(240, 36, 36, 0.5)"; // string
                    //Color of the shadow;  RGB, RGBA, HSL, HEX, and other inputs are valid.
                    //ctx.shadowOffsetX = 3; // integer
                    //Horizontal distance of the shadow, in relation to the text.
                    //ctx.shadowOffsetY = 3; // integer
                    //Vertical distance of the shadow, in relation to the text.
                    //ctx.shadowBlur = 20; // integer
                    ctx.font = "bold 13pt Arial";
                    if (chartInstance.options.ucpriorityLabel) {
                        for (index = 0; index < chartInstance.options.ucpriorityLabel.length; index++) {
                            line = chartInstance.options.ucpriorityLabel[index];

                            if (!line.style) {
                                style = "rgb(231, 76, 60)";
                            } else {
                                style = line.style;
                            }

                            if (line.y) {
                                yValue = yScale.getPixelForValue(line.y);
                            } else {
                                yValue = 0;
                            }

                            if (line.x) {
                                xValue = xScale.getPixelForValue(line.x);
                            } else {
                                xValue = 0;
                            }

                            ctx.lineWidth = 3;

                            if (yValue) {
                                //ctx.beginPath();
                                //ctx.moveTo(0, yValue);
                                //ctx.lineTo(canvas.width, yValue);
                                //ctx.strokeStyle = style;
                                //ctx.stroke();
                            }

                            if (line.text) {
                                //console.log([xValue,yValue]);

                                ctx.fillStyle = style;
                                if (line.reverse) {
                                    if (line.bottom) {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue + 5, yValue + ctx.lineWidth, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth + 20);
                                    }
                                    else {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue + 5, yValue + ctx.lineWidth - 40, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth - 20);

                                    }

                                }
                                if (!line.reverse) {
                                    if (line.bottom) {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue - 75, yValue + ctx.lineWidth, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue - 70, yValue + ctx.lineWidth + 20);

                                    }
                                    else {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue - 75, yValue + ctx.lineWidth - 40, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue - 70, yValue + ctx.lineWidth - 20);

                                    }
                                }


                                ctx.shadowColor = "white"; // string
                                //Color of the shadow;  RGB, RGBA, HSL, HEX, and other inputs are valid.
                                ctx.shadowOffsetX = 0; // integer
                                //Horizontal distance of the shadow, in relation to the text.
                                ctx.shadowOffsetY = 0; // integer
                                //Vertical distance of the shadow, in relation to the text.
                                ctx.shadowBlur = 0; // integer

                            }


                        }
                        return;
                    };
                }
            };
            Chart.pluginService.register(ucPriorityLabelPlugin);
        }

    }

    function is_float(n) {
        return Number(n) === n && n % 1 !== 0;
    }


    function resize_UC_canvas() {
        var newchart = chart;
        canvas_width = newchart.chartArea.right - newchart.chartArea.left - 3; //3 px for allowance to the right
        left_margin = newchart.chartArea.left-1;
        canvas_height = newchart.chart.height;
        $("#sliderCurve").attr("style", "width:" + canvas_width + "px !important; margin-left:" + left_margin + "px");

        var handlePos = $(".tt-utility-curve-wrap .ui-slider-handle").position();
        var dragHandle = $(".tt-utility-curve-wrap .ui-slider-handle");
        resizeCurveHandle(dragHandle, handlePos);
    }

    function resizeCurveHandle(dragHandle) {
        var canvasHeight = $('#uCurve').height() + $('#sliderCurve').height();
        var startHeight = 291;
        //dragHandle.height(canvasHeight);
        var newTop = (-1 * $('#uCurve').height());
        //dragHandle.animate({ "top": newTop + 'px' });

        dragHandle.css({
            "top": (newTop)
        });
        return false;
    }


</script>

<script>
    function set_slider_blue(element) {
        //$(element).removeClass("disabled");
        $(".ui-slider-handle").removeClass("gray_bar");
    }

    function set_slider_gray(element) {
        // $(element).addClass("disabled");
        $(".ui-slider-handle").addClass("gray_bar");
    }


    // setTimeout(function () {
    var utilityCurveInput = XValue; //(parseFloat(output.UCData.XValue.toFixed(2)));


    // $(".tt-curve-slider").slider("value", XValue);
    //     $("#sliderCurve .ui-slider-handle").           resizeCurveHandle($dragHandle, $handlePos);

    //setTimeout(function () {
    $(".tt-curve-slider").slider("value", XValue);
    //}, 1500);

    setTimeout(function () {
        if (utilityCurveInput == -2147483648000) {
            $("#uCurveInput").val("");
            priorityLabel = "";
        }
        var sessionValue = '<%=Session("XValue") %>';
        if (utilityCurveInput < XMin || sessionValue=='') {
            set_slider_gray("#sliderCurve");
            $("#sliderCurve .ui-slider-handle").addClass("back-pulse");

            setTimeout(function () {
                $("#sliderCurve .ui-slider-handle").removeClass("back-pulse");
            }, 1000);
        }
        else {
            $("#uCurveInput").val(utilityCurveInput);
            set_slider_blue("#sliderCurve .ui-slider-handle");
        }

        $("#sliderCurve .ui-slider-handle").fadeIn();
        //chart.options.ucpriorityLabel[0].text = priorityLabel;
        ////console.log(canvasData[0] == "" ? + "" : canvasData[0] + " %");
        //chart.update();

    }, 500);


    //}, 800);
    function resizeStepFunctionHandle(dragHandle) {
        var cavasHeight = $('#sFunctionCanvas').height() + $('#stepsFunctionSlider').height();
        var startHeight = 291;
        //dragHandle.height(cavasHeight);
        var newTop = (-1 * $('#sFunctionCanvas').height());
        dragHandle.animate({ "top": newTop + 'px' });

        $('.tt-steps-slider .ui-slider-handle').fadeIn();
        return false;
    }

    $(document).ready(function () {
        $('.ui-slider-handle').click(function () {
            if ($('#steps-functionInput').val() == '') {
                $('#steps-functionInput').val(output.PipeParameters.min);
                $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
            }
            if ($('#uCurveInput').val() == '') {
                $('#uCurveInput').val('0');
                $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
                $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
            }
        });
    })
    
</script>

<script>
    var stepChart = null;
    var fStepPriorityLoaded = false;
    function render_step_function(min, max, stepX, stepY) {
        //        $(".tt-step-function-wrap").hide();       
        if (stepChart != null ? stepChart.destroy() : 1 + 1);
        fStepPriorityLoaded = false;
        StepInitPriorityLabel();
        /*var value = parseFloat(output.current_value);*/
        var value = XValue;
        //Change By Bhawan Above Line Skip replace with XValue this ints in doc.ready 
        $(".load-canvas-gif").show();
        xAxisLabels = [];

        var canvas = document.getElementById("sFunctionCanvas");
        var ctx = canvas.getContext("2d");
        //ctx.css('background-color', 'white');
        /*var overridenStep = (max - min) / 7.5;*/
        var overridenStep = (max + min) / 7.75;

        Chart.defaults.global.legend.display = false; //remove label at the top of graph
        Chart.defaults.global.tooltips.enabled = false;
        Chart.defaults.global.color = '';
        if (value < min) value = min;
        if (value > max) value = max;
        var current_value = parseFloat($('#steps-functionInput').val());  //Added line to get current position.
        var canvasData = getXandYofPriority(current_value, getUCStepData(min, max, stepX, stepY)[0]); //Pass Current value in the place of value to get current position.

        if (canvasData[0] != "")
            canvasData[0] = parseFloat((canvasData[0])).toFixed(2);

        Chart.pluginService.register({
            beforeDraw: function (chart, easing) {
                if (chart.config.options.chartArea && chart.config.options.chartArea.backgroundColor) {
                    var helpers = Chart.helpers;
                    var ctx1 = chart.chart.ctx;
                    var chartArea = chart.chartArea;

                    ctx1.save();
                    ctx1.fillStyle = chart.config.options.chartArea.backgroundColor;
                    ctx1.fillRect(chartArea.left, chartArea.top, chartArea.right - chartArea.left, chartArea.bottom - chartArea.top);
                    ctx1.restore();
                }
            }
        });

        stepChart = Chart.Line(ctx, {
            data: {
                datasets: [{
                    label: output.child_node,
                    data: getUCStepData(min, max, stepX, stepY)[0],
                    lineTension: 0,
                    borderColor: "#0058a3",
                    fill: false,
                    pointBackgroundColor: "#ecfaff"
                }]
            },

            options: {
                responsive: true,
                scales: {
                    xAxes: [{
                        type: "linear",
                        position: "bottom",
                        scaleLabel: {
                            display: true,
                            labelString: "Data",
                            fontSize: 16
                        },
                        ticks: {
                            min: min,
                            max: max,
                            fontSize: 10,
                            fixedStepSize: overridenStep,
                            callback: function (label, index, labels) {
                                try {
                                    var fInt = parseFloat(label);
                                    label = fInt.toFixed(2);
                                }
                                catch (e) {

                                }
                                if (index == 0) {
                                    label = "-inf";
                                }
                                if (index == labels.length - 1) {
                                    label = "+inf";
                                }


                                return label;
                            }
                        }
                    }],
                    yAxes: [{
                        scaleLabel: {
                            display: true,
                            labelString: "Priority",
                            fontSize: 16
                        },
                        ticks: {
                            callback: function (label, index, labels) {
                                if (is_float(label)) {
                                    label = parseFloat(label).toFixed(2);
                                }
                                return label + ".00%";
                            }
                        }
                    }]
                },
                chartArea: {
                    /*backgroundColor: 'white'*/
                },
                priorityLabel: [{
                    "x": output.current_value == -2147483648000 ? output.current_value : (value == 0 ? 0.00000001 : value),
                    "y": canvasData[0],
                    "style": "white",
                    "text": canvasData[0] == "" || output.current_value == -2147483648000 ? + "" : canvasData[0] + " %",
                    "reverse": canvasData[1],
                    "bottom": canvasData[2]
                }]
            }

        });

        //new Chart(ctx, stepChart);

        // //console.log(stepChart);
        $(".tt-step-function-wrap").show();
        $(".load-canvas-gif").hide();
        setTimeout(function () {
            canvas_width = stepChart.chartArea.right - stepChart.chartArea.left; //3 px for allowance to the right
            left_margin = stepChart.chartArea.left;
            canvas_height = stepChart.chart.height;
            $("#stepsFunctionSlider").width(canvas_width).css({ left: left_margin - 1 });

            //Added to shet marker position
            stepChart.options.priorityLabel[0].x = parseFloat(current_value);
            stepChart.update();

        }, 500);

    }

    function StepInitPriorityLabel() {
        if (!fStepPriorityLoaded) {
            fStepPriorityLoaded = true;
            var priorityLabelPlugin = {
                afterDraw: function (chartInstance) {
                    var yScale = chartInstance.scales["y-axis-0"];
                    var xScale = chartInstance.scales["x-axis-0"];
                    var canvas = chartInstance.chart;
                    var ctx = canvas.ctx;
                    var index;
                    var line;
                    var style;
                    ctx.font = "bold 12pt Arial";
                    if (chartInstance.options.priorityLabel) {
                        for (index = 0; index < chartInstance.options.priorityLabel.length; index++) {
                            line = chartInstance.options.priorityLabel[index];

                            if (!line.style) {
                                style = "rgb(231, 76, 60)";
                            } else {
                                style = line.style;
                            }

                            if (line.y || line.y == 0) {
                                yValue = yScale.getPixelForValue(line.y);
                            } else {
                                yValue = 0;
                            }
                            if (line.x) {
                                xValue = xScale.getPixelForValue(line.x);
                            } else {
                                xValue = 0;
                            }

                            ctx.lineWidth = 3;

                            if (line.text) {
                                if (line.reverse) {
                                    if (line.bottom) {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue + 5, yValue + ctx.lineWidth, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth + 20);
                                    }
                                    else {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue + 5, yValue + ctx.lineWidth - 40, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue + 5, yValue + ctx.lineWidth - 20);
                                    }
                                }
                                if (!line.reverse) {
                                    if (line.bottom) {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue - 70, yValue + ctx.lineWidth, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue - 65, yValue + ctx.lineWidth + 20);
                                    }
                                    else {
                                        ctx.fillStyle = "black";
                                        ctx.globalAlpha = 0.4;
                                        ctx.fillRect(xValue - 70, yValue + ctx.lineWidth - 40, 70, 30);
                                        ctx.globalAlpha = 1.0;
                                        ctx.fillStyle = style;
                                        ctx.fillText(line.text, xValue - 65, yValue + ctx.lineWidth - 20);
                                    }
                                }
                            }
                        }
                        return;
                    };
                }
            };
            Chart.pluginService.register(priorityLabelPlugin);
        }
    }


    function getUCStepData(XMin, XMax, StepsX, StepsY) {
        var data = [[]];
        var Shift = (XMax - XMin) / 100;
        data[0].push({ x: XMin, y: StepsY[0] * 100 });
        if (output.piecewise) {
            for (var i = 0; i <= StepsX.length - 1; i++) {
                data[0].push({ x: StepsX[i], y: StepsY[i] * 100 });

            };
        }
        else {
            for (var i = 0; i <= StepsX.length - 1; i++) {
                data[0].push({ x: StepsX[i], y: StepsY[i] * 100 });
                if (i == StepsX.length - 1)
                    data[0].push({ x: XMax, y: StepsY[i] * 100 });
                else {
                    data[0].push({ x: StepsX[i + 1], y: StepsY[i] * 100 });
                }
            };

        }
        data[0].push({ x: XMax, y: StepsY[StepsY.length - 1] * 100 });
        //console.log(JSON.stringify(data));
        return data;
    }

    function updateSliderSteps(value, first, last) {
        var min = 0; var max = 100;
        if (first > last) {
            max = parseFloat(first);
            min = parseFloat(last);
        }
        if (last > first) {
            max = parseFloat(last);
            min = parseFloat(first);
        }
        var current_value = parseFloat($('#steps-functionInput').val());
        if (current_value < -999999999999) {
            starting_value = min;
        }

        if (value > 0) {
            $(".tt-steps-slider").slider("option", "value", parseFloat($('#steps-functionInput').val()));
        } else {
            var val = parseFloat($('#steps-functionInput').val());
            if (isNaN(val)) {
                val = parseInt(-2147483648000);
            }

            $(".tt-steps-slider").slider({
                step: 0.01,
                value: val,
                min: min,
                max: max,
                animate: "fast",
                disabled: $(this).attr("data-disable"),
                slide: function (event, ui) {
                    var scope;
                    //if (is_anytime) {
                    //    scope = angular.element($("#anytime-page")).scope();
                    //} else {
                    //    scope = angular.element($("#TeamTimeDiv")).scope();
                    //}
                    var steplabel = getXandYofPriority(ui.value, stepChart.data.datasets[0].data);

                    stepChart.options.priorityLabel[0].x = parseFloat(ui.value);
                    stepChart.options.priorityLabel[0].y = steplabel[0];
                    stepChart.options.priorityLabel[0].reverse = steplabel[1];
                    stepChart.options.priorityLabel[0].bottom = steplabel[2];
                    stepChart.options.priorityLabel[0].text = steplabel[0].toFixed(2) + "%";
                    stepChart.update();
                    getPosition(event, ui);
                },
                stop: function (event, ui) {
                    var slidervalue = parseFloat(ui.value);
                    $("#steps-functionInput").val(slidervalue);
                    if (output.pipeOptions.dontAllowMissingJudgment) {
                        if (slidervalue > 0) {
                            EnableDisablePagination(true, false);
                        }
                        else {
                            EnableDisablePagination(true);
                        }
                    }
                    /*save_StepFunction(slidervalue);*/
                    save_utility_curve(8, null, 0, '<% =UCType %>')
                }
            });
        }
    }

    var screen_sizes = [{ "option": 0 }];
    function execute_onresize() {
        if ($(".graph-green-div").length > 0) {
            var nouibase = $(".noUi-base").width();
            $(".graph-green-div").css("left", "50%");
            $(".graph-green-div").width((nouibase / 2) - 1);
            var uibaseWidth = $("#gslider" + active_multi_index).width();
            $("#gslider" + active_multi_index + " .graph-green-div").width((uibaseWidth / 2) - 1);
        }

        var screen_height = window.outerHeight;
        var screen_width = document.documentElement.clientWidth;
        screens = screen_width;
        if (screen_width < 410) {
            screen_sizes.option = 1;
        }
        else if (screen_width < 500) {
            screen_sizes.option = 2;
        }//below is for desktop
        else if (screen_width < 600 || (screen_width > 1024 && screen_width < 1350)) {
            screen_sizes.option = 3;
        }
        else if (screen_width < 700 || (screen_width > 1024 && screen_width < 1600)) {

            screen_sizes.option = 4;
        }
        else if (screen_width < 800 || (screen_width > 1024 && screen_width < 1900)) {
            screen_sizes.option = 5;
        }
        else {
            screen_sizes.option = 6;
        }

        if (screen_height < 900 && screen_height > 700 && screen_width > 1024) {
            is_screen_reduced = true;
        }
        else {
            is_screen_reduced = false;
        }

        if (output != null) {
            if (output.non_pw_type == "mtStep") {
                setTimeout(function () {
                    canvas_width = stepChart.chartArea.right - stepChart.chartArea.left - 3; //3 px for allowance to the right
                    left_margin = stepChart.chartArea.left;
                    canvas_height = stepChart.chart.height;
                    $("#stepsFunctionSlider").width(canvas_width - 3).css({ left: left_margin - 1 });

                    if (screen_width < 1024) {
                        $(".tt-steps-slider .ui-slider-handle").width(12);
                    } else {
                        $(".tt-steps-slider .ui-slider-handle").width(6);
                    }

                }, 200);
            }
        }
    }

    function getPosition(event, ui) {
        var resetBtn = $(".tt-j-clear");
        $("#steps-functionInput").val(ui.value);
        var handlePos = $('.tt-steps-slider .ui-slider-handle').position();
        var dragHandle = $('.tt-steps-slider .ui-slider-handle');
        resizeStepFunctionHandle(dragHandle, handlePos);

        if ($("#steps-functionInput").val() != '') {
            $('.imgClose').css({ 'filter': 'none', 'opacity': '1' });
            $('.ui-slider-handle').css('background-color', 'var(--Primary-Blue)');
        }
        else {
            $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            $('.ui-slider-handle').css('background-color', 'var(--Spanish-Gray)');
        }

        //var scope = angular.element($("#anytime-page")).scope();
        //var scope;
        //if (is_anytime) {
        //    scope = angular.element($("#anytime-page")).scope();
        //} else {
        //    scope = angular.element($("#TeamTimeDiv")).scope();
        //}

        set_slider_blue('.tt-steps-slider');

        resetBtn.fadeIn();
    }

</script>
<div class=" res_padding">
    <div class="page_heading_section">
        <div class="container d-md-block">
            <div class="row">
                <div class="col-md-6">
                    <div class="heading_content justify-content-between" id="divutc" runat="server">
                    </div>
                </div>
                <div class="col-md-6" runat="server" id="HeaderInfoRigh">
                </div>

            </div>
        </div>
    </div>

    <div class="container">
        <div class="row">
            <div class="col-lg-6 order-2 mt-0 mt-lg-2">
                <div class="accordion accordion-flush" id="faqlist" runat="server">
                </div>
            </div>
            <div class="col-lg-6 order-1 mt-0 mt-lg-2 ">
                <div class="heading_content " id="UtDiv" runat="server">
                </div>
                <div class="chart_wrapper_outer">

                    <div class="large-6 medium-11 small-12 columns small-centered text-center tt-utility-curve-wrap" style="padding: 0px; display: none;" id="dvUtilityfunction">
                        <div class="close_icon">
                            <div id="infoScale" runat="server"></div>
                            <input type='text' class='as_number' id="uCurveInput" name="SliderValue" value="<%= IIf(XValueString = "0", "", XValueString) %>" onkeyup='onChangeText();' onmousedown='focusOnClick(this);' style='width: 6em; margin-right: 6px' />

                            <a onclick='eraseValue();' id="resetIcon1" class="newclass">
                                <img src="../../img/icon/erasar.svg" id='imgClose' runat="server" border='0' style='cursor: pointer;' class='imgClose' />
                                <span class='tooltip_item'>Clear Judgment</span>
                            </a>

                        </div>
                        <div id="sliderCurve" value="<%= IIf(XValueString = "0", "", XValueString) %>" class="slider columns tt-curve-slider dsBarClr"></div>

                        <canvas id="uCurve" class="u-curve">Your browser does not support HTML5 Canvas</canvas>

                    </div>


                    <div class="collapse text-center tt-step-function-wrap" style="display: none;" id="dvStepfunction">
                        <div class="close_icon">
                            <div id="infoScale1" runat="server"></div>
                            <input type='text' id="steps-functionInput" name="SliderValue" class="steps-slider-dragger as_number" value="<%= IIf(XValueString = "0", "", XValueString) %>" onkeyup='onChangeText();' onmousedown='focusOnClick(this);' style='width: 6em; margin-right: 6px' />
                            <a onclick='eraseValue();' id="resetIcon" class="newclass">
                                <img src="../../img/icon/erasar.svg" id='imgCloseOne' runat="server" border='0' style='cursor: pointer;' class='imgClose' />
                                <span class='tooltip_item'>Clear Judgment</span>
                            </a>
                        </div>

                        <div class="large-6 medium-11 small-12 columns small-centered text-center">
                            <div class="load-canvas-gif tt-fullwidth-loading hide" style="position: absolute; z-index: 1; top: 50%; left: 50%;">
                                <div class="tt-loading-icon small-loading-animate"><span class="icon-tt-loading "></span></div>
                            </div>
                            <div id="stepsFunctionSlider" style="margin-left: 0px; margin-right: 0px; width: unset" class="slider columns tt-steps-slider dsBarClr "></div>

                            <canvas id="sFunctionCanvas">Your browser does not support HTML5 Canvas</canvas>




                        </div>
                    </div>



                </div>
            </div>
        </div>
    </div>




    <div runat="server" class="row" id="infoDiv"></div>

    <div class="large-12 columns">
        <div class="tt-judgements-item large-12 columns" style="padding: 0px;">

            <div id="code" class="columns">
                <div class="columns tt-j-content col-lg-6 columns offset-lg-3 ">
                </div>
            </div>

            <div class="large-8 columns large-centered tt-j-clear text-center">
            </div>
        </div>
    </div>


    <div id="ditest" runat="server"></div>

</div>
