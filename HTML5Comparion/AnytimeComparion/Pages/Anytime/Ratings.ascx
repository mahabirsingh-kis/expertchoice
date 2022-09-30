<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Ratings.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.Ratings" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %> 
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/ScaleDescription.ascx" TagPrefix="includes" TagName="ScaleDescription" %>

<includes:QuestionHeader runat="server" id="QuestionHeader" />
<div class="large-12 columns tt-auto-resize-content">     
    <div class="columns tt-question-choices">
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
    <div class="large-12 columns disable-when-pause tt-clear-padding-640" ng-init="get_ratings_data() " >
    <div class="tt-judgements-item large-12 columns tt-rating-scale-wrap single-rating-scale">
        <div class="row large-uncollapse small-collapse tt-j-content tt-rating-scale-wrap ds-div-trigger">
            <div class="large-6 medium-12 columns tt-j-others-result tt-clear-padding-left tt-rating-scale-content">
                  <div class="row large-uncollapse small-collapse">
                          <ul class="tt-drag-slider-wrap">                              
                              <li>
                                  <div class="large-12 columns active user-div tt-drag-slider-item">
                                       <div ng-if="output.show_comments" class="small-1  columns text-center">
                                           <%--desktop comment starts--%>
                                           <span ng-if="!$parent.isMobile()" class="mobile-comment-btn">
                                               <a href="#" data-options="align:top" data-dropdown="comment-single" aria-controls="comment-single" aria-expanded="false" class="smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" >
                                                   <span class="icon-tt-comments  icon-30"></span>
                                               </a>
                                            </span>
                                           <%--desktop comment ends--%>

                                           <%--mobile comment starts--%>
                                           <span ng-if="$parent.isMobile()" class="mobile-comment-btn">
                                               <a href="#" data-reveal-id="tt-c-modal" class="smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" >
                                                   <span class="icon-tt-comments  icon-30"></span>
                                               </a>
                                           </span>
                                           <%--mobile comment ends--%>
                                        </div>
                                      <div class="small-7  columns text-right">
                                           <div id="ratings-dropdown" class="tt-hidden-select-wrap">
                                                <div data-index="" class="columns progress hidden-progress-bar hidden-toggle-btn htd">
                                                    <span class="meter" ng-style="{'width' : ratings_data[0] * 100 + '%'}"></span> 
                                                    <span class="text-detail" ng-model="ratings_data"> {{ratings_data[1]}} </span>
                                                    
                                                    <span id="" class="icon-tt-chevron-up close-drop-down cdd hide point"></span>
                                                    <span id="intensity-trigger" class="icon-tt-chevron-down down-arrow arrow"></span>
                                                </div>
                                                <ul  class="hidden-item-wrap">
                                                     <li
                                                        ng-if="output.showPriorityAndDirect || intensity[1] != 'Direct Value'" 
                                                        ng-click="save_ratings(intensity[2])"
                                                        ng-repeat="intensity in output.intensities" 
                                                        class="hidden-item hidden-item-btn"
                                                        ng-class="test-{{intensity[0]}}">
                                                        <a ng-class="{ 'selected': ratings_data[1] == intensity[1]}" id="selected-ratings">
                                                            {{intensity[1]}} 
                                                            <span class="right" ng-if="output.showPriorityAndDirect && intensity[0] >= 0 && intensity[1] != 'Direct Value'">
                                                                &nbsp;{{(intensity[0] * 100) | number:output.precision}}%
                                                            </span>
                                                        </a>
                                                     </li>
                                                </ul>
                                            </div>                                     
                                      </div>
                                    <div class="small-3  columns tg-ratings"> 
                                        <input style="font-size:0.8em;" ng-if="output.showPriorityAndDirect" id="direct_value_input" ng-change="get_direct_value(output.non_pw_value)" ng-model="output.non_pw_value" value="{{output.non_pw_value == -1 ||  output.non_pw_value == -2? '' : output.non_pw_value}}" step="0.1" type="number" min="0" max="1" class="ratings-tooltip" title="Enter number from 0 to 1">
                                    </div>
                                     <div class="small-1 columns large-text-right small-text-center reset-result-btn-wrap">
                                          <a ng-if="output.showPriorityAndDirect" title="">
                                              <span ng-click="save_ratings('-1')"  ng-if="output.non_pw_value != -1"  class="icon-tt-close reset-btn"></span>
                                          </a>
                                     </div>
                                  </div>
                              </li>          
                          </ul>
                   </div>
            </div>
            <div class="large-6 hide-for-medium-down columns">
                    <ul class="tt-rating-scale large-12 columns">
                        <div class="large-12 columns tt-rating-header">
                            <div class="small-6 columns text-left">
                                Intensity Name
                            </div>
                            <div ng-if="output.showPriorityAndDirect" class="small-6 columns large-text-center small-text-right">
                                Priority
                            </div>
                        </div>
                        <li ng-if="output.showPriorityAndDirect || intensity[1] != 'Direct Value'" class="row collapse tt-rs-list li-{{intensity[2]}}" ng-repeat="intensity in output.intensities track by $index">
                            <label>
                                <div class="small-6 columns">
                                    <div class="small-2 columns radio-intensity">
                                        <input ng-click="save_ratings(intensity[2])" ng-if="$index != output.intensities .length - 1 " type="radio" id="{{intensity[2]}}" ng-checked="intensity[0] == output.non_pw_value" ng-click=""   class="radio-judgement rs" name="radioTest" />
                                        <input ng-if="$index == output.intensities.length - 1 " type="radio" id="{{intensity[2]}}" ng-checked="output.is_direct" class="radio-judgement rs" name="radioTest" />
                                    </div>
                                    <div class="small-10 columns">
                                        <span class="existing_priority-name ex-{{intensity[2]}}">{{intensity[1]}}</span>
                                        <span class="ex-des-{{intensity[2]}}" style="color: green;" title="{{intensity[4]}}">{{getIntensityDescription(intensity[1], intensity[4])}}</span>
                                        <span ng-if="$index != output.intensities.length - 1 && is_AT_owner == 1" class="">
                                            <a href="#" data-options="align:top" data-dropdown="ratings-description" aria-controls="ratings-description" aria-expanded="false" ng-click="changeEditIntensityIndex($index)">
                                                <span class="icon-tt-pencil"></span>
                                            </a>
                                        </span>
                                    </div>
                                </div>
                                <div ng-if="output.showPriorityAndDirect" class="small-6 columns text-right">
                                    <div class="small-10 columns" ng-if="$index != output.intensities.length - 1">
                                        <div class="rs-result-bar">
                                            <span class="bar columns bar-{{intensity[2]}}  {{intensity[0] == output.non_pw_value ? 'green' : ''}}" ng-style="{'width': '{{ (intensity[0] < 0 ? 0 : (intensity[0] * 100)) | number:output.precision}}%'}"></span>
                                        </div>
                                    </div>
                                    <div class="small-2 columns text-right" ng-if="$index != output.intensities.length - 1">
                                         <span class="bar-text" ng-if="$index !=  output.intensities.length - 2">
                                             {{(intensity[0] * 100) | number:output.precision}}%
                                         </span>
                                         <span class="bar-text"  ng-if="$index ==  output.intensities.length - 2" ></span>
                                     </div>
                                    <div class="small-10 columns" ng-if="$index == output.intensities.length - 1">
                                         <input id="InputSingleRatingDirect" style="height: 30px;" ng-change="get_direct_value(singleRatingTempDirectValue)" ng-model="singleRatingTempDirectValue" value="{{output.is_direct && output.non_pw_value != -2 ? output.non_pw_value : ''}}" step="0.1" type="number" min="0" max="1" name="name" class="ratings-tooltip" title="Enter number from 0 to 1" />
                                    </div>
                                    <div class="small-2 columns text-right" ng-if="$index == output.intensities.length - 1">
                                        <a id="" ng-if="output.non_pw_value != -1" ng-click="save_ratings('-1')" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise">
                                            <span class="icon-tt-close icon"></span>
                                        </a>
                                    </div>
                                </div>
                            </label>
                        </li>
                    </ul>
                <div>
                    <includes:ScaleDescription runat="server" ID="ScaleDescription" />
                </div>
              </div>
        </div>
    </div>
