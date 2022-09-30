<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GlobalResults.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.GlobalResults" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<link href="/Content/stylesheets/results.css" rel="stylesheet" />

<% if (Session["User"] != null || Session["UserType"] != null)
   { %>
<div class="tt-mobile-wrap direct-comparison-wrap-mobile">
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

    <div class="row">
        <div class="large-12 columns">
                <!---<div class="row tt-j-title text-center">
                    <h3 id="parentnodetxt"></h3>
                </div>--->
                <div class="row tt-j-content multi-loop-wrap-local-results">
                    <div class="large-12 columns ds-div-trigger">
                                                    <!-- start desktop layout -->
                            <div class="teamtime-data-sorter hide-for-medium-down" >
                                <div class="row sorter-view ">
                                    <div class="large-3 columns">
                                        <span class="sv-label left">Normalization: </span>
                                        <select class="priorities-view-result sv-sorter large-2 columns" ng-init="normalization = normalization_list[0]" ng-change="change_normalization(normalization)" ng-model="normalization"  ng-options="obj.text for obj in normalization_list track by obj.value">
                                        </select>
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="large-4 medium-5 small-12 columns tt-results-nav-wrap text-left large-left">
                                        <div class="tt-header-nav blue-header capitalize-text">
                                            {{output.pipeData.objective}}
                                        </div>
                                         <div class="tt-results-nav-content " >
                                            <ul class="tt-r-ul ">
                                                <li class="tt-r-ul-li"
                                                    ng-repeat="node in treeNode"
                                                    ng-include="'objective_sublevel.html'"
                                                    >
                                                </li>
                                            </ul>
                                        </div>
                                        <!-- start drop downs -->
                                        <div id="drop-1" data-dropdown-content class="f-dropdown" aria-hidden="true">
                                          <p>Lorem ipsum</p>

                                        </div>
                                        <!-- end drop downs -->
                                    </div>
                                    <div class="large-8 medium-7 small-12 columns ">
                                        <table class="tt-table-wrap priorities-cbr">
                                        <thead>
                                            <tr>
                                                <th class="tt-with-sorter">
                                                    <div class="text small-12 columns">No.
                                                        <div class="sorter-icons">
                                                            <a href="#" ng-click="sort_Results('nodeID')"><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="tt-with-sorter">
                                                    <div class="text small-12 columns">Name
                                                        <div class="sorter-icons ">
                                                            <a href="#" ng-click="sort_Results('nodeName')"><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </div>

                                                </th>
                                                <th class="tt-with-sorter">
                                                    <div class="text small-12 columns">Your Results
                                                        <div class="sorter-icons ">
                                                            <a href="#" ng-click="sort_Results('yourResults')"><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="tt-with-sorter">
                                                    <div class="text small-12 columns">Combined Results
                                                        <div class="sorter-icons">
                                                            <a href="#" ng-click="sort_Results('combine')"><span class="icon-tt-sort-toggle"></span></a>
                                                        </div>
                                                    </div>
                                                </th>
                                                <th class="">
                                                    <div class="text small-12 columns">Bar Graph</div>
                                                </th>
                                            </tr>
                                        </thead>

                                        <tbody>
                                            <tr ng-repeat="child in results_data | orderBy: columnname:reverse">
                                                <td>{{child[0]}}</td>
                                                <td>{{child[1]}}</td>
                                                <td class="blue ir-your-result">{{child[2] *100 | number:2}}%</td>
                                                <td class="green">{{child[3] *100 | number:2}}%</td>
                                                <td>
                                                    <div class="direc-comparison-wrap priorities">
                                                        <div  class="priorities-result-bar" ng-style="get_results('own_bar', child[2])"></div>
                                                    </div>
                                                    <div class="direc-comparison-wrap priorities cbr">
                                                        <div class="priorities-result-bar green" ng-style="get_results('combine_bar', child[3])"></div>
                                                    </div>
                                                </td>
                                            </tr>
                                        </tbody>

                                    </table>
                                    </div>
                                </div>
                            </div>  
                            <!-- end desktop layout --> 

                            <!-- start mobile layout -->
                            <div class="off-canvas-wrap tt-hidden-menu" data-offcanvas>
                           <aside class="tg-obj-content" >

                                <div class="tt-results-nav-wrap">
                                    <div class="tt-header-nav blue-header">
                                        {{output.pipeData.objective}}
                                        <a class="right show-for-medium-down tt-toggler  tt-show-objectives-btn left"  href="#" data-toggler="tg-obj-content"  title="Show Objectives"><span class="icon-tt-close "></span></a>
                                    </div>
                                        <div class="tt-results-nav-content " >
                                            <!-- start parent 1 -->
                                            <!-- start parent 2 -->
                                            <ul class="tt-r-ul ">
                                                <li class="tt-r-ul-li tt-toggler" data-toggler="tg-obj-content"
                                                    ng-click="wrtNodeId = node[0]"
                                                    ng-repeat="node in treeNode"
                                                    ng-include="'objective_sublevel.html'"
                                                    >

                                                </li>
                                            </ul>
                                            <!-- end parent 2 -->
                                        </div>
                                </div>

                            </aside> 
                            <div class="inner-wrap" style="position: initial" data-equalizer>
 

                                <!-- Off Canvas Menu -->                                
                                    <div class="small-12 columns itemLabel text-center show-for-medium-down teamtime-data-sorter-mobile-wrap"  > 
                                        <a class="show-for-medium-down tt-toggler  tt-show-objectives-btn left"  href="#" data-toggler="tg-obj-content"  title="Show Objectives"><span class="icon-tt-menu "></span></a>
                                        <div class="small-10 columns">With respect to {{wrt_node_name}}</div>
                                        <!-- Normalization & Sorting Mobile-->
                                          <a class="button tt-button-primary radius right teamtime-normalization-dropdown hide-for-large-up" data-dropdown="DataSorter" aria-controls="DataSorter" aria-expanded="false">
                                              <span class="icon icon-tt-chevron-down"></span>
                                          </a>
                                          <div id="DataSorter" class="f-dropdown" data-dropdown-content tabindex="-1" aria-hidden="true" aria-autoclose="false" tabindex="-1">
                                            <div class="row">
                                               <div class="sorter-view small-6 columns show-for-medium-down" >
                                                <span class="sv-label left">Normalization: </span>
                                                <select class="large-12 left small-12 columns  priorities-view-result sv-sorter" ng-init="normalization = normalization_list[0]" ng-change="normalization_change(normalization)" ng-model="normalization"  ng-options="obj.text for obj in normalization_list track by obj.value" >
                                                </select>
                                            </div>
                                            <div class="sorter-view small-6 columns" ng-if="$parent.isMobile()" ng-cloak>
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
                                        <!-- End Normalization & Sorting Mobile--> 
                                    </div>                                                                                                            
                                
                                                                  
                                <div class="results-wrapper">
                                    <div class="show-for-medium-down">
                                        <div class="large-12 columns single-item show-for-medium-down teamtime-progress-bars">
                                            <div class="res-data-repeat" ng-repeat="child in results_data | orderBy: columnname:reverse">
                                                <h3 class="itemTitle text-left">                                                    
                                                    <label>
                                                        <div class="results-label">{{child[0]}}</div>
                                                        {{child[1]}}                                                    
                                                    </label>
                                                </h3>
                                                <div class="row collapse" >
                                                    <div class="small-12 columns">
                                                        <div class="small-9 columns progress-bar-wrap mobile">
                                                            <div class="priorities-result-bar" id="mgblue_{{child[0]}}" ng-style="get_results('own_bar', child[2])"></div>
                                                        </div>
                                                        <div class="small-3 columns progress-bar-wrap mobile text-center">
                                                            <span class="right priorities-text" id="mgyou_{{result[3]}}">{{child[2] * 100 | number:2}}%</span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row collapse">
                                                    <div class="small-12 columns text-right">
                                                        <div class="progress-bar-wrap mobile cbr">
                                                            <div class=" small-9 columns progress-bar-wrap mobile cbr">
                                                                <div class="priorities-result-bar green" id="mggreen_{{child[0]}}" ng-style="get_results('combine_bar', child[3])"></div>
                                                            </div>
                                                            <div class="small-3 columns progress-bar-wrap mobile text-center">
                                                                <span class="right priorities-text" id="mgcombine_{{child[0]}}">{{child[3] * 100 | number:2}}%</span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                              </div>
                            </div>


                                <!-- end mobile layout -->                        
                    </div>
                </div>  
            </div>
    </div>

</div>
<script type="text/ng-template" id="objective_sublevel.html">
    <div class="row collapse parent-tree  {{wrtNodeId === node[0] ? 'selected' : ''}}">
        <div class="main-plus-icon" ng-if="node[6] != 0">
            <a id="{{node[0]}}" href="#" class="left tt-toggler" data-toggler="tg-tree-nav"><span ng-class="node[8] == 1 ? 'icon-tt-minus-circle tt-sm-icon' : 'icon-tt-plus-circle tt-sm-icon'" ng-click="node[8] = node[8] == 1 ? 0 : 1"></span></a>
        </div>
 
        <div class="small-12 columns" title="{{node[2]}}" >
            <a href="#" ng-click="isPM ? save_WRTNodeID(node[0]) : ''">{{ node[2] | limitTo:40}}{{ node[2].length > 40 ? '&hellip;' : ''}}</a>
        </div>
        <div class="small-3 columns progress-wrap" ng-if="node[0] != 1">
            <div class="progress">
                <span class="meter" ng-style="{'width': '{{ node[5] * 100}}%'}">{{ node[5] * 100 | number:0}}%</span>
            </div>
        </div>
    </div>
    <ul class="" ng-if="node[8] == 1">
        <li class=""
        ng-repeat="node in node[7]"
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
