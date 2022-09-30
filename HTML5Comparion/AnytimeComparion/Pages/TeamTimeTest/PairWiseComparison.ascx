<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PairWiseComparison.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.PairWiseComparison" ClassName="PairWiseComparison" %>
<!-- Questions -->
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/RightNodeInfoDoc.ascx" TagPrefix="includes" TagName="RightNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTRightNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTRightNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
 <link href="/Content/stylesheets/nouislider.css" rel="stylesheet" />
<link href="/Content/stylesheets/nouislider.pips.css" rel="stylesheet" />
<link href="/Content/stylesheets/nouislider.tooltips.css" rel="stylesheet" />
<link href="/Content/stylesheets/pairwise.css" rel="stylesheet" />

<%-- Desktop Codes --%>
                                
<div class=" ds-div-trigger" ng-init="get_gradient_checker();init()">

    <div class="large-12 columns questionsHeader">
        <includes:QuestionHeader runat="server" id="QuestionHeader" />
    </div>
 

    <div class="large-12 columns questionsWrap hi-test">
        <includes:ParentNodeInfoDoc runat="server" id="ParentNodeInfoDoc" />     

       <div class="columns tt-question-choices five-px-height"><br/></div>
       

        <div data-equalizer class="columns tt-question-choices hide-for-medium-down ">
            <div class="medium-6 columns">
                <includes:LeftNodeInfoDoc runat="server" id="LeftNodeInfoDoc" />     
                <includes:WRTLeftNodeInfoDoc runat="server" id="WRTLeftNodeInfoDoc" />
            </div>
            <div class="vs">VS</div>
            <div class="medium-6 columns">
                <includes:RightNodeInfoDoc runat="server" id="RightNodeInfoDoc" /> 
                <includes:WRTRightNodeInfoDoc runat="server" id="WRTRightNodeInfoDoc" />    
            </div>
        </div>
    </div>
    <%-- //Questions --%>
          
    <%-- Verbal --%>         
    <div class="large-12 columns hide-for-medium-down disable-when-pause">
        <div class="tt-judgements-item large-12 columns " ng-if="current_mtype == 1 || current_mtype == 'ptVerbal'">
            <!--<div class="row tt-j-title">
                <h3>Judgments</h3>
            </div>-->
            <div class="row tt-j-content ">
                <div class="large-12 text-center ">

                    <!-- Verbal Bars only Here -->
                    <div class="row tt-j-content tt-verbal-bars-wrap current-user-main-judgment">
                        <div class="small-12 columns text-center">
                            <ul class="tt-equalizer-levels" ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1">
                                <li title="" class="nine lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Extreme</span>
                                    <!--<asp:Image ID="Image2" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-1 eight lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Very Strong<br>
                                        to Extreme</span>
                                    <!--<asp:Image ID="Image3" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="seven lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Very Strong</span>
                                    <div class="verbal-level-arrow very-strong-left">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image4" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-2 six lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Strong to<br>
                                        Very Strong</span>
                                    <!--<asp:Image ID="Image5" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="five lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Strong</span>
                                    <div class="verbal-level-arrow strong-left">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image6" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-3 four lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Equal to<br>
                                        Strong</span>
                                    <!--<asp:Image ID="Image7" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="three lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Moderate</span>
                                    <div class="verbal-level-arrow moderate-left">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image8" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-4 two lft tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Equal to<br>
                                        Moderate</span>
                                    <!--<asp:Image ID="Image9" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>

                                <li title="" class="zero mid tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Equal</span>
                                    <div class="verbal-level-arrow equal">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image10" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>

                                <li title="" class="lvl-even lvl-e-5 two rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Equal to<br>
                                        Moderate</span>
                                    <!--<asp:Image ID="Image11" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="three rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Moderate</span>
                                    <div class="verbal-level-arrow moderate-right">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image12" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-6 four rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Moderate
                                        <br>
                                        to Strong</span>
                                    <!--<asp:Image ID="Image17" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="five rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Strong</span>
                                    <div class="verbal-level-arrow strong-right">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image13" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-7 six rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Strong to<br>
                                        Very Strong</span>
                                    <!--<asp:Image ID="Image14" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="seven rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Very Strong</span>
                                    <div class="verbal-level-arrow very-strong-right">
                                        <div class="arrow-down"></div>
                                    </div>
                                    <!--<asp:Image ID="Image18" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="lvl-even lvl-e-8 eight rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt hide">Very Strong<br>
                                        to Extreme</span>
                                    <!--<asp:Image ID="Image15" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                                <li title="" class="nine rgt tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                    <span class="lvl-txt">Extreme</span>
                                    <!--<asp:Image ID="Image16" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>-->
                                </li>
                            </ul>
                            <ul ng-style="{opacity:get_opacity(current_user[2])}" id="pm-bars" class="tt-equalizer">
                                <li id="1" ng-click="tt_update(['-8',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length,  current_user[0], 0);" title="Extreme" class="pm-bars nine tg-gradients {{user_judgment == -8 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-1' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>9</span></li>
                                <li id="2" ng-click="tt_update(['-7',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Very Strong to Extreme" class="pm-bars eight lvl-even-bar tg-gradients {{user_judgment <= -7 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-2' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>8</span></li>
                                <li id="3" ng-click="tt_update(['-6',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Very Strong" class="pm-bars seven tg-gradients {{user_judgment <= -6 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-3' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>7</span></li>
                                <li id="4" ng-click="tt_update(['-5',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Strong to Very Strong" class="pm-bars six lvl-even-bar tg-gradients {{user_judgment <= -5 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-4' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>6</span></li>
                                <li id="5" ng-click="tt_update(['-4',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Strong" class="pm-bars five tg-gradients {{user_judgment <= -4 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-5' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>5</span></li>
                                <li id="6" ng-click="tt_update(['-3',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Equal to Strong" class="pm-bars four lvl-even-bar tg-gradients {{user_judgment <= -3 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-6' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>4</span></li>
                                <li id="7" ng-click="tt_update(['-2',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Moderate" class="pm-bars three tg-gradients {{user_judgment <= -2 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-7' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>3</span></li>
                                <li id="8" ng-click="tt_update(['-1',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Equal to Moderate" class="pm-bars two lvl-even-bar tg-gradients {{user_judgment <= -1 && user_judgment > -9 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'lft lft-eq lft-8' : ''}}" data-pos="lft" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>2</span></li>
                                <li id="9" ng-click="tt_update(['0',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Equal" class="pm-bars zero tg-gradients {{user_judgment > -9 && user_judgment < 0 ? 'lft-selected' : '' }} {{user_judgment < 9 && user_judgment > 0 ? 'rgt-selected' : '' }} {{user_judgment == 0 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'mid lft-9 lft-eq rgt-eq rgt-9 mid-9' : ''}}" data-pos="mid" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"> <span> 1</span></li>
                                <li id="8" ng-click="tt_update(['1',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Equal to Moderate" class="pm-bars two lvl-even-bar tg-gradients {{user_judgment >= 1 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-8' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>2</span></li>
                                <li id="7" ng-click="tt_update(['2',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Moderate" class="pm-bars three tg-gradients {{user_judgment >= 2 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-7' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>3</span></li>
                                <li id="6" ng-click="tt_update(['3',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Moderate to Strong" class="pm-bars four lvl-even-bar tg-gradients {{user_judgment >= 3 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-6' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>4</span></li>
                                <li id="5" ng-click="tt_update(['4',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Strong" class="pm-bars five tg-gradients {{user_judgment >= 4 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-5' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>5</span></li>
                                <li id="4" ng-click="tt_update(['5',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Strong to Very Strong" class="pm-bars six lvl-even-bar tg-gradients {{user_judgment >= 5 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-4' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>6</span></li>
                                <li id="3" ng-click="tt_update(['6',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Very Strong" class="pm-bars seven tg-gradients {{user_judgment >= 6 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-3' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>7</span></li>
                                <li id="2" ng-click="tt_update(['7',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Very Strong to Extreme" class="pm-bars eight lvl-even-bar tg-gradients {{user_judgment >= 7 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-2' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>8</span></li>
                                <li id="1" ng-click="tt_update(['8',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0);" title="Extreme" class="pm-bars nine tg-gradients {{user_judgment == 8 ? 'active-selected' : '' }} {{current_user[2] == 0 ? 'rgt rgt-eq rgt-1' : ''}}" data-pos="rgt" ng-class="main_gradient_checkbox ? '' : 'no-gradient'"><span>9</span></li>
                            </ul>
                            <div class="columns">
                            </div>
                        </div>
                    </div>

                    <div class="row tt-j-result pw-verbal">
                        <div class="row collapse">
                            <div id="erasebtn" class="large-8 columns large-centered text-center" ng-if="user_judgment > -9" >
                                <div class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise radius" ng-click="update(['-2147483648000',current_user[0],comment_txt[1]],'save','ptVerbal', baseurl, users_list.length, user[1], 0);"><span class="icon-tt-close icon"></span></div>
                            </div>
                            <div class="tt-j-others-result columns div1">
                                <!---------------------------------------------------------->
                                <!-- Viewing Options & Pagination for Verbal Large Screen -->
                                <!---------------------------------------------------------->
                                <div class="large-12 columns large-centered tt-paginate-users">
                            <div class="row collapse teamtime-pagination-row">
                                <div ng-if="output.isPM" class="small-4 medium-3 large-3 columns">                                           
                                   <span class="sort-label show-for-large-up">Show</span> <select ng-model="user_display" ng-change="set_user_display(user_display.value)" ng-options="obj.text for obj in user_display_options track by obj.value" class="toggle-user-display toggle-name-email">
                                   </select>
                                </div>
                                <div class="small-7 medium-5 large-6 columns text-center user-pagination pagination-centered" ng-if="show_participant">
                                    <ul class="pagination verbal-paginate verbal" ng-if="users_list.length > 5"><!--ng-repeat="page in pagination_pages"-->
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
                                <!-------------------------------------------------------------->
                                <!-- End Viewing Options & Pagination for Verbal Large Screen -->
                                <!-------------------------------------------------------------->
                                <!--------------------------------------->
                                <!-- Start PW Verbal Result Large Screen -->
                                <!--------------------------------------->                            
                                <div class="row teamtime-pw-result-large participants-list">
                                    <div class="large-12 columns large-centered tt-j-others-result-content teamtime-pw-verbal-result desktop" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers " ng-if="(check_hide_offline(user[3])) && (check($index) || (current_user[0] == user[1].toLowerCase()))"
                                        ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1">
                                    <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'  " data-index="{{$index}}" >
                                        <div class="large-2 small-12 columns small-text-left" >
                                            <span class="tt-user-status online online" ng-if="user[3] == 1">&#9679;</span>
                                                <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en" style="overflow:hidden">
                                                    {{get_user_style(user[4])}}
                                                    <span ng-style="{color:get_user_color(user[4])}"  ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                                    <span ng-style="{color:get_user_color(user[4])}"  ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>     
                                                    <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                                </div>
                                        </div>
                                        <div class="large-8 columns text-center verbal-bars">
                                            <ul ng-style="{opacity:get_opacity(user[4])}" 
                                                ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM) || current_user[0] == user[1].toLowerCase()"
                                                class="tt-desktop-bars tt-equalizer-result {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}">
                                                
                                                <li ng-style="{background:get_left_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_left" data-index="{{$parent.$index}}" id="left_{{bars[0] +''+ user[0] }}" 
                                                    ng-click="tt_update([bars[0],user[1],user[6], user[4]],'save','ptVerbal', baseurl, users_list.length,  user[1], $parent.$index)" 
                                                    title="{{bars[1]}}" 
                                                    class="point {{bars[2]}} lft  ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{(bars[0] >= user[5] && user[5] >-9) ? 'active' : ''}}" style="margin-left:4px;"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li> 

                                                <li data-index="{{$index}}" id="left_0{{ user[0] }}" 
                                                    ng-click="tt_update(['0',user[1],user[6], user[4]],'save','ptVerbal', baseurl, users_list.length,  user[1], $parent.$index)" 
                                                    title="Equal" 
                                                    class="point zero midd ownerResult left_0{{ user[0] }} tg-gradients {{(user[5] < 9 && user[5] > -9) && user[5] > 0 ? 'rgt-selected' : '' }} {{(user[5] < 9 && user[5] > -9) && user[5] < 0 ? 'lft-selected' : '' }} {{(user[5] < 9 && user[5] > -9) ? 'active' : '' }}"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>

                                                <li ng-style="{background:get_right_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_right" data-index="{{$parent.$index}}" 
                                                    id="left_{{bars[0] +''+ user[0] }}" 
                                                    ng-click="tt_update([bars[0],user[1],user[6], user[4]],'save','ptVerbal', baseurl, users_list.length,  user[1], $parent.$index)" 
                                                    title="{{bars[1]}}" 
                                                    class="point {{bars[2]}} rgt ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{bars[0] <= user[5] ? 'active' : '' }}" 
                                                    style="margin-right:4px;"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>
                                            </ul>
                                            
                                            <%--when judgments are hidden for all--%>
                                            <ul ng-style="{opacity:get_opacity(user[4])}" 
                                                ng-if="!(TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM) || current_user[0] == user[1].toLowerCase())"
                                                class="tt-desktop-bars tt-equalizer-result {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}">
                                                
                                                <li ng-style="{background:get_left_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_left" data-index="{{$parent.$index}}" id="left_{{bars[0] +''+ user[0] }}" 
                                                    class="{{bars[2]}} lft  ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients  {{user[5] > -9 && user[5] != 0 ? 'disabled' : 'no-judgment'}}" style="margin-left:4px;"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li> 

                                                <li data-index="{{$index}}" id="left_0{{ user[0] }}" 
                                                    class="point zero midd ownerResult left_0{{ user[0] }} tg-gradients  {{user[5] > -9 ? 'disabled' : 'no-judgment'}}"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>

                                                <li ng-style="{background:get_right_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_right" data-index="{{$parent.$index}}" 
                                                    id="left_{{bars[0] +''+ user[0] }}" 
                                                    class="{{bars[2]}} rgt ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{user[5] > -9 && user[5] != 0 ? 'disabled' : 'no-judgment'}}" 
                                                    style="margin-right:4px;"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>
                                            </ul>
                                        </div>

                                        <div class="large-2 columns text-right reset-result-btn-wrap" >
                                            <a title="" ng-if="user[4] == 0 && user[5] != -2147483648000 && (TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM)" 
                                                ng-click="update([-2147483648000,user[1],user[6]],'save','ptVerbal', baseurl, users_list.length,   user[1], $index)">
                                                <span  id="reset_{{$index}}" data-index="{{$index}}" data-mtype="ptVerbal" data-email="{{ user[1] }}" class="icon-tt-close reset-btn teamtime-page"></span>
                                            </a>
                                        </div>

                                    </div>        
                                    
                                </div>
                                </div>
                                <!--------------------------------------->
                                <!-- End PW Verbal Result Large Screen -->
                                <!--------------------------------------->
                            </div>
                        </div>
                    </div>
                    
                    
                </div>
            </div>
             <!--------------------------------------------------->
             <!-- Start PW Verbal Group Result Row Large Screen -->
             <!--------------------------------------------------->
            <div class="tt-j-result teamtime-group-result-row pw-verbal large-12 columns large-centered" style="margin-bottom: 20px;">
                <div class="large-12 columns large-centered">                    
                    <!-- owner result -->
                    <div class="row collapse">
                        <div class="large-12 columns tt-j-result-title"> 
                            <div class="medium-2 columns text-left" >
                                <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                            </div>
                            <div class="medium-8 columns text-center teamtime-pw-verbal-result teamtime-results-verbal-bars" >
                                <ul class="tt-equalizer-result" ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM)">
                                    <%--left bars--%>
                                    <li ng-repeat="bars in bars_left" id="res_{{bars[0]}}" 
                                        class="{{bars[2]}} lft  res_{{bars[0]}}" style="margin-left:4px;"
                                        ng-class="{'active-selected' : group_result[0] <= bars[0] && group_result[0] > -9, 'bar-one-fourth-blue' : group_result[0] < 0 && number_postive(bars[0]) == number_ceiling(number_postive(group_result[0])) && (( 1- (number_postive(bars[0]) - number_postive(group_result[0])) >= 0) && ( 1- (number_postive(bars[0]) - number_postive(group_result[0])) <=0.40)), 'bar-one-half-blue' : group_result[0] < 0 && number_postive(bars[0]) == number_ceiling(number_postive(group_result[0])) && ( 1- (number_postive(bars[0]) - number_postive(group_result[0])) <0.65) && ( 1- (number_postive(bars[0]) - number_postive(group_result[0])) >=0.41), 'bar-three-fourths-blue' : group_result[0] < 0 && number_postive(bars[0]) == number_ceiling(number_postive(group_result[0])) &&  (1- (number_postive(bars[0]) - number_postive(group_result[0])) >=0.65) }">
                                    </li>
                                    <%--middle bars--%>
                                    <li id="res_0" class="zero mid res_0"
                                        ng-class="{'lft-selected' : group_result[0] > -9 && group_result[0] < 0, 'rgt-selected' : group_result[0] < 9 && group_result[0] > 0, 'active' : group_result[0] == 0}" >
                                    </li>
                                    <%--right bars--%>
                                    <li ng-repeat="bars in bars_right" id="res_{{bars[0]}}" 
                                        class="{{bars[2]}} rgt  res_{{bars[0]}}" style="margin-right:4px;"
                                        ng-class="{'active-selected' : group_result[0] >= bars[0] && group_result[0] > 0, 'bar-three-fourths-green' : bars[0] == number_ceiling(group_result[0]) && ( 1 - (bars[0] - group_result[0])   > .65),'bar-one-half-green' : bars[0] == number_ceiling(group_result[0]) && (( 1- (bars[0] - group_result[0])>= .41)  && ( 1- (bars[0] - group_result[0])) <0.65),'bar-one-fourth-green' : bars[0] == number_ceiling(group_result[0]) && (( 1- (bars[0] - group_result[0]) >= 0) && ( 1- (bars[0] - group_result[0]) <=0.40))}">
                                    </li>
                                </ul>

                                <%--when judgments are hidden for all--%>
                                <ul class="tt-equalizer-result" ng-if="!(TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM))">
                                    <%--left bars--%>
                                    <li ng-repeat="bars in bars_left" id="res_{{bars[0]}}" 
                                        class="{{bars[2]}} lft  res_{{bars[0]}} disabled" style="margin-left:4px;">
                                    </li>
                                        
                                    <%--middle bars--%>
                                    <li id="res_0" class="zero mid res_0 disabled"></li>
                                        
                                    <%--right bars--%>
                                    <li ng-repeat="bars in bars_right" id="res_{{bars[0]}}" 
                                        class="{{bars[2]}} rgt  res_{{bars[0]}} disabled" style="margin-right:4px;">
                                    </li>
                                </ul>
                            </div>
                            <div class="medium-2 columns text-right"></div>
                        </div>
                    </div>                    
                    <!-- //owner result -->
                </div>                                 
            </div>
            <div class="row">
                   <div class="large-12 columns text-center group-result-figure"
                        ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM)">
                       Geometric Variance: {{group_result[1] | number:2}}
                   </div>
                </div>
            <!------------------------------------------------->
            <!-- End PW Verbal Group Result Row Large Screen -->
            <!------------------------------------------------->
        </div>
    </div>
    <%-- //Verbal --%>    



