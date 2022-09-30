<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="temporaryList.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.temporaryList" %>
<%@ Import Namespace="AnytimeComparion.Pages.TeamTimeTest" %>
<%@ Import Namespace="Canvas" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<div data-equalizer class="large-12 columns">
        <ul style="list-style:none;">
            <li ng-repeat="step in steps_list" id="step_{{$index + 1}}_modal" class="{{output.current_step == $index + 1 ? 'current' : '' }} step_{{$index + 1}}_modal" ng-click="MovePipeStep($index + 1)" >
                <a ng-style="{'background-color': (output.current_step == $index + 1 ? '#c77e30' : ''), 'color': (output.current_step == $index + 1 ? 'white' :  step[1] ) }" ng-bind-html="getHtml(step[0])"></a>
            </li>

        </ul>
</div>
