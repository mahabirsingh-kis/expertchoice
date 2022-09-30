<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UtilityCurve.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.UtilityCurve" %>


<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>



<div class="large-12 columns ">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">
        <span class="icon-tt-minus-circle icon"></span>
        <span class="text">Data: "Candidate 3" with respect to: <a href="#">Logical thought process</a></span>
    </div>
    <div class="columns tt-accordion-content">
        <div class="panel tt-panel text-center">
            Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi.


        </div>
    </div>

    <!--<div class="columns tt-question-title">
        <h3>Enter values from 0 to 1 for each objective</h3>
    </div>-->

    <div class="columns tt-question-choices">
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">Candidate 3</span>
            </div>
            <div class="columns tt-accordion-content">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor. 
                </div>
            </div>
            <!--<div class="columns tt-accordion-sub">
              <div class="columns tt-accordion-head blue">
                  <span class="icon-tt-minus-circle icon"></span>
                  <span class="text">WRT Goal: Select Best Consultant</span>
              </div>
              <div class="columns tt-accordion-content">
                  <div class="panel tt-panel text-center">
                      Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. 
                  </div>
              </div>
            </div>-->
        </div>
        <div class="vs">VS</div>
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">WRT Logical thought process</span>
            </div>
            <div class="columns tt-accordion-content">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor. 
                </div>
            </div>

            <!--<div class="columns tt-accordion-sub">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">WRT Avoid Selling Shares</span>
            </div>
            <div class="columns tt-accordion-content">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. 
                </div>
            </div>
          </div>-->
        </div>
    </div>
</div>

<div class="large-12 columns">
    <div class="tt-judgements-item large-12 columns">
        <div class="columns tt-j-title">
            <h3>
                <% if (view == "anytime")
                   { %>Your Judgment<% } %>
                <% if (view == "teamtime")
                   { %>Judgments<% } %>
            </h3>
        </div>
        <div id="code" class="columns"></div>
        <div class="columns tt-j-content">
            <div class="large-5 small-12 columns small-centered text-center tt-utility-curve-wrap">



                    
                    <canvas id="uCurve" class="u-curve" width="" height="">Your browser does not support HTML5 Canvas</canvas>
                    
                    
                    <div id="sliderCurve" data-sliderval="10" data-disable="" onclick="" class="slider columns tt-curve-slider dsBarClr"></div>
                    <input id="uCurveInput" type="text" name="" value="" class="curve-slider-dragger">
            </div>

        </div>

    </div>

    <div class="large-8 columns large-centered tt-j-clear text-center">
        <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Erase</a>
    </div>


    <% if (view == "teamtime")
       { %>


    <div class="columns tt-j-result">
        <div class="columns tt-normal-head blue text-left">
            <span class="icon-tt-chart icon"></span>
            <span class="text">GROUP RESULT</span>
        </div>

        <div class="columns">

            <!-- owner result -->
            <div class="row collapse">
                <div class="large-12 columns tt-j-result-title">
                    <div class="large-3 columns">Priority: 0.51</div>
                    <div class="large-4 columns direc-comparison-wrap">
                        <div class="dc-group-result-bar" style="width: 20%;"></div>
                    </div>
                    <div class="large-5 columns text-center">Variance: 0.00</div>
                </div>


                <div class="medium-6 columns tt-j-result-graph hide">
                    <ul class="tt-equalizer-result">
                        <li class="eight lft active"></li>
                        <li class="seven lft active"></li>
                        <li class="six lft active"></li>
                        <li class="five lft active"></li>
                        <li class="four lft active"></li>
                        <li class="three lft active"></li>
                        <li class="two lft active"></li>
                        <li class="one lft active"></li>

                        <li class="zero mid active"></li>

                        <li class="one rgt active"></li>
                        <li class="two rgt active"></li>
                        <li class="three rgt active"></li>
                        <li class="four rgt active"></li>
                        <li class="five rgt active"></li>
                        <li class="six rgt active"></li>
                        <li class="seven rgt active"></li>
                        <li class="eight rgt active"></li>
                    </ul>

                    <div class="columns tt-j-result-comment"></div>
                </div>
            </div>
            <!-- //owner result -->

        </div>
    </div>


    <% } %>
</div>
