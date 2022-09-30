<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StepsList.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.StepsList" %>
<%@ Import Namespace="AnytimeComparion.Pages.TeamTimeTest" %>
<%@ Import Namespace="Canvas" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<div data-equalizer class="large-12 columns">
        <ul style="list-style:none;">
            <li ng-repeat="step in output.steps" id="step_{{$index + 1}}_modal" class="{{output.current_step == $index + 1 ? 'current' : '' }} step_{{$index}}_modal" ng-init="output.current_step == $index + 1 ? scrolltoelement($index + 1) : ''" >
                <a ng-click="get_pipe_data($index + 1)" ng-style="{'background-color': '{{step[2]}}', 'color': '{{step[1]}}'}" >{{step[0]}}</a>
            </li>
        </ul>
</div>