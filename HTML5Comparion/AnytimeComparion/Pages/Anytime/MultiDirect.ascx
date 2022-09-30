<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MultiDirect.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.MultiDirect" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
<%@ Register Src="~/Pages/includes/FramedInfodDocs.ascx" TagPrefix="includes" TagName="FramedInfodDocs" %> 
<link href="/Content/stylesheets/directcomparison.css" rel="stylesheet" />
<includes:QuestionHeader runat="server" id="QuestionHeader" />  
<div class="columns tt-auto-resize-content">
     <%-- framed docs --%>
    <includes:FramedInfodDocs runat="server" id="FramedInfodDocs" ng-cloak/>
    <%-- // framed docs --%>

    <div class="large-12 columns disable-when-pause">
        <div class="tt-judgements-item large-12 columns">
            <div class="columns tt-j-content">
                <div class="row collapse tt-j-result multi-loop-wrap ">

                    <div class="large-8  columns large-centered text-center" style="margin-bottom: 10px !important;">
                        <ul class="tt-drag-slider-wrap direct-user">
                            <li
                                class="column" 
                                ng-repeat="data in output.multi_non_pw_data"
                                ng-click="set_multi_index($index)"
                                >
                                <div data-equalizer
                                    id="multi-row-{{$index}}"  
                                    ng-class="{'selected': $index == 0, 'fade-fifty' : active_multi_index!=$index }"
                                    class="large-12 columns tt-drag-slider-item active user-div multi-direct-row multi-rows multi-row-{{$index}}">
                                    <div data-equalizer-watch class="medium-6 columns text-center multi-direct-content" >
                                        <div class="tt-center-height-wrap">
                                        <%-- desktop comment --%>
                                        <a  
                                            ng-if="output.show_comments"
                                            data-options="align:top"
                                            data-dropdown="comment-{{$index}}" 
                                            aria-controls="comment-{{$index}}" 
                                            aria-expanded="false"
                                            class="dropdown-btn hide-for-medium-down"
                                            >
                                                <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                        </a>
                                        <%-- //desktop comment --%>

                                        <%-- mobile comment --%>
                                        <a  
                                            ng-if="output.show_comments"
                                            data-reveal-id="mobile-multi-tt-c-modal"
                                            class="show-for-medium-down"
                                            >
                                                <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                        </a>
                                        <%-- //mobile comment --%>
                                        <a  style="position: absolute; top: 0px; left: 20px;"
                                            ng-if="(output.is_infodoc_tooltip || isMobile()) && output.showinfodocnode"
                                            data-dropdown="gdrop{{$index}}_1" data-options="is_hover:true; hover_timeout:1000" 
                                            aria-controls="gdrop{{$index}}_1" aria-expanded="false" data-index="{{$index}}" 
                                            data-node-type="2" data-location="1" data-node="left-node" data-node-description="{{data.Title}}" 
                                            data-node-id="{{$index + 1}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                            class="left-node-{{$index+1}}-{{$index}}-tooltip tooltip-infodoc-btn">
                                            <span ng-if="(is_AT_owner || (!is_AT_owner && getHtml(data.Infodoc) != '')) && output.showinfodocnode" ng-class="getHtml(data.Infodoc) == '' ? 'disabled' : '' "  class="icon icon-tt-info-circle"></span>
                                        </a> 
                                        <span class="tt-center-content-wrap">{{data.Title}}</span>
                                        <span class="tt-center-content-wrap" style="padding-right: 10px;">
                                            <a ng-if="(output.is_infodoc_tooltip  || isMobile()) && output.showinfodocnode"
                                               data-dropdown="gdrop{{$index}}_2" data-options="is_hover:true; hover_timeout:1000" aria-expanded="false"
                                               aria-controls="gdrop{{$index}}_2" data-node-type="3" data-index="{{$index}}" data-location="1"
                                               data-node="wrt-left-node" data-node-title="{{data.Title}}" data-node-id="{{$index + 1}}"
                                               data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                               class="wrt-left-node-{{$index+1}}-{{$index}}-tooltip tooltip-infodoc-btn">
                                                <span ng-if="is_AT_owner || (!is_AT_owner && getHtml(data.InfodocWRT) != '')"
                                                    ng-class="getHtml(data.InfodocWRT) == '' ? 'disabled' : ''"  class="icon icon-tt-w-font multi-direct"></span>
                                                <span ng-if="(!is_AT_owner && getHtml(data.InfodocWRT) == '')">&nbsp;&nbsp;</span>
                                            </a>  
                                        </span>
                                        </div>
                                    </div>
                                    <div data-equalizer-watch class="medium-2 small-3 columns " >
                                        <div class="tt-center-height-wrap">
                                            <div class="tt-center-content-wrap">
                                                <input
                                                    min="0"
                                                    max="1"
                                                    id="at_direct_input{{$index}}" 
                                                    ng-change="update_multi_direct_slider($index, at_multi_direct_input[$index], null, true)"
                                                    ng-focus="move_to_next_row($index - 1)"
                                                    ng-model="at_multi_direct_input[$index]"  
                                                    ng-value="at_multi_direct_slider[$index] >= 0 ? at_multi_direct_slider[$index] : ''" 
                                                    step="0.01" 
                                                    type="number"
                                                    style="width:90%" 
                                                    class="dsInputClr direct-tooltip"
                                                    title="Enter number from 0 to 1"
                                                    >
                                            </div>
                                        </div>
                                    </div>
                                    <div class="medium-3 small-8 columns tt-dsBar dsBarClr slider multiDirectSlider" 
                                        id="at_direct_slider{{$index}}" 
                                        ng-model="at_multi_direct_slider[$index]" 
                                        anytime-Multidirectslider 
                                        permission="0" 
                                        value="{{at_multi_direct_slider[$index]}}"
                                        index="{{$index}}" tabindex="-1">
                                    </div>                                    
                                    <div  class="medium-1 small-1 columns text-right reset-result-btn-wrap multi-direct">
                                        <a ng-if="at_multi_direct_slider[$index] >= 0" ng-click="update_multi_direct_slider($index, '')" title="">
                                            <span class="icon-tt-close reset-btn multi-direct"></span>
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
