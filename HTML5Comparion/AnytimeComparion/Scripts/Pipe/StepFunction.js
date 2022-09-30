//***** START Dragable sliders for STEP Function*****//
$(document).ready(function () {

    var reverse, reversed = false; 

    var dataSteps = {
        labels: [
            "0",
            "30",
            "50",
            "70",
            "80",
            "100"
        ],
        datasets: [
            {
                label: "Data",
                fillColor: "rgba(151,187,205,0.2)",
                strokeColor: "rgba(151,187,205,1)",
                pointColor: "rgba(151,187,205,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(151,187,205,1)",
                //data: [100, 10, 60, 80, 100, 90]
            }

        ]
    };

 
    
        
//        var canvasSteps = document.getElementById("sFunctionCanvas");
//        var ctxSteps = canvasSteps.getContext("2d");
    //        new Chart(ctxSteps).Line(dataSteps, optionsSteps);
    try{
        var canvasSteps = document.getElementById("sFunctionCanvas");
        var ctxSteps = canvasSteps.getContext("2d");
    }
    catch (e) {

    }
    
//    $(window).on('load', Foundation.utils.throttle(function (e) {
//
//
//        //set initial values on load
//        var lowerValues = "0, 10, 50, 70, 80, 100, 100";
//        dataSteps.datasets[0].data = lowerValues.split(",");
//        //console.log('lowerValues:' + lowerValues);
//
//        var priorityValues = "0, 10, 50, 70, 80, 100, 100";
//        dataSteps.labels = priorityValues.split(",");
//        //console.log('priorityValues:' + priorityValues + '\nEna, change these values dynamically');
//
//        try{
//            canvasSteps = document.getElementById("sFunctionCanvas");
//            ctxSteps = canvasSteps.getContext("2d");
//            myLineChart = Chart.Line(ctx, {
//                data: dataSteps,
//                options: optionsSteps
//            });
//        }
//        catch(e){
//
//        }
//        
//    }));
    
    var hehe = 1;
    //*********** update values on click

    
    
    $(document).on("keyup", 'input[name="lowerBound"]', Foundation.utils.throttle(function (e) {
        updateCanvas();
     }, 500));
    
    $(document).on("change", 'input[name="lowerBound"]', Foundation.utils.throttle(function (e) {
        updateCanvas();
     }, 100));
    
    $(document).on("keyup", 'input[name="priority"]', Foundation.utils.throttle(function (e) {
        updateCanvas();
     }, 500));
    
    $(document).on("change", 'input[name="priority"]', Foundation.utils.throttle(function (e) {
        updateCanvas();
     }, 100));
        
   

    
    /* start toggle switch-high-low */
    $(document).on("click", '.switch-high-low', Foundation.utils.debounce(function (e) {
       
        sortValues();
        if(!$('input[name="curvesToggle"]').is(':checked')) updateCanvas();
    }, 200,true));
    /* end toggle switch-high-low */
    
    /* start Sort values for reversing */
    function sortValues(){
         var highToLowCheckbox = $('input[name="highToLow"]');
        var priority = $('input[name="priority"]');
        var priorityVal = $('input[name="priority"]').val().split(",");
        var lowerBound = $('input[name="lowerBound"]');
        var lowerBoundVal = $('input[name="lowerBound"]').val().split(",");
        
        if( highToLowCheckbox.is(':checked')){
            lowerBoundVal.sort(sortNumber);
            lowerBound.val(lowerBoundVal.join(","));
            reverse = false;
            
//            priorityVal.sort(sortNumber);
//            priority.val(priorityVal.join(","));
        }else{
            reverse = true;
            lowerBoundVal.sort(reverseSort);
            lowerBound.val(lowerBoundVal.join(","));
            
//            priorityVal.sort(reverseSort);
//            priority.val(priorityVal.join(","));
            
        }
        
        function sortNumber(a,b) {
            return a - b;
        }
        function reverseSort(a,b) {
            return b - a;
        }
        
        
        if($('input[name="curvesToggle"]').is(':checked')) $('input[name="curvesValue"]').trigger('change');
    }
    /* end Sort values for reversing */
    
    /* start switch linearToggle and curvesToggle*/
    $(document).on("click", 'input[name="linearToggle"]', Foundation.utils.debounce(function (e) {
        var curveValueWrap = $('.curves-value-wrap');
        var curvesToggle = $('input[name="curvesToggle"]');
        var nonCurveContent = $('.non-curve-content');
        
        curveValueWrap.hide();
        nonCurveContent.show();
        curvesToggle.prop('checked',false);
        
        var lowerBound = $('input[name="lowerBound"]');
        var highToLowCheckbox = $('input[name="highToLow"]');
        
        if( $(this).is(':checked') && highToLowCheckbox.is(':checked')){ 
            var newLowerBoundVal = lowerBound.val('100,90, 80,70,60,50, 40,30,20,10,0');
        }
          if( $(this).is(':checked') && !highToLowCheckbox.is(':checked')){ 
            var newLowerBoundVal = lowerBound.val('0,10,20,30, 40,50,60,70, 80,90,100');
        }
        
        updateCanvas();
    }, 100,true));
    
     $(document).on("click", 'input[name="curvesToggle"]', Foundation.utils.debounce(function (e) {
        var linearToggle = $('input[name="linearToggle"]');
        var nonCurveContent = $('.non-curve-content');
        var curveValueWrap = $('.curves-value-wrap');
        linearToggle.prop('checked',false);
         
        
        var lowerBound = $('input[name="lowerBound"]');
        var highToLowCheckbox = $('input[name="highToLow"]');
         
         if( $(this).is(':checked') && highToLowCheckbox.is(':checked')){
            var newLowerBoundVal = lowerBound.val('100,99.3,98.4,96.5 ,93, 87, 77,63,45,25,0');
            
             
         }if( $(this).is(':checked') && !highToLowCheckbox.is(':checked')){
            var newLowerBoundVal = lowerBound.val('0,10,20,30, 40,50,60,70, 80,90,100');
//            var newLowerBoundVal = lowerBound.val('0,25,45,63, 77, 87,93,96.5 ,98.4,99.3,100');
            
        } 
         if( $(this).is(':checked')){
            curveValueWrap.slideToggle();
            nonCurveContent.slideToggle();
        }
        else{
            nonCurveContent.slideToggle();
            curveValueWrap.slideToggle();
        }    
                                          
         
         updateCanvas();
    }, 100,true));
    /* end switch linearToggle and curvesToggle*/
    
    /* start switch dots toggle*/
    $(document).on("click", 'input[name="dotsToggle"]', Foundation.utils.debounce(function (e) {
        var dotsToggle = $(this);
//        dotsToggle.prop('checked',false);
         updateCanvas();
    }, 100,true));
    /* end switch dots toggle*/
    
    /* start save canvas via OK btn*/
    $(document).on("click", '.tt-save-steps-btn', Foundation.utils.debounce(function (e) {       
        updateCanvas();
    }, 100,true));
    /* end save canvas via OK btn*/
    
    
    /* start toggle steps value*/
    $(document).on("click", '.stepsToggle', Foundation.utils.debounce(function (e) {
        var stepsToggle = $('input[name="stepsToggle"]');
        var stepsValueWrap = $('.steps-value-wrap');
        if( stepsToggle.is(':checked')){
            stepsValueWrap.slideToggle();
        }else{
            stepsValueWrap.slideToggle();
            $('input[name="stepsValue"]').val('');
        }
        updateCanvas();
    }, 100,true));
    /* end toggle steps value*/
    
    
    /* start Update canvas via curveValues */
    $(document).on("keyup", 'input[name="curvesValue"]', Foundation.utils.debounce(function (e) {
        $(this).trigger('change');
    }, 100,true));
    
    $(document).on("change", 'input[name="curvesValue"]', Foundation.utils.throttle(function (e) {
        
        //on change curve value
            var data = [0,10,20,30,40,50,60,70,80,90,100];
//            $('input[name="lowerBound"]').val("0,10,20,30,40,50,60,70,80,90,100");
			var meter = $(this).val();
            var positive = [ 0,25,45,63, 77, 87,93,96.5 ,98.4,99.3,100];
            var negative = [ 0,0.7,1.8,3.5,7,13,23,37,55,75,100 ];
//            dataSteps.datasets[0].data = $.map($('input[name="lowerBound"]').val().split(','), function(value){
//                return parseInt(value, 10);
//            });
            dataSteps.datasets[0].data = [0,10,20,30,40,50,60,70,80,90,100];
            var original = [];

            original = dataSteps.datasets[0].data.slice();
        
            function arrayReverse(){
                positive = positive.reverse();
                negative = negative.reverse();
                original = original.reverse();
                dataSteps.datasets[0].data = dataSteps.datasets[0].data.reverse();
                //console.log(dataSteps.datasets[0].data);
            }
//            //console.log(reverse)
            if(reverse){
                //console.log(1);
                reversed = true;
                arrayReverse();
            }

            if(!reverse && reversed){
                //console.log(2);
                reversed = false;
                arrayReverse();
                arrayReverse();
            }
//            //console.log(dataSteps.datasets[0].data);
        
			for(var i = 1; i < dataSteps.datasets[0].data.length-1; i++){
				if(meter > 0){
					dataSteps.datasets[0].data[i] = (original[i] + Math.floor((meter/(100)) * (positive[i]-original[i]))) < 100 ? (original[i] + Math.floor((meter/(100)) * (positive[i]-original[i]))) : 100;
                   
				} else {
					dataSteps.datasets[0].data[i] = (original[i] + ((meter/(100)) * Math.floor(original[i]-negative[i]))) < 100 ? (original[i] + Math.floor((meter/(100)) * (original[i]-negative[i]))) : 100;
                   
				}
			}
        
			var myLineChart = Chart.Line(ctxSteps, {
			    data: dataSteps,
			    options: optionsSteps
			});

            myLineChart.update();
        
        
//			window.myLine = new Chart(ctx).Mike(dataSteps, {
//				responsive: true
//			});
//        updateCanvas();
     }, 800));
    /* end Update canvas via curveValues */
        
    
    /* start Update canvas via stepsValue */
    $(document).on("keyup", 'input[name="stepsValue"]', Foundation.utils.debounce(function (e) {
        $(this).trigger('change');
     }, 800, true));
    
    $(document).on("change", 'input[name="stepsValue"]', Foundation.utils.throttle(function (e) {
        updateCanvas();
     }, 800));
    /* start Update canvas via stepsValue */
    
    
    $(window).on("slidestart", Foundation.utils.throttle(function (e) {
        $dragHandle = $('.tt-steps-slider .ui-slider-handle');
        resizeStepFunctionHandle($dragHandle);
    }, 300));
     
//    function updateCanvas() {
//        
//        var lowerBound = $('input[name="lowerBound"]');
//        var priority = $('input[name="priority"]');
//        
//        var lowerBoundVal = $('input[name="lowerBound"]').val();
//        var priorityVal = $('input[name="priority"]').val();
//        var stepsVal = $('input[name="stepsValue"]').val();
//        
//        var linearToggle = $('input[name="linearToggle"]');
//        var curvesToggle = $('input[name="curvesToggle"]');
//        var stepsToggle = $('input[name="stepsToggle"]');
//        var highToLowCheckbox = $('input[name="highToLow"]');
//        var dotsToggle = $('input[name="dotsToggle"]');
//
//
//        //console.log(lowerBoundVal+'\n'+priorityVal);
//
//        var lowerValues = lowerBoundVal;
//        dataSteps.datasets[0].data = lowerValues.split(",");
//
//        var priorityValues = priorityVal;
//        var priors = priorityValues.split(",")
//        //remove labels
//        for (i = 0; i < priors.length; i++) {
//            if (i % 6 != 0) {
//                priors[i] = "";
//            }
//        }
//
//        dataSteps.labels = priors;
//        var first = dataSteps.labels[0];
//        var last = dataSteps.labels[dataSteps.labels.length - 1];
//          
//        /** start linear **/
//        if( linearToggle.is(':checked') && dotsToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(false, false, true, true);
//        }
//        
//        if( linearToggle.is(':checked') && !dotsToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(false, false, true, false);
//        }
//        /** end linear **/
//        
//        
//        /** start curves **/
//        if( curvesToggle.is(':checked') && dotsToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(false, true, false, true);
//           
//        }
//                                             
//        if( curvesToggle.is(':checked') && !dotsToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(false, true, false, false);
//            
//            /*var nonInverted = $('.nonInverted');
//            var inverted = $('.inverted');
//            inverted.addClass('hide');
//            nonInverted.removeClass('hide');
//            //console.log('must hide');*/
//        }/*else{
//            var nonInverted = $('.nonInverted');
//            var inverted = $('.inverted');
//            .addClass('hide');
//            inverted.removeClass('hide');
//            //console.log('hide');
//        }*/
//        
//        
//        /** end curves **/
//        
//        /** start curves **/
//        if(!linearToggle.is(':checked') && !curvesToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(true, false, false,'');
//        }
//        
//        /** start dots **/
//        if( dotsToggle.is(':checked') && linearToggle.is(':checked') && curvesToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(false, true, true, true);
//        }
//        
//        if( dotsToggle.is(':checked') && !linearToggle.is(':checked') && !curvesToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(true, false, false, true);
//        }
//        
//        if( !dotsToggle.is(':checked') && linearToggle.is(':checked') && curvesToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(false, true, true, false);
//        }
//        
//        if( !dotsToggle.is(':checked') && !linearToggle.is(':checked') && !curvesToggle.is(':checked')){
//            optionsSteps =  checkOptionSteps(true, false, false, false);
//        }
//        
//        /** end dots **/
//        
//        
//        if( stepsToggle.is(':checked')){
//            if(stepsVal != '' || stepsVal > 0){
//                updateSliderSteps(stepsVal, first, last);
//            }
//            else{
//                var new_stepsVal = $('input[name="stepsValue"]').val('1');
//                updateSliderSteps(new_stepsVal, first, last);
//            }       
//        }
//        else{
//            var new_stepsVal = $('input[name="stepsValue"]').val('1');
//            updateSliderSteps(new_stepsVal, first, last);
//        } 
//                
//        resetStepsData();
//        var myLineChart = new Chart(ctxSteps).Line(dataSteps, optionsSteps);
//        //remove dots for non measurements 
//        try {
//            var temploop = priorityValues.split(",");
//            var scope = angular.element($(".full-width-enabled")).scope();
//
//            for (i = 0; i < temploop.length; i++) {
//                for (j = 0; j < scope.step_lower_bound.length; j++) {
//                    if (i == 0) {
//                        myLineChart.datasets[0].points[i].display = true;
//                        break;
//                    }
//                    if (i == temploop.length - 1) {
//                        myLineChart.datasets[0].points[i].display = true;
//                        break;
//                    }
//                    if (parseFloat(scope.step_lower_bound[j]) == parseFloat(temploop[i])) {
//
//                        myLineChart.datasets[0].points[i].display = true;
//                        break;
//                    } 
//                    else {
//                        myLineChart.datasets[0].points[i].display = false;
//                    }
//
//                }
//            }
//
//        }
//        catch (error) {
//            //do nothing
//        }
//
//        myLineChart.update();
//
//
//
//    }
    
    //***** END Dragable sliders for STEP Function*****//

    
    //***** START add data on  STEP Function*****//
    
  /*  $(document).on("click", '.add-data-steps-btn', Foundation.utils.debounce(function (e) {
        var dataStepContent = $('.data-steps-content');
        
        dataStepContent.hide().append('        <tr>   <td>B</td>         <td><input type="text" name="" class="inline-edit"></td>            <td><input type="text" name="" class="inline-edit"></td>            <td><input type="text" name="" class="inline-edit"</td>        </tr>').fadeIn(800);
        
    }, 500, true));*/
    
    //***** END add data on  STEP Function*****//
    
    
    
    
    //***** START responsive slider height for STEP Function*****//
    //**--Get  step function's current height on resize and on body load--**//
    $(window).on('resize', Foundation.utils.throttle(function (e) {
        $dragHandle = $('.tt-steps-slider .ui-slider-handle');
        resizeStepFunctionHandle($dragHandle);
    }, 800));
    $(window).on('load', Foundation.utils.throttle(function (e) {
       
        //$dragHandle = $('.tt-steps-slider .ui-slider-handle');
        //$($dragHandle).hide();

        //setTimeout(function () {
        //    resizeStepFunctionHandle($dragHandle);
        //    setTimeout(function () {
        //        $($dragHandle).slideDown();
        //    }, 1000);
        //}, 1000);
    }, 500));

    //**--clear all judgement in step function--**//
    $(document).on("click", '.clr-steps', Foundation.utils.debounce(function (e) {
        
        resetStepsData();

    }, 1000, true));
    
    function resetStepsData(){
        var $resetBtn = $(".tt-j-clear");

        $resetBtn.fadeOut();

        $dragHandle = $('.tt-steps-slider .ui-slider-handle');
        $($dragHandle).hide();

        setTimeout(function () {
            //$(".dsBarClr").slider("value", 0);
            resizeStepFunctionHandle($dragHandle);
            setTimeout(function () {
                $($dragHandle).slideDown();
            }, 1000);
        }, 1000);

    }
    

    //***** END responsive slider height for STEP Function*****//

});

