<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="my-projects.aspx.cs" Inherits="AnytimeComparion.Pages.my_projects" %>
            <asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server" ng-cloak>
                
                
            
            <style>
                .not-working{
                    display:none;
                }

                #warning-projects-message{
                    margin-bottom:0px !important;
                }

                .not-valid-project{
                    /*opacity:0.5;*/
                }
                #select2-select_workgroup-container {
                    font-size: small !important;
                }
                .select2-results__option {
                    font-size: small !important;
                }
                .select2-container--default {
                    width: 150px !important;
                }
            </style>
                <div class="tt-body tt-myprojects-wrap" ng-init="allow_my_projects()" ng-cloak>
                     <%-- warning message --%>    
                       <%-- <div ng-if="(!is_chrome && !is_safari)" id="warning-projects-message" data-alert class="columns small-centered text-center alert-box"
                            style="background-color: #f3f3f3;
                                    padding: 10px 20px 10px 10px;
                                    border-radius: 5px;
                                    border: 1px solid #ddd;
                                    clear: both;
                                    font-size: 14px !important;
                                    color:#777;
                                    ">
                                <!-- alert-box info radius --> 
                              <p ng-if="isMobile()" style="margin-bottom: 0px;font-size: 14px;">
                               For better performance, we recommend using Google Chrome/Default Browser in Android phones or Safari/Chrome in Apple (iOS) phones.
                            </p>
                            <p ng-if="!isMobile()" style="margin-bottom: 0px;font-size: 14px;">For better performance, we recommend using Google Chrome for Windows and Safari/Chrome for MAC.</p>    
                                                        
                            <a href="#" class="close" style="position: absolute;
                                    top: 15px;
                                    right: 5px;
                                    color: #fff;
                                    background-color: #555;
                                    padding:0px 5px;
                                    border-radius: 3px;
                                    ">&times;</a>
                        </div>--%>
                        <%-- end warning message --%> 
                    <% if (Session["User"] != null || Session["UserType"] != null)
                       { %>
                    <div class="row collapse tt-2-cols">

                        <div class="large-12 columns tt-left-col" >
                            <div class="large-12 columns">
                                <!--                  <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>-->
                                <div class="columns tt-header-nav blue-header">
                                    <ul class="breadcrumbs">
                                        <li>
                                            <a ng-click="main_workgroups(ecProjectStatus.psActive)"><span class="text ">My Projects</span></a>
                                            <a class="hide" data-dropdown="help-dd" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                        </li>
                                        <li class="not-working hide" ng-class="{'hide':current_project.project_id == -1}">
                                            <a href="/pages/evaluation-progress.aspx"><span class="text ">Evaluation Progress</span></a>
                                        </li>
                                        <li ng-if="ProjectsStatus == ecProjectStatus.psArchived" class="current"><span>Archive</span> <a href="#" class="help" data-dropdown="help-dd-a" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a></li>
                                        <li ng-if="ProjectsStatus == ecProjectStatus.psTrash" class="current"><span>Trash</span> <a href="#" class="help" data-dropdown="help-dd-d" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a></li>
                                        </ul>
                                </div>

                            </div>
                            <div class="large-12 columns tt-header-nav " >
                                <div class="large-12 columns">
                                    <ul class="tt-main-nav-wrap-search">
                                        <li class="full-nav-small">
                                            <label>Workgroup:</label>
                                        </li>
                                        <li class="full-nav-small workgroup-drop-selector">

                                            <select class="form-control" id="select_workgroup" ng-model="workgroup" ng-options="obj[1] for obj in workgroups track by obj[0]" ng-change="main_SetWorkgroup(workgroup[0])">
                                            </select>

                                        </li>
                                        <li ng-if="isPM" class="not-working">
                                            <a runat="server" href="" ng-click="main_workgroups(ecProjectStatus.psArchived)" class="button tiny tt-button-icond-left tt-button-primary tt-button-lite-blue">
                                                <span class="text">Archive</span>
                                                <span class="icon-tt-archive icon"></span>
                                            </a>
                                        </li>
                                        <li ng-if="isPM">
                                            <a runat="server" href="" ng-click="main_workgroups(ecProjectStatus.psTemplate)" class="not-working button tiny tt-button-icond-left tt-button-primary tt-button-lite-blue">
                                                <span class="text">Templates</span>
                                                <span class="icon-tt-clipboard icon"></span>
                                            </a>
                                        </li>
                                        <li ng-if="isPM" class="not-working">
                                            <a runat="server" href="" ng-click="main_workgroups(ecProjectStatus.psTrash)" class="button tiny tt-button-icond-left tt-button-primary tt-button-lite-blue">
                                                <span class="text">Trash</span>
                                                <span class="icon-tt-trash icon"></span>
                                            </a>
                                        </li>
                                        <li ng-if="isPM" class="workgroup-filter">
                                            <a  class="icon" data-dropdown="tt-col-dd" data-options="is_hover:false;"><span class="icon-tt-filter"></span></a>
                                        </li>
                                        <li class="row collapse">                                      
                                            <div class="medium-6 small-12 columns user-pagination">

                                                <span class="small-12 columns pagination-wrap text-center">
                                                    Page: 
                                                    <a class="arrow button tt-button-secondary" data-arrow="left" ng-click="set_Page(true)"> &laquo; </a>
                                                     {{currentPage + 1}}/{{numberOfPages}} 
                                                    <a class="arrow button tt-button-secondary" data-arrow="left" ng-click="set_Page(false)"> &raquo; </a>
                                                    <select class="show-smart users-verbal" ng-model="pageSize" ng-change="change_pageSize(pageSize)" ng-options="obj for obj in pageOptions">
                                                    </select>
                                                </span>
                                                <span class="pagination-wrap show-for-small small-12 columns text-center">
                                                  Sort By:
                                                    <select class="show-smart users-verbal tt-filters" ng-init="changecolumns()"
                                                        ng-model="mobile_sort_selected" ng-change="sort_Project(mobile_sort_selected[0])" 
                                                        ng-options="column[1] for column in columns track by column[1]">
                                                    </select>
                                                    <a>
                                                        <span ng-class="reverse == null ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                            ng-click="sort_Project(mobile_sort_selected[0])"></span>
                                                    </a>
                                                </span>
                                            </div>
                                            <div class="medium-6 small-12 columns user-pagination">
                                                <input type="text" name="" placeholder="Filter results below" class="search-input" ng-model="project_name_searching"  ng-keyup="search_ProjectName(project_name_searching)">
                                            </div>
                                            
                                        </li>
                                    </ul>


                                    <ul id="tt-col-dd" class="tt-dropdown-content f-dropdown tt-dropdown-content-has-form" data-dropdown-content ng-if="isPM">
                                        <li>
                                            <label>
                                                <input type="checkbox" ng-model="output.project_access" ng-change="rememberFilter(output.project_access, 'project_access')" ng-init="output.project_access=true" class="togglePart-columns">
                                                Online
                                            </label>
                                        </li>
                                        <li>
                                            <label>
                                                <input type="checkbox" ng-model="output.project_status" ng-change="rememberFilter(output.project_status, 'project_status')" ng-init="output.project_status=true" class="togglePart-columns">
                                                Status</label>
                                        </li>
                                        <li>
                                            <label>
                                                <input type="checkbox" ng-model="output.last_access" ng-change="rememberFilter(output.last_access, 'last_access')" class="togglePart-columns">
                                                Last Access</label>
                                        </li>
                                        <li>
                                            <label>
                                                <input type="checkbox" ng-model="output.last_modified" ng-change="rememberFilter(output.last_modified, 'last_modified')" class="togglePart-columns">
                                                Last Modified</label>
                                        </li>
                                        <li>
                                            <label>
                                                <input type="checkbox" ng-model="output.date_created" ng-change="rememberFilter(output.date_created, 'date_created')" class="togglePart-columns">
                                                Date Created</label>
                                        </li>
                                        <li class="not-working">
                                            <label>
                                                <input type="checkbox" ng-model="output.overal_judgment_process" ng-change="rememberFilter(output.overal_judgment_process, 'overal_judgment_process')" class="togglePart-columns">
                                                Overall Judgment Process</label>
                                        </li>

                                    </ul>

                                </div>

                            </div>

                            <div class="row collapse">
                                <div class="large-12 columns">
                                    <div class="projects-loop-wrap hide-for-small-only">
                                    <table class="tt-table-wrap hide-for-small-only" style="margin-bottom: -30px !important;position: relative;z-index: 2;">
                                        <thead class="project-list-orig">
                                            <tr>
                                                <th class="proj-title-duplicate">
                                                    <div class="tt-with-sorter">
                                                        <div class="text small-6 columns">
                                                            <div class="left">PROJECTS {{listofallProjects.length}}</div>
                                                            <div class="sorter-icons left">
                                                               <a>
                                                                   <span ng-class="columnname != 'this[1]' ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                        ng-click="sort_Project('name')"></span>
                                                               </a>

                                                            </div>
                                                        </div>
                                                        <div class="text small-6 columns">
                                                            <button class="button tiny tt-button-primary not-working" data-reveal-id="selectProjMode" ng-if="isPM">
                                                                <span class="text">ADD NEW PROJECT</span>
                                                            </button>
                                                        </div>
                                                    </div>

                                                </th>
                                                <th class="proj-ol-duplicate tt-project_access" ng-if="isPM && ProjectsStatus == ecProjectStatus.psActive && output.project_access">
                                                    <div class="tt-with-sorter left">
                                                        <div class="text small-11 columns">ONLINE</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                           <a>
                                                               <span ng-class="columnname != 'this[4]' ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                    ng-click="sort_Project('online')"></span>
                                                           </a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="proj-status-duplicate tt-project_status" ng-if="ProjectsStatus == ecProjectStatus.psActive && output.project_status">
                                                    <div class="tt-with-sorter left">
                                                        <div class="text small-11 columns">STATUS</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                           <a>
                                                               <span ng-class="columnname != 'this[3]' ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                    ng-click="sort_Project('status')"></span>
                                                           </a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="tt-last_access tt-last_access-duplicate" ng-if="isPM && output.last_access">
                                                    <div class="tt-with-sorter left">
                                                        <div class="text small-11 columns">LAST ACCESS</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                           <a>
                                                               <span ng-class="columnname != 'this[10]' ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                    ng-click="sort_Project('last')"></span>
                                                           </a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="tt-last_modified" ng-if="isPM && output.last_modified">
                                                    <div class="tt-with-sorter left">
                                                        <div class="text small-11 columns">LAST MODIFIED</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                           <a>
                                                               <span ng-class="columnname != 'this[7]' ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                    ng-click="sort_Project('modify')"></span>
                                                           </a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="tt-date_created" ng-if="isPM && output.date_created">
                                                    <div class="tt-with-sorter left">
                                                        <div class="text small-11 columns">DATE CREATED</div>
                                                        <div class="sorter-icons small-1 columns text-center" >
                                                           <a>
                                                               <span ng-class="columnname != 'this[8]' ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                    ng-click="sort_Project('create')"></span>
                                                           </a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="tt-overal_judgment_process not-working" ng-if="isPM && output.overal_judgment_process">
                                                    <div class="tt-with-sorter left">
                                                        <div class="text small-11 columns">OVERALL JUDGMENT PROCESS</div>
                                                        <div class="sorter-icons small-1 columns text-center">
                                                            <a ><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="large-4 columns proj-extra-columns-duplicate">

                                                </th>
                                            </tr>
                                        </thead>
                                    <%--<div class="projects-loop-wrap hide-for-small-only">--%>
                                        <%--<table ng-class="{'no-margin':  listofallProjects.length < 6}" class="projects-loop-wrap tt-table-wrap hide-for-small-only" style="position: relative;    z-index: 1;">--%>
                                        
                                        <tbody class=" hide-for-small-only" ng-show="!isMobile()" infinite-scroll="incrementProjects(listofallProjects.length)" infinite-scroll-distance="3" ng-cloak>
                                            
                                            <tr 
                                                ng-class="{'not-valid-project' : project[12] != 'True' }"
                                                ng-repeat="project in projects | startFrom:currentPage*pageSize | limitTo:pageSize | filter:project_name_search | orderBy: columnname:reverse" ng-show="projects.length > 0" ng-cloak>
                                                <td class="proj-details" ng-init="$last ? setProjWidth($last) : ''">
                                                    <div class="" title="{{project[1]}}">
                                                        <span  class="proj-name">{{::project[1] | limitTo:100}}{{ ::project[1].length > 100 ? '&hellip;' : ''}}</span>
                                                    </div>
                                                    <div class="proj-author">
                                                        <span class="gray">Owner: </span><span class="projOwner">{{::project[2]}}</span>
                                                        <span class="gray">Your Role: </span><span class="projRole" ng-if="isPM && project[11] != 'False'">Project Manager</span>
                                                        <span class="projRole" ng-if="project[11] == 'False'">Evaluator</span>

                                                    </div>
                                                    <div class=" left">
                                                        <a  data-dropdown="viewUsers{{$index}}" aria-controls="drop1" aria-expanded="false" ng-click="getUsersByProject(project[0])" ng-if="isPM && project[11] != 'False'"><span class="icon-tt-users mini-icons"></span><span <%--class="num-users"--%>> {{::project[9]}} &nbsp;</span></a>

                                                        <ul id="viewUsers{{$index}}" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                                            <div class="dropdown-long-content-wrap" ng-if="listofusers != null" ng-cloak>
                                                                <li ng-repeat="user in listofusers"><a  title="{{user.UserName}}">{{user.UserEMail}}</a></li>
                                                            </div>
                                                        </ul>

                                                        <!--<a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="Has Judgments"><span class="icon-tt-edit mini-icons"></span></a>
                                                        <a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="No Judgments"><span class="icon-tt-edit mini-icons none"></span></a>

                                                        <a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="Has Survey"><span class="icon-tt-file-outline mini-icons"></span></a>
                                                        <a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="No Survey"><span class="icon-tt-file-outline mini-icons none"></span></a>-->
                                                    </div> 
                                                    
                                                    <div class="left" ng-if="isPM">
                                                        <a data-tooltip aria-haspopup="true" title="Project is opened. Click to close." class="toggleOpenCloseFoler has-tip tip-top not-working" ng-if="current_project.project_id == project[0]" ng-click="ProcessProject(project[0], false)"><span class="icon-tt-folder-open-solid"></span></a>
                                                        <a data-tooltip aria-haspopup="true" title="Project is closed. Click to open." class="toggleOpenCloseFoler has-tip tip-top not-working" ng-if="current_project.project_id != project[0]" ng-click="ProcessProject(project[0], true)"><span class="icon-tt-folder-close-solid"></span></a>
                                                    </div>
                                                    <div class="left" ng-if="isPM && project[11] != 'False'">
                                                        <a ng-click="selectedProject.project_id = project[0]" title="More Actions" data-dropdown="moreActions-dd" aria-controls="drop1" aria-expanded="false">&nbsp; &nbsp;<span class="icon-tt-chevron-down icon"></span></a>
                                                    </div>
                                                </td>

                                                <td class="tt-status" ng-if="isPM && ProjectsStatus == ecProjectStatus.psActive">
                                                    <div class="onoffswitch" ng-if="project[11] != 'False'">
                                                        <input type="checkbox" name="onoffswitch" class="onoffswitch-checkbox" id="switch-{{project[0]}}" ng-checked="project[4] == 'True'" ng-click="main_DisplayStatus(project[0])">
                                                        <label class="onoffswitch-label" for="switch-{{project[0]}}">
                                                            <span class="onoffswitch-inner"></span>
                                                            <span class="onoffswitch-switch"></span>
                                                        </label>
                                                    </div>
                                                    <span ng-if="project[11] == 'False'" ng-bind-html="project[4] == 'True' ? 'Online' : 'Offline'"></span>
                                                </td>
                                                <td class="tt-project_access " ng-if="ProjectsStatus == ecProjectStatus.psActive">
                                                    <div class="columns text-center" ng-if="project[3] == 'True'">
                                                        <span>{{::(project[6].toLowerCase() == "true" ? project[5] : project[6])}} TeamTime Evaluation in progress</span>
                                                            <br />
                                                        <%-- case 12287: Block Teamtime evaluation progress for Gecko Release --%>
                                                            <b class="not-working">
                                                                <span ng-if="project[5] == current_user" >
                                                                    <a href="#" ng-click="go_ToTeamtime(true, project[0])" class='columns large-6'>Continue</a>
                                                                    <a href="#" ng-click="go_ToTeamtime(false, project[0])" class='columns large-6'>Stop</a>
                                                                </span>
                                                                <span ng-if="project[5] != current_user">
                                                                    <a href="#" ng-click="go_ToTeamtime(true, project[0])" class='columns large-6'>Join</a>
                                                                    <a href="#" ng-click="go_ToTeamtime(false, project[0])" ng-if="isPM" class='columns large-6'>Stop</a>
                                                                </span>
                                                            </b>
                                                            
                                                    </div>
                                                    <div class="small-text-right medium-text-center large-text-center" ng-if="project[4] == 'False'">
                                                        Not Available
                                                    </div>
                                                    <div class="small-text-right medium-text-center large-text-center" ng-if="project[4] == 'True' && project[3] != 'True'">
                                                        Available 
                                                    </div>
                                                </td>
                                                <td class="tt-last_access" ng-if="isPM && output.last_access">
                                                    <span ng-bind-html="return_Date(project[10])"></span>
                                                </td>
                                                <td class="tt-last_modified" ng-if="isPM && output.last_modified"><span ng-bind-html="return_Date(project[7])"></span></td>
                                                <td class="tt-date_created" ng-if="isPM && output.date_created"><span ng-bind-html="return_Date(project[8])"></span></td>
                                                <td class="tt-overal_judgment_process not-working" ng-show="isPM && output.overal_judgment_process">
                                                    <div class="nice radius success progress"><span class="meter" style="width:{{project[12]}}%"></span><span class="text">{{project[12]}}%</span></div>
                                                </td>
                                               

                                                <td ng-if="ProjectsStatus == ecProjectStatus.psActive">
                                                    <!--<div class="large-2 medium-12 columns text-center" ng-if="isPM">
                                                        <a data-tooltip aria-haspopup="true" title="Project is opened. Click to close." class="toggleOpenCloseFoler has-tip tip-top" ng-if="current_project == project[0]" ng-click="ProcessProject(project[0], false)"><span class="icon-tt-folder-open-solid"></span></a>
                                                        <a data-tooltip aria-haspopup="true" title="Project is closed. Click to open." class="toggleOpenCloseFoler has-tip tip-top" ng-if="current_project != project[0]" ng-click="ProcessProject(project[0], true)"><span class="icon-tt-folder-close-solid"></span></a>
                                                    </div>-->
                                                    <div class="large-6 medium-12 columns text-center"  >
                                                        

                                                        
                                                        <div class="at-open-wrap toggle-tt-at-btn">
                                                            <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal inverted start-btn" ng-disabled="project[4] == 'False' && project[11] == 'False'" ng-click="(project[4] == 'False' && !isPM)|| main_StartAnytime(project[0], project[12], project[4], project[11])">
                                                                <span class="icon-tt-play-circle icon-20"></span>
                                                                <span class="text">Start Anytime</span>
                                                            </a>
                                                        </div>

                                                        
                                                        <div class="invite-wrap" ng-if="isPM && project[11] != 'False'">
                                                            <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal inverted" data-reveal-id="InviteParticipant" ng-click="generateLink(false, project[0], project[1], project[4])">
                                                                <span class="icon-tt-user-add"></span>
                                                                <span class="text">Invite</span>
                                                            </a>
                                                        </div>
                                                    </div>
                                                    
                                                     <div  ng-if="isPM && project[11] != 'False'" class="large-6 medium-12 columns text-center">
                                                         <div class="tt-open-wrap toggle-tt-at-btn not-working" ng-if="isPM && project[3] != 'True' ">
                                                            <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal inverted start-btn" ng-click="reveal_TT_modal(project[0])">
                                                                <span class="icon-tt-play-circle icon-20"></span>
                                                                <span class="text">Start Teamtime</span>
                                                            </a>
                                                        </div>
                                                         <div class="tt-close-wra toggle-tt-at-btn not-working" ng-if="isPM && project[3] == 'True'">
                                                            <a  class="button has-tip tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-green-normal inverted start-btn" ng-click="go_ToTeamtime(false, project[0])">
                                                                <span class="icon-tt-stop icon-20"></span>
                                                                <span class="text">Stop Teamtime</span>
                                                            </a>
                                                        </div>
                                                        <div class="invite-wrap not-working">
                                                            <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal inverted" data-reveal-id="InviteParticipant" ng-click="generateLink(true, project[0], project[1], project[4])">
                                                                <span class="icon-tt-user-add"></span>
                                                                <span class="text">Invite</span>
                                                            </a>
                                                        </div>
                                                    </div>

                                                    

                                                   
                                                </td>
                                                <td ng-if="ProjectsStatus == ecProjectStatus.psArchived || ProjectsStatus == ecProjectStatus.psTrash">
                                                    <!--<div class="large-2 medium-12 columns text-center" ng-if="isPM">
                                                        <a data-tooltip aria-haspopup="true" title="Project is opened. Click to close." class="toggleOpenCloseFoler has-tip tip-top" ng-if="current_project == project[0]" ng-click="ProcessProject(project[0], false)"><span class="icon-tt-folder-open-solid"></span></a>
                                                        <a data-tooltip aria-haspopup="true" title="Project is closed. Click to open." class="toggleOpenCloseFoler has-tip tip-top" ng-if="current_project != project[0]" ng-click="ProcessProject(project[0], true)"><span class="icon-tt-folder-close-solid"></span></a>
                                                    </div>-->
                                                    
                                                     <div ng-if="isPM" class="large-6 medium-12 columns text-center">
                                                         <div class="tt-open-wrap toggle-tt-at-btn">
                                                            <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal inverted start-btn" ng-click="setProjectbyStatus(ecProjectStatus.psActive, project[0])">
                                                                <span class="icon-tt-play-circle icon-20"></span>
                                                                <span class="text">Activate</span>
                                                            </a>
                                                        </div>
                                                    </div>

                                                    

                                                   
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    </div>
                                    </div>
                            
                                    <div class="show-for-small-only">
                                        <div class="mobile-projects-wrap" ng-if="isMobile()" infinite-scroll="incrementProjects(listofallProjects.length)" infinite-scroll-distance="3" ng-cloak>
                                            <div class="row projNav">
                                                <div class="small-8 small-centered columns text-center">
                                                    <button ng-if="isPM" class="button tiny tt-button-icond-left tt-button-primary not-working" data-reveal-id="selectProjMode">
                                                        <span class="text">Add New Project</span>
                                                        <span class="icon-tt-plus icon"></span>
                                                    </button>
                                                </div>
                                            </div>


                                            <!-- single project -->
                                            <div 
                                                ng-class="{'not-valid-project' : project[12] != 'True' }"
                                                class="row proj-single" ng-repeat="project in projects | startFrom:currentPage*pageSize | limitTo:pageSize | filter:project_name_search | orderBy: columnname:reverse" ng-show="projects.length > 0" ng-cloak>
                                                <h3 class="proj-name panel"><a tt-last_modified ng-binding>{{format_project_name(project[1])}}</a></h3>
                                                <div class="columns proj-author ">
                                                    <span class="gray">Owner: </span><span class="projOwner">{{project[2]}}</span>
                                                    <span class="gray">| Your Role: </span><span class="projRole" ng-if="isPM && project[11] != 'False'">Project Manager</span>
                                                        <span class="projRole" ng-if="project[11] == 'False'">Evaluator</span>
                                                </div>
                                                <div class="columns">
                                                    <a  data-dropdown="viewUsers_mobile{{$index}}" aria-controls="drop1" aria-expanded="false" ng-click="getUsersByProject(project[0])" ng-if="isPM"><span class="icon-tt-users mini-icons"></span><span class="num-users">{{::project[9]}}</span></a>

                                                    <ul id="viewUsers_mobile{{$index}}" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                                        <div class="dropdown-long-content-wrap" ng-if="listofusers != null" ng-cloak>
                                                            <li ng-repeat="user in listofusers"><a  title="{{user.UserName}}">{{user.UserEMail}}</a></li>
                                                        </div>
                                                    </ul>

                                                    <a class="toggleOpenCloseFoler not-working" title="Open" ng-if="current_project.project_id == project[0]" ng-click="ProcessProject(project[0], false)"><span class="icon-tt-folder-open-solid"></span></a>
                                                    <a class="toggleOpenCloseFoler not-working" title="Close" ng-if="current_project.project_id != project[0]" ng-click="ProcessProject(project[0], true)"><span class="icon-tt-folder-close-solid"></span></a>

                                                    <!--<a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="Has Judgments"><span class="icon-tt-edit mini-icons"></span></a>
                                                    <a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="No Judgments"><span class="icon-tt-edit mini-icons none"></span></a>

                                                    <a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="Has Survey"><span class="icon-tt-file-outline mini-icons"></span></a>
                                                    <a  data-options="disable_for_touch:true" data-tooltip aria-haspopup="true" class="has-tip" title="No Survey"><span class="icon-tt-file-outline mini-icons none"></span></a>-->
                                                </div>
                                                <div class="columns tt-project_access" ng-if="isPM && output.project_access">
                                                    <div class="small-4 columns">
                                                        Online: 
                                                    </div>
                                                    <div class="small-8 columns text-right" >
                                                        <div class="switch round tiny mobile-switch right" ng-if="project[11] != 'False'">
                                                            <input id="switchmobile-{{project[0]}}" type="checkbox" ng-checked="project[4] == 'True'" ng-click="main_DisplayStatus(project[0])" />
                                                            <label for="switchmobile-{{project[0]}}"></label>
                                                        </div>
                                                        <span ng-if="project[4] == 'True'" class="right"> Yes </span>
                                                        <span ng-if="project[4] == 'False'" class="right"> No </span>
                                                    </div>
                                                </div>
                                                <div class="columns tt-project_status"  ng-if="ProjectsStatus == ecProjectStatus.psActive && output.project_status">
                                                    <div class=" columns" ng-class="{'small-6' : project[3] != 'True' , 'small-12' :  project[3] == 'True' }">
                                                        Status:
                                                    </div>
                                                    <div class=" columns " ng-class="{'small-6' : project[3] != 'True' , 'small-12' :  project[3] == 'True' }">
                                                        <div>
                                                            
                                                            <div class="panel columns text-center" ng-if="project[3] == 'True'">
                                                                <span><b>{{::(project[6].toLowerCase() == "true" ? project[5] : project[6])}}</b> TeamTime Evaluation in progress</span>
                                                                    <br />
                                                                <%-- case 12287: Block Teamtime evaluation progress for Gecko Release --%>
                                                                    <b class="not-working">
                                                                        <span ng-if="project[5] == current_user" >
                                                                            <a href="#" ng-click="go_ToTeamtime(true, project[0])" class='columns small-6'>Continue</a>
                                                                            <a href="#" ng-click="go_ToTeamtime(false, project[0])" class='columns small-6'>Stop</a>
                                                                        </span>
                                                                        <span ng-if="project[5] != current_user">
                                                                            <a href="#" ng-click="go_ToTeamtime(true, project[0])" class='columns small-6'>Join</a>
                                                                            <a href="#" ng-click="go_ToTeamtime(false, project[0])" ng-if="isPM" class='columns small-6'>Stop</a>
                                                                        </span>
                                                                    </b>

                                                            </div>
                                                            <div class="small-text-right medium-text-center large-text-center" ng-if="project[4] == 'False'">
                                                                Not Available
                                                            </div>
                                                            <div class="small-text-right medium-text-center large-text-center" ng-if="project[4] == 'True' && project[3] != 'True'">
                                                                Available
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="columns tt-overal_judgment_process" ng-if="isPM && output.overal_judgment_process">
                                                    <div class="small-5 columns">
                                                        Progress:
                                                    </div>
                                                    <div class="small-7 columns not-working">
                                                        <div class="nice radius success progress"><span class="meter" style="width:{{project[12]}}%"></span><span class="text">{{project[12]}}%</span></div>
                                                    </div>
                                                </div>
                                                <div class="columns tt-last_access" ng-if="isPM && output.last_access">
                                                    <div class="small-5 columns">
                                                        Last Access:
                                                    </div>
                                                    <div class="small-7 columns text-right">                                                        
                                                        <span ng-bind-html="return_Date(project[10])"></span>
                                                    </div>
                                                </div>
                                                
                                                <div class="columns tt-last_modified" ng-if="isPM && output.last_modified">
                                                    <div class="small-5 columns">
                                                        Last Modified:
                                                    </div>
                                                    <div class="small-7 columns text-right">
                                                        <span ng-bind-html="return_Date(project[7])"></span>
                                                    </div>
                                                </div>
                                                <div class="columns tt-date_created" ng-if="isPM && output.date_created">
                                                    <div class="small-5 columns">
                                                        Created Date:
                                                    </div>
                                                    <div class="small-7 columns text-right">
                                                        <span ng-bind-html="return_Date(project[8])"></span>
                                                        <!--<small ng-bind-html="return_Date(project[8])"></small>-->
                                                    </div>
                                                </div>
                                                <div class="columns">
                                                    <div class="small-6 columns">
                                                        Actions:
                                                    </div>
                                                    <div class="small-6 columns text-right" ng-if="ProjectsStatus == ecProjectStatus.psActive">
                                                        
                                                        <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal" ng-disabled="project[4] == 'False' && project[11] == 'False'" ng-click="(project[4] == 'False' && !isPM) || main_StartAnytime(project[0], project[12], project[4], project[11])">
                                                            <span class="icon-tt-play-circle"></span>
                                                        </a>

                                                        
                                                        <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-orange-normal" ng-if="isPM && project[11] != 'False'" data-reveal-id="InviteParticipant"  ng-click="generateLink(false, project[0], project[1], project[6])">
                                                            <span class="icon-tt-user-add"></span>
                                                        </a>
                                                        <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-lite-blue not-working" ng-if="isPM && project[11] != 'False'" ng-click="reveal_TT_modal(project[0])">
                                                            <span class="icon-tt-play-circle"></span>
                                                        </a>
                                                        <a  class="button tiny tt-button-icond-left tt-button-primary no-bottom-margin tt-button-lite-blue not-working" ng-if="isPM && project[11] != 'False'" data-reveal-id="InviteParticipant"  ng-click="generateLink(true, project[0], project[1], project[4])">
                                                            <span class="icon-tt-user-add"></span>
                                                        </a>
                                                                                                                
                                                       <%-- <a  title="More Actions" data-dropdown="moreActions-dd" aria-controls="drop1" aria-expanded="false" ng-if="isPM && project[11] != 'False'"><span class="icon-tt-chevron-down icon"></span></a>--%>
                                                    </div>
                                                </div>
                                            </div>
                                            <!-- //single project -->
                                        </div>
                                    </div>

                                    <!-- moreActions-dd dropdown -->
                                    <ul id="moreActions-dd" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <li class="not-working"><a ng-click="setProjectbyStatus(ecProjectStatus.psArchived, selectedProject.project_id)">Archive</a></li>
                                        <li><a ng-click="downloadFile(selectedProject.project_id)">Download</a></li>
                                        <li class="not-working"><a ng-click="setProjectbyStatus(ecProjectStatus.psTrash, selectedProject.project_id)" >Delete</a></li>
<%--                                    <li><a >Collect My Input</a></li>
                                        <li><a  data-reveal-id="getLink">Get Link</a></li>
                                        <li><a >Invite Participants</a></li>
                                        <li><a >Create Template or Default Option Set</a></li>--%>
                                    </ul>
                                    <!-- moreActions-dd dropdown -->

                                    <!-- help dropdown -->
                                    <div id="help-dd-a" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <p>
                                            {{help_descriptions.archive[1]}}
                                        </p>
                                    </div>

                                    <div id="help-dd-d" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <p>
                                            {{help_descriptions.delete[1]}}
                                        </p>
                                    </div>
                                    <!-- help-dd dropdown -->

                                    <!-- getlink -->
                                    <div id="getLink" class="reveal-modal tiny tt-modal-wrap" data-reveal aria-labelledby="getLink" aria-hidden="true" role="dialog">
                                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                        <h2 id="getLink" class="tt-modal-header blue-header">Link to project "theProjectNameHere"</h2>
                                        <div class="tt-modal-content large-12 columns">
                                            <div class="large-12 columns">
                                                <input type="text">
                                            </div>
                                            <div class="medium-6 columns text-center">
                                                <button type="submit" class="button tiny tt-button-primary tt-button-primary">
                                                    <span class="text">Copy to clipoard</span>
                                                </button>
                                            </div>
                                            <div class="medium-6 columns text-center">
                                                <button type="button" aria-label="Close" class="close-reveal-modal button tiny tt-button-primary tt-button-transparent">
                                                    <span class="text">Close</span>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- //getlink -->

                                    <!-- createNewProj modal wrap -->
                                    <div id="createNewProj" class="reveal-modal small tt-modal-wrap" data-reveal aria-labelledby="newProj" aria-hidden="true" role="dialog">
                                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                        <h2 id="addPart" class="tt-modal-header blue-header">Create New Project <a  data-reveal-id="tt-create-new-project-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>

                                        <div class="tt-modal-content my-proj large-12 columns">
                                            <!--                    <form data-abide="ajax">-->
                                            <div class="large-12 columns">
                                                <div class="row collapse">
                                                    <div class="small-5 large-4 columns">
                                                        <span class="prefix">Project Name:</span>
                                                    </div>
                                                    <div class="small-7 large-8 columns">
                                                        <input type="text" placeholder="" required>
                                                        <small class="error">Your name is important.</small>
                                                    </div>
                                                </div>

                                                <div class="row collapse">
                                                    <div class="small-5 large-4 columns">
                                                        <span class="prefix">Default Model Type:</span>
                                                    </div>
                                                    <div class="small-7 large-8 columns">
                                                        <select required>
                                                            <option>Choice project with many alternatives</option>
                                                        </select>
                                                        <small class="error">Choose Default Type</small>
                                                    </div>
                                                </div>

                                            </div>

                                            <div class="large-12 columns text-right">
                                                <hr>
                                                <button type="submit" class="button tiny tt-button-primary tt-button-primary right">
                                                    <span class="text">Create</span>
                                                </button>
                                                <button data-reveal-id="selectProjMode" type="submit" class="button tiny tt-button-primary right">
                                                    <span class="text">Back</span>
                                                </button>
                                                <button type="button" aria-label="Close" class="close-reveal-modal button tiny tt-button-primary tt-button-transparent right custom-close-modal">
                                                    <span class="text">Cancel</span>
                                                </button>
                                            </div>
                                            <!--                    </form>-->
                                        </div>
                                    </div>
                                    <!-- // createNewProj modal wrap -->

                                    <!-- newFromTemplate modal wrap -->
                                    <div id="newFromTemplate" class="reveal-modal small tt-modal-wrap" data-reveal aria-labelledby="newTemp" aria-hidden="true" role="dialog">
                                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                        <h2 id="addPart" class="tt-modal-header blue-header">Create Project from Template <a  data-reveal-id="tt-create-from-template-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>

                                        <div class="tt-modal-content my-proj large-12 columns">
                                            <!--                    <form data-abide="ajax">-->
                                            <div class="large-12 columns">
                                                <div class="row collapse">
                                                    <div class="small-5 large-4 columns">
                                                        <span class="prefix">Project Name:</span>
                                                    </div>
                                                    <div class="small-7 large-8 columns">
                                                        <input type="text" placeholder="" required>
                                                        <small class="error">Project name is important.</small>
                                                    </div>
                                                </div>

                                                <div class="row collapse">
                                                    <div class="small-5 large-4 columns">
                                                        <span class="prefix">Template:</span>
                                                    </div>
                                                    <div class="small-7 large-8 columns">
                                                        <select required>
                                                            <option>Sample Project Template</option>
                                                        </select>
                                                        <small class="error">Choose Template</small>
                                                    </div>
                                                </div>

                                            </div>

                                            <div class="large-12 columns text-right">
                                                <hr>
                                                <button type="submit" class="button tiny tt-button-primary tt-button-primary right">
                                                    <span class="text">Create</span>
                                                </button>
                                                <button data-reveal-id="selectProjMode" type="submit" class="button tiny tt-button-primary right">
                                                    <span class="text">Back</span>
                                                </button>
                                                <button type="button" aria-label="Close" class="close-reveal-modal button tiny tt-button-primary tt-button-transparent right custom-close-modal">
                                                    <span class="text">Cancel</span>
                                                </button>
                                            </div>
                                            <!--                    </form>-->
                                        </div>
                                    </div>
                                    <!-- // newFromTemplate modal wrap -->

                                    <!-- uploadFile modal wrap -->
                                    <div id="uploadFile" class="reveal-modal small tt-modal-wrap" data-reveal aria-labelledby="upF" aria-hidden="true" role="dialog">
                                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                        <h2 id="addPart" class="tt-modal-header blue-header">Upload Project from File <a  data-reveal-id="tt-uploading-project-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>

                                        <div class="tt-modal-content my-proj large-12 columns">
                                            <!--                    <form data-abide="ajax">-->
                                            <div class="large-12 columns">
                                                <div class="row collapse">
                                                    <div class="small-5 large-4 columns">
                                                        <span class="prefix">Project Name:</span>
                                                    </div>
                                                    <div class="small-7 large-8 columns">
                                                        <input type="text" placeholder="" required>
                                                        <small class="error">Project name is important.</small>
                                                    </div>
                                                </div>

                                                <div class="row collapse">
                                                    <div class="small-5 large-4 columns">
                                                        <span class="prefix">Project File:</span>
                                                    </div>
                                                    <div class="small-7 large-8 columns">
                                                        <input type="file" id="fileupload">
                                                        <small class="error">Choose Template</small>
                                                    </div>
                                                </div>

                                            </div>

                                            <div class="large-12 columns text-right">
                                                <hr>
                                                <button type="submit" class="button tiny tt-button-primary tt-button-primary right">
                                                    <span class="text">Create</span>
                                                </button>
                                                <button data-reveal-id="selectProjMode" type="submit" class="button tiny tt-button-primary right">
                                                    <span class="text">Back</span>
                                                </button>
                                                <button type="button" aria-label="Close" class="close-reveal-modal button tiny tt-button-primary tt-button-transparent right custom-close-modal">
                                                    <span class="text">Cancel</span>
                                                </button>
                                            </div>
                                            <!--                    </form>-->
                                        </div>
                                    </div>
                                    <!-- // uploadFile modal wrap -->

                                    <!-- selectProjMode modal wrap -->
                                    <div id="selectProjMode" class="reveal-modal small tt-modal-wrap" data-reveal aria-labelledby="selProj" aria-hidden="true" role="dialog">
                                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                        <h2 id="addPart" class="tt-modal-header blue-header">Create Project <a  data-reveal-id="tt-create-project-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>

                                        <div class="tt-modal-content large-12 columns">
                                            <div class="large-9 large-centered columns">

                                                <button data-reveal-id="createNewProj" type="button" class="button expand tt-button-primary tt-button-primary tt-button-lite-blue">
                                                    <span class="text">Create New Project</span>
                                                </button>
                                                <br>
                                                <button data-reveal-id="newFromTemplate" type="button" class="button expand tt-button-primary tt-button-lite-blue">
                                                    <span class="text">New From Template</span>
                                                </button>

                                                <br>
                                                <button data-reveal-id="uploadFile" type="button" class="button expand tt-button-primary tt-button-lite-blue">
                                                    <span class="text">Upload a File</span>
                                                </button>
                                                <br>
                                                <button type="button" aria-label="Close" class="close-reveal-modal button expand tt-button-primary tt-button-transparent custom-close-modal">
                                                    <span class="text">Cancel</span>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- // selectProjMode modal wrap -->

                                    <!--create project help modal wrap -->
                                    <div id="tt-create-project-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                                        <!--                                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>-->
                                        <a class='close-reveal-modal second-modal-btn'>&#215;</a>
                                        <h2 id="modalTitle" class="tt-modal-header blue-header"><a class='back-to-parent create-project second-modal-btn'>Create Project</a> > Help</h2>
                                        <div class="tt-modal-content large-12 columns">

                                            <div class="row">
                                                <div class="large-12 columns">
                                                    <p>Here are some helpful tips on creating a project.</p>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="small-12 columns"><strong>Lorem Ipsum.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>
                                                <div class="small-12 columns"><strong>tempor an sunt si nam.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>
                                                <div class="small-12 columns"><strong>si expetendis nulla.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>

                                            </div>
                                        </div>
                                    </div>
                                    <!-- //create project help modal wrap -->

                                    <!--create new project help modal wrap -->
                                    <div id="tt-create-new-project-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                                        <!--                                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>-->
                                        <a class='close-reveal-modal second-modal-btn'>&#215;</a>
                                        <h2 id="modalTitle" class="tt-modal-header blue-header"><a class='back-to-parent create-new second-modal-btn'>Create New Project</a> > Help</h2>
                                        <div class="tt-modal-content large-12 columns">

                                            <div class="row">
                                                <div class="large-12 columns">
                                                    <p>Creating New Project</p>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="small-12 columns"><strong>Lorem Ipsum.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>
                                                <div class="small-12 columns"><strong>tempor an sunt si nam.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>
                                                <div class="small-12 columns"><strong>si expetendis nulla.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>

                                            </div>
                                        </div>
                                    </div>
                                    <!-- //create new project help modal wrap -->

                                    <!--create from template help modal wrap -->
                                    <div id="tt-create-from-template-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                                        <a class='close-reveal-modal second-modal-btn'>&#215;</a>
                                        <h2 id="modalTitle" class="tt-modal-header blue-header"><a class='back-to-parent create-new second-modal-btn'>Create Project from Template</a> > Help</h2>
                                        <div class="tt-modal-content large-12 columns">

                                            <div class="row">
                                                <div class="large-12 columns">
                                                    <p>Creating Project from Template</p>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="small-12 columns"><strong>Lorem Ipsum.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- //create from template help modal wrap -->

                                    <!--uploading project help modal wrap -->
                                    <div id="tt-uploading-project-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                                        <a class='close-reveal-modal second-modal-btn'>&#215;</a>
                                        <h2 id="modalTitle" class="tt-modal-header blue-header"><a class='back-to-parent upload-file second-modal-btn'>Uploading Project from File</a> > Help</h2>
                                        <div class="tt-modal-content large-12 columns">

                                            <div class="row">
                                                <div class="large-12 columns">
                                                    <p>Uploading Project</p>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="small-12 columns"><strong>Lorem Ipsum.</strong> commodo an domesticarum magna dolor amet domesticarum probant quo in</div>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- //uploading project help modal wrap -->


                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="large-3 columns tt-comments-wrap hide tt-right-col">
                        <!-- sidebar-->
                        <%@ Register TagPrefix="MainPages" TagName="SidebarRight" Src="~/Pages/sidebar.ascx" %>
                        <MainPages:SidebarRight ID="SidebarRight" runat="server" />

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
                <% } %>


              </div>
     <script type="text/javascript">
          

         $(".fullwidth-loading-wrap").removeClass("hide");
         $(".fullwidth-loading-wrap").css("display", "inline");

         var is_my_projects = true;

         $(document).ready(function () {
             $("#PasscodeLink").focus(function () { this.select(); });
             if ($("#select_workgroup").length > 0) {
                 ////console.log("select exists");
                 $("#select_workgroup").select2();
             } else {
                 console.log("select does not exists");
             }
             setTimeout(function(){
//                 $('.proj-title-orig-wrap').remove();
             }, 1000);
         });

         function CopyToClipboard() {
             var controlValue = document.getElementById('PasscodeLink');
             $('#PasscodeLink').select();
         }

         function showlink(ID) {

             $.ajax({
                 type: "POST",
                 url: baseUrl + "Pages/my-projects.aspx/GetLink",
                 contentType: "application/json; charset=utf-8",
                 data: JSON.stringify({
                     ProjectID: ID
                 }),
                 success: function (data) {
                     $("#PasscodeLink").val('<%=(Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath).ToString() + "?passcode="%>' + data.d)
                 },
                 error: function (response) {
                     console.log(response);
                 }
             });
        }
        // $(window).on("resize", function () {
        //     console.log($(".proj-title").width());
        //     $(".proj-title-duplicate").width($(".proj-title").width());
        //     $(".proj-ol-duplicate").width($(".proj-ol").width());
        //     $(".proj-status-duplicate").width($(".proj-status").width());
        //     $(".proj-extra-columns-duplicate").width($(".proj-extra-columns").width());
        //});
         

     </script>   
            </asp:Content>