<%@ Page Language="VB" Inherits="SilverlightCustomReport" title="Advanced/Custom" Codebehind="SilverlightReport.aspx.vb" %>

<%@ Register Assembly="DevExpress.Web.ASPxGridView.v9.1.Export, Version=9.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.ASPxGridView.Export" TagPrefix="dxwgv" %>

<%@ Register Assembly="DevExpress.Web.ASPxGridView.v9.1" Namespace="DevExpress.Web.ASPxGridView" TagPrefix="dxwgv" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxClasses" tagprefix="dxw" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
    
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
        <dxwgv:ASPxGridViewExporter ID="ASPxGridViewExporter" runat="server" GridViewID="ASPxGridView0">
        </dxwgv:ASPxGridViewExporter>
       <asp:Panel runat="server" Height="30" ID="Panel" Width="90%" HorizontalAlign="Left">
       <telerik:RadToolbar ID="tbExport" runat="server" AutoPostback="true">
            <Items>
                <telerik:RadToolBarButton ID="btnGeneratePrioritesOverview" runat="server"
                    ButtonText="Generate report" CommandName="Generate" DisplayType="TextImage" Text="Generate report"/>
                <telerik:RadToolBarButton Text="|" Enabled="false" />
                <telerik:RadToolbarButton ID="btnRTF" runat="server" 
                    ButtonText="Export to RTF" CommandName="ExporttoRTF" DisplayType="TextImage" Text="RTF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnPDF" runat="server" 
                    ButtonText="Export to PDF" CommandName="ExporttoPDF" DisplayType="TextImage" Text="PDF" Enabled="false" />
                <telerik:RadToolbarButton ID="btnXLS" runat="server" 
                    ButtonText="Export to XLS" CommandName="ExporttoXLS" DisplayType="TextImage" Text="XLS" Enabled="false" />
            </Items>
        </telerik:RadToolbar>
        </asp:Panel>
        <dxwgv:ASPxGridView runat="server" ID="ASPxGridView" Visible="false" AutoGenerateColumns="true">
            <Settings ShowFilterRow="True" ShowFilterBar="Visible" ShowGroupPanel="True" ShowGroupedColumns="True"></Settings>
        </dxwgv:ASPxGridView>
</asp:Content>

