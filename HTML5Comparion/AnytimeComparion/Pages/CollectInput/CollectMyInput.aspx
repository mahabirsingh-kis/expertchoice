<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CollectMyInput.aspx.cs" Inherits="AnytimeComparion.Pages.CollectInput.CollectMyInput" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-body tt-collect-input-wrap">
        <% if (Session["User"] != null || Session["UserType"] != null)
           { %>
        <div class="row collapse tt-2-cols">

            <div class="large-12 columns tt-left-col">
                <div class="large-12 columns">
                    <div class="columns tt-header-nav blue-header">
                        <ul class="breadcrumbs">
                            <li><a runat="server" href="~/collect-input"> Collect Input</a></li>
                            <li class="current">
                                <span>Collect My Input 
                                    <a href="#" class="help collect-input" data-dropdown="help-dd" aria-controls="moreActions-dd" aria-expanded="false"><span class="icon-tt-info-circle"></span></a>
                                </span>
                            </li>
                            <li class="tt-with-button">
                                <a class="button tt-button-primary" data-dropdown="output-dd" aria-controls="moreActions-dd" aria-expanded="false">Output</a>
                            </li>
                        </ul>
                    </div>


                        <!-- output-dd dropdown -->
                        <ul id="output-dd" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                            <li><a href="#">Lorem</a></li>
                            <li><a href="#">Ipsum</a></li>
                            <li><a href="#">Dolor</a></li>
                            <li><a href="#">Sit</a></li>
                            <li><a href="#">Amet</a></li>
                        </ul>
                        <!-- moreActions-dd dropdown -->

                    <!-- help dropdown -->
                    <div id="help-dd" class="f-dropdown" data-dropdown-content aria-hidden="true" tabindex="-1">
                        <p>
                            Lorem ipsum dolor sit amet. aman pulo ditr yu imusta putri
                        </p>
                    </div>
                    <!-- help-dd dropdown -->

                </div>


                <div class="row tt-meeting-setup-wrap">
                    <div class="large-12 columns">
                        <div class="row text-center">
                            <h3>Meeting Setup</h3>
                            <hr>
                        </div>
                        <div class="large-9 columns large-centered">
                            <div class="large-6 columns">
                                <div class="tt-header-nav blue-header columns">Options</div>
                                <div class="row collapse tt-meeting-type">
                                    <div class="large-12 columns">
                                      <div class="row ">
                                        <div class="large-5 medium-6 small-12 columns">
                                          Select Meeting Type:
                                        </div>
                                        <div class="large-7 medium-6 small-12 columns">
                                                <select>
                                                <option value="">Anytime Meeting</option>
                                                <option value="">Teamtime Meeting</option>
                                              </select>
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                <div class="row collapse tt-meeting-timeline">
                                    <div class="large-12 columns">
                                        Meeting Timeline:
                                        <div class="large-12 columns">
                                            <div class="large-5 medium-6 small-12 columns text-center">
                                              Begins
                                            </div>
                                            <div class="large-7 medium-6 small-12 columns">
                                              <input type="date">
                                            </div>
                                        </div>
                                        
                                            <div class="large-12 columns">
                                                <div class="large-5 medium-6 small-12 columns text-center">
                                                  Ends
                                                </div>
                                                <div class="large-7 medium-6 small-12 columns">
                                                  <input type="date">
                                                </div>
                                          </div>
                                      </div>
                                  </div>
                            </div>
                            <div class="large-6 columns">
                                <div class="tt-header-nav blue-header columns">Settings</div>
                                <div class="large-12 columns">
                                    <div class="row">
                                        <div class="large-12 columns">
                                            <label><input type="checkbox"> Display users with "View Only" access</label>
                                        </div>
                                        <div class="large-12 columns">
                                            <label><input type="checkbox"> Hide Judgments</label>
                                        </div>
                                        <div class="large-12 columns">
                                            <label><input type="checkbox"> Anonymous Mode</label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="row">
                    <div class="large-12 columns text-center">
                        <a class="button tt-button-primary tt-button-start-meeting">Start Meeting</a>
                    </div>
                </div>
            </div>
            

        </div>
        <% }
           else
           { %>
               <div class="row">
                <div class="large-12 columns text-center">
                    <div class="tt-center-height-wrap">
                        <div class="tt-center-content-wrap">

                            <%@ Register TagPrefix="includes" TagName="loginMessage" Src="~/Pages/includes/NeedToLogin.ascx" %>
                            <includes:loginMessage ID="needToLogin" runat="server" />

                        </div>
                    </div>
                </div>
            </div>
        <% } %>
    </div>
</asp:Content>
