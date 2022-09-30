<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlReport" Codebehind="ctrlReport.ascx.vb" %>
<%@ Register Assembly="DevExpress.XtraReports.v9.1.Web"  Namespace="DevExpress.XtraReports.Web" TagPrefix="dxwc" %>
<table class="whole" border='0' cellpadding='0' cellspacing='0' id="tblMainTable"><tr><td align="center"><table style="height:100%" border='0' cellpadding='0' cellspacing='0'><tr style="height:24px;"><td align="center">
    <div style='border: 1px outset #cccccc; text-align:center; background: #f0f0f0;'>
        <table border='0' cellpadding='0' cellspacing='0' width="100%">
            <tr>
                <td>
                    <dxwc:ReportToolbar ID="ReportToolbarMain" runat="Server" ReportViewer="<%# ReportViewerMain %>" ShowDefaultButtons="False" Border-BorderWidth="0">
                        <Items>
<%--                            <dxwc:ReportToolbarButton ToolTip="Print the report" ItemKind="PrintReport" ImageUrl="BtnPrint.gif" />
                            <dxwc:ReportToolbarButton ToolTip="Print the current page" ItemKind="PrintPage" ImageUrl="BtnPrintPage.gif" />
                            <dxwc:ReportToolbarSeparator />--%>
                            <dxwc:ReportToolbarButton Enabled="False" ToolTip="First Page" ItemKind="FirstPage" ImageUrl="BtnFirstPage.gif" />
                            <dxwc:ReportToolbarButton Enabled="False" ToolTip="Previous Page" ItemKind="PreviousPage" ImageUrl="BtnPrevPage.gif" />
                            <dxwc:ReportToolbarLabel Text="<span id='divPages' class='text small' style='cursor:default;'></span>"/>
                            <dxwc:ReportToolbarButton ToolTip="Next Page" ItemKind="NextPage" ImageUrl="BtnNextPage.gif" />
                            <dxwc:ReportToolbarButton ToolTip="Last Page" ItemKind="LastPage" ImageUrl="BtnLastPage.gif" />
                            <dxwc:ReportToolbarSeparator />
                            <dxwc:ReportToolbarLabel Text="<span id='divMode' class='text' style='padding-left:5px;'></span>"/>
                        </Items>
                    </dxwc:ReportToolbar>
                </td><% If ExportEnabled Then%>
                <td style='background: #f0f0f0; padding: 0px 10px 0px 16px;' align="right" class='text'><nobr><b>Export</b>:
<span style='color:#999'> 
    <a href='' onclick='viewer.SaveToDisk("pdf"); return false;' class='actions'>PDF</a> |
    <a href='' onclick='viewer.SaveToDisk("xls"); return false;' class='actions'>XLS</a> |
    <a href='' onclick='viewer.SaveToDisk("rtf"); return false;' class='actions'>RTF</a> |
    <a href='' onclick='viewer.SaveToDisk("mht"); return false;' class='actions'>MHT</a> |
    <a href='' onclick='viewer.SaveToDisk("txt"); return false;' class='actions'>Text</a> |
    <a href ='' onclick='viewer.SaveToDisk("png"); return false;' class='actions'>PNG</a>
</span></nobr></td><% End If%>
            </tr>
        </table>
    </div></td></tr>
    <tr id="tdReportContent"><td align="center" valign="top" style="padding-top:6px;"><div class="whole" style="border:1px solid #cccccc; overflow-y:auto; overflow-x:hidden" id="divReportContent">
        <dxwc:ReportViewer ID="ReportViewerMain" runat="server" Width="735" Border-BorderWidth="0" Border-BorderColor="LightGray" BorderBottom-BorderStyle="outset" ClientInstanceName="viewer" ClientSideEvents-PageLoad="onLoadReport" PrintUsingAdobePlugIn="false"/>
    </div></td></tr>
</table></td></tr></table>