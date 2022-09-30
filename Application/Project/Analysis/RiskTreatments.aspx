<%@ Page Language="VB" Inherits="RiskSelectTreatmentsPage" title="SelectTreatments" Codebehind="RiskTreatments.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script language="javascript" type="text/javascript" src="/Scripts/jszip.min.js"></script>
    <%If RA_ShowTimeperiods Then%>
    <script language="javascript" type="text/javascript" src="/Scripts/drawMisc.js"></script>
    <script language="javascript" type="text/javascript" src="/Scripts/RA.js"></script>
    <%End If%>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
    .dx-tab.dx-tab-selected .dx-item-content.dx-tab-content {
        font-weight: bold;
    }
    .dx-datagrid .dx-row > td {
        padding: 2px 3px;
        margin: 0;
    }
    #toolbar {
        margin-left: 0px;
        padding-left: 8px;
        background: #ffffff;
        border: 1px solid #cccccc;
        margin-top: -1px;
        margin-bottom: 0px;
    }
</style>
<script language="javascript" type="text/javascript">
    //var optionsVisible = !$(document.body).hasClass('fullscreen'); <%--<%=Bool2JS(ShowOptions)%>;--%>

    var imagePath = "<%=ImagePath%>";    

    var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
    var isEditable = <%=Bool2JS(IsEditable)%>; // is Identify - Controls Register page
    var isMultiselect = isEditable;
    var isOptimizationAllowed = <%=Bool2JS(isOptimizationAllowed)%>;
    var pagination = <%=PM.Parameters.Riskion_Control_Pagination%>;

    var marked_controls_list = [];

    var dlg_control_editor = null;
    var dlg_select_events = null;
    var dlg_select_controls = null;
    var dlg_scenarios;

    var scenarios = <%=GetScenariosData()%>;
    var scenario_active = <%=RA.Scenarios.ActiveScenarioID %>;

    var hierarchy_data = <% = GetHierarchyData() %>;
    var hierarchy_columns = <% = GetHierarchyColumns() %>;
    var selectedHierarchyNodeID = "<% = SelectedHierarchyNodeID.ToString %>";

    var view_grid = <% = Bool2JS(Not RA_ShowTimeperiods) %>;

    var cancelled = false;
    var new_cat_name = ""; //new category name
    var rename_cat_id = "";
    var pageNum = 0;
    var last_click_cb = "";
    var last_click_cb_oper = "";

    var mh = 500;   // maxHeight for dialogs;
    
    var cat_data     =  <% = GetCategoriesData() %>;
    var sEventIDs    = "<% = StringSelectedEventIDs%>";
    var sControlIDs    = "<% = StringSelectedControlIDs%>";

    var dlg_applications;
    var edit_uid = "";
    var edit_name = "";
    var edit_cost = -1;
    var edit_param = "";
    var edit_do_edit = 0;

    // data for categories
    var IDX_CAT_ID = 0;
    var IDX_CAT_NAME = 1;

    // data for applications dialog
    var IDX_CONTROL_TYPE = 0;
    var IDX_CONTROL_NAME = 1;
    var IDX_CONTROL_APPLICATIONS = 2;

    var all_obj_data = <% = GetAllObjsData()%>; // impact hierarchy for Dollar values
    
    var sources_list = <%=GetObjsData(ECHierarchyID.hidLikelihood)%>;
    var impacts_list = <%=GetObjsData(ECHierarchyID.hidImpact)%>;

    var event_list = <%=GetEventsData()%>;
    var events_data = event_list;
    var IDX_EVENT_ID = 0;
    var IDX_EVENT_NAME = 1;
    var IDX_EVENT_ENABLED = 2;

    var new_cat_name = ""; //new category name
    var rename_cat_id = "";

    var cat_data = <% = GetCategoriesData() %>;     //cat_data[]     - controls categories

    var ctUndefined = "<%=CInt(ControlType.ctUndefined)%>";
    var ctCause = <%=CInt(ControlType.ctCause)%>;
    var ctConsequenceOld = <%=CInt(ControlType.ctConsequence)%>; // OBSOLETE
    var ctCauseToEvent = <%=CInt(ControlType.ctCauseToEvent)%>;
    var ctConsequenceToEvent = <%=CInt(ControlType.ctConsequenceToEvent)%>;

    var ctUndefinedName = "<%=ControlTypeToString(ControlType.ctUndefined)%>";
    var ctCauseName = "<%=ControlTypeToString(ControlType.ctCause)%>";
    var ctCauseToEventName = "<%=ControlTypeToString(ControlType.ctCauseToEvent)%>";
    var ctConsequenceToEventName = "<%=ControlTypeToString(ControlType.ctConsequenceToEvent)%>";


    var CATEGORY_DELIMITER = ";";
    var CHAR_CURRENCY_SYMBOL = "<%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%>";
    var CurrencyThousandSeparator = "<% =UserLocale.NumberFormat.CurrencyGroupSeparator %>";

    var controls_data = <% = GetControlsData() %>;  
    var total_risk = <%=JS_SafeNumber(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue)%>;
    var total_risk_wc = <%=JS_SafeNumber(RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls)%>;

    var columns_ver = "2.0";

    // visible columns
    var COL_CTRL_ID = "id";
    var COL_CTRL_GUID = "guid";
    var COL_CTRL_IS_ACTIVE = "active";
    var COL_CTRL_NAME = "name";
    var COL_CTRL_TYPE = "type";
    var COL_CTRL_FUNDED = "funded"; // "Selected" readonly column
    var COL_CTRL_COST = "cost";
    var COL_CTRL_APPL_COUNT = "app_cnt";
    var COL_CTRL_VIEW_CATEGORIES = "cat";
    var COL_CTRL_DISABLED = "dis";
    var COL_CTRL_RISK_REDUCTION = "risk";
    var COL_CTRL_MUST = "must";
    var COL_CTRL_MUST_NOT = "mustnot";
    var COL_CTRL_ATTRIBUTES_START = 12;

    // invisible columns
    var COL_CTRL_GUID = "guid";
    var COL_CTRL_INFODOC = "infodoc";
    var COL_CTRL_CATEGORY_IDS = "cat_ids";    
    var COL_CTRL_SA_REDUCTION = "sa_red";
    var COL_CTRL_SA_REDUCTION_DOLL = "sa_red_doll";
    var COL_CTRL_ATTRIBUTES_START_DATA = "";

    var controls_columns = <% = GetControlsColumns() %>;
    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_CLASS = 3;
    var COL_COLUMN_TYPE = 4;
    var COL_COLUMN_SORTABLE = 5;
    var COL_COLUMN_DATA_FIELD = 6;
    var COL_COLUMN_ATTR_INDEX = 7;
    var COL_COLUMN_WIDTH = 8;
    var COL_COLUMN_ATTR_TYPE = 9;

    var ctrls_risk = []; // controls individual risk reduction

    <%If IsOptimizationAllowed Then%>
    var solver_id_xa = <% =CInt(raSolverLibrary.raXA)%>;         // 1
    var solver_id_gurobi = <% =CInt(raSolverLibrary.raGurobi)%>; // 2
    var solver_id_baron = <% =CInt(raSolverLibrary.raBaron)%>;   // 3
    var solver_names = ['<% =JS_SafeString(ResString("lblRASolverXA"))%>', '<% =JS_SafeString(ResString("lblRASolverGurobi"))%>', '<% =JS_SafeString(ResString("lblRASolverBaron"))%>'];

    var solver_id = <%=CInt(RA.RiskOptimizer.SolverLibrary)%>; // 1 or 2, 3 (when posible)
    <%End If%>

    var showDescriptions = <%=Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs)%>;
    var ShowControlsRiskReduction = <% = Bool2JS(PM.Parameters.Riskion_ShowControlsRiskReduction) %>;
    var ShowRiskReductionOptions = <% = Bool2JS(PM.Parameters.RiskionShowRiskReductionOptions) %>
    var Use_Simulated_Values = <% = CInt(PM.CalculationsManager.UseSimulatedValues) %>;
    var objectiveFunctionType = <% = CInt(PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType)%>;

    var ShowDollarValue = <%=Bool2JS(ShowDollarValue)%>; // Show Value of Enterprise
    var DollarValue = <% = DollarValue%>; // Value of Enterprise
    var DollarValueTarget = "<%=DollarValueTarget%>"; // Target Of Value of Enterprise
    var DollarValueOfEnterprise = <%=JS_SafeNumber(PM.DollarValueOfEnterprise)%>; // Value of Enterprise
    var UNDEFINED_INTEGER_VALUE = <%=UNDEFINED_INTEGER_VALUE%>;
    var UNDEFINED_ATTRIBUTE_VALUE = "<% = UNDEFINED_ATTRIBUTE_VALUE%>";

    // Control Attributes
    var attr_data = <% =GetAttributesData() %>;
    var IDX_ATTR_ID     = 0;
    var IDX_ATTR_NAME   = 1;    
    var IDX_ATTR_ITEMS     = 2;
    var IDX_ATTR_DEF_VALUE = 2;
    //var IDX_ATTR_ITEM_ORIG_INDEX = 2;
    var IDX_ATTR_TYPE      = 3;
    //var IDX_ATTR_ORIG_INDEX= 4;    
    var IDX_ATTR_ITEM_DEFAULT = 2;

    var avtString       = 0;
    var avtBoolean      = 1;
    var avtLong         = 2;
    var avtDouble       = 3;
    var avtEnumeration  = 4;
    var avtEnumerationMulti = 5;

    var SelectedAttrIndex = 0;
    var SelectedItemIndex = 0;

    var item_name = "";
    var on_hit_enter_tmp = "";

    var on_hit_enter = "";    
    var dlg_attributes = null;

    var on_hit_enter_cat = "";
    var dlg_attributes_cat = null;

    var dlg_multi_cat = null;
    var cancelled = false;

    /* simulations settings */
    var numSimulations = <%=PM.RiskSimulations.NumberOfTrials%>;
    var randSeed       = <%=PM.RiskSimulations.RandomSeed%>;
    var keepSeed       = <%=Bool2JS(PM.RiskSimulations.KeepRandomSeed)%>;

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

    function saveKeepRandSeed(value) {
        keepRandSeed = value;
        sendCommand("action=keep_rand_seed&val=" + value);
    }

    function manageAttributes() {
        initAttributes();
        initAttributesDlg();
    }

    function initAttributes() {
        var t = $("#tblAttributes tbody");
        
        if ((t)) {
            t.html("");        
            for (var i=0; i<attr_data.length; i++) {
                var v = attr_data[i];
                var n = htmlEscape(v[IDX_ATTR_NAME]);
                
                var vals = "&nbsp;";
                var attr_type = v[IDX_ATTR_TYPE]*1;

                switch (attr_type) {
                    case avtString:
                    case avtLong:
                    case avtDouble:
                        if ((v[IDX_ATTR_DEF_VALUE])) vals = htmlEscape(v[IDX_ATTR_DEF_VALUE]);
                        break;
                    case avtBoolean:
                        vals = (v[IDX_ATTR_DEF_VALUE] == "1" ? "[<%=ResString("lblYes")%>]" : "[<%=ResString("lblNo")%>]");
                        break;
                    case avtEnumeration:
                    case avtEnumerationMulti:
                        vals = (v[IDX_ATTR_ITEMS].length == 0 ? " ... " : "");
                        for (j=0; j<v[IDX_ATTR_ITEMS].length; j++) {
                            var isDefault = v[IDX_ATTR_ITEMS][j][IDX_ATTR_ITEM_DEFAULT] == "1";
                            vals += (vals == "" ? "" : ", ") + (isDefault ? "<b>" : "") + htmlEscape(v[IDX_ATTR_ITEMS][j][IDX_ATTR_NAME]) + (isDefault ? "</b>" : "");
                        };
                        break;
                    //case avtEnumerationMulti:
                    //   vals = 'not implemented';
                    //   break;
                }

                var sHidden = ""; //(attr_type == avtEnumeration || attr_type == avtEnumerationMulti ? "" : " style='display:none;' ");
                
                sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "' " + sHidden + ">";
                sRow += "<td " + sHidden + " align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td " + sHidden + " id='tdName" + i + "''>" + n + "</td>";                
                sRow += "<td " + sHidden + " id='tdEditAction" + i + "' align='right'><i class='fas fa-pencil-alt' style='cursor: pointer;' onclick='onEditAttribute(" + i + "); return false;'></i></td>";
                sRow += "<td " + sHidden + " id='tdType" + i + "' align='center'>" + getAttrTypeName(v[IDX_ATTR_TYPE]) + "</td>";
                sRow += "<td " + sHidden + " id='tdValues" + i + "'>" + vals + "</td>";
                sRow += "<td " + sHidden + " id='tdEditValues" + i + "' align='right'><i class='fas fa-pencil-alt' style='cursor: pointer;' onclick='onEditAttributeValues(" + i + ","+v[IDX_ATTR_TYPE]+"); return false;'></i></td>";
                sRow += "<td " + sHidden + " id='tdActions" + i + "' align='center'><i class='fas fa-trash-alt' style='cursor: pointer;' onclick='DeleteAttribute(" + i + "); return false;'></i></td>";
                sRow +="</tr>";
                t.append(sRow);
            }

            sRow = "<tr class='text grid_footer' id='trNew'>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width:100%' id='tbAttrName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'></td>";
            sRow += "<td><select class='select' style='width:120px;' id='tbType' onchange='cbNewAttrTypeChanged(this.value)'>" + getAttrTypeOptions() + "</select></td>";
            //sRow += "<td>&nbsp;</td>"; // edit name icon
            sRow += "<td colspan='2'><nobr>&nbsp;<span id='lblDefaultValue' style='display:none'><%=ResString("lblDefaultAttrValue")%>:&nbsp;</span><input type='text' id='tbDefaultTextValue' style='display:none; width:130px;' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'><select id='tbDefaultBoolValue' class='select' style='display:none;width:80px;'><option value='1'><%=ResString("lblYes")%></option><option value='0'><%=ResString("lblNo") %></option></select></nobr></td>"; // values
            //sRow += "<td>&nbsp;</td>"; // edit values icon
            sRow += "<td align='center'><a href='' style='cursor: pointer;' onclick='AddAttribute(); return false;'><i class='fas fa-plus' style='cursor: pointer;' title='<% = JS_SafeString(ResString("titleAddCC")) %>'></i></td></tr>";
            t.append(sRow);
            if ((dlg_attributes) && dlg_attributes.dialog("isOpen")) on_hit_enter = "AddAttribute();";
        }
    }

    function cbNewAttrTypeChanged(value) {
        $("#lblDefaultValue").hide();
        $("#tbDefaultTextValue").hide();
        $("#tbDefaultBoolValue").hide();
        
        if (value == avtString || value == avtLong || value == avtDouble) {
            $("#lblDefaultValue").show();
            $("#tbDefaultTextValue").show();
        }

        if (value == avtBoolean) {
            $("#lblDefaultValue").show();
            $("#tbDefaultBoolValue").show();
        }
    }

    function getAttrTypeOptions() {
        var retVal = "<option value='" + avtEnumeration + "' selected='selected'><%=ResString("optAttrTypeEnum")%></option>";
        retVal += "<option value='" + avtEnumerationMulti + "'><%=ResString("optAttrTypeMultiEnum")%></option>";
        retVal += "<option value='-1' disabled='disabled'><%=ResString("optSeparatorLine")%></option>";
        retVal += "<option value='" + avtString + "'><%=ResString("optAttrTypeString")%></option>";
        retVal += "<option value='" + avtLong + "'><%=ResString("optAttrTypeInteger")%></option>";
        retVal += "<option value='" + avtDouble + "'><%=ResString("optAttrTypeDouble")%></option>";
        retVal += "<option value='" + avtBoolean + "'><%=ResString("optAttrTypeBoolean")%></option>";
        return retVal;
    }

    function getAttrTypeName(attr_type) {
        switch (attr_type*1) {
            case avtString: return "<%=ResString("optAttrTypeString") %>"; break;
            case avtBoolean: return "<%=ResString("optAttrTypeBoolean") %>"; break;
            case avtLong: return "<%=ResString("optAttrTypeInteger") %>"; break;
            case avtDouble: return "<%=ResString("optAttrTypeDouble") %>"; break;
            case avtEnumeration: return "<%=ResString("optAttrTypeEnum") %>"; break;
            case avtEnumerationMulti: return "<%=ResString("optAttrTypeMultiEnum") %>"; break;
        }
        return "";
    }

    /* Drag-Drop === */
    var drag_index = -1;
    var attr_order = "";
    var attr_values_order = "";

    function InitDragDrop() {
        $(function () {
            $(".drag_drop_grid").sortable({
                items: 'tr:not(tr:last-child)',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid',
                axis: 'y',
                start: function( event, ui ) { 
                    drag_index = ui.item.index(); 
                    ui.helper.css('display', 'table');
                },
                update: function( event, ui ) { onDragIndex(ui.item.index()); },
                stop: function(event, ui) {
                    ui.item.css('display', '');
                }
            });
        });
    }

    function onDragIndex(new_idx) {
        if (new_idx>=0 && drag_index>=0 && new_idx!=drag_index) {
            
            if (dlg_attributes_cat == null) {
                // reorder attributes
                var el = attr_data[drag_index];
                attr_data.splice(drag_index, 1);
                attr_data.splice(new_idx, 0, el);
                attr_order = "";
                for (var i=0; i<attr_data.length; i++) {                    
                    attr_order += (attr_order=="" ? "" : ",") + attr_data[i][IDX_ATTR_ID];
                    //attr_data[i][IDX_ATTR_ID] = i;
                }            
                initAttributes();
            } else {
                // reorder attribute values
                var attr = GetSelectedAttr();
                if ((attr)) {
                    var el = attr[IDX_ATTR_ITEMS][drag_index];
                    attr[IDX_ATTR_ITEMS].splice(drag_index, 1);
                    attr[IDX_ATTR_ITEMS].splice(new_idx, 0, el);
                    attr_values_order = "";
                    for (var i=0; i<attr[IDX_ATTR_ITEMS].length; i++) {                    
                        attr_values_order += (attr_values_order=="" ? "" : ",") + attr[IDX_ATTR_ITEMS][i][IDX_ATTR_ID];
                        //list[i][IDX_ATTR_ID] = i;
                    }            
                    initAttributes();
                    initAttributesValues();
                }
            }            
            document.getElementById("tbAttrName").focus();
            drag_index = -1;
        }
    }
    /* Drag-Drop == */

    function initAttributesDlg() {        
        dlg_attributes = $("#divAttributes").dialog({
              autoOpen: true,
              width: 750,
              minWidth: 530,
              maxWidth: 950,
              minHeight: 250,
              maxHeight: mh,
              modal: true,
              dialogClass: "no-close",
              closeOnEscape: true,
              bgiframe: true,
              title: "<% = JS_SafeString(ResString("btnRAEditAttributes")) %>",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                Close: function() {
                    if (checkUnsavedData(document.getElementById("tbAttrName"), "dlg_attributes.dialog('close')")) dlg_attributes.dialog( "close" );
                }
              },
              open: function() {
                $("body").css("overflow", "hidden");
                on_hit_enter = "AddAttribute();";
                document.getElementById("tbAttrName").focus();
              },
              close: function() {
                $("body").css("overflow", "auto");
                initAttributes();
                $("#tblAttributes tbody").html("");
                on_hit_enter = "";
                $("#divAttributes").dialog("destroy");
                if (attr_order!="") {
                    sendCommand("action=attributes_reorder&lst=" + encodeURIComponent(attr_order));
                    for (var i=0; i<attr_data.length; i++) {
                        attr_data[i][IDX_ATTR_ID] = i; 
                    };
                    attr_order = "";
                } else {
                    sendCommand("action=refresh_full");
                }
                dlg_attributes = null;
              },
              resize: function( event, ui ) { $("#pAttributes").height(30); $("#pAttributes").height(Math.round(ui.size.height-150)); }
        });
        if ($("#pAttributes").height()>mh-150) $("#pAttributes").height(mh-150);
        setTimeout('$("#pAttributes").scrollTop(10000);', 30);
    }

    function GetSelectedAttr() {
        var attr = null;
        if ((attr_data) && (SelectedAttrIndex >= 0) && (SelectedAttrIndex < attr_data.length)) {
            attr = attr_data[SelectedAttrIndex];        
        }
        return attr;
    }

    function initAttributesValues() {
        var t = $("#tblAttributesValues tbody");
        if ((t)) {
            t.html("");        
            
            var attr = GetSelectedAttr();
            if ((attr)) {            
                for (i = 0; i < attr[IDX_ATTR_ITEMS].length; i++) {
                    var v = attr[IDX_ATTR_ITEMS][i]
                    var n = htmlEscape(v[IDX_ATTR_NAME]);
                    var isDefault = v[IDX_ATTR_ITEM_DEFAULT] == "1";
                    var isChecked = (isDefault ? "checked='checked'" : "");
                    sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "'>";
                    sRow += "<td align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                    sRow += "<td id='tdCatName" + i + "''>" + (isDefault ? "<b>" : "") + n + (isDefault ? "</b>" : "") + "</td>";
                    sRow += "<td align='center' id='tdCatIsDefault" + i + "''><input type='checkbox' class='checkbox' onclick='onEnumDefaultClick("+i+",this.checked);' onchange='onEnumDefaultClick("+i+",this.checked);' onkeydown='onEnumDefaultClick("+i+",this.checked);' "+isChecked+"></td>";
                    sRow += "<td id='tdCatActions" + i + "' align='center'><i class='fas fa-pencil-alt' onclick='onEditAttributeValue(" + i + "); return false;'></i><i class='fas fa-trash-alt' onclick='DeleteAttributeValue(" + i + "); return false;'></i></td>";
                    sRow +="</tr>";
                    t.append(sRow);
                }
            }

            sRow = "<tr class='text grid_footer' id='trCatNew'>";
            //sRow += "<td align='center' style='width:20px'>&nbsp;</td>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width:100%' id='tbCatName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'></td>";
            sRow += "<td align='center'><i class='fas fa-plus' onclick='AddAttributeValue(); return false;' title='<% = JS_SafeString(ResString("titleAddCategory")) %>'></i></td></tr>";
            t.append(sRow);
            if ((dlg_attributes_cat) && dlg_attributes_cat.dialog("isOpen")) on_hit_enter_cat = "AddAttributeValue();";
        }
    }

    function initAttributesValuesDlg() {
        dlg_attributes_cat = $("#divAttributesValues").dialog({
              autoOpen: true,
              width: 450,
              minWidth: 390,
              maxWidth: 850,
              minHeight: 200,
              maxHeight: mh,
              modal: true,
              dialogClass: "no-close",
              closeOnEscape: true,
              bgiframe: true,
              title: "<% = JS_SafeString(ResString("btnRAEditCategories")) %>",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                Close: function() {
                  if (checkUnsavedData(document.getElementById("tbCatName"), "dlg_attributes_cat.dialog('close')")) dlg_attributes_cat.dialog( "close" );
                }
              },
              open: function() {
                $("body").css("overflow", "hidden");
                on_hit_enter_cat = "AddAttributeValue();";
                document.getElementById("tbCatName").focus();
              },
              close: function() {
                $("body").css("overflow", "auto");
                initAttributesValues();
                $("#tblAttributesValues tbody").html("");
                on_hit_enter_cat = "";
                $("#divAttributesValues").dialog("destroy");
                if (attr_values_order!="") {
                    sendCommand("action=enum_items_reorder&lst=" + encodeURIComponent(attr_values_order) + "&clmn=" + SelectedAttrIndex);
                    //var attr = GetSelectedAttr();
                    //if ((attr)) {
                    //    var v = attr[IDX_ATTR_ITEMS];
                    //    for (var i=0; i<v.length; i++) {
                    //        v[i][IDX_ATTR_ID] = i; 
                    //    };
                    //}
                    attr_values_order = "";
                };
                dlg_attributes_cat = null;
              },
              resize: function( event, ui ) { $("#pAttributesValues").height(30); $("#pAttributesValues").height(Math.round(ui.size.height-150)); }
        });
        if ($("#pAttributesValues").height()>mh-150) $("#pAttributesValues").height(mh-150);
        setTimeout('$("#pAttributesValues").scrollTop(10000);', 30);
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", on_agree + ";", ";", "<% = JS_SafeString(ResString("titleConfirmation")) %>");
            return false;
        }
        return true;
    }

    function onEditAttribute(index, skip_check) {
        SelectedAttrIndex = index;
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbAttrName"), "onEditAttribute(" + index + ", true)")) return false;
        initAttributes();
        $("#tdName" + index).html("<input type='text' class='input' style='width:" + $("#tdName" + index).width()+ "' id='tbAttrName' value='" + replaceString("'", "&#39;", attr_data[index][IDX_ATTR_NAME]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdEditAction" + index).html("<i class='fas fa-check' style='cursor: pointer;' onclick='EditAttribute(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></i>&nbsp;<i class='fas fa-ban' onclick='initAttributes(); document.getElementById(\"tbAttrName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></i>");
        $("#trNew").html("").hide();
        setTimeout("document.getElementById('tbAttrName').focus();", 50);
        on_hit_enter = "EditAttribute(" + index + ");";
    }

    function onEditAttributeValue(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbCatName"), "onEditAttributeValue(" + index + ", true)")) return false;
        initAttributesValues();
        $("#tdCatName" + index).html("<input type='text' class='input' style='width:" + $("#tdCatName" + index).width()+ "' id='tbCatName' value='" + replaceString("'", "&#39;", attr_data[SelectedAttrIndex][IDX_ATTR_ITEMS][index][IDX_ATTR_NAME]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdCatActions" + index).html("<i class='fas fa-check' style='cursor: pointer;' onclick='EditAttributeValue(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></i>&nbsp;<i class='fas fa-ban' onclick='initAttributes(); document.getElementById(\"tbCatName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></i>");
        $("#trCatNew").html("").hide();
        setTimeout("document.getElementById('tbCatName').focus();", 50);
        on_hit_enter_cat = "EditAttributeValue(" + index + ");";
    }

    function onEditAttributeValues(index, attr_type) {
        SelectedAttrIndex = index;
        var attr = GetSelectedAttr();

        initAttributes();
        if (attr_type == avtEnumeration || attr_type == avtEnumerationMulti) {
            initAttributesValues();
            initAttributesValuesDlg();
        } else {            
            //if (!(skip_check) && !checkUnsavedData(document.getElementById("tbAttrName"), "onEditAttribute(" + index + ", true)")) return false;
            if (attr_type == avtBoolean) {
                $("#tdValues"   + index).html("<select id='cbDefValue' class='select' style='width:80px;'><option value='1' " + (attr[IDX_ATTR_DEF_VALUE] == "1" ? " selected='selected' " : " ") + "><%=ResString("lblYes") %></option><option value='0' " + (attr[IDX_ATTR_DEF_VALUE] == "0" ? " selected='selected' " : " ") + "><%=ResString("lblNo") %></option></select>");
            } else {
                $("#tdValues"   + index).html("<input type='text' class='input' style='width:" + $("#tdValues" + index).width()+ "' id='tbDefValue' value='" + htmlEscape(attr[IDX_ATTR_DEF_VALUE]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
            }
            $("#tdEditValues" + index).html("<i class='fas fa-check' style='cursor: pointer;' onclick='EditDefaultValue(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></i>&nbsp;<i class='fas fa-ban' onclick='initAttributes(); document.getElementById(\"tbAttrName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></i>");
            $("#trNew").html("").hide();
            if (attr_type == avtBoolean) { setTimeout("document.getElementById('cbDefValue').focus();", 50) } else { setTimeout("document.getElementById('tbDefValue').focus();", 50) };
            on_hit_enter = "EditDefaultValue(" + index + ");";
        }
    }

    function EditDefaultValue(index) {
        SelectedItemIndex = index;
        var attr = GetSelectedAttr();
        
        var n = document.getElementById("tbDefValue");
        if (attr[IDX_ATTR_TYPE] == avtBoolean) n = document.getElementById("cbDefValue");

        if ((n) && (attr)) {
            var def_val = n.value.trim();                        
            if ((index >= 0) && (index < attr_data.length)) {
                switch (attr[IDX_ATTR_TYPE]) {
                    case avtDouble:
                        if (validFloat(def_val)) {def_val = str2double(def_val); } else { def_val=""; }
                        break;
                    case avtLong:
                        if (validInteger(def_val)) { def_val = str2int(def_val); } else { def_val=""; }
                        break;
                }
                var idx = attr[IDX_ATTR_ID];
                attr[IDX_ATTR_DEF_VALUE] = htmlEscape(def_val);
                sendCommand("action=set_default_value&def_val=" + encodeURIComponent(def_val) + "&clmn=" + index);
            }
        }
    }

    function onEnumDefaultClick(item_index, checked) {
        var attr = GetSelectedAttr();
        if ((attr) && (attr[IDX_ATTR_TYPE] == avtEnumeration || attr[IDX_ATTR_TYPE] == avtEnumerationMulti)) {
            for (var i=0; i<attr[IDX_ATTR_ITEMS].length; i++) {
                attr[IDX_ATTR_ITEMS][i][IDX_ATTR_ITEM_DEFAULT] = ((i == item_index) && checked ? "1" : "0");
            }
            initAttributes();
            initAttributesValues();
            sendCommand("action=set_default_value&def_val=" + (checked ? "1" : "0") + "&clmn=" + SelectedAttrIndex + "&item_index=" + item_index);
        }
    }

    /* Add-remove-rename columns */
    function EditAttribute(index) {
        var n = document.getElementById("tbAttrName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgEmptyCCName") %>", "setTimeout(\"document.getElementById('tbAttrName').focus();\", 150);", undefined, "<%=ResString("lblError") %>");
            } else {
                if ((index >= 0) && (index < attr_data.length)) {
                    // idx = attr_data[index][IDX_ATTR_ID];
                    attr_data[index][IDX_ATTR_NAME] = htmlEscape(n.value);
                    sendCommand("action=rename_column&name=" + n.value + "&clmn=" + index);
                }
            }
        }
    }

    function AddAttribute() {
        on_hit_enter = "";
        var n = document.getElementById("tbAttrName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgCreateEmptyCCName") %>", "setTimeout(\"document.getElementById('tbAttrName').focus();\", 150);", undefined, "<%=ResString("lblError") %>");
            } else {
                var t = document.getElementById("tbType");
                var def_val = "";
                if ((t)) {
                    switch (t.value*1) {
                        case avtEnumeration:
                        case avtEnumerationMulti:
                            break;
                        case avtString:
                            var dv = document.getElementById("tbDefaultTextValue");
                            if ((dv)) def_val = dv.value.trim();
                            break;
                        case avtLong:
                            var dv = document.getElementById("tbDefaultTextValue");                            
                            if ((dv)) { def_val = dv.value.trim(); if (validInteger(def_val)) { def_val = str2int(def_val); } else { def_val=""; }}
                            break;
                        case avtDouble:
                            var dv = document.getElementById("tbDefaultTextValue");                            
                            if ((dv)) { def_val = dv.value.trim(); if (validFloat(def_val)) {def_val = str2double(def_val); } else { def_val=""; }}
                            break;
                        case avtBoolean:
                            var dv = document.getElementById("tbDefaultBoolValue");                            
                            if ((dv)) def_val = dv.value.trim();                            
                            break;
                    }
                    sendCommand('action=add_column&name='+encodeURIComponent(n.value)+'&type='+t.value+'&def_val='+encodeURIComponent(def_val));
                }
            }
        }
    }

    function DeleteAttribute(idx) {
        SelectedAttrIndex = idx;
        dxConfirm("<%=ResString("msgSureDeleteCC") %>", "sendCommand(\"action=del_column&clmn=" + idx + "\");", ";");
    }

    /* Add-remove-rename items */
    function EditAttributeValue(index) {
        SelectedItemIndex = index;
        var n = document.getElementById("tbCatName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgEmptyCategoryName") %>", "setTimeout(\"document.getElementById('tbCatName').focus();\", 150);", undefined, "<%=ResString("lblError") %>");
            } else {
                var attr = GetSelectedAttr();
                if ((index >= 0) && (index < attr[IDX_ATTR_ITEMS].length)) {
                    var idx = attr[IDX_ATTR_ITEMS][index][IDX_ATTR_ID];
                    attr[IDX_ATTR_ITEMS][index][IDX_ATTR_NAME] = htmlEscape(n.value);
                    sendCommand("action=rename_item&name=" + n.value + "&item=" + index + '&clmn=' + SelectedAttrIndex);
                }
            }
        }
    }

    function AddAttributeValue() {
        on_hit_enter = "";
        var n = document.getElementById("tbCatName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgCreateEmptyCCName") %>", "setTimeout(\"document.getElementById('tbCatName').focus();\", 150);", undefined, "<%=ResString("lblError") %>");
            } else {
                sendCommand('action=add_item&name=' + encodeURIComponent(n.value)+'&clmn=' + SelectedAttrIndex);
            }
        }
    }

    function DeleteAttributeValue(index) {
        SelectedItemIndex = index;
        dxConfirm("<%=ResString("msgSureDeleteCategory") %>", "sendCommand(\"action=del_item&item=" + index + "&clmn=" + SelectedAttrIndex + "\");", ";");
    }
    // end Control Attributes

    function onSetScenario(id) {
        showLoadingPanel();
        document.location.href = '<% =PageURL(CurrentPageID) %>&action=scenario<% =GetTempThemeURI(true) %>&sid=' + id;
        return false;
    }

    function activateControl(e, id, chk) {
        var oper = "activate";
        var c = null;
        var ids = id;

        // process Shift + Click
        if ((e.shiftKey) && e.shiftKey && (last_click_cb != "") && (last_click_cb != id) && last_click_cb_oper == oper) {
            var start_id = last_click_cb;
            var stop_id = id;
            var is_checking = false;
            $("input.control_is_active_cb").each(function() {
                var this_id = this.getAttribute("data-ctrl_id") + "";
                var is_found = this_id == start_id || this_id == stop_id;
                if (is_found && !is_checking) {
                    is_checking = true;
                } else {
                    if (is_found && is_checking) {
                        this.checked = chk;
                        c = getControlByID(this_id);
                        //if ((c) && c[COL_CTRL_APPL_COUNT] > 0) {
                        if ((c)) {
                            c[COL_CTRL_IS_ACTIVE] = chk;
                            c[COL_CTRL_FUNDED] = chk;
                        }
                        if (ids.indexOf(this_id) == -1) ids += (ids == "" ? "" : ",") + this_id;
                        
                        is_checking = false;
                    }
                }
                if (is_checking) {
                    this.checked = chk;
                    c = getControlByID(this_id);
                    //if ((c) && c[COL_CTRL_APPL_COUNT] > 0) {
                    if ((c)) {
                        c[COL_CTRL_IS_ACTIVE] = chk;
                        c[COL_CTRL_FUNDED] = chk;
                    }
                    if (ids.indexOf(this_id) == -1) ids += (ids == "" ? "" : ",") + this_id;
                }
            });            
        } else {
            c = getControlByID(id);
            //if ((c) && c[COL_CTRL_APPL_COUNT] > 0) {
            if ((c)) {
                c[COL_CTRL_IS_ACTIVE] = chk;
                c[COL_CTRL_FUNDED] = chk;
            }
        }
        
        last_click_cb = id;
        last_click_cb_oper = oper;
        //alert("shift:" + e.shiftKey + ",   last:"+ last_click_cb);
        sendCommand('action=activate&ids=' + ids + '&chk=' + chk, true);
    }
    
    function activateAll(e, type, chk) {
        if(e.preventDefault) e.preventDefault(); else e.returnValue = false;
        
        var controls_len = controls_data.length;
        for (var i = 0; i < controls_len; i++) {
            //if (controls_data[i][COL_CTRL_APPL_COUNT] > 0) {
                controls_data[i][COL_CTRL_IS_ACTIVE] = chk;
                controls_data[i][COL_CTRL_FUNDED] = chk;

            //}
        }
        
        sendCommand('action=activate_all&type=' + type + '&chk='+chk);
    }

    function updateCheckboxes() {
        $('input:checkbox.indeterminate_cb').prop('indeterminate', true);
    }
    
    /* jQuery Ajax */
    function syncReceived(params) {
        var msg = "";
        var continue_ajax = false;

        var reopen_editor = false;
        var reopen_cat_editor = false;

        if ((params)) {            
            var received_data = eval(params);
            if ((received_data)) {
                if (received_data[0] == "refresh" || received_data[0] == "refresh_full") { // refresh controls list for updating the infodocs
                    controls_data = received_data[1];
                    attr_data = received_data[2];
                    controls_columns = received_data[3];

                    //synchControlsRiskReduction();
                    if (received_data[0] == "refresh") {
                        refreshDataGrid(controls_data);
                    }
                    if (received_data[0] == "refresh_full") {                        
                        initDatatable();
                    }
                }

                if (received_data[0] == "show_options") {

                }

                if (received_data[0] == "selected_node") {
                    // update node name in page title
                    var name = received_data[1];
                    var d = document.getElementById("divNodeName");        
                    if ((d)) d.innerHTML = htmlStrip(ShortString(name, 85));
                    if (received_data[2] !== "") dxDialog(received_data[2], function(){}, null, "Error");
                }

                if (received_data[0] == "add_column" || received_data[0] == "del_column" || received_data[0] == "rename_column" || received_data[0] == "enum_items_reorder") {
                    attr_data = received_data[1];
                    reopen_editor = true;
                    if (received_data[0] == "add_column" && (received_data[2] == avtEnumeration || received_data[2] == avtEnumerationMulti)) {
                        SelectedAttrIndex = attr_data.length - 1;
                        reopen_cat_editor = true;
                    }
                }
                
                if (received_data[0] == "add_item" || received_data[0] == "del_item" || received_data[0] == "rename_item") {
                    attr_data = received_data[1];
                    reopen_editor = true;
                    reopen_cat_editor = true;
                }

                if (received_data[0] == "set_attr_value") {
                    //controls_data = received_data[1];
                    //if (received_data[2] == 1 || received_data[3] == avtEnumerationMulti) { // more than 1 controls were selected (multiselect)
                    //    // need to refresh grid
                    //    refreshDataGrid(controls_data);
                    //}
                    var success = received_data[1];
                    DevExpress.ui.notify(success ? "Changes Saved" : "Error", success ? "success" : "error");
                }

                if (received_data[0] == "set_default_value") {
                    attr_data = received_data[1];
                    controls_data = received_data[2];
                    refreshDataGrid(controls_data);
                }

                if (received_data[0] == "pagination") {
                    var table = (($('#tableContent').data("dxDataGrid"))) ? $('#tableContent').dxDataGrid("instance") : null;
                    if (table !== null) table.state({});
                    initDatatable();
                }

                if (received_data[0] == "solve") {
                    var data = received_data[1];
                    
                    // update the controls selected (active) state based on Solver results
                    var controls_len = controls_data.length;
                    var data_len = data.length;
                    for (var i = 0; i < controls_len; i++) {
                        controls_data[i][COL_CTRL_IS_ACTIVE] = false;
                        controls_data[i][COL_CTRL_FUNDED] = false;
                        for (var j = 0; j < data_len; j++) {
                            if (controls_data[i][COL_CTRL_GUID] == data[j]) {
                                controls_data[i][COL_CTRL_IS_ACTIVE] = true;
                                controls_data[i][COL_CTRL_FUNDED] = true;
                                j = data_len;
                            }
                        }
                    }
                    
                    //$("#tbBudgetLimit").val(received_data[2]); // budget limit
                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk
                    budgetTotals(received_data[5]); // budget limit, max risk, min risk reduction
                    
                    //$("td.td_risk_value").show();

                    refreshDataGrid(controls_data);
                    //initDatatable();

                    if (received_data.length > 6 && received_data[6] != "") msg = received_data[6];
                    if (received_data.length > 7 && received_data[7] != "") msg += (msg == "" ? "" : "<br>") + received_data[7]; // error message about no controls applicable

                    if (!view_grid && received_data.length > 8) {
                        updateTimeperiods(received_data[8]);
                    }
                    
                    resizePage();
                }
              
                if (received_data[0] == "edit_cost") {
                    controls_data = received_data[5];
                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk                    
                    refreshDataGrid(controls_data);
                }

                if (received_data[0] == "select_controls" || received_data[0] == "select_events" || received_data[0] == "select_sources" || received_data[0] == "select_impacts") {
                    switch (received_data[0] + "") {
                        case "select_controls": 
                            controls_data = received_data[1];
                            break;
                        case "select_events":
                            event_list = received_data[1];
                            break;
                        case "select_sources":
                            sources_list = received_data[1];
                            break;
                        case "select_impacts":
                            impacts_list = received_data[1];
                            break;
                    }
                    
                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk
                    refreshDataGrid(controls_data);
                }

                if (received_data[0] == "selected_dollar_value_target" || received_data[0] == "set_dollar_value") {
                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk
                    budgetTotals(received_data[5]); // budget limit, max risk, min risk reduction
                    updateDollarValueUI(received_data[6]); //dollar value
                    all_obj_data = received_data[7];
                    event_list = received_data[8];
                }

                if (received_data[0] == "use_simulated_values") {
                    if (received_data.length > 1) {
                        costTotals(received_data[2]); // total cost, funded cost, selected controls count
                        riskTotals(received_data[1]); // optimized risk, risk with all controls, total risk
                        //callSolve();
                        //continue_ajax = true;
                        controls_data = received_data[3];
                        refreshDataGrid(controls_data);
                    }
                    <%If Not CanUserSelectControlsManually Then%>
                    callSolve();
                    continue_ajax = true;
                    <%End If%>
                }

                if (received_data[0] == "showapplications") {
                    ShowApplicationsDialog(received_data[1]);
                }

                if (received_data[0] == "activate") {
                    $('#tbBudgetLimit').val("");
                    markFundedControls(controls_data);
                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk

                    //if (Use_Simulated_Values > 0) $("td.td_risk_value").hide();
                    refreshDataGrid(controls_data);
                }

                if (received_data[0] == "num_sim" || received_data[0] == "rand_seed"  || received_data[0] == "keep_rand_seed" ) {
                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk
                }

                if (received_data[0] == "show_dollar_value") {
                    budgetTotals(received_data[1]); // budget limit, max risk, min risk reduction
                    riskTotals(received_data[2]); // optimized risk, risk with all controls, total risk
                    initDatatable();

                    //$("#lblSAChar").html(ShowDollarValue ? CHAR_CURRENCY_SYMBOL : "&#37;");
                }

                if (received_data[0] == "activate_all") {
                    $('#tbBudgetLimit').val("");                    
                    //if (Use_Simulated_Values > 0) $("td.td_risk_value").hide();
                }
                                
                if (received_data[0] == "activate_all" || received_data[0] == "editcontrol" || received_data[0] == "deletecontrol" || received_data[0] == "remove_controls") {
                    $('#tbBudgetLimit').val("");

                    controls_data = received_data[1];
                    //synchControlsRiskReduction();
                    
                    if (received_data[0] == "deletecontrol" || received_data[0] == "remove_controls") {    
                        initDatatable();
                    } else {
                        refreshDataGrid(controls_data);
                    }

                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    riskTotals(received_data[4]); // optimized risk, risk with all controls, total risk

                    //if (Use_Simulated_Values > 0) $("td.td_risk_value").hide();
                }

                if (received_data[0] == "paste_controls" ) {
                    attr_data = received_data[1];
                    controls_data = received_data[2];
                    controls_columns = received_data[3];
                    cat_data = received_data[4];
                    costTotals(received_data[5]);  

                    initDatatable();                  
                }

                if (received_data[0] == "addcontrol" || received_data[0] == "paste_costs" || received_data[0] == "paste_attr") {
                    controls_data = received_data[1];
                    cat_data = received_data[2];  // refresh categories list, they may have been added from clipboard

                    if (received_data[0] == "paste_attr") {
                        attr_data = received_data[4];
                    }

                    //synchControlsRiskReduction();                                        
                    //refreshDataGrid(controls_data);
                    initDatatable();

                    costTotals(received_data[3]); // total cost, funded cost, selected controls count
                    
                    //if (Use_Simulated_Values > 0) $("td.td_risk_value").hide();
                }

                if (received_data[0] == "editcontrol" || received_data[0] == "paste_costs") {

                }

                if (received_data[0] == "add_category" || received_data[0] == "delete_category" || received_data[0] == "rename_category") {
                    cat_data = received_data[1];
                    if (received_data[0] == "add_category") {
                        createCategoryElement(document.getElementById('lbCategories'), cat_data.length - 1, cat_data[cat_data.length - 1][IDX_CAT_ID], cat_data[cat_data.length - 1][IDX_CAT_NAME], "");
                    }
                    if (received_data[0] == "delete_category") {
                        controls_data = received_data[2];
                        //synchControlsRiskReduction();
                        refreshDataGrid(controls_data);
                    }
                }

                if (received_data[0] == "set_must" || received_data[0] == "set_must_not") {
                    var has_musts = received_data[2];
                    var has_must_nots = received_data[3];

                    var cbOptMusts = document.getElementById("lblcbOptMusts");
                    var cbOptMustNots = document.getElementById("lblcbOptMustNots");
                    
                    if ((cbOptMusts)) {
                        cbOptMusts.style.fontWeight = (has_musts ? "bold" : "normal");
                        cbOptMusts.style.color = (has_musts ? "#000" : "#999");
                    }
                    if ((cbOptMustNots)) {
                        cbOptMustNots.style.fontWeight = (has_must_nots ? "bold" : "normal");
                        cbOptMustNots.style.color = (has_must_nots ? "#000" : "#999");
                    }

                    //if (Use_Simulated_Values > 0) $("td.td_risk_value").hide();
                }

                if (received_data[0] == "show_individual_risk") {
                    // update individual risk values
                    //if (ShowControlsRiskReduction) {
                    //ctrls_risk = received_data[1];
                    //synchControlsRiskReduction();
                    //}
                    //dataTablesColumnVisibility('tableContent', COL_CTRL_RISK_REDUCTION, ShowControlsRiskReduction);
                    //refreshDataGrid(controls_data);
                    //controls_data = received_data[1];
                    //initDatatable();
                    //setTimeout(function () {
                    //    $('#tableContent').dxDataGrid("instance").columnOption("risk", "visible", ShowControlsRiskReduction);
                    //}, 300);
                }

                if (received_data[0] == "show_risk_reduction_options") {
                    ShowRiskReductionOptions = received_data[1];
                    $("#divOptimizerTabs").find(".dx-tabpanel-tabs").toggle(ShowRiskReductionOptions);
                    $("#divOptimizerTabs").find(".dx-multiview-wrapper").css({"border": 0 /*ShowRiskReductionOptions ? "1px solid #6699cc;" : 0*/});

                    var tabs = $("#divOptimizerTabs").dxTabPanel("instance");
                    if ((tabs)) tabs.option("selectedIndex", 0);
                }

                if (received_data[0] == "control_type") {

                }

                if (received_data[0] == "show_descriptions") {
                    // refresh datatable to show or hide the descriptions
                    initDatatable(controls_data);
                }

                if (received_data[0] == "scenario_reorder" || received_data[0] == "edit_scenario" || received_data[0] == "copy_scenario" || received_data[0] == "delete_scenario") {
                    scenarios = received_data[1];
                    initScenarios();
                }

                if (received_data[0] == "show_timeperiods") {
                    _ajaxPageReload();
                }

                if (received_data[0] == "objective_function_type") {
                    sendCommand("action=refresh");
                }

            }
        }
        if (msg == "") {
            $("#divMessage").hide(); 
        } else {
            $("#divMessage").show().html(msg);
        }
        
        updateToolbar();
        multiselectUI();

        if (reopen_editor) {
            manageAttributes();
        }
        
        if (reopen_cat_editor) {
            if ((dlg_attributes_cat)) dlg_attributes_cat.dialog("close");
            initAttributesValues();
            initAttributesValuesDlg();
        }

        if (!continue_ajax) {
            hideLoadingPanel();
            var btnOptimize = document.getElementById("btnOptimize");
            if ((btnOptimize)) {
                btnOptimize.disabled = false;
            }
        }
    }

    function refreshDataGrid(ds) {
        if ($("#tableContent").data("dxDataGrid")) {
            var dg = $("#tableContent").dxDataGrid("instance");
            store = new DevExpress.data.ArrayStore({
                key: 'guid',
                data: ds
            });
            dg.option("dataSource", store);
            dg.refresh(true);
        }
        multiselectUI();
    }

    function budgetTotals(vals) {
        if ((vals) && vals.length > 2) {
            $("#tbBudgetLimit").val(vals[0]*1 == 0 ? "" : vals[0]); // budget limit
            $("#tbMaxRisk").val(vals[1]);
            $("#tbMinReduction").val(vals[2]);
        }
        if (ShowDollarValue) {
            $("#lblMaxRiskDollSign").show();
            $("#lblMinRiskDollSign").show();
            $("#lblMaxRiskPrcSign").hide();
            $("#lblMinRiskPrcSign").hide();
        } else {
            $("#lblMaxRiskDollSign").hide();
            $("#lblMinRiskDollSign").hide();
            $("#lblMaxRiskPrcSign").show();
            $("#lblMinRiskPrcSign").show();
        }
    }

    function costTotals(vals) {
        if ((vals) && vals.length > 2) {
            $("#tbFundedCost").html(vals[0]);
            $("#tbAllControlsCost").html(vals[1] + (vals.length > 3 ? " (with applications: " + vals[3] + ")" : ""));
            $("#lblActiveCount").html(vals[2]);
        }
    }

    function riskTotals(vals) {
        if ((vals) && vals.length > 4) {
            $("#tbOptimizedRisk").html(vals[0]);
            $("#tbRiskTotalWithControls").html(vals[1]);
            $("#tbRiskTotal").html(vals[2]);
            $("#tbTotalRiskReduction").html(vals[3]);            

            initSimulatedIndicators(vals[4]);
        }
    }

    function initSimulatedIndicators(isSimulated) {
        if (isSimulated) {
            // simulated
            $(".simulated_results_indicator").show();
            $(".simulated_results_label").css({"color" : "blue"});
        } else {
            // computed
            $(".simulated_results_indicator").hide();
            $(".simulated_results_label").css({"color" : "black"});
        }
    }

    function syncError() {
        hideLoadingPanel();
        dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error");
        $(".ui-dialog").css("z-index", 9999);
    }

    function sendCommand(params, please_wait) {
        if (typeof please_wait == "undefined" || please_wait) showLoadingPanel();

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }
    // end jQuery Ajax

    function InitApplicationsDlg(data) {
        dlg_applications = $("#divApplications").dialog({
              autoOpen: false,
              width: 650,
              minWidth: 530,
              maxWidth: 950,
              minHeight: 250,
              maxHeight: mh,
              modal: true,
              dialogClass: "no-close",
              closeOnEscape: true,
              bgiframe: true,
              title: "<% =ParseString("%%Controls%%")%> Applications",
              position: { my: "center", at: "center", of: $("body"), within: $("body") },
              buttons: {
                Close: function() {
                  dlg_applications.dialog( "close" );
                }
              },
              open: function() {
                $("body").css("overflow", "hidden");
                InitApplications(data);
              },
              close: function() {
                $("body").css("overflow", "auto");
                $("#tblApplications").empty();
              }
        });
    }

    function InitApplications(sData) {
        var data = eval(sData);
        var lbl = document.getElementById("lblHeader");
        if ((lbl)) {
            lbl.innerHTML = '<h6>"' + htmlEscape(data[IDX_CONTROL_NAME]) + '" is applied to:</h6>';
            
            var grid = document.getElementById("tblApplications");
            $("#tblApplications").empty();
            if ((grid)) {
                //init columns headers
                var header = grid.createTHead();
                var hRow = header.insertRow(0);            
                hRow.className = "text grid_header";
                var th0 = "<% =ParseString("%%Objective(l)%%")%> Name";
                var th1 = "<% =ParseString("%%Objective(i)%%")%> Name";
                var th2 = "<% =ParseString("%%Event%%")%> Name";
                var th3 = "Measure Type";
                var th4 = "Effectiveness";

                switch (data[IDX_CONTROL_TYPE]) {
                    case ctCause:                                          
                        var dc = hRow.insertCell(0); 
                        dc.innerHTML = "<b>" + th0 + "</b>";
                        dc = hRow.insertCell(1); 
                        dc.innerHTML = "<b>" + th3 + "</b>";
                        dc = hRow.insertCell(2); 
                        dc.innerHTML = "<b>" + th4 + "</b>";
                        for (var i=0; i<data[IDX_CONTROL_APPLICATIONS].length; i++) {
                            var d = data[IDX_CONTROL_APPLICATIONS][i];
                            var tr = grid.insertRow(i + 1);
                            tr.className = "text";
                            var c1 = document.createElement("td");
                            c1.innerHTML = htmlEscape(d[1]);
                            tr.appendChild(c1);
                            var c2 = document.createElement("td");
                            c2.innerHTML = d[3];
                            c2.style.textAlign = "center";
                            tr.appendChild(c2); 
                            var c3 = document.createElement("td");
                            c3.innerHTML = "<a href='' class='actions' onclick='" + d[5] + "'>" + d[4] + "</a>";
                            c3.style.textAlign = "right";
                            tr.appendChild(c3); 
                        }
                        break;
                    case ctCauseToEvent:
                        var dc = hRow.insertCell(0); 
                        dc.innerHTML = "<b>" + th0 + "</b>";
                        dc = hRow.insertCell(1); 
                        dc.innerHTML = "<b>" + th2 + "</b>";
                        dc = hRow.insertCell(2); 
                        dc.innerHTML = "<b>" + th3 + "</b>";
                        dc = hRow.insertCell(3); 
                        dc.innerHTML = "<b>" + th4 + "</b>";
                        for (var i=0; i<data[IDX_CONTROL_APPLICATIONS].length; i++) {
                            var d = data[IDX_CONTROL_APPLICATIONS][i];
                            var tr = grid.insertRow(i + 1);
                            tr.className = "text";
                            var c1 = document.createElement("td");
                            c1.innerHTML = htmlEscape(d[1]);
                            tr.appendChild(c1);
                            var c2 = document.createElement("td");
                            c2.innerHTML = htmlEscape(d[0]);
                            tr.appendChild(c2); 
                            var c3 = document.createElement("td");
                            c3.innerHTML = d[3];
                            c3.style.textAlign = "center";
                            tr.appendChild(c3); 
                            var c4 = document.createElement("td");
                            c4.innerHTML = "<a href='' class='actions' onclick='" + d[5] + "'>" + d[4] + "</a>";
                            c4.style.textAlign = "right";
                            tr.appendChild(c4); 
                        }
                        break;
                    case ctConsequenceToEvent:
                        var dc = hRow.insertCell(0); 
                        dc.innerHTML = "<b>" + th1 + "</b>";
                        dc = hRow.insertCell(1); 
                        dc.innerHTML = "<b>" + th2 + "</b>";
                        dc = hRow.insertCell(2); 
                        dc.innerHTML = "<b>" + th3 + "</b>";
                        dc = hRow.insertCell(3); 
                        dc.innerHTML = "<b>" + th4 + "</b>";
                        for (var i=0; i<data[IDX_CONTROL_APPLICATIONS].length; i++) {
                            var d = data[IDX_CONTROL_APPLICATIONS][i];
                            var tr = grid.insertRow(i + 1);
                            tr.className = "text";
                            var c1 = document.createElement("td");
                            c1.innerHTML = htmlEscape(d[2]);
                            tr.appendChild(c1);
                            var c2 = document.createElement("td");
                            c2.innerHTML = htmlEscape(d[0]);
                            tr.appendChild(c2); 
                            var c3 = document.createElement("td");
                            c3.innerHTML = d[3];
                            c3.style.textAlign = "center";
                            tr.appendChild(c3); 
                            var c4 = document.createElement("td");
                            c4.innerHTML = "<a href='' class='actions' onclick='" + d[5] + "'>" + d[4] + "</a>";
                            c4.style.textAlign = "right";
                            tr.appendChild(c4); 
                        }
                        break;
                    default:
                        break;                
                }
            }
        }
    }

    function ShowApplicationsDialog(data) {
        InitApplicationsDlg(data);
        dlg_applications.dialog("open");
    }

    function getControlByID(control_id) {
        var controls_len = controls_data.length;
        for (var i = 0; i < controls_len; i++) {
            if (controls_data[i][COL_CTRL_GUID] == control_id) return controls_data[i];
        }
        return null;
    }

    var is_context_menu_open = false;

    function showMenu(event, control_id) {
        event = event || window.event;
        is_context_menu_open = false;
        var ctrl = getControlByID(control_id);        
        if ((ctrl)) {   
            var control_name = ShortString(ctrl[COL_CTRL_NAME], 50, true);
            $("div.context-menu").hide().remove();
            var sMenu = "<div class='context-menu'>";
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameClick(\"" + control_id + "\"); return false;' style='text-align:left;'><div><nobr><i class='fas fa-pencil-alt'></i>&nbsp;Edit&nbsp;&quot;" + control_name + "&quot;</nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuEditInfodoc(\"" + control_id + "\"); return false;' style='text-align:left;'><div><nobr><i class='fas fa-info-circle'></i>&nbsp;<% = ResString("lblCMenuEditCtrlInfodoc") %></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='confrimDeleteControl(\"" + control_id + "\"); return false;' style='text-align:left;'><div><nobr><i class='fas fa-trash-alt'></i>&nbsp;Delete&nbsp;&quot;" + control_name + "&quot;</nobr></div></a>";
            sMenu += "</div>";                
            var x = event.clientX;
            var y = event.clientY;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: (x - $("div.context-menu").width()) + "px"});                        
            $("body").css("overflow", "hidden");
            $("div.context-menu").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }        
        return false;
    }

    function canCloseMenu() {
        is_context_menu_open = true;
    }

    function onContextMenuEditInfodoc(id) {
        <% = PopupRichEditor(reObjectType.Control , "guid=' + id + '") %>;
    }

    function onRichEditorRefresh(empty, infodoc, callback)
    { 
        sendCommand("action=refresh");
        window.focus();
    }

    function showInfodoc(id)
    {
        var url = "<% =PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid="" + id + """, CInt(reObjectType.Control)))%>&r="+Math.round(Math.random()*1000000);
        var w = CreatePopup(url, 'infodoc', 'menubar=no,maximize=no,titlebar=yes,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=800,height=560');
        if (w) w.focus();
        return false;
    }

    function parseDescription(sDesc, id) {
        if (sDesc.indexOf("%%viewmore%%")>=0) {
            sDesc = replaceString("%%viewmore%%", "&nbsp;<i class='fas fa-search ec-icon' style='cursor: pointer;' onclick='showInfodoc(\"" + id + "\"); return false;' title='Edit Infodoc'></i>", sDesc);
        }
        return sDesc;
    }

    function onContextMenuRenameClick(control_id) {
        resetVars();
        hideCMenu();
        var controls_data_len = controls_data.length;
        ctrl_id = control_id;
        for (var i=0; i<controls_data_len; i++) {
            if ((controls_data[i]) && (controls_data[i][COL_CTRL_GUID] == control_id)) {
                var cost = (controls_data[i][COL_CTRL_COST] == "", "0.00", controls_data[i][COL_CTRL_COST]);                
                initEditorForm("<% = ParseString("%%Control%% properties") %>", controls_data[i][COL_CTRL_NAME], parseDescription(controls_data[i][COL_CTRL_INFODOC], controls_data[i][COL_CTRL_GUID]), cost, "editcontrol", controls_data[i][COL_CTRL_CATEGORY_IDS]);
                dlg_control_editor.dialog("open");
                dxDialogBtnDisable(true, true);
                if ((theForm.tbControlName)) theForm.tbControlName.focus();
                i = controls_data_len;
            }
        }        
    }

    function onContextMenuRemoveClick(control_id) {
        resetVars();
        hideCMenu();
        var controls_data_len = controls_data.length;
        for (var i=0; i<controls_data_len; i++) {
            if ((controls_data[i]) && (controls_data[i][COL_CTRL_GUID] == control_id)) {
                controls_data.splice(i, 1);
                sendCommand("action=deletecontrol&control_id=" + control_id);
                i = controls_data_len;
            }
        }        
    }

    function confrimDeleteControl(control_id) {
        hideCMenu();
        dxConfirm("Are you sure you want to delete this <%=ParseString("%%control%%") %>?", "onContextMenuRemoveClick('" + control_id + "');", ";");
    }

    function navigateToApplyControls(controlType, controlId) {
        var url = "";
        switch (controlType) {
            case ctCause:
                url = "<% =JS_SafeString(PageURL(_PGID_RISK_TREATMENTS_MAP_CAUSES)) %>" //  70055;
                break;
            case ctCauseToEvent:
                url = "<% =JS_SafeString(PageURL(_PGID_RISK_TREATMENTS_MAP_VULNERABILITIES_BY_TREATMENT)) %>" //  70056;
                break;
            case ctConsequenceToEvent:
                url = "<% =JS_SafeString(PageURL(_PGID_RISK_TREATMENTS_MAP_CONSEQUENCES_BY_TREATMENT)) %>" //  70057;
                break;
        }
        loadURL(urlWithParams("&ctrl_id=" + controlId, url));
    }
    
    // Column Header context menu
    function showColumnMenu(e, el, hasCopyButton, hasPastButton, attr_idx, col_idx) {                       
        if(e.preventDefault) e.preventDefault(); else e.returnValue = false;
        e.stopPropagation();
        e.preventDefault();

        is_context_menu_open = false;                
        $("#contextmenuheader").hide().remove();
        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
        if (hasCopyButton) sMenu += "<a href='' class='context-menu-item' onclick='" + ((attr_idx >= 0) ? "doCopyToClipboardValues(\""+attr_idx+"\");" : "doCopyToClipboardCosts();") + " return false;'><div><nobr><i class='fas fa-copy'></i>&nbsp;&nbsp;&nbsp;<%=ResString("titleCopyToClipboard")%>&nbsp;</nobr></div></a>";
        if (hasPastButton) sMenu += "<a href='' class='context-menu-item' onclick='" + ((attr_idx >= 0) ? "doPasteAttr(\""+attr_idx+"\");" : "doPasteCosts();") + " return false;'><div><nobr><i class='fas fa-paste'></i>&nbsp;&nbsp;&nbsp;<%=ResString("titlePasteFromClipboard")%>&nbsp;</nobr></div></a>";
           sMenu += "</div>";                
        if ((el)) {
            var rect = el.getBoundingClientRect();
            var x = rect.left+ 2;
            var y = rect.top + 12;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});                        
            if ((s)) { var w = s.width();var pw = $("#divMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

            $("#contextmenuheader").fadeIn(500);
            $("body").css("overflow", "hidden");
            setTimeout('canCloseMenu()', 200);
        }

        var table = (($('#tableContent').data("dxDataGrid"))) ? $('#tableContent').dxDataGrid("instance") : null;
        if (table !== null) {
            var clmn = table.option("columns")[col_idx];
            clmn.allowSorting = false;
            setTimeout(function () {
                clmn.allowSorting = true;
            }, 100);
        }

        return false;
    }

    function hideCMenu() {
        $("div.context-menu").hide(200); 
        is_context_menu_open = false;
        setTimeout('$("body").css("overflow", "auto");', 250);
    }

    function doPasteCosts() {
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteCosts(data);
        } else {
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteCosts);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteCostsChrome();", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }
        }
    }

    function pasteCosts(data) {
        if (data != undefined && data != "") {
            var visControlIDs = "";
            $("input.tb_cost").each(function() { visControlIDs += (visControlIDs == "" ? "" : ",") + this.getAttribute("data-ctrl_id") + ""; });
            //var controls_data_len = controls_data.length;        
            //for (var i = 0; i < controls_data_len; i++) {
            //    visControlIDs += (visControlIDs == "" ? "" : ",") + controls_data[i][COL_CTRL_GUID];
            //}
            sendCommand("action=paste_costs&data=" + encodeURIComponent(data) + "&ids=" + visControlIDs);
        } else { dxDialog("<%=ResString("msgUnableToPaste") %>", "", undefined, "<%=ResString("lblError") %>"); }
    }

    function doCopyToClipboardCosts(unique_id) {
        var res = "";
        $("input.tb_cost").each(function() { res += (res == "" ? "" : "\r\n") + this.value; });
        //var controls_data_len = controls_data.length;        
        //for (var i = 0; i < controls_data_len; i++) {
        //    res += (res == "" ? "" : "\r\n") + controls_data[i][COL_CTRL_COST];
        //}
        copyDataToClipboard(res);
    }

    function commitPasteCostsChrome() {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) pasteCosts(pasteBox.value);
    }

    var attribute_index;

    function doPasteAttr(attr_idx) {
        attribute_index = attr_idx;
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteAttr(data);
        } else {
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteAttr);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteAttrChrome('"+attr_idx+"');", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");                        
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }
        }
    }

    function pasteAttr(data) {
        var attr_idx = attribute_index;
        if (data != undefined && data != "") {
            var visControlIDs = "";
            $("input.tb_cost").each(function() { visControlIDs += (visControlIDs == "" ? "" : ",") + this.getAttribute("data-ctrl_id") + ""; });
            sendCommand("action=paste_attr&attr_idx=" + attr_idx + "&data=" + encodeURIComponent(data) + "&ids=" + visControlIDs);
        } else { dxDialog("<%=ResString("msgUnableToPaste") %>", "", undefined, "<%=ResString("lblError") %>"); }
    }

    function commitPasteAttrChrome(attr_idx) {
        attribute_index = attr_idx;
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) pasteAttr(pasteBox.value);
    }

    //function doCopyToClipboardAttr(attr_idx) {
    //    var res = "";
        
    //    $(".attrinput.attr_"+attr_idx).each(function() { 
    //        res += (res == "" ? "" : "\r\n") + (this.nodeName.toLowerCase() == "select" ? this.options[this.selectedIndex].text : this.value );
    //    });

    //    copyDataToClipboard(res);
    //}
    function doCopyToClipboardValues(attr_idx) {
        var res = "";
        var controls_data_len = controls_data.length;        
        for (var i = 0; i < controls_data_len; i++) {
            res += (res==""?"":"\r\n") + controls_data[i]["v" + attr_idx];
        }
        copyDataToClipboard(res);
    }
    // end Column Header context menu
   
    function Hotkeys(event) {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            //            if (code == 13) editControl(edit_param, edit_uid, 0);
            //            if (code == 27) editControl(edit_param, edit_uid, -1);
            if (code == ENTERKEY) {
                if (on_hit_enter != "" && (!(dlg_attributes_cat) || !dlg_attributes_cat.dialog("isOpen"))) {
                    eval(on_hit_enter);
                    //if (code == TABKEY) on_hit_enter_tmp = "showValuesEditor";
                }
            }
            if ((code == ENTERKEY) || (code == TABKEY)) {
                if (on_hit_enter_cat != "") {                    
                    eval(on_hit_enter_cat);
                    on_hit_enter_cat = "";
                }
                return false;
            }
       }
    }

    function markFundedControls(data) {
        
        var active_guids = [];
        for (var i = 0; i < data.length; i++) {
            if (data[i][COL_CTRL_IS_ACTIVE]) active_guids.push(data[i][COL_CTRL_GUID]);
        }

        for (var i = 0; i < controls_data.length; i++) {
            var c = controls_data[i];
            c[COL_CTRL_IS_ACTIVE] = $.inArray(c[COL_CTRL_GUID], active_guids) >= 0;
            c[COL_CTRL_FUNDED] = c[COL_CTRL_IS_ACTIVE];
            $("input.control_is_active_cb[data-ctrl_id='" + c[COL_CTRL_GUID] + "']").prop("checked", c[COL_CTRL_IS_ACTIVE]);
        }

        var rows = $("tr.control_row");        
        //rows.find("td").css("background-color", "");
        rows.each(function (index, val) {
            //$(val).removeClass("funded_row");
            //$(val).removeClass("ra_funded");
            $(val).removeClass("ra_funded_solid");
            <%--<%If IsOptimizationAllowed Then%>--%>
            $(val).find("td:eq(" + COL_CTRL_FUNDED + ")").html("&nbsp;"); // funded column
            <%--<%End If%>--%>
            var ctrl_id =  $(val).attr("data-ctrl_id") + "";
            if (ctrl_id != "") {
                for (var i = 0; i < active_guids.length; i++) {
                    if (active_guids[i] == ctrl_id) {
                        if (!isEditable) $(val).addClass("ra_funded_solid"); // don't highlight funded controls with yellow if editable
                        <%--<%If IsOptimizationAllowed Then%>--%>
                        $(val).find("td:eq(" + COL_CTRL_FUNDED + ")").html("<%=ResString("lblYes")%>");
                        <%--<%End If%>--%>
                        i = active_guids.length;
                    }
                }
            }
        });
    }

    // Export Solver Logs
    function onExportLogsClick() {
        CreatePopup("<% = PageURL(CurrentPageID, String.Format("&{0}=export&type=logs{1}&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" +  Math.round(100000*Math.random()), "export_logs", "");
    }

    function onDownloadBar() {
        document.location.href = "<% = PageURL(CurrentPageID, String.Format("&{0}=download&type=bar{1}&rnd=", _PARAM_ACTION, GetTempThemeURI(True)))%>" +  Math.round(100000*Math.random());
    }

    // Options Menu
    function onOptions_Menu(sender) {
        is_context_menu_open = false;
        if ((sender)) {
            var rect = sender.getBoundingClientRect();
            var x = rect.left   + $(window).scrollLeft() + 0;
            var y = rect.height + $(window).scrollTop()  + 10;
            $("div.options-menu").css({top: y + "px", left: x + "px"}).hide();
            $("div.options-menu").fadeIn(500);
            setTimeout('canCloseMenu()', 200); // this will set is_context_menu_open = true;
        }
    }

    // Pagination
    //function onPagination_Menu(sender) {
    //    is_context_menu_open = false;
    //    if ((sender)) {
    //        var rect = sender.getBoundingClientRect();
    //        var x = rect.left   + $(window).scrollLeft() + 0;
    //        var y = rect.bottom + $(window).scrollTop()  + 1;
    //        $("div.pagination-menu").css({top: y + "px", left: x + "px"}).hide();
    //        $("div.pagination-menu").fadeIn(500);
    //        setTimeout('canCloseMenu()', 200); // this will set is_context_menu_open = true;
    //    }
    //}

    function setPagination(value) {
        pagination = value*1;        
        sendCommand("action=pagination&value=" + value, true);
    }

    // Randomize Controls
    function onConfirmRandomizeControls() {
        dxConfirm("All current <%=ParseString("%%control%%")%> assignments will be lost. Are you sure you want to proceed?", "randomizeControls();", ";");
    }

    function randomizeControls() {
        sendCommand("action=randomize_controls", true);
    }

    // Select Events/Sources/Consequences Dialog
    function onSelectObjectsClick(_nodes, _is_hierarchy, _hid, _cmd) {
        var _title = "Select <%=ParseString("%%Alternatives%%")%>";
        if (_is_hierarchy) {
            if (_hid == 0) {
                _title = "Select <%=ParseString("%%Objective(l)%%")%>";
            } else {
                _title = "Select <%=ParseString("%%Objective(i)%%")%>";
            }
        }

        if (_nodes.length > 0) {
            initSelectObjectsForm(_title, _nodes, _is_hierarchy, _cmd);
            dlg_select_events.dialog("open");
            dxDialogBtnDisable(true, true);
        }
    }

    var chk_tree_settings = {
        callback: {
            onCheck: onCheck
        },
        view: {
            showIcon: false,
            showTitle: true,
            fontCss: getFont,
            nameIsHTML: false,
            dblClickExpand: true
        }, 
        check: {
            enable: true,
            chkStyle: "checkbox",
            chkboxType: { "Y": "s", "N": "s" }
        }
    };

    function onCheck(event, treeId, treeNode) {        
        dxDialogBtnDisable(true, false);
    }
   
    function getFont(treeId, node) {
        return node.font ? node.font : {};
    } 

    function getCheckedCVTreeNode(nodes) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i = 0; i < nodes_len; i++) {
                if (nodes[i].checked) sEventIDs += (sEventIDs == "", "", ",") + nodes[i].id;
                getCheckedCVTreeNode(nodes[i].children);
            }
        }
    }

    function filterSelectAllNodes(chk) {
        zTreeObj.checkAllNodes(chk == 1);
        dxDialogBtnDisable(true, false);
    }

    var isHierarchy = false;
    var zTreeObj;

    function initSelectObjectsForm(_title, _nodes, _is_hierarchy, _cmd) {
        cancelled = false;
        isHierarchy = _is_hierarchy;

        var dlg_id = "#selectEventsForm";
        // content
        if (!_is_hierarchy) {
            var labels = "";

            // generate list of events
            var event_list_len = event_list.length;
            for (var k = 0; k < event_list_len; k++) {
                var checked = event_list[k][IDX_EVENT_ENABLED] == 1;
                labels += "<div class='divCheckbox'><label><input type='checkbox' class='select_event_cb' value='" + event_list[k][IDX_EVENT_ID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(event_list[k][IDX_EVENT_NAME]) + "</label></div>";
            }

            $("#divSelectEvents").html(labels);
        } else {
            dlg_id = "#selectNodesForm";
            zTreeObj = $.fn.zTree.init($("#ulTree"), chk_tree_settings, _nodes);
        }

        dlg_select_events = $(dlg_id).dialog({
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
                    
                    if (!isHierarchy) {
                        var cb_arr = $("input:checkbox.select_event_cb");
                        var chk_count = 0;
                        $.each(cb_arr, function (index, val) { 
                            var cid = val.value + ""; 
                            if (val.checked) { 
                                sEventIDs += (sEventIDs == "" ? "" : ",") + cid; 
                                chk_count += 1;
                            } 
                            event_list[index][IDX_EVENT_ENABLED] = (val.checked ? 1 : 0);
                        });
                        //if ((chk_count == event_list.length) && (chk_count > 0)) sEventIDs = "all";
                        //if (chk_count == 0) sEventIDs = "";
                    } else {
                        var nodes = zTreeObj.getNodes();                
                        getCheckedCVTreeNode(nodes);      
                    }
                    sendCommand('action=' + _cmd + '&ids=' + sEventIDs); // save the selected events via ajax
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
            initSelectControlsForm("Available <%=ParseString("%%Controls%%")%>");
            dlg_select_controls.dialog("open");
            dxDialogBtnDisable(true, true);
        }
    }
    function initSelectControlsForm(_title) {
        cancelled = false;

        var labels = "";

        // generate list of events
        var controls_data_len = controls_data.length;
        for (var k = 0; k < controls_data_len; k++) {
            var checked = !controls_data[k][COL_CTRL_DISABLED];
            labels += "<div class='divCheckbox'><label><input type='checkbox' class='select_control_cb' value='" + controls_data[k][COL_CTRL_GUID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' >" + htmlEscape(controls_data[k][COL_CTRL_NAME]) + "</label></div>";
        }

        $("#divSelectControls").html(labels);

        dlg_select_controls = $("#selectControlsForm").dialog({
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
                        dlg_select_controls.dialog("close");
                    }
                },
                Cancel: function () {
                    cancelled = true;
                    dlg_select_controls.dialog("close");
                }
            },
            open: function () {
                $("body").css("overflow", "hidden");
            },
            close: function () {
                $("body").css("overflow", "auto");
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
                        controls_data[index][COL_CTRL_DISABLED] = !val.checked;
                    });
                    //if ((chk_count == controls_data.length) && (chk_count > 0)) sControlIDs = "all";
                    //if (chk_count == 0) sControlIDs = "none";
                    sendCommand('action=select_controls&ids=' + sControlIDs); // save the selected controls via ajax
                }
            }
        });
        $(".ui-dialog").css("z-index", 9999);
    }

    function filterSelectAllControls(chk) {
        $("input:checkbox.select_control_cb").prop('checked', chk * 1 == 1);
        dxDialogBtnDisable(true, false);
    }
    // end Select Controls dialog

    // DataTables
    function initPage() {
        if (view_grid) {
            initDatatable();
        } else {
            initTimeperiods();
        }
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(id, parent_id, offset) {
        var d = ($("body").hasClass("fullscreen") ? 12 + offset : offset);
        $("#" + id).height(0).width(100);
        var td = $("#" + parent_id);
        $("#" + id).width(td[0].clientWidth);
        $("#" + id).height(td[0].clientHeight-d);
        var w = $("#" + id).width();
        var h = $("#" + id).height();
        //$("#grid").css("max-width", w);
        //$("#grid").show();
        if ((grid_w_old!=w || grid_h_old!=h)) {
            grid_w_old = w;
            grid_h_old = h;
            //onSetAutoPager();
        };
    }

    function checkResize(id, w_o, h_o) {
        var w = $(window).width();
        var h = $(window).height();
        if (!w || !h || !w_o || !h_o || (w==w_o && h==h_o)) {
            resizeGrid(id, "divContent", 8);
        }
    }    

    function resizePage(force_redraw) {        
        $("button.button").css({ "max-height": "24px", "width" : "auto" });

        $("#tableContent").height(0).width(100);
        <% If CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then %>
        $("#divHierarchy").height(0).width(100);
        <% End If %>

        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        if (view_grid) { 
            checkResize("tableContent", force_redraw ? 0 : w, force_redraw ? 0 : h);
        } else {
            resizeTimeperiods();
        }        
        
        <% If CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then %>
        resizeGrid("divHierarchy", "divTreeContainer", 0);
        <% End If %>
    }

    function getControlTypeName(ctrlType) {
        var retVal = "";
        switch (ctrlType*1) {
                case ctCause:
                    retVal = ctCauseName;
                    break;
                case ctConsequenceOld:
                    retVal = ctConsequenceToEventName;
                    break;
                case ctCauseToEvent:
                    retVal = ctCauseToEventName;
                    break;
                case ctConsequenceToEvent:
                    retVal = ctConsequenceToEventName;
                    break;
            }
        return retVal;
    }

    var store;

    function setAttributeValueSilent(cellInfo, e, is_multi) {
        var oldAttrValue = cellInfo.value;
        var value = "";
         
        if (is_multi) {
            for (var i = 0; i < e.value.length; i++) {
                value += (value == "" ? "" : ";") + e.value[i].key;
            }
        } else {
            value = e.value;
        }

        cellInfo.data[cellInfo.column.dataField] = typeof e.text == "undefined" ? value : e.text;
        //var alt = getAlternativeById(cellInfo.data["guid"]);
        //if ((alt)) {
        //    alt[cellInfo.column.dataField] = value;
        //sendCommand("action=set_attribute_value&guid=" + cellInfo.data["guid"] + "&" + cellInfo.column.dataField + "=" + e.value, true);
        e.component.option("validationStatus", "pending");
        callAPI("pm/hierarchy/?action=set_attribute_value", {"guid" : cellInfo.data["guid"], "dataField" : cellInfo.column.dataField, "value" : value}, function (data) {
            var dg = $("#tableAlts").dxDataGrid("instance");
            if (!data.success) {
                alt[cellInfo.column.dataField] = oldAttrValue;
                cellInfo.data[cellInfo.column.dataField] = oldAttrValue;
                store = new DevExpress.data.ArrayStore({
                    key: 'guid',
                    data: alts_data
                });
                dg.option("dataSource", store);
                dg.refresh(true);
            }
            DevExpress.ui.notify(data.success ? "Changes Saved" : "Error", data.success ? "success" : "error");
            e.component.option("validationStatus", data.success ? "valid" : "invalid");
        }, true);
        //}
    }

    var booleanCustomizeText = function (cellInfo) {
        return cellInfo.value ? "<% = ResString("lblYes") %>" : "";
    }

    var is_init = true;

    function initDatatable() {
        var table = (($('#tableContent').data("dxDataGrid"))) ? $('#tableContent').dxDataGrid("instance") : null;
        if (table !== null) table.dispose();

        //init columns headers                
        var columns = [];
        for (var i = 0; i < controls_columns.length; i++) {
            columns.push({ "caption" : controls_columns[i][COL_COLUMN_NAME], "dataField" : controls_columns[i][COL_COLUMN_DATA_FIELD], "alignment" : controls_columns[i][COL_COLUMN_CLASS], "allowSorting" : controls_columns[i][COL_COLUMN_SORTABLE], "allowSearch" : controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_NAME || controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_TYPE, "visible" : controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_RISK_REDUCTION ? ShowControlsRiskReduction : controls_columns[i][COL_COLUMN_VISIBILITY], "attrIndex": controls_columns[i][COL_COLUMN_ATTR_INDEX], "type" : controls_columns[i][COL_COLUMN_TYPE], "width" : controls_columns[i][COL_COLUMN_WIDTH]} );

            if (i <= 3) {
                columns[columns.length - 1].fixed = true;
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] === "actions") {
                columns[columns.length - 1].allowExporting = false;
            }
            
            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_IS_ACTIVE || (controls_columns[i][COL_COLUMN_DATA_FIELD] == "" && controls_columns[i][COL_COLUMN_DATA_FIELD] !== "actions")) {
                columns[columns.length - 1].caption = "";
                columns[columns.length - 1].showInColumnChooser = false;
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_ID) {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    element.append(padWithZeros(info.value, controls_data_len));
                }
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_IS_ACTIVE) {
                columns[columns.length - 1].allowExporting = false;
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var sChk = "";
                    if (isMultiselect) {
                        var isMarked = getIsControlMarked(info.data[COL_CTRL_GUID]);                    
                        sChk = "<input type='checkbox' class='control_mark_cb checkbox' " + (isMarked ? " checked='checked' " : "") + " data-ctrl_id='" + info.data[COL_CTRL_GUID] + "' onclick='markControl(event,\"" + info.data[COL_CTRL_GUID] + "\", this.checked);' >";
                    } else {
                        if (info.data[COL_CTRL_TYPE] != ctUndefined && !info.data[COL_CTRL_DISABLED]) {
                            <%--sChk = "<input type='checkbox' class='control_is_active_cb checkbox' " + (info.data[COL_CTRL_IS_ACTIVE] && info.data[COL_CTRL_APPL_COUNT] > 0 ? " checked='checked' " : "") + (info.data[COL_CTRL_APPL_COUNT] == 0 || info.data[COL_CTRL_TYPE] == ctUndefined ? " disabled='disabled' " : "") + " data-ctrl_id='" + info.data[COL_CTRL_GUID] + "' onclick='activateControl(event,\"" + info.data[COL_CTRL_GUID] + "\", this.checked);' " + (info.data[COL_CTRL_APPL_COUNT] == 0 ? " title='<%=ResString("lblNoApplications")%>' " : "") + " >";--%>
                            sChk = "<input type='checkbox' class='control_is_active_cb checkbox' " + (info.data[COL_CTRL_IS_ACTIVE] ? " checked='checked' " : "") + (info.data[COL_CTRL_TYPE] == ctUndefined ? " disabled='disabled' " : "") + " data-ctrl_id='" + info.data[COL_CTRL_GUID] + "' onclick='activateControl(event,\"" + info.data[COL_CTRL_GUID] + "\", this.checked);' " + (info.data[COL_CTRL_APPL_COUNT] == 0 ? " title='<%=ResString("lblNoApplications")%>' " : "") + " >";
                        }
                    }
                    element.append(sChk);
                }

                columns[columns.length - 1].headerCellTemplate = function (columnHeader, headerInfo) {                    
                    return columnHeader.append("<%= ParseString(If(IsEditable, "<input id=\'cbMarkAll\' type=\'checkbox\' onclick=\'markAllControls(event, this.checked);\' >", "Selected"))%>");
                }            
                
                columns[columns.length - 1].customizeText = booleanCustomizeText;
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_NAME) {
                columns[columns.length - 1].cellTemplate = function(element, info) {            
                    var data = info.data;
                    var sName =  htmlEscape(trim(data[COL_CTRL_NAME]));                    
                    var sDescription = (data[COL_CTRL_INFODOC] == "" ? "" : (showDescriptions ? "<div class='small' style='font-style:normal; font-weight:normal;'>" + parseDescription(data[COL_CTRL_INFODOC], data[COL_CTRL_GUID]) + "</div>" : ""));
                    element.append(sName + sDescription);
                }
                columns[columns.length - 1].minWidth = 130;
            }            

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_TYPE) {
                columns[columns.length - 1].headerFilter = {dataSource : [
                    { text: ctUndefinedName, value: ctUndefined },
                    { text: ctCauseName, value: ctCause },
                    { text: ctCauseToEventName, value: ctCauseToEvent },
                    { text: ctConsequenceToEventName, value: ctConsequenceToEvent }
                ]};
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;

                    <%If IsEditable Then%>
                    var cbControlType = "<select class='select_control_type' style='width:100%;' onchange='setControlType(this,\"" + data[COL_CTRL_GUID] + "\",this.value);' data-ctrl_id='" + data[COL_CTRL_GUID] + "' data-ctrl_type='" + data[COL_CTRL_TYPE] + "' " + ($("input.control_mark_cb:checked").length == 0 || getIsControlMarked(data[COL_CTRL_GUID])  ? "" : " disabled='disabled' ")  + " >";
                    cbControlType += "<option value='" + ctUndefined + "' " + (data[COL_CTRL_TYPE] == ctUndefined ? " selected='selected' " : "") + ">" + ctUndefinedName + "</option>";
                    cbControlType += "<option value='" + ctCause + "' " + (data[COL_CTRL_TYPE] == ctCause ? " selected='selected' " : "") + ">" + ctCauseName + "</option>";
                    cbControlType += "<option value='" + ctCauseToEvent + "' " + (data[COL_CTRL_TYPE] == ctCauseToEvent ? " selected='selected' " : "") + ">" + ctCauseToEventName + "</option>";
                    cbControlType += "<option value='" + ctConsequenceToEvent + "' " + (data[COL_CTRL_TYPE] == ctConsequenceToEvent ? " selected='selected' " : "") + ">" + ctConsequenceToEventName + "</option>";
                    cbControlType += "</select>";
                    element.append(cbControlType);
                    <%Else%>
                    element.append(getControlTypeName(data[COL_CTRL_TYPE]));
                    <%End If%>
                },
                columns[columns.length - 1].customizeText = function(cellInfo) {
                    return getControlTypeName(cellInfo.value);
                }
            }
            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == "actions") {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    var sInfodoc = data[COL_CTRL_INFODOC] == "" ? "" : "&nbsp;<i class='fas fa-search' style='cursor: pointer;' onclick='showInfodoc(\"" + data[COL_CTRL_GUID] + "\"); return false;'></i>";
                    var sHamButton = "&nbsp;<i class='fas fa-bars' style='cursor: pointer;' onclick='showMenu(event,\"" + data[COL_CTRL_GUID] + "\");'></i>";
                    element.append(sHamButton + sInfodoc);
                }
            }            

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_FUNDED) {
                columns[columns.length - 1].allowEditing = false;
                columns[columns.length - 1].alignment = "center";
                columns[columns.length - 1].headerFilter = {dataSource : [{
                    text: "<%=ResString("btnYes")%>",
                    value: "1"
                },{
                    text: "<%=ResString("btnNo")%>",
                    value: "0"
                }]};
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    element.append(info.data.funded ? "<%=ResString("btnYes")%>" : "");
                }
                columns[columns.length - 1].customizeText = booleanCustomizeText;
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_COST) {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    element.append("<input type='number' class='input number tb_cost' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' step='any' style='width:100%; text-align:right; -moz-appearance:textfield;' value='" + data[COL_CTRL_COST] + "' data-ctrl_id='" + data[COL_CTRL_GUID] + "' onclick='this.select();' " + (!isReadOnly && ($("input.control_mark_cb:checked").length == 0 || getIsControlMarked(data[COL_CTRL_GUID]))  ? "" : " disabled='disabled' ") + " >"); // Cost
                    element.className += " datatables_cost_column";
                }
                columns[columns.length - 1].headerCellTemplate = function (columnHeader, headerInfo) {                    
                    return columnHeader.append(headerInfo.column.caption + "<i class=\'fas fa-bars toolbar-icon\' style=\'cursor:context-menu; padding-left:3px; float:right;\' onclick=\'return showColumnMenu(event, this, true, <%=Bool2JS(Not App.IsActiveProjectReadOnly)%>,-1,5);\'></i>&nbsp;");
                }
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_APPL_COUNT) {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    var sChk = "";
                    if (data[COL_CTRL_TYPE] != ctUndefined) {
                        sChk = (data[COL_CTRL_APPL_COUNT] > 0 ? "<a href='' class='actions' onclick='sendCommand(\"" + "action=showapplications&id=" + data[COL_CTRL_GUID] + "\"); return false;'>" + data[COL_CTRL_APPL_COUNT] + "</a>" : "<center><small><%=ResString("lblNoApplications")%></small></center>");
                        sChk += "&nbsp;<i class='fas fa-plus-circle ec-icon' style='cursor: pointer;' onclick='navigateToApplyControls(" + data[COL_CTRL_TYPE] + ", \"" + data[COL_CTRL_GUID] + "\"); return false;' title='<%=ResString("lblApplyControls")%>' alt='<%=ResString("lblApplyControls")%>'></i>";
                    }
                    element.append(sChk);
                }
            }                

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_RISK_REDUCTION) {
                columns[columns.length - 1].showInColumnChooser = false;
                columns[columns.length - 1].visible = ShowControlsRiskReduction;
                //columns[columns.length - 1].cellTemplate = function(element, info) {
                //    var data = info.data;
                //    data[COL_CTRL_RISK_REDUCTION] = ShowDollarValue ? data[COL_CTRL_SA_REDUCTION_DOLL] : data[COL_CTRL_SA_REDUCTION];
                //    element.append(ShowDollarValue ? data[COL_CTRL_SA_REDUCTION_DOLL] : data[COL_CTRL_SA_REDUCTION]);
                //}
                columns[columns.length - 1].dataField = ShowDollarValue ? COL_CTRL_SA_REDUCTION_DOLL : COL_CTRL_SA_REDUCTION;
                columns[columns.length - 1].dataType = "number";
                columns[columns.length - 1].format = { "type" : ShowDollarValue ? "currency" : "percent", "precision" : ShowDollarValue ? 0 : 2 };
                columns[columns.length - 1].customizeText = function(cellInfo) {
                    return cellInfo.value === UNDEFINED_INTEGER_VALUE ? "" : cellInfo.valueText;
                }
                columns[columns.length - 1].headerCellTemplate = function (columnHeader, headerInfo) {                    
                    return columnHeader.append(headerInfo.column.caption + ",&nbsp;" + "<span id='lblSAChar'>" + (ShowDollarValue ? CHAR_CURRENCY_SYMBOL : "&#37;") + "</span>");
                }
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_DISABLED) {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    element.append("<input type='checkbox' class='checkbox cb_set_disabled' " + (data[COL_CTRL_DISABLED] ? " checked='checked' " : " ") + " onclick='setConstraint(event, this.checked, \"set_disabled\", \"" + data[COL_CTRL_GUID] + "\");' data-ctrl_id='" + data[COL_CTRL_GUID] + "' data-cb_type='dis' >");
                }
                columns[columns.length - 1].customizeText = booleanCustomizeText;
            }
                
            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_MUST) {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    element.append("<input type='checkbox' class='checkbox cb_set_must' " + (data[COL_CTRL_MUST] ? " checked='checked' " : " ") + " onclick='setConstraint(event, this.checked, \"set_must\", \"" + data[COL_CTRL_GUID] + "\");' data-ctrl_id='" + data[COL_CTRL_GUID] + "' data-cb_type='must' >");
                }
                columns[columns.length - 1].customizeText = booleanCustomizeText;
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == COL_CTRL_MUST_NOT) {
                columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;
                    element.append("<input type='checkbox' class='checkbox cb_set_must_not' " + (data[COL_CTRL_MUST_NOT] ? " checked='checked' " : " ") + " onclick='setConstraint(event, this.checked, \"set_must_not\", \"" + data[COL_CTRL_GUID] + "\");' data-ctrl_id='" + data[COL_CTRL_GUID] + "' data-cb_type='mustnot' >");
                }
                columns[columns.length - 1].customizeText = booleanCustomizeText;
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == "sa") {
                columns[columns.length - 1].showInColumnChooser = false;
                columns[columns.length - 1].customizeText = function(cellInfo) {
                    return cellInfo.value === UNDEFINED_INTEGER_VALUE ? "" : cellInfo.valueText;
                };
                columns[columns.length - 1].format = {
                    "type": "percent", 
                    "precision": 2
                }
            }
            if (controls_columns[i][COL_COLUMN_DATA_FIELD] == "sa_doll") {
                columns[columns.length - 1].showInColumnChooser = false;
                columns[columns.length - 1].customizeText = function(cellInfo) {
                    return cellInfo.value === UNDEFINED_INTEGER_VALUE ? "" : cellInfo.valueText;
                };
                columns[columns.length - 1].format = {
                    "type": "currency"
                }
            }

            if (controls_columns[i][COL_COLUMN_DATA_FIELD][0] == "v") {
                if (controls_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtLong%> || controls_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtDouble%>) {                
                    columns[columns.length - 1].cellTemplate = function(element, info) {
                        $("<div></div>").dxNumberBox({
                            width: "100%",                        
                            value: info.value,
                            onValueChanged: function(e) {
                                var data = info.data;
                                //setAttributeValueSilent(cellInfo, e);
                                data["v" + controls_columns[info.column.visibleIndex][COL_COLUMN_ATTR_INDEX]] = e.value;
                                txtAttrValueChange(data[COL_CTRL_GUID], controls_columns[info.column.visibleIndex][COL_COLUMN_ATTR_INDEX], e.value);
                            }
                        }).appendTo(element);                    
                    }
                }

                if (controls_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtBoolean%>) {
                    columns[columns.length - 1].allowEditing = false;
                    columns[columns.length - 1].alignment = "center";
                    columns[columns.length - 1].headerFilter = {dataSource : [{
                        text: "<%=ResString("btnYes")%>",
                        value: "1"
                    },{
                        text: "<%=ResString("btnNo")%>",
                        value: "0"
                    }]};
                    columns[columns.length - 1].cellTemplate = function (cellElement, cellInfo) {
                        $("<div></div>").dxCheckBox({
                            width: 50,
                            value: cellInfo.value,
                            onValueChanged: function(e) {
                                //setAttributeValueSilent(cellInfo, e)
                                cellInfo.data["v" + controls_columns[cellInfo.column.visibleIndex][COL_COLUMN_ATTR_INDEX]] = e.value;
                                boolAttrValueChange(cellInfo.data[COL_CTRL_GUID], controls_columns[cellInfo.column.visibleIndex][COL_COLUMN_ATTR_INDEX], e.value);
                            }
                        }).appendTo(cellElement);                    
                    }
                }

                if (controls_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtEnumeration%>) {
                        columns[columns.length - 1].cellTemplate = function (cellElement, cellInfo) {
                            var sOptions = [];
                            var selectedOption = null;
                            var attr_id = "";
                            var alt_id = cellInfo.data["guid"];
                            for (var k = 0; k < attr_data.length; k++) {
                                var v = attr_data[k];
                                if (v[IDX_ATTR_ID] == cellInfo.column.attrIndex) {
                                    attr_id = v[IDX_ATTR_ID];
                                    for (var t = 0; t < v[IDX_ATTR_ITEMS].length; t++) {
                                        sOptions.push({ "guid" : v[IDX_ATTR_ITEMS][t][0], "text" : v[IDX_ATTR_ITEMS][t][1] });
                                        if (v[IDX_ATTR_ITEMS][t][0] == cellInfo.data["vguid" + k]) selectedOption = cellInfo.data["vguid" + k];
                                    }
                                    k = attr_data.length;
                                }
                            }

                            $("<div></div>").dxSelectBox({
                                acceptCustomValue: false,
                                width: "100%",
                                dataSource: sOptions,
                                displayExpr: "text",
                                searchEnabled: true,
                                value: selectedOption,
                                valueExpr: "guid",
                                //onKeyDown: function (e) {
                                //    if (e.event.originalEvent.keyCode == KEYCODE_ENTER) {
                                //        e.component.blur();
                                //    }
                                //},
                                onValueChanged: function(e) {
                                    if (typeof e.value == "undefined") {
                                        var newValue = e.component.option("selectedItem");
                                        var optionExists = false;
                                        var nvl = newValue.toLowerCase();
                                        for (var i = 0; i < sOptions.length; i++) {
                                            if (nvl == sOptions[i].text.toLowerCase()) optionExists = true;
                                        }
                                        if ((newValue) && newValue !== "" && !optionExists) {
                                            dxConfirm("Do you want to add a new category \""+newValue+"\" and associate it with this <%=ResString("lblAlternative").ToLower%>", "sendCommand(\"action=add_item_and_assign&name=" + newValue + "&clmn=" + attr_id + "&alt_id=" + alt_id + "\");", function () { e.value = e.previousValue });
                                        }
                                    } else {                           
                                        for (var t = 0; t < sOptions.length; t++) { if (sOptions[t].guid == e.value) e.text = sOptions[t].text; }
                                        //setAttributeValueSilent(cellInfo, e);
                                        enumAttrValueChange(cellInfo.data[COL_CTRL_GUID], controls_columns[cellInfo.column.visibleIndex][COL_COLUMN_ATTR_INDEX], e.value);
                                    }
                                }
                             }).appendTo(cellElement);                    
                         }
                    }
                

                    if (controls_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtEnumerationMulti%>) {
                        columns[columns.length - 1].cellTemplate = function (cellElement, cellInfo) {

                        var multi_items = new DevExpress.data.DataSource({ store: [] });
                        var sel_items = [];

                        for (var k = 0; k < attr_data.length; k++) {
                            var v = attr_data[k];
                            if (v[IDX_ATTR_ID] == cellInfo.column.attrIndex) {
                                for (var t = 0; t < v[IDX_ATTR_ITEMS].length; t++) {
                                    var item = {"key" : v[IDX_ATTR_ITEMS][t][0], "chk" : (cellInfo.data["vguid" + k]) && (cellInfo.data["vguid" + k] !== "") && cellInfo.data["vguid" + k].indexOf(v[IDX_ATTR_ITEMS][t][0]) != -1, "text" : v[IDX_ATTR_ITEMS][t][1]};
                                    multi_items.store().insert(item);
                                    
                                    if (item.chk) {
                                        sel_items.push(item);
                                    }
                                }
                                k = attr_data.length;
                            }
                        }

                        $('<div></div>').dxTagBox({
                            dataSource: multi_items,
                            showSelectionControls: true,
                            keyExpr: "key",
                            displayExpr: "text",
                            value: sel_items,
                            wrapItemText: false,
                            onValueChanged: function (e) {
                            //if (e.addedItems.length || e.removedItems.length) {
                                    e.text = "";
                                    for (var t = 0; t < e.value.length; t++) { e.text += (e.text == "" ? "" : ", ") + e.value[t].text; }
                                    notImplemented();
                                    //setAttributeValueSilent(cellInfo, e, true);
                                //}
                            },
                            onCustomItemCreating: function(e){
                                e.customItem = { "key" : "new_item_guid", "chk" : true, "text" : "" };
                                multi_items.store().insert(e.customItem);
                                multi_items.reload();
                            }
                        }).appendTo(cellElement);
                        }    
                    }

                <%--columns[columns.length - 1].cellTemplate = function(element, info) {
                    var data = info.data;

                    if (attr_data.length > 0 && ((data))) {
                        var disabled = isReadOnly ? " disabled='disabled' " : "";
                        var backgroundColor = "#ffffff;";
                        //for (var i = 0; i < attr_data.length; i++) {
                            var k = controls_columns[info.column.visibleIndex][COL_COLUMN_ATTR_INDEX];
                            var sAttrValue = data[controls_columns[info.column.visibleIndex][COL_COLUMN_DATA_FIELD]];
                            var sAttrValueInput = "";
                            var attr = attr_data[k];
                            var sAlign = "text-align: right;";
                            switch (attr[IDX_ATTR_TYPE]) {
                                case avtBoolean:
                                    var items = attr[IDX_ATTR_ITEMS];
                                    sAttrValueInput = "<select class='select attrinput attr_"+attr[IDX_ATTR_ID]+"' style='width: 50px; margin-right: 10px; margin-left: 10px; background-color: " + backgroundColor + ";' onchange='boolAttrValueChange(\"" + data[COL_CTRL_GUID] + "\"," + k + ",this.value);'>";
                                    sAttrValueInput += "<option value='1' " + (sAttrValue == "1" ? " selected " : "") + "><%=ResString("btnYes")%></option>";
                                    sAttrValueInput += "<option value='0' " + (sAttrValue == "0" ? " selected " : "") + "><%=ResString("btnNo")%></option>";                                
                                    sAttrValueInput += "</select>";
                                    break;
                                case avtLong:
                                case avtDouble:
                                case avtString:                                
                                    if (attr[IDX_ATTR_TYPE] == avtString) sAlign = "text-align: left;";
                                    sAttrValueInput = "<input type='text' class='input attrinput attr_"+attr[IDX_ATTR_ID]+"' style='width: 100%; background-color: " + backgroundColor + sAlign + "' value='" + sAttrValue + "' onchange='txtAttrValueChange(\"" + data[COL_CTRL_GUID] + "\"," + k + ",this.value);' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' " + disabled + ">";
                                    break;
                                case avtEnumeration:
                                    var items = attr[IDX_ATTR_ITEMS];
                                    sAttrValueInput = "<select class='select attrinput attr_"+attr[IDX_ATTR_ID]+"' style='width: 120px; margin-right: 10px; margin-left: 10px; background-color: " + backgroundColor + ";' onchange='enumAttrValueChange(\"" + data[COL_CTRL_GUID] + "\"," + k + ",this.value);'><option value='-1' selected>" + UNDEFINED_ATTRIBUTE_VALUE + "</option>";
                                    for (var j = 0; j < items.length; j++) {
                                        var isSelected = "";
                                        if (sAttrValue == items[j][0]) {
                                            isSelected = "selected";
                                        }
                                        sAttrValueInput += "<option value='" + items[j][0] + "' " + isSelected + ">" + items[j][1] + "</option>";
                                    }
                                    sAttrValueInput += "</select>";
                                    break;
                                case avtEnumerationMulti:
                                    var items = attr[IDX_ATTR_ITEMS];
                                    var sMultiValues = "";
                                    for (var j = 0; j < items.length; j++) {
                                        if (sAttrValue.indexOf(items[j][0]) >= 0) {
                                            sMultiValues += (sMultiValues == "" ? "" : ", ") + ShortString(items[j][1], 10, true);
                                        }
                                    }                     
                                    //var btn = "<button style='float: right;' onclick='multiAttrValueEditor(\"" + data[COL_CTRL_GUID] + "\"," + i + ", \"" + sAttrValue + "\"); return false;'>&hellip;</button>";
                                    var btn = "<i class='fas fa-pencil-alt' style='display: inline-block; vertical-align:middle; cursor: pointer;' onclick='multiAttrValueEditor(\"" + data[COL_CTRL_GUID] + "\"," + k + ", \"" + sAttrValue + "\"); return false;'></i>";
                                    if (sMultiValues == "") {
                                        sAttrValueInput = "<nobr>" + UNDEFINED_ATTRIBUTE_VALUE + btn + "</nobr>";
                                    } else {
                                        sAttrValueInput = "<span>" + sMultiValues + "</span>" + btn;
                                    }
                                    break;
                            }
                            element.append(sAttrValueInput);
                        //}
                    }
                }          --%>      
                <%--columns[columns.length - 1].headerCellTemplate = function (columnHeader, headerInfo) {                    
                    var k = controls_columns[headerInfo.column.visibleIndex][COL_COLUMN_ATTR_INDEX];
                    return columnHeader.append("<span title='" + headerInfo.column.caption + "'>" + headerInfo.column.caption + "</span><i class=\'fas fa-bars toolbar-icon\' style=\'cursor:context-menu; padding-left:3px; float:right;\' onclick=\'return showColumnMenu(event, this, true, <%=Bool2JS(Not App.IsActiveProjectReadOnly)%>,"+k+","+(columns.length-1)+");\'></i>&nbsp;");
                }--%>
            }
        }

        <%--var exportOptions = { //columns: [0, 4, 6, 7, 9], 
            columns: ":not(:eq(4))", // all except hidden "Selected" column
            format: {
                body: function ( data, row, column, node ) {
                    var lblYes = "<%=ResString("lblYes")%>";
                    var retVal = data;
                    switch (row) {
                        case COL_CTRL_IS_ACTIVE: // Selected
                        case COL_CTRL_MUST: // Must - 9 
                        case COL_CTRL_MUST_NOT: // Must Not - 10
                            retVal = data ? lblYes : "";
                            break;
                        case COL_CTRL_TYPE: // Control Type
                            retVal = getControlTypeName(data);
                            break;
                        //case 4: // Selected - hidden 
                        //    retVal = "";
                        //    break;
                    }
                    return retVal;
                }
            }
        };--%>

        var controls_data_len = controls_data.length;

        store = new DevExpress.data.ArrayStore({
            key: 'guid',
            data: controls_data
        });

        $("#tableContent").dxDataGrid({
            dataSource: store,
            columns: columns,
            filterPanel: {
                filterEnabled: true,
                visible: true
            },
            filterRow: {
                visible: false
            },
            pager: {
                allowedPageSizes: <% = _OPT_PAGINATION_PAGE_SIZES %>,
                showPageSizeSelector: true,
                showNavigationButtons: true,
                visible: controls_data.length > pagination
            },
            paging: {
                enabled: controls_data.length > pagination,
                pageSize: pagination > 0 ? pagination : 20
            },
            filterPanel: {
                visible: false
            },
            filterRow: {
                visible: false
            },
            headerFilter: {
                visible: true
            },
            searchPanel: {
                visible: true,
                text: "",
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            allowColumnResizing: true,
            allowColumnReordering: false,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            columnFixing: {
                enabled: true
            },            
            columnChooser: {                
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: true
            },
            "export": {
                enabled: true
            },
            hoverStateEnabled: true,
            rowAlternationEnabled: true,
            //scrolling: {
            //    mode: "virtual"
            //},
            stateStoring: {
                ignoreColumnOptionNames: [],
                enabled: false,
                type: "localStorage",
                storageKey: "Risk_ControlsGrid1_<%=CurrentPageID%>_PRJID_<%=App.ProjectID%>" + columns_ver
            },            
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            onContentReady: function(e) {                
                setTimeout('$("input[type=search]").focus();', 100);

                $('input.tb_cost').on('blur',function(e) {
                    var id = this.getAttribute("data-ctrl_id") + "";
                    var list = getMarkedControlsList();
                    var ctrls = [];
                    if (list == "") {
                        ctrls.push(id);
                    } else {
                        ctrls = list.split(",");
                    }
                    var s = this.value + "";
                    if (validFloat(s)) {
                        var v = str2cost(s);
                        var doSave = false;
                        for (var i = 0; i < ctrls.length; i ++ ) {
                            if (ctrls[i].length > 32) {
                                var ctrl = getControlByID(ctrls[i]);
                                if ((ctrl) && ctrl[COL_CTRL_COST] != v) {
                                    ctrl[COL_CTRL_COST] = v;
                                    doSave = true;
                                }
                            }
                        }
                        if (doSave) {                                                           
                            if (list != "") refreshDataGrid(controls_data);
                            sendCommand('action=edit_cost&ctrl_id=' + id + '&value=' + s +'&ids=' + list, true);
                        }
                    } else { refreshDataGrid(controls_data); }                  

                    return false;
                });

                $('input.tb_cost').on('keyup',function(e) {
                    e.preventDefault();
                    if (e.which == ENTERKEY) { //|| e.which == TABKEY) {
                        //this.blur();
                        //$(this).next('input.tb_cost').focus();
                        $(this).closest('tr').next('tr').find('input.tb_cost').focus().select(); //this.select();
                        return false;
                    }
                });                

                if (is_init) {
                    is_init = false;
                    var state = $("#tableContent").dxDataGrid("instance").state();
                    if (typeof state.columns !== "undefined") {
                        for (var i = 0; i < state.columns.length; i++) {
                            if (state.columns[i].dataField === COL_CTRL_SA_REDUCTION_DOLL || state.columns[i].dataField === COL_CTRL_SA_REDUCTION) {
                                state.columns[i].visible = ShowControlsRiskReduction;
                            }
                        }
                        $("#tableContent").dxDataGrid("instance").state(state);
                    }
                }
            },
            onRowPrepared: function (e) {
                if (e.rowType == "data") {
                    var data = e.data;
                    var row = e.element;
                    var $row = $(row);
                    var $rowEl = $(e.rowElement);
                    if (data[COL_CTRL_TYPE] == ctUndefined || data[COL_CTRL_DISABLED]) {
                        //var undefinedRowStyle = {"font-style" : "italic", "color" : "#666666"};
                        var undefinedRowStyle = {"color" : "#666666"};
                        $row.css(undefinedRowStyle);
                        $("td>select", row).css(undefinedRowStyle);
                        $("td>input", row).css(undefinedRowStyle);
                    //} else {
                    //    $row.css({"font-weight" : "bold"});
                    }

                    $rowEl.attr("data-ctrl_id", data[COL_CTRL_GUID]);
                    $rowEl.addClass("control_row");

                    //$("td", row)
                    $rowEl.removeClass("ra_funded_solid");
                    if (data[COL_CTRL_IS_ACTIVE]) {
                        $rowEl.addClass("ra_funded_solid");
                    }
                }
                //SARR
                if (is_running && e.rowType !== "header" && e.data.sa !== UNDEFINED_INTEGER_VALUE && (e.cells) && e.cells.length >= 6) {
                    for (var i = 0; i < e.cells.length; i++) {
                        var cell = e.cells[i];
                        if (cell.column.dataField === "sa" || cell.column.dataField === "sa_doll") {
                            $(cell.cellElement).effect("highlight", {}, 800);
                            $(cell.cellElement).effect("highlight", {}, 800);
                        }
                    }
                }
            },
            onToolbarPreparing: onCreateGridToolbar,
            noDataText: "<% = GetEmptyMessage() %>",
            wordWrapEnabled: false
        });        
        
        hideLoadingPanel();

        markFundedControls(controls_data);
       
        //setTimeout("dataTablesColumnVisibility('tableContent', COL_CTRL_RISK_REDUCTION, ShowControlsRiskReduction);", 100);
      
        resizePage();        
    }

    function onCreateGridToolbar(e) {       
        <%If Not IsEditable Then%>
        //var template = '<span style="text-align: center; background-color: #fafafa; border: 1px solid #f0f0f0; padding: 3px; vertical-align: middle;">';
        var template = '<span>';
        template += '<input type="checkbox" style="cursor: pointer;" class="checkbox" id="cbDollarValue" ' + (ShowDollarValue ? ' checked="checked" ' : '') + (DollarValue == UNDEFINED_INTEGER_VALUE ? ' disabled="disabled" ' : '') + ' onclick="ShowDollarValueClick(this.checked);" ><label for="cbDollarValue"><span id="lblDollarValue" style="color:' + (DollarValue == UNDEFINED_INTEGER_VALUE ? "#999999" : "black") + '"><% = ResString("lblShowDollarValue") + GetDollarValueFullString() %></span>';
        template += '&nbsp;<i class="fas fa-edit ec-icon fa-lg" style="cursor:pointer;" title="Edit Value of Enterprise" onclick="onEditDollarValueClick(); return false;"></i>';
        template += '</label>';
        template += '</span>';
        e.toolbarOptions.items.splice(0, 0, { location: 'center', locateInMenu: 'never',  template: template, visible: true });
        <%End If%>
        <%If Not App.IsActiveProjectReadOnly And CurrentPageID <> _PGID_RISK_TREATMENTS_DICTIONARY Then %>        
        e.toolbarOptions.items.splice(0, 0, { location: 'before', locateInMenu: 'never',  template: '<span class="text" style="white-space: nowrap;"><span>Select:&nbsp;</span><a href="#" onclick="activateAll(event, \'all\', 1); return false;" class="actions"><%=ResString("lblAll")%></a>&nbsp;|&nbsp;<a href="#" onclick="activateAll(event, \'all\',0); return false;" class="actions"><%=ResString("lblNone")%></a></span>', visible: true });
        <%End If%>
    }
    // end DataTables

    // Attribute value changed functions
    function txtAttrValueChange(altIndex, attrIndex, value) {
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&value=" + encodeURIComponent(value) + "&ids=" + getMarkedControlsList(), false);
    }

    function boolAttrValueChange(altIndex, attrIndex, value) {
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&value=" + value + "&ids=" + getMarkedControlsList(), false);
    }
    
    function enumAttrValueChange(altIndex, attrIndex, value) { // edit single categorical attribute from drop-down
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&enum_idx=" + value + "&ids=" + getMarkedControlsList(), false);
    }

    function multiAttrValueEditor(altIndex, attrIndex, value) { // multi-categorical attribute values editor
        // sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&enum_idx=" + value, false);
        var attr = attr_data[attrIndex];
        var items = attr[IDX_ATTR_ITEMS];
        var sList = "";
        for (var i = 0; i < items.length; i++) {
            var chk = value.indexOf(items[i][0]) >= 0;
            sList += "<label><input class='multi_edit_cb' data-itemid='" + items[i][0] + "' type='checkbox' " + (chk ? " checked='checked' " : "") + " onclick='multiCatClick(event);' >" + items[i][1] + "</label><br>";
        }

        if (items.length > 1) {
            sList += "<br><div class='text' style='white-space: nowrap;'><span>Select:&nbsp;</span><a href='#' onclick='checkAllCategories(event, true); return false;' class='actions'><%=ResString("lblAll")%></a>&nbsp;|&nbsp;<a href='#' onclick='checkAllCategories(event, false); return false;' class='actions'><%=ResString("lblNone")%></a></div>";
        }

        var sMenu = "<div>";
        sMenu += sList;
        sMenu += "</div>";                
        //is_context_menu_open = false;
        //$("div.context-menu").hide().remove();
        //var x = event.clientX;
        //var y = event.clientY;
        //var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});
        //$("body").css("overflow", "hidden");
        //$("div.context-menu").fadeIn(500);
        //setTimeout('canCloseMenu()', 200);
        dxDialog(sMenu, "saveMultiCategories(\"" + altIndex + "\"," + attrIndex + ");", ";", attr[IDX_ATTR_NAME], "<%=ResString("btnSave") %>", "<%=ResString("btnCancel") %>");
    }

    function multiCatClick(e) {
        //var sChkItems = "";
        //$("input:checkbox.multi_edit_cb:checked").each(function() { sChkItems += (sChkItems == "" ? "" : ",") + this.getAttribute("data-itemid"); });
        //alert(sChkItems);
    }

    function checkAllCategories(e, chk) {
        if(e.preventDefault) e.preventDefault(); else e.returnValue = false;        
        $("input:checkbox.multi_edit_cb").prop("checked", chk);
    }

    function saveMultiCategories(altIndex, attrIndex) {
        var value = "";
        $("input:checkbox.multi_edit_cb:checked").each(function() { value += (value == "" ? "" : ";") + this.getAttribute("data-itemid"); });
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&value=" + value + "&ids=" + getMarkedControlsList(), false);
    }
    // end  Attribute value changed functions

    // Optimizer Types Tabs
    function openOptimizerTab(tab_id) {
        sendCommand("action=optimizer_type&value=" + tab_id, false);
    }
    // end Optimizer Types Tabs

    /* Add Controls, Paste From Clipboard */
    var ctrl_id  = "";
    var ctrl_name  = "";
    var ctrl_descr = "";
    var ctrl_cost  = "";
    var ctrl_cat   = "";

    function resetVars() {
        ctrl_id  = "";
        ctrl_name  = "";
        ctrl_descr = "";
        ctrl_cost  = "";
        ctrl_cat   = "";
    }

    function getCtrlName(sName) {
        ctrl_name = sName;        
        dxDialogBtnDisable(true, (!validFloat(ctrl_cost)) || (ctrl_name.trim() == ""));
    }

    function getCtrlCost(sCost) {
        if (validFloat(sCost)) { ctrl_cost = sCost } else {ctrl_cost = "0" };
        dxDialogBtnDisable(true, (!validFloat(ctrl_cost)) || (ctrl_name.trim() == ""));
    }

    function getCtrlCategories() {        
        ctrl_cat = "";
        $('input:checkbox.cb_cat:checked').each(function() { ctrl_cat += (ctrl_cat == "" ? "" : ";") + this.value; });
    }

    function initEditorForm(_title, name, descr, cost, action, categories) {
        ctrl_name  = name;
        //ctrl_descr = descr;
        ctrl_cost  = cost;
        ctrl_cat   = categories;

        document.getElementById("tbControlName").value  = name;
        //document.getElementById("tbControlDescr").value = descr;
        document.getElementById("tbControlCost").value  = cost;

        var lb = document.getElementById("lbCategories");
        
        while (lb.firstChild) {
            lb.removeChild(lb.firstChild);
        }
                                
        var cat_data_len = cat_data.length;
        for (var i = 0; i < cat_data_len; i++) {
            createCategoryElement(lb, i, cat_data[i][IDX_CAT_ID], cat_data[i][IDX_CAT_NAME], categories);
        }

        cancelled = false;

        dlg_control_editor = $("#editorForm").dialog({
            autoOpen: false,
            modal: true,
            width: 420,
            dialogClass: "no-close",
            closeOnEscape: true,
            closeOnEnter: true,
            bgiframe: true,
            title: _title,
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: {
                Ok: { id: 'jDialog_btnOK', text: "OK", click: function() {
                    dlg_control_editor.dialog( "close" );
                }},
                Cancel: function() {
                    cancelled = true;
                    dlg_control_editor.dialog( "close" );
                }
            },
            open: function() {
                $("body").css("overflow", "hidden");
                document.getElementById("tbControlName").focus();
            },
            close: function() {
                $("body").css("overflow", "auto");                
                if ((!cancelled) && (ctrl_name.trim() != "")) {
                    //sendCommand('action='+action+'&control_id='+ctrl_id+'&name='+encodeURIComponent(ctrl_name)+'&descr='+encodeURIComponent(ctrl_descr)+'&cost='+ctrl_cost+'&cat='+ctrl_cat);
                    sendCommand('action='+action+'&control_id='+ctrl_id+'&name='+encodeURIComponent(ctrl_name)+'&cost='+ctrl_cost+'&cat='+ctrl_cat);
                }
            }
        });
    }

    function confirmRemoveControls() {
        var list = getMarkedControlsList();
        var msg = "Are you sure you want to delete this <%=ParseString("%%control%%") %>?";
        if (list.length > 50) msg = "Are you sure you want to delete these <%=ParseString("%%controls%%") %>?";
        dxConfirm(msg, "removeControlsClick();", ";");
    }

    function removeControlsClick() {
        sendCommand('action=remove_controls&ids=' + getMarkedControlsList(), true);
    }

    function addControlClick() {
        resetVars();
        initEditorForm("<% = ParseString("Add a %%control%%") %>", "","","0.00", "addcontrol", "");
        dlg_control_editor.dialog("open");
        dxDialogBtnDisable(true, false);
        if ((theForm.tbControlName)) theForm.tbControlName.focus();
    }

    function pasteControlsClick() {
        var data = "";
        if (window.clipboardData) { 
            data = window.clipboardData.getData('Text') 
            pasteData(data);
        } else { 
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteData);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>", "commitPasteChrome();", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }
        }  
    }

    function commitPasteChrome() {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) pasteData(pasteBox.value);
    }

    function pasteData(data) {
        if (data != undefined && data != "") {
            sendCommand("action=paste_controls&data=" + encodeURIComponent(htmlEscape(data)));
        } else { dxDialog("<%=ResString("msgUnableToPaste") %>", "", undefined, "<%=ResString("lblError") %>"); }
    }

    var tmp_id;
    var tmp_value;

    function setControlType(element, id, value) {        
        tmp_id = id;
        tmp_value = value;

        var curApplCount = 0;

        var list = getMarkedControlsList();        

        var ctrls = [];
        if (list == "") {
            ctrls.push(id);
        } else {
            ctrls = list.split(",");
        }

        for (var i = 0; i < ctrls.length; i ++ ) {
            if (ctrls[i].length > 32) {
                var ctrl = getControlByID(ctrls[i]);
                if ((ctrl)) {
                    curApplCount += ctrl[COL_CTRL_APPL_COUNT];
                }
            }
        }

        if (curApplCount > 0) {
            dxConfirm("Existing applications of " + (ctrls.length > 1 ? "<%=ParseString("these %%controls%%")%>" : "<%=ParseString("this %%control%%")%>") + " will be lost. Do you want to continue?", function () { $(element).attr("data-ctrl_type", value); onSetControlTypeContinue();}, function () { $(element).val($(element).attr("data-ctrl_type")*1); });
        } else {
            onSetControlTypeContinue();
        }
    }

    function onSetControlTypeContinue() {
        var list = getMarkedControlsList();        
        var ctrls = [];
        var id = tmp_id;
        var value = tmp_value;

        if (list == "") {
            ctrls.push(id);
        } else {
            ctrls = list.split(",");
        }
        for (var i = 0; i < ctrls.length; i ++ ) {
            if (ctrls[i].length > 32) {
                var ctrl = getControlByID(ctrls[i]);
                if ((ctrl)) {
                    ctrl[COL_CTRL_TYPE] = value;
                    ctrl[COL_CTRL_APPL_COUNT] = 0;
                    ctrl[COL_CTRL_IS_ACTIVE] = false;
                    ctrl[COL_CTRL_FUNDED] = false;
                }
            }
        }

        sendCommand("action=control_type&id=" + id + "&value=" + value + "&ids=" + list);
        refreshDataGrid(controls_data);
    }
    /* end Add Controls, Paste From Clipboard */

    /* Region "context menu for categories" */    
    function getCategoryByID(cat_id) {
        var cat_data_len = cat_data.length;
        for (var i=0; i<cat_data_len; i++) {
            if (cat_data[i][IDX_CAT_ID] == cat_id) return cat_data[i];
        }
    }

    function getCategoryByName(cat_name) {
        cat_name = cat_name.trim();
        var cat_data_len = cat_data.length;
        for (var i = 0; i < cat_data_len; i++) {
            if (cat_data[i][IDX_CAT_NAME] == cat_name) return cat_data[i];
        }
        return null;
    }

    function onContextMenuRenameCategoryClick(id) {
        hideCMenu();
        rename_cat_id = id;
        var cat = getCategoryByID(id);        
        if ((cat)) {
            new_cat_name = cat[IDX_CAT_NAME];
            jDialog_show_icon = false;
            var sCatNameEditor = "<input type='text' class='input' style='width:300px;' id='tbCategoryName' value='" + htmlEscape(new_cat_name) + "' onfocus='getCatName(this.value);' onkeyup='getCatName(this.value);' onblur='getCatName(this.value);'>";
            dxDialog(sCatNameEditor, "if (new_cat_name.trim().length>0) {renameCategory(new_cat_name.trim());} else {onRenameEmptyName();}", ";", "Edit category name");
            if ((theForm.tbCategoryName)) theForm.tbCategoryName.focus();
            jDialog_show_icon = true;
        }        
    }

    function getCatName(sName) {
        new_cat_name = sName;
        dxDialogBtnDisable(true, (sName.trim() == ""));
    }

    function renameCategory(newName) {        
        var cat = getCategoryByID(rename_cat_id);
        if ((cat) && (cat[IDX_CAT_NAME] != newName) && (newName.length > 0)) {
            sendCommand("action=rename_category&cat_id=" + rename_cat_id + "&cat_name=" + encodeURIComponent(newName));
            new_cat_name = ""
            cat[IDX_CAT_NAME] = newName;
            
            var lb = document.getElementById("lbCategories");        
            var lb_children_count = lb.childNodes.length;
            for (var j=0; j<lb_children_count; j++) {
                if (lb.childNodes[j].getAttribute("id") == rename_cat_id) {
                   var idx = lb.childNodes[j].getAttribute("i");
                   var lbl = document.getElementById('lbl_cat_'+idx);
                   if ((lbl)) { lbl.innerHTML = newName };
                   j = lb_children_count;
                }
            }
        }
    }

    function onRenameEmptyName() {
        dxDialog("Error: Empty category name!", "onContextMenuRenameCategoryClick(rename_cat_id);", undefined, "Error");
    }

    function onContextMenuRemoveCategoryClick(id) {
        hideCMenu();
        var cat_data_len = cat_data.length;
        for (var i=0; i<cat_data_len; i++) {
            if ((cat_data[i]) && (cat_data[i][IDX_CAT_ID] == id)) {
                cat_data.splice(i, 1);
                
                var lb = document.getElementById("lbCategories");        
                var lb_children_count = lb.childNodes.length;
                for (var j = 0; j < lb_children_count; j++) {
                    if (lb.childNodes[j].getAttribute("id") == id) {
                        lb.removeChild(lb.childNodes[j]);
                        j = lb_children_count;
                    }
                }

                sendCommand("action=delete_category&cat_id=" + id);
                i = cat_data_len;
            }
        }
    }

    function confrimDeleteCategory(id) {
        hideCMenu();
        dxConfirm("Are you sure you want to delete this category?", "onContextMenuRemoveCategoryClick('" + id + "');", ";", "Confirmation");
    }

    function onNewCategoryKeyUp(value) {
        var tOpacity = (value.trim() == "" ? 0.5 : 1);
        $("#btnAddCategory").fadeTo(0, tOpacity);
    }

    function AddCategory() {
        var tb = document.getElementById("tbNewCategory");
        if ((tb) && tb.value.trim() != "" && getCategoryByName(tb.value.trim()) == null) {
            new_cat_name = tb.value.trim();
            tb.value = "";
            sendCommand("action=add_category&name=" + encodeURIComponent(new_cat_name));
        }
    }

    function createCategoryElement(lb, i, cat_id, name, categories) {
        var opt = document.createElement("li");
                   
        var cat_name = ShortString(name, 40);
        
        opt.setAttribute("i", i + "");
        opt.setAttribute("id", cat_id + "");
        
        //opt.style.padding = "0px 0px 0px 0px";
        //opt.style.margin  = "0px 0px 0px 0px";
        //opt.style.lineHeight = "1px";

        opt.value = cat_id;
        var chk = (categories.indexOf(cat_id) >= 0 ? " checked " : " ");
        
        opt.innerHTML = "<table cellpadding='0' cellspacing='0' class='text' style='width:100%;'><tr><td align='left'><label><input type='checkbox' id='cb_cat_" + i + "' class='cb_cat' " + chk + " value='" + cat_id + "' onclick='getCtrlCategories();' ><span id='lbl_cat_" + i + "'>" + name + "</span></label></td><td align='right'><i class='fas fa-ellipsis-h' style='visibility:hidden;vertical-align:top;cursor:pointer;' id='mnu_cat_img_" + i + "' onclick='showMenuCat(event, \"" + cat_id + "\", \"" + decodeURIComponent(cat_name) + "\");'></i></td></tr></table>";
        opt.onmouseover = function() { document.getElementById("mnu_cat_img_"+this.getAttribute("i")).style.visibility = "visible"; };
        opt.onmouseout  = function() { document.getElementById("mnu_cat_img_"+this.getAttribute("i")).style.visibility = "hidden"; };
        lb.appendChild(opt);
        lb.disabled = false;
    }

    function showMenuCat(event, id, name) {        
        is_context_menu_open = false;
        var cat = getCategoryByID(id);
        if ((cat)) {               
            $("div.context-menu").hide().remove();
            var sMenu = "<div class='context-menu'>";
                sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameCategoryClick(\"" + id + "\"); return false;'><div><nobr><i class='fas fa-pencil-alt'></i>&nbsp;Edit&nbsp;&quot;" + htmlEscape(name) + "&quot;</nobr></div></a>";
                sMenu += "<a href='' class='context-menu-item' onclick='confrimDeleteCategory(\"" + id + "\"); return false;'><div><nobr>&nbsp;<i class='fas fa-trash-alt'></i>&nbsp;&nbsp;Delete&nbsp;&quot;" + htmlEscape(name) + "&quot;</nobr></div></a>";
                sMenu += "</div>";                
            var x = event.clientX;
            var y = event.clientY;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});
            $("body").css("overflow", "hidden");
            $("div.context-menu").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }
    }

    /* End Region "context menu for categories" */

    function setSolver(id) {
        if (solver_id != id * 1) {
            switch (id * 1) {
                case solver_id_xa:
                case solver_id_baron:
                    solver_id = id * 1;
                    sendCommand("action=solver&id=" + id, false);
                    break;
                case solver_id_gurobi:
                    dxConfirm("<% = JS_SafeString(ResString("msgRASwitchGurobi")) %>", "solver_id = solver_id_gurobi; sendCommand('action=solver&id=' + solver_id_gurobi, false);", "$('#cbSolver1').prop('checked', true);");
                    break;
            }        
        }
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

    function updateDollarValueUI(stringValue) {
        $("#lblDollarValue").html("<%=ResString("lblShowDollarValue")%>" + stringValue);
    }

    function ShowDollarValueClick(chk) {
        ShowDollarValue = chk;
        sendCommand("action=show_dollar_value&chk=" + (chk ? "1" : "0"));
    }

    function onDollarValueTargetChange(cb) {
        var val = cb.options[cb.selectedIndex].getAttribute("doll_value");
        var tb = document.getElementById("tbDollarValue");
        if ((tb)) tb.innerText = val == UNDEFINED_INTEGER_VALUE ? "" : val;
        sendCommand("action=selected_dollar_value_target&value=" + cb.value + "&doll_value=" + val);
    }

    function onEditDollarValueClick() {
        if ($("#divValueOfEnterpriseEditor").data("dxPopup")) {
            $("#divValueOfEnterpriseEditor").dxPopup("instance").dispose();
        }
        $("#divValueOfEnterpriseEditor").empty();

        var sValue = (DollarValue == UNDEFINED_INTEGER_VALUE ? "0" : DollarValue + "");
        var events_data_len  = events_data.length;
        var all_obj_data_len = all_obj_data.length;
        // init combobox
        var sCB = "";
        var sOpts ="";
        sOpts += "<option value='' " + (DollarValueTarget == "" ? " selected='selected' " : "") + "><% = JS_SafeString(ResString("lblEnterprise")) %></option>";
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
                sOpts += "<option value='" + events_data[i][0] + "' doll_value='" + Math.round(events_data[i][3]) + "' " + (DollarValueTarget == "" || events_data[i][0] != DollarValueTarget ? "" : " selected='selected' ") + ">" + events_data[i][1] + "</option>";
            }
        }
        sCB = "<select id='cbDollarValueTarget' style='width:180px; margin-bottom:-1px;' onchange='onDollarValueTargetChange(this);'>" + sOpts + "</select>";

        var sInfo = "<%=ResString("msgEnterDollarValue")%><br><br>";

        var msg = "<div><span>" + sInfo + "</span><center><span><% = JS_SafeString(ResString("lblValueOf")) %>&nbsp;" + sCB + "</span>&nbsp;=&nbsp;<input id='tbDollarValue' type='input text' style='text-align:right;' value='" + sValue + "' onkeydown='is_reset = false;'></center></div>";
        msg += "<br><center><div id='btnValueOfEntClose' style='width: 120px;'></div></center>";
        
        $("#divValueOfEnterpriseEditor").html(msg);
        
        $("#divValueOfEnterpriseEditor").dxPopup({
              width: "70%",
              height: "auto",
              title: "<% = JS_SafeString(ResString("titleEditUSD")) %>",
              visible: true
        });
        $("#btnValueOfEntClose").dxButton({
            text: "<%=ResString("btnOK")%>",
            icon: "fas fa-check",
            onClick: function() {
                onSaveDollarValue($('#tbDollarValue').val(), $('#cbDollarValueTarget').val());
                $("#divValueOfEnterpriseEditor").dxPopup("hide");
            }
        });

    }

    function onSaveDollarValue(value, target) {        
        if (typeof value != 'undefined' && validFloat(value)) {
            var dValue = str2double(value);            
            DollarValue = dValue;
            DollarValueTarget = target;
            initDollarValueUI(true);
            sendCommand("action=set_dollar_value&value=" + value + "&target=" + target);
        } else { is_reset = true }
        if (is_reset) {
            ShowDollarValue = false;
            DollarValue = UNDEFINED_INTEGER_VALUE;
            DollarValueTarget = "";
            sendCommand("action=set_dollar_value&value=" + UNDEFINED_INTEGER_VALUE + "&target=" + target);
        }
    }

    function onResetDollarValue() {
        is_reset = true;
        onSaveDollarValue(UNDEFINED_INTEGER_VALUE, "");
        $('#tbDollarValue').val(0);
        $("#btnResetDollarValue").prop("disabled", true);
        is_reset = false;
        return false;
    }   

    function setControlConstraint(chk, oper, ctrl_id) {
        var ctrl = getControlByID(ctrl_id);
        if ((ctrl)) {
            if (oper == "set_must") {
                ctrl[COL_CTRL_MUST] = chk;
                if (chk && ctrl[COL_CTRL_MUST_NOT]) {
                    ctrl[COL_CTRL_MUST_NOT] = !chk;
                    $("input[data-ctrl_id='" + ctrl_id + "'][data-cb_type='mustnot']").prop("checked", !chk);
                }
            }
            if (oper == "set_must_not") {
                ctrl[COL_CTRL_MUST_NOT] = chk;
                if (chk && ctrl[COL_CTRL_MUST]) {
                    ctrl[COL_CTRL_MUST] = !chk;
                    $("input[data-ctrl_id='" + ctrl_id + "'][data-cb_type='must']").prop("checked", !chk);
                }
            }
            if (oper == "set_disabled") {
                ctrl[COL_CTRL_DISABLED] = chk;                
                if (chk) {
                    ctrl[COL_CTRL_IS_ACTIVE] = false;
                    ctrl[COL_CTRL_FUNDED] = false;
                }
            }
        }
    }

    function setConstraint(e, chk, oper, ctrl_id) {
        var ids = ctrl_id;

        // process Shift + Click
        if ((e.shiftKey) && e.shiftKey && (last_click_cb != "") && (last_click_cb != ctrl_id) && last_click_cb_oper == oper) {
            var start_id = last_click_cb;
            var stop_id = ctrl_id;
            var is_checking = false;
            $("input.cb_" + oper).each(function() {
                var this_id = this.getAttribute("data-ctrl_id") + "";
                var is_found = this_id == start_id || this_id == stop_id;
                if (is_found && !is_checking) {
                    is_checking = true;
                } else {
                    if (is_found && is_checking) {
                        this.checked = chk;
                        setControlConstraint(chk, oper, this_id);
                        if (ids.indexOf(this_id) == -1) ids += (ids == "" ? "" : ",") + this_id;                       
                        is_checking = false;
                    }
                }
                if (is_checking) {
                    this.checked = chk;
                    setControlConstraint(chk, oper, this_id);
                    if (ids.indexOf(this_id) == -1) ids += (ids == "" ? "" : ",") + this_id;
                }
            });            
        } else {
            setControlConstraint(chk, oper, ctrl_id);
        }
        
        last_click_cb = ctrl_id;
        last_click_cb_oper = oper;
        
        sendCommand('action=' + oper + '&value=' + chk + '&ids=' + ids, true);
    }

    function onOptionClick(opt_id, chk) {
        switch (opt_id) {
            case "cbOptMusts":
                sendCommand('action=set_option&param=musts&value=' + chk, true);
                break;
            case "cbOptMustNots":
                sendCommand('action=set_option&param=mustnots&value=' + chk, true);
                break;
            case "cbOptDependencies":
                sendCommand('action=set_option&param=dependencies&value=' + chk, true);
                break;
            case "cbOptGroups":
                sendCommand('action=set_option&param=groups&value=' + chk, true);
                break;
            case "cbOptConstraints":
                sendCommand('action=set_option&param=constraints&value=' + chk, true);
                break;
            case "cbOptFundingPools":
                sendCommand('action=set_option&param=fundingpools&value=' + chk, true);
                break;
            case "cbOptTimePeriods":
                sendCommand('action=set_option&param=timeperiods&value=' + chk, true);
                break;
        }
    }

    function callSolve() {
        //var sBudget = $('#tbBudgetLimit').length ? number2locale($('#tbBudgetLimit').val(), true, false) : "";
        var sBudget = $('#tbBudgetLimit').length && $('#tbBudgetLimit').val().trim() != "" ? str2double($('#tbBudgetLimit').val()) : "";
        var sRisk   = $('#tbMaxRisk').length && $('#tbMaxRisk').val().trim() != "" ? (!ShowDollarValue ? $('#tbMaxRisk').val() : str2double($('#tbMaxRisk').val())) : "";
        var sReduct = $('#tbMinReduction').length && $('#tbMinReduction').val().trim() != "" ? (!ShowDollarValue ? $('#tbMinReduction').val() : str2double($('#tbMinReduction').val())) : "";
        sendCommand('action=solve&value=' + sBudget + '&risk=' + sRisk + '&reduction=' + sReduct, true);
    }

    function setSimulationMode(recalc) {
        var SimIn = $("#cbUseSimulatedInput").prop("checked") || $("#cbUseSimulated").prop("checked");
        var SimOut = $("#cbUseSimulatedOutput").prop("checked") || $("#cbUseSimulated").prop("checked");
        if (!SimIn && !SimOut) Use_Simulated_Values = 0;
        if (SimIn && !SimOut) Use_Simulated_Values = 1;
        if (!SimIn && SimOut) Use_Simulated_Values = 2;
        if (SimIn && SimOut) Use_Simulated_Values = 3;
        
        sendCommand("action=use_simulated_values&value=" + Use_Simulated_Values + "&recalc=" + recalc);
    }

    function evaluateControlEffectiveness(control_id, alt_id, node_id, appl_id) {
        loadURL("<%=JS_SafeString(PageURL(_PGID_EVALUATE_RISK_CONTROLS, "action=getstep&retpage=" + CurrentPageID.ToString + "&mt_type=-1")) %>&control_id=" + control_id + "&alt_id=" + alt_id + "&node_id=" + node_id); //  + '&appl_id=' + appl_id
    }

    function showControlsRiskReductionClick(chk) {
        is_init = true;
        ShowControlsRiskReduction = chk;
        $("#cbShowControlsRiskReduction").prop("checked", chk);
        sendCommand('action=show_individual_risk&value=' + chk, true);

        loadSAReductions();
    }

    /* S.A. Reductions - limited */
    var loaded_count = 0;
    var is_running = false;
    var cancelled = false;
    var please_wait_delay = 4000; //ms
    var load_n_at_a_time = 5;
    var cf; // callback function
    var progress_inc = 0; // simulate progress bar
    var cur_progress = 0;
    
    function cancelDialog() {
        cancelled = true;
        stopProgress();
        updateToolbar();
        DevExpress.ui.notify(resString("msgCancelled"), "error");
    }

    function startProgress() {
        cur_progress = 0;
        setProgress(progress_inc);
        $("#tdProgressBar").show();
        $("#btn_cancel").show();

        progress_inc = Math.ceil(100 / Math.ceil(controls_data.length / load_n_at_a_time));
        if (progress_inc <= 0) progress_inc = 10;
    }

    function stopProgress() {
        setProgress(100);
        $("#tdProgressBar").hide("slow");
        $("#btn_cancel").hide();
    }

    function setProgress(value) {
        cur_progress = value;
        if (cur_progress < 0) cur_progress = 0;
        if (cur_progress > 100) cur_progress = 100;
        var bar = document.getElementById("progress_bar");            
        bar.style.width = value + "%";            
    }

    function loadSAReductionsByPortions() {
        var ids = "";
        for (var i = loaded_count; (i < controls_data.length) && (i < loaded_count + load_n_at_a_time); i++) {
            ids += (ids === "" ? "" : ",") + controls_data[i].guid;
        }

        if (ids !== "") {
            is_running = true;
            callAPI("risk/?action=GetControlsRiskReduction".toLowerCase(), { "ids": ids }, function onGetControlsRiskReduction(data) {
                if (typeof data == "object") {
                    var dg = $("#tableContent").dxDataGrid("instance");
                    //var dataSource = dg.getDataSource();
                    dg.beginUpdate();
                    
                    if (!dg.columnOption("sa", "visible")) dg.columnOption("sa", "visible", true);
                    if (!dg.columnOption("sa_doll", "visible")) dg.columnOption("sa_doll", "visible", true);
                    
                    for (var i = 0; i < data.Data.length; i++) {
                        //var rowIndex = dg.getRowIndexByKey(data.Data[i].id);
                        //dg.cellValue(rowIndex, "sa", data.Data[i].sa);
                        //dg.cellValue(rowIndex, "sa_doll", data.Data[i].sa_doll);
                            
                        store.update(data.Data[i].id, { "sa": data.Data[i].sa });
                        store.update(data.Data[i].id, { "sa_doll": data.Data[i].sa_doll });

                        loaded_count += 1;
                    }
                        
                    is_running = false;
                        
                    var bar = document.getElementById("progress_bar");
                    if (bar.style.display !== "none" && cur_progress < 100) {
                        setProgress(Math.round(cur_progress + progress_inc));
                    }
                                               
                    if (loaded_count < controls_data.length && !cancelled) {
                        loadSAReductionsByPortions();
                    } else {
                        if (cancelled && typeof cf === "function") cf();
                        stopProgress();
                    }
                        
                    setTimeout(function () { dg.saveEditData(); }, 1500);
                    dg.endUpdate();

                    updateToolbar();
                }
            }, false, please_wait_delay);
        }
    }

    function loadSAReductions() {
        cancelled = false;
        startProgress();
        loadSAReductionsByPortions();
    }
    
    function onPageUnload(callback_func) {
        if (is_running) {
            cf = callback_func;
            cancelDialog();
        } else {
            if (typeof callback_func === "function") callback_func();
        }
    }

    $(window).on('beforeunload', function() {
        return onPageUnload();
    });

    logout_before = function(callback_func) {
        onPageUnload(callback_func);
    }

    /* S.A.R.R.*/

    function showRiskReductionOptionsClick(chk) {
        ShowRiskReductionOptions = chk;
        $("#cbShowRiskReductionOptions").prop("checked", chk);
        sendCommand('action=show_risk_reduction_options&value=' + chk, true);
    }

    function getIsControlMarked(ctrl_guid) {
        for (var i = 0; i < marked_controls_list.length; i++) {
            if (marked_controls_list[i] == ctrl_guid) {
                return true;
            }
        }
        return false;
    }

    function setIsControlMarked(ctrl_guid, chk) {
        // remove from the list
        for (var i = 0; i < marked_controls_list.length; i++) {
            if (marked_controls_list[i] == ctrl_guid) {
                marked_controls_list.splice(i, 1);
            }
        }
        // add to list if marked
        if (chk) marked_controls_list.push(ctrl_guid);
    }

    function markControl(e, id, chk) {
        var oper = "mark";
        var c = null;
        var ids = id;

        // process Shift + Click
        if ((e.shiftKey) && e.shiftKey && (last_click_cb != "") && (last_click_cb != id) && last_click_cb_oper == oper) {
            var start_id = last_click_cb;
            var stop_id = id;
            var is_checking = false;
            $("input.control_mark_cb").each(function() {
                var this_id = this.getAttribute("data-ctrl_id") + "";
                var is_found = this_id == start_id || this_id == stop_id;
                if (is_found && !is_checking) {
                    is_checking = true;
                } else {
                    if (is_found && is_checking) {
                        this.checked = chk;
                        setIsControlMarked(this_id, chk);
                        c = getControlByID(this_id);
                        if (ids.indexOf(this_id) == -1) ids += (ids == "" ? "" : ",") + this_id;                       
                        is_checking = false;
                    }
                }
                if (is_checking) {
                    this.checked = chk;
                    setIsControlMarked(this_id, chk);
                    if (ids.indexOf(this_id) == -1) ids += (ids == "" ? "" : ",") + this_id;
                }
            });            
        } else {
            setIsControlMarked(id, chk);
        }
        
        last_click_cb = id;
        last_click_cb_oper = oper;
        multiselectUI();
    }
    
    function multiselectUI() {
        var marked = getMarkedControlsList();
        
        // enable or disable control type comboboxes
        $("select.select_control_type").each(function() {
            var this_id = this.getAttribute("data-ctrl_id") + "";
            this.disabled = isReadOnly || (marked != "" && marked.indexOf(this_id) < 0);
        });   
        
        // enable or disable control cost inputs
        $("input.tb_cost").each(function() {
            var this_id = this.getAttribute("data-ctrl_id") + "";
            this.disabled = isReadOnly || (marked != "" && marked.indexOf(this_id) < 0);
        });

        // update Remove button disabled state
        $("#btnRemoveControls").prop("disabled", isReadOnly || marked == "");

        // update the Mark All checkbox state (On/Off/Indeterminate)
        var cbMarkAll = document.getElementById("cbMarkAll");
        if ((cbMarkAll)) {
            $(cbMarkAll).prop('indeterminate', false);
            var cbCount = $("input.control_mark_cb").length;
            cbMarkAll.disabled = cbCount == 0;
            if (cbCount == 0) {
                cbMarkAll.checked = false;
            } else {
                var checkedCount = $("input.control_mark_cb:checked").length;
                if (checkedCount == 0) cbMarkAll.checked = false;
                if (checkedCount > 0 && checkedCount == cbCount) cbMarkAll.checked = true;
                if (checkedCount > 0 && checkedCount < cbCount) $(cbMarkAll).prop('indeterminate', true);
            }
        }
    }

    function markAllControls(e, chk) {       
        if (!chk) marked_controls_list = [];
        $("input.control_mark_cb").each(function() {
            this.checked = chk;
            if (chk) marked_controls_list.push(this.getAttribute("data-ctrl_id") + "");
        });
        multiselectUI();
    }

    function getMarkedControlsList() {       
        //var retVal = "";
        //$("input.control_mark_cb").each(function() {
        //    if (this.checked) {
        //        var cid = this.getAttribute("data-ctrl_id");
        //        if (retVal.indexOf(cid) === -1) retVal += (retVal == "" ? "" : ",") + cid;
        //    }
        //});
        //return retVal;
        return marked_controls_list.join(",");
    }

    function saveBudget() {
        var value = "";
        var sval = str2cost($('#tbBudgetLimit').val());
        if (sval !== 'undefined') value = sval;
        sendCommand('action=edit_budget&value=' + value, false)
    }

    function initScenarios() {
          activeScenarioUI("divActiveScenario", <%=PM.ResourceAlignerRisk.Scenarios.ActiveScenarioID%>, "<%=JS_SafeString(PM.ResourceAlignerRisk.Scenarios.ActiveScenario.Name)%>", "<%=JS_SafeString(PM.ResourceAlignerRisk.Scenarios.ActiveScenario.Description)%>")
    }

    function CopyScenario(idx_dest, id_src) {
        dlg_scenarios.dialog('close');
        var msg = replaceString('{0}', scenarios[idx_dest][1], '<% = JS_SafeString(ResString("confRAonScenarioCopied")) %>');
        on_ok_code = "dxConfirm('" + msg + "', 'onSetScenario(" + idx_dest + ");', ';');";
        sendCommand("action=copy_scenario&from=" + id_src + "&to=" + scenarios[idx_dest][0], 0);
    } 
  
    function SaveScenario(index, idx) {
        for (var i=0; i<scenarios.length; i++) {
            if (scenarios[i][0] == idx) {
                if (index>=0) initScenarios();
                on_ok_code = (index<0 ? "dlg_scenarios.dialog('close'); showLoadingPanel(); onSetScenario(" + idx + ");" :  "document.getElementById('tbName').focus();");
                sendCommand("action=edit_scenario&id=" + (index>=0 ? idx : "-1&copy=" + ((scen_add_copy) ? "1" : "0")) + "&name=" +  encodeURIComponent(scenarios[i][1]) + "&desc=" + encodeURIComponent(scenarios[i][2]), 0);
            }
        }
    }

    function DeleteScenario(id) {
        dxConfirm("<% = JS_SafeString(ResString("confRADeleteScenario")) %>", "on_ok_code = 'onDeleteScenario(" + id + ");'; sendCommand('action=delete_scenario&id=" + id + "',0);", ";");
    }

    function onDeleteScenario(id) {
        var l = [];
        for (var i=0; i<scenarios.length; i++) {
            if (scenarios[i][0]!=id) l.push(scenarios[i]);
        }
        scenarios = l;
        initScenarios();
        if (id==<% =RA.Scenarios.ActiveScenarioID %>) { dlg_scenarios.dialog('close'); document.location.href='<% =PageURL(CurrentPageID) %>?<% =GetTempThemeURI(false) %>'; }
    }    

    $(document).ready(function () {        
        $("#divMainContent").css("overflow", "hidden");
        $(".mainContentContainer").css({ "overflow" : "hidden"});         

        showLoadingPanel();
        setTimeout('initPage();', 100);
        updateToolbar();
        multiselectUI();
        InitDragDrop();
        initScenarios();
        <%If IsOptimizationAllowed Then%>
        initOptimizerTabs();
        $('#tbBudgetLimit').on('blur',function(e){
            setTimeout("saveBudget();", 500);
        });
        initSimulatedIndicators(<%=Bool2JS(Use_Simulated_Values <> SimulatedValuesUsageMode.Computed)%>);
        <%End If%>

        <%If CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES Or CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then%>
        initTreeList();
        <%End If%>

        $("#tooltip").dxTooltip({
            target: "#btnPasteControls",
            showEvent: "dxhoverstart",
            hideEvent: "dxhoverend"
        });        
        //initPinOptions();
        $("#divOptimizerTabs").find(".dx-tabpanel-tabs").toggle(ShowRiskReductionOptions);
        $("#divOptimizerTabs").find(".dx-multiview-wrapper").css({"border": 0 /*ShowRiskReductionOptions ? "1px solid #6699cc;" : 0*/});
        resizePage();

    });

    function initOptimizerTabs() {        
        $("#divOptimizerTabs").dxTabPanel({
            //height: 60,
            width: 340,
            //height: 62,
            items: [
                {id: 0, hint:'<%=ResString("msgBudgetLimitHint")%>', title:"<%=ResString("optROBudgetLimit")%>", icon: "fas fa-hand-holding-usd", template: $("#optimizerTab0") },
                {id: 1, hint:'<%=ResString("msgMaxRiskHint")%>', title:"<%=ResString("optROMaxRisk")%>", icon: "fas fa-bolt", template: $("#optimizerTab1") },
                {id: 2, hint:'<%=ResString("msgMinReductionHint")%>', title:"<%=ResString("optROMinReduction")%>", icon: "fas fa-bolt", template: $("#optimizerTab2") }
            ],
            onTitleRendered: function (e) {  
               e.itemElement.find(".dx-tab-text").attr("title", e.itemData.hint);  
            }, 
            selectedIndex: <% = CInt(OptimizationTypeAttribute)%>,
            swipeEnabled: false,
            onSelectionChanged: function(e) {
                if (e.addedItems.length > 0) {
                    var tab_id = e.addedItems[0].id;
                    openOptimizerTab(tab_id);
                }
            }
        });
    }

    //function togglePinOptions() {
    //    optionsVisible = !optionsVisible;
    //    initPinOptions();
    //    sendCommand('action=show_options&val=' + optionsVisible);
    //    setTimeout("resizePage(true);", 800);
    //}

    //function initPinOptions() {
        //$("#trIgnores").toggle(!$(document.body).hasClass('fullscreen'));
        //if (!$(document.body).hasClass('fullscreen')) {
        //    $("#trIgnores").slideDown(400);
        //} else {
        //    $("#trIgnores").slideUp(400);
        //}
    //}

    var checkedNum = 0;
    var data_len = 0;
    function getCheckedObjsCount(data) {
        if ((data)) {
            for (var k = 0; k < data.length; k++) {
                if (data[k].checked) {
                    checkedNum += 1;
                }
                data_len += 1;
                getCheckedObjsCount(data[k].children);
            }
        }
    }

    /* Tree View */
    function initTreeList() {
        <% If CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then %>
        var storageKey = "EfficientFrontier_TreeList_<%=PRJ.ID%>";
        var hasState = typeof localStorage.getItem(storageKey) !== "undefined" && localStorage.getItem(storageKey) != null;

        //init columns headers                
        var columns = [];
        for (var i = 0; i < hierarchy_columns.length; i++) {
            var clmn = { "caption" : hierarchy_columns[i][1], "dataField" : hierarchy_columns[i][4], "alignment" : "left", "allowSorting" : true, "allowSearch" : true, "allowEditing" : false, "encodeHtml" : hierarchy_columns[i][4] == "name" || hierarchy_columns[i][4] == "info" }
            columns.push(clmn);
            if (!hasState) {
                clmn.visible = hierarchy_columns[i][2];
            }
        }     

        var isLoadingTreeListSessionSettings = true;

        $("#divHierarchy").dxTreeList({
            allowColumnResizing: true,
            allowColumnReordering: true,
            autoExpandAll: true,
            dataSource: hierarchy_data,
            columns: columns,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            columnChooser: {
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: true
            },
            columnFixing: {
                enabled: true
            },
            hoverStateEnabled: true,
            focusedRowEnabled: true,
            focusedStateEnabled: true,
            focusedRowKey: selectedHierarchyNodeID,
            onFocusedRowChanged: function (e) {
                if ((e.row) && (selectedHierarchyNodeID !== e.row.key) && !isLoadingTreeListSessionSettings) {
                    selectedHierarchyNodeID = e.row.key;
                    sendCommand("<% =_PARAM_ACTION %>=selected_node&value=" + e.row.key, true);
                }
            },
            keyExpr: "guid",
            parentIdExpr: "pguid",
            rootValue: "",
            //onCellPrepared: function(e) {
            //    if (e.rowType === "header" && e.column.dataField !== "id" && e.column.dataField !== "name" && e.column.dataField !== "info") {
            //        e.cellElement.on("click", function(args) {
            //            var sortOrder = e.column.sortOrder;
            //            if (!e.column.type && sortOrder == undefined) {
            //                e.component.columnOption(e.column.index, "sortOrder", "desc");
            //                args.preventDefault();
            //                args.stopPropagation();
            //            }
            //        });
            //    }
            //},
            onContentReady: function (e) {
                $(e.element).find(".dx-toolbar").css("background-color", "transparent");
                setTimeout(function () {
                    isLoadingTreeListSessionSettings = false;
                    e.component.option("focusedRowKey", selectedHierarchyNodeID);
                }, 10);
            },
            onToolbarPreparing: function(e) {
                
            },
            paging: {
                enabled: false
            },
            rowAlternationEnabled: true,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            searchPanel: {
                visible: true,
                //width: 240,
                placeholder: "<%=ResString("btnDoSearch")%>..."
            },
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: storageKey
            },
            sorting: {
                mode: 'multiple'
            },
            noDataText: "<% = GetEmptyMessage() %>",
            "export": {
                "enabled" : true
            },
            wordWrapEnabled: false                        
        });
        
        <%End If%>
    }
    /* end Tree View */

    /* Time Periods */
    var projects = <% = GetRAProjects() %>;
    var fundedonly = <% = Bool2JS(RA_ShowFundedOnly) %>;
    var cents =  <% = Cents %>;

    function setShowTimeperiods(chk) {
        sendCommand("action=show_timeperiods&value=" + chk);
    }

    function resizeTimeperiods() {
        $("#RATimeline").height(100).height($("#divContent").height()).width(300).width($("#divContent").width()-2);
    }

    function isByPages() {
        return pagination > 0;
    }

    function pageSize() {
        return pagination;
    }

    var cur_page = 0;

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
        showLoadingPanel(false);
    }

    function updateTimeperiods(dataSource) {
        $("#RATimeline").timeline("option", "solvedResults", dataSource);
        $("#RATimeline").timeline("updateSolvedResults");
        resizeTimeperiods();
        $("#RATimeline").timeline("resize");
    }

    function setPage(pg) {
        cur_page = pg-1;
        sendCommand("action=setpage&pg=" + cur_page);
        //if (view_grid) {
        //   if ($("#RATimeline").timeline() !== "undefined") $("#RATimeline").timeline("option", "currentPage", cur_page + 1);
        //} else {
        //    cur_page = pg;
        //}
    }

    /* end Time Periods */

    function updateToolbar() {
        <%If IsOptimizationAllowed Then%>
        // update the Select Events icon
        var lbl = document.getElementById("lblEventsFiltered");        
        if ((lbl)) {
            var event_list_len = event_list.length;
            var checkedEvents = 0;
            for (var k = 0; k < event_list_len; k++) {
                if (event_list[k][IDX_EVENT_ENABLED] == 1) checkedEvents += 1;
            }
            lbl.innerText = checkedEvents == event_list_len ? "" : " (*)";
        }
        // update the Available Controls icon
        lbl = document.getElementById("lblControlsFiltered");
        if ((lbl)) {
            var controls_data_len = controls_data.length;
            var checkedControls = 0;
            for (var k = 0; k < controls_data_len; k++) {
                if (controls_data[k][COL_CTRL_IS_ACTIVE] || controls_data[k][COL_CTRL_TYPE] == ctUndefined) checkedControls += 1;
            }
            lbl.innerText = checkedControls == controls_data_len ? "" : " (*)";
        }
        // update the Select Sources icon
        lbl = document.getElementById("lblSourcesFiltered");
        if ((lbl)) {
            checkedNum = 0;
            data_len = 0;
            getCheckedObjsCount(sources_list);
            lbl.innerText = checkedNum == 1 || checkedNum == data_len ? "" : " (*)";
        }
        // update the Select Impacts icon
        lbl = document.getElementById("lblObjsFiltered");
        if ((lbl)) {
            checkedNum = 0;
            data_len = 0;
            getCheckedObjsCount(impacts_list);
            lbl.innerText = checkedNum == 1 || checkedNum == data_len ? "" : " (*)";
        }
        <%End If%>

        var btn;
        
        //btn = document.getElementById("btnEditAttributes");
        //if ((btn)) btn.disabled = controls_data.length == 0;

        //SARR
        var btn_cancel = $("#btn_cancel");
        if ((btn_cancel) && btn_cancel.length) {
            btn_cancel.prop("disabled", controls_data.length == loaded_count || cancelled);
        }            

        resizePage();
    }

    function selectXML(vis) {
        if ((vis)) {
            $(".overlay").show();
            $("#divUpload").show();
            $("#tblMain").prop("disabled", "disabled");
            setTimeout('theForm.btnUpload.disabled=0; theForm.btnUpload.focus(); if (theForm.<% =FileUpload.ClientID %>.value=="") theForm.btnUpload.disabled=1;',150);
        } else {
            $(".overlay").hide();
            $("#divUpload").hide();
            $("#tblMain").prop("disabled", "");
        }
    }

    function uploadXML() {
        if (theForm.<% =FileUpload.ClientID %>.value!="") {
            theForm.btnUpload.disabled = 1;
            theForm.submit();
        }
    }

    function msgWrongFile() {
        dxDialog("<% =JS_SafeString(ResString("msgWrongXMLNISTFile")) %>", ";");
    }

    function hotkeys(event) {
        if (!document.getElementById) return;
        if (window.event) event = window.event;
        if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            switch (code) {
                case KEYCODE_PERIOD:
                    if (event.ctrlKey) {
                        onToolbarDblClick();
                    }
                    break;
                case KEYCODE_ENTER:
                    if ($("#editorForm").dialog("isOpen")) $("#editorForm").dialog("close");
                    break;
            }
        }
    }

    document.onkeydown = hotkeys;
    document.onclick = function () { if (is_context_menu_open == true) { $("div.context-menu,div.options-menu").hide(200); hideCMenu(); } };
    document.onkeypress = Hotkeys;
    resize_custom = resizePage;

