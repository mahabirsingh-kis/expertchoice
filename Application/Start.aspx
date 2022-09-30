<%@ Page Language="VB" Inherits="StartSignUpPage" CodeBehind="Start.aspx.vb" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="600" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<asp:Panel runat="server" ID="pnlSignupForm" Visible="false">
<telerik:RadScriptBlock runat="server" ID="RadScriptBlock1">
<script language="javascript" type="text/javascript">

    function frm_mode(val)
    {
        var m = theForm.frmMode;
        if ((m)) m.value = val;
    }

    function frm_mode_val()
    {
        <% If isSSO() Then %>return 0;<% Else %>var m = theForm.frmMode;
        return ((m) ? m.value : -1);<% End if %>
    }

    function CheckForm() {
        if (frm_mode_val()==1)
        {
            var e = theForm.<% =tbEmailR.ClientID %>; if ((e) && e.value.trim()=='') { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgEmailEmpty")) %>', "<% =JS_SafeString(ResString("titleError")) %>"); return false; }
        }
        else
        {
            <% if fEmail AndAlso fReqEmail Then %>var e = theForm.<% =tbEmail.ClientID %>; if ((e) && e.value.trim()=='') { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgEmailEmpty")) %>', "<% =JS_SafeString(ResString("titleError")) %>"); return false; } <% End If %>
            <% if fName AndAlso fReqName Then %>var n = theForm.<% =tbFullName.ClientID %>; if ((n) && n.value.trim()=='') { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgNameEmpty")) %>', "<% =JS_SafeString(ResString("titleError")) %>"); return false; } <% End If %>
            <% If fPhone AndAlso fReqPhone Then%>var t = theForm.<% =tbPhone.ClientID%>; if ((t) && t.value.trim()=='') { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgPhoneEmpty"))%>', "<% =JS_SafeString(ResString("titleError")) %>"); return false; } <% End If %>
            <% if fPsw Then %>var p = theForm.<% =tbPassword.ClientID %>; var p2 = theForm.<% =tbPassword2.ClientID %>; 
            if ((p) && (p2) && (p.value!=p2.value)) { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgPasswordsMustBeEqual")) %>', true, "theForm.<% =tbPassword.ClientID %>.focus()"); return false; }
            <% If fReqPsw OrElse Not WebOptions.AllowBlankPsw Then%>if ((p) && p.value.trim()=='') { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgPasswordEmpty")) %>',"<% =JS_SafeString(ResString("titleError")) %>"); return false; } <% Else %>var em = theForm.<% =tbEmail.ClientID %>; if ((p) && (em) && (p.value.trim()!='') && (em.value.trim()=='')) { DevExpress.ui.dialog.alert('<%=JS_SafeString(ResString("msgEmailMustBeFilled")) %>', "<% =JS_SafeString(ResString("titleError")) %>"); return false; }<% End If %><% End If %>
        }
        return true;
    }

    function Hotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event)
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);	// D0073
            if ((code==13) && (!event.altKey))
            {
                if (frm_mode_val()==1 && (theForm.<% =btnOKR.ClientID %>)) theForm.<% =btnOKR.ClientID %>.click(); else theForm.<% =btnOK.ClientID %>.click();
            } 
        }
    }

    document.onkeydown = Hotkeys;

</script>
</telerik:RadScriptBlock>
<telerik:RadAjaxPanel ID="RadAjaxPanelLogon" runat="server" HorizontalAlign="Center" LoadingPanelID="AjaxLoadingPanelLogon">
<telerik:RadCodeBlock runat="server" ID="CodeBlock">
    <h4 style="margin-bottom:1em;margin-top:3em" runat="server" id="hdr"></h4><% =CustomContent %>
        <div style="text-align:center"><table border="0" cellpadding="12" cellspacing="0" style="margin: 1ex auto 3em auto;">
          <tr valign="top"><td <% If Not isSSO() Then %> width="50%" align="right" style="padding-right:24px"<% End if %> ><table border="0" cellpadding="0" cellspacing="1">
            <% If Not isSSO() Then %><tr>
                <td>&nbsp;</td>
                <td style="white-space:nowrap;"><h6><% = ResString("lblSignupRegistering")%></h6></td>
            </tr><% End If %>
            <tr runat="server" id="trEmail" style="height:1.5em">
                <td align="right" class="text"><b><% =ResString("lblEmail")%><% = IIf(fReqEmail, "*", "")%></b>:</td>
                <td style="white-space:nowrap;"><asp:TextBox ID="tbEmail" runat="server" Width="18em" TextMode="SingleLine" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr runat="server" id="trName" style="height:1.5em">
                <td align="right" class="text"><nobr><b><% =ResString("lblFullname") %><% = IIf(fReqName, "*", "")%></b>:</nobr></td>
                <td><asp:TextBox ID="tbFullName" runat="server" Width="18em" TextMode="SingleLine" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr runat="server" id="trPhone" style="height:1.5em">
                <td align="right" class="text"><nobr><b><% =ResString("lblUserPhone")%><% = IIf(fReqPhone, "*", "")%></b>:</nobr></td>
                <td><asp:TextBox ID="tbPhone" runat="server" Width="18em" TextMode="SingleLine" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr runat="server" id="trPsw1" style="height:1.5em">
                <td align="right" class="text"><nobr><b><% =ResString("lblPassword") %><% = IIf(fReqPsw, "*", "")%></b>:</nobr></td>
                <td><asp:TextBox ID="tbPassword" runat="server" TextMode="Password" Width="18em" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr runat="server" id="trPsw2" style="height:1.5em">
                <td align="right" class="text"><nobr><b><% =ResString("lblPasswordAgain") %><% = IIf(fReqPsw, "*", "")%></b>:</nobr></td>
                <td><asp:TextBox ID="tbPassword2" runat="server" TextMode="Password" Width="18em" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="text">&nbsp;</td>
                <td style="padding-top:1ex"><asp:Button ID="btnOK" runat="server" CssClass="button" Width="10em" OnClientClick="frm_mode(0);" /></td>
            </tr>
        </table></td>
        <% If Not isSSO() Then %><td style="border-left:2px solid #d0d0d0; padding-left:24px" align="left"><table border="0" cellpadding="0" cellspacing="1">
            <tr>
                <td>&nbsp;</td>
                <td style="white-space:nowrap;"><h6><% = ResString("lblSignupRegistered")%></h6></td>
            </tr>
            <tr style="height:1.5em">
                <td align="right" class="text"><nobr><b><% =ResString("lblEmail")%>*</b>:</nobr></td>
                <td style="white-space:nowrap;"><asp:TextBox ID="tbEmailR" runat="server" Width="18em" TextMode="SingleLine" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr style="height:1.5em">
                <td align="right" class="text"><nobr><b><% =ResString("lblPassword")%></b>:</nobr></td>
                <td><asp:TextBox ID="tbPasswordR" runat="server" TextMode="Password" Width="18em" CssClass="input"></asp:TextBox></td>
            </tr>
            <tr>
                <td class="text">&nbsp;</td>
                <td style="padding-top:1ex"><asp:Button ID="btnOKR" runat="server" CssClass="button" Width="10em" OnClientClick="frm_mode(1);" /></td>
            </tr>
        </table></td><% End If %>
       </tr>
       <tr class="text">
         <td <% If Not isSSO() Then %> colspan="2"<% End if %> style="height:4em; padding: 12px;" align="center"><div class="error text" style="max-width: 600px; text-align:center"><b><asp:Label runat="server" ID="lblFormError" Visible="false"/></b></div></td>
       </tr>
      </table></div><input type="hidden" name="frmMode" value="0" />
    </telerik:RadCodeBlock>        
</telerik:RadAjaxPanel>

<telerik:RadAjaxLoadingPanel  ID="AjaxLoadingPanelLogon" runat="server">
    <EC:LoadingPanel ID="LoadingPanel" runat="server" />
</telerik:RadAjaxLoadingPanel>
</asp:Panel>

<asp:Label runat="server" ID="lblError" Visible="false" CssClass="text"/>

</asp:Content>
