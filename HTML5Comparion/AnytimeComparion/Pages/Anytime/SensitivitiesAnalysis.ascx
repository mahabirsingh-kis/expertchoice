<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SensitivitiesAnalysis.ascx.cs" Inherits="AnytimeComparion.Pages.Anytime.SensitivitiesAnalysis" %>
<%@ Register Src="~/Pages/includes/QHicons.ascx" TagPrefix="includes" TagName="QHicons" %> 

<div class="large-12 columns tt-homepage anytime-sensitivities" <%--ng-if="Sense != null"--%> ng-click="SAFocusout()" style="padding:0px;margin:0px;">
    <div class="row tt-header-nav blue-header">
        <div class="large-12 columns text-center">
            <h1 class="tt-survey-title" ng-bind-html="getHtml(output.step_task)" style="color:white"></h1>
        </div>
    </div>
    
       <div ng-if="!show_apply_qh_cluster" class="large-1 medium-12 small-12 columns right hide-for-medium-down" id="QHIcons">
            <includes:QHicons runat="server" ID="QHicons" />
        </div>
    <div class="row collapse sensitivities-data-options">
        <div class="large-3 columns hide-for-medium-down">
            <div class="row collapse" ng-show="Sense.sensitivities.SAType == 1" >
                <div class="large-12 columns">
                    <span class="sv-label 2 left">Normalization: </span>
                     <select class="priorities-view-result sv-sorter left" ng-model="normalization_SA" ng-options="obj.text for obj in normalization_SA_list track by obj.value" ng-change="setSANotmalization(normalization_SA)">
                    </select>                    
                </div>
            </div>
            <div class="row collapse" ng-show="Sense.sensitivities.SAType != 1" >&nbsp;</div>
        </div>
        <div class="large-8 columns hide-for-medium-down sensitivity-caption left">
            <div class="row">
                <div class="large-12 columns">
                    <span class="left" ng-show="Sense.GetNodesList != ''" ng-bind-html="getHtml(Sense.GetNodesList)"></span>
                    <button type="button" class="large-2 button small left tt-button-primary tt-h-btn btnRefresh hide-for-medium-down radius" id="btnRefresh" onclick="onReset(); return false;">
                        <span class="text">{{Sense.lblRefreshCaption}}</span>
                    </button>
                    <div class="large-4 columns collapsesensitivity-label-wrap text-center hide-for-medium-down" ng-bind-html="getHtml(Sense.GetOptions)"></div>
                </div>
            </div>
        </div>    
    </div>
    
    <div id='tblSA' class="small-12 columns" style="margin:0px;padding:0px;" >
        <div ng-show="Sense.Opt_ShowYouAreSeeing">
<%--            <div valign="middle" align="center" class="text columns large-centered text-center"style="color:#006666">
                <b><span ID="lblSeeing" class="lblSeeing" style="font-size:12px"></span></b>
                {{output.step_task}}
            </div>--%>
        </div>
        <!-- Mobile QH ans Sensitivities -->
        <div class="row hide-for-large-up">
            <div class="small-12 columns">
                <!-- Mobile Sensitivity Refresh and Select Objective -->
                <div class="row sensitivity-drop-row ">                        
                        <a data-options="align:left" data-dropdown="drop001" aria-controls="drop2" aria-expanded="false" class="button tt-button-primary right"><span class="icon-tt-chevron-down"></span></a>                      
                        <a class="button tt-button-primary the-refresh-btn right" id="btnRefresh" onclick="onReset(); return false;">
                                    <span>{{Sense.lblRefreshCaption}}</span>
                        </a> 
                        <div ng-if="!show_apply_qh_cluster" id="QHIcons" class="right" style="margin-right:5px;"><includes:QHicons runat="server" ID="QHicons1" /></div>                        
                        <div id="drop001" data-dropdown-content class="f-dropdown medium" aria-hidden="true" aria-autoclose="false" tabindex="-1">
                            <div class="small-12 columns" data-equalizer-watch>
                                <span ng-show="Sense.GetNodesList != ''" ng-bind-html="getHtml(Sense.GetNodesList)"></span>
                            </div>                    
                            <div class="columns large-12">
                                 <label ng-show="Sense.sensitivities.SAType == 1" >
                                    <span class="sv-label left">Normalization: </span>
                                     <select class="priorities-view-result sv-sorter large-12 columns" ng-model="normalization_SA" ng-options="obj.text for obj in normalization_SA_list track by obj.value" ng-change="setSANotmalization(normalization_SA)">
                                                            </select>
                                 </label>
                            </div> 
                            <div class="large-12 columns sensitivity-label-wrap" ng-bind-html="getHtml(Sense.GetOptions)"></div>
                            <div class="small-12 columns text-center hide-for-large-up" ng-show="$parent.isMobile()"> 
                            </div>
                        </div>                        
                 </div>      
                <!-- End Mobile Sensitivity Refresh and Select Objective --> 
            </div>
            <div class="columns">                
            </div>
        </div>           
        <!-- Mobile QH ans Sensitivities -->

        <div style="margin:0px;padding:0px;">
            <div class="text"  ng-click="SAFocusin()" style="margin:0px;padding:0px;">
                <%--<span ID="lblMessage" style="height:50px" Font-Bold="true">{{Sense.lblMessage}}</span>--%>
                <div id="divSA" class="whole large-9 columns large-centered" style="margin:0px;padding:0px;">
                    <canvas id="DSACanvas" class="whole" style="margin:0px;padding:0px;">Your browser doesn't support HTML5</canvas>
                </div>
            </div>
        </div>

        <div ng-if="Sense.GetSATypeString == 'GSA' && !$parent.isMobile()" class="large-centered small-right small-3 columns text-centered">
                 <select id="selSubobjectives" class="sensitivity-select select" style="margin-top:5px" ng-model="saobj" ng-init="saobj = Sense.GetGSASubobjectives[0] " ng-options="obj[2] for obj in Sense.GetGSASubobjectives track by obj[0]" ng-change="onChangeSubobjective(saobj[0])" >
                </select>
         </div>

 
    </div>
