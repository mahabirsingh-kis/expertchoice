<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LocalResults.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.LocalResults" %>
<link href="/Content/stylesheets/results.css" rel="stylesheet" />

<div class="tt-mobile-wrap direct-comparison-wrap-mobile">
    <div class="ds-div-trigger">
        <% if (Session["User"] != null || Session["UserType"] != null)
           { %>


        <!-- Toggle Title -->
        <ul class="tt-sticky-element accordion teamtime-title-accordion question-header" data-accordion>
          <li class="accordion-navigation row">
            <a href="#panel1a" class="small-1 columns hide-for-large-up"><span class="icon icon-tt-plus-square"></span></a>
            <div id="panel1a" class="content active small-10 medium-10 large-12 columns text-center end">
                <p class="text" ng-bind-html="task"></p>                
            </div>            
          </li>
        </ul>
        <!-- End Toggle Title -->

        <div class="row" >
            <div class="large-12 columns">
                    <!--<div class="row tt-j-title text-center">
                        <h3>{{ JSON_results[0] }}</h3>
                    </div>-->
                    <div class="row tt-j-content multi-loop-wrap-local-results teamtime-only">
                        <div class="small-12 columns teamtime-data-sorter-mobile-wrap">
                        <!-- Normalization & Sorting -->
                              <a class="button tt-button-primary radius right teamtime-normalization-dropdown hide-for-large-up" data-dropdown="DataSorter" aria-controls="DataSorter" aria-expanded="false">
                                  <span class="icon icon-tt-chevron-down"></span>
                              </a>
                              <div id="DataSorter" class="f-dropdown" data-dropdown-content tabindex="-1" aria-hidden="true" aria-autoclose="false" tabindex="-1">
                                <div class="row">
                                    <div class="sorter-view large-2 small-6 columns" ng-if="!isObjective">
                                        <span class="sv-label left">Normalization: </span>
                                        <select class="large-12 left small-12 columns  priorities-view-result sv-sorter" ng-init="normalization = normalization_list[0]" ng-change="normalization_change(normalization)" ng-model="normalization"  ng-options="obj.text for obj in normalization_list track by obj.value" >
                                        </select>
                                    </div>
                                    <div class="sorter-view large-2 small-6 columns" ng-if="$parent.isMobile()" ng-cloak>
                                        <span class="sv-label left">Sort By: 
                                            <a class="sv-arrow-sorter">
                                                <span style="font-size:12px" ng-class="reverse == null ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                    ng-click="set_reverse()"></span>
                                            </a>
                                        </span>
                                        <select class="priorities-view-result sv-sorter small-10 columns" ng-model="columnsort" ng-init="columnsort = teamtimecolumns[output.pipeData.defaultsort]" ng-change="sort_Results(columnsort[0])" ng-options="column[1] for column in teamtimecolumns track by column[0]">
                                        </select>
                                    </div>
                                </div>    
                              </div>
                              <div class="teamtime-data-sorter hide-for-medium-down">
                                    <div class="sorter-view large-3 small-6 columns" ng-if="!isObjective">
                                        <span class="sv-label left">Normalization: </span>
                                        <select class="large-12 left small-12 columns  priorities-view-result sv-sorter" ng-init="normalization = normalization_list[0]" ng-change="normalization_change(normalization)" ng-model="normalization"  ng-options="obj.text for obj in normalization_list track by obj.value" >
                                        </select>
                                    </div>
                                    <div class="sorter-view large-2 small-6 columns" ng-if="$parent.isMobile() && !isObjective" ng-cloak>
                                        <span class="sv-label left">Sort By: 
                                            <a class="sv-arrow-sorter">
                                                <span style="font-size:12px" ng-class="reverse == null ? 'icon-tt-sort-toggle' : !reverse ? 'icon-tt-sort-up' :  'icon-tt-sort-down'" 
                                                    ng-click="set_reverse()"></span>
                                            </a>
                                        </span>
                                        <select class="priorities-view-result sv-sorter small-10 columns" ng-model="columnsort" ng-init="columnsort = teamtimecolumns[output.pipeData.defaultsort]" ng-change="sort_Results(columnsort[0])" ng-options="column[1] for column in teamtimecolumns track by column[0]">
                                        </select>
                                    </div>
                                </div>                          
                            <!-- End Normalization & Sorting -->
                            <div class="row collapse">
                                <table class="tt-table-wrap priorities-cbr hide-for-medium-down">
                                    <thead>
                                        <tr>
                                            <th class="tt-with-sorter">
                                                <div class="text small-11 columns">No.</div>
                                                <div class="sorter-icons small-1 columns text-center">
                                                    <a href="#" ng-click="sort_Results('nodeID')"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="tt-with-sorter">
                                                <div class="text small-11 columns">Name</div>
                                                <div class="sorter-icons small-1 columns text-center">
                                                    <a href="#" ng-click="sort_Results('nodeName')"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="tt-with-sorter ir-result">
                                                <div class="text small-11 columns">Your Results</div>
                                                <div class="sorter-icons small-1 columns text-center">
                                                    <a href="#" ng-click="sort_Results('yourResults')"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="tt-with-sorter">
                                                <div class="text small-11 columns">Combined Results</div>
                                                <div class="sorter-icons small-1 columns text-center">
                                                    <a href="#" ng-click="sort_Results('combine')"><span class="icon-tt-sort-toggle"></span></a>
                                                </div>
                                            </th>
                                            <th class="">
                                                <div class="text small-12 columns">Bar Graph</div>
                                            </th>
                                        </tr>
                                    </thead>

                                    <tbody>

                                        <tr ng-repeat="child in results_data | orderBy: columnname:reverse ">
                                            <td>{{child[0]}}</td>
                                            <td>{{child[1]}}</td>
                                            <td id="you_{{child[0]}}" class="blue ir-result" >{{child[2] == -1 ? '-' : (child[2] *100 | number:2) + '%'}}</td>
                                            <td id="combine_{{child[0]}}" class="green">{{child[3] == -1 ? '-' : (child[3] *100 | number:2) + '%'}}</td>
                                            <td>
                                                <div class="direc-comparison-wrap priorities">
                                                    <div id="blue_{{child[0]}}" class="priorities-result-bar" ng-style="get_results('own_bar', child[2])"></div>
                                                </div> 
                                                <div class="direc-comparison-wrap priorities cbr">
                                                    <div id="green_{{child[0]}}" class="priorities-result-bar green" ng-style="get_results('combine_bar', child[3])"></div>
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>

                                </table>

                                <!-- mobile layout -->


                                <div class="show-for-medium-down" ng-dbltap>
                                    <div class="large-12 columns single-item show-for-medium-down teamtime-progress-bars">
                                        <div class="res-data-repeat" ng-repeat="child in results_data  | orderBy: columnname:reverse">
                                            <h3 class="itemTitle text-lef">
                                                 <label>
                                                        <div class="results-label">{{result[0]}}</div>
                                                        {{child[1]}}                                                    
                                                    </label>
                                            </h3>

                                            <div class="row collapse ir-result" title="individual result">
        <!--                                        <div class="small-6 columns itemLabel" ng-if="result[4] == 'rvIndividual' || result[4] == 'rvBoth'">Your Results:</div>-->
                                                <div class="row collapse">
                                                    <div class="small-12 columns">
                                                        <div class="small-9 columns progress-bar-wrap mobile">
                                                            <div class="priorities-result-bar" id="mblue_{{child[0]}}" ng-style="get_results('own_bar', child[2])"></div>
                                                        </div>
                                                        <div class="small-3 columns progress-bar-wrap mobile text-center">
                                                            <span class="right" id="myou_{{child[0]}}">{{child[2] == -1 ? "-" : (child[2] * 100 | number:2) + "%"}}</span>
                                                        </div>
                                                    </div>                                                    
                                                </div>
                                            </div>
                                            <div class="row collapse" title="combined result">
    <%--                                            <div class="small-6 columns itemLabel" ng-if="result[4] == 'rvGroup' || result[4] == 'rvBoth'">Combined Results:</div>--%>
                                                <div class="small-12 columns text-right">
                                                    <div class="small-9 columns progress-bar-wrap mobile cbr">
                                                        <div class="priorities-result-bar green"  id="mgreen_{{child[0]}}" ng-style="get_results('combine_bar', child[3])"></div>
                                                    </div>
                                                    <div class="small-3 columns progress-bar-wrap mobile text-center">
                                                        <span class="right" id="mcombine_{{child[0]}}">{{child[3] == -1 ? "-" : (child[3] * 100 | number:2) + "%"}}</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- //mobile layout -->

                            </div>
                            <div class="small-12 columns text-center small-centered inconsistency"></div>
                            <div class="small-12 columns text-center small-centered " ng-if="output.pipeData.messagenote != ''" ng-bind-html="getHtml(output.pipeData.messagenote)"></div>
                        </div>
                    </div>
            </div>

        </div>
    </div>


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
</div>
