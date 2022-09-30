<%@ Page Language="VB" Inherits="ProjectTeamTimeStatusPage" title="TeamTime Status" Codebehind="Status.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    var isReadOnly = <%= Bool2JS(Not isTeamTimeAvailable())%>;
    var isTTActive = <% =Bool2JS(isTeamTimeActive)%>;
    var isTTOwner = <% =Bool2JS(IsTeamTimeOwner)%>;
    
    var tt_wnd = null;
    var last_callback = new Date();
    var refresh_timeout = 5000;
  
    var _SET_ROLES_OBJECTIVES = "<% =JS_SafeString(PageURL(_PGID_STRUCTURE_PERMISSION_ALTS)) %>";
    var _SET_ROLES_ALTERNATIVES = "<% =JS_SafeString(PageURL(_PGID_STRUCTURE_PERMISSION_ALTS)) %>";

    var last_cb = null;

    function initButtons() {
        $("#btnStartMeeting").dxButton({
            disabled: true,
            icon: "fas fa-play",
            onClick: function (e) { sessionStart(); },
            text: "<% =JS_SafeString(ResString("btnTTStartSession"))%>",
            type: "success",
            useSubmitBehavior: false,
            visible: true
        });
        $("#btnStopMeeting").dxButton({
            disabled: true,
            icon: "fas fa-stop",
            onClick: function (e) { sessionStop(); },
            text: "<%=JS_SafeString(ResString("btnTTStopSession"))%>",
            type: "normal",
            useSubmitBehavior: false,
            visible: true
        });
        checkButtons(); 
    }

    function checkButtons() {
        $("#btnStopMeeting").dxButton("instance").option("disabled", !isTTActive || isReadOnly);
        $("#btnStartMeeting").dxButton("instance").option("disabled", isReadOnly || (isTTActive && !isTTOwner));
        $("#btnStartMeeting").dxButton("instance").option("text", (isTTActive ? "<% = JS_SafeString(ResString("btnTTContinueSession"))%>" : "<% = JS_SafeString(ResString("btnTTStartSession"))%>"));
        $("#btnStartMeeting").dxButton("instance").option("icon", (isTTActive ? "fas fa-step-forward" : "fas fa-play"));
    }

    function brdOptionsWarningClick() {
        document.location.href='<%=PageURL(_PGID_PROJECT_OPTION_EVALUATE) %><% =GetTempThemeURI(true) %>';
        return false;
    }

    function brdRolesWarningClick() {
        var tag = document.getElementById("<%=brdRolesWarningTag.ClientID%>")[text] + "";
        showLoadingPanel();
        setTimeout('document.location.href = ' + (tag==1 ? "_SET_ROLES_ALTERNATIVES" : "_SET_ROLES_OBJECTIVES") + ";", 200);
        return false;
    }

    function switchOption(chk) {
        last_cb = chk; 
        if (!isReadOnly) {
            callAjax("<% =_PARAM_ACTION %>=set_param&id=" + chk.value + "&chk=" + (chk.checked ? 1 : 0), onSetOption, _method_POST);
        }
    }

    function onSetOption(data) {
        var res = parseReply(data);
        if (res) {
            if (res.Result != _res_Success) {
                if ((last_cb)) last_cb.checked = !lst_cb.checked;
                DevExpress.ui.dialog.alert((res.Message == "" ? err : res.Message), "<% =JS_SafeString(ResString("msgCantPerformAction")) %>");
            }
        } else {
            if (err!="") DevExpress.ui.notify(err, "error"); 
        }
        last_callback = new Date();
    }

    function selectAllSettings(chk) {
        $("input:checkbox.setting_cb").prop('checked', chk*1 == 1);
        if (!isReadOnly) {
            callAjax("<% =_PARAM_ACTION %>=check_all&chk=" + chk, onCheckAll, _method_POST);
        }
    }

    function onCheckAll(data) {
        var res = parseReply(data);
        if (res) {
            if (res.Result != _res_Success) {
                var cbs = $("input:checkbox.setting_cb");
                cbs.prop('checked', !cbs.prop('checked'));
                DevExpress.ui.dialog.alert((res.Message == "" ? err : res.Message), "<% =JS_SafeString(ResString("msgCantPerformAction")) %>");
            }
        } else {
            if (err!="") DevExpress.ui.notify(err, "error"); 
        }
        last_callback = new Date();
    }

    function showTTWindow(params) {
        tt_wnd = CreatePopup("<% =JS_SafeString(PageURL(_PGID_TEAMTIME, "?temptheme=tt&passcode=" + HttpUtility.UrlEncode(App.ActiveProject.Passcode(App.ActiveProject.isImpact)))) %>" + params, "TeamTime", "");
        hideLoadingPanel();
        if ((tt_wnd)) setTimeout("tt_wnd.focus();", 2000);
    }

    function sessionStart() {
        if (!isReadOnly) {
            if (isTTActive) {
                setTimeout('showTTWindow("");', 300);
                setTimeout('if ((tt_wnd)) isTTActive = true; checkButtons();', 400);
            } else {
                callAjax("<% =_PARAM_ACTION %>=start_tt", onTTStart, _method_POST);
            }
        }
    }

    function onTTStart(data) {
        var res = parseReply(data);
        if (res) {
            if (res.Result == _res_Success) {
                isTTActive = true;
                isTTOwner = true;
                checkButtons();
                showLoadingPanel();
                setTimeout('showTTWindow("&step=1");', 300);
            } else {
                DevExpress.ui.dialog.alert((res.Message == "" ? err : res.Message), "<% =JS_SafeString(ResString("msgCantStartTT")) %>");
            }
        } else {
            if (err!="") DevExpress.ui.notify(err, "error"); 
        }
        last_callback = new Date();
    }

    function sessionStop() {
        if (!isReadOnly) {
            if (isTTActive) {
                callAjax("<% =_PARAM_ACTION %>=stop_tt", onTTStop, _method_POST);
            }
        }
    }

    function onTTStop(data) {
        var res = parseReply(data);
        if (res) {
            if (res.Result == _res_Success) {
                if (!isTTOwner) $("#<% =lblMessage.ClientID %>").hide();
                isTTActive = false;
                isTTOwner = false;
                if ((tt_wnd!=null)) { tt_wnd.close(); tt_wnd = null; }
                checkButtons();
            } else {
                DevExpress.ui.dialog.alert((res.Message == "" ? err : res.Message), "<% =JS_SafeString(ResString("msgCantStopTT")) %>");
            }
        } else {
            if (err!="") DevExpress.ui.notify(err, "error"); 
        }
        last_callback = new Date();
    }

    function checkStatus() {<% If isTeamTimeAvailable() Then%>
        var dt = new Date();
        if (dt-last_callback > refresh_timeout) {
            $("#spanStatus").show();
            callAjax("<% =_PARAM_ACTION %>=status", onGetStatus, _method_POST, true);
        }
        setTimeout('checkStatus();', refresh_timeout + 1000);
    <% End If%>}

    function onGetStatus(data) {
        var res = parseReply(data);
        if (res) {
            if (res.Result == _res_Success) {
                var dt = eval(res.Data);
                if (dt.length>3) {
                    isTTActive = (dt[0]);
                    isTTOwner = (dt[1]);
                    $("#cbDisplayViewOnlyUsers").prop("checked", (dt[2]));
                    $("#cbPollingMode").prop("checked", (dt[3]));
                    $("#cbAnonymous").prop("checked", (dt[4]));
                    $("#cbHidePM").prop("checked", (dt[5]));
                    $("#cbHideOfflineUsers").prop("checked", (dt[6]));
                    checkButtons();
                }
            }
        }
        $("#spanStatus").hide();
        last_callback = new Date();
    }

    $(document).ready(function () { 
        initButtons(); 
        setTimeout('checkStatus();', refresh_timeout);
    });
 
