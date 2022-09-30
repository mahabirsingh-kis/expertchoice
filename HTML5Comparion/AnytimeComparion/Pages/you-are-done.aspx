<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="you-are-done.aspx.cs" Inherits="AnytimeComparion.Pages.you_are_done" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="large-12 columns text-center large-centered">
        <h1 style="padding-top:30px;">You are done. You may now close this window/tab</h1>
    </div>
    <script>
        $(".tt-topnav").hide();
        $(".tt-topnav-last").hide();
    </script>
</asp:Content>
