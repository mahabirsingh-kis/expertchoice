<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RightNodeInfoDoc.ascx.cs" Inherits="AnytimeComparion.Pages.includes.RightNodeInfoDoc" %>
<%-- anytime --%>
<span ng-if="is_anytime">
    <div 
        data-equalizer-watch="nodes"
        ng-if="output.is_infodoc_tooltip && (output.showinfodocnode || (!output.showinfodocnode &&  output.page_type=='atPairwise'))"
        class="tooltip-mode"
         ng-class="{
            'text-center ' : output.page_type!='atPairwise',
            'tt-accordion-head green no-gradient  tg-gradients vs-right': output.page_type=='atPairwise'
        }"
        >
        <span>
            <a ng-if="output.showinfodocnode" data-dropdown="gdrop_right_node" data-options="is_hover:true; hover_timeout:1000" 
               aria-controls="gdrop_right_node" aria-expanded="false" data-node-type="2" data-location="2"
               data-node="right-node" data-node-description=" {{output.second_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                class="right-node-tooltip tooltip-infodoc-btn">
                <span ng-if="is_AT_owner || (!is_AT_owner && !is_html_empty(output.second_node_info))" 
                      ng-class="is_html_empty(output.second_node_info) ? 'disabled' : 'not-disabled'" 
                      class="icon icon-tt-info-circle"></span>
            </a>
            {{output.second_node}}{{output.child_node}}
        </span>
        <%-- show dropdown only in non pw since dropdown of PW are in Pairwise.ascx files --%>
         <div 
             ng-class="{'medium' : has_long_content(output.second_node_info)}"
             ng-if="output.page_type!='atPairwise'"  id="gdrop_right_node" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                        {{output.parent_node}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a style="color: #008CBA;"
                                    ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                                    data-node-type="2" data-location="2" data-node="right-node" 
                                    data-node-description=" {{output.second_node}}">
                                    <span class="icon-tt-edit"></span>
                                </a>  
                            </div>
                            <div class="small-6 columns text-center">
                                <a style="color: #008CBA;" href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                            </div>
                        </div>
                    </div>
                <hr>
            </div>
            <div class="row">
                <div class="infotext-wrap large-12 columns right-node-info-text" ng-bind-html="getHtml(output.second_node_info)"></div>
            </div>
        </div>
    </div>
    <div  style="min-height: 40px;"
        ng-if="!output.is_infodoc_tooltip"
        data-equalizer-watch="nodes"  
         class="columns tt-accordion-head green {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients vs-right">
        <div class="tt-center-height-wrap columns">
            <div class="tt-center-content-wrap">
                <a ng-click="set_collapse_cookies('right-node');update_infodoc_params('right-node', output.RightNodeGUID, output.LeftNodeGUID)"
                   <%-- ng-click="set_collapse_cookies('right-node')" --%>
                    ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 &&  !is_html_empty(output.second_node_info))}"
                    class="tt-toggler-2" id="2" data-toggler="tg-accordion-sub">

                    <span 
                        ng-if="output.showinfodocnode"
                            ng-class="{
                            'hide' :(is_AT_owner != 1 && is_html_empty(output.second_node_info)),
                            'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[1],
                            'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[1]
                        }"
                        class="icon icon-desktop" data-node="2" ></span>
                </a>

        
    
    
            <a 
                 ng-if="output.pairwise_type == 'ptVerbal'"
                 href="#" data-reveal-id="dropRightNode"><span class="right-node"> {{output.second_node}} </span></a>
            <a 
                 ng-if="output.pairwise_type != 'ptVerbal'"
                 href="#"><span class="right-node"> {{output.second_node}} </span></a>
                
                <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.second_node_info)"
                   class="magnify-icon edit-info-doc-btn"
                   data-node-type="2" data-location="2" data-node="right-node" data-node-description="{{output.second_node}}" data-readonly="1">
                    <span class="icon icon-tt-zoom-in"></span>
                </a>

            <%--{{is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[1]}}--%>           
         </div>
        </div>
    </div>
    <div
        ng-if="!output.is_infodoc_tooltip"
        ng-class="{
            'hide' :   !output.showinfodocnode || collapsed_info_docs[1] || (is_AT_owner != 1 && is_html_empty(output.second_node_info))
        }"
        class="right-node-info-div columns tt-accordion-content  tg-accordion-sub-2 ">
        <div data-index="1" class="tt-panel-temp tt-panel  tt-resizable-panel tt-resizable-panel-1 box-2 small-centered columns">
             <a 
                ng-class="{
                        'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[1] }"
                data-node-type="2" 
                data-location="2" 
                data-node="right-node" 
                data-node-description="{{output.second_node}}" 
                class=" edit-info-doc-btn edit-pencil ep-sub-2 edit-info-doc-btn" 
                title="" 
                href="#">
                    <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
            </a>
            <div class="right-node-info-text nodes-wrap" ng-bind-html="getHtml(output.second_node_info)">
            </div>
        </div>
    </div>
