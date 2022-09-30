<%@ Control Language="VB" AutoEventWireup="false" Inherits="Spyron_Surveys_ctrlSurveyPageView" Strict="false" Codebehind="ctrlSurveyPageView.ascx.vb" %>
<%@ Register TagPrefix="Spyron" TagName="SurveyQuestion" Src="ctrlSurveyQuestion.ascx" %>

<script type="text/javascript">
    <!--
    function ConfirmDelete()
    {
        return (confirm('Are you sure want to delete this question?'));
    }
    function SetSelection(t, cls) {
        $('.' + cls).not(t).prop('checked', t.checked);
    }
    var s_manual = false;
    // -->
</script>
    
<asp:UpdatePanel ID="UpPanel" runat="server" UpdateMode="Conditional">
<ContentTemplate>
    <%--<asp:Panel ID="Panel2" runat="server" HorizontalAlign="Left" Width="100%" Height="100%" BackColor="#EFF3FB" BorderColor="#B5C7DE" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em">--%>
    <asp:Panel ID="PanelSurveyMain" runat="server" HorizontalAlign="Left" Width="100%">
    <asp:Panel ID="pnlSurveyHeader" runat="server" HorizontalAlign="Left">
       
        <asp:Label runat="server" ID="lblPageTitle" Width="100%" CssClass="h4"></asp:Label>
        <asp:Panel ID="HeaderPanel" runat="server" HorizontalAlign="left" Width="100%" Visible="false"><% If OptionDisplayQuestionEditButton Then%><b>Page Heading</b>: <% End if %>
            <asp:TextBox runat="server" ID="tbPageTitleEdit" AutoPostBack="true" CssClass="text"></asp:TextBox>
            <%--<asp:Button runat="server" ID="btnRenamePage" Text="Rename Page" Enabled="false" />--%>
        </asp:Panel>
        </asp:Panel>
    <asp:Table runat="server" ID="tblSurveyPageForm" CellSpacing="6" CellPadding="0" BorderWidth="0" CssClass="text"></asp:Table>
  </asp:Panel> 
</ContentTemplate>
</asp:UpdatePanel>