$(document).ready(function () {
    //var scope = angular.element($("#anytime-page")).scope();
    var scope;
    if (is_anytime) {
        scope = angular.element($("#anytime-page")).scope();
    } else {
        scope = angular.element($("#TeamTimeDiv")).scope();
    }

    try {
        var min = scope.output.UCData.XMinValue;
        var max = scope.output.UCData.XMaxValue;
        var value = scope.utilityCurveInput;
        $(".tt-curve-slider").slider({
            step: 0.01,
            value: value,
            min: min,
            max: max,
            animate: "fast",
            disabled: $(this).attr("data-disable"),
            slide: function (event, ui) {
                var $handlePos = $('.tt-utility-curve-wrap .ui-slider-handle').position();
                var $dragHandle = $('.tt-utility-curve-wrap .ui-slider-handle');
                $("#uCurveInput").val(ui.value);
                resizeCurveHandle($dragHandle, $handlePos);

                //var scope = angular.element($("#anytime-page")).scope();
                var scope;
                if (is_anytime) {
                    scope = angular.element($("#anytime-page")).scope();
                } else {
                    scope = angular.element($("#TeamTimeDiv")).scope();
                }

                var steplabel = getXandYofPriority(ui.value, scope.chart.data.datasets[0].data);
                //console.log(steplabel + ' ' + ui.value);
                scope.chart.options.ucpriorityLabel[0].x = parseFloat(ui.value);
                scope.chart.options.ucpriorityLabel[0].y = steplabel[0];
                scope.chart.options.ucpriorityLabel[0].reverse = steplabel[1];
                scope.chart.options.ucpriorityLabel[0].bottom = steplabel[2];
                scope.chart.options.ucpriorityLabel[0].text = (steplabel[0]).toFixed(2) + "%";
                scope.$apply(scope.chart.update());
                //if (ui.value > 0) {
                    scope.set_slider_blue('#sliderCurve');
                //}

            },
            stop: function (event, ui) {
                var val = ui.value;
                scope.save_utility_curve(val);
            }
        });
    }
    catch (e) {

    }
   
    

    $(window).on("slidestart", Foundation.utils.throttle(function (e) {

        $handlePos = $('.tt-utility-curve-wrap .ui-slider-handle').position();
        $dragHandle = $('.tt-utility-curve-wrap .ui-slider-handle');
        resizeCurveHandle($dragHandle, $handlePos);
    }, 300));
    //*****END utility curve Dragable sliders


    //*****START Utility Curve responsive slider height *****//   
    //**--Get Utility Curve's current height on resize--**//
    $(window).on('resize', Foundation.utils.throttle(function (e) {
        $handlePos = $('.tt-utility-curve-wrap .ui-slider-handle').position();
        $dragHandle = $('.tt-utility-curve-wrap .ui-slider-handle');
        resizeCurveHandle($dragHandle, $handlePos);
    }, 800));
    $(window).on('load', Foundation.utils.throttle(function (e) {
//        $handlePos = $('.tt-utility-curve-wrap .ui-slider-handle').position();
//        $dragHandle = $('.tt-utility-curve-wrap .ui-slider-handle');
//        resizeCurveHandle($dragHandle, $handlePos);

    }, 300));
    //**--clear all judgement in utility curve--**//
    $(document).on("click", '.tt-action-btn-clr-j', Foundation.utils.debounce(function (e) {
        //var $resetBtn = $(".erase-uc-btn");

        $handlePos = $('.tt-utility-curve-wrap .ui-slider-handle').position();
        $dragHandle = $('.tt-utility-curve-wrap .ui-slider-handle');
        $($dragHandle).hide();

//        setTimeout(function () {
//            $(".dsBarClr").slider("value", 0);
//            $("#uCurveInput").val('');
//            //            resizeCurveHandle($dragHandle,$handlePos);
//            $dragHandle.css({
//                "height": 65,
//                "top": -50
//            });
//            setTimeout(function () {
//                $($dragHandle).slideDown();
//            }, 1000);
//        }, 1000);


    }, 1000, true));
});


function resizeCurveHandle($dragHandle) {
    var canvasHeight = $('#uCurve').height() + $('#sliderCurve').height();
    var startHeight = 291;
    $dragHandle.height(canvasHeight);
    var newTop = (-1 * $('#uCurve').height());
    $dragHandle.animate({ "top": newTop + 'px' });

    $dragHandle.css({
        "top": (newTop)
    });
    return false;
}

//function resizeCurveHandle($dragHandle, $handlePos) {
//    var strJSON = $handlePos;
//    var startTop = -50;
//    var startHeight = 65;
//    var newHeight = startHeight;
//    var newTop = startTop;
//    if (typeof (strJSON) === 'undefined') {
//        return false;
//    }
//    else {
//        //newHeight = startHeight + (strJSON.left / 2);
//        //newTop = startTop - (startHeight + strJSON.left / 2) + 65
//    }

//    $dragHandle.css({
//        "height": (newHeight),
//        "top": (newTop)
//    });

//    return false;
//}