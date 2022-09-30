<%@ Page Language="VB" Inherits="RACustomConstraints" title="Custom Constraints" Codebehind="CustomConstraints.aspx.vb" %>
<%@ Import Namespace="Canvas.RAGlobalSettings" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<%--<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>--%>
<%--<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" %>--%>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<script language="javascript" type="text/javascript">

    var on_error_code = "";
    var on_ok_code = "";
    var is_ajax = 0;

    var scenarios = [<% =GetScenarios() %>];
    var scenario_active = <% =RA.Scenarios.ActiveScenarioID %>;

    var constraints = [<% =GetConstraints() %>];
    var attribs_cat = [<% =GetAttributes(True) %>];
    var attribs_num = [<% =GetAttributes(False) %>];

    var IDX_CC_ID       = 0;
    var IDX_CC_NAME     = 1;
    var IDX_CC_ENABLED  = 0;

    var on_hit_enter = "";
    var on_hit_esc = "";

    var OPT_SCENARIO_LEN     = 45; //A0965
    var OPT_DESCRIPTION_LEN = 200; //A0965

    var is_grag_n_drop = false;

    function enc(sVal) {
        return encodeURIComponent(sVal);
    }

    function getToolbarButton(btn_id) {
        var toolbar = $find("<%=RadToolBarMain.ClientID %>");
        if ((toolbar)) {
            var el = toolbar.findItemByValue(btn_id);
            if ((el)) return el;
            return toolbar.findItemByText(btn_id);
        }
        return "undefined";
    }

    <%--function PleaseWait(vis)
    {
        if ((vis)) {
            document.body.style.cursor = "wait";
            setTimeout('if (is_ajax) SwitchWarning("<% =pnlLoadingPanel.ClientID %>", ' + vis + ');', 300);
        }
        else {
            SwitchWarning("<% =pnlLoadingPanel.ClientID %>", vis);
            document.body.style.cursor = "default";
        }
//        theForm.disabled = vis;
    }--%>
    
    function onClickToolbar(sender, args) {
        var button = args.get_item();
        if ((button))
        {
            var btn_id = button.get_value();
            if ((btn_id))
            {
                switch (btn_id+"") {
                    case "delete_all":
                        DeleteAllConstraints();
                        break;
                    case "delete":
                        var ids = "";
                        var sel = GetSelected();
                        if (sel.length>0) {
                            if (sel.length==1) {
                                DeleteConstraint(sel[0]);
                            } else {
                                for (var i=0; i<sel.length; i++) ids += (ids=="" ? "" : ",") + sel[i];
                                DeleteSelectedConstraints(ids);
                            }
                        }
                        break;
                    case "import_cat":
                        ImportConstraints(1);
                        break;
                    case "import_num":
                        ImportConstraints(0);
                        break;
                    case "sync":
                        SyncConstraints();
                        break;
                }
            }
        }
    }

    function GetSelected() {
        var lst = [];
        for (var i=0; i<constraints.length; i++) {
            if (eval("theForm.Select" + i + ".checked")) Array.add(lst, constraints[i][0]);
        }
        return lst;
    }

    function UpdateButtons() {
        var da = getToolbarButton("delete_all");
        if ((da)) da.set_enabled(constraints.length>0);

        var sel = 0;
        var unsel = 0;
        var lnk = 0;
        for (var i=0; i<constraints.length; i++) {
            if (eval("theForm.Select" + i + ".checked")) {
                sel += 1;
                if (constraints[i][3]>0) lnk+=1;
            } else {
                unsel += 1;
            }
        }

        var s = getToolbarButton("sync");
        if ((s)) s.set_enabled(lnk>0);
        var d = getToolbarButton("delete");
        if ((d)) d.set_enabled(sel>0);
        theForm.SelectAll.checked = (sel == constraints.length && sel>0);
        $("#SelectAll").prop("indeterminate", (sel>0 && unsel>0) ? "1" : "");

        var has_sel = false;
        var has_unsel = false;
        for (var i=0; i<constraints.length; i++) {
            if (constraints[i][2]==1) has_sel = true; else has_unsel = true;
        }
        $("#cbIgnoreAll").prop("checked", (has_sel || !constraints.length ? "" : "checked")).prop("disabled", (constraints.length ? "" : "disabled")).prop("indeterminate", (has_sel && has_unsel) ? "1" : "");
    }

    function switchAllCC(chk) {
        on_ok_code = 'ReInitData(data);'; 
        sendAJAX('action=enable_all&val=' + (chk ? 0 : 1));
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", on_agree + ";");
            return false;
        }
        return true;
    }

    function sendAJAX(params) {
        is_ajax = 1;
        //PleaseWait(1);
        displayLoadingPanel(true);
        $.ajax({
            type: "GET",
            data: "ajax=yes&" + params + "&r=" + Math.random(),
            dataType: "text",
            async:true,
            beforeSend: function () {
                // check data if needed and "return false" to cancel
                return true;
            },
            success: function (data) {
               //PleaseWait(0);
               displayLoadingPanel(false);
               receiveAJAX(data);
            },
            error: function () {
                //PleaseWait(0);
                displayLoadingPanel(false);
                DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
            }
        });
    }

    function receiveAJAX(data) {
        is_ajax = 0;
        if ((typeof data!="undefined")) {    
        }
        //PleaseWait(0);
        displayLoadingPanel(false);
        resizeGrid();   
        if ((on_ok_code!="")) eval(on_ok_code);
        on_ok_code = "";
    }

    function InitConstraints() {
        theForm.SelectAll.disabled = (constraints.length == 0);
        var t = $("#tblConstraints tbody");
        var ro = " (<% =JS_SafeString(ResString("lblRACCReadOnly")) %>)";
        var has_links = false;
        if ((t)) {
            t.html("");
            for (var i=0; i<constraints.length; i++) {
                sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "'>";
                sRow += "<td align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td><input type='checkbox' id='Select" + i + "' value='1' onclick='UpdateButtons();'></td>";
                sRow += "<td id='tdCName" + i + "'>" + ((constraints[i][3]>0) ? "<a href='' onclick='SwitchConstrReadOnly(" + i + "); return false;' title='" + constraints[i][4] + (constraints[i][3] == 2 ? ro : "") +  "' style='margin-left:8px;float:right'>" + (constraints[i][3]==2 ? "<i class='fas fa-link'></i>" : "<i class='fas fa-link' style='color: #ccc;'></i>") + "</a>" : ((constraints[i][3]<0) ? "<a href='' onclick='ResetConstrlLink(" + i + "); return false;'><i class='fas fa-link' style='color: red; float:right; margin-left:8px' title='<% = SafeFormString(ResString("lblRABrokenLink")) %>'></i></a>" : "")) +  htmlEscape(constraints[i][1]) + "</td>";
                <% If RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then%>sRow += "<td id='tdCEnabled" + i + "' align='center'><input type='checkbox' id='tbCEnabled" + i + "'" + ((constraints[i][2]!=1) ? " checked" : "") + " title='' onclick='return onEnableConstraint(" + i + ", this.checked);'></td>";<% End if %>
                <% If isTimePeriodsAvailable() Then%>sRow += "<td id='tdCResource" + i + "' align='center'><input type='checkbox' id='tbCResource" + i + "'" + ((constraints[i][7]==1) ? " checked" : "") + " title='' onclick='return onResource(" + i + ", this.checked);'></td>";<% End if %>
                sRow += "<td id='tdCActions" + i + "' align='center'><a href='' onclick='onEditConstraint(" + i + "); return false;' title='<% =JS_SafeString(ResString("lblRAEditCCName")) %>'><i class='fas fa-pencil-alt'></i></a>&nbsp;<a href='' onclick='DeleteConstraint(" + constraints[i][0] + "); return false;' title='<% =JS_SafeString(ResString("lblRADeleteCCName")) %>'><i class='fas fa-ban'></i></a></td></tr>";
                //sRow += "<td id='tdCActions" + i + "' align='center'><a href='' onclick='onEditConstraint(" + i + "); return false;'><img src='<% =ImagePath %>edit_small.gif' width=16 height=16 border=0 alt='<% =JS_SafeString(ResString("lblRAEditCCName")) %>'></a></td></tr>";
                t.append(sRow);
                if ((constraints[i][3]==2)) has_links = true;
            }
            sRow = "<tr class='text grid_footer' id='trCNew'>";
            sRow += "<td colspan='2'>&nbsp;</td>";
            sRow += "<td><input type='text' style='width:100%;' id='tbCName' maxlength=120 onfocus='on_hit_enter=\"EditConstraint(-1);\"; on_hit_esc = \"theForm.reset();\"; ' onblur=''on_hit_enter=\"\"\; on_hit_esc = \"\";'></td>";
            <% If RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then%>sRow += "<td align='center'><input type='checkbox' id='tbCEnabled' title=''></td>";<% End If %>
            <% If isTimePeriodsAvailable() Then%>sRow += "<td align='center'><input type='checkbox' id='tbCResource' title=''></td>";<% End If %>
            sRow += "<td align='center'><a href='' onclick='EditConstraint(-1); return false;' title='<% =JS_SafeString(ResString("lblRAAddCCName")) %>'><i class='fas fa-plus'></i></a></td></tr>";
            t.append(sRow);
            var b = document.getElementById("btnDelAllContraints");
            if ((b)) b.disabled = (constraints.length==0);
            //on_hit_enter = "EditConstraint(-1);";
            //on_hit_esc = "theForm.reset();";
            setTimeout('UpdateButtons(); SetFocus();', 30);
            var s = getToolbarButton("sync");
            if ((s)) s.set_enabled(has_links);
        }
        UpdateButtons();
    }

    function ResetConstrlLink(idx) {
        if (idx>=0 && idx<constraints.length) {
            var c = constraints[idx];
            dxConfirm("<% = JS_SafeString(ResString("confRAResetLink")) %>", "onResetConstrLink(" + idx + ");");
        }
    }

    function onResetConstrLink(idx) {
        constraints[idx][3] = 0;
        on_ok_code = "InitConstraints();";
        sendAJAX("action=reset_link&id=" + constraints[idx][0]);
    }

    var sync_cc = true;
    function SwitchConstrReadOnly(idx) {
        if (idx>=0 && idx<constraints.length) {
            var c = constraints[idx];
            sync_cc = true;
            dxConfirm((c[3]==2 ? "<% = JS_SafeString(ResString("confRACC_RO_no").Replace("""", "&quot;")) %>" : "<% = JS_SafeString(ResString("confRACC_RO_yes").Replace("""", "&quot;")) %>"), "onSwitchConstReadOnly(" + idx + ");");
        }
    }

    function onSwitchConstReadOnly(idx) {
        constraints[idx][3] = (constraints[idx][3]==2 ? 1 : 2);
//        on_ok_code = "InitConstraints(); AskSyncConstraint('" + constraints[idx][0] + "');";
        on_ok_code = "InitConstraints();";
        sendAJAX("action=readonly&id=" + constraints[idx][0] + "&val=" + constraints[idx][3] + "&sync=" + ((sync_cc) ? 1 : 0));
    }

