<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PairWiseComparison.ascx.cs" Inherits="AnytimeComparion.Pages.JudgmentTemplates.PairWiseComparison" ClassName="PairWiseComparison" %>




<% var doThis = Request.QueryString["doThis"];  %>
<% var view = Request.QueryString["view"];  %>
<% var style = Request.QueryString["style"];  %>
<% var design = Request.QueryString["design"];  %>
<% var pairwisetype = Request.QueryString["pairwisetype"];  %>

<div class="large-12 columns">
    <div class="tt-toggle-comments-wrap toggleComments tc-btn-left"><span>show comments</span></div>
    <div class="columns tt-accordion-head blue">
        <a class="tt-toggler" id="1" data-toggler="tg-accordion">
            <span class="icon-tt-plus-square icon"></span>
            <span class="text">With respect to
                <asp:Label ID="goals" runat="server" Text=""></asp:Label>: Select Best Consultant</span>
        </a>
        <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-5"><span class="icon-tt-pencil"></span></a>
    </div>
    <div class="columns tt-accordion-content tg-accordion-1 hide">
        <div class="panel tt-panel text-center">
            This is a demo created by Expert Choice. This is a tempalte created by Expert Choice. You can view results on a the Synthesis Results and Sensitivity Screens

                  <ul>
                      <li>First sample line</li>
                      <li>Second sample line</li>
                      <li>Third sample line</li>
                  </ul>

        </div>
        <div id="editCon-5" class="reveal-modal small tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
            <h2 id="modalTitle" class="tt-modal-header blue-header">Lorem Title </h2>

            <div class="tt-modal-content large-12 columns">
                <textarea rows="10" name="modalMce">
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

   <% if (design != "multi"){ %>
    <div class="columns tt-question-title">
        <h3>Which of the two objectives below is more preferable?</h3>
    </div>

       <div class="columns tt-question-choices">
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <a class="tt-toggler" id="1" data-toggler="tg-accordion-sub">
                    <span class="icon-tt-plus-square icon"></span>
                    <span class="text" id="leftnode" runat="server"></span>
                </a>
                <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-3"><span class="icon-tt-pencil"></span></a>
            </div>
            <div class="columns tt-accordion-content tg-accordion-sub-1 hide">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor.
                </div>
                <div id="editCon-3" class="reveal-modal small tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
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
            <div class="columns tt-accordion-sub">
                <div class="columns tt-accordion-head">
                    <a class="tt-toggler" id="2" data-toggler="tg-accordion-sub">
                        <span class="icon-tt-plus-square icon"></span>
                        <span class="text">WRT Goal: Select Best Consultant</span>
                    </a>
                    <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-4"><span class="icon-tt-pencil"></span></a>
                </div>
                <div class="columns tt-accordion-content tg-accordion-sub-2 hide">
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
        <div class="vs">VS</div>
        <div class="medium-6 columns">
            <div class="columns tt-accordion-head blue">
                <a class="tt-toggler" id="3" data-toggler="tg-accordion-sub">
                    <span class="icon-tt-plus-square icon"></span>
                    <span class="text" id="rightnode" runat="server">Lorem Title </span>
                </a>
                <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-1"><span class="icon-tt-pencil"></span></a>
            </div>
            <div class="columns tt-accordion-content tg-accordion-sub-3 hide">
                <div class="panel tt-panel text-center">
                    Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis. Suspendisse porttitor bibendum rutrum. Donec tempor mi vitae urna volutpat lacinia. Praesent finibus cursus commodo. Nullam vitae risus mi. Fusce aliquam eros et enim bibendum eleifend. Donec eu nunc vitae libero tempor dapibus sit amet facilisis dolor.
                </div>

                <div id="editCon-1" class="reveal-modal small tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
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

            <div class="columns tt-accordion-sub">
                <div class="columns tt-accordion-head">
                    <a class="tt-toggler" id="4" data-toggler="tg-accordion-sub">
                        <span class="icon-tt-plus-square icon"></span>
                        <span class="text">WRT Goal: Select Best Consultant</span>
                    </a>
                    <a class="edit-pencil" title="" href="#" data-reveal-id="editCon-2"><span class="icon-tt-pencil"></span></a>
                </div>
                <div class="columns tt-accordion-content tg-accordion-sub-4 hide">
                    <div class="panel tt-panel text-center">
                        Mauris eu enim quis mi elementum faucibus at ut arcu. Phasellus vulputate orci id faucibus maximus. Maecenas nec lacus eu orci tempus sagittis.
                    </div>
                    <div id="editCon-2" class="reveal-modal small tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
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
       <% } %>
