<%@ Page Language="vb" CodeBehind="Invite.aspx.vb" Inherits="AnytimeInvitePage" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server">
    <style>
        .dx-list-item-content {
            padding-top: 2px;
            padding-bottom: 1px;
        }
        .cssPadding {
            margin-top: 5px;
            padding-bottom: 8px;
        }
        .dx-field-item-ldxTababel-content {
            max-width: 100px;
            white-space: normal;
            text-align: right;
        }
        .dx-list-item, .dx-list-item-content {
            width:auto;
        }
        .dx-list-item-content, .dx-list .dx-empty-message {
            padding-left: 0px;
        }
        .dx-texteditor-input {
            padding: 0px 3px;
        }
    </style>
    <script type="text/template" id="tmpInviteByEmail">
        <div id="mainTable" class="table">
            <div class="tr">
                <div class="td" style="vertical-align: top; padding:8px;">
                    <table class="whole" id="tblEmail">
                        <tr style="vertical-align:top;">
                            <td>
                                <div id="dgUsers"></div>
                            </td>
                            <td width="50%" style="padding-left:1em;">
                                <div id="formEmail"></div>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
            <div class="tr" style="height:1em;">
                <div class="td" style="padding: 0px 10px 6px 10px; text-align: right;">
                    <div style="float:left; text-align: left; padding-bottom:4px;"><div id="btnAddParticipants"></div><div id="btnDownloadMailMerge" style="margin-left:1ex"></div></div>
                    <nobr><div id="btnEditInvite"></div><div id="btnReset" style="margin:0px 1ex"></div><div id="btnSendInvite"></div></nobr>
                </div>
            </div>
        </div>
    </script>
    <script type="text/template" id="tmpLinks">
        <div class="table">
            <div class="tr">
                <div class="td" style="padding: 1em;" id="tdLinks">
                    <div id="taLinks" class="whole" style="padding: 1em"></div>
                </div>
            </div>
            <div class="tr">
                <div class="td" style="height: 1em; padding: 0px 1em 1em 0px; text-align: right">
                    <div id="btnCopyLinks"></div>
                    <div id="btnDownloadLinks"></div>
                </div>
            </div>
        </div>
    </script>
    <script type="text/template" id="tmpGeneralLinks">
        <div id="scrollview" style="padding:1em;">
            <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %><div style="padding-bottom:1em;">A hyperlink will be created that can be sent to people for them to evaluate this model. You can send the link to those who are not registered in this Comparion workgroup as well as those who are already registered.</div>
            <label><input type="radio" value="<% =JS_SafeString(ResString("lblSignupAnonymous")) %>" name="linkType"/><% =JS_SafeString(ResString("lblSignupAnonymous")) %></label><br />
            <label><input type="radio" value="<% =JS_SafeString(ResString("lblSignupOptions")) %>" name="linkType" checked="checked"/><% =JS_SafeString(ResString("lblSignupOptions")) %></label><br />
            <label><input type="radio" value="<% =JS_SafeString(ResString("lblSignupPasscode")) %>" name="linkType" <% If isSSO_Only AndAlso Not _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %> checked="checked" disabled="disabled" <% End if %>/><% =JS_SafeString(ResString("lblSignupPasscode")) %></label>
            <% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then  %><fieldset id="linkTypeSet" style="margin:1em 0px; padding:1ex 1em 1em 1em;  width:99%; border-color: #cccccc;">
            <legend style="color:#333333;">&nbsp;Options:&nbsp;</legend>
                <div id="formSignup"></div>
                <div style="padding-top:1em">
                    <table border="0" cellspacing="0" cellpadding="1">
                    <tr valign="bottom">
                        <td style="width:49%;"><div style="height:29px;"><div id="lblAssignNew" class="disabled" style="float:left; margin-top:10px; width:85%;"><% =JS_SafeString(ResString("lblAssignGroupForNewUsers")) %>:</div><div style="float:right;"><div id="btnManageGroups"></div></div></div>
                        <div id="cbAssignNew"></div></td>
                        <td style="padding-left:2em;"><span id="lblAssignExisting" class="disabled"><% =JS_SafeString(ResString("lblAssignPermissionForNewUsers")) %>:</span><br />
                        <div id="cbAssignExisting"></div></td>
                    </tr>
                    <tr valign="top">
                        <td style="padding-right:1ex;"><div id="cbAllowPM"></div></td>
                        <td></td>
                    </tr>
                    </table>
                </div>
            </fieldset><% End If %><% End If %>
            <div id="formGeneralLinks" style="margin-top:1em;"></div>
        </div>
    </script>
    <script type="text/template" id="tmpGroupLinks">
        <div class="table">
            <div class="tr">
                <div class="td" style="padding: 1em;" id="tdGroupLinks">
                    <div id="taGroupLinks" class="whole" style="padding: 1em"></div>
                </div>
            </div>
            <div class="tr">
                <div class="td" style="height: 1em; padding: 0px 1em 1em 0px; text-align: right">
                    <div id="btnCopyGroupLinks"></div>
                </div>
            </div>
        </div>
    </script>

<div class="table">
    <% If (If(App.isRiskEnabled, App.Options.EvalURL4Anytime_Riskion, App.Options.EvalURL4Anytime) AndAlso App.Options.EvalSiteURL <> "" AndAlso Not isResponsiveOnly) Then %><div class="tr" style="height:1em;">
        <div id="td" style="text-align:right; padding:8px 12px 0px 10px;">
            <div id="cbResponsive"></div><i class="as_icon fas fa-question-circle" title="<% = SafeFormString(ResString("lblResponsiveLinksHint")) %>" style="margin:0px 3px; font-size:0.97rem"></i>
        </div>
    </div><% End If %>
    <div class="tr" id="trTabs">
        <div class="td" style="padding:10px 10px 10px 0px;">
            <div id="tabs" class="ec_tabs"></div>
        </div>
    </div>
