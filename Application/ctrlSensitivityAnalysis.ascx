<%@ Control Language="vb" AutoEventWireup="true" Inherits="ExpertChoice.Web.Controls.ctrlSensitivityAnalysis" Codebehind="ctrlSensitivityAnalysis.ascx.vb" %>
<!--[if lt IE 9]><script type="text/javascript" src="<% =_URL_ROOT %>Scripts/excanvas.min.js"></script><script type="text/javascript">var is_excanvas = true;</script><![endif]-->
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/misc.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/drawMisc.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/ec.sa.js"></script>
<script type="text/javascript"><!--

    var pnl_loading_id = "<% =pnlLoadingID %>";

    var ACTION_DSA_UPDATE_VALUES = "<%=ACTION_DSA_UPDATE_VALUES%>";
    var ACTION_DSA_RESET = "<%=ACTION_DSA_RESET%>";
    var ACTION_SA_WRT_NODE_ID = "<%=ACTION_SA_WRT_NODE_ID%>";

    var data_objs = <%=GetSAObjectives() %>;
    var data_alts = <%=GetSAAlternatives() %>;
    var data_decimals = <%=GetDecimalsValue() %>;
    var data_viewMode = <%=GetSATypeString() %>;

    var cmd_ok = "parseData(params)";
    var cmd = "";
    
    var on_ok_code = "";
    var on_error_code = "";

    /* jQuery Ajax */
    function SyncReceived(params) {
        if (pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 0); $("#tblSA").removeAttr('disabled'); }
        if (on_ok_code!="") eval(on_ok_code);
        on_ok_code = "";
        if ((params) && (IsAction(ACTION_DSA_UPDATE_VALUES, cmd))) {
            var received_data = eval(params);
            if ((received_data)) {             
                $("#DSACanvas").sa("GradientMinValues", received_data[0]);       
            };
        };
        if ((params) && (IsAction(ACTION_SA_WRT_NODE_ID, cmd))) {
            var received_data = eval(params);
            if ((received_data)) {
                data_objs = received_data[0];
                data_alts = received_data[1];
                $('#DSACanvas').sa("option", "objs", data_objs); 
                $('#DSACanvas').sa("option", "alts", data_alts);
                $('#DSACanvas').sa("resetSA");         
            };
        };
    }

    function sendCommand(params, showPleaseWait) {
        cmd = params;
        if (showPleaseWait && pnl_loading_id != "") { SwitchWarning(pnl_loading_id, 1); $("#tblSA").attr('disabled', 'disabled'); }

        $.ajax({
            type: "POST",
            data: params + "&r=" + Math.random(),
            dataType: "text",
            async: true,
            success: function (data) {
                SyncReceived(data);
            },
            error: function () {
                if (pnl_loading_id != "") SwitchWarning(pnl_loading_id, 0);
                if (on_error_code!="") eval(on_error_code);
                on_error_code = "";
                dxDialog("Erron on perform callback. Please reload page.", true, ";", "undefined", "Error", 350, 280);
            }
        });
    }

    function parseData(params) {
        if ((params) && params!='') { 
            var data = eval(params); 
            if (data.length==2) { 
                $('#selSubobjectives').find('option').remove().end();
                data_objs = data[0]; 
                data_alts = data[1]; 
                for (var i = 0; i < data_objs.length; i++) {
                    var obj = data_objs[i];
                    $('#selSubobjectives').append('<option value="'+i+'">'+obj.name+'</option>');
                };
                $('#DSACanvas').sa("option", "objs", data_objs); 
                $('#DSACanvas').sa("option", "alts", data_alts); 
                $('#DSACanvas').sa("resetSA");
                
            }
        }
    }

    function onChangeNode(id) {
        on_ok_code = cmd_ok;
        sendCommand("action="+ ACTION_SA_WRT_NODE_ID + "&node_id=" + id, true);
    }

    function onChangeSubobjective(id){
        $('#DSACanvas').sa("option", "SelectedObjectiveIndex", Number(id));
        $('#DSACanvas').sa("redrawSA");
    }

    function getNormModeSA(id) {
        var normMode = 'unnormalized';
        switch (id){
            case '0':
                normMode = 'normAll';
                break;
            case '1':
                normMode = 'normMax100';
                break;
            default:
                normMode = 'unnormalized';
        };
        return normMode;
    }

    function onChangeNormalization(id) {
        //on_ok_code = cmd_ok;
        //sendCommand("action=normalization&norm_mode=" + id, true);
        $('#DSACanvas').sa("option", "normalizationMode", getNormModeSA(id)); 
        $('#DSACanvas').sa("redrawSA");
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

    function onKeepSorted(chk) {
        $('#DSACanvas').sa("option", "DSASorting", (chk));
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
        initPage();
    }

    function initPage() {
        var widget = $('#DSACanvas')[0];
        //var w = $(widget).parent()[0].clientWidth;
        $('#DSACanvas').width(100).height(100);
        var h = $('#divSA').height()-2;
        var w = $('#divSA').width()-2;
        if (w>1400) w = 1400;
        if (w<400) w = 300;
        if (h<100) h = 100;
        if (w>5*h) w = 5*h;
        $('#DSACanvas').width(w).height(h);
        $("#DSACanvas").sa({
            viewMode: data_viewMode,
            objs: data_objs,
            alts: data_alts,
            valDigits: data_decimals,
            SAAltsSortBy: 1,
            GSAShowLegend: true,
            normalizationMode: 'normAll'
        });
        $('#DSACanvas').sa("resizeCanvas", w, h);
    }

    <% If CanSeeResults Then%>$(document).ready(function () { setTimeout('initPage();', 10); });
    window.onresize = resizePage;<% End If %>

//-->
</script>
<table style="height:98%; width:90%; margin: 0px auto;" border='0' cellpadding='0' id='tblSA'>
    <tr style="height:100%">
        <td valign="top" align="left" width="150px" style="padding-right:1ex;">
            <% If CanSeeResults Then%>    
                <div><% = GetNodesList() %></div>
                <div><% = GetOptions()%></div><br />
                <div><input type='button' id="btnRefresh" class="button" value="<% =lblRefreshCaption %>" onclick="onReset(); return false;" style="height:21px;width:150px;" /></div>
                <% If GetSATypeString() = "'GSA'" Then%>
                    <br />      
                    <div><select id="selSubobjectives" class="select" style="width:150px;" onchange="onChangeSubobjective(this.value);">
                            <%= GetGSASubobjectives() %>
                        </select>
                    </div>
                <%End If %>
            <% End If%>
        </td>
        <td style="height:100%"><% If CanSeeResults Then%>
            <div id="divSA" class="whole"><canvas id="DSACanvas" class="whole">Your browser doesn't support HTML5</canvas></div>
        <% Else%><asp:Label runat="server" ID="lblMessage" CssClass="h6" Visible="false"/><% End If %> 
        </td>
    </tr>
</table>