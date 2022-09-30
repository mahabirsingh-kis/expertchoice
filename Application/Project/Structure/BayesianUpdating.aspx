<%@ Page Language="VB" Inherits="BayesianUpdatingPage" title="Bayesian Updating" Codebehind="BayesianUpdating.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script language="javascript" type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script language="javascript" type="text/javascript">

    var isReadOnly = <%=Bool2JS(IsReadOnly)%>;

    /* Datatables datagrid */
    var datagrid   = null;
    var cur_order  = [0, "asc"];
    var datagrid_data = <% = GetBayesianData() %>;
    var datagrid_columns = <% = GetColumns() %>;

    var pagination = <%=PM.Parameters.Riskion_Control_Pagination%>;

    var COL_ID = "id";
    var COL_INDEX = "alt_idx";
    var COL_NAME = "name";
    var COL_PRIOR_ODDS = "prior_odds";
    var COL_PRIOR = "prior";
    var COL_SOURCE_OF_PRIOR = "source_prior";
    var COL_PROB_E = "prob_e";
    var COL_PROB_E_COMMENT = "prob_e_comment";
    var COL_PROB_NOT_E = "prob_not_e";
    var COL_PROB_NOT_E_COMMENT = "prob_not_e_comment";
    var COL_LR = "lr";
    var COL_DATA_GUID = "data_guid";
    var COL_DATA_NAME = "data_name";
    var COL_DATA_COMMENT = "data_comment";
    var COL_ACTIONS = "actions";
    var COL_PRIOR_E_ODDS = "prior_e_odds";
    var COL_POSTERIOR_E_ODDS = "post_e_odds";
    var COL_POSTERIOR_E = "post_e";

    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_CLASS = 3;
    var COL_COLUMN_TYPE = 4;
    var COL_COLUMN_SORTABLE = 5;
    var COL_COLUMN_DATAPROP = 6;

    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;

    var calc_mode = <%=CInt(BayesianMode)%>;
    var bmProbability = 0;
    var bmOdds = 1;

    var dlg_edit, dlg_dict;
    var cancelled = false;

    var current_data_guid;

    /* Callback */
    function sendCommand(params, show_please_wait) {
        if (show_please_wait) showLoadingPanel();
        cmd = params;

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function syncReceived(params) {
        var rd = ["",[]];
        
        if ((params)) {
            rd = JSON.parse(params);
            if (rd[0] == "add_an_update_info" || rd[0] == "delete_info" || rd[0] == "set_pde" || rd[0] == "set_pd_e") {    
                datagrid_data = rd[1];
                initDatagrid();
                resizeDatatable();
            }
            if (rd[0] == "get_event_pe") {                    
                var tb = document.getElementById("tbPosteriorPE");
                if (rd[1] !== <% = UNDEFINED_INTEGER_VALUE %>) {
                    tb.innerText = rd[1];
                    jqHighlight(tb);
                } else {
                    tb.innerText = "<% = ResString("lblNA") %>";
                }
                tb = document.getElementById("tbPriorPE");
                if (rd[2] !== <% = UNDEFINED_INTEGER_VALUE %>) {
                    tb.innerText = rd[2];
                } else {
                    tb.innerText = "<% = ResString("lblNA") %>";
                }
            }
        }

        updateToolbar();
        hideLoadingPanel();
    }

    function jqHighlight(element, duration) {
        duration = duration || 800;
        $(element).effect("highlight", {}, duration);
    }

    function updateToolbar() {

    }

    function syncError() {
        hideLoadingPanel();
        dxConfirm("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined);
    }
    /* end Callback */

    // Datatables datagrid
    function initDatagrid() {

        dataTablesDestroy(datagrid, "tableContent");        

        var btn_styles = "ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only";
                
        //init columns headers                
        var columns = [];
        for (var i = 0; i < datagrid_columns.length; i++) {
            columns.push({ "title" : datagrid_columns[i][COL_COLUMN_NAME] + "", "class" : datagrid_columns[i][COL_COLUMN_CLASS], "type" : datagrid_columns[i][COL_COLUMN_TYPE], "sortable" : datagrid_columns[i][COL_COLUMN_SORTABLE], "searchable" : false, "bVisible" : datagrid_columns[i][COL_COLUMN_VISIBILITY], "data" : datagrid_columns[i][COL_COLUMN_DATAPROP] });

            if (datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_DATA_NAME) {
                columns[columns.length - 1].createdCell = function (cell, cellData, rowData, rowIndex, colIndex) {
                    //var s = "<input type='text' class='input' readonly autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='width:250px; text-align: left;' value='" + rowData[COL_DATA_NAME] + "' data-id='" + rowData[COL_DATA_GUID] + "' onclick='this.select();' />";
                    var s = "<b><span style='min-width:250px; text-align: left; word-wrap: break-word;'>" + rowData[COL_DATA_NAME] + "</span></b>";
                    if (rowData[COL_DATA_COMMENT] !== "") s += "<br><i class='grey-text'>" + rowData[COL_DATA_COMMENT] + "</i>";
                    cell.innerHTML = s;
                };
            }     

            if (datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_PROB_E) {
                columns[columns.length - 1].createdCell = function (cell, cellData, rowData, rowIndex, colIndex) {
                    var s = "<input type='text' class='input' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='width:80px; text-align: right;' value='" + (rowData[COL_PROB_E] == UNDEFINED_INTEGER_VALUE ? "" : rowData[COL_PROB_E]) + "' data-id='" + rowData[COL_DATA_GUID] + "' onclick='this.select();' onchange='saveValue(\"set_pde\",this.value,\"" + rowData[COL_DATA_GUID] + "\");' >"
                    if (rowData[COL_PROB_E_COMMENT] !== "") s += "<br><i class='grey-text'>" + rowData[COL_PROB_E_COMMENT] + "</i>";
                    cell.innerHTML = s;
                };
            }

            if (datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_PROB_NOT_E) {
                columns[columns.length - 1].createdCell = function (cell, cellData, rowData, rowIndex, colIndex) {
                    var s = "<input type='text' class='input' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='width:80px; text-align: right;' value='" + (rowData[COL_PROB_NOT_E] == UNDEFINED_INTEGER_VALUE ? "" : rowData[COL_PROB_NOT_E]) + "' data-id='" + rowData[COL_DATA_GUID] + "' onclick='this.select();' onchange='saveValue(\"set_pd_e\",this.value,\"" + rowData[COL_DATA_GUID] + "\");' >";                    
                    if (rowData[COL_PROB_NOT_E_COMMENT] !== "") s += "<br><i class='grey-text'>" + rowData[COL_PROB_NOT_E_COMMENT] + "</i>";
                    cell.innerHTML = s;
                };
            }
          
            if (datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_PRIOR_ODDS || datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_PRIOR || datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_LR || datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_PRIOR_E_ODDS || datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_POSTERIOR_E_ODDS || datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_POSTERIOR_E) {
                columns[columns.length - 1].createdCell = function (cell, cellData, rowData, rowIndex, colIndex) {
                    if (cellData == UNDEFINED_INTEGER_VALUE) cell.innerHTML = "";
                };
            }

            if (datagrid_columns[i][COL_COLUMN_DATAPROP] == COL_ACTIONS) {
                columns[columns.length - 1].createdCell = function (cell, cellData, rowData, rowIndex, colIndex) {
                    cell.innerHTML = "<i class='fas fa-pencil-alt ec-icon' style='cursor: pointer;' onclick='addDataClick(\"" + rowData[COL_ID] + "\", \"" + rowData[COL_DATA_GUID] + "\");'></i>&nbsp;&nbsp;<i class='fas fa-trash-alt ec-icon' style='cursor: pointer;' onclick='deleteDataClick(\"" + rowData[COL_DATA_GUID] + "\",\"" + rowData[COL_DATA_NAME] + "\");'></i><!--&nbsp;<i class='fas fa-arrow-up ec-icon' style='opacity: 0.5; cursor: pointer;'></i>&nbsp;<i class='fas fa-arrow-down ec-icon' style='opacity: 0.5; cursor: pointer;'></i>-->";
                };
            }
        }
                
        datagrid = $("#tableContent").DataTable({
            destroy: true,
            dom: 'Bfprti',
            data: datagrid_data,
            columns: columns,
            paging:    false,
            "pageLength": pagination > 0 ? pagination : 20,
            ordering:  false,
            order: cur_order,
            colReorder: false,
            scrollY: 245,
            scrollX: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "bScrollCollapse": false,
            stateSave: false,
            searching: false,
            info:      false,
            "fnDrawCallback": function ( settings ) {
                var api = this.api();
                var rows = api.rows( {page:'current'} ).nodes();
                var last = null;
 
                api.column(3, {page:'current'} ).data().each( function ( group, i ) {
                    if ( last !== group ) {
                        var x = $(rows).eq(i);
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan="15" style="background-color: #f0f5ff; cursor: default; font-size: larger;">[' + (datagrid_data[i].alt_event_type == <% = CInt(EventType.Risk)%> ? "<% = ParseString("%%Risk%%") %>" : "Opportuinity") + '] ' + group + "&nbsp;&nbsp;<i class='fas fa-plus-square ec-icon' style='cursor: pointer;' onclick='addDataClick(" + datagrid_data[i].alt_idx + ");'></i>" + '</td></tr>'
                        );
 
                        last = group;
                    }
                });

            },
            "language" : {"emptyTable" : "<h6 style='margin:2em 10em'><nobr><% =GetEmptyMessage()%></nobr></h6>"},
        });
       
        $('#tableContent').on('order.dt', function () {
            if ((datagrid)) cur_order = datagrid.order()[0];
        } );                       

        dataTablesInit(datagrid);

        resizeDatatable();
    }    

    function resizeDatatable() {
        if ((datagrid)) {
            dataTablesResize('tableContent', 'divGrid', 0);
            dataTablesRefreshHeaders('tableContent');
        }
    }
    // end Datatables datagrid

    // Actions
    function setMode(value) {
        calc_mode = value;
        sendCommand("action=calc_mode&value=" + value, true);
    }    

    function addDataClick(eventIndex, dataGuid) {
        cancelled = false;
        dlg_edit = $("#editorForm").dialog({
            autoOpen: false,
            modal: true,
            width: 570,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: "Add Data",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                    dlg_edit.dialog( "close" );
                }},
                Cancel: function() {
                    cancelled = true;
                    dlg_edit.dialog( "close" );
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");                

                var cbEvents = document.getElementById("cbEvents");
                if (typeof eventIndex == "undefined") {
                    cbEvents.disabled = false;
                } else {
                    cbEvents.disabled = true;
                    if (cbEvents.options.length > eventIndex) cbEvents.selectedIndex = eventIndex;
                }
                var tb_pde = $get("tbPDE");
                tb_pde.value = (0).toFixed(2);
                tb_pde.focus();
                tb_pde.select();

                var tb_pde_comment = $get("tbPDEComment");
                tb_pde_comment.value = "";

                var tb_pd_e = $get("tbPD_E");
                tb_pd_e.value = (0).toFixed(2);

                var tb_pd_e_comment = $get("tbPD_EComment");
                tb_pd_e_comment.value = "";

                var tb_data_name = $get("tbDataName");
                tb_data_name.value = "";
                
                var tb_data_comment = $get("tbDataComment");
                tb_data_comment.value = "";

                if (typeof dataGuid !== "undefined") {
                    var data;
                    for (var t = 0; t < datagrid_data.length; t++) {
                        if (datagrid_data[t][COL_DATA_GUID] == dataGuid) {
                            data = datagrid_data[t];
                            break;
                        }
                    }
                    cbEvents.value = data[COL_ID];
                    tb_data_name.value = data[COL_DATA_NAME];
                    tb_data_comment.value = data[COL_DATA_COMMENT];
                    tb_pde.value = data[COL_PROB_E];
                    tb_pde_comment.value = data[COL_PROB_E_COMMENT];
                    tb_pd_e.value = data[COL_PROB_NOT_E];
                    tb_pd_e_comment.value = data[COL_PROB_NOT_E_COMMENT];
                }

                current_data_guid = dataGuid;
                getEventPE();
            },
            close: function() {
                $("body").css("overflow", "auto");                
                if ((!cancelled) && ($("#cbEvents").val() != "") && ($("#tbDataName").val().trim() != "")) {
                    sendCommand('action=add_an_update_info&event_id=' + $("#cbEvents").val() + '&data_name=' + $("#tbDataName").val().trim() + '&pde=' + $("#tbPDE").val() + '&pd_e=' + $("#tbPD_E").val() + "&data_comment=" + encodeURIComponent($("#tbDataComment").val()) + "&pde_comment=" + encodeURIComponent($("#tbPDEComment").val()) + "&pd_e_comment=" + encodeURIComponent($("#tbPD_EComment").val()) + (typeof dataGuid == "undefined" ? "" : "&data_guid=" + dataGuid));
                }
            }
        });
        dlg_edit.dialog("open");
    }

    function deleteDataClick(data_guid, data_name) {
        dxConfirm(replaceString("{0}", data_name, resString("msgSureDeleteCommon")), function () {
            sendCommand("action=delete_info&data_guid=" + data_guid);
        });
    }

    function saveValue(action, value, data_guid) {
        sendCommand("action=" + action + "&value=" + value + "&data_guid=" + data_guid);
    }

    function useBayesianDataGloballyClick(chk) {
        sendCommand("action=bayes_global&value=" + chk, true);
    }

    var old_tbPDEval, old_tbPD_Eval;

    function getEventPE() {
        //var tb = document.getElementById("tbPosteriorPE");
        //tb.innerText = "";
        //tb = document.getElementById("tbPriorPE");
        //tb.innerText = "";

        var tbPDEval = $("#tbPDE").val();
        var tbPD_Eval = $("#tbPD_E").val();

        if (tbPDEval !== old_tbPDEval || tbPD_Eval !== old_tbPD_Eval) {
            old_tbPDEval = tbPDEval;
            old_tbPD_Eval = tbPD_Eval;
            sendCommand('action=get_event_pe&event_id=' + $("#cbEvents").val() + '&pde=' + $("#tbPDE").val() + '&pd_e=' + $("#tbPD_E").val() + "&data_guid=" + ((current_data_guid) && typeof current_data_guid !== "undefined" ? current_data_guid : ""));
        }
    }
    // end Actions

    var toolbarItems = [{
            location: 'before',
            widget: 'dxButton',
            locateInMenu: 'auto',
            options: {
                icon: "fas fa-plus-square",
                text: "Add Data",
                showText: true,
                elementAttr: {id: 'btnAddData' },
                onClick: function() {
                    addDataClick();
                }
            }
        }, {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxSelectBox',
            visible: false,
            options: {
                searchEnabled: false,
                valueExpr: "ID",
                value: <% = CInt(BayesianMode) %>,
                disabled: true,
                displayExpr: "Text",
                elementAttr: {id: 'btn_switch_mode'},
                onValueChanged: function (data) { 
                    onSetView(data.value);
                },
                items: [{"ID": <%=CInt(BayesianModes.bmProbability)%>, "Text": "Probability"}, {"ID": <%=CInt(BayesianModes.bmOdds)%>, "Text": "Odds"}]
            }
        }, {
            location: 'before',
            widget: 'dxCheckBox',
            locateInMenu: 'auto',
            options: {
                text: "<%=ParseString("Use Bayesian computation globally")%>",
                showText: true,
                elementAttr: {id: 'cbUseBayesianDataGlobally'},
                disabled: true,
                value: false,
                onValueChanged: function(e) {
                    useBayesianDataGloballyClick(e.value);
                }
            }
        }
    ];

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItems
        });

        $("#toolbar").css("margin-bottom", 0); 
    }

    function hotkeys(event) {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            switch (code) {
                case KEYCODE_LEFT:
                case KEYCODE_RIGHT:
                    break;
                case KEYCODE_ESCAPE:
                    break;
            }
        }
    }

    function resizePage() {
        $("#divGrid").height(100);
        var m = $("#tblMain").height();
        var r0 = $("#trToolbar").height();
        var r1 = $("#trTitle").height();
        if ((m) && (r0) && (r1)) {
            $("#divGrid").height(m - r0 - r1 - 2);
        }

        resizeDatatable();
    }

    $(document).ready(function () {
        initToolbar();
        resizePage();
        initDatagrid();
    });

    function initTableMainSize() {
        $("#tdMain").attr("valign", "top");
        $("#tblMain").attr("valign", "top");
    }

    document.onkeypress = hotkeys;
    resize_custom = resizePage;

