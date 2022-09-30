<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WaitingPage.ascx.cs" Inherits="AnytimeComparion.Pages.TeamTimeTest.WaitingPage" %>

<div class="tt-body">
    <div class="row">
        <div class="large-12 columns">
            <div class="large-8 columns large-centered text-center tt-resources-wrap waiting " ng-if="output.TeamTimeOn">
                <h3>Waiting for the Project Manager to start/resume the meeting. 
                You will automatically join the meeting when it starts.</h3>
            </div>
            <div class="large-8 columns large-centered text-center tt-resources-wrap ended " ng-if="!output.TeamTimeOn">
                <h3>TeamTime™ meeting was ended by the Project Manager.</h3>
                <br /><br />
                <a href="<%= Page.ResolveUrl("~") %>"><span>Return to Home Page</span></a>
            </div>
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
        </div>
    </div>
</div>
