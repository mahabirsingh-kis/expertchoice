<%@ Page Language="VB" Inherits="RABasePage" title="Resource Aligner" Codebehind="Default.aspx.vb" %>
<%@ Import Namespace="RASolverPriority" %>
<%@ Import Namespace="Canvas.RAGlobalSettings" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/gridviewScroll.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/drawMisc.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/RA.js"></script>
<script type="text/javascript">    

    var projects = <%= GetRAProjects() %>;

    var show_combined_grp = true;

    var on_error_code = "";
    var on_ok_code = "";
    var val_old = "";
    var is_ajax = 0;
    var last_submit = 0;
    var is_solve = 0;
    var last_focus = "";
    var last_cb = ["", ""];
    var use_jquery_ajax = <% =Bool2Num(UseAjax) %>;
    var submits_checked = <% =Bool2Num(Not String.IsNullOrEmpty(SessVar(SESS_USE_AJAX)))%>;
    var view_grid = <% =Bool2Num(Not RA_ShowTimeperiods) %>;
    var fundedonly = <%= Bool2JS(RA_ShowFundedOnly) %>;
    var cents =  <% = Cents %>;

    var max_submit_delay = 30000;
    var mh = 500;   // maxHeight for dialogs;

    var dlg_scenarios;
    var dlg_columns;
    var dlg_solver;
    var dlg_solverprty;
    var dlg_logs;
    var dlg_models;
    var dlg_infeas;

    var solver_id_xa = <% =CInt(raSolverLibrary.raXA)%>;
    <%--var solver_id_msf = <% =CInt(raSolverLibrary.raMSF)%>;--%>
    var solver_id_gurobi = <% =CInt(raSolverLibrary.raGurobi)%>;
    var solver_names = ['<% =JS_SafeString(ResString("lblRASolverMSF"))%>', '<% =JS_SafeString(ResString("lblRASolverXA"))%>', '<% =JS_SafeString(ResString("lblRASolverGurobi"))%>', '<% =JS_SafeString(ResString("lblRASolverBaron"))%>'];

    var solver_id = <% =CInt(RA_Solver)%>;
    var solver_xa_strategy = <% =RA_XAStrategy %>;
    var solver_xa_variance = <% =GetRA_XAVariationIdx()%>;
    var solver_xa_timeout = <% =RA_XATimeout%>;
    var solver_xa_timeout_unchanged = <% =RA_XATimeoutUnchanged%>;

    var solver_xa_conf = "<% = JS_SafeString(GetCookie(COOKIE_ASK_GUROBI_CLOUD, ""))%>";

    var solver_timeouts = [10,15,20,30,45,60,90,120,180,240,300];
    var solver_timeouts_unchanged = [1,3,5,10,15,20,30,45,60,90,120];

    var scenarios = [<% =GetScenarios() %>];
    var scenario_active = <% =RA.Scenarios.ActiveScenarioID %>;

    var solver_priorities = [<% =GetSolverPriorities() %>];

    var alts_all = [<% =GetAllAlternatives() %>];

    var c_groups = [<% =GetCombinedGroups() %>];

    var master = [<% =GetMasterProjects() %>];

    var use_scrolling = <% =IIf(RA.Scenarios.GlobalSettings.ShowFrozenHeaders, "true", "false") %>;

    var associated_risk = <% =App.ActiveProject.PipeParameters.AssociatedModelIntID %>;
    var associated_name = "<% =JS_SafeString(AssociatedRiskModelName) %>";

    var on_hit_enter = "";

    var url_open_risk = "<% =URLProjectID(PageURL(_DEF_PGID_ONSELECTPROJECT), -999999999) %>";
    var def_page = 20101;   // define alts
    var def_sl_url = "<% =_URL_ROOT %>Comparion.aspx?<% =_PARAM_PROJECT %>={0}#/Structure/DefineObjectives";

    var startup_msg = "<% = JS_SafeString(StartupMessage) %>";
    var search_old = "";

    var cur_page = <% =RA_Pages_CurPage %>;

    var scroll_l = -1;
    var scroll_t = -1;

    var OPT_SCENARIO_LEN     = 45; //A0965
    var OPT_DESCRIPTION_LEN = 200; //A0965
    var OPT_GROUP_DETAILS = <% = Bool2JS(RA_ShowGroupDetails) %>;

    var btns_lst = [];
    var tp_lst = [];

    function enc(sVal) {
        return encodeURIComponent(sVal);
    }

    function onGetReportTitle() {
        return ("<% = JS_SafeString(PageTitle(CurrentPageID)) %>");
    }

    function onGetReport(addReportItem) {
        if (typeof addReportItem == "function") {
            addReportItem({"name": onGetReportTitle(), "type": "<% =JS_SafeString(ecReportItemType.PortfolioGrid) %>", "edit": document.location.href, "ContentOptions": {}});
        }
    }

    function getToolbarButton(btn_id) {
        <% If Scenario.IsInfeasibilityAnalysis Then %>if (btn_id!="solve" && btn_id!="timeperiods") return false;<% End if %>
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var el = toolbar.findItemByValue(btn_id);
            if ((el)) return el;
            return toolbar.findItemByText(btn_id);
        }
        return undefined;
    }

    function isAutoSolve() {
        var as = getToolbarButton("autosolve");
        return ((as) && as.get_isEnabled() && as.get_isChecked());
    }

    function solverStateID() {
        var s = theForm.solverState;
        if ((s)) return s.value; else return -1;
    }

    function PleaseWait(vis, block_form)
    {
        //var dis_form = false;
        var as = getToolbarButton("autosolve");
        if ((!vis) || !(as) || (as.get_isEnabled() && as.get_isChecked()) || (is_solve))
        {
            if ((vis)) {
                //$("#<% = divGrid.ClientID %>").fadeOut(250); 
                document.body.style.cursor = "wait";
                showLoadingPanel();
                if (block_form!=0 && !use_jquery_ajax) {
                    setTimeout('$("#<% = divGrid.ClientID %> :input").attr("disabled", true);', 10);
                    dis_form = true;
                }
            }
            else {
                hideLoadingPanel();
                document.body.style.cursor = "default";
            }
        }
        //if (last_focus!='') eval('theForm.'+last_focus+'.disabled = ' + vis + ';');
        var toolBar = $find("<%= RadToolBarMain.ClientID%>");
        if ((toolBar) && ((vis && btns_lst.length==0) || (!vis && btns_lst.length>0))) {
            var items = toolBar.get_items();
            for (var i=1; i<items.get_count(); i++) {
                var it = items.getItem(i);
                if (vis) {
                    if (it.get_isEnabled()) {
                        btns_lst.push(i);
                        it.disable();
                    }
                } else {
                    if ($.inArray(i, btns_lst)>=0) it.enable();
                }
            }
            if (!vis) btns_lst = [];
        }
        theForm.disabled = vis;
        if (vis) $('#divMain').fadeTo(650, 0.6); else $('#divMain').fadeTo(500, 1);
        //if (dis_form) {
        //    var focused = $(':focus');
        //    if ((focused) && (focused.length)) focused.prop("disabled", "disable");
        //}
    }
    
    var CurrencyThousandSeparator = "<% =UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    
    function Str2Val(val) {                
        var s = replaceString(",", ".", replaceString(CurrencyThousandSeparator, "", val)); //A0922
        //A0907 === remove thousands separators
        s = s.replace(/[^\-0-9.]/g, "");
        var last = s.lastIndexOf('.');
        if ((last > 0) && (last < s.length)) s = replaceString(".", "", s.substring(0, last)) + s.substring(last);
        //A0907 ==        
        if (s == "0.") s = "0";
        if (s == "1.") s = "1";
        if (s[0] == ".") s = "0" + s;
        var v = s * 1;
        if (s == v && s!="") {
            //            edit.value = v;
            return v;
        }
        else {
            return "undefined"
        };
    }

    // is_0_1: 0 = any number >0; 1 = [0..1]; -1 = any number, even empty string;
    function isValidNumber(edit, is_0_1, warning) {
        var v = Str2Val(edit.value);
        var msg = "";
        //        if (edit.value==="" && is_0_1==-1) return "";
        if (edit.value==="") return "";
        if (v=="undefined" || (is_0_1==1 && (v<0 || v>1))) msg = (is_0_1==1 ? "<% =JS_SafeString(String.Format(ResString("errWrongNumberRange"), 0,1)) %>" : "<%= JS_SafeString(ResString("errNotANumber")) %>");
        if (msg=="" && is_0_1==1 && v<0) msg = "<% =JS_SafeString(ResString("errPositiveZeroNumber")) %>";
        //        if (msg=="" && is_0_1==0 && v<=0) msg = "<% =JS_SafeString(ResString("errPositiveNumber")) %>";
        if (msg!="") {
            if ((warning) && !(dxDialog_opened)) dxDialog(msg, "setTimeout(\'theForm." + edit.id + ".focus();\', 30);");
            return "undefined";
        }
        return v;
    }

    function CheckForm() {
        return true;
    }

    function showGrid(el, all) {
        if (!view_grid && el.checked) {
            UpdateOpt();
            var t = getToolbarButton("timeperiods");
            if ((t)) {
                t.toggle();
                onChangeTimeperiods(t.get_isChecked());
                on_ok_code += (all ? "SetOptAll(true);" : "onOptionClick('" + el.name + "', " + el.checked + ");");
                return true;
            }
        }
        return onOptionClick(el.name, el.checked);
    }

    function onOptionClick(name, checked) {
        //        var d = getToolbarButton("Ignores");
        //        if ((d)) d.hideDropDown();
        UpdateOpt();
        return sendCommand("action=option&name=" + name + "&val=" + ((checked) ? 1 : 0), 1);
    }

    //function RowHover(r, hover, alt, funded) {
    //    if ((r)) {
    //        //var funded = (r.id.indexOf("funded") > 0);
    //        var bg = (hover == 1 ? (funded ? "#ffff33" :  (alt ? "#eaf0f5": "#f0f5ff")) : (funded ? "#ffff80" : (alt ? "#fafafa" : "#ffffff")));
    //        for (var i = 0; i < r.cells.length; i++) {
    //            r.cells[i].style.background = bg;
    //        }
    //    }
    //}

    function isFreezeView() {
        var c = $("#cbOptScrolling");
        return ((c) && (c.length) && (c.prop("checked")));
    }

    function isByPages() {
        return ($("#cbOptShowByPages").is(":checked") && pageSize()>0);
    }

    function pageSize() {
        return $("#cbPageMode").val()*1;
    }

    function checkPageSettings() {
        $("#cbPageMode").prop("disabled", (isByPages() ? "" : "disabled"));
    }

    function onKeyUp(id, is0_1)
    {
        var c = eval("theForm." + id);
        if ((c)) {
            var v = isValidNumber(c, is0_1, false);
            c.style.color = (c.value!="" && v=="undefined" ? "#ff0000" : "");
            if (typeof v!="undefined") return true;
        }
        return false;
    }

    function onBlur(id, name, guid, is0_1) {
        var c = eval("theForm." + id);
        if (!(is_ajax) && (c) && (!is_solve) && (((val_old=="" || c.value=="") && c.value!=val_old) || (Str2Val(c.value)!=Str2Val(val_old) || (c.value!=val_old)))) {
            var v = isValidNumber(c, is0_1, true);
            if (typeof v!="undefined" && (last_focus==id)) {
                //PleaseWait(1);
                last_focus=""; 
                //                on_ok_code="if (last_focus!='') eval('theForm.'+last_focus+'.focus(); theForm.'+last_focus+'.scrollIntoView(); $(\"#' + last_focus +'\").select();');"; 
                on_ok_code="if (last_focus!='') eval('theForm.'+last_focus+'.focus(); $(\"#' + last_focus +'\").select();'); CheckAssociatedRisk();"; 
                return sendCommand("action=" + name + (guid=="" ? "" : "&guid=" + guid) + "&val=" + v, 0);
            }
        }
        return false;
    }

    function onBlurCC(id, name, guid, com_id, is_max) {
        var c = eval("theForm." + id);
        if (!(is_ajax) && (c) && (((val_old=="" || c.value=="") && c.value!=val_old) || (Str2Val(c.value)!=Str2Val(val_old)))) {
            var v = isValidNumber(c, -1, true);
            if (typeof v!="undefined" && (last_focus==id)) {
                var cc = eval("theForm." + com_id);
                var is_ok = true;
                if (c.value!="" && (cc) && (cc.value!="")) {
                    var vv = isValidNumber(cc, -1, false);
                    if ((typeof vv!="undefined")) {
                        var msg = "";
                        if (!(is_max) && v>vv) {
                            is_ok = false;
                            msg = "<% = JS_SafeString(ResString("msgRAConstrMaxMin")) %>";
                        }
                        if ((is_max) && v<vv) {
                            is_ok = false;
                            msg = "<% = JS_SafeString(ResString("msgRAConstrMinMax"))%>";
                        }
                        if (msg!="") msg = replaceString("{0}", vv, msg);
                    }
                }
                if (is_ok) {
                    onBlur(id, name, guid, -1);
                    return true;
                }
                else {
                    dxDialog(msg, "setTimeout('theForm." + id + ".focus();', 200);");
                }
            }
        }
        return false;
    }
    
    function onFocus(id, val) {
        if (!(dxDialog_opened)) setTimeout('val_old = "' + val + '"; last_focus = "' + id + '"; $("#' + id + '").select();', 1);
    }

    function onSearch(value, force) {
        value = value.trim().toLowerCase();
        if (value == search_old && !force) return false;
        if (view_grid) {
            filterGrid(value);
        }
        search_old = value;
    }

    function filterGrid(search) {
        if (typeof(search)=="undefined") search = "";
        var rows = [];
        $("#divGridAlts").hide();
        var lst = $('[id^="tr_name_"]'); // $("td.cansearch");
        if (search == "") {
            lst.each(function (index, cell) {
                var name = $(cell).find("span");
                var attr = cell.getAttribute("clip_data");
                //if ((attr)) cell.innerHTML = attr;
                if ((attr) && (name)) name.html(attr);
                cell.parentNode.style.display = "";
                rows.push(cell.parentNode.id);
            });
        } else {
            lst.each(function (index, cell) { 
                var name = $(cell).find("span");
                var r = new RegExp(search, "gi");
                var attr = cell.getAttribute("clip_data");
                var showRow = false;
                if ((attr) && (name)) {
                    //cell.innerHTML = attr;
                    name.html(attr);
                    if ((attr.toLowerCase().indexOf(search) >= 0)) {
                        //cell.innerHTML = cell.innerHTML.replace(r, "<span style='font-weight:bold; color:#0066cc;'>$&</span>");
                        name.html(name.html().replace(r, "<span style='font-weight:bold; color:#0066cc;'>$&</span>"));
                        showRow = true;
                    }
                }
                cell.parentNode.style.display = (showRow ? "" : "none");
                if (showRow) rows.push(cell.parentNode.id);
            });
        }
        var pages = "";
        var p_size = pageSize();
        $("#tr_empty").hide();
        $("#tr_footer").show();
        $("#tr_footer").prop("disabled", "");
        if (isByPages() && rows.length>0 && p_size>0) {
            var p_total = Math.ceil(rows.length/p_size);
            var p_cur = cur_page;
            if (p_total>1) {
                if (p_cur>=p_total) p_cur = p_total-1;
                if (p_cur<0) p_cur = 0;
                if (p_cur!=cur_page && search!="") cur_page = p_cur;
                var r_start = (p_cur*p_size);
                for (i=0; i<rows.length; i++) {
                    if (i<r_start || i>=(r_start+p_size)) $("#"+rows[i]).hide();
                }
                pages = getPagesList(p_total, p_cur+1, "<div style='text-align:center; padding-top:4px;' class='text'><b><% = JS_SafeString(ResString("lblPage")) %></b>: <nobr><span class='gray'>%%pages%%</span></nobr></div>", "setPage(%%pg%%);");
            }
        }
        if (rows.length==0 && search!="") {
            if (!$("#tr_empty").length) {
                var tr = $('#<% =GridAlternatives.ClientID%> > tbody > tr').eq(0);
                if ((tr) && tr.length>0) {
                    tr.after('<tr id="tr_empty"><td class="text grid_row" align=center valign=middle style="padding:3em;" colspan=' + (tr[0].childNodes.length) +'><h5><% =JS_SafeString(ResString("errRANoSearch")) %></h5></td></tr>');
                }
            }
            $("#tr_empty").show();
            $("#tr_footer").hide();
            $("#tr_footer").prop("disabled", "disabled");
        }
        $("#spanPages").html(pages);
        $("#divGridAlts").show();
    }

    function InitSearch() {
        $("#tbSearch").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
            if (oldValue == "") return;
            setTimeout(function(){ if ($input.val() == "") onSearch("", false); }, 100);
        });
        //setTimeout("$('#tbSearch').focus();", 200);
    }

    function onMustSet(checked, chk2_id) {
        if ((checked) && (chk2_id != "")) {
            var c = eval("theForm." + chk2_id);
            //if ((c) && (c.checked)) on_ok_code += "theForm." + chk2_id + ".checked = 0;";
            if ((c) && (c.checked)) eval("theForm." + chk2_id + ".checked = 0;");
        }
    }

    function onMustsChanged(name, guid, checked, chk2_id, event, idx) {
        on_ok_code = "";
        var guids = "";
        if ((event) && (event.shiftKey) && last_cb[0]==name && last_cb[1]!=idx) {
            var a = last_cb[1];
            var b = idx;
            for (var i=(a<b ? a : b); i<=(a<b ? b : a); i++) {
                var c = $("[data_id='" + name + i + "']");
                var c2 = $("[data_id='" + (name=="musts" ? "mustnot" : "musts") + i + "']");
                if ((c) && (c.length) && (c2) && (c2.length) && !(c.is(':hidden')) && !(c2.is(':hidden'))) {
                    //if (i!=idx) { 
                    c.prop('checked', checked);
                    guids += (guids=="" ? "" : "_") + c.attr("data_guid");
                    //}
                    onMustSet(checked, c2.attr("id"));
                }
            }
        }
        else {
            onMustSet(checked, chk2_id);
        }
        on_ok_code += "if (last_focus!='') eval('theForm.'+last_focus+'.focus();');";
        last_cb = [name, idx];
        return sendCommand("action=" + name + "&guid=" + guid + "&val=" + ((checked) ? 1 : 0) + "&guids_lst=" + guids, 0);
    }

    function onChangeAutoSolve(chk) {
        if ((chk)) {
            //submits_checked = 0;
            use_jquery_ajax = 0;
        } else {
            //submits_checked = 0;
        }
        return sendCommand("action=autosolve&val=" + ((chk) ? 1 : 0), 1);
    }

    function onPartialChk(minprc, checked, focus) {
        var e = eval("theForm." + minprc);
        if ((e)) e.style.display = ((checked) ? "inline" : "none");
        if ((focus) && (e) && (checked)) on_ok_code += "setTimeout('theForm." + minprc + ".focus();', 50);";
    }

    function onPartialChanged(name, guid, minprc, checked, event, idx) {
        on_ok_code = "";
        var guids = "";
        if ((event) && (event.shiftKey) && (last_cb[0]=="partial" || last_cb[0]=="istolerance") && last_cb[1]!=idx) {
            var a = last_cb[1];
            var b = idx;
            for (var i=(a<b ? a : b); i<=(a<b ? b : a); i++) {
                var c = $("[data_id='" + name + i + "']");
                var e = $("[data_id='" + name + "_edit" + i + "']");
                if ((c) && (c.length) && (e) && (e.length) && !(c.is(':hidden')) && !(e.is(':hidden'))) {
                    c.prop('checked', checked);
                    guids += (guids=="" ? "" : "_") + c.attr("data_guid");
                    onPartialChk(e.attr("id"), checked, (i==idx));
                }
            }
        }
        else {
            onPartialChk(minprc, checked, true);
        }
        on_ok_code += "if (last_focus!='') eval('theForm.'+last_focus+'.focus();');";
        last_cb = [name, idx];
        return sendCommand("action=" + name + "&guid=" + guid + "&val=" + ((checked) ? 1 : 0) + "&guids_lst=" + guids, 0);
    }

    function onChangeColumn(id, chk) {
        return sendCommand("action=column&id=" + id + "&chk=" + ((chk) ? 1 : 0), 1);
    }

    function SetOptAll(chk) {
        theForm.cbOptMusts.checked=chk;
        theForm.cbOptMustNots.checked=chk;
        theForm.cbOptConstraints.checked=chk;
        theForm.cbOptDependencies.checked=chk;
        theForm.cbOptGroups.checked=chk;
        <% If Not IgnoreFP() Then%>theForm.cbOptFundingPools.checked=chk;<% End If%>
        theForm.cbOptRisks.checked = chk;
        if ((theForm.cbOptTimePeriodMins)) theForm.cbOptTimePeriodMins.checked=chk;
        if ((theForm.cbOptTimePeriodMaxs)) theForm.cbOptTimePeriodMaxs.checked=chk;
        if ((theForm.cbOptTimePeriods)) { 
            theForm.cbOptTimePeriods.checked=chk;
            UpdateOpt();
            if (!(view_grid) && (chk)) {
                showGrid(theForm.cbOptTimePeriods, true);
                return false;
            }
        }
        UpdateOpt();
        sendCommand("action=opt_all&val=" + ((chk) ? 1 : 0), 0);
        return false;
    }

    function UpdateOpt()
    {
        if (view_grid) {
            $("#lblcbOptConstraints").css("font-weight", ((<% =Scenario.Constraints.Constraints.Count %> == 0) ? "normal" : "bold"));
            var m = 0;
            if (theForm.tbMustsSum) m = theForm.tbMustsSum.value*1;
            $('input[name*="cbMusts"]').each(function (v) { m += (this.checked ? 1 : 0);} );
            $("#lblcbOptMusts").css("font-weight", ((m) ? "bold" : "normal"));
            var n = 0;
            if (theForm.tbMustNotSum) n = theForm.tbMustNotSum.value*1;
            $('input[name*="cbMustNot"]').each(function (v) { n += (this.checked ? 1 : 0);} );
            $("#lblcbOptMustNots").css("font-weight", ((n) ? "bold" : "normal"));
            var r = 0;
            $('input[name*="tbPFailure"]').each(function (v) { r += replaceString(",",".",this.value)*1;} );
            $("#lblcbOptRisks").css("font-weight", ((r) ? "bold" : "normal"));
            if ((theForm.cbOptTimePeriods)) {
                var tp = ((theForm.cbOptTimePeriods.checked));
                if ((theForm.cbOptTimePeriodMins)) theForm.cbOptTimePeriodMins.disabled = (tp);
                if ((theForm.cbOptTimePeriodMaxs)) theForm.cbOptTimePeriodMaxs.disabled = (tp);
            }
        }
        $("#divSolverActive").html(solver_names[solver_id][0]);
    }

    function UpdateButtons() {
        try {
            var a = eval("[" + theForm.<% =LinkedCC.ClientID %>.value + "]");
            var s = getToolbarButton("sync");
            if ((a) && (s)) s.set_enabled(a.length>0);
            <%--            <% If Scenario.Alternatives.Count > 0 Then%>var x = getToolbarButton("export_logs");
            if ((x)) x.set_enabled(solver_id != solver_id_msf);<% End If%>--%>
            var as = getToolbarButton("autosolve");
            <% If RA.Solver.OPT_GUROBI_USE_CLOUD Then %>if ((as)) as.set_enabled(solver_id != solver_id_gurobi);<% End If%>
            if ((as)) as.set_imageUrl("../../Images/ra/" + (as.get_isEnabled() && as.get_isChecked() ? "repeat-20.png" : "repeat-20_.png"));
            var sa = theForm.<% =SolverActive.ClientID %>;
            var toolbar = $find("<%=RadToolBarMain.ClientID %>");
            if ((sa) && (toolbar)) {
                var r = toolbar.get_items().getItem(<% = BTN_SOLVER %>);
                if ((r)) { 
                    r.set_imageUrl("../../Images/ra/" + ((sa.value*1) ? "config-20_blue.png" : "config-20.png"));
                    r.get_imageElement().title = "<% =JS_SafeString(ResString("lblRASolverSettings")) %>" + ((sa.value*1) ? "\n[ Solver is not active ]" : "");
                }
            }
            if ((toolbar)) {
                var t = getToolbarButton("timeperiods");
                if ((t)) {
                    var ts_idx = t.get_index() + 1;
                    var s = toolbar.get_items().getItem(ts_idx);
                    if ((s)) s.set_enabled(t.get_isChecked() && t.get_enabled());
                }
<%--                var d = toolbar.get_items().getItem(<% = BTN_DOWNLOAD %>);
                if ((d) && (t)) d.set_enabled(!t.get_isChecked());--%>
            }
            var e = theForm.cbOptExpandTP;
            if ((e)) {
                var have_closed = false;
                var have_opened = false;
                tp_lst = [];
                var c = $("#cbColumn<% = CInt(raColumnID.Cost) %>");
                if ((c) && (c.length) && c.prop("checked")) tp_lst.push(<% = CInt(raColumnID.Cost) %>);
                for (var i=<% = CInt(raColumnID.CustomConstraintsStart) %>; i<<% = CInt(raColumnID.CustomConstraintsStart) + Scenario.Constraints.Constraints.Count %>; i++) {
                    var h = $("#tp_" + i + "_hdr");
                    if ((h) && (h.length)) tp_lst.push(i);
                }
                for (var i=0; i<tp_lst.length; i++) {
                    var vis = ($("#tp_" + tp_lst[i] + "_hdr").css('display') != 'none');
                    if ((vis)) have_opened = true; else have_closed = true;
                }
                var st = -1;
                if (have_opened && !have_closed) st = 1;
                if (!have_opened && have_closed) st = 0;
                if (tp_lst.length<=1) st = (have_opened);
                e.checked = (st==1);
                $("#cbOptExpandTP").prop("indeterminate", (st==-1) ? "1" : "");
                if ((theForm.cbOptTimePeriods)) {
                    e.disabled = ((theForm.cbOptTimePeriods.checked) || (theForm.cbOptTimePeriods.disabled) || (tp_lst.length<=1));
                }
            }
            var fx = $("#cbOptFixedWidth");
            if ((fx) && (fx.length)) fx.prop("disabled", isFreezeView() ? "disabled": "");

            var ex = getToolbarButton("export_xls");
            if ((ex)) ex.set_enabled(view_grid);

            var l = getToolbarButton("view_logs");
            if ((l)) l.set_enabled($("#tbLogs").val()!="");

            var ud = theForm.cbTPUseDiscountFactor;
            var df = theForm.tbDiscount;
            if ((ud) && (df)) df.disabled = !(ud.checked);

            var d = getToolbarButton("delete_nonfunded");
            if ((d)) d.set_enabled(solverStateID()==<% = CInt(raSolverState.raSolved)%>);
          
            <%If Not ShowDraftPages() Then %>
            var atbl = getToolbarButton("alloc_tbl");
            if ((atbl)) atbl.set_visible(false);
            atbl = getToolbarButton("alloc_tbl2");
            if ((atbl)) atbl.set_visible(false);
            <%End If%>

            updateSolveButton();
        } 
        catch (e) {
        }
    }

    function updateSolveButton() {
        var s = getToolbarButton("solve");
        if ((s)){
            var c = "#454566";
            switch (solverStateID()*1+"") {
                case "<% = CInt(raSolverState.raSolved) %>":
                    c = "#228c22";
                    break;
                case "<% = CInt(raSolverState.raInfeasible) %>":
                    c = "#cc3d00";
                    break;
                case "<% = CInt(raSolverState.raError)%>", "<% = CInt(raSolverState.raExceedLimits)%>":
                    c = "#c2000d";
                    break;
            }
            $(s._element).find(".rtbText").css("color", c);
        }
        s = getToolbarButton("unsolve");
        if ((s)){
            s.set_enabled(solverStateID()+""=="<% = CInt(raSolverState.raSolved) %>");
        }
    }

    function sendCommand(cmd, force, form_data) {
        if (!view_grid) {
            sendAJAX(cmd, form_data);
            return true;
        };
        flashed = false;
        last_submit = 0;
        if (CheckForm()) {

            if (((use_jquery_ajax || !isAutoSolve()) && solverStateID()!=<% = CInt(raSolverState.raSolved)%>) && force!=1 && force!=true) {
            //if (use_jquery_ajax && force!=1 && force!=true) {

                sendAJAX(cmd, form_data);

            } else {

                var tga = $("#divGridAlts").get(0);
                scroll_l = -1;
                scroll_t = -1;
                if ((tga)) {
                    scroll_l = tga.scrollLeft;
                    scroll_t = tga.scrollTop;
                }
                var am = $find("<%=RadAjaxManagerMain.ClientID%>");
                if ((am)) { PleaseWait(1); am.ajaxRequest(cmd); is_ajax=1; if (!force) last_submit =new Date(); return true; }
            }

        }
        return true;
    }
    
    function onResponseEnd(sender, arguments) {
        is_ajax = 0;
        PleaseWait(0);
        InitSearch();
        $("#tbSearch").val(search_old);
        //UpdateTimeperiods(true);
        onResize();
        on_error_code = "";
        if (on_ok_code != "") eval(on_ok_code);
        on_ok_code = "";
        //        if (scroll_l>0 && scroll_t>0) setTimeout('var tga = $("#divGridAlts").get(0); if ((tga)) { tga.scrollLeft = ' + scroll_l + '; tga.scrollTop = ' + scroll_t + '; }', 10);
        if (scroll_l>0 && scroll_t>0) {
            var tga = $("#divGridAlts").get(0); 
            if ((tga)) { 
                tga.scrollLeft = scroll_l; 
                tga.scrollTop = scroll_t;
            }
        }
        var t = new Date() - last_submit;
        if (!use_jquery_ajax && !submits_checked && last_submit>0 && (t)> max_submit_delay) {
            submits_checked = 1;
            var b = getToolbarButton('autosolve');
            var ask_autosolve = ((b) && (b.get_isChecked()));
            if (ask_autosolve) {
                dxConfirm('<% = JS_SafeString(ResString("confRAUseJQueryAutoSolve"))%>', "use_jquery_ajax=1; on_ok_code=\"onChangeAutoSolve(false); var b = getToolbarButton('autosolve'); if ((b)) b.unCheck();\"; onAskUseAjax();", "onAskUseAjax();");
            } else {
                use_jquery_ajax = 1; onAskUseAjax();    
            }
        }
        last_submit = 0;
        updateSolveButton();
    }

    function onAskUseAjax() {
        sendAJAX("action=use_ajax&use_ajax=" + (use_jquery_ajax ? "1" : "0"));
        window.status = "Switch to ajax requests...";
        setTimeout("window.status='';", 1500);
    }

    function setSolverXAConf() {
        solver_xa_conf = 1; 
        document.cookie = '<% =COOKIE_ASK_GUROBI_CLOUD %>=1';
    }

    function onSolve() {
        <% If RA.Solver.OPT_GUROBI_USE_CLOUD Then %>if (!is_solve) {
            var ask = ((solver_id == solver_id_gurobi) && (solver_xa_conf != "1"));
            //if (!(ask) && theForm.<% =SolverActive.ClientID %>) ask = !(theForm.<% =SolverActive.ClientID %>.value*1);
            if ((ask)) dxConfirm("<% = JS_SafeString(ResString("msgRASwitchGurobi")) %>", "setSolverXAConf();", ";"); else Solve();
        }<% End If %>
    }

    function Solve() {
        is_solve = 1;
        on_ok_code = "is_solve=0;";
        on_error_code = "is_solve=0;";
        //var $focused = $(':focus');
        //if ((focused) && (focused.length)) focused.blur();
        var focused = document.activeElement;
        if ((focused)) focused.blur();
        theForm.blur();
        theForm.focus();
        showLoadingPanel();
        return sendCommand("action=solve", 1);
    }

    function onUnSolve() {
        showLoadingPanel();
        return sendCommand("action=unsolve", 1);
    }
    
    function onClickToolbar(sender, args) {
        var button = args.get_item();
        //        args.set_cancel(true);    
        if ((button))
        {
            var btn_id = button.get_value();
            if ((btn_id))
            {
                if (btn_id.substr(0, 5) == "cbOpt") return false;
                switch (btn_id+"") {
                    case "solve":
                        <% If RA.Solver.OPT_GUROBI_USE_CLOUD Then %>onSolve();<% Else%>Solve();<% End if%>
                        break;
                    case "unsolve":
                        onUnSolve();
                        break;
                    case "autosolve":
                        var chk = (button.get_isChecked());
                        var al = <% =Bool2JS(RA.Scenarios.GlobalSettings.isBigModel) %>;
                        if (chk && al) {
                            var result = DevExpress.ui.dialog.confirm("<% = JS_SafeString(String.Format(ResString("confRAEnableAutoSolve"), Scenario.Alternatives.Count)) %>", resString("titleConfirmation"));
                            result.done(function (dialogResult) {
                                if (dialogResult) {
                                    onChangeAutoSolve(chk);
                                } else {
                                    var b = getToolbarButton('autosolve'); 
                                    if ((b)) b.unCheck();
                                }
                            });
                        } else {
                            onChangeAutoSolve(chk);
                        }
                        break;
                    case "scenarios":
                        InitScenarios();
                        InitScenariosDlg();
                        break;
                    case "select_alts":
                        SelAlternatives();
                        break;
                    case "solver":
                        InitSolverDlg();
                        dlg_solver.dialog("open");
                        setTimeout('showSolverSettings(solver_id);', 150);
                        break;
                    case "columns":
                        InitColumnsDlg();
                        dlg_columns.dialog("open");
                        break;
                    case "download":
                        //                        CreatePopup("<% = PageURL(_PGID_PROJECT_DOWNLOAD, String.Format("&action=download&type=ahp&ext=ahpz&zip=yes&id={1}&ra=yes{0}&skipunused=yes", GetTempThemeURI(True), App.ProjectID)) %>", "download_ra", "");
                        CreatePopup("<% = PageURL(_PGID_PROJECT_DOWNLOAD, String.Format("&action=download&type=ahp&ext=ahpz&zip=yes&id={1}&ra=yes{0}", GetTempThemeURI(True), App.ProjectID)) %>", "download_ra", "");
                        break;
                    case "export_xls":
                        downloadExcel();
                        break;
                    case "export_oml":
                        CreatePopup("<% = PageURL(CurrentPageID, String.Format("?{0}=export&type=oml{1}&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" + Math.round(100000*Math.random()), "export_oml", "");
                        break;
                    case "export_mps":
                        CreatePopup("<% = PageURL(CurrentPageID, String.Format("?{0}=export&type=mps{1}&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" + Math.round(100000*Math.random()), "export_mps", "");
                        break;
                    case "export_logs":
                        CreatePopup("<% = PageURL(CurrentPageID, String.Format("?{0}=export&type=logs{1}&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" +  Math.round(100000*Math.random()), "export_logs", "");
                        break;
                    case "view_logs":
                        if (!(dlg_logs)) InitLogsDlg(); else dlg_logs.dialog("open");
                        break;
                    case "alloc_tbl":
                    case "alloc_tbl2":
                        onAllocTable();
                        break;
                    case "export_alloc":
                        exportAllocationComparision();
                        break;
                    case "sync":
                        onSyncConstraints();
                        break;
                    case "help":
                        showLegend();
                        break;
                    case "create_risk":
                        onCreateRisk();
                        break;
                    case "select_risk":
                        onSelectRiskProject();
                        break;
                    case "unset_risk":
                        onDetachRisk();
                        break;
                    case "open_risk":
                        if (associated_risk>0) OpenRisk();
                        break;
                    case "sync_alts_risk":
                        if (associated_risk>0) SyncAltsRM();
                        break;
                    case "import_risk":
                        ImportRisk(true);
                        break;
                    case "import_prob":
                        ImportRisk(false);
                        break;
                    case "delete_risk":
                        DeleteRisks();
                        break;
                    case "timeperiods":
                        onChangeTimeperiods(button.get_isChecked());
                        break;
                    case "delete_nonfunded": //A1431 ===
                        dxConfirm("<%=JS_SafeString(ResString("msgSureDeleteNonFunded"))%>", "sendCommand('action=delete_nonfunded', 1);");
                        break; //A1431 ==
                }
            }
        }
    }

    function OnClientDropDownOpening(sender, args) {
        var dropDown = args.get_item();
        if ((dropDown) && (dropDown.get_index()==<% =BTN_DOWNLOAD%>)) {
            var e = dropDown.get_dropDownElement();
            if ((e)) {
                var l = e.parentNode;
                var o = -(dropDown.get_element().clientWidth);
                e.style.left = o;
                //dropDown.get_animationContainer().style.left = o;
            }
        }
    }  

    function SelAlternatives() {
        var sList = "<% =JS_SafeString(ResString("lblRASelAlts"))%><div style='margin:1ex;" + (alts_all.length>8 ? " height:292px; overflow:auto" : "") + "'>";
        for (i=0; i<alts_all.length; i++) {
            var a = alts_all[i];
            sList += "<div style='margin:2px'><input type='checkbox' id='alt_" + i + "' value='" + a[0] + "' " + ((a[3]) ? "checked " : "") + "onclick='checkSelAlts();'><label for='alt_" + i + "'>" + a[1] + ". " + a[2] + "</label></div>";
        }
        sList += "</div>";
        if (alts_all.length>1) {
            sList += "<div class='small' style='text-align:right; padding-right:1ex;'>";
            sList += ((solverStateID() == <% = CInt(raSolverState.raSolved)%>) ? "<a href='' onclick='return selectFundedAlts(1);' class='actions'><% =ResString("lblSelectFundedOnly") %></a>" : "<span class='gray'><% =ResString("lblSelectFundedOnly") %></span>");
            sList += " | <a href='' onclick='return selectAlts(1);' class='actions'><% =ResString("lblSelectAll") %></a> | <a href='' onclick='return selectAlts(-1);' class='actions'><% =ResString("lblSelectInverse") %></a> | <a href='' onclick='return selectAlts(0);' class='actions'><% =ResString("lblSelectNone") %></a></div>";
        }
        dxDialog(sList, "SelectAlternatives();", ";", "<% =JS_SafeString(ResString("btnRASelAlts"))%>", "<% = JS_SafeString(ResString("btnSelect")) %>", "<% = JS_SafeString(ResString("btnCancel"))%>");
        setTimeout("checkSelAlts();", 100);
    }

    function onSyncConstraints() {
        var a = [];
        try { a = eval("[" + theForm.<% =LinkedCC.ClientID %>.value + "]"); } catch (e) { }

        if ((a) && (a.length>0)) {
            var sList = "<% =JS_SafeString(ResString("lblRASyncSelCC")) %><div style='margin:1ex;" + (a.length>8 ? " height:292px; overflow:auto" : "") + "'>";
            for (i=0; i<a.length; i++) {
                sList += "<div style='margin:2px'><input type='checkbox' id='cc_" + i + "' value='" + a[i][0] + "' " + ((a[i][3]) ? "checked " : "") + "onclick='checkSelCC();'><label for='cc_" + i + "'>" + a[i][1] + "</label>" + (a[i][2] == "" ? "" : "<div class='small gray' style='padding-left:21px'>" + a[i][2] + "</div>") + "</div>";
            }
            sList += "</div>";
            if (a.length>1) {
                sList += "<div class='small' style='text-align:right; padding-right:1ex;'><a href='' onclick='return selectCC(" + a.length + ", 1);' class='actions'><% =ResString("lblSelectAll") %></a> | <a href='' onclick='return selectCC(" + a.length + ", -1);' class='actions'><% =ResString("lblSelectInverse") %></a> | <a href='' onclick='return selectCC(" + a.length + ", 0);' class='actions'><% =ResString("lblSelectNone") %></a></div>";
            }
            dxDialog(sList, "SyncConstraints();", ";", "<% =JS_SafeString(ResString("btnRASyncCC")) %>");
            setTimeout("checkSelCC();", 100);
        } else {
            sendCommand("action=sync_constr", 1);
        }
    }

    function checkSelAlts() {
        dxDialogBtnDisable(true, !(getSelAlts().length));
    }

    function checkSelCC() {
        dxDialogBtnDisable(true, !(getSelCC().length));
    }

    function selectAlts(sel) {
        for (var i=0; i<alts_all.length; i++) {
            var c = $("#alt_" + i);
            if ((c)) {
                var s = c.is(':checked');
                if (sel==-1) s = !s; else s = sel;
                c.prop('checked', (s) ? true : false);
            }
        }
        checkSelAlts();
        return false;
    }

    function selectFundedAlts(sel) {
        for (var i=0; i<alts_all.length; i++) {
            var c = $("#alt_" + i);
            if ((c)) {
                var tr = $('tr[aidx="' + i + '"]');
                var checked = ((tr) && (tr.length) && (tr.attr("is_funded")=="1"));
                c.prop('checked', (checked) ? true : false);
            }
        }
        checkSelAlts();
        return false;
    }

    function selectCC(cnt, sel) {
        for (var i=0; i<cnt; i++) {
            var c = $("#cc_" + i);
            if ((c)) {
                var s = c.is(':checked');
                if (sel==-1) s = !s; else s = sel;
                c.prop('checked', (s) ? true : false);
            }
        }
        checkSelCC();
        return false;
    }

    function getSelAlts() {
        var lst = [];
        for (var i=0; i<alts_all.length; i++) {
            var c = $("#alt_" + i);
            if ((c)) {
                var s = c.is(':checked');
                if ((s)) lst.push(c.prop('value'));
            }
        }
        return lst;
    }
    
    function getSelCC() {
    var lst = [];
    var a = [];
    try { a = eval("[" + theForm.<% =LinkedCC.ClientID %>.value + "]"); } catch (e) { }
    if ((a) && (a.length>0)) {
        for (var i=0; i<a.length; i++) {
            var c = $("#cc_" + i);
            if ((c)) {
                var s = c.is(':checked');
                if ((s)) lst.push(c.prop('value'));
            }
        }
    }
    return lst;
    }

    function SelectAlternatives() {
        var sel = getSelAlts();
        var ids = sel.join(",");
        on_ok_code = "initSelAlts('" + ids + "');";
        if (!view_grid) on_ok_code += "sendAJAX('action=refresh');";
        sendCommand("action=sel_alts&ids=" + ids, 1);
    }

    function initSelAlts(ids) {
        ids = " ," + ids + ",";
        for (var i=0; i<alts_all.length; i++) {
            alts_all[i][3] = (ids.indexOf("," + alts_all[i][0] + ",", 0)>0 ? 1 : 0);
        }
        CheckSelAlts();
    }

    function SyncConstraints() {
        var sel = getSelCC();
        var ids = sel.join(",");
        sendCommand("action=sync_constr&ids=" + ids, 1);
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", on_agree + ";", ";");
            return false;
        }
        return true;
    }

    var risk_id = associated_risk;
    var prj_name = "Risk_<% = SafeFormString(App.ActiveProject.ProjectName) %>";
    var master_id = -1;
    var dlg_models = null;
    var scroll_id = -1;

    function onCreateRisk() {
        var prj = "";
        master_id = -1;
        for (var i=0; i<master.length; i++) {
            if (master_id<0 && master[i][1].toLowerCase().indexOf("risk")>=0) master_id = master[i][0];
            prj += "<option value='" + master[i][0] + "'" + (master_id == master[i][0] ? " selected" : "") + ">" + master[i][1]  + "</option>";
        }
        if (master_id<0 && master.length>0) master_id = master[0][0];
        if (prj!="") prj = "<div style='margin-top:4px'><% =JS_SafeString(ResString("lbl_psMasterProject")) %>:&nbsp;<select name='master_id' style='width:280px;' onchange='master_id = this.value;'>" + prj + "</select></div>";
        dxDialog("<div style='text-align:right; margin-right:1ex;'><nobr><b><% =JS_SafeString(ResString("lblRACreateARMPrjName")) %></b>:&nbsp;<input type='text' id='tbPrjName' value='" + prj_name + "' style='width:280px' onkeyup ='prj_name = this.value; CheckEmptyValue(this.value);'></nobr>" + prj + "</div>", "CreateRisk();", ";", "<% =JS_SafeString(ResString("lblRACreateARM")) %>", "<% =JS_SafeString(ResString("btnCreate")) %>", "<% =JS_SafeString(ResString("btnCancel")) %>");
        setTimeout('CheckEmptyValue($("#tbPrjName").val());', 100);
    }

    function CreateRisk() {
        if (prj_name!="") {
            on_ok_code = "onRiskCreated(data, '" + replaceString("'", "\'", prj_name) + "');";
            showLoadingPanel();
            sendAJAX("action=create_risk&master_id=" + master_id + "&name=" + enc(prj_name));
            $("#divModels").html("");
            dlg_models = null;
        }
    }

    function onRiskCreated(data, prj_name) {
        var id = eval(data); 
        if (id>0) { 
            associated_risk = id; 
            associated_name = prj_name; 
            CheckAssociatedRisk(); 
            setTimeout('onOpenRisk();', 250);
        }
    }

    function onOpenRisk() {
        if (associated_risk>0) dxConfirm("<% =JS_SafeString(ResString("confRAOpenARM")) %>", "OpenRisk();", ";");
    }

    function SyncAltsRM() {
        on_ok_code = "CheckAssociatedRisk();onOpenRisk();";
        sendCommand("action=sync_alts_risk", 1);
    }

    function doImportRisk() {
        on_ok_code = "onImportRisk(eval(data));";
        showLoadingPanel();
        sendAJAX("action=import_risk");
    }

    var as_risk = true;
    function ImportRisk(risks) {
        as_risk = risks;
        if (scenarios.length<2) {
            dxConfirm((risks ? '<% =JS_SafeString(ResString("confRAImportRisksYesNo")) %>' : '<% =JS_SafeString(ResString("confRAImportPoSYesNo"))%>'), "doImportRisk(false);");
        } else {
            var msg = risks ? '<% =JS_SafeString(ResString("confRAImportRisks")) %>' : '<% =JS_SafeString(ResString("confRAImportPoS")) %>';            
            var myDialog = DevExpress.ui.dialog.custom({
                title: "<% =JS_SafeString(ResString("titleConfirmation")) %>",
                messageHtml: msg,
                buttons: [{ text: "<% =JS_SafeString(ResString("btnRAAllScenarios"))%>", onClick : function () { return 1; }}, {text: "<% =JS_SafeString(ResString("btnRACurScenario")) %>", onClick : function () { return 2; }}, {text: "<% =JS_SafeString(ResString("btnRACancelImport")) %>", onClick : function () { return 3; }}]
            });
            myDialog.show().done(function(dialogResult){
                if (dialogResult == 1) doImportRisk(true);
                if (dialogResult == 2) doImportRisk(false);
            });
        }
    }

    function doImportRisk(to_all) {
        on_ok_code = "CheckAssociatedRisk();";
        $("#cbColumn<% =CInt(raColumnID.ProbFailure) %>").prop("checked", true);
        $("#cbColumn<% =CInt(raColumnID.ProbSuccess)%>").prop("checked", true);
        sendCommand("action=import_risk&asrisk=" + (as_risk ? 1 : 0) + "&to_all=" + ((to_all) ? 1 : 0), 1);
        //$(jDialog_ID).dialog("close");
    }

    function DeleteRisks() {
        dxConfirm('<% =JS_SafeString(ResString("confRADeleteRisks")) %>', "doDeleteRisks();");
    }

    function doDeleteRisks() {
        on_ok_code = "CheckAssociatedRisk();";
        sendCommand("action=delete_risks", 1);
    }

    var cnt_checks = 0 ;
    function OpenRisk() {
        if (associated_risk>0) {
            showLoadingPanel();
            <% If isSLTheme() Then%>
            cnt_checks = 0;
            DoOpenProject();
            return false;
            <% Else%>document.location.href = replaceString("-999999999", associated_risk, url_open_risk);<% End If%>
        }
    }

    function DoOpenProject() {
        if ((window.opener) && ((typeof window.opener.openProject)!="undefined")) { window.opener.openProject(associated_risk, def_page); return true; }
        if ((window.parent) && ((typeof window.parent.openProject)!="undefined")) { window.parent.openProject(associated_risk, def_page); return true; }
        cnt_checks+=1;
        if (cnt_checks<3) {
            setTimeout("DoOpenProject();", 10000);
        } else {
            //            OnOpenAsReload(1);
            on_ok_code = "OnOpenAsReload(eval(data));";
            sendAJAX("action=active&prj_id=" + associated_risk);
        }
    }

    function OnOpenAsReload(res) {
        if ((res)) {
            var w = window.opener;
            if (!(w)) w = window.parent;
            if ((w)) w.document.location.href = replaceString("{0}", associated_risk, def_sl_url);
        } else {
            jQuery("<% = JS_SafeString(ResString("errCantFindProject")) %>", true, "reload();");
        }
    }

    function onSelectRiskProject() {
        if ($("divModels").html()=="" || (dlg_models==null)) {
            on_ok_code = "AssociateRisk(data);";
            showLoadingPanel();
            sendAJAX("action=prj_list");
        } else {
            $("#divModelsList").dialog("open");
        }
    }

    function CheckEmptyValue(val) {
        dxDialogBtnDisable(true, (val.trim()=="" ? true : false));
    }

    function SetRiskPrj(id, idx) {
        $("#btnSelectPrj").button({disabled: false});
        risk_id = id;
        var n = $("#prj_title" + idx);
        if ((n) && (n.length)) prj_name = n.prop("title");
    }

    function CheckAssociatedRisk() {
        var has_risk = associated_risk>0;
        var b = getToolbarButton("open_risk");
        if ((b)) {
            b.set_enabled(has_risk);
            var name_img = "";
            if (has_risk && associated_name!="") name_img = "<img src='../../Images/ra/info12.png' width=12 height=12 style='float:right; padding-left:4px; padding-top:6px;' border=0 title=\"Model name: &laquo;" + replaceString('"', '&quot;', associated_name) + "&raquo;\">";
            b.set_text(name_img + "<% = JS_SafeString(ResString("lblRAOpenARM")) %>");
        }
        b = getToolbarButton("sync_alts_risk");
        if ((b)) b.set_enabled(associated_risk>0);
        b = getToolbarButton("import_risk");
        if ((b)) b.set_enabled(has_risk);
        b = getToolbarButton("import_prob");
        if ((b)) b.set_enabled(has_risk);
        b = getToolbarButton("unset_risk");
        if ((b)) b.set_enabled(has_risk);
        b = getToolbarButton("select_risk");
        if ((b)) b.set_text((has_risk) ? "<% = JS_SafeString(ResString("lblRAAssociateARMNew")) %>" : "<% = JS_SafeString(ResString("lblRAAssociateARM")) %>");
        b = getToolbarButton("delete_risk");
        if ((b)) {
            b.set_enabled(0);
            //            b.set_enabled(theForm.tbRiskSum.value!="0");
            var r = 0;
            $('input[name*="tbPFailure"]').each(function (v) { r += replaceString(",",".",this.value)*1;} );
            b.set_enabled((r!=0));
        }
        <% If RadToolBarMain.Items(BTN_RISKS).Visible Then%>var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var r = toolbar.get_items().getItem(<% = BTN_RISKS%>);
            if ((r)) r.set_imageUrl("../../Images/ra/" + ( associated_risk>0 ? "dices_blue_20.png" : "dices_black_20.png"));
        }<% End if %>
    }

    function CheckSelAlts() {
        var has_dis = false;
        for (var i=0; i<alts_all.length; i++) if (!(alts_all[i][3])) has_dis = true;
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var r = getToolbarButton("select_alts");
            if ((r)) r.set_imageUrl("../../Images/ra/" + ((has_dis) ? "filter20_red.png" : "filter20.png"));
        }
    }
    
    function onDetachRisk() {
        dxConfirm("<% =JS_SafeString(ResString("confRADetachARM")) %>", "DetachRisk();");
    }

    function DetachRisk() {
        on_ok_code = "associated_risk = -1; associated_name = ''; CheckAssociatedRisk();";
        sendCommand("action=unset_risk", 0);
    }

    function SetAssociatedRisk() {
        if (risk_id>0) {
            on_ok_code = "associated_risk = " + risk_id + "; associated_name = '" + replaceString("'", "\'", prj_name) + "'; CheckAssociatedRisk(); onOpenRisk();";
            sendCommand("action=select_risk&model=" + risk_id, 1);
        }
    }

    var prj_count = 0;
    function InitProjectsList(lst) {
        var t = $("#divModels");
        if ((t)) {
            var prj = "";
            prj_count = lst.length;
            for (var i=0; i<lst.length; i++) {
                if (lst[i][0]<0 || lst[i][0]==associated_risk) scroll_id = i;
                prj += "<div id='prj" + i + "'"+(i&1 ? " style='background:#f0f0f0'" : "")+"><nobr><input type='radio' class='radio' id='prjName" + i + "' name='prjName' value='" + Math.abs(lst[i][0]) + "'" + (i==scroll_id ? " checked" : "") +" onclick='SetRiskPrj(this.value, " + i + ")'><label for='prjName" + i + "'><span id='prj_title" + i + "' title='" + replaceString("'", "&#39;", htmlEscape(lst[i][1])) + "'>" + htmlEscape(lst[i][1]) + "</span></label></nobr></div>\n";
            }
            prj += "<div id='divPrjMsg' class='h5 gray' style='text-align:center; padding-top:5em; display:none'></div>";
            t.html(prj);
        }
    }

    function filterProjectsList() {
        var e = document.getElementById("inpSearch");
        if ((e)) {
            var txt = e.value.toLowerCase();
            var txt_len = txt.length;
            var has_files = false;
            for (var i=0; i<prj_count; i++) {
                var p = document.getElementById("prj_title" + i);
                if ((p) && (p.title) && (p.title!="")) {
                    var n = p.title;
                    var s = 0;
                    var show = (txt=="");
                    if (!show) {
                        while (n.toLowerCase().indexOf(txt, s)>=0) {
                            var idx = n.toLowerCase().indexOf(txt, s);
                            var n_ = n.slice(0, idx) + "<span class='src'>" + n.slice(idx, idx+txt_len) + "</span>" + n.slice(idx+txt_len);
                            n = n_;
                            s = idx + txt_len + 25;
                            show = true;
                        }
                    }
                    if (show) {
                        $("#prj" + i).show(); 
                        has_files= true; 
                    } else  { 
                        $("#prj" + i).hide(); 
                    }
                    p.innerHTML = n;
                }
            }
            if (has_files) {
                $("#divPrjMsg").hide();
            } else {
                $("#divPrjMsg").html("<% = JS_SafeString(ResString("errRANoSearch")) %>").show();
            }
        }
    }

    function onSearchRisk() {
        setTimeout("filterProjectsList();", 25);
    }

    function AssociateRisk(data) {
        var lst = eval(data);
        InitProjectsList(lst);
        dlg_models = $("#divModelsList").dialog({
            autoOpen: true,
            //              height: 500, //(lst.length>10 ? 400 : "auto"),
            minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: 500,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("lblRAAssociateARM")) %>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                OK: {
                    id: "btnSelectPrj",
                    text: "<% = JS_SafeString(ResString("btnSelect")) %>",
                          disabled: true,
                          click: function() {
                              $("#divModelsList").dialog("close");
                              SetAssociatedRisk();
                          }
                      },
                      Cancel: function() {
                          $("#divModelsList").dialog("close");
                      }
                  },
            open: function() {
                $("body").css("overflow", "hidden");
                if (scroll_id>=0) { setTimeout('var p = $("#prj' + scroll_id + '").offset().top - $("#divModels").offset().top - 20; if (p>0) $("#divModels").animate({ scrollTop: p }, 1000);', 10); }
                setTimeout('var h = $("#divModelsList").height(); if (h*1>100) { $("#divModels").height(30); $("#divModels").height(h-60); }', 50);
            },
            close: function() {
                $("body").css("overflow", "auto");
                //                $("#divModelsList").dialog("destroy");
            },
            resize: function( event, ui ) { $("#divModels").height(30); $("#divModels").height(Math.round(ui.size.height-176)); }
        });
      }

      function sendAJAX(params, form_data) {
          is_ajax = 1;
          PleaseWait(1, 0);
          var is_post = ((typeof form_data!="undefined" && (form_data)));
          $.ajax({
              type: (is_post ? "POST" : "GET"),
              url: (is_post ? "<% =PageURL(CurrentPageID, "ajax=yes") %>&" + params + "&r=" + Math.random() : ""),
              data: (is_post ? form_data : "ajax=yes&" + params + "&r=" + Math.random()),
              dataType: "text",
              async:true,
              beforeSend: function () {
                  // check data if needed and "return false" to cancel
                  return true
              },
              success: function (data) {
                  PleaseWait(0);
                  receiveAJAX(data);
              },
              error: function(x, t, m) {
                  PleaseWait(0);
                  DevExpress.ui.dialog.alert("<% =ResString("ErrorMsg_ServiceOperation") %><br><br><span class='small gray'>" + t + "</span>", "error");
            },
            timeout: 180000    // 180 seconds
        });
    }

    function receiveAJAX(data) {
        //UpdateTimeperiods(true);
        is_ajax = 0;
        $("#divSolverTime").html("");
        PleaseWait(0);
        if ((on_ok_code!="")) eval(on_ok_code);
        on_ok_code = "";  
        
        if (!(view_grid) && (data != 'timeperiods')){
            if ((data) && (data.indexOf("[") == 0)) {
                var idx_refresh = -1;

                var received_data = eval(data);
                if ((received_data)) {
                    if (received_data[0] == 'solved') {
                        $("#RATimeline").timeline("option", "solvedResults", received_data[1]);
                        idx_refresh = 2;
                    }
                    if (received_data[0] == 'refresh') {
                        $("#RATimeline").timeline("option", "projs", received_data[1]);
                        $("#RATimeline").timeline("option", "portfolioResources", received_data[2]);
                        $("#RATimeline").timeline("option", "solvedResults", received_data[3]);
                        idx_refresh = 4;
                    };
                };
                if (idx_refresh>0) {
                    if (received_data.length>idx_refresh && received_data[idx_refresh]!="") $("#tdFundedInfo").html(received_data[idx_refresh]);
                    if (received_data.length>idx_refresh+1) $("#divSolverMsg").html(received_data[idx_refresh+1]);
                    if (solverStateID==<% =raSolverState.raInfeasible %>) $("#RATimeline").timeline("setInfeasible");
                    $("#RATimeline").timeline("updateSolvedResults");
                    resizeTimeperiods();
                    $("#RATimeline").timeline("resize");
                }
            } else {
                if (data.indexOf("[") !== 0) {
                    resizeTimeperiods();
                    $("#RATimeline").timeline("resize");
                }
            };
            tpShowState();
        };        
    }

    function exportAllocationComparision() {
        CreatePopup("<% = PageURL(CurrentPageID, String.Format("?{0}=export&type=alloc{1}&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" +  Math.round(100000*Math.random()), "export_alloc", "");
    }

    function onAllocTable() {
        on_ok_code = "onAllocateTableRecieved(data);";
        sendAJAX("<% = _PARAM_ACTION %>=alloc_table");
        PleaseWait(1, 1);
        showLoadingPanel();
    }

    var alloc_plain_data = "";
    var dlg_alloc = null;
    function onAllocateTableRecieved(data) {
        PleaseWait(0, 0);
        if (data!="" && data[0]=="<") {
            alloc_plain_data = htmlStrip(data);
            //var idx = plain_data.indexOf("\n");
            //if ((idx)) plain_data = plain_data.substr(idx+1);
            
            data += "<p align=right style='margin-bottom:0px; padding-bottom:0px;'>";
            data += "<img src='../../Images/ra/download-14.png' width=14 height=14 title='Download' border=0>&nbsp;<a href='' class='actions' onclick='exportAllocationComparision(); return false;'><span class='dashed'><% = JS_SafeString(ResString("lblRADownloadComparision")) %></span></a>";
            if (alloc_plain_data!="") {
                data += "<span style='float:left'><img src='../../Images/ra/copy-16.png' width=16 height=16 title='Copy to clipboard' border=0>&nbsp;<a href='' class='actions' onclick='copyDataToClipboard(alloc_plain_data); return false;'><span class='dashed'><% = JS_SafeString(ResString("titleCopyToClipboard")) %></span></a></span>";
            }
            data += "</p>";

            dlg_alloc = $("<div>" + data + "</div>").dialog({
                autoOpen: true,
                //width: "auto",
                //height: "auto",
                //width: 650,
                //height: 450,
                minWidth: 450,
                //maxWidth: 950,
                //minHeight: 250,
                //maxHeight: mh,
                modal: true,
                dialogClass: "no-close",
                closeOnEscape: true,
                bgiframe: true,
                title: "<% = JS_SafeString(ResString("lblRAAllocComparision")) %>",
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: {
                    Close: { id : "btnAllocClose", text: "<% = JS_SafeString(ResString("btnClose")) %>", click: function(e) { dlg_alloc.dialog("close"); } }
                },
                open: function() {
                    $("#btnAllocClose").focus();
                    $("body").css("overflow", "hidden");
                },
                close: function() {
                    $("body").css("overflow", "auto");
                }
            });


        } else {
            if (data!="") alert(data);
        }
    }

    function InitLogsDlg() {
        dlg_logs = $("#divSolverLogs").dialog({
            autoOpen: true,
            //width: "auto",
            //height: "auto",
            width: 650,
            height: 450,
            minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: mh,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("lblRALogs")) %>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Close: function() {
                    dlg_logs.dialog("close");
                }
            },
            open: function() {
                $("#txtLogs").val($("#tbLogs").val());
                $("body").css("overflow", "hidden");
            },
            close: function() {
                $("body").css("overflow", "auto");
            },
            resize: function( event, ui ) { $("#txtLogs").height(30); $("#txtLogs").height(Math.round(ui.size.height-115)); }
        });
    }

    function InitSolverDlg() {
        dlg_solver = $("#divSolverSettings").dialog({
            autoOpen: false,
            width: 350,
            height: 335,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("btnRASolver"))%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                OK: {
                    id: "btnSetSolver",
                    text: "<% = JS_SafeString(ResString("btnOK"))%>",
                    width: 65,
                    click: function() {
                        if (onSetSolver()) dlg_solver.dialog("close");
                    }
                },
                Reset: {
                    id: "btnResetSolver",
                    text: "<% = JS_SafeString(ResString("btnReset"))%>",
                    width: 65,
                    click: function() {
                        resetSolver();
                    }
                },
                Cancel: {
                    id: "btnCancelSolver",
                    text: "<% = JS_SafeString(ResString("btnCancel"))%>",
                    width: 65,
                    click: function() {
                        dlg_solver.dialog("close");
                    }
                }},
            open: function() {
                $("body").css("overflow", "hidden");
                showSolverSettings(solver_id);
            },
            close: function() {
                $("body").css("overflow", "auto");
            }
        });
    }

    function InitColumnsDlg() {
        dlg_columns = $("#divColumns").dialog({
            autoOpen: false,
            /*              width: 420,
                          height: 500,*/
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("lblRAShowColumns")) %>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Close: function() {
                    dlg_columns.dialog( "close" );
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
            },
            close: function() {
                $("body").css("overflow", "auto");
            }
        });
    }

    function InitScenariosDlg() {
        dlg_scenarios = $("#divScenarios").dialog({
            autoOpen: true,
            width: 650,
            minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: mh,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: false,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("lblRAPortfolioScenarios")) %>",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                  Close: function() {
                      if (checkUnsavedData(document.getElementById("tbName"), "dlg_scenarios.dialog('close')") && checkUnsavedData(document.getElementById("tbDescription"), "dlg_scenarios.dialog('close')")) dlg_scenarios.dialog( "close" );
                  }
              },
              open: function() {
                  $("body").css("overflow", "hidden");
                  on_hit_enter = "EditScenario(-1);";
                  document.getElementById("tbName").focus();
              },
              close: function() {
                  $("body").css("overflow", "auto");
                  InitScenarios();
                  $("#tblScenarios tbody").html("");
                  on_hit_enter = "";
                  $("#divScenarios").dialog("destroy");
                  dlg_scenarios = null;
                  if (scenario_order!="") {
                      sendAJAX("action=scenario_reorder&lst=" + scenario_order);
                      scenario_order = "";
                  }

              },
              resize: function( event, ui ) { $("#pScenarios").height(30); $("#pScenarios").height(Math.round(ui.size.height-150)); }
          });
          if ($("#pScenarios").height()>mh-150) $("#pScenarios").height(mh-150);
          setTimeout('$("#pScenarios").scrollTop(10000);', 30);
      }

      function GetCombinedGrpList(id, grp_id) {
          var lst = "<select id='group_" + id + "' style='width:98%'>";
          for (var i=0; i<c_groups.length; i++) {
              lst += "<option value='" + c_groups[i][0] + "'" + (c_groups[i][0] == grp_id ? " selected" : "") + ">" + c_groups[i][1] + "</option>";
          }
          lst += "</select>";
          return lst;
      }

      function InitScenarios() {
          $("#cntScenarios").html(scenarios.length);
          var s = theForm.cbScenarios;
          var t = $("#tblScenarios tbody");
          var w = dlgMaxWidth(1600);
          if (w>1400) $("#cbScenarios").width(180);
          if ((s))
          {
              s.options.length = 0;
              <% If Not Scenario.IsInfeasibilityAnalysis Then %>s.disabled = 0;<% End if %>
          }
          if ((t)) t.html("");
          for (var i=0; i<scenarios.length; i++) {
              //if ((s)) s.options[i] = new Option(scenarios[i][1], scenarios[i][0], (scenarios[i][0]==scenario_active), (scenarios[i][0]==scenario_active));
              if ((s)) s.options[i] = new Option(ShortString(scenarios[i][1], OPT_SCENARIO_LEN, false) + (scenarios[i][2] == "" ? "" : " (" + ShortString(scenarios[i][2], OPT_DESCRIPTION_LEN, false) + ")"), scenarios[i][0], (scenarios[i][0]==scenario_active), (scenarios[i][0]==scenario_active)); // A0965
              if ((t)) {
                  var is_active = (scenarios[i][0]==scenario_active);
                  var n = htmlEscape(scenarios[i][1]);
                  var sGrpName = "&nbsp;";
                  for (var j=0; j<c_groups.length; j++) {
                      if (c_groups[j][0] == scenarios[i][4]) sGrpName = c_groups[j][1];
                  }
                  if (is_active) n = "<b>" + n + "</b>"; else n = "<a href='' onclick='onSetScenario(" + scenarios[i][0] + "); return false;' class='actions'>" + n + "</a>";
                  //                sRow = "<tr class='text " + (is_active ? "grid_row_sel" : ((i&1) ? "grid_row" : "grid_row_alt")) + "'><td align='center' style='width:20px'><span class='drag_vert'>" + (is_active ? "<img src='<% =ImagePath %>active_small.png' width=10 height=10 border=0 alt='Active Scenario'>" : "&nbsp;") + "</span></td>";
                  sRow = "<tr class='text grid_row_dragable " + (is_active ? "grid_row_sel" : ((i&1) ? "grid_row" : "grid_row_alt")) + "'><td align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                  sRow += "<td id='tdName" + i + "''>" + n + "</td><td id='tdDesc" + i + "'>" + (scenarios[i][2] == "" ? "&nbsp;" : htmlEscape(scenarios[i][2])) + "</td>";
                  if (show_combined_grp) sRow += "<td id='tdGroup" + i + "''>" + sGrpName + "</td>";
                  sRow += "<td id='tdActions" + i + "' align='center'><a href='' onclick='onEditScenario(" + i + "); return false;'><img src='../../Images/ra/edit_small.gif' width=16 height=16 border=0 alt='Edit Scenario properties'></a>" + (is_active ? "" : "<a href='' onclick='onCopyScenario(" + i + "); return false;'><img src='../../Images/ra/copy_here.gif' width=16 height=16 border=0 alt='Copy Active Scenario here'></a>") + (scenarios[i][0]==0 ? "" : "&nbsp;<a href='' onclick='DeleteScenario(" + scenarios[i][0] + "); return false;'><img src='../../Images/ra/recycle.gif' width=16 height=16 border=0 alt='Delete Scenario'></a>") + "</td></tr>";
                t.append(sRow);
            }
        }
        if ((t)) {
            sRow = "<tr class='text grid_footer' id='trNew'><td>&nbsp;</td>";
            sRow += "<td><input type='text' class='input' style='width:100%' id='tbName'></td>";
            sRow += "<td><input type='text' class='input' style='width:100%' id='tbDescription' value=''></td>";
            if (show_combined_grp) sRow += "<td>" + GetCombinedGrpList(-1, -1) + "</td>";
            sRow += "<td align='center'><a href='' onclick='EditScenario(-1); return false;'><img src='../../Images/ra/add-16.png' width=16 height=16 border=0 alt='<% = JS_SafeString(ResString("lblRAAddScenario")) %>'></a></td></tr>";
            t.append(sRow);
            if ((dlg_scenarios) && dlg_scenarios.dialog("isOpen")) on_hit_enter = "EditScenario(-1);";
            // $("#tbDescription").TextAreaExpander(18, 150);
        }        
        setTimeout("if (document.getElementById('tbName')) document.getElementById('tbName').focus();", 80);
    }

    function onCopyScenario(idx) {
        dxConfirm(replaceString("{0}", scenarios[idx][1], "<% = JS_SafeString(ResString("confRACopyScenario")) %>"), "CopyScenario(" + idx + "," + scenario_active + ");");
    }

    function CopyScenario(idx_dest, id_src) {
        dlg_scenarios.dialog('close');
        var msg = replaceString('{0}', scenarios[idx_dest][1], '<% = JS_SafeString(ResString("confRAonScenarioCopied")) %>');
        on_ok_code = "dxConfirm('" + msg + "', 'onSetScenario(" + idx_dest + ");');";
        sendCommand("action=copy_scenario&from=" + id_src + "&to=" + scenarios[idx_dest][0], 0);
    } 

    function onEditScenario(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbName"), "onEditScenario(" + index + ", true)") && !checkUnsavedData(document.getElementById("tbDescription"), "onEditScenario(" + index + ", true)")) return false;
        InitScenarios();
        $("#tdName" + index).html("<input type='text' class='input' style='width:" + $("#tdName" + index).width()+ "' id='tbName' value='" + replaceString("'", "&#39;", scenarios[index][1]) + "'>");
        $("#tdDesc" + index).html("<input type='text' class='input' style='width:" + $("#tdDesc" + index).width()+ "' id='tbDescription' value='" + replaceString("'", "&#39;", scenarios[index][2]) + "'>");
        $("#tdGroup" + index).html(GetCombinedGrpList(index, scenarios[index][4]));
        $("#tdActions" + index).html("<a href='' onclick='EditScenario(" + index + "); return false;'><img src='../../Images/ra/apply-16.png' width=16 height=16 border=0 alt='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></a>&nbsp;<a href='' onclick='InitScenarios(); document.getElementById(\"tbName\").focus(); return false;'><img src='../../Images/ra/cancel-16.png' width=16 height=16 border=0 alt='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></a>");
        $("#trNew").html("").hide();
        setTimeout("document.getElementById('tbName').focus();", 50);
        on_hit_enter = "EditScenario(" + index + ");";
        //$("#tbDescription").TextAreaExpander(18, 150);
    }

    var scen_add_copy = 1;

    function EditScenario(index) {
        var n = document.getElementById("tbName");
        var d = document.getElementById("tbDescription");
        var g = document.getElementById("group_" + index);
        if ((n) && (d) && ((g) || !show_combined_grp) ) {
            if ((n.value=='' || n.value.trim()=='')) {
                dxDialog("<% = JS_SafeString(ResString("errRAScenarioNameEmpty")) %>", "setTimeout(\"document.getElementById('tbName').focus();\", 150);");
            } else {
                var idx = scenarios.length-1;
                if (index>=0) {
                    idx = scenarios[index][0];
                    scenarios[index][1] = n.value;
                    scenarios[index][2] = d.value;
                    if ((g)) scenarios[index][4] = g.value;
                    SaveScenario(index, idx);
                } else {
                    for (var i=0; i<scenarios.length; i++) {
                        if (scenarios[i][0]>idx) idx=scenarios[i][0];
                    }
                    idx+=1;
                    scenarios[scenarios.length]=[idx, n.value, d.value, idx, ((g) ? g.value : -1)];
                    var act = "SaveScenario(" + index +", " + idx + ");";
                    dxConfirm("<% = JS_SafeString(ResString("confRACopyScenarioSettings")) %>", "scen_add_copy = 1; " + act, "scen_add_copy = 0; " + act);
                }
            }
        }
    }

    function SaveScenario(index, idx) {
        for (var i=0; i<scenarios.length; i++) {
            if (scenarios[i][0] == idx) {
                if (index>=0) InitScenarios();
                on_ok_code = (index<0 ? "dlg_scenarios.dialog('close'); PleaseWait(1,0); onSetScenario(" + idx + ");" :  "document.getElementById('tbName').focus();");
                sendCommand("action=edit_scenario&id=" + (index>=0 ? idx : "-1&copy=" + ((scen_add_copy) ? "1" : "0")) + "&name=" +  enc(scenarios[i][1])+ "&grp=" + scenarios[i][4] + "&desc=" + enc(scenarios[i][2]), 0);
            }
        }
    }

    function DeleteScenario(id) {
        dxConfirm("<% = JS_SafeString(ResString("confRADeleteScenario")) %>", "on_ok_code = 'onDeleteScenario(" + id + ");'; sendCommand('action=delete_scenario&id=" + id + "',0);");
    }

    function onDeleteScenario(id) {
        var l = [];
        for (var i=0; i<scenarios.length; i++) {
            if (scenarios[i][0]!=id) l.push(scenarios[i]);
        }
        scenarios = l;
        InitScenarios();
        if (id==<% =RA.Scenarios.ActiveScenarioID %>) { dlg_scenarios.dialog('close'); document.location.href='<% =PageURL(CurrentPageID) %>?<% =GetTempThemeURI(False) %>'; }
    }

    //    var init_tries = 0;
    //    function InitSolve() {
    //        init_tries += 1;
    //        var s = getToolbarButton("solve");
    //        if ((s)) {
    //            var d = s.get_element();
    //            if ((d)) {
    //                d.style.styleFloat = d.style.cssFloat = "right";
    //                d.style.display = "inline";
    //            }
    //        } else {
    //            if (init_tries < 10) setTimeout("InitSolve();", 50);
    //        }
    //    }

    function DoSort(idx, dir) {
        showLoadingPanel();
        sendCommand("action=sorting&fld=" + idx + "&dir=" + dir, 1);
        return false;
    }

    var flashed = false;

    function doFlashChanges() {
        if (!flashed) {
            var del = 300;
            var hc = "#3399ff";
            var nc = "#000000";
            $(".flash").animate({color: hc}, del).animate({color: nc}, del).animate({color: hc}, del).animate({color: nc}, del).animate({color: hc}, del).animate({color: "#003399"}, 2*del);
            $(".tohide").delay(8*del).fadeOut(del*2);
            flashed = true;
        }
    }

    function onResize() {
        var s = getToolbarButton("sync");
        var h = getToolbarButton("help");
        if ((s) && (h)) {
            s.set_visible(false);
        }
        resizeGlobal();
        if (view_grid) resizeGrid(); else resizeTimeperiods();
        var d = $("#tdGrid")
        if ((s) && (h) && (d)) {
            s.set_visible(true);
            //var oh = h.get_element();
            var ot = s.get_element();
            if ((ot) && (ot.parentNode.clientWidth>d.width())) s.set_visible(false);
            //if ((oh) && (ot) && ((oh.offsetLeft + oh.offsetWidth + ot.offsetWidth + 16) < d.width())) s.set_visible(true);
        }
    }

    function resizeTimeperiods() {
        $("#RATimeline").height(100).height($("#tdGridAlts").height()).width(300).width($("#tdGridAlts").width()-2);
    }

    function resizeGlobal() {

        var tg = $("#tdGrid");
        var tga = $("#tdGridAlts");
        var dga = $("#divGridAlts");
        var tb = $("#divToolbar");
        var dg = $("#<% = divGrid.ClientID %>");

        dga.height(10).width(10);
        tga.width(10);
        tb.width(1);
        dg.height(10).width(10);

        //        var hm = tg.height() - (is_chrome ? 24 : 2);
        var hm = tg.height() - 2;
        var wm = tg.width() - 2;

        dg.height(hm).width(wm);
        tb.width(wm);
        tga.width(wm-4);
        dga.width(tga.width()).height(tga.height()).css("oveflow", (use_scrolling ? "hidden" : "auto"));

        $('input.as_number').attr('autocomplete','off').attr('autocorrect','off').attr('autocapitalize','off');

        if ((theForm.tbBudgetLimit) && !(theForm.tbBudgetLimit.disabled)) {
            $('#tbBudgetLimit').unbind("contextmenu").bind('contextmenu', function(e){
                e.preventDefault();
                //                $("#divBudgetLimitCMenu").css({top: e.pageY + "px", left: e.pageX + "px"}).fadeIn(300); 
                showCMenu(); //A0937                
                setTimeout("fadeCMenuHandler();", 500); //A0937                
            }).attr('autocomplete','off').attr('autocorrect','off').attr('autocapitalize','off');
            //-A0937 setTimeout("theForm.tbBudgetLimit.title='<% =JS_SafeString(ResString("lblRClickMenu")) %>';", 350);
            //A0937 ===
            $('#tbBudgetLimit').unbind("click").bind('click', function(e){
                var offset = $(this).offset();
                if ((e.pageX - offset.left >= 72) && (e.pageY - offset.top > 2)) {
                    showCMenu(); //A0937                
                    setTimeout("fadeCMenuHandler();", 500);                    
                }
            }).attr('autocomplete','off').attr('autocorrect','off').attr('autocapitalize','off');
            //A0937 ==
        }        

        onSearch($("#tbSearch").val().trim(), true);
        doFlashChanges();
        UpdateOpt();
        switchBaseCase();
        UpdateButtons();
    }

    function resizeGrid() {
        var dga = $("#divGridAlts");

        var g = $('#<% =GridAlternatives.ClientID %>');
        if ((g) && use_scrolling) {
            var k = (is_chrome ? 18 : 0);
            gridView1 = g.gridviewScroll({
                width: dga.width()-k,
                height: dga.height()-k,
                railcolor: "#F0F0F0",
                barcolor: "#CDCDCD",
                barhovercolor: "#606060",
                bgcolor: "#F0F0F0",
                arrowsize: 30,
                varrowtopimg: "../../Images/ra/arrowvt.png",
                varrowbottomimg: "../../Images/ra/arrowvb.png",
                harrowleftimg: "../../Images/ra/arrowhl.png",
                harrowrightimg: "../../Images/ra/arrowhr.png",
                freezesize: ((theForm.cbColumn0) && (theForm.cbColumn0.checked) ? 2 : 1),
                headerrowcount: 1,
                railsize: 16,
                barsize: 12,
                startVertical: $("#<%=hfGridView1SV.ClientID%>").val(), 
                startHorizontal: $("#<%=hfGridView1SH.ClientID%>").val(),
                onScrollVertical: function (delta) { 
                    $("#<%=hfGridView1SV.ClientID%>").val(delta); 
                }, 
                onScrollHorizontal: function (delta) { 
                    $("#<%=hfGridView1SH.ClientID%>").val(delta); 
                } 
            });
        }
    }

    function showCMenu() {
        $("#divBudgetLimitCMenu").css({top: ($('#tbBudgetLimit').position().top+15) + "px", left: ($('#tbBudgetLimit').position().left+74) + "px"}).fadeIn(500); //-A0937 show("fold", 250); 
    }

    function fadeCMenuHandler() {
        $(document).bind("click", function(event) { $("#divBudgetLimitCMenu").fadeOut(200); $(document).unbind("click"); });
    }

    function onBudgetLimitContext(event) {

    }

    function reload() {
        PleaseWait(1);
        document.location.reload();
    }

    var drag_index = -1;
    var scenario_order = "";
    var sprty_order = "";

    function onDragIndex(new_idx) {
        if (new_idx>=0 && drag_index>=0 && new_idx!=drag_index) {
            if (dlg_scenarios) {
                var el = scenarios[drag_index];
                scenarios.splice(drag_index, 1);
                scenarios.splice(new_idx, 0, el);
                scenario_order = "";
                for (var i=0; i<scenarios.length; i++) {
                    scenarios[i][3] = i+1;
                    scenario_order += (i==0 ? "" : ",") + scenarios[i][0];
                }
                drag_index = -1;
                InitScenarios();
                setTimeout("document.getElementById('tbName').blur();", 60);
                setTimeout("document.getElementById('tbName').focus();", 100);
            }
            if (dlg_solverprty) {
                var el = solver_priorities[drag_index];
                solver_priorities.splice(drag_index, 1);
                solver_priorities.splice(new_idx, 0, el);
                sprty_order = "";
                for (var i=0; i<solver_priorities.length; i++) {
                    solver_priorities[i][0] = i+1;
                    sprty_order += (i==0 ? "" : ",") + solver_priorities[i][1];
                }
                drag_index = -1;
                InitSolverPrty();
            }
        }
    }

    function InitPage() {
        //        setTimeout("InitSolve();", 50);
        $("#tdGridAlts").show();
        InitScenarios();    // for init combobox and counter
        onResize();
        InitSearch();
        checkPageSettings();

        document.getElementById('cbShowConstraints2').checked = <% = IIf(RA.Scenarios.GlobalSettings.ShowCustomConstraints, "1", "0")%>;
        $get('cbShowAttributes').checked = <% = IIf(ShowAltAttributes, "1", "0") %>; // A1010
        setTimeout('CheckAssociatedRisk(); CheckSelAlts(); UpdateButtons(); theForm.tbBudgetLimit.focus();', 300);
        if (startup_msg!="") setTimeout('dxDialog(startup_msg, ";"); startup_msg="";', 650);
        $('.rtbChoiceArrow').css("margin-left", "0px");

        $(function () {
            $(".drag_drop_grid").sortable({
                items: 'tr.grid_row_dragable',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid',
                axis: 'y',
                start: function( event, ui ) { drag_index = ui.item.index(); },
                update: function( event, ui ) { onDragIndex(ui.item.index()); }
                //                dropOnEmpty: true
                //                receive: function (e, ui) {
                //                    $(this).find("tbody").append(ui.item);
                //                }
            });
            //        $("[id*=gvDest] tr:not(tr:first-child)").remove();
        });

        <% If Scenario.IsInfeasibilityAnalysis Then %>
        $("input[type=text]").prop("disabled", "disabled");
        $("input[type=checkbox]").prop("disabled", "disabled");
        $("#btnUseAll").prop("disabled", "disabled");
        $("#btnIgnoreAll").prop("disabled", "disabled");
        <% End If %>

        if (!view_grid) UpdateTimeperiods(true);
        setTimeout('hideLoadingPanel();', 150);
    }

    function onSetScenario(id) {
        //        PleaseWait(1);      //A0903
        showLoadingPanel();
        document.location.href='<% =PageURL(CurrentPageID) %>?action=scenario<% =GetTempThemeURI(True) %>&sid='+ id;
        return false;
    }

    function onChangeSettings(name, chk) {
        //        var d = getToolbarButton("Settings");
        //        if ((d)) d.hideDropDown();
        fundedonly = (chk);
        if (name=="scrolling" && view_grid) on_ok_code = "reload();";
        sendCommand("action=settings&name=" + name + "&chk=" + ((chk) ? 1 : 0), 1);
        if (!view_grid && name == "showfunded") {
            $("#RATimeline").timeline("option", "fundedOnly", fundedonly).timeline("resize");
        }
    }

    function switchBaseCase() {
        var bc = theForm.cbBaseCase;
        if ((bc)) {
            theForm.cbBCGroups.disabled = !bc.checked;
        }
    }

    /* Copy to Clipboard - Cross-Browser */
    <%--function copyDataToClipboard(data) {
        if (window.clipboardData) {
            if (window.clipboardData.setData('Text', data)) {
                DevExpress.ui.notify(resString("msgDataCopied"), "success");
            } else {
                DevExpress.ui.notify(resString("msgUnableToCopy"), "error");                
            }
        } else {
            if (is_firefox) {
                dxDialog("<%=ResString("titleNonIECopy") %>:<pre><textarea id='copyBox' cols='48' rows='6'>" + data + "</textarea></pre>", ";", ";", "<%=ResString("titleCopyToClipboard") %>", "<%=ResString("btnCopy") %>", "<%=ResString("btnCancel") %>");
                $("#copyBox").select();
            } else {
                var success = chromeCopyToClipboard(data);
                DevExpress.ui.notify(success ? resString("msgDataCopied") : resString("msgUnableToCopy"), success ? "success" : "error");
            }
        }
    }--%>

    function copyColumn(periodID) {
        var data = '';
        for (i = 0; i < projects.length; i++) {
            var prj = projects[i];
            var value = prj.resources[$("#RATimeline").timeline('option', 'resID')].values[periodID - prj.periodStart];
            if ((typeof(value) == "undefined")||(value == -2147483648)) {value = ' '};
            data += (data == "" ? "" : "\r\n") + value;
        };
        copyDataToClipboard(data);
    }
    
    function pasteColumn(periodID) {
        var data = "";
        var pastedata = '';
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            if (typeof data != "undefined" && data != "") {
                pasteperioddata = data.replace(/(?:\r\n|\r|\n)/g, '\n');
                $("#RATimeline").timeline("pastecolumn", pasteperioddata, periodID);
            } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
        } else {
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChromeTimeline('" + periodID + "');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
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
                if ((typeof(value) == "undefined")||(value == -2147483648)) {value = ' '};
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

    //A0937 ===
    var is_context_menu_open = false;

    function showMenu(event, uid, hasCopyButton, hasPastButton, SyncID, isDisabledCC) {
        is_context_menu_open = false;                
        $("#contextmenuheader").hide().remove();
        var isCC = (uid >= <% =raColumnID.CustomConstraintsStart %> && uid < <% = raColumnID.CustomConstraintsStart + Scenario.Constraints.Constraints.Count %>);
        var can_edit = (!isCC || !isDisabledCC || <% =IIf(RA_OPT_CC_EDIT_DISABLED, 1, 0) %>);
        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
        if (hasCopyButton) sMenu += "<a href='' class='context-menu-item' onclick='doCopyToClipboardValues(\"" + uid + "\"); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/copy2-16.png' alt='' >&nbsp;<%=ResString("titleCopyToClipboard")%>&nbsp;</nobr></div></a>";
        if (hasPastButton && can_edit) sMenu += "<a href='' class='context-menu-item' onclick='doPasteAttributeValues(\""+ uid + "\"); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/paste-16.png' alt='' >&nbsp;<%=ResString("titlePasteFromClipboard")%>&nbsp;</nobr></div></a>";
        if (SyncID!="" && can_edit) sMenu += "<div class='context-menu-item-hr' width=230>&nbsp;</div><a href='' class='context-menu-item' onclick='doSyncConstr(\""+ SyncID + "\"); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/refresh-16.png' alt='' >&nbsp;<%=ResString("titleRASyncCC")%>&nbsp;</nobr></div></a>";
        if (isCC) sMenu += "<div class='context-menu-item-hr' width=230>&nbsp;</div><a href='' class='context-menu-item' onclick='doSwitchConstr(\"" + (uid-<% =raColumnID.CustomConstraintsStart %>) +"\"," + (isDisabledCC ? 1 : 0) +"); return false;'><div style='padding:1px;'><nobr><!--img align='left' style='vertical-align:middle;' src='../../Images/ra/checkbox_" + (isDisabledCC ? "no" : "yes") + ".png' alt='' /--><input type='checkbox' id='cbIgnore" + uid + "'" + (isDisabledCC ? " checked" : "") + "><label for='cbIgnore" + uid + "'>&nbsp;<%=JS_SafeString(ResString("tblRACCEnabled"))%>&nbsp;</label></nobr></div></a>";
        if (uid==<% = raColumnID.Groups %>) sMenu += "<div class='context-menu-item-hr' width=230>&nbsp;</div><a href='' class='context-menu-item' onclick='doSwitchGroupDetails(); return false;'><div style='padding:1px;'><nobr><input type='checkbox' id='cbGroupDetails'" + (OPT_GROUP_DETAILS ? " checked" : "") + "><label for='cbGroupDetails'>&nbsp;<%=JS_SafeString(ResString("lblRAGoupDetails"))%>&nbsp;</label></nobr></div></a>";
        sMenu += "</div>";                
        var img = document.getElementById("mnu_img_" + uid);
        if ((img)) {
            var rect = img.getBoundingClientRect();
            var x = rect.left+ 2;
            var y = rect.top + 12;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});                        
            if ((s)) { var w = s.width();var pw = $("#divMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

            $("#contextmenuheader").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }
    }

    function showMenuCB(event, uid) {
        is_context_menu_open = false;                
        $("#contextmenuheader").hide().remove();
        var mn = (uid == <% = CInt(raColumnID.MustNot) %>);
        var part = (uid == <% = CInt(raColumnID.isPartial) %> || uid == <% = CInt(raColumnID.isCostTolerance) %>);
        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
        sMenu += "<a href='' class='context-menu-item' onclick='doCopyToClipboardCB(" + uid + "); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/copy2-16.png' width=16 height=16 alt='' >&nbsp;<%=JS_SafeString(ResString("titleCopyToClipboard"))%>&nbsp;</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='doPasteCBValues("+ uid + "); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/paste-16.png' width=16 height=16 alt='' >&nbsp;<%=JS_SafeString(ResString("titlePasteFromClipboard"))%>&nbsp;</nobr></div></a>";
        sMenu += "<div class='context-menu-item-hr' width=180>&nbsp;</div>";
        if (solverStateID() == <% = CInt(raSolverState.raSolved)%>) {
            if (!part) sMenu += "<a href='' class='context-menu-item' onclick='doCreateScenarioFunded(); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/scenario_add.png' width=16 height=16 alt='' >&nbsp;<%=JS_SafeString(ResString("lblSpecificPortfolio"))%>&nbsp;</nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='doSelectCB(" + uid + ",2); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/" + (mn ? "checkbox_white.png" : "checkbox_yellow.png") + "' width=16 height=16 alt='' >&nbsp;" + (mn ? "<%=JS_SafeString(ResString("lblCheckNonFunded"))%>" : "<%=JS_SafeString(ResString("lblCheckFunded"))%>") + "&nbsp;</nobr></div></a>";
        } else {
            if (!part) sMenu += "<div class='context-menu-item-inactive'><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/scenario_add_.png' width=16 height=16 alt='' >&nbsp;<%=ResString("lblSpecificPortfolio")%>&nbsp;</nobr></div>";
            sMenu += "<div class='context-menu-item-inactive'><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/checkbox_yellow_.png' width=16 height=16 alt='' >&nbsp;<%=ResString("lblCheckFunded")%>&nbsp;</nobr></div>";
        }
        sMenu += "<div class='context-menu-item-hr' width=180>&nbsp;</div>";
        sMenu += "<a href='' class='context-menu-item' onclick='doSelectCB(" + uid + ", 1); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/checkbox_yes.png' width=16 height=16 alt='' >&nbsp;<%=JS_SafeString(ResString("lblCheckAll"))%>&nbsp;</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='doSelectCB(" + uid + ", 0); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/checkbox_no.png' width=16 height=16 alt='' >&nbsp;<%=JS_SafeString(ResString("lblUnCheckAll"))%>&nbsp;</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='doSelectCB(" + uid + ", -1); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../Images/ra/checkbox_diag.png' width=16 height=16 alt='' >&nbsp;<%=JS_SafeString(ResString("lblInvertAll"))%>&nbsp;</nobr></div></a>";
        sMenu += "</div>";
        var img = document.getElementById("mnu_img_" + uid);
        if ((img)) {
            var rect = img.getBoundingClientRect();
            var x = rect.left+ 2;
            var y = rect.top + 12;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});                        
            if ((s)) { var w = s.width();var pw = $("#divMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

            $("#contextmenuheader").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }
    }

    function doSelectCB(col_id, sel) {
        $("#contextmenuheader").hide(200); is_context_menu_open = false;
        var guids = "";
        switch (col_id*1) {
            case <% =Cint(raColumnID.Musts) %>:
            case <% =Cint(raColumnID.MustNot) %>:
                var name = (col_id*1 == <% =Cint(raColumnID.Musts) %> ? "musts" : "mustnot");
                break;
            case <% =Cint(raColumnID.isPartial) %>:
                var name = "partial";
                break;
            case <% =CInt(raColumnID.isCostTolerance) %>:
                var name = "istolerance";
                break;
        }
        for (var i=0; i<alts_all.length; i++) {
            var checked = (sel!=0);
            if (sel==2) {
                var tr = $("#tr_" + i);
                checked = ((tr) && (tr.length) && (tr.attr("is_funded")=="1"));
                if (name=="mustnot") checked = !checked;
            }
            //if (sel!=2 || (checked)) {
                switch (col_id*1) {
                    case <% =Cint(raColumnID.Musts) %>:
                    case <% =Cint(raColumnID.MustNot) %>:
                        var c = $("[data_id='" + name + i + "']");
                        var c2 = $("[data_id='" + (name=="musts" ? "mustnot" : "musts") + i + "']");
                        if ((c) && (c.length) && (c2) && (c2.length)) {
                            if (sel==-1) checked = !(c.prop('checked'));
                            c.prop('checked', checked);
                            guids += (guids=="" ? "" : "_") + ((sel!=0) && !(checked) ? "!" : "") + c.attr("data_guid");
                            onMustSet(checked, c2.attr("id"));
                        }
                        break;
                    case <% =Cint(raColumnID.isPartial) %>:
                        var c = $("[data_id='partial" + i + "']");
                        var e = $("[data_id='partial_edit" + i + "']");
                        if ((c) && (c.length) && (e) && (e.length)) {
                            if (sel==-1) checked = !(c.prop('checked'));
                            c.prop('checked', checked);
                            guids += (guids=="" ? "" : "_") + ((sel!=0) && !(checked) ? "!" : "") + c.attr("data_guid");
                            onPartialChk(e.attr("id"), checked, false);
                        }
                        break;
                    case <% =CInt(raColumnID.isCostTolerance) %>:
                        var c = $("[data_id='istolerance" + i + "']");
                        var e = $("[data_id='istolerance_edit" + i + "']");
                        if ((c) && (c.length) && (e) && (e.length)) {
                            if (sel==-1) checked = !(c.prop('checked'));
                            c.prop('checked', checked);
                            guids += (guids=="" ? "" : "_") + ((sel!=0) && !(checked) ? "!" : "") + c.attr("data_guid");
                            onPartialChk(e.attr("id"), checked, false);
                        }
                        break;
                }
            //}
        }
        if (guids!="") return sendCommand("action=" + name + "&val=" + ((sel!=0) ? 1 : 0) + "&guids_lst=" + guids, 0); else return false;
    }

    function doCopyToClipboardCB(id) {
        $("#contextmenuheader").hide(200); is_context_menu_open = false;
        var res = "";
        switch (id*1) {
            case <% =Cint(raColumnID.Musts) %>:
                var name = "musts";
                break;
            case <% =Cint(raColumnID.MustNot) %>:
                var name = "mustnot";
                break;
            case <% =Cint(raColumnID.isPartial) %>:
                var name = "partial";
                break;
            case <% =CInt(raColumnID.isCostTolerance) %>:
                var name = "istolerance";
                break;
        }
        for (var i=0; i<alts_all.length; i++) {
            var c = $("[data_id='" + name + i + "']");
            if ((c) && (c.length)) {
                res += (res==""?"":"\r\n") + (c.prop("checked") ? "1" : "0");
            }
        }
        copyDataToClipboard(res);        
    }

    function doPasteCBValues(id) {
        $("#contextmenuheader").hide(200); is_context_menu_open = false;
        var data = "";
<%--        switch (id*1) {
            case <% =Cint(raColumnID.Musts) %>:
                var name = "musts";
                break;
            case <% =Cint(raColumnID.MustNot) %>:
                var name = "mustnot";
                break;
            case <% =Cint(raColumnID.isPartial) %>:
                var name = "partial";
                break;
        }--%>
        if (window.clipboardData) { 
            data = window.clipboardData.getData('Text');
            pasteData(id, data);
        } else { 
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome(" + id + ");", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
        };                
    }

    function doCreateScenarioFunded() {
        $("#divAddScenario").dialog({
            autoOpen: true,
            width:450,
            //              height: 500, //(lst.length>10 ? 400 : "auto"),
            /*minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: 500,*/
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("lblSpecificPortfolio")) %>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                OK: {
                    id: "btnSPAddScenario",
                    text: "<% = JS_SafeString(ResString("btnCreate")) %>",
                    //disabled: true,
                    click: function() {
                        if (onSaveSpecificPortfolio()) $("#divAddScenario").dialog("close");
                    }
                },
                Cancel: function() {
                    $("#divAddScenario").dialog("close");
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
                $("#tbSPName").focus();
            },
            close: function() {
                on_hit_enter = "";
                $("body").css("overflow", "auto");
                //                $("#divModelsList").dialog("destroy");
            }
        });
        on_hit_enter = "$('#btnSPAddScenario').click();";
    }

    function onSaveSpecificPortfolio() {
        var n = $("#tbSPName").val();
        var d = $("#tbSPDesc").val();
        var m = ($("#cbCopyMusts").prop("checked"));
        var mn = ($("#cbCopyMustNot").prop("checked"));
        if (n=="") {
            dxDialog("<% = JS_SafeString(ResString("errRAScenarioNameEmpty")) %>", "setTimeout(\"document.getElementById('tbSPName').focus();\", 150);");
            return false;
        }
        idx=0;
        for (var i=0; i<scenarios.length; i++) {
            if (scenarios[i][0]>idx) idx = scenarios[i][0];
        }
        on_ok_code = "onSetScenario(" + (idx+1) +");";
        var p = "action=edit_scenario&id=-1&name=" + encodeURIComponent(n) + "&desc=" + encodeURIComponent(d) + "&copy=1&mode=specific_portfolio&m=" + (m ? 1 : 0) + "&mn=" + (mn ? 1 : 0);
        sendCommand(p, 1);
        return true;
    }

    function canCloseMenu() {
        is_context_menu_open = true;
        $(document).unbind("click").bind("click", function () { if (is_context_menu_open == true) { $("#contextmenuheader").hide(200); is_context_menu_open = false; } });        
    }
    //A0937 ==
    //A0910 ===
    function doPasteAttributeValues(attr_idx) {
        $("#contextmenuheader").hide(200); is_context_menu_open = false;
        var data = "";
        if (window.clipboardData) { 
            data = window.clipboardData.getData('Text');
            pasteData(attr_idx, data);
        } else { 
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome('"+attr_idx+"');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
        };                
    }

    function pasteData(attr_idx, data) {
        if (typeof data != "undefined" && data != "") {
            sendCommand("action=paste_column&column="+attr_idx+"&data="+encodeURIComponent(data), 1);
        } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
    }

    function doCopyToClipboardValues(unique_id) {
        $("#contextmenuheader").hide(200); is_context_menu_open = false;
        var res = "";
        var grid = document.getElementById("<%=GridAlternatives.ClientID %>");
        var row_count = grid.rows.length;        
        //var cell_index = unique_id*1;
        var cells_len = grid.rows[0].cells.length;
        //if ((cell_index >= 0) && (cell_index < cells_len)) {
        for (var i=1;i<row_count;i++) { //skip header row           
            for (var k=0; k<cells_len; k++) {
                if ((grid.rows[i]) && (grid.rows[i].cells[k]) && (grid.rows[i].cells[k].getAttribute('clip_data_id') == unique_id)) {
                    var cell = grid.rows[i].cells[k];
                    var value= cell.getAttribute('clip_data').toString();
                    res += (res==""?"":"\r\n") + value;
                }
            }
        }
        //}
        copyDataToClipboard(res);
    }

    function commitPasteChrome(attr_idx) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(attr_idx, pasteBox.value);
        }
    }

    function commitPasteChromeTimeline(periodID) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            var data = pasteBox.value;
            if (typeof data != "undefined" && data != "") {
                pasteperioddata = data.replace(/(?:\r\n|\r|\n)/g, '\n');
                $("#RATimeline").timeline("pastecolumn", pasteperioddata, periodID);
            } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
        }
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
    //A0910 ==

    function doSyncConstr(ids) {
        sendCommand("action=sync_constr&ids=" + ids, 1);
    }

    function showGroupDetails() {
        if (OPT_GROUP_DETAILS) {
            $(".grp_title").show();
            $(".grp_id").hide();
        } else {
            $(".grp_title").hide();
            $(".grp_id").show();
        }
    }

    function doSwitchGroupDetails() {
        OPT_GROUP_DETAILS = !OPT_GROUP_DETAILS;
        document.cookie = '<% = COOKIE_GROUP_DETAILS %>=' + (OPT_GROUP_DETAILS ? 1 : 0) + ";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT;";
        showGroupDetails();
    }

    function doSwitchConstr(id, enabled) {
        sendCommand("action=enable_constraint&id=" + id + "&chk=" + (enabled ? 1 :0), 1);
    }

    function onSetSolverGurobi() {
        <% If RA.Solver.OPT_GUROBI_USE_CLOUD Then %>dxConfirm("<% = JS_SafeString(ResString("msgRASwitchGurobi")) %>", "setSolverXAConf(); showSolverSettings(solver_id_gurobi);", "showSolverSettings(" + solver_id + ");", "<% =JS_SafeString("Confirmation")%>", "<% = JS_SafeString(ResString("btnContinue"))%>", "<% = JS_SafeString(ResString("btnCancel"))%>");<% Else %>showSolverSettings(solver_id_gurobi);<% End If%>
    }

    function onSetSolver() {
        var adv = "";
        solver_id = $("input:radio[name='cbSolver']:checked").val();
        if (solver_id == solver_id_xa) {
            solver_xa_strategy = $("#xa_strategy").val();
            solver_xa_variance = $("#xa_variation").val();
            solver_xa_timeout = $("#xa_timeout").val();
            solver_xa_timeout_unchanged = $("#xa_timeout_unch").val();
            adv += "&strategy=" + solver_xa_strategy + "&variation=" + solver_xa_variance + "&timeout=" + solver_xa_timeout + "&timeout_unch=" + solver_xa_timeout_unchanged;
        }
        on_ok_code = "UpdateButtons(); UpdateOpt();";
        sendCommand("action=solver&lib=" + solver_id + adv, 1);
        return true;
    }

    function resetSolver() {
        var id = solver_id;
        var s = solver_xa_strategy;
        var v = solver_xa_variance;
        var t = solver_xa_timeout;
        var u = solver_xa_timeout_unchanged;

        solver_xa_strategy = 1;
        solver_xa_variance = 0;
        solver_xa_timeout = 120;
        solver_xa_timeout_unchanged = 30;
        showSolverSettings(solver_id_xa);

        solver_id = id;
        solver_xa_strategy = s;
        solver_xa_variance = v;
        solver_xa_timeout = t;
        solver_xa_timeout_unchanged = u;
    }

    function createComboBox(id, params, active) {
        var lst = "";
        for (var i=0; i<params.length; i++) {
            var v = params[i];
            lst += "<option value='" + v + "'" + (v==active ? " selected" : "")+">" + v + "</option>";
        }
        if (lst!="") lst = "<select id='" + id + "' style='width:100%'>" + lst + "</option>";
        return lst;
    }

    function showSolverSettings(id) {
        $("input:radio[name='cbSolver'][value=" + id + "]").prop('checked', true);
        if (id == solver_id_xa) {
            $("#divSolverNoSettings").hide();
            $("#divSolverXA").show();
            $("#xa_strategy").prop("value", solver_xa_strategy);
            $("#xa_variation").prop("value", solver_xa_variance);
            $("#tdXATimeout").html(createComboBox("xa_timeout", solver_timeouts, solver_xa_timeout));
            $("#tdXATimeoutUnch").html(createComboBox("xa_timeout_unch", solver_timeouts_unchanged, solver_xa_timeout_unchanged));
        } else  {
            $("#divSolverNoSettings").show();
            $("#divSolverXA").hide();
        }
    }

    function noFPWarning(warn) {
        sendAJAX("action=no_fp_warn");
        hideFPWarning();
    }

    // === timeperiods ===

    function hideFPWarning() {
        $("#divFPWarning").hide();
    }

    function switchTP(idx) {
        var vis = ($("#tp_" + idx + "_hdr").css('display') == 'none');
        $("#cal_img_" + idx).prop("src", "../../Images/ra/" + ((vis) ? "calendar_12.png" : "calendar_12_.png"));
        $("*[id^='tp_" + idx + "']").toggle();
        if (view_grid && isFreezeView()) on_ok_code = "reload();";
        sendAJAX("action=tp_switch&id=" + idx + "&vis=" + ((vis) ? "1" : "0"));
        UpdateButtons();
    }

    function switchAllTP(idx) {
        var e =theForm.cbOptExpandTP;
        if ((e)) {
            var chk = e.checked;
            //var indet = $("#cbOptExpandTP").prop("indeterminate");
            var vis = (chk);
            for (var i=0; i<tp_lst.length; i++) {
                $("#cal_img_" + tp_lst[i]).prop("src", "../../Images/ra/" + ((vis) ? "calendar_12.png" : "calendar_12_.png"));
                var c = $("*[id^='tp_" + tp_lst[i] + "']");
                if ((vis)) c.show(); else c.hide();
            }
            if (view_grid && isFreezeView()) on_ok_code = "reload();";
            sendAJAX("action=tp_switch_all&vis=" + ((vis) ? "1" : "0"));
            UpdateButtons();
        }
    }

    function switchTPDistribute(val) {
        $("#RATimeline").timeline("option", "distribMode", val);
        $("#RATimeline").timeline("resize");
        sendAJAX("action=tp_distr&val=" + val);
    }

    function onChangeTimeperiods(chk) {
        view_grid = ((chk) ? 0 :1);
        UpdateButtons();
        on_ok_code = "UpdateTimeperiods(true);";
        if (view_grid) sendCommand('action=timeperiods&val=0', 1); else sendAJAX("action=timeperiods&val=" + ((chk) ? 1 : 0));
    }

    function tpShowState() {
        if (!view_grid) {
            var dis = (theForm.cbOptTimePeriods) && (theForm.cbOptTimePeriods.checked);
            var t  = $find("<% =tooltipTPIgnored.ClientID %>");
            if (dis) {
                if ((t)) t.show();
                $("#divRATimelineOverride").height($("#RATimeline").height()).css('opacity', '0.75').show();
                $("#RATimeline").prop("disabled", "disabled");
            } else {
                $("#divRATimelineOverride").hide();
                if ((t)) t.hide();
                $("#RATimeline").prop("disabled", "");
            }
        }
        <% If Scenario.IsInfeasibilityAnalysis Then %>
        $("input[type=text]").prop("disabled", "disabled");
        $("input[type=number]").prop("disabled", "disabled");
        $("input[type=checkbox]").prop("disabled", "disabled");
        $(".ratp_copypaste").hide();
        $(".ratp_btncopy").hide();
        <% End If %>
    }

    function showCents(val) {
        cents = (val ? 2 : 0);
        if (!view_grid) {
            $("#RATimeline").timeline("option", "valDigits", cents).timeline("option", "hideCents", !cents);
            $("#RATimeline").timeline("resize");
        }
    }

    function UpdateTimeperiods(init_page) {
        var grid = $("#divGridAlts");
        var tp = $("#RATimeline");
        if (view_grid) {
            tp.hide();
            grid.show();
            if (!init_page) sendCommand("refresh", false);
        } else {
            grid.hide();
            tp.show();
            if (init_page) initTimeperiods();
            tpShowState();
        }
    }

    function initTimeperiods() {
        //if ($("#RATimeline").timeline() === "undefined") {
            var startDate = '<%= GetTimelineStartDate()%>';
            var resources = <%= GetResourcesList() %>;
            var distribMode = <%= GetTimeperiodsDistribMode() %>;
            var pgSize = pageSize();
            if (!isByPages()) pgSize = 0;
            var curPage = cur_page + 1;
            if (curPage<0) curPage = 0;
            $("#RATimeline").timeline({
                projs: projects,
                periodsCount: <%= GetTimePeriodsCount() %>,
                resources: resources,
                imgPath: "<% =ImagePath %>",
                imgInfodoc: "info12.png",
                imgNoInfodoc: "info12_dis.png",
                viewMode: 'results',
                pageSize: pgSize,
                currentPage: curPage,
                tableAlign: true,
                valDigits: cents,
                distribMode: distribMode,
                selectedResourceID: "<%= TP_RES_ID%>",
                periodNamingMode: <%= GetTimePeriodsType() %>,
                periodNamePrefix: '<%= GetTimePeriodNameFormat %>',
                startDate: startDate,
                portfolioResources: <%= GetRAPortfolioResources() %>,
                solvedResults: <%= GetPeriodResults() %>,
                index_visible: <%= Bool2JS(RA.Scenarios.GlobalSettings.IsIndexColumnVisible) %>,
                fundedOnly: fundedonly
                }).timeline({setPage: function (event) { setPage($(event.target).timeline('option', 'currentPage')-1); }});
        //}
    }

    function changeUseDiscount(chk) {
        on_ok_code = "UpdateButtons();";
        if ((chk)) on_ok_code += " if ((theForm.tbDiscount)) theForm.tbDiscount.focus();";
        sendCommand("action=tp_usediscount&val=" + ((chk)? 1 : 0), false);
    }

    function onchangeDiscount(val) {
        setTimeout('changeDiscount("' + replaceString('"', '', val) + '");', 300);
    }

    function changeDiscount(val) {
        var e = theForm.tbDiscount;
        if ((e) && e.value==val) {
            var v = isValidNumber(e, true, false);
            if (typeof v != "undefined" && onKeyUp(e.id, true)) {
                sendCommand("action=tp_discount&val=" + v);
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

    var alt_name = -1;
    var alt_value = "";

    function switchAltNameEditor(id, vis) {
        <% if Scenario.IsInfeasibilityAnalysis Then %>return false;<% End if %>
        if (vis==1) {
            if (alt_name<0 || alt_name==id || (alt_name>=0 && alt_name!=id && switchAltNameEditor(alt_name, 0))) {
                var w = $("#td_hdr_name").width() - 24;
                if (w<50) w = $("#name"+id).width();
                $("#name"+id).hide();
                $("#tbName"+id).width(w-2).show().select().focus();
                alt_name = id;
                alt_value = $("#tbName"+id).val();
                on_hit_enter = "switchAltNameEditor(" + id + ", 0);";
            }
        } else {
            var val = $("#tbName"+id).val();
            if (val=="") {
                if (!dxDialog_opened) dxDialog("<% = JS_SafeString(ResString("msgRAEmptyName")) %>", 'setTimeout(\'$("#tbName'+id+'").focus();\', 150);');
                return false;
            }
            if (vis==0 && val!="" && alt_value!="" && val!=alt_value) saveAltName(id, val);
            $("#tbName"+id).hide();
            $("#name"+id).show();
            alt_name = -1;
            alt_value = "";
            on_hit_enter = "";
        }
        return true;
    }

    function saveAltName(id, val) {
        var w = <% =CInt(IIf(RA_FixedColsWidth, 65, 150)) %>;
        var alt_id = $("#altID" + id).val();
        if (val!="" && alt_id!="" ) {
            var v = (val.length>w ? ShortString(val, w) :  val);
            $("#name"+id).html(replaceString("&amp;hellip", "&hellip", htmlEscape(v)));
            var cell = $("#name"+id).parent();
            if ((cell)) {
                cell.attr("clip_data", v);
            }
            on_ok_code = "filterGrid(search_old);";
            sendAJAX("action=save_altname&id=" + enc(alt_id) + "&val=" + enc(val));
        }
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
            on_ok_code = "";
            sendCommand("action=save_altname&id=" + alt_id + "&val=" + encodeURIComponent(val));
        }
    }
    var sprty_changed = false;

    function onShowSolverPrty() {
        var sprty_changed = false;
        //InitSolverPrty();
        //InitSolverPrtyDlg();
        on_ok_code = "PleaseWait(false); onGetSolverPriorities(data); InitSolverPrtyDlg();";
        showLoadingPanel();
        sendAJAX("action=sprty_list");
        return false;
    }

    function InitSolverPrtyDlg() {
        dlg_solverprty = $("#divSolverPrty").dialog({
            autoOpen: true,
            width: 450,
            minWidth: 330,
            maxWidth: 950,
            minHeight: 200,
            maxHeight: mh,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: false,
            bgiframe: true,
            title: "<% = JS_SafeString(ResString("titleRASolverPriorities")) %>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Close: function() {
                    dlg_solverprty.dialog('close');
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
                if (solver_id == solver_id_xa) $("#lblRASPrtyWarning").show(); else $("#lblRASPrtyWarning").hide();
                if (theForm.cbOptConstraints.checked) $("#lblRASPrtyCCWarning").show(); else $("#lblRASPrtyCCWarning").hide();
            },
            close: function() {
                $("body").css("overflow", "auto");
                InitSolverPrty();
                $("#tblSPriorities tbody").html("");
                $("#divSolverPrty").dialog("destroy");
                dlg_solverprty = null;
                if (sprty_order!="") {
                    on_ok_code = "PleaseWait(false);" + (isAutoSolve() ? "Solve();" : "");
                    PleaseWait(true);
                    sendAJAX("action=sprty_reorder&lst=" + sprty_order);
                    sprty_order = "";
                } else {
                    if (sprty_changed && isAutoSolve()) Solve();
                }
                sprty_changed = false;
            },
            resize: function( event, ui ) { $("#divSPriorties").height(30); var dif = $("#divSPriortyHints").height() + 120; $("#divSPriorties").height(Math.round(ui.size.height-dif)); }
        });
        var dif = $("#divSPriortyHints").height() + 120;
        if ($("#divSPriorties").height()>mh-dif) $("#divSPriorties").height(mh-dif);
        setTimeout('$("#divSPriorties").scrollTop(10000);', 30);
    }

    function InitSolverPrty() {
        var t = $("#tblSPriorities tbody");
        if ((t)) t.html("");
        var has_act = false;
        var use_cc = !(theForm.cbOptConstraints.checked);
        for (var i=0; i<solver_priorities.length; i++) {
            if ((t)) {
                var p = solver_priorities[i];
                var id = p[1];
                var n = htmlEscape(p[2]);
                var is_act = (p[3]);
                var ignored = (id>=0 && (!use_cc || !p[5]));
                var st = "";
                if (!is_act || ignored || (solver_id == solver_id_xa && (has_act || (!has_act && !is_act)))) st = "inactive ";
                if (ignored) n = "<nobr><span class='gray xsmall' style='float:right; margin-left:1ex;'>[<% =JS_SafeString(ResString("lblRACCIgnored")) %>]</span><i>" + n + "</i></nobr>";
                if (id<0 && st=="") n ="<span style='color:#009966'>" + n + "</span>";
                if (is_act && !ignored) has_act = true;
                var is_min = (p[4]==<% =Cint(raSolverCondition.raMinimize) %>);
                var num = p[0];
                if (st=="" && is_act && !ignored) {
                    num = "<b>" + num + "</b>";
                    n = "<b>" + n + "</b>";
                }
                var cond = "<a href='' onclick=\"switchSolverPrtyCondition('" + id + "'); return false;\" class='actions' style='padding:2px 4px;'><img src='../../Images/ra/" + (is_min ? "down_11.png" : "up_11.png") + "' width=11 height=11 border=0 title='" + (is_min ? "<% =JS_SafeString(ResString("lblRASolverPrtyMin")) %>" : "<% =JS_SafeString(ResString("lblRASolverPrtyMax")) %>") +"' id='sprty_cond_" + id + "'></a>";
                var chk = "<input type='checkbox' id='sprty_act_" + i + "' value='" + id + "'" + (is_act ? " checked" : "") + " onclick=\"switchSolverPrtyActive('" + id + "', this.checked);\">";
                sRow = "<tr class='text grid_row_dragable " + st + ((i&1) ? "grid_row" : "grid_row_alt") + "' align=center><td><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<!--td>" + num + "</td--><td align=left>" + n + "</td><td>" + chk + "</td><td class='text small'>" + cond +"</td>";
                sRow += "</tr>";
                t.append(sRow);
            }
        }
    }

    function onGetSolverPriorities(data) {
        if ((data) && (data!="")) {
            //try {
                solver_priorities = eval(data);
                InitSolverPrty();
            //}
        }
    }

    function getSolverPrtyByID(id) {
        for (var i=0; i<solver_priorities.length; i++) {
            if (solver_priorities[i][1]==id) return solver_priorities[i];
        }
        return null;
    }

    function switchSolverPrtyCondition(id) {
        var p = getSolverPrtyByID(id);
        if ((p)) {
            //on_ok_code = "onGetSolverPriorities(data);";
            p[4] = (p[4]==<%=Cint(raSolverCondition.raMaximize) %> ? <%=Cint(raSolverCondition.raMinimize) %> : <%=Cint(raSolverCondition.raMaximize) %>);
            sendAJAX("action=sprty_cond&id=" + id);
            InitSolverPrty();
            sprty_changed = true;
        }
    }

    function switchSolverPrtyActive(id, act) {
        var p = getSolverPrtyByID(id);
        if ((p)) {
            //on_ok_code = "onGetSolverPriorities(data);";
            p[3] = !p[3];
            sendAJAX("action=sprty_active&id=" + id + "&act=" + ((act) ? 1 : 0));
            InitSolverPrty();
            sprty_changed = true;
        }
    }

    function onCheckInfeas() {
        closePopup();
        on_ok_code = "onLoadInfeasConstr(data);";
        showLoadingPanel();
        sendAJAX("action=infeas_constr_get");
    }

    function onLoadInfeasConstr(data) {
        hideLoadingPanel();
        if ((data!="") && (data)) {
            var lst = eval(data);
            if (!lst.length) {
                DevExpress.ui.notify("<% = JS_SafeString(ResString("lblRANoInfeasData")) %>", "warning");
            } else {
                showInfeasConstr(lst);
            }
        }
    }

    function selInfeas(sel, cnt) {
        var t = $("#ciType").val();
        for (var i=0; i<cnt; i++) {
            var c = $("#inf_" + i);
            if ((c) && (c.length)) {
                if (typeof t == "undefined" || t=="" || t=="-1" || c.hasClass("infeas_type_" + t)) {
                    c.prop("checked", (sel==1 || (sel==-1 && !(c.prop("checked"))) ? "checked" : ""));
                }
            }
        }
        checkSelInfeas(cnt);
        return false;
    }

    function showInfeasConstr(lst) {
        var cnts = [1,3,5,10,15,20];
        var html = "<input type='hidden' name='inf_cnt' id='inf_cnt' value='" + lst.length  + "'>\n<p>Please choose constraint(s) to include to Infeasibility Analysis:</p>";
        html += "<div style='text-align:center'><b>Search solutions</b>: <select  id='cntSolutions'>";
        for (var i=0; i<cnts.length; i++) {
            html += "<option value='" + cnts[i] + "'" + (cnts[i] == <% =RA_Infeasibility_Solutions_Count %> ? " selected='selected'" : "") + ">" + (cnts[i] == 1 ? "The best one only" : "Up to " + cnts[i]) + "</option>";
        }
        html += "</select></div>";

        var options = "<div id='divInfeasConstr' style='overflow-y:auto'>";
        var types = [[false, 0, "Must"], [false, 1, "Must Not"], [false, 2, "Group"], [false, 3, "Dependency"], [false, 4, "Custom Constraint Min"], [false, 5, "Custom Constraint Max"], [false, 6, "Time Periods Dependency"], [false, 7, "Time Period Resource Min"], [false, 8, "Time Period Resource Max"], [false, 9, "Funding Pool Limit"], [false, 10, "Cost"]];
        var types_cnt = 0;
        for (var i=0; i<lst.length; i++) {
            var o = lst[i];
            options += "<div class='divCheckbox ra_infeas_item'><input type='checkbox' id='inf_" + i + "' name='inf_" + i + "' value='" + o[0] + "'" + ((o[3]) ? " checked" : "") + " onclick='checkSelInfeas(" + lst.length + ");' class='infeas_type_" + o[4] + "'><label for='inf_" + i + "'><b>" + o[1] + "</b></label>" + (o[2]=="", "", "<div class='text small ra_infeas_item'>" + o[2] + "</div>") + "</div>\n";
            if (typeof types[o[4]] != "undefined" && !(types[o[4]][0])) {
                types[o[4]][0] = true;
                types_cnt++;
            }
        }
        options += "</div>";
        html += "<div style='text-align:center; margin:4px 0px; padding:3px; background:#f0f0f0;'>Select:&nbsp;";
        if (types_cnt>1) {
            html += "<select  id='ciType' name='ciType' style='font-size:9pt'><option value='-1' selected='selected'>Any constraint type</option>";
            for (var i=0; i<types.length; i++) {
                if ((types[i][0])) {
                    html += "<option value='" + i + "'>" + types[i][2] + "</option>";
                }
            }
            html +="</select>&nbsp;&nbsp;&ndash;&nbsp;&nbsp;";
        }
        html += "<a href='' onclick='selInfeas(1, " + lst.length + "); return false;' class='actions dashed'>All</a> | <a href='' onclick='selInfeas(0, " + lst.length + "); return false;' class='actions dashed'>None</a> | <a href='' onclick='selInfeas(-1, " + lst.length + "); return false;' class='actions dashed'>Invert</a></div>";

        $("#divCheckInfeas").html(html + options);
        if ((dlg_infeas)) {
            $("#divCheckInfeas").dialog("open");
        } else {
            dlg_infeas = $("#divCheckInfeas").dialog({
                autoOpen: true,
                minWidth: dlgMaxWidth(400),
                maxWidth: dlgMaxWidth(900),
                minHeight: dlgMaxHeight(350),
                maxHeight: dlgMaxHeight(800),
                modal: true,
                dialogClass: "no-close",
                closeOnEscape: true,
                bgiframe: true,
                title: "<% = JS_SafeString(ResString("lblRACheckInfeas")) %>",
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: {
                    OK: {
                        id: "btnApply",
                        text: "<% = JS_SafeString(ResString("btnContinue")) %>",
                        width: 80,
                        //disabled: true,
                        click: function() {
                            SetInfeasOptions();
                            $("#divCheckInfeas").dialog("close");
                        }
                    },
                    Cancel: {
                        width: 80,
                        text: "<% = JS_SafeString(ResString("btnCancel")) %>",
                        click: function() {
                            $("#divCheckInfeas").dialog("close");
                        }
                    }
                },
                open: function() {
                    $("body").css("overflow", "hidden");
                    setTimeout('var h = $("#divCheckInfeas").height(); if (h*1>100) { $("#divInfeasConstr").height(30); $("#divInfeasConstr").height(h-120); checkSelInfeas(' + lst.length + '); $("#btnApply").focus(); }', 50);
                },
                close: function() {
                    $("body").css("overflow", "auto");
                },
                resize: function( event, ui ) { $("#divInfeasConstr").height(30); $("#divInfeasConstr").height(Math.round(ui.size.height-206)); }
            });
        }
    }

    function checkSelInfeas(cnt) {
        var has_Sel = false;
        for (var i=0; i<cnt; i++) {
            var c = $("#inf_" + i);
            if ((c) && (c.length) && c.prop("checked")) {
                has_Sel = true;
                break;
            }
        }
        $("#btnApply").prop("disabled", (has_Sel ? "" : "disabled"));
    }

    function SetInfeasOptions() {
        var cnt = $("#inf_cnt").val();
        if (cnt>0) {
            var lst = "";
            for (var i=0; i<cnt; i++) {
                var o = $("#inf_" + i);
                if ((o) && (o.length) && (o.prop("checked"))) lst += (lst=="" ? "" : ",") + o.val();
            }
            on_ok_code = "onLoadInfeasInfo(data);";
            showLoadingPanel();
            sendAJAX("action=infeas_constr_set","lst=" + lst+"&cnt=" + $("#cntSolutions").val()*1);
        }
    }

    function onLoadInfeasInfo(data) {
        hideLoadingPanel();
        //dxDialog(replaceString("\n", "<br>", data), ";", undefined, "<%= JS_SafeString(ResString("lblRACheckInfeas")) %>");
        if (data*1==1) {
            showLoadingPanel();
            document.location.reload();
        } else {
            DevExpress.ui.dialog.alert((data=="" ? "Unable to get results for this data" : data), "Load Infeasibility data");
        }
    }

    function onInfeasibilityResults(mode) {
        on_ok_code = "showLoadingPanel(); document.location.reload();";
        sendAJAX("action=infeas_result&mode=" + mode*1);
        showLoadingPanel();
    }

    function showRemovedConstr(id) {
        var is_single = <% =Bool2JS(InfeasibilitySolutionsCount <2) %>;
        var obj = $((is_single) ? "#divRemovedConstr": "#titleRemovedConstr" + id); 
        if (obj.length) {
            var lst = (is_single ? obj.html() : obj.prop("title"));
            if (typeof lst != "undefined") {
                if (lst=="") {
                    DevExpress.ui.dialog.alert("Nothing has been changed", "Removing constraints");
                } else {
                    DevExpress.ui.dialog.alert(lst, "Infeasibility analysis");
                }
            }
        }
        return false;
    }

    function showSolutions() {
        if ($("#divSolutions").length) {
            var lst = $("#divSolutions").html();
            if (typeof lst != "undefined") {
                if (lst=="") {
                    DevExpress.ui.dialog.alert("Solutions not found ", "Infeasibility analysis");
                } else {
                    DevExpress.ui.dialog.alert($("#divSolutions").html(), "Infeasibility analysis solutions");
                }
            }
        }
        return false;
    }


    function downloadExcel() {
        var t = getToolbarButton("timeperiods");
        var ask_tp = ((t) && t.get_enabled());

        if ((theForm.cbOptTimePeriods)) { 
            if (theForm.cbOptTimePeriods.checked) ask_tp = false;
        }

        if (ask_tp) {
            dxConfirm('<% = JS_SafeString(ResString("confRADownloadTP"))%>', "onDownloadExcel(true);", "onDownloadExcel(false);");
        } else {
            onDownloadExcel(false);
        }
    }

    var dl_warning = true;
    function onDownloadExcel(include_tp) {
        CreatePopup("<% = PageURL(CurrentPageID, String.Format("?{0}=export&type=xml{1}&tp=""+(include_tp ? 1 : 0)+""&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" + Math.round(100000*Math.random()), "export_xml", "");
        if ((dl_warning)) {
            showErrorMessage("<% = JS_SafeString(ResString("msgRADownloadExcel")) %>", "false");
            dl_warning = false;
        }
    }

    function showLegend() {
        DevExpress.ui.dialog.alert("<% = JS_SafeString(ResString("hintRALegend")) %>", "<% =JS_SafeString(ResString("btnRAHelp")) %>");
    }

    // called for pg=1..max, but cur_page started with 0
    function setPage(pg) {
        cur_page = pg-1;
        sendAJAX("action=setpage&pg=" + cur_page);
        if (view_grid) {
            if ($("#RATimeline").timeline() !== "undefined") $("#RATimeline").timeline("option", "currentPage", cur_page+1);
            filterGrid(search_old);
        } else {
            cur_page = pg;
        }
    }

    function setPagination(val) {
        var pgsize = pageSize();
        if (!isByPages()) pgsize = 0;
        sendAJAX("action=pagination&id=" + pgsize);
        if ($("#RATimeline").timeline() !== "undefined") $("#RATimeline").timeline("option", "pageSize", pgsize);
        if (view_grid) {
            filterGrid(search_old);
        } else {
            resizeTimeperiods();
            $("#RATimeline").timeline("resize");
        }
        checkPageSettings();
    }

    function nextTabindex(el) {
        if ((el)&&(view_grid)) {
            var cur = el.tabIndex*1;
            if (cur>0) {
                var coeff = <% =TAB_COEFF %>;
                var l = cur % coeff
                var b = Math.floor(cur/coeff);
                
                var next_el = null;
                var ids = [];
                for (var i = 0; i < theForm.elements.length; i++) {
                    var idx = theForm.elements[i].tabIndex*1;
                    if (idx>coeff && !(theForm.elements[i].disabled) && (theForm.elements[i].style.display!="none")) ids.push(idx);
                }

                var n = (b+1) * coeff + l;
                while (n<65535 && !(next_el)) {
                    for (var i=0; i<ids.length; i++) {
                        if (ids[i]==n) {
                            next_el = $('[TabIndex="' + n + '"]')[0];
                            break;
                        }
                    }
                    if (!(next_el)) n+=coeff;
                }

                if (!(next_el)) {
                    var n = coeff + l+1;
                    while (n<65535 && next_el == null) {
                        while (n<Math.floor(65535/coeff)*coeff && next_el == null) {
                            for (var i=0; i<ids.length; i++) {
                                if (ids[i]==n) {
                                    next_el = $('[TabIndex="' + n + '"]')[0];
                                    break;
                                }
                            }
                            if (!(next_el)) n+=coeff;
                        }
                        l += 1;
                        if (!(next_el)) n = coeff + l+1;
                    }
                }
            }
            if (n>=65535 && !(next_el)) { next_el = theForm.tbBudgetLimit; if ((next_el) && next_el.disabled) next_el = null; }
            if ((next_el)) { next_el.focus(); next_el.select(); }
        }
        return false;
    }    

    function Hotkeys(event)
    {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event)
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            if (code == 13 && !is_solve) {
                if (on_hit_enter=="") {
                    var b = getToolbarButton("solve");
                    //                    if ((b) && !theForm.tbBudgetLimit.disabled && (!dlg_scenarios || !dlg_scenarios.dialog("isOpen"))) { b.focus(); b.click(); } else { if (last_focus=="") onSolve(); }
                    if (last_focus!="")
                    {
                        if (last_focus == "tbBudgetLimit" && onBlur(last_focus, "budget", "", 0)) { 
                            last_focus = "tbBudgetLimit";
                        } else {
                            nextTabindex(eval('theForm.'+last_focus));
                        }
                    } else return true;
                } else {
                    eval(on_hit_enter);
                }
                return false;
            }
            if (code == 27 && is_solve) {
                dxConfirm("Do you really want to stop solving and refresh the page?", "PleaseWait(1); document.location.reload();");
                $("div.ui-dialog").css("z-index", "9999999");
                return false;
            }
            if (code == 27 && alt_name>=0 && !(dxDialog_opened)) {
                $("#tbName"+alt_name).val(alt_value);
                switchAltNameEditor(alt_name, -1);
                return false;
            }
        }
    }

    $(document).ready(function () { 
        showLoadingPanel(); 
        ResetRadToolbar();
        InitPage(); 
        <% If Scenario.IsInfeasibilityAnalysis AndAlso Scenario.InfeasibilityRemovedConstraints IsNot Nothing AndAlso Scenario.InfeasibilityRemovedConstraints.Count > 0 Then %>setTimeout(function () {
            $("#btnSolutions").focus(); 
            showRemovedConstr(<% =RA.Scenarios.ActiveScenarioID %>);
        }, 500);<% End If %>
    });
    
    // Trigger action when the contexmenu is about to be shown
    $(document).bind("contextmenu", function (event) {    
        if (event.target.id.indexOf("resInput") >= 0) {
            var name = event.target.id.replace("resInput", "");

            // Avoid the real one
            event.preventDefault();
    
            // Show contextmenu
            $("#ctx" + name).finish().toggle(100).
    
            // In the right position (the mouse)
            css({
                top: event.pageY + "px",
                left: event.pageX + "px"
            });
        };
    });

    // If the document is clicked somewhere
    $(document).bind("mousedown", function (e) {
    
        // If the clicked element is not the menu
        //if (!$(e.target).parents(".custom-menu").length > 0) {
        
            // Hide it
            $(".custom-menu").hide(100);
        //}
    });

    // If the menu element is clicked
    $(".custom-menu li").click(function(){
    
        // This is the triggered action name
        switch($(this).attr("data-action")) {
        
            // A case for each action. Your actions here
            case "first": alert("first"); break;
            case "second": alert("second"); break;
            case "third": alert("third"); break;
        }
  
        // Hide it AFTER the action was triggered
        $(".custom-menu").hide(100);
    });

    resize_custom = onResize;
    document.onkeypress = Hotkeys;
    $("#tdGridAlts").hide();

