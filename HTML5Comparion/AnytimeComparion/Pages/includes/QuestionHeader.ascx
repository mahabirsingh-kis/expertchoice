<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuestionHeader.ascx.cs" Inherits="AnytimeComparion.Pages.includes.QuestionHeader" %>
<%@ Register Src="~/Pages/includes/QHicons.ascx" TagPrefix="includes" TagName="QHicons" %> 
    <%-- Questions and Parent Info Doc Node--%>
          <div 
            ng-if="is_anytime" 
            ng-class="{'tt-sticky-element': (output.page_type != 'atAllPairwise') || (output.page_type == 'atAllPairwise' && output.pairwise_type=='ptGraphical') || ( output.page_type == 'atAllPairwise' && output.pairwise_type == 'ptVerbal' && multi_data.length >= 7)}"
            class="question-header large-12 columns anytime-question-header">
            <div class="tg-legend">
                <div class="small-12 columns tt-question-title text-center qh-icons-wrap">
                        <div 
                            ng-if="output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults'"
                            class="row qhelp-questions common-location"><includes:QHicons runat="server" ID="Info_QHicons" />
                        </div> 
                    <h3 class="no-margin">                        
                        <span class="pipe-question question-node-info-text" ng-bind-html="getHtml(output.step_task)" ng-class="output.step_task != '' ? '' : 'hide'">
                        </span>
                        <span 
                            class="pipe-question"
                            ng-class="output.step_task == '' ? '' : 'hide'">
                            which of the two 
                            <span class='question' ng-bind-html="getHtml(output.question)"></span>            
                            <span class='wording' ng-bind-html="getHtml(output.wording)">output.wording</span>
                        </span>    
                </h3>
                </div>
            </div>
              <div class="hide" ng-class="{'other-info-doc-height': isMobile() && output.framed_info_docs && !(is_html_empty(multi_data[active_multi_index].InfodocLeft) && is_html_empty(multi_data[active_multi_index].InfodocRight))}">
            <div ng-if="!output.is_infodoc_tooltip && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes')" class="other-info-doc columns {{ multi_data.length > 7 ? 'duplicate-parent-infodoc' : ''}} hide large-8 large-centered">
                <div class="text-center" 
                    ng-class="{'hide' : (is_AT_owner == 0 &&  is_html_empty(output.parent_node_info))}">
                    <a ng-click="set_collapse_cookies('parent-node'); update_infodoc_params('parent-node', output.ParentNodeGUID, '')"
                        ng-class="{'tt-toggler': is_AT_owner == 1 ||  ( is_AT_owner == 0 && !is_html_empty(output.parent_node_info))}"
                        class="tt-toggler-0" id="0" data-toggler="tg-accordion">
                        <span 
                            ng-class="{
                                        <%--'hide' : (is_AT_owner == 0  &&  is_html_empty(output.parent_node_info)),--%>
                                        'icon-tt-minus-square' : (output.showinfodocnode) && !collapsed_info_docs[0],
                                        'icon-tt-plus-square' :  !output.showinfodocnode || collapsed_info_docs[0]
                                    }"
                            class="icon icon-desktop" data-node="0"></span>
                    </a>

                    <span id="0" class="text parent-lbl parent-node hide-when-editing-0 text editable-trigger143 et-0 small"> {{output.hideInfoDocCaptions ? "" : output.parent_node}}    </span>
                
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
                    </span>
                </a>
                </div>
                <div 
                    ng-class="{
                                 'hide': !output.showinfodocnode || collapsed_info_docs[0] || (is_html_empty(output.parent_node_info) && is_AT_owner == 0), 
                                'infodoc-height-mobile': isMobile(), 
                                'infodoc-height': !isMobile()   
                            }"
                    class="parent-node-info-div  tt-accordion-content tg-accordion-0  tg-accordion-sub-0">
                    <div data-index="0" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-0 small-centered columns">
                        <div class="parent-node-info-text"  ng-class="is_screen_reduced ? 'zoom-out' : ''" ng-bind-html="getHtml(output.parent_node_info)">               
                        </div>
                    </div>
                </div>
            </div>
            <div ng-if="isMobile() && output.framed_info_docs && !output.is_infodoc_tooltip && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes')">
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
              </div>
            <div 
                 ng-if="output.pairwise_type=='ptVerbal' && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes')"
                class="legend columns {{ multi_data.length > 7 ? 'duplicate-parent-infodoc' : ''}} hide">
                <div class="text-center panel" style="font-size: .750rem;padding: 1px;margin-bottom: 3px;">
                    <b>EQ</b>ual<b>&nbsp;&nbsp;&nbsp;M</b>oderate
                    &nbsp;&nbsp;&nbsp;<b>S</b>trong
                    &nbsp;&nbsp;&nbsp;<b>V</b>ery<b>S</b>trong
                    &nbsp;&nbsp;&nbsp;<b>EX</b>treme
                </div>
            </div>
        </div>

<!-- Teamtime Question -->
        <div ng-if="is_teamtime" class="tt-sticky-element teamtime-question question-header" >
            
            <div class="tg-legend">
                <div class="hide-for-medium-down">
                    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left" style="padding: 0px;"><span style="">show comments</span></div>
                </div> 
                <div class="small-12 columns tt-question-title text-center small-centered">
                    <h3><span class="pipe-question question-node-info-text" ng-bind-html="getHtml(information_nodes.Task)" ng-class="information_nodes.Task != '' ? '' : 'hide'"></span></h3>
                </div>
            </div>
        </div>
        <!-- Teamtime Question -->
        <%-- End of Desktop Questions --%>

