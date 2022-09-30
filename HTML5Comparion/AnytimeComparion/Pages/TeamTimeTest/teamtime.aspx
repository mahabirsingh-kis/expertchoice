<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="teamtime.aspx.cs" Debug="true" Inherits="AnytimeComparion.test.teamtime" %>

<%@ Import Namespace="AnytimeComparion.Pages.external_classes" %>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%@ Reference Control="PairWiseComparison.ascx" %>
    <%@ Reference Control="DirectUser.ascx" %>

    <style>
         .mce-notification-warning{
            display:none !important;
        }
    </style>
    
    <%var meeting_owner = App.ActiveProject.MeetingOwner;
        if ((App.ActiveProject != null && App.ActiveProject.isTeamTime) || TeamTimeClass.isTeamTimeOwner)
        { %>
    <div id="TeamTimeDiv" class="tt-body" ng-controller="TeamTimeController">
        <div class="row collapse tt-2-cols" ng-model="teamtime_body" ng-init="teamtime_body = false" ng-show="teamtime_body" ng-cloak>
            <div class="large-12 columns tt-left-col">

<!--                <div class="tt-stick-that-footer-wrap">-->
<!--                    <div class="tt-stick-that-footer-content">-->
                <div class="columns tt-auto-resize-content">            

                    <%@ Register Src="~/Pages/TeamTimeTest/InformationPage.ascx" TagPrefix="includes" TagName="InformationPage" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/ThankYou.ascx" TagPrefix="includes" TagName="ThankYou" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/WaitingPage.ascx" TagPrefix="includes" TagName="WaitingPage" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/PairWiseComparison.ascx" TagPrefix="includes" TagName="PairWiseComparison" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/RatingScale.ascx" TagPrefix="includes" TagName="RatingScale" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/DirectUser.ascx" TagPrefix="includes" TagName="DirectUser" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/UtilityCurve.ascx" TagPrefix="includes" TagName="UtilityCurve" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/LocalResults.ascx" TagPrefix="includes" TagName="LocalResults" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/GlobalResults.ascx" TagPrefix="includes" TagName="GlobalResults" %>
                    <%@ Register Src="~/Pages/TeamTimeTest/StepFunction.ascx" TagPrefix="includes" TagName="StepFunction" %>
                    <%--no judgment page--%>
                    <div ng-if="current_action_type == 'pairwise'">
                        <includes:PairWiseComparison runat="server" ID="PairWiseComparison" />
                    </div>
                    <div ng-if="current_action_type == 'rating' && !restricted" > 
                        <includes:RatingScale runat="server" ID="RatingScale" />
                    </div>
                    <div ng-if="current_action_type == 'direct' && !restricted">
                        <includes:DirectUser runat="server" ID="DirectUser" />
                    </div>
                    <div ng-if="current_action_type == 'step' && !restricted">
                        <includes:StepFunction runat="server" ID="StepFunction"/>
                    </div>  
                    <div ng-if="current_action_type == 'ruc' && !restricted">
                        <includes:UtilityCurve runat="server" ID="UtilityCurve"/>
                    </div>
                    <div ng-if="current_action_type == 'waiting'">
                        <includes:WaitingPage runat="server" ID="WaitingPage" />
                    </div>  

                    <div ng-if="current_action_type == 'localresults'">
                        <includes:LocalResults runat="server" ID="LocalResults" />
                    </div>
                    <div ng-if="current_action_type == 'globalresults'">
                        <includes:GlobalResults runat="server" ID="GlobalResults" />
                    </div>
                    <div ng-if="current_action_type == 'message' && output.current_step == 1" >
                        <includes:InformationPage runat="server" ID="InformationPage" />
                    </div>
                    <div ng-if="current_action_type == 'message' && output.current_step == steps_list.length" >
                        <includes:ThankYou runat="server" ID="ThankYou" />
                    </div>
    
                    <div class="columns" ng-if="restricted && current_action_type != 'waiting'">
                        <div class="row">
                            <div class="large-12 columns text-center">
                                <div class="tt-stick-that-footer-content large-12 columns thankyouText">
                                    <a href="#">
                                        We are sorry, but your role setting does not allow you to view this step.
Please wait while the meeting organizer moves to the next step.
                                    </a>
                                </div>
                            </div>
                        </div>
                        
                    </div>
               
                <% 
          HttpContext context = HttpContext.Current;
                        
                %>

                    
                   

                <%@ Register TagPrefix="includes" TagName="temporaryList" Src="~/Pages/TeamTimeTest/temporaryList.ascx" %>
                    
                </div>        
<!--                    </div>-->
<!--                <div class="row detect-scroll-wrap">Scroll down to see more details</div>-->
<!--                <div class="row text-center detect-scroll-div-bottom ds-div-bottom hide-for-medium-down"></div>-->
                        
                <!-- deskop footer here -->
                <div class="tt-judgements-footer-nav desktop visible-for-large-up" >
                    <div class="row collapse">
                        <div class="large-5 medium-12 columns">
                            <div class="large-12 medium-6 columns">
                                <!-- Start Evaluation Progress Modal -->
                                <div id="othersPrecentageModal" class="tt-modal-wrap fixed-modal reveal-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog" >                                    
                                    <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                                    <h2 id="modalTitle" class="tt-modal-header blue-header">User's Evaluation Progress</h2>
                                    <div class="tt-modal-content large-12 columns othersPrecentageModal" >
                                        <div class="row collapse" ng-repeat="user_eval in users_evaluation track by $index">                                            
                                            <div class="small-12 medium-5 columns">
                                                <span ng-if="TTOptions.isAnonymous == 0"> 
                                                 {{ user_eval[0] }}
                                                </span>
                                                <span ng-if="TTOptions.isAnonymous == 1"> 
                                                     {{current_user[0] != user_eval[0].toLowerCase() ? 'Person ' + ($index) : current_user[0]}}
                                                </span>
                                            </div>
                                            <div class="small-12 medium-7 columns">
                                                <div class="row collapse">
                                                    <div class="small-6 medium-3 large-2 columns">
                                                        {{ user_eval[1] }}/{{ user_eval[2] }}
                                                    </div>
                                                    <div class="small-6 small-push-0 medium-3 medium-push-6 large-2 large-push-8 columns text-right">
                                                        {{(user_eval[1]/user_eval[2])*100 | number:0}}%
                                                    </div>
                                                    <div class="small-12 medium-6 medium-pull-3 large-8 large-pull-2 columns">
                                                        <div class="progress progress-wrap">
                                                            <span class="meter" ng-style="{'width': '{{(user_eval[1]/user_eval[2])*100}}%'}"></span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <!-- End Evaluation Progress Modal -->
                            </div>
                        </div>
                        <div id="timeDetails"  class="reveal-modal tt-modal-wrap tiny" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
                            <h2 class="tt-modal-header blue-header">Time Details</h2>
                            <div class="tt-modal-content large-10 large-centered columns">
                                    <table style="width: 100%;">
                                    <%  if (context.Session["ttOwner"] != null && (bool)context.Session["ttOwner"])
                                            { %>
                                        <tr>
                                            <td>
                                        <span class="last_judgment_div" class="hide last_judgment_div">Time since last judgement:</span>
                                            </td>
                                            <td><span class="last_judgment_time">00:00</span></td>
                                        </tr>

                                        <%  } %>
                                    <tr>
                                        <td>Last Request:</td><td>  <span id="last_request_time" class="time last_request_time"></span></td>
                                    </tr>
                                        <tr>
                                            <td>  Current Time:</td><td> <span id="current_time" class="time current_time"></span></td>
                                    </tr>
                                        <tr>
                                            <td> Failed Requests:</td><td> <span id="failed_requests" class="time failed_requests">0</span></td>
                                    </tr>
                                </table> 
                            </div>
                        </div>


                        <div class="large-12 columns pipe-footer">
                            <div class="medium-6 columns">

                                <%  if (context.Session["ttOwner"] != null && (bool)context.Session["ttOwner"])
                                    { %>
                                        <%--<div class="columns pipe-settings">
                                            <label class="medium-4 columns">
                                                <input class="anonymous-checkbox"  type="checkbox" name="">
                                                <span>Anonymous Mode&nbsp;&nbsp;&nbsp;</span>
                                            </label>
                                            <label class="resize-checkbox-label medium-4 columns">
                                                <input  class="resize-checkbox" type="checkbox" name="" <% 
                                                        if (PmOverwriteInfoDocs == 1)
                                                        {
                                                            Response.Write("checked");
                                                        }
                                                        %>>
                                                <span>Resize Infodocs To All</span>

                                            </label>
                                            <label class="infodocs-checkbox-label medium-4 columns">
                                                <input  class="infodocs-checkbox" type="checkbox" name="" <% 
                                                    if (PmHideAndShowInfoDocs == 1)
                                                        {
                                                            Response.Write("checked");
                                                        }
                                                        %>
                                                >
                                                <span>Hide/Show Infodocs To All</span>

                                            </label>
                                        </div>
                                        <div class="columns hide-pie-chart hide">
                                            <label >
                                                <input class="hide-pie-checkbox"  type="checkbox" name="">
                                                <span>Hide Pie Chart</span>
                                            </label>
                                        </div>--%>
                                <% } %>
                                <%else
                                    { %>
<%--                                <div class="left tt-eval-steps tt-s-btn">
                                    <span><strong>Steps:</strong> <span >{{STEPS_current_step}}</span> of {{STEPS_total_steps}} </span><span><strong>Evaluated 2:</strong> <span class="evaluatedj">{{output.evaluation[1]}}/{{output.evaluation[0]}}%></span></span>
                                </div>
                                <div  class="overall tt-c100 p{{output.evaluation[2]}} size-30">
                                    <span class="overallj">{{output.evaluation[2] | number:0}}%</span>
                                    <div class="slice">
                                        <div class="bar"></div>
                                        <div class="fill"></div>
                                    </div>
                                </div>--%>
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

                                <ul class="tt-site-map-wrap"  >
                                    <li ng-repeat="hierarchy in hierarchies"  ng-include="'menu_sublevel.html'">
                                        </li>
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
                                <ul class="hide">
                                    <li class="notice"><a runat="server" href="~/Judgement/direct-comparison">#1: Direct Comparison (collect input)</a></li>
                                </ul>
                                <includes:temporaryList ID="Prioritities" runat="server" />

                            </div>
                        </div>
                        <!-- // modal wrap -->

                        <!-- help modal wrap -->
                            <div id="tt-help-modal" class="reveal-modal small tt-modal-wrap fixed-modal teamtime-steps-help-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
                             <a class='close-reveal-modal second-modal-btn'>&#215;</a>
                            <h2 id="modalTitle" class="tt-modal-header blue-header">Judgment Links Info</h2>
                            <div class="tt-modal-content large-12 columns">

                                <div class="row">
                                    <div class="large-12 columns">
                                        <p>The current step is displayed with an <span class="label orange">orange</span> background. The step numbers are colored as follows:</p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="small-12 columns">
                                        <div class="row collapse">
                                            <div class="small-3 medium-2 columns normal-red">Red </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-8 medium-9 columns">Judgment has not yet been made</div>
                                            <div class="small-3 medium-2 columns normal-blue">Blue </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-8 medium-9 columns">Results or information steps</div>
                                            <div class="small-3 medium-2 columns">Black </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-8 medium-9 columns">Judgment has been made</div>
                                            <div class="small-3 medium-2 columns lite-orange">Orange </div>
                                            <div class="small-1 medium-1 columns text-center">-</div>
                                            <div class="small-8 medium-9 columns">Missing judgments</div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                    </div>
                        <!-- //help modal wrap -->

                    </div>
                </div>
                <!-- //footer-->

                <%-- Mobile Footer --%>

                <%-- // Mobile Footer --%>
            </div>

            <div class="large-3 columns tt-comments-wrap hide tt-right-col">

                <!-- sidebar-->
                <%@ Register TagPrefix="MainPages" TagName="SidebarRight" Src="~/Pages/sidebar.ascx" %>

                <MainPages:SidebarRight ID="SidebarRight" runat="server" />

            </div>

        </div>
        
   </div>
                                            
                                            
    <%-- GLobal Modal for Nodes --%>
    <div id="GlobalNodesModal"  class="reveal-modal tt-modal-wrap  tt-edit-content" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
        <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        <h2 class="tt-modal-header blue-header">Apply to</h2>
        <div class="tt-modal-content large-12 columns">
            <div class="row">
                <div class="large-12 columns">
                    <%
                        if (TeamTimeClass.isTeamTimeOwner)
                        {


                        var WorkSpace = App.DBWorkspaceByUserIDProjectID(App.ActiveProject.OwnerID, App.ProjectID);
                        var nodes = App.ActiveProject.HierarchyObjectives.Nodes;
                        var TeamTime = (clsTeamTimePipe)Session["team"];
                        foreach(ECCore.clsNode Node in nodes)
                        {
                            var ActionType = TeamTimeClass.Action(0).ActionType;
                            var node_type = Node.get_MeasureType(false).ToString().Replace("mt", "");
                            var action_type = ActionType.ToString();
                            if (action_type.Contains(node_type))
                            { %>
                                 <input name="node_guids[]" class="apply_to_checkboxes" style="float:none;" type="checkbox" id="node-<%=Node.NodeID%>" data-id="<%=Node.NodeID%>"/>        
                                <label  for="node-<%=Node.NodeID%>"><%=Node.NodeName%></label>
                                <br />
                    <%      } %>            
                    <%    }
                                                }
                    %>
               
                </div> 
                <div class="row">
                    <br />
                    <div class="small-6 columns text-left">
                        <a href="#" class="button tiny tt-button-icond-left close-reveal-modal  cn-btn-modal button tiny tt-button-primary alert ie-cancel-btn-fix">
                                <span class="icon-tt-close icon"></span>
                                <span class="text">Cancel</span>
                            </a>
                    </div>
                    <div class="small-6 columns text-right">
                            <a href="#" class="button tiny tt-button-icond-left apply-nodes-btn button tiny tt-button-primary success">
                                <span class="icon-tt-save icon"></span>
                                <span class="text">Save</span>
                            </a>
                    </div>
                </div>
                <div class="columns">
                    <div data-alert class="alert-box success radius" style="display:none;">
                        Updated successfully!
                        <a href="#" class="close">&times;</a>
                    </div>
                    <div data-alert class="alert-box alert radius hide" style="display:none;">
                        Error on saving. Please try again.
                        <a href="#" class="close">&times;</a>
                    </div>
                </div>
               
        </div>
    </div>
<%-- //end of modal --%>
        
        <div id="MobileHelpModal" class="reveal-modal" data-reveal aria-labelledby="modalTitle" aria-hidden="true" role="dialog">
            <h2 id="modalTitle">{{mobile_help_data[0]}}</h2>
            <p>{{mobile_help_data[1]}}</p>
            <a class="close-reveal-modal" aria-label="Close">&#215;</a>
        </div>
   </div>
     
<%}
else
{ %>
<div class="tt-body">
    <div class="row">
        <div class="large-12 columns">
            <div class="large-12 columns text-center tt-resources-wrap">
                <h3>Waiting for the Project Manager to start/resume the meeting. 
                You will automatically join the meeting when it starts.</h3>
            </div><%=App.ActiveProject.ProjectManager.UserID %> <%=meeting_owner %>
            <%if (App.ActiveProject.ProjectManager.UserID == Convert.ToInt32(meeting_owner))
                { %> 
            <div class="large-centered large-8 small-8 small-centered columns text-center">
                <a href="#" class='button normal-green-bg ng-scope startMeetingBtn'><span>Start Session<span></a>
            </div>
            <%} %>
        </div>
    </div>
</div>
<% } %>
    
<script src="/Scripts/Controllers/TeamTimeController.js"></script> 
<script src="/Content/bower_components/Chart.js/dist/Chart.js"></script>   
<script src="/Scripts/Pipe/Global.js"></script> 
<script src="/Scripts/judgment.js"></script>

<%-- added this snippet to save space --%>
    
<%--<asp:PlaceHolder runat="server">
    <%: Scripts.Render("~/bundles/teamTimeController") %>
    <%: Scripts.Render("~/bundles/pipeExtension") %>
</asp:PlaceHolder>--%>
<script src='https://cloud.tinymce.com/stable/tinymce.min.js'></script>
<script src="/Scripts/NoUI/nouislider.min.js" ng-if="output.pairwise_type == 'ptGraphical'" ></script>
<script src="/Scripts/NoUI/wNumb.js" ng-if="output.pairwise_type == 'ptGraphical'"></script>
<script>   
    $(".fullwidth-loading-wrap").removeClass("hide");
    $(".fullwidth-loading-wrap").css("display", "inline");
    var loaded = false;
    $(window).load(function () {
        loaded = true;
    });

 

    //checkboxes

    <% if (PmOverwriteInfoDocs == 1) { %>
            $(".resize-checkbox").prop("checked", "checked");
    <%  } %>

    <% if (PmHideAndShowInfoDocs == 1)
        { %>
            $(".infodocs-checkbox").prop("checked", "checked");
    <%  } %>
    //info docs
  

        //end of bdody load
        //make sure all nodes are displayed
<%--        <%var _users = (List<ECCore.ECTypes.clsUser>)TeamTimeClass.TeamTimeUsersList;
    var You = (ECCore.ECTypes.clsUser)TeamTimeClass.TeamTime.ProjectManager.GetUserByEMail(App.ActiveUser.UserEMail);
    var indexOfLoggednInUser = AnytimeComparion.test.teamtime.indexOfLoggednInUser;
    %>--%>

            
        var reset_btn = 0;
        var reset_email = "";
<%--        $(".reset-btn").click( function() {
            reset_btn = $(this).attr("data-index");
            reset_email = $(this).attr("data-email");
            reset_mtype = $(this).attr("data-mtype");
            update('-2147483648000_'+reset_email, 'save', reset_mtype , "<%=ResolveUrl("~/")%>", "<%=_users.Count%>", "<%=App.ActiveUser.UserEMail.ToLower()%>", reset_btn);

        });--%>
        
        //functions for footer
        function displayCurrentTime(time){
            var date = new Date();
            var hours = date.getHours();
            var minutes = date.getMinutes() < 10 ? "0" + date.getMinutes().toString() : date.getMinutes() ;
            var seconds = date.getSeconds() < 10 ? "0" + date.getSeconds().toString() : date.getSeconds() ;

            var ampm = "AM";
            if (hours > 12) {
                hours -= 12;
                ampm = "PM";
            } else if (hours == 12) {
                hours = 12;
                ampm = "PM";
            }
            else if (hours == 0) {
                hours = 12;
                ampm = "AM";
            }

            var month = date.getMonth();
            var day = date.getDate();
            var year = date.getFullYear();
            ;

            var monthNames = [ "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December" ];

            $("." + time).html(hours+":"+minutes+":" + seconds + " " + ampm);
        }

        var failed_requests = 0;

        var intervalID =   setInterval(function () {}, 1000 );

        function updateTimeSinceLastJudgment(){

            var input = {
                hours: 0,
                minutes: 0,
                seconds: 0
            };

            var timestamp = new Date(input.hours, input.minutes, input.seconds);

            var interval = 1;

            intervalID = setInterval(function () {

                timestamp = new Date(timestamp.getTime() + interval * 1000);
                var min_0 =  timestamp.getMinutes() < 10 ? "0"  : "";
                var sec_0 = timestamp.getSeconds() < 10 ? "0"  : "";
                var hour_0 =  timestamp.getHours() < 10 ? "0"  : "";

                $('.last_judgment_time').html(hour_0 + timestamp.getHours() + ":" + min_0 + timestamp.getMinutes() + ":" + sec_0 + timestamp.getSeconds());
            }, Math.abs(interval) * 1000);

        }
        //end of functions for footer
    
        //show_info_docs_with_content();

    <%
        if(TeamTimeClass.isTeamTimeOwner)
              {
                  switch (TeamTimeClass.Action(0).ActionType)
                  {
                      case Canvas.ActionType.atInformationPage:
                          break;
                      case Canvas.ActionType.atShowLocalResults:
                          break;
                      case Canvas.ActionType.atShowGlobalResults:
                          break;
                          %>
    //hide footer for PM
    $('.pipe-footer').hide();
        
    <%
                  default:
                      break;
              }
          }%>

    //Global Vars

        var action_type;

        var show_participant_list;
        var list_of_steps;
        var steps;
        var steps_current_step;
        var current_paginate_number = 1;
        var task_latest = "";
        var hide_offline;
        var current_user_status;

        var your_judgment;
        var teamtime_page_load = false;
        var total_users;
        var counter = 0;
        var pairwise_nav_update =1;
        var trigger_update=0;
        var TT_users_list;
        var curve_handle_bar_position = "";
        var curve_slider_erase;
        var Graphical_Slider_Erase;
        var graphical_user_judgment;
        var userInfo;
        var checkstep;
        var step = 1;
        var jdata = "";
        var nodeinfos;
        var evaluated;
        var is_timer_running = false;
        var question = "";
        var wording = "";
        var PipeVal;
        var users_eval_progress;
        var baseUrl = '<%= ResolveUrl("~/") %>';
        var welcome_page = "";
        var thank_you_page = "";
        var d2data = [];
        //start of timer
        timer = function () {
            if(is_timer_running){
                //console.log("Timer is running");
                return false;
            }
            counter++;
            displayCurrentTime("current_time");
            
            $.ajax({
                type: "POST",
                url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/refreshPage",
                contentType: "application/json; charset=utf-8",
                data: "{}",
                timeout:1000*100, // 30 seconds, if server hangs it cuts the timer after 30 sec
                beforeSend: function(){
                    is_timer_running = true;
                    if (counter==1){
                    } 
                    //console.log("Timer is running " + counter);
                },
                complete: function(){
                    // console.log("Timer is done");
                    // Handle the complete event
                    is_timer_running = false;
                },
                success: function (data) {

                    displayCurrentTime("last_request_time");
                    var TTData = JSON.parse(data.d);
                    var output = TTData.output;
                    if (output == null ) {
                        output = d2data;
                        
                    }
                    else {
                        d2data = output;
                    }


                    if(data!=null && data.isPM){
                        $("#breadcrumbs").show();
                    }
                    else{
                        $("#breadcrumbs").hide();
                    }
                
                    current_step =  output.current_step; //added by ena 

                    
                    sRes = output.sRes;
                    cluster_phrase = output.cluster_phrase;
                    if(cluster_phrase){
                        //change color of edit question button
                        var cluster_phrase_text = cluster_phrase.replace(/(<([^>]+)>)/ig,"").trim();
                        var default_wording = sRes.replace(/(<([^>]+)>)/ig,"").trim();
                        if(cluster_phrase_text != default_wording){
                            $(".pipe-question .edit-pencil").css("color", "#c0392b");
                        }
                        else{
                            $(".pipe-question .edit-pencil").css("color", "#f4ab5e");
                        }
                        //end of change color
                    }

                    if (checkstep != output.current_step || step == null) {

                        checkstep = output.current_step;
                    }

                    //<<<-------------------------------------------------------judgment made
                 <% var totalObjectives = App.ActiveProject.HierarchyObjectives.Nodes.Count;
                    var totalAlternatives = App.ActiveProject.HierarchyAlternatives.Nodes.Count;%>
                    var jmade = output.jmade;
                    <%
                    if (totalObjectives == 1)
                    {
                        totalObjectives = 0;
                    }
                    if (totalAlternatives == 0)
                    {
                        totalObjectives = 0;
                    }%>

                    //judgment made -------------------------------------------------->>>

                    //$(".currentStepInt").html(output.current_step);
                    if (output == "0" || output == "") {
                        $("#<%=WaitingPage.ClientID%>").show();
                    }
                    else{
                        if(output.sdata != null)
                        {
                            if (output.pipeData && $("#TeamTimeDiv").length == 0) {
                                location.reload();
                            }
                            if (output.current_step == 0 || checkstep == 0) {
                                $("#<%=WaitingPage.ClientID%>").show();
                            }
                            else {
                                    //output.leftnodeinfo = output.leftnodeinfo;
                                    //output.rightnodeinfo = output.rightnodeinfo.replace("hide", "");
                                    //output.WrtLeftInfo = output.WrtLeftInfo.replace("hide", "");
                                    //output.WrtRightInfo = output.WrtRightInfo.replace("hide", "");
                                    //output.ParentNodeInfo = output.ParentNodeInfo.replace("hide", "");
                                    
                                    question = output.question;
                                    wording = output.wording;
                                    //check if results or actual screen
                                    action_type = output.actions;
                                    var dbarray = eval("[" + output.sdata + "]");
                                    var dbarr = dbarray.toString();
                                    var options;
                                    var hidejudge;
                                    steps = output.steps;
                                    //if (dbarr.indexOf("pended") > -1) {
                                    //    PipeVal = dbarray[4 + dbarr.indexOf("pended")][4];
                                    //    options = dbarray[3 + dbarr.indexOf("pended")][5];
                                    //    hidejudge = dbarray[3 + dbarr.indexOf("pended")][4];
                                    //    task_latest = dbarray[3 + dbarr.indexOf("pended")][2];
                                    //    users_eval_progress = dbarray[2 + dbarr.indexOf("pended")][3];
                                    //    list_of_steps = dbarray[2 + dbarr.indexOf("pended")][2];
                                    //    steps_current_step = dbarray[2 + dbarr.indexOf("pended")][1];
                                    //    hide_offline = dbarray[3 + dbarr.indexOf("pended")][12] == 1;
                                    //}
                                    //else{
                                    //    users_eval_progress = dbarray[1][3];
                                    //    PipeVal = dbarray[3][4];
                                    //    options = dbarray[2][5];
                                    //    hidejudge = dbarray[2][4];
                                    //    task_latest = dbarray[3][2];
                                    //    $(".toggle-user-display").val(output.displayuser)
                                    //    list_of_steps = dbarray[1][2];
                                    //    steps_current_step = dbarray[1][1];
                                    //    hide_offline = dbarray[2][12] == 1;
                                    //}
                                   
                                    

                                    //TT_users_list = PipeVal[1];
                                    //if(output.mtype == "mtRegularUtilityCurve" || output.mtype == "mtRatings")
                                    //{
                                    //    TT_users_list = PipeVal[2];
                                    //}
                                    slide_infodoc_node(output.infodoc_dropdown); 
                                    resize_infodoc_node(output.infodoc_sizes);
                                    
                                    


                                    
                                    if(output.actions == "atInformationPage")
                                    {
                                        $('.pipe-settings').hide();
                                        if(output.welcome != welcome_page){
                                            //$(".informationText").html(output.welcome);
                                            $(".informationText p").attr("style", $("body").attr("style"));
                                            $(".informationText").attr("style", $("body").attr("style"));
                                            $(".tt-stick-that-footer-content").css({ height: $(window).height() - 240  });
<%--                                            $("#<%=InformationPage.ClientID%>").show();
                                            $("#<%=WaitingPage.ClientID%>").hide();
                                            welcome_page = output.welcome;--%>
                                        }


                                        if(output.thankyou != thank_you_page){
                                            //$(".thankyouText").html(output.thankyou);
                                            $(".thankyouText p").attr("style", $("body").attr("style"));
                                            $(".thankyouText").attr("style", $("body").attr("style"));
                                            $(".tt-stick-that-footer-content").css({ height: $(window).height() - 240  });
<%--                                            $("#<%=ThankYou.ClientID%>").show();
                                            $("#<%=WaitingPage.ClientID%>").hide();
                                            thank_you_page = output.thankyou;--%>
                                        }

                                
                                    }
                                    else
                                    {
                                        $('.pipe-settings').show();
                                        if(teamtime_page_load == false)
                                        {
                                            //if(output.pagination[0] == 0)
                                            //{
                                            //    output.pagination[0] = 'all'
                                            //}
                                            //pagination_CurrentPage = output.pagination[0];
                                            //pagination_NoOfUsers = output.pagination[1];

                                            get_pipe_settings_status(teamtime_page_load, hidejudge, options, output.hide_to_me, output.is_pie_hidden);
                                            teamtime_page_load = true;

                                            //show_participant_list = Boolean(output.toggle);
                                            //if(hide_offline){
                                            //    $('.online-checkbox').prop('checked', true);
                                            //}
                                            //else{
                                            //    $('.online-checkbox').prop('checked', false);
                                            //}

                                                
                                        }
                                    
                                    if(output.actions == "atPairwise")
                                    {
                                            //var l_info = output.leftnodeinfo;
                                            //var r_info = output.rightnodeinfo;
                                            //var wrt_l_info = output.WrtLeftInfo;
                                            //var wrt_r_info = output.WrtRightInfo;
                                            //var p_info = output.ParentNodeInfo;
                                            ////console.log(output.ParentNodeInfo);
                                         
                                            //$(".left-node-info-text").html(l_info);
                                            //$(".right-node-info-text").html( r_info);
                                            //$(".wrt-left-node-info-text").html( wrt_l_info);
                                            //$(".wrt-right-node-info-text").html( wrt_r_info);
                                            //$(".parent-node-info-text").html( p_info);
                                            //$(".tt-content-wrap .parent-node").show();
                                            //$(".tt-content-wrap .left-node").show();
                                            //$(".tt-content-wrap .right-node").show();

                                        var infoajax = output.leftnodeinfo + output.rightnodeinfo + output.WrtLeftInfo + output.WrtRightInfo + output.ParentNodeInfo;

                                       
                                        

                                        is_pairwise = true;


                                        //comments
                                        //if(total_users != PipeVal[1].length)
                                        //{
                                        //    total_users = PipeVal[1].length;
                                            
                                        //}
                                        if((jdata != dbarr && dbarr.indexOf("pended") < 0) || userInfo != output.displayuser)
                                        {  
                                            //$(".tt-content-wrap .tt-body .tt-judgements-item .tt-j-others-result .tt-j-others-result-content > div.active .tt-user-status").show;
                                            //$('.tt-loading-icon-wrap').show();
                                            clearInterval(intervalID);
                                            updateTimeSinceLastJudgment();

                                            if (PipeVal) {
                                                $(".parent-node").html(PipeVal[0][0][2]);
                                                $(".left-node").html(PipeVal[0][1][2]);
                                                $(".right-node").html(PipeVal[0][2][2]);

                                                output_parent_node = PipeVal[0][0][2];
                                                output_left_node = PipeVal[0][1][2];
                                                output_right_node = PipeVal[0][2][2];
                                            }

                                            //users_list_length = PipeVal[1].length;

                                            //$(".pendingIndicator").hide();
                                            //$(".reset-btn").show();
                                            //userInfo = output.displayuser;
                                            //jdata = dbarr;
                                            //hide_loading_icon();
                                            
                                        }

                                       

                                        //displayToggle(output.toggle, output.grouptoggle)
                                    }
                                    else if(output.actions == "atNonPWOneAtATime")
                                    {
                                        if(output.mtype == "mtRatings")
                                        {
                                            is_ratings = true;
                                            <%if (RatingScale1 != null){%>

                                            $(".left-node-info-text").html(output.leftnodeinfo);
                                            $(".parent-node-info-text").html(output.ParentNodeInfo);
                                            $(".wrt-left-node-info-text").html(output.WrtLeftInfo);



                                            if((jdata != dbarr && dbarr.indexOf("pended") < 0) || userInfo != output.displayuser) //this execute only once
                                            {
                                                if (PipeVal) {
                                                    output_parent_node = PipeVal[0][0][2];
                                                    output_left_node = PipeVal[0][1][2];

                                                    $(".left-node").html(PipeVal[0][1][2]);
                                                    $(".parent-node-name").html(PipeVal[0][0][2]);
                                                    output_parent_node = PipeVal[0][0][2];
                                                }

                                                output_right_node = "";
                                                $('.tt-loading-icon-wrap').show();
                                                console.log("show");
                                                $(".rswording").html(output.wording);

                                                //if(output.task.indexOf("(!)") < 0)
                                                //{
                                                //    $('.LblTask').html(task_latest);
                                                //    $(".LblTask").find("p").contents().unwrap();
                                                //}
                                                //else
                                                //{
                                                //    $('.LblTask .rswording').html(output.question);
                                                //}

                                                if (PipeVal) {
                                                    $(".score").html((PipeVal[4] * 1).toFixed(2));
                                                    $(".dc-group-result-bar").width((PipeVal[3] * 100).toFixed(2) + "%");
                                                    $("#RatingsPriority").html("Priority: " + (PipeVal[3] * 1).toFixed(2));
                                                }

                                                hide_loading_icon();
                                                jdata = dbarr; //stop code from repeating after executing once
                                                userInfo = output.displayuser;
                                            }
                                            //displayToggle(output.toggle, output.grouptoggle)
                                            if (PipeVal) {
                                                UserslistDisplay(options, output.displayuser, PipeVal[2]);
                                            }
                                            HideJudgment(hidejudge, output.actions, output.mtype);
                                            <%}%>
                                        }

                                        if(output.mtype == "mtDirect")
                                        {
                                            is_direct = true;
                                            <%if (DirectUser1 != null){%>
                                            //var infoajax = output.leftnodeinfo + output.rightnodeinfo + output.WrtLeftInfo;
                                            //if(nodeinfos != infoajax)
                                            //{
                                                $(".left-node-info-text").html(output.leftnodeinfo);
                                                $(".parent-node-info-text").html(output.parentnodeinfo);
                                                $(".wrt-left-node-info-text").html(output.WrtLeftInfo);
                                               
                                                //nodeinfos = infoajax;

                                                //$("#DirectChildTextArea").html(output.leftnodeinfo);
                                                //$("#DirectParentTextArea").html(output.rightnodeinfo);
                                                //$("#DirectWrtTextArea").html(output.WrtLeftInfo);
                                           // }

                                            


                                            if((jdata != dbarr && dbarr.indexOf("pended") < 0) || userInfo != output.displayuser) //this execute only once
                                            {
                                                if (PipeVal) {
                                                    $(".left-node").html(PipeVal[0][1][2]);
                                                    $(".parent-node").html(PipeVal[0][0][2]);
                                                    $(".parent-node-name").html(PipeVal[0][0][2]);

                                                    output_parent_node = PipeVal[0][0][2];
                                                    output_left_node = PipeVal[0][1][2];
                                                }
                                                output_right_node = "";

                                                jdata = dbarr; //stop code from repeating after executing once
                                                userInfo = output.displayuser;

                                               
                                                //if(output.task.indexOf("(!)") < 0)
                                                //{
                                                //    //$('.LblTask').html(task_latest);
                                                //    $(".LblTask").find("p").contents().unwrap();
                                                //}

                                                if (PipeVal) {
                                                    $(".score").html((PipeVal[3] * 1).toFixed(2));
                                                    $(".dc-group-result-bar").width((PipeVal[2] * 100).toFixed(2) + "%");
                                                    $("#DirectPriority").html("Priority: " + (PipeVal[2]).toFixed(2));
                                                }

                                                hide_loading_icon();
                                            }
                                            //displayToggle(output.toggle, output.grouptoggle)
                                            if (PipeVal) {
                                                UserslistDisplay(options, output.displayuser, PipeVal[1]);
                                            }
                                            HideJudgment(hidejudge, output.actions, output.mtype);
                                            <%}%>
                                        }
                                        if(output.mtype == "mtRegularUtilityCurve")
                                        {

                                        }
                                        if(output.mtype == "mtStep"){
                                        }
                                    }
                                    else if(output.actions == "atShowLocalResults")
                                    {
                                        $('.pipe-settings').hide();
                                    }
                                    else if(output.actions == "atShowGlobalResults")
                                    {
                                        $('.pipe-settings').hide();
                                    }      
                                 }

                                } //else 
                            } 
                        else
                        {
                            if (output.TeamTimeOn === true)
                            {
                                //$('.disable-when-pause').addClass("disabledbutton");
                                //$(".ended").hide();
                                //$(".waiting").show();

                                step = null;
                                checkstep = 0;
                            }
                            else
                            {
                                //$(".waiting").hide();
                                //$(".ended").show();

                            }
                        }
                
                    }
                    if(loaded && counter==1){

                    }
                },
                error: function (response) {
                    console.log(response);
                    failed_requests++;
                    $(".failed_requests").html(failed_requests);
                }
            });
                                        
                                        
                                        
        };

        timer_interval = setInterval(timer, 1000);

        //userlist display
        var toggle = 1;
    
        $(".toggle-user-display").change( function() {
            toggle = $(this).val();
            UserslistDisplay();
        });    

        var checkval = -1;
        function UserslistDisplay(options, displayuser, userslist)
        {

        }
        //End of start of userlist display

//    $('.hide-judgment-checkbox').change(function(){
//        $('.hide-radio.default-radio').prop('checked', true);
//    });

//    $('.hide-radio.default-radio').change(function(){
//        if($(this).is(':checked'))
//            $('.hide-radio.default-radio').prop('checked', true);
//        else
//            $('.hide-radio.default-radio').prop('checked', false);
//    });
//    $('.hide-radio.me-radio').change(function(){
//        if($(this).is(':checked'))
//            $('.hide-radio.me-radio').prop('checked', true);
//        else
//            $('.hide-radio.me-radio').prop('checked', false);
//    });
        //Show/hide Judgments

        //End of Show/hide Judgments

        function get_pipe_settings_status(is_page_load, is_judgment_hidden, is_anonymous_mode, is_shown_to_me, is_pie_hidden)
        {
            if(is_page_load== false)
            {
//                $('.hide-judgment-checkbox').prop('checked', Boolean(is_judgment_hidden));
//                $('.anonymous-checkbox').prop('checked', Boolean(is_anonymous_mode));
//                if(is_shown_to_me){
//                    $('.hide-radio.me-radio').prop("checked", true);
//                }
//                else{
//                    $('.hide-radio.default-radio').prop("checked", true);
//                }
//                $('.hide-pie-checkbox').prop('checked', Boolean(is_pie_hidden == 1));
                

            }
        }


        <%var showtog = Convert.ToInt16(Session["showtoggle"]);
        if (showtog == 1)
        {%>
        $(".tg-idr").slideDown();
        <%}
        else
        {%>
        $(".tg-idr").slideUp();
        <%}
            %>

        <%var showgroup = Convert.ToInt16(Session["grouptoggle"]);
        if (showgroup == 1)
        {%>
        $(".tg-1").slideDown();
        <%}
        else
        {%>
        $(".tg-1").slideUp();
        <%}%>
        //end of toggles codes
        
        //Show/Hide Pipe
        var checkpie;
        function hide_pie_chart(is_pie_hidden)
        {
        }
        //End of Show/Hide Pipe

        //loading 
        function hide_loading_icon()
        {
            setTimeout(function() {
                console.log('hiding loading icon');
                $('.tt-loading-icon-wrap').fadeOut(); //hide loading icon
            },500);
        }
        function show_loading_icon()
        {
                $('.tt-loading-icon-wrap').show(); //hide loading icon
        }
        
        //End of Loading
       
        function show_info_docs_with_content(){
            for(var i=0;i<5;i++){
                if($(".tt-resizable-panel-"+i +" .nodes-wrap").html() != ""){
                    update("true_"+i, 'node', '', '<%= ResolveUrl("~/") %>');
                }
            }
        }


        //Hide/Show Info Nodes
        var infodoc_info_nodes;
        function slide_infodoc_node(infodoc_dropdown){
            if(infodoc_info_nodes !=  JSON.stringify(infodoc_dropdown)){
                for(a=0; a< infodoc_dropdown.length; a++){
                    if(infodoc_dropdown[a]){
                        //$(".tg-accordion-sub-" + a).slideDown();
                        $(".tt-accordion-head .icon").each(function(e){
                            var node = $(this).attr("data-node");
                            
                            var is_mobile = $(this).attr("data-mobile");
                            if(a == parseInt(node) && is_mobile != 'true'){
                                //$(this).removeClass('icon-tt-plus-square');
                                //$(this).addClass('icon-tt-minus-square');
                            }
                            //$(".ep-sub-"+ a).removeClass('hide')
                        });

                    }
                    else if(infodoc_dropdown[a] == false){
                        //$(".tg-accordion-sub-" + a).slideUp();
                        $(".tt-accordion-head .icon-desktop").each(function(e){
                            var node = $(this).attr("data-node");
                            if(a == parseInt(node)){ 
                                //$(this).removeClass('icon-tt-minus-square');
                                //$(this).addClass('icon-tt-plus-square');
                            }
                            //$(".ep-sub-"+ a).addClass('hide');
                        });
                    }
                }
                infodoc_info_nodes = JSON.stringify(infodoc_dropdown);
                hide_loading_icon();
            }

        }
        //End of Hide/Show Info Nodes
        
        var infodoc_sizes_values;
        //resize nodes
        function resize_infodoc_node(infodoc_sizes){
            // alert(JSON.stringify(participant_sizes));
            if(infodoc_sizes_values !=  JSON.stringify(infodoc_sizes)){
                //reformat array
                var nodes_sizes = [];
                
                $.each(infodoc_sizes, function(index, value){
                    if(index < 5){
                        nodes_sizes.push([ infodoc_sizes[index + index], infodoc_sizes[index + (index + 1)]]); 
                    }
                });

                $.each(nodes_sizes, function(node, size){
                    if(size[0] != "0" || size[1] != "0"){
                        $(".tt-resizable-panel-"+node).css("width", size[0]);
                        $(".tt-resizable-panel-"+node).css("height", size[1]); //not responsive yet
                    }    
                });
                infodoc_sizes_values = JSON.stringify(infodoc_sizes); 
                }
        }



//    $('.online-checkbox').change(function(){
//        update($(this).prop('checked'), 'offline','', '<%=ResolveUrl("~/")%>');
//    });

        //movie pipe        
        function movePipe(val)
        {
            window.location.href = "teamtime.aspx?pipe=" + val;
        }
        //end of movie pipe 

        $(document).ready( function(){
            $(document).on('click', '.resize-checkbox-label', function (e) {
                var overwrite = !$(".resize-checkbox").is(":checked") ? 1 : 0;
                clearInterval(timer_interval);
                var baseUrl = '<%= ResolveUrl("~/") %>';
                $.ajax({
                    type: "POST",
                    url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/SetResizeInfoDocsToAll",
                    data: JSON.stringify({
                        value: overwrite
                    }),
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        // alert(data);
                        timer_interval = setInterval(timer, 1000);
                    },
                    error: function (response) {
                        timer_interval = setInterval(timer, 1000);
                        // alert(response);
                    }
                });
            });

            $(document).on('click', '.resize-checkbox', function (e) {
                e.stopPropagation();
            });

            $(document).on('click', '.infodocs-checkbox-label', function (e) {
                var checked = !$(".infodocs-checkbox").is(":checked") ? 1 : 0;
                clearInterval(timer_interval);
                var baseUrl = '<%= ResolveUrl("~/") %>';
                $.ajax({
                    type: "POST",
                    url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/SetHideAndShowInfoDocsToAll",
                    data: JSON.stringify({
                        value: checked
                    }),
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        // alert(data);
                        timer_interval = setInterval(timer, 1000);
                    },
                    error: function (response) {
                        timer_interval = setInterval(timer, 1000);
                        // alert(response);
                    }
                });
            });

            $(document).on('click', '.infodocs-checkbox', function (e) {
                e.stopPropagation();
            });

            // continue session if PM
            $('.startMeetingBtn').click(function () {
            show_loading_modal();
                //var restart_pipe = $('.start-tt-as-pm .restart').prop('checked');
            var restart_pipe = false;
            var baseUrl = '<%= ResolveUrl("~/") %>';
            $.ajax({
                type: "POST",
                url: baseUrl + "Default.aspx/StartMeeting",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    IfRestartPipe: restart_pipe
                }),
                dataType: "json",
                success: function (data) {
                    if (data.d) {
                        window.location = baseUrl + 'pages/teamtimetest/teamtime.aspx';

                    }
                },
                error: function (response) {
                    $(".fullwidth-loading-wrap").addClass("hide");
                    console.log(response);
                    hide_loading_modal();
                }
            });
            return false;
        });