</script>
</telerik:RadScriptBlock>

<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server"  ClientEvents-OnResponseEnd="onResponseEnd" EnableAJAX="true" EnableHistory="false" EnablePageHeadUpdate="false">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="divGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="divGrid"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>
<table border='0' cellspacing='0' cellpadding='0' class='whole' id='divMain'>

<tr valign='bottom' align='left' class='text'>
<td style="height:24px" class="text"><div id='divToolbar' style='overflow:hidden;'><telerik:RadToolBar ID="RadToolBarMain" runat="server" Skin="Default" Width="100%" CssClass="dxToolbar" OnClientButtonClicked="onClickToolbar" OnClientDropDownOpening="OnClientDropDownOpening" AutoPostBack="false" EnableViewState="false" CollapseAnimation-Duration="0" ExpandAnimation-Duration="0">
    <Items>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Solve" Width="80px" ImageUrl="~/App_Themes/EC09/images/assembly-20.png" Value="solve" Font-Bold="true" Font-Size="11pt" BorderColor="#ffaa44" BackColor="#f0f0f0" ForeColor="#454545" BorderWidth="1" OnLoad="OnLoadSolveButton"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" AllowSelfUnCheck="True" ImageUrl="~/App_Themes/EC09/images/repeat-20.png" CheckOnClick="True" Checked="False" Text="Auto-solve" Value="autosolve" Group="autosolve" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/config-20.png" Value="solver" Text="<span id='divSolverActive' style='position:absolute; width:10px; text-align:center; margin-left:-19px; padding:1px; margin-top:4px; font-family:Tahoma, Verdana, Arial; font-size:5pt; color:#6688cc;'>&hellip;</span>" ToolTip="" Width="25"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" IsSeparator="true" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOptScenario" Enabled="false">
            <ItemTemplate><select disabled="disabled" id="cbScenarios" style="width:145px; margin-top:3px" onchange="onSetScenario(this.value);"><option selected="selected">Not available</option></select></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/scripts_cnt_20.png" Value="scenarios" Text="<span id='cntScenarios' style='position:absolute; width:12px; text-align:center; margin-left:-20px; padding:1px; margin-top:6px; font-family:Arial; font-size:5pt;'>...</span>" ToolTip="Manage Scenarios" Width="25"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" IsSeparator="true" />
        <%--<telerik:RadToolBarButton runat="server" EnableViewState="false" Value="select_alts" ImageUrl="~/App_Themes/EC09/images/filter20.png" Text="Select Alternatives"/>--%>
        <telerik:RadToolBarDropDown runat="server" DropDownWidth="320" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/dices_black_20.png" Text="Risks">
        <Buttons>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Create and Associate a Risk Model" Value="create_risk"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Associate Risk Model" Value="select_risk"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Open Associated Risk Model" Value="open_risk" Enabled="false"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Synchronize alternatives in Associated Risk Model" Value="sync_alts_risk" Enabled="false"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Import as Risks from Associated Risk Model" Value="import_risk" Enabled="false"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Import as Probabilyty of Succes" Value="import_prob" Enabled="false"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Detach Associated Risk Model" Value="unset_risk" Enabled="false"/>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Delete all Risks" Value="delete_risk"/>
        </Buttons>
        </telerik:RadToolBarDropDown>
        <%--<telerik:RadToolBarButton runat="server" EnableViewState="false" IsSeparator="true" />--%>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" AllowSelfUnCheck="True" ImageUrl="~/App_Themes/EC09/images/timeperiods_20.png" CheckOnClick="True" Checked="False" Text="Time Periods" Value="timeperiods" IsSeparator="False" Group="tp" />
        <telerik:RadToolBarDropDown runat="server" EnableViewState="false" DropDownWidth="220" ImageUrl="~/App_Themes/EC09/images/config-20_green.png" Text="" ToolTip="Time Periods Settings">
        <Buttons>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding:6px 0px 8px 0px;'><input type='checkbox' id='cbTPUseDiscountFactor' <% = IIF(Scenario.TimePeriods.UseDiscountFactor, " checked", "") %> onclick="changeUseDiscount(this.checked);" style="margin-left:8px;"/><label for="cbTPUseDiscountFactor">&nbsp; &nbsp;<% = ResString("lblRAUseDiscountFactor") %>:</label> <input id="tbDiscount" size="5" type="text" value="<% = Double2String(Scenario.TimePeriods.DiscountFactor,2) %>" class="as_number" onkeyup="onchangeDiscount(this.value);" /></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding:3px 2px 3px 36px;'><span style='line-height:1em; color:#003399;'><% =ResString("lblRATPDistribute") %></span>
                    <div style="margin-left:8px; margin-top:4px;">
                        <input type="radio" name="DistribMode" value="1" <% =IIf(PM.Parameters.TimeperiodsDistributeMode=1, " checked", "") %> id="distribAll"  onclick="switchTPDistribute(1);" /><label for="distribAll"><% = ResString("lblRATPDistrSubseq") %></label><br />
                        <input type="radio" name="DistribMode" value="0" <% =IIf(PM.Parameters.TimeperiodsDistributeMode=0, " checked", "") %> id="distribNext" onclick="switchTPDistribute(0);" /><label for="distribNext"><% = ResString("lblRATPDistrAll") %></label><br />
                        <input type="radio" name="DistribMode" value="2" <% =IIf(PM.Parameters.TimeperiodsDistributeMode=2, " checked", "") %> id="distribTotal" onclick="switchTPDistribute(2);" /><label for="distribTotal"><% = ResString("lblRATPDistrTotal") %></label>
                    </div></div></ItemTemplate>
            </telerik:RadToolBarButton>
        </Buttons>
        </telerik:RadToolBarDropDown>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="False" IsSeparator="true" />
        <telerik:RadToolBarDropDown runat="server" EnableViewState="false" DropDownWidth="250" ImageUrl="~/App_Themes/EC09/images/settings-20.png" Text="Settings">
        <Buttons>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/columns-20.png" Text="Columns&hellip;" Value="columns"/>
            <telerik:RadToolBarButton IsSeparator="true" />
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptFixedWidth' <% = IIF(RA_FixedColsWidth, " checked", "") %> onclick='onChangeSettings("fixedwidth", this.checked);'/><label for='cbOptFixedWidth'>&nbsp; &nbsp;<% =ResString("lbRAOptFixedWidth")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptHideIgnored' <% = IIF(RA_HideIgnored, " checked", "") %> onclick='onChangeSettings("hideignored", this.checked);'/><label for='cbOptHideIgnored'>&nbsp; &nbsp;<% =ResString("lblRAOptHideIgnored")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptHideNoRisks' <% = IIF(RA_HideNoRisks, " checked", "") %> onclick='onChangeSettings("hidenorisks", this.checked);'/><label for='cbOptHideNoRisks'>&nbsp; &nbsp;<% =ResString("lblRAHideRisk")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowFunded' <% = IIF(RA_ShowFundedOnly, " checked", "") %> onclick='onChangeSettings("showfunded", this.checked);'/><label for='cbOptShowFunded'>&nbsp; &nbsp;<% =ResString("lblRAOptShowFundedOnly")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowSurplus' <% = IIf(RA_ShowSurplus, " checked", "")%> onclick='onChangeSettings("showsurplus", this.checked);'/><label for='cbOptShowSurplus'>&nbsp; &nbsp;<% =ResString("lblRAOptShowSurplus")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowChanges' <% = IIF(RA_ShowChanges, " checked", "") %> onclick='onChangeSettings("showchanges", this.checked);'/><label for='cbOptShowChanges'>&nbsp; &nbsp;<% =ResString("lblRAOptShowChanges")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Visible="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowMinMax' <% = IIF(RA_ShowMinMaxExtra, " checked", "") %> onclick='onChangeSettings("minmaxextra", this.checked);'/><label for='cbOptShowMinMax'>&nbsp; &nbsp;Show extra min/max info</label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowTinyBars' <% = IIF(RA_ShowTinyBars, " checked", "") %> onclick='onChangeSettings("showbars", this.checked);'/><label for='cbOptShowTinyBars'>&nbsp; &nbsp;<% =ResString("lblRAOptShowBars")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptExpandTP' onclick='switchAllTP();'/><label for='cbOptExpandTP'>&nbsp; &nbsp;<% =ResString("lblRAExpandAllTP")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptScrolling' <% = IIF(RA.Scenarios.GlobalSettings.ShowFrozenHeaders, " checked", "") %> onclick='use_scrolling = this.checked; onChangeSettings("scrolling", this.checked);'/><label for='cbOptScrolling'>&nbsp; &nbsp;<% =ResString("lblRAOptFreezeHeaders")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowCents' <% = IIF(RA_ShowCents, " checked", "") %> onclick='showCents(this.checked); onChangeSettings("showcents", this.checked);'/><label for='cbOptShowCents'>&nbsp; &nbsp;<% =ResString("lblRAOptShowCents")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px; padding-left:4px;'><input type='checkbox' id='cbOptShowByPages' <% = IIF(RA_Pages_Mode <> ecRAGridPages.NoPages , " checked", "") %> onclick='setPagination(this.checked ? $("#cbPageMode").val() : 0);'/><label for='cbOptShowByPages'>&nbsp; &nbsp;<% =ResString("lblRAOptShowByPages")%></label>&nbsp;&nbsp;<select id="cbPageMode" onchange="setPagination(this.value);"><% =GetPageModes %></select></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" CheckOnClick="false" Checked="false" Enabled="false">
                <ItemTemplate><div class='toolbarcustom' style="padding:1ex 1em; margin-left:-2px;"><nobr>
                <div style='text-align:center; padding-top:3px;'><input type='button' class='button button_small' id='btnCloseSettings' value='OK' onclick='var d = getToolbarButton("Settings"); if ((d)) d.hideDropDown(); return false;' /></div>
                </nobr></div></ItemTemplate>
            </telerik:RadToolBarButton>
        </Buttons>
        </telerik:RadToolBarDropDown>
        <telerik:RadToolBarDropDown runat="server" EnableViewState="false" DropDownWidth="270" ImageUrl="~/App_Themes/EC09/images/docs_20.png" Text="More">
            <Buttons>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="select_alts" ImageUrl="~/App_Themes/EC09/images/filter20.png" Text="Select Alternatives"/>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="sync" ImageUrl="~/App_Themes/EC09/images/sync_20.png" Text="Sync linked constraints" Enabled="false"/>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/delete_tbl_20.png" Text="Delete alternatives that aren't funded" Value="delete_nonfunded" Enabled="false"/>                
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/compare_20.png" Text="Allocations table" Value="alloc_tbl" />
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/reload-20.png" Text="Reset" Value="unsolve" />
            </Buttons>
        </telerik:RadToolBarDropDown>
        <telerik:RadToolBarDropDown runat="server" DropDownWidth="200" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/download-20.png" Text="Download">
            <Buttons>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/ecd_ra-20.png" Text="Download to ECD" Value="download" ToolTip="Download for ECD Resource Alligner"/>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/export-excel-20.png" Text="Export to Excel" Value="export_xls" />
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/export-oml-20.png" Text="Export to OML" Value="export_oml" Visible="false" />
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/export-mps-20.png" Text="Export to MPS" Value="export_mps" Visible="false" />
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/export-log-20.png" Text="Export Logs" Value="export_logs" />
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/file_20.png" Text="View Logs (debug only)" Value="view_logs" />
                <%--<telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/copy2-20.png" Text="Allocations comparison" Value="export_alloc" />--%>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/import_20.png" Text="Allocations table" Value="alloc_tbl2" />
            </Buttons>
        </telerik:RadToolBarDropDown>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="help" ImageUrl="~/App_Themes/EC09/images/help_20.png" Text="Colors legend"/>
    </Items>
