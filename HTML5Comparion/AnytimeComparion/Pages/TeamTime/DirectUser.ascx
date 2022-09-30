<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DirectUser.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.DirectUser" className="DirectUser" %>

  
  
  
<div class="large-12 columns">
                    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
                    <div class="columns tt-accordion-head blue">
                        <span class="icon-tt-minus-circle icon"></span>
                        <span class="text">Measure objectives with respect to <asp:Label ID="goals" runat="server"></asp:Label></span>
                    </div>
                    <div class="columns tt-accordion-content">
                        <div class="panel tt-panel text-center">
                            This is a demo created by Expert Coice. This is a template created by Expert Coice. You can view results on the Synthesize Results and Sensitivity Screens.
                          
                              
                        </div>
                    </div>

                    <div class="columns tt-question-title">
                        <h3>Enter a value between 0 and 1 to indicate how well <asp:Label ID="alternativeLbl" runat="server"></asp:Label> contributes to <asp:Label ID="objectLbl" runat="server"></asp:Label></h3>
                    </div>

                    
<div class="large-12 columns">
                    <div class="tt-judgements-item large-12 columns">
                        <div class="columns tt-j-title">
                            <h3>
                                Judgments
                            </h3>
                        </div>
                        <div class="columns tt-j-content">
                            <div class="large-8 medium-11 small-12 columns small-centered text-center">


                               
  <ul class="tt-drag-slider-wrap">
    <li>
      <div id="1" class="large-12 columns tt-drag-slider-item">
        <div class="medium-4 columns ">You</div> 
        <div class="medium-2 columns "><input type="text" readonly  id="dsInput1" class="dsInputClr" value=""></div>
        <div class="medium-4 columns tt-dsBar dsBar1 dsBarClr"></div>
        <div class="medium-2 columns"><a class="dsReset1 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
      </div>
    </li>
    <li>
      <div id="2" class="large-12 columns tt-drag-slider-item">
        <div class="medium-4 columns ">System Manager</div> 
        <div class="medium-2 columns "><input type="text" readonly  id="dsInput2" class="dsInputClr"></div>
        <div class="medium-4 columns tt-dsBar dsBar2 dsBarClr"></div>
        <div class="medium-2 columns"><a class="dsReset2 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
      </div>
    </li>
    <li>
      <div id="3" class="large-12 columns tt-drag-slider-item">
        <div class="medium-4 columns ">John</div> 
        <div class="medium-2 columns "><input type="text" readonly  id="dsInput3" class="dsInputClr"></div>
        <div class="medium-4 columns tt-dsBar dsBar3 dsBarClr"></div>
        <div class="medium-2 columns"><a class="dsReset3 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
      </div>
    </li>
    <li>
      <div id="4" class="large-12 columns tt-drag-slider-item">
        <div class="medium-4 columns ">Mary</div> 
        <div class="medium-2 columns "><input type="text" readonly  id="dsInput4" class="dsInputClr"></div>
        <div class="medium-4 columns tt-dsBar dsBar4 dsBarClr"></div>
        <div class="medium-2 columns"><a class="dsReset4 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
      </div>
    </li>
    <li>
      <div id="5" class="large-12 columns tt-drag-slider-item">
        <div class="medium-4 columns ">Mike</div> 
        <div class="medium-2 columns "><input type="text" readonly  id="dsInput5" class="dsInputClr"></div>
        <div class="medium-4 columns tt-dsBar dsBar5 dsBarClr"></div>
        <div class="medium-2 columns"><a class="dsReset5 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
      </div>
    </li>
    <li>
      <div id="6" class="large-12 columns tt-drag-slider-item">
        <div class="medium-4 columns ">Rich</div> 
        <div class="medium-2 columns "><input type="text" readonly  id="dsInput6" class="dsInputClr"></div>
        <div class="medium-4 columns tt-dsBar dsBar6 dsBarClr"></div>
        <div class="medium-2 columns"><a class="dsReset6 ds-res-btn hide" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
      </div>
    </li>
  </ul>



                            </div>

                        </div>
                      
                        <div class="large-8 columns large-centered tt-j-clear text-center">
<!--                            <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear Judgment</a>-->
                            <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear All Judgments</a>
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
                    </div>
                  
                    

                </div>