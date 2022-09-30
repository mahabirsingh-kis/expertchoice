<%@ Page Language="VB" Inherits="SystemMessageEditorPage" title="Restore MasterDB" MaintainScrollPositionOnPostback="true" Codebehind="Message.aspx.vb" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" Runat="Server">

<h4><% = PageTitle(_PGID_SYSTEM_MESSAGE) %></h4>

<div style='width:600px; margin:2em; text-align:left' class='text'>
 <div><b><% =ResString("lblSystemMessage") %></b>:</div>
 <div><asp:TextBox runat="server" id="tbMessage" CssClass='input' Columns="65" Rows="10" Width="100%" TextMode="MultiLine" /></div>
 <div style='margin-top:1em; text-align:right'><asp:Button runat="server" ID="btnSave" CssClass="button" Width="9em" />&nbsp;<asp:Button runat="server" ID="btnExit" CssClass="button" UseSubmitBehavior="false" Width="9em" /></div>
</div>

<p align='center'><asp:Label runat="server" ID="lblMessage" CssClass="text" Font-Bold="true" Width="380px" /></p>

</asp:Content>