</telerik:RadToolBar></div>
</td></tr>

<tr valign='top' style='height:99%'>
<td class='text'><div id='tdGrid' style='height:100%; overflow:hidden'><%--<telerik:RadAjaxPanel ID="RadAjaxManagerMain2" runat="server" ClientEvents-OnRequestError="onRequestError" ClientEvents-OnResponseEnd="onResponseEnd">--%>
<table border='0' cellspacing='0' cellpadding='0' runat='server' id='divGrid' class='whole' EnableViewState="false">

<tr style="height:3em"><td align="center" valign="middle" class='text' style='padding:4px'><h5 style="padding-bottom:3px" id="divHelper" title="<% =SafeFormString(Scenario.Description) %>"><% =String.Format(ResString("lblRATitle"), SafeFormString(ShortString(Scenario.Name, 65)))%></h5>
<div id='tblInfo' style='overflow:hidden'><table border='0' cellspacing='0' cellpadding='0'>

<tr valign="top">
<td style='padding-top:6px;' id="tdFundedInfo"><%=GetFundedInfo() %></td>

<!-- Ignore -->
<td><div style="padding:0px 1em;"><fieldset class="legend" style="padding-bottom:3px; padding-top:3px;width:340px;" runat="server" id="pnlIgnore">
    <legend class="text legend_title">&nbsp;<%=ResString("lblIgnorePnl")%>:&nbsp;——&nbsp;<nobr><input type="button" class='button button_small' id='btnUseAll'  <%=If(Scenario.IsInfeasibilityAnalysis, "disable='disabled'", "") %> value='<%=ResString("btnUseAll")%>' style='width:70px' onclick="return SetOptAll(<% =iif(OPT_SHOW_AS_IGNORES, 0, 1) %>);" /><input type='button' id='btnIgnoreAll' <%=If(Scenario.IsInfeasibilityAnalysis, "disable='disabled'", "") %> value='<%=ResString("btnIgnoreAll")%>' class="button button_small" style='width:70px' onclick="return SetOptAll(<% =iif(OPT_SHOW_AS_IGNORES, 1, 0) %>);" /></nobr>&nbsp;</legend>
