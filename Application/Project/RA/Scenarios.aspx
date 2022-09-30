<%@ Page Language="VB" Inherits="RAScenariosPage" title="Resource Aligner" Codebehind="Scenarios.aspx.vb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.jqplot.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.barRenderer.min.js"></script>
<%--<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.pieRenderer.min.js"></script>--%>
<%--<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.donutRenderer.min.js"></script>--%>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.pieRenderer.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.donutRenderer.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.categoryAxisRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisLabelRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisTickRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasTextRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/hPie.js"></script>
<script language="javascript" type="text/javascript">  
    
    var ShowDraftPages = <%=CStr(IIf(ShowDraftPages, "true", "false"))%>;

    var OPT_AUTO_EXPAND_SCENARIOS   = false;
    var OPT_MIN_PIE_PLOT_SIZE       = <%=OPT_MIN_PIE_PLOT_SIZE %>;
    var OPT_MAX_PIE_PLOT_SIZE       = <%=OPT_MAX_PIE_PLOT_SIZE %>;
    
    var OPT_PLOT_LABEL_DEFAULT_COLOR   = "#212121";
    var OPT_PLOT_LABEL_HIGHLIGHT_COLOR = "#ffffff";
    var OPT_LEGEND_HIGHLIGHT_COLOR     = "#ffff99";

    var OPT_SCENARIO_LEN     = 45; //A0965
    var OPT_DESCRIPTION_LEN = 200; //A0965
    
    var has_init = 0;
    var on_error_code = "";
    var on_ok_code = "";

    var use_scrolling = <% =CStr(IIf(RA_Scrolling, "true", "false")) %>;

    var DisplayField = <% = CInt(DisplayField) %>;
    var dfFunded = 0;
    var dfBenefit = 1;
    var dfEBenefit = 2;
    var dfPFailure = 3;
    var dfCost = 4;
    var dfMust = 5;
    var dfMustNot = 6;
    var dfCustomConstraints = 7;
    var dfFundingByCategory = 8;
    var dfObjPriorities = 9;
    var dfDependencies  = 10;
    var dfGroups        = 11;
    var dfMustsAndMustNots = 12;
    var dfRisk = 14;
    var dfPSuccess = 15;

    var ViewMode = <% = CInt(ViewMode) %>;
    var vmGridView = 0;
    var vmColumnChart  = 1;
    var vmPieChart  = 2;
    var vmDonutChart= 3;

    var GroupingMode = <% = CInt(GroupingMode) %>;
    var gmScenario    = 0;
    var gmAlternative = 1;

    var ObjPriorityMode = <% = CInt(ObjPriorityMode) %>;
    var pmLocal = 0;
    var pmGlobal = 1;

    var FundedFilteringMode = <% = CInt(FundedFilteringMode) %>;
    var ffAll = 0;
    var ffFunded = 1;
    var ffPartiallyFunded = 2;
    var ffFullyFunded = 3;
    var ffNotFunded = 4;

    var ShowExcludedAlts = <% = CStr(IIf(ShowExcludedAlts, "true", "false")) %>;

    var scenarios = [<% =GetScenarios() %>];
    var IDX_ID   = 0;
    var IDX_NAME = 1;
    var IDX_CHK  = 2;
    var IDX_DESCRIPTION = 3; 
    var IDX_SUM_RISK = 4; 
    
    var IDX_AXIS_LABELS  = 0;
    var IDX_SERIES_NAMES = 1;
    var IDX_SERIES_DATA  = 2;
    var IDX_Y_LABEL      = 3;
    var IDX_Y_CATEGORICAL= 4;
    var IDX_Y_TICKS      = 5;
    var IDX_SERIES_COLORS= 6;

    var alts_count = <%=RA.Scenarios.Scenarios(0).Alternatives.Count() %>;

    var dlg_editor = null;
    var dlg_customize = null;
    var plot_data = [];
    var scenarios_chart = null;
    var editing_scenario = null;

    var last_pointIndex = -1;

    var palette_id = 0;

    //var IsGraphicalMode = "< %=IsGraphicalMode%>" == "True";
    //var ScenarioSettingRisks = "< % =RA.ScenarioComparisonSettings.Risks %>" == "True";

    function DoEnable(e) {    
        if (e == 0) { displayLoadingPanel(true); } else { displayLoadingPanel(false); };
        if (e == 0) {
            document.getElementById("cbFields").disabled = true;
            document.getElementById("cbViewMode").disabled = true;
            document.getElementById("cbGroupBy").disabled = true;
            //$("#cbGridViewMode").prop("disabled", "disabled");
            $("#cbCategories").prop("disabled", "disabled");
            $("#cbFundedFilter").prop("disabled", "disabled");        
            $("#cbObjPrty").prop("disabled", "disabled");            
            document.getElementById("<%=pnlIgnore.ClientID %>").disabled = true;
            document.getElementById("<%=pnlBaseCase.ClientID %>").disabled = true;
        }
        var toolbar = $find("<%= RadToolBarMain.ClientID %>");
        var button = toolbar.findItemByValue("export");
        var combo = toolbar.findItemByText("<%=ResString("lblSelectScenarios") %>");
        if (e == 0) { if ((button)) button.disable(); if ((combo)) combo.disable(); } else { if ((button)) button.enable(); if ((combo)) combo.enable(); };
    }

    function sendCommand(cmd) {
        var am = $find("<%=RadAjaxManagerMain.ClientID%>");
        if ((am)) { am.ajaxRequest(cmd); setTimeout('DoEnable(0);', 10); return true; }
    }    

    <%--function onRequestError(sender, e) {
        if (on_error_code != "") eval(on_error_code);
        on_error_code = "";
        on_ok_code = "";
        DoEnable(1);
        dxDialog("<%=ResString("lblCallbackError") %>\n<%=ResString("lblStatusCode") %>: " + e.Status + "\n<%=ResString("lblResponseText") %>: " + e.ResponseText, true, null);
    }--%>

    function onResponseEnd(sender, argmnts) {
        on_error_code = "";
        if (on_ok_code != "") eval(on_ok_code);
        on_ok_code = "";
        DoEnable(1);
        UpdateButtons();        
        initMode();
        resizeGrid();
    };

    function onSetField(value) {
        DisplayField = value*1;
        var lbl = document.getElementById("lblFieldTitle");        
        if ((lbl)) {        
            lbl.innerHTML = "";
            switch (value*1) {
                case dfFunded:            lbl.innerHTML = "<%=ResString("optFunded") %>"; break;
                case dfBenefit:           lbl.innerHTML = "<%=ResString("lblBenefit") %>"; break;
                case dfEBenefit:          lbl.innerHTML = "<%=ResString("lblExpectedBenefit") %>"; break;
                case dfPFailure:          lbl.innerHTML = "<%=ResString("optPFailure") %>"; break;
                case dfPSuccess:          lbl.innerHTML = "<%=ResString("optPSuccess") %>"; break;
                case dfRisk:              lbl.innerHTML = "<%=ResString("optRisk") %>"; break;
                case dfCost:              lbl.innerHTML = "<%=ResString("optCost") %>"; break;
                case dfMust:              lbl.innerHTML = "<%=ResString("optMust") %>"; break;
                case dfMustNot:           lbl.innerHTML = "<%=ResString("optMustNot") %>"; break;
                case dfMustsAndMustNots:  lbl.innerHTML = "<%=ResString("optMustsAndMustNots") %>"; break;
                case dfDependencies:      lbl.innerHTML = "<%=ResString("optDependencies") %>"; break;
                case dfGroups:            lbl.innerHTML = "<%=ResString("optGroups") %>"; break;
                case dfCustomConstraints: lbl.innerHTML = "<%=ResString("optCustomConstraints") %>"; break;
                case dfFundingByCategory: lbl.innerHTML = "<%=ResString("optFundingByCategory") %>"; break;
                case dfObjPriorities:     lbl.innerHTML = "<%=ResString("optObjPriorities") %>"; break;
            }
            lbl.innerHTML = "(" + lbl.innerHTML + ")";            
        }
        sendCommand("action=field&value=" + value);
    }    

    function getScenarioById(id) {
        var scenarios_len = scenarios.length;
        for (i=0;i<scenarios_len;i++) {
            if (scenarios[i][IDX_ID] == id) return scenarios[i];
        }
        return null;
    }

    function anyScenarioHasRisks() {
        var scenarios_len = scenarios.length;
        var sum_risk = 0;
        for (i=0;i<scenarios_len;i++) {
            if (scenarios[i][IDX_CHK]*1 == 1) sum_risk += scenarios[i][IDX_SUM_RISK]*1;
        }
        return sum_risk > 0;
    }

    function InitScenarios() {
        $("#<%=divGrid.ClientID%>").hide();
        $("#<%=divChart.ClientID%>").hide();

        var lbl = document.getElementById("lblScenarios");
        if ((lbl) && (scenarios)) {
            lbl.innerHTML = "";
            var scenarios_len = scenarios.length;
            for (var i=0; i<scenarios_len; i++)
            {                
                var a = scenarios[i];
                lbl.innerHTML += "<div><label style='cursor:default;'><input type='checkbox' class='cb_scenario' id='tpl" + i + "' name='tpl" + i + "' value='" + a[IDX_ID] + "' " + (a[IDX_CHK] == 1 ? " checked='checked' " : "") +" onclick='return onScenarioClick(this.value, this.checked);' onkeydown='return onScenarioClick(this.value, this.checked);' title='" + a[IDX_NAME] + "' >&nbsp;&nbsp;" + ShortString(a[IDX_NAME], OPT_SCENARIO_LEN) + "</label>" + (a[IDX_DESCRIPTION] == "" ? "" : " (" + ShortString(a[IDX_DESCRIPTION], OPT_DESCRIPTION_LEN) + ")")+"</div>"; 
            }
        }
        UpdateButtons();
        if (OPT_AUTO_EXPAND_SCENARIOS) setTimeout("HighlightSelectScenarios();", 200);
    }

    function onToolbarDropdownClosed(sender, args) {
        onScenariosOK_Click();
    } 

    function onScenariosOK_Click() {
        var ids = "";
        for (i=0; i<scenarios.length; i++) {
            ids += (scenarios[i][IDX_CHK] == 1 ? (ids == "" ? "" : "-") + scenarios[i][IDX_ID] + "" : "");
        }
        sendCommand("action=scenarios_load&ids=" + ids);
        return true;
    }

    function onScenarioClick(id, chk) {
        var sc = getScenarioById(id);
        if ((sc)) sc[IDX_CHK] = (chk ? 1 : 0);
        UpdateScenariosButtons();
        return true;
        //sendCommand("action=scenario_checked&id=" + id + "&chk=" + chk); return true
    }

    function checkAllScenarios(chk) {
        for (i=0; i<scenarios.length; i++) {scenarios[i][IDX_CHK] = (chk ? 1 : 0)};
        $("input:checkbox.cb_scenario").prop("checked", chk);
        UpdateScenariosButtons();
        //sendCommand("action=check_all&chk=" + chk);        
    }    

    function UpdateScenariosButtons() {
        var scen_len = scenarios.length;        
        var checked_len = $("input:checkbox.cb_scenario:checked").length;

        $("#btnAll"  ).prop("disabled", (scen_len == 0) || (checked_len == scen_len));
        $("#btnNone" ).prop("disabled", (scen_len == 0) || (checked_len == 0));        
    }

    function UpdateButtons() {
        var scen_len = scenarios.length;        
        var checked_len = $("input:checkbox.cb_scenario:checked").length;

        UpdateScenariosButtons();
        $("#cbFields").prop("disabled", checked_len == 0);
        $("#cbFields option[value=<%=Cint(DisplayFields.dfEBenefit)%>]").prop('disabled', !anyScenarioHasRisks());
        $("#cbFields option[value=<%=Cint(DisplayFields.dfPFailure)%>]"    ).prop('disabled', !anyScenarioHasRisks());
        $("#cbFields option[value=<%=Cint(DisplayFields.dfDependencies)%>]").prop('disabled', ViewMode == vmColumnChart);
        $("#cbFields option[value=<%=Cint(DisplayFields.dfGroups)%>]").prop('disabled', ViewMode == vmColumnChart);
        $("#cbFields option[value=<%=Cint(DisplayFields.dfCustomConstraints)%>]").prop('disabled', ViewMode == vmColumnChart);
        $("#cbFields option[value=<%=Cint(DisplayFields.dfFundingByCategory)%>]").prop('disabled', (ViewMode == vmColumnChart) || (<%=IIf(AttributesList.Count=0, "true","false") %>));
        $("#cbViewMode").prop("disabled", (checked_len == 0 || <%=IIf(RA.Scenarios.Scenarios(0).Alternatives.Count = 0, "true", "false") %>) || ($("#cbFields").val() == dfDependencies || $("#cbFields").val() == dfGroups || $("#cbFields").val() == dfCustomConstraints || $("#cbFields").val() == dfFundingByCategory));
        
        var canViewCharts = (DisplayField == dfBenefit) || (DisplayField == dfEBenefit) || (DisplayField == dfPFailure)  || (DisplayField == dfPSuccess)  || (DisplayField == dfRisk) || (DisplayField == dfObjPriorities);        
        //document.getElementById("cbViewMode").options[<%=Cint(ViewModes.vmPieChart)%>].disabled = !canViewCharts;
        //document.getElementById("cbViewMode").options[<%=Cint(ViewModes.vmDonutChart)%>].disabled = !canViewCharts;
        $("#cbViewMode option[value=<%=Cint(ViewModes.vmPieChart)%>]").prop('disabled', !canViewCharts);
        $("#cbViewMode option[value=<%=Cint(ViewModes.vmDonutChart)%>]").prop('disabled', !canViewCharts);

        if (((ViewMode == vmPieChart) || (ViewMode == vmDonutChart)) && !canViewCharts) {
            ViewMode = vmGridView;
            $("#cbViewMode").val("<%=Cint(ViewModes.vmGridView)%>");
        }

        $("#cbGroupBy").prop("disabled", (checked_len == 0) || ViewMode != vmColumnChart);
        document.getElementById("cbGroupBy").options[1].text = (DisplayField == dfObjPriorities ? "<%=ResString("tblObjective") %>" : "<%=ResString("tblAlternative") %>");
        //$("#cbGridViewMode").prop("disabled", < %=IIf(RA.Scenarios.Scenarios(0).Alternatives.Count = 0, "true", "false") %>);
        $("#cbCategories").prop("disabled", <%=IIf(AttributesList.Count = 0, "true", "false") %>);
        $("#cbFundedFilter").prop("disabled", false);        
        $("#cbObjPrty").prop("disabled", <%= IIf(RA.ProjectManager.Hierarchy(RA.ProjectManager.ActiveHierarchy).Nodes.Count < 1, "true", "false") %>);
        $("#<%=pnlIgnore.ClientID%>").prop("disabled", checked_len == 0);
        $("#<%=pnlBaseCase.ClientID%>").prop("disabled", checked_len == 0);
        
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");        
        if ((toolbar)) {
            var btn =  toolbar.findItemByValue("export");
            if ((btn)) btn.set_enabled(checked_len != 0);

            var cust_btn =  toolbar.findItemByValue("customize");
            if ((cust_btn) && (ShowDraftPages)) cust_btn.set_visible(ViewMode != vmGridView);
            
            var lblCat =  toolbar.findItemByValue("lbl_ctgs");
            var btnCat =  toolbar.findItemByValue("btnViewCategories");
            var lblMode =  toolbar.findItemByValue("lbl_mode");
            var btnMode =  toolbar.findItemByValue("btnViewOptions");
            var lblView =  toolbar.findItemByValue("lbl_view");
            var btnView =  toolbar.findItemByValue("btnViewMode");
            var lblGroup =  toolbar.findItemByValue("lbl_group_by");
            var btnGroup =  toolbar.findItemByValue("btnGroupBy");
            if ((btnCat) && (lblCat) && (btnMode) && (lblMode) && (btnView) && (lblView) && (btnGroup) && (lblGroup)) {
                if ($("#cbFields").val() == dfFundingByCategory) {
                    lblCat.show(); lblMode.hide(); lblGroup.hide();
                    btnCat.show(); btnMode.hide(); btnGroup.hide();
                } else {
                    lblCat.hide(); lblMode.show(); lblGroup.show();
                    btnCat.hide(); btnMode.show(); btnGroup.show();
                }

                if ($("#cbViewMode").prop("disabled") || $("#cbViewMode").val() != vmColumnChart) {
                    lblGroup.hide(); btnGroup.hide();
                } else {
                    lblGroup.show(); btnGroup.show();
                }

                if (!($("#cbFields").val() == dfFundingByCategory) && ($("#cbFields").val() == dfObjPriorities || $("#cbViewMode").prop("disabled") || $("#cbViewMode").val() == vmGridView)) {
                    lblView.hide(); btnView.hide();
                } else {
                    lblView.show(); btnView.show();
                }
            }
            //var lblScenCount =  toolbar.findItemByValue("lbl_scen_count");
            var lblScenCount =  document.getElementById("lbl_scen_count");
            if ((lblScenCount)) {
                var scenAll = scenarios.length;
                var scenSel = 0;
                for (var i=0; i<scenAll; i++) { 
                    if (scenarios[i][IDX_CHK]*1 == 1) scenSel += 1;
                }
                lblScenCount.innerHTML = "<small>" + scenSel + "/" + scenAll + "</small>";
                //lblScenCount.set_toolTip(scenSel + "/" + scenAll);
            }
            
            var btnShowExcluded = toolbar.findItemByValue("btnShowExcluded");
            if ((btnShowExcluded)) {
                if (((ViewMode == vmPieChart) || (ViewMode == vmDonutChart)) && canViewCharts && (DisplayField != dfObjPriorities)) {
                    btnShowExcluded.show();
                } else {
                    btnShowExcluded.hide();
                }
            }
        }

        $(".empty_gridview").parents("table").css("border-width", "0px").prop("border", "0");
        switchIgnores();
        switchBaseCase();        
    }

    function DoSort(idx, dir) {
        sendCommand("action=sort&idx=" + idx + "&dir=" + dir);
    }

    var is_context_menu_open = false;

    function showMenu(event, scenario_id) {
        scen_id  = scenario_id;
        scen_name  = "";
        scen_descr = "";
                
        is_context_menu_open = false;                
        
        if ((scenarios)) {   
            var scenario = getScenarioById(scenario_id);
            
            $("div.context-menu").hide().remove();
            var sMenu = "<div class='context-menu'>";
               sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameClick(" +'"' + scenario_id +'"' + "); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../images/ra/edit_small.gif' alt='' >&nbsp;&nbsp;&nbsp;Edit&nbsp;</nobr></div></a>";
                var cb = document.getElementById("cbFields");
                //if ((!window.chrome) && (window.clipboardData) && (cb) && ((cb.value == 2) || (cb.value == 3))) {
                if ((cb) && ((cb.value == 2) || (cb.value == 3))) {
                    sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuPasteClick(" +'"' +  scenario_id +'"' + "); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../images/ra/paste-16.png' alt='' >&nbsp;<%=ResString("titlePasteFromClipboard") %>&nbsp;</nobr></div></a>";
                }
               sMenu += "</div>";                
            //var x = event.clientX;
            //var y = event.clientY;
            var img = document.getElementById("mnu_img_" + scenario_id);
            if ((img)) {
                var rect = img.getBoundingClientRect();
                var x = rect.left+ 2;
                var y = rect.top + 12;
                var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});                        
                if ((s)) { var w = s.width();var pw = $("#divMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

                $("div.context-menu").fadeIn(500);
                setTimeout('canCloseMenu()', 200);
            }
        }
    }

    function canCloseMenu() {
        is_context_menu_open = true;
    }

    function onContextMenuRenameClick(scenario_id) {
        $("div.context-menu").hide(200); is_context_menu_open = false; 
        editing_scenario = getScenarioById(scenario_id);
        initEditorForm("Edit", editing_scenario[IDX_NAME], editing_scenario[IDX_DESCRIPTION]);
        dlg_editor.dialog("open");
        dxDialogBtnDisable(true, true);
        if ((theForm.tbScenarioName)) theForm.tbScenarioName.focus();       
    }

    function onContextMenuPasteClick(scenario_id) {
        if (window.clipboardData) {
            var data = window.clipboardData.getData('Text');
            pasteData(scenario_id, data);
        } else {
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome('" + scenario_id + "');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
        }
    }

    function commitPasteChrome(scenario_id) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {            
            pasteData(scenario_id, pasteBox.value);
        }
    }

    function pasteData(scenario_id, data) {
        sendCommand("action=paste&scenario_id="+scenario_id+"&data="+encodeURIComponent(data));
    }

    var scen_id  = -1;
    var scen_name  = "";
    var scen_descr = "";
    var cancelled = false;

    function initEditorForm(_title, name, descr) {
        scen_name  = name;
        scen_descr = descr;
        
        document.getElementById("tbScenarioName").value  = name;
        document.getElementById("tbScenarioDescr").value = descr;

        cancelled = false;

        dlg_editor = $("#editorForm").dialog({
              autoOpen: false,
              modal: true,
              width: 440,
              dialogClass: "no-close",
              closeOnEscape: true,
              bgiframe: true,
              title: _title,
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                  dlg_editor.dialog( "close" );
                }},
                Cancel: function() {
                  cancelled = true;
                  dlg_editor.dialog( "close" );
                }
              },
              open: function() {
                $("body").css("overflow", "hidden");
                document.getElementById("tbScenarioName").focus();
              },
              close: function() {
                $("body").css("overflow", "auto");                
                if ((!cancelled) && (scen_name.trim() != "")) {
                    editing_scenario[IDX_NAME]        = scen_name.trim();
                    editing_scenario[IDX_DESCRIPTION] = scen_descr.trim();
                    sendCommand('action=edit&id='+scen_id+'&name='+encodeURIComponent(editing_scenario[IDX_NAME])+'&descr='+encodeURIComponent(editing_scenario[IDX_DESCRIPTION]));
                }
              }
        });
    }

    function initCustomizeForm() {
        cancelled = false;

        dlg_customize = $("#customizeForm").dialog({
            autoOpen: false,
            modal: true,
            width: "80%",
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "Customize",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                    dlg_customize.dialog( "close" );
                }},
                Cancel: function() {
                    cancelled = true;
                    dlg_customize.dialog( "close" );
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
                initPalettes();
                //$('input:radio[name=rbPalette]').filter('[value=' + palette_id + ']').prop('checked', true);
            },
            close: function() {
                $("body").css("overflow", "auto");                
                if ((!cancelled)) {
                    // get selected palette id
                    palette_id = $("input:radio[name=rbPalette]:checked").val();
                    
                    // parse values
                    var palette_html = $("#tbPalette" + palette_id).html();
                    var colors = palette_html.split( "\n" );
                    ColorPalette = [];
                    for (var i = 0; i < colors.length; i++) {
                        if (colors[i].trim() != "") ColorPalette.push(colors[i]);
                    }
                    
                    // rebuild charts
                    initMode(); 

                    // update pie/donut color legend marks
                    var cp_len = ColorPalette.length;

                    $('.pie-legend-mark').each(function(indx, element){
                        var color_id = indx % cp_len;
                        $(element).css("background-color", ColorPalette[color_id]);
                    });
                }
            }
        });
    }

    function initPalettes() {
        var p = "";
        for (var i=0; i<ec_palettes.length; i++) {
            p += "<td valign='top' style='padding-left:8px; padding-bottom:2px;" + (ec_palettes[i][1]!="" ? "background-color:" + ec_palettes[i][1] : "") + "'><label><input type='radio' value='" + i + "' name='rbPalette'" + (palette_id == i ? " checked='checked'" : "") + "><nobr>" + ec_palettes[i][0] + "</nobr></label>";
            p += "<div id='divPalette" + i + "' style='overflow-y:auto; overflow-x:hidden; border:1px solid #ccc; background:#ffffff; width:8em;'><table border=0 cellspacing=0 cellpadding=2><tr valign=top><td style='width:15px;' id='tdPalettes" + i + "' align=right class='text'>&nbsp;&nbsp;</td><td class='text'><textarea id='tbPalette" + i + "' rows=15 cols=10 class='textarea' style='background-color:transparent; border:0px; overflow-x:auto; overflow-y:hidden; font-family:sans-serif, arial; font-size:" + (is_ie ? 13 : 12) +"px;' onkeyup='refreshPalette(" + i + ");' " + (i == 0 ? "readonly='readonly'" : "") + ">";
            p += "</textarea></td></tr></table></div></td>\r\n";
        }
        $("#trPalettes").html(p);
        for (var i=0; i<ec_palettes.length; i++) { $("#tbPalette" + i ).val(ec_palettes[i][2]); refreshPalette(i); }
    }
        
    function refreshPalette(id) {
        var t = $("#tbPalette" + id);
        if ((t)) {
            $("#divPalette" + id).height(10).height($("#trPalettes").height()-30);
            var c = replaceString("\r", "", t.val());
            var l = c.split("\n");
            t.prop("rows", l.length);
            var lst = "";
            for (var i=0; i<l.length; i++) {
                var r = "";
                var clr = replaceString(" ", "", l[i]);
                if (clr!="" && clr[0]=="#" && (clr.length==4 || clr.length==7) && (/^#[0-9A-F]{3,6}$/i.test(clr))) {
                    r = "<img src='../../images/ra/legend-15.png' width='15' height='15' title='' border='0' style='background-color:" + clr + "'>";
                }
                else {
                    if (clr!="" && clr!="#") r = "<img src='../../images/ra/legend-15_tr.png' width='15' height='15' title='' border='0'>";
                }
                lst += (lst=="" ? "" : "<br>") +  r;
            }
            $("#tdPalettes" + id).html(lst);
        }
    }
    
    function getScenarioName(sName)
    {
        scen_name = sName;        
        dxDialogBtnDisable(true, scen_name.trim() == "");
    }

    function getScenarioDescr(sDescr)
    {
        scen_descr = sDescr;
    }

    function onClickToolbar(sender, args) {
        var button = args.get_item();
        if ((button)) {
            var btn_id = button.get_value();
            if ((btn_id) && (btn_id == "export")) {
                //render plot as image (if Graphical mode)
                if (ViewMode == vmColumnChart) {
                    var imgData = $('#scenarios_chart').jqplotToImageStr({}); //works only for non-IE browsers (which support canvas)
                    if (imgData != null) {                        
                        CreatePopup(imgData, "_blank", "");                        
                    } else {
                        DevExpress.ui.notify("Your browser is not supported", "error");
                    }
                    return false;
                }
                //download file
                CreatePopup(document.location.href + (document.location.href.indexOf("?") > -1 ? "&" : "?") + "action=download", "export", ""); 
                return false;
            }
            if ((btn_id) && (btn_id == "customize")) {
                initCustomizeForm();
                dlg_customize.dialog("open");
            }
        }
    }

    function closeScenariosDropdown() {
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var btn =  toolbar.findItemByText("<%=ResString("lblSelectScenarios") %>");
            if ((btn)) btn.hideDropDown();
        }
    }

    function onSetViewMode(value) {
        //IsGraphicalMode = value == 1;
        ViewMode = value*1;
        sendCommand("action=view_mode&mode="+value);
    }
    
    function onSetGroupBy(value) {
        GroupingMode = value;
        sendCommand("action=group_mode&mode="+value);
    }

