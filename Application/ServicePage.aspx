<%@ Page Language="VB" Inherits="ServicePage" MasterPageFile="~/mpEmpty.master" Codebehind="ServicePage.aspx.vb" %>
<asp:Content ID="Content2" ContentPlaceHolderID="PageContent" runat="Server">
<table border="0" height="100%" width="100%" class="whole">
    <tr><td valign="middle" align="center" class="text" runat="server" id="tdMessage"></td></tr>
</table>
<script language="javascript" type="text/javascript">

    function Redirect(url)
    {
        if ((window.opener))
        {
            window.opener.location.href = url;
        }
        else
        {
            if ((window.parent) && (window.parent.location))
            {
                window.parent.location.href = url;
            }
            else 
            {
                document.location.href = url;
            }
        }
    }
<% if CheckVar(_PARAM_ACTION, "")="upload" Then %>
  var o = window.opener;
  if ((o) && ((typeof window.opener.onUploadFile)!="undefined")) o.onUploadFile('<% =CheckVar(_PARAM_ID, "-1") %>','<% =CheckVar("risk", "") %>');
  setTimeout("window.close();", 250);
<% End If %>
<% If CheckVar(_PARAM_ACTION, "") = "uploadsurvey" Then%>
    var o = window.opener;
    if ((o) && ((typeof window.opener.onUploadSurvey)!="undefined")) o.onUploadSurvey("<% = JS_SafeString(CheckVar(_PARAM_ID, "")) %>");
    setTimeout("window.close();", 250);
<% End If %><% If CheckVar(_PARAM_ACTION, "").tolower = "intensities" Then %>
    var called = 0;
    if ((window.opener) && ((typeof window.opener.onEndEvaluateIntensities)!="undefined")) { window.opener.onEndEvaluateIntensities(true); called = 1; do_close = 1; setTimeout("closeWin();", <% =CheckVar("pause", 250) %>); }
    if (!(called) && (window.parent) && ((typeof window.parent.onEndEvaluateIntensities)!="undefined")) { window.parent.onEndEvaluateIntensities(true); }
<% End If %>
<% if CheckVar("close", false) Then %>
    var do_close = 1;    
<% If CheckVar("teamtime", False) %>
    if ((window.opener) && (typeof window.opener.onEndTeamTime)!="undefined") 
    { 
        window.opener.focus();
        window.opener.onEndTeamTime();
    } 
    else 
    { 
        if ((window.parent) && (typeof window.parent.onEndTeamTime)!="undefined") 
        { 
            window.parent.focus();
            window.parent.onEndTeamTime();
        }     
//        document.location.href='<% =PageURL(_PGID_SILVERLIGHT_UI) %>#/Workflow/StartMeeting/30502'; 
    }    
    if ((window.opener))
    {
        setTimeout('window.close();', 10); 
    }
    else 
    {
        if ((window.parent)) { do_close=0; window.parent.document.location.href='<% =PageURL(_PGID_SILVERLIGHT_UI) %><% If Not CanEditActiveProject OrElse App.ActiveProject.MeetingOwnerID<>App.ActiveUser.UserID Then %>#/Overview/ProjectsList/20002<% Else %>#/Workflow/StartMeeting/30502<% End if %>'; }
    }    
<% else %>    
    if ((do_close)) {
        $("#<% = tdMessage.ClientID %>").html("<h6><% = JS_SafeString(ResString("msgCloseBrowser")) %></h6>");
        setTimeout("closeWin();", <% =CheckVar("pause", 250) %>);
    }
<% End If %>
<% End If %>

    function closeWin() {
        if ((window.opener) && (typeof window.opener.onCloseWin)!="undefined") { window.opener.onCloseWin(); return true; }
        if ((window.parent) && (typeof window.parent.onCloseWin)!="undefined") { window.parent.onCloseWin(); return true; }
        window.open('', '_self', '');
        window.close();
    }
    
    function AskSLReload()
    {
        <% if Not App.isAuthorized Then %>setTimeout('window.close();', 500);<% End if %>
        try {
            if ((window.opener) && (typeof window.opener.Ask4ReloadPage)!="undefined") { window.opener.Ask4ReloadPage(); return true; } 
            if ((window.parent) && (typeof window.parent.Ask4ReloadPage)!="undefined") { window.parent.Ask4ReloadPage(); return true; } 
//            if ((window.opener) && (typeof window.opener.LogOutOnInactivityTimeout)!="undefined") { window.opener.LogOutOnInactivityTimeout(); return true; } 
//            if ((window.parent) && (typeof window.parent.LogOutOnInactivityTimeout)!="undefined") { window.parent.LogOutOnInactivityTimeout(); return true; } 
        }
        catch (e) {}
        if (!(window.parent)) document.location.href='<% =PageURL(_PGID_LOGOUT) %>';
        return false;
    }

    function CheckExitEvent()
    {
        return true;
    }

    function RefreshPage()
    {
        if ((window.opener) && (typeof window.opener.reloadPage())!="undefined") return window.opener.reloadPage();
        if ((window.parent) && (typeof window.parent.reloadPage())!="undefined") return window.parent.reloadPage();
        return false;
    }

    function CustomerSupport()
    {
        if ((window.opener) && (typeof window.opener.CustomerSupport)!="undefined") return window.opener.CustomerSupport();
        if ((window.parent) && (typeof window.parent.CustomerSupport)!="undefined") return window.parent.CustomerSupport();
        return false;
    }

    function tryLogin() {
        var url = "<% =JS_SafeString(PageURL(_PGID_START))%>";
        var w = null;
        if ((window.opener)) w = window.opener.document;
        if ((window.parent)) w = window.parent.document;
        if (w!=null && w!=document) {
            w.location.href = url;
            setTimeout('window.close();', 150);
        } else {
            document.location.href = url;
        }
        return false;
    }

//    function onError403()
//    {
//        if ((window.opener) && (typeof window.opener.LogOutOnInactivityTimeout)!="undefined") { window.opener.LogOutOnInactivityTimeout(); setTimeout("window.close();", 200); return true; }
//        if ((window.parent) && (typeof window.parent.LogOutOnInactivityTimeout)!="undefined") { window.parent.LogOutOnInactivityTimeout(); setTimeout("window.close();", 200); return true; }
//        document.location.href='<% =PageURL(_PGID_LOGOUT) %>';
//        return false;
//    }

    function StartEvalImpact()
    {
        if ((window.opener) && (typeof window.opener.EvalImpact)!="undefined") return window.opener.EvalImpact();
        if ((window.parent) && (typeof window.parent.EvalImpact)!="undefined") return window.parent.EvalImpact();
        return false;
    }

    function StartEvalLikelihood()
    {
        if ((window.opener) && (typeof window.opener.EvalLikelihood)!="undefined") return window.opener.EvalLikelihood();
        if ((window.parent) && (typeof window.parent.EvalLikelihood)!="undefined") return window.parent.EvalLikelihood();
        return false;
    }

    function RiskResults()
    {
        navOpenPage(70016, "");
    }

    function openChat() {
        var w = (window.opener || window.parent);
        if ((w) && (typeof(w.callChat) == "function")) w.callChat();
    }

</script>
</asp:Content>
