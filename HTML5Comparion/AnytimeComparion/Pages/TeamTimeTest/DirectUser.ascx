<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DirectUser.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.DirectUser" ClassName="DirectUser" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<link href="/Content/stylesheets/nouislider.css" rel="stylesheet" />
<link href="/Content/stylesheets/nouislider.pips.css" rel="stylesheet" />
<link href="/Content/stylesheets/nouislider.tooltips.css" rel="stylesheet" />
<link href="/Content/stylesheets/directcomparison.css" rel="stylesheet" />
<div ng-init="init()" class="ds-div-trigger">  
    <div class="large-12 columns questionsHeader">
        <includes:QuestionHeader runat="server" id="QuestionHeader" /> 
    </div>

    
    <div class="columns tt-question-choices questionsWrap">
        <div class="medium-4 columns">
             <includes:LeftNodeInfoDoc runat="server" id="LeftNodeInfoDoc" />     
        </div>
        <div class="medium-4 columns">
            <includes:ParentNodeInfoDoc runat="server" id="ParentNodeInfoDoc" />    
        </div>
        <div class="medium-4 columns">
           <includes:WRTLeftNodeInfoDoc runat="server" id="WRTLeftNodeInfoDoc" />
        </div>
    </div>

    <div class="large-12 columns disable-when-pause">
    <div class="tt-judgements-item large-12 columns">
        <div class="columns tt-j-content">
            <div class="row tt-j-result teamtime-direct-results-wrap">
                <!------------------------------------------------------------------->
                <!-- Start Viewing Options & Pagination Row -->   
                <!------------------------------------------------------------------->
                         <div class="tt-paginate-users large-10 columns large-centered">
                            <div class="row collapse teamtime-pagination-row">
                                <div ng-if="output.isPM" class="small-4 medium-3 large-3 columns">                                           
                                   <span class="sort-label show-for-large-up">Show</span> <select ng-model="user_display" ng-change="set_user_display(user_display.value)" ng-options="obj.text for obj in user_display_options track by obj.value" class="toggle-user-display toggle-name-email">
                                   </select>
                                </div>
                                <div class="small-7 medium-5 large-6 columns text-center user-pagination pagination-centered" ng-if="show_participant">
                                    <ul  class="pagination verbal-paginate verbal" ng-if="users_list.length > 5"><!--ng-repeat="page in pagination_pages"-->
                                        <li ng-repeat="page in pagination_pages" ng-hide="(page > pagination_CurrentPage || page < pagination_CurrentPage) && page != 'all' && page != 'select' && page != '<' && page != '>'">
                                        <a class="arrow" data-arrow="left" ng-click="pagination_save('decrement', pagination_CurrentPage - 1)" ng-if="page == '<'">&laquo;</a>
                                        <a class="{{pagination_CurrentPage == page ? 'current' : ''}} paginate-users" ng-if="page == pagination_CurrentPage && page != '<' && page != '>' && page != 'all' && page != 'select'">
                                            {{page}}
                                        </a>
                                        <select class="show-smart users-verbal" ng-model="pagination_NoOfUsers" ng-options="item for item in pagination_select_list" ng-change="pagination_save('option' , pagination_NoOfUsers)" ng-if="page == 'select'">
                                        </select>
                                        <a ng-click="pagination_save('all', page)" class="paginate-all {{pagination_CurrentPage == 0 ? 'current' : ''}}" ng-if="page == 'all'">{{page}}</a>
                                        <a ng-click="pagination_save('increment', pagination_CurrentPage + 1)" class="arrow" data-arrow="right" ng-if="page == '>'" >&raquo;</a>
                                    </li>
                                    </ul>
                              </div>
                              <div class="small-1 medium-3 large-3 columns text-right right">                                   
                                        <a class="" id="idr" data-slide-toggle=".participant-div" data-slide-toggle-duration="400" ng-click="update('', 'individual', '', baseurl, '', '', '')">
                                            <span class="iconstyle icon-tt-minus-square icon" ng-if="show_participant"></span>
                                            <span  ng-click="pagination_save('click', pagination_CurrentPage)" class="iconstyle icon-tt-plus-square icon" ng-if="show_participant == false"></span>
                                            <div class="hide-for-small">
                                                <span  class="togname text" ng-if="show_participant">Hide Individual</span>
                                                <span  class="togname text"  ng-click="pagination_save('click', pagination_CurrentPage)" ng-if="show_participant == false">Show Individual</span>
                                            </div>
                                        </a>
                                    
                                </div>
                            </div>                           
                            </div>
                <!----------------------------------------------------------------->
                <!-- End Viewing Options & Pagination Row  -->
                <!----------------------------------------------------------------->
                </div>
                                    
                <div class="large-10  columns small-centered text-center teamtime-direct-results">
                    <ul class="tt-drag-slider-wrap direct-user participants-list">
                        <li ng-init="user_index=$index" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers " ng-if="check_hide_offline(user[3]) && (check($index) || (current_user[0] == user[1].toLowerCase()))">
                            <div 
                                id="{{user[0]}}"
                                class="large-12 columns tt-drag-slider-item " 
                                ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div' "
                                >
                                <div class="large-4 medium-4 small-12 columns no-more-padding">
                                    <div id="pending_{{$index}}" style="display: none;" class="tt-pending-label radius secondary label pendingIndicator pending_{{$index}}">
                                        <span class="icon-tt-clock"></span>
                                        <span class="text">Pending</span>
                                    </div>
                                    <span class="tt-user-status online online" ng-if="user[3] == 1">&#9679;</span>
                                    <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en">
                                        <span ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                        <span ng-style="{color:get_user_color(user[4])}"  ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>
                                         <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                    </div>
                                </div>
                                <%--input readonly: {{ user[4]==-2 }}
                                input disabled: {{ (meeting_owner != current_user[0] && current_user[0] != user[1].toLowerCase()) || (meeting_owner == current_user[0] && user[4]==-2) }}
                                slider disabled: {{ meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase() ? false : true }}

                                Slider Value: {{ direct_sliders[$index] }}--%>

                                <div class="large-2 medium-2 small-3 columns no-more-padding" >
                                    <input
                                        ng-readonly="user[4]!=0"
                                        min="0"
                                        max="1" 
                                        ng-model="direct[users_list.indexOf(user)]"
                                        ng-disabled="(meeting_owner != current_user[0] && current_user[0] != user[1].toLowerCase()) || (meeting_owner == current_user[0] && user[4]==-2)" 
                                        ng-change="update_slider(event, users_list.indexOf(user), user[1], user[6], 'up')" 
                                        ng-keypress="update_slider($event, users_list.indexOf(user), user[1], user[6], 'press')"
                                        ng-keyup="update_slider($event, users_list.indexOf(user), user[1], user[6], 'up')" 
                                        id="direct-input-{{users_list.indexOf(user)}}"  
                                        <%--ng-focus="move_to_next_row(user[0] - 1)"--%>
                                        step="0.01" 
                                        type="number" 
                                        style="width:90%;" 
                                        class="dsInputClr" 
                                        data-index="{{users_list.indexOf(user)}}" 
                                        ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()">
                                </div>

                                <div class="large-4 medium-4 small-8 columns graphical-nouislider no-more-padding"  ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()" >
                                <div 
                                    ng-style="{opacity:get_opacity(user[4], current_user[0], user)}" 
                                    id="direct-dsBarClr{{users_list.indexOf(user)}}" 
                                    ng-init="initializeNoUiDirect(direct[users_list.indexOf(user)], users_list.indexOf(user), user)"
                                    value="{{direct[users_list.indexOf(user)]}}"
                                    <%--slider--%>
                                    disable="{{meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase() ? false : true}}"
                                    index="{{user[0]}}" 
                                    user="{{user[1]}}"
                                    user_list="{{users_list.length}}" 
                                    cur_user="{{current_user[1]}}" 
                                    comment="{{user[6]}}" 
                                    ng-model="direct[users_list.indexOf(user)]"
                                    permission="{{user[4]}}"></div>
                                    </div>
                                <div class="medium-2 small-1 columns text-right reset-result-btn-wrap no-more-padding" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()">
                                    <a class="icon-tt-close reset-btn teamtime-page" title="" ng-if="(meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4] == 0 && user[5] != -2147483648000"
                                        ng-click="reset_slider(users_list.indexOf(user), user[1], user[6])"
                                        <%--ng-click="update([-2147483648000,user[1],user[6]],'save','mtDirect', baseurl, users_list.length, user[1], users_list.indexOf(user))"--%>><span  id="reset_{{$index}}" data-index="{{$index}}" data-mtype="mtDirect" data-email="{{ user[1] }}"></span></a>
                                </div>
                            </div>

                        </li>
                    <li class="group-result-wrap">
                    <%--<div id="erasebtn" class="large-12 columns text-center" ng-if="user_judgment > -9">
                        <a ng-click="update([-2147483648000,current_user[0],comment_txt[1]],'save','mtDirect', baseurl, users_list.length,   user[1], $index)"
                            class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise"><span class="icon-tt-close icon"></span></a>
                    </div>--%>

                        
                    </li>
                    </ul>
                      <div class="row tt-j-result" ng-if="!(!TTOptions.showToMe && TTOptions.hideJudgments)">                        
                        <div class="small-12 medium-12 large-12 columns no-more-padding">
                            <!-- owner result -->
                            <div class="row teamtime-group-result-row direct-user">
                                <div class="small-12 medium-4 large-4 columns small-only-text-center medium-text-left large-text-left">
                                    <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                                </div>
                                <div class="small-2 medium-2 large-2 columns text-center" ng-if="!(!TTOptions.showToMe && TTOptions.hideJudgments)"><!--Priority:--><strong>{{group_result[0] >= 0 ? (group_result[0] | number:2) : '' }}</strong></div>                                
                                <div class="small-10 medium-5 large-5 columns tt-j-result-title end">
                                    <div class="medium-12 large-12 columns direc-comparison-wrap du-progress-wrap" style="padding-top: 2px; margin-left: 0px; width: 100%;">
                                        <div class="dc-group-result-bar" ng-style="{'width': (group_result[0] > 0 ? group_result[0] * 100 : 0) + '%'}"></div>
                                    </div>                                    
                                </div>                                                       
                            </div>
                            <div class="row">
                                <div class="large-12 columns text-center group-result-figure no-bottom-margin">Geometric Variance: {{group_result[1] | number:2}}</div>
                            </div>                                                    
                            <!-- //owner result -->
                        </div>
                    </div>
                
                </div>
            </div>


            
        </div>
    </div>
</div>
<!--<div class="row detect-scroll-wrap">Scroll down to see more details</div>-->
<!--<div class="row text-center detect-scroll-div-bottom ds-div-bottom hide-for-medium-down"></div>-->
    <script>
    var Obj;
    var NodeNum;
    var id;
    $('.edit-pencil').on('click', function() {
        Obj = $(this).attr("data-obj");
        NodeNum = $(this).attr("data-element");

    });

    $('.cancelbtn').on('click', function() {
        $('#editCon-3').foundation('reveal', 'close');
    });

    </script>
