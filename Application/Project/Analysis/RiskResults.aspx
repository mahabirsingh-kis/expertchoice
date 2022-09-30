<%@ Page Language="VB" Inherits="RiskResultsPage" title="Risk Results" Codebehind="RiskResults.aspx.vb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<%If IsRiskMapPage Then%>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.jqplot.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.bubbleRenderer.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.BezierCurveRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.cursor.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisTickRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasAxisLabelRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.categoryAxisRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasTextRenderer.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.canvasOverlay.min.js"></script>
<%--<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>js/eventCloud.js"></script>--%>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/jqplot.highlighter.min.js"></script>
<%End If%>
<%If Not IsRiskMapPage AndAlso Not IsBowTiePage Then%>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<%End If%>
<%If IsBowTiePage Then%>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/bowTie.js"></script>
<%End If%>
<%If IsRiskMapPage Then%>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/html2canvas.min.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/download.js"></script>
<%End If%>
<script language="javascript" type="text/javascript">
    <%--var showHiddenSettings = <%=Bool2JS(ShowHiddenSettings)%>;--%>
    var savedFilters = []; <%--eval("[<%=PM.Parameters.AlternativeFilters%>]");--%>
    <%--var IsMixedEventsAllowed = <%=Bool2JS(IsMixedEventsAllowed)%>;--%>

    var attr_list = []; <%--<%=LoadAttribData()%>;--%>
    var attr_flt  = []; <%--<%=LoadFilterData()%>;--%>

    var aidx_id = 0;
    var aidx_name = 1;
    var aidx_type = 2;
    var aidx_vals = 3;
    var aidx_col_vis = 4;

    var fidx_chk = 0;
    var fidx_id = 1;
    var fidx_oper = 2;
    var fidx_val = 3;

    var avtString = 0;
    var avtBoolean = 1;
    var avtLong = 2;
    var avtDouble = 3;
    var avtEnumeration = 4;
    var avtEnumerationMulti = 5;

    var flt_max_id = -1;
    window.plot1 = {};
    window.plot2 = {};
    var plot_data1 = [], plot_data2 = [];
    var cur_cmd = "";
    
    var dlg_users     = null;
    var dlg_hierarchy = null;
    var dlg_expanded  = null;
    var dlg_select_columns = null;
    var dlg_attributes = null;

    var dlg_width = 790;

    var table_users    = null; 
    var table_groups   = null;

    var all_obj_data = <% = GetAllObjsData()%>; // impact hierarchy for Dollar values
    var events_data = <% = GetEventsData()%>;      // events
    var event_list = events_data;
    var controls_data = <% = GetControlsData() %>;  
    var users_data= <% = GetUsersData()%>;         // users
    var groups_data= <% = GetGroupsData()%>;       // groups

    var ctUndefined = "<%=CInt(ControlType.ctUndefined)%>";
    var ctCause = <%=CInt(ControlType.ctCause)%>;
    var ctConsequenceOld = <%=CInt(ControlType.ctConsequence)%>; // OBSOLETE
    var ctCauseToEvent = <%=CInt(ControlType.ctCauseToEvent)%>;
    var ctConsequenceToEvent = <%=CInt(ControlType.ctConsequenceToEvent)%>;

    // visible columns
    var COL_CTRL_ID = 0;
    var COL_CTRL_IS_ACTIVE = 1;
    var COL_CTRL_NAME = 2;
    var COL_CTRL_TYPE = 3;
    var COL_CTRL_FUNDED = 4; // "Selected" readonly column
    var COL_CTRL_COST = 5;
    var COL_CTRL_APPL_COUNT = 6;
    var COL_CTRL_VIEW_CATEGORIES = 7;
    var COL_CTRL_RISK_REDUCTION = 8;
    var COL_CTRL_MUST = 9;
    var COL_CTRL_MUST_NOT = 10;

    // invisible columns
    var COL_CTRL_GUID = 11;
    var COL_CTRL_INFODOC = 12;
    var COL_CTRL_CATEGORY_IDS = 13;    
    var COL_CTRL_CHECKED = 14;  

    var sEventIDs    = "<% = StringSelectedEventIDs%>";
    var sControlIDs    = "<% = StringSelectedControlIDs%>";
    
    var IDX_EVENT_GUID = 0;
    var IDX_EVENT_NAME = 1;
    var IDX_EVENT_CHECKED = 2;
    var IDX_EVENT_DOLLAR_VALUE = 3;
    var dlg_select_events = null;
    //var dlg_select_controls = null;
    
    var mh = 500;   // maxHeight for dialogs;

    var IDX_USER_CHECKED = 0;
    var IDX_USER_ID      = 1;
    var IDX_USER_NAME    = 2;
    var IDX_USER_EMAIL   = 3;
    var IDX_USER_HAS_DATA= 4;
    var IDX_GROUP_PARTICIPANTS = 5;

    var ACTION_SELECTED_USER = "<%=ACTION_SELECTED_USER%>";
    var ACTION_SELECTED_USERS = "<%=ACTION_SELECTED_USERS%>";
    var ACTION_GET_TREATMENTS = "<%=ACTION_GET_TREATMENTS%>";
    var ACTION_SET_BUBBLE_SIZE = "<%=ACTION_SET_BUBBLE_SIZE%>";
    var ACTION_SHOW_DOLLAR_VALUE = "<%=ACTION_SHOW_DOLLAR_VALUE%>";

    var ShowEventNumbers = <%=Bool2JS(PM.Parameters.NodeIndexIsVisible)%>;
    var ShowDollarValue = <%=Bool2JS(ShowDollarValue)%>; // Show Value of Enterprise
    var DollarValue = <%=JS_SafeNumber(DollarValue)%>; // Value of Enterprise
    var DollarValueTarget = "<%=DollarValueTarget%>"; // Target Of Value of Enterprise
    var DollarValueOfEnterprise = <%=JS_SafeNumber(DollarValueOfEnterprise)%>;
    var DollarValueDefined = <%=Bool2JS(Not (DollarValue = UNDEFINED_INTEGER_VALUE OrElse DollarValueTarget = UNDEFINED_STRING_VALUE))%>;
    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;
    var CHAR_CURRENCY_SYMBOL = "<%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%>";
    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    var numSimulations = <% = PM.RiskSimulations.NumberOfTrials %>;
    var randSeed       = <% = PM.RiskSimulations.RandomSeed %>;
    var keepSeed       = <% = Bool2JS(PM.RiskSimulations.KeepRandomSeed) %>;
    var UseControlsReductions = <%=Bool2JS(UseControlsReductions)%>;
    var ControlsSelectionMode = <%=PM.Parameters.Riskion_ControlsActualSelectionMode%>;
    var ControlsSimulationMode = <%=PM.Parameters.Riskion_Use_Simulated_Values%>;
    var UseSimulatedValues = <% = Bool2JS(PM.CalculationsManager.UseSimulatedValues) %>;
    var likelihoodCalculationMode = <%=CInt(PM.CalculationsManager.LikelihoodsCalculationMode)%>;
    var dilutedMode = <% = CInt(PM.CalculationsManager.ConsequenceSimulationsMode)%>;
    var decimals = <% = PM.Parameters.DecimalDigits %>;
    var showCents = <% = Bool2JS(ShowCents) %>;

    var OPT_MESSAGE_HIGHLIGHT_TIMOUT = 10000;
    
    var ShowAttributesPane = <%=Bool2JS(ShowAttributes AndAlso ActiveProjectHasAlternativeAttributes)%>;

    var oper_list = ["Contains", "Equal", "Not Equal", "Starts With", "Greater Than", "Greater Than Or Equal", "Less Than", "Less Than Or Equal", "Is True", "Is False"];
    
    var oper_available = [[0,1,2,3],
                          [8,9],
                          [1,2,4,5,6,7],
                          [1,2,4,5,6,7],
                          [1,2],
                          [0,1,2]];

    var is_request = false;

    var img_path = '<% =ImagePath %>';
    var img_qh = new Image; img_qh.src = img_path + 'help/icon-question.png';
    var img_qh_ = new Image; img_qh_.src = img_path + 'help/icon-question_dis.png';
   
    function SelectAllUsers(do_checked) {
        if (is_request) return false;
        var re = new RegExp("(.*)cbListUsers(.*)");
        for (var i = 0; i < theForm.elements.length; i++)
        {
            var obj = theForm.elements[i];
            if (obj && (obj.id))
            {
                if (re.test(obj.id)) 
                {
                    if (do_checked==-1) obj.checked =!obj.checked; else obj.checked = do_checked;
                }    
            }
        }
        sendCommand('-1');
        return false;
    }
   
    function sendCommand(cmd) {        
        cur_cmd = cmd;
        var frm = $find("<% =RadAjaxManagerMain.ClientID %>");
        if (frm) { showLoadingPanel(); frm.ajaxRequest(cmd); is_request = true; };
    }

    function OnSelectedNodeClick(sender, eventArgs) {
        var node = eventArgs.get_node();
        if (!is_request && node.get_enabled())
        {
            if ((node)) {
                SelectedHierarchyNodeName = node.get_text();
                SelectedHierarchyNodeID = node.get_value() + "";
                SetNodeName(SelectedHierarchyNodeName);            
            }
            sendCommand(node);
        }
    }

    function OnRadAjaxCallback(sender, eventArgs) {
        if (eventArgs.EventArgument.indexOf("select_events") !== -1) {
            reloadPage();
            return false;
        }
        resizePage();
        //reload_project_sl();
        if (nav_to_pgid != "") {
            //navOpenPage(nav_to_pgid, '');
            setTimeout("navOpenPage('" + nav_to_pgid + "', true); nav_to_pgid = '';", 500);
        } else {
            is_request = false;
            InitBowTieAndRiskPlot();
            InitTooltips();
            initGridSize();      
            //updateAttributesUI();
        }
        var tree = $find("<% =RadTreeHierarchy.ClientID %>");
        if ((tree))
        {
            var n = tree.get_selectedNode();
            if ((n)) SetNodeName(n.get_text()); else SetNodeName("");
        }
        initToolbar();
        updateFilterWarning();
        updateColorCodingWarning();
        hideLoadingPanel();
    }

    function updateFilterWarning() {
        var sName = getFilterName();
        var sFullFilterName = "Filtering by Attributes is ON. Filter string: " + (sName == "" ? "\"undefined\"" : sName);
        $("#divFilterWarning").html(sFullFilterName);
        if (ShowAttributesPane && sName != "") { $("#divFilterWarning").show();} else { $("#divFilterWarning").hide(); sFullFilterName = ""; }
        document.cookie = "events_filter_name=" + escape(sFullFilterName);
    }

    function updateColorCodingWarning() {
        if (ColorCodingByCategory) { $("#<%=DivGridColorLegend.ClientID%>").show(); } else { $("#<%=DivGridColorLegend.ClientID%>").hide(); }
    }

    function OnPanelTreeLoaded(sender, eventArgs) {
        is_request = false;
        hideLoadingPanel();
        sendCommand("refresh");
    }

    function SetNodeName(name) {
        if (name.indexOf("[G:") !== -1) name = name.substring(0, name.indexOf("[G:"));
        if (name.indexOf("[L:") !== -1) name = name.substring(0, name.indexOf("[L:"));
        var d = document.getElementById("divNodeName");        
        if ((d)) d.innerHTML = htmlStrip(name);
        if ((name=="")) {
            d = document.getElementById("<% = divHeader.ClientID %>");
            if ((d)) d.style.display = "none";
        }
        $(".divWRTHeader").html(htmlStrip(name));
    }

    function OnClientResized(sender, eventArgs)
    {
        var dif = eventArgs.get_oldWidth() - sender.get_width();
        var tree = $find("<% =RadTreeHierarchy.ClientID %>");
        if ((dif) && (tree))
        {
            var nodes = tree.get_allNodes();
            for (var i = 0; i < nodes.length; i++) 
            {
                var node = nodes[i];
                var tbl = document.getElementById("tbl" + node.get_value());
                var w =  sender.get_width() - 45 - node.get_level() * 20;
                if ((tbl))
                {
                    tbl.style.width = w + "px";
                }
            }
        }
        OnPaneResized(sender, eventArgs);
    }
    
    function OnPaneResized(sender, eventArgs) {
        setTimeout("initGridSize();", 300);
    }

    var has_init = 0;
    function resizePage() {
        var s = $find("<% =RadSplitterMain.ClientID %>");
        var d = $("#divRiskResultsMain");
        if ((s) && (d)) {
            var pane = s.getPaneById("<% =RadPaneHierarchy.ClientID %>");            
            <% If IsWidget Or CurrentPageID=_PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_OVERALL OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS Then %>
            if (pane) pane.collapse();
            var sr = document.getElementById("<% = RadSplitRight.ClientID %>");
            if ((sr)) sr.style.display = "none";
            <% End if %>            
            s.set_width(400);
            s.set_height(200);
            s.set_width(d.width() - 2);
            s.set_height(d.height() - 2);            
            //updateAttributesUI();
            var tree = $find("<% =RadTreeHierarchy.ClientID %>");
            if ((tree))
            {
                var n = tree.get_selectedNode();
                if ((n)) {                    
                    SetNodeName(n.get_text()); 
                } else { SetNodeName(""); }
            }
            <% If IsRiskMapPage() Then%>
            setTimeout('resizeChart(); if (typeof window["plot1"].replot != "undefined") { window["plot1"].replot({ resetAxes: false }); plotExtraInit(window["plot1"]); } if (typeof window["plot2"].replot != "undefined") { window["plot2"].replot({ resetAxes: false }); plotExtraInit(window["plot2"]); }', 500);
            <% End If %>            

            setTimeout("initGridSize();", 100);
            if (has_init == 0) setTimeout("hideLoadingPanel(); $('#divRiskResultsMain').animate({'opacity':  1});", 200);
            has_init = 1;
        } else {
            if (has_init == 0) setTimeout("resizePage();", 500);
            has_init = 1;
        }
    }

    function initGridSize() {
        // init content pane size
        var s = $find("<% =RadSplitterMain.ClientID %>");
        if (s) {
            var pane = s.getPaneById("<%=RadPaneGrid.ClientID %>");            
            if (pane) {
                var hw = 0;
                var aw = 0;
                var uw = 0;
                var hp = document.getElementById("<% =RadPaneHierarchy.ClientID %>");            
                if (hp) hw = hp.clientWidth + 10;
                pane.set_width($("#divRiskResultsMain").width() - hw - aw - uw); 
            }
        }

        // init grid size
        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        var divContent = $("#GridViewContainer");   
        if ((divContent)) {
            divContent.height(100);
            var t = $("#tblToolbar");
            var l = $('#tdLegend');            
            var x = document.getElementById("divTitles");
            var xx = document.getElementById("tdInfobar");
            var m = document.getElementById('<%=RadPaneGrid.ClientID%>');
            if ((t) && (l) && (m)) {
                var extra_height = ((x) ? x.clientHeight : 0) + ((xx) ? xx.clientHeight : 0);
                var h = m.clientHeight - ((l.length > 0) ? l[0].clientHeight : 0) - ((t.length > 0) ? t[0].clientHeight : 0) - extra_height;
                if (h < 150) h = 150;
                divContent.height(h);
                divContent.width(100);
                //d.width(100);
                var w = $("#tdContent").width();
                divContent.width(w);
                //d.width(w);                
            }
        }
        
        removeExtraBorders();
        <% End If %>

        // init bow-tie
        <% If IsBowTiePage() Then%>
        var divBowTie = $("#DivCanvas");   
        if ((divBowTie)) {
            var t = $("#tblToolbar");
            var l = $('#gTitlesLegend');            
            var x = document.getElementById("divTitles");
            var z = document.getElementById("tableBowTieHeaders");
            var m = document.getElementById('divRiskResultsMain');
            if ((t) && (l) && (m)) {
                var extra_height = ((x) ? x.clientHeight + 2 : 2);
                extra_height += ((z) ? z.clientHeight + 10: 0);
                var h = m.clientHeight - l.height() - t.height() - extra_height - $("#<% = divHeader.ClientID %>").height() - 25;
                if (h < 150) h = 150;
                divBowTie.height(h);
                //divBowTie.width(100);
                //var w = $("#tdContent").width();
                //divBowTie.width(w);
            }
        }
        <% End If %>
        
    }

    function getAttrById(id) {        
        for (var j = 0; j < attr_list.length; j++) {
            if (attr_list[j][aidx_id] == id) return attr_list[j];            
        }
        return null;
    }

    function AddFilerRow(d, i)
    {
        var tbl = document.getElementById("tblFilters");
        if ((tbl))
        {
            if (i>flt_max_id) flt_max_id = i;

            var td = document.getElementById("flt_tr_" + i);
            if (!(td))
            {
                var r = tbl.rows.length;
                td = tbl.insertRow(r);
                td.id = "flt_tr_" + i;
                td.style.verticalAlign = "top";
                td.style.textAlign  = "left";
                td.className = "text";
                td.insertCell(0);
                td.insertCell(1);
                td.insertCell(2);
            }

            var attr = null;
                
            var sChk = "<input type='checkbox' id='flt_chk_" + i + "' value='1' style='vertical-align: middle; display: inline-block; margin-top: 6px;' onclick='IsAnyFilterChecked(); '" + (d[fidx_chk]=="1" ? " checked='checked' " : "") + ">";
            var sAttribs = "";            
            for (var j=0; j<attr_list.length; j++)
            {
                var act = (d[fidx_id] == attr_list[j][aidx_id]);
                sAttribs += "<option value='" + attr_list[j][aidx_id] + "'" + (act ? " selected" : "") + ">" + replaceString("'", "&#39;", attr_list[j][aidx_name]) + "</option>";
                if (act) attr = attr_list[j];
            }
            sAttribs = "<select id='flt_attr_" + i + "' style='width: 180px; vertical-align: top;' onChange='ChangeFilterAttr(" + i + ", this.value);'>" + sAttribs + "</select>";

            sOpers = "";
            var sVal = "";

            if ((attr))
            {
                var o = oper_available[attr[aidx_type]];
                if (o) {
                    for (j=0; j<o.length; j++)
                    {
                        sOpers += "<option value='" + o[j] + "'" + (d[fidx_oper] == o[j] ? " selected" : "") + ">" + oper_list[o[j]] + "</option>";
                    }
                }
                sOpers = "<select id='flt_oper_" + i + "' style='width:10em; vertical-align: top;'>" + sOpers + "</select>";

                if ((attr[aidx_type] == avtEnumeration) || (attr[aidx_type] == avtEnumerationMulti)) //approach 1 - select multiple
                {
                    var v = attr[aidx_vals];
                    var vals = [d[fidx_val]];
                    if (attr[aidx_type] == avtEnumerationMulti) vals = d[fidx_val].split(";");                    
                    
                    for (j=0; j<v.length; j++)
                    {
                        var is_selected = false;
                        for (k=0; k<vals.length; k++)
                        {
                            if (vals[k] == v[j][0]) is_selected = true;
                        }
                        sVal += "<option value='" + v[j][0] + "'" + (is_selected ? " selected" : "") + ">" + replaceString("'", "&#39;", v[j][1]) + "</option>";
                    }
                    var multi = "";
                    if (attr[aidx_type] == avtEnumerationMulti) multi = "multiple='multiple'";
                    sVal = "<select " + multi + " id='flt_val_" + i + "' style='width: 180px; vertical-align: top;'>" + sVal + "</select>";

                    //approach 2 - check list box
                    if (attr[aidx_type] == avtEnumerationMulti)
                    {
                        sVal="";
                        for (j=0; j<v.length; j++)
                        {
                            var is_selected = false;
                            for (k=0; k<vals.length; k++)
                            {
                                if (vals[k] == v[j][0]) is_selected = true;                            
                            }
                            sVal += "<li style='margin:0; padding:0;' title='" + replaceString("'", "&#39;", v[j][1]) +"'><label for='chk_" + i + "_" + j + "_" + k + "'><input type='checkbox' id='chk_" + i + "_" + j + "_" + k + "' value='" + v[j][0] + "'" + (is_selected ? " checked" : "") + ">" + replaceString("'", "&#39;", v[j][1]) + "</label></li>";
                        }
                        sVal = "<ul id='flt_val_" + i + "' style='height:8ex; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:4px; width: 180px; vertical-align: top;'>" + sVal + "</ul>";
                    }
                }
                else
                {
                    if (attr[aidx_type] != avtBoolean) {
                        sVal = "<input type='text' size='10' style='width: 180px; vertical-align: top;' id='flt_val_" + i + "' value='" + replaceString("'", "&#39;", d[fidx_val]) + "'>";
                    }
                }
            }

            td.cells[0].innerHTML = "<nobr>" + sChk + sAttribs + "&nbsp;" + sOpers + "</nobr>";
            td.cells[1].innerHTML = sVal;
            td.cells[2].innerHTML = "<button type='button' class='button' style='width: 24px; height: 24px; vertical-align: top; margin: 0px;' id='flt_del_" + i + "' onclick='onDeleteRule(" + i + "); IsAnyFilterChecked(); return false;'><i class='fas fa-times ec-icon'></i></button>";
        }
    }

    function ChangeFilterAttr(id, val) {
        var c = document.getElementById('flt_chk_' + id);
        var o = document.getElementById('flt_oper_' + id);
        var d = [((c) && (c.checked) ? 1 : 0), val, o.value, ''];
        AddFilerRow(d, id);
    }

    function onAddNewRule() {
        var d = [1, attr_list[0][aidx_id], 1, ''];
        AddFilerRow(d, flt_max_id+1);
        CheckDeleteRuleBtn();
        var a = document.getElementById("flt_attr_" + flt_max_id);
        if ((a)) a.focus();
    }

    function onDeleteRule(id) {
        dxConfirm("<% =JS_SafeString(ResString("msgAreYouSure")) %>", "DeleteRule(" + id + ");", ";", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
    }

    function DeleteRule(id) {
        var td = document.getElementById("flt_tr_" + id);
        if ((td)) td.parentNode.removeChild(td);
        CheckDeleteRuleBtn();
    }

    function CheckDeleteRuleBtn() {
        var tbl = document.getElementById("tblFilters");
        if ((tbl)) {
            var c = (tbl.rows.length>1);
            for (var i=0; i<=flt_max_id; i++) {
                var b = document.getElementById("flt_del_" + i);
                if ((b)) b.disabled = !c;
            }
        }
    }

    function initFilterData() {
        var dataDiv = document.getElementById("<% =divFilterData.ClientID %>");
        if ((dataDiv)) {
            var data = eval(dataDiv[text]);
            attr_list = data[0];
            attr_flt  = data[1];
        }
    }

    function initFilterGrid(filters, sComb) {
        flt_max_id = -1;
        var d = document.getElementById("divFilters");
        if ((d)) d.innerHTML = "<table id='tblFilters' border=0 cellspacing=4 cellpadding=0></table>";
        if ((filters)) {
            for (var i = 0; i < filters.length; i++) {
                var d = filters[i];
                AddFilerRow(d, i);
            }
        }

        var fc = document.getElementById("cbFilterCombination");
        if ((fc)) fc.selectedIndex = sComb * 1;

        CheckDeleteRuleBtn();
        IsAnyFilterChecked();
        //updateFilterWarning();
    }

    function ApplyFilter(do_reset) {
        var sData = (do_reset ? "" : getFilterString());
        showLoadingPanel();
        sendCommand("filter=" + encodeURIComponent(sData) + "&combination=" + document.getElementById("cbFilterCombination").value);
        if (!do_reset && !ShowAttributesPane) ShowAttributes(true);
    }

    function onFilterReset() {
        dxConfirm("<% =JS_SafeString(ResString("msgResetFilter")) %>", "DoFilterReset();", ";", "<% =JS_SafeString(ResString("titleConfirmation")) %>");
    }

    function DoFilterReset() {
        for (var i = 0; i <= flt_max_id; i++) {
            var c = document.getElementById('flt_chk_' + i);
            if ((c)) c.checked = false;
        }
        ApplyFilter(true);
    }

    function initSavedFilters(filters) {
        var cb = document.getElementById("cbSavedFilters");
        if ((cb) && (filters)) {
            clearCombobox(cb);            
            for (var i = -1; i < filters.length; i++) {
                var opt = document.createElement("option");
                opt.innerHTML = i >= 0 ? filters[i][0] : "";
                cb.appendChild(opt);
            }
        }
    }

    function onSavedFilterChanged() {
        var cb = document.getElementById("cbSavedFilters");
        if ((cb) && (savedFilters)) {
            updateToolbar();
            if (cb.selectedIndex == 0) {
                initFilterGrid(attr_flt, "0");
            } else {
                renderFilterUI(savedFilters[cb.selectedIndex - 1][1], savedFilters[cb.selectedIndex - 1][2]);
            }
        }
    }

    function renderFilterUI(s, comb) {
        sendAjax("action=parse_filter&value=" + encodeURIComponent(s) + "&combination=" + comb);        
    }

    function onAddSavedFilterClick() {
        var sName = prompt("Enter new filter name:", "");
               
        if ((sName) && sName.trim() != "") {
            sendAjax("action=add_filter&name=" + htmlEscape(sName.trim()) + "&value=" + encodeURIComponent(getFilterString()) + "&combination=" + document.getElementById("cbFilterCombination").value);
        }
    }

    function onUpdateSavedFilterClick() {
        var cb = document.getElementById("cbSavedFilters");
        if ((cb) && (cb.selectedIndex > 0) && (savedFilters.length > cb.selectedIndex - 1)) {
            var res = "";
            for (var i = 0; i < savedFilters.length; i++) {
                res += (res == "" ? "" : "," ) + "['" + savedFilters[i][0] + "','" + (i == cb.selectedIndex - 1 ? getFilterString() : savedFilters[i][1]) + "'," + (i == cb.selectedIndex - 1 ? document.getElementById("cbFilterCombination").value : savedFilters[i][2]) + "]";
            }
            sendAjax("action=update_filters&value=" + encodeURIComponent(res));
        }
    }

    function onRemoveSavedFilterClick() {
        dxConfirm("Are you sure you want to delete this filter?", "removeSavedFilter();", ";", "Confirmation");
    }
    
    function removeSavedFilter() {
        var cb = document.getElementById("cbSavedFilters");
        if ((cb) && (cb.selectedIndex > 0) && (savedFilters.length > cb.selectedIndex - 1)) {
            var res = "";
            for (var i = 0; i < savedFilters.length; i++) {
                if (i != cb.selectedIndex - 1) {
                    res += (res == "" ? "" : "," ) + "['" + savedFilters[i][0] + "','" + savedFilters[i][1] + "']";
                }
            }
            sendAjax("action=update_filters&value=" + encodeURIComponent(res));
        }
    }

    function onClearSavedFiltersClick() {
        dxConfirm("Are you sure you want to delete all filters?", "sendAjax(\"action=clear_filters\");", ";", "Confirmation");
    }

    function IsAnyFilterChecked() {
        var retVal = false;
        for (var i = 0; i <= flt_max_id; i++) {            
            var c = document.getElementById('flt_chk_' + i);
            if ((c) && (c.checked)) {
                retVal = true;
            }
        }    
                
        var btn = document.getElementById("btnApplyFilter");    
        if ((btn)) { btn.disabled = !retVal; }   
    }

    function getFilterString() {
        var sData = "";
        for (var i = 0; i <= flt_max_id; i++) {
            var c = document.getElementById('flt_chk_' + i);
            var a = document.getElementById('flt_attr_' + i);
            var o = document.getElementById('flt_oper_' + i);
            var v = document.getElementById('flt_val_' + i);
            
            var selValue = "";

            if (typeof (v) == 'object') {
             //if (v.multiple) {
                selValue = "";
                //Approach 1: <select multiple>
                //for (var k=0; k < v.options.length; k++)
                //{
                //    if (v.options[k].selected) 
                //    {
                //        if (selValue.length > 0) selValue += ";"
                //        selValue += v.options[k].value;
                //    }
                //}
                
                //approach 2: checkbox list
                var ul = document.getElementById("flt_val_" + i);                
                if (ul) {
                    if (ul.nodeName.toLowerCase() == "ul") {
                        var items = ul.getElementsByTagName("input");
                        for (var k = 0; k < items.length; k++) {                    
                            if (items[k].checked) {
                                if (selValue.length > 0) selValue += ";";
                                selValue += items[k].value;
                            }
                        }
                    } else {
                        if (v.type == "select-one" && v.selectedIndex != -1) {
                            selValue = ul.options[ul.selectedIndex].value;
                        } else {
                            selValue = ul.value;
                        }
                    }
                }
            } else { selValue = v.value; }
            
            if ((c) && (a) && (o)) {
                sData += (sData == "" ? "" : "<%=Flt_Row_Separator%>") + (c.checked ? 1 : 0) + "<% =Flt_Separator %>" + a.value  + "<% =Flt_Separator %>" + o.value  + "<% =Flt_Separator %>" + selValue;
            }
        }
        return sData;
    }

    function getFilterName() {
        var sData = "";
        
        var fc = document.getElementById("cbFilterCombination");
        var fcName = (fc) ? " " + fc.options[fc.selectedIndex].text + " " : "";

        for (var i = 0; i <= flt_max_id; i++) {
            var c = document.getElementById('flt_chk_' + i);
            var a = document.getElementById('flt_attr_' + i);
            var o = document.getElementById('flt_oper_' + i);
            var v = document.getElementById('flt_val_' + i);
            
            var selValue = "";

            if (typeof (v) == 'object') {                
                selValue = "";
                //approach 2: checkbox list
                var ul = document.getElementById("flt_val_" + i);                
                if ((ul)) {
                    if (ul.nodeName.toLowerCase() == "ul") {
                        var items = ul.getElementsByTagName("input");
                        for (var k = 0; k < items.length; k++) {                    
                            if (items[k].checked) {
                                if (selValue.length > 0) selValue += ";";
                                //selValue += items[k].value;
                                selValue += items[k].parentNode.innerText;
                            }
                        }
                    } else { 
                        if (v.type == "select-one" && v.selectedIndex != -1) {
                            selValue = v.options[v.selectedIndex].text;
                        } else { selValue = v.value; }                
                    }
                }
            }
            
            if ((c) && (a) && (o) && c.checked && a.selectedIndex != -1) {
                var condition = oper_list[o.value*1];                
                sData += (sData == "" ? "" : fcName) + "\"" + a.options[a.selectedIndex].text + "\" " + condition + ((selValue) && selValue != "" ? " \"" + selValue + "\"" : "");
            }
        }
        return sData == "" ? "" : "<b>" + sData + "</b>";
    }

    function OnUserName_Click(userID, withControls) {<% If isSLTheme Then %>        
        var w = ((window.opener) ? window.opener : window.parent);
        if ((w) && ((typeof w.NavigateToBowTieEvent) == "function")) {
            w.NavigateToBowTieEvent(userID, withControls);
        }<% End if %>
        return false;
    }

    var nav_to_pgid = "";

    function OnEventName_Click(eventID, withControls) {
        nav_to_pgid = 70008;
        var pg = <%=CurrentPageID%>;
        switch (pg*1) {
            case 70016:
            case 70005:
                nav_to_pgid = 70008;
                break;
            case 70017:
            case 70003:
                nav_to_pgid = 70009;
                break;
            case 70018:
            case 70004:
                nav_to_pgid = 70010;
                break;
        }        
        if (withControls == "1") {
            switch (pg*1) {
                case 70614:
                case 70635:
                    nav_to_pgid = 70608;
                    break;
                case 70611:
                case 70633:
                    nav_to_pgid = 70609;
                    break;
                case 70612:
                case 70634:
                    nav_to_pgid = 70610;
                    break;
            }   
        }
        sendCommand("selected_event=" + eventID);        
        return false;
    }

    function showEventMenu(event_id, event_name) {
        is_context_menu_open = false;
        $("div.context-menu").hide().remove();
        var sMenu = "<div class='context-menu' style='text-align: left;'>";
        <%--if (IsMixedEventsAllowed) {
            sMenu += "<label class='text' style='margin-left: 3px;'><input type='checkbox' " + (is_op ? " checked='checked' " : "") + " class='context-menu-item' onclick='setEventType(" +'"' + event_id +'"' + ",this.checked); return false;' style='text-align:left;' /><nobr>&nbsp;&nbsp;<%=ParseString("Is Opportunity %%Alternative%%")%>&nbsp;</nobr></label>";
        }--%>
        <%--sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameEventClick(" +'"' + event_id +'"' + "," +'"' + event_name +'"' + "); return false;' style='text-align:left;'><div><nobr><img align='left' style='vertical-align:middle;' src='<% =ImagePath %>edit_small.gif' alt='' >&nbsp;Edit&nbsp;\"" + htmlEscape(event_name) + "\"</nobr></div></a>";--%>
        sMenu += "<a href='' class='context-menu-item' onclick='OnEventName_Click(" +'"' + event_id +'"' + ",<%=If(HasListPage(PagesWithControlsList, CurrentPageID), 1, 0)%>); return false;' style='text-align:left;'><div><nobr><i class='fas fa-pen'></i>&nbsp;Jump&nbsp;to&nbsp;Bow-Tie</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameEventClick(" +'"' + event_id +'"' + "); return false;' style='text-align:left;'><div><nobr><i class='fas fa-pen'></i>&nbsp;Edit&nbsp;Item</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuEditEventGroupClick(" +'"' + event_id +'"' + "); return false;' style='text-align:left;'><div><nobr><i class='fas fa-pen'></i>&nbsp;<%=ParseString("Edit %%event%% group")%></nobr></div></a>";
        <%--sMenu += "<a href='' class='context-menu-item' onclick='confrimDeleteEvent(" +'"' + event_id +'"' + "," +'"' + event_name +'"' + "); return false;' style='text-align:left;'><div><nobr>&nbsp;<img align='left' style='vertical-align:middle;margin-left:3px;' src='<% =ImagePath %>delete_tiny.gif' alt='' >&nbsp;Delete&nbsp;\"" + htmlEscape(event_name) + "\"</nobr></div></a>";--%>
        sMenu += "<a href='' class='context-menu-item' onclick='confrimDeleteEvent(" +'"' + event_id +'"' + "," +'"' + htmlEscape(event_name) +'"' + "); return false;' style='text-align:left;'><div><nobr>&nbsp;<i class='fas fa-trash-alt'></i>&nbsp;Delete&nbsp;Item</nobr></div></a>";
        sMenu += "</div>";                
        var x = event.clientX;
        var y = event.clientY;
        var s = $(sMenu).appendTo("#divRiskResultsMain").css({top: (y - 90) + "px", left: x + "px"});                        
        $("div.context-menu").fadeIn(500);
        setTimeout('canCloseMenu()', 200);
    }

    function onContextMenuRenameEventClick(event_id) {
        $("div.context-menu").hide(200); is_context_menu_open = false;
        switchAltNameEditor(event_id, 1);
    }
    
    var alt_name = -1;
    var alt_value = "";
    
    function switchAltNameEditor(id, vis) {
        if (vis == 1) {
            if (alt_name < 0 || alt_name == id || (alt_name >= 0 && alt_name != id && switchAltNameEditor(alt_name, 0))) {                
                $("#lblName" + id).hide();
                $("#tbName" + id).show().focus();
                alt_name = id;
                alt_value = $("#tbName" + id).val();
                on_hit_enter = "switchAltNameEditor(" + id + ", 0);";
            }
        } else {
            if ($("#tbName" + id).is(':visible')) {
                var val = $("#tbName" + id).val();
                if (val == "") {
                    dxDialog("Name can't be empty!", 'setTimeout(\'$("#tbName' + id + '").focus();\', 50);', '');
                    return false;
                }
                $("#tbName" + id).hide();
                $("#lblName" + id).show();
                if (vis == 0 && val != "" && alt_value != "" && val != alt_value) {                
                    sendCommand("action=update_event_name&event_id=" + encodeURIComponent(id) + "&val=" + encodeURIComponent(val));
                }
                alt_name = -1;
                alt_value = "";
                on_hit_enter = "";
            }
        }
        return true;
    }
    
    function confrimDeleteEvent(event_id, event_name) {
        $("div.context-menu").hide(200); is_context_menu_open = false;
        dxConfirm("Are you sure you want to delete \"" + htmlEscape(event_name) + "\"?", "deleteEvent(\"" + event_id + "\");", ";", "Confirmation");
    }
    
    function deleteEvent(event_id) {
        sendCommand("action=delete_event&event_id=" + event_id);    
    }

    //function setEventType(event_id, is_opportunity) {
    //    sendCommand("action=event_type&event_id=" + event_id + "&is_opportunity=" + is_opportunity);
    //}

    function onContextMenuEditEventGroupClick(event_id) {
        $("div.context-menu").hide(200); is_context_menu_open = false;
        // get event group and precedence via ajax call
        sendAjax("action=event_group_data&event_id=" + event_id)
        // open editor in callback function
    }

    function OnNormalize(isImpact, tValue) {
        return false;
        var w = ((window.opener) ? window.opener : window.parent);
        if ((w) && ((typeof w.GetSLObject) == "function")) {
            var sl = w.GetSLObject();
            if ((sl) && (sl.Content.Shell)) sl.Content.Shell.SessionRiskNormalized(((isImpact) ? "1" : "0"), ((tValue)? "4" : "2"));
        }
    }

    function OnNormalizeLocal(isImpact, tValue) {
        return false;
        var w = ((window.opener) ? window.opener : window.parent);
        if ((w) && ((typeof w.GetSLObject) == "function")) {
            var sl = w.GetSLObject();
            if ((sl) && (sl.Content.Shell)) sl.Content.Shell.SessionRiskNormalizedLocal(((isImpact) ? "1" : "0"), ((tValue)? "0" : "3"));
        }
    }

    //function updateAttributesUI() {
    //    if (ShowAttributesPane) { 
    //        //$("#pnlEventAttributes").show();
    //        //initFilterGrid();                  
    //    } else { 
    //        //$("#pnlEventAttributes").hide();
    //    }
    //}

    function ShowAttributes(isShow) {
        $("#cbShowAttributes").prop("checked", isShow);
        $("#cbShowAttributesRiskMap").prop("checked", isShow);
        ShowAttributesPane = isShow;
        //updateAttributesUI();
        sendCommand("showattributes=" + (ShowAttributesPane ? "1" : "0"));
        updateFilterWarning();
    }

    var is_reset = false;

    function initDollarValueUI(isEnabled) {
        $("#cbDollarValue").prop('disabled', !isEnabled);
        $("#lblDollarValue").css("color", (isEnabled ? "black" : "#999999"));
        <%--var lblTarget = "<%=ResString("lblEnterprise")%>";
        if (DollarValueTarget != "") {
            for (var i = 0; i < all_obj_data.length; i++) {
                if (all_obj_data[i][0] == DollarValueTarget) lblTarget = '"' + all_obj_data[i][1] + '"';
            }
            for (var i = 0; i < events_data.length; i++) {
                if (events_data[i][0] == DollarValueTarget) lblTarget = '"' + events_data[i][1] + '"';
            }
        }
        $("#lblDollarValue").html("<%=ResString("lblShowDollarValue")%>" + (DollarValue != UNDEFINED_INTEGER_VALUE ? " (<%=ResString("lblValueOf")%>&nbsp;" + lblTarget + ": " + number2locale(DollarValue, true) + ")" : ""));--%>
    }

    function ShowDollarValueClick(chk) {
        ShowDollarValue = chk;
        sendCommand("action=" + ACTION_SHOW_DOLLAR_VALUE + "&chk=" + (chk ? "1" : "0"));
    }

    function onDollarValueTargetChange(cb) {
        var val = cb.options[cb.selectedIndex].getAttribute("doll_value");
        var tb = document.getElementById("tbDollarValue");
        if ((tb)) tb.innerText = val == UNDEFINED_INTEGER_VALUE ? "" : val;
        sendAjax("action=selected_dollar_value_target&value=" + cb.value + "&doll_value=" + val);
    }

    function onEditDollarValueClick() {
        var dataDiv = document.getElementById("<% =divNodesDataWithDollars.ClientID %>");
        if ((dataDiv)) {
            var data = eval(dataDiv[text]);
            all_obj_data = data[0];
            events_data  = data[1];
        }

        var sValue = (DollarValue == UNDEFINED_INTEGER_VALUE ? "0" : DollarValue + "");
        var events_data_len  = events_data.length;
        var all_obj_data_len = all_obj_data.length;
        // init combobox
        var sCB = "";
        var sOpts ="";
        sOpts += "<option value='' doll_value='" + Math.round(all_obj_data[0][2]) + "' " + (DollarValueTarget == "" ? " selected='selected' " : "") + "><% = JS_SafeString(ResString("lblEnterprise")) %></option>";
        if (all_obj_data_len > 0) {
            sOpts += "<option disabled='disabled'> </option>";
            sOpts += "<option disabled='disabled'>------<%=ParseString("%%Impacts%%")%>------</option>";
            for (var i = 0; i < all_obj_data_len; i++) {
                sOpts += "<option value='" + all_obj_data[i][0] + "' doll_value='" + Math.round(all_obj_data[i][2]) + "' " + (DollarValueTarget == "" || all_obj_data[i][0] != DollarValueTarget ? "" : " selected='selected' ") + ">" + all_obj_data[i][1] + "</option>";
            }
        }
        if (false && events_data_len > 0) {            
            sOpts += "<option disabled='disabled'> </option>";
            sOpts += "<option disabled='disabled'>------<%=ParseString("%%Alternatives%%")%>------</option>";
            for (var i = 0; i < events_data_len; i++) {
                sOpts += "<option value='" + events_data[i][0] + "' doll_value='" + Math.round(events_data[i][IDX_EVENT_DOLLAR_VALUE]) + "' " + (DollarValueTarget == "" || events_data[i][0] != DollarValueTarget ? "" : " selected='selected' ") + ">" + events_data[i][1] + "</option>";
            }
        }
        sCB = "<select id='cbDollarValueTarget' style='width:180px; margin-bottom:-1px;' onchange='onDollarValueTargetChange(this);'>" + sOpts + "</select>";

        var sInfo = "<%=ResString("msgEnterDollarValue")%><br><br>";

        dxDialog(sInfo + "<center><span><% = JS_SafeString(ResString("lblValueOf")) %>&nbsp;" + sCB + "</span>&nbsp;=&nbsp;<input id='tbDollarValue' type='input text' style='text-align:right;' value='" + sValue + "' onkeydown='is_reset = false;'></center>", "onSaveDollarValue($('#tbDollarValue').val(), $('#cbDollarValueTarget').val());", "onResetDollarValue();",  "<% = JS_SafeString(ResString("titleEditUSD")) %>", undefined, "<%=ResString("btnReset")%>");
        //$("#btnResetDollarValue").prop("disabled", DollarValue == UNDEFINED_INTEGER_VALUE);        
        //$("#jDialog_btnExtra").prop("disabled", DollarValue == UNDEFINED_INTEGER_VALUE);
    }

    function onSaveDollarValue(value, target) {        
        if (typeof value != 'undefined' && validFloat(value)) {
            var dValue = str2double(value);            
            DollarValue = dValue;
            DollarValueTarget = target;
            initDollarValueUI(true);
            DollarValueDefined = true;
            sendAjax("action=set_dollar_value&value=" + value + "&target=" + target);
        } else { is_reset = true; }
        if (is_reset) {
            ShowDollarValue = false;
            DollarValue = UNDEFINED_INTEGER_VALUE;
            DollarValueTarget = "";
            DollarValueDefined = false;
            sendAjax("action=set_dollar_value&value=" + UNDEFINED_INTEGER_VALUE + "&target=" + target);
        }
    }

    function onResetDollarValue() {
        is_reset = true;
        onSaveDollarValue(UNDEFINED_INTEGER_VALUE, "");
        $('#tbDollarValue').val(0);
        //$("#btnResetDollarValue").prop("disabled", true);
        //$("#jDialog_btnExtra").prop("disabled", true);        
        is_reset = false;
        return false;
    }

    function btnSettingsClick() {
        InitSettingsDlg();
    }

    function InitSettingsDlg() {                
        $("#divSettings").dxPopup({
              visible: true,
              width: 612,
              //height: "auto",
              title: "Settings",
              onShowing:  function() { 
                  // init settings
                  var cb = document.getElementById('cbSwitchAxes');
                  if ((cb)) cb.checked = RiskMapSwitchAxes;
                  cb = document.getElementById('cbShowRegions0'); 
                  if ((cb)) cb.checked = RiskMapShowRegions == 0;
                  cb = document.getElementById('cbShowRegions1'); 
                  if ((cb)) cb.checked = RiskMapShowRegions == 1;
                  cb = document.getElementById('cbShowRegions2'); 
                  if ((cb)) cb.checked = RiskMapShowRegions == 2;
                  cb = document.getElementById('cbShowRegions3'); 
                  if ((cb)) cb.checked = RiskMapShowRegions == 3;
                  cb = document.getElementById('cbZoomRiskPlot');
                  if ((cb)) cb.checked = ZoomPlot;
                  cb = document.getElementById('cbShowLegend');
                  if ((cb)) cb.checked = RiskMapShowLegends;
                  cb = document.getElementById('cbShowLabels');
                  if ((cb)) cb.checked = RiskMapShowLabels;
                  cb = document.getElementById('cbJitter');
                  if ((cb)) cb.checked = RiskMapJitterDatapoints;
                  cb = document.getElementById('cbBubbleSize');
                  if ((cb)) cb.checked = RiskMapBubbleSizeWithRisk;

                  sendAjax("action=get_rand_seed");
              },
              onHiding: function() {  },
              elementAttr: {
                  style: "padding-bottom: 25px;"
              }
        });

        $("#btnSettingsClose").dxButton({
            text: "<%=ResString("btnClose")%>",
            icon: "fas fa-times",
            onClick: function() {
                $("#divSettings").dxPopup("hide");
            }
        });
    }

    function sliderBubbleSizeChange(value) {        
        <%If IsRiskMapPage() Then%>
        var v = value / 10;
        if (v != bubble_mult) {
            bubble_mult = v;
            //sendAjax("action=" + ACTION_SET_BUBBLE_SIZE + "&val=" + (bubble_mult * 10));
            sendCommand("action=" + ACTION_SET_BUBBLE_SIZE + "&val=" + (bubble_mult * 10));
            <%--loadChart('divRiskMap', window["plot1"], plot_data1, RiskPlotMode == 1 ? "<%=ParseString(" With %%Controls%%")%>" : "<%=ParseString(" Without %%Controls%%")%>");
            if (RiskPlotMode == 2) loadChart('divRiskMapWC', window["plot2"], plot_data2, "<%=ParseString(" With %%Controls%%")%>");--%>
        }
        <%End If%>
    }

    function cbDecimalsChange(value) {
        decimals = value;
        sendCommand("action=decimals&value=" + value);
    }

    function sendTreeViewCommand(value) {
        showLoadingPanel();
        $find("<%=RadAjaxPanelTree.ClientID%>").ajaxRequest(value);
    }

    function cbShowEventNumbersChecked(chk) {
        $("#cbEventIDMode").prop("disabled", !chk); 
        ShowEventNumbers = chk; 
        sendTreeViewCommand("action=show_event_numbers&value=" + chk);
        //sendCommand("action=show_event_numbers&value=" + chk);
    }

    function cbShowEventDescriptionsChecked(chk) {
        sendCommand("action=show_event_descriptions&value=" + chk);
    }

    function cbEventIDModeChange(value) {
        sendTreeViewCommand("action=event_id_mode&value=" + value);
        //sendCommand("action=event_id_mode&value=" + value);
    }

    function onGroupByAttribute(chk) {
        var cb = document.getElementById("#cbGroupingCategories");
        if ((cb)) cb.disabled = (chk ? "" : "disabled");        
        //if (chk) { $("#cbGroupTotals").show(); $("#lblGroupTotals").show(); } else { $("#cbGroupTotals").hide(); $("#lblGroupTotals").hide(); }
        sendCommand("action=is_grouping&chk=" + chk);
    }

    function onShowGroupTotals(chk) {
        sendCommand("action=group_totals&chk=" + chk);
    }

    function onShowSolverPane(chk) {
        sendCommand("action=solver_pane&chk=" + chk);
    }

    function onSetGroupingCategory(value) {
        sendCommand("action=grouping_category&value=" + value);
    }

    function InitTooltips() {
        $(".sort_link_likelihood").prop("title","<%=ResString("hintSortByLikelihood") %>");  
        $(".sort_link_impact").prop("title","<%=ResString("hintSortByImpact") %>");  
        $(".sort_link_risk").prop("title","<%=ResString("hintSortByRisk") %>");  
    }

    function removeExtraBorders() {
        //if (ColorCodingByCategory) $("TR.grid_row TD").css("background", ""); 
        $("TR.grid_row TD").css({"border-bottom" : "0px", "background" : "" }); 
        $("TR.grid_row_alt TD").css({"border-bottom" : "0px", "background" : "" }); 
        $("TR.grid_row_alt_dark TD").css({"border-bottom" : "0px", "background" : "" }); 
    }

    function onSelectColumnsClick() {
        initSelectColumnsForm("Select Columns");
        dlg_select_columns.dialog("open");
        dxDialogBtnDisable(true, true);
    }

    function initSelectColumnsForm(_title) {
        cancelled = false;

        var labels = "";

        // generate list of attributes
        var attr_list_len = attr_list.length;
        for (var k = 0; k < attr_list_len; k++) {
            var checked = attr_list[k][aidx_col_vis] == 1;
            labels += "<label><input type='checkbox' class='select_clmn_cb' value='" + attr_list[k][aidx_id] + "' " + (checked ? " checked='checked' " : " " ) + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + attr_list[k][aidx_name] + "</label><br>";
        }    

        $("#divSelectColumns").html(labels);

        dlg_select_columns = $("#selectColumnsForm").dialog({
            autoOpen: false,
            modal: true,
            width: 420,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: _title,
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                    dlg_select_columns.dialog( "close" );
                }},
                Cancel: function() {
                    cancelled = true;
                    dlg_select_columns.dialog( "close" );
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
            },
            close: function() {
                $("body").css("overflow", "auto");                
                if (!cancelled) {
                    var clmn_ids = "";
                    var cb_arr = $("input:checkbox.select_clmn_cb");
                    $.each(cb_arr, function(index, val) { var cid = val.value + ""; getAttrById(cid)[aidx_col_vis] = val.checked ? 1 : 0; if (val.checked) { clmn_ids += (clmn_ids == "" ? "" : ",") + cid; } });
                    sendCommand('action=select_columns&column_ids=' + clmn_ids);
                }
            }
        });
    }

    function filterColumnsCustomSelect(chk) {
        $("input:checkbox.select_clmn_cb").prop('checked', chk*1 == 1);
        dxDialogBtnDisable(true, false);
    }

    function filterByAttributesDialog() {
        if ((dlg_attributes)) dlg_attributes.dialog('destroy'); //.remove();
        
        dialog_result = false;
        $("#divFilters").empty();
        
        dlg_attributes = $("#divAttributesFilter").dialog({
            autoOpen: false,
            width: 710,
            height: 435,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "<%=ResString("lblFilterByAltAttr")%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"Apply", id:"btnApplyFilter", click: function() { dialog_result = true; dlg_attributes.dialog( "close" ); }}, 
                     { text:"<%=ResString("btnClose")%>", click: function() { dialog_result = false; dlg_attributes.dialog( "close" ); }}],
            open: function() { $("body").css("overflow", "hidden"); },
            close: onFilterAttrDialogClose
        });
        initSavedFilters(savedFilters);
        updateToolbar();
        initFilterData();
        initFilterGrid(attr_flt, "0");
        dlg_attributes.dialog('open');
    }

    function onFilterAttrDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            ApplyFilter(false);
            $("#divFilters").empty();
        }        
    }

    // Users And Groups
    function SelectAllUsers(action) {
        var cb_arr = $("input:checkbox.user_cb");
        $.each(cb_arr, function(index, val) { 
            if (action == 0) val.checked = false; 
            if (action == 1) val.checked = true;
            if (action == 2) { val.checked = val.getAttribute("has_data") == "1" }
        });
        updateUsersWithDataInGroupCheckState();
        return false;
    }

    var dialog_result = false;

    function selectUsersDialog() {
        if ((dlg_users)) dlg_users = null;
        document.body.style.cursor = "wait";
        dialog_result = false;
        
        dlg_users = $("#divUsersAndGroups").dialog({
            autoOpen: false,
            width: 880,
            height: 435,
            minHeight: 150,
            maxHeight: 550,
            modal: true,
            closeOnEscape: true,
            dialogClass: "no-close",
            bgiframe: false,
            title: "<%=SafeFormString(ResString("btnParticipantsAndGroups"))%>",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"<%=ResString("btnOK")%>", click: function() { dialog_result = true; dlg_users.dialog( "close" ); }},
                      { text:"<%=ResString("btnCancel")%>", click: function() { dialog_result = false; dlg_users.dialog( "close" ); }}],
            open:  function() { $("body").css("overflow", "hidden"); initUsersTable(); initGroupsTable(); document.body.style.cursor = "default";},
            close: onUsersDialogClose
        });
        dlg_users.dialog('open');
    }

    function getUserByID(id) {
        for (var u = 0; u < users_data.length; u++) {
            if (users_data[u][IDX_USER_ID] == id) return users_data[u];
        }
        for (var u = 0; u < groups_data.length; u++) {
            if (groups_data[u][IDX_USER_ID] == id) return groups_data[u];
        }
        return false;
    }

    function onUsersDialogClose() {
        $("body").css("overflow", "auto");
        if (dialog_result) {
            var params = "";
            var sa_user_id = -1;
            //var cb_arr = $("input:checkbox.user_cb:checked");
            //$.each(cb_arr, function(index, val) { params += (params == "" ? "" : ",") + val.getAttribute("uid"); });
            var cb_arr = $("input:checkbox.user_cb");
            $.each(cb_arr, function(index, val) { var uid = val.getAttribute("uid")*1; if (val.checked) {params += (params == "" ? "" : ",") + uid;}; var u = getUserByID(uid); if ((u)) u[IDX_USER_CHECKED] = (val.checked ? 1 : 0); });
            sendCommand("action="+ACTION_SELECTED_USERS+"&ids="+params+"&sa_user="+sa_user_id, true);        
        }        
    }

    function initUsersTable() {        
        var columns = [];

        //init columns headers                
        columns.push({ "title" : "", "class" : "td_center", "sortable" : true, "searchable" : false });
        columns.push({ "title" : "UserID", "bVisible" : false, "searchable" : false });
        columns.push({ "title" : "<%=ResString("tblSyncUserName")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });
        columns.push({ "title" : "<%=ResString("tblEmailAddress")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });        
        columns.push({ "title" : "<nobr><%=ResString("lblHasData")%>&nbsp;&nbsp;</nobr>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });

        $.each(columns, function(index, val) { val.title += "&nbsp;&nbsp;"; });

        table_users = $('#tableUsers').DataTable( {
            dom: 'frti',
            data: users_data,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  true,
            "order": [[ 2, 'asc' ]],
            scrollY: 245,
            //scrollX: true,
            stateSave: false,
            searching: true,
            info:      false,
            "rowCallback": function( row, data, index ) {
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" onclick='updateUsersWithDataInGroupCheckState();'>");
                $("td:eq(1)", row).html(htmlEscape(data[IDX_USER_NAME]));
                $("td:eq(3)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : "<%=ResString("lblNo")%>"));
                if (data[IDX_USER_HAS_DATA] != 1) { $("td", row).css("color", "#909090"); $("td:eq(0)", row).children().first().attr("disabled","disabled"); }
            },
            "language" : {"emptyTable" : "<h6 style='margin:2em 10em'><nobr><% =GetEmptyMessage()%></nobr></h6>"}
        });
        
        setTimeout('$(".dataTables_filter").css({"float":"left", "padding-bottom":"10px"});', 100);
        setTimeout('$("input[type=search]").focus();', 1000);

        // search in groups table when typing in a search field of the users table
        table_users.on('search.dt', function () {
            table_groups.search(table_users.search()).draw();
        } );
    }

    function allGroupUsersWithDataChecked(grp_id) {
        var retVal = true;

        var g = [];
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][IDX_USER_ID] == grp_id) g = groups_data[i];
        }

        if (g.length > 0) {
            if (g[IDX_GROUP_PARTICIPANTS].length == 0) retVal = false;
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);                                
                if ((u)) {
                    if (u[IDX_USER_HAS_DATA] == 1 && u[IDX_USER_CHECKED] != 1) retVal = false;
                }
            }
        }

        return retVal;
    }

    function checkUsersWithDataInGroup(grp_id, chk) {
        var g = [];
        for (var i = 0; i < groups_data.length; i++) {
            if (groups_data[i][IDX_USER_ID] == grp_id) g = groups_data[i];
        }

        if (g.length > 0) {
            if (g[IDX_GROUP_PARTICIPANTS].length == 0) retVal = false;
            var cb_arr = $("input:checkbox.user_cb");
            for (var i = 0; i < g[IDX_GROUP_PARTICIPANTS].length; i++) {
                var u = getUserByID(g[IDX_GROUP_PARTICIPANTS][i]);                                
                if ((u) && u[IDX_USER_HAS_DATA] == 1) {
                    $.each(cb_arr, function(index, val) { var uid = val.getAttribute("uid")*1; if (uid == g[IDX_GROUP_PARTICIPANTS][i]) val.checked = chk; });
                }
            }
        }
    }

    function updateUsersWithDataInGroupCheckState() {        
        var cb_arr = $("input:checkbox.group_all_users_cb");
        $.each(cb_arr, function(index, val) { 
            var gid = val.getAttribute("uid")*1; 
            var g = [];
            for (var i = 0; i < groups_data.length; i++) {
                if (groups_data[i][IDX_USER_ID] == gid) g = groups_data[i];
            }
            if (g.length > 0) {                
                var u_arr = $("input:checkbox.user_cb");
                var all_checked = g[IDX_GROUP_PARTICIPANTS].length > 0;
                $.each(u_arr, function(u_index, u_val) { 
                    var uid = u_val.getAttribute("uid")*1; 
                    var u = getUserByID(uid);
                    if ($.inArray(uid, g[IDX_GROUP_PARTICIPANTS]) >= 0 && u[IDX_USER_HAS_DATA] == 1 && !u_val.checked) all_checked = false; 
                });
                val.checked = all_checked;
            }
        });
    }

    function initGroupsTable() {        
        var columns = [];

        //init columns headers                
        columns.push({ "title" : "", "class" : "td_center", "sortable" : true, "searchable" : false });
        columns.push({ "title" : "GroupID", "bVisible" : false, "searchable" : false });
        columns.push({ "title" : "<%=ResString("lblGroupName")%>&nbsp;&nbsp;", "class" : "td_left", "type" : "html", "sortable" : true, "searchable" : true });
        columns.push({ "title" : "<nobr><%=ResString("lblHasData")%>&nbsp;&nbsp;</nobr>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });        
        columns.push({ "title" : "<small>Select all users with data</small>", "class" : "td_center", "type" : "html", "sortable" : true, "searchable" : false });

        $.each(columns, function(index, val) { val.title += "&nbsp;&nbsp;"; });

        table_groups = $('#tableGroups').DataTable( {
            dom: 'rti',
            data: groups_data,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  true,
            "order": [[ 0, 'desc' ]],
            scrollY: 245,
            //scrollX: true,
            stateSave: false,
            searching: true,
            info:      false,
            "rowCallback": function( row, data, index ) {
                $("td:eq(0)", row).html("<input type='checkbox' class='user_cb' uid='"+data[IDX_USER_ID]+"' has_data='"+data[IDX_USER_HAS_DATA]+"' "+(data[IDX_USER_CHECKED] == 1?" checked ":"")+" >");
                $("td:eq(1)", row).html((data[IDX_USER_ID] != -1 ? "<i id='imgGrp" + data[IDX_USER_ID] + "' class='fas fa-plus' style='cursor:pointer;' onclick='expandGroupUsers(" + data[IDX_USER_ID] + ");'></i>" : "") + htmlEscape(data[IDX_USER_NAME]) + "<div id='divExpandedUsers" + data[IDX_USER_ID] + "'></div>");
                $("td:eq(2)", row).html((data[IDX_USER_HAS_DATA] == 1 ? "<%=ResString("lblYes")%>" : "<%=ResString("lblNo")%>"));
                $("td:eq(3)", row).html("<input type='checkbox' class='group_all_users_cb' uid='"+data[IDX_USER_ID]+"' " + (allGroupUsersWithDataChecked(data[IDX_USER_ID]) ? " checked ":"")+" onclick='checkUsersWithDataInGroup(\"" + data[IDX_USER_ID] + "\", this.checked);' " + (data[IDX_GROUP_PARTICIPANTS].length == 0 ? " disabled='disabled' " : "") + " >");
                if (data[IDX_USER_HAS_DATA] != 1) { $("td", row).css("color", "#909090"); $("td:eq(0)", row).children().first().attr("disabled","disabled"); }
            },
        });
    }
    // End Users And Groups

    // Riskion Event Groups for LEC
    var cur_event_id = "";

    function eventGroupEditor(event_id, event_group_name, event_group_precedence) {
        var cur_event_id = event_id;
        var sContent = "<br><br><table class='text' cellpadding= cellspacing=0 border=0><tr><td align='right'><%=JS_SafeString(ParseString("%%Event%% group name:"))%>&nbsp;</td><td><input type='text' id='tbGroupName' value='" + event_group_name + "' ></td></tr><tr><td align='right'><%=JS_SafeString(ParseString("%%Event%% precedence:"))%>&nbsp;</td><td><input type='text' id='tbGroupValue' style='text-align:right;' value='" + event_group_precedence + "' ></td></tr></table>"
        dxDialog(sContent, "onSaveEventGroup(" + cur_event_id + ", $('#tbGroupName').val(), $('#tbGroupValue').val());", ";",  "<% = JS_SafeString(ParseString("Edit %%event%% group")) %>");
    }

    function onSaveEventGroup(event_id, event_group_name, event_group_precedence) {
        sendCommand("action=save_event_group&event_id=" + event_id + "&group_name=" + event_group_name + "&group_value=" + event_group_precedence);
    }
    // End Riskion Event Groups for LEC

    // Risk Optimization 
    function callSolve() {
        var sBudget = str2cost($('#tbBudgetLimit').val());
        var sRisk   = (!ShowDollarValue ? $('#tbMaxRisk').val() : str2cost($('#tbMaxRisk').val()));
        var sReduct = (!ShowDollarValue ? $('#tbMinReduction').val() : str2cost($('#tbMinReduction').val()));
        sendCommand('action=solve&value=' + sBudget + '&risk=' + sRisk + '&reduction=' + sReduct, true);
    }

    // Select Events Dialog
    function onSelectEventsClick() {
        if (event_list.length > 0) {
            initSelectEventsForm("Select <%=ParseString("%%Alternatives%%")%>");
            dlg_select_events.dialog("open");
            dxDialogBtnDisable(true, true);
        }
    }

    function initSelectEventsForm(_title) {
        cancelled = false;

        var labels = "";

        // generate list of events
        var event_list_len = event_list.length;
        for (var k = 0; k < event_list_len; k++) {
            var checked = event_list[k][IDX_EVENT_CHECKED] == 1;
            labels += "<div class='divCheckbox'><label><input type='checkbox' class='select_event_cb' value='" + event_list[k][IDX_EVENT_GUID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(event_list[k][IDX_EVENT_NAME]) + "</label></div>";
        }

        $("#divSelectEvents").html(labels);

        dlg_select_events = $("#selectEventsForm").dialog({
            autoOpen: false,
            modal: true,
            width: 420,
            minWidth: 530,
            maxWidth: 950,
            minHeight: 250,
            maxHeight: mh,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: _title,
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: {
                    id: 'jDialog_btnOK', text: "OK", click: function () {
                        dlg_select_events.dialog("close");
                    }
                },
                Cancel: function () {
                    cancelled = true;
                    dlg_select_events.dialog("close");
                }
            },
            open: function () {
                $("body").css("overflow", "hidden");
            },
            close: function () {
                $("body").css("overflow", "auto");
                if (!cancelled) {
                    sEventIDs = "";
                    var cb_arr = $("input:checkbox.select_event_cb");
                    var chk_count = 0;
                    $.each(cb_arr, function (index, val) { 
                        var cid = val.value + ""; 
                        if (val.checked) { 
                            sEventIDs += (sEventIDs == "" ? "" : ",") + cid; 
                            chk_count += 1;
                        } 
                        event_list[index][IDX_EVENT_CHECKED] = (val.checked ? 1 : 0);
                    });
                    //if ((chk_count == event_list.length) && (chk_count > 0)) sEventIDs = "all";
                    //if (chk_count == 0) sEventIDs = "";
                    sendCommand('action=select_events&event_ids=' + sEventIDs); // save the selected events via ajax
                }
            }
        });
        $(".ui-dialog").css("z-index", 9999);
    }

    function filterSelectAllEvents(chk) {
        $("input:checkbox.select_event_cb").prop('checked', chk * 1 == 1);
        dxDialogBtnDisable(true, false);
    }
    // end Select Events Dialog

    // Select Controls dialog
    function onSelectControlsClick() {
        if (controls_data.length > 0) {
            initSelectAvailableControls("Available <%=ParseString("%%Controls%%")%>");
            //dlg_select_controls.dialog("open");
            dxDialogBtnDisable(true, true);
        }
    }

    function initSelectAvailableControls(_title) {
        cancelled = false;

        var labels = "";

        // generate list of events
        var controls_data_len = controls_data.length;
        for (var k = 0; k < controls_data_len; k++) {
            var checked = controls_data[k][COL_CTRL_CHECKED] == 1;
            labels += "<div class='divCheckbox'><label><input type='checkbox' class='select_control_cb' value='" + controls_data[k][COL_CTRL_GUID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(controls_data[k][COL_CTRL_NAME]) + "</label></div>";
        }

        $("#divSelectControls").html(labels);

        $("#selectControlsForm").dxPopup({
            width: "60%",
            //height: "80%",
            visible: true,
            title: _title,            
            onHiding: function () {
                if (!cancelled) {
                    sControlIDs = "";
                    var cb_arr = $("input:checkbox.select_control_cb");
                    var chk_count = 0;
                    $.each(cb_arr, function (index, val) { 
                        var cid = val.value + ""; 
                        if (val.checked) { 
                            sControlIDs += (sControlIDs == "" ? "" : ",") + cid; 
                            chk_count += 1;
                        } 
                        controls_data[index][COL_CTRL_CHECKED] = (val.checked ? 1 : 0);
                    });
                    //if ((chk_count == controls_data.length) && (chk_count > 0)) sControlIDs = "all";
                    //if (chk_count == 0) sControlIDs = "none";
                    sendCommand('action=select_controls&control_ids=' + sControlIDs); // save the selected controls via ajax
                }
            }
        });

        $("#btnSelectControlsOK").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            onClick: function() {
                $("#selectControlsForm").dxPopup("hide");
            }
        });
        
        $("#btnSelectControlsCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            onClick: function() {
                cancelled = true;
                $("#selectControlsForm").dxPopup("hide");
            }
        });
    }

    function filterSelectAllControls(chk) {
        $("input:checkbox.select_control_cb").prop('checked', chk * 1 == 1);
        dxDialogBtnDisable(true, false);
    }
    // end Select Controls dialog

    // Optimizer Types Tabs
    function openOptimizerTab(evt, tab_id) {
        var i, tabcontent, tablinks;

        // Get all elements with class="ec_toolbar_tabcontent" and hide them
        tabcontent = document.getElementsByClassName("ec_toolbar_tabcontent");
        for (i = 0; i < tabcontent.length; i++) {
            tabcontent[i].style.display = "none";
        }

        // Get all elements with class="ec_toolbar_tablink" and remove the class "active"
        //tablinks = document.getElementsByClassName("ec_toolbar_tablink");
        //for (i = 0; i < tablinks.length; i++) {
        //    tablinks[i].className = tablinks[i].className.replace(" active", "");
        //}

        // Show the current tab, and add an "active" class to the link that opened the tab
        document.getElementById("tabContent" + tab_id).style.display = "block";
        //evt.currentTarget.className += " active";

        // ajax
        sendAjax("action=optimizer_type&value=" + tab_id);
    }
        // end Optimizer Types Tabs

    // end Risk Optimization

    function updateToolbar() {
        if (isRiskMap) {
            <%--// set EventCloud toggle button state
            var img = document.getElementById("btnEventCloudImg");
            if ((img)) {
                img.src = "../../images/risk/" + (isEventCloud ? "eventcloud_checked.png" : "eventcloud_unchecked.png");
                img.className = (isEventCloud ? "toggle_btn_checked" : "toggle_btn_unchecked");
            }--%>

            if (isEventCloud) {
                $("#mnuZoomRiskPlot").hide();
                $("#mnuDatapointSize").hide();
            } else {
                $("#mnuZoomRiskPlot").show();
                $("#mnuDatapointSize").show();
            }
        }

        // update filtering UI
        var cb = document.getElementById("cbSavedFilters");
        if ((cb)) {
            var btnUpd = document.getElementById("btnUpdateSavedFilter");
            if ((btnUpd)) btnUpd.disabled = cb.selectedIndex <= 0;

            var btnClear = document.getElementById("btnClearSavedFilters");
            if ((btnClear)) btnClear.disabled = cb.options.length <= 1;
            
            var btnRem = document.getElementById("btnRemoveSavedFilter");
            if ((btnRem)) btnRem.disabled = cb.selectedIndex <= 0;
        }

        // update the Select Events icon
        var img = document.getElementById("imgFilterEvents");        
        if ((img)) {
            var event_list_len = event_list.length;
            var checkedEvents = 0;
            for (var k = 0; k < event_list_len; k++) {
                if (event_list[k][IDX_EVENT_CHECKED] == 1) checkedEvents += 1;
            }
            img.src = "../../images/ra/" + (checkedEvents == event_list_len ? "filter20.png" : "filter20_red.png");
        }
        // update the Available Controls icon
        img = document.getElementById("imgFilterControls");
        if ((img)) {
            var controls_data_len = controls_data.length;
            var checkedControls = 0;
            for (var k = 0; k < controls_data_len; k++) {
                if (controls_data[k][COL_CTRL_CHECKED] == 1 || controls_data[k][COL_CTRL_TYPE] == ctUndefined) checkedControls += 1;
            }
            img.src = "../../images/ra/" + (checkedControls == controls_data_len ? "filter20.png" : "filter20_red.png");
        }
        //if (showHiddenSettings) {
            //$("#tdSimulatedSwitch").show();
            //$("#pnlNormSettings").show();
        //} else {
        //    $("#tdSimulatedSwitch").hide();
        //    $("#pnlNormSettings").hide();
        //}

        $("#divProbabilityCalculationMode").toggle(!UseSimulatedValues);
        $(".use-shuffling").toggle(UseSimulatedValues);
        
        $(".on_advanced").toggle(isAdvancedMode);
    }

    //function onToolbarDblClick() {
    //    showHiddenSettings = !showHiddenSettings;
    //    sendAjax("action=show_hidden_settings&val=" + showHiddenSettings);
    //    updateToolbar();
    //}

    var plot_wnd = null;

    function showExtraPlot() {
        var _PGURL_EXTRA_PLOT = '<% = GetLecUrl() %>';
        var event_list = _PGURL_EXTRA_PLOT.indexOf("?") === -1 ? "?ids=" : "&ids=";
        plot_wnd = CreatePopup(_PGURL_EXTRA_PLOT + event_list + "&<% =_PARAM_TEMP_THEME + "=" + _THEME_SL %>", 'RiskPlot', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=1300,height=700');
        setTimeout("if ((plot_wnd)) plot_wnd.focus();", 100);
    }

    function getOptimalNumberOfTrials() {
        sendAjax('action=get_optimal_unmber_of_trials');
    }

    function IsAction(sAction) {
        return cmd.indexOf("action="+sAction) == 0
    }

    function syncReceived(params) {
        if ((params) && (IsAction(ACTION_GET_TREATMENTS))) {            
            showExpadedTreatments(params);
        }        

        if (IsAction("add_category")) {
            var new_cat_id = eval(params)[1];
            cat_data.push([new_cat_id, new_cat_name]);
            
            var lb = document.getElementById("lbCategories");
            createCategoryElement(lb, cat_data.length-1, new_cat_id, new_cat_name, ctrl_cat);
            lb.scrollTop = lb.lastChild.offsetTop;
        }

        if (IsAction(ACTION_SHOW_DOLLAR_VALUE)) {
            // do nothing
        }

        if (IsAction("get_optimal_unmber_of_trials")) {
            numSimulations = eval(params)[1] * 1;
            $("#tbNumSimulations").val(numSimulations).effect("highlight", {}, 900);;
        }

        if (IsAction("selected_dollar_value_target") || IsAction("set_dollar_value")) {
            var received_data = eval(params);
            all_obj_data = received_data[1];
            events_data = received_data[2];
            DollarValue = received_data[3];
            DollarValueTarget = received_data[4];
            DollarValueOfEnterprise = received_data[5];
            sendCommand("refresh");
        }

        if (IsAction("set_checked_value") || IsAction("set_default_value") || IsAction("delete_enum_attr_value") || IsAction("edit_enum_attr_value") || IsAction("add_enum_attr_value")) {
            selected_alt_data = eval(params)[0];
            refresh_bowtie(canvasID);
        }

        if (IsAction("event_group_data")) {
            var received_data = eval(params);
            var event_id = received_data[1];
            var event_group_name = received_data[2];
            var event_group_precedence = received_data[3];
            eventGroupEditor(event_id, event_group_name, event_group_precedence);
        }

        if (IsAction("get_filters")) {
            var received_data = eval(params);
            savedFilters = received_data[1];
        }

        if (IsAction("add_filter") || IsAction("clear_filters") || IsAction("update_filters")) {
            var received_data = eval(params);
            savedFilters = received_data[1];
            
            initSavedFilters(savedFilters);
            var cb = document.getElementById("cbSavedFilters");
            if ((cb) && (cb.options.length > 0)) {
                cb.selectedIndex = cb.options.length - 1; 
                if (cb.selectedIndex == 0) {
                    initFilterGrid(attr_flt, "0");
                } else {
                    renderFilterUI(savedFilters[cb.selectedIndex - 1][1], savedFilters[cb.selectedIndex - 1][2]);
                }
            }
        }

        if (IsAction("reverse_zoom") || IsAction(ACTION_SET_BUBBLE_SIZE)) {
            InitBowTieAndRiskPlot();
            <%--loadChart('divRiskMap', window["plot1"], plot_data1, RiskPlotMode == 1 ? "<%=ParseString(" With %%Controls%%")%>" : "<%=ParseString(" Without %%Controls%%")%>");
            if (RiskPlotMode == 2) loadChart('divRiskMapWC', window["plot2"], plot_data2, "<%=ParseString(" With %%Controls%%")%>");
            resizeChart();--%>
        }

        if (IsAction("parse_filter")) {
            var received_data = eval(params);
            var data = received_data[1];
            initFilterGrid(data, received_data[2]);
        }
        
        if (IsAction("get_rand_seed")) {
            var received_data = eval(params);
            var data = received_data[1];
            $("#tbRandSeed").val(data);
        }

        updateToolbar();
        updateColorCodingWarning();
        hideLoadingPanel();

        if (IsAction("manual_zoom")) {
            var received_data = eval(params);
            if (received_data[1] == "") sendCommand("refresh");
        }
    }

    function saveNumerOfSimulations(value) {
        if (validInteger(value)) {
            var v = str2int(value);
            if (v !== numSimulations) {
                numSimulations = v;
                sendCommand("action=num_sim&val=" + v);
            }
        }
    }

    function saveRandSeed(value) {
        if (validInteger(value)) {
            var v = str2int(value);
            if (v !== randSeed) {
                randSeed = v;
                sendCommand("action=rand_seed&val=" + v);
            }
        }
    }

    function initToolbar() {
        $("#lblSimMode").html("");
        if (UseControlsReductions) {
            <%If IsRiskWithControlsPage Then %>
            if (ControlsSelectionMode == 0) $("#lblSimMode").html("<br>(<%=ParseString("%%Controls%% are manually selected")%>)");
            if ((ControlsSelectionMode == 1) && (ControlsSimulationMode == 1)) $("#lblSimMode").html("<br>(<%=ParseString("%%Controls%% are optimized based on simulated input and computed output")%>)");
            if ((ControlsSelectionMode == 1) && (ControlsSimulationMode == 2)) $("#lblSimMode").html("<br>(<%=ParseString("%%Controls%% are optimized based on computed input and simulated output")%>)");
            if ((ControlsSelectionMode == 1) && (ControlsSimulationMode == 3)) $("#lblSimMode").html("<br>(<%=ParseString("%%Controls%% are optimized based on simulated input and output")%>)");
            <%End If%>
        }
    }

    function switchSimResults(chk) {        
        UseSimulatedValues = chk;
        //if (ShowResultsMode > 0) { $("#divProbabilityCalculationMode").hide(); } else { $("#divProbabilityCalculationMode").show(); }
        sendCommand("show_sim_results=" + UseSimulatedValues);
    }

    function saveKeepRandSeed(value) {
        keepRandSeed = value;
        sendCommand("action=keep_rand_seed&val=" + value);
    }
    
    function onUseSourceGroups(value) {
        //initSimulationStart();
        sendCommand("action=use_source_groups&value=" + value);
    }

    function onUseEventGroups(value) {
        sendCommand("action=use_event_groups&value=" + value);
    }

    function syncError() {
        hideLoadingPanel();
        dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error");
    }

    function sendAjax(params) {
        cmd = params;
        showLoadingPanel();

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    var on_hit_enter = "";

    function Hotkeys(event) {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);                
            if (code == 13) {
                if (on_hit_enter != "") {                                                
                    eval(on_hit_enter);
                }
                return false;
            }
            if (code == 27 && alt_name >= 0) {
                $("#tbName" + alt_name).val(alt_value);
                switchAltNameEditor(alt_name, -1);
                return false;
            }
        }
    }

    function onEditControlInfodoc(id) {
        OpenRichEditor("?type=16&&guid=" + id);
        /*<% = PopupRichEditor(reObjectType.Control, "guid=' + id + '") %>;*/
    }

    function ChangeImage(obj, img)
    {
        if ((obj) && (img)) obj.src = img.src;
        return false;
    }

    function onRichEditorRefreshQH(empty, infodoc, callback)
    { 
        window.focus();
        ChangeImage(document.getElementById("imgQH"), ((empty=="1") ? img_qh_ : img_qh));
        if ((callback) && (callback.length>1) && (callback[0]!="") && (callback[0])) {
            var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("confQHPreview")) %>", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    setTimeout('$(\"#lnkQHVew\")[0].click();', 350);
                }
            });
        }
    }

    function editQuickHelp(cmd) {
        OpenRichEditor("?type=11&" + cmd);
        /*<% =PopupRichEditor(reObjectType.QuickHelp, QuickHelpParams)%>*/
    }

    function ShowIdleQuickHelp()
    {
        showQuickHelp("<% =JS_SafeString(QuickHelpParams) %>", true, <% =Bool2JS(isPM) %>);
    }

    function InitColorPickers() {
        $("#RadColorPickerHigh").dxColorBox( { acceptCustomValue: true, readOnly: false, visible: true }); 
        $("#RadColorPickerMid").dxColorBox(  { acceptCustomValue: true, readOnly: false, visible: true }); 
        $("#RadColorPickerLow").dxColorBox(  { acceptCustomValue: true, readOnly: false, visible: true }); 
    }
    
    <%If Not IsBowTiePage Then%>function onRichEditorRefresh(empty, infodoc, callback)
    { 
        window.focus();
    }<% End If%>

    onSwitchAdvancedMode = function (value) { 
        updateToolbar();
    };

    document.onkeypress = Hotkeys;

    resize_custom = resizePage;
    
    $(document).ready( function () {
        toggleScrollMain();
        
        showLoadingPanel();
        <% If Checkvar("back", "") <> "" Then %>if (!menu_switch_hover) { toggleSideBarMenu(true, 0, false); }<% End if %>
        <%If IsBowTiePage() Then %>
        initBowTieResources();
        <%End If %>

        <%If (IsRiskWithControlsPage OrElse IsBowTiePage OrElse IsRiskMapPage()) AndAlso Not UseControlsReductions Then %>
        $("#lblWC").hide();
        <%End If%>
        filterByAttributesDialog();
        dlg_attributes.dialog('close');

        removeExtraBorders();

        InitTooltips(); 
        initToolbar(); 

        $(".jqplot-title").css("padding-bottom","0px");                
        updateFilterWarning();
        updateColorCodingWarning();

        <%If IsOptimizationAllowed Then%>
        $('#tbBudgetLimit').on('blur',function(e){
            setTimeout("if ((this.value)) sendAjax('action=edit_budget&value=' + str2cost(this.value));", 500);
        });
        <%End If%>
        
        InitBowTieAndRiskPlot();
        InitColorPickers();            

        updateToolbar();
        <% If IsWidget Then %>
        <% Else %>
        setTimeout("$('#btnOptimize').focus();", 100);
        setTimeout('sendAjax("action=get_filters");', 200);
        <% End if %>


        <% If IsRiskMapPage Then %>
        $("#tdRiskMap").width(0);
        $("#tdColorLegend").width(0);

        var totalWidth = $("#tableRiskMap").innerWidth();

        $("#tdRiskMap").width(totalWidth * 0.7 - 15);
        $("#tdColorLegend").width(totalWidth * 0.3 - 15);        
        <% End If %>

        if (is_ie) $("#btnDownloadLegend").hide();

        //if ($(window.parent.document.getElementById("popupContainer")).data("dxPopup")) {
        //    $(window.parent.document.getElementById("popupContainer")).dxPopup("instance").option("title", $("#divHeader").text());
        //}

        resizePage(); 
    });

