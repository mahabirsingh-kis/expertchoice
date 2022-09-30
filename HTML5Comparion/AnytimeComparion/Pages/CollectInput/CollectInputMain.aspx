<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CollectInputMain.aspx.cs" Inherits="AnytimeComparion.Pages.CollectInput.CollectInputMain" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
        <link href="/Content/stylesheets/fullcalendar.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div ng-controller="CollectInputController" ng-cloak >
        <div class="tt-body tt-collect-input-wrap" >
<%--            <% if (Session["User"] != null || Session["UserType"] != null){ %>--%>
                <%-- <div>
                    <span><a ng-click="changePage(collectInputPages.defaultPage)">Collect Input</a></span> > <span ng-if="currentPage != collectInputPages.defaultPage">{{currentPage.name}}</span>
                </div>
                <div>
                    <div class="large-12 large-centered columns text-centered" ng-include="currentPage.url"></div>
                </div> --%>

                <div class="row collapse tt-2-cols">

                <div class="large-12 columns tt-left-col">
                    <div class="large-12 columns">
                        <div class="columns tt-header-nav blue-header">
                            <ul class="breadcrumbs">
                                <li><a ng-click="changePage(collectInputPages.defaultPage)"> Collect Input</a> </li>
                                <li class="current" ng-if="currentPage != collectInputPages.defaultPage">
                                    <span >{{currentPage.name}}
                                        <a href="#" class="help collect-input" data-dropdown="help-dd" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                    </span>
                                </li>
                                <li class="tt-with-button">
                                    <a class="button tt-button-primary tt-dropdown" data-dropdown="output-dd" aria-controls="moreActions-dd" aria-expanded="false">
                                        <span class="text">Output </span>
                                        <span class="icon-tt-chevron-down drop-arrow"></span>
                                    </a>

                                </li>
                            </ul>
                        </div>


                        <!-- output-dd dropdown -->
                        <ul id="output-dd" class="f-dropdown" data-dropdown-content="" aria-hidden="true" tabindex="-1">
                            <li><a href="#">Lorem</a></li>
                            <li><a href="#">Ipsum</a></li>
                            <li><a href="#">Dolor</a></li>
                            <li><a href="#">Sit</a></li>
                            <li><a href="#">Amet</a></li>
                        </ul>
                        <!-- moreActions-dd dropdown -->

                        <!-- help dropdown -->
                        <div id="help-dd" class="f-dropdown" data-dropdown-content="" aria-hidden="true" tabindex="-1">
                            <p>
                                Lorem ipsum dolor sit amet. aman pulo ditr yu imusta putri
                            </p>
                        </div>
                        <!-- help-dd dropdown -->

                    </div>
                    <div class="large-12 columns tt-tabs-wrap-synthesize-result">
                                <div class="large-12 large-centered columns text-centered" ng-include="currentPage.url"></div>
                    </div>
                 </div> <!-- tt-left-col-->
<%--                <% } else { %>--%>
<%--                    <div class="row">--%>
<%--                        <div class="large-12 columns text-center">--%>
<%----%>
<%--                            <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>--%>
<%--                                <includes:loginMessage ID="needToLogin" runat="server" />--%>
<%----%>
<%--                        </div>--%>
<%--                    </div>--%>
<%--                </div>--%>
<%--                <% } %>--%>

