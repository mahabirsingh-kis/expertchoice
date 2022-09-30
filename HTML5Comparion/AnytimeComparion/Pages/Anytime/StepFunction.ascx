<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StepFunction.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.StepFunction" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/EraseAndCommentButtons.ascx" TagPrefix="includes" TagName="EraseAndCommentButtons" %> 
<link href="/Content/stylesheets/stepfunction.css" rel="stylesheet" />
<includes:QuestionHeader runat="server" id="QuestionHeader" />
<div class="large-12 columns tt-auto-resize-content">  
    <div class="columns tt-question-choices">
       <div class="medium-4 columns">
            <includes:LeftNodeInfoDoc runat="server" id="LeftNodeInfoDoc" />       
        </div>
        <div class="medium-4 columns">
            <includes:ParentNodeInfoDoc runat="server" id="ParentNodeInfoDoc1" />    
        </div>
        <div class="medium-4 columns">
           <includes:WRTLeftNodeInfoDoc runat="server" id="WRTLeftNodeInfoDoc" />
        </div>
    </div>
    <div class="row collapse ">
        <div class="tt-judgements-item">
          
            <div id="code" class="columns"></div>
            <div class="columns tt-j-content  >">                  
                           
                <div class="row collapse text-center tt-step-function-wrap ">
                        <div class="large-6 medium-11 small-12 columns small-centered text-center">
                            <div class="load-canvas-gif tt-fullwidth-loading hide" style="position:absolute; z-index:1;   top: 50%;    left: 50%;">
                                <div class="tt-loading-icon small-loading-animate"  ><span class="icon-tt-loading " ></span></div>
                            </div>
                            <canvas id="sFunctionCanvas" >Your browser does not support HTML5 Canvas</canvas>
                            
                            <div id="stepsFunctionSlider" style="margin-left:0px; margin-right:0px; width:unset" data-sliderval="{{output.current_value}}" data-disable="" class="slider columns tt-steps-slider dsBarClr "></div>
                            <input id="steps-functionInput" type="number" step="any" name="" class="steps-slider-dragger" ng-model="step_input" ng-init="output.current_value < lowest_value ?  (step_input = '') : blabla = true" ng-change="save_StepFunction(step_input)" ng-keypress="save_StepFunction(step_input)">
                        </div>
                </div>
            </div>
        </div>

            <div class="large-8 columns large-centered text-center">
                 <includes:EraseAndCommentButtons runat="server" id="EraseAndCommentButtonsVerbal" />  
            </div>

           
    </div>
</div>      
<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/bundles/stepFunction") %>
</asp:PlaceHolder>