</script>
</telerik:RadScriptBlock>

<div id="divRiskResultsMain" class="whole" style="width:100%; height:100%; opacity:0">
<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server" ClientEvents-OnResponseEnd="OnRadAjaxCallback" EnableAJAX="true" EnableHistory="false" EnablePageHeadUpdate="false">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="divGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="divGrid"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<telerik:RadSplitter ID="RadSplitterMain" runat="server" CssClass="whole" Width="600" Height="200" ResizeMode="EndPane" BorderSize="0">

<!-- Tree -->
<telerik:RadPane ID="RadPaneHierarchy" runat="server" CssClass="text" Width="175" MinWidth="150" OnClientResized="OnClientResized" Scrolling="Both" BorderSize="0">
  <telerik:RadAjaxPanel ID="RadAjaxPanelTree" runat="server" Width="100%" Height="98%" ClientEvents-OnResponseEnd="OnPanelTreeLoaded">
    <telerik:RadCodeBlock runat="server" ID="RadCodeBlockTree"><div style='border:0px; padding:0px 0px 0px 5px'> 
<% If CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_GRID OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_LIKELIHOOD_CHART OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_SPECIFIC_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES OrElse CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES OrElse CurrentPageID = _PGID_RISK_PLOT_CAUSES_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES Then%>
<div class="bg_fill"><% =ResString("lblLikelihood")%> Hierarchy</div><%ElseIf IsRiskResultsByEventPage OrElse (IsBowTiePage AndAlso Not IsBowTieByObjectivePage) Then%><div class="bg_fill"><% =ResString("lblRiskResultsEvents")%></div><%Else%><div class="bg_fill"><% =ResString("lblImpact")%> Hierarchy</div><%End If%>
    <% If Not ( CurrentPageID = _PGID_RISK_BOW_TIE OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse IsRiskResultsByEventPage) Then%>
    <div style='text-align:center; margin-top:1ex' class='text'>
        <nobr><input type='checkbox' class='input checkbox' id='ShowLocal' value='1'<% =IIf(ShowLocalPrty, " checked", "") %> onclick='sendTreeViewCommand("local=" + (this.checked)); return false;' /><label for='ShowLocal'>Local (Absolute)</label>&nbsp;&nbsp;|</nobr> &nbsp;
        <nobr><input type='checkbox' class='input checkbox' id='ShowGlobal' value='1'<% =IIf(ShowGlobalPrty, " checked", "") %> onclick='sendTreeViewCommand("global=" + (this.checked)); return false;' /><label for='ShowGlobal'>Global (Absolute)</label>&nbsp;<% If IsRiskWithControlsPage Then%>&nbsp;|</nobr> &nbsp;
        <nobr><input type='checkbox' class='input checkbox' id='cbUseControlsHierarchy' value='1'<% =IIf(UseControlsReductionsHierarchy, " checked", "") %> <% =IIf((Not App.isRiskTreatmentEnabled AndAlso Not App.isRiskTreatmentEnabled) OrElse (Not ShowLocalPrty AndAlso Not ShowGlobalPrty), " disabled", "") %> onclick='sendTreeViewCommand("use_controls=" + (this.checked)); return false;' /><label for='cbUseControlsHierarchy'<% =IIf(Not ShowLocalPrty AndAlso Not ShowGlobalPrty, " disabled", "") %>><% =ResString("lblUseControlsReductions")%></label></nobr><% End If%>
    </div>
    <% End if%>
    <telerik:radtreeview id="RadTreeHierarchy" runat="server" style="overflow: hidden;" AllowNodeEditing="false" Width="99%" EnableDragAndDrop="false" EnableEmbeddedScripts="true" MultipleSelect="false" OnClientNodeClicked="OnSelectedNodeClick">         
        <NodeTemplate><table border='0' cellspacing='0' cellpadding='0' id='tbl<%#DataBinder.Eval(Container, "Value") %>' style='width:<%#130-Cint(DataBinder.Eval(Container, "Level"))*20%>px;'><tr><td class='text' style='color:<%#DataBinder.Eval(Container, "Attributes('Foreground')")%>;font-weight:<%#DataBinder.Eval(Container, "Attributes('FontWeight')")%>;'><%#DataBinder.Eval(Container, "Text")%></td></tr></table></NodeTemplate>
    </telerik:radtreeview>
    </div></telerik:RadCodeBlock>
  </telerik:RadAjaxPanel>
 </telerik:RadPane>
