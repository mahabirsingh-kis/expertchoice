<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FramedInfodDocs.ascx.cs" Inherits="AnytimeComparion.Pages.includes.FramedInfodDocs" %>
<%--this is a test--%>
 

<%--{{output.multi_non_pw_data[active_multi_index] }}--%>

<%--{{output.multi_pw_data[active_multi_index]}}--%>

<%--{{multi_collapsed_info_docs}}<br />
{{active_multi_index}}
<br />
{{output.multi_GUIDs[active_multi_index][1]}}<br />
{{output.multi_GUIDs[active_multi_index][2]}}<br />
{{output.multi_infodoc_params[active_multi_index]}}--%>

<div style="margin-bottom:5px;" class="InfoDocsParentDiv">

    <span 
        ng-class="{'hide': is_html_empty(output.parent_node_info) && is_AT_owner==0}"
        ng-if="output.is_infodoc_tooltip && output.showinfodocnode">
        <div class="row editable-content">
            <div class="large-4 columns large-centered">
                <div class="text-center">
                  
                     <a data-dropdown="gdrop0" data-options="is_hover:true; hover_timeout:1000" aria-controls="gdrop0" 
                        aria-expanded="false" data-node-type="-1" data-location="-1" data-node="parent-node" 
                        data-node-description="{{output.parent_node}}" data-readonly="{{is_AT_owner == 0 ? 1 : 0}}"
                        class="parent-node-tooltip tooltip-infodoc-btn" id="parent_tooltip_trigger">
                       <span ng-class="{'disabled': is_html_empty(output.parent_node_info)}"
                           style="font-size: 14px;" class="icon icon-tt-info-circle small"></span>
                    </a>
                    <span id="0" class="text parent-lbl parent-node hide-when-editing-0 text editable-trigger143 et-0 small"> {{output.parent_node}}    </span>
                
                </div>
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-0">
                    <div class="small-12 columns">
                        <input ng-model="WRT_parent_heading" ng-init="WRT_parent_heading = output.parent_node" name="" placeholder="Add something here" type="text" class="text editable-input ei-0" />
                    </div>
                    <div class="small-12 columns">
                        <a id="0" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a  id="0" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                 <!-- end editing -->
                
            </div>
        </div>
    </span>
    <span ng-if="(!output.is_infodoc_tooltip || (is_AT_owner == 0 && output.framed_info_docs)) && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes') && output.showinfodocnode">
        <div class="row editable-content" ng-class="{'editable-content-height': isMobile() && output.framed_info_docs}">

            <div 
                ng-if="is_html_empty(multi_data[active_multi_index].InfodocLeft) && is_AT_owner==0"
                class="columns large-3 hide-for-medium-down">
                &nbsp;
            </div>
            <div ng-if="is_AT_owner == 0 && is_html_empty(output.parent_node_info)" class="columns large-2 hide-for-medium-down">
                &nbsp;
            </div>
            <div ng-if="!isMobile()"
                ng-class="{
                    'hide': (is_html_empty(multi_data[active_multi_index].InfodocLeft) && is_AT_owner==0), 
                    'large-3': is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-4': is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
                class="columns ">
                <div class="text-center">
                    <a 
                        ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocLeft))}"
                        ng-click="set_collapse_cookies('left-node'); update_infodoc_params('left-node', output.ParentNodeGUID, '')"
                        class=" tt-toggler-1" id="1"  data-toggler="tg-accordion-sub">
                        <span
                            ng-class="{
                                <%--'hide' : (is_AT_owner == 0 && is_html_empty(multi_data[active_multi_index].InfodocLeft)),--%>
                                'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[1],
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[1]
                            }"
                                class="icon icon-desktop" data-node="1"></span>
                    </a>
                    <span id="1" class="left-node hide-when-editing-1 text editable-trigger143 et-1 small" style="font-size: 12px;"> {{output.hideInfoDocCaptions ? "" : multi_data[active_multi_index].LeftNode}}    </span>
                    
                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocLeft)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-type="2" data-location="1" data-node-id="{{ multi_data[active_multi_index].NodeID_Left}}" 
                       data-node="left-node" data-node-description="{{multi_data[active_multi_index].LeftNode}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
            
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-1">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading" ng-init="WRT_left_heading = multi_data[active_multi_index].LeftNode"  name="" placeholder="Add something here" type="text" class="text editable-input ei-1" />
                    </div>
                    <div class="small-12 columns">
                        <a id="1" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a  id="1" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                 <!-- end editing -->            
                <div 
                    ng-class="{
                                    'hide': !output.showinfodocnode || collapsed_info_docs[1] || (is_html_empty(multi_data[active_multi_index].InfodocLeft) && is_AT_owner == 0 )
                            }"
                    class="left-node-info-div tt-accordion-content tg-accordion-1  tg-accordion-sub-1">
                    <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-1" data-box-index="1" style="max-height: 90px;">  
                        <a  
                        data-index="{{active_multi_index}}"
                        ng-class="{
                                'hide': is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[1] 
                        }"
                        data-node-type="2" 
                        data-location="1" 
                        data-node-id="{{ multi_data[active_multi_index].NodeID_Left}}" 
                        data-node="left-node" 
                        data-node-description="{{multi_data[active_multi_index].LeftNode}}" 
                        class="edit-info-doc-btn edit-pencil ep ep-sub-1"
                        title=""
                        href="#"
                        >
                        <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                    </a>               
                        <div  class="left-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(multi_data[active_multi_index].InfodocLeft)">                
                        </div>
                    </div>
                </div>
            </div>

            <div 
                ng-class="{
                    'hide': (is_html_empty(output.parent_node_info) && is_AT_owner == 0),
                    'large-6': is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-2': is_AT_owner == 0 && is_html_empty(output.parent_node_info)
                }"
                class="columns original-info-doc">
                <div class="text-center">
                    <a 
                       ng-click="set_collapse_cookies('parent-node'); update_infodoc_params('parent-node', output.ParentNodeGUID, '')"
                        ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(output.parent_node_info))}"
                        class="tt-toggler-0" id="0" data-toggler="tg-accordion">
                        <span 
                            ng-class="{
                                         'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[0],
                                        'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[0]
                                    }"
                            class="icon icon-desktop" data-node="0"></span>
                    </a>

                    <span id="0" class="text parent-lbl parent-node hide-when-editing-0 text editable-trigger143 et-0 small" style="font-size: 12px;"> {{output.hideInfoDocCaptions ? "" : output.parent_node}}    </span>                
                    
                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.parent_node_info)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-type="-1" data-location="-1" 
                       data-node="parent-node" data-node-description="{{output.parent_node}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-0">
                    <div class="small-12 columns">
                        <input ng-model="WRT_parent_heading" ng-init="WRT_parent_heading = output.parent_node"  name="" placeholder="Add something here" type="text" class="text editable-input ei-0" />
                    </div>
                    <div class="small-12 columns">
                        <a id="0" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a   id="0" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                 <!-- end editing -->
                <div 
                    ng-class="{
                                    'hide': !output.showinfodocnode ||  collapsed_info_docs[0] || (is_html_empty(output.parent_node_info) && is_AT_owner == 0), 'infodoc-height-mobile': isMobile()
                            }"
                    class="parent-node-info-div  tt-accordion-content tg-accordion-0  tg-accordion-sub-0">
                    <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-0 tt-resizable-panel-1 small-centered columns box-0" data-box-index="0">
                        <a 
                        data-index="{{active_multi_index}}"
                        data-node-type="-1" 
                        data-location="-1" 
                        data-node="parent-node" 
                        data-node-description="{{output.parent_node}}" 
                        class="edit-info-doc-btn edit-pencil ep ep-0 ep-sub-0"
                        ng-class="{
                                    'hide': is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[0]     
                            }"
                        title="" 
                        href="#"
                        >
                        <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil">
                    </span></a>
                        <div class="parent-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(output.parent_node_info)">               
                        </div>
                    </div>
                </div>
            </div>
            <div ng-if="!isMobile()"
                ng-class="{
                    'hide': (is_html_empty(multi_data[active_multi_index].InfodocRight) && is_AT_owner==0),
                    'large-3' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-4':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                } "
                class="large-3 columns ">
                <div class="text-center">
                     <a ng-click="set_collapse_cookies('right-node'); update_infodoc_params('right-node', output.ParentNodeGUID, '')" 
                        ng-class="{
                            'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocRight))
                         }"
                        class=" tt-toggler-2" id="2" data-toggler="tg-accordion-sub">
                        <span 
                                ng-class="{
                                <%--'hide' : (is_AT_owner == 0 && is_html_empty(multi_data[active_multi_index].InfodocRight)),--%>
                                'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[2],
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[2]
                            }"
                            class="icon icon-desktop" data-node="2" ></span>
                    </a>

                    <span id="2" class="right-node hide-when-editing-2 text editable-trigger143 et-2 small" style="font-size: 12px;">    {{multi_data[active_multi_index].RightNode}}    </span>
                    
                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocRight)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-type="2" data-node-id="{{ multi_data[active_multi_index].NodeID_Right}}" 
                       data-node-description="{{multi_data[active_multi_index].RightNode}}" data-location="2" data-node="right-node" 
                       data-node-title="{{multi_data[active_multi_index].RightNode}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-2">
                    <div class="small-12 columns">
                        <input ng-model="WRT_right_heading" ng-init="WRT_right_heading = multi_data[active_multi_index].RightNode"  name="" placeholder="Add something here" type="text" class="text editable-input ei-2" />
                    </div>
                    <div class="small-12 columns">
                        <a id="2" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a   id="2" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>

                <div 
                    ng-class="{
                            'hide': !output.showinfodocnode || collapsed_info_docs[2] ||( is_html_empty(multi_data[active_multi_index].InfodocRight) && is_AT_owner == 0)          
                        }"
                    class="right-node-info-div  tt-accordion-content tg-accordion-2  tg-accordion-sub-2">
                    <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-2" data-box-index="2" style="max-height: 90px;">
                        <a 
                        data-index="{{active_multi_index}}"
                        ng-class="{
                                'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[2] }"
                        data-node-type="2" 
                        data-location="2" 
                        data-node="right-node" 
                        data-node-id="{{ multi_data[active_multi_index].NodeID_Right}}" 
                        data-node-description="{{multi_data[active_multi_index].RightNode}}" 
                        data-node-title="{{multi_data[active_multi_index].RightNode}}"  
                        class="edit-info-doc-btn edit-pencil ep ep-sub-2" 
                        title="" 
                        href="#">
                        <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                    </a>                 
                        <div  class="right-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml( multi_data[active_multi_index].InfodocRight)">               
                        </div>
                    </div>
                </div>
            </div>

            <div ng-if="isMobile() && output.framed_info_docs">
                      <div class="row editable-content mobile-frame">
                        <div 
                            ng-if="is_AT_owner == 0 && is_html_empty(multi_data[active_multi_index].InfodocLeft)"
                            class="columns small-6">
                            &nbsp;
                        </div>
                        <div 
                            ng-class="{
                                'hide': (is_html_empty(multi_data[active_multi_index].InfodocLeft) && is_AT_owner==0), 
                            }"
                            class="columns small-6">
                            <div class="text-center">
                                <a 
                                    ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocLeft))}"
                                    ng-click="set_collapse_cookies('left-node'); update_infodoc_params('left-node', output.ParentNodeGUID, '')"
                                    class=" tt-toggler-1" id="1"  data-toggler="tg-accordion-sub">
                                    <span
                                        ng-class="{
                                            <%--'hide' : (is_AT_owner == 0 && is_html_empty(multi_data[active_multi_index].InfodocLeft)),--%>
                                            'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[1],
                                            'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[1]
                                        }"
                                            class="icon icon-desktop" data-node="1"></span>
                                </a>
                                <span id="1" class="left-node hide-when-editing-1 text editable-trigger143 et-1 small" style="font-size: 12px;"> {{output.hideInfoDocCaptions ? "" : multi_data[active_multi_index].LeftNode}}    </span>
                                
                            </div>
                        
                            <!-- start editing -->
                            <div class="row collapse editable-input-wrap eiw-1">
                                <div class="small-12 columns">
                                    <input ng-model="WRT_left_heading" ng-init="WRT_left_heading = multi_data[active_multi_index].LeftNode"  name="" placeholder="Add something here" type="text" class="text editable-input ei-1" />
                                </div>
                                <div class="small-12 columns">
                                    <a id="1" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                                    <a  id="1" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                                </div>
                            </div>
                             <!-- end editing -->            
                            <div 
                                ng-class="{
                                                'hide': !output.showinfodocnode || collapsed_info_docs[1] || (is_html_empty(multi_data[active_multi_index].InfodocLeft) && is_AT_owner == 0 )
                                        }"
                                class="left-node-info-div tt-accordion-content tg-accordion-1  tg-accordion-sub-1">
                                <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-1" data-box-index="1" style="max-height: 90px;">  
                                    <a  
                                    data-index="{{active_multi_index}}"
                                    ng-class="{
                                            'hide': is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[1] 
                                    }"
                                    data-node-type="2" 
                                    data-location="1" 
                                    data-node-id="{{ multi_data[active_multi_index].NodeID_Left}}" 
                                    data-node="left-node" 
                                    data-node-description="{{multi_data[active_multi_index].LeftNode}}" 
                                    class="edit-info-doc-btn edit-pencil ep ep-sub-1"
                                    title=""
                                    href="#"
                                    >
                                    <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                                </a>               
                                    <div  class="left-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(multi_data[active_multi_index].InfodocLeft)">                
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div 
                            ng-class="{
                                'hide': (is_html_empty(multi_data[active_multi_index].InfodocRight) && is_AT_owner==0)
                                
                            } "
                            class="small-6 columns ">
                            <div class="text-center">
                                 <a 
                                    ng-click="set_collapse_cookies('right-node'); update_infodoc_params('right-node', output.ParentNodeGUID, '')" 
                                    ng-class="{
                                        'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocRight))
                                     }"
                                    class=" tt-toggler-2" id="2" data-toggler="tg-accordion-sub">
                                    <span 
                                            ng-class="{
                                            <%--'hide' : (is_AT_owner == 0 && is_html_empty(multi_data[active_multi_index].InfodocRight)),--%>
                                            'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[2],
                                            'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[2]
                                        }"
                                        class="icon icon-desktop" data-node="2" ></span>
                                </a>

                                <span id="2" class="right-node hide-when-editing-2 text editable-trigger143 et-2 small" style="font-size: 12px;">    {{multi_data[active_multi_index].RightNode}}    </span>                    
                            </div>
                            <!-- start editing -->
                            <div class="row collapse editable-input-wrap eiw-2">
                                <div class="small-12 columns">
                                    <input ng-model="WRT_right_heading" ng-init="WRT_right_heading = multi_data[active_multi_index].RightNode"  name="" placeholder="Add something here" type="text" class="text editable-input ei-2" />
                                </div>
                                <div class="small-12 columns">
                                    <a id="2" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                                    <a   id="2" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                                </div>
                            </div>

                            <div 
                                ng-class="{
                                        'hide': !output.showinfodocnode || collapsed_info_docs[2] ||( is_html_empty(multi_data[active_multi_index].InfodocRight) && is_AT_owner == 0)          
                                    }"
                                class="right-node-info-div  tt-accordion-content tg-accordion-2  tg-accordion-sub-2">
                                <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-2" data-box-index="2"  style="max-height: 90px;">
                                    <a 
                                    data-index="{{active_multi_index}}"
                                    ng-class="{
                                            'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[2] }"
                                    data-node-type="2" 
                                    data-location="2" 
                                    data-node="right-node" 
                                    data-node-id="{{ multi_data[active_multi_index].NodeID_Right}}" 
                                    data-node-description="{{multi_data[active_multi_index].RightNode}}" 
                                    class="edit-info-doc-btn edit-pencil ep ep-sub-2" 
                                    title="" 
                                    href="#">
                                    <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                                </a>                 
                                    <div  class="right-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml( multi_data[active_multi_index].InfodocRight)">               
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div ng-if="is_AT_owner == 0 && is_html_empty(multi_data[active_multi_index].InfodocRight)"
                            class="columns small-6">
                            &nbsp;
                        </div>
                    </div>

                </div>
        
            <div ng-if="is_AT_owner == 0 && is_html_empty(output.parent_node_info)" class="columns large-2 hide-for-medium-down">
                &nbsp;
            </div>
        </div>

        <div class="row editable-content">
            <div ng-if="is_AT_owner == 0 && is_html_empty(output.parent_node_info)" class="columns large-2 hide-for-medium-down">
                &nbsp;
            </div>
            <div 
                ng-class="{
                    'hide': (is_html_empty(multi_data[active_multi_index].InfodocLeftWRT) && is_AT_owner==0) || isMobile(),
                    'large-3' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-4':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
                class="columns ">
                <div class="text-center">
                    <a ng-click="set_collapse_cookies('wrt-left-node'); update_infodoc_params('wrt-left-node',  output.ParentNodeGUID, output.ParentNodeGUID)"  
                        ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocLeftWRT))}"
                        class="tt-toggler-3" id="3" data-toggler="tg-accordion-sub" >
                        <span 
                            ng-class="{
                                'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[3],
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[3]
                            }"
                            class="icon icon-desktop" data-node="3"></span>
                        
                    </a>
                    <span id="3" class="wrt-node pw hide-when-editing-2 text editable-trigger143 et-3 small" style="font-size: 12px;">
                        {{output.hideInfoDocCaptions ? "" : get_wrt_text('left')}}
                    </span>                     
                    
                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocLeftWRT)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-type="3" data-node-id="{{ multi_data[active_multi_index].NodeID_Left}}"
                       data-location="1" data-node="wrt-left-node" data-node-title="{{multi_data[active_multi_index].LeftNode}}" 
                       data-node-description="{{output.parent_node}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-3">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading" name="" placeholder="Add something here" type="text" class="text editable-input ei-3" />
                    </div>
                    <div class="small-12 columns">
                        <a id="3" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a  id="3" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
           
                <div 
                    ng-class="{
                                    'hide': !output.showinfodocnode || collapsed_info_docs[3] || (is_html_empty(multi_data[active_multi_index].InfodocLeftWRT) && is_AT_owner == 0)                   
                            }"
                    class="wrt-left-node-info-div tt-accordion-content tg-accordion-3  tg-accordion-sub-3">
                    <div data-index="2" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-2 small-centered columns box-3" data-box-index="3" style="max-height: 90px;"> 
                        <a  
                         data-index="{{active_multi_index}}"
                        ng-class="{
                        'hide' :  is_AT_owner != 1 ||  !output.showinfodocnode || collapsed_info_docs[3] }"
                        data-node-type="3" 
                        data-location="1"
                        data-node="wrt-left-node" 
                        data-node-id="{{ multi_data[active_multi_index].NodeID_Left}}" 
                        data-node-title="{{multi_data[active_multi_index].LeftNode}}" 
                        data-node-description="{{output.parent_node}}" 
                        class ="edit-info-doc-btn edit-pencil ep ep-sub-3" 
                        title="" 
                        href="#">
                            <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                    </a>                
                        <div  class="wrt-left-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(multi_data[active_multi_index].InfodocLeftWRT)">               
                        </div>
                    </div>
                </div>
            </div>
            <div 
                ng-class="{
                    'large-6' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-2':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
                class="columns">
                 <div  class="tt-accordion-content tg-accordion-0  tg-accordion-sub-0">
                </div>
            </div>
            <div 
                ng-class="{
                    'hide': (is_html_empty(multi_data[active_multi_index].InfodocRightWRT) && is_AT_owner==0) || isMobile(),
                    'large-3' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-4':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
                class="columns large-3 ">
                 <div class="text-center">
                    <a ng-click="set_collapse_cookies('wrt-right-node'); update_infodoc_params('wrt-right-node',  output.ParentNodeGUID, output.ParentNodeGUID)" 
                        ng-class="{
                            'tt-toggler': is_AT_owner == 1 ||  (is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocRightWRT))
                        }"
                        class="tt-toggler-4" id="4"  data-toggler="tg-accordion-sub">
                        <span 
                            ng-class="{
                                'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[4] ,
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[4]
                            }"
                            class="icon icon-desktop" data-node="4"></span>
                       
                    </a>
                    <span id="4" class="wrt-node pw hide-when-editing-4 text editable-trigger143 et-4 small" style="font-size: 12px;">
                        {{output.hideInfoDocCaptions ? "" : get_wrt_text('right')}}
                    </span>                    
                     
                     <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(multi_data[active_multi_index].InfodocRightWRT)"
                        class="magnify-icon edit-info-doc-btn"
                        data-index="{{active_multi_index}}" data-node-type="3" data-node-id="{{ multi_data[active_multi_index].NodeID_Right}}" 
                        data-location="2" data-node="wrt-right-node" data-node-title="{{multi_data[active_multi_index].RightNode}}" 
                        data-node-description="{{output.parent_node}}" data-readonly="1">
                         <span class="icon icon-tt-zoom-in"></span>
                     </a>
                </div>
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-4">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading" name="" placeholder="Add something here" type="text" class="text editable-input ei-4" />
                    </div>
                    <div class="small-12 columns">
                        <a id="4" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a  id="4" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
               
                <div 
                    ng-class="{
                                    'hide': !output.showinfodocnode || collapsed_info_docs[4] || (is_html_empty(multi_data[active_multi_index].InfodocRightWRT) && is_AT_owner == 0)         
                            }"
                    class="wrt-right-node-info-div tt-accordion-content tg-accordion-4  tg-accordion-sub-4">
                    <div data-index="2" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-2 small-centered columns box-4" data-box-index="4" style="max-height: 90px;">     
                        <a 
                        data-index="{{active_multi_index}}"
                        ng-class="{
                        'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[4] }"
                        data-node-type="3"
                        data-location="2" 
                        data-node-id="{{ multi_data[active_multi_index].NodeID_Right}}" 
                        data-node="wrt-right-node" 
                        data-node-title="{{multi_data[active_multi_index].RightNode}}" 
                        data-node-description="{{output.parent_node}}"
                        class="edit-info-doc-btn edit-pencil ep ep-sub-4" 
                        title=""
                        href="#">
                            <span  
                                ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                    </a>            
                        <div  class="wrt-right-node-info-text  "  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(multi_data[active_multi_index].InfodocRightWRT)">               
                        </div>
                    </div>
                </div>
            </div>
            <div ng-if="is_AT_owner == 0 && is_html_empty(output.parent_node_info)" class="columns large-2 hide-for-medium-down">
                &nbsp;
            </div>
        </div>
    </span>

    <span ng-if="!output.is_infodoc_tooltip && (output.page_type == 'atNonPWAllChildren' ||output.page_type == 'atNonPWAllCovObjs') && output.showinfodocnode">
        <div class="row editable-content">
            <div 
                ng-if="is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc) && is_AT_owner==0"
                class="columns large-4">
                &nbsp;
            </div>
            <div 
                ng-class="{
                    'hide': is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc) && is_AT_owner==0,
                    'large-4' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-6':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
            
                class="columns hide-for-medium-down">
                <div class="text-center">
                    <a 
                        ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc))}"
                        ng-click="set_collapse_cookies('left-node'); update_infodoc_params('left-node', output.ParentNodeGUID, '')"  class=" tt-toggler-1" id="1"  data-toggler="tg-accordion-sub">
                        <span
                            ng-class="{
                                'hide' : (is_AT_owner == 0 && (is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc) && is_AT_owner == 0)),
                                'icon-tt-minus-square' : (output.showinfodocnode ) && !collapsed_info_docs[1],
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[1]
                            }"
                                class="icon icon-desktop" data-node="1">  </span>
              
                    </a>
                    <span 
                        ng-class="{'hide':(is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc) && is_AT_owner == 0) }"
                        style="font-size: 12px;" id="1" class="left-node hide-when-editing-1 text editable-trigger143 et-1 small"> {{output.hideInfoDocCaptions ? "" : output.multi_non_pw_data[active_multi_index].Title}}     </span>                    

                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-type="2" data-location="1" data-node="left-node" 
                       data-node-guid="{{output.multi_non_pw_data[active_multi_index].sGUID}}"
                       data-node-id="{{ output.multi_non_pw_data[active_multi_index].ID }}" 
                       data-node-description="{{output.multi_non_pw_data[active_multi_index].Title}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
            
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-1">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading"  name="" placeholder="Add something here" type="text" class="text editable-input ei-1" />
                    </div>
                    <div class="small-12 columns">
                        <a id="1" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a   id="1" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                 <!-- end editing -->
                <div 
                    ng-class="{
                                    'hide': !output.showinfodocnode || collapsed_info_docs[1] || (is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc) && is_AT_owner == 0)        
                            }"
                    class="left-node-info-div tt-accordion-content tg-accordion-1  tg-accordion-sub-1">
                    <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-1" data-box-index="1">          
                        <a  
                        ng-class="{
                                'hide': is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[1] 
                        }"
                        data-index="{{active_multi_index}}"
                        data-node-type="2" 
                        data-location="1" 
                        data-node="left-node" 
                        data-node-guid="{{output.multi_non_pw_data[active_multi_index].sGUID}}"
                        data-node-id="{{ output.multi_non_pw_data[active_multi_index].ID }}" 
                        data-node-description="{{output.multi_non_pw_data[active_multi_index].Title}}" 
                        class="edit-info-doc-btn edit-pencil ep ep-sub-1"
                        title=""
                        href="#"
                        >
                        <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                    </a>       
                        <div  class="left-node-info-text" ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(output.multi_non_pw_data[active_multi_index].Infodoc)">                
                        </div>
                    </div>
                </div>
            </div>

            <div 
                ng-class="{
                    'hide': is_html_empty(output.parent_node_info) && is_AT_owner==0,
                    'large-4' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-6':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
                class="columns original-info-doc">
                <div class="text-center">
                    <a  ng-click="set_collapse_cookies('parent-node'); update_infodoc_params('parent-node', output.ParentNodeGUID, '')" 
                    ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && output.parent_node_info != '')}"
                    class="tt-toggler-0" id="0" data-toggler="tg-accordion">
                        <span 
                            ng-class="{
                                        'hide' : is_AT_owner == 0  && is_html_empty(output.parent_node_info),
                                        'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[0],
                                        'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[0]
                                    }"
                            class="icon icon-desktop" data-node="0"> </span>

                    </a>

                     <span 
                         ng-class="{'hide':(is_html_empty(output.parent_node_info) && is_AT_owner == 0) }"
                         style="font-size: 12px;" id="0" class="parent-node hide-when-editing-0 text editable-trigger143 et-0 small">  {{output.hideInfoDocCaptions ? "" : output.parent_node}}   </span>  
                    
                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.parent_node_info)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-type="-1" data-location="-1" data-node="parent-node" 
                       data-node-guid="{{output.ParentNodeGUID}}" data-node-description="{{output.parent_node}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
            
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-0">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading"  name="" placeholder="Add something here" type="text" class="text editable-input ei-0" />
                    </div>
                    <div class="small-12 columns">
                        <a id="0" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a id="0" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                 <!-- end editing -->

            
                <div 
                    ng-class="{
                                    'hide': !output.showinfodocnode ||  collapsed_info_docs[0] || (is_html_empty(output.parent_node_info) && is_AT_owner == 0)
                            }"
                    class="parent-node-info-div  tt-accordion-content tg-accordion-0  tg-accordion-sub-0">
                    <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-0 tt-resizable-panel-1 small-centered columns box-2" data-box-index="2"
                         ng-class="{ 'mobile-resizable': isMobile() }">         
                        <a 
                        data-index="{{active_multi_index}}"
                        data-node-type="-1" 
                        data-location="-1" 
                        data-node="parent-node" 
                        data-node-guid="{{output.ParentNodeGUID}}"
                        data-node-description="{{output.parent_node}}" 
                        class="edit-info-doc-btn edit-pencil ep ep-0 ep-sub-0"
                        ng-class="{
                                    'hide': is_AT_owner != 1 || !output.showinfodocnode ||  collapsed_info_docs[0]
                            }"
                        title="" 
                        href="#"
                        >
                        <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil">
                        </span>
                    </a>        
                        <div  class="parent-node-info-text  " ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(output.parent_node_info)">               
                        </div>
                    </div>
                </div>
            </div>
    
            <div 
                 ng-class="{
                    'hide': is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT) && is_AT_owner==0,
                    'large-4' : is_AT_owner == 1 || (is_AT_owner == 0 && !is_html_empty(output.parent_node_info)), 
                    'large-6':is_AT_owner == 0 && is_html_empty(output.parent_node_info) 
                }"
            
                class="columns hide-for-medium-down">
                <div class="text-center">
                    <a ng-click="set_collapse_cookies('wrt-left-node'); update_infodoc_params('wrt-left-node', output.ParentNodeGUID, '')" 
                        ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && output.multi_non_pw_data[active_multi_index].InfodocWRT != '')}"
                        class=" tt-toggler-2" id="2" data-toggler="tg-accordion-sub">
                        <span 
                                ng-class="{
                                'hide' : (is_AT_owner == 0 && is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT)),
                                'icon-tt-minus-square' : output.showinfodocnode && !collapsed_info_docs[2],
                                'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[2]
                            }"
                            class="icon icon-desktop" data-node="2"></span>
                         
                    </a>
                     <span 
                         ng-class="{'hide': is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT) && is_AT_owner==0}"
                         style="font-size: 12px;" id="2" class="wrt-left-node hide-when-editing-2 text editable-trigger143 et-2 small">  {{output.hideInfoDocCaptions ? "" : get_wrt_text('right')}}   </span>
                     
                    <a ng-if="!(isMobile() || output.is_infodoc_tooltip) && is_AT_owner == 0 && !is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT)"
                       class="magnify-icon edit-info-doc-btn"
                       data-index="{{active_multi_index}}" data-node-id="{{output.multi_non_pw_data[active_multi_index].ID }}"
                       data-node-type="3" data-node-guid="{{output.multi_non_pw_data[active_multi_index].sGUID}}"
                       data-location="1" data-node="wrt-left-node" data-node-description="{{output.parent_node}}" 
                       data-node-title="{{output.multi_non_pw_data[active_multi_index].Title}}" data-readonly="1">
                        <span class="icon icon-tt-zoom-in"></span>
                    </a>
                </div>
            
                <!-- start editing -->
                <div class="row collapse editable-input-wrap eiw-2">
                    <div class="small-12 columns">
                        <input ng-model="WRT_left_heading"  name="" placeholder="Add something here" type="text" class="text editable-input ei-2" />
                    </div>
                    <div class="small-12 columns">
                        <a id="2" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a id="2" href="#"  class="button tiny tt-button-primary save-wrt-btn">Save</a>
                    </div>
                </div>
                 <!-- end editing -->

           
                <div 
                    ng-class="{
                            'hide': !output.showinfodocnode || collapsed_info_docs[2] || (is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT) && is_AT_owner == 0)
                        }"
                    class="wrt-left-node-info-div  tt-accordion-content tg-accordion-2  tg-accordion-sub-2">
                    <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns box-3" data-box-index="3"> 
                        <a 
                        data-index="{{active_multi_index}}"
                        ng-class="{
                                'hide' :  is_AT_owner != 1 || !output.showinfodocnode || collapsed_info_docs[2] || (is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT) && is_AT_owner == 0) }"
                        data-node-type="3" 
                        data-location="1" 
                        data-node="wrt-left-node" 
                        data-node-id="{{output.multi_non_pw_data[active_multi_index].ID }}"
                        data-node-guid="{{output.multi_non_pw_data[active_multi_index].sGUID}}"
                        data-node-title="{{output.multi_non_pw_data[active_multi_index].Title}}" 
                        data-node-description="{{output.parent_node}}" 
                        class="edit-info-doc-btn edit-pencil ep ep-sub-2" 
                        title="" 
                        href="#">
                        <span ng-class="{'hide': is_AT_owner != 1}" class="icon-tt-pencil"></span>
                    </a>                
                        <div  class="wrt-left-node-info-text" ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml( output.multi_non_pw_data[active_multi_index].InfodocWRT)">               
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </span>


</div>