</script>

<div id="divUpload" class="text" style="z-index:9999; width:400px; position:absolute; left:266px; top:39px; border:1px solid #999999; background:#fafaff; text-align:left; padding:1em 2em 2em 2em; display:none;">
    <h6 style="text-align:left; padding-bottom:12px;"><% =ResString("lblUploadFile")%></h6>
    <nobr><%=ResString("lblSelectXMLUpload")%><br /><asp:FileUpload ID="FileUpload" runat="server" Width="100%" CssClass="input"/></nobr>
    <br /><br /><label><input type="checkbox" class='checkbox' <%=If(LoadAllControls, " checked='checked' ", "")%> onclick="sendCommand('action=load_all_controls&value=' + this.checked, false);" /><%=ParseString("Load %%control%% enhancements")%></label>
    <br /><br /><input type="button" class="button" name="btnUpload" value="<% =ResString("btnUpload")%>" onclick="uploadXML();" /> <input type="button" class="button" name="btnCancel" value="<% =ResString("btnCancel")%>" onclick="selectXML(false);" />
</div>

<%-- Page content --%>

<div class='overlay' style="display:none"></div>

<table id='tblMain' class='whole' style='overflow:hidden;padding-left:1px;' border='0' cellspacing='0' cellpadding='0'>    
    <tr valign="top"><td colspan="2">
        <div id='divToolbar' class='text ec_toolbar' style="margin: 0px 0px 6px 0px; padding: 2px;">
            <table class="text" style="text-align: left;">
                <tr valign="middle">
                    <td class="ec_toolbar_td_separator text" valign="middle" style="vertical-align: middle;">
                        <%If IsOptimizationAllowed AndAlso Not IsEditable AndAlso CurrentPageID <> _PGID_RISK_SELECT_TREATMENTS Then %>
                        <button id="btnOptimize" onclick="this.disabled = true; callSolve(); return false;" class='button' type="button"><img src="<% =ImagePath %>assembly-16.png" width=16 height=16 />&nbsp;<%=ParseString("Optimize")%></button>
                        <%End If%>
                        <%If Not IsEditable Then%>
                        <div style="vertical-align: middle; display: inline-block; margin-top: 6px;">
                            <label><input type='checkbox' id='cbUseSimulated' <% = If(PM.CalculationsManager.UseSimulatedValues, " checked='checked' ", "") %>  onclick='setSimulationMode(true);' />Simulated</label>
                        </div>
                        <%End If%>
                        <%If False AndAlso IsOptimizationAllowed AndAlso Not IsEditable AndAlso CurrentPageID <> _PGID_RISK_SELECT_TREATMENTS AndAlso CurrentPageID <> _PGID_RISK_OPTIMIZER_TIME_PERIODS_VIEW Then %>
                        <label><input type='checkbox' id='cbShowTimeperiods' <% = If(RA_ShowTimeperiods, " checked='checked' ", "") %> <% = If(RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count = 0, " disabled='disabled' ", "") %> onclick='setShowTimeperiods(this.checked);' />Time Periods</label>
                        <%End If%>
