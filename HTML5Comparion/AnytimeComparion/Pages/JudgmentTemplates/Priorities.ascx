<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Priorities.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.Priorities" %>

<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>
<% var actions = Request.QueryString["actions"];  %>

<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">
        <span class="icon-tt-minus-circle icon"></span>
        <span class="text">Priority of objectives with respect to "Goal: Select Best Consultant"</span>
    </div>
    <div class="columns tt-accordion-content">
        <div class="panel tt-panel text-center">
            You have completed prioritizing your objectives with respect to "Goal: Select Best Consultant."
            <br>
            Review your results below to ensure they make sense to you.
            <br>
            if not, you navigate back to the previous judgments to edit them
                          
                              
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
                            <!--<div class="columns tt-accordion-sub">
                              <div class="columns tt-accordion-head blue">
                                  <span class="icon-tt-minus-circle icon"></span>
                                  <span class="text">WRT Goal: Select Best Consultant</span>
                              </div>
                              <div class="columns tt-accordion-content">
                                  <div class="panel tt-panel text-center">
                                      Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. 
                                  </div>
                              </div>
                            </div>-->
                        </div>
                        <div class="vs">VS</div>
                        <div class="medium-6 columns">
                          <div class="columns tt-accordion-head blue">
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
            <h3>Results</h3>
        </div>
        <div class="columns tt-j-content">
            <div class="large-10 medium-11 small-12 columns small-centered text-center">




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

                <table class="responsive tt-table-wrap priorities-cbr">
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
                <% if (actions == "redo")
                   { %>
                <table class="responsive tt-table-wrap">
                    <thead>
                        <tr>
                            <th></th>
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
                            <td>
                                <input type="checkbox"></td>
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
                            <td>
                                <input type="checkbox"></td>
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
                            <td>
                                <input type="checkbox"></td>
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
                            <td>
                                <input type="checkbox"></td>
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
                            <td>
                                <input type="checkbox"></td>
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
                            <td>
                                <input type="checkbox"></td>
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

                <% }
                   else
                   {%>
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

                <% } %>
            </div>

        </div>



        <% if (actions == "redo")
           { %>
        <div class="large-12 columns text-right">
            <hr>
            <div class="medium-6 columns">
                <p class="panel text-center" style="background: #FBECB0;">Select a pair of elements (by clicking the checkbox on left) for which you think: One has too high a priority, and the other has too low of a priority</p>
            </div>
            <div class="medium-6 columns">
                <a href="#" class="button tiny tt-button tt-button-primary right">
                    <span class="text">Re-evaluate</span>
                </a>
                <a href="#" class="button tiny tt-button-primary tt-button-transparent right">
                    <span class="text">Cancel</span>
                </a>
            </div>

        </div>
        <% }
           else
           {%>

        <div class="large-8 columns large-centered text-center">
            <a id="" class="button tiny tt-button-primary">Click here if you would like to redo a judgment for one pair of elementes</a>
        </div>
        <% } %>
    </div>
</div>
