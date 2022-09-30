<%@ Page Language="VB" Inherits="RiskRolesPage" title="Participant Roles" Codebehind="RiskRoles.aspx.vb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<script language="javascript" type="text/javascript">

    var cur_cmd = "";
    var cur_scroll;

    function DoEnable(e) {
        var tagNames = ["INPUT", "SELECT", "TEXTAREA"];
        for (var i = 0; i < tagNames.length; i++) {
            var elems = document.getElementsByTagName(tagNames[i]);
            for (var j = 0; j < elems.length; j++) {
                elems[j].disabled = !e;
            }
        }
    }

    function onShowRoles(value) {
        sendCommand("action=grid_filter&val=" + value);
    }

    function sendCommand(cmd) {
        cur_cmd = cmd;
        cur_scroll = $("#<% = GridControls.ClientID %>").offset();
        
        DoEnable(false);
        showLoadingPanel();
        var am = $find("<%=RadAjaxManagerMain.ClientID%>");
        if ((am)) { am.ajaxRequest(cmd); return true; }        
        return false;
    }

    function onRequestError(sender, e) {
        DoEnable(true);
        hideLoadingPanel();
        dxDialog("(!) Callback error.\nStatus code: " + e.Status + "\nResponse Text: " + e.ResponseText, null);                
    }

    function onResponseEnd(sender, argmnts) {
        DoEnable(true);
        var divAllowed = $get("<% =divAllAllowed.ClientID %>");
        var divDropped = $get("<% =divAllDropped.ClientID %>");
        var btnAllowed = $get("btnAllowAll");
        var btnDropped = $get("btnDropAll");
        if ((divAllowed) && (divDropped) && (btnAllowed) && (btnDropped)) {
            btnAllowed.disabled = (divAllowed.innerText == "1");
            btnDropped.disabled = (divDropped.innerText == "1");
        }
        
        onPageResize();
        if ((cur_scroll)) {
            $("#<% = divGrid.ClientID %>").scrollTop(-cur_scroll.top + 54);
            $("#<% = divGrid.ClientID %>").scrollLeft(-cur_scroll.left);
        }
        hideLoadingPanel();
    };

    function RowHover(r, hover, alt) {
        if ((r)) {
            var bg = (hover == 1 ? (alt ? "#eaf0f5" : "#f0f5ff") : (alt ? "#fafafa" : "#ffffff"));
            for (var i = 0; i < r.cells.length; i++) {
                r.cells[i].style.background = bg;
            }
        }
    }

    function removeExtraBorders() {
        $("TR.grid_row TD").css("border-bottom", "0px");
        $("TR.grid_row_alt TD").css("border-bottom", "0px");
    }

    $(document).ready(function () {
        removeExtraBorders();
    });

</script>
</telerik:RadScriptBlock>

<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server" ClientEvents-OnRequestError="onRequestError" ClientEvents-OnResponseEnd="onResponseEnd" EnableAJAX="true">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="divGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="divGrid" />
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<%-- Page content --%>

<table style='overflow:hidden;' border='0' cellspacing='0' cellpadding='0' class='whole'>
<tr style='height:1em'><td valign="top"><h4>Participant Roles</h4></td></tr>
<tr><td class="text">
  <nobr><input type="button" id="btnAllowAll" class="button" onclick="sendCommand('action=allow_all');" style='margin-bottom:2px; margin-left:1px;' value="Allow&nbsp;All" <% =IIF(IsAllAllowed(), " disabled", "") %> />
        <input type="button" id="btnDropAll"  class="button" onclick="sendCommand('action=drop_all');"  style='margin-bottom:2px;' value="Drop&nbsp;All" <% =IIF(IsAllAllowed(True), " disabled", "") %> />
        <span class="text" style="margin-left:25px;">Show:</span>       
        <input type='radio' class='radio' name='rbShowRoles' style='margin-left:5px;' id='cbShowAll' <%=IIf(GridFilter = -1, " checked ", "")%> value='-1' onclick='onShowRoles(this.value)' /><label for='cbShowAll'><%=ResString("lblAll") %></label>
        <input type='radio' class='radio' name='rbShowRoles' style='margin-left:5px;' id='cbShow1'   <%=IIf(GridFilter = 0, " checked ", "") %> value='0' onclick='onShowRoles(this.value)' /><label for='cbShow1'><%=ParseString("%%Controls%% for %%Sources%%")%></label>
        <input type='radio' class='radio' name='rbShowRoles' style='margin-left:5px;' id='cbShow2'   <%=IIf(GridFilter = 3, " checked ", "") %> value='3' onclick='onShowRoles(this.value)' /><label for='cbShow2'><%=ParseString("%%Controls%% for Vulnerabilities")%></label>
        <input type='radio' class='radio' name='rbShowRoles' style='margin-left:5px;' id='cbShow3'   <%=IIf(GridFilter = 4, " checked ", "") %> value='4' onclick='onShowRoles(this.value)' /><label for='cbShow3'><%=ParseString("%%Controls%% for Consequences")%></label>
    </nobr>
</td></tr>
<tr id='tdGrid' class='whole' valign='top'><td>
<telerik:RadCodeBlock ID="RadPaneGrid" runat="server">
<div id='divGrid' runat='server' style='overflow:auto;'><div id='divAllAllowed' runat="server" style='display:none'></div><div id='divAllDropped' runat="server" style='display:none'></div>
<asp:GridView AutoGenerateColumns="true" EnableViewState="false" AllowSorting="false" AllowPaging="false" ID="GridControls" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" TabIndex="1" Width="99%">
    <RowStyle VerticalAlign="Middle" CssClass="text grid_row"/>
    <HeaderStyle CssClass="text grid_header actions" />
    <AlternatingRowStyle CssClass="text grid_row_alt" />
    <FooterStyle VerticalAlign="Middle" CssClass="text grid_row" Font-Bold="true" />
    <EmptyDataTemplate><h6 style='margin:8em 2em'><nobr><% =GetEmptyMessage()%></nobr></h6></EmptyDataTemplate>
</asp:GridView>
</div></telerik:RadCodeBlock></td></tr>
</table>

<script language="javascript" type="text/javascript">

    function onPageResize() {
        $("#<% = divGrid.ClientID %>").height(100);
        $("#<% = divGrid.ClientID %>").width(100);
        $("#<% = divGrid.ClientID %>").height($("#tdGrid").height() - 2);
        $("#<% = divGrid.ClientID %>").width($("#tdGrid").width() - 2);
        removeExtraBorders();
    }

    resize_custom = onPageResize;

</script>
</asp:Content>

