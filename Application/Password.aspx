<%@ Page Language="VB" Inherits="CreatePasswordPage" Codebehind="Password.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/passfield.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">

    .pwd_input {
        width: 96%;
        border: 0px;
        margin: 3px 2px 2px 4px;
    }

    .pwd_warn {
        position: absolute;
        z-index: 3333;
    }

    .pwd_error {
    }

    .pwd_strength {
        height: 2px;
        margin: 0px 2px 0px 2px;
        background: white;
        opacity: 0;
        position: relative;
        top: -3px;
    }

</style>
<script language="javascript" type="text/javascript">

    var pwd_id = "pwd";
    <% If isFormEnabled Then %>
    function doSaveUserPsw(keep_hashes) {
        var pwd = $("#" + pwd_id);
        callAjax("params=" + replaceString("&", "%26", JSON.stringify({"value": pwd.val(), "keep_hashes": (typeof keep_hashes == "undefined" ? true : keep_hashes)})), onSaveUserData, _method_POST);
    }

    function onSaveUserData(res) {
        hideLoadingPanel();
        if (isValidReply(res)) {
            var err = "Unable To save data";
            if (res.Result == _res_Success) {
                DevExpress.ui.notify("<% =JS_SafeString(ResString("msgSaved")) %>");
                var url = "default.aspx";
                if (typeof res.URL != "undefined") url = res.URL;
                loadURL(url);
                return true;
            } else {
                if (res.Message != "") err = res.Message;
            }
            showErrorMessage(err, true);
        }
        return false;
    }

    function confirmKeepHashes() {
        var dlg = DevExpress.ui.dialog.custom({
            title: resString("titleConfirmation"),
            messageHtml: "<div style='max-width:600px'><% = JS_SafeString(String.Format(ResString("confUserPswUpdate"), If(CurrentUser Is Nothing, "your account", CurrentUser.UserEmail))) %><p><div id='cbRemoveHashes'></div></div>",
            buttons: [{
                text: resString("btnContinue"),
                type: "default",
                onClick: function (e) {
                    var keep_hashes = true;
                    var cb = $("#cbRemoveHashes");
                    if ((cb) && cb.data("dxCheckBox")) {
                        keep_hashes = !(cb.dxCheckBox("instance").option("value"));
                    }
                    doSaveUserPsw(keep_hashes);
                }
            }, {
                text: resString("btnCancel"),
                onClick: function (e) {
                }
            }]
        });
        dlg.show();
        $("#cbRemoveHashes").dxCheckBox({
            "text": "<% =JS_SafeString(String.Format(ResString("lblRemoveHashes"), If(CurrentUser Is Nothing, "your account", CurrentUser.UserEmail))) %>",
            value: false
        });
        return false;
    }

    function onChangePsw(e) {
        <% If Not AllowBlankPsw() Then %>
        var labels = [];
        labels[PassField.CharTypes.DIGIT] = "at least one number";
        labels[PassField.CharTypes.LETTER] = "at least one lowercase letter";
        labels[PassField.CharTypes.LETTER_UP] = "at least one capital (uppercase) letter"; 
        labels[PassField.CharTypes.SYMBOL] = "at least one special character";

        var lbl = $("#msgInstruction");
        if ((lbl) && (lbl.length)) {
            lbl.hide();
            if ((e) && (e.strength)) {
                var s = e.strength;
                //if ((s.pattern)) {
                if ((s.pattern) && (Object.keys(s.pattern).length>1)) {
                    var msg = "Password requirements:<div style='padding:4px 1em; line-height:140%;'>";
                    for (let charType in s.pattern) {
                        var pass = ((s.charTypes[charType]));
                        msg += "<div><i class='far fa-{0}' style='color:{2}'></i> {1}</div>".format((pass ? "check-circle" : "times-circle"), labels[charType], (pass ? "green" : "red"));
                    }
                    if ((s.length)) {
                        var l = "";
                        var pass = false;
                        if (s.length.max > 2 && s.length.max < 32) {
                            if (s.length.min > 1) {   // min and max
                                pass = (s.length.current >= s.length.min && s.length.current <= s.length.max);
                                l = "must be {0}-{1} characters".format(s.length.min, s.length.max);
                            }
                            else {  // only max
                                pass = (s.length.current <= s.length.max);
                                l = "less than " + s.length.max + " characters";
                            }
                        } else {
                            if (s.length.min > 1) {   // min only
                                pass = (s.length.current >= s.length.min);
                                l = "at least " + s.length.min + " characters";
                            }
                        }
                        if (l!="") msg += "<div><i class='far fa-{0}' style='color:{2}'></i> {1}</div>".format((pass ? "check-circle" : "times-circle"), l, (pass ? "green" : "red"));

                    }
                    msg += "</div>";
                    lbl.html(msg).show();
                }
            }
        }
        var pwd = $("#" + pwd_id);
        if ((pwd) && (pwd.length)) {
            isValid = pwd.validatePass();
            var btn = $("#btnAccept");
            if ((btn) && (btn.data("dxButton"))) {
                btn.dxButton("instance").option({
                    //"type": (isValid ? "success" : "default"),
                    "icon": (isValid ? "fa fa-check" : "")
                });
            }
        }

        <% End If %>
    }

    function saveUserPsw() {
        var pwd = $("#" + pwd_id);
        if ((pwd) && (pwd.length)) {
            isValid = pwd.validatePass();
            if (isValid) {
                <% If byToken Then %>confirmKeepHashes();<% Else %>doSaveUserPsw(true);<% End If %>
            } else {
                var msg = pwd.getPassValidationMessage();
                if (msg != "") {
                    msg = msg.charAt(0).toUpperCase() + msg.slice(1);
                    var a = DevExpress.ui.dialog.alert(msg, "Password validation");
                    a.done(function () {
                        pwd.focus();
                        return true;
                    });
                } else {
                    pwd.focus();
                }

            }
        }
    }<% End If %>

    function DoResetPassword() {
        onPswRemind();
    }

    function onKeyUp(e) {
        if (!e.ctrlKey && !e.altKey && !e.shiftKey && e.keyCode==13) {
            var btn = $("#btnAccept");
            if ((btn) && btn.data("dxButton")) {
               <% If isFormEnabled Then %>saveUserPsw();<% End If %>
            }
        }
    };

    keyup_custom = onKeyUp;

    $(document).ready(function () {
        var pwd = $("#"+pwd_id);

        initPasswordField(pwd, {
            pattern: "<% = JS_SafeString(App.GetPswComplexityPattern) %>",
            acceptRate: <% = JS_SafeNumber(If(_DEF_PASSWORD_COMPLEXITY, 1, 0.8)) %>,
            allowEmpty: <% =Bool2JS(AllowBlankPsw) %>,
            warnMsgClassName: "pwd_warn",
            errorWrapClassName: "text pwd_error",
            showTip: false,<% If _DEF_PASSWORD_COMPLEXITY Then %>
            nonMatchField: "inpEmail",
            length: {<% If _DEF_PASSWORD_MIN_LENGTH > 1 Then%>min: <%=_DEF_PASSWORD_MIN_LENGTH%><% End If %>,<% If _DEF_PASSWORD_MAX_LENGTH > 1 Then%>max: <%=_DEF_PASSWORD_MAX_LENGTH%><% End If %>}
            <% Else %>blackList: "",
            length: { min:1 }<% End If %>
        }, true, "pwd_strength", onChangePsw);

        $("#btnAccept").dxButton({
            text: "<% =JS_SafeString(ResString("btnSave")) %>",
            type: "default",
            //icon: "fa fa-check",
            disabled: <% =Bool2JS(Not isFormEnabled) %>,
            onClick: function (e) {
                <% If isFormEnabled Then %>saveUserPsw();<% End If %>
                return false;
            }
        });

        <% If isFormEnabled Then %>pwd.focus();
        pwd.setPass("");
        //$("#msgInstruction").html(pwd.getRequirements()).show();<% Else %>pwd.prop("disabled", "disabled");
        <% End If %>
    });