/** removed comment form scripts. transferred to app.js by Mike A. 5/24/2017 **/

            //***** START resizable DIV
            $( ".tt-resizable-panel" ).css("height", "100%");
            $( ".tt-resizable-panel" ).resizable({       
                /*****  handles: 's', http://api.jqueryui.com/resizable/#option-handles *****/
                handles: 's',
                start: function( event, ui ) {
                    var element = ui.element;
                    temp_height  = ui.size.height;
                    info_docs_resize_event = true;
                },
                stop: function( event, ui ) {
                    var element = ui.element;
                    var index = element.attr("data-index") 
                    var height = ui.size.height;
                    var width = ui.size.width;
                    //if(index==0){
                       // var accordion_class_name = ".tg-accordion-0";
                    //}
                   // else{
                        //var accordion_class_name = ".tg-accordion-sub-"+index;
                      
                    //}
                   
                    //var responsive_width = (( width) / $(accordion_class_name).width()) * 100; //not sure with the formula
                    //var responsive_height = ((height) /  temp_height) * 100; //not working percentage
                    <% if(TeamTimeClass.isTeamTimeOwner){%>
                        update(index+"_"+width+"_"+height, "infodoc-size", "", '<%= ResolveUrl("~/") %>');
                    <% } else{%>
                        $.ajax({
                            type: "POST",
                            url: baseUrl + "Pages/TeamTimeTest/teamtime.aspx/ResizeParticipantInfoDocs",
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({
                                index:  index,
                                width: width,
                                height: height
                            }),
                            success: function (data) {
                                //resize_infodoc_node(output.infodoc_sizes);
                                timer_interval = setInterval(timer, 1000);
                            },
                            error: function (response) {
                                console.log(JSON.stringify(response));
                            }
                        });
                    <% }%>
                }
            });
            //***** END resizable DIV
            //End of Mike A's section for design

            // remove loading on error
            window.onerror = function (errorMsg, url, lineNumber) {
                console.log("!!!!"  + errorMsg + lineNumber);
                setInterval(function () {

                }, 4000 );



                // alert("This is a stack trace! Wow! --> %s", error.stack);
            };

        });