function updateSliderSteps($value, first, last) {

    var min = 0; var max = 100;
    if (first > last) {
        max = parseFloat(first);
        min = parseFloat(last);
    }
    if (last > first) {
        max = parseFloat(last);
        min = parseFloat(first);
    }
    //var priorityVal = $('input[name="priority"]').val();
    //var steparray = priorityVal.split(",");
    //var dividend = (steparray[steparray.length - 1] / 8) * 10;
    var current_value = parseFloat($('#steps-functionInput').val());
    //var starting_value;
    //for (i = 0; i < steparray.length; i++) {
    //    if (parseInt(steparray[i]) <= parseFloat(current_value) && parseInt(steparray[i + 1]) >= parseFloat(current_value)) {
    //        var valstep = (Math.abs(parseInt(steparray[i]) - parseInt(steparray[i + 1]))) / dividend;
    //        var stepvalue = parseFloat((parseFloat(current_value) - parseFloat(steparray[i])) / valstep) + (i * dividend);
    //        if (steparray[i] == steparray[steparray.length - 1]) {
    //            stepvalue = parseFloat(steparray[i]);
    //        }
    //        starting_value = stepvalue
    //        break;
    //    }
    //}
    if (current_value < -999999999999) {
        starting_value = min;
    }

    //if ($('#steps-functionInput').val() == null) {
    //    $('#steps-functionInput').val(steparray[0])
    //}
    

    if ($value > 0) {
        $(".tt-steps-slider").slider("option", "value", parseFloat($('#steps-functionInput').val()));
    } else {
        var val = parseFloat($('#steps-functionInput').val());
        if(isNaN(val)){
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
                //current_value = $('#steps-functionInput').val();
                //if(current_value == ''){
                //    current_value=-8;
                //}
                
                //var scope = angular.element($("#anytime-page")).scope();
                var scope;
                if (is_anytime) {
                    scope = angular.element($("#anytime-page")).scope();
                } else {
                    scope = angular.element($("#TeamTimeDiv")).scope();
                }

                console.log(scope.stepChart);
                var steplabel = getXandYofPriority(ui.value, scope.stepChart.data.datasets[0].data);
                
                scope.stepChart.options.priorityLabel[0].x = parseFloat(ui.value);
                scope.stepChart.options.priorityLabel[0].y = steplabel[0];
                scope.stepChart.options.priorityLabel[0].reverse = steplabel[1];
                scope.stepChart.options.priorityLabel[0].bottom = steplabel[2];
                scope.stepChart.options.priorityLabel[0].text = steplabel[0].toFixed(2) + "%";
                scope.$apply(scope.stepChart.update());
                getPosition(event, ui);
            },
            stop: function (event, ui) {
                //save_step();
                //show_loading_modal();
                //var scope = angular.element($("#anytime-page")).scope();
                var scope;
                if (is_anytime) {
                    scope = angular.element($("#anytime-page")).scope();
                } else {
                    scope = angular.element($("#TeamTimeDiv")).scope();
                }

                var slidervalue = parseFloat($('#steps-functionInput').val());
                scope.save_StepFunction(slidervalue);
            }
        });
    }



    //return;
}

