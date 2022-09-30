<%@ Page Language="VB" Inherits="RiskTreatmentsReportPage" title="Risk Treaments Report" Codebehind="RiskTreatmentsReport.aspx.vb" %>
<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">

<script language="javascript" type="text/javascript">

    $(document).ready(function () {
        // fix for the report toolbar in IE
        var r = $("#<%=ReportViewerMain.ClientID %>" + "_ctl05");
        r.children().children(":first").css("float", "left");
        onResize();
    });

    function onResize() {
        // fix for the report height in Firefox
        var td = $("#tdReport");
        if ((td) && (td.height() > 0)) {
            $("#<%=ReportViewerMain.ClientID %>").height(100);
            $("#<%=ReportViewerMain.ClientID %>").height(td.height());
        }
    }

    resize_custom = onResize;

</script>

<div class='overlay' style='background-color:transparent; z-index:998; position:absolute; left:0; top:0; width:100%; height:100%; display:none;'>
    <EC:LoadingPanel id="pnlLoadingPanel" runat="server" isWarning="true" WarningShowOnLoad="false" WarningShowCloseButton="false" Width="230" Visible="true"/>
</div>
    
<table border="0" cellspacing="0" cellpadding="0" class="whole" style="width:720px;">
<tr>
    <td style="height:1em; text-align:center; width:680px;" class="text">
        <h5 style='padding:1ex'><% =GetPageTitle%></h5>
    </td>
    <td style="height:1em; width:40px; text-align:right;" class="text">
        <asp:button runat="server" ID="btnPrint" Text="Print" />
    </td>
</tr>
<tr>
    <td align="center" id='tdReport' class="whole" colspan="2">
        <rsweb:ReportViewer ID="ReportViewerMain" runat="server" BorderWidth="1" BorderColor="LightGray" Width="720" Height="100%" ShowToolBar="true" ShowPrintButton="true" ShowRefreshButton="false" ShowZoomControl="true" ShowBackButton="false" ShowFindControls="false" ShowPageNavigationControls="true" ExportContentDisposition="AlwaysAttachment" ShowWaitControlCancelLink="false" />
    </td>
</tr>
</table>

</asp:Content>
