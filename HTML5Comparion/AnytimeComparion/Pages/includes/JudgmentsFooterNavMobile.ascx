<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JudgmentsFooterNavMobile.ascx.cs" Inherits="AnytimeComparion.Pages.includes.JudgmentsFooterNavMobile1" %>

<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
  <% var doThis = Request.QueryString["doThis"];  %>
    <% var view = Request.QueryString["view"];  %>
      <% var style = Request.QueryString["style"];  %>
        <% var design = Request.QueryString["design"];  %>
          <% var actions = Request.QueryString["actions"];  %>


            <div class="row collpase footer-nav-mobile up">
              <button type="button" class="fnv-toggler tt-toggler" data-toggler="fmobile"><span class="icon-tt-sort-down icon"></span></button>
              <div class="pagination-wrap columns">
                <div class="small-9 columns ">
                  <% if (view == "teamtime" && actions == "evaluate") { %>
                    <div class="stepsWrap">
                      <span><strong>Steps:</strong> 7 of 53 </span><span><strong>Evaluated:</strong> 6/38</span>
                    </div>
                    <% } %>
                    <% if (view == "teamtime" && actions != "evaluate"){ %>
                      <div class="paginate-container">
                        <ul class="pagination">
                          <li class="arrow unavailable"><a href="">&laquo;</a></li>
                          <li class="current"><a href="">1</a></li>
                          <li><a href="">2</a></li>
                          <li class="unavailable"><a href="">&hellip;</a></li>
                          <li><a href="">12</a></li>
                          <li class="arrow"><a href="">&raquo;</a></li>
                        </ul>
                      </div>
                      <% } %>
                </div>
                <div class="small-3 columns text-right">
                  <div class="tt-c100 p19 size-30 circ-graph">
                    <span>19%</span>
                    <div class="slice">
                      <div class="bar"></div>
                      <div class="fill"></div>
                    </div>
                  </div>
                  <a href="#" class="right openDetailsWrap"><span class="icon-tt-ellipsis-v"></span></a>
                </div>
              </div>
              <div class="hsc-wrap columns">

                <% if (view == "teamtime" && actions != "evaluate"){ %>
                  <div class="small-4 columns text-center"><a href="#" data-reveal-id="tt-h-modal"><span class="icon-tt-site-map  icon-30"></span><span class="icon-text columns">Hierarchy</span></a></div>
                  <div class="small-4 columns text-center"><a href="#" data-reveal-id="tt-s-modal"><span class="icon-tt-th-list  icon-30"></span><span class="icon-text columns">Steps</span></a></div>
                  <div class="small-4 columns text-center"><a href="#" class=""><span class="icon-tt-comments  icon-30"></span><span class="icon-text columns">Comments</span></a></div>
                  <% } %>

                    <% if (view == "teamtime" && actions == "evaluate"){ %>
                      <div class="small-12 columns text-center"><a href="#"><span class="icon-tt-comments  icon-30"></span><span class="icon-text columns">Comments</span></a></div>
                      <% } %>
              </div>
            </div>

            <div class="row collapse details-wrap">
              <a class="close-details-wrap">&#215;</a>

              <div class="medium-6 columns">
                <label>
                  <input type="checkbox" name="">
                  <span>Auto Advance</span>
                </label>
              </div>



              <div class="medium-6  tt-overall-result-wrap columns">
                <% if (view == "teamtime"){ %>
                  <% if (doThis != "DirectComparisonPriorities" && doThis != "DirectComparisonPrioritiesEvaluation" && view == "teamtime" && actions != "evaluate"){ %>
                    <div class="columns">
                      <label>
                        <input type="checkbox" name="cb-showOverallResult">
                        <span>Show Overall Result</span>
                      </label>
                    </div>

                    <div class="columns tt-j-show-overall-result hide">
                      <div class="all">
                        <label>
                          <input type="radio" name="all-me">
                          <span>All</span></label>
                      </div>
                      <div class="me-only">
                        <label>
                          <input type="radio" name="all-me">
                          <span>Me only</span></label>
                      </div>
                    </div>
                    <% } %>
                      <div class="columns tt-j-stat-part">
                        <div class="panel">
                          Time since last judgement: <span class="time">5:34</span>
                          <br> Last Request: <span class="time">8:58:26 PM</span>
                          <br> Current Time: <span class="time">9:01:28 PM</span>
                          <br> Failed Requests: <span class="time">0</span>
                        </div>
                      </div>
                      <% } %>
              </div>

            </div>

            <!-- modal wrap -->
            <div id="tt-h-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
              <a class="close-reveal-modal" aria-label="Close">&#215;</a>
              <h2 id="modalTitle" class="tt-modal-header blue-header">Hierarchy</h2>

              <div class="tt-modal-content">
                        <div class="row">
                            <div class="large-12 columns">
                <ul class="tt-site-map-wrap">
                  <li data-toggler-id="p1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Goal: Improve Aircraft and Passenger Flight Security - Commercial Aviation</li>
                  <ul class="tg-sm-p1">
                    <!-- first item -->
                    <li data-toggler-id="s1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Financial Impact Considerations</li>
                    <ul class="tg-sm-s1">
                      <li><span class="icon-tt-disabled tt-sm-icon"></span>Research & Development Costs</li>
                      <li data-toggler-id="s1-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Implementation or Deployment Costs</li>
                      <ul class="tg-sm-s1-1">
                        <li data-toggler-id="s1-1-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Training</li>
                        <ul class="tg-sm-s1-1-1">
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Comprehensive Training Program Development</li>
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Administrater Initail Training</li>
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Periodic Retraining & Updates (Annual et al)</li>
                        </ul>
                        <li data-toggler-id="s1-1-2" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Equipment & Systems Installations</li>
                        <ul class="tg-sm-s1-1-2">
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Initial Installation Costs</li>
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Systems Repalcement Costs</li>
                        </ul>
                      </ul>
                    </ul>
                  </ul>
                  <!-- //first item -->
                  <!-- second item -->
                  <li data-toggler-id="p2" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Second Item</li>
                  <ul class="tg-sm-p2">
                    <li data-toggler-id="s2" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Lorem header</li>
                    <ul class="tg-sm-s2">
                      <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub</li>
                      <li data-toggler-id="s2-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Lorem sub header</li>
                      <ul class="tg-sm-s2-1">
                        <li data-toggler-id="s2-1-1" class="tt-toggler" data-toggler="tg-site-map"><span class="icon-tt-minus-circle tt-sm-icon"></span>Lorem sub header item</li>
                        <ul class="tg-sm-s2-1-1">
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub header item > item</li>
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub header item > item</li>
                          <li><span class="icon-tt-disabled tt-sm-icon"></span>Lorem sub header item > item</li>
                        </ul>

                      </ul>
                    </ul>
                  </ul>
                  <!-- //second item -->


                </ul>
                    </div>
                  </div>
              </div>

            </div>
            <!-- // modal wrap -->
            <!-- modal wrap -->