</span>
<%-- //anytime --%>
<%-- teamtime --%>
<span ng-if="is_teamtime">
    <span ng-if="!is_infodoc_tooltip">
        <div  data-equalizer-watch class="columns tt-accordion-head green lite tg-gradients " ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
            <a 
                    class=""
                    ng-class="{'tt-toggler' :((!output.isPM && !is_html_empty(output.pipeData.RightNodeInfo)) || output.isPM) && output.showinfodocnode }"
                    ng-click="update_infodocs(2, $event)" id="2" data-toggler="tg-accordion-sub">
                <span 
                    ng-if="output.showinfodocnode"
                    ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.RightNodeInfo))}"
                    class="icon-tt-plus-square icon icon-desktop" data-node="2" ></span>
                <span 
                    ng-if="(output.showinfodocnode) || (!output.showinfodocnode && current_action_type == 'pairwise')"
                    class="right-node">{{information_nodes.RightNode}}</span>
                </a>                          
        </div>
        <div class="columns tt-accordion-content  tg-accordion-sub-2 hide">
            <div data-index="2" class="tt-panel  tt-resizable-panel tt-resizable-panel-2 box-2 small-centered columns">  
             <!-- Edit Icon -->
                <a 
                    ng-if="output.isPM"
                    data-node-description="{{information_nodes.RightNode}}"
                    data-node-type="2" data-location="2" data-node="right-node" class="edit-pencil ep-sub-2 edit-info-doc-btn" title="" href="#"><span class="icon-tt-pencil"></span></a>   
             <!-- End Edit Icon -->          
            <div class="right-node-info-text nodes-wrap" id="rightnodeInfo" ng-bind-html="getHtml(output.pipeData.RightNodeInfo)" >
            </div>
            </div>
        </div>
    </span>
    <div 
        ng-if="is_infodoc_tooltip && (output.showinfodocnode || (!output.showinfodocnode && current_action_type == 'pairwise'))"
        class="tooltip-mode"
            ng-class="{
            'text-center ' : current_action_type != 'pairwise',
            'tt-accordion-head green no-gradient  tg-gradients vs-left': current_action_type=='pairwise',
            'hide' : !output.isPM &&  is_html_empty(output.pipeData.RightNodeInfo) && current_action_type != 'pairwise'
        }"
        >
        <span>
            <a  
                ng-if="output.showinfodocnode"
                data-dropdown="gdrop_right_node" 
                aria-controls="gdrop_right_node" 
                aria-expanded="false"
                class=""
                >
                    <span 
                        ng-if="output.isPM || (!output.isPM && !is_html_empty(output.pipeData.RightNodeInfo))"
                        ng-class="is_html_empty(output.pipeData.RightNodeInfo) ? 'disabled' : 'not-disabled'" class="icon icon-tt-info-circle"></span>
            </a>
            {{information_nodes.RightNode}}
        </span>
        <div 
            ng-class="{'medium' : has_long_content(output.pipeData.RightNodeInfo)}"
            id="gdrop_right_node" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                          {{information_nodes.RightNode}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a  
                                    style="color: #008CBA;"
                                    ng-class="output.isPM ? '' : 'hide'" 
                                    class="edit-info-doc-btn" 
                                    data-node-type="2" 
                                    data-location="2"  
                                    data-node="right-node" 
                                    data-node-description=" {{information_nodes.Right}}"
                                    >
                                    <span class="icon-tt-edit"></span>
                                </a>  
                            </div>
                            <div class="small-6 columns text-center">
                                <a style="color: #008CBA;" href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                            </div>
                        </div>
                    </div>
                <hr>
                </div>
                <div class="row">
                    <div class="infotext-wrap large-12 columns right-node-info-text" ng-bind-html="getHtml(output.pipeData.RightNodeInfo)"></div>
                </div>
            </div>
        </div>
</span>
<%-- //teamtime --%>
