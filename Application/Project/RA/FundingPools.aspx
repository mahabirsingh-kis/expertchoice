<%@ Page Language="VB" Inherits="RAFundingPoolsScrren" title="Funding Pools" Codebehind="FundingPools.aspx.vb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<%--<%@ Register TagPrefix="EC" TagName="LoadingPanel" Src="~/ctrlLoading.ascx" %>--%>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/jquery.textarea-expander.js"></script>
<script language="javascript" type="text/javascript">

    var on_error_code = "";
    var on_ok_code = "";
    var val_old = "";
    var is_ajax = 0;
    var is_solve = 0;
    var last_focus = "";

    var mh = 500;   // maxHeight for dialogs;

    var dlg_pools;

    var scenarios = [<% =GetScenarios() %>];
    var scenario_active = <% =RA.Scenarios.ActiveScenarioID %>;

    var pools = [<% =GetFundingPools() %>];

    var on_hit_enter = "";

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

    function isAutoSolve() {
        var as = getToolbarButton("autosolve");
        return ((as) && as.get_isEnabled() && as.get_isChecked());
    }

    <%--function PleaseWait(vis)
    {
        var as = getToolbarButton("autosolve");
        if ((!vis) || !(as) || (as.get_isChecked()) || (is_solve))
        {
            if ((vis)) {
                setTimeout('if (is_ajax) SwitchWarning("<% =pnlLoadingPanel.ClientID %>", ' + vis + ');', 300);
            }
            else {
                SwitchWarning("<% =pnlLoadingPanel.ClientID %>", vis);
            }
        }
        theForm.disabled = vis;
    }--%>
    
    function Str2Val(val) {
        var s = replaceString(",", ".", val);
        //A0907 === remove thousands separators
        s = s.replace(/[^0-9.]/g, "");
        var last = s.lastIndexOf('.');
        if ((last > 0) && (last < s.length)) s = s.substring(0, last).replace(".", "") + s.substring(last);
        //A0907 ==
        if (s == "0.") s = "0";
        if (s == "1.") s = "1";
        if (s[0] == ".") s = "0" + s;
        var v = s * 1;
        if (s == v && s!="") {
            return v;
        }
        else {
            return "undefined";
        };
    }

    // is_0_1: 0 = any number >0; 1 = [0..1]; -1 = any number, even empty string;
    function isValidNumber(edit, is_0_1, warning) {
        var v = Str2Val(edit.value);
        var msg = "";
        //        if (edit.value==="" && is_0_1==-1) return "";
        if (edit.value==="") return "";
        if (v=="undefined" || (is_0_1==1 && (v<0 || v>1))) msg = (is_0_1==1 ? "<% =JS_SafeString(String.Format(ResString("errWrongNumberRange"), 0, 1)) %>" : "<%= JS_SafeString(ResString("errNotANumber")) %>");
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

    function RowHover(r, hover, alt) {
        if ((r)) {
            var funded = (r.id.indexOf("funded") > 0);
            //var bg = (hover == 1 ? (funded ? "#bad991" :  (alt ? "#eaf0f5": "#f0f5ff")) : (funded ? "#d1e6b5" : (alt ? "#fafafa" : "#ffffff")));
            var bg = (hover == 1 ? (funded ? "#bad991" : "#fafafa") : (funded ? "#d1e6b5" : "#ffffff"));
            for (var i = 0; i < r.cells.length; i++) {
                r.cells[i].style.background = bg;
            }
        }
    }

    function onKeyUp(id, is0_1)
    {
        var c = eval("theForm." + id);
        if ((c)) {
            var v = isValidNumber(c, is0_1, false);
            if ((c.style)) c.style.color = (c.value!="" && v=="undefined" ? "#ff0000" : "");
            if (typeof v!="undefined" && v!="undefined") return true;
        }
        return false;
    }

    function onBlur(id, name, guid, is0_1) {
        var c = eval("theForm." + id);
        if (!(is_ajax)) last_focus = ""; 
        if (!(is_ajax) && (c) && (((val_old=="" || c.value=="") && c.value!=val_old) || (Str2Val(c.value)!=Str2Val(val_old)))) {
            var v = isValidNumber(c, is0_1, true);
            if (typeof v!="undefined" && v!="undefined") {
                on_ok_code="setTimeout('onRestoreFocus();', 150);"; 
                return sendCommand("action=" + name + (guid=="" ? "" : "&guid=" + guid) + "&val=" + v);
            }
        }
        return false;
    }

    function onRestoreFocus() {
        if (last_focus!='') {
            var t = eval('theForm.'+last_focus);
            if ((t)) { 
                t.focus(); 
                t.select(); 
            } else {
                t = $("#" + id);
                if ((t) && (t.length)) t.focus();
            }
        }
    }

    function onFocus(id, val) {
        if (!dxDialog_opened) setTimeout('val_old = "' + val + '"; last_focus = "' + id + '";', 10);
    }

    function onChangeAutoSolve(chk) {
        return sendCommand("action=autosolve&val=" + ((chk) ? 1 : 0));
    }

    function onChangeSettings(name, chk) {
        sendCommand("action=settings&name=" + name + "&chk=" + ((chk) ? 1 : 0));
    }

    function sendCommand(cmd) {
        if (CheckForm()) {
            var am = $find("<%=radAjaxManagerMain.ClientID%>");
            if ((am)) { 
                //PleaseWait(1); 
                displayLoadingPanel(true);
                am.ajaxRequest(cmd); 
                is_ajax=1; 
                return true; 
            }
        }
        return false;
    }
    
    function onRequestError(sender, e) {
        is_ajax = 0;
        //PleaseWait(0); 
        displayLoadingPanel(false);
        InitGrid();
        if (on_error_code != "") eval(on_error_code);
        on_error_code = "";
        on_ok_code = "";
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function onResponseEnd(sender, argmnts) {
        is_ajax = 0;
        //PleaseWait(0);
        displayLoadingPanel(false);
        InitGrid();
        on_error_code = "";
        if (on_ok_code != "") eval(on_ok_code);
        on_ok_code = "";
    };

    function onSolve() {
        if (!CheckForm()) return false;
        return true;
    }

    function Solve() {
        is_solve = 1;
        on_ok_code = "is_solve=0;";
        on_error_code = on_ok_code;
        return sendCommand("action=solve");
    }

    function onClickToolbar(sender, args) {
        var button = args.get_item();
        if ((button))
        {
            var btn_id = button.get_value();
            if ((btn_id))
            {
                switch (btn_id+"") {
                    case "solve":
                        Solve();
                        break;
                    case "autosolve":
                        onChangeAutoSolve(button.get_isChecked());
                        break;
                    case "pools":
                        InitPools();
                        InitPoolsDlg();
                        break;
                }
            }
        }
    }

    var is_context_menu_open = false;

    function showMenu(event, uid, hasCopyButton, hasPasteButton, hasDisabledButton, isDisabledFP) {                       
        is_context_menu_open = false;                
        $("#contextmenuheader").hide().remove();
        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
        if (hasCopyButton) sMenu += "<a href='' class='context-menu-item actions' onclick='doCopyToClipboardValues(\"" + uid + "\"); return false;'><div><nobr>&nbsp;<i class='fas fa-copy'></i>&nbsp;<%=ResString("titleCopyToClipboard")%></nobr></div></a>";
        if (hasPasteButton) sMenu += "<a href='' class='context-menu-item actions' onclick='doPasteAttributeValues(\""+ uid + "\"); return false;'><div><nobr>&nbsp;<i class='fas fa-paste'></i>&nbsp;<%=ResString("titlePasteFromClipboard")%></nobr></div></a>";
        if (hasDisabledButton) sMenu += "<a href='' class='context-menu-item actions' onclick='doSwitchFP(\"" + uid +"\"," + (isDisabledFP ? 0 : 1) +"); return false;'><div style='padding:1px;'><nobr><!--" + (isDisabledFP ? "<i class='far fa-square'></i>" : "<i class='fas fa-check-square'></i>") + "--><input type='checkbox' id='cbIgnore" + uid + "'" + (isDisabledFP ? " checked" : "") + "><label for='cbIgnore" + uid + "'>&nbsp;<%=JS_SafeString(ResString("tblRAFPEnabled"))%>&nbsp;</label></nobr></div></a>";
        sMenu += "</div>";                
        var img = document.getElementById("mnu_img_" + uid + (hasDisabledButton ? "i" : ""));
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

    function doSwitchFP(uid, e) {
        onEnablePool(uid, e);
        return false;
    }

    function canCloseMenu() {
        is_context_menu_open = true;
        $(document).unbind("click").bind("click", function () { if (is_context_menu_open == true) { $("#contextmenuheader").hide(200); is_context_menu_open = false; } });        
    }

    function doPasteAttributeValues(attr_idx) {
        $("#contextmenuheader").hide(200); is_context_menu_open = false;
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteData(attr_idx, data);
        } else {
            dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome('" + attr_idx + "');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
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

        for (var i=2;i<row_count;i++) { //skip headers row           
            for (var k=0; k<grid.rows[i].cells.length; k++) {   // Skip name and cost
                if (grid.rows[i].cells[k].getAttribute('clip_data_id') == unique_id) {
                    var cell = grid.rows[i].cells[k];
                    var value= cell.getAttribute('clip_data').toString();
                    res += (res==""?"":"\r\n") + value;
                }
            }
        }

        copyDataToClipboard(res);
    }

    function commitPasteChrome(attr_idx) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(attr_idx, pasteBox.value);
        }
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", on_agree + ";");
            return false;
        }
        return true;
    }

    function InitPoolsDlg() {
        dlg_pools = $("#divPools").dialog({
              autoOpen: true,
              width: 550,
              minWidth: 530,
              maxWidth: 950,
              minHeight: 250,
              maxHeight: mh,
              modal: true,
              dialogClass: "no-close",
              closeOnEscape: true,
              bgiframe: true,
              title: "<% = JS_SafeString(ResString("titleRAFundingPools")) %>",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                Close: function() {
                  if (checkUnsavedData(document.getElementById("tbName"), "dlg_pools.dialog('close')")) dlg_pools.dialog( "close" );
                }
              },
              open: function() {
                $("body").css("overflow", "hidden");
                document.getElementById("tbName").focus();
              },
              close: function() {
                $("body").css("overflow", "auto");
                $("#tblPools tbody").html("");
                on_hit_enter = "";
                $("#divPools").dialog("destroy");
                dlg_scenarios = null;
                if (pools_order!="") {
                    sendCommand("action=pools_reorder&lst=" + pools_order);
                    pools_order = "";
                }

              },
              resize: function( event, ui ) { $("#pPools").height(30); $("#pPools").height(Math.round(ui.size.height-125)); }
        });
        if ($("#pPools").height()>mh-125) $("#pPools").height(mh-125);
        setTimeout('$("#pPools").scrollTop(10000);', 30);
    }

    function InitPools() {
        var t = $("#tblPools tbody");
        if ((t)) {
            t.html("");
            for (var i=0; i<pools.length; i++) {
                sRow = "<tr class='text grid_row_dragable " + ((i&1) ? "grid_row" : "grid_row_alt") + "'><td align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td id='tdName" + i + "'>" + htmlEscape(pools[i][1]) + "</td>";
                <% If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then%>sRow += "<td id='tdCEnabled" + i + "' align='center'><input type='checkbox' id='tbCEnabled" + i + "'" + ((pools[i][2]!=1) ? " checked" : "") + " title='' onclick='return onEnablePool(" + i + ", this.checked);'></td>";<% End if %>

                sRow += "<td id='tdActions" + i + "' align='center'><a href='' onclick='onEditPool(" + i + "); return false;' title='<% =JS_SafeString(ResString("lblRAEditFPoolName")) %>'><i class='fas fa-pencil-alt'></i></a>&nbsp;<a href='' onclick='DeletePool(" + pools[i][0] + "); return false;' title='<% =JS_SafeString(ResString("btnDelete")) %>'><i class='fas fa-trash-alt'></i></a></td></tr>";
                t.append(sRow);
            }
            sRow = "<tr class='text grid_footer' id='trNew'><td>&nbsp;</td>";
            sRow += "<td><input type='text' class='input' style='width:100%;' id='tbName'></td>";
            <% If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then%>sRow += "<td align='center'><input type='checkbox' id='tbCEnabled' title=''></td>";<% End If %>
            sRow += "<td align='center'><a href='' onclick='EditPool(-1); return false;' title='<% =JS_SafeString(ResString("lblRAAddFPool")) %>'><i class='fas fa-plus'></i></a></td></tr>";
            t.append(sRow);
            on_hit_enter = "EditPool(-1); ; setTimeout(\"document.getElementById('tbName').focus();\", 250);";
        }
    }

    function onEnablePool(index, chk) {
        pools[index][2] = ((chk) ? 0 : 1);
        return sendCommand("action=enable_pool&id=" + pools[index][0] + "&chk=" + pools[index][2]);
    }

    function onEditPool(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbName"), "onEditPool(" + index + ", true)")) return false;
        InitPools();
        $("#tdName" + index).html("<input type='text' class='input' style='width:100%;' id='tbName' value='" + replaceString("'", "&#39;", pools[index][1]) + "'>");
        <% If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then%>$("#tdCEnabled" + index).html("<input type='checkbox' id='tbCEnabled' " + ((pools[index][2]!=1) ? " checked" : "") +" title='<% = JS_SafeString(ResString("lblRAFPEnabled"))%>'>");<% End if %>
        $("#tdActions" + index).html("<a href='' onclick='EditPool(" + index + "); return false;' title='<% =JS_SafeString(REsString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='InitPools(); document.getElementById(\"tbName\").focus(); return false;' title='<% =JS_SafeString(REsString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        $("#trNew").html("").hide();
        setTimeout("document.getElementById('tbName').focus();", 50);
        on_hit_enter = "EditPool(" + index + "); setTimeout(\"document.getElementById('tbName').focus();\", 250);";
    }

    function EditPool(index) {
        var n = document.getElementById("tbName");
        var c = <% If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then%>document.getElementById("tbCEnabled")<% Else %>null<% End if %>;
        if ((n)) {
            if ((n.value=='' || n.value.trim=='')) {
                dxDialog("<% = JS_SafeString(ResString("errRAEmptyFPoolName")) %>", "setTimeout(\"document.getElementById('tbName').focus();\", 150);");
            } else {
                var idx = pools.length-1;
                if (index>=0) {
                    idx = pools[index][0];
                    pools[index][1] = n.value;
                    pools[index][2] = ((c) && c.checked ? 0 : 1);
                } else {
                    for (var i=0; i<pools.length; i++) {
                        if (pools[i][0]>idx) idx=pools[i][0];
                    }
                    idx+=1;
                    pools[pools.length] = [idx, n.value, ((c) && c.checked ? 0 : 1)];
                }
                InitPools();
                on_ok_code = "setTimeout(\"document.getElementById('tbName').focus();\", 150);";
                sendCommand("action=edit_pool&id=" + (index>=0 ? idx : -1) + "&name=" +  enc(n.value)<% If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then%>+ "&chk=" + (c.checked ? 0 : 1)<% End If %>);
            }
        }
    }

    function DeletePool(id) {
        dxConfirm("<% = JS_SafeString(ResString("confRADeleteFPool")) %>", "on_ok_code = 'onDeletePool(" + id + ");'; sendCommand('action=delete_pool&id=" + id + "');");
    }

    function onDeletePool(id) {
        var l = [];
        for (var i=0; i<pools.length; i++) {
            if (pools[i][0]!=id) l.push(pools[i]);
        }
        pools = l;
        InitPools();
    }

    function switchExhausted(chk) {
        //on_ok_code = "PleaseWait(false);" + (isAutoSolve ? "Solve();" : "");
        sendCommand("action=exhausted&chk=" + ((chk) ? 1 : 0));
        return false;
    }

    function onSetScenario(id) {
        <%--SwitchWarning("<% =pnlLoadingPanel.ClientID %>", true);--%>
        displayLoadingPanel(true);
        document.location.href='<% =PageURL(CurrentPageID) %>?action=scenario<% =GetTempThemeURI(true) %>&sid='+ id;
        return false;
    }

    function enableFP() {
        <%--SwitchWarning("<% =pnlLoadingPanel.ClientID %>", true);--%>
        displayLoadingPanel(true);
        document.location.href='<% =PageURL(CurrentPageID) %>?action=enable_fp<% =GetTempThemeURI(true) %>';
        return false;
    }

    function InitScenarios() {
        var s = theForm.cbScenarios;
        if ((s))
        {
            s.options.length = 0;
            s.disabled = 0;
            for (var i=0; i<scenarios.length; i++) {
                s.options[i] = new Option(scenarios[i][1], scenarios[i][0], (scenarios[i][0]==scenario_active), (scenarios[i][0]==scenario_active));
            }
        }
    }

    function InitGrid() {
        $("#divGrid").height(100);
        $("#<% =tblGrid.ClientID %>").height(150).height($("#trGrid").height()-8);
        $("#divGrid").height($("#divGrid").parent().parent().height());
        $('input.as_number').attr('autocomplete','off');
        $('input.as_number').attr('autocorrect','off');
        $('input.as_number').attr('autocapitalize','off');
    }

    var drag_index = -1;
    var pools_order = "";

    function onDragIndex(new_idx) {
        if (new_idx>=0 && drag_index>=0 && new_idx!=drag_index) {
            var el = pools[drag_index];
            pools.splice(drag_index, 1);
            pools.splice(new_idx, 0, el);
            pools_order = "";
            for (var i=0; i<pools.length; i++) {
                pools[i][3] = i+1;
                pools_order += (i==0 ? "" : ",") + pools[i][0];
            }
            drag_index = -1;
            InitPools();
            setTimeout("document.getElementById('tbName').blur();", 60);
            setTimeout("document.getElementById('tbName').focus();", 100);
        }
    }

    function InitPage() {
        InitScenarios();
        setTimeout('InitGrid();', 500);
        <% if sFocusID<>"" Then %>setTimeout("if (last_focus=='') <% =sFocusID %>", 350);<% End if %>

        $(function () {
            $(".drag_drop_grid").sortable({
                items: 'tr.grid_row_dragable',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid',
                axis: 'y',
                start: function( event, ui ) { drag_index = ui.item.index(); },
                update: function( event, ui ) { onDragIndex(ui.item.index()); }
            });
        });
    }

    function nextTabindex(el) {
        if ((el)) {
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
            if (n>=65535 && !(next_el)) next_el = null;
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
            if (code == 13) {
                if (on_hit_enter=="") {
                    if (last_focus == "") { 
                        var b = getToolbarButton("solve");
                        if ((b)) { b.focus(); b.click(); } else onSolve();
                    } else {
                        nextTabindex(eval('theForm.'+last_focus));
                    }
                } else {
                    eval(on_hit_enter);
                }
                return false;
            }
        }
    }

    function selectConstr(guid) {
        if (guid != "") {
            loadURL("?constr=" + guid);
        }
    }

    $(document).ready(function () { 
        toggleScrollMain();
        ResetRadToolbar();
    });

    resize_custom = InitGrid;
    document.onkeypress = Hotkeys;

    $(document).ready(function () {
        $(".filter-link-info").click(function () {
            $(".filter-data-wrapper").toggleClass("show-filter-link-info");
        });
    })
</script>
</telerik:RadScriptBlock>

<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server" ClientEvents-OnRequestError="onRequestError" ClientEvents-OnResponseEnd="onResponseEnd" EnableAJAX="true" EnableHistory="false" EnablePageHeadUpdate="false">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="tblGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="tblGrid"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<%--<EC:LoadingPanel id="pnlLoadingPanel" runat="server" isWarning="true" WarningShowOnLoad="false" WarningShowCloseButton="false" Width="230" Visible="true"/>--%>

<div class="rightside-filter-wrapper">
    <div class="filter-wrapper">
        <div class="filter-icon-info">
            <a href="#" class="filter-link-info portfoliof_info" id="filterlinkinfo">
                <img src="../../Images/img/icon/filter-right-icon.svg" alt="filter-icon">
            </a>
        </div>
        <div class="filter-data-wrapper portfolio_view_filters">
            <div class="show-filters-data-info">
                <div class="show-filters-info">
                    <div class="col-md-10">
                            <div class="d-flex align-items-center">
                                <button type="button" class="btn dropdown-toggle border bg-white ms-3">
                                    Default Scenario
                                    <img src="../../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                </button>
                                <button type="button" class="btn text-nowrap  bg-light-gray ms-3" role="button" data-bs-toggle="modal" data-bs-target="#funding-pool-modal">
                                    <img class="me-2" src="../../Images/img/manage_f.svg" alt="img">
                                    Manage Funding Pools
                                    <img src="../../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                </button>
                                <button type="button" class="btn text-nowrap bg-light-gray ms-3" role="button" id="dropdownMenuLinka" data-bs-toggle="dropdown" aria-expanded="false">
                                    <img class="me-2" src="../../Images/img/Settings-2-svgrepo-com.svg" alt="img">
                                    <img src="../../Images/img/icon/down-arrow-bordered-icon.svg" class="down-arrow-icon" alt="down-arrow">
                                </button>

                                <ul class="dropdown-menu rounded-10 solve_dropdown " aria-labelledby="dropdownMenuLinka" data-popper-placement="bottom-start">
                                    <li>
                                        <a class="dropdown-item" href="#">
                                            <div class="form-group">
                                                <input type="checkbox" id="check-list52" name="list1" class="checkbox">
                                                <label for="check-list52" class="checkbox_label">Show funded only</label>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="dropdown-item" href="#">
                                            <div class="form-group">
                                                <input type="checkbox" id="check-list52q" name="list1" class="checkbox">
                                                <label for="check-list52q" class="checkbox_label">Show graph bars</label>
                                            </div>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="dropdown-item" href="#">
                                            <div class="form-group">
                                                <input type="checkbox" id="check-list52b" name="list1" class="checkbox">
                                                <label for="check-list52b" class="checkbox_label">Use Pool Effectiveness</label>
                                            </div>
                                        </a>
                                    </li>
                                </ul>
                               <div class="d-flex align-items-center justify-content-end " style="right:20px">
                                <div class="pin-map-icon-info" style="position:absolute;right:20px">
                                       <a href="#"> <img src="../../Images/img/icon/pin-icon.svg"> </a>
                                </div>
                              </div>
                          </div>
                        </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div class="container" style="margin-top:30px;height:700px;">
<table border='0'  cellspacing='0' cellpadding='0' class='whole' style="width:98%;height:99%;">
<tr valign='bottom' align='left' class='text'>
<td style="height:24px" class="text">
    <telerik:RadToolBar ID="RadToolBarMain" Visible="false" runat="server" CssClass="dxToolbar" Skin="Default" OnClientButtonClicked="onClickToolbar" AutoPostBack="false" EnableViewState="false">
    <Items>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Solve" Width="90px" ImageUrl="~/App_Themes/EC09/images/assembly-20.png" Value="solve" Font-Bold="true" Font-Size="11pt" BorderColor="#ffaa44" BackColor="#f0f0f0" ForeColor="#cc6600" BorderWidth="1" OnLoad="OnLoadSolveButton"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" AllowSelfUnCheck="True" ImageUrl="~/App_Themes/EC09/images/repeat-20.png" CheckOnClick="True" Checked="False" Text="Auto-solve" Value="autosolve"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" IsSeparator="true" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span><select disabled="disabled" id="cbScenarios" style="width:150px; margin-top:3px; margin-right:4px;" onchange="onSetScenario(this.value);"><option selected="selected">Not available</option></select></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" IsSeparator="true" />
        <telerik:RadToolBarButton runat="server" EnableViewState="false" ImageUrl="~/App_Themes/EC09/images/coins-20.png" Text="Manage Funding Pools&hellip;" Value="pools"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false">
            <ItemTemplate><span style="margin:3px 5px 0px 0px"><input type='checkbox' id='cbExhausted' name='cbExhausted' value='1' <% =IIf(App.ActiveProject.ProjectManager.Parameters.RAFundingPoolsExhausted, "checked", "")%> onclick="switchExhausted(this.checked);" /><label for="cbExhausted"><% =ResString("lblRAFPoolsExhausted")%></label></span></ItemTemplate>
        </telerik:RadToolBarButton>        
        <telerik:RadToolBarButton runat="server" EnableViewState="false" IsSeparator="true" />
        <telerik:RadToolBarDropDown runat="server" EnableViewState="false" DropDownWidth="200" ImageUrl="~/App_Themes/EC09/images/settings-20.png" Text="Settings">
        <Buttons>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px;'><label style='margin-left:6px'><input type='checkbox' id='cbOptShowFunded' <% = IIf(RA_ShowFundedOnly, " checked", "") %> onclick='onChangeSettings("showfunded", this.checked);'/>&nbsp; &nbsp;<% =ResString("btnRAShowFundedOnly")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px;'><label style='margin-left:6px'><input type='checkbox' id='cbOptShowTinyBars' <% = IIF(RA_ShowTinyBars, " checked", "") %> onclick='onChangeSettings("showbars", this.checked);'/>&nbsp; &nbsp;<% =ResString("btnRAShowBars")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
            <telerik:RadToolBarButton runat="server" EnableViewState="false">
                <ItemTemplate><div class='toolbarcustom' style='margin-left:-2px;'><label style='margin-left:6px'><input type='checkbox' id='cbOptShowFPPrty' <% = IIf(RA_UseFPPrty, " checked", "") %> onclick='onChangeSettings("showprty", this.checked);'/>&nbsp; &nbsp;<% =ResString("btnRAShowFPPrty")%></label></div></ItemTemplate>
            </telerik:RadToolBarButton>
        </Buttons>
        </telerik:RadToolBarDropDown>
    </Items>
</telerik:RadToolBar>
</td></tr>

<tr valign='top' id="trGrid">
<td class='text' id="tdGrid" style="height:100%;">
<table border='0' cellspacing='0' cellpadding='0' style="margin-top:100px" runat='server' id='tblGrid' class='table table-striped-info mb-0' EnableViewState="false">

<%--<tr style="height:3em"><td align="center" valign="middle" class='text' style='padding:4px'><h5><% = String.Format(ResString("lblRAFundingPoolsTitle"), GetConstraintName())%></h5><% =GetMessage %></td></tr>--%>
<tr style="height:3em">
    <td class="m-0 header-content-wrapper d-flex jus-con-sb align-center-info" style='padding:4px;float:left' align="left">
        <div class="pt-0 head-title-wrapper flex-wrap">
        <h5 class="head-title-info col-blue"><% = String.Format(ResString("lblRAFundingPoolsTitle"), GetConstraintName())%></h5>
    </div>
    <div class="toggle-on-off-btn-info ms-4">
        <label class="txt-info text-capitalize text-nowrap"><b class="col-blue">Auto solve</b></label>
        <label class="toggle-switch-info">
            <input type="checkbox" checked="checked" id="toggle-list-view">
            <span class="toggle-slider-info"></span>
        </label>
    </div>
       
 </td>
</tr>

<tr style='width:100%;height:300px'>
    <td style="width:100%" align='center' colspan="4" valign='top'>
    <div id="divGrid" class="table-responsive"  style="text-align:center;">
        <asp:GridView EnableViewState="false" AllowSorting="false" AllowPaging="false" 
            ID="GridAlternatives" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" 
            CellPadding="0" TabIndex="1" HorizontalAlign="Center" AutoGenerateColumns="false" SelectedIndex="0" 
            CssClass="table table-striped-info mb-0" style="width:100%" ShowFooter="true">
            <RowStyle VerticalAlign="Middle" HorizontalAlign="Right" CssClass="text grid_row" Height="1em" Wrap="false"/>
            <AlternatingRowStyle VerticalAlign="Middle" HorizontalAlign="Right" CssClass="text grid_row_alt" Height="2em" Wrap="false"/>
            <HeaderStyle CssClass="bg-white" />
            <HeaderStyle CssClass="col-blue position-relative bg_midgrey align-middle" />
            <FooterStyle CssClass="text bg_midgrey" Wrap="false" Font-Italic="false" VerticalAlign="Top" />
            <EmptyDataRowStyle CssClass="text grid_row" />
            <EmptyDataTemplate><h6 style='margin:8em'><nobr><%=ResString("msgRiskResultsNoData")%></nobr></h6></EmptyDataTemplate>
            <Columns>
                <asp:BoundField HeaderText="ID" DataField="SortOrder" ReadOnly="true" HtmlEncode="true" ItemStyle-Wrap="false" ItemStyle-CssClass="" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField HeaderText="Alternative Name" DataField="Name" ReadOnly="true" HtmlEncode="true" ItemStyle-Wrap="false" ItemStyle-CssClass="" ItemStyle-HorizontalAlign="Left" />
                <asp:TemplateField HeaderText="Cost"  ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="98" ItemStyle-Width="98">
                    <ItemTemplate><div style="padding:0px 4px;"><asp:TextBox ID="tbCost" runat="server" SkinID="TextInput" CssClass="form-control" Text='<%#Eval("Cost")%>' Width="100%"/></div></ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    
    </div></td></tr>
    <tr>
       <td></td><td>
           <div class="text-end">
               <asp:Button ID="btnSolveOnLoad" OnClientClick="OnLoadSolveButton" CssClass="btn rounded-10 sg-color-info text-white" runat="server" Text="Solve" />
             <%-- <button runat="server"  type="button" class="btn rounded-10 sg-color-info text-white">
                        <img class="me-2" src="../../Images/img/check-svgrepo-com.svg" alt="img">
                        <span class="px-3">Solve</span>
               </button>--%>
           </div>
        </td>
    </tr>
</table>
</td></tr>
</table>

</div>
<telerik:RadCodeBlock runat="server" ID="radCodeBlockPools">
<div id="divPools" style="display:none;">
<h6><% =ResString("mnuRAFundingPools")%>:</h6>
<div style="overflow:auto;" id="pPools"><p align="center" style="padding:0; margin:0"><table id="tblPools" border='0' cellspacing='1' cellpadding='2' style='width:96%' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header">
        <th style="width:20px;">&nbsp;</th>
        <th><% =ResString("lblFormName")%></th>
        <% If RA_OPT_FPOOLS_ALLOW_ENABLED_PROPERTY Then%><th style="width:85px"><% = JS_SafeString(ResString("tblRAFPEnabled"))%></th><% End If %>
        <th style="width:40px"><% =ResString("tblAction")%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></p></div>
</div>
</telerik:RadCodeBlock>
</asp:Content>