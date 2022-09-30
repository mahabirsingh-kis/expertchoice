<%@ Page Language="vb" CodeBehind="Default.aspx.vb" MasterPageFile="~/mpEmpty.Master" Strict="true" Inherits="LoginV6Page" %>
<asp:Content ID="headerContent" runat="server" ContentPlaceHolderID="head_JSFiles"><% If Request IsNot Nothing AndAlso Not Request.IsLocal Then %><script async src="https://www.googletagmanager.com/gtag/js?id=<% =HttpUtility.UrlEncode(WebConfigOption(_OPT_GOOGLE_UID, _DEF_GOOGLE_UA, False)) %>"></script>
<script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', '<% =JS_SafeString(WebConfigOption(_OPT_GOOGLE_UID, _DEF_GOOGLE_UA, False)) %>');
</script>
<% End If %></asp:Content> 
<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server"><div id="popupContainer" style="display:none;"></div>
    <div class="table tblMain bgFill skin-default">
        <div class="tr">
            <div class="td tdCentered">
                <div class="login-logo"><img src="<% =ThemePath + _FILE_IMAGES %><% If App.isRiskEnabled Then %>riskion_logo_250.png" width="250" height="65"<% Else %>ecc_logo_240.png" width="240" height="58"<% End if %> border="0" title="ExpertChoice Comparion" /></div>
                <div class="tdRoundBox" id="divLogonForm">
                    <!-- form -->
                    <% If _ShowRegularLogon OrElse _ShowMeetingForm Then %>
                    <div id="frmLogon"><div style="min-width:160px; margin:60px auto;text-align:center"><img src="Images/loader.gif" width="60" height="64" border="0" title="Loading..." /><br /><br /><% =ResString("lblPleaseWait") %></div></div>
                    <div id="btnLogon"></div>
                    <% If App.isTeamTimeAvailable AndAlso _ShowRegularLogon Then  %><div class="frmLogonOR small gray"><% =ResString("lblFormLogonOR") %></div>
                    <div id="btnMode"></div>
                    <% End If %><% Else %>
                    <div style="padding-bottom:1em;">Sign in to start your session:</div>
                    <% End If %>
                    <% If isSSO Then %><div><% If _ShowRegularLogon OrElse _ShowMeetingForm Then %><div class="frmLogonOR small gray"><% =ResString("lblFormLogonOR") %></div><% End if %><div id="btnSSO" style="padding:2px 1em;"></div></div><% End If %>
                    <% If _ShowRegularLogon Then %><div class="gray forgot-password">
                        <i class="fa fa-question-circle icon-main"></i>
                        <a href="" onclick="onPswRemind(); return false;" class="actions"><% =ResString("mnuForgottenPassword") %></a>
                    </div><% End if %>
                    <!-- /form -->
                </div>
                <% If sMessage <> "" Then %><div style="margin-top:3ex; text-align: center;"><div class="frmLogonMsg"><% =sMessage %></div></div><% End if %>
                <input type="hidden" name="action" value="" /><input type="hidden" name="po" value="<% =SafeFormString(sPswOrig) %>" />
            </div>
        </div>
        <div class="tr">
            <div class="td tdFooterClear tdFooterCentered" id="tdFooter" <% =If(ShowChatSupport(), " style='padding-right:80px;'", "") %>>
                <% If App.Languages.Count > 1 Then %><div class="login-lang" id="divLanguage" >
                    <a href="" onclick="return false;" class="actions"><i class="fa fa-globe"></i><span><% =If(App.CurrentLanguage IsNot Nothing, App.CurrentLanguage.LanguageName, "") %></span></a><div id="divLangsList"></div>
                </div><% Else %><div style="display:inline-block; width:100px; float: left;">&nbsp;</div><% End if %>
                <span class="login-version" title="<% = JS_SafeString(GetVersion(GetWebCoreVersion(), VersionFormat.Normal)) %>">
                    <% =String.Format(ResString("lblVersion"), GetVersion(GetWebCoreVersion(), VersionFormat.Normal)) %>
                </span>
                <span><% =ResString("lblCopyright") %></span>
            </div>
        </div>
    </div>
    <script type="text/javascript">

        var formWidget = "undefined";
        var code_timeout = <% =CodeTimeout %>;
        var code_timer = 0;
        var url_original = "<% =JS_SafeString(PageURL(_PGID_PROJECTSLIST)) %>";

        var formData = {"Email": "<% =JS_SafeString(sEmail) %>",
                        "Password": "<% =JS_SafeString(sPsw) %>",
                        "Name": "<% =JS_SafeString(sName) %>",
                        "AccessCode": "<% =JS_SafeString(sAccessCode) %>",
                        "MeetingID": "<% =JS_SafeString(sMeetingID) %>",
                        "RememberMe": <% = Bool2JS(fRemember) %>};

        var auth_regular = (formData["MeetingID"] == "");

        function initLogonForm() {
            showLoadingPanel();
            <% If _ShowLogonForm OrElse _ShowMeetingForm Then %>formWidget = $("#frmLogon").dxForm({
                formData: formData,
                disabled: <% =Bool2JS(Not fDB_OK) %>,
                validationGroup: "userData",
                items: [{
                    dataField: "Email",
                    showClearButton: true,
                    label: {
                        text: "<% =JS_SafeString(ResString("lblFormEmail"))%>:"
                    },
                    editorOptions: {
                        //placeholder: "your@email.com"
                        mode: "email"
                    },
                    validationRules: [{
                        type: "required",
                        message: "<% =JS_SafeString(ResString("msgEmailEmpty")) %>"
                    }]
                }, {
                    dataField: "Password",
                    showClearButton: true,
                    visible: auth_regular,
                    editorOptions: {
                        mode: "password"
                    },
                    label: {
                        text: "<% =JS_SafeString(ResString("lblFormPassword"))%>:"
                    },
                    //validationRules: [{
                    //    type: "required",
                    //    message: "Password is required"
                    //}]
                }, {
                    dataField: "Name",
                    showClearButton: true,
                    visible: !auth_regular,
                    label: {
                        text: "<% =JS_SafeString(ResString("lblFormName"))%>:"
                    },
                }, {
                    dataField: "AccessCode",
                    showClearButton: true,
                    visible: auth_regular,
                    label: {
                        text: "<% =JS_SafeString(ResString("lblFormPasscode"))%>:"
                    },
                }, {
                    dataField: "MeetingID",
                    visible: !auth_regular,
                    label: {
                        text: "<% =JS_SafeString(ResString("lblFormMeetingID"))%>:"
                    },
                    validationRules: [{
                        type: "required",
                        message: "<% =JS_SafeString(ResString("msgAuthWrongMeetingID")) %>"
                    }]
                }, {
                    dataField: "RememberMe",
                    disabled: <% =Bool2JS(isSSO) %>,
                    visible: <% =Bool2JS(Not DebugDisableAutoComplete4Logon) %>,
                    label: {
                        text: " ",
                        visible: true,
                    },
                    editorOptions: {
                        text: "<% =JS_SafeString(ResString("lblRememberMe"))%>",
                    }
                }],
                readOnly: false,
                showColonAfterLabel: false,
                showValidationSummary: false,
                labelLocation: "left"
            }).dxForm("instance");

            <%  If _ShowRegularLogon OrElse _ShowMeetingForm Then %>$("#btnLogon").dxButton({
                text: (auth_regular ? "<% =JS_SafeString(ResString("btnLogIn")) %>" : "<% =JS_SafeString(ResString("btnJoinTT")) %>"),
                type: "success",
                useSubmitBehavior: false,
                validationGroup: "userData",
                disabled: <% =Bool2JS(Not fDB_OK) %>,
                onClick: function (e) {
                    logon();
                    //e.preventDefault();
                    return false;
                }
            }).focus();<% End If %>

            <%  If _ShowRegularLogon Then %>$("#btnMode").dxButton({
                text: (auth_regular ? "<% =JS_SafeString(ResString("btnTeamTimeOpen")) %>" : "<% =JS_SafeString(ResString("btnLogIn")) %>"),
                //type: "success",
                useSubmitBehavior: false,
                disabled: <% =Bool2JS(Not fDB_OK) %>,
                onClick: function (e) {
                    auth_regular = !auth_regular;
                    initLogonForm();
                    return false;
                }
            });

            if (auth_regular) {
                $("#btnLogon").removeClass("teamtime");
                $("#btnMode").addClass("teamtime");
                $(".tdRoundBox").removeClass("teamtime");
            } else {
                $("#btnMode").removeClass("teamtime");
                $("#btnLogon").addClass("teamtime");
                $(".tdRoundBox").addClass("teamtime");
            }<% End If %>

            $("#theForm").on("submit", function (e) {
                var a = theForm.action;
                if ((a) && (a.value!="")) return true; else { onFormSubmit(e); return false; }
            });
            <% End if %>

            <% If DebugDisableAutoComplete4Logon Then %>$("input").prop("autocomplete", "off"); $("#theForm").prop("autocomplete", "off");<% Else %>$("[type=password]").prop("autocomplete", "current-password");<% End if %>
            <% If isSSO Then %>$("#btnSSO").dxButton({
                text: "<% =JS_SafeString(ResString(If(isSSO_Only(), "btnSSO_OnlyLogin", "btnSSOLogin"))) %>",
                type: "default",
                width: "100%",
                useSubmitBehavior: false,
                onClick: function (e) {
                    var url = document.location.href;
                    url += (url.indexOf("?")>0 ? "&" : "?") + "<% =_PARAM_ACTION %>=sso_start";
                    loadURL(url);
                }
            });<% End if %>

            onResize();
            hideLoadingPanel();
        }

        function logon() {
            var val = formWidget.validate();
            if (val.isValid) {
                var d = {<% =GetURLParam() %>};
                if (auth_regular) {
                    d["mode"] = <% =CInt(ecAuthenticateWay.awRegular) %>;
                    d["password"] = theForm.Password.value;
                    d["passcode"] = theForm.AccessCode.value;
                    d["po"] = encodeURI(theForm.po.value);
                } else {
                    d["mode"] = <% =CInt(ecAuthenticateWay.awJoinMeeting) %>;
                    d["name"] = theForm.Name.value;
                    d["meetingid"] = theForm.MeetingID.value;
                }
                d["email"] = theForm.Email.value;
                if (theForm.RememberMe) d["remember"] = (theForm.RememberMe.value ? 1 : 0);
                <% If Request IsNot Nothing AndAlso Request.QueryString.ToString = "pin" Then %>d["pin"] = true;
                <% End if %>
                callAPI("account/?<% = _PARAM_ACTION %>=logon", d, onLogon);
                theForm.blur();
            } else {
                if (val.brokenRules.length) DevExpress.ui.notify(val.brokenRules[0].message, "error"); 
                theForm.Email.focus();
            }
        }

        function onFormSubmit() {
            if (!($("#btnLogon").is(':disabled')))  {
                var a = document.activeElement;
                if ((a)) a.blur();
                $("#btnLogon").click();
            }
        }

        function onLogon(res) {
            setTimeout('theForm.blur();', 40);
            theForm.blur();
            var err = resString("msgCantLogin");
            if (isValidReply(res)) {
                if (res.Result == _res_Success) {
                    showLoadingPanel();
                    theForm.disabled = 1;
                    if (typeof res.URL!="undefined" && res.URL!="") document.location.href = res.URL; else reloadPage();
                    return true;
                } else {
                    <% If _MFA_REQUIRED AndAlso _MFA_EMAIL_ALLOWED Then %>
                    if (res.Data == "<% = JS_SafeString(ecAuthenticateError.aeMFA_Required.ToString) %>") {
                        if (res.URL != "") url_original = res.URL;
                        if (typeof res.Tag != "undefined") code_timeout = res.Tag * 1;
                        code_timer = Date.now();
                        showMFACodeForm();
                        return false;
                    }
                    <% End if %>if (typeof res.Message != "undefined" && res.Message!="") err = res.Message;
                }
            }
            DevExpress.ui.dialog.alert(err, "<% =JS_SafeString(ResString("titleError")) %>");
            //DevExpress.ui.notify(err, "error");
            return false;
        }

        var mfa_form = null;
        function showMFACodeForm() {
            mfa_form = $("#popupContainer").dxPopup({
                width: dlgMaxWidth(520),
                height: "auto",
                title: resString("titleVerificationCode"),
                toolbarItems: [{
                    widget: 'dxButton',
                    options: {
                        elementAttr: { "id": "btnMFAContinue", "class": "button_enter" },
                        width: 80,
                        disabled: false,
                        type: "default",
                        text: resString("btnContinue"),
                        onClick: onCheckMFACode
                    },
                    toolbar: "bottom",
                    location: 'after'
                }, {
                //    widget: 'dxButton',
                //    options: {
                //        elementAttr: { "id": "btnMFAResend" },
                //        width: 90,
                //        disabled: true,
                //        text: resString("btnSendAgain"),
                //        onClick: function () {
                //            resendMFACode();
                //            return false;
                //        }
                //    },
                //    toolbar: "bottom",
                //    location: 'after'
                //}, {
                    widget: 'dxButton',
                    options: {
                        width: 80,
                        text: resString("btnCancel"),
                        elementAttr: { "class": "button_esc" },
                        onClick: function () {
                            closePopup();
                            mfa_form = null;
                            doLogout();
                            showLoadingPanel();
                            return false;
                        }
                    },
                    toolbar: "bottom",
                    location: 'after'
                }],
                contentTemplate: function () {
                    return $("<div id='divMFAEmail' style='max-width:540px; min-width:200px; padding: 0px 1ex;'><p align='justify' class='text'>" + resString("lblMFAEmail").format(<% =(_DEF_MFA_EMAIL_TIMEOUT \ 60) %>) + "</p><div style='text-align:left'><span style='line-height:2.2; vertical-align:top; font-weight:bold; font-size:0.975rem;'>" + resString("lblMFAEmailCode") + ": </span><div id='inpMFACode' style='display:inline-block; width:5em; font-size:1.05rem;'></div></div class='text gray' style='text-align:center'><span id='divResendTimeout'></span></div>");
                }
            }).dxPopup("instance");
            $("#popupContainer").dxPopup("show");
            $("#inpMFACode").dxTextBox({ value: "" });
            checkMFACodeTimeout(code_timer);
            setTimeout("$('#inpMFACode').find('input').select().focus();", 300);
            setTimeout("$('#inpMFACode').find('input').select().focus();", 600);
        }

        function onCheckMFACode() {
            var e = $('#inpMFACode').find('input').val();
            if (e.trim() == "") {
                DevExpress.ui.notify(resString("msgEmptyCode"), "error");
                setTimeout("$('#inpMFACode').find('input').focus();", 500);
            } else {
                checkMFACode(e);
            }
            return false;
        }

        function checkMFACodeTimeout(timer) {
            if (typeof mfa_form != "undefined" && (mfa_form) && timer == code_timer) {
                //var btn = $("#btnMFAResend");
                //if ((btn) && (btn.length) && btn.data("dxButton")) {
                //    btn.dxButton("instance").option({"text": resString("btnSendAgain") + (code_timeout > 0 ? " (" + code_timeout + ")" : ""), "width": (code_timeout > 0 ? 120 : 80), "disabled": (code_timeout>0)});
                var timeout = <% =_MFA_CODE_RESEND_TIMEOUT %> + code_timeout - <% = _DEF_MFA_EMAIL_TIMEOUT %>;
                var lbl = $("#divResendTimeout");
                if ((lbl) && (lbl.length) && (timeout>=-1 || lbl.text()=="")) {
                    if (timeout<0) timeout = 0;
                    var text = resString("lblMFACodeAgain");
                    var sec = timeout % 60;
                    text = (timeout>0 ? text + " (" + Math.floor(timeout / 60) + ":" + (sec < 10 ? "0" : "") + sec + ")" : "<a href='' onclick='resendMFACode(); return false' class='actions as_link dashed'>" + text + "</a>");
                    if (timeout > 0) {
                        lbl.addClass("gray");
                        lbl.addClass("dashed");
                    } else {
                        lbl.removeClass("gray");
                        lbl.removeClass("dashed");
                    }
                    lbl.html(text);
                }
                if (code_timeout > 0) {
                    setTimeout(function () {
                        code_timeout--;
                        checkMFACodeTimeout(timer);
                    }, 1000);
                } else {
                    var a = DevExpress.ui.dialog.alert(resString("errMFACodeExpired"), resString("msgWarning"));
                    a.done(function () {
                        reloadPage();
                    });
                }
            }
        }

        function resendMFACode() {
            callAPI("account/?action=MFASendCode", {}, onMFACodeSent, false);
        }

        function onMFACodeSent(res) {
            hideLoadingPanel();
            if (isValidReply(res)) {
                if (res.Result == _res_Success) {
                    setTimeout("$('#inpMFACode').find('input').select().focus();", 100);
                    if (typeof res.Tag != "undefined") code_timeout = res.Tag * 1;
                    code_timer = Date.now();
                    checkMFACodeTimeout(code_timer);
                } else {
                    showErrorMessage((res.Message == "" ? err : res.Message), true);
                }
            } else {
                showErrorMessage("Unable to perform this action", true);
            }
            return false;
        }

        function checkMFACode(code) {
            callAPI("account/?action=MFACheckCode", { "code": code, "type": <% =CInt(ecPinCodeType.mfaEmail) %>}, onMFACodeChecked, false);
        }

        function onMFACodeChecked(res) {
            hideLoadingPanel();
            if (isValidReply(res)) {
                if (res.Result == _res_Success) {
                    closePopup();
                    mfa_form = null;
                    var url = url_original;
                    if (res.URL !="") url = res.URL;
                    loadURL(url);
                    return true;
                } else {
                    var a = showErrorMessage((res.Message == "" ? err : res.Message), true);
                    if ((a)) {
                        a.done(function () {
                            if (typeof res.Tag != "undefined")
                            {
                                code_timeout = res.Tag * 1;
                                code_timer = Date.now();
                                checkMFACodeTimeout(code_timer);
                            }
                            $('#inpMFACode').find('input').select().focus();
                            return true;
                        });
                    }
                }
            } else {
                showErrorMessage("Unable to perform this request", true);
            }
            return false;
        }

        function onKeyDown(e) {
            if (!e.ctrlKey && !e.altKey && !e.shiftKey && e.keyCode==13) {
                if (typeof reminder != "undefined" && (reminder) && $("#popupContainer").is(":visible")) {
                    if (typeof onSendPswReminder == "function") onSendPswReminder();
                } else {
                    if (typeof mfa_form != "undefined" && (mfa_form) && $("#popupContainer").is(":visible")) {
                        onCheckMFACode()
                    } else {
                        onFormSubmit();
                    }
                }
            }
        };

        function onResize(force, w, h) {
            onFormNarrow("#frmLogon", 400);
        }

        $(document).ready(function () {
            //onPageLoaded();
            hideLoadingPanel();
            <% If _ShowRegularLogon OrElse _ShowMeetingForm Then %><% If _ShowRegularLogon Then %>if (auth_regular) {
                if (theForm.AccessCode.value == "") theForm.AccessCode.focus(); 
                if (theForm.Password.value == "") theForm.Password.focus(); 
                if (theForm.Email.value == "") theForm.Email.focus();
            } else <% End If %>{
                if (theForm.MeetingID.value == "") theForm.MeetingID.focus();
                if (theForm.Name.value == "") theForm.Name.focus();
                if (theForm.Email.value == "") theForm.Email.focus();
            }<% End if %>
            <% if sAlert<>"" Then %>DevExpress.ui.dialog.alert("<div style='max-width:800px'><% =JS_SafeString(sAlert) %></div>", "<% =JS_SafeString(ResString("titleAlert")) %>");<% Else %>
            <% if CheckVar("remind", False) OrElse CheckVar("reminder", False) OrElse CheckVar(_PARAM_ACTION, "").ToLower.StartsWith("remind") Then %>onPswRemind();<% Else %>
            <% if DebugAutoLogon AndAlso Not isSSO_Only() AndAlso isFirstRun AndAlso (sMessage = "" OrElse Request.IsLocal) AndAlso sAlert = "" AndAlso Not App.isAuthorized AndAlso sEmail <> "" AndAlso fRemember Then %>onFormSubmit();<% End if %><% End If %><% End If %>
            <% If _MFA_REQUIRED AndAlso App.isAuthorized AndAlso App.MFA_Requested Then %>showMFACodeForm();<%End If %>
        });

        keyup_custom = onKeyDown;
        resize_custom = onResize;
        window.focus();
        initLogonForm();
        initLanguages("divLangsList", "divLanguage", [<% =If(TypeOf Page.Master Is clsMasterPageBase, CType(Page.Master, clsMasterPageBase).GetLanguages(), "") %>], "<% = JS_SafeString(If(App.CurrentLanguage IsNot Nothing, App.CurrentLanguage.LanguageCode, "")) %>");

    </script>
</asp:Content>
