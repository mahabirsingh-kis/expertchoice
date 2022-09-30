<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Footer.ascx.cs" Inherits="AnytimeComparion.Pages.includes.Footer" %>
<%-- Footer --%>
<!-- desktop footer here -->
<div class="tt-judgements-footer-nav desktop visible-for-large-up" ng-if="is_under_review && is_pipescreen" ng-cloak>
    <div class="columns large-2 text-center large-centered" >
        <a class="columns button large tt-button-primary" style="margin-bottom:5px" ng-click="move_to_cluster()">Next</a>
    </div>
</div>
<div class="tt-judgements-footer-nav desktop visible-for-large-up" ng-if="!is_under_review && is_pipescreen" ng-cloak>
    <div class="large-5 medium-12 columns" style="padding-right: 0;">

            <div class="large-12 columns" ng-if="is_anytime">             
                <button ng-if="(!output.pipeOptions.disableNavigation && !output.pipeOptions.hideNavigation)" ng-click="load_hierarchies()" ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active' : ''" type="button" class="button tiny tt-button-icond-left tt-button-primary left tt-h-btn radius" data-reveal-id="tt-h-modal">
                    <span class="icon-tt-site-map icon"></span>
                    <span class="text">Current<span class="hide-for-small-down"> Cluster</span></span>
                </button>
                <div ng-if="output.pipeOptions.disableNavigation || output.pipeOptions.hideNavigation" style="float: left; width: 50px;">&nbsp;</div>

                <button ng-if="(!output.pipeOptions.disableNavigation && !output.pipeOptions.hideNavigation)" ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active' : ''" type="button"  class="steps_modal_btn button tiny tt-button-icond-left tt-button-primary left tt-s-btn radius" data-reveal-id="tt-s-modal" ng-click="checkStepList()">
                    <span class="icon-tt-th-list icon"></span>
                    <span class="text">Steps 
                        <span class="currentStepInt">{{output.current_step}}/{{output.total_pipe_steps}}
                        </span>
                    </span>
                </button>
                <div ng-if="output.pipeOptions.disableNavigation || output.pipeOptions.hideNavigation" style="float: left; width: 50px;">&nbsp;</div>

                <!-- Insert Steps Help Button for Anytime -->
                <a href="#" ng-if="!output.pipeOptions.hideNavigation" data-reveal-id="tt-help-modal" class="left steps-help-icon-large-screen"><span class="icon-tt-question-circle"></span></a>               
                <!-- End Insert Steps Help Button for Anytime -->
                <span class="tt-evaluated-wrap"><strong>Evaluated: </strong> 
                    <span class="evaluatedj">
                        <span ng-if="is_anytime">{{output.judgment_made }}/{{output.total_evaluation}}</span>
                    </span>
                </span>

                <div id="overall" class="tt-c100 p{{output.overall | number:0}} size-30">
                    <span id="overallj" title="{{getEvaluationProgressPercentage(output.overall, true)}}%">
                        {{getEvaluationProgressPercentage(output.overall, false)}}%
                    </span>
                    <div class="slice">
                        <div class="bar"></div>
                        <div class="fill"></div>
                    </div>
                </div>

                <span  ng-if="output.pipeOptions.hideNavigation" class="tt-evaluated-wrap"><strong>Step: </strong> 
                    <span class="evaluatedj">
                        <span ng-if="is_anytime">{{output.current_step}}/{{output.total_pipe_steps}}</span>
                    </span>
                </span>
            </div>

            <div class="large-12 columns" ng-if="is_teamtime">
                <button ng-if="output.isPM" type="button" class="button tiny tt-button-icond-left tt-button-primary left tt-h-btn radius" data-reveal-id="tt-h-modal">
                    <span class="icon-tt-site-map icon"></span>
                    <span class="text">Current<span class="hide-for-small-down"> Cluster</span></span>
                </button>
                <button ng-if="output.isPM" type="button"  class="steps_modal_btn button tiny tt-button-icond-left tt-button-primary left tt-s-btn radius" data-reveal-id="tt-s-modal" ng-click="scrolltoelement(output.current_step, 0)">
                    <span class="icon-tt-th-list icon"></span>
                    <span class="text">Steps 
                        <span class="currentStepInt">{{output.current_step}}/{{steps_list.length}}
                        </span>
                    </span>
                </button>
                <!-- Insert Steps Help Button for Teamtime -->
                <a href="#" data-reveal-id="tt-help-modal" class="left steps-help-icon-large-screen"><span class="icon-tt-question-circle"></span></a>               
                <!-- End Insert Steps Help Button for Teamtime -->
                <span ng-if="output.isPM" class="tt-evaluated-wrap"><strong>Evaluated </strong> 
                    <span class="evaluatedj">
                        <span>{{output.evaluation[1]}}/{{output.evaluation[0]}}</span>
                    </span>
                </span>

                <div class="left tt-eval-steps tt-s-btn" ng-if="!output.isPM">
                    <span><strong>Steps:</strong> <span >{{output.current_step}}</span> of {{output.totalPipe}} </span><span><strong>Evaluated:</strong> <span class="evaluatedj">{{output.evaluation[1]}}/{{output.evaluation[0]}}</span></span>
                </div>
                <div id="overall" class="tt-c100 p{{output.evaluation[2] | number:0}} size-30">
                    <span id="overallj" title="{{getEvaluationProgressPercentage(output.evaluation[2], true)}}%">
                        {{getEvaluationProgressPercentage(output.evaluation[2], false)}}%
                    </span>
                    <div class="slice">
                        <div class="bar"></div>
                        <div class="fill"></div>
                    </div>
                </div>
                <a href="#" class="see-others-percentage-eval" ng-click="view_evaluation()" data-reveal-id="othersPrecentageModal" ng-if="output.isPM">
                    <span class="icon-tt-list-alt" ></span>  
                </a>
            </div>
        

        

    </div>

    <div class="large-7 medium-12 columns tt-pagination text-right">
        <ul class="pagination">
            <div class="paginate-container">
                <%-- Anytime Footer --%>    
                <ul class="pagination" ng-if="is_anytime">
                    <li ng-if="!output.pipeOptions.hideNavigation" class="has-tip tip-top" data-tooltip ng-attr-title="{{'Last visited step'}}">
                        <a href="" ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active' : ''" id="last_visited"  ng-click="MoveStepat($event, output.previous_step, output.current_step == output.previous_step)" style="background-color:#539ddd" ><span class="icon-tt-step-back icon {{output.current_step == output.previous_step ? 'not-active' : ''}}" style="background-color:transparent; color:#fff;"></span></a></li>
                    <li ng-click="MoveStepat($event, output.current_step - 1, false, null, true)" class="arrow steps" ng-if="output.current_step != 1"><a href="" id="prev_step"><span class="icon-tt-chevron-left icon"></span> prev</a></li>
                    <li  ng-repeat="step in stepButtons" 
                        id="step_{{::step[0]}}" 
                        class="  step_{{::step[0]}} " 
                        ng-class="output.current_step == step[0] ? 'current ' : ''"
                        data-id="{{::step[0]}}"

                        ng-if="!output.pipeOptions.hideNavigation && ((output.current_step == step[0]) || (get_step_display(step[0], output.current_step, output.total_pipe_steps, 'unavailable')) || get_step_display(step[0], output.current_step, output.total_pipe_steps, 'available'))">
                        <a class="step-{{::step[0]}} "
                            ng-class="output.pipeOptions.disableNavigation ? ' not-active ' : ''"
                            href="#" ng-click="MoveStepat($event, step[0], false, null, false)"  ng-if="output.current_step == step[0]">{{::step[0]}}</a>
                        <a href="#" ng-if="get_step_display(step[0], output.current_step, output.total_pipe_steps, 'unavailable')" 
                            ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active ' : ''"
                            data-options="align:top" data-dropdown="drop" ng-click="setFocusonElement('.txttofocus')">&hellip; </a>
                        <a class="step-{{::step[0]}}" 
                            ng-class="output.pipeOptions.disableNavigation || checkStepStatus($index, step[0], step[3]) ? 'disabled-element not-active' : ''"
                            href="#" ng-click="MoveStepat($event, step[0], false, null, false)"
                            ng-if="get_step_display(step[0], output.current_step, output.total_pipe_steps, 'available')"
                            ng-style="{'color':  step[1] }">{{::step[0]}}</a>
                    </li>
                    <li ng-if="output.current_step != output.total_pipe_steps && !$parent.disableNext" ng-class="$parent.disableNext ? 'disabled-element footer-btn not-active' : ''" ng-click="MoveStepat($event, output.current_step + 1, false, null, false)" class="arrow steps for-auto-advance">
                        <a href=""  id="next_step"  ng-class="$parent.disableNext ? 'disabled-element' : ''" class="next_step">next <span class="icon-tt-chevron-right icon"></span> </a>
                    </li>
                    <li ng-if="output.current_step != output.total_pipe_steps && $parent.disableNext" ng-click="MoveStepat($event, output.current_step, false, null, false)" class="arrow steps for-auto-advance">
                        <a href="" id="save_step" class="save-step">save <span class="icon-tt-save icon"></span> </a>
                    </li>
                    <li data-from="pipe"  ng-if="output.current_step == output.total_pipe_steps" class="arrow">
                        <%--desktop --%>
                        <a ng-if="!output.fromComparion && showFinish"  href="" id="next_step" class="next_step" ng-click="finish_anytime(<%=(int) Session[AnytimeComparion.Pages.external_classes.Constants.Sess_LoginMethod]%>)"><span class="icon-tt-check icon"></span> finish</a>
                        <a ng-if="output.fromComparion"  href="" id="next_step" class="next_step" ng-click="finish_anytime()"><span class="icon-tt-check icon"></span> done</a>
                        <a ng-if="!output.fromComparion && showSave"  href="" id="save_sussrvey" class="next_step" ng-click="finish_anytime(<%=(int) Session[AnytimeComparion.Pages.external_classes.Constants.Sess_LoginMethod]%>)"><span class="icon-tt-check icon"></span> save </a>
                    </li>
                    <li ng-if="output.pipeOptions.showUnassessed && !output.pipeOptions.disableNavigation" ng-class="$parent.disableNext ? 'disabled-element footer-btn not-active' : ''">
                        <a ng-if="(output.nextUnassessedStep != null) && (output.nextUnassessedStep[0] != output.current_step) && ( unassessed_data[0] != output.current_step || unassessed_data[1] > 1)" ng-click="next_unassessed()" class="">
                            <span class="text">Next<span class="hide-for-small-down"> Unassessed</span></span>