//    function onSetGridViewMode(value) {
//        GridViewMode = value*1;
//        sendCommand("action=grid_mode&mode="+ value);
//    }

    function onSetObjPrtyMode(value) {
        ObjPriorityMode = value*1;
        sendCommand("action=obj_mode&mode="+ value);
    }

    function initMode() {
        if (ViewMode == vmColumnChart) {            
            $("#<%=divGrid.ClientID%>").hide();
            $("#<%=divChart.ClientID%>").show();    
            $("#divColorLegend").show();
            setTimeout("initChartSize(); createChart();", 50);
        } else {
            $("#<%=divChart.ClientID%>").hide();
            $("#<%=divGrid.ClientID%>").show();
            $("#divColorLegend").hide();
            //$("#divColorLegend").width(0);
            //$("#tdColorLegend").width(0);
            if ((ViewMode == vmPieChart) || (ViewMode == vmDonutChart))  setTimeout("createTableCharts()", 50);
        };
        switchExportButton();
    }

    function createChart() {
        var d = document.getElementById("<% =DivPlotData.ClientID %>");
        if ((ViewMode == vmColumnChart) && (d)) {            
            $('#scenarios_chart').html("");
            var data = eval(d[text]);
            if ((data) && (data.length >= 2)) {
                var ticks = data[IDX_AXIS_LABELS];
                var names = data[IDX_SERIES_NAMES];
                var arr   = data[IDX_SERIES_DATA];
                var ylabel= data[IDX_Y_LABEL];
                var yticks= data[IDX_Y_TICKS];
                var y_categorical = data[IDX_Y_CATEGORICAL] == "1";
                var colors = ((DisplayField == dfObjPriorities) || (GroupingMode == gmAlternative || palette_id != 0) ? ColorPalette : data[IDX_SERIES_COLORS]); //color palette from misc.js
                
                //check if data array is empty or all array elements are empty
                var arr_len = arr.length;
                var has_data = arr_len > 0;
                
                if (has_data) {
                    var k = 0;
                    for (var i=0; i<arr_len; i++) {
                        if ((arr[i]).length == 0) k+=1;
                    }
                    has_data = k < arr_len;    
                }
                
                if (!has_data)
                {
                    $("#divColorLegend").hide();
                    $("#scenarios_chart").html("<table border=0 class='whole'><tr><td valign='middle' align='center'><h6><%=ResString("msgRiskResultsNoData")%></h6></td></tr></table>");
                    return false;
                }
                
                scenarios_chart = $.jqplot ('scenarios_chart', arr, {
                    animate: !$.jqplot.use_excanvas,
                    series: names,
                    seriesColors: colors,
                    seriesDefaults: {
                        renderer: $.jqplot.BarRenderer,
                        rendererOptions: { fillToZero: DisplayField == dfMustsAndMustNots },
                        pointLabels: { show: false }
                    },
                    axes: { 
                        xaxis: { 
                            renderer: $.jqplot.CategoryAxisRenderer,
                            tickRenderer:$.jqplot.CanvasAxisTickRenderer,
                            ticks: ticks,
                            tickOptions:{
                                //labelPosition: 'middle', 
                                angle: (ticks.length > 5 ? -30 : 0)
                            } 
                        }, 
                        yaxis: { 
                            renderer: (y_categorical ? $.jqplot.CategoryAxisRenderer : $.jqplot.LinearAxisRenderer), 
                            labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                            ticks: yticks,
                            label: ylabel
                        } 
                    },
                    legend:{
                        show: false,
                        placement: 'outsideGrid'
                    } 
                });
                
                $('#scenarios_chart').unbind('jqplotDataHighlight');
                $('#scenarios_chart').bind('jqplotDataHighlight',
                function (ev, seriesIndex, pointIndex, data) {
                    pos_x = ev.pageX+10;
                    pos_y = ev.pageY+10;
                    var label = getTooltipLabel(data[1], (data.length > 1 ? data[1] : ""), (data.length > 2 ? data[2] : ""));
                    $('#chart_tooltip').css({ left: pos_x, top: pos_y });
                    $('#chart_tooltip').html('<h6>'+htmlEscape(names[seriesIndex].label)+'</h6>'+label); //Scenario:'+ticks[pointIndex]+'<br >
                    $('#chart_tooltip').show();
                    $('#divColorLegend').animate({scrollTop: $('#divColorLegend').scrollTop() + $('#legend_row_' + seriesIndex).offset().top - $('#divColorLegend').offset().top}, 100);
                    $('tr.legend_row').css('background', '#fff');
                    $('#legend_row_' + seriesIndex).css('background', OPT_LEGEND_HIGHLIGHT_COLOR);
                });

                // Bind a function to the unhighlight event to clean up after highlighting.
                $('#scenarios_chart').unbind('jqplotDataUnhighlight');
                $('#scenarios_chart').bind('jqplotDataUnhighlight',
                function (ev, seriesIndex, pointIndex, data) {
                    $('#chart_tooltip').empty();
                    $('#chart_tooltip').hide();
                });

                // init Color Legend
                var d = $("#divColorLegend");
                if ((d)) { d.show(); d.width(170); }
                var t = $("#tdColorLegend");
                if ((t)) { t.show(); t.width(170); }
                $("#TableColorLegend").empty();
                var legends = names;
                if ((colors) && (colors.length>0)) {
                    $.each(legends, function(index, val) {                       
                        //$('#TableColorLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:2px 2px;border:0px solid #ccc; width:18px;"><div style="width:16px;height:26px;border:1px solid #ccc;padding:1px;"><div class="text small" style="width:12px;height:24px;background:'+ColorPalette[index % ColorPalette.length]+'">&nbsp;</div></div></td><td style="word-wrap:break-word; border:0px solid #ccc; padding:1px 3px;" title="'+val.label+'" id="legend_row_'+index+'" class="text small" valign="center">'+val.label+'</td></tr>');
                        $('#TableColorLegend').append('<tr id="legend_row_'+index+'" class="legend_row" onmouseover="LegendRowHover(this,1,'+index+');" onmouseout="LegendRowHover(this,0,0);" valign="top"><td style="border:0px; width:16px;"><img src="../../images/ra/legend-15.png" width="15" height="15" title="" border="0" style="background-color:'+colors[index]+'"></td><td style="word-wrap:break-word; border:0px;" title="'+htmlEscape(val.label)+'" class="text small" valign="center">'+htmlEscape(val.label)+'</td></tr>');
                    });
                }
            }
        }        
    }

    function getTooltipLabel(val, fpercent, fcost) {
        var label = "";                    
        switch (DisplayField*1) {
            case dfFunded:            label = ""; break; //Funded
            case dfBenefit:           label = "Benefit: "+val; break;
            case dfEBenefit:          label = "Expected Benefit: "+val; break;
            case dfPFailure:          label = "P.Failure: "+val; break;
            case dfPSuccess:          label = "P.Success: "+val; break;
            case dfRisk:              label = "Risk: "+val; break;
            case dfCost:              label = "Cost: "+number2locale(val, true); break;
            case dfMust:              label = ""; break; //Must
            case dfMustNot:           label = ""; break; //Must Not
            case dfMustsAndMustNots:  label = ""; break; //Musts and Must Nots
            case dfDependencies:      label = ""; break; //Dependencies
            case dfGroups:            label = ""; break; //Groups
            case dfCustomConstraints: label = ""; break; //Custom Constraints
            case dfFundingByCategory: label = ""; break; //Funding by category
            case dfObjPriorities:     label = ""; break; //Objectives Priorities
        }
        if (typeof fpercent != 'undefined' && typeof fcost != 'undefined' && fpercent != "" && fcost != "" && FundedFilteringMode == ffPartiallyFunded) {
            label += "<br>" + "Funded:&nbsp;" + fpercent;
            label += "<br>" + "Funded Cost:&nbsp;" + fcost;
        }
        return label;
    }
    
    var piePlots = [];
    var piePlotsData; 
    var pieHPlots = [];      //hierarchical plots

    function createTableCharts() {
        piePlots = [];
        pieHPlots= [];
        var d = document.getElementById("<% =DivPlotData.ClientID %>");
        if (ViewMode != vmColumnChart && (d)) {            
            var data = eval(d[text]);
            piePlotsData = data;
            if ((data) && (data.length >= 2)) {
                var ticks  = data[IDX_AXIS_LABELS];
                var names  = data[IDX_SERIES_NAMES];
                var arr_all= data[IDX_SERIES_DATA];
                var ylabel = data[IDX_Y_LABEL];
                var yticks = data[IDX_Y_TICKS];
                var y_categorical = data[IDX_Y_CATEGORICAL] == "1";
                var colors = data[IDX_SERIES_COLORS];

                var arr_all_len = arr_all.length;

                var w = OPT_MIN_PIE_PLOT_SIZE;
                var ww = document.getElementById("divMain").clientWidth;                
                if ((ww > 0) && (arr_all_len + 1 > 0)) { w = Math.round(ww / (arr_all_len + 1)); if (w < OPT_MIN_PIE_PLOT_SIZE) w = OPT_MIN_PIE_PLOT_SIZE; if (w > OPT_MAX_PIE_PLOT_SIZE) w = OPT_MAX_PIE_PLOT_SIZE };                               
                $("#divPieLegend").height(w+20);  //hight of the pie chart + 10 top margin + 10 bottom margin             
                $("#divPieLegend").parent().css("padding-right", "0");
                
                var pie_chart;
                
                for (var i=0; i<arr_all_len; i++) {                    
                    var arr = arr_all[i];
                    var arr_len = arr.length;
                    $('#pieChart'+i).width(w).height(w);

                    //check if data array is empty or all array elements are empty
                    var has_data = arr_len > 0;
                    
                    var isSingleSlice = false;

                    if (has_data) {
                        var k = 0;
                        for (var p=0; p<arr_len-1; p++) {
                            if ((arr[p]).length == 0 || arr[p][1] == 0) k+=1;
                        }
                        has_data = k < arr_len-1;
                        isSingleSlice = k == arr_len-2;    
                    }
                    
                    if (!has_data) {
                        $('#pieChart'+i).html("<table border=0 class='whole'><tr><td valign='middle' align='center'><h6><%=ResString("msgRiskResultsNoData")%></h6></td></tr></table>");                                            
                    
                    } else {                    
                        if ((DisplayField == dfObjPriorities) && (ViewMode == vmDonutChart)) {
                            var canvas = document.getElementById("pieChart" + i);
                            if ((typeof G_vmlCanvasManager != 'undefined') && (G_vmlCanvasManager)) G_vmlCanvasManager.initElement(canvas);
                            
                            if ((canvas) && (canvas.getContext)) {
		                        var ctx = canvas.getContext("2d");

                                arr[0].push(arr[0][0]);

                                for (var p=1;p<arr.length;p++) {
                                    arr[p].push(ticks[p-1]);
                                }
                                
                                if ((canvas.width))  canvas.width  = w;
                                if ((canvas.height)) canvas.height = w;

                                $('#lblHPiePleaseWait'+i).html("");
                                var segments = hPiePerformLayout(ctx, w, w, arr,  ColorPalette, true);
                                pieHPlots.push(segments);
                            } else { 
                                $('#pieChart'+i).html("<h6>Error: Your browser is not supported</h6>");
                            }

                            <% = Cstr(IIf(OPT_PIE_BORDERS, "$('#pieChart'+i).css('border', '1px solid #ccc');", "")) %>
                        } else {
                            
                            $('#pieChart'+i).html("");

                            var renderer;
                            switch (ViewMode) {
                                case (vmPieChart):
                                    renderer = $.jqplot.PieRenderer;
                                    break;
                                case (vmDonutChart):
                                    renderer = $.jqplot.DonutRenderer;
                                    break;
                            }
                                                                  
                            pie_chart = $.jqplot("pieChart"+i, [arr], {
                                animate: !$.jqplot.use_excanvas,
                                seriesColors: ((colors) && (colors.length > i) && (palette_id == 0) ? colors[i] : ColorPalette), //ColorPalette, //color palette from misc.js
                                seriesDefaults: {
                                    //shadow: false,
                                    renderer: renderer,
                                    //rendererOptions: { showDataLabels: true, startAngle: -90, dataLabels: 'label', padding: 2, sliceMargin: 2, dataLabelPositionFactor: 0.65 }
                                    rendererOptions: { showDataLabels: true, startAngle: (isSingleSlice ? 0 : -90), sliceMargin: 0.5, dataLabelPositionFactor: 0.65 }
                                },
                                legend:{ show: false },
                                grid: {
                                    drawBorder: false, 
                                    drawGridlines: false,
                                    //background: '#ffffff',
                                    shadow:false
                                },
                                gridPadding:{ top:0, bottom:0, right:0, left:0 } 
                            });

                            piePlots.push(pie_chart);
                            var chartType = (ViewMode == vmPieChart ? "pie" : "donut");

                            <% = Cstr(IIf(OPT_PIE_BORDERS, "$('#pieChart'+i).css('border', '1px solid #ccc');", "")) %>
                            $('#pieChart'+i).children(".jqplot-"+chartType+"-series.jqplot-data-label:last").html("");

                            $('#pieChart'+i).unbind('jqplotDataHighlight');
                            $('#pieChart'+i).bind('jqplotDataHighlight',
                            function (ev, seriesIndex, pointIndex, data) {
                                var li = $("#legendRow" + pointIndex);
                                var li_el = document.getElementById("legendRow" + pointIndex);

                                if ((pointIndex < ticks.length) && (li) && (li_el)) {
                                    pos_x = ev.pageX+10;
                                    pos_y = ev.pageY+10;
                                    $('#chart_tooltip').css({ left: pos_x, top: pos_y });
                                    $('#chart_tooltip').html('<h6>'+htmlEscape(ticks[pointIndex])+'</h6>'+getTooltipLabel(data[0], (data.length > 2 ? data[2] : ""), (data.length > 3 ? data[3] : "")));
                                    $('#chart_tooltip').show();
                                    resetLegendHighlight();
                                    last_pointIndex = pointIndex;
                                    //li.children().css("background", OPT_LEGEND_HIGHLIGHT_COLOR);
                                    li.css("background", OPT_LEGEND_HIGHLIGHT_COLOR);
                                    //smooth scrolling
                                    $("#divPieLegend").animate({scrollTop: $("#divPieLegend").scrollTop() + li.offset().top - $("#divPieLegend").offset().top}, 100);   
                                    pieLegendRowHover(li_el, pointIndex, 1, i);
                                    highlightPointLabel(pointIndex);
                                } else {
                                    for (var i=0; i<piePlots.length;i++) {
                                       if (ViewMode == vmPieChart)   unhighlightPie  (piePlots[i]);
                                       if (ViewMode == vmDonutChart) unhighlightDonut(piePlots[i]);                                
                                    }
                                }
                            });

                            // Bind a function to the unhighlight event to clean up after highlighting.
                            $('#pieChart'+i).unbind('jqplotDataUnhighlight');
                            $('#pieChart'+i).bind('jqplotDataUnhighlight',
                            function (ev, seriesIndex, pointIndex, data) {
                                $('#chart_tooltip').empty();
                                $('#chart_tooltip').hide();
                                resetLegendHighlight();
                            });
                        
                        }
                    }
                }
            }
        }
    }

    function highlightPie (plot, sidx, pidx) {
        var s = plot.series[sidx];
        var canvas = plot.plugins.pieRenderer.highlightCanvas;
        canvas._ctx.clearRect(0,0,canvas._ctx.canvas.width, canvas._ctx.canvas.height);
        s._highlightedPoint = pidx;
        plot.plugins.pieRenderer.highlightedSeriesIndex = sidx;
        s.renderer.drawSlice.call(s, canvas._ctx, s._sliceAngles[pidx][0], s._sliceAngles[pidx][1], s.highlightColorGenerator.get(pidx), false);
    }

    function highlightDonut(plot, sidx, pidx) {
        var s = plot.series[sidx];
        var canvas = plot.plugins.donutRenderer.highlightCanvas;
        canvas._ctx.clearRect(0,0,canvas._ctx.canvas.width, canvas._ctx.canvas.height);
        s._highlightedPoint = pidx;
        plot.plugins.donutRenderer.highlightedSeriesIndex = sidx;
        s.renderer.drawSlice.call(s, canvas._ctx, s._sliceAngles[pidx][0], s._sliceAngles[pidx][1], s.highlightColors[pidx], false);
    } 
    
    function unhighlightPie(plot) {
        var canvas = plot.plugins.pieRenderer.highlightCanvas;
        canvas._ctx.clearRect(0,0, canvas._ctx.canvas.width, canvas._ctx.canvas.height);
        for (var i=0; i<plot.series.length; i++) {
            plot.series[i]._highlightedPoint = null;
        }
        plot.plugins.pieRenderer.highlightedSeriesIndex = null;
        plot.target.trigger('jqplotDataUnhighlight');
    }

    function unhighlightDonut(plot) {
        var canvas = plot.plugins.donutRenderer.highlightCanvas;
        canvas._ctx.clearRect(0,0, canvas._ctx.canvas.width, canvas._ctx.canvas.height);
        for (var i=0; i<plot.series.length; i++) {
            plot.series[i]._highlightedPoint = null;
        }
        plot.plugins.donutRenderer.highlightedSeriesIndex = null;
        plot.target.trigger('jqplotDataUnhighlight'); 
    }

    function highlightPointLabel(pointIndex) {
        //approach 1        
        $(".jqplot-data-label").css("color", OPT_PLOT_LABEL_DEFAULT_COLOR).css("font-weight", "normal");
        $(".jqplot-dgl" + pointIndex).css("color", OPT_PLOT_LABEL_HIGHLIGHT_COLOR).css("font-weight", "bold");
        
        //approach 2
        //$(".jqplot-data-label").css("color", OPT_PLOT_LABEL_DEFAULT_COLOR).css("font-weight", "normal");
        //for (var i=0; i<piePlots.length;i++) {
            //var lbl = $('#pieChart'+i).children(".jqplot-dgl" + pointIndex + ":first");
            //lbl.css("color", OPT_PLOT_LABEL_HIGHLIGHT_COLOR).css("font-weight", "bold").html(getTooltipLabel(piePlotsData[IDX_SERIES_DATA][i][pointIndex][0]));
        //}
    }

    function pieLegendRowHover(r, pointIndex, hover, except) {                
        //if (except < 0) $("#legendRow" + pointIndex).children().css("background", (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : ""));        
        if (except < 0) $("#legendRow" + pointIndex).css("background", (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : ""));        
        if ((piePlots)) {
            var piePlots_len = piePlots.length;
            if (hover == 1) {
                for (var i=0;i<piePlots_len;i++) {                
                    if (i != except) {
                        if (ViewMode == vmPieChart)   { highlightPie  (piePlots[i], 0, pointIndex); highlightPointLabel(pointIndex) }
                        if (ViewMode == vmDonutChart) { 
                            if (DisplayField != dfObjPriorities) {
                                highlightDonut(piePlots[i], 0, pointIndex); highlightPointLabel(pointIndex) 
                            } else {
                                
                            }
                        }
                    }
                }
            }
        }
    }

    function HpieLegendRowHover(r, nodeId, hover, except) {                
        //if (except < 0) $("#legendRow" + nodeId).children().css("background", (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : ""));        
        if (except < 0) $("#legendRow" + nodeId).css("background", (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : ""));        
        if ((pieHPlots)) {
            var pieHPlots_len = pieHPlots.length;
            for (var i=0;i<pieHPlots_len;i++) {                
                if (i != except) {                        
                    if ((ViewMode == vmDonutChart) && (DisplayField == dfObjPriorities)) {
                        var canvas = document.getElementById("pieChart" + i);
		                ctx = canvas.getContext("2d");
                        highlightHDonut(pieHPlots[i], nodeId, hover);
                    }
                }
            }
        }
    }

    function highlightHDonut(segments, nodeId, hover) { //segments, nodeId
        var segments_len = segments.length;
        for (var i=0;i<segments_len;i++) {
            var segment = segments[i];
            if (segment.nodeId == nodeId) {
                if (hover == 1) {
                    segment.color = ColorBrightness(segment.origColor, 0.7);
                } else {
                    segment.color = segment.origColor;
                }
                
                LayoutSegment(segment.startAngle, segment.endAngle, segment);
            }
        }
    }

    function resetLegendHighlight() {
        //if (last_pointIndex >= 0) { $("#legendRow" + last_pointIndex).children().css("background", ""); last_pointIndex = -1; }
        if (last_pointIndex >= 0) { $("#legendRow" + last_pointIndex).css("background", ""); last_pointIndex = -1; }
    }

    function switchExportButton() {
        //hide the Export button if this action is not supported        
        //var imgData = null;
        var toolbar = $find("<%= RadToolBarMain.ClientID %>");
        var button = toolbar.findItemByValue("export");
        //if (ViewMode == vmColumnChart) { imgData = $('#scenarios_chart').jqplotToImageStr({}); }
        //if ((button)) (ViewMode == vmColumnChart && imgData == null ? button.hide() : button.show());
        //if ((button)) button.set_enabled(!(ViewMode == vmColumnChart && imgData == null));
        if ((button)) button.set_visible(ViewMode == vmGridView);
    }
    
    function LegendRowHover(r, hover, sidx) {
        $('tr.legend_row').css('background', '');
        if (r.style.background != OPT_LEGEND_HIGHLIGHT_COLOR) {
            r.style.background = (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : '');
            if (hover == 1) {                
                if (scenarios_chart.series.length > 0) {
                    var points_len = scenarios_chart.series[sidx]._barPoints.length;
                    for (var pidx=0; pidx<points_len; pidx++) {                    
                        highlightBar(scenarios_chart, sidx, pidx, scenarios_chart.series[sidx]._barPoints[pidx]);
                    }
                }
            } else {
                unhighlightBar(scenarios_chart);
            }
        }
    }
    
    function highlightBar(plot, sidx, pidx, points) {
        var s = plot.series[sidx];
        var canvas = plot.plugins.barRenderer.highlightCanvas;
        //canvas._ctx.clearRect(0,0,canvas._ctx.canvas.width, canvas._ctx.canvas.height);
        s._highlightedPoint = pidx;
        plot.plugins.barRenderer.highlightedSeriesIndex = sidx;
        var opts = {fillStyle: s.highlightColors[pidx]};
        s.renderer.shapeRenderer.draw(canvas._ctx, points, opts);
        canvas = null;
    }
    
    function unhighlightBar(plot) {
        var canvas = plot.plugins.barRenderer.highlightCanvas;
        canvas._ctx.clearRect(0,0, canvas._ctx.canvas.width, canvas._ctx.canvas.height);
        for (var i=0; i<plot.series.length; i++) {
            plot.series[i]._highlightedPoint = null;
        }
        plot.plugins.barRenderer.highlightedSeriesIndex = null;
        plot.target.trigger('jqplotDataUnhighlight');
        canvas =  null;
    }    

    function initChartSize() {
        $("#scenarios_chart").height(200);
        $("#scenarios_chart").width(300);

        $("#<%=divGrid.ClientID%>").height(1);
        $("#<%=divGrid.ClientID%>").width(1);

        $("#divColorLegend").width(170);
        $("#tdColorLegend").width(170);
      
        var td = $("#tdGridAlts");
        var h = (td.innerHeight()-20 >200 ? td.innerHeight()-20 : 200);
        var w = (td.innerWidth()-2   >300 ? td.innerWidth()-2   : 300);
        $("#scenarios_chart").height(h);
        $("#scenarios_chart").width(w);

        $("#divColorLegend").height(h-35);

        initChartLegendScroll();
    }

    function initChartLegendScroll() {
        //var h = $("#scenarios_chart").height();
        //var legendTable = $($('.jqplot-table-legend')[0]);    
        //legendTable.css('display','block');
        //legendTable.css('z-index',100);
        //legendTable.css('height', h-100);
        //legendTable.css('overflow-y','scroll');
    }

    function resizeChart() {                
        if (ViewMode == vmColumnChart) {            
            initChartSize();
            if ((scenarios_chart)) scenarios_chart.replot({ resetAxes: false });
        }
    }

    function resizeGrid() {
        if (ViewMode != vmColumnChart) {            
            $("#<%=divGrid.ClientID %>").height(10);        
            $("#<%=divGrid.ClientID %>").height($("#tdGridAlts").innerHeight()-4);
        }
    }

    function SetOptAll(chk) {
        //var cb = theForm.cbIgnoreOptions;
        //if ((cb) && (!cb.checked)) return false;
        var rb = document.getElementById("rbIgnor1");
        if ((rb) && (!rb.checked)) return false;
        theForm.cbOptMusts.checked=chk;
        theForm.cbOptMustNots.checked=chk;
        theForm.cbOptConstraints.checked=chk;
        theForm.cbOptDependencies.checked=chk;
        theForm.cbOptGroups.checked=chk;
        theForm.cbOptFundingPools.checked=chk;
        theForm.cbOptRisks.checked = chk;
        if ((theForm.cbOptTimePeriods)) theForm.cbOptTimePeriods.checked=chk;
        sendCommand("action=opt_all&val=" + ((chk) ? 1 : 0));
        return false;
    }
    
    function switchIgnores() {
        //var cb = theForm.cbIgnoreOptions;
        var rb = document.getElementById("rbIgnor1");
        if ((rb)) { 
            $("#tblIgnores").find("input").prop("disabled", !rb.checked); 
            $("#btnUseAll").css("display", rb.checked ? "" : "none");
            $("#btnIgnoreAll").css("display", rb.checked ? "" : "none");
        } else { 
            $("#tblIgnores").find("input").prop("disabled", false);
            $("#btnUseAll").show();
            $("#btnIgnoreAll").show();
        }
    }

    function switchBaseCase() {
        var bc  = theForm.cbBaseCase;
        var bc1 = theForm.cbBaseCaseIncludes;
        if ((bc)) {
            if ((bc1)) { 
                $("#tblBaseCase").find("input").prop("disabled", !(bc.checked && bc1.checked));
                bc1.disabled = !bc.checked;
            }
        } else {
            if ((bc1)) {
                $("#tblBaseCase").find("input").prop("disabled", !bc1.checked);
                bc1.disabled = false;
            }
        } 
    }

    function onOptionClick(name, checked) {
        sendCommand("action=option&name=" + name + "&val=" + ((checked) ? 1 : 0));
    }

    function onIgnoreOptionsClick(checked) {
        switchIgnores();
        sendCommand("action=use_ignore_options&val=" + ((checked) ? 1 : 0));
    }

    function onBaseCaseOptionsClick(checked) {
        switchBaseCase();
        sendCommand("action=use_base_case_options&val=" + ((checked) ? 1 : 0));
    }

    function onSetCategory(cat_guid) {
        sendCommand("action=select_category&cat_guid="+ cat_guid);
    }

    function onSetFundedFilter(value) {
        FundedFilteringMode = value*1;
        sendCommand("action=funding_filter&value="+ value);
    }

    function onShowExcludedClick(checked) {
        ShowExcludedAlts = checked;
        sendCommand("action=show_excluded&val=" + ((checked) ? 1 : 0));
    }

    function HighlightSelectScenarios() {
        var toolbar = $find("<%= RadToolBarMain.ClientID %>");
        var btn =  toolbar.findItemByText("<%=ResString("lblSelectScenarios") %>");
        if ((btn)) btn.showDropDown();
    }
            
    //window.onload = InitScenarios;
    resize_custom  = function () { resizeChart(); resizeGrid(); };
    document.onclick = function () { if (is_context_menu_open == true) { $("div.context-menu").hide(200); is_context_menu_open = false; } };
    $(document).ready( function () { ResetRadToolbar(); setTimeout('InitScenarios(); initMode();', 200) });

