<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WRTLeftNodeInfoDoc.ascx.cs" Inherits="AnytimeComparion.Pages.includes.WRTLeftNodeInfoDoc" %>
<%--{{{{output.first_node}} {{output.child_node}} WRT {{output.parent_node}++++++++++++++++++++++++++++++++++++++++++++++++}}}--%>

<span ng-if="is_anytime">
    <div ng-if="output.is_infodoc_tooltip && output.showinfodocnode"  class="tooltip-mode" 
        ng-class="{
        'text-center ' : output.page_type!='atPairwise',
        'hide' : is_AT_owner == 0 &&  is_html_empty(output.wrt_first_node_info)
        }">
        <span>
            <a data-dropdown="gdrop_wrt_left_node" data-options="is_hover:true; hover_timeout:1000" 
                aria-controls="gdrop_wrt_left_node" aria-expanded="false" data-node-type="3"
               data-location="1" data-node="wrt-left-node" data-node-title="{{output.first_node}}{{output.child_node}}" 
               data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                class="wrt-left-node-tooltip tooltip-infodoc-btn">
                    <span ng-if="is_AT_owner || (!is_AT_owner && !is_html_empty(output.wrt_first_node_info))" 
                          ng-class="is_html_empty(output.wrt_first_node_info) ? 'disabled' : 'not-disabled'" 
                          class="icon icon-tt-w-font"></span>
            </a>
<%--            <span ng-if="output.page_type!='atPairwise'">{{output.first_node}} {{output.child_node}} WRT {{output.parent_node}}</span>--%>
            <span>  {{get_wrt_text('left')}} </span>
        </span>
         <%-- show dropdown only in non pw since dropdown of PW are in Pairwise.ascx files --%>
         <div 
             ng-class="{'medium' : has_long_content(output.wrt_first_node_info)}"
             ng-if="output.page_type!='atPairwise'" id="gdrop_wrt_left_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                       {{get_wrt_text('left')}}
                    </div>

                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                                    data-node-type="3" data-location="1" data-node="wrt-left-node" 
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
                <div class="infotext-wrap large-12 columns wrt-left-node-info-text" ng-bind-html="getHtml(output.wrt_first_node_info)"></div>
            </div>
        </div>
    </div>
  
    <div 
        ng-if="!output.is_infodoc_tooltip && output.showinfodocnode"
        ng-class="{'hide' : (is_AT_owner == 0 &&  is_html_empty(output.wrt_first_node_info))}" class="columns tt-accordion-sub">
        <div class="columns tt-accordion-head {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients">
        
        
                <a ng-click="set_collapse_cookies('wrt-left-node'); update_infodoc_params('wrt-left-node', output.LeftNodeGUID, output.ParentNodeGUID)"
                    <%--ng-click="set_collapse_cookies('wrt-left-node')" --%>
                    ng-class="{
                        'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0),
                                            'left' :output.page_type =='atPairwise',
                            'center': output.page_type !='atPairwise'
                    } "
                    class="hide-when-editing-3 tt-toggler-3" id="3"  data-toggler="tg-accordion-sub">
                    <span 
                        ng-class="{
                            'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[3] ,
                            'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[3],
    
                        }"
                        class="icon icon-desktop" data-node="3"> </span>
                </a>
        
                   
                <span id="3" class="hide-when-editing-3 text et-3 black" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}"> {{output.hideInfoDocCaptions ? "" : get_wrt_text('left')}} </span>        

            <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.wrt_first_node_info)"
               class="magnify-icon edit-info-doc-btn"
               data-node-type="3" data-location="1" data-node="wrt-left-node" data-node-description="{{output.parent_node}}"
               data-node-title="{{output.first_node}}{{output.child_node}}" data-readonly="1">
                <span class="icon icon-tt-zoom-in"></span>
            </a>
                   <%-- <span 
                        ng-class="{'editable-trigger' : is_AT_owner == 1, 'white':output.page_type =='atPairwise', 'black':output.page_type !='atPairwise'}"  ng-if="info_docs_headings[3] == '-1'" id="3" class="hide-when-editing-3 text  et-3" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}"> {{get_wrt_text('left')}} </span>
                
                    <%-- if blank --%>
                 <%--   <span ng-class="{'editable-trigger' : is_AT_owner == 1}"  ng-if="info_docs_headings[3] == '-2'" id="3" class="hide-when-editing-3 text  et-3" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}" style="color:white;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>--%>

                   <%-- if has value --%>
                  <%--  <span 
                        ng-class="{'editable-trigger' : is_AT_owner == 1}"  ng-if="info_docs_headings[3] != '-1' && info_docs_headings[3] != '-2'" id="3" class="hide-when-editing-3 text  et-3 black" ng-class="{'wrt-node pw' : output.page_type == 'atPairwise'}">
                        {{info_docs_headings[3]}}
                    </span>--%>

                    <!-- start editing -->
                    <div class="row collapse editable-input-wrap eiw-3">
                        <div class="small-12 columns">
                            <input ng-model="WRT_left_heading" id="WRT_left_heading" name="" placeholder="Add something here" type="text" class="text editable-input ei-3" />
                        </div>
                        <div class="small-12 columns">
                          <a id="3" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                          <a ng-click="update_infodoc_params('wrt-left-node', output.LeftNodeGUID, output.ParentNodeGUID, 'WRT_left_heading')" id="3" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                        </div>
                    </div>
                    <!-- end editing -->
                    <%-- end of info doc heading --%>
    
                    
        </div>
        <div 
                ng-class="{
                'hide' : !output.showinfodocnode || collapsed_info_docs[3] || (is_AT_owner != 1 && is_html_empty(output.wrt_first_node_info)) }"
            class="wrt-left-node-info-div columns tt-accordion-content tg-accordion-sub-3">
            <div 
                ng-class="{'tt-resizable-panel-1': output.page_type != 'atPairwise' && !is_multi, 'tt-resizable-panel-2' : output.page_type == 'atPairwise' || is_multi}"
                data-index="2" class="tt-panel-temp tt-panel tt-resizable-panel  small-centered columns box-3">
                <a  ng-class="{
                        'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[3],
                        'right' : output.page_type == 'atPairwise'                        
                        }"
                        data-node-type="3"
                        data-location="1" 
                        data-node="wrt-left-node" 
                        data-node-title="{{output.first_node}}{{output.child_node}}" 
                        data-node-description="{{output.parent_node}}"
                        class=" edit-info-doc-btn edit-pencil ep-sub-3 edit-info-doc-btn" 
                        title=""
                        href="#">
                        <span  ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>                        
                    </a>
                <div class="wrt-left-node-info-text nodes-wrap" ng-bind-html="getHtml(output.wrt_first_node_info)" >
                </div>
            </div>
        </div>
    </div>