<table border='0' cellspacing='0' cellpadding='0' style="width:98%">
<tr valign="top"><td class='text' align='left'><nobr>
<label id='lblcbOptMusts'        <% =GetOptionStyle("cbOptMusts")%>       ><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptMusts")%>        name='cbOptMusts' /> <%=ResString("optIgnoreMusts")%></label><br />
<label id='lblcbOptMustNots'     <% =GetOptionStyle("cbOptMustNots")%>    ><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptMustNots")%>     name='cbOptMustNots' /> <%=ResString("optIgnoreMustNot")%></label><br />
<label id='lblcbOptConstraints'  <% =GetOptionStyle("cbOptConstraints")%> ><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptConstraints")%>  name='cbOptConstraints' /> <%=ResString("optIgnoreCC")%></label>&nbsp;<% =GetConstraintsInfo()%>&nbsp;<a href='<% =GetRALink(_PGID_RA_CUSTOM_CONSTRAINTS)%>' class='actions'>&raquo;&raquo;</a><br />
<label id='lblcbOptDependencies' <% =GetOptionStyle("cbOptDependencies")%>><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptDependencies")%> name='cbOptDependencies' /> <%=ResString("optIgnoreDependencies")%></label>&nbsp;<a href='<% =GetRALink(_PGID_RA_DEPENDENCIES)%>' class='actions'>&raquo;&raquo;</a><br />
<label id='lblcbOptGroups'       <% =GetOptionStyle("cbOptGroups")%>      ><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptGroups")%>       name='cbOptGroups' /> <%=ResString("optIgnoreGroups")%></label> &nbsp;<a href='<% =GetRALink(_PGID_RA_GROUPS)%>' class='actions'>&raquo;&raquo;</a>
</nobr></td><td class='text' align='left' style='padding-left:1em'><nobr>
<label id='lblcbOptFundingPools' <% =GetOptionStyle("cbOptFundingPools")%>><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptFundingPools")%> name='cbOptFundingPools' /><span <% =IIf(IgnoreFP, String.Format(" style='color:#cc6666' title='{0}'", SafeFormString(ResString("hintRAFPIgnored"))), "")%>> <%=ResString("optIgnoreFP")%></span></label> &nbsp;<a href='<% =GetRALink(_PGID_RA_FUNDINGPOOLS)%>' class='actions'>&raquo;&raquo;</a><br />
<label id='lblcbOptRisks'        <% =GetOptionStyle("cbOptRisks")%>       ><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptRisks")%>        name='cbOptRisks' /> <%=ResString("optIgnoreRisks")%></label><br />
<% If ShowDraftPages() OrElse Not isDraftPage(_PGID_RA_TIMEPERIODS_SETTINGS) Then%>
    <label id='lblcbOptTimePeriods'  <% =GetOptionStyle("cbOptTimePeriods")%> ><input type='checkbox' class='checkbox checkbox_tiny' onclick='return showGrid(this, false);' <% =GetOptionValue("cbOptTimePeriods")%>  name='cbOptTimePeriods' id='cbOptTimePeriods' /><span <% =IIf(IgnoreTP, String.Format(" style='color:#cc6666' title='{0}'", SafeFormString(ResString("hintRATPIgnored"))), "")%>> <%=ResString("optTimePeriods")%></span></label> &nbsp;<a href='<% =GetRALink(_PGID_RA_TIMEPERIODS_SETTINGS)%>' class='actions'>&raquo;&raquo;</a><br />
    <div style='padding-left:18px'><label id='lblcbOptTPMins'><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptTimePeriodMins")%>  name='cbOptTimePeriodMins' id='cbOptTimePeriodMins' /><span <% =IIf(IgnoreTP, String.Format(" style='color:#cc6666' title='{0}'", SafeFormString(ResString("hintRATPIgnored"))), "")%>> <%=ResString("optTimePeriodMins")%></span></label><br />
    <label id='lblcbOptTPMaxs'><input type='checkbox' class='checkbox checkbox_tiny' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptTimePeriodMaxs")%>  name='cbOptTimePeriodMaxs' id='cbOptTimePeriodMaxs' /><span <% =IIf(IgnoreTP, String.Format(" style='color:#cc6666' title='{0}'", SafeFormString(ResString("hintRATPIgnored"))), "")%>> <%=ResString("optTimePeriodMaxs")%></span></label></div>