</script>
</telerik:RadScriptBlock>

<div style="display:none; z-index:999; position:absolute; left:0; top:0; background:#fffae0; border:1px solid #ccc; padding:4px;" id="chart_tooltip" class='text'></div>

<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server" ClientEvents-OnResponseEnd="onResponseEnd" EnableAJAX="true">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="divGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="divGrid" />
                <telerik:AjaxUpdatedControl ControlID="divIgnores" />
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<% %>
<table border='0' cellspacing='0' cellpadding='0' class='whole' id='divMain'>
<tr valign='top' class='text'>
<td id='tdToolbar' style="height:24px;" class="text" colspan='2'><telerik:RadToolBar ID="RadToolBarMain" runat="server" CssClass="dxToolbar" Skin="Default" Width="100%" AutoPostBack="false" EnableViewState="false" OnClientButtonClicked="onClickToolbar" OnClientDropDownClosed="onToolbarDropdownClosed">
    <Items>
        <%--<telerik:RadToolBarDropDown Text="Select_Scenarios" EnableViewState="false" DropDownWidth="285" ImageUrl="~/App_Themes/EC09/images/options_x-20.png">--%>
        <telerik:RadToolBarDropDown Text="Select_Scenarios" EnableViewState="false" DropDownWidth="285" ImageUrl="~/App_Themes/EC09/images/scenarios_chk-20.png">       
            <Buttons>
                <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOpt" Enabled="false">
                    <ItemTemplate>
                        <div id='div' class='toolbarcustom' style="margin-left:-2px; padding:0px; line-height:26px;">
                            <div id='lblScenarios' class='text' style='margin-left:4px;'></div>
                            <div style='text-align:left; padding:10px 0px 2px 35px;'><nobr>
                                <input type="button" id='btnAll'  value='<%=ResString("lblAll") %>'  class='button' style='width:75px' onclick="checkAllScenarios(true);" />
                                <input type='button' id='btnNone' value='<%=ResString("lblNone") %>' class="button" style='width:75px' onclick="checkAllScenarios(false);" />
                                <input type='button' id='btnOK'   value='<%=ResString("btnOK") %>'   class="button" style='width:75px' onclick="closeScenariosDropdown();" /></nobr>
                            </div>
                        </div>
                    </ItemTemplate>
                </telerik:RadToolBarButton>
            </Buttons>
        </telerik:RadToolBarDropDown>
        <telerik:RadToolBarButton runat="server" EnableViewState="false">
            <ItemTemplate><span id="lbl_scen_count"></span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false">
            <ItemTemplate>&nbsp;|&nbsp;</ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Display:">
            <ItemTemplate><span id="lbl_display" class='toolbar-label'><%=ResString("lblDisplay") + ":" %>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOptField" Enabled="false">
            <ItemTemplate>
                <select id="cbFields" style="width:140px; margin-top:3px" onchange="onSetField(this.value);">
                    <option <%=IIf(DisplayField = DisplayFields.dfFunded, "selected='selected'","")%> value='<%=Cint(DisplayFields.dfFunded)%>'><%=ResString("optFunded")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfBenefit,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfBenefit)%>'><%=ResString("lblBenefit")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfEBenefit,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfEBenefit)%>'><%=ResString("lblExpectedBenefit")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfFundingByCategory,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfFundingByCategory)%>'><%=ResString("optFundingByCategory")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfObjPriorities,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfObjPriorities)%>'><%=ResString("optObjPriorities")%></option>
                    <option value='-1' disabled='disabled'><%=ResString("optSeparatorLine")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfPFailure,   "selected='selected'","")%> value='<%=Cint(DisplayFields.dfPFailure)%>'><%=ResString("optPFailure")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfPSuccess,   "selected='selected'","")%> value='<%=Cint(DisplayFields.dfPSuccess)%>'><%=ResString("optPSuccess")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfRisk, "selected='selected'","")%> value='<%=Cint(DisplayFields.dfRisk)%>'><%=ResString("optRisk")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfCost,   "selected='selected'","")%> value='<%=Cint(DisplayFields.dfCost)%>'><%=ResString("optCost")%></option>
                    <%--<option <%=IIf(DisplayField = DisplayFields.dfMust,   "selected='selected'","")%> value='<%=Cint(DisplayFields.dfMust)%>'><%=ResString("optMust")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfMustNot,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfMustNot)%>'><%=ResString("optMustNot")%></option>--%>
                    <option <%=IIf(DisplayField = DisplayFields.dfMustsAndMustNots,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfMustsAndMustNots)%>'><%=ResString("optMustsAndMustNots")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfDependencies,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfDependencies)%>'><%=ResString("optDependencies")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfGroups,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfGroups)%>'><%=ResString("optGroups")%></option>
                    <option <%=IIf(DisplayField = DisplayFields.dfCustomConstraints,"selected='selected'","")%> value='<%=Cint(DisplayFields.dfCustomConstraints)%>'><%=ResString("optCustomConstraints")%></option>
                    <%--<option value='-2' disabled='disabled'><%=ResString("optSeparatorLine")%></option>--%>
                </select>
            </ItemTemplate>
        </telerik:RadToolBarButton>
        <%--<telerik:RadToolBarButton runat="server" EnableViewState="false" Enabled="False" Text="|" />--%>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_ctgs">
            <ItemTemplate><span id="lbl_ctgs" class='toolbar-label'>&nbsp;<%=ResString("lblRAAttributes") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="btnViewCategories" Enabled="false">
            <ItemTemplate>
                <%=GetCategories()%>
            </ItemTemplate>
        </telerik:RadToolBarButton>        
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_mode">
            <ItemTemplate><span id="lbl_mode" class='toolbar-label'>&nbsp;<%=ResString("lblMode") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="btnViewOptions" Enabled="false">
            <ItemTemplate>
                <select id="cbViewMode" style="width:140px; margin-top:3px" onchange="onSetViewMode(this.value);">
                    <option <%=IIf(ViewMode = ViewModes.vmGridView,"selected='selected'","")%>    value='<%=CInt(ViewModes.vmGridView)%>'><%=ResString("lblGridView") %></option>
                    <option <%=IIf(ViewMode = ViewModes.vmColumnChart,"selected='selected'","")%> value='<%=CInt(ViewModes.vmColumnChart)%>'><%=ResString("optRAColumnChart")%></option>
                    <option <%=IIf(ViewMode = ViewModes.vmPieChart,"selected='selected'","")%>    value='<%=CInt(ViewModes.vmPieChart)%>'><%=ResString("optRAPieChart")%></option>
                    <option <%=IIf(ViewMode = ViewModes.vmDonutChart,"selected='selected'","")%>  value='<%=CInt(ViewModes.vmDonutChart)%>'><%=ResString("optRADonutChart")%></option>
                </select>
            </ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_view">
            <ItemTemplate><span id="lbl_view" class='toolbar-label'>&nbsp;<%=ResString("btnView") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="btnViewMode" Enabled="false">
            <ItemTemplate>
                <%=GetViewOptions()%>
            </ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="True" Value="btnShowExcluded" Enabled="false">
            <ItemTemplate>
                <label id='lblShowExcluded' for='cbShowExcluded'><input type='checkbox' class='checkbox' id='cbShowExcluded' style='margin-top:2px' <%=CStr(IIf(ShowExcludedAlts, " checked='checked' ", ""))%> onclick='onShowExcludedClick(this.checked)'><%=ResString("lblRAShowExcluded")%></label>
            </ItemTemplate>
        </telerik:RadToolBarButton>        
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_group_by">
            <ItemTemplate><span id="lbl_group_by" class='toolbar-label'>&nbsp;<%=ResString("lblGroupBy") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="btnGroupBy" Enabled="false">
            <ItemTemplate>
                <select id="cbGroupBy" style="width:140px; margin-top:3px" onchange="onSetGroupBy(this.value);">
                    <option <%=IIf(GroupingMode = GroupingModes.gmScenario   , "selected='selected'", "")%> value='<%=Cint(GroupingModes.gmScenario)    %>'><%=ResString("lblScenario")%></option>
                    <option <%=IIf(GroupingMode = GroupingModes.gmAlternative, "selected='selected'", "")%> value='<%=Cint(GroupingModes.gmAlternative) %>' id=''><%=IIf(DisplayField = DisplayFields.dfObjPriorities, ResString("tblObjective"), ResString("tblAlternative"))%></option>
                </select>
            </ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false">
            <ItemTemplate>&nbsp;|&nbsp;</ItemTemplate>
        </telerik:RadToolBarButton>
        <%--<telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Export" Width="100px" ImageUrl="~/App_Themes/EC09/images/file.png" Value="export" Font-Bold="true" Font-Size="11pt" BorderColor="#ffaa44" BackColor="#f0f0f0" ForeColor="#cc6600" BorderWidth="1" />--%>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Export" ImageUrl="~/App_Themes/EC09/images/file.png" Value="export" />
        <telerik:RadToolBarButton Text="Customize" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/palette-16.png" Value="customize" Visible="false" />       
    </Items>
