<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DirectComparison.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.DirectComparison" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/LeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="LeftNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/WRTLeftNodeInfoDoc.ascx" TagPrefix="includes" TagName="WRTLeftNodeInfoDoc" %>
<link href="/Content/stylesheets/directcomparison.css" rel="stylesheet" />
<includes:QuestionHeader runat="server" id="QuestionHeader" />
<div class="large-12 columns tt-auto-resize-content">
    <div class="row large-uncollapse small-collapse">
        <div class="tt-question-choices">
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
    </div>
    <div class="large-12 columns disable-when-pause">
        <div class="tt-judgements-item large-12 columns">
            <div class="columns tt-j-content">
                <div class="row tt-j-result ">
                    <div class="">
                        <ul class="tt-drag-slider-wrap direct-user">
                            <li class="row collapse">
                                <div class="large-5 large-centered medium-6 medium-centered small-12 columns tt-drag-slider-item active user-div">
                                    <div class="medium-1 small-1 columns columns text-center" ng-if="output.show_comments">
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
                                    <div class="medium-2 small-3 columns ">
                                        <input id="at_direct_input" ng-model="at_direct_input" ng-change="set_direct_input(at_direct_input)" ng-value="at_direct_slider >= 0 ? at_direct_slider : ''" step="0.01" max="1" min="0" type="number" style="width:90%"
                                               class="dsInputClr direct-tooltip"
                                               title="Enter number from 0 to 1">
                                    </div>
                                    <div class="medium-1 small-1 columns ">&nbsp;</div>
                                    <div ng-class='{"tt-dsBar medium-6 small-6 columns dsBarClr slider " : !output.show_comments, "tt-dsBar medium-6 small-5 columns dsBarClr slider ": output.show_comments}' id="at_direct_slider" ng-model="at_direct_slider" anytime-Directslider permission="0"></div>
                                    <div class="medium-1 small-1  columns text-center reset-result-btn-wrap single-direct" ng-click="set_direct_input(-1)">
                                        <a ng-if="!output.IsUndefined" title="">
                                            <span class="icon-tt-close reset-btn"></span>
                                        </a>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>