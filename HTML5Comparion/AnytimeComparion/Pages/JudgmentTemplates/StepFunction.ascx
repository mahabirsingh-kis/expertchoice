<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StepFunction.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.StepFunction" %>

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
                                
                                
                                <div class="large-12 columns small-centered text-center tt-step-function-wrap">

                                        <a class="tt-toggler close cancel-tg-sf" data-toggler="tg-sf">Edit Data</a>

                                            <div class="large-8 columns small-centered text-center">
                                                <div class="row tt-edit-steps-wrap text-left tg-sf hide">
                                                <div class="columns">Scale Name: </div>
                                                <div class="medium-7 columns">
                                                    <input type="text" placeholder="Name of scale">
                                                </div>
                                                <div class="medium-5 columns">
                                                    <label class="item-list">
                                                        <input type="checkbox" class=""> <span class="">Make default name.</span>
                                                    </label>
                                               </div>
                                                <div class="row">
                                                    <div class="large-12 columns">
                                                        <div class="large-3 columns">
                                                            <label class="item-list">
                                                                <input type="checkbox" name="linearToggle"> <span class="">Piecewise Linear</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Disables step lines and curves"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                                            </label>
                                                        </div>  
                                                        <div class="large-3 columns">
                                                            <div class="small-12 columns">
                                                                <label class="item-list ">
                                                                    <input type="checkbox" name="stepsToggle" class="stepsToggle"> <span class="">Add Steps Value</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Number of steps while dragging the handle bar"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                                                </label>
                                                            </div>
                                                            <div class="small-12 columns steps-value-wrap">
                                                                <input type="number" name="stepsValue" placeholder="ex: 5" class="stepsValue"> 
                                                            </div>
                                                        </div>
                                                        <div class="large-3 columns">
                                                            <div class="small-12 columns">
                                                                <label class="item-list">
                                                                    <input type="checkbox" name="curvesToggle"> <span class="">Activate Curves</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Add curves on edges to make lines smooth"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                                                </label>
                                                            </div>
                                                            <!--<div class="small-12 columns curves-value-wrap">
                                                                <div id="curveSlider" class="columns"></div>
                                                                <input type="number" name="curvesValue" placeholder="0-100 or negative" maxlength="100" class="curvesValue left"> <a data-tooltip aria-haspopup="true" class="has-tip right" title="0 to 100 or Negative -1 to -100"><span class="icon-tt-question-circle help-icon size20"></span></a> 
                                                            </div>-->
                                                        </div>
                                                        <div class="large-3 columns">
                                                            <label class="item-list">
                                                                <input type="checkbox" name="dotsToggle"> <span class="">Toggle Dots</span><a data-tooltip aria-haspopup="true" class="has-tip" title="Show / Hide dots in the graph"><span class="icon-tt-question-circle help-icon size20"></span></a>
                                                            </label>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row panel">
                                                    <div class="small-2 columns">
                                                        <a data-tooltip aria-haspopup="true" class="has-tip tt-toggler switch-high-low nonInverted" data-toggler="tg-class-checkbox" data-class="icon-tt-high-low icon-tt-low-high" title="Switch from High to Low or viceversa"><span class="icon-tt-low-high icon"></span>
                                                            <input type="checkbox" name="highToLow" readonly class="hide">
                                                        </a>
                                                    </div>
                                                    
                                                    <div class="small-10 columns curves-value-wrap">
                                                        <div class="row collapse">
                                                            <div class="small-6 columns">Negative</div>
                                                            <div class="small-6 columns text-right"> Positive</div>
                                                            
                                                        </div>
                                                        <div id="curveSlider" class="columns"></div>
                                                        <input type="hidden" name="curvesValue" placeholder="0-100 or negative" maxlength="100" class="curvesValue"> 
