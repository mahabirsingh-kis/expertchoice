<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="breadcrumbs.ascx.cs" Inherits="AnytimeComparion.Pages.includes.breadcrumbs" %>
    <%@ Register Src="~/Pages/includes/QHicons.ascx" TagPrefix="includes" TagName="QHicons" %>

<style ng-if="is_teamtime">
    /* show this breadcrumbs for teamtime only */
    .tooltip {
        margin-left: -23px;
    }
</style>


    <div class="large-12 columns tt-breadcrumb-wrap tg-s " ng-if="is_pipescreen" ng-cloak>
        <!-- breadcrumbs-->
        <div class="columns tt-breadcrumb hide-for-medium-down">
            <ul>
                 
                <li ng-if="!current_project.is_teamtime">Workgroup:  <span class="action workgroup-name" ng-bind-html="current_project.workgroup_name"></span></li>
                <li>Project:  <span class="action project-name" ng-bind-html="current_project.project_name"></span></li>
                <li class="tt-related-elements" ng-if="current_project.is_teamtime">Meeting ID: <span class="action" id="Meeting_ID" runat="server">219-679-424</span> </li>
                <li ng-if="output.read_only" >
                  View Only Mode:    <span class="action">{{output.read_only_user}}.</span>
                </li> 
                <li class="the-evaluator right">
                    <div class="row">
                        <div class="large-4 small-4 columns">
                            <div class="the-text right" ng-if="!current_project.is_teamtime && output.isPM"> View as Evaluator:</div>
                        </div>
                        <div class="large-4 small-4 columns">
                            <div class="the-evaluator-switch" ng-if="!current_project.is_teamtime && output.isPM">
                               <div class="onoffswitch">
                                    <input type="checkbox" ng-model="pm_switch" name="is_AT_owner_toggle" class="onoffswitch-checkbox" id="is_AT_owner_toggle"  ng-change="switch_pm(pm_switch)">
                                    <label class="onoffswitch-label" for="is_AT_owner_toggle">
                                        <span class="onoffswitch-inner"></span>
                                        <span class="onoffswitch-switch"></span>
                                    </label>
                                </div>
                            </div>
                        </div>
                        <div class="large-4 small-4 columns">
                                <!--<div ng-if="output.page_type=='atShowLocalResults' || output.page_type=='atShowGlobalResults' || output.page_type=='atInformationPage'" class="columns" id="QHIcons">
                                 <includes:QHicons runat="server" ID="Info_QHicons" />
                             </div>-->

                             <div ng-if="$parent.projectLockedInfoteamTimeUrl.length = 0 && output.page_type == 'atTeamTime'" class="columns small-12 small-centered text-center">
                                 <span class="text">Teamtime Session is running. Please click the button below to join</span>
                                 <label>
                                     <button ng-click="redirectToComparionTeamtime()" class=" button tiny tt-button-primary">Join Meeting</button>
                                 </label>
                             </div> 
                        </div>
                    </div>
                </li>
                               
                                
                <li class="left tt-related-elements" ng-if="current_project.is_teamtime">
                    TeamTime&trade; <span class="action">Meeting in Progress.</span> 
                </li>             
                <li>
                    <div class="small-12 columns tt-controllers tt-related-elements" ng-if="isPM">
                         
                        <a href="#" ng-click="setTTEvent(TTpause)" data-tooltip aria-haspopup="true" data-options="disable_for_touch:true" title="Play / Pause" id="TTpause" class=" has-tip tip-top" data-toggler="tg-pause-play">
                            <span ng-if="TTpause == 'pause'" class="icon icon-tt-pause event-buttons" data-toggler="tg-icon tg-i-TTpause"></span>
                            <span ng-if="TTpause == 'resume'" class="icon icon-tt-play event-buttons" data-toggler="tg-icon tg-i-TTpause"></span>
                        </a>
                        <a href="#" ng-click="setTTEvent('stop')" data-tooltip aria-haspopup="true" data-options="disable_for_touch:true" title="Stop" class="has-tip tip-top TTstop"><span class="icon-tt-stop event-buttons" data-event="stop"></span></a>
                        <a href="#" ng-click="setTTEvent('refresh')" data-tooltip aria-haspopup="true" data-options="disable_for_touch:true" title="Refresh data" class="has-tip tip-top" ng-click="remove_user_list()"><span class="icon-tt-refresh" data-event="refresh"></span></a>
                        <a href="#" data-dropdown="settingDropDown" aria-controls="settingDropDown" aria-expanded="false" data-tooltip aria-haspopup="true" data-options="disable_for_touch:true" title="Pipe Settings" class="has-tip tip-top dropdown"><span class="icon-tt-cog"></span></a>
                    </div>
                </li>
            </ul>
        </div>
    <!-- //breadcrumbs-->
    </div>
                    
    <ul id="settingDropDown" data-dropdown-content class="f-dropdown pipe-settings-dropdown" aria-hidden="true">
        <li class="row collapse">
            <label class="small-12 columns infodocs-checkbox-label pipe-settings">
            <input ng-click="set_infodoc_mode(is_infodoc_tooltip)" 
            ng-model="is_infodoc_tooltip" type="checkbox" 
                />
            <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.tooltip_mode[1]}}"> Tooltip Mode</span>
            </label>
        </li>
        <li class="row collapse" ng-if="output.mtype == 'ptGraphical'">
            <label class="small-12 columns pipe-settings">
                <input class="graphical-single" ng-model="TTOptions.singleLineMode" type="checkbox" ng-change="tt_is_single_line(TTOptions.singleLineMode)" ng-true-value="1">
                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.single_line[1]}}">Single Line Mode (beta)</span>
            </label>
        </li>
        <li class="row collapse">
            <label class="small-12 columns pipe-settings">
            <input class="anonymous-checkbox"  type="checkbox" ng-model="TTOptions.isAnonymous" ng-change="setAnonymousMode(TTOptions.isAnonymous)" ng-true-value="1" name="">
            <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.anonymous_mode[1]}}">Anonymous Mode</span>
            </label>
        </li>
        <li class="row collapse">
            <label class="small-12 columns pipe-settings">
            <input class="online-checkbox" type="checkbox" ng-model="TTOptions.hideOfflineUsers" ng-change="hideOfflineUsers(TTOptions.hideOfflineUsers)" ng-true-value="1" name="">
            <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.hide_offline_users[1]}}">Hide Offline Users</span>
            </label>
        </li>
        <li class="row collapse  hide-pie-chart" ng-if="main_teamtime_screen == 'ptGraphical'">
            <label class="small-12 columns pipe-settings">
                <input class="hide-pie-checkbox"  type="checkbox" ng-model="TTOptions.hidePie" ng-change="hidePieChart(TTOptions.hidePie)" ng-true-value="1">
                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.hide_pie_chart[1]}}">Hide Pie Chart</span>
            </label>
        </li>
        <li class="row collapse">
            <label class="small-12 columns resize-checkbox-label pipe-settings">
            <input  class="resize-checkbox" type="checkbox" name="">
            <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.resize_info_docs[1]}}">Resize Infodocs for All</span>
            </label>
        </li>
        <li class="row collapse">
            <label class="small-12 columns infodocs-checkbox-label pipe-settings">
            <input  class="infodocs-checkbox" type="checkbox" name="">
            <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.toggle_info_docs[1]}}">Toggle Infodocs for All</span>
            </label>
        </li>
        <li class="row collapse">                        
            <div class="columns pipe-settings">
                <label>
                    <input type="checkbox" class="hide-judgment-checkbox" ng-model="TTOptions.hideJudgments" ng-change="setHiddenJUdgmentForPM()" ng-true-value="1" name="cb-showOverallResult">
                    <span><strong>Hide Judgments</strong></span>
                </label>
            </div>

            <div class="columns tt-j-show-overall-result" ng-if="TTOptions.hideJudgments" >
                        <div class="all">
                            <label>
                                <input type="radio" name="all-me" class="hide-radio default-radio" ng-model="showToMe" ng-change="showJudgment(showToMe)"  ng-value="false" style="margin:10px 0px 10px 10px;">
                                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.hide_for_all[1]}}">Hide for All</span>
                            </label>
                        </div>
                        <div class="me-only">
                            <label>
                                <input type="radio" name="all-me" class="hide-radio me-radio" ng-model="showToMe" ng-change="showJudgment(showToMe)" ng-value="true" style="margin-left:10px; margin-bottom:0px;">
                                <span  data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.show_to_me[1]}}">Show to Me</span>
                            </label>
                        </div>
            </div>

        </li>
                            
    </ul>

 
 