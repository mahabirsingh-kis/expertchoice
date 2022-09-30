<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UtilityCurve.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.UtilityCurve" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<div class="ds-div-trigger">
    <div class="large-12 columns questionsHeade">
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

    <div class="large-12 columns disable-when-pause utility-curve-screen">
    <div class="tt-judgements-item large-12 columns utility-curve-screen">
       <!-- <div class="columns tt-j-title">
           <%-- <h3>Judgments
            </h3>--%>
        </div>-->
        <div id="code" class="columns"></div>
        <div class="large-uncollapse small-collapse tt-j-content tt-rating-scale-wrap" style="margin-bottom:0px !Important">            
                <div class="medium-12 large-5 columns medium-centered large-uncentered">
<!------------------------------------>
<!-------- Data Graph Toggle --------->
<!------------------------------------>
<ul class="accordion teamtime-graph-accordion" data-accordion>
  <li class="accordion-navigation">
    <a href="#graphpanel" class="hide-for-large-up text-center">Hide Graph/Show Graph</a>
    <div id="graphpanel" class="content active">
       <div class="large-12 small-12 columns small-centered text-centered tt-utility-curve-wrap">
             <canvas id="uCurve" class="u-curve">Your browser does not support HTML5 Canvas</canvas>             
             <input id="uCurveInput" type="number" step="any" name="" class="curve-slider-dragger"  value="{{ current_user[3] == -2147483648000 ? '' : current_user[3] }}" ng-model="current_user[3]"  ng-change="save_utility_curve(current_user[3], current_user[0], comment_txt[1], 0)">
             <div id="erasebtn" ng-if="current_user[3] > -9">
                 <a id="" ng-click="save_utility_curve('-2147483648000', current_user[0], comment_txt[1], 0)" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise radius"><span class="icon-tt-close icon"></span></a>
             </div>             
       </div>       
    </div>
  </li>
</ul>

<!------------------------------------>
<!------ End Data Graph Toggle ------->
<!------------------------------------>                    
                </div>
                <div class="medium-12 large-7 columns tt-j-others-result medium-centered large-uncentered" style="z-index:9">
                <!-------------------------------------------->
                <!-- Start Viewing Options & Pagination Row -->   
                <!-------------------------------------------->     
                    <div class="tt-paginate-users">
                                        <div class="row collapse teamtime-pagination-row teamtime-data-pagination">
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
                <!-------------------------------------------->
                <!-- End Viewing Options & Pagination Row -->   
                <!-------------------------------------------->


                    <div class="row">
                        <div>
                            <ul class="tt-drag-slider-wrap teamtime-data-rows-wrap participants-list utility-curve">
                              <li  ng-init="user_index=$index" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers " ng-if="check_hide_offline(user[3]) && (check($index) || (current_user[0] == user[1].toLowerCase()))" >
                                  <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div' "
                                      class=" tt-drag-slider-item teamtime-data-row" data-index="{{$index}}">
                                      <div class="small-12 large-5 columns teamtime-user-column">
                                          <span class="tt-user-status online online" ng-hide="user[3] == 0">&#9679;</span>
                                          <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en">
                                            <span  ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                            <span  ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>
                                               <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                          </div>
                                      </div>
                                      <div ng-style="{opacity:get_opacity(user[4])}" class="small-4 large-2 columns tg-ratings teamtime-user-rating" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()"> 
                                            <input 
                                                 ng-readonly="user[4]!=0"
                                                 ng-disabled="(meeting_owner != current_user[0] && current_user[0] != user[1].toLowerCase()) || (meeting_owner == current_user[0] && user[4]==-2)" 
                                                type="number" step="0.01" class="input-curve {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}"
                                                 style="cursor: auto;" ng-model="uc_input[$index]" ng-change="save_utility_curve(uc_input[$index], user[1], user[6], $index);">
                                       </div>
                                      <div ng-style="{opacity:get_opacity(user[4])}" class="small-7 large-4 columns text-right  teamtime-user-judgment" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()"
                                            ng-init="calculate_uc_priority( (user[5] < lowest_value && user[5] != -2147483648000) ? 0 : user[5], $index)">
                                            <div class="small-4 large-3 columns text-center"> 
                                                <span class="curve-priority-{{user[0]}} priority-sign {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}">{{uc_priority[$index]}}</span>
                                            </div>
                                            <div class="small-8 large-9 columns direc-comparison-wrap" >
                                                <div class="curve-bar-{{user[0]}} dc-group-result-bar progress-sign {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}" ng-style="uc_pwidth[$index]"></div>
                                            </div>            
                                      </div>
                                     <div class="small-1 medium-1 columns large-text-right small-text-center reset-result-btn-wrap" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()">
                                          <a title="" ng-if="(meeting_owner == current_user[0] || current_user[0] ==  user[1])" 
                                              ng-click="save_utility_curve(-2147483648000, user[1], user[6], $index)"><span  id="reset_{{$index}}" data-index="{{$index}}" data-mtype="mtRatings" data-email="{{ user[1] }}" class="icon-tt-close reset-btn teamtime-page"></span></a>
                                    </div>
                                  </div>
                              </li>   
                            </ul>
                        </div>
                        <!------------------------>
                        <!-- Start Group Result -->
                        <!------------------------>     
                        <div class="row tt-j-result utility-curve"  ng-if="!(!TTOptions.showToMe && TTOptions.hideJudgments)">                       
                                <!-- owner result -->
                                <div class="tt-rating-scale-group-result ">
                                    <div class="the-clear-fix">&nbsp;</div>
                                                <div class="tt-j-result-title small-12 medium-12 large-12 columns medium-centered">   
                                                        <div class="teamtime-group-result-row utility-curve row">
                                                        <div class="small-12 medium-4 large-5 columns small-text-center medium-text-left large-text-left">
                                                            <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                                                        </div> 
                                                        <div class="medium-8 large-7 columns">
                                                            <div class="large-5 medium-5 columns large-text-left medium-text-center small-text-center tt-clear-padding-left" >Priority: {{group_result[0] >= 0 ? (group_result[0] | number:2) : '' }}</div>
                                                            <div class="large-7 medium-7 columns rating-scale-wrap">
                                                                <div class="dc-group-result-bar" ng-style="{'width': (group_result[0] > 0 ? group_result[0] * 100 : 0) + '%'}"></div>
                                                            </div>
                                                        </div>                                        
                                                    </div>                         
                                                </div>                            
                                                <div class="row">
                                                    <div class="large-12 columns small-centered text-center group-result-figure">
                                                        Geometric Variance: {{group_result[1] | number:2}}
                                                    </div>
                                                </div>
                                </div>
                                <!-- //owner result -->            
                        </div>
                        <!------------------------>
                        <!-- End Group Result -->
                        <!------------------------>
                    </div>
                </div>  
        </div>
        <!------------------------>
        <!-- Start Group Result -->
        <!------------------------>     
        <!-- Original Location -->
        <!------------------------>
        <!-- End Group Result -->
        <!------------------------>
    </div>
</div>
</div>                                  
<div class="row detect-scroll-wrap">Scroll down to see more details</div>
<div class="row text-center detect-scroll-div-bottom ds-div-bottom hide-for-medium-down"></div>                                    
<script type="text/javascript">
    is_utility_curve = true;
    var dummy_value = 1;
    var page_load = false;
</script>