//      //***** START togglers *****//

//    $(document).on('click', '.tt-toggler-tt-fix', function (e) {
//        var toggleWhat = $(this).attr("data-toggler");
//        switch (toggleWhat) {
                
//            case 'tg-class-checkbox': //toggles class and title attribute
//                var thisClass = $(this).attr('data-class');
//                var thisSpan = $(this).find('span.icon ');
//                var thisCheckbox = $(this).find('input[type="checkbox"]');
                
//                thisSpan.toggleClass(thisClass);
                
//                if(thisCheckbox.is(':checked')){
//                    thisCheckbox.prop('checked', false);
//                }else{
//                    thisCheckbox.prop('checked', true);
//                }
                
//                break;
                
//            case 'tg-class-checkbox-inverted': //toggles class and title attribute
//                var thisClass = $(this).attr('data-class');
//                var thisSpan = $(this).find('span.icon ');
//                var thisCheckbox = $(this).find('input[type="checkbox"]');
                
//                thisSpan.toggleClass(thisClass);
                
//                if(thisCheckbox.is(':checked')){
//                    thisCheckbox.prop('checked', false);
//                }else{
//                    thisCheckbox.prop('checked', true);
//                }
                
//                break;
                
//            case 'tg-mic': //mute unmute microphone
//                var thisWrap = $(this);
//                var thisSpan = $(this).find('span.icon ');
//                thisWrap.toggleClass('disabled');
//                thisSpan.toggleClass('icon-tt-microphone icon-tt-microphone-mute');
//                break;
            