function getPosition(event, ui) {
    var $resetBtn = $(".tt-j-clear");
    $("#steps-functionInput").val(ui.value);
    var $handlePos = $('.tt-steps-slider .ui-slider-handle').position();
    var $dragHandle = $('.tt-steps-slider .ui-slider-handle');
    resizeStepFunctionHandle($dragHandle, $handlePos);

    //var scope = angular.element($("#anytime-page")).scope();
    var scope;
    if (is_anytime) {
        scope = angular.element($("#anytime-page")).scope();
    } else {
        scope = angular.element($("#TeamTimeDiv")).scope();
    }

    scope.set_slider_blue('.tt-steps-slider');

    $resetBtn.fadeIn();
}

function resizeStepFunctionHandle($dragHandle) {
    var cavasHeight = $('#sFunctionCanvas').height() + $('#stepsFunctionSlider').height();
    var startHeight = 291;
    $dragHandle.height(cavasHeight);
    var newTop = (-1 * $('#sFunctionCanvas').height());
    $dragHandle.animate({ "top": newTop + 'px' });
    
    $('.tt-steps-slider .ui-slider-handle').fadeIn();
    return false;
}

//$(".tt-steps-slider").slider({
//    step: 0.01,
//    value: $('#steps-functionInput').val(),
//    min: min,
//    max: max,
//    animate: "fast",
//    disabled: $(this).attr("data-disable"),
//    slide: function (event, ui) {
//        //current_value = $('#steps-functionInput').val();
//        //if(current_value == ''){
//        //    current_value=-8;
//        //}
//        getPosition(event, ui);
//    },
//    stop: function (event, ui) {
//        //save_step();
//    }
//});