<telerik:RadSplitBar ID="RadSplitRight" runat="server" EnableResize="true" CollapseMode="Forward" />

<!-- Grid -->
<telerik:RadPane ID="RadPaneGrid" runat="server" CssClass="text whole" MinWidth="275" Scrolling="None" BorderSize="0">
<div style="margin:0px 0px;" class="whole">

<div id='divGrid' runat='server' style='text-align:center;'>
<telerik:RadCodeBlock runat="server" ID="RadCodeBlockGrid">
    <center><table id='tblRiskResultsMain' border='0' cellspacing='0' cellpadding='0' style='text-align:center; width:99%; padding-left:1px;'><tr class='text' align="left">
        <td id='tdContent'>
            <%--<div id='divToolbar' class='text ec_toolbar' style="margin:0px 0px;" ondblclick="onToolbarDblClick();">--%>
            <% If IsWidget Then %><% =GetQuickHelpIcons() %><% End if%>
            <div id='divToolbar' class="titles">
            <% If Not IsWidget And Not IsRiskResultsByEventPage AndAlso CanEditActiveProject AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace) Then%>
            <table id='tblToolbar' border='0' cellspacing='0' cellpadding='2' style='text-align:center; width:100%; padding:2px 4px 4px 4px; margin:2px 0px 2px 0px; background:#fdfdfd;'>
            <tr class='text' align="left">                    
            <td class="ec_toolbar_td_separator" style="line-height: 2.2;">
            <% If Not App.Options.isSingleModeEvaluation AndAlso Not IsWidget Then%>
            <% If Not IsWidget AndAlso (CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS OrElse CurrentPageID = _PGID_RISK_PLOT_OVERALL) AndAlso CheckVar("back", "") <> "" Then %><% =GetQuickHelpIcons() %><% End if%>
                <%If Not (App.Options.isSingleModeEvaluation OrElse Not App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace)) Then %>
                    <% If IsRiskMapPage() OrElse IsBowTiePage() Then%>
                    <div style="display:inline-block; text-align:left;">
                        <span>Participant or Group:</span>
                        <% = GetSAParticipants() %>
                    </div>    
                    <% Else %>
                        <a href='' onclick='selectUsersDialog(); return false;'><i class="fas fa-users fa-lg"></i>&nbsp;<%=ResString("btnParticipantsAndGroups")%></a>
                    <% End If %>
                <% End If %>                
                <% If IsRiskMapPage() OrElse IsBowTiePage() Then%>                
                <a href='' id='btnEditRegions' runat="server" style="cursor: pointer;" onclick='ShowRegionsDialog(); return false;'><i class="fas fa-th fa-lg" title='<%=ResString("lblRiskRegions") %>'></i>&nbsp;Regions&hellip;</a>
                <%End If%>
                <% If Not IsRiskMapPage() AndAlso Not App.Options.isSingleModeEvaluation Then%>                
                <a href='' id='btnExport' style="cursor:pointer;" runat="server" onclick='document.location.href = document.location.href + "&action=download"; return false;'><i id='imgExport' class='fas fa-file-export fa-lg' title='<%=ResString("btnExport") %>'></i>&nbsp;Export&nbsp;&nbsp;</a>
                <% End If %>
                <%--<% If IsRiskMapPage() Then%>
                <a href='' id='btnEventCloud' style="cursor:pointer;" onclick='isEventCloud = !isEventCloud; updateToolbar(); sendCommand("action=event_cloud&value=" + (isEventCloud)); return false;' title="<%=ParseString("%%Alternative%%") + " cloud"%>"><img id='btnEventCloudImg' class='toggle_btn_<%=IIf(RiskMapEventCloud, "checked", "unchecked")%>' style='vertical-align:top; margin-top:2px;' src='../../images/risk/eventcloud_<%=IIf(RiskMapEventCloud, "checked", "unchecked")%>.png' alt='<%=ParseString("%%Alternative%%") + " cloud"%>' /></a>
                <%End If%>--%>
                <% If Not IsRiskResultsByEventPage Then %>
                    <a href="" id='btnRiskExtraPlot' onclick="showExtraPlot(); return false;" title='<%=ResString(CStr(IIf(PRJ.isOpportunityModel, "titleRiskExtraPlotOpp", "titleRiskExtraPlot")))%>'><i id='imgRiskExtraPlot' class='fas fa-chart-area fa-lg'></i>&nbsp;Loss Exceedance&hellip;</a>
                <% End If %>
                <% If PM.ActiveAlternatives.Nodes.Count > 0 Then %>
                <a href="" id="btnSelEvents" onclick="onSelectEventsClick(); return false;" style="cursor: pointer;"><img id="imgFilterEvents" src="<% = ImagePath %>filter20.png" width=20 height=20 style="vertical-align: top;" />&nbsp;<% = ParseString("Select %%Alternatives%%&hellip;")%></a>
                <% End If %>
                    <% If IsOptimizationAllowed AndAlso PM.Controls.Controls.Count > 0 Then%><a href="" onclick="onSelectControlsClick(); return false;"><img id="imgFilterControls" src="../../images/ra/filter20.png" style="vertical-align: text-bottom;" /><%=ParseString("Available %%Controls%%&hellip;")%></a><% End If%>
                    <%--<a href='' onclick="onSelectEventsClick(); return false;" style="margin: 4px; margin-left: 6px;"><i id="imgFilterEvents" class="fas fa-filter fa-lg"></i></a>--%>
                <% If IsBowTiePage Then %><input type='checkbox' id='cbShowResults' value='1'<% =If(CanShowAllPriorities, " checked", "") %> onclick='CanShowAllPriorities = this.checked; sendCommand("show_bowtie_results=" + (this.checked));' /><label for='cbShowResults'><%=ResString("lblRiskShowResults")%></label><% End If %>
                <input id="cbSimulatedSwitch" type="checkbox" <%=If(PM.CalculationsManager.UseSimulatedValues, " checked='checked' ", "") %> onclick="switchSimResults(this.checked); return false;"><label for="cbSimulatedSwitch">Simulated Results</label>&nbsp;
                <input type='checkbox' class='use-shuffling' id='cbUseShuffling' <% =If(PM.RiskSimulations.UseShuffling, " checked ", "") %>  onclick='sendCommand("use_shuffling=" + (this.checked));' /><label for='cbUseShuffling' class="use-shuffling">Use Shuffling</label>&nbsp;
            <% End If %>                
            <%--</td>
            <td class="ec_toolbar_td">  --%>                      
            <%If IsRiskMapPage() Then%>
                <% If ActiveProjectHasAlternativeAttributes AndAlso Not IsRiskMapPage() Then%>
                <input type='checkbox' class='checkbox' id='cbShowAttributes' <% = If(ShowAttributes, " checked", "") %> onclick='ShowAttributes(this.checked);' /><label for='cbShowAttributes' style='color:<% = If(IsGroupByAttribute, "#999999", "black") %>'><%=ResString("lblFilterByAltAttr")%></label><a href='' id='btnFilterSettings' style="display: inline-block; vertical-align: middle;" onclick='filterByAttributesDialog(); return false;'>&nbsp;<i id='imgFilterSettings' class="fas fa-sliders-h fa-lg"></i></a>&nbsp;
                <% End If%>
            <% End If%>                        
            <% If (App.isRiskTreatmentEnabled AndAlso Not IsRiskMapPage()) AndAlso IsRiskWithControlsPage Then%>
            <nobr><input type='checkbox' class='input checkbox' id='cbUseControlsReductions' value='1' <% =If(UseControlsReductions And Treatments.Count > 0, " checked", "")%> <% =If(Treatments.Count > 0, "", "disabled='disabled'")%> onclick='UseControlsReductions = this.checked; if (this.checked) {$("#divSolverPane").show(); $("#trResults").show();} else {$("#divSolverPane").hide(); $("#trResults").hide();}; sendCommand("use_controls=" + (this.checked));' /><label for='cbUseControlsReductions'><% =ResString("lblUseControlsReductions")%></label></nobr>&nbsp;
            <% End If%>                        
            <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
                <% If CheckedUsersCount() > 1 Then%>
                <nobr><input type='checkbox' class='checkbox' id='cbWorstCase' value='1'<% =iif(IsWorstCaseVisible, " checked", "") %> onclick='sendCommand("worst_case=" + (this.checked));' /><label for='cbWorstCase'><%=ResString("lblRiskMaxOption")%></label>&nbsp;</nobr>            
                <% End If%>
            <% End If%>            
            <input type='checkbox' class='checkbox' id='cbDollarValue' <%=IIf(ShowDollarValue, " checked", "") %> <%=If(DollarValue = UNDEFINED_INTEGER_VALUE OrElse DollarValueTarget = UNDEFINED_STRING_VALUE, " disabled='disabled' ", "") %> onclick='ShowDollarValueClick(this.checked);' /><label for='cbDollarValue' id='lblDollarValue' style='color:<%=IIf(DollarValue = UNDEFINED_INTEGER_VALUE, "#999999", "black") %>'><%=ResString("lblShowDollarValue") + GetDollarValueFullString()%></label>
            <i style='vertical-align:middle; cursor:pointer;' class='fas fa-pen toolbar-icon' title='Edit Value of Enterprise' onclick='onEditDollarValueClick(); return false;'></i>            
            <%--<% If Not IsRiskMapPage() AndAlso Not App.Options.isSingleModeEvaluation Then%>
                <nobr><asp:Button runat="server" CssClass="button" ID="btnExport" Text="Export" OnClientClick='CreatePopup(document.location.href + "&action=download", "export", ""); return false;'/>&nbsp;</nobr>
            <% End If %>--%>                                    
            <% If IsBowTiePage() AndAlso Not isIE(Request) Then%>
                <a download="project.jpeg" id="downloadLnk" style='color:#0000ff;'><u>Save as image</u></a>
            <%End If%>
                <% If IsRiskMapPage() Then%>
                <nobr id="mnuZoomRiskPlot">
                    <input type='checkbox' class='input checkbox' id='cbZoomRiskPlot' <% =IIf(ZoomPlot, " checked ", "") %>  onclick='ZoomPlot = this.checked; sendCommand("zoom_plot=" + (this.checked));' /><label for='cbZoomRiskPlot'><%=ResString("lblRiskZoomPlot")%></label>&nbsp;
                </nobr>
                <%If Not IsWidget Then%>
                <nobr><input type='checkbox' class='input checkbox' id='cbBubbleSize' <% =iif(RiskMapBubbleSizeWithRisk, " checked ", "") %> onclick='RiskMapBubbleSizeWithRisk = this.checked; sendCommand("risk_bubble_size="  + (this.checked));' /><label for='cbBubbleSize'><%=ResString("lblRiskBubbleSize")%></label>&nbsp;</nobr>
                <%End If%>
                <nobr id="mnuDatapointSize"><label><%=ResString("lblBubbleSlider")%>:</label>&nbsp;
                <select class='select' id='sliderBubbleSize' style='width:8em;' onchange='sliderBubbleSizeChange(this.value);'>
                    <option value='2'  <% = IIf(RiskMapBubbleSize = 2, "selected='selected'", "") %>>2</option>
                    <option value='3'  <% = IIf(RiskMapBubbleSize = 3, "selected='selected'", "") %>>3</option>
                    <option value='4'  <% = IIf(RiskMapBubbleSize = 4, "selected='selected'", "") %>>4</option>
                    <option value='5'  <% = IIf(RiskMapBubbleSize = 5, "selected='selected'", "") %>>5</option>
                    <option value='6'  <% = IIf(RiskMapBubbleSize = 6, "selected='selected'", "") %>>6</option>
                    <option value='7'  <% = IIf(RiskMapBubbleSize = 7, "selected='selected'", "") %>>7</option>
                    <option value='8'  <% = IIf(RiskMapBubbleSize = 8, "selected='selected'", "") %>>8</option>
                    <option value='9'  <% = IIf(RiskMapBubbleSize = 9, "selected='selected'", "") %>>9</option>
                    <option value='10'  <% = IIf(RiskMapBubbleSize = 10, "selected='selected'", "") %>>10</option>
                    <option value='11'  <% = IIf(RiskMapBubbleSize = 11, "selected='selected'", "") %>>11</option>
                    <option value='12'  <% = IIf(RiskMapBubbleSize = 12, "selected='selected'", "") %>>12</option>
                    <option value='13'  <% = IIf(RiskMapBubbleSize = 13, "selected='selected'", "") %>>13</option>
                    <option value='14'  <% = IIf(RiskMapBubbleSize = 14, "selected='selected'", "") %>>14</option>
                    <option value='15'  <% = IIf(RiskMapBubbleSize = 15, "selected='selected'", "") %>>15</option>
                    <option value='20'  <% = IIf(RiskMapBubbleSize = 20, "selected='selected'", "") %>>20</option>
                    <option value='25'  <% = IIf(RiskMapBubbleSize = 25, "selected='selected'", "") %>>25</option>
                    <option value='40'  <% = IIf(RiskMapBubbleSize = 40, "selected='selected'", "") %>>40</option>
                    <option value='50' <% = IIf(RiskMapBubbleSize = 50, "selected='selected'", "") %>>50</option>
                    <option value='75' <% = IIf(RiskMapBubbleSize = 75, "selected='selected'", "") %>>75</option>
                    <option value='100' <% = IIf(RiskMapBubbleSize = 100, "selected='selected'", "") %>>100</option>
                    <option value='250' <% = IIf(RiskMapBubbleSize = 250, "selected='selected'", "") %>>250</option>
                    <option value='500' <% = IIf(RiskMapBubbleSize = 500, "selected='selected'", "") %>>500</option>
                </select>
                </nobr>                
            <%End If%>
                <% If IsRiskMapPage() AndAlso AttributesList.Count > 0 Then%>
                <fieldset class="legend" style="text-align:left; vertical-align:top; margin: 7px 0px; line-height: 0;" id="pnlRiskMapAttributes">
                    <legend class="text legend_title">&nbsp;Attributes&nbsp;</legend>
                    <input type='checkbox' class='checkbox' id='cbShowAttributesRiskMap' <%=IIf(ShowAttributes, " checked", "") %> <%=IIf(Not IsRiskMapPage() AndAlso IsGroupByAttribute, " disabled ", "") %> onclick='ShowAttributes(this.checked);' /><label for='cbShowAttributesRiskMap' style='color:<%=IIf(Not IsRiskMapPage() AndAlso IsGroupByAttribute, "#999999", "black") %>'><%=ResString("lblFilterByAltAttr")%></label><a href='' id='btnFilterSettingsRiskMap' onclick='filterByAttributesDialog(); return false;' style='vertical-align:middle;display:inline-block;'>&nbsp;<i id='imgFilterSettingsRiskMap' style='vertical-align:middle;margin-top:2px;' class="fas fa-sliders-h fa-lg"></i></a>&nbsp;
                    <label><input type="checkbox" class='checkbox' id='cbColorBubbles' <% =IIf(ColorCodingByCategory, " checked='checked' ","") %> onclick='ShowColorLegend = this.checked; onSetColoringByCategory(this.checked);' style='margin-top:3px;'><%=ResString("lblRiskColorBubbles")%>:&nbsp;</label><% =GetCategories(120)%>&nbsp;&nbsp;
                    <label><input type="checkbox" class='checkbox' id='cbShapeBubbles' <%=IIf(AttributesList.Count=0, " disabled='disabled' ","") %>  <% =IIf(ShapeBubblesByCategory AndAlso AttributesList.Count > 0, " checked='checked' ","") %> onclick='ShowShapeLegend = this.checked;    onSetShapeByCategory(this.checked);' style='margin-top:3px;'><%=ResString("lblRiskShapeBubbles")%>:&nbsp;</label><% =GetCategoriesShapes(120)%>
                </fieldset>
            <%End If%>
                    </td>
                <td style="text-align: right; white-space: nowrap; line-height: 2.2;" valign="top">
                    <a href='' id='btnSettings' onclick='btnSettingsClick(); return false;'><i id='imgSettings' class="fas fa-cog fa-lg"></i>&nbsp;Preferences&hellip;</a>
                </td>
                </tr>
            </table>
                <% End If %>
            </div>
                    
        <div id="divTitles" class="titles">
        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        <div id="divFilterWarning" class="note" style="display: none; margin-top: 4px;"></div>
        <% End If%>

        <!-- Page Title -->
        <h5 id="divHeader" runat="server" style="margin:3px;"></h5><% If IsWidget AndAlso CurrentPageID = _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS AndAlso PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank Then%><% =GetScore() %><% End If %>
        <div runat='server' id='divFilterData' style='display:none;'></div>
        <div runat='server' id='divNodesDataWithDollars' style='display:none;'></div>

        <!-- Risk Optimizer -->
        <%If IsOptimizationAllowed AndAlso ShowSolverPane And False Then %>
        <div id="divSolverPane" style="display:<%=IIf(UseControlsReductions, "", "none")%>;">
        <center>
        <div id="divSolverMode" class="ec_toolbar_tabcontrol text" style="display:inline-block; margin: 0px 0px 5px 0px; float:left;">
            <div class="ec_toolbar_tabs" style="display:inline-block; width:320px;">
                <label><input type="radio" name="rbOptimizerTabs" <%=CStr(IIf(CInt(OptimizationTypeAttribute) = 0, " checked='checked'", ""))%> onclick="openOptimizerTab(event, 0)" title="<%=ResString("msgBudgetLimitHint")%>" /><%=ResString("optROBudgetLimit")%></label>&nbsp;&nbsp;&nbsp;
                <label><input type="radio" name="rbOptimizerTabs" <%=CStr(IIf(CInt(OptimizationTypeAttribute) = 1, " checked='checked'", ""))%> onclick="openOptimizerTab(event, 1)" title="<%=ResString("msgMaxRiskHint")%>" /><%=ResString("optROMaxRisk")%></label>&nbsp;&nbsp;&nbsp;
                <label><input type="radio" name="rbOptimizerTabs" <%=CStr(IIf(CInt(OptimizationTypeAttribute) = 2, " checked='checked'", ""))%> onclick="openOptimizerTab(event, 2)" title="<%=ResString("msgMinReductionHint")%>" /><%=ResString("optROMinReduction")%></label>
            </div>
            <div class="ec_toolbar_tabcontent" id="tabContent0" <%=CStr(IIf(CInt(OptimizationTypeAttribute) = 0, "style='display:block'", "style='display:none'"))%>>
                <nobr><span><%=ResString("lblROBudgetLimit")%>:</span>&nbsp;<%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%>       
                <input type="text" id="tbBudgetLimit" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=IIf(PM.Parameters.Riskion_ControlsActualSelectionMode = 0, "", CostString(RA.RiskOptimizer.BudgetLimit))%>" style="width:140px; height:20px; margin-top:2px; text-align:right;" />&nbsp;</nobr>
            </div>                        
            <div class="ec_toolbar_tabcontent" id="tabContent1" <%=CStr(IIf(CInt(OptimizationTypeAttribute) = 1, "style='display:block'", "style='display:none'"))%>>
                <nobr><span><%=ResString("lblROMaxRisk")%>:</span>&nbsp;<span id="lblMaxRiskDollSign" <%=CStr(IIF(ShowDollarValue, "", " style='display:none;'")) %>><%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%></span>
                <input type="number" id="tbMaxRisk" class="input number" step='any' autocomplete="off" value="<%=IIf(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.MaxRisk * DollarValueOfEnterprise), RA.RiskOptimizer.CurrentScenario.MaxRisk * 100)%>" style="width:80px; height:20px; margin-top:2px; text-align:right;" /><span id="lblMaxRiskPrcSign" <%=CStr(IIF(ShowDollarValue, " style='display:none;'", "")) %>>&#37;</span>&nbsp;</nobr>
            </div>                        
            <div class="ec_toolbar_tabcontent" id="tabContent2" <%=CStr(IIf(CInt(OptimizationTypeAttribute) = 2, "style='display:block'", "style='display:none'"))%>>
                <nobr><span><%=ResString("lblROMinReduction")%>:</span>&nbsp;<span id="lblMinRiskDollSign" <%=CStr(IIF(ShowDollarValue, "", " style='display:none;'")) %>><%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%></span>
                <input type="number" id="tbMinReduction" class="input number" step='any' autocomplete="off" value="<%=IIf(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.MinReduction * DollarValueOfEnterprise), RA.RiskOptimizer.CurrentScenario.MinReduction * 100)%>" style="width:80px; height:20px; margin-top:2px; text-align:right;" /><span id="lblMinRiskPrcSign" <%=CStr(IIF(ShowDollarValue, " style='display:none;'", "")) %>>&#37;</span>&nbsp;</nobr>
            </div>
        </div>        
        
        <button id="btnOptimize" onclick="this.disabled = true; callSolve(); return false;" class='button' type="button" style="float:left; margin:12px 12px 12px 12px; width:205px; height:25px; vertical-align:top;"><img src="../../images/ra/assembly-20.png" width=16 height=16 style="vertical-align:middle; padding-bottom:0px;" /> <%=ParseString("Optimize %%controls%% (Solve)")%></button>

        <div id='divSolverResults' class='text' style=" margin: 6px 0px 6px 0px;">            
            <table class="text" cellpadding="0" cellspacing="0">
                <tr>
                    <td align="right"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ResString("lblRiskTotal")%>:&nbsp;</b></span></nobr></td>
                    <td align="left"><nobr><span class="number_result" id="tbRiskTotal"><%=IIf(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * DollarValueOfEnterprise, , True), (RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * 100).ToString("F2") + "%")%></span></nobr></td>
                    <td align="right"><span id="lblActiveCountLabel" style="display:inline-block;width:210px;text-align:right;"><b>Selected <% =ParseString("%%controls%%") %>:&nbsp;</b></span></td>
                    <td align="left"><span class="number_result" id="lblActiveCount"><%=RA.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCount%></span></td>
                    <%--<td><%If Not IsOptimizationAllowed Then %><span style="display:inline-block;width:100px;text-align:right;">Select:&nbsp;</span><a href='#' onclick='activateAll(event, "all", 1); return false;' class='actions'><%=ResString("lblAll")%></a>&nbsp;|&nbsp;<a href='#' onclick='activateAll(event, "all",0); return false;' class='actions'><%=ResString("lblNone")%></a><%End If%></td>--%>
                </tr>
                <tr>
                    <td align="right"><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("%%Risk%% With Selected %%Controls%%")%>:&nbsp;</b></span></td>
                    <td align="left"><span class="number_result" id="tbOptimizedRisk"><%=getInitialOptimizedRisk()%></span></td>                    
                    <td align="right"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("Cost Of Selected %%Controls%%")%>:&nbsp;</b></span></nobr></td>
                    <td align="left"><nobr><span class="number_result" id="tbFundedCost"><%=CostString(RA.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, , True) + DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, RA.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True, ResString("lblUnfunded") + ":")%></span></nobr></td>                    
                    <%--<td></td>--%>
                </tr>
                <tr>
                    <td align="right"><nobr><%If IsOptimizationAllowed Then %><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("%%Risk%% With All %%Controls%%")%>:&nbsp;</b></span><%End If %></nobr></td>
                    <td align="left"><nobr><%If IsOptimizationAllowed Then %><span class="number_result" id="tbRiskTotalWithControls"><%=IIf(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * DollarValueOfEnterprise, , True) + DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * DollarValueOfEnterprise, RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * DollarValueOfEnterprise, CostDecimalDigits, ShowDollarValue), (RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * 100).ToString("F2") + "%" + DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue, RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls, CostDecimalDigits, ShowDollarValue))%></span><%End If %></nobr></td>
                    <td align="right"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("Total Cost Of All %%Controls%%")%>:&nbsp;</b></span></nobr></td>
                    <td align="left"><nobr><span class="number_result" id="tbAllControlsCost"><%=CostString(RA.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, , True)%></span></nobr></td>
                    <%--<td></td>--%>
                </tr>
                <%If IsOptimizationAllowed Then %>
                <tr> <!-- Risk Reduction Row -->
                    <td align="right"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ResString("lblRiskReduction")%>:&nbsp;</b></span></nobr></td>
                    <td align="left"><nobr><span class="number_result" id="tbTotalRiskReduction" style="white-space: nowrap;"><%=IIf(ShowDollarValue, CostString(GetCurRiskReduction(True), CostDecimalDigits, True), (GetCurRiskReduction(False) * 100).ToString("F2") + "%")%></span></nobr></td>
                    <td></td>
                    <td></td>
                </tr>
                <%End If %>
            </table>                                
        </div><br/>
        </center>
        </div>
        <%End If %>

        <!-- Attributes -->
        <div id="divAttributesFilter" style="display:none;">
        <center>
            <div style="display: none; /*inline-block;*/ border: 1px solid #ccc; background-color: #ccecce; padding: 4px; margin: 10px 15px 15px 15px; white-space: nowrap;">
                Saved filters:<select id="cbSavedFilters" style="width:120px;" onchange="onSavedFilterChanged();">
                    <option value=''> </option>                    
                </select>
                <button type="button" class="button" style='width: 5ex; height: 24px;' id='btnAddSavedFilter' onclick="onAddSavedFilterClick();"><i class="fas fa-plus"></i></button>
                <button type="button" class="button" style='width: 5ex; height: 24px;' id='btnUpdateSavedFilter' onclick="onUpdateSavedFilterClick();"><i class="fas fa-save"></i></button>
                <button type="button" class="button" style='width:5ex; height: 24px;' id='btnRemoveSavedFilter' onclick="onRemoveSavedFilterClick();"><i class="fas fa-minus"></i></button>
                <button type="button" class="button" style='width:5ex; height: 24px;' id='btnClearSavedFilters' onclick="onClearSavedFiltersClick();"><i class="fas fa-eraser"></i></button>
            </div>
            <fieldset class="legend" style="text-align:center; vertical-align:top; width:600px; height:100%;" id="pnlEventAttributes">
            <legend class="text legend_title">&nbsp;<%=ResString("lblAttributes")%>&nbsp;</legend>
            
            <!-- Filter combinations toolbar -->
            <div style='text-align:center; margin-top:1ex' class='text'>
                <nobr>Use: <select id="cbFilterCombination">
                    <option value='0'<% =iif(FilterCombination=0, " selected", "") %>>AND</option>
                    <option value='1'<% =iif(FilterCombination=1, " selected", "") %>>OR</option>                
                </select> &nbsp;          
                <input type='button' class='button' style='width: 11ex; height: 24px;' id='btnAddRule' value='Add&nbsp;Rule' onclick='onAddNewRule(); return false;' />
                <%--<input type='button' class='button' style='width:11ex' id='btnApplyFilter' value='Apply' onclick='ApplyFilter(false); return false;' />--%>
                <input type='button' class='button' style='width: 11ex; height: 24px;' id='btnRest' value='Reset' onclick='onFilterReset(); return false;' />
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                </nobr>
            </div>

            <p align="center" id='divFilters'>Loading...</p>
        </fieldset></center>
       </div>

        <% If Not IsWidget And Not IsRiskResultsByEventPage AndAlso CanEditActiveProject AndAlso App.CanUserModifyProject(App.ActiveUser.UserID, App.ProjectID, App.ActiveUserWorkgroup, App.ActiveWorkspace) Then%>
        <% If IsRiskMapPage() Then%>
            <div style="text-align: center; margin: 10px;">
                <% If PRJ.RiskionProjectType = ProjectType.ptMixed OrElse PRJ.RiskionProjectType = ProjectType.ptMyRiskReward Then%>
                <nobr><input type='radio' class='input radio' name="rbRiskEventTypes" id='rbEventTypeRisk' value='0' <% = If(RiskPlotEventType = 0, " checked='checked'", "")%> onclick='RiskPlotEventType = this.value * 1; sendCommand("riskplot_event_type=" + this.value);' /><label for='rbEventTypeRisk'><% = ParseString("%%Risks%%") %></label>&nbsp;</nobr>
                <nobr><input type='radio' class='input radio' name="rbRiskEventTypes" id='rbEventTypeOpportunity' value='1' <% = If(RiskPlotEventType = 1, " checked='checked'", "")%> onclick='RiskPlotEventType = this.value * 1; sendCommand("riskplot_event_type=" + this.value);' /><label for='rbEventTypeOpportunity'><% = ParseString("%%Opportunities%%") %></label>&nbsp;</nobr>
                <% End If%>
                <% If App.isRiskTreatmentEnabled Then%>
                <% If PRJ.RiskionProjectType = ProjectType.ptMixed OrElse PRJ.RiskionProjectType = ProjectType.ptMyRiskReward Then%>
                <span style="display: inline-block; margin: 0px 20px; color: #ccc;">|</span>
                <% End If%>
                <% If IsRiskMapWithControlsPage() Then%>
                <nobr><input type='radio' class='input radio' name="rbRiskPlotModes" id='rbRiskPlotShowWithoutControls' value='0' <% =IIf(RiskPlotMode = 0, " checked='checked'", "")%> onclick='RiskPlotMode = this.value * 1; sendCommand("riskplot_controls_mode=" + this.value);' /><label for='rbRiskPlotShowWithoutControls'><%=ParseString("W/o %%Controls%%")%></label>&nbsp;</nobr>
                <nobr><input type='radio' class='input radio' name="rbRiskPlotModes" id='rbRiskPlotShowWithControls' value='1' <% =IIf(RiskPlotMode = 1, " checked='checked'", "")%> onclick='RiskPlotMode = this.value * 1; sendCommand("riskplot_controls_mode=" + this.value);' <%=If(Treatments.Count = 0, " disabled='disabled' ", "") %> /><label for='rbRiskPlotShowWithControls'><%=ParseString("With %%Controls%%")%></label>&nbsp;</nobr>                
                <nobr><input type='radio' class='input radio' name="rbRiskPlotModes" id='rbRiskPlotShowSplit' value='2' <% =IIf(RiskPlotMode = 2, " checked='checked'", "")%> onclick='RiskPlotMode = this.value * 1; sendCommand("riskplot_controls_mode=" + this.value);' <%=If(Treatments.Count = 0, " disabled='disabled' ", "") %> /><label for='rbRiskPlotShowSplit'><%=ParseString("Both - Split")%></label>&nbsp;</nobr>
                <nobr><input type='radio' class='input radio' name="rbRiskPlotModes" id='rbRiskPlotShowBoth' value='3' <% =IIf(RiskPlotMode = 3, " checked='checked'", "")%> onclick='RiskPlotMode = this.value * 1; sendCommand("riskplot_controls_mode=" + this.value);' <%=If(Treatments.Count = 0, " disabled='disabled' ", "") %> /><label for='rbRiskPlotShowBoth'><%=ParseString("Both - Combine")%></label>&nbsp;</nobr>
                <% End If%>
                <% End If%>
            </div>
        <% End If%>            
        <% End If%>            

        <%If PagesWithControlsList.Contains(CurrentPageID) Then%>
        <div id="trResults" class="titles" style="text-align: center; width: 100%;">            
            <div id='divResults' class='text' style="margin: 0px 0px 6px 0px; text-align: center; <% =If(UseControlsReductions And Treatments.Count > 0, " display:block;", " display:none;")%>">            
                <table class="text" cellpadding="0" cellspacing="0" style="width: 100%;">
                    <tr>
                        <td align="right"><span id="lblActiveCountLabel" style="display:inline-block;text-align:right;white-space:nowrap;"><b>Selected <% =ParseString("%%controls%%") %>:&nbsp;</b></span></td>
                        <td align="left"><span class="number_result" id="lblActiveCount" style="color:darkorange;"><%=PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCount%></span></td>
                        <td align="right"><span style="display:inline-block;padding-left:10px;white-space:nowrap;text-align:right;"><b><%=ParseString("Cost Of Selected %%Controls%%")%>:&nbsp;</b></span></td>
                        <td align="left"><span class="number_result" id="tbFundedCost" style="white-space: nowrap;"><%=CostString(PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True) + DeltaString(PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True, ResString("lblUnfunded") + ":")%></span></td>
                        <td align="right"><span style="display:inline-block;padding-left:10px;white-space:nowrap;text-align:right;"><b><%=ParseString("Total Cost Of All %%Controls%%")%>:&nbsp;</b></span></td>
                        <td align="left"><span class="number_result" id="tbAllControlsCost" style="white-space: nowrap;"><%=CostString(PM.ResourceAligner.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, CostDecimalDigits, True)%></span></td>
                        <td align="right"><span style="display:inline-block;padding-left:10px;white-space:nowrap;text-align:right;"><b><%=ParseString("How Selected")%>:&nbsp;</b></span></td>
                        <td style="text-align: left;"><span class="number_result"><% = HowControlsSelected(PRJ, AddressOf ParseString) %></span></td>
                    </tr>
                </table>                                   
            </div>
        </div>
        <%End If%>

        </div>


        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        <div id="GridViewContainer" style="width:100%; overflow:auto;">
            <asp:GridView EnableViewState="false" AllowSorting="true" AllowPaging="false" ID="GridResults" runat="server" BorderWidth="1" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" TabIndex="1" HorizontalAlign="Center" ShowFooter="true">
                <RowStyle VerticalAlign="Middle" CssClass="text grid_row"/>
                <HeaderStyle CssClass="text grid_header actions" />
                <%--<AlternatingRowStyle CssClass="text grid_row_alt_dark" />--%>
                <AlternatingRowStyle CssClass="text grid_row" />
                <FooterStyle VerticalAlign="Middle" CssClass="text grid_row" Font-Bold="true" />
                <EmptyDataTemplate><h6 style='margin:8em 2em'><nobr><% =GetEmptyMessage()%></nobr></h6></EmptyDataTemplate>
            </asp:GridView>
            <% If Not {_PGID_ANALYSIS_RISK_RESULTS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_ALL_OBJS, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_CAUSES, _PGID_ANALYSIS_RISK_RESULTS_WITH_CONTROLS_ALL_OBJS}.Contains(CurrentPageID) Then %>
            <center><div class="text small" style="margin-top: 3px;">
                <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_likelihood'/>&nbsp;<% =ResString("lblLikelihood")%>&nbsp;(<% = ResString("lblLikelihood")(0)%>)&nbsp; &nbsp;
                <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_impact'>&nbsp;<% =ResString("lblImpact")%>&nbsp;(<% = ResString("lblImpact")(0)%>)&nbsp; &nbsp;
                <%If Not IsRiskResultsByEventPage AndAlso PRJ.RiskionProjectType <> ProjectType.ptOpportunities Then%>
                <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_risk' >&nbsp;<% =ResString("lblRisk")%>&nbsp;(<% = ResString("lblRisk")(0)%>)
                <%--<% If IsWorstCaseVisible Then%>&nbsp; &nbsp;<span class='progress' style='width:10px; height:10px;'><span class='graph_worst_case'><img src='<% =ThemePath + _FILE_IMAGE_BLANK %>' width='8' height='8' title='' border='0'/></span></span>&nbsp;Worst Avg. <% End If%>--%>
                <%End If%>
                 <%If PRJ.isMixedModel OrElse PRJ.isOpportunityModel OrElse PRJ.isMyRiskRewardModel Then%>
                <img src='../../images/risk/legend-10.png' width='10' height='10' title='' border='0' class='graph_opportunity' >&nbsp;<% =ResString("lblOpportunityModel")%>&nbsp;(<% = ResString("lblOpportunityModel")(0)%>)
                <%End If%>
            </div></center>
            <% End If %>
        </div>
        <% End If%>

        <%-- Risk Map (Plot) --%>
        <% If IsRiskMapPage() Then%>
            <div runat='server' id='DivRiskMapData' style='display: none;'></div>                        
            <table id="tableRiskMap" cellpadding='0' cellspacing='0' border='0' class="whole"><tr>
            <td id='tdRiskMap' valign='top' align='left' style='white-space: nowrap;'>
                <%--<div id="divRiskMapContainer">--%>
                <div id='divRiskMap' class='plot' style='width:400px; height:450px; display: inline-block; margin-left: 30px; background-color: white;'>
                    <center><p style="margin-top:6em;"><%=ResString("lblPleaseWait")%></p></center>
                </div>
                <div id='divRiskMapWC' class='plot' style='display: inline-block; width:400px; height:450px; background-color: white;'>
                    <center><p style="margin-top:6em;"><%=ResString("lblPleaseWait")%></p></center>
                </div>
                <%--</div>--%>
                <%--<div id='divEventCloud' class='cloud' style='margin-top:2px; margin-left:0px; width:800px; height:450px; overflow:hidden; display:none;'>
                    <canvas id="canvasEventCloud" width="920" height="620" style="margin:2px; background-color:#c4c4c4;"></canvas>
                </div>--%>
                <asp:Button runat="server" CssClass="button" Width="100" ID="btnResetZoom" Text="reset zoom" OnClientClick='resetZoom(); return false;' style="display:none;position:absolute;" />
            </td>
            <td id='tdColorLegend' class="whole"; valign='top' style='display:none;'>
                <div id="divRiskMapModes" class="text" style="background-color: #f1f1f1; padding: 3px; width: 100%;">                
                    <input type='radio' name="rbShowRegions" class='input checkbox' id='cbShowRegions0' <% =iif(RiskMapShowRegions = 0, " checked ", "") %> onclick='setRiskMapRegiosnMode(0);' /><label for='cbShowRegions0'><%=ParseString("Hide Regions")%></label>&nbsp;<br/>
                    <input type='radio' name="rbShowRegions" class='input checkbox' id='cbShowRegions1' <% =iif(RiskMapShowRegions = 1, " checked ", "") %> onclick='setRiskMapRegiosnMode(1);' /><label for='cbShowRegions1'><%=ResString("lblRiskShowRegions")%></label>&nbsp;<br/>
                    <input type='radio' name="rbShowRegions" class='input checkbox' id='cbShowRegions2' <% =iif(RiskMapShowRegions = 2, " checked ", "") %> onclick='setRiskMapRegiosnMode(2);' /><label for='cbShowRegions2'><%=ParseString("Heat Map with borders")%></label>&nbsp;<br/>
                    <input type='radio' name="rbShowRegions" class='input checkbox' id='cbShowRegions3' <% =iif(RiskMapShowRegions = 3, " checked ", "") %> onclick='setRiskMapRegiosnMode(3);' /><label for='cbShowRegions3'><%=ParseString("Heat Map")%></label>&nbsp;
                </div>
                <div id="DivRiskRegionsLegend" style="overflow: hidden; margin-top: 2px; margin-right: 2px; vertical-align:top; display:none; width: 100%;">
                    <div class="bg_fill text" id="divRiskRegionsLegendHeader" style='margin-top:0px; margin-bottom:0px;'><%=ResString("lblRiskRegions")%></div>
                    <table id="RiskRegionsLegend" class='text grid small' style='table-layout:fixed; border-collapse:collapse; width:100%;' cellspacing='0'></table>
                </div>
                <div id="DivRiskMapLegend" style="overflow:hidden; margin-top:4px; margin-right:2px; vertical-align:top; display:none; width: 100%;">
                    <div class="bg_fill text" id="divRiskEventsLegendHeader" style='margin-top:0px; margin-bottom:0px;'><%=ResString("lblRiskResultsEvents")%>
                        <% If IsWidget Then %><i class="fas fa-times ec-icon" style="float: right; cursor: pointer; vertical-align: middle;" onclick="RiskMapShowLegends = false; InitBowTieAndRiskPlot();"></i><% End If %>
                        <span id="btnDownloadLegend" class="actions ec-icon small" style="float: right; font-weight: normal;" onclick="btnDownloadLegendClick(); return false;">Save as image</span>
                    </div>
                    <div id='DivRiskMapLegendInt' style='overflow: auto; padding: 0;'><table id="RiskMapLegend" class='text grid small' style='vertical-align: top; table-layout: fixed; border-collapse: collapse; width: 100%;' cellspacing='0'>
                        <colgroup>
                           <col span="1">
                           <col span="1" id="legendNameCol" style="width: 200px;"> <%--Name--%>
                           <col span="1" style="min-width: 70px;"> <%--L--%>
                           <col span="1" style="min-width: 60px;"> <%--I--%>
                           <col span="1" style="min-width: 50px;"> <%--R--%>
                        </colgroup>
                        <tr><th style='border:1px solid #ccc;width:24px;'><a href='' onclick='onRiskLegendSortClick(0); return false;'><%=If(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank, ResString("optRank"), ResString("tblNo_")) %><%=IIf(RiskMapLegendSortColumn = 0, IIf(RiskMapLegendSortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC), "")%></a></th><th style='border:1px solid #ccc;padding:3px;'><a href='' onclick='onRiskLegendSortClick(1); return false;'><%=ResString("lblFormName")%><%=IIf(RiskMapLegendSortColumn = 1, IIf(RiskMapLegendSortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC), "")%></a></th><th style='border:1px solid #ccc;width:70px;word-wrap:break-word;'><a href='' onclick='onRiskLegendSortClick(2); return false;'><%=ResString("lblLikelihood")%><%=IIf(RiskMapLegendSortColumn = 2, IIf(RiskMapLegendSortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC), "")%></a></th><th style='border:1px solid #ccc;padding:3px;width:70px;word-wrap:break-word;'><a href='' onclick='onRiskLegendSortClick(3); return false;'><%=ResString("lblImpact") + CStr(IIf(ShowDollarValue, ", $", ""))%><%=IIf(RiskMapLegendSortColumn = 3, IIf(RiskMapLegendSortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC), "")%></a></th><th style='border:1px solid #ccc;padding:3px;width:50px;word-wrap:break-word;'><a href='' onclick='onRiskLegendSortClick(4); return false;'><%=ResString("lblRisk") + CStr(IIf(ShowDollarValue, ", $", ""))%><%=IIf(RiskMapLegendSortColumn = 4, IIf(RiskMapLegendSortDirection = SortDirection.Ascending, _SORT_ASC, _SORT_DESC), "")%></a></th></tr></table></div>
                </div>
                <div id="DivAttributeColorsLegend" style="overflow:hidden; margin-top:2px; margin-right:2px; vertical-align:top;display:none; width: 100%;">
                    <div class="bg_fill text" id="divAttributesColorLegendHeader" style='margin-top:0px; margin-bottom:0px;'><%=ResString("lblRiskResultsCatColors")%></div>
                    <div id='DivAttributeColorsLegendInt' style='overflow:auto'><table id="RiskAttrColorsLegend" class='text grid small' style='table-layout:fixed; border-collapse:collapse; width:100%;' cellspacing='0'></table></div>
                </div>
                <div id="DivAttributeShapesLegend" style="overflow:hidden; margin-top:2px; margin-right:2px; vertical-align:top;display:none; width: 100%;">
                    <div class="bg_fill text" id="divAttributesShapesLegendHeader" style='margin-top:0px; margin-bottom:0px;'><%=ResString("lblRiskResultsCatShapes")%></div>
                    <div id='DivAttributeShapesLegendInt' style='overflow:auto'><table id="RiskAttrShapesLegend" class='text grid small' style='table-layout:fixed; border-collapse:collapse; width:100%;' cellspacing='0'></table></div>
                </div>
                <% =GetPipeButtons() %>
            </td>
            </tr>
            </table>
        <%End If%>

        <%If IsBowTieByObjectivePage() Then %>
            <div style="text-align:center">
                <nobr><%=ParseString("Select an %%Alternative%% &nbsp;&nbsp;") + GetEvents(150)%></nobr>
             </div>
        <%End If%>

        <% If IsBowTiePage() Then %>
            <center>
            <div id='DivBowTie' style='margin-top:0px; width:1000px; height:100%; overflow:hidden;'>
            <table id='tableBowTieHeaders' cellpadding='4' cellspacing='0' class='text' style='margin:5px 0px 5px 0px;' width='100%'>
                <tr><td align="center" style="width: 20%;"><%--<label id="tbCauses" style='text-align:center; font-size:18px; color:Green; cursor:default;'><%=ResString("lblSources")%></label>--%>
                <a href='#' id='hbCauses' style='float:left;text-align:center; font-size:18px; color:Green; cursor:pointer;' onclick='showHierarchy(<%=CInt(ECHierarchyID.hidLikelihood)%>); return false;'><i class="fas fa-sitemap"></i>&nbsp;<%=ResString("lblSources")%></a></td>
                <td align="center" style="width: 20%;"><label id="tbLikelihoodOfCauses" style='text-align:center; color:Green; cursor:default; display:none;'><%=ResString("lblLikelihoodGivenEvent")%></label></td>
                <td align="center" style="width: 20%;"><label id="tbRisk" style='text-align:center; color:Red; cursor:default; display:none;'><% = IF(CanShowAllPriorities, ResString("lblBowTieRiskTitle"), ResString("tblAlternative"))%></label></td>
                <td align="center" style="width: 20%;"><label id="tbImpactOfConsequences" style='text-align:center; color:Blue; cursor:default; display:none;'><%=ResString("lblImpactGivenEvent")%></label></td>
                <td align="center" style="width: 20%;"><%--<label id="tbConsequences" style='text-align:center; color:Blue; font-size:18px; cursor:default;'><%=ResString("lblConsequences")%></label>--%>
                <a href='#' id='hbConsequences' style='float:right; text-align:center; color:Blue; font-size:18px; cursor:pointer;' onclick='showHierarchy(<%=CInt(ECHierarchyID.hidImpact)%>); return false;'><i class="fas fa-sitemap"></i>&nbsp;<%=ResString("lblConsequences")%></a></td></tr>
            </table>
            <div id='DivCanvas' style='overflow-y:auto; overflow-x:hidden; margin: 0px auto; text-align:center; position:relative;'>
                <canvas id="BowTieViewChart" width="1000" height="220" style="background:white;"></canvas>
                <div id='divControls' style='position:absolute; top:0; left:0;'></div>
            </div>
            </div>
            </center>
            <div runat='server' id='DivBowTieData' style='display:none;'></div>
            <% If CanShowAllPriorities Then%>
            <center><div id='gTitlesLegend' style='text-align:center; width:100%; margin:3px 0px 0px 0px; vertical-align:top; overflow:hidden;'>
            <table cellpadding='4' cellspacing='0' class='text' style='text-align:center; margin:0px 0px 0px 0px; width:100%;' > <!-- Visibility="Collapsed" -->
                <%--<tr><td style="width:50%;" align='left'><label id="lblLegendA" style='cursor:default;'><%=ResString("lblRiskLegendA").Trim()(0)%> - <%=ResString("lblRiskLegendA")%></label>&nbsp;</td><td align='left'>&nbsp;<label id="lblLegendC" style='cursor:default;'><%=ResString("lblRiskLegendC").Trim()(0)%>- <%=ResString("lblRiskLegendC")%></label></td></tr>
                <tr><td style="width:50%;" align='left'><label id="lblLegendB" style='cursor:default;'><%=ResString("lblRiskLegendB").Trim()(0)%> - <%=ResString("lblRiskLegendB")%></label>&nbsp;</td><td align='left'>&nbsp;<label id="lblLegendD" style='cursor:default;'><%=ResString("lblRiskLegendD").Trim()(0)%>- <%=ResString("lblRiskLegendD")%></label></td></tr>--%>
                <tr><td style="width:50%; padding-right:30px;" align='right'><div style="text-align:left;float:right;"><label id="lblLegendA" style='cursor:default;'><%=ResString("lblRiskLegendA").Trim()(0)%> - <%=ResString("lblRiskLegendA")%></label><br/><label id="lblLegendB" style='cursor:default;'><%=ResString("lblRiskLegendB").Trim()(0)%> - <%=ResString("lblRiskLegendB")%></label></div></td><td style="width:50%; padding-left:30px;" align='left'><div style="text-align:left;"><label id="lblLegendC" style='cursor:default;'><%=ResString("lblRiskLegendC").Trim()(0)%> - <%=ResString("lblRiskLegendC")%></label><br/><label id="lblLegendD" style='cursor:default;'><%=ResString("lblRiskLegendD").Trim()(0)%> - <%=ResString("lblRiskLegendD")%></label></div></td></tr>
            </table>
            </div></center>
            <% End If %>            
        <% End If%>
            
        </td></tr>        

        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        <tr>
          <td id='tdLegend' align='center' style='padding-top:1em'>
              <% =GetPipeButtons() %>
          </td>
        </tr>
        <% End If%>

        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        <tr>
          <td id='tdInfobar' class='text small' align='center' style='padding-top:1em'>
            <%--<div id="DivGridColorLegend" runat="server" class="note" style="display: none; margin-top: 4px;"></div>--%>
            <div id="DivGridColorLegend" runat="server" class="" style="display: none; margin-top: 4px;"></div>
          </td>
        </tr>
        <% End If%>
    </table></center>
    
    </telerik:RadCodeBlock>
