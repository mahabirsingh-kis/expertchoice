<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Graphical.aspx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.WebForm1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

        <div class="large-8 large-centered columns graphical-slider-wrap text-center"> 
    <input name="curvesValue" type="text" style="width:100px;">
        
        <div class="medium-1 columns medium-text-right">A</div>
        <div class="medium-10 columns text-center" ><div id="graphicalSlider" style="height:25px;" class="columns slider"><span class="content-left hide"><input type="text" /></span> <span class="content-right hide"><input type="text" /></span></div></div>
        <div class="medium-1 columns medium-text-left">B</div>
        
    
    </div>
        
         
        <script>
            /*$(function() { 
                  var $curveVal = $( 'input[name="curvesValue"]' );
                  $("#graphicalSlider").slider({
  
                  value: 0,
                  min: -100,
                  max: 100,
                  step: 1,
                  create : function() {
                   var value = $(".slider").slider( "value" );
                    $curveVal.val((value > '0') ? ('+'+ value) : value);
                  },
  
                  slide: function (event, ui) {
                      $curveVal.val( ui.value );
  
                      var value = Math.abs(ui.value);
                      var percentage = (value/100)*100;
                      if(ui.value>0){
                          $('#blue_bar_bg .min span').css('width',percentage+'%');
                          $('#blue_bar_bg .max span').css('width','0%');
                      }
  
                      if(ui.value<0){
                          $('#blue_bar_bg .max span').css('width',percentage+'%');
                          $('#blue_bar_bg .min span').css('width','0%');
                      }  
                      if(ui.value==0){
                          $('#blue_bar_bg .max span').css('width','0%');
                          $('#blue_bar_bg .min span').css('width','0%');
                      } 
  
  
                  },
                      stop: function( event, ui ) {
  
                          if($curveVal.val() != $curveVal.val( ui.value )){
                              $curveVal.trigger('change');
                          }
                      }
  
                     }).append('<div id="blue_bar_bg" style="width: 100%"><div class="min"><span></span></div><div class="max"><span></span></div></div>');
                  });*/``

            $(function () {
                var handlers = [400, 800, 1200];
                var colors = ["gray", "#539ddd", "#6aa84f", "gray"];
                updateColors(handlers);
                var $curveVal = $('input[name="curvesValue"]');
                var $leftVal = $('.content-left input[type=text]');
                var $rightVal = $('.content-right input[type=text]');

                $("#graphicalSlider").slider({
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
                                $("#graphicalSlider").slider("option", "values", [ui.values[0], 800, ui.values[0] + 800]);
                                if (ui.values[0] > 800) {
                                    $("#graphicalSlider").slider("option", "values", [800, 800, ui.values[0] + 800]);
                                    handlers = $("#graphicalSlider").slider("option", "values");
                                    return false;
                                }
                                handlers = $("#graphicalSlider").slider("option", "values");
                            }
                        }
                        else if (handlers[2] != ui.values[2]) {
                            if (ui.values[0] != ui.values[2] - 800) {
                                $("#graphicalSlider").slider("option", "values", [ui.values[2] - 800, 800, ui.values[2]]);
                                if (ui.values[2] < 800) {
                                    $("#graphicalSlider").slider("option", "values", [ui.values[2] - 800, 800, 800]);
                                    updateColors(handlers);
                                    return false;
                                }
                                handlers = $("#graphicalSlider").slider("option", "values");
                            }
                        }
                        updateColors(handlers);
                        //refreshSwatch
                    }
                    //change: refreshSwatch
                });
                var left = $('#graphicalSlider .ui-slider-handle:nth-of-type(4)').css('left');
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
                    $('#graphicalSlider').css('background-image', css);
                }
            });


          </script>

</asp:Content>
