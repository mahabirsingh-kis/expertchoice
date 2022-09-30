<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaleDescription.ascx.cs" Inherits="AnytimeComparion.Pages.includes.ScaleDescription" %>

<%-- anytime --%>
<div ng-if="is_anytime">
    <div 
      data-equalizer-watch="nodes"
      ng-if="output.is_infodoc_tooltip && (output.showinfodocnode || (!output.showinfodocnode &&  output.page_type=='atPairwise'))"
        class="tooltip-mode"
         ng-class="{
            'text-center ' : output.page_type!='atPairwise',
            'tt-accordion-head blue no-gradient  tg-gradients vs-left': output.page_type=='atPairwise',
            'hide' : is_AT_owner == 0 && ((is_multi && is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || is_html_empty(output.ScaleDescriptions[0].Description)) && output.page_type != 'atPairwise'
        }"
        >
        <span>
            <a ng-if="output.showinfodocnode" data-dropdown="gdrop_scale_node" aria-controls="gdrop_scale_node" 
               aria-expanded="false" data-node-type="4" data-location="1" data-node="scale-node" 
               data-node-id="{{is_multi ? active_multi_index + 1 : 0}}" data-options="is_hover:true; hover_timeout:1000"
               data-node-guid="{{is_multi ? output.ScaleDescriptions[active_multi_index].Guid : output.ScaleDescriptions[0].Guid}}"
               data-node-description="{{is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}"
               data-readonly="{{is_AT_owner == 0 ? 1 : 0}}" class="scale-node-tooltip tooltip-infodoc-btn">
                <span ng-if="is_AT_owner || (!is_AT_owner && ((is_multi && !is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || !is_html_empty(output.ScaleDescriptions[0].Description)))" 
                      ng-class="(is_multi && is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || is_html_empty(output.ScaleDescriptions[0].Description) ? 'disabled' : 'not-disabled'" 
                      class="icon icon-tt-info-circle"></span>
            </a>
            {{is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}
        </span>
         <div 
             ng-class="{'medium' : has_long_content((is_multi ? output.ScaleDescriptions[active_multi_index].Description : output.ScaleDescriptions[0].Description))}"
             ng-if="output.page_type!='atPairwise'" id="gdrop_scale_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                
                    <div class="small-9 columns text-center">
                        {{output.parent_node}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a  style="color: #008CBA;"
                                    ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" 
                                    data-node-type="4" data-location="1" data-node="scale-node" data-node-id="{{is_multi ? active_multi_index + 1 : 0}}"
                                    data-node-guid="{{is_multi ? output.ScaleDescriptions[active_multi_index].Guid : output.ScaleDescriptions[0].Guid}}"
                                    data-node-description="{{is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}">
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
                <div class="infotext-wrap large-12 columns scale-node-info-text" ng-bind-html="getHtml((is_multi ? output.ScaleDescriptions[active_multi_index].Description : output.ScaleDescriptions[0].Description))"></div>
            </div>
        </div>
    </div>

    <div ng-if="is_AT_owner == 0 && ((is_multi && is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || is_html_empty(output.ScaleDescriptions[0].Description)) && output.page_type != 'atPairwise'">
        &nbsp;
    </div>

    <div 
        ng-class="{'hide' : (is_AT_owner == 0 && ((is_multi && is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || is_html_empty(output.ScaleDescriptions[0].Description)) && output.page_type != 'atPairwise')}"
        ng-if="!output.is_infodoc_tooltip"
        data-equalizer-watch="nodes"  
        class="columns tt-accordion-head {{output.page_type == 'atPairwise' ? 'blue' : ''}} {{main_gradient_checkbox ? '' : 'no-gradient'}} tg-gradients vs-left">
        <div class="tt-center-height-wrap columns">
            <div class="tt-center-content-wrap">
            <a 
               ng-class="
                {
                    'tt-toggler': is_AT_owner == 1 || (is_AT_owner == 0 && ((is_multi && !is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || !is_html_empty(output.ScaleDescriptions[0].Description)))
                }"
                <%--ng-click="set_collapse_cookies('scale-node'); update_infodoc_params('scale-node', output.LeftNodeGUID, output.RightNodeGUID)"--%>
                class="tt-toggler-1" id="5"  data-toggler="tg-accordion-sub">
                <span
                    ng-if="output.showinfodocnode"
                    ng-class="{
                        'hide' :(is_AT_owner == 0 && ((is_multi && is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || is_html_empty(output.ScaleDescriptions[0].Description))),
                        'icon-tt-minus-square' : output.showinfodocnode && !is_html_empty(output.ScaleDescriptions[active_multi_index].Description),
                        'icon-tt-plus-square' :  !output.showinfodocnode || is_html_empty(output.ScaleDescriptions[active_multi_index].Description)
                    }"
                        class="icon icon-desktop" data-node="1"></span>
            </a>



            <%--<a 
                ng-if="output.pairwise_type == 'ptVerbal'" href="#" data-reveal-id="dropLeftNode"><span class="left-node child-label" ng-class="{'black':output.page_type !='atPairwise'}"> {{is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}</span></a>

            <a 
                ng-if="output.pairwise_type != 'ptVerbal'" href="#"><span class="left-node child-label" ng-class="{'black':output.page_type !='atPairwise', 'hide': output.page_type == 'atNonPWOneAtATime'}"> {{is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}</span></a>--%>

            <span
                 ng-if="output.page_type != 'atPairwise' && output.showinfodocnode" >
                <span  id="1" class="hide-when-editing-1 text et-1 black">
                    {{output.hideInfoDocCaptions ? "" : is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}
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
                        <a id="5" href="#" class="button tiny tt-button-primary cancel-wrt-btn ">Cancel</a>
                        <a id="5" href="#"  class="button tiny tt-button-primary save-wrt-btn"
                           <%--ng-click="update_infodoc_params('scale-node', output.LeftNodeGUID, '', 'left_heading')"--%>
                            >Save</a>
                    </div>
                </div>
            </span>
            <!-- end editing -->
            <%-- end of info doc heading --%>

            </div>
        </div>
    </div>
    <div
        ng-if="!output.is_infodoc_tooltip"
        ng-class="{
                    'hide' :  !output.showinfodocnode || ((is_multi && is_html_empty(output.ScaleDescriptions[active_multi_index].Description)) || is_html_empty(output.ScaleDescriptions[0].Description))}"
            class="left-node-info-div columns tt-accordion-content tg-accordion-sub-5" > 
        <div data-index="1" class="tt-panel-temp tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns" id="scales_description">
            <a ng-class="{ 'hide': is_AT_owner == 0 || !output.showinfodocnode }"
                data-node-type="4" data-location="1" data-node="scale-node" data-node-id="{{is_multi ? active_multi_index + 1 : 0}}"
                data-node-guid="{{is_multi ? output.ScaleDescriptions[active_multi_index].Guid : output.ScaleDescriptions[0].Guid}}"
                data-node-description="{{is_multi ? output.ScaleDescriptions[active_multi_index].Name : output.ScaleDescriptions[0].Name}}" 
                class="edit-pencil ep-sub-5 edit-info-doc-btn" title="" href="#">
                <span ng-class="{'hide': is_AT_owner == 0}" class="icon-tt-pencil"></span>
            </a>
            <div class="scale-node-info-text nodes-wrap" ng-bind-html="getHtml((is_multi ? output.ScaleDescriptions[active_multi_index].Description : output.ScaleDescriptions[0].Description))">
            </div>
        </div>
    </div>
</div>
<%-- //anytime --%>
<%-- teamtime --%>
<div ng-if="is_teamtime">
    <div ng-if="!is_infodoc_tooltip">
        <div data-equalizer-watch class="columns tt-accordion-head lite tg-gradients " ng-class="{'no-gradient' : main_gradient_checkbox, 'blue':current_action_type == 'pairwise'}">
            <a
                ng-class="{'tt-toggler' :((!output.isPM && !is_html_empty(output.pipeData.ScaleDescription.Description)) || output.isPM) && output.showinfodocnode }"
                class="" ng-click="update_infodocs(5, $event)" id="5" data-toggler="tg-accordion-sub">
                <span
                    ng-if="output.showinfodocnode"
                    ng-class="{'hide' :(!output.isPM && is_html_empty(output.pipeData.ScaleDescription.Description))}"
                    class="icon-tt-plus-square icon icon-desktop" data-node="5"></span>
                <%--<span ng-if="(output.showinfodocnode) || (!output.showinfodocnode && current_action_type == 'pairwise')" class="left-node">{{information_nodes.LeftNode}}</span>--%>
                <span ng-if="(output.showinfodocnode) || (!output.showinfodocnode && current_action_type == 'pairwise')" class="left-node">Scale Description</span>
            </a>
        </div>
        <div
            class="columns tt-accordion-content tg-accordion-sub-5 hide">
            <div data-index="5" class="tt-panel tt-resizable-panel tt-resizable-panel-1 small-centered columns">
                <!-- Edit Icon -->
                <a ng-if="output.isPM" class="edit-pencil ep-sub-5 edit-info-doc-btn" title="" href="#"
                   data-node-description="{{output.pipeData.ScaleDescription.Name}}" data-node-type="4" data-location="1"
                   data-node="scale-node" data-node-guid="{{output.pipeData.ScaleDescription.Guid}}">
                    <span class="icon-tt-pencil"></span>
                </a>
                <!-- End Edit Icon -->
                <div class="scale-node-info-text nodes-wrap" id="scaleDescription" ng-bind-html="getHtml(output.pipeData.ScaleDescription.Description)">
                </div>
            </div>
        </div>
    </div>


    <div
        ng-if="is_infodoc_tooltip && (output.showinfodocnode || (!output.showinfodocnode && current_action_type == 'pairwise'))"
        class="tooltip-mode"
        ng-class="{
            'text-center ' : current_action_type != 'pairwise',
            'tt-accordion-head blue no-gradient  tg-gradients vs-left': current_action_type=='pairwise',
            'hide' : !output.isPM &&  is_html_empty(output.pipeData.ScaleDescription.Description) && current_action_type != 'pairwise' 
         }">
        <span>
            <a
                ng-if="output.showinfodocnode"
                data-dropdown="gdrop_scale_node"
                aria-controls="gdrop_scale_node"
                aria-expanded="false"
                class="">
                <span
                    ng-if="output.isPM || (!output.isPM && !is_html_empty(output.pipeData.ScaleDescription.Description))"
                    ng-class="is_html_empty(output.pipeData.ScaleDescription.Description) ? 'disabled' : 'not-disabled'" class="icon icon-tt-info-circle"></span>
            </a>
            Scale Description
        </span>
        <div
            ng-class="{'medium' : has_long_content(output.pipeData.ScaleDescription.Description)}"
            id="gdrop_scale_node" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">

                <div class="small-9 columns text-center">
                    Scale Description
                </div>
                <div class="small-3 columns text-center">
                    <div class="row collapse">
                        <div class="small-6 columns text-center">
                            <a style="color: #008CBA;"
                                ng-class="output.isPM ? '' : 'hide'" class="edit-info-doc-btn" data-node-type="4"
                                data-location="1" data-node="scale-node" data-node-guid="{{output.pipeData.ScaleDescription.Guid}}"
                                data-node-description="{{output.pipeData.ScaleDescription.Name}}">
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
                <div class="infotext-wrap large-12 columns scale-node-info-text" ng-bind-html="getHtml(output.pipeData.ScaleDescription.Description)"></div>
            </div>
        </div>
    </div>
</div>
<%-- //teamtime --%>