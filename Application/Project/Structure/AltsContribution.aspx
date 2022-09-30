<%@ Page Language="VB" Inherits="AltsContributionPage" title="Alternatives Contribution" Codebehind="AltsContribution.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/ec.dg.js"></script>
    <script>
        /* jQuery Ajax */
        function syncReceived(params) {
            if ((params)) {            
                var received_data = eval(params);
                if ((received_data)) {
                    if (received_data[0] == 'ok-syncprojs'){

                    };
                    if (received_data[0] == 'reload'){
                        document.location.href = '<% =PageURL(CurrentPageID) %>?<% =GetTempThemeURI(true) %>';
                    };
                    if (received_data[0] == 'contributed_nodes') {                        
                        initContributionsDlg(received_data[1]);
                        dlg_contributions.dialog('open');
                    }
                }
            }        
        }

        function syncError() {
            hideLoadingPanel();
            DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
            $(".ui-dialog").css("z-index", 9999);
        }

        function sendCommand(params) {
            cmd = params;
            _ajax_ok_code = syncReceived;
            _ajax_error_code = syncError;
            _ajaxSend(params);
        }

        //A1203 ===
        var dlg_contributions = null;
        var selected_event_name = "";

        function showContributedNodes(event_id, event_name) {                
            selected_event_name = event_name;
            sendCommand('action=contributed_nodes&event_id=' + event_id);
        }

        function initContributionsDlg(nodes) {
            dlg_contributions = $("#divContributions").dialog({
                autoOpen: false,
                width: 490,
                height: 350,
                modal: true,
                closeOnEscape: true,
                dialogClass: "no-close",
                bgiframe: true,
                title: "<%=IIf(App.isRiskEnabled, IIf(PM.ActiveHierarchy = ECHierarchyID.hidImpact, ParseString("Sources of "), ParseString("Objectives of ")), ParseString("Contributions of ")) %>" + selected_event_name,
                position: { my: "center", at: "center", of: $("body"), within: $("body") },
                buttons: [{ text:"Close", click: function() { dlg_contributions.dialog( "close" ); }}],
                open:  function() { },
                close: function() { }
            });
            $(".ui-dialog").css("z-index", 9999);
            var txt = "";
            for (var i = 0; i < nodes.length; i++) {
                txt += nodes[i][1] + "<br>";
            }
            $("#divContributions").html(txt);
        }
        //A1203 ==

        //A1201 ===        
        
        var showAttributes = '<%=SelectedColumns(App.ActiveProject)%>';
        var attr_list = <%= GetAttributes() %>;
        var dlg_select_columns = null;
        var cancelled = false;

        function getAttrById(id) {        
            for (var j = 0; j < attr_list.length; j++) {
                if (attr_list[j].guid == id) return attr_list[j];            
            }
            return null;
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
                var checked = showAttributes.indexOf(attr_list[k].guid) != -1;
                labels += "<label><input type='checkbox' class='select_clmn_cb' value='" + attr_list[k].guid + "' " + (checked ? " checked='checked' " : " " ) + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + attr_list[k].name + "</label><br>";
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
                        $.each(cb_arr, function(index, val) { var cid = val.value + ""; if (val.checked) { clmn_ids += (clmn_ids == "" ? "" : ",") + cid; } });
                        showAttributes = clmn_ids;
                        _ajaxSend('action=select_columns&column_ids=' + clmn_ids); // save the selected columns via ajax
                        options.showAttributes = showAttributes;  // update the visible columns option                        
                        $('#dataGridTable').datagrid('option', 'showAttributes', showAttributes);    // update the datagrid option
                        $('#dataGridTable').datagrid('redraw'); // redraw datagrid
                    }
                }
            });
            $(".ui-dialog").css("z-index", 9999);
        }

        function filterColumnsCustomSelect(chk) {
            $("input:checkbox.select_clmn_cb").prop('checked', chk*1 == 1);
            dxDialogBtnDisable(true, false);
        }

        var options = {
            rows: <%= GetRows() %>,
            columns: <%= GetColumns() %>,
            rowsTitle: "<%=ParseString("%%Alternatives%%")%>",
            viewMode: 'contributions',    
            showContributions: <%=Bool2JS(ShowContributions) %>,
            showNoSources: false,
            showAttributes: showAttributes, //A1201
            attributes: attr_list,          //A1201
            indexColumn: '<%=IDColumnMode%>', //A1202
            imgPath: '<%=ImagePath%>',
            contextMenu: true,
            hid: <%=IIf(App.isRiskEnabled, CInt(PM.ActiveHierarchy), -1)%>,
            readonly: <%=Bool2JS(App.IsActiveProjectStructureReadOnly)%>
        }

        //A1201 ==

        $(document).ready(function () {
            initToolbar(); 
            if (options.readonly) DevExpress.ui.notify(resString("lblProjectReadOnly"), "warning");
        });
        
        document.onclick = function () { 
            if (is_context_menu_open == true) { $("div.context-menu").hide(200); is_context_menu_open = false; } 
        }    

        function initToolbar() {
            var disabled =  <%=Bool2JS(App.IsActiveProjectStructureReadOnly OrElse Not ShowContributions())%>;
            $("#divToolbar").dxToolbar({
                items: [{
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    options: {
                        disabled: disabled,
                        text: resString("lblAltsContribSelectAllNodes"),
                        onClick: function () {
                            $('#dataGridTable').datagrid('setAllContributions', true);
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    options: {
                        disabled: disabled,
                        text: resString("lblAltsContribUnSelectAllNodes"),
                        onClick: function () {
                            $('#dataGridTable').datagrid('setAllContributions', false);
                        }
                    }
                }, {
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    options: {
                        disabled: false,
                        text: "Select Columns",
                        onClick: function () {
                            onSelectColumnsClick();
                        }
                    }
                }
            ]
            });
        }

        resize_custom = resizePage;

        function resizePage(force, w, h) {
            //$("#divContent").height(h);
            //$("#divContent").width(w);
            //var toolbarHeight = ((($("#divToolbar"))) ? $("#divToolbar").height():0);
            //if (typeof ((toolbarHeight)) == "undefined") {
            //    toolbarHeight = 0;
            //};
            var headersHeight = 0;
            $(".header-row").each(function (index, el) { headersHeight += $(el).height(); });
            if ($("#dataGridTable").hasClass("emptyCanvas")) {
                options["width"] = w-24;
                options["height"] = h-headersHeight;
                $("#dataGridTable").datagrid(options);
                $("#dataGridTable").removeClass("emptyCanvas")
            };
            $('#dataGridTable').hide();
            var td = $get("tdCanvas");
            h = td.clientHeight;
            $('#dataGridTable').show().datagrid('resize', w-24, h);
            $('#dataGridTable').datagrid('redraw');           
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

        document.onkeypress = Hotkeys;

    </script>
    <table border="0" cellpadding="0" cellspacing="0" class="whole">
        <tr class="header-row" valign="top" style="height: 24px;"><td><div id='divToolbar' class='dxToolbar'></div></td></tr>
        <tr class="header-row" valign="top" style="height: 24px;"><td><h5><% = If(Not PM.IsRiskProject, ResString("titleReportObjectivesAndAlternatives"), If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("Vulnerability Of %%Alternatives%% To %%Objectives(l)%%"), ParseString("Consequences Of %%Alternatives%% On %%Objectives(i)%%"))) %></h5></td></tr>
<%--    <div id='divToolbar' class='text ec_toolbar'>
        <table class="text">
            <tr>
                <td class="ec_toolbar_td">
                    <input type="button" class="button" <%=If(Not App.IsActiveProjectStructureReadOnly And ShowContributions(), " ", "disabled='disabled'")%> value="Check All" onclick="$('#dataGridTable').datagrid('setAllContributions', true); return true;" /></td>
                <td class="ec_toolbar_td">
                    <input type="button" class="button" <%=If(Not App.IsActiveProjectStructureReadOnly And ShowContributions(), " ", "disabled='disabled'")%> value="Uncheck All" onclick="$('#dataGridTable').datagrid('setAllContributions', false); return true;" /></td>
                <%If PM.Attributes.GetAlternativesAttributes().Count > 0 Then%>
                <td class="ec_toolbar_td">
                    <a href="#" Class="actions" id="hbSelectColumns" style="text-align: center; font-size: 14px; cursor: pointer;" onclick="onSelectColumnsClick();">Select Columns</a>
                    </td>
                <%End If%>
            </tr>
        </table>
    </div>--%>
        
        <tr><td id="tdCanvas"><canvas id="dataGridTable" class="emptyCanvas">HTML5 not supported</canvas></td></tr>
    </table>
    <div id='selectColumnsForm' style='display: none; position: relative;'>
        <div id="divSelectColumns" style="padding: 5px; text-align: left;"></div>
        <div style='text-align: center; margin-top: 1ex; width: 100%;'>
            <a href="" onclick="filterColumnsCustomSelect(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
            <a href="" onclick="filterColumnsCustomSelect(0); return false;" class="actions"><% =ResString("lblNone")%></a>
        </div>
    </div>

    <div id="divContributions" style="display:none; overflow:auto; position: relative;">
    </div>

</asp:Content>