//            case 'tg-pause-play': //pause play pipe
//                var thisId = $(this).attr("id");
//                var thisWrap = $(this);
//                var thisSpan = $(this).find('span.icon ');
//                thisSpan.toggleClass('icon-tt-pause icon-tt-play');
                
//                if(thisId == "TTpause"){
//                    thisWrap.attr("id", "TTstart");
//                    thisSpan.attr("data-event", "resume");
//                }
//                if(thisId == "TTstart"){
//                    thisWrap.attr("id", "TTpause");
//                    thisSpan.attr("data-event", "pause");
//                }
                    
//                break;
                
//            case 'tg-s':
//                $('.tg-s').slideToggle();
//                break;
            
//            case 'tg-sf':
//                $('.tg-sf').slideToggle();
//                $(this).toggleClass('close open');
//                if($(this).hasClass('open')){
//                    $(this).text('Cancel');
//                }else{
//                    $(this).text('Edit Data');
//                }
                    
//                break;

//            case 'fmobile':
//                $(this).toggleClass('down up'); 
                
//                var thisWrap = $('.footer-nav-mobile');
//                var thisSpan = $(this).find('span.icon');
//                thisSpan.toggleClass('icon-tt-sort-down icon-tt-sort-up');
//                thisWrap.toggleClass('up down');

              
//                break;