<%-- //Desktop Codes --%>
<%-- Mobile Codes --%>
<div class="hidden-for-large-up ">
    
    <%-- Questions for Mobile--%>
     <div class="row">
            <div class="tg-legend">
                <div class="row tt-question-choices" data-equalizer>
                    <div class="small-6 columns" >
                       <includes:LeftNodeInfoDoc runat="server" id="MobileLeftNodeInfoDoc" />     
                       <includes:WRTLeftNodeInfoDoc runat="server" id="MobileWRTLeftNodeInfoDoc" />  
                    </div>
                    <div class="small-6 columns">
                        <includes:RightNodeInfoDoc runat="server" id="MobileRightNodeInfoDoc" />     
                        <includes:WRTRightNodeInfoDoc runat="server" id="MobileWRTRightNodeInfoDoc" />  
                        

                    </div>
                </div>
            </div>

        </div>
    <%-- //Questions --%>
          
    <%-- Verbal --%>  
    <div class="tt-mobile-wrap disable-when-pause" ng-if="current_mtype == 1 || current_mtype == 'ptVerbal'"> 
        <div class="row">
            <div class="tt-judgements-item large-12 columns pw-verbal">


                <div class="columns tt-j-content">

                  <!-- show selected bar -->
                    <div class="columns selected-item-single-pw text-center " style="" ng-bind-html="show_selected_item(user_judgment)">
                                        </div>
                  <!-- /show selected bar -->

                    <div class="large-8 large-centered medium-8 medium-centered columns text-center teamtime-top-verbal-bars" ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1">
                      <ul ng-style="{opacity:get_opacity(current_user[2])}" id="pm-bars" class="tt-equalizer-mobile">
                             <li  
                                    id="{{$index+1}}" 
                                    title="{{bar[1]}}"
                                    class="{{bar[2]}} {{main_gradient_checkbox ? '' : 'no-gradient'}} pm-bars lft lft-eq lft-{{$index+1}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients"  
                                    data-pos="lft"
                                    ng-click="tt_update([bar[0],current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length,  current_user[0], 0);" 
                                    ng-class="{'active-selected' : bar[0] >= user_judgment && user_judgment >-9, 'lvl-even-bar' : bar[0]%2==0}" 
                                    ng-repeat="bar in bars_left track by $index" >
                                    <span ng-if="bar[3] != 'M'" class="bar-label">{{bar[3]}}</span>
                                    <span ng-if="bar[3] == 'M'" class="bar-label" style="top:-7px; position:relative">{{bar[3]}}</span>
                                </li>

                                <li 
                                    id="9" 
                                    title="Equal"
                                    class="pm-bars zero mid lft-9 lft-eq rgt-eq rgt-9 mid-9 {{main_gradient_checkbox ? '' : 'no-gradient'}} 
                                        {{user_judgment > -9 && user_judgment < 0 ? 'lft-selected' : '' }} {{user_judgment < 9 && user_judgment > 0 ? 'rgt-selected' : '' }} {{user_judgment == 0 ? 'active-selected' : '' }} tg-gradients" 
                                    data-pos="mid"
                                    ng-click="tt_update(['0',current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0)" 
                                    >                                            
                                    <span class="bar-label" style="display: block; height: 100%; font-size: 7px; margin-top: -1px;">EQ</span> 
                                    <span data-pos="-1" ng-if="user_judgment > -9" style="position: absolute;z-index: 1;top: 20px;margin-left: -8px;" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise radius" ng-click="update(['-2147483648000',current_user[0],comment_txt[1]],'save','ptVerbal', baseurl, users_list.length, user[1], 0); $event.stopPropagation()"><span class="icon-tt-close icon"></span></span>
                                </li>

                                <li id="{{bars_right.length-$index}}" 
                                    title="{{bar[1]}}" 
                                    class="{{bar[2]}} pm-bars rgt rgt-eq rgt-{{bars_right.length-$index}} {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients" 
                                    data-pos="rgt" 
                                    ng-click="tt_update([bar[0],current_user[0],comment_txt[1], current_user[2]],'save','ptVerbal', baseurl, users_list.length, current_user[0], 0) "
                                    ng-class="{'active-selected' : bar[0] <= user_judgment, 'lvl-even-bar' : bar[0]%2==0}" 
                                    ng-repeat="bar in bars_right track by $index" 
                                    >
                                    <span ng-if="bar[3] != 'M'" class="bar-label">{{bar[3]}}</span>
                                    <span ng-if="bar[3] == 'M'" class="bar-label" style="top:-7px; position:relative">{{bar[3]}}</span>
                                </li> 
                        </ul>
                    </div>
                </div>

                <div class="row collapse">
                    <%-- <div id="erasebtn" class="large-8 columns large-centered text-center" ng-if="user_judgment > -9">
                        <a id="" ng-click="update(['-2147483648000',current_user[0],comment_txt[1]],'save','ptVerbal', baseurl, users_list.length, user[1], 0);" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise">ERASE</a>
                    </div> --%>
                    <div class="tt-j-others-result tt-j-content columns">  
                     <!------------------------------------------------------------------->
                     <!-- Start Viewing Options & Pagination Row for Verbal Small to Medium Screen -->   
                     <!------------------------------------------------------------------->
                        <div class="row teamtime-pagination-row">
                            <div ng-if="output.isPM" class="small-4 medium-2 columns" >
                                <select ng-model="user_display" ng-init="user_display = user_display_options[0]" ng-change="set_user_display(user_display.value)"  ng-options="obj.text for obj in user_display_options track by obj.value" class="toggle-user-display toggle-name-email">
                                </select>
                            </div>
                            <div class="small-7 medium-8 columns text-center user-pagination pagination-centered" ng-if="show_participant">
                               <ul class="pagination verbal-paginate verbal" ng-if="users_list.length > 5">
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
                            <div class="small-1 medium-1 columns text-right right mikeboo">
                                <a class="" id="idr" data-slide-toggle=".participant-div" data-slide-toggle-duration="400" ng-click="update('', 'individual', '', baseurl, '', '', '')">
                                    <span class="iconstyle icon-tt-minus-square icon" ng-if="show_participant"></span>
                                    <span class="iconstyle icon-tt-plus-square icon" ng-click="pagination_save('click', pagination_CurrentPage)"  ng-if="show_participant == false"></span>
                                </a>
                            </div>
                        </div>
                        
                         <!--<div ng-if="!show_participant" class="small-7 columns" >
                            &nbsp;
                        </div>-->                        
                        <!--<div ng-if="!output.isPM" class="small-3 columns" >
                            &nbsp;
                        </div>-->
                        
                       <!---------------------------------------------------------------------------->
                       <!-- End Viewing Options & Pagination Row for Verbal Small to Medium Screen -->
                       <!---------------------------------------------------------------------------->

                       <!----------------------------------------------------->
                       <!-- Start Results for Verbal Small to Medium Screen -->
                       <!----------------------------------------------------->
                        <div class="large-12 columns tt-j-others-result-content teamtime-pw-result-small-medium">
                            <div class="row collapse">
                                <div class="large-12 columns">
                                    <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'  " 
                                        data-index="{{$index}}" 
                                        ng-if="(check_hide_offline(user[3])) && (check($index) || (current_user[0] == user[1].toLowerCase()))" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers "
                                        ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1">
                                        <div class="medium-8 medium-centered columns">
                                            <div id="pending_{{$index}}" style="display:none;" class="tt-pending-label radius secondary label pendingIndicator pending_{{$index}}">
                                                <span class="icon-tt-clock"></span>
                                                <span class="text">Pending</span>
                                            </div>
                                            <span class="tt-user-status online online" ng-hide="user[3] == 0">&#9679;</span>
                                            <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-nameOnly tt-emailOnly  tt-nameAndEmail tg-en">
                                                <span  ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                                <span  ng-style="{color:get_user_color(user[4])}" ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>
                                                <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                            </div>
                                        </div>
                                        <div class="medium-8 medium-centered small-12 columns text-center teamtime-page teamtime-results-verbal-bars">
                                            <ul ng-style="{opacity:get_opacity(user[4])}" 
                                                ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM) || current_user[0] == user[1].toLowerCase()"
                                                class="tt-equalizer-result-mobile {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}">

                                                <li ng-style="{background:get_left_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_left" data-index="{{$parent.$index}}" id="left_{{bars[0] +''+ user[0] }}" 
                                                    ng-click="tt_update([bars[0],user[1],user[6],user[4]],'save','ptVerbal', baseurl, users_list.length,  user[1], $parent.$index)" 
                                                    title="{{bars[1]}}" 
                                                    class="point {{bars[2]}} lft  ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{(bars[0] >= user[5] && user[5] >-9) ? 'active' : ''}}"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li> 

                                                <li data-index="{{$index}}" id="left_0{{ user[0] }}" 
                                                    ng-click="tt_update(['0',user[1],user[6],user[4]],'save','ptVerbal', baseurl, users_list.length,   user[1], $index)" 
                                                    title="Equal" 
                                                    class="point zero midd ownerResult left_0{{ user[0] }} tg-gradients {{(user[5] < 9 && user[5] > -9) && user[5] > 0 ? 'rgt-selected' : '' }} {{(user[5] < 9 && user[5] > -9) && user[5] < 0 ? 'lft-selected' : '' }} {{(user[5] < 9 && user[5] > -9) ? 'active' : '' }}"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>

                                                <li ng-style="{background:get_right_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_right" data-index="{{$parent.$index}}" 
                                                    id="left_{{bars[0] +''+ user[0] }}" 
                                                    ng-click="tt_update([bars[0],user[1],user[6],user[4]],'save','ptVerbal', baseurl, users_list.length,   user[1], $parent.$index)" 
                                                    title="{{bars[1]}}" class="point {{bars[2]}} rgt ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{bars[0] <= user[5] ? 'active' : '' }}" 
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>
                                            </ul>
                                            
                                            <%--when judgments are hidden for all--%>
                                            <ul ng-style="{opacity:get_opacity(user[4])}" 
                                                ng-if="!(TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM) || current_user[0] == user[1].toLowerCase())"
                                                class="tt-equalizer-result-mobile {{current_user[0].toLowerCase() == user[1].toLowerCase() ? '' : 'judgment-results-div'}}">

                                                <li ng-style="{background:get_left_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_left" data-index="{{$parent.$index}}" id="left_{{bars[0] +''+ user[0] }}" 
                                                    class="point {{bars[2]}} lft  ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{user[5] > -9 && user[5] != 0 ? 'disabled' : 'no-judgment'}}"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li> 

                                                <li data-index="{{$index}}" id="left_0{{ user[0] }}" 
                                                    class="point zero midd ownerResult left_0{{ user[0] }} tg-gradients {{user[5] > -9 ? 'disabled' : 'no-judgment'}}"
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>

                                                <li ng-style="{background:get_right_color(user[4], user[5])}"
                                                    ng-repeat="bars in bars_right" data-index="{{$parent.$index}}" 
                                                    id="left_{{bars[0] +''+ user[0] }}" 
                                                    class="point {{bars[2]}} rgt ownerResult left_{{ bars[0] +''+ user[0] }} tg-gradients {{user[5] > -9 && user[5] != 0 ? 'disabled' : 'no-judgment'}}" 
                                                    ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
                                                </li>
                                            </ul>

                                            <a title="" ng-if="user[4] == 0 && user[5] != -2147483648000 && (TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM)" 
                                                ng-click="update([-2147483648000,user[1],user[6]],'save','ptVerbal', baseurl, users_list.length,   user[1], $index)">
                                                <span  id="reset_{{$index}}" data-index="{{$index}}" data-mtype="ptVerbal" data-email="{{ user[1] }}" class="icon-tt-close reset-btn teamtime-page"></span>
                                            </a>
                                        </div>

                                        </div>
                                </div>
                            </div>
                        </div>
                       <!--------------------------------------------------->
                       <!-- End Results for Verbal Small to Medium Screen -->
                       <!--------------------------------------------------->
                    </div>

                </div>
                
                <!-- Group Result -->
                <!-- Start PW Verbal GROUP Result Row Small to Medium Screen -->
                <div class="row tt-j-content teamtime-group-result-row pw-verbal collapse" style="margin-bottom: 20px;">
                    <div class="large-12 tt-j-result columns">     
                            <!-- owner result -->
                            <div class="row">
                                <div class="tt-j-result-title"> 
                                    <div class="small-12 medium-8 medium-centered large-12 columns small-text-center">
                                         <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                                     </div>                           
                                    <div class="small-12 medium-8 large-6 columns text-center medium-centered teamtime-results-verbal-bars">
                                        <ul class="tt-equalizer-result-mobile" ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM)">
                                            <li ng-repeat="bars in bars_left" id="res_{{bars[0]}}" 
                                                class="{{bars[2]}} lft  res_{{bars[0]}}" style="margin-left:4px;"
                                                ng-class="{'active-selected' : group_result[0] <= bars[0] && group_result[0] > -9, 'bar-one-fourth-blue' : group_result[0] < 0 && number_postive(bars[0]) == number_ceiling(number_postive(group_result[0])) && (( 1- (number_postive(bars[0]) - number_postive(group_result[0])) >= 0) && ( 1- (number_postive(bars[0]) - number_postive(group_result[0])) <=0.40)), 'bar-one-half-blue' : group_result[0] < 0 && number_postive(bars[0]) == number_ceiling(number_postive(group_result[0])) && ( 1- (number_postive(bars[0]) - number_postive(group_result[0])) <0.65) && ( 1- (number_postive(bars[0]) - number_postive(group_result[0])) >=0.41), 'bar-three-fourths-blue' : group_result[0] < 0 && number_postive(bars[0]) == number_ceiling(number_postive(group_result[0])) &&  (1- (number_postive(bars[0]) - number_postive(group_result[0])) >=0.65) }">
                                            </li>
                                            <li id="res_0" class="zero mid res_0"
                                                ng-class="{'lft-selected' : group_result[0] > -9 && group_result[0] < 0, 'rgt-selected' : group_result[0] < 9 && group_result[0] > 0, 'active' : group_result[0] == 0}">
                                            </li>
                                            <li ng-repeat="bars in bars_right" id="res_{{bars[0]}}" 
                                                class="{{bars[2]}} rgt  res_{{bars[0]}}" style="margin-right:4px;"
                                                ng-class="{'active-selected' : group_result[0] >= bars[0] && group_result[0] > 0, 'bar-three-fourths-green' : bars[0] == number_ceiling(group_result[0]) && ( 1 - (bars[0] - group_result[0])   > .65),'bar-one-half-green' : bars[0] == number_ceiling(group_result[0]) && (( 1- (bars[0] - group_result[0])>= .41)  && ( 1- (bars[0] - group_result[0])) <0.65),'bar-one-fourth-green' : bars[0] == number_ceiling(group_result[0]) && (( 1- (bars[0] - group_result[0]) >= 0) && ( 1- (bars[0] - group_result[0]) <=0.40))}">
                                            </li>
                                        </ul>
                                        
                                        <%--when judgments are hidden for all--%>
                                        <ul class="tt-equalizer-result-mobile" ng-if="!(TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM))">
                                            <li ng-repeat="bars in bars_left" id="res_{{bars[0]}}" 
                                                class="{{bars[2]}} lft  res_{{bars[0]}} disabled" style="margin-left:4px;">
                                            </li>
                                            <li id="res_0" class="zero mid res_0 disabled">
                                            </li>
                                            <li ng-repeat="bars in bars_right" id="res_{{bars[0]}}" 
                                                class="{{bars[2]}} rgt  res_{{bars[0]}} disabled" style="margin-right:4px;">
                                            </li>
                                        </ul>
                                    </div>                                                                    
                                        <div 
                                            ng-if="current_mtype != 1"
                                            class="large-6 columns text-center">
                                            <div class="ratio-box">
                                                <div class="large-12 columns text-center rb-wrap">

                                                    <div class="large-8 columns large-centered">
                                                        <!--<input type="text" class="textbox left" value="4.300" readonly>-->
                                                    <!--<input type="text" class="textbox right" value="1.000" readonly>-->
                                                    </div>

                                                </div>

                                            </div>
                                        </div>
                                </div>

                                <div class="medium-6 columns tt-j-result-graph hide">
                                    <div class="columns tt-j-result-comment"></div>
                                </div>
                            </div>

                    </div>
                </div>
                <div class="row">
                     <div ng-class="{'large-12' : current_mtype == 1, 'large-6':current_mtype != 1}"
                          class="columns text-center group-result-figure pw-verbal" 
                          ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM)">
                          Geometric Variance: {{group_result[1] | number:2}}
                      </div>
                </div>
                <!-- End PW Verbal GROUP Result Row Small to Medium Screen -->
            </div>
        </div>
    </div>
    <%-- //Verbal --%>
