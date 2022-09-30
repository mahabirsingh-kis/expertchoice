s<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RatingScale.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.RatingScale" %>




<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>

<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">

        <a class="tt-toggler" id="1" data-toggler="tg-accordion">
            <span class="icon-tt-minus-square icon"></span>
            <span class="text">Rate the preference of alternatives with respect to <a href="#">Logical thought process</a></span>
        </a>
        <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-5"><span class="icon-tt-pencil"></span></a>

    </div>
    <div class="columns tt-accordion-content tg-accordion-1">
        <div class="panel tt-panel text-center">
            This is a demo created by Expert Choice. This is a tempalte created by Expert Choice. You can view results on a the Synthesis Results and Sensitivity Screens

                  

        </div>
        <div id="editCon-5" class="reveal-modal small tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">Lorem Title </h2>

            <div class="tt-modal-content large-12 columns">
                <textarea rows="10">
                      Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor.
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
    <!--
            <div class="columns tt-accordion-content">
              <div class="panel tt-panel text-center">
                This is a demo created by Expert Coice. This is a template created by Expert Coice. You can view results on the Synthesize Results and Sensitivity Screens.


              </div>
            </div>
-->

    <!--
    <div class="columns tt-question-title">
        <h3>Enter values from 0 to 1 for each objective</h3>
    </div>
-->

    <div class="columns tt-question-choices" style="margin-top: 20px;">
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <a class="tt-toggler" id="2" data-toggler="tg-accordion-sub">
                    <span class="icon-tt-minus-square icon"></span>
                    <span class="text">Candidate 1</span>
                </a>
                <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-5"><span class="icon-tt-pencil"></span></a>
            </div>
            <div class="columns tt-accordion-content tg-accordion-sub-2">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis.
                </div>
                <div id="editCon-4" class="reveal-modal small tt-modal-wrap tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                    <h2 id="modalTitle" class="tt-modal-header blue-header">Lorem Title </h2>

                    <div class="tt-modal-content large-12 columns">
                        <textarea rows="10">
                        Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor.
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

            <!--
                <div class="columns tt-accordion-content">
                  <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor.
                  </div>
                </div>
-->
        </div>
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <a class="tt-toggler" id="3" data-toggler="tg-accordion-sub">
                    <span class="icon-tt-minus-square icon"></span>
                    <span class="text">WRT Logical thought proces</span>
                </a>
                <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-5"><span class="icon-tt-pencil"></span></a>

            </div>
            <div class="columns tt-accordion-content tg-accordion-sub-3">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis.
                </div>
                <div id="editCon-4" class="reveal-modal small tt-modal-wrap tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                    <h2 id="modalTitle" class="tt-modal-header blue-header">Lorem Title </h2>

                    <div class="tt-modal-content large-12 columns">
                        <textarea rows="10">
                        Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor.
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


        </div>
    </div>
</div>