</div>

<div class="large-12 columns">
    <div class="tt-judgements-item large-12 columns">
        <div class="columns tt-j-title">
            <h3>
                <% if (view == "anytime")
                   { %>
                                Your Judgment
                              <% } %>
                <% if (view == "teamtime")
                   { %>
                                Judgments
                              <% } %>
            </h3>
        </div>
        <div class="columns tt-j-content">
            <div class="small-10 columns small-centered text-center">

                <% if (design == "multi"){ %>
                    <div class="columns text-center">
                    <h3>Area Validation Excercise</h3>
                </div>

                <div class="columns tt-j-result ">


                        <div class="columns">


                            <div class="row collapse">
                                <div class="large-8 columns large-centered tt-j-clear text-center">
                                    <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise">Clear All Judgments</a>
                                </div>

                                <div class="tt-j-others-result columns">
                                   
                                    <!-- start multi -->
                                    <div class="tt-multi-pairwise-wrap large-12 columns tt-j-others-result-content1 desktop">
                                        <div class="multi-loop-wrap">
                                            
                                        <!-- LOOP start -->
                                            
                                            <!-- ///start top row -->
                                        <div id="1" class="top-row large-12 columns">

                                            <div class="medium-3 columns text-center">
                                                  <a data-dropdown="drop1" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                                  <span class="">Circle A: Circle</span>
                                                  <a class="disabled" data-dropdown="drop2" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                                    
                                             </div>


                                            <div class="medium-6 columns top-row-mid">
                                                <div class="small-11 columns text-left">
                                                     
                                                    <!-- START only show this labels in first child -->
                                                    <ul class="tt-equalizer-result tt-equalizer-result-labels">
                                                        <li class="eight lft  ownerResult-1 with-line-bg">
                                                            <span class="label-text">EX</span> 
                                                        </li>
                                                        <li class="seven lft  ownerResult-1"></li>
                                                        <li class="six lft  ownerResult-1 with-line-bg"><span class="label-text">VS</span></li>
                                                        <li class="five lft ownerResult-1"></li>
                                                        <li class="four lft ownerResult-1 with-line-bg"><span class="label-text">S</span></li>
                                                        <li class="three lft active no-gradient ownerResult-1"></li>
                                                        <li class="two lft active no-gradient ownerResult-1 with-line-bg"><span class="label-text">M</span></li>
                                                        <li class="one lft active no-gradient ownerResult-1"></li>

                                                        <li class="zero mid active no-gradient ownerResult-1 with-line-bg"><span class="label-text">EQ</span></li>

                                                        <li class="one rgt ownerResult-1"></li>
                                                        <li class="two rgt ownerResult-1 with-line-bg"><span class="label-text">M</span></li>
                                                        <li class="three rgt ownerResult-1"></li>
                                                        <li class="four rgt ownerResult-1 with-line-bg"><span class="label-text">S</span></li>
                                                        <li class="five rgt ownerResult-1"></li>
                                                        <li class="six rgt ownerResult-1 with-line-bg"><span class="label-text">VS</span></li>
                                                        <li class="seven rgt ownerResult-1"></li>
                                                        <li class="eight rgt ownerResult-1 with-line-bg"><span class="label-text">EX</span></li>
                                                    </ul>
                                                    <!-- END only show this labels in first child -->
                                                    
                                                    <ul class="tt-equalizer-result">
                                                        <li class="eight lft  ownerResult-1"></li>
                                                        <li class="seven lft  ownerResult-1"></li>
                                                        <li class="six lft  ownerResult-1"></li>
                                                        <li class="five lft ownerResult-1"></li>
                                                        <li class="four lft ownerResult-1"></li>
                                                        <li class="three lft active no-gradient ownerResult-1"></li>
                                                        <li class="two lft active no-gradient ownerResult-1"></li>
                                                        <li class="one lft active no-gradient ownerResult-1"></li>

                                                        <li class="zero mid active no-gradient ownerResult-1"></li>

                                                        <li class="one rgt ownerResult-1"></li>
                                                        <li class="two rgt ownerResult-1"></li>
                                                        <li class="three rgt ownerResult-1"></li>
                                                        <li class="four rgt ownerResult-1"></li>
                                                        <li class="five rgt ownerResult-1"></li>
                                                        <li class="six rgt ownerResult-1"></li>
                                                        <li class="seven rgt ownerResult-1"></li>
                                                        <li class="eight rgt ownerResult-1"></li>
                                                    </ul>
                                                    <a id="1" class="reset-btn clrPairwiseResult" title="">
                                                        <span class="icon-tt-close"></span>
                                                    </a>
                                                </div>
                                            </div>
                                            <div class="medium-3 columns text-center">
                                                <a class="disabled" data-dropdown="drop3" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                                <span> Triangle B: Triangle</span>
                                                <a data-dropdown="drop4" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                            </div>


                                        </div>
                                            <!-- ///end top row -->
                                        
                                            
                                        <div id="2" class="large-12 columns">

                                            <div class="medium-3 columns text-center">
                                                  <a class="disabled" data-dropdown="drop1" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                                  <span class="">Circle A: Circle</span>
                                                  <a data-dropdown="drop2" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                                
                                                
                                                    
                                             </div>


                                            <div class="medium-6 columns">
                                                <div class="small-11 columns text-left">
                                                     
                                                    
                                                    
                                                    <ul class="tt-equalizer-result">
                                                        <li class="eight lft  ownerResult-1"></li>
                                                        <li class="seven lft  ownerResult-1"></li>
                                                        <li class="six lft  ownerResult-1"></li>
                                                        <li class="five lft ownerResult-1"></li>
                                                        <li class="four lft ownerResult-1"></li>
                                                        <li class="three lft  ownerResult-1"></li>
                                                        <li class="two lft  ownerResult-1"></li>
                                                        <li class="one lft  ownerResult-1"></li>

                                                        <li class="zero mid active no-gradient ownerResult-1"></li>

                                                        <li class="one rgt active no-gradient ownerResult-1"></li>
                                                        <li class="two rgt active no-gradient ownerResult-1"></li>
                                                        <li class="three rgt active no-gradient ownerResult-1"></li>
                                                        <li class="four rgt ownerResult-1"></li>
                                                        <li class="five rgt ownerResult-1"></li>
                                                        <li class="six rgt ownerResult-1"></li>
                                                        <li class="seven rgt ownerResult-1"></li>
                                                        <li class="eight rgt ownerResult-1"></li>
                                                    </ul>
                                                    <a id="1" class="reset-btn clrPairwiseResult" title="">
                                                        <span class="icon-tt-close"></span>
                                                    </a>
                                                </div>
                                                
                                            </div>
                                            <div class="medium-3 columns text-center">
                                                <a data-dropdown="drop3" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                                <span> Triangle B: Triangle</span>
                                                <a class="disabled" data-dropdown="drop4" aria-controls="drop1" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                            </div>


                                        </div>
                                       <!-- LOOP ends -->
                                        </div>
                                        
                                        

                                    </div>
                                     <div id="drop1" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <p>Si noster illustriora ut nam cernantur consectetur si id noster ipsum nulla 
                                            voluptate, ingeniis do aliquip e ad magna fore duis iudicem te admodum est esse 
                                            officia, quibusdam cillum arbitror aliquip, culpa qui de varias doctrina. Ex 
                                            arbitror sempiternum ut fore aut qui magna i </p>
                                    </div>
                                    <div id="drop2" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <p>Si noster illustriora ut nam cernantur consectetur si id noster ipsum nulla 
                                            voluptate, ingeniis do aliquip e ad magna fore duis iudicem te admodum est esse 
                                            officia, quibusdam cillum arbitror aliquip, culpa qui de varias doctrina. Ex 
                                            arbitror sempiternum ut fore aut qui magna i </p>
                                    </div>
                                    
                                    <div id="drop3" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <p>Si noster illustriora ut nam cernantur consectetur si id noster ipsum nulla 
                                            voluptate, ingeniis do aliquip e ad magna fore duis iudicem te admodum est esse 
                                            officia, quibusdam cillum arbitror aliquip, culpa qui de varias doctrina. Ex 
                                            arbitror sempiternum ut fore aut qui magna i </p>
                                    </div>
                                    <div id="drop4" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                                        <p>Si noster illustriora ut nam cernantur consectetur si id noster ipsum nulla 
                                            voluptate, ingeniis do aliquip e ad magna fore duis iudicem te admodum est esse 
                                            officia, quibusdam cillum arbitror aliquip, culpa qui de varias doctrina. Ex 
                                            arbitror sempiternum ut fore aut qui magna i </p>
                                    </div>
                                    <!-- end multi -->
                                    
                                </div>
                            </div>
                        </div>
                    </div>
                <% } else{ %>
                    
                    
                    
                    <% if (design == "numerical")
                       { %>
                    <div class="columns tt-j-content">
                        <div class="small-1 columns text-right">A</div>
                        <div class="small-10 columns text-center">
                            <a href="#"><span class="icon-tt-chevron-left"></span></a>
                            <ul class="tt-equalizer-numeric">

                                <li id="1" title="Extreme" class="nine lft lft-eq lft-1" data-pos="lft" data-value="9"><span>9</span></li>
                                <li id="2" title="Very Strong to Extreme" class="eight lft lvl-even-bar lft-eq lft-2" data-pos="lft"><span>8</span></li>
                                <li id="3" title="Very Strong" class="seven lft lft-eq lft-3" data-pos="lft"><span>7</span></li>
                                <li id="4" title="Strong to Very Strong" class="six lft lvl-even-bar lft-eq lft-4" data-pos="lft"><span>6</span></li>
                                <li id="5" title="Strong" class="five lft lft-eq lft-5" data-pos="lft"><span>5</span></li>
                                <li id="6" title="Equal to Strong" class="four lft lvl-even-bar lft-eq lft-6" data-pos="lft"><span>4</span></li>
                                <li id="7" title="Moderate" class="three lft lft-eq lft-7" data-pos="lft"><span>3</span></li>
                                <li id="8" title="Equal to Moderate" class="two lft lvl-even-bar lft-eq lft-8" data-pos="lft"><span>2</span></li>

                                <li id="9" title="Equal" class="zero mid lft-9 rgt-eq rgt-9 mid-9" data-pos="mid"><span>1</span></li>

                                <li id="8" title="Equal to Moderate" class="two rgt lvl-even-bar rgt-eq rgt-8" data-pos="rgt"><span>2</span></li>
                                <li id="7" title="Moderate" class="three rgt rgt-eq rgt-7" data-pos="rgt"><span>3</span></li>
                                <li id="6" title="Moderate to Strong" class="four rgt lvl-even-bar rgt-eq rgt-6" data-pos="rgt"><span>4</span></li>
                                <li id="5" title="Strong" class="five rgt rgt-eq rgt-5 " data-pos="rgt"><span>5</span></li>
                                <li id="4" title="Strong to Very Strong" class="six rgt lvl-even-bar rgt-eq rgt-4" data-pos="rgt"><span>6</span></li>
                                <li id="3" title="Very Strong" class="seven rgt rgt-eq rgt-3" data-pos="rgt"><span>7</span></li>
                                <li id="2" title="Very Strong to Extreme" class="eight rgt lvl-even-bar rgt-eq rgt-2" data-pos="rgt"><span>8</span></li>
                                <li id="1" title="Extreme" class="nine rgt rgt-eq rgt-1" data-pos="rgt"><span>9</span></li>

                            </ul>
                            <a href="#"><span class="icon-tt-chevron-right"></span></a>

                            <div class="columns ratio-box">
                                <div class="large-8 columns large-centered text-center rb-wrap">
                                    <div class="medium-1 columns text-right ">
                                        <a href="#"><span class="icon-tt-back-fort"></span></a>
                                    </div>
                                    <div class="medium-10 columns">
                                        <input type="text" class="textbox left" value="4.300" readonly>:
                                    <input type="text" class="textbox right" value="1.000" readonly>
                                    </div>
                                    <div class="medium-1 columns text-left">
                                        <a href="#"><span class="icon-tt-close"></span></a>
                                    </div>
                                </div>

                            </div>

                        </div>
                        <div class="small-1 columns">B</div>

                    </div>



                            <% }
                       else
                       { %>
                            </small>
                            <div class="columns tt-j-content">

                                <div class="small-12 columns text-center">

                                    <ul class="tt-equalizer-levels">

                                        <li title="" class="nine lft">
                                            <span class="lvl-txt">Extreme</span>
                                            <asp:Image ID="Image2" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-1 eight lft">
                                            <span class="lvl-txt hide">Very Strong<br>
                                                to Extreme</span>
                                            <asp:Image ID="Image3" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="seven lft">
                                            <span class="lvl-txt">Very Strong</span>
                                            <asp:Image ID="Image4" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-2 six lft">
                                            <span class="lvl-txt hide">Strong to<br>
                                                Very Strong</span>
                                            <asp:Image ID="Image5" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="five lft">
                                            <span class="lvl-txt">Strong</span>
                                            <asp:Image ID="Image6" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-3 four lft">
                                            <span class="lvl-txt hide">Equal to<br>
                                                Strong</span>
                                            <asp:Image ID="Image7" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="three lft">
                                            <span class="lvl-txt">Moderate</span>
                                            <asp:Image ID="Image8" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-4 two lft">
                                            <span class="lvl-txt hide">Equal to<br>
                                                Moderate</span>
                                            <asp:Image ID="Image9" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>

                                        <li title="" class="zero mid">
                                            <span class="lvl-txt">Equal</span>
                                            <asp:Image ID="Image10" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>

                                        <li title="" class="lvl-even lvl-e-5 two rgt">
                                            <span class="lvl-txt hide">Equal to<br>
                                                Moderate</span>
                                            <asp:Image ID="Image11" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="three rgt">
                                            <span class="lvl-txt">Moderate</span>
                                            <asp:Image ID="Image12" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-6 four rgt">
                                            <span class="lvl-txt hide">Moderate
                                                <br>
                                                to Strong</span>
                                            <asp:Image ID="Image17" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="five rgt">
                                            <span class="lvl-txt">Strong</span>
                                            <asp:Image ID="Image13" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-7 six rgt">
                                            <span class="lvl-txt hide">Strong to<br>
                                                Very Strong</span>
                                            <asp:Image ID="Image14" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="seven rgt">
                                            <span class="lvl-txt">Very Strong</span>
                                            <asp:Image ID="Image18" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="lvl-even lvl-e-8 eight rgt">
                                            <span class="lvl-txt hide">Very Strong<br>
                                                to Extreme</span>
                                            <asp:Image ID="Image15" class="hide lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                        <li title="" class="nine rgt">
                                            <span class="lvl-txt">Extreme</span>
                                            <asp:Image ID="Image16" class="lvl-line" runat="server" ImageUrl='/Images/level-line.png'></asp:Image>
                                        </li>
                                    </ul>

                                    <ul class="tt-equalizer">

                                        <li id="1" title="Extreme" class="nine lft lft-eq lft-1" data-pos="lft"><span>9</span></li>
                                        <li id="2" title="Very Strong to Extreme" class="eight lft lvl-even-bar lft-eq lft-2" data-pos="lft"><span>8</span></li>
                                        <li id="3" title="Very Strong" class="seven lft lft-eq lft-3" data-pos="lft"><span>7</span></li>
                                        <li id="4" title="Strong to Very Strong" class="six lft lvl-even-bar lft-eq lft-4" data-pos="lft"><span>6</span></li>
                                        <li id="5" title="Strong" class="five lft lft-eq lft-5" data-pos="lft"><span>5</span></li>
                                        <li id="6" title="Equal to Strong" class="four lft lvl-even-bar lft-eq lft-6" data-pos="lft"><span>4</span></li>
                                        <li id="7" title="Moderate" class="three lft lft-eq lft-7" data-pos="lft"><span>3</span></li>
                                        <li id="8" title="Equal to Moderate" class="two lft lvl-even-bar lft-eq lft-8" data-pos="lft"><span>2</span></li>

                                        <li id="9" title="Equal" class="zero mid lft-9 rgt-eq rgt-9 mid-9" data-pos="mid"><span>1</span></li>

                                        <li id="8" title="Equal to Moderate" class="two rgt lvl-even-bar rgt-eq rgt-8" data-pos="rgt"><span>2</span></li>
                                        <li id="7" title="Moderate" class="three rgt rgt-eq rgt-7" data-pos="rgt"><span>3</span></li>
                                        <li id="6" title="Moderate to Strong" class="four rgt lvl-even-bar rgt-eq rgt-6" data-pos="rgt"><span>4</span></li>
                                        <li id="5" title="Strong" class="five rgt rgt-eq rgt-5" data-pos="rgt"><span>5</span></li>
                                        <li id="4" title="Strong to Very Strong" class="six rgt lvl-even-bar rgt-eq rgt-4" data-pos="rgt"><span>6</span></li>
                                        <li id="3" title="Very Strong" class="seven rgt rgt-eq rgt-3" data-pos="rgt"><span>7</span></li>
                                        <li id="2" title="Very Strong to Extreme" class="eight rgt lvl-even-bar rgt-eq rgt-2" data-pos="rgt"><span>8</span></li>
                                        <li id="1" title="Extreme" class="nine rgt rgt-eq rgt-1" data-pos="rgt"><span>9</span></li>
                                    </ul>

                                    <div class="columns">
                                    </div>

                                </div>

                            </div>
                    <% } %>


                    <% if (view == "teamtime")
                       { %>

                    <div class="columns tt-j-result ">


                        <div class="columns">


                            <div class="row collapse">
                                <div class="large-8 columns large-centered tt-j-clear text-center">
                                    <a id="" class="button tiny tt-button-primary tt-action-btn-clr-j clrPairwise">Clear All Judgments</a>
                                </div>

                                <div class="tt-j-others-result columns">
                                    <div class="switch-name-email">
                                      <select class="toggle-name-email">
                                        <option value="1">Name Only</option>
                                        <option value="2">Email Only</option>
                                        <option value="3">Name with Email</option>
                                      </select>
                                    </div>

                                    <small>
                                        <a class="tt-toggler tgidr-btn" id="idr" data-toggler="tg">
                                            <span class="icon-tt-plus-square icon"></span>
                                            <span class="text">Toggle Individual Result</span>
                                        </a>
                                    </small>
                                    <div class="large-12 columns tt-j-others-result-content">

                                        <div class="large-12 columns active">

                                            <div class="medium-3 columns">
                                                <div class="tt-pending-label radius secondary label">
                                                    <span class="icon-tt-clock"></span>
                                                    <span class="text">Pending</span>
                                                </div>
                                                <span class="tt-user-status online" id="" style="display:none">&#9679;</span>

                                                <div class="tt-nameOnly tg-en">
                                                  <!-- Just name -->
                                                  You
                                                  <!-- //Just name -->
                                                </div>

                                                <div class="tt-nameAndEmail tg-en hide">
                                                  <!-- Just name -->
                                                  <span title="You | email@email.com" class="hide-for-medium-down">You (email@email.com)</span>
                                                  <span title="You | email@email.com" class="show-for-medium-down">You...</span>
                                                  <!-- //Just name -->
                                                </div>

                                                <div class="tt-emailOnly tg-en hide">
                                                  <!-- with email -->
                                                  <span title="email@email.com" class="hide-for-medium-down">email@email.com</span>
                                                  <span title="email@email.com" class="show-for-medium-down">email@emai...</span>
                                                  <!-- //with email -->
                                                </div>



                                            </div>


                                            <div class="medium-7 columns text-center">
                                                <ul class="tt-equalizer-result">
                                                    <li class="eight lft  ownerResult-1"></li>
                                                    <li class="seven lft  ownerResult-1"></li>
                                                    <li class="six lft  ownerResult-1"></li>
                                                    <li class="five lft ownerResult-1"></li>
                                                    <li class="four lft ownerResult-1"></li>
                                                    <li class="three lft active ownerResult-1"></li>
                                                    <li class="two lft active ownerResult-1"></li>
                                                    <li class="one lft active ownerResult-1"></li>

                                                    <li class="zero mid active ownerResult-1"></li>

                                                    <li class="one rgt ownerResult-1"></li>
                                                    <li class="two rgt ownerResult-1"></li>
                                                    <li class="three rgt ownerResult-1"></li>
                                                    <li class="four rgt ownerResult-1"></li>
                                                    <li class="five rgt ownerResult-1"></li>
                                                    <li class="six rgt ownerResult-1"></li>
                                                    <li class="seven rgt ownerResult-1"></li>
                                                    <li class="eight rgt ownerResult-1"></li>
                                                </ul>

                                            </div>
                                            <div class="medium-2 columns text-right">
                                                <a id="1" class="reset-btn clrPairwiseResult" title=""><span class="icon-tt-close"></a>
                                                </span>
                                            </div>



                                        </div>
                                        <div class="large-12 columns tg-idr hide">
                                            <div class="medium-3 columns">

                                              <div class="tt-nameOnly tg-en">
                                                <!-- Just name -->
                                                <span title="JaneWithLongName" class="hide-for-medium-down">JaneWithLongName</span>
                                                <span title="JaneWithLongName" class="show-for-medium-down">JaneWithLong...</span>
                                                <!-- //Just name -->
                                              </div>

                                              <div class="tt-nameAndEmail tg-en hide">
                                                <!-- Just name -->
                                                <span title="Jane | email@email.com" class="hide-for-medium-down">Jane (email@email.com)</span>
                                                <span title="Jane | email@email.com" class="show-for-medium-down">Jane...</span>
                                                <!-- //Just name -->
                                              </div>

                                              <div class="tt-emailOnly tg-en hide">
                                                <!-- with email -->
                                                <span title="email@email.com" class="hide-for-medium-down">email@email.com</span>
                                                <span title="email@email.com" class="show-for-medium-down">email@emai...</span>
                                                <!-- //with email -->
                                              </div>






                                            </div>


                                            <div class="medium-7 columns text-center">
                                                <ul class="tt-equalizer-result">
                                                    <li class="eight lft ownerResult-2"></li>
                                                    <li class="seven lft ownerResult-2"></li>
                                                    <li class="six lft ownerResult-2"></li>
                                                    <li class="five lft ownerResult-2"></li>
                                                    <li class="four lft ownerResult-2"></li>
                                                    <li class="three lft ownerResult-2"></li>
                                                    <li class="two lft ownerResult-2"></li>
                                                    <li class="one lft ownerResult-2"></li>

                                                    <li class="zero mid active ownerResult-2"></li>

                                                    <li class="one rgt active ownerResult-2"></li>
                                                    <li class="two rgt active ownerResult-2"></li>
                                                    <li class="three rgt active ownerResult-2"></li>
                                                    <li class="four rgt active ownerResult-2"></li>
                                                    <li class="five rgt active ownerResult-2"></li>
                                                    <li class="six rgt ownerResult-2"></li>
                                                    <li class="seven rgt ownerResult-2"></li>
                                                    <li class="eight rgt ownerResult-2"></li>
                                                </ul>

                                            </div>
                                            <div class="medium-2 columns text-right">
                                                <a id="2" class="reset-btn clrPairwiseResult" title=""><span class="icon-tt-close"></span></a>
                                            </div>
                                        </div>
                                        <div class="large-12 columns tg-idr hide">
                                            <div class="medium-3 columns">

                                              <div class="tt-nameOnly tg-en">
                                                <!-- Just name -->
                                                <span title="JhonWithLongName" class="hide-for-medium-down">JhonWithLongName</span>
                                                <span title="JhonWithLongName" class="show-for-medium-down">JhonWithLong...</span>
                                                <!-- //Just name -->
                                              </div>

                                              <div class="tt-nameAndEmail tg-en hide">
                                                <!-- Just name -->
                                                <span title="Jhon | email@email.com" class="hide-for-medium-down">Jhon (email@email.com)</span>
                                                <span title="Jhon | email@email.com" class="show-for-medium-down">Jhon...</span>
                                                <!-- //Just name -->
                                              </div>

                                              <div class="tt-emailOnly tg-en hide">
                                                <!-- with email -->
                                                <span title="email@email.com" class="hide-for-medium-down">email@email.com</span>
                                                <span title="email@email.com" class="show-for-medium-down">email@emai...</span>
                                                <!-- //with email -->
                                              </div>



                                            </div>


                                            <div class="medium-7 columns text-center">
                                                <ul class="tt-equalizer-result">
                                                    <li class="eight lft active ownerResult-3"></li>
                                                    <li class="seven lft active ownerResult-3"></li>
                                                    <li class="six lft active ownerResult-3"></li>
                                                    <li class="five lft active ownerResult-3"></li>
                                                    <li class="four lft active ownerResult-3"></li>
                                                    <li class="three lft active ownerResult-3"></li>
                                                    <li class="two lft active ownerResult-3"></li>
                                                    <li class="one lft active ownerResult-3"></li>

                                                    <li class="zero mid active ownerResult-3"></li>

                                                    <li class="one rgt ownerResult-3"></li>
                                                    <li class="two rgt ownerResult-3"></li>
                                                    <li class="three rgt ownerResult-3"></li>
                                                    <li class="four rgt ownerResult-3"></li>
                                                    <li class="five rgt ownerResult-3"></li>
                                                    <li class="six rgt ownerResult-3"></li>
                                                    <li class="seven rgt ownerResult-3"></li>
                                                    <li class="eight rgt ownerResult-3"></li>
                                                </ul>

                                            </div>
                                            <div class="medium-2 columns text-right">
                                                <a id="3" class="reset-btn clrPairwiseResult" title=""><span class="icon-tt-close"></span></a>
                                            </div>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>


                    <% } %>
                <% } %>
            </div>

        </div>




        <% if (design != "multi"){ %>

        <div class="columns tt-j-result">
            <div class="columns tt-normal-head blue text-left">
                <span class="icon-tt-chart icon"></span>
                <span class="text">GROUP RESULT</span>
                <div class="right tt-accordion-head">
                    <small>
                        <a class="tt-toggler" id="1" data-toggler="tg" style="color: #fff;">
                            <span class="icon-tt-plus-square icon"></span>
                            <span class="text">Toggle Group Result</span>
                        </a>
                    </small>
                </div>
            </div>

            <div class="columns tg-1">

                <!-- owner result -->
                <div class="row collapse">
                    <div class="large-12 columns tt-j-result-title">
                        <div class="large-6 columns">Able to overcome obstacles</div>
                        <div class="large-6 columns text-center">

                            <ul class="tt-equalizer-result">
                                <li class="eight lft "></li>
                                <li class="seven lft "></li>
                                <li class="six lft "></li>
                                <li class="five lft "></li>
                                <li class="four lft "></li>
                                <li class="three lft "></li>
                                <li class="two lft "></li>
                                <li class="one lft "></li>

                                <li class="zero mid active"></li>

                                <li class="one rgt active"></li>
                                <li class="two rgt active"></li>
                                <li class="three rgt active"></li>
                                <li class="four rgt active"></li>
                                <li class="five rgt active"></li>
                                <li class="six rgt"></li>
                                <li class="seven rgt"></li>
                                <li class="eight rgt"></li>
                            </ul>
                        </div>
                        <% if (design == "numerical")
                           { %>
                        <div class="large-6 columns">
                            <strong>Geometric Variance: 0.24</strong>
                        </div>
                        <div class="large-6 columns text-center">
                            <div class="ratio-box">
                                <div class="large-12 columns text-center rb-wrap">

                                    <div class="large-8 columns large-centered">
                                        <input type="text" class="textbox left" value="4.300" readonly>:
                                    <input type="text" class="textbox right" value="1.000" readonly>
                                    </div>

                                </div>

                            </div>
                        </div>

                        <% } %>

                        <% if (design == "verbal")
                           { %>

                        <div class="large-12 columns">
                            <strong>Agreement Score:  66%</strong>
                        </div>

                        <% } %>
                    </div>


                    <div class="medium-6 columns tt-j-result-graph hide">

                        <div class="columns tt-j-result-comment"></div>
                    </div>
                </div>


            </div>
        </div>
<% } %>

     
    </div>



</div>