</div>
</telerik:RadPane>

</telerik:RadSplitter>
</div>

<%-- Users --%>
<div id="divUsersAndGroups" style="display:none;position:relative;">
    <table border="0" cellpadding="0" cellspacing="0" style="width:100%;">
        <tr valign="top">
            <td valign="bottom" style="width:450px;"><table id='tableUsers' class='text cell-border hover order-column' style='width:450px;'></table></td>
            <td valign="bottom" style="padding-left:5px;width:300px;"><table id='tableGroups' class='text cell-border hover order-column' style='width:100%;'></table></td>
        </tr>
    </table>

    <div style='text-align:center; margin-top:1ex; width:100%;'>
        <a href="" onclick="return SelectAllUsers(1)" class="actions"><% =ResString("lblSelectAll")%></a> |
        <a href="" onclick="return SelectAllUsers(2)" class="actions"><% =ResString("btnSelectAllWithData")%></a> | 
        <a href="" onclick="return SelectAllUsers(0)" class="actions"><% =ResString("lblDeselectAll")%></a>
    </div>
</div>

<div id="divApplications" style="display:none;">
    <label id='lblHeader'></label>
    <table id="tblApplications" border='0' cellspacing='1' cellpadding='2' style='width:100%;' class='grid'></table>
</div>