//            case 'tg-legend':
//                $('.tg-legend').fadeToggle();
//                var thisSpan = $(this).find('span.text');
//                if ($(thisSpan).text() == "View Legend")
//                    $(thisSpan).text("Hide Legend");
//                else
//                    $(thisSpan).text("View Legend");
//                break;

//            case 'tg-site-map':
//                var id = $(this).attr('data-toggler-id');
//                $('.tg-sm-' + id).slideToggle();
//                var thisSpan = $(this).find('span.tt-sm-icon');
//                thisSpan.toggleClass('icon-tt-minus-circle icon-tt-plus-circle');
//                break;

//            case 'tg-mobileNav':
//                $('.tg-mobileNav').slideToggle();
//                break;

//            case 'tg-mNav-sub':
//                $('.tg-mNav-sub').slideToggle();
//                var thisSpan = $(this).find('span.rightArrow');
//                thisSpan.toggleClass('icon-tt-chevron-down icon-tt-chevron-up');
//                break;

//            case 'tg-accordion':
//                var id = $(this).attr("id");
//                $('.tg-accordion-' + id).slideToggle();
//                $('.ep-' + id).toggleClass('hide');
//                var thisSpan = $(this).find('span.icon');
//                thisSpan.toggleClass('icon-tt-plus-square icon-tt-minus-square');
//                break;