////lowerbound_highest += ',' + parseFloat(parseFloat(short[1][short[1].length - 1]) * 100);
//var lowerbound_highest = parseFloat(parseFloat(short[1][short[1].length - 1]));
//var step_lowest = parseFloat(short[0][0]) - parseFloat(short[0][short[0].length - 1] * .1);
//var step_highest = parseFloat((parseFloat(short[0][short[0].length - 1]) * .1) + parseFloat(short[0][short[0].length - 1]));
//var priority = "";
////priority = "" + parseInt(short[0][0]) - parseFloat(short[0][short[0].length - 1] * .1) + "";
//var hulag = parseFloat(((step_highest - step_lowest) / 48).toFixed(2));
//var hulags = (lowerbound_highest / 24).toFixed(2);
//var tracker = [];
//var indx = 0;
//var hoho = short[0];
//hoho.splice(0, 0, short[0][0]);
//hoho.splice(short[0].length, 0, short[0][short[0].length - 1]);
//for (var i = 0; i < hoho.length; i++) {
//    var lowerboundtemp = parseFloat(hoho[i]).toFixed(2);
//    $scope.$parent.step_lower_bound.push(lowerboundtemp);
//}
//for (i = step_lowest; i <= step_highest; i = i + hulag) {

//    if (priority != "")
//        priority += ',';
//    priority = priority + i.toFixed(2);
//    var curi = i.toFixed(2);
//    var movement;
//    movement += hulags;
//    for (j = 0; j < hoho.length; j++) {
//        if (parseFloat(hoho[j]) == curi) {
//            tracker.push(indx);

