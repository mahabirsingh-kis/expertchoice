<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LocalResults.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.LocalResults" %>
<%@ Register Src="~/Pages/includes/QHicons.ascx" TagPrefix="includes" TagName="QHicons" %> 
 <%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>
<link href="/Content/stylesheets/results.css" rel="stylesheet" />
<script src="../../Scripts/responsive-tables.js"></script>

<div id="IntermediateResults" class="tt-mobile-wrap direct-comparison-wrap-mobile" 
    ng-if="((output.PipeParameters.canshowresults.individual || output.PipeParameters.canshowresults.combined) && 
    check_shown_results(output.PipeParameters.canshowresults.individual, output.PipeParameters.canshowresults.combined)
    && !output.PipeParameters.equalMessage)" ng-init="matrixReview ? show_inconsistency_view() : ''" ng-cloak>

    <div class="columns small-centered small-12 hide-for-large-up hide results-main-header-mobile">
            <a class="left text-center" ng-click="restoreDefaultView()">
                <span class="icon-tt-chevron-left  tt-icon-15"></span>
            </a>
                <span id="results-title" ng-class="intermediate_screen == 2 ? 'matrix-options right' : 'matrix-redo-options'"></span>
    </div>      
            <%-- start new design --%>  
            <!--- Quick help for mobile -->
           <!-- <div ng-show="intermediate_screen == 3" class="row hide-for-large-up">
                  <div class="columns">
                        <includes:QHicons runat="server" ID="QHicons1" />
                  </div> 
            </div>-->
            <!--- End Quick help for mobile -->
        <div class="row large-uncollapse medium-collapse" id="results-instruction">
            <div class="small-12 columns large-text-left small-text-right large-1 medium-1 the-toggler hide-for-large-up">
                <a class="tt-toggler left" id="1" data-toggler="tg-accordion">
                    <span class="icon" ng-class="screens < 600 && $parent.show_check_box ? 'icon-tt-plus-square' : 'icon-tt-minus-square'"></span>                   
                </a>
            </div>
            <div class="small-12 medium-11 large-11 columns tg-accordion-1" ng-class="screens < 600 && $parent.show_check_box ? 'hide' : ''">                 
                <div class="tt-panel small-text-left tt-local-results-header large-text-center" style="border: none;">
                    <includes:QuestionHeader runat="server" id="QuestionHeader" />
                </div>
            </div>
            <div style="padding-top: 10px;" ng-if="output.PipeParameters.showTitle"
                class="small-12 medium-11 large-11 columns tg-accordion-1 hide-for-medium-down">
                <div class="tt-panel small-text-left tt-local-results-header large-text-center" style="border: none;">
                    <div class="tg-legend">
                        <div class="small-12 columns text-center ">
                            <h2 class="no-margin">
                                <span style="font-size: 18px;" ng-bind-html="getHtml(output.PipeParameters.resultsTitle)"></span>
                            </h2>
                        </div>
                    </div>
                </div>
            </div>
        </div>
            <%-- end new design --%> 
    <%-- Collapse Normalization & Sorting Desktop --%> 
                            <div class="row large-uncollapse medium-collapse hide-form-medium-down hide-normalization" id="normalization">
                                <div class="columns large-text-left small-text-right large-1 small-1 the-toggler" ng-if="!output.PipeParameters.CurrentNode[2]" >
                                     <a class="tt-toggler left" id="2" data-toggler="tg-accordion" >
                                        <span class="icon icon-tt-minus-square"></span>
                                        <span class="text "></span>
                                    </a>
                                </div>
                                <div class="columns large-11 small-11" ng-class="screens < 600 && $parent.show_check_box ? 'hide' : ''"> <!-- tg-accordion-2 -->
                                    <div class="tt-panel large-text-center small-text-left" style="border: none;">
                                        <div class="left small-3  priorities-cbr" ng-if="!output.PipeParameters.CurrentNode[2]" style="margin-bottom: 0px !important;">
                                            <span class="sv-label left" style="font-size:12px">Normalization: </span>
                                            <select class="priorities-view-result sv-sorter "  ng-change="normalization_Change(normalization, false)" ng-model="normalization" ng-options="obj.text for obj in normalization_list track by obj.value"></select>
                                        </div>
                                        <div class="  left small-6 priorities-cbr show-for-medium-down" style="padding-left:8px; margin-bottom: 0px !important;">
                                             <span class="sv-label left" style="font-size:12px">Sort By: 
                                                <a >
                                                    <span style="font-size:12px" ng-class="reverseLocal == null ? 'icon-tt-sort-toggle' : !reverseLocal ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                ng-click="set_reverse()"></span>
                                                </a>
                                            </span>
						                    <select class="priorities-view-result sv-sorter " ng-model="columnsortLocal"  ng-change="sort_Results(columnsortLocal[0])" ng-options="column[1] for column in columns track by column[0]">
							                    <option value="" selected hidden />
						                    </select>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <%-- End collapse Normalization & Sorting Desktop--%>
        <div class="pipe-wrap columns tt-auto-resize-content">            
            <div class="row">

                <div class="large-12 results-main" ng-style="{'padding-right' : '0px'}">
                    <div class="row tt-j-title text-center">
                        <h3>{{ JSON_results[0] }}</h3>
                    </div>
                          <%-- Normalization & Sorting in Dropdown Mobile Only--%>
                                <div class="row sensitivity-drop-row hide-for-large-up {{intermediate_screen == 2 ? 'hide' : ''}}">  
                                     <%-- Re-evaluate Mobile --%>
                                     <div class="mobile-re-evaluate show-for-medium-down " ng-show="intermediate_screen == 3">
                                            <a class="button radius lr-btn-adjustments left" ng-class="checked_boxes== 2 ? '' : 'disabled-element'" ng-click="reEvaluate(output.parentnodeID, output.current_step)">
                                             <span class="icon-tt-edit"></span> Re-evaluate</a>
                                     </div>
                                   <%-- End Re-evaluate Mobile --%>
                                    <a data-options="align:left" data-dropdown="drop001" aria-controls="drop2" aria-expanded="false" class="button tt-button-primary lr-normalization-dropdown"><span class="icon-tt-chevron-down"></span></a>
                                    <div id="drop001" data-dropdown-content class="f-dropdown medium" aria-hidden="true" aria-autoclose="false" tabindex="-1">
                                           <div class="row large-uncollapse medium-collapse hide-form-medium-down" id="normalization">
                                                    <div class="columns large-text-left small-text-right large-1 small-1 the-toggler hide" ng-if="!output.PipeParameters.CurrentNode[2]" 
                                                        <%--ng-show="$parent.show_check_box"--%> >
                                                    </div>
                                                    <div class="columns large-11 small-12" ng-class="screens < 600 && $parent.show_check_box ? 'hide' : ''">
                                                        <div class="tt-panel large-text-center small-text-left" style="border: none;">
                                                            <div class="left small-6  priorities-cbr" ng-if="!output.PipeParameters.CurrentNode[2]" style="margin-bottom: 0px !important;">
                                                                <span class="sv-label left" style="font-size:12px">Normalization: </span>
                                                                <select class="priorities-view-result sv-sorter "  ng-change="normalization_Change(normalization, false)" ng-model="normalization" ng-options="obj.text for obj in normalization_list track by obj.value"></select>
                                                            </div>
                                                            <div class="  left small-6 priorities-cbr show-for-medium-down" style="padding-left:8px; margin-bottom: 0px !important;">
                                                                 <span class="sv-label left" style="font-size:12px">Sort By: 
                                                                    <a >
                                                                        <span style="font-size:12px" ng-class="reverseLocal == null ? 'icon-tt-sort-toggle' : !reverseLocal ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                                    ng-click="set_reverse()"></span>
                                                                    </a>
                                                                </span>
						                                        <select class="priorities-view-result sv-sorter " ng-model="columnsortLocal" ng-change="sort_Results(columnsortLocal[0])" ng-options="column[1] for column in columns track by column[0]">
							                                        <option value="" selected hidden />
						                                        </select>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                    </div>
                               <!-- </div> -->
                            </div>
                        <%-- End Normalization & Sorting in Dropdown Mobile Only --%>

                    <div ng-class="{'many-alternatives' : intermediate_screen !=2 , 'jj-wrap-fix' : intermediate_screen ==2 }"  class="row tt-j-content many-alternatives {{intermediate_screen == 2 ? 'inherit-overflow' : ''}}">  <!-- multi-loop-wrap-local-results -->                     
                        <div class="small-12" ng-style="{'padding-right' : '0px', 'padding-left' : '0px', 'font-size' : $parent.isMobile() ? '90%' : '100%'}">                                                        
                            <div class="row collapse priorities-cbr">                                
                                <table class="tt-table-wrap priorities-cbr hide-for-medium-down"  ng-if="(output.PipeParameters.canshowresults.individual || output.PipeParameters.canshowresults.combined)">
                                    <thead>
                                        <tr>
 
                                            <th class="results-redo-pairs" ng-if="intermediate_screen == 3"></th>
                                            <th class="tt-with-sorter" ng-if="show_check_box">
                                            </th>
                                            <th class="tt-with-sorter" ng-if="output.PipeParameters.showIndex">
                                                <div class="text small-11 columns">No.</div>
                                                <div class="sorter-icons small-1 columns text-center" ng-click="sort_Results('nodeID')">
                                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="tt-with-sorter">
                                                <div class="text small-11 columns">Name</div>
                                                <div class="sorter-icons small-1 columns text-center" ng-click="sort_Results('nodeName')">
                                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="tt-with-sorter ir-result" ng-if="(results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && output.PipeParameters.canshowresults.individual ">
                                                <div class="text small-11 columns">Your Results</div>
                                                <div class="sorter-icons small-1 columns text-center" ng-click="sort_Results('yourResults')">
                                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="tt-with-sorter" ng-if="(results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && output.PipeParameters.canshowresults.combined">
                                                <div class="text small-11 columns">{{output.PipeParameters.columnValueCombined}}</div>
                                                <div class="sorter-icons small-1 columns text-center" ng-click="sort_Results('combine')">
                                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="">
                                                <div class="text small-12 columns">Bar Graph</div>
                                            </th>
                                        </tr>
                                    </thead>

                                    <tbody ng-if="results_data.length > 0" >
                                        
                                   
                                        <tr ng-repeat="result in results_data | orderBy: columnname:reverseLocal">
                                            <td class="hide"></td>
                                            <td class="results-redo-pairs" ng-if="intermediate_screen == 3" style="width:60px">
                                                <div class="columns large-right text-right" >
                                                    <label >
                                                        <input type="checkbox" class="results-main-checkboxes"  ng-model="checkbox_items[result[0]]" ng-init="checkbox_items[result[0]] = false" ng-change="selectCheckbox(checkbox_items[result[0]], result[5])" ng-disabled="checked_boxes >= check_limit && checkbox_items[result[0]] == false" />
                                                    </label>
                                                </div>
                                            </td>
                                            <td ng-if="output.PipeParameters.showIndex">{{result[0]}}</td>
                                            <td>{{result[1]}}</td>
                                            <td id="you_{{$index}}" ng-if="(result[4] == 'rvIndividual' || result[4] == 'rvBoth') && output.PipeParameters.canshowresults.individual" class="blue ir-result">{{result[2] * 100 | number:2}}%</td>
                                            <td id="combine_{{$index}}" ng-if="(result[4] == 'rvGroup' || result[4] == 'rvBoth') && output.PipeParameters.canshowresults.combined" class="green">{{result[3] * 100 | number:2}}%</td>
                                            <td>
                                                <div class="direc-comparison-wrap priorities">
                                                    <div id="blue_{{$index}}" class="priorities-result-bar" ng-if="(result[4] == 'rvIndividual' || result[4] == 'rvBoth') && output.PipeParameters.canshowresults.individual" ng-style="get_results('own_bar', result[2])"></div>
                                                </div>
                                                <div class="direc-comparison-wrap priorities cbr">
                                                    <div id="green_{{$index}}" class="priorities-result-bar green" ng-if="(result[4] == 'rvGroup' || result[4] == 'rvBoth') && output.PipeParameters.canshowresults.combined" ng-style="get_results('combine_bar', result[3])"></div>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>

                                </table>

                                <!-- mobile layout -->                                        
                                <div class="small-12 columns" ng-if="results_data[0][4] == 'rvBoth'">                                                                
                                    <div class="small-6 columns itemLabel text-center show-for-medium-down normal-blue-bg" ng-if="(results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && output.PipeParameters.canshowresults.individual" style="background:#539ddd;color:white;font-size:14px;" >Your Results</div>
                                    <div class="small-6 columns itemLabel text-center show-for-medium-down normal-green-bg" ng-if="(results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && output.PipeParameters.canshowresults.combined" style="background:#a5d490;color:white;font-size:14px;">{{output.PipeParameters.columnValueCombined}}</div>
                                </div>
 

                                <div class="" ng-dbltap>
                                    <div class="large-12 columns single-item show-for-medium-down">
                                        <div class="res-data-repeat" ng-class="checkbox_items[result[0]] && intermediate_screen == 3 ? 'results-row-selected' : ''" ng-repeat="result in results_data  | orderBy: columnname:reverseLocal" ng-click="intermediate_screen == 3 ? selectReevaluateRow(result[0], result[5]) : ''">
                                            <h3 class="itemTitle text-left">
                                            <label>
                                                <input ng-if="intermediate_screen == 3" class="itemCheckbox-{{$index}} hide " type="checkbox" ng-model="checkbox_items[result[0]]" ng-init="checkbox_items[result[0]] = false" ng-change="selectCheckbox(checkbox_items[result[0]], result[5])" ng-disabled="checked_boxes == check_limit && checkbox_items[result[0]] == false" />
                                                <div ng-if="output.PipeParameters.showIndex" class="results-label">{{'' + result[0] + ''}}</div>
                                                <div> &thinsp;{{' ' + result[1]}}</div>
                                            </label>

                                                

                                            </h3>
                                            <div class="row collapse ir-result" title="individual result">
                                                <div class="row collapse" ng-if="output.PipeParameters.canshowresults.individual">
                                                    <div class="small-12 columns progress-bar-wrap mobile"  ng-if="(result[4] == 'rvIndividual' || result[4] == 'rvBoth')">
                                                        <div class="small-9 columns">
                                                            <div class="priorities-result-bar" id="mblue_{{result[0]}}" ng-style="get_results('own_bar', result[2])"></div>

                                                        </div>
                                                        <div class="small-3 columns ">
                                                            <div class="right priorities-text" id="myou_{{result[0]}}">{{result[2] * 100 | number:2}}%</div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="row collapse" title="combined result">
                                                 <div class="small-12 columns text-right" ng-if="output.PipeParameters.canshowresults.combined">
                                                    <div class="progress-bar-wrap mobile cbr" ng-if="result[4] == 'rvGroup' || result[4] == 'rvBoth'">
                                                        <div class=" small-12 columns progress-bar-wrap mobile cbr">
                                                            <div class="small-9 columns">
                                                                <div class="left priorities-result-bar green"  id="mgreen_{{result[0]}}" ng-style="get_results('combine_bar', result[3])"></div>
                                                            </div>
                                                            <div class="small-3 columns  ">
                                                                <div class="right priorities-text" id="mcombine_{{result[0]}}">{{result[3] * 100 | number:2}}%</div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- //mobile layout -->
                                
                            </div>
                            <div class="large-6 columns text-center large-centered results-error hide"  >
                                <div data-alert class="alert-box small alert  round large-centered text-center">
                                  Error! You have not selected a pair of elements.
                                  <a href="#" class="close">&times;</a>
                                </div>
                            </div>

                            <!-- Inconsistency is too high view -->
                            <div class="large-11 small-12 columns large-centered large-text-center small-text-left tt-inconsistency-wrap hide" >    
                                <span class="show-for-large-up">
                                        <includes:QHicons runat="server" ID="QHicons2" />
                                </span>   
                                <h3 class="title show-for-large-up">
                                    Judgment Table
                                    
                                </h3>
                                <p class="show-for-large-up">If elements in the table are sorted from high to low priority, then judgments should generally be increasing in any row from left to right, and in any column from bottom to top. Although exceptions to this pattern are valid, they may indicate a judgment that should be examined for accuracy.</p>
                                 <%-- Judgement Table Mobile Menu --%>
                                 <div class="hide-for-large-up row">
                                     <div class="columns">
                                           <includes:QHicons runat="server" ID="Info_QHicons" />
                                     </div> 
                                 </div>
                                <div class="judgement-table-menu hide-for-large-up">
                                   
                                    <div class="row">
                                        <div class="small-3 columns">Data Options</div>
                                        <div class="small-9 columns text-right">
                                            <a class="button" ng-click="saveOBPriority(true)"><i class="fas fa-sort-amount-down"></i></a>
                                            <a class="button" ng-click="saveOBPriority(false)"><i class="fas fa-list-ol"></i></a>
                                            <a ng-if="!output.isPipeViewOnly" href="#" class="button copy-paste" data-reveal-id="copyModalMobile"><span class="icon-tt-copy"></span></a>
                                            <a ng-if="!output.isPipeViewOnly" href="#" class="button copy-paste" ng-click="open_pasteModal()"><span class="icon-tt-paste"></span></a>                                             
                                            <a ng-if="!output.isPipeViewOnly" class="button" ng-class="output.PipeParameters.JudgmentsSaved ? 'tt-button-green-normal' : 'tt-button-transparent not-active-footer-navs'" ng-click="doMatrixOperation(-1, 3, null , output.parentnodeID)">
                                                  <span class="icon-tt-restore"></span> 
                                            </a>                                            
                                            <a ng-if="!output.isPipeViewOnly" class="button tt-toggler" href="#" data-dropdown="local-dropdown" data-options="is_hover:false; hover_timeout:300">
                                                  <span class="icon-tt-eye"></span><!--icon-tt-ellipsis-v -->
                                            </a>
                                            
                                            <button id="navInvertJudgment" ng-if="!output.isPipeViewOnly" type="button" class="button tt-button-transparent not-active-footer-navs" ng-click="invertCurrentJudgment()">
                                                <span class="icon icon-tt-minus" style="font-size: inherit;"></span>
                                            </button>
                                            
                                            <a class="button" ng-if="!output.isPipeViewOnly" ng-click="doMatrixOperation(-1, 5, null , output.parentnodeID)" ng-if="output.PipeParameters.isAlternative">
                                                  <span class="icon-tt-exchange"></span><!-- icon-tt-back-fort -->
                                            </a>
                                            <%--<a class="button">
                                                  <span class="icon-tt-invert-one"></span><!-- icon-tt-back-fort -->
                                            </a>--%>
                                        </div>
                                    </div>
                                </div>
                                    <%-- End Judgement Table Mobile Menu --%>
                                <div id="tableshouldbe">                                   
                                </div>         
                                <div id="hiddentable" style="display:none">
                                </div>                   
                            </div>

                            <!-- //Inconsistency is too high view -->
                            <div class="small-12 columns text-center " ng-if="showExpectedValue.Value" >
                                <b>
                                    <span class="columns" style="color:#0058a3 ">{{ExpectedValue[0]}}</span>
                                    
                                    <span class="columns" style="color:#6aa84f ">{{ExpectedValue[1]}}</span>
                                </b>
                            </div>
                            <div class="small-12 columns text-center inconsistency" ng-if="output.PipeParameters.InconsistencyRatioStatus && (output.PipeParameters.JudgmentType == 'atAllPairwise' || output.PipeParameters.JudgmentType == 'atPairwise')"><h2 class="bold">Inconsistency Ratio: {{output.PipeParameters.InconsistencyRatio | number:2}}</h2></div>

                            <div class="large-12 columns inconsistency-view-wrap tt-inconsistency-wrap hide">
                                <h3 class="small-12 columns text-center" style="color: red; padding-left: 0;" ng-bind-html="output.PipeParameters.lblredNumbers" ng-if="isRedNumber"></h3>
                                <div class="large-11 columns large-centered hide-for-medium-down" style="padding-left: 0;">
                                    <div class="large-4 columns" style="padding-left: 0;">
                                        <div class="row">
                                            <div class="large-4 column"><label><input type="checkbox" ng-change="setMatrixRank(matrixbox.rank)" ng-model="matrixbox.rank"/> Rank</label></div>
                                            <div class="large-4 column"><label><input type="checkbox" ng-change="setMatrixLegend(matrixbox.legend)" ng-model="matrixbox.legend" ng-if="output.PipeParameters.MeasurementType == 'ptVerbal'"/> {{output.PipeParameters.MeasurementType == 'ptVerbal' ? 'Legend' : ''}}</label></div>
                                            <div class="large-4 column"><label><input type="checkbox" ng-init="bestfit=output.bestfit" ng-model="bestfit" ng-change="savebestfit(bestfit)"/> Best Fit</label></div>
                                        </div>
                                        <div class="row">
                                            <div class="large-5 columns" style="padding-right: 0;"><a class="button tiny tt-button-primary tt-button-green-normal lr-btn-adjustments" style="padding: 6px 7px !important;" ng-click="saveOBPriority(true)"><i class="fas fa-sort-amount-down"></i> Sort by priority</a></div>
                                            <div class="large-7 columns"><a class="button tiny tt-button-primary tt-button-green-normal lr-btn-adjustments" style="padding: 6px 10px !important;" ng-click="saveOBPriority(false)"><i class="fas fa-list-ol"></i> Sort by original order</a></div>
                                        </div>
                                    </div>
                                    
                                    <div ng-if="!output.isPipeViewOnly" class="large-3 columns">
                                        <div><label><input type="radio" name="inconsistency" id="reviewjudgment" ng-model="inconsistency" ng-value="true" ng-change="saveInconsistency(true)" ng-click="saveInconsistency(true)" /> Review all judgments in cluster</label></div>
                                        <div><label>
                                        <input type="radio" ng-model="inconsistency" name="inconsistency" id="changejudgment" ng-value="false"  ng-change="saveInconsistency(false)" ng-click="saveInconsistency(false)"/> Make changes on this screen</label></div>
                                    </div>
                                    <div class="large-5 columns judgement-controls">
                                        <div class="row collapse">
                                            <div ng-if="!output.isPipeViewOnly" class="large-12 columns text-right">
                                                <a href="#" ngclipboard data-clipboard-text="{{output.PipeParameters.ClipBoardData}}" data-reveal-id="copyModal" ng-click="copyJudgmentTable()" class="button tt-button-primary lr-btn-adjustments"><span class="icon icon-tt-copy"></span> Copy</a>
                                                <a href="" class="button tt-button-primary lr-btn-adjustments" ng-click="open_pasteModal()"><span class="icon icon-tt-paste"></span> Paste</a>
                                                <a data-equalizer-watch="btn" class="button tiny tt-button-primary tt-button-transparent local-results-btn lr-btn-adjustments" ng-class="output.PipeParameters.JudgmentsSaved ? 'tt-button-green-normal' : 'tt-button-transparent not-active disabled'" ng-click="doMatrixOperation(-1, 3, null , output.parentnodeID)" style="margin-right:0 !important;"><span class="icon icon-tt-restore"></span> Restore Judgments</a>
                                                <a data-equalizer-watch="btn" class="button tiny tt-button-primary tt-button-green-normal local-results-btn lr-btn-adjustments" ng-click="restoreDefaultView()" style="padding-left:10px !important;" >Priorities</a>
                                                <button id="buttonInvertJudgment" type="button" data-equalizer-watch="btn" class="button tiny tt-button-primary tt-button-transparent not-active disabled local-results-btn lr-btn-adjustments" ng-click="invertCurrentJudgment()" style="margin-right:0 !important;"><span class="icon icon-tt-minus"></span> Invert This Judgment</button>
                                                <a data-equalizer-watch="btn" class="button tiny tt-button-primary tt-button-green-normal local-results-btn lr-btn-adjustments" ng-click="doMatrixOperation(-1, 5, null , output.parentnodeID)" ng-if="output.PipeParameters.isAlternative"><span class="icon icon-tt-exchange"></span> Invert Judgments</a>
                                                <textarea id="judgment-table-values" class="hide"></textarea>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>


                <!-- show 3 buttons -->
                <div class="large-4 columns large-centered hide results-inconsistency-tree">
                    <div class="small-12 columns text-center small-centered">
                        <a class="button small radius tt-button-primary tt-button-green-normal local-results-btn expand lr-btn-adjustments" ng-click="reviewJudgment(output.parentnodeID, output.current_step)">Click here to review all judgments</a> 
                    </div>
                    <div class="small-12 columns text-center small-centered">
                        <a class="button small radius tt-button-primary tt-button-green-normal local-results-btn expand lr-btn-adjustments" ng-click="show_inconsistency_view()">Click here if you think the inconsistency is too high</a> 
                    </div>
                    <div class="small-12 columns text-center small-centered">
                        If you think the priorities are not reasonable then:
                    </div>
                                
                    <div class="small-12 columns text-center small-centered">
                        <a class="button small radius tt-button-primary tt-button-green-normal local-results-btn expand lr-btn-adjustments" ng-click="show_redo_pairs()">Click here if you would like to redo a judgment for one pair of elements</a> 
                    </div>
                    <div class="small-12 columns text-right large-right">
                        <a class="button tiny warning radius " ng-click="restoreDefaultView()">Cancel</a> 
                    </div>
                </div>
                <!-- //show 3 buttons -->
            </div>
         

