<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Footer.ascx.vb" Inherits=".Footer" %>

<!-- Modal -->
<div class="modal fade transaction-detailModal1" tabindex="-1" role="dialog" aria-labelledby="transaction-detailModalLabel1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="transaction-detailModalLabel1">Cluster Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body" id="current-cluster">
                <div id="accordionFlushExample"></div>
            </div>
        </div>
    </div>
</div>
<div class="modal fade transaction-detailModal2 step_model" tabindex="-1" role="dialog" aria-labelledby="transaction-detailModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl modal-dialog-centered" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="transaction-detailModalLabel2">STEPS</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" id="btnclose"></button>
            </div>
            <div class="modal-body">
                <div class="steps_body">
                    <div class="steps_list">
                        <ul id="stepparent">
                        </ul>
                    </div>
                    <div class="step_legent hide_lagents">
                        <h3 class="hide_legent_mob">Judgment Links Info</h3>
                        <p>The steps are colored as follows:</p>
                        <hr>
                        <ul>
                            <li class="current_step">Current Step</li>
                            <li class="notjudge">Judgment is not made</li>
                            <li class="result_step">Results or information steps</li>
                            <li class="judge_step">Judgment is made</li>
                            <%--  <li class="abstained_step">Judgment is abstained</li>--%>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<div class="modal fade tt-help-modal" tabindex="-1" role="dialog" aria-labelledby="tt-help-modalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered" style="max-width: 500px" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="tt-help-modal">Judgment Links Info</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body set-step-model-view changeModalHeight" style="font-size: 16px;">
                <div class="row">
                    <div class="large-12 columns">
                        <p>The current step is displayed with an <span style="background-color: #dc6201; color: #fff" class="label orange">orange</span> background. The step numbers are colored as follows:</p>
                    </div>
                </div>
                <div class="row steps-legend">
                    <div class="small-12 columns">
                        <div class="row collapse  mb-3">
                            <div class="col-2 col-md-2" style="color: red;">Red </div>
                            <div class="col-1 col-md-1">-</div>
                            <div class="col-9 col-md-9">Judgment has not yet been made</div>
                        </div>
                        <div class="row collapse mb-3">
                            <div class="col-2 col-md-2" style="color: #0051a8;">Blue </div>
                            <div class="col-1 col-md-1">-</div>
                            <div class="col-9 col-md-9">Results or information steps</div>
                        </div>
                        <div class="row collapse">
                            <div class="col-2 col-md-2" style="color: #000;">Black </div>
                            <div class="col-1 col-md-1">-</div>
                            <div class="col-9 col-md-9">Judgment has been made</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Model End -->

<!-- Footer -->

<%--<a class="footertogler d-lg-none togglePairwise" onclick="showHideBtn()"><i class="chevronIcons fas fa-angle-down"></i></a>--%>
<footer class="footer">


    <div class="d-none" id="mcluster">
        <div class="text-center">
            <input type="button" value="Next" class="font-size-16 foot_btn px-5 py-2" onclick="move_to_cluster()" />
        </div>
        +

        2.

        50+2.
    </div>

    <div class="container" id="ftrView">
        <div class=" d-none d-lg-block">
            <div class="row" id="divbuttonsView">
                <div class="col-sm-6 col-lg-5  align-self-center d-lg-block">
                    <div class="align-items-center d-flex justify-content-center justify-content-lg-start">
                        <div class="me-3">
                            <button type="button" class="foot_btn" data-bs-toggle="modal" data-bs-target=".transaction-detailModal1" id="btnCurrentCluster">
                                <img src="../../img/icon/sitemap-svgrepo-com.svg" class="icon">
                                Current Cluster
                            </button>
                            <button type="button" class="foot_btn ms-2" id="btnstep">
                                <img src="../../img/icon/list-svgrepo-com.svg" class="icon">
                                <span id="stepvalue"></span>
                            </button>
                            <button type="button" class="foot_btn mx-1 hide" id="hdnstepsbtn">
                                <i class="bx bx bx-list-ul font-size-16 align-middle me-2"></i><span id="hdnstepvalue"></span>
                            </button>
                        </div>
                        <div class="footer_progress">
                            <div class="progress" id="divEvaluatedBar">
                                <div class="progress-bar" id="FooterProgressBar" role="progressbar" style="width: 25%;" aria-valuenow="25"
                                    aria-valuemin="0" aria-valuemax="100">
                                </div>
                            </div>
                            <span class="color_blue" id="spnEvaluatedStep" style="font-size: 16px;"><strong>Evaluated:</strong> 48/100 <span class="text-blue">(15.3%)</span></span>
                        </div>

                        <%--<a href="javascript:void(0);" data-bs-toggle="modal" id="aQuestionIcon" data-bs-target=".tt-help-modal" class="font-size-18 left m-0 steps-help-icon-large-screen"><i class="fas fa-question-circle"></i></a>--%>
                    </div>
                </div>
                <%--<div class="col-sm-5 col-lg-2 col-md-6 col-4">
                    <div class="align-items-center d-flex justify-content-center justify-content-lg-start m-0">
                        <div class="progressdiv" id="divEvaluatedBar" data-percent="0">
                            <svg class="progress" width="50" height="50" viewport="0 0 100 100" version="1.1" xmlns="http://www.w3.org/2000/svg">
                                <circle r="22" cx="20" cy="20" fill="transparent" stroke-dasharray="157" stroke-dashoffset="0"></circle>
                                <circle class="bar" r="22" cx="20" cy="20" fill="transparent" stroke-dasharray="157" stroke-dashoffset="0"></circle>
                            </svg>
                        </div>
                        <span id="spnEvaluatedStep"><strong>Evaluated:</strong> 10/17</span>
                    </div>
                </div>--%>

                <div class="col-sm-7 col-lg-7  col-md-12 align-self-lg-center">
                    <div class="d-flex justify-content-center justify-content-lg-end">
                        <ul class="pagination pagination-sm mb-0" id="pgntn">
                        </ul>
                    </div>
                </div>
                <div id="divtooltp" class="tg-aa-wrap tg-aa toggle-ellipsis d-none">
                    <label id="autoadvance">
                        <input type="checkbox" id="chkautoadvanceTemp" onchange="set_auto_advance()" />&nbsp; 
                                   Auto Advance
                 <a href="javascript:void(0);" class="right question-icon-wrap"><span class="icon-tt-question-circle"></span></a>
                    </label>
                </div>
            </div>
        </div>
        <div id="divmobileview" class="d-lg-none">
            <div class="mobile_pagination">

                <div class="footer_progress_mob">
                    <%-- <div class="progress">

                <div class="footer_progress_mob" id="mdivEvaluatedBar">
                    <div class="progress">

                        <span id="spnEvaluate" class="title timer" data-from="0" data-to="15" data-speed="1800">15</span>
                        <div class="overlay"></div>
                        <div class="left"></div>
                        <div class="right"></div>
                    </div>--%>
                    <div class="footer_loader">
                        <div class="align-items-center d-flex justify-content-center justify-content-lg-start m-0">
                            <div class="progressdiv" id="mdivEvaluatedBar" data-percent="69.6">
                                <svg class="progress" width="50" height="50" viewport="0 0 100 100" version="1.1" xmlns="http://www.w3.org/2000/svg">
                                    <circle r="22" cx="20" cy="20" fill="transparent" stroke-dasharray="157" stroke-dashoffset="0"></circle>
                                    <circle class="bar" r="22" cx="20" cy="20" fill="transparent" stroke-dasharray="157" stroke-dashoffset="0" style="stroke-dashoffset: 109.272;"></circle>
                                </svg>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="justify-content-end d-flex" id="pgntn1"></div>
            </div>
            <div id="mdivtooltp" class="tg-aa-wrap tg-aa toggle-ellipsis d-none">
                <label id="mautoadvance">
                    <input type="checkbox" id="mchkautoadvance" onchange="set_auto_advance()" />&nbsp; 
                                   Auto Advance
                 <a href="javascript:void(0);" class="right question-icon-wrap"><span class="icon-tt-question-circle"></span></a>
                </label>
            </div>
            <%--<div class="footer_bottom">
                <div class="footerbtn">
                    <button type="button" class="foot_btn crtclst" data-bs-toggle="modal" data-bs-target=".transaction-detailModal1" id="mbtnCurrentCluster">
                        <i class="bx bx-sitemap font-size-12 align-middle me-2"></i>Current
                    </button>
                    <button type="button" class="foot_btn" data-bs-toggle="modal" data-bs-target=".transaction-detailModal2" id="mbtnstep">
                        <i class="bx bx bx-list-ul font-size-12 align-middle me-2"></i><span id="stepvalue1"></span>
                    </button>
                    <button type="button" class="foot_btn disabled hide" id="hdnmbtnstep">
                        <i class="bx bx bx-list-ul font-size-12 align-middle me-2"></i><span id="hdnstepvalue1"></span>
                    </button>
                    <div class="page-item " style="cursor: pointer;" onclick="next_unassessed()"><a class="bg-transparent border-0 p-1 page-link text-nowrap" href="javascript:void(0);"><i class="fas fa-chevron-right font-size-12 me-1"></i>Next Unassessed</a></div>
                </div>
            </div>--%>
        </div>
    </div>
</footer>



<!-- JAVASCRIPT -->
<%--<script src="/assets/libs/jquery/jquery.min.js"></script>--%>
<%--<script src="/assets/libs/bootstrap/js/bootstrap.bundle.min.js"></script>--%>


