<%@ Page Language="VB" Inherits="URLDecodePage" title="URL Decoder" Codebehind="URLDecode.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    function CheckForm() {
        var val = theForm.<% =tbURL.ClientID %>;
        if ((val) && (val.value=="")) { jDialog("URL value is not specified!", true, "theForm.<% =tbURL.ClientID %>.focus();"); return false; }
        return true;
    }

</script>

<h4 style='margin-bottom:1em'>URL Decoder</h4>

<table border=0>
<tr><td class='text' align='left'>
Type an URL or URI parameter:<br />
<asp:TextBox runat="server" ID="tbURL" TextMode="MultiLine" Rows="4" Columns="70" CssClass="input" /><br />
<div style='margin-top:4px; margin-bottom:3em; text-align:center'>
<asp:Button runat="server" ID="btnEncode" Text="Decode URL" Width="10em" CssClass="button" OnClientClick="return CheckForm();" />
</div>

<div id="lblResult" runat="server" style="height:7em"></div>

</td></tr>
</table>

</asp:Content>