<% end if %>
<telerik:RadToolTip runat="server" ID="tooltipTPIgnored" IsClientID="true" ShowEvent="FromCode" HideEvent="FromCode" SkinID="tooltipInfo" TargetControlID="cbOptTimePeriods" Position="BottomLeft" OffsetY="-1" OffsetX="-8" ><div style="padding:1ex 1em;"><b><% =ResString("msgRATPIgnoredNoResults") %></b></div></telerik:RadToolTip>
</nobr></td></tr></table>
</fieldset></div></td>

<!-- Base case -->
<td><fieldset class="legend" style="padding-bottom:3px; padding-top:0px;" runat="server" id="pnlBaseCase">
    <legend class="text legend_title">&nbsp;<input type='checkbox' id="cbBaseCase" name='cbBaseCase' value='1' <% =GetOptionValue("cbBaseCase")%> onclick='switchBaseCase(); return onOptionClick(this.name, this.checked);'/><label for="cbBaseCase"><%=ResString("lblBaseCasePnl")%>:</label>&nbsp;</legend>
<table border='0' cellspacing='0' cellpadding='0'>
<tr valign="middle"><td class='text' align='left'><div id='optBaseCase'>
<label id='lblBSGroups' <% =GetOptionStyle("cbBCGroups")%>><input type='checkbox' class='checkbox' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbBCGroups")%> name='cbBCGroups'/> <%=ResString("optBCGroups")%></label> &nbsp;<a href='<% =GetRALink(_PGID_RA_GROUPS)%>' class='actions'>&raquo;&raquo;</a><br />
</div></td></tr></table>
</fieldset><div class="text legend_title" style='text-align:right; margin-top:16px;'><nobr><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearch" class="input" style="width:100px;" value="" onkeyup="onSearch(this.value, true)"/></label></nobr></div><div id="divSolverTime" style="padding-top:6px; text-align:right;" class="text gray xsmall"><% =GetSolveTime() %></div>
</td>
</tr>
</table>
<div id="divSolverMsg"><% = GetSolverMessage()%></div><% If Scenario.IsInfeasibilityAnalysis Then %>
<div class="warning" style="display:inline-block; margin:1ex 5em; padding:1ex; border-color: #c8c13a;"><% If Scenario.InfeasibilityRemovedConstraints IsNot Nothing AndAlso Scenario.InfeasibilityRemovedConstraints.Count > 0 Then %>
<table border="0" cellspacing="0" cellpadding="0">
    <tr><td align="center"><!--Results of infeasibility analysis with <a href="" onclick="showRemovedConstr(<% = RA.Scenarios.ActiveScenarioID %>); return false" class="actions dashed">removed constraint(s)</a> for scenario "<% = ShortString(If(RA.Scenarios.Scenarios.ContainsKey(Scenario.InfeasibilityScenarioID), RA.Scenarios.Scenarios(Scenario.InfeasibilityScenarioID).Name, "-unknown-"), 40) %>".<br />