<!--                            <span data-tooltip  data-options="show_on:large" class="has-tip tip-left icon-tt-jump" title="Next Unassessed"></span>-->
                        </a>
                    </li>
                    <li>
                         <a 
                             ng-if="output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && showOptions()"
                             id="aa" href="#" class="right tt-toggler" data-toggler="tg-"><span class="icon-tt-ellipsis-v"></span></a>                      

                         <div class="text-left tg-aa-wrap tg-aa hide">  
                             <label ng-if="(output.page_type=='atPairwise' && output.pairwise_type == 'ptVerbal') || (output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtRatings')" id="gradient-checker-label">
                                <input ng-click="set_auto_advance()" 
                                    ng-model="output.is_auto_advance" type="checkbox" 
                                        />&nbsp; 
                                   <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.auto_advance[1]}}">Auto Advance</span>
                             </label> 

                            <%-- <label ng-if=" is_multi && output.pairwise_type == 'ptVerbal' ">
                                <input ng-click="set_vertical_bars()" ng-model="output.collapse_bars" type="checkbox" 
                                        />&nbsp; 
                                   <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.vertical_bars[1]}}">Collapse Bars</span>
                             </label> --%>

                             <label ng-if="is_teamtime && output.pairwise_type == 'ptGraphical' && !graphical_switch ">
                                <input type="checkbox" ng-model="hide_piechart[0]">&nbsp; 
                                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.hide_pie_chart[1]}}">Hide Pie Chart</span>
                            </label>
                             <label ng-if="output.page_type == 'atShowLocalResults' && is_AT_owner == 1 && ExpectedValue[2] == '1'">
                                <input type="checkbox" ng-model="showExpectedValue.Value">&nbsp; 
                                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.expected_value[1]}}">Show Expected Value</span>
                             </label>
                             <label  ng-if="(output.page_type == 'atShowLocalResults' || output.page_type == 'atShowGlobalResults') && is_AT_owner == 1 && is_anytime">
                                <input type="checkbox" ng-model="pipeOptions.showIndex" ng-change="showResultsIndex(pipeOptions.showIndex, output.page_type == 'atShowGlobalResults')">
                                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="Show Index">Show Index</span>
                             </label>
                             <label class="hide" ng-if="output.page_type=='atPairwise' && output.pairwise_type == 'ptGraphical' ">
                                <input type="checkbox" ng-model="graphical_switch"  ng-change="switch_graphical(graphical_switch)">&nbsp; 
                                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.single_line[1]}}">Single Line Mode</span>
                             </label>
                             <label 
                                 ng-if="is_anytime && output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults' && output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && output.showinfodocnode"
                                 id="gradient-checker-label">
                                <input ng-click="set_infodoc_mode(output.is_infodoc_tooltip)" 
                                    ng-model="output.is_infodoc_tooltip" type="checkbox" 
                                        />&nbsp; 
