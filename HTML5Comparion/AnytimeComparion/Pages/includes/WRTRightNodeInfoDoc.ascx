<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WRTRightNodeInfoDoc.ascx.cs" Inherits="AnytimeComparion.Pages.includes.WRTRightNodeInfoDoc" %>

<span ng-if="is_anytime">
    <div ng-if="output.is_infodoc_tooltip && output.showinfodocnode"
         class="tooltip-mode"
          ng-class="{
            'text-center ' : output.page_type!='atPairwise',
            'hide' : (is_AT_owner == 0 &&  is_html_empty(output.wrt_second_node_info))
            }">
        
   
        <span>
            <a data-dropdown="gdrop_wrt_right_node" data-options="is_hover:true; hover_timeout:1000" 
               aria-controls="gdrop_wrt_right_node" aria-expanded="false" data-node-type="3"
               data-location="2" data-node="wrt-right-node" data-node-title="{{output.first_node}}{{output.child_node}}" 
               data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
               class="wrt-right-node-tooltip tooltip-infodoc-btn">
                    <span ng-if="is_AT_owner || (!is_AT_owner && !is_html_empty(output.wrt_second_node_info))"
                          ng-class="is_html_empty(output.wrt_second_node_info) ? 'disabled' : 'not-disabled'" 
                          class="icon icon-tt-w-font"></span>
            </a>
            <%--<span ng-if="output.page_type!='atPairwise'">{{output.second_node}} {{output.child_node}} WRT {{output.parent_node}}</span>--%>
            <span <%--ng-if="output.page_type=='atPairwise'--%>"> {{get_wrt_text('right')}} </span>
        </span>
        <%-- show dropdown only in non pw since dropdown of PW are in Pairwise.ascx files --%>
         <div 
              ng-class="{'medium' : has_long_content(output.wrt_second_node_info)}"
             ng-if="output.page_type!='atPairwise'" id="gdrop_wrt_right_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                        {{get_wrt_text('right')}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                                   data-node-type="3" data-location="1" data-node="wrt-right-node" 
                                   data-node-title="{{output.first_node}}{{output.child_node}}"
                                   data-node-description="{{output.parent_node}}">
                                    <span class="icon-tt-edit"></span>
                                </a>  
                            </div>
                            <div class="small-6 columns text-center">
                                <a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                            </div>
                        </div>
                    </div>
                <hr>
            </div>
            <div class="row">
                <div class="infotext-wrap large-12 columns wrt-right-node-info-text" ng-bind-html="getHtml(output.wrt_second_node_info)"></div>
            </div>
        </div>

    </div>
    <div
        ng-if="!output.is_infodoc_tooltip && output.showinfodocnode"
        ng-class="{'hide' : (is_AT_owner == 0 &&  is_html_empty(output.wrt_second_node_info))}" class="columns tt-accordion-sub">
        <div class="columns tt-accordion-head {{main_gradient_checkbox ? '' : 'no-gradient'}}  tg-gradients">
            <a ng-click="set_collapse_cookies('wrt-right-node'); update_infodoc_params('wrt-right-node', output.RightNodeGUID, output.ParentNodeGUID)"
             <%--   ng-click="set_collapse_cookies('wrt-right-node')" --%>
                ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0)}"
                class="left hide-when-editing-4 tt-toggler-4" id="4"  data-toggler="tg-accordion-sub">
                <span 
                    ng-class="{
                        'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[3] ,
                        'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[3]
                    }"
                    class="icon icon-desktop" data-node="4"></span>
            </a>
        
                   <%-- <span 
                         ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}"
                         class="text">
                        <span class="second-node">{{output.second_node}}{{output.child_node}}</span>
                            WRT  
                        <span class="parent-node">{{output.parent_node}}</span>
                    </span>--%>

                <%-- Infodoc Heading --%>

               <span id="4" class="black hide-when-editing-4 text et-4" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}">{{output.hideInfoDocCaptions ? "" : get_wrt_text('right')}}</span>
                
            <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.wrt_second_node_info)"
               class="magnify-icon edit-info-doc-btn"
               data-node-type="3" data-location="2" data-node="wrt-right-node" data-node-title="{{output.second_node}}" 
               data-node-description="{{output.parent_node}}" data-readonly="1">
                <span class="icon icon-tt-zoom-in"></span>
            </a>

                <%-- if undefined show default --%>
               <%-- <span 
                    ng-class="{'editable-trigger' : is_AT_owner == 1, 'white':output.page_type =='atPairwise', 'black':output.page_type !='atPairwise'}"  ng-if="info_docs_headings[4] == '-1'" id="4" class="hide-when-editing-4 text et-4" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}">{{get_wrt_text('right')}}</span>--%>
                
                <%-- if blank --%>
               <%-- <span ng-class="{'editable-trigger' : is_AT_owner == 1}"  ng-if="info_docs_headings[4] == '-2'" id="4" class="hide-when-editing-4 text et-4" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}" style="color:white">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>--%>

               <%-- if has value --%>
              <%--  <span 
                    ng-class="{'editable-trigger' : is_AT_owner == 1}"  ng-if="info_docs_headings[4] != '-1' && info_docs_headings[4] != '-2'" id="4" class="black hide-when-editing-4 text et-4" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}">
                    {{info_docs_headings[4]}}
                </span>--%>

                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-4">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading" id="WRT_right_heading" name="" placeholder="Add something here" type="text" class="text editable-input ei-4" />
                    </div>
                    <div class="small-12 columns">
                      <a id="4" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                      <a ng-click="update_infodoc_params('wrt-right-node', output.RightNodeGUID, output.ParentNodeGUID, 'WRT_right_heading')" id="4" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                <!-- end editing -->
                <%-- end of info doc heading --%>
    
        </div>
        <div
            ng-class="{
                'hide' : !output.showinfodocnode || collapsed_info_docs[3] || (is_AT_owner != 1 && is_html_empty(output.wrt_second_node_info)) }"
                class="wrt-right-node-info-div columns tt-accordion-content tg-accordion-sub-4 ">
            <div data-index="2" class="tt-panel-temp tt-panel  tt-resizable-panel tt-resizable-panel-2 box-4 small-centered columns">
                <a  ng-class="{
                    'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[3] }"
                    data-node-type="3"
                    data-location="2" 
                    data-node="wrt-right-node" 
                    data-node-title="{{output.second_node}}" 
                    data-node-description="{{output.parent_node}}"
                    class="right edit-info-doc-btn edit-pencil ep-sub-4 edit-info-doc-btn" 
                    title=""
                    href="#">
                    <span  ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                </a>
            <div class="wrt-right-node-info-text nodes-wrap" ng-bind-html="getHtml(output.wrt_second_node_info)" >
            </div>
            </div>
        </div>
    </div>
