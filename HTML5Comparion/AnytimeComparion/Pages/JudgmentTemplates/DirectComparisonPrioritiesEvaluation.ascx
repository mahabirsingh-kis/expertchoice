<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DirectComparisonPrioritiesEvaluation.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.DirectComparisonPrioritiesEvaluation" %>



<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>

  
<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    
    <div class="columns tt-accordion-head blue">
          <a class="tt-toggler" id="1" data-toggler="tg-accordion">
              <span class="icon-tt-plus-square icon"></span>
              <span class="text">Priority of objectives with respect to "Goal: Select Best Consultant"</span>
          </a>
          <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-5"><span class="icon-tt-pencil"></span></a>
      </div>
    <div class="columns tt-accordion-content tg-accordion-1 hide">
          <div class="panel tt-panel text-center">
             You have completed prioritizing your objectives with respect to "Goal: Select Best Consultant."
              <br>
              Review your results below to ensure they make sense to you.
              <br>
              if not, you navigate back to the previous judgments to edit them

          </div>
          <div id="editCon-5" class="reveal-modal small tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
              <a class="close-reveal-modal" aria-label="Close">&#215;</a>
              <h2 id="modalTitle" class="tt-modal-header blue-header">Priority of objectives with respect to "Goal: Select Best Consultant"</h2>

              <div class="tt-modal-content large-12 columns">
                  <textarea rows="10">
                       You have completed prioritizing your objectives with respect to "Goal: Select Best Consultant." Review your results below to ensure they make sense to you. if not, you navigate back to the previous judgments to edit them
                      </textarea>
                  <div class="medium-6 columns">
                      <a href="#" class="button tiny tt-button-primary alert left">Cancel</a>
                  </div>
                  <div class="medium-6 columns">
                      <a href="#" class="button tiny tt-button-primary right ">Save</a>
                  </div>
                  <div class="columns">
                      <div data-alert class="alert-box success radius">
                          Updated successfully!
                          <a href="#" class="close">&times;</a>
                      </div>
                      <div data-alert class="alert-box alert radius">
                          Error on saving. Please try again.
                          <a href="#" class="close">&times;</a>
                      </div>
                  </div>

              </div>

          </div>
      </div>
    <%-- 
                    <div class="columns tt-question-title hide">
                        <h3>Results</h3>
                    </div>

                   <div class="columns tt-question-choices hide">
                        <div class="medium-6 columns">
                            <div class="columns tt-accordion-head blue">
                                <span class="icon-tt-minus-circle icon"></span>
                                <span class="text">Able to use facts and data</span>
                            </div>
                            <div class="columns tt-accordion-content">
                                <div class="panel tt-panel text-center">
                                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor. 
                                </div>
                            </div>
                            
                        </div>
                        <div class="vs">VS</div>
                        <div class="medium-6 columns">
                          <div class="columns tt-accordion-head bluee">
                              <span class="icon-tt-minus-circle icon"></span>
                              <span class="text">WRT Goal: Select Best Consultant</span>
                          </div>
                          <div class="columns tt-accordion-content">
                              <div class="panel tt-panel text-center">
                                  Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor. 
                              </div>
                          </div>

                          <!--<div class="columns tt-accordion-sub">
                            <div class="columns tt-accordion-head blue">
                                <span class="icon-tt-minus-circle icon"></span>
                                <span class="text">WRT Avoid Selling Shares</span>
                            </div>
                            <div class="columns tt-accordion-content">
                                <div class="panel tt-panel text-center">
                                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. 
                                </div>
                            </div>
                          </div>-->
                        </div>
                    </div> --%>
</div>

