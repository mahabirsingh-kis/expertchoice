$(document).ready(function () {
    //*****START GRAPHICAL *****//

    $(".tt-dsBar").each(function (i, e) {
        $(e).slider({
            range: "min",
            value: $(this).attr("data-sliderval"),
            min: 0,
            max: 100,
            animate: true,
            disabled: $(this).attr("data-disable"),
            slide: function (event, ui) {

                var id = $(this).parent().attr("id");
                $("#dsInput" + id).val(ui.value / 100);
                //console.log(id); 

                var $resetBtn = $(".dsReset" + id);

                if ($("#dsInput" + id).val() != 0) {
                    $resetBtn.fadeIn();


                } else {
                    $resetBtn.fadeOut();
                }

                $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();


                //clear all judgement
                $(document).on("click", '.tt-action-btn-clr-j', Foundation.utils.debounce(function (e) {

                    $(".dsBarClr").slider("value", 0);
                    $(".dsInputClr").val('');
                    $(".dsReset" + id).hide();
                    $(this).fadeOut();

                }, 300, true));

            }
        });
    });

    /**-- reset button --**/
    $(document).on('click', '.ds-res-btn', Foundation.utils.debounce(function (e) {
        var id = $(this).parent().parent().attr("id");
        var $resetBtn = $(".dsReset" + id);
        var $slider = $(".dsBar" + id);
        $slider.slider("value", 0);
        $resetBtn.text('0');
        $("#dsInput" + id).val('');
        $(this).fadeOut();

        //$event.stopPropagation(); //added 8-17-2016

    }, 300, true));
    //*****END GRAPHICAL *****//

    //pizza
    var $a_currentVal = $("#slider").attr('data-slideval'); // get the initial value of the slider
    var tralala = 1;
    $(window).load(function () {
        if (tralala == 1) {
            var $a1 = $a_currentVal;
            var $b = 1600 - $a1;
            var pieA = parseInt($a1);
            var pieB = 1600 - pieA;
            //case 12226
            //updateFrontEnd($a1, $b); // then update the user's view
            //reloadPizza(pieA, pieB); // reload the pizza with the new data
            positionBarInputs($a1);

        }
        tralala = 2;

        try {
            var scope = angular.element($("#TeamTimeDiv")).scope();
            calculate_pie(scope.user_judgment);
        }
        catch (error) {
            //do nothing
        }

        
    });
    var info_docs_resize_event = false;
    $(window).on("resize", Foundation.utils.throttle(function (e) {
        if (!info_docs_resize_event) { //added this since when info docs resize is called the bars are updating case 9405
        }
        try {
            var scope = angular.element($("#TeamTimeDiv")).scope();
            calculate_pie(scope.user_judgment);
//            alert(1);
            //$(window).load();
            //hide_loading_modal();

        }
        catch (error) {
            //do nothing
        }

    }, 800));

    function calculate_pie(judgment) {
            var scope = angular.element($("#TeamTimeDiv")).scope();
            var $a1 = scope.graphical_slider[0];
            var $b = 1600 - $a1;
            var result;
            if (scope.user_judgment < -999999999999) {
                $a1 = null;
                updateFrontEnd($a1, $b);
                //Pizza.init('body', {
                //    data: [0, 100],
                //});
            }
            else {
                updateFrontEnd($b, $a1);
                result = calculate_pie_and_slider('blue', $a1);

            }

    }


    $("#slider").slider({
        //range: "min",
        //step: 0,
        min: 0,
        max: 1600,
        step: 1,
        slide: function (event, ui) {

            info_docs_resize_event = false;
            var $a = ui.value;
            var $b = 1600 - $a;
            updateFrontEnd($a, $b); // then update the user's view


            var a = $a / 16;
            var b = $b / 16;
            var areal;
            var breal;
            if ($a == 800) {
                areal = 1;
                breal = 1;
            }
            if ($a > 800) {
                areal = (1 + (($a - 800) / 100)).toFixed(2);
                breal = 1;
                //$(".bar-b-input").show()
                //$(".bar-a-input").hide()
            }
            if ($a < 800) {
                areal = 1;
                breal = (1 + ((800 - $a) / 100)).toFixed(2);
                //$(".bar-a-input").show()
                //$(".bar-b-input").hide()
            }
            var total = areal + breal


            reloadPizza(areal / total, breal / total); // reload the pizza with the new data

            //Action if 100% detected
            var theFullPie = $('.pie-full');

            //if($a == 0){
            //    theFullPie.removeClass('blue');
            //    theFullPie.removeClass('green');
            theFullPie.removeClass('gray');
            //}
            //$('.tt-j-clear').fadeIn(); //show the erase button 
            positionBarInputs($a);

        }
    });
    $(document).on("click", '.erasebtn', Foundation.utils.debounce(function (e) {
        $('.bar-a .pie-100-percent-ratio, .bar-b .pie-100-percent-ratio').css({ "margin-right": "-126px" });
    }, 300, true));    
    //end of pizza

    //***** START pairwise graphical side-by-side slider *****//
    $(function () {
        var handlers = [400, 800, 1200];
        var colors = ["gray", "#539ddd", "#6aa84f", "gray"];
        updateColors(handlers);
        var $curveVal = $('input[name="curvesValue"]');
        var $leftVal = $('.content-left input[type=text]');
        var $rightVal = $('.content-right input[type=text]');

        $("#graphicalSlider-main").slider({
            orientation: "horizontal",
            range: 'min',
            min: 0,
            max: 1600,
            values: handlers,

            step: 1,
            slide: function (event, ui) {
                if (ui.values[1] != 800) {
                    return false;
                }
                $leftVal.val((ui.values[0] / 100) + 1);
                $rightVal.val((ui.values[2] / 100) + 1);
                if (handlers[0] != ui.values[0]) {
                    if (ui.values[2] != ui.values[0] + 800) {
                        $("#graphicalSlider-main").slider("option", "values", [ui.values[0], 800, ui.values[0] + 800]);
                        if (ui.values[0] > 800) {
                            $("#graphicalSlider-main").slider("option", "values", [800, 800, ui.values[0] + 800]);
                            handlers = $("#graphicalSlider-main").slider("option", "values");
                            return false;
                        }
                        handlers = $("#graphicalSlider-main").slider("option", "values");
                    }
                }
                else if (handlers[2] != ui.values[2]) {
                    if (ui.values[0] != ui.values[2] - 800) {
                        $("#graphicalSlider-main").slider("option", "values", [ui.values[2] - 800, 800, ui.values[2]]);
                        if (ui.values[2] < 800) {
                            $("#graphicalSlider-main").slider("option", "values", [ui.values[2] - 800, 800, 800]);
                            updateColors(handlers);
                            return false;
                        }
                        handlers = $("#graphicalSlider-main").slider("option", "values");
                    }
                }
                updateColors(handlers);
                //refreshSwatch
            }
            //change: refreshSwatch
        });
        var left = $('#graphicalSlider-main .ui-slider-handle:nth-of-type(4)').css('left');
        left = parseInt(left, 10);
        $('.content-left input[type=text]').css('margin-left', left - 47 + 'px');
        $('.content-right input[type=text]').css('margin-right', left - 52 + 'px');

        function updateColors(values) {
            var colorstops = colors[0] + ", "; // start left with the first color
            for (var i = 0; i < values.length; i++) {
                colorstops += colors[i] + " " + (values[i] / 1600) * 100 + "%,";
                colorstops += colors[i + 1] + " " + (values[i] / 1600) * 100 + "%,";
            }
            // end with the last color to the right
            colorstops += colors[colors.length - 1];

            /* Safari 5.1, Chrome 10+ */
            var css = '-webkit-linear-gradient(left,' + colorstops + ')';
            $('#graphicalSlider-main').css('background-image', css);
        }
    });
    //***** END pairwise graphical side-by-side slider *****//
});