<!--                                                        <a data-tooltip aria-haspopup="true" class="has-tip right" title="0 to 100 or Negative -1 to -100"><span class="icon-tt-question-circle help-icon size20"></span></a> -->
                                                    </div>
                                                    
                                                    <!--<a data-tooltip aria-haspopup="true" class="has-tip tt-toggler switch-high-low-inverted inverted hide" data-toggler="tg-class-checkbox-inverted" data-class="icon-tt-inverted-high-low icon-tt-inverted-low-high" title="Inverted High to Low or viceversa"><span class="icon-tt-inverted-low-high icon"></span>
                                                    </a>-->
                                                    
                                                    
                                                </div>
                                                    
                                                <div class="large-12 columns non-curve-content">
                                                    <table>
                                                        <thead>
<!--                                                            <th></th>-->
<!--                                                            <th>Name</th>-->
                                                            <th>Priority</th>
                                                            <th>Lower Bound Data</th>
                                                        </thead>
                                                        <tbody class="data-steps-content">
                                                            <tr>
<!--                                                                <td>A</td>-->
<!--                                                                <td><input type="text" name="" class="inline-edit"></td>-->
                                                                <td>
                                                                    <input type="text" name="lowerBound" class="inline-edit" placeholder="ex: 0,10,20,40,50, 80,90,100" value="0,10,20,30, 40,50,60,70, 80,90,100">
                                                                    <!--<div id="sliderCurveHandle" class="columns"></div>
                                                                    <hr>
                                                                    <input type="text" id="amount" readonly style="border:0; color:#f6931f; font-weight:bold;">-->
                                                                </td>
                                                                <td><input type="text" name="priority" class="inline-edit" placeholder="ex: 0,10,20,40,50, 80,90,100" value="0,10,20,30, 40,50,60,70, 80,90,100"></td>
                                                            </tr>
                                                        </tbody>
                                                    </table>

                                                    
<!--                                                    <a data-tooltip aria-haspopup="true" class="has-tip right add-data-steps-btn" title="Add data row"><span class="icon-tt-plus-circle icon"></span></a>-->
                                               </div>
                                                <div class="row collapse">
                                                    <hr>
                                                    <div class="medium-9 columns">
                                                            <button type="button" class="button tiny tt-button-icond-left tt-button-primary info">
                                                                <span class="icon-tt-copy icon"></span>
                                                                <span class="text">Copy to clipboard</span>
                                                            </button>
                                                            <button type="button" class="button tiny tt-button-icond-left tt-button-primary info">
                                                                <span class="icon-tt-clipboard icon"></span>
                                                                <span class="text">Paste to clipboard</span>
                                                            </button>

                                                            <button type="button" class="button tiny tt-button-icond-left tt-button-primary info">
                                                                <span class="icon-tt-clipboard icon"></span>
                                                                <span class="text">Asset Priorities</span>
                                                            </button>

                                                    </div>
                                                    <div class="medium-3 columns text-right">
                                                        <!--<button type="button" class="button tiny tt-button-icond-left tt-button-primary success tt-save-steps-btn">
                                                            <span class="icon-tt-save icon"></span>
                                                            <span class="text">Save</span>
                                                        </button>-->
                                                        <button type="button" class="button tiny tt-button-icond-left tt-button-primary success cancel-sf">
                                                            <span class="icon-tt-check-circle icon"></span>
                                                            <span class="text">OK</span>
                                                        </button>

                                                    </div>

                                               </div>
</div>
                                            </div>
                                            <div class="large-5 columns small-centered text-center">
                                                <canvas id="sFunctionCanvas" class="steps-function" width="" height="">Your browser does not support HTML5 Canvas</canvas>
                                                <div id="stepsFunctionSlider" data-sliderval="10" data-disable="" onclick="" class="slider columns tt-steps-slider dsBarClr"></div>
                                                <input id="steps-functionInput" type="text" name="" value="" class="steps-slider-dragger">

                                        </div>



                                </div>

                            </div>

                        </div>

                        <div class="large-8 columns large-centered tt-j-clear text-center">
                            <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j clr-steps">Erase</a>
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