<!--                                   <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.tooltip_mode[1]}}">Tooltip Mode </span> -->
                                   <span class="" title="{{help_descriptions.tooltip_mode[1]}}">Tooltip Mode </span> 
                             </label>
                             <label ng-if="is_anytime && is_AT_owner && (output.page_type=='atPairwise' || output.page_type=='atAllPairwise')" id="gradient-checker-label">
                                 <input ng-click="setAutoFitInfoDocImages(autoFitInfoDocImages)" ng-model="autoFitInfoDocImages" type="checkbox" 
                                 />&nbsp; 
                                 <span class="">{{autoFitInfoDocImagesOptionText}}</span> 
                             </label>
                             <label ng-if="is_AT_owner == 1 && !is_html_empty(qh_text)">
                                <input ng-click="show_qh_automatically(!output.show_qh_setting)" type="checkbox" ng-model="output.show_qh_setting">&nbsp; 
                                <span data-tooltip aria-haspopup="true" class="has-tip tip-left" title="{{help_descriptions.quick_help[1]}}">Show Quick Help Automatically</span>
                            </label>
                        </div>
                    </li>
                </ul>

                
                    
                <%-- Teamtime Footer --%>
                <ul class="pagination" ng-if="is_teamtime && output.isPM">
                    <li class="arrow steps" ng-if="output.current_step != 1"><a ng-click="MovePipeStep(output.current_step == 1 ? output.current_step : output.current_step - 1)" id="PreviousStep"><span class="icon-tt-chevron-left icon"></span> prev</a></li>
                    <li ng-repeat="step in steps_list" 
                        id="step_{{$index + 1}}" 
                        class="  step_{{$index + 1}}" 
                        ng-class="output.current_step == $index + 1 ? 'current' : ''"
                        data-id="{{$index + 1}}"
                        ng-if="(output.current_step == $index + 1) || (get_step_display($index + 1, output.current_step, output.totalPipe, 'unavailable')) || get_step_display($index + 1, output.current_step, output.totalPipe, 'available')">
                        <a ng-click="MovePipeStep($index + 1)" class="steps" ng-if="output.current_step == $index + 1">{{$index + 1}}</a>
                        <a href="#" ng-if="get_step_display($index + 1, output.current_step, output.totalPipe, 'unavailable')" 
                            data-options="align:top" data-dropdown="drop" ng-click="setFocusonElement('#move_to_pipe')">&hellip; </a>
                        <a ng-click="MovePipeStep($index + 1)" class="steps"
                            ng-if="get_step_display($index + 1, output.current_step, output.totalPipe, 'available')"
                            ng-style="{'color': step[1]}">{{$index + 1}}</a>
                    </li>
                    <li ng-if="output.current_step != output.totalPipe" class="arrow steps"><a ng-click="MovePipeStep(output.current_step == output.totalPipe ? output.current_step : output.current_step + 1)" id="NextStep">next <span class="icon-tt-chevron-right icon"></span> </a></li>
                    <li>
                        <a data-reveal-id="timeDetails" class="button tiny tt-button-icond-left tt-button-primary left tt-h-btn unassessed">
                            <span class="text">Time Details</span>
                        </a>
                    </li>
                </ul>
                <div id="drop" class="small content f-dropdown tt-dropdown tt-jump-to-pipe-wrap" data-dropdown-content>
                    <div class="row collapse">