<div id="divRegions" style="display:none;">
<table id="tblRegionsSettings" cellpadding='0' class="text" cellspacing='0' border='0'>
<tr style='height:25px;'><td colspan="2"><%=ResString("lblRiskSettings")%>:&nbsp;</td></tr>
<tr style='height:25px;'><td><nobr>If <%=ResString("lblRisk")%> &gt; Rh</nobr></td><td>                       <div id="RadColorPickerHigh"></div></td></tr>
<tr style='height:25px;'><td><nobr>If <%=ResString("lblRisk")%> &lt;= Rh and &gt;= Rl&nbsp;&nbsp;&nbsp;</nobr></td><td><div id="RadColorPickerMid"></div></td></tr>
<tr style='height:25px;'><td><nobr>If <%=ResString("lblRisk")%> &lt; Rl</nobr></td><td>                       <div id="RadColorPickerLow"></div></td></tr>
<tr style='height:25px;'><td colspan="2">&nbsp;</td></tr>
<%--<tr style='height:25px;'><td colspan="2"><b><span id="lblWRTNodeName"></span></b></td></tr>--%>
<tr style="height:25px;">
    <td colspan="2">
        <label><input type="radio" id="rbDollarInputType0" name="rbDollarInputType" <%=If(DollarValue = UNDEFINED_INTEGER_VALUE OrElse DollarValueTarget = UNDEFINED_STRING_VALUE, " disabled='disabled' ", "") %> checked="checked" onclick="convertRiskRegion(false);" />Percentage</label>
        <label><input type="radio" id="rbDollarInputType1" name="rbDollarInputType" <%=If(DollarValue = UNDEFINED_INTEGER_VALUE OrElse DollarValueTarget = UNDEFINED_STRING_VALUE, " disabled='disabled' ", "") %> onclick="convertRiskRegion(true);" />Monetary Value</label>
    </td>
</tr>
<tr style='height:25px;'><td>Rh (%) = </td><td><telerik:RadNumericTextBox ID="RadNumericTextBoxRh" runat="server" Width="150" MinValue="0" MaxValue="100" ButtonsPosition="Right" AutoPostBack="false" IncrementSettings-Step="1" Value="0.00" NumberFormat-DecimalDigits="2" ShowSpinButtons="true" EnabledStyle-HorizontalAlign="Right"></telerik:RadNumericTextBox></td></tr>
<tr style='height:25px;'><td>Rl (%) = </td><td><telerik:RadNumericTextBox ID="RadNumericTextBoxRl" runat="server" Width="150" MinValue="0" MaxValue="100" ButtonsPosition="Right" AutoPostBack="false" IncrementSettings-Step="1" Value="0.00" NumberFormat-DecimalDigits="2" ShowSpinButtons="true" EnabledStyle-HorizontalAlign="Right"></telerik:RadNumericTextBox></td></tr>
<tr style='height:25px;'><td colspan="2">&nbsp;</td></tr>
<tr style='height:25px;'><td colspan="2">&nbsp;</td></tr>
<tr style='height:25px;'><td colspan="2">&nbsp;</td></tr>
</table>
</div>

