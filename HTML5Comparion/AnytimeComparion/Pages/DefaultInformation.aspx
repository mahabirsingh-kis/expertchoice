<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DefaultInformation.aspx.cs" Inherits="AnytimeComparion.Pages.DefaultInformationaspx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="small-10 medium-5 large-3 columns text-center small-centered" 
        style="padding: 20px 0 0 0;
        background: #f8f8f8;
        border-radius: 5px;
        border: 2px solid #eee;
        margin-top: 40px;">
        <p style="margin-bottom:0px">Logged in as:</p>
        <p style="font-weight:bold; color:#539ddd"><span runat="server" id="UserName"></span></p>
        <p><button class="LogoutButtons button radius normal-green-bg">Logout</button></p>
    </div>
</asp:Content>
