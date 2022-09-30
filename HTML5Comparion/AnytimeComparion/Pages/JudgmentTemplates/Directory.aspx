<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Directory.aspx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.Directory" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <div class="tt-body">
        <% if (Session["User"] != null || Session["UserType"] != null){ %>
        <div class="row collapse tt-2-cols">
          
            <div class="large-12 columns tt-left-col">
                <div class="large-12 columns">
                  <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
                  <div class="columns  tt-header-nav blue-header">
                      <span class="text">My Projects</span>
                  </div>
                  <div data-equalizer class="large-12 columns">
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>Direct Comparison</h3>
                    
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparison&view=anytime&style=&design">Direct Comparison - PM (Anytime)</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparison&view=teamtime&style=&design">Direct Comparison - PM (TeamTime)</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparisonEvaluation&view=anytime&style=&design">Direct Comparison - Anytime Evaluation</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparisonEvaluation&view=teamtime&style=&design">Direct Comparison - TeamTime Evaluation</a></li>
                      
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPriorities&view=anytime&style=&design">Direct Comparison > Priorities - PM (Anytime)</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPriorities&view=teamtime&style=&design">Direct Comparison > Priorities - PM (TeamTime)</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPrioritiesEvaluation&view=anytime&style=&design">Direct Comparison > Priorities - Anytime</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=DirectComparisonPrioritiesEvaluation&view=teamtime&style=&design">Direct Comparison > Priorities - TeamTime</a></li>
                      
                    </ul>
                    </div>  
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>Pairwise Comparison </h3>
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=PairWise&view=anytime&style=&design">Pairwise Comparison - PM (Anytime) wrt </a></li>
                      <li><a runat="server" href="~/Judgement?doThis=PairWise&view=teamtime&style=&design">Pairwise Comparison - PM (TeamTime) wrt </a></li>
                      <li><a runat="server" href="~/Judgement?doThis=PairWiseEvaluation&view=anytime&style=&design">Pairwise Comparison - Anytime Evaluation </a></li>
                      <li><a runat="server" href="~/Judgement?doThis=PairWiseEvaluation&view=teamtime&style=verbal&design">Pairwise Comparison - TeamTime Evaluation (Verbal) </a></li>
                      <li><a runat="server" href="~/Judgement?doThis=PairWiseEvaluation&view=teamtime&style=numerical&design">Pairwise Comparison - TeamTime Evaluation (Numerical) </a></li>
                  </ul> 
                      </div>  
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>All Pairwise Comparison</h3>
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=AllPairWiseComparison&view=anytime&style=&design=1">All Pairwise Comparison - PM (Anytime) design 1</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=AllPairWiseComparison&view=anytime&style=&design=2">All Pairwise Comparison - PM (Anytime) design 2 </a></li>
                    </ul> 
                      </div>  
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>Rating Scale</h3>
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=RatingScale&view=anytime&style=&design">Rating Scale - PM (Anytime) wrt </a></li>
                      <li><a runat="server" href="~/Judgement?doThis=RatingScaleEvaluation&view=anytime&style=&design">Rating Scale - Anytime</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=RatingScaleEvaluation&view=teamtime&style=&design">Rating Scale - TeamTime Evaluation</a></li>
                    </ul> 
                      </div> 
                  </div>
                  <div data-equalizer class="large-12 columns">
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>Simple Utility Curve</h3>
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=SimpleUtilityCurve&view=anytime&style=&design">Simple Utility Curve - PM (Anytime) wrt</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=SimpleUtilityCurve&view=teamtime&style=&design">Simple Utility Curve - PM (TeamTime) wrt</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=SimpleUtilityCurveEvaluation&view=anytime&style=&design">Simple Utility Curve - Anytime</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=SimpleUtilityCurveEvaluation&view=teamtime&style=&design">Simple Utility Curve - TeamTime Evaluation</a></li>
                    </ul>
                      </div>  
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>Step Funcion</h3>
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=StepFunction&view=anytime&style=&design">Step Function - PM (Anytime) wrt</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=StepFunction&view=teamtime&style=&design">Step Function - PM (TeamTime) wrt</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=StepFunctionEvaluation&view=anytime&style=&design">Step Function - Anytime</a></li>
                      <li><a runat="server" href="~/Judgement?doThis=StepFunctionEvaluation&view=teamtime&style=&design">Step Function - TeamTime Evaluation</a></li>
                    </ul>
                      </div>  
                    <div data-equalizer-watch class="large-3 medium-4 columns"><h3>Judgement Priorities</h3>
                    <ul>
                      <li><a runat="server" href="~/Judgement?doThis=Priorities&view=anytime&style=&design">Judgement Priorities - PM (Anytime)</a></li>
                  
                    </ul> 
                    </div> 
                    
                  </div>
                  
              </div>
            </div>
            <div class="large-3 columns tt-comments-wrap hide tt-right-col">
                <!-- sidebar-->
                <%--<%@ Register TagPrefix="MainPages" TagName="SidebarRight" Src="~/Pages/sidebar.ascx" %>
                <MainPages:SidebarRight id="SidebarRight" runat="server" />--%>

              </div>
            
        </div>
        <% } else { %>
        <div class="row">
          <div class="large-12 columns text-center">
            
             <%-- <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
              <includes:loginMessage id="needToLogin" runat="server" />--%>
                
          </div>
        </div>
      <% } %>
    </div>
</asp:Content>