<%--                    </td>
                    <td>--%>                        
                    <%--</td>
                    <td class="ec_toolbar_td_separator text" valign="middle"> --%>                       
                        <%If IsEditable Then%>
                        <button type='button' class='button' <%=If(CanUserAddControls, "", " disabled='disabled' ")%> onclick='addControlClick(); return false;'><%=ResString("btnAddControl")%>…</button>
                        <button type='button' class='button' <%=If(CanUserAddControls, "", " disabled='disabled' ")%> onclick='pasteControlsClick(); return false;' id='btnPasteControls'><% = ResString("btnPasteControlsFromClipboard") %></button>
                        <button type='button' class='button' <%=If(CanUserAddControls, "", " disabled='disabled' ")%> onclick='selectXML(true); return false;' title='<%=SafeFormString(ResString("msgUploadControlsHint"))%>'><% = ResString("btnUploadNISTControls") %>…</button>
                        <button type='button' class='button' disabled='disabled' id="btnRemoveControls" onclick='confirmRemoveControls(); return false;'><% = ResString("btnDelete") %></button>
                        <%End If%>
                        <%If IsOptimizationAllowed AndAlso Not IsEditable AndAlso CurrentPageID <> _PGID_RISK_SELECT_TREATMENTS Then %>
                        <%--<label><input type='checkbox' class='checkbox' id='cbUseSimulatedValues' <%=If(PM.Parameters.Riskion_Use_Simulated_Values, " checked", "") %> onclick='sendCommand("action=use_simulated_values&chk=" + this.checked);' />Simulated</label>--%>
                        <%--<input type="button" id="btnOptimize" onclick="sendCommand('action=solve&budget='+$('#tbBudgetLimit').val() + '&risk='+$('#tbMaxRisk').val() + '&reduction='+$('#tbMinReduction').val()); return false;" class='button' style="width:205px;" value="<%=ParseString("Optimize %%controls%% (Solve)")%>" />                                               --%>
                        <% If _OPT_ALLOW_CHOSE_SOLVER AndAlso App.isRASolversAvailable(True) Then%><nobr>&nbsp;<%--<img src="<% =ImagePath %>settings-16.png" width="16" height="16" style="margin-right:4px" />--%><b><% =ResString("lblRASolver") %>:</b>
                        <input type='radio' id='cbSolver1' name='cbSolver' value="<% =CInt(raSolverLibrary.raXA)  %>" <%=If(RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raXA, " checked='checked' ", "")%> onclick='setSolver(this.value);'/><label for='cbSolver1'><% =ResString("lblRASolverXA")%></label>
                        <% If App.isGurobiAvailable Then%><input type='radio' id='cbSolver2' name='cbSolver' value="<% =CInt(raSolverLibrary.raGurobi)%>" <%=If(RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raGurobi, " checked='checked' ", "")%> onclick='setSolver(this.value);'/><label for='cbSolver2'><% =ResString("lblRASolverGurobi")%></label><% End if %>
                        <% If App.isBaronAvailable Then%><input type='radio' id='cbSolver3' name='cbSolver' value="<% =CInt(raSolverLibrary.raBaron)%>" <%=If(RA.RiskOptimizer.SolverLibrary = raSolverLibrary.raBaron, " checked='checked' ", "")%> onclick='setSolver(this.value);'/><label for='cbSolver2'><% =ResString("lblRASolverBaron")%></label><% End If %>
                        &nbsp;</nobr> <% End If%>
                        <button id="btnSelEvents" onclick="onSelectObjectsClick(event_list, false, 0, 'select_events'); return false;" class='button' type="button" <%=If(PM.AltsHierarchy(PM.ActiveAltsHierarchy).TerminalNodes.Count > 0, "" , "disabled")%>><%=ParseString("Filter %%Alternatives%%")%><span id="lblEventsFiltered"></span></button>
                        <%--<span class="text" style="display: inline-block; margin-top: 8px;">Available:&nbsp;</span>
                        <button id="btnSelControls" onclick="onSelectControlsClick(); return false;" class='button' type='button' style="width:90px;" <%=If(PM.Controls.Controls.Count > 0, "" , "disabled")%>><%=ParseString("%%Controls%%")%><span id="lblControlsFiltered"></span></button>
                        <button id="btnSelSources" onclick="onSelectObjectsClick(sources_list, true, 0, 'select_sources'); return false;" class='button' type="button" style="width:80px;" <%=If(PM.Hierarchy(ECHierarchyID.hidLikelihood).Nodes.Count > 0, "" , "disabled")%>><%=ParseString("%%Sources%%")%><span id="lblSourcesFiltered"></span></button>
                        <button id="btnSelImpacts" onclick="onSelectObjectsClick(impacts_list, true, 2, 'select_impacts'); return false;" class='button' type="button" style="width:80px;" <%=If(PM.Hierarchy(ECHierarchyID.hidImpact).Nodes.Count > 0, "" , "disabled")%>><%=ParseString("%%Consequences%%")%><span id="lblObjsFiltered"></span></button>--%>
                    <%--</td>
                    <td class="ec_toolbar_td text" valign="middle">--%>
                        <% If _OPT_ALLOW_CHOSE_SOLVER Then%>
                        <%--<button id="btnExportXA" onclick="onExportLogsClick() return false;" class="ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only" type='button' style="width:150px;"><img src="<% =ImagePath %>export_xa_16.png" width=16 height=16 style="vertical-align:middle; padding-bottom:0px;" /> <%=ResString("lblRAExportXALogs")%></button><% End If%>--%>
                        <% If Not RA_ShowTimeperiods Then %>
                        <input id="btnExportXA" onclick="onExportLogsClick() return false;" class="button" type='button' style="width:150px;" value="<%=ResString("lblRAExportXALogs")%>" /><% End If%>
                        <% End If %>
                        <button id="btnDownload" onclick="onDownloadBar(); return false;" class='button' type='button'><%=ResString("lblRADownloadBar")%></button>
                        <%End If %>
                        <% If Not App.IsActiveProjectReadOnly Then %> 
                        <%--<button id="btnEditAttributes" onclick="manageAttributes(); return false;" class='button' type="button" style="width:150px;" <%=If(PM.Controls.Controls.Count = 0, " disabled='disabled' ", "")%>><img src="<% =ImagePath %>config-20.png" width="16" height="16" />&nbsp;<%=ParseString("Edit Attributes")%></button>--%>
                        <button id="btnEditAttributes" onclick="manageAttributes(); return false;" class='button' type="button" <%=If(App.IsActiveProjectReadOnly, " disabled='disabled' ", "")%>><%=ParseString("Edit Attributes")%>…</button>
                        <% End If %>
                        <button id="btnOptions" onclick="onOptions_Menu(this); return false;" class='button' type="button"><%=ResString("titleOptionsPnl")%>&nbsp;&#x25BE;</button>
                        <%--<%If ShowDraftPages() Then %>
                        <button id="btnRandomizeControls" <%=If(PM.Controls.Controls.Count = 0, " disabled='disabled' ", "")%> onclick="onConfirmRandomizeControls(); return false;" class='button' type="button" style="width:100px; color: #cc1b00;" title="<%=ParseString("Generate random %%control%% assignments and assignment effectivenesses")%>""><img src="<% =ImagePath %>dices_blue_20.png" width=16 height=16 />&nbsp;Randomize&nbsp;</button>
                        <%End If %>--%>
                        <%--<nobr><span>Optimization Function Type:</span>--%>
                        <select style="display: none;" class="select" id='cbObjectiveFunctionType' onclick="if (this.value != objectiveFunctionType) { objectiveFunctionType = this.value; sendCommand('action=objective_function_type&value='+this.value); }">
                            <option value="0" <%=If(PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType = ObjectiveFunctionType.SumProduct, " selected='selected'", "") %>>Sum Product</option>
                            <option value="1" <%=If(PM.ResourceAlignerRisk.Solver.ObjectiveFunctionType = ObjectiveFunctionType.Combinatorial, " selected='selected'", "") %>>Combinatorial</option>
                        </select>
                        <%--</nobr>--%>
                        <%--<div style="text-align: right; display: inline-block;">
                            <a href='' style='cursor: pointer; background-color: white; width: 30px; border: 1px solid #ccc; padding: 5px; border-radius: 2px;' onclick='togglePinOptions(); return false;'><i class="fas fa-sliders-h fa-lg"></i></a>
                        </div>--%>
                        <button id="btn_cancel" onclick="cancelDialog(); return false;" class='button' type="button" style="display: none;"><i class="fas fa-ban error"></i><% = ResString("btnCancel") %></button>
                    </td>
                </tr>
            </table>
        </div>
    </td></tr>
    <tr id="trProgressBar" valign="top" style="height: 8px;">
        <td id="tdProgressBar" style="display: none; vertical-align: top; padding: 2px 0px;" valign="top">
            <div id="progress_control" style="overflow: hidden; background-color: #d0d0d0; height: 4px; width: 100%; text-align: left;">
                <div id="progress_bar" style="background-color: #8899cc; height: 4px; width: 100%; margin: 0px;"></div>
            </div>
        </td>
    </tr>
    <tr>        
        <td valign="top" colspan="2">
            <h5 id='lblPageTitle' style='margin:0px 6px 0px 6px; padding-bottom: 2px;'><%=GetTitle()%><div id="divActiveScenario" style="display: inline-block;"></div></h5>            
            <%--<a href='' style='cursor: pointer; position: absolute; right: 6px; margin-top: -25px;' onclick='togglePinOptions(); return false;'><img id="imgOptions" src='<% =ImagePath %>options_v-20.png' width='20' height='20' alt='' /></a>--%>
        </td>
    </tr>
    <tr id="trIgnores">
        <td align="center" colspan="2" valign="top">        
        <div id='divSolverResults' class='small' style="margin: 0px 0px 2px 0px;">
            <table cellpadding="0" cellspacing="0">
                <tr>
                    <td class="td_risk_value"><nobr><%If IsOptimizationAllowed AndAlso Not IsEditable Then %><span style="display:inline-block;width:210px;text-align:right;" class="simulated_results_label"><b><%=ResString("lblRiskTotal")%><span class="simulated_results_indicator" style="display: none; color: blue;">*</span>:&nbsp;</b></span><%End If %></nobr></td>
                    <td class="td_risk_value"><nobr><%If IsOptimizationAllowed AndAlso Not IsEditable Then %><span class="number_result" id="tbRiskTotal" style="white-space: nowrap;"><%=If(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * PM.DollarValueOfEnterprise, CostDecimalDigits, True), (RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * 100).ToString("F2") + "%")%></span><%End If %></nobr></td>
                    <td class="td_cost_value"><span id="lblActiveCountLabel" style="display:inline-block;width:210px;text-align:right;"><b>Selected <% =ParseString("%%controls%%") %>:&nbsp;</b></span></td>                    
                    <td class="td_cost_value"><span class="number_result" id="lblActiveCount" style="white-space: nowrap;"><%=RA.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCount%></span></td>
                </tr>
                <tr>
                    <td class="td_risk_value"><nobr><%If IsOptimizationAllowed AndAlso Not IsEditable Then %><span style="display:inline-block;width:210px;text-align:right;" class="simulated_results_label"><b><%=ParseString("%%Risk%% With Selected %%Controls%%")%><span class="simulated_results_indicator" style="display: none; color: blue;">*</span>:&nbsp;</b></span><%End If %></nobr></td>
                    <td class="td_risk_value"><nobr><%If IsOptimizationAllowed AndAlso Not IsEditable Then %><span class="number_result" id="tbOptimizedRisk" style="white-space: nowrap;"><%=getInitialOptimizedRisk()%></span><%End If %></nobr></td>                    
                    <td class="td_cost_value"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("Cost Of Selected %%Controls%%")%>:&nbsp;</b></span></nobr></td>
                    <td class="td_cost_value"><nobr><span class="number_result" id="tbFundedCost" style="white-space: nowrap;"><%=CostString(RA.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True) + DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, RA.RiskOptimizer.CurrentScenario.OriginalSelectedControlsCost, CostDecimalDigits, True, ResString("lblUnfunded") + ":")%></span></nobr></td>                    
                </tr>
                <tr>
                    <td class="td_risk_value"><nobr><%If IsOptimizationAllowed AndAlso Not IsEditable Then %><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("%%Risk%% With All %%Controls%%")%>:&nbsp;</b></span><%End If %></nobr></td>
                    <td class="td_risk_value"><nobr><%If IsOptimizationAllowed AndAlso Not IsEditable Then %><span class="number_result" id="tbRiskTotalWithControls" style="white-space: nowrap;"><%=If(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * PM.DollarValueOfEnterprise, CostDecimalDigits, True) + DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue * PM.DollarValueOfEnterprise, RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * PM.DollarValueOfEnterprise, CostDecimalDigits, ShowDollarValue), (RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls * 100).ToString("F2") + "%" + DeltaString(RA.RiskOptimizer.CurrentScenario.OriginalRiskValue, RA.RiskOptimizer.CurrentScenario.OriginalRiskValueWithControls, CostDecimalDigits, ShowDollarValue))%></span><%End If %></nobr></td>
                    <td class="td_cost_value"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ParseString("Total Cost Of All %%Controls%%")%>:&nbsp;</b></span></nobr></td>
                    <td class="td_cost_value"><nobr><span class="number_result" id="tbAllControlsCost" style="white-space: nowrap;"><%=CostString(RA.RiskOptimizer.CurrentScenario.OriginalAllControlsCost, CostDecimalDigits, True) + " (with applications: " + CostString(RA.RiskOptimizer.CurrentScenario.OriginalAllControlsWithAssignmentsCost, CostDecimalDigits, True) + ")"%></span></nobr></td>
                </tr>
                <%If IsOptimizationAllowed AndAlso Not IsEditable AndAlso False Then %>
                <tr> <!-- Risk Reduction Row -->
                    <td class="td_risk_value"><nobr><span style="display:inline-block;width:210px;text-align:right;"><b><%=ResString("lblRiskReduction")%>:&nbsp;</b></span></nobr></td>
                    <td class="td_risk_value"><nobr><span class="number_result" id="tbTotalRiskReduction" style="white-space: nowrap;"><%=If(ShowDollarValue, CostString(GetCurRiskReduction(True), CostDecimalDigits, True), (GetCurRiskReduction(False) * 100).ToString("F2") + "%")%></span></nobr></td>
                    <td class="td_cost_value"></td>
                    <td class="td_cost_value"></td>
                </tr>
                <tr>
                    <td class="td_risk_value"><span class="simulated_results_indicator" style="display: none; color: blue;">* &#8211; Simulated</span></td>
                    <td class="td_risk_value"></td>
                    <td class="td_cost_value"></td>
                    <td class="td_cost_value"></td>
                </tr>
                <%End If %>                
            </table>            
        </div>
        <%If IsOptimizationAllowed AndAlso Not IsEditable AndAlso CurrentPageID <> _PGID_RISK_SELECT_TREATMENTS Then %>
        <div id="divSolverMode" style="text-align: left; display: inline-block; vertical-align: top; padding-top: 7px;">
            <div id="divOptimizerTabs" class="ec_tabs small" style="border: 1px solid #6699cc;">
                <div id="optimizerTab0" class="whole" style="padding: 2px;">
                     <nobr><span><%=ResString("lblROBudgetLimit")%>:</span>&nbsp;<%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%></nobr> 
                     <input type="text" id="tbBudgetLimit" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=If(PM.Parameters.Riskion_ControlsActualSelectionMode = 0, "", CostString(RA.RiskOptimizer.BudgetLimit))%>" style="width:140px; height:20px; margin-top:2px; text-align:right;" />
                </div>
                
                <div id="optimizerTab1" class="whole" style="padding: 2px;">
                    <nobr><span><%=ResString("lblROMaxRisk")%>:</span>&nbsp;<span id="lblMaxRiskDollSign" <%=If(ShowDollarValue, "", " style='display:none;'")%>><%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%></span></nobr> 
                    <nobr><input type="number" id="tbMaxRisk" class="input number" step='any' autocomplete="off" value="<%=If(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.MaxRisk * PM.DollarValueOfEnterprise, CostDecimalDigits), CStr(RA.RiskOptimizer.CurrentScenario.MaxRisk * 100))%>" style="width:80px; height:20px; margin-top:2px; text-align:right;" /><span id="lblMaxRiskPrcSign" <%=If(ShowDollarValue, " style='display:none;'", "")%>>&#37;</span></nobr>
                </div>
                
                <div id="optimizerTab2" class="whole" style="padding: 2px;">
                    <nobr><span><%=ResString("lblROMinReduction")%>:</span>&nbsp;<span id="lblMinRiskDollSign" <%=IF(ShowDollarValue, "", " style='display:none;'")%>><%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol%></span></nobr> 
                    <nobr><input type="number" id="tbMinReduction" class="input number" step='any' autocomplete="off" value="<%=If(ShowDollarValue, CostString(RA.RiskOptimizer.CurrentScenario.MinReduction * PM.DollarValueOfEnterprise, CostDecimalDigits), CStr(RA.RiskOptimizer.CurrentScenario.MinReduction * 100))%>" style="width:80px; height:20px; margin-top:2px; text-align:right;" /><span id="lblMinRiskPrcSign" <%=If(ShowDollarValue, " style='display:none;'", "")%>>&#37;</span></nobr>
                </div>
            </div>            
        </div>        
        <%End If %>
        <%If Not IsEditable Then %>
        <%--<div id="divIgnores" class="text" style='display: inline-block; overflow: hidden; margin: -5px 2px 0px 2px; text-align:center ; vertical-align: top;'>--%>
            <%If CurrentPageID <> _PGID_RISK_SELECT_TREATMENTS Then%>
            <div style="text-align: left; display: inline-block; vertical-align: top;">
            <fieldset class="legend small" style="display:inline-block; width:auto; padding-bottom:7px; padding-top:3px;" id="pnlIgnore">                
            <legend class="text legend_title">&nbsp;<%=ResString("lblIgnorePnl")%>:&nbsp;</legend>
            <label id='lblcbOptMusts'        <%=If(PM.Controls.EnabledControls.Where(Function(a) a.Must).Count > 0, "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.Musts, "checked='checked'", "")%> id='cbOptMusts' /><%=ResString("optIgnoreMusts")%></label>
            <label id='lblcbOptMustNots'     <%=If(PM.Controls.EnabledControls.Where(Function(a) a.MustNot).Count > 0, "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.MustNots, "checked='checked'", "")%> id='cbOptMustNots' /><%=ResString("optIgnoreMustNot")%></label>
            <label id='lblcbOptGroups'       <%=If(RA.Scenarios.ActiveScenario.Groups.HasData(RA.Scenarios.ActiveScenario.Alternatives), "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.Groups, "checked='checked'", "")%> id='cbOptGroups' /><%=ResString("optIgnoreGroups")%></label>
            <br/>
            <% If Not App.isRiskEnabled Then %>
            <label id='lblcbOptConstraints'  <%=If(RA.Scenarios.ActiveScenario.Constraints.Constraints.Count > 0, "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.CustomConstraints, "checked='checked'", "")%> id='cbOptConstraints' /><%=ResString("optIgnoreCC")%></label>
            <% End If %>
            <label id='lblcbOptDependencies' <%=If(RA.Scenarios.ActiveScenario.Dependencies.HasData(RA.Scenarios.ActiveScenario.Alternatives, RA.Scenarios.ActiveScenario.Groups), "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.Dependencies, "checked='checked'", "")%> id='cbOptDependencies' /><%=ResString("optIgnoreDependencies")%></label>
            <br/>
            <label id='lblcbOptFundingPools' <%=If(RA.Scenarios.ActiveScenario.FundingPools.Pools.Count > 0, "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.FundingPools, "checked='checked'", "")%> id='cbOptFundingPools' /><%=ResString("optIgnoreFP")%></label>
            <% If RA_ShowTimeperiods Then %>
            <label id='lblcbOptTimePeriods'  <%=If(RA.Scenarios.ActiveScenario.TimePeriods.Periods.Count > 0, "style='font-weight:bold;'","style='color:#999;'")%> ><input type='checkbox' class='checkbox' onclick='onOptionClick(this.id, this.checked);' <%=If(Not RA.RiskOptimizer.CurrentScenario.Settings.TimePeriods, "checked='checked'", "")%> id='cbOptTimePeriods' /><%=ResString("optTimePeriods")%></label>
            <% End If %>
            </fieldset>
            </div>
            <%End If%>
            <div style="text-align: left; display: inline-block; vertical-align: top;">
            <fieldset class="legend small" style="display: inline-block; text-align:left; vertical-align:top; padding-bottom:5px; padding-top:1px;" id="pnlSimulationsSettings">
            <legend class="text legend_title">&nbsp;Simulations Settings&nbsp;</legend>
            Number of trials:<input type="text" id="tbNumSimulations" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=PM.RiskSimulations.NumberOfTrials.ToString%>" style="width:80px; margin-top:2px; text-align:right;" onblur="saveNumerOfSimulations(this.value);" /><br/>
                Seed:<input type="text" id="tbRandSeed" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' value="<%=PM.RiskSimulations.RandomSeed.ToString%>" style="width:50px; margin-top:2px; text-align:right;" onblur="saveRandSeed(this.value);" />
                &nbsp;<label><input type="checkbox" id="cbKeepRandSeed" <%=CStr(IIf(PM.RiskSimulations.KeepRandomSeed, " checked='checked' ", " "))%> style="margin-top:2px;" onclick="saveKeepRandSeed(this.checked);" />&nbsp;Keep Seed</label>
            </div>
        </fieldset>
        <%--</div>--%>
        <%End If%>        
        <div id="divMessage" style="display: none; margin: 2px;" class="warning"></div>
        </td>
    </tr>    
    <tr id='trContent' valign='top' style="height: 100%;">        
        <% If CurrentPageID = _PGID_RISK_OPTIMIZER_FROM_SOURCES OrElse CurrentPageID = _PGID_RISK_OPTIMIZER_TO_OBJS Then %>
        <td style="width: 250px; height: 100%;">
            <div id="divTreeContainer" valign='top' style="height: 100%;">
                <div id='divHierarchy'>                
            </div>
        </td>
        <% End If %>
        <td id="tdContent" align="center">
            <%--<div id="divMessage" style="display:none; position: absolute; width: 100%; left: 50%; margin-left: -50%; padding:1ex; text-align: center" class="text error"></div>--%>
            <div id="divContent" style='overflow: hidden; height: 100%; padding: 0px; text-align: center;' onscroll='$("div.context-menu,div.options-menu").hide(200); hideCMenu();'>
                    <div id='tableContent' style="width:100%; padding: 4px 1px; <% = If(RA_ShowTimeperiods, "display:none;", "") %>"></div>
                    <%--<span id="lblLoading" class="text" style="display:inline-block; margin-top:40px;">Loading...</span>--%>
                    <div id="RATimeline" style="overflow: auto; <% = If(Not RA_ShowTimeperiods, "display:none;", "") %> text-align: center;">...</div>
            </div>
        </td>
    </tr>
</table>

<div id="divApplications" style="display:none;">
    <label id='lblHeader'><h6>is applied to:</h6></label>
    <table id="tblApplications" border='0' cellspacing='1' cellpadding='2' style='width:100%;' class='grid'></table>
</div>

<%-- Category editor dialog --%>
<div id='editorForm' style='display:none;'>
<table border='0' cellspacing='0' cellpadding='5' class='text' style='margin-right:5px;'>
<tr><td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'><nobr><% = ParseString("%%Control%% name:") %>&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' style='width:280px;' id='tbControlName' value='' onfocus='getCtrlName(this.value);' onkeyup='getCtrlName(this.value);' onblur='getCtrlName(this.value);' /></td></tr>
<%--<tr><td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'><nobr><% = ParseString("Description:") %>&nbsp;&nbsp;</nobr></td><td><textarea rows='4' cols='40' style='width:280px;margin-top:2px;' id='tbControlDescr' onfocus='getCtrlDescr(this.value);' onkeyup='getCtrlDescr(this.value);' onblur='getCtrlDescr(this.value);'></textarea></td></tr>--%>
<tr><td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'><nobr><% = ParseString("Cost:") %>&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' style='width:100px;margin-top:3px;text-align:right;' id='tbControlCost' value='' onfocus='getCtrlCost(this.value);' onkeyup='getCtrlCost(this.value);' onblur='getCtrlCost(this.value);' /></td></tr>
<tr><td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'><nobr><% = ParseString("Category:") %>&nbsp;&nbsp;</nobr></td><td>
    <%--<select id='lbCategories' style='width:280px;margin-top:4px;' size='4' disabled='disabled'></select>--%>
    <ul id='lbCategories' class="text" disabled="disabled" style='height:14ex; width:280px; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'></ul>
</td></tr>
<tr><td valign='top' style='padding:10px 0px 0px 0px' align='right' style='width:100px;'><nobr>Add category:&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' id='tbNewCategory' style='width:254px; margin:2px 0px;' onfocus="onNewCategoryKeyUp(this.value);" onkeyup="onNewCategoryKeyUp(this.value);" onblur="onNewCategoryKeyUp(this.value);" />&nbsp;<i class="fas fa-plus" id="btnAddCategory" style='vertical-align:middle; cursor:pointer; filter:alpha(opacity=50); opacity:0.5;' onclick='AddCategory(); return false;'></i></td></tr>
</table>
</div>

<%-- Events with Checkboxes dialog --%>
<div id='selectEventsForm' style='display: none; overflow:hidden;'>
    <div id="divSelectEvents" style="padding: 5px; text-align: left; overflow:auto; height:300px;"></div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllEvents(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllEvents(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

<div id='selectNodesForm' style='display: none; overflow:hidden;'>
    <div id='divSelectNodes' class='cvTree' style='padding: 5px; text-align: left; overflow:auto; height:300px;'>
        <ul id='ulTree' class='ztree'></ul>
    </div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllNodes(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllNodes(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

<%-- Controls with Checkboxes dialog --%>
<div id='selectControlsForm' style='display: none; overflow:hidden;'>
    <div id="divSelectControls" style="padding: 5px; text-align: left; overflow:auto; height:300px;"></div>
    <div style='text-align: center; margin-top: 1ex; width: 100%;'>
        <a href="" onclick="filterSelectAllControls(1); return false;" class="actions"><% =ResString("lblAll")%></a> |
        <a href="" onclick="filterSelectAllControls(0); return false;" class="actions"><% =ResString("lblNone")%></a>
    </div>
</div>

<div id="divScenarios" style="display:none; text-align:center">
<h6><% = JS_SafeString(ResString("lblRAScenarios"))%>:</h6>
<div style="overflow:auto;" id="pScenarios"><table id="tblScenarios" border='0' cellspacing='1' cellpadding='2' style='width:96%' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header" align="center">
        <th style="width:1em">&nbsp;</th>
        <th><% = JS_SafeString(ResString("tblRAScenarioName"))%></th>
        <th><% = JS_SafeString(ResString("tblRAScenarioDesc"))%></th>
        <th width="80"><% = JS_SafeString(ResString("tblRAScenarioAction"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
</div>

<div runat='server' id='DivCategoryData' style='display:none;'></div>
<div id="divOptions" class="options-menu text" style="z-index:1000; position:absolute; background-color:#f0f0f0; border:1px solid #9ba7b7; padding:5px; width:220px; text-align: left; display: none;">

    <%--<button id="btnPagination" onclick="onPagination_Menu(this); return false;" class='button' type="button" style="width:150px;" <%=If(PM.Controls.Controls.Count > 0, "" , " disabled='disabled' ")%>><img id="imgPagination" src="<% =ImagePath %>pane-list.png" width=16 height=16 />&nbsp;<%=ResString("lblShowByPages")%>&nbsp;&#x25BE;</button>--%>
    <div style="display: inline-block; vertical-align: top;">
    <nobr><input type='checkbox' class='checkbox' id='cbControlsDescriptions' <%=If(PM.Parameters.Riskion_Control_ShowInfodocs , " checked", "") %> onclick='showDescriptions = this.checked; sendCommand("action=show_descriptions&chk=" + showDescriptions);' /><label for='cbControlsDescriptions' ><%=ParseString("Show descriptions")%></label></nobr>
    </div><br/>                       
    <%If _OPT_ALLOW_SA_REDUCTIONS AndAlso Not IsEditable Then%>
    <div style="display: inline-block; vertical-align: top;" title="<%=ParseString("The amount of %%risk%% the %%control%% would reduce if it were the only %%control%% applied. The %%risk%% reduction due to the %%control%% might be less if other %%controls%% are also applied")%>">
    <nobr><input type='checkbox' class='checkbox' id='cbShowControlsRiskReduction' <% = If(PM.Parameters.Riskion_ShowControlsRiskReduction, " checked", "") %> onclick='showControlsRiskReductionClick(this.checked);'/><label for='cbShowControlsRiskReduction' id='lblShowControlsRiskReduction'><%=ParseString("S.A. Reduction")%></label></nobr>
    </div>
    <%If IsOptimizationAllowed AndAlso Not IsEditable AndAlso CurrentPageID <> _PGID_RISK_SELECT_TREATMENTS Then %>
    <div style="display: inline-block; vertical-align: top;">
    <nobr><input type='checkbox' class='checkbox' id='cbShowRiskReductionOptions' <%=If(PM.Parameters.RiskionShowRiskReductionOptions, " checked", "") %> onclick='showRiskReductionOptionsClick(this.checked);'/><label for='cbShowRiskReductionOptions' id='lblShowRiskReductionOptions'><%=ParseString("Show %%Risk%% Reduction Options")%></label></nobr>
    </div>
    <%End If%>
    <%End If%>
</div>

<div id="divAttributes" style="display:none; text-align:center">
<h6><%=JS_SafeString(ResString("lblRAColumns"))%>:</h6>
<div style="overflow:auto;" id="pAttributes"><table id="tblAttributes" border='0' cellspacing='1' cellpadding='2' style='width:96%;' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header" align="center">
        <th style="width:1em">&nbsp;</th> <%--Drag-drop handle column--%>
        <th style="width:40%" colspan="2"><%=JS_SafeString(ResString("tblRAAttributeName"))%></th>
        <th width="100"><%=JS_SafeString(ResString("tblAttributeType"))%></th>
        <th colspan="2"><%=JS_SafeString(ResString("tblCategoriesOrDefault"))%></th>
        <th width="80"><%=JS_SafeString(ResString("tblRAScenarioAction"))%></th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
</div>

<div id="divAttributesValues" style="display:none; text-align:center">
<div style="overflow:auto;" id="pAttributesValues"><table id="tblAttributesValues" border='0' cellspacing='1' cellpadding='2' style='width:96%;' class='grid drag_drop_grid'>
    <thead>
      <tr class="text grid_header" align="center">        
        <th style="width:1em">&nbsp;</th> <%--Drag-drop handle column--%>
        <th style="width:90%"><%=JS_SafeString(ResString("lblRACategory"))%></th>
        <th>&nbsp;<%=JS_SafeString(ResString("tblIsDefault"))%>&nbsp;</th>
        <th>&nbsp;<%=JS_SafeString(ResString("tblRAScenarioAction"))%>&nbsp;</th>
      </tr>
    </thead>
    <tbody>
    </tbody>
</table></div>
</div>

<div id='divValueOfEnterpriseEditor' style="max-width: 70%; display: none;"></div>

<div id="tooltip" style="display: none; text-align: left;">
    <p style="text-align: left;"><%=ResString("msgPasteControlsHint")%></p>
</div>

</asp:Content>