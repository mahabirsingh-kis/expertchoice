<%@ Page Language="VB" Inherits="RATimelinePage" title="Projects Timeline" Codebehind="Timeline.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/drawMisc.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/RA.js"></script>
<script>
    var projects = <%= GetRAProjects() %>;
    var solverState = '<%= GetSolverState() %>';
    var CurrencyThousandSeparator = "<% =UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    /* jQuery Ajax */
    function syncReceived(params) {
        if ((params)) {            
            var received_data = eval(params);
            if ((received_data)) {
                if (received_data == 'ok-syncprojs'){

                };
                if ((received_data[0] == 'solved')||(received_data[0] == 'infeasible')){
                    if (received_data[0] == 'infeasible') {
                        $("#divInfeasible").show();
                        solverState = 'infeasible';
                    }else{
                        $("#divInfeasible").hide();
                    };
                    var sResults = received_data[1];
                    $("#RATimeline").timeline("option", "solvedResults", sResults);
                    $("#RATimeline").timeline("updateSolvedResults");
                    if (received_data[0] == 'infeasible') {$("#RATimeline").timeline("setInfeasible");};
                    $("#RATimeline").timeline("resize");
                };
                if (received_data[0] == 'resolved'){
                    projects = received_data[1];
                    var pres = received_data[2];
                    var solvedRes = received_data[3];
                    $("#RATimeline").timeline("option", "projs", projects);
                    $("#RATimeline").timeline("option", "portfolioResources", pres);
                    $("#RATimeline").timeline("option", "solvedResults", solvedRes);
                    $("#RATimeline").timeline("updateSolvedResults");
                    $("#RATimeline").timeline("resize");
                };
                if (received_data[0] == 'paste'){
                    projects = received_data[1];
                    $("#RATimeline").timeline("option", "projs", projects);
                    $("#RATimeline").timeline("updateSolvedResults");
                    $("#RATimeline").timeline("resize");
                };
            }
        }        
    }

    function OpenRichEditor(cmd)
    {
        CreatePopup('<% =PageURL(_PGID_RICHEDITOR) %>' + cmd, 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);
    }

    function onRichEditorRefresh(empty, infodoc, callback_param)
    {        
        $("#img_name_" + callback_param[0]).prop("src", "<% =JS_SafeString(ImagePath) %>" + ((empty*1) ?  "info12_dis.png" : "info12.png"));
        window.focus();
    }

    function pasteColumn(colID) {
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteData(colID, data);
        } else {
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome('" + colID + "');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
        };   
    }

    function getProjectByIdx(idx) {
        for (i = 0; i < projects.length; i++) {
            var prj = projects[i];
            if (prj.idx == idx) return prj;
        };
        return null;
    }

    function copyRow(projectIdx) {
        var data = '';
        var prj = getProjectByIdx(projectIdx);
        var resID = $("#RATimeline").timeline("option", "resID");
        if (prj !== null && typeof resID !== "undefined" && resID !== "") {
            //data += prj.resources[resID].totalValue;
            var periodsCount = $("#RATimeline").timeline("option", "periodsCount");
            for (i = 0; i < periodsCount; i++) {
                var value = prj.resources[resID].values[i - prj.periodStart];
                if ((typeof (value) == "undefined") || (value == -2147483648)) { value = ' ' };
                data += (data == "" ? "" : "\t") + value;
            };
            copyDataToClipboard(data);
        }
    }

    function pasteRow(projectIdx) {
        var data = "";
        var pastedata = '';
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text');
            if (typeof data != "undefined" && data != "") {
                $("#RATimeline").timeline("pasterow", data, projectIdx);
            } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
        } else {
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChromeTimelineRow('" + projectIdx + "');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
        };
    }

    function commitPasteChromeTimelineRow(projectIdx) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            var data = pasteBox.value;
            if (typeof data != "undefined" && data != "") {
                //pasteperioddata = data.replace(/(?:\r\n|\r|\n)/g, '\n');
                $("#RATimeline").timeline("pasterow", data, projectIdx);
            } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
            }
        }

    function pasteData(colID, data) {
        if (typeof data != "undefined" && data != "") {
            sendCommand("action=paste_column&column="+colID+"&data="+encodeURIComponent(data));
        } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
    }

    function commitPasteChrome(colID) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(colID, pasteBox.value);
        }
    }

    function syncError() {
        DevExpress.ui.notify("<%=ResString("ErrorMsg_ServiceOperation") %>", "error");
    }

    function sendCommand(params) {
        cmd = params;

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }
    
    var on_hit_enter = "";

    function Hotkeys(event)
    {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event)
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            if (code == 13) {
                if (on_hit_enter=="") {
                    return true;
                } else {
                    eval(on_hit_enter);
                }
                return false;
            };
            if (code == 27 && prj_name!="") {
                $("#tbName" + prj_name).val(alt_value);
                switchPrjNameEditor(prj_name, -1);
                return false;
            }
        }
    }

    $(document).ready(function() {
        document.onkeypress = Hotkeys;
        $("#lblNoDataToShow").hide();
        if (solverState == 'infeasible') {$("#divInfeasible").show();}else{$("#divInfeasible").hide();};
        setTimeout('initPage();', 100);
        $( window ).resize(function() {
            setTimeout('resizeView();', 100);
        });
    });

    function onSetScenario(id) { //A1142
        document.location.href = '<% =PageURL(CurrentPageID, "action=scenario" + GetTempThemeURI(True))%>&sid=' + id;
        return false;
    }

    function initPage(){
        if (projects.length > 0) {
            var resources = <%= GetResourcesList() %>;
            var distribMode = <%= GetTimeperiodsDistribMode() %>;
            input = $('input:radio[name=DistribMode]');
            input.filter('[value=' + distribMode + ']').prop('checked', true);
            input.change(function () {
                var rVal = $(this).val();
                $("#RATimeline").timeline("option", "distribMode", rVal);
                sendCommand('action=tpdistribmode&mode=' + rVal);
                $("#RATimeline").timeline("resize");
            });

            var startDate = '<%= GetTimelineStartDate()%>';
            var dateParts = startDate.split('/');
            var startYear = parseInt(dateParts[2]);
            var viewMode = '<%= GetRAViewMode() %>';

            var dateYearSel = $("#tbDateYear");
            dateYearSel.find('option').remove();
            for (var i = -15; i < 15; i++) {
                dateYearSel.append('<option value="'+dateParts[0]+'/'+dateParts[1]+'/'+(startYear+i)+'">'+(startYear+i)+'</option>') 
            };
            dateYearSel.val(startDate);

            $("#RATimeline").timeline({
                projs: projects,
                periodsCount: <%= GetTimePeriodsCount() %>,
                resources: resources,
                imgPath: "<% =ImagePath %>",
                imgInfodoc: "info12.png",
                imgNoInfodoc: "info12_dis.png",
                viewMode: viewMode,
                distribMode: distribMode,
                selectedResourceID: "<%= TP_RES_ID %>",
                periodNamingMode: <%= GetTimePeriodsType() %>,
                periodNamePrefix: '<%= GetTimePeriodNameFormat %>',
                startDate: startDate,
                portfolioResources: <%= GetRAPortfolioResources() %>,
                solvedResults: <%= GetPeriodResults() %>,
                index_visible: <%=CStr(IIf(RA.Scenarios.GlobalSettings.IsIndexColumnVisible, "true", "false"))%>,
                fundedOnly: <%= RA_ShowFundedOnly() %>,
                titleProject: "<% =JS_SafeString(ResString("tblAlternativeName")) %>"
                });
            if (viewMode == 'results') {
                //$('#divSelectResource').show();
                $('#tdDistribute').show();
                $('#tdAutoExpand').hide();
            } else {
                //$('#divSelectResource').hide();
                $('#tdDistribute').hide();
                $('#tdAutoExpand').show();
            };
            if (solverState == 'infeasible') {$("#RATimeline").timeline("setInfeasible"); $("#RATimeline").timeline("resize");};
            dialog = $( "#dialog-resources" ).dialog({
                title: 'Edit Resources',
                autoOpen: false,
                modal: true,
                width: 350,
                height: 250,
                buttons: {
                    Ok: function() {
                        dialog.dialog( "close" );
                        location.reload();
                    }
                },
                close: function() {
                    location.reload();
                }
            });
            $( "#tbDate" ).datepicker({
                changeMonth: true,
                changeYear: true,
                showButtonPanel: true
            });
            var tpType = <%= GetTimePeriodsType() %>;
            $("#viewTPMode").prop("value", tpType);
            if ((tpType == 5)||(tpType == 4)) {
                //$("#tdPrefix").show();
                $("#tdDate").hide();
                if (tpType == 4) {$("#tdDateYear").show();};
            }else{
                $("#tdDateYear").hide();
                $("#tdPrefix").hide();
                $("#tdDate").show();
            };
            var tpStep = <%= GetTimePeriodsStep() %>;
            $("#tbStep").prop("value", tpStep).focus().blur();
            var tpFormat = '<%= GetTimePeriodNameFormat() %>';
            var tpDiscount = <%= GetTimePeriodDiscount() %>;
            var tpUseDiscount = <%= GetTimePeriodDiscountChecked() %>;
            $("#tbDiscount").prop("value", tpDiscount);
            $("#cbCheckbox").prop("checked", tpUseDiscount);
            $("#tbPrefix").prop("value", tpFormat);
            $("#tbDate").prop("value", startDate);
            $("input:text").focus(function() { $(this).select(); } );
            if (viewMode == 'resources') {
                $('#tdDate').hide();
                $('#tdPrefix').hide();
                $('#tdPFormat').hide();
                $('#tdPStep').hide();
            };
            $("#lblNoDataToShow").hide();
        }else{
            $("#lblNoDataToShow").show();
            $("#mainView").hide();
        };
    }
    function enableEditButtons(isEnabled){
        if (isEnabled) {
            $(".edit_btn").prop('disabled', false);
        }else{
            $(".edit_btn").prop('disabled', true);
        };
    }

    function resizeView(){
        if (projects.length > 0) $("#RATimeline").timeline("resize");
    }

    function expandProjects(){
        $("#RATimeline").timeline("expandProjects");
        sendCommand('action=expandProjects');
    }

    function contractProjects(){
        $("#RATimeline").timeline("contractProjects");
        sendCommand('action=contractProjects');
    }

    function trimTimeline(){
        $("#RATimeline").timeline("trimTimeline");
        sendCommand('action=trimTimeline');
    }

    function showResults(mode){
        var viewMode = mode;
        $("#RATimeline").timeline("option", "viewMode", viewMode);
        $("#RATimeline").timeline("resize");
        switch(mode){
            case 'edit':
            case 'portfolio':
                $('#divSelectResource').hide();
                break;
            case 'results':
                sendCommand('action=solve');
                break;
            default:
                $('#divSelectResource').show();
        };
    };

    function changeTPMode(tpType){
        if ((tpType == 5)||(tpType == 4)) {
            //$("#tdPrefix").show();
            $("#tdDate").hide();
            if (tpType == 4) {$("#tdDateYear").show();};
        }else{
            $("#tdDateYear").hide();
            $("#tdPrefix").hide();
            $("#tdDate").show();
        };
        $("#RATimeline").timeline("option", "periodNamingMode", Number(tpType));
        $("#RATimeline").timeline("resize");
        sendCommand('action=tpnamingmode&mode=' + tpType);
    };

    function changeStartDate(sDate){
        $("#RATimeline").timeline("option", "startDate", sDate);
        $("#RATimeline").timeline("resize");
        sendCommand('action=tpstartdate&startdate=' + sDate);
    };

    function changeStartYear(sDate){
        $("#tbDate").val(sDate);
        $("#RATimeline").timeline("option", "startDate", sDate);
        $("#RATimeline").timeline("resize");
        sendCommand('action=tpstartdate&startdate=' + sDate);
    };

    function changeDiscount(sVal){
        sendCommand('action=tpdiscount&discount=' + sVal);
    };

    function changeUseDiscount(sVal){
        sendCommand('action=tpusediscount&usediscount=' + sVal);
    };

    function changePeriodStep(step){
        $("#RATimeline").timeline("option", "periodStep", Number(step));
        $("#RATimeline").timeline("resize");
        sendCommand('action=tpstep&step=' + step);
    };

    function showOptionsDialog() {
        dialog.dialog("open");
    };

    function distributeResources(){
        $("#RATimeline").timeline("distributeAll");
    };

    function solveTimeperiods(){
        sendCommand('action=solve');
    };

    function enableTP() {
        document.location.href='<% =PageURL(CurrentPageID, "action=enable_tp" + GetTempThemeURI(True))%>';
        return false;
    }

    function saveTPAltName(id, val) {
        var w = 150;
        var alt_id = id;
        if (val!="" && alt_id!="" ) {
            var v = (val.length>w ? ShortString(val, w) :  val);
            $("#name"+id).html(replaceString("&amp;hellip", "&hellip", htmlEscape(v)));
            var cell = $("#name"+id).parent();
            if ((cell)) {
                cell.attr("clip_data", v);
            }
            on_ok_code = "filterGrid(search_old); UpdateSLData();";
            sendCommand("action=save_altname&id=" + alt_id + "&val=" + encodeURIComponent(val));
        }
    }

