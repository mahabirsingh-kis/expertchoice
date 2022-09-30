<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"  CodeBehind="RequiredBrowsers.aspx.cs" Inherits="AnytimeComparion.Pages.RequiredBrowsers" %>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server" ng-cloak>
    <div  id="required-browsers-page" ng-controller="RequiredBrowsersController">
       <div ng-if="isMobile() && !browser_recommended" ng-cloak>
           For mobile android, we require users to use Google Chrome or the Android default browser.
           <br />
           For mobile IOS, re require Safari or Google Chrome.
       </div>
        <div ng-if="!isMobile()  && !browser_recommended" ng-cloak>
            For desktop, we require users to use Google Chrome.  
       </div>
    </div>
    <asp:PlaceHolder runat="server">
        <%: Scripts.Render("~/bundles/RequiredBrowsersController") %>
    </asp:PlaceHolder>
</asp:Content>