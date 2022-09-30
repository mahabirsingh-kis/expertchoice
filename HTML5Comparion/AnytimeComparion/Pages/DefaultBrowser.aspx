<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DefaultBrowser.aspx.cs" Inherits="AnytimeComparion.Pages.DefaultBrowser" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" class="tt-content-wrap" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row" style="display:table;">
        <div class="large-9 columns large-centered text-center" style="vertical-align: middle; padding: 10% 0 0 0;">
            <div class="columns panel browser-notice">
                <div class="large-1 small-12  columns">
                    <span class="icon-tt-info-circle notice-icon"></span>
                </div>     
                <div class="large-11 small-12 columns ">
                    <span ng-if="isMobile()" >
                        For mobile android, we require users to use Google Chrome or the Android default browser.
                        <br />
                        For mobile IOS, we require Safari or Google Chrome.
                    </span>
                    <span ng-if="!isMobile()">
                        For desktop Windows, we require users to use Google Chrome.
                        <br />
                        For desktop Mac, we require users to use Safari or Google Chrome.
                    </span>
                </div>
            </div>
        </div>
       <%-- <div runat="server" id="DivComparionResponsiveLink" class="small-12 medium-12 large-12 columns hide" style="vertical-align: middle; padding: 10px 0 0 0;"> <!-- hide -->
            <div class="row panel collapse browser-notice" style="background: none; border:none; padding-top:0px; padding:0px;"> <!-- collapse -->
                <div class="small-10 medium-8 large-9 small-centered large-centered columns ">
                    <!-- start message -->
                    <div class="row">
                        <div class="small-12 medium-9 large-9 columns large-centered medium-centered" style="margin-bottom:10px; border-bottom:1px dotted #ccc; padding-bottom:10px;">                                    
                            <p style="color: #fff;font-weight: bold; line-height: normal; font-size: .7em; background-color: #539ddd;  padding: 10px 5px; border-radius: 3px; text-align:center">FOR BIGGER MODELS, WE RECOMMEND:</p>                            
                            <p style="line-height: normal;">Copy the following link; Paste and run the link on Google Chrome in Windows or Safari/Chrome in Mac</p> 
                            <div class="row">
                                <div class="large-11 columns" style="padding-right:5px;">
                                    <input type="text" runat="server" id="SpanComparionResponsiveLink" placeholder="URL goes here" />
                                </div>
                                <div class="large-1 columns large-text-right small-text-left">
                                    <button class="radius button normal-green-bg" style="font-size:14px; padding: 10px; margin-bottom:0px"  ngclipboard-success="copy_responsive_link()" ngclipboard data-clipboard-text="" runat="server" id="CopyResponsiveLinkBtn">COPY</button>
                                </div>
                            </div>                            
                        </div>
                        <div class="small-12 medium-9 large-9 columns large-centered medium-centered">
                            <p style="color: #fff;font-weight: bold; line-height: normal; font-size: .7em; background-color: #539ddd;  padding: 10px 5px; border-radius: 3px; text-align:center">FOR SMALLER MODELS</p>                            
                            <p style="line-height: normal;">Click <a href="#" style="font-weight:bold;" ng-click="continue_in_browser()">here</a> to continue using this browser.</p> 
                        </div>
                    </div>                  
                </div>
            </div>
        </div>--%>
    </div>

   <%-- <asp:PlaceHolder runat="server">
        <%: Scripts.Render("~/bundles/defaultBrowserController") %>
    </asp:PlaceHolder>--%>
    <%--<asp:Label runat="server" ID="LabelComparionResponsiveLink"></asp:Label>--%>

   <%-- <asp:Label runat="server" ID="LabelBrowserName"></asp:Label>--%>
</asp:Content>