//            case 'tg-accordion-sub':
//                var id = $(this).attr("id");
//                $('.tg-accordion-sub-' + id).slideToggle();
//                $('.ep-sub-' + id).toggleClass('hide');
//                var thisSpan = $(this).find('span.icon');
//                thisSpan.toggleClass('icon-tt-plus-square icon-tt-minus-square');
//                break;
            
//            case 'tg-tree-nav':
//                var id = $(this).attr("id");
//                $('.tg-tree-nav-' + id).toggle();
//                var thisSpan = $(this).find('span.icon');
//                thisSpan.toggleClass('icon-tt-plus-square icon-tt-minus-square');
                
//                console.log(id);
//                break;
            
//            case 'tg-tree-nav-sub-first':
//                var id = $(this).attr("id");
//                $('.tg-tree-nav-sub-first-' + id).toggle();
//                var thisSpan = $(this).find('span.icon');
//                thisSpan.toggleClass('icon-tt-plus-square icon-tt-minus-square');
                
//                console.log(id);
//                break;
            
//            case 'tg-tree-nav-sub-second':
//                var id = $(this).attr("id");
//                $('.tg-tree-nav-sub-second-' + id).toggle();
//                var thisSpan = $(this).find('span.icon');
//                thisSpan.toggleClass('icon-tt-plus-square icon-tt-minus-square');
                