</script>
<h6><% = String.Format(ResString("msgCreatePassword"), If(CurrentUser Is Nothing, "your account", SafeFormString(CurrentUser.UserEmail))) %>:</h6>
<div style="text-align:center;margin:1ex 0px 1em 0px"><table border="0" cellspacing="0" cellpadding="3">
    <% If isFormEnabled Then %><tr valign="top">
        <td><div class='dx-textbox dx-texteditor dx-editor-outlined dx-widget' style="width:220px; height:26px; margin:0px;"><input type='password' id='pwd' class='pwd_input'/></div><div id='pwd_strength' class='pwd_strength'></div></td>
        <td><div id="btnAccept" style="padding:1px 0px; width:100px; height:26px; margin:0px; "></div></td>
    </tr><% End If %>
    <tr><td class="text" colspan="2"><div id="msgInstruction" style="margin-top:1ex; max-width:320px; text-align:left; font-size:0.77rem; display:none;" class="info_pnl"></div></td></tr>
</table></div><input type="hidden" id="inpEmail" value="<% = JS_SafeString(If(CurrentUser Is Nothing, "", CurrentUser.UserEmail)) %>" />
<asp:Label runat="server" ID="lblMsg" Font-Bold="true" CssClass="text error" Visible="false" />
</asp:Content>