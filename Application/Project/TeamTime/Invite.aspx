<%@ Page Language="vb" CodeBehind="Invite.aspx.vb" Inherits="TeamTimeInvitePage" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server">
    <script type="text/template" id="tmpInviteByEmail">
        <div class="table">
            <div class="tr">
                <div class="td">
                    <div id="formEmail" class="whole" style="padding:1em"></div>
                </div>
            </div>
            <div class="tr" style="height:1ex;">
                <div class="td" style="padding:0px 1em 1em 0px;text-align:right">
                    <div id="btnEditInvite"></div>
                    <div id="btnSendInvite"></div>
                    <div id="btnViewSelected"></div>
                    <div id="btnReset"></div>
                    <div id="btnAddParticipants"></div>
                    <div id="btnDownload"></div>
                </div>
            </div>
        </div>
    </script>
    <script type="text/template" id="tmpInviteByPhone">
        <div class="whole" style="padding:1em;"><div id="taInviteByPhone" class="whole" style="padding:1em"></div></div>
    </script>
    <script type="text/template" id="tmpCopyPaste">
        <div class="table">
            <div class="tr">
                <div class="td" style="padding:1em;">
                    <div id="taCopyPaste" class="whole" style="padding:1em"></div>
                </div>
            </div>
            <div class="tr" style="height:1em;">
                <div class="td" style="padding:0px 1em 1em 0px;text-align:right">
                    <div id="btnCopy"></div>
                </div>
            </div>
        </div>
    </script>
    <script type="text/template" id="tmpLinks">
        <div class="table">
            <div class="tr">
                <div class="td" style="padding:1em;">
                    <div id="taLinks" class="whole" style="padding:1em"></div>
                </div>
            </div>
            <div class="tr" style="height:1em;">
                <div class="td" style="padding:0px 1em 1em 0px;text-align:right">
                    <div id="btnDownloadLinks"></div>
                    <div id="btnCopyLinks"></div>
                </div>
            </div>
        </div>
    </script>
    <script type="text/javascript">

        var isOnline = <% =Bool2JS(App.ActiveProject.isOnline) %>;
        var selectedUsers = "<% =GetSelectedUsers %>";
        var selectedIDs = "<% =GetSelectedUserIDs %>";

        var usersLinks = "<%= GetUserLinks() %>";
        var copyPasteText = "<% =JS_SafeString(ResString(If(isSSO(), "bodyInviteCopyPasteSSO", "bodyInviteCopyPaste")), True) %>";

        var tabs = [
            { title: "<% =JS_SafeString(ResString("tabSyncInvitationsByEmail")) %>", icon: "email", template: tmpInviteByEmail },
            { title: "<% =JS_SafeString(ResString("tabSyncInvitationsByPhone")) %>", icon: "tel", template: tmpInviteByPhone },
            { title: "<% =JS_SafeString(ResString("tabSyncInvitationsCopyPaste")) %>", icon: "fas fa-copy", template: tmpCopyPaste },
            { title: "<% =JS_SafeString(ResString("tabSyncInvitationsLinks")) %>", icon: "fas fa-link", template: tmpLinks, visible: <% =Bool2JS(Not isSSO_Only() OrElse _OPT_SHOW_LINKS_WHEN_SSO_ONLY) %> }
        ];

        var InviteInfodocParams = "<%=JS_SafeString(String.Format("?project={0}&type=8&id={1}", App.ProjectID, _PARAM_INVITATION_TT))%>";

        $(document).ready(function () {
            var formEmailData = {
                "From": "<% =JS_SafeString(SystemEmail) %>",
                "Subject": "<% =JS_SafeString(GetSubject()) %>"
            }

            subject_default = "<% =JS_SafeString(ResString("subjectTeamTimeInvitation")) %>";

            var sel_cnt = (selectedIDs == "" ? 0 : selectedIDs.split(",").length);

            $("#tabs").dxTabPanel({
                items: tabs,
                animationEnabled: false,
                swipeEnabled: true,
                focusStateEnabled: false,
                onItemRendered: function (e) {
                    $("#taInviteByPhone").dxTextArea({
                        readOnly: true,
                        value: "<% =JS_SafeString(ResString(If(isSSO(), "bodyInviteByPhoneSSO", "bodyInviteByPhone")), True) %>"
                    });
                    $("#formEmail").dxForm({
                        formData: formEmailData,
                        colCount: 1,
                        items: [
                            {
                                dataField: "From",
                                label: {
                                    text: "<% =JS_SafeString(ResString("lblEmailFrom")) %>"
                                },
                                editorOptions: {
                                    disabled: true
                                }
                            },
                            {
                                dataField: "Subject",
                                label: {
                                    text: "<% =JS_SafeString(ResString("lblEmailSubject")) %>"
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
                                    text: "<% =JS_SafeString(ResString("lblEmailContent")) %>"
                                },
                                template: function (data, itemElement) {
                                    itemElement.append("<div id='divBody' style='padding:2px' class='dx-editor-outlined dx-texteditor'><iframe src='' id='frmBody' style='width:100%; height:100%; background:#ffffff;' frameborder='0'></iframe></div>");
                                    setTimeout('resizeEmailBody();', 200);
                                    setTimeout('loadBody();', 50);
                                }
                            }
                        ]
                    });
                    $("#btnEditInvite").dxButton({
                        text: "<% =JS_SafeString(ResString("btnEditInvite")) %>",
                        width: 120,
                        icon: "edit",
                        onClick: function (e) {
                            OpenRichEditor(InviteInfodocParams + "&callback=invitation");
                        }
                    });
                    $("#btnSendInvite").dxButton({
                        text: "<% =JS_SafeString(ResString("btnSendInvite")) %>",
                        icon: "message",
                        disabled: !sel_cnt,
                        width: 120,
                        onClick: function (e) {
                            if (selectedIDs == "") {
                                DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("msgNoSelectedUsers")) %>", "<% =JS_SafeString(ResString("titleInformation")) %>");
                            } else {
                                var f = function (prjid, is_online) {
                                    isOnline = is_online;
                                    callAjax("action=send&ignoreval=1", onSent);
                                }
                                if (isOnline) {
                                    f(curPrjID, isOnline);
                                } else {
                                    askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                                }
                            }
                        }
                    });
                    $("#btnViewSelected").dxButton({
                        icon: "fas fa-user-check",
                        text: "(" + sel_cnt + ")",
                        onClick: function (e) {
                            var lst = ((selectedUsers!="") ? "<b><% =JS_SafeString(ResString("lblPrivate")) %>:</b><ul style='padding:1ex 0px 0px 2em; max-height:400px;overflow-y:auto;'>" + selectedUsers + "</ul>": "<% =JS_SafeString(ResString("msgNoSelectedUsers")) %>");
                            DevExpress.ui.dialog.alert(lst, "<% =JS_SafeString(ResString("titleInformation")) %>");
                        }
                    });
                    $("#btnReset").dxButton({
                        text: "<% =JS_SafeString(ResString("btnReset")) %>",
                        icon: "revert",
                        width: 100,
                        onClick: function (e) {
                            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgResetCustomInvites")) %>", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
                            result.done(function (dialogResult) {
                                if (dialogResult) {
                                    callAjax("action=reset", onReset);
                                }
                            });
                        }
                    });
                    $("#btnAddParticipants").dxButton({
                        text: "<% =JS_SafeString(ResString("btnAddParticipants")) %>",
                        icon: "user",
                        width: 150,
                        onClick: function (e) {
                            showLoadingPanel();
                            document.location.href = "<% =JS_SafeString(PageURL(_PGID_PROJECT_USERS)) %>";
                        }
                    });
                    $("#btnDownload").dxButton({
                        text: "<% =JS_SafeString(ResString("btnDownloadMailMerge")) %>",
                        width: 240,
                        icon: "download",
                        //disabled: (selectedUsers == ""),
                        onClick: function (e) {
                            if (selectedUsers == "") {
                                DevExpress.ui.dialog.alert("<% =JS_SafeString(ResString("msgNoSelectedUsers")) %>", "<% =JS_SafeString(ResString("titleInformation")) %>");
                            } else {
                                CreatePopup("<% = JS_SafeString(PageURL(_PGID_PROJECT_DOWNLOAD, _PARAM_ACTION + "=download&type=mailmerge&tt=yes&uid_list=")) %>" + encodeURIComponent(selectedUsers), "download", "");
                            }
                        }
                    });
                    $("#taCopyPaste").dxTextArea({
                        readOnly: true,
                        value: copyPasteText
                    });
                    $("#btnCopy").dxButton({
                        text: "<% =JS_SafeString(ResString("btnCopy")) %>",
                        icon: "far fa-copy",
                        width: 120,
                        onClick: function (e) {
                            var f = function (prjid, is_online) {
                                isOnline = is_online;
                                copyDataToClipboard(copyPasteText);
                            }
                            if (isOnline) {
                                f(curPrjID, isOnline);
                            } else {
                                askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                            }
                        }
                    });
                    $("#taLinks").dxTextArea({
                        value: usersLinks,
                        readOnly: true
                    });
                    $("#btnCopyLinks").dxButton({
                        text: "<% =JS_SafeString(ResString("btnCopy")) %>",
                        disabled: (usersLinks == ""),
                        icon: "far fa-copy",
                        width: 120,
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
                        text: "<% =JS_SafeString(ResString("btnDownloadExcel")) %>",
                        icon: "fa fa-download",
                        width: 180,
                        disabled: (usersLinks == ""),
                        onClick: function (e) {
                            var f = function (prjid, is_online) {
                                isOnline = is_online;
                                CreatePopup("<% = JS_SafeString(PageURL(_PGID_PROJECT_DOWNLOAD, _PARAM_ACTION + "=download&type=tt_invitations")) %>", "download", "");
                            }
                            if (isOnline) {
                                f(curPrjID, isOnline);
                            } else {
                                askSetProjectOnlineAndAction(<% =App.ProjectID %>, f, f);
                            }
                        }
                    });
                }
            });
        });

        function resizeEmailBody() {
            $("#divBody").height(100).height(Math.round($("#formEmail").height() - 70));
        }

        function loadBody() {
            var src = "<%=PageURL(_PGID_EVALUATE_INFODOC)%>" + InviteInfodocParams + "&preview=yes&ignoreval=1&r=" + (Math.floor(Math.random() * 999999));
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
            resizeEmailBody();
        }

        resize_custom = onResize;

        function onRichEditorRefresh(empty, infodoc, callback_param) {
            window.focus();
            loadBody();
        }

        function onSaveSubject(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    DevExpress.ui.notify("<% =JS_SafeString(ResString("msgSaved")) %>");
                }
            }
            if (res.Message!="") {
                DevExpress.ui.dialog.alert(res.Message, "<% =JS_SafeString(ResString("titleError")) %>");
            }
        }

        function onSent(data) {
            var res = parseReply(data);
            if (res) {
                if (res.Result == _res_Success) {
                    var msg = replaceString("{0}", res.ObjectID * 1, "<% =JS_SafeString(ResString("msgSentReport")) %>");
                    if (typeof (res.Data) != "undefined" && res.Data != "") { msg = "<b>" + msg + "</b>\r\n\r\n<span class='error'><% =JS_SafeString(ResString("lblErrors")) %></span>: \r\n" + res.Data }
                    DevExpress.ui.dialog.alert(replaceString("\r", "<br>", msg), "<% =JS_SafeString(ResString("titleSendMail")) %>");
                }
            }
            if (res.Message!="") {
                DevExpress.ui.dialog.alert(res.Message, "<% =JS_SafeString(ResString("titleError")) %>");
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
                DevExpress.ui.dialog.alert(res.Message, "<% =JS_SafeString(ResString("titleError")) %>");
            }
       }

    </script>
<div class="table">
    <div class="tr">
        <div class="td" style="padding:1em;">
            <div id="tabs" class="whole ec_tabs"></div>
        </div>
    </div>
</div>
</asp:Content>
