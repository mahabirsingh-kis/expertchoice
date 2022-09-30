<%@ Page Language="VB" Inherits="EditAccountNewPage" Codebehind="Edit.aspx.vb" %>
<asp:Content runat="server" ContentPlaceHolderID="head_JSFiles">
    <script type="text/javascript" src="/Scripts/passfield.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
<style type="text/css">

    .pwd_input {
        width: 98%; 
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
        height:2px; 
        width:100%; 
        margin-bottom:0px; 
        background: white;
        opacity: 0;
    }

    .dx-accordion-item, .dx-accordion-item-body {
        overflow: visible;
    }

    .dx-item-content.dx-accordion-item-title, .dx-item.dx-state-hover .dx-accordion-item-title {
        background: #ecf6fc;
        font-size: 1.15em;
        font-weight: bold;
        padding:3px 1ex;
        margin: 0px;
    }

    .ui-spinner-input {
        border: none;
        background: none;
        color: inherit;
        padding: 0;
        margin: 0.2em;
        vertical-align: middle;
        margin-left: 0.4em;
        margin-right: 2em;
    }

</style>
<script type="text/javascript">
   
    var mnu_id = '#accordion';
    var pwd_id = "pwd";

    var formData = {
        "id": "<% =JS_SafeString(App.Options.SessionID) %>",
        "email": "<% = JS_SafeString(App.ActiveUser.UserEmail) %>",
        "name": "<%=JS_SafeString(App.ActiveUser.UserName)%>",
        "password_old": "",
        "password_new": "",
    };

    function saveUserName(name) {
        callAPI("account/?action=option", {"name": "user_name", "value": name}, onSaveUserData);
    }

    <% If Not isSSO() Then %>function doSaveUserPsw(keep_hashes) {
        var pwd = $("#" + pwd_id);
        var formObj = $("#formUser").dxForm("instance");
        var data = formObj.option("formData");
        if ((data)) callAPI("account/?action=option", {"id": data.id, "old": data.password_old, "name": "user_password", "value": pwd.val(), "keep_hashes": keep_hashes}, onSaveUserData);
    }

    function confirmKeepHashes() {
        var dlg = DevExpress.ui.dialog.custom({
            title: resString("titleConfirmation"),
            messageHtml: "<div style='max-width:600px'><% = JS_SafeString(String.Format(ResString("confUserPswUpdate"), App.ActiveUser.UserEmail)) %><p><div id='cbRemoveHashes'></div></div>",
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
            "text": "<% =JS_SafeString(String.Format(ResString("lblRemoveHashes"), App.ActiveUser.UserEmail)) %>",
            value: false
        });
        return false;
    }

    function saveUserPsw() {
        var pwd = $("#" + pwd_id);
        if ((pwd) && (pwd.length)) {
            isValid = pwd.validatePass();
            if (isValid) {
                confirmKeepHashes();
            } else {
                pwd.focus();
            }
        }
    }
        <% End If %>
    function onSaveUserData(res) {
        hideLoadingPanel();
        if (isValidReply(res)) {
            var err = "Unable To save data";
            if (res.Result == _res_Success) {
                DevExpress.ui.notify("<% =JS_SafeString(ResString("msgSaved")) %>");
                return true;
            } else {
                if (res.Message != "") err = res.Message;
            }
            showErrorMessage(err, true);
        }
    }

    function saveOption(id, checked) {
        var c = "";
        switch (id) {
            case "AutoLogon":
                c = "<% =_COOKIE_DEBUG_AUTOLOGON%>";
                break;
            case "RestoreLastPage":
                c = "<% =_COOKIE_DEBUG_LASTPAGE%>";
                break;
            case "AutoComplelete":
                c = "<% =_COOKIE_DEBUG_AUTOCOMPLETE%>";
                break;
            case "ShowLanding":
                c = "<% =_COOKIE_SHOW_LANDING %>";
                break;
        }
        if (c!=="") document.cookie = c + "=" + ((checked) ? "true" : "false") + ";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT;";
            
        callAPI("account/?action=option", {"name": id, "value": (checked)}, hideLoadingPanel);
    }

    function initForm() {

        var formObj = $("#formUser").dxForm({
            colCount: 1,
            formData: formData,
            readOnly: false,
            showColonAfterLabel: false,
            labelLocation: "left",
            items: [{
                dataField: "email",
                label: { alignment: "right", text: "<% =ResString("lblEmail") %>:" },
            isRequired: false,
            disabled: true,
            editorOptions: {
                width: "100%",
            }
            }, {
                dataField: "name",
                label: { alignment: "right", text: "<% =ResString("lblFullName") %>:" },
                isRequired: false,
                disabled: false,
                editorOptions: {
                    width: "100%",
                    maxLength: 78,
                    onEnterKey: function (e) {
                        var val = e.element.find("input").prop("value");
                        var formObj = $("#formUser").dxForm("instance");
                        var data = formObj.option("formData");
                        data.name = val;
                        saveUserName(val);
                    },
                    onValueChanged: function (e) {
                        if (e.previousValue != e.value) saveUserName(e.value);
                    }
                }
            }<% If Not isSSO() Then %>, {
                itemType: "empty"
            }, {
                dataField: "password_old",
                label: { alignment: "right", text: "<% =ResString("lblPasswordExisted") %>:" },
                showClearButton: true,
                editorOptions: {
                    mode: "password"
                }
            }, {
                dataField: "password_new",
                label: { alignment: "right", text: "<% =ResString("lblPasswordNew") %>:" },
                isRequired: false,
                disabled: false,
                template: function (data, itemElement) {
                    itemElement.append("<div class='dx-textbox dx-texteditor dx-editor-outlined dx-widget'><input type='password' id='" + pwd_id + "' class='pwd_input'><div id='pwd_strength' class='pwd_strength'></div></div>");
                }
            }, {
                dataField: "save",
                label: { text: " ", visible: true },
                editorType: "dxButton",
                editorOptions: {
                    text: "<% =JS_SafeString(ResString("btnSavePsw")) %>",
                    elementAttr: {"style": "margin-top: 6px"},
                    width: "12em",
                    useSubmitBehavior: false,
                    //type: "default",
                    onClick: function (e) {
                        saveUserPsw();
                    }
                }
            }<% End if %>]
        });

        <% If Not isSSO() Then %>initPasswordField($("#"+pwd_id), {
            pattern: "<% = JS_SafeString(App.GetPswComplexityPattern) %>",
            acceptRate: <% = JS_SafeNumber(If(_DEF_PASSWORD_COMPLEXITY, 1, 0.8)) %>,
            allowEmpty: <% =Bool2JS(AllowBlankPsw) %>,
            warnMsgClassName: "pwd_warn",
            errorWrapClassName: "text pwd_error",
            nonMatchField: "inpEmail",<% If _DEF_PASSWORD_COMPLEXITY Then %>
            length: {<% If _DEF_PASSWORD_MIN_LENGTH > 1 Then%>min: <%=_DEF_PASSWORD_MIN_LENGTH%><% End If %>,<% If _DEF_PASSWORD_MAX_LENGTH > 1 Then%>max: <%=_DEF_PASSWORD_MAX_LENGTH%><% End If %>}<% End If %>
        }, true, "pwd_strength");<% End if %>

        <% If not isSSO Then %>$("#divOptions").append(createECSwitch("AutoLogon", "<% =JS_SafeString(ResString("lblOptionAutoLogon"))%>", <% =Bool2JS(DebugAutoLogon)%>, function () { saveOption(this.id, this.checked) }));<% End If %>
        $("#divOptions").append(createECSwitch("RestoreLastPage", "<% =JS_SafeString(ResString("lblOptionRestoreLastPage"))%>", <% =Bool2JS(DebugRestoreLastPage)%>, function () { saveOption(this.id, this.checked) }));
        <% If Not isSSO Then %>$("#divOptions").append(createECSwitch("AutoComplete", "<% =JS_SafeString(ResString("lblDisableAutoComplete"))%>", <% =Bool2JS(DebugDisableAutoComplete4Logon)%>, function () { saveOption(this.id, this.checked) }));<% End If %>
        $("#divOptions").append(createECSwitch("ShowLanding", "<% =JS_SafeString(ResString("lblShowLandingPage"))%>", <% =Bool2JS(Not ShowLandingPages(App, App.ActiveUser.UserID))%>, function () { saveOption(this.id, !this.checked) }));
        <% If App.CanUserModifySomeProject(App.ActiveUser.UserID, App.ActiveProjectsList, App.ActiveUserWorkgroup, App.Workspaces) Then%>
        $("#divOptions").append(createECSwitch("show_splash", "<% =JS_SafeString(ResString("lblShowInstructionPage"))%>", <% =Bool2JS(PMShowInstruction(App, App.ActiveUser.UserID))%>, function () { saveOption(this.id, this.checked) }));
        <% If _OPT_ALLOW_REVIEW_ACCOUNT AndAlso ReviewAccount() <> "" Then%>$("#divOptions").append(createECSwitch("ReviewAccount", "<div style='display:inline-block; max-width:400px'><% =JS_SafeString(ResString("lblReviewAccountEnabled"))%></div>", <% =Bool2JS(App.ReviewAccountEnabled(ReviewAccount() <> ""))%>, function () { saveOption(this.id, this.checked) }));<% End If %>
        //$("#divOptions").append("<div style='padding-left:30px'><% =ResString("lblOptionWipeoutProjectsTimeout")%> <input type='text' name='tbPurgeTimeout' id='tbPurgeTimeout' class='spinner' value='<% =App.WipeoutProjectsTimeout%>' style='width:3em'></div>");
        $("#divOptions").append("<div style='padding-left:30px'><span style='vertical-align:top; line-height:22px'><% =ResString("lblOptionWipeoutProjectsTimeout")%></span> <div id='spinWipeout' style='display:inline-block; width:4em'></div></div>");
        $("#spinWipeout").dxNumberBox({
            value: <% =App.WipeoutProjectsTimeout %>,
            min: 1,
            max: 365,
            mode: "number",
            showSpinButtons: true,
            //useLargeSpinButtons: true,
            onValueChanged: function (e) {
                saveOption("wipeout_timeout", e.component.option("value"));
            }
        });<% End If %>
    }

    function initAccordeon() {
        var tabs = [
            { title: "<% =ResString(If(isSSO(), "lblOwnerName", "lblAccountEditTabInfo")) %>", "icon": "", template: $("<div style='text-align:left; padding-bottom:1em'><div id='formUser'>111</div></div>") },
            { title: "<% =ResString("lblAccountEditTabPreferences") %>", "icon": "", template: $("<div id='divOptions' style='padding:4px 0px;'></div>") }
        ];

        var accordion = $(mnu_id).dxAccordion({
            items: tabs,
            collapsible: false,
            multiple: true,
            selectedItems: tabs,
        }).dxAccordion("instance");
    }

    function onResize() {
        onFormNarrow("#formUser", 650);
    }

    resize_custom = onResize;

    $(document).ready(function () {
        initAccordeon();
        initForm();
    });

</script>
<h3 style="margin-bottom:1ex"><% =GetPageName %></h3>
<div id="accordion" style="text-align:left; min-width:300px;"></div><input type="hidden" id="inpEmail" value="<% = JS_SafeString(App.ActiveUser.UserEmail) %>" />

</asp:Content>