<div id="tt-s-modal" class="reveal-modal medium tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                    <h2 id="modalTitle" class="tt-modal-header blue-header">Steps <a href="#" data-reveal-id="tt-help-modal"><span class="icon-tt-question-circle help-icon size20"></span></a></h2>
                    <div class="tt-modal-content">
                        <ul class="">
                            <%
                                var actiond = (List<Canvas.clsAction>)Session["action"];
                                var obj = (List<ECCore.clsNode>)App.ActiveProject.HierarchyObjectives.Nodes;
                                var count = 0;
                                var steps = TeamTimeClass.TeamTimePipeStep;
                                for (int step = 1; step <= TeamTimeClass.TeamTime.Pipe.Count; step++)
                                {
                                    var action = actiond[step - 1];
                                 %>
                                <li class="notice"><a  href="mteamtime.aspx?pipe=<%=step%>">
                                <%if (step == 1)
                                  { %>#<%=step%>: "Welcome" Page <% } %>
                                      <%if (action.ActionType == Canvas.ActionType.atPairwise)
                                      {
                                            List<object> nodes = TeamTimeClass.getnodes(step);
                                            var firstnode = (ECCore.clsNode) nodes[4];
                                            var secondnode = (ECCore.clsNode) nodes[5];
                                            %>#<%=step%>: Pairwise for "<%=firstnode.NodeName%>" VS "<%=secondnode.NodeName%>"  
                                    <% } %>

                                    <%if (action.ActionType == Canvas.ActionType.atShowLocalResults)
                                    { %>
                                        #<%=step%>: Priorities of "<%if (count == 0)
                                         {%>Objectives<%}
                                         else
                                         {%>Alternatives<%} %>" with respect to "<%=obj[count].NodeName%>" <% count += 1;
                                            } %>

                                        <%if (action.ActionType == Canvas.ActionType.atShowGlobalResults)
                                        { %> #<%=step%>: Overall results for "<%=obj[0].NodeName%>" <%} %>

                                          <%if (step == TeamTimeClass.TeamTime.Pipe.Count)
                                           { %>#<%=step%>: "Thank You" Page <% } %>
                                 </a></li>

                         <% } %>



                    </div>
                </div>
            <!-- // modal wrap -->

            <!-- help modal wrap -->
            <div id="tt-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
              <!--                                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>-->
              <a class='open-first close-reveal-modal second-modal-btn'>&#215;</a>
              <h2 id="modalTitle" class="tt-modal-header blue-header">Judgment Links Info</h2>
              <div class="tt-modal-content large-12 columns">

                <div class="row"><div class="large-12 columns">
                    <p>The current step is displayed with an <span class="label orange">orange</span> background. The step numbers are colored as follows:</p>
                </div>
                </div>
                <div class="row">
                  <div class="small-2 columns">Red </div>
                  <div class="small-10 columns">- judgement has not yet been made</div>
                  <div class="small-2 columns">Blue </div>
                  <div class="small-10 columns">- results or information steps</div>
                  <div class="small-2 columns">Black </div>
                  <div class="small-10 columns">- Judgement has been made</div>
                </div>
              </div>
            </div>
            <!-- //help modal wrap -->