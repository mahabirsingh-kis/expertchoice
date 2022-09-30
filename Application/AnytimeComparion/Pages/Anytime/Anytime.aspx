<%@ Page Async="true" Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/AnytimeComparion/Site.Master" CodeBehind="Anytime.aspx.vb" Inherits=".Anytime" EnableEventValidation="false" %>

<%@ Register Src="~/AnytimeComparion/Pages/Anytime/AllPairWiseUserCtrl.ascx" TagPrefix="uc1" TagName="AllPairWise" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/InformationUserCtrl.ascx" TagPrefix="uc1" TagName="Information" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/PairwiseUserCtrl.ascx" TagPrefix="uc1" TagName="PairWiseCtrl" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/LocalResults.ascx" TagPrefix="uc1" TagName="LocalResults" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/GlobalResults.ascx" TagPrefix="uc1" TagName="GlobalResults" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/Pairwise.ascx" TagPrefix="uc1" TagName="Pairwise" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/MultiRatings.ascx" TagPrefix="uc1" TagName="MultiRatings" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/Ratings.ascx" TagPrefix="uc1" TagName="Ratings" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/DirectComparison.ascx" TagPrefix="uc1" TagName="DirectComparison" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/MultiDirect.ascx" TagPrefix="uc1" TagName="MultiDirect" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/Survey.ascx" TagPrefix="uc1" TagName="Survey" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/SensitivitiesAnalysis.ascx" TagPrefix="uc1" TagName="SensitivitiesAnalysis" %>
<%@ Register Src="~/AnytimeComparion/Pages/Anytime/ctUtilityCurve.ascx" TagPrefix="uc1" TagName="UtilityCurve" %>
<%@ Register Src="~/AnytimeComparion/Pages/includes/Footer.ascx" TagPrefix="includes" TagName="Footer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style type="text/css">
        .tox-toolbar__overflow button.tox-tbtn.tox-tbtn--select {
            background: #1b51a8;
            color: #fff !important;
            margin-right: 5px;
            height: 24px;
            font-size: 12px;
            cursor: pointer;
        }
    </style>
    <div id="overlay" style="display: flex;" class="large-5 columns large-centered text-center tt-loading-icon-wrap">
        <div class="label alert radius tt-loading-icon-content">
            <%--   <h2>Comparion®</h2>
            <p>Collaborative Decision Making Solution</p>--%>
            <div class="tt-loading-icon left">
                <img src="../../../Images/loadergimage.png" />
                <%--<span class="progress-view"></span>--%>
            </div>
            <span id="pipe-loading-message"></span>
        </div>
    </div>
    <%--<script src="//code.jquery.com/jquery-1.10.2.js"></script>--%>
    <%--<script src='http://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js'></script>
    <script src="//code.jquery.com/ui/1.11.4/jquery-ui.js"></script>--%>
    <script src="../../../Scripts/jquery.min.js"></script>
    <script src="../../../Scripts/jquery-ui.min.js"></script>

    <%--<script src="../../assets/libs/jquery/jquery.min.js"></script>--%>
    <script src="../../CustomScripts/anytime.js"></script>
    <script src="../../../ckeditor/ckeditor.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery-cookie/1.4.1/jquery.cookie.min.js"></script>

    <style>
        /*thead {
            position: fixed;
            background-color: blue;
            color: #fff;
        }*/
    </style>
    <script type="text/javascript">
        //var p = 0;
        //function timeout_trigger() {
        //    $(".progress-view").text(p + "%");
        //    if (p != 100) {
        //        setTimeout('timeout_trigger()', 200);
        //    }
        //    p++;
        //}
        //timeout_trigger();

        var block_Utilityunload_prompt = false;

        var newprojectname = $.cookie('OpenProjectName');
        newprojectname = newprojectname == undefined ? '' : newprojectname;
        newprojectname = newprojectname == null ? '' : newprojectname;
        var projectname = '<%= Session("ProjectName") %>'
        projectname = projectname == undefined ? '' : projectname;
        projectname = projectname == null ? '' : projectname;

        setInterval(function () {
            newprojectname = $.cookie('OpenProjectName');
             //newprojectname = '<%= Session("OpenProjectName") %>';
            projectname = '<%= Session("ProjectName") %>'
        }, 100);
        var timeVal = 1000;

        $(window).focus(function () {
            setTimeout(function () {

                var title = document.title;
                var pname = newprojectname != '' ? newprojectname.substring(newprojectname.indexOf("=") + 1) : '';
                if (pname == "none") {
                    alert("Please select Project to be viewed.")
                    window.top.close();
                    return false;
                }
                if (pname != projectname) {
                    timeVal = 5000;
                    newprojectname = $.cookie('OpenProjectName');
                    alert("Your active model has been changed, please click OK to reload the page.")
                    location.reload();
                    return false;
                }
            }, timeVal);
        });

        function CloseVideoOnPopupClose() {
            $('.yt_player_iframe').each(function () {
                this.contentWindow.postMessage('{"event":"command","func":"stopVideo","args":""}', '*')
            });
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            }
        }

        var t2s = null;
        var can_autoplay = <% =IIf(IsMobile, "false", "true; //!(is_chrome)") %>;
        var t2s_auto = <% =Bool2JS(GetCookie(COOKIE_T2S_MODE, CStr(IIf(Request.IsLocal, "0", IIf(App IsNot Nothing AndAlso App.ActiveProject.ProjectManager.Parameters.EvalTextToSpeech = ecText2Speech.AutoPlay, "1", "0")))) = "1") %>;

        function initT2S() {
            var div_t2s = $("#divT2S");
            if ((div_t2s) && (div_t2s.length)) {
                div_t2s.show();
                if ('speechSynthesis' in window) {
                    div_t2s.html("<a href='' onclick='doT2SS(); return false;' class='actions'><i class='far fa-play-circle' style='font-size:1.33rem; margin-right:8px; margin-top:-3px;' title='Play/Pause' id='btnT2SPlay'></i>" + "<a href='' onclick='toggleT2S(); return false;' class='_actions'><i class='fas " + (t2s_auto ? "fa-volume-down" : "fa-volume-mute") + "' title=\"Switch auto-play\" id='btnT2SAuto'></i></a>");
                    if (!can_autoplay) setTimeout(function () {
                        $("#btnT2SPlay").addClass("blink_twice");
                    }, 3000);
                <%--<% If task.Visible AndAlso lblTask.Text <> "" Then %>if (!can_autoplay && t2s_auto) {
                    //var task = $("#<% = lblTask.ClientID %>");
                    //if ((task) && (task.length) && task.text()!="") {
                    //    setTimeout(function () {
                    //        if (!(window.speechSynthesis.speaking)) DevExpress.ui.notify("Your browser doesn't support auto-play. Please click the play button.", 'warning', 3000);
                    //    }, 1750);
                    //}
                <%--}<% End If %>--%>
                    t2s = new SpeechSynthesisUtterance();
                    //var voices = window.speechSynthesis.getVoices();
                    //t2s.voice = voices[1]; 
                    //t2s.volume = 1; // From 0 to 1
                    t2s.rate = 0.75; // From 0.1 to 10
                    t2s.pitch = 0.8; // From 0 to 2
                <%--t2s.lang = "<% =JS_SafeString(ResString("_LanguageCode")) %>";--%>
                    t2s.onstart = function () {
                        $("#btnT2SPlay").addClass("blink_me").removeClass("fa-play-circle").addClass("fa-pause-circle");
                    };
                    t2s.onend = function () {
                        $("#btnT2SPlay").removeClass("blink_me").addClass("fa-play-circle").removeClass("fa-pause-circle");
                    };

                    if (t2s_auto && can_autoplay) {
                        if (localStorage.getItem("not2s") != "1") {
                            setTimeout(function () {
                                if (!(window.speechSynthesis.speaking)) doT2SS();
                            }, 1500);
                        } else {
                            localStorage.removeItem("not2s");
                        }
                    }
                } else {
                    var msg_unavail = "Sorry, your browser doesn't support the speaker mode.";
                    div_t2s.html("<i class='fas fa-volume-mute gray' style='font-size:1.0rem' title=\"" + msg_unavail + "\"></i>");
                }
            }
        }

        function doQuickHelpT2S() {
            t2s = new SpeechSynthesisUtterance();

            t2s.rate = 0.75; // From 0.1 to 10
            t2s.pitch = 0.8; // From 0 to 2

            t2s.onstart = function () {
                $("#btnT2SPlayQuickHelp").removeClass("fa-play").addClass("fa-pause");
            };
            t2s.onend = function () {
                $("#btnT2SPlayQuickHelp").addClass("fa-play").removeClass("fa-pause");
            };

            doT2S($('#quick-help-content').text());

        }

        function doT2SS() {
            t2s = new SpeechSynthesisUtterance();
            t2s.onstart = function () {
                $("#btnT2SPlay").addClass("blink_me").removeClass("fa-play-circle").addClass("fa-pause-circle");
            };
            t2s.onend = function () {
                $("#btnT2SPlay").removeClass("blink_me").addClass("fa-play-circle").removeClass("fa-pause-circle");
            };
            doT2S($('div.txtStepTask').text());
        }

        function doT2S(text) {
            <% If showT2S Then %>
            var task = $("#lblTask");
            document.cookie = "<% =COOKIE_T2S_MODE %>=" + (t2s_auto ? "1" : "0") + ";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT";
            if ((t2s) && (t2s != null) && ('speechSynthesis' in window)) {
                if (window.speechSynthesis.speaking) {
                    window.speechSynthesis.cancel();
                } else {
                    if (typeof text == "undefined" || text == "") text = task.text();
                    t2s.text = text;
                    window.speechSynthesis.speak(t2s);
                }
            }<% End If %>
            //
            //window.speechSynthesis.speak(t2s);
        }
        function doQuuickHelpT2SStop() {
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            }
        }

        function toggleT2S() {<% If showT2S Then %>
            t2s_auto = !t2s_auto;
            document.cookie = "<% =COOKIE_T2S_MODE %>=" + (t2s_auto ? "1" : "0") + ";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT";
            if (t2s_auto) {
                $("#btnT2SAuto").addClass("fa-volume-down").removeClass("fa-volume-mute");
                doT2S($('div.txtStepTask').text());
            } else {
                $("#btnT2SAuto").removeClass("fa-volume-down").addClass("fa-volume-mute");
                if (window.speechSynthesis.speaking) {
                    window.speechSynthesis.cancel();
                }
            }<% End If %>
        }

        var timeout_show = 1;
        function ShowTimeoutWarning() {
    <%--<% If Not IsReadOnly Then %>
        if (timeout_show) {
            timeout_show = 0;
            if (t2s_auto) localStorage.setItem("not2s", "1");
            if ((theForm.<% =isChanged.ClientID %>.value > '0')) {
                S(<% =CurStep%>);
            } else {
                document.location.reload(true);
            }

<%--            var time = new Date();
            var msg = '<% =GetTimeoutMessage() %>  ' + time.toLocaleString() + ' \n\n';
            if (cur_step_btn=="") {
                dxDialog(msg + '<%= JS_SafeString(ResString("msgTimeoutWarningAction")) %>', null, null, "<% = ResString("lblWarning") %>", 530, 300);
            }
            else {
                if (confirm(msg + '<% = JS_SafeString(ResString(CStr(IIf(isEvaluation(CurAction), "msgSaveJudgments", "msgPageRefresh")))) %>')) { 
                    SetNextStep(<% =CurStep %>);
                    var b =  eval("theForm." + cur_step_btn); 
                    if ((b)) b.click();
                }
            }
        }
    <% End If %>--%>        
        }

        function OnExitPage(is_logout) {
        <% If showT2S Then %>if (('speechSynthesis' in window)) {
                if (window.speechSynthesis.speaking) {
                    window.speechSynthesis.cancel();
                }
            }<% End If%>        
            if (!(alert_on_exit) && (show_wait)) {
            <%--var t = $find("<% =RadToolTipStepsList.ClientID %>");
            if ((t)) t.hide();
            var r = $get("tblIntermediateResults");
            if ((r)) r.style.display = "none";
            var p = $get("<% =pnlEvaluate.ClientID %>");
            if ((p)) p.style.display = "none";--%>
                showLoadingPanel();
            }
        <%--<% If Not IsReadOnly Then %>
        if ((alert_on_exit) && theForm.<% =isChanged.ClientID %>.value>'0') {
            _loadPanelPersist = false;
            hideLoadingPanel();
            return ((is_logout) ? '<% =JS_SafeString(ResString("msgExitUnsavedEvaluation")) %>' : '<% =JS_SafeString(ResString("msgUnloadUnsavedEvaluation")) %>');               
        }
        <% End If %>--%>
        }

        $('#btnQHPlayTS').click(function () {
            alert('Rahul');
        });

        $(document).ready(function () {
            window.speechSynthesis.cancel();
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            }
        });

    </script>

    <style>
        #lblTask {
            border: 0px solid #e0e0e0 !important;
            background: #ffffff !important;
            padding: 8px 4em !important;
            margin: 0px 3em 1em 3em !important;
            text-align: center !important;
        }

        .info_tooltip.right_tooltip::after {
            right: 16px;
            left: 50%;
            bottom: -7px;
            top: auto;
            transform: rotate(135deg);
            border-top-color: var(--Primary-Blue);
            border-right-color: var(--Primary-Blue);
        }

        .right_tooltip {
            position: absolute;
            top: -170px;
            min-width: 18rem;
            background: #fff;
            border: 1px solid #ccc;
            padding: 3px 8px;
            box-shadow: 0 0 5px #ccc;
            color: var(--Black);
            font-size: 12px;
            left: -10px;
            z-index: 9;
        }
    </style>

    <div class="classforeveryevent" runat="server" id="hdndivforeveryevent">
        <%--<form runat="server" class="container changeView">--%>
        <form runat="server" class="changeView">
            <asp:Button ID="hdnPageNo" runat="server" OnClick="hdnPageNo_Click" Style="display: none;" />
            <%--<asp:HiddenField ID="hdnPageNumber" runat="server" Value="0" />--%>
            <input type="hidden" id="hdnPageNumber" name="hdnPageNumber" value="0" />
            <asp:HiddenField ID="hdnOutput" runat="server" Value="" />
            <asp:HiddenField ID="hdnLeftBar" runat="server" Value="" />
            <asp:HiddenField ID="hdnRightBar" runat="server" Value="" />
            <asp:HiddenField ID="hdnWelcomeText" runat="server" Value="" />
            <asp:HiddenField ID="hdnPagePostBack" runat="server" Value="" />
            <asp:Button ID="hdnbtnWelcomeUpdate" runat="server" OnClick="hdnbtnWelcomeUpdate_Click" Style="display: none;" />

            <%-- User Controls --%>
            <uc1:Information ID="InformationControl" runat="server" Visible="false" />
            <uc1:AllPairWise ID="AllPairWiseControl" runat="server" Visible="false" />
            <uc1:Survey ID="SurveyControl" runat="server" Visible="false" />
            <uc1:LocalResults ID="LocalResultsControl" runat="server" Visible="false" />
            <uc1:GlobalResults ID="GlobalResultsControl" runat="server" Visible="false" />
            <uc1:Pairwise ID="PairwiseControl" runat="server" Visible="false" />
            <uc1:MultiRatings ID="MultiRatingsControl" runat="server" Visible="false" />
            <uc1:Ratings ID="RatingsControl" runat="server" Visible="false" />
            <uc1:DirectComparison ID="DirectComparisonControl" runat="server" Visible="false" />
            <uc1:MultiDirect ID="MultiDirectControl" runat="server" Visible="false" />
            <uc1:UtilityCurve ID="UtilityCurveControl" runat="server" Visible="false" />
            <uc1:SensitivitiesAnalysis ID="SensitivitiesAnalysisControl" runat="server" Visible="false" />
            <includes:Footer ID="footer1" runat="server" />

            <div id="divProjectErrorMsg" class="columns project-locked-message large-offset-2 large-8 medium-offset-1 medium-10 small-12 sorry_msg" runat="server" visible="false">
                <div id="divProjectError" runat="server" class="locked-message error_msg">
                </div>
            </div>

            <%--<div id="divProjectLockedInfo" class="columns project-locked-message large-offset-2 large-8 medium-offset-1 medium-10 small-12 sorry_msg" runat="server" visible="false">
                <div id="divlockedMessage" runat="server" class="locked-message"></div>
            </div>
            <div id="divinvalidtimelineInfo" class="columns project-locked-message large-offset-2 large-8 medium-offset-1 medium-10 small-12 sorry_msg" runat="server" visible="false">
                <div id="divinvalidtimelineMsg" runat="server" class="locked-message"></div>
            </div>
            <div id="divAccessDisabledInfo" class="columns project-locked-message large-offset-2 large-8 medium-offset-1 medium-10 small-12 sorry_msg" runat="server" visible="false">
                <div id="divAccessDisabledMsg" runat="server" class="locked-message"></div>
            </div>--%>
        </form>
    </div>

    <div class="modal fade" id="pasteModal" aria-hidden="true" aria-labelledby="exampleModalToggleLabel" tabindex="-1" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-md">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Paste from Clipboard</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="tt-modal-content large-12 columns">
                        <div class="row">
                            <div class="large-12 columns text-center">
                                <p id="paste-message" class="font-size-14"></p>
                            </div>
                            <div class="large-12 columns">
                                <textarea id="ClipBoardData" autofocus class="w-100 form-control" rows="5" oninput="display();"></textarea>
                            </div>
                            <div class="large-12 columns text-center">
                                <a class="button close-reveal-modal tt-button-primary success paste-message-a modal-go-btn Btndisabled" id="btngo" style="cursor: pointer" data-bs-dismiss="modal" onclick="pasteJudgmentTable()">
                                    <span class="icon-tt-check icon paste-message-icon"><i class="fa fa-check" aria-hidden="true"></i></span>
                                    <span class="text">Go</span>
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal" id="SelectStepModal" aria-hidden="true" aria-labelledby="exampleModalToggleLabel" tabindex="-1" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-md">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Welcome back</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="small-12 columns panel text-center">
                            <span class="icon-tt-info-circle"></span>
                            <span class="highlighted-text">What step would you like to start with?</span>
                        </div>
                        <div class="columns d-flex justify-content-center mt-4">
                            <div class="text-center m-2" id="divfirst_unassessed_step">
                                <a href="javascript:void(0);" id="btnfirst_unassessed_step" onclick="accesStep('first_unassessed_step');" class="bg_green btn text-white">First Unassessed</a>
                                <%--<button id="btnfirst_unassessed_step" type="button" onclick="accesStep('first_unassessed_step');" class="bg_green btn text-white">First Unassessed</button>--%>
                            </div>
                            <div class="text-center m-2" id="divcurrent_step">
                                <a href="javascript:void(0);" id="btncurrent_step" onclick="accesStep('current_step');" class="bg_green btn text-white">This Step</a>
                                <%--<button id="btncurrent_step" type="button" onclick="accesStep('current_step');" class="bg_green btn text-white">This Step</button>--%>
                            </div>
                            <div class="text-center m-2" style="display: block;" id="divfirst_step">
                                <a href="javascript:void(0);" id="btnfirst_step" onclick="accesStep('first_step');" class="bg_green btn text-white">First Step</a>
                                <%--<button id="btnfirst_step" type="button" onclick="accesStep('first_step');" class="bg_green btn text-white">First Step</button>--%>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="warningModal" aria-hidden="true" aria-labelledby="exampleModalToggleLabel" tabindex="-1" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-md">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="warningModalTitle">Information</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" onclick="closeWarningModal('warningModal')"></button>
                </div>
                <div class="modal-body">
                    <div id="warningModalMessage" runat="server">
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" id="btnCloseWarning" class="btn btn-secondary" onclick="closeWarningModal('warningModal')" data-bs-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>

    <%-- Auto Advance Modal --%>
    <div class="modal fade" id="AutoAdvanceModal" aria-hidden="true" aria-labelledby="exampleModalToggleLabel" tabindex="-1" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-md">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Confirmation</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="row m-0">
                        The auto advance feature can speed up the judgment process. You can turn it on at any time. Do you wish to turn it on now?
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" data-bs-dismiss="modal" onclick="set_auto_advance(true)">Yes</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">No</button>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade seconday-anytime-popup" id="GlobalInfoDocModal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl my-3">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLongTitle">Edit content</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">

                    <table border="0" cellspacing="0" cellpadding="0" style="width: 100%; height: 99%">
                        <tr runat="server" id="trHead">
                            <td rowspan="2" style="width: 1px" runat="server" id="tdColLeft" visible="false">
                                <div runat="server" id="lblColLeft" visible="false"></div>
                            </td>
                            <%-- <td valign="middle" align="center" runat="server" id="tdTask" class="eval_task"><div runat="server" id="divEditTask" visible="false">
                        <input type='hidden' name='SaveNewTask' id='SaveNewTask' value='0' /><input type='hidden' name='TaskNodeGUID' id='TaskNodeGUID' value='<% =TaskNodeGUID %>' /><input type='hidden' id='TaskNodesList' name='TaskNodesList' value='<% =CurStep %>' />
                        <input type='button' name='btnTaskEditor' id='btnTaskEditor' value='<% =ResString("btnEdit") %><% = iif(ClusterPhraseIsCustom, "*", "") %>' class='button button_small<% =iif(ClusterPhraseIsCustom, " button_evaluate_undefined", "") %>' style='float:right; margin:6px 0px 6px 1em; width:6em; padding:2px;' onclick='SwitchTaskEditor(0); return false;' <% =iif(TaskNodeGUID<>"", "", "disabled=1") %> />
                        <div id='divClusterPhrase' style='display:none;'><textarea name='txtTask' id='txtTask' class='textarea' cols='65' rows='3' style='width:950px; height:6.5em;'><% = SafeFormString(ClusterPhrase) %></textarea><telerik:RadToolTip ID="RadToolTipHints" runat="server" HideEvent="ManualClose" ShowEvent="FromCode" ShowDelay="50" Position="BottomCenter" RelativeTo="Mouse" TargetControlID="divClusterPhrase" OffsetY="-6" OffsetX="-32" IsClientID="true" /><telerik:RadToolTip ID="RadToolTipClusterSteps" runat="server" HideEvent="ManualClose" ShowEvent="FromCode" ShowDelay="50" Position="BottomCenter" RelativeTo="Mouse" TargetControlID="divClusterPhrase" OffsetY="-6" OffsetX="-32" IsClientID="true" /></div>
                    </div><asp:Label ID="lblTask" runat="server"/><span id="divT2S" style="font-size:20px; margin-left:1ex; display:none"></span><input type="hidden" id="txtEvalObject" name="txtEvalObject" value="" /></td>
                    <td rowspan="2" style="width:1px" runat="server" id="tdColRight" visible="false"><div runat="server" id="lblColRight" visible="false"></div></td>--%>
                        </tr>
                    </table>
                    <textarea id="GlobalInfoDocValue" name="GlobalInfoDocValue" rows="7"></textarea>
                    <%--      <script type="text/javascript" lang="javascript">
                        //CKEDITOR.replace('GlobalInfoDocValue');
                        CKEDITOR.replace('GlobalInfoDocValue',
                            {
                                //toolbar: 'Basic', / this does the magic /
                                uiColor: '#9AB8F3',
                                height: 300,
                                allowedContent: true
                            });
                    </script>--%>
                    <div runat="server" id="saveStatus" class="tt-GlobalInfoDoc-status"></div>
                </div>
                <div class="modal-footer">
                    <%--<button type="button" id="btnmodelClose" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>--%>
                    <button type="button" id="btnmodelSave" onclick="saveContent()" class="btn btn-primary">Save changes</button>
                </div>
            </div>
        </div>
    </div>
 
    <div class="modal fade" id="GlobalInfoDocModal1" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl my-3">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLongTitle1">Edit Header Content</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <table border="0" cellspacing="0" cellpadding="0" style="width: 100%; height: 99%">
                        <tr runat="server" id="tr1">
                            <td rowspan="2" style="width: 1px" runat="server" id="td1" visible="false">
                                <div runat="server" id="Div2" visible="false"></div>
                            </td>
                        </tr>
                    </table>
                    <textarea id="GlobalInfoDocValue1" name="GlobalInfoDocValue1" rows="7"></textarea>
                    <div runat="server" id="Div3" class="tt-GlobalInfoDoc-status"></div>

                </div>
                <div class="modal-footer">
                    <%--<button type="button" id="btnapplySavecluster" class="btn btn-primary">Apply to cluster(s)...</button>--%>
                    <%--<button type="button" id="btnmodelClose1" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>--%>
                    <button type="button" id="btnmodelSave1" onclick="saveHeaderContent()" class="btn btn-primary">Save changes</button>
                </div>
            </div>
        </div>
    </div>
        
    <div class="modal fade" id="GlobalInfoDocHeaderPopupModal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl my-3">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLongTitle2">Header Content</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body" id="headerinfodocpopupbody">               
                </div>
              <%--  <div class="modal-footer">
                    <%--<button type="button" id="btnapplySavecluster" class="btn btn-primary">Apply to cluster(s)...</button>--%>
                <%--    <button type="button" id="btnmodelClosePopup" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>                
                </div>--%>
            </div>
        </div>
    </div>
   
    <%-- Quick help modal --%>
    <div class="modal fade" id="quickHelpmodal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalTitle">Quick Help</h5>
                    <button id="btnClose" onclick="CloseVideoOnPopupClose();" type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    <%--<button type="button" class="close popup_close" data-bs-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>--%>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="anytime-pages comparion-text  large-12 columns qh-parent-div" style="height: 100%;">
                            <span id="quick-help-content"></span><%--ng-if="qh_text != ''"--%>
                            <span id="no-quick-help-content">No Quick Help yet.</span> <%--ng-if="qh_text == ''"--%>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <div class="row w-100">
                        <div class="col-lg-5 p-0">
                            <div class="align-items-center d-flex">
                                <input type='checkbox' class='collabse_arrow' id="dont_show_qh" onchange="show_qh_automatically()">
                                <label id="lbl_dont_show_qh" for="dont_show_qh" class="info hide-for-small mb-0 me-3">Don't automatically show Quick Help</label>
                                <div class="dx-item dx-toolbar-item dx-toolbar-button">
                                    <div class="dx-item-content dx-toolbar-item-content">
                                        <div class="dx-widget dx-button dx-button-mode-contained dx-button-normal dx-button-has-icon dx-btn-no-minwidth" id="btnQHPlayTS" role="button" title="Start/pause text to speech" tabindex="0">
                                            <div class="dx-button-content">
                                                <a href="" onclick='doQuickHelpT2S(); return false;' class='actions'><i class="dx-icon fas fa-play" id='btnT2SPlayQuickHelp'></i></a>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="dx-item dx-toolbar-item dx-toolbar-button">
                                    <div class="dx-item-content dx-toolbar-item-content">
                                        <div class="dx-widget dx-button dx-button-mode-contained dx-button-normal dx-button-has-icon dx-btn-no-minwidth" id="btnQHStopTS" role="button" aria-label="fas fa-stop" aria-disabled="true" title="Stop text to speech">
                                            <div class="dx-button-content">
                                                <a href="" onclick="doQuuickHelpT2SStop(); return false;">
                                                    <i class="dx-icon fas fa-stop"></i>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <%-- <div class="col-lg-6 text-center">
                            
                        </div>--%>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Edit quick help modal --%>
    <div class="modal fade" id="EditquickHelpmodal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalTitle">Edit Quick Help</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <textarea id="qh_info_value" name="qh_info_value" rows="7"></textarea>

                    <%--       <script type="text/javascript" lang="javascript">
                        //CKEDITOR.replace('GlobalInfoDocValue');
                        CKEDITOR.replace('qh_info_value',
                            {
                                //toolbar: 'Basic', / this does the magic /
                                uiColor: '#9AB8F3',
                                height: 300,
                                allowedContent: true
                            });
                    </script>    --%>

                    <script src="../../TxtEditor/tinymce/tinymce.min.js"></script>
                    <%--              <script src="https://cdn.tiny.cloud/1/k9ofrmqwtpfkxhxbnawfwrpi7fm58hp3kuo39mw6fb3ic7rk/tinymce/6/tinymce.min.js" referrerpolicy="origin"></script>--%>

                    <script type="text/javascript">
                        var fonts = "Andale Mono=andale mono,times;" + "Arial=arial,helvetica,sans-serif;" + "Arial Black=arial black,avant garde;" + "Book Antiqua=book antiqua,palatino;" + "Comic Sans MS=comic sans ms,sans-serif;" + "Courier New=courier new,courier;" + "Georgia=georgia,palatino;" + "Helvetica=helvetica;" + "Impact=impact,chicago;" + "Symbol=symbol;" + "Tahoma=tahoma,arial,helvetica,sans-serif;" + "Terminal=terminal,monaco;" + "Times New Roman=times new roman,times;" + "Trebuchet MS=trebuchet ms,geneva;" + "Verdana=verdana,geneva;" + "Webdings=webdings;" + "Wingdings=wingdings,zapf dingbats";
                        var font_sizes = "8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 24pt 36pt";
                        var colors = [
                            "000000", "Black",
                            "993300", "Burnt orange",
                            "333300", "Dark olive",
                            "003300", "Dark green",
                            "003366", "Dark azure",
                            "000080", "Navy Blue",
                            "333399", "Indigo",
                            "333333", "Very dark gray",
                            "800000", "Maroon",
                            "FF6600", "Orange",
                            "808000", "Olive",
                            "008000", "Green",
                            "008080", "Teal",
                            "0000FF", "Blue",
                            "666699", "Grayish blue",
                            "808080", "Gray",
                            "FF0000", "Red",
                            "FF9900", "Amber",
                            "99CC00", "Yellow green",
                            "339966", "Sea green",
                            "33CCCC", "Turquoise",
                            "3366FF", "Royal blue",
                            "800080", "Purple",
                            "999999", "Medium gray",
                            "FF00FF", "Magenta",
                            "FFCC00", "Gold",
                            "FFFF00", "Yellow",
                            "00FF00", "Lime",
                            "00FFFF", "Aqua",
                            "00CCFF", "Sky blue",
                            "993366", "Red violet",
                            "FFFFFF", "White",
                            "FF99CC", "Pink",
                            "FFCC99", "Peach",
                            "FFFF99", "Light yellow",
                            "CCFFCC", "Pale green",
                            "CCFFFF", "Pale cyan",
                            "99CCFF", "Light sky blue",
                            "CC99FF", "Plum"
                        ];

                        // Prevent Bootstrap dialog from blocking focusin
                        document.addEventListener('focusin', function (e) { if (e.target.closest('.tox-tinymce-aux, .moxman-window, .tam-assetmanager-root') !== null) { e.stopImmediatePropagation(); } });
                        //var useDarkMode = (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'oxide' : 'default');
                        tinymce.init({
                            selector: 'textarea#GlobalInfoDocValue',
                            plugins: 'print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons',
                            imagetools_cors_hosts: ['picsum.photos'],
                            menubar: 'file edit view insert format tools table help',
                            /*   toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads',*/
                            toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads | paste',
                            toolbar_sticky: true,
                            autosave_ask_before_unload: false,
                            autosave_interval: '30s',
                            autosave_prefix: '{path}{query}-{id}-',
                            autosave_restore_when_empty: false,
                            force_p_newlines: false,
                            forced_root_block: '',
                            remove_trailing_brs: false,
                            autosave_retention: '2m',
                            image_advtab: true,
                            paste_block_drop: true,
                            paste_as_text: true,
                            link_list: [
                                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                                { title: 'My page 2', value: 'http://www.moxiecode.com' }
                            ],
                            image_list: [
                                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                                { title: 'My page 2', value: 'http://www.moxiecode.com' }
                            ],
                            image_class_list: [
                                { title: 'None', value: '' },
                                { title: 'Some class', value: 'class-name' }
                            ],
                            importcss_append: true,
                            /* enable title field in the Image dialog*/
                            image_title: true,
                            /* enable automatic uploads of images represented by blob or data URIs*/
                            automatic_uploads: true,
                            /*
                              URL of our upload handler (for more details check: https://www.tiny.cloud/docs/configure/file-image-upload/#images_upload_url)
                              images_upload_url: 'postAcceptor.php',
                              here we add custom filepicker only to Image dialog
                            */
                            file_picker_types: "image",
                            /* and here's our custom image picker*/
                            file_picker_callback: function (cb, value, meta) {
                                ///* Provide file and text for the link dialog */
                                //if (meta.filetype === 'file') {
                                //    callback('https://www.google.com/logos/google.jpg', { text: 'My text' });
                                //}

                                ///* Provide image and alt text for the image dialog */
                                //if (meta.filetype === 'image') {
                                //    callback('https://www.google.com/logos/google.jpg', { alt: 'My alt text' });
                                //}

                                ///* Provide alternative source and posted for the media dialog */
                                //if (meta.filetype === 'media') {
                                //    callback('movie.mp4', { source2: 'alt.ogg', poster: 'https://www.google.com/logos/google.jpg' });
                                //}

                                var input = document.createElement("input");
                                input.setAttribute("type", "file");
                                input.setAttribute("accept", "image/*");

                                /*
                                  Note: In modern browsers input[type="file"] is functional without
                                  even adding it to the DOM, but that might not be the case in some older
                                  or quirky browsers like IE, so you might want to add it to the DOM
                                  just in case, and visually hide it. And do not forget do remove it
                                  once you do not need it anymore.
                                */

                                input.onchange = function () {
                                    var file = this.files[0];

                                    var reader = new FileReader();
                                    reader.onload = function () {
                                        /*
                                          Note: Now we need to register the blob in TinyMCEs image blob
                                          registry. In the next release this part hopefully won't be
                                          necessary, as we are looking to handle it internally.
                                        */
                                        var id = "blobid" + (new Date()).getTime();
                                        var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                                        var base64 = reader.result.split(",")[1];
                                        var blobInfo = blobCache.create(id, file, base64);
                                        blobCache.add(blobInfo);

                                        /* call the callback and populate the Title field with the file name */
                                        cb(blobInfo.blobUri(), { title: file.name });
                                    };
                                    reader.readAsDataURL(file);
                                };

                                input.click();
                            },
                            templates: [
                                { title: 'New Table', description: 'creates a new table', content: '<div class="mceTmpl"><table width="98%%"  border="0" cellspacing="0" cellpadding="0"><tr><th scope="col"> </th><th scope="col"> </th></tr><tr><td> </td><td> </td></tr></table></div>' },
                                { title: 'Starting my story', description: 'A cure for writers block', content: 'Once upon a time...' },
                                { title: 'New list with dates', description: 'New List with dates', content: '<div class="mceTmpl"><span class="cdate">cdate</span><br /><span class="mdate">mdate</span><h2>My List</h2><ul><li></li><li></li></ul></div>' }
                            ],
                            template_cdate_format: '[Date Created (CDATE): %m/%d/%Y : %H:%M:%S]',
                            template_mdate_format: '[Date Modified (MDATE): %m/%d/%Y : %H:%M:%S]',
                            height: 250,
                            image_caption: true,
                            quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
                            noneditable_noneditable_class: 'mceNonEditable',
                            toolbar_mode: 'sliding',
                            contextmenu: 'link image imagetools table',
                            skin: 'oxide',// useDarkMode ? 'oxide-dark' : 'oxide',
                            content_css: 'default', // useDarkMode ? 'dark' : 'default',
                            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
                            setup: function (editor) {
                                editor.ui.registry.addButton('Download', {
                                    icon: 'save',
                                    tooltip: "Download",
                                    onAction: function () {
                                        var textToSave = tinymce.get(editor.id).getContent();
                                        var textToSaveAsBlob = new Blob([textToSave], { type: "text/plain" });
                                        var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
                                        var projectname = $.cookie('OpenProjectName');
                                        var fileNameToSaveAs = projectname.substring(projectname.indexOf("=") + 1) + "_" + hdnpage_type + ".mht";
                                        var downloadLink = document.createElement("a");
                                        downloadLink.download = fileNameToSaveAs;
                                        downloadLink.innerHTML = "Download File";
                                        downloadLink.href = textToSaveAsURL;
                                        downloadLink.onclick = destroyClickedElement;
                                        downloadLink.style.display = "none";
                                        document.body.appendChild(downloadLink);
                                        downloadLink.click();
                                    }
                                })
                                editor.ui.registry.addButton('Uploads', {
                                    icon: 'upload',
                                    tooltip: "Upload",
                                    onAction: function () {
                                        tinymce.activeEditor.windowManager.open({
                                            title: 'File Upload', // The dialog's title - displayed in the dialog header
                                            body: {
                                                type: 'panel', // The root body type - a Panel or TabPanel
                                                items: [ // A list of panel components
                                                    {
                                                        type: 'htmlpanel', // A HTML panel component
                                                        html: '<div> <input type="file" id="myFile" accept=".mht,.txt" "><input type="button" class="clsupload" value="Upload" onclick="uploadfile()"></div>'
                                                    }
                                                ]
                                            },
                                            buttons: [ // A list of footer buttons
                                                {
                                                    type: 'cancel',
                                                    text: 'Cancel'
                                                }
                                            ]
                                        });
                                    }
                                })
                            }
                        });

                        tinymce.init({
                            //textcolor_map: colors,
                            selector: 'textarea#GlobalInfoDocValue1',
                            theme: "silver",
                            //font_formats: fonts,
                            //fontsize_formats: font_sizes,
                            plugins: 'print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons image code',
                            /*imagetools_cors_hosts: ['picsum.photos'],*/
                            imagetools_cors_hosts: ["www.tinymce.com", "codepen.io"],
                            menubar: 'file edit view insert format tools table help',
                            /*   toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads',*/
                            toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads | AddVariable ResetDefault ApplyChanges ApplyTo btncancel | link image | code',
                            toolbar_sticky: true,
                            autosave_ask_before_unload: false,
                            autosave_interval: '30s',
                            autosave_prefix: '{path}{query}-{id}-',
                            autosave_restore_when_empty: false,
                            force_p_newlines: false,
                            forced_root_block: '',
                            remove_trailing_brs: false,
                            autosave_retention: '2m',
                            image_advtab: true,
                            link_list: [
                                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                                { title: 'My page 2', value: 'http://www.moxiecode.com' }
                            ],
                            image_list: [
                                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                                { title: 'My page 2', value: 'http://www.moxiecode.com' }
                            ],
                            image_class_list: [
                                { title: 'None', value: '' },
                                { title: 'Some class', value: 'class-name' }
                            ],
                            importcss_append: true,
                            /* enable title field in the Image dialog*/
                            image_title: true,
                            /* enable automatic uploads of images represented by blob or data URIs*/
                            automatic_uploads: true,
                            /*
                              URL of our upload handler (for more details check: https://www.tiny.cloud/docs/configure/file-image-upload/#images_upload_url)
                              images_upload_url: 'postAcceptor.php',
                              here we add custom filepicker only to Image dialog
                            */
                            file_picker_types: "image",
                            /* and here's our custom image picker*/
                            file_picker_callback: function (cb, value, meta) {
                                ///* Provide file and text for the link dialog */
                                //if (meta.filetype === 'file') {
                                //    callback('https://www.google.com/logos/google.jpg', { text: 'My text' });
                                //}

                                ///* Provide image and alt text for the image dialog */
                                //if (meta.filetype === 'image') {
                                //    callback('https://www.google.com/logos/google.jpg', { alt: 'My alt text' });
                                //}

                                ///* Provide alternative source and posted for the media dialog */
                                //if (meta.filetype === 'media') {
                                //    callback('movie.mp4', { source2: 'alt.ogg', poster: 'https://www.google.com/logos/google.jpg' });
                                //}

                                var input = document.createElement("input");
                                input.setAttribute("type", "file");
                                input.setAttribute("accept", "image/*");

                                /*
                                  Note: In modern browsers input[type="file"] is functional without
                                  even adding it to the DOM, but that might not be the case in some older
                                  or quirky browsers like IE, so you might want to add it to the DOM
                                  just in case, and visually hide it. And do not forget do remove it
                                  once you do not need it anymore.
                                */

                                input.onchange = function () {
                                    var file = this.files[0];

                                    var reader = new FileReader();
                                    reader.onload = function () {
                                        /*
                                          Note: Now we need to register the blob in TinyMCEs image blob
                                          registry. In the next release this part hopefully won't be
                                          necessary, as we are looking to handle it internally.
                                        */
                                        var id = "blobid" + (new Date()).getTime();
                                        var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                                        var base64 = reader.result.split(",")[1];
                                        var blobInfo = blobCache.create(id, file, base64);
                                        blobCache.add(blobInfo);

                                        /* call the callback and populate the Title field with the file name */
                                        cb(blobInfo.blobUri(), { title: file.name });
                                    };
                                    reader.readAsDataURL(file);
                                };

                                input.click();
                            },
                            templates: [
                                { title: 'New Table', description: 'creates a new table', content: '<div class="mceTmpl"><table width="98%%"  border="0" cellspacing="0" cellpadding="0"><tr><th scope="col"> </th><th scope="col"> </th></tr><tr><td> </td><td> </td></tr></table></div>' },
                                { title: 'Starting my story', description: 'A cure for writers block', content: 'Once upon a time...' },
                                { title: 'New list with dates', description: 'New List with dates', content: '<div class="mceTmpl"><span class="cdate">cdate</span><br /><span class="mdate">mdate</span><h2>My List</h2><ul><li></li><li></li></ul></div>' }
                            ],
                            template_cdate_format: '[Date Created (CDATE): %m/%d/%Y : %H:%M:%S]',
                            template_mdate_format: '[Date Modified (MDATE): %m/%d/%Y : %H:%M:%S]',
                            height: 250,
                            image_caption: true,
                            quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
                            noneditable_noneditable_class: 'mceNonEditable',
                            toolbar_mode: 'sliding',
                            contextmenu: 'link image imagetools table',
                            skin: 'oxide',// useDarkMode ? 'oxide-dark' : 'oxide',
                            content_css: 'default', // useDarkMode ? 'dark' : 'default',
                            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
                            setup: function (editor) {
                                editor.ui.registry.addButton('Download', {
                                    icon: 'save',
                                    tooltip: "Download",
                                    onAction: function () {
                                        var textToSave = tinymce.get(editor.id).getContent();
                                        var textToSaveAsBlob = new Blob([textToSave], { type: "text/plain" });
                                        var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
                                        var projectname = $.cookie('OpenProjectName');
                                        var fileNameToSaveAs = projectname.substring(projectname.indexOf("=") + 1) + "_" + hdnpage_type + ".mht";
                                        var downloadLink = document.createElement("a");
                                        downloadLink.download = fileNameToSaveAs;
                                        downloadLink.innerHTML = "Download File";
                                        downloadLink.href = textToSaveAsURL;
                                        downloadLink.onclick = destroyClickedElement;
                                        downloadLink.style.display = "none";
                                        document.body.appendChild(downloadLink);
                                        downloadLink.click();
                                    }
                                })
                                editor.ui.registry.addButton('Uploads', {
                                    icon: 'upload',
                                    tooltip: "Upload",
                                    onAction: function () {
                                        tinymce.activeEditor.windowManager.open({
                                            title: 'File Upload', // The dialog's title - displayed in the dialog header
                                            body: {
                                                type: 'panel', // The root body type - a Panel or TabPanel
                                                items: [ // A list of panel components
                                                    {
                                                        type: 'htmlpanel', // A HTML panel component
                                                        html: '<div> <input type="file" id="myFile" accept=".mht,.txt" "><input type="button" class="clsupload" value="Upload" onclick="uploadfile()"></div>'
                                                    }
                                                ]
                                            },
                                            buttons: [ // A list of footer buttons
                                                {
                                                    type: 'cancel',
                                                    text: 'Cancel'
                                                }
                                            ]
                                        });
                                    }
                                })
                                editor.ui.registry.addButton('AddVariable', {
                                    text: "Variables...",
                                    tooltip: "Add Variable",
                                    onAction: function () {
                                        tinymce.activeEditor.windowManager.open({
                                            title: 'Add Variable', // The dialog's title - displayed in the dialog header
                                            body: {
                                                type: 'panel', // The root body type - a Panel or TabPanel
                                                items: [ // A list of panel components
                                                    {
                                                        type: 'htmlpanel', // A HTML panel component
                                                        /*html: '<div>' + $.cookie('PopupBodyStr') + '</div>'*/
                                                        html: '<div>' + $.cookie('CVariable') + '</div><div style="margin-top:1em">Click on a variable to add the variable template</div>'
                                                    }
                                                ]
                                            },
                                            buttons: [ // A list of footer buttons
                                                {
                                                    type: 'cancel',
                                                    text: 'Cancel'
                                                }
                                            ]
                                        });
                                    }
                                })
                                editor.ui.registry.addButton('ResetDefault', {
                                    text: "Reset to default",
                                    tooltip: "Reset to default",
                                    onAction: function () {
                                        //alert("Reset to default");
                                        ConfrimResetTask();
                                    }
                                })
                                editor.ui.registry.addButton('ApplyChanges', {
                                    text: "Apply changes",
                                    tooltip: "Apply changes",
                                    onAction: function () {
                                        //alert("Apply changes");
                                        saveHeaderContent();
                                    }
                                })
                                editor.ui.registry.addButton('ApplyTo', {
                                    text: "Apply to...",
                                    tooltip: "Apply to...",
                                    onAction: function () {
                                        tinymce.activeEditor.windowManager.open({
                                            title: 'Apply to...', // The dialog's title - displayed in the dialog header
                                            body: {
                                                type: 'panel', // The root body type - a Panel or TabPanel
                                                items: [ // A list of panel components
                                                    {
                                                        type: 'htmlpanel', // A HTML panel component                                                 
                                                        html: '<div>' + GetHtmlData() + '</div>'
                                                        /* html: '<div><input type="button" class="clsupload" value="Upload" onclick="uploadfile()"></div>'*/
                                                    }
                                                ]
                                            },
                                            buttons: [ // A list of footer buttons
                                                {
                                                    type: 'cancel',
                                                    text: 'Cancel'
                                                }
                                            ]
                                        });
                                    }
                                })

                                editor.ui.registry.addButton('btncancel', {
                                    text: "Cancel",
                                    tooltip: "Cancel",
                                    onAction: function () {
                                        //alert("Apply changes");
                                        HideAndCloseEditor();
                                    }
                                })
                            }
                        });


                        tinymce.init({
                            selector: 'textarea#qh_info_value',
                            plugins: 'print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons',
                            imagetools_cors_hosts: ['picsum.photos'],
                            menubar: 'file edit view insert format tools table help',
                            /*   toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads',*/
                            toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads ',
                            toolbar_sticky: true,
                            autosave_ask_before_unload: false,
                            autosave_interval: '30s',
                            autosave_prefix: '{path}{query}-{id}-',
                            autosave_restore_when_empty: false,
                            force_p_newlines: false,
                            forced_root_block: '',
                            remove_trailing_brs: false,
                            autosave_retention: '2m',
                            image_advtab: true,
                            link_list: [
                                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                                { title: 'My page 2', value: 'http://www.moxiecode.com' }
                            ],
                            image_list: [
                                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                                { title: 'My page 2', value: 'http://www.moxiecode.com' }
                            ],
                            image_class_list: [
                                { title: 'None', value: '' },
                                { title: 'Some class', value: 'class-name' }
                            ],
                            importcss_append: true,
                            /* enable title field in the Image dialog*/
                            image_title: true,
                            /* enable automatic uploads of images represented by blob or data URIs*/
                            automatic_uploads: true,
                            /*
                              URL of our upload handler (for more details check: https://www.tiny.cloud/docs/configure/file-image-upload/#images_upload_url)
                              images_upload_url: 'postAcceptor.php',
                              here we add custom filepicker only to Image dialog
                            */
                            file_picker_types: "image",
                            /* and here's our custom image picker*/
                            file_picker_callback: function (cb, value, meta) {
                                ///* Provide file and text for the link dialog */
                                //if (meta.filetype === 'file') {
                                //    callback('https://www.google.com/logos/google.jpg', { text: 'My text' });
                                //}

                                ///* Provide image and alt text for the image dialog */
                                //if (meta.filetype === 'image') {
                                //    callback('https://www.google.com/logos/google.jpg', { alt: 'My alt text' });
                                //}

                                ///* Provide alternative source and posted for the media dialog */
                                //if (meta.filetype === 'media') {
                                //    callback('movie.mp4', { source2: 'alt.ogg', poster: 'https://www.google.com/logos/google.jpg' });
                                //}

                                var input = document.createElement("input");
                                input.setAttribute("type", "file");
                                input.setAttribute("accept", "image/*");

                                /*
                                  Note: In modern browsers input[type="file"] is functional without
                                  even adding it to the DOM, but that might not be the case in some older
                                  or quirky browsers like IE, so you might want to add it to the DOM
                                  just in case, and visually hide it. And do not forget do remove it
                                  once you do not need it anymore.
                                */

                                input.onchange = function () {
                                    var file = this.files[0];

                                    var reader = new FileReader();
                                    reader.onload = function () {
                                        /*
                                          Note: Now we need to register the blob in TinyMCEs image blob
                                          registry. In the next release this part hopefully won't be
                                          necessary, as we are looking to handle it internally.
                                        */
                                        var id = "blobid" + (new Date()).getTime();
                                        var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                                        var base64 = reader.result.split(",")[1];
                                        var blobInfo = blobCache.create(id, file, base64);
                                        blobCache.add(blobInfo);

                                        /* call the callback and populate the Title field with the file name */
                                        cb(blobInfo.blobUri(), { title: file.name });
                                    };
                                    reader.readAsDataURL(file);
                                };

                                input.click();
                            },
                            templates: [
                                { title: 'New Table', description: 'creates a new table', content: '<div class="mceTmpl"><table width="98%%"  border="0" cellspacing="0" cellpadding="0"><tr><th scope="col"> </th><th scope="col"> </th></tr><tr><td> </td><td> </td></tr></table></div>' },
                                { title: 'Starting my story', description: 'A cure for writers block', content: 'Once upon a time...' },
                                { title: 'New list with dates', description: 'New List with dates', content: '<div class="mceTmpl"><span class="cdate">cdate</span><br /><span class="mdate">mdate</span><h2>My List</h2><ul><li></li><li></li></ul></div>' }
                            ],
                            template_cdate_format: '[Date Created (CDATE): %m/%d/%Y : %H:%M:%S]',
                            template_mdate_format: '[Date Modified (MDATE): %m/%d/%Y : %H:%M:%S]',
                            height: 250,
                            image_caption: true,
                            quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
                            noneditable_noneditable_class: 'mceNonEditable',
                            toolbar_mode: 'sliding',
                            contextmenu: 'link image imagetools table',
                            skin: 'oxide',// useDarkMode ? 'oxide-dark' : 'oxide',
                            content_css: 'default', // useDarkMode ? 'dark' : 'default',
                            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
                            setup: function (editor) {
                                editor.ui.registry.addButton('Download', {
                                    icon: 'save',
                                    tooltip: "Download",
                                    onAction: function () {
                                        var textToSave = tinymce.get(editor.id).getContent();
                                        var textToSaveAsBlob = new Blob([textToSave], { type: "text/plain" });
                                        var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
                                        var projectname = $.cookie('OpenProjectName');
                                        var fileNameToSaveAs = projectname.substring(projectname.indexOf("=") + 1) + "_" + hdnpage_type + ".mht";
                                        var downloadLink = document.createElement("a");
                                        downloadLink.download = fileNameToSaveAs;
                                        downloadLink.innerHTML = "Download File";
                                        downloadLink.href = textToSaveAsURL;
                                        downloadLink.onclick = destroyClickedElement;
                                        downloadLink.style.display = "none";
                                        document.body.appendChild(downloadLink);
                                        downloadLink.click();
                                    }
                                })
                                editor.ui.registry.addButton('Uploads', {
                                    icon: 'upload',
                                    tooltip: "Upload",
                                    onAction: function () {
                                        tinymce.activeEditor.windowManager.open({
                                            title: 'File Upload', // The dialog's title - displayed in the dialog header
                                            body: {
                                                type: 'panel', // The root body type - a Panel or TabPanel
                                                items: [ // A list of panel components
                                                    {
                                                        type: 'htmlpanel', // A HTML panel component
                                                        html: '<div> <input type="file" id="myFile" accept=".mht,.txt" "><input type="button" class="clsupload" value="Upload" onclick="uploadfile()"></div>'
                                                    }
                                                ]
                                            },
                                            buttons: [ // A list of footer buttons
                                                {
                                                    type: 'cancel',
                                                    text: 'Cancel'
                                                }
                                            ]
                                        });
                                    }
                                })
                            }
                        });

                        function HideAndCloseEditor() {
                            $('#MainContent_GlobalInfoDocModal1').modal('hide');
                            return false;
                        }
                        function destroyClickedElement(event) {
                            document.body.removeChild(event.target);
                        }
                        function uploadfile() {
                            var fileToLoad = document.getElementById("myFile");
                            /*      if (document.getElementById("myFile").files.length == 0) {*/
                            if (fileToLoad.files.length == 0) {
                                alert("Please select file");
                                return false;
                            }
                            var editorid = tinymce.activeEditor.id;
                            var fileextention = fileToLoad.value.split('.')[1];
                            if (fileextention == 'mht' || fileextention == 'txt') {
                                // var fileToLoad = document.getElementById("fileToLoad").files[0];  
                                var fileReader = new FileReader();
                                fileReader.onload = function (fileLoadedEvent) {
                                    var textFromFileLoaded = fileLoadedEvent.target.result;
                                    tinymce.get(editorid).setContent(textFromFileLoaded);
                                };
                                fileReader.readAsText(fileToLoad.files[0], "UTF-8");
                                tinymce.activeEditor.windowManager.close();
                            }
                            else {
                                alert("Please Select .mht or .txt file.");
                                return false;
                            }

                        }
                        function InsertText(txtid) {
                            const lnkbut = document.getElementById('InsertlnkID' + txtid);
                            var txt = "%%" + lnkbut.textContent + "%%";
                            var editorid = tinymce.activeEditor.id;
                            tinyMCE.get(editorid).execCommand('mceInsertContent', true, txt)
                            tinymce.activeEditor.windowManager.close();
                        }
                    </script>



                    <div class="row">
                        <div class="mt-3">
                            <a href="javascript:void(0);" onclick="set_qe_content();" class="text_bold" id="qhSample">[Use quick help sample]</a>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <div class="row w-100">
                        <div class="col-lg-6">
                            <input type='checkbox' class='collabse_arrow' id="show_qh_automatically">
                            <label for="show_qh_automatically" class="info hide-for-small">Show Quick Help Automatically</label>
                        </div>
                        <div class="col-lg-6 text-end">
                            <%--<button type="button" id="btnEqhClose" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>--%>
                            <button type="button" id="btnEqhSave" onclick="save_quick_help();" class="btn btn-primary">Save for this step</button>
                            <button type="button" id="btneditSavecluster" class="btn btn-primary">Save for cluster(s)...</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- cluster apply modal wrap  --%>
    <div class="modal fade w-100" id="ApplyEditSaveclustermodal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="saveapplyclustermodalTitle">Save for cluster(s)...</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="row w-100">
                    <div id="divapplychkboxclusterapply">
                    </div>
                </div>
                <div runat="server" id="Div1" class="tt-GlobalInfoDoc-status"></div>
                <div class="modal-footer">
                    <div class="row w-100">
                        <div class="col-lg-6 text-end">
                            <button type="button" id="btnapplyclustermodelSave" class="btn btn-primary" onclick="saveHeaderContentBy_Cluster();">Save</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- cluster modal wrap  --%>
    <div class="modal fade w-100" id="EditSaveclustermodal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="saveclustermodalTitle">Save for cluster(s)...</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="row w-100">
                    <div id="divchkboxcluster">
                    </div>
                </div>
                <div class="modal-footer">
                    <div class="row w-100">
                        <div class="col-lg-6 text-end">
                            <%--<button type="button" id="btnclustermodelClose" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>--%>
                            <button type="button" id="btnclustermodelSave" class="btn btn-primary" onclick="save_by_cluster();">Save</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- dropdown for editor --%>
    <div class="modal fade w-100 " id="variablesdivModal" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Add Variable</h5>
                </div>
                <div class="row w-100">
                    <div id="divaddvariable">
                    </div>
                </div>
                <div class="modal-footer">
                    <div class="row w-100">
                        <div class="col-lg-6 text-end">
                            Click on a variable to add the variable template
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal fade infodocmodal" id="exampleModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-fullscreen">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title" id="exampleModalLabel">INFODOCS</h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="row h-100">
                        <div class="col-md-6 border-end" id="Infodocsidedata">
                            <div class="modal_info_header">
                                <h3 id="headerinfodoc"></h3>
                                <a href="#" id="infodoc_a" onclick="">
                                    <img class="chkEvaluatorone" src="../../img/icon/edit-icon.svg">
                                </a>

                            </div>
                            <div class="modal_info_content">
                                <p id="TextInfo">.</p>
                            </div>

                        </div>
                        <div class="col-md-6 " id="infodocwrtsidedata">
                            <div class="modal_info_header">
                                <h3 id="HeaderWRT"></h3>
                                <a href="#" id="WRT_a" onclick="">
                                    <img class="chkEvaluatorone" src="../../img/icon/edit-icon.svg">
                                </a>

                            </div>
                            <div class="modal_info_content">
                                <p id="TextWRT"></p>
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="infodocPop" tabindex="-1" aria-labelledby="exampleModalHeaderLabel" aria-hidden="true">
        <div class="modal-dialog modal-fullscreen">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title" id="exampleModalHeaderLabel">INFODOCS</h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="row h-100">
                        <div class="col-md-12">
                            <div class="modal_info_header">
                                <h3 id="h_headertxt"></h3>
                                <a href="javascript:void(0);" id="h_a" onclick="">
                                    <img class="chkEvaluatorone" src="../../img/icon/edit-icon.svg">
                                </a>

                            </div>
                            <div class="modal_info_content">
                                <p id="HText">.</p>
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="PopupModalWelcomeEndPage" tabindex="-1" aria-labelledby="exampleModalToggleLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-dialog-centered modal-xl my-3">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLongTitleWelcome">Edit Header Content</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <table border="0" cellspacing="0" cellpadding="0" style="width: 100%; height: 99%">
                        <tr runat="server" id="tr2">
                            <td rowspan="2" style="width: 1px" runat="server" id="td2" visible="false">
                                <div runat="server" id="Div5" visible="false"></div>
                            </td>
                        </tr>
                    </table>
                    <textarea id="txtWelcome_EndPage" name="txtWelcome_EndPage" rows="7"></textarea>
                    <div runat="server" id="Div6" class="tt-GlobalInfoDoc-status"></div>

                </div>
                <div class="modal-footer">
                    <button type="button" id="btnsave" onclick="updateWelcomeText();" class="btn btn-primary">Save changes</button>
                </div>
            </div>
        </div>
    </div>

    <%-- Start Jump To Model --%>

    <div class="modal fade" id="sleepingsnowman_nav" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true" runat="server">
        <div class="modal-dialog modal-sm modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title" id="exampleModalLabel">JUMP TO STEP</h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="steps_jump">

                        <input type="text" class="input" name="jump" id="txtjump2" value="" />
                        <input type="button" class="jumptooltip_btn foot_btn" onclick="jumpToPage(2)" value="OK">
                        <input type="button" class="jumptooltip_btn foot_btn" value="CANCEL" onclick="HideToggleJump()">
                    </div>
                </div>

            </div>
        </div>
    </div>

    <%-- End Jump To Model --%>

    <%-- Start Mobile Comment Model --%>

    <div class="modal fade" id="comment_mobile" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-sm modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title" id="exampleModalLabel">ADD YOUR COMMENT</h3>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body pt-0">
                    <textarea class="form-control mb-3 mt-2 w-100" id="txtRightComment" rows="3"></textarea>
                    <div class="comt_btn text-end">
                        <button id="btnUpdateRightSideComment"><i class="fa fa-check" aria-hidden="true"></i>OK</button>
                    </div>
                </div>

            </div>
        </div>
    </div>

    <%-- End Mobile Comment Popup --%>

    <script type="text/javascript">
        tinymce.init({
            selector: 'textarea#txtWelcome_EndPage',
            plugins: 'print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons',
            imagetools_cors_hosts: ['picsum.photos'],
            menubar: 'file edit view insert format tools table help',
            /*   toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads',*/
            toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview print | insertfile image media template link anchor codesample | ltr rtl Download Uploads | AddVariable ',
            toolbar_sticky: true,
            autosave_ask_before_unload: false,
            autosave_interval: '30s',
            autosave_prefix: '{path}{query}-{id}-',
            autosave_restore_when_empty: false,
            force_p_newlines: false,
            autosave_retention: '2m',
            image_advtab: true,
            link_list: [
                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                { title: 'My page 2', value: 'http://www.moxiecode.com' }
            ],
            image_list: [
                { title: 'My page 1', value: 'https://www.tiny.cloud' },
                { title: 'My page 2', value: 'http://www.moxiecode.com' }
            ],
            image_class_list: [
                { title: 'None', value: '' },
                { title: 'Some class', value: 'class-name' }
            ],
            importcss_append: true,
            /* enable title field in the Image dialog*/
            image_title: true,
            /* enable automatic uploads of images represented by blob or data URIs*/
            automatic_uploads: true,
            /*
              URL of our upload handler (for more details check: https://www.tiny.cloud/docs/configure/file-image-upload/#images_upload_url)
              images_upload_url: 'postAcceptor.php',
              here we add custom filepicker only to Image dialog
            */
            file_picker_types: "image",
            /* and here's our custom image picker*/
            file_picker_callback: function (cb, value, meta) {
                ///* Provide file and text for the link dialog */
                //if (meta.filetype === 'file') {
                //    callback('https://www.google.com/logos/google.jpg', { text: 'My text' });
                //}

                ///* Provide image and alt text for the image dialog */
                //if (meta.filetype === 'image') {
                //    callback('https://www.google.com/logos/google.jpg', { alt: 'My alt text' });
                //}

                ///* Provide alternative source and posted for the media dialog */
                //if (meta.filetype === 'media') {
                //    callback('movie.mp4', { source2: 'alt.ogg', poster: 'https://www.google.com/logos/google.jpg' });
                //}

                var input = document.createElement("input");
                input.setAttribute("type", "file");
                input.setAttribute("accept", "image/*");

                /*
                  Note: In modern browsers input[type="file"] is functional without
                  even adding it to the DOM, but that might not be the case in some older
                  or quirky browsers like IE, so you might want to add it to the DOM
                  just in case, and visually hide it. And do not forget do remove it
                  once you do not need it anymore.
                */

                input.onchange = function () {
                    var file = this.files[0];

                    var reader = new FileReader();
                    reader.onload = function () {
                        /*
                          Note: Now we need to register the blob in TinyMCEs image blob
                          registry. In the next release this part hopefully won't be
                          necessary, as we are looking to handle it internally.
                        */
                        var id = "blobid" + (new Date()).getTime();
                        var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                        var base64 = reader.result.split(",")[1];
                        var blobInfo = blobCache.create(id, file, base64);
                        blobCache.add(blobInfo);

                        /* call the callback and populate the Title field with the file name */
                        cb(blobInfo.blobUri(), { title: file.name });
                    };
                    reader.readAsDataURL(file);
                };

                input.click();
            },
            templates: [
                { title: 'New Table', description: 'creates a new table', content: '<div class="mceTmpl"><table width="98%%"  border="0" cellspacing="0" cellpadding="0"><tr><th scope="col"> </th><th scope="col"> </th></tr><tr><td> </td><td> </td></tr></table></div>' },
                { title: 'Starting my story', description: 'A cure for writers block', content: 'Once upon a time...' },
                { title: 'New list with dates', description: 'New List with dates', content: '<div class="mceTmpl"><span class="cdate">cdate</span><br /><span class="mdate">mdate</span><h2>My List</h2><ul><li></li><li></li></ul></div>' }
            ],
            template_cdate_format: '[Date Created (CDATE): %m/%d/%Y : %H:%M:%S]',
            template_mdate_format: '[Date Modified (MDATE): %m/%d/%Y : %H:%M:%S]',
            height: 350,
            image_caption: true,
            quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
            noneditable_noneditable_class: 'mceNonEditable',
            toolbar_mode: 'sliding',
            contextmenu: 'link image imagetools table',
            skin: 'oxide',// useDarkMode ? 'oxide-dark' : 'oxide',
            //content_css: [
            //    '../../CustomCSS/bootstrap.min.css',
            //    '../../CustomCSS/bootstrap.min.css.map',
            //    '../../CustomCSS/style.css',
            //    '../../CustomCSS/fonts.css',
            //    '../../CustomCSS/nav.css',
            //], // useDarkMode ? 'dark' : 'default',
            content_css: [
                "../../../App_Themes/ec2018/deco.css",
                "../../../App_Themes/ec2018/main.css",
                "../../../App_Themes/ec2018/fontawesome-all.min.css",
                '../../CustomCSS/style.css'
            ],
            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
            setup: function (editor) {
                editor.ui.registry.addButton('Download', {
                    icon: 'save',
                    tooltip: "Download",
                    onAction: function () {
                        var textToSave = tinymce.get(editor.id).getContent();
                        var textToSaveAsBlob = new Blob([textToSave], { type: "text/plain" });
                        var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
                        var projectname = $.cookie('OpenProjectName');
                        var fileNameToSaveAs = projectname.substring(projectname.indexOf("=") + 1) + "_" + hdnpage_type + ".mht";
                        var downloadLink = document.createElement("a");
                        downloadLink.download = fileNameToSaveAs;
                        downloadLink.innerHTML = "Download File";
                        downloadLink.href = textToSaveAsURL;
                        downloadLink.onclick = destroyClickedElement;
                        downloadLink.style.display = "none";
                        document.body.appendChild(downloadLink);
                        downloadLink.click();
                    }
                })
                editor.ui.registry.addButton('Uploads', {
                    icon: 'upload',
                    tooltip: "Upload",
                    onAction: function () {
                        tinymce.activeEditor.windowManager.open({
                            title: 'File Upload', // The dialog's title - displayed in the dialog header
                            body: {
                                type: 'panel', // The root body type - a Panel or TabPanel
                                items: [ // A list of panel components
                                    {
                                        type: 'htmlpanel', // A HTML panel component
                                        html: '<div> <input type="file" id="myFile" accept=".mht,.txt" "><input type="button" class="clsupload" value="Upload" onclick="uploadfile()"></div>'
                                    }
                                ]
                            },
                            buttons: [ // A list of footer buttons
                                {
                                    type: 'cancel',
                                    text: 'Cancel'
                                }
                            ]
                        });
                    }
                })
                editor.ui.registry.addButton('AddVariable', {
                    text: "Variables...",
                    tooltip: "Add Variable",
                    onAction: function () {
                        tinymce.activeEditor.windowManager.open({
                            title: 'Add Variable', // The dialog's title - displayed in the dialog header
                            body: {
                                type: 'panel', // The root body type - a Panel or TabPanel
                                items: [ // A list of panel components
                                    {
                                        type: 'htmlpanel', // A HTML panel component
                                        /*html: '<div>' + $.cookie('PopupBodyStr') + '</div>'*/
                                        html: '<div>' + $.cookie('welcomeVar') + '</div>'
                                    }
                                ]
                            },
                            buttons: [ // A list of footer buttons
                                {
                                    type: 'cancel',
                                    text: 'Cancel'
                                }
                            ]
                        });
                    }
                })
            }
        });
    </script>
</asp:Content>
