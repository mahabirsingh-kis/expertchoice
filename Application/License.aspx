<%@ Page Language="VB" Inherits="Error503Page" title="License" Strict="true" Codebehind="License.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    $(document).ready(function () {
        if ($("#btnReturn").length) {
            $("#btnReturn").dxButton({
                text: resString("btnReturnBack"),
                type: "normal",
                icon: "arrowleft",
                width: 150,
                height: 30,
                onClick: function (e) { document.location.href = "<% =JS_SafeString(GetBackURL(If(App.ApplicationError.PageURL = "", PageURL(If(App.isAuthorized, _PGID_PROJECTSLIST, _PGID_START)), App.ApplicationError.PageURL))) %>"; }
            }).focus();
        }
    });

</script>
    <div style="text-align:center">
        <asp:Label ID="lblMessage" runat="server" CssClass="text"/>
        <div id="btnReturn"></div>
    </div>
</asp:Content>