You can <a href="" onclick="onInfeasibilityResults(1); return false;" class="actions">save it as new scenario</a> or 
<nobr><a href="" onclick="onInfeasibilityResults(2); return false;" class="actions">modify the original scenario</a></nobr> or 
<nobr>just <a href="" onclick="onInfeasibilityResults(0); return false;" class="actions">return back</a>.</nobr>-->
<input type='button' id='btnShowRemoved' value='Show removed constraints&hellip;' class="button" style='width:200px; margin-right:2em;' onclick="showRemovedConstr(<% = RA.Scenarios.ActiveScenarioID %>); return false" />
<nobr>
<input type='button' id='btnSaveAsNew' value='Save as new scenario' class="button" style='width:200px; margin-right:4px;' onclick="onInfeasibilityResults(1); return false;" />
<input type='button' id='btnUpdateScenario' value='Modify the original scenario' class="button" style='width:200px; margin-right:4px;' onclick="onInfeasibilityResults(2); return false;" />
<input type='button' id='btnCancelInfeas' value='Cancel' class="button" style='width:120px;' onclick="onInfeasibilityResults(0); return false;" />
</nobr></td>
<% If InfeasibilitySolutionsCount > 1 Then %><td style="padding-left:2em;"><input type='button' id='btnSolutions' value='Solutions&hellip;' class="button button-primary" style='width:120px;' onclick="return showSolutions();" /></td><% End If %>
</tr></table>
<% If InfeasibilitySolutionsCount < 2 AndAlso RA.Scenarios.ActiveScenario.IsInfeasibilityAnalysis Then %><div id="divRemovedConstr" style="display:none"><% =GetRemovedConstraints(RA.Scenarios.ActiveScenario) %></div><% Else %><div id="divSolutions" style="display:none"><% =GetSolutions() %></div><% End if %>
<%--<% =GetSolutionsRemovedConstraints() %>--%>
<% Else %><b><% =ResString("msgRANoInfeasSolutions") %></b>.<br /><a href="" onclick="onInfeasibilityResults(0); return false;" class="actions">Return back</a>.<% End If  %></div>
<% End if %></div></td></tr>

