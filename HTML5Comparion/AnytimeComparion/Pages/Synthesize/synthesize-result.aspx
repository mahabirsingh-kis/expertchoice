<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="synthesize-result.aspx.cs" Inherits="AnytimeComparion.Pages.synthesize_result" %>


    <asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

        <div ng-controller="SynthesizeController">
            <!--<div class="row">
                <div class="large-12 large-centered columns text-centered" ng-include="currentPage">
                </div>
            </div>-->

            <!-- START landing page of synthesize result -->
           <%-- <div class="tt-body hide">
                <% if (Session["User"] != null || Session["UserType"] != null){ %>
                    <div class="row collapse tt-2-cols ">

                        <!-- start tt-synthesize-wrap -->
                        <div class="large-12 columns tt-synthesize-wrap">
                            <div class="row">
                                <div class="panel cta large-12 columns">
                                    <div class="columns">
                                        <h2>What is Synthesize Result?</h2>
                                        <p>
                                            A synthesis (combining) or the measures according to the objectives hierarchy follows the structuring and measurement steps is done automatically by Expert Choice. This synthesis is quite unique (as for as models go) since it includes both objective information (based on whatever hard data is available) as well as subjectivity in the form of knowledge, experience, and judgment of the participants. If you have more questions, please visit out <a href="#" class="help">HELP SECTION</a>
                                        </p>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="large-6 columns">
                                    <section class="large-12 columns">
                                        <div class="columns tt-header-nav blue-header">Evalutation Progress <a href="#" data-dropdown="help-ep" aria-controls="help-ep" aria-expanded="false"><span class="icon-tt-question-circle"></span></a></div>
                                        <div id="help-ep" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                            <div class="columns">This is the help section for evalutaion progress</div>
                                        </div>
                                        <div class="medium-6 columns">
                                            <div class="nice radius success progress"><span class="meter" style="width: 50%"><span class="text">50%</span></span>
                                            </div>
                                        </div>
                                        <div class="medium-6 columns medium-text-right small-text-center">
                                            Overall Evalutation Progress
                                        </div>
                                    </section>

                                    <section class="large-12 columns">
                                        <div class="columns tt-header-nav blue-header">OVERALL RESULTS <a href="#" data-dropdown="help-or" aria-controls="help-or" aria-expanded="false"><span class="icon-tt-question-circle"></span></a></div>
                                        <div id="help-or" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                            <div class="columns">This is the help section for OVERALL RESULTS</div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-alternatives-grid"></span></a>
                                                <br>Alternatives Grid</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-alternatives-chart"></span></a>
                                                <br>Alternatives Chart</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-objectives-grid"></span></a>
                                                <br>Objectives Grid</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-objectives-chart"></span></a>
                                                <br>Objectives Chart</div>
                                        </div>
                                    </section>

                                </div>
                                <div class="large-6 columns">
                                    <section class="large-12 columns">
                                        <div class="columns tt-header-nav blue-header">SENSITIVITY <span class="icon-tt-delta-symbol"></span> OBJECTIVES <a href="#" data-dropdown="help-so" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-question-circle"></span></a></div>
                                        <div id="help-so" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                            <div class="columns">This is the help section for SENSITIVITY OBJECTIVES</div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-dynamic-graph"></span></a>
                                                <br>Dynamic</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-performance-graph"></span></a>
                                                <br>Performance</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-gradient-graph"></span></a>
                                                <br>Gradient</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-2d-graph"></span></a>
                                                <br>2D</div>
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-head-to-head-graph"></span></a>
                                                <br>Head to Head Analysis</div>
                                            <div class="medium-3 small-2 columns left"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-mixed-graph"></span></a>
                                                <br>Mixed</div>
                                        </div>

                                    </section>

                                </div>
                            </div>

                            <div class="row">
                                <div class="large-6 columns">
                                    <section class="large-12 columns">
                                        <div class="columns tt-header-nav blue-header">SENSITIVITY <span class="icon-tt-delta-symbol"></span> ALTERNATIVES <a href="#" data-dropdown="help-sa" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-question-circle"></span></a></div>
                                        <div id="help-sa" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                            <div class="columns">This is the help section for SENSITIVITY ALTERNATIVES</div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-chart-1x"></span></a>
                                                <br>One at a time</div>
                                            <div class="medium-3 small-2 columns left"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-chart-4x"></span></a>
                                                <br>Four at a time</div>
                                        </div>
                                    </section>



                                </div>
                                <div class="large-6 columns">
                                    <section class="large-12 columns">
                                        <div class="columns tt-header-nav blue-header">OTHERS <a href="#" data-dropdown="help-others" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-question-circle"></span></a></div>
                                        <div id="help-others" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                            <div class="columns">This is the help section for OTHERS</div>
                                        </div>
                                        <div class="row collapse">
                                            <div class="medium-3 small-2 columns"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-alternatives-grid"></span></a>
                                                <br>Data Grid</div>
                                            <div class="medium-3 small-2 columns left"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-alternatives-grid"></span></a>
                                                <br>Consensus View</div>
                                            <div class="medium-3 small-2 columns left"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-chart-inconsistency"></span></a>
                                                <br>Inconsistency</div>
                                            <div class="medium-3 small-2 columns left"><a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-survey-results"></span></a>
                                                <br>Survey Results</div>
                                        </div>

                                    </section>

                                </div>
                            </div>
                            <!-- end tt-synthesize-wrap -->




                        </div>

                        <% } else { %>
                            <div class="row">
                                <div class="large-12 columns text-center">

                                    <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
                                        <includes:loginMessage ID="needToLogin" runat="server" />

                                </div>
                            </div>
                    </div>
                    <% } %>
            </div> --%>
            <!-- END landing page of synthesize result -->


            <!-- START INNER page of synthesize result -->
            <div class="tt-body tt-synthesize-content">
                <% if (Session["User"] != null || Session["UserType"] != null){ %>
                    <div class="row collapse tt-2-cols">

                        <div class="large-12 columns tt-left-col">
                            <div class="large-12 columns">
                                <div class="columns tt-header-nav blue-header">
                                    <ul class="breadcrumbs">
                                        <li><a href="../synthesize-result"> Synthesize Results</a></li>
                                        <li class="current">
                                            <span>SENSITIVITY <span class="icon-tt-delta-symbol"></span> OBJECTIVES
                                                <a href="#" class="help collect-input" data-dropdown="help-dd" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                            </span>
                                        </li>
                                        <li class="tt-with-button">
                                            <a class="button tt-button-primary tt-dropdown" data-dropdown="output-dd" aria-controls="moreActions-dd" aria-expanded="false">
                                                <span class="text">Output </span>
                                                <span class="icon-tt-chevron-down drop-arrow"></span>
                                            </a>
                                            
                                        </li>
                                    </ul>
                                </div>


                                <!-- output-dd dropdown -->
                                <ul id="output-dd" class="f-dropdown" data-dropdown-content="" aria-hidden="true" tabindex="-1">
                                    <li><a href="#">Lorem</a></li>
                                    <li><a href="#">Ipsum</a></li>
                                    <li><a href="#">Dolor</a></li>
                                    <li><a href="#">Sit</a></li>
                                    <li><a href="#">Amet</a></li>
                                </ul>
                                <!-- moreActions-dd dropdown -->

                                <!-- help dropdown -->
                                <div id="help-dd" class="f-dropdown" data-dropdown-content="" aria-hidden="true" tabindex="-1">
                                    <p>
                                        Lorem ipsum dolor sit amet. aman pulo ditr yu imusta putri
                                    </p>
                                </div>
                                <!-- help-dd dropdown -->

                            </div>
                            <div class="large-12 columns tt-tabs-wrap-results">
                                <ul class="tabs" data-tab>
                                  <li class="tab-title "><a href="#dynamic">Dynamic</a></li>
                                  <li class="tab-title"><a href="#performance">Performance</a></li>
                                  <li class="tab-title"><a href="#gradient">Gradient</a></li>
                                  <li class="tab-title"><a href="#two-d">2D</a></li>
                                  <li class="tab-title active"><a href="#head-to-head">Head to Head</a></li>
                                  <li class="tab-title"><a href="#mixed">Mixed</a></li>
                                </ul>
                                <div class="tabs-content">
                                  <div class="content active" id="head-to-head">
                                    <div class="row collapse tt-tabs-header-menus">
                                        <div class="large-12 columns">
                                            <ul>
                                                <li>
                                                    <a class="button tt-button-primary">
                                                        <span class="icon-tt-restore"></span>
                                                        <span class="text">Reset </span>
                                                    </a>
                                                </li>
                                                <li>
                                                    <a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-users"></span></a>
                                                </li>
                                                
                                                <li>
                                                    <a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-users-config"></span></a>
                                                </li>
                                                <li>
                                                    <a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-pie-chart-filter"></span></a>
                                                </li>
                                                <li>
                                                    <a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-database-filter"></span></a>
                                                </li>
                                                <li>
                                                    <a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-back-fort"></span></a>
                                                </li>
                                                <li>
                                                    <a href="#" data-tooltip class="has-tip" data-options="disable_for_touch:true" title="Shows the -add description here-"><span class="icon icon-tt-move-arrow"></span></a>
                                                </li>
                                                <li>
                                                    <a class="button tt-button-primary tt-dropdown" data-dropdown="advanced-view" aria-controls="advanced-view" aria-expanded="false">
                                                        <span class="text">Advanced View </span>
                                                        <span class="icon-tt-chevron-down drop-arrow"></span>
                                                    </a>
                                                    <ul id="advanced-view" class="f-dropdown" data-dropdown-content="" aria-hidden="true" tabindex="-1">
                                                        <li><a href="#">Lorem</a></li>
                                                        <li><a href="#">Ipsum</a></li>
                                                        <li><a href="#">Dolor</a></li>
                                                        <li><a href="#">Sit</a></li>
                                                        <li><a href="#">Amet</a></li>
                                                    </ul>
                                                </li>
                                            </ul>
                                        </div>
                                        <div class="tt-resize-div-wrap">
                                            <div class="large-4 columns tt-sr-left">
                                                
                                                <h3>All Objectives</h3>
                                                <div class="large-12 columns tt-results-nav-wrap text-left">
                                                    <div class="row collapse" >
                                                        <span style="color:black">Obectives Name</span>
                                                    </div>

                                                    <div class="row collapse" >
                                                        <!-- start parent 1 -->
 

                                                        <!-- start siblings 2-->
                                                        <ul class="large-12 columns tt-sw-ul tt-r-child tg-tree-nav-1" >        
                                                                    <li class="tt-sw-ul-li with-sub" >
                                                                        <div class="row collapse parent-tree  ">
                                                                            
                                                                            <div class="small-12 columns" title=" ">
                                                                                    <a id="1" href="#" class="left tt-toggler" data-toggler="tg-tree-nav-sub-first"><span class="icon icon-tt-plus-square"></span></a>
                                                                                <a href="#" >test1</a>
                                                                            </div>
                                                                            
                                                                        </div>

                                                                        <!-- start sub first level-->
                                                                            <ul class="tt-sw-ul tt-r-child sub tg-tree-nav-sub-first-1" >        
                                                                                    <li class="tt-sw-ul-li" >
                                                                                        <div class="row collapse parent-tree first-sub ">
                                                                                            <div class="small-12 columns">
                                                                                           
                                                                                                    <a id="1" href="#" class="left tt-toggler" data-toggler="tg-tree-nav-sub-second"><span class="icon icon-tt-plus-square"></span></a>
                                                                                                <a href="#"  >test2</a>
                                                                                            </div>
                                                                                            
                                                                                        </div>

                                                                                         <!-- start sub second level-->
                                                                                            <ul class="tt-sw-ul tt-r-child sub tg-tree-nav-sub-second-1" >        
                                                                                                    <li class="tt-sw-ul-li" >
                                                                                                        <div class="row collapse parent-tree second-sub">
                                                                                                            
                                                                                                            <div class="small-12 columns">
                                                                                                                <a href="#"  >test3</a>
                                                                                                            </div>
                                                                                                            
                                                                                                        </div>
                                                                                                    </li>
                                                                                            </ul>
                                                                                        <!-- end sub second level-->
                                                                                    </li>
                                                                            </ul>
                                                                        <!-- end sub first level-->




                                                        </ul>
                                                        <!-- end siblings 2-->




                                                    </div>
                                                    <!-- start drop downs -->
                                                    <div id="drop-1" data-dropdown-content class="f-dropdown" aria-hidden="true">
                                                      <p>Lorem ipsum</p>

                                                    </div>
                                                    <!-- end drop downs -->
                                                </div>
                                            </div>
                                            <div class="large-8  columns tt-sr-right">
                                                <div class="tt-sr-mid tt-resize-div-handle"></div>
                                                <h3>With respect to <span class="orange">Goal: Select Best Consultant</span></h3>
                                                <div class="large-12 columns">
                                                    <span>Distribut Mode</span>
                                                    <span>All Participants (8)</span>
                                                </div>
                                                <div class="row collapse">
                                                    <div class="large-5 columns">
                                                        <select>
                                                            <option>Candidate 1</option>
                                                        </select>
                                                    </div>
                                                    <div class="large-2 columns text-center">
                                                        vs
                                                    </div>
                                                        
                                                    <div class="large-5 columns text-right">
                                                        <select>
                                                            <option>Candidate 1</option>
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="row collapse">
                                                    <div class="large-12 columns text-center">
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                        <div class="progress columns">
                                                          <span class="meter" style="width: 50%;"></span>
                                                          <div class="text">Logical Thought Process</div>
                                                          <span class="percent">50%</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                        
                                  </div>
                                  
                                </div>
                            </div>


                            
                        </div> <!-- tt-left-col-->




                        <% } else { %>
                            <div class="row">
                                <div class="large-12 columns text-center">

                                    <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
                                        <includes:loginMessage ID="needToLogin" runat="server" />

                                </div>
                            </div>
                    </div>
                    <% } %>
                        <!-- END landing page of synthesize result -->
            </div>
            <!-- END INNER page of synthesize result -->
        </div>
        <script src="/Scripts/Controllers/SynthesizeController.js"></script>
    </asp:Content>