<%@ Page Language="VB" Inherits="TokenPage" title="Tokenizer" Codebehind="Token.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">

<h4 style='margin-bottom:1ex'>Tokenizer</h4>

<table border='0'><tr><td align='center'>

<div style='text-align:left' class='text small'>Please put any text or link here for decode/encode. This form working for current instance links.<br /> If you would like to decode the token string from another instance, you should provide custom cript key (InstanceID) for that case.</div>
<asp:TextBox runat="server" ID="tbSource" TextMode="MultiLine" Rows="4" Columns="100" CssClass="input" /><br />

<div style='text-align:left' class='text'>
    <asp:CheckBox runat="server" ID="cbUserInstanceID" Text="Use custom crypt key:" CssClass="checkbox text"/>&nbsp;<asp:TextBox runat="server" ID="tbInstanceID" TextMode="SingleLine" Rows="1" Columns="50" CssClass="input" />
</div>

<div style='margin-top:1ex; margin-bottom:1em'>
    <asp:Button runat="server" ID="btnDecode" Text="Decode Hash/Token string" Width="18em" CssClass="button" /> &nbsp;
    <asp:Button runat="server" ID="btnEncode" Text="Encode URL string" Width="18em" CssClass="button" />
</div>

<asp:TextBox runat="server" ID="tbResult" TextMode="MultiLine" Rows="15" Columns="100" CssClass="input" /><br />

</td></tr></table>

</asp:Content>

