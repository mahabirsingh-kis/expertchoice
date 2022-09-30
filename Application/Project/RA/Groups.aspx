<%@ Page Language="VB" Inherits="RAGroupsPage" title="Groups" Codebehind="Groups.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    <% = LoadGroupsData() %>
    var view_all_groups = "<% = ViewAllGroups() %>" == "True";
    
    var search_old = "";
    var SelectedGroupID = 0;

    var OPT_CAN_USER_ENABLE_GROUPS = true;

    var LBL_TOTAL_COST = "<%=ResString("lblCostTotal") %>";
    var LBL_SEL_COST = "<%=ResString("lblCostSelected") %>";
    var LBL_OUT_OF_COST = "<%=ResString("lblCostOutOf") %>";

    var alts_count = <%=CInt(IIf(IsPageForEventGroups, OptimizerNodeAlternatives.Count, OptimizerRAAlternatives.Count))%>;

    var IDX_GUID = 0;
    var IDX_NAME = 1;
    var IDX_TYPE = 2;    
    var IDX_INCLUDED_ALTS = 3;
    var IDX_IS_ENABLED = 4;
    
    var IDX_ALT_COST = 2;

    var COLOR_INACTIVE = "#777";
    var COLOR_DISABLED_LBL = "#cc4e5c";

    var showCosts = true;
    var smoothScrolling = true;
    var last_click_cb_left = "";
    var last_click_cb_right = "";
  
    function includeAlts() {
        var sData = "";
        var sel_alts = [];
        
        var g = selGroup();
        if ((g)) sel_alts = groups_list[SelectedGroupID][IDX_INCLUDED_ALTS];
        
        for (var i=0; i < alts_count; i++) {
            var o = eval("theForm.tpl" + i);            
            if ((o) && (o.checked) && (o.parentNode.parentNode.style.display != "none")) {
                o.checked = false;
                o.disabled = 1;
                sData += (sData == "" ? "" : "\n") + o.value;
                //var a = [o.value, all_alts_list[i][1]];
                var a = all_alts_list[i];
                sel_alts.push(a);                
            }
        }        
                
        if ((g)) groups_list[SelectedGroupID][IDX_INCLUDED_ALTS] = sel_alts;

        if (sData != "") {
            sendCommand("action=include&items=" + sData);
            return true;
        }
        return false;
    }

    function excludeAlts() {
        var sData = "";
        var sel_alts = [];
        
        var g = selGroup();
        if ((g)) sel_alts = groups_list[SelectedGroupID][IDX_INCLUDED_ALTS];
        
        var i = 0;
        var j = 0;

        while (i<sel_alts.length) {
            var alt = sel_alts[i];            
            var o = eval("theForm.tplLinked" + j);
            
            if ((o) && (o.checked)) {
                sel_alts.splice(i, 1);
                sData += (sData == "" ? "" : "\n") + o.value;
            } else {
                i += 1;
            }
            j += 1;
        }        
                
        if ((g)) groups_list[SelectedGroupID][IDX_INCLUDED_ALTS] = sel_alts;
        
        if (sData != "") {
            sendCommand("action=exclude&items=" + sData);
        }
    }

    function SelectAllAlts(sel) {
        for (var i=0; i < alts_count; i++) {
            var o = eval("theForm.tpl" + i);
            if ((o) && !(o.disabled == 1)) o.checked = sel;
        }
        InitButton(true);
    }

    function SelectLinkedAlts(sel) {
        for (var i=0; i < alts_count; i++) {
            var o = eval("theForm.tplLinked" + i);
            if ((o) && !(o.disabled == 1)) o.checked = sel; 
        }
        InitButton(false);
    }

    function onCheckboxClick(e, cb, id, isForAll, chk) {
        var last_click_cb = last_click_cb_right;
        if (isForAll) last_click_cb = last_click_cb_left;
        
        // process Shift + Click
        if (((typeof chk) != "undefined") && (e.shiftKey) && e.shiftKey && (last_click_cb != "") && (last_click_cb != id)) {
            var start_id = last_click_cb;
            var stop_id = id;
            var is_checking = false;
            var cb_class = "cb_linked";
            if (isForAll) cb_class = "cb_all";
            $("input." + cb_class).each(function() {
                var this_id = this.id + "";
                var is_found = this_id == start_id || this_id == stop_id;
                if (is_found && !is_checking) {
                    is_checking = true;
                } else {
                    if (is_found && is_checking) {
                        this.checked = chk;                      
                        is_checking = false;
                    }
                }
                if (is_checking) {
                    this.checked = chk;
                }
            });            
        }
        
        cb.focus();
        if (isForAll) { last_click_cb_left = id; } else { last_click_cb_right = id; }
        InitButton(isForAll);
    }

    function InitButton(isForAll) {
        var isDisabled = emptyGroupsList();
        var any_checked = false;
        var total_cost = 0.00;
        var sel_cost   = 0.00;
        var btnName = "btnInclude";
        var lblName = "<% =lblAllTotalCost.ClientID %>";
        
        var sel_alts = [];
        var g = selGroup();
        if ((g)) sel_alts = groups_list[SelectedGroupID][IDX_INCLUDED_ALTS];

        var alts = (isForAll ? all_alts_list : sel_alts);
        if (!isForAll) {
            btnName = "btnExclude";
            lblName = "<% =lblLinkedTotalCost.ClientID %>";
        }
        for (var i=0; i<alts.length; i++) {
            var o = eval("theForm.tpl"+ (isForAll ? "" : "Linked") + i);
            if ((o) && (o.checked) && !(o.disabled == 1)) {
                any_checked = true;
                if (alts[i][IDX_ALT_COST]) sel_cost += parseFloat(alts[i][IDX_ALT_COST]);
            }
            if ((o) && (alts[i][IDX_ALT_COST])) total_cost += parseFloat(alts[i][IDX_ALT_COST]);
        }            
        var btn = document.getElementById(btnName);
        if ((btn)) {
             if (any_checked && !(isDisabled == 1)) {btn.disabled = 0} else {btn.disabled = 1};
        }

        // show selected cost
        var lblAllTotalCost    = document.getElementById(lblName);      
        if ((lblAllTotalCost)) {
            if (any_checked == true) {
                lblAllTotalCost[text]    = LBL_SEL_COST + " " + number2locale(sel_cost, true) + " " + LBL_OUT_OF_COST + " " + number2locale(total_cost, true);
            } else {
                lblAllTotalCost[text]    = LBL_TOTAL_COST + " " + number2locale(total_cost, true);
            }
        }
    }
    
    function switchShowCosts() {
        var lbl1    = document.getElementById("<% =lblAllTotalCost.ClientID %>");
        var lbl2 = document.getElementById("<% =lblLinkedTotalCost.ClientID %>");
        showCosts = !showCosts;
        if (showCosts == true) {
            $("#<% =lblAllTotalCost.ClientID %>").show();
            $("#<% =lblLinkedTotalCost.ClientID %>").show();
        } else {
            $("#<% =lblAllTotalCost.ClientID %>").hide();
            $("#<% =lblLinkedTotalCost.ClientID %>").hide();
        }
    }

    function emptyGroupsList() {
        var isDisabled = 1;

        var cb = document.getElementById("cbGroups");
        if ((cb)) isDisabled = ((cb.childNodes.length > 0) ? 0 : 1);

        return isDisabled;
    }

    function UpdateButtons() {        
        var isDisabled = emptyGroupsList();

        var cb = document.getElementById("cbGroups");
        if ((cb)) cb.disabled = isDisabled;

        cb = document.getElementById("cbType");
        if ((cb)) cb.disabled = isDisabled;
        
        cb = document.getElementById("cbEnabled");
        if ((cb)) cb.disabled = isDisabled;
               
//        var btn = document.getElementById("btnDelGroup");
//        if ((btn)) btn.disabled = isDisabled;
        disableGlyphButton($("#btnDelGroup"), isDisabled);

//        btn = document.getElementById("btnCopyGroup");
//        if ((btn)) btn.disabled = isDisabled;              
        disableGlyphButton($("#btnCopyGroup"), isDisabled);

//        btn = document.getElementById("btnEditGroup");
//        if ((btn)) btn.disabled = isDisabled;
        disableGlyphButton($("#btnEditGroup"), isDisabled);

        disableGlyphButton($("#btnClearAll"), isDisabled);
    }
       
    function InitPage() {
        displayLoadingPanel(false);

        // init all alternatives
        var lbl = document.getElementById("divLeftPanel");
        if ((lbl) && (all_alts_list)) {
            lbl.innerHTML = "";
            for (var i=0; i<all_alts_list.length; i++) {                
                var a = all_alts_list[i];                
                lbl.innerHTML += "<div id='t" + i + "'><label><input type='checkbox' id='tpl" + i + "' name='tpl" + i + "' class='cb_all' value='" + a[IDX_GUID] + "' onclick='onCheckboxClick(event, this, this.id, true, this.checked);' onchange='InitButton(true);' onkeydown='InitButton(true);' ><span class='cansearch' clip_data='"+htmlEscape(a[IDX_NAME])+"'>" + htmlEscape(a[IDX_NAME]) + "</span></label></div>"; <%--onkeydown='onCheckboxClick(event, this, this.id, true, "undefined");' --%>
            }
        }

        var sel_alts = [];
        
        // init groups combobox        
        var cb = document.getElementById("cbGroups");
        if ((cb) && (groups_list)) {            
            while (cb.firstChild) {
               cb.removeChild(cb.firstChild);
            }

            for (var i=0; i<groups_list.length; i++) {                
               var v = groups_list[i];
               var opt = createOption(v[IDX_GUID], "#" + (i+1) + " " + v[IDX_NAME]);

               cb.appendChild(opt); 
               if (i == SelectedGroupID) {
                   var cb2 = document.getElementById("cbType");
                   if (cb2.options.length > v[IDX_TYPE]) cb2.selectedIndex = v[IDX_TYPE];
                   sel_alts = v[IDX_INCLUDED_ALTS];
                   var cbEn = document.getElementById("cbEnabled");
                   cbEn.checked = (((v[IDX_IS_ENABLED] == "1") || (v[IDX_IS_ENABLED] == "true")) ? true : false);
               }              
            }
        }        

        // hide the "Enabled" checkbox
        cb = document.getElementById("cbEnabled");
        var lbl = document.getElementById("lblEnabled"); 
        if ((cb) && (lbl)) {
            cb.style.display = (OPT_CAN_USER_ENABLE_GROUPS ? "inline" : "none");
            lbl.style.display = cb.style.display;
        }

        UpdatePage();
    }

    function UpdatePage() {
        var sel_alts = [];
        var sel_type = 0;
        var sel_enabled = false;
        
        var total_cost_all_alts = 0.00;
        var total_cost_sel_alts = 0.00;
        
        var any_group_exists = false;
        var any_alt_exists = false;

        // update groups combobox        
        var cb = document.getElementById("cbGroups");
        if ((cb)) {            
            for (var i=0; i<cb.childNodes.length; i++) {                               
               any_group_exists = true; 
               if (i == SelectedGroupID) {
                    cb.selectedIndex = i;

                    var opt = cb.childNodes[SelectedGroupID];                                        
                    sel_alts = groups_list[SelectedGroupID][IDX_INCLUDED_ALTS];
                    sel_type = groups_list[SelectedGroupID][IDX_TYPE];
                    var eg   = groups_list[SelectedGroupID][IDX_IS_ENABLED];
                    sel_enabled = (((eg == "1") || (eg == "true")) ? true : false);
                    
                    // calc cost of selected alts
                    for (var j=0; j<sel_alts.length; j++) {
                        if (sel_alts[j][IDX_ALT_COST]) total_cost_sel_alts += parseFloat(sel_alts[j][IDX_ALT_COST]);
                    }
                }
            }
        }
        
        // update all alternatives
        for (var i=0; i < alts_count; i++) {
            any_alt_exists = true;
            var o = eval("theForm.tpl" + i);
            if ((o)) {   
                o.checked = false;
                o.disabled = 0;
                for (var j=0; j<sel_alts.length; j++) {              
                    if (all_alts_list[i][IDX_GUID] == sel_alts[j][IDX_GUID]) o.disabled = 1; 
                }                
            }
        }

        // show the hint message if no alternatives
        var lbl = document.getElementById("divLeftPanel");
        if ((lbl) && !any_alt_exists) lbl.innerHTML = "<div id='infoLeft' style='text-align:center; vertical-align:middle; margin-top:40px;'><i><%=ResString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "msgNoControls", "msgNoAlternatives"))) %></i></div>";        

        // update selected alternatives        
        var lbl2 = document.getElementById("divAllGroups");
        if ((lbl2)) {            
            lbl2.innerHTML = "";
            if (any_group_exists) {
                lbl2.innerHTML = "<div id='infoRight' style='text-align:center; vertical-align:middle; margin-top:20px;'><%=String.Format(ResString("lblAddAlternativesHint"), CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, ParseString("%%controls%%"), ParseString("%%alternatives%%"))))%></div>";

                var sel_alts_len    = sel_alts.length;
                var groups_list_len = groups_list.length;            

                if (view_all_groups) {                
                    lbl2.innerHTML = "";
                    
                    document.getElementById("lblLinkedAlternatives")[text] = "<%=ResString("lblGroups") %>:";
                    if ((groups_list)) {
                        var s = "";
                        for (var j=0; j<groups_list_len; j++) {                
                            var v = groups_list[j];
                            
                            var img_name = "";
                            if (v[IDX_TYPE] == 0) img_name = "ra_group_any.png";
                            if (v[IDX_TYPE] == 1) img_name = "ra_group_single.png";
                            if (v[IDX_TYPE] == 2) img_name = "ra_group_combined.png";
                            if (v[IDX_TYPE] == 3) img_name = "ra_group_all_or_none.png";
                            
                            var is_selected = j == SelectedGroupID;
                            var img_group_type = "<img src='../../Images/ra/"+img_name+"' width='16' height='16' border='0' style='margin-top:1px;" + (is_selected ? "" : "opacity:0.4;filter:alpha(opacity=40);") + "' >";                        
                            var s_enabled = (OPT_CAN_USER_ENABLE_GROUPS ? "<img src='../../Images/ra/" + ((v[IDX_IS_ENABLED] == "1") || (v[IDX_IS_ENABLED] == "true") ? "online_tiny.gif' title='Enabled'" : "offline_tiny.gif' title='Disabled'") + " width='10' height='10' border='0' style='margin:3px;' >" : "");
                            var mousehandlers = (is_selected ? "" : " onmouseover='onGroupMouseover(this);' onmouseout='onGroupMouseout(this);'");
                            s += "<div id='DivGroup" + j + "' style='width:99%; padding-bottom:5px; border:" + (is_selected ? "2px solid " + COLOR_SELECTION_BORDER + ";" : "2px solid #fff;") + "' onclick='if (SelectedGroupID != " + j + ") { SelectedGroupID = " + j + "; last_click_cb_left = \"\"; last_click_cb_right = \"\"; UpdatePage(); }' " + mousehandlers + ">";
                            s += "<table width='100%' style='background:#f5f5f5;'><tr><td align='left' valign='top' width='20px'>" + img_group_type + "</td>";
                            s += "<td align='left' valign='top'><label class='text' style='text-align:left;font-size:11pt;" + (is_selected ? "color:#000;cursor:default;" : "color:"+COLOR_INACTIVE+";cursor:pointer;")+";'><b>#" + (j+1) + "&nbsp;" + htmlEscape(v[IDX_NAME]) + "</b></label></td>";
                            //s += "<td align='left'><h6 style='text-align:left;margin:6px 0px 0px 0px;" + (is_selected ? "color:#000;cursor:default;" : "color:"+COLOR_INACTIVE+";cursor:pointer;")+";'>" + htmlEscape(v[IDX_NAME]) + "</h6></td>";
                            s += "<td align='right' valign='top'>" + s_enabled + "</td></tr></table><div style='padding-left:20px;'>";
                            if ((v[IDX_INCLUDED_ALTS]) && (v[IDX_INCLUDED_ALTS].length > 0)) {
                                var alts_len = v[IDX_INCLUDED_ALTS].length;
                                for (var i=0; i<alts_len; i++) {   
                                    var a = v[IDX_INCLUDED_ALTS][i];                                
                                    if (is_selected) {
                                        // add clickable alternatives items
                                        s += "<div id='tLinked" + i + "'><label><input type='checkbox' id='tplLinked" + i + "' name='tplLinked" + i + "' class='cb_linked' value='" + a[IDX_GUID] + "' onclick='onCheckboxClick(event, this, this.id, false, this.checked);' onchange='InitButton(false);' onkeydown='InitButton(false);' >" + htmlEscape(a[IDX_NAME]) + "</label></div>"; <%--onkeydown='onCheckboxClick(event, this, this.id, true, "undefined");' --%>
                                    } else {
                                        // add readonly alternatives items
                                        s += "<div style='cursor:default;color:"+COLOR_INACTIVE+";'><label><input type='checkbox' id='cbAlt_" + j + "_" + i + "' value='" + a[IDX_GUID] + "' disabled='disabled'>" + htmlEscape(a[IDX_NAME]) + "</label></div>"; 
                                    }
                                }
                            } else {
                                s += "<div id='infoRight' style='text-align:center; vertical-align:middle; margin-top:6px; margin-bottom:6px;'><%=String.Format(ResString("lblAddAlternativesHint"), CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, ParseString("%%controls%%"), ParseString("%%alternatives%%"))))%></div>";
                            }
                            s += "</div></div>";
                        }
                        lbl2.innerHTML = s; 
                    }
                } else {
                    document.getElementById("lblLinkedAlternatives")[text] = "";
                    if (SelectedGroupID>=0) {
                        document.getElementById("lblLinkedAlternatives")[text] = "#" + (SelectedGroupID+1) + " " + ShortString(groups_list[SelectedGroupID][IDX_NAME], 48, false);
                    }
                    if ((sel_alts) && (sel_alts.length > 0)) {
                        lbl2.innerHTML = "";
                        for (var i=0; i<sel_alts_len; i++) {   
                            var a = sel_alts[i];                                
                            lbl2.innerHTML += "<div id='tLinked" + i + "'><input type='checkbox' id='tplLinked" + i + "' name='tplLinked" + i + "' class='cb_linked' value='" + a[IDX_GUID] + "' onclick='onCheckboxClick(event, this, this.id, false, this.checked);' onchange='InitButton(false);' onkeydown='InitButton(false);' ><label for='tplLinked" + i + "'>" + htmlEscape(a[IDX_NAME]) + "</label></div>"; <%--onkeydown='onCheckboxClick(event, this, this.id, true, "undefined");' --%>
                        }
                    }
                }
            }

            lbl2.innerHTML += "<div id='infoRight' style='text-align:center; vertical-align:middle; margin-top:20px;'><%=ResString("lblAddGroupsHint") %></div>";
        }

        // update type combobox
        var cb = document.getElementById("cbType");
        if ((cb)) {           
            if (cb.options.length > sel_type) cb.selectedIndex = sel_type;
            setGroupTypeImg(sel_type, false); 
        }

        // update enabled checkbox
        cb = document.getElementById("cbEnabled");
        if ((cb)) {           
            cb.checked = sel_enabled; 
        }

        // show total costs
        var lblAllTotalCost    = document.getElementById("<% =lblAllTotalCost.ClientID %>");
        var lblLinkedTotalCost = document.getElementById("<% =lblLinkedTotalCost.ClientID %>");
        
        if ((lblAllTotalCost) && (lblLinkedTotalCost)) {
            lblAllTotalCost[text]    = LBL_TOTAL_COST + " " + number2locale(total_cost_all_alts, true);
            lblLinkedTotalCost[text] = LBL_TOTAL_COST + " " + number2locale(total_cost_sel_alts, true);
        }

        InitButton(true); //btnInclude
        InitButton(false);//btnExclude
        UpdateButtons();        

        if (view_all_groups && any_group_exists) scrollToGroup(SelectedGroupID);
    }

    function selGroup() {
        var cb = document.getElementById("cbGroups");
        if ((cb) && (SelectedGroupID >= 0) && (cb.childNodes.length > 0)) return cb.childNodes[SelectedGroupID]; 
        return null;
    }

    function cbGroupsChange() {
        var cb = document.getElementById("cbGroups");
        if ((cb)) {
            SelectedGroupID = cb.selectedIndex;
            UpdatePage();
        }        
        last_click_cb_left = "";
        last_click_cb_right = "";
    }

    function delGroup() {
        sendCommand("action=del_group");         
    }

    function delAllGroups() {
        sendCommand("action=del_all_groups");         
    }

    function createGroup(sName) {               
        sendCommand('action=add_group&name='+encodeURIComponent(sName)+"&type="+grp_type+"&enabled="+grp_enabled);
    }

    function renameGroup(sName) {
        var g = selGroup();
        if ((g)) {
            newName = sName.trim();
            if ((newName) && (newName.length > 0)) {
                //if (newName!=g[text]) {
                    sendCommand("action=rename_group&name="+encodeURIComponent(newName)+"&type="+grp_type+"&enabled="+grp_enabled);
                    g.innerHTML = htmlEscape(newName);
                    
                    groups_list[SelectedGroupID][IDX_NAME] = newName;
                    groups_list[SelectedGroupID][IDX_TYPE] = grp_type;
                    groups_list[SelectedGroupID][IDX_IS_ENABLED] = grp_enabled;

                    //if (view_all_groups) 
                    UpdatePage();
                //}
            }            
        }
    }

    var grp_name = "";
    var grp_type = 0;
    var grp_enabled = true;

    function getGrpName(sName) {
        grp_name = sName;
        dxDialogBtnDisable(true, sName.trim() == "");
    }

    function addGroup() {
        inputGroupName("<%=ResString("titleCreateGroup") %>", "", "if (grp_name.trim().length>0) {createGroup(grp_name);} else {onCreateEmptyName();}");        
    }

    function onCreateEmptyName() {
        dxDialog("<%=ResString("msgGroupNameBlankCreate") %>", "addGroup();", "undefined", "<%=ResString("lblError") %>");
    }
    
    function onRenameEmptyName() {
        dxDialog("<%=ResString("msgGroupNameBlankRename") %>", "onEditName();", "undefined", "<%=ResString("lblError") %>");
    }

    function onEditName() {
        var g = selGroup();
        if ((g)) {
            inputGroupName("<%=ResString("titleEditGroup") %>", g[text], "if (grp_name.trim().length>0) {renameGroup(grp_name)} else {onRenameEmptyName()}");
            var sel = document.getElementById("cbCreateGroupType");
            if ((sel)) {
                var v_type = groups_list[SelectedGroupID][IDX_TYPE];
                if (sel.options.length > v_type) sel.selectedIndex = v_type;
                grp_type = v_type;
                setGroupTypeImg(v_type, true);
            }
            var cb = document.getElementById("cbCreateGroupEnabled");
            if ((cb)) {
                cb.checked = groups_list[SelectedGroupID][IDX_IS_ENABLED] == 1;                
            }
            grp_enabled = groups_list[SelectedGroupID][IDX_IS_ENABLED];
        }
    }

    function inputGroupName(title, oldName, onOkClick) {
        var sGrpNameEditor = "<norb><div><span style='float:left;width:100px;vertical-align:top;margin-top:2px;'><%=ResString("lblFormGroupName") %>&nbsp;</span><input type='text' class='input text' style='width:280px;' id='tbGroupName' value='" + replaceString("'", "&#39;", oldName) + "' onfocus='getGrpName(this.value);' onkeyup='getGrpName(this.value);' onblur='getGrpName(this.value);'></div></nobr>";
        sGrpNameEditor += "<nobr><div style='margin-top:5px;'><div style='float:left;width:100px;vertical-align:top;margin-top:6px;'><span style='vertical-align:top;'><%=ResString("lblFormGroupType") %></span>";
        sGrpNameEditor += "<img id='imgCreateGroupType0' src='../../Images/ra/ra_group_any.png' width='16' height='16' border='0' style='margin-left:20px;margin-top:2px;' ><img id='imgCreateGroupType1' src='../../Images/ra/ra_group_single.png'  width='16' height='16' border='0' style='display:none;margin-left:20px;margin-top:1px;' ><img id='imgCreateGroupType2' src='../../Images/ra/ra_group_combined.png' width='16' height='16' border='0' style='display:none;margin-left:20px;margin-top:1px;' ><img id='imgCreateGroupType3' src='../../Images/ra/ra_group_all_or_none.png' width='16' height='16' border='0' style='display:none;margin-left:20px;margin-top:1px;' ></div>";
        sGrpNameEditor += "<select class='select' id='cbCreateGroupType' style='width:280px; margin-right:3px; margin-top:5px;' onchange='setGroupTypeImg(this.selectedIndex,true); grp_type=this.selectedIndex;'>";
        sGrpNameEditor += "      <option value='0' style='color:#0000cc;'><%=String.Format(ResString("optGroupType0"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>";
        sGrpNameEditor += "      <option value='1' style='color:#993333;'><%=String.Format(ResString("optGroupType1"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>";
        sGrpNameEditor += "      <option value='2' style='color:#9900cc;'><%=String.Format(ResString("optGroupType2"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>";
        <%If OPT_ALLOW_ALL_OR_NONE_GROUP_TYPE Then%>sGrpNameEditor += "      <option value='3' style='color:#339933;'><%=String.Format(ResString("optGroupType3"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>";<%End If%>
        sGrpNameEditor += "</select></div></nobr>";
        sGrpNameEditor += "<div style='margin-left:99px; margin-top:3px;'><label class='text' style='text-align:left;' id='lblCreateGroupEnabled'><input type='checkbox' id='cbCreateGroupEnabled' checked='checked' onchange='grp_enabled=(this.checked?1:0);' ><%=ResString("lblGroupEnabled") %></label></div>";
        dxDialog(sGrpNameEditor, onOkClick, ";", title);
        var tbGroupName = document.getElementById("tbGroupName");
        if ((tbGroupName)) tbGroupName.focus();
        
        // hide the "Enabled" checkbox
        var cb = document.getElementById("cbCreateGroupEnabled");
        var lbl = document.getElementById("lblCreateGroupEnabled"); 
        if ((cb) && (lbl)) {
            cb.style.display = (OPT_CAN_USER_ENABLE_GROUPS ? "inline" : "none");
            lbl.style.display = cb.style.display;
        }
    }

    function copyGroup() {
        sendCommand("action=copy_group");
    }

    function enableGroup() {
        var cb = document.getElementById("cbEnabled");
        if ((cb)) {
            var param = cb.checked;
            var g = selGroup();
            if ((g)) {
                groups_list[SelectedGroupID][IDX_IS_ENABLED] = param;
                sendCommand("action=enable_group&enabled=" + param);
            }
        }
    }

    function cbTypeChange() {
        var cb = document.getElementById("cbType");
        if ((cb)) {
            var g = selGroup();
            if ((g)) { groups_list[SelectedGroupID][IDX_TYPE] = cb.selectedIndex; setGroupTypeImg(cb.selectedIndex, false) };
            sendCommand("action=set_type&type=" + cb.selectedIndex);
        }
    }

    function setGroupTypeImg(id, isCreateGroup) {
        $("#img" + (isCreateGroup ? "Create" : "") + "GroupType0").hide();
        $("#img" + (isCreateGroup ? "Create" : "") + "GroupType1").hide();
        $("#img" + (isCreateGroup ? "Create" : "") + "GroupType2").hide();
        $("#img" + (isCreateGroup ? "Create" : "") + "GroupType3").hide();
        $("#img" + (isCreateGroup ? "Create" : "") + "GroupType" + id).show();
    }

    function scrollToGroup(index) {
        if (smoothScrolling) {
            // jQuery scrolling with animation
            $("#divAllGroups")[0].scrollIntoView(true);
            $("#divAllGroups").animate({scrollTop: $("#divAllGroups").scrollTop() + $("#DivGroup"+index).offset().top - $("#divAllGroups").offset().top}, 600);   
        } else {
            // javascript scrollIntoView
            var g = document.getElementById("DivGroup"+index);
            if ((g)) g.scrollIntoView();
        }        
    }

    function createOption(guid, name) {
        var opt = document.createElement("option");
        opt.value= guid;
        opt.innerHTML = ShortString(name, 75);
        return opt;
    }

    function syncReceived(params) {
        var received_data = eval(params);
        var cmd = received_data[0];
        if (cmd == "add_group") {
            // add a new combobox item
            var cb = document.getElementById("cbGroups");
            if ((cb) && (groups_list)) {                            
                //add new group to groups_list
                var p = received_data[1];
                var name = decodeURIComponent(p[IDX_NAME]);
                var el = [p[IDX_GUID], name, p[IDX_TYPE], [], p[IDX_IS_ENABLED]]; //new group is enabled by default
                groups_list.push(el);

                var opt = createOption(p[IDX_GUID], name);

                cb.appendChild(opt); 
                cb.selectedIndex = cb.childNodes.length - 1;
                SelectedGroupID = cb.selectedIndex;
                
                // update group type
                var cb2 = document.getElementById("cbType");
                if (cb2.options.length > p[IDX_TYPE]) cb2.selectedIndex = p[IDX_TYPE];
            }     
            // update UI
            if (!includeAlts()) UpdatePage();
        }        
        
        if (cmd == "copy_group") {            
            // add a copy of group
            // ID, Name, Condition, sourceGroup.ID
            var cb = document.getElementById("cbGroups");
            if ((cb) && (groups_list)) {            
                var p = received_data[1];
                var name = decodeURIComponent(p[IDX_NAME]);
                var el = [p[IDX_GUID], name, p[IDX_TYPE], [], p[4]];                
                groups_list.push(el);

                var opt = createOption(p[IDX_GUID], name);

                // copy alternatives
                var g = selGroup();
                if ((g)) {
                    var sel_alts = groups_list[SelectedGroupID][IDX_INCLUDED_ALTS];
                    var copy_of_alts = sel_alts.slice();
                    groups_list[groups_list.length-1][IDX_INCLUDED_ALTS] = copy_of_alts;
                }

                cb.appendChild(opt); 
                cb.selectedIndex = cb.childNodes.length - 1;
                SelectedGroupID = cb.selectedIndex;
                
                // update group type
                var cb2 = document.getElementById("cbType");
                if (cb2.options.length > p[IDX_TYPE]) cb2.selectedIndex = p[IDX_TYPE];
            }     
            // update UI
            UpdatePage();
        }        
        
        if (cmd == "include" || cmd == "exclude" || cmd == "enable_group") {
            UpdatePage();
        }

        if (cmd == "del_group") {
            // remove selected combobox item
            var cb = document.getElementById("cbGroups");
            if ((cb) && (groups_list)) {            
                //remove from groups_list
                groups_list.splice(SelectedGroupID, 1);                

                //remove from combobox items
                var cb = document.getElementById("cbGroups");
                if ((cb) && (cb.selectedIndex >= 0)) {                       
                    SelectedGroupID = 0;            
                    cb.removeChild(cb.childNodes[cb.selectedIndex]); 
                    if (cb.childNodes.length > 0) cb.selectedIndex = 0;
                }
                
            }     
            // update UI            
            UpdatePage();
        }

        if (cmd == "del_all_groups") {
            groups_list.splice(0, groups_list.length);            
            var cb = document.getElementById("cbGroups");
            if ((cb)) {
                while (cb.firstChild) {
                   cb.removeChild(cb.firstChild);
                }
            }
            UpdatePage();
        }

        if (cmd == "view_all_groups") {
            UpdatePage();
        }

        if (view_all_groups && (cmd == "set_type")) {
            UpdatePage();
        }

        last_click_cb_left = "";
        last_click_cb_right = "";

        displayLoadingPanel(false);
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function confrimDeleteGroup() {
        dxConfirm(resString("msgSureDeleteGroup"), "delGroup();");
    }

    function confrimDeleteAllGroups() {
        dxConfirm(resString("msgSureDeleteAllGroups"), "delAllGroups();");
    }

    function onSetScenario(id) {
        displayLoadingPanel(true);
        document.location.href='<% =PageURL(CurrentPageID) %>?action=scenario<% =GetTempThemeURI(true) %>&sid='+ id;
        return false;
    }

    /* jQuery Ajax */

    function sendCommand(params) {
        params += "&group_index=" + SelectedGroupID;
        displayLoadingPanel(true);

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);        
    }

    function onViewAllGroupsChecked(chk) {
        view_all_groups = chk;
        sendCommand("action=view_all_groups&value=" + (chk ? "1" : "0"));
    }

    function onGroupMouseover(div) {
        div.style.background = BKG_DEFAULT_HOVER;
    }

    function onGroupMouseout(div) {
        div.style.background = '#fff';
    }

    function onSearch(value) {
        value = value.trim().toLowerCase();
        if (value == search_old) return false;
        if (value == "") {
            $("span.cansearch").each(function (index, cell) {
                var attr = cell.getAttribute("clip_data");
                if ((attr)) cell.innerHTML = htmlEscape(attr);
                cell.parentNode.parentNode.style.display = "";
            });
        } else {
            $("span.cansearch").each(function (index, cell) { 
                var r = htmlEscape(new RegExp(value, "gi"));
                var attr = cell.getAttribute("clip_data");
                var showRow = false;
                if ((attr)) {
                    cell.innerHTML = htmlEscape(attr);
                    if ((attr.toLowerCase().indexOf(value) >= 0)) {
                        cell.innerHTML = cell.innerHTML.replace(r, "<span style='font-weight:bold; color:#0066cc;'>$&</span>");
                        showRow = true;
                    }
                }
                cell.parentNode.parentNode.style.display = (showRow ? "" : "none");
            });
        }
        search_old = value;
    }

    function InitSearch() {
        $("#tbSearch").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
            if (oldValue == "") return;
            setTimeout(function(){ if ($input.val() == "") onSearch(""); }, 100);
        });
        setTimeout("document.getElementById(\"tbSearch\").disabled = false; document.getElementById(\"tbSearch\").focus();", 250);
    }

    function resizePanels() {
        $("#divLeftPanel").height(30);
        $("#divAllGroups").height(30);
        $("#divLeftPanel").height($("#tdLeftPanel").innerHeight() - 15);
        $("#divAllGroups").height($("#tdLeftPanel").innerHeight() - 15);        
    }

    resize_custom = resizePanels;

    $(document).ready(function () {
        $("#divMainContent").css("overflow", "hidden");
        $(".mainContentContainer").css({ "overflow" : "hidden"});
        resizePanels();
        setTimeout("resizePanels();", 1000);
    });

