<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Judgement.aspx.cs" Inherits="AnytimeComparion.Pages.WebForm1" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%-- if url is ?do=theMeasurement&view=theView&style=theStyle&design=theDesign --%>


    <% var doThis = Request.QueryString["doThis"];  %>
    <% var view = Request.QueryString["view"];  %>
    <% var style = Request.QueryString["style"];  %>
    <% var design = Request.QueryString["design"];  %>

    <!-- body -->
    <div class="tt-body">
        <% if (Session["User"] != null || Session["UserType"] != null){ %>
        <div class="row collapse tt-2-cols">
            <div class="large-12 columns tt-left-col">
                
                <!-- START top nav -->
                <%--<div class="large-12 columns tt-judgements-footer-nav">
                    <%@ Register TagPrefix="includes" TagName="jfoterNav_top" Src="~/Pages/includes/JudgmentsFooterNav.ascx" %>
                    <includes:jfoterNav_top ID="jfoterNav_top" runat="server" />
                </div>--%>
                <!-- END top nav -->
                    
                <div class="columns the-contents"> 
                    <% if (doThis == "Graphical")
                   { %>
                        
                    

                    <div class="large-8 large-centered columns graphical-slider-wrap tt-pizza-wrap text-center"> 
                        
                        <div class="row">
                            
                            <div id="" class="a_Slider small-8 columns dual-slider-wrap">
                                <input name="a-value" class="number-input" type="number">
                                <span class="a-content-left content-texts">50%</span> 
                            </div>
                        
                        </div>
                        
                        <div class="row">
                            
                            <div id="" class="b_Slider small-8 columns dual-slider-wrap">
                                <input name="b-value" class="number-input" type="number">
                                <span class="b-content-left content-texts">50%</span> 
                            </div>
                        
                        </div>
                        
                    
                    </div>
                      
                        <script>
                          
                           
                                $(function() {
                                      $( ".a_Slider" ).slider({
                                            orientation: "horizontal",
                                            range: "min",
                                            max: 1600,
                                            value: 0,
                                            slide: function (event, ui) {
                                                $( ".b_Slider" ).slider('value', 1600 - ui.value);
                                                $("input[name='a-value']").val(ui.value);
                                                $(".a-content-left").text(ui.value + '%');
                                                var handlePosition = $('.ui-slider-handle').position();
                                                console.log(handlePosition);
                                            }
                                      });
                                });

                            
                            $(function() {
                                      $( ".b_Slider" ).slider({
                                            orientation: "horizontal",
                                            range: "min",
                                            max: 1600,
                                            value: 1600,
                                            slide: function (event, ui) {
                                                $(".a_Slider").slider('value', 1600 - ui.value);
                                                $("input[name='b-value']").val(ui.value);
                                                $(".b-content-left").text(ui.value + '%');

                                            }
                                      });
                                });


                          </script>
                     <% } %>
                    
                <% if (doThis == "DirectComparison")
                   { %>
                <%-- DirectComparison --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="DirectComparison" Src="~/Pages/JudgmentTemplates/DirectComparison.ascx" %>
                <JudgmentTemplates:DirectComparison ID="DirectComparison" runat="server" />
                <% } %>

                <% if (doThis == "DirectComparisonEvaluation")
                   { %>
                <%-- DirectComparison --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="DirectComparisonEvaluation" Src="~/Pages/JudgmentTemplates/DirectComparisonEvaluation.ascx" %>
                <JudgmentTemplates:DirectComparisonEvaluation ID="DirectComparisonEvaluation" runat="server" />
                <% } %>


                <% if (doThis == "DirectComparisonPrioritiesEvaluation")
                   { %>
                <%-- DirectComparison --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="DirectComparisonPrioritiesEvaluation" Src="~/Pages/JudgmentTemplates/DirectComparisonPrioritiesEvaluation.ascx" %>
                <JudgmentTemplates:DirectComparisonPrioritiesEvaluation ID="DirectComparisonPrioritiesEvaluation" runat="server" />
                <% } %>

                <% if (doThis == "DirectComparisonPriorities")
                   { %>
                <%-- DirectComparison --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="DirectComparisonPriorities" Src="~/Pages/JudgmentTemplates/DirectComparisonPriorities.ascx" %>
                <JudgmentTemplates:DirectComparisonPriorities ID="DirectComparisonPriorities" runat="server" />
                <% } %>


                <% if (doThis == "PairWiseComparison")
                   { %>
                <%-- PairWiseComparison --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="PairWiseComparison" Src="~/Pages/JudgmentTemplates/PairWiseComparison.ascx" %>
                <JudgmentTemplates:PairWiseComparison ID="PairWiseComparison" runat="server" />
                <% } %>

                <% if (doThis == "PairWiseComparisonEvaluation")
                   { %>
                <%-- PairWiseComparisonEvaluation --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="PairWiseComparisonEvaluation" Src="~/Pages/JudgmentTemplates/PairWiseComparisonEvaluation.ascx" %>
                <JudgmentTemplates:PairWiseComparisonEvaluation ID="PairWiseComparisonEvaluation" runat="server" />
                <% } %>

                <% if (doThis == "AllPairWiseComparison")
                   { %>
                <%-- AllPairWiseComparison --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="AllPairWiseComparison" Src="~/Pages/JudgmentTemplates/AllPairWiseComparison.ascx" %>
                <JudgmentTemplates:AllPairWiseComparison ID="AllPairWiseComparison" runat="server" />
                <% } %>

                <% if (doThis == "RatingScale")
                   { %>
                <%-- RatingScale --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="RatingScale" Src="~/Pages/JudgmentTemplates/RatingScale.ascx" %>
                <JudgmentTemplates:RatingScale ID="RatingScale" runat="server" />
                <% } %>

                <% if (doThis == "RatingScaleEvaluation")
                   { %>
                <%-- RatingScaleEvaluation --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="RatingScaleEvaluation" Src="~/Pages/JudgmentTemplates/RatingScaleEvaluation.ascx" %>
                <JudgmentTemplates:RatingScaleEvaluation ID="RatingScaleEvaluation" runat="server" />
                <% } %>

                <% if (doThis == "UtilityCurve")
                   { %>
                <%-- UtilityCurve --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="UtilityCurve" Src="~/Pages/JudgmentTemplates/UtilityCurve.ascx" %>
                <JudgmentTemplates:UtilityCurve ID="UtilityCurve" runat="server" />
                <% } %>

                <% if (doThis == "UtilityCurveEvaluation")
                   { %>
                <%--  UtilityCurveEvaluation--%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="UtilityCurveEvaluation" Src="~/Pages/JudgmentTemplates/UtilityCurveEvaluation.ascx" %>
                <JudgmentTemplates:UtilityCurveEvaluation ID="UtilityCurveEvaluation" runat="server" />
                <% } %>

                <% if (doThis == "StepFunction")
                   { %>
                <%-- StepFunction --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="StepFunction" Src="~/Pages/JudgmentTemplates/StepFunction.ascx" %>
                <JudgmentTemplates:StepFunction ID="StepFunction" runat="server" />
                <% } %>

                <% if (doThis == "StepFunctionEvaluation")
                   { %>
                <%-- StepFunctionEvaluation --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="StepFunctionEvaluation" Src="~/Pages/JudgmentTemplates/StepFunctionEvaluation.ascx" %>
                <JudgmentTemplates:StepFunctionEvaluation ID="StepFunctionEvaluation" runat="server" />
                <% } %>


                <% if (doThis == "Priorities")
                   { %>
                <%-- Priorities --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="Priorities" Src="~/Pages/JudgmentTemplates/Priorities.ascx" %>
                <JudgmentTemplates:Priorities ID="Priorities" runat="server" />
                <% } %>

                <% if (doThis == "InformationPage")
                   { %>
                <%-- InformationPage --%>
                <%@ Register TagPrefix="JudgmentTemplates" TagName="InformationPage" Src="~/Pages/JudgmentTemplates/InformationPage.ascx" %>
                <JudgmentTemplates:InformationPage ID="InformationPage" runat="server" />
                <% } %>
                </div>

                <!-- START footer nav bottom -->
                <div class="large-12 columns tt-judgements-footer-nav">
                    <%--<%@ Register TagPrefix="includes" TagName="jfoterNav_bottom" Src="~/Pages/includes/JudgmentsFooterNav.ascx" %>
                    <includes:jfoterNav_bottom ID="jfoterNav_bottom" runat="server" />--%>
                        
                    <!-- START page details -->
                    <div class="columns">
                        <div class="large-12 columns" data-equalizer>


                        <div class="medium-6 columns" data-equalizer-watch>
                            <label>
                                <input type="checkbox" name="">
                                <span>Auto Advance</span>
                            </label>

                            <label>
                                <input type="checkbox" name="">
                                <span>Anonymous Mode</span>
                            </label>
                        </div>




                        <div class="medium-6  tt-overall-result-wrap columns" data-equalizer-watch>

                            <div class="columns">
                                <label>
                                    <input type="checkbox" name="cb-showOverallResult">
                                    <span>Show Overall Result</span>
                                </label>
                            </div>

                            <div class="columns tt-j-show-overall-result">
                                <div class="all">
                                    <label>
                                        <input type="radio" name="all-me">
                                        <span>All</span></label>
                                </div>
                                <div class="me-only">
                                    <label>
                                        <input type="radio" name="all-me">
                                        <span>Me only</span></label>
                                </div>
                            </div>

                            <div class="columns tt-j-stat-part">
                                <div class="panel">
                                    Time since last judgement: <span class="time">5:34</span>
                                    <br>
                                    Last Request: <span class="time">8:58:26 PM</span>
                                    <br>
                                    Current Time: <span class="time">9:01:28 PM</span>
                                    <br>
                                    Failed Requests: <span class="time">0</span>
                                </div>
                            </div>

                        </div>

</div>
                    </div>
                    <!-- END page details -->
                        
                </div>
                <!-- END footer nav bottom -->
                    
            </div>



            <div class="large-3 columns tt-comments-wrap hide tt-right-col">

                <!-- sidebar-->
               <%-- <%@ Register TagPrefix="MainPages" TagName="SidebarRight" Src="~/Pages/sidebar.ascx" %>
                <MainPages:SidebarRight ID="SidebarRight" runat="server" /--%>>

            </div>

        </div>
        <% }
           else
           { %>
        <div class="row">
            <div class="large-12 columns text-center">
                <%@ Register TagPrefix="includes" TagName="loginmessage" Src="~/Pages/includes/NeedToLogin.ascx" %>


                <includes:loginmessage ID="needToLogin" runat="server" />
            </div>
        </div>
        <% } %>
    </div>
            
    <!-- modal wrap -->
    <div id="tt-h-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        <h2 id="modalTitle" class="tt-modal-header blue-header">Hierarchy</h2>

    <div class="tt-modal-content">
            <div class="row">
                <div class="large-12 columns">
            <ul class="tt-site-map-wrap">
                <li data-toggler-id="p1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Goal: Improve Aircraft and Passenger Flight Security - Commercial Aviation</li>
                <ul class="tg-sm-p1">
                    <!-- first item -->
                    <li data-toggler-id="s1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Financial Impact Considerations</li>
                    <ul class="tg-sm-s1">
                        <li><span class="icon-tt-disabled tt-sm-icon"></span>Research & Development Costs</li>
                        <li data-toggler-id="s1-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Implementation or Deployment Costs</li>
                        <ul class="tg-sm-s1-1">
                            <li data-toggler-id="s1-1-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Training</li>
                            <ul class="tg-sm-s1-1-1">
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Comprehensive Training Program Development</li>
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Administrater Initail Training</li>
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Periodic Retraining & Updates (Annual et al)</li>
                            </ul>
                            <li data-toggler-id="s1-1-2" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Equipment & Systems Installations</li>
                            <ul class="tg-sm-s1-1-2">
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Initial Installation Costs</li>
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Systems Repalcement Costs</li>
                            </ul>
                        </ul>
                    </ul>
                </ul>
                <!-- //first item -->
                <!-- second item -->
                <li data-toggler-id="p2" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Second Item</li>
                <ul class="tg-sm-p2">
                    <li data-toggler-id="s2" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Lorem header</li>
                    <ul class="tg-sm-s2">
                        <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub</li>
                        <li data-toggler-id="s2-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Lorem sub header</li>
                        <ul class="tg-sm-s2-1">
                            <li data-toggler-id="s2-1-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Lorem sub header item</li>
                            <ul class="tg-sm-s2-1-1">
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub header item > item</li>
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub header item > item</li>
                                <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub header item > item</li>
                            </ul>

                        </ul>
                    </ul>
                </ul>
                <!-- //second item -->


            </ul>
                </div>
            </div>
        </div>

    </div>
    <!-- // modal wrap -->
    <!-- modal wrap -->
    <div id="tt-s-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        <h2 id="modalTitle" class="tt-modal-header blue-header">Steps <a href="#" data-reveal-id="tt-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>
        <div class="tt-modal-content">
            <ul class="hide">
                <li class="notice"><a runat="server" href="~/Judgement/direct-comparison">#1: Direct Comparison (collect input)</a></li>
            </ul>


<%--            <%@ Register TagPrefix="includes" TagName="temporaryList" Src="~/Pages/includes/temporaryList.ascx" %>
            <includes:temporaryList ID="Prioritities" runat="server" />--%>

        </div>
    </div>
    <!-- // modal wrap -->

    <!-- help modal wrap -->
    <div id="tt-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
        <!--                                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>-->
        <a class='open-first close-reveal-modal second-modal-btn'>&#215;</a>
        <h2 id="modalTitle" class="tt-modal-header blue-header">Judgment Links Info</h2>
        <div class="tt-modal-content large-12 columns">

            <div class="row">
                <p>The current step is displayed with an <span class="label orange">orange</span> background. The step numbers are colored as follows:</p>
            </div>
            <div class="row">
                <div class="small-2 columns">Red </div>
                <div class="small-10 columns">- judgement has not yet been made</div>
                <div class="small-2 columns">Blue </div>
                <div class="small-10 columns">- results or information steps</div>
                <div class="small-2 columns">Black </div>
                <div class="small-10 columns">- Judgement has been made</div>
            </div>
        </div>
    </div>
    <!-- //help modal wrap -->
        
        
    <!-- // body -->
</asp:Content>

