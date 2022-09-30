<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UtilityCurve.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.UtilityCurve" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/EraseAndCommentButtons.ascx" TagPrefix="includes" TagName="EraseAndCommentButtons" %> 
<includes:QuestionHeader runat="server" id="QuestionHeader" />

<div class="columns tt-question-choices">
    <div class="medium-4 columns">
        <includes:LeftNodeInfoDoc runat="server" id="LeftNodeInfoDoc" />       
    </div>
    <div class="medium-4 columns">
        <includes:ParentNodeInfoDoc runat="server" id="ParentNodeInfoDoc" />    
    </div>
    <div class="medium-4 columns">
        <includes:WRTLeftNodeInfoDoc runat="server" id="WRTLeftNodeInfoDoc" />
    </div>
</div>

<%--{{output.UCData}}--%>
<div class="large-12 columns">
    <div class="tt-judgements-item large-12 columns" style="padding:0px;">
        <div class="columns tt-j-title" style="display:none;">
        </div>
        <div id="code" class="columns"></div>
        <div class="columns tt-j-content">            
            <div class="large-6 medium-11 small-12 columns small-centered text-center tt-utility-curve-wrap" style="padding:0px;">
                <div class="load-canvas-gif tt-fullwidth-loading hide" style="position:absolute; z-index:1;   top: 50%;    left: 50%;">
                    <div class="tt-loading-icon small-loading-animate"  ><span class="icon-tt-loading " ></span></div>
                </div>
                <canvas id="uCurve" class="u-curve">Your browser does not support HTML5 Canvas</canvas>
                <div data-min-value="{{output.UCData.XMinValue}}" data-max-value="{{output.UCData.XMaxValue}}" id="sliderCurve" data-sliderval="{{utilityCurveInput }}" data-disable="" onclick="" class="slider columns tt-curve-slider dsBarClr"></div>
                <input ng-change="save_utility_curve(utilityCurveInput)" id="uCurveInput" ng-model="utilityCurveInput" type="number" name="" value="" class="curve-slider-dragger">
                
            </div>
            <div class="large-12 columns text-center" style="margin-bottom:30px;">
                <includes:EraseAndCommentButtons runat="server" id="EraseAndCommentButtonsVerbal" />  
            </div>
        </div>
    </div>

    <div class="large-8 columns large-centered tt-j-clear text-center">
        <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Erase</a>
    </div>
</div>
<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/bundles/utilityCurve") %>
</asp:PlaceHolder>