<%--    function AskSyncConstraint(c_id) {
        dxDialog("<% =JS_SafeString(ResString("confRASyncCC")) %>", false, "sendAJAX('action=sync_constr&id=" + c_id + "');", ";", "", -1, -1, "<% = JS_SafeString(ResString("btnYes")) %>", "<% = JS_SafeString(ResString("btnNo")) %>");
    }--%>

    function SyncConstraints() {
        var lst = GetSelected();
        var ids = "";
        for (var i=0; i<lst.length; i++) {
            if (constraints[lst[i]][3]>0) ids += (ids=="" ? "" : ",") + lst[i];
        }
        sendAJAX('action=sync_constr&ids=' + ids);
    }

    function SetFocus() {
        document.getElementById("tbCName").focus();
        setTimeout('document.getElementById("tbCName").blur();',50);
        setTimeout('document.getElementById("tbCName").focus();',100);
    }

    function onEditConstraint(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbCName"), "onEditConstraint(" + index + ", true)")) return false;
        InitConstraints();
        var w = $("#tbCName").width();
        if (w<200) w = 200;
        if (w>800) w = 800;
        $("#tdCName" + index).html("<input type='text' style='width:" + w + "px; resize:none;' maxlength=120 id='tbCName' onfocus='on_hit_enter=\"EditConstraint(" + index + ");\"; on_hit_esc = \"InitConstraints(); SetFocus();\"; ' onblur=''on_hit_enter=\"\"\; on_hit_esc = \"\";' value='" + replaceString("'", "&#39;", constraints[index][1]) + "'>");
        <% If RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then%>$("#tdCEnabled" + index).html("<input type='checkbox' id='tbCEnabled' " + ((constraints[index][2]!=1) ? " checked" : "") +" title='<% = JS_SafeString(ResString("lblRACCEnabled")) %>'>");<% End if %>
        <% If isTimePeriodsAvailable() Then%>$("#tdCResource" + index).html("<input type='checkbox' id='tbCResource' " + ((constraints[index][7]==1) ? " checked" : "") +" title='<% = JS_SafeString(ResString("tblRACCResource"))%>'>");<% End If %>
        $("#tdCActions" + index).html("<a href='' onclick='EditConstraint(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='InitConstraints(); SetFocus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        $("#trCNew").html("").hide();
        //on_hit_enter = "EditConstraint(" + index + ");";
        //on_hit_esc = "InitConstraints(); SetFocus();";
        SetFocus();
    }

    function onEnableConstraint(index, chk) {
        constraints[index][2] = ((chk) ? 0 : 1);
        UpdateButtons();
        return sendAJAX("action=enable_constraint&id=" +  constraints[index][0] + "&chk=" + (1-constraints[index][2]));
    }

    function onResource(index, chk) {
        constraints[index][7] = ((chk) ? 1 : 0);
        return sendAJAX("action=is_resource&id=" +  constraints[index][0] + "&chk=" + constraints[index][7]);
    }

    function EditConstraint(index) {
        var n = document.getElementById("tbCName");
        var c = <% If RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then%>document.getElementById("tbCEnabled")<% Else %>null<% End if %>;
        var r = <% If isTimePeriodsAvailable() Then%>document.getElementById("tbCResource");<% Else %>null<%End If%>
        if ((n)) {
            if ((n.value=='' || n.value.trim()=='')) {
                n.blur;
                on_hit_enter = "";
                on_hit_esc = "";
                DevExpress.ui.notify(resString("errRACCNameEmpty"), "error");
                return false;
            } else {
                var idx = constraints.length-1;
                var nm = n.value;
                if (index>=0) {
                    idx = constraints[index][0];
                    nm = replaceString("\n", " ", replaceString("\r", "", nm)).trim();
                    constraints[index][1] = nm;
                    constraints[index][2] = ((c) && c.checked ? 0 : 1);
                    constraints[index][7] = ((r) && r.checked ? 1 : 0);
                } else {
                    for (var i=0; i<constraints.length; i++) {
                        if (constraints[i][0]>idx) idx=constraints[i][0];
                    }
                    idx+=1;

//                    constraints[constraints.length]=[idx, n.value, (c.checked ? 1 : 0)];
                    
                    var c_idx = idx;
                    var lst = replaceString("\r", "", nm).split("\n");
                    for (var j=0; j<lst.length; j++) {
                        var c_n = lst[j].trim();
                        if (c_n>"") {
                            constraints[constraints.length]=[c_idx, c_n, ((c) && c.checked ? 0 : 1),0,0,'','',((r) && r.checked ? 1 : 0)];
                            c_idx+=1;
                        }
                    }
                }
                on_ok_code = "InitConstraints(); SetFocus();";
                sendAJAX("action=edit_constraint&id=" + (index>=0 ? idx : -1) + "&name=" +  enc(nm) <% If RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then%>+ "&chk=" + (c.checked ? 0 : 1)<% End If %><% If isTimePeriodsAvailable() Then%> +"&resource=" + (r.checked ? 1 : 0)<% End if %>);
            }
        }
    }

    function ReInitData(data) {
        if ((data)) { 
            data = eval(data);
            if (data.length==3) {
                constraints = eval(data[0]); 
                attribs_cat = eval(data[1]); 
                attribs_num = eval(data[2]);
            }
            InitConstraints(); 
        };
    }

    function DeleteConstraint(id) {
        dxConfirm("<% = JS_SafeString(ResString("confRADelCC")) %>", "on_ok_code = 'ReInitData(data);'; sendAJAX('action=delete_constraint&id=" + id + "');");
    }

    function DeleteSelectedConstraints(ids) {
        dxConfirm("<% = JS_SafeString(ResString("confRADeleteSelectedCC")) %>", "on_ok_code = 'ReInitData(data);'; sendAJAX('action=delete_constraints&ids=" + ids + "');");
    }

    function DeleteAllConstraints() {
        dxConfirm("<% = JS_SafeString(ResString("confRADelAllCC")) %>", "on_ok_code = 'ReInitData(data);'; sendAJAX('action=delete_all_constraints');");
    }

    var attr_id = '';
    var is_ro = false;
    function ImportConstraints(is_cat) {
        var attribs = ((is_cat) ? attribs_cat : attribs_num);
        attr_id = "";
        is_ro = false;
        var lst = "";
        for (var i=0; i<attribs.length; i++) {
            if (attr_id=="" && !(attribs[i][3])) attr_id = attribs[i][0];
            lst += "<option value='" + attribs[i][0] + "'" + (attr_id == attribs[i][0] ? " selected" : "") + ((attribs[i][3]) ? " style='color:#999999'" : "") + ">" + attribs[i][1] + ((is_cat) ? (attribs[i][2] == <% = AttributeValueTypes.avtEnumerationMulti %> ? " [M]" : "") : (attribs[i][2] == <% = AttributeValueTypes.avtDouble %> ? " [F]" : " [L]")) + "</option>";
        }
        if (lst == "") {
            dxDialog(((is_cat) ? "<% = JS_SafeString(ResString(If(App.ActiveProject.ProjectManager.IsRiskProject, "msgRANoCatAttribRisk", "msgRANoCatAttrib"))) %>" : "<% = JS_SafeString(ResString("msgRANoNumAttrib")) %>"), ";", false, resString("lblInformation"), resString("btnClose"));
        } else {
            if (attr_id=="" && attribs.length>0) attr_id = attribs[0][0];
            dxDialog(((is_cat) ? "<% = JS_SafeString(ResString("lblRASelCatAttrib")) %>" : "<% = JS_SafeString(ResString("lblRASelNumAttrib")) %>") + ": <select name='attr_id' style='width:180px;' onclick='attr_id=this.value;'>" + lst + "</select><div style='padding-top:1ex; padding-left:-3px'><label><input type='checkbox' id='isRO' value='1' onclick='is_ro=this.checked;'> <% =JS_SafeString(ResString("lblRA_CC_ReadOnly")) %></label></div>", ((is_cat) ?  "GetCatAttribs(" + is_cat + ",attr_id);" : "CreateContraintFromAttrib(" + is_cat + ", attr_id);"), ";", "<% = JS_SafeString(ResString("lblRACreateCC")) %>", "<% = JS_SafeString(ResString("btnContinue")) %>");
        }
    }

    function GetCatAttribs(is_cat, aid) {
        on_ok_code = "if ((data)) ShowCatAttribs(eval(data), '" + aid + "');";
        sendAJAX("action=get_attribs&aid=" + aid);
    }

    var attribs_count = 0;
    function ShowCatAttribs(attribs, aid) {
        if ((attribs) && attribs.length>0)
        {
            attribs_count = attribs.length;
            var h = -1;
            if (attribs.length>10) h = 470;
            var lst = "<table border='0' style='height:94%; width:" + (h==-1 ? "96%" : "98%") + "'><tr><td class='text' style='height:1%'><b><% = JS_SafeString(ResString("msgRASelectCatAttribs")) %></b></td></tr><tr><td class='text' style='padding:1em 0px 1em 1em; height:98%;'><div style='" + (h==-1 ? "" : "max-height: " + h + "px;overflow:auto;") + "'>";
            for (var i=0; i<attribs.length; i++) {
                lst += "<nobr><input type='checkbox' id='atr_" + i + "' name='atr" + i + "' value='" + attribs[i][0] + "' " + ((attribs[i][2]) ? "" : " checked") + " onclick='checkSelAttrib();'><label for='atr_" + i + "'>" + attribs[i][1] + "</label>" + (attribs[i][2] ? "&nbsp;<span class='small gray'>(<% =JS_SafeString(ResString("lblRACatAtrLinked"))%>)</span>" : "")+ "</nobr><br>";
            }
            lst += "</div></td></tr><tr><td class='text' style='height:1%'><div class='small' style='text-align:right'><a href='' onclick='return selectAttribs(1);' class='actions'><% =ResString("lblSelectAll") %></a> | <a href='' onclick='return selectAttribs(-1);' class='actions'><% =ResString("lblSelectInverse") %></a> | <a href='' onclick='return selectAttribs(0);' class='actions'><% =ResString("lblSelectNone") %></a></div></td></tr></table>";
            //jDialog_show_icon = false;
            dxDialog(lst, "CheckAndCreateCatAttribs('" + aid + "');", "ImportConstraints(1);", "<% = JS_SafeString(ResString("titleRACatAttribs")) %>");
            setTimeout("checkSelAttrib();", 100);
        }
        else
        {
            DevExpress.ui.notify("<% = JS_SafeString(ResString("msgRANoCatAttribs")) %>", "info");
        }
    }

    function checkSelAttrib() {
        dxDialogBtnDisable(true, !(getSelAttribs().length));
    }

    function selectAttribs(sel) {
        for (var i=0; i<attribs_count; i++) {
            var c = $("#atr_" + i);
            if ((c)) {
                var s = c.is(':checked');
                if (sel==-1) s = !s; else s = sel;
                c.prop('checked', (s) ? true : false);
            }
        }
        checkSelAttrib();   
        return false;
    }

    function getSelAttribs() {
        var lst = [];
        for (var i=0; i<attribs_count; i++) {
            var c = $("#atr_" + i);
            if ((c)) {
                var s = c.is(':checked');
                if ((s)) lst.push(c.prop('value'));
            }
        }
        return lst;
    }

    function CheckAndCreateCatAttribs(aid) {
        var sel = getSelAttribs();
        var has_created = 0;
        var ids = "";
        for (var i=0; i<sel.length; i++) {
            var id = sel[i];
            ids += (ids == "" ? "" : ";") + id;
            for (var j=0; j<constraints.length; j++) {
                if (id==constraints[j][5] || id==constraints[j][6]) { has_created = 1; break; }
            }
        }
        if ((has_created)) {
            var cmd = 'on_ok_code = "ReInitData(data);"; sendAJAX("action=import_constraint&is_cat=1&aid=' + aid + '&ids=' + ids + '&ro=' + ((is_ro) ? 1 : 0) +'");';
            dxConfirm("<% = JS_SafeString(ResString("msgRACCCatExistsOnImport")) %>", cmd, "ImportConstraints(1);");
        }
        else {
            onCreateCatAttribs(aid);
        }
    }

    function onCreateCatAttribs(aid) {
        var sel = getSelAttribs();
        var ids = "";
        for (var i=0; i<sel.length; i++) {
            ids += (ids == "" ? "" : ";") + sel[i];
        }
        on_ok_code = "ReInitData(data);";
        sendAJAX("action=import_constraint&is_cat=1&aid=" + aid + "&ids=" + ids + "&ro=" + ((is_ro) ? 1 : 0));
    }

    function CreateContraintFromAttrib(is_cat, aid) {
        var attribs = ((is_cat) ? attribs_cat : attribs_num);
        var idx = -1;
        for (var i=0; i<attribs.length; i++) {
            if (attribs[i][0]==aid) idx=i;
        }
        if (idx>=0 && (attribs[idx][3])) {
            dxConfirm("<% = JS_SafeString(ResString("msgRACCExistsOnImport")) %>", "onCreateContraint(" + is_cat + ", '" + aid + "');", "ImportConstraints(" + is_cat + ");"); 
        } else {
            onCreateContraint(is_cat, aid);
        }
    }

    function onCreateContraint(is_cat, aid) {
        on_ok_code = "ReInitData(data);";
        sendAJAX("action=import_constraint&is_cat=" + is_cat + "&aid=" + aid + "&ro=" + ((is_ro) ? 1 : 0));
    }

    function onDeleteConstraint(id) {
        var l = [];
        for (var i=0; i<constraints.length; i++) {
            if (constraints[i][0]!=id) l.push(constraints[i]);
        }
        constraints = l;
        InitConstraints();
    }

    function onDeleteConstraints(ids) {
        var sel = eval("[" + ids + "]");
        for (var j=0; j<sel.length; j++) {
            var l = [];
            for (var i=0; i<constraints.length; i++) {
                if (constraints[i][0]!=sel[j]) l.push(constraints[i]);
            }
            constraints = l;
        }
        InitConstraints();
    }

    function onDeleteAllConstraints() {
        constraints = [];
        InitConstraints();
    }

    function Nav2Alts() {
        navOpenPage(20102, true);
        return false;
    }

    function Nav2SBs() {
        navOpenPage(77108, true);
        return false;
    }

    function Nav2Controls() {
        navOpenPage(70904, true);
        return false;
    }    
    
    function onSelectAll() {
        var sel = (GetSelected().length != constraints.length);
        for (var i=0; i<constraints.length; i++) {
            eval("theForm.Select" + i +". checked=" + sel);
        }
        theForm.SelectAll.checked = sel;
        UpdateButtons();
    }

    function onSetScenario(id) {
        <%--SwitchWarning("<% =pnlLoadingPanel.ClientID %>", true);--%>
        displayLoadingPanel(true);
        document.location.href='<% =PageURL(CurrentPageID) %>?action=scenario<% =GetTempThemeURI(true) %>&sid='+ id;
        return false;
    }

    /* Drag-Drop === */
    var drag_index = -1;

    function InitDragDrop() {
        $(function () {
            $(".drag_drop_grid").sortable({
                items: 'tr:not(tr:last-child)',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid',
                axis: 'y',
                start: function( event, ui ) { is_grag_n_drop = true; drag_index = ui.item.index(); },
                stop: function( event, ui ) { is_grag_n_drop = false; onDragIndex(ui.item.index()); }
            });
        });
    }

    function onDragIndex(new_idx) {
        if (new_idx>=0 && drag_index>=0 && new_idx!=drag_index) {
            var el = constraints[drag_index];
            constraints.splice(drag_index, 1);
            constraints.splice(new_idx, 0, el);
            var cc_order = "";
            for (var i=0; i<constraints.length; i++) {                    
                cc_order += (cc_order=="" ? "" : ",") + constraints[i][IDX_CC_ID];
            }            
            sendAJAX("action=reorder_constraints&lst=" + encodeURIComponent(cc_order));
            InitConstraints();
            drag_index = -1;
        }
    }

    /* Drag-Drop == */

    function Hotkeys(event)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event)
        {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            if (code == 13) {
                if (on_hit_enter!="") {
                    eval(on_hit_enter);
                    on_hit_enter = "";
                    return false;
                }
            }
            if (code == 27) {
                if (on_hit_esc!="") {
                    eval(on_hit_esc);
                    on_hit_esc = "";
                    return false;
                }
            }
        }
    }

    function InitScenarios() {
        var s = theForm.cbScenarios;
        if ((s))
        {
            s.options.length = 0;
            s.disabled = 0;
            for (var i=0; i<scenarios.length; i++) {
                //s.options[i] = new Option(scenarios[i][1], scenarios[i][0], (scenarios[i][0]==scenario_active), (scenarios[i][0]==scenario_active));
                s.options[i] = new Option(ShortString(scenarios[i][1], OPT_SCENARIO_LEN, false) + (scenarios[i][2] == "" ? "" : " (" + ShortString(scenarios[i][2], OPT_DESCRIPTION_LEN, false) + ")"), scenarios[i][0], (scenarios[i][0]==scenario_active), (scenarios[i][0]==scenario_active)); // A0965 
            }
        }
    }

    function InitPage() {
        InitScenarios();
        InitConstraints();
        if (constraints.length<15) setTimeout("SetFocus();", 200);
    }

    function onFixBrokenLinks(msg) {
        dxConfirm(msg, "on_ok_code = 'ReInitData(data);'; sendAJAX('action=fix_links');");
    }

    function resizeGrid() {
        if (!is_grag_n_drop)  $("#divConstraints").height(10).height($("#tdContent").innerHeight());
//        if (!is_grag_n_drop)  $("#divConstraints").height(10).width(600).height($("#tdContent").height()).width($("#tblConstraints").width()+24);
//        $("#tblConstraints").css( { left: Math.round(($("#tdContent").width() - $("#tblConstraints").width()) / 2) +"px" } );
    }

    document.onkeypress = Hotkeys;
    $(document).ready(function () { ResetRadToolbar(); InitDragDrop(); InitPage(); resizeGrid(); });
    resize_custom  = resizeGrid;

