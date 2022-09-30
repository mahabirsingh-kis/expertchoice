<%@ Page Language="VB" Inherits="InstallPage" title="Install Databases" MaintainScrollPositionOnPostback="true" Codebehind="Default.aspx.vb" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="600" ></asp:ScriptManager>
<asp:UpdatePanel ID="pnlLogon" runat="server">
<ContentTemplate>
    <script language ="javascript" type="text/javascript">
    
    function DoAction(btn)
    {
        showLoadingPanel();
        return true;
    }
    
    function SwitchPatch()
    {
        var has_selected = 0;
        var re = new RegExp("(.*)cblPatches");
        for (var i=0; i<theForm.elements.length; i++)
        {
            var obj = theForm.elements[i];
            if (obj && (obj.id))
            {
                if (re.test(obj.id)) 
                {
                    if (!obj.disabled && obj.checked) { has_selected = 1; }
                }    
            }
        }
        theForm.<% =btnApplyPatch.ClientID %>.disabled = !has_selected;
    }
    
    function ShowSettings()
    {
        var d = document.getElementById("settings");
        if ((d))
        {
            var v = d.style.display == 'none';
            d.style.display = (v ? 'block' : 'none');
            var l = document.getElementById("settings_link");
            if ((l)) l.innerHTML = (v ? "Hide" : "Show");
        }
    }

    function CopyText(text)
    {
        copyDataToClipboard(text);
        return false;
    }

    function goBack() {
        loadURL("../"); 
        return false;
    }

    window.onload = function () {
        $("#<% =pnlLogon.ClientID %>").css("height", "100%");
        hideLoadingPanel();
    }
           
</script>
<table border="0" cellpadding="0" cellspacing="0" style="width:98%; height:99%; margin:0px auto;">

<tr align="center" valign="bottom" style="height:1em">
<td colspan="3"><h4 style="margin-top:1ex;"><% =PageTitle(CurrentPageID)%></h4></td>
</tr>

<tr valign="top" style="height:98%">
    <td align="center" style="white-space:nowrap"><div class='text bg_fill'>Settings Overview</div><div class="text" style="text-align:left;"><asp:Label ID="lblInfo" runat="server" Text=""></asp:Label></div></td>
    <td width="33%" style="padding:0px 1em" align="left"><div class='text bg_fill'>Database Patches</div>
        <p style="white-space:nowrap; text-align:left"><asp:CheckBoxList ID="cblPatches" runat="server" CssClass="text"/></p>
        <div style="text-align:center" runat="server" id="lblPatches"><%--<asp:Button ID="btnApplyPatch" runat="server" Text="Apply Patches" CssClass="button" Width="12em" Visible="false" Enabled="false" />--%></div>
    </td>
    <td width="33%"><div class='text bg_fill'>Logs</div>
        <p><asp:TextBox ID="tbLogs" BorderColor="#c0c0c0" runat="server" Columns="60" CssClass="input whole" ReadOnly="True" Rows="30" Width="100%" Height="100%" TextMode="MultiLine" Visible="True" Font-Names="Tahoma,Verdana,arial" Font-Size="8pt"></asp:TextBox></p>
    </td>
</tr>

<tr align="left" valign="bottom" style="height:1em">
<td>
    <asp:Button ID="btnCreateAll" runat="server" Text="Create Database" CssClass="button" Width="11em" UseSubmitBehavior = "true" OnClientClick="this.disable = true; return DoAction(this)" />
</td>
<td align="center">
    <asp:Button ID="btnApplyPatch" runat="server" Text="Apply Patches" CssClass="button" Width="11em" Enabled="false" OnClientClick="this.disable = true; return DoAction(this);" />
</td>
<td align="right">
    <asp:Button ID="btnAdminDecisions" runat="server" Text="Decisions" CssClass="button" Width="8em" UseSubmitBehavior="false" Visible="False"/>&nbsp;<asp:Button ID="btnSettings" runat="server" Text="Settings" CssClass="button" Width="10em" UseSubmitBehavior="false"/>&nbsp;<asp:Button ID="btnSysMessage" runat="server" Text="Sys Message" CssClass="button" Width="10em" UseSubmitBehavior="false"/>&nbsp;<asp:Button ID="btnReturn" runat="server" Text="Return" CssClass="button" Width="10em" UseSubmitBehavior="false" OnClientClick="return goBack();" />
</td>
</tr>

</table>
</ContentTemplate>
</asp:UpdatePanel>
</asp:Content>