<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GlobalResults.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.GlobalResults" %>
<%@ Register Src="~/Pages/includes/QuestionHeader.ascx" TagPrefix="includes" TagName="QuestionHeader" %>
<link href="/Content/stylesheets/results.css" rel="stylesheet" />
<div class="tt-mobile-wrap direct-comparison-wrap-mobile overall-result-screen">

    <% if (Session["User"] != null || Session["UserType"] != null)
       { %>
         <div class="large-uncollapse medium-collapse global-result">
            <div class="large-12 columns">
                <div class="row">                
            <div class="small-1 large-1 columns large-text-left small-text-right the-toggler no-more-padding">
                <a class="tt-toggler" id="1" data-toggler="tg-accordion">
                    <span class="icon-tt-minus-square icon"></span>
                    <span class="text "><%--Overall results for {{output.hierarchy[0][1]}}--%></span>
                </a>
            </div>
            <div class="small-11 large-12 columns tg-accordion-1 no-more-padding">
                <div class="tt-panel small-text-left large-text-center global-result-header">
                     <includes:QuestionHeader runat="server" id="QuestionHeader" />  
                </div>
            </div>
                </div>
            </div>
        </div>
         <div class="pipe-wrap large-12 columns many-alternatives">                
                <div class="row collapse tt-j-content">
                    <div class="large-12 columns ">
                        <div class="small-12 columns">
                            <!-- start desktop layout -->
                            <div class="">
                                 <div class="row large-uncollapse medium-collapse hide-for-medium-down">
                                <div class="columns large-11 small-11" ng-class="screens < 600 && $parent.show_check_box ? 'hide' : ''">
                                    <div class="tt-panel large-text-center small-text-left" style="border: none;">
                                         <div class="left large-3 small-6  priorities-cbr" ng-if="!output.PipeParameters.CurrentNode[2]" style="margin-bottom: 0px !important;">
                                            <span class="sv-label left" style="font-size:12px">Normalization: </span>
                                            <select class="priorities-view-result sv-sorter "  ng-change="normalization_Change(normalization, true)" ng-model="normalization" ng-options="obj.text for obj in normalization_list track by obj.value"></select>
                                        </div>
                                        <div class=" left small-6 priorities-cbr show-for-medium-down" style="padding-left:8px; margin-bottom: 0px !important;">
                                           <span class="sv-label left" style="font-size:12px">Sort By: 
                                                    <a ><span style="font-size:12px" ng-class="reverseGlobal == null ? 'icon-tt-sort-toggle' : !reverseGlobal ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                            ng-click="set_reverse(true)"></span></a>
                                           </span>
                                            <select class="priorities-view-result sv-sorter " ng-model="columnsortGlobal" ng-change="sort_Results(columnsortGlobal[0], true)" ng-options="column[1] for column in columns track by column[0]">
                                                    <option value="" selected hidden />
                                            </select>
                                          </div>  
                                    </div>
                                </div>
                            </div>
                                 <%-- End Collapse Normalization & Sorting Desktop--%>

                                <div class="row hide-for-medium-down">
                                    <div class="large-4 medium-5 small-12 columns tt-results-nav-wrap text-left large-left overflow-fix">
                                        <div class="capitalize-text" >
                                            <span style="color:black">{{output.question}}</span>
                                        </div>
                                            
                                        <div class="tt-results-nav-content " >
                                            <!-- start parent 1 -->
                                            

                                            <!-- start parent 2 -->
                                            <ul class="tt-r-ul ">
                                                <li class="tt-r-ul-li"
                                                    ng-click="wrtNodeID = node[0]"
                                                    ng-repeat="node in $parent.hierarchies"
                                                    ng-include="'objective_sublevel.html'"
                                                    >

                                     
                                                </li>
                                            </ul>
                                            <!-- end parent 2 -->

                                            <!-- start siblings 2-->
                                            <ul class="tt-r-ul tt-r-child tg-tree-nav-1 hide" >        
                                                        <li class="tt-r-ul-li with-sub" ng-repeat="node in $parent.hierarchies[0][4]" 
                                                            ng-click="wrtNodeID = node[0]">
                                                            <div class="row collapse parent-tree {{wrt_node_id == node[0] ? 'selected' : ''}}">
                                                                <div class="sub-plus-icon first" ng-if="node[4] != null">
                                                                    <a id="{{node[0]}}" href="#" class="left tt-toggler" data-toggler="tg-tree-nav-sub-first"><span class="icon icon-tt-plus-square"></span></a>
                                                                </div>
                                                                <div class="small-1 columns">
                                                                    <a href="#" class="left dropdown" data-dropdown="drop-1" aria-controls="drop-1" aria-expanded="false">
                                                                        <span class="icon-tt-info-circle"></span>
                                                                    </a>
                                                                </div>
                                                                <div class="small-8 columns" title="{{node[1]}}">
                                                                    <a href="#" ng-click="save_WRTNodeID(node[0])">{{ node[1] | limitTo:35}}{{ node[1].length > 35 ? '&hellip;' : ''}}</a>
                                                                </div>
                                                                <div class="small-3 columns text-right progress-wrap">
                                                                    <div class="progress">
                                                                        <span class="meter right" ng-style="{'width': '{{node[2] * 100}}%'}">{{node[2] * 100 | number:0}}%</span>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            
                                                            <!-- start sub first level-->
                                                                <ul class="tt-r-ul tt-r-child sub tg-tree-nav-sub-first-{{node[0]}}" >        
                                                                        <li class="tt-r-ul-li" ng-repeat="childnode in node[4]" ng-click="wrtNodeID = childnode[0]">
                                                                            <div class="row collapse parent-tree first-sub ">
                                                                                <div class="sub-plus-icon second" ng-if="childnode[4] != null">
                                                                                    <a id="{{childnode[0]}}" href="#" class="left tt-toggler" data-toggler="tg-tree-nav-sub-second"><span class="icon icon-tt-plus-square"></span></a>
                                                                                </div>
                                                                                <div class="small-1 columns">
                                                                                    <a href="#" class="left dropdown" data-dropdown="drop-1" aria-controls="drop-1" aria-expanded="false">
                                                                                        <span class="icon-tt-info-circle"></span>
                                                                                    </a>
                                                                                </div>
                                                                                <div class="small-8 columns">
                                                                                    <a href="#" ng-click="save_WRTNodeID(childnode[0])">{{childnode[1]}}</a>
                                                                                </div>
                                                                                <div class="small-3 columns text-right progress-wrap">
                                                                                    <div class="progress">
                                                                                        <span class="meter right" ng-style="{'width': '{{childnode[2] * 100}}%'}">{{childnode[2] * 100 | number:0}}%</span>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                            
                                                                             <!-- start sub second level-->
                                                                                <ul class="tt-r-ul tt-r-child sub tg-tree-nav-sub-second-1" >        
                                                                                        <li class="tt-r-ul-li" ng-repeat="grand in childnode[4]" ng-click="wrtNodeID = grand[0]">
                                                                                            <div class="row collapse parent-tree second-sub">
                                                                                                <div class="small-1 columns" ng-if="grand[4] != null">
                                                                                                    <a href="#" class="left dropdown" data-dropdown="drop-1" aria-controls="drop-1" aria-expanded="false">
                                                                                                        <span class="icon-tt-info-circle"></span>
                                                                                                    </a>
                                                                                                </div>
                                                                                                <div class="small-8 columns">
                                                                                                    <a href="#" ng-click="save_WRTNodeID(grand[0])">{{grand[1]}}</a>
                                                                                                </div>
                                                                                                <div class="small-3 columns text-right progress-wrap">
                                                                                                    <div class="progress">
                                                                                                        <span class="meter right" ng-style="{'width': '{{grand[2] * 100}}%'}">{{grand[2] * 100 | number:0}}%</span>
                                                                                                    </div>
                                                                                                </div>
                                                                                            </div>
                                                                                        </li>
                                                                                </ul>
                                                                            <!-- end sub second level-->
                                                                        </li>
                                                                </ul>
                                                            <!-- end sub first level-->
                                                            
                                                        </li>
 
                                                       
                                                

                                            </ul>
                                            <!-- end siblings 2-->
                                        
                                            

                                            
                                        </div>
                                        <!-- start drop downs -->
                                        <div id="drop-1" data-dropdown-content class="f-dropdown" aria-hidden="true">
                                          <p>Lorem ipsum</p>

                                        </div>
                                        <!-- end drop downs -->
                                    </div>
                                



                                        <div class="large-8 medium-7 small-12 columns multi-loop-wrap-local-results1 overflow-fix" ng-if="(canshowresult.individual || canshowresult.combined) && check_shown_results(canshowresult.individual, canshowresult.combined)">
                                            <%--<div class="columns large-centered text-center" style="position: absolute; margin-top:-30px">
                                                The priorities of {{wrt_node_id <= 1 ? output.question : output.nameAlternatives}} with respect to {{wrt_node_name}}
                                            </div>--%>
                                            <div class="columns large-centered text-center" style="position: absolute; margin-top:-30px">
                                                The priorities of {{output.nameAlternatives}} with respect to {{wrt_node_name}}
                                            </div>
                                            <table class="tt-table-wrap priorities-cbr"  ng-cloak>
                                                <thead>
                                                    <tr>
                                                        <th class="tt-with-sorter" ng-if="output.PipeParameters.showIndex">
                                                            <div class="text small-12 columns">No.
                                                                <div class="sorter-icons">
                                                                    <a href="#"><span class="icon-tt-sort-toggle" ng-click="sort_Results('nodeID', true)"></span></a>
                                                                </div>
                                                            </div>
                                                        </th>
                                                        <th class="tt-with-sorter">
                                                            <div class="text small-12 columns">Name
                                                                <div class="sorter-icons">
                                                                    <a href="#"><span class="icon-tt-sort-toggle" ng-click="sort_Results('nodeName', true)"></span></a>
                                                                </div>
                                                            </div>

                                                        </th>
                                                        <th class="tt-with-sorter" ng-if="(results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && canshowresult.individual">
                                                            <div class="text small-12 columns">Your Results
                                                                <div class="sorter-icons">
                                                                    <a href="#"><span class="icon-tt-sort-toggle" ng-click="sort_Results('yourResults', true)"></span></a>
                                                                </div>
                                                            </div>
                                                        </th>
                                                        <th class="tt-with-sorter" ng-if="(results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && canshowresult.combined">
                                                            <div class="text small-12 columns">{{output.PipeParameters.columnValueCombined}}
                                                                <div class="sorter-icons text-right">
                                                                    <a href="#"><span class="icon-tt-sort-toggle" ng-click="sort_Results('combine', true)"></span></a>
                                                                </div>
                                                            </div>
                                                        </th>
                                                        <th class="">
                                                            <div class="text small-12 columns">Bar Graph</div>
                                                        </th>
                                                    </tr>
                                                </thead>
                                                <tbody ng-if="results_data.length > 0">
                                                    <tr ng-repeat="result in results_data | orderBy: columnname:reverseGlobal">
                                                        <td  ng-if="output.PipeParameters.showIndex">{{result[0]}}</td>
                                                        <td>{{result[1]}}</td>
                                                        <td ng-if="(result[4] == 'rvIndividual' || result[4] == 'rvBoth') && canshowresult.individual" class="blue ir-your-result">{{result[2] * 100 | number:2}}%</td>
                                                        <td ng-if="(result[4] == 'rvGroup' || result[4] == 'rvBoth') && canshowresult.combined" class="green">{{result[3] * 100 | number:2}}%</td>
                                                        <td>
                                                            <div class="direc-comparison-wrap priorities">
                                                                <div  class="priorities-result-bar" ng-if="(result[4] == 'rvIndividual' || result[4] == 'rvBoth') && canshowresult.individual" ng-style="get_results('own_bar', result[2])"></div>
                                                            </div>
                                                            <div class="direc-comparison-wrap priorities cbr">
                                                                <div class="priorities-result-bar green" ng-if="(result[4] == 'rvGroup' || result[4] == 'rvBoth') && canshowresult.combined" ng-style="get_results('combine_bar', result[3])"></div>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>                                            
                                            <div class="small-12 columns text-center small-centered" ng-if="$parent.showExpectedValue.Value" >
                                                <b>
                                                    <span style="color:#0058a3 ">{{ExpectedValue[0]}}</span>
                                                    <br />
                                                    <span style="color:#6aa84f ">{{ExpectedValue[1]}}</span>
                                                </b>
                                            </div>

                                        </div>
                                        <div class="columns small-centered small-12 text-center show-for-large-up" ng-if="(!canshowresult.individual && !canshowresult.combined) || !check_shown_results(canshowresult.individual, canshowresult.combined)" ng-cloak>
                                                <span ng-bind-html="getHtml(output.PipeParameters.messagenote[0])"></span>
                                        </div>
                                </div>
                            </div>  
                            <!-- end desktop layout --> 
                        
 
                           <aside class="tg-obj-content overflow-fix" ng-if="$parent.isMobile()" >

                                <div class="tt-results-nav-wrap ">
                                    <div class="tt-header-nav blue-header capitalize-text">
                                        {{output.question}} 
                                        <a class="right show-for-medium-down tt-toggler tt-show-objectives-btn left"  href="#" data-toggler="tg-obj-content"  title="Close Objectives"><span class="icon-tt-close white"></span></a>
                                    </div>
                                    <div class="tt-results-nav-content " >
                                        <!-- start parent 1 -->
                                        <!-- start parent 2 -->
                                        <ul class="tt-r-ul ">
                                            <li class="tt-r-ul-li tt-toggler" data-toggler="tg-obj-content"
                                                ng-click="wrtNodeID = node[0]"
                                                ng-repeat="node in $parent.hierarchies"
                                                ng-include="'objective_sublevel.html'"
                                                >
                                            </li>
                                        </ul>
                                        <!-- end parent 2 -->
                                    </div>
                                </div>

                            </aside> 
                            <div class="inner-wrap" data-equalizer style="margin-top:0px;margin-bottom: 10px;">

                                 
                                <div style="padding: 5px 0px 5px 0px;"class="small-12 columns itemLabel text-center show-for-medium-down sensitivity-drop-row">
 
                                    <a class="show-for-medium-down tt-toggler  tt-show-objectives-btn left"  href="#" data-toggler="tg-obj-content"  title="Show Objectives"><span class="icon-tt-menu "></span></a>
                                    <div class="columns medium-11 small-10">With respect to {{wrt_node_name}}</div>
                                     <%-- Collapse Normalization & Sorting Mobile --%>                                                           
                                                    <div class="sensitivity-drop-row hide-for-large-up right"> 
                                                                 <a data-dropdown="drop001" aria-controls="drop2" aria-expanded="false" class="button tt-button-primary right"><span class="icon-tt-chevron-down"></span></a>
                                                                 <div id="drop001" data-dropdown-content class="f-dropdown medium" aria-hidden="true" aria-autoclose="false" tabindex="-1">
                                                                    <div class="columns medium-12 small-12" ng-class="screens < 600 && $parent.show_check_box ? 'hide' : ''">
                                                                        <div class="tt-panel large-text-center small-text-left" style="border: none;">
                                                                             <div class="  left large-2 small-6  priorities-cbr" ng-if="!output.PipeParameters.CurrentNode[2]" style="margin-bottom: 0px !important;">
                                                                                <span class="sv-label left" style="font-size:12px">Normalization: </span>
                                                                                <select class="priorities-view-result sv-sorter "  ng-change="normalization_Change(normalization, true)" ng-model="normalization" ng-options="obj.text for obj in normalization_list track by obj.value"></select>
                                                                            </div>
                                                                            <div class=" left small-6 priorities-cbr show-for-medium-down" style="padding-left:8px; margin-bottom: 0px !important;">
                                                                               <span class="sv-label left" style="font-size:12px">Sort By: 
                                                                                        <a ><span style="font-size:12px" ng-class="reverseGlobal == null ? 'icon-tt-sort-toggle' : !reverseGlobal ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                                                                ng-click="set_reverse(true)"></span></a>
                                                                               </span>
                                                                                <select class="priorities-view-result sv-sorter " ng-model="columnsortGlobal" ng-change="sort_Results(columnsortGlobal[0], true)" ng-options="column[1] for column in columns track by column[0]">
                                                                                        <option value="" selected hidden />
                                                                                </select>
                                                                              </div>  
                                                                        </div>
                                                                     </div>
                                                                 </div>
                                                         </div>                               
                                      <%-- End Collapse Normalization & Sorting Mobile--%>
                                </div>
                             
                                <div class="results-wrapper" ng-if="(canshowresult.individual || canshowresult.combined) && check_shown_results(canshowresult.individual, canshowresult.combined)"  ng-cloak>
                                    <div class="show-for-medium-down">
                                        <div class="small-12 columns" ng-if="results_data[0][4] == 'rvBoth'">                                                                
                                            <div class="small-6 columns itemLabel text-center show-for-medium-down normal-blue-bg" ng-if="(results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth')" style="background:#539ddd;color:white;font-size:14px;" >Your Results</div>
                                            <div class="small-6 columns itemLabel text-center show-for-medium-down normal-green-bg" ng-if="(results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth')" style="background:#a5d490;color:white;font-size:14px;">{{output.PipeParameters.columnValueCombined}}</div>
                                        </div>
                                        <div class="large-12 columns single-item show-for-medium-down">
                                            <div class="res-data-repeat" ng-repeat="result in results_data  | orderBy: columnname:reverseGlobal">
                                                <h3 class="itemTitle text-left">
                                                    <label>
                                                    <div ng-if="output.PipeParameters.showIndex" class="results-label">{{result[0]}}</div>
                                                    <div>{{' ' + result[1]}}</span>
                                                </label>
                                                </h3>
                                                <div class="row collapse" ng-if="(result[4] == 'rvIndividual' || result[4] == 'rvBoth') && canshowresult.individual">
                                                    <div class="small-12 columns progress-bar-wrap mobile">
                                                        <div class="small-9 columns">
                                                            <div class="left priorities-result-bar" ng-model="mgblue" id="mgblue_{{result[0]}}" ng-change="get_results('own_bar', result[2])" ng-style="get_results('own_bar', result[2])"></div>
                                                        </div>
                                                        <div class="small-3 columns">
                                                            <div class="right priorities-text" id="mgyou_{{result[3]}}">{{result[2] * 100 | number:2}}%</div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row collapse" ng-if="result[4] == 'rvGroup' || result[4] == 'rvBoth' && canshowresult.combined">
                                                    <div class="progress-bar-wrap mobile cbr">
                                                        <div class=" small-12 columns progress-bar-wrap mobile cbr">
                                                            <div class="small-9 columns">
                                                                <div class="left priorities-result-bar green" ng-model="mggreen" id="mggreen_{{result[0]}}" ng-change="get_results('combine_bar', result[3])" ng-style="get_results('combine_bar', result[3])"></div>
                                                            </div>
                                                            <div class="small-3 columns">
                                                                <div class="right priorities-text" id="mgcombine_{{result[0]}}">{{result[3] * 100 | number:2}}%</div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="large-12 columns text-center " ng-if="$parent.ExpectedValue" style="min-height: 100px;" >
                                            <b>
                                                <span style="color:#0058a3 ">{{ExpectedValue[0]}}</span>
                                                <br />
                                                <span style="color:#6aa84f ">{{ExpectedValue[1]}}</span>
                                            </b>
                                        </div>
                                    </div>
                                </div>
                                <!--<div class="large-12 small-12 columns large-centered text-center hide-for-large-up" ng-bind-html="getHtml(output.PipeParameters.messagenote[1])" ng-if="!canshowresult.individual || !canshowresult.combined "></div>-->
                                <div class="columns small-12 text-center hide-for-large-up" ng-if="(!canshowresult.individual && !canshowresult.combined) || !check_shown_results(canshowresult.individual, canshowresult.combined)" data-equalizer-watch ng-cloak>

                                    <p class="panel">
                                        <span ng-bind-html="getHtml(output.PipeParameters.messagenote[0])"></span>
                                    </p>
                                </div>
                                


                              </div>
 
                            <!-- end mobile layout -->
                        
                            </div>
                    </div>
                </div>               
        </div>
        <%--<div class="row results-options">--%>
              <div class="small-centered small-12 text-center row results-options" ng-bind-html="getHtml(output.PipeParameters.messagenote[1])" ng-if="!(canshowresult.individual || canshowresult.combined) && !((!canshowresult.individual && !canshowresult.combined) || !check_shown_results(canshowresult.individual, canshowresult.combined))"></div>
        <%--</div>--%>
 </div>


<script type="text/ng-template" id="objective_sublevel.html">
    <div class="row collapse parent-tree" ng-class="wrt_node_id ==  node[0] ? 'selected' : ''" ng-click="save_WRTNodeID(node[0])">
        <div class="main-plus-icon" ng-if="node[5] != null">
            <a id="{{node[0]}}" href="#" class="left tt-toggler" data-toggler="tg-tree-nav"><span ng-class="node[3] == 1 ? 'icon-tt-minus-circle tt-sm-icon' : 'icon-tt-plus-circle tt-sm-icon'" ng-click="node[3] = node[3] == 1 ? 0 : 1"></span></a>
        </div>
 
        <div  ng-class="node[0] == output.parentnodeID ? 'small-12 columns' : 'small-9 columns' "class=" large-left small-left text-left" title="{{node[1]}}" >
            <a >{{ node[1] | limitTo:40}}{{ node[1].length > 40 ? '&hellip;' : ''}}</a>
        </div>
        <div class="small-3 columns progress-wrap"  ng-if="node[0] != output.parentnodeID">
            <div class="progress">
                <span class="meter" style="min-width: 0%;" ng-style="{'width': '{{ node[2]}}%'}">                
                    <span style="overflow:visible; display: block;" >{{ node[2] | number:2}}%
                    </span>
                </span>
            </div>
        </div>
    </div>
    <ul class="" ng-if="node[3] == 1">
        <li class=""
        ng-click="wrtNodeID = node[0]"
        ng-repeat="node in node[5]"
        ng-include="'objective_sublevel.html'"
        >
        </li>
    </ul>
</script>


<% }
       else
       { %>
<div class="row">
    <div class="large-12 columns text-center">

        <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
        <includes:loginMessage ID="needToLogin" runat="server" />

    </div>
</div>
<% } %>  
    
    