<!-- scales modal wrap -->
<div id="newScales" class="reveal-modal medium tt-modal-wrap collect-input-modal" data-reveal aria-labelledby="addPart" aria-hidden="true" role="dialog">
    <a class="close-reveal-modal" aria-label="Close">&#215;</a>
    <h2 id="addPart" class="tt-modal-header blue-header">Add New Scales <a  href="#" class=""><span  class="icon-tt-question-circle "></span></a></h2>

    <div class="tt-modal-content collect-input large-12 columns">
        <div class="large-12 columns">
            <ul class="tabs" data-tab>
              <li class="tab-title active"><a href="#create-new-scales">Create New Scales</a></li>
              <li class="tab-title"><a href="#copy-modify-new-scales">Copy and Modify an existing scale</a></li>

            </ul>
            <div class="tabs-content">

                <!-- #create-new-scales -->
              <div class="content active" id="create-new-scales">

                  <div class="row collapse">
                    <div class="large-6 columns">
                        <div class="large-6 columns">
                            Measurement Methods:
                        </div>
                        <div class="large-6 columns">
                            <select>
                                <option>Rating Scale</option>
                                <option>Others1</option>
                                <option>Others2</option>
                            </select>
                        </div>
                    </div>


                  </div>

                  <div class="row">
                    <div class="large-2 columns">
                        Scale Name:
                      </div>
                    <div class="large-10 columns">
                        <input type="text" placeholder="Ex: lorem ipsum">
                    </div>
                  </div>
                  <div class="row">
                    <div class="large-2 columns">
                        Description:
                      </div>
                    <div class="large-10 columns">
                        <textarea rows="5" placeholder="Lorem ipsum dolor sit amet"></textarea>
                    </div>
                  </div>
              </div>
                <!-- //#create-new-scales -->

                <!-- #copy-modify-new-scales -->
              <div class="content" id="copy-modify-new-scales">
                <div class="row collapse">
                    <div class="large-6 columns">
                        <div class="large-6 columns">
                            Measurement Methods:
                        </div>
                        <div class="large-6 columns">
                            <select>
                                <option>Rating Scale</option>
                                <option>Others1</option>
                                <option>Others2</option>
                            </select>
                        </div>
                    </div>
                      <div class="large-6 columns">
                        <div class="large-6 columns">
                            Copy from Measurement Scales:
                        </div>
                        <div class="large-6 columns">
                            <select>
                                <option>Default Rating Scale</option>
                                <option>Others1</option>
                                <option>Others2</option>
                            </select>
                        </div>
                    </div>

                  </div>

                  <div class="row">
                    <div class="large-2 columns">
                        Scale Name:
                      </div>
                    <div class="large-10 columns">
                        <input type="text" placeholder="Ex: lorem ipsum">
                    </div>
                  </div>
                  <div class="row">
                    <div class="large-2 columns">
                        Description:
                      </div>
                    <div class="large-10 columns">
                        <textarea rows="5" placeholder="Lorem ipsum dolor sit amet"></textarea>
                    </div>
                  </div>
              </div>
                <!-- // #copy-modify-new-scales -->
            </div>
        </div>

        <div class="large-12 columns">
            <table class="">
                <thead>
                    <tr>
                        <td>Intensity Name</td>
                        <td>Priority</td>
                        <td>Description</td>
                        <td></td>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><input type="text" placeholder="Enter Intensity Name Here"></td>
                        <td><input type="text" placeholder="0.000"></td>
                        <td><input type="text" placeholder="Enter Description"></td>
                        <td><a class=""><span class="icon-tt-times-circle"></span></a></td>
                    </tr>

                    <tr>
                        <td><input type="text" placeholder="Enter Intensity Name Here"></td>
                        <td><input type="text" placeholder="0.000"></td>
                        <td><input type="text" placeholder="Enter Description"></td>
                        <td><a class=""><span class="icon-tt-times-circle"></span></a></td>
                    </tr>

                </tbody>
            </table>
        </div>
        <div class="tt-modal-footer-links-wrap">
            <div class="medium-9 columns">
                <div class="medium-4 columns large-text-left small-text-center">
                    <a href="#"><span class="icon-tt-plus"></span> <span class="text">Add new rating intensity</span></a>
                </div>
                <div class="medium-4 columns large-text-left small-text-center">
                    <a href="#"><span class="icon-tt-copy"></span> <span class="text">Copy to clipboard</span></a>
                </div>
                <div class="medium-4 columns large-text-left small-text-center">
                    <a href="#"><span class="icon-tt-clipboard"></span> <span class="text">Paste from clipboard</span></a>
                </div>
            </div>
            <div class="medium-3 columns">
                <div class="text-right">
                    <button type="submit" class="button tiny tt-button-primary tt-button-primary right">
                        <span class="text">Create</span>
                    </button>
                    <!--<button data-reveal-id="selectProjMode" type="submit" class="button tiny tt-button-primary right">
                        <span class="text">Back</span>
                    </button>-->
                    <button type="button" aria-label="Close" class="close-reveal-modal button tiny tt-button-primary tt-button-transparent right custom-close-modal">
                        <span class="text">Cancel</span>
                    </button>
                </div>
            </div>
        </div>
    </div>
