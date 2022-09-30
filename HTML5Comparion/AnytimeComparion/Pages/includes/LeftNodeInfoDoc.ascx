<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LeftNodeInfoDoc.ascx.cs" Inherits="AnytimeComparion.Pages.includes.LeftNodeInfoDoc" %>
<%-- anytime --%>


<span ng-if="is_anytime">
    <div 
      data-equalizer-watch="nodes"
      ng-if="output.is_infodoc_tooltip && (output.showinfodocnode || (!output.showinfodocnode &&  output.page_type=='atPairwise'))"
        class="tooltip-mode"
         ng-class="{
            'text-center ' : output.page_type!='atPairwise',
            'tt-accordion-head blue no-gradient  tg-gradients vs-left': output.page_type=='atPairwise',
            'hide' : is_AT_owner == 0 &&  is_html_empty(output.first_node_info) && output.page_type != 'atPairwise'
        }"
        >
        <span>
            <a ng-if="output.showinfodocnode" data-dropdown="gdrop_left_node" aria-expanded="false" 
               data-options="is_hover:true; hover_timeout:1000" aria-controls="gdrop_left_node" 
               data-node-type="2" data-location="1" data-node="left-node" 
               data-node-description="{{output.first_node}}{{output.child_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
               class="left-node-tooltip tooltip-infodoc-btn">
                <span ng-if="is_AT_owner || (!is_AT_owner && !is_html_empty(output.first_node_info))" 
                      ng-class="is_html_empty(output.first_node_info) ? 'disabled' : 'not-disabled'" 
                      class="icon icon-tt-info-circle"></span>
            </a>
            {{output.first_node}}{{output.child_node}}
        </span>
         <div 
             ng-class="{'medium' : has_long_content(output.first_node_info)}"
             ng-if="output.page_type!='atPairwise'" id="gdrop_left_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                        {{output.parent_node}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a style="color: #008CBA;"
                                   ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" data-node-type="2" data-location="1" 
                                   data-node="left-node" data-node-description=" {{output.first_node}}{{output.child_node}}">
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
                <div class="infotext-wrap large-12 columns left-node-info-text" ng-bind-html="getHtml(output.first_node_info)"></div>
            </div>
        </div>
    </div>

    <div ng-if="is_AT_owner == 0 &&  is_html_empty(output.first_node_info) && output.page_type != 'atPairwise'">
        &nbsp;
    </div>

    <div 
        ng-class="{'hide' : (is_AT_owner == 0 &&  is_html_empty(output.first_node_info) && output.page_type != 'atPairwise')}"
        ng-if="!output.is_infodoc_tooltip"
        data-equalizer-watch="nodes"  
        class="columns tt-accordion-head {{output.page_type == 'atPairwise' ? 'blue' : ''}} {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients vs-left">
        <div class="tt-center-height-wrap columns">
            <div class="tt-center-content-wrap">
            <a ng-class="
                {
                    'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(output.first_node_info))
                }"
               <%-- ng-click="set_collapse_cookies('left-node')" --%>
                ng-click="set_collapse_cookies('left-node'); update_infodoc_params('left-node', output.LeftNodeGUID, output.RightNodeGUID)"
                class="tt-toggler-1" id="1"  data-toggler="tg-accordion-sub">
                <span
                    ng-if="output.showinfodocnode"
                    ng-class="{
                        'hide' :(is_AT_owner != 1 && is_html_empty(output.first_node_info)),
                        'icon-tt-minus-square' : output.showinfodocnode && !collapsed_info_docs[1],
                        'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[1]
                    }"
                        class="icon icon-desktop" data-node="1"></span>
            </a>



            <a 
                ng-if="output.pairwise_type == 'ptVerbal'" href="#" data-reveal-id="dropLeftNode"><span class="left-node child-label" ng-class="{'black':output.page_type !='atPairwise'}"> {{output.first_node}}  {{output.child_node}}</span></a>

            <a 
                ng-if="output.pairwise_type != 'ptVerbal'" href="#"><span class="left-node child-label" ng-class="{'black':output.page_type !='atPairwise', 'hide': output.page_type == 'atNonPWOneAtATime'}"> {{output.first_node}}  {{output.child_node}}</span></a>

            <span
                 ng-if="output.page_type != 'atPairwise' && output.showinfodocnode" >
                <span  id="1" class="hide-when-editing-1 text et-1 black">
                    {{output.hideInfoDocCaptions ? "" : output.first_node + (output.first_node.length > 0 ? " " : "") + output.child_node}}
                </span>
               <%-- <span 
                    ng-class="{'editable-trigger' : is_AT_owner == 1, 'white':output.page_type =='atPairwise', 'black':output.page_type !='atPairwise'}" ng-if="info_docs_headings[1] == '-1'"  id="1" class="hide-when-editing-1 text  et-1">
                    {{output.first_node}}  {{output.child_node}}
                </span>


                <span ng-class="{'editable-trigger' : is_AT_owner == 1}" ng-if="info_docs_headings[1] == '-2'" id="1" class="hide-when-editing-1 text et-1" style="color:#000;" >
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                </span>

                <span ng-class="{'editable-trigger' : is_AT_owner == 1}" ng-if="info_docs_headings[1] != '-1' && info_docs_headings[0] != '-2'" id="1" class="hide-when-editing-1 text et-1 black" >
                    {{info_docs_headings[1]}}
                </span>--%>

                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-1 hide">
                    <div class="small-12 columns">
                        <input ng-model="left_heading" id="left_heading" name="" placeholder="Add something here" type="text" class="text editable-input ei-1 parent" />
                    </div>
                    <div class="small-12 columns">
                        <a id="1" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a ng-click="update_infodoc_params('left-node', output.LeftNodeGUID, '', 'left_heading')" id="1" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
            </span>
            <!-- end editing -->
            <%-- end of info doc heading --%>
                
                <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.first_node_info)"
                   class="magnify-icon edit-info-doc-btn"
                   data-node-type="2" data-location="1" data-node="left-node" 
                   data-node-description="{{output.first_node}}{{output.child_node}}" data-readonly="1">
                    <span class="icon icon-tt-zoom-in"></span>
                </a>
            </div>
        </div>
    </div>
    <div
        ng-if="!output.is_infodoc_tooltip"
        ng-class="{
                    'hide' :  !output.showinfodocnode || collapsed_info_docs[1] || ( is_AT_owner != 1 && is_html_empty(output.first_node_info) )}"
            class="left-node-info-div columns tt-accordion-content tg-accordion-sub-1" > 
        <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 box-1 small-centered columns">
            <a  
                ng-class="{
                        'hide': is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[1] 
                }"
                data-node-type="2" 
                data-location="1" 
                data-node="left-node" 
                data-node-description="{{output.first_node}}{{output.child_node}}" 
                class="edit-info-doc-btn edit-pencil ep-sub-1 edit-info-doc-btn"
                title=""
                href="#"
                >
                <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
            </a>
            <div class="left-node-info-text nodes-wrap" ng-bind-html="getHtml(output.first_node_info)">
            </div>
        </div>
    </div>