<!--                        <div class="small-8 columns small-centered">-->
                            <div class="medium-4 columns large-text-left small-text-center text">Jump to step:</div>
                            <div class="medium-3 columns large-text-left small-text-center">
                                <input class="txttofocus" type="number" id="move_to_pipe" ng-model="move_to_pipe" ng-keypress="MoveToPipe(getCorrectStep(move_to_pipe), $event)" min="1" max="{{enabledLastStep > 0 ? enabledLastStep : output.total_pipe_steps}}">
                            </div>
                            <div class="medium-2 columns large-text-right small-text-center">
                                <input type="button" class="button tiny tt-button-primary success " value="OK" ng-click="MoveToPipe(getCorrectStep(move_to_pipe))">
                            </div>
                            <div class="medium-3 columns large-text-right small-text-center">
                                <input type="button" class="button tiny tt-button-primary cancel" value="CANCEL" data-dropdown="drop">
                            </div>

<!--                        </div>-->
                    </div>
                </div>
            </div>
        </ul>

<%--            <ul class="pagination">
                <li><a href="" id="prev_step"><span class="icon-tt-chevron-left icon"></span> prev</a></li>
                <li><a href="" id="">1</a></li>
                <li><a href="" id="">2</a></li>
                <li><a href="" id="">3</a></li>
                <li><a href="" id="next_step">next <span class="icon-tt-chevron-right icon"></span></a></li>
            </ul>--%>
    </div>

</div>



<%-- //desktop footer --%>

