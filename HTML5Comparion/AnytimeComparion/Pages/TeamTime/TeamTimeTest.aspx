<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="teamtime.aspx.cs" Inherits="AnytimeComparion.test.teamtime" %>




<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% var  doThis = Request.QueryString["doThis"];  %>
<% var  view = "teamtime";  %>
<% var  style = Request.QueryString["style"];  %>
<% var  design = "evaluate";  %> 

<%@ Reference Control="~/pages/JudgmentTemplates/PairWiseComparison.ascx" %>
<%@ Reference Control="DirectUser.ascx" %>
    <%if (App.ActiveProject != null)
    {
    %>

        <div class="tt-body">
            <div class="row collapse tt-2-cols">
                <div class="large-12 columns tt-left-col" id="dv1" runat="server">

                    <% if (Action.ActionType == Canvas.ActionType.atPairwise) { %>
                             <%@ Register TagPrefix="JudgmentTemplates" TagName="PairWiseComparison" Src="~/Pages/JudgmentTemplates/PairWiseComparison.ascx"%>
                    
                             <JudgmentTemplates:PairWiseComparison runat="server" ID="PairWiseComparison2" />
                    <%}
                    if (Action.ActionType == Canvas.ActionType.atNonPWOneAtATime && measuretype.MeasurementType == ECCore.ECMeasureType.mtDirect)
                    { %>
                         <%@ Register src="DirectUser.ascx" TagName="DirectUser" tagprefix="JudgmentTemplates" %>
                          <JudgmentTemplates:DirectUser ID="DirectUser1" runat="server" />
                    < %} %>

                   <% if (Action.ActionType == Canvas.ActionType.atNonPWOneAtATime && measuretype.MeasurementType == ECCore.ECMeasureType.mtRatings)
                    { %>
                        <%@ Register src="RatingScale.ascx" tagname="RatingScale" tagprefix="uc1" %>
                        <uc1:RatingScale ID="RatingScale1" runat="server" />
                    <% } %>

                  <div class="large-12 columns tt-judgements-footer-nav">
                  <!-- tt-judgements-footer-nav-->
                  <%@ Register TagPrefix="includes" TagName="jfoterNav" Src="~/Pages/includes/JudgmentsFooterNav.ascx"%>
                  <includes:jfoterNav id="jfoterNav" runat="server" />
                </div>
                 
            </div>

          
          
            <div class="large-3 columns tt-comments-wrap hide tt-right-col">
              
                <!-- sidebar-->
                <%@ Register TagPrefix="MainPages" TagName="SidebarRight" Src="~/Pages/sidebar.ascx" %>
                <MainPages:SidebarRight id="SidebarRight" runat="server" />
                
            </div>


            </div>
 </div>

           
           
   <% } %>
</asp:Content>