</telerik:RadToolBar>
</td></tr>
<tr class='text' style='height:30;'><td valign='top' id='tdTitle' class='text' colspan='2'>
    <h5 style='margin-top:6px;padding-bottom:0px;'><%=ResString("titleRAScenarios") %>&nbsp;<label id='lblFieldTitle' style='cursor:default;'><% = GetSelectedFieldName(True)%></label>&nbsp;<%=ResString("lblForProject") %>&nbsp;"<% = ShortString(SafeFormString(JS_SafeString(App.ActiveProject.ProjectName)), 45)%>"</h5>
</td></tr>
<tr valign='top'><!-- Ignore --><td align='center' valign='top' id='tdIgnore' style='padding:2px; text-align:center; overflow:hidden; padding-top:3px;' colspan='2'>
<telerik:RadCodeBlock ID="RadBlockIgnores" runat="server">
    <center><div id='divIgnores' runat='server' style='width:950px;overflow:hidden;'>
    <div style='width:720px;float:left;display:inline-block;overflow:hidden;'>
    <fieldset class="legend" style="display:inline-block; width:auto; padding-bottom:3px; padding-top:0px;" runat="server" id="pnlIgnore">
    <%--<legend class="text legend_title">&nbsp;<%=ResString("lblIgnorePnlTemp")%>:&nbsp;</legend>--%>
    <%--<legend class="text legend_title">&nbsp;<%If IsSingleScenarioChecked() Then%><%=ResString("lblIgnorePnl")%>:<%Else%><label id='lblIgnoreOptions'><input type='checkbox' name='cbIgnoreOptions' value='1' <% =IIF(RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, "checked='checked'", "")%> onclick='onIgnoreOptionsClick(this.checked);'/><%=ResString("lblIgnorePnlTemp")%>:</label><%End If %>&nbsp;</legend>--%>
    <legend class="text legend_title">&nbsp;<%If IsSingleScenarioChecked() Then%><%=ResString("lblIgnorePnl")%>:<%Else%><label id='lblIgnoreOptions2'><input type='radio' id="rbIgnor0" name='rbIgnoreOptions' value='0' <%=If(Not RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, "checked='checked'", "")%> onclick='onIgnoreOptionsClick(false);'/><%=ResString("lblIgnorePnlTemp2")%>&nbsp;&nbsp;</label><label id='lblIgnoreOptions'><input type='radio' id="rbIgnor1" name='rbIgnoreOptions' value='1' <%=If(RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseIgnoreOptions, "checked='checked'", "")%> onclick='onIgnoreOptionsClick(true);'/><%=ResString("lblIgnorePnlTemp")%></label><%End If %>&nbsp;</legend>
    <table id='tblIgnores' border='0' cellspacing='0' cellpadding='0'>
    <tr valign='top'><td class='text' align='left' style='text-align:left;'>
    <label id='lblcbOptMusts'        class="no_wrap" <% =GetOptionStyle("cbOptMusts")%>       ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptMusts")%>        name='cbOptMusts' /> <%=ResString("optIgnoreMusts")%></label>
    <label id='lblcbOptMustNots'     class="no_wrap" <% =GetOptionStyle("cbOptMustNots")%>    ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptMustNots")%>     name='cbOptMustNots' /> <%=ResString("optIgnoreMustNot")%></label>
    <label id='lblcbOptConstraints'  class="no_wrap" <% =GetOptionStyle("cbOptConstraints")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptConstraints")%>  name='cbOptConstraints' /> <%=ResString("optIgnoreCC")%></label>
    <label id='lblcbOptDependencies' class="no_wrap" <% =GetOptionStyle("cbOptDependencies")%>><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptDependencies")%> name='cbOptDependencies' /> <%=ResString("optIgnoreDependencies")%></label><% If OPT_SHOW_IGNORE_LINKS Then%>&nbsp;<a href='<% =PageURL(_PGID_RA_DEPENDENCIES, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a><%End If%>
    <label id='lblcbOptGroups'       class="no_wrap" <% =GetOptionStyle("cbOptGroups")%>      ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptGroups")%>       name='cbOptGroups' /> <%=ResString("optIgnoreGroups")%></label><% If OPT_SHOW_IGNORE_LINKS Then%> &nbsp;<a href='<% =PageURL(_PGID_RA_GROUPS, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a><%End If%>
    <label id='lblcbOptFundingPools' class="no_wrap" <% =GetOptionStyle("cbOptFundingPools")%>><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptFundingPools")%> name='cbOptFundingPools' /> <%=ResString("optIgnoreFP")%></label><% If OPT_SHOW_IGNORE_LINKS Then%> &nbsp;<a href='<% =PageURL(_PGID_RA_FUNDINGPOOLS, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a><%End If%>
    <label id='lblcbOptRisks'        class="no_wrap" <% =GetOptionStyle("cbOptRisks")%>       ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptRisks")%>        name='cbOptRisks' /> <%=ResString("optIgnoreRisks")%></label>
    <% If ShowDraftPages() OrElse Not isDraftPage(_PGID_RA_TIMEPERIODS_SETTINGS) Then%><label id='lblcbOptTimePeriods' class="no_wrap" <% =GetOptionStyle("cbOptTimePeriods")%> ><input type='checkbox' class='checkbox' onclick='return onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbOptTimePeriods")%>  name='cbOptTimePeriods' /> <%=ResString("optTimePeriods")%></label><% end if %>
    </td></tr><tr valign='top'><td class='text' align='left'>
        <div style='text-align:left; font-weight: bold;'>
            <input type="button" class='button button_small' id='btnUseAll' value='Use all' style='width:70px' onclick="return SetOptAll(<% =iif(OPT_SHOW_AS_IGNORES, 0, 1) %>);" />
            <input type='button' id='btnIgnoreAll' value='Ignore all' class="button button_small" style='width:70px' onclick="return SetOptAll(<% =iif(OPT_SHOW_AS_IGNORES, 1, 0) %>);" />
        </div>
    </td></tr></table>
    </fieldset></div><%--</td>
    <td align='left' valign='top' id='tdBaseCase' style='padding:2px;'>--%>
    <div style='width:230px;display:inline-block;overflow:hidden;'>
    <fieldset class="legend" style="display:inline-block; width:auto; padding-bottom:3px; padding-top:0px;text-align:left;" runat="server" id="pnlBaseCase">
    <%If Not IsSingleScenarioChecked() Then%><legend class="text legend_title">&nbsp;<label><input type='checkbox' name='cbBaseCase' value='1' <% =IIF(RA.Scenarios.GlobalSettings.ScenarioComparisonSettings.UseBaseCaseOptions, "checked='checked'", "")%> onclick='onBaseCaseOptionsClick(this.checked);'/><%=ResString("lblBaseCasePnlTemp")%>:</label>&nbsp;</legend>
    <label class='text' style='margin-left:12px;text-align:left;'><input type='checkbox' class='checkbox' name='cbBaseCaseIncludes' value='1' <% =GetOptionValue("cbBaseCase")%> onclick='switchBaseCase(); onOptionClick("cbBaseCase", this.checked);'/><%=ResString("lblBaseCasePnl")%>:</label>&nbsp;
    <%Else%>
    <legend class="text legend_title">&nbsp;<label style='margin-left:12px;text-align:left;'><input type='checkbox' class='checkbox' name='cbBaseCaseIncludes' value='1' <% =GetOptionValue("cbBaseCase")%> onclick='switchBaseCase(); onOptionClick("cbBaseCase", this.checked);'/><%=ResString("lblBaseCasePnl")%>:</label>&nbsp;</legend>
    <%End If%>
    <table id='tblBaseCase' border='0' cellspacing='0' cellpadding='0' style='padding-left:24px;'>    
    <tr valign="top"><td class='text' align='left'><div id='optBaseCase'>
    <label id='lblBSGroups' <% =GetOptionStyle("cbBCGroups")%>><input type='checkbox' class='checkbox' onclick='onOptionClick(this.name, this.checked);' <% =GetOptionValue("cbBCGroups")%> name='cbBCGroups'/> <%=ResString("optBCGroups")%></label> &nbsp;<%--<a href='<% =PageURL(_PGID_RA_GROUPS, GetTempThemeURI(False)) %>' class='actions'>&raquo;&raquo;</a>--%><br />
    </div></td></tr></table>
    </fieldset></div></div></center></telerik:RadCodeBlock>
