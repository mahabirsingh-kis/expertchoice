<%@ Page Language="VB" Inherits="WordingTemplatePage" title="Wording Template" Codebehind="WordingTemplate.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">
    
    var isReadOnly = "<%=App.ActiveWorkgroup.Status <> ecWorkgroupStatus.wsEnabled%>" === "True";

    var OPT_ROW_COLOR           = "#ffffff";
    var OPT_ROW_COLOR_ALT       = "#f5f5f5";
    var OPT_HIGHLIGHT_COLOR     = "#f0f5ff";
    var OPT_HIGHLIGHT_COLOR_ALT = "#eaf0f5";    

    var rows = <%=GetTemplatesList()%>;
    var rows_orig = <%=GetTemplatesList()%>;
    var on_hit_enter = ";";
    var old_index = -1;
    var edit_action_html = "";
    var has_changes = false;

    <%--var show_confirm = "<%=ShowConfirmation()%>" === "True";--%>

    <%--var conf_text = "<div style='max-width:500px'><%=SafeFormString(String.Format(ResString("msgWordingTemplChange"), PageMenuItem(_PGID_PROJECT_OPTION_EVALUATE))) %></div><div style='margin-top:1ex'><nobr><input type='checkbox' class='input checkbox' id='cbDontShowAgain'><label for='cbDontShowAgain'><%=SafeFormString(ResString("msgDoNotShowAgain"))%></label></div>";--%>

    function TemplateRowHover(r, alt, hover) {
        if (alt === 1) {
            if (r.style.background !== OPT_HIGHLIGHT_COLOR_ALT) r.style.background = (hover === 1 ? OPT_HIGHLIGHT_COLOR_ALT : OPT_ROW_COLOR_ALT);
        } else {
            if (r.style.background !== OPT_HIGHLIGHT_COLOR) r.style.background = (hover === 1 ? OPT_HIGHLIGHT_COLOR : OPT_ROW_COLOR);
        }
    }

    function initTable() {
        var tbl = $("#tblMain");
        tbl.find("tr:gt(0)").remove();
        $.each(rows, function(index, val) {
            var title  = val[0];
            var value0 = val[2]; // singular
            var value1 = val[4]; // plural
            var value2 = val[6]; // past tense
            
            var isHeader = val[7] === 1; // is group header
            var isAlt = index % 2 === 0; // is alternate row

            var tdEdit = "<td id='tdEditAction" + index + "' align='right' style='word-wrap:break-word; border:0px solid #ccc; padding:4px;'>" + (isHeader ? "" : "<a href='' onclick='onEditTemplate(" + index + "); return false;'><i class='fas fa-pencil-alt'></i></a>") + "</td>";
            tbl.append('<tr class="text tbl_row' + (isAlt ? "_alt" : "") + '" valign="top" onmouseover="TemplateRowHover(this, ' + (isAlt ? "1" : "0") + ', 1);" onmouseout="TemplateRowHover(this, ' + (isAlt ? "1" : "0") + ', 0);"><td style="word-wrap:break-word; border:0px solid #ccc; padding:4px ' + (isHeader || <%=IIf(Not App.isRiskEnabled, "true", "false")%> ? '4' : '40') + 'px;" title="' + title + '">' + title + '</td><td id="tdValue0_' + index + '" style="word-wrap:break-word; border:0px solid #ccc; padding:4px;" title="' + value0 + '">' + value0 + '</td><td id="tdValue1_' + index + '" style="word-wrap:break-word; border:0px solid #ccc; padding:4px;" title="' + value1 + '">' + value1 + '</td><%If App.isRiskEnabled Then%><td id="tdValue2_' + index + '" style="word-wrap:break-word; border:0px solid #ccc; padding:4px;"' + (value2 != '' ? ' title="Examples: Treated, Controlled, or Mitigated"' : '') + ' ">' + value2 + '</td><%End If%>' + tdEdit + '</tr>');
        });        
    }

    function onEditTemplate(index) {
        onCancelChanges(old_index);
        old_index = index;
        edit_action_html = $("#tdEditAction" + index).html();
        $("#tdValue0_" + index).html("<input type='text' class='input' style='width:" + $("#tdValue0_" + index).width() + "' id='tbValue0' value='" + replaceString("'", "&#39;", rows[index][2]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='has_changes=true;'>");
        $("#tdValue1_" + index).html("<input type='text' class='input' style='width:" + $("#tdValue1_" + index).width() + "' id='tbValue1' value='" + replaceString("'", "&#39;", rows[index][4]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='has_changes=true;'>");
        <%If App.isRiskEnabled Then%>
        if (rows[index][6] != '') {
            $("#tdValue2_" + index).html("<input type='text' class='input' style='width:" + $("#tdValue2_" + index).width() + "' id='tbValue2' value='" + replaceString("'", "&#39;", rows[index][6]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='has_changes=true;'>");
        }
        <%End If%>
        $("#tdEditAction" + index).html("<a href='' onclick='onSaveChanges(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>' style='white-space: pre;'><i class='fas fa-save fa-lg'></i></a>&nbsp;<a href='' onclick='onCancelChanges(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>' style='white-space: pre;'><i class='fas fa-ban'></i></a>");
        setTimeout("$get('tbValue0').focus();", 50);
        on_hit_enter = "EditAttribute(" + index + ");";
        has_changes = false;
    }

    function onCancelChanges(index) {
        if (index >= 0) {
            $("#tdValue0_" + index).html(rows[index][2]);
            $("#tdValue1_" + index).html(rows[index][4]);
            $("#tdValue2_" + index).html(rows[index][6]);
            $("#tdEditAction" + index).html(edit_action_html);
        }
        has_changes = false;
    }

    function onSaveChanges(index) {
        var value0 = $("#tbValue0").val().trim();
        if (value0 !== "") rows[index][2] = value0;

        var value1 = $("#tbValue1").val().trim();
        if (value1 !== "") rows[index][4] = value1;

        var value2 = "";
        <%If App.isRiskEnabled Then%>
        value2 = $("#tbValue2").val();
        if ((value2) && value2.trim() !== "") rows[index][6] = value2.trim();
        <%End If%>

        onCancelChanges(index); // restoring the Edit button
        
        $("#btnApply").prop("disabled", false);
        $("#btnReset").prop("disabled", false);
        has_changes = false;
    }

    function onApplyClick() {
        onCancelChanges(old_index);
        <%--        if (show_confirm) {
            dxDialog(conf_text, "show_confirm = !$('#cbDontShowAgain').prop('checked'); doApplyChanges();", ";", "<%=ResString("msgWarning")%>");
        } else {
            doApplyChanges();
        }--%>
        doApplyChanges();
    }

    function doApplyChanges() {
        var args = "";
        for (i = 0; i < rows.length; i++) {
            if (rows[i][7] === 0) { // not isHeader
                args += "&" + rows[i][1] + "=" + rows[i][2].trim();
                args += "&" + rows[i][3] + "=" + rows[i][4].trim();
                <%If App.isRiskEnabled Then%>
                if (i == 4) { // Controls - Controlled
                    args += "&" + rows[i][5] + "=" + rows[i][6].trim();
                }
                <%End If%>
            }
        }
        //sendCommand("action=save&show_confirm=" + (show_confirm ? "1" : "0") + args);
        sendCommand("action=save" + args);
    }

    function onResetClick() {
        onCancelChanges(old_index);        
        rows = [];
        for (i = 0; i < rows_orig.length; i++) {
            rows.push(rows_orig[i].slice());
        }
        initTable();
    }

    function onRestoreClick() {
        onCancelChanges(old_index);
        <%--        if (show_confirm) {
            dxDialog(conf_text, "show_confirm = !$('#cbDontShowAgain').prop('checked'); doRestoreChanges();", ";", "<%=ResString("msgWarning")%>");
        } else {
            doRestoreChanges();
        }--%>
        doRestoreChanges();
    }
    
    function doRestoreChanges() {
        var args = "";
        for (i = 0; i < rows.length; i++) {
            if (rows[i][7] === 0) { // not isHeader
                args += "&" + rows[i][1] + "=";
                args += "&" + rows[i][3] + "=";
                <%If App.isRiskEnabled Then%>                
                if ((rows[i][5]) && rows[i][5] != "") args += "&" + rows[i][5] + "=";
                <%End If%>
            }
        }        
        //sendCommand("action=save&show_confirm=" + (show_confirm ? "1" : "0") + args);
        sendCommand("action=save" + args);
    }

    function sendCommand(params) {
        var cmd = params + "&rnd=" + Math.random();
        displayLoadingPanel(true);

        $.ajax({
            type: "POST",
            data: cmd,
            dataType: "text",
            async:true,
            beforeSend: function () {
                // check data if needed and "return false" to cancel
                return true;
            },
            success: function (data) {
               SyncReceived(data);
            },
            error: function () {
                displayLoadingPanel(false);
                DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
            }

        });
    }

    function SyncReceived(params) {
        if ((params)) {
            var received_data = eval(params);
            if ((received_data) && (received_data[0] == "save" || received_data[0] == "restore")) {
                //rows = eval(params);
                reloadPage();
            }
        }

        displayLoadingPanel(false);
    }

    function isSaved() {
        var eq = true;
        for (i = 0; i < rows_orig.length; i++) {
            for (j = 0; j < rows_orig[i].length; j++) {
                if (rows_orig[i][j] !== rows[i][j]) eq = false;
            }            
        }
        return eq;
    }

    function CheckExitEvent(is_logout) {        
        if (has_changes || !isSaved()) {
            var res = confirm("<%=JS_SafeString(ResString("msgUnloadUnsavedWordingTemplate"))%>");
            return res;
        } else {
            return true;
        }
    }

    $(document).ready( function () {
        initTable();
    });
  
</script>

<div style="text-align:center;">
    
    <h4 id="lblPageTitle">Wording Template for Workgroup &quot;<%=SafeFormString(App.ActiveWorkgroup.Name)%>&quot;</h4>
    
    <%If App.isRiskEnabled Then%>
    <span class='note' style='display:inline-block;margin-bottom:15px;'><%=SafeFormString(ResString("lblWordingTemplatesHeader1"))%></span>
    <%End If%>

    <center>
    <table id="tblMain" class="tbl" border="0" cellspacing="2" cellpadding="6" style="table-layout:fixed; border-collapse:collapse; background: rgb(240, 240, 240); border: 1px solid rgb(208, 208, 208); margin:1em 0px 3ex 0px;">
        <thead>
            <tr class="tbl_hdr h6">
                <td>Wording Template</td>
                <td>Singular</td>
                <td>Plural</td>
                <%If App.isRiskEnabled Then%>
                <td>Past</td>
                <%End If%>
                <td style='width:40px'></td> <%--Actions--%>
            </tr>
        </thead>
        <tbody>            
        </tbody>  
    </table>
    </center>

    <div style="text-align:center; margin-top:15px;">
        <%--<input type="button" class="button" id="btnApply" value="<%=ResString("btnApply") + "*"%>" onclick="onApplyClick();" disabled="disabled" />--%>
        <input type="button" class="button" id="btnApply" value="<%=ResString("btnApply")%>" onclick="onApplyClick();" disabled="disabled" />
        <input type="button" class="button" id="btnReset" value="<%=ResString("btnReset")%>" onclick="onResetClick();" disabled="disabled" />
        <input type="button" class="button" id="btnRestore" value="<%=ResString("btnResetTemplates")%>" style="width:200px;" onclick="onRestoreClick();" />
        <div style="margin-top:2em; max-width:500px; text-align:left; border-color: darkgray;" class="warning"><div style="display:inline-block; float:left; margin: 1em 1ex 1em 0px;"><i class="as_icon fas fa-exclamation-circle"></i></div><span style="font-size:0.72rem"><% =String.Format(ResString("msgWordingTemplChange"), PageMenuItem(_PGID_PROJECT_OPTION_EVALUATE)) %></span></div>
        <%--<br />
        <span class="small" style="display:block; text-align:left; margin:5px;">*&nbsp;<%=ResString("msgPageMustbeReloaded")%></span>--%>
    </div>

</div>
      
</asp:Content>