<div class="large-12 columns">
    <div class="tt-judgements-item large-12 columns">
        <div class="columns tt-j-title">
            <h3>
                <% if (view == "anytime")
                   { %>
                                Results
                              <% } %>
                <% if (view == "teamtime")
                   { %>
                                Goal: Select Best Consultant
                              <% } %>
            </h3>
        </div>
        <div class="columns tt-j-content">
            <div class="small-10 columns small-centered text-center">




                <% if (view == "teamtime")
                   { %>
                <div class="row collapse">
                    <div class="large-5 columns">
                        <span class="left">View Results: </span>
                        <select class="priorities-view-result">
                            <option>Normalized</option>
                            <option>Unnormalized</option>
                            <option>% of Max. Normalized</option>
                            <option>% of Max. Unnormalized</option>
                        </select>
                    </div>
                </div>

                <table class="tt-table-wrap priorities-cbr">
                    <thead>
                        <tr>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">No.</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">Name</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">Your  Results</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">Combined Results</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="">
                                <div class="text small-12 columns">Bar Graph</div>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr>
                            <td>1</td>
                            <td>Able to overcome obstacles</td>
                            <td class="blue">18.25%</td>
                            <td class="green">19.35%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 18.25%;"></div>
                                </div>
                                <div class="direc-comparison-wrap priorities cbr">
                                    <div class="priorities-result-bar green" style="width: 19.35%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>2</td>
                            <td>Logical though process</td>
                            <td class="blue">17.50%</td>
                            <td class="green">28.32%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 17.50%;"></div>
                                </div>
                                <div class="direc-comparison-wrap priorities cbr">
                                    <div class="priorities-result-bar green" style="width: 28.32%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>3</td>
                            <td>Solid communications skills</td>
                            <td class="blue">22.75%</td>
                            <td class="green">21.18%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 22.75%;"></div>
                                </div>
                                <div class="direc-comparison-wrap priorities cbr">
                                    <div class="priorities-result-bar green" style="width: 21.18%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>4</td>
                            <td>Able to use facts and data</td>
                            <td class="blue">12.24%</td>
                            <td class="green">21.22%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 12.24%;"></div>
                                </div>
                                <div class="direc-comparison-wrap priorities cbr">
                                    <div class="priorities-result-bar green" style="width: 21.22%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>5</td>
                            <td>Team Player</td>
                            <td class="blue">22.75%</td>
                            <td class="green">12.25%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 22.75%;"></div>
                                </div>
                                <div class="direc-comparison-wrap priorities cbr">
                                    <div class="priorities-result-bar green" style="width: 12.25%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>6</td>
                            <td>Math Skills</td>
                            <td class="blue">6.51%</td>
                            <td class="green">16.21%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 6.51%;"></div>
                                </div>
                                <div class="direc-comparison-wrap priorities cbr">
                                    <div class="priorities-result-bar green" style="width: 16.21%;"></div>
                                </div>
                            </td>
                        </tr>


                    </tbody>

                </table>
                <% } %>
                <% if (view == "anytime")
                   { %>

                <table class="responsive tt-table-wrap">
                    <thead>
                        <tr>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">No.</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">Name</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="tt-with-sorter">
                                <div class="text small-11 columns">Participant Results</div>
                                <div class="sorter-icons small-1 columns text-center">
                                    <a href="#"><span class="icon-tt-sort-toggle"></span></a>
                                </div>
                            </th>
                            <th class="">
                                <div class="text small-12 columns">Bar Graph</div>
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr>
                            <td>1</td>
                            <td>Able to overcome obstacles</td>
                            <td>18.25%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 18.25%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>2</td>
                            <td>Logical though process</td>
                            <td>17.50%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 17.50%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>3</td>
                            <td>Solid communications skills</td>
                            <td>22.75%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 22.75%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>4</td>
                            <td>Able to use facts and data</td>
                            <td>12.24%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 12.24%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>5</td>
                            <td>Team Player</td>
                            <td>22.75%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 22.75%;"></div>
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td>6</td>
                            <td>Math Skills</td>
                            <td>6.51%</td>
                            <td>
                                <div class="direc-comparison-wrap priorities">
                                    <div class="priorities-result-bar" style="width: 6.51%;"></div>
                                </div>
                            </td>
                        </tr>


                    </tbody>

                </table>


                <% } %>
            </div>

        </div>

        <div class="large-8 columns large-centered tt-j-clear text-center">
            <!--                            <a id="Equalizer" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear Judgment</a>-->
            <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j">Clear All Judgments</a>
        </div>


        <% if (doThis != "DirectComparisonPrioritiesEvaluation" && view == "teamtime")
           { %>


        <div class="columns tt-j-result">
            <div class="columns tt-normal-head blue text-left">
                <span class="icon-tt-chart icon"></span>
                <span class="text">GROUP RESULT</span>
            </div>

            <div class="columns">

                <!-- owner result -->
                <div class="row collapse">
                    <div class="large-12 columns tt-j-result-title">
                        <div class="large-3 columns">Priority: 0.51</div>
                        <div class="large-4 columns direc-comparison-wrap">
                            <div class="dc-group-result-bar" style="width: 20%;"></div>
                        </div>
                        <div class="large-5 columns text-center">Variance: 0.00</div>
                    </div>


                    <div class="medium-6 columns tt-j-result-graph hide">
                        <ul class="tt-equalizer-result">
                            <li class="eight lft active"></li>
                            <li class="seven lft active"></li>
                            <li class="six lft active"></li>
                            <li class="five lft active"></li>
                            <li class="four lft active"></li>
                            <li class="three lft active"></li>
                            <li class="two lft active"></li>
                            <li class="one lft active"></li>

                            <li class="zero mid active"></li>

                            <li class="one rgt active"></li>
                            <li class="two rgt active"></li>
                            <li class="three rgt active"></li>
                            <li class="four rgt active"></li>
                            <li class="five rgt active"></li>
                            <li class="six rgt active"></li>
                            <li class="seven rgt active"></li>
                            <li class="eight rgt active"></li>
                        </ul>

                        <div class="columns tt-j-result-comment"></div>
                    </div>
                </div>
                <!-- //owner result -->


            </div>
        </div>


        <% } %>
    </div>



</div>