function positionBarInputs(a) {
    //    console.log( $('.atext').text() );
    var $a_input = $('.bar-a .pie-100-percent-ratio');
    var $b_input = $('.bar-b .pie-100-percent-ratio');
    if (a < 800) {
        $a_input.css({ "margin-right": "4px" });
        $b_input.css({ "margin-right": "-46px" });

    } else {
        $a_input.css({ "margin-right": "-46px" });
        $b_input.css({ "margin-right": "4px" });

    }

}
function reloadPizza(a, b) {
    //Pizza.init('body', {
    //    data: [a, b],
    //    stroke_color: '#fff',
    //    stroke_width: 2,
    //    percent_offset: 35,
    //    show_text: false,
    //    show_percent: true,
    //    always_show_text: false,
    //    animation_speed: 500,
    //    animation_type: 'elastic'
    //});
}

function updateFrontEnd(aval, bval) {
    var a = aval / 16;
    var b = bval / 16;
    if (aval == null) {
        a = 50;
        b = 50;
    }

    //var areal;
    //var breal;
    //if (aval == 800) {
    //    areal = 1;
    //    breal = 1;
    //}
    //if (aval > 800) {
    //    areal = (1 + ((aval - 800) / 100)).toFixed(2);
    //    breal = 1;
    //}
    //if (aval < 800) {
    //    areal = 1;
    //    breal = (1 + ((800 - aval) / 100)).toFixed(2);
    //}

    $("#ttPizzaData").find('li.m2').attr('data-value', +a);
    //$("#ttPizzaData").find('li.m2 span.percent').html(areal);
    $('input[name="a"]').val(b);
    $("#pie").find('g text[data-id="s0"]').html(a + "%");
    $(".bar-a").css({ 'width': b + "%" });
    $("#ttPizzaData").find('li.m1').attr('data-value', +b);
    //$("#ttPizzaData").find('li.m1 span.percent').html(breal);
    $('input[name="b"]').val(a);
    $("#pie").find('g text[data-id="s1"]').html(b + "%");
    $(".bar-b").css({ 'width': a + "%" });
}

