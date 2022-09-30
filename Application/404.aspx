<%@ Page Language="VB" Inherits="Error404Page" title="Page not Found" Codebehind="404.aspx.vb" %>
<asp:Content ID="ContentMain" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    $(document).ready(function () {
        $("#btnReturn").dxButton({
            text: "<% =JS_SafeString(ResString("btnReturnBack")) %>",
            type: "normal",
            icon: "arrowleft",
            width: 150,
            height: 28,
            visible: false,
            onClick: function (e) { <% =JSGetBackURL() %> }
        }).focus();
    });

</script>
<div class="table">
    <div class="tr">
        <div class="td tdMainContent tdCentered">
            <div style="font-size: 96px; font-weight: bold; color: #d15a3f;">404</div>
            <div style="font-size: 24px; font-weight: bold; color: #2569b4;"><% = ResString("lblPageNotFound").ToUpper %></div>
            <h4 style="margin: 3em;"><asp:Label ID="lblError" runat="server" /></h4>
        </div>
    </div>
    <div class="tr">
        <div class="td" style="height:1em; padding-bottom:2em;">
            <div id="btnReturn"></div>
        </div>
    </div>
</div>
</asp:Content>