</td></tr>
<tr>
    <td id="tdColorLegend" style="width:170px;" valign="top">
        <div id="divColorLegend" style="width:160px; margin-top:12px; margin-right:10px; overflow:auto;border:1px solid #ccc;"><table id="TableColorLegend" class='text grid' style='table-layout:fixed; border-collapse:collapse;' cellspacing='0' cellpadding="1"></table></div>
    </td>
    <td align='center' valign='top' id='tdGridAlts' style='width:100%; height:100%;'>
    <telerik:RadCodeBlock runat='server' ID='RadCodeBlockMain'>
    <div style='overflow:auto; vertical-align:top; text-align:center; margin-top:5px;' id='divGrid' runat='server'>
     <div id='DivPlotData' runat='server' style='display:none;'></div>
     <asp:GridView EnableViewState="false" AutoGenerateColumns="false" AllowSorting="true" AllowPaging="false" ID="GridAlternatives" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" TabIndex="1" HorizontalAlign="Center" CssClass="grid" ShowFooter="false">
        <RowStyle VerticalAlign="Middle" CssClass="text grid_row" Height="2.5em"/>
        <AlternatingRowStyle CssClass="text grid_row_alt" />
        <HeaderStyle CssClass="text grid_header actions"/>        
        <EmptyDataRowStyle CssClass="empty_gridview" />
        <EmptyDataTemplate><h6 style='margin:8em 2em'><nobr><% =GetEmptyMessage()%></nobr></h6></EmptyDataTemplate>            
     </asp:GridView>    
    </div>
    <div runat='server' id='divChart' style='vertical-align:top;'>
        <div id="scenarios_chart" class='whole' style="overflow:hidden;width:1100px;height:300px;"><center><p style='vertical-align:middle; margin-top:8ex;'><%=ResString("lblPleaseWait") %></p></center></div>
    </div>    
    </telerik:RadCodeBlock></td>
    </tr>    