</script>

<table border='0' cellspacing="0" cellpadding="2" style='width:1000px; height:100%' id='tblMain'>
    <tr style='height:2em'>
        <td colspan='3' valign="top">
        <div id='divToolbar' class='text' style="margin: 0px; padding: 2px;">
            <table class="text">
                <tr valign="middle">
                    <td class="ec_toolbar_td_separator text" valign="middle">
                        <span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span>
                        <%=GetScenarios%>
                    </td>
                    <td class="ec_toolbar_td text" valign="middle">                       
                        <%-- Custom modern switch small - View All Groups --%>
                        <nobr><div style="display: inline-block; position:relative; margin-top: -7px; padding-right:3px; height:14px; text-align: right; vertical-align: middle;"><span class="text"><%=ResString("btnViewAllGroups")%></span></div><label class="ec_switch small_element" style="position:relative;">
                          <input id="btnViewAllGroups" type="checkbox" <%=CStr(IIf(ViewAllGroups, " checked='checked' ", ""))%> onclick="onViewAllGroupsChecked(this.checked);">
                          <span class="ec_slider small"></span>
                        </label>
                        </nobr>
                    </td>
                </tr>
            </table>
        </div>

        <h5 style='padding: 0;' title="<% =SafeFormString(RA.Scenarios.ActiveScenario.Description) %>"><%=ResString("lblGroupsTitle")%>&nbsp;&quot;<%=SafeFormString(ShortString(RA.Scenarios.ActiveScenario.Name, 45, True))%>&quot;</h5>
        </td>
    </tr>
    <tr>
        <td colspan="3" class="text" align="center" valign="top" style="margin: 0px 2px; padding: 12px; padding-top: 4px;"><nobr>
              <span style="vertical-align: middle;"><b>Group name</b>:</span>
              <select id='cbGroups' disabled="disabled" style='width:30ex; margin-right:4px; vertical-align: middle;' onchange='cbGroupsChange();'></select>&nbsp;&nbsp;
              <button disabled="disabled" id='btnEditGroup' onclick="onEditName(); return false;" title="<%=ResString("btnGroupEditName") %>"><i class="fas fa-pencil-alt"></i></button>
              <button disabled="disabled" id='btnDelGroup'  onclick='confrimDeleteGroup(); return false;'  title="<%=ResString("btnGroupRemove") %>"><i class="fas fa-minus"></i></button>
              <button                     id='btnAddGroup'  onclick='addGroup(); return false;'  title="<%=ResString("btnGroupAdd") %>"><i class="fas fa-plus"></i></button>
              <button disabled="disabled" id='btnCopyGroup' onclick='copyGroup(); return false;' title="<%=ResString("btnGroupCopy") %>"><i class="fas fa-clone"></i></button>
              <button disabled="disabled" id='btnClearAll'  onclick='confrimDeleteAllGroups(); return false;'  title="<%=ResString("btnGroupRemoveAll") %>"><i class="fas fa-broom"></i></button>
              <span style='padding-left:20px; vertical-align: middle;'><b><%=ResString("lblFormGroupType")%></b></span>
              <div style="display: inline-block; vertical-align: middle;">
                <img id='imgGroupType0' src="../../Images/ra/ra_group_any.png" width="16" height="16" border="0" ><img id='imgGroupType1' src="../../Images/ra/ra_group_single.png"  width="16" height="16" border="0" ><img id='imgGroupType2' src="../../Images/ra/ra_group_combined.png" width="16" height="16" border="0" ><img id='imgGroupType3' src="../../Images/ra/ra_group_all_or_none.png" width="16" height="16" border="0" />
              </div>
              <select class='select' id='cbType' disabled="disabled" style='margin-right:3px;  vertical-align: middle;' onchange='cbTypeChange();'>
                    <option value='0' style='color:#0000cc;'><%=String.Format(ResString("optGroupType0"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>
                    <option value='1' style='color:#993333;'><%=String.Format(ResString("optGroupType1"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>
                    <option value='2' style='color:#9900cc;'><%=String.Format(ResString("optGroupType2"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option>
                    <%If OPT_ALLOW_ALL_OR_NONE_GROUP_TYPE Then%><option value='3' style='color:#339933;'><%=String.Format(ResString("optGroupType3"), ParseString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "%%controls%%", "%%alternatives%%"))))%></option><%End If%>
              </select>
              <label class='text' id='lblEnabled'><input type='checkbox' id='cbEnabled' onclick='enableGroup();' class='text' /><%=ResString("lblGroupEnabled") %></label>
        </nobr></td>
    </tr>
    <tr>
        <td style='padding:1ex 0px 0px 1ex'><nobr><span style='float:right;' class='text'><a href='' onclick='SelectAllAlts(1); return false;' class='actions'><%=ResString("lblAll")%></a> | <a href='' onclick='SelectAllAlts(0); return false;' class='actions'><%=ResString("lblSelectNone")%></a></span><div class="text" style="float:right; margin-right:20px; margin-top:-3px;"><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearch" class="input" style="width:130px;" value="" onkeyup="onSearch(this.value)"/></label></div><h6 style='text-align:left; margin:0px 0px; padding:0px; white-space:nowrap'><%=ResString(CStr(IIf(App.ActiveProject.IsRisk AndAlso Not IsPageForEventGroups, "lblAllControls", "lblAllAlternatives")))%></h6></nobr></td>
        <td>&nbsp;</td>
        <td style='padding:1ex 0px 0px 1ex'><nobr><span style='float:right;' class='text'><a href='' onclick='SelectLinkedAlts(1); return false;' class='actions'><%=ResString("lblAll")%></a> | <a href='' onclick='SelectLinkedAlts(0); return false;' class='actions'><%=ResString("lblSelectNone")%></a></span><h6 id='lblLinkedAlternatives' style='text-align:left; margin:0px 0px; padding:0px; white-space:nowrap'></h6></nobr></td>
    </tr>
    <tr style='height: 100%'>
        <td style='width:460px; height:100%;' valign='top' id="tdLeftPanel"><div id="divLeftPanel" class="text" style='overflow:auto; border:1px solid #d0d0d0'><%--<asp:Label runat="server" ID="lblAllAlts" CssClass="text"/>--%></div></td>
        <td style='padding:0px 1em 5em 1em;' align="center" valign="middle">
            <input type='button' disabled="disabled" class='button' style='width:50px; height:50px; background:#f0f0f0; font-size:20pt;' id='btnInclude' value='&raquo;' onclick='includeAlts(); return false;' /><br>
            <input type='button' disabled="disabled" class='button' style='width:50px; height:50px; background:#f0f0f0; font-size:20pt; margin:8px 0px;' id='btnExclude' value='&laquo;' onclick='excludeAlts(); return false;' />
        </td>
        <td style='width:460px; height:100%;' valign='top' id="tdRightPanel"><div id="divAllGroups" class="text" style='overflow: auto; border: 1px solid #d0d0d0'><%--<asp:Label runat="server" ID="lblLinkedAlts" CssClass="text" />--%></div></td>
    </tr>
    <% If CanShowCosts Then %>
    <tr>
        <td align="left" class="text"><span style='float:right;padding-top:3px;'><asp:Label runat="server" ID="lblAllTotalCost"/></span><label><input type="checkbox" id="cbShowCosts" checked="checked" onclick="switchShowCosts();" /><%=ResString("lblCostShow")%></label></td>
        <td>&nbsp;</td>
        <td align="right" class='text'><asp:Label runat="server" ID="lblLinkedTotalCost" CssClass="text" /></td>
    </tr><%End If%>
</table>

<script language='javascript' type='text/javascript'>
    checkGlyphButtons();
    InitPage();
    InitSearch();    
</script>

</asp:Content>