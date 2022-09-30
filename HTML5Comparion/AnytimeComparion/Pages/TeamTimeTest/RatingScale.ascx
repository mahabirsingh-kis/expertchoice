<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RatingScale.ascx.cs" Inherits="AnytimeComparion.test.RatingScale" ClassName="RatingScale" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
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

    <div class="large-12 columns disable-when-pause tt-clear-padding-640">
        <div class="tt-judgements-item large-12 columns tt-rating-scale-wrap rating-scale">
        
        <div class="row large-uncollapse small-collapse tt-j-content tt-rating-scale-wrap">
            <div class="row large-uncollapse small-collapse">
              <div class="large-7 medium-12 columns tt-j-others-result tt-rating-scale-content">
                
                <!-------------------------------------------->
                <!-- Start Viewing Options & Pagination Row -->   
                <!-------------------------------------------->                  
                    <div class="tt-paginate-users">
                                        <div class="row collapse teamtime-pagination-row rating-scale-pagination">
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

                  <div class="row large-uncollapse small-collapse">
                      <div>
                          <ul class="tt-drag-slider-wrap participants-list teamtime-ratings">
                              <li  ng-init="user_index=$index"  my-repeat-directive ng-repeat="user in users_list | startTo : pagination_NoOfUsers : pagination_CurrentPage  | limitTo:pagination_CurrentPage == 0 ? users_list.length : pagination_NoOfUsers "  ng-if="check_hide_offline(user[3]) && (check($index) || (current_user[0] == user[1].toLowerCase()))">
                                  <div ng-class=" current_user[0] == user[1].toLowerCase() ? 'large-12 columns active user-div' : 'large-12 columns participant-div user-div' "
                                      class=" tt-drag-slider-item " data-index="{{$index}}">
                                      <div class="large-5 columns ">
                                        <span class="tt-user-status online online" ng-if="user[3] == 1">&#9679;</span>
                                        <div class=" user_{{$index}} userdisplay{{ user[0] }} tt-emailOnly tt-nameOnly tt-nameAndEmail tg-en" style="overflow:hidden">
                                            {{get_user_style(user[4])}}
                                            <span ng-style="{color:get_user_color(user[4])}"  ng-if="TTOptions.isAnonymous == 1">{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : 'Person ' + $index}}</span>
                                            <span ng-style="{color:get_user_color(user[4])}"  ng-if="TTOptions.isAnonymous == 0" >{{current_user[0] == user[1].toLowerCase() ? 'Your Judgment' : (user_display.value==0) ? user[2] : (user_display.value==1) ? user[1] : user[2] + '(' + user[1] + ')'}}</span>     
                                            <span ng-if="user[4] == -1" ng-style="{color:get_user_color(user[4])}" >(View Only)</span>
                                        </div>
                                      </div>
                                      
                                      <div ng-style="{opacity:get_opacity(user[4])}" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()" class="large-4 medium-8 small-7 columns text-right " >

                                       <div id="ratings-dropdown{{$index}}" class="tt-hidden-select-wrap">
                                            <div data-index="{{$index}}" class="columns progress hidden-progress-bar" 
                                                ng-class="current_user[0].toLowerCase() == meeting_owner.toLowerCase() || current_user[0].toLowerCase() == user[1].toLowerCase() ? 'hidden-toggle-btn hidden-toggle-btn-{{index}} htd_{{$index}} htd' : ''">
                                                <span ng-model="ratings_percentage[$index]" class="meter"
                                                    ng-style="{'width' :display_user_rating( user[5] ) < 0 || user[4]==-2  ? 0 : (display_user_rating( user[5]) * 100) + '%'}">
                                                </span>
                                                <span ng-if="user[4]!=-2" ng-model="ratings_dropdown[$index]" class="text-detail">      {{display_user_dropdown(user[5])}}</span>
                                                <span ng-if="user[4]==-2" ng-model="ratings_dropdown[$index]" class="text-detail"> </span>
                                                
                                                <span id="{{$index}}" class="icon-tt-chevron-up close-drop-down cdd_{{$index}} hide point"></span>
                                                <span class="icon-tt-chevron-down down-arrow arrow_{{$index}}"></span>
                                            </div>
                                            <ul ng-if="user[4]==0" data-index="{{$index}}" class="hidden-item-wrap">
                                                <li ng-click="save_ratings(item[0], user[1],user[6], user_index)"
                                                    ng-if="output.showPriorityAndDirect || item[1] != 'Direct Value'"
                                                    ng-repeat="item in pipe_measurements" 
                                                    data-user-index="{{user_index}}" 
                                                    data-index="{{user_index}}" 
                                                    class="hidden-item hidden-item-btn">
                                                    <a 
                                                        ng-class="{'selected': item[1] == display_user_dropdown(user[5])}"
                                                        id="selected-ratings-{{user_index}}-{{$index}}">
                                                        {{item[1]}}
                                                        <span class="right" ng-if="output.showPriorityAndDirect && item[0] >= 0 && item[1] != 'Direct Value'">
                                                            &nbsp;{{(item[2] * 100) | number:2}}%
                                                        </span>
                                                    </a>
                                                </li>
                                            </ul>
                                        </div>
                                          
                                     </div>
                                      <div ng-style="{opacity:get_opacity(user[4])}" class="large-2 medium-3 small-4 columns tg-ratings" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()"> 
                                           <%-- for particpant --%>
                                             <input 
                                                id="rating-input-box-{{user_index}}"
                                                ng-change="mtRatings_direct_value(ratings_number_value[$index], user[1], user[6], user_index )"
                                                ng-if="output.showPriorityAndDirect && current_user[0].toLowerCase() != meeting_owner.toLowerCase()" 
                                                ng-readonly="current_user[0].toLowerCase() != user[1].toLowerCase()" 
                                                ng-model ="ratings_number_value[$index]"
                                                step="0.1" type="number" min="0" max="1" 
                                               ng-value = "display_user_rating(user[5]) < 0 < 0 || user[4]==-2 ? '' : display_user_rating(user[5])"
                                                 />
                                            <%-- for pm --%>
                                            <input 
                                                ng-readonly="user[4]!=0" 
                                                id="rating-input-box-{{user_index}}"
                                                ng-change="mtRatings_direct_value(ratings_number_value[$index], user[1], user[6], user_index )"
                                                ng-if="output.showPriorityAndDirect && current_user[0].toLowerCase() == meeting_owner.toLowerCase()"
                                                ng-model ="ratings_number_value[$index]"
                                                step="0.1" type="number" min="0" max="1"
                                                 ng-value= "display_user_rating(user[5]) < 0 || user[4]==-2 ? '' : display_user_rating(user[5])">
                                       </div>
                                      
                                     <div class="medium-1 small-1 columns large-text-right small-text-center reset-result-btn-wrap" ng-if="!(TTOptions.hideJudgments) || (output.isPM && $parent.showToMe) || current_user[0] == user[1].toLowerCase()">
                                          <a title="" ng-if="output.showPriorityAndDirect && (meeting_owner == current_user[0] || current_user[0] ==  user[1].toLowerCase()) && (user[5] >= 0 || user[5].toString().indexOf('*') >=0) && user[4]==0" 
                                              ng-click="update([-2147483648000,user[1],user[6]],'save','mtRatings', baseurl, users_list.length,   user[1], $index)"><span  id="reset_{{$index}}" data-index="{{$index}}" data-mtype="mtRatings" data-email="{{ user[1] }}" class="icon-tt-close reset-btn teamtime-page"></span></a>
                                    </div>
                                  </div>
                              </li>          
                          </ul>
            <!------------------------------->
            <!-- Start Rating Group Result -->
            <!------------------------------->
            <div class="row">
                <div class="small-12 medium-12 large-12 columns">
                    <div class="row tt-j-result teamtime-group-result-row teamtime-rating"  ng-if="!(!TTOptions.showToMe && TTOptions.hideJudgments)">                    
                       <div class="small-12 medium-12 large-5 columns small-text-center medium-text-center large-text-left">
                           <span class="icon-tt-chart icon"></span> <strong>GROUP RESULT</strong>
                       </div>
                        <div class="small-12 medium-12 large-7 columns teamtime-data-group-result-row no-bottom-margin no-more-padding">
                            <!-- owner result -->
                            <div class="row tt-rating-scale-group-result small-collapse medium-uncollapse">  
                                        <div class="small-12 medium-12 large-12 columns tt-j-result-title">                                            
                                            <div class="small-7 medium-8 large-7 columns rating-scale-wrap">
                                                <div class="dc-group-result-bar" ng-style="{'width': (group_result[0] > 0 ? group_result[0] * 100 : 0) + '%' }"></div>
                                            </div>
                                            <div class="small-5 medium-4 large-5 columns text-center" >Priority: {{group_result[0] >= 0 ? (group_result[0] | number:2) : '' }}</div>
                                    </div>
                            </div>
                        </div>
                </div>
                <div class="row">
                    <div class="large-12 columns small-centered text-center group-result-figure"> <!-- tt-geometric-variance-wrap -->
                       Geometric Variance: {{group_result[1] | number:2}}
                    </div>
                </div>
            </div>
            </div>
        <!------------------------------->
        <!--- End Rating Group Result --->
        <!------------------------------->
                      </div>
                  </div>
              </div>
              <div class="large-5 hide-for-medium-down columns">

                    <ul class="tt-rating-scale large-12 columns">
                        <div class="large-12 columns tt-rating-header">
                            <div class="small-6 columns text-left">
                                Intensity Name
                            </div>
                            <div ng-if="output.showPriorityAndDirect" class="small-6 columns large-text-center small-text-right">
                                Priority
                            </div>
                        </div>
                        <li ng-if="output.showPriorityAndDirect || measurement[1] != 'Direct Value'" class="row collapse tt-rs-list li-{{measurement[0]}}" ng-repeat="measurement in pipe_measurements track by $index">
                            <label>
                                <div class="small-6 columns">
                                    <div class="small-2 columns">
                                        <input ng-if="$index != pipe_measurements.length - 1" type="radio" id="{{measurement[0]}}" ng-checked="user_judgment == measurement[0]" ng-click="set_current_user_ratings(measurement[0])"   class="radio-judgement rs" name="radioTest" >

                                        <input ng-if="$index == pipe_measurements.length - 1" type="radio" id="{{measurement[0]}}" ng-checked="user_judgment.indexOf('*') >=0" class="radio-judgement rs" name="radioTest" >
                                    </div>
                                    <div class="small-10 columns">
                                        <span class="existing_priority-name ex-{{measurement[0]}}">{{measurement[1]}}</span>
                                        <input id="{{measurement[0]}}" type="text" name="" class="edit_priority-name er-inpt er-{{measurement[0]}}" value="{{measurement[1]}}" placeholder="{{measurement[1]}}">
                                       <a id="{{measurement[0]}}" ng-if="$index != pipe_measurements.length - 1" class="editThisRating hide" title="" href="#">
                                           <span class="icon-tt-pencil"></span>
                                       </a>
                                    </div>
                                </div>
                                <div ng-if="output.showPriorityAndDirect" class="small-6 columns text-right">
                                    <div class="small-10 columns" ng-if="$index != pipe_measurements.length - 1">
                                        <div class="rs-result-bar">
                                            <span class="bar columns bar-{{measurement[0]}} {{user_judgment == measurement[0] ? 'green' : ''}}" ng-style="{'width': '{{(measurement[2] * 100) | number:2}}%'}"></span>
                                        </div>
                                    </div>
                                    <div class="small-2 columns text-right" ng-if="$index != pipe_measurements.length - 1">
                                         <span class="bar-text" ng-if="$index !=  pipe_measurements.length - 2">
                                             {{(measurement[2] * 100) | number:2}}%
                                         </span>
                                         <span class="bar-text"  ng-if="$index ==  pipe_measurements.length - 2" ></span>
                                     </div>
                                    <div class="small-10 columns" ng-if="$index == pipe_measurements.length - 1">
                                         <input type="text" ng-change="mtRatings_direct_value(ratings_direct_values[0], current_user[0], comment_txt[1], 0 )" ng-model="ratings_direct_values[0]" value="ratings_direct_values[0]" name="name" />
                                    </div>
                                </div>
                            </label>
                        </li>
                        <li>
                             <div class="columns large-centered text-center" ng-if="output.showPriorityAndDirect && user_judgment > -9 || user_judgment.toString().indexOf('*') >= 0">
                                <a id="" ng-click="update([-2147483648000,current_user[0],comment_txt[1]],'save','mtRatings', baseurl, users_list.length,   user[1], $index)" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise radius"><span class="icon-tt-close icon"></span></a>
                            </div>
                        </li>
                    </ul>
              </div>

            </div>
            <!------------------------------->
            <!-- Start Rating Group Result -->
            <!------------------------------->
        <!------------------------------->
        <!--- End Rating Group Result --->
        <!------------------------------->
        </div>

       


    </div>
    </div>