</script>
</telerik:RadScriptBlock>

<table border='0' cellspacing='0' cellpadding='0' class='whole'>

<tr valign='bottom' align='left' class='text'>
<td style="height:24px" class="text"><div id='divToolbar' style='overflow:hidden;'><telerik:RadToolBar ID="RadToolBarMain" runat="server" CssClass="dxToolbar" Skin="Default" Width="100%" OnClientButtonClicked="onClickToolbar" AutoPostBack="false" EnableViewState="false">
    <Items>
        <telerik:RadToolBarButton runat="server" EnableViewState="false">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOptScenario" Enabled="false">
            <ItemTemplate><select disabled="disabled" id="cbScenarios" style="width:170px; margin-top:3px" onchange="onSetScenario(this.value);"><option selected="selected">Not available</option></select></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false"><ItemTemplate>&nbsp;|&nbsp;</ItemTemplate></telerik:RadToolBarButton>
        <telerik:RadToolBarDropDown runat="server" DropDownWidth="250" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/import_20.png" Text="Import from attributes">
        <Buttons>
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="import_cat" ImageUrl="~/App_Themes/EC09/images/import_cat_20.png" Text="Create From Categorical Attribute" />
            <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="import_num" ImageUrl="~/App_Themes/EC09/images/import_num_20.png" Text="Create From Numerical Attribute" />
        </Buttons>
        </telerik:RadToolBarDropDown>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="sync" ImageUrl="~/App_Themes/EC09/images/sync_20.png" Text="Sync linked constraints" Enabled="false" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false"><ItemTemplate>&nbsp;|&nbsp;</ItemTemplate></telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="delete" ImageUrl="~/App_Themes/EC09/images/delete_20.png" Text="Delete" Enabled="false" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="delete_all" ImageUrl="~/App_Themes/EC09/images/clear-20.png" Text="Delete All" />
    </Items>