</script>

<table id="tblMain" border='0' cellspacing='0' cellpadding='0' class='whole'>
    <tr valign='top' id="trToolbar" class='text'>
        <td valign="top" colspan='2'>
            <div id='toolbar' class='dxToolbar'></div>
        </td>
    </tr>
    
    <tr id="trTitle" valign='top' align='left' class='text'>
        <td style="height:24px; padding:14px 4px 0px 4px;" class="text" colspan='2'>
            <h5 id="lblPageTitle" style='margin: 0px 30px;'><%=ResString("titleBayesianUpd")%> for &quot;<% = ShortString(SafeFormString(App.ActiveProject.ProjectName), 85)%>&quot;</h5>
        </td>
    </tr>
    
    <tr valign='top' style='height:100%;'>
        <td id='tdContent' class="whole" valign='top'>
            <div id='divContent' class="whole">
                <!-- Grid -->
                <div id='divGrid' style="margin: 0px; text-align: center;">
                    <table id='tableContent' class='text cell-border order-column'></table>
                </div>    
            </div>
        </td>
    </tr>
</table>

<%-- Add data dialog --%>
<div id='editorForm' style='display:none;'>
    <table border='0' cellspacing='0' cellpadding='5' class='text' style='margin-right:5px; font-family: serif; font-size: medium;'>
        <tr>
            <td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'>
                <nobr><% = ParseString("%%Alternative%% Name:") %>&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <%=GetEventsDropdown()%>
            </td>
        </tr>
        <tr>
            <td valign='top' align='right' style='width:100px;'>
                <nobr>Prior P(E):&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <span id="tbPriorPE" style="width: 400px;" />
            </td>
        </tr>
        <tr>
            <td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'>
                <nobr><% = ParseString("Data Name (*):") %>&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <input type="text" id="tbDataName" style="width: 406px;" />
            </td>
        </tr>
        <tr>
            <td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'>
                <nobr><% = ParseString("Data Comment:") %>&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <textarea type="text" id="tbDataComment" style="width: 404px;"></textarea>
            </td>
        </tr>
        <tr>
            <td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'>
                <nobr>P (I/E) (*):&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <nobr>
                <input type='number' class='input number' min="0" max="1" step="0.1" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='vertical-align: top; width: 100px; text-align: right;' id='tbPDE' value='' onclick='this.select();' onkeyup="getEventPE();" onchange="getEventPE();" />
                <span style="width: 80px; display: inline-block; text-align: right; vertical-align: top;">Comment:</span>
                <textarea type='text' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='width: 214px; text-align: left;' id='tbPDEComment' value='' onclick='this.select();'></textarea>
                </nobr>
            </td>
        </tr>
        <tr>
            <td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'>
                <nobr>P (I/&#274;) (*):&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <nobr>
                <input type='number' class='input number' min="0" max="1" step="0.1" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='vertical-align: top; width: 100px; text-align: right;' id='tbPD_E' value='' onclick='this.select();' onkeyup="getEventPE();" onchange="getEventPE();" />
                <span style="width: 80px; display: inline-block; text-align: right; vertical-align: top;">Comment:</span>
                <textarea type='text' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' style='width: 214px; text-align: left;' id='tbPD_EComment' value='' onclick='this.select();'></textarea>
                </nobr>
            </td>
        </tr>
        <tr>
            <td valign='top' align='right' style='width:100px;'>
                <nobr>Posterior P(E):&nbsp;&nbsp;</nobr>
            </td>
            <td>
                <span id="tbPosteriorPE" style="width: 400px;" />
            </td>
        </tr>
    </table>
</div>

</asp:Content>