</div>
<!--<div class="row detect-scroll-wrap">Scroll down to see more details</div>-->
<!--<div class="row text-center detect-scroll-div-bottom ds-div-bottom hide-for-medium-down"></div>-->
            
<script>
    $(document).ready(function () {
        var Obj;
        var NodeNum;
        var id;
        $('.edit-pencil').on('click', function () {
            Obj = $(this).attr("data-obj");
            NodeNum = $(this).attr("data-element");

        });

        $('.cancelbtn').on('click', function () {
            $('#editCon-3').foundation('reveal', 'close');
        });

        $(".tt-toggler").click(function () {
            var node = $(".icon", this).attr("data-node");
            var has_class = $(".icon", this).hasClass("icon-tt-plus-square");
            <% if(TeamTimeClass.isTeamTimeOwner){%>
            save_dropdown(node, has_class, '<%=ResolveUrl("~/")%>');
            <%}%>
        });

        $(document).on('click', '.rs', Foundation.utils.debounce(function (e) {
            $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();
        }, 200, true));

        //**-- click on single item --**//
        $(document).on('click', '.tt-rs-list', Foundation.utils.debounce(function (e) {
            var $this = $(this);
            $('.bar').removeClass('green');
            $this.find('.bar').addClass('green');
            $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();
        }, 100, true));

        $(document).on('click', '.editThisRating', Foundation.utils.debounce(function (e) {
            var id = $(this).attr('id');
            var er = $('.er-' + id);
            var ex = $('.ex-' + id);

            $(this).find('span').toggleClass('icon-tt-pencil icon-tt-times-circle');
            ex.toggle().click();
            $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();
            er.toggle();


        }, 100, true));

        $(document).on('click', '.er-inpt', Foundation.utils.debounce(function (e) {
            var id = $(this).attr('id');
            var ex = $('.ex-' + id);
            $('.tt-j-clear, .tt-action-btn-clr-j').fadeIn();
            ex.click();
        }, 100, true));

    });

    //start ratings dropdown
    $(document).ready(function () {
         $(document).on('click', '.hidden-toggle-btn', Foundation.utils.debounce(function (e) {
            //get the current index
            $('.htd').addClass('hidden-toggle-btn');
            var closeIcon = $('.close-drop-down');
            closeIcon.hide();
            
            var arrowIcon = $('.down-arrow');
            arrowIcon.show();
          
             
            var $this = $(this);
            $this.removeClass('hidden-toggle-btn');
            
            var index = $(this).attr("data-index");
            
            
            $(".hidden-item-wrap").hide();

            var closeIcon = $('.cdd_'+index);
            closeIcon.show();
            
            var arrowIcon = $('.arrow_'+index);
            arrowIcon.hide();
            
            var theHiddenSelect = $("#ratings-dropdown" + index + ' .hidden-item-wrap');
            theHiddenSelect.slideDown();

        }, 200, true));


         $(document).on('click', '.hidden-item-btn', Foundation.utils.debounce(function (e) {
                var index = $(this).attr("data-index");
            
                var $dropDownBtn = $('.htd_'+index);
                $dropDownBtn.addClass('hidden-toggle-btn');
            
                var theHiddenSelect = $("#ratings-dropdown" + index + ' .hidden-item-wrap');
                theHiddenSelect.slideUp();

                var closeIcon = $('.cdd_'+index);
                closeIcon.hide();
            
                var arrowIcon = $('.arrow_'+index);
                arrowIcon.show();
            
                var thisValue = $(this).text();
                var select_index = $(this).attr("data-index");

           

                $("#ratings-dropdown" + index + ' .hidden-item-btn').removeClass('selected');

                $(".hidden-item-btn a").removeClass('selected');

                if ($("#selected-ratings-" + index + "-" + select_index).hasClass('selected')) {
                    $("#selected-ratings-" + index + "-" + select_index).removeClass('selected');
                } else {
                    $("#selected-ratings-" + index + "-" + select_index).addClass('selected');
                }

        }, 200, true));
        
        //close dropdown
        $(document).on('click', '.close-drop-down', Foundation.utils.debounce(function (e) {
            var id = $(this).attr("id");
            var theHiddenSelect = $("#ratings-dropdown" + id + ' .hidden-item-wrap');
            theHiddenSelect.slideUp();
            
            var $dropDownBtn = $('.htd_'+id);
            $dropDownBtn.addClass('hidden-toggle-btn');
            
            var $this = $(this);
            $this.addClass('hidden-toggle-btn');
            
            var closeIcon = $('.cdd_'+id);
            closeIcon.hide();
            
            var arrowIcon = $('.arrow_'+id);
            arrowIcon.show();

        }, 200, true));

        setTimeout(function () {
            $(".arrow_0").addClass("back-pulse");
            setTimeout(function () {
                $(".arrow_0").removeClass("back-pulse");
            },
            3000);
        },
       1500);


    });

    //$(document).on('touchstart', function (e) {
    //    var container = $(".hidden-item-wrap");

    //    if (!container.is(e.target) // if the target of the click isn't the container...
    //        && container.has(e.target).length === 0) // ... nor a descendant of the container
    //    {
    //        container.hide();
    //    }
    //    else {
    //        e.StopPropagation();
    //    }
    //});
    //end ratings dropdown
</script>