</span>

<span ng-if="is_teamtime">
    <div 
        ng-if="!is_infodoc_tooltip && output.showinfodocnode"
        ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.WrtRightNodeInfo))}"
        class="columns tt-accordion-sub teamtime-wrt-right">
        <div class="columns tt-accordion-head tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
            <a 
                ng-class="{'tt-toggler' :(!output.isPM && !is_html_empty(output.pipeData.WrtRightNodeInfo)) || output.isPM }"
                ng-click="update_infodocs(4, $event)" id="4" data-toggler="tg-accordion-sub">
                <span
                     ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.WrtRightNodeInfo))}"
                     class="icon-tt-plus-square icon icon-desktop" data-node="4"></span>
                <%--<span class="right-node">{{information_nodes_infodocs.RightNode}}</span>--%>
                WRT
               <%-- <span class="parent-node">{{information_nodes.ParentNode}}</span>--%>
            </a>                      
        </div>
        <div class="columns tt-accordion-content tg-accordion-sub-4 hide">
            <div data-index="4" class="tt-panel  tt-resizable-panel tt-resizable-panel-4 box-4 small-centered columns"> 
                <!-- Edit Icon -->
                <a 
                ng-if="output.isPM"
                data-node-title="{{information_nodes.RightNode}}" 
                data-node-description="{{information_nodes.ParentNode}}"
                data-node-type="3" data-location="2" data-node="wrt-right-node" class="edit-pencil ep-sub-4 edit-info-doc-btn" title="" href="#">
                <span class="icon-tt-pencil"></span>
                </a>              
                <!-- End Edit Icon -->               
                <div class="wrt-right-node-info-text nodes-wrap" id="WrtRightNode" ng-bind-html="getHtml(output.pipeData.WrtRightNodeInfo)">
                </div>
            </div>
        </div>

    </div>
    
    <div ng-if="is_infodoc_tooltip && output.showinfodocnode"  class="tooltip-mode text-center" 
        ng-class="{
            ' ' : current_action_type!='pairwise',
            'hide': !output.isPM &&  is_html_empty(output.pipeData.WrtRightNodeInfo)
        }">
        <span>
            <a  
                data-dropdown="gdrop_wrt_right_node" 
                aria-controls="gdrop_wrt_right_node" 
                aria-expanded="false"
                class="wrt-right-node-tooltip"
                >
                    <span 
                        ng-if="output.isPM || (!output.isPM && !is_html_empty(output.pipeData.WrtRightNodeInfo))"
                        ng-class="is_html_empty(output.pipeData.WrtRightNodeInfo) ? 'disabled' : 'not-disabled'" class="icon icon-tt-info-circle"></span>
            </a>
            <span ng-if="current_action_type!='pairwise'">{{information_nodes.RightNode}} WRT {{information_nodes.ParentNode}}</span>
            <span ng-if="current_action_type=='pairwise'"> WRT </span>
        </span>
         <%-- show dropdown only in non pw since dropdown of PW are in Pairwise.ascx files --%>
         <div 
             ng-class="{'medium' : has_long_content(output.pipeData.WrtRightNodeInfo)}"
             id="gdrop_wrt_right_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                       WRT
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a ng-class="output.isPM ? '' : 'hide'" 
                                    class="edit-info-doc-btn" 
                                    data-node-type="3"
                                    data-location="2" 
                                    data-node="wrt-right-node" 
                                    data-node-title="{{information_nodes.RightNode}}" 
                                    data-node-description="{{information_nodes.ParentNode}}"
                                    >
                                    <span class="icon-tt-edit"></span>
                                </a>  
                            </div>
                            <div class="small-6 columns text-center">
                                <a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                            </div>
                        </div>
                    </div>
                <hr>
            </div>
            <div class="row">
                <div class="infotext-wrap large-12 columns wrt-wight-node-info-text" ng-bind-html="getHtml(output.pipeData.WrtRightNodeInfo)"></div>
            </div>
        </div>
    </div>
</span>