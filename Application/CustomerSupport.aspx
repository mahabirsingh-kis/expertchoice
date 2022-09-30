<%@ Page Title="" Language="VB" MasterPageFile="~/mpPopup.master" AutoEventWireup="false" Inherits="CustomerSupportFormPage" EnableViewState="false" EnableEventValidation="false" MaintainScrollPositionOnPostback="false" ResponseEncoding="utf-8" ViewStateEncryptionMode="Never" Codebehind="CustomerSupport.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">
 
    function InitForm() {
        var o = window.opener;
        if ((o) && (typeof o.err_msg == "string")) {
            theForm.subject.value = o.err_msg;
            theForm.description.value = o.err_stack;
        }
        if (theForm.name.value == "") theForm.name.focus(); else theForm.description.focus();
    }

    function callChat()
    {
        w = (window.opener || window.parent);
        if (w && (typeof w.callChat) != "undefined") {
            w.focus();
            w.callChat();
            window.close();
        } else {
            alert("Unable to find the parent window!");
        }
    }

</script>
<asp:Panel runat="server" ID="pnlForm" CssClass="whole">
<h4><% =PageTitle(CurrentPageID)%></h4>
<div class='text' style='margin-bottom:1ex'><% =String.Format(ResString("msgCustomerSupportEmail"), String.Format("<a href='mailto:{0}' class='actions'>{0}</a>", WebConfigOption(WebOptions._OPT_FOGBUGZ_INBOX)))%>
<% If (App.isAuthorized) Then%><br /><% =String.Format(ResString("msgUseChat"), "callChat(); return false")%><% End If%>
</div>

<table border="0" cellspacing="0" cellpadding="2" width="100%">
 <tr>
  <td class="text" align="right" style="width:20%; white-space:nowrap"><label for="name"><% =ResString("lblCSName")%>:</label></td>
  <td><input id="name" maxlength="80" name="name" size="30" type="text" style="width:400px" value="<% =UserName %>"/></td>
 </tr>
 <tr class="text">
  <td class="text" align="right"><label for="email"><% =ResString("lblCSEmail")%>:</label></td>
  <td><input id="email" maxlength="80" name="email" size="30" type="text" style="width:400px" value="<% =UserEmail %>"/></td>
 </tr>
 <tr class="text">
  <td class="text" align="right"><label for="phone"><% =ResString("lblCSPhone")%>:</label></td>
  <td><input id="phone" maxlength="40" name="phone" size="30" type="text" style="width:400px"/></td>
 </tr>
 <tr class="text">
  <td class="text" align="right"><label for="subject"><% =ResString("lblCSSubject")%>:</label></td>
  <td><input id="subject" maxlength="80" name="subject" size="30" type="text" style="width:400px"/></td>
 </tr>
 <tr class="text" valign="top">
  <td class="text" align="right"><label for="description"><% =ResString("lblCSDescription")%>:</label></td>
  <td><textarea name="description" cols="25" rows="7" style="width:400px" class='textarea'></textarea></td>
 </tr>
 <tr class="text">
  <td class="text" align="right">&nbsp;</td>
  <td><input type="checkbox" name="status" id="status" value="1" checked/><label for="status"><% =ResString("lblSendAppStatus")%></label></td>
 </tr>
</table>
<!--/form-->
</asp:Panel>
<asp:Panel runat="server" ID="pnlMessage" CssClass="whole" Visible="false">
<h4><% =PageTitle(CurrentPageID)%></h4>

<div style="padding-top:6em; color:#006666" class="text"><b><asp:Label runat="server" ID="lblReport"/></b></div>

</asp:Panel>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolderButtons" Runat="Server">
    <asp:Button ID="btnSend" runat="server" CssClass="button" Width="8em" />
    <asp:Button ID="btnClose" runat="server" CssClass="button" OnClientClick="self.close(); return false;" Width="8em" />
</asp:Content>

