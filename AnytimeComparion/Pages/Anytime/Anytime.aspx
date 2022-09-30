<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Anytime.aspx.vb" Inherits="AnytimeComparion.Anytime" %>

<%@ Register Src="~/Pages/Anytime/AllPairWiseUserCtrl.ascx" TagPrefix="uc1" TagName="AllPairWise" %>
<%@ Register Src="~/Pages/Anytime/InformationUserCtrl.ascx" TagPrefix="uc1" TagName="Information" %>
<%@ Register Src="~/Pages/Anytime/PairwiseUserCtrl.ascx" TagPrefix="uc1" TagName="PairWiseCtrl" %>
<%@ Register Src="~/Pages/Anytime/LocalResults.ascx" TagPrefix="uc1" TagName="LocalResults" %>
<%@ Register Src="~/Pages/Anytime/GlobalResults.ascx" TagPrefix="uc1" TagName="GlobalResults" %>
<%@ Register Src="~/Pages/Anytime/Pairwise.ascx" TagPrefix="uc1" TagName="Pairwise" %>
<%@ Register Src="~/Pages/Anytime/Survey.ascx" TagPrefix="uc1" TagName="Survey" %>
<%@ Register Src="~/Pages/Anytime/SensitivitiesAnalysis.ascx" TagPrefix="uc1" TagName="SensitivitiesAnalysis" %>
<%@ Register Src="~/Pages/includes/Footer.ascx" TagPrefix="includes" TagName="Footer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <script src="/assets/libs/jquery/jquery.min.js"></script>
    <script src="../../CustomScripts/anytime.js"></script>
    <form runat="server">
        <asp:Button ID="hdnPageNo" runat="server" OnClick="hdnPageNo_Click" Style="display: none;" />
        <asp:HiddenField ID="hdnPageNumber" runat="server" Value="0" />
        <asp:HiddenField ID="hdnCurrentStep" runat="server" Value="0" />
        <asp:HiddenField ID="hdnTotalSteps" runat="server" Value="0" />
        <asp:HiddenField ID="hdnparentnodeID" runat="server" Value="0" />
        <asp:HiddenField ID="intermediate_screen" runat="server" Value="0" />

        <%-- User Controls --%>
        <uc1:Information ID="InformationControl" runat="server" Visible="false" />
        <uc1:AllPairWise ID="AllPairWiseControl" runat="server" Visible="false" />
        <uc1:Survey ID="SurveyControl" runat="server" Visible="false" />
        <uc1:LocalResults ID="LocalResultsControl" runat="server" Visible="false" />
        <includes:Footer ID="footer1" runat="server" />
    </form>

</asp:Content>