</table>

<div id='editorForm' style='display:none;'>
<table border='0' cellspacing='1' cellpadding='2' class='text'>
<tr><td valign='top'><span style='width:100px;margin-top:4px;'><%=ResString("tblScenarioName") %>:&nbsp;&nbsp;</span></td><td><input type='text' style='width:280px;' id='tbScenarioName' value='' onfocus='getScenarioName(this.value);' onkeyup='getScenarioName(this.value);' onblur='getScenarioName(this.value);' /></td></tr>
<tr><td valign='top'><span style='width:100px;margin-top:4px;'><%=ResString("tblDescription") %>:&nbsp;&nbsp;</span></td><td><textarea rows='8' cols='40' style='width:280px;margin-top:2px;' id='tbScenarioDescr' onfocus='getScenarioDescr(this.value);' onkeyup='getScenarioDescr(this.value);' onblur='getScenarioDescr(this.value);'></textarea></td></tr>
</table>
</div>

<div id='customizeForm' style='display:none;'>
<table border='0' cellspacing='1' cellpadding='2' class='text' style="height:350px;">
<tr><td valign='top' colspan="9" align="center"><span style='width:100px; margin-top:4px; text-align:center;'>Select A Color Palette</span></td></tr>
<tr id="trPalettes"></tr>
</table><br />
</div>

</asp:Content>