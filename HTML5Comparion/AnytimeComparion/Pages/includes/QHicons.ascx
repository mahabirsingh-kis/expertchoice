<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QHicons.ascx.cs" Inherits="AnytimeComparion.Pages.includes.QHicons" %>
<div ng-class="{'hide' : (is_AT_owner == 0)}" class="right qhelp-edit-wrap">
    <a href="#" data-reveal-id="tt-qh-modal"><span class="icon-tt-edit"></span></a>
</div>
<div ng-class="{'hide' : (is_AT_owner == 0 && is_html_empty(qh_text) )}"  class="right qhelp-question-wrap">
    <a href="#" data-reveal-id="tt-view-qh-modal"><span ng-init="flash(qh_text)" ng-class="{'disabled': is_html_empty(qh_text), 'active-qh' : !is_html_empty(qh_text)} " class="icon-tt-question-circle qh-icons qh-qm-icon qhelp-icon"></span></a>
</div>



