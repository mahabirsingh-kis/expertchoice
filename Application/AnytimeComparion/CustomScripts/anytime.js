var checked_boxes = 0, node_names = [], inconsistency = false, textField = null, matrixTargetID = -1, ClipBoardData = '', SurveyAnswers = [], answer = [];
var SurveyPageQuestions = [], hdnpage_type = '', node = '', node_guid = '0', node_id = 0, node_location = '0', node_name = '0', isExtremeMessage = false;
var dontAllowMissingJudgment = '', extremeMessage = '', outputvalue = '', pairwise_type = '', is_auto_advance = '', advantages = 0, IsUndefined = '', comment = '';
var is_comment_updated = false, is_multi = false, activePageNo = 0, graphical_slider = [], numericalSlider = [], main_bar = [], main_bar_txt = [], outerIndex = -1;
var left_input = [], right_input = [], sliderLeft = [], sliderRight = [], oldSliderValue = [], fFirst = true, graphical_switch = true, hdnpairwise_type = '';
var node_type = '0'; // -1 if parent, 2 if alternative then 3 if wrt
var node_type_text = '', Guids = '';
var multi_ratings_row = [], selected_multi_data = [], multi_direct_value = [], active_multi_index = 0, multi_last_index = 0, activate_first_row = true, multi_non_pw_data = [], old_multi_non_pw_data = [];
var listItems = '', dropdownHeader = '', chevronIcon = '', dropdownListBody = '', dropdownHeaderValue = '', isDropdownDisabled = '', dropdown = '', new_active_multi_index = 0;
var is_judgment_screen = true, multi_collapsed_info_docs = [], temp_collapsed_info_docs = [true, true, true, true, true];
//multi pairwise
var participant_slider = [], multi_pw_data = [], multi_data = [], snapValues = [], matrixCellIDs = [];
//pairwise
var collapsed_info_docs = [], infodocSizes = null, info_docs_headings = [];
//multi direct
var at_multi_direct_slider = [], at_multi_direct_input = [];
//single direct
var at_single_direct_slider = 0, at_single_direct_input = 0, current_row = 0;
var hdnMultiPwData = [], output = '';
var main_bar1 = '', main_bar2 = '';
var judgmentTableValues = {
    pairsData: [],
    objsData: []
}
var isReadOnly = false;
//Local Results
var currentColumn = null, reverseLocal = null, reverseGlobal = null, columnname = '', columntosort = -1, columnsortGlobal = null, columnsortLocal = null;
var columns = [["nodeID", "Index"], ["nodeName", "Name"], ["yourResults", "Your Results"], ["combine", "Combined"]], orderType = 'asc';
var hdnsortparam = null, hdnsortorder = null, hdnCookieProjName = null, sortingCount = 0, hdnNormalization = '';
var block_unload_prompt = false, defaultSorting = false, matrixCellIDs = [];
//Rating.ascx
var ratings_data = [], singleRatingTempDirectValue = "";
var RedirecttoNext = true, showFinish = false, showSave = false;
var projectLockedInfo = { "status": false, "message": "" };
var selectedNode = 0;
var isSortByPriority = false, AllowAddStep = true, isreview = 'false';
var OldDropDownListID = "0";
var editbuttonClick = false;

function ShowDropDownList(DropDownListID) {
    if (OldDropDownListID == "0") {
        $(DropDownListID).show();
        OldDropDownListID = DropDownListID;
    }
    else {
        if (OldDropDownListID != DropDownListID) {
            $(OldDropDownListID).hide();
            $(DropDownListID).show();
            OldDropDownListID = DropDownListID;
        }
        else {
            $(OldDropDownListID).hide();
            OldDropDownListID = "0";

        }

    }
}
function ShowDropDownListDesktop(DropDownListID) {
    if (OldDropDownListID == "0") {
        $(DropDownListID).show();
        OldDropDownListID = DropDownListID;
    }
    else if (DropDownListID.id == OldDropDownListID.id) {
        if (output.multi_non_pw_data.length > 0) {
            $(OldDropDownListID).hide();
            //$(DropDownListID).show();
            OldDropDownListID = "0";
        }
        else {
            $(OldDropDownListID).hide();
            OldDropDownListID = "0";
        }
    }
    else {
        if (DropDownListID.id.indexOf(OldDropDownListID.id) == -1) {
            if (OldDropDownListID != DropDownListID) {
                $(OldDropDownListID).hide();
                $(DropDownListID).show();
                OldDropDownListID = DropDownListID;
            }
            else {
                $(OldDropDownListID).hide();
                OldDropDownListID = "0";

            }
        }
        if (DropDownListID.id == OldDropDownListID.id) {
            $(OldDropDownListID).hide();
            $(DropDownListID).show();
            OldDropDownListID = DropDownListID;
        }
    }
}
function HideDropDownList(DropDownListID) {
    $(DropDownListID).hide();
}

$(document).ready(function () {
    ShowModels();
    /*   document.addEventListener('DOMContentLoaded', SetCssHieghtDynamicMobileView());*/
    //setTimeout(function ()
    //{
    //    SetCssHieghtDynamicMobileView();   
    //}, 3800);
    /*$.cookie('ShowJudgTable', true);*/
    //pageasncronize();
    if (output.isPipeViewOnly) {
        $('.master_wrapper').addClass('view_only_wrapper');
        $('#page-topbar,#topHeader').hide();
        $('#divViewOnlyMessage').removeClass('hide');
        var html = '(&lt;<a href="mailto:' + output.read_only_user + '">' + output.read_only_user + '</a>&gt;,' + output.read_only_username + ')';
        $('#spnReadonly').html(html);
    }
    $('#checkout-shipping-address-FirstNode').resizable({
        stop: function (event, ui) {
            if (confirm("You have changed the display size for information documents at this level of the display. Would you like this setting to be applied for all participants in this model?")) {
                var element = ui.element;
                var index = 1;
                var height = ui.size.height;
                var width = ui.size.width;
                set_infodoc_sizes(width, height, index);
            }
            else
                return false;
        }
    });

    $(".select-legends-info").click(function () {
        $(".show-legend-wrapper").toggle();
    });
    //$(".legends-description-wrapper").click(function () {
    //    $(".legends-description-info").toggle();
    //});
    $('#checkout-shipping-address-SecondNode').resizable({
        // animate: true,
        stop: function (event, ui) {
            if (confirm("You have changed the display size for information documents at this level of the display. Would you like this setting to be applied for all participants in this model?")) {
                var element = ui.element;
                var index = 1;
                var height = ui.size.height;
                var width = ui.size.width;
                set_infodoc_sizes(width, height, index);
            }
            else
                return false
        }
    });

    $("#checkout-shipping-address-SecondNodeWRT").resizable({
        // animate: true,
        stop: function (event, ui) {
            if (confirm("You have changed the display size for information documents at this level of the display. Would you like this setting to be applied for all participants in this model?")) {
                var element = ui.element;
                var index = 2;
                var height = ui.size.height;
                var width = ui.size.width;
                set_infodoc_sizes(width, height, index);
            }
            else
                return false;
        }
    });

    $('#checkout-shipping-address-FirstNodeWRT').resizable({
        //animate: true,
        stop: function (event, ui) {
            if (confirm("You have changed the display size for information documents at this level of the display. Would you like this setting to be applied for all participants in this model?")) {
                var element = ui.element;
                var index = 2;
                var height = ui.size.height;
                var width = ui.size.width;
                set_infodoc_sizes(width, height, index);
            }
            else {
                return false;
            }
        }
    });


    $('.hideinfoicon').css('display', 'none');
    $("#showtooltiptext").css("display", "none");
    $("#checkboxlabeltext").mouseover(function (event) {
        $("#showtooltiptext").css("display", "block");
    });
    $("#checkboxlabeltext").mouseout(function (event) {
        $("#showtooltiptext").css("display", "none");
    });

    $("#showcheckboxbestfit").css("display", "none");
    $("#checkboxbestfit").mouseover(function (event) {
        $("#showcheckboxbestfit").css("display", "block");
    });
    $("#checkboxbestfit").mouseout(function (event) {
        $("#showcheckboxbestfit").css("display", "none");
    });

    $('#tblGlobalResults thead th, #tblLocalResults thead th').click(function () {
        var table = $(this).parents('table').eq(0)
        var rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()))
        var th = table.find('th i');
        th.removeClass();
        th.addClass('fas fa-sort ms-2');
        $.cookie('hdnsortparam', $(this).index());
        hdnsortparam = $.cookie("hdnsortparam");
        hdnsortorder = $.cookie("hdnsortorder");
        $.cookie("hdnCookieProjName", output.name);
        var thead = $("#tblLocalResults thead th i").eq($(this).index());
        if (hdnsortparam != undefined && hdnsortparam != null && hdnsortparam != '' && parseInt(hdnsortparam) != $(this).index()) {
            thead = $("#tblLocalResults thead th i").eq(hdnsortparam);
        }

        if (output.page_type == 'atShowGlobalResults') {
            thead = $("#tblGlobalResults thead th i").eq($(this).index());
            if (hdnsortparam != undefined && hdnsortparam != null && hdnsortparam != '' && parseInt(hdnsortparam) != $(this).index()) {
                thead = $("#tblLocalResults thead th i").eq($(this).index());
            }
        }


        if (hdnsortorder != undefined && hdnsortorder != null && hdnsortorder != '' && hdnsortparam != undefined && hdnsortparam != null && hdnsortparam != ''
            && parseInt(hdnsortparam) == $(this).index() /*&& sortingCount == 0*/) {
            var DefaultSort = $.cookie('defaultsort');
            this.asc = hdnsortorder == 'asc' ? false : true;
            sortingCount = 1;
        }
        else {
            this.asc = !this.asc
        }
        if (!this.asc) {
            rows = rows.reverse()
            orderType = 'desc';
            thead.removeClass('fa-sort')
            thead.addClass('fa-caret-down')
            $.cookie('hdnsortorder', 'desc');
        }
        else {
            orderType = 'asc';
            thead.removeClass('fa-sort')
            thead.addClass('fa-caret-up')
            $.cookie('hdnsortorder', 'asc');
        }
        for (var i = 0; i < rows.length; i++) { table.append(rows[i]) }
    });

    function comparer(index) {
        return function (a, b) {
            var valA = getCellValue(a, index), valB = getCellValue(b, index)
            return $.isNumeric(valA.replace('%', '')) && $.isNumeric(valB.replace('%', '')) ? parseFloat(valA) - parseFloat(valB) : valA.toString().localeCompare(valB)
        }
    }


    function getCellValue(row, index) { return $(row).children('td').eq(index).text() }

    $('#chkLegend').change(function () {
        $('#divRedoJudg').removeClass();
        if ($(this).is(':checked')) {
            $('#divRedoJudg').addClass('col-lg-10');
            $('#divlegent_box').removeClass('d-none');
        }
        else {
            $('#divRedoJudg').addClass('col-lg-12');
            $('#divlegent_box').addClass('d-none');
        }
    });

    $('#chkRank').change(function () {
        /*   $('input[id="reviewjudgment"]').prop('checked', true);*/
        /*     $('input[id="reviewjudgment"]').trigger('change');*/
        if ($('#reviewjudgment').is(':checked')) {
            $('input[id="reviewjudgment"]').trigger('change');
        }
        else {
            $('input[id="changejudgment"]').trigger('change');
        }
        if ($(this).is(':checked')) {
            $('.rank-value-info').removeClass('d-none');
        }
        else {
            $('.rank-value-info').addClass('d-none');
        }
    });

    $('#chkBestFit').change(function () {
        //$('input[id="reviewjudgment"]').prop('checked', true); 
        /*     $('input[id="reviewjudgment"]').trigger('change');*/
        if ($('#reviewjudgment').is(':checked')) {
            $('input[id="reviewjudgment"]').trigger('change');
        }
        else {
            $('input[id="changejudgment"]').trigger('change');
        }

        if ($(this).is(':checked')) {
            $('.bestfit-value-info').removeClass('d-none');
        }
        else {
            $('.bestfit-value-info').removeClass('d-none');
            $('.bestfit-value-info').addClass('d-none');
        }
    });

    $('input[type="radio"][name="inconsistency"]').change(function () {
        if (this.id == "reviewjudgment") {
            /*  $('.divReviewjudgment').removeClass('d-none');*/
            /*  $('.divMakeChanges').addClass('d-none');*/
            $('.matrix-cell').prop('readonly', true);
            $('.pwnl-click').removeClass('d-none');
            //bindDynamicHtml('Judgment_table');
            //saveOBPriority('True');
        }
        else if (this.id == "changejudgment") {
            /*   $('.divReviewjudgment').addClass('d-none');*/
            /*  $('.divMakeChanges').removeClass('d-none');*/
            $('.matrix-cell').prop('readonly', false);
            $('.pwnl-click').addClass('d-none');
            /*  $('.divRank_BestFit').addClass('d-none');*/
            //$('.spnBestFit').removeClass('d-none');
            //$('.spnBestFit').addClass('d-none');
            //$('.spnRank').removeClass('d-none');
            //$('.spnRank').addClass('d-none');

            //$('#chkBestFit').prop('checked', false);
            //$('#chkRank').prop('checked', false);
        }
        else {
            $('.divReviewjudgment').addClass('d-none');
            /*  $('.divMakeChanges').removeClass('d-none');*/
            $('.matrix-cell').prop('readonly', false);
        }
        if ($('#chkBestFit').is(':checked')) {
            $('.bestfit-value-info').removeClass('d-none');
        }
        else {
            $('.bestfit-value-info').removeClass('d-none');
            $('.bestfit-value-info').addClass('d-none');
        }
        if ($('#chkRank').is(':checked')) {
            $('.divRank_BestFit').removeClass('d-none');
        }
        else {
            $('.divRank_BestFit').removeClass('d-none');
            $('.divRank_BestFit').addClass('d-none');
        }

    });

    //$('#ddlNormalization,#mlddlNormalization').change(function () {
    //    $.cookie('Normalization', $(this).val());
    //    normalization_Change($(this).val(), false, 'LocalResult');
    //});

    //$('#ddlGlobalNormalization,#mddlGlobalNormalization').change(function () {
    //    $('#tblGlobalResults').addClass('d-none');
    //    $.cookie('Normalization', $(this).val());
    //    normalization_Change($(this).val(), true, 'GlobalResult');
    //});

    //$('#sortNormalization').change(function () {
    //    sort_Results($(this).val(), true);
    //    $.cookie('Normalization', $('#mddlGlobalNormalization').val());
    //    normalization_Change($('#mddlGlobalNormalization').val(), true, 'GlobalResult');
    //});

    //$('#mlsortNormalization').change(function () {
    //    sort_Results($(this).val(), false);
    //    $.cookie('Normalization', $('#mlddlNormalization').val());
    //    normalization_Change($('#mlddlNormalization').val(), false, 'LocalResult');
    //});

    pageTypeWiseFun();

    $('.clcheckboxes').each(function () {
        var chtml = $(this).html();
        $(this).html('');
        var newhtml = chtml.replace('<b>', '').replace('</b>', '');
        $(this).html(newhtml);
    });

    $('.checkbold').each(function () {
        var chtml = $(this).html();
        $(this).html('');
        var newhtml = chtml.replace('<b><input', '<input').replace('</div></b>', '</div>');
        $(this).html(newhtml);
    });

    $("#chkkpsort").change(function () {
        var ischecked = ($('#chkkpsort').is(':checked'));

        isAlternativesSorted = ischecked ? true : false;
        $("input[name=cbKeepSorted]").prop("checked", isAlternativesSorted);

        $('#DSACanvas').sa("option", "SAAltsSortBy", (isAlternativesSorted ? 1 : 0));
        $('#DSACanvas').sa("sortAlternatives");
        $('#DSACanvas').sa("redrawSA");
    });

    $("#checkboxesFirstNode,#checkboxesSecondNode,#checkboxesSecondNodeWRT,#checkboxesFirstNodeWRT,#checkboxes5,#checkboxes1,#checkboxes6,#checkboxes3").change(function () {
        var Id = this.id.substring(10);
        if (cookieValue == undefined || cookieValue == null || cookieValue.trim() == 'false') {
            if (Id == "FirstNode" || Id == "SecondNode") {

                if (Id == "FirstNode") {
                    set_collapse_cookies('left-node');
                    //update_infodoc_params('left-node', '' + output.LeftNodeGUID + '', '' + output.RightNodeGUID + '');
                }
                else if (Id == "SecondNode") {
                    set_collapse_cookies('right-node');
                    //update_infodoc_params('right-node', '' + output.RightNodeGUID + '', '' + output.LeftNodeGUID + '');
                }

                //    $(".checkout-shipping-address5").toggle($(this)[0].checked);
                //    if ($(this)[0].checked) {
                //        $('.checkBoxIcon5').removeClass('fa fa-plus-square');
                //        $('.checkBoxIcon5').addClass('fa fa-minus-square');
                //    }
                //    else {
                //        $('.checkBoxIcon5').removeClass('fa fa-minus-square');
                //        $('.checkBoxIcon5').addClass('fa fa-plus-square');
                //    }

                var is_hidden = $(".checkout-shipping-address5").is(":visible") ? false : true;
                if (is_hidden) {
                    $(".checkout-shipping-address5").toggle(true);
                    $('.checkBoxIcon5').removeClass('fa fa-plus-square');
                    $('.checkBoxIcon5').addClass('fa fa-minus-square');
                }
                else {
                    $(".checkout-shipping-address5").toggle(false);
                    $('.checkBoxIcon5').removeClass('fa fa-minus-square');
                    $('.checkBoxIcon5').addClass('fa fa-plus-square');
                }
            }
            else if (Id == "SecondNodeWRT" || Id == "FirstNodeWRT") {

                if (Id == "FirstNodeWRT") {
                    set_collapse_cookies('wrt-left-node');
                    //update_infodoc_params('wrt-left-node', '' + output.LeftNodeGUID + '', '' + output.ParentNodeGUID + '');
                }
                else if (Id == "SecondNodeWRT") {
                    set_collapse_cookies('wrt-right-node');
                    //update_infodoc_params('wrt-right-node', '' + output.RightNodeGUID + '', '' + output.ParentNodeGUID + '');
                }
                //$(".checkout-shipping-address1").toggle(this.checked);
                //if ($(this)[0].checked) {
                //    $('.checkBoxIcon1').removeClass('fa fa-plus-square');
                //    $('.checkBoxIcon1').addClass('fa fa-minus-square');
                //}
                //else {
                //    $('.checkBoxIcon1').removeClass('fa fa-minus-square');
                //    $('.checkBoxIcon1').addClass('fa fa-plus-square');
                //}
                var is_hidden = $(".checkout-shipping-address1").is(":visible") ? false : true;
                if (is_hidden) {
                    $(".checkout-shipping-address1").toggle(true);
                    $('.checkBoxIcon1').removeClass('fa fa-plus-square');
                    $('.checkBoxIcon1').addClass('fa fa-minus-square');
                }
                else {
                    $(".checkout-shipping-address1").toggle(false);
                    $('.checkBoxIcon1').removeClass('fa fa-minus-square');
                    $('.checkBoxIcon1').addClass('fa fa-plus-square');
                }
            }
            else {
                //$("#checkout-shipping-address" + Id).toggle(this.checked);
                //if ($(this)[0].checked) {
                //    $('#checkBoxIcon' + Id).removeClass('fa fa-plus-square');
                //    $('#checkBoxIcon' + Id).addClass('fa fa-minus-square');
                //}
                //else {
                //    $('#checkBoxIcon' + Id).removeClass('fa fa-minus-square');
                //    $('#checkBoxIcon' + Id).addClass('fa fa-plus-square');
                //}
                var is_hidden = $("#checkout-shipping-address-" + Id).is(":visible") ? false : true;
                if (is_hidden) {
                    $("#checkout-shipping-address-" + Id).removeClass('d-none');
                    $('#checkBoxIcon' + Id).removeClass('fa fa-plus-square');
                    $('#checkBoxIcon' + Id).addClass('fa fa-minus-square');
                }
                else {
                    $("#checkout-shipping-address-" + Id).removeClass('d-none');
                    $("#checkout-shipping-address-" + Id).addClass('d-none');
                    /*$('.checkout-shipping-address1').addClass('d-none');*/
                    $('#checkBoxIcon' + Id).removeClass('fa fa-minus-square');
                    $('#checkBoxIcon' + Id).addClass('fa fa-plus-square');
                }
            }
        }
        else {
            var NodeId = '';
            if (Id == "FirstNode" || Id == "SecondNode") {
                if (Id == "FirstNode") {
                    set_collapse_cookies('left-node');
                    NodeId = 5;
                }
                else if (Id == "SecondNode") {
                    set_collapse_cookies('right-node');
                    NodeId = 6;
                }
                var is_hidden = $(".checkout-shipping-address" + NodeId).is(":visible") ? false : true;
                if (is_hidden) {
                    $(".checkout-shipping-address" + NodeId).toggle(true);
                    $('.checkBoxIcon' + NodeId).removeClass('fa fa-plus-square');
                    $('.checkBoxIcon' + NodeId).addClass('fa fa-minus-square');
                }
                else {
                    $(".checkout-shipping-address" + NodeId).toggle(false);
                    $('.checkBoxIcon' + NodeId).removeClass('fa fa-minus-square');
                    $('.checkBoxIcon' + NodeId).addClass('fa fa-plus-square');
                }
            }
            else if (Id == "SecondNodeWRT" || Id == "FirstNodeWRT") {
                if (Id == "FirstNodeWRT") {
                    set_collapse_cookies('wrt-left-node');
                    NodeId = 1;
                }
                else if (Id == "SecondNodeWRT") {
                    set_collapse_cookies('wrt-right-node');
                    NodeId = 2;
                }
                var is_hidden = $(".checkout-shipping-address" + NodeId).is(":visible") ? false : true;
                if (is_hidden) {
                    $(".checkout-shipping-address" + NodeId).toggle(true);
                    $('.checkBoxIcon' + NodeId).removeClass('fa fa-plus-square');
                    $('.checkBoxIcon' + NodeId).addClass('fa fa-minus-square');
                }
                else {
                    $(".checkout-shipping-address" + NodeId).toggle(false);
                    $('.checkBoxIcon' + NodeId).removeClass('fa fa-minus-square');
                    $('.checkBoxIcon' + NodeId).addClass('fa fa-plus-square');
                }
            }
            else {
                var is_hidden = $("#checkout-shipping-address-" + Id).is(":visible") ? false : true;
                if (is_hidden) {
                    $("#checkout-shipping-address-" + Id).removeClass('d-none');
                    $('#checkBoxIcon' + Id).removeClass('fa fa-plus-square');
                    $('#checkBoxIcon' + Id).addClass('fa fa-minus-square');
                }
                else {
                    $("#checkout-shipping-address-" + Id).removeClass('d-none');
                    $("#checkout-shipping-address-" + Id).addClass('d-none');
                    $('#checkBoxIcon' + Id).removeClass('fa fa-minus-square');
                    $('#checkBoxIcon' + Id).addClass('fa fa-plus-square');
                }
            }
        }
    });

    $(".checkboxes").change(function () {
        //var Id = this.id.substring(this.id.length - 1);
        var Id = $(this).data("val");

        if (Id == 'Parent') {
            set_collapse_cookies('parent-node');
            ////update_infodoc_params('parent-node', '' + output.ParentNodeGUID + '', '');

            //$("#checkout-shipping-address-" + Id).toggle(this.checked);
            //if ($(this)[0].checked) {
            //    $('#checkBoxIcon' + Id).removeClass('fa fa-plus-square');
            //    $('#checkBoxIcon' + Id).addClass('fa fa-minus-square');
            //}
            //else {
            //    $('#checkBoxIcon' + Id).removeClass('fa fa-minus-square');
            //    $('#checkBoxIcon' + Id).addClass('fa fa-plus-square');
            //}
            var is_hidden = $("#checkout-shipping-address-" + Id).is(":visible") ? false : true;
            if (is_hidden) {
                $("#checkout-shipping-address-" + Id).toggle(true);
                $('#checkBoxIcon' + Id).removeClass('fa fa-plus-square');
                $('#checkBoxIcon' + Id).addClass('fa fa-minus-square');
            }
            else {
                $("#checkout-shipping-address-" + Id).toggle(false);
                $('#checkBoxIcon' + Id).removeClass('fa fa-minus-square');
                $('#checkBoxIcon' + Id).addClass('fa fa-plus-square');
            }
        }
    });

    /*if (output.show_qh_automatically && !output.dont_show_qh && output.qh_info != undefined && output.qh_info != null && output.qh_info.trim() != '') {*/
    if (!output.dont_show_qh && (output.show_qh_automatically && output.qh_info.trim() != "")) {

        setTimeout(function () {
            //if (output.current_step > output.previous_step) {
            set_qh_cookies(1, output.qh_yt_info);
            if ($('#MainContent_hdnPagePostBack').val() !== 'True' && output.lockedInfo.status == false && isreview != 'true') {
                $('#quickHelp').trigger('click');
            }
            else {
                $('#MainContent_hdnPagePostBack').val('');
            }
            //}
        }, 500);
        //var QHAutoShow = $.cookie("QHAutoShow");
        //if (QHAutoShow == undefined || QHAutoShow == null || QHAutoShow == '') {
        //    $.cookie("QHAutoShow", 0);
        //    QHAutoShow = 0;
        //}
        //if (QHAutoShow != undefined && QHAutoShow != null && parseInt(QHAutoShow) == 0) {
        //    set_qh_cookies(1, output.qh_yt_info);
        //    $('#quickHelp').trigger('click');
        //    $.cookie("QHAutoShow", 1);
        //}
    }

    //if ($('#titleDiv').length > 0) {
    //    $('.divHeader').css('padding-top', parseInt($('#titleDiv').height() + 27) + 'px');
    //}

    //$(function () {
    //    if ($('#titleDiv1').length > 0) {
    //        $('.divHeader1').css('padding-top', parseInt($('#titleDiv1').height() + 27) + 'px');
    //    }
    //});

    if ((($.cookie("hdnCookieProjName") != undefined && output.name == decodeURI($.cookie("hdnCookieProjName")))
        || (output.PipeParameters != undefined && output.PipeParameters != null && output.PipeParameters.defaultsort > 0)) /*&& !defaultSorting*/) {

        hdnsortparam = $.cookie("hdnsortparam");
        hdnsortorder = $.cookie("hdnsortorder");
        hdnCookieProjName = $.cookie("hdnCookieProjName");

        if ((hdnsortparam != undefined && hdnsortparam != null && hdnsortparam != '') && (hdnsortorder != undefined && hdnsortorder != null && hdnsortorder != '')) {
            if (hdnsortorder == 'asc') {
                $.cookie("hdnsortorder", 'desc');
            }
            else if (hdnsortorder == 'desc') {
                $.cookie("hdnsortorder", 'asc');
            }

            hdnpage_type = output.page_type;
            var rowCount = 0;
            if (hdnpage_type != undefined && hdnpage_type != null && hdnpage_type.trim() != '') {
                if (hdnpage_type == 'atShowLocalResults') {
                    rowCount = $('#tblLocalResults thead th').length;
                    if (rowCount < 5) {
                        if (parseInt(hdnsortparam) == rowCount - 1) {
                            hdnsortparam = '3';
                            $.cookie("hdnsortparam", '3');
                        }
                    }
                }
                if (hdnpage_type == 'atShowGlobalResults') {
                    rowCount = $('#tblLocalResults thead th').length;
                    if (rowCount < 5) {
                        if (parseInt(hdnsortparam) == rowCount - 1) {
                            hdnsortparam = '3';
                            $.cookie("hdnsortparam", '3');
                        }
                    }
                }
            }
            if (output.PipeParameters.defaultsort > 0) {
                defaultSorting = true;
                $.removeCookie('hdnsortorder');
                $.removeCookie('hdnsortparam');
                $.removeCookie("hdnCookieProjName");
                $.cookie('defaultsort', 1);
                $('#ddlLocalFilter').val(hdnpage_type == 'atShowLocalResults' ? columnsortLocal[0] : columnsortGlobal[0]).prop('selected', true).trigger('change');
                $('thead th:eq(' + output.PipeParameters.defaultsort + ')').trigger('click');
                $('#tblGlobalResults').removeClass('d-none');
            }
            else {
                $.cookie('defaultsort', 0);
                $('#tblGlobalResults thead th:eq(' + hdnsortparam + '), #tblLocalResults thead th:eq(' + hdnsortparam + ')').trigger('click');
            }
            sortingCount = 1;
        }
        else {
            if (output.PipeParameters != undefined && output.PipeParameters != null && output.PipeParameters.defaultsort > 0) {
                defaultSorting = true;
                $.removeCookie('hdnsortorder');
                $.removeCookie('hdnsortparam');
                $.removeCookie("hdnCookieProjName");
                $.cookie('defaultsort', 1);
                $('#ddlLocalFilter').val(hdnpage_type == 'atShowLocalResults' ? columnsortLocal[0] : columnsortGlobal[0]).prop('selected', true).trigger('change');
                $('thead th:eq(' + output.PipeParameters.defaultsort + ')').trigger('click');
                $('#tblGlobalResults').removeClass('d-none');
            }
            else {
                $('#quickHelp').trigger('click');
                $.cookie('defaultsort', 0);
            }
        }
    }
    else {
        //$.removeCookie("hdnsortparam");
        //$.removeCookie("hdnsortorder");
        //$.removeCookie("hdnCookieProjName");
        if (output.PipeParameters != undefined && output.PipeParameters != null && output.PipeParameters.defaultsort > 0) {
            defaultSorting = true;
            //hdnsortorder = $.cookie("hdnsortorder");
            //if (hdnsortorder != undefined || hdnsortorder != null || hdnsortorder != '') {
            //    if (hdnsortorder == 'asc') {
            //        $.cookie("hdnsortorder", 'desc');
            //    }
            //    else if (hdnsortorder == 'desc') {
            //        $.cookie("hdnsortorder", 'asc');
            //    }
            //}
            $.removeCookie('hdnsortorder');
            $.removeCookie('hdnsortparam');
            $.removeCookie("hdnCookieProjName");
            $.cookie('defaultsort', 1);
            $('#ddlLocalFilter').val(hdnpage_type == 'atShowLocalResults' ? columnsortLocal[0] : columnsortGlobal[0]).prop('selected', true).trigger('change');
            $('thead th:eq(' + output.PipeParameters.defaultsort + ')').trigger('click');
            $('#tblGlobalResults').removeClass('d-none');
        }
        else {
            $.cookie('defaultsort', 0);
        }
        if ($.cookie('Normalization') != undefined) {
            hdnNormalization = $.cookie('Normalization');
            //if (hdnNormalization != undefined && hdnNormalization != null && hdnNormalization != '') {
            //    if (hdnpage_type == 'atShowLocalResults') {
            //        $('#ddlNormalization, #mlddlGlobalNormalization').val(hdnNormalization).trigger('change');
            //    }
            //    else if (hdnpage_type == 'atShowGlobalResults') {
            //        $('#ddlGlobalNormalization, #mddlGlobalNormalization').val(hdnNormalization).trigger('change');
            //    }
            //}
        }
    }

    setTimeout(function () {
        ToggleWRTPath(2, true);
    }, 1000);
    /*    HideInfoDocs();*/
    if (output.page_type == 'atAllPairwise') {
        setTimeout(function () {
            SetCssHieghtDynamicMobileView();
        }, 500);
    }
    else {
        setTimeout(function () {
            SetCssHieghtDynamicMobileView();

        }, 50);
    }

    //setTimeout(function () {
    //    $('i.hide_infodoc_tooltip').removeClass('fa fa-minus-square');
    //    $('i.hide_infodoc_tooltip').addClass('fa fa-plus-square');
    //    $('.hide_infodoc_tooltip_div').hide();
    //    if (output.is_infodoc_tooltip) {
    //        $('#chkTooltip').prop('checked', true);
    //        $('.hide_infodoc_tooltip').hide();
    //        $('.hide_infodoc_tooltip_div').hide();
    //        $('.show_infodoc_tooltip').show();
    //    }
    //    else {
    //        $('#chkTooltip').prop('checked', false);
    //        $('.hide_infodoc_tooltip').show();
    //        $('.show_infodoc_tooltip').hide();
    //    }
    //}, 500);

    //$(window).scroll(function () {
    //    alert(123);
    //    if (hdnpage_type == 'atAllPairwise') {
    //        if ($(window).scrollTop() + $(window).height() > $(document).height() - 200) {
    //            multi_last_index = $('div.chart_wrapper').last().data('val');
    //            allPairwiseHtml(multi_last_index + 1, multi_last_index + 100);
    //        }
    //    }
    //});
});

//$("div#divAllPairwise").scroll(function () {
//    alert('divAllPairwise');
//});

//var opt = '';
//var storeOutputVal = function (val) {
//    //console.log(val);
//    opt = val.replace(/\r?\n|\r/g, " ");
//}

var onSortNormalizationChange = function (type) {
    if (type != undefined && type != null && type != '') {
        if (type == 'ddlGlobalNormalization' || type == 'mddlGlobalNormalization') {
            $('#tblGlobalResults').addClass('d-none');
            $.cookie('Normalization', $('#' + type).val());
            normalization_Change($('#' + type).val(), true, 'GlobalResult', type);
        }
        if (type == 'sortNormalization') {
            sort_Results($('#' + type).val(), true);
            $.cookie('Normalization', $('#mddlGlobalNormalization').val());
            normalization_Change($('#mddlGlobalNormalization').val(), true, 'GlobalResult', type);
        }
        if (type == 'ddlNormalization' || type == 'mlddlNormalization') {
            $.cookie('Normalization', $('#' + type).val());
            normalization_Change($('#' + type).val(), false, 'LocalResult', type);
        }
        if (type == 'mlsortNormalization') {
            sort_Results($('#' + type).val(), false);
            $.cookie('Normalization', $('#mlsortNormalization').val());
            normalization_Change($('#mlddlNormalization').val(), false, 'LocalResult', type);
        }
    }
}

var FrameModeHtml = function (index) {
    window.speechSynthesis.cancel();
    if (index != undefined && index != null && parseInt(index) >= 0) {
        var html = '', text = '', WRTtext = '', fClass = '';
        if ((!output.is_infodoc_tooltip || output.framed_info_docs) && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes') && output.showinfodocnode) {
            $('#chkTooltip').prop('checked', false);
            html += '<div class="row">';
            //left node div
            html += '<div class="col-md-3">';
            //first node
            html += '<div class="top_content_wrapper">';
            if (output.first_node_info == undefined || output.first_node_info == null || output.first_node_info.trim() == '')
                fClass = ' frameChkEvaluator';
            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="1" id="checkboxes1">';
            html += '<div class="toggle_heading topheading_ellipse">';
            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon1" aria-hidden="true"></i>';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + '</span>';
            }
            else {
                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + '</span>';
            }
            html += '</div>';
            html += '<div class="toggle_area" id="checkout-shipping-address-1">';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<p>' + output.multi_pw_data[index].InfodocLeft + '</p>';
            }
            else {
                html += '<p>' + output.multi_pw_data[index].InfodocLeft + '</p>';
            }
            text = encodeURIComponent(output.multi_pw_data[index].InfodocLeft);
            WRTtext = encodeURIComponent(output.multi_pw_data[index].LeftNode);
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Left + '",2,2,1,null,"left-node",decodeURIComponent("' + WRTtext + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            else {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Left + '",2,2,1,null,"left-node",decodeURIComponent("' + WRTtext + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
            html += '</a>';
            html += '</div>';
            html += '</div>';

            //first WRT node
            html += '<div class="top_content_wrapper' + fClass + '">';
            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="3" id="checkboxes3">';
            html += '<div class="toggle_heading topheading_ellipse">';
            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon3" aria-hidden="true"></i>';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + ' WRT ' + output.parent_node.replace('%20', ' ') + '</span>';
            }
            else {
                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + ' WRT ' + output.parent_node.replace('%20', ' ') + '</span>';
            }
            html += '</div>';
            html += '<div class="toggle_area" id="checkout-shipping-address-3">';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<p>' + output.multi_pw_data[index].InfodocLeftWRT + '</p>';
            }
            else {
                html += '<p>' + output.multi_pw_data[index].InfodocLeftWRT + '</p>';
            }

            text = encodeURIComponent(output.LeftNode);
            WRTtext = encodeURIComponent(output.InfodocLeftWRT);
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Left + '",3,2,1,null,"wrt-left-node",decodeURIComponent("' + text + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            else {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Left + '",3,2,1,null,"wrt-left-node",decodeURIComponent("' + text + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
            html += '</a>';
            html += '</div>';
            html += '</div>';

            html += '</div>';
            //End left node div

            //Parent div
            fClass = '';
            if (output.parent_node_info == undefined || output.parent_node_info == null || output.parent_node_info.trim() == '')
                fClass = ' frameChkEvaluator';
            html += '<div class="col-md-6' + fClass + '">';
            html += '<div class="top_content_wrapper">';
            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="0" id="checkboxes0">';
            html += '<div class="toggle_heading topheading_ellipse">';
            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon0" aria-hidden="true"></i>';
            html += '<span class="ms-2">' + output.parent_node + '</span>';
            html += '</div>';
            html += '<div class="toggle_area" id="checkout-shipping-address-0">';
            html += '<p>' + output.parent_node_info + '</p>';
            text = encodeURIComponent(output.parent_node_info);
            WRTtext = encodeURIComponent(output.parent_node);
            if (output.page_type == 'atAllPairwise') {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),0,-1,2,-1,0,"parent-node1",decodeURIComponent("' + WRTtext + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            else {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),0,-1,2,-1,0,"parent-node1",decodeURIComponent("' + WRTtext + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            if (output.parent_node_info > 0) {
                html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
            }
            else {
                html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
            }
            html += '</a>';
            html += '</div>';
            html += '</div>';
            html += '</div>';
            //End parent div

            //right node div
            html += '<div class="col-md-3">';
            //second node div
            fClass = '';
            if (output.second_node_info == undefined || output.second_node_info == null || output.second_node_info.trim() == '')
                fClass = ' frameChkEvaluator';
            html += '<div class="top_content_wrapper' + fClass + '">';
            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="2" id="checkboxes2">';
            html += '<div class="toggle_heading topheading_ellipse">';
            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon2" aria-hidden="true"></i>';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + '</span>';
            }
            else {
                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + '</span>';
            }
            html += '</div>';
            html += '<div class="toggle_area" id="checkout-shipping-address-2">';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<p>' + output.multi_pw_data[index].InfodocRight + '</p>';
            }
            else {
                html += '<p>' + output.multi_pw_data[index].InfodocRight + '</p>';
            }

            text = encodeURIComponent(output.multi_pw_data[index].InfodocRight);
            WRTtext = encodeURIComponent(output.multi_pw_data[index].RightNode);
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Right + '",2,2,1,null,"right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            else {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Right + '",2,2,1,null,"right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
            html += '</a>';
            html += '</div>';
            html += '</div>';

            //second WRT node
            html += '<div class="top_content_wrapper' + fClass + '">';
            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="4" id="checkboxes4">';
            html += '<div class="toggle_heading topheading_ellipse">';
            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon4" aria-hidden="true"></i>';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + ' WRT ' + output.parent_node.replace('%20', ' ') + '</span>';
            }
            else {
                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + ' WRT ' + output.parent_node.replace('%20', ' ') + '</span>';
            }
            html += '</div>';
            html += '<div class="toggle_area" id="checkout-shipping-address-4">';
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<p>' + output.multi_pw_data[index].InfodocRightWRT + '</p>';
            }
            else {
                html += '<p>' + output.multi_pw_data[index].InfodocRightWRT + '</p>';
            }

            text = encodeURIComponent(output.multi_pw_data[index].RightNode);
            WRTtext = encodeURIComponent(output.multi_pw_data[index].InfodocRightWRT);
            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Right + '",3,2,1,null,"wrt-right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            else {
                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Right + '",3,2,1,null,"wrt-right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '", decodeURIComponent("' + output.parent_node + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
            }
            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
            html += '</a>';
            html += '</div>';
            html += '</div>';

            html += '</div>';
            //End right node div
            html += '</div>';

            $('#InfoDocsParentDiv_1').html('');
            $('#InfoDocsParentDiv_1').html(html);
            $('#InfoDocsParentDiv').hide();
            $('#InfoDocsParentDiv_1').show();
            $('#chkTooltip').prop('checked', false);
        }
        else {
            $('#chkTooltip').prop('checked', true);
            $('#InfoDocsParentDiv').show();
            $('#InfoDocsParentDiv_1').hide();
        }
    }
}

var allPairwiseHtml = function (from, to) {
    from = from == '' ? 0 : parseInt(from);
    active_multi_index = 0;
    to = parseInt(to) > output.multi_pw_data.length ? output.multi_pw_data.length : parseInt(to);
    if (parseInt(from) < output.multi_pw_data.length && parseInt(to) <= output.multi_pw_data.length) {

        var bars_left =
            new Array(
                ["9", "Extreme", "nine", "EX"],
                ["8", "Very Strong to Extreme", "eight", " "],
                ["7", "Very Strong", "seven", "VS"],
                ["6", "Strong to Very Strong", "six", " "],
                ["5", "Strong", "five", "S"],
                ["4", "Moderate to Strong", "four", " "],
                ["3", "Moderate", "three", "M"],
                ["2", "Equal to Moderate", "two", " "]
            );
        var bars_right = new Array(
            ["2", "Equal to Moderate", "two", " "],
            ["3", "Moderate", "three", "M"],
            ["4", "Moderate to Strong", "four", " "],
            ["5", "Strong", "five", "S"],
            ["6", "Strong to Very Strong", "six", " "],
            ["7", "Very Strong", "seven", "VS"],
            ["8", "Very Strong to Extreme", "eight", " "],
            ["9", "Extreme", "nine", "EX"]
        );
        for (var i = from; i < to; i++) {
            var html = '';
            //var pointer = output.isPipeViewOnly ? ' class="d-none"' : '';
            var pointer = '';
            if (output.isPipeViewOnly) {
                pointer = ' class="d-none"';
            }
            html += "<div class='chart_wrapper graphical_chart unselected_row' data-val='" + i + "' onclick='activeRow(" + i + ", true); ' id='index_" + i + "'>";
            var left = encodeURIComponent(output.multi_pw_data[i].InfodocLeft);
            var leftWRT = encodeURIComponent(output.multi_pw_data[i].InfodocLeftWRT);
            var leftPNode = encodeURIComponent(output.parent_node);
            if (output.multi_pw_data[i].InfodocLeft != '') {
                html += "<div class='value_wrapper left_info'><div class='info_data selected'>";
            }
            else {
                html += "<div class='value_wrapper left_info no_data'><div class='info_data selected'>";
            }

            var onClickLeft = "showInfoPopup(decodeURIComponent('" + left + "')," + output.multi_pw_data[i].NodeID_Left + ",2,2,1,null,'left-text-node',decodeURIComponent('" + left + "')," + i + ",decodeURIComponent('" + leftPNode + "'))";

            html += "<div Class='info_header'>";
            html += "<span Class='title_tag text-uppercase'>" + output.multi_pw_data[i].LeftNode + "</span>";
            //var paramleftnode = output.multi_pw_data[i].LeftNode;
            html += "<a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' Class='icon' onclick=" + onClickLeft + " aria-hidden='true'></a>";
            //html += "<a href='javascript:void(0);'><i class='fas fa-edit' onclick=showInfoPopup(decodeURIComponent('" + left + "'),'" + output.multi_pw_data[i].NodeID_Left + "',2,2,1,null,'left-text-node','" + output.multi_pw_data[i].LeftNode + "','" + i + "', decodeURIComponent('" + output.parent_node + "')) aria-hidden='true'></i></a>";
            html += "<a href='javascript:void(0);'  data-bs-toggle='modal' data-bs-target='#ex ampleModal' Class='d-md-none'><img src='../../img/icon/info-close.svg' Class='icon'></a>";
            html += "</div>";
            html += "<div Class='body_content'>";
            html += "<span>" + output.multi_pw_data[i].InfodocLeft + "</span>";
            html += "<div Class='no-data_content'>";
            html += "<h2> No Data</h2>";
            html += "<a href='javascript:void(0);'>+<h3> Add Data</h3></a>";
            html += "</div>";
            html += "</div>";
            html += "<div Class='info_datafooter'>";
            html += "<button type='button' data-bs-toggle='modal' data-bs-target='#exampleModalWRT_" + i + "'>With respect To <img src='../../img/icon/button_arrow.svg'></button></div>";

            var right = encodeURIComponent(output.multi_pw_data[i].InfodocRight);
            var rightWRT = encodeURIComponent(output.multi_pw_data[i].InfodocRightWRT);

            var editNodeClickEvent = "onclick=showInfoPopup(decodeURIComponent('" + left + "')," + output.multi_pw_data[i].NodeID_Left + ",2,2,1,null,'left-text-node'," + output.multi_pw_data[i].LeftNode + "," + i + ", decodeURIComponent('" + output.parent_node + "'))";
            html += "<div class='modal fade' id='exampleModalWRT_" + i + "' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>" + output.multi_pw_data[i].LeftNode + "</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' " + editNodeClickEvent + "'></a></div><div class='modal_info_content'><p>" + output.multi_pw_data[i].InfodocLeft + "</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>" + output.multi_pw_data[i].LeftNode + " WRT " + output.parent_node + "</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg'onclick=showInfoPopup(decodeURIComponent('" + output.multi_pw_data[i].InfodocLeftWRT + "'),'" + i + "','3','0','1',null,'wrt-left-node',decodeURIComponent(" + rightWRT + "''),decodeURIComponent('" + right + "')) ></a></div><div class='modal_info_content'><p>" + output.multi_pw_data[i].InfodocLeftWRT + "</p></div></div></div></div></div></div></div>";
            if (left != "" && left != "NAN") {
                html += "<div class='info_tooltip hideshowinfo parentnode102" + i + "' style='display:none;' id='il_" + i + "'>";
            }
            else {
                html += "<div class='info_tooltip hideshowinfo parentnode102" + i + " divchkEvaluator' style='display:none;' id='il_" + i + "'>";
            }
            html += "<div class='tooltop_head'><span>" + output.multi_pw_data[i].LeftNode + "</span><div class='action_icons'>";
            html += "<a" + pointer + " href='javascript:void(0);'><i class='fas fa-edit chkEvaluator d-none' onclick=showInfoPopup(decodeURIComponent('" + left + "'),'" + output.multi_pw_data[i].NodeID_Left + "',2,2,1,null,'left-text-node','" + output.multi_pw_data[i].LeftNode + "','" + i + "', decodeURIComponent('" + output.parent_node + "')) aria-hidden='true'></i></a>";
            html += "<a href='javascript:void(0);'><i class='fa fa-times' onclick='hideWrapper(il_" + i + ")' aria-hidden='true'></i></a></div></div><p>" + output.multi_pw_data[i].InfodocLeft + "</p></div></div>";
            html += "</div>";

            if (output.pairwise_type == "ptVerbal") {
                html += "<div class='rating_area'>";
                html += "<div class='comment_div' readonly id='comdiv" + i + "'>";
                html += "<div style='cursor:pointer;' onclick='toggleBox(" + i + ")'>";
                if (output.multi_pw_data[i].Comment != undefined && output.multi_pw_data[i].Comment != null && output.multi_pw_data[i].Comment.trim() == '') {
                    html += "<img src='../../img/icon/commentadd.svg'> <span style='color: #333333'>Add Comment</span></div>";
                }
                else {
                    html += "<img src='../../img/icon/commentadd.svg'> <span style='color: #333333'>Add Comment</span></div>";
                }
                if (i == 0) {
                    html += "<div class='info_tooltip right_tooltip hideshowinfo' id='toggleComment_" + i + "' style='display:none;' ><div class='d-flex justify-content-between'><span>Add your comment</span>";
                }
                else {
                    html += "<div class='info_tooltip right_tooltip hideshowinfo' id='toggleComment_" + i + "' style='display:none;' ><div class='d-flex justify-content-between'><span>Add your comment</span>";
                }
                html += "<div class='action_icons'><a href='javascript:void(0);' onclick='hideNode(" + i + ")'><i class='fa fa-times' aria-hidden='true'></i></a></div></div>";
                //pointer = output.isPipeViewOnly ? ' style="pointer-events: none;"' : '';
                if (output.isPipeViewOnly) {
                    pointer = ' style="pointer-events: none;"';
                }
                html += "<textarea" + pointer + "  id='comment_" + i + "' class='form-control mb-3 mt-2 w-100' rows='2'>" + output.multi_pw_data[i].Comment + "</textarea><div class='comt_btn text-end'>";
                html += "<button type='button' class='py-1 px-2' onclick=save_multi_pairwise('comment'," + i + ",0,1," + i + "," + output.current_step + ")><i class='fa fa-check' aria-hidden='true'></i>OK</button>";
                html += "</div></div></div>";
                html += "<div class='chart_area'" + pointer + ">";
                var value = 0;
                try {
                    value = Math.abs(Math.floor(output.multi_pw_data[i].Value));
                } catch (e) {
                    value = 0;
                }
                if (active_multi_index != i || output.collapse_bars) {
                    html += "<div class='rating_result'>";
                    //if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value >= 2 && value <= 8) {
                    //    html += "<div style='display:block;' class='left_result left_result_" + i + "'><span id='left_result_" + i + "'>" + bars_right[value - 2][1] + "</span></div>";
                    //}
                    //else {
                    //    html += "<div style='display:none;' class='left_result left_result_" + i + "'><span id='left_result_" + i + "'></span></div>";
                    //}
                    var innerIndex = parseFloat((bars_left.length) - value);
                    //if (parseInt(output.multi_pw_data[i].Advantage) == -1) {
                    //    innerIndex = isNaN(innerIndex) ? -1 : innerIndex;
                    //    if (innerIndex < 0) {
                    //        innerIndex = -1 * innerIndex;
                    //    }
                    //    try {
                    //        html += "<div style='display:block;'  class='right_result right_result_" + i + "'><span id='right_result_" + i + "'>" + bars_left[parseInt(innerIndex + 1)][1] + "</span></div>";
                    //    } catch (e) {
                    //        if (bars_left.length >= innerIndex) {
                    //            html += "<div style='display:block;'  class='right_result right_result_" + i + "'><span id='right_result_" + i + "'>" + bars_left[parseInt(innerIndex)][1] + "</span></div>";
                    //        }
                    //        else {
                    //            html += "<div style='display:block;'  class='right_result right_result_" + i + "'><span id='right_result_" + i + "'>" + bars_left[bars_left.length - 1][1] + "</span></div>";
                    //        }
                    //    }
                    //}
                    //else {
                    //    html += "<div style='display:none;'  class='right_result right_result_" + i + "'><span id='right_result_" + i + "'></span></div>";
                    //}
                    //if (value == 1 && parseInt(output.multi_pw_data[i].Advantage) == 0) {
                    //    html += "<div class='equal equal_" + i + "'><span>Equal</span></div>";
                    //}
                    //else {
                    //    html += "<div style='display:none;' class='equal equal_" + i + "'><span>Equal</span></div>";
                    //}
                    html += "</div>";
                }
                html += "<div class='ratting_box' id='rating_box_" + i + "'>";
                var brlength = bars_left.length;
                var currentRating = '', currentIndex = 0;
                for (var leftindex = 0; leftindex <= bars_left.length - 1; leftindex++) {
                    if (bars_left[leftindex][3] != ' ') {
                        currentRating = bars_left[leftindex][3];
                        currentIndex = parseInt(bars_left[leftindex][0]);

                        if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value >= currentIndex) {
                            if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value == currentIndex) {
                                html += "<div id='show_" + i + "_" + (bars_left.length - leftindex) + "' title='" + bars_left[leftindex][1] + "' onclick=save_multi_pairwise('left'," + leftindex + "," + parseInt(bars_left.length - leftindex) + ",1," + i + "," + output.current_step + ") class='rating_" + bars_left[leftindex][3] + " rating_content rating_content_left selected_left selcted_spot'>";
                            }
                            else {
                                html += "<div id='show_" + i + "_" + (bars_left.length - leftindex) + "' title='" + bars_left[leftindex][1] + "' onclick=save_multi_pairwise('left'," + leftindex + "," + parseInt(bars_left.length - leftindex) + ",1," + i + "," + output.current_step + ") class='rating_" + bars_left[leftindex][3] + " rating_content rating_content_left selected_left'>";
                            }
                        } else {
                            html += "<div id='show_" + i + "_" + (bars_left.length - leftindex) + "' title='" + bars_left[leftindex][1] + "' onclick=save_multi_pairwise('left'," + leftindex + "," + parseInt(bars_left.length - leftindex) + ",1," + i + "," + output.current_step + ") class='rating_" + bars_left[leftindex][3] + " rating_content rating_content_left'>";
                        }
                        if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value == currentIndex) {
                            html += "<div class='multiverbal_spotname left_verbal left_name_" + i + " clsname_" + i + " ' id='spotL" + i + "_" + (bars_left.length - leftindex) + "' >" + bars_right[value - 2][1] + "</div>";
                        }
                        else {
                            html += "<div class='multiverbal_spotname left_verbal left_name_" + i + " hide clsname_" + i + " ' id='spotL" + i + "_" + (bars_left.length - leftindex) + "' >" + bars_right[bars_left.length - (leftindex + 1)][1] + "</div>";
                        }
                        html += "<span>" + bars_left[leftindex][3] + "</span></div>";
                    }
                    else {
                        currentIndex = parseInt(bars_left[leftindex][0]);
                        html += "<div id='show_" + i + "_" + (bars_left.length - leftindex) + "' title='" + bars_left[leftindex][1] + "' onclick=save_multi_pairwise('left'," + leftindex + "," + parseInt(bars_left.length - leftindex) + ",1," + i + "," + output.current_step + ") ";
                        if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value >= currentIndex) {
                            if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value == currentIndex) {
                                html += " Class='rating_" + currentRating + "_s rating_content rating_content_left selected_left selcted_spot" + "'>";
                            }
                            else {
                                html += " Class='rating_" + currentRating + "_s rating_content rating_content_left selected_left" + "'>";
                            }
                        }
                        else {
                            html += " Class='rating_" + currentRating + "_s rating_content rating_content_left" + "'>";
                        }
                        if (parseInt(output.multi_pw_data[i].Advantage) == 1 && value == currentIndex) {
                            html += "<div class='multiverbal_spotname left_verbal left_name_" + i + " clsname_" + i + " ' id='spotL" + i + "_" + (bars_left.length - leftindex) + "' >" + bars_right[value - 2][1] + "</div>";
                        }
                        else {
                            html += "<div class='multiverbal_spotname left_verbal left_name_" + i + " hide clsname_" + i + " ' id='spotL" + i + "_" + (bars_left.length - leftindex) + "' >" + bars_right[bars_left.length - (leftindex + 1)][1] + "</div>";
                        }
                        html += "</div>";
                    }
                }
                if (value >= 1 && parseInt(output.multi_pw_data[i].Advantage) == 1) {
                    html += "<div id='divRatingEQ_e_" + i + "' onclick=save_multi_pairwise('equal'," + i + ",1,0," + i + "," + output.current_step + ") class='rating_e rating_content rating_content_left selected_left'><span>EQ</span></div>";
                }
                else if (value >= 1 && parseInt(output.multi_pw_data[i].Advantage) == -1) {
                    html += "<div id='divRatingEQ_e_" + i + "' onclick=save_multi_pairwise('equal'," + i + ",1,0," + i + "," + output.current_step + ") class='rating_e rating_content rating_content_right selected_right'><span>EQ</span></div>";
                }
                else if (value == 1 && parseInt(output.multi_pw_data[i].Advantage) == 0) {
                    html += "<div id='divRatingEQ_e_" + i + "' onclick=save_multi_pairwise('equal'," + i + ",1,0," + i + "," + output.current_step + ") class='rating_e eqalizer_color rating_content rating_content_right selcted_spot'>";
                    html += "<div Class='multiverbal_spotname equal_" + i + " clsname_" + i + "'>Equal</div><span>EQ</span></div>";
                }
                else {
                    html += "<div id='divRatingEQ_e_" + i + "' onclick=save_multi_pairwise('equal'," + i + ",1,0," + i + "," + output.current_step + ") class='rating_e rating_content rating_content_right rating_content_left'><span>EQ</span></div>";
                }
                for (var rightindex = 0; rightindex <= bars_right.length - 1; rightindex++) {
                    if (bars_right[rightindex][3] != ' ') {
                        currentIndex = parseInt(bars_right[rightindex][0]);
                        html += "<div id='showr_" + i + "_" + rightindex + "' title='" + bars_right[rightindex][1] + "' onclick=save_multi_pairwise('right'," + rightindex + "," + rightindex + ",-1," + i + "," + output.current_step + ") ";
                        if (parseInt(output.multi_pw_data[i].Advantage) == -1 && value >= currentIndex) {
                            if (parseInt(output.multi_pw_data[i].Advantage) == -1 && value == currentIndex) {
                                html += "Class='rating_" + bars_right[rightindex][3] + " rating_content rating_content_right selected_right selcted_spot'><span>" + bars_right[rightindex][3] + "</span>";
                            }
                            else {
                                html += "Class='rating_" + bars_right[rightindex][3] + " rating_content rating_content_right selected_right'><span>" + bars_right[rightindex][3] + "</span>";
                            }
                        }
                        else {
                            html += "Class='rating_" + bars_right[rightindex][3] + " rating_content rating_content_right'><span>" + bars_right[rightindex][3] + "</span>";
                        }
                        if (parseInt(output.multi_pw_data[i].Advantage) == -1 && value >= currentIndex) {
                            html += "<div class='multiverbal_spotname right_name_" + i + " clsname_" + i + "' id='spotR" + i + "_" + (rightindex) + "'>" + bars_right[value - 2][1] + "</div>";
                        }
                        else {
                            html += "<div class='multiverbal_spotname right_name_" + i + " hide clsname_" + i + "' id='spotR" + i + "_" + (rightindex) + "'>" + bars_left[bars_right.length - rightindex - 1][1] + "</div>";
                        }
                        html += "</div>";
                    }
                    else {
                        currentRating = bars_right[parseInt(rightindex + 1)][3];
                        currentIndex = parseInt(bars_right[rightindex][0]);
                        if (parseInt(output.multi_pw_data[i].Advantage) == -1 && value >= currentIndex) {
                            if (parseInt(output.multi_pw_data[i].Advantage) == -1 && value == currentIndex) {
                                html += "<div id='showr_" + i + "_" + rightindex + "' title='" + bars_right[rightindex][1] + "' onclick=save_multi_pairwise('right'," + rightindex + "," + rightindex + ",-1," + i + "," + output.current_step + ") class='rating_" + currentRating + "_s rating_content rating_content_right selected_right selcted_spot'>";
                            }
                            else {
                                html += "<div id='showr_" + i + "_" + rightindex + "' title='" + bars_right[rightindex][1] + "' onclick=save_multi_pairwise('right'," + rightindex + "," + rightindex + ",-1," + i + "," + output.current_step + ") class='rating_" + currentRating + "_s rating_content rating_content_right selected_right'>";
                            }
                        } else {
                            html += "<div id='showr_" + i + "_" + rightindex + "' title='" + bars_right[rightindex][1] + "' onclick=save_multi_pairwise('right'," + rightindex + "," + rightindex + ",-1," + i + "," + output.current_step + ") class='rating_" + currentRating + "_s rating_content rating_content_right'>";
                        }
                        if (parseInt(output.multi_pw_data[i].Advantage) == -1 && value >= currentIndex) {
                            html += "<div class='multiverbal_spotname right_name_" + i + " clsname_" + i + "' id='spotR" + i + "_" + (rightindex) + "'>" + bars_right[value - 2][1] + "</div>";
                        }
                        else {
                            html += "<div class='multiverbal_spotname right_name_" + i + " hide clsname_" + i + "' id='spotR" + i + "_" + (rightindex) + "'>" + bars_left[bars_right.length - rightindex - 1][1] + "</div>";
                        }
                        html += "</div>";
                    }
                }
                html += "</div></div>";
                /*html += "<div" + pointer + " class='close_icon' style='cursor:pointer;' onclick=save_multi_pairwise('close'," + i + "," + i + ",-1," + i + "," + output.current_step + ") ><i class='fa fa-times' aria-hidden='true'></i></div>";*/
                html += "<div" + pointer + " class='close_icon' style='cursor:pointer;' onclick=save_multi_pairwise('close'," + i + "," + i + ",-1," + i + "," + output.current_step + ")><span><img src='../../img/icon/erasar.svg' class='imgClose'></span><span>Clear Judgment</span></div>";
                html += "</div>";
            }

            if (output.pairwise_type == "ptGraphical") {
                html += "<div class='rating_area'>";
                html += "<div class='form-group w-100 mt-3'" + pointer + ">";
                html += "<div id='gslider" + i + "'></div>";
                html += "</div>";
                html += "<div class='chart_data graphical_data mt-3'>";
                html += "<div Class='comment_div px-2'>";
                html += "<div Class='comment_icon'>";
                if (output.multi_pw_data[i].Comment == undefined || output.multi_pw_data[i].Comment == null || output.multi_pw_data[i].Comment == '') {
                    html += "<a href='javascript:void(0)' onclick='showNode(3" + i + "," + i + ")'><img src = '../../img/icon/commentadd.svg' id='multi_comment_icon_" + i + "'  aria-hidden='true'><span style='color: #333333'> Add Comment</span></a>";
                }
                else {
                    html += "<a href='javascript:void(0)' onclick='showNode(3" + i + "," + i + ")'><img src = '../../img/icon/commentadd.svg' id='multi_comment_icon_" + i + "' title='" + output.multi_pw_data[i].Comment + "'  aria-hidden='true'><span style='color: #333333'> Add Comment</span></a>";
                }
                if (i == 0) {
                    html += "<div class='info_tooltip right_tooltip hideshowinfo'  id='parentnode3" + i + "' style='display:none;'>";
                }
                else {
                    html += "<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3" + i + "' style='display:none;'>";
                }
                html += "<div class='d-flex justify-content-between'>";
                html += "<span>Add your comment</span>";
                html += "<div class='action_icons' ><a href='javascript:void(0);' onclick='toggleBox()'><i class='fa fa-times' onclick=' onclick='showInfoPopup(decodeURIComponent('" + right + "')," + output.multi_pw_data[i].NodeID_Right + ",2,2,1,null,'right-node'," + output.multi_pw_data[i].RightNode + "," + i + ", decodeURIComponent('" + output.parent_node + "'))' aria-hidden='true'></i></a></div>";
                html += "</div>";
                html += "<textarea" + pointer + " class='form-control mb-3 mt-2 w-100' rows='2' id='multi_comment_" + i + "' placeholder='Add your comment'>" + output.multi_pw_data[i].Comment + "</textarea>";
                html += "<div class='comt_btn text-end'>";
                html += "<button type='button' class='py-1 px-2' onclick='hideNode(3" + i + "," + i + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>";
                html += "</div>";
                html += "</div>";

                html += "</div>";
                html += "<div Class='info_tooltip right_tooltip' style='transform: scaleY(0);'>";
                html += "<div Class='d-flex justify-content-between'>";
                html += "<span> Add your comment</span>";
                html += "<div Class='action_icons'>";
                html += "<a href='javascript:void(0);'>";
                html += "<i class='fa fa-times font-size-16 text-danger' aria-hidden='true'></i>";
                html += "</a>";
                html += "</div>";
                html += "</div>";
                html += "<textarea Class='form-control mb-3 mt-2 w-100' rows='2'></textarea>";
                html += "<div Class='comt_btn text-end'>";
                html += "<button> <i Class='fa fa-check' aria-hidden='true'></i> OK</button>";
                html += "</div>";
                html += "</div>";
                html += "</div>";
                html += "<div class='curvechart_input'>";
                var fun = "onkeyup=graphicalall_key_up(this,'0','-1','up','" + i + "','false')";
                if (output.multi_pw_data[i].Advantage > 0) {
                    html += "<input type='text'  value='" + parseFloat(output.multi_pw_data[i].Value).toFixed(2) + "' id='input1" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                else {
                    html += "<input type='text' value='1' id='input1" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                html += "<div class='arrowicons'>";
                html += "<a href='javascript:void(0);' onclick=swap_value('" + i + "'); id='reverse_icon_" + i + "'>";
                html += "<img src='../../img/icon/value_exchange.svg'>";
                html += "</a>";
                html += "</div>";
                if (output.multi_pw_data[i].Advantage < 0) {
                    html += "<input type='text'  value='" + parseFloat(output.multi_pw_data[i].Value).toFixed(2) + "' id='input2" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                else {
                    html += "<input type='text' value='1' id='input2" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                html += "</div>";
                fun = "onclick='add_multivalues(-2147483648000, 0, " + i + ", event); set_slider_blank(" + i + "); clearInputOnMultiPWGraphical(" + i + ")'";
                html += "<div" + pointer + " class='close_icon ms-3 me-3' " + fun + ">";
                html += "<img src = '../../img/icon/erasar.svg' style='cursor: pointer;' class=''imgClose >";
                html += "<span>Clear Judgment</span>";
                html += "</div>";

                html += "</div>";
                html += "<div class='result_values d-none'>";

                //Comment box
                if (output.show_comments) {
                    html += "<div style='cursor:pointer;'  class='comment_div'>";
                    if (output.multi_pw_data[i].Comment == undefined || output.multi_pw_data[i].Comment == null || output.multi_pw_data[i].Comment == '') {
                        html += "<i class='far fa-comments' id='multi_comment_icon_" + i + "' onclick='showNode(3" + i + "," + i + ")'  aria-hidden='true'></i>";
                    }
                    else {
                        html += "<i Class='fa fa-comments' id='multi_comment_icon_" + i + "' onclick='showNode(3" + i + "," + i + ")'  title='" + output.multi_pw_data[i].Comment + "' aria-hidden='true'></i>";
                    }
                    if (i == 0) {
                        html += "<div class='info_tooltip right_tooltip hideshowinfo'  id='parentnode3" + i + "' style='display:none;'>";
                    }
                    else {
                        html += "<div class='info_tooltip right_tooltip tooltip_wrapper hideshowinfo'  id='parentnode3" + i + "' style='display:none;'>";
                    }
                    html += "<div class='d-flex justify-content-between'>";
                    html += "<span>Add your comment</span>";
                    html += "<div class='action_icons'><a href='javascript:void(0);' onclick='toggleBox()'><i class='fa fa-times' onclick='hideNode(3" + i + "," + i + ")' aria-hidden='true'></i></a></div>";
                    html += "</div>";
                    html += "<textarea" + pointer + " class='form-control mb-3 mt-2 w-100' rows='2' id='multi_comment_" + i + "' placeholder='Add your comment'>" + output.multi_pw_data[i].Comment + "</textarea>";
                    html += "<div class='comt_btn text-end'>";
                    html += "<button type='button' class='py-1 px-2' onclick='hideNode(3" + i + "," + i + ")'> <i class='fa fa-check' aria-hidden='true'></i> OK</button>";
                    html += "</div>";
                    html += "</div>";
                    html += "</div>";
                }
                //comment box end
                html += "<div class='reverse_icon'" + pointer + ">";
                html += "<a href='javascript:void(0);' onclick=swap_value('" + i + "'); id='reverse_icon_" + i + "'><img src='../../img/icon/value_exchange.svg'/></a>";
                html += "</div>";
                html += "<div class='inputs_value'" + pointer + ">";
                if (output.multi_pw_data[i].Advantage < 0) {
                    html += "<input type='text' value='" + parseFloat(output.multi_pw_data[i].Value).toFixed(2) + "' id='input2" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                else {
                    html += "<input type='text' value='1' id='input2" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                if (output.multi_pw_data[i].Advantage > 0) {
                    html += "<input type='text'  value='" + parseFloat(output.multi_pw_data[i].Value).toFixed(2) + "' id='input1" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                else {
                    html += "<input type='text' value='1' id='input1" + i + "' class='value_control checkEnter' " + fun + ">";
                }
                html += "</div>";
                html += "<div class='d-flex justify-content-center slide_valueComment'>";

                fun = "onclick='add_multivalues(-2147483648000, 0, " + i + ", event); set_slider_blank(" + i + ")'";
                html += "<div" + pointer + " class='close_icon ms-3 me-3' " + fun + ">";
                html += "<i class='fa fa-times' aria-hidden='true' id='close_icon_" + i + "' onclick='hideWrapper(il_" + i + ")'></i>";
                html += "</div>";

                //comment box start

                html += "</div>";
                html += "</div>";
                html += "</div>";
                //html += "</div>";
            }

            var details = navigator.userAgent;
            var regexp = /android|iphone|kindle|ipad/i;
            var isMobile = regexp.test(details);
            if (!isMobile) {
                var right = encodeURIComponent(output.multi_pw_data[i].InfodocRight);
                var rightWRT = encodeURIComponent(output.multi_pw_data[i].InfodocRightWRT);
                var pnode = encodeURIComponent(output.parent_node);
                if (output.multi_pw_data[i].InfodocRight != undefined && output.multi_pw_data[i].InfodocRight != null && output.multi_pw_data[i].InfodocRight != '') {
                    html += "<div class='value_wrapper right_info'><div class='info_data selected'>";
                }
                else {
                    html += "<div class='value_wrapper right_info no_data'><div class='info_data selected'>";
                }
                var onClickReght = "showInfoPopup(decodeURIComponent('" + right + "')," + output.multi_pw_data[i].NodeID_Right + ",2,2,1,null,'right-node',decodeURIComponent('" + right + "')," + i + ",decodeURIComponent('" + pnode + "'))";


                html += "<div Class='info_header'>";
                html += "<span Class='title_tag text-uppercase'>" + output.multi_pw_data[i].RightNode + "</span>";
                html += "<a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' class='icon' onclick=" + onClickReght + " aria-hidden='true'></a>";
                html += "<a href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#ex ampleModal' Class='d-md-none'><img src='../../img/icon/info-close.svg' Class='icon'></a>";
                html += "</div>";
                html += "<div Class='body_content'>";
                html += "<span>" + output.multi_pw_data[i].InfodocRight + "</span>";
                html += "<div Class='no-data_content'>";
                html += "<h2> No Data</h2>";
                html += "<h3>+ <u>Add Data</u></h3>";
                html += "</div>";
                html += "</div>";
                html += "<div Class='info_datafooter'>";
                html += "<button type='button' data-bs-toggle='modal' data-bs-target='#exampleModalWRTRight_" + i + "'>with respect to <img src='../../img/icon/button_arrow.svg'></button>";
                html += "</div>";

                html += "<div class='modal fade' id='exampleModalWRTRight_" + i + "' tabindex='-1' aria-labelledby='exampleModalLabel' aria-hidden='True'><div class='modal-dialog modal-fullscreen'><div class='modal-content'><div class='modal-header'><h3 class='modal-title'id='exampleModalLabel' style='color:white;'>INFODOCS</h3><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div><div class='modal-body'><div class='row h-100'><div class='col-md-6 border-End'><div class='modal_info_header'><h3>" + output.multi_pw_data[i].RightNode + "</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' onclick=showInfoPopup(decodeURIComponent('" + right + "')," + output.multi_pw_data[i].NodeID_Right + ",2,2,1,null,'right-node'," + output.multi_pw_data[i].RightNode + "," + i + ", decodeURIComponent('" + output.parent_node + "'))></a></div>";
                html += "<div class='modal_info_content'><p>" + output.multi_pw_data[i].InfodocRight + "</p></div></div><div class='col-md-6'><div class='modal_info_header'><h3>" + output.multi_pw_data[i].RightNode + " WRT " + output.parent_node + "</h3><a href='javascript:void(0);' class='chkEvaluatorone'><img src='../../img/icon/edit-icon.svg' onclick='showInfoPopup(decodeURIComponent('" + output.multi_pw_data[i].RightNode + "')," + output.multi_pw_data[i].NodeID_Right + ",3,2,1,null,'wrt-right-node',decodeURIComponent('" + rightWRT + "')," + i + ", decodeURIComponent('" + output.parent_node + "'))'></a></div><div class='modal_info_content'><p>" + output.multi_pw_data[i].InfodocRightWRT + "</p></div></div></div></div></div></div></div>";
                html += "</div>";
                //html += "</div>";
            }
            html += "</div>";
            html += "</div>";
            active_multi_index = i;
            $('#divAllPairwise').append(html);

            if (hdnpairwise_type == 'ptGraphical') {
                var flast = false;
                if ((multi_pw_data.length - 1) == i) {
                    flast = true;
                }
                $('#input1' + i).val(left_input[i] == undefined ? '' : left_input[i]);
                $('#input2' + i).val(right_input[i] == undefined ? '' : right_input[i]);

                initializeNoUIAllGraphical(participant_slider[i], participant_slider[i] + 800, i, flast);
            }
        }
        $('div.chart_wrapper').removeClass('active_row');
        if (hdnpairwise_type == 'ptGraphical') {
            slideScroll(true, false);
        }
        else if (hdnpairwise_type == 'ptVerbal') {
            slideMultiPWVerbal(true, false);
        }
    }
}

function ToggleWRTPath(timeOutSeconds, shouldFlash) {
    if (timeOutSeconds != undefined && timeOutSeconds != null && timeOutSeconds != '' && parseInt(timeOutSeconds) > 0) {
        timeOutSeconds = timeOutSeconds * 1000;
    }
    else {
        timeOutSeconds = 0;
    }

    if (shouldFlash) {
        $(".wrt_path").addClass("flash");
    } else {
        $(".wrt_path").removeClass("flash");
    }

    setTimeout(function () {
        if (timeOutSeconds > 0) {
            $(".wrt_path").fadeOut(2000);
        } else {
            $(".wrt_path").toggle();
        }

    }, timeOutSeconds);
}

var set_qh_cookies = function (value, qh_text) {
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setQHCookies",
        data: JSON.stringify({
            ProjectID: 0,
            stepNo: output.current_step,
            status: value,
            qh_text: qh_text
        }),
        dataType: "json",
        async: false,
        contentType: "application/json; charset=utf-8",
        success: function (response) {
        },
        error: function (response) {
        }
    });
}

var is_html_empty = function (html) {
    if (typeof html == 'string') {
        if (/<[a-z\][\s\S]*>/i.test(html)) {
            if (html.indexOf("img") >= 0) {
                return false;
            }
            if ($(html).text().trim() == "") {
                return true;
            } else {
                return false;
            }
        } else {
            if (html.trim() != "") {
                return false;
            } else {
                return true;
            }
        }
    } else {
        return true;
    }
}

//Local Results
var sort_Results = function (request, global) {
    //$.removeCookie('hdnsortorder');
    //$.removeCookie('hdnsortparam');
    //$.removeCookie("hdnCookieProjName");
    if (global == undefined || global == null || global == '') {
        global = hdnpage_type == 'atShowLocalResults' ? false : true;
    }

    global = global.toString() == 'true' ? true : false;
    var reverse;
    if (global)
        reverse = reverseGlobal;
    else
        reverse = reverseLocal;

    if (currentColumn == request)
        reverse = !reverse;
    else {
        if (request == "nodeName" || request == "nodeID") {
            reverse = false;
        } else {
            reverse = true;
        }
    }
    switch (request) {
        case "nodeID":
            columnname = "nodeID";
            columntosort = 0;
            break;
        case "nodeName":
            columnname = "nodeName";
            columntosort = 1;
            break;
        case "yourResults":
            columnname = "yourResults";
            columntosort = 2;
            break;
        case "combine":
            columnname = "combine";
            columntosort = 3;
            break;
    }

    currentColumn = request;
    //resultsreverse = reverse;
    //columnsort = currentColumn;
    if (global) {
        reverseGlobal = reverse;
        columnsortGlobal = columns[columntosort];
    }
    else {
        reverseLocal = reverse;
        columnsortLocal = columns[columntosort];
    }

    //var resulttype = 0;
    //if (output.page_type == "atShowGlobalResults")
    //    resulttype = 2;
    //if (output.page_type == "atShowLocalResults")
    //    resulttype = 1;
}

var filterLocalGlobalinMobile = function (type) {
    if (type != undefined && type != null && type != '') {
        if (type == 'Local') {
            sort_Results($('#ddlLocalFilter').val(), false);
            normalization_Change($('#ddlNormalization').val(), false, 'LocalResult');
        }
        else if (type == 'Global') {
            sort_Results($('#ddlGlobalFilter').val(), true);
        }
    }
}

var pageTypeWiseFun = function () {
    $("#overlay").css('display', 'flex');
    if ($('#MainContent_hdnOutput').val() != '') {
        output = eval("(" + $('#MainContent_hdnOutput').val() + ")");
        if ((output.lockedInfo == undefined && output.lockedInfo == null || !output.lockedInfo.status) && output.validTimeline && output.access_avaiable) {
            if (!output.isPM) {
                $('.evaluator_toggle,#editQuickHelp,.chkEvaluator,img.chkEvaluatorone,.chkEvaluatorone,.vwEval').removeClass('d-none').addClass('d-none');
                if ($("#img1").is(":visible")) {
                    $("#img1").hide();
                    $("#Header_InfoDocs").hide();
                }
            }
            hdnpage_type = output.page_type;
            is_judgment_screen = hdnpage_type == 'atInformationPage' || hdnpage_type == 'atShowLocalResults' || hdnpage_type == 'atShowGlobalResults' ? false : true;
            addLastStepButton();
            if (hdnpage_type != undefined && hdnpage_type != null && hdnpage_type.trim() != '') {
                loadStepsdata();
                if (hdnpage_type == 'atInformationPage') {
                    is_judgment_screen = false;
                    //setTimeout(function () {
                    //    $('#hdnPageNumber').val(output.current_step + 1);
                    //    $('#MainContent_hdnPageNo').trigger('click');
                    //}, 100);
                }
                if (hdnpage_type == 'atShowLocalResults') {
                    if (output.PipeParameters.resultsTitle == '') {
                        $('#img').css('filter', 'grayscale(1)');
                        $('#imgFullScreen').css('display', 'none');
                    }
                    else {
                        $('#img').css('filter', 'none');
                        $('#imgFullScreen').css('display', 'block');
                    }
                    is_judgment_screen = false;
                    $("#divshow_satisfactory").removeClass('d-none');
                    if ($('#titleDiv2').length > 0) {
                        $('.divHeader2').css('padding-top', parseInt($('#titleDiv2').height() + 27) + 'px');
                    }
                    $("#divshow_satisfactory").addClass('d-none');
                    $("#divredo_judgement").removeClass('d-none');
                    if ($('#titleDiv3').length > 0) {
                        $('.divHeader3').css('padding-top', parseInt($('#titleDiv3').height() + 27) + 'px');
                    }
                    $("#divredo_judgement").addClass('d-none');
                    $("#divJudgment_table").removeClass('d-none');
                    if ($('#titleDiv4').length > 0) {
                        $('.divHeader4').css('padding-top', parseInt($('#titleDiv4').height() + 27) + 'px');
                    }
                    $("#divJudgment_table").addClass('d-none');
                    reverseLocal = output.PipeParameters.defaultsort < 2 ? false : true;
                    columnsortLocal = columns[output.PipeParameters.defaultsort] != null ? columns[output.PipeParameters.defaultsort] : columns[0];

                    if ($.cookie('Normalization') != undefined) {
                        hdnNormalization = $.cookie('Normalization');
                        if (hdnNormalization != undefined && hdnNormalization != null && hdnNormalization != '') {
                            $('#ddlNormalization, #mlddlGlobalNormalization').val(hdnNormalization).trigger('change');
                        }
                    }

                    if ($.cookie('CallJudgementTable') != undefined && $.cookie('CallJudgementTable') != null) {
                        if ($.cookie('CallJudgementTable') == "True") {
                            //document.cookie = 'CallJudgementTable =; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                            /*$.cookie('CallJudgementTable', "False");*/
                            $.removeCookie('CallJudgementTable');
                            $.removeCookie('SelectionID');
                            $.removeCookie('judgementStep')
                            bindDynamicHtml('Judgment_table');
                        }
                    }
                    initT2S();
                }
                if (hdnpage_type == 'atShowGlobalResults') {
                    is_judgment_screen = false;
                    reverseGlobal = output.PipeParameters.defaultsort < 2 ? false : true;
                    columnsortGlobal = columns[output.PipeParameters.defaultsort] != null ? columns[output.PipeParameters.defaultsort] : columns[0];
                    LoadGlobalHierarchy();
                    $('#tblGlobalResults').removeClass('d-none');

                    if ($.cookie('Normalization') != undefined) {
                        hdnNormalization = $.cookie('Normalization');
                        if (hdnNormalization != undefined && hdnNormalization != null && hdnNormalization != '') {
                            $('#ddlGlobalNormalization, #mddlGlobalNormalization').val(hdnNormalization).trigger('change');
                        }
                    }
                    initT2S();
                }
                if (hdnpage_type == 'atSpyronSurvey') {
                    SurveyAnswers = eval(output.PipeParameters.SurveyAnswers);
                    //answer = eval(output.PipeParameters.SurveyAnswers);
                    SurveyPageQuestions = eval(output.PipeParameters.SurveyPage.Questions);
                    if (SurveyPageQuestions != undefined && SurveyPageQuestions != null && SurveyPageQuestions.length > 0) {
                        //answer.length = SurveyPageQuestions.length;
                        for (var i = 0; i < SurveyPageQuestions.length; i++) {
                            var question = SurveyPageQuestions[i];
                            if (question.Type == 0 || question.Type == 1 || question.Type == 2 || question.Type == 14 || question.Type == 15) {
                                answer[i] = SurveyAnswers[i][0];
                                //answer.push(SurveyAnswers[i][0]);
                            }
                            if (question.Type == 3) {
                                if (question.Variants != undefined && question.Variants != null && question.Variants.length > 0) {
                                    var variant = question.Variants;
                                    var type3 = [];
                                    type3.length = 2;
                                    for (var j = 0; j < variant.length; j++) {
                                        if (j == 0) {
                                            type3[0] = text_is_equal(SurveyAnswers[i][0], variant[j].Text, false);
                                        }
                                        else {
                                            type3[1] = get_othervalue(SurveyAnswers[i][0], question.Type, variant[j].Text, i);
                                        }
                                    }
                                    answer[i] = type3;
                                }
                            }
                            if (question.Type == 4) {
                                if (question.Variants != undefined && question.Variants != null && question.Variants.length > 0) {
                                    var variant = question.Variants;
                                    var type4 = [];
                                    type4.length = variant.length - 1;
                                    for (var j = 0; j < variant.length; j++) {
                                        if (variant[j].Type == 0) {
                                            type4[j] = text_is_equal(SurveyAnswers[i][0], variant[j].Text, true, question);
                                        }
                                        else {
                                            type4[j] = get_othervalue(SurveyAnswers[i][0], question.Type, variant[j].Text, i);
                                        }
                                    }
                                    answer[i] = type4;
                                }
                            }
                            if (question.Type == 5) {
                                if (question.Variants != undefined && question.Variants != null && question.Variants.length > 0) {
                                    var variant = question.Variants;
                                    var type5 = [];
                                    type5.length = 2;
                                    type5[0] = get_combobox_value(question.Variants, SurveyAnswers[i][0]);
                                    if (variant[variant.length - 1].Type == 2) {
                                        type5[1] = (SurveyAnswers[i][0]).indexOf(':') > -1 ? get_othervalue(SurveyAnswers[i][0], question.Type) : '';
                                    }
                                    answer[i] = type5;
                                }
                            }
                            if (question.Type == 12) {
                                if (output.PipeParameters.objectivelist != undefined && output.PipeParameters.objectivelist != null
                                    && output.PipeParameters.objectivelist.length > 0) {
                                    var type12 = [];
                                    type12.length = output.PipeParameters.objectivelist.length - 1;
                                    for (var j = 0; j < output.PipeParameters.objectivelist.length; j++) {
                                        var objective = output.PipeParameters.objectivelist[j];
                                        type12[j] = objective.isDisabled;
                                    }
                                    answer[i] = type12;
                                }
                            }
                            if (question.Type == 13) {
                                if (output.PipeParameters.alternativelist != undefined && output.PipeParameters.alternativelist != null
                                    && output.PipeParameters.alternativelist.length > 0) {
                                    var type13 = [];
                                    type13.length = output.PipeParameters.alternativelist.length - 1;
                                    for (var j = 0; j < output.PipeParameters.alternativelist.length; j++) {
                                        var alternative = output.PipeParameters.alternativelist[j];
                                        type13[j] = alternative.isDisabled;
                                        $('#qType_13_' + j).prop('checked', alternative.isDisabled);
                                    }
                                    answer[i] = type13;
                                }
                            }
                        }
                    }
                    initT2S();
                }
                if (hdnpage_type == 'atNonPWOneAtATime') {
                    if (output.parent_node_info == '') {
                        $('#img2').css('filter', 'grayscale(1)');
                        $('#ibtnFullscreen').css('display', 'none');
                    }
                    else {
                        $('#img2').css('filter', 'none');
                        $('#ibtnFullscreen').css('display', 'block');
                    }
                    if (output.non_pw_type == 'mtRatings') {
                        get_ratings_data();
                        hdnpairwise_type = output.non_pw_type;
                    }
                    if (output.non_pw_type == 'mtDirect') {
                        get_direct_data();
                    }
                    if (output.non_pw_type == 'mtStep' || output.non_pw_type == 'mtRegularUtilityCurve') {
                        if (output.isPipeViewOnly) {
                            $('#titleDiv a').css('display', 'none');
                            $('#steps-functionInput,#resetIcon,#stepsFunctionSlider,#sliderCurve,#uCurveInput,#resetIcon').css('pointer-events', 'none');
                        }
                        if (output.show_comments) {
                            if (output.comment !== '') {
                                $('#ImgComment_iconEmpty_5').css('display', 'none')
                                $('#ImgComment_icon_5').css('display', 'block')
                            }
                        }
                        else {
                            $('#ImgComment_iconEmpty_5').css('display', 'none')
                            $('#ImgComment_icon_5').css('display', 'none')
                        }
                    }
                    if (output.first_node_info == '' && output.wrt_first_node_info == '') {
                        $('#lnkUtilityInfoDoc').css('filter', 'grayscale(1)');
                        $('#div_row_wrapper_0 a').css('filter', 'grayscale(1)');
                        $('#comment_icon_0').css('filter', 'grayscale(1)');
                    }
                    else {
                        $('#lnkUtilityInfoDoc').css('filter', 'none');
                        $('#div_row_wrapper_0 a').css('filter', 'none');
                        $('#comment_icon_0').css('filter', 'none');
                    }
                    initT2S();
                }
                if (hdnpage_type == 'atNonPWAllChildren' || hdnpage_type == 'atNonPWAllCovObjs') {
                    if (output.parent_node_info == '') {
                        $('#img2').css('filter', 'grayscale(1)');
                        $('#ibtnFullscreen').css('display', 'none');
                    }
                    else {
                        $('#img2').css('filter', 'none');
                        $('#ibtnFullscreen').css('display', 'block');
                    }
                    hdnpairwise_type = output.non_pw_type;
                    if (hdnpairwise_type == 'mtRatings') {
                        multi_non_pw_data = eval(output.multi_non_pw_data);
                        old_multi_non_pw_data = multi_non_pw_data;
                        active_multi_index = $('#MainContent_MultiRatingsControl_hdnactive_multi_index').val();
                        active_multi_index = active_multi_index == '' ? 0 : active_multi_index;
                        new_active_multi_index = active_multi_index;
                        for (var j = 0; j < multi_non_pw_data.length; j++) {
                            multi_ratings_row[j] = get_multi_ratings_data(multi_non_pw_data[j].RatingID, multi_non_pw_data[j].DirectData, j);
                            multi_direct_value[j] = multi_ratings_row[j][0] != -1 ? multi_ratings_row[j][0] : multi_direct_value[j];
                        }
                        for (var i = 0; i < multi_direct_value.length; i++) {
                            if (parseFloat(multi_direct_value[i]) >= 0) {
                                $('#input_1_' + i).val(multi_direct_value[i].toFixed(4));
                                if (multi_ratings_row[i][1].toLocaleLowerCase() == 'direct value')
                                    $('#input_2_' + i).val(multi_direct_value[i].toFixed(4));
                            }
                            else if (multi_ratings_row[i][1].toLocaleLowerCase() == 'none') {
                                $('#input_1_' + i).val('0.0000');
                                $('#input_2_' + i).val('0.0000');
                            }
                            else {
                                $('#input_1_' + i).val('');
                                $('#input_2_' + i).val('');
                            }
                        }
                        for (var i = 0; i < multi_ratings_row.length; i++) {
                            var preogree_bar_val = parseFloat(multi_ratings_row[i][0] * 100);
                            if (preogree_bar_val.toString().length > 3) {
                                if (preogree_bar_val > 0 || multi_ratings_row[i][1].toLowerCase() == 'none') {
                                    if (output.showPriorityAndDirect) {
                                        $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1] + (' ' + preogree_bar_val.toFixed(2) + '%'));
                                        $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1] + (' ' + preogree_bar_val.toFixed(2) + '%'));
                                    }
                                    else {
                                        $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1]);
                                        $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1]);
                                    }
                                }
                                else {
                                    if (multi_ratings_row[i][1].toLowerCase() == 'none') {
                                        $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1] + (' ' + preogree_bar_val.toFixed(2) + '%'));
                                        $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1] + (' ' + preogree_bar_val.toFixed(2) + '%'));
                                    }
                                    else {
                                        $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1]);
                                        $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1]);
                                    }
                                }
                                //if (preogree_bar_val > 0) {
                                //    $('#div_row_wrapper_' + i + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                                //}
                                //else {
                                //    $('#div_row_wrapper_' + i + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                                //}
                                $('#div_row_wrapper_' + i + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                            }
                            else {
                                if (preogree_bar_val > 0 || multi_ratings_row[i][1].toLowerCase() == 'none') {
                                    if (output.showPriorityAndDirect) {
                                        $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1] + (' ' + parseFloat(preogree_bar_val) + '%'));
                                        $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1] + (' ' + parseFloat(preogree_bar_val) + '%'));
                                    }
                                    else {
                                        $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1]);
                                        $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1]);
                                    }
                                }
                                else {
                                    $('#dropdownHeaderValue' + i).text(multi_ratings_row[i][1]);
                                    $('#spndropdownHeaderValueM' + i).text(multi_ratings_row[i][1]);
                                }
                                if (preogree_bar_val > 0) {
                                    $('#div_row_wrapper_' + i + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                                }
                                else {
                                    $('#div_row_wrapper_' + i + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                                }
                            }
                            $('#progress_bar_' + i).attr('aria-valuenow', preogree_bar_val.toFixed(4)).css('width', preogree_bar_val.toFixed(4) + '%');
                            $('#dropdownHeaderValueM' + i).attr('aria-valuenow', preogree_bar_val.toFixed(4)).css('width', preogree_bar_val.toFixed(4) + '%');
                            if (multi_ratings_row[i][1] == 'Not Rated') {
                                $('#close_icon_' + i + ' i').hide();
                            }
                            if (hdnpairwise_type != 'mtRatings') {
                                if (output.multi_non_pw_data[i].Infodoc == '' && output.multi_non_pw_data[i].InfodocWRT == '') {
                                    $('#div_row_wrapper_' + i + ' a').css('filter', 'grayscale(1)');
                                }
                                else {
                                    $('#div_row_wrapper_' + i + ' a').css('filter', 'none');
                                }
                            }
                            else {
                                if (output.multi_non_pw_data[i].Infodoc == '' && output.multi_non_pw_data[i].InfodocWRT == '') {
                                    $('#div_row_wrapper_' + i + ' #comment_icon_' + i).css('filter', 'grayscale(1)');
                                }
                                else {
                                    $('#div_row_wrapper_' + i + ' #comment_icon_' + i).css('filter', 'none');
                                }
                            }
                        }

                        if (multi_non_pw_data[active_multi_index].Ratings != null && multi_non_pw_data[active_multi_index].Ratings.length > 0) {
                            for (var i = 0; i < multi_non_pw_data[active_multi_index].Ratings.length; i++) {
                                var Ratings = multi_non_pw_data[active_multi_index].Ratings[i];
                                if (Ratings.Name == multi_ratings_row[active_multi_index][1]) {
                                    $("input[type='radio'][id='" + Ratings.GuidID + "']").prop("checked", true);
                                }
                            }
                        }

                        setTimeout(function () {
                            //var el = $('#div_row_wrapper_' + parseInt(active_multi_index));
                            //var elOffset = el.offset().top;
                            //var elHeight = el.height();
                            //var windowHeight = $(window).height();
                            //var offset;

                            //if (elHeight < windowHeight) {
                            //    offset = elOffset - ((windowHeight / 2) - (elHeight / 2));
                            //}
                            //else {
                            //    offset = elOffset;
                            //}
                            //var speed = 700;
                            //$('html, body').animate({ scrollTop: offset });   
                            if (output.page_type == 'atNonPWAllChildren') {
                                if (output.parent_node_info == '') {
                                    $('#img2').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                                }
                                else {
                                    $('#img2').css({ 'filter': 'none', 'opacity': '1' })
                                }
                                if (!!$.cookie('GenActiveRowID')) {
                                    var index = $.cookie("GenActiveRowID");
                                    //var indexOne = $.cookie("GenActiveRowIDOne");
                                    if (index == "-1") {
                                        if (new_active_multi_index != active_multi_index && active_multi_index == 0) {
                                            active_multi_index = new_active_multi_index;
                                        }
                                        var $container = $('div.rating_scale_data'), $scrollTo = $('#div_row_wrapper_' + parseInt(active_multi_index));
                                        $container.animate({
                                            scrollTop: $scrollTo.offset().top - $container.offset().top + $container.scrollTop()
                                        });
                                    }
                                    else {
                                        $.cookie("GenActiveRowID", "-1");
                                    }

                                }
                            }
                            else {
                                var $container = $('div.rating_scale_data'), $scrollTo = $('#div_row_wrapper_' + parseInt(active_multi_index));
                                $container.animate({
                                    scrollTop: $scrollTo.offset().top - $container.offset().top + $container.scrollTop()
                                });
                            }


                        }, 500);

                        //$('.changeView').removeClass('container');
                        //$('.changeView').addClass('container-fluid');
                    }
                    else if (hdnpairwise_type == 'mtDirect') {

                        if (output.parent_node_info == '') {
                            $('#img2').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                            $('#imgFullScreen').css({ 'display': 'none' })
                        }
                        else {
                            $('#img2').css({ 'filter': 'none', 'opacity': '1' })
                            $('#imgFullScreen').css({ 'display': 'block' })
                        }
                        multi_non_pw_data = eval(output.multi_non_pw_data);
                        active_multi_index = 0;
                        if (!!$.cookie('GenActiveRowID')) {
                            var index = $.cookie("GenActiveRowID");
                            if (index != "-1") {
                                active_multi_index = index;
                                $.cookie("GenActiveRowID", "-1");
                            }
                            else {
                                for (i = 0; i < multi_non_pw_data.length; i++) {
                                    if (multi_non_pw_data[i].DirectData == -1) { //undefined
                                        active_multi_index = i;
                                        break;
                                    }
                                }
                            }
                        }
                        else {
                            for (i = 0; i < multi_non_pw_data.length; i++) {
                                if (multi_non_pw_data[i].DirectData == -1) { //undefined
                                    active_multi_index = i;
                                    break;
                                }
                            }
                        }
                        get_multi_direct_data(active_multi_index);

                        setTimeout(function () {
                            if (at_multi_direct_slider.length > 0) {
                                var disablePage = false;
                                for (var i = 0; i < at_multi_direct_slider.length; i++) {
                                    //at_multi_direct_slider[i] = at_multi_direct_slider[i] == '' ? '0' : at_multi_direct_slider[i];
                                    if (at_multi_direct_slider[i] >= 0) {
                                        $('#at_direct_slider' + i).slider("value", (parseFloat(at_multi_direct_slider[i]) * 100));
                                        $('#at_direct_input' + i).val(at_multi_direct_slider[i]);
                                        $('#close_icon_' + i + ' i').show();
                                    }
                                    else {
                                        $('#at_direct_slider' + i).slider("value", 0);
                                        $('#at_direct_input' + i).val('');
                                        $('#close_icon_' + i + ' i').hide();
                                    }
                                }
                            }

                            $('div.row_wrapper').removeClass('active_table_row');
                            $('#multi_direct_slider_' + active_multi_index).addClass('active_table_row');
                        }, 20);
                    }
                    initT2S();
                }
                if (hdnpage_type == 'atPairwise') {
                    if (output.parent_node_info == '') {
                        $('#img2').css('filter', 'grayscale(1)');
                        $('#ibtnfullScreen').css('display', 'none');
                    }
                    else {
                        $('#img2').css('filter', 'none');
                        $('#ibtnfullScreen').css('display', 'block');
                    }
                    hdnpairwise_type = output.pairwise_type;
                    if (hdnpairwise_type == 'ptGraphical') {

                        advantages = output.advantage;
                        outputvalue = output.value;
                        if (advantages != undefined && advantages != null && advantages != '') {
                            if (parseInt(advantages) == -1) {
                                graphical_slider[1] = (1600 * ((parseFloat(outputvalue)) / ((parseFloat(outputvalue)) + 1)));
                                graphical_slider[0] = 1600 - parseFloat(graphical_slider[1]);
                                numericalSlider[0] = (800 * ((parseFloat(outputvalue)) / ((parseFloat(outputvalue)) + 1)));
                            }
                            if (parseInt(advantages) == 1) {
                                graphical_slider[0] = (1600 * (((parseFloat(outputvalue))) / ((parseFloat(outputvalue)) + 1)));
                                graphical_slider[1] = 1600 - parseFloat(graphical_slider[0]);
                                numericalSlider[0] = 800 - (800 * ((parseFloat(outputvalue)) / ((parseFloat(outputvalue)) + 1)));
                            }
                            if (parseInt(advantages) == 0) {
                                graphical_slider[0] = 800;
                                graphical_slider[1] = 1600 - graphical_slider[0];
                                numericalSlider[0] = 400;
                            }
                        }
                        main_bar[0] = parseFloat(outputvalue) > 0 ? parseFloat((parseInt(advantages) == 1 ? (parseFloat(outputvalue)) * 1 : 1).toFixed(2)) : "";
                        main_bar[1] = parseFloat(outputvalue) > 0 ? parseFloat((parseInt(advantages) == -1 ? (parseFloat(outputvalue)) * 1 : 1).toFixed(2)) : "";
                        main_bar_txt[0] = (parseInt(advantages) == 1 ? (parseFloat(outputvalue)) * 1 : 1).toFixed(2);
                        main_bar_txt[1] = (parseInt(advantages) == -1 ? (parseFloat(outputvalue)) * 1 : 1).toFixed(2);

                        main_bar1 = main_bar[0];
                        main_bar2 = main_bar[1];
                        $('#noUiInput11').val(main_bar[0]);
                        $('#noUiInput21').val(main_bar[1]);

                        if ($('#noUiInput11').val() === '' && $('#noUiInput21').val() === '') {
                            $('.arrowicons').hide();
                        }

                        initializeNoUIGraphical(numericalSlider[0], numericalSlider[0] + 800);
                        if (main_bar[0] == "" && main_bar[1] == "") {
                            setTimeout(function () {
                                var slider = getNoUiSlider(undefined);
                                var SliderVal = slider.noUiSlider.get();
                                slider.noUiSlider.set([400, 1200]);
                                setNoUiColor(false, -1);
                                $('#graphicalSlider').addClass('newsample1');
                                $(".noUi-connect").css("background-color", "#AAAAAA");
                                $(".graph-green-div").css("background-color", "#AAAAAA");
                                //$('div.reverse_icon, div.close_icon').hide();
                                main_bar[0] = "";
                                main_bar[1] = "";
                                sliderLeft = parseFloat(main_bar[0]);
                                sliderRight = parseFloat(main_bar[1]);

                            }, 1000);
                            IsUndefined = true;
                        }
                    }
                    if (hdnpairwise_type == 'ptVerbal') {
                        if (output.value <= 0) {
                            $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                        }
                        else {
                            $('.chart_wrapper .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                        }
                    }
                    get_collapsed_info_docs();

                    if (output.isPipeViewOnly) {
                        $('.bxs-edit').addClass('d-none');
                    }
                    initT2S();
                }
                if (hdnpage_type == 'atAllPairwise') {
                    if (output.parent_node_info == '') {
                        $('#img2').css('filter', 'grayscale(1)');
                        $('#ibtnfullScreen').css('display', 'none');
                    }
                    else {
                        $('#img2').css('filter', 'none');
                        $('#ibtnfullScreen').css('display', 'block');
                    }
                    hdnpairwise_type = output.pairwise_type;
                    multi_pw_data = eval(output.multi_pw_data);
                    hdnMultiPwData = eval(output.multi_pw_data);
                    if (hdnpairwise_type == 'ptGraphical') {
                        is_multi = true;
                        var flast = false;
                        active_multi_index = 0;
                        hdnMultiPwData = eval(output.multi_pw_data);

                        multi_data = multi_pw_data;
                        for (i = 0; i < multi_data.length; i++) {
                            if (multi_data[i].Advantage == -1) {
                                var reverse = (800 * ((multi_data[i].Value) / ((multi_data[i].Value) + 1)));
                                participant_slider[i] = reverse;
                            }
                            if (multi_data[i].Advantage == 1) {
                                participant_slider[i] = 800 - (800 * ((multi_data[i].Value) / ((multi_data[i].Value) + 1)));
                            }
                            if (multi_data[i].Advantage == 0) {
                                participant_slider[i] = 400;
                            }
                            left_input[i] = parseFloat((multi_data[i].Advantage > 0 ? (multi_data[i].Value) * 1 : 1).toFixed(2));
                            right_input[i] = parseFloat((multi_data[i].Advantage < 0 ? (multi_data[i].Value) * 1 : 1).toFixed(2));

                            if (multi_data[i].Value <= 0) {
                                left_input[i] = "";
                                right_input[i] = "";
                            }

                            if (multi_data[i].Value == -2147483648000) { //undefined
                                active_multi_index = active_multi_index == 0 ? i : active_multi_index;
                            }
                        }

                        if (multi_pw_data.length > 50) {
                            //allPairwiseHtml(21, 100);
                            if (active_multi_index > 45) {
                                multi_last_index = $('div.chart_wrapper').last().data('val');
                                var to = 0;
                                if (active_multi_index <= multi_last_index) {
                                    to = parseInt(multi_last_index + 100);
                                    to = to > multi_pw_data.length - 1 ? multi_pw_data.length - 1 : to;
                                }
                                else if (active_multi_index > multi_last_index) {
                                    to = parseInt(active_multi_index + 100);
                                    to = to > multi_pw_data.length - 1 ? multi_pw_data.length - 1 : to;
                                }
                                allPairwiseHtml(parseInt(multi_last_index + 1), to);
                            }
                        }

                        var multiLength = multi_pw_data.length > 50 ? 11 : multi_pw_data.length;
                        for (i = 0; i < multiLength; i++) {
                            if ((multi_pw_data.length - 1) == i) {
                                flast = true;
                            }
                            $('#input1' + i).val(left_input[i] == undefined ? '' : left_input[i]);
                            $('#input2' + i).val(right_input[i] == undefined ? '' : right_input[i]);

                            initializeNoUIAllGraphical(participant_slider[i], participant_slider[i] + 800, i, flast);
                        }

                        setTimeout(function () {
                            var disablePage = false;
                            var eCount = 0;
                            if (multi_pw_data.length > 0) {
                                for (i = 0; i < multi_pw_data.length; i++) {
                                    if (left_input[i] == "" && right_input[i] == "") {
                                        disablePage = true;
                                        $('#reverse_icon_' + i).hide();
                                        $('#close_icon_' + i).hide();
                                    }
                                }
                                if (output.pipeOptions.dontAllowMissingJudgment) {
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
                            }
                        }, 40);
                        slideScroll(true);
                    }
                    else if (hdnpairwise_type == 'ptVerbal') {
                        if (output.multi_pw_data != null && output.multi_pw_data.length > 50) {
                            //allPairwiseHtml(21, 100);
                        }
                        for (i = 0; i < hdnMultiPwData.length; i++) {
                            if (parseFloat(hdnMultiPwData[i].Value) <= 0) {
                                if (hdnMultiPwData.length > 50) {
                                    if (i > 45) {
                                        multi_last_index = $('div.chart_wrapper').last().data('val');
                                        var to = 0;
                                        if (i <= multi_last_index) {
                                            to = parseInt(multi_last_index + 100);
                                            to = to > hdnMultiPwData.length - 1 ? hdnMultiPwData.length - 1 : to;
                                        }
                                        else if (i > multi_last_index) {
                                            to = parseInt(i + 100);
                                            to = to > hdnMultiPwData.length - 1 ? hdnMultiPwData.length - 1 : to;
                                        }
                                        allPairwiseHtml(parseInt(multi_last_index + 1), to);
                                    }
                                }
                                break;
                            }
                        }
                        $('.chart_wrapper').removeClass('active_row');
                        slideMultiPWVerbal(true);
                        //setTimeout(function () {
                        //    $.removeCookie('SelectionID');
                        //}, 50);
                    }
                    initT2S();
                    //multi_last_index = $('div.chart_wrapper').last().data('val');
                    //if (active_multi_index >= parseInt(multi_last_index)) {
                    //    allPairwiseHtml(parseInt(multi_last_index) + 1, active_multi_index + 50);
                    //}
                }
                if (hdnpage_type == 'atSensitivityAnalysis') {
                    if (output != undefined && output != null && output.initSA != null && output.initSA != undefined) {
                        Sense = output.initSA;
                        initData();
                        initPage();
                        $("#DSACanvas").sa("resetSA");
                    }
                    if (output.initSA.GetSATypeString == 'GSA') {
                        if (output.initSA.GetGSASubobjectives != undefined && output.initSA.GetGSASubobjectives != null && output.initSA.GetGSASubobjectives.length > 0) {
                            $("#selSubobjectives").empty();
                            $.each(output.initSA.GetGSASubobjectives, function () {
                                $("#selSubobjectives").append($("<option/>").val(this[0]).text(this[2]));
                            });
                        }
                    }
                }
            }
        }
        else if (output.lockedInfo != undefined && output.lockedInfo != null && output.lockedInfo.status) {
            $("#overlay").css('display', 'none');
            $('#topHeader').removeClass('d-none').addClass('d-none');
            if (output.lockedInfo.teamTimeUrl.length > 0) {
                $("div.teamtime-started").remove();
                $(".tt-full-height.tt-body.tt-pipe.anytime-wrap").remove();
                projectLockStatusTimer = setTimeout(function () {
                    window.onbeforeunload = null;
                    window.location.href = output.lockedInfo.teamTimeUrl;
                }, 5000);
            }

            setInterval(function () {
                $.ajax({
                    type: "POST",
                    url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/GetModelLockedStatus",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({}),
                    success: function (data) {
                        var result = data.d;
                        if (result != undefined && result != null && result != "") {
                            //if (result.status != output.lockedInfo != null ? output.lockedInfo.status : "")
                            if (!result.status && (result.status != output.lockedInfo != null ? output.lockedInfo.status : "")) {
                                window.open("/AnytimeComparion/pages/Anytime/Anytime.aspx", "_self", "");
                            }
                        }
                    },
                    error: function (response) {
                    }
                });
            }, 5000);
        }
        else if (!output.validTimeline) {
            $("#overlay").css('display', 'none');
            $('#topHeader').removeClass('d-none').addClass('d-none');
        }
        else if (!output.access_avaiable) {
            $("#overlay").css('display', 'none');
            $('#topHeader').removeClass('d-none').addClass('d-none');
        }
    }
}

var text_is_equal = function (answer, variant, is_chb, question) {
    if (answer) {
        if (is_chb) {
            var options = answer.split(";");
            var totalAnsweredChb = options.length - 1;
            var min = question.MinSelectedVariants;
            var max = question.MaxSelectedVariants == 0 ? Number.MAX_VALUE : question.MaxSelectedVariants;
            for (x in options) {
                if (options[x] == variant) {
                    return true;
                }
            }
        }
        else {
            if (variant == "Other:") {
                var options = answer.split(":");
                if (answer.indexOf(":") > -1)
                    return variant;
            }
            return answer;

        }
    }
    return false;
}

var get_combobox_value = function (variants, answer) {
    if (answer.indexOf(";") > -1) {
        var strippedAnswer = answer.split(";");
        answer = strippedAnswer[0];
    }
    for (i = 0; i < variants.length; i++) {
        if (variants[i].VariantValue == answer) {
            return variants[i];
        }
    }
}

var get_othervalue = function (answer, type, variant, index) {
    //for checkbox other value
    if (type == 3) {
        if (answer.indexOf(":") > -1) {
            var options = answer.split(":");
            return options[0];
        }
    }
    if (type == 4) {
        var options = answer.split(";");
        var last = options.length - 1;

        for (x in options) {
            if (x == last) {
                if ((options[last]).indexOf(":") > -1) {
                    var text = options[last].split(":");
                    //$scope.change_otherAnswer(index, answer, variant);
                    return text[0];
                }
                else
                    return "";
            }
        }
        return "";
    }

    //for combobox other value
    if (type == 5) {
        var options = answer.split(":");
        return options[0];
    }
}


//$(window).scroll(function () {
//    if (hdnpage_type == 'atAllPairwise') {
//        if ($(window).scrollTop() + $(window).height() > $(document).height() - 200) {
//            alert('atAllPairwise');
//            multi_last_index = $('div.chart_wrapper').last().data('val');
//            allPairwiseHtml(multi_last_index + 1, multi_last_index + 100);
//        }
//    }
//});

$('div.main_wrapper').scroll(function () {
    //alert('atAllPairwise')
    if (hdnpage_type == 'atAllPairwise') {
        if ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight) {
            /*alert('end reached');*/
            multi_last_index = $('div.chart_wrapper').last().data('val');
            allPairwiseHtml(multi_last_index + 1, multi_last_index + 100);
        }
        //if ($(window).scrollTop() + $(window).height() > $(document).height() - 200) {
        //    //alert('atAllPairwise');
        //    multi_last_index = $('div.chart_wrapper').last().data('val');
        //    //alert('multi_last_index: ' + multi_last_index);
        //    //allPairwiseHtml(multi_last_index + 1, multi_last_index + 100);
        //}
    }
});


var addLastStepButton = function () {
    // what button to displayed in footer at the last step
    if (output.current_step == output.total_pipe_steps) {
        if (is_judgment_screen) {
            if (output.doneOptions.stayAtEval == 'True') {
                showSave = output.doneOptions.logout == 'True' ? false : true;
                showFinish = output.doneOptions.logout == 'True' ? true : false;
            }
            else {
                if (output.doneOptions.logout == 'False') {
                    showSave = (output.doneOptions.redirect == 'True' && output.doneOptions.url == "") || (output.doneOptions.openProject == 'True' && output.nextProject == -1) ? true : false;
                    showFinish = (output.doneOptions.redirect == 'True' && output.doneOptions.url == "") || (output.doneOptions.openProject == 'True' && output.nextProject == -1) ? false : true;
                }
                else {
                    showFinish = true;
                }
            }
        }
        else {
            showSave = false;
            if (output.doneOptions.stayAtEval == 'True') {
                showFinish = output.doneOptions.logout == 'True' ? true : false;
                //showFinish = output.doneOptions.logout == 'False' ? true : false;
            }
            else {
                if (output.doneOptions.logout == 'False') {
                    showFinish = (output.doneOptions.redirect == 'True' && output.doneOptions.url == "") || (output.doneOptions.openProject == 'True' && output.nextProject == -1) ? false : true;
                    //showFinish = (output.doneOptions.redirect == 'True' && output.doneOptions.url == "") || (output.doneOptions.openProject == 'True' && output.nextProject == -1) ? true : false;
                }
                else {
                    showFinish = true;
                }
            }
        }
    }
}

var get_collapsed_info_docs = function () {
    if (is_multi) {

    }
    else {
        try {
            if (output.infodoc_params != undefined && output.infodoc_params != null && output.infodoc_params.length > 0) {
                for (i = 0; i < 5; i++) {
                    ////to display or hide the comment box on page load
                    if (i == 0)
                        get_collapse_cookies('parent-node');
                    if (i == 1 || i == 2) {
                        get_collapse_cookies('pw-nodes');
                    }
                    if (i == 3 || i == 4) {
                        get_collapse_cookies('pw-wrt-nodes');
                    }
                    //output.infodoc_params[i] = output.infodoc_params[i] ? output.infodoc_params[i] : "c=-1&w=200&h=200";
                    //var node_params = output.infodoc_params[i].split("&");
                    //var is_close = node_params[0].split("=");
                    //if (is_close[1] == -1) {
                    //    temp_collapsed_info_docs[i] = false;
                    //    if (is_html_empty(output.parent_node_info)) {
                    //        temp_collapsed_info_docs[0] = true;
                    //    }

                    //    if (is_html_empty(output.first_node_info)) {
                    //        temp_collapsed_info_docs[1] = true;
                    //    }

                    //    if (is_html_empty(output.second_node_info)) {
                    //        temp_collapsed_info_docs[2] = true;
                    //    }

                    //    if (is_html_empty(output.wrt_first_node_info)) {
                    //        temp_collapsed_info_docs[3] = true;
                    //    }

                    //    if (is_html_empty(output.wrt_second_node_info)) {
                    //        temp_collapsed_info_docs[4] = true;
                    //    }
                    //}
                    //else {
                    //    //temp_collapsed_info_docs[i] = is_close[1] == 1 ? true : false;
                    //    temp_collapsed_info_docs[i] = is_close[1] == 0 ? true : false;
                    //}


                    ////get headings
                    //if (typeof (node_params[3]) != "undefined") {
                    //    var heading = node_params[3].split("=");
                    //    info_docs_headings[i] = heading[1];
                    //}
                    //else {
                    //    info_docs_headings[i] = "-1";
                    //}

                    //setCollapsedInfoDocsValue(i, temp_collapsed_info_docs[i]);
                }
            }
        }
        catch (e) {
        }

        try {
            if (output.currentProjectInfo != undefined && output.currentProjectInfo != null
                && output.currentProjectInfo.project_id != undefined && output.currentProjectInfo.project_id != null
                && output.currentProjectInfo.project_id != '' && parseInt(output.currentProjectInfo.project_id) > 0) {
                project_id = output.currentProjectInfo.project_id;
            }

            $.ajax({
                type: "POST",
                url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setSingleCollapsePrivateVar",
                data: JSON.stringify({
                    ProjectID: project_id,
                    step: current_step,
                    collapsed_status_list: temp_collapsed_info_docs
                }),
                dataType: "json",
                async: false,
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    callGetCollapseCookies();
                },
                error: function (response) {
                    collapsed_info_docs = temp_collapsed_info_docs;
                }
            });
        }
        catch (e) {
            collapsed_info_docs = temp_collapsed_info_docs;
        }

        if (infodocSizes == null) {
            $.ajax({
                type: "POST",
                url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/getInfoDocSizes",
                dataType: "json",
                async: false,
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    var infodoc_sizes = response.d;
                    var temp_infodoc_sizes = [];
                    var width = "auto";

                    if (infodoc_sizes == null) {
                        temp_infodoc_sizes[0] = new Array(width, "150px");
                        temp_infodoc_sizes[1] = new Array(width, "150px");
                        temp_infodoc_sizes[2] = new Array(width, "150px");
                        infodoc_sizes = temp_infodoc_sizes;
                    }

                    infodocSizes = infodoc_sizes;
                    //setTimeout(function () {
                    change_info_doc_image_size();
                    //}, 100);
                },
                error: function (response) {
                }
            });
        }
    }
}

var callGetCollapseCookies = function () {
    if (is_multi) {
        //$scope.get_collapse_cookies("pw-nodes");
        //$scope.get_collapse_cookies("pw-wrt-nodes");
        if (output.page_type == "atPairwise") {
            get_collapse_cookies("parent-node");
        }
        get_collapse_cookies("pw-nodes");

        if (output.non_pw_type != "mtDirect" && output.non_pw_type != "mtRatings") {
            get_collapse_cookies("pw-wrt-nodes");
        }

        if (output.page_type != "atPairwise") {
            get_collapse_cookies("pw-nodes");
        }
    } else {
        try {
            if (output.page_type == "atPairwise") {
                get_collapse_cookies("parent-node");
            }
            get_collapse_cookies("pw-nodes");

            if (output.non_pw_type != "mtDirect" && output.non_pw_type != "mtRatings" && output.non_pw_type != "mtStep" && output.non_pw_type != "mtRegularUtilityCurve") {
                get_collapse_cookies("pw-wrt-nodes");
            }

            if (output.page_type != "atPairwise") {
                get_collapse_cookies("pw-nodes");
            }
            // alert("got cookies!");
        } catch (e) {
            collapsed_info_docs = temp_collapsed_info_docs;
        }
    }
}

var get_collapse_cookies = function (node_type) {
    var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;
    var currentStepType = (is_multi ? "All_" : "One_") + output.pairwise_type + output.non_pw_type;

    if (output.currentProjectInfo != undefined && output.currentProjectInfo != null
        && output.currentProjectInfo.project_id != undefined && output.currentProjectInfo.project_id != null
        && output.currentProjectInfo.project_id != '' && parseInt(output.currentProjectInfo.project_id) > 0) {
        project_id = output.currentProjectInfo.project_id;
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/getCollapseCookies",
        data: JSON.stringify({
            projectId: project_id,
            stepType: currentStepType,
            node_type: node_type,
            is_multi: is_multi
        }),
        dataType: "json",
        async: false,
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            //$(".tt-accordion-content").attr("style", "");
            var is_hidden = response.d;
            var bool_hidden = is_hidden == "1" ? true : false;
            var value1, value2, value3;

            ////console.log("node_type: " + node_type);

            //Setting collapsed_info_docs values and set collapsed_info_docs to true when info doc is empty
            if (node_type == "pw-nodes" || node_type == "parent-node") {
                if (is_multi) {
                    value1 = is_html_empty(output.parent_node_info) ? true : bool_hidden;
                    setCollapsedInfoDocsValue(0, value1);

                    if (output.page_type == "atAllPairwise") {
                        value2 = is_html_empty(multi_data[active_multi_index].InfodocLeft) ? true : bool_hidden;
                        value3 = is_html_empty(multi_data[active_multi_index].InfodocRight) ? true : bool_hidden;
                        setCollapsedInfoDocsValue(1, value2);
                        setCollapsedInfoDocsValue(2, value3);
                    } else if (output.page_type != "atShowLocalResults") {
                        value2 = is_html_empty(output.multi_non_pw_data[active_multi_index].Infodoc) ? true : bool_hidden;
                        value3 = is_html_empty(output.multi_non_pw_data[active_multi_index].InfodocWRT) ? true : bool_hidden;
                        setCollapsedInfoDocsValue(1, value2);
                        setCollapsedInfoDocsValue(2, value3);
                    }
                }
                else {
                    if (node_type == "pw-nodes") {
                        if (output.non_pw_type == "mtDirect" || output.non_pw_type == "mtRatings" || output.non_pw_type == "mtStep" || output.non_pw_type == "mtRegularUtilityCurve") {
                            value1 = is_html_empty(output.parent_node_info) ? true : bool_hidden;
                            value2 = is_html_empty(output.first_node_info) ? true : bool_hidden;
                            value3 = is_html_empty(output.wrt_first_node_info) ? true : bool_hidden;
                            setCollapsedInfoDocsValue(0, value1);
                            setCollapsedInfoDocsValue(1, value2);
                            setCollapsedInfoDocsValue(3, value3);
                        }
                        else {
                            value1 = is_html_empty(output.first_node_info) && is_html_empty(output.second_node_info) ? true : bool_hidden;
                            value2 = bool_hidden;    //Doesn't have any info doc for this
                            setCollapsedInfoDocsValue(1, value1);
                            setCollapsedInfoDocsValue(2, value2);

                            if (cookieValue == undefined || cookieValue == null || cookieValue.trim() == 'false') {
                                if (($("#checkout-shipping-address-FirstNode").text().trim() == '') && ($("#checkout-shipping-address-SecondNode").text().trim() == '')) {
                                    $(".checkout-shipping-address5").toggle(false);
                                    $('.checkBoxIcon5').removeClass('fa fa-minus-square');
                                    $('.checkBoxIcon5').addClass('fa fa-plus-square');
                                }
                                else {
                                    $(".checkout-shipping-address5").toggle(!bool_hidden);
                                    if (bool_hidden) {
                                        $('.checkBoxIcon5').removeClass('fa fa-minus-square');
                                        $('.checkBoxIcon5').addClass('fa fa-plus-square');
                                    }
                                    else {
                                        $('.checkBoxIcon5').removeClass('fa fa-plus-square');
                                        $('.checkBoxIcon5').addClass('fa fa-minus-square');
                                    }
                                }
                            }
                            else {
                                if ($("#checkout-shipping-address-FirstNode").text().trim() == '') {
                                    $("#checkout-shipping-address-FirstNode").toggle(false);
                                    $('#checkBoxIconFirstNode').removeClass('fa fa-minus-square');
                                    $('#checkBoxIconFirstNode').addClass('fa fa-plus-square');
                                    $("#checkout-shipping-address-FirstNode").removeClass('d-none');
                                    $("#checkout-shipping-address-FirstNode").addClass('d-none');
                                    $("#dvfirstnodelbl").addClass('d-none');
                                    $("#dvfirstnodelbl").removeClass('d-none');
                                }
                                else {
                                    $("#checkout-shipping-address-FirstNode").toggle(!bool_hidden);
                                    if (bool_hidden) {
                                        $('#checkBoxIconFirstNode').removeClass('fa fa-minus-square');
                                        $('#checkBoxIconFirstNode').addClass('fa fa-plus-square');
                                    }
                                    else {
                                        $('#checkBoxIconFirstNode').removeClass('fa fa-plus-square');
                                        $('#checkBoxIconFirstNode').addClass('fa fa-minus-square');
                                    }
                                }

                                if ($("#checkout-shipping-address-SecondNode").text().trim() == '') {
                                    $("#checkout-shipping-address-SecondNode").toggle(false);
                                    $('#checkBoxIconSecondNode').removeClass('fa fa-minus-square');
                                    $('#checkBoxIconSecondNode').addClass('fa fa-plus-square');
                                    $("#checkout-shipping-address-SecondNode").removeClass('d-none')
                                    $("#checkout-shipping-address-SecondNode").addClass('d-none')
                                    $("#dvsecondnodelbl").removeClass('d-none');
                                    $("#dvsecondnodelbl").addClass('d-none');
                                }
                                else {
                                    $("#checkout-shipping-address-SecondNode").toggle(!bool_hidden);
                                    if (bool_hidden) {
                                        $('#checkBoxIconSecondNode').removeClass('fa fa-minus-square');
                                        $('#checkBoxIconSecondNode').addClass('fa fa-plus-square');
                                    }
                                    else {
                                        $('#checkBoxIconSecondNode').removeClass('fa fa-plus-square');
                                        $('#checkBoxIconSecondNode').addClass('fa fa-minus-square');
                                    }
                                }
                            }
                        }
                    }
                    else if (node_type == "parent-node") {
                        if (cookieValue == undefined || cookieValue == null || cookieValue.trim() == 'false') {
                            if ($("#checkout-shipping-address-Parent").text().trim() == '') {
                                $("#checkout-shipping-address-Parent").toggle(false);
                                $('#checkBoxIconParent').removeClass('fa fa-minus-square');
                                $('#checkBoxIconParent').addClass('fa fa-plus-square');
                            }
                            else {
                                $("#checkout-shipping-address-Parent").toggle(!bool_hidden);
                                if (bool_hidden) {
                                    $('#checkBoxIconParent').removeClass('fa fa-minus-square');
                                    $('#checkBoxIconParent').addClass('fa fa-plus-square');
                                }
                                else {
                                    $('#checkBoxIconParent').removeClass('fa fa-plus-square');
                                    $('#checkBoxIconParent').addClass('fa fa-minus-square');
                                }
                            }
                        }
                        else {
                            if ($("#checkout-shipping-address-Parent").text().trim() == '') {
                                $("#divparentbox").removeClass('d-none');
                                $("#divparentbox").addClass('d-none');
                            }
                            else {
                                $("#checkout-shipping-address-Parent").toggle(!bool_hidden);
                                if (bool_hidden) {
                                    $('#checkBoxIconParent').removeClass('fa fa-minus-square');
                                    $('#checkBoxIconParent').addClass('fa fa-plus-square');
                                }
                                else {
                                    $('#checkBoxIconParent').removeClass('fa fa-plus-square');
                                    $('#checkBoxIconParent').addClass('fa fa-minus-square');
                                }
                            }
                        }
                    }
                    else {
                        value1 = is_html_empty(output.parent_node_info) ? true : bool_hidden;
                        setCollapsedInfoDocsValue(0, value1);
                    }
                }
            }
            else if (node_type == "pw-wrt-nodes") {
                if (is_multi) {
                    value1 = is_html_empty(multi_data[active_multi_index].InfodocLeftWRT) && is_html_empty(output.wrt_second_node_info) ? true : bool_hidden;
                    value2 = is_html_empty(multi_data[active_multi_index].InfodocRightWRT) ? true : bool_hidden;
                    setCollapsedInfoDocsValue(3, value1);
                    setCollapsedInfoDocsValue(4, value2);
                }
                else {
                    value1 = is_html_empty(output.wrt_first_node_info) && is_html_empty(output.wrt_second_node_info) ? true : bool_hidden;
                    value2 = bool_hidden;    //Doesn't have any info doc for this
                    setCollapsedInfoDocsValue(3, value1);
                    setCollapsedInfoDocsValue(4, value2);

                    if (cookieValue == undefined || cookieValue == null || cookieValue.trim() == 'false') {
                        if (($("#checkout-shipping-address-FirstNodeWRT").text().trim() == '') && ($("#checkout-shipping-address-SecondNodeWRT").text().trim() == '')) {
                            $(".checkout-shipping-address1").toggle(false);
                            $('.checkBoxIcon1').removeClass('fa fa-minus-square');
                            $('.checkBoxIcon1').addClass('fa fa-plus-square');
                        }
                        else {
                            $(".checkout-shipping-address1").toggle(!bool_hidden);
                            if (bool_hidden) {
                                $('.checkBoxIcon1').removeClass('fa fa-minus-square');
                                $('.checkBoxIcon1').addClass('fa fa-plus-square');
                            }
                            else {
                                $('.checkBoxIcon1').removeClass('fa fa-plus-square');
                                $('.checkBoxIcon1').addClass('fa fa-minus-square');
                            }
                        }
                    }
                    else {
                        if ($("#checkout-shipping-address-FirstNodeWRT").text().trim() == '') {
                            $("#checkout-shipping-address-FirstNodeWRT").toggle(false);
                            $('#checkBoxIconFirstNodeWRT').removeClass('fa fa-minus-square');
                            $('#checkBoxIconFirstNodeWRT').addClass('fa fa-plus-square');
                            $("#checkout-shipping-address-FirstNodeWRT").removeClass('d-none');
                            $("#checkout-shipping-address-FirstNodeWRT").addClass('d-none');
                            $("#idwrapper_c_1").removeClass('d-none');
                            $("#idwrapper_c_1").addClass('d-none');
                        }
                        else {
                            $("#checkout-shipping-address-FirstNodeWRT").toggle(!bool_hidden);
                            if (bool_hidden) {
                                $('#checkBoxIconFirstNodeWRT').removeClass('fa fa-minus-square');
                                $('#checkBoxIconFirstNodeWRT').addClass('fa fa-plus-square');
                            }
                            else {
                                $('#checkBoxIconFirstNodeWRT').removeClass('fa fa-plus-square');
                                $('#checkBoxIconFirstNodeWRT').addClass('fa fa-minus-square');
                            }
                        }

                        if ($("#checkout-shipping-address-SecondNodeWRT").text().trim() == '') {
                            $("#checkout-shipping-address-SecondNodeWRT").toggle(false);
                            $('#checkBoxIconSecondNodeWRT').removeClass('fa fa-minus-square');
                            $('#checkBoxIconSecondNodeWRT').addClass('fa fa-plus-square');
                            $("#checkout-shipping-address-SecondNodeWRT").removeClass('d-none');
                            $("#checkout-shipping-address-SecondNodeWRT").addClass('d-none');
                            $("#idwrapper_c_2").removeClass('d-none');
                            $("#idwrapper_c_2").addClass('d-none');
                        }
                        else {
                            $("#checkout-shipping-address-SecondNodeWRT").toggle(!bool_hidden);
                            if (bool_hidden) {
                                $('#checkBoxIconSecondNodeWRT').removeClass('fa fa-minus-square');
                                $('#checkBoxIconSecondNodeWRT').addClass('fa fa-plus-square');
                            }
                            else {
                                $('#checkBoxIconSecondNodeWRT').removeClass('fa fa-plus-square');
                                $('#checkBoxIconSecondNodeWRT').addClass('fa fa-minus-square');
                            }
                        }
                    }
                }
            }
        },
        error: function (response) {
        }
    });
}

// use as onclick = focusOnClick(this); or onmousedown event;
function focusOnClick(obj) {
    if ((obj)) {
        if (!$(obj).is(":focus")) obj.select();
    }
    return true;
}

setTimeout(function () {
    var chartHtml = $('.chart_wrapper').html();
    if ($('.chart_wrapper').find('b').length > 0) {
        var newchartHtml = chartHtml.replace('<b>', '').replace('</b>', '');
        $('.chart_wrapper').html(newchartHtml);
    }
    if (hdnpairwise_type == 'ptGraphical') {
        if ($('#noUiInput11').val() == '' && $('#noUiInput21').val() == '') {
            $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }
        else {
            $('.chart_wrapper .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }
    }
}, 100);

var leftval = 0;
var rightval = 0;
function save_multi_pairwise(chart, currentIndex, value, advantage, index, currentStep) {
    //$('.chart_wrapper').removeClass('active_row');
    //$('#index_' + index).addClass('active_row');
    var hdnLeftBar = eval($('#MainContent_hdnLeftBar').val());
    var hdnRightBar = eval($('#MainContent_hdnRightBar').val());
    $('.clsname_' + index).removeClass("hide");
    $('.clsname_' + index).addClass("hide");
    $("#divRatingEQ_e_" + index).removeClass("selcted_spot");

    if (chart === 'close') {
        $('#index_' + index + ' .imgClose').css({ 'filter': 'grayscale(1);', 'opacity': '0.5;' })
    }
    else {
        $('#index_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' });
    }

    if (chart == "close") {
        $('.left_result_' + index).css('display', 'none');
        $('.right_result_' + index).css('display', 'none');
        $('.equal_' + index).css('display', 'none');
        $("#divRatingEQ_e_" + index).removeClass("selected_left");
        $("#divRatingEQ_e_" + index).removeClass("selected_right");
        $("#divRatingEQ_e_" + index).removeClass("eqalizer_color");

        for (var j = 8; j >= 0; j--) {
            $("#show_" + index + "_" + j).removeClass("selected_left");
            $("#show_" + index + "_" + j).removeClass("selcted_spot");
        }
        for (var j = 0; j <= 8; j++) {
            $("#showr_" + index + "_" + j).removeClass("selected_right");
            $("#showr_" + index + "_" + j).removeClass("selcted_spot");
        }
    }
    if (chart == "left") {
        leftval = value;
        rightval = 0;
        $('.left_result_' + index).css('display', 'none');
        $('.right_result_' + index).css('display', 'none');
        $('#left_result_' + index).text(hdnLeftBar[currentIndex][1]);

        $("#divRatingEQ_e_" + index).addClass("selected_left");
        $("#divRatingEQ_e_" + index).removeClass("selected_right");
        $("#divRatingEQ_e_" + index).removeClass("eqalizer_color");

        $("#spotL" + index + "_" + value).removeClass("hide");

        //$('.left_verbal').removeClass("multiverbal_spotname");
        //$('.left_name_' + index).addClass("multiverbal_spotname")
        //$('.left_name_' + index).innerText('');
        //$('.left_name_' + index).innerText(hdnLeftBar[currentIndex][1]);

        $('.equal_' + index).css('display', 'none');
        for (var j = 8; j >= 0; j--) {
            $("#show_" + index + "_" + j).removeClass("selected_left");
            $("#show_" + index + "_" + j).removeClass("selcted_spot");
        }
        $("#divRatingEQ_r_" + index).removeClass("selected_right");
        for (var j = 0; j <= 8; j++) {
            $("#showr_" + index + "_" + j).removeClass("selected_right");
            $("#showr_" + index + "_" + j).removeClass("selcted_spot");
        }

        for (var j = 1; j <= value; j++) {
            $("#show_" + index + "_" + j).addClass("selected_left");
        }
        //if (value >= 0) {
        //    $("#divRatingEQ_l_" + index).css('display', 'block');
        //    $("#divRatingEQ_r_" + index).css('display', 'none');
        //    $("#divRatingEQ_l_" + index).addClass("selected_left");
        //    $("#divRatingEQ_e_" + index).removeClass("eqalizer_color");
        //    $("#divRatingEQ_e_" + index).addClass("selected_left");
        //    $('#divRatingEQ_e_' + index).css('display', 'block');
        //    $("#divRatingEQ_ee_" + index).removeClass("eqalizer_color");
        //    $("#divRatingEQ_ee_" + index).addClass("selected_left");
        //    $('#divRatingEQ_ee_' + index).css('display', 'block');

        //}

        $("#show_" + index + "_" + value).addClass("selcted_spot");
    }
    if (chart == "right") {
        leftval = 0;
        rightval = value;
        $('.left_result_' + index).css('display', 'none');
        $('.right_result_' + index).css('display', 'none');

        $('#right_result_' + index).text(hdnRightBar[currentIndex][1]);

        $("#divRatingEQ_e_" + index).removeClass("selected_left");
        $("#divRatingEQ_e_" + index).addClass("selected_right");
        $("#divRatingEQ_e_" + index).removeClass("eqalizer_color");

        $("#spotR" + index + "_" + currentIndex).removeClass("hide");
        //$("#spotR" + index + "_" + currentIndex).addClass("selcted_spot");

        //$("#divRatingEQ_l_" + index).removeClass("selected_left");
        //$("#divRatingEQ_e_" + index).css('display', 'none');
        //$("#divRatingEQ_re_" + index).css('display', 'none');
        //$("#divRatingEQ_r_" + index).css('display', 'block');
        //$("#divRatingEQ_r_" + index).removeClass("eqalizer_color");

        $('.equal_' + index).css('display', 'none');
        for (var j = 8; j >= 0; j--) {
            $("#show_" + index + "_" + j).removeClass("selected_left");
            $("#show_" + index + "_" + j).removeClass("selcted_spot");
        }
        $("#divRatingEQ_r_" + index).removeClass("selected_right");
        for (var j = 1; j <= 8; j++) {
            $("#showr_" + index + "_" + j).removeClass("selected_right");
            $("#showr_" + index + "_" + j).removeClass("selcted_spot");
        }
        for (var j = 0; j <= value; j++) {
            $("#showr_" + index + "_" + j).addClass("selected_right");
        }

        $("#showr_" + index + "_" + currentIndex).addClass("selcted_spot");
    }
    if (chart == "equal") {
        $('.left_result_' + index).css('display', 'none');
        $('.right_result_' + index).css('display', 'none');
        $('.equal_' + index).removeClass("hide");

        $("#divRatingEQ_e_" + index).removeClass("selected_left");
        $("#divRatingEQ_e_" + index).removeClass("selected_right");
        $("#divRatingEQ_e_" + index).addClass("eqalizer_color");

        for (var j = 8; j >= 0; j--) {
            $("#show_" + index + "_" + j).removeClass("selected_left");
            $("#show_" + index + "_" + j).removeClass("selcted_spot");
        }
        for (var j = 0; j <= 8; j++) {
            $("#showr_" + index + "_" + j).removeClass("selected_right");
            $("#showr_" + index + "_" + j).removeClass("selcted_spot");
        }

        $("#divRatingEQ_e_" + index).addClass("selcted_spot");
    }

    var multivalues = [];
    for (i = 0; i < hdnMultiPwData.length; i++) {
        try {
            var vals = [];
            if (i == index) {
                if (chart == "comment") {
                    vals[0] = ((leftval == 0 && rightval == 0) || (typeof leftval == 'undefined' && typeof rightval == 'undefined')) ? hdnMultiPwData[i].Value : (leftval == 0 ? rightval + 2 : leftval + 1); //-2147483648000
                    vals[1] = ((leftval == 0 && rightval == 0) || (typeof leftval == 'undefined' && typeof rightval == 'undefined')) ? hdnMultiPwData[i].Advantage : (leftval == 0 ? -1 : 1);
                    if (!isMobile)
                        vals[2] = $("#comment_" + currentIndex).val();
                    else
                        vals[2] = $("#txtRightComment").val();
                }
                else if (chart == "close") {
                    vals[0] = -2147483648000;
                    vals[1] = 0;
                    vals[2] = hdnMultiPwData[i].Comment;
                }
                else if (chart == "equal") {
                    vals[0] = 1;
                    vals[1] = 0;
                    vals[2] = hdnMultiPwData[i].Comment;
                }
                else {
                    vals[0] = (chart == "left") ? parseFloat(value == '' ? 0 : value) + 1 : parseFloat(value == '' ? 0 : value) + 2;
                    vals[1] = advantage;
                    vals[2] = hdnMultiPwData[i].Comment;
                }
            }
            else {
                vals[0] = hdnMultiPwData[i].Value;
                vals[1] = hdnMultiPwData[i].Advantage;
                vals[2] = hdnMultiPwData[i].Comment;
            }
            hdnMultiPwData[i].Value = vals[0];
            hdnMultiPwData[i].Advantage = vals[1];
            hdnMultiPwData[i].Comment = vals[2];
            multivalues[i] = vals;
        }
        catch (e) { }
    }
    if (chart == "comment") {
        HideShowComment_icon(currentIndex);
    }
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveMultiPairwiseData",
        data: JSON.stringify({
            step: hdnCurrentStep,
            multivalues: multivalues
        }),
        dataType: "json",
        //async: false,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            //toggleBox(index, chart);
            if (output.pipeOptions.dontAllowMissingJudgment) {
                var disablePage = false;
                if (hdnMultiPwData.length > 0) {
                    for (i = 0; i < hdnMultiPwData.length; i++) {
                        if (parseFloat(hdnMultiPwData[i].Value) <= 0) {
                            disablePage = true;
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
            }
            if (chart != "close")
                slideMultiPWVerbal(false);
        },
        error: function (response) {
        }
    });
    if ($('#toggleComment_' + currentIndex).css('display') !== "none") {
        $('#toggleComment_' + currentIndex).toggle();
    }
    if ($('#comment_mobile').css('display') == 'block') {
        $('#comment_mobile').modal('hide');
    }
}
$(document).ready(function () {
    /*  var selid = $.cookie('SelectionID');*/
    /*if ($.cookie('SelectionID') != null && hdnpage_type == 'atShowLocalResults'*//* && $.cookie('ShowJudgTable') == true*//*) {*/
    if ($.cookie('SelectionID') != null && hdnpage_type == 'atShowLocalResults' && $.cookie('judgementStep').toString() == output.current_step.toString()) {
        $.removeCookie('SelectionID');
        $.removeCookie('judgementStep');
        bindDynamicHtml('show_satisfactory');
        bindDynamicHtml('Judgment_table');
    }

});
function slideMultiPWVerbal(isLoad, scroll) {
    setTimeout(function () {
        var flag = false;
        var limit = 250;

        if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            $('.left_result').css('display', 'none');
            $('.right_result').css('display', 'none');
            $('.equal').css('display', 'none');
            limit = 180;
        }

        for (i = 0; i < hdnMultiPwData.length; i++) {
            if (i >= current_row) {
                if (parseFloat(hdnMultiPwData[i].Value) <= 0) {
                    //for (j = 0; j < hdnMultiPwData.length; j++) {
                    //    $('#index_' + j).removeClass('active_row');
                    //    $('#index_' + j).addClass('unselected_row');
                    //}
                    $('.chart_wrapper').removeClass('active_row');
                    $('.chart_wrapper').addClass('unselected_row');
                    $('#index_' + i).removeClass('unselected_row');
                    $('#index_' + i).addClass('active_row');
                    if (parseFloat(hdnMultiPwData[i].Value) == -2147483648000) {
                        $('#index_' + i + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                    }
                    else {
                        $('#index_' + i + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                    }
                    flag = true;
                }
            }
            if (flag)
                break;
        }
        if (!!$.cookie('GenActiveRowID')) {
            var index = $.cookie("GenActiveRowID");
            if (index != "-1") {
                $('#index_' + i).addClass('unselected_row');
                $('#index_' + i).removeClass('active_row');
                $('#index_' + index).removeClass('unselected_row');
                $('#index_' + index).addClass('active_row');
                i = index;
                $.cookie("GenActiveRowID", "-1");
            }
        }
        //if (!flag) {
        //    $('#index_' + current_row).addClass('active_row');
        //    $('#index_' + current_row).removeClass('unselected_row');
        //}
        var IsJudmentCompletet = true;
        if (!flag) {
            $('.chart_wrapper').removeClass('active_row').addClass('unselected_row');
            $('#index_' + current_row).removeClass('unselected_row');
            $('#index_' + current_row).addClass('active_row');
            for (i = 0; i < hdnMultiPwData.length; i++) {
                if (parseFloat(hdnMultiPwData[i].Value) <= 0) {
                    $('.chart_wrapper').removeClass('active_row').addClass('unselected_row');
                    $('#index_' + i).removeClass('unselected_row');
                    $('#index_' + i).addClass('active_row');
                    IsJudmentCompletet = false;
                    flag = true;
                    break;
                }
            }
            if (IsJudmentCompletet) {
                $('.chart_wrapper').removeClass('active_row').addClass('unselected_row');
                $('#index_' + current_row).removeClass('unselected_row');
                $('#index_' + current_row).addClass('active_row');
            }
        }
        if (flag) {
            if (i < hdnMultiPwData.length) {
                if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
                    $('.left_result_' + i).css('display', 'none');
                    $('.right_result_' + i).css('display', 'none');
                    $('.equal_' + i).css('display', 'none');
                    if (hdnMultiPwData[i].Advantage == 1)
                        $('.left_result_' + i).css('display', 'block');
                    if (hdnMultiPwData[i].Advantage == -1) {
                        var id = 8 - hdnMultiPwData[i].Value;
                        $('.right_result_' + id).css('display', 'block');
                    }
                    if (hdnMultiPwData[i].Advantage == 0 && hdnMultiPwData[i].Value == 1)
                        $('.equal_' + i).css('display', 'block');
                }
            }
        }


        if ((!flag && isLoad) || ($.cookie('SelectionID') != undefined && $.cookie('SelectionID') != null)) {
            for (i = 0; i < hdnMultiPwData.length; i++) {
                if (hdnMultiPwData[i].Advantage == 1)
                    $('.left_result_' + i).css('display', 'block');
                if (hdnMultiPwData[i].Advantage == -1) {
                    $('.right_result_' + i).css('display', 'block');
                    $("#gslider" + i + " .noUi-connect").css("background-color", "#5F93C1");
                    $("#right_result_" + i).css("background-color", "#b5d4a7");
                }
                if (hdnMultiPwData[i].Advantage == 0 && hdnMultiPwData[i].Value == 1) {
                    $('.equal_' + i).css('display', 'block');
                    $('.equal_' + i).addClass('equal_color');
                }
            }
            var selid = 0;
            if ($.cookie('SelectionID') == null) {
                selid = 0;
            }
            else {
                selid = $.cookie('SelectionID');
            }

            $('.chart_wrapper').removeClass('active_row').addClass('unselected_row');
            $('#index_' + selid).removeClass('unselected_row');
            $('#index_' + selid).addClass('active_row');
            $('.left_result_' + selid).css('display', 'none');
            $('.right_result_' + selid).css('display', 'none');
            $('.equal_' + selid).css('display', 'none');
            if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
                if (hdnMultiPwData[0].Advantage == 1) {
                    $('.left_result_' + selid).css('display', 'block');
                    $("#left_result_" + selid).css("background-color", "#0058a3");
                }
                if (hdnMultiPwData[0].Advantage == -1) {
                    $('.right_result_' + selid).css('display', 'block');
                    $("#right_result_" + selid).css("background-color", "#6aa84f");
                }
                if (hdnMultiPwData[0].Advantage == 0 && hdnMultiPwData[0].Value == 1) {
                    $('.equal_' + selid).css('display', 'block');
                    $('.equal_' + selid).addClass('equal_color');
                }
            }
        } else if (!flag && !isLoad) {

            if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
                if (active_multi_index > 0) {
                    $('.chart_wrapper').removeClass('active_row').addClass('unselected_row');
                    $('#index_' + active_multi_index).removeClass('unselected_row');
                    $('#index_' + active_multi_index).addClass('active_row');
                    $('.left_result_' + active_multi_index).css('display', 'none');
                    $('.right_result_' + active_multi_index).css('display', 'none');
                    $('.equal_' + active_multi_index).css('display', 'none');
                    if (hdnMultiPwData[active_multi_index].Advantage == 1) {
                        $('.left_result_' + active_multi_index).css('display', 'block');
                        $("#left_result_" + active_multi_index).css("background-color", "#0058a3");
                    }
                    if (hdnMultiPwData[active_multi_index].Advantage == -1) {
                        var id = 8 - hdnMultiPwData[i].Value;
                        $('.right_result_' + id).css('display', 'block');
                        $("#right_result_" + id).css("background-color", "#6aa84f");
                    }
                    if (hdnMultiPwData[active_multi_index].Advantage == 0 && hdnMultiPwData[active_multi_index].Value == 1) {
                        $('.equal_' + active_multi_index).css('display', 'block');
                        $('.equal_' + active_multi_index).removeClass('equal_color');
                    }
                }

            }
        }

        if (scroll != false) {
            var container = $('div.main_wrapper');
            container.scrollTop(0);
            if ($('.active_row').position().top > 350)
                container.scrollTop($('.active_row').position().top - 300);
            //$('html, body').animate({
            //    scrollTop: $('div#MainContent_AllPairWiseControl_dvContent').offset().top - 300
            //}, 'slow');
        }
    }, 2000);
}


var save_multi_pairwise_on_next = function () {
    var multivalues = [];
    if (multi_pw_data.length == 0) {
        multi_pw_data = eval(output.multi_pw_data);
    }
    for (i = 0; i < multi_pw_data.length; i++) {
        try {
            var vals = [];
            vals[0] = multi_data[i].Value;
            vals[1] = multi_data[i].Advantage;
            vals[2] = multi_data[i].Comment;
            multivalues[i] = vals;
        }
        catch (e) {

        }
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveMultiPairwiseData",
        async: true,
        data: JSON.stringify({
            step: hdnCurrentStep,
            multivalues: multivalues
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
        },
        error: function (response) {
        }
    });
}

function ShowModels() {
    if ($('#MainContent_hdnOutput').val() != '') {
        output = eval("(" + $('#MainContent_hdnOutput').val() + ")");
        var current_step = output.current_step;

        if (output.lockedInfo != undefined && output.lockedInfo != null && !output.lockedInfo.status) {
            if (output.isPipeViewOnly) {
                inconsistency = true;

                if (!output.isPipeStepFound) {
                    //$("#PipeStepNotFoundModal").foundation("reveal", "open");
                }
            }
            else {
                //when users comes to left judgement first time after login && judgment made > 0 && total pipe steps != 1 && first unassessed step != current step && (current step > 1 || (current step > 0 && first unassessed step > 0))
                if (output.is_first_time && parseInt(output.judgment_made) > 0 && parseInt(output.total_pipe_steps) != 1 && parseInt(output.first_unassessed_step) != parseInt(current_step) && (parseInt(current_step) > 1 || (parseInt(current_step) > 0 && parseInt(output.first_unassessed_step) > 0))) {
                    if (hdnCurrentStep == 1) {
                        $('#divfirst_step').css('display', 'none');
                    }
                    else {
                        $('#divfirst_step').css('display', 'block');
                    }
                    $("#MainContent_SelectStepModal").modal('show');
                    $('#divfirst_unassessed_step').hide();
                    $('#divfirst_step').hide();

                    $.removeCookie('hdnsortorder');
                    $.removeCookie('hdnsortparam');
                    $.removeCookie("hdnCookieProjName");

                    if (parseInt(output.first_unassessed_step) > 0) {
                        $('#divfirst_unassessed_step').show();
                    }
                    if (parseInt(current_step) > 1) {
                        $('#divfirst_step').show();
                    }
                }
            }

            if (output.show_auto_advance_modal) {
                $("#MainContent_AutoAdvanceModal").modal('show');
            }
        }
    }
}

function accesStep(type) {
    if (type != undefined && type != null && type != '') {
        //var activeIndex = '';
        if (type == 'first_unassessed_step') {
            //activeIndex = $('#MainContent_hdnfirst_unassessed_step').val();
            $('#hdnPageNumber').val(output.first_unassessed_step);
            setTimeout(function () {
                setTimeout(function () {
                    $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
            }, 10);
        }
        else if (type == 'current_step') {
            $("#MainContent_SelectStepModal").modal('hide');
        }
        else if (type == 'first_step') {
            activeIndex = '1';
            $('#hdnPageNumber').val(activeIndex);
            setTimeout(function () {
                $('#MainContent_hdnPageNo').trigger('click');
            }, 10);
        }
    }
}

function add_multivisPipeViewOnlyalues(value, advantage, index, event, isSelected, pairwise_type) {
    $(".next_step").removeClass("back-pulse");
    var flashTimeOut = value == "-2147483648000" ? 0 : 900;
    var isSelectedIndex = false;
    if (isSelected == "1")
        isSelectedIndex = true;
    if (value != "-2147483648000") {
        $timeout(function () {
            //adding flash css class on judgment line if it's selected line
            if (isSelectedIndex) {
                $(".multi-row-" + index).addClass("judgment-flash");
                $(".tt-verbal-bars-wrap.multi-loop.selected ul li").addClass("judgment-flash-li");
            }
        }, 25, false);
    }
    var active_multi_index = index;
    if (isSelectedIndex) {
        $timeout(function () {
            $(".multi-rows").removeClass("selected");
            $(".multi-row-" + index).addClass("selected");

            if (value != "-2147483648000") {
                //removing flash css class on judgment line
                $(".multi-row-" + index).removeClass("judgment-flash");
                $(".tt-verbal-bars-wrap.multi-loop.selected ul li").removeClass("judgment-flash-li");

                if (pairwise_type == "ptGraphical") {
                    $(".graph-green-div").addClass("others");
                    $("#gslider" + index + " .graph-green-div").removeClass("others");
                    $(".graph-green-div").width(zero);
                    var max = $(".noUi-base").width();
                    var zero = (max / 2);
                    $(".graph-green-div.others").width(zero);
                    var uibaseWidth = $("#gslider" + index).width();
                    $("#gslider" + index + " .graph-green-div").width((uibaseWidth / 2) - 1);
                    $(".graph-green-div.others").width((uibaseWidth / 2));
                }

                var activate_first_row = true;

                setTimeout(function () {

                    if (index + 2 > parseInt(pwLength)) {
                        active_multi_index = parseInt(pwLength) - 1;
                        $(".multi-row-" + active_multi_index).addClass("selected");
                        activate_first_row = true;
                    }
                    else {
                        $(".multi-row-" + (index + 1)).addClass("selected");

                        active_multi_index = index + 1;
                        activate_first_row = false;

                    }

                    if (active_multi_index > 0 || activate_first_row) {
                        // $location.hash("multi-row-" + $scope.active_multi_index);
                        //$anchorScroll();
                    }

                    active_multi_index = active_multi_index;
                    if (activate_first_row) {
                        //angular.element("html, body").animate({ scrollTop: 0 }, "slow");
                        angular.element(".next_step").addClass("back-pulse");
                        //var loadingMessage = stringResources.yellowLoadingIcon.multiSave;
                        //$scope.$parent.runProgressBar(2, 0, 100, loadingMessage, false);
                    }
                    else {
                        var scroll_to_index = active_multi_index - 2;
                        // //console.log($scope.scroll_to_index);
                        if (scroll_to_index <= 1) {
                            //do not scroll
                        }
                        else {
                            //$location.hash("multi-row-" + $scope.scroll_to_index);
                            //$anchorScroll();
                        }
                    }

                    // $scope.scroll_to_active_index();
                    // Foundation.libs.tooltip.hide(angular.element($scope.temp_tip));

                }, 300);
            }
        }, flashTimeOut, false);
    }
    else {
        //callResizeFrameImagesProportionally(100, "add_multivalues");
    }


    updateMultiPairwiseObjectValue(value, advantage, index, event);
}

function updateMultiPairwiseObjectValue(value, advantage, index, event) {
    ////console.log("updating... value: " + value + ", advantage: " + advantage);
    multi_pw_data[index].Value = parseFloat(value);
    multi_pw_data[index].Advantage = parseInt(advantage);
    multi_pw_data[index].Comment = $('#multi_comment_' + index).val();
    checkMultiValue(output.multi_pw_data);

    if (advantage == 0) {
        multi_pw_data[index].isUndefined = true;
    }
    else {
        multi_pw_data[index].isUndefined = false;
    }

    if (event) {
        event.stopPropagation();
    }
}

function checkMultiValue(multiData, fNonPw) {
    var isUndefined = false;

    for (i = 0; i < multiData.length; i++) {
        if (fNonPw == 1) {
            var value = multiData[i][0];
            if (value < 0 || value == "") {
                isUndefined = true;
            }
        }
        else if (fNonPw == 2) {
            var value = multiData[i];
            if (value < 0 || value == "") {
                isUndefined = true;
            }
        }
        else {
            if (multiData[i].Advantage == 0 && multiData[i].Value < 1) {
                isUndefined = true;
            }
        }
    }

    //this.$parent.disableNext = this.output.pipeOptions.dontAllowMissingJudgment && isUndefined;
    //this.output.IsUndefined = isUndefined;
    //this.$parent.output.IsUndefined = isUndefined;

    if (isUndefined) {
        $(".next_step").removeClass("back-pulse");
    }
    else {
        if (!$(".next_step").hasClass("back-pulse")) {
            $(".next_step").addClass("back-pulse");
            //console.log("added back-pulse");
        }
    }
}

function activeRow(id, flag) {
    for (i = 0; i < multi_pw_data.length; i++) {
        if ($('#toggleComment_' + i).css('display') !== 'none' && i !== id) {
            $('#toggleComment_' + i).css('display', 'none');
        }
        if ($('#parentnode3' + i).css('display') !== 'none' && i !== id) {
            $('#parentnode3' + i).css('display', 'none');
        }
    }

    if (hdnpairwise_type === 'ptGraphical') {
        if (($('#input1' + id).val() !== '' && parseInt($('#input1' + id).val()) > 0) || ($('#input2' + id).val() !== '' && parseInt($('#input2' + id).val()) > 0)) {
            $('#index_' + id + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }
        else
            $('#index_' + id + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
    }
    else if (hdnpairwise_type == 'ptVerbal') {
        if (output.multi_pw_data[id].Value == -2147483648000)
            $('#index_' + id + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        else
            $('#index_' + id + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
    }
    else {
        if (hdnMultiPwData[id].Value < 0) {
            $('#index_' + id + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }
        else {
            $('#index_' + id + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }
    }
    CurrrentSelectedRow = id;
    current_row = id;
    if (hdnpairwise_type == 'ptVerbal') {

        $('.chart_wrapper').removeClass('active_row');
        $('#index_' + id).removeClass('unselected_row');
        $('#index_' + id).addClass('active_row');
        if (hdnMultiPwData[id].Advantage == 1)
            $("#left_result_" + id + "").css("background-color", "#80acd1");

        if (hdnMultiPwData[id].Advantage == -1)
            $("#right_result_" + id + "").css("background-color", "#6aa84f");

        if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
            $('.left_result').css('display', 'none');
            $('.right_result').css('display', 'none');
            $('.equal').css('display', 'none');
            if (hdnMultiPwData[id].Advantage == 1) {
                $('.left_result_' + id).css('display', 'block');
                $("#left_result_" + id + "").css("background-color", "#0058a3");
            }
            if (hdnMultiPwData[id].Advantage == -1) {
                $('.right_result_' + id).css('display', 'block');
            }
            if (hdnMultiPwData[id].Advantage == 0 && hdnMultiPwData[id].Value == 1) {
                $('.equal_' + id).css('display', 'block');
                $('.equal_' + id).removeClass('equal_color');
            }
        }
        else {
            for (i = 0; i < hdnMultiPwData.length; i++) {
                if (hdnMultiPwData[i].Advantage == 1)
                    $('.left_result_' + i).css('display', 'block');
                if (hdnMultiPwData[i].Advantage == -1) {
                    $('.right_result_' + i).css('display', 'block');
                    $("#gslider" + i + " .noUi-connect").css("background-color", "#5F93C1");
                    $("#right_result_" + i).css("background-color", "#b5d4a7");
                }
                if (hdnMultiPwData[i].Advantage == 0 && hdnMultiPwData[i].Value == 1) {
                    $('.equal_' + i).css('display', 'block');
                    $('.equal_' + i).addClass('equal_color');

                }
            }
            $('.left_result_' + id).css('display', 'none');
            $('.right_result_' + id).css('display', 'none');
            $('.equal_' + id).css('display', 'none');
        }
        // slideMultiPWVerbal();
    }
    else {
        $('#comdiv' + id).removeAttr('readonly');
        localStorage.setItem("currentId", id);
        for (i = 0; i < multi_pw_data.length; i++) {
            if (left_input[i] != "" && right_input[i] != "") {
                $("#gslider" + i + " .noUi-connect").css("background-color", "#5F93C1");
                $("#gslider" + i + " .graph-green-div").css("background-color", "#b5d4a7");
            }

        }
        // slideScroll();
    }
    if (flag) {
        if (left_input[id] != "" && right_input[id] != "") {
            $("#gslider" + id + " .noUi-connect").css("background-color", "#0058a3");
            $("#gslider" + id + " .graph-green-div").css("background-color", "#6aa84f");
        }
        if (hdnpairwise_type != "ptGraphical") {
            $('.chart_wrapper').removeClass('active_row');
        }
        else {
            $('.chart_wrapper').removeClass('active_row');
            $('.chart_wrapper').addClass('active_row');
        }
        for (i = 0; i < multi_pw_data.length; i++) {
            $('#index_' + i).removeClass("active_row");
            $('#index_' + i).addClass("unselected_row");
        }
        $('#index_' + id).removeClass('unselected_row');
        $('#index_' + id).addClass('active_row');
    }
    var container = $('div.main_wrapper');
    container.scrollTop(0);
    if ($('.active_row').position().top > 350)
        container.scrollTop($('.active_row').position().top - 300);
}

$('.checkEnter').on("keypress", function (e) {
    if (e.keyCode == 13) {
        slideScroll(false);
    }
});

function slideScroll(isLoad, scroll) {
    setTimeout(function () {
        var tempMultiPairData = [];
        tempMultiPairData = multi_pw_data;
        var pflag = false;
        var active_multi_index = $('div.active_row').data("val");

        for (i = 0; i < tempMultiPairData.length; i++) {
            if (left_input[i] != "" && right_input[i] != "") {
                $("#gslider" + i + " .noUi-connect").css("background-color", "#5F93C1");
                $("#gslider" + i + " .graph-green-div").css("background-color", "#b5d4a7");
            }

        }
        if (isLoad)
            current_row = 0;
        var currentIndex = active_multi_index;
        for (i = 0; i < multi_pw_data.length; i++) {
            if (i >= current_row) {
                if (left_input[i] == "" && right_input[i] == "") {
                    $('.chart_wrapper').removeClass('active_row');
                    $('#index_' + i).removeClass('unselected_row');
                    $('#index_' + i).addClass('active_row');
                    pflag = true;
                    currentIndex = i;
                    break;
                }
                else {
                    if (left_input[i] == "" && right_input[i] == "") {
                        if (j != i) {
                            for (j = 0; j < multi_pw_data.length; j++) {
                                $('#index_' + j).removeClass('active_row');
                                $('#index_' + j).addClass('unselected_row');
                            }
                        }
                    }
                }
            }
            if (pflag)
                return false;
        }
        if (!!$.cookie('GenActiveRowID')) {
            var InfoDocSaveRow = $.cookie("GenActiveRowID");
            if (InfoDocSaveRow != "-1") {
                $.cookie("GenActiveRowID", "-1");
                currentIndex = InfoDocSaveRow;
            }
        }
        if (!pflag) {
            for (i = 0; i < multi_pw_data.length; i++) {
                if (left_input[i] == "" && right_input[i] == "") {
                    $('.chart_wrapper').removeClass('active_row');
                    $('#index_' + i).removeClass('unselected_row');
                    $('#index_' + i).addClass('active_row');
                    pflag = true;
                    currentIndex = i;
                    break;
                }
                if (pflag)
                    return false;
            }
        }
        if (pflag) {
            for (j = 0; j < multi_pw_data.length; j++) {
                $('#index_' + j).removeClass('active_row');
                $('#index_' + j).addClass('unselected_row');
            }
            $('.chart_wrapper').removeClass('active_row');
            $('#index_' + currentIndex).removeClass('unselected_row');
            $('#index_' + currentIndex).addClass('active_row');
            if (left_input[currentIndex] != "" && right_input[currentIndex] != "") {
                $("#gslider" + id + " .noUi-connect").css("background-color", "#0058a3");
                $("#gslider" + currentIndex + " .graph-green-div").css("background-color", "#6aa84f");
            }
        }
        //if (!pflag) {
        //    var index = multi_pw_data.length - 1;
        //    $('#index_' + index).addClass('active_row');
        //    if (left_input[index] != "" && right_input[index] != "") {
        //        $("#gslider" + index + " .graph-green-div").css("background-color", "#6aa84f");
        //    }
        //}

        if ((!pflag && isLoad) || isLoad) {
            var selid = $.cookie('SelectionID');
            if ($.cookie('SelectionID') == null) {
                selid = 0;
                if (currentIndex > -1) { selid = currentIndex; }
            }
            if (currentIndex > -1 && !pflag && isLoad) {
                selid = currentIndex;
            }
            for (j = 0; j < multi_pw_data.length; j++) {
                $('#index_' + j).removeClass('active_row');
                $('#index_' + j).addClass('unselected_row');
            }
            $('.chart_wrapper').removeClass('active_row');
            $('#index_' + selid).removeClass('unselected_row');
            $('#index_' + selid).addClass('active_row');
            activeRow(selid, true);
            //$("#gslider0 .noUi-connect").css("background-color", "#0058a3");
            //$("#gslider0 .graph-green-div").css("background-color", "#6aa84f");
        }
        else if (!pflag && !isLoad) {
            if (left_input[currentIndex] != "" && right_input[currentIndex] != "") {
                // $('#index_' + currentIndex).addClass('active_row');
                $("#gslider" + currentIndex + " .noUi-connect").css("background-color", "#0058a3");
                $("#gslider" + currentIndex + " .graph-green-div").css("background-color", "#6aa84f");
            }
        }

        if ($('#input1' + currentIndex).val() == '' && $('#input2' + currentIndex).val() == '') {
            $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }

        if (scroll != false) {
            var container = $('div.main_wrapper');
            container.scrollTop(0);
            if ($('.active_row').position().top > 300)
                container.scrollTop($('.active_row').position().top - 300);
        }
    }, 2000);
}
function SetJudgmentTableEditable(IsEdited) {
    if (IsEdited) {
        $('.divReviewjudgment').addClass('d-none');
        /* $('.divMakeChanges').removeClass('d-none');*/
        $('.matrix-cell').prop('readonly', false);
        /*    $('.divRank_BestFit').addClass('d-none');*/
        //$('.spnBestFit').removeClass('d-none');
        //$('.spnBestFit').addClass('d-none');
        //$('.spnRank').removeClass('d-none');
        //$('.spnRank').addClass('d-none');
        //$('#chkBestFit').prop('checked', false);
        //$('#chkRank').prop('checked', false);
    }
    else {
        $('.divReviewjudgment').removeClass('d-none');
        /*   $('.divMakeChanges').addClass('d-none');*/
        /*   $('.divRank_BestFit').removeClass('d-none');*/
        $('.matrix-cell').prop('readonly', true);
    }
}
function bindDynamicHtml(val) {
    val = val == undefined || null ? '' : val;
    var html = '';
    $("#divIntermediateResults").removeClass('d-none');
    $("#divIntermediateResults").addClass('d-none');
    $("#divshow_satisfactory").removeClass('d-none');
    $("#divshow_satisfactory").addClass('d-none');
    $("#divredo_judgement").removeClass('d-none');
    $("#divredo_judgement").addClass('d-none');
    $("#divJudgment_table").removeClass('d-none');
    $("#divJudgment_table").addClass('d-none');
    if (val == 'cancel') {
        RemoveLocalResultsPageClass();
    }
    //if (IsLocalResultClassAdded) {
    //    SetDefaultCssClass();
    //}
    if (val == 'show_satisfactory') {
        $("#divshow_satisfactory").removeClass('d-none');
    }
    else if (val == 'Judgment_table') {
        saveOBPriority('False');
        $("#divJudgment_table").removeClass('d-none');
        var isChecked = $('#changejudgment').prop('checked');
        SetJudgmentTableEditable(isChecked);
    }
    else if (val == 'redo_judgement') {
        $("#divredo_judgement").removeClass('d-none');
        ChangeClassLocalResultsPage();
    }
    else {
        $("#divIntermediateResults").removeClass('d-none');
    }
    if (output.PipeParameters.MeasurementType === 'ptVerbal') {
        $('#divRedoJudg').removeClass('col-lg-12');
        $('#divRedoJudg').addClass('col-lg-10');
    }
    else {
        $('#divlegent_box').addClass('d-none');
        $('#divChkLegend').addClass('d-none');
    }

}

//Save chnaged page number
function reviewJudgment() {
    block_unload_prompt = true;
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/reviewJudgment",
        data: JSON.stringify({
            parentnodeID: output.parentnodeID,
            current_step: output.current_step
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var output = JSON.parse(data.d);
            $('#hdnPageNumber').val(output);
            setTimeout(function () {
                $('#MainContent_hdnPageNo').trigger('click');
            }, 10);
        },
        error: function (response) {
        }
    });
}

function checkGPtoVerbal(data) {
    var gptoVerbal = false;
    if (!gptoVerbal) {
        gptoVerbal = data.Value % 1 > 0 ? gptoVerbal = true : false;
    }
}

function LocalselectCheckbox(is_checked, node_name) {
    if ($('#' + is_checked).is(':checked')) {
        checked_boxes++;
        node_names.push(node_name);
    }
    else {
        checked_boxes--;
        var index = node_names.indexOf(node_name);
        node_names.splice(index, 1);
    }
    if (checked_boxes == 2) {
        $(".redoCheckbox").each(function () {
            var chkId = this.id;
            if (!$('#' + chkId).is(':checked')) {
                $('#' + chkId).prop('disabled', true);
            }
        });
        $('#btnReevaluate').prop('disabled', false);
    }
    else {
        $(".redoCheckbox").each(function () {
            var chkId = this.id;
            if (!$('#' + chkId).is(':checked')) {
                $('#' + chkId).prop('disabled', false);
            }
        });
        $('#btnReevaluate').prop('disabled', true);
    }
}

function reEvaluate(parentnodeID, current_step) {
    $.cookie('currentstep', current_step);
    $.cookie('is_under_review', true);

    if (parseInt(checked_boxes) == 2) {
        block_unload_prompt = true;
        var addstep = false;
        if (getStepNumber(node_names[0], node_names[1]) < 0)
            addstep = true;

        $.cookie('AllowAddStep', addstep);

        $.ajax({
            type: "POST",
            url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/redoPairs",
            data: JSON.stringify({
                parentnodeID: parentnodeID,
                current_step: current_step,
                firstNode: node_names[0],
                secondNode: node_names[1],
                is_name: false,
                add_step: addstep
            }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var step = data.d;
                //$scope.$parent.is_under_review = true;
                //$scope.matrixReview = false;
                if (step == current_step)
                    addstep = true;
                if (addstep) {
                    //$scope.load_step_list();
                    //$scope.previous_cluster_step = current_step + 1;
                    //$scope.get_pipe_data(current_step, $scope.is_AT_owner);
                    /*$('#hdnPageNumber').val(parseInt(current_step) + 1);*/
                    $('#hdnPageNumber').val(parseInt(current_step));
                    setTimeout(function () {
                        $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                    return true;
                }

                if (!addstep && $.isNumeric(step)) {
                    $.cookie('SelectionID', (step < 0 ? Math.abs(step) : step));
                    $.cookie('callResultTable', 'true');
                }

                if (step <= 0) {
                    //$scope.active_multi_index = Math.abs(data.d);                    
                    step = JSON.stringify(parseInt(JSON.parse(output.PipeParameters.StepPairsData)[0][0]) + 1);
                }

                $('#hdnPageNumber').val(step);
                setTimeout(function () {
                    $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
            },
            error: function (response) {
            }
        });
    }
}

function getStepNumber(id1, id2) {
    var retVal = -1;
    var o1 = parseInt(id1);
    var o2 = parseInt(id2);
    var step_data, pwn_sd;
    //step_data = eval($('#MainContent_hdnStepPairsData').val());
    step_data = eval(output.PipeParameters.StepPairsData);
    if (step_data) {
        for (var i = 0; i < step_data.length; i++) {
            pwn_sd = step_data[i];
            if ((((parseInt(pwn_sd[2]) == o1) && (parseInt(pwn_sd[3]) == o2)) || ((parseInt(pwn_sd[2]) == o2) && (parseInt(pwn_sd[3]) == o1))) && parseInt(pwn_sd[0]) > 0) {
                retVal = 1;
                break;
            }
        }
    }
    return retVal;
}

function SetCollapsedCookies(node_type, output) {
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SetCollapsedCookies",
        data: JSON.stringify({
            node_type: node_type,
            output: output
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
        },
        error: function (response) {
        }
    });
}

function setInfodocParams(node_type, output) {
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfodocParams",
        data: JSON.stringify({
            node_type: node_type,
            output: output
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
        },
        error: function (response) {
        }
    });
}

function update_infodoc_params_new(node_type, node_guid, wrt_guid, heading_id) {

    if (is_multi) {
        setTimeout(function () {
            //CheckMultiHeight("tt-homepage-main-wrap", "40");
            //CheckMultiHeight("multi-loop-wrap", "80");

            //CheckMultiHeight("multi-loop-wrap-local-results", "500");
            //CheckMultiHeight("tt-pipe-canvas", "152");
            //$scope.CheckMultiHeight("tt-rating-scale", "100");
        }, 800);
        //return false;
    }

    //        alert(node_guid);
    var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;

    //`c=1&w=300&h=200`
    var width = $("." + node_type + "-info-div > .tt-panel").width();
    var height = $("." + node_type + "-info-div > .tt-panel").height();
    var params = "c=" + is_hidden + "&w=" + width + "&h=" + height;
    var pair_params = params; //pass this w/o 't'
    if (typeof (heading_id) != "undefined") {
        is_hidden = is_hidden == 1 ? 0 : 1;
        params = "c=" + is_hidden + "&w=" + width + "&h=" + height;
        if ($("#" + heading_id).val() != "") {
            params = params + "&t=" + $("#" + heading_id).val();
        }
        else {
            params = params + "&t=-2";
        }
    }
    else {
        heading_id = "";
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfodocParams",
        data: JSON.stringify({
            NodeID: node_guid,
            WrtNodeID: wrt_guid,
            value: params,
            is_multi: is_multi,
            NodeType: node_type
        }),
        async: false,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $(".tt-accordion-content").attr("style", "");
            if (pair_indeces.length > 0 && heading_id == "") {
                $.each(pair_indeces, function (index, pair_index) {
                    $.ajax({
                        type: "POST",
                        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfodocParams",
                        data: JSON.stringify({
                            NodeID: node_guids[index],
                            WrtNodeID: wrt_guids[index],
                            value: pair_params,
                            is_multi: is_multi,
                            NodeType: ''
                        }),
                        async: false,
                        dataType: "json",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {

                        },
                        error: function (response) {
                        }
                    });
                });
            }
        },
        error: function (response) {
        }
    });
}
function SetButtonEnableOrDisable(chb) {
    IsShortByPriority = chb;
    if (chb == "True") {
        $("#SortbyOriginalOrder").removeClass("disabled");
        $("#SortByPriority").addClass("disabled");

    }
    else {
        $("#SortbyOriginalOrder").addClass("disabled");
        $("#SortByPriority").addClass("disabled");
        $("#SortByPriority").removeClass("disabled");
    }
}
//   .change
//$('#msortbypriority').click(function ()
function msortbypriority(mevent) {

    var check = $('#sbpriority').is(':checked');
    if (check) {
        saveOBPriority('True');
    }
    else
        saveOBPriority('False');
}

//$('.judgeHeader').click(function () {
//    debugger
//    $(".judgeHeader").each(function () {
//        this.removeClass('activeHeader')
//    });
//    this.addClass('activeHeader');
//});

var IsShortByPriority = false;
function saveOBPriority(chb, value) {
    if (value === 0) {
        IsShortByPriority = false;
    }

    $(".judgeHeader").each(function () {
        $(".judgeHeader").removeClass('activeHeader');
    });
    if (value == 1)
        $(".judgeHeader1").addClass('activeHeader');
    else if (value == 2)
        $(".judgeHeader2").addClass('activeHeader');
    $('#idropdown').css('display', 'none');
    SetButtonEnableOrDisable(chb);
    block_unload_prompt = true;
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/saveOBPriority",
        data: JSON.stringify({
            chb: chb
        }),
        async: false,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            if (data != undefined && data != null && data != '') {
                ClipBoardData = data.d.ClipBoardData;
                var html = '';
                var ObjectivesData = JSON.parse(data.d.ObjectivesData.replace(/'/g, '"'));
                html += '<table class="table table-bordered">';
                html += '<thead>';
                html += '<tr>';
                html += '<th></th>';
                for (i = 0; i < ObjectivesData.length; i++) {
                    var item = ObjectivesData[i];
                    if (item[2].length > 20) {
                        html += '<th scope="col" title="' + item[2] + '" class="">' + item[2].substring(0, 17) + '...' + '</th>';
                    }
                    else {
                        html += '<th scope="col" title="' + item[2] + '"  class="">' + item[2] + '</th>';
                    }
                }
                html += '</tr>';
                html += '</thead>';

                html += '<tbody>';
                for (i = 0; i < ObjectivesData.length; i++) {
                    var objective = ObjectivesData[i];
                    html += '<tr>';

                    html += '<td class="table-data-wrapper">';
                    html += '<div class="table-data-info"><div class="table-progress-data-info">';
                    html += '<div class="progress-data">';
                    if (objective[2].length > 24) {
                        html += '<span class="progress-data-info">' + objective[2].substring(0, 17) + '... - ' + objective[3] + '</span>';
                    }
                    else {
                        html += '<span class="progress-data-info">' + objective[2] + ' - ' + objective[3] + '</span>';
                    }
                    html += '</div>';
                    var barlength = (parseFloat(objective[1]) / parseFloat(data.d.highest_result)) * 100;
                    html += '<div class="progress"><div class="progress-bar bg_blue" role="progressbar" style="width:' + barlength + '%" aria-valuenow="' + barlength + '" aria-valuemin="0" aria-valuemax="100"></div></div>';
                    html += '</div></div>';
                    html += '</td>';

                    //html += '<td class="table-data-wrapper">< div class="table-data-info bg-col-gray" > </div ></td >';

                    for (j = 0; j < ObjectivesData.length; j++) {
                        var result = ObjectivesData[j];
                        html += '<td class="table-data-wrapper"> <div class="table-data-info">';
                        if (i < j && objective[1] != result[0]) {
                            var StepPairsData = JSON.parse(data.d.StepPairsData.replace(/'/g, '"'));
                            //if (StepPairsData.length < 1)
                            //{
                            if ((StepPairsData.indexOf(result[0]) == -1) && StepPairsData.indexOf(objective[0]) == -1) {
                                html += '<div style="cursor: pointer;" class="pwnl-click" data-pair-left="' + objective[0] + '" data-pair-right="' + result[0] + '">&nbsp;</div>';
                                /* html += '<div class="bestfit-value-info d-none" style="color:' + pcolor + '"></div>';*/
                            }
                            /* } */
                            if (i >= j) {
                                html += '<div class="table-data-info bg-col-gray"></div>';
                            }
                            else {
                                /*   html += '<div class="table-data-info"></div>';*/
                            }
                            /*        html += '<div class="check-select-values-info">';*/
                            for (k = 0; k < StepPairsData.length; k++) {
                                var pair = StepPairsData[k];
                                var color = 'black';
                                if (parseFloat(pair[1]).toFixed(2) == 8.00) {
                                    var checkkkk = 0;
                                }
                                if (pair[2] == objective[0]) {
                                    if (pair[4] == -1) color = "red"; else color = "black";
                                }
                                else {
                                    if (pair[2] == result[0] && pair[4] <= 0)
                                        color = "black"; else color = "red";
                                }
                                var pcolor = (pair[6] < 0) ? "red" : "green";
                                /* var pcolor = (pair[6] < 0) ? "red" : "black";*/
                                var rank = (pair[7] == 0) ? '' : pair[7];
                                var bestfit = pair[1] == 0 ? '' : ((pair[5] == 0) ? '' : parseFloat(pair[5]).toFixed(2));

                                if ((pair[2] == objective[0] && pair[3] == result[0]) || (pair[3] == objective[0] && pair[2] == result[0])) {
                                    html += '<div class="divMakeChanges check-input-data-info ">';
                                    if (parseFloat(pair[1]) > 0) {
                                        html += '<input type="text" class="matrix-cell form-control" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + '; cursor: pointer;" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="' + parseFloat(pair[1]).toFixed(2) + '">';
                                        //html += '<input type"text" Class="matrix-cell w-100 form-contro" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + '; cursor: pointer;" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="' + parseFloat(pair[1]).toFixed(2) + '">';

                                        html += '</div>';

                                        html += '<div class="check-select-values-info" data-pair-left="' + pair[2] + '" data-pair-right="' + pair[3] + '">';
                                        /*  html += '<div style="cursor: pointer;" class="pwnl-click" data-pair-left="' + pair[2] + '" data-pair-right="' + pair[3] + '"></div>';*/
                                        html += '<div class="rank-value-info d-none divRank_BestFit"><span class="v-info spnRank"> ' + rank + '</span> </div>';
                                        html += '<div class="bestfit-value-info d-none spnBestFit" style="color:' + pcolor + '"> <span class="v-info"> ' + bestfit + '</span> </div>';
                                        if (parseFloat(pair[1]) > 0) {
                                            html += '<div class="reevaluate-value-info d-none"> <span class="v-info"> ' + parseFloat(pair[1]).toFixed(2) + '</span> </div>';
                                        }
                                        else {
                                            html += '';
                                        }

                                    }
                                    else {
                                        html += '<input type="text" class="matrix-cell form-control" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + '; cursor: pointer;" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="">';
                                        html += '</div>';
                                        html += '<div class="check-select-values-info" data-pair-left="' + pair[2] + '" data-pair-right="' + pair[3] + '">';
                                        /*  html += '<div style="cursor: pointer;" class="pwnl-click" data-pair-left="' + pair[2] + '" data-pair-right="' + pair[3] + '"></div>';*/
                                        html += '<div class="rank-value-info d-none divRank_BestFit"><span class="v-info spnRank"> ' + rank + '</span> </div>';
                                        html += '<div class="bestfit-value-info d-none spnBestFit" style="color:' + pcolor + '"> <span class="v-info"> ' + bestfit + '</span> </div>';
                                        if (parseFloat(pair[1]) > 0) {
                                            html += '<div class="reevaluate-value-info d-none"> <span class="v-info"> ' + parseFloat(pair[1]).toFixed(2) + '</span> </div>';
                                        }
                                        else {
                                            html += '';
                                        }
                                    }
                                    html += '</div>';
                                    //else {
                                    //    html += '<input type="text" class="matrix-cell form-control" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + ';" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="">';
                                    //}


                                    //html += '<div class="divMakeChanges check-input-data-info ">';
                                    //if (parseFloat(pair[1]) > 0) {
                                    //    html += '<input type="text" class="matrix-cell form-control" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + '; cursor: pointer;" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="' + parseFloat(pair[1]).toFixed(2) + '">';
                                    //    //html += '<input type"text" Class="matrix-cell w-100 form-contro" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + '; cursor: pointer;" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="' + parseFloat(pair[1]).toFixed(2) + '">';
                                    //}
                                    //else {
                                    //    html += '<input type="text" class="matrix-cell form-control" data-value="' + pair[1] + '" data-advantage="' + pair[4] + '" id="cell' + pair[2] + '' + pair[3] + '" style="color : ' + color + ';" data-index="' + k + '" data-operation="1" data-pairs="' + pair + '" value="">';
                                    //}
                                    //html += '</div>';
                                }
                            }
                            html += '</div>';

                            //html += '</div></div>';
                        }
                        else {
                            if (i >= j) {
                                html += '<div class="table-data-info bg-col-gray"></div>';
                            }
                            else {
                                /* html += '<div class="table-data-info"></div>';*/
                            }
                        }
                        html += '</div>';
                        html += '</td>';
                    }
                    html += '</tr>';
                }
                html += '</tbody>';
                html += '</table>';
                //console.log(html);
                //$('#tblJugement tbody').empty();
                $('#divJugement').html('');
                $('#divJugement').html(html);
                $('.spnInconsistencyRatio').text(parseFloat(data.d.InconsistencyRatio.toFixed(2)));
            }
            var isChecked = $('#changejudgment').prop('checked');
            SetJudgmentTableEditable(isChecked);
            if ($('#reviewjudgment').is(':checked')) {
                $('input[id="reviewjudgment"]').trigger('change');
            }
            else {
                $('input[id="changejudgment"]').trigger('change');
            }
            if (isChecked) { $('.pwnl-click').addClass('d-none'); }
            if ($('#chkBestFit').is(':checked')) {
                $('.bestfit-value-info').removeClass('d-none');
            }
            else {
                $('.bestfit-value-info').addClass('d-none');
            }
            if ($('#chkRank').is(':checked')) {
                $('.rank-value-info').removeClass('d-none');
            }
            else {
                $('.rank-value-info').addClass('d-none');
            }
        },
        error: function (response) {
        }
    });
}

function showEyeDropDown() {
    $(".judgeHeader").each(function () {
        $(".judgeHeader").removeClass('activeHeader')
    });
    $(".judgeHeader6").addClass('activeHeader');
    $('#idropdown').toggle();
}

var MatrixOperation = function (type) {
    isShortByPriority = true;
    $('#idropdown').css('display', 'none');
    $(".judgeHeader").each(function () {
        $(".judgeHeader").removeClass('activeHeader')
    });
    if (type != undefined && type != null && type.trim() != '') {
        if (type == 'invertThis') {
            $(".judgeHeader5").addClass('activeHeader');
            doMatrixOperation('-1', '3', '', output.parentnodeID, '', 5);
        }
        else if (type == 'invert') {
            $(".judgeHeader8").addClass('activeHeader');
            IsShortByPriority = false;
            doMatrixOperation('-1', '5', '', output.parentnodeID, '', 8);
        }
        else if (type == 'invertCurrent') {
            $(".judgeHeader7").addClass('activeHeader');
            if (textField != null) {
                var judgementId = $(textField)[0].id.substring(4);
            }
            doMatrixOperation('-1', '7', '', output.parentnodeID, judgementId, 7);
        }
    }
}

function doMatrixOperation(judgment, operationID, content, parentnodeID, judgementId, e, blur, id) {
    block_unload_prompt = true;
    content = content == '' ? null : content;
    var normalization_list = [{ "value": 1, "text": "Normalized" }, { "value": 2, "text": "% of Maximum" }, { "value": 3, "text": "Unnormalized" }];
    var normalization = normalization_list[0];
    var fInvert = false;
    var judgmentText = judgment;

    if (parseInt(operationID) === 5) {
        var pass = window.confirm("Are you sure you want to invert all of your judgments?");
        if (!pass)
            return true;
    }

    //if (operationID === 1 && operationID === 3)
    //    $scope.$parent.runProgressBar(2, 100, 100, stringResources.yellowLoadingIcon.save, false);


    if (e != null && operationID != 7) {
        var code = e.which;
        if (code === 109 || code === 189 || code === 45 || code === 73 || code === 173 || e === 8 || e === 5) {

            fInvert = true;
            try {
                if (judgment.includes("-") || judgment.includes("i")) {
                    judgment = judgment.replace("-", "").replace("i", "");
                }
            }
            catch (e) {
                if (judgment.indexOf("-") != -1 || judgment.indexOf("i") != -1) {
                    judgment = judgment.replace("-", "").replace("i", "");
                }
            }
        }
        else if (code === 13) {
            fInvert = false;
        }
        else {
            return false;
        }
    }
    else {
        if (blur != null) {
            var regex = /^\d+(\.\d{0,2})?$/;

            try {
                if (judgment.includes("i") || judgment.includes("-")) {
                    return false;
                }
            }
            catch (e) {
                if (judgment.indexOf("-") != -1 || judgment.indexOf("i") != -1) {
                    return false;
                }
            }

            if (!(regex.test(judgment))) {
                saveOBPriority('False');
                return false;
            }
        }
    }

    if (judgment != undefined && judgment != null) {
        $.ajax({
            type: "POST",
            url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/doMatrixOperation",
            data: JSON.stringify({
                judgment: judgment,
                ID: operationID,
                content: content,
                parent: parentnodeID,
                invert: fInvert,
                rmode: normalization.value,
                judgementId: judgementId
            }),
            async: false,
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data != undefined && data != null && data != '') {
                    if (operationID == 1) {
                        output.PipeParameters.JudgmentsSaved = true;
                        $('#btnRestoreJudgment').prop('disabled', false);
                        var output_New = data.d;
                        matrixCellIDs.push("#cell" + output_New.ObjID);
                        output.PipeParameters.ObjectivesData = eval(output_New.results.ObjectivesData);
                        output.PipeParameters.highest_result = output_New.results.highest_result;
                        output.PipeParameters.StepPairsData = eval(output_New.results.StepPairsData);
                        output.PipeParameters.InconsistencyRatio = output_New.results.InconsistencyRatio;
                        output.PipeParameters.results_data = output_New.results.results_data;
                        /* saveOBPriority('False');*/
                        saveOBPriority(IsShortByPriority);
                    }
                    else {
                        matrixCellIDs = [];
                        var output_New = data.d.PipeParameters;
                        output.PipeParameters.JudgmentsSaved = output_New.JudgmentsSaved;
                        $('#btnRestoreJudgment').prop('disabled', !output_New.JudgmentsSaved);
                        output.PipeParameters.ObjectivesData = eval(output_New.ObjectivesData);
                        output.PipeParameters.highestResult = output_New.highest_result;
                        output.PipeParameters.StepPairsData = eval(output_New.StepPairsData);
                        output.PipeParameters.InconsistencyRatio = output_New.InconsistencyRatio;
                        output.PipeParameters.results_data = output_New.results_data;
                        saveOBPriority(IsShortByPriority);
                        /* saveOBPriority('False');*/
                        glowMatrix(true);
                    }
                }
                $(".judgeHeader" + id).addClass('activeHeader');
            },
            error: function (response) {
            }
        });
    }
}

$(document).on("keyup", ".matrix-cell", function (e) {
    var value = $(this).val();
    var index = $(this).data("index");
    var operationID = $(this).data("operation");
    var pairs = eval("[" + $(this).data("pairs") + "]");
    checkKeyPress(value, operationID, pairs, output.parentnodeID, e);
})

var iindex, iwidth, iheight;
function set_infodoc_sizes(width, height, index) {

    var is_multi = false; // is_multi;
    iindex = index;
    iwidth = width;
    iheight = height;

    if (output.page_type != "atPairwise") {
        is_multi = true;
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfoDocSizes",
        data: JSON.stringify({
            index: index,
            width: width,
            height: height,
            is_multi: is_multi
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            if (iindex == 1) {
                $(".tt-resizable-panel-0").css("width", iwidth);
            }
            if (iindex != 0)
                $(".tt-resizable-panel-" + iindex).css("height", iheight);
            $(".tt-resizable-panel-" + iindex).css("width", iwidth);
            if (iindex == 0)
                $(".tt-resizable-panel-" + iindex).css("height", "auto !impotant");
        },
        error: function (response) {
        }
    });


}

var checkKeyPress = function (val, operation, content, parent, e) {
    //$scope.$parent.inputcode = e.keyCode;
    var code = e.which;
    //if (code == 45 || code == 105 || code == 73 || code == 189 || code == 173 || code == 109) {
    //    $scope.doMatrixOperation(val, operation, content, parent, e);
    //}
    if ((code >= 48 && code <= 57) || code == 8 || code == 190 || (code >= 96 && code <= 105) || code == 110 || code == 46)
        doMatrixOperation(val, operation, content, parent, '', e);
    try {
        if (val.includes("i") || val.includes("-")) {
            e.which = 189;
            if (textField != null) {
                var judgementId = $(textField)[0].id.substring(4);
            }
            //doMatrixOperation(val, operation, content, parent, judgementId, e); 
            MatrixOperation('invertCurrent');
        }
    }
    catch (e) {
        if (val.indexOf("-") != -1 || val.indexOf("i") != -1) {
            e.which = 189;
            //doMatrixOperation(val, operation, content, parent, '', e);
            MatrixOperation('invertCurrent');
        }
    }
}

$(document).on("change", ".matrix-cell", function (e) {
    var value = $(this).data("value");
    matrixTargetID = e.target.id;
    var newValue = $(this).val();
    newValue = newValue.length == 0 ? "0" : newValue;

    if (value != newValue) {
        //var parentnodeID = $('#MainContent_hdnparentnodeID').val();
        var index = $(this).data("index");
        var operationID = $(this).data("operation");
        var pairs = eval("[" + $(this).data("pairs") + "]");
        doMatrixOperation(newValue, operationID, pairs, output.parentnodeID, '', null, true);
        //$(this).val(newValue);
    }
});

$(document).on("keydown", ".matrix-cell", function (e) {
    var code = e.which;
    if (((code >= 65 && code <= 72) || (code >= 74 && code <= 90)) || code == 188 || code == 191 || code == 192 || (code >= 219 && code <= 222) || code == 186 || code == 187 || code == 106 || code == 107 || code == 111) {
        return e.preventDefault();
    }
});

$(document).on("focus", ".matrix-cell", function (e) {
    textField = null;
    $("#btnInvertJudgment").removeClass("tt-button-green-normal");
    $("#btnInvertJudgment").addClass("tt-button-transparent not-active disabled");
    $("#navInvertJudgment").removeClass("tt-button-green-normal");
    $("#navInvertJudgment").addClass("tt-button-transparent not-active-footer-navs");

    if ($(this).val().trim().length > 0 && !isNaN($(this).val())) {
        textField = this;
        $("#btnInvertJudgment").removeClass("tt-button-transparent not-active disabled");
        $("#btnInvertJudgment").addClass("tt-button-green-normal");
        $("#navInvertJudgment").removeClass("tt-button-transparent not-active-footer-navs");
        $("#navInvertJudgment").addClass("tt-button-green-normal");
    }
});

$(document).on("blur", ".matrix-cell", function (e) {
    setTimeout(function () {
        var focusedElement = document.activeElement;
        if (textField == null || (focusedElement.id.indexOf("btnInvertJudgment") < 0 && focusedElement.className.indexOf("matrix-cell") < 0)) {
            $("#btnInvertJudgment").removeClass("tt-button-green-normal");
            $("#btnInvertJudgment").addClass("tt-button-transparent not-active disabled");
            $("#navInvertJudgment").removeClass("tt-button-green-normal");
            $("#navInvertJudgment").addClass("tt-button-transparent not-active-footer-navs");
            textField = null;
        }
    }, 100, false);
});

$(document).on("click", ".pwnl-click", function (e) {
    $.cookie('currentstep', hdnCurrentStep);
    $.cookie('is_under_review', true);
    var pair1 = $(this).data("pair-left");
    var pair2 = $(this).data("pair-right");
    //var PairvalSubs = parseInt(parseInt(pair2) - parseInt(pair1));
    //if (PairvalSubs <= 1) {
    //    if (PairvalSubs == 1) {
    //        $.cookie('SelectionID', pair1);
    //    }
    //    else {
    //        $.cookie('SelectionID', pair2);
    //    }
    //}
    //else {
    //    $.cookie('SelectionID', (parseInt(pair1) + parseInt($.cookie('TotalResultRows'))));
    //}
    $.cookie('judgementStep', output.current_step);
    /*$.cookie('ShowJudgTable', false);*/
    editPwnlClick(output.parentnodeID, output.current_step, pair1, pair2);
});

var editPwnlClick = function (parentnodeID, current_step, id1, id2) {
    var addstep = false;
    if (getStepNumber(id1, id2, true) < 0) {
        AllowAddStep = true;
        addstep = true;
    }
    else
        AllowAddStep = false;

    $.cookie('AllowAddStep', AllowAddStep);
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/redoPairs",
        data: JSON.stringify({
            parentnodeID: parentnodeID,
            current_step: current_step,
            firstNode: id1,
            secondNode: id2,
            is_name: false,
            add_step: addstep
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var step = data.d;
            if (parseInt(step) == parseInt(current_step))
                addstep = true;
            //$scope.$parent.is_under_review = true;
            //$scope.matrixReview = false;
            if (addstep) {
                //load_step_list();
                //previous_cluster_step = current_step + 1;
                //get_pipe_data(current_step, output.is_AT_owner);
                //setPagination();
                //hdnPreviousStep = current_step + 1;                
                $('#hdnPageNumber').val(current_step);
                setTimeout(function () {
                    $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
                return true;
            }

            if (step <= 0) {
                //$scope.active_multi_index = Math.abs(data.d);
                // step = 9;

                $.cookie('SelectionID', Math.abs(step));
                var stepval = output.PipeParameters.StepPairsData[0][0];
                if (stepval == '[') {
                    step = JSON.stringify(parseInt(JSON.parse(output.PipeParameters.StepPairsData)[0][0]) + 1);
                }
                else {
                    step = stepval + 1;
                }

                // step = parseInt(output.PipeParameters.StepPairsData[0][0] + 1);

            }

            if (!addstep) {
                $('#hdnPageNumber').val(step);
                setTimeout(function () {
                    $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
            }
        },
        error: function (response) {
        }
    });
}

var steps_list = null;
//var is_pipescreen =
var is_anytime = true;
var is_teamtime = false;
var load_step_list = function (first, last) {
    if (!first)
        first = 1;
    if (!last)
        last = output.total_pipe_steps;
    steps_list = null;
    if (steps_list == null) {
        $http({
            method: "POST",
            url: baseUrl + "pages/Anytime/Anytime.aspx/loadStepList",
            data: {
                first: first,
                last: last
            },
        }).then(function success(response) {
            //$scope.$apply(function () {
            steps_list = eval("[" + response.data.d + "]");
            is_pipescreen = is_anytime || is_teamtime ? true : false;
            //$scope.showLoadingModal = false;
            loadSteps = false;
            //  $scope.scrolltoelement($scope.output.current_step, 0);
            //$scope.get_pipe_data(step, is_AT_owner);
            //console.log(2);
            //});
            // $scope.get_pipe_data(step, is_AT_owner);

        }, function error(response) {
            //console.log(response);
            // $scope.get_pipe_data(1, is_AT_owner);
        });
    }
}


function glowMatrix(removeGlow, idElem) {
    if (removeGlow) {
        $(".matrix-cell").removeClass("box-glow");
    }
    else if (idElem) {
        $(".matrix-cell").removeClass("box-glow");
        $(idElem).addClass("box-glow");
    }
    else {
        $.each(matrixCellIDs, function (i, value) {
            $(value).addClass("box-glow");
        });
    }
}

var invertCurrentJudgment = function () {
    //alert(textField);
    //var textField = textField;
    if (textField != null && $(textField).val().trim().length > 0 && !isNaN($(textField).val())) {
        $(textField).val("-" + $(textField).val());
        $(textField).keyup();
        $(textField).change();
        //setTimeout(function () {
        //    if ($('#' + textField.id).css('color') == 'rgb(0, 0, 0)') {
        //        $('#' + textField.id).css('color', 'red');
        //    }
        //    else {
        //        $('#' + textField.id).css('color', 'black');
        //    }
        //    $('#' + textField.id).css('font-weight', '500')
        //}, 100);
    }
}

/*Copy to clipboard link*/
function copyJudgmentTable(text, value) {
    //judgmentTableValues.pairsData = eval($('#MainContent_hdnStepPairsData').val());
    //judgmentTableValues.objsData = eval($('#MainContent_hdnObjectivesData').val());
    //text = $('#txtarClipBoardData').text();
    text = ClipBoardData;
    $(".judgeHeader").each(function () {
        $(".judgeHeader").removeClass('activeHeader')
    });
    if (value == 3)
        $(".judgeHeader3").addClass('activeHeader');
    //console.log('text: ' + text);
    var sampleTextarea = document.createElement("textarea");
    document.body.appendChild(sampleTextarea);
    sampleTextarea.value = text; //save main text in it
    sampleTextarea.select(); //select textarea contenrs
    document.execCommand("copy");
    document.body.removeChild(sampleTextarea);
    alert('Data copied to clipbord');
}

var open_pasteModal = function (value) {
    $(".judgeHeader").each(function () {
        $(".judgeHeader").removeClass('activeHeader')
    });
    if (value == 4)
        $(".judgeHeader4").addClass('activeHeader');
    $('#paste-message').text('Paste the clipboard below and click Go');
    $("#ClipBoardData").val("");
    $('#MainContent_pasteModal').modal('show');
}

var pasteJudgmentTable = function () {
    var clipBoardData = $("#ClipBoardData").val();
    $('#btngo').addClass('Btndisabled');
    var thisIsBad = false;
    //if (judgmentTableValues.objsData.length < output.PipeParameters.ObjectivesData.length) {
    //    thisIsBad = true;
    //}

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/pasteClipBoardData",
        data: JSON.stringify({
            clipBoardData: clipBoardData,
            sameElements: thisIsBad
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.d.success) {
                //var results_object = response.data.d.data;
                //results_object.StepPairsData = eval(results_object.StepPairsData);
                //results_object.ObjectivesData = eval(results_object.ObjectivesData);
                //$scope.output.PipeParameters = results_object;
                //$scope.output.PipeParameters.equalMessage = false;
                //$scope.pasteMessage = "Pasted all data successfully";
                $("#paste-message").text("Pasted all data successfully!");
                $(".paste-message-icon").removeClass("icon-tt-close");
                $(".paste-message-icon").addClass("icon-tt-check");
                $(".paste-message-a").addClass("success").removeClass("error");
                saveOBPriority('False');
            }
            else {
                alert(JSON.stringify(response));
            }
        },
        error: function (response) {
            $("#paste-message").text("Failed! The size of data copied does not match the current table");
            $(".paste-message-icon").addClass("icon-tt-close");
            $(".paste-message-icon").removeClass("icon-tt-check");
            $(".paste-message-a").removeClass("success").addClass("error");
        }
    });
}

function sortValues(a, b) {
    //if ((type == 'GlobalResult' && reverseGlobal) || (type == 'LocalResult' && !reverseLocal)) {
    //    return (a[columntosort] > b[columntosort]) ? 1 : ((a[columntosort] < b[columntosort]) ? -1 : 0);
    //} else {
    //    return (b[columntosort] > a[columntosort]) ? 1 : ((b[columntosort] < a[columntosort]) ? -1 : 0);
    //}

    if ((hdnpage_type == 'atShowGlobalResults' && reverseGlobal) || (hdnpage_type == 'atShowLocalResults' && reverseLocal)) {
        if (a[columntosort] > b[columntosort]) {
            return -1;
        }
        if (a[columntosort] < b[columntosort]) {
            return 1;
        }
        return 0;
    }
    else {
        if (b[columntosort] > a[columntosort]) {
            return -1;
        }
        if (b[columntosort] < a[columntosort]) {
            return 1;
        }
        return 0;
    }
}

var sort = function (a, b, ascending) {
    if (ascending == 'asc') {
        return a[columntosort] > b[columntosort] ? 1 : -1;
    }
    else if (ascending == 'desc') {
        return a[columntosort] > b[columntosort] ? -1 : 1;
    }
    else {
        return 0;
    }
}

var normalization_Change = function (value, is_global, type, viewType) {
    value = value == null ? 0 : value;
    $('#sort_change').prop('checked', false);
    type = type == undefined || null ? 'LocalResult' : type;
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/changeNormalization",
        data: JSON.stringify({
            normalization: value,
            step: output.current_step,
            fGlobal: is_global,
            wrtNodeID: selectedNode > 0 ? selectedNode : -1
        }),
        async: false,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data != undefined && data != null && data != '') {

                var html = '';
                var results_data = data.d.results_data;
                if (columntosort != undefined && columntosort != null && parseInt(columntosort) > 0) {
                    //orderType = 'asc';
                    results_data.sort((a, b) => sort(a, b, orderType));

                }

                var canshowresult = data.d.canshowresults == undefined ? data.d.canshowresult : data.d.canshowresults;

                var maxCombinedResult = 0, loopCount = 0;
                var hdnCombinedResult = 0, hdnindividualResult = 0;
                for (var i = 0; i < results_data.length; i++) {
                    if (loopCount == 0) {
                        /*if ((results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {*/
                        if ((results_data[0][4] == 'rvBoth')) {
                            hdnCombinedResult = ((results_data[i][3] == '' || results_data[i][3] == 'NaN') ? 0 : parseFloat(results_data[i][3]));
                            hdnindividualResult = ((results_data[i][2] == '' || results_data[i][2] == 'NaN') ? 0 : parseFloat(results_data[i][2]));
                            if (hdnCombinedResult > hdnindividualResult)
                                maxCombinedResult = hdnCombinedResult;
                            else
                                maxCombinedResult = hdnindividualResult;
                        }
                        else if ((results_data[0][4] == 'rvGroup') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {
                            if (results_data[i][3] == '' || results_data[i][3] == 'NaN') {
                                maxCombinedResult = 0;
                            }
                            else {
                                maxCombinedResult = parseFloat(results_data[i][3]);
                            }
                        }
                        else {
                            if (results_data[i][2] == '' || results_data[i][2] == 'NaN') {
                                maxCombinedResult = 0;
                            }
                            else {
                                maxCombinedResult = parseFloat(results_data[i][2]);
                            }
                        }
                    }
                    else {
                        /*if ((results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {*/
                        if ((results_data[0][4] == 'rvBoth')) {
                            if (results_data[i][3] != '' && results_data[i][3] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][3])) {
                                maxCombinedResult = parseFloat(results_data[i][3]);
                            }
                            if (results_data[i][2] != '' && results_data[i][2] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][2])) {
                                maxCombinedResult = parseFloat(results_data[i][2]);
                            }
                        }
                        else if ((results_data[0][4] == 'rvGroup') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {
                            if (results_data[i][3] != '' && results_data[i][3] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][3])) {
                                maxCombinedResult = parseFloat(results_data[i][3]);
                            }
                        }
                        else {
                            if (results_data[i][2] != '' && results_data[i][2] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][2])) {
                                maxCombinedResult = parseFloat(results_data[i][2]);
                            }
                        }
                    }
                    loopCount = loopCount + 1;
                }
                if (type == 'LocalResult') {
                    var ResultColWidth = '';
                    html += "<div class='border-bottom pb-2 pt-3 m-0 row'>";
                    if (results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') {
                        html += "<div class='col-6'><h6 class='mobile_progress text_green text-center'>Your Results</h6></div>";
                    }
                    if (results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') {
                        html += "<div class='col-6'><h6 class='text_blue text-center'>Combined Results</h6></div>";
                    }
                    if (results_data[0][4] == 'rvBoth') {
                        ResultColWidth = 'col-6';
                    }
                    else { ResultColWidth = 'col-12'; }
                    html += "</div>";

                    for (i = 0; i < results_data.length; i++) {
                        var resultsData = results_data[i];
                        html += "<div class='row m-0 py-3 border-bottom align-items-end'>";

                        html += "<div class='col-12'><div class='resultTable_column'><p class='mb-2'><span class='number'>" + resultsData[0] + "</span>" + resultsData[1] + "</p></div></div>";

                        //html += "<p><span>" + resultsData[0] + "</span> " + resultsData[1] + "</p>";                   
                        if (results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') {
                            html += "<div class='" + ResultColWidth + "'><div class='resultTable_column'><div class='mobile_progress d-flex align-items-center'>";
                            //html += "<div class='progress_result'>";
                            if (output.PipeParameters.ShowBars) {
                                html += "<div class='progress w-100'>";
                                if (resultsData[2] == '' || resultsData[2] == 'NaN') {
                                    html += "<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                                }
                                else {
                                    /*html += "<div class='progress-bar bg_blue' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";*/
                                    html += "<div class='progress-bar bg_green' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                                }
                                html += "</div>";
                            }
                            if (resultsData[2] != "" && resultsData[2] != "NaN") { html += "<h6 class='text_green mb-0 ms-3'>" + (parseFloat(resultsData[2]) * 100).toFixed(2) + "%</h6>"; }
                            //else
                            //    html += '<p>0.00%</p>';
                            //html += "</div>";
                            html += "</div></div></div>";
                        }
                        if (results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') {
                            html += "<div class='" + ResultColWidth + "'><div class='resultTable_column'><div class=' d-flex align-items-center'>";
                            //html += "<div class='progress_result'>";
                            if (output.PipeParameters.ShowBars) {
                                html += "<div class='progress w-100'>";
                                if (resultsData[3] != "" && resultsData[3] != "NaN") {
                                    html += "<div class='progress-bar bg_blue abc1' role='progressbar' style='width:" + ((parseFloat(resultsData[3]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[3]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";

                                }
                                else {
                                    html += "<div class='progress-bar bg_blue abc9' role='progressbar' style='width:0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                                }
                                //}
                                html += "</div>";
                            }
                            //if (results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') {
                            if (resultsData[3] != "" && resultsData[3] != "NaN") { html += '<h6 class="text_blue  mb-0 ms-3">' + (parseFloat(resultsData[3]) * 100).toFixed(2) + '%</h6>'; }
                            //else
                            //    html += '<p>0.00%</p>';
                            //html += "</div>";
                            html += "</div></div></div>";
                        }

                        html += "</div>";
                    }
                    $('#divLocalResults').html('');
                    $('#divLocalResults').html(html);
                }

                html = '';



                var details = navigator.userAgent;
                var regexp = /android|iphone|kindle|ipad/i;
                var isMobile = regexp.test(details);
                if (isMobile || viewType == "mddlGlobalNormalization") {
                    var colWidth = '';
                    if (canshowresult.individual && canshowresult.combined) {
                        colWidth = 'col-6';
                    }
                    else {
                        colWidth = 'col-12';
                    }
                    for (i = 0; i < results_data.length; i++) {
                        var resultsData = results_data[i];
                        html += "<div class='row m-0 py-3 border-bottom align-items-end'><div class='col-12'> <div class='resultTable_column ps-3'><p class='mb-2'>";
                        if (output.PipeParameters.showIndex == true) {
                            html += "<span class='number'>" + resultsData[0] + "</span>";
                        }
                        html += resultsData[1] + "</p></div></div>";

                        var clsdnone = !output.PipeParameters.ShowBars ? ' d-none' : '';
                        html += "<div class='" + colWidth + "'><div class='resultTable_column ps-3'><div class='mobile_progress d-flex align-items-center'><div class='progress" + clsdnone + "'>";
                        if (output.PipeParameters.ShowBars && (results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.individual)) {
                            if (resultsData[2] != "" && resultsData[2] != "NaN") {
                                html += "<div class='progress-bar bg_green' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + maxCombinedResult + "'></div>";
                            }
                            else {
                                html += "<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='25' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                            }
                        }
                        html += "</div>";
                        if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.individual)) {
                            html += '<h6 class="text_green mb-0 ms-3">' + (parseFloat(resultsData[2]) * 100).toFixed(2) + '%</h6>';
                        }
                        html += "</div></div></div>";
                        html += "<div class='" + colWidth + "'><div class='resultTable_column pe-3'><div class='mobile_progress mobile_progress_right d-flex align-items-center'><div class='progress" + clsdnone + "'>";
                        if (output.PipeParameters.ShowBars && (results_data[0][4] == "rvGroup" || results_data[0][4] == "rvBoth") && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {
                            if (resultsData[3] == "" || resultsData[3] == "NaN") {
                                html += "<div class='progress-bar' role='progressbar' style='width:0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                            }
                            else {
                                html += "<div class='progress-bar' role='progressbar' style='width:" + ((parseFloat(resultsData[3]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%; ' aria-valuenow='" + Math.ceil(parseFloat(resultsData[3]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";

                            }
                        }
                        html += "</div>";
                        if ((results_data[0][4] == "rvGroup" || results_data[0][4] == "rvBoth") && (canshowresult != undefined && canshowresult != null && canshowresult.combined))
                            if (resultsData[3] != "" && resultsData[3] != "NaN")
                                html += "<h6 class='text_blue mb-0 ms-3'>" + (parseFloat(resultsData[3]) * 100).toFixed(2) + "%</h6>";
                        html += "</div></div></div>";
                        html += "</div>";
                    }
                }
                else {
                    html += '<tbody>';

                    for (i = 0; i < results_data.length; i++) {
                        var resultsData = results_data[i];
                        html += '<tr>';
                        if (output.PipeParameters.showIndex == true)
                            html += '<td scope="row">' + resultsData[0] + '</td>';
                        html += '<td>' + resultsData[1] + '</td>';
                        if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.individual)) {
                            if (resultsData[2] != "" && resultsData[2] != "NaN")
                                html += '<td class="text_green text-center text_bold">' + (parseFloat(resultsData[2]) * 100).toFixed(2) + '%</td>';
                            else
                                html += '<td class="text_green text-center text_bold">0.00%</td>';
                        }
                        if ((results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {
                            if (resultsData[3] != "" && resultsData[3] != "NaN")
                                html += '<td class="text_blue text-center text_bold">' + (parseFloat(resultsData[3]) * 100).toFixed(2) + '%</td>';
                            else
                                html += '<td class="text_blue text-center text_bold">0.00%</td>';
                        }
                        if (output.PipeParameters.ShowBars) {
                            html += '<td>';
                            if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.individual)) {
                                html += '<div class="progress" style="height: 10px;" data-toggle"tooltip" title="' + (parseFloat(resultsData[2]) * 100).toFixed(2) + ' %">';
                                //html += '<div class="progress-bar bg_blue" role="progressbar" style="width:' + (parseFloat(resultsData[2]) * 100) + '%" aria-valuenow="' + Math.ceil(parseFloat(resultsData[2]) * 100) + '" aria-valuemin="0" aria-valuemax="100"></div>';
                                if (resultsData[2] == '' || resultsData[2] == 'NaN') {
                                    html += "<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                                }
                                else {
                                    /*html += "<div class='progress-bar bg_blue' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + maxCombinedResult + "'></div>";*/
                                    html += "<div class='progress-bar bg_green' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + maxCombinedResult + "' data-toggle='tooltip' title='" + (parseFloat(resultsData[2]) * 100).toFixed(2) + "%'></div>";
                                }
                                html += '</div>';
                            }
                            if ((results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {
                                html += '<div class="progress" style="height: 10px;" data-toggle="tooltip" title="' + (parseFloat(resultsData[3]) * 100).toFixed(2) + ' %">';
                                //html += '<div class="progress-bar bg_green" role="progressbar" style="width:' + (parseFloat(resultsData[3]) * 100) + '%" aria-valuenow="' + Math.ceil(parseFloat(resultsData[3]) * 100) + '" aria-valuemin="0" aria-valuemax="100"></div>';
                                if (resultsData[3] != "" && resultsData[3] != "NaN") {
                                    html += "<div class='progress-bar bg_blue abc2' role='progressbar' style='width:" + ((parseFloat(resultsData[3]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[3]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "' data-toggle='tooltip' title='" + (parseFloat(resultsData[3]) * 100).toFixed(2) + "%'></div>";
                                }
                                else {
                                    html += "<div class='progress-bar bg_blue abc3' role='progressbar' style='width:0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                                }
                                html += '</div>';
                            }
                            html += '</td>';
                        }
                        html += '</tr>';
                    }
                    html += '</tbody>';

                }
                //console.log(html);
                //$('#tblJugement tbody').empty();
                if (type == 'LocalResult') {
                    if (!isMobile)
                        $('#tblLocalResults tbody').replaceWith(html);
                    $('#tblLocalResults').removeClass('d-none');
                }
                if (type == 'GlobalResult') {
                    if (isMobile || viewType == "mddlGlobalNormalization") {
                        $('#mtblGlobalResults').html('');
                        $('#mtblGlobalResults').html(html);
                    }
                    else {
                        $('#tblGlobalResults tbody').replaceWith(html);
                    }
                    $('#tblGlobalResults').removeClass('d-none');
                }
                //save_WRTNodeID(selectedNode);
                //$('#tblGlobalResults thead th, #tblLocalResults thead th:eq(' + columntosort + ')').trigger('click');
            }
        },
        error: function (response) {
        }
    });
}

var LoadGlobalHierarchy = function () {
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/Login.aspx/loadHierarchy",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: true,
        success: function (data) {
            var accordionItem = '';
            var accordionmItem = '';
            var steps = data.d.data;
            accordionItem = LoadGlobalHierarchyHtml(steps, accordionItem, false);
            $("#divHierarchy").html(accordionItem);
            accordionmItem = LoadmGlobalHierarchyHtml(steps, accordionmItem, false);
            $("#mdivHierarchy").html(accordionmItem);
            $("#accordionExample").html(accordionmItem);
            $("#accordionExample ul").first().addClass("tree tree_chartm table_Scroll");

            $("#mdivHierarchy ul").first().addClass("tree tree_chartm table_Scroll");
            $("#divHierarchy ul").first().addClass("tree tree_chart table_Scroll");
            $('.selectcolor, #selecthdrcolor0').first().addClass('active_treerow');
            $('.mselectcolor, #mselecthdrcolor0').first().addClass('active_treerow');
            /*$('.mselectcolor').first().addClass('active_treerow');*/

            var hText = $('.selectcolor').first()[0];
            if (steps != undefined && steps != null && steps.length > 0)
                $('#headText').text("With respect to " + steps[0][1]);

        },
        error: function () {
            alert("error");
        }
    });
}

var LoadmGlobalHierarchyHtml = function (steps, accordionmItem, flag) {
    indx = 0;
    accordionmItem += '<ul>';
    steps.forEach((element, index) => {
        if (element.length == 6 && element[5].length > 0) {
            accordionmItem += '<li class="treee_section">';
            accordionmItem += '<input type="checkbox" id=group' + element[4] + ' checked />';
            if (element[1] != '' && element[1].length > 30) {
                accordionmItem += '<label for="group" id="mselecthdrcolor' + element[0] + '" class="mselectcolor sethovercolor rselecthdrcolor" onclick="save_WRTNodeID(' + element[0] + ',' + indx + ')" title="' + element[1] + '"><span title="' + element[1] + '" class="treeSpan">' + element[1].substr(0, 30) + '...</span><span>' + parseFloat(element[2]).toFixed(2) + '%</span></label>';
            }
            else {
                accordionmItem += '<label for="group" id="mselecthdrcolor' + element[0] + '" class="mselectcolor sethovercolor rselecthdrcolor" onclick="save_WRTNodeID(' + element[0] + ',' + indx + ')"><span title="' + element[1] + '" class="treeSpan">' + element[1] + '</span><span>' + parseFloat(element[2]).toFixed(2) + '%</span></label>';
            }
        }
        else {
            if (element[1].length > 30) {
                accordionmItem += '<li class="mselectcolor sethovercolor rselecthdrcolor" style="cursor: pointer;" id="mselectrowcolor' + element[0] + '" onclick="save_WRTNodeID(' + element[0] + ',' + indx + ')" title="' + element[1] + '"><span title="' + element[1] + '" class="treeSpan">' + element[1].substr(0, 30) + '...</span><span>' + parseFloat(element[2]).toFixed(2) + '%</span >';
            }
            else {
                accordionmItem += '<li class="mselectcolor sethovercolor" style="cursor: pointer;" id="mselectrowcolor' + element[0] + '" onclick="save_WRTNodeID(' + element[0] + ',' + indx + ')"><span title="' + element[1] + '" class="treeSpan">' + element[1] + '</span><span>' + parseFloat(element[2]).toFixed(2) + '%</span >';
            }
        }
        stepno[i++] = element[4];
        indx++;
        if (element.length == 6 && element[5].length > 0) {
            accordionmItem = LoadmGlobalHierarchyHtml(element[5], accordionmItem, true);
        }
        accordionmItem += '</li>'
    });
    accordionmItem += '</ul>'
    return accordionmItem;
}

var indx = 0;

var LoadGlobalHierarchyHtml = function (steps, accordionItem, flag) {
    //if (flag)
    //    accordionItem += '<ul class="ul' + element[4] + '">';
    //else
    accordionItem += '<ul class="tree" id="main-tab">';
    steps.forEach((element, index) => {
        if (element.length == 6 && element[5].length > 0) {
            accordionItem += '<li class="treee_section">';
            accordionItem += '<input type="checkbox" id=group' + element[4] + ' checked />';
            if (element[1] != '' && element[1].length > 30) {
                accordionItem += "<label onclick='save_WRTNodeID(" + element[0] + ", " + indx + ")' for='group' id='selecthdrcolor" + indx + "' class='w-100 rselecthdrcolor'  title='" + element[1] + "'><span title='" + element[1] + "' class='treeSpan addTreeSpan'>" + element[1].substr(0, 30) + "...</span><span>" + parseFloat(element[2]).toFixed(2) + "%</span></label>";
            }
            else {
                accordionItem += "<label onclick='save_WRTNodeID(" + element[0] + ",  " + indx + ")' for='group' id='selecthdrcolor" + indx + "' class='w-100 rselecthdrcolor'  ><span title='" + element[1] + "' class='treeSpan addTreeSpan'>" + element[1] + "</span><span>" + parseFloat(element[2]).toFixed(2) + "%</span></label>";
            }
        }
        else {

            accordionItem += "<li onclick='save_WRTNodeID(" + element[0] + ",  " + indx + ")' class='w-100 rselecthdrcolor' style='cursor: pointer;' id='selecthdrcolor" + indx + "'  ><span title='" + element[1] + "' class='treeSpan'>" + element[1] + "</span><span>" + parseFloat(element[2]).toFixed(2) + "%</span >";
        }
        stepno[i++] = element[4];
        indx++;
        if (element.length == 6 && element[5].length > 0) {
            accordionItem = LoadGlobalHierarchyHtml(element[5], accordionItem, true);
        }
        accordionItem += '</li>'
    });
    accordionItem += '</ul>'
    return accordionItem;
}
var PreviusSelecthdrcolorID = 0;
var save_WRTNodeID = function (wrtnodeID, indx) {
    selectedNode = wrtnodeID;
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    var hText = '';
    hText = $("#mselectrowcolor" + wrtnodeID)[0];
    if (typeof (hText) == 'undefined') {
        hText = $("#mselecthdrcolor" + wrtnodeID)[0];
    }
    if (typeof (hText) !== 'undefined') {
        $('#headText').text("With respect to " + (hText.children[0]).title);
    }
    // if (!isMobile) {
    $(".selectcolor").each(function () {
        $(".selectcolor").removeClass('prm-color_brown');
    });
    if (PreviusSelecthdrcolorID != 0) {
        $('.rselecthdrcolor').removeClass('active_treerow');
        $("#selecthdrcolor" + indx).addClass('active_treerow');
        PreviusSelecthdrcolorID = wrtnodeID;
    }
    else {
        //$('#main-tab li, #selecthdrcolor0').removeClass('active_treerow');
        $('.rselecthdrcolor').removeClass('active_treerow');
        $("#selecthdrcolor" + indx).addClass('active_treerow');
        PreviusSelecthdrcolorID = wrtnodeID;
    }

    //$("#selectrowcolor" + wrtnodeID).addClass('activecurrentcluster');
    // $('.toggle_tree').prop('checked', false);
    //  }
    // else {
    $(".mselectcolor").each(function () {
        $(".mselectcolor").removeClass('prm-color_brown');
        $(".mselectcolor").removeClass('active_treerow');
    });
    $("#mselectrowcolor" + wrtnodeID).addClass('active_treerow');
    $("#mselecthdrcolor" + wrtnodeID).addClass('active_treerow');
    //$("#mselectrowcolor" + wrtnodeID).addClass('activecurrentcluster');
    $('.toggle_tree').prop('checked', false);
    // }
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/GetOverallResultsData",
        data: JSON.stringify({
            rmode: !output.PipeParameters.showNormalization ? 0 : $('#ddlGlobalNormalization').val(),
            //rmode: '1',
            wrtnodeID: wrtnodeID,
            isReload: true
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var results_data = data.d[0];
            if (columntosort != undefined && columntosort != null && parseInt(columntosort) >= 0) {
                results_data.sort((a, b) => sort(a, b, orderType));
            }

            var html = '', mhtml = '';
            html += '<tbody>';
            /*  mhtml += '<tbody>';*/

            var maxCombinedResult = 0, loopCount = 0;
            var hdnCombinedResult = 0, hdnindividualResult = 0;
            for (var i = 0; i < results_data.length; i++) {
                if (loopCount == 0) {
                    /*if ((results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {*/
                    if ((results_data[0][4] == 'rvBoth')) {
                        if (results_data[i][3] == '' || results_data[i][3] == 'NaN') {
                            maxCombinedResult = 0;
                        }
                        else {
                            hdnCombinedResult = parseFloat(results_data[i][3]);
                            hdnindividualResult = parseFloat(results_data[i][2]);
                            if (hdnCombinedResult > hdnindividualResult)
                                maxCombinedResult = hdnCombinedResult;
                            else
                                maxCombinedResult = hdnindividualResult;
                        }
                    }
                    else if ((results_data[0][4] == 'rvGroup') && data.d[1].combined) {
                        if (results_data[i][3] == '' || results_data[i][3] == 'NaN') {
                            maxCombinedResult = 0;
                        }
                        else {
                            maxCombinedResult = parseFloat(results_data[i][3]);
                        }
                    }
                    else {
                        if (results_data[i][2] == '' || results_data[i][2] == 'NaN') {
                            maxCombinedResult = 0;
                        }
                        else {
                            maxCombinedResult = parseFloat(results_data[i][2]);
                        }
                    }
                }
                else {
                    /*if ((results_data[0][4] == 'rvBoth') && (canshowresult != undefined && canshowresult != null && canshowresult.combined)) {*/
                    if ((results_data[0][4] == 'rvBoth')) {
                        if (results_data[i][3] != '' && results_data[i][3] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][3])) {
                            maxCombinedResult = parseFloat(results_data[i][3]);
                        }
                        if (results_data[i][2] != '' && results_data[i][2] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][2])) {
                            maxCombinedResult = parseFloat(results_data[i][2]);
                        }
                    }
                    else if ((results_data[0][4] == 'rvGroup') && data.d[1].combined) {
                        if (results_data[i][3] != '' && results_data[i][3] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][3])) {
                            maxCombinedResult = parseFloat(results_data[i][3]);
                        }
                    }
                    else {
                        if (results_data[i][2] != '' && results_data[i][2] != 'NaN' && maxCombinedResult < parseFloat(results_data[i][2])) {
                            maxCombinedResult = parseFloat(results_data[i][2]);
                        }
                    }
                }
                loopCount = loopCount + 1;
            }

            //if (isMobile || (window.screen.availWidth < 975 && window.screen.availHeight < 675)) {

            //}

            //for (i = 0; i < results_data.length; i++) {
            //    var resultsData = results_data[i];
            //    mhtml += "<tr> <td><div class='table_item'><p>";
            //    if (output.PipeParameters.showIndex == true) {
            //        mhtml += '<span>' + resultsData[0] + '</span>';
            //    }
            //    mhtml += resultsData[1] + "</p><div class='progress_result'>";
            //    if (output.PipeParameters.ShowBars && (results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && data.d[1].individual) {
            //        mhtml += "<div class='progress mb-1' style='height: 20px;'>";
            //        if (resultsData[2] != "" && resultsData[2] != "NaN")
            //            mhtml += "<div class='progress-bar bg_blue' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + maxCombinedResult + "'></div>";
            //        else
            //            mhtml += "<div class='progress-bar bg_blue' role='progressbar' style='width: 0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
            //        mhtml += "</div>";
            //    }
            //    if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && data.d[1].individual) {
            //        if (resultsData[2] != "" && resultsData[2] != "NaN")
            //            mhtml += '<p>' + (parseFloat(resultsData[2]) * 100).toFixed(2) + '%</p>';
            //        else
            //            mhtml += '<p>0.00%</p>';
            //    }
            //    mhtml += "</div><div class='progress_result'>";
            //    if (output.PipeParameters.ShowBars && (results_data[0][4] == "rvGroup" || results_data[0][4] == "rvBoth") && data.d[1].combined) {
            //        mhtml += "<div class='progress' style='height: 20px;'>"
            //        if (resultsData[3] == "" || resultsData[3] == "NaN")
            //            mhtml += "<div class='progress-bar bg_green abc4' role='progressbar' style='width:0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
            //        else
            //            mhtml += "<div class='progress-bar bg_green abc5' role='progressbar' style='width:" + ((parseFloat(resultsData[3]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%; ' aria-valuenow='" + Math.ceil(parseFloat(resultsData[3]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";

            //        mhtml += "</div>";
            //    }

            //    if ((results_data[0][4] == "rvGroup" || results_data[0][4] == "rvBoth") && data.d[1].combined)
            //        if (resultsData[3] != "" && resultsData[3] != "NaN")
            //            mhtml += "<p class='hl-color'>" + (parseFloat(resultsData[3]) * 100).toFixed(2) + "%</p>";
            //        else
            //            mhtml += '<p>0.00%</p>';

            //    mhtml += '</div></div></td></tr>';
            //}

            var clsdnone = !output.PipeParameters.ShowBars ? ' d-none' : '';
            for (i = 0; i < results_data.length; i++) {
                var resultsData = results_data[i];
                mhtml += "<div class='row m-0 py-3 border-bottom align-items-end'><div class='col-12'> <div class='resultTable_column ps-3'><p class='mb-2'>";
                if (output.PipeParameters.showIndex == true) {
                    mhtml += "<span class='number'>" + resultsData[0] + "</span>";
                }
                mhtml += resultsData[1] + "</p></div></div>";

                mhtml += "<div class='col-6'><div class='resultTable_column ps-3'><div class='mobile_progress d-flex align-items-center'><div class='progress" + clsdnone + "'>";
                if (output.PipeParameters.ShowBars && (results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && data.d[1].individual) {
                    if (resultsData[2] != "" && resultsData[2] != "NaN") {
                        mhtml += "<div class='progress-bar bg_green' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + maxCombinedResult + "'></div>";
                    }
                    else {
                        mhtml += "<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='25' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                    }

                }
                mhtml += "</div>";
                if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && data.d[1].individual) {
                    mhtml += '<h6 class="text_green mb-0 ms-3">' + (parseFloat(resultsData[2]) * 100).toFixed(2) + '%</h6>';
                }
                mhtml += "</div></div ></div >";

                mhtml += "<div class='col-6'><div class='resultTable_column pe-3'><div class='mobile_progress mobile_progress_right d-flex align-items-center'><div class='progress" + clsdnone + "'>";
                if (output.PipeParameters.ShowBars && (results_data[0][4] == "rvGroup" || results_data[0][4] == "rvBoth") && data.d[1].combined) {
                    if (resultsData[3] == "" || resultsData[3] == "NaN") {
                        mhtml += "<div class='progress-bar' role='progressbar' style='width:0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                    }
                    else {
                        mhtml += "<div class='progress-bar' role='progressbar' style='width:" + ((parseFloat(resultsData[3]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%; ' aria-valuenow='" + Math.ceil(parseFloat(resultsData[3]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                    }
                }
                mhtml += "</div>";
                if ((results_data[0][4] == "rvGroup" || results_data[0][4] == "rvBoth") && data.d[1].combined) {
                    if (resultsData[3] != "" && resultsData[3] != "NaN") {
                        mhtml += "<h6 class='text_blue mb-0 ms-3'>" + (parseFloat(resultsData[3]) * 100).toFixed(2) + "%</h6>";
                    } else {
                        mhtml += "<h6 class='text_blue mb-0 ms-3'>0.00%</h6>";
                    }
                }
                mhtml += "</div></div></div></div>";

            }

            for (i = 0; i < results_data.length; i++) {
                var resultsData = results_data[i];
                html += '<tr>';
                if (output.PipeParameters.showIndex) {
                    html += '<td>' + resultsData[0] + '</td>';
                }
                html += '<td>' + resultsData[1] + '</td>';



                if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && data.d[1].individual) {
                    if (resultsData[2] != "" && resultsData[2] != "NaN")
                        html += '<td class="text_green text-center text_bold">' + (parseFloat(resultsData[2]) * 100).toFixed(2) + '%</td>';
                    else
                        html += '<td class="text_green text-center text_bold">0.00%</td>';
                }
                if ((results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && data.d[1].combined) {
                    if (resultsData[3] != "" && resultsData[3] != "NaN")
                        html += '<td class="text_blue text-center text_bold">' + (parseFloat(resultsData[3]) * 100).toFixed(2) + '%</td>';
                    else
                        html += '<td class="text_blue text-center text_bold">0.00%</td>';
                }
                if (output.PipeParameters.ShowBars) {
                    html += '<td>';
                    if ((results_data[0][4] == 'rvIndividual' || results_data[0][4] == 'rvBoth') && data.d[1].individual) {
                        html += '<div class="progress" style="height: 10px;">';
                        //html += '<div class="progress-bar bg_blue" role="progressbar" style="width:' + (parseFloat(resultsData[2]) * 100) + '%" aria-valuenow="' + Math.ceil(parseFloat(resultsData[2]) * 100) + '" aria-valuemin="0" aria-valuemax="100"></div>';
                        if (resultsData[2] == '' || resultsData[2] == 'NaN') {
                            html += "<div class='progress-bar bg_green' role='progressbar' style='width: 0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                        }
                        else {
                            html += "<div class='progress-bar bg_green' role='progressbar' style='width:" + ((parseFloat(resultsData[2]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[2]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                        }
                        html += '</div>';
                    }
                    if ((results_data[0][4] == 'rvGroup' || results_data[0][4] == 'rvBoth') && data.d[1].combined) {
                        html += '<div class="progress" style="height: 10px;">';
                        //html += '<div class="progress-bar bg_green" role="progressbar" style="width:' + (parseFloat(resultsData[3]) * 100) + '%" aria-valuenow="' + Math.ceil(parseFloat(resultsData[3]) * 100) + '" aria-valuemin="0" aria-valuemax="100"></div>';
                        if (resultsData[3] != "" && resultsData[3] != "NaN") {
                            html += "<div class='progress-bar bg_blue abc6' role='progressbar' style='width:" + ((parseFloat(resultsData[3]) * 100 / parseFloat(maxCombinedResult * 100).toFixed(2)) * 100).toFixed(2) + "%;' aria-valuenow='" + Math.ceil(parseFloat(resultsData[3]) * 100) + "' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                        }
                        else {
                            html += "<div class='progress-bar bg_blue abc7' role='progressbar' style='width:0%;' aria-valuenow='0' aria-valuemin='0' aria-valuemax='" + parseFloat(maxCombinedResult * 100).toFixed(2) + "'></div>";
                        }
                        html += '</div>';
                    }
                    html += '</td>';
                }
                html += '</tr>';
            }

            html += '</tbody>';
            /*       mhtml += '</tbody>';*/
            $('#mtblGlobalResults').html(mhtml);
            $('#tblGlobalResults tbody').replaceWith(html);
            $('#tblGlobalResults').removeClass('d-none');
            //if (isMobile || (window.screen.availWidth < 975 && window.screen.availHeight < 675)) {
            //    $('#mtblGlobalResults tbody').replaceWith(html);
            //}
            //else {
            //    $('#tblGlobalResults tbody').replaceWith(html);
            //}
        },
        error: function (response) {
        }
    });
}

var change_respondentAnswer = function (index, text, type, is_chb, variant, question) {
    variant = variant == undefined || null ? '' : variant.split('##').join(' ');
    if (type == '5') {
        text = $(text).children("option").filter(":selected").text();
        text = text.split('##').join(' ');
    }
    else if (type == '2' || type == '1' || type == '15') {
        text = $(text).val();
        text = text.split('##').join(' ');
    }
    else if (type == '4') {
        if ($(text).is(':checked')) {
            text = true;
        }
        else {
            text = false;
        }
    }
    else if (type == '14') {
        text = $(text).val();
    }
    else {
        text = text.split('##').join(' ');
    }


    if (is_chb == 'true') {
        if (text) {
            var currentAnswer = ";" + variant;
            if (SurveyAnswers[index][0] == "")
                currentAnswer = variant;
            SurveyAnswers[index][0] += currentAnswer;
        }
        else {
            //if is first on string then delete ------

            if (SurveyAnswers[index][0].indexOf(";" + variant) < 0)
                SurveyAnswers[index][0] = (SurveyAnswers[index][0]).replace(variant + ";", "");
            else
                SurveyAnswers[index][0] = (SurveyAnswers[index][0]).replace(";" + variant, "");

            if (SurveyAnswers[index][0].indexOf(";") < 0)
                SurveyAnswers[index][0] = (SurveyAnswers[index][0]).replace(variant, "");
        }
    }
    else {
        if ([1, 2, 14, 15].indexOf(parseInt(type)) > -1) {
            SurveyAnswers[index][0] = text;

            answer[index][0] = text;
        }
        if (type == 3) {

            if (text == "")
                SurveyAnswers[index][0] = "";
            else {
                /*SurveyAnswers[index][0] = text[0].indexOf(":") < 0 ? text[0] : text[1];*/
                SurveyAnswers[index][0] = text;
            }

            //$scope.answer[index] = text;
        }
        if (type == 5) {
            //answer[index][1] = "";
            SurveyAnswers[index][0] = text;
        }
        if (type == 12 || type == 13) {
            var stringAnswer = "";
            for (i = 0; i < Object.keys(answer[index]).length; i++) {
                //if (variant == 0) {
                //    if (answer[index][0]) {
                //        answer[index][i] = true;
                //    }
                //    //else {
                //    //    //variant is index2
                //    //    if (variant == 0) {
                //    //        if ((i !== 1 && type == 12) || (type == 13))
                //    //            answer[index][i] = false;
                //    //    }
                //    //}
                //}
                var toAdd = '';
                toAdd = (answer[index][i] == 'true' || answer[index][i] == 'True' || answer[index][i] == true ? 'True' : 'False') + ";";
                if (i == Object.keys(answer[index]).length - 1) {
                    toAdd = answer[index][i] == 'true' || answer[index][i] == 'True' || answer[index][i] == true ? 'True' : 'False';
                }
                stringAnswer += toAdd;
            }
            //alert(stringAnswer);
            SurveyAnswers[index][0] = stringAnswer;
        }

        //if (type == 12 || type == 13) {
        //    var stringAnswer = "";
        //    var ans_length = Object.keys(answer[index][0].split(';')).length;
        //    for (i = 0; i < ans_length; i++) {
        //        if (variant == 0) {
        //            if (answer[index][0].split(';')[0]) {
        //                answer[index][0].split(';')[i] = true;
        //            }
        //            else {
        //                //variant is index2
        //                if (variant == 0) {
        //                    if ((i !== 1 && type == 12) || (type == 13))
        //                        answer[index][0].split(';')[i] = false;
        //                }
        //            }
        //        }
        //        var qt_checked = $('#qType_' + type + '_' + i).is(':checked');
        //        var toAdd = qt_checked + ";";
        //        if (i == ans_length - 1) {
        //            toAdd = qt_checked;
        //        }
        //        stringAnswer += toAdd;

        //    }
        //    //alert(stringAnswer);
        //    SurveyAnswers[index][0] = stringAnswer;
        //}
    }

    if (output.pipeOptions.dontAllowMissingJudgment) {
        var skip = false, disablePage = false;
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
    //changeNextButtonStatusForSurvey();
}

var change_AllrespondentAnswer = function (index, text, type, is_chb, variant, question) {
    if (type == '12') {
        if ($.isNumeric(variant) && parseInt(variant) == 0) {
            var ischecked = $('#qType_' + type + '_0').is(':checked');
            $('.clsType12').prop("checked", ischecked);
            for (var i = 0; i < answer[index].length; i++) {
                answer[index][i] = ischecked;
            }
        }
        if ($.isNumeric(variant) && parseInt(variant) > 0) {
            answer[index][variant] = $('#qType_12_' + variant).is(':checked');
        }
    }
    if (type == '13') {
        if ($.isNumeric(variant) && parseInt(variant) == 0) {
            $('.clsType13').prop("checked", $('#qType_13_0').is(':checked'));
            for (var i = 0; i < answer[index].length; i++) {
                answer[index][i] = $('#qType_13_0').is(':checked');
            }
        }
        if ($.isNumeric(variant) && parseInt(variant) > 0) {
            answer[index][variant] = $('#qType_13_' + variant).is(':checked');
        }
    }
    change_respondentAnswer(index, text, type, is_chb, index);
}

var change_otherAnswer = function (type, index, text, variant) {
    text = $(text).val();
    if (type == 3) {
        SurveyAnswers[index][0] = text;
    }
    if (type == 4) {
        if ((SurveyAnswers[index][0]).indexOf(":") > -1) {
            var removestring = (SurveyAnswers[index][0]).split(";");
            SurveyAnswers[index][0] = "";
            //remove 'other' text so it wont create duplicate items
            for (i = 0; i < removestring.length; i++) {
                if (removestring[i].indexOf(":") < 0)
                    SurveyAnswers[index][0] += removestring[i] + ";";
            }
        }
        if (text) {
            var currentAnswer = text + ":";
            if (SurveyAnswers[index][0] == "")
                currentAnswer = text + ":";
            SurveyAnswers[index][0] += currentAnswer;
        }
        else {
            //if is first on string then delete ------
            if (SurveyAnswers[index][0].indexOf(";" + text + ":") < 0)
                SurveyAnswers[index][0] = (SurveyAnswers[index][0]).replace(text + ":" + ";", "");
            else
                SurveyAnswers[index][0] = (SurveyAnswers[index][0]).replace(";" + text + ":", "");

            if (SurveyAnswers[index][0].indexOf(";") < 0)
                SurveyAnswers[index][0] = (SurveyAnswers[index][0]).replace(text + ":", "");
        }
    }
    if (type == 5) {
        answer[index][0] = "";
        SurveyAnswers[index][0] = text;
    }

    if (output.pipeOptions.dontAllowMissingJudgment) {
        var skip = false, disablePage = false;
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
    //$scope.changeNextButtonStatusForSurvey();
}

function removeTags(str) {
    if ((str === null) || (str === ''))
        return false;
    else
        str = str.toString();

    // Regular expression to identify HTML tags in 
    // the input string. Replacing the identified 
    // HTML tag with a null string.
    return str.replace(/(<([^>]+)>)/ig, '');
}

var openQueModal = function (type, text) {
    if (type != undefined && type != null && type != '') {
        /*if (type == 'editQuestion') {*/
        if (type == '1') {
            $('#MainContent_GlobalInfoDocModal').modal('show');
            tinymce.get("GlobalInfoDocValue").setContent('');
            setTimeout(function () {
                //CKEDITOR.instances['GlobalInfoDocValue'].insertText(text);
                tinymce.get("GlobalInfoDocValue").setContent(text);
            }, 1000);
        }
    }
}

function SetExpendedPopupElement(text, nodeId, type, step, location, guid, nodeType, nodeText, wrtText, iconcolorid, Wrttext, WrtnodeId, Wrttype, Wrtstep, Wrtlocation, Wrtguid, WrtnodeType, WrtnodeText, WrtWrtText, Wrticoncolorid) {
    var decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
    var decodeurlWrtTxt = 'decodeURIComponent("' + encodeURIComponent(Wrttext) + '")';
    var decodeurlWrtnodeText = 'decodeURIComponent("' + encodeURIComponent(WrtnodeText) + '")';

    var headerinfodoc = document.getElementById("headerinfodoc");
    var TextInfo = document.getElementById("TextInfo");
    var infodoc_a = document.getElementById("infodoc_a");
    var OldWrticoncolorid = Wrticoncolorid;
    var WrtEditorFun = "";
    var InfoDocEditorFun = "";

    node_type_text = nodeType;

    if (nodeType == 'right-node') {
        headerinfodoc.innerHTML = text;
        TextInfo.innerHTML = nodeText;
        decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
        nodeText = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
    }
    else {
        if (nodeType == "left-text-node") {
            headerinfodoc.innerHTML = nodeText;
            TextInfo.innerHTML = text;
            decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
            nodeText = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
        }
        else {
            if (nodeType == 'left-node') {
                headerinfodoc.innerHTML = nodeText;
                TextInfo.innerHTML = text;
                decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
                nodeText = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
            }
            else {
                if (nodeType == null) {
                    if (wrtText == true) {
                        headerinfodoc.innerHTML = nodeText;
                        TextInfo.innerHTML = text;
                        decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
                        nodeText = 'decodeURIComponent("' + encodeURIComponent(text) + '")';

                    }
                    else {
                        if (wrtText == false) {
                            headerinfodoc.innerHTML = text;
                            TextInfo.innerHTML = nodeText;
                            decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
                            nodeText = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';

                        }
                        else {
                            headerinfodoc.innerHTML = text;
                            TextInfo.innerHTML = nodeText;
                            decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
                            nodeText = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
                        }

                    }
                }
                else {
                    headerinfodoc.innerHTML = nodeText;
                    TextInfo.innerHTML = text;
                    decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
                    nodeText = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
                }

            }
        }
    }

    if (WrtnodeType == 'wrt-text-node' || WrtnodeType == 'wrt-right-node') {
        decodeurlWrtnodeText = 'decodeURIComponent("' + encodeURIComponent(Wrttext) + '")';
        Wrticoncolorid = 'decodeURIComponent("' + encodeURIComponent(Wrticoncolorid) + '")';
        if (WrtnodeType == 'wrt-right-node') {
            /*  WrtWrtText = WrtnodeText;*/
            /*     Wrtguid = null;*/
            WrtEditorFun = 'showInfoPopup("' + WrtnodeText + '",' + WrtnodeId + ',' + Wrttype + ',' + Wrtstep + ',' + Wrtlocation + ',"' + Wrtguid + '","' + WrtnodeType + '",' + decodeurlWrtnodeText + ',' + WrtWrtText + ',' + Wrticoncolorid + ')';

        }
        else {
            WrtEditorFun = 'showInfoPopup(' + decodeurlWrtnodeText + ',' + WrtnodeId + ',' + Wrttype + ',' + Wrtstep + ',' + Wrtlocation + ',"' + Wrtguid + '","' + WrtnodeType + '","' + WrtnodeText + '",' + WrtWrtText + ',' + Wrticoncolorid + ')';
        }

    }
    else {

        if (WrtnodeType == 'wrt-Left-node') {
            /*  WrtWrtText = WrtnodeText;*/
            /*     Wrtguid = null;*/
            WrtEditorFun = 'showInfoPopup("' + WrtnodeText + '",' + WrtnodeId + ',' + Wrttype + ',' + Wrtstep + ',' + Wrtlocation + ',"' + Wrtguid + '","' + WrtnodeType + '",' + decodeurlWrtnodeText + ',' + WrtWrtText + ',' + Wrticoncolorid + ')';

        }
        else {
            WrtEditorFun = 'showInfoPopup(' + decodeurlWrtnodeText + ',' + WrtnodeId + ',' + Wrttype + ',' + Wrtstep + ',' + Wrtlocation + ',"' + Wrtguid + '","' + WrtnodeType + '",' + decodeurlWrtTxt + ',"' + WrtWrtText + '","' + Wrticoncolorid + '")';
        }
    }

    if (nodeType == null) {
        InfoDocEditorFun = 'showInfoPopup(' + nodeText.replace(/(\r\n|\n|\r)/gm, "") + ',' + nodeId + ',' + type + ',' + step + ',' + location + ',"' + guid + '",' + nodeType + ',' + decodeurlInfoDocTxt + ',"' + wrtText + '","' + iconcolorid + '")';

    }
    else {
        InfoDocEditorFun = 'showInfoPopup(' + nodeText.replace(/(\r\n|\n|\r)/gm, "") + ',' + nodeId + ',' + type + ',' + step + ',' + location + ',"' + guid + '","' + nodeType + '",' + decodeurlInfoDocTxt + ',"' + wrtText + '","' + iconcolorid + '")';
    }

    Wrticoncolorid = OldWrticoncolorid;
    infodoc_a.setAttribute('onclick', InfoDocEditorFun);
    var HeaderWRT = document.getElementById("HeaderWRT");
    var TextWRT = document.getElementById("TextWRT");
    var WRT_a = document.getElementById("WRT_a");
    if (WrtnodeType == 'parent-node1') {
        HeaderWRT.innerHTML = Wrttext;
        TextWRT.innerHTML = WrtnodeText;
    }
    else {
        if (WrtnodeType == 'wrt-left-node') {
            HeaderWRT.innerHTML = 'With respect to "' + WrtWrtText + '"';
            TextWRT.innerHTML = Wrttext;
        }
        else {
            if (WrtnodeType == 'wrt-left-node1' || WrtnodeType == 'wrt-right-node1') {
                HeaderWRT.innerHTML = 'With respect to "' + WrtWrtText + '"';
                TextWRT.innerHTML = Wrttext;
            }
            else {
                if (WrtnodeType == 'wrt-text-node') {
                    HeaderWRT.innerHTML = 'With respect to "' + Wrticoncolorid + '"';
                    TextWRT.innerHTML = Wrttext;
                }
                else
                    if (WrtnodeType == 'wrt-right-node') {
                        HeaderWRT.innerHTML = 'With respect to "' + Wrticoncolorid + '"';
                        TextWRT.innerHTML = Wrttext;
                    }
                    else {
                        HeaderWRT.innerHTML = WrtnodeText;
                        TextWRT.innerHTML = Wrttext;

                    }
            }
        }
    }
    var isMobile = regexp.test(details);
    if (isMobile) {
        HideEmptyInfoDocCol(TextWRT, TextInfo);
    }
    else {
        var cookieValue = $.cookie('evaluatorView');
        if (cookieValue != undefined && cookieValue != null && cookieValue.trim() == 'true') {
            HideEmptyInfoDocCol(TextWRT, TextInfo);
        }
        else {
            if (!output.isPM) {
                HideEmptyInfoDocCol(TextWRT, TextInfo);
            }
            else {
                if (TextWRT.innerHTML == '') {
                    $("#Infodocsidedata").removeClass('w-100');
                    TextWRT.innerHTML = 'No Data';
                }
                if (TextInfo.innerHTML == '') {
                    $("#infodocwrtsidedata").removeClass('w-100');
                    TextInfo.innerHTML = 'No Data';
                }
                $("#infodocwrtsidedata").show();
                $("#Infodocsidedata").show();
            }
        }
    }
    WRT_a.setAttribute('onclick', WrtEditorFun);
}
function HideEmptyInfoDocCol(TextWRT, TextInfo) {
    $("#infodocwrtsidedata").removeClass("w-100").show();
    $("#Infodocsidedata").removeClass("w-100").show();
    if (TextWRT.innerHTML == '') {
        $("#infodocwrtsidedata").hide();
        $("#Infodocsidedata").addClass('w-100');
    }
    if (TextInfo.innerHTML == '') {
        $("#Infodocsidedata").hide();
        $("#infodocwrtsidedata").addClass('w-100');
    }
}
function Expandtxt(text, nodeId, type, step, location, guid, nodeType, nodeText, wrtText, iconcolorid) {

    var decodeurlInfoDocTxt = 'decodeURIComponent("' + encodeURIComponent(text) + '")';
    var decodeurlNodeText = 'decodeURIComponent("' + encodeURIComponent(nodeText) + '")';
    var InfoDocEditorFun = "";
    if (nodeType == 'parent-node1') {
        InfoDocEditorFun = 'showInfoPopup(' + decodeurlNodeText + ',' + nodeId + ',' + type + ',' + step + ',' + location + ',' + guid + ',"' + nodeType + '",' + decodeurlInfoDocTxt + ',"' + wrtText + '","' + iconcolorid + '")';
    }
    else {
        InfoDocEditorFun = 'showInfoPopup(' + decodeurlInfoDocTxt + ',' + nodeId + ',' + type + ',' + step + ',' + location + ',' + guid + ',"' + nodeType + '",' + decodeurlNodeText + ',"' + wrtText + '","' + iconcolorid + '")';
    }
    /*var InfoDocEditorFun = 'showInfoPopup("' + text.replace(/(\r\n|\n|\r)/gm, "") + '",' + nodeId + ',' + type + ',' + step + ',' + location + ',' + guid + ',"' + nodeType + '","' + nodeText + '","' + wrtText + '","' + iconcolorid + '")';*/
    var h_headertxt = document.getElementById("h_headertxt");
    var HText = document.getElementById("HText");
    var h_a = document.getElementById("h_a");
    h_headertxt.innerHTML = text;
    HText.innerHTML = nodeText;
    h_a.setAttribute('onclick', InfoDocEditorFun);
}
//function showInfoPopup(text, type, location, step, nodeId, guid) {
function showInfoPopup(text, nodeId, type, step, location, guid, nodeType, nodeText, wrtText, iconcolorid) {
    $(".hideshowinfo").css('display', 'none');
    //$('.infocolor' + iconcolorid).removeClass('infoColor');
    if (nodeType == undefined || nodeType == null || nodeType == '' || nodeType == 'parent-node1' || nodeType == 'wrt-text-node' || nodeType == 'left-text-node') {
        node_type = type;
        node_location = location;
        current_step = step;
        node_id = nodeId;
        node_guid = guid;
        if (nodeType == 'wrt-text-node') {
            $("#exampleModalLongTitle").html('Edit ' + nodeText + ' with respect to "' + iconcolorid + '"');
            //hideWrapper(0, $('#irr_' + wrtText));
            //hideWrapper(0, $('#ilr_' + wrtText));
            activeRow(wrtText, false);
        }
        else
            if (nodeType == 'left-text-node') {
                $("#exampleModalLongTitle").html('Edit Description/definition for "' + nodeText + '"');
                //hideWrapper(0, $('#ir_' + wrtText));
                //hideWrapper(0, $('#il_' + wrtText));
                activeRow(wrtText, false);
            }
            else {
                if (nodeType == null) {
                    $("#exampleModalLongTitle").html('Edit Description/Definition for "' + nodeText + '"');
                }
                else {
                    $("#exampleModalLongTitle").val('Edit Description/Definition for "Price"');
                }
            }

        if (nodeType == 'parent-node1') {
            $("#exampleModalLongTitle").html('Edit Description/Definition for "' + nodeText + '"');
        }
        $('#MainContent_GlobalInfoDocModal').modal('show');
        //CKEDITOR.instances.GlobalInfoDocValue.setData('');
        tinymce.get("GlobalInfoDocValue").setContent('');
        setTimeout(function () {
            //CKEDITOR.instances[** fieldname **].setData(** your data **)
            // CKEDITOR.instances['GlobalInfoDocValue'].setData(text);
            tinymce.get("GlobalInfoDocValue").setContent(text);
        }, 500);
    }
    else {
        node = nodeType;
        node_type = type;
        node_location = location;
        current_step = hdnCurrentStep;
        node_id = nodeId;
        node_guid = guid;
        //parent node
        if (nodeType == 'parent-node') {
            $("#exampleModalLongTitle").html('Edit description/definition for "' + text + '"');
        }
        //first node
        if (nodeType == 'left-node') {
            $("#exampleModalLongTitle").html('Edit description/definition for "' + text + '"');
        }
        //wrt first node
        if (nodeType == 'wrt-left-node1' || nodeType == 'wrt-right-node1') {
            $("#exampleModalLongTitle").html('Edit ' + text + ' with respect to "' + wrtText + '"');
            /*  $("#exampleModalLongTitle").html('Edit ' + text);*/
        }
        if (nodeType == 'wrt-left-node') {
            $("#exampleModalLongTitle").html('Edit ' + text + ' with respect to "' + wrtText + '"');
            /* $("#exampleModalLongTitle").html('Edit ' + nodeText + ' with respect to "' + iconcolorid + '"');*/
        }
        //second node
        if (nodeType == 'right-node') {
            $("#exampleModalLongTitle").html('Edit description/definition for "' + text + '"');
        }
        //wrt second node
        if (nodeType == 'wrt-right-node') {
            $("#exampleModalLongTitle").html('Edit ' + text + ' with respect to "' + iconcolorid + '"');
            //$("#exampleModalLongTitle").html('Edit ' + nodeText + ' with respect to "' + wrtText + '"');
        }
        //scale-node
        if (nodeType == 'scale-node') {
            $("#exampleModalLongTitle").html('Edit description for measurement scale "' + text + '"');
        }
        //question-node
        if (nodeType == 'question-node') {
            $("#exampleModalLongTitle").html('Edit content');
        }

        //$("#exampleModalLongTitle").html('Edit Description/Definition for "Price"');
        $('#MainContent_GlobalInfoDocModal').modal('show');
        //CKEDITOR.instances.GlobalInfoDocValue.setData('');
        tinymce.get("GlobalInfoDocValue").setContent('');
        setTimeout(function () {
            // CKEDITOR.instances['GlobalInfoDocValue'].setData(nodeText);          
            tinymce.get("GlobalInfoDocValue").setContent(nodeText);
        }, 500);
        if (tinymce.get("GlobalInfoDocValue_ifr") !== null)
            tinymce.get("GlobalInfoDocValue_ifr").setContent('');
        setTimeout(function () {
            // CKEDITOR.instances['GlobalInfoDocValue'].setData(nodeText); 
            if (tinymce.get("GlobalInfoDocValue_ifr") !== null)
                tinymce.get("GlobalInfoDocValue_ifr").setContent(nodeText);
        }, 500);

    }
}


function toggleBox(id, chart) {
    //$(".hideshowinfo").css('display', 'none');
    if (chart == undefined || chart == null || chart == '') {
        $("#toggleComment_" + id).toggle(); //.css('display', 'block');
    }

    //if (id != undefined && id != null)
    // activeRow(id, false);
}

function showComment(id) {
    $("#divComment_" + id).css("display", "block");
}

function showCommentWrapper(id) {

}

function showWrapper(id) {
    $(".hideshowinfo").css('display', 'none');
    $("#" + id.id).toggle(); //.css('display', 'block');
    //$("div").css("display", "block")
}
var PrevParentnodeID = 0;

function showNodeMobile(id, index, type) {
    if (output.page_type == 'atPairwise') {
        if (is_comment_updated) {
            $('#txtRightComment').val(comment);
        }
        else {
            $('#txtRightComment').val(output.comment);
        }
    }
    else {
        if (output.page_type == 'atNonPWOneAtATime') {
            if (is_comment_updated) {
                $('#txtRightComment').val(comment);
            }
            else {
                $('#txtRightComment').val(output.comment);
            }
        }

    }
    $(".hideshowinfo").css('display', 'none');
    $(".parentnodemr").css('display', 'none');
    PrevParentnodeID = id;
    PrevIntensityID = index;
    if (id != null && id != '' && typeof id != 'undefined') {
        //var ele = $(".parentnode" + id);
        //$("#parentnode" + id).toggle();
        //ele.css('display', 'block');
        $('#comment_mobile').modal('show');
        if (index != undefined) {
            if (hdnpairwise_type == '') {
                $('#btnUpdateRightSideComment').attr('onclick', 'SaveDirectCommentMobile(' + index + ')');
            }
            else {
                $('#btnUpdateRightSideComment').attr('onclick', 'hideNodeModel(3' + index + ', ' + index + ')');
            }
        }
        else {
            if (hdnpairwise_type == '') {
                $('#btnUpdateRightSideComment').attr('onclick', 'SaveDirectCommentMobile(' + id + ')');
            }
            else {
                $('#btnUpdateRightSideComment').attr('onclick', 'hideNodeModel(' + id + ', ' + id + '); comment_updated_mobile();');
            }

        }
    }
    else {
        if (type == undefined || type == null || type == '') {
            /*$("#parentnode").toggle();*/
            $('#comment_mobile').modal('show');
            $('#btnUpdateRightSideComment').attr('onclick', 'hideNodeModel(3' + node + ', ' + type + ')');
        }
        else {
            //$('#' + type).show();
            $('#comment_mobile').modal('show');
            $('#btnUpdateRightSideComment').attr('onclick', 'hideNodeModel(3' + node + ', ' + type + '); comment_updated_mobile();');
        }
    }
    if (parseInt(index) >= 0) {
        /* multi_pw_data[index].Comment = $('#multi_comment_' + index).val();*/
        $('#txtRightComment').val(multi_pw_data[index].Comment);
        $('#multi_comment_icon_' + index).removeClass('far fa-comments');
        $('#multi_comment_icon_' + index).removeClass('fa fa-comments');

        if ($('#txtRightComment').val() != '') {
            $('#multi_comment_icon_' + index).addClass('fa fa-comments');
        }
        else {
            $('#multi_comment_icon_' + index).addClass('far fa-comments');
        }
    }
    else {
        $('#single_comment_icon').removeClass('far fa-comments');
        $('#single_comment_icon').removeClass('fa fa-comments');
        //if ($('#single_comment').val() != '') {
        //    $('#single_comment_icon').addClass('fa fa-comments');
        //}
        //else {
        //    $('#single_comment_icon').addClass('far fa-comments');
        //}
    }
    editbuttonClick = true;
}

function showNode(id, index, type) {
    if (output.page_type == 'atPairwise') {
        if (is_comment_updated) {
            $('#single_comment').val(comment);
        }
        else {
            $('#single_comment').val(output.comment);
        }
    }
    else {
        if (output.page_type == 'atNonPWOneAtATime') {
            if (is_comment_updated) {
                $('#single_comment').val(comment);
            }
            else {
                $('#single_comment').val(output.comment);
            }
        }

    }
    $(".hideshowinfo").css('display', 'none');
    $(".parentnodemr").css('display', 'none');
    PrevParentnodeID = id;
    PrevIntensityID = index;
    if (id != null && id != '' && typeof id != 'undefined') {
        var ele = $(".parentnode" + id);
        $("#parentnode" + id).toggle();
        ele.css('display', 'block');
    }
    else {
        if (type == undefined || type == null || type == '') {
            $("#parentnode").toggle();
        }
        else {
            $('#' + type).show();
        }
    }
    if (parseInt(index) >= 0) {
        /* multi_pw_data[index].Comment = $('#multi_comment_' + index).val();*/
        $('#multi_comment_' + index).val(multi_pw_data[index].Comment);
        $('#multi_comment_icon_' + index).removeClass('far fa-comments');
        $('#multi_comment_icon_' + index).removeClass('fa fa-comments');

        if ($('#multi_comment_' + index).val() != '') {
            $('#multi_comment_icon_' + index).addClass('fa fa-comments');
        }
        else {
            $('#multi_comment_icon_' + index).addClass('far fa-comments');
        }
    }
    else {
        $('#single_comment_icon').removeClass('far fa-comments');
        $('#single_comment_icon').removeClass('fa fa-comments');
        //if ($('#single_comment').val() != '') {
        //    $('#single_comment_icon').addClass('fa fa-comments');
        //}
        //else {
        //    $('#single_comment_icon').addClass('far fa-comments');
        //}
    }
    editbuttonClick = true;
}

function hideNodeModel(id, index) {
    if (id != '' && typeof id != 'undefined')
        //$("#parentnode" + id).css('display', 'none');
        $('#comment_mobile').modal('hide');
    else
        //$("#parentnode").css('display', 'none');
        $('#comment_mobile').modal('hide');
    if (output.page_type == 'atPairwise') {
        if (index == undefined) {
            $('#single_comment').val('');
        }
    }
    else {
        if (parseInt(index) >= 0) {
            multi_pw_data[index].Comment = $('#txtRightComment').val();
            $('#multi_comment_icon_' + index).removeClass('far fa-comments');
            $('#multi_comment_icon_' + index).removeClass('fa fa-comments');
            if ($('#txtRightComment').val() != '') {
                $('#multi_comment_icon_' + index).addClass('fa fa-comments');
            }
            else {
                $('#multi_comment_icon_' + index).addClass('far fa-comments');
            }
        }
    }
    if (index != undefined) {
        if (!isMobile)
            HideShowComment_icon(index);
        else
            HideShowComment_icon_mobile(index);
    }
    //$("div").css("display", "block")
}

function hideNode(id, index) {
    if (id != '' && typeof id != 'undefined')
        $("#parentnode" + id).css('display', 'none');
    else
        $("#parentnode").css('display', 'none');
    if (output.page_type == 'atPairwise') {
        if (index == undefined) {
            $('#single_comment').val('');
        }
    }
    else {
        if (parseInt(index) >= 0) {
            multi_pw_data[index].Comment = $('#multi_comment_' + index).val();
            $('#multi_comment_icon_' + index).removeClass('far fa-comments');
            $('#multi_comment_icon_' + index).removeClass('fa fa-comments');
            if ($('#multi_comment_' + index).val() != '') {
                $('#multi_comment_icon_' + index).addClass('fa fa-comments');
            }
            else {
                $('#multi_comment_icon_' + index).addClass('far fa-comments');
            }
        }
    }
    if (index != undefined) {
        if (!isMobile)
            HideShowComment_icon(index);
        else
            HideShowComment_icon_mobile(index);
    }
    //$("div").css("display", "block")
}

function hideWrapper(id, newid) {
    if (id == 0) {
        $("#" + newid[0].id).css('display', 'none');
    }
    else {
        $("#" + id.id).css('display', 'none');
    }
    //$("div").css("display", "block")
}
function saveHeaderContent() {
    $("#overlay").css('display', 'flex');
    //var editors_content = CKEDITOR.instances['GlobalInfoDocValue'].getData();
    // var editors_content = tinymce.get('GlobalInfoDocValue').getContent({ format: "text" });
    var editors_content = tinymce.get('GlobalInfoDocValue1').getContent();
    node_id = node_id == null ? "0" : node_id;
    node_guid = node_guid == null ? "" : node_guid;


    Guids = '';
    if (node_location == 0) {
        $('.headerNodesCheckbox').each(function () {
            if ($(this).is(':checked')) {
                Guids += $(this).val() + ',';
            }
        });
    }
    else if (node_location == -1) {
        $('.titleNodesCheckbox').each(function () {
            if ($(this).is(':checked')) {
                Guids += $(this).val() + ',';
            }
        });
    }
    if (Guids != '' && Guids.length > 0)
        Guids = Guids.replace(/(\s*,?\s*)*$/, "");

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveInfoDocs",
        data: JSON.stringify({
            nodetxt: editors_content,
            obj: node_type,
            node: node_location,
            current_step: hdnCurrentStep,
            node_id: node_id,
            node_guid: node_guid,
            Guids: Guids
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        beforeSend: function () {
            $('.tt-GlobalInfoDoc-status').html('<div data-alert="" class="alert alert-success alert-dismissible fade show">           Saving...              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        },
        success: function (data) {
            if (node_type == -1) {
                node_name = "parent-node";
            }
            else if (node_type == 2) {
                if (node_location == 1) {
                    node_name = "left-node";
                }
                else {
                    node_name = "right-node";
                }
            }
            else if (node_type == 3) {
                if (node_location == 1) {
                    node_name = "wrt-left-node";
                }
                else {
                    node_name = "wrt-right-node";
                }
            }
            else if (node_type == 4) {
                node_name = "scale-node";
            }
            else {
                node_name = "question-node";
            }

            if (hdnpage_type == 'atNonPWAllChildren') {
                save_multi_ratings_on_next();
            }

            tinymce.activeEditor.windowManager.close(this);
            $('#MainContent_GlobalInfoDocModal1').modal('hide');
            setTimeout(function () {
                $('#MainContent_hdnPageNo').trigger('click');
            }, 10);
        },
        complete: function () {
            $('.tt-GlobalInfoDoc-status').html('<div data-alert="" class="alert alert-primary alert-dismissible fade show">           Save              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        },
        error: function (response) {
            $("#overlay").css('display', 'none');
        }
    });
    //$("#overlay").css('display', 'none');
    block_unload_prompt = true;
}
var CurrrentSelectedRow = -1;
function saveContent() {
    $('#MainContent_GlobalInfoDocModal').modal('hide');

    $('#infodocPop').modal('hide');

     $("#overlay").css('display', 'flex');
    //var editors_content = CKEDITOR.instances['GlobalInfoDocValue'].getData();
    // var editors_content = tinymce.get('GlobalInfoDocValue').getContent({ format: "text" });
    $.cookie("GenActiveRowID", CurrrentSelectedRow);
    var editors_content = tinymce.get('GlobalInfoDocValue').getContent();
    node_id = node_id == null ? "0" : node_id;
    node_guid = node_guid == null ? "" : node_guid;
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveInfoDocs",
        data: JSON.stringify({
            nodetxt: editors_content,
            obj: node_type,
            node: node_location,
            current_step: hdnCurrentStep,
            node_id: node_id,
            node_guid: node_guid,
            Guids: Guids
        }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        beforeSend: function () {
            $('.tt-GlobalInfoDoc-status').html('<div data-alert="" class="alert alert-success alert-dismissible fade show">           Saving...              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        },
        success: function (data) {
            if (node_type == -1) {
                node_name = "parent-node";
            }
            else if (node_type == 2) {
                if (node_location == 1) {
                    node_name = "left-node";
                }
                else {
                    node_name = "right-node";
                }
            }
            else if (node_type == 3) {
                if (node_location == 1) {
                    node_name = "wrt-left-node";
                }
                else {
                    node_name = "wrt-right-node";
                }
            }
            else if (node_type == 4) {
                node_name = "scale-node";
            }
            else {
                node_name = "question-node";
            }

            if (hdnpage_type == 'atNonPWAllChildren') {
                save_multi_ratings_on_next();
            }
            $("#overlay").css('display', 'flex');
            $('#MainContent_hdnPageNo').trigger('click');
            //setTimeout(function () {
            //    if (node_type == 2) {
            //        $('#exampleModal').modal('hide');

            //        var encodedNodeData = encodeURIComponent(editors_content);
            //        if (output.multi_pw_data[node_id !== undefined]) {
            //            var encodedWRTLeft = encodeURIComponent(output.multi_pw_data[node_id].InfodocLeftWRT);
            //            var encodedWRTRight = encodeURIComponent(output.multi_pw_data[node_id].InfodocRightWRT);
            //        }
            //        var encodedParentNodeData = encodeURIComponent(output.parent_node)

            //        if (hdnpage_type === 'atAllPairwise' && node_type_text === 'left-text-node') {

            //            var onclick = 'SetExpendedPopupElement(decodeURIComponent("' + encodedNodeData + '"),"' + output.multi_pw_data[node_id].NodeID_Left + '",2,2,1,null, "left-text-node","' + output.multi_pw_data[node_id].LeftNode + '","' + node_id + '", decodeURIComponent("' + encodedParentNodeData + '"),decodeURIComponent("' + encodedWRTLeft + '"),"' + output.multi_pw_data[node_id].NodeID_Left + '",3,2,1,null, "wrt-text-node","' + output.multi_pw_data[node_id].LeftNode + '","' + node_id + '",decodeURIComponent("' + encodedParentNodeData + '"))';

            //            //$('#index_' + node_id).find('.left_info').find('.body_content')[0].innerHtml = '<span>' + editors_content + '</span>';
            //            $('#index_' + node_id).find('.left_info').find('.body_content')[0].innerHTML = '<div class="body_content"><span>' + editors_content + '</span></div>';
            //            $("#btnWRT_" + node_id).attr("onclick", onclick);

            //            if (node_id > 0) {
            //                $('#index_' + (node_id - 1)).find('.right_info').find('.body_content')[0].innerHTML = '<div class="body_content"><span>' + editors_content + '</span></div>';

            //                $("#btnWRTRight_" + (node_id - 1)).attr("onclick", onclick);
            //            }
            //        }
            //        else if (hdnpage_type === 'atAllPairwise' && node_type_text === 'right-node') {

            //            var onclick = 'SetExpendedPopupElement(decodeURIComponent("' + encodedNodeData + '"),"' + $('#index_' + output.multi_pw_data[node_id].NodeID_Right).find('.right_info').find('.body_content')[0].innerHTML + '",2,2,1,null, "right-node","' + output.multi_pw_data[node_id].RightNode + '","' + node_id + '", decodeURIComponent("' + encodedParentNodeData + '"),decodeURIComponent("' + encodedWRTRight + '"),"' + output.multi_pw_data[node_id].NodeID_Right + '",3,2,1,null, "wrt-text-node","' + output.multi_pw_data[node_id].RightNode + '","' + node_id + '",decodeURIComponent("' + encodedParentNodeData + '"))';

            //            $('#index_' + output.multi_pw_data[node_id].NodeID_Right).find('.right_info').find('.body_content')[0].innerHTML = '<div class="body_content"><span>' + editors_content + '</span></div>';

            //            $("#btnWRTRight_" + output.multi_pw_data[node_id].NodeID_Right).attr("onclick", onclick);

            //            $('#index_' + (output.multi_pw_data[node_id].NodeID_Right + 1)).find('.left_info').find('.body_content')[0].innerHTML = '<div class="body_content"><span>' + editors_content + '</span></div>';

            //            $("#btnWRT_" + (output.multi_pw_data[node_id].NodeID_Right + 1)).attr("onclick", onclick);

            //        }
            //        else if (hdnpage_type === 'atNonPWAllChildren') {
            //            $('#div_row_wrapper_' + node_id).find('.body_content')[0].innerHTML = '<div class="body_content"><span>' + editors_content + '</span></div>';
            //            $('#MainContent_hdnPageNo').trigger('click');
            //        }
            //        else {
            //            $('#MainContent_hdnPageNo').trigger('click');
            //        }
            //    }
            //    else if (node_type == 3) {
            //        $('#exampleModal').modal('hide');
            //        if (output.multi_pw_data[node_id] !== undefined) {
            //            var encodedInfoDocLeft = encodeURIComponent(output.multi_pw_data[node_id].InfodocLeft);
            //            var encodedInfoDocRight = encodeURIComponent(output.multi_pw_data[node_id].InfodocRight);
            //        }
            //        var onclick = '';

            //        if (hdnpage_type === 'atAllPairwise' && node_type_text === 'left-text-node') {
            //            var encodedUpdatedWRT = encodeURIComponent(editors_content);
            //            onclick = 'SetExpendedPopupElement(decodeURIComponent("' + encodedInfoDocLeft + '"),"' + output.multi_pw_data[node_id].NodeID_Left + '","2","2","1",null, "left-text-node" ,decodeURIComponent(encodeURIComponent("' + output.multi_pw_data[node_id].LeftNode + '")),"' + node_id + '", decodeURIComponent(encodeURIComponent("' + output.parent_node + '")),decodeURIComponent("' + encodedUpdatedWRT + '"),"' + output.multi_pw_data[node_id].NodeID_Left + '","3","2","1",null,"wrt-text-node",decodeURIComponent(encodeURIComponent("' + output.multi_pw_data[node_id].LeftNode + '")),"' + node_id + '",decodeURIComponent(encodeURIComponent("' + output.parent_node + '")))';
            //            console.log('onclick: ' + onclick);

            //            $("#btnWRT_" + node_id).attr("onclick", onclick);
            //            $("#btnWRTRight_" + (node_id-1)).attr("onclick", onclick);

            //        }

            //        else if (hdnpage_type === 'atAllPairwise' && node_type_text === 'right-node') {

            //            onclick = 'SetExpendedPopupElement(decodeURIComponent("' + encodedInfoDocRight + '"),"' + output.multi_pw_data[node_id].NodeID_Right + '","2","2","1",null, "right-node" ,decodeURIComponent(encodeURIComponent("' + output.multi_pw_data[node_id].RightNode + '")),"' + node_id + '", decodeURIComponent(encodeURIComponent("' + output.parent_node + '")),(decodeURIComponent("' + encodedInfoDocRight + '"),"' + output.multi_pw_data[node_id].NodeID_Right + '","3","2","1",null,"wrt-text-node",decodeURIComponent(encodeURIComponent("' + output.multi_pw_data[node_id].RightNode + '")),"' + node_id + '",decodeURIComponent(encodeURIComponent("' + output.parent_node + '")))';
            //            console.log('onclick: ' + onclick);

            //            $("#btnWRTRight_" + node_id).attr("onclick", onclick);
            //            $("#btnWRT_" + (node_id + 1)).attr("onclick", onclick);
            //        }
            //        else if (hdnpage_type === 'atNonPWAllChildren') {

            //            onclick = 'SetExpendedPopupElement(decodeURIComponent(encodeURIComponent("' + output.multi_non_pw_data[node_id].Infodoc.replace('<p>', '').replace('</p>', '') + '")), "' + node_id + '","2","0","1","' + node_guid + '","left-node",decodeURIComponent(encodeURIComponent("' + output.multi_non_pw_data[node_id].Title + '")),null,null,encodeURIComponent("' + editors_content.replaceAll('<p>', '').replaceAll('</p>', '') + '"),"' + node_id + '","3","0","1","' + node_guid + '", "wrt-left-node", decodeURIComponent(encodeURIComponent("' + output.multi_non_pw_data[node_id].Title + '")), decodeURIComponent(encodeURIComponent("' + output.parent_node + '")),' + node_id + ')';
            //            console.log('onclick: ' + onclick);

            //            $("#btnWRT_" + node_id).attr("onclick", onclick);
            //        }
            //        else {
            //            $('#MainContent_hdnPageNo').trigger('click');
            //        }
            //    }
            //    else
            //        $('#MainContent_hdnPageNo').trigger('click');
            //}, 10);
        },
        complete: function () {
            $('.tt-GlobalInfoDoc-status').html('<div data-alert="" class="alert alert-primary alert-dismissible fade show">           Save              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        },
        error: function (response) {
        }
    });
    $("#overlay").css('display', 'none');
    block_unload_prompt = true;
    $('#MainContent_hdnPagePostBack').val('True');
}

var save_pairwise = function (value, advantage, chartType, chartTypeValue) {
    $("#overlay").css('display', 'flex');
    $("#divRatingEQ").removeClass("selcted_spot");
    if (chartType != undefined && chartType != null) {
        if (chartType == 'left') {
            $('.spnleft').text(chartTypeValue.title);
            $('#divleft').css('display', 'inline-block');
            $('#divequal').css('display', 'none');
            $('#divright').css('display', 'none');
            $('.spnequal').text('');
            $('.spnright').text('');

            $('div.rating_content_right').removeClass('selected_hover_right');
            $('div.rating_content_right').removeClass('selected_right1');
            $('div.rating_content_right').removeClass("selcted_spot");

            $('div.rating_content_left').removeClass('selected_hover_left');
            $('div.rating_content_left').removeClass('selected_left1');
            $('div.rating_content_left').removeClass('selcted_spot');

            $('#div_rating_left_' + value).nextAll('.rating_content_left').addClass('selected_left1');
            $('#div_rating_left_' + value).addClass('selected_left1');
            $('#div_rating_left_' + value).addClass("selcted_spot");

            $('#divRatingEQ').removeClass('rating_content_left selected_left1');
            $('#divRatingEQ').removeClass('rating_content_right selected_right1');
            $('#divRatingEQ').removeClass('eqalizer_color');
            $('#divRatingEQ').addClass('rating_content_left selected_left1');
        }
        else if (chartType == 'right') {

            $('.spnleft').text('');
            $('#divleft').css('display', 'none');
            $('#divequal').css('display', 'none');
            $('#divright').css('display', 'inline-block');

            $('.spnequal').text('');
            if (chartTypeValue !== undefined) {
                $('.spnright').text(chartTypeValue.title);
            }

            $('div.rating_content_left').removeClass('selected_hover_left');
            $('div.rating_content_left').removeClass('selected_left1');
            $('div.rating_content_left').removeClass('selcted_spot');

            //$('#div_rating_right_' + value).prevAll('.rating_content_right').removeClass('selected_hover_right');
            //$('#div_rating_right_' + value).prevAll('.rating_content_right').removeClass('selected_right1');
            //$('#div_rating_right_' + value).removeClass('selected_hover_left');
            $('div.rating_content_right').removeClass('selected_hover_right');
            $('div.rating_content_right').removeClass('selected_right1');
            $('div.rating_content_right').removeClass("selcted_spot");

            $('#div_rating_right_' + value).addClass("selcted_spot");

            $('#div_rating_right_' + value).prevAll('.rating_content_right').addClass('selected_right1');
            $('#div_rating_right_' + value).addClass('selected_right1');



            $('#divRatingEQ').removeClass('rating_content_left selected_left1');
            $('#divRatingEQ').removeClass('rating_content_right selected_right1');
            $('#divRatingEQ').removeClass('eqalizer_color');
            $('#divRatingEQ').addClass('rating_content_right selected_right1');
        }
        else if (chartType == '') {
            $('.spnleft').text('');
            $('#divleft').css('display', 'none');
            $('#divequal').css('display', 'inline-block');
            $('#divright').css('display', 'none');
            $('.spnequal').text(chartTypeValue.title);
            $('.spnright').text('');

            $('div.rating_content_right').removeClass('selected_hover_right');
            $('div.rating_content_right').removeClass('selected_right1');
            $('div.rating_content_right').removeClass("selcted_spot");

            $('div.rating_content_left').removeClass('selected_hover_left');
            $('div.rating_content_left').removeClass('selected_left1');
            $('div.rating_content_left').removeClass('selcted_spot');

            $('#divRatingEQ').removeClass('selected_right1');
            $('#divRatingEQ').removeClass('selected_left1');
            $('#divRatingEQ').addClass('eqalizer_color');
            $('#divRatingEQ').addClass("selcted_spot");
        }
    }

    extremeMessage = output.extremeMessage;
    //outputvalue = $('#MainContent_hdnoutputvalue').val();
    outputvalue = parseFloat(value);
    pairwise_type = output.pairwise_type;
    //is_auto_advance = $('#MainContent_hdnis_auto_advance').val();

    advantages = parseFloat(advantage);
    //$scope.pw_advantage = advantage;
    //$scope.$parent.disableNext = $scope.output.pipeOptions.dontAllowMissingJudgment && !(parseFloat(value) > 0);

    if (!isExtremeMessage && outputvalue == 9 && pairwise_type != "ptGraphical" && output.pipeWarning != undefined && output.pipeWarning != null && output.pipeWarning != '') {
        //extremeMessage = true;
        isExtremeMessage = true;

        $('#MainContent_warningModalMessage').text(output.pipeWarning);
        $("#MainContent_warningModal").modal('show');
        return;
    }

    if (parseInt(value) != -2147483648000 && output.is_auto_advance && pairwise_type != "ptGraphical") {
        save_pairwise_on_next(output.is_auto_advance);
    }
    if (parseInt(advantages) == 0 && value <= 0 && pairwise_type == "ptGraphical") {
        var slider = getNoUiSlider(undefined);
        setTimeout(function () {
            slider.noUiSlider.set([400, 1200]);
            setNoUiColor(false, -1);
            $('#graphicalSlider').addClass('newsample1');
            $(".noUi-connect").css("background-color", "#AAAAAA");
            $(".graph-green-div").css("background-color", "#AAAAAA");
            $('.arrowicons').hide();

            main_bar[0] = "";
            main_bar[1] = "";
            sliderLeft = parseFloat(main_bar[0]);
            sliderRight = parseFloat(main_bar[1]);

            $('#noUiInput11').val(main_bar[0]);
            $('#noUiInput21').val(main_bar[1]);

            if ((main_bar[0] == '' || parseInt(main_bar[0]) <= 0) && (main_bar[1] == '' || parseInt(main_bar[1]) <= 0)) {
                $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
            }
            else {
                $('.chart_wrapper .imgClose').css({ 'filter': 'none', 'opacity': '1' })
            }
        }, 10);
        IsUndefined = true;
    }
    else {
        IsUndefined = false;
    }

    if (value == -2147483648000) {
        IsUndefined = true;
        $(".next_step").removeClass("back-pulse");
    }
    else {
        $(".next_step").addClass("back-pulse");
    }
    /*$('#MainContent_hdnIsUndefined').val(IsUndefined);*/
    output.IsUndefined = IsUndefined;
    output.advantage = advantage;
    output.value = value;

    //$('#MainContent_hdnadvantage').val(advantage);
    //$('#MainContent_hdnvalue').val(value);
    save_pairwise_on_next(advantage);
    $('.chart_wrapper .imgClose').css({ 'filter': 'none', 'opacity': '1' })

    setTimeout(function () {
        //$("#overlay").css('display', 'none');
    }, 800);
}

var closeWarningModal = function (modalName) {
    $("#" + modalName).modal('hide');

    /*if (modalName == "warningModal" && output.is_auto_advance && isExtremeMessage) {*/
    if (modalName == "warningModal" && isExtremeMessage) {
        //isExtremeMessage = false;
        save_pairwise(outputvalue, advantages);
    }
    block_unload_prompt = true;
    return false;
}

function leftBars() {
    //$(this).nextAll('.rating_content_left').removeClass('selected_hover_left');
    //$(this).removeClass('selected_hover_left');

    //$(this).nextAll('.rating_content_left').addClass('selected_hover_left');
    //$(this).addClass('selected_hover_left');
}

/*$('div.rating_content_left').on("mouseover", function (e) {*/
$(document).on("mouseover", "div.rating_content_left", function (e) {
    $(this).parents('.rating_content_right').removeClass('selected_hover_right');

    $(this).nextAll('.rating_content_left').removeClass('selected_hover_left');
    $(this).removeClass('selected_hover_left');

    $(this).nextAll('.rating_content_left').addClass('selected_hover_left');
    $(this).addClass('selected_hover_left');
    $('#divRatingEQ').addClass('selected_hover_left');
});

/*$('div.rating_content_left').on("mouseleave", function (e) {*/
$(document).on("mouseleave", "div.rating_content_left", function (e) {
    $(this).parents('.rating_content_right').removeClass('selected_hover_right');

    $(this).nextAll('.rating_content_left').removeClass('selected_hover_left');
    $(this).removeClass('selected_hover_left');
    $('#divRatingEQ').removeClass('selected_hover_left');
});

function rightBars() {
    //$(this).parents('.rating_content_right').removeClass('.selected_hover_right');
    //$(this).removeClass('rating_content_right');

    //$(this).parents('.rating_content_right').addClass('.selected_hover_right');
    //$(this).addClass('rating_content_right');
}

/*$('div.rating_content_right').on("mouseover", function (e) {*/
$(document).on("mouseover", "div.rating_content_right", function (e) {
    $(this).nextAll('.rating_content_left').removeClass('selected_hover_left');

    $(this).prevAll('.rating_content_right').removeClass('selected_hover_right');
    $(this).removeClass('selected_hover_right');

    $(this).prevAll('.rating_content_right').addClass('selected_hover_right');
    $(this).addClass('selected_hover_right');
    $('#divRatingEQ').addClass('selected_hover_right');
});

/*$('div.rating_content_right').on("mouseleave", function (e) {*/
$(document).on("mouseleave", "div.rating_content_right", function (e) {
    $(this).nextAll('.rating_content_left').removeClass('selected_hover_left');

    $(this).prevAll('.rating_content_right').removeClass('selected_hover_right');
    $(this).removeClass('selected_hover_right');
    $('#divRatingEQ').removeClass('selected_hover_right');
});

var save_pairwise_on_next = function (auto_advance) {
    $("#overlay").css('display', 'flex');
    var temp_auto_advance = false;
    if (typeof (auto_advance) != "undefined") {
        temp_auto_advance = auto_advance;
    }

    outputvalue = outputvalue != output.value ? output.value : outputvalue;
    var value = outputvalue == 0 ? -2147483648000 : outputvalue;
    var advantage = advantages != output.advantage ? output.advantage : advantages;

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SavePairwise",
        data: JSON.stringify({
            step: hdnCurrentStep,
            value: value,
            advantage: advantage,
            comments: comment,
            userId: 0
        }),
        async: false,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var fextreme = false;
            if (extremeMessage == 'false' && parseInt(outputvalue) == 9 && !temp_auto_advance && pairwise_type != "ptGraphical") {
                extremeMessage = true;
                fextreme = true;
            }
            if (output.pipeOptions.dontAllowMissingJudgment) {
                if (value <= 0) {
                    EnableDisablePagination(true);
                }
                else if (value > 0) {
                    if (enabledLastStep == hdnCurrentStep)
                        EnableDisablePagination(false, true);
                    else
                        EnableDisablePagination(true, false);
                }
            }

            //setPagination(activePageNo);
            if (!temp_auto_advance)
            loadStepsdata();
            if ($.cookie('currentstep')) {
                $.cookie('is_under_review', false);
                //$('#hdnPageNumber').val($.cookie('currentstep'));
                //$('#MainContent_hdnPageNo').trigger('click');
            }

            if ((output.page_type == 'atPairwise' && output.pairwise_type == 'ptVerbal') || (output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtRatings')) {
                if (output.is_auto_advance && parseInt(value) > 0) {
                    $('#hdnPageNumber').val(parseInt(hdnCurrentStep + 1));
                    setTimeout(function () {
                        $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                }
            }
        },
        error: function (response) {
        }
    });
}

var comment_updated_mobile = function (formatted_comment) {
    /*$("#comment-single").tooltip("hide"); //close tooltip*/
    $("#comment-single").toggle(); //close tooltip

    $('#parentnode15').css('display', 'none');
    $('#parentnode16').css('display', 'none');

    is_comment_updated = true;
    openModal = false;   //11646
    var comment_str = "";

    if (!is_multi) {

    }
    else {
        save_multis();
    }

    comment = $("#txtRightComment").val();
    //$('#single_comment_icon').removeClass('far fa-comments');
    //$('#single_comment_icon').removeClass('fa fa-comments');

    //if (comment.trim() != '') {
    //    $('#single_comment_icon').addClass('fa fa-comments');
    //}
    //else {
    //    $('#single_comment_icon').addClass('far fa-comments');
    //}

    if (comment.indexOf("@@") != -1) {
        comment = comment.split("@@");
        comment = comment[0];
    }

    if (hdnpage_type == "atPairwise") {
        if (is_comment_updated && outputvalue == 0) {
            outputvalue = -2147483648000;
        }
        // $scope.save_pairwise($scope.output.value, $scope.output.advantage);
        save_pairwise(output.value.toString(), output.advantage.toString());
    }

    var non_pw_type = output.non_pw_type;
    if (hdnpage_type == "atNonPWOneAtATime" && non_pw_type == "mtRatings") {
        $.each(output.intensities, function (index, intensity) {
            if (intensity[0] == output.non_pw_value) {
                save_ratings(intensity[2]);
                return false;
            }
        });
    }

    if (hdnpage_type == "atNonPWOneAtATime" && non_pw_type == "mtDirect") {
        // $scope.save_direct($scope.output.non_pw_value);
    }
    else if (hdnpage_type == "atNonPWOneAtATime" && non_pw_type == "mtStep") {
        // $scope.save_StepFunction($scope.step_input);
    }
    else if (hdnpage_type == "atNonPWOneAtATime" && (non_pw_type == "mtAdvancedUtilityCurve" || non_pw_type == "mtRegularUtilityCurve")) {
        // $scope.save_utility_curve($scope.utilityCurveInput);
    }

    //$timeout(function () {
    //    $scope.$apply(function () {
    //        $scope.formatted_comment = "";
    //    });
    //}, 1000);


    //$(document).foundation();

    //if ($scope.isMobile()) {
    //    $("#tt-c-modal").foundation("reveal", "close"); // 11532
    //} else {
    //    $("#comment-single").removeClass("open");    // 11532
    //}
    //var hdnadvantage = $('#MainContent_hdnadvantage').val();
    //var hdnvalue = $('#MainContent_hdnvalue').val();
    var chartTyp = '';
    if (output.advantage.toString() == '1')
        chartTyp = 'left';
    else if (output.advantage.toString() == '-1')
        chartTyp = 'right';
    else {
        chartTyp = undefined;
    }

    $('#comment-single').toggle();
    save_pairwise(output.value.toString(), output.advantage.toString(), chartTyp);
}

var comment_updated = function (formatted_comment) {
    /*$("#comment-single").tooltip("hide"); //close tooltip*/
    $("#comment-single").toggle(); //close tooltip

    $('#parentnode15').css('display', 'none');
    $('#parentnode16').css('display', 'none');

    is_comment_updated = true;
    openModal = false;   //11646
    var comment_str = "";

    if (!is_multi) {

    }
    else {
        save_multis();
    }

    comment = $("#single_comment").val();
    //$('#single_comment_icon').removeClass('far fa-comments');
    //$('#single_comment_icon').removeClass('fa fa-comments');

    //if (comment.trim() != '') {
    //    $('#single_comment_icon').addClass('fa fa-comments');
    //}
    //else {
    //    $('#single_comment_icon').addClass('far fa-comments');
    //}

    if (comment.indexOf("@@") != -1) {
        comment = comment.split("@@");
        comment = comment[0];
    }

    if (hdnpage_type == "atPairwise") {
        if (is_comment_updated && outputvalue == 0) {
            outputvalue = -2147483648000;
        }
        // $scope.save_pairwise($scope.output.value, $scope.output.advantage);
        save_pairwise(output.value.toString(), output.advantage.toString());
    }

    var non_pw_type = output.non_pw_type;
    if (hdnpage_type == "atNonPWOneAtATime" && non_pw_type == "mtRatings") {
        $.each(output.intensities, function (index, intensity) {
            if (intensity[0] == output.non_pw_value) {
                save_ratings(intensity[2]);
                return false;
            }
        });
    }

    if (hdnpage_type == "atNonPWOneAtATime" && non_pw_type == "mtDirect") {
        // $scope.save_direct($scope.output.non_pw_value);
    }
    else if (hdnpage_type == "atNonPWOneAtATime" && non_pw_type == "mtStep") {
        // $scope.save_StepFunction($scope.step_input);
    }
    else if (hdnpage_type == "atNonPWOneAtATime" && (non_pw_type == "mtAdvancedUtilityCurve" || non_pw_type == "mtRegularUtilityCurve")) {
        // $scope.save_utility_curve($scope.utilityCurveInput);
    }

    //$timeout(function () {
    //    $scope.$apply(function () {
    //        $scope.formatted_comment = "";
    //    });
    //}, 1000);


    //$(document).foundation();

    //if ($scope.isMobile()) {
    //    $("#tt-c-modal").foundation("reveal", "close"); // 11532
    //} else {
    //    $("#comment-single").removeClass("open");    // 11532
    //}
    //var hdnadvantage = $('#MainContent_hdnadvantage').val();
    //var hdnvalue = $('#MainContent_hdnvalue').val();
    var chartTyp = '';
    if (output.advantage.toString() == '1')
        chartTyp = 'left';
    else if (output.advantage.toString() == '-1')
        chartTyp = 'right';
    else {
        chartTyp = undefined;
    }

    $('#comment-single').toggle();
    save_pairwise(output.value.toString(), output.advantage.toString(), chartTyp);
}

var refreshRating = function () {
    $('div.rating_content_right').prevAll('.rating_content_right').removeClass('selected_hover_right');
    $('div.rating_content_right').prevAll('.rating_content_right').removeClass('selected_right1');
    $('div.rating_content_right').removeClass("selcted_spot");

    $('div.rating_content_left').prevAll('.rating_content_left').removeClass('selected_hover_left');
    $('div.rating_content_left').prevAll('.rating_content_left').removeClass('selected_left1');
    $('div.rating_content_left').removeClass('selcted_spot');

    $('#divRatingEQ').removeClass('selected_right1');
    $('#divRatingEQ').removeClass('selected_left1');
    $('#divRatingEQ').removeClass('eqalizer_color');
    $('#div_rating_right_9').removeClass('selected_right1')
    $('#divRatingEQ').removeClass("selcted_spot");

    $('.spnleft').text('');
    $('#divleft').css('display', 'none');
    $('#divequal').css('display', 'none');
    $('#divright').css('display', 'none');
    $('.spnequal').text('');
    $('.spnright').text('');
    save_pairwise(-2147483648000, 0);

    $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
}

var initializeNoUIAllGraphical = function (upper, lower, index, flast) {
    if (upper == undefined || upper == null || upper == '') upper = 400;
    if (lower == undefined || lower == null || lower == '') lower = 1200;
    if (parseInt(advantages) == 0 && parseFloat(outputvalue) <= 0) { upper = 400; lower = 1200; };
    var lefthandle = upper;
    var righthandle = lower;
    var slider = false;
    var active_multi_index = 0;
    setTimeout(function () {
        if (!slider)
            slider = getNoUiSlider(index);
        //slider = document.getElementById('graphicalSlider' + index);
        noUiSlider.create(slider, {
            start: [lefthandle, righthandle],
            behaviour: "drag-fixed-tap",
            connect: true,
            step: 1,
            range: {
                "min": 0,
                "max": 1600
            },
            pips: {
                mode: "positions",
                values: [5, 9, 13, 17, 25, 50, 75, 83, 87, 91, 95],
                density: 5,
                stepped: false
            }
        });


        var slider_value = slider.noUiSlider.get();
        var step = (parseFloat(slider_value[1]) - parseFloat(slider_value[0]));

        var lower = $(".noUi-handle-upper").offset().left - $(".noUi-handle-lower").offset().left;
        var half = lower / 2;

        var $div2 = $("<div></div>", { "class": "graph-green-div" });
        var max = $("#gslider" + index).width();
        var zero = (max / 2);

        $div2.css({
            "background-color": "#6aa84f",
            "position": "absolute",
            "left": "50%",
            "overflow": "hidden",
            "pointer-events": "none",
            "max-height": $("#" + slider.id).height(),
            "width": zero - 1 + "px",
            "height": $("#" + slider.id).height()
        });

        var pipsy = ["9", "5", "3", "2", "1", "", "1", "2", "3", "5", "9"];
        if (is_multi) {
            $div2.height($(".multigraphical0").height());
            var storedValue = [
                document.getElementById("input1" + index),
                document.getElementById("input2" + index)
            ];
            snapValues.push(storedValue);
            sliderLeft.push(parseFloat(storedValue[0]));
            sliderRight.push(parseFloat(storedValue[1]));
            $("#" + slider.id + " .noUi-value-large").each(function (i, e) {
                $(this).css({ "font-size": "11.5px", "top": "7.5px" });
                $(this).text(pipsy[i]);
            });
        }
        else {
            $div2.height($("#" + slider.id).height());
            snapValues = [];
            sliderLeft = [];
            sliderRight = [];
            var storedValue = [
                document.getElementById("noUiInput11" + index),
                document.getElementById("noUiInput21" + index)
            ];
            snapValues = storedValue;
            sliderLeft = parseFloat(storedValue[0]);
            sliderRight = parseFloat(storedValue[1]);
            $div2.width(($(".noUi-base").width() / 2));
            $(".noUi-value-large").each(function (i, e) {
                $(this).text(pipsy[i]);
            });
        }
        $div2.insertAfter($("#" + slider.id + " .noUi-connect"));

        $(".noUi-handle").addClass("disabled");
        $(".noUi-handle").css("width", "20px").css("top", "-5px").css("height", "26px");
        $(".noUi-handle.noUi-handle-upper").css("left", "-10px");
        $(".noUi-handle.noUi-handle-lower").css("left", "-10px");
        if (flast) {
            //$(".graph-green-div").css("background", "#6aa84f");
            $(".graph-green-div").width(zero - 4);
            outerIndex = -1;
        }

        $("#" + slider.id + " .noUi-draggable ").on("mousedown", function () {
            //if (!$('#index_' + index).hasClass('active_row')) {
            //    activeRow(index);
            //}
            $(".multi-rows").removeClass("selected");
            active_multi_index = index;
            $(".multi-row-" + active_multi_index).addClass("selected");
            //$scope.set_multi_index(index);
        });

        //$("#" + slider.id + " .noUi-draggable ").on("mouseup", function () {
        //    slideScroll();
        //});

        $("#" + slider.id + " .noUi-draggable ").on("touchstart", function () {
            //console.log("start");
            //if (!$('#index_' + index).hasClass('active_row')) {
            //    activeRow(index);
            //}
            $(".multi-rows").removeClass("selected");
            active_multi_index = index;
            $(".multi-row-" + active_multi_index).addClass("selected");
        });

        //var isChangeRunning = true;
        slider.noUiSlider.on("change", function (value, handle) {

            outerIndex = -1;
            var SliderVal = slider.noUiSlider.get();
            var thisUiOrigin;
            var thisGreenDiv;
            if (is_multi) {
                $("#gslider" + index).removeClass("newsample");
                thisUiOrigin = $("#gslider" + index + " .noUi-origin");
                thisGreenDiv = $("#gslider" + index + " .graph-green-div");
                //if (!$('#index_' + index).hasClass('active_row')) {
                //    activeRow(index);
                //}
            }
            else {
                $('#graphicalSlider').removeClass('newsample1');
                thisUiOrigin = $(".noUi-origin");
                thisGreenDiv = $(".graph-green-div");
            }
            if (thisUiOrigin.is(":hidden")) {
                thisUiOrigin.show();
                thisGreenDiv.show();
                if (parseFloat(SliderVal[1]) < 800) {
                    slider.noUiSlider.set([SliderVal[1], parseFloat(SliderVal[1]) + 800]);
                    return false;
                }
            };

            var signal = -1;
            if (is_multi) {
                left_input[index] = isNaN(sliderLeft[index]) ? 1 : sliderLeft[index];
                right_input[index] = isNaN(sliderRight[index]) ? 1 : sliderRight[index];
                signal = oldSliderValue[index];

                left_input[index] = isNaN(left_input[0]) ? '0' : left_input[index];
                right_input[index] = isNaN(right_input[index]) ? '0' : right_input[index];
                $('#input1' + index).val(parseFloat(left_input[index]).toFixed(2));
                $('#input2' + index).val(parseFloat(right_input[index]).toFixed(2));
            }
            else {
                main_bar[0] = isNaN(sliderLeft) ? 1 : sliderLeft;
                main_bar[1] = isNaN(sliderRight) ? 1 : sliderRight;
                $('#noUiInput11').val(main_bar[0]);
                $('#noUiInput21').val(main_bar[1]);
                signal = oldSliderValue;
            }

            var pass = false;
            if (signal != parseInt(value[0])) {
                var value1 = parseFloat(SliderVal[0]) + step;
                slider.noUiSlider.set([SliderVal[0], value1]);
            }
            else {
                var value1 = parseFloat(SliderVal[1]) - step;
                if (left_input[index] == "1" && right_input[index] == "1")
                    slider.noUiSlider.set([400, 1200]);
                else
                    slider.noUiSlider.set([value1, SliderVal[1]]);

                $("#gslider" + i + " .noUi-connect").css("background-color", "#0058a3");
            }
            slider_value = slider.noUiSlider.get();

            SliderVal = slider.noUiSlider.get();
            if (parseFloat(SliderVal[0]) > 720) {
                slider.noUiSlider.set([720, 1520]);

            }
            else if (parseFloat(SliderVal[0]) < 80) {
                slider.noUiSlider.set([80, 880]);
            }

            var SliderVal = slider.noUiSlider.get();
            if (is_multi)
                oldSliderValue[index] = parseInt(SliderVal[0]);
            else
                oldSliderValue = parseInt(SliderVal[0]);

            var value = ""; var advantage = "";
            if (SliderVal[0] > 400) {
                SliderVal = getandCalculateSlider(SliderVal, false);
                value = SliderVal[1];
                advantage = -1;
                $('#input2' + index).val(parseFloat(value).toFixed(2));
            }
            else {
                SliderVal = getandCalculateSlider(SliderVal, false);
                value = SliderVal[0];
                advantage = 1;
            }

            if (value > 9)
                value = 9;

            if (is_multi) {
                add_multivalues(value, advantage, index);
            }
            else {
                save_pairwise(value, advantage);
            }

            if (index && parseFloat(value) == 1) {
                left_input[index] = 1;
                right_input[index] = 1;
            }
        });


        slider.noUiSlider.on("update", function (value, handle) {
            if (is_multi) {
                if (index != outerIndex) {
                    setNoUiColor(true, index);
                    outerIndex = index;
                }
            }
            else {
                setNoUiColor(true, -1);
            }
        });

        slider.noUiSlider.on("slide", function (data, handle, value) {

            //for (i = 0; i < multi_pw_data.length; i++) {
            //    if (i !== index) {
            //        $('#index_' + i).removeClass('active_row');
            //        $('#index_' + i).addClass('unselected_row');
            //    }
            //    if (!$('#index_' + index).hasClass('active_row')) {
            //        $('#index_' + index).removeClass('unselected_row');
            //        $('#index_' + index).addClass('active_row');
            //    }
            //}

            activeRow(index, true);

            $('#graphicalSlider').removeClass('newsample1');
            $('#gslider' + index).removeClass('newsample');
            //$("#gslider" + index + " .noUi-connect").css("background-color", "#0058a3");
            //$("#gslider" + index + " .graph-green-div").css("background-color", "#6aa84f");
            if (document.activeElement && document.activeElement.tagName == "INPUT") {
                document.activeElement.blur();
            }

            fFirst = true;
            var firstvalue = parseInt(value[0]);
            if (is_multi) {
                if (oldSliderValue) {
                    if (oldSliderValue[index] != firstvalue) {
                        fFirst = false;
                    }
                }
            }
            else {
                if (oldSliderValue) {
                    if (oldSliderValue != firstvalue) {
                        fFirst = false;
                    }

                }
            }

            var SliderVal = getandCalculateSlider(slider.noUiSlider.get(), fFirst);

            if (is_multi) {

                if (SliderVal[0] > 9 && left_input[index] <= 9) SliderVal[0] = 9;
                if (SliderVal[1] > 9 && right_input[index] <= 9) SliderVal[1] = 9;
                //for fast
                snapValues[index][0] = SliderVal[0];
                snapValues[index][1] = SliderVal[1];
                sliderLeft[index] = parseFloat(SliderVal[0]);
                sliderRight[index] = parseFloat(SliderVal[1]);
                $('#input1' + index).val(parseFloat(SliderVal[0]).toFixed(2));
                $('#input2' + index).val(parseFloat(SliderVal[1]).toFixed(2));

                if (output.pipeOptions.dontAllowMissingJudgment) {
                    var disablePage = false;
                    if (multi_pw_data.length > 0) {
                        for (i = 0; i < multi_pw_data.length; i++) {
                            if (left_input[i] == "" && right_input[i] == "") {
                                disablePage = true;
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
                }
            }
            else {
                if (SliderVal[0] > 9) SliderVal[0] = 9;
                if (SliderVal[1] > 9) SliderVal[1] = 9;
                snapValues[0] = SliderVal[0];
                snapValues[1] = SliderVal[1];
                sliderLeft = parseFloat(SliderVal[0]);
                sliderRight = parseFloat(SliderVal[1]);

                $('#noUiInput11').val(parseFloat(SliderVal[0]).toFixed(2));
                $('#noUiInput21').val(parseFloat(SliderVal[1]).toFixed(2));
            }
            slideScroll(false);
        });


        if (is_multi) {
            oldSliderValue[index] = parseInt(slider_value[0]);
            if (multi_pw_data[index].isUndefined) {
                oldSliderValue[index] = 400;
                setNoUiColor(false, index);
                left_input[index] = "";
                right_input[index] = "";
            }

        }
        else {
            oldSliderValue = parseInt(slider_value[0]);
            if (parseInt(advantages) == 0 && parseFloat(outputvalue) <= 0) {
                oldSliderValue = 400;
                setNoUiColor(false, -1);
                //$(".noUi-connect").css("background-color", "#AAAAAA");
                //$(".graph-green-div").css("background-color", "#AAAAAA");
                main_bar[0] = "";
                main_bar[1] = "";
            }
        }

        if (flast) {
            var max = $(".noUi-base").width();
            var zero = (max / 2);
            $(".graph-green-div").width(zero - 4);
        }
    }, 30); // TODO: optimize timeout
}

var add_multivalues = function (value, advantage, index, event) {
    var flashTimeOut = value == "-2147483648000" ? 0 : 100;
    var isSelectedIndex = (active_multi_index == index);

    //Flash only when judgment is entered and no flash when a judgment is deleted
    if (value.toString() != "-2147483648000") {
        setTimeout(function () {
            //adding flash css class on judgment line if it's selected line
            if (isSelectedIndex) {
                $(".multi-row-" + index).addClass("judgment-flash");
                $(".tt-verbal-bars-wrap.multi-loop.selected ul li").addClass("judgment-flash-li");
            }
        }, 25, false);
    }

    active_multi_index = index;

    if (isSelectedIndex) {
        setTimeout(function () {
            $(".multi-rows").removeClass("selected");
            $(".multi-row-" + index).addClass("selected");

            if (value.toString() != "-2147483648000") {
                //removing flash css class on judgment line
                $(".multi-row-" + index).removeClass("judgment-flash");
                $(".tt-verbal-bars-wrap.multi-loop.selected ul li").removeClass("judgment-flash-li");

                if (hdnpairwise_type == "ptGraphical") {
                    $(".graph-green-div").addClass("others");
                    $("#gslider" + index + " .graph-green-div").removeClass("others");
                    $(".graph-green-div").width(zero);
                    var max = $(".noUi-base").width();
                    var zero = (max / 2);
                    $(".graph-green-div.others").width(zero);
                    var uibaseWidth = $("#gslider" + index).width();
                    $("#gslider" + index + " .graph-green-div").width((uibaseWidth / 2) - 1);
                    $(".graph-green-div.others").width((uibaseWidth / 2));
                }

                setTimeout(function () {

                    if (index + 2 > multi_pw_data.length) {
                        active_multi_index = multi_pw_data.length - 1;
                        $(".multi-row-" + active_multi_index).addClass("selected");
                        activate_first_row = true;
                    }
                    else {
                        $(".multi-row-" + (index + 1)).addClass("selected");
                        active_multi_index = index + 1;
                        activate_first_row = false;
                    }

                    if (active_multi_index > 0 || activate_first_row) {
                        // $location.hash("multi-row-" + $scope.active_multi_index);
                        //$anchorScroll();
                    }

                    //set_multi_index(active_multi_index);
                    //if (activate_first_row) {

                    //} else {
                    //    scroll_to_index = active_multi_index - 2;
                    //    // //console.log($scope.scroll_to_index);
                    //    if (scroll_to_index <= 1) {
                    //        //do not scroll
                    //    } else {
                    //        //$location.hash("multi-row-" + $scope.scroll_to_index);
                    //        //$anchorScroll();
                    //    }
                    //}

                    //scroll_to_active_index();
                    // Foundation.libs.tooltip.hide(angular.element($scope.temp_tip));

                }, 50);
            }
        }, flashTimeOut, false);
    }
    else {
        //callResizeFrameImagesProportionally(100, "add_multivalues");
    }

    updateMultiPairwiseObjectValue(value, advantage, index, event);

    if (output.pipeOptions.dontAllowMissingJudgment) {
        setTimeout(function () {
            var disablePage = false;
            if (multi_pw_data.length > 0) {
                for (i = 0; i < multi_pw_data.length; i++) {
                    if (left_input[i] == "" && right_input[i] == "") {
                        disablePage = true;
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
        }, 40);
    }
    if (value > 0)
        slideScroll(false);
}

var set_multi_index = function (index) {
    $(".multi-rows").removeClass("selected");

    var callApply = active_multi_index != index;
    active_multi_index = index;

    $(".multi-row-" + active_multi_index).addClass("selected");
    $(".multi-row-" + index + " .not-disabled").addClass("change-color-pulse");

    try {
        if (isMobile() && (hdnpage_type == "atAllPairwise" || hdnpage_type == "atAllPairwiseOutcomes")) {
            setTimeout(function () {
                mobile_manual_equalizer();
            }, 2, callApply);
        }
    }
    catch (e) {
    }

    setTimeout(function () {
        $(".not-disabled").removeClass("change-color-pulse");
        $(".disabled").removeClass("change-color-pulse");

        scroll_to_active_index();
    }, 200, false);

    callResizeFrameImagesProportionally(200, "set_multi_index " + index);
}

var updateMultiPairwiseObjectValue = function (value, advantage, index, event) {
    ////console.log("updating... value: " + value + ", advantage: " + advantage);
    if (multi_pw_data != undefined && multi_pw_data != null && multi_pw_data.length > 0) {
        multi_pw_data[index].Value = parseFloat(value);
        multi_pw_data[index].Advantage = parseInt(advantage);
        checkMultiValue(multi_pw_data);

        if (advantage == 0) {
            multi_pw_data[index].isUndefined = true;
        }
        else {
            multi_pw_data[index].isUndefined = false;
        }
    }

    if (event) {
        //event.stopPropagation();
        return false;
    }
}

var getNoUiSlider = function (index) {
    //multi
    if (parseInt(index) >= 0)
        return document.getElementById("gslider" + index);
    //single        
    return document.getElementById("graphicalSlider");
}

var initializeNoUIGraphical = function (upper, lower, index, flast) {
    if (upper == undefined || upper == null || upper == '') upper = 400;
    if (lower == undefined || lower == null || lower == '') lower = 1200;
    if (parseInt(advantages) == 0 && parseFloat(outputvalue) <= 0) { upper = 400; lower = 1200; };
    var lefthandle = upper;
    var righthandle = lower;
    var slider = false;
    var active_multi_index = 0;
    setTimeout(function () {
        if (!slider)
            //slider = document.getElementById('graphicalSlider');
            slider = getNoUiSlider(index);
        noUiSlider.create(slider, {
            start: [lefthandle, righthandle],
            behaviour: "drag-fixed-tap",
            connect: true,
            step: 1,
            range: {
                "min": 0,
                "max": 1600
            },
            pips: {
                mode: "positions",
                values: [5, 9, 13, 17, 25, 50, 75, 83, 87, 91, 95],
                density: 5,
                stepped: false
            }
        });


        var slider_value = slider.noUiSlider.get();
        var step = (parseFloat(slider_value[1]) - parseFloat(slider_value[0]));

        var lower = $(".noUi-handle-upper").offset().left - $(".noUi-handle-lower").offset().left;
        var half = lower / 2;

        //var $div2 = $("<div></div>", { "class": "noUi-connect" });
        //var max = $("#gslider" + index).width();
        //var zero = (max / 2);

        //$div2.css({
        //    "background-color": "#b5d4a7",
        //    "position": "absolute",
        //    "left": "50%",
        //    "overflow": "hidden",
        //    "pointer-events": "none",
        //    "max-height": $("#" + slider.id).height(),
        //    "width": zero - 1 + "px",
        //    "height": $("#" + slider.id).height()
        //});

        var $div2 = $("<div></div>", { "class": "graph-green-div" });
        var max = $("#gslider" + index).width();
        var zero = (max / 2);

        $div2.css({
            "background-color": "#6aa84f",
            "position": "absolute",
            "left": "50%",
            "overflow": "hidden",
            "pointer-events": "none",
            "max-height": $("#" + slider.id).height(),
            "width": zero - 1 + "px",
            "height": $("#" + slider.id).height()
        });

        var pipsy = ["9", "5", "3", "2", "1", "", "1", "2", "3", "5", "9"];
        if (is_multi) {
            $div2.height($(".multigraphical0").height());
            var storedValue = [
                document.getElementById("input1" + index),
                document.getElementById("input2" + index)
            ];
            snapValues.push(storedValue);
            sliderLeft.push(parseFloat(storedValue[0]));
            sliderRight.push(parseFloat(storedValue[1]));
            $("#" + slider.id + " .noUi-value-large").each(function (i, e) {
                $(this).css({ "font-size": "11.5px", "top": "7.5px" });
                $(this).text(pipsy[i]);
            });
        }
        else {
            $div2.height($("#" + slider.id).height());
            snapValues = [];
            sliderLeft = [];
            sliderRight = [];
            var storedValue = [
                document.getElementById("noUiInput11"),
                document.getElementById("noUiInput21")
            ];
            snapValues = storedValue;
            sliderLeft = parseFloat(storedValue[0]);
            sliderRight = parseFloat(storedValue[1]);
            $div2.width(($(".noUi-base").width() / 2));
            $(".noUi-value-large").each(function (i, e) {
                $(this).text(pipsy[i]);
            });
        }
        $div2.insertAfter($("#" + slider.id + " .noUi-connect"));

        //$(".noUi-handle").addClass("disabled");
        $(".noUi-handle").css("width", "20px").css("top", "-5px").css("height", "26px");
        $(".noUi-handle.noUi-handle-upper").css("left", "-10px");
        $(".noUi-handle.noUi-handle-lower").css("left", "-10px");
        //slider functions
        if (flast) {
            //$(".graph-green-div").css("background", "#6aa84f");
            setTimeout(function () {
                $(".graph-green-div").width(zero - 4);
                outerIndex = -1;
                $(".noUi-origin").width(zero - 4);
                outerIndex = -1;
            }, 500);
        }

        $("#" + slider.id + " .noUi-draggable ").on("mousedown", function () {

            $(".multi-rows").removeClass("selected");
            active_multi_index = index;
            $(".multi-row-" + active_multi_index).addClass("selected");
            //$scope.set_multi_index(index);
        });

        $("#" + slider.id + " .noUi-draggable ").on("touchstart", function () {
            //console.log("start");
            $(".multi-rows").removeClass("selected");
            active_multi_index = index;
            $(".multi-row-" + active_multi_index).addClass("selected");
        });

        //var isChangeRunning = true;
        slider.noUiSlider.on("change", function (value, handle) {
            $('.arrowicons').show();
            outerIndex = -1;
            var SliderVal = slider.noUiSlider.get();
            var thisUiOrigin;
            var thisGreenDiv;
            if (is_multi) {
                thisUiOrigin = $("#gslider" + index + " .noUi-origin");
                thisGreenDiv = $("#gslider" + index + " .graph-green-div");
            }
            else {
                thisUiOrigin = $(".noUi-origin");
                thisGreenDiv = $(".graph-green-div");
            }
            if (thisUiOrigin.is(":hidden")) {
                thisUiOrigin.show();
                thisGreenDiv.show();
                if (parseFloat(SliderVal[1]) < 800) {
                    slider.noUiSlider.set([SliderVal[1], parseFloat(SliderVal[1]) + 800]);
                    return false;
                }
            };

            var signal = -1;
            if (is_multi) {
                left_input[index] = isNaN(sliderLeft[index]) ? 1 : sliderLeft[index];
                right_input[index] = isNaN(sliderRight[index]) ? 1 : sliderRight[index];
                signal = oldSliderValue[index];
            }
            else {
                main_bar[0] = isNaN(sliderLeft) ? 1 : sliderLeft;
                main_bar[1] = isNaN(sliderRight) ? 1 : sliderRight;
                $('#noUiInput11').val(main_bar[0]);
                $('#noUiInput21').val(main_bar[1]);
                signal = oldSliderValue;
            }

            var pass = false;
            if (signal != parseInt(value[0])) {
                var value1 = parseFloat(SliderVal[0]) + step;
                slider.noUiSlider.set([SliderVal[0], value1]);
            }
            else {
                var value1 = parseFloat(SliderVal[1]) - step;
                slider.noUiSlider.set([value1, SliderVal[1]]);
            }
            slider_value = slider.noUiSlider.get();

            SliderVal = slider.noUiSlider.get();
            if (parseFloat(SliderVal[0]) > 720) {
                slider.noUiSlider.set([720, 1520]);

            }
            else if (parseFloat(SliderVal[0]) < 80) {
                slider.noUiSlider.set([80, 880]);
            }

            var SliderVal = slider.noUiSlider.get();
            if (is_multi)
                oldSliderValue[index] = parseInt(SliderVal[0]);
            else
                oldSliderValue = parseInt(SliderVal[0]);

            var value = ""; var advantage = "";
            if (SliderVal[0] > 400) {
                SliderVal = getandCalculateSlider(SliderVal, false);
                value = SliderVal[1];
                advantage = -1;
                $('#noUiInput11').val(SliderVal[0]);
                $('#noUiInput21').val(SliderVal[1]);
                signal = oldSliderValue;
            }
            else {
                SliderVal = getandCalculateSlider(SliderVal, false);
                value = SliderVal[0];
                advantage = 1;
            }

            if (value > 9)
                value = 9;

            if (is_multi) {
                add_multivalues(value, advantage, index);
            }
            else {
                save_pairwise(value, advantage);
            }

            if (index && parseFloat(value) == 1) {
                left_input[index] = 1;
                right_input[index] = 1;
            }
        });


        slider.noUiSlider.on("update", function (value, handle) {
            if ($('#noUiInput11').val() != '' && $('noUiInput21') != '') {
                $('.arrowicons').show();
            }
            if (is_multi) {
                if (index != outerIndex) {
                    setNoUiColor(true, index);
                    outerIndex = index;
                }
            }
            else {
                setNoUiColor(true, -1);
            }
        });

        slider.noUiSlider.on("slide", function (data, handle, value) {

            $('#graphicalSlider').removeClass('newsample1');
            $("#gslider" + index + " .graph-green-div").css("background-color", "#6aa84f");
            if (document.activeElement && document.activeElement.tagName == "INPUT") {
                document.activeElement.blur();
                //$scope.blur_graphical_input();
            }

            fFirst = true;
            var firstvalue = parseInt(value[0]);
            if (is_multi) {

                if (oldSliderValue) {
                    if (oldSliderValue[index] != firstvalue) {
                        fFirst = false;
                    }
                }
            }
            else {
                if (oldSliderValue) {
                    if (oldSliderValue != firstvalue) {
                        fFirst = false;
                    }

                }
            }

            var SliderVal = getandCalculateSlider(slider.noUiSlider.get(), fFirst);

            if (is_multi) {

                if (SliderVal[0] > 9 && left_input[index] <= 9) SliderVal[0] = 9;
                if (SliderVal[1] > 9 && right_input[index] <= 9) SliderVal[1] = 9;
                //for fast
                snapValues[index][0].value = SliderVal[0];
                snapValues[index][1].value = SliderVal[1];
                sliderLeft[index] = parseFloat(SliderVal[0]);
                sliderRight[index] = parseFloat(SliderVal[1]);

            }
            else {
                if (SliderVal[0] > 9) SliderVal[0] = 9;
                if (SliderVal[1] > 9) SliderVal[1] = 9;
                snapValues[0].value = SliderVal[0];
                snapValues[1].value = SliderVal[1];
                sliderLeft = parseFloat(SliderVal[0]);
                sliderRight = parseFloat(SliderVal[1]);


                $('#noUiInput11').val(parseFloat(SliderVal[0]).toFixed(2));
                $('#noUiInput21').val(parseFloat(SliderVal[1]).toFixed(2));
            }
            //});

        });


        if (is_multi) {
            oldSliderValue[index] = parseInt(slider_value[0]);
            if (output.multi_pw_data[index].isUndefined) {
                oldSliderValue[index] = 400;
                setNoUiColor(false, index);

                left_input[index] = "";
                right_input[index] = "";
            }
        }
        else {
            oldSliderValue = parseInt(slider_value[0]);
            if (parseInt(advantages) == 0 && parseFloat(outputvalue) <= 0) {
                oldSliderValue = 400;
                setNoUiColor(false, -1);
                //$('#graphicalSlider').addClass('newsample');
                //$(".noUi-connect").css("background-color", "#AAAAAA");
                //$(".graph-green-div").css("background-color", "#AAAAAA");
                main_bar[0] = "";
                main_bar[1] = "";
            }
        }

        if ($('#noUiInput11').val() == '' && $('#noUiInput21').val() == '') {
            $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }
        else {
            $('.chart_wrapper .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }

        if (flast) {
            var max = $(".noUi-base").width();
            var zero = (max / 2);
            $(".graph-green-div").width(zero - 4);
        }
    }, 300); // TODO: optimize timeout

}

var getandCalculateSlider = function (sliderValue, fFirst) {
    if (fFirst)
        sliderValue[0] = sliderValue[1] - 800;

    var ranges = [];
    ranges.push({ "left": 80, "right": 1520 });
    ranges.push({ "left": 144, "right": 1456 });
    ranges.push({ "left": 208, "right": 1392 });
    ranges.push({ "left": 272, "right": 1328 });
    ranges.push({ "left": 400, "right": 1200 });

    if (sliderValue[0] > 400) {
        if (sliderValue[1] >= ranges[0].right) {
            sliderValue[1] = 9;
        }
        else if (sliderValue[1] >= ranges[1].right && sliderValue[1] < ranges[0].right) {
            sliderValue[1] = (5 + ((sliderValue[1] - ranges[1].right) / (ranges[0].right - ranges[1].right) * 4)).toFixed(2);
        }
        else if (sliderValue[1] >= ranges[2].right && sliderValue[1] < ranges[1].right) {
            sliderValue[1] = (3 + ((sliderValue[1] - ranges[2].right) / (ranges[1].right - ranges[2].right) * 2)).toFixed(2);
        }
        else if (sliderValue[1] >= ranges[3].right && sliderValue[1] < ranges[2].right) {
            sliderValue[1] = (2 + ((sliderValue[1] - ranges[3].right) / (ranges[2].right - ranges[3].right) * 1)).toFixed(2);
        }
        else {
            sliderValue[1] = (1 + ((sliderValue[1] - ranges[4].right) / (ranges[3].right - ranges[4].right) * 1)).toFixed(2);
        }

        sliderValue[1] = getSliderLabelValue(sliderValue[1]);
        sliderValue[0] = 1;
    }
    else {
        if (sliderValue[0] <= ranges[0].left) {
            sliderValue[0] = 9;
        }
        else if (sliderValue[0] > ranges[0].left && sliderValue[0] <= ranges[1].left) {
            sliderValue[0] = (9 - ((sliderValue[0] - ranges[0].left) / (ranges[1].left - ranges[0].left) * 4)).toFixed(2);
        }
        else if (sliderValue[0] > ranges[1].left && sliderValue[0] <= ranges[2].left) {
            sliderValue[0] = (5 - ((sliderValue[0] - ranges[1].left) / (ranges[2].left - ranges[1].left) * 2)).toFixed(2);
        }
        else if (sliderValue[0] > ranges[2].left && sliderValue[0] < ranges[3].left) {
            sliderValue[0] = (3 - ((sliderValue[0] - ranges[2].left) / (ranges[3].left - ranges[2].left) * 1)).toFixed(2);
        }
        else {
            sliderValue[0] = (2 - ((sliderValue[0] - ranges[3].left) / (ranges[4].left - ranges[3].left) * 1)).toFixed(2);
        }

        sliderValue[0] = getSliderLabelValue(sliderValue[0]);
        sliderValue[1] = 1;
    }

    //console.log(sliderValue[0] + ", " + sliderValue[1]);

    return sliderValue;
}

var getSliderLabelValue = function (value) {
    var ceilValue = Math.ceil(value);
    var middleValue = ceilValue - 0.5;
    var floorValue = Math.floor(value);

    if (value > (middleValue + 0.05) && value >= (ceilValue - 0.05)) {
        value = ceilValue;
    }
    else if (value <= (middleValue + 0.05) && value >= (middleValue - 0.05)) {
        value = middleValue;
    }
    else if (value <= (floorValue + 0.05)) {
        value = floorValue;
    }

    return value;
}

function setNoUiColor(fJudgmentExist, index) {
    if (fJudgmentExist) {
        if (index < 0) {
            $('#graphicalSlider').removeClass('newsample1');
            $(".noUi-connect").css("background-color", "#0058a3 ");
            $(".graph-green-div").css("background-color", "#6aa84f");
            //$('div.reverse_icon, div.close_icon').show();
        }
        else {
            $("#gslider" + index + " .noUi-connect").css("background-color", "##0058A3");
            $("#gslider" + index + " .graph-green-div").css("background-color", "#6aa84f");
            $('#reverse_icon_' + index).show();
            $('#close_icon_' + index).show();
        }
    }
    else {
        if (index < 0) {
            $(" .noUi-connect").css("background-color", "#AAAAAA ");
            $(" .graph-green-div").css("background-color", "#AAAAAA ");
        }
        else {
            $("#gslider" + index).addClass("newsample");
            $("#gslider" + index + " .noUi-connect").css("background-color", "#AAAAAA");
            $("#gslider" + index + " .graph-green-div").css("background-color", "#AAAAAA");
        }
    }

    if (output.pipeOptions.dontAllowMissingJudgment) {
        var disablePage = false;
        if (multi_pw_data.length > 0) {
            for (i = 0; i < multi_pw_data.length; i++) {
                if (left_input[i] == "" && right_input[i] == "") {
                    disablePage = true;
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
    }
    if (hdnpage_type == 'atAllPairwise') {
        var actroid = $.cookie("SelectionID");
        index = actroid;

    }
    //activeRow(index, true); 
}

var set_slider_blank = function (index) {
    setTimeout(function () {
        setNoUiSlider(400, 1200, index);

        oldSliderValue[index] = 400;
        left_input[index] = "";
        right_input[index] = "";

        sliderLeft = [];
        sliderRight = [];
        setNoUiColor(false, index);
        outerIndex = -1;

        $('#input1' + index).val('');
        $('#input2' + index).val('');

        if (($('#input1' + index).val() !== '' || parseInt($('#input1' + index).val()) > 0) && ($('#input2' + index).val() !== '' || parseInt($('#input2' + index).val()) > 0)) {
            $('#index_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }
        else
            $('#index_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })

        $('#reverse_icon_' + index).hide();
        $('#close_icon_' + index).hide();

        if (output.pipeOptions.dontAllowMissingJudgment) {
            var disablePage = false;
            if (multi_pw_data.length > 0) {
                for (i = 0; i < multi_pw_data.length; i++) {

                    if (left_input[i] == "" && right_input[i] == "") {
                        disablePage = true;
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
        }
    }, 100);
}

var graphicalall_key_up = function (event, value, position, state, index, is_main_bar) {
    var ctlIndex = value;
    main_bar[value] = parseFloat(event.value);
    value = [parseFloat(event.value), undefined, undefined];
    var code = null;
    if (event) {
        code = event.keyCode;
    }
    //setTimeout.cancel($scope.graphical_input_timeout);
    var inputId = "";

    var inputIdOld = is_main_bar ? event.id : event.id.replace(index, "");
    if (typeof inputIdOld != 'undefined')
        inputId = inputIdOld.substr(0, inputIdOld.length - 1)
    ////console.log(event.type);
    if (parseFloat(value[0]) < 0) {
        ////console.log("value less than 0");

        if (inputId === "input1" && left_input[index] < 0) {
            left_input[index] = 0;
        }
        else if (inputId === "input2" && right_input[index] < 0) {
            right_input[index] = 0;
        }
        if (!(event.type === "mouseup" || event.type === "mouseout" || event.type === "mouseleave" || event.type === "blur")) {
            //return event.preventDefault();
            return false;
        }
    }

    if (state == "press") {
        if (code || event.ctrlKey || event.altKey) {

        }
        //else if (!((code > 47 && code < 58) || (code > 95 && code < 106)) && code !== 110 && code !== 190 && code != 46 && code !== 13 && code !== 9 && code !== 8) {
        //    //return event.preventDefault();
        //    return false;
        //}
    }
    else {
        if (state == "up" && value[0] != null && value[0] != "") {
            setTimeout(function () {
                var advantage;
                if (is_main_bar.toLowerCase() == 'true') {
                    $('#graphicalSlider').removeClass('newsample1');
                    if (ctlIndex == 0) {
                        if ($('#noUiInput21').val().trim() == '')
                            $('#noUiInput21').val(1);
                        main_bar[0] = parseFloat(event.value);
                        //main_bar[1] = 1;
                    }
                    else {
                        if ($('#noUiInput11').val().trim() == '')
                            $('#noUiInput11').val(1);
                        main_bar[1] = parseFloat(event.value);
                        //main_bar[0] = 1;
                    }
                    //FOR SINGLE 
                    if (main_bar[0] > 0 || main_bar[1] > 0) {
                        $('#graphicalSlider').removeClass('newsample1');
                    }
                    outerIndex = -1;
                    if (main_bar[0] == main_bar[1]) {
                        value[0] = 1;
                        advantage = 1;
                    }
                    else if (main_bar[0] != 1 || main_bar[1] != 1) {
                        if (main_bar[0] > main_bar[1]) {
                            if (main_bar[1] == null || main_bar[1] == "") {
                                main_bar[1] = 1;
                            }
                            value[0] = main_bar[0] / main_bar[1];
                            advantage = 1;
                        }
                        if (main_bar[0] < main_bar[1]) {
                            if (main_bar[0] == null || main_bar[0] == "") {
                                main_bar[0] = 1;
                            }
                            value[0] = main_bar[1] / main_bar[0];
                            advantage = -1;
                        }
                    }
                    else {
                        value[0] = main_bar[0];
                        advantage = 0;
                    }

                    var underreview = $.cookie('is_under_review');
                    if (underreview == false || underreview == null || typeof underreview == "undefined")
                        save_pairwise(value[0], advantage);

                    if (advantage == -1) {
                        numericalSlider[0] = (800 * ((value[0]) / ((value[0]) + 1)));


                    }
                    if (advantage == 1) {
                        numericalSlider[0] = 800 - (800 * ((value[0]) / ((value[0]) + 1)));
                    }

                    //$scope.main_bar[0] = value[0] > 0 ? parseFloat((advantage == 1 ? (value[0]) * 1 : 1).toFixed(2)) : "";
                    //$scope.main_bar[1] = value[0] > 0 ? parseFloat((advantage == -1 ? (value[0]) * 1 : 1).toFixed(2)) : "";
                    main_bar_txt[0] = (advantage == 1 ? (value[0]) * 1 : 1).toFixed(2);
                    main_bar_txt[1] = (advantage == -1 ? (value[0]) * 1 : 1).toFixed(2);
                    if (advantage == 0) {
                        numericalSlider[0] = 400;
                    }


                    if (advantage) {

                        /*setNoUiSlider(numericalSlider[0], numericalSlider[0] + 800, -1);*/
                        setNoUiSlider(numericalSlider[0], numericalSlider[0] + 800, index);
                    }
                    if (getNoUiSlider() != null && advantage == 0 && value[0] == 1) {
                        noUiTimeOut = setTimeout(function () {

                            setNoUiSlider(400, 1200, -1);
                            //$(".noUi-connect").css("background-color", "#0058a3");
                            //$(".graph-green-div").css("background-color", "#6aa84f");
                        }, 100);
                    }
                }
                else {
                    //FOR MULTI
                    $('#gslider' + index).removeClass('newsample');
                    var leftbox = $('#input1' + index).val();
                    var rightbox = $('#input2' + index).val();
                    if (inputId == 'input1' && leftbox > 0) {
                        if ($('#input2' + index).val().trim() == '')
                            $('#input2' + index).val(1);
                        //right_input[index] = 1;
                        $('#input2' + index).val(rightbox);
                        right_input[index] = rightbox;
                        left_input[index] = leftbox;
                    } else
                        if (inputId == 'input2' && rightbox > 0) {
                            if ($('#input1' + index).val().trim() == '')
                                $('#input1' + index).val(1);
                            //left_input[index] = 1;
                            $('#input1' + index).val(leftbox);
                            left_input[index] = leftbox;
                            right_input[index] = rightbox;
                        }
                    var slider_value = 0;
                    if (left_input[index] == right_input[index]) {
                        value[0] = 0;
                        advantage = 1;
                        slider_value = 400;
                    }
                    else if (left_input[index] != 1 || right_input[index] != 1) {
                        if (left_input[index] > right_input[index]) {
                            if (right_input[index] == null || right_input[index] == "") {
                                right_input[index] = 1;
                                $('#input2' + index).val(1);
                            }
                            value[0] = left_input[index] / right_input[index];
                            advantage = 1;
                            slider_value = 800 - (800 * ((value[0]) / ((value[0]) + 1)));
                        }
                        if (left_input[index] < right_input[index]) {
                            if (left_input[index] == null || left_input[index] == "") {
                                left_input[index] = 1;
                                $('#input1' + index).val(1);
                            }
                            value[0] = right_input[index] / left_input[index];
                            advantage = -1;
                            slider_value = (800 * ((value[0]) / ((value[0]) + 1)));
                        }
                    }
                    else {
                        value[0] = left_input[index];
                        advantage = 0;
                        slider_value = 800;
                        right_input[index] = 1;
                        left_input[index] = 1;
                    }

                    if (left_input[index] !== "" && right_input[index] !== "") {

                        setNoUiSlider(slider_value, slider_value + 800, index);
                        updateMultiPairwiseObjectValue(value, advantage, index, event);
                    }
                }

            }, 200);
        }
        else if (value[0] == null || value[0] == "") {
            if (is_main_bar.toLowerCase() == 'true') {
                output.value = -2147483648000;
                output.advantage = 0;

                var slider = getNoUiSlider();
                slider.noUiSlider.set([400, 1200]);
                setNoUiColor(false, -1);
            }
            else {

                updateMultiPairwiseObjectValue(-2147483648000, 0, index, event);
                setNoUiSlider(400, 1200, index);
                oldSliderValue[index] = 400;
                setNoUiColor(false, index);
                outerIndex = -1;
            }
        }
    }
    if (($('#input1' + index).val() !== '' && parseInt($('#input1' + index).val()) > 0) || ($('#input2' + index).val() !== '' && parseInt($('#input2' + index).val()) > 0)) {
        $('#index_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
    }
    else
        $('#index_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
}

var graphical_key_up = function (event, value, position, state, index, is_main_bar) {

    var ctlIndex = value;
    main_bar[value] = parseFloat(event.value);
    value = [parseFloat(event.value), undefined, undefined];
    var code = null;
    if (event) {
        code = event.keyCode;
    }
    //setTimeout.cancel($scope.graphical_input_timeout);

    var inputId = is_main_bar ? event.id : event.id.replace(index, "");
    ////console.log(event.type);
    if (parseFloat(value[0]) < 0) {
        ////console.log("value less than 0");

        if (inputId === "input1" && left_input[index] < 0) {
            left_input[index] = 0;
        }
        else if (inputId === "input2" && right_input[index] < 0) {
            right_input[index] = 0;
        }
        else if (inputId === "noUiInput11" && main_bar[0] < 0) {
            main_bar[0] = 0;
        }
        else if (inputId === "noUiInput21" && main_bar[1] < 0) {
            main_bar[1] = 0;
        }
        if (!(event.type === "mouseup" || event.type === "mouseout" || event.type === "mouseleave" || event.type === "blur")) {
            //return event.preventDefault();
            return false;
        }
    }

    if (state == "press") {
        if (code || event.ctrlKey || event.altKey) {

        }
        else if (!((code > 47 && code < 58) || (code > 95 && code < 106)) && code !== 110 && code !== 190 && code != 46 && code !== 13 && code !== 9 && code !== 8) {
            //return event.preventDefault();
            return false;
        }
    }
    else {
        if (state == "up" && value[0] != null && value[0] != "") {
            /*$scope.graphical_input_timeout = $timeout(function () {*/
            setTimeout(function () {
                var advantage;

                if (is_main_bar.toLowerCase() == 'true') {
                    //FOR SINGLE 
                    if (main_bar[0] > 0 || main_bar[1] > 0) {
                        $('#graphicalSlider').removeClass('newsample1');
                    }
                    outerIndex = -1;
                    if (main_bar[0] == main_bar[1]) {
                        value[0] = 1;
                        advantage = 1;
                    }
                    else if (main_bar[0] != 1 || main_bar[1] != 1) {
                        if (main_bar[0] > main_bar[1]) {
                            if (main_bar[1] == null || main_bar[1] == "") {
                                main_bar[1] = 1;
                            }
                            value[0] = main_bar[0] / main_bar[1];
                            advantage = 1;
                        }
                        if (main_bar[0] < main_bar[1]) {
                            if (main_bar[0] == null || main_bar[0] == "") {
                                main_bar[0] = 1;
                            }
                            value[0] = main_bar[1] / main_bar[0];
                            advantage = -1;
                        }
                    }
                    else {
                        value[0] = main_bar[0];
                        advantage = 0;
                    }

                    var underreview = $.cookie('is_under_review');
                    if (underreview == false || underreview == null || typeof underreview == "undefined")
                        save_pairwise(value[0], advantage);

                    if (advantage == -1) {
                        numericalSlider[0] = (800 * ((value[0]) / ((value[0]) + 1)));


                    }
                    if (advantage == 1) {
                        numericalSlider[0] = 800 - (800 * ((value[0]) / ((value[0]) + 1)));
                    }

                    //$scope.main_bar[0] = value[0] > 0 ? parseFloat((advantage == 1 ? (value[0]) * 1 : 1).toFixed(2)) : "";
                    //$scope.main_bar[1] = value[0] > 0 ? parseFloat((advantage == -1 ? (value[0]) * 1 : 1).toFixed(2)) : "";
                    main_bar_txt[0] = (advantage == 1 ? (value[0]) * 1 : 1).toFixed(2);
                    main_bar_txt[1] = (advantage == -1 ? (value[0]) * 1 : 1).toFixed(2);
                    if (advantage == 0) {
                        numericalSlider[0] = 400;
                    }


                    if (advantage) {

                        /*setNoUiSlider(numericalSlider[0], numericalSlider[0] + 800, -1);*/
                        setNoUiSlider(numericalSlider[0], numericalSlider[0] + 800, index);
                    }
                    if (getNoUiSlider() != null && advantage == 0 && value[0] == 1) {
                        noUiTimeOut = setTimeout(function () {

                            setNoUiSlider(400, 1200, -1);
                            //$(".noUi-connect").css("background-color", "#0058a3");
                            //$(".graph-green-div").css("background-color", "#6aa84f");
                        }, 100);
                    }
                }
                else {
                    //FOR MULTI
                    var slider_value = 0;
                    if (left_input[index] == right_input[index]) {
                        value[0] = 0;
                        advantage = 1;
                        slider_value = 400;
                    }
                    else if (left_input[index] != 1 || right_input[index] != 1) {
                        if (left_input[index] > right_input[index]) {
                            if (right_input[index] == null || right_input[index] == "") {
                                right_input[index] = 1;
                            }
                            value[0] = left_input[index] / right_input[index];
                            advantage = 1;
                            slider_value = 800 - (800 * ((value[0]) / ((value[0]) + 1)));
                        }
                        if (left_input[index] < right_input[index]) {
                            if (left_input[index] == null || left_input[index] == "") {
                                left_input[index] = 1;
                            }
                            value[0] = right_input[index] / left_input[index];
                            advantage = -1;
                            slider_value = (800 * ((value[0]) / ((value[0]) + 1)));
                        }
                    }
                    else {
                        value[0] = left_input[index];
                        advantage = 0;
                        slider_value = 800;
                        right_input[index] = 1;
                        left_input[index] = 1;
                    }

                    if (left_input[index] !== "" && right_input[index] !== "") {
                        setNoUiSlider(slider_value, slider_value + 800, index);
                        updateMultiPairwiseObjectValue(value, advantage, index, event);
                    }
                }
            }, 200);

        }
        else if (value[0] == null || value[0] == "") {
            //if (is_main_bar) {
            //    output.value = -2147483648000;
            //    output.advantage = 0;

            //    var slider = getNoUiSlider();
            //    slider.noUiSlider.set([400, 1200]);
            //    setNoUiColor(false, -1);
            //} else {
            updateMultiPairwiseObjectValue(-2147483648000, 0, index, event);
            setNoUiSlider(400, 1200, index);
            oldSliderValue[index] = 400;
            setNoUiColor(false, index);
            outerIndex = -1;
            // }
        }
    }
}

var setNoUiSlider = function (upper, lower, index) {

    var slider = getNoUiSlider(index);
    slider.noUiSlider.set([parseFloat(upper), parseFloat(lower)]);
}

var swap_value = function (index) {
    var value;
    var advantage;
    if (index != null) {
        if (left_input[index] != 1 || right_input[index] != 1) {
            if (parseFloat(left_input[index]) > parseFloat(right_input[index])) {
                value = left_input[index] / right_input[index];
                advantage = 1;
            }
            if (parseFloat(left_input[index]) < parseFloat(right_input[index])) {
                value = right_input[index] / left_input[index];
                advantage = -1;
            }

        }
        if (value < -999999999999 || value == 0)
            advantage = advantage;
        else
            advantage *= -1;

        value = typeof (value) == "undefined" ? -2147483648000 : value;
        add_multivalues(value, advantage, index);
        var count = multi_pw_data.length;
        multi_data = multi_pw_data;
        for (i = 0; i < count; i++) {
            if (index == i) {
                if (multi_data[i].Advantage == -1) {
                    var reverse = (800 * ((multi_data[i].Value) / ((multi_data[i].Value) + 1)));
                    participant_slider[i] = reverse;
                }
                if (multi_data[i].Advantage == 1) {
                    participant_slider[i] = 800 - (800 * ((multi_data[i].Value) / ((multi_data[i].Value) + 1)));
                }
                if (multi_data[i].Advantage == 0) {
                    participant_slider[i] = 400;
                }

                if (multi_data[i].Value == 0) {
                    participant_slider[i] = 800;
                    left_input[i] = "";
                    right_input[i] = "";
                }
                var slidervalue = participant_slider[i];
                var lefttxt = left_input[index];
                var righttxt = right_input[index];
                setTimeout(function () {
                    if (lefttxt > 1 || righttxt > 1)
                        setNoUiSlider(slidervalue, slidervalue + 800, index);
                    //console.log(lefttxt + " : " + righttxt);
                    left_input[index] = parseFloat(advantage > 0 ? righttxt * 1 : righttxt);
                    right_input[index] = parseFloat(advantage < 0 ? lefttxt * 1 : lefttxt);

                    left_input[index] = isNaN(left_input[0]) ? '0' : left_input[index];
                    right_input[index] = isNaN(right_input[index]) ? '0' : right_input[index];
                    $('#input1' + index).val(parseFloat(left_input[index]).toFixed(2));
                    $('#input2' + index).val(parseFloat(right_input[index]).toFixed(2));
                }, 0);
            }
        }

        left_input[index] = isNaN(left_input[0]) ? '0' : left_input[index];
        right_input[index] = isNaN(right_input[index]) ? '0' : right_input[index];
        $('#input1' + index).val(parseFloat(left_input[index]).toFixed(2));
        $('#input2' + index).val(parseFloat(right_input[index]).toFixed(2));
    }
    else {

        if (graphical_switch) {
            setTimeout(function () {
                //var slider = getNoUiSlider(index);
                var slider = getNoUiSlider(index);
                var val = slider.noUiSlider.get();
                val[0] = 1600 - parseFloat(val[0]);
                val[1] = 1600 - parseFloat(val[1]);

                slider.noUiSlider.set([val[1], val[0]]);

            }, 0);
        }
        if (main_bar[0] != 1 || main_bar[1] != 1) {
            if (main_bar[0] > main_bar[1]) {
                value = main_bar[0];
                main_bar[0] = main_bar[1];
                advantage = 1;
                main_bar[1] = value;
                value = main_bar[1] / main_bar[0];
            }
            else if (main_bar[0] < main_bar[1]) {
                value = main_bar[1];
                advantage = -1;
                main_bar[1] = main_bar[0];
                main_bar[0] = value;
                value = main_bar[0] / main_bar[1];
            }
        }
        if (value < -999999999999 || value == 0)
            advantage = advantage;
        else
            advantage *= -1;
        value = typeof (value) == "undefined" ? -2147483648000 : value;

        $('#noUiInput11').val(main_bar[0]);
        $('#noUiInput21').val(main_bar[1]);

        save_pairwise(value, advantage);
    }

}

var set_collapse_cookies = function (node_type) {
    var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;
    var bool_hidden = $("." + node_type + "-info-div").is(":visible");
    var currentStepType = (is_multi ? "All_" : "One_") + output.pairwise_type + output.non_pw_type;

    ////console.log("node_type: " + node_type);
    if (node_type == "left-node" || node_type == "right-node" || node_type == "parent-node") {
        if (node_type == "parent-node" && !is_multi && output.page_type == "atPairwise") {
            node_type = "parent-node";
        } else {
            node_type = "pw-nodes";
        }
    }
    else if (node_type == "wrt-left-node" || node_type == "wrt-right-node") {
        if (output.non_pw_type == "mtDirect" || output.non_pw_type == "mtRatings" || currentStepType == "One_mtStep" || currentStepType == "One_mtRegularUtilityCurve") {
            node_type = "pw-nodes";
        } else {
            node_type = "pw-wrt-nodes";
        }
    }

    ////console.log("changed node_type: " + node_type);
    if (output.currentProjectInfo != undefined && output.currentProjectInfo != null
        && output.currentProjectInfo.project_id != undefined && output.currentProjectInfo.project_id != null
        && output.currentProjectInfo.project_id != '' && parseInt(output.currentProjectInfo.project_id) > 0) {
        project_id = output.currentProjectInfo.project_id;
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setCollapseCookies",
        data: JSON.stringify({
            projectId: project_id,
            stepType: currentStepType,
            node_type: node_type,
            status: is_hidden
        }),
        dataType: "json",
        async: false,
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            //$(".tt-accordion-content").attr("style", "");
            if (node_type == "pw-nodes" || node_type == "parent-node") {
                if (is_multi) {
                    setCollapsedInfoDocsValue(0, bool_hidden);
                    setCollapsedInfoDocsValue(1, bool_hidden);
                    setCollapsedInfoDocsValue(2, bool_hidden);
                    setCollapsedInfoDocsValue(3, !bool_hidden);
                    setCollapsedInfoDocsValue(4, !bool_hidden);
                } else {
                    if (node_type == "pw-nodes") {
                        if (output.non_pw_type == "mtDirect" || output.non_pw_type == "mtRatings" || output.non_pw_type == "mtStep" || output.non_pw_type == "mtRegularUtilityCurve") {
                            setCollapsedInfoDocsValue(0, bool_hidden);
                            setCollapsedInfoDocsValue(1, bool_hidden);
                            setCollapsedInfoDocsValue(2, !bool_hidden);
                            setCollapsedInfoDocsValue(3, bool_hidden);
                            setCollapsedInfoDocsValue(4, !bool_hidden);
                        } else {
                            setCollapsedInfoDocsValue(0, !bool_hidden);
                            setCollapsedInfoDocsValue(1, bool_hidden);
                            setCollapsedInfoDocsValue(2, bool_hidden);
                            setCollapsedInfoDocsValue(3, !bool_hidden);
                            setCollapsedInfoDocsValue(4, !bool_hidden);
                        }
                    }
                    else {
                        setCollapsedInfoDocsValue(0, bool_hidden);
                    }
                }
            } else if (node_type == "pw-wrt-nodes") {
                setCollapsedInfoDocsValue(3, bool_hidden);
                setCollapsedInfoDocsValue(4, bool_hidden);
            }

            if (!is_hidden) {
                if (is_judgment_screen) {
                    setTimeout(function () {
                        //change_info_doc_image_size();
                    }, 50);
                }
            }
        },
        error: function (response) {
        }
    });
}

var setCollapsedInfoDocsValue = function (index, newValue) {
    if (collapsed_info_docs[index] != newValue) {
        collapsed_info_docs[index] = newValue;
        //console.log("collapsed_info_docs[" + index + "] got new value");
    }
}

var update_infodoc_params = function (node_type, node_guid, wrt_guid, heading_id) {

    if (is_multi) {
        setTimeout(function () {
            CheckMultiHeight("tt-homepage-main-wrap", "40");
            CheckMultiHeight("multi-loop-wrap", "80");

            CheckMultiHeight("multi-loop-wrap-local-results", "500");
            CheckMultiHeight("tt-pipe-canvas", "152");
            //$scope.CheckMultiHeight("tt-rating-scale", "100");
        }, 800);
        //return false;
    }

    //        alert(node_guid);
    var is_hidden = $("." + node_type + "-info-div").is(":visible") ? 1 : 0;

    //`c=1&w=300&h=200`
    var width = $("." + node_type + "-info-div > .tt-panel").width();
    var height = $("." + node_type + "-info-div > .tt-panel").height();
    var params = "c=" + is_hidden + "&w=" + width + "&h=" + height;
    var pair_params = params; //pass this w/o 't'
    if (typeof (heading_id) != "undefined") {
        is_hidden = is_hidden == 1 ? 0 : 1;
        params = "c=" + is_hidden + "&w=" + width + "&h=" + height;
        if ($("#" + heading_id).val() != "") {
            params = params + "&t=" + angular.element("#" + heading_id).val();
        }
        else {
            params = params + "&t=-2";
        }
    }
    else {
        heading_id = "";
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfodocParams",
        data: JSON.stringify({
            NodeID: node_guid,
            WrtNodeID: wrt_guid,
            value: params,
            is_multi: is_multi,
            NodeType: ''
        }),
        dataType: "json",
        async: false,
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            var index = 0;
            var pair_index = -1;
            var pair_indeces = [];
            var node_guids = [];
            var wrt_guids = [];
            if (node_type == "parent-node") {
                index = 0;

                node_guids[0] = node_guid;
                wrt_guids[0] = wrt_guid;
                pair_indeces.push(0);
                if (output.page_type != "atPairwise" && !is_multi) {
                    node_guids[1] = output.LeftNodeGUID;
                    wrt_guids[1] = "";

                    node_guids[2] = output.LeftNodeGUID;
                    wrt_guids[2] = output.ParentNodeGUID;

                    pair_indeces.push(1);
                    pair_indeces.push(3);
                }
            }
            else if (node_type == "left-node") {
                index = 1;
                node_guid = output.RightNodeGUID;
                wrt_guid = output.LeftNodeGUID;

                node_guids[0] = output.RightNodeGUID;
                wrt_guids[0] = output.LeftNodeGUID;
                pair_index = 2;
                if (is_multi) {

                }
                else {
                    pair_indeces.push(pair_index);

                    if (output.page_type != "atPairwise") {
                        node_guids[1] = output.ParentNodeGUID;
                        wrt_guids[1] = "";

                        node_guids[2] = output.LeftNodeGUID;
                        wrt_guids[2] = output.ParentNodeGUID;

                        pair_indeces.push(0);
                        pair_indeces.push(3);
                    }
                }
            }
            else if (node_type == "right-node") {
                index = 2;
                node_guid = output.LeftNodeGUID;
                wrt_guid = output.RightNodeGUID;

                node_guids[0] = output.LeftNodeGUID;
                wrt_guids[0] = output.RightNodeGUID;
                pair_index = 1;
                if ($scope.is_multi) {

                }
                else {
                    pair_indeces.push(pair_index);
                }
            }
            else if (node_type == "wrt-left-node") {
                index = 3;
                node_guid = output.RightNodeGUID;
                wrt_guid = output.ParentNodeGUID;

                node_guids[0] = output.RightNodeGUID;
                wrt_guids[0] = output.ParentNodeGUID;

                pair_index = 4;
                if (is_multi) {

                }
                else {
                    pair_indeces.push(pair_index);
                    if (output.page_type != "atPairwise") {
                        node_guids[1] = output.ParentNodeGUID;
                        wrt_guids[1] = "";
                        node_guids[2] = output.LeftNodeGUID;
                        wrt_guids[2] = "";
                        pair_indeces.push(0);
                        pair_indeces.push(1);
                    }
                }
            }
            else if (node_type == "wrt-right-node") {
                index = 4;
                node_guid = output.LeftNodeGUID;
                wrt_guid = output.ParentNodeGUID;

                node_guids[0] = output.LeftNodeGUID;
                wrt_guids[0] = output.ParentNodeGUID;

                pair_index = 3;
                if (is_multi) {

                }
                else {
                    pair_indeces.push(pair_index);
                }

            }
            // alert(index + "-" + params);
            $(".tt-accordion-content").attr("style", "");

            if (is_multi) {

            }
            else {
                output.infodoc_params[index] = params;
            }

            //console.log(pair_indeces);
            if (pair_indeces.length > 0 && heading_id == "") {
                $.each(pair_indeces, function (index, pair_index) {
                    //console.log(node_guids[index]);
                    //console.log(wrt_guids[index]);
                    //console.log(pair_params);
                    //console.log("==============================");
                    $.ajax({
                        type: "POST",
                        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfodocParams",
                        data: JSON.stringify({
                            NodeID: node_guids[index],
                            WrtNodeID: wrt_guids[index],
                            value: pair_params,
                            is_multi: is_multi,
                            NodeType: ''
                        }),
                        dataType: "json",
                        async: false,
                        contentType: "application/json; charset=utf-8",
                        success: function () {
                            if (is_multi) {

                            }
                            else {
                                output.infodoc_params[pair_index] = pair_params;
                            }
                        },
                        error: function () {

                        }
                    });
                });
            }
        },
        error: function (response) {
        }
    });
}

var change_info_doc_image_size = function () {
    if (infodocSizes != null) {
        for (var i = 0; i < infodocSizes.length; i++) {
            //var width = "auto";

            $(".tt-resizable-panel-" + i).parent().attr("style", "");
            if (infodocSizes[i].length < 1) {
                $(".tt-resizable-panel-" + i).css("height", "100%");
            } else {
                // $(".tt-resizable-panel-" + i).css("width", width);

                if (!(is_multi)) {
                    var width = infodocSizes[i][0] == null ? 50 : infodocSizes[i][0];
                    var height = infodocSizes[i][1] == null ? 50 : infodocSizes[i][1];
                    if (i == 1) {
                        $(".tt-resizable-panel-0").css("width", width);
                    }
                    if (i != 0) {
                        $(".tt-resizable-panel-" + i).css("height", height);
                    }
                    $(".tt-resizable-panel-" + i).css("width", width);
                    if (i == 0) {
                        $(".tt-resizable-panel-" + i).css("height", "auto !important");
                    }
                }
            }

            //  add_resizable_info_doc_event_handler(i);
        }

        // callResizeFrameImagesProportionally(100, "change_info_doc_image_size");
    }

    // hide_loading_modal();
}

//Multi rating
var ActiveDirect = function (index) {
    CurrrentSelectedRow = index;
    $('div.row_wrapper').removeClass('active_table_row');
    $('#multi_direct_slider_' + index).addClass('active_table_row');

    for (i = 0; i < at_multi_direct_input.length; i++) {
        if (index !== i.toString() && $('#comment_div_box_' + i).css('display') === 'block') {
            $('#comment_div_box_' + i).css('display', 'none');
        }

        if ($('#multi_direct_slider_' + i).hasClass('active_table_row')) {
            if (parseFloat(at_multi_direct_slider[i]) >= 0)
                $('#close_icon_' + i).css({ 'filter': 'none', 'opacity': '1' })
            else
                $('#close_icon_' + i).css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
        }
        else
            $('#close_icon_' + i).css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
    }

    setTimeout(function () {
        var $container = $('div.main_wrapper'), $scrollTo = $('#multi_direct_slider_' + parseInt(index));
        if (isMobile) {
            $container.animate({
                scrollTop: $scrollTo.position().top - 240 - $container.position().top + $container.scrollTop()
            });
        }
        else {
            $container.animate({
                scrollTop: $scrollTo.position().top - 200 - $container.position().top + $container.scrollTop()
            });
        }
        //if ($('#multi_direct_slider_' + parseInt(index + 1)).length > 0) {
        //    $container.animate({
        //        scrollTop: $scrollTo.position().top - 200 - $container.position().top + $container.scrollTop()
        //    });
        //}
        //else if (index > 2) {

        //}
    }, 50);
}

var checkMultiValue = function (multiData, fNonPw) {
    var isUndefined = false;

    for (i = 0; i < multiData.length; i++) {
        if (fNonPw == 1) {
            var value = multiData[i][0];
            if (value < 0 || value == "") {
                isUndefined = true;
            }
        }
        else if (fNonPw == 2) {
            var value = multiData[i];
            if (value < 0 || value == "") {
                isUndefined = true;
            }
        }
        else {
            if (multiData[i].Advantage == 0 && multiData[i].Value < 1) {
                isUndefined = true;
            }
        }
    }

    IsUndefined = isUndefined;
}

var add_multi_ratings_values = function (rating_id, index, $event, value) {
    if ($event) {
        $event.stopPropagation(); //for stopping parent element ng-click event
    }

    $("#ratings-dropdown" + index + " .hidden-item-wrap").slideUp();
    $(".cdd_" + index).hide();
    $(".arrow_" + index).show();

    if (active_multi_index !== index) {
        $(".multi-ratings-list .selected").removeClass("selected"); //11690
    }

    active_multi_index = index;
    var is_direct = false;
    var is_not_rated = false;

    if (rating_id >= 0) {
        multi_non_pw_data[active_multi_index].RatingID = rating_id;
    }
    else {
        if (rating_id == -2) {
            if (value >= 0 && value <= 1) {
                multi_non_pw_data[active_multi_index].RatingID = -1;
                multi_non_pw_data[active_multi_index].DirectData = multi_direct_value[active_multi_index];
                is_direct = true;
            }
            else {
                multi_non_pw_data[active_multi_index].RatingID = -1;
                multi_non_pw_data[active_multi_index].DirectData = -1;
                multi_direct_value[active_multi_index] = "";
                is_not_rated = true;
                is_direct = true;
            }
        }
        else {
            multi_non_pw_data[active_multi_index].RatingID = -1;
            multi_non_pw_data[active_multi_index].DirectData = -1;
            multi_direct_value[active_multi_index] = "";
            is_not_rated = true;
        }
    }

    setTimeout(function () {
        if (is_not_rated) {
            multi_direct_value[$scope.active_multi_index] = "";
        }

        multi_ratings_row[active_multi_index] = get_multi_ratings_data(rating_id, multi_direct_value[active_multi_index], active_multi_index);
        var multiData = multi_ratings_row;

        if (rating_id >= 0) {
            multi_direct_value[active_multi_index] = "";
            var i = active_multi_index;
            multi_direct_value[i] = multi_ratings_row[i][0] != -1 ? multi_ratings_row[i][0] : multi_direct_value[i];
        }

        if (is_direct == false && !is_not_rated) {
            //Adding flash to judgment row when user enters judgment
            if (active_multi_index >= 0) {
                $("#multi-row-" + active_multi_index).addClass("judgment-flash");
            }

            select_multi_ratings_row(active_multi_index + 1, true);
        }

        multiData = multi_direct_value;
        checkMultiValue(multi_ratings_row, 1);
        active_multi_index = index;
    }, 300);
}

var get_multi_ratings_data = function (rating_id, direct_value, index) {
    //$scope.refreshed = true;

    if (rating_id < 0) {
        if (direct_value >= 0) {
            return [direct_value, "Direct Value"];
        }
        else {
            return ["", "Not Rated"];
        }
    }
    else {
        for (var i = 0; i < multi_non_pw_data[index].Ratings.length; i++) {
            if (multi_non_pw_data[index].Ratings[i].ID == rating_id) {
                return [multi_non_pw_data[index].Ratings[i].Value, multi_non_pw_data[index].Ratings[i].Name];
            }
        }
    }
}

var select_multi_ratings_row = function (index, judgmentAdded) {
    //var timeOutValue = $scope.isMobile() ? 300 : 1000;
    setTimeout(function () {
        $(".multi-ratings-row").removeClass("judgment-flash");  //Removing flash from judgment row after desired time
    }, 1000);

    if (index > multi_non_pw_data.length - 1) {
        index = 0;
        activate_first_row = true;
    }
    else {
        activate_first_row = false;
        active_multi_index = index;

        //Change row selection when judgment flash stops
        setTimeout(function () {
            $(".multi-ratings-row").removeClass("selected");
            $("#multi-row-" + index).addClass("selected");

            $(".multi-ratings-row").addClass("fade-fifty");
            $("#multi-row-" + index).removeClass("fade-fifty");
        }, 1000);

        selected_multi_data[index] = multi_non_pw_data[index];
    }

    //Hiding value from direct input field when value is not direct
    var directValue = selected_multi_data[index].RatingID == -1 && selected_multi_data[index].DirectData != 1 ? multi_direct_value[index] : "";
    if (directValue == "") {
        $("#multiRatingsDirectValue").css("color", "white");    //Setting color to white on direct input filed so that user doesn't see the value
        setTimeout(function () {
            $("#multiRatingsDirectValue").val(directValue); //Removing value from direct input field after a timeout
            $("#multiRatingsDirectValue").css("color", ""); //Removing color from direct input field
        }, 1000);
    }

    if (index > 0) {
        //Changing selected row after desired time so that user can see flash on judgment row
        setTimeout(function () {
            set_multi_index(index);
            callGetCollapseCookies();
        }, timeOutValue - 200);
    }

    //if (activate_first_row) {
    //    angular.element(".next_step").addClass("back-pulse");
    //}
    //else {
    //    $scope.scroll_to_index = $scope.active_multi_index - 2;

    //    if ($scope.scroll_to_index <= 1) {
    //        //do not scroll
    //    }
    //}

    //if (!(judgmentAdded && $scope.isMobile())) {
    //    $scope.selectNextMultiRatingsRow(index);
    //}
}

var set_multi_index = function (index) {
    $(".multi-rows").removeClass("selected");

    var callApply = active_multi_index != index;
    active_multi_index = index;

    $(".multi-row-" + active_multi_index).addClass("selected");
    $(".multi-row-" + index + " .not-disabled").addClass("change-color-pulse");

    try {
        if (hdnpage_type == "atAllPairwise" || hdnpage_type == "atAllPairwiseOutcomes") {
            setTimeout(function () {
                mobile_manual_equalizer();
            }, 2, callApply);
        }
    }
    catch (e) {
    }

    setTimeout(function () {
        $(".not-disabled").removeClass("change-color-pulse");
        $(".disabled").removeClass("change-color-pulse");
        scroll_to_active_index();
    }, 200, false);

    //callResizeFrameImagesProportionally(200, "set_multi_index " + index);
}

var mobile_manual_equalizer = function () {

    $(".selected .left-question").attr("style", "height: !important;");
    $(".selected .right-question").attr("style", "height: !important;");

    var height = $(".selected .right-question").outerHeight();
    if ($(".selected .left-question").outerHeight() > height) {
        height = $(".selected .left-question").outerHeight();
    }

    var minHeight = 40;
    height = height < minHeight ? minHeight : height + 5;

    if (height != 0) {
        $(".left-question").attr("style", "");
        $(".right-question").attr("style", "");

        $(".selected .left-question").attr("style", "height:" + height + "px !important;");
        $(".selected .right-question").attr("style", "height:" + height + "px !important;");
    }

    $(".multi-rows").each(function (i, element) {
        if (!$(element).hasClass("selected")) {
            var height = $(element).find(".right-question").outerHeight();
            if ($(element).find(".left-question").outerHeight() > $(element).find(".right-question").outerHeight()) {
                height = $(element).find(".left-question").outerHeight();
            }
            height = height < 50 ? 50 : height + 5;
            if (height != 0) {
                //balance the uneven boxes
                if ($(element).find(".left-question").outerHeight() != $(element).find(".right-question").outerHeight()) {
                    $(element).find(".left-question").attr("style", "height:" + height + "px !important;");
                    $(element).find(".right-question").attr("style", "height:" + height + "px !important;");
                }
            }
        }
    });
}

var save_multi_ratings_on_next = function () {
    var multivalues = [];
    var updatedIntensities = [];    //for intensity description

    for (i = 0; i < multi_non_pw_data.length; i++) {
        try {
            var vals = [];
            vals[0] = multi_non_pw_data[i].RatingID;
            vals[1] = multi_non_pw_data[i].DirectData;
            vals[2] = multi_non_pw_data[i].Comment;
            multivalues[i] = vals;

            var ratings = multi_non_pw_data[i].Ratings;
            var newRatings = [];
            for (var j = 0; j < ratings.length; ++j) {
                newRatings[j] = [];
                newRatings[j][0] = ratings[j].GuidID;
                newRatings[j][1] = ratings[j].Comment;
            }
            updatedIntensities[i] = newRatings;
        }
        catch (e) {

        }
    }

    //do ajax for update judgement backend
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveMultiRatingsData",
        data: JSON.stringify({
            step: hdnCurrentStep,
            multivalues: multivalues,
            intensities: updatedIntensities
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var output = JSON.parse(data.d);
            //$('#hdnPageNumber').val(output);
            //$('#MainContent_hdnPageNo').trigger('click');
        },
        error: function (response) {
        }
    });
}
function roundToTwo(num) {
    return +(Math.round(num + "e+2") + "e-2");
}

var addActiveClass = function (event, cls, index) {
    CurrrentSelectedRow = index;
    for (i = 0; i <= multi_non_pw_data.length; i++) {
        if (i.toString() != index) {
            $('#div_row_wrapper_' + i + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
            HideDropDownList('#UiDropdownList' + i);
            HideDropDownList('#UiDropdownList' + i + 'D');
        }
    }
    if (event != undefined && event != null) {
        $(event).nextAll('.row_wrapper').removeClass('active_table_row');

        $(event).prevAll('.row_wrapper').removeClass('active_table_row');
        $(event).addClass('active_table_row');

        var itm = multi_non_pw_data[index];
        if (itm != undefined && itm != null && new_active_multi_index != index) {
            new_active_multi_index = index;
            //var hdnshowPriorityAndDirect = $('#MainContent_MultiRatingsControl_hdnshowPriorityAndDirect').val();
            var html = '';
            html += "<div class='divtable_title'>";
            html += "<p title='" + itm.Title + "'>" + itm.Title + "</p>";
            html += "</div>";
            html += "<div class='divtable_header'>";
            html += "<div class='product_name'>";
            html += "<p>Intensity Name</p>";
            html += "</div>";
            if (output.showPriorityAndDirect) {
                html += "<div class='product_intensity'>";
                html += "<p>Priority</p>";
                html += "</div>";
            }
            html += "</div>";
            html += "<div class='form-check radio_progress'>";
            for (var i = 0; i < itm.Ratings.length; i++) {
                var rating = itm.Ratings[i];
                var readStyle = output.isPipeViewOnly ? ' style="pointer-events:none;"' : '';
                if (output.showPriorityAndDirect || itm.Ratings[i].Name != "Direct Value") {
                    html += "<div class='form-check radio_progress customvalue'>";
                    html += "<input" + readStyle + " class='form-check-input intensity" + i + "' type='radio' onchange=SetValuesFromRadio('" + index + "','" + i + "') onclick=SetValuesFromRadio('" + index + "','" + i + "') name='" + itm.sGUID + "' id='" + rating.GuidID + "' value='" + rating.Value + "' checked/>";
                    html += "<label" + readStyle + " class='form-check-label' >";
                    if (rating.Comment != "") {
                        html += "<div class='radio_btnname' onclick=SetValuesFromRadio('" + index + "','" + i + "')>" + rating.Name + "<span class='idesc intensitydesc" + i + "' title='" + rating.Comment + "'> - " + rating.Comment + "</span>";
                        if (output.isPM) {
                            html += "<a href='javascript:void(0);' class='ms-2 prm-color' onclick='showNode(6" + i + ");return false'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>";
                        }
                        else {
                            html += "<a href='javascript:void(0);' class='ms-2 prm-color' onclick='showNode(6" + i + ");return false'><img class='chkEvaluatorone d-none' src='../../img/icon/edit-icon.svg'></a>";
                        }
                        html += "</div>";
                    }
                    else {
                        html += "<div class='radio_btnname' onclick=SetValuesFromRadio('" + index + "','" + i + "')>" + rating.Name + "<span class='idesc intensitydesc" + i + "'></span>";
                        if (output.isPM) {
                            html += "<a href='javascript:void(0);' class='ms-2 prm-color' onclick='showNode(6" + i + ");return false'><img class='chkEvaluatorone' src='../../img/icon/edit-icon.svg'></a>";
                        }
                        else {
                            html += "<a href='javascript:void(0);' class='ms-2 prm-color' onclick='showNode(6" + i + ");return false'><img class='chkEvaluatorone d-none' src='../../img/icon/edit-icon.svg'></a>";
                        }
                        html += "</div>";
                    }
                    //html += " </div >";

                    //html += "<div class='tooltips_group1 w-auto'>";
                    //if (j == multi_non_pw_data.length - 1) {
                    //    html += "<div class='info_tooltip_main position-relative scndLastColumn'>";
                    //}
                    //else {
                    //    html += "<div class='info_tooltip_main position-relative'>";
                    //}
                    html += "<div class='comment_div'>";
                    //if (i != itm.Ratings.length - 1) {
                    //    html += "<i class='bx bxs-edit-alt chkEvaluator d-none'  onclick='showNode(6" + i + ")'></i>";
                    //}
                    html += "<div class='info_tooltip right_tooltip hideshowinfo' id='parentnode6" + i + "' style='display: none;'>";
                    html += "<div class='d-flex justify-content-between'>";
                    html += "<span>Edit intensity description</span>";
                    html += "<div class='action_icons'><a href='javascript: void (0);' onclick='hideNode(6" + i + ")'><i class='fa fa-times' aria-hidden='true'></i></a></div>"
                    html += "</div>";
                    html += "<textarea" + readStyle + " class='form-control mb-3 mt-2 w-100' id='txtleftComment_" + i + "' rows='2'>" + rating.Comment + "</textarea>";
                    html += "<div class='comt_btn text-end'>";
                    readStyle = output.isPipeViewOnly ? ' class="d-none"' : '';
                    if (!isMobile)
                        html += "<button" + readStyle + " type='button' onclick=updateRightSideComment('" + i + "','showcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>";
                    else
                        html += "<button" + readStyle + " type='button' onclick=updateRightSideCommentMobile('" + i + "','showcomment','close',event)><i class='fa fa-check' aria-hidden='true'></i> OK</button>";
                    html += "</div></div></div>";

                    if (output.showPriorityAndDirect) {

                        readStyle = output.isPipeViewOnly ? ' style="pointer-events:none;"' : '';
                        if (i != itm.Ratings.length - 1)
                            html += "<div class='radio_btn_progress'" + readStyle + ">";
                        else
                            html += "<div class='directvalue_btn_progress'" + readStyle + "  onclick='CloseList()'>";
                        if (i != itm.Ratings.length - 1) {
                            html += "<div class='progress'>";
                            html += "<div class='progress-bar' onclick='SetValuesFromRadio(" + index + ", " + i.toString() + ")' role='progressbar' style='width: " + parseFloat(itm.Ratings[i].Value * 100) + "%' aria-valuenow='" + parseFloat(itm.Ratings[i].Value * 100) + "' aria-valuemin='0' aria-valuemax='100'></div>";
                            html += "</div>";
                            if (i != itm.Ratings.length - 1) {
                                if (itm.Ratings[i].Name == "Direct Value" || itm.Ratings[i].Name == "Not Rated") {
                                    html += "";
                                }
                                else {
                                    html += "" + parseFloat(rating.Value * 100).toFixed(2) + "%";
                                }
                            }
                            else {
                                html += "";
                            }
                        }
                        else {
                            html += "<input type='text' onkeypress='return isNumberKeyWithDecimalMR(this, event);' class='custom-value' id='input_2_" + index + "' onkeyup=setValuesFromText('" + index + "','2')>";
                            html += "<img src='../../img/icon/erasar.svg' onclick=ResetValue('" + itm.sGUID + "','" + index + "',event); style='cursor: pointer;' class='imgClose' ></img>";
                        }
                        html += "</div>";
                    }
                    html += "</label>";
                    html += "</div>";
                }
            }
            //if (output.showinfodocnode) {
            //    html += "<div class='bottom_info'>";
            //    html += $('div.bottom_info').html();
            //    html += "</div>";
            //}
            html += "</div>";

            $('#divRadioButtons').html('');
            $('#divRadioButtons').html(html);

            for (var i = 0; i < multi_non_pw_data[index].Ratings.length; i++) {
                var Ratings = multi_non_pw_data[index].Ratings[i];
                if (Ratings.Name == multi_ratings_row[index][1]) {
                    var DropVal = parseFloat(multi_ratings_row[index][0] * 100);
                    if (DropVal.toString().length > 3) { DropVal = DropVal.toFixed(2) }

                    var PerCentage = ' ' + DropVal + '%';
                    if (Ratings.Name.toLowerCase() == 'not rated')
                        PerCentage = '';
                    $("input[type='radio'][id='" + Ratings.GuidID + "']").prop("checked", true);
                    if (output.showPriorityAndDirect) {
                        if (DropVal > 0 || Ratings.Name.toLowerCase() == 'non') {
                            $('#dropdownHeaderValue' + index).text(Ratings.Name + PerCentage);
                        }
                        else {
                            $('#dropdownHeaderValue' + index).text(Ratings.Name + PerCentage);
                        }
                        $('#spndropdownHeaderValueM' + index).text(Ratings.Name + PerCentage);
                    }
                    else {
                        $('#dropdownHeaderValue' + index).text(Ratings.Name);
                        $('#spndropdownHeaderValueM' + index).text(Ratings.Name);
                    }
                    var Value = multi_ratings_row[index][0];
                    if (Value != '') {

                        $('#progress_bar_' + index).attr('aria-valuenow', parseFloat(Value * 100).toFixed(4)).css('width', parseFloat(Value * 100).toFixed(4) + '%');
                        $('#dropdownHeaderValueM' + index).attr('aria-valuenow', parseFloat(Value * 100).toFixed(4)).css('width', parseFloat(Value * 100).toFixed(4) + '%');
                    }
                    else {
                        $('#progress_bar_' + index).attr('aria-valuenow', 0).css('width', '0%');
                        $('#dropdownHeaderValueM' + index).attr('aria-valuenow', 0).css('width', '0%');
                    }

                    if (multi_ratings_row[index][1] != 'Not Rated' && Value * 100 >= 0) {
                        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                        $('#divRadioButtons .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                    }
                    else {
                        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                        $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                    }
                }
            }
            /*    $.cookie("GenActiveRowID", index);*/

            if (multi_ratings_row[index][1].trim().toLocaleLowerCase() == 'direct value') {
                $('#input_1_' + index).val(multi_ratings_row[index][0]);
                $('#input_2_' + index).val(multi_ratings_row[index][0]);
                $('#input_2_' + 0).val(multi_ratings_row[index][0]);
            }
            else {
                $('#input_2_' + index).val('');
            }
        }

        //var el = $('#div_row_wrapper_' + parseInt(index));
        ////var elOffset = el.offset().top;
        ////var elHeight = el.height();
        ////var windowHeight = $(window).height();
        ////var offset;

        ////if (elHeight < windowHeight) {
        ////    offset = elOffset - ((windowHeight / 2) - (elHeight / 2));
        ////}
        ////else {
        ////    offset = elOffset;
        ////}
        ////var speed = 700;
        //$('.rating_scale_data').animate({ scrollTop: el.offset().top - 150 }); 

        if (isMobile) {
            var $container = $('div.main_wrapper'), $scrollTo = $('#div_row_wrapper_' + parseInt(index));
            $container.animate({
                scrollTop: $scrollTo.offset().top - $container.offset().top + $container.scrollTop() - 20
            });
        }
        else {
            var $container = $('div.rating_scale_data'), $scrollTo = $('#div_row_wrapper_' + parseInt(index));
            if (hdnpage_type != "atPairwise") {
                $container.animate({
                    scrollTop: $scrollTo.offset().top - $container.offset().top + $container.scrollTop()
                });
            }
        }

    }
    else {
        if (hdnMultiPwData.length > 0) {
            if (hdnMultiPwData[index].Value < 0) {
                $('#index_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
            }
            else {
                $('#index_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
            }
        }
        else if (output.non_pw_value <= 0) {
            $('.imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }
        else {
            if (output.value > 0) {
                $('.chart_wrapper .imgClose').css({ 'filter': 'none', 'opacity': '1' });
            }
            else {
                $('.chart_wrapper .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            }
        }
    }
}

var ResetValue = function (forId, index, event) {

    if (output.pipeOptions.dontAllowMissingJudgment) {
        EnableDisablePagination(true);
    }
    if ($('#div_row_wrapper_' + index).hasClass('active_table_row')) {
        event.stopPropagation();
    }
    else {
        $('#div_row_wrapper_' + index).addClass('active_table_row');
        addActiveClass($('#div_row_wrapper_' + index), '', index);
    }
    //event.stopPropagation();
    if (forId != undefined && forId != null && forId != '') {
        $('#input_1_' + index).val('');
        $('#input_2_' + index).val('');
        var id = '';
        for (var i = 0; i < multi_non_pw_data[index].Ratings.length; i++) {
            var rating = multi_non_pw_data[index].Ratings[i];
            if (rating.Name.toLowerCase() == 'not rated') {
                text = rating.Name;
                id = rating.GuidID;
                rating.Value = 0;
                multi_non_pw_data[index].RatingID = rating.ID;
                multi_non_pw_data[index].DirectData = -1;
                multi_non_pw_data[index].Ratings[i] = rating;

                multi_ratings_row[index][0] = '0';
                multi_ratings_row[index][1] = text;
            }
        }
        $('#dropdownHeaderValue' + index).text(text);
        if (output.showPriorityAndDirect)
            $('#spndropdownHeaderValueM' + index).text(text + ' ' + parseFloat(multi_ratings_row[index][0] * 100) + '%');
        else
            $('#spndropdownHeaderValueM' + index).text(text);


        if (parseFloat(multi_ratings_row[index][0] * 100) > 0) {
            $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }
        else {
            $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }

        $('#progress_bar_' + index).attr('aria-valuenow', '0').css('width', '0%');
        $('#dropdownHeaderValueM' + index).attr('aria-valuenow', '0').css('width', '0%');
        $("input[type='radio'][name='" + forId + "']").prop("checked", false);
        $("input[type='radio'][id='" + id + "']").prop("checked", true);
        $('#close_icon_' + index + ' i').hide();
        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        $('#UiDropdownList' + index + 'D').hide();

    }
}

var toggleFunction = function (index, event) {
    event.stopPropagation();
    for (var i = 0; i < $('div.row_wrapper').length; i++) {
        listItems = $('#dropdownList' + i + ' li');
        dropdownHeader = $('#dropdownHeader' + i);
        chevronIcon = $('#chevronIcon' + i);
        dropdownListBody = $('#dropdownListBody' + i);
        dropdownHeaderValue = $('#dropdownHeaderValue' + i);
        isDropdownDisabled = $('#dropdown' + i).hasClass("disabled");
        dropdown = $('#dropdown' + i);

        if (i != parseInt(index)) {
            chevronIcon.removeClass('rotate-icon-home');
            chevronIcon.removeClass('rotate-icon');
            dropdownListBody.removeClass('dropdown__body--hide');
            dropdownListBody.removeClass('dropdown__body--show');
            dropdownHeader.removeClass('dropdown__header--hide');
            dropdownHeader.removeClass('dropdown__header--show');

            chevronIcon.addClass('rotate-icon-home');
            dropdownListBody.addClass('dropdown__body--hide');
            dropdownHeader.addClass('dropdown__header--hide');
        }
        else {
            chevronIcon.toggleClass('rotate-icon-home');
            chevronIcon.toggleClass('rotate-icon');
            dropdownListBody.toggleClass('dropdown__body--hide');
            dropdownListBody.toggleClass('dropdown__body--show');
            dropdownHeader.toggleClass('dropdown__header--hide');
            dropdownHeader.toggleClass('dropdown__header--show');
        }
    }

    //var el = $('#div_row_wrapper_' + parseInt(index));
    //var elOffset = el.offset().top;
    //var elHeight = el.height();
    //var windowHeight = $(window).height();
    //var offset;

    //if (elHeight < windowHeight) {
    //    offset = elOffset - ((windowHeight / 2) - (elHeight / 2));
    //}
    //else {
    //    offset = elOffset;
    //}
    //var speed = 700;
    //$('html, body').animate({ scrollTop: offset }, 'slow');
    //$('html, body, div.progress_table_row').animate({
    //    scrollTop: $('#div_row_wrapper_' + parseInt(index)).offset().top - 50
    //}, 'slow');
    //chevronIcon.nextAll().toggleClass('rotate-icon-home');
    //chevronIcon.nextAll().toggleClass('rotate-icon');
    //dropdownListBody.nextAll().toggleClass('dropdown__body--hide');
    //dropdownListBody.nextAll().toggleClass('dropdown__body--show');
    //dropdownHeader.nextAll().toggleClass('dropdown__header--hide');
    //dropdownHeader.nextAll().toggleClass('dropdown__header--show');

    //chevronIcon.prevAll().toggleClass('rotate-icon-home');
    //chevronIcon.prevAll().toggleClass('rotate-icon');
    //dropdownListBody.prevAll().toggleClass('dropdown__body--hide');
    //dropdownListBody.prevAll().toggleClass('dropdown__body--show');
    //dropdownHeader.prevAll().toggleClass('dropdown__header--hide');
    //dropdownHeader.prevAll().toggleClass('dropdown__header--show');    
}

var getSelectText = function (index, text, val, id, event) {
    if ($('#div_row_wrapper_' + index).hasClass('active_table_row')) {
        event.stopPropagation();
    }
    else {
        $('#div_row_wrapper_' + index).addClass('active_table_row');
        addActiveClass($('#div_row_wrapper_' + index), '', index);
    }
    new_active_multi_index = index;
    //listItems = $('#dropdownList' + index + ' li');
    //dropdownHeader = $('#dropdownHeader' + index);
    //chevronIcon = $('#chevronIcon' + index);
    //dropdownListBody = $('#dropdownListBody' + index);
    //dropdownHeaderValue = $('#dropdownHeaderValue' + index);
    //isDropdownDisabled = $('#dropdown' + index).hasClass("disabled");
    //dropdown = $('#dropdown' + index);

    //chevronIcon.toggleClass('rotate-icon-home');
    //chevronIcon.toggleClass('rotate-icon');
    //dropdownListBody.toggleClass('dropdown__body--hide');
    //dropdownListBody.toggleClass('dropdown__body--show');
    //dropdownHeader.toggleClass('dropdown__header--hide');
    //dropdownHeader.toggleClass('dropdown__header--show');

    $('#progress_bar_' + index).css('width', val + '%');
    if (val > 0 || text.toLowerCase() == 'none') {
        if (val.toString().length > 3)
            $('#dropdownHeaderValue' + index).text(text + ' ' + parseFloat(val).toFixed(2) + '%');
        else
            $('#dropdownHeaderValue' + index).text(text + ' ' + parseFloat(val) + '%');
    }
    else {
        $('#dropdownHeaderValue' + index).text(text)
    }
    if (output.showPriorityAndDirect)
        $('#spndropdownHeaderValueM' + index).text(text + ' ' + parseFloat(val).toFixed(2) + '%');
    else
        $('#spndropdownHeaderValueM' + index).text(text);
    $('#progress_bar_' + index).attr('aria-valuenow', val).css('width', val + '%');
    $('#dropdownHeaderValueM' + index).attr('aria-valuenow', val).css('width', val + '%');
    $("input[type='radio']").prop("checked", false);
    $("input[type='radio'][id='" + id + "']").prop("checked", true);
    $('#input_1_' + index).val(parseFloat(val / 100).toFixed(4));

    if (text == 'Not Rated' || text == 'Direct Value') {
        $('#input_1_' + index).val('');
        $('#close_icon_' + index + ' i').hide();
    }
    else {
        $('#close_icon_' + index + ' i').show();
    }

    if (text != 'Not Rated' && val >= 0) {
        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
    }
    else {
        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
    }

    if (multi_non_pw_data.length > 0) {
        for (var i = 0; i < multi_non_pw_data[index].Ratings.length; i++) {
            var item = multi_non_pw_data[index].Ratings[i];
            if (item.GuidID == id) {
                multi_non_pw_data[index].RatingID = item.ID;
                multi_ratings_row[index][0] = item.Value;
                multi_ratings_row[index][1] = item.Name;
            }
        }
    }

    if (output.pipeOptions.dontAllowMissingJudgment) {
        var disablePage = false;
        for (var i = 0; i < multi_ratings_row.length; i++) {
            if (multi_ratings_row[i][1] == 'Not Rated' || multi_ratings_row[i][1] == 'None') {
                disablePage = true;
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

    toggleFunction(index, event);
    HideDropDownList(OldDropDownListID);
    setTimeout(function () {
        var NextIndex = GetNextMultiRatingIncompleteJudgmentInex(index);
        if (NextIndex > -1) {
            $('#div_row_wrapper_' + index).removeClass('active_table_row');
            $('#div_row_wrapper_' + (parseInt(NextIndex))).addClass('active_table_row');
            /*$('#div_row_wrapper_' + parseInt(index + 1)).toggle('click');*/
            addActiveClass($('#div_row_wrapper_' + (parseInt(NextIndex))), '', (parseInt(NextIndex)).toString());
        }
        else {
            if ($('#div_row_wrapper_' + (parseInt(index) + 1)).length > 0) {
                $('#div_row_wrapper_' + index).removeClass('active_table_row');
                $('#div_row_wrapper_' + (parseInt(index) + 1)).addClass('active_table_row');
                /*$('#div_row_wrapper_' + parseInt(index + 1)).toggle('click');*/
                addActiveClass($('#div_row_wrapper_' + (parseInt(index) + 1)), '', (parseInt(index) + 1).toString());
            }
        }
        //else {
        //    $('#div_row_wrapper_0').addClass('active_table_row');
        //    /*$('#div_row_wrapper_0').toggle('click');*/
        //    addActiveClass($('#div_row_wrapper_0'), '', '0');
        //}
    }, 500);
}
function selectItem(_this) {
    dropdownHeaderValue.innerText = _this.innerText;

    Array.from(listItems).forEach((item) => {
        item.classList.remove('selected');
    });

    _this.classList.add('selected');
}

var updateRightSideCommentMobile = function (index, type, operation, event) {
    //event.stopPropagation();
    var dindex = index;
    index = index == undefined || null ? 0 : index;
    type = type == undefined || null ? 0 : type;
    operation = operation == undefined || null ? 0 : operation;

    $('.hideshowinfo').css('display', 'none');
    if (type == 'comment') {
        if (operation == 'close') {
            /* $('#txtRightComment_' + index).val('');*/
            multi_non_pw_data[index].Comment = $('#txtRightComment').val();
            //$('#comment_icon_' + index).removeClass('far fa-comments');
            //$('#comment_icon_' + index).removeClass('fa fa-comments');
            //$('#comment_icon_' + index).removeClass('filledComment');
            //$('#comment_icon_' + index).removeClass('emptyComment');
            //if ($('#txtRightComment_' + index).val() != '') {
            //    $('#comment_icon_' + index).addClass('fa fa-comments');
            //    $('#comment_icon_' + index).addClass('filledComment');
            //}
            //else {
            //    $('#comment_icon_' + index).addClass('far fa-comments');
            //    $('#comment_icon_' + index).removeClass('emptyComment');
            //}         

            //$('#comment_div_box_' + index).css('display', 'none');
            $('#comment_mobile').modal('hide');
            HideShowComment_icon_mobile(index);
        }
        if (operation == 'open') {
            $('#txtRightComment').val(multi_non_pw_data[index].Comment);
            $('#comment_mobile').modal('show');
            $('#btnUpdateRightSideComment').attr('onclick', 'updateRightSideCommentMobile(' + index + ', "comment", "close", event)')
        }

    }
    else if (type == 'Ratingcomment') {
        if (operation == 'close') {
            output.comment = $('#txtRightComment').val();
            //$('#comment_icon_' + index).removeClass('far fa-comments');
            //$('#comment_icon_' + index).removeClass('fa fa-comments');
            //$('#comment_icon_' + index).removeClass('filledComment');
            //$('#comment_icon_' + index).removeClass('emptyComment');

            //if ($('#txtRightComment_' + index).val() != '') {
            //    $('#comment_icon_' + index).addClass('fa fa-comments');
            //    $('#comment_icon_' + index).addClass('filledComment');
            //}
            //else {
            //    $('#comment_icon_' + index).addClass('far fa-comments');
            //    $('#comment_icon_' + index).removeClass('emptyComment');           
            //}
            $('#comment_mobile').modal('hide');
            HideShowComment_icon(index);
            save_ratings_on_next();
        }
        if (operation == 'open') {
            $('#txtRightComment').val(output.comment);
            $('#comment_mobile').modal('show');
            $('#btnUpdateRightSideComment').attr('onclick', 'updateRightSideCommentMobile(' + index + ', "Ratingcomment", "close", event)')
        }
    }
    else if (type == 'DirectComment') {
        if (operation == 'close') {
            output.comment = $('#txtRightComment_' + index).val();
            //$('#comment_icon_' + index).removeClass('far fa-comments');
            //$('#comment_icon_' + index).removeClass('fa fa-comments');
            //$('#comment_icon_' + index).removeClass('filledComment');
            //$('#comment_icon_' + index).removeClass('emptyComment');

            //if ($('#txtRightComment_' + index).val() != '') {
            //    $('#comment_icon_' + index).addClass('fa fa-comments');
            //    $('#comment_icon_' + index).addClass('filledComment');
            //}
            //else {
            //    $('#comment_icon_' + index).addClass('far fa-comments');
            //    $('#comment_icon_' + index).removeClass('emptyComment');
            //}parentnode16
            // $('#comment_div_box_' + index).css('display', 'none');
            $('#parentnode' + index).css('display', 'none');
            save_direct_on_next();
        }
        if (operation == 'open') {
            $('#comment_div_box_' + index).css('display', 'block');
        }
    }
    else if (type == 'tooltip') {
        $('#tooltip_wrapper_' + index).toggle();
        if (operation == 'open') {
            $('#tooltip_wrapper_' + index).css('display', 'block');
        }
        else if (operation == 'close') {
            $('#tooltip_wrapper_' + index).css('display', 'none');
        }
        else {
            $('#tooltip_wrapper_' + index).css('display', 'none');
        }
    }
    else if (type == 'bandage') {
        //$('#w_bandage_wrapper_' + index).toggle();
        if (operation == 'open') {
            $('#w_bandage_wrapper_' + index).css('display', 'block');
        }
        else if (operation == 'close') {
            $('#w_bandage_wrapper_' + index).css('display', 'none');
        }
        else {
            $('#w_bandage_wrapper_' + index).css('display', 'none');
        }
    }
    else if (type == 'leftComment') {

        //var itm = multi_non_pw_data[index];
        //for (var i = 0; i < itm.Ratings.length; i++) {
        //    var rating = itm.Ratings[i];
        //    if (index == i) {


        //        itm.Ratings[index].Comment = $('#txtleftComment_' + index).val();
        //    }

        //}
        if (operation == 'close') {

            for (var i = 0; i < multi_non_pw_data.length; i++) {
                for (var j = 0; j < multi_non_pw_data[i].Ratings.length; j++) {
                    if (j == index) {
                        multi_non_pw_data[i].Ratings[j].Comment = $('#txtleftComment_' + index).val();


                    }

                }

            }
            addActiveClass($('#txtleftComment_' + (parseInt(index) + 1)), '', (parseInt(index) + 1).toString());

        }
        $('#leftComment_' + index).toggle();
    }
    else if (type == 'ScaleDescriptions') {
        if (operation == 'close')
            $('#ScaleDescriptions').css('display', 'none');
        else
            $('#ScaleDescriptions').css('display', 'block');
    }
    else if (type == 'hdrtooltip') {
        //$('#tooltip_wrapper').toggle();
        if (operation == 'close')
            $('#tooltip_wrapper').css('display', 'none');
        else
            $('#tooltip_wrapper').css('display', 'block');
    }

    if (type == 'showcomment') {
        for (var i = 0; i < multi_non_pw_data.length; i++) {
            for (var j = 0; j < multi_non_pw_data[i].Ratings.length; j++) {
                if (j == dindex) {
                    multi_non_pw_data[i].Ratings[j].Comment = $('#txtleftComment_' + dindex).val();
                }
            }
        }
        var commentdesc = $('#txtleftComment_' + dindex).val();
        var intval = $('.intensitydesc' + dindex).html();
        /*   $('.intensitydesc' + dindex).html('');*/
        if (intval.includes('-'))
            $('.intensitydesc' + dindex).html(intval.substring(0, '-') + '-' + commentdesc);
        else
            $('.intensitydesc' + dindex).html(intval + ' - ' + commentdesc);
        $('#parentnode6' + dindex).css('display', 'none');
        $('.intensitydesc' + dindex).attr('title', commentdesc);
    }
    else if (type == 'Ratingshowcomment') {
        var commentdesc = $('#txtleftComment_' + dindex).val();
        output.intensities[dindex][4] = commentdesc;
        var intval = $('.intensitydesc' + dindex).html();
        $('.intensitydesc' + dindex).html('');
        if (intval.includes('-'))
            $('.intensitydesc' + dindex).html(intval.substring(0, '-') + '-' + commentdesc);
        else
            $('.intensitydesc' + dindex).html(intval + ' - ' + commentdesc);
        $('#parentnode6' + dindex).css('display', 'none');
        $('.intensitydesc' + dindex).attr('title', commentdesc);
        save_ratings_on_next();
    }
}

var updateRightSideComment = function (index, type, operation, event) {
    //event.stopPropagation();
    var dindex = index;
    index = index == undefined || null ? 0 : index;
    type = type == undefined || null ? 0 : type;
    operation = operation == undefined || null ? 0 : operation;

    $('.hideshowinfo').css('display', 'none');
    if (type == 'comment') {
        if (operation == 'close') {
            /* $('#txtRightComment_' + index).val('');*/
            multi_non_pw_data[index].Comment = $('#txtRightComment_' + index).val();
            //$('#comment_icon_' + index).removeClass('far fa-comments');
            //$('#comment_icon_' + index).removeClass('fa fa-comments');
            //$('#comment_icon_' + index).removeClass('filledComment');
            //$('#comment_icon_' + index).removeClass('emptyComment');
            //if ($('#txtRightComment_' + index).val() != '') {
            //    $('#comment_icon_' + index).addClass('fa fa-comments');
            //    $('#comment_icon_' + index).addClass('filledComment');
            //}
            //else {
            //    $('#comment_icon_' + index).addClass('far fa-comments');
            //    $('#comment_icon_' + index).removeClass('emptyComment');
            //}         

            $('#comment_div_box_' + index).css('display', 'none');
            HideShowComment_icon(index);
        }
        if (operation == 'open') {
            $('#txtRightComment_' + index).val(multi_non_pw_data[index].Comment);
            $('#comment_div_box_' + index).css('display', 'block');
        }

    }
    else if (type == 'Ratingcomment') {
        if (operation == 'close') {
            output.comment = $('#txtRightComment_' + index).val();
            //$('#comment_icon_' + index).removeClass('far fa-comments');
            //$('#comment_icon_' + index).removeClass('fa fa-comments');
            //$('#comment_icon_' + index).removeClass('filledComment');
            //$('#comment_icon_' + index).removeClass('emptyComment');

            //if ($('#txtRightComment_' + index).val() != '') {
            //    $('#comment_icon_' + index).addClass('fa fa-comments');
            //    $('#comment_icon_' + index).addClass('filledComment');
            //}
            //else {
            //    $('#comment_icon_' + index).addClass('far fa-comments');
            //    $('#comment_icon_' + index).removeClass('emptyComment');           
            //}
            $('#comment_div_box_' + index).css('display', 'none');
            HideShowComment_icon(index);
            save_ratings_on_next();
        }
        if (operation == 'open') {
            $('#txtRightComment_' + index).val(output.comment);
            $('#comment_div_box_' + index).css('display', 'block');
        }
    }
    else if (type == 'DirectComment') {
        if (operation == 'close') {
            output.comment = $('#txtRightComment_' + index).val();
            //$('#comment_icon_' + index).removeClass('far fa-comments');
            //$('#comment_icon_' + index).removeClass('fa fa-comments');
            //$('#comment_icon_' + index).removeClass('filledComment');
            //$('#comment_icon_' + index).removeClass('emptyComment');

            //if ($('#txtRightComment_' + index).val() != '') {
            //    $('#comment_icon_' + index).addClass('fa fa-comments');
            //    $('#comment_icon_' + index).addClass('filledComment');
            //}
            //else {
            //    $('#comment_icon_' + index).addClass('far fa-comments');
            //    $('#comment_icon_' + index).removeClass('emptyComment');
            //}parentnode16
            // $('#comment_div_box_' + index).css('display', 'none');
            $('#parentnode' + index).css('display', 'none');
            save_direct_on_next();
        }
        if (operation == 'open') {
            $('#comment_div_box_' + index).css('display', 'block');
        }
    }
    else if (type == 'tooltip') {
        $('#tooltip_wrapper_' + index).toggle();
        if (operation == 'open') {
            $('#tooltip_wrapper_' + index).css('display', 'block');
        }
        else if (operation == 'close') {
            $('#tooltip_wrapper_' + index).css('display', 'none');
        }
        else {
            $('#tooltip_wrapper_' + index).css('display', 'none');
        }
    }
    else if (type == 'bandage') {
        //$('#w_bandage_wrapper_' + index).toggle();
        if (operation == 'open') {
            $('#w_bandage_wrapper_' + index).css('display', 'block');
        }
        else if (operation == 'close') {
            $('#w_bandage_wrapper_' + index).css('display', 'none');
        }
        else {
            $('#w_bandage_wrapper_' + index).css('display', 'none');
        }
    }
    else if (type == 'leftComment') {

        //var itm = multi_non_pw_data[index];
        //for (var i = 0; i < itm.Ratings.length; i++) {
        //    var rating = itm.Ratings[i];
        //    if (index == i) {


        //        itm.Ratings[index].Comment = $('#txtleftComment_' + index).val();
        //    }

        //}
        if (operation == 'close') {

            for (var i = 0; i < multi_non_pw_data.length; i++) {
                for (var j = 0; j < multi_non_pw_data[i].Ratings.length; j++) {
                    if (j == index) {
                        multi_non_pw_data[i].Ratings[j].Comment = $('#txtleftComment_' + index).val();


                    }

                }

            }
            addActiveClass($('#txtleftComment_' + (parseInt(index) + 1)), '', (parseInt(index) + 1).toString());

        }
        $('#leftComment_' + index).toggle();
    }
    else if (type == 'ScaleDescriptions') {
        if (operation == 'close')
            $('#ScaleDescriptions').css('display', 'none');
        else
            $('#ScaleDescriptions').css('display', 'block');
    }
    else if (type == 'hdrtooltip') {
        //$('#tooltip_wrapper').toggle();
        if (operation == 'close')
            $('#tooltip_wrapper').css('display', 'none');
        else
            $('#tooltip_wrapper').css('display', 'block');
    }

    if (type == 'showcomment') {
        for (var i = 0; i < multi_non_pw_data.length; i++) {
            for (var j = 0; j < multi_non_pw_data[i].Ratings.length; j++) {
                if (j == dindex) {
                    multi_non_pw_data[i].Ratings[j].Comment = $('#txtleftComment_' + dindex).val();
                }
            }
        }
        var commentdesc = $('#txtleftComment_' + dindex).val();
        var intval = $('.intensitydesc' + dindex).html();
        /*   $('.intensitydesc' + dindex).html('');*/
        if (intval.includes('-'))
            $('.intensitydesc' + dindex).html(intval.substring(0, '-') + '-' + commentdesc);
        else
            $('.intensitydesc' + dindex).html(intval + ' - ' + commentdesc);
        $('#parentnode6' + dindex).css('display', 'none');
        $('.intensitydesc' + dindex).attr('title', commentdesc);
    }
    else if (type == 'Ratingshowcomment') {
        var commentdesc = $('#txtleftComment_' + dindex).val();
        output.intensities[dindex][4] = commentdesc;
        var intval = $('.intensitydesc' + dindex).html();
        $('.intensitydesc' + dindex).html('');
        if (intval.includes('-'))
            $('.intensitydesc' + dindex).html(intval.substring(0, '-') + '-' + commentdesc);
        else
            $('.intensitydesc' + dindex).html(intval + ' - ' + commentdesc);
        $('#parentnode6' + dindex).css('display', 'none');
        $('.intensitydesc' + dindex).attr('title', commentdesc);
        save_ratings_on_next();
    }
}

var setValuesFromText = function (index, inputIndex) {
    //old_multi_non_pw_data = multi_non_pw_data;  
    var text = '', id = '';
    var val = 0;
    var txtVal = $('#input_' + inputIndex + '_' + index).val();
    if (parseFloat($('#input_' + inputIndex + '_' + index).val()) > 0)
        val = parseFloat($('#input_' + inputIndex + '_' + index).val() * 100).toFixed(4);

    if (txtVal != '' && txtVal >= 0) {
        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        $('#divRadioButtons .imgClose').css({ 'filter': 'none', 'opacity': '1' })
    }
    else {
        $('#div_row_wrapper_' + index + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
    }

    if (parseFloat(val / 100) < 0 || parseFloat(val / 100) > 1) {
        alert('please enter value from 0 to 1');
        $('#input_' + inputIndex + '_' + index).val('');
        txtVal = '';
    }
    if (multi_non_pw_data[active_multi_index].Ratings != null && multi_non_pw_data[active_multi_index].Ratings.length > 0) {
        for (var i = 0; i < multi_non_pw_data[new_active_multi_index].Ratings.length; i++) {
            var rating = multi_non_pw_data[new_active_multi_index].Ratings[i];
            if (rating.Name.toLowerCase() == 'direct value') {
                text = rating.Name;
                id = rating.GuidID;
                if (parseFloat(val) > 0)
                    rating.Value = parseFloat(val / 100);

                multi_non_pw_data[new_active_multi_index].RatingID = rating.ID;
                multi_non_pw_data[new_active_multi_index].DirectData = parseFloat(val / 100);
                multi_non_pw_data[new_active_multi_index].Ratings[i] = rating;
            }
            else if (rating.Name.toLowerCase() == 'Not Rated') {
                if (parseFloat(val) <= 0) {
                    text = rating.Name;
                    id = rating.GuidID;
                    rating.Value = 0;

                    multi_non_pw_data[new_active_multi_index].RatingID = rating.ID;
                    multi_non_pw_data[new_active_multi_index].Ratings[i] = rating;
                }
            }
        }
        multi_ratings_row[index][0] = parseFloat(val / 100);
        multi_ratings_row[index][1] = text;
        var mval = parseFloat(txtVal * 100);
        if (mval.toString().length > 3) {
            if (output.showPriorityAndDirect) {
                $('#dropdownHeaderValue' + index).text(text + ' ' + mval.toFixed(2) + '%');
                $('#spndropdownHeaderValueM' + index).text(text + ' ' + mval.toFixed(2) + '%');
            }
            else {
                $('#dropdownHeaderValue' + index).text(text);
                $('#spndropdownHeaderValueM' + index).text(text);
            }

        }
        else {
            if (output.showPriorityAndDirect) {
                $('#dropdownHeaderValue' + index).text(text + ' ' + mval + '%');
                $('#spndropdownHeaderValueM' + index).text(text + ' ' + mval + '%');
            }
            else {
                $('#dropdownHeaderValue' + index).text(text);
                $('#spndropdownHeaderValueM' + index).text(text);
            }
        }

        $('#progress_bar_' + index).attr('aria-valuenow', val).css('width', val + '%');
        $('#dropdownHeaderValueM' + index).attr('aria-valuenow', val).css('width', val + '%');
        $("input[type='radio'][id='" + id + "']").prop("checked", true);
        inputIndex = inputIndex == 1 ? 2 : 1;
        if (txtVal != '') {
            $('#input_' + inputIndex + '_' + index).val(parseFloat(val / 100).toFixed(4));
        }
        else {
            $('#input_' + inputIndex + '_' + index).val('');
        }

        if (text == 'Not Rated') {
            $('#input_1_' + index).val('');
            $('#close_icon_' + index + ' i').hide();
        }
        else {
            $('#close_icon_' + index + ' i').show();
        }
    }
}

var SetValuesFromRadio = function (mainIndex, radioIndex) {
    if (!editbuttonClick) {
        if (mainIndex >= 0 && radioIndex >= 0) {
            var item = multi_non_pw_data[mainIndex].Ratings[radioIndex];
            multi_non_pw_data[mainIndex].RatingID = item.ID;
            $('#UiDropdownList' + mainIndex + 'D').hide();
            CheckeRadioButton(radioIndex);
            if (item != undefined && item != null) {
                var DropVal = parseFloat(item.Value * 100);
                if (DropVal.toString().length > 3) { DropVal = DropVal.toFixed(2) }
                var PerCentage = DropVal > 0 ? (' ' + DropVal + '%') : (' ' + DropVal + '%');
                if (output.showPriorityAndDirect) {
                    $('#dropdownHeaderValue' + mainIndex).text(item.Name + PerCentage);
                    $('#spndropdownHeaderValueM' + mainIndex).text(item.Name + PerCentage);
                }
                else {
                    $('#dropdownHeaderValue' + mainIndex).text(item.Name);
                    $('#spndropdownHeaderValueM' + mainIndex).text(item.Name);
                }

                if (item.Name == "None" || item.Name == "Direct Value" || DropVal > 0) {
                    $('#div_row_wrapper_' + mainIndex + ' .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                    $('#divRadioButtons .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                }
                else {
                    $('#div_row_wrapper_' + mainIndex + ' .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                    $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                }

                if (item.Name.toLowerCase() == 'direct value' && $('#input_2_' + mainIndex).val() != undefined && $('#input_2_' + mainIndex).val() != null
                    && $('#input_2_' + mainIndex).val().trim() != '' && parseFloat($('#input_2_' + mainIndex).val()) >= 0) {
                    item.Value = parseFloat($('#input_2_' + mainIndex).val());
                }
                $('#progress_bar_' + mainIndex).attr('aria-valuenow', parseFloat(item.Value * 100).toFixed(4)).css('width', parseFloat(item.Value * 100).toFixed(4) + '%');
                $('#dropdownHeaderValueM' + mainIndex).attr('aria-valuenow', parseFloat(item.Value * 100).toFixed(4)).css('width', parseFloat(item.Value * 100).toFixed(4) + '%');
            }
            $('#input_1_' + mainIndex).val(parseFloat(item.Value).toFixed(4));

            if (item.Name == 'Not Rated') {
                $('#input_1_' + mainIndex).val('');
                $('#close_icon_' + mainIndex + ' i').hide();
            }
            else {
                $('#close_icon_' + mainIndex + ' i').show();
            }

            var disablePage = false;
            for (var i = 0; i < multi_ratings_row.length; i++) {
                if (i == parseInt(mainIndex)) {
                    multi_ratings_row[i][0] = parseFloat(item.Value).toFixed(4);
                    multi_ratings_row[i][1] = item.Name;
                }
                if (multi_ratings_row[i][1] == 'Not Rated' || multi_ratings_row[i][1] == 'None') {
                    disablePage = true;
                }
            }

            if (output.pipeOptions.dontAllowMissingJudgment) {
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

            // toggleFunction(mainIndex, event);
            setTimeout(function () {
                var NextIcmJudIndex = GetNextMultiRatingIncompleteJudgmentInex(mainIndex);
                if (NextIcmJudIndex > -1) {
                    addActiveClass($('#div_row_wrapper_' + NextIcmJudIndex.toString()), '', NextIcmJudIndex.toString());
                }
                else {
                    var activeRow = 0;
                    for (i = 0; i < output.multi_non_pw_data.length; i++) {
                        if (i !== mainIndex && activeRow == 0) {
                            if (multi_ratings_row[i][0].toString() == '' || multi_ratings_row[i][1] == "Not Rated") {
                                addActiveClass($('#div_row_wrapper_' + i.toString()), '', i.toString());
                                activeRow++;
                            }
                        }
                    }
                    if (activeRow == 0) {
                        addActiveClass($('#div_row_wrapper_' + (parseInt(mainIndex))), '', (parseInt(mainIndex)).toString());
                    }
                }
                //if ($('#div_row_wrapper_' + (parseInt(mainIndex) + 1)).length > 0) {
                //    //$('#div_row_wrapper_' + mainIndex).removeClass('active_table_row');
                //    //$('#div_row_wrapper_' + (parseInt(mainIndex) + 1)).addClass('active_table_row');
                //    //addActiveClass($('#div_row_wrapper_' + (parseInt(mainIndex) + 1)), '', (parseInt(mainIndex) + 1).toString());

                //    //var lastElementTop = $('div.progress_table_row #div_row_wrapper_' + (parseInt(mainIndex) + 1)).position().top;
                //    //var scrollAmount = lastElementTop - 200;

                //    //$('div.progress_table_row #div_row_wrapper_' + (parseInt(mainIndex) + 1)).animate({ scrollTop: scrollAmount }, 1000);

                //    //$('html, body, div.progress_table_row').animate({
                //    //    scrollTop: $('#div_row_wrapper_' + parseInt(mainIndex)).offset().top - 250
                //    //}, 'slow');

                //    //var el = $('#div_row_wrapper_' + parseInt(mainIndex));
                //    //var elOffset = el.offset().top;
                //    //var elHeight = el.height();
                //    //var windowHeight = $(window).height();
                //    //var offset;

                //    //if (elHeight < windowHeight) {
                //    //    offset = elOffset - ((windowHeight / 2) - (elHeight / 2));
                //    //}
                //    //else {
                //    //    offset = elOffset;
                //    //}
                //    //var speed = 700;
                //    //$('html, body').animate({ scrollTop: offset });
                //}

                //listItems = $('#dropdownList' + mainIndex + ' li');
                //dropdownHeader = $('#dropdownHeader' + mainIndex);
                //chevronIcon = $('#chevronIcon' + mainIndex);
                //dropdownListBody = $('#dropdownListBody' + mainIndex);
                //dropdownHeaderValue = $('#dropdownHeaderValue' + mainIndex);
                //isDropdownDisabled = $('#dropdown' + mainIndex).hasClass("disabled");
                //dropdown = $('#dropdown' + mainIndex);

                //chevronIcon.removeClass('rotate-icon-home');
                //chevronIcon.removeClass('rotate-icon');
                //dropdownListBody.removeClass('dropdown__body--hide');
                //dropdownListBody.removeClass('dropdown__body--show');
                //dropdownHeader.removeClass('dropdown__header--hide');
                //dropdownHeader.removeClass('dropdown__header--show');

                //chevronIcon.addClass('rotate-icon-home');
                //dropdownListBody.addClass('dropdown__body--hide');
                //dropdownHeader.addClass('dropdown__header--hide');

                //if (chevronIcon.hasClass('rotate-icon-home'))
                //    chevronIcon.removeClass('rotate-icon-home');
                //if (chevronIcon.hasClass('rotate-icon'))
                //    chevronIcon.removeClass('rotate-icon');
                //if (dropdownListBody.hasClass('dropdown__body--hide'))
                //    dropdownListBody.removeClass('dropdown__body--hide');
                //if (dropdownListBody.hasClass('dropdown__body--show'))
                //    dropdownListBody.removeClass('dropdown__body--show');
                //if (dropdownHeader.hasClass('dropdown__header--hide'))
                //    dropdownHeader.removeClass('dropdown__header--hide');
                //if (dropdownHeader.hasClass('dropdown__header--show'))
                //    dropdownHeader.removeClass('dropdown__header--show');

                //if (!chevronIcon.hasClass('rotate-icon-home'))
                //    chevronIcon.addClass('rotate-icon-home');
                //if (!dropdownListBody.hasClass('dropdown__body--hide'))
                //    dropdownListBody.addClass('dropdown__body--hide');
                //if (!dropdownHeader.hasClass('dropdown__header--hide'))
                //    dropdownHeader.addClass('dropdown__header--hide');

                //else {
                //    $('#div_row_wrapper_0').addClass('active_table_row');
                //    addActiveClass($('#div_row_wrapper_0'), '', '0');
                //}            
            }, 500);
        }
    }
    editbuttonClick = false;
}

var SetValuesFromSingleRadio = function (radioid, radioIndex) {
    if (radioIndex >= 0) {

        var item = output.intensities[radioIndex];
        $("input[type='radio']").prop("checked", false);
        $("input[type='radio'][id='" + radioid + "']").prop("checked", true);
        //multi_non_pw_data[mainIndex].RatingID = item.ID;
        if (item != undefined && item != null) {
            $('#dropdownHeaderValue' + radioIndex).text(item[1]);
            if (item[1].toLowerCase() == 'direct value' && $('#input_2_' + radioIndex).val() != undefined && $('#input_2_' + radioIndex).val() != null
                && $('#input_2_' + radioIndex).val().trim() != '' && parseFloat($('#input_2_' + radioIndex).val()) >= 0) {
                item.Value = parseFloat($('#input_2_' + radioIndex).val());
            }
            $('#progress_bar_' + radioIndex).attr('aria-valuenow', parseFloat(item[0] * 100).toFixed(4)).css('width', parseFloat(item[0] * 100).toFixed(4) + '%');
            $('#dropdownHeaderValueM' + radioIndex).attr('aria-valuenow', parseFloat(item[0] * 100).toFixed(4)).css('width', parseFloat(item[0] * 100).toFixed(4) + '%');
        }
        $('#input_1_' + mainIndex).val(parseFloat(item[0]).toFixed(4));

        if (item.Name == 'Not Rated') {
            $('#input_1_' + mainIndex).val('');
            $('#close_icon_' + mainIndex + ' i').hide();
        }
        else {
            $('#close_icon_' + mainIndex + ' i').show();
        }

        for (var i = 0; i < multi_ratings_row.length; i++) {
            if (i == parseInt(mainIndex)) {
                multi_ratings_row[i][0] = parseFloat(item.Value).toFixed(4);
                multi_ratings_row[i][1] = item.Name;
            }
        }

        // toggleFunction(mainIndex, event);
        setTimeout(function () {
            if ($('#div_row_wrapper_' + (parseInt(mainIndex) + 1)).length > 0) {
                $('#div_row_wrapper_' + mainIndex).removeClass('active_table_row');
                $('#div_row_wrapper_' + (parseInt(mainIndex) + 1)).addClass('active_table_row');
                addActiveClass($('#div_row_wrapper_' + (parseInt(mainIndex) + 1)), '', (parseInt(mainIndex) + 1).toString());

                //var lastElementTop = $('div.progress_table_row #div_row_wrapper_' + (parseInt(mainIndex) + 1)).position().top;
                //var scrollAmount = lastElementTop - 200;

                //$('div.progress_table_row #div_row_wrapper_' + (parseInt(mainIndex) + 1)).animate({ scrollTop: scrollAmount }, 1000);

                //$('html, body, div.progress_table_row').animate({
                //    scrollTop: $('#div_row_wrapper_' + parseInt(mainIndex)).offset().top - 250
                //}, 'slow');

                var el = $('#div_row_wrapper_' + parseInt(mainIndex));
                var elOffset = el.offset().top;
                var elHeight = el.height();
                var windowHeight = $(window).height();
                var offset;

                if (elHeight < windowHeight) {
                    offset = elOffset - ((windowHeight / 2) - (elHeight / 2));
                }
                else {
                    offset = elOffset;
                }
                var speed = 700;
                $('html, body').animate({ scrollTop: offset });
            }

            listItems = $('#dropdownList' + mainIndex + ' li');
            dropdownHeader = $('#dropdownHeader' + mainIndex);
            chevronIcon = $('#chevronIcon' + mainIndex);
            dropdownListBody = $('#dropdownListBody' + mainIndex);
            dropdownHeaderValue = $('#dropdownHeaderValue' + mainIndex);
            isDropdownDisabled = $('#dropdown' + mainIndex).hasClass("disabled");
            dropdown = $('#dropdown' + mainIndex);

            chevronIcon.removeClass('rotate-icon-home');
            chevronIcon.removeClass('rotate-icon');
            dropdownListBody.removeClass('dropdown__body--hide');
            dropdownListBody.removeClass('dropdown__body--show');
            dropdownHeader.removeClass('dropdown__header--hide');
            dropdownHeader.removeClass('dropdown__header--show');

            chevronIcon.addClass('rotate-icon-home');
            dropdownListBody.addClass('dropdown__body--hide');
            dropdownHeader.addClass('dropdown__header--hide');

            //if (chevronIcon.hasClass('rotate-icon-home'))
            //    chevronIcon.removeClass('rotate-icon-home');
            //if (chevronIcon.hasClass('rotate-icon'))
            //    chevronIcon.removeClass('rotate-icon');
            //if (dropdownListBody.hasClass('dropdown__body--hide'))
            //    dropdownListBody.removeClass('dropdown__body--hide');
            //if (dropdownListBody.hasClass('dropdown__body--show'))
            //    dropdownListBody.removeClass('dropdown__body--show');
            //if (dropdownHeader.hasClass('dropdown__header--hide'))
            //    dropdownHeader.removeClass('dropdown__header--hide');
            //if (dropdownHeader.hasClass('dropdown__header--show'))
            //    dropdownHeader.removeClass('dropdown__header--show');

            //if (!chevronIcon.hasClass('rotate-icon-home'))
            //    chevronIcon.addClass('rotate-icon-home');
            //if (!dropdownListBody.hasClass('dropdown__body--hide'))
            //    dropdownListBody.addClass('dropdown__body--hide');
            //if (!dropdownHeader.hasClass('dropdown__header--hide'))
            //    dropdownHeader.addClass('dropdown__header--hide');

            //else {
            //    $('#div_row_wrapper_0').addClass('active_table_row');
            //    addActiveClass($('#div_row_wrapper_0'), '', '0');
            //}            
        }, 500);
    }
}

var get_direct_value = function (value) {
    value = $('#' + value).val().trim() == '' ? -1 : parseFloat($('#' + value).val());
    //$scope.cancelDirectValueTimer();
    //var timeOutMs = value == 0 ? 500 : 100;
    if ((value < 0 && value > 1) || value == undefined) {
        output.non_pw_value = "";
        return false;
    }
    RedirecttoNext = false;
    save_ratings("*" + value);
}

var ResetRatingValue = function () {
    $('#InputSingleRatingDirect').val('');
    $('#direct_value_input').val('');
    $('#direct_value_input').focus();
    if (output.pipeOptions.dontAllowMissingJudgment) {
        EnableDisablePagination(true);
    }
    for (var i = 0; i < output.intensities.length; i++) {
        var rating = output.intensities[i];
        if (rating[1].toLowerCase() == 'not rated') {
            //text = rating.Name;
            //id = rating.GuidID;
            //rating.Value = 0;

            ratings_data = rating;
        }
    }
    output.non_pw_value = '';
    if (output.showPriorityAndDirect) {
        $('#dropdownHeaderValue0').text(ratings_data[1]);
        $('#spndropdownHeaderValueM0').text(ratings_data[1]);
    }
    else {
        $('#dropdownHeaderValue0').text(ratings_data[1]);
        $('#spndropdownHeaderValueM0').text(ratings_data[1]);
    }
    //$('#spndropdownHeaderValueM0').text(ratings_data[1]);
    $('#progress_bar_0').attr('aria-valuenow', '0').css('width', '0%');
    $("input[type='radio']").prop("checked", false);
    $("input[type='radio'][id='intensity_-1']").prop("checked", true);
    save_ratings(-1)
    $('#close_icon_0 i').hide();
    $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
    $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
    OldDropDownListID = DropDownListID;
    save_ratings_on_next();
}

var save_ratings_on_next = function () {

    $("#overlay").css('display', 'flex');

    var direct_value = "";
    var value = ratings_data[2];

    if (output.is_direct) {
        direct_value = ratings_data[0];
        value = -2;
    }

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveRatings",
        data: JSON.stringify({
            step: hdnCurrentStep,
            RatingID: value,
            sComment: output.comment,
            intensities: output.intensities,
            UserID: 0,
            DirectValue: direct_value
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (output.pipeOptions.dontAllowMissingJudgment) {
                setTimeout(function () {
                    var disablePage = false;
                    if (ratings_data[1].toLowerCase() == 'not rated' || ratings_data[1].toLowerCase() == 'none') {
                        disablePage = true;
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
                }, 50);
            }

            if ((output.page_type == 'atPairwise' && output.pairwise_type == 'ptVerbal') || (output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtRatings')) {
                if (output.is_auto_advance && RedirecttoNext) {
                    $('#hdnPageNumber').val(parseInt(hdnCurrentStep + 1));
                    setTimeout(function () {
                        $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                }
            }
        },
        error: function (response) {
            //alert(JSON.stringify(response));
            //window.location.href = baseUrl;
        }
    });
    setTimeout(function () {
        $("#overlay").css('display', 'none');
    }, 250);
    
}

var get_ratings_data = function () {
    var refreshed = true;
    if (output.is_direct) {
        ratings_data = [output.non_pw_value, "Direct Value", "-2", "0"];
    }
    else {
        $.each(output.intensities, function (index, intensity) {
            if (intensity[0] == output.non_pw_value && intensity[2] !="-2") {
                ratings_data = intensity;
            }
        });
    }
    if (ratings_data != undefined && ratings_data != null && ratings_data.length > 0) {
        $("input[type='radio']").prop("checked", false);
        $("input[type='radio'][id='intensity_" + ratings_data[2] + "']").prop("checked", true);
        if (ratings_data != undefined && ratings_data != null) {
            var TxtNum = parseFloat(ratings_data[0] * 100);
            TxtNum = Math.round(TxtNum * 100) / 100;
            if (TxtNum >= 0) {
                if (output.showPriorityAndDirect) {
                    $('#dropdownHeaderValue0').text(ratings_data[1] + " " + TxtNum + "%");
                    $('#spndropdownHeaderValueM0').text(ratings_data[1] + " " + TxtNum + "%");
                }
                else {
                    $('#dropdownHeaderValue0').text(ratings_data[1]);
                    $('#spndropdownHeaderValueM0').text(ratings_data[1]);
                }

                if (ratings_data[0] != "Not Rated" && TxtNum >= 0) {
                    $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                }
                else {
                    $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                }

                $('#progress_bar_0').attr('aria-valuenow', parseFloat(TxtNum).toFixed(2)).css('width', parseFloat(TxtNum).toFixed(2) + '%');
                $('#dropdownHeaderValueM0').attr('aria-valuenow', parseFloat(TxtNum).toFixed(2)).css('width', parseInt(TxtNum) + '%');
                //$('#spndropdownHeaderValueM0').text(ratings_data[1] + " " + TxtNum + "%");
                //$('#spndropdownHeaderValueM0').text(ratings_data[1]);

            }
            else {
                $('#dropdownHeaderValue0').text(ratings_data[1]);
                $('#spndropdownHeaderValueM0').text(ratings_data[1]);
            }
            if (ratings_data[1].toLowerCase() == 'direct value') {
                $('#InputSingleRatingDirect').val(parseFloat(ratings_data[0]).toFixed(4));
                $('#direct_value_input').val(parseFloat(ratings_data[0]).toFixed(4));
            }
            else if (ratings_data[1].toLowerCase() == 'not rated') {
                $('#InputSingleRatingDirect').val('');
                $('#direct_value_input').val('');
                $('#close_icon_0 i').hide();
            }
            else {
                $('#direct_value_input').val(parseFloat(ratings_data[0]).toFixed(4));
            }
            if (ratings_data[1].toLowerCase() !== 'not rated')
                $('#dropdownHeaderValue0').attr('aria-valuenow', parseFloat(ratings_data[0] * 100).toFixed(4)).css('width', '100%');
            else
                $('#dropdownHeaderValue0').css('width', '100%');



            if (parseFloat(ratings_data[0]) * 100 >= 0) {
                $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'none', 'opacity': '1' })
                $('#divRadioButtons .imgClose').css({ 'filter': 'none', 'opacity': '1' })
            }
            else {
                $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
                $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
            }
        }

        //if (output.pipeOptions.dontAllowMissingJudgment) {
        //    setTimeout(function () {
        //        var disablePage = false;
        //        if (ratings_data[1].toLowerCase() == 'not rated' || ratings_data[1].toLowerCase() == 'none') {
        //            disablePage = true;
        //        }
        //        if (disablePage) {
        //            EnableDisablePagination(true);
        //        }
        //        else {
        //            if (enabledLastStep == hdnCurrentStep)
        //                EnableDisablePagination(false, true);
        //            else
        //                EnableDisablePagination(true, false);
        //        }
        //    }, 500);
        //}
    }
}

//Restrict the user to enter only decimal value with one decimal point
function isNumberKeyWithDecimal(txt, evt) {

    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode == 46) {
        //Check if the text already contains the . character
        if (txt.value.indexOf('.') === -1) {
            return true;
        }
        else {
            return false;
        }
    }
    else {
        if (charCode > 31 &&
            (charCode < 48 || charCode > 57))
            return false;
    }
    return true;
}

//multi direct
var set_multi_index = function (index) {
    $(".multi-rows").removeClass("selected");

    var callApply = parseInt(active_multi_index) != parseInt(index);
    active_multi_index = parseInt(index);

    $(".multi-row-" + active_multi_index).addClass("selected");
    $(".multi-row-" + index + " .not-disabled").addClass("change-color-pulse");

    try {
        if (hdnpage_type == "atAllPairwise" || hdnpage_type == "atAllPairwiseOutcomes") {
            setTimeout(function () {
                mobile_manual_equalizer();
            }, 2, callApply);
        }
    }
    catch (e) {
    }

    setTimeout(function () {
        $(".not-disabled").removeClass("change-color-pulse");
        $(".disabled").removeClass("change-color-pulse");

        //$scope.scroll_to_active_index();
    }, 200, false);

    //$scope.callResizeFrameImagesProportionally(200, "set_multi_index " + index);
}

var get_multi_direct_data = function (multi_index) {
    setTimeout(function () {
        for (var i = 0; i < multi_non_pw_data.length; i++) {
            var value = multi_non_pw_data[i].DirectData;
            update_multi_direct_slider(i, value, multi_index);
        }
        ActiveDirect(multi_index);
    }, 10);
}

var update_multi_direct_slider = function (index, value, multi_index, from_input) {
    var temp_values = at_multi_direct_slider;
    if (parseFloat(value) || parseFloat(value) == 0) {

        if (value > 1) {
            setTimeout(function () {
                at_multi_direct_input[index] = temp_values[index];
                at_multi_direct_slider[index] = temp_values[index];
            }, 1000);
            return false;
        }
        $('div.row_wrapper').removeClass('active_table_row');
        $('#multi_direct_slider_' + index).addClass('active_table_row');
        at_multi_direct_input[index] = value < 0 ? "" : value; //display the direct data from output 
        at_multi_direct_slider[index] = value < 0 ? -1 : value;

        //if (!angular.isUndefined(from_input)) { // if input box is edited move to next row
        //    $("#at_direct_input" + index).focus().val("").val($scope.at_multi_direct_input[index]); //11508

        //    checkMultiValue($scope.at_multi_direct_slider, 2);
        //}

        setTimeout(function () {
            $("#at_multi_direct_input" + index).val(value < 0 ? "" : value);
            $("#at_multi_direct_slider" + index).slider("value", value);
        }, 10);
    }
    else {
        if (value == undefined || value == null || value == "") {
            IsUndefined = true;

            setTimeout(function () {
                at_multi_direct_input[index] = "";
                at_multi_direct_slider[index] = -1;
                $("#at_multi_direct_input" + index).val("");
                $("#at_multi_direct_slider" + index).slider(0);
            }, 10);
        }
    }

    if (output.pipeOptions.dontAllowMissingJudgment) {
        var disablePage = false;
        if (at_multi_direct_input.length > 0) {
            for (i = 0; i < at_multi_direct_input.length; i++) {
                if (at_multi_direct_input[i] == "") {
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
}

var save_multi_direct_on_next = function () {
    //do ajax for update judgement backend
    var multivalues = [];
    // //console.log($scope.output.multi_non_pw_data);
    for (i = 0; i < at_multi_direct_input.length; i++) {
        try {
            var vals = [];
            vals[0] = at_multi_direct_slider[i] == -1 ? "" : at_multi_direct_slider[i];
            vals[1] = multi_non_pw_data[i].Comment;
            multivalues[i] = vals;
        }
        catch (e) {

        }
    }
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveMultiDirectData",
        async: true,
        data: JSON.stringify({
            step: hdnCurrentStep,
            multivalues: multivalues
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            //var output = JSON.parse(data.d);
            //$('#hdnPageNumber').val(output);
            //$('#MainContent_hdnPageNo').trigger('click');
        },
        error: function (response) {
        }
    });
}

var SetMultiDirectSlider = function (index) {
    //$('div.row_wrapper').removeClass('active_table_row');
    //$('#multi_direct_slider_' + index).addClass('active_table_row');
    if (!$('#multi_direct_slider_' + index).hasClass('active_table_row')) {
        $('div.row_wrapper').removeClass('active_table_row');
        $('#multi_direct_slider_' + index).addClass('active_table_row');
    }

    var val = $('#at_direct_input' + index).val();
    val = val == '' ? '0' : val;
    update_multi_direct_slider(index, parseFloat(val), null, true);

    setTimeout(function () {
        if (at_multi_direct_slider[index] >= 0) {
            $('#at_direct_slider' + index).slider("value", (parseFloat(at_multi_direct_slider[index]) * 100));
            $('#close_icon_' + index + ' i').show();
        }
        else {
            //$('#at_direct_slider' + index).slider("value", 0);
            $('#close_icon_' + index + ' i').hide();
        }


        if ($('#at_direct_input' + index).val() != '' && at_multi_direct_slider[index] >= 0)
            $('#close_icon_' + index).css({ 'filter': 'none', 'opacity': '1' })
        else
            $('#close_icon_' + index).css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
    }, 20);
}

var ResetMultiDirectSlider = function (index) {
    if (!$('#multi_direct_slider_' + index).hasClass('active_table_row')) {
        $('div.row_wrapper').removeClass('active_table_row');
        $('#multi_direct_slider_' + index).addClass('active_table_row');
    }

    $('#at_direct_slider' + index).slider("value", 0);
    $('#at_direct_input' + index).val('');
    $('#close_icon_' + index + ' i').hide();
    at_multi_direct_input[index] = '';
    at_multi_direct_slider[index] = '';

    if (output.pipeOptions.dontAllowMissingJudgment) {
        EnableDisablePagination(true);
    }
}

//DirectComparion.ascx
var at_direct_input = "", at_direct_slider = 0;
var update_single_direct_slider = function (value) {
    var temp_values = at_single_direct_slider;
    if (parseFloat(value) || parseFloat(value) == 0) {

        if (value > 1) {
            setTimeout(function () {
                at_single_direct_input = temp_values;
                at_single_direct_slider = temp_values;
            }, 100);
            return false;
        }
        at_single_direct_input = value < 0 ? "" : value; //display the direct data from output
        at_single_direct_slider = value < 0 ? -1 : value;
        output.non_pw_value = at_single_direct_input;
        /*save_direct_on_next();*/

        //setTimeout(function () {
        //    $("#at_multi_direct_input").val(value < 0 ? "" : value);
        //    $("#at_multi_direct_slider").slider("value", value);
        //}, 10);
        $('.progress_table_row .imgClose').css({ 'filter': 'none', 'opacity': '1' });
    }
    else {
        if (value == undefined || value == null || value == "") {
            IsUndefined = true;

            setTimeout(function () {
                at_single_direct_input = 0;
                at_single_direct_slider = -1;
                output.non_pw_value = -1;
                /*save_direct_on_next();*/
                //$("#at_multi_direct_input" + index).val("");
                //$("#at_multi_direct_slider" + index).slider(0);
                $('.progress_table_row .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            }, 10);
        }
    }
}

var SetsingleDirectSlider = function () {
    var val = $('#at_direct_input').val();
    val = val == '' ? '0' : val;
    val = parseFloat(val) >= 1 ? '-1' : val;
    update_single_direct_slider(parseFloat(val));

    setTimeout(function () {
        if ($('#at_direct_input').val() != '' && at_single_direct_slider >= 0) {
            $('#at_direct_slider').slider("value", (parseFloat(at_single_direct_slider) * 100));
            $('#close_icon i').removeClass('d-none');
            save_direct_on_next();
            $('.progress_table_row .imgClose').css({ 'filter': 'none', 'opacity': '1' });
        }
        else {
            $('#at_direct_slider').slider("value", 0);
            $('#close_icon i').removeClass('d-none');
            $('#close_icon i').addClass('d-none');
            save_direct_on_next();
            $('.progress_table_row .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
        }
    }, 20);
}

var ResetSingleDirectSlider = function () {
    $('#at_direct_slider').slider("value", 0);
    $('#at_direct_input').val('');
    $('#close_icon i').removeClass('d-none');
    $('#close_icon i').addClass('d-none');
    at_single_direct_input = '';
    at_single_direct_slider = '';
    output.non_pw_value = -1;
    save_direct_on_next();
    $('.progress_table_row .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
}

var get_direct_data = function () {
    var value = output.non_pw_value;
    if (output.non_pw_value != undefined && output.non_pw_value != null && output.non_pw_value != '') {
        setTimeout(function () {
            if (output.non_pw_value < -1) {
                value = 0;
            }
            else if (output.non_pw_value > 0) {
                value = (value * 100);
            }
            $("#at_direct_input").val(parseFloat(output.non_pw_value));
            $("#at_direct_slider").slider({
                value: value,
                range: "min",
            });
            at_direct_input = parseFloat(output.non_pw_value);
            at_direct_slider = value;
            $('#close_icon i').removeClass('d-none');
            if ($("#at_direct_input").val() == '' || value < 0) {
                $('#close_icon i').addClass('d-none');
                $('.progress_table_row .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
            }
            else {
                $('#close_icon i').removeClass('d-none');
                $('.progress_table_row .imgClose').css({ 'filter': 'none', 'opacity': '1' });
            }
        }, 20);
    }
    else {
        setTimeout(function () {
            $("#at_direct_input").val('');
            $("#at_direct_slider").slider({
                value: 0,
                range: "min",
            });
            at_direct_input = 0;
            at_direct_slider = 0;
            $('#close_icon i').removeClass('d-none');
            $('#close_icon i').addClass('d-none');
            $('.progress_table_row .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' });
        }, 20);
    }

    //if (output.pipeOptions.dontAllowMissingJudgment) {
    //    setTimeout(function () {
    //        if (output.non_pw_value != undefined && output.non_pw_value != null && output.non_pw_value != '') {
    //            if (enabledLastStep == hdnCurrentStep)
    //                EnableDisablePagination(false, true);
    //            else
    //                EnableDisablePagination(true, false);
    //        }
    //        else {
    //            EnableDisablePagination(true);
    //        }
    //    }, 500);
    //}
}

var save_direct_on_next = function () {
    var value = output.non_pw_value;
    //consoleLog("SaveDirect. current_step: " + $scope.current_step);

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveDirect",
        data: JSON.stringify({
            step: hdnCurrentStep,
            value: value,
            sComment: output.comment
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (output.pipeOptions.dontAllowMissingJudgment) {
                if (output.non_pw_value == undefined || output.non_pw_value == null || parseFloat(output.non_pw_value) < 0) {
                    EnableDisablePagination(true);
                }
                else {
                    if (enabledLastStep == hdnCurrentStep)
                        EnableDisablePagination(false, true);
                    else
                        EnableDisablePagination(true, false);
                }
            }
        },
        error: function (response) {
            alert(JSON.stringify(response.d));
            //window.location.href = baseUrl;
        }
    });
}

$(window).scroll(function () {
    var sticky = $('#layout-wrapper'),
        scroll = $(window).scrollTop();

    var stickyDiv = $('#divRadioButtons3'),
        divscroll = $(window).scrollTop();

    if (scroll >= 30) {
        sticky.addClass('mob-scroll-fix');
        if (output.isPipeViewOnly) {
            sticky.addClass('mob-scroll-fix-readonly');
        }
    }
    else {
        sticky.removeClass('mob-scroll-fix');
        if (output.isPipeViewOnly) {
            sticky.removeClass('mob-scroll-fix-readonly');
        }
    }

    if (divscroll >= 50) stickyDiv.addClass('divScrollCatch');
    else stickyDiv.removeClass('divScrollCatch');
});

var set_infodoc_mode = function () {

    //show_framed_info_docs(output.is_infodoc_tooltip ? false : output.framed_info_docs);
    var chk = $('#chkTooltip').is(':checked');
    var valueinput = chk ? 1 : 0;
    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setInfodocMode",
        data: JSON.stringify({
            value: valueinput
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $("#divtooltp").removeClass('d-none');
            $("#divtooltp").addClass('d-none');
            $("#mdivtooltp").removeClass('d-none');
            $("#mdivtooltp").addClass('d-none');

            if (chk) {
                $('.hide_infodoc_tooltip').hide();
                $('.hide_infodoc_tooltip_div').hide();
                $('.show_infodoc_tooltip').show();
                output.is_infodoc_tooltip = true;
            }
            else {
                $('.hide_infodoc_tooltip').show();
                $('.show_infodoc_tooltip').hide();
                output.is_infodoc_tooltip = false;
            }
            is_info_tooltip = output.is_infodoc_tooltip;

            //FrameModeHtml(active_multi_index);
        },
        error: function (response) {
            alert('error');
        }
    });
}
var set_auto_advance = function (bool) {
    if (bool != undefined && bool != null)
        $('#chkautoadvance').prop('checked', bool);

    if ($('#chkautoadvance').is(':checked')) {
        bool = true;
    }
    else {
        bool = false;
    }
    //if (bool) {
    //    $('#chkautoadvance').prop('checked', true);
    //    $('#mchkautoadvance').prop('checked', true);
    //}
    var chk = bool;
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    if (isMobile)
        chk = $('#chkautoadvance').is(':checked');
    else
        chk = $('#chkautoadvance').is(':checked');
    //var valueinput = chk ? 1 : 0;

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/setAutoAdvance",
        data: JSON.stringify({
            value: chk
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            //$("#divtooltp").removeClass('d-none');
            //$("#divtooltp").addClass('d-none');
            //$("#mdivtooltp").removeClass('d-none');
            //$("#mdivtooltp").addClass('d-none');
            output.is_auto_advance = chk;
        },
        error: function (response) {
            alert('error');
        }
    });
}

var showtooltip = function (showicon) {
    /*Auto Advance*/
    if ((output.page_type == 'atPairwise' && output.pairwise_type == 'ptVerbal') || (output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtRatings')) {
        if (output.is_auto_advance) {
            $('#chkautoadvance').prop('checked', true);
            $('#mchkautoadvance').prop('checked', true);
        }
        else {
            $('#chkautoadvance').prop('checked', false);
            $('#mchkautoadvance').prop('checked', false);
        }
        $('#autoadvance').removeClass('d-none');
        $('#mautoadvance').removeClass('d-none');
    }
    else {
        $('#autoadvance').removeClass('d-none');
        $('#mautoadvance').removeClass('d-none');
        // $('#autoadvance').addClass('d-none');
    }
    /*Tooltip Mode*/
    if (output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults' && output.page_type != 'atInformationPage'
        && output.page_type != 'atSensitivityAnalysis' && output.showinfodocnode) {
        if (output.is_infodoc_tooltip) {
            $('#chkTooltip').prop('checked', true);
        }
        else {
            $('#chkTooltip').prop('checked', false);
        }
        $('#tooltipmode').removeClass('d-none');
    }
    else {
        $('#tooltipmode').removeClass('d-none');
        $('#tooltipmode').addClass('d-none');
    }

    if ($("#divtooltp").hasClass('d-none') || $("#mdivtooltp").hasClass('d-none')) {
        $("#divtooltp").removeClass('d-none');
        $("#mdivtooltp").removeClass('d-none');
    }
    else {
        $("#divtooltp").addClass('d-none');
        $("#mdivtooltp").addClass('d-none');
    }
}

var showOptions = function () {
    var count = 0;
    /*Auto Advance*/
    if ((output.page_type == 'atPairwise' && output.pairwise_type == 'ptVerbal')
        || (output.page_type == 'atNonPWOneAtATime' && output.non_pw_type == 'mtRatings')) {
        count = 1;
    }
    /*Tooltip Mode*/
    //if (output.page_type != 'atShowLocalResults' && output.page_type != 'atShowGlobalResults' && output.page_type != 'atInformationPage'
    //    && output.page_type != 'atSensitivityAnalysis' && output.showinfodocnode) {
    //    count = 1;
    //}

    if (count == 1) {
        return true;
    }
    else {
        return false;
    }
}

var show_framed_info_docs = function (shouldShow) {
    alert('show');
    if (typeof shouldShow == "undefined") {
        output.framed_info_docs = !output.framed_info_docs;
    } else {
        $(".top_content_wrapper").addClass("hide");
        output.framed_info_docs = shouldShow;

        //$timeout(function () {
        //    scroll_to_active_index();
        //}, 50);
    }

    framed_info_docs = output.framed_info_docs;
    callResizeFrameImagesProportionally(50, "show_framed_info_docs");
    if (output.framed_info_docs) {
        callResizeFrameImagesProportionally(50, "show_framed_info_docs");
    }

    //if (is_AT_owner) {
    //    $http({
    //        method: "POST",
    //        url: baseUrl + "pages/Anytime/Anytime.aspx/SetShowFramedInfodocsMobile",
    //        data: { value: output.framed_info_docs }
    //    }).then(function success(response) {

    //    }, function error(response) {
    //        console.log(response);
    //    });
    //}
}

var isGoodToCallResizeFrameImages = true;
var callResizeFrameImagesCounter = 0;

var callResizeFrameImagesProportionally = function (timeOut, message, isRepeat) {
    alert('resize');
    if (isGoodToCallResizeFrameImages) {
        if (isRepeat != undefined && callResizeFrameImagesCounter > 0) {
            callResizeFrameImagesCounter = 0;
        }

        resizeFrameImagesProportionally();
        //$timeout(function () {
        //    resizeFrameImagesProportionally();
        //}, timeOut, false);
    }
}

var resizeFrameImagesProportionally = function () {
    alert('prop');
    if (infodocSizes != null && output.pairwise_type == "ptVerbal" && (output.page_type == "atAllPairwise" || output.page_type == "atPairwise")) {
        var multiplier = 1;
        var loopCounter = 0;
        var container;
        var isWidthSmaller = false;

        if (is_multi) {
            container = $(".other-info-doc-height");
            container = (container.length == 0 || container.hasClass("hide")) ? ".InfoDocsParentDiv" : ".other-info-doc-height";
        } else {
            container = $(".pairwise-desktop");
            container = container.length == 0 ? ".tg-legend .single-pw-verbal" : ".pairwise-desktop";
        }

        if (output.autoFitInfoDocImages) {
            var maxImageSize = 0;
            var frameWidth = 0, frameHeight = 0, maxImageWidth = 0, maxImageHeight = 0;
            //console.log("maxImageSize: " + maxImageSize);

            var currentFrame;
            for (loopCounter = 0; loopCounter < 5; loopCounter++) {
                $(container + " .box-" + loopCounter + " img").each(function (index) {
                    ////console.log("loop counter: " + loopCounter + ", index: " + index + ", this.naturalWidth: " + this.naturalWidth + ", this.naturalHeight: " + this.naturalHeight);
                    maxImageWidth = this.naturalWidth > maxImageWidth ? this.naturalWidth : maxImageWidth;
                    maxImageHeight = this.naturalHeight > maxImageHeight ? this.naturalHeight : maxImageHeight;

                    currentFrame = $(container + " .box-" + loopCounter);
                    if (currentFrame.length > 0 && currentFrame.width() > 100 && frameHeight == 0 && maxImageHeight > 0) {
                        frameWidth = currentFrame.width();
                        frameHeight = currentFrame.height() - 1;
                    }

                    ////console.log("frameWidth: " + currentFrame.width() + ", frameHeight: " + currentFrame.height());
                });
            }

            //Setting maxImageWidth and maxImageWidth of all the info doc images.
            if (angular.isDefined(output.maxImageWidth) && angular.isDefined(output.maxImageHeight)) {
                maxImageWidth = output.maxImageWidth > maxImageWidth ? output.maxImageWidth : maxImageWidth;
                maxImageHeight = output.maxImageHeight > maxImageHeight ? output.maxImageHeight : maxImageHeight;
            }

            var widthMultiplier = (frameWidth > 0 && maxImageWidth > 0 && maxImageWidth > frameWidth) ? (frameWidth / maxImageWidth) : 1;
            var heightMultiplier = (frameHeight > 0 && maxImageHeight > 0 && maxImageHeight > frameHeight) ? (frameHeight / maxImageHeight) : 1;

            isWidthSmaller = widthMultiplier < heightMultiplier;

            if (isWidthSmaller) {
                maxImageSize = maxImageSize > maxImageWidth ? maxImageSize : maxImageWidth;
                multiplier = widthMultiplier;
            } else {
                maxImageSize = maxImageSize > maxImageHeight ? maxImageSize : maxImageHeight;;
                multiplier = heightMultiplier;
            }

            ////console.log("new maxImageSize: " + maxImageSize + ", multiplier: " + multiplier + ", widthMultiplier: " + widthMultiplier + ", heightMultiplier: " + heightMultiplier);
        }

        var callRepeater = false;

        for (loopCounter = 0; loopCounter < 5; loopCounter++) {
            $(container + " .box-" + loopCounter + " img").each(function (index) {
                ////console.log("2nd loop counter: " + loopCounter + ", image index: " + index + ", this.naturalHeight: " + this.naturalHeight + ", this.naturalWidth: " + this.naturalWidth);

                var newImageSize = "auto";
                callRepeater = (this.naturalWidth == 0 || this.naturalHeight == 0);

                if (multiplier == 1) {
                    newImageSize = ((this.naturalWidth * multiplier).toFixed(2) + "px");
                    $(this).css("max-width", newImageSize);
                    $(this).css("height", "auto");
                } else {
                    if (isWidthSmaller) {
                        newImageSize = ((this.naturalWidth * multiplier).toFixed(2) + "px");
                        $(this).css("width", newImageSize);
                        $(this).css("height", "auto");
                    } else {
                        newImageSize = ((this.naturalHeight * multiplier).toFixed(2) + "px");
                        $(this).css("width", "auto");
                        $(this).css("height", newImageSize);
                    }
                }

                ////console.log("newImageSize: " + newImageSize + ", multiplier: " + multiplier);
            });
        }

        if (callRepeater) {
            output.callResizeFrameImagesRepeater();
        }
    }
}


function move_to_cluster() {

    save_multis();
}

function save_multis() {

    gptoVerbal = false;
    if (output != null) {
        var comment_str = "";
        if (output.page_type == "atAllPairwise" || output.page_type == "atAllPairwiseOutcomes") {
            //$parent.runProgressBar(1, 0, 50, stringResources.yellowLoadingIcon.save);
            //save_multi_pairwise_on_next();
            if (hdnpairwise_type == 'ptGraphical') {
                save_multi_pairwise_on_next();
            }
            if ($.cookie('currentstep') != undefined) {
                var Cookie_currentstep = $.cookie('currentstep');
                if (Cookie_currentstep != undefined && Cookie_currentstep != null && Cookie_currentstep.trim() != ''
                    && $.isNumeric(Cookie_currentstep) && parseInt(Cookie_currentstep) > 0) {
                    $.cookie('CallJudgementTable', "True");
                    var callResultTable = $.cookie('callResultTable');
                    if (callResultTable != undefined && callResultTable != null && callResultTable == 'true') {
                        $.removeCookie('CallJudgementTable');
                        $.removeCookie('callResultTable');
                    }

                    /*$.removeCookie('SelectionID');*/
                    /*$.cookie('judgementStep');*/
                    $('#hdnPageNumber').val(Cookie_currentstep);
                    setTimeout(function () {
                        $('#MainContent_hdnPageNo').trigger('click');
                    }, 10);
                }
            }
        }
        if ((output.page_type == "atNonPWAllChildren" || output.page_type == "atNonPWAllCovObjs") && output.non_pw_type == "mtRatings") {
            //$parent.runProgressBar(1, 0, 50, stringResources.yellowLoadingIcon.save);
            save_multi_ratings_on_next();
        }
        if ((output.page_type == "atNonPWAllChildren" || output.page_type == "atNonPWAllCovObjs") && output.non_pw_type == "mtDirect") {
            //$parent.runProgressBar(1, 0, 50, stringResources.yellowLoadingIcon.save);
            save_multi_direct_on_next();
        }
        if (output.page_type == "atPairwise") {
            save_pairwise_on_next(false);
            if (output.previous_step > 0) {
                $.cookie('CallJudgementTable', "True");
                var callResultTable = $.cookie('callResultTable');
                if (callResultTable != undefined && callResultTable != null && callResultTable == 'true') {
                    $.removeCookie('CallJudgementTable');
                    $.removeCookie('callResultTable');
                }

                /*$.removeCookie('SelectionID');*/
                /*$.cookie('judgementStep');*/
                $('#hdnPageNumber').val(output.previous_step);
                setTimeout(function () {
                    $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
            }

            //$('#hdnPageNumber').val(output.previous_step);
            //setTimeout(function () {
            //    $('#MainContent_hdnPageNo').trigger('click');
            //}, 10);
        }

        if (output.page_type == "atNonPWOneAtATime" && output.non_pw_type == "mtStep") {
            save_step_function_on_next();
        }
        if (output.page_type == "atNonPWOneAtATime" && (output.non_pw_type == "mtAdvancedUtilityCurve" || output.non_pw_type == "mtRegularUtilityCurve")) {
            save_utility_curve_on_next();
        }

        if (output.page_type == "atNonPWOneAtATime" && output.non_pw_type == "mtRatings") {
            save_ratings_on_next();
        }

        if (output.page_type == "atNonPWOneAtATime" && output.non_pw_type == "mtDirect") {
            save_direct_on_next();
        }
    }
}

var cancelProjectLockStatusTimer = function () {
    if (angular.isDefined(output.projectLockStatusTimer)) {
        setTimeout.cancel(output.projectLockStatusTimer);
        output.projectLockStatusTimer = undefined;
    }
}

var checkProjectLockStatus = function () {
    //cancelProjectLockStatusTimer();
    $(".locked-reload-message").show();
    var urlbase = getBaseUrl();
    $.ajax({
        type: "POST",
        url: urlbase + "AnytimeComparion/pages/Anytime/Anytime.aspx/CheckProjectLockStatus",
        success: function (data) {
            var lockedInfo = JSON.parse(data.d);

            if (lockedInfo.teamTimeUrl.length > 0) {
                LoadingScreen.end(100, false);
                output.lockedInfo = lockedInfo;
                //$scope.$parent.projectLockedInfo.status = true;

                $("div.teamtime-started").remove();
                $(".tt-full-height.tt-body.tt-pipe.anytime-wrap").remove();

                //Redirect to Comparion TeamTime after 3 seconds as TeamTime session started
                projectLockStatusTimer = setTimeout(function () {
                    window.onbeforeunload = null;
                    window.location.href = lockedInfo.teamTimeUrl;
                }, 5000);
            }
            else {
                if (projectLockedInfo.status != lockedInfo.status) {
                    output.lockedInfo = lockedInfo;

                    if (projectLockedInfo.status) {
                        $(".tt-full-height.tt-body.tt-pipe.anytime-wrap").remove();
                    }
                    else {
                        window.onbeforeunload = null;
                        window.location.href = baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx";
                    }
                }

                if (projectLockedInfo.status) {
                    output.projectLockStatusTimer = setTimeout(function () {
                        checkProjectLockStatus();
                    }, 20000);
                }
            }

            $(".locked-reload-message").hide();
        },
        error: function (response) {
            output.projectLockedInfo.status = false;
            output.projectLockedInfo.message = "";
        }
    });
}

/*===============SensitivitiesAnalysis===============*/
var normalization_SA_list = [{ "value": "normAll", "text": "Normalized" }, { "value": "unnormalized", "text": "Unnormalized" }];
var normalization_SA = normalization_SA_list[0], Sense = null;

var setSANotmalization = function (normalization) {
    normalization_SA = normalization;
    initPage();
}


var IsInputText = false;
$(document).ready(function () {
    if ($('#direct_value_input').val() == '' || parseFloat($('#direct_value_input').val()) <= 0) {
        $('#dropdownHeaderValue0').css('width', '100%');
        $('#dropdownHeaderValueM0').css('width', '0%');
    }
    new_active_multi_index = 1;
    var activeRow = 0;
    for (i = 0; i < output.multi_non_pw_data.length; i++) {
        if (activeRow == 0) {
            if (multi_ratings_row[i][0].toString() == '' || multi_ratings_row[i][1] == "Not Rated") {
                addActiveClass($('#div_row_wrapper_' + i), '', i.toString())
                activeRow++;
            }
        }
    }
    if (activeRow == 0) {
        addActiveClass($('#div_row_wrapper_0'), '', '0')
    }
    $("#direct_value_input").on("input", function () {
        var VAL = $(this).val();
        if (VAL < 0 || VAL > 1) {
            alert('please enter value from 0 to 1');
            $('#direct_value_input').val('');
            $('#InputSingleRatingDirect').val('');
            return false;
        }
        IsInputText = true;
        get_direct_value('direct_value_input');
        $('#InputSingleRatingDirect').val(VAL);
    });
});

$(document).ready(function () {
    $("#InputSingleRatingDirect, #direct_value_input").on("input", function () {
        var VAL = $(this).val();
        if (VAL < 0 || VAL > 1) {
            alert('please enter value from 0 to 1');
            $('#direct_value_input').val('');
            $('#InputSingleRatingDirect').val('');
            return false;
        }
        IsInputText = true;
        $('#direct_value_input').val(VAL);
        $('#InputSingleRatingDirect').val(VAL);
        get_direct_value('InputSingleRatingDirect');

    });
});
function isNumberKeyWithDecimalMR(txt, evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode == 13) {
        var index = parseInt(new_active_multi_index) + 1;
        var crid = "input_2_" + index;
        addActiveClass($('#div_row_wrapper_' + index), '', index.toString());
        $('#' + crid).focus();
        return false;
    }
    if (charCode == 46) {
        //Check if the text already contains the . character
        if (txt.value.indexOf('.') === -1) {
            return true;
        }
        else {
            return false;
        }
    }
    else {
        if (charCode > 31 &&
            (charCode < 48 || charCode > 57))
            return false;
    }
    return true;
}
$(function () {
    $('.directinput').on("keypress", function (e) {
        /* ENTER PRESSED*/
        if (e.keyCode == 13) {
            var inpid = e.currentTarget.id;
            inpid = inpid.replace("at_direct_input", "");
            inpid = parseInt(inpid) + 1;
            inpid = "at_direct_input" + inpid;
            document.getElementById(inpid).focus();
            document.getElementById(inpid).select();
            return false;
        }
    });
});

$(function () {
    $('.TabMultiRate').on("keypress", function (e) {
        /* ENTER PRESSED*/
        if (e.keyCode == 13) {
            var inpid = e.currentTarget.id;
            inpid = inpid.replace("input_1_", "");
            inpid = parseInt(inpid) + 1;
            var inpidtext = "input_1_" + inpid;

            document.getElementById(inpidtext).focus();
            document.getElementById(inpidtext).select();
            $('#div_row_wrapper_' + (parseInt(inpid) - 1)).removeClass('active_table_row');
            $('#div_row_wrapper_' + (parseInt(inpid))).addClass('active_table_row');
            /*$('#div_row_wrapper_' + parseInt(index + 1)).toggle('click');*/
            addActiveClass($('#div_row_wrapper_' + (parseInt(inpid))), '', (parseInt(inpid)).toString());
            return false;
        }
        return true;
    });
});
var PrevIntensityID = 0;
var save_ratings = function (value) { //add comment attr here later
    // var loadingMessage = stringResources.yellowLoadingIcon.save;
    //if ($scope.output.is_auto_advance && (!$scope.is_comment_updated && !(value < 0)))
    // loadingMessage = stringResources.yellowLoadingIcon.saveAA;  
    if (PrevParentnodeID != 0) {
        if (PrevIntensityID != value) {
            $("#parentnode" + PrevParentnodeID).hide();
        }
    }
    PrevIntensityID = value;
    var direct_value = "";
    var DirVal = false;
    if (value.toString().indexOf("*") >= 0 || value == -2) {
        output.is_direct = true;
        direct_value = value.toString().replace("*", "");
        if (direct_value == "-2") {
            /*direct_value = "0";*/
            DirVal = true;
            if (!IsInputText) {
                $("#direct_value_input").val('');
            }
        }
        ratings_data = [direct_value, "Direct Value", "-2", "0"];
        value = -2;
        //Bhawan Scip
        singleRatingTempDirectValue = direct_value >= 0 ? parseFloat(direct_value) : "";
        output.non_pw_value = direct_value >= 0 ? parseFloat(direct_value) : "";
        //console.log("singleRatingTempDirectValue: " + singleRatingTempDirectValue + ", non_pw_value: " + output.non_pw_value);        
        //$("#direct_value_input").focus();
        //$("#-2").prop("checked", true);
    }
    else {
        $("#InputSingleRatingDirect").val("");
        RedirecttoNext = true;
        $.each(output.intensities, function (index, intensity) {
            if (intensity[2] == value) {
                ratings_data = intensity;
                output.non_pw_value = intensity[0] >= 0 ? parseFloat(intensity[0]) : intensity[0];
                $('#InputSingleRatingDirect').val(parseFloat(intensity[0]).toFixed(4))
                $("#direct_value_input").val(parseFloat(intensity[0]).toFixed(4))
                output.is_direct = false;
                return false;
            }
        });

        //if (value < 0) {
        //    $("#direct_value_input").val("");
        //}
    }
    $("input[type='radio']").prop("checked", false);
    $("input[type='radio'][id='intensity_" + value + "']").prop("checked", true);

    if (ratings_data != undefined && ratings_data != null) {
        var TxtNum = parseFloat(ratings_data[0] * 100);
        TxtNum = Math.round(TxtNum * 100) / 100;
        if (TxtNum > 0 || ratings_data[1] == "None") {
            if (output.showPriorityAndDirect) {
                $('#dropdownHeaderValue0').text(ratings_data[1] + " " + TxtNum + "%");
                $('#spndropdownHeaderValueM0').text(ratings_data[1] + " " + TxtNum + "%");
            }
            else {
                $('#dropdownHeaderValue0').text(ratings_data[1]);
                $('#spndropdownHeaderValueM0').text(ratings_data[1]);
            }
            //$('#spndropdownHeaderValueM0').text(ratings_data[1] + " " + TxtNum + "%");
            //$('#spndropdownHeaderValueM0').text(ratings_data[1]);
            $('#progress_bar_0').attr('aria-valuenow', parseFloat(TxtNum * 100).toFixed(4)).css('width', parseFloat(TxtNum * 100).toFixed(4) + '%');
            $('#dropdownHeaderValueM0').attr('aria-valuenow', parseFloat(TxtNum * 100).toFixed(4)).css('width', parseFloat(TxtNum * 100).toFixed(4) + '%');
        }
        else {
            $('#dropdownHeaderValue0').text(ratings_data[1]);
            $('#spndropdownHeaderValueM0').text(ratings_data[1]);
        }
        //if (ratings_data[1].toLowerCase() == 'direct value' && $('#InputSingleRatingDirect').val() != undefined && $('#InputSingleRatingDirect').val() != null
        //    && $('#InputSingleRatingDirect').val().trim() != '' && parseFloat($('#InputSingleRatingDirect').val()) >= 0) {
        //    item.Value = parseFloat($('#InputSingleRatingDirect').val());
        //}
        $('#progress_bar_0').attr('aria-valuenow', parseFloat(ratings_data[0] * 100).toFixed(4)).css('width', parseFloat(ratings_data[0] * 100).toFixed(4) + '%');
        $('#dropdownHeaderValueM0').attr('aria-valuenow', parseFloat(ratings_data[0] * 100).toFixed(4)).css('width', parseFloat(ratings_data[0] * 100).toFixed(4) + '%');
        if (TxtNum == undefined || TxtNum <= 0) {
            $('#dropdownHeaderValue0').css('width', '100%');
        }
        if (ratings_data[1].toString() != 'Not Rated' || TxtNum >= 0) {
            $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'none', 'opacity': '1' })
            $('#divRadioButtons .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        }
        else {
            $('#div_row_wrapper_0 .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
            $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        }
        //if ($('#intensity_-2')[0].checked) {
        //    if ($('#InputSingleRatingDirect').val() != '' && parseFloat($('#InputSingleRatingDirect').val()) > 0) {
        //        $('#divRadioButtons .imgClose').css({ 'filter': 'none', 'opacity': '1' })
        //    }
        //    else {
        //        $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        //    }
        //}
        //else {
        //    $('#divRadioButtons .imgClose').css({ 'filter': 'grayscale(1)', 'opacity': '0.5' })
        //}
    }

    //$('#direct_value_input').val(parseFloat(ratings_data[0]).toFixed(4));


    //Bhawan Skiped above line
    if (ratings_data[2].toString() == '' || parseFloat(ratings_data[2]) < 0) {
        if (ratings_data[2].toString() == '-1') {
            $('#direct_value_input').val('');
        }
        //else {
        //    $('#direct_value_input').val('0.0000');
        //    $('#InputSingleRatingDirect').val('0.0000');
        //}
    }

    if (value == -1) {
        $("#direct_value_input").val("");
        $("#InputSingleRatingDirect").val("");
        $('#close_icon_0 i').hide();
    }
    else {
        $('#close_icon_0 i').show();
    }
    if (!DirVal && !IsInputText) {
        $('#InputSingleRatingDirect').val('');
    }
    else {

    }
    IsInputText = false;
    if (output.is_auto_advance && !output.is_direct && value != -1) {
        save_ratings_on_next();

        setTimeout(function () {
            $(".next_step").removeClass("back-pulse");
        }, 200);
    }
    else {
        save_ratings_on_next();
        //$timeout(function () {
        //    $scope.$apply(function () {
        //        $scope.ratings_data = $scope.ratings_data;
        //        // //console.log(  $scope.ratings_data );
        //    });
        //}, 2000);
    }

    disableNext = output.pipeOptions.dontAllowMissingJudgment && (value == -1);
    output.IsUndefined = (value == -1);
    output.IsUndefined = (value == -1);

    if (value == -1) {
        $(".next_step").removeClass("back-pulse");
    } else {
        $(".next_step").addClass("back-pulse");
    }
}
//Header Popu
function showHeaderPopup(text, nodeId, type, step, location, guid, nodeType, nodeText, wrtText, iconcolorid) {
    $(".hideshowinfo").css('display', 'none');
    //$('.infocolor' + iconcolorid).removeClass('infoColor');
    if (nodeType == undefined || nodeType == null || nodeType == '' || nodeType == 'parent-node1' || nodeType == 'wrt-text-node' || nodeType == 'left-text-node') {
        node_type = type;
        node_location = location;
        current_step = step;
        node_id = nodeId;
        node_guid = guid;
        if (nodeType == 'wrt-text-node') {
            $("#exampleModalLongTitle1").html('Edit ' + nodeText + '" with respect to ' + iconcolorid);
            hideWrapper(0, $('#irr_' + wrtText));
            hideWrapper(0, $('#ilr_' + wrtText));
            activeRow(wrtText, false);
        }
        else
            if (nodeType == 'left-text-node') {
                $("#exampleModalLongTitle1").html('Edit Description/definition for ' + nodeText);
                hideWrapper(0, $('#ir_' + wrtText));
                hideWrapper(0, $('#il_' + wrtText));
                activeRow(wrtText, false);
            }
            else
                $("#exampleModalLongTitle1").val('Edit Description/Definition for "Price"');
        if (nodeType == 'parent-node1') {
            $("#exampleModalLongTitle1").html('Edit Description/Definition for ' + nodeText);
        }
        $('#MainContent_GlobalInfoDocModal1').modal('show');
        //CKEDITOR.instances.GlobalInfoDocValue.setData('');
        tinymce.get("GlobalInfoDocValue1").setContent('');
        setTimeout(function () {
            //CKEDITOR.instances[** fieldname **].setData(** your data **)
            // CKEDITOR.instances['GlobalInfoDocValue'].setData(text);
            tinymce.get("GlobalInfoDocValue1").setContent(text);
        }, 500);
    }
    else {
        node = nodeType;
        node_type = type;
        node_location = location;
        current_step = hdnCurrentStep;
        node_id = nodeId;
        node_guid = guid;
        //parent node
        if (nodeType == 'parent-node') {
            $("#exampleModalLongTitle1").html('Edit description/definition for ' + nodeText);
        }
        //first node
        if (nodeType == 'left-node') {
            $("#exampleModalLongTitle1").html('Edit description/definition for ' + nodeText);
        }
        //wrt first node
        if (nodeType == 'wrt-left-node1' || nodeType == 'wrt-right-node1') {
            $("#exampleModalLongTitle").html('Edit ' + nodeText + ' with respect to "' + wrtText + '"');
        }
        if (nodeType == 'wrt-left-node') {
            $("#exampleModalLongTitle1").html('Edit ' + nodeText + ' with respect to "' + iconcolorid + '"');
        }
        //second node
        if (nodeType == 'right-node') {
            $("#exampleModalLongTitle1").html('Edit description/definition for ' + nodeText);
        }
        //wrt second node
        if (nodeType == 'wrt-right-node') {
            $("#exampleModalLongTitle1").html('Edit ' + nodeText + ' with respect to "' + iconcolorid + '"');
        }
        //scale-node
        if (nodeType == 'scale-node') {
            $("#exampleModalLongTitle1").html('Edit description for measurement scale "' + nodeText + '"');
        }
        //question-node
        if (nodeType == 'question-node') {
            $("#exampleModalLongTitle1").html('Edit Header content');
        }
        if (wrtText == 'Subheading') { $("#exampleModalLongTitle1").html('Edit Heading'); }

        //$("#exampleModalLongTitle").html('Edit Description/Definition for "Price"');
        $('#MainContent_GlobalInfoDocModal1').modal('show');
        //CKEDITOR.instances.GlobalInfoDocValue.setData('');
        tinymce.get("GlobalInfoDocValue1").setContent('');
        setTimeout(function () {
            // CKEDITOR.instances['GlobalInfoDocValue'].setData(nodeText);          
            tinymce.get("GlobalInfoDocValue1").setContent(text);
        }, 500);
        tinymce.get("GlobalInfoDocValue_ifr").setContent('');
        setTimeout(function () {
            // CKEDITOR.instances['GlobalInfoDocValue'].setData(nodeText);          
            tinymce.get("GlobalInfoDocValue_ifr").setContent(text);
        }, 500);

    }
}
var ConfrimResetTask = function () {
    if (confirm('Do you really want to reset text to default value?')) {
        $("#overlay").css('display', 'block');
        var editors_content = output.sRes;
        node_id = node_id == null ? "0" : node_id;
        node_guid = node_guid == null ? "" : node_guid;
        $.ajax({
            type: "POST",
            url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveInfoDocs",
            data: JSON.stringify({
                nodetxt: editors_content,
                obj: node_type,
                node: node_location,
                current_step: hdnCurrentStep,
                node_id: node_id,
                node_guid: node_guid,
                Guids: Guids
            }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            beforeSend: function () {
                $('.tt-GlobalInfoDoc-status').html('<div data-alert="" class="alert alert-success alert-dismissible fade show">           Saving...              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
            },
            success: function (data) {
                $('#MainContent_GlobalInfoDocModal1').modal('hide');
                setTimeout(function () {
                    $('#MainContent_hdnPageNo').trigger('click');
                }, 10);
            },
            complete: function () {
                $('.tt-GlobalInfoDoc-status').html('<div data-alert="" class="alert alert-primary alert-dismissible fade show">           Save              <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
            },
            error: function (response) {
            }
        });
    }
    $("#overlay").css('display', 'none');
}
//function pageasncronize() {
//    var lastActiveIndex = $('div.chart_wrapper').last().data('val');
//    var lastIndexToBeBind = lastActiveIndex + 10;
//    var html = '', text = '', WRTtext = '', fClass = '';
//    for (var index = lastActiveIndex + 1; index <= lastIndexToBeBind; index++) {
//        if ((!output.is_infodoc_tooltip || output.framed_info_docs) && (output.page_type == 'atAllPairwise' || output.page_type == 'atAllPairwiseOutcomes') && output.showinfodocnode) {
//            $('#chkTooltip').prop('checked', false);
//            html += '<div class="row">';
//            //left node div
//            html += '<div class="col-md-3">';
//            //first node
//            html += '<div class="top_content_wrapper">';
//            if (output.first_node_info == undefined || output.first_node_info == null || output.first_node_info.trim() == '')
//                fClass = ' frameChkEvaluator';
//            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="1" id="checkboxes1">';
//            html += '<div class="toggle_heading topheading_ellipse">';
//            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon1" aria-hidden="true"></i>';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + '</span>';
//            }
//            else {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + '</span>';
//            }
//            html += '</div>';
//            html += '<div class="toggle_area" id="checkout-shipping-address-1">';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<p>' + output.multi_pw_data[index].InfodocLeft + '</p>';
//            }
//            else {
//                html += '<p>' + output.multi_pw_data[index].InfodocLeft + '</p>';
//            }
//            text = encodeURIComponent(output.multi_pw_data[index].InfodocLeft);
//            WRTtext = encodeURIComponent(output.multi_pw_data[index].LeftNode);
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Left + '",2,2,1,null,"left-node",decodeURIComponent("' + WRTtext + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            else {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Left + '",2,2,1,null,"left-node",decodeURIComponent("' + WRTtext + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
//            html += '</a>';
//            html += '</div>';
//            html += '</div>';

//            //first WRT node
//            html += '<div class="top_content_wrapper' + fClass + '">';
//            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="3" id="checkboxes3">';
//            html += '<div class="toggle_heading topheading_ellipse">';
//            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon3" aria-hidden="true"></i>';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + ' WRT ' + output.parent_node + '</span>';
//            }
//            else {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].LeftNode + ' WRT ' + output.parent_node + '</span>';
//            }
//            html += '</div>';
//            html += '<div class="toggle_area" id="checkout-shipping-address-3">';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<p>' + output.multi_pw_data[index].InfodocLeftWRT + '</p>';
//            }
//            else {
//                html += '<p>' + output.multi_pw_data[index].InfodocLeftWRT + '</p>';
//            }

//            text = encodeURIComponent(output.LeftNode);
//            WRTtext = encodeURIComponent(output.InfodocLeftWRT);
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Left + '",3,2,1,null,"wrt-left-node",decodeURIComponent("' + text + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            else {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Left + '",3,2,1,null,"wrt-left-node",decodeURIComponent("' + text + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
//            html += '</a>';
//            html += '</div>';
//            html += '</div>';

//            html += '</div>';
//            //End left node div

//            //Parent div
//            fClass = '';
//            if (output.parent_node_info == undefined || output.parent_node_info == null || output.parent_node_info.trim() == '')
//                fClass = ' frameChkEvaluator';
//            html += '<div class="col-md-6' + fClass + '">';
//            html += '<div class="top_content_wrapper">';
//            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="0" id="checkboxes0">';
//            html += '<div class="toggle_heading topheading_ellipse">';
//            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon0" aria-hidden="true"></i>';
//            html += '<span class="ms-2">' + output.parent_node + '</span>';
//            html += '</div>';
//            html += '<div class="toggle_area" id="checkout-shipping-address-0">';
//            html += '<p>' + output.parent_node_info + '</p>';
//            text = encodeURIComponent(output.parent_node_info);
//            WRTtext = encodeURIComponent(output.parent_node);
//            if (output.page_type == 'atAllPairwise') {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),0,-1,2,-1,0,"parent-node1",decodeURIComponent("' + WRTtext + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            else {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),0,-1,2,-1,0,"parent-node1",decodeURIComponent("' + WRTtext + '")) class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            if (output.parent_node_info > 0) {
//                html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
//            }
//            else {
//                html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
//            }
//            html += '</a>';
//            html += '</div>';
//            html += '</div>';
//            html += '</div>';
//            //End parent div

//            //right node div
//            html += '<div class="col-md-3">';
//            //second node div
//            fClass = '';
//            if (output.second_node_info == undefined || output.second_node_info == null || output.second_node_info.trim() == '')
//                fClass = ' frameChkEvaluator';
//            html += '<div class="top_content_wrapper' + fClass + '">';
//            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="2" id="checkboxes2">';
//            html += '<div class="toggle_heading topheading_ellipse">';
//            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon2" aria-hidden="true"></i>';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + '</span>';
//            }
//            else {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + '</span>';
//            }
//            html += '</div>';
//            html += '<div class="toggle_area" id="checkout-shipping-address-2">';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<p>' + output.multi_pw_data[index].InfodocRight + '</p>';
//            }
//            else {
//                html += '<p>' + output.multi_pw_data[index].InfodocRight + '</p>';
//            }

//            text = encodeURIComponent(output.multi_pw_data[index].InfodocRight);
//            WRTtext = encodeURIComponent(output.multi_pw_data[index].RightNode);
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Right + '",2,2,1,null,"right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            else {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + text + '"),"' + output.multi_pw_data[index].NodeID_Right + '",2,2,1,null,"right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
//            html += '</a>';
//            html += '</div>';
//            html += '</div>';

//            //second WRT node
//            html += '<div class="top_content_wrapper' + fClass + '">';
//            html += '<input type="checkbox" class="toggle_checkbox me-2 checkboxes" data-val="4" id="checkboxes4">';
//            html += '<div class="toggle_heading topheading_ellipse">';
//            html += '<i class="fa fa-minus-square mb-2' + fClass + '" id="checkBoxIcon4" aria-hidden="true"></i>';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + ' WRT ' + output.parent_node + '</span>';
//            }
//            else {
//                html += '<span class="ms-2">' + output.multi_pw_data[index].RightNode + ' WRT ' + output.parent_node + '</span>';
//            }
//            html += '</div>';
//            html += '<div class="toggle_area" id="checkout-shipping-address-4">';
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<p>' + output.multi_pw_data[index].InfodocRightWRT + '</p>';
//            }
//            else {
//                html += '<p>' + output.multi_pw_data[index].InfodocRightWRT + '</p>';
//            }

//            text = encodeURIComponent(output.multi_pw_data[index].RightNode);
//            WRTtext = encodeURIComponent(output.multi_pw_data[index].InfodocRightWRT);
//            if (output.page_type == 'atAllPairwise' && output.multi_pw_data.length > 0) {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Right + '",3,2,1,null,"wrt-right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            else {
//                html += '<a href="javascript:void(0);" onclick=showInfoPopup(decodeURIComponent("' + WRTtext + '"),"' + output.multi_pw_data[index].NodeID_Right + '",3,2,1,null,"wrt-right-node",decodeURIComponent("' + WRTtext + '"),"' + index + '") class="edit_icon" data-bs-toggle="modal" data-bs-target="#staticBackdrop1">';
//            }
//            html += '<i class="fa fa-pencil frameChkEvaluator" aria-hidden="true"></i>';
//            html += '</a>';
//            html += '</div>';
//            html += '</div>';

//            html += '</div>';
//            //End right node div
//            html += '</div>';

//            //$('#InfoDocsParentDiv_1').html('');
//            $('#InfoDocsParentDiv_1').html(html);
//            $('#InfoDocsParentDiv').hide();
//            $('#InfoDocsParentDiv_1').show();
//            $('#chkTooltip').prop('checked', false);
//        }
//        else {
//            $('#chkTooltip').prop('checked', true);
//            $('#InfoDocsParentDiv').show();
//            $('#InfoDocsParentDiv_1').hide();
//        }
//    }
//}

setTimeout(function () {
    if ($(".removep").length) {
        var phtml = $('.removep').html().replaceAll('&nbsp;', '').replaceAll('<p></p>', '');

        $('.removep').html('');
        $('.removep').html(phtml);
    }
    if ($(".removep1").length) {
        var phtml1 = $('.removep1').html().replaceAll('&nbsp;', '').replaceAll('<p></p>', '');

        $('.removep1').html('');
        $('.removep1').html(phtml1);
    }
    if ($(".removep2").length) {
        var phtml2 = $('.removep2').html().replaceAll('&nbsp;', '').replaceAll('<p></p>', '');

        $('.removep2').html('');
        $('.removep2').html(phtml2);
    }
    if ($(".removep3").length) {
        var phtml3 = $('.removep3').html().replaceAll('&nbsp;', '').replaceAll('<p></p>', '');
        $('.removep3').html('');
        $('.removep3').html(phtml3);
    }
    //
    //if ($("#ctl00").length) {
    //    var BoldRemovedHtml = $('#ctl00').html().replaceAll('<b>', '').replaceAll('</b>', '');
    //  /*  $('#ctl00').html('');*/
    //    $('#ctl00').html(BoldRemovedHtml);
    //}ChartRightInfoDoc

    if ($("#ChartRightInfoDoc").length) {
        var BoldRemovedHtml = $('#ChartRightInfoDoc').html().replaceAll('<b>', '').replaceAll('</b>', '');
        /*   $('#ChartRightInfoDoc').html('');*/
        $('#ChartRightInfoDoc').html(BoldRemovedHtml);
    }
    if ($("#CurvechartInput").length) {
        var BoldRemovedHtml = $('#CurvechartInput').html().replaceAll('<b>', '').replaceAll('</b>', '');
        /*    $('#CurvechartInput').html('');*/
        $('#CurvechartInput').html(BoldRemovedHtml);
        $("#noUiInput11").val(main_bar[0]);
        $("#noUiInput21").val(main_bar[1]);
    }

    if ($("#Rating_Scall").length) {
        var BoldRemovedHtml = $('#Rating_Scall').html().replaceAll('<b>', '').replaceAll('</b>', '');
        $('#Rating_Scall').html('');
        $('#Rating_Scall').html(BoldRemovedHtml);
    }
    if ($("#Rating_Scall").length) {
        var BoldRemovedHtml = $('#Rating_Scall').html().replaceAll('<b>', '').replaceAll('</b>', '');
        $('#Rating_Scall').html('');
        $('#Rating_Scall').html(BoldRemovedHtml);
    }
    if ($("#MultingRatingHeader").length) {
        var BoldRemovedHtml = $('#MultingRatingHeader').html().replaceAll('<b>', '').replaceAll('</b>', '');
        $('#MultingRatingHeader').html('');
        $('#MultingRatingHeader').html(BoldRemovedHtml);
    }

}, 2000);


function SortByPriorityAndOrder(Sortval) {
    var SortingVal = Sortval.value;
    saveOBPriority(SortingVal);
}
//function HeaderBtnInfoDocHideShow() {
//    var IsAllHeaderHideOnLoad = false;
//    var NoOfClick = 0;
//    if (NoOfClick == 0) {
//        if ($("#Header_InfoDocs").is(":hidden")) {
//            $("#img1").hide();
//            $("#img2").show();
//            $("#img3").hide();
//            IsAllHeaderHideOnLoad = true;
//            $("#Header_InfoDocs").show();
//        }
//        else {
//            $("#img1").hide();
//            $("#img2").hide();
//            $("#img3").show();
//            $("#Header_InfoDocs").hide();
//        }
//    }
//    else {
//        if (IsAllHeaderHideOnLoad) {
//            if ($("#Header_InfoDocs").is(":hidden")) {
//                $("#img1").hide();
//                $("#img2").show();
//                $("#img3").hide();
//                $("#Header_InfoDocs").show();
//            }
//            else {
//                $("#img1").show();
//                $("#img2").hide();
//                $("#img3").hide();
//                $("#Header_InfoDocs").hide();
//            }
//        }
//        else {
//            if ($("#Header_InfoDocs").is(":hidden")) {
//                $("#img2").show();
//                $("#img3").hide();
//                $("#Header_InfoDocs").show();
//            }
//            else {
//                $("#img2").hide();
//                $("#img3").show();
//                $("#Header_InfoDocs").hide();
//            }
//        }
//    }

//    NoOfClick = NoOfClick + 1;

//}
$(document).ready(function () {
    setTimeout(function () {
        var IsRemoveCss = true;
        if (output.page_type == 'atInformationPage') {
            IsRemoveCss = false;
        }
        else {
            if (output.page_type == 'atSpyronSurvey') {
                IsRemoveCss = false;
            }
        }

        if (IsRemoveCss) {

            $('.main_wrapper1').addClass('main_wrapper');
            $('.main_wrapper').removeClass('main_wrapper1');
        }
        if (output.page_type == 'atNonPWAllChildren') {
            if (!!$.cookie('GenActiveRowID')) {
                var index = $.cookie("GenActiveRowID");
                if (index != "-1") {
                    /*  $('#div_row_wrapper_' + index).addClass('active_table_row');*/
                    addActiveClass($('#div_row_wrapper_' + index), '', index);
                    /* $.cookie("GenActiveRowID", "-1");*/
                }
            }
        }

    }, 3);

    $('.judgement-help-icon-info').click(function () {
        $('.show-judgement-help-info').toggle();
    })

    $('.openpinbtn').click(function () {
        $('.main-content-data-wrapper').toggleClass('col-md-10');
        $('.main-content-data-wrapper').removeClass('col-md-12');
        //$('.show-legend-and-scale-wrapper').toggleClass('col-md-2');
        $('.show-legend-and-scale-wrapper').toggle();

    })

    var IsAllHeaderHideOnLoad = false;
    var NoOfClick = 0;
    $("#HeaderDivIcon").click(function () {
        if ($("#img1").is(":visible")) {
            IsBlankTxt = true;
            IsVisibleImgNo = 2;
        }
        else {
            if (IsBlankTxt) { IsVisibleImgNo = 1; }
        }
        if (NoOfClick == 0) {
            if ($("#Header_InfoDocs").is(":hidden")) {
                $("#img1").hide();
                $("#img2").show();
                $("#img3").hide();
                IsAllHeaderHideOnLoad = true;
                $("#Header_InfoDocs").show();
            }
            else {
                $("#img1").hide();
                $("#img2").hide();
                $("#img3").show();
                $("#Header_InfoDocs").hide();
            }
        }
        else {
            if (IsAllHeaderHideOnLoad) {
                if ($("#Header_InfoDocs").is(":hidden")) {
                    $("#img1").hide();
                    $("#img2").show();
                    $("#img3").hide();
                    $("#Header_InfoDocs").show();
                }
                else {
                    $("#img1").show();
                    $("#img2").hide();
                    $("#img3").hide();
                    $("#Header_InfoDocs").hide();
                }
            }
            else {
                if ($("#Header_InfoDocs").is(":hidden")) {
                    $("#img2").show();
                    $("#img3").hide();
                    $("#Header_InfoDocs").show();
                }
                else {
                    $("#img2").hide();
                    $("#img3").show();
                    $("#Header_InfoDocs").hide();
                }
            }
        }
        SetCssHieghtDynamicMobileView();
        NoOfClick = NoOfClick + 1;

    });

});

function CheckeRadioButton(id) {
    $('.intensity' + id)[0].checked = true;
};

var updateWelcomeText = function (value) {
    var editors_content = tinymce.get('txtWelcome_EndPage').getContent();
    editors_content = editors_content.replaceAll("<button class='welcome_next'disabled><span class='d-none d-lg-inline-block me-1'>Next</span> <img src='../../img/icon/Next.svg' class='icon'></button>", "'Next'");
    editors_content = editors_content.replaceAll('<button class="welcome_next" disabled="disabled"><span class="d-none d-lg-inline-block me-1">Next</span> <img class="icon" src="../../img/icon/Next.svg" /></button>', "'Next'");
    editors_content = editors_content.replaceAll("on the right of the screen", "near the top right of the screen");
    $('#MainContent_hdnWelcomeText').val(editors_content);
    $('#MainContent_hdnbtnWelcomeUpdate').trigger('click');
    //var html = "<div id='DivEditorIcon' class='heading_content justify-content-between'><div class='heading_info'><a href='javascript:void(0);' onclick='ShowWelcomPopup()'> <img src='../../img/icon/edit-icon.svg'></a></div></div>";
    //html += editors_content;
    //$('#MainContent_InformationControl_dvInformation').html(html);


    //$.ajax({
    //    type: "POST",
    //    url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/UpdateWelcomeText",
    //    data: JSON.stringify({
    //        welcomeText: value
    //    }),
    //    dataType: "json",
    //    async: false,
    //    contentType: "application/json; charset=utf-8",
    //    success: function (response) {
    //$('#MainContent_PopupModalWelcomeEndPage').modal('hide');
    //$('#MainContent_hdnPageNo').trigger('click');
    //        console.log(response);
    //    },
    //    error: function (error) {
    //        console.log(error
    //        );
    //    }
    //});
}
function ShowWelcomPopup(text, Title) {
    /* $(".hideshowinfo").css('display', 'none');*/
    $("#exampleModalLongTitleWelcome").html('Edit Welcome Screen');
    $('#MainContent_PopupModalWelcomeEndPage').modal('show');
    tinymce.get("txtWelcome_EndPage").setContent('');
    setTimeout(function () {
        tinymce.get("txtWelcome_EndPage").setContent(output.information_message);
    }, 500);
}
$(window).on('resize', function () {
    if (hdnpage_type == 'atSensitivityAnalysis') {
        //alert('Resize function called');
        //initData();
        initPage();
        //window.location = window.location.href;
    }
    var NewScreenWidth = window.innerWidth;
    if ($.cookie('ScreenWidth') != null) {
        var OldScreenWidth = $.cookie('ScreenWidth');
        if (NewScreenWidth != OldScreenWidth) {
            $.cookie('ScreenWidth', NewScreenWidth)
            /*   window.location.reload();*/
        }
    }
    else {
        $.cookie('ScreenWidth', NewScreenWidth)
        /* window.location.reload();*/
    }
    SetMobView();
});

function GetMtDirectIncompleteJudgementRowID(CurrentActiveRowID) {
    var active_multi_index = -1;
    for (i = CurrentActiveRowID; i < at_multi_direct_input.length; i++) {
        if (at_multi_direct_input[i] == '') { //undefined
            active_multi_index = i;
            break;
        }
    }
    if (active_multi_index == -1) {
        for (i = 0; i < CurrentActiveRowID; i++) {
            if (at_multi_direct_input[i] == '') { //undefined
                active_multi_index = i;
                break;
            }
        }
    }
    return active_multi_index;
}

function HideShowComment_icon_mobile(CommentImgID) {
    if (CommentImgID != undefined) {
        var Commentxt = "";
        if (output.page_type == 'atAllPairwise') {
            Commentxt = $('#txtRightComment').val();
            if (Commentxt == undefined) {
                Commentxt = $('#comment_' + CommentImgID).val();
            }
            if (Commentxt == '') {
                $("#multi_comment_icon_" + CommentImgID).hide();
                $("#multi_comment_iconEmpty_" + CommentImgID).show();

            }
            else {
                $("#multi_comment_icon_" + CommentImgID).prop('title', Commentxt);
                $("#multi_comment_icon_" + CommentImgID).show();
                $("#multi_comment_iconEmpty_" + CommentImgID).hide();
            }
        }
        else {
            if (output.page_type == 'atPairwise') {
                if (output.pairwise_type == 'ptGraphical' || output.pairwise_type == 'ptVerbal') {
                    if (!isMobile)
                        CommentImageShowHide(CommentImgID, $('#single_comment').val());
                    else
                        CommentImageShowHide(CommentImgID, $('#txtRightComment').val());
                }
            }
            else {
                if (output.page_type == 'atNonPWOneAtATime') {
                    if (output.non_pw_type == 'mtRegularUtilityCurve' || output.non_pw_type == 'mtStep') {
                        CommentImageShowHide(CommentImgID, $('#txtRightComment').val());
                    }
                    else {
                        if (output.non_pw_type == 'mtRatings') {
                            CommentImageShowHide(CommentImgID, $('#txtRightComment').val());
                        }
                        else {
                            CommentImageShowHide(CommentImgID, $('#single_comment').val());
                        }
                    }
                }
                else {
                    if (output.page_type == 'atNonPWAllChildren') {
                        if (output.non_pw_type == 'mtDirect' || output.non_pw_type == 'mtRatings') {
                            CommentImageShowHide(CommentImgID, $('#txtRightComment').val());
                        }
                    }
                }
            }
        }
    }
}

function HideShowComment_icon(CommentImgID) {
    if (CommentImgID != undefined) {
        var Commentxt = "";
        if (output.page_type == 'atAllPairwise') {
            Commentxt = $('#multi_comment_' + CommentImgID).val();
            if (Commentxt == undefined) {
                Commentxt = $('#comment_' + CommentImgID).val();
            }
            if (Commentxt == '') {
                $("#multi_comment_icon_" + CommentImgID).hide();
                $("#multi_comment_iconEmpty_" + CommentImgID).show();
            }
            else {
                $("#multi_comment_icon_" + CommentImgID).prop('title', Commentxt);
                $("#multi_comment_icon_" + CommentImgID).show();
                $("#multi_comment_iconEmpty_" + CommentImgID).hide();
            }
        }
        else {
            if (output.page_type == 'atPairwise') {
                if (output.pairwise_type == 'ptGraphical' || output.pairwise_type == 'ptVerbal') {
                    CommentImageShowHide(CommentImgID, $('#single_comment').val());
                }
            }
            else {
                if (output.page_type == 'atNonPWOneAtATime') {
                    if (output.non_pw_type == 'mtRegularUtilityCurve' || output.non_pw_type == 'mtStep') {
                        CommentImageShowHide(CommentImgID, $('#txtRightComment_0').val());
                    }
                    else {
                        if (output.non_pw_type == 'mtRatings') {
                            CommentImageShowHide(CommentImgID, $('#txtRightComment').val());
                        }
                        else {
                            CommentImageShowHide(CommentImgID, $('#single_comment').val());
                        }
                    }
                }
                else {
                    if (output.page_type == 'atNonPWAllChildren') {
                        if (output.non_pw_type == 'mtDirect' || output.non_pw_type == 'mtRatings') {
                            CommentImageShowHide(CommentImgID, $('#txtRightComment_' + CommentImgID).val());
                        }
                    }
                }
            }
        }
    }
}
function CommentImageShowHide(CommentImgID, Commentxt) {
    if (Commentxt == '') {
        $("#ImgComment_icon_" + CommentImgID).hide();
        $("#ImgComment_iconEmpty_" + CommentImgID).show();
        $('#comment_div_box_0').hide();
        $('#comment_mobile').modal('hide');
    }
    else {
        $("#ImgComment_icon_" + CommentImgID).prop('title', Commentxt);
        $("#ImgComment_icon_" + CommentImgID).show();
        $("#ImgComment_iconEmpty_" + CommentImgID).hide();
        $('#comment_div_box_0').hide();
        $('#comment_mobile').modal('hide');
    }
}
function ShowCommentBox(id) {
    $("#comment_" + id).val(hdnMultiPwData[id].Comment);
    $("#toggleComment_" + id).toggle()
}
function ShowCommentBoxMobile(id) {
    $("#txtRightComment").val(hdnMultiPwData[id].Comment);
    $('#comment_mobile').modal('show');
    $('#btnUpdateRightSideComment').attr('onclick', 'save_multi_pairwise("comment", ' + id + ', 0, 1, ' + id + ', ' + output.current_step + ')');
}
var CurveComment = '';
function HideMultiDerectCommentBox(index) {
    /*   $('.hideshowinfo').css('display', 'none');*/
    $('#comment_div_box_' + index).hide();
    $('#txtRightComment_' + index).val('');
}

function ShowUtiliCurveCommentTxtMobile() {
    //if ($('#comment_div_box_0').css('display') == 'none') {
    //    $('#comment_div_box_0').css('display', 'block')
    //}
    //else {
    //    $('#comment_div_box_0').css('display', 'none')
    //}
    $('#comment_mobile').modal('show');
    if (is_comment_updated) {
        $("#txtRightComment").val(comment);
       /* $("#commentBox").val($('#ImgComment_icon_5').attr("title"))*/;
    }
    else {
        $("#txtRightComment").val(output.comment);
        //$("#txtRightComment_0").val($('#ImgComment_icon_5').attr("title"));
    }
    $('#btnUpdateRightSideComment').attr('onclick', 'save_utility_curve_mobile(8, null, 1, "' + UCtype + '")');

}

function ShowUtiliCurveCommentTxt() {
    if ($('#comment_div_box_0').css('display') == 'none') {
        $('#comment_div_box_0').css('display', 'block')
    }
    else {
        $('#comment_div_box_0').css('display', 'none')
    }
    if (is_comment_updated) {
        $("#txtRightComment_0").val(comment);
       /* $("#commentBox").val($('#ImgComment_icon_5').attr("title"))*/;
    }
    else {
        $("#txtRightComment_0").val(output.comment);
        //$("#txtRightComment_0").val($('#ImgComment_icon_5').attr("title"));
    }

}

function SaveDirectCommentMobile(index) {
    CommentImageShowHide(index, $('#txtRightComment').val());
    $('#parentnode' + index).css('display', 'none');
    output.comment = $('#txtRightComment').val();
    save_direct_Comment();
}

function SaveDirectComment(index) {
    CommentImageShowHide(index, $('#single_comment').val());
    $('#parentnode' + index).css('display', 'none');
    output.comment = $('#single_comment').val();
    save_direct_Comment();
}
var save_direct_Comment = function () {
    var value = output.non_pw_value;
    //consoleLog("SaveDirect. current_step: " + $scope.current_step);

    $.ajax({
        type: "POST",
        url: baseUrl + "AnytimeComparion/pages/Anytime/Anytime.aspx/SaveDirect",
        data: JSON.stringify({
            step: hdnCurrentStep,
            value: value,
            sComment: output.comment
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (output.pipeOptions.dontAllowMissingJudgment) {
                if (output.non_pw_value == undefined || output.non_pw_value == null || parseFloat(output.non_pw_value) < 0) {
                    EnableDisablePagination(true);
                }
                else {
                    if (enabledLastStep == hdnCurrentStep)
                        EnableDisablePagination(false, true);
                    else
                        EnableDisablePagination(true, false);
                }
            }
        },
        error: function (response) {
            alert(JSON.stringify(response));
            //window.location.href = baseUrl;
        }
    });
}
/*var IsLocalResultClassAdded = false;*/
function ChangeClassLocalResultsPage() {
    $(".main_wrapper").removeAttr("style");
    $('.main_wrapper').addClass('main_wrapper_LocalResults');
    $('.main_wrapper_LocalResults').removeClass('main_wrapper');
    $('.main_wrapper').addClass('main_wrapper_LocalResults');
    SetCssHieghtDynamicLocalResultsMobileView();
    /* IsLocalResultClassAdded = true;*/
}

function SetDefaultCssClass() {
    /*   if (output.page_type == 'atShowLocalResults') */
    $('.main_wrapper_LocalResults').addClass('main_wrapper');
    $('.main_wrapper').removeClass('main_wrapper_LocalResults');
    $(".main_wrapper").removeAttr("style");
    /* IsLocalResultClassAdded = false;*/
    SetCssHieghtDynamicMobileView();
    /*    SetCssHieghtDynamicMobileView();*/
}
var AreaHeightOnLoadVal = 0;
var MarginOnLoad = 0;
function SetCssHieghtDynamicMobileView() {
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    if (isMobile) {
        // var areaHeight = ($('.page_heading_section').height() + 156);
        if ($("#img1").is(":visible")) {
            $("#img1").hide();
            $("#Header_InfoDocs").hide();
        }
        var areaHeight = ($('.page_heading_section').height() + 156);
        var screenHeight = window.innerHeight;
        var CalcHieght = "calc(" + screenHeight + "px - " + areaHeight + "px)";
        $('.main_wrapper').css("height", CalcHieght);
        $('.main_wrapper').css("max-height", CalcHieght);
        var margin = ($('.page_heading_section').height() + 98);
        $('.main_wrapper').css("margin-top", + margin + "px");
        if (AreaHeightOnLoadVal == 0) { AreaHeightOnLoadVal = areaHeight; MarginOnLoad = margin; }
    }
}
function SetCssHieghtDynamicLocalResultsMobileView() {
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    if (isMobile) {
        // var areaHeight = ($('.page_heading_section').height() + 156);
        var areaHeight = ($('.page_heading_section').height() + 156);
        $('.main_wrapper_LocalResults').css("height", "calc(100vh - " + areaHeight + "px)");
        $('.main_wrapper_LocalResults').css("max-height", "calc(100vh - " + areaHeight + "px)");
        var margin = ($('.page_heading_section').height() + 98);
        $('.main_wrapper_LocalResults').css("margin-top", + margin + "px");
    }
}
function RemoveLocalResultsPageClass() {
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    if (isMobile) {
        $(".main_wrapper").removeAttr("style");
        $('.main_wrapper').addClass('main_wrapper_LocalResults');
        $('.main_wrapper_LocalResults').removeClass('main_wrapper');
        // var areaHeight = ($('.page_heading_section').height() + 156);    
        $('.main_wrapper_LocalResults').css("height", "calc(100vh - " + AreaHeightOnLoadVal + "px)");
        $('.main_wrapper_LocalResults').css("max-height", "calc(100vh - " + AreaHeightOnLoadVal + "px)");
        $('.main_wrapper_LocalResults').css("margin-top", + MarginOnLoad + "px");
    }
    else {
        $(".main_wrapper_LocalResults").removeAttr("style");
        $('.main_wrapper_LocalResults').addClass('main_wrapper');
        $('.main_wrapper').removeClass('main_wrapper_LocalResults');
        $('.main_wrapper_LocalResults').addClass('main_wrapper');
    }
    /* IsLocalResultClassAdded = true;*/
}
function clearInputOnMultiPWGraphical(index) {

    $('#input1' + index).val('');
    $('#input2' + index).val('');
}
function display() {
    if ($.trim($('#ClipBoardData').val()) == '') {
        $('#btngo').addClass('Btndisabled');
    }
    else {
        $('#btngo').removeClass('Btndisabled');
    }
}

$(document).ready(function () {
    if (!output.showinfodocnode) {
        HideInfoDocs();
    }
    else {
        var details = navigator.userAgent;
        var regexp = /android|iphone|kindle|ipad/i;
        var isMobile = regexp.test(details);
        if (isMobile || !output.isPM) {
            HideInfoDocsWhentxtEmpty(true);
        }
    }
    $('#img2').click(function () {
        if (output.parent_node_info == '' && (output.PipeParameters.resultsTitle == undefined || output.PipeParameters.resultsTitle == '')) {
            $('#img3').css("filter", "grayscale(1)");
            $('#img3').css("opacity", "0.5");
        }
        else {
            $('#img3').css("filter", "none");
            $('#img3').css("opacity", "1");
        }
    });
});
function ShowMainWrapperStyle() {
    var stl = $(".main_wrapper").attr("style");
    alert(stl);
}
function HideInfoDocs() {
    $('.page_heading_section').children('.container').children('.row').children('.col-md-6').eq(1).hide();
    if (output.page_type == 'atAllPairwise') {
        $(".wrt_content").hide();
        $('.value_wrapper').children('.info_data').children('.body_content').hide();
        $('.value_wrapper').children('.info_data').children('.info_header').children('.header_edit_fw').hide();
        $('.value_wrapper').children('.info_data').children('.info_header').children('a').hide();
        $('.value_wrapper').addClass('empty_info');
        $('.title_tag').addClass('w-100');
    }
    else {
        if (output.page_type == 'atNonPWAllChildren') {
            if (output.non_pw_type == 'mtDirect') {
                $(".open_tooltip_info,.spnInfo").hide();
            }
            else {
                if (output.non_pw_type == 'mtRatings') {
                    $(".open_tooltip_info,.information_icon").hide();
                    $(".row_wrapper").addClass('shot_data');
                }
            }
        }
        else {
            if (output.page_type == 'atPairwise') {
                $(".wrt_content").hide();
                $('.value_wrapper').children('.info_data').children('.body_content').hide();
                $('.value_wrapper').children('.info_data').children('.info_header').children('.header_edit_fw').hide();
                $('.value_wrapper').children('.info_data').children('.info_header').children('a').hide();
                $('.value_wrapper').addClass('empty_info');
                $('.title_tag').addClass('w-100');
            }
            else {
                if (output.page_type == 'atNonPWOneAtATime') {
                    if (output.non_pw_type == 'mtRegularUtilityCurve' || output.non_pw_type == 'mtStep') {
                        $('.tooltips_group').children('.info_tooltip_main').eq(0).hide();
                        $('.row_wrapper').children('hr').hide();
                        $("div.accordion-body").hide();
                    }
                    else {
                        if (output.non_pw_type == 'mtRatings') {
                            $(".open_tooltip_info").hide();
                            $(".active_table_row").addClass('shot_data');
                            $('.tooltips_group').children('.info_tooltip_main').eq(0).hide();
                        }
                        else {
                            if (output.non_pw_type == 'mtDirect') {
                                $(".open_tooltip_info,.spnInfo").hide();
                            }
                        }
                    }
                }
            }
        }
    }

}

function HideInfoDocsWhentxtEmpty(IsHide) {
    if (IsHide) {
        //if (output.parent_node_info.trim() == '') {
        //    $('.page_heading_section').children('.container').children('.row').children('.col-md-6').eq(1).hide();
        //}
        //$(".NoInfDocDataRow").hide();
        $(".NoInfoData").addClass('d-none');
    }
    else {
        //if (output.parent_node_info.trim() == '')
        //{
        //    $('.page_heading_section').children('.container').children('.row').children('.col-md-6').eq(1).show();
        //}
        $(".NoInfoData").removeClass('d-none');
    }
    PageWiseAddRemoveClass(IsHide);
}
function PageWiseAddRemoveClass(IsHide) {
    if (output.page_type == 'atAllPairwise') {
        if (IsHide) {
            $(".nodataonesideinfodocrow").addClass("one_info_align")
            $(".emptyInfodocbothside").addClass('empty_info');
        }
        else {
            $(".nodataonesideinfodocrow").removeClass("one_info_align");
            $(".emptyInfodocbothside").removeClass('empty_info');
        }
        $('.title_tag').addClass('w-100');
    }
    else {
        if (output.page_type == 'atNonPWAllChildren') {
            if (output.non_pw_type == 'mtDirect') {
                if (IsHide) {
                    $(".nodataonesideinfodocrow").addClass("noinfdocdataoneside")
                }
                else {
                    $(".nodataonesideinfodocrow").removeClass("noinfdocdataoneside")
                }
            }
            else {
                if (output.non_pw_type == 'mtRatings') {
                    if (IsHide) {
                        $(".NoInfDocDataRow,.nodataonesideinfodocrow").addClass('shot_data');
                    }
                    else {
                        $(".NoInfDocDataRow,.nodataonesideinfodocrow").removeClass('shot_data');
                    }
                }
            }
        }
        else {
            if (output.page_type == 'atPairwise') {
                if (IsHide) {
                    $(".emptyInfodocbothside").addClass("empty_info");
                    $(".nodataonesideinfodocrow").addClass("one_info_align")
                }
                else {
                    $(".emptyInfodocbothside").removeClass("empty_info");
                    $(".nodataonesideinfodocrow").removeClass("one_info_align");
                }
                $('.title_tag').addClass('w-100');
            }
            else {
                if (output.page_type == 'atNonPWOneAtATime') {
                    if (output.non_pw_type == 'mtRegularUtilityCurve' || output.non_pw_type == 'mtStep') {
                        if (output.parent_node_info.trim() == '') {
                            $('.page_heading_section').children('.container').children('.row').children('.col-md-6').eq(1).hide();
                            if (!IsHide) { $('.page_heading_section').children('.container').children('.row').children('.col-md-6').eq(1).show(); }
                        }
                    }
                    else {
                        if (output.non_pw_type == 'mtDirect') {
                            if (IsHide) {
                                $(".nodataonesideinfodocrow").addClass("noinfdocdataoneside")
                            }
                            else {
                                $(".nodataonesideinfodocrow").removeClass("noinfdocdataoneside")
                            }
                        }

                    }
                }
            }
        }
    }
}

$(document).ready(function () {
    $(".custom-value").click(function () {
        if (output.multi_non_pw_data.length > 0) {
            for (i = 0; i < output.multi_non_pw_data.length; i++) {
                $('#UiDropdownList' + i + 'D').hide();
            }
        }
    });
});

function CloseList() {
    if (output.multi_non_pw_data.length > 0) {
        for (i = 0; i < output.multi_non_pw_data.length; i++) {
            $('#UiDropdownList' + i + 'D').hide();
        }
    }
}
function GetNextMultiRatingIncompleteJudgmentInex(currentIndex) {
    var FirstIncompleteJudgmentInex = -1;
    for (i = currentIndex; i < output.multi_non_pw_data.length; i++) {
        if (multi_ratings_row[i][0].toString() == '' || multi_ratings_row[i][1] == "Not Rated") {
            FirstIncompleteJudgmentInex = i;
            break;
        }
    }
    if (FirstIncompleteJudgmentInex == -1) {
        for (i = 0; i < output.multi_non_pw_data.length; i++) {
            if (multi_ratings_row[i][0].toString() == '' || multi_ratings_row[i][1] == "Not Rated") {
                FirstIncompleteJudgmentInex = i;
                break;
            }
        }
    }
    return FirstIncompleteJudgmentInex;
}

function SetMobView() {
    var details = navigator.userAgent;
    var regexp = /android|iphone|kindle|ipad/i;
    var isMobile = regexp.test(details);
    if (isMobile) {
        var areaHeight = ($('.page_heading_section').height() + 156);
        var screenHeight = window.innerHeight;
        var CalcHieght = "calc(" + screenHeight + "px - " + areaHeight + "px)";
        $('.main_wrapper').css("height", CalcHieght);
        $('.main_wrapper').css("max-height", CalcHieght);
        var margin = ($('.page_heading_section').height() + 98);
        $('.main_wrapper').css("margin-top", + margin + "px");
    }
}
$(document).ready(function () {
    setTimeout(function () {
        var element = document.querySelector('#MainHeaderInfodoc');
        if ((element.offsetHeight + 1 < element.scrollHeight) || (element.offsetWidth < element.scrollWidth)) {
            // your element have overflow
            /*   element.style.background = "yellow";*/
            $('#btnheadinfo').removeClass('d-none');
        }
        else {
            $('#btnheadinfo').addClass('d-none');
            //your element don't have overflow
        }
    }, 1000);
});
function showheaderpopup() {
    var txt = $('#MainHeaderInfodoc').html();
    $('#headerinfodocpopupbody').html(txt);
    $('#headerinfodocpopupbody').children('.threedoticon').remove();
    /* $('#headerinfodocpopupbody').html(output.cluster_phrase);*/
    $('#MainContent_GlobalInfoDocHeaderPopupModal').modal('show');
}