<%--<div id="divSettings" style="display:none;" ondblclick="onToolbarDblClick();">--%>
<div id="divSettings" style="display:none;">
    <div style="width: 100%; height: 96%; overflow: auto; vertical-align: top;">
    <% If Not IsRiskResultsByEventPage Then%>
    <%--<fieldset class="legend" style="display:none; text-align:left; vertical-align:top; width:550px;" id="pnlNormSettings">
        <legend class="text legend_title">&nbsp;<%=ResString("lblNormalizeOptions")%>&nbsp;</legend>
        <table border='0' cellspacing='0' cellpadding='0' class='text'>
            <tr>
                <td class='text' align='left' style="padding-bottom: 2px;">
                    <nobr><input type='checkbox' class='input checkbox' id='cbNormEventLkhd' value='1'<% =iif(NormalizedLkhd <> SynthesisMode.smAbsolute, " checked", "") %> onclick='OnNormalizeLocal(false, this.checked); sendCommand("norm_lkhd_local=" + (this.checked));' /><label for='cbNormEventLkhd'><% =ResString("lblNormalizeEventsLikelihoods")%> (Relative B)</label></nobr>
                </td>
                <td class='text' align='left'>
                    <nobr><input type='checkbox' class='checkbox' id='cbNormalizeImpact' value='1'<%=IIf(NormalizedLocalImpact <> LocalNormalizationType.ntUnnormalized, " checked", "") %> onclick='OnNormalizeLocal(true, this.checked); sendCommand("norm_impact=" + (this.checked));' /><label for='cbNormalizeImpact'>Normalized <% =ResString("lblImpact")%></label></nobr>
                </td>
            </tr>
        </table>
    </fieldset>
    <br />--%>    
    <fieldset class="legend" style="text-align:left; vertical-align:top; width:550px;" id="pnlSimulationsSettings">
        <legend class="text legend_title">&nbsp;Simulations Settings&nbsp;</legend>
        <nobr>
            Number of trials:&nbsp;<input type="text" id="tbNumSimulations" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=PM.RiskSimulations.NumberOfTrials.ToString%>" style="width:70px; height:20px; margin-top:2px; text-align:right;" onblur="saveNumerOfSimulations(this.value);" />
            <a href="" onclick="getOptimalNumberOfTrials(); return false;"><img src="<% =ImagePath %>magic_wand.png"/>&nbsp;Get Optimal Number Of Trials</a>
            <br/>
            &nbsp;&nbsp;Seed:&nbsp;<input type="text" id="tbRandSeed" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<% = PM.RiskSimulations.RandomSeed.ToString%>" style="width:50px; height:20px; margin-top:2px; text-align:right;" onblur="saveRandSeed(this.value);" />
            &nbsp;<label><input type="checkbox" id="cbKeepRandSeed" <% = If(PM.RiskSimulations.KeepRandomSeed, " checked='checked' ", " ") %> onclick="saveKeepRandSeed(this.checked);" />&nbsp;Keep Seed</label>
            &nbsp;<label title="<%=ResString("lblLECUseSourceGroups")%>"><input type="checkbox" id="cbUseSourceGroups" class="checkbox" <% = If(PM.RiskSimulations.UseSourceGroups, " checked='checked' ", "") %> onclick="onUseSourceGroups(this.checked);" /><%=ResString("lblLECUseSourceGroups")%></label>
            &nbsp;<label title="<%=ResString("lblLECUseEventGroups")%>"><input type="checkbox" id="cbUseEventGroups" class="checkbox" <% = If(PM.RiskSimulations.UseEventsGroups, " checked='checked' ", "") %> onclick="onUseEventGroups(this.checked);" /><%=ResString("lblLECUseEventGroups")%></label>
        </nobr><br/><br/>
        <nobr class="on_advanced">
            Consequences Simulation Mode:&nbsp;
                <select class="select" id='cbDilutedMode' onclick="if (this.value != dilutedMode) { dilutedMode = this.value; sendCommand('diluted_mode='+this.value); }">
                    <option value="0" <%=If(PM.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Diluted, " selected='selected'", "") %>>Diluted</option>
                    <option value="1" <%=If(PM.CalculationsManager.ConsequenceSimulationsMode = ConsequencesSimulationsMode.Undiluted, " selected='selected'", "") %>>Undiluted</option>
                </select>
        </nobr>
        
        <%--Simulation mode:<br/>
        <nobr><input type='radio' name="rbRiskSimuationMode" class='input radio' id='rbRiskSimulationsAdditive' value='0' checked='checked' onclick='' /><label for='rbRiskSimulationsAdditive'><%=ParseString("Additive")%></label>&nbsp;</nobr>
        <nobr><input type='radio' name="rbRiskSimuationMode" class='input radio' id='rbRiskSimulationsMultiplicative' value='1' onclick='' /><label for='rbRiskSimulationsMultiplicative'><%=ParseString("Multiplicative")%></label>&nbsp;</nobr>--%>
    </fieldset>
    <br/>
    <fieldset class="legend" style="text-align:left; vertical-align:top; width:550px;" id="pnlEventSettings">
        <legend class="text legend_title">&nbsp;<%=ResString("lblDisplayOptions")%>&nbsp;</legend>
        <nobr><input type='checkbox' class='input checkbox' id='cbShowEventNumbers' value='1' <% =IIf(PM.Parameters.NodeIndexIsVisible, " checked", "") %> onclick='cbShowEventNumbersChecked(this.checked);' /><label for='cbShowEventNumbers'><%=ResString("lblShowEventNumbers")%></label>
        <select class='select' id='cbEventIDMode' onchange='cbEventIDModeChange(this.value);' <%=IIf(Not PM.Parameters.NodeIndexIsVisible, " disabled='disabled' ", "") %>>
            <option value='0'<% =IIf(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.UniqueID, " selected", "") %>><%=ResString("optUniqueID")%></option>
            <option value='1'<% =IIf(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.IndexID, " selected", "") %>><%=ResString("optIndexID")%></option>
            <option value='2'<% =IIf(PM.Parameters.NodeVisibleIndexMode = IDColumnModes.Rank, " selected", "") %>><%=ResString("optRank")%></option>
        </select>
        </nobr><br/>
        <% If Not IsBowTiePage() AndAlso Not IsRiskMapPage() Then%>
        <input type='checkbox' class='input checkbox' id='cbShowEventDescriptions' value='1' <% =IIf(ShowEventDescriptions, " checked", "") %> onclick='cbShowEventDescriptionsChecked(this.checked);' /><label for='cbShowEventDescriptions'><%=ResString("lblShowEventDescriptions")%></label>
        <%End If%>
    </fieldset>
    <br />
    <%End If%>
    <% If IsBowTiePage() Then%>
    <fieldset class="legend" style="text-align:left; vertical-align:top; width:550px;" id="pnlBowTieSettings">
        <legend class="text legend_title">&nbsp;Bow-Tie Settings&nbsp;</legend>
        <div style="white-space: nowrap;">
        <div style="display: inline-block; width: 50%">
        <% If CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_CAUSES OrElse CurrentPageID = _PGID_RISK_BOW_TIE_WITH_CONTROLS_OBJS Then%>
            <%--<input type='checkbox' class='input checkbox' id='cbCanUserEditControls' value='1'<% =iif(CanUserEditControls, " checked", "") %> onclick='CanUserEditControls = this.checked; sendCommand("show_controls=" + (this.checked));' /><label for='cbCanUserEditControls'><% = ParseString("%%Controls%%") %></label>&nbsp;<br/>--%>
            <input type='checkbox' class='input checkbox' id='cbShowControlIndices' value='1'<% =IIf(ShowTreatmentIndices, " checked", "") %> onclick='ShowTreatmentIndices = this.checked; sendCommand("show_control_indices=" + (this.checked));' /><label for='cbShowControlIndices'><% = ParseString("%%Control%% Indices") %></label>&nbsp;<br/>    
            <input type='checkbox' class='input checkbox' id='cbShowControlNames' value='1'<% =IIf(ShowTreatmentNames, " checked", "") %> onclick='ShowTreatmentNames = this.checked; sendCommand("show_control_names=" + (this.checked));' /><label for='cbShowControlNames'><% = ParseString("%%Control%% Names") %></label>&nbsp;<br/>
        <% End If%>                
        <input type='checkbox' class='input checkbox' id='cbShowUpperLevelNodes' value='1'<% =IIf(CanShowUpperLevelNodes, " checked", "") %> onclick='CanShowUpperLevelNodes = this.checked; sendCommand("show_upper_level_nodes=" + (this.checked));' /><label for='cbShowUpperLevelNodes'>Show Upper Level Nodes</label>&nbsp;<br/>
        <input type='checkbox' class='input checkbox' id='cbShowBowTieBackground' value='1'<% =IIf(CanShowBowTieBackground, " checked", "") %> onclick='CanShowBowTieBackground = this.checked; sendCommand("show_bowtie_background=" + (this.checked));' /><label for='cbShowBowTieBackground'>Show Background Colors</label>&nbsp;<br/>
        <input type='checkbox' class='input checkbox' id='cbShowBowTieAttributes' value='1'<% =IIf(CanShowBowTieAttributes, " checked", "") %> onclick='CanShowBowTieAttributes = this.checked; sendCommand("show_bowtie_attributes=" + (this.checked));' /><label for='cbShowBowTieAttributes'><%=ResString("lblRiskShowEventAttributes") %></label>&nbsp;<br/>
        <%--<input type='checkbox' class='input checkbox' id='cbShowResults' value='1'<% =If(CanShowAllPriorities, " checked", "") %> onclick='CanShowAllPriorities = this.checked; sendCommand("show_bowtie_results=" + (this.checked));' /><label for='cbShowResults'><%=ResString("lblRiskShowResults")%></label>&nbsp;<br/>--%>
        </div>
        <div style="display:inline-block; width: 50%; padding:3px; background-color:white;">
            <nobr>
            <label id='cbSortLikelihood' style='cursor:default;'>Sort:</label>&nbsp;
            <select size='1' style='width:100px;' onchange='sendCommand("bowtie_likelihood_sort=" + (this.value));'>
                <option value='0' <% = IIf(BowTieSort(0) = 0,"selected='selected'","") %>><% = ResString("lblSelectNone")%></option>
                <option value='1' <% = IIf(BowTieSort(0) = 1,"selected='selected'","") %>><% = ResString("lblRiskLegendA")%> (<%=ResString("lblRiskLegendA").Trim()(0)%>)</option>
                <option value='3' <% = IIf(BowTieSort(0) = 3,"selected='selected'","") %>><% = ResString("lblRiskLegendB")%> (<%=ResString("lblRiskLegendB").Trim()(0)%>)</option>
                <option value='2' <% = IIf(BowTieSort(0) = 2,"selected='selected'","") %>><% = ResString("optSortByCauseLkh")%> (<%=ResString("lblRiskLegendA").Trim()(0)%>*<%=ResString("lblRiskLegendB").Trim()(0)%>)</option>
            </select>&nbsp;
            <label id='cbSortImpact' style='cursor:default;'>:</label>&nbsp;
            <select size='1' style='width:100px;' onchange='sendCommand("bowtie_impact_sort=" + (this.value));'>
                <option value='7' <% = IIf(BowTieSort(1) = 7,"selected='selected'","") %>><% = ResString("lblSelectNone")%></option>                    
                <option value='5' <% = IIf(BowTieSort(1) = 5,"selected='selected'","") %>><% = ResString("optSortByConsequenceImp")%> (<%=ResString("lblRiskLegendC").Trim()(0)%>*<%=ResString("lblRiskLegendD").Trim()(0)%>)</option>
                <option value='6' <% = IIf(BowTieSort(1) = 6,"selected='selected'","") %>><% = ResString("lblRiskLegendC")%> (<%=ResString("lblRiskLegendC").Trim()(0)%>)</option>
                <option value='4' <% = IIf(BowTieSort(1) = 4,"selected='selected'","") %>><% = ResString("lblRiskLegendD")%> (<%=ResString("lblRiskLegendD").Trim()(0)%>)</option>
            </select></nobr></div>
        </div>
    </fieldset>
    <br />
    <%End If%>
    <% If IsRiskMapPage() Then%>
    <fieldset class="legend" style="text-align:left; vertical-align:top; width:550px;" id="pnlRiskMapSettings">
        <legend class="text legend_title">&nbsp;Risk Map Settings&nbsp;</legend>            
            <nobr><label id='lblAxisXTitle' for='tbAxisXTitle'>Axis X Name:</label>
            <input type='text' id='tbAxisXTitle' size='10' value='<% = AxisXTitle %>' onblur='saveAxisTitle(this.value, true); return false;' />&nbsp;&nbsp;
            <label id='lblAxisYTitle' for='tbAxisYTitle'>Axis Y Name:</label>
            <input type='text' id='tbAxisYTitle' size='10' value='<% = AxisYTitle %>' onblur='saveAxisTitle(this.value, false); return false;' /></nobr><br/>
            <nobr style="display:inline-block; width:150px;"><input type='checkbox' class='input checkbox' id='cbSwitchAxes' <% =iif(RiskMapSwitchAxes, " checked ", "") %>  onclick='RiskMapSwitchAxes  = (this.checked); sendCommand("switch_axes=" + (this.checked));' /><label for='cbSwitchAxes'><%=ResString("lblRiskSwitchAxes")%></label>&nbsp;</nobr>            
            <br/>
            <%--<nobr style="display:inline-block; width:150px;"><input type='checkbox' class='input checkbox' id='cbZoomRiskPlot' <% =iif(ZoomPlot, " checked ", "") %>  onclick='ZoomPlot = this.checked; sendCommand("zoom_plot=" + (this.checked));' /><label for='cbZoomRiskPlot'><%=ResString("lblRiskZoomPlot")%></label>&nbsp;</nobr>--%>
            <nobr><input type='checkbox' class='input checkbox' id='cbShowLegend' <% =iif(RiskMapShowLegends, " checked ", "") %> onclick='RiskMapShowLegends = (this.checked); sendCommand("show_legends=" + (this.checked));' /><label for='cbShowLegend'><%=ResString("lblRiskShowLegend")%></label>&nbsp;</nobr><br/>
            <nobr style="display:inline-block; width:150px;"><input type='checkbox' class='input checkbox' id='cbShowLabels'  <% =iif(RiskMapShowLabels, " checked ", "") %> onclick='RiskMapShowLabels  = (this.checked); sendCommand("show_labels="  + (this.checked));' /><label for='cbShowLabels'><%=ResString("lblRiskShowLabels")%></label>&nbsp;</nobr>
            <nobr><input type='checkbox' class='input checkbox' id='cbJitter' <% =iif(RiskMapJitterDatapoints, " checked ", "") %> onclick='RiskMapJitterDatapoints  = (this.checked); sendCommand("jitter_datapoints="  + (this.checked));' /><label for='cbJitter'>Jitter Overlapping Data Points</label>&nbsp;</nobr><br/>            
            <hr/>    
            <nobr><label for='tbReverseZoom' style="font-weight: bold;"><%=ResString("lblReverseZoom")%></label><br/>
            <center style="margin-right: 40px;">
            <span style="display: inline-block; width: 60px; text-align: right;">top:</span><input type="number" id='tbReverseZoomtop' min="0" size='6' style="text-align:right; width: 80px;" value='<%=IIf(RiskMapReverseZoomValueTop>0,RiskMapReverseZoomValueTop,"")%>' onblur='setReverseZoomValue(this.value,"top");' /><br/>
            <span style="display: inline-block; width: 60px; text-align: right;">left:</span><input type="number" id='tbReverseZoomleft' min="0" size='6' style="text-align:right; width: 80px;" value='<%=IIf(RiskMapReverseZoomValueLeft>0,RiskMapReverseZoomValueLeft,"")%>' onblur='setReverseZoomValue(this.value,"left");' />
            <div style="display: inline-block; width: 60px;">&nbsp;</div>
            <span style="display: inline-block; width: 60px; text-align: right;">right:</span><input type="number" id='tbReverseZoomright' min="0" size='6' style="text-align:right; width: 80px;" value='<%=IIf(RiskMapReverseZoomValueRight>0,RiskMapReverseZoomValueRight,"")%>' onblur='setReverseZoomValue(this.value,"right");' /><br/>
            <span style="display: inline-block; width: 60px; text-align: right;">bottom:</span><input type="number" id='tbReverseZoombottom' min="0" size='6' style="text-align:right; width: 80px;" value='<%=IIf(RiskMapReverseZoomValueBottom>0,RiskMapReverseZoomValueBottom,"")%>' onblur='setReverseZoomValue(this.value,"bottom");' /></center>
            </nobr>
        </fieldset>
        <br />
    <% End If%>
    <fieldset class="legend" style="text-align:left; vertical-align:top; width:550px;" id="pnlDisplaySettings">
        <legend class="text legend_title">&nbsp;Display Settings&nbsp;</legend>
        <nobr><input type='checkbox' class='input checkbox' id='cbCIS' value='1' <% =IIf(App.ActiveProject.ProjectManager.CalculationsManager.UseCombinedForRestrictedNodes, " checked", "") %> onclick='sendCommand("use_cis=" + (this.checked));' /><label for='cbCIS'><%=ResString("lblCIS")%></label>&nbsp;</nobr><br />
        <nobr><input type='checkbox' class='input checkbox' id='cbWeights' value='1' <% =IIf(App.ActiveProject.ProjectManager.CalculationsManager.UseUserWeights And (Not App.ActiveProject.ProjectManager.IsUsingDefaultWeightForAllUsers), " checked", "") %> <%=IIf(App.ActiveProject.ProjectManager.IsUsingDefaultWeightForAllUsers, " disabled='disabled' ", " ") %> onclick='sendCommand("use_weights=" + (this.checked));' /><label for='cbWeights'><%=ResString("lblUserPriorities")%></label>&nbsp;</nobr><br />
        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
            <nobr><input type='checkbox' class='input checkbox' id='cbShowTotalRisk' value='1' <% =IIf(ShowTotalRisk, " checked", "") %> onclick='sendCommand("show_total_risk=" + this.checked);' /><label for='cbRiskTotal'><%=ResString("lblRiskTotal")%> & Average <%=ParseString("%%Loss%%")%></label>&nbsp;</nobr><br/>
        <% End If%>
        <input type='checkbox' class='input checkbox' id='cbShowCents' <% = If(ShowCents, " checked", "") %> onclick='showCents = this.checked; sendCommand("action=show_cents&chk=" + (this.checked));' /><label for='cbShowCents'><%=ParseString("Show cents for monetary values")%></label>&nbsp;<br />
        <nobr><span>&nbsp;<%=ResString("lblDecimals")%>:</span>
        <select class='select' id='cbDecimals' style='width:60px;' onchange='cbDecimalsChange(this.value);'>
            <option value='0'<% = if(PM.Parameters.DecimalDigits=0, " selected", "") %>>0</option>
            <option value='1'<% = if(PM.Parameters.DecimalDigits=1, " selected", "") %>>1</option>
            <option value='2'<% = if(PM.Parameters.DecimalDigits=2, " selected", "") %>>2</option>
            <option value='3'<% = if(PM.Parameters.DecimalDigits=3, " selected", "") %>>3</option>
            <option value='4'<% = if(PM.Parameters.DecimalDigits=4, " selected", "") %>>4</option>
            <option value='5'<% = if(PM.Parameters.DecimalDigits=5, " selected", "") %>>5</option>
        </select></nobr>&nbsp;<br/>        
        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
            <nobr><%If CheckedUsersCount() >= 1 Then%><span>&nbsp;<%=ResString("lblShowRelativeToOne")%>:</span><input type='checkbox' class='checkbox' id='cbLRel' value='1'<% =IIf(BarsLRelativeTo1, " checked", "") %> onclick='sendCommand("bars_l=" + (this.checked));' /><label for='cbLRel'><%=ParseString("%%likelihood%%").ToUpper().First%>&nbsp;</label><input type='checkbox' class='checkbox' id='cbIRel' value='1'<% =IIf(BarsIRelativeTo1, " checked", "") %> onclick='sendCommand("bars_i=" + (this.checked));' /><label for='cbIRel'><%=ParseString("%%impact%%").ToUpper().First%>&nbsp;</label><input type='checkbox' class='checkbox' id='cbRRel' value='1'<% =IIf(BarsRRelativeTo1, " checked", "") %> onclick='sendCommand("bars_r=" + (this.checked));' /><label for='cbRRel'><%=ParseString("%%risk%%").ToUpper().First%>&nbsp;</label><%End If%>&nbsp;</nobr><br/>            
        <% End If%>                
        <% If UseControlsReductions AndAlso Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        <nobr><input type='checkbox' class='input checkbox' id='cbShowRiskReduction' value='1'<% =iif(ShowRiskReduction, " checked", "") %> onclick='sendCommand("show_risk_reduction=" + (this.checked));' /><label for='cbShowRiskReduction'><%=ResString("lblRiskReductionOption")%></label>&nbsp;<nobr>
        <% End If%>
        <% If Not IsRiskMapPage() AndAlso Not IsBowTiePage() Then%>
        <%--<nobr><input type='checkbox' class='checkbox' id='cbGroupTotals' <%=IIf(ShowGroupTotals, " checked", "") %> <%=IIf(ShowAttributes OrElse AttributesList Is Nothing OrElse AttributesList.Count = 0, " disabled ", "") %> onclick='onShowGroupTotals(this.checked);' <%=IIf(IsGroupByAttribute, "", " style='display:none;' ")%> /><label id="lblGroupTotals" for='cbGroupTotals' <%=IIf(IsGroupByAttribute, "", " style='display:none;' ")%>>Group Totals</label>&nbsp;</nobr>--%>
        <% End If%>
        <% If IsOptimizationAllowed And False Then%>
        <br/><nobr><input type='checkbox' class='checkbox' id='cbShowSolverPane' <%=IIf(ShowSolverPane, " checked", "") %> <%=IIf(PM.Controls.DefinedControls.Count = 0, " disabled ", "") %> onclick='onShowSolverPane(this.checked);' /><label id="lblShowSolverPane" for='cbShowSolverPane'><%=ResString("lblShowSolverPane")%><%If PM.Controls.DefinedControls.Count = 0 Then%><%=ParseString(" <span class='text'><sup>No %%controls%% defined")%></sup></span><%End If%></label>&nbsp;</nobr>
        <% End If%>
        <div class="on_advanced" style="display: inline-block;" id="divProbabilityCalculationMode">
        <nobr>
            <span>Probability Calculation:</span>
            <select class="select" id='cbProbabilityCalculation' onchange="likelihoodCalculationMode = this.value; sendCommand('probability_calculation_mode=' + this.value);">
                <option value="0" <%=If(PM.CalculationsManager.LikelihoodsCalculationMode = LikelihoodsCalculationMode.Regular, " selected='selected'", "") %>>Sum Product</option>
                <option value="1" <%=If(PM.CalculationsManager.LikelihoodsCalculationMode = LikelihoodsCalculationMode.Probability, " selected='selected'", "") %>>Combinatorial</option>
            </select>
        </nobr>&nbsp;</div>
        <div class="on_advanced" style="display: inline-block;" id="divWRTCalculationMode">
        <nobr>
            <span>Probability Calculation:</span>
            <select class="select" id='cbWRTCalculation' onchange="sendCommand('wrt_calculation_mode=' + this.value);">
                <option value="0" <%=If(Not PM.CalculationsManager.ShowDueToPriorities, " selected='selected'", "") %>><% = ParseString("Show likelihoods and priority of %%objectives(i)%%") %></option>
                <option value="1" <%=If(PM.CalculationsManager.ShowDueToPriorities, " selected='selected'", "") %>><% = ParseString("Show vulnerabilities and consequences of %%objectives(i)%%") %></option>
            </select>
        </nobr>&nbsp;</div>            
    </fieldset>    
    </div>
    <div style="text-align: center;">
        <div id="btnSettingsClose" style=" width: 110px; text-align: center; position: absolute; bottom: 2px; right: 50%; margin-right: -55px;"></div>
    </div>
</div>

<div id='editorForm' style='display:none;'>
<table border='0' cellspacing='0' cellpadding='0' class='text' style='margin-right:5px;'>
<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("%%Control%% Name:") %>&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' style='width:280px;' id='tbControlName' value='' onfocus='getCtrlName(this.value);' onkeyup='if ((typeof getCtrlName != "undefined")) getCtrlName(this.value);' onblur='if ((typeof getCtrlName != "undefined")) getCtrlName(this.value);' /></td></tr>
<%--<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("Description:") %>&nbsp;&nbsp;</nobr></td><td><textarea rows='4' cols='40' style='width:280px;margin-top:2px;border:1px solid #999999;' id='tbControlDescr' onfocus='getCtrlDescr(this.value);' onkeyup='if ((typeof getCtrlDescr != "undefined")) getCtrlDescr(this.value);' onblur='if ((typeof getCtrlDescr != "undefined")) getCtrlDescr(this.value);'></textarea></td></tr>--%>
<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("Cost:") %>&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' style='width:100px;margin-top:3px;text-align:right;' id='tbControlCost' value='' onfocus='getCtrlCost(this.value);' onkeyup='if ((typeof getCtrlCost != "undefined")) getCtrlCost(this.value);' onblur='if ((typeof getCtrlCost != "undefined")) getCtrlCost(this.value);' /></td></tr>
<tr><td colspan='2'><hr style='margin:6px 0px 6px 0px; height:1px; border:none; color:#999999;background-color:#999999;' /></td></tr>
<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("Category:") %>&nbsp;&nbsp;</nobr></td><td>
    <ul id='lbCategories' disabled="disabled" style='height:14ex; width:280px; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'></ul>
</td></tr>
<tr><td>&nbsp;</td><td><input type='text' class='input' id='tbNewCategory' style='width:254px; margin:2px 0px;' /><input type='button' class='button' style='width:24px; margin:2px;' value='+' onclick='if ((typeof AddCategory != "undefined")) AddCategory(); return false;' /></td></tr>
</table>
</div>

<div id='selectColumnsForm' style='display:none;position:relative;'>
    <div id="divSelectColumns" style="padding:5px; text-align:left;"></div>
    <div style='text-align:center; margin-top:1ex; width:100%;'>
        <a href="" onclick="filterColumnsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterColumnsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>
    
<%-- Controls with Checkboxes dialog --%>
<div id='selectControlsForm' style='display:none;'>
    <div style="overflow: auto; height: 92%;" class="whole">
    <div id="divSelectControls" style="padding:5px; text-align:left;"></div>
    </div>
    <div style='width: 100%; padding: 10px;'>
        <a href="" onclick="filterSelectAllControls(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllControls(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>    
    <div style="height: 18px;"></div>
    <div style="position: absolute; bottom: 5px; left: 50%; margin-left: -150px; width: 300px;">
        <div id="btnSelectControlsOK" style="width: 148px;"></div>
        <div id="btnSelectControlsCancel" style="width: 148px;"></div>
    </div>
</div>