</span>
<%-- //anytime --%>

<%-- if teamtime --%>
<span ng-if="is_teamtime">
    <span ng-if="!is_infodoc_tooltip">
        <div data-equalizer-watch class="columns tt-accordion-head lite tg-gradients " ng-class="{'no-gradient' : main_gradient_checkbox, 'blue':current_action_type == 'pairwise'}">
                <a 
                    ng-class="{'tt-toggler' :((!output.isPM && !is_html_empty(output.pipeData.LeftNodeInfo)) || output.isPM) && output.showinfodocnode }"
                    class="" ng-click="update_infodocs(1, $event)" id="1" data-toggler="tg-accordion-sub">
                <span 
                    ng-if="output.showinfodocnode"
                    ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.LeftNodeInfo))}"
                    class="icon-tt-plus-square icon icon-desktop" data-node="1"></span>
                <span ng-if="(output.showinfodocnode) || (!output.showinfodocnode && current_action_type == 'pairwise')" class="left-node">{{information_nodes.LeftNode}}</span>
            </a>                     
        </div>
        <div 
            class="columns tt-accordion-content tg-accordion-sub-1 hide">
            <div data-index="1" class="tt-panel tt-resizable-panel tt-resizable-panel-1 box-1 small-centered columns"> 
                <!-- Edit Icon -->
                <a 
                ng-if="output.isPM"
                data-node-description="{{information_nodes.LeftNode}}"
                data-node-type="2" data-location="1" data-node="left-node" class="edit-pencil ep-sub-1 edit-info-doc-btn" title="" href="#"><span class="icon-tt-pencil"></span></a>                  
                <!-- End Edit Icon -->
                <div class="left-node-info-text nodes-wrap" id="leftnodeInfo" ng-bind-html="getHtml(output.pipeData.LeftNodeInfo)">
                </div>
            </div>
        </div>
    </span>
      
    
    <div 
        ng-if="is_infodoc_tooltip && (output.showinfodocnode || (!output.showinfodocnode && current_action_type == 'pairwise'))"
        class="tooltip-mode"
         ng-class="{
            'text-center ' : current_action_type != 'pairwise',
            'tt-accordion-head blue no-gradient  tg-gradients vs-left': current_action_type=='pairwise',
            'hide' : !output.isPM &&  is_html_empty(output.first_node_info) && current_action_type != 'pairwise' 
         }"
        >
        <span>
            <a  
                ng-if="output.showinfodocnode"
                data-dropdown="gdrop_left_node" 
                aria-controls="gdrop_left_node" 
                aria-expanded="false"
                class=""
                >
                    <span 
                        ng-if="output.isPM || (!output.isPM && !is_html_empty(output.pipeData.LeftNodeInfo))"
                        ng-class="is_html_empty(output.pipeData.LeftNodeInfo) ? 'disabled' : 'not-disabled'" class="icon icon-tt-info-circle"></span>
            </a>
           {{information_nodes.LeftNode}}
        </span>
        <div 
            ng-class="{'medium' : has_long_content(output.pipeData.LeftNodeInfo)}"
            id="gdrop_left_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                         {{information_nodes.LeftNode}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a  
                                    style="color: #008CBA;"
                                    ng-class="output.isPM ? '' : 'hide'" 
                                    class="edit-info-doc-btn" 
                                    data-node-type="2" 
                                    data-location="1"  
                                    data-node="left-node" 
                                    data-node-description="{{information_nodes.LeftNode}}"
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
                <div class="infotext-wrap large-12 columns left-node-info-text" ng-bind-html="getHtml(output.pipeData.LeftNodeInfo)"></div>
            </div>
        </div>
    </div>
</span>
<%-- //teamtime --%>