</div>
<!-- // scales modal wrap -->
    
<!-- setSched modal -->
    <div id="setSched" class="reveal-modal small tt-modal-wrap fixed-modal collect-input medium" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        <h2 id="modalTitle" class="tt-modal-header blue-header">Set Settings <a  href="#" class=""><span  class="icon-tt-question-circle "></span></a></h2>
        <div class="tt-modal-content">
            <div class="large-12 columns">
                <div class="row collapse">
                    <div class="medium-6 columns">
                        <div class="row">
                            <div class="small-12 columns">
                              <div class="row collapse">
                                <div class="medium-4 small-3 columns">
                                  <label for="right-label" class="right inline text-center">Start Date:</label>
                                </div>
                                <div class="medium-8 small-9 columns">
                                    <div class="small-10 columns">
                                      <input type="date" id="right-label"> 
                                    </div>
                                    <div class="small-2 columns">
                                        <label for="right-label" class="right inline  text-center"><span class="icon-tt-calendar icon"></span></label>
                                    </div>
                                </div>
                              </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="small-12 columns">
                              <div class="row collapse">
                                <div class="medium-4 small-3 columns">
                                  <label for="right-label" class="right inline text-center">Start Time:</label>
                                </div>
                                <div class="medium-8 small-9 columns">
                                    <div class="small-10 columns">
                                      <input type="text" id="right-label" placeholder="Inline Text Input" class=""> 
                                    </div>
                                    <div class="small-2 columns">
                                        <label for="right-label" class="right inline  text-center"><span class="icon-tt-clock icon"></span></label>
                                    </div>
                                </div>
                              </div>
                            </div>
                        </div>
                    </div>
                    <div class="medium-6 columns">
                        <div class="row">
                            <div class="small-12 columns">
                              <div class="row collapse">
                                <div class="medium-4 small-3 columns">
                                  <label for="right-label" class="right inline text-center" >End Date:</label>
                                </div>
                                <div class="medium-8 small-9 columns">
                                    <div class="small-10 columns">
                                      <input type="date" id="right-label"> 
                                    </div>
                                    <div class="small-2 columns">
                                        <label for="right-label" class="right inline  text-center"><span class="icon-tt-calendar icon"></span></label>
                                    </div>
                                </div>
                              </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="small-12 columns">
                              <div class="row collapse">
                                <div class="medium-4 small-3 columns">
                                  <label for="right-label" class="right inline text-center">End Time:</label>
                                </div>
                                <div class="medium-8 small-9 columns">
                                    <div class="small-10 columns">
                                      <input type="text" id="right-label" placeholder="Inline Text Input" class=""> 
                                    </div>
                                    <div class="small-2 columns">
                                        <label for="right-label" class="right inline  text-center"><span class="icon-tt-clock icon"></span></label>
                                    </div>
                                </div>
                              </div>
                            </div>
                        </div>
                    </div>
                </div>
                
            </div>
            
            <div class="tt-modal-footer-links-wrap"><hr>
            
            <div class="medium-3 columns right">
                <div class="text-right">
                    <button type="submit" class="button tiny tt-button-primary tt-button-primary right">
                        <span class="text">Create</span>
                    </button>
                    <!--<button data-reveal-id="selectProjMode" type="submit" class="button tiny tt-button-primary right">
                        <span class="text">Back</span>
                    </button>-->
                    <button type="button" aria-label="Close" class="close-reveal-modal button tiny tt-button-primary tt-button-transparent right custom-close-modal">
                        <span class="text">Cancel</span>
                    </button>
                </div>
            </div>
        </div>
        </div>
    </div>
<!-- //setSched modal -->
    
        </div><!-- END tt-body-->
    </div>
</div>
    <script src="../../Scripts/moment.min.js"></script>
    <script src="../../Scripts/fullcalendar.js"></script>
    <script src="/Scripts/Controllers/CollectInputController.js"></script>
</asp:Content>


