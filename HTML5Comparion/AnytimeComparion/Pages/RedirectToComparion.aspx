<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RedirectToComparion.aspx.cs" Inherits="AnytimeComparion.Pages.RedirecToComparion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" class="tt-content-wrap" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row" style="display: table;">
        <div class="large-9 columns large-centered text-center" style="vertical-align: middle; padding: 10% 0 0 0;">
            <div class="columns panel browser-notice">
                <div class="large-12 small-12 columns">
                    <span class="icon-tt-info-circle notice-icon" style="font-size: x-large;"></span>
                    <span style="vertical-align: top; horiz-align: left;">We are currently updating the functionality for  evaluations for new or anonymous participants that contain an Insight Survey. Please copy and paste the link below to perform your evaluation.</span>
                </div>
                <div class="large-12 small-12 columns">
                    <br/>
                    <span>Participant link for Comparion site: </span><strong><%= ComparionUrl %></strong>
                    <a href="#" class="radius button normal-green-bg" style="font-size: 14px; padding: 5px 10px; margin-bottom: 0px" ngclipboard-success="copyToClipboard()" ngclipboard data-clipboard-text="<%= ComparionUrl %>">COPY</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
