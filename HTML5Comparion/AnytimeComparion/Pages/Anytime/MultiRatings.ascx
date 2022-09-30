<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MultiRatings.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.MultiRatings" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/FramedInfodDocs.ascx" TagPrefix="includes" TagName="FramedInfodDocs" %>
<%@ Register Src="~/Pages/includes/ScaleDescription.ascx" TagPrefix="includes" TagName="ScaleDescription" %>


<includes:QuestionHeader runat="server" id="QuestionHeader" />
<div class="columns tt-auto-resize-content">
    <%-- framed docs --%>
    <includes:FramedInfodDocs runat="server" id="FramedInfodDocs" ng-cloak/>
    <%-- // framed docs --%>
    <div class="large-12 columns disable-when-pause tt-clear-padding-640 anytime-ratings">
        <div class="tt-judgements-item large-12 columns tt-rating-scale-wrap">
            <div class="row tt-j-content tt-rating-scale-wrap ds-div-trigger">
                <div class="large-6 medium-12 columns tt-j-others-result tt-clear-padding-left tt-rating-scale-content ">
                    <div class="row collapse multi-loop-wrap">
                        <div ng-if="isMobile()" class="space-before-judgment-for-mobile hide" style="height: 20px;">&nbsp;</div>
                        <div class="large-12 columns" style="margin-bottom: 10px !important;">
                            <ul class="tt-drag-slider-wrap multi-ratings-list">
                                <li 
                                    ng-click="select_multi_ratings_row($index)" 
                                    ng-init="multi_ratings_row[$index] = get_multi_ratings_data(data.RatingID, data.DirectData, $index); row_index = $index" 
                                    ng-repeat="data in output.multi_non_pw_data track by $index"
                                    >
                                    <div id="multi-row-{{$index}}" data-index="{{$index}}" 
                                        ng-class="{ 'selected' : active_multi_index==$index, 'fade-fifty' : active_multi_index!=$index }" 
                                        class="multi-ratings-row large-12 columns active user-div tt-drag-slider-item">
                                        <div class="small-12 columns tg-ratings medium-text-left small-text-center" ng-class="output.showPriorityAndDirect ? 'large-5 medium-5' : 'large-7 medium-7'">
                                            <%-- desktop comment --%>
                                                <div style="display: inline;">
                                                    <a ng-if="output.show_comments" data-options="align:top" data-dropdown="comment-{{$index}}" aria-controls="comment-{{$index}}" aria-expanded="false" class="dropdown-btn hide-for-medium-down">
                                                        <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                                    </a>
                                                </div>
                                                <%-- //desktop comment --%>

                                                    <%-- mobile comment --%>
                                                        <div style="display: inline;">
                                                            <a ng-if="output.show_comments" data-reveal-id="mobile-multi-tt-c-modal" class="show-for-medium-down">
                                                                <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                                            </a>
                                                        </div>
                                                        <%--//  mobile comment --%>
                                                            <a ng-if="(output.is_infodoc_tooltip || isMobile()) && output.showinfodocnode" 
                                                               data-dropdown="gdrop{{$index}}_1" data-options="is_hover:true; hover_timeout:1000" 
                                                               aria-controls="gdrop{{$index}}_1" aria-expanded="false" data-index="{{$index}}" 
                                                               data-node-type="2" data-location="1" data-node="left-node" data-node-description="{{data.Title}}" 
                                                               data-node-id="{{$index + 1}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                                               class="left-node-{{$index+1}}-{{$index}}-tooltip tooltip-infodoc-btn">
                                                                <span ng-if="is_AT_owner || (!is_AT_owner && getHtml(data.Infodoc) != '')" ng-class="getHtml(data.Infodoc) == '' ? 'disabled' : '' " class="icon icon-tt-info-circle"></span>
                                                                <span ng-if="(!is_AT_owner && getHtml(data.Infodoc) == '')">&nbsp;&nbsp;</span>
                                                            </a>
                                                            <span ng-bind-html="data.Title"></span>
                                                            <a ng-if="(output.is_infodoc_tooltip || isMobile()) && output.showinfodocnode" 
                                                               data-dropdown="gdrop{{$index}}_2" data-options="is_hover:true; hover_timeout:1000" 
                                                               aria-controls="gdrop{{$index}}_2" aria-expanded="false" data-node-type="3" 
                                                               data-index="{{$index}}" data-location="1" data-node="wrt-left-node" data-node-title="{{data.Title}}" 
                                                               data-node-id="{{$index + 1}}" data-node-description="{{output.parent_node}}"
                                                               data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                                               class="wrt-left-node-{{$index+1}}-{{$index}}-tooltip tooltip-infodoc-btn">
                                                                <span ng-if="is_AT_owner || (!is_AT_owner && getHtml(data.InfodocWRT) != '')" 
                                                                      ng-class="getHtml(data.InfodocWRT) == '' ? 'disabled' : ''" class="icon icon-tt-w-font"></span>
                                                                <span ng-if="(!is_AT_owner && getHtml(data.Infodoc) == '')">&nbsp;&nbsp;</span>
                                                            </a>
                                        </div>
                                        <div class="large-4 medium-4 small-7 columns text-right" ng-class="output.showPriorityAndDirect ? 'small-7' : 'small-11'">
                                            <div id="ratings-dropdown{{$index}}" class="tt-hidden-select-wrap">
                                                <div data-index="{{$index}}" id="hidden-toggle-btn-{{$index}}" class="columns progress hidden-progress-bar hidden-toggle-btn htd_{{$index}} htd">
                                                    <span class="meter" ng-style="{'width' :  (multi_ratings_row[$index][0] ? multi_ratings_row[$index][0] * 100  : 0 ) + '%'}"></span>
                                                    <span class="text-detail"> {{multi_ratings_row[$index][1]}}</span>
                                                    <span id="{{$index}}" class="icon-tt-chevron-up close-drop-down cdd_{{$index}} hide point"></span>
                                                    <span class="icon-tt-chevron-down down-arrow arrow_{{$index}}"></span>
                                                </div>

                                                <ul data-index="{{$index}}" class="hidden-item-wrap">

                                                    <li data-index="{{row_index}}" ng-if="output.showPriorityAndDirect || intensity.Name != 'Direct Value'"
                                                        ng-click="add_multi_ratings_values(intensity.ID, row_index, $event)" ng-repeat="intensity in output.multi_non_pw_data[active_multi_index].Ratings" class="hidden-item hidden-item-btn">
                                                        <a ng-class="{ 'selected': multi_ratings_row[row_index][1] == intensity.Name}" id="selected-ratings">{{intensity.Name}}
                                                            <span class="right" ng-if="output.showPriorityAndDirect && intensity.Value >= 0 && intensity.Name != 'Direct Value' && intensity.Name != 'Not Rated'">&nbsp;{{(intensity.Value * 100) | intensityDecimal}}%</span>
                                                        </a>
                                                    </li>
                                                </ul>
                                            </div>
                                        </div>
                                        <div ng-if="output.showPriorityAndDirect" class="large-2 medium-2 small-4 columns tg-ratings" style="padding-left: 5px;">
                                            <input class="ratings-tooltip" title="Enter number from 0 to 1"
                                                ng-focus="select_multi_ratings_row($index)" style="height: 30px; font-size: 0.8em;" intensity-input
                                                ng-change="add_multi_ratings_values(-2, row_index, $event, multi_direct_value[$index])" ng-model="multi_direct_value[$index]" step="0.1" type="number" min="0" max="1">
                                        </div>
                                        <div class="large-1 small-1 columns tg-ratings">
                                            <a title="" ng-if="(output.multi_non_pw_data[row_index].RatingID == -1 && output.multi_non_pw_data[row_index].DirectData >= 0) || output.multi_non_pw_data[row_index].RatingID >= 0">
                                                <span ng-click="add_multi_ratings_values('-1', row_index, $event)" class="icon-tt-close reset-btn"></span>
                                            </a>
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
                <div class="large-5 columns hide-for-medium-down tt-sticky-element-ratings">
                    <ul class="tt-rating-scale large-12 columns ">
                        <div class="text-center">
                           <h4>{{selected_multi_data[active_multi_index].Title}}</h4> 
                        </div>
                        <div class="large-12 columns tt-rating-header">
                            <div class="small-6 columns text-left">
                                Intensity Name
                            </div>
                            <div ng-if="output.showPriorityAndDirect" class="small-6 columns large-text-center small-text-right">
                                Priority
                            </div>
                        </div>
                        <li ng-if="output.showPriorityAndDirect || intensity.Name != 'Direct Value'" class="row collapse tt-rs-list li-{{intensity.ID}}" ng-repeat="intensity in output.multi_non_pw_data[active_multi_index].Ratings track by $index">
   
                            <label>
                                <div class="columns multi-ratings-list" ng-class="output.showPriorityAndDirect ? 'small-6' : 'small-12'">
                                    <div class="columns" ng-class="output.showPriorityAndDirect ? 'small-2' : 'small-1'">

                                        <input ng-click="add_multi_ratings_values(intensity.ID, active_multi_index, $event)" ng-if="$index != output.multi_non_pw_data[active_multi_index].Ratings.length - 1 " type="radio" id="{{intensity.ID}}" ng-checked="selected_multi_data[active_multi_index].RatingID == intensity.ID" class="radio-judgement rs hidden-item-btn" name="radioTest" data-index="{{active_multi_index}}" />

                                        <input ng-if="$index == output.multi_non_pw_data[active_multi_index].Ratings.length - 1 " type="radio" id="{{intensity.ID}}" ng-checked="selected_multi_data[active_multi_index].RatingID == -1 && selected_multi_data[active_multi_index].DirectData != -1" class="radio-judgement rs" name="radioTest"/>
                                    </div>

                                    <div class="columns" ng-class="output.showPriorityAndDirect ? 'small-10' : 'small-11'">
                                        <span class="existing_priority-name ex-{{intensity.ID}}">{{intensity.Name}}</span>
                                        <span class="ex-des-{{intensity[2]}}" style="color: green;" title="{{intensity.Comment}}">{{getIntensityDescription(intensity.Name, intensity.Comment)}}</span>
                                        <span ng-if="$index != output.multi_non_pw_data[active_multi_index].Ratings.length - 1 && is_AT_owner == 1" class="">
                                            <a href="#" data-options="align:top" data-dropdown="ratings-description" aria-controls="ratings-description" aria-expanded="false" ng-click="changeEditIntensityIndex($index)">
                                                <span class="icon-tt-pencil"></span>
                                            </a>
                                        </span>
                                    </div>
                                </div>
                                <div ng-if="output.showPriorityAndDirect" class="small-6 columns text-right">
                                    <div class="small-10 columns" ng-if="$index != output.multi_non_pw_data[active_multi_index].Ratings.length- 1">
                                        <div class="rs-result-bar">
                                            <span class="bar columns bar-{{intensity.ID}}  {{selected_multi_data[active_multi_index].RatingID == intensity.ID ? 'green' : ''}}" ng-style="{'width': '{{ (intensity.Value < 0 ? 0 : (intensity.Value * 100)) | number:output.precision}}%'}"></span>
                                        </div>
                                    </div>
                                    <div class="small-2 columns text-right" ng-if="$index != output.multi_non_pw_data[active_multi_index].Ratings.length- 1">
                                        <span class="bar-text" ng-if="$index !=  output.multi_non_pw_data[active_multi_index].Ratings.length - 2"> {{(intensity.Value * 100) | intensityDecimal}}%</span>
                                        <span class="bar-text" ng-if="$index == output.multi_non_pw_data[active_multi_index].Ratings.length - 2"></span>
                                    </div>
                                    <div class="small-10 columns" ng-if="$index == output.multi_non_pw_data[active_multi_index].Ratings.length - 1">
                                        <input id="multiRatingsDirectValue" style="height: 30px;" ng-model="multi_direct_value[active_multi_index]" ng-change="add_multi_ratings_values(-2, active_multi_index, $event, multi_direct_value[active_multi_index])" value="{{selected_multi_data[active_multi_index].RatingID == -1 && selected_multi_data[active_multi_index].DirectData != -1 ? multi_direct_value[active_multi_index] : ''}}" step="0.1" type="number" min="0" max="1" name="name" class="ratings-tooltip" title="Enter number from 0 to 1" />
                                    </div>
                                    <div class="small-2 columns text-right" ng-if="$index == output.multi_non_pw_data[active_multi_index].Ratings.length- 1">
                                        <a id="" ng-if="selected_multi_data[active_multi_index].RatingID != -1 || selected_multi_data[active_multi_index].DirectData != -1" ng-click="add_multi_ratings_values(-1, active_multi_index, $event)" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise">
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
       