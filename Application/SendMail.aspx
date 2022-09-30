<%@ Page Language="vb" CodeBehind="SendMail.aspx.vb" MasterPageFile="~/mpPopup.Master" Strict="true" Inherits="SendMailPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript" src="/Scripts/dx.all.js"></script>
<script type="text/javascript" src="/Scripts/misc.js"></script>
<script type="text/javascript" src="/Scripts/masterpage.js"></script>
<script language="javascript" type="text/javascript">

    var on_ok_code = "";
    var is_ajax = 0;
    var last_focus = "";

    function PleaseWait(vis) {
        displayLoadingPanel((vis)); //A1514
        document.body.style.cursor = ((vis) ? "wait" : "default");
    }

    function sendAJAX(params) {
        is_ajax = 1;
        PleaseWait(1);
        $.ajax({
            type: "POST",
            data: "ajax=yes&" + params + "&r=" + Math.random(),
            dataType: "text",
            async: true,
            success: function (data) {
                receiveAJAX(data);
            },
            error: function () {
                PleaseWait(0);
                dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", true, ";", "undefined", "<% = JS_SafeString(ResString("titleError")) %>", 350, 280);
            }
        });
    }

    function receiveAJAX(data) {
        is_ajax = 0;
        if ((typeof data != "undefined")) {
        }
        PleaseWait(0);
        if ((on_ok_code != "")) eval(on_ok_code);
        on_ok_code = "";
    }

    function enc(sVal) {
        return encodeURIComponent(sVal);
    }

    function CloseWin()
    {
        window.close();
        return false;
    }

    function ShowList(vis) {
        if ((vis)) { $("#divList").hide(); $("#divListAll").show(); } else { $("#divList").show(); $("#divListAll").hide(); }
    }

    function SaveChanges() {
        on_ok_code = "if (last_focus!='') eval('theForm.' + last_focus + '.focus();');";
        sendAJAX("action=save&subj=" + enc(theForm.tbSubj.value) + "&body=" + enc(theForm.tbBody.value));
    }

    function onSend() {
        var s = theForm.tbSubj.value;
        var b = theForm.tbBody.value;
        if (b == "") {
            dxDialog("<% =JS_SafeString(ResString("msgEmptyBody"))%>", true, ";");
        } else {
            on_ok_code = "onShowReply(data);";
            sendAJAX("action=send&subj=" + enc(theForm.tbSubj.value) + "&body=" + enc(theForm.tbBody.value));
        }
    }

    function onShowReply(data) {
        if (data != "") dxDialog(data, false, ";");
    }

    function onShowPreview(data) {
        if (data != "") {
            jDialog_show_icon = false;
            dxDialog(data, false, ";", "", "<% =JS_SafeString(ResString("titlePreview"))%>");
            setTimeout("jDialog_show_icon=true;", 300);
        }
    }

    function onPreview() {
        on_ok_code = "onShowPreview(data);";
        sendAJAX("action=preview&subj=" + enc(theForm.tbSubj.value) + "&body=" + enc(theForm.tbBody.value));
    }

    function InsertTemplate(txt) {
        var t = $("#dxToolTipHints").dxTooltip("instance"); //A1514
        if ((t)) { if (t.option("visible")) t.hide(); else t.show(); }//A1514
        $("#" + (last_focus == "" ? "tbBody" : last_focus)).insertAtCaret(txt).focus();
        return false;
    }

    /* A1514 === */
    $(document).ready(function () {
        $("#dxToolTipHints").dxTooltip({
            target: '#lnkHints',
            showEvent: 'dxhoverstart',
            hideEvent: 'dxclick',
            hoverStateEnabled: true
        });
    }); /* A1514 == */

    setTimeout("window.focus(); if (theForm.tbSubj.value=='') theForm.tbSubj.focus(); else theForm.tbBody.focus();", 500);

</script>
<table border="0" class="whole" cellspacing="1" cellpadding="2">
<tr><td align="center" colspan="2" style="height:1em;"><h5 style="padding:0px; margin:0px;"><% = PageTitle(CurrentPageID) %></h5></td></tr>
<tr><td align="right" style="width:60px; height:1em;" class="text">From:</td><td><% =GetSenders()%></td></tr>
<tr><td align="right" style="height:1em;" valign="top" class="text">To:</td><td class="text"><% =GetRecepients()%></td></tr>
<tr><td align="right" style="height:1em;" class="text">Subject:</td><td><input type="text" id="tbSubj" maxlength="200" style="width:100%" value="<% =SafeFormString(CustomSubject)%>" onfocus="last_focus=this.id;" /></td></tr>
<tr><td align="right" valign="top" class="text">Body:</td><td><textarea id="tbBody" class="input textarea" maxlength="5000" style="width:100%; height:100%" onfocus="last_focus=this.id;" ><% =SafeFormString(CustomBody)%></textarea></td></tr>
<tr><td align="right" style="height:1em;" class="text small">&nbsp;</td><td class="text small gray">[&nbsp;<a href='' class="actions dashed" onclick="return false;" id="lnkHints"><% =ResString("lblEmailVariables")%></a>&nbsp;]</td></tr>
</table><div id="dxToolTipHints"><%=GetTooltipHints()%></div>
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="ContentPlaceHolderButtons">
    <div id="divButtons">
    <input type="button" id="btnPreview" value="<% = ResString("btnPreview")%>" class="button" onclick="onPreview(); return false;"/>
    <input type="button" id="btnSave"  value="<% = ResString("btnSave")%>"   class="button" onclick="SaveChanges(); return false;"/>
    <input type="button" id="btnSend"  value="<% = ResString("btnSend")%>"   class="button" onclick="onSend(); return false;"/>
    <input type="button" id="btnClose" value="<% = ResString("btnClose") %>" class="button" onclick="self.close(); return false;"/>
    </div>
</asp:Content>