<div class="row results-options anytime-local-results">
     <!-- redo pairs view -->
                            <div class="large-12 small-12 columns text-center small-centered results-redo-pairs hide-for-medium-down" ng-if="intermediate_screen == 3">
                                <div class="columns small-12 large-8" >
                                    <textarea class="txt-area-redo" disabled style="background-color: lightyellow">Select a pair of elements (by clicking the checkbox on left) for which you think: One has too high a priority, and the other has too low of a priority.</textarea>
                                </div>
                                <div class="columns small-12 large-4 " >
                                        <a class="button tt-button-primary tt-button-orange-normal lr-btn-adjustments" ng-click="restoreDefaultView()">Cancel</a>
                                        <a class="button tt-button-primary tt-button-green-normal lr-btn-adjustments" ng-click="reEvaluate(output.parentnodeID, output.current_step)">Re-evaluate</a>
                                </div>

                            </div>
                            <!-- //redo pairs view -->
    <div class="small-12 columns text-center small-centered results-main-extra" ng-if="intermediate_screen == 0">
         <a class="button small radius tt-button-primary tt-button-green-normal local-results-btn lr-btn-adjustments priority-btn" ng-model="satisfactory" ng-click="change_satisfactory(satisfactory)" ng-show="['atPairwise','atAllPairwise'].indexOf(output.PipeParameters.JudgmentType) > -1  && !show_check_box && output.PipeParameters.InconsistencyRatioStatus">Click here if these priorities or the inconsistency are not satisfactory</a> 
         <a class="button small radius tt-button-primary tt-button-green-normal local-results-btn lr-btn-adjustments" ng-if="['atPairwise','atAllPairwise'].indexOf(output.PipeParameters.JudgmentType) > -1  && !show_check_box && !output.PipeParameters.InconsistencyRatioStatus" ng-click="show_redo_pairs()">Click here if you would like to redo a judgment for one pair of elements</a> 
    </div>