<%-- Events with Checkboxes dialog --%>
<div id='selectEventsForm' style='display: none; overflow:hidden;'>
    <div id="divSelectEvents" style="padding: 5px; text-align: left; overflow:auto; height:300px;"></div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllEvents(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllEvents(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

<div id="divExpandedTreatmentView" style="display:none;">
    <div id="divExpandedTreatmentViewContainer" style="padding:5px; overflow:auto; margin:5px; text-align:center;">
        <canvas id="canvasExpandedTreatmentView" width="740" height="200"></canvas>
    </div>
</div>

<div style="margin:1ex; padding:1ex; display:none; overflow:auto;" id="divTreeLikelihood">
    <telerik:RadTreeView runat="server" ID="RadTreeViewLikelihood" AllowNodeEditing="false"></telerik:RadTreeView>
</div>

<div style="margin:1ex; padding:1ex; display:none; overflow:auto;" id="divTreeImpact">
    <telerik:RadTreeView runat="server" ID="RadTreeViewImpact" AllowNodeEditing="false"></telerik:RadTreeView>
</div>

<div id='DivRiskMapTooltip' style='position:absolute; display:none; background:#ffff99; width:300px; padding:4px; border:1px solid #ccc;' class='text'></div>

<div id="divAttributesValues" style="display:none; text-align:center">
<%--<div style="overflow:auto;" id="pAttributesValues"><table id="tblAttributesValues" border='0' cellspacing='1' cellpadding='2' style='width:96%;' class='grid drag_drop_grid'>--%>
    <div style="overflow:auto;" id="pAttributesValues"><table id="tblAttributesValues" border='0' cellspacing='1' cellpadding='2' style='width:96%;' class='grid'>
    <thead>
      <tr class="text grid_header" align="center">        
        <th style="width:1em">&nbsp;</th> <%--Drag-drop handle column--%>
        <th style="width:90%"><%=JS_SafeString(ResString("lblRACategory"))%></th>
        <th>&nbsp;Is Selected&nbsp;</th>
        <th>&nbsp;<%=JS_SafeString(ResString("tblIsDefault"))%>&nbsp;</th>
        <th>&nbsp;<%=JS_SafeString(ResString("tblRAScenarioAction"))%>&nbsp;</th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
</div>

<script language='javascript' type='text/javascript'>

    //window.onload = InitBowTieAndRiskPlot;    

    /* #Region "RISK MAP" */
    var selected_alt_data = [];
    var likelihood_data = [];
    var impact_data = [];
    var control_data = [];
    var cat_data = [];

    var likelihood_goal_guid = "<%=LikelihoodGoalGuid().ToString%>";
    var impact_goal_guid = "<%=ImpactGoalGuid().ToString%>";

    var dlg_regions = null;
    var dlg_treatment_editor = null;
    //var dlg_select_treatment_editor = null;

    var ColorsLegend = [];
    var ShapesLegend = [];

    var with_controls_suffix = "";

    function InitBowTieAndRiskPlot() {
        var cd = document.getElementById("<% =DivBowTieData.ClientID %>");
        if (isBowTie && (cd)) {
            var data = eval(cd[text]);
            selected_alt_data = data[0];
            likelihood_data = data[1];
            impact_data = data[2];
            control_data = data[3];
            cat_data = data[4];
        }
        cd = document.getElementById("<% =DivRiskMapData.ClientID %>");
        if (isRiskMap && (cd)) {
            var data = eval(cd[text]);
            l0 = data[0];
            l1 = data[1];
            l2 = data[2];
            plot_data = data[3];
            plot_data1 = [];
            plot_data2 = [];
            if (RiskPlotMode == 2) {
                for (var k = 0; k < plot_data.length; k ++) {
                    var dp = plot_data[k];
                    if (dp[3]["guid"].charAt(0) == "-") {
                        plot_data2.push(dp);
                    } else {
                        plot_data1.push(dp);
                    }
                }
            } else {
                plot_data1 = plot_data;
            }
            <% If PRJ.RiskionProjectType = ProjectType.ptMixed OrElse PRJ.RiskionProjectType = ProjectType.ptMyRiskReward Then %>
            plot_data1 = plot_data1.filter(function (dp) { return dp[3]["event_type"] == RiskPlotEventType; });
            plot_data2 = plot_data2.filter(function (dp) { return dp[3]["event_type"] == RiskPlotEventType; });
            <% End If %>

            MaxX = data[4]*1;
            MaxY = data[5]*1;
            MinX = data[6]*1;
            MinY = data[7]*1;
            ColorsLegend = data[8];
            ShapesLegend = data[9];
            cloud_data = data[10];
            with_controls_suffix = data[11];
            _Rh = data[12]*1;
            _Rl = data[13]*1;
        }
        updateToolbar();
        if (isBowTie)  {
            InitLabels(); 
            showProducts = !UseSimulatedValues;
            drawBowTie("BowTieViewChart");
        }
        if (isRiskMap) {
            loadChart('divRiskMap', window["plot1"], plot_data1, RiskPlotMode == 1 ? "<%=ParseString(" <b class='attention'>With %%Controls%%</b>")%>" : (RiskPlotMode == 3 ? "" : "<%=ParseString(" Without %%Controls%%")%>"));
            loadLegends(window["plot1"]);
            if (RiskPlotMode == 2) {
                loadChart('divRiskMapWC', window["plot2"], plot_data2, "<%=ParseString(" <b class='attention'>With %%Controls%%</b>")%>");
                loadLegends(window["plot2"]);
            }
        }
    }

    var IDX_COLOR_LEGEND = 10;

    var isRiskMap = <%=Bool2JS(IsRiskMapPage())%>;
    var isEventCloud = false; <%--<% =Bool2JS(RiskMapEventCloud)%>;--%>
    var isPlotZoomedbyUser = false;
    var RiskMapSwitchAxes  = <% =Bool2JS(RiskMapSwitchAxes())  %>;
    var RiskMapShowRegions = <% =RiskMapShowRegions %>;
    var ZoomPlot           = <% =Bool2JS(ZoomPlot()) %>;
    var RiskMapShowLabels  = <% =Bool2JS(RiskMapShowLabels())  %>;
    var RiskMapJitterDatapoints = <%=Bool2JS(RiskMapJitterDatapoints())%>;
    var RiskMapShowLegends = <% =Bool2JS(RiskMapShowLegends()) %>;
    var RiskMapBubbleSizeWithRisk= <% =Bool2JS(RiskMapBubbleSizeWithRisk()) %>;
    var ColorCodingByCategory  = <%=Bool2JS(ColorCodingByCategory)%>;
    var ShowColorLegend    = <% =Bool2JS(ColorCodingByCategory AndAlso AttributesList.Count > 0) %>;
    var ShowShapeLegend    = <% =Bool2JS(ShapeBubblesByCategory AndAlso AttributesList.Count > 0) %>;
    var reverseZoomTop = <%=RiskMapReverseZoomValueTop%>;
    var reverseZoomLeft = <%=RiskMapReverseZoomValueLeft%>;
    var reverseZoomRight = <%=RiskMapReverseZoomValueRight%>;
    var reverseZoomBottom = <%=RiskMapReverseZoomValueBottom%>;
    var RiskPlotMode = <% = CInt(RiskPlotMode) %>;
    var RiskPlotEventType = <% = CInt(RiskPlotEventType) %>;

    var OPT_PLOT_LABEL_DEFAULT_COLOR   = "#212121";
    var OPT_PLOT_LABEL_HIGHLIGHT_COLOR = "#ffffff";
    var OPT_LEGEND_HIGHLIGHT_COLOR     = "#ffff99";
    
    var _Rh = <% =JS_SafeNumber(_Rh) %>;
    var _Rl = <% =JS_SafeNumber(_Rl) %>;
    var _redBrush = "<% =PM.Parameters.Riskion_Regions_Rh_Color %>";
    var _whiteBrush = "<% =PM.Parameters.Riskion_Regions_Rm_Color %>";
    var _greenBrush = "<% =PM.Parameters.Riskion_Regions_Rl_Color %>";

    var DefaultRh = <% =JS_SafeNumber(PM.Parameters.DefaultRh) %>;
    var DefaultRl = <% =JS_SafeNumber(PM.Parameters.DefaultRl) %>;
    var DefaultRedBrush = "<% =PM.Parameters.DefaultRedBrush %>";
    var DefaultWhiteBrush = "<% =PM.Parameters.DefaultWhiteBrush %>";
    var DefaultGreenBrush = "<% =PM.Parameters.DefaultGreenBrush %>";
    
    var AxisXTitle = "<% = AxisXTitle %>";
    var AxisYTitle = "<% = AxisYTitle %>";

    var lblRisk = "<% = ParseString("%%Risk%%") %>";
    var sManualZoomParams = "<%=PM.Parameters.Riskion_RiskPlotManualZoomParams%>";

    // < % = GetRiskFunctionData(0) % >
    // < % = GetRiskFunctionData(1) % >
    // < % = GetRiskFunctionData(2) % >
    //   
    // < % = GetRiskPlotData() % >

    var l0 = [];
    var l1 = [];
    var l2 = [];
    var plot_data = [];
    var cloud_data = [];
    var bubble_mult = <%=RiskMapBubbleSize%> / 10;

    var MinX =   0;
    var MinY =   0;

    var MaxX = 100;
    var MaxY = 100;

    var IDX_LIKELIHOOD = 0;
    var IDX_IMPACT = 1;
    var IDX_BUBBLE_SIZE = 2;
    var IDX_LABEL1 = 3; // number
    var IDX_LABEL2 = 4; // full name
    var IDX_RISK   = 5;
    var IDX_EVENT_ID   = 6; // event guid

    //var DATAPOINT_COLORS = ["#4bb2c5", "#c5b47f", "#EAA228", "#579575", "#839557", "#958c12", "#953579", "#4b5de4", "#d8b83f", "#ff5800", "#0085cc", "#c747a3", "#cddf54", "#FBD178", "#26B4E3", "#bd70c7"];

    var SelectedHierarchyNodeID = "<%=SelectedHierarchyNodeID.ToString%>";
    var SelectedHierarchyNodeName = "<%=SelectedHierarchyNodeName%>";

    var tickCostFormatter = function (format, val) {        
        //if (plot_data.length > 0) {
        //    var d = plot_data[plot_data.length-1][IDX_LABEL1];
        //    var c0 = d.actual_impact;
        //    var c1 = d.dollar_impact;
        //    var c = c0 == 0 ? 0 : (val * c1) / c0;
        //    return showCost(c, true, !showCents);
        //}
        //return val + "";
        
        return val > 0 ? showCost(val * DollarValueOfEnterprise / 100, true, !showCents, true) : "";
    }

    var RHLabel = "";
    var RMLabel = "";
    var RLLabel = "";

    function loadLegends(plot) {
        if (DollarValueDefined && ShowDollarValue) {
            RHLabel = "Over " + showCost(_Rh * DollarValueOfEnterprise, true, !showCents);
            RMLabel = showCost(_Rl * DollarValueOfEnterprise, true, !showCents) + " - " + showCost(_Rh * DollarValueOfEnterprise, true, !showCents);
            RLLabel = "Under " + showCost(_Rl * DollarValueOfEnterprise, true, !showCents);
        } else {
            RHLabel = "Over " + (_Rh * 100).toFixed(2) + " %";
            RMLabel = (_Rl * 100).toFixed(2) + " % - " + (_Rh * 100).toFixed(2) + " %";
            RLLabel = "Under " + (_Rl * 100).toFixed(2) + " %";
        }

        if (RiskMapShowLegends) { $("#tdColorLegend").show() } else { $("#tdColorLegend").hide() };
        var isFirstChart = plot.targetId == "#divRiskMap";

        $("#DivRiskMapLegend").hide();
        if (isFirstChart) {
            $("#RiskMapLegend").find("tr:gt(0)").remove();
        }
        if (RiskMapShowLegends) {
          // Populate Legend with the labels from each data value
          //var dp_colors_len = DATAPOINT_COLORS.length;
          //$('#RiskMapLegend').empty();
          var delimiterAdded = false;
          if (isFirstChart && RiskPlotMode != 1) {
              var firstLegendTitle = "<%=ParseString("Without %%Controls%%")%>";
              $('#RiskMapLegend').append('<tr class="legend_row" valign="top"><td colspan="5" style="padding:1px 2px; border:1px solid #ccc; text-align:center;"><b>'+firstLegendTitle+'</b></td></tr>');
          }
          $.each(isFirstChart ? plot_data1 : plot_data2, function(index, val) {
              var lbl = val[IDX_LABEL2]; // full alt name //(val[IDX_LABEL1].label == "" ? val[IDX_LABEL2] : val[IDX_LABEL1].label);
              var lbl_short = <% If IsWidget Then %>ShortString(lbl, 55);<% Else %>"<a href='' class='actions' onclick='return OnEventName_Click(\"" + val[IDX_EVENT_ID] + "\",\"<%=CStr(IIf(HasListPage(PagesWithControlsList, CurrentPageID), 1, 0))%>\")' >" + ShortString(lbl, 55) + "</a>";<% End If %>
              var lbl_num = ShowEventNumbers ? val[IDX_LABEL1].label : ""; // number
              //var datapoint_color = DATAPOINT_COLORS[(index<dp_colors_len ? index : index % dp_colors_len)];
              
              //$('#RiskMapLegend').append('<tr class="legend_row" onmouseover="LegendRowHoverWithHighlight(this,1,3,'+index+');" onmouseout="LegendRowHoverWithHighlight(this,0,3,'+index+');" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; text-align:center;">' + lbl_num + '<br/><div style="width:12px;height:12px;background:'+val[IDX_LABEL1].color+'">&nbsp;</div></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+lbl+'" id="legend_row_'+index+'">'+lbl_short+'</td><td style="text-align:center; border:1px solid #ccc;" class="text small">'+(RiskMapSwitchAxes ? val[IDX_IMPACT] : val[IDX_LIKELIHOOD])+'%</td><td style="text-align:center; border:1px solid #ccc;">'+(RiskMapSwitchAxes ? val[IDX_LIKELIHOOD] : val[IDX_IMPACT])+'%</td><td style="text-align:center; border:1px solid #ccc;">'+val[IDX_RISK]+'</td></tr>');
              //$('#RiskMapLegend').append('<tr class="legend_row" onmouseover="LegendRowHoverWithHighlight(this,1,3,'+index+');" onmouseout="LegendRowHoverWithHighlight(this,0,3,'+index+');" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; text-align:center;">' + lbl_num + '<br/><div style="width:12px;height:12px;background:'+val[IDX_LABEL1].color+'">&nbsp;</div></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+lbl+'" id="legend_row_'+index+'">'+lbl_short+'</td><td style="text-align:center; border:1px solid #ccc;" class="text small">'+ val[IDX_LABEL1].actual_likelihood + '%</td><td style="text-align:center; border:1px solid #ccc;">' + (ShowDollarValue ? showCost(val[IDX_LABEL1].dollar_impact, true) : val[IDX_LABEL1].actual_impact + '%')+'</td><td style="text-align:center; border:1px solid #ccc;">' + (ShowDollarValue ? showCost(val[IDX_LABEL1].dollar_risk, true): val[IDX_RISK]) + '</td></tr>');
              if (!delimiterAdded && val[IDX_EVENT_ID].indexOf("-") == 0) {
                  $('#RiskMapLegend').append('<tr class="legend_row" valign="top"><td colspan="5" style="padding:1px 2px; border:1px solid #ccc; text-align:center;"><b><%=ParseString("With %%Controls%%")%></b></td></tr>');              
                  delimiterAdded = true;
              }
              $('#RiskMapLegend').append('<tr class="legend_row" id="legend_row_tr_' + val[IDX_EVENT_ID] + '" onmouseover="LegendRowHoverWithHighlight(' + (isFirstChart ? 'window[\'plot1\']' : 'window[\'plot2\']') + ',this,1,3,' + index + ');" onmouseout="LegendRowHoverWithHighlight(' + (isFirstChart ? 'window[\'plot1\']' : 'window[\'plot2\']') + ',this,0,3,'+index+');" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; text-align:center; vertical-align: middle;">' + lbl_num + '<br/><img src="<% =ImagePath %>legend-10.png" width="10" height="10" title="" border="0" style="background:'+val[IDX_LABEL1].color+'">&nbsp;</td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px; width: 200px;" title="'+lbl+'" id="legend_row_' + index + '">'+lbl_short+'</td><td style="text-align:center; border:1px solid #ccc;" class="text small">'+ val[IDX_LABEL1].actual_likelihood + '%</td><td style="text-align:center; border:1px solid #ccc;">' + (ShowDollarValue ? showCost(val[IDX_LABEL1].dollar_impact, true, !showCents) : val[IDX_LABEL1].actual_impact + '%')+'</td><td style="text-align:center; border:1px solid #ccc;">' + (ShowDollarValue ? showCost(val[IDX_LABEL1].dollar_risk, true, !showCents): val[IDX_RISK]) + '</td></tr>');
          });
          $("#DivRiskMapLegend").show();
        }
        
        $("#DivRiskRegionsLegend").hide();
        if (!isEventCloud && !<% = Bool2JS(IsWidget) %>) {        
            $('#divRiskMapModes').show();
        } else {
            $('#divRiskMapModes').hide();
        }
        $("#RiskRegionsLegend").find("tr").remove();
        if (RiskMapShowRegions > 0 && !isEventCloud && !<% = Bool2JS(IsWidget) %>) {        
            $('#divRiskRegionsLegendHeader').html(isEventCloud ? "<%=ParseString("%%Risk%%")%>" : "<%=ResString("lblRiskRegions")%>");
            $('#RiskRegionsLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; width:18px; text-align:center;"><div style="width:12px;height:12px;background:'+_redBrush+'">&nbsp;</div></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+RHLabel+'" id="regions_legend_row_0">'+RHLabel+'</td></tr>');
            $('#RiskRegionsLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; width:18px; text-align:center;"><div style="width:12px;height:12px;background:'+_whiteBrush+'">&nbsp;</div></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+RMLabel+'" id="regions_legend_row_1">'+RMLabel+'</td></tr>');
            $('#RiskRegionsLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; width:18px; text-align:center;"><div style="width:12px;height:12px;background:'+_greenBrush+'">&nbsp;</div></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+RLLabel+'" id="regions_legend_row_2">'+RLLabel+'</td></tr>');
            $("#DivRiskRegionsLegend").show();
        }

        $("#DivAttributeColorsLegend").hide();
        $("#RiskAttrColorsLegend").find("tr").remove();
        if (ShowColorLegend && (ColorsLegend)) {  
          $.each(ColorsLegend, function(index, val) {
              //$('#RiskAttrColorsLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; width:18px; text-align:center;"><div style="width:12px;height:12px;background:'+val[0]+'">&nbsp;</div></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+val[1]+'" id="color_legend_row_'+index+'">'+val[1]+'</td></tr>');
              $('#RiskAttrColorsLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; width:18px; text-align:center;"><img src="../../images/risk/legend-10.png" width="10" height="10" title="" border="0" style="height:100%;background:'+val[0]+'"></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+val[1]+'" id="color_legend_row_'+index+'">'+val[1]+'</td></tr>');              
          });      
          $("#DivAttributeColorsLegend").show();
        }

        $("#DivAttributeShapesLegend").hide();
        $("#RiskAttrShapesLegend").find("tr").remove();
        if (ShowShapeLegend && (ShapesLegend)) {  
          $.each(ShapesLegend, function(index, val) {
              $('#RiskAttrShapesLegend').append('<tr onmouseover="LegendRowHover(this,1);" onmouseout="LegendRowHover(this,0);" valign="top"><td style="padding:1px 2px;border:1px solid #ccc; width:18px; text-align:center;"><img style="width:13px;height:13px;" src="../../images/risk/graph_shape'+val[0]+'.png"></td><td style="word-wrap:break-word; border:1px solid #ccc; padding:1px 2px;" title="'+val[1]+'" id="shape_legend_row_'+index+'">'+val[1]+'</td></tr>');
          });      
          $("#DivAttributeShapesLegend").show();
        }

        // Bind function to the highlight event to show the tooltip and highlight the row in the legend. 
        $('.plot').bind('jqplotDataHighlight', 
          function (ev, seriesIndex, pointIndex, data, radius) {                
            var isFirstChart = ev.target.id == "divRiskMap";
            var chart_left = $(this).offset().left, 
                chart_top = $(this).offset().top,
                x = (isFirstChart ? window["plot1"] : window["plot2"]).axes.xaxis.u2p(data[0]),  // convert x axis unita to pixels
                y = (isFirstChart ? window["plot1"] : window["plot2"]).axes.yaxis.u2p(data[1]);  // convert y axis units to pixels
            var color = '#ffff99';
            var pos_x = chart_left + x + 5;
            var w = document.getElementById('<% =RadPaneGrid.ClientID%>').clientWidth; //$('#divRiskMap').width();
            if ((w) && w > 100 && pos_x > (w - 300)) { pos_x -= 300; if (pos_x < 20) pos_x = 20; }
            var pos_y = y + 5;
            var h = document.getElementById('<% =RadPaneGrid.ClientID%>').clientHeight; //$('#divRiskMap').height();
            if ((h) && h > 100 && pos_y > (h - 120)) { pos_y -= 120; if (pos_y < 20) pos_y = 20; }
            $('#DivRiskMapTooltip').css({ left: pos_x, top: pos_y });            
            var header = data[IDX_LABEL2];  //(data[IDX_LABEL1].label != "" ? data[IDX_LABEL1].label : data[IDX_LABEL2]);
            //header = ShortString(header, 50);
            //var lblX =  AxisXTitle + ': ' + (!RiskMapSwitchAxes ? data[IDX_LABEL1].actual_likelihood : data[IDX_LABEL1].actual_impact) + (ShowDollarValue ? "" : '%');
            //var lblY =  AxisYTitle + ': ' + ( RiskMapSwitchAxes ? data[IDX_LABEL1].actual_likelihood : data[IDX_LABEL1].actual_impact) + (ShowDollarValue ? "" : '%');
            var lblX =  AxisXTitle + ': ' + data[IDX_LABEL1].actual_likelihood + '%';
            var lblY =  AxisYTitle + ': ' + (ShowDollarValue ? showCost(data[IDX_LABEL1].dollar_impact, true, !showCents) : data[IDX_LABEL1].actual_impact + '%');            
            $('#DivRiskMapTooltip').html('<h6>' + header + '</h6><b>' + lblRisk + ': ' + (ShowDollarValue ? showCost(data[IDX_LABEL1].dollar_risk, true, !showCents) : data[IDX_RISK]) + '</b><br />' + lblX + '<br />' + lblY);
            $('#DivRiskMapTooltip').show();
            if (RiskMapShowLegends) {
                //$("#DivRiskMapLegend").scrollTop($("#legend_row_" + pointIndex).offset().top);
                $('#RiskMapLegend tr').css('background', '#ffffff');
                //$('#RiskMapLegend tr').eq(pointIndex + 1 + (isFirstChart ? 0 : plot_data1.length)).css('background', color);
                //$("#legend_row_" + (pointIndex + (isFirstChart ? 0 : plot_data1.length)))[0].scrollIntoView(true);
                $('#legend_row_tr_' + data[IDX_EVENT_ID]).css('background', color);
                
                // - todo $("#legend_row_tr_" + data[IDX_EVENT_ID])[0].scrollIntoView(true);
                
                if (RiskMapShowRegions > 0 && !<% = Bool2JS(IsWidget) %>) {
                    var risk_prc = data[IDX_LABEL1].actual_risk;
                    var regionIndex = (risk_prc > _Rh ? 0 : (risk_prc > _Rl ? 1 : 2));
                    $('#RiskRegionsLegend tr').css('background', '#ffffff');
                    $('#RiskRegionsLegend tr').eq(regionIndex).css('background', color);
                    // - todo $("#regions_legend_row_" + regionIndex)[0].scrollIntoView(true);                    
                }
                if (ShowColorLegend) {
                    var color_cat = data[IDX_LABEL1].color_cat * 1;
                    $('#RiskAttrColorsLegend tr').css('background', '#ffffff');
                    $('#RiskAttrColorsLegend tr').eq(color_cat).css('background', color);
                    // - todo $("#color_legend_row_" + color_cat)[0].scrollIntoView(true);                    
                }
                if (ShowShapeLegend) {
                    var shape_cat = data[IDX_LABEL1].shape_cat * 1;
                    $('#RiskAttrShapesLegend tr').css('background', '#ffffff');
                    $('#RiskAttrShapesLegend tr').eq(shape_cat).css('background', color);
                    // - todo $("#shape_legend_row_" + shape_cat)[0].scrollIntoView(true);                    
                }
            }
        });
         
        // Bind a function to the unhighlight event to clean up after highlighting.
        $('.plot').bind('jqplotDataUnhighlight', 
            function (ev, seriesIndex, pointIndex, data) {
                //$('#DivRiskMapTooltip').empty();
                $('#DivRiskMapTooltip').hide();
                //$('#RiskMapLegend tr').css('background-color', '#ffffff');
            });
        
    }

    function loadChart(id, plot, data, titleExtra) {
        var x = "Axis X Name:";
        var y = "Axis Y Name:";
        document.getElementById("lblAxisXTitle").innerHTML = (RiskMapSwitchAxes ? y : x);
        document.getElementById("lblAxisYTitle").innerHTML = (RiskMapSwitchAxes ? x : y);
        
        //$("#divEventCloud").hide();
        $("#" + id).empty();

        plot = $.jqplot(id, [l2, l1, l0, data], {
            title: <% = Bool2JS(IsWidget)%> ? "<% = ParseString("%%Risk%% Heat Map")%>" : (RiskMapSwitchAxes ? AxisXTitle +" vs. "+ AxisYTitle : AxisYTitle +" vs. "+ AxisXTitle) + " " + with_controls_suffix + (typeof titleExtra == "undefined" ? "" : titleExtra)<%If CurrentPageID <> _PGID_RISK_PLOT_OVERALL AndAlso CurrentPageID <> _PGID_RISK_PLOT_OVERALL_WITH_CONTROLS Then%> + '<br/>With respect to <span class="divWRTHeader"><%=SafeFormString(If(IsRiskResultsByEventPage, SelectedEventName, SelectedHierarchyNodeName(True)))%></span>'<%End If%><% If IsWidget AndAlso Not String.IsNullOrEmpty(MapUserEmail) Then %> + ' (<% =JS_SafeString(MapUserName) %>)'<%End If%>,
            legend: {
                show: false, //!RiskMapShowLegends,
                placement: 'outsideGrid'
            },
            grid: {
                background: '#ffffff'
            },
            gridPadding: {
                right: reverseZoomRight > 0 ? reverseZoomRight : null,
                left: reverseZoomLeft > 0 ? reverseZoomLeft : null,
                top: reverseZoomTop > 0 ? reverseZoomTop : null,
                bottom: reverseZoomBottom > 0 ? reverseZoomBottom : null
                },
            //seriesColors: DATAPOINT_COLORS,
            /*highlighter: { show: true },*/
            series: [
            /*Red area*/
            {
                /*color: 'rgba(8,88,88,.6)',*/
                show: RiskMapShowRegions > 0,
                color: _redBrush,
                showMarker: false,
                showLine: RiskMapShowRegions == 1 || RiskMapShowRegions == 2,
                fill: RiskMapShowRegions == 1,
                fillAndStroke: RiskMapShowRegions == 1,
                label: RHLabel,
                shadow: false,
                rendererOptions: {
                    smooth: true,
                    highlightMouseOver: false,
                    highlightMouseDown: false
                }
            },
            /*Yellow area*/
            {
                show: RiskMapShowRegions > 0,
                color: RiskMapShowRegions == 2 ? "#aaaaaa" : _whiteBrush,
                showMarker: false,
                showLine: RiskMapShowRegions == 1 || RiskMapShowRegions == 2,
                lineWidth: 1, //RiskMapShowRegions == 2 ? 1.5 : 2,
                linePattern: RiskMapShowRegions == 2 ? 'dashed' : 'solid',
                fill: RiskMapShowRegions == 1,
                fillAndStroke: RiskMapShowRegions == 1,
                label: RMLabel,
                shadow: false,
                lineJoin: 'round',
                rendererOptions: {
                    smooth: true,
                    highlightMouseOver: false,
                    highlightMouseDown: false
                }
            },
            /*Green area*/
            {
                color: RiskMapShowRegions == 2 ? "#aaaaaa" : _greenBrush,
                show: RiskMapShowRegions > 0,
                showMarker: false,
                showLine: RiskMapShowRegions == 1 || RiskMapShowRegions == 2,
                lineWidth: 1, //RiskMapShowRegions == 2 ? 1.5 : 2,
                linePattern: RiskMapShowRegions == 2 ? 'dashed' : 'solid',
                fill: RiskMapShowRegions == 1,
                fillAndStroke: RiskMapShowRegions == 1,
                label: RLLabel,
                shadow: false,
                rendererOptions: {
                    smooth: true,
                    highlightMouseOver: false,
                    highlightMouseDown: false
                }
            },
            /*Bubble series*/
            {
                renderer: $.jqplot.BubbleRenderer,
                //showLabel: false, /* hide this series from chart legend */
                //shadow: !ShowShapeLegend,
                //shadowAlpha: 0.05,
                rendererOptions: {
                    autoscaleBubbles: !RiskMapBubbleSizeWithRisk || <% = Bool2JS(IsWidget)%>,
                    autoscaleMultiplier: bubble_mult, //(!RiskMapBubbleSizeWithRisk ? bubble_mult / 3 : bubble_mult),
                    //bubbleAlpha: 0.95,
                    //highlightAlpha: 0.8,
                    //bubbleGradients: true,
                    showLabels: RiskMapShowLabels,
                    highlightMouseOver: true,
                    highlightMouseDown: true
                }                
            }
        ],
        cursor: {
            show: true,
            tooltipLocation: 'sw',
            showTooltip: false,
            zoom: true,
            looseZoom: true,
            clickReset: true,
            dblClickReset: true,
            showVerticalLine: false,
            showHorizontalLine: false
        },
        axesDefaults: { tickRenderer: $.jqplot.CanvasAxisTickRenderer},
        axes: {
            xaxis: {
                min: MinX > 0 ? MinX : 0, //(ZoomPlot || (sManualZoomParams != "") ? MinX : 0),
                max: MaxX < 100 ? MaxX : 100,
                label: (RiskMapSwitchAxes ? AxisYTitle : AxisXTitle),
                tickOptions: ShowDollarValue && RiskMapSwitchAxes ? {   
                    fontSize: '8pt', 
                    enableFontSupport: true, 
                    showGridline: false,
                    formatter: tickCostFormatter
                } : { 
                    formatString: "%." + decimals + "f %%", 
                    fontSize: '8pt', 
                    enableFontSupport: true, 
                    showGridline: false 
                },
                renderer: $.jqplot.LinearAxisRenderer,
                labelRenderer: $.jqplot.CanvasAxisLabelRenderer                
                },
            yaxis: {
                min: MinY > 0 ? MinY : 0, //(ZoomPlot || (sManualZoomParams != "") ? MinY : 0),
                max: MaxY,
                autoscale: true,
                label: (RiskMapSwitchAxes ? AxisXTitle : AxisYTitle),                
                tickOptions: ShowDollarValue && !RiskMapSwitchAxes ? {   
                    fontSize: '8pt', 
                    enableFontSupport: true, 
                    showGridline: false,
                    formatter: tickCostFormatter
                } : { 
                    formatString: "%." + decimals + "f %%", 
                    fontSize: '8pt', 
                    enableFontSupport: true, 
                    showGridline: false 
                },
                renderer: $.jqplot.LinearAxisRenderer,
                labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
                rendererOptions: {
                    forceTickAt0: true
                }
            }
        }        
      });

      //var lnk = document.getElementById("downloadLnk"); if ((lnk)) lnk.onclick = saveAsImage;
      
        $("#<%=btnResetZoom.ClientID() %>").hide();
        $("#<%=btnResetZoom.ClientID() %>").prop('disabled', 'disabled').css({top:'10px', left:'80px'});
        plot.target.bind('jqplotZoom', function(ev, gridpos, datapos, plot, cursor){            
            isPlotZoomedbyUser = true;
            $("#<%=btnResetZoom.ClientID() %>").prop('disabled', false);
            $("#<%=btnResetZoom.ClientID() %>").show();
            plotExtraInit(plot);
            sManualZoomParams = plot.axes.xaxis.min + "," + plot.axes.xaxis.max + "," + plot.axes.yaxis.min + "," + plot.axes.yaxis.max;
            sendAjax("action=manual_zoom&params=" + sManualZoomParams);
        });
        
        plot.target.bind('jqplotResetZoom', function(ev, plot, cursor){            
            isPlotZoomedbyUser = false;
            $("#<%=btnResetZoom.ClientID() %>").prop('disabled', 'disabled');
            $("#<%=btnResetZoom.ClientID() %>").hide();
            plotExtraInit(plot);
            sManualZoomParams = "";
            showLoadingPanel();
            sendAjax("action=manual_zoom&params=" + sManualZoomParams);
        });
        
        if (sManualZoomParams != "") {
            isPlotZoomedbyUser = true;
            $("#<%=btnResetZoom.ClientID() %>").prop('disabled', false);
            $("#<%=btnResetZoom.ClientID() %>").show();
        }

        plotExtraInit(plot);

        //highlightWithControlsBubbles(plot);

        if (id == "divRiskMap") window["plot1"] = plot;
        if (id == "divRiskMapWC") window["plot2"] = plot;
    }

    //function highlightWithControlsBubbles(plot) {
    //    return false;
    //    if (ShowShapeLegend) return false;
    //
    //    var drawingCanvas = $(".jqplot-highlight-canvas")[0];
    //    if (!(drawingCanvas)) return false;
    //
    //    var ctx = drawingCanvas.getContext('2d');
    //    ctx.strokeStyle ="#ccc";
    //
    //    for (var s = 0; s < plot.series.length; s++) {
    //        var data = plot.series[s].data;
    //        var data_len = data.length;
    //        for (var i = 0; i < data_len; i++) {
    //            var x = plot.axes.xaxis.series_u2p(data[i][0]);
    //            var y = plot.axes.yaxis.series_u2p(data[i][1]);
    //            var radius = plot.series[s].gridData[i][2];
    //
    //            if (radius > 3 && data[i][IDX_EVENT_ID].indexOf("-") == 0) {
    //                //ctx.lineWidth = (radius < 10 ? 1 : (radius > 30 ? 3 : 2));
    //                ctx.lineWidth = Math.round(1 + bubble_mult);
    //                ctx.beginPath();
    //                ctx.setLineDash([5, 5]);
    //                ctx.arc(x, y, radius - ctx.lineWidth - 1, 0, Math.PI * 2, false);
    //                ctx.closePath();
    //                ctx.stroke();
    //                //ctx.lineWidth = (radius < 10 ? 1 : 2);
    //                //ctx.beginPath();
    //                //ctx.strokeStyle = COLOR_NEGATIVE_RADIUS;
    //                //ctx.arc(x,y,radius,0,Math.PI*2,false);
    //                //ctx.closePath();
    //                //ctx.stroke();
    //            }
    //        }
    //    }
    //}

    function plotGradientBackground(plot) {        
        if (RiskMapShowRegions < 2) return false;

        var canvas = $(plot.targetId).find('.jqplot-series-canvas')[0];
        if ((canvas)) {
            var ctx = canvas.getContext('2d');
        
            //ctx.clearRect(0,0,ctx.canvas.width, ctx.canvas.height);
        
            // Simple linear gradient
            //var grd = ctx.createLinearGradient(0, ctx.canvas.height, ctx.canvas.width, 0);
            //grd.addColorStop(1, _redBrush);
            //grd.addColorStop(0, _greenBrush);

            //return false;
        
            ctx.save();

            var pixel_size = 5;
            var p2 = pixel_size / 2;
            var width = ctx.canvas.width / pixel_size;
            var height = ctx.canvas.height / pixel_size;
            //var charMarginLeft = plot.axes.xaxis.u2p(ZoomPlot ? MinX : 0);
            //var charMarginTop = plot.axes.yaxis.u2p(ZoomPlot ? MaxY : 100);
            var charMarginLeft = plot.axes.xaxis.u2p(plot.axes.xaxis.min);
            var charMarginTop = plot.axes.yaxis.u2p(plot.axes.yaxis.max);
            var _Rl100 = _Rl * 100;
            var _Rh100 = _Rh * 100;

            for (x = 0; x < width; x++) {
                for (y = 0; y < height + pixel_size; y++) {
                    //var P = ( x * y / ratio) * 255;
                    //ctx.fillStyle = 'rgba(' + (255 - P) + ', 0, ' + P + ', 255)';
                    //ctx.fillStyle = 'rgba(0, 255, 0,' + P + ')';
                    //var riskValue = ((x + p2) / width) * (((height - (y + p2)) / height));
                    var px = plot.axes.xaxis.p2u(x * pixel_size + p2 + charMarginLeft);
                    var py = plot.axes.yaxis.p2u(charMarginTop + ctx.canvas.height - y * pixel_size - p2);
                    var riskValue =  px * py / 100;
                    ctx.fillStyle = heatMapColor(_greenBrush, _whiteBrush, _redBrush, _Rl100, _Rh100, riskValue, 3);
                    ctx.fillRect(x * pixel_size, ctx.canvas.height - y * pixel_size, pixel_size, pixel_size);
                    //ctx.fillstyle = '#000000';
                    //ctx.fillText(riskValue, x*pixel_size, ctx.canvas.height - y*pixel_size);
                }
            }
            //ctx.fillStyle = grd;
            //ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

            ctx.restore();
        }
    }

    function plotExtraInit(plot) {
        plotGradientBackground(plot);
        $("div.jqplot-bubble-label").css({"font-size" : "7.5pt", "margin-left" : "2px", "margin-top" : "2px"});
    }

    function resetZoom(plot) {
        isPlotZoomedbyUser = false;
        $("#<%=btnResetZoom.ClientID() %>").prop('disabled', 'disabled');
        $("#<%=btnResetZoom.ClientID() %>").hide();
        if ((window["plot1"]) && typeof window["plot1"].resetZoom == "function") window["plot1"].resetZoom();
        if ((window["plot2"]) && typeof window["plot2"].resetZoom == "function") window["plot2"].resetZoom();
        //if (ZoomPlot) {ZoomPlot = false; sendCommand("zoom_plot=false");} 
    }

    function resizeChart() { 
        var x = document.getElementById("divTitles");
        var m = document.getElementById('<% = RadPaneGrid.ClientID %>');
        var l = $('#DivRiskMapLegend');
        var r = $("#DivRiskRegionsLegend");
        var c = $("#DivAttributeColorsLegend");
        var s = $("#DivAttributeShapesLegend");
        var o = $("#divRiskMapModes");
        var t = $("#divToolbar");
        if ((m)) {   // (d) && 
            //if (isEventCloud) {                
            //    var canvas = document.getElementById("canvasEventCloud");
            //    if ((canvas)) {
            //        canvas.width = 600;                
            //        canvas.height = 250;
            //    }
            //} 

            var pb = <%If IsWidget Then%>$("#divPipeButtons")<%Else%>null<%End If%>;
            var qh = <%If IsWidget Then%>$("#divQHIcons")<%Else%>null<%End If%>;
            var w = document.getElementById('tdRiskMap').clientWidth - 35;
            var h = Math.floor(m.clientHeight - ((pb) && (pb.length) ? pb.height() + 8 : 0) - ((qh) && (qh.length) ? qh.height() + 2 : 0) - ((t) && (t.length) ? t.height() : 0)) - 15; //(is_firefox ? 15 : 215);
            if (w < 200) w = 200;
            if (h < 100) h = 100;
            //if (w>h) { h = w };
            //if (h>w) { w = h };
            
            // resize the plot/eventclound container elements
            var extra_height = ((x) ? Math.ceil(x.clientHeight + 2): 2) + 5;
            //if (isEventCloud) {                
            //    var canvas = document.getElementById("canvasEventCloud");
            //    if ((canvas)) {
            //        canvas.width = w;                
            //        canvas.height = h - 10 - extra_height;
            //    }
            //} else {
            
            $(".plot").hide();
            $("#divRiskMap").hide();
            $("#divRiskMapWC").hide();
            if (RiskPlotMode == 2) {
                $(".plot").height(h - extra_height - 30);
                $(".plot").width(Math.floor(w / 2));
                if (!isEventCloud) {
                    $(".plot").show();
                }
            } else {
                $("#divRiskMapWC").width(0).hide();
                $("#divRiskMap").height(h - extra_height - 30);
                $("#divRiskMap").width(w);
                if (!isEventCloud) {
                    $("#divRiskMap").show();
                }
            }
            //} 
            
            var coef = 1;
            if (ShowColorLegend) coef += 1;
            if (ShowShapeLegend) coef += 1;
            var lh = (h - 30 - (!<% = Bool2JS(IsWidget)%> ? (!isEventCloud ? (RiskMapShowRegions ? 68 + 100 : 100) : 0) - 15 : 0)) / coef;
            if (lh < 50) lh = 50;
            l.height(lh - extra_height);
            c.height(lh);
            s.height(lh);
            
            $('#DivRiskMapLegendInt').height(l.height()-18);
            $('#DivAttributeColorsLegendInt').height(c.height()-18);
            $('#DivAttributeShapesLegendInt').height(s.height()-18);
        }
        
        //setTimeout("highlightWithControlsBubbles(plot);", 300);
    }
    
    function LegendRowHover(r, hover) {
        if (r.style.background != OPT_LEGEND_HIGHLIGHT_COLOR) r.style.background = (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : "#ffffff");
    }

    function LegendRowHoverWithHighlight(plot, r, hover, sidx, pidx) {        
        if (r.style.background != OPT_LEGEND_HIGHLIGHT_COLOR) {
            $('tr.legend_row').css('background', '');
            r.style.background = (hover == 1 ? OPT_LEGEND_HIGHLIGHT_COLOR : '');
            if (hover == 1) {                
                if ((plot) && (plot.series) && (plot.series.length > 0) && (sidx < plot.series.length)) {
                    var points_len = plot.series[sidx].gridData.length;
                    if (pidx < points_len) {                    
                        highlightBubble(plot, sidx, pidx);
                    }
                }
            } else {
                unhighlightBubble(plot);
            }
        }
    }

    function highlightBubble (plot, sidx, pidx) {
        if (isEventCloud) return;

        plot.plugins.bubbleRenderer.highlightLabelCanvas.empty();
        var s = plot.series[sidx];
        var canvas = plot.plugins.bubbleRenderer.highlightCanvas;
        var ctx = canvas._ctx;
        ctx.clearRect(0,0,ctx.canvas.width, ctx.canvas.height);
        s._highlightedPoint = pidx;
        plot.plugins.bubbleRenderer.highlightedSeriesIndex = sidx;
        
        var color = s.highlightColorGenerator.get(pidx); 
        var x = s.gridData[pidx][0],
            y = s.gridData[pidx][1],
            r = s.gridData[pidx][2];
        ctx.save();
        ctx.fillStyle = color;
        ctx.strokeStyle = color;
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.arc(x, y, r, 0, 2*Math.PI, 0);
        ctx.closePath();
        ctx.fill();
        ctx.restore();        
        // bring label to front
        if (s.labels[pidx]) {
            plot.plugins.bubbleRenderer.highlightLabel = s.labels[pidx].clone();
            plot.plugins.bubbleRenderer.highlightLabel.appendTo(plot.plugins.bubbleRenderer.highlightLabelCanvas);
            plot.plugins.bubbleRenderer.highlightLabel.addClass('jqplot-bubble-label-highlight');
            plot.plugins.bubbleRenderer.highlightLabel.css("color", OPT_PLOT_LABEL_HIGHLIGHT_COLOR).css("font-weight", "bold");
        }

        //highlightWithControlsBubbles(plot);
    }
    
    function unhighlightBubble(plot) {
        if (!isEventCloud && (plot)) {
            var canvas = plot.plugins.bubbleRenderer.highlightCanvas;
            var sidx = plot.plugins.bubbleRenderer.highlightedSeriesIndex;
            plot.plugins.bubbleRenderer.highlightLabelCanvas.empty();
            canvas._ctx.clearRect(0,0, canvas._ctx.canvas.width, canvas._ctx.canvas.height);
            for (var i=0; i<plot.series.length; i++) {
                plot.series[i]._highlightedPoint = null;
            }
            plot.plugins.bubbleRenderer.highlightedSeriesIndex = null;
            plot.target.trigger('jqplotDataUnhighlight');
        }

        //highlightWithControlsBubbles(plot);
    } 

    function saveAxisTitle(title, isXaxis) {
        title = title.trim();
        if ((isXaxis && (title != AxisXTitle)) || (!isXaxis && (title != AxisYTitle))) {
            sendCommand((isXaxis?"axis_x_name=" :"axis_y_name=") + title);            
            if (isXaxis) { AxisXTitle = title } else { AxisYTitle = title };
        }
    }

    function setReverseZoomValue(value, dir) {
        value = value.trim();
        if (value == "") value = 0;
        var tvalue = 0;
        switch (dir) {
            case "top":
                tvalue = reverseZoomTop;
                break;
            case "left":
                tvalue = reverseZoomLeft;
                break;
            case "right":
                tvalue = reverseZoomRight;
                break;
            case "bottom":
                tvalue = reverseZoomBottom;
                break;
        }
        if (validInteger(value) && (value >= 0)) {
            if (tvalue != str2int(value)) {
                tvalue = str2int(value);
                switch (dir) {
                    case "top":
                        reverseZoomTop = tvalue;
                        break;
                    case "left":
                        reverseZoomLeft = tvalue;
                        break;
                    case "right":
                        reverseZoomRight = tvalue;
                        break;
                    case "bottom":
                        reverseZoomBottom = tvalue;
                        break;
                }
                sendAjax("action=reverse_zoom&value=" + tvalue + "&dir=" + dir);
            }
        }        
    }

    function InitRegionsParams(h, l, r, w, g) {
        document.getElementById("rbDollarInputType0").disabled = !DollarValueDefined;
        document.getElementById("rbDollarInputType1").disabled = !DollarValueDefined;
        //init text boxes with high and low values
        var tbHigh = $find("<% =RadNumericTextBoxRh.ClientID %>");
        var tbLow  = $find("<% =RadNumericTextBoxRl.ClientID %>");
        if ((tbHigh) && (tbLow)) {
            if (!DollarValueDefined || document.getElementById("rbDollarInputType0").checked) {
                tbHigh.set_maxValue(100);
                tbLow.set_maxValue(100);
                tbHigh.set_value(h);
                tbLow.set_value(l);
            } else {
                tbHigh.set_maxValue(<%=Integer.MaxValue%>);
                tbLow.set_maxValue(<%=Integer.MaxValue%>);
                tbHigh.set_value(h * DollarValueOfEnterprise / 100);
                tbLow.set_value(l * DollarValueOfEnterprise / 100);
            }
        }
        //init color pickers
        var cpHigh = $("#RadColorPickerHigh").dxColorBox("instance");
        var cpMid  = $("#RadColorPickerMid").dxColorBox("instance");
        var cpLow  = $("#RadColorPickerLow").dxColorBox("instance");
        if ((cpHigh) && (cpMid) && (cpLow)) {
            cpHigh.option("value", r);
            cpMid.option("value", w);
            cpLow.option("value", g);
        }
        <%--if (<%=Bool2JS(Not IsBowTiePage OrElse IsBowTieByObjectivePage)%> && SelectedHierarchyNodeID != "<%=PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes(0).NodeGuidID%>" && SelectedHierarchyNodeID != "<%=PM.Hierarchy(ECHierarchyID.hidImpact).Nodes(0).NodeGuidID%>") {
            $("#lblWRTNodeName").html("<%=ResString("lblRiskRegionsFor")%>" + " &quot;" + SelectedHierarchyNodeName + "&quot;");
        }--%>
    }

    function SaveRegionsParams() {
        var h, l, r, w, g;
        //get text boxes with high and low values
        var tbHigh = $find("<% =RadNumericTextBoxRh.ClientID %>");
        var tbLow  = $find("<% =RadNumericTextBoxRl.ClientID %>");
        //get color pickers
        var cpHigh = $("#RadColorPickerHigh").dxColorBox("instance");
        var cpMid  = $("#RadColorPickerMid").dxColorBox("instance");
        var cpLow  = $("#RadColorPickerLow").dxColorBox("instance");
        if ((tbHigh) && (tbLow) && (cpHigh) && (cpMid) && (cpLow)) {
            h = tbHigh.get_value() + 0;
            l =  tbLow.get_value() + 0;
            r = cpHigh.option("value");
            w =  cpMid.option("value");
            g =  cpLow.option("value");
            if (l <= h) {
                _redBrush = r;
                _whiteBrush = w;
                _greenBrush = g;
                if (document.getElementById("rbDollarInputType1").checked) {
                    h = h * 100 / DollarValueOfEnterprise;
                    l = l * 100 / DollarValueOfEnterprise;
                }
                _Rh = h / 100;
                _Rl = l / 100;
                sendCommand("regions_params=1&h="+h+"&l="+l+"&r="+r+"&w="+w+"&g="+g);
            } else {
                DevExpress.ui.notify("Rh value can't be lower than Rl", "error", OPT_MESSAGE_HIGHLIGHT_TIMOUT);
            }
        }
    }

    function InitRegionsDlg() {                
        dlg_regions = $("#divRegions").dialog({
              autoOpen: false,
              //width: $("#divRiskResultsMain").innerWidth()- 40,
              width: 500,
              //height: $("#divRiskResultsMain").innerHeight()- 40,
              //maxHeight: $("#divRiskResultsMain").innerHeight(),
              maxWidth: $("#divRiskResultsMain").innerWidth(),
              minWidth: 600,
              //minHeight: 400,
              modal: true,
              closeOnEscape: true,
              dialogClass: "no-close",
              bgiframe: true,
              title: "Regions Editor",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: [{ text:"Reset to defaults", "class": 'reset_button', click: function() { InitRegionsParams(DefaultRh * 100, DefaultRl * 100, DefaultRedBrush, DefaultWhiteBrush, DefaultGreenBrush); }},
                        { text:"Ok", click: function() { dlg_regions.dialog( "close" ); SaveRegionsParams(); }},
                        { text:"Cancel", click: function() { dlg_regions.dialog( "close" ); }}],
              open: function() {
                  $("body").css("overflow", "hidden");
                InitRegionsParams(_Rh * 100, _Rl * 100, _redBrush, _whiteBrush, _greenBrush);
              },
              close: function() { $("body").css("overflow", "auto"); }
        });
        $('.reset_button').css('margin-right', '90px');
    }

    function ShowRegionsDialog() {
        InitRegionsDlg();
        dlg_regions.dialog("open");
        // enableRiskRegionsSettings(RiskMapShowRegions != 0);
    }

    function convertRiskRegion() {
        InitRegionsParams(_Rh * 100, _Rl * 100, _redBrush, _whiteBrush, _greenBrush);
    }

    //function enableRiskRegionsSettings(is_enabled) {
    //    $("#tblRegionsSettings").fadeTo(0, is_enabled ? 1 : 0.5);
        <%--var cpHigh = $find("<% =RadColorPickerHigh.ClientID %>");
        var cpMid  = $find("<% =RadColorPickerMid.ClientID %>");
        var cpLow  = $find("<% =RadColorPickerLow.ClientID %>");
        var tbRh  = $find("<% =RadNumericTextBoxRh.ClientID %>");
        var tbRl  = $find("<% =RadNumericTextBoxRl.ClientID %>");
        
        if (is_enabled) {
            //$("#tblRegionsSettings").bind("click", function () { if(event.preventDefault) event.preventDefault(); else event.returnValue = false; return false;} );   
            if ((cpHigh) && (cpMid) && (cpLow) && (tbRh) && (tbRl)) {
                cpHigh.set_enabled();
                cpMid.set_enabled();
                cpLow.set_enabled();
                tbRh.set_enabled();
                tbRl.set_enabled();
            }
        } else {
            //$("#tblRegionsSettings").unbind("click");
            if ((cpHigh) && (cpMid) && (cpLow) && (tbRh) && (tbRl)) {
                cpHigh.set_disabled();
                cpMid.set_disabled();
                cpLow.set_disabled();
                tbRh.set_disabled();
                tbRl.set_disabled();
            }
        }--%>
    //}

    function setRiskMapRegiosnMode(mode) {
        RiskMapShowRegions = mode; 
        // enableRiskRegionsSettings(RiskMapShowRegions != 0);
        sendCommand("show_regions=" + mode);
        resizeChart();
    }

    function onSetColoringByCategory(checked) {
        ColorCodingByCategory = checked;
        var cb = $("#cbCategories");
        if ((cb)) cb.prop('disabled', !checked);        
        sendCommand("action=color_by_category&chk=" + (checked ? "1" : "0"));
    }

    function onSetShapeByCategory(checked) {
        var cb = $("#cbCategoriesShapes");
        if ((cb)) cb.prop('disabled', !checked);
        sendCommand("action=shape_by_category&chk=" + (checked ? "1" : "0"));
    }

    function onSetCategory(cat_guid) {
        sendCommand("action=select_category&cat_guid="+ cat_guid);
    }

    function onSetCategoryShape(cat_guid) {
        sendCommand("action=select_shape_category&cat_guid="+ cat_guid);
    }

    function onRiskLegendSortClick(col_idx) {
        sendCommand("action=risk_legend_sort_click&col_idx="+ col_idx);
    }

    /* #Region "BOW-TIE" */
    
    //< % = GetBowTieData() % >
    // selected_alt_data
    // likelihood_data
    // impact_data

    var dlg_applications = null;
        
    var isBowTie = <% =Bool2JS(IsBowTiePage()) %>;
    CanShowAllPriorities = <% =Bool2JS(CanShowAllPriorities()) %>;
    CanShowUpperLevelNodes = <% =Bool2JS(CanShowUpperLevelNodes) %>;
    CanShowBowTieBackground = <% =Bool2JS(CanShowBowTieBackground) %>;
    CanShowBowTieAttributes = <% =Bool2JS(CanShowBowTieAttributes) %>;
    <%--CanUserEditControls = <% =Bool2JS(CanUserEditControls) %>;--%>
    CanUserEvaluateControls = true;
    ShowTreatmentIndices = <%=Bool2JS(ShowTreatmentIndices)%>;
    ShowTreatmentNames = <%=Bool2JS(ShowTreatmentNames)%>;
    ShowAllNodesExpanded = <%=Bool2JS(IsRiskWithControlsPage)%>;
    //< % = GetControlsData() % > 
    // control_data

    var PARAM_DELIMITER = ";";

    function InitLabels() {
        var tbL = document.getElementById("tbLikelihoodOfCauses");
        var tbI = document.getElementById("tbImpactOfConsequences");
        var tbR = document.getElementById("tbRisk");
        if ((tbL) && (tbI) && (tbR)) {
            if ((selected_alt_data) && CanShowAllPriorities) {
                var sl = !UseSimulatedValues ? '<%=ResString("lblLikelihoodGivenEvent")%>' : '<%=ResString("lblLikelihoodGivenEventSimulated")%>';
                var si = !UseSimulatedValues ? '<%=ResString("lblImpactGivenEvent")%>' : '<%=ResString("lblImpactGivenEventSimulated")%>';
                var sr = '<%=ResString("lblBowTieRiskTitle")%>';
                tbL.innerHTML = replaceString("{0}", selected_alt_data[ALT_L], sl);
                tbI.innerHTML = replaceString("{0}", (!ShowDollarValue ? selected_alt_data[ALT_I] : showCost(str2double(selected_alt_data[ALT_I]), true, !showCents)), si);
                tbR.innerHTML = replaceString("{0}", (!ShowDollarValue ? selected_alt_data[ALT_R] : showCost(str2double(selected_alt_data[ALT_R]), true, !showCents)), sr);
            } else {
                tbL.innerHTML = "";
                tbI.innerHTML = "";
                tbR.innerHTML = "";
            }
            tbL.style.display = "";
            tbI.style.display = "";
            tbR.style.display = "";
        }
    }

        function saveAsImage() {
            <%If IsBowTiePage() Then %>
            var canvas = document.getElementById("BowTieViewChart");
            var lnk = document.getElementById("downloadLnk");
            if ((canvas) && (lnk) && (canvas.toDataURL)) {
                var img = canvas.toDataURL('image/jpeg');
                this.download = "<% =JS_SafeString(App.ActiveProject.ProjectName) %>.jpeg";
                this.href = img;
            }
            <%End If %>        
            <%If IsRiskMapPage() Then %>
            var imgelem = window["plot1"].jqplotToImageStr({}); //window["plot1"].jqplotToImageElem
            <%End If %>
        }

        function showHierarchy(hid) { // show the hierarchy (0 - Likelihood; 2 - Impact) in a modal window
            InitHierarchyDlg(hid);
            dlg_hierarchy.dialog("open");
        }

        function InitHierarchyDlg(hid) {                
            dlg_hierarchy = $("#divTree" + (hid == 0 ? "Likelihood" : "Impact")).dialog({
                autoOpen: false,
                width: 490,
                height: 350,
                modal: true,
                closeOnEscape: true,
                dialogClass: "no-close",
                bgiframe: true,
                title: (hid == 0 ? "<%=ParseString("%%Likelihood%%") %>" : "<%=ParseString("%%Impact%%") %>") + " hierarchy",
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: [{ text:"Close", click: function() { dlg_hierarchy.dialog( "close" ); }}],
                open:  function() { $("body").css("overflow", "hidden"); },
                close: function() { $("body").css("overflow", "auto"); }
            });
        }

    function initBowTieResources() {
        LegendA = "<%=ResString("lblRiskLegendA").Trim()(0)%>"; // L - Likelihood of Source 
        LegendB = "<%=ResString("lblRiskLegendB").Trim()(0)%>"; // V - Vulnerability 
        LegendC = "<%=ResString("lblRiskLegendC").Trim()(0)%>"; // C - Consequence of Event on Objective
        LegendD = "<%=ResString("lblRiskLegendD").Trim()(0)%>"; // P - Priority of Objective
        lblEmptyMessage = "<% =GetEmptyMessage()%>";
        lblNoSource = "<%=ResString("lblNoSpecificCause")%>";
        SourceGoalGuid = "<% =SourcesGoalNodeID() %>";
        lblControlActive = "<%=ResString("lblActive").ToLower%>";
        lblControlDeactivate = "<% =ResString("lblControlDeactivate")%>";
        lblControlActivate = "<% =ResString("lblControlActivate")%>";
        lblControlEdit = "<% =ResString("lblControlEdit")%>";
        lblControlEval = "<% =ResString("lblControlEvalEffectiveness")%>";
        lblControlDelete = "<% =ResString("lblControlDelete")%>";
        lblConfirmation = "<% =JS_SafeString(ResString("titleConfirmation")) %>";
        lblSureRemoveContribution = "<% =JS_SafeString(ResString("msgSureRemoveContribution")) %>";
        ImagesPath = "<% =ImagePath %>";
        _PGURL_RICHEDITOR = '<% =PageURL(_PGID_RICHEDITOR) %>';
        lblControl  = "<% =ParseString("%%Control%%") %>";
        lblControls = "<% =ParseString("%%Controls%%") %>";
        lblBEvent = "<%=ResString("tblAlternative")%>";
    }

    function evaluateControlEffectiveness(control_id, alt_id, node_id, appl_id) {
        loadURL('<%= PageURL(_PGID_EVALUATE_RISK_CONTROLS, _PARAM_ACTION + "=getstep") %>&retpage=' + pgID + '&mt_type=-1&control_id=' + control_id + '&alt_id=' + alt_id +'&node_id=' + node_id); //  + '&appl_id=' + appl_id
    }

    document.onclick = function () { if (is_context_menu_open == true) { $("div.context-menu").hide(200); is_context_menu_open = false; } }
    
    function btnDownloadLegendClick() {
        showLoadingPanel();
        $("#legendNameCol").css("width", "300px");
        html2canvas(document.querySelector('#RiskMapLegend'), {
                logging: false,
                scrollX: 0,
                scrollY: -5
            }).then(function(canvas) {
            hideLoadingPanel();
            download(canvas.toDataURL(), "<% = SafeFileName(App.ActiveProject.ProjectName) %>.png", "image/png");
        });
        $("#legendNameCol").css("width", "200px");
    }

</script>
</asp:Content>