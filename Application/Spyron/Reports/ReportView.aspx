<%@ Page Language="VB" Inherits="ReportView" title="View Logs" Codebehind="ReportView.aspx.vb" %>
<%@ Register Assembly="System.Web" Namespace="System.Web.UI" TagPrefix="cc1" %>
<%@ Register Src="~/ctrlReport.ascx" TagName="ctrlReport" TagPrefix="uc1" %>
<%@ Register Assembly="DevExpress.Web.v9.1"  Namespace="DevExpress.Web.ASPxTabControl" TagPrefix="dxtc" %>
<%@ Register Assembly="DevExpress.Web.v9.1"  Namespace="DevExpress.Web.ASPxClasses" tagprefix="dxw" %>
<%--<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>--%>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript" >

    function onLoadReport(s, e) {
        <%--SwitchWarning("<% =pnlLoadingNextStep.ClientID %>", 0);--%>
        if ((e)) {
            $("#divPages").html((e.PageCount>0) ? "Page " + (e.PageIndex + 1) + " of " + e.PageCount : "");
        }
        showReportMode();
    }

    function showReportMode() {
        if ((viewer)) $("#divMode").html("<input type='button' class='button btn_small' onclick='switchReportMode(); return false;' style='width:10em;' value='" + (viewer.pageByPage ? "Show all" : "Show by pages") + "'>");
        onResize();
    }

    function switchReportMode() {
<%--        if ((viewer)) {
            if (viewer.pageCount > 5) {
                dxDialog("<% = JS_SafeString(ResString("confReportShowAllPages"))%>", false, "onSwitchReportMode();", ";", "<% = JS_SafeString(ResString("titleConfirmation")) %>", "undefined", "undefined", "<% = JS_SafeString(ResString("btnYes")) %>", "<% = JS_SafeString(ResString("btnNo")) %>");
            } else {
                onSwitchReportMode();
            }
        }--%>
        onSwitchReportMode();
    }

    function onSwitchReportMode() {
        if ((viewer)) {
            viewer.pageByPage = !viewer.pageByPage;
            document.cookie = "rv=" + ((viewer.pageByPage) ? 1 : 0);
            //viewer.Refresh();
            document.location.href=document.location.href;
        }
    }

    function onResize() {
        $("#tblMainTable").parent().css("height", "97%");
        $("#divReportContent").height(200);
        var h = Math.round($("#tdReportContent").height()-8);
        $("#divReportContent").height(h);
    }

    resize_custom = onResize;
    window.onload = setTimeout("showReportMode();", 2000);

</script>
<%--<div style="height:98%; text-align:center; overflow-y:auto;">--%>
    <%--<asp:CheckBox Text="Display full objective path" ID="cbFullPath" runat="server" AutoPostBack="true" Checked="true"/>--%>
    <dxtc:ASPxPageControl ID="ASPxPageControlReports" runat="server" TabPosition="Left" ActiveTabIndex="0" ContentStyle-HorizontalAlign="Center" Width="100%" Height="99%" AutoPostBack="true" TabStyle-Height="98%" ActiveTabStyle-Height="98%">
        <TabPages>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <uc1:ctrlReport ID="CtrlReport1" runat="server"/>
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <asp:CheckBox runat="server" ID="cbFullPath2" AutoPostBack="true" CssClass="text"/>
                <asp:CheckBox runat="server" ID="cbDescription2" AutoPostBack="true" CssClass="text"/>
                <uc1:ctrlReport ID="CtrlReport2" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl ID="ContentControl2" runat="server">
                <asp:CheckBox runat="server" ID="CheckBox2"  AutoPostBack="true" CssClass="text"/>
                <uc1:ctrlReport ID="CtrlReport8" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <asp:CheckBox runat="server" ID="cbFullPath3"  AutoPostBack="true" CssClass="text"/>
                <uc1:ctrlReport ID="CtrlReport3" runat="server" EnableTheming="true" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <uc1:ctrlReport ID="CtrlReport4" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <uc1:ctrlReport ID="CtrlReport10" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                    <asp:CheckBox runat="server" ID="cbFullPath5" AutoPostBack="true" CssClass="text"/>
                    <asp:CheckBox runat="server" ID="cbBars1" AutoPostBack="true" CssClass="text"/>
                <uc1:ctrlReport ID="CtrlReport5" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl ID="ContentControl1" runat="server">
                <asp:CheckBox runat="server" ID="cbBars2" AutoPostBack="true" CssClass="text"/>
                <uc1:ctrlReport ID="CtrlReport6" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <asp:CheckBox runat="server" ID="cbFullPath6" AutoPostBack="true" CssClass="text"/>
                <asp:CheckBox runat="server" ID="cbLocal6" AutoPostBack="true" CssClass="text"/>
                <asp:CheckBox runat="server" ID="cbGlobal6" AutoPostBack="true" CssClass="text"/>
                <uc1:ctrlReport ID="CtrlReport7" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
            <dxtc:TabPage><ContentCollection><dxw:ContentControl runat="server">
                <uc1:ctrlReport ID="CtrlReport9" runat="server" />
            </dxw:ContentControl></ContentCollection></dxtc:TabPage>
    </TabPages>
    </dxtc:ASPxPageControl>
<%--</div>--%>
<%--<EC:LoadingPanel id="pnlLoadingNextStep" runat="server" isWarning="true" WarningShowOnLoad="true" WarningShowCloseButton="false" Width="230" Visible="true"/>--%>
</asp:Content>