</script>
    <div id="mainView" style="width:100%;height:100%;text-align:left;vertical-align:top">
    <div style="width:100%;height:100%;text-align:left;vertical-align:top;">      
        <div id="divToolbar" class="text ec_toolbar ra_fixed" style="background-color:white;margin:0px;width:100%;">
        <table border="0" cellpadding="0" cellspacing="0" class="text">
            <tr>
                <td class="ec_toolbar_td_separator">
                    <input id="btnSolve" type="button" value="Solve" onclick="solveTimeperiods();" />&nbsp;&nbsp;
                </td>
                <td class="ec_toolbar_td_separator">
                    <label for='cbScenarios'><%=ResString("lblScenario") + ":"%></label><br/>
                    <% =GetScenarios%>
                </td>
                <td class="ec_toolbar_td" id="tdPFormat">
                    <label for="viewTPMode">Period format:</label><br />
                    <select id="viewTPMode" style="height:21px;" onchange="changeTPMode(this.value);">        
                        <option value="0">Days</option>
                        <option value="1">Weeks</option>
                        <option value="2">Months</option>
                        <option value="3">Quarters</option>
                        <option value="4">Years</option>
                        <option value="5">Period</option>
                    </select>
                </td>
                <td class="ec_toolbar_td" id="tdPStep">
                    <nobr><label for="tbStep">Period step:</label></nobr><br />
                    <input id="tbStep" type="text" value="1" style="width:60px" onchange="changePeriodStep(this.value);" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'/>
                </td>
                <td id="tdPrefix" class="ec_toolbar_td" style="visibility:collapse">
                    <label for="tbPrefix">Custom format:</label><br />
                    <input id="tbPrefix" type="text" value="Period #" style="width:80px" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'/>
                </td>
                <td id="tdDate" class="ec_toolbar_td_separator">
                    <label for="tbDate">Start date:</label><br />
                    <input id="tbDate" type="text" value="" onchange="changeStartDate(this.value);" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'/>
                </td>
                <td id="tdDateYear" class="ec_toolbar_td_separator">
                    <label for="tbDateYear">Start year:</label><br />
                    <select id="tbDateYear" onchange="changeStartYear(this.value);">
                    </select>
                </td>