//                console.log(id);
//                break;
            
//            case 'tg-obj-content':
////                var id = $(this).attr("id");
//                $('.tg-obj-content').toggle("slide");
////                var thisSpan = $(this).find('span.icon');
                
//                break;

//            default:
//                var id = $(this).attr("id");
//                $('.tg-' + id).slideToggle();
//                var thisSpan = $(this).find('span.icon');
//                var thisText = $(this).find('span.text');
//                if (thisText.text() == "Show"){
//                    thisText.text("Hide")}
//                else{
//                    thisText.text("Show");
//                }
//                thisSpan.toggleClass('icon-tt-plus-square icon-tt-minus-square');
//                break;
//        }
//    });
//    //***** END togglers *****//
</script>
    <script type="text/ng-template" id="menu_sublevel.html">
    <span class="hierarchy" ng-style="{'color': '{{output.pipeData.ParentNodeID == hierarchy[0] ? '#b45f06' : '' }}'}">
        <span ng-class="hierarchy[3] == 1 ? 'icon-tt-minus-circle tt-sm-icon' : 'icon-tt-plus-circle tt-sm-icon'" ng-click="hierarchy[3] = hierarchy[3] == 1 ? 0 : 1"></span>
        <span ng-click="MoveToPipe(hierarchy[4])">
        <b class="bolded">{{hierarchy[1]}} </b></span>
        <span style="color: gray;">(#{{hierarchy[4]}})</span>
    </span>
    <ul ng-if="hierarchy[3] == 1">
        <li ng-repeat="hierarchy in hierarchy[5]"  ng-include="'menu_sublevel.html'">
        </li>
    </ul>
</script>
  
</asp:Content>
