<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="SensitivitiesAnalysis.ascx.vb" Inherits=".SensitivitiesAnalysis" %>

<script type="text/javascript" src="../../../Scripts/misc.js"></script>
<script type="text/javascript" src="../../../Scripts/drawMisc.js"></script>
<script type="text/javascript" src="../../../Scripts/ec.sa.js"></script>

<div id="divContent" class="divHeader" runat="server">
</div>


<script type="text/javascript">

    const { ui } = require("jquery");

    function SyncReceived(params) {

        //if (pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 0); $("#tblSA").removeAttr('disabled'); }
        if (pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 0); }
        //console.log(0);
        if (on_ok_code != "")
            eval(on_ok_code);
        on_ok_code = "";
        if (params != "") {
            //console.log(2);
            var received_data = JSON.parse(params);
            received_data = eval(received_data.d);
            if ((received_data)) {
                $("#DSACanvas").sa("GradientMinValues", received_data[0]);
            };
        };
    }

    function sendCommand(params, showPleaseWait) {
        //console.log(params + "&r=" + Math.random());
        if (showPleaseWait && pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 1); }
        $.ajax({
            type: "POST",
            data: JSON.stringify({
                data: params + "&r=" + Math.random()
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            async: false,
            url: baseUrl + "pages/Anytime/Anytime.aspx/Ajax_Callback",
            success: function (data) {
                SyncReceived(data);
            },
            error: function (response) {
                //console.log(response);
                if (pnl_loading_id != "") SwitchWarning(pnl_loading_id, 0);
                if (on_error_code != "") eval(on_error_code);
                on_error_code = "";
                jDialog("Erron on perform callback. Please reload page.", true, ";", undefined, "Error", 350, 280);
            }
        });
    }

    function parseData(params) {
        if ((params) && params != '') {
            var data = JSON.parse(params);
            data = eval(data.d);
            if (data.length == 2) {
                $('#selSubobjectives').find('option').remove().end();
                data_objs = data[0];
                data_alts = data[1];
                for (var i = 0; i < data_objs.length; i++) {
                    var obj = data_objs[i];
                    $('#selSubobjectives').append('<option value="' + i + '">' + obj.name + '</option>');
                };
                $('#DSACanvas').sa("option", "objs", data_objs);
                $('#DSACanvas').sa("option", "alts", data_alts);
                $('#DSACanvas').sa("resetSA");

            }
        }
    }

    function onChangeNode(id) {
        on_ok_code = cmd_ok;
        sendCommand("action=node&node_id=" + id, true);
    }

    function onChangeSubobjective(id) {
        id = $('#selSubobjectives').val();
        $('#DSACanvas').sa("option", "SelectedObjectiveIndex", Number(id));
        $('#DSACanvas').sa("redrawSA");
    }

    function onChangeNormalization(id) {
        on_ok_code = cmd_ok;
        sendCommand("action=normalization&norm_mode=" + id, true);
    }

    function onChangeSorting(id) {
        //var lineup = (id == 1);
        //$('#DSACanvas').sa("option", "PSALineup", lineup);
        $('#DSACanvas').sa("option", "SAAltsSortBy", id);
        $('#DSACanvas').sa("sortAlternatives");
        $('#DSACanvas').sa("redrawSA");
    }

    function onReset() {
        $('#DSACanvas').sa("resetSA");
    }

    var isAlternativesSorted = false;
    function onKeepSorted(chk) {
        isAlternativesSorted = chk ? true : false;
        $("input[name=cbKeepSorted]").prop("checked", isAlternativesSorted);

        $('#DSACanvas').sa("option", "SAAltsSortBy", (isAlternativesSorted ? 1 : 0));
        $('#DSACanvas').sa("sortAlternatives");
        $('#DSACanvas').sa("redrawSA");
    }

    function onShowLines(chk) {
        $('#DSACanvas').sa("option", "PSAShowLines", (chk));
        $('#DSACanvas').sa("redrawSA");
    }

    function onShowLegend(chk) {
        $('#DSACanvas').sa("option", "GSAShowLegend", (chk));
        $('#DSACanvas').sa("redrawSA");
    }

    function onLineUp(chk) {
        $('#DSACanvas').sa("option", "PSALineup", (chk));
        $('#DSACanvas').sa("redrawSA");
    }

    function resizePage() {
        var body = $('body');
        if ((body.width() > body.height()) && body.height() < 422) {
            if ($('.fnv-toggler').hasClass('up')) {
                $('.fnv-toggler').click();
            }
        }
        else {
            if ($('.fnv-toggler').hasClass('down')) {
                $('.fnv-toggler').click();
            }
        }
        initPage();
    }

    function initPage() {
        try {
            //scope = angular.element($("#anytime-page")).scope();
            //if (output.is_anytime) {
            //    scope = angular.element($("#anytime-page")).scope();
            //} else {
            //    scope = angular.element($("#TeamTimeDiv")).scope();
            //}

            SATypeString = Sense.GetSATypeString;
            var widget = $('#DSACanvas')[0];
            //var w = $(widget).parent()[0].clientWidth;
            $('#DSACanvas').width(100).height(100);
            var h = $('#divSA').height() - 2;
            var w = $('#divSA').width() - 2;
            if (w > 1400) w = 1400;
            if (w < 400) w = 300;
            if (h < 100) h = 100;
            if (w > 5 * h) w = 5 * h;

            var initialSortBy = SATypeString == "PSA" ? 1 : (isAlternativesSorted ? 1 : 0);
            debugger
            $('#DSACanvas').width(w).height(h);
            $("#DSACanvas").sa({
                viewMode: SATypeString,
                objs: data_objs,
                alts: data_alts,
                valDigits: data_decimals,
                SAAltsSortBy: initialSortBy,
                GSAShowLegend: true,
                normalizationMode: normalization_SA.value
            });
            //replace height and width temporarily
            x = $('#divSA').width();
            var headerNavHeight = $(".headings").height();
            if ($('body').width() < 1000) {

                var z = $(".footer-nav-mobile").position();
                if (headerNavHeight !== undefined) {
                var header = $('.tt-mobile-nav').height() + 30 + headerNavHeight + $(".sensitivity-caption").height() + $(".sensitivity-refresh").height();
                //alert(z.top);
                //alert((z.top - $('.tt-homepage').offset().top) - header);
                y = (z.top - $('.tt-homepage').offset().top) - header;
                //if(SATypeString == 'GSA')
                //    y = (z.top - $('.tt-homepage').offset().top) - (header + 52);
                }
            }
            else {
                var headerHeight = $(".headings").height();

                var footerHeight = $(".footer-pagination-wrap").height() + $(".footer-content").height();
                var bodyHeight = $("body").height();
                //var SAHeight = bodyHeight - (headerHeight + footerHeight + headerNavHeight);
                var SAHeight = 500;
                y = SAHeight - 110;

            }
            //y = 190;
            $('#DSACanvas').width($('#divSA').width());
            $('#DSACanvas').height(y);
            $('#DSACanvas').sa("resizeCanvas", x, y);
            $('#DSACanvas').sa("option", "ObjScrollShift", (1));

            var SAcanvas = $('#DSACanvas').sa("option", "canvas");
            var canvaswidth = SAcanvas.width / 2;
            var thisobjs = $('#DSACanvas').sa("option", "objs");
            var thisPSABarHeight = $('#DSACanvas').sa("option", "PSABarHeight");
            var areaWidth = thisobjs.length * thisPSABarHeight + 8;

            if (SATypeString == 'PSA') {
                if (canvaswidth < areaWidth) {
                    $('.lblSeeing').html("Drag a bar up or down or scroll left or right.");
                }
                else {
                    $('.lblSeeing').html("Drag a bar up or down");
                }
            }
            else {
                $('.lblSeeing').html('');
            }
        }
        catch (e) {

        }

    };
    var SATypeString = "DSA";
    var scope = [];
    try {
        var pnl_loading_id = '';

        var ACTION_DSA_UPDATE_VALUES = '';
        var ACTION_DSA_RESET = '';

        var data_objs = [];
        var data_alts = [];
        var data_decimals = [];

        var cmd_ok = "parseData(params)";
        var on_ok_code = "";
        var on_error_code = "";
    }
    catch (e) {

    }

    function initData() {
        //scope = angular.element($("#anytime-page")).scope();
        //if (output) {
        //    scope = angular.element($("#anytime-page")).scope();
        //} else {
        //    scope = angular.element($("#TeamTimeDiv")).scope();
        //}

        pnl_loading_id = Sense.pnlLoadingID;
        ACTION_DSA_UPDATE_VALUES = Sense.ACTION_DSA_UPDATE_VALUES;
        ACTION_DSA_RESET = Sense.ACTION_DSA_RESET;

        data_objs = eval(Sense.GetSAObjectives);
        data_alts = eval(Sense.GetSAAlternatives);
        data_decimals = eval(Sense.GetDecimalsValue);

        cmd_ok = "parseData(params)";
        on_ok_code = "";
        on_error_code = "";

        var body = $('body');
        if ((body.width() > body.height()) && body.height() < 422) {
            if ($('.fnv-toggler').hasClass('up')) {
                $('.fnv-toggler').click();
            }
        }
        else {
            if ($('.fnv-toggler').hasClass('down')) {
                $('.fnv-toggler').click();
            }
        }
    }
    window.onresize = resizePage;
//-->
</script>
