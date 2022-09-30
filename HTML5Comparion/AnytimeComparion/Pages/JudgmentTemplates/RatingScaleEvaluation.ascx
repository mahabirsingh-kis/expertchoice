<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RatingScaleEvaluation.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.RatingScaleEvaluation" %>


<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>

<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">
        <span class="icon-tt-minus-circle icon"></span>
        <span class="text">Rate the preference of alternatives with respect to <a href="#">Logical thought process</a></span>
    </div>
    <div class="columns tt-accordion-content">
        <div class="panel tt-panel text-center">
            This is a demo created by Expert Coice. This is a template created by Expert Coice. You can view results on the Synthesize Results and Sensitivity Screens.


        </div>
    </div>

    <!--
    <div class="columns tt-question-title">
        <h3>Enter values from 0 to 1 for each objective</h3>
    </div>
-->

    <div class="columns tt-question-choices">
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">Candidate 1</span>
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
        <!--                        <div class="vs">VS</div>-->
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">WRT Logical thought proces</span>
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
        <div class="large-uncollapse small-collapse tt-j-content tt-rating-scale-wrap">
            <div class="large-8 medium-11 small-12 columns small-centered text-center">




                <div class="row collapse">
                    <div class="large-9 columns large-centered">
                        <div class="small-6 columns text-left">
                            0 = None
                        </div>
                        <div class="small-6 columns text-right">
                            10 = Outstanding
                        </div>
                    </div>
                    <ul class="tt-rating-scale">
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                0</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                1</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                2</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                3</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                4</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                5</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                7</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                6</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                7</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                8</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                9</label></li>
                        <li>
                            <label>
                                <input type="radio" name="radioTest"><br>
                                10</label></li>
                    </ul>
                </div>



            </div>

        </div>

        <div class="large-8 columns large-centered tt-j-clear text-center">
            <!--                            <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear Judgment</a>-->
            <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear All Judgments</a>
        </div>



        <% if (view == "anytime")
           { %>
        <div class="row">
            <div class="large-12 columns">

                <ul class="tt-drag-slider-wrap">
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Candidate 1</div>
                            <div class="medium-2 columns ">0</div>
                            <div class="medium-4 columns ">None</div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Candidate 2</div>
                            <div class="medium-2 columns ">10</div>
                            <div class="medium-4 columns ">Outstanding</div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Candidate 3</div>
                            <div class="medium-2 columns ">0</div>
                            <div class="medium-4 columns ">Not Rated</div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                </ul>
            </div>
        </div>
        <% } %>

        <% if (view == "teamtime")
           { %>

        <div class="row">
            <div class="large-12 columns">

                <ul class="tt-drag-slider-wrap">
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">You</div>
                            <div class="medium-2 columns ">0</div>
                            <div class="medium-4 columns ">None</div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">John Doe</div>
                            <div class="medium-2 columns ">10</div>
                            <div class="medium-4 columns ">Outstanding</div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                    <li>
                        <div id="1" class="large-12 columns tt-drag-slider-item">
                            <div class="medium-4 columns ">Jane Doe</div>
                            <div class="medium-2 columns ">0</div>
                            <div class="medium-4 columns ">Not Rated</div>
                            <div class="medium-2 columns"><a class="dsReset1 ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                        </div>
                    </li>
                </ul>
            </div>
        </div>
        <div class="columns tt-j-result">
            <div class="columns tt-normal-head blue text-left">
                <span class="icon-tt-chart icon"></span>
                <span class="text">GROUP RESULT</span>
            </div>

            <div class="columns">

                <!-- owner result -->
                <div class="row collapse">
                    <div class="large-12 columns tt-j-result-title">
                        <div class="medium-6 columns">Priority: 0.51</div>
                        <div class="medium-6 columns direc-comparison-wrap">
                            <div class="dc-group-result-bar" style="width: 20%;"></div>
                        </div>
                        <div class="large-12 columns text-center">Variance: 0.00</div>
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