</div>
<%-- // Mobile Codes --%>

<%-- Graphical --%>
<div class=" large-12 columns disable-when-pause" ng-if="(current_mtype == 9 || current_mtype == 99 || current_mtype == 'ptGraphical') && teamtime_body && renderTeamTime">
        <div class="tt-judgements-item large-12 columns" >
            <!--<div class="row tt-j-title">
                <h3>Judgments</h3>
            </div>-->
            <div class="row tt-j-content">
                <div class="large-12 text-center "> 
                    <%--<div class="columns"><label><input type="checkbox" ng-model="switch_to_old" ng-change="SwitchToOld(switch_to_old)" ng-init="switch_to_old=false" />Switch to Old Pairwise Graphical Slider</label></div>--%>
                    <!-- Graphical Pie Chart only Here -->
                    <div class="columns tt-j-content teamtime-graphical-main-slider-row current-user-main-judgment" ng-if="!TTOptions.singleLineMode">
                        <div class="row tt-pizza-wrap" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers ">
                            <%--<div ng-hide="TTOptions.hidePie" class="large-1 medium-3 small-6 columns large-centered medium-centered small-centered text-center tt-pizza-graph ">
                                <div id="pie"></div>
                                <div class="spaces hide"></div>
                            </div>--%>

                            <%--<div class="columns" ng-if=" ((current_user[0] == user[1].toLowerCase()) && user[4]==0) && !TTOptions.singleLineMode">
                                
                            </div>--%>
                            <%-- Start of main slider Old pairwise graphical --%>
                            <div class="row small-collapse large-uncollapse tt-j-result-graph pizza graphical-slider-wrap" ng-if="switch_to_old">
                                <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'" 
                                        data-index="{{$index}}" ng-if="((check_hide_offline(user[3])) && (check($index) && current_user[2] == user[0]) || (current_user[0] == user[1].toLowerCase()))">
                                <div class="large-2 medium-3 small-3 columns large-text-right" ng-if="(current_user[0] == user[1].toLowerCase()) && user[4]==0 && !TTOptions.singleLineMode">
                                    <%--<input ng-readonly="current_user[2]!=0" name="a-value" class="number-input" type="number" <%--ng-model="main_bar[0]" ng-model="left_input[$index]" tabindex="1" 
                                    ng-keypress="graphical_key_up($event, left_input[$index], current_user[0], comment_txt[1]], -1, 'press', $index, true)" ng-keyup="graphical_key_up($event, left_input[$index], current_user[0], comment_txt[1]], -1, 'up', $index, true)"
                                    ng-mouseup="graphical_key_up($event, left_input[$index], current_user[0], comment_txt[1]], -1, 'up', $index, true)">--%>
                                    <input 
                                            value="{{user[4]==-2 ? '' : left_input[user[0]]}}"
                                            ng-readonly="current_user[2]!=0" 
                                            name="a-value" 
                                            tabindex="{{3+$index}}" 
                                            class="number-input" 
                                            type="text" 
                                            ng-model="left_input[user[0]]" 
                                            <%--ng-style="nav_left_style[$index]"--%>  
                                            ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                            ng-keypress="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'press', user[0])" 
                                            ng-keyup="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'up', user[0])"  />
                                    <%--<input  ng-readonly="current_user[2]!=0" name="b-value" class="number-input" type="number" tabindex="2" 
                                    <%--ng-model="main_bar[1]" ng-model="right_input[$index]"
                                    ng-keypress="graphical_key_up($event, right_input[$index], current_user[0], comment_txt[1]], 1, 'press', $index, true)" ng-keyup="graphical_key_up($event, right_input[$index], current_user[0], comment_txt[1]], 1, 'up', $index, true)"
                                    ng-mouseup="graphical_key_up($event, right_input[$index], current_user[0], comment_txt[1]], 1, 'up', $index, true)">--%>
                                    <input 
                                            value="{{user[4]==-2 ? '' : right_input[user[0]]}}"
                                            ng-readonly="current_user[2]!=0" 
                                            name="a-value" 
                                            tabindex="{{3+$index}}" 
                                            class="number-input"
                                            type="text" 
                                            ng-model="right_input[user[0]]" 
                                            <%--ng-style="nav_left_style[$index]"--%>  
                                            ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                            ng-keypress="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], -1, 'press', user[0])" 
                                            ng-keyup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], -1, 'up', user[0])"  />
                                </div>
                                <div class="large-8 medium-6 small-6 columns large-text-right g-slider{{user[0]}}" ng-if="(current_user[0] == user[1].toLowerCase()) && user[4]==0 && !TTOptions.singleLineMode">
                                    <div ng-if="teamtime_body" participant-slider value="participant_slider[user[0]]" ng-model="participant_slider[user[0]]" permission="{{user[4]}}" judgment="{{user[5]}}" user="{{user[1]}}" comment="{{user[6]}}" style="height:20px" index="{{user[0]}}" class="columns slider graphicalSlider"
                                        disable="{{meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase() ? false : true}}">
                                    </div>
                                </div>
                                    <div class="large-2 medium-3 small-3 columns large-text-right" ng-if="(current_user[0] == user[1].toLowerCase()) && user[4]==0 && !TTOptions.singleLineMode">
                                        <div class="large-6 medium-6 small-6 columns large-text-right">
                                            <a class="swap-judgment" ng-click="swap_value(user_judgment, current_user[0], comment_txt[1], user[0])">
                                                <img src="../../Images/swap-icon.png" title="swap" alt="swap" width="30" class="point" />
                                            </a>
                                        </div>
                                        <div class="large-6 medium-6 small-6 columns large-text-right">
                                            <a id="pizza" ng-click="update(['-2147483648000',current_user[0],comment_txt[1]],'save','ptGraphical', baseurl, users_list.length, user[1], 0);"  class="button tiny tt-button-primary radius" ><span class="icon-tt-close icon"></span></a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <%-- End of main slider Old pairwise graphical --%>

                            <%-- Start of main slider pairwise graphical NoUiSlider --%>

                            <div class="row collapse large-uncollapse tt-j-result-graph pizza graphical-slider-wrap" ng-if="!switch_to_old">
                                <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'" 
                                        ng-if="((check_hide_offline(user[3])) && (check(users_list.indexOf(user)) && current_user[2] == user[0]) || (current_user[0] == user[1].toLowerCase()))">
                                    <div class="large-6 medium-8 small-12 columns large-centered medium-centered small-centered text-center" ng-if="(current_user[0] == user[1].toLowerCase())" ng-style="{opacity: get_opacity(user[4], current_user[0], user)}"
                                        id="mainSlider{{users_list.indexOf(user)}}" <%--ng-model="single_line_slider[user[0]]"--%> ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1" ng-model="main_slider[users_list.indexOf(user)]" ng-init="initializaNoUIGraphical(main_slider[users_list.indexOf(user)], main_slider[users_list.indexOf(user)] + 800, users_list.indexOf(user), user, true);">
                                    </div>
                                    <div class="small-12 medium-8 large-6 medium-centered large-centered large-text-left columns small-text-center" ng-if="(current_user[0] == user[1].toLowerCase())" ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1">
                                        <div class="large-12 medium-12  {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-7' : 'small-12'}} columns large-centered text-centered medium-centered small-centered graphical-nouislider">
                                            <div class="small-1 medium-3 large-3 columns text-left teamtime-pw-graphical-icon-wrap">
                                                <a class="swap-judgment"  ng-if="(meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4]==0 && user[5] != -2147483648000"
                                                    ng-click="swap_value(user[5], user[1], user[6], users_list.indexOf(user))">
                                                    <img src="../../Images/swap-icon.png" title="swap" alt="swap"  class="point" style="max-width:unset;width:22px" />
                                                </a>
                                            </div>
                                            <div class="small-5 medium-3 large-3 columns teamtime-pw-graphical-input-wrap-left">
                                                <input type="number" step="any" <%--ng-model="left_input[users_list.indexOf(user)]"--%> ng-model="left_bar[users_list.indexOf(user)]" 
                                                    tabindex="1" data-index="users_list.indexOf(user)"
                                                    ng-readonly="user[4]!=0" min="0" max="9"
                                                    ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                                    ng-change="graphical_key_up($event, [left_bar[users_list.indexOf(user)], user[1], user[6]], -1, 'up', users_list.indexOf(user), true)"
                                                    ng-keyup="graphical_key_up($event, [left_bar[users_list.indexOf(user)], user[1], user[6]], -1, 'up', users_list.indexOf(user), true)"
                                                    ng-keypress="graphical_key_up($event, [left_bar[users_list.indexOf(user)], user[1], user[6]], -1, 'press', users_list.indexOf(user), true)" 
                                                    <%--ng-mouseup="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'up', user[0])"--%>/>
                                            </div>
                                            <div class="small-5 medium-3 large-3 columns teamtime-pw-graphical-input-wrap-left">
                                                <input type="number" step="any" <%--ng-model="right_input[users_list.indexOf(user)]"--%> ng-model="right_bar[users_list.indexOf(user)]"
                                                    tabindex="1" data-index="users_list.indexOf(user)"
                                                    ng-readonly="user[4]!=0" min="0" max="9"
                                                    ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                                    ng-change="graphical_key_up($event, [right_bar[users_list.indexOf(user)], user[1], user[6]], 1, 'up', users_list.indexOf(user), true)" 
                                                    ng-keyup="graphical_key_up($event, [right_bar[users_list.indexOf(user)], user[1], user[6]], 1, 'up', users_list.indexOf(user), true)"
                                                    ng-keypress="graphical_key_up($event, [right_bar[users_list.indexOf(user)], user[1], user[6]], 1, 'press', users_list.indexOf(user), true)" 
                                                    <%--ng-mouseup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'up', user[0])"--%>/>
                                            </div>
                                            <div class="small-1 medium-3 large-3 columns text-right teamtime-pw-graphical-icon-wrap">
                                                <span class="button tiny tt-button-primary tt-action-btn-clr-j radius" ng-if="(meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4]==0 && user[5] != -2147483648000"
                                    ng-click="update([-2147483648000,user[1],user[6]],'save','ptGraphical', baseurl, users_list.length, user[1], users_list.indexOf(user))"><span class="icon-tt-close icon"></span></span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <%-- End of main slider pairwise graphical NoUiSlider --%>
                        </div>
                    </div>
                    <div class="row tt-j-result ">
                        <div class="row collapse">
                            <%--<div ng-if="!TTOptions.singleLineMode" class="small-12 columns tt-pizza-button-wrap  text-center" ng-if="user_judgment > lowest_value" style="clear: both;">
                                
                            </div>--%>
                            <div class="tt-j-others-result columns ">
                                <div class="row collapse">
                <!-------------------------------------------->
                <!-- Start Viewing Options & Pagination Row -->   
                <!-------------------------------------------->
                                     <div class="tt-paginate-users">
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
                <!-------------------------------------------->
                <!-- End Viewing Options & Pagination Row -->   
                <!-------------------------------------------->
                                    </div>

                            </div>
                            <%-- Start of participant's slider Old pairwise graphical --%>
                            <div class="row teamtime-pw-result-large small-collapse large-uncollapse pizza participants-list" ng-if="switch_to_old">
                                  <div class="large-12 columns tt-j-others-result-content mobile teamtime-pw-graphical-result participants-list" >
                                    <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'" 
                                        data-index="{{$index}}" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers " ng-if="(check_hide_offline(user[3])) && (check($index) || (current_user[0] == user[1].toLowerCase()))">
                                            <div class="medium-2 small-12 columns large-text-left">
                                                <!--<div id="pending_{{$index}}" style="display: none;" class="tt-pending-label radius secondary label pendingIndicator pending_{{$index}}">
                                                    <span class="icon-tt-clock"></span>
                                                    <span class="text">Pending</span>
                                                </div>-->
                                                <span class="tt-user-status online online" ng-hide="user[3] == 0">&#9679;</span>
                                                <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en">
                                                    <span ng-style="{color:get_user_color(user[4])}" 
                                                        ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                                    <span ng-style="{color:get_user_color(user[4])}" 
                                                        ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>
                                                    <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                                </div>
                                            </div>
                                           <div class="medium-8 small-10 columns tt-j-result-graph pizza graphical-slider-wrap" ng-style="{opacity:get_opacity(user[4])}"  >
                                                <div  class="medium-12 columns text-center g-slider{{user[0]}}" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()" >
                                                    <div class="row collapse" ng-if="!TTOptions.singleLineMode" >
                                                        <div class="large-2 medium-3 small-5 columns large-text-left">
                                                            <span class="content-left content-participant{{$index}}">
                                                                    <input type="text" step="any" ng-model="left_input[user[0]]" style="height:28px; font-size:0.8rem;"
                                                                        tabindex="1" class="number-input"
                                                                        ng-readonly="user[4]!=0" 
                                                                        ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                                                        ng-keyup="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'up', user[0])"
                                                                        ng-keypress="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'press', user[0])" 
                                                                        <%--ng-mouseup="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'up', user[0])"--%>/>

                                                                    <input type="text" step="any" ng-model="right_input[user[0]]" style="height:28px; font-size:0.8rem;"
                                                                        tabindex="1" class="number-input"
                                                                        ng-readonly="user[4]!=0" 
                                                                        ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                                                        ng-keyup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'up', user[0])"
                                                                        ng-keypress="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'press', user[0])" 
                                                                        <%--ng-mouseup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'up', user[0])"--%>/>
                                                                </span> 
                                                        </div>
                                                        <div class="large-10 medium-9 small-7 columns large-text-right">

                                                            <div ng-if="teamtime_body"  participant-slider value="participant_slider[user[0]]" ng-model="participant_slider[user[0]]" permission="{{user[4]}}" judgment="{{user[5]}}" user="{{user[1]}}" comment="{{user[6]}}" style="height:20px" index="{{user[0]}}" class="columns slider graphicalSlider"
                                                                disable="{{meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase() ? false : true}}">
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="row collapse" ng-if="TTOptions.singleLineMode" >
                                                        <div class="small-12 columns large-text-left small-text-center " style="padding-top:5px;">
                                                            <div class="large-11 columns large-centered text-center" 
                                                                id="gslider{{$index}}" ng-model="single_line_slider[user[0]]" ng-init="initializaNoUIGraphical(single_line_slider[user[0]], single_line_slider[user[0]] + 800, user[0], user);">
                                                            </div>
                                                        </div>
                                                        <div class="small-12 columns large-text-left small-text-center">
                                                            <div class="large-7 medium-6  {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-7' : 'small-12'}} columns large-centered text-centered medium-centered small-centered graphical-nouislider">
                                                                <div class="large-3 medium-2 {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-2' : 'small-3'}}  columns small-right text-right" style="margin-top:22px; padding-left:5px; padding-right:7px">
                                                                    <a class="swap-judgment"  ng-if=" (meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4]==0"
                                                                        ng-click="swap_value(user[5], user[1], user[6], user[0])">
                                                                        <img src="../../Images/swap-icon.png" title="swap" alt="swap"  class="point" style="max-width:unset;width:22px" />
                                                                    </a>
                                                                </div>
                                                                <div class="large-3 medium-4 {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns" style="margin-top:15px; padding-right:7px; padding-left:5px">
                                                                    <input type="text" step="any" ng-model="left_input[user[0]]" style="height:28px; font-size:0.8rem;"
                                                                        tabindex="1" 
                                                                        ng-readonly="user[4]!=0" 
                                                                        ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                                                        ng-keyup="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'up', user[0])"
                                                                        ng-mousedown="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'press', user[0])" 
                                                                        ng-mouseup="graphical_key_up($event, [left_input[user[0]], user[1], user[6]], -1, 'up', user[0])"/>
                                                                </div>
                                                                <div class="large-3 medium-4 {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns"  style="margin-top:15px;  padding-left:7px; padding-right:5px">
                                                                    <input type="text" step="any" ng-model="right_input[user[0]]" style="height:28px; font-size:0.8rem;"
                                                                        tabindex="1" 
                                                                        ng-readonly="user[4]!=0" 
                                                                        ng-disabled="current_user[0] != meeting_owner && current_user[0] != user[1].toLowerCase()"
                                                                        ng-keyup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'up', user[0])"
                                                                        ng-mousedown="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'press', user[0])" 
                                                                        ng-mouseup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'up', user[0])"/>
                                                                </div>
                                                                <div class="large-3 medium-2 {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-2' : 'small-3'}} columns small-left text-left" style="margin-top:22px; padding-left:5px">
                                                                    <span class="button tiny tt-button-primary tt-action-btn-clr-j " ng-if=" (meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4]==0"
                                                        ng-click="update([-2147483648000,user[1],user[6]],'save','ptGraphical', baseurl, users_list.length,   user[1], $index)"><span class="icon-tt-close icon"></span></span>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>

                                            </div> 
                                            <div class="medium-1 small-1 columns large-text-right small-text-center" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()" >
                                                <a class="swap-judgment" ng-if=" ((meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4]==0) && !TTOptions.singleLineMode"
                                                    ng-click="swap_value(user[5], user[1], user[6], user[0])" ><span id="reset_{{$index}}" data-index="{{$index}}" data-mtype="ptGraphical" data-email="{{user[1]}}"
                                                        data-email="{{user[1]}}"></span>
                                                    <img src="../../Images/swap-icon.png" title="swap" alt="swap" class="point" />
                                                </a>
                                            </div>

                                            <div class="medium-1 small-1 columns large-text-center small-text-right reset-result-btn-wrap" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()">
                                                <a title="" ng-if=" ((meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4]==0) && !TTOptions.singleLineMode"
                                                        ng-click="update([-2147483648000,user[1],user[6]],'save','ptGraphical', baseurl, users_list.length,   user[1], $index)" ><span id="reset_{{$index}}" data-index="{{$index}}" data-mtype="ptGraphical" data-email="{{user[1]}}" class="icon-tt-close reset-btn teamtime-page "></span></a>
                                            </div>
                                </div>
                            </div>
                            </div>
                            <%-- End of participant's slider Old pairwise graphical --%>

                            <%-- Start of participant's slider pairwise graphical NoUiSlider commit--%>
                            <div class="row small-collapse large-uncollapse tt-j-result-graph pizza graphical-slider-wrap " ng-if="!switch_to_old">
                                <div class="large-12 columns mobile teamtime-pw-graphical-result participants-list ">
                                    <div class="">
                                    <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'" 
                                        data-index="{{$index}}" ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers " ng-if="(check_hide_offline(user[3])) && (check($index) || (current_user[0] == user[1].toLowerCase()))"
                                          ng-hide="user[1].toLowerCase() == meeting_owner && TTOptions.hideProjectManager == 1">

                                        <div class="row">
                                             <div class="small-12 medium-8 medium-centered large-2 large-uncentered columns text-left">
                                                <!--<div id="pending_{{$index}}" style="display: none;" class="tt-pending-label radius secondary label pendingIndicator pending_{{$index}}">
                                                    <span class="icon-tt-clock"></span>
                                                    <span class="text">Pending</span>
                                                </div>-->
                                                <span class="tt-user-status online online" ng-hide="user[3] == 0">&#9679;</span>
                                                <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en">
                                                    <span ng-style="{color:get_user_color(user[4])}" 
                                                        ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                                    <span ng-style="{color:get_user_color(user[4])}" 
                                                        ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>
                                                    <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                                </div>
                                            </div>

                                    <%--<div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div'" 
                                            data-index="{{$index}}" ng-if="((check_hide_offline(user[3])) && (check($index) && current_user[2] == user[0]) || (current_user[0] == user[1].toLowerCase()))">--%>
                                            <div class="columns small-12 medium-10 large-10 medium-centered large-uncentered">                                                                                
                                            <div class="row">
                                                <div class="small-12 medium-10 medium-centered large-12 {{$parent.screen_sizes.option <= 3 && $parent.screen_sizes.option >= 2 ? 'small-7' : 'small-12'}} columns text-right graphical-nouislider">
                                                    <div class="row">                                                        
                                                        <div class="small-12 medium-12 large-6 large-push-4 columns">
                                                            <div class="row">
                                                                <div ng-style="{opacity: get_opacity(user[4], current_user[0], user)}"
                                                                id="gslider{{users_list.indexOf(user)}}" ng-model="single_line_slider[users_list.indexOf(user)]" ng-init="initializaNoUIGraphical(single_line_slider[users_list.indexOf(user)], single_line_slider[users_list.indexOf(user)] + 800, users_list.indexOf(user), user)">
                                                            </div>
                                                            </div>
                                                        </div>
                                                        <div class="small-1 medium-3 large-1 large-push-4 columns teamtime-pw-graphical-icon-wrap small-text-left medium-text-left large-text-center">
                                                                <a class="swap-judgment" ng-if="(meeting_owner == current_user[0] || current_user[0] == user[1].toLowerCase()) && user[4] == 0 && user[5] != -2147483648000"
                                                                    ng-click="swap_value(user[5], user[1], user[6], users_list.indexOf(user))">
                                                                    <img src="../../Images/swap-icon.png" title="swap" alt="swap"  class="point" style="max-width:unset;width:22px" />
                                                                </a>&nbsp;
                                                        </div>
                                                        <div class="small-5 medium-3 large-2 large-pull-7 columns teamtime-pw-graphical-input-wrap-left">
                                                            <input type="number" step="any" ng-if="user[4] == 0" 
                                                                ng-model="left_input[users_list.indexOf(user)]"
                                                                ng-value="left_input[users_list.indexOf(user)] >= 0 ? left_input[users_list.indexOf(user)] : ''" 
                                                                tabindex="1" data-index="users_list.indexOf(user)" class="left-input{{users_list.indexOf(user)}}"
                                                                ng-readonly="!((TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM))" 
                                                                ng-disabled="!((TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM))"
                                                                ng-change="graphical_key_up($event, [left_input[users_list.indexOf(user)], user[1], user[6]], -1, 'up', users_list.indexOf(user))" 
                                                                ng-keyup="graphical_key_up($event, [left_input[users_list.indexOf(user)], user[1], user[6]], -1, 'up', users_list.indexOf(user))"
                                                                ng-keypress="graphical_key_up($event, [left_input[users_list.indexOf(user)], user[1], user[6]], -1, 'press', users_list.indexOf(user))" 
                                                                <%--ng-mouseup="graphical_key_up($event, [left_input[users_list.indexOf(user)], user[1], user[6]], -1, 'up', users_list.indexOf(user))"--%> />
                                                            <span ng-if="user[4] != 0">&nbsp;</span>
                                                        </div>
                                                        <div class="small-5 medium-3 large-2 large-pull-7 columns teamtime-pw-graphical-input-wrap-right">
                                                            <input type="number" step="any" ng-if="user[4] == 0" 
                                                                ng-model="right_input[users_list.indexOf(user)]"
                                                                ng-value="right_input[users_list.indexOf(user)] >= 0 ? right_input[users_list.indexOf(user)] : ''" 
                                                                tabindex="1" data-index="users_list.indexOf(user)" class="right-input{{users_list.indexOf(user)}}"
                                                                ng-readonly="!((TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM))" 
                                                                ng-disabled="!((TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM))"
                                                                ng-change="graphical_key_up($event, [right_input[users_list.indexOf(user)], user[1], user[6]], 1, 'up', users_list.indexOf(user))"
                                                                ng-keyup="graphical_key_up($event, [right_input[users_list.indexOf(user)], user[1], user[6]], 1, 'up', users_list.indexOf(user))"
                                                                ng-keypress="graphical_key_up($event, [right_input[users_list.indexOf(user)], user[1], user[6]], 1, 'press', users_list.indexOf(user))" 
                                                                <%--ng-mouseup="graphical_key_up($event, [right_input[user[0]], user[1], user[6]], 1, 'up', user[0])"--%> />
                                                            <span ng-if="user[4] != 0">&nbsp;</span>
                                                         </div>
                                                        <div class="small-1 medium-3 large-1 columns teamtime-pw-graphical-icon-wrap small-text-right medium-text-right large-text-center">                                                                
                                                            <span class="button tiny tt-button-primary tt-action-btn-clr-j" 
                                                                ng-if="user[4] == 0 && user[5] != -2147483648000 && (TTOptions.hideJudgments == 0 || current_user[0] == user[1].toLowerCase() || $parent.showToMe) && (current_user[0] == user[1].toLowerCase() || output.isPM)"
                                                                ng-click="clear_value(user[1], user[6], users_list.indexOf(user))">
                                                                <span class="icon-tt-close icon"></span>
                                                            </span>
                                                        </div>

                                                    </div>
                                               </div>
                                            </div>
                                        </div>
                                      </div>

                                    </div>
                                    </div>
                               </div>
                            </div>
                            <%-- End of participant's slider pairwise graphical NoUiSlider --%>

                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row tt-j-result">                
                                          
                <!-- owner result -->                
                    <div class="large-12 columns tt-j-result-title teamtime-group-result-row pw-graphical">
                        <!----------------------------------> 
                        <!-- Start Graphical Group Result -->
                        <!----------------------------------> 
                        <div class="row collapse">
                            <div class="small-12 medium-8 medium-centered large-5 large-uncentered columns medium-text-center small-text-center large-text-left">
                                <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                            </div>                             
                            <div class="small-12 medium-9 medium-centered small-centered large-5 large-uncentered columns tt-j-result-graph pizza">
                                <ul class="columns" ng-if="group_result[0] > lowest_value && (TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM))">
                                    <li class="left">
                                        <div class="columns bar-a-result" ng-style="{'width' : (group_result[0] < 0 ? (((group_result[0]* -1) + 8)/16)*100 : 100 - ((group_result[0] + 8)/16)*100) + '%'}">
                                            <span  class="percent clr-j-percent-pizza-result">{{ group_result[0] <= 0 ? (group_result[0]* -1) + 1 : 1 | number:2}}</span>
                                        </div>
                                    </li>
                                    <li class="right">
                                        <div class="columns bar-b-result" ng-style="{'width' : (group_result[0] > 0 ? (((group_result[0]) + 8)/16)*100 : 100 - (((group_result[0]* -1) + 8)/16)*100) + '%'}">
                                            <span class="percent clr-j-percent-pizza-result">{{group_result[0] >= 0 ? (group_result[0] * 1) + 1 : 1 |number:2}}</span>
                                        </div>
                                    </li>
                                </ul>
                                <ul class="columns" ng-if="group_result[0] < lowest_value || !(TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM))">
                                    <li class="left">
                                        <div class="columns bar-a-result" ng-style="{'width' : '50%', 'background-color' : '#AAAAAA'}">
                                        </div>
                                    </li>
                                    <li class="right">
                                        <div class="columns bar-b-result" ng-style="{'width' : '50%', 'background-color' : '#AAAAAA'}">
                                        </div>
                                    </li>
                                </ul>
                                <div class="the-clear-fix">&nbsp;</div>                                 
                            </div>  
                        </div>                        
                      </div>                
                      <div class="row"> 
                           <div class="large-12 columns text-center group-result-figure pw-graphical ng-binding"
                                ng-if="TTOptions.hideJudgments == 0 || ($parent.showToMe && output.isPM)">
                               Geometric Variance: {{group_result[1] | number:2}}
                           </div>
                      </div>
                <!--------------------------------> 
                <!-- End Graphical Group Result -->
                <!--------------------------------> 
            </div>
            
 
     
        </div>