</span>
<span ng-if="is_teamtime">
    <div 
        ng-if="!is_infodoc_tooltip && output.showinfodocnode"
         ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.WrtLeftNodeInfo))}"
        class="columns">
        <div class="columns tt-accordion-head tg-gradients" ng-class="main_gradient_checkbox ? '' : 'no-gradient'">
            <a 
                ng-class="{'tt-toggler' :(!output.isPM && !is_html_empty(output.pipeData.WrtLeftNodeInfo)) || output.isPM }"
                 ng-click="update_infodocs(3, $event)" id="3" data-toggler="tg-accordion-sub">
                <span 
                    ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.WrtLeftNodeInfo))}"
                    class="icon-tt-plus-square icon icon-desktop" data-node="3"></span>
                <span class="text">
                    WRT 
                </span>
            </a>                      
        </div>
        <div class="columns tt-accordion-content tg-accordion-sub-3 hide">
            <div data-index="3" class="tt-panel tt-resizable-panel tt-resizable-panel-3 box-3 small-centered columns"> 
                <!-- Edit Icon -->
                <a 
                ng-if="output.isPM"
                data-node-title="{{information_nodes.LeftNode}}" 
                data-node-description="{{information_nodes.ParentNode}}"
                    data-node-type="3" data-location="1" data-node="wrt-left-node" class="edit-pencil ep-sub-3 edit-info-doc-btn" title="" href="#"><span class="icon-tt-pencil"></span></a>                 
                <!-- End Edit Icon -->
                <div class="wrt-left-node-info-text nodes-wrap" id="WrtLeftNode" ng-bind-html="getHtml(output.pipeData.WrtLeftNodeInfo)">
                </div>
            </div>
        </div>
    </div>
    
    <div ng-if="is_infodoc_tooltip  && output.showinfodocnode"  class="tooltip-mode text-center" 
        ng-class="{
            '' : current_action_type!='pairwise',
            'hide': !output.isPM &&  is_html_empty(output.pipeData.WrtLeftNodeInfo)
        }">
        <span>
            <a  
                data-dropdown="gdrop_wrt_left_node" 
                aria-controls="gdrop_wrt_left_node" 
                aria-expanded="false"
                class=""
                >
                    <span 
                        ng-if="output.isPM || (!output.isPM && !is_html_empty(output.pipeData.WrtLeftNodeInfo))"
                        ng-class="is_html_empty(output.pipeData.WrtLeftNodeInfo) ? 'disabled' : 'not-disabled'" class="icon icon-tt-info-circle"></span>
            </a>
            <span ng-if="current_action_type!='pairwise'">{{information_nodes.LeftNode}} WRT {{information_nodes.ParentNode}}</span>
            <span ng-if="current_action_type=='pairwise'"> WRT </span>
        </span>
         <%-- show dropdown only in non pw since dropdown of PW are in Pairwise.ascx files --%>
         <div 
             ng-class="{'medium' : has_long_content(output.pipeData.WrtLeftNodeInfo)}"
             id="gdrop_wrt_left_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
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
                                    data-location="1" 
                                    data-node="wrt-left-node" 
                                    data-node-title="{{information_nodes.LeftNode}}" 
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
                <div class="infotext-wrap large-12 columns wrt-left-node-info-text" ng-bind-html="getHtml(output.pipeData.WrtLeftNodeInfo)"></div>
            </div>
        </div>
    </div>
</span>