</div>
</div>

<script>
    $(document).ready(function () {
        

        $(document).on("click", ".hidden-toggle-btn", Foundation.utils.debounce(function (e) {
            //get the current index
            e.stopPropagation();
            
            var $this = $(this);
            $this.removeClass("hidden-toggle-btn");
            
            var index = $(this).attr("data-index");
            
            
            $(".hidden-item-wrap").hide();

            var closeIcon = $('.cdd');
            closeIcon.show();
            
            var arrowIcon = $(".down-arrow");
            arrowIcon.hide();
            
            var theHiddenSelect = $("#ratings-dropdown .hidden-item-wrap");
            theHiddenSelect.slideDown();
            return false;
        }, 200, true));


        $(document).on("click", ".hidden-item-btn", Foundation.utils.debounce(function (e) {
            
//            var index = $(this).attr("data-index");
            e.preventDefault();
            var $dropDownBtn = $(".htd");
            $dropDownBtn.addClass("hidden-toggle-btn");
            
            
            var theHiddenSelect = $("#ratings-dropdown .hidden-item-wrap");
            //theHiddenSelect.slideUp();

            //var closeIcon = $('.cdd');
            //closeIcon.hide();
            
            //var arrowIcon = $('.down-arrow');
            //arrowIcon.show();
            
            var thisValue = $(this).text();
//            var select_index = $(this).attr("data-index");
            //$("#ratings-dropdown .hidden-item-btn").removeClass('selected');

            //$(".hidden-item-btn a").removeClass('selected');

            //if ($("#selected-ratings").hasClass('selected')) {
            //    $("#selected-ratings").removeClass('selected');
            //} else {
            //    $("#selected-ratings").addClass('selected');
            //}
            return false;
        }, 200, true));
        
        //close dropdown
        $(document).on("click", ".close-drop-down", Foundation.utils.debounce(function (e) {
            e.stopPropagation();
        
//            var id = $(this).attr("id");
            //var theHiddenSelect = $("#ratings-dropdown .hidden-item-wrap");
            //theHiddenSelect.slideUp();
            
            //var $dropDownBtn = $('.down-arrow');
            //$dropDownBtn.addClass('hidden-toggle-btn');

            //var closeIcon = $('.cdd');
            //closeIcon.hide();
            
            //var arrowIcon = $('.down-arrow');
            //arrowIcon.show();
            return false;
        }, 200, true));


        setTimeout(function () {
            $(".tt-hidden-select-wrap .arrow").addClass("back-pulse");
            setTimeout(function () {
                $(".tt-hidden-select-wrap .arrow").removeClass("back-pulse");
            },
            3000);

            $("#intensity-trigger").click();
        },
        1500);

        //**-- click on single item --**//
        $(document).on("click", ".tt-rs-list .radio-intensity", Foundation.utils.debounce(function (e) {
            //var $this = $(this);
            var $this = $(this).closest("li");
            $(".bar").removeClass("green");
            $this.find(".bar").addClass("green");
            $(".tt-j-clear, .tt-action-btn-clr-j").fadeIn();
        }, 100, true));

        $(document).on("click", ".editThisRating", Foundation.utils.debounce(function (e) {
            var id = $(this).attr('id');
            var er = $(".er-" + id);
            var ex = $(".ex-des-" + id);

            $(this).find("span").toggleClass("icon-tt-pencil icon-tt-times-circle");
            ex.toggle().click();
            $(".tt-j-clear, .tt-action-btn-clr-j").fadeIn();
            er.toggle();
        }, 100, true));

        $(document).on("click", ".er-inpt", Foundation.utils.debounce(function (e) {
            var id = $(this).attr("id");
            var ex = $(".ex-des-" + id);
            $(".tt-j-clear, .tt-action-btn-clr-j").fadeIn();
            ex.click();
        }, 100, true));
    });

</script>