//            //if (short[0][j] <= curi && short[0][j + 1] > curi) {
//            //if (lowerbound != "")
//            //    lowerbound += ',';
//            //lowerbound = lowerbound + i;
//        }
//    }
//    indx += 1;
//}

////tracker.push(indx);
//var steparray = priority.split(",");
////priority += ',' + parseInt((parseInt(short[0][short[0].length - 1]) * .1) + parseInt(short[0][short[0].length - 1]));
//var stp = 0; var shsh = 0; var testarr = [];
//var hehe = short[1].slice(0);
//hehe.splice(0, 0, short[1][0]);
//hehe.splice(short[1].length, 0, short[1][short[1].length - 1]);
//var sgy = [];
//for (var i = 0; i < hehe.length; i++) {
//    sgy.push(hehe[i]);
//}

//if ($scope.output.piecewise) {
//    //for piecewise linear
//    for (i = 0; i < sgy.length; i++) {
//        if (parseFloat(sgy[i]) <= parseFloat(sgy[i + 1])) {
//            var condition;
//            if (parseInt(tracker[i]) == parseInt(tracker[i + 1])) {
//                condition = parseInt(tracker[i]) + 1;
//            }
//            else {
//                condition = Math.abs(parseInt(tracker[i]) - parseInt(tracker[i + 1]));
//            }
//            for (j = 0; j < condition ; j++) {


