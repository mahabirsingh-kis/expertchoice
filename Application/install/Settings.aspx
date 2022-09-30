<%@ Page Language="VB" Inherits="SystemSettingsPage" title="System Settings" MaintainScrollPositionOnPostback="true" Codebehind="Settings.aspx.vb" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    function checkForm() {
        var a = theForm.optAutoUnLockPsw;
        var p1 = theForm.optLockPswAttempts;
        var p2 = theForm.optLockPswPeriod;
        var p3 = theForm.optLockPswTimeout;
        if ((a) && (p1) && (p2) && (p3)) {
            var d = (!(a.checked));
            //p1.disabled = d;
            p2.disabled = d;
            p3.disabled = d;
        }
        var pc = theForm.optPswHighComplexity;
        if ((pc)) {
            var d = (!(pc.checked));
            var opts = ["optPswMinLen", "optPswMaxLen", "optPswMinChanges", "optPswMinLifetime", "optPswMaxLifetime", "optPswPrevHashes"];
            for (var i = 0; i < opts.length; i++) {
                var opt = eval("theForm." + opts[i]);
                if ((opt)) opt.disabled = d;
            }
            var bp = theForm.optAllowBlankPassword;
            if ((bp)) {
                bp.disabled = !d;
                //if (!d) bp.checked = false;
            }
        }
        var sso = theForm.optUseSSO;
        if ((sso)) {
            var d = (!(sso.checked));
            var opts = ["optSSO_Only", "optSSO_DefWkg", "optSSO_DefRole"];
            for (var i = 0; i < opts.length; i++) {
                var opt = eval("theForm." + opts[i]);
                if ((opt)) opt.disabled = d;
            }
        }
    }

    function onChange(name, val) {
        checkForm();
        theForm.disabled = true;
        _ajax_ok_code = "getResponse(data);";
        _ajaxSend("<% =_PARAM_ACTION %>=<% =_ACTION_SAVE %>&name=" + name + "&val=" + encodeURIComponent(val));
    }

    function reset(name) {
        var result = DevExpress.ui.dialog.confirm("Do you want to reset this option custom value and use the default or setting value from the config file?", resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                _ajax_ok_code = "getResponse(data); document.location.reload();";
                _ajaxSend("<% =_PARAM_ACTION %>=reset&name=" + name);
            }
        });
        return false;
    }

    function getResponse(data) {
        theForm.disabled = false;
        if (data == "" || data[0] == "<") {
            //DevExpress.ui.notify("Changes not saved", "error");
        } else {
            var d = eval(data);
            if ((d) && d.length>2) {
                var n = d[0];
                var v = d[1];
                var b = ((d[2]));
                var f = eval("theForm.opt" + n);
                if ((f)) {
                    if ((b)) f.checked = str2bool(v); else f.value = v;
                    checkForm();
                }
            }
            DevExpress.ui.notify("Saved succesfully","info");
        }
    }

    $(document).ready(function () {
        checkForm();
    });

</script>
<table border="0" cellspacing="0" cellpadding="4">
    <tr style="height:2em;"><td align="center"><h4><% = PageTitle(CurrentPageID)%></h4></td></tr>
    <tr valign="top" align="left"><td class="text"><div class="whole" style="border:1px solid #e0e0e0; border-radius:5px; overflow-y:auto; padding:5px 8px;"><% =GetSettings(False)%></div></td></tr>
    <tr><td class="text" style="height:3em; text-align:center">
        <p align="center" style="margin-bottom:1em;"><span class="warning">Please be extremely accurate during the change of any setting, since the wrong option value can break the system or make it very unstable.</span></p>
        <% If Not _OPT_USE_CUSTOM_SETTINGS Then%><h6 class="error">Note: using custom settings is disabled for this instance. All changes, which you did on that screen will not take effect.</h6><% End If%>
    </td></tr>
</table>

</asp:Content>