<script>
    var TotalPagecount = 0;
    var defaultValueActivepage = 1;
    var stepno = [];
    var i = 0;
    var hdnCurrentStep = 1, hdnTotalSteps = 1, hdnPreviousStep = 1, hdnPreviousToCurrent = 1, hdnLastUsedPage = 0;
    var flag = 0, stepButtons = '';
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    var submitSuccess = true, pageLoad = false;


    $(document).ready(function () {
        pageLoad = true;
        if (output != undefined && output != null && output != '') {
            if (output.pipeOptions.disableNavigation) {
                $("#btnCurrentCluster").hide();
                $("#btnstep,#mbtnstepLi").hide();
                $("#aQuestionIcon,#maQuestionIcon").addClass("hide");
                /*    $("#mbtnCurrentCluster").hide();*/
                $("#mbtnCurrentClusterLi").hide();
                //$("#hdnstepsbtn,#hdnmbtnstep").removeClass("hide");
            }
            setTimeout(function () {
                if (output.pipeOptions.hideNavigation) {
                    $("#btnCurrentCluster").hide();
                    $("#btnstep,#mbtnstepLi").hide();
                    $("#aQuestionIcon,#maQuestionIcon").addClass("hide");
                    //$("#hdnstepsbtn,#hdnmbtnstep").addClass("hide");
                    $("#spnEvaluatedStep").hide();
                    $("#divEvaluatedBar").hide();
                    $("#mdivEvaluatedBar").hide();
                    $('.footertogler').addClass('changefooter');
                    /*     $("#mbtnCurrentCluster").hide();*/
                    $("#mbtnCurrentClusterLi").hide();
                }
            }, 800);

            hdnCurrentStep = output.current_step;
            hdnCurrentStep = hdnCurrentStep == undefined || null ? 1 : hdnCurrentStep;
            hdnTotalSteps = output.total_pipe_steps;
            hdnTotalSteps = hdnTotalSteps == undefined || null ? 1 : hdnTotalSteps;
            setTimeout(function () {
                if ($(".removeB").length) {
                    if ($('#removeB').siblings('b').length) {
                        var sibHtml = $('#removeB').siblings('b')[0];
                        var newsibHtml = sibHtml.innerHTML;
                        $('#removeB').next().remove();
                        $('<div>' + newsibHtml + '</div>').insertAfter($('#removeB'));
                    }
                }
            }, 100);

            isreview = $.cookie('is_under_review');
            AllowAddStep = $.cookie('AllowAddStep');
            AllowAddStep = AllowAddStep == undefined || AllowAddStep == null || AllowAddStep == 'false' ? false : true;
            //AllowAddStep = AllowAddStep == undefined ? false : AllowAddStep;
            //AllowAddStep = AllowAddStep == null ? false : AllowAddStep;
            //AllowAddStep = AllowAddStep == 'false' ? false : true;
            if (isreview == "true") {
                //$('#mcluster').removeClass('d-none');
                //$('#ftrView').addClass('d-none');
                //$("#overlay").css('display', 'none');
                //$.cookie('is_under_review', false);
                if (AllowAddStep) {
                    $('#mcluster').removeClass('d-none');
                    $('#mcluster').addClass('d-none');
                    $('#ftrView').removeClass('d-none');
                    $("#overlay").css('display', 'none');
                    $.cookie('is_under_review', false);
                    AllowAddStep = true;
                    //loadStepsdata(false);
                    setsteps(hdnCurrentStep);
                }
                else {
                    $('#mcluster').removeClass('d-none');
                    $('#ftrView').addClass('d-none');
                    $("#overlay").css('display', 'none');
                    $.cookie('is_under_review', false);
                    AllowAddStep = true;
                }
            }
            else {
                $('#mcluster').addClass('d-none');
                $('#ftrView').removeClass('d-none');
                //loadStepsdata(false);
                setsteps(hdnCurrentStep);
            }
            /*console.log(output.judgment_made);*/
            $('#spnEvaluatedStep').html('');
            $('#spnEvaluate').html();
            var evaluate = output.judgment_made > 0 && output.total_evaluation ? ((output.judgment_made / output.total_evaluation) * 100).toFixed(2) : 0;
            $('#spnEvaluate').html(evaluate + "%");
            $('#spnEvaluatedStep').html('<strong>Evaluated: </strong>' + output.judgment_made + '/' + output.total_evaluation + "<span class='text-blue'> (" + (output.judgment_made > 0 && output.total_evaluation ? ((output.judgment_made / output.total_evaluation) * 100).toFixed(2) : 0) + "%)</span>");
            getEvaluationProgressPercentage(output.overall, true);
        }
    });

    (function () {
        window.onload = function () {
            getEvaluationProgressPercentage(output.overall, true);
            var totalProgress, progress;
            const circles = document.querySelectorAll('svg.progress');
            for (var i = 0; i < circles.length; i++) {
                totalProgress = circles[i].querySelector('circle').getAttribute('stroke-dasharray');
                progress = circles[i].parentElement.getAttribute('data-percent');

                circles[i].querySelector('.bar').style['stroke-dashoffset'] = totalProgress * progress / 100;

            }
        }
    })();

    $(function () {
        $(".multiDirectSlider").slider({
            orientation: "horizontal",
            range: "min",
            min: 0,
            max: 100,
            value: 55,
            slide: function (event, ui) {
                /*$("#amount-1").val("$" + ui.value);*/
                var id = event.target.id;
                var myval = $('#' + id).data('myval');
                if (!$('#multi_direct_slider_' + myval).hasClass('active_table_row')) {
                    $('div.row_wrapper').removeClass('active_table_row');
                    $('#multi_direct_slider_' + myval).addClass('active_table_row');
                }

                if (parseFloat(ui.value) >= 0) {
                    $('#at_direct_slider' + myval).slider("value", ui.value);
                    $('#at_direct_input' + myval).val((parseFloat(ui.value) / 100));
                    $('#close_icon_' + myval + ' i').show();
                }
                else {
                    $('#at_direct_slider' + myval).slider("value", 0);
                    $('#at_direct_input' + myval).val('0');
                    $('#close_icon_' + myval + ' i').hide();
                }
                update_multi_direct_slider(myval, (parseFloat(ui.value) / 100));
            },
            stop: function (event, ui) {
                setTimeout(function () {
                    var id = event.target.id;
                    var myval = $('#' + id).data('myval');
                    var NextIncJudgRowID = GetMtDirectIncompleteJudgementRowID(myval);
                    /*$('#multi_direct_slider_' + myval).removeClass('active_table_row');*/
                    $('div.row_wrapper').removeClass('active_table_row');
                    if (NextIncJudgRowID != -1) {
                        $('#multi_direct_slider_' + NextIncJudgRowID).addClass('active_table_row');
                        ActiveDirect(NextIncJudgRowID);
                    }
                    else {
                        if ($('#multi_direct_slider_' + (parseInt(myval) + 1)).length > 0) {
                            $('#multi_direct_slider_' + (parseInt(myval) + 1)).addClass('active_table_row');
                            ActiveDirect(parseInt(myval) + 1);

                        }
                        else {
                            $('#multi_direct_slider_' + myval).addClass('active_table_row');
                            ActiveDirect(myval);
                        }
                    }
                }, 1000);
            }
        });
    });

    $(function () {
        $(".singleDirectSlider").slider({
            orientation: "horizontal",
            range: "min",
            min: 0,
            max: 100,
            value: 0,
            slide: function (event, ui) {
                var id = event.target.id;

                if (parseFloat(ui.value) >= 0) {
                    $('#at_direct_slider').slider("value", ui.value);
                    $('#at_direct_input').val((parseFloat(ui.value) / 100));
                    $('#close_icon i').removeClass('d-none');
                }
                else {
                    $('#at_direct_slider').slider("value", 0);
                    $('#at_direct_input').val('0');
                    $('#close_icon i').removeClass('d-none');
                    $('#close_icon i').addClass('d-none');
                }
                update_single_direct_slider((parseFloat(ui.value) / 100));
            },
            stop: function (event, ui) {
                setTimeout(function () {
                    var id = event.target.id;
                    save_direct_on_next();
                }, 50);
            }
        });
    });

    function setsteps(activepage) {
        //$('#stepvalue').text("Step " + activepage + "/" + localStorage.getItem("totalpages"));
        $('#stepvalue').text("Step " + activepage + "/" + hdnTotalSteps);
        $('#hdnstepvalue').text("Step " + activepage + "/" + hdnTotalSteps);

        $('#stepvalue1').text("Step " + activepage + "/" + hdnTotalSteps);
        $('#hdnstepvalue1').text("Step " + activepage + "/" + hdnTotalSteps);
    }

    function showHideBtn() {
        $('.footertogler').toggleClass('downfooter_btn');
        //$('.footertogler').toggle('hide-footer');
        // $('.footer').toggle('downfooter_btn');
        $('.footer').toggleClass('hide-footer');
    }

    function loadStepsdata(isclick) {
        var first = 1;
        var last = 0;
        $.ajax({
            type: "POST",
            url: baseUrl + "AnytimeComparion/Login.aspx/loadStepList",
            data: JSON.stringify({ first: first, last: last }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            success: function (data) {
                var childItem = '';
                var firstArray = data.d.split('[');
                var finaldata = firstArray.map(r => r.replace('],', '').replace(']', '').replace(/['"`]/g, '').split('$$'));
                stepButtons = finaldata;
                TotalPagecount = finaldata.length - 1;
                for (i = 0; i < finaldata.length; i++) {
                    var item = finaldata[i];
                    var pageIndex = i;

                    var color = stepColor(stepButtons[i]);
                    var scolor = (color == '#0058a3') ? '#0058FF' : (color == '#e74c3c' ? '#F0142F' : color);

                    if (pageIndex == hdnCurrentStep) {
                        if (item[0] && item[0] != " ") childItem += "<li class='active' style='cursor: pointer;'><a  id='" + pageIndex + "' onclick=setPagination(" + pageIndex + ",true)><span style='color:white; background: #DC6200 !important;'>" + item[item.length - 1] + "</span>" + item[0].substring(item[0].indexOf(":") + 1) + "</a></li>";
                        defaultValueActivepage = defaultValueActivepage + 1;
                        //if (hdnCurrentStep != 10) {
                        //    $.removeCookie('SelectionID');
                        //}

                    }
                    else {

                        if (item[0] && item[0] != " ") childItem += "<li style='cursor: pointer;'><a id='" + pageIndex + "' onclick=setPagination(" + pageIndex + ") style='color: " + scolor + "' ><span style='color: " + scolor + " ; background:  white !important; border: 1px solid " + scolor + ";'>" + item[item.length - 1] + "</span>" + item[0].substring(item[0].indexOf(":") + 1) + "</a></li>";
                    }
                }

                $('#stepparent').html(childItem);


                setPagination();
                EnableDisablePageWise();
                if (isclick)
                    $(".transaction-detailModal2").modal("show");

                //$("#overlay").css('display', 'none');

                setTimeout(function () {
                    $("#overlay").css('display', 'none');
                }, 800);
            },

            error: function () {
                //alert("error");
            }
        })
    }

    var EnableDisablePageWise = function () {
        if (output.pipeOptions.dontAllowMissingJudgment) {
            setTimeout(function () {
                var disablePage = false;
                if (hdnpage_type == 'atPairwise') {
                    if (output.value <= 0) {
                        EnableDisablePagination(true);
                    }
                    else if (output.value > 0) {
                        if (enabledLastStep == hdnCurrentStep)
                            EnableDisablePagination(false, true);
                        else
                            EnableDisablePagination(true, false);
                    }
                }
                else if (hdnpage_type == 'atShowLocalResults' || hdnpage_type == 'atShowGlobalResults') {
                    if (enabledLastStep == hdnCurrentStep)
                        EnableDisablePagination(false, true);
                    else
                        EnableDisablePagination(true, false);
                }
                else if (hdnpage_type == 'atSpyronSurvey') {
                    var skip = false;
                    SurveyAnswers.forEach(function (value, key) {
                        if (!skip) {
                            var surveyquestion = SurveyPageQuestions[key];
                            var reg = new RegExp('^[0-9.\n]+$');
                            if (surveyquestion.Type == 4) {
                                var options = [];
                                if (value[0] == "")
                                    options = [];
                                else
                                    options = value[0].split(";");
                                var totalAnsweredChb = options.length;
                                var totalVariants = surveyquestion.Variants.length;
                                if (surveyquestion.Variants[totalVariants - 1].Type == 2)
                                    totalAnsweredChb -= 1;
                                var min = surveyquestion.MinSelectedVariants;
                                var max = surveyquestion.MaxSelectedVariants == 0 ? Number.MAX_VALUE : surveyquestion.MaxSelectedVariants;
                                if (min > totalAnsweredChb || max < totalAnsweredChb) {
                                    skip = true;
                                    disablePage = true;
                                }
                            }
                            else if (surveyquestion.Type == 15 && value[0] != "") {
                                if (!value[0].match(reg)) {
                                    skip = true;
                                    disablePage = true;
                                }
                            }
                            if (value[2] == "False") {
                                if (!checkSurveyValue(value[0])) {
                                    skip = true;
                                    disablePage = true;
                                }
                            }
                        }
                    });

                    if (disablePage) {
                        EnableDisablePagination(true);
                    }
                    else {
                        if (enabledLastStep == hdnCurrentStep)
                            EnableDisablePagination(false, true);
                        else
                            EnableDisablePagination(true, false);
                    }
                }
                else if (hdnpage_type == 'atAllPairwise') {
                    if (hdnpairwise_type == 'ptGraphical') {
                        if (multi_pw_data.length > 0) {
                            for (i = 0; i < multi_pw_data.length; i++) {
                                if (left_input[i] == "" && right_input[i] == "") {
                                    disablePage = true;
                                }
                            }
                        }
                    }
                    else if (hdnpairwise_type == 'ptVerbal') {
                        if (hdnMultiPwData.length > 0) {
                            for (i = 0; i < hdnMultiPwData.length; i++) {
                                if (parseFloat(hdnMultiPwData[i].Value) <= 0) {
                                    disablePage = true;
                                }
                            }
                        }
                    }
                    if (disablePage) {
                        EnableDisablePagination(true);
                    }
                    else {
                        if (enabledLastStep == hdnCurrentStep)
                            EnableDisablePagination(false, true);
                        else
                            EnableDisablePagination(true, false);
                    }
                }
                else if (hdnpage_type == 'atNonPWAllChildren' || hdnpage_type == 'atNonPWAllCovObjs') {
                    if (hdnpairwise_type == 'mtDirect') {
                        if (at_multi_direct_input.length > 0) {
                            for (i = 0; i < at_multi_direct_input.length; i++) {
                                if (at_multi_direct_input[i] == "") {
                                    disablePage = true;
                                }
                            }
                        }
                    }
                    else if (hdnpairwise_type == 'mtRatings') {
                        for (var i = 0; i < multi_ratings_row.length; i++) {
                            if (multi_ratings_row[i][1] == 'Not Rated' || multi_ratings_row[i][1] == 'None') {
                                disablePage = true;
                            }
                        }
                    }
                    if (disablePage) {
                        EnableDisablePagination(true);
                    }
                    else {
                        if (enabledLastStep == hdnCurrentStep)
                            EnableDisablePagination(false, true);
                        else
                            EnableDisablePagination(true, false);
                    }
                }
                else if (hdnpage_type == 'atNonPWOneAtATime') {
                    if (hdnpairwise_type == 'mtRatings') {
                        if (ratings_data[1].toLowerCase() == 'not rated' || ratings_data[1].toLowerCase() == 'none') {
                            disablePage = true;
                        }
                    }
                    if (hdnpairwise_type == 'mtDirect') {
                        if (output.pipeOptions.dontAllowMissingJudgment) {
                            if (output.non_pw_value == undefined || output.non_pw_value == null || parseFloat(output.non_pw_value) < 0) {
                                disablePage = true;
                            }
                        }
                    }

                    if (disablePage) {
                        EnableDisablePagination(true);
                    }
                    else {
                        if (enabledLastStep == hdnCurrentStep)
                            EnableDisablePagination(false, true);
                        else
                            EnableDisablePagination(true, false);
                    }
                }
            }, 50);
        }
    }

    $(document).on("keypress", "#txtjump2, #txtjump1", function (e) {
        if (e.which == 13) {
            var id = (e.currentTarget.id).substring(7);
            jumpToPage(id);
        }
    });

    function reToggleJump() {
        $('#MainContent_sleepingsnowman_nav').modal('show')
        $('#txtjump2').focus();
    }

    function HideToggleJump() {
        $('#MainContent_sleepingsnowman_nav').modal('hide')
    }


    function toggleJump(id) {
        $(".divError").css('display', 'none');
        $('.dvjump').css('display', 'none');
        //$('#jump' + id).toggle();
        $('#jump' + id).css('display', 'block');
        //$('#txtjump' + id).val();
        $('#txtjump' + id).focus();
    }

    function hideJump(id) {
        $(".divError").css('display', 'none');
        $('#jump' + id).css('display', 'none');
    }

    function jumpToPage(id) {
        $(".divError").css('display', 'none');
        var pageNo = $('#txtjump' + id).val();
        if (pageNo == "" || pageNo < 1 || pageNo > TotalPagecount) {
            $(".divError").css('display', 'block');
            $(".errormessage").html();
            $(".errormessage").html('Wrong step number. Should be between 1 and ' + TotalPagecount + '.');
        }
        else {
            var disabledli = $('li.page-item.disabledli').first().text();
            //if (disabledli != '' && parseInt(disabledli) > 0) {
            //    if (parseInt(disabledli) <= parseInt(pageNo)) {
            //        $(".divError").css('display', 'block');
            //        $(".errormessage").html();
            //        $(".errormessage").html('Wrong step number. Should be between 1 and ' + parseInt(disabledli - 1) + '.');
            //    }
            //    else {
            //        setPagination(pageNo);
            //    }
            //}
            if (parseInt(enabledLastStep) > 0) {
                if (parseInt(enabledLastStep) < parseInt(pageNo)) {
                    $(".divError").css('display', 'block');
                    $(".errormessage").html();
                    $(".errormessage").html('Wrong step number. Should be between 1 and ' + parseInt(enabledLastStep) + '.');
                }
                else {
                    setPagination(pageNo);
                }
            }
            else {
                setPagination(pageNo);
            }
        }
    }

    var screen_size = 0;
    var mactivePageValue = 0;
    $(window).on('resize', function () {
        if (output != '')
            checkpagination();
    });

    function checkpagination() {
        var screen_height = window.outerHeight;
        var screen_width = document.documentElement.clientWidth;
        var mstartIndex = 0, mendIndex = 0;
        var activePage = mactivePageValue ? mactivePageValue : hdnCurrentStep ? parseInt(hdnCurrentStep) : 1;
        //6
        if (screen_width < 410) {  //1
            if (activePage != 1)
                mstartIndex = activePage - 1;
            else
                mstartIndex = 1;

            if (mstartIndex < 1)
                mstartIndex = 1;

            mendIndex = mstartIndex + 2;
            if (mendIndex > TotalPagecount) {
                mstartIndex = TotalPagecount - 2;
                mendIndex = TotalPagecount;
            }
            if (activePage < 3)
                refreshPagination(mstartIndex, mendIndex, false, true, activePage);
            else if (activePage > TotalPagecount - 2)
                refreshPagination(mstartIndex, mendIndex, true, false, activePage);
            else
                refreshPagination(mstartIndex, mendIndex, true, true, activePage);
        }
        else if (screen_width >= 410 && screen_width < 480) {
            if (activePage != 1)
                mstartIndex = activePage - 2;
            else
                mstartIndex = 1;

            if (mstartIndex < 1)
                mstartIndex = 1;

            mendIndex = mstartIndex + 4;
            if (mendIndex > TotalPagecount) {
                mstartIndex = TotalPagecount - 4;
                mendIndex = TotalPagecount;
            }
            if (activePage < 5)
                refreshPagination(mstartIndex, mendIndex, false, true, activePage);
            else if (activePage > TotalPagecount - 4)
                refreshPagination(mstartIndex, mendIndex, true, false, activePage);
            else
                refreshPagination(mstartIndex, mendIndex, true, true, activePage);
        }
        else if (screen_width >= 480 && screen_width < 600) {
            if (activePage != 1)
                mstartIndex = activePage - 3;
            else
                mstartIndex = 1;

            if (mstartIndex < 1)
                mstartIndex = 1;

            mendIndex = mstartIndex + 5;
            if (mendIndex > TotalPagecount) {
                mstartIndex = TotalPagecount - 5;
                mendIndex = TotalPagecount;
            }
            if (activePage < 6 && mstartIndex < 2)
                refreshPagination(mstartIndex, mendIndex, false, true, activePage);
            else if (activePage > TotalPagecount - 5)
                refreshPagination(mstartIndex, mendIndex, true, false, activePage);
            else
                refreshPagination(mstartIndex, mendIndex, true, true, activePage);
        }
        else if (screen_width >= 600 || screen_width < 700) {
            if (activePage != 1)
                mstartIndex = activePage - 5;
            else
                mstartIndex = 1;

            if (mstartIndex < 1)
                mstartIndex = 1;

            mendIndex = mstartIndex + 7;
            if (mendIndex > TotalPagecount) {
                mstartIndex = TotalPagecount - 7;
                mendIndex = TotalPagecount;
            }
            if (activePage < 8 && mstartIndex < 2)
                refreshPagination(mstartIndex, mendIndex, false, true, activePage);
            else if (activePage > TotalPagecount - 7)
                refreshPagination(mstartIndex, mendIndex, true, false, activePage);
            else
                refreshPagination(mstartIndex, mendIndex, true, true, activePage);
        }
        else if (screen_width >= 700 || screen_width < 800) {
            if (activePage != 1)
                mstartIndex = activePage - 7;
            else
                mstartIndex = 1;

            if (mstartIndex < 1)
                mstartIndex = 1;

            mendIndex = mstartIndex + 9;
            if (mendIndex > TotalPagecount) {
                mstartIndex = TotalPagecount - 9;
                mendIndex = TotalPagecount;
            }
            if (activePage < 10 && mstartIndex < 2)
                refreshPagination(mstartIndex, mendIndex, false, true, activePage);
            else if (activePage > TotalPagecount - 9)
                refreshPagination(mstartIndex, mendIndex, true, false, activePage);
            else
                refreshPagination(mstartIndex, mendIndex, true, true, activePage);
        }
        else {
            if (activePage != 1)
                mstartIndex = activePage - 9;
            else
                mstartIndex = 1;

            if (mstartIndex < 1)
                mstartIndex = 1;

            mendIndex = mstartIndex + 11;
            if (mendIndex > TotalPagecount) {
                mstartIndex = TotalPagecount - 11;
                mendIndex = TotalPagecount;
            }
            if (activePage < 10)
                refreshPagination(mstartIndex, mendIndex, false, true, activePage);
            else if (activePage > TotalPagecount - 9)
                refreshPagination(mstartIndex, mendIndex, true, false, activePage);
            else
                refreshPagination(mstartIndex, mendIndex, true, true, activePage);
        }

    }

    function refreshPagination(mstartIndex, mendIndex, jump1, jump2, activePage) {
        $('#ulPagination').html('');
        var pItems = '';
        var mcount = 0;
        /*pItems += '<li class="ccc page-item" onclick=setPagination(' + localStorage.getItem('hdnPreviousStep') + ')><a class="bg-primary page-link text-light" href="javascript:void(0);" tabindex="-1"><i class="fa fa-undo" aria-hidden="true"></i></a></li>'*/
        //if (output.pipeOptions.hideNavigation || output.pipeOptions.disableNavigation) {
        //    if (activePage > 1) {
        //        pItems += '<li class="page-item mx-2" onclick=setPagination(1)><a class="page-link btn-green" href="javascript:void(0);" tabindex="-1">First</a></li>'
        //    }
        //    else {
        //        pItems += '<li class="page-item mx-2 d-none" onclick=setPagination(1)><a class="page-link btn-green" href="javascript:void(0);" tabindex="-1">First</a></li>'
        //    }
        //}

        if (activePage <= 1) {
            pItems += '<li class="rc-pagination-prev disabled d-none"><a class="page-link" title="First" href="javascript:void(0);" tabindex="-1"><img src="../../img/icon/ext-prev.svg" class="icon me-1"></a></li><li class="rc-pagination-prev disabled d-none"><a class="page-link" title="Previous" href="javascript:void(0);" tabindex="-1"><img src="../../img/icon/Back.svg" class="icon me-1"></a></li>'
        }
        else {
            pItems += '<li class="rc-pagination-prev" onclick=setPagination(1)><a class="page-link btn-green" title="First" href="javascript:void(0);" tabindex="-1"><img src="../../img/icon/ext-prev.svg" class="icon me-1"> </a></li><li class="rc-pagination-prev" onclick=previous()><a class="page-link btn-green" title="Previous"../../img/icon/Next.svg href="javascript:void(0);" tabindex="-1"><img src="../../img/icon/Back.svg" class="icon me-1"> </a></li>'
        }
        var clcount = 10;
        if (!output.pipeOptions.hideNavigation && !output.pipeOptions.disableNavigation) {
            for (var i = 0; i < TotalPagecount; i++) {
                mcount = mcount + 1;

                var StepBtnMouseOverTxt = stepButtons[mcount][0];
                StepBtnMouseOverTxt = StepBtnMouseOverTxt.replace('#', '');
                StepBtnMouseOverTxt = StepBtnMouseOverTxt.replace(':', '-');

                var color = stepColor(stepButtons[mcount]);
                //if (CSS.supports('color', stepButtons[mcount][1])) {
                //    color = stepButtons[mcount][1];
                //}
                //else if (CSS.supports('color', stepButtons[mcount][3])) {
                //    color = stepButtons[mcount][3];
                //}
                //else if (CSS.supports('color', stepButtons[mcount][5])) {
                //    color = stepButtons[mcount][5];
                //}
                //else if (CSS.supports('color', stepButtons[mcount][8])) {
                //    color = stepButtons[mcount][8];
                //}
                //else if (CSS.supports('color', stepButtons[mcount][10])) {
                //    color = stepButtons[mcount][10];
                //}
                //else {
                //    color = 'black';
                //}

                if (i == 1) {
                    if (jump1) {
                        /*pItems += '<li class="page-item paginationSpace" ><div class="pagination_tooltip dvjump" id="jump11" style="display:none;"><div class="blue alert alert-danger divError" style="display:none;" role="alert"><span class="errormessage" ></span></div ><div class="jumptooltip_wrapper"><span> Jump to step </span> <input type="text" class="input" id="txtjump11" name="jump" value="" /><input type="button" class="jumptooltip_btn foot_btn" onclick="jumpToPage(11)" value="OK">  <input type="button" class="jumptooltip_btn foot_btn" onclick="hideJump(11)" value="CANCEL"></div></div> <span style="cursor:pointer" onclick="toggleJump(11)">&hellip;</span></li > ';*/

                        pItems += '<li title="Jump To" class="page-item paginationSpace"><span style="cursor:pointer" onclick="reToggleJump()">&hellip;</span></li > ';
                    }
                }
                if (i == TotalPagecount - 1) {
                    if (jump2) {
                        /*pItems += '<li class="page-item paginationSpace" id="mbtnjump"><div class="pagination_tooltip dvjump" id="jump12" style="display:none;"><div class="blue alert alert-danger divError" style="display:none;" role="alert"><span class="errormessage" ></span></div ><div class="jumptooltip_wrapper"><span> Jump to step </span> <input type="text" class="input" name="jump" id="txtjump12" value="" /><input type="button" class="jumptooltip_btn foot_btn" onclick="jumpToPage(12)" value="OK"> <input type="button" class="jumptooltip_btn foot_btn" onclick="hideJump(12)" value="CANCEL"></div></div><span style="cursor:pointer" onclick="toggleJump(12)">&hellip;</span></li>';*/

                        pItems += '<li title="Jump To" class="page-item paginationSpace" > <span style="cursor:pointer" onclick="reToggleJump()">&hellip;</span></li > ';
                    }
                }
                //Bhawan    

                if (TotalPagecount > 9) {
                    if (activePage == mcount && mcount == 1) {
                        pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-item rc-pagination-item-3 rc-pagination-item-active"><a class="nofollow" href="javascript:void(0);"">' + mcount + '</a></li>';
                    } else if (mcount == 1) {
                        pItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-7 d-lg-block" onclick=setPagination(' + mcount + ')><a style="color:' + color + ' !important" class="nofollow" href="javascript:void(0);"">' + mcount + '</a></li>';
                    }
                }

                if (mcount > mstartIndex && mcount < mendIndex) {
                    clcount = clcount - 1;
                    if (activePage == mcount) {
                        pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-item rc-pagination-item-3 rc-pagination-item-active" onclick=setPagination(' + mcount + ',true)><a class="nofollow" href="javascript:void(0);"">' + mcount + '</a></li>';
                    } else {
                        pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-item rc-pagination-item-7 d-lg-block" onclick=setPagination(' + mcount + ')><a style="color:' + color + ' !important" class="nofollow" href="javascript:void(0);"">' + mcount + '</a></li>';
                    }
                }
            }
        }

        if (activePage == mcount) {
            if (mcount > 0)
                pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-item rc-pagination-item-3 rc-pagination-item-active"><a class="nofollow" href="javascript:void(0);" style="color:' + color + '">' + mcount + '</a></li>';
            if (output.current_step < output.total_pipe_steps)
                pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-item rc-pagination-item-7 d-lg-block disabled"><a class="nofollow" href="javascript:void(0);">Next</a></li></ul>'
        }
        else {
            if (mcount > 0)
                pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-item rc-pagination-item-7 d-lg-block" onclick=setPagination(' + mcount + ')><a class="nofollow" href="javascript:void(0);" style="color:' + color + ' !important">' + mcount + '</a></li>';
            pItems += '<li title="' + StepBtnMouseOverTxt +'" class="rc-pagination-next" id="mbtnNext" onclick=next()><a class="page-link btn-green" tilte="Next" href="javascript:void(0);"><img title="Next" src = "../../img/icon/Next.svg" class="icon ms-1" ></button ></li ></a></li><li class="rc-pagination-next" title="Last" onclick=setPagination(' + TotalPagecount + ')><a class="page-link btn-green" href="javascript:void(0);"><img src = "../../img/icon/ext-next.svg" class="icon " ></button ></li ></a></li>'


        }
        if (output.current_step == output.total_pipe_steps) {
            /*if ((!output.fromComparion && showFinish) || (output.doneOptions != undefined && output.doneOptions != null && output.doneOptions.logout == 'True')) {*/
            /*if (!output.fromComparion && showFinish) {*/
            if (!output.fromComparion && (showFinish || showSave)) {
                pItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime(<%= Convert.ToInt32(Session(Pages.external_classes.Constants.Sess_LoginMethod))%>)"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            }
            else if (output.fromComparion && output.doneOptions.stayAtEval == 'True' && output.doneOptions.logout == 'True') {
                pItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime(<%= Convert.ToInt32(Session(Pages.external_classes.Constants.Sess_LoginMethod))%>)"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            }

            /*if (output.fromComparion && output.doneOptions != undefined && output.doneOptions != null && output.doneOptions.stayAtEval.toLowerCase() == 'false') {*/
            //if (output.fromComparion) {
            //    pItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime()"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            //}
        }
        var AutoAdvancehtml = '';
        var IsShowAutoAdvanceOp = showOptions();
        if (IsShowAutoAdvanceOp) {
            /*  if (output.pipeOptions.disableNavigation) {*/
            if (output.is_auto_advance) {
                AutoAdvancehtml += '<li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" checked/>Auto Advance</a></li>';
            }
            else {
                AutoAdvancehtml += '<li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" />Auto Advance</a></li>';

            }
        }
        else {
            AutoAdvancehtml += '<li class="d-none"><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" />Auto Advance</a></li>';
        }
        var ThreeDotsHideOrNotStyle = " ";
        if (output != undefined && output != null && output != '') {
            if (output.pipeOptions.disableNavigation) {
                if (!IsShowAutoAdvanceOp) {
                    ThreeDotsHideOrNotStyle = 'style="display: none;" ';
                }
            }
        }
        var NextassessedbtnHtml = '</li><li class="footer_btns_mob d-none disabledli" id="mbtnUnass" onclick=next_unassessed()><a class="bg-transparent unassesed_btn page-link text-nowrap" href="javascript:void(0);" >Next Unassessed</a></li>';
        var nextassessed = output.nextUnassessedStep != undefined || null ? eval(output.nextUnassessedStep[0]) : '0';
        var cstep = eval(output.current_step);
        if (output.pipeOptions.showUnassessed) {
            if (nextassessed != 0 && nextassessed != null && typeof nextassessed != 'undefined' && nextassessed != cstep && activePage != mcount) {
                NextassessedbtnHtml = '</li><li class="footer_btns_mob" id="mbtnUnass" onclick=next_unassessed()><a class="bg-transparent unassesed_btn page-link text-nowrap" href="javascript:void(0);" >Next Unassessed</a></li>';
                ThreeDotsHideOrNotStyle = '';
            }
        }
        var DisplayHideStyle = ' ';
        if (output.pipeOptions.disableNavigation) {
            DisplayHideStyle = ' d-none ';
        }
        pItems += '<li title="More" class="rc-pagination-next" ' + ThreeDotsHideOrNotStyle + 'aria-disabled="false" id="mbtnlinthreedot" tabindex="0"><div class="btn-group" ><button class="btn btn-primary bg-white" type="button" id="defaultDropdown" data-bs-toggle="dropdown" data-bs-auto-close="true" aria-expanded="false"><img src="../../img/icon/Snow man.svg" class="icon"></button><ul class="dropdown-menu" aria-labelledby="defaultDropdown">' + AutoAdvancehtml + '<li class="footer_btns_mob' + DisplayHideStyle + '" id="mbtnCurrentClusterLi"><button type="button" class="foot_btn" data-bs-toggle="modal" data-bs-target=".transaction-detailModal1" id="mbtnCurrentCluster" onclick="currentClusterOnClick()"><img src="../../img/icon/sitemap-svgrepo-com.svg" class="icon me-2">Current Cluster</button ></li><li class="footer_btns_mob' + DisplayHideStyle + '" id="mbtnstepLi"><button type="button" class="foot_btn" id="mbtnstep" onclick="stepValueOnClick()"><img src="../../img/icon/list-svgrepo-com.svg" class="icon me-2"><span id="stepvalue">Step ' + output.current_step + '/' + hdnTotalSteps + '</span></button >' + NextassessedbtnHtml + '</ul></div></li >';

        //pItems += '<li title="Next" class="rc-pagination-next" aria-disabled="false" tabindex="0"><div class="btn-group" ><button class="btn btn-primary bg-white" type="button" id="defaultDropdown" data-bs-toggle="dropdown" data-bs-auto-close="true" aria-expanded="false"><img src="../../img/icon/Snow man.svg" class="icon"></button><ul class="dropdown-menu" aria-labelledby="defaultDropdown"><li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()"' + AutoAdvanceCheck + ' />Auto Advance</a></li><li class="footer_btns_mob"><button type="button" class="foot_btn" data-bs-toggle="modal" data-bs-target=".transaction-detailModal1" id="btnCurrentCluster"><img src="../../img/icon/sitemap-svgrepo-com.svg" class="icon me-2">Current Cluster</button ></li><li class="footer_btns_mob"><button type="button" class="foot_btn" id="btnstep"><img src="../../img/icon/list-svgrepo-com.svg" class="icon me-2"><span id="stepvalue">Step 2/44</span></button ></li><li class="footer_btns_mob"><a class="bg-transparent unassesed_btn page-link text-nowrap" href="javascript:void(0);">Next Unassessed</a></li></ul></div></li >';

        $('#ulPagination').html(pItems);
        EnableDisablePageWise();
    }

    var message = "";
    function setPagination(activePageValue = null, currentPage = false) {

        $("#overlay").css('display', 'flex');

        if (activePageNo == 0) {
            if (window.speechSynthesis.speaking) {
                window.speechSynthesis.cancel();
            }
        }
        /*if (activePageValue != hdnCurrentStep) {*/
        if (activePageValue != undefined && activePageValue != null && $.isNumeric(activePageValue) && activePageValue > 0) {
            $("#overlay").css('display', 'flex');
        }
        $.cookie("pageCount", TotalPagecount);
        mactivePageValue = activePageValue;

        if (activePageValue != undefined && activePageValue != null) {
            $.removeCookie('SelectionID');
            block_unload_prompt = true;
            if (hdnpage_type == 'atSpyronSurvey' && !output.isPipeViewOnly) {
                //if (activePage != hdnCurrentStep) {
                if (activePageValue != hdnCurrentStep) {
                    var ret = false;
                    if (activePageValue > hdnCurrentStep) {
                        ret = save_respondentAnswer(hdnCurrentStep, hdnCurrentStep + 1, true);
                    }
                    else {
                        ret = save_respondentAnswer(hdnCurrentStep, hdnCurrentStep + 1, true, true);
                    }
                    if (ret == false) {
                        if (activePageValue > hdnCurrentStep) {
                            alert(message);
                            submitSuccess = false;
                            message = "";
                            return false;
                        }
                    }
                }
                else {
                    var ret = save_respondentAnswer(hdnCurrentStep, hdnCurrentStep + 1, false);
                    if (ret == false) {
                        if (activePageValue > hdnCurrentStep) {
                            alert(message);
                            submitSuccess = false;
                            message = "";
                            return false;
                        }
                    }
                }
                //if (activePage != hdnCurrentStep && submitSuccess) {
                //    SurveyAnswers = [];
                //    answer = [];
                //    SurveyPageQuestions = [];
                //    $('#hdnPageNumber').val(activePage);
                //    $('#MainContent_hdnPageNo').trigger('click');
                //}
                //else {
                //    return;
                //}
            }
        }

        if ($('.page-item.active > a').text() != '') {
            var pStep = ($('.page-item.active > a')[0]).text;
            localStorage.setItem('hdnPreviousStep', pStep);
        }

        var count = 0;
        var mcount = 0;
        var activePage = activePageValue ? activePageValue : output.current_step ? parseInt(output.current_step) : 1;
        if (activePage == null)
            activePage = '<%= Session("AT_CurrentStep") %>';
        hdnPreviousToCurrent = hdnCurrentStep;
        var displayPage = 9;
        var pageItems = "";
        var pageItems1 = "";
        var startIndex = 0;
        var endIndex = 0;
        var mstartIndex = 0;
        var mendIndex = 0;

        var firstArray_New = output.stepButtons.split('[');
        var finaldata_New = firstArray_New.map(r => r.replace('],', '').replace(']', '').replace(/['"`]/g, '').split(','));
        finaldata_New = finaldata_New.slice(1);
        for (var lst = 0; lst < finaldata_New.length; ++lst) {
            checkStepStatus(finaldata_New[lst][0], finaldata_New[lst][3]);
        }
        //addLastStepButton();

        var finaldata_NewLength = finaldata_New.length;

        sethighlightstepcrtclstr(activePage);

        if ((TotalPagecount - activePage) < (displayPage / 2)) {
            startIndex = TotalPagecount - displayPage;
            startIndex = parseFloat(startIndex) < 0 ? 1 : startIndex;
            endIndex = TotalPagecount;
        }
        else if (activePage > (displayPage / 2)) {
            startIndex = (activePage - (displayPage / 2));
            endIndex = startIndex + displayPage;
        }
        else if (activePage < (displayPage / 3)) {
            startIndex = 1;
            if (TotalPagecount > displayPage)
                endIndex = startIndex + displayPage;
            else
                endIndex = TotalPagecount;
        }
        else {
            startIndex = 1;
            endIndex = startIndex + displayPage;
        }
        // if (isMobile) {
        if (activePage != 1)
            mstartIndex = activePage - 3;
        else
            mstartIndex = 1;

        if (mstartIndex < 1)
            mstartIndex = 1;

        mendIndex = mstartIndex + 9;
        if (mendIndex > TotalPagecount) {
            mstartIndex = TotalPagecount - 9;
            mendIndex = TotalPagecount;
        }
        //  }


        // if (isMobile) {
        pageItems1 += '<ul id="ulPagination" class="rc-pagination pagination-data"></ul>';
        //if (output.pipeOptions.hideNavigation || output.pipeOptions.disableNavigation) {
        //    pageItems1 += '<div class="align-items-center d-flex"><a href="javascript:void(0);" data-bs-toggle="modal" id="maQuestionIcon" data-bs-target=".tt-help-modal" class="font-size-18 left m-0 steps-help-icon-large-screen hide"><i class="fas fa-question-circle"></i></a><div class="footer_loader"><div class="align-items-center d-flex justify-content-center justify-content-lg-start m-0">';
        //}
        //else {
        //    pageItems1 += '<div class="align-items-center d-flex"><a href="javascript:void(0);" data-bs-toggle="modal" id="maQuestionIcon" data-bs-target=".tt-help-modal" class="font-size-18 left m-0 steps-help-icon-large-screen"><i class="fas fa-question-circle"></i></a><div class="footer_loader"><div class="align-items-center d-flex justify-content-center justify-content-lg-start m-0">';
        //}

        /*pageItems1 += '<div class="progressdiv" id="mdivEvaluatedBar" data-percent="10"><svg class="progress" width="50" height="50" viewport="0 0 100 100" version="1.1" xmlns="http://www.w3.org/2000/svg"><circle r="22" cx="20" cy="20" fill="transparent" stroke-dasharray="157" stroke-dashoffset="0"></circle><circle class="bar" r="22" cx="20" cy="20" fill="transparent" stroke-dasharray="157" stroke-dashoffset="0"></circle></svg></div></div></div></div>';*/

        if (output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && showOptions()) {
            pageItems1 += '<li class="page-item align-self-center d-none" style="cursor:pointer;" onclick=showtooltip()>&nbsp;<a class="bg-transparent border-0 p-1 page-link text-nowrap" href="javascript:void(0);"><i class="fa fa-ellipsis-v" aria-hidden="true"></i></a></li>';
        }

        //}
        //else {
        if (activePage <= 1) {
            pageItems += '<li class="rc-pagination-prev disabled d-none"><button disabled><img src = "../../img/icon/ext-prev.svg" class="icon" > </button ></li>';
        }
        else {
            pageItems += '<li class="rc-pagination-prev" onclick=setPagination(1)><button title="First"><img src = "../../img/icon/ext-prev.svg" class="icon" > </button ></li>'
        }
        //if (output.pipeOptions.hideNavigation || output.pipeOptions.disableNavigation) {
        //    if (activePage > 1) {
        //        pageItems += '<li class="page-item mx-2" onclick=setPagination(1)><a class="page-link btn-green" href="javascript:void(0);" tabindex="-1">First</a></li>';
        //    }
        //    else {
        //        pageItems += '<li class="page-item mx-2 d-none" onclick=setPagination(1)><a class="page-link btn-green" href="javascript:void(0);" tabindex="-1">First</a></li>';
        //    }
        //}
        if (activePage <= 1) {
            pageItems += '<li title="Previous" class="rc-pagination-prev disabled d-none"><button disabled><img src = "../../img/icon/Back.svg" class="icon me-1" > Previous</button ></li>'
        }
        else {
            pageItems += '<li title="Previous" class="rc-pagination-prev" onclick=previous()><button><img src = "../../img/icon/Back.svg" class="icon me-1" > Previous</button ></li>'
        }
        if (!output.pipeOptions.hideNavigation && !output.pipeOptions.disableNavigation) {
            for (var i = 0; i < TotalPagecount; i++) {
                count = count + 1;
                var color = stepColor(stepButtons[count]);
                var StepBtnMouseOverTxt = stepButtons[count][0];
                StepBtnMouseOverTxt = StepBtnMouseOverTxt.replace('#', '');
                StepBtnMouseOverTxt = StepBtnMouseOverTxt.replace(':', '-');

                if (i == 1) {
                    if (parseInt(TotalPagecount) > 9 && parseInt(activePage) > 6) {
                        pageItems += '<li class="page-item paginationSpace"><div class="pagination_tooltip dvjump" id="jump1" style="display:none;"><div class="blue alert alert-danger divError" style="display:none;" role="alert"><span class="errormessage" ></span></div ><div class="jumptooltip_wrapper"><span> Jump to step </span> <input type="text" class="input" id="txtjump1" name="jump" value="" /><input type="button" class="jumptooltip_btn foot_btn" onclick="jumpToPage(1)" value="OK">  <input type="button" class="jumptooltip_btn foot_btn" onclick="hideJump(1)" value="CANCEL"></div></div> <span style="cursor:pointer" title="Jump To" onclick="toggleJump(1)">&hellip;</span></li > ';
                    }
                }
                if (i == TotalPagecount - 1) {
                    if (parseInt(TotalPagecount) > 9 && parseInt(TotalPagecount) - activePage > 5) {
                        pageItems += '<li class="page-item paginationSpace"><div class="pagination_tooltip dvjump" id="jump2" style="display:none;"><div class="blue alert alert-danger divError" style="display:none;" role="alert"><span class="errormessage" ></span></div ><div class="jumptooltip_wrapper"><span> Jump to step </span> <input type="text" class="input" name="jump" id="txtjump2" value="" /><input type="button" class="jumptooltip_btn foot_btn" onclick="jumpToPage(2)" value="OK"> <input type="button" class="jumptooltip_btn foot_btn" onclick="hideJump(2)" value="CANCEL"></div></div><span style="cursor:pointer" title="Jump To" onclick="toggleJump(2)">&hellip;</span></li>';
                    }
                }
                if (activePage == count && count == 1) {
                    /*pageItems += '<li class="page-item active"><a class="page-link" href="javascript:void(0);" style="color:' + color + '">' + count + '</a></li>';*/
                    pageItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-3 rc-pagination-item-active" tabindex = "0" > <a rel="nofollow" style="color:' + color + '">' + count + '</a></li >';
                } else if (count == 1 && activePage != 5) {
                    /*pageItems += '<li class="page-item" onclick=setPagination(' + count + ')><a class="page-link" href="javascript:void(0);" style="color:' + color + '">' + count + '</a></li>';*/
                    pageItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-2 d-none d-lg-block" tabindex="0" onclick=setPagination(' + count + ')><a rel="nofollow" style="color:' + color + ' "> ' + count + '</a ></li >';
                }
                if (count > startIndex && count < endIndex) {
                    if (activePage == count) {
                        /*pageItems += '<li class="page-item active" onclick=setPagination(' + count + ')><a class="page-link" href="javascript:void(0);" style="color:' + color + '">' + count + '</a></li>';*/
                        pageItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-3 rc-pagination-item-active" tabindex="0" onclick=setPagination(' + count + ',true)><a rel="nofollow" style="color:' + color + '"> ' + count + '</a ></li >';
                    } else {
                        /*pageItems += '<li class="page-item" onclick=setPagination(' + count + ')><a class="page-link" href="javascript:void(0);" style="color:' + color + '">' + count + 'Hi</a></li>';*/
                        pageItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-2 d-none d-lg-block" tabindex="0" onclick=setPagination(' + count + ')><a rel="nofollow" style="color:' + color + '"> ' + count + '</a ></li >';
                    }
                }
            }
        }
        //
        if (activePage == count) {
            if (count > 0)
                /*pageItems += '<li class="page-item active"><a class="page-link" href="javascript:void(0);" style="color:' + color + '">' + count + '</a></li>';*/
                pageItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-3 rc-pagination-item-active" tabindex="0" onclick=setPagination(' + count + ',true)><a rel="nofollow" style="color:' + color + '"> ' + count + '</a ></li >';
            if (output.current_step < output.total_pipe_steps)
                pageItems += '<li title="Next" class="rc-pagination-next disabled"><button disabled>Next <img src = "../../img/icon/Next.svg" class="icon ms-1" ></button ></li>'
        }
        else {
            if (count > 0)
                /*pageItems += '<li class="page-item" onclick=setPagination(' + count + ')><a class="page-link" href="javascript:void(0);" style="color:' + color + '">' + count + 'Hi</a></li>';*/
                pageItems += '<li title="' + StepBtnMouseOverTxt + '" class="rc-pagination-item rc-pagination-item-2 d-none d-lg-block" tabindex="0" onclick=setPagination(' + count + ')><a rel="nofollow" style="color:' + color + '"> ' + count + '</a ></li >';
            pageItems += '<li title="Next" class="rc-pagination-next" id="LiBtnNext" onclick=next()><button title="Next">Next <img title="Next" src = "../../img/icon/Next.svg" class="icon ms-1" ></button ></li>';
            pageItems += '<li title="Last" class="rc-pagination-next" onclick=setPagination(' + TotalPagecount + ')><button title="Last"><img src = "../../img/icon/ext-next.svg" class="icon" ></button ></li>';
        }

        if (output.current_step == output.total_pipe_steps) {
            /*if ((!output.fromComparion && showFinish) || (output.doneOptions != undefined && output.doneOptions != null && output.doneOptions.logout == 'True')) {*/
            <%--if (!output.fromComparion && showFinish) {
                pageItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime(<%= Convert.ToInt32(Session(Pages.external_classes.Constants.Sess_LoginMethod))%>)"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            }

            /*if (output.fromComparion && output.doneOptions != undefined && output.doneOptions != null && output.doneOptions.stayAtEval.toLowerCase() == 'false') {*/
            if (output.fromComparion) {
                pageItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime()"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            }--%>

            if (!output.fromComparion && (showFinish || showSave)) {
                pageItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime(<%= Convert.ToInt32(Session(Pages.external_classes.Constants.Sess_LoginMethod))%>)"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            }
            else if (output.fromComparion && output.doneOptions.stayAtEval == 'True' && output.doneOptions.logout == 'True') {
                pageItems += '<li class="page-item mx-2 li_btn" onclick="finish_anytime(<%= Convert.ToInt32(Session(Pages.external_classes.Constants.Sess_LoginMethod))%>)"><a class="page-link btn-green" href="javascript:void(0);">Done</a></li>';
            }
        }

        var nextassessed = output.nextUnassessedStep != undefined || null ? eval(output.nextUnassessedStep[0]) : '0';
        var cstep = eval(output.current_step); //&& !($('#MainContent_disableNavigation').val())
        var showicon = true;
        //if ($('#MainContent_showUnassessed').val()) {
        if (output.pipeOptions.showUnassessed) {
            //(output.nextUnassessedStep != null) && (output.nextUnassessedStep[0] != output.current_step) && ( unassessed_data[0] != output.current_step || unassessed_data[1] > 1)
            if (nextassessed != 0 && nextassessed != null && typeof nextassessed != 'undefined' && nextassessed != cstep && activePage != count) {  //&& $('#hdnPageNumber').val() == cstep
                pageItems += '<li class="page-item li_btn " style="cursor:pointer;" onclick=next_unassessed()><a class="bg-transparent unassesed_btn page-link text-nowrap" href="javascript:void(0);">Next Unassessed</a></li>'
                var AutoAdvancehtml = '';
                if (showOptions()) {
                    /*  if (output.pipeOptions.disableNavigation) {*/
                    if (output.is_auto_advance) {
                        AutoAdvancehtml += '<li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" checked/>Auto Advance</a></li>';
                    }
                    else {
                        AutoAdvancehtml += '<li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()"/>Auto Advance</a></li>';
                    }
                    pageItems += '<li title="More" class="rc-pagination-next" aria-disabled="false" tabindex="0"><div class="btn-group" ><button class="btn btn-primary bg-white" type="button" id="defaultDropdown 01" data-bs-toggle="dropdown" data-bs-auto-close="true" aria-expanded="false"><img src="../../img/icon/Snow man.svg" class="icon"></button><ul class="dropdown-menu" aria-labelledby="defaultDropdown">' + AutoAdvancehtml + '</ul></div></li >';

                }
                else {
                    AutoAdvancehtml += '<li class="d-none"><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" />Auto Advance</a></li>';
                }

            }
            else {
                if (showOptions()) {
                    /*  if (output.pipeOptions.disableNavigation) {*/
                    AutoAdvancehtml = "";
                    if (output.is_auto_advance) {
                        AutoAdvancehtml += '<li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" checked/>Auto Advance</a></li>';
                    }
                    else {
                        AutoAdvancehtml += '<li><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()"/>Auto Advance</a></li>';
                    }
                    pageItems += '<li title="More" class="rc-pagination-next" aria-disabled="false" tabindex="0"><div class="btn-group" ><button class="btn btn-primary bg-white" type="button" id="defaultDropdown 01" data-bs-toggle="dropdown" data-bs-auto-close="true" aria-expanded="false"><img src="../../img/icon/Snow man.svg" class="icon"></button><ul class="dropdown-menu" aria-labelledby="defaultDropdown">' + AutoAdvancehtml + '</ul></div></li >';

                }
                else {
                    AutoAdvancehtml += '<li class="d-none"><a class="dropdown-item" href="javascript:void(0);"> <input type="checkbox" id="chkautoadvance" onchange="set_auto_advance()" />Auto Advance</a></li>';
                }
            }
        }
        if (output.page_type != 'atInformationPage' && output.page_type != 'atSensitivityAnalysis' && showOptions()) {
            pageItems += '<li class="page-item align-self-center d-none" style="cursor:pointer;" onclick=showtooltip()>&nbsp;<a class="bg-transparent border-0 p-1 page-link text-nowrap" href="javascript:void(0);"><i class="fa fa-ellipsis-v" aria-hidden="true"></i></a></li>';
        }

        //  }
        // var options = eval($('#MainContent_disableNavigation').val());

        setsteps(activePage);
        var hideNavigation = output.pipeOptions.hideNavigation;

        $('#pgntn1').html(pageItems1);
        if ($('#pgntn')[0].innerText == "")
            $('#pgntn').html(pageItems);
        checkpagination();

        if (hdnpage_type == 'atNonPWAllChildren' || hdnpage_type == 'atNonPWAllCovObjs') {
            if (hdnpairwise_type == 'mtRatings') {
                save_multi_ratings_on_next();
            }
            else if (hdnpairwise_type == 'mtDirect') {
                save_multi_direct_on_next();
            }
            if (activePage != hdnCurrentStep) {
                $('#hdnPageNumber').val(activePage);
                setTimeout(function () {
                    if (activePage != hdnCurrentStep)
                        $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
            }
        }
        else {
            if (hdnpage_type == "atPairwise") {
                activePageNo = activePage;
                //save_multis();
                if (activePage != output.current_step ) {
                    $('#hdnPageNumber').val(activePage);
                    setTimeout(function () {
                        if (activePage != hdnCurrentStep)
                            $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                    $('#MainContent_hdnPageNo').trigger('click');
                }
            }
            else if (hdnpage_type == "atAllPairwise") {
                activePageNo = activePage;
                if (hdnpairwise_type == 'ptGraphical') {
                    save_multi_pairwise_on_next();
                }
                if (activePage != hdnCurrentStep) {
                    $('#hdnPageNumber').val(activePage);
                    setTimeout(function () {
                        if (activePage != hdnCurrentStep)
                            $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                }
            }
            else {
                if (activePage != output.current_step) {
                    $('#hdnPageNumber').val(activePage);
                    setTimeout(function () {
                        if (activePage != hdnCurrentStep)
                            $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                    $('#MainContent_hdnPageNo').trigger('click');
                }
            }
        }
        // }

        if (pageLoad) {
            //$("#overlay").css('display', 'none');
            pageLoad = false;
        }
        if (output.current_step == hdnCurrentStep && currentPage) {
            $('#hdnPageNumber').val(output.current_step);
            setTimeout(function () {
                $('#MainContent_hdnPageNo').trigger('click');
            }, 10);
        }
    }

    var stepColor = function (step) {
        var color = '';
        if (step != undefined && step.length > 0) {
            for (var i = 0; i < step.length - 1; i++) {
                if (CSS.supports('color', step[i])) {
                    color = step[i];
                    break;
                }
            }
        }
        color = color == '' ? 'black' : color;
        return color;
    }

    var next_unassessed = function () {

        if (hdnpage_type == 'atSpyronSurvey') {
            var ret = save_respondentAnswer(hdnCurrentStep, hdnCurrentStep + 1, true);
            if (ret == false || submitSuccess == false) {
                alert(message);
                submitSuccess = false;
                message = "";
                return false;
            }
            else {
                $('#hdnPageNumber').val(output.nextUnassessedStep[0]);
            }
        }
        else if (hdnpage_type == 'atNonPWAllChildren' || hdnpage_type == 'atNonPWAllCovObjs') {
            if (hdnpairwise_type == 'mtRatings') {
                save_multi_ratings_on_next();
            }
            else if (hdnpairwise_type == 'mtDirect') {
                save_multi_direct_on_next();
            }
            $('#hdnPageNumber').val(output.nextUnassessedStep[0]);
        }
        else {
            $('#hdnPageNumber').val(output.nextUnassessedStep[0]);
        }

        setTimeout(function () {
            $('#MainContent_hdnPageNo').trigger('click');
        }, 10);

        //setTimeout(function () {
        //    if (output.nextUnassessedStep != undefined && output.nextUnassessedStep != null && output.nextUnassessedStep.length > 0) {
        //        $('#hdnPageNumber').val(output.nextUnassessedStep[0]);
        //        setTimeout(function () {
        //            $('#MainContent_hdnPageNo').trigger('click');
        //        }, 50);
        //    }
        //    //$.ajax({
        //    //    type: "POST",
        //    //    url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/GetNextUnassessed",
        //    //    data: JSON.stringify({
        //    //        StartingStep: output.current_step
        //    //    }),
        //    //    dataType: "json",
        //    //    async: false,
        //    //    contentType: "application/json; charset=utf-8",
        //    //    success: function (response) {
        //    //        $('#hdnPageNumber').val(response.d[0]);
        //    //        setTimeout(function () {
        //    //            $('#MainContent_hdnPageNo').trigger('click');
        //    //        }, 10);
        //    //    },
        //    //    error: function (response) {
        //    //    }
        //    //});
        //}, 800);
        //if (hdnpage_type == 'atSpyronSurvey') {
        //    $('#MainContent_hdnPageNo').trigger('click');
        //}
    }

    $("#btnstep").click(function () {
        var setdefaultval = 0;
        if (defaultValueActivepage == 2) {
            setdefaultval = defaultValueActivepage;
            setdefaultval = setdefaultval - 1;
        }

        loadStepsdata(true);

        setTimeout(function () {
            if ($('#stepparent li.active').position().top >= $('#stepparent').height() - 5) {
                //$('html.body,#stepparent').animate({
                //    scrollTop: $('#stepparent li.active').offset().top - 50
                //}, 'slow');
                $('#stepparent').scrollTop($('#stepparent li.active').position().top - 100);
            }
        }, 800);
    });

    function iconMenu() {
        if ($('.tg-aa-wrap').hasClass('hide')) {
            $('.tg-aa-wrap').removeClass('hide');
        } else {
            $('.tg-aa-wrap').addClass('hide');
        }
    }
    function previous() {
        $.removeCookie('SelectionID');
        block_unload_prompt = true;
        $("#overlay").css('display', 'flex');
        //var activeIndex = parseInt($(".page-item.active .page-link").text());
        var activeIndex = hdnCurrentStep;
        activeIndex = activeIndex > TotalPagecount ? 1 : activeIndex;
        activePageNo = parseInt(activeIndex - 1);
        if (!output.isPipeViewOnly) {
            if (hdnpage_type == 'atSpyronSurvey') {
                var ret = save_respondentAnswer(activeIndex, activePageNo, true, true);

                if (submitSuccess) {
                    SurveyAnswers = [];
                    answer = [];
                    SurveyPageQuestions = [];
                    $('#hdnPageNumber').val(activePageNo);
                }
            }
            else if (hdnpage_type == 'atNonPWAllChildren' || hdnpage_type == 'atNonPWAllCovObjs') {
                if (hdnpairwise_type == 'mtRatings') {
                    save_multi_ratings_on_next();
                }
                else if (hdnpairwise_type == 'mtDirect') {
                    save_multi_direct_on_next();
                }
                $('#hdnPageNumber').val(activeIndex - 1);
            }
            else {
                if (hdnpage_type == "atAllPairwise") {
                    //activePageNo = activePage;
                    if (hdnpairwise_type == 'ptGraphical') {
                        save_multi_pairwise_on_next();
                    }
                    $('#hdnPageNumber').val(activePageNo);
                }
                else {
                    $('#hdnPageNumber').val(activeIndex - 1);
                }
                //if (hdnpage_type == "atPairwise") {
                //    save_multis();
                //}
                //else {
                //    setPagination(activeIndex - 1);
                //    $('#hdnPageNumber').val(activeIndex - 1);
                //    $('#MainContent_hdnPageNo').trigger('click');
                //}
                //setPagination(activeIndex - 1);            
            }
        }
        else {
            $('#hdnPageNumber').val(activePageNo);
        }
        //$("#overlay").css('display', 'none');
        setTimeout(function () {
            $('#MainContent_hdnPageNo').trigger('click');
        }, 10);
        $('#MainContent_hdnPageNo').trigger('click');
    }

    function next(activePage) {
        //debugger;
        $.removeCookie('SelectionID');
        block_unload_prompt = true;
        $("#overlay").css('display', 'flex');
        //var activeIndex = parseInt($(".page-item.active .page-link").text());
        var activeIndex = hdnCurrentStep;
        activeIndex = activeIndex > TotalPagecount ? 1 : activeIndex;
        activePageNo = parseInt(activeIndex + 1);
        if (!output.isPipeViewOnly) {
            if (hdnpage_type == 'atSpyronSurvey') {
                var ret = save_respondentAnswer(hdnCurrentStep, parseInt(activeIndex + 1), true);
                if (ret == false) {
                    alert(message);
                    submitSuccess = false;
                    message = "";
                    return false;
                }
                else {
                    $('#hdnPageNumber').val(activeIndex + 1);
                }
            }
            else if (hdnpage_type == 'atNonPWAllChildren' || hdnpage_type == 'atNonPWAllCovObjs') {
                if (hdnpairwise_type == 'mtRatings') {
                    save_multi_ratings_on_next();
                }
                else if (hdnpairwise_type == 'mtDirect') {
                    save_multi_direct_on_next();
                }
                $('#hdnPageNumber').val(activeIndex + 1);
            }
            else {
                if (hdnpage_type == "atAllPairwise") {
                    //activePageNo = activePage;
                    if (hdnpairwise_type == 'ptGraphical') {
                        save_multi_pairwise_on_next();
                    }
                    $('#hdnPageNumber').val(activePageNo);
                }
                else {
                    $('#hdnPageNumber').val(activeIndex + 1);

                }
                //if (hdnpage_type == "atPairwise") {
                //    save_multis();
                //}
                //else {
                //    setPagination(activeIndex + 1);
                //    $('#hdnPageNumber').val(activeIndex + 1);
                //    $('#MainContent_hdnPageNo').trigger('click');
                //}
                //setPagination(activeIndex + 1);
            }
        }
        else {
            $('#hdnPageNumber').val(activePageNo);
        }
        //$("#overlay").css('display', 'none');
        setTimeout(function () {
            $('#MainContent_hdnPageNo').trigger('click');
        }, 10);
        $('#MainContent_hdnPageNo').trigger('click');
    }


    function GoToNext() {

        if (submitSuccess) {
            SurveyAnswers = [];
            answer = [];
            SurveyPageQuestions = [];
            $('#hdnPageNumber').val(activePageNo);
            setTimeout(function () {
                $('#MainContent_hdnPageNo').trigger('click');
            }, 10);
        }
    }
    function saveCurrentPage(activeIndex) {
        activeIndex = activeIndex == undefined || null ? 0 : activeIndex;
        $.ajax({
            type: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/setCurrentStep",
            data: JSON.stringify({
                stepNo: activeIndex
            }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                window.location.reload();
            },
            error: function (response) {
                alert('Somwthing went wrong');
                window.location.reload();
            }
        });
    }

    var baseUrl = '<%= ResolveUrl("~/") %>';
    $("#btnCurrentCluster, #mbtnCurrentCluster").click(function () {
        debugger;
        $.ajax({
            type: "POST",
            url: baseUrl + "AnytimeComparion/Login.aspx/loadHierarchy",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            success: function (data) {
                debugger;
                var accordionItem = '';
                var steps = data.d.data;
                accordionItem = getStepesDate(steps, accordionItem);
                $("#accordionFlushExample").html(accordionItem);
                setPagination();
                $("#overlay").css('display', 'none');
            },
            error: function () {
                //alert("error");
            }
        });
    });

    function getStepesDate(steps, accordionItem) {
        accordionItem += '<ul class="tree" id="main-tab">'
        steps.forEach(element => {
            if (element.length == 6 && element[5].length > 0) {
                accordionItem += '<li class="treee_section">'
                accordionItem += '<input type="checkbox" id=group checked />'
            } else {
                accordionItem += '<li class="tree_empty">'
            }
            if ($.isNumeric(element[4]) && parseInt(element[4]) > 0) {
                accordionItem += '<label class="w-100" for=group' + element[4] + '><span id="cluster-' + element[4] + '"onclick=setPagination(' + element[4] + ')  class="sethovercolor spncls" style="color:#ed8f2b;" ><span class="treeSpan">' + element[1] + '</span></span><span class="cluster_number" style="color:#A9A9A9;">&nbsp;(#' + element[4] + ')</span></label>'
            }
            else {
                accordionItem += '<label class="w-100"  for=group' + element[4] + '><span id="cluster-' + element[4] + '" class="sethovercolor spncls" style="color:#ed8f2b;" ><span class="treeSpan">' + element[1] + '</span><span></label>'
            }
            stepno[i++] = element[4];

            if (element.length == 6 && element[5].length > 0) {
                accordionItem = getStepesDate(element[5], accordionItem);
            }
            accordionItem += '</li>'
        });
        accordionItem += '</ul>'
        return accordionItem;
    }

    function sethighlightstepcrtclstr(actpage = 0) {
        var result = [];
        stepno.sort(function (a, b) {
            if (a > b) return 1;
            if (a < b) return -1;
            return 0;
        });

        for (var j = 1; j < stepno.length; j++) {
            result.push({ startpage: stepno[j - 1], endpage: stepno[j] });
        }
        result.push({ startpage: stepno[stepno.length - 1], endpage: TotalPagecount });
        var clusterpage = result.filter(x => actpage >= x.startpage && actpage < x.endpage);
        var sp = clusterpage.map(x => x.startpage);
        if ($('#main-tab #cluster-' + sp + '').length > 0) {
            $('#main-tab #cluster-' + sp + '').addClass('activecurrentcluster prm-color_brown');
            $('#cluster-' + sp).removeClass('spncls');
        }
        else {
            $('#main-tab #cluster-' + hdnCurrentStep).addClass('activecurrentcluster prm-color_brown');
            $('#cluster-' + hdnCurrentStep).removeClass('spncls');
        }
        //$('#main-tab #cluster-' + hdnCurrentStep + '').addClass('activecurrentcluster');
    }

    //function removeclass() {
    //    $('#main-tab li label span').removeClass('activecurrentcluster');
    //}

    var getEvaluationProgressPercentage = function (decimalPercent, isTooltip) {
        var valueReturn = 0;

        if (decimalPercent != undefined && decimalPercent != null && decimalPercent != '' && decimalPercent != '0' && parseFloat(decimalPercent) > 0) {
            //var total_evaluation = $('#MainContent_hdntotal_evaluation').val()
            valueReturn = output.total_evaluation.toString().length - 3; //getting how many digits more than thousand
            var decimalPoints = isTooltip ? ((valueReturn > 0 ? valueReturn : 0) + 3)
                : decimalPercent < 100 ? 1 : 0;

            valueReturn = parseFloat(parseFloat(decimalPercent).toFixed(1));
        }
        //$('#spnEvaluatedBar').attr('data-to', valueReturn);
        //$('#spnEvaluatedBar').text(valueReturn + '%');
        $('#divEvaluatedBar,#mdivEvaluatedBar').attr('data-percent', valueReturn);
        /*$('#divEvaluatedBar').text(valueReturn + '%');*/
        //return valueReturn;
        /* $('.progress-bar').css({ 'width': valueReturn + '%' });*/
        $('#FooterProgressBar').css({ 'width': valueReturn + '%' });
    }

    var save_respondentAnswer = function (stip, step, isFinish, byPassValid) {
        var canSubmit = false;
        // var message = "";
        var skip = false;
        SurveyAnswers.forEach(function (value, key) {
            if (!skip) {
                var surveyquestion = SurveyPageQuestions[key];
                var reg = new RegExp('^-?[0-9.\n\d]+$');
                if (surveyquestion.Type == 4) {
                    var options = [];
                    if (value[0] == "")
                        options = [];
                    else
                        options = value[0].split(";");
                    var totalAnsweredChb = options.length;
                    var totalVariants = surveyquestion.Variants.length;
                    if (surveyquestion.Variants[totalVariants - 1].Type == 2)
                        totalAnsweredChb -= 1;
                    var min = surveyquestion.MinSelectedVariants;
                    var max = surveyquestion.MaxSelectedVariants == 0 ? Number.MAX_VALUE : surveyquestion.MaxSelectedVariants;
                    if (min > totalAnsweredChb || max < totalAnsweredChb) {
                        canSubmit = true;
                        skip = true;
                        message = "Please select necessary count of items on list";
                    }
                }
                else if (output.pipeOptions.dontAllowMissingJudgment && surveyquestion.Type == 12 || surveyquestion.Type == 13) {
                    var min = surveyquestion.MinSelectedVariants;
                    var max = surveyquestion.MaxSelectedVariants == 0 ? Number.MAX_VALUE : surveyquestion.MaxSelectedVariants;
                    var totalAnsweredChb = (value[0].match(/true/g) || []).length;
                    if (min > totalAnsweredChb || max < totalAnsweredChb) {
                        canSubmit = true;
                        skip = true;
                        message = "Please select necessary count of items";
                    }
                    //if (value[0].indexOf('True') < 0) {
                    //    canSubmit = true;
                    //    skip = true;
                    //    message = "Please select necessary count of items";
                    //}
                }
                //surveyquestion.Type == 14 || 

                else if (surveyquestion.Type == 14 && value[0] != "") {
                    if (!value[0].match(reg) || !$.isNumeric(value[0])) {
                        message = "Please enter a valid number";
                        canSubmit = true;
                        skip = true;
                    }
                }
                else if (surveyquestion.Type == 15 && value[0] != "") {
                    var a = value[0].split('\n');
                    for (var i = 0; i < a.length; i++) {
                        if (!a[i].match(reg) || !$.isNumeric(a[i])) {
                            message = "Please enter a valid number";
                            canSubmit = true;
                            skip = true;
                            break;
                        }
                    }
                }
                if (value[2] == "False") {
                    if (!checkSurveyValue(value[0])) {
                        canSubmit = true;
                        skip = true;
                        message = "Please, complete all required questions.";
                    }
                }
            }
        });

        //if (canSubmit && parseInt(stip) < parseInt(step)) {
        if (message != "" && byPassValid == undefined && !output.isPipeViewOnly) {
            $("#overlay").css('display', 'none');
            //alert(message); //need to be changed into a modal alert!
            //submitSuccess = false;
            return false;
        }

        $.ajax({
            type: "POST",
            url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/saveRespondentAnswers",
            async: true,
            data: JSON.stringify({
                step: stip,
                RespondentAnswers: SurveyAnswers
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            success: function (data) {
                submitSuccess = true;
                if (isFinish) {
                    SurveyAnswers = [];
                    answer = [];
                    SurveyPageQuestions = [];

                    hdnpage_type = '';
                    GoToNext();
                    //$('#hdnPageNumber').val(step);
                    //$('#MainContent_hdnPageNo').trigger('click');
                }
                return true;
            },
            error: function (error) {
                //alert("error");
                return false;
            }
        });
    }

    var checkSurveyValue = function (value) {
        if (value == "" || value == " " || value == ":" || value == null)
            return false;
        return true;
    }

    var enabledLastStep = -1;
    var checkStepStatus = function (step, isUndefined) {
        if (output.pipeOptions.dontAllowMissingJudgment) {
            ////console.log("index: " + index + ", step: " + step + ", isUndefined: " + isUndefined + ", enabledLastStep: " + enabledLastStep + ", current_step: " + output.current_step + ", output.IsUndefined: " + output.IsUndefined);

            Number.isInteger(enabledLastStep)
            if (!Number.isInteger(enabledLastStep)) {
                enabledLastStep = -1;
            }
            if (enabledLastStep > 0) {
                if (output.isUndefined) {
                    enabledLastStep = hdnCurrentStep;
                    ////console.log("enabledLastStep: " + enabledLastStep);
                }

                if (enabledLastStep == hdnCurrentStep && !output.isUndefined && hdnCurrentStep != hdnTotalSteps) {
                    enabledLastStep = -1;
                }
            }

            if (isUndefined == 1) {
                ////console.log("isUndefined -> enabledLastStep: " + enabledLastStep);

                if (enabledLastStep < 0) {
                    enabledLastStep = parseInt(step);
                    ////console.log("isUndefined -> Changed enabledLastStep: " + enabledLastStep);
                }

                if (parseInt(step) > enabledLastStep) {
                    ////console.log("true isUndefined -> step: " + step + ", enabledLastStep: " + enabledLastStep);
                    return true;
                }
                else {
                    ////console.log("false isUndefined -> step: " + step + ", enabledLastStep: " + enabledLastStep);
                    return false;
                }
            }
            else {
                ////console.log("enabledLastStep: " + enabledLastStep);
                if (enabledLastStep < 0) {
                    enabledLastStep = hdnTotalSteps;

                    var hdnfirstArray_New = output.stepButtons.split('[');
                    var hdnfinaldata_New = hdnfirstArray_New.map(r => r.replace('],', '').replace(']', '').replace(/['"`]/g, '').split(','));
                    hdnfinaldata_New = hdnfinaldata_New.slice(1);

                    for (var lst = 0; lst < hdnfinaldata_New.length; ++lst) {
                        if (hdnfinaldata_New[lst][3] == "1" && (hdnCurrentStep != parseInt(hdnfinaldata_New[lst][0]))) {
                            enabledLastStep = parseInt(hdnfinaldata_New[lst][0]);
                            break;
                        }
                    }

                    //for (var i = 0; i < stepButtons.length; ++i) {
                    //    if (stepButtons[i][3] == "1" && (hdnCurrentStep != parseInt(stepButtons[i][0]))) {
                    //        enabledLastStep = parseInt(stepButtons[i][3]);
                    //        var stpbtn0 = stepButtons[i][0];
                    //        var stpbtn3 = stepButtons[i][3];
                    //        break;
                    //    }
                    //}
                    ////console.log("Changed enabledLastStep: " + enabledLastStep);
                }

                if (parseInt(step) > enabledLastStep) {
                    ////console.log("true -> step: " + step + ", enabledLastStep: " + enabledLastStep);
                    return true;
                }
                else {
                    ////console.log("false -> step: " + step + ", enabledLastStep: " + enabledLastStep);
                    return false;
                }
            }
        }
        else {
            if (enabledLastStep < 0) {
                enabledLastStep = hdnTotalSteps;
            }
            return false;
        }
    }

    //$(document).on("mouseover", "div.classforeveryevent", function (e) {
    //    setPagination(hdnCurrentStep);
    //});

    //$(document).on("click", "div.classforeveryevent", function (e) {
    //    setPagination(hdnCurrentStep);
    //});

    //$(document).on("keypress", "div.classforeveryevent", function (e) {
    //    setPagination(hdnCurrentStep);
    //});

    //$(document).on("submit", "div.classforeveryevent", function (e) {
    //    setPagination(hdnCurrentStep);
    //});

    var EnableDisablePagination = function (bool, onlyNext) {
        bool = bool == true ? output.pipeOptions.dontAllowMissingJudgment : false;
        if (bool) {
            /*$('li.page-item.active').nextAll('li').addClass('disabledli');*/
            $('li.rc-pagination-item.rc-pagination-item-active').nextAll('li').removeClass('disabledli');
            $('li.rc-pagination-item').removeClass('disabled');
            //$('li.rc-pagination-item.active').nextAll('li:not(.li_btn)').addClass('disabledli');
            $('li.rc-pagination-item.rc-pagination-item-active').nextAll('li').addClass('disabledli');
            /*  $('li.rc-pagination-item.rc-pagination-item-active').nextAll('li').off('click');*/
            $('#stepparent li.active').nextAll('li').removeClass('disabledli');
            $('#stepparent li').removeClass('disabled');
            $('#stepparent li.active').nextAll('li').addClass('disabledli');
            $('#mbtnlinthreedot').removeClass('disabledli');
            $('#mbtnUnass').addClass('disabledli');
            if (onlyNext != undefined || onlyNext != null || onlyNext == false) {
                $('li.li_btn').removeClass('disabledli');
                $('#LiBtnNext').removeClass('disabledli');
                $('#mbtnNext').removeClass('disabledli');
                $('#mbtnUnass').removeClass('disabledli');
                for (var pgitem = 0; pgitem < $('li.rc-pagination-item').length; ++pgitem) {
                    if ($('li.rc-pagination-item')[pgitem].innerText <= enabledLastStep) {
                        $('li.rc-pagination-item')[pgitem].classList.remove('disabledli');
                    }
                }
                $('li.rc-pagination-item.li_btn').removeClass('disabledli');
                for (var paritem = 0; paritem < $('#stepparent li').length; ++paritem) {
                    if ($('#stepparent li a')[paritem].id <= enabledLastStep) {
                        $('#stepparent li')[paritem].classList.remove('disabledli');
                    }
                }
            }
        }
        else {
            if (onlyNext) {
                $('li.rc-pagination-item.rc-pagination-item-active').next('li').removeClass('disabledli');
                $('li.rc-pagination-item').removeClass('disabled');
                $('#stepparent li.active').next('li').removeClass('disabledli');
                $('#stepparent li').removeClass('disabled');
                $('li.rc-pagination-item.li_btn').removeClass('disabledli');
            }
            else {
                $('li.rc-pagination-item.rc-pagination-item-active').nextAll('li').removeClass('disabledli');
                $('li.rc-pagination-item').removeClass('disabled');
                $('#stepparent li.active').nextAll('li').removeClass('disabledli');
                $('#stepparent li').removeClass('disabled');
            }
        }
    }

    var finish_anytime = function (LoginMethod) {
        //case 12155 - login method to track if user signs in to login page or via hash link
        setPagination();
        $("#overlay").css('display', 'flex');
        // case 15140 - to continuously save_survey
        if (output.page_type == "atSpyronSurvey" || output.page_type == "") {
            //$scope.$broadcast("save_survey");
            save_respondentAnswer(output.current_step, output.current_step, true, byPassValid);
        }

        window.onbeforeunload = null;
        if (typeof (LoginMethod) == "undefined") {
            if (output.doneOptions.redirect == 'True' && output.doneOptions.url != "") {
                redirect_to_page(output.doneOptions.url);
            }
            else if (output.doneOptions.openProject == 'True' && output.nextProject != -1) {
                /*window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");*/
                window.open(output.doneOptions.nextProjectURL, "_self", "");
            }
            else if (output.doneOptions.stayAtEval == 'True') {
                setPagination();
                window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");
            }
            else {
                setPagination();
                if (confirm("You are done. Click OK to close this tab/window.")) {
                    window.top.close();
                }
                else {
                    $("#overlay").css('display', 'none');
                    return false;
                }
            }
        }
        else {
            if (output.doneOptions.logout == 'True') {
                if (confirm("Thank you! Click OK to logout.")) {
                    if (output.doneOptions.redirect != 'True') {
                        window.open(getBaseUrl() + "AnytimeComparion/Pages/you-are-done.aspx", "_self");
                    }
                    else {
                        if (output.doneOptions.url != "")
                            redirect_to_page(output.doneOptions.url);
                        else
                            window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");
                    }
                }
                else {
                    $("#overlay").css('display', 'none');
                    return false;
                }

                setTimeout(function () {
                    $.ajax({
                        type: "POST",
                        url: baseUrl + "AnytimeComparion/Login.aspx/logout",
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        async: true,
                        success: function (data) {
                            if (output.doneOptions.redirect == 'True') {
                                if (output.doneOptions.url != '') {
                                    redirect_to_page(output.doneOptions.url);
                                }
                                else {
                                    redirect_to_page(getBaseUrl());
                                }
                            }
                            else if (output.doneOptions.closeTab == 'True') {
                                var is_explorer = false;
                                is_explorer = navigator.appName == "Microsoft Internet Explorer" || !!(navigator.userAgent.match(/Trident/) || navigator.userAgent.match(/rv:11/)) || (typeof $.browser !== "undefined" && $.browser.msie == 1);
                                if (is_explorer != undefined && is_explorer != null && is_explorer)
                                    window.open(location.href, "_self").close();
                                //else if (output.doneOptions.logout == 'True')
                                //{
                                //    window.open(getBaseUrl(), "_self");
                                //}
                                else {
                                    window.open(getBaseUrl() + "AnytimeComparion/Pages/you-are-done.aspx", "_self");
                                }
                            }
                            else {
                                if (LoginMethod == 1 && !output.doneOptions.redirect == 'True') {
                                    redirect_to_page(getBaseUrl() + "evaluationDone");
                                    return true;
                                } else {
                                    redirect_to_page(getBaseUrl());
                                }
                            }
                        },
                        error: function (error) {
                            //alert("error");
                        }
                    });
                }, 200);
            }
            else {
                if (output.doneOptions.redirect == 'True') {
                    if (output.doneOptions.url != "")
                        redirect_to_page(output.doneOptions.url);
                    else
                        window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");
                }
                else if (output.doneOptions.closeTab == 'True') {
                    window.open(getBaseUrl() + "/AnytimeComparion/Pages/you-are-done.aspx", "_self");
                }
                else if (output.doneOptions.openProject == 'True') {
                    /*window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");*/
                    window.open(output.doneOptions.nextProjectURL, "_self", "");
                }
                else if (output.doneOptions.stayAtEval == 'True') {
                    setPagination();
                    window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");
                }
                else {
                    redirect_to_page(getBaseUrl());
                }
            }
        }
    }

    var redirect_to_page = function (url) {
        block_unload_prompt = true;
        if (url.indexOf("http") == -1 && url != getBaseUrl()) {
            url = "http://" + url;
        }
        window.location.href = url;
    }

    var getBaseUrl = function () {
        return window.location.origin ? window.location.origin + "/" : window.location.protocol + "/" + window.location.host + "/";
    }

    function currentClusterOnClick() {
        $('#btnCurrentCluster').click();
    }

    function stepValueOnClick() {
        //alert("Button Clicked");
        var setdefaultval = 0;
        if (defaultValueActivepage == 2) {
            setdefaultval = defaultValueActivepage;
            setdefaultval = setdefaultval - 1;
        }

        loadStepsdata(true);

        setTimeout(function () {
            if ($('#stepparent li.active').position().top >= $('#stepparent').height() - 5) {
                //$('html.body,#stepparent').animate({
                //    scrollTop: $('#stepparent li.active').offset().top - 50
                //}, 'slow');
                $('#stepparent').scrollTop($('#stepparent li.active').position().top - 100);
            }
        }, 800);
    }

    $('.hide_legent_mob').on('click', function (e) {
        $('.step_legent').toggleClass("hide_lagents"); //you can list several class names
        e.preventDefault();
    });

    //$(document).ready(function () {
    //    $('.rc-pagination-item-active').click(function () {
    //        alert('Page reload called');
    //    });
    //});
</script>
