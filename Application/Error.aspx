<%@ Page Language="VB" Inherits="Error500Page" title="Application Error" Strict="true" Codebehind="Error.aspx.vb" %>
<asp:Content ID="ContentMain" ContentPlaceHolderID="PageContent" Runat="Server"><!-- Error 500 -->
<script  type="text/javascript">

    function onResize(f, w, h) {
        var d = $("#divDetails");
        if ((d) && (d.length) && (d.css("overflow") == "auto")) {
            var h = dlgMaxHeight(1200);
            if (h<500) h = 500;
            var c = $(".code");
            h -= ((c) && c.height()>30 ? c.height(): 50);
            d.css({"maxHeight": Math.round(h-380) + "px"});
        }
    }

    function ShowDetails()
    {
        var details = $("#<% =FeedbackExtra.ClientID %>");
        var lnk = document.getElementById("<% =lblPostText.ClientID %>");
        if ((details) && (lnk)) {
            $("#divDetails").css("overflow", "auto");
            lnk.innerHTML = details.val();
            onResize();
        }
        return false;
    }

    function onSendFeedback(data) {
        hideLoadingPanel();
        $("#divPushFeedback").hide();
        if ($("#divDetails").length && (data)) {
            $("#divDetails").css("overflow", "").height("").html("<p></p>" + data + "<p></p>");
            $("#<% =lblSourceCode.ClientID %>").hide();
            //$("#divRteInfo").hide();
            if ($("#btnFeedback").length) $("#btnFeedback").dxButton("instance").option({
                "disabled": true,
                "icon": "fa fa-check"
            });
        } else {
            if ((data)) DevExpress.ui.dialog.alert(data, "Information");
        }
        onResize();
    }

    $(document).ready(function () {
        $("#btnBack").dxButton({
            text: "<% = JS_SafeString(ResString("btnContinue")) %>",
            type: "normal",
            icon: "arrowright",
            width: 130,
            height: 28,
            onClick: function (e) { <% =JSGetBackURL(ReturnURL) %> }
        });

        if ($("#btnFeedback").length) {
            $("#btnFeedback").dxButton({
                text: "<% = JS_SafeString(ResString("btnFeedback")) %>",
                type: "success",
                icon: "globe",
                width: 130,
                height: 28,
                visible: <% =Bool2JS(CanSubmit) %>,
                onClick: function (e) {
                    $("#btnFeedback").dxButton("instance").option("disabled", true);
                    var w = (window.opener || window.parent || window);
                    if (typeof w.callAjax != "function") w = window;
                    w.callAjax("submit=true", onSendFeedback, _method_POST, false, "<% =PageURL(_PGID_ERROR_500) %>");
                }
            }).focus();
            <% if CanSubmit Then %>for(i=0;i<5;i++) {
                $("#btnFeedback").fadeTo('slow', 0.5).fadeTo('slow', 1.0);
            }<% End If %>
        }
        var c = $(".code");
        if ((c) && (c.length)) {
            var h = dlgMaxHeight(1200) - 450;
            if (h<150) h = 150;
            c.css({"maxHeight": h + "px"});
        }
    });

    resize_custom = onResize;

</script>
<div style="text-align:center; min-width:50%; max-width:80em; margin:1em;">
<h2 class="error" style="margin-left:auto; margin-right:auto;"><% = ResString("msgApplicationError") %></h2>
<div style="text-align:center;">
    <div style="border:1px solid #ff9999; background:#fffaf0; padding:1ex; text-align:left; margin:1ex auto 1em auto; width:100%;" >
        <h5 class="error" style="margin:1em" id="divRteInfo"><asp:Label ID="lblError" runat="server" /></h5>
        <asp:Label ID="lblSourceCode" runat="server" CssClass="text"/>
        <div id="divDetails"><blockquote><asp:Label ID="lblPostText" runat="server" CssClass="text small"/></blockquote></div>
    </div>
    <div style="margin-bottom:1ex"><div id="btnFeedback"></div>&nbsp;&nbsp;<div id="btnBack"></div></div>
    <% If CanSubmit Then %><div style="text-align:center; margin:1em 0px" id="divPushFeedback"><div class="" style="font-size:0.75rem; margin:0px auto; text-align:left; max-width:42em;"><% =ResString("msgRTESubmitInfo") %></div></div><% End If %>
    <asp:HiddenField runat="server" ID="FeedbackExtra"/><input type="hidden" name="submit" value="" />
</div>
</div>
</asp:Content>