</div>
<%-- //Graphical --%>  

<!--    <div class="row detect-scroll-wrap">Scroll down to see more details</div>-->
<!--    <div class="row text-center detect-scroll-div-bottom ds-div-bottom hide-for-medium-down"></div>-->
<script src="/Scripts/Pipe/PairwiseVerbal.js"></script> 
<script src="/Scripts/Pipe/PairwiseGraphical.js"></script> 
<script type="text/javascript">
    $(document).ready(function () {
        setTimeout(function(){
            $(".tt-accordion-head").removeClass("back-pulse");
        }, 7000);

        $(".tt-toggler")
            .click(function () {
                clearInterval(timer);
                var node = $(".icon", this).attr("data-node");
                var is_mobile = $(this).attr("data-mobile");
                var this_id = $(this).attr('id');
                var parent = $(this).parent().parent();
                var sub_visible = $('.tg-accordion-sub-' + this_id, parent).is(':hidden');
                clearInterval(timer);
                //console.log(node);
                <% if(TeamTimeClass.isTeamTimeOwner){%>
                save_dropdown(node, sub_visible, '<%=ResolveUrl("~/")%>');
                <%}else{%>
                var baseUrl = '<%= ResolveUrl("~/") %>';
                $.ajax({
                    type: "POST",
                    url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/HideAndShowInfoDocs",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        index: node,
                        value: sub_visible
                    }),
                    success: function(data) {
                        //resize_infodoc_node(output.infodoc_sizes);
                        setInterval(timer, 1000);
                    },
                    error: function(response) {
                        console.log(JSON.stringify(response));
                    }
                });
                <%}%>
            });

    });
</script>
