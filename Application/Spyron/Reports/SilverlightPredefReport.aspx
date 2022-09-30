<%@ Page Language="VB" Inherits="SilverlightPredefReport" title="Report" Codebehind="SilverlightPredefReport.aspx.vb" %>

<%@ Register Src="~/ctrlReport.ascx" TagName="ctrlReport" TagPrefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <asp:Panel runat="server" Width="10%" HorizontalAlign="Center" >
        <uc1:ctrlReport ID="CtrlReport1" runat="server" />
    </asp:Panel>
</asp:Content>

