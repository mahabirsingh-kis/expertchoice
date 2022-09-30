<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="evaluation-progress.aspx.cs" Inherits="AnytimeComparion.Pages.evaluation_progress" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<style>
   .disabled {
       color: #ccc !important;
   }
</style>
<div class="tt-body tt-collect-input-wrap">

    <% if (Session["User"] != null || Session["UserType"] != null) { %>


        <div  ng-controller="EvaluationProgressController" ng-init="get_evaluation_progress_data()" class="row collapse tt-2-cols tt-evaluation-progress-wrap" ng-cloak>

            <div class="large-12 columns tt-left-col">
                <div class="large-12 columns">
                        <div class="columns tt-header-nav blue-header">
                            <ul class="breadcrumbs">
                                <li><a href="../collect-input"> Collect Input</a></li>
                                <li class="current">
                                    <span>EVALUATION PROGRESS
                                        <a href="#" class="help collect-input" data-dropdown="help-dd" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                    </span>
                                </li>
                                
                            </ul>
                        </div>


                    <!-- help dropdown -->
                        <div id="help-dd" class="f-dropdown" data-dropdown-content="" aria-hidden="true" tabindex="-1">
                            <p>
                                Lorem ipsum dolor sit amet. aman pulo ditr yu imusta putri
                            </p>
                        </div>
                        <!-- help-dd dropdown -->

                    </div>

                <div class="large-12 columns text-center">
                    <h2>Evaluation Progress for Project <span class="bold">"{{current_project.project_name}}"</span></h2>
                </div>
                
                <div class="large-5 medium-8 columns large-centered medium-centered text-center">
                    <div class="large-6 columns large-text-right medium-text-center small-text-center">
                        Overall Evaluation Progress:
                    </div>
                    <div class="large-6 columns tt-overall-ep-wrap">
                        <div class="progress"><span class="meter" style="width: {{output.OverallProgress}}%"></span><span class="text">{{output.OverallProgress}}%</span></div>
                    </div>
                </div>
                
             
                    
                <div class="large-12 columns tt-header-nav">
                    <div class="columns  tt-header-nav blue-header ">
                        <span class="text-header">List of evaluators <span class="">(Total: {{output.evaluation_progress.length}} <small>, Online users shown in green</small>)</span></span>
                    </div>

                    <ul class="large-12 columns ">
                       
                        <li>
                            <button class="button tiny tt-button-icond-left tt-button-primary">
                                <span class="text">Copy</span>
                                <span class="icon-tt-plus icon"></span>
                            </button>

                            

                        </li>
                        <li>
                            <button  class="button tiny tt-button-icond-left tt-button-primary">
                                <span class="text">CSV</span>
                                <span class="icon-tt-table icon"></span>
                            </button>
                          
                        </li>
                        <li>
                            <button class="button tiny tt-button-icond-left tt-button-primary">
                                <span class="text">Print</span>
                                <span class="icon-tt-printer icon"></span>
                            </button>
                          
                        </li>
                        <li>
                            <button  class="button tiny tt-button-icond-left tt-button-primary">
                                <span class="text">Refresh</span>
                                <span class="icon-tt-refresh icon"></span>
                            </button>
                          
                        </li>
                        <li class="hide-for-medium-down">
                            <a href="#" class="tt-dropdown button tiny tt-button-primary" href="#" data-dropdown="tt-col-dd" data-options="is_hover:false;">
                                <span class="icon-tt-filter"></span>
                                <span class="text">Columns </span>
                                <span class="icon-tt-chevron-down drop-arrow"></span>
                            </a>

                            <ul id="tt-col-dd" class="tt-dropdown-content f-dropdown tt-dropdown-content-has-form" data-dropdown-content>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="name" checked class="togglePart-columns">
                                        Name</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="email" checked class="togglePart-columns">
                                        Email</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="progress" checked class="togglePart-columns">
                                        Progress</label>
                                </li>
                                <li>
                                    <label>
                                        <input type="checkbox" data-name="time" checked class="togglePart-columns">
                                        Last Judgment Time</label>
                                </li>
                                <li>
                                    
                                </li>
                                
                            </ul>
                        </li>



                    </ul>


                    <div class="row collapse">
                        <div class="large-12 columns">  
                            <table class="tt-table-wrap hide-for-medium-down">
                                <thead>
                                    <tr>
                                        <th class="tt-name"><span class="icon-tt-user"></span>Name 
                                            <a><span class="icon-tt-sort-toggle"></span></a>
                                        </th>
                                        <th class="tt-email"><span class="icon-tt-envelope"></span>Email<a><span class="icon-tt-sort-down"></span></a></th>
                                        <th class="tt-progress"><span class="icon-tt-loading"></span>Progress<a><span class="icon-tt-sort-up"></span></a></th>
                                        <th class="tt-time"><span class="icon-tt-clock"></span>Last Judgment Time<a><span class="icon-tt-sort-toggle"></span></a></th>
    <!--                                        <th class="tt-access"><span class="icon-tt-lock"></span>Access</th>-->
    <!--                                        <th class="tt-role"><span class="icon-tt-user-secret"></span>Role</th>-->
    <!--                                        <th class="tt-pgroup"><span class="icon-tt-users"></span>Participant Group</th>-->
                                        <th class="tt-action"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr 
                                        ng-repeat="evaluator in output.evaluation_progress"

                                        >
                                        <td class="tt-name"><span ng-class="{'online':evaluator.IsOnline, 'offline':!evaluator.IsOnline}" class="tt-user-status">&#9679;</span>{{evaluator.Name}}</td>
                                        <td class="tt-email"><a href="mailto:{{evaluator.Email}}">{{evaluator.Email}}</a></td>
                                        <td ng-init="progress_bar = (evaluator.EvaluatedCount/evaluator.TotalCount)*100" class="tt-progress">
                                            <div class="nice radius success progress">
                                                <span class="meter" style="width: {{ progress_bar }}%"></span>
                                                <span class="text">
                                                    {{ progress_bar.toFixed(1) }}% 
                                                    ({{evaluator.EvaluatedCount}}/{{evaluator.TotalCount}})
                                                </span>
                                            </div>
                                        </td>
                                        <td class="tt-time">{{evaluator.LastJudgmentTimeUTC}}</td>
                                        
                                        <td class="tt-action table-action">
                                            <a ng-click="open_view_link_modal(evaluator.EvaluationLink)" ng-if="evaluator.IsLinkEnabled" href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Get User's Projet Link (Anytime Evaluation when available)"><span class="icon-tt-link"></span></a>
                                            <a ng-if="!evaluator.IsLinkEnabled" href="#" class="disabled"><span class="icon-tt-link"></span></a>
                                            
                                            <a ng-if="evaluator.IsLinkGoEnabled" ng-click="login_as_participant(evaluator.EvaluationLink)" href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="Login as this participant"><span class="icon-tt-sign-in"></span></a>
                                            <a ng-if="!evaluator.IsLinkGoEnabled" href="#" class="disabled"><span class="icon-tt-sign-in"></span></a>
                                            
                                            <a ng-if="evaluator.IsPreviewLinkEnabled"ng-click="view_anytime_readonly(evaluator.UserID)"  href="#" data-tooltip aria-haspopup="true" class="has-tip tip-top" data-options="disable_for_touch:true" title="View (ONLY) Evaluation Steps for this Evaluator"><span class="icon-tt-eye"></span></a>
                                            <a ng-if="!evaluator.IsPreviewLinkEnabled" href="#" class="disabled"><span class="icon-tt-eye"></span></a>
                                        </td>
                                    </tr>                                 
                                </tbody>
                            </table>
                            <div class="show-for-medium-down">
                                
                                <aside ng-init="progress_bar = (evaluator.EvaluatedCount/evaluator.TotalCount)*100" ng-repeat="evaluator in output.evaluation_progress" class="row tt-evaluation-mobile-wrap">
                                    <div class="row collapse">
                                        <div class="small-2 columns text-center"><span class="icon-tt-user"></span></div>     
                                        <div class="small-10  columns"><span ng-class="{'online':evaluator.IsOnline, 'offline':!evaluator.IsOnline}" class="tt-user-status">&#9679;</span> {{evaluator.Name}} </div>
                                    </div>
                                    <div class="row collapse">
                                        <div class="small-2 columns text-center"><span class="icon-tt-envelope"></span></div>     
                                        <div class="small-10 columns"><a href="mailto:{{evaluator.Email}}">{{evaluator.Email}} </a></div>
                                    </div>
                                    <div class="row collapse">
                                        <div class="small-2 columns text-center"><span class="icon-tt-loading"></span></div>     
                                            <div class="small-10 columns"><div class="radius success progress"><span class="meter" style="width: {{ progress_bar }}%"></span>
                                                <span class="text">  
                                                    {{ progress_bar.toFixed(1) }}% 
                                                    ({{evaluator.EvaluatedCount}}/{{evaluator.TotalCount}})
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row collapse">
                                        <div class="small-2 columns text-center"><span class="icon-tt-clock"></span></div>     
                                        <div class="small-10 columns last-j-date">{{evaluator.LastJudgmentTimeUTC}}</div>
                                    </div>
                                    <div class="row collapse">
                                        <div class="small-4 right columns">
                                            <div class="small-4 columns text-center">
                                                <a ng-click="open_view_link_modal(evaluator.EvaluationLink, evaluator.IsLinkEnabled)" ng-class="{'disabled' : !evaluator.IsLinkEnabled}" href="#" title="Edit"><span class="icon-tt-link"></span></a>
                                            </div>
                                            <div class="small-4 columns text-center">
                                                <a ng-click="login_as_participant(evaluator.EvaluationLink, evaluator.IsLinkGoEnabled)" ng-class="{'disabled' : !evaluator.IsLinkGoEnabled}" href="#"  title="View"><span class="icon-tt-sign-in"></span></a>
                                            </div>
                                            <div class="small-4 columns text-center">
                                                <a ng-class="{'disabled' : !evaluator.IsPreviewLinkEnabled}" href="#" title="Delete"><span class="icon-tt-eye"></span></a>
                                            </div>
                                        </div>
                                    </div>
                                </aside>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="viewLinkModal" data-reveal class="reveal-modal small tt-modal-wrap fixed-modal" aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">Evaluation Link</h2>
            <div class="tt-modal-content large-12 columns tt-center-height-wrap text-center">
                 <br/>
                <span id="evaluators_link"></span>
                <br/>
                <a href="" id="evaluators_copy_link_btn" class="cc-btns button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-lite-blue" ngclipboard-success='copied_clipboard(e, "#evaluators_copy_link_btn");' ngclipboard data-clipboard-text="">
                    Copy to Clipboard
                </a>
           </div>
        </div>

    <% }
    else
    { %>
        <div class="row">
            <div class="large-12 columns text-center">
                <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
                <includes:loginMessage ID="needToLogin" runat="server" />
            </div>
        </div>
    <%} %>
</div>
<script src="/Scripts/Controllers/EvaluationProgressController.js"></script>
</asp:Content>

