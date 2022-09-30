<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Anytime.aspx.cs" Inherits="AnytimeComparion.Pages.Anytime.Anytime" %>
<%@ Register Src="~/Pages/Anytime/LocalResults.ascx" TagPrefix="uc1" TagName="LocalResults" %>
<%@ Register Src="~/Pages/Anytime/StepsList.ascx" TagPrefix="uc1" TagName="StepsList" %>
<%@ Register Src="~/Pages/Anytime/GlobalResults.ascx" TagPrefix="uc1" TagName="GlobalResults" %>
<%@ Register Src="~/Pages/Anytime/MultiPairwise.ascx" TagPrefix="uc1" TagName="MultiPairwise" %>
<%@ Register Src="~/Pages/Anytime/Pairwise.ascx" TagPrefix="uc1" TagName="Pairwise" %>
<%@ Register Src="~/Pages/Anytime/Ratings.ascx" TagPrefix="uc1" TagName="Ratings" %>
<%@ Register Src="~/Pages/Anytime/StepFunction.ascx" TagPrefix="uc1" TagName="StepFunction" %>
<%@ Register Src="~/Pages/Anytime/MultiRatings.ascx" TagPrefix="uc1" TagName="MultiRatings" %>
<%@ Register Src="~/Pages/Anytime/DirectComparison.ascx"  TagPrefix="uc1" TagName="DirectComparison" %>
<%@ Register Src="~/Pages/Anytime/MultiDirect.ascx" TagPrefix="uc1" TagName="MultiDirect" %>
<%@ Register Src="~/Pages/Anytime/UtilityCurve.ascx" TagPrefix="uc1" TagName="UtilityCurve" %>
<%@ Register Src="~/Pages/Anytime/Survey.ascx" TagPrefix="uc1" TagName="Survey" %>
<%@ Register Src="~/Pages/Anytime/SensitivitiesAnalysis.ascx" TagPrefix="uc1" TagName="SensitivitiesAnalysis" %>
<%@ Register Src="~/Pages/includes/Footer.ascx" TagPrefix="includes" TagName="Footer" %> 
<%@ Register Src="~/Pages/includes/QHicons.ascx" TagPrefix="includes" TagName="QHicons" %>
 <%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <asp:PlaceHolder runat="server">
       <link href="/Content/stylesheets/responsive-tables.css" rel="stylesheet" />
        
        <style>
            /*copied css from comparion*/

            #information_page h1,  #information_page h2,  #information_page h3,  #information_page h4,  #information_page h5,  #information_page h6,  #information_page .h1,  #information_page .h2,  #information_page .h3,  #information_page .h4,  #information_page .h5,  #information_page .h6 {
                color: #005292;
                text-align: center;
                margin: 0px;
                padding: 0px 0px 0.75ex 0px;
            }

            #information_page h5,  #information_page .h5 {
                font-size: 13pt;
            }

            .mce-notification-warning{
                display:none !important;
            }
            
            /* //copied css from comparion*/

            .flash {
                -webkit-animation: flash 1s linear;
                -webkit-animation-iteration-count: infinite;

                animation: flash 1s linear;
                animation-iteration-count: infinite;
            }

            @-webkit-keyframes flash {
                50% { background-color: yellow; }
                100% { background-color: inherit; }
            }

            @keyframes flash {
                50% { background-color: yellow; }
                100% { background-color: inherit; }
            }

 
            #si-wrapper .silc-btn {
                bottom: 70px !important;
            }

            .tt-content-wrap .tt-body .tt-sticky-element.fixed.sticky {
                position: -webkit-sticky !important;
                position: sticky !important;
            }

            /*==================== Flashing for judgment line starts ====================*/
            /***** Support IE-10+, Chrome-4+, Safari-4+, Firefox-16+ and Opera-15+ *****/

            .judgment-flash {
                -webkit-animation: judgment-flash 1s linear;
                -webkit-animation-iteration-count: infinite;
                animation: judgment-flash 1s linear;
                animation-iteration-count: infinite;
            }

            @-webkit-keyframes judgment-flash {
                17% { background-color: #FFFFA4; }
                33% { background-color: #FFFFE6; }
                50% { background-color: #FFFFA4; }
                67% { background-color: #FFFFE6; }
                83% { background-color: #FFFFA4; }
                100% { background-color: #FFFFE6; }
            }

            @keyframes judgment-flash {
                17% { background-color: #FFFFA4; }
                33% { background-color: #FFFFE6; }
                50% { background-color: #FFFFA4; }
                67% { background-color: #FFFFE6; }
                83% { background-color: #FFFFA4; }
                100% { background-color: #FFFFE6; }
            }

            .judgment-flash-li {
                -webkit-animation: judgment-flash-li 1s linear;
                -webkit-animation-iteration-count: infinite;
                animation: judgment-flash-li 1s linear;
                animation-iteration-count: infinite;
            }

            @-webkit-keyframes judgment-flash-li {
                17% { border-top-color: #FFFFA4; }
                33% { border-top-color: #FFFFE6; }
                50% { border-top-color: #FFFFA4; }
                67% { border-top-color: #FFFFE6; }
                83% { border-top-color: #FFFFA4; }
                100% { border-top-color: #FFFFE6; }
            }

            @keyframes judgment-flash-li {
                17% { border-top-color: #FFFFA4; }
                33% { border-top-color: #FFFFE6; }
                50% { border-top-color: #FFFFA4; }
                67% { border-top-color: #FFFFE6; }
                83% { border-top-color: #FFFFA4; }
                100% { border-top-color: #FFFFE6; }
            }

            /*==================== Flashing for judgment line ends ====================*/

            .mobile-frame .tt-resizable-panel-1{
                height: 125px;
            }

            .ui-tooltip {
                padding: 5px 10px;
                font-size: 12px;
            }
        </style>
    </asp:PlaceHolder>
</asp:Content>
<asp:Content ID="Content2" class="tt-content-wrap" ContentPlaceHolderID="MainContent" runat="server" >
 

<span  id="anytime-page" ng-controller="AnytimeController" ng-init="initializeAnytime(<%=Session["AT_CurrentStep"]%>, <%=Session["AT_isOwner"]%>);" ng-mousedown="swipe_enabled = false" ng-mouseup="swipe_enabled = true"   ng-cloak>

   <%-- hash link message //is_explorer && is_firefox && --%>    
    <div id="browser-warning-message" ng-if="showBrowserWarning() && output.hashLink" data-alert class="small-10 medium-10 large-8 columns small-centered text-center alert-box"
        style="background-color: #f3f3f3; padding: 1px 20px 1px 10px; border-radius: 5px; border: 1px solid #ddd;
                clear: both; font-size: 14px !important; color:#777; margin-bottom: 0">
            <!-- alert-box info radius -->
           
            <p ng-if="isMobile()" style="margin-bottom: 0px;font-size: 14px;">
                If response is slow, copy the following link and paste it into Chrome or the default browser on Android or Safari or Chrome on iOS phones.
            </p>
            <p ng-if="!isMobile()" style="margin-bottom: 0px;font-size: 14px;">If response is slow, copy the following link and paste it into a Chrome or Edge browser on Windows or Safari or Chrome on a Mac.</p> 
        
             <p style="margin-bottom: 5px; font-size:12px;"><strong>{{output.hashLink}}</strong> <a href="#" class="radius button normal-green-bg" style="font-size:14px; padding:5px 10px; margin-bottom:0px"  ngclipboard-success="copy_responsive_link()" ngclipboard data-clipboard-text="{{output.hashLink}}" >COPY</a></p>                   
            <a  ng-click="close_warning_message()" href="#" class="" style="position: absolute; top: 5px; right: 5px;
                color: #fff; background-color: #999; padding:0px 5px; border-radius: 3px;">&times;</a>
    </div>
    <%-- end hash link message --%> 

     <div id="warningModal" data-reveal class="reveal-modal small tt-modal-wrap fixed-modal" aria-labelledby="modalTitle" aria-hidden="true" role="dialog" data-options="close_on_background_click:false;close_on_esc:false;">
        <%--<a class="close-reveal-modal" aria-label="Close">&#215;</a>--%>
        <h2 id="modalTitle" class="tt-modal-header blue-header">
            Information
        </h2>

        <div class="large-12 columns  ">
            <div class="columns text"><p>{{output.pipeWarning}}</p></div>
        </div>
        <div class="columns text-center">
            <div class="button medium tt-button-primary" ng-click="closeModal('warningModal')">OK</div>
        </div>
        
    </div>

    
    <div class="tt-full-height tt-body tt-pipe anytime-wrap" ng-style="{visibility: projectLockedInfo.status ? 'hidden' : ''}">
        
        <div id="dropLeftNode" class="reveal-modal small tt-modal-wrap" data-reveal ariadropLeftNode-labelledby="modalTitle" aria-hidden="true" role="dialog">
                <h2 id="modalTitle" class="tt-modal-header blue-header">{{output.first_node}}  {{output.child_node}}</h2>
                <div class="large-12 columns">Click on one of the intensities depending on how strong you feel toward the choices</div>
                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        </div>

        <div id="dropRightNode" class="reveal-modal small tt-modal-wrap" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
              <h2 id="modalTitle" class="tt-modal-header blue-header">{{output.second_node}}</h2>
              <div class="large-12 columns">Click on one of the intensities depending on how strong you feel toward the choices</div>
              <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        </div>  
        <div 
            style="right: 4%;position: relative;"
            ng-if="(output.page_type == 'atInformationPage') || (output.page_type == 'atShowLocalResults' && (intermediate_screen==0 || (intermediate_screen==3 && !isMobile()))) || (output.page_type == 'atShowGlobalResults') || (output.page_type == 'atSensitivityAnalysis')"
            class="row qhelp-questions special-locations"><includes:QHicons runat="server" ID="Info_QHicons" />
        </div>      
       
         <div class="tt-full-height row collapse tt-2-cols">
             
                    <div class="tt-full-height large-12 columns tt-left-col">
                                <div class="large-12 columns large-centered tt-clear-padding-640">
                                      
                                    <%-- transferred to Site.master and breadcrumns.aspx--%>   
                                   
                                    <div id="pipeContent"></div>

                                      

                                    <%-- Information / Welcome Page --%>
                                    <div class="tt-auto-resize-content tt-homepage"  ng-if="output.page_type == 'atInformationPage'" ng-cloak>
                                        <div class="large-12 columns tt-homepage-main-wrap">
                                            <div id="information_page"  class="anytime-pages comparion-text  large-12 columns" style="height: 100%;">
                                                <div ng-bind-html="getHtml(information_message)" class="ds-div-trigger wrap-hyperlink tt-center-height-wrap columns thankyouText" ng-class="output.current_step != 1 ? ' text-center ' : ''"></div>
                                               
                                            </div>
                                        </div>
                                    </div>
                                    <%-- End of Information / Welcome Page --%>
                                 </div>
                             
                     
                            
                            
                    </div>
                    
                    
                    
                    <!-- modal wrap -->

                     <%-- GLobal Modal for Nodes --%>
                        <div id="GlobalNodesModal"  class="reveal-modal tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                            <h2 class="tt-modal-header blue-header">Apply to</h2>
                            <div class="tt-modal-content large-12 columns">
                                <div class="row">
                                    <div class="large-12 columns">
                                        <div
                                            ng-repeat="node in output.nodes_data">
                                               <label>
                                                   <input 
                                                   ng-checked="output.parentnodeID == node[0]"
                                                   name="node_guids[]" class="apply_to_checkboxes" style="float:none;" type="checkbox" data-id="{{node[0]}}"/>        
                                                   {{node[1]}}
                                               </label>
                                        </div>
                                    </div> 
                                        <div class="row">
                                            <br />
                                            <div class="small-6 columns text-left">
                                                <a href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                                        <span class="icon-tt-close icon"></span>
                                                        <span class="text">Cancel</span>
                                                    </a>
                                            </div>
                                            <div class="small-6 columns text-right">
                                                    <a href="#" data-is-multi="{{is_multi}}" class="button tiny tt-button-icond-left apply-nodes-btn button tiny tt-button-primary success">
                                                        <span class="icon-tt-save icon"></span>
                                                        <span class="text">Save</span>
                                                    </a>
                                            </div>
                                        </div>
                                        <div class="columns">
                                            <div data-alert class="alert-box success radius" style="display:none;">
                                                Updated successfully!
                                                <a href="#" class="close">&times;</a>
                                            </div>
                                            <div data-alert class="alert-box alert radius hide" style="display:none;">
                                                Error on saving. Please try again.
                                                <a href="#" class="close">&times;</a>
                                            </div>
                                        </div>

                                </div>
                            </div>
                       </div>
                    <%-- //end of modal --%>

                    <div id="tt-c-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                        <h2 id="modalTitle" class="tt-modal-header blue-header">Your Comment About The Judgment</h2>

                        <div class="tt-modal-content">
                            <div class="row">
                                <div class="large-12 columns">
                                   <%-- <textarea class="single_comment" ng-init="single_comment = output.comment" ng-model="single_comment" name="" rows="10" ng-value="output.comment" placeholder="Please enter your comment."></textarea>--%>

                                    <textarea class="single_comment" ng-model=" output.comment" name="" rows="10" placeholder="Please enter your comment."></textarea>
                                </div>
                                <div class="large-12 columns">
                                   <%-- <div class="small-6 columns text-left">
                                        <a ng-click="close_single_modal()" href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                            <span class="icon-tt-close icon"></span>
                                            <span class="text">Cancel</span>
                                        </a>
                                    </div>--%>
                                    <div class="text-right ">
                                        <a  ng-click="comment_updated()" href="#" class="button tiny tt-button-icond-left button tiny tt-button-primary success">
                                            <span class="icon-tt-save icon"></span>
                                            <span class="text">Save</span>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div ng-click="$event.preventDefault(); $event.stopPropagation();" id="comment-single" 
                        class="f-dropdown stay-open-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1" 
                        ng-class="{'drop-top': output.page_type !== 'atPairwise'}">
                        <div class="row">
                            <div class="small-12 columns text-left">
                                    Add Comment 
                            </div>
                            <div class="small-12 columns">
                               <%-- <textarea class="single_comment" ng-init="single_comment = output.comment" ng-model="single_comment" name="" rows="4" ng-value="output.comment" placeholder="Add your comment"></textarea>--%>

                                 <textarea ng-model="output.comment" name="" rows="4" placeholder="Add your comment"></textarea>
                            </div>
                            <div class="large-12 columns text-right">
                                <%--<a ng-click="close_multi_comment('single', 'close')" href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                        <span class="icon-tt-close icon"></span>
                                        <span class="text">Cancel</span>
                                    </a>--%>
                                <a ng-click="comment_updated()" href="#" class="button tiny tt-button-icond-left button tiny tt-button-primary success">
                                    <span class="icon-tt-check icon"></span>
                                    <span class="text">OK</span>
                                </a>
                            </div>
                        </div>
                    </div>
             
         <div ng-click="$event.preventDefault(); $event.stopPropagation();" id="ratings-description" 
              class="f-dropdown stay-open-dropdown dropdown-long-content-wrap" data-dropdown-content aria-hidden="true" tabindex="-1" 
              ng-class="{'drop-top': output.page_type !== 'atPairwise'}">
             <div class="row">
                 <div class="small-12 columns text-left">
                     Edit intensity description 
                 </div>
                 <div class="small-12 columns">
                     <%-- <textarea class="single_comment" ng-init="single_comment = output.comment" ng-model="single_comment" name="" rows="4" ng-value="output.comment" placeholder="Add your comment"></textarea>--%>
                     <textarea ng-if="is_multi" ng-model="output.multi_non_pw_data[active_multi_index].Ratings[editIntensityIndex].Comment" name="" rows="4" placeholder=""></textarea>
                     <textarea ng-if="!is_multi" ng-model="output.intensities[editIntensityIndex][4]" name="" rows="4" placeholder=""></textarea>
                 </div>
                 <div class="large-12 columns text-right">
                     <a ng-click="updateIntensityDescription()" href="#" class="button tiny tt-button-icond-left button tiny tt-button-primary success">
                         <span class="icon-tt-check icon"></span>
                         <span class="text">OK</span>
                     </a>
                 </div>
             </div>
         </div>

                     <!-- modal wrap -->
                    
                    <div id="tt-qh-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                        <h2 id="modalTitle" class="tt-modal-header blue-header">Edit Quick Help</h2>

                        <div class="tt-modal-content">
                            <div class="row">
                                <div class="hide large-12 columns qh-text-area">
                                    <textarea id="qh_info_value" ng-model="output.qh_info" ng-value="output.qh_info" rows="10" name="" placeholder="Please enter quick help.">                                      
                                    </textarea>
                                </div>
                                <div class="large-12 columns" style="margin-top: 10px;">
                                    <div class="row collapse q-help" ng-if="show_apply_qh_cluster == true">
                                        <span  ng-if="show_apply_qh_cluster" ng-click="set_qe_content()"><a href="#">[Use quick help sample]</a></span>
                                        <span  ng-if="!show_apply_qh_cluster"><a>&nbsp;</a></span>
                                    </div>
                                    <div class="large-3 small-3 columns large-text-left small-text-center qh-btn-wrap">
                                        <a href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                            <span class="icon-tt-close icon"></span>
                                            <span class="text hide-for-small">Cancel</span>
                                        </a>
                                    </div>
                                     <div class="large-6 small-6 columns modal-bottom-info-wrap" style="padding: 0px;">
                                        <div class=" small-12 columns   modal-bottom-info-wrap">
                                            <div class="cluster">
                                                <label>
                                                    <input ng-model="output.show_qh_automatically" type="checkbox" class="ng-pristine ng-untouched">
                                                    <span class="info hide-for-small">Show Quick Help Automatically</span>                                                    
                                                    <span class="info show-for-small">Show Quick Help</span>                                                    
                                                </label>
                                            </div>
                                        </div>
                                    </div>
                                   
                                    <div class="large-3 small-3 columns large-text-right small-text-center qh-btn-wrap">                                      
                                        <%-- <a 
                                            data-reveal-id="cluster-modal"
                                            ng-if="output.has_cluster"
                                             href="#" class="button tiny tt-button-icond-left tt-button-primary tt-s-btn save-qh-btn">
                                            <span class="icon-tt-save icon"></span>
                                            <span class="text hide-for-small">Save for cluster(s)...</span>
                                        </a>--%>
                                        <a 
                                            ng-click="output.QH_cluster=false; save_quick_help()"
                                            href="#" class="button tiny tt-button-icond-left button tiny tt-button-primary success save-qh-btn">
                                            <span class="icon-tt-save icon"></span>
                                            <span class="text hide-for-small" >Save for this step</span>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

             <!-- cluster modal wrap -->

             <%--<div id="cluster-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                <h2 id="modalTitle" class="tt-modal-header blue-header">Save for cluster(s)...</h2>

                <div class="tt-modal-content">
                    <div class="row">
                          <div
                            ng-repeat="node in output.cluster_nodes">
                                <label>
                                    <input 
                                        ng-model="temp_node_ids[$index]"
                                        ng-checked="node[2] == output.parentnodeID"
                                        ng-click="add_to_cluster_nodes(temp_node_ids[$index], node)"
                                        style="float:none;" type="checkbox"/>        
                                    {{node[0]}}
                                </label>
                        </div>
                    </div>
                    <div class="row">
                        <br />
                        <div class="small-6 columns text-left">
                            <a href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                    <span class="icon-tt-close icon"></span>
                                    <span class="text">Cancel</span>
                                </a>
                        </div>
                        <div class="small-6 columns text-right">
                                <a href="#" ng-click="save_by_cluster(qh_cluster_nodes)" class="button tiny tt-button-icond-left button tiny tt-button-primary success">
                                    <span class="icon-tt-save icon"></span>
                                    <span class="text">Save</span>
                                </a>
                        </div>
                    </div>
                </div>
            </div>--%>

             <!-- cluster modal wrap -->
                    
                    <div id="tt-view-qh-modal" class="reveal-modal tt-modal-wrap fixed-modal medium" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                        <h2 id="modalTitle" class="tt-modal-header blue-header">Quick Help</h2>

                        <div class="tt-modal-content">
                            <div class="row">
                                <div  class="anytime-pages comparion-text  large-12 columns qh-parent-div" style="height: 100%;">
                                    <span ng-if="qh_text != ''" id="quick-help-content"></span>
                                    <span ng-if="qh_text == ''">
                                       No Quick Help yet.
                                    </span>
                                </div>
                                <div class="large-12 columns">
                                    <div class="small-8 columns text-left qh-btn-wrap">
                                         <label ng-if="output.show_qh_automatically==1">
                                                 <input style="margin-top: 0px;" ng-click="show_qh_automatically(output.dont_show_qh)" type="checkbox" ng-model="output.dont_show_qh">
                                                 <span>Don't automatically show Quick Help</span>
                                        </label>
                                    </div>
                                    <div class="small-4 columns text-right qh-btn-wrap">
                                         <a href="#" class="button tiny tt-button-icond-left close-reveal-modal cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                            <span class="icon-tt-close icon"></span>
                                            <span class="text">Close</span>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                                    
                    <div id="tt-h-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                        <h2 id="modalTitle" class="tt-modal-header blue-header">Current Cluster</h2>

                        <div class="tt-modal-content">
                            <div class="row">
                                <div class="large-12 columns">
                                    <ul class="tt-site-map-wrap hierarchies"  >
                                        <li ng-repeat="hierarchy in $parent.hierarchies"  ng-include="'menu_sublevel.html'">

                                        </li>

                                    </ul>
                                    <div class="nothing hide">
                                        <div class="large-5 large-centered columns tt-fullwidth-loading tt-center-height-wrap text-center" >
                                            <div class="tt-loading-icon small-loading-animate" style=" width: 100%; z-index:1;" ><span class="icon-tt-loading " ></span></div>
                                            <div id="loading-percentage"><span class="percentage-value"></span></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>

                    <!--steps pop-up-->
                    <div id="tt-s-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog" >
                        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                        <h2 id="modalTitle" class="tt-modal-header blue-header">Steps <a href="#" data-reveal-id="tt-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>
                        <div class="large-12 columns steps-range-option " ng-show="output.total_pipe_steps >= 2000">
                                <div class="medium-7 small-12 columns">
                                    <div class="row collapse goto-steps left">
                                        <div class="small-2 medium-2 columns">
                                            <label for="right-label" class="right inline">Range:</label>
                                        </div>
                                        <div class="small-8 medium-8 columns">
                                            <div class="row">
                                                <div class="small-6 columns">
                                                    <input type="number" ng-model="$parent.stepRange.first" ng-keyup="check_key_for_steplist($event, $parent.stepRange.first, $parent.stepRange.last)" class="range-input" placeholder="From" />
                                                </div>
                                                <div class="small-6 columns">
                                                    <input type="number" ng-model="$parent.stepRange.last" ng-keyup="check_key_for_steplist($event, $parent.stepRange.first, $parent.stepRange.last)" class="range-input" placeholder="To" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="small-2 medium-2 columns">
                                            <button class="button postfix tiny tt-button-icond-left tt-button-primary" ng-click="check_key_for_steplist(null, $parent.stepRange.first, $parent.stepRange.last)">Go</button>
                                        </div>
                                    </div>
                                    
                                </div>
                                
                               
                                <div class="medium-5 small-12 columns"> 
                                    <div class="row collapse goto-steps">
                                        <div class="small-10 columns">                                            
                                            <input type="number" placeholder="Go to Step:" ng-model="move_to_pipe" class="range-input" />                                            
                                        </div>
                                        <div class="small-2 columns">
                                            <button class="button postfix tiny tt-button-icond-left tt-button-primary" ng-click="$parent.MoveStepat($event, move_to_pipe, false, null, false)">Go</button>
                                        </div>
                                      </div>
                                    
                                </div>

                            </div>
                        <div class="tt-modal-content large-12 columns">
                            <div data-equalizer class="large-12 columns steps_list_content">
                                    <div class="nothing hide">
                                        <div class="large-5 large-centered columns tt-fullwidth-loading tt-center-height-wrap text-center" >
                                            <div class="tt-loading-icon small-loading-animate" style=" width: 100%; z-index:1;" ><span class="icon-tt-loading " ></span></div>
                                            <div class="loading-percentage"><span class="percentage-value"></span></div>
                                        </div>
                                    </div>
                                    <ul style="list-style:none;" class="hide">
                                        <li ng-repeat="step in $parent.steps_list " id="step_{{$index + 1}}_modal" class="{{output.current_step == $index + 1 ? 'current' : '' }} step_{{$index + 1}}_modal"  >
                                            <a ng-click="saveMultisAndGetPipeData($parent.getCorrectStep($index + 1))"
                                               ng-style="{'background-color': (output.current_step == $index + 1 ? '#c77e30' : ''), 'color': (output.current_step == $index + 1 ? 'white' : ($parent.enabledLastStep > 0 && $parent.enabledLastStep >= $index + 1 ? step[1] : '')) }"
                                               ng-class="{'disabled-element not-active' : (($index + 1) > $parent.enabledLastStep && $parent.enabledLastStep > 0)}"
                                               ng-bind-html="getHtml(step[0])"></a>
                                        </li>
                                    </ul>
                                    <div ng-if="$parent.fStepList2Long && steps_list == null" ng-init="addToptosteplist()" class="text-center columns large-12">
                                        <span class="text text-center"><%=TeamTimeClass.ResString("msgStepsListMessage")%></span>
                                    </div>
                            </div>
                        </div>
                    </div>
                    <!--End steps pop-up-->
                   <!-- // modal wrap -->         
                    <!-- help odal wrap -->
                    <div id="tt-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal anytime-steps-help-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                         <a class='close-reveal-modal second-modal-btn'>&#215;</a>
                        <h2 id="modalTitle" class="tt-modal-header blue-header">Judgment Links Info</h2>
                        <div class="tt-modal-content large-12 columns">

                            <div class="row">
                                <div class="large-12 columns">
                                    <p>The current step is displayed with an <span class="label orange">orange</span> background. The step numbers are colored as follows:</p>
                                </div>
                            </div>
                            <div class="row steps-legend">                              
                                <div class="small-12 columns">
                                        <div class="row collapse">
                                            <div class="small-2 medium-2 columns normal-red">Red </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-9 medium-9 columns">Judgment has not yet been made</div>
                                            <div class="small-2 medium-2 columns normal-blue">Blue </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-9 medium-9 columns">Results or information steps</div>
                                            <div class="small-2 medium-2 columns">Black </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-9 medium-9 columns">Judgment has been made</div>
                                        </div>
                                    </div>
                            </div>
                        </div>
                    </div>
                    <!-- //help modal wrap -->

                </div>

    <!-- start multi pw dropdown-->
     <div ng-class="{'medium' : has_long_content(output.parent_node_info)}"
         id="gdrop0" class="f-dropdown dropdown-layout-fix anytime-multi-verbal question-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
        <div class="row collapse">
                 
                <div class="small-9 columns text-center"> 
                    {{output.parent_node}}
                </div>
                <div class="small-3 columns text-center">
                    <div class="row collapse">
                        <div class="small-6 columns text-center">
                            <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" data-node-type="-1" 
                               data-location="-1" data-node="parent-node" data-node-description="{{output.parent_node}}">
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
            <div class="infotext-wrap large-12 columns parent-node-info-text parent-node-info-div"  ng-bind-html="getHtml(output.parent_node_info)"></div>
        </div>
    </div>
    <div ng-repeat="data in multi_data" class="large-12 columns" ng-if="output.page_type == 'atAllPairwise'">  
        <div 
            ng-class="{'medium' : has_long_content(data.InfodocLeft), 'drop-top': $index >= multi_data.length-1 }"
            id="gdrop{{$index}}_1" class="f-dropdown dropdown-layout-fix anytime-multi-verbal alternative-dropdown"  data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                     <div class="small-9 columns text-center">
                        {{data.LeftNode}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                            <div class="small-6 columns text-center">
                                <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn" data-index="{{$index}}" 
                                   data-node-type="2" data-location="1" data-node="left-node" 
                                   data-node-description="{{data.LeftNode}}" data-node-id="{{data.NodeID_Left}}">
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
                <div class="infotext-wrap large-12 columns node-{{data.NodeID_Left}}-info-text left-node-{{data.NodeID_Left}}-info-text" ng-bind-html="getHtml(data.InfodocLeft)"></div>
            </div>
        </div>


        <div 
            ng-class="{'medium' : has_long_content(data.InfodocLeftWRT), 'drop-top': $index >= multi_data.length-1}"
            id="gdrop{{$index}}_2" class="f-dropdown dropdown-layout-fix anytime-multi-verbal alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                     <div class="small-9 columns text-center">
                    {{get_wrt_text('left')}}
                    </div>
                    <div class="small-3 columns text-center">
                        <div class="row collapse">
                                <div class="small-6 columns text-center">
                            <a ng-class="is_AT_owner == 1 ? '' : 'hide'" data-node-type="3" data-index="{{$index}}" 
                               data-location="1" data-node="wrt-left-node" data-node-title="{{data.LeftNode}}" 
                               data-node-id="{{data.NodeID_Left}}" data-node-description="{{output.parent_node}}" 
                               class="edit-info-doc-btn">
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
            <div class="large-12 columns wrt-node-{{data.NodeID_Left}}-info-text wrt-left-node-{{data.NodeID_Left}}-info-text" ng-bind-html="getHtml(data.InfodocLeftWRT)"></div>
            </div>

        </div>

        <div ng-class="{'medium' : has_long_content(data.InfodocRight), 'drop-top': $index >= multi_data.length-1}"
            id="gdrop{{$index}}_3" class="f-dropdown dropdown-layout-fix anytime-multi-verbal alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                             <div class="small-9 columns text-center">
                                {{data.RightNode}}
                            </div>
                            <div class="small-3 columns text-center">
                                <div class="row collapse">
                                    <div class="small-6 columns text-center">
                                        <a ng-class="is_AT_owner == 1 ? '' : 'hide'" class="edit-info-doc-btn"
                                            data-index="{{$index}}" data-node-type="2" data-location="2" data-node="right-node" 
                                            data-node-id="{{data.NodeID_Right}}" data-node-description="{{data.RightNode}}" >
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
                    <div class="large-12 columns  node-{{data.NodeID_Right}}-info-text right-node-{{data.NodeID_Right}}-info-text" ng-bind-html="getHtml(data.InfodocRight)"></div>
                    </div>
        </div>
        <div
             ng-class="{'medium' : has_long_content(data.InfodocRightWRT), 'drop-top': $index >= multi_data.length-1}"
             id="gdrop{{$index}}_4" class="f-dropdown dropdown-layout-fix anytime-multi-verbal alternative-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                     <div class="small-9 columns text-center">
                        {{get_wrt_text('right')}}
                    </div>
                    <div class="small-3 columns text-center">
            <div class="row collapse">
                <div class="small-6 columns text-center">
                    <a ng-class="is_AT_owner == 1 ? '' : 'hide'" data-node-type="3" data-index="{{$index}}"
                        data-location="2" data-node="wrt-right-node" data-node-title="{{data.RightNode}}" 
                        data-node-description="{{output.parent_node}}" data-node-id="{{data.NodeID_Right}}"
                        class="edit-info-doc-btn" >
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
                <div class="large-12 columns wrt-node-{{data.NodeID_Right}}-info-text wrt-right-node-{{data.NodeID_Right}}-info-text"   ng-bind-html="getHtml(data.InfodocRightWRT)"></div>
            </div>
        </div>
        
        <div aria-autoclose="false" ng-click="$event.preventDefault(); $event.stopPropagation();" id="comment-{{$index}}" class="comment-dropdowns f-dropdown stay-open-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row">
                <div class="small-12 columns text-left">
                        <div class="row collapse">
                <div class="small-9 columns text-left">
                    Add Your Comment
                            </div>
                   

                            <div class="small-2 columns text-center">
                                <a href="#" ng-click="close_multi_comment($index, 'close')"><span class="icon-tt-close"></span></a>
                            </div>
                        </div>
                </div>
                <div class="small-12 columns">
                    <textarea ng-model="multi_data[$index].Comment" name="" rows="4" placeholder="Add your comment"></textarea>
                   <%-- <textarea class="multi_comment_{{$index}}"  ng-init="temp_comments[$index] = multi_data[$index].Comment"  ng-model="temp_comments[$index]" name="" rows="4" placeholder="Add your comment"></textarea>--%>
                </div>
                <div class="large-12 columns text-right">
                   <%-- <a ng-click="close_multi_comment($index, 'close')" href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                <span class="icon-tt-close icon"></span>
                                <span class="text">Cancel</span>
                            </a>--%>
                    <a ng-click="close_multi_comment($index,'save')" href="#" class="button tiny tt-button-icond-left button tiny tt-button-primary success">
                        <span class="icon-tt-check icon"></span>
                        <span class="text">OK</span>
                    </a>
                </div>
            </div>
        </div>
    </div>
    <!-- end multi pairwiseGraphical dropdown-->
    
    <!-- start multi ratings dropdown-->
    <%-- global comment modal for mobile --%>
         <div id="mobile-multi-tt-c-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">Your Comment About The Judgment</h2>

            <div class="tt-modal-content">
                <div class="row">
                    <div class="large-12 columns">
                        <%--<textarea 
                            ng-if="output.page_type == 'atNonPWAllChildren' || output.page_type == 'atNonPWAllCovObjs'" 
                            class="multi_comment_{{active_multi_index}}"  
                            ng-init="temp_comments[active_multi_index] = output.multi_non_pw_data[active_multi_index].Comment" 
                            ng-model="temp_comments[active_multi_index]"
                            rows="10" name="" placeholder="Please enter your comment.">
                        </textarea>
                         <textarea 
                             ng-if="output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes'" 
                             class="multi_comment_{{active_multi_index}}"  
                             ng-init="temp_comments[active_multi_index] = multi_data[active_multi_index].Comment" 
                             ng-model="temp_comments[active_multi_index]"
                             rows="10" name="" placeholder="Please enter your comment.">
                        </textarea>--%>

                        <textarea 
                            ng-if="output.page_type == 'atNonPWAllChildren' || output.page_type == 'atNonPWAllCovObjs'" 
                            class="multi_comment_{{active_multi_index}}"  
                            ng-model="output.multi_non_pw_data[active_multi_index].Comment"
                            rows="10" name="" placeholder="Please enter your comment.">
                        </textarea>
                         <textarea 
                             ng-if="output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes'" 
                             class="multi_comment_{{active_multi_index}}"  
                             ng-model="multi_data[active_multi_index].Comment"
                             rows="10" name="" placeholder="Please enter your comment.">
                        </textarea>
                    </div>
                    <div class="large-12 columns">
                        <%--<div class="small-6 columns text-left">
                            <a ng-click="close_all_modal('close')" href="#" class="button tiny tt-button-icond-left cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                <span class="icon-tt-close icon"></span>
                                <span class="text">Cancel</span>
                            </a>
                        </div>--%>
                        <div class="columns text-right ">
                            <a ng-click="close_all_modal('save')" href="#" class=" button tiny tt-button-icond-left tt-button-primary success">
                                <span class="icon-tt-save icon"></span>
                                <span class="text">OK</span>
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    <%-- //end of global comment  --%>


    <div ng-repeat="data in output.multi_non_pw_data" class="large-12 columns" ng-if="output.page_type == 'atNonPWAllChildren' || output.page_type == 'atNonPWAllCovObjs'">  
        <div 
            aria-autoclose="false"
            ng-class="{'medium' : has_long_content(data.Infodoc)}"
            id="gdrop{{$index}}_1" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1" class="">
            <div class="row collapse">
                <div class="small-12 columns">
                    <div class="row">
                        <div class="small-9 columns text-center">
                        {{data.Title}}
                        </div>
                        <div class="small-3 columns text-center">  
                                      <a ng-class="is_AT_owner == 1 ? '' : 'hide'" data-index="{{$index}}" data-node-type="2" 
                                         data-location="1" data-node="left-node" data-node-description="{{data.Title}}" 
                                         data-node-id="{{$index + 1}}" class="edit-info-doc-btn">                      
                                        <span class="icon-tt-edit"></span>
                                      </a> 
                            &nbsp;<a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a>
                        </div>
                    </div>
                </div>
                <hr>
            </div>
            <div class="row">
                <div class="infotext-wrap large-12 columns node-{{$index + 1}}-info-text left-node-{{$index + 1}}-info-text" ng-bind-html="getHtml(data.Infodoc)"></div>
            </div>
        </div>


        <div 
             ng-class="{'medium' : has_long_content(data.InfodocWRT)}"
            id="gdrop{{$index}}_2" class="f-dropdown dropdown-layout-fix" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row collapse">
                <div class="small-12 columns">
                    <div class="row">
                       <div class="small-9 columns text-center">
                        {{get_wrt_text('left')}}
                        </div>
                        <div class="small-3 columns text-center">
                        <a ng-class="is_AT_owner == 1 ? '' : 'hide'" data-node-type="3" data-index="{{$index}}" 
                           data-location="1" data-node="wrt-left-node" data-node-title="{{data.Title}}" 
                           data-node-id="{{$index + 1}}" data-node-description="{{output.parent_node}}" 
                           class="edit-info-doc-btn">
                            <span class="icon-tt-edit"></span>
                        </a> 
                           &nbsp;<a href="#" class="force-close-drowdown"><span class="icon-tt-close"></span></a> 
                        </div>
                    </div>
                </div>
                <hr>
            </div>
            <div class="row">
            <div class="large-12 columns wrt-node-{{$index + 1}}-info-text wrt-left-node-{{$index + 1}}-info-text" ng-bind-html="getHtml(data.InfodocWRT)"></div>
            </div>

        </div>
        
        <div ng-click="$event.preventDefault(); $event.stopPropagation();" id="comment-{{$index}}" class="f-dropdown stay-open-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
            <div class="row">
                <div class="small-12 columns text-left">
                        Add Comment 
                </div>
                <div class="small-12 columns">
                    <%--<textarea  class="multi_comment_{{$index}}"  ng-init="temp_comments[$index] = output.multi_non_pw_data[$index].Comment" ng-model="temp_comments[$index]" name="" rows="4" placeholder="Add your comment"></textarea>--%>
                     <textarea ng-model="output.multi_non_pw_data[$index].Comment" name="" rows="4" placeholder="Add your comment"></textarea>
                </div>
                <div class="large-12 columns text-right">
                  <%--  <a ng-click="close_multi_comment($index, 'close')" href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                            <span class="icon-tt-close icon"></span>
                            <span class="text">Cancel</span>
                        </a>--%>
                    <a ng-click="close_multi_comment($index, 'save')" href="#" class="button tiny tt-button-icond-left button tiny tt-button-primary success">
                        <span class="icon-tt-check icon"></span>
                        <span class="text">OK</span>
                    </a>
                </div>
            </div>
        </div>
    </div>
                                
    
    <!-- end multi ratings dropdown-->
        <div id="MobileHelpModal" class="reveal-modal tt-modal-wrap small" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">{{mobile_help_data[0]}}</h2>
            <div class="tt-modal-content">
                <div class="large-12 columns">{{mobile_help_data[1]}}</div>
            </div>
        </div>
        
        <div id="SelectStepModal" class="reveal-modal tt-modal-wrap small" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog" data-options="close_on_background_click:false;close_on_esc:false;" style="min-height: 150px;">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">Welcome back</h2>

            <div class="tt-modal-content" style="min-height: 130px;">
                <div class="row">
                    <div class="small-12 columns panel text-center">
                        <span class="icon-tt-info-circle"></span>
                        <span>What step would you like to start with?</span>
                    </div>
                
                    <div class="columns">
                        <div class="columns text-center" ng-if="output.first_unassessed_step > 0"
                             ng-class="current_step == 1 ? 'medium-6' : 'medium-4'">
                            <a  style="line-height: inherit; font-size: 10px;"
                                ng-click="closeModal('SelectStepModal'); get_pipe_data(output.first_unassessed_step)"
                                class="button tiny tt-button-icond-left cn-btn-modal tt-button-primary expand">
                                First Unassessed
                            </a>
                        </div>
                        <div class="columns text-center"
                             ng-class="current_step == 1 || output.first_unassessed_step == 0 ? 'medium-6' : 'medium-4'">
                            <a  style="line-height: inherit; font-size: 10px; top: 4px; right: 0px;"
                                class="button tiny tt-button-icond-left cn-btn-modal tt-button-primary expand close-reveal-modal">
                                This Step
                            </a>
                        </div>
                        <div class="columns text-center" ng-if="current_step > 1"
                             ng-class="output.first_unassessed_step == 0 ? 'medium-6' : 'medium-4'">
                            <a  style="line-height: inherit; font-size: 10px;"
                                ng-click="closeModal('SelectStepModal'); get_pipe_data(1)"
                                class="button tiny tt-button-icond-left cn-btn-modal tt-button-primary expand">
                                First Step
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    
    <div id="PipeStepNotFoundModal" class="reveal-modal tt-modal-wrap small" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog" data-options="close_on_background_click:false;close_on_esc:false;" style="min-height: 150px;">
        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        <h2 id="modalTitle" class="tt-modal-header blue-header">Message</h2>

        <div class="tt-modal-content" style="min-height: 130px;">
            <div class="row">
                <div class="small-12 columns panel text-center">
                    <span class="icon-tt-info-circle"></span>
                    <span>Sorry, but the results for the selected cluster can't be found for this user.</span>
                </div>
                
                <div class="columns">
                    <div class="medium-4 medium-offset-4 columns text-center ">
                        <a  style="line-height: inherit; font-size: 10px;"
                            class="close-reveal-modal button tiny tt-button-icond-left cn-btn-modal tt-button-primary expand">
                            OK
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="AutoAdvanceModal" class="reveal-modal tt-modal-wrap small" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog" data-options="close_on_background_click:false;close_on_esc:false;" style="min-height: 150px;">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">Confirmation</h2>

            <div class="tt-modal-content" style="min-height: 130px;">
                <div class="row">
                    <div class="small-12 columns panel text-center">
                        <span class="icon-tt-info-circle"></span>
                        <span>The auto advance feature can speed up the judgment process. You can turn it on at any time. Do you wish to turn it on now?</span>
                    </div>
                
                    <div class="columns">
                        <div class="medium-4 columns text-center">
                            <a style="line-height: inherit; font-size: 10px;"
                                ng-click="output.is_auto_advance = true; set_auto_advance()"
                                class="button tiny tt-button-icond-left cn-btn-modal tt-button-primary expand">
                                Yes
                            </a>
                        </div>
                        <div class="medium-4 columns text-center ">
                            <a  style="line-height: inherit; font-size: 10px;"
                                class="close-reveal-modal button tiny tt-button-icond-left cn-btn-modal tt-button-primary expand">
                                No
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        
        <!-- START copy and paste modals -->
        <div id="copyModal"  class="reveal-modal tt-modal-wrap  tt-edit-content small" aria-labelledby="modalTitle" aria-hidden="true" role="dialog" data-reveal>
                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                <h2 class="tt-modal-header blue-header">Information</h2>
                <div class="tt-modal-content large-12 columns">
                    <div class="row">                                    
                        <div class="large-12 columns text-center">
                                
                                <span class="icon-tt-info icon"></span><p>Data copied to your clipboard.</p>
                        </div>
                        
                        <div class="large-12 columns">
                        <div class="large-12 columns">
                            <hr />
                            <a href="#" class="button tiny tt-button-icond-left close-reveal-modal expand tt-button-primary success lr-btn-adjustments right">
                                    <span class="icon-tt-check icon"></span>
                                    <span class="text">OK</span>
                                </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div id="pasteModal"  class="reveal-modal tt-modal-wrap  tt-edit-content small" role="dialog" data-reveal>
                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                <h2 class="tt-modal-header blue-header">Paste from Clipboard</h2>
                <div class="tt-modal-content large-12 columns">
                    <div class="row">
                        <div class="large-12 columns text-center">
                                <p id="paste-message" ng-model="pasteMessage">{{pasteMessage}}</p>
                        </div>
                        <div class="large-12 columns">
                            <textarea id="ClipBoardData" ng-model="ClipBoardData" autofocus class="" ></textarea>
                        </div> 
                        <div class="large-12 columns">
                            <a class="button tiny tt-button-icond-left close-reveal-modal tt-button-primary success paste-message-a lr-btn-adjustments right" ng-click="pasteJudgmentTable()" style="position:relative">
                                <span class="icon-tt-check icon paste-message-icon"></span>
                                <span class="text">Go</span>
                            </a>
                        </div> 
                    </div>
                </div>
        </div>
        <div id="copyModalMobile"  class="reveal-modal tt-modal-wrap  tt-edit-content small" aria-labelledby="modalTitle" aria-hidden="true" role="dialog" data-reveal>
                <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                <h2 class="tt-modal-header blue-header">Copy all data</h2>
                <div class="tt-modal-content large-12 columns">
                    <div class="row">                                    
                        <div class="large-12 columns text-center">
                                <span class="icon-tt-info icon"></span>
                            <div>Select and copy the text below</div>
                        </div>
                        
                        <div class="large-12 columns">
                            <textarea id="copy-clipboard" ng-model="output.PipeParameters.ClipBoardData" class="" autofocus rows="{{output.PipeParameters.results_data.length}}"></textarea>
                        </div>
                        <div class="large-12 columns">
                            <div><a href="#" class="button tt-button-icon-left close-reveal-modal expand tt-button-primary success lr-btn-adjustments right">
                                    <span class="icon-tt-check icon"></span>
                                    <span class="text">OK</span>
                                </a>
                                </div>
                        </div>
                </div>
            </div>
        </div>
                    
                    
        <!-- END copy and paste modals -->

          <ul
              ng-if="output.page_type == 'atShowLocalResults'"
               id="local-dropdown" class="f-dropdown text-left" data-dropdown-content aria-hidden="true" tabindex="-1">
              <li class="hide">
                  <a class="close-reveal-modal" aria-label="Close">×</a>
              </li>
            <li class="columns">
                <div class="columns"><label><input type="checkbox" ng-change="setMatrixRank(matrixbox.rank)" ng-model="matrixbox.rank"/> Rank</label></div>
                <div class="columns"><label><input type="checkbox" ng-model="output.bestfit" ng-change="savebestfit(output.bestfit)"/> Best Fit</label></div>
                <div class="columns"><label><input type="checkbox" ng-model="matrixbox.legend" ng-change="setMatrixLegend(matrixbox.legend)" ng-if="output.PipeParameters.MeasurementType == 'ptVerbal'"/> {{output.PipeParameters.MeasurementType == 'ptVerbal' ? 'Legend' : ''}}</label></div>
                <div class="columns"><label><input type="checkbox" ng-model="orderbypriority" ng-init="orderbypriority=output.orderbypriority" ng-change="saveOBPriority(orderbypriority)" /> Order by priority</label></div>
                <div class="columns"><label><input type="radio" id="reviewjudgment" ng-model="inconsistency" ng-value="true" ng-change="saveInconsistency(true)" ng-click="saveInconsistency(true)"/> Review all judgments in cluster</label></div>
                <div class="columns"><label><input type="radio" id="changejudgment" ng-model="inconsistency" ng-value="false" ng-change="saveInconsistency(false)" ng-click="saveInconsistency(false)"/> Make changes on this screen</label></div>
            </li>
        </ul>
    </div> 

    <div ng-if="$parent.projectLockedInfo.status" class="columns project-locked-message large-offset-2 large-8 medium-offset-1 medium-10 small-12">
        <div class="locked-message">{{$parent.projectLockedInfo.message}}</div>
        <div ng-if="$parent.projectLockedInfoteamTimeUrl.length = 0" class="locked-reload">
            <input type="button" class="button tt-button-primary btn-reload" value="Reload" ng-click="checkProjectLockStatus()" />
        </div>
        <div ng-if="$parent.projectLockedInfoteamTimeUrl.length = 0" class="locked-reload" class="locked-reload-message hide">Reloading...</div>
    </div>

<script src="/Content/bower_components/Chart.js/dist/Chart.min.js" ng-if="output.page_type == 'atNonPWOneAtATime' && (output.non_pw_type=='mtStep' || output.non_pw_type=='mtRegularUtilityCurve' || output.non_pw_type=='mtAdvancedUtilityCurve')"></script>

<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/bundles/anytimeController") %>
    <%: Scripts.Render("~/bundles/pipeExtension") %>
</asp:PlaceHolder>
<%--<script src='https://cloud.tinymce.com/stable/tinymce.min.js'></script>--%>
<script src="/Content/tinymce/tinymce.min.js"></script>
</span>
<script>
    $(document).ready(function () {
        $(".footer-nav-mobile").removeClass("down");
        $(".fnv-toggler .icon").removeClass("icon-tt-sort-up");
        $(".fnv-toggler").removeClass("down");
        $(".footer-nav-mobile").addClass("up");
        $(".fnv-toggler").addClass("up");
        $(".fnv-toggler .icon").addClass("icon-tt-sort-down");
 
        //toggle comments
        $(document).on('click', '.toggleComments', Foundation.utils.debounce(function (e) {
            var comBtn = $('.comBtnDesktop'); //the comments button
            var leftCol = $('.tt-left-col'); //the comments wrap
            var rightCol = $('.tt-right-col'); //the right content wrap
            var leftBtn = $('.tc-btn-left');
            var rightBtn = $('.tc-btn-right');

            comBtn.toggleClass('hide');
            leftBtn.toggle();
            rightBtn.toggle();
            rightCol.toggle();
 

        }, 100, true));
        
        $("#dragCom").draggable(); //{ containment: ".tt-content-wrap", scroll: false }

        $(".anytime-question-header").attr("data-magellan-expedition", "fixed");
  
    });


</script>

<script type="text/ng-template" id="menu_sublevel.html">
    <div class="hierarchy" ng-style="{'color': getNodeColor(hierarchy[0])}">
    <span ng-class="hierarchy[3] == 1 ? 'icon-tt-minus-circle tt-sm-icon' : 'icon-tt-plus-circle tt-sm-icon'" ng-click="hierarchy[3] = hierarchy[3] == 1 ? 0 : 1" ng-hide="hierarchy.length <= 5"></span>
    <span ng-click="MoveToPipe(getCorrectStep(hierarchy[4]))">
    <b class="bolded">{{hierarchy[1]}} </b>
    <span style="color: gray;">{{hierarchy[4] != 0 ? '(#' + hierarchy[4] + ')' : ''}}</span></div>
    <ul ng-if="hierarchy[3] == 1">
        <li ng-repeat="hierarchy in hierarchy[5]"  ng-include="'menu_sublevel.html'">
        </li>
    </ul>
</script>
 
</asp:Content>