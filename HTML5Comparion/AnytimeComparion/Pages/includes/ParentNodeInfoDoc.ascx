<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ParentNodeInfoDoc.ascx.cs" Inherits="AnytimeComparion.Pages.includes.ParentNodeInfoDoc" %>
<span ng-if="is_anytime">
    <%--div 
        class="hidden-for-large-up"
        ng-if="is_AT_owner == 0 &&  is_html_empty(output.parent_node_info) && output.showinfodocnode">
        <div class="columns tt-accordion-head text-left">
            <div ng-if="output.page_type != 'atAllPairwise' && output.page_type != 'atNonPWAllChildren' && output.page_type == 'atPairwise' && output.page_type != 'atNonPWAllCovObjs' && output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults' && output.show_comments" class="mobile-comment-btn right hidden-for-large-up">
                <a href="#" data-reveal-id="tt-c-modal" class="toggleComments smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" ><span class="icon-tt-comments  icon-30"></span></a>
            </div>
        </div>
    </div>--%>
    <div ng-if="output.is_infodoc_tooltip && output.showinfodocnode"
        class="tooltip-mode"
        ng-class="{
            'text-center ' : output.page_type!='atPairwise',
            'hide' : is_AT_owner == 0 &&  is_html_empty(output.parent_node_info)
        }"
        >
            <span>
            <a data-dropdown="gdrop_parent_node" data-options="is_hover:true; hover_timeout:1000" 
                aria-controls="gdrop_parent_node" aria-expanded="false"
                data-node-type="-1" data-location="-1" data-node="parent-node" 
                data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                class="text parent-node-tooltip tooltip-infodoc-btn">
                    <span ng-if="is_AT_owner || (!is_AT_owner && !is_html_empty(output.parent_node_info))" 
                          ng-class="is_html_empty(output.parent_node_info) ? 'disabled' : 'not-disabled'" 
                          class="icon icon-tt-info-circle"></span>
            </a>
            {{output.parent_node}}

             <%-- show for pw screen only --%>
            <%--<div ng-if="output.show_comments && output.page_type == 'atPairwise'" class="mobile-comment-btn right">
                <a href="#" data-reveal-id="tt-c-modal" class="toggleComments smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" ><span class="icon-tt-comments  icon-30"></span></a>
            </div>--%>
        </span>

         <%-- show dropdown only in non pw since dropdown of PW are in Pairwise.ascx files --%>
         <div 
             ng-class="{'medium' : has_long_content(output.parent_node_info)}"
             ng-if="output.page_type!='atPairwise'"  id="gdrop_parent_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                        {{output.parent_node}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                                    data-node-type="-1" data-location="-1" data-node="parent-node" 
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
                <div class="infotext-wrap large-12 columns parent-node-info-text" ng-bind-html="getHtml(output.parent_node_info)"></div>
            </div>
        </div>
    </div>
     
    <div 
         ng-if="!output.is_infodoc_tooltip && output.showinfodocnode"
         ng-class="{'hide' : (is_AT_owner == 0 &&  is_html_empty(output.parent_node_info))}"
         class="large-12 columns" ng-if="!is_multi|| (is_multi && output.is_infodoc_tooltip)">
        <div class="columns tt-accordion-head"
            ng-class="{'text-center': output.page_type != 'atPairwise', 'text-left': output.page_type == 'atPairwise'}"
            >
            <a 
                ng-click="set_collapse_cookies('parent-node'); update_infodoc_params('parent-node', output.ParentNodeGUID, '')"
              <%--  ng-click="set_collapse_cookies('parent-node')" --%>
                ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(output.parent_node_info))}"
                class="tt-toggler-0" id="0" data-toggler="tg-accordion">
                <span 
                    ng-class="{
                                'hide' : (is_AT_owner == 0  &&  is_html_empty(output.parent_node_info)),
                                'icon-tt-minus-square' : output.showinfodocnode && !collapsed_info_docs[0],
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[0]
                            }"
                    class="icon icon-desktop" data-node="0"></span>
            </a>

             <%-- boomike --%>
                <span
                    class="text parent-lbl parent-node black">
                    {{output.hideInfoDocCaptions ? "" : output.parent_node}}
                </span>
            
            <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.parent_node_info)"
               class="magnify-icon edit-info-doc-btn"
               data-node-type="-1" data-location="-1" data-node="parent-node" data-node-description="{{output.parent_node}}" data-readonly="1">
                <span class="icon icon-tt-zoom-in"></span>
            </a>
                <%-- Infodoc Heading --%>

                <%-- if undefined show default --%>
                <%--<span 
                    ng-class="{'editable-trigger' : is_AT_owner == 1, 'white':output.page_type =='atPairwise', 'black':output.page_type !='atPairwise'}" ng-if="info_docs_headings[0] == '-1'"  id="0" class="hide-when-editing-0 text  et-0">
                    {{output.parent_node}}
                </span>


                <span ng-class="{'editable-trigger' : is_AT_owner == 1}" ng-if="info_docs_headings[0] == '-2'" id="0" class="hide-when-editing-0 text et-0" style="color:#000;" >
                   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                </span>

                <span ng-class="{'editable-trigger' : is_AT_owner == 1}" ng-if="info_docs_headings[0] != '-1' && info_docs_headings[0] != '-2'" id="0" class="hide-when-editing-0 text et-0 black" >
                    {{info_docs_headings[0]}}
                </span>--%>

                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-0 hide">
                    <div class="small-12 columns">
                        <input ng-model="parent_heading" id="parent_heading" name="" placeholder="Add something here" type="text" class="text editable-input ei-0 parent" />
                    </div>
                    <div class="small-12 columns">
                        <a id="0" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a ng-click="update_infodoc_params('parent-node', output.ParentNodeGUID, '', 'parent_heading')" id="0" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                <!-- end editing -->
                <%-- end of info doc heading --%>

           