//                var intervs = Math.abs(sgy[i] - sgy[i + 1]);
//                var ystep;
//                if (intervs == 0) {
//                    ystep = 0;
//                }
//                else {
//                    ystep = parseFloat(intervs) / (parseInt(tracker[i + 1]) - parseInt(tracker[i]));
//                }

//                hehe[i] = parseFloat(hehe[i]) + parseFloat(ystep);
//                hehe[i] = hehe[i].toFixed(2);
//                if (lowerbound != "")
//                    lowerbound += ',';
//                lowerbound = lowerbound + (hehe[i]);
//                testarr.push(ystep);
//            }
//        }
//        else {
//            var condition = Math.abs(parseInt(tracker[i]) - parseInt(tracker[i + 1]));
//            for (j = 0; j < condition ; j++) {

//                var intervs = Math.abs(sgy[i] - sgy[i + 1]);
//                var ystep;
//                if (intervs == 0) {
//                    ystep = 0;
//                }
//                else {
//                    ystep = parseFloat(intervs) / (parseInt(tracker[i]) - parseInt(tracker[i + 1]));
//                }

//                hehe[i] = parseFloat(hehe[i]) + parseFloat(ystep);
//                hehe[i] = hehe[i].toFixed(2);
//                if (lowerbound != "")
//                    lowerbound += ',';
//                lowerbound = lowerbound + (hehe[i]);
//                testarr.push(ystep);
//            }
//        }
//    }
//}
//else {
//    //for non-piecewise linear
//    for (i = 0; i < sgy.length; i++) {
//        if (parseFloat(sgy[i]) <= parseFloat(sgy[i + 1])) {
//            var condition;
//            if (parseInt(tracker[i]) == parseInt(tracker[i + 1])) {
//                condition = parseInt(tracker[i]);
//            }
//            else {
//                condition = Math.abs(parseInt(tracker[i]) - parseInt(tracker[i + 1]));
//            }
//            for (j = 0; j < condition ; j++) {
//                if (lowerbound != "")
//                    lowerbound += ',';
//                lowerbound = lowerbound + (hehe[i]);
//                testarr.push(ystep);
//            }
//        }
//        else {
//            var condition = Math.abs(parseInt(tracker[i]) - parseInt(tracker[i + 1]));
//            for (j = 0; j < condition ; j++) {
//                if (lowerbound != "")
//                    lowerbound += ',';
//                lowerbound = lowerbound + (hehe[i]);
//                testarr.push(ystep);
//            }
//        }
//    }
//}


//$('input[name="lowerBound"]').val(lowerbound);
//$('input[name="priority"]').val(priority);

//$('input[name="dotsToggle"]').prop('checked', true);
//$('input[name="priority"]').change();
//$('input[name="lowerBound"]').change();