<%--                <td class="ec_toolbar_td_separator" id="divSelectResource">
                        <span>Resource Constraint:</span><br />  
                        <select id="resSelect" onchange="changeResource(this.value);">
                        </select>
                        <%--<input type="button" value="Edit Resources" onclick="showOptionsDialog();" />
                </td>--%>
                <td class="ec_toolbar_td_separator" id="tdDiscount">
                    <span style="text-align:left;"><input type="checkbox" id="cbCheckbox" onchange="changeUseDiscount(this.checked);" style="margin-top:-2px; margin-left:0px;" /> Use discount factor:</span><br />  
                    <input id="tbDiscount" style="width:90px" type="text" value="0" onchange="changeDiscount(this.value);" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'/>
                </td>
                <td class="ec_toolbar_td" id="tdDistribute">
                    <nobr><input type="radio" name="DistribMode" value="1" id="distribNext" /><label for="distribNext">Distribute resource value to SUBSEQUENT periods</label></nobr><br />
                    <nobr><input type="radio" name="DistribMode" value="0" id="distribAll" /><label for="distribAll">Distribute resource value to ALL periods</label></nobr>
                </td>
                <td class="ec_toolbar_td" id="tdAutoExpand">
                    <nobr><input id="btnExpandProjects" class="edit_btn" runat="server" type="button" value="Expand Projects" onclick="expandProjects();" />
                    <input id="btnContractProjects" class="edit_btn" runat="server" type="button" value="Contract Projects" onclick="contractProjects();" />
                    <input id="btnTrimTimeline" class="edit_btn" runat="server" type="button" value="Trim Timeline" onclick="trimTimeline();" /></nobr>
                </td>
            </tr>
        </table>
        </div>
        <div style="height:60px;"></div>
        <% =GetMessage %>
        <div id="divInfeasible" style="width:100%;text-align:center;display:none;"><label id="lblError" class="error">Infeasible</label></div>
        <div id="RATimeline"></div>
        </div>
        </div>
        <label id="lblNoDataToShow" class="error">No Data to Show on this Page, please <a href='<% =GetLink(_PGID_STRUCTURE_ALTERNATIVES)%>'>add</a> some Alternatives first</label>
</asp:Content>