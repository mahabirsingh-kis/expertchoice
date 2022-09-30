<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PairWiseComparisonEvaluation.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.AllPairWiseComparison" %>




<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>

<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">
        <span class="icon-tt-minus-circle icon"></span>
        <span class="text">With respect to Goal: Select Best Consultant</span>
    </div>
    <div class="columns tt-accordion-content">
        <div class="panel tt-panel text-center">
            This is a demo created by Expert Choice. This is a tempalte created by Expert Choice. You can view results on a the Synthesis Results and Sensitivity Screens
                          
                              
        </div>
    </div>

    <div class="columns tt-question-title">
        <h3>Which of the two objectives below is more preferable?</h3>
    </div>

    <div class="columns tt-question-choices">
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">Able to overcome obstacles</span>
            </div>
            <div class="columns tt-accordion-content">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor. 
                </div>
            </div>
            <div class="columns tt-accordion-sub">
                <div class="columns tt-accordion-head blue">
                    <span class="icon-tt-minus-circle icon"></span>
                    <span class="text">WRT Goal: Select Best Consultant</span>
                </div>
                <div class="columns tt-accordion-content">
                    <div class="panel tt-panel text-center">
                        Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. 
                    </div>
                </div>
            </div>
        </div>
        <div class="vs">VS</div>
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <span class="icon-tt-minus-circle icon"></span>
                <span class="text">Logical thought process  </span>
            </div>
            <div class="columns tt-accordion-content">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor. 
                </div>
            </div>

            <div class="columns tt-accordion-sub">
                <div class="columns tt-accordion-head blue">
                    <span class="icon-tt-minus-circle icon"></span>
                    <span class="text">WRT Goal: Select Best Consultant</span>
                </div>
                <div class="columns tt-accordion-content">
                    <div class="panel tt-panel text-center">
                        Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. 
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<div class="large-12 columns">
    <div class="tt-judgements-item large-12 columns">
        <div class="columns tt-j-title">
            <h3>
                <% if (view == "anytime")
                   { %>
                                Your Judgment
                              <% } %>
                <% if (view == "teamtime")
                   { %>
                                Judgments
                              <% } %>
            </h3>
        </div>
        <div class="columns tt-j-content">
            <div class="small-10 columns small-centered text-center">




                <% if (design == "numerical")
                   { %>

                <div class="columns tt-j-content">
                    <div class="small-1 columns text-right">A</div>
                    <div class="small-10 columns text-center">
                        <a href="#"><span class="icon-tt-chevron-left"></span></a>
                        <ul class="tt-equalizer-numeric">
                            <li class="nine lft"><span>9</span></li>
                            <li class="eight lft"><span>8</span></li>
                            <li class="seven lft"><span>7</span></li>
                            <li class="six lft"><span>6</span></li>
                            <li class="five lft"><span>5</span></li>
                            <li class="four lft"><span>4</span></li>
                            <li class="three lft"><span>3</span></li>
                            <li class="two lft"><span>2</span></li>
                            <li class="one lft"><span>1</span></li>

                            <li class="zero mid"></li>

                            <li class="one rgt"><span>1</span></li>
                            <li class="two rgt"><span>2</span></li>
                            <li class="three rgt"><span>3</span></li>
                            <li class="four rgt"><span>4</span></li>
                            <li class="five rgt"><span>5</span></li>
                            <li class="six rgt"><span>6</span></li>
                            <li class="seven rgt"><span>7</span></li>
                            <li class="eight rgt"><span>8</span></li>
                            <li class="nine rgt"><span>9</span></li>
                        </ul>
                        <a href="#"><span class="icon-tt-chevron-right"></span></a>

                        <div class="columns ratio-box">
                            <div class="large-8 columns large-centered text-center rb-wrap">
                                <div class="medium-1 columns text-right ">
                                    <a href="#"><span class="icon-tt-back-fort"></span></a>
                                </div>
                                <div class="medium-10 columns">
                                    <input type="text" class="textbox left" value="4.300" readonly>:
                    <input type="text" class="textbox right" value="1.000" readonly>
                                </div>
                                <div class="medium-1 columns text-left">
                                    <a href="#"><span class="icon-tt-close"></span></a>
                                </div>
                            </div>

                        </div>

                    </div>
                    <div class="small-1 columns">B</div>

                </div>

                <% } %>
                <% if (design == "verbal")
                   { %>
                <div class="columns tt-j-content">

                    <div class="small-12 columns text-center">
                        <!--
              <ul class="tt-equalizer">
               
                
                <li data-bgcolor="blue" data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Extreme" class="has-tip tip-top nine lft"><span>9</span></li>
                <li data-bgcolor="red" data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Very Strong to Extreme" class="has-tip tip-top eight lft"><span>8</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Very Strong" class="has-tip tip-top seven lft"><span>7</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Strong to Very Strong" class="has-tip tip-top six lft"><span>6</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Strong" class="has-tip tip-top five lft"><span>5</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Equal to Strong" class="has-tip tip-top four lft"><span>4</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Moderate" class="has-tip tip-top three lft"><span>3</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Equal to Moderate" class="has-tip tip-top two lft"><span>2</span></li>
                
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Equal" class="has-tip tip-top zero mid"><span>1</span></li>
                
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Equal to Moderate" class="has-tip tip-top two rgt"><span>2</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Moderate" class="has-tip tip-top three rgt"><span>3</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Moderate to Strong" class="has-tip tip-top four rgt"><span>4</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Strong" class="has-tip tip-top five rgt"><span>5</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Strong to Very Strong" class="has-tip tip-top six rgt"><span>6</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Very Strong" class="has-tip tip-top seven rgt"><span>7</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Very Strong to Extreme" class="has-tip tip-top eight rgt"><span>8</span></li>
                <li data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" title="Extreme" class="has-tip tip-top nine rgt"><span>9</span></li>
              </ul>
-->
                        <ul class="tt-equalizer-levels">

                            <li title="" class="nine lft">
                                <span class="lvl-txt">Extreme</span>
                                <asp:Image ID="Image2" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-1 eight lft">
                                <span class="lvl-txt hide">Very Strong<br>
                                    to Extreme</span>
                                <asp:Image ID="Image3" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="seven lft">
                                <span class="lvl-txt">Very Strong</span>
                                <asp:Image ID="Image4" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-2 six lft">
                                <span class="lvl-txt hide">Strong to<br>
                                    Very Strong</span>
                                <asp:Image ID="Image5" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="five lft">
                                <span class="lvl-txt">Strong</span>
                                <asp:Image ID="Image6" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-3 four lft">
                                <span class="lvl-txt hide">Equal to<br>
                                    Strong</span>
                                <asp:Image ID="Image7" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="three lft">
                                <span class="lvl-txt">Moderate</span>
                                <asp:Image ID="Image8" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-4 two lft">
                                <span class="lvl-txt hide">Equal to<br>
                                    Moderate</span>
                                <asp:Image ID="Image9" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>

                            <li title="" class="zero mid">
                                <span class="lvl-txt">Equal</span>
                                <asp:Image ID="Image10" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>

                            <li title="" class="lvl-even lvl-e-5 two rgt">
                                <span class="lvl-txt hide">Equal to<br>
                                    Moderate</span>
                                <asp:Image ID="Image11" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="three rgt">
                                <span class="lvl-txt">Moderate</span>
                                <asp:Image ID="Image12" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-6 four rgt">
                                <span class="lvl-txt hide">Moderate
                                    <br>
                                    to Strong</span>
                                <asp:Image ID="Image17" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="five rgt">
                                <span class="lvl-txt">Strong</span>
                                <asp:Image ID="Image13" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-7 six rgt">
                                <span class="lvl-txt hide">Strong to<br>
                                    Very Strong</span>
                                <asp:Image ID="Image14" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="seven rgt">
                                <span class="lvl-txt">Very Strong</span>
                                <asp:Image ID="Image18" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="lvl-even lvl-e-8 eight rgt">
                                <span class="lvl-txt hide">Very Strong<br>
                                    to Extreme</span>
                                <asp:Image ID="Image15" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                            <li title="" class="nine rgt">
                                <span class="lvl-txt">Extreme</span>
                                <asp:Image ID="Image16" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                            </li>
                        </ul>

                        <ul class="tt-equalizer">

                            <li id="1" title="Extreme" class="nine lft lft-eq lft-1" data-pos="lft"><span>9</span></li>
                            <li id="2" title="Very Strong to Extreme" class="eight lft lvl-even-bar lft-eq lft-2" data-pos="lft"><span>8</span></li>
                            <li id="3" title="Very Strong" class="seven lft lft-eq lft-3" data-pos="lft"><span>7</span></li>
                            <li id="4" title="Strong to Very Strong" class="six lft lvl-even-bar lft-eq lft-4" data-pos="lft"><span>6</span></li>
                            <li id="5" title="Strong" class="five lft lft-eq lft-5" data-pos="lft"><span>5</span></li>
                            <li id="6" title="Equal to Strong" class="four lft lvl-even-bar lft-eq lft-6" data-pos="lft"><span>4</span></li>
                            <li id="7" title="Moderate" class="three lft lft-eq lft-7" data-pos="lft"><span>3</span></li>
                            <li id="8" title="Equal to Moderate" class="two lft lvl-even-bar lft-eq lft-8" data-pos="lft"><span>2</span></li>

                            <li id="9" title="Equal" class="zero mid lft-eq" data-pos="mid"><span>1</span></li>

                            <li id="8" title="Equal to Moderate" class="two rgt lvl-even-bar rgt-eq rgt-8" data-pos="rgt"><span>2</span></li>
                            <li id="7" title="Moderate" class="three rgt rgt-eq rgt-7" data-pos="rgt"><span>3</span></li>
                            <li id="6" title="Moderate to Strong" class="four rgt lvl-even-bar rgt-eq rgt-6" data-pos="rgt"><span>4</span></li>
                            <li id="5" title="Strong" class="five rgt rgt-eq rgt-5" data-pos="rgt"><span>5</span></li>
                            <li id="4" title="Strong to Very Strong" class="six rgt lvl-even-bar rgt-eq rgt-4" data-pos="rgt"><span>6</span></li>
                            <li id="3" title="Very Strong" class="seven rgt rgt-eq rgt-3" data-pos="rgt"><span>7</span></li>
                            <li id="2" title="Very Strong to Extreme" class="eight rgt lvl-even-bar rgt-eq rgt-2" data-pos="rgt"><span>8</span></li>
                            <li id="1" title="Extreme" class="nine rgt rgt-eq rgt-1" data-pos="rgt"><span>9</span></li>
                        </ul>

                        <div class="columns">
                            <!--<div class="large-12 columns">
                  Equal
                </div>-->
                        </div>

                    </div>
                    <!--            <div class="small-3 columns">B</div>-->

                </div>
                <% } %>
                <% if (view == "teamtime")
                   { %>

                <div class="columns tt-j-result ">
                    <!--<div class="columns tt-normal-head blue text-left">
    <span class="icon-tt-chart icon"></span>
    <span class="text">GROUP RESULT</span>
  </div>-->

                    <div class="columns">

                        <!-- owner result -->
                        <!--<div class="row collapse">
        <div class="medium-6 columns tt-j-result-title">
            Financal Commmitment and Viability
        </div>


        <div class="medium-6 columns tt-j-result-graph">
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

        </div>
    </div>-->
                        <!-- //owner result -->

                        <!-- Other's result -->

                        <div class="row collapse">
                            <!--
        <div class="large-12 columns panel text-center">
            Agreement Score: 66%
        </div>
-->

                            <div class="tt-j-others-result columns">
                                <!--            <div class="large-12 columns tt-j-others-result-label">Participant Name:</div>-->
                                <div class="large-12 columns tt-j-others-result-content">
                                    <div class="large-12 columns">

                                        <div class="medium-3 columns">
                                            You
                                        </div>


                                        <div class="medium-9 columns text-center">
                                            <ul class="tt-equalizer-result">
                                                <li class="eight lft "></li>
                                                <li class="seven lft "></li>
                                                <li class="six lft "></li>
                                                <li class="five lft active"></li>
                                                <li class="four lft active"></li>
                                                <li class="three lft active"></li>
                                                <li class="two lft active"></li>
                                                <li class="one lft active"></li>

                                                <li class="zero mid active"></li>

                                                <li class="one rgt"></li>
                                                <li class="two rgt"></li>
                                                <li class="three rgt"></li>
                                                <li class="four rgt"></li>
                                                <li class="five rgt"></li>
                                                <li class="six rgt"></li>
                                                <li class="seven rgt"></li>
                                                <li class="eight rgt"></li>
                                            </ul>

                                        </div>

                                    </div>
                                    <div class="large-12 columns">
                                        <div class="medium-3 columns">
                                            John Doe
                                        </div>


                                        <div class="medium-9 columns text-center">
                                            <ul class="tt-equalizer-result">
                                                <li class="eight lft "></li>
                                                <li class="seven lft "></li>
                                                <li class="six lft "></li>
                                                <li class="five lft "></li>
                                                <li class="four lft "></li>
                                                <li class="three lft "></li>
                                                <li class="two lft "></li>
                                                <li class="one lft "></li>

                                                <li class="zero mid active"></li>

                                                <li class="one rgt active"></li>
                                                <li class="two rgt active"></li>
                                                <li class="three rgt active"></li>
                                                <li class="four rgt active"></li>
                                                <li class="five rgt active"></li>
                                                <li class="six rgt"></li>
                                                <li class="seven rgt"></li>
                                                <li class="eight rgt"></li>
                                            </ul>

                                        </div>

                                    </div>
                                    <div class="large-12 columns">
                                        <div class="medium-3 columns">
                                            Jane Doe
                                        </div>


                                        <div class="medium-9 columns text-center">
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

                                                <li class="one rgt"></li>
                                                <li class="two rgt"></li>
                                                <li class="three rgt"></li>
                                                <li class="four rgt"></li>
                                                <li class="five rgt"></li>
                                                <li class="six rgt"></li>
                                                <li class="seven rgt"></li>
                                                <li class="eight rgt"></li>
                                            </ul>

                                        </div>

                                    </div>

                                </div>
                            </div>
                        </div>
                        <!-- //Other's result -->
                    </div>
                </div>


                <% } %>
            </div>

        </div>

        <div class="large-8 columns large-centered tt-j-clear text-center">
            <!--                            <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear Judgment</a>-->
            <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear All Judgments</a>
        </div>


        <% if (doThis != "DirectComparisonPriorities" && view == "teamtime")
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
                        <div class="large-6 columns">Able to overcome obstacles</div>
                        <div class="large-6 columns text-center">

                            <ul class="tt-equalizer-result">
                                <li class="eight lft "></li>
                                <li class="seven lft "></li>
                                <li class="six lft "></li>
                                <li class="five lft "></li>
                                <li class="four lft "></li>
                                <li class="three lft "></li>
                                <li class="two lft "></li>
                                <li class="one lft "></li>

                                <li class="zero mid active"></li>

                                <li class="one rgt active"></li>
                                <li class="two rgt active"></li>
                                <li class="three rgt active"></li>
                                <li class="four rgt active"></li>
                                <li class="five rgt active"></li>
                                <li class="six rgt"></li>
                                <li class="seven rgt"></li>
                                <li class="eight rgt"></li>
                            </ul>
                        </div>
                        <div class="large-6 columns">Geometric Variance: 0.24</div>
                        <div class="large-6 columns text-center">
                            <div class="ratio-box">
                                <div class="large-12 columns text-center rb-wrap">

                                    <div class="large-8 columns large-centered">
                                        <input type="text" class="textbox left" value="4.300" readonly>:
                                <input type="text" class="textbox right" value="1.000" readonly>
                                    </div>

                                </div>

                            </div>
                        </div>
                    </div>


                    <div class="medium-6 columns tt-j-result-graph hide">

                        <div class="columns tt-j-result-comment"></div>
                    </div>
                </div>
                <!-- //owner result -->


            </div>
        </div>


        <% } %>
    </div>



</div>