</telerik:RadToolBar></div>
</td></tr>

<tr valign='top'>
    <td class='text' align="center" style='height:30px'><h5 id="h5Header" style="padding:1ex"><% =String.Format(ResString("lblRACCTitle"), SafeFormString(ShortString(RA.Scenarios.ActiveScenario.Name, 65)))%></h5></td>
</tr>
<tr valign='top'>
    <td class='text' align="center" id='tdContent'><div id="divConstraints" style="width:100%; height:100%; overflow:auto; position:relative;"><p align="center" style='padding:0px; margin:0px'>
<table id="tblConstraints" border='0' cellspacing='1' cellpadding='2' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header">
        <th style="width:1em">&nbsp;</th> <%--Drag-drop handle column--%>
        <th><input type="checkbox" id="SelectAll" value="1" onclick='onSelectAll()'></th>
        <th><% = JS_SafeString(ResString("tblRACCName"))%><br ><img src="<% =ImagePath %>blank.gif" width="250" height="1" title="" border="0" ></th>
        <% If RA_OPT_CC_ALLOW_ENABLED_PROPERTY Then%><th style="width:115px"><input type='checkbox' id='cbIgnoreAll' onclick='switchAllCC(this.checked);'><label for='cbIgnoreAll'><% = JS_SafeString(ResString("tblRACCEnabled").Replace("(", "<br>("))%></label></th><% End If %>
        <% If isTimePeriodsAvailable() Then%><th style="width:85px"><% = JS_SafeString(ResString("tblRACCResource"))%></th><% End if %>
        <th style="width:40px"><% = JS_SafeString(ResString("tblRACCAction"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></p>
</div>
</td></tr>
</table>

<%--<EC:LoadingPanel id="pnlLoadingPanel" runat="server" isWarning="true" WarningShowOnLoad="false" WarningShowCloseButton="false" Width="230" Visible="true"/>--%>

</asp:Content>