<%--            <div ng-if="output.page_type != 'atAllPairwise' && output.page_type == 'atPairwise' && output.page_type != 'atNonPWAllChildren' && output.page_type != 'atNonPWAllCovObjs' && output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults' && output.show_comments" class="mobile-comment-btn right">
                <a href="#" data-reveal-id="tt-c-modal" class="toggleComments smoothScrollLink" data-link="#comWrap" ng-style="{color:output.comment != '' && output.comment != null ? '#008CBA' : '#b0d5ee'}" ><span class="icon-tt-comments  icon-30"></span></a>
            </div>--%>

        </div>

        <div
            ng-class="{
                            'hide': !output.showinfodocnode || collapsed_info_docs[0] || (is_html_empty(output.parent_node_info) && is_AT_owner != 1)
                    }"
            class="parent-node-info-div columns tt-accordion-content tg-accordion-0  tg-accordion-sub-0">
            <div data-index="0" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-0 small-centered columns box-0">   
                 <a 
                data-node-type="-1" 
                data-location="-1" 
                data-node="parent-node" 
                data-node-description="{{output.parent_node}}" 
                ng-class="{
                        'edit-info-doc-btn edit-pencil ep ep-0 ep-sub-0' : output.page_type=='atPairwise' , 
                        'edit-info-doc-btn edit-pencil ep-0 ep-sub-0': output.page_type !='atPairwise',
                        'hide': is_AT_owner != 1 || !output.showinfodocnode ||  collapsed_info_docs[0]   
                }"
                title="" 
                href="#"
                >
                <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil">
                </span> 
            </a>              
                <div id="ParentNodeInfo" class="parent-node-info-text  " ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(output.parent_node_info)">               
                </div>
            </div>
        </div>
        <%--<div class="spacer columns"></div>--%>
    </div>
</span>
<span ng-if="is_teamtime">
    <div ng-if="!is_infodoc_tooltip">
        <div
            ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.ParentNodeInfo))}"
            class="columns tt-accordion-head" ng-class="{'text-left':current_action_type == 'pairwise'}">
            <a class="tt-toggler" ng-click="update_infodocs(0, $event)" id="0" data-toggler="tg-accordion-sub">
                <span class="icon-tt-plus-square icon icon-desktop" data-node="0"></span>
            <span class="text parent-lbl parent-node">{{information_nodes.ParentNode}}</span>
            </a> 
        </div> 

        <div class="columns tt-accordion-content tg-accordion-0 tg-accordion-sub-0 hide">
            <div data-index="0" class="tt-panel tt-resizable-panel tt-resizable-panel-0 box-0 small-centered columns">   
                <!-- Edit Icon -->
                <a ng-if="output.isPM" data-node-type="-1" data-location="-1" data-node="parent-node" data-node-description="{{information_nodes.ParentNode}}"  class="edit-pencil ep-0 ep-sub-0 edit-info-doc-btn" title="" href="#">
                <span class="icon-tt-pencil"></span>
                </a>
                <!-- End Edit Icon -->
                <div id="ParentNodeInfo" class="parent-node-info-text" ng-bind-html="getHtml(output.pipeData.ParentNodeInfo)">       
                </div>
            </div>
        </div>
    </div>
    <div 
        ng-if="is_infodoc_tooltip  && output.showinfodocnode"
        class="tooltip-mode"
         ng-class="{
            'text-center ': current_action_type != 'pairwise',
            'hide' : !output.isPM &&  is_html_empty(output.pipeData.ParentNodeInfo) && current_action_type != 'pairwise'
        }"
        >
        <span>
            <a  
                data-dropdown="gdrop_parent_node" 
                aria-controls="gdrop_parent_node" 
                aria-expanded="false"
                class=""
                >
                    <span 
                        ng-if="output.isPM || (!output.isPM && !is_html_empty(output.pipeData.ParentNodeInfo))"
                        ng-class="is_html_empty(output.pipeData.ParentNodeInfo) ? 'disabled' : 'not-disabled'" class="icon icon-tt-info-circle"></span>
            </a>
           {{information_nodes.ParentNode}}
        </span>
        <div
             ng-class="{'medium' : has_long_content(output.pipeData.ParentNodeInfo)}"
             id="gdrop_parent_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                        {{information_nodes.ParentNode}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a  
                                    style="color: #008CBA;"
                                    ng-class="output.isPM ? '' : 'hide'" 
                                    class="edit-info-doc-btn" 
                                    data-node-type="-1" 
                                    data-location="-1"  
                                    data-node="parent-node" 
                                    data-node-description=" {{information_nodes.ParentNode}}"
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
                <div class="infotext-wrap large-12 columns parent-node-info-text" ng-bind-html="getHtml(output.pipeData.ParentNodeInfo)"></div>
            </div>
        </div>
    </div> 
</span>