<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MultiPairwise.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.MultiPairwise" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %> 
<%@ Register Src="~/Pages/includes/FramedInfodDocs.ascx" TagPrefix="includes" TagName="FramedInfodDocs" %> 
<%@ Register Src="~/Pages/includes/ParentNodeInfoDoc.ascx" TagPrefix="includes" TagName="ParentNodeInfoDoc" %>
    <link href="/Content/stylesheets/nouislider.css" rel="stylesheet" />
    <link href="/Content/stylesheets/nouislider.pips.css" rel="stylesheet" />
    <link href="/Content/stylesheets/nouislider.tooltips.css" rel="stylesheet" />
    <link href="/Content/stylesheets/pairwise.css" rel="stylesheet" />
    <style>
        .tt-content-wrap .tt-body .tt-judgements-item .tt-j-result .tt-multi-pairwise-wrap .multi-loop-wrap .multi-rows {
            padding: 5px 10px 0px 10px;
        }
        .top-first-row {
            top: 6px;
        }
    </style>
 
        <div class="large-12 columns questionsWrap" ng-cloak>
            <includes:QuestionHeader runat="server" id="QuestionHeader" ng-cloak/>     
             <div class="columns tt-auto-resize-content">

            <%-- framed docs --%>
            <includes:FramedInfodDocs runat="server" id="FramedInfodDocs" ng-cloak/>
            <%-- // framed docs --%>

                

            <div class="tt-judgements-item large-12 columns" >       
                <div class="columns tt-j-content ">
                    <div class="row collapse tt-j-result ds-div-trigger">
                        <div class="columns">
                             <div class="row collapse">
                                 
                                <div class="original-legend text-center" style="font-size: .750rem;" ng-if="output.pairwise_type == 'ptVerbal' && isMobile()">
                                    <b>EQ</b>ual<b>&nbsp;&nbsp;&nbsp;M</b>oderate
                                    &nbsp;&nbsp;&nbsp;<b>S</b>trong
                                    &nbsp;&nbsp;&nbsp;<b>V</b>ery<b>S</b>trong
                                    &nbsp;&nbsp;&nbsp;<b>EX</b>treme
                                </div>
                                <div class="tt-j-others-result columns">
                                    <div class=" {{ multi_data.length <= 7 ? 'fix-multi-pw' : ''}} tt-multi-pairwise-wrap multi-verbal large-10 large-centered columns multi-graphical">
                                        <div class="multi-loop-wrap hide">
                                            
                                            <!-- LOOP start -->
                                            <div
                                                ng-click="set_multi_index($index)"
                                                ng-init="pair_index = $index;"
                                                ng-class="{
                                                'fade-fifty' : active_multi_index!=$index,
                                                'top-row' :active_multi_index==0,
                                                'selected' : active_multi_index==$index, 
                                                'multi-verbal large-12 columns' : active_multi_index != $index, 
                                                'columns tt-j-content multi-loop' :  active_multi_index == $index, 'tt-verbal-bars-wrap' : output.pairwise_type == 'ptVerbal'}" 
                                                ng-repeat="data in ::output.multi_pw_data track by $index"
                                                id="multi-row-{{::$index}}" 
                                                class="multi-rows multi-row-{{::$index}}"
                                                ng-cloak>
                                                   
                                                  <!-- START added for mobile view only -->
                                                <div ng-init="checkGPtoVerbal(data)" ng-if="isMobile()" class="small-6 columns text-left show-for-medium-down multi-mobile left-question" >
                                                    <div class="small-2 columns text-center">
                                                        <a ng-if="(is_AT_owner || (!is_AT_owner && !is_html_empty(data.InfodocLeft))) && output.showinfodocnode && !output.framed_info_docs"
                                                           data-dropdown="gdrop{{::$index}}_1" aria-controls="gdrop{{::$index}}_1" aria-expanded="false" data-options="align:top;"
                                                           class="left-node-{{::data.NodeID_Left}}-{{::$index}}-tooltip">
                                                            <span ng-class="is_html_empty(data.InfodocLeft) ? 'disabled' : 'not-disabled' " class="icon icon-tt-info-circle"></span>
                                                        </a>
                                                        <a ng-if="( !is_AT_owner && is_html_empty(data.InfodocLeft)) || !output.showinfodocnode">
                                                            <span>&nbsp;</span>
                                                        </a>
                                                    </div>
                                                    <div class="small-8 text-center  columns">
                                                         <span 
                                                            ng-class="{'hide': active_multi_index == $index}"
                                                            class="mobile multi-left-node limited-lft-{{::$index}}">
                                                            <a 
                                                                class="show-for-small-only"
                                                                id="l-{{::$index}}"
                                                                ng-if="data.LeftNode.length > text_limit"
                                                                href="#">
                                                                {{::data.LeftNode.substring(0, text_limit - 2)}}...
                                                            </a>
                                                             <a 
                                                                class="show-for-small-only"
                                                                id="l-{{::$index}}"
                                                                ng-if="data.LeftNode.length <= text_limit"
                                                                href="#">
                                                               {{::data.LeftNode}}
                                                            </a>
                                                            <a 
                                                                class="show-for-medium-up"
                                                                id="l-{{::$index}}" 
                                                                href="#">
                                                                {{::data.LeftNode}}
                                                            </a>
                                                        </span>
                                                        <span 
                                                            ng-class="{'hide': active_multi_index != $index}"
                                                            class="multi-left-node full-lft-{{::$index}}">
                                                            <a 
                                                                id="l-{{::$index}}">
                                                                {{::data.LeftNode}}
                                                            </a>
                                                        </span>

                                                    </div>    
                                                     
                                                   
                                                    <div class="small-2 columns infodoc-icon-right text-center" >
                                                        <a 
                                                            ng-if="( is_AT_owner || ( !is_AT_owner && !is_html_empty(data.InfodocLeftWRT))) && output.showinfodocnode"
                                                            data-dropdown="gdrop{{::$index}}_2" 
                                                            aria-controls="gdrop{{::$index}}_2" 
                                                            aria-expanded="false"                                                                                       
                                                            class="wrt-left-node-{{::data.NodeID_Left}}-{{::$index}}-tooltip"    
                                                            >
                                                            <span 
                                                             
                                                                ng-class="is_html_empty(data.InfodocLeftWRT) ? 'disabled' : 'not-disabled' " class="icon icon-tt-w-font"></span>
                                                         </a>  
                                                         <a ng-if="( !is_AT_owner && is_html_empty(data.InfodocLeftWRT)) ">
                                                              <span>&nbsp;</span>
                                                         </a>
                                                    </div>
                                                </div> 
                                                <div ng-if="isMobile()" class="small-6 columns text-right show-for-medium-down multi-mobile right-question" >
                                                    <div class="small-2 columns text-center"> 
                                                        <a ng-if="(is_AT_owner || ( !is_AT_owner && !is_html_empty(data.InfodocRight))) && output.showinfodocnode && !output.framed_info_docs"
                                                            data-dropdown="gdrop{{::$index}}_3" aria-controls="gdrop{{::$index}}_3" aria-expanded="false"
                                                            class="right-node-{{::data.NodeID_Right}}-{{::$index}}-tooltip">
                                                            <span ng-class="is_html_empty(data.InfodocRight) ? 'disabled' : 'not-disabled' "  class="icon icon-tt-info-circle"></span>
                                                        </a>
                                                        <a ng-if="(is_AT_owner || ( !is_AT_owner && is_html_empty(data.InfodocRight))) || !output.showinfodocnode">
                                                               <span>&nbsp;</span>
                                                         </a>
                                                    </div>
                                                    <div class="small-8 text-center columns">
 
                                                         <span 
                                                            ng-class="{'hide': active_multi_index == $index}"
                                                            class="mobile multi-right-node limited-rgt-{{::$index}}">
                                                            <a 
                                                                class="show-for-small-only"
                                                                id="r-{{::$index}}"
                                                                ng-if="data.RightNode.length > text_limit"
                                                                href="#">
                                                                {{::data.RightNode.substring(0, text_limit - 2)}}...
                                                            </a>
                                                             <a 
                                                                class="show-for-small-only"
                                                                id="r-{{::$index}}"
                                                                ng-if="data.RightNode.length <= text_limit"
                                                                href="#">
                                                               {{::data.RightNode}}
                                                            </a>
                                                            <a 
                                                                class="show-for-medium-up"
                                                                id="r-{{::$index}}" 
                                                                href="#">
                                                                {{::data.RightNode}}
                                                            </a>
                                                        </span>


                                                        <span 
                                                            ng-class="{'hide': active_multi_index != $index}"
                                                            class="multi-right-node full-rgt-{{::$index}}" >
                                                            <a  
                                                                id="r-{{::$index}}" 
                                                                href="#">
                                                                {{::data.RightNode}}
                                                            </a>
                                                        </span>

                                                    </div>  
                                                    <div class="small-2 left text-center columns" >
                                                        <a 
                                                             ng-if="(is_AT_owner || (!is_AT_owner && !is_html_empty(data.InfodocRightWRT))) && output.showinfodocnode"
                                                            data-dropdown="gdrop{{::$index}}_4" 
                                                            aria-controls="gdrop{{::$index}}_4" 
                                                            aria-expanded="false"
                                                            class="wrt-right-node-{{::data.NodeID_Right}}-{{::$index}}-tooltip"  
                                                            >
                                                            <span 
                                                               
                                                                ng-class="is_html_empty(data.InfodocRightWRT) ? 'disabled' : 'not-disabled' "  class="icon icon-tt-w-font"></span>
                                                            
                                                        </a>
                                                        <a  ng-if="( !is_AT_owner && is_html_empty(data.InfodocRightWRT))">
                                                            <span>&nbsp;</span>
                                                        </a>
                                                    </div>

                                                </div>
                                                
                                                <div 
                                                    ng-if="active_multi_index==$index && output.pairwise_type == 'ptVerbal' && isMobile()"
                                                    class="small-12 medium-9 medium-centered columns show-for-medium-down text-center multi-pw-verbal-labels-wrap anytime">
                                                                                                        
                                                    <!-- START only show this labels in first child-->
                                                    <div 
                                                            ng-if="active_multi_index==$index || (active_multi_index==0 && $index==0) && !output.collapse_bars"
                                                            class="columns selected-item text-center selected-item-multi-pw show-for-medium-down" style="" 
                                                            ng-bind-html ="display_selected_bar(data.Value, data.Advantage)">
                                                    </div>
                                                    
                                                    <!-- END only show this labels in first child -->
                                                    <div class="tt-mobile-wrap columns">
                                                       <div class="tt-j-content">
                                                           <div 
                                                               ng-if="active_multi_index == $index && !output.collapse_bars"
                                                               class="tt-equalizer-mobile multi-vb-bars-wrap">
                                                                <ul id="pm-bars" class="vb-bars-small-medium-screen">
                                                            <li  
                                                                data-index="{{::$parent.$index}}" 
                                                                data-bar-index="{{::$index}}"
                                                                ng-click="add_multivalues(bar[0] , 1 , pair_index, $event)"
                                                                id="{{$index+1}}" 
                                                                title="{{::bar[1]}}"
                                                                class="{{::bar[2]}} {{main_gradient_checkbox ? '' : 'no-gradient'}} pm-bars lft lft-eq lft-{{$index+1}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients"  
                                                                data-pos="lft"
                                                                ng-class="data.Advantage == 1 ? data.Value >= bar[0] ? 'active-selected' : '' : ''" 
                                                                ng-repeat="bar in ::bars_left track by $index" 
                                                                ng-style="{ 'background' : (data.Advantage == 1 && bar[0] > data.Value && bar[0] - data.Value < 1 &&  data.Value % 1 > 0 ? 'linear-gradient(270deg, #0058a3 '+ (bar[0] - data.Value) +'%, #cccccc 50%)' : '') }"
                                                                >
                                                                <span ng-if="bar[3] != 'M'" class="bar-label">{{::bar[3]}}</span>
                                                                <span ng-if="bar[3] == 'M'" class="bar-label" style="top:-7px; position:relative">{{::bar[3]}}</span>
                                                            </li>

                                                            <li 
                                                                 data-bar-index="0"
                                                                ng-click="add_multivalues(1, 0 ,pair_index, $event)"
                                                                data-index="{{::$parent.$index}}"
                                                                id="9" 
                                                                title="Equal"
                                                                class="pm-bars zero mid lft-9 lft-eq rgt-eq rgt-9 mid-9 {{main_gradient_checkbox ? '' : 'no-gradient'}} 
                                                {{data.Advantage == 1   ? 'lft-selected' : ''}}
                                                {{data.Advantage == -1 ? 'rgt-selected' : ''}}  
                                                {{data.Advantage == 0 && data.Value == 1 ? 'active-selected' : ''}} tg-gradients" 
                                                                data-pos="mid"
                                                                >

                                                                <span class="bar-label" style="display: block; height: 100%; font-size: 7px; margin-top: -1px;">EQ</span> 
                                                                
                                                            </li>

                                                            <li 
                                                                data-index="{{::$parent.$index}}" 
                                                                data-bar-index="{{::$index}}"
                                                                ng-click="add_multivalues(bar[0], -1 , pair_index, $event)"
                                                                id="{{bars_right.length-$index}}" 
                                                                title="{{::bar[1]}}" 
                                                                class="{{::bar[2]}} pm-bars rgt lvl-even-bar rgt-eq rgt-{{bars_right.length-$index}} {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients" 
                                                                data-pos="rgt" 
                                                                ng-class="data.Advantage == -1 ? data.Value >= bar[0] ? 'active-selected' : '' : ''" 
                                                                ng-repeat="bar in ::bars_right track by $index" 
                                                                ng-style="{ 'background' : (data.Advantage == -1 && bar[0] > data.Value && bar[0] - data.Value < 1 &&  data.Value % 1 > 0 ? ('linear-gradient(90deg, #6aa84f '+ (bar[0] - data.Value) +'%, #cccccc 50%)') : '') }"
                                                                >
                                                                <span ng-if="bar[3] != 'M'" class="bar-label">{{::bar[3]}}</span>
                                                                <span ng-if="bar[3] == 'M'" class="bar-label" style="top:-7px; position:relative">{{::bar[3]}}</span>
                                                            </li>       
                                                        </ul>


                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <!-- END added for mobile view only -->
                                                     
                                                <!-- START DESKTOP view only -->
                                                <div ng-if="!isMobile()" class="medium-3 columns text-left hide-for-medium-down pw-verbal-desktop left-alternative" ng-class="active_multi_index==$index || (active_multi_index==0 && $index==0) ? 'top-row-left' : ''">
                                                    <%--<div ng-if="(output.is_infodoc_tooltip && (is_AT_owner || ( !is_AT_owner && !is_html_empty(data.InfodocLeftWRT)))) && output.showinfodocnode"
                                                         class="small-2 left text-center columns tt-center-height-wrap">
                                                        <a data-dropdown="gdrop{{::$index}}_2" 
                                                           aria-controls="gdrop{{::$index}}_2" 
                                                           aria-expanded="false" 
                                                           class="tt-center-content-wrap wrt-left-node-{{data.NodeID_Left}}-{{::$index}}-tooltip" 
                                                           data-options="{{$index >= multi_data.length-1 ? 'align:top' : 'align:top'}}">
                                                            <span ng-class="is_html_empty(data.InfodocLeftWRT) ? 'disabled' : 'not-disabled' " class="icon icon-tt-w-font"></span>
                                                        </a>
                                                    </div>--%>
                                                    <div <%--ng-class="{'small-10' : output.is_infodoc_tooltip && output.showinfodocnode, 'small-12':  !output.is_infodoc_tooltip || !output.showinfodocnode} "--%>  class="small-12 left text-center columns multi-left-node-wrap">
                                                        <div  class="small-2 columns tt-center-height-wrap" ng-if="output.is_infodoc_tooltip">
                                                            <a ng-if="(is_AT_owner || ( !is_AT_owner && !is_html_empty(data.InfodocLeft))) && output.showinfodocnode"
                                                               data-dropdown="gdrop{{::$index}}_1" aria-controls="gdrop{{::$index}}_1" aria-expanded="false"
                                                               data-options="{{$index >= multi_data.length-1 ? 'align:top;' : 'align:top;'}} is_hover:true; hover_timeout:1000" 
                                                               data-index="{{::$index}}" data-node-type="2" data-location="1" data-node="left-node" 
                                                               data-node-description="{{data.LeftNode}}" data-node-id="{{::data.NodeID_Left}}"
                                                               data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                                               class="tt-center-content-wrap left-node-{{::data.NodeID_Left}}-{{::$index}}-tooltip i-icon tooltip-infodoc-btn">
                                                                    <span ng-class="is_html_empty(data.InfodocLeft) ? 'disabled' : 'not-disabled' " class="icon icon-tt-info-circle"></span>
                                                            </a>
                                                            <a ng-if="( !is_AT_owner && is_html_empty(data.InfodocLeft))">   
                                                                <span>&nbsp;</span>
                                                            </a>
                                                         </div>
                                                        <div ng-class="{'small-8' : output.is_infodoc_tooltip && output.showinfodocnode, 'small-12':  !output.is_infodoc_tooltip || !output.showinfodocnode} " class=" columns multi-node-content">
                                                            <div class="tt-center-height-wrap">
                                                                <p 
                                                                    ng-class="{'desktop':(!output.collapse_bars && output.pairwise_type !== 'ptGraphical')}"
                                                                    class="default-desktop tt-center-content-wrap multi-left-node" style="width:100%;">
                                                                    <span ng-if="active_multi_index == $index">
                                                                         {{::data.LeftNode}}
                                                                    </span>
                                                                   <span ng-if="active_multi_index != $index">
                                                                          {{::data.LeftNode.substring(0, 27)}}
                                                                           <span ng-if="data.LeftNode.length > 27">
                                                                              ...
                                                                          </span>
                                                                    </span>
                                                                </p>
                                                            </div>
                                                        </div>
                                                        <div class="small-2 columns tt-center-height-wrap">
                                                            <a ng-if="(output.is_infodoc_tooltip && (is_AT_owner == 1 || ( is_AT_owner != 1 && !is_html_empty(data.InfodocLeftWRT)))) && output.showinfodocnode"
                                                               data-dropdown="gdrop{{::$index}}_2" aria-controls="gdrop{{::$index}}_2" aria-expanded="false" 
                                                               class="tt-center-content-wrap wrt-left-node-{{::data.NodeID_Left}}-{{::$index}}-tooltip i-icon tooltip-infodoc-btn" 
                                                               data-options="{{$index >= multi_data.length-1 ? 'align:top;' : 'align:top;'}} is_hover:true; hover_timeout:1000"
                                                               data-node-type="3" data-index="{{::$index}}" data-location="1" data-node="wrt-left-node" 
                                                               data-node-title="{{data.LeftNode}}" data-node-id="{{::data.NodeID_Left}}"
                                                               data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}">
                                                                <span ng-class="is_html_empty(data.InfodocLeftWRT) ? 'disabled' : 'not-disabled' " class="icon icon-tt-w-font"></span>
                                                            </a>
                                                            <a ng-if="( !is_AT_owner && is_html_empty(data.InfodocLeft))">
                                                                <span>&nbsp;</span>
                                                            </a>
                                                        </div>
                                                    </div>    
                                                    
                                                    <%--<div <%--ng-class="{'small-1 text-center' : output.is_infodoc_tooltip && output.showinfodocnode, 'small-1 text-center':  !output.is_infodoc_tooltip || !output.showinfodocnode} " 
                                                        class="small-1 columns tt-center-height-wrap">
                                                        <a  
                                                            ng-if="output.show_comments"
                                                            data-options="align:top"
                                                            data-dropdown="comment-{{::$index}}" 
                                                            aria-controls="comment-{{::$index}}" 
                                                            aria-expanded="false"
                                                            class="dropdown-btn tt-center-content-wrap"
                                                             >
                                                                <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>

                                                         </a>
                                                    </div>--%>
                                                   
                                                    <%--<div ng-if="(output.is_infodoc_tooltip && (is_AT_owner || ( !is_AT_owner && !is_html_empty(data.InfodocLeftWRT)))) && output.showinfodocnode"
                                                        class="small-2 left text-center columns tt-center-height-wrap"> 
                                                        <a data-dropdown="gdrop{{::$index}}_2" aria-controls="gdrop{{::$index}}_2" aria-expanded="false" 
                                                           class="tt-center-content-wrap wrt-left-node-{{::data.NodeID_Left}}-{{::$index}}-tooltip tooltip-infodoc-btn" 
                                                           data-options="{{$index >= multi_data.length-1 ? 'align:top;' : 'align:top;'}} is_hover:true; hover_timeout:1000"
                                                           data-node-type="3" data-index="{{::$index}}" data-location="1"data-node="wrt-left-node" 
                                                           data-node-title="{{data.LeftNode}}" data-node-id="{{::data.NodeID_Left}}"
                                                           data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}">
                                                            <span ng-class="is_html_empty(data.InfodocLeftWRT) ? 'disabled' : 'not-disabled' " class="icon icon-tt-w-font"></span>
                                                        </a>
                                                    </div>--%>
                                                </div>         
                                                
  
                                                <div ng-if="output.pairwise_type == 'ptVerbal'" ng-class="{'top-row-mid' : $index==0, 'large-6 medium-12 columns top-row-mid': active_multi_index!=$index, 'columns text-center':active_multi_index==$index, 'small-6' : (isMobile() && active_multi_index==$index && !output.collapse_bars) || !isMobile(), 'small-12' : isMobile() && active_multi_index==$index && output.collapse_bars}" class="multi-div-fix medium-6 pw-verbal-desktop-bars-wrap">
                                                    <div class="small-12 columns text-left">
                                                       <a ng-if="output.show_comments && isMobile()" data-reveal-id="mobile-multi-tt-c-modal"
                                                            class="dropdown-btn show-for-medium-down multi-mobile comments-btn multoutput.show_commentsi-icon-fix" style="left:0 !important;">
                                                                <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                                        </a>
                                                        
                                                        <!-- original bars multi -->   
                                                        <div ng-if="active_multi_index!=$index && !output.collapse_bars" 
                                                              class="columns selected-item text-center selected-item-multi-pw multi-pw-verbal-labels-wrap anytime" ng-style="get_label_position(data.Advantage, data.Value)" 
                                                              ng-bind-html ="display_selected_bar(data.Value, data.Advantage)" style="top: -10px; margin-bottom: -20px">
                                                        </div>
                                                        <ul ng-cloak ng-if="active_multi_index != $index || output.collapse_bars" class="tt-equalizer-result" ng-style="{'top': (!isMobile() ? '10px' : '5px')}">
                                                        <li 
                                                            data-bar-index="{{::$index}}"
                                                            ng-click="add_multivalues(bar[0] , 1 , pair_index, $event)"
                                                            ng-repeat="bar in ::bars_left" 
                                                            data-index="{{::$parent.$index}}" 
                                                            title="{{::bar[1]}}" 
                                                            class="point {{::bar[2]}} lft  ownerResult tg-gradients {{data.Advantage == 1 && data.Value >= bar[0] ? 'active' : ''}}" 
                                                            style="margin-left: 4px; width: 4.2%;"
                                                            ng-class="main_gradient_checkbox ? '' : 'no-gradient'"
                                                            ng-style="{ 'background' : (data.Advantage == 1 && bar[0] > data.Value && bar[0] - data.Value < 1 &&  data.Value % 1 > 0 ? 'linear-gradient(270deg, #0058a3 '+ (bar[0] - data.Value) +'%, #cccccc 50%)' : '') }"
                                                            >
                                                            <span ng-if="bar[3] != ' '" class="text">{{::bar[3]}}</span>
                                                            <span ng-if="bar[3] == ' '" class="text pw-verbal-blank-bar"><span>0</span></span>
                                                        </li > 
                                                        <li 
                                                            data-bar-index="0"
                                                            ng-click="add_multivalues(1, 0 ,pair_index, $event)"
                                                            data-index="{{::$parent.$index}}"
                                                            title="Equal" 
                                                            style="width: 4.2%;"
                                                            class="point zero mid ownerResult tg-gradients "
                                                            ng-class="
                                                                    {
                                                                        'lft-selected' : data.Advantage ==  1, 
                                                                        'rgt-selected' : data.Advantage == -1,
                                                                        'active-selected' : data.Advantage == 0 && data.Value == 1,
                                                                        '' : data.Value < 0, 'no-gradient': main_gradient_checkbox}"
                                                        ><span class="text">EQ</span>
                                                        </li>
                                                        <li 
                                                            data-bar-index="{{::$index}}"
                                                            ng-click="add_multivalues(bar[0], -1 , pair_index, $event)"
                                                            ng-repeat="bar in ::bars_right" 
                                                            data-index="{{::$parent.$index}}" 
                                                            title="{{::bar[1]}}" 
                                                            class="point {{::bar[2]}} rgt ownerResult  tg-gradients {{data.Advantage == -1 && data.Value >= bar[0]  ? 'active' : '' }}" 
                                                            style="margin-right: 4px; width: 4.2%;"
                                                            ng-class="main_gradient_checkbox ? '' : 'no-gradient'"
                                                            ng-style="{ 'background' : (data.Advantage == -1 && bar[0] > data.Value && bar[0] - data.Value < 1 &&  data.Value % 1 > 0 ? ('linear-gradient(90deg, #6aa84f '+ (bar[0] - data.Value) +'%, #cccccc 50%)') : '') }"
                                                            >  
                                                            <span ng-if="bar[3] != ' '" class="text">{{::bar[3]}}</span>
                                                            <span ng-if="bar[3] == ' '" class="text pw-verbal-blank-bar"><span>0</span> </span>
                                                        </li>
                                                        </ul>                                    
                                                        <!-- //original bars multi -->                                   
                                                        <a id="{{index}}" ng-if="data.Value >= 0 && isMobile()" ng-click="add_multivalues(-2147483648000, 0 , pair_index, $event)" class="show-for-medium-down reset-btn verbal clrPairwiseResultn multi-icon-fix" title="" ng-class="{'not-collapsed' : !output.collapse_bars}">
                                                            <span class="icon-tt-close"></span>
                                                        </a>
                                                    </div>
                                                    
                                                     <%-- new bars --%>
                                                    <div ng-if="!isMobile()" class="hide-for-medium-down" >
                                                        <div class="tt-center-height-wrap hide-for-medium-down" style="position:absolute;top:50%;left:7px">
                                                            <a ng-if="output.show_comments"
                                                               data-options="align:top"
                                                               data-dropdown="comment-{{::$index}}" 
                                                               aria-controls="comment-{{::$index}}" 
                                                               aria-expanded="false"
                                                               class="dropdown-btn tt-center-content-wrap">
                                                                <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                                            </a>
                                                        </div>
                                                        <ul ng-if="active_multi_index == $index && !output.collapse_bars"  class="tt-equalizer-levels">
                                                                    <li title="" class="nine lft {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients">
                                                                        <span class="lvl-txt">Extreme</span>
                                                                        <asp:Image ID="Image2" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-1 eight lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Very Strong<br>
                                                                            to Extreme</span>
                                                                        <asp:Image ID="Image3" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="seven lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Very Strong</span>
                                                                        <asp:Image ID="Image4" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-2 six lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Strong to<br>
                                                                            Very Strong</span>
                                                                        <asp:Image ID="Image5" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="five lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Strong</span>
                                                                        <asp:Image ID="Image6" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-3 four lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Equal to<br>
                                                                            Strong</span>
                                                                        <asp:Image ID="Image7" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="three lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Moderate</span>
                                                                        <asp:Image ID="Image8" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-4 two lft {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Equal to<br>
                                                                            Moderate</span>
                                                                        <asp:Image ID="Image9" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>

                                                                    <li title="" class="zero mid {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Equal</span>
                                                                        <asp:Image ID="Image10" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>

                                                                    <li title="" class="lvl-even lvl-e-5 two rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Equal to<br>
                                                                            Moderate</span>
                                                                        <asp:Image ID="Image11" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="three rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Moderate</span>
                                                                        <asp:Image ID="Image12" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-6 four rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Moderate
                                                                            <br>
                                                                            to Strong</span>
                                                                        <asp:Image ID="Image17" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="five rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Strong</span>
                                                                        <asp:Image ID="Image13" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-7 six rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Strong to<br>
                                                                            Very Strong</span>
                                                                        <asp:Image ID="Image14" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="seven rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Very Strong</span>
                                                                        <asp:Image ID="Image18" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="lvl-even lvl-e-8 eight rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt hide">Very Strong<br>
                                                                            to Extreme</span>
                                                                        <asp:Image ID="Image15" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                    <li title="" class="nine rgt {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
                                                                        <span class="lvl-txt">Extreme</span>
                                                                        <asp:Image ID="Image16" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                                                    </li>
                                                                </ul>
                                                        <ul ng-if="active_multi_index == $index && !output.collapse_bars" id="" class="tt-equalizer large-10 medium-11 medium-centered large-centered columns">
                                                                    <li  
                                                                        data-bar-index="{{::$index}}"
                                                                        data-index="{{::$parent.$index}}" 
                                                                        id="{{::$index+1}}" 
                                                                        title="{{::bar[1]}}"
                                                                        class="{{::bar[2]}} pm-bars lft lft-eq lft-{{$index+1}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients"  
                                                                        data-pos="lft"
                                                                        ng-click="add_multivalues(bar[0] , 1 , pair_index, $event)" 
                                                                        ng-class="data.Advantage == 1 && data.Value >= bar[0] ? 'active-selected': ''" 

                                                                        ng-repeat="bar in ::bars_left" 
                                                                        ng-style="{ 'background' : (data.Advantage == 1 && bar[0] > data.Value && bar[0] - data.Value < 1 &&  data.Value % 1 > 0 ? 'linear-gradient(270deg, #0058a3 '+ (bar[0] - data.Value) +'%, #cccccc 50%)' : '') }"
                                                                    ></li>   
                                                                    <li 
                                                                        data-bar-index="0"
                                                                        data-index="{{::$parent.$index}}"
                                                                        id="9" 
                                                                        title="Equal"
                                                                        class="pm-bars zero mid lft-9 lft-eq rgt-eq rgt-9 mid-9  {{main_gradient_checkbox ? '' : 'no-gradient'}} 
                                                        {{data.Advantage == 1  ? 'lft-selected' : ''}}
                                                        {{data.Advantage == -1 ? 'rgt-selected' : ''}}  
                                                        {{data.Advantage == 0 && data.Value == 1 ? 'active-selected' : ''}}                                              
                                                                         tg-gradients" 
                                                                        data-pos="mid"
                                                                        ng-click="add_multivalues(1, 0 ,pair_index, $event)"
                                                                    ></li>
                                                                    <li 
                                                                        data-bar-index="{{::$index}}"
                                                                        data-index="{{::$parent.$index}}" 
                                                                        id="{{bars_right.length-$index}}" 
                                                                        title="{{::bar[1]}}" 
                                                                        class="{{::bar[2]}} pm-bars rgt lvl-even-bar rgt-eq rgt-{{bars_right.length-$index}} {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients" 
                                                                        data-pos="rgt" 
                                                                        ng-click="add_multivalues(bar[0], -1 , pair_index, $event) "
                                                                        ng-class="data.Advantage == -1 && data.Value >= bar[0] ? 'active-selected': ''" 
                                                                        ng-repeat="bar in ::bars_right" 
                                                                        ng-style="{ 'background' : (data.Advantage == -1 && bar[0] > data.Value && bar[0] - data.Value < 1 &&  data.Value % 1 > 0 ? ('linear-gradient(90deg, #6aa84f '+ (bar[0] - data.Value) +'%, #cccccc 50%)') : '') }"
                                                                        ></li>
                                                                </ul>   
                                                        
                                                        <div class="reset-btn-multi-loop tt-center-height-wrap hide-for-medium-dow">
                                                            <a id="{{::index}}" ng-if="data.Value >= 0" ng-click="add_multivalues(-2147483648000, 0 , pair_index, $event)" class="tt-center-content-wrap verbal clrPairwiseResultn multi-pw-verbal-close" title="">
                                                                <span class="icon-tt-close"></span>
                                                            </a>
                                                        </div>
                                                    </div>
                                                     <%-- //new bars --%>
                                                         
                                                         
                                                </div>
                                                <div style="padding-bottom:0px;" ng-if="output.pairwise_type == 'ptGraphical'" class="large-6 small-12 columns pw-graphical" >
                                                    <div class="small-12 columns large-text-left small-text-center " style="padding-top:5px;">
                                                        <div class="large-11 columns large-centered text-center graphical-slider multigraphical{{$last ? 0 : ''}}" id="gslider{{::$index}}"  
                                                            ng-model="participant_slider[$index]" index="{{::$index}}" 
                                                            ng-init="$last ? createGraphicalSlider() : ''" <%--initializeNoUIGraphical(participant_slider[$index], participant_slider[$index] + 800, $index, $last)"--%>>
                                                        </div>
                                                    </div>
                                                    <div class="small-12 columns large-text-left small-text-center" style="height:60px;">
                                                        <div class="large-7 medium-6  {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-7' : 'small-12'}} columns large-centered text-centered medium-centered small-centered graphical-nouislider">
                                                            <div class="large-3 medium-2 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-2' : 'small-3'}}  columns small-right text-right " style="margin-top:30px; padding-left:5px; padding-right:7px">
                                                                <a class="swap-judgment" ng-hide="output.multi_pw_data[$index].isUndefined" ng-click="swap_value($index)"
                                                                   style="float: right; padding-left: 8px;">
                                                                    <img src="../../Images/swap-icon.png" title="swap" alt="swap"  class="point" style="max-width:unset;width:22px" />
                                                                </a>
                                                                <a ng-if="output.show_comments" data-options="align:top"
                                                                   data-dropdown="comment-{{::$index}}" 
                                                                   aria-controls="comment-{{::$index}}" 
                                                                   aria-expanded="false" style="font-size: 15px; position: relative;"
                                                                   class="hide-for-medium-down dropdown-btn multi-mobile comments-btn multi-icon-fix">
                                                                    <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                                                </a>
                                                                <a ng-if="output.show_comments" data-reveal-id="mobile-multi-tt-c-modal"
                                                                   class="show-for-medium-down multi-mobile comments-btn multi-icon-fix"
                                                                   style="font-size: 15px; position: relative;">
                                                                    <span ng-class="data.Comment == '' ? 'comment-disabled' : ''" class="icon icon-tt-comments"></span>
                                                                </a>
                                                            </div>
                                                            <div class="large-3 medium-4 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns" style="margin-top:25px; padding-right:7px; padding-left:5px">
                                                                <input 
                                                                    class="graphical-input"
                                                                    type="number" id="input1{{$index}}" step="any" ng-model="left_input[$index]" style="height:30px; font-size:0.8rem;"
                                                                    tabindex="1" 
                                                                    ng-focus="focus_graphical_input($index)"
                                                                    ng-keyup="graphical_key_up($event, [left_input[$index], user[1], user[6]], -1, 'up', $index, false)"
                                                                    ng-keydown="graphical_key_up($event, [left_input[$index], user[1], user[6]], -1, 'press', $index, false)"
                                                                    ng-mouseup="graphical_key_up($event, [left_input[$index], user[1], user[6]], -1, 'up', $index, false)"
                                                                    ng-mousedown="graphical_key_up($event, [left_input[$index], user[1], user[6]], -1, 'press', $index, false)"/>
                                                            </div>
                                                            <div class="large-3 medium-4 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-4' : 'small-3'}} columns"  style="margin-top:25px;  padding-left:7px; padding-right:5px">
                                                                <input 
                                                                    class="graphical-input"
                                                                    type="number" id="input2{{::$index}}" step="any" ng-model="right_input[$index]" style="height:30px; font-size:0.8rem;"
                                                                    tabindex="1" 
                                                                    ng-focus="focus_graphical_input($index)"
                                                                    ng-keyup="graphical_key_up($event, [right_input[$index], user[1], user[6]], 1, 'up', $index, false)"
                                                                    ng-keydown="graphical_key_up($event, [right_input[$index], user[1], user[6]], -1, 'press', $index, false)"
                                                                    ng-mouseup="graphical_key_up($event, [right_input[$index], user[1], user[6]], 1, 'up', $index, false)"
                                                                    ng-mousedown="graphical_key_up($event, [right_input[$index], user[1], user[6]], -1, 'press', $index, false)"/>
                                                            </div>
                                                            <div class="large-3 medium-2 {{screen_sizes.option <= 3 && screen_sizes.option >= 2 ? 'small-2' : 'small-3'}} columns small-left text-left" style="margin-top:28px; padding-left:5px">
                                                                <span ng-if="left_input[$index] > 0" class="button tiny tt-button-primary tt-action-btn-clr-j " style="top: 3px" ng-click="add_multivalues(-2147483648000, 0 , $index, $event); set_slider_blank($index);"><span class="icon-tt-close icon"></span></span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>

                                                        
                                                <div ng-if="!isMobile()" class="medium-3 small-6 columns text-right hide-for-medium-down pw-verbal-desktop right-alternative">
                                                    <div <%--ng-class="{'small-10' : output.is_infodoc_tooltip && output.showinfodocnode, 'small-12':  !output.is_infodoc_tooltip || !output.showinfodocnode} "--%> class="small-12 end left text-center columns multi-right-node-wrap">
                                                        <div class="small-2 columns text-center tt-center-height-wrap"  ng-if="output.is_infodoc_tooltip || !output.showinfodocnode">
                                                            <a ng-if="(is_AT_owner || ( !is_AT_owner && !is_html_empty(data.InfodocRight))) && output.showinfodocnode"
                                                               data-dropdown="gdrop{{::$index}}_3" aria-controls="gdrop{{::$index}}_3" aria-expanded="false"
                                                               data-options="{{$index >= multi_data.length-1 ? 'align:top;' : 'align:top;'}} is_hover:true; hover_timeout:1000"
                                                               data-index="{{::$index}}" data-node-type="2" data-location="2" data-node="right-node" 
                                                               data-node-id="{{::data.NodeID_Right}}" data-node-description="{{data.RightNode}}"
                                                               data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                                               class="tt-center-content-wrap right-node-{{::data.NodeID_Right}}-{{::$index}}-tooltip i-icon tooltip-infodoc-btn">
                                                                <span ng-class="is_html_empty(data.InfodocRight) ? 'disabled' : 'not-disabled' "  class="icon icon-tt-info-circle"></span>
                                                               
                                                            </a>
                                                            <a ng-if="( !is_AT_owner && is_html_empty(data.InfodocRight))">
                                                                 <span>&nbsp;</span>
                                                            </a>
                                                         </div>
                                                        <div ng-class="{'small-8' : output.is_infodoc_tooltip && output.showinfodocnode, 'small-12':  !output.is_infodoc_tooltip || !output.showinfodocnode} " class="left columns multi-node-content">
                                                            <div class="tt-center-height-wrap">
                                                                <p 
                                                                      ng-class="{'desktop':(!output.collapse_bars && output.pairwise_type !== 'ptGraphical')}"
                                                                    class="default-desktop multi-right-node tt-center-content-wrap">             
                                                                    
                                                                    <span ng-if="active_multi_index == $index">
                                                                         {{::data.RightNode}}
                                                                    </span>
                                                                   <span ng-if="active_multi_index != $index">
                                                                          {{::data.RightNode.substring(0, 27)}}
                                                                          <span ng-if="data.RightNode.length > 27">
                                                                              ...
                                                                          </span>
                                                                    </span>
                                                                </p>
                                                            </div>
                                                        </div>
                                                        <div ng-if="(output.is_infodoc_tooltip && (is_AT_owner ||  (!is_AT_owner && !is_html_empty(data.InfodocRightWRT)))) && output.showinfodocnode" class="tt-center-height-wrap small-2 text-center columns">
                                                            <a data-dropdown="gdrop{{::$index}}_4" aria-controls="gdrop{{::$index}}_4" aria-expanded="false" 
                                                               data-options="{{$index >= multi_data.length-1 ? 'align:top;' : 'align:top;'}} is_hover:true; hover_timeout:1000"
                                                               data-node-type="3" data-index="{{::$index}}" data-location="2" data-node="wrt-right-node" data-node-title="{{data.RightNode}}" 
                                                               data-node-description="{{output.parent_node}}" data-node-id="{{::data.NodeID_Right}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                                                               class="tt-center-content-wrap wrt-right-node-{{::data.NodeID_Right}}-{{::$index}}-tooltip tooltip-infodoc-btn">
                                                                <span ng-class="is_html_empty(data.InfodocRightWRT) ? 'disabled' : 'not-disabled' "  class="icon icon-tt-w-font"></span>
                                                            </a>
                                                            <a ng-if="( !is_AT_owner && is_html_empty(data.InfodocRight))">
                                                                <span>&nbsp;</span>
                                                            </a>
                                                    </div>
                                                    </div>
                                                    <%--<div ng-if="(output.is_infodoc_tooltip && (is_AT_owner ||  (!is_AT_owner && !is_html_empty(data.InfodocRightWRT)))) && output.showinfodocnode" class="tt-center-height-wrap small-2 left text-center columns">
+                                                        <a 
+                                                            data-options="{{$index >= multi_data.length-1 ? 'align:top' : 'align:top'}}"
+                                                            data-dropdown="gdrop{{::$index}}_4" 
+                                                            aria-controls="gdrop{{::$index}}_4" 
+                                                            aria-expanded="false"
+                                                            class="tt-center-content-wrap wrt-right-node-{{::data.NodeID_Right}}-{{::$index}}-tooltip"  
+                                                            >
+                                                            <span 
+                                                                ng-class="is_html_empty(data.InfodocRightWRT) ? 'disabled' : 'not-disabled' "  class="icon icon-tt-w-font"></span>
+                                                        </a>
+                                                    </div>--%>
                                                </div>
                                                <!-- END DESKTOP view only -->
                                            </div>                               
                                            <!-- LOOP ends -->
                                        </div>
                                        <span ng-if="output.pairwise_type == 'ptVerbal' && gptoVerbal" class="columns text-center icon-tt-info-circle action "><%=AnytimeComparion.Pages.external_classes.TeamTimeClass.ResString("msgGPWJudgment") %></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>      
            </div>

            </div>
     </div>
  <script src="/Scripts/NoUI/nouislider.min.js"></script>
<script src="/Scripts/NoUI/wNumb.js"></script>
<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/bundles/pairwise") %>
</asp:PlaceHolder>

<script>
    $(document).ready(function () {
        function show_multi_wrap() {
            $(".multi-loop-wrap").removeClass("hide");

            //checkMultiLoopHeight();
            var max = $('.noUi-base').width();
            var zero = (max / 2);
            //console.log(zero);
            $(".graph-green-div").width(zero - 4);
            //$(".graph-green-div").css("background", "#6aa84f");
        }


        setTimeout(function () {
            show_multi_wrap();
        },600);

        

        //var scope = angular.element($("#anytime-page")).scope();
        var scope;
        if (is_anytime) {
            scope = angular.element($("#anytime-page")).scope();
        } else {
            scope = angular.element($("#TeamTimeDiv")).scope();
        }

        try {
            if (scope.isMobile()) {
                setTimeout( function(){
                 scope.mobile_manual_equalizer();
                }, 700);
               
            }

        }
        catch (e) {

        }


    });

</script>