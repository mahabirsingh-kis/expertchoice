<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AnytimeComparion._Default" EnableEventValidation="false" %>
<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div class="tt-body" ng-cloak ng-init="checkSignUpLink('<%=Session[Constants.Sess_SignUpMode] %>')">
        <div ng-if="!DeviceChecker.isMajorBrowser()" data-alert class="alert-box warning radius tt-alert msg-stat">           
            <h3>
                We've detected that your browser might not be supported by our application.
To have the best experience with Comparion, we recommend that you use major browsers such as Chrome, Internet Explorer, Firefox and Safari.
                <a href="#" class="close">&times;</a>          
            </h3>        
        </div>   
        
<% if(string.Compare(Request.Url.LocalPath,"/default.aspx")==0 || string.Compare(Request.Url.LocalPath,"/")==0){  %>
    <div class="hide">you are on home page</div>
 <% } %>
         <% if (Session["User"] != null || Session["UserType"] != null){%>
             <div class="small-10 medium-5 large-3 columns text-center small-centered" 
                style="padding: 20px 0 0 0;
                background: #f8f8f8;
                border-radius: 5px;
                border: 2px solid #eee;
                margin-top: 40px;">
                <p style="margin-bottom:0px">Logged in as:</p>
                <p style="font-weight:bold; color:#539ddd"><span runat="server" id="UserName"></span></p>
                 <p><input type="button" class="LogoutButtons button radius normal-green-bg" value="Logout" /></p>
            </div>
         <% } %>
        <div class="row tt-full-height" style="display: table;">
            <div class='tt-full-height large-5 large-centered medium-8 medium-centered columns tt-homepage <% if (Session["User"] == null || Session["UserType"] == null){ %>tt-homepage-content<% } %>' style="display: table-cell; vertical-align: middle;">
                <!--comment-->
                <% if (Session["User"] != null || Session["UserType"] != null){ %>

                <% } if (Session["UserType"] == null && Session["User"] == null){ %>

                <div class="large-12 columns text-center">
                     <h1 class="tt-header normal-bold">Comparion&reg;<br />
                         Collaborative Decision Making Solution</h1>
                   
                    <p class="tt-sub-head">Ensure that you set objectives that align with strategic goals. Make better decisions with fewer wasted meetings and less gridlock. Improve communication, transparency leading to improved alignment and implementation.</p>
                    
                    <div id="hashLinkMessage" class="text-center" style="font-size: 14px; margin-bottom: 15px;" ng-init="getLastHash();"></div>

                    <h1 class="tt-header home-login normal-green">Login to Expert Choice Comparion&reg;</h1>
                    <p class="hide tt-sub-head">
                        Comparion is a collaborative decision tool on the web where a team can come together to evaluate factors influencing a decision, and alternative decision choices.
                    </p>
                    <%if (Request.QueryString["pageError"] != null)
                      {
                            var App = (ExpertChoice.Data.clsComparionCore)HttpContext.Current.Session["App"];
                            var pageError = Request.QueryString["pageError"];
                             var debug = Request.QueryString["debug"];
                            %>
                    <div class="large-12 columns ">          
                        <div data-alert class="alert-box warning radius tt-alert msg-stat">           
                                <%switch (pageError)
                                    {
                                        case "inviteUserExist":
                                            Response.Write("Sign up failed. User already exist.");
                                            break;
                                        case "inviteNoAccess":
                                            var message = Session["UserSpecificHashErrorMessage"] == null ? string.Empty : (string)Session["UserSpecificHashErrorMessage"];
                                            if (string.IsNullOrEmpty(message))
                                            {
                                                var passCode = Request.QueryString["passCode"];
                                                var project = App.DBProjectByPasscode(passCode);
                                                message = string.Format(TeamTimeClass.ResString("msgAuthProjectDisabled"), project.ProjectName);
                                            }
                                            
                                            Response.Write(message);
                                            break;
                                        case "invalidLink":
                                            Response.Write("Sorry but we don\'t recognized your evaluation link. Please check if you have the correct link or contact your Project Manager");
                                            //Response.Write(debug);
                                            break;
                                    }%>
                            <a href="#" class="close">&times;</a>   
                        </div>      
                    </div>
                    <%} %>

                </div>
                 <div class="tt-modal-content large-12 columns">
                        <section class="large-12 columns">
                            <div class="large-12 columns">
                                <input type="email" placeholder="Email" class="tt-input-form" id="LoginEmail" />
                                <small id="EmailError" class="error">Invalid email.</small>
                                <small id="EmailExistError" class="error">Incorrect email. Please check your credentials.</small>
                            </div>
                            <div class="large-12 columns">
                                <input type="password" placeholder="Password" class="tt-input-form" id="LoginPassword" />
                                <a href="#" style="font-size: 11px;" ng-click="showPasswordResetModal('login')">Password Help?</a>
                                <small id="PasswordError" class="error">Password is required.</small>
                                <small id="IncorrectPasswordError" class="error">Incorrect password. Please check your credentials.</small>
                            </div>
                            <div class="large-12 columns">
                                <div class="row collapse">
                                    <div class="small-10 columns">
                                        <input id="AccessCodeTxt" type="text" placeholder="Access Code" class="tt-input-form">
                                        <small class="error">Missing Access Code</small>
                                    </div>
                                    <div class="small-2 columns text-right">
                                        <span class="right inline span-inline"><small>* Optional</small></span>
                                    </div>
                                </div>
                            </div>

                            <div class="tt-remember-forgot-wrap columns">
                                <div class="large-7 medium-12 columns">
                                    <div class="row collapse">
                                        <!--<div class="small-1 columns">
                                            <input type="checkbox" id="SrembMeLogin" name="SrembMeLogin">
                                        </div>-->
                                        <div class="row collapse">
                                            <div class="small-12 columns tt-remember">
<!--                                                        <span class="suffix">-->
                                                    <label class=""><input type="checkbox" id="SrembMeLogin" > <span>Remember Me</span></label>
<!--                                                        </span>-->
                                            </div>
                                        </div>
                                    </div>
                                </div>
                               <div class="large-5 medium-12 columns text-right tt-forgot-password hide">
                                    <a href="#">Forgot Password?</a>
                                </div>
                            </div>
                            <div class="large-12 columns">
                                <hr>
                                <a id="LoginBtn" href="#" class="button tiny right clogin success normal-green-bg radius">Login</a>
                            </div>
                            <br />
                            <div runat="server" id="LoginStatus" class="tt-login-form-status"></div>
                        </section>
                    </div>
                <div runat="server" id="Div2" style="clear: both;"></div>

              <%--  <div data-equalizer class="columns tt-home-not-logged">
                    <div class="large-8 columns tt-join-meeting-wrap" data-equalizer-watch>

                        <section class="columns">
                            <span class="icon-tt-users tt-icon-60"></span>
                            <br>
                            <h3>Join Meeting</h3>
                        </section>

                        <section class="large-12 columns">
                            <p>Please enter the ID of the meeting in which you would like to participate</p>
                        </section>

                        <section class=" large-8 large-centered columns">
                            
                            <input type="text" name="meetingId" class="" placeholder="Meeting ID" id="MeetingID">
                            <div class="jm-error"></div>
                          

                            <a href="#"  class="button tiny tt-button-icond-right tt-button-cta tt-joinMeeting-btn">
                                <span class="text">Join Meeting</span>
                                <span class="icon-tt-chevron-right icon"></span>
                            </a>

                        </section>

                        <footer class="columns hidden-for-large-up">
                            If you're the organizer, please <a href="#" class="button tiny tt-button-cta login-btn " data-reveal-id="loginToMeeting">login.</a>
                        </footer>

                        <section class="columns hide-for-medium-down">
                            If you're the organizer, please <a href="#" class="button tiny tt-button-cta login-btn " data-reveal-id="loginToMeeting">login.</a>
                        </section>

                    </div>


                    <div class="large-4 columns" data-equalizer-watch>
                        <div class="columns tt-video-wrap">
                            <section class="columns">
                                <span class="icon-tt-film tt-icon-60"></span>
                                <br>
                                <p>New to Expert Choice Comparion's Team Time Meeting? Learn from our Video Tutorials</p>
                            </section>
                            <div class="columns">
                                <a href="#" class="button tiny tt-button-cta"><span class="text">See more tutorials</span></a>
                            </div>
                        </div>

                        <div class="columns tt-social-wrap">
                            <h3>Get Connected with Us</h3>
                            <p>Keep in touch with us via social media</p>
                            <a href="#" class="socialLinks-homepage"><span class="icon-tt-facebook-square"></span></a>
                            <a href="#" class="socialLinks-homepage"><span class="icon-tt-twitter-square"></span></a>
                            <a href="#" class="socialLinks-homepage"><span class="icon-tt-google-plus-square"></span></a>
                            <a href="#" class="socialLinks-homepage"><span class="icon-tt-linkedin-square"></span></a>
                            <a href="#" class="socialLinks-homepage"><span class="icon-tt-youtube-square"></span></a>
                        </div>
                    </div>
                </div>--%>
                <% } %>
               <%-- <div id="joinMeetingEval" data-reveal class="reveal-modal small tt-modal-wrap fixed-modal" aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                    <h2 id="modalTitle" class="tt-modal-header blue-header">Join Meeting</h2>

                    <div class="tt-modal-content large-12 columns tt-center-height-wrap">

                        <section class="large-8 columns large-centered  tt-center-content-wrap">
                            <div class="columns hide error-msg" id="join_meeting_auth_error">
                                
                            </div>
                            <div class="columns hide error-msg" id="server_error_msg">
                                <div class="large-12 columns">          
                                    <div data-alert class="alert-box alert radius tt-alert msg-stat">                   <h3>Somethign went wrong. Please try again.
                                            <a href="#" class="close">&times;</a>          
                                       </h3>      
                                    </div>      
                                </div>
                            </div>
                            <div class="columns">
                                <input class="meetingInput tt-input-form" type="text" placeholder="Name" id="meeting_name">
                                <small id="MeetingNameError" class="error">Your name is important .
                                </small>
                            </div>
                            <div class="columns">
                                <input class="meetingInput tt-input-form" type="text" placeholder="Email" id="meeting_email">
                                <small id="MeetingEmailError" class="error">Your email is important .
                                </small>
                            </div>
                            <div class="tt-remember-forgot-wrap columns">
                                <div class="small-1 columns">
                                    <input type="checkbox" id="rembMeLogin">
                                </div>
                                <div class="small-11 columns tt-remember">
                                    <span class="suffix">
                                        <label for="rembMeLogin" class="">Remember Me</label></span>
                                </div>

                            </div>
                            <div class="large-12 columns">
                                <hr>
                                <button type="submit" class="button tiny right clogin success" id="joinmeetingbtn">Join</button>
                            </div>
                            <div runat="server" id="TTLoginStatus" class="tt-login-form-status"></div>
                        </section>
                    </div>
                </div>--%>
            </div>
        </div>
    </div>
    <script>
        //HIT ENTER SUBMIT
        $(document).on('keypress', '.meetingInput', function (e) {
            if (e.which == 13) {
                $('#joinmeetingbtn').click();
                //console.log('enter clicked');
                return false;
            }
        });

        //HIT ENTER SUBMIT
        $(document).on('keypress', '.tt-input-form', function (e) {
            if (e.which == 13) {
                if (this.id == "ResetEmail") {
                    $("#ResetPasswordBtn").click();
                } else if (this.id == "LoginEmail" || this.id == "LoginPassword" || this.id == "AccessCodeTxt") {
                    $("#LoginBtn").click();
                }
                
                //console.log('enter clicked');
                return false;
            }
        });

        $(document).ready(function () {
            <%  if (Request.Cookies["usernam"] != null && Request.Cookies["passwor"] != null)
                { %>
                var a = "<%=Request.Cookies["usernam"].Value  %>";
                var b = "<% =Request.Cookies["passwor"].Value  %>";
                $("#LoginEmail").val(a);
                $("#LoginPassword").val(b);
                $("#SrembMeLogin").prop('checked', true);

                <% }
        bool fDB_OK = true;
        if (Session["App"] != null)
        {
            var App = (ExpertChoice.Data.clsComparionCore)HttpContext.Current.Session["App"];
            bool fMasterExists = App.isCanvasMasterDBValid;
            bool fProjectsExists = App.isCanvasProjectsDBValid;
            fDB_OK = (fMasterExists && fProjectsExists);
        }

         if (!fDB_OK) { %>
                $("#LoginBtn").addClass("disabled");
                $("#joinmeetingbtn").addClass("disabled");
                $("#LoginBtn, #joinmeetingbtn").click(function (event) {
                    event.preventDefault();
                    return false;
                });
             <% } %>
        }); 
    </script>



</asp:Content>