</div>

<%--<script src="/Scripts/SA/SA.js"></script>--%>
<script src="../../Scripts/SA/ec.sa.js"></script>
<script src="/Scripts/SA/drawMisc.js"></script>
<script src="/Scripts/SA/misc.js" ></script>

<script type="text/javascript" ng-if="Sense != null" ng-cloak><!--
    /* jQuery Ajax */
    function SyncReceived(params) {

        //if (pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 0); $("#tblSA").removeAttr('disabled'); }
        if (pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 0); }
        console.log(0);
        if (on_ok_code != "")
            eval(on_ok_code);
        on_ok_code = "";
        if (params != "") {
            console.log(2);
            var received_data = JSON.parse(params);
            received_data = eval(received_data.d);
            if ((received_data)) {
                $("#DSACanvas").sa("GradientMinValues", received_data[0]);
            };
        };
    }

    function sendCommand(params, showPleaseWait) {
         console.log(params + "&r=" + Math.random());
        if (showPleaseWait && pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 1); }
        $.ajax({
            type: "POST",
            data: JSON.stringify({
                data : params + "&r=" + Math.random()
            }),
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            async: false,
            url: baseUrl + "pages/Anytime/Anytime.aspx/Ajax_Callback",
            success: function (data) {
                SyncReceived(data);
            },
            error: function (response) {
                console.log(response);
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
        try{
            //scope = angular.element($("#anytime-page")).scope();
            if (is_anytime) {
                scope = angular.element($("#anytime-page")).scope();
            } else {
                scope = angular.element($("#TeamTimeDiv")).scope();
            }
            
            SATypeString = scope.Sense.GetSATypeString;
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

            $('#DSACanvas').width(w).height(h);
            $("#DSACanvas").sa({
                viewMode: SATypeString,
                objs: data_objs,
                alts: data_alts,
                valDigits: data_decimals,
                SAAltsSortBy: initialSortBy,
                GSAShowLegend: true,
                normalizationMode: scope.normalization_SA.value
            });
            //replace height and width temporarily
            x = $('#divSA').width();
            var headerNavHeight = $(".tt-header-nav.blue-header").height();
            if ($('body').width() < 1000) {
                
                var z = $(".footer-nav-mobile").position();
                var header = $('.tt-mobile-nav').height() + 30 + headerNavHeight + $(".sensitivity-caption").height() + $(".sensitivity-refresh").height();
                //alert(z.top);
                //alert((z.top - $('.tt-homepage').offset().top) - header);
                y = (z.top - $('.tt-homepage').offset().top) - header;
                //if(SATypeString == 'GSA')
                //    y = (z.top - $('.tt-homepage').offset().top) - (header + 52);

            }
            else {
                var headerHeight = $(".tt-header").height();
                
                var footerHeight = $(".footer-pagination-wrap").height() + $(".footer-content").height();
                var bodyHeight = $("body").height();
                var SAHeight = bodyHeight - (headerHeight + footerHeight + headerNavHeight);
                y = SAHeight - 110;
                
            }
            //y = 190;
            $('#DSACanvas').width(x);
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
            else{
                $('.lblSeeing').html('');
            }
        }
        catch (e) {

        }

    };
    var SATypeString = "DSA";
    var scope = [];
    try{
        var pnl_loading_id = '';

        var ACTION_DSA_UPDATE_VALUES = '';
        var ACTION_DSA_RESET = '';

        var data_objs = [];
        var data_alts = [];
        var data_decimals = [];

        var cmd_ok = "parseData(params)";
        var on_ok_code = "";
        var on_error_code = "";
    } catch (e) {

    }

    function initData() {
        //scope = angular.element($("#anytime-page")).scope();
        if (is_anytime) {
            scope = angular.element($("#anytime-page")).scope();
        } else {
            scope = angular.element($("#TeamTimeDiv")).scope();
        }

        pnl_loading_id = scope.Sense.pnlLoadingID;

        ACTION_DSA_UPDATE_VALUES = scope.Sense.ACTION_DSA_UPDATE_VALUES;
        ACTION_DSA_RESET = scope.Sense.ACTION_DSA_RESET;

        data_objs = eval(scope.Sense.GetSAObjectives);
        data_alts = eval(scope.Sense.GetSAAlternatives);
        data_decimals = eval(scope.Sense.GetDecimalsValue);

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

 

<style type="text/css" >
/* This only applies to this specific page. Do not transfer this to main app.css, it will ruin the whole site */
    html {
        overflow: hidden;

    }
    body {
        overflow: hidden;

    }
        
</style>




                                    

