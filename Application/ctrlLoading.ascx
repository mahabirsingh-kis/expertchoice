<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlLoadingPanel" Codebehind="ctrlLoading.ascx.vb" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxRoundPanel" TagPrefix="dxrp" %>
<script type="text/javascript">
    <!--
    function SwitchWarning(id, show)
    {
        var l = document.getElementById(id);
        if (l && l.style.display) l.style.display=(show ? "block" :"none");
    }
//-->
</script>
<% =CStr(IIf(isWarning, "<div id='" + CStr(ClientID) + "' style='position:absolute;top:48%;left:50%;width:" + CStr(Width) + "px;margin-left:-" + (CStr(Width / 2)) + "px;margin-top:-80px;text-align:center;z-index:99999;display:" + CStr(IIf(WarningShowOnLoad, "block", "none")) + "'>", ""))%>
<table border="0" class="whole" cellpadding="6" style="opacity: 1.0; filter: alpha(opacity=100); z-index:99999;">
<tr><td align="center" valign="middle"><dxrp:ASPxRoundPanel ID="ASPxRoundPanelLoading" runat="server" SkinID="RoundPanel" ShowHeader="true" Width="200px">
<PanelCollection>
    <dxrp:PanelContent runat="server">
        <div style="text-align:center; padding:3ex; border:0px solid #e0e0e0;"><asp:Image ID="imgLoading" runat="server" SkinID="LoadingImage"/><div <% =CStr(iif(isWarning, "", "class='gray'")) %> style="margin:1ex 0px"><b><asp:Label runat="server" ID="lblMessage" SkinID="LoadingMessage"><% =Message %></asp:Label></b></div><asp:button runat="server" ID="btnClose" UseSubmitBehavior="false" CssClass="button" Visible="false" /></div>
    </dxrp:PanelContent>
</PanelCollection>
</dxrp:ASPxRoundPanel></td></tr></table>
<% =CStr(IIf(isWarning, "</div>", ""))%>