<%-- Mobile Footer --%>
<div class="tt-mobile-wrap hidden-for-large-up tt-mobile-footer" ng-if="is_pipescreen" ng-cloak>
    <div class="row collapse">
        <div class="large-12 columns tt-judgements-footer-nav mobile ">
            <%-- Footer --%>
            <div class="row collapse footer-nav-mobile up {'at-customer-footer' : is_anytime, 'tt-custom-footer': is_teamtime}" style="z-index: 10 !important;">
                <button type="button" class="fnv-toggler tt-toggler up" data-toggler="fmobile" ng-click="expandForPSA()"><span class="icon-tt-sort-down icon"></span></button>
                <div class="pagination-wrap columns">
                    <div class="small-8 medium-9 columns mobile-paginator">
                        <div class="paginate-container">
                            <%-- Anytime MOBILE --%>
                            <ul class="pagination small-2 medium-1 columns" ng-if="!is_under_review && is_anytime">
                                <li   class="left arrow">
                                    <a href="" ng-if="output.current_step != 1" ng-click="MoveStepat($event, output.current_step - 1, output.current_step == 1, null, true)" id="prev_step" style="margin-left:0rem;margin-right: 0px !important;" class="a prev_step">prev</a>
                                </li>
                            </ul>
                            <ul class="pagination small-8 medium-10 columns small-centered text-center end" ng-if="!is_under_review && is_anytime">
                                <li ng-repeat="step in stepButtons" 
                                    id="step_{{::step[0]}}" 
                                    class="steps step_{{::step[0]}} text-center" 
                                    ng-class="output.current_step == step[0] ? 'current' : ''"
                                    data-id="{{::step[0]}}"
                                    style="display: inline-block;  float:none; margin:0px !important"
                                    ng-if="(!output.pipeOptions.hideNavigation) && ((output.current_step == step[0]) || (get_step_display(step[0], output.current_step, output.total_pipe_steps, 'unavailable', true)) || get_step_display(step[0], output.current_step, output.total_pipe_steps, 'available', true))">
                                    <a href="#" style="margin-left:0rem;" class="steps step-{{::step[0]}} " ng-click="MoveStepat($event, step[0], false, null, false)" ng-if="output.current_step == step[0]">{{::step[0]}}</a>
                                    <a href="#" style="margin-left:0rem;" 
                                        ng-if="get_step_display(step[0], output.current_step, output.total_pipe_steps, 'unavailable', true)" 
                                        ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active ' : ''"
                                        ng-click="setFocusonElement('.txttofocus2')" data-options="align:bottom" data-reveal-id="GoToStep" class="">&hellip;</a>
                                    <a  href="#" style="margin-left:0rem;" class="steps step-{{::step[0]}}" ng-click="MoveStepat($event, step[0], false, null, false)" 
                                        ng-if="get_step_display(step[0], output.current_step, output.total_pipe_steps, 'available' , true) "
                                        ng-class="output.pipeOptions.disableNavigation || checkStepStatus($index, step[0], step[3]) ? 'disabled-element not-active' : ''"

                                        ng-style="{'color': step[1] }">{{::step[0]}}</a>
                                </li>
                            </ul>
                            <ul class="pagination small-2 medium-1 columns end" ng-if="!is_under_review && is_anytime">
                                <li  ng-if="output.current_step != output.total_pipe_steps" ng-class="$parent.disableNext ? 'disabled-element footer-btn not-active' : ''"
                                    ng-click="MoveStepat($event, output.current_step + 1, output.current_step == output.total_pipe_steps, null, false)" class="right arrow for-auto-advance" style="margin-left:0px !important;">
                                    <a href="" ng-class="$parent.disableNext ? 'disabled-element' : ''" style="margin-left:0rem;" id="next_step" class="next_step">next</a>
                                </li>

                                <li  data-from="pipe" ng-if="output.current_step == output.total_pipe_steps"  class="arrow right ">
                                    <%--mobile --%>
                                    <a ng-if=" !output.fromComparion && showFinish"  href="" id="next_step" class="next_step" ng-click="finish_anytime(<%=(int) Session[AnytimeComparion.Pages.external_classes.Constants.Sess_LoginMethod]%>)"> finish</a>
                                    <a ng-if="output.fromComparion"  href="" id="next_step" class="next_step" ng-click="finish_anytime()"> done</a>
                                    <a ng-if="!output.fromComparion && showSave"  href="" id="save_sussrvey" class="next_step" ng-click="finish_anytime(<%=(int) Session[AnytimeComparion.Pages.external_classes.Constants.Sess_LoginMethod]%>)"> save </a>
                                </li>
                            </ul>
                            <%-- Anytime MOBILE --%>
                            <%-- Teamtime MOBILE --%>
                                <ul class="pagination small-2 medium-1 columns" ng-if="is_teamtime && output.isPM">
                                    <li ng-click="MovePipeStep(output.current_step == 1 ? output.current_step : output.current_step - 1)" ng-if="output.current_step != 1" class=" left arrow">
                                        <a href="" id="prev_step" style="margin-left:0rem;" class="prev_step">prev</a>
                                    </li> 
                                </ul>   
                                <ul class="pagination small-8 medium-10 columns small-centered text-center" style="font-size:inherit; line-height: inherit;" ng-if="is_teamtime && output.isPM">
                                    <li ng-repeat="step in steps_list" 
                                        id="Li1" 
                                        class="step_{{$index + 1}}" 
                                        ng-class="output.current_step == $index + 1 ? 'current' : ''"
                                        data-id="{{$index + 1}}"
                                        ng-if="(!output.pipeOptions.hideNavigation) && ((output.current_step == $index + 1) || (get_step_display($index + 1, output.current_step, output.totalPipe, 'unavailable', true)) || get_step_display($index + 1, output.current_step, output.totalPipe, 'available', true))">
                                        <a ng-click="MovePipeStep($index + 1)" class="steps" ng-if="output.current_step == $index + 1">{{$index + 1}}</a>
                                        <a href="#" style="margin-left:0rem;" ng-if="get_step_display($index + 1, output.current_step, output.totalPipe, 'unavailable', true)" ng-click="setFocusonElement('.txttofocus2')" data-options="align:bottom" 
                                            ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active ' : ''" data-dropdown="drop" class="" data-reveal-id="GoToStep">&hellip; </a>
                                        <a ng-click="MovePipeStep($index + 1)" class="steps"
                                            ng-if="get_step_display($index + 1, output.current_step, output.totalPipe, 'available', true)"
                                            ng-style="{'color': step[1] }">{{$index + 1}}</a>
                                    </li>
                                </ul>
                                <ul class="pagination small-2 medium-1 columns" ng-if="is_teamtime && output.isPM">
                                    <li  ng-if="output.current_step != output.totalPipe" ng-click="MovePipeStep(output.current_step == output.totalPipe ? output.current_step : output.current_step + 1)" class="  arrow for-auto-advance">
                                        <a href="" style="margin-left:0rem;" id="next_steps" class="next_step">next</a>
                                    </li>
                                </ul>
                                <ul class="pagination small-12 medium-12 columns" ng-if="is_teamtime && !output.isPM">
                                    <li>
                                        <span><strong>Steps:</strong> <span >{{output.current_step}}</span> of {{output.totalPipe}} </span><span><strong>Evaluated:</strong> <span class="evaluatedj">{{output.evaluation[1]}}/{{output.evaluation[0]}}</span></span>
                                    </li> 
                                </ul>                                
                            <%-- Teamtime MOBILE --%>
                            <ul class="pagination columns" ng-if="is_under_review ">
                                <li ng-click="move_to_cluster()" class=" arrow for-auto-advance" class="small-12 columns text-center small-centered">
                                    <a href="" style="margin-left:0rem;" id="next_step" class="next_step">next</a>
                                </li>
                            </ul>                            
                        </div>
                    <%-- Move to Modal --%>

                    </div >                    
                    <div class="small-3 medium-2 columns mobile-icons text-center">
                        <ul class="inline-list" ng-class="{'right': $scope.screen_sizes.option > 1}">
                            <li ng-if="!output.pipeOptions.hideNavigation" class="the-steps-help-list"><a href="#" data-reveal-id="tt-help-modal" class="steps-help-icon-small-screen"><span class="icon-tt-question-circle"></span></a></li>
                            <li>
                                <div class=" overall tt-c100 p{{output.overall | number:0}} size-30 circ-graph" style="margin-left:10px;" ng-if="is_anytime">
                                    <span class="overallj" title="{{getEvaluationProgressPercentage(output.overall, true)}}%">
                                        {{getEvaluationProgressPercentage(output.overall, false)}}%
                                    </span>
                                    <div class="slice">
                                        <div class="bar"></div>
                                        <div class="fill"></div>
                                    </div>
                                </div>
                                <div class="overall tt-c100 p{{output.evaluation[2] | number:0}} size-30 circ-graph" ng-if="is_teamtime">
                                    <span class="overallj" title="{{getEvaluationProgressPercentage(output.evaluation[2], true)}}%">
                                        {{getEvaluationProgressPercentage(output.evaluation[2], false)}}%
                                    </span>
                                    <div class="slice">
                                        <div class="bar"></div>
                                        <div class="fill"></div>
                                    </div>
                                </div>
                            </li>                        
                            <li class="the-ellipsis">
                                <%-- ||  (is_AT_owner == 0 && ( (output.page_type=='atAllPairwise') || output.page_type == 'atNonPWAllChildren' || output.page_type == 'atNonPWAllCovObjs')) --%>
                                <a ng-if="is_teamtime && output.isPM && output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && showOptions()" ng-click="toggleEllips()" ng-model="ellipsis"
                                    id="aa" class=" tt-toggler" href="#" data-toggler="tg-">
                                    <span class="icon-tt-ellipsis-v"></span></a>
                                <a ng-if="is_anytime && output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && showOptions()" 
                                    id="aa" href="#" class="small-12 columns  tt-toggler" data-toggler="tg-">
                                    <span class="icon-tt-ellipsis-v"></span>
                                </a>
                            </li>
                         </ul>
                    </div>
                         <div class=" tg-aa-wrap tg-aa toggle-ellipsis hide ">  
                             <label ng-if="(output.page_type=='atPairwise' && output.pairwise_type == 'ptVerbal') || (output.page_type=='atPairwise' && output.pairwise_type == 'ptVerbal') || (output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtRatings') " id="gradient-checker-label">
                                <input ng-click="set_auto_advance()" 
                                    ng-model="output.is_auto_advance" type="checkbox" 
                                        />&nbsp; 
                                   Auto Advance
                                  <a ng-click="open_help_modal(help_descriptions.auto_advance)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                             </label> 
                              <label ng-if=" is_multi && output.pairwise_type == 'ptVerbal' && is_AT_owner == 1 ">
                                <input ng-click="set_vertical_bars()" ng-model="output.orig_collapse_bars" type="checkbox" 
                                        />&nbsp; 
                                     <span>Collapse Bars</span>
                                    <a ng-click="open_help_modal(help_descriptions.vertical_bars)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                             </label> 
                             <label ng-if="is_teamtime && output.pairwise_type == 'ptGraphical' || output.mtype =='ptGraphical' && !graphical_switch ">
                                <input type="checkbox"  ng-model="TTOptions.hidePie" ng-change="hidePieChart(TTOptions.hidePie)" >&nbsp; 
                                <span>Hide Pie Chart</span>
                                <a ng-click="open_help_modal(help_descriptions.hide_pie_chart)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
                             <label ng-if="(output.page_type == 'atShowLocalResults' || output.page_type == 'atShowGlobalResults') && is_AT_owner == 1 && is_anytime">
                                <input type="checkbox" ng-model="pipeOptions.showIndex" ng-init="pipeOptions.showIndex = output.PipeParameters.showIndex" ng-change="showResultsIndex(pipeOptions.showIndex, output.page_type == 'atShowGlobalResults')">&nbsp; 
                                <span>Show Index</span>
                                 <a ng-click="open_help_modal(help_descriptions.show_index)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                             </label>
 
                            <label ng-if="output.page_type == 'atShowLocalResults' && is_AT_owner == 1 && is_anytime && ExpectedValue[2] == '1'">
                                <input type="checkbox" ng-model="showExpectedValue.Value">&nbsp;
                                <span>Show Expected Value</span>
                                <a ng-click="open_help_modal(help_descriptions.expected_value)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
                             <label class="hide" ng-if="output.page_type=='atPairwise' && output.pairwise_type == 'ptGraphical' && is_anytime">
                                <input type="checkbox" ng-model="graphical_switch"  ng-change="switch_graphical(graphical_switch)">&nbsp;
                                <span>Single Line Mode</span>
                                <a ng-click="open_help_modal(help_descriptions.single_line)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                             </label>
                             <label id="gradient-checker-label" ng-if="is_anytime && output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults' && output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && output.showinfodocnode">
                                <input ng-click="set_infodoc_mode(output.is_infodoc_tooltip)" 
                                    ng-model="output.is_infodoc_tooltip" type="checkbox" 
                                        />&nbsp; 
                                   Tooltip Mode 
                                 <a ng-click="open_help_modal(help_descriptions.tooltip_mode)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                             </label>
                             <label id="gradient-checker-label" ng-if="is_anytime && is_AT_owner && (output.page_type=='atPairwise' || output.page_type=='atAllPairwise')">
                                 <input ng-click="setAutoFitInfoDocImages(autoFitInfoDocImages)" ng-model="autoFitInfoDocImages" type="checkbox" 
                                 />&nbsp; 
                                 {{autoFitInfoDocImagesOptionText}}
                             </label>
                             <label ng-if="is_anytime && (output.page_type=='atPairwise' || output.page_type=='atAllPairwise') && is_AT_owner && !output.is_infodoc_tooltip" id="gradient-checker-label">
                                 <input ng-click="show_framed_info_docs(framed_info_docs)" ng-model="framed_info_docs" type="checkbox"/>&nbsp;
                                 <span class="">Show Left And Right Framed Infodocs</span>
                             </label>
                             <label ng-if="is_AT_owner == 1 && is_anytime && !is_html_empty(qh_text)" >
                                <input ng-click="show_qh_automatically()" type="checkbox" ng-model="output.show_qh_automatically">&nbsp;
                                <span>Show Quick Help Automatically</span>
                                 <a ng-click="open_help_modal(help_descriptions.quick_help)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
                            <label ng-if="is_teamtime && output.isPM">
                                <input class="anonymous-checkbox"  type="checkbox" name="anony">&nbsp;
                                <span>Anonymous Mode</span>
                                <a ng-click="open_help_modal(help_descriptions.anonymous_mode)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>

                            <label ng-if="is_teamtime && output.isPM">
                                <input  class="resize-checkbox" type="checkbox" name="">&nbsp;
                                <span>Resize Infodocs for All</span>
                                <a ng-click="open_help_modal(help_descriptions.resize_info_docs)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>

                            <label ng-if="is_teamtime && isPM">
                                <input  class="infodocs-checkbox" type="checkbox" name="">&nbsp;
                                <span>Toggle Infodocs for All</span>
                               
                                <a ng-click="open_help_modal(help_descriptions.toggle_info_docs)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
                            <label ng-if="is_teamtime && isPM">
                                <input class="online-checkbox" ng-model="TTOptions.hideOffline" ng-change="hideOfflineUsers(TTOptions.hideOffline)" type="checkbox" name="nline">&nbsp;
                                <span>Hide Offline Users</span>
                                <a ng-click="open_help_modal(help_descriptions.hide_offline_users)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
