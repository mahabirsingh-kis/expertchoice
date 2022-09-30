<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DirectComparison.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.DirectComparison" %>



<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>

<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">
        <span class="icon-tt-minus-circle icon"></span>
        <span class="text">Measure objectives with respect to "Goal: Select Best Consultant"</span>
    </div>
    <div class="columns tt-accordion-content">
        <div class="panel tt-panel text-center">
            This is a demo created by Expert Coice. This is a template created by Expert Coice. You can view results on the Synthesize Results and Sensitivity Screens.
                          
                              
        </div>
    </div>

    <div class="columns tt-question-title">
        <h3>Enter values from 0 to 1 for each objective</h3>
    </div>

    <div class="columns tt-question-choices">
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">Able to use facts and data</span>
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
                <span class="text">WRT Goal: Select Best Consultant</span>
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
        <div class="columns tt-j-content">
            <div class="large-8 medium-11 small-12 columns small-centered text-center">




                <% if (view == "teamtime")
                   { %>
                <ul class="tt-drag-slider-wrap">
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">You</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput1" class="dsInputClr" value=""></div>
                            <div class="medium-4 columns tt-dsBar dsBar1 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="2" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">System Manager</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput2" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar2 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset2 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="3" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">John</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput3" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar3 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset3 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="4" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Mary</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput4" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar4 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset4 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="5" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Mike</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput5" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar5 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset5 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="6" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Rich</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput6" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar6 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset6 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                </ul>
                <% } %>
                <% if (view == "anytime")
                   { %>


                <ul class="tt-drag-slider-wrap">
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Able to overcome obstacles</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput1" class="dsInputClr" value=""></div>
                            <div class="medium-4 columns tt-dsBar dsBar1 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="2" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Logical thought process</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput2" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar2 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset2 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="3" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Solid Communication Skills</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput3" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar3 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset3 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="4" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Able to use facts and data</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput4" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar4 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset4 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="5" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Team Player</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput5" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar5 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset5 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="6" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Math skills</div>
                            <div class="medium-2 columns ">
                                <input type="text" readonly id="dsInput6" class="dsInputClr"></div>
                            <div class="medium-4 columns tt-dsBar dsBar6 dsBarClr"></div>
                            <div class="medium-2 columns"><a class="dsReset6 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                </ul>

                <% } %>
            </div>

        </div>

        <div class="large-8 columns large-centered tt-j-clear text-center">
            <!--                            <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear Judgment</a>-->
            <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear All Judgments</a>
        </div>


        <% if (view == "teamtime")
           { %>


                      F 
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



</div>
