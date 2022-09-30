$(document).ready(function () {
    $('div.txtStepTask').text('')
    $('#tblLocalResults thead th').click(function () {
        var table = $(this).parents('table').eq(0)
        var rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()))
        this.asc = !this.asc
        if (!this.asc) { rows = rows.reverse() }
        for (var i = 0; i < rows.length; i++) { table.append(rows[i]) }
    });
    function comparer(index) {
        return function (a, b) {
            var valA = getCellValue(a, index), valB = getCellValue(b, index)
            return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB)
        }
    }
    function getCellValue(row, index) { return $(row).children('td').eq(index).text() }
});

function bindDynamicHtml(val) {
    val = val == undefined || null ? '' : val;
    var html = '';
    $("#divTabResults").hide();
    $("#divDynamicResults").hide();

    if (val == 'show_satisfactory') {
        html += '<div class="text-center buttons_div">';
        html += '<button class="btn btn-clipboard w-100" type="button" onclick="reviewJudgment()"> Click here to review all judgments</button>';
        html += '<button class="btn btn-clipboard w-100" type="button"> Click here if you think the inconsistency is too high</button>';
        html += '<p class="pt-3"> if you think the priorities are not reasonable then:</p>';
        html += '<button class="btn btn-clipboard w-100 " type="button"> Click here if you would like to redo a judgment for one pair of elements</button>';
        html += '<button class="btn foot_btn p-2" onclick="bindDynamicHtml()" type="button">Cancel</button>';
        html += '</div>';
        $("#divDynamicResults").show();
        $("#divDynamicResults").addClass('col-lg-6 offset-lg-3');
        $("#divDynamicResults").html(html);
    }
    else {
        $("#divDynamicResults").html('');
        $("#divDynamicResults").removeClass();
        $("#divDynamicResults").hide();
        $("#divTabResults").show();
    }
}

//Save chnaged page number
function reviewJudgment() {
    var parentnodeID = $('#MainContent_hdnparentnodeID').val();
    var current_step = $('#MainContent_hdnCurrentStep').val();
    $.ajax({
        type: "POST",
        url: baseUrl + "pages/Anytime/Anytime.aspx/reviewJudgment",
        data: JSON.stringify({
            parentnodeID: parentnodeID,
            current_step: current_step
        }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var output = JSON.parse(data.d);
            $('#MainContent_hdnPageNumber').val(output);
            $('#MainContent_hdnPageNo').trigger('click');
        },
        error: function (response) {
        }
    });
}