<tr style='height:99%'><td align='left' valign='top'><div id='tdGridAlts' style='overflow:hidden;' class='whole text'>
    <div style='overflow:auto;<% =iif(RA_ShowTimeperiods, "display:none;", "")%>' id='divGridAlts'>
        <asp:GridView EnableViewState="false" AllowSorting="true" AllowPaging="false" PagerSettings-Mode="NumericFirstLast" PagerStyle-CssClass="text small" PageIndex="0" PagerSettings-Position="Bottom" PagerStyle-HorizontalAlign="Center" PagerSettings-Visible="false" PageSize="10" ID="GridAlternatives" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" TabIndex="1" HorizontalAlign="Center" AutoGenerateColumns="false" SelectedIndex="0" CssClass="grid" ShowFooter="true">
            <RowStyle VerticalAlign="Middle" CssClass="text grid_row" Height="24px" Wrap="false"/>
            <AlternatingRowStyle VerticalAlign="Middle" CssClass="text grid_row_alt" Height="24px" Wrap="false"/>
            <HeaderStyle CssClass="text grid_header actions" Height="40px"/>
            <FooterStyle CssClass="text grid_footer" Wrap="false" VerticalAlign="Bottom" HorizontalAlign="Right" />
            <EmptyDataRowStyle CssClass="text grid_row" />
            <EmptyDataTemplate><h6 style='margin:8em'><nobr><%=ResString("msgRiskResultsNoData")%></nobr></h6></EmptyDataTemplate>
            <Columns>
                <asp:BoundField DataField="ID" ReadOnly="true" ItemStyle-CssClass="as_number" Visible="false" />
                <asp:BoundField DataField="SortOrder" ReadOnly="true" ItemStyle-CssClass="as_number" />
                <asp:BoundField DataField="Name" ReadOnly="true" HtmlEncode="true" ItemStyle-Wrap="false" ItemStyle-CssClass="cansearch"/>
                <asp:BoundField DataField="Funded" ReadOnly="true" ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="ID" ReadOnly="true" ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="BenefitOriginal" ReadOnly="true" ItemStyle-CssClass="as_number" />
                <asp:BoundField DataField="Benefit" ReadOnly="true" ItemStyle-CssClass="as_number" />
                <asp:BoundField DataField="Risk" ReadOnly="true" ItemStyle-CssClass="as_number" />
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><nobr><asp:TextBox ID="tbProbSuccess" runat="server" SkinID="TextInput" CssClass="as_number" Text='<%#Eval("RiskOriginal")%>' Width="4em"/></nobr></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><nobr><asp:TextBox ID="tbPFailure" runat="server" SkinID="TextInput" CssClass="as_number" Text='<%#Eval("RiskOriginal")%>' Width="4em"/></nobr></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><nobr><asp:TextBox ID="tbCost" runat="server" SkinID="TextInput" CssClass="as_number" Text='<%#Eval("Cost")%>' Width="6em"/></nobr></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center" Visible="false">
                    <ItemTemplate><asp:CheckBox ID="cbIsTolerance" runat="server" Checked='<%#Eval("AllowCostTolerance")%>'/></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center" Visible="false">
                    <ItemTemplate><nobr>&nbsp;<asp:TextBox ID="tbTolerance" runat="server" SkinID="TextInput" CssClass="as_number" Text='<%#Eval("CostTolerance")%>' Width="4em"/></nobr></ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ID" ReadOnly="true" ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center" />
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><asp:CheckBox ID="cbPartial" runat="server" Checked='<%#Eval("IsPartial")%>'/></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><nobr>&nbsp;<asp:TextBox ID="tbMinPrc" runat="server" SkinID="TextInput" CssClass="as_number" Text='<%#Eval("MinPercent")%>' Width="4em"/></nobr></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><asp:CheckBox ID="cbMusts" runat="server" Checked='<%#Eval("Must")%>'/></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" FooterStyle-HorizontalAlign="Center">
                    <ItemTemplate><asp:CheckBox ID="cbMustNot" runat="server" Checked='<%#Eval("MustNot")%>'/></ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView><span id="spanPages"></span>
        <asp:HiddenField ID="hfGridView1SV" runat="server" /> 
        <asp:HiddenField ID="hfGridView1SH" runat="server" />
        <asp:HiddenField ID="LinkedCC" runat="server" />
        <input type='hidden' id='tbLogs' value='<% = SafeFormString(RA.Logs) %>'/>
        <% If Not GridAlternatives.Columns(CInt(raColumnID.Musts)).Visible Then%><input type='hidden' name='tbMustsSum' value='<% =JS_SafeNumber(Scenario.Alternatives.Sum(Function(a) if(a.Must, 1, 0)))%>' /><% End If%>
        <% If Not GridAlternatives.Columns(CInt(raColumnID.MustNot)).Visible Then%><input type='hidden' name='tbMustNotSum' value='<% =JS_SafeNumber(Scenario.Alternatives.Sum(Function(a) if(a.MustNot, 1, 0)))%>' /><% End If%>
        <% If Not GridAlternatives.Columns(CInt(raColumnID.ProbFailure)).Visible Then%><input type='hidden' name='tbPFailureSum' value='<% =JS_SafeNumber(Scenario.Alternatives.Sum(Function(a) a.RiskOriginal))%>' /><% End If %>
        <asp:HiddenField ID="SolverActive" runat="server" /></div>

    <div id="divRATimelineOverride" style="width:100%;height:500px; position:absolute;text-align:center; background:#ffffff; opacity:0.3; display:none;">&nbsp;</div>
    <div id="RATimeline" style="overflow:auto; display:none; text-align:center">...</div>

    </div></td></tr></table><%--</telerik:RadAjaxPanel>--%>

