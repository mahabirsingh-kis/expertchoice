<%@ Page Language="VB" Inherits="WorkgroupEditPage" title="Workgroup Edit" Codebehind="WorkgroupEdit.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<script language="javascript" type="text/javascript">

    function SetFile(name) {
        var tb =theForm.<% =tbEULAFile.ClientID %>;
        if ((tb)) { tb.value = name; tb.focus(); }
        $("#divEULAFiles").dxTooltip("instance").hide();
        return false;
    }

    function ConfirmChangeType(msg)
    {
        var result = DevExpress.ui.dialog.confirm(msg, resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                loadURL('<% =JS_SafeString(PageURL(_PGID_ADMIN_WORKGROUPS)) %>'); 
            } else {
                loadURL('<% = String.Format("{0}&{1}={2}&reject=1", PageURL(_PGID_ADMIN_WORKGROUP_EDIT, GetTempThemeURI(False)), _PARAM_ID, CheckVar(_PARAM_ID, -1)) %>');
            }
        });
        return false;
    }

    function ReloadWorkgroups()
    {
        if ((window.parent) && (typeof (window.parent.ReloadWorkgroupsList) == "function")) {
            window.parent.ReloadWorkgroupsList();
        }
    }

    function viewEULA(name, title) {
        //CreatePopup(name, 'UELA_Preview', 'menubar=no,maximize=no,titlebar=no,status=no,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=600,height=400', true);
        DevExpress.ui.dialog.alert("<div style='width:" + dlgMaxWidth(1200) + "px;height:" + (dlgMaxHeight(800)-120) + "px;'><iframe frameborder='0' scrolling='yes' id='frmRichEditor' style='border:0px;' class='whole' src='" + name + "'></iframe></div>", "Preview: " + title);
    }

    function initEULAFiles() {

        $("#divEULAFiles").show().dxTooltip({
            target: "#imgFiles",
            showEvent: "dxclick",
            //hideEvent: "dxclick",
            closeOnBackButton: true,
            closeOnOutsideClick: true,
        });
    }
    
    $(document).ready(function () {
        initEULAFiles();
    });

</script>
    <h4><% =PageTitle(CurrentPageID)%></h4>
    <div style='width:480px; text-align:center; border:1px solid #ccc; padding:1em;'>
    <table border="0" cellpadding="1" cellspacing="1" width="100%">
        <tr><td class="text" align="right"><b><% = ResString("lblTitle") %></b>:</td><td style="width:305px"><asp:TextBox ID="tbTitle" CssClass="input" runat="server" MaxLength="60" Width="330px"></asp:TextBox></td></tr>
        <tr valign="top"><td class="text" align="right"><% = ResString("lblDescription") %>:</td><td><asp:TextBox ID="tbComment" CssClass="input" runat="server" MaxLength="60" Width="330px" Rows="4" TextMode="MultiLine"></asp:TextBox></td></tr>
        <tr runat="server" id="rowECAM"><td class="text" align="right"><b><% = ResString("lblECAM") %></b>:</td><td><asp:TextBox ID="tbECAM" CssClass="input" runat="server" MaxLength="60" Width="330px"></asp:TextBox></td></tr>
        <tr runat="server" id="rowlLicenseFile"><td class="text" align="right"><b><% = ResString("lblLicenseData") %></b>:</td><td><asp:FileUpload ID="fileLicenseData" CssClass="input" runat="server" Width="330px" /></td></tr>
        <tr runat="server" id="rowlLicenseKey"><td class="text" align="right"><b><% = ResString("lblLicenseKey") %></b>:</td><td><asp:TextBox ID="tbLicenseKey" CssClass="input" runat="server" MaxLength="60" Width="330px"></asp:TextBox></td></tr>
        <% If CanEditOppID() Then%><tr><td class="text" align="right"><% = ResString("lblOpportunityID")%>:</td><td><asp:TextBox ID="tbOpportunityID" CssClass="input" runat="server" MaxLength="250" Width="330px"></asp:TextBox></td></tr><% End if %>
        <tr><td class="text" align="right"><% =ResString("lblCustomEULA")%></td><td class="text" valign="middle"><asp:TextBox ID="tbEULAFile" CssClass="input" runat="server" MaxLength="40" Width="310px"/>&nbsp;<img ID="imgFiles" style="cursor:pointer" Border="0" Height="16" width="16" src="<% =ImagePath %>file.png" title="<% =ResString("lblEULAFiles") %>"></td></tr>
        <%--<% If CurrentWorkgroup.Status <> ecWorkgroupStatus.wsSystem Then%><tr><td class="text">&nbsp;</td><td class="text" align="left"><asp:CheckBox ID="cbDetailMode" runat="server" Checked="true"/></td></tr><% End If %>--%>
        <tr><td class="text">&nbsp;</td><td class="text" align="left"><asp:CheckBox  ID="cbDisabled" runat="server" /></td></tr>
        <tr><td class="text">&nbsp;</td><td class="text" align="left"><asp:CheckBox  ID="cbNoCopyStartupContent" runat="server" Checked="true" Visible="false" /></td></tr>
        <tr><td colspan="2"><div style="text-align:right; margin-top:1ex; padding-top:1em; border-top:1px solid #d0d0d0"><asp:Button ID="btnOK" runat="server" Width="7em" CssClass="button"/> <%--<asp:Button ID="btnCancel" runat="server" CssClass="button" Width="7em" UseSubmitBehavior="false" Visible="false"/>--%></div></td></tr>
    </table></div>

<div style="text-align:center; margin:1em auto 0px autp; max-width:460px; height:2em;">
    <asp:Label ID="lblMessage" runat="server" CssClass="text" Text=""/>
    <div style='padding:1em; text-align:left; display:none;' id="divEULAFiles"><b><% =ResString("lblEULAExists")%></b><br /><asp:Label runat="server" ID="lblEULAFiles" CssClass="text"/></div>
</div>
</asp:Content>