<div class="large-12 columns">
    <div class="tt-judgements-item large-12 columns tt-rating-scale-wrap">
        <div class="columns tt-j-title">
            <h3>
                <% if (view == "anytime")
                   { %>Your Judgment<% } %>
                <% if (view == "teamtime")
                   { %>Judgments<% } %>
            </h3>
        </div>
        <div class="large-uncollapse small-collapse tt-j-content tt-rating-scale-wrap">
            <div class="large-12 columns ">


              <div class="medium-6 columns">
                <% if (view == "anytime")
                     { %>
                  <div class="row collapse">
                      <div class="large-12 columns">

                          <ul class="tt-drag-slider-wrap">
                              <li>
                                  <div id="1" class="large-12 columns tt-drag-slider-item">
                                      <div class="medium-4 columns ">Candidate 1</div>
                                      <div class="medium-2 columns dsResetData1">0</div>
                                      <div class="medium-4 columns ">None</div>
                                      <div class="medium-2 columns"><a id="1" class=" ds-res-btn" title="reset judgment selection"><span class="icon-tt-close"></span></a></div>
                                  </div>
                              </li>
                              
                          </ul>
                      </div>
                  </div>
                  <% } %>

                  <% if (view == "teamtime")
                     { %>

                  <div class="row collapse">
                      <div class="tt-paginate-users">
                          <div class="large-4 columns">
                              <div class="switch-name-email">
                                  <span class="sort-label">Show</span>
                                  <select name="testSelect" onchange="toggleUserDisplay()" class="toggleuser toggle-name-email">
                                      <option value="1">Name Only</option>
                                      <option value="2">Email Only</option>
                                      <option value="3">Name & Email</option>
                                  </select>
                              </div>

                          </div>

                          <div class="large-8 columns text-right">
                                  <ul class="pagination ">
                                      <li class="arrow unavailable"><a href="">&laquo;</a></li>
                                      <li class="current"><a href="">1</a></li>
                                      <li><a href="">2</a></li>
                                      <li class="unavailable"><a href="">&hellip;</a></li>
                                      <li><a href="">10</a></li>
                                     <li>
                                        <select name="" class="show-smart">
                                            <option value="0">5</option>
                                            <option value="0">10</option>
                                            <option value="0">15</option>
                                            <option value="0">20</option>
                                        </select>
                                        </li>
                                      <li><a href="">All</a></li>
                                      <li class="arrow"><a href="">&raquo;</a></li>
                                    </ul>
                          </div>
                      </div>
                      <div class="large-12 columns">

                          <ul class="tt-drag-slider-wrap">
                              <li>
                                  <div id="1" class="large-12 columns tt-drag-slider-item">
                                      <div class="medium-6 columns active">You</div>
                                      <div class="medium-6 columns text-right">
                                            <select>
                                                <option value="0">Good</option>
                                                <option value="1">Not Rated</option>
                                                <option value="2">Very Good</option>
                                            </select>
                                      </div>
                                  </div>
                              </li>
                              
                              <li>
                                  <div id="1" class="large-12 columns tt-drag-slider-item">
                                      <div class="medium-6 columns ">John</div>
                                      <div class="medium-6 columns text-right">
                                            <select>
                                                <option value="0">Good</option>
                                                <option value="1" selected>Not Rated</option>
                                                <option value="2">Very Good</option>
                                            </select>
                                      </div>
                                  </div>
                              </li>
                              
                              <li>
                                  <div id="1" class="large-12 columns tt-drag-slider-item">
                                      <div class="medium-6 columns ">Jane</div>
                                      <div class="medium-6 columns text-right">
                                            <select>
                                                <option value="0">Good</option>
                                                <option value="1" >Not Rated</option>
                                                <option value="2" selected>Very Good</option>
                                            </select>
                                      </div>
                                  </div>
                              </li>
                             
                          </ul>
                      </div>
                  </div>
                    <% } %>
              </div>
              <div class="medium-6 columns">

                <div class="row collapse">
                    <div class="large-12 columns tt-rating-header">
                        <div class="small-6 columns text-left">
                            Industry Name
                        </div>
                        <div class="small-6 columns text-right">
                            Priority
                        </div>
                    </div>
                    <ul class="tt-rating-scale" large-12 columns>
                        <li class="row tt-rs-list li-1">
                            <label>
                                
                                <div class="large-6 columns">
                                    <div class="small-2 columns">
                                        <input type="radio" id="1" name="radioTest" class="rs">
                                    </div>
                                    <div class="small-10 columns">
                                        <span class="existing_priority-name ex-1">Outstanding</span>
                                        <input id="1" type="text" name="" class="edit_priority-name er-inpt er-1" value="Oustanding" placeholder="Oustanding">
                                        <a id="1" class="editThisRating" title="" href="#"><span class="icon-tt-pencil"></span></a>
                                    </div>
                                </div>
                                <div class="large-6 columns text-right">
                                    <div class="small-10 columns">
                                        <div class="rs-result-bar">
                                            <span class="bar columns " style="width: 100%;"></span>
                                        </div>
                                    </div>
                                    <div class="small-2 columns text-right">
                                        <span>100%</span>
                                    </div>
                                </div>
                                
                            </label>
                        </li>
                        
                        <li class="row tt-rs-list li-2">
                            <label>
                                
                                <div class="large-6 columns">
                                    <div class="small-2 columns">
                                        <input type="radio" id="2" name="radioTest" class="rs">
                                    </div>
                                    <div class="small-10 columns">
                                        <span class="existing_priority-name ex-2">Excellent</span>
                                        <input id="2" type="text" name="" class="edit_priority-name er-inpt er-2" value="Excellent" placeholder="Excellent">
                                        <a id="2" class="editThisRating" title="" href="#"><span class="icon-tt-pencil"></span></a>
                                    </div>
                                </div>
                                <div class="large-6 columns text-right">
                                    <div class="small-10 columns">
                                        <div class="rs-result-bar">
                                            <span class="bar columns " style="width: 70%;"></span>
                                        </div>
                                    </div>
                                    <div class="small-2 columns text-right">
                                        <span>70%</span>
                                    </div>
                                </div>
                                
                            </label>
                        </li>
                        
                        <li class="row tt-rs-list li-3">
                            <label>
                                
                                <div class="large-6 columns">
                                    <div class="small-2 columns">
                                        <input type="radio" id="3" name="radioTest" class="rs">
                                    </div>
                                    <div class="small-10 columns">
                                        <span class="existing_priority-name ex-3">Excellent</span>
                                        <input id="3" type="text" name="" class="edit_priority-name er-inpt er-3" value="Excellent" placeholder="Excellent">
                                        <a id="3" class="editThisRating" title="" href="#"><span class="icon-tt-pencil"></span></a>
                                    </div>
                                </div>
                                <div class="large-6 columns text-right">
                                    <div class="small-10 columns">
                                        <div class="rs-result-bar">
                                            <span class="bar columns " style="width: 70%;"></span>
                                        </div>
                                    </div>
                                    <div class="small-2 columns text-right">
                                        <span>70%</span>
                                    </div>
                                </div>
                                
                            </label>
                        </li>
                        
  
                    </ul>
                </div>

              </div>

            </div>

        </div>

        <div class="large-8 columns large-centered tt-j-clear text-center">
            <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise"><span class="icon-tt-close icon"></span></a>
        </div>



        
         
            <% if (view == "teamtime")
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
                        <div class="medium-4 columns">Priority: 0.51</div>
                        <div class="medium-4 columns direc-comparison-wrap">
                            <div class="dc-group-result-bar" style="width: 20%;"></div>
                        </div>
                        <div class="medium-4 columns text-center">Variance: 0.00</div>
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