</div></td></tr>
</table>

<telerik:RadCodeBlock runat="server" ID="radCodeBlockDlg">

<div id="divSolverLogs" style="display:none;"><textarea id="txtLogs" class="whole" style="width:100%; height:340px; border:1px solid #e0e0e0; font-family:'Courier New'; font-size:9pt;" readonly="readonly"></textarea></div>

<div id="divScenarios" style="display:none; text-align:center">
<h6><% = JS_SafeString(ResString("lblRAScenarios"))%>:</h6>
<div style="overflow:auto;" id="pScenarios"><table id="tblScenarios" border='0' cellspacing='1' cellpadding='2' style='width:96%' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header" align="center">
        <th style="width:1em">&nbsp;</th>
        <th><% = JS_SafeString(ResString("tblRAScenarioName"))%></th>
        <th><% = JS_SafeString(ResString("tblRAScenarioDesc"))%></th>
        <th><nobr><% = JS_SafeString(ResString("tblRAScenarioGroup"))%></nobr></th>
        <th width="80"><% = JS_SafeString(ResString("tblRAScenarioAction"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
</div>

<div id="divAddScenario" style="display:none; text-align:center">
<h6 style="padding-top:1ex;"><% = JS_SafeString(ResString("lblRAAddSpecificPortfolio"))%></h6>
<p align="center" style="margin:0px;"><table border='0' cellspacing='1' cellpadding='2'>
    <tr class="text"><td align="right"><nobr><% =ResString("tblRAScenarioName") %>:</nobr></td><td><input type="text" id="tbSPName" style="width:300px" /></td></tr>
    <tr class="text"><td align="right"><nobr><% =ResString("tblRAScenarioDesc") %>:</nobr></td><td><input type="text" id="tbSPDesc" style="width:300px" /></td></tr>
    <tr class="text"><td>&nbsp;</td><td><input type='checkbox' id='cbCopyMusts' checked/><label for='cbCopyMusts'><% = ResString("lblRACopyMusts") %></label><br />
        <input type='checkbox' id='cbCopyMustNot' checked/><label for='cbCopyMustNot'><% = ResString("lblRACopyMustNot") %></label></td></tr>
</table></p>
</div>


<div id="divColumns" style="display:none;">
<telerik:RadCodeBlock runat="server" EnableViewState="false">
<h6><% = JS_SafeString(ResString("lblRAOptColumns"))%>:</h6>
<div style='line-height:24px'>
<% =GetColumnsList()%>
<div style='margin-top:4px;'><input type='checkbox' id='cbShowConstraints2' onclick='onChangeSettings("showconstraints", this.checked); document.getElementById("cbShowConstraints2").checked=this.checked;'/><label for='cbShowConstraints2'><% = JS_SafeString(ResString("tblCustConstr"))%></label></div>
<div><input type='checkbox' id='cbShowAttributes' onclick='onChangeSettings("showattributes", this.checked); document.getElementById("cbShowAttributes").checked=this.checked;'/><label for='cbShowAttributes'><% = JS_SafeString(ResString("tblAltAttributes"))%></label></div>
</div>
</telerik:RadCodeBlock>
</div>

<div id="divModelsList" style="display:none;">
<h6><% = JS_SafeString(ResString("lblRASelectARM")) %>:</h6>
    <div style="overflow:auto; height:350px; border:1px solid #f0f0f0; padding:4px 2px;" class="text" id="divModels"></div>
    <div style="margin-top:4px"><b><% =ResString("lblRASearchModel")%></b>: <input type='text' name='inpSearch' id='inpSearch'  value='' onkeyup='onSearchRisk();' onmouseup='onSearchRisk();' style="width:200px" /></div>
</div>

<div class='text context-menu' id='divBudgetLimitCMenu' style='display:none'>
<a href='' class='context-menu-item' onclick='theForm.tbBudgetLimit.value=""; theForm.tbBudgetLimit.onblur(); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/clean-16.png' alt=''/><% =ResString("lblRAClearBudget")%></nobr></div></a>
<a href='' class='context-menu-item' onclick='theForm.tbBudgetLimit.value=theForm.TotalCost.value; theForm.tbBudgetLimit.onblur(); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/totalcost_16.png' alt=''/><% =ResString("lblRATotalCostBudget")%></nobr></div></a>
</div>

<div id="divSolverSettings" style="display:none;">
<h6><% = JS_SafeString(ResString("lblRASolverSettings"))%>:</h6>
<%--<table border="0" cellspacing="0" cellpadding="2">
<tr><td align="right" class="text"></td><td class="text"></td></tr>
<tr><td align="right" class="text"></td><td class="text"></td></tr>
<tr><td align="right" class="text"></td><td class="text"></td></tr>
</table>--%>

<div style="background:#f5f5f5; border:1px solid #e0e0e0; padding:4px; text-align:center;">
    <nobr><img src="../../Images/ra/assembly-20.png" width="20" height="20" style="margin-right:4px" /><b><% =ResString("lblRASolver") %></b>:
    <% If App.isXAAvailable Then%><input type='radio' id='cbSolver1' name='cbSolver' value="<% =CInt(raSolverLibrary.raXA)  %>" onclick='showSolverSettings(this.value);'/><label for='cbSolver1'><% =ResString("lblRASolverXA")%></label> &nbsp; <% End if %>
    <% If App.isGurobiAvailable Then%><input type='radio' id='cbSolver2' name='cbSolver' value="<% =CInt(raSolverLibrary.raGurobi)%>" onclick='onSetSolverGurobi();'/><label for='cbSolver2'><% =ResString("lblRASolverGurobi")%></label> &nbsp; <% End if %>
    <% If App.isBaronAvailable Then%><input type='radio' id='cbSolver3' name='cbSolver' value="<% =CInt(raSolverLibrary.raBaron)%>" onclick='showSolverSettings(this.value);'/><label for='cbSolver3'><% =ResString("lblRASolverBaron")%></label><% End If %>
    </nobr>
</div>
<div id="divSolverXA" style="padding:1em 1em 0px 1em; display:none; text-align:center">
<div class="legend_title" style="margin:4px; padding-bottom:4px; border-bottom:1px solid #f0f0f0;">Advanced settings:</div>
<p align="center" style="padding:0px;margin:0px;"><table border="0" cellspacing="0" cellpadding="2">
<tr><td align="right" class="text"><% = ResString("lblRAStrategy")%>:</td><td class="text"><% =GetRA_Strategy(raSolverLibrary.raXA)%></td><td></td></tr>
<tr><td align="right" class="text"><% = ResString("lblRAVariation")%>:</td><td class="text"><% =GetRA_Variation(raSolverLibrary.raXA)%></td><td></td></tr>
<tr><td align="right" class="text"><% = ResString("lblRASolverTimeout")%>:</td><td class="text" id="tdXATimeout"></td><td class="text"><% =ResString("lblRATimeoutSec")%></td></tr>
<tr><td align="right" class="text"><% = ResString("lblRASolverTimeoutUnchanged")%>:</td><td class="text" id="tdXATimeoutUnch"></td><td class="text"><% =ResString("lblRATimeoutSec")%></td></tr>
</table></p>
</div>

<div id="divSolverNoSettings" style="padding-top:5em; text-align:center; display:none;" class="gray"><% = ResString("lblRASolverNoSettings") %></div>

</div>

<div id="divSolverPrty" style="display:none; text-align:center">
<div id="divSPriortyHints">
<%--<h6><% = JS_SafeString(ResString("lblRASolverPriorities"))%>:</h6>--%>
<p class="text" style="text-align:center;"><% = JS_SafeString(ResString("lblRASolverPrtyHint"))%></p>
<p class="text small note" style="text-align:center; display:none; margin:0px 4px;" id="lblRASPrtyCCWarning"><span class='small'><% = JS_SafeString(ResString("lblRASolverPrtyCCWarning"))%></span></p>
<p class="text small infopanel" style="text-align:center; display:none; margin:3px 4px 8px 4px" id="lblRASPrtyWarning"><% = JS_SafeString(ResString("lblRASolverPrtyXAHint"))%></p>
</div>
<div style="overflow:auto;" id="divSPriorties"><p align="center" style="padding:0px; margin:0px"><table id="tblSPriorities" border='0' cellspacing='1' cellpadding='2' style='width:99%' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header" align="center">
        <th style="width:20px;">&nbsp;</th>
        <%--<th width="40"><% = JS_SafeString(ResString("tblRASPrtyID"))%></th>--%>
        <th><% = JS_SafeString(ResString("tblRASPrtyField"))%></th>
        <th width="50"><% = JS_SafeString(ResString("tblRASPrtyInUse"))%></th>
        <th width="50"><% = JS_SafeString(ResString("tblRASPrtyCond"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></p></div>
</div>

<div id="divCheckInfeas" style="display:none;">
<h6><% = JS_SafeString(ResString("lblRACheckInfeas")) %>:</h6>
    <div style="border:0px solid #f0f0f0; padding:4px 2px;" class="text" id="divInfeasList"></div>
</div>

</telerik:RadCodeBlock>
</asp:Content>