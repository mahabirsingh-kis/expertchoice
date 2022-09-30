<%@ Page Language="VB" Inherits="UserDetailsPage" Codebehind="UserInfo.aspx.vb" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxCallback" TagPrefix="dxcb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">
<!--

    var element = "";

    function GetUserInfo() {
        element = "lblInfo";
        var e = theForm.<% =tbEmail.ClientID %>.value;
        if (e !== "") SendCallback("<% = _PARAM_EMAIL %>=" + encodeURI(e)+"&<% = _PARAM_ACTION %>=common");
    }

    function GetWorkgroups() {
        element = "lblWorkgroups";
        var e = theForm.<% =tbEmail.ClientID %>.value;
        if (e !== "") SendCallback("<% = _PARAM_EMAIL %>=" + encodeURI(e)+"&<% = _PARAM_ACTION %>=wkglist");
    }

    function SendCallback(cmd) {
        setTimeout("showLoadingPanel();", 30);
        ASPxCallbackServices.SendCallback(cmd);
    }

    function ParseService(msg) {
        hideLoadingPanel();
        if (element === "") element = "lblInfo";
        var lbl = document.getElementById(element);
        if ((lbl)) lbl.innerHTML = msg;
        element = "";
        theForm.<% =tbEmail.ClientID %>.focus();
    }

    function Expand(id)
    {
        var lbl = document.getElementById(id);
        if ((lbl)) lbl.style.display = 'block';
        var lbl = document.getElementById("div"+id);
        if ((lbl)) lbl.style.display = 'none';
        return false;
    }

    function Hotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if ((event))
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);	// D0073
            if ((code === 13) && (!event.altKey)) // Enter
             {
                GetUserInfo();
                return false;
             }   
        }
    }
    
    document.onkeydown = Hotkeys;

//-->
</script>
<h4><% = PageTitle(CurrentPageID) %></h4>
<table border="0">
    <tr><td valign="middle" align="center" class="text" style="background:#f0f0f0; border:1px solid #d0d0d0; padding:2em 2em 1em 2em">
        User E-mail: <asp:TextBox runat="server" ID="tbEmail" Width="15em"></asp:TextBox>&nbsp; <asp:Button runat="server" ID="btnSend" CssClass="button" Text="Get Info" Width="8em"  UseSubmitBehavior="true" OnClientClick="GetUserInfo(); return false;"/>
        <div id='lblInfo' style='margin-top:1em; text-align:left'></div>
    </td></tr>
</table>

<dxcb:ASPxCallback ID="ASPxCallbackServices" runat="server" ClientInstanceName="ASPxCallbackServices">
    <ClientSideEvents CallbackComplete="function(s, e) {
     ParseService(e.result);
}" />
</dxcb:ASPxCallback>
</asp:Content>
