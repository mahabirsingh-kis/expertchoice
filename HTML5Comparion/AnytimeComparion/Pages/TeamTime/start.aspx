<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="start.aspx.cs" Inherits="AnytimeComparion.test.start" %>


    <asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

        <%if (apps.ActiveProject != null){
              Response.Write(apps.ActiveProject.isTeamTime.ToString());
              %>
        
        <center>
        <asp:Button ID="startsession" runat="server" Text="Start Session" OnClick="Button1_Click" />
        <asp:Button ID="Button1" runat="server" Text="Stop Session" OnClick="Button1_Click1"></asp:Button>
        </center>   
        <% } %>
    </asp:Content>