<%--                            <label ng-if="is_teamtime && isPM">--%>
<%--                                <input type="checkbox" class="hide-judgment-checkbox" ng-model="TTOptions.hideJudgment" ng-change="setHiddenJUdgmentForPM(TTOptions.showToMe)" >&nbsp;--%>
<%--                                <span>Hide Judgments</span>--%>
<%--                                --%>
<%--                            </label>--%>
                            <label ng-if="is_teamtime && isPM">
                                <input type="checkbox" name="cb-showOverallResult" class="hide-judgment-checkbox" ng-model="TTOptions.hideJudgment" ng-change="setHiddenJUdgmentForPM(TTOptions.showToMe)">&nbsp;
                                <span>Hide Judgments</span>
                            </label>
                            <label ng-if="is_teamtime && isPM && TTOptions.hideJudgment">
                                <input type="radio" name="cb-showOverallResult" class="hide-radio default-radio" ng-model="TTOptions.showToMe" ng-change="setHiddenJUdgmentForPM(TTOptions.showToMe)" ng-value="false">&nbsp;
                                <span>Hide Judgments (for All)</span>
                                <a ng-click="open_help_modal(help_descriptions.hide_for_all)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
                            <label ng-if="is_teamtime && isPM && TTOptions.hideJudgment">
                                <input type="radio" name="cb-showOverallResult" class="hide-radio me-radio" ng-model="TTOptions.showToMe" ng-change="setHiddenJUdgmentForPM(TTOptions.showToMe)" ng-value="true">&nbsp;
                                <span>Hide Judgments (Show to Me)</span>
                                <a ng-click="open_help_modal(help_descriptions.show_to_me)" href="#" class="right question-icon-wrap"><span  class="icon-tt-question-circle "></span></a>
                            </label>
                            <label ng-if="is_teamtime">
                                 <input data-reveal-id="timeDetails" type="checkbox" class="hide-judgment-checkbox">&nbsp;
                                <span>View Time Details</span>
                            </label>
                             <label ng-if="is_teamtime && isPM">
                                 <input data-reveal-id="othersPrecentageModal" ng-click="view_evaluation()" type="checkbox" class="hide-judgment-checkbox">&nbsp;
                                <span>View Evaluation Progress</span>
                            </label>
                        </div>
                </div>
                <div class="hsc-wrap columns footer-bottom-btns" ng-if="is_anytime">
                    <div 
                        ng-click="load_hierarchies()"
                        ng-class="{
                            'small-3': output.no_footer_options == 4,
                            'small-4': output.no_footer_options == 3,
                            'small-6': output.no_footer_options == 2
                        }"
                        class="columns text-center">
                        <a href="#" ng-if="(!output.pipeOptions.disableNavigation && !output.pipeOptions.hideNavigation)"
                            ng-class="{
                                'disabled-element footer-btn not-active': output.pipeOptions.disableNavigation, 
                                'not-active-footer-navs' : is_under_review
                            }"
                            data-reveal-id="tt-h-modal" ng-if="(!output.pipeOptions.hideNavigation)">
                            <span class="icon-tt-site-map  tt-icon-15"></span><span class="icon-text">Current<span class="hide-for-small-down"> Cluster</span></span>
                        </a>
                    </div>
                    <div 
                        ng-class="{
                            'small-3': output.no_footer_options == 4,
                            'small-4': output.no_footer_options == 3,
                            'small-6': output.no_footer_options == 2,
                            'not-active-footer-navs' : is_under_review
                        }"
                        class="columns text-center">
                        <a href="#" ng-if="(!output.pipeOptions.disableNavigation && !output.pipeOptions.hideNavigation)" 
                           ng-class="output.pipeOptions.disableNavigation ? 'disabled-element footer-btn not-active' : ''" 
                           data-reveal-id="tt-s-modal" ng-if="(!output.pipeOptions.hideNavigation)" class="steps_modal_btn" ng-click="checkStepList()">
                            <span class="icon-tt-th-list  tt-icon-15"></span><span class="icon-text">Steps</span>
                        </a>
                    </div>          
                        <div 
                        ng-class="{
                            'not-active' : $parent.disableNext,
                            'not-active-footer-navs' : is_under_review,
                            'small-3': output.no_footer_options == 4,
                            'small-4': output.no_footer_options == 3,
                            'small-6': output.no_footer_options == 2
                        }" ng-style="{'background' : $parent.disableNext ? 'rgba(153, 153, 153, 0)' : ''}"
                         ng-if="(output.pipeOptions.showUnassessed && !output.pipeOptions.disableNavigation && output.nextUnassessedStep != null) && (output.nextUnassessedStep[0] != output.current_step) && ( unassessed_data[0] != output.current_step || unassessed_data[1] > 1)" class="columns text-center">
                        <a type="button" ng-click="next_unassessed()" ng-style="{'color': $parent.disableNext ? 'rgba(255, 255, 255, 0.5)' : ''}" class="next-unassessed-btn">
                            <span class="icon-tt-chevron-right  tt-icon-15"></span><span class="icon-text">Next Unassessed<span class="hide-for-small-down"></span></span>
                        </a>
                    </div>
                </div>
                <div class="hsc-wrap columns" ng-if="is_teamtime">
                    <div class="small-4 columns text-center" ng-if="output.isPM"><a href="#" data-reveal-id="tt-h-modal"><span class="icon-tt-site-map tt-icon-15"></span><span class="icon-text">Hierarchy</span></a></div>
                    <div class="small-4 columns text-center" ng-if="output.isPM"><a href="#" data-reveal-id="tt-s-modal" ng-click="scrolltoelement(output.current_step, 0)"><span class="icon-tt-th-list tt-icon-15"></span><span class="icon-text">Steps</span></a></div>
                    <div ng-class="output.isPM ? 'small-4 columns text-center' : 'small-12 columns text-center'" ng-if="!isRestricted"><a href="#" class="toggleComments smoothScrollLink" data-link="#comWrap" ng-click="scrolltoTTelement('tt-comments-wrap', 1000)"><span class="icon-tt-comments  tt-icon-15"></span><span class="icon-text">Comments</span></a></div>
                </div>
            </div>
            <%--<div class="row collapse details-wrap pipe-footer" ng-if="is_anytime">
                <div ng-if="!is_multi" class="medium-6 columns">
                    <label>
                        <input ng-click="set_auto_advance(output.is_auto_advance)" 
                        ng-model="output.is_auto_advance" type="checkbox">
                        <span>Auto Advance</span>
                    </label>
                </div>
                <div class="medium-6 columns">
                    <label ng-if="output.pairwise_type == 'ptGraphical'">
                        <input type="checkbox" ng-model="hide_piechart[0]" >
                        <span  >Hide Pie Chart</span>
                    </label>
                </div>
                <div class="medium-6 columns">
                    
                    <label ng-if="output.page_type == 'atShowLocalResults'">
                        <input type="checkbox" ng-model="showExpectedValue.Value">
                        <span  >Show Expected Value</span>
                    </label>
                </div>                                        
                <div class="large-12 columns text-center">
                    <a class="close-details button tt-button-primary tiny">close</a>
                </div>

            </div>
            <div class="row collapse details-wrap pipe-footer" ng-if="is_teamtime && output.isPM">
                <div class="medium-6 columns">
                    <div class="columns pipe-settings">
                        <label>
                            <input class="anonymous-checkbox"  type="checkbox">
                            <span>Anonymous Mode</span>
                        </label>
                    </div>
                    <div class="columns pipe-settings">
                        <label>
                            <input class="online-checkbox" type="checkbox">
                            <span>Hide Offline Users</span>
                        </label>
                    </div>
                    <div class="columns hide-pie-chart hide pipe-settings">
                        <label >
                            <input class="hide-pie-checkbox"  type="checkbox">
                            <span>Hide Pie Chart</span>
                        </label>
                    </div>
                </div>
                <div class="medium-6  tt-overall-result-wrap columns">
                    <div class="columns pipe-settings">
                        <label>
                            <input type="checkbox" class="hide-judgment-checkbox" name="cb-showOverallResult">
                            <span>Hide Judgments</span>
                        </label>
                    </div>
                    <div class="columns mobile-result tt-j-show-overall-result hide">
                        <div class="all">
                            <label>
                                <input type="radio" name="me-all" class="hide-radio default-radio" value="all">
                                <span>Hide for All</span>
                            </label>
                        </div>
                        <div class="me-only">
                            <label>
                                <input type="radio" name="me-all" class="hide-radio me-radio" value="me">
                                <span>Show to Me</span>
                            </label>
                        </div>
                    </div>
                </div>
            </div>--%>
            <%-- //Footer --%>
        </div>
    </div>
</div>
<%-- // Mobile Footer --%>


<%-- End of Footer --%>
