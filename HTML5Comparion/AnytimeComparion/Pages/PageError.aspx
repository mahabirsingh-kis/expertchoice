<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PageError.aspx.cs" Inherits="AnytimeComparion.Pages.WebForm5" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    
    <div class="large-12 columns large-centered text-center small-centered">
        <h2>Internal Server Error Occurred</h2>
        <br /><br />
        <div>
            <%=global_asax.ServerError %>
        </div>

    </div>

    <canvas id="myChart" width="200" height="200"></canvas>

    <script>
    //    var ctx = document.getElementById("myChart").getContext("2d");
    //    var data = [
    //        {
    //            label: 'My First dataset',
    //            strokeColor: '#F16220',
    //            pointColor: '#F16220',
    //            pointStrokeColor: '#fff',
    //            data: [
    //              { x: 10, y: 12 },
    //              { x: 14, y: 16 },
    //              { x: 20, y: 23 },
    //              { x: 23, y: 35 },
    //              { x: 35, y: 42 }
    //            ]
    //        },
    //        {
    //            label: 'My Second dataset',
    //            strokeColor: '#007ACC',
    //            pointColor: '#007ACC',
    //            pointStrokeColor: '#fff',
    //            data: [
    //              { x: 19, y: 75, r: 4 },
    //              { x: 27, y: 69, r: 7 },
    //              { x: 28, y: 70, r: 5 },
    //              { x: 40, y: 31, r: 3 },
    //              { x: 48, y: 76, r: 6 },
    //              { x: 52, y: 23, r: 3 },
    //              { x: 24, y: 32, r: 4 }
    //            ]
    //        }
    //    ];

    //    var dataSteps = {
    //        labels: [
    //            "0",
    //            "30",
    //            "50",
    //            "70",
    //            "80",
    //            "100"
    //        ],
    //        datasets: [
    //            {
    //                label: 'My First dataset',
    //                strokeColor: '#F16220',
    //                pointColor: '#F16220',
    //                pointStrokeColor: '#fff',
    //                data: [
    //                  { x: -7.5, y: 30 },
    //                  { x: 0, y: 30 },
    //                  { x: 25, y: 30 },
    //                  { x: 25, y: 1 },
    //                  { x: 46, y: 1 },
    //                  { x: 46, y: 100 },
    //                  { x: 50, y: 100 },
    //                  { x: 50, y: 50 },
    //                  { x: 75, y: 50 },
    //                  { x: 75, y: 75 },
    //                  { x: 82.5, y: 75 }
    //                ]
    //            },
    //            {
    //                label: 'My Second dataset',
    //                strokeColor: '#007ACC',
    //                pointColor: '#007ACC',
    //                pointStrokeColor: '#fff',
    //                data: [
    //                  { x: 19, y: 75, r: 4 },
    //                  { x: 27, y: 69, r: 7 },
    //                  { x: 28, y: 70, r: 5 },
    //                  { x: 40, y: 31, r: 3 },
    //                  { x: 48, y: 76, r: 6 },
    //                  { x: 52, y: 23, r: 3 },
    //                  { x: 24, y: 32, r: 4 }
    //                ]
    //            }
    //        ]
    //    };

    //var options = {
    //    // Boolean - If we should show the scale at all
    //    showScale: true,

    //    // Interpolated JS string - can access value
    //    scaleLabel: "<" + "%=value%>",

    //    // Boolean - Whether the scale should stick to integers, not floats even if drawing space is there
    //    scaleIntegersOnly: true,

    //    // Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
    //    scaleBeginAtZero: false,

    //    // String - Scale label font declaration for the scale label
    //    scaleFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",

    //    // Number - Scale label font size in pixels
    //    scaleFontSize: 40,

    //    // String - Scale label font weight style
    //    scaleFontStyle: "normal",

    //    // String - Scale label font colour
    //    scaleFontColor: "#666",

    //    // Boolean - Whether to show labels on the scale
    //    scaleShowLabels: true,

    //    // Boolean - whether or not the chart should be responsive and resize when the browser does.
    //    responsive: true,

    //    // Boolean - Determines whether to draw tooltips on the canvas or not
    //    showTooltips: false,

    //    ///Boolean - Whether grid lines are shown across the chart
    //    scaleShowGridLines: true,

    //    //String - Colour of the grid lines
    //    scaleGridLineColor: "rgba(0,0,0,.05)",

    //    //Number - Width of the grid lines
    //    scaleGridLineWidth: 2,

    //    //Boolean - Whether to show horizontal lines (except X axis)
    //    scaleShowHorizontalLines: true,

    //    //Boolean - Whether to show vertical lines (except Y axis)
    //    scaleShowVerticalLines: true,


    //    //Number - Tension of the bezier curve between points
    //    bezierCurveTension: 0,

    //    //Boolean - Whether to show a dot for each point
    //    pointDot: true,
    //    //Number - Radius of each point dot in pixels
    //    pointDotRadius: 4,

    //    //Number - Pixel width of point dot stroke
    //    pointDotStrokeWidth: 1,

    //    //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
    //    pointHitDetectionRadius: 20,

    //    //Boolean - Whether to show a stroke for datasets
    //    datasetStroke: true,

    //    //Number - Pixel width of dataset stroke
    //    datasetStrokeWidth: 2,

    //    //Boolean - Whether to fill the dataset with a colour
    //    datasetFill: false

    //    //***** added by mike: Feb 2, 2016
    //    //Draw line or not
    //    //            drawLineStroke: true,
    //    //activate Steps

    //};

    //var myLineChart = new Chart(ctx).Scatter(dataSteps, options);
    //console.log(JSON.stringify(myLineChart.datasets));  
    //alert(dataSteps.datasets[0].label);

    //    var slider = document.getElementById('sliderer');


    //    noUiSlider.create(slider, {
    //        start: [400, 1200],
    //        behaviour: 'drag-fixed-tap',
    //        connect: true,
    //        step: 0.01,
    //        range: {
    //            'min': 0,
    //            'max': 1600
    //        },
    //        pips: {
    //            mode: 'positions',
    //            values: [ 5, 12.5, 25, 50, 75, 87.5, 95],
    //            density: 2,
    //            stepped: false
    //        }
    //    });
    //    var slider_value = slider.noUiSlider.get();
    //    var step = (parseFloat(slider_value[1]) - parseFloat(slider_value[0]));
    //    var max = $('.noUi-base').width();
    //    var sliderleft = $('.noUi-base').offset().left;
        
    //    var zero = (max / 2);

    //    var lower = $('.noUi-handle-upper').offset().left - $('.noUi-handle-lower').offset().left;
    //    //var width = $('.noUi-handle-lower').width();
    //    var half = lower / 2;
    //    //var $div = $("<div></div>", { id: "foo", "class": "a" });
    //    //$div.css("background-color", "yellow").css("left", $('.noUi-handle-upper').offset().left);
    //    //$div.width(half);
    //    //$div.height(100 + '%');
    //    var $div2 = $("<div></div>", { "class": "graph-green-div" });
    //    $div2.css("background-color", "lightgreen").css("position", "fixed").css('left', zero);
    //    $div2.width(zero);
    //    $div2.height(100 + '%');
    //    $('.noUi-connect').prepend($div2);
    //    //$('.noUi-handle-lower').css('top', -22);

    //    slider.noUiSlider.on('change', function () {
    //        var value = slider.noUiSlider.get();
    //        //console.log(slider.noUiSlider.get());
    //        if ($('.noUi-origin').is(":hidden")) {
    //            $('.noUi-origin').show();
    //            if (parseFloat(value[1]) < 800) {
    //                slider.noUiSlider.set([value[1], parseFloat(value[1]) + 800]);
    //                return false;
    //            }
                   

                
    //        };

            
    //        if (value[0] != slider_value[0])
    //        {
    //            var value1 = parseFloat(value[0]) + step;
    //            slider.noUiSlider.set([value[0], value1]);
    //        }
    //        if (value[1] != slider_value[1])
    //        {
    //            var value1 = parseFloat(value[1]) - step;
    //            slider.noUiSlider.set([value1, value[1]]);
    //        }
    //        slider_value = slider.noUiSlider.get();

    //        var value = slider.noUiSlider.get();
    //        if (parseFloat(value[0]) > 720) {
    //            slider.noUiSlider.set([720, 1520]);
    //            return false;
    //        }
    //        else if (parseFloat(value[0]) < 80) {
    //            slider.noUiSlider.set([80, 880]);
    //            return false;
    //        }


    //    });

    //    var pipsy = ['9', '3', '1', '0', '1', '3', '9'];

    //    //var orig_pips = $('.noUi-value-large');
    //    //for (i = 0; i < pipsy.length; i++) {
    //    //    orig_pips[i].text
    //    //}
    //    $('.noUi-value-large').each(function (i, e) {
    //        $(this).text(pipsy[i]);

    //    });


    //    slider.noUiSlider.on('update', function () {
    //        $('.noUi-connect').css('background-color', 'cornflowerblue');
    //        $('.graph-green-div').css('background-color', 'lightgreen');
    //        var valala = slider.noUiSlider.get();
    //        scope = angular.element($(".full-width-enabled")).scope();
    //        scope.$apply(function () {
    //            if (valala[0] > 400){
    //                valala[1] = ((valala[1] - 800) / (800 - valala[0])).toFixed(2);
    //                valala[0] = 1;
    //            }
    //             else{
    //                valala[0] = ((800 - valala[0]) / (valala[0])).toFixed(2);
    //                valala[1] = 1;
    //            }

                    
                
    //            scope.lefttxt = valala[0];
    //            scope.righttxt = valala[1];
    //        })

    //        $('.graph-green-div').width(50 + '%');

    //        //console.log(value);
    //    });

    //    function swap() {
    //        var val = slider.noUiSlider.get();
    //        val[0] = 1600 - parseFloat(val[0]);
    //        val[1] = 1600 - parseFloat(val[1]);
    //        slider.noUiSlider.set([val[1], val[0]]);
    //    }

    //    function erase() {
    //        var val = slider.noUiSlider.get();
    //        slider.noUiSlider.set([80, 880]);
    //        $('.noUi-origin').hide();
    //    }
        



    </script>
    <style>
        /*.noUi-background {
            background-color:red;
        }*/

        .noUi-handle-lower {
            background-color:blue;
        }
        .noUi-handle-upper {
            background-color:green;
        }

        /*.noUi-target {
            background-color:yellow;
        }*/

        .noUi-connect {
            background-color:cornflowerblue;
        }
    </style>
</asp:Content>