</div>

    <script type="text/javascript">
        var userRole_def = <%= GetDefaultUserRolesID() %>;
        var userRoles = <%= GetUserRoles() %>;
        var groupsList = [<%= GetGroupsList()%>];
        var dataSourceUsers = <%= GetParticipantsList()%>;
        var usersLinks = "<%= GetUserLinks(isResponsiveOnly) %>";
        var groupsLinks = "<%= GetGroupLinks(True, GetCookie("invite_fld", ""), GetCookie("invite_req", ""), GetDefaultUserRolesID(), False, isResponsiveOnly) %>";
        var hasData = [{"ID":0, "Name":"<% =JS_SafeString(ResString("lblNo")) %>"}, {"ID":1, "Name":"<% =JS_SafeString(ResString("lblYes")) %>"}];
        var tabInviteComparionSend = "<% =JS_SafeString(ResString("tabInviteComparionSend")) %>";
        var tabInviteGeneralLink = "<% =JS_SafeString(ResString(If(isSSO_Only(), "tabInviteGeneralLinkSSO", "tabInviteGeneralLink"))) %>";
        var tabInviteParticipantLinks = "<% =JS_SafeString(ResString("tabInviteParticipantLinks")) %>";
        var tabInviteGroupLinks = "<% =JS_SafeString(ResString("tabInviteGroupLinks")) %>";
        var isOnline = <% =Bool2JS(App.ActiveProject.isOnline) %>;

        var tabs = [
            { title: tabInviteComparionSend, icon: "email", template: tmpInviteByEmail },
            { title: tabInviteGeneralLink, icon: "fas fa-link", template: tmpGeneralLinks },
            { title: tabInviteParticipantLinks, icon: "fas fa-user", template: tmpLinks, visible: <% =Bool2JS(CanSeeLinks) %> },
            { title: tabInviteGroupLinks, icon: "fas fa-users", template: tmpGroupLinks, visible: <% =Bool2JS(CanSeeLinks) %> }
        ];

        var InviteInfodocParams = "<%=JS_SafeString(String.Format("?project={0}&type=8&id={1}&callback=invitation", App.ProjectID, _PARAM_INVITATION_EVAL))%>";
        var SignupMessageParams = "<%=JS_SafeString(String.Format("?project={0}&type=8&id=custom_invite", App.ProjectID))%>";

        var fromValue = "<% =JS_SafeString(SystemEmail) %>";
        var formEmailData = {
            "From": fromValue,
            "Subject": "<% =JS_SafeString(GetSubject()) %>"
        };

        var generalLink = "";
        var projectName = "<%= JS_SafeString(App.ActiveProject.ProjectName) %>";
        var applicationURL = "<%= JS_SafeString(ApplicationURL(False, False)) %>";
        var applicationResponsiveURL = "<%= JS_SafeString(ApplicationURL(True, False)) %>";
        var projectPasscode = "<%= JS_SafeString(App.ActiveProject.Passcode) %>";
        var bodyAnytimeAnonymousInvite = " <% =JS_SafeString(ResString("bodyAnytimeAnonymousInvite" + If(App.isRiskEnabled, "Risk", "")), True) %>";
        var bodyAnytimeLoginInvite = "<% =JS_SafeString(ResString("bodyAnytimeLoginInvite" + If(App.isRiskEnabled, "Risk", "")), True) %>";
        var bodyAnytimeCopyInvite = "<% =JS_SafeString(ResString("bodyAnytimeCopyInvite" + If(App.isRiskEnabled, "Risk", "")), True) %>";
        <% If App.isRiskEnabled Then %>var bodyAnytimeAnonymousInviteBoth = " <% =JS_SafeString(ResString("bodyAnytimeAnonymousInviteRiskBoth"), True) %>";
        var bodyAnytimeLoginInviteBoth = "<% =JS_SafeString(ResString("bodyAnytimeLoginInviteRiskBoth"), True) %>";
        var bodyAnytimeCopyInviteBoth = "<% =JS_SafeString(ResString("bodyAnytimeCopyInviteRiskBoth"), True) %>";
        var generalLink_l = "";
        var generalLink_i = "";
        var invite_both = <% =Bool2JS(Str2Bool(GetCookie("invite_both", "0"))) %>;
        <% End if %>
        var hintAnytimeAnonymousInvite = " <% =JS_SafeString(ResString("lblInviteLinkA").Replace(vbNewLine, "<br>"), True) %>";
        var hintAnytimeLoginInvite = "<% =JS_SafeString(ResString("lblInviteLinkB").Replace(vbNewLine, "<br>"), True) %>";
        var hintAnytimeCopyInvite = "<% =JS_SafeString(ResString("lblInviteLinkC").Replace(vbNewLine, "<br>"), True) %>";
        var lblSignupAnonymous = "<% =JS_SafeString(ResString("lblSignupAnonymous")) %>";
        var lblSignupOptions = "<% =JS_SafeString(ResString("lblSignupOptions")) %>";
        var lblSignupPasscode = "<% =JS_SafeString(ResString("lblSignupPasscode")) %>";
        var generalLinkTypes = [lblSignupAnonymous, lblSignupOptions, lblSignupPasscode];
        var linkType = <% = if(isSSO_Only AndAlso Not _OPT_SHOW_LINKS_WHEN_SSO_ONLY, "lblSignupPasscode", "lblSignupOptions") %>; //lblSignupAnonymous;
        var lblSignup_Email = "<% =JS_SafeString(ResString("lblSignup_Email")) %>";
        var lblSignup_Name = "<% =JS_SafeString(ResString("lblSignup_Name")) %>";
        var lblSignup_Phone = "<% =JS_SafeString(ResString("lblSignup_Phone")) %>";
        var lblSignup_Password = "<% =JS_SafeString(ResString("lblSignup_Password")) %>";
        var generalLoginItems = [{text:lblSignup_Email, disabled:false}, {text:lblSignup_Name, disabled:false}, {text:lblSignup_Phone, disabled:false}<% If Not isSSO() Then %>, {text:lblSignup_Password, disabled:false}<% End If %>];
        var generalLoginReqItems = [{text:lblSignup_Email, disabled:true}, {text:lblSignup_Name, disabled:true}, {text:lblSignup_Phone, disabled:true}<% If Not isSSO() Then %>, {text:lblSignup_Password, disabled:true}<% End If %>];
        var generalSignupTitle = "<%= PM.Parameters.InvitationCustomTitle %>";
        var selectedUserID = <% =PM.UserID %>;
 
        subject_default = "<% =JS_SafeString(GetSubject()) %>";

        var lblEmailFrom = "<% =JS_SafeString(ResString("lblEmailFrom")) %>";
        var lblEmailSubject = "<% =JS_SafeString(ResString("lblEmailSubject")) %>";
        var lblEmailContent = "<% =JS_SafeString(ResString("lblEmailContent")) %>";
        var btnEditInvite = "<% =JS_SafeString(ResString("btnEditInvite")) %>";
        var btnSendInvite = "<% =JS_SafeString(ResString("btnSendInvite")) %>";
        var btnDownloadMailMerge = "<% =JS_SafeString(ResString("btnMailMergeDownload")) %>";
        var btnReset = "<% =JS_SafeString(ResString("btnReset")) %>";
        var btnAddParticipants = "<% =JS_SafeString(ResString("btnAddParticipants")) %>…";
        var urlUsers = "<% =JS_SafeString(PageURL(_PGID_PROJECT_USERS)) %>";
        var btnCopy = "<% =JS_SafeString(ResString("btnCopy")) %>";
        var btnDownloadExcel = "<% =JS_SafeString(ResString("btnDownloadExcel")) %>";
        var urlProjectDownload = "<% = JS_SafeString(PageURL(_PGID_PROJECT_DOWNLOAD, _PARAM_ACTION + "=download&type=invitations")) %>";
        var lblName = "<% =JS_SafeString(ResString("lblName")) %>";
        var lblEmail = "<% =JS_SafeString(ResString("lblEmail")) %>";
        var tblUserHasData = "<% =JS_SafeString(ResString("tblUserHasData")) %>";
        var msgSaved = "<% =JS_SafeString(ResString("msgSaved")) %>";
        var fromOptions = [fromValue, "<Use Your Local Mail Client>"];
        var btnWidth = 110;
        var lblNewUserGroup = "<% =JS_SafeString(ResString("lblAssignGroupForNewUsers")) %>";
        var lblNewUserRole = "<% =JS_SafeString(ResString("lblAssignPermissionForNewUsers")) %>";
        var isAnonymous = true;
        var isResponsive = <% =Bool2JS(If(isResponsiveOnly, True, Str2Bool(GetCookie("gecko_links", "True")))) %>;
        var isResponsive_orig = <% =Bool2JS(isResponsiveOnly) %>;
        var askFields = "<% =JS_SafeString(GetCookie("invite_fld", "enp")) %>";
        var reqFields = "<% =JS_SafeString(GetCookie("invite_req", "ep")) %>";
        var groupID =  groupsList[0].id;
        var roleGroupID = (userRole_def>=0 ? userRole_def : userRoles[0].id);
        var joinAsPM = <% =Bool2JS(Str2Bool(GetCookie("invite_allowpm", "0"))) %>;
        var lblAssignAsPM = "<% =JS_SafeString(ResString("lblAssignAsPM")) %>";
        var formGeneralLinksData = {
            "Invite Link": generalLink,
            "Invite Instruction": getInviteInstruction(),
            "Sign-up page title": generalSignupTitle,
            "Sign-up page message": "Sign up Default message",
            //"New User Group": groupID,
            //"New User Role": roleGroupID
        };
        var initFinished = false;
        var msgNoSelectedUsers = "<% =JS_SafeString(ResString("msgNoSelectedUsers")) %>";
        var titleInformation = "<% =JS_SafeString(ResString("titleInformation")) %>";
        var mailmergeURL = "<% = JS_SafeString(PageURL(_PGID_PROJECT_DOWNLOAD, _PARAM_ACTION + "=download&type=mailmerge&tt=no&uid_list=")) %>";
        var joined_pipe = <% =Bool2JS(App.isRiskEnabled AndAlso PM.Parameters.EvalJoinRiskionPipes) %>;

        var tmp = "<% = GetCookie("invite_new_grp", "") %>";
        if (tmp!="") groupID = tmp*1;
        tmp = "<% = GetCookie("invite_exist_grp", "") %>";
        if (tmp != "") roleGroupID = tmp*1;

        function getFieldsValue(itemsArray) {
            retVal = ""; //enpt ["E-Mail","Name","Phone number","Password"]
            for (i = 0; i < itemsArray.length; i++) {
                if (itemsArray[i].text == generalLoginItems[0].text) {retVal += "e"};
                if (itemsArray[i].text == generalLoginItems[1].text) {retVal += "n"};
                <% if Not isSSO Then %>if (typeof generalLoginItems[3] != "undefined" && itemsArray[i].text == generalLoginItems[3].text) {retVal += "p"};<% End If %>
                if (itemsArray[i].text == generalLoginItems[2].text) {retVal += "t"};
            };
            return retVal;
        }

        function getInviteInstruction(){
            var both = <% If App.isRiskEnabled Then %>($("#divInviteBoth").length) && ($("#divInviteBoth").dxCheckBox("option", "value"))<% Else %>false<% End If %>;
            switch(linkType) {
                case generalLinkTypes[0]:
                    return (both ? bodyAnytimeAnonymousInviteBoth.f(projectName, generalLink_l, generalLink_i) : bodyAnytimeAnonymousInvite.f(projectName, generalLink));
                    break;
                case generalLinkTypes[1]:
                    return (both ? bodyAnytimeLoginInviteBoth.f(projectName, generalLink_l, generalLink_i) : bodyAnytimeLoginInvite.f(projectName, generalLink));
                    break;
                default:
                    return (both ? bodyAnytimeCopyInviteBoth.f(projectName, (isResponsive ? applicationResponsiveURL : applicationURL), "<% = JS_SafeString(App.ActiveProject.PasscodeLikelihood) %>", "<% = JS_SafeString(App.ActiveProject.PasscodeImpact) %>") : bodyAnytimeCopyInvite.f(projectName, (isResponsive ? applicationResponsiveURL : applicationURL), projectPasscode));
            }
        }

        function getInviteHint(){
            switch(linkType) {
                case generalLinkTypes[0]:
                    return hintAnytimeAnonymousInvite;
                    break;
                case generalLinkTypes[1]:
                    return hintAnytimeLoginInvite;
                    break;
                default:
                    return hintAnytimeCopyInvite;
            }
        }

        function updateOptions() {
            $("#cbAllowPM").dxCheckBox("option","disabled", (linkType !== generalLinkTypes[1]));
            $("#cbAssignNew").dxSelectBox("option","disabled", (linkType == generalLinkTypes[2]));
            $("#cbAssignExisting").dxSelectBox("option","disabled", (linkType == generalLinkTypes[2]));
        }

        function updateInviteLink() {
            if (initFinished) {
                hideLoadingPanel();
                //ByVal isAnonymous As Boolean, ByVal sAskFileds As String, ByVal sRequiredFields As String, tGroupID As Integer, tWkgRoleGroupID As Integer, JoinAsPM As Boolean
                updateOptions();
                isAnonymous = linkType == generalLinkTypes[0];
                if (linkType == generalLinkTypes[2]) {
                    var form = $("#formGeneralLinks").dxForm("instance");
                    generalLink = "{0}/?passcode={1}".f((isResponsive ? applicationResponsiveURL : applicationURL), projectPasscode);
                    $("#formGeneralLinks").dxForm("option","formData.Invite Link", generalLink);
                    $("#tbLink").dxTextBox("option", "value", generalLink);
                    $("#formGeneralLinks").dxForm("option","formData.Invite Instruction", getInviteInstruction());  
                    $("#taInstruction").dxTextArea("option", "value", getInviteInstruction());
                    $("#divInstructionHint").html(getInviteHint());
                    $("#lblAssignNew").addClass("disabled");
                    $("#lblAssignExisting").addClass("disabled");
                } else {
                    showLoadingPanel();
                    var askItems = ($("#listLoginItems").dxList("instance") ? $("#listLoginItems").dxList("instance").option("selectedItems") : "") 
                    askFields = getFieldsValue(askItems);
                    var reqItems = ($("#listReqLoginItems").dxList("instance") ? $("#listReqLoginItems").dxList("instance").option("selectedItems") : "");
                    reqFields = getFieldsValue(reqItems);
                    $("#lblAssignNew").removeClass("disabled");
                    $("#lblAssignExisting").removeClass("disabled");
                    //if ((linkType == generalLinkTypes[0])&& $("#formGeneralLinks").dxForm("instance")) {
                    //    groupID = $("#formGeneralLinks").dxForm("option","formData.New User Group");
                    //    roleGroupID = $("#formGeneralLinks").dxForm("option","formData.New User Role");
                    //};
                    //if (linkType == generalLinkTypes[1] && $("#formGeneralLinks").dxForm("instance") && $("#formSignup").dxForm("instance")) {
                    //    groupID = $("#formGeneralLinks").dxForm("option","formData.New User Group");
                    //    roleGroupID = $("#formGeneralLinks").dxForm("option","formData.New User Role");
                    //};
                    var actionStr = "action=getevalurl&isanonymous=" + isAnonymous + "&askfields=" + askFields + "&reqfields=" + reqFields + "&grpid=" + groupID + "&roleid=" + roleGroupID + "&joinaspm=" + joinAsPM + "&responsive=" + isResponsive;
                    callAjax(actionStr, onGetURL);  
                };      
            };
        }

        function updateButtons() {
            var grid = $("#dgUsers").dxDataGrid("instance");
            var selectedItems = 0;
            if ((grid)) {
                selectedItems = grid.getSelectedRowKeys().length;
            };
            if ($("#btnDownloadMailMerge").length && $("#btnDownloadMailMerge").data("dxButton")) $("#btnDownloadMailMerge").dxButton("instance").option("disabled", selectedItems == 0);
            if ($("#btnSendInvite").length && $("#btnSendInvite").data("dxButton")) $("#btnSendInvite").dxButton("instance").option("disabled", selectedItems == 0);
        }

        function loadBody() {
            var uid = "";
            if ($("#dgUsers").html()!="") {
                var grid = $("#dgUsers").dxDataGrid("instance");
                if ((grid)) {
                    var res = grid.getSelectedRowKeys()
                    if ((res) && (res.length)) {
                        uid = "&user_id=" + res[0];
                        selectedUserID = res[0];
                    } else {
                        selectedUserID = -1;
                    };
                }
            }
            var src = "<%=PageURL(_PGID_EVALUATE_INFODOC)%>"+ InviteInfodocParams + uid + "&preview=yes&r=" + (Math.floor(Math.random() * 999999)) + (isResponsive ? "" : "&ignoreval=1");
            var f = document.getElementById("frmBody");
            if ((f)) {
                if (src == "") {
                    f.src = "";
                } else {
                    initFrameLoader(f);
                    setTimeout('document.getElementById("frmBody").src="' + src +'";', 30);
                }
            }
        }

        function onResize() {
            $("#tabs").height(300).height($("#trTabs").height()-24);
            if (!is_ie) {
                $("#taLinks").height(200).height($("#tdLinks").height()-24);
                $("#taGroupLinks").height(200).height($("#tdGroupLinks").height()-24);
            }
            var g = $("#dgUsers");
            var frm = $("#divBody");
            if ((g) && (g.length) && g.html()!="" && (frm) && (frm.length)) {
                g.height(200).width(200);
                frm.height(100);

                var h = Math.round($("#tblEmail").height()-10);
                var w = Math.round($("#tblEmail").width());
                //var grid = g.dxDataGrid("instance");
                //if (grid) {
                //    if (h>100) grid.option("height", h);
                //    if (w>100) grid.option("width", Math.round(w/2));
                //    setTimeout('$("#dgUsers").dxDataGrid("instance").repaint();', 100);
                //}
                if (h>100) g.height(h);
                if (w>100) g.width(Math.round(w/2));

                var p = frm.parent().parent().parent();
                if ((p) && (p.length)) {
                    var frm_h = Math.round(h - p.prop("offsetTop")-4);
                    if (frm_h>100) frm.height(frm_h);
                }
            }
            //var l = $("#linkTypeSet");
            //if ((l) && (l.length)) {
            //    l.width(400);
            //    l.width($("#tabs").width()-48);
            //}
        }

        function selectedUsers() {
            var res = "";
            if ($("#dgUsers").html()!="") {
                var grid = $("#dgUsers").dxDataGrid("instance");
                if ((grid)) {
                    res = grid.getSelectedRowKeys().join(",");
                }
            }
            return res;
        }

        function onRichEditorRefresh(empty, infodoc, callback_param) {
            window.focus();
            loadBody();
        }

        function onSaveSubject(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    DevExpress.ui.notify(msgSaved);
                }
            }
            if (res.Message!="") {
                showResMessage(res, true);
            }
        }

        function onSent(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    DevExpress.ui.notify("Send e-mails: " + res.Data);
                }
            }
            if (res.Message!="") {
                showResMessage(res, true);
            }
        }

        function onSentLocal(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    document.location.href=res.Data;
                }
            }
            if (res.Message!="") {
                showResMessage(res, true);
            }
        }

        function onReset(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    var form = $("#formEmail").dxForm("instance");
                    form.getEditor("Subject").option("value", subject_default);
                    loadBody();
                }
            }
            if (res.Message!="") {
                showResMessage(res, true);
            }
        }

        function onGetURL(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    var resData = eval(res.Data);
                    generalLink = resData[0];
                    groupsLinks = resData[1];
                    usersLinks = resData[2];
                    <% If App.isRiskEnabled Then %>generalLink_l = ((resData[3]) ? resData[3] : generalLink);
                    generalLink_i = ((resData[4]) ? resData[4] : generalLink);<% End If %>
                    $("#taGroupLinks").dxTextArea("option","value", groupsLinks);
                    $("#formGeneralLinks").dxForm("option","formData.Invite Link", generalLink);
                    $("#tbLink").dxTextBox("option", "value", generalLink);
                    $("#formGeneralLinks").dxForm("option","formData.Invite Instruction", getInviteInstruction());
                    $("#taInstruction").dxTextArea("option", "value", getInviteInstruction());
                    $("#divInstructionHint").html(getInviteHint());
                    $("#taLinks").dxTextArea("option", "value", usersLinks);
                }
            }
            hideLoadingPanel();
            if (res.Message!="") {
                showResMessage(res, true);
            }
        }

        function initEmailEditor() {
            $("#formEmail").dxForm({
                formData: formEmailData,
                colCount: 1,
                items: [
                    {
                        dataField: "From",
                        editorType: "dxSelectBox",
                        editorOptions: {
                            items: fromOptions
                        },
                        label: {
                            alignment: "right",
                            text: lblEmailFrom
                        }
                    },
                    {
                        dataField: "Subject",
                        label: {
                            alignment: "right",
                            text: lblEmailSubject
                        },
                        editorOptions: {
                            onChange: function(e) {
                                callAjax("action=subject&val=" + encodeURIComponent(e.component.option("value")), onSaveSubject, _method_GET, true);
                            }
                        },
                    },
                    {
                        dataField: "Body",
                        label: {
                            alignment: "right",
                            text: lblEmailContent
                        },
                        template: function (data, itemElement) {
                            itemElement.append("<div id='divBody' style='padding:2px; height:100%;' class='dx-editor-outlined dx-texteditor'><iframe src='' id='frmBody' style='width:100%; height:100%; background:#ffffff;' frameborder='0'></iframe></div>");
                            loadBody();
                        }
                    }
                ]
            });
            $("#btnEditInvite").dxButton({
                text: btnEditInvite,
                icon: "edit",
                width: btnWidth,
                onClick: function (e) {
                    OpenRichEditor(InviteInfodocParams)
                }
            });
            $("#btnSendInvite").dxButton({
                text: btnSendInvite,
                icon: "message",
                disabled: (selectedUsers() == ""),
                width: btnWidth,
                type: "default",
                onClick: function (e) {
                    if (selectedUsers() == "") {
                        DevExpress.ui.dialog.alert(msgNoSelectedUsers, titleInformation);
                    } else {
                        if (isOnline) {
                            doSendEmails(curPrjID, isOnline);
                        } else {
                            askSetProjectOnlineAndAction(<% =App.ProjectID %>, doSendEmails, doSendEmails);
                        }
                    }
                }
            });
            $("#btnDownloadMailMerge").dxButton({
                text: btnDownloadMailMerge,
                icon: "download",
                disabled: (selectedUsers() == ""),
                onClick: function (e) {
                    if (selectedUsers() == "") {
                        DevExpress.ui.dialog.alert(msgNoSelectedUsers, titleInformation);
                    } else {
                        CreatePopup(mailmergeURL + encodeURIComponent(selectedUsers()), "download", "");
                    }
                }
            });
            $("#btnReset").dxButton({
                text: btnReset,
                icon: "fas fa-undo-alt",
                width: btnWidth,
                onClick: function (e) {
                    var result = DevExpress.ui.dialog.confirm("<%=JS_SafeString(ResString("confResetInvite")) %>", "<%=JS_SafeString(ResString("titleConfirmation")) %>");
                    result.done(function (dialogResult) {
                        if (dialogResult) {
                            callAjax("action=reset", onReset);
                        }
                    });
                }
            });
            $("#btnAddParticipants").dxButton({
                text: btnAddParticipants,
                icon: "user",
                width: btnWidth + 48,
                onClick: function (e) {
                    showLoadingPanel();
                    document.location.href = urlUsers;
                }
            });
        }

        var manual_both = false
        function onSetBoth(both) {
            if (!manual_both) {
                manual_both = true;
                document.cookie = 'invite_both=' + ((both) ? "1" : "0");
                invite_both = (both);
                if ($("#divInviteBoth").length && $("#divInviteBoth").data("dxCheckBox")) $("#divInviteBoth").dxCheckBox("option", "value", (both));
                if ($("#formGeneralLinks").length && $("#formGeneralLinks").data("dxForm")) $("#formGeneralLinks").dxForm("option","formData.Invite Instruction", getInviteInstruction());  
                if ($("#taInstruction").length && $("#taInstruction").data("dxTextArea")) $("#taInstruction").dxTextArea("option", "value", getInviteInstruction());
                loadBody();
                manual_both = false;
            }
        }

        function signupOptionsEnable(isEnabled) {
            $("#formSignup").dxForm("option", "disabled", !isEnabled);
        }

        function onChangeMode(val) {
            signupOptionsEnable(linkType == generalLinkTypes[1]);
            //if (linkType == generalLinkTypes[2]) {
            //    $("#formGeneralLinks").dxForm('instance').getEditor("New User Group").option("disabled", true);
            //    $("#formGeneralLinks").dxForm('instance').getEditor("New User Role").option("disabled", true);
            //} else {
            //    $("#formGeneralLinks").dxForm('instance').getEditor("New User Group").option("disabled", false);
            //    $("#formGeneralLinks").dxForm('instance').getEditor("New User Role").option("disabled", false);             
            //};
            updateInviteLink();
        }

        function initGeneralLinks() {
            showLoadingPanel();
            initFinished = false;
            $("input[name='linkType']").change(function(e){
                linkType = $(this).val();
                onChangeMode(linkType);
                var idx = -1;
                switch (linkType) {
                    case generalLinkTypes[0]:
                        idx = 0; break;
                    case generalLinkTypes[1]:
                        idx = 1; break;
                    case generalLinkTypes[2]:
                        idx = 2; break;
                }
                if (idx>=0) document.cookie = 'invite_mode=' + idx;
            });
            $("#formGeneralLinks").dxForm({
                formData: formGeneralLinksData,
                labelLocation: "left",
                //onFieldDataChanged: function(e) {
                //    if ((e.dataField == "New User Group")||(e.dataField == "New User Role")) {
                //        updateInviteLink();
                //    };
                //},
                items: [{
                    itemType: "group",
                    items: [
                            {
                            dataField: "Invite Link",
                            editorOptions: { 
                                readOnly: true
                            },                                        
                            template: function (data, itemElement) {
                                itemElement.append("<table width='100%'><tr><td><div id='tbLink'></div></td><td width='50px'><div id='btnCopyLink'></div></td></tr></table>");
                                $("#tbLink").dxTextBox({
                                    value: data.editorOptions.value,
                                    readOnly: true
                                });
                                $("#btnCopyLink").dxButton({
                                    text: "<% =JS_SafeString(ResString("btnCopy")) %>",
                                    icon: "far fa-copy",
                                    onClick: function(e){
                                        var f = function (prjid, is_online) {
                                            isOnline = is_online;
                                            copyDataToClipboard(generalLink);
                                        }
                                        if (isOnline) {
                                            f(curPrjID, isOnline);
                                        } else {
                                            askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                                        }
                                    }
                                });
                            }
                        },
                        {
                            dataField: "Invite Instruction",
                            template: function (data, itemElement) {
                                itemElement.append("<table width='100%'><tr><td><div id='taInstruction'></div><div id='divInviteBoth' style='margin-top:1ex; display:none'></div><% If App.isRiskEnabled Then %><div class='small'><% =ResString(If(PM.Parameters.EvalJoinRiskionPipes, "msgRiskionJoinedPipe", "msgRiskionSinglePipe")) %> <nobr>[<a href='<% =PageURL(_PGID_PROJECT_OPTION_NAVIGATE) %>' onclick='navOpenPage(<% =_PGID_PROJECT_OPTION_NAVIGATE %>, true); return false;' class='dashed actions'><% =ResString("lblChangeOptions") %></a>]</nobr></div><% End If%><% If Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY Then %><div id='divInstructionHint' style='margin-top:1em; opacity:0.8;'></div><% End If %></td><td width='50px' style='vertical-align: top;'><div id='btnCopyInstruction'></div></td></tr></table>");
                                $("#taInstruction").dxTextArea({
                                    value: data.editorOptions.value,
                                    height: 100,
                                    readOnly: true
                                });
                                $("#btnCopyInstruction").dxButton({
                                    text: "<% =JS_SafeString(ResString("btnCopy")) %>",
                                    icon: "far fa-copy",
                                    onClick: function(e){
                                        var f = function (prjid, is_online) {
                                            isOnline = is_online;
                                            copyDataToClipboard(getInviteInstruction());
                                        }
                                        if (isOnline) {
                                            f(curPrjID, isOnline);
                                        } else {
                                            askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                                        }
                                    }
                                });
                                <% If App.isRiskEnabled Then %>$("#divInviteBoth").show().dxCheckBox({
                                    text: "<% =JS_SafeString(ResString("lblInviteUseBothLinks")) %>",
                                    value: (joined_pipe ? false : invite_both),
                                    disabled: joined_pipe,
                                    onValueChanged: function(e){
                                        onSetBoth(e.value);
                                    }    
                                });<% End if %>
                            }
                        }
                    ]
                }, 
                //{
                //    itemType: "group",
                //    colCount: 2,
                //    items: [
                //        {
                //            dataField: "New User Group",
                //            label: {
                //                location: "top",
                //                text: lblNewUserGroup
                //            },
                //            editorType: "dxSelectBox",
                //            editorOptions: {
                //                dataSource: groupsList,
                //                displayExpr: "name",
                //                valueExpr: "id",
                //                value: groupsList[0].id,
                //            }
                //        },
                //        {
                //            dataField: "New User Role",
                //            label: {
                //                location: "top",
                //                text: lblNewUserRole
                //            },
                //            editorType: "dxSelectBox",
                //            editorOptions: {
                //                dataSource: userRoles,
                //                displayExpr: "name",
                //                valueExpr: "id",
                //                value: userRoles[0].id
                //            }
                //        },
                //    ]
                //}
                ]
            });
            $("#cbAssignNew").dxSelectBox({
                //name: "New User Group",
                disabled: true,
                dataSource: groupsList,
                displayExpr: "name",
                valueExpr: "id",
                value: groupID,
                onValueChanged: function(e){
                    groupID = e.value;
                    document.cookie = 'invite_new_grp=' + groupID;
                    updateInviteLink();
                }    
            });

            $("#cbAssignExisting").dxSelectBox({
                //name: "New User Role",
                disabled: true,
                dataSource: userRoles,
                displayExpr: "name",
                valueExpr: "id",
                value: roleGroupID,
                onValueChanged: function(e){
                    for (var k=0; k<userRoles.length; k++) {
                        if (e.value == userRoles[k].id) {
                            if (typeof userRoles[k].can_eval != "undefined") isResponsive = (userRoles[k].can_eval ? isResponsive_orig : false);
                            break;
                        }
                    }
                    roleGroupID = e.value;
                    document.cookie = 'invite_exist_grp=' + roleGroupID;
                    updateInviteLink();
                }    
            });

            $("#cbAllowPM").dxCheckBox({
                text: lblAssignAsPM,
                disabled: true,
                value: joinAsPM,
                onValueChanged: function(e){
                    joinAsPM = e.value;
                    document.cookie = 'invite_allowpm=' + (joinAsPM ? "1" : "0");
                    updateInviteLink();
                }    
            });
            updateOptions();
            if ($("#formSignup").html()=="") initFormSignup();
            var m = "<% = If(isSSO_Only() AndAlso Not _OPT_SHOW_LINKS_WHEN_SSO_ONLY, "2", GetCookie("invite_mode", "")) %>";
            if (m!="" && m*1>=0) {
                var r = $("input[name='linkType']");
                if (typeof r[m*1] != "undefined") {
                    linkType = generalLinkTypes[m*1]
                    $("input[name='linkType']").val([linkType]);
                    onChangeMode(linkType);
                }
            }
            $("#scrollview").dxScrollView({});
            initFinished = true;
            setTimeout(function () {
                updateInviteLink();
            }, 30);

            $("#btnManageGroups").dxButton({
                "icon": "fa fa-user-cog",
                "hint": "Manage Groups [Navigate to other page]",
                onClick: function () {
                    loadURL('<%=JS_SafeString(PageURL(_PGID_PROJECT_USERS, "dlg=groups")) %>');
                }
            });
        }

        function initFormSignup() {
            var sel_fld = [];
            var sel_req = [];
            if (askFields.indexOf("e")>=0) { sel_fld.push(generalLoginItems[0]); generalLoginReqItems[0].disabled = false; }
            if (askFields.indexOf("n")>=0) { sel_fld.push(generalLoginItems[1]); generalLoginReqItems[1].disabled = false; }
            if (askFields.indexOf("t")>=0) { sel_fld.push(generalLoginItems[2]); generalLoginReqItems[2].disabled = false; }
            <% If Not isSSO Then %>if (askFields.indexOf("p")>=0) { sel_fld.push(generalLoginItems[3]); generalLoginReqItems[3].disabled = false; }<% End If %>
            if (reqFields.indexOf("e")>=0) sel_req.push(generalLoginReqItems[0]);
            if (reqFields.indexOf("n")>=0) sel_req.push(generalLoginReqItems[1]);
            if (reqFields.indexOf("t")>=0) sel_req.push(generalLoginReqItems[2]);
            <% If Not isSSO Then %>if (reqFields.indexOf("p")>=0) sel_req.push(generalLoginReqItems[3]);<% End If %>

            $("#formSignup").dxForm({
                formData: formGeneralLinksData,
                //disabled: true,
                onFieldDataChanged: function(e) {
                    updateInviteLink();
                },
                items: [
                    {
                        itemType: 'group',
                        //caption: 'Sign-up page options',
                        colCount: 4,
                        items: [
                            {
                                dataField: "Sign-up form fields",
                                label: { 
                                    location: "top",
                                    alignment: "left"
                                },
                                template: function (data, itemElement) {
                                    itemElement.append("<div id='listLoginItems'></div>");
                                    $("#listLoginItems").dxList({
                                        items: generalLoginItems,
                                        showSelectionControls: true,
                                        selectionMode: "multiple",
                                        selectedItems: sel_fld,
                                        onSelectionChanged: function(e){
                                            updateInviteLink();
                                            document.cookie = "invite_fld=" + getFieldsValue(e.component.option("selectedItems"));
                                            var reqList = $("#listReqLoginItems").dxList("instance");
                                            if (reqList) {
                                                for (i = 0; i < e.addedItems.length; i++) {
                                                    selItem = e.addedItems[i];
                                                    for (j = 0; j < generalLoginReqItems.length; j++) {
                                                        reqitem = generalLoginReqItems[j];
                                                        if (reqitem.text == selItem.text) {
                                                            generalLoginReqItems[j].disabled = false;
                                                        };
                                                    };
                                                };
                                                for (i = 0; i < e.removedItems.length; i++) {
                                                    selItem = e.removedItems[i];
                                                    for (j = 0; j < generalLoginReqItems.length; j++) {
                                                        reqitem = generalLoginReqItems[j];
                                                        if (reqitem.text == selItem.text) {
                                                            generalLoginReqItems[j].disabled = true;
                                                        };
                                                    };
                                                };
                                                $("#listReqLoginItems").dxList("option", "items", generalLoginReqItems);
                                            };
                                        }
                                    });
                                }
                            },
                            {
                                dataField: "Required form fields",
                                label: { 
                                    location: "top",
                                    alignment: "left"
                                },
                                template: function (data, itemElement) {
                                    itemElement.append("<div id='listReqLoginItems'></div>");
                                    $("#listReqLoginItems").dxList({
                                        items: generalLoginReqItems,
                                        showSelectionControls: true,
                                        selectionMode: "multiple",
                                        selectedItems: sel_req,
                                        onSelectionChanged: function(e){
                                            updateInviteLink();
                                            document.cookie = "invite_req=" + getFieldsValue(e.component.option("selectedItems"));
                                        }
                                    });
                                    //itemElement.parent().css({ 'pointer-events': 'none' });
                                    //itemElement.parent().find(".dx-checkbox").dxCheckBox("instance").option("disabled", true);

                                }
                            },
                            {
                                itemType: 'group',
                                colSpan: 2,
                                items: [
                                        {
                                            dataField: "Sign-up page title",
                                            label: { 
                                                location: "top",
                                                alignment: "left",
                                                showColon: true,
                                                text: "Sign-up page title"
                                            },
                                            editorOptions: {
                                                onChange: function(e) {
                                                    callAjax("action=signuptitle&val=" + encodeURIComponent(e.component.option("value")), onSaveSubject, _method_GET, true);
                                                }
                                            }
                                        },
                                        {
                                                    
                                            itemType: 'group',
                                            colSpan: 2,
                                            items: [
                                        {
                                            dataField: "Sign-up page message",
                                            label: { 
                                                location: "top",
                                                alignment: "left",
                                                showColon: false,
                                                text: " "
                                            },
                                            template: function(data, itemElement) {
                                                itemElement.append('<span style="white-space: nowrap;"><div id="btnEditMessage"></div><div id="btnPreview" style="margin:2px;"></div></span>');
                                                $("#btnEditMessage").dxButton({
                                                    text: "Edit Sign-up page message",
                                                    icon: "edit",
                                                    onClick: function (e) {
                                                        OpenRichEditor(SignupMessageParams)
                                                    }
                                                });
                                                $("#btnPreview").dxButton({
                                                    text: "",
                                                    icon: "search",
                                                    onClick: function (e) {
                                                        CreatePopup("<%=PageURL(_PGID_EVALUATE_INFODOC)%>"+SignupMessageParams, "SignupPageMessage", "menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=600,height=400")
                                                    }
                                                });
                                            }
                                        },
                                            ]
                                        }
                                ]
                            }
                        ] 
                    }
                ]
            });
            //updateInviteLink();
        }

        function initUsersList() {
            $("#dgUsers").dxDataGrid({
                dataSource: new DevExpress.data.ArrayStore({
                    data: dataSourceUsers,
                    key: "id"
                }),
                rowAlternationEnabled: true,
                columnAutoWidth: true,
                allowColumnResizing: true,
                columnChooser: { 
                    enabled: true,
                    mode: "select"
                },
                //height: 300,
                loadPanel: {
                    enabled: true,
                },
                loadPanel: main_loadPanel_options,
                showBorders: true,
                hoverStateEnabled: true,
                filterRow: {
                    visible: true,
                    applyFilter: "auto"
                },
                columns:[
                       {
                           dataField: "name",
                           caption: lblName
                       },
                    {
                        dataField: "email",
                        caption: lblEmail
                    },
                    {dataField: "hasdata",
                        width: 70,
                        lookup: {
                            dataSource: hasData,
                            displayExpr: "Name",
                            valueExpr: "ID"
                        },
                        caption: tblUserHasData,
                        alignment: "center"
                    },
                    {dataField: "progress",
                        allowFiltering: false,
                        width: 70,
                        format: {
                            type: "percent",
                            precision: 1
                        }}
                ],
                //scrolling: {
                //    mode: "standard"
                //},
                paging: {
                    enabled: true,
                    pageSize: 200
                },
                searchPanel: {
                    visible: true,
                    placeholder: resString("btnDoSearch") + "..."
                },
                selection: {
                    mode: "multiple",
                    allowSelectAll: true,
                    showCheckBoxesMode: "always"
                },
                onContentReady: function (e) {
                    updateButtons();
                },
                onInitialized: function (e) {
                    setTimeout('onResize();', 500);
                },
                onSelectionChanged: function (e) {
                    updateButtons();
                    if (e.component.getSelectedRowKeys().length<2) loadBody();
                    setTimeout('onResize();', 300);
                }
            });
        }

        function doSendEmails(prjid, is_online) {
            var fromVal = $("#formEmail").dxForm("option","formData.From");
            if (fromVal == fromOptions[0]) {
                callAjax("action=send&users=" + selectedUsers() + (isResponsive ? "" : "&ignoreval=1"), onSent);
            } else {
                if (selectedUserID >= 0) {callAjax("action=sendlocal&uid=" + selectedUserID, onSentLocal)};
            };
            isOnline = is_online;
        }

        resize_custom = onResize;

        $(document).ready(function () {
            showLoadingPanel();
            $("#cbResponsive").dxCheckBox({
                text: "Use Responsive UI Links",
                hint: "Generate invitation links for \"Responsive\" evaluation pages",
                value: isResponsive,
                onValueChanged: function (e) {
                    isResponsive = e.value;
                    document.cookie = "gecko_links=" + (isResponsive ? 1 : 0);
                    loadBody();
                    updateInviteLink();
                }
            });

            $("#tabs").dxTabPanel({
                items: tabs,
                //animationEnabled: false,
                //swipeEnabled: false,
                focusStateEnabled: false,
                //deferRendering: false,
                selectedIndex: "<% = If(CanSeeLinks, GetCookie("invite_tab", "0"), "0") %>",
                onItemRendered: function(e) {
                    showLoadingPanel();
                    initFinished = false;
                    switch (e.itemIndex) {
                        case 0:
                            setTimeout('initEmailEditor(); initUsersList(); ', 150);
                            break;

                        case 1:
                            setTimeout('initGeneralLinks();', 50);
                            setTimeout('signupOptionsEnable(linkType == generalLinkTypes[1]);', 350);
                            break;

                        case 2:
                            $("#taLinks").dxTextArea({
                                value: usersLinks,
                                readOnly: true
                            });
                            $("#btnCopyLinks").dxButton({
                                text: btnCopy,
                                icon: "far fa-copy",
                                width: btnWidth,
                                onClick: function (e) {
                                    var f = function (prjid, is_online) {
                                        isOnline = is_online;
                                        copyDataToClipboard(usersLinks);
                                    }
                                    if (isOnline) {
                                        f(curPrjID, isOnline);
                                    } else {
                                        askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                                    }
                                }
                            });
                            $("#btnDownloadLinks").dxButton({
                                text: btnDownloadExcel,
                                icon: "download",
                                width: btnWidth + 48,
                                onClick: function (e) {
                                    var f = function (prjid, is_online) {
                                        isOnline = is_online;
                                        CreatePopup(urlProjectDownload + "&ignoreval=" + (isResponsive ? "0" : "1"), "download", "");
                                    }
                                    if (isOnline) {
                                        f(curPrjID, isOnline);
                                    } else {
                                        askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                                    }
                                }
                            });
                            break;

                        case 3:
                            $("#taGroupLinks").dxTextArea({
                                value: groupsLinks,
                                readOnly: true
                            });
                            $("#btnCopyGroupLinks").dxButton({
                                text: btnCopy,
                                icon: "far fa-copy",
                                width: btnWidth,
                                onClick: function (e) {
                                    var f = function (prjid, is_online) {
                                        isOnline = is_online;
                                        copyDataToClipboard(groupsLinks);
                                    }
                                    if (isOnline) {
                                        f(curPrjID, isOnline);
                                    } else {
                                        askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                                    }
                                }
                            });
                            break;
                        }
                    initFinished = true;
                    setTimeout('hideLoadingPanel(); onResize();', 300);
                },
                onSelectionChanged: function (e) {
                    document.cookie = 'invite_tab=' + 1*e.component.option("selectedIndex");
                    onResize();
                }

            });
        });

    </script>
</asp:Content>