</script>

<%--<h5 style='padding:1ex'>Project status of &quot;<%=SafeFormString(ShortString(PRJ.ProjectName, 65, True))%>&quot;</h5>--%>

<h5 style='padding:1ex'><%=PageTitle(CurrentPageID)%></h5>
<div id="divWarnings" style="text-align:left; margin:16px 0px; max-width:445px;">
    <div id="brdOptionsWarning" class="warning" style="cursor:pointer;" runat="server" visible="false" onclick="brdOptionsWarningClick();">
        <span style="display:block;font-weight:bold;" class="error"><%=IIf(IsAlternativesEvaluateWarning, ResString("msgWarningOptionsAlternatives"), "")%></span>
        <span style="display:block;font-weight:bold;" class="error"><%=IIf(IsObjectivesEvaluateWarning, ResString("msgWarningOptionsObjectives"), "")%></span>            
        <span style="display:block;font-weight:bold; margin-top:1ex;"><%=ResString("msgWarningOptions")%></span>            
    </div>
    <div id="brdRolesWarningTag" style="display:none;" runat="server"></div>
    <div id="brdRolesWarning" class="warning" style="cursor:pointer;" runat="server" visible="false" onclick="brdRolesWarningClick();">
        <span style="display:block;font-weight:bold;"><%=ResString("msgWarningTTRoles")%></span>            
    </div>
</div>
<div id="divSettings" class="text" style="margin:10px 0px 12px 0px;">
    <fieldset class="legend" style="text-align:left; vertical-align:top; padding:15px 15px 0px 20px; width:400px;" id="pnlSettings">
        <legend class="text legend_title">&nbsp;Settings&nbsp;<span id="spanStatus" class="tiny" style="color:#9999ff; display:none;">&#149;</span></legend>
        <label><input type="checkbox" class="checkbox setting_cb" id="cbDisplayViewOnlyUsers" <%=IIf(isTeamTimeAvailable, "", " disabled='disabled' ")%> value="0" onclick="switchOption(this);" <%=IIf(App.ActiveProject.PipeParameters.TeamTimeDisplayUsersWithViewOnlyAccess, " checked='checked' ", "") %> /><%=ResString("lblTTDisplayViewOnlyUsers")%></label><br/>
        <label><input type="checkbox" class="checkbox setting_cb" id="cbPollingMode"          <%=IIf(isTeamTimeAvailable, "", " disabled='disabled' ")%> value="1" onclick="switchOption(this);" <%=IIf(App.ActiveProject.PipeParameters.SynchStartInPollingMode, " checked='checked' ", "") %> /><%=ResString("lblTTStartPollingMode")%></label><br/>
        <label><input type="checkbox" class="checkbox setting_cb" id="cbAnonymous"            <%=IIf(isTeamTimeAvailable, "", " disabled='disabled' ")%> value="2" onclick="switchOption(this);" <%=IIf(App.ActiveProject.PipeParameters.TeamTimeStartInAnonymousMode, " checked='checked' ", "") %> /><%=ResString("lblTTAnonymous")%></label><br/>
        <label><input type="checkbox" class="checkbox setting_cb" id="cbHidePM"               <%=IIf(isTeamTimeAvailable, "", " disabled='disabled' ")%> value="3" onclick="switchOption(this);" <%=IIf(App.ActiveProject.PipeParameters.TeamTimeHideProjectOwner, " checked='checked' ", "") %> /><%=ResString("lblTTHidePM")%></label><br/>
        <label><input type="checkbox" class="checkbox setting_cb" id="cbHideOfflineUsers"     <%=IIf(isTeamTimeAvailable, "", " disabled='disabled' ")%> value="4" onclick="switchOption(this);" <%=IIf(PM.Parameters.TTHideOfflineUsers, " checked='checked' ", "") %> /><%=ResString("lblTTHideOfflineUsers")%></label><br/>
        <% If isTeamTimeAvailable() Then%><div id="pnlAllOrNoneSelector" class="small gray" style='text-align:right; margin-top:1ex;'>
            <a href="" onclick="selectAllSettings(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
            <a href="" onclick="selectAllSettings(0); return false;" class="actions"><% =ResString("lblNone")%></a>
        </div><% End If%>
        <br/>
    </fieldset>
</div>
<span runat="server" class="warning" id="lblMessage" visible="false"></span>    
<div id="divActions" style="margin:8px;">
    <div id="btnStartMeeting" style="margin:10px; width:170px; padding:4px;"></div>
    <div id="btnStopMeeting" style="margin:10px; width:170px; padding:4px;"></div>
</div>

</asp:Content>