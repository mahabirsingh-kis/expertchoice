<%@ Page Language="VB" CodeBehind="EULA.aspx.vb" Inherits="EULAPage" Strict="true" title="EULA" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    var url = "<% = JS_SafeString(_URL_EULA) %>";
    var pdf = "<% = JS_SafeString(GetPDFFile) %>";
    var htm = "<% = JS_SafeString(GetEULAFile) %>";
    var url_post = "";

    var accepted = <% =Bool2JS(IsValidEULA()) %>;

    var wnd_print = null;

    function onAccept(res) {
        var err = "Unable to perform request";
        if (isValidReply(res)) {
            if (res.Result == _res_Success) {
                accepted = true;
                showLoadingPanel("", true);
                document.location.href = "<% = JS_SafeString(GetRedirectURL) %>";
                return true;
            } else {
                if (typeof res.Message != "undefined" && res.Message!="") err = res.Message;
            }
        }
        DevExpress.ui.notify(err, "error");
    }

    function isAccepted(e) {
        if (!(accepted) && (e)) {
            e.preventDefault();
            e.returnValue = '<% = JS_SafeString(ResString("msgAcceptEULA")) %>';
            return e.returnValue;
        }
        return;
    }

    window.addEventListener("beforeunload", isAccepted); 

    $(document).ready(function () {

        showLoadingPanel();

        $("#btnContinue").dxButton({
            text: resString("btnAccept"),
            type: "success",
            icon: "ion ion-ios-checkmark-circle",
            width: 130,
            disabled: <% = Bool2JS(App.ActiveUser Is Nothing) %>,
            onClick: function (e) {
                var d = {"EULA": htm, "Version": "<% = JS_SafeString(EULAVersion) %>" };
                callAPI("account/?<% =_PARAM_ACTION %>=EULA_accept", d, onAccept);
            }
        }).focus();

        $("#btnDownload").dxButton({
            text: resString("btnEULAPDF"),
            type: "normal",
            icon: "ion ion-md-document",
            width: 130,
            visible: (pdf!=""),
            onClick: function(e) { 
                CreatePopup(url + pdf, "eula_pdf", "menubar=no,maximize=yes,titlebar=yes,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes");
            }
        });

        $("#btnPrint").dxButton({
            text: resString("btnEULAPrint"),
            type: "normal",
            icon: "ion ion-md-print",
            width: 130,
            onClick: function (e) {
                wnd_print = CreatePopup(url + htm, 'eula', "menubar=no,maximize=yes,titlebar=yes,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes");
                wnd_print.onload = wnd_print.print();
            }
        });

    });

</script>
<table border="0" cellpadding="0" cellspacing="12" align="center" class="whole" id="tblEULA">
    <tr valign="top" align="center" style="height:98%">
        <td colspan="2" style="height:100%"><iframe frameborder="0" name="frmEULA" id="frmEULA" height="100%" width="100%" scrolling="yes" src="<% =_URL_EULA + GetEULAFile() %>" style="border:0px solid #f0f0f0" class="whole" onload="setTimeout('hideLoadingPanel();',500);"></iframe></td>
    </tr>
    <tr valign="bottom" style="height:1em">
        <td align="left"><div id="btnDownload"></div>&nbsp;&nbsp;<div id="btnPrint"></div></td>
        <td align="right"><div id="btnContinue"></div></td>
    </tr>
</table>
</asp:Content>