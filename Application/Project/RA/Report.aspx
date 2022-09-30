<%@ Page Language="VB" Inherits="RAReportPage" title="RA Report" Codebehind="Report.aspx.vb" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<script language="javascript" type="text/javascript">
    
    function onSetScenario(id) {
        displayLoadingPanel(true);
        document.location.href = '<% =PageURL(CurrentPageID) %>&set=scenario&sid=' + id;
        return false;
    }

    function onSetViewMode(value) {
        displayLoadingPanel(true);
        document.location.href = '<% =PageURL(CurrentPageID) %>&set=viewmode&vm=' + value;
        return false;
    }

    function onSetReportMode(value) {
        displayLoadingPanel(true);
        document.location.href = '<% =PageURL(CurrentPageID) %>&set=reportmode&rm=' + value;
        return false;
    }

    function cbDecimalsChange(value) {
        displayLoadingPanel(true);
        document.location.href = '<% =PageURL(CurrentPageID) %>&set=decimals&dcm=' + value;
        return false;
    }
    
</script>

<table border="0" cellspacing="0" cellpadding="0" class="whole">
<tr style='height:2em'>
    <td valign="top">
        <div id='divToolbar' class='text ec_toolbar'>
            <table border="0" cellpadding="0" cellspacing="0" class="text">
                <tr>
                    <td class="ec_toolbar_td_separator" valign="bottom">
                        <span class='toolbar-label'><%=ResString("lblScenario") + ":"%></span>
                        <% =GetScenarios%>
                    </td>
                    <%If ActiveReportID = RAReportIDs.rpModelSpecification Then%>
                    <td class="ec_toolbar_td_separator">
                        <span class='toolbar-label small' style="text-align:center;">&nbsp;Custom&nbsp;Constraints&nbsp;/&nbsp;Funding&nbsp;Pools</span><br/>
                        <span class='toolbar-label'>&nbsp;Show:&nbsp;</span>                    
                        <select id="cbViewMode" style="width:140px; margin-top:3px" onchange="onSetViewMode(this.value);" <%=CStr(IIf(HasConstraintsAndFundingPools(), "", " disabled='disabled' ")) %>>
                            <option <%=IIf(ViewMode = ViewModes.vmDetails, "selected='selected'", "")%> value='<%=CInt(ViewModes.vmDetails)%>'>Details</option>
                            <option <%=IIf(ViewMode = ViewModes.vmSummary, "selected='selected'", "")%> value='<%=CInt(ViewModes.vmSummary)%>'>Summary</option>
                        </select>                    
                        <span class='toolbar-label'>&nbsp;Mode:&nbsp;</span>                    
                        <select id="cbReportMode" style="width:140px; margin-top:3px" onchange="onSetReportMode(this.value);"<%=CStr(IIf(HasConstraintsAndFundingPools(), "", " disabled='disabled' ")) %>>
                            <option <%=IIf(ReportMode = ReportModes.rmAllConstraints, "selected='selected'", "")%>    value='<%=CInt(ReportModes.rmAllConstraints)%>'>All</option>
                            <option <%=IIf(ReportMode = ReportModes.rmActiveConstraints, "selected='selected'", "")%> value='<%=CInt(ReportModes.rmActiveConstraints)%>'>Active (Not ignored and having min and/or max)</option>
                        </select>
                    </td>
                    <%End If%>
                    <td class="ec_toolbar_td_separator" valign="bottom">
                        <span class='toolbar-label'>&nbsp;<%=ResString("lblDecimals") + ":"%>&nbsp;</span>
                        <select class='select' name='cbDecimals' style='width:7ex; margin-right:3px; margin-top:3px;' onchange='cbDecimalsChange(this.value);'>
                              <option value='0'<% = If(DecimalDigits = 0, " selected", "") %>>0</option>
                              <option value='1'<% = If(DecimalDigits = 1, " selected", "") %>>1</option>
                              <option value='2'<% = If(DecimalDigits = 2, " selected", "") %>>2</option>              
                              <option value='3'<% = If(DecimalDigits = 3, " selected", "") %>>3</option>
                              <option value='4'<% = If(DecimalDigits = 4, " selected", "") %>>4</option>
                              <option value='5'<% = If(DecimalDigits = 5, " selected", "") %>>5</option>
                        </select>  
                    </td>
                    <%--<td class="ec_toolbar_td_separator">--%>
                        <%--<telerik:RadToolBarButton runat="server" EnableViewState="false" ForeColor="DarkBlue" Enabled="true" Text="Print" CheckOnClick="false" Value="btnPrint" />--%>
                        <%--<asp:Button runat="server" id="btnPrint" type="button" class="button_small" Value="Print" Width="80" Height="20" Text="Print" />--%>
                    <%--</td>--%>
                </tr>
            </table>
        </div>
    </td>
</tr>
<tr>
    <td style="height:1em; text-align:center;" class="text">
        <%--<h4><% =GetPageTitle%></h4>--%>
        <h5 style='padding:1ex'><% =GetPageTitle%>&nbsp;<%=ResString("lblForScenario")%>&nbsp;&quot;<%=ShortString(SafeFormString(RA.Scenarios.ActiveScenario.Name), 45, True)%>&quot;</h5>
        <%--<% =GetReportLinks()%>--%>
    </td>
</tr>
<tr>
    <td align="center" id='tdReport' style='height:100%; padding: 0px 6px;' valign="top">
        <rsweb:ReportViewer ID="ReportViewerMain" runat="server" BorderWidth="1" Width="720" Height="100%" BorderColor="LightGray" ShowToolBar="True" ShowPrintButton="True" ShowRefreshButton="False" ShowZoomControl="True" ShowBackButton="False" ShowFindControls="False" ShowPageNavigationControls="True" ExportContentDisposition="AlwaysAttachment" ShowWaitControlCancelLink="false" AsyncRendering="False" SizeToReportContent="True" />
    </td>    
</tr>
</table>

</asp:Content>