</div>
    </div>

    </div>
    <div class="tt-mobile-wrap direct-comparison-wrap-mobile columns" ng-if="!output.PipeParameters.equalMessage && (!output.PipeParameters.canshowresults.individual && !output.PipeParameters.canshowresults.combined) || !check_shown_results(output.PipeParameters.canshowresults.individual, output.PipeParameters.canshowresults.combined)" >
        <div class="tt-auto-resize-content columns" style="display: table;">
            <div class="tt-full-height large-12 columns text-center " style="display: table-cell; vertical-align: middle;">
                <h1>
                    <span ng-bind-html="getHtml(output.PipeParameters.messagenote)"></span>
                </h1>
            </div>
        </div>
    </div>

    <div class="tt-mobile-wrap direct-comparison-wrap-mobile columns" ng-if="output.PipeParameters.equalMessage && check_shown_results(output.PipeParameters.canshowresults.individual, output.PipeParameters.canshowresults.combined)" ng-init="showMessage(output.dont_show)">
        <div class="tt-auto-resize-content columns" style="display: table;">
            <div class="large-12 columns text-center " style="display: table-cell; vertical-align: middle;">
                <h1>
                    <span ng-bind-html="getHtml(output.PipeParameters.messagenote)"></span>
                </h1>
            </div>
        </div>
        <div class="large-12 small-12 columns text-center">
            <div class="row" >
                <div class="large-6 columns text-center large-centered" >
 
                    <div class="large-12 columns text-center" >
                        <label class="">
                          <input type="checkbox" ng-model="output.dont_show" ng-change="dontShowMessageCookie(output.dont_show)"> Don't show this message again
                        </label>
                    </div>
                </div>
  		    </div>
            <div class="row" >
                <div class="large-7 columns text-center large-centered" >
                    <button type="button" class="button  tt-button-icond-left tt-button-primary tt-h-btn lr-btn-adjustments" ng-click="reviewJudgment(output.parentnodeID, output.current_step)">
                        <span>Review Judgments</span>
                    </button>
                    <button type="button" class="button  tt-button-icond-left tt-button-primary tt-h-btn lr-btn-adjustments" ng-click="showResult()">
                        <span>Continue</span>
                    </button>
                </div>
            </div>
        </div>
    </div>
