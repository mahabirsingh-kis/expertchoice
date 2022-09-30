<%@ Page Language="VB" Inherits="UpdateDecisionPage" title="Update Decision" Codebehind="Update.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<h6 style="margin:0px 4em; text-align:center;" runat="server" id="lblMessage">Please wait...</h6>
 <script>

     var back_url = "<% = JS_SafeString(PageURL(CheckVar("back", _PGID_PROJECTSLIST))) %>";

     function doUpdate() {
         callAPI("project/?<% = _PARAM_ACTION %>=upgrade", {"pgid": "<% =CheckVar("back", -1) %>"}, onUpgradeComplete);
         setTimeout(function () { showLoadingPanel("<% = JS_SafeString(ResString("msgUpdatingDecision")) %>"); }, 300);
     }

    function onUpgradeComplete(res) {
        var err ="<%  =JS_SafeString(ResString("msgCantUpgradeProject")) %>";
        if (isValidReply(res)) {
            if (res.Result == _res_Success) {
                showLoadingPanel("", true);
                if (res.URL == "") res.URL = back_url;
                document.location.replace(res.URL);
                return true;
            } else {
                if (res.Message != "") err = res.Message;
            }
        }
        var a = showErrorMessage(err, true);
        a.done(function () {
            document.location.replace(back_url);
        });
    }

     $(document).ready(function () {
         doUpdate();
    });

 </script>
</asp:Content>

