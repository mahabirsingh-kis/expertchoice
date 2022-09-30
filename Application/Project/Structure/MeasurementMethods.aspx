<%@ Page Language="VB" Inherits="MeasurementMethodsPage" title="Measurement Methods" Codebehind="MeasurementMethods.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
    <script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
    .dx-treelist .dx-row > td {
        /*padding: 1px 5px !important;
        margin: 0 !important;*/
        height: 28px;
        vertical-align: middle !important;
    }
    /*.dx-tab-selected {
    background-color: #0051A9 !important;
    }
    .dx-tab-selected > .dx-tab-content > .dx-tab-text {
        color: white !important;
    }
    .dx-tab-selected > .dx-tab-content > .dx-tab-text > .dx-icon {
        color: white !important;
    }*/
    .ui-dialog .ui-dialog-content {
        overflow: hidden;
    }
</style>
<script language="javascript" type="text/javascript">
    
    var column_ver = "20201212";
    
    var MTMode = <% = CInt(MTMode) %>;
    var isReadOnly = <% = Bool2JS(App.IsActiveProjectStructureReadOnly) %>;
    var isRisk = <% = Bool2JS(App.isRiskEnabled) %>;

    var noSourcesGuid = "<% = NoSourcesGuid.ToString %>";
    var noSourcesID = <% = NoSourcesID.ToString %>;

    var mmAll = <%=CInt(MeasurementMethodsPageTypes.mmAll)%>;
    var mmObj = <%=CInt(MeasurementMethodsPageTypes.mmObj)%>;
    var mmAlt = <%=CInt(MeasurementMethodsPageTypes.mmAlt)%>;

    var mtPairwise = <%=CInt(ECMeasureType.mtPairwise)%>;
    var mtDirect = <%=CInt(ECMeasureType.mtDirect)%>;
    var mtRatings = <%=CInt(ECMeasureType.mtRatings)%>;
    var mtUtilityCurve = <%=CInt(ECMeasureType.mtRegularUtilityCurve)%>;
    var mtStep = <%=CInt(ECMeasureType.mtStep)%>;
    var mtPWOutcomes = <%=CInt(ECMeasureType.mtPWOutcomes)%>; // Pairwise of probabilities
    var mtPWAnalogous = <%=CInt(ECMeasureType.mtPWAnalogous)%>; // Pairwise with Given Likelihood

    var nodes = <% = GetNodesData %>;
    // columns
    var COL_ID = "id";
    var COL_NODE_GUID = "guid";
    var COL_PID = "pid"; // parent node ID
    var COL_EXPANDED = "exp"; // 1 - expanded; 0 - collapsed; -1 - none
    var COL_LINES = "lines";
    var COL_NAME = "name";
    var COL_MT = "mt";
    var COL_MS_GUID = "ms";
    var COL_ACTION = "action";
    var COL_EL_COUNT = "el_count";
    var COL_JUDG_COUNT = "judg_count";
    var COL_JUDG_COUNT_STRING = "s_judg_count";
    var COL_JUDG_COUNT_HINT = "h_judg_count";
    var COL_NUM_COMPARISONS = "num_comp";
    var COL_DISPLAY_OPTION = "disp_opt";
    var COL_PAIRWISE_TYPE = "pw_type";
    var COL_FORCE_GRAPHICAL = "force_g";
    var COL_IS_TERMINAL = "is_term";
    var COL_PIPE_STEP = "pipe_step";
    var COL_IS_STRUCTURAL_ADJUST = "structural"; // 0 - no, 1 - yes, -1 - disabled
    var COL_RISK_NODE_TYPE = "node_type"; // 0 - Uncertainty, 1 - Category
    var COL_NODE_SYNTH_TYPE = "synth_type"; // 0 - sum, 1 - max
    var COL_NODE_HAS_STAT_DATA = "has_stat"; // 0 - no, 1 - yes, -1 - disabled (has statistical (Bayesian Updating) data)
    var COL_FORCE_GRAPHICAL_ENABLED = "fg_enabled";
    var COL_KNOWN_VALUE_STRING = "s_kw";
    var COL_IS_ALT_WITH_NO_SOURCE = "is_ns";

    var mscales = <% = GetMeasureScalesData()%>;    

    // indices
    var IDX_GUID  = 0;
    var IDX_MEASURE_TYPE = 1;
    var IDX_MEASURE_TYPE_NAME = 2;
    var IDX_SCALE_NAME  = 3;
    var IDX_DESCR = 4; // unused
    var IDX_RATINGS_TYPE = 5;
    var IDX_IS_DEFAULT   = 6;
    var IDX_APPLICATIONS = 7;
    var IDX_TAG   = 8;
    var IDX_INTENSITIES = 9;
    var IDX_USE_DIRECT_RATINGS = 10; // Either "Use direct ratings" OR "Step function Is Piecewise Linear" options
    var IDX_IS_STUCTURAL_ADJUST = 11;
    var IDX_RISK_NODE_TYPE = 12;
    var IDX_NODE_SYNTHESIS_TYPE = 13;

    var INT_ID = 0;
    var INT_NAME = 1;
    var INT_VALUE = 2;
    var INT_COMMENT = 3;

    // actions
    var ACTION_VIEW    = "<%=ACTION_VIEW%>";
    var ACTION_MEASURE_TYPE = "<%=ACTION_MEASURE_TYPE%>";
    var ACTION_MEASURE_SCALE = "<%=ACTION_MEASURE_SCALE%>";
    var ACTION_NUM_COMPARISONS = "<%=ACTION_NUM_COMPARISONS%>";
    var ACTION_PAIRWISE_TYPE = "<%=ACTION_PAIRWISE_TYPE%>";
    var ACTION_DISPLAY_OPTION = "<%=ACTION_DISPLAY_OPTION%>";
    var ACTION_FORCE_GRAPHICAL = "<%=ACTION_FORCE_GRAPHICAL%>";
    var ACTION_SET_MEASURE_TYPE_FOR_ALL = "<%=ACTION_SET_MEASURE_TYPE_FOR_ALL%>";
    var ACTION_SET_NODE_PROPERTY = "<%=ACTION_SET_NODE_PROPERTY%>";
    var ACTION_GET_STAT_DATA = "<%=ACTION_GET_STAT_DATA%>";
    var ACTION_SAVE_STAT_DATA = "<%=ACTION_SAVE_STAT_DATA%>";

    // default pairwise options
    var DefaultMeasurementTypeAlt = <%=CInt(PM.PipeBuilder.PipeParameters.DefaultCoveringObjectiveMeasurementType)%>;
    var DefaultNumberComparisonsObj = <%=CInt(PM.PipeBuilder.PipeParameters.EvaluateDiagonals)%>;
    var DefaultNumberComparisonsAlt = <%=CInt(PM.PipeBuilder.PipeParameters.EvaluateDiagonalsAlternatives)%>;
    var DefaultPairwiseTypeObj = <%=CInt(PM.PipeBuilder.PipeParameters.PairwiseType)%>;
    var DefaultPairwiseTypeAlt = <%=CInt(PM.PipeBuilder.PipeParameters.PairwiseTypeForAlternatives)%>;
    var DefaultDisplayObj = <%=CStr(IIf(PM.PipeBuilder.PipeParameters.ObjectivesPairwiseOneAtATime, "1", "0"))%>;
    var DefaultDisplayAlt = <%=CStr(IIf(PM.PipeBuilder.PipeParameters.AlternativesPairwiseOneAtATime, "1", "0"))%>;
    var DefaultForceGraphicalObj = <%=CStr(IIf(PM.PipeBuilder.PipeParameters.ForceGraphical, "1", "0"))%>;
    var DefaultForceGraphicalAlt = <%=CStr(IIf(PM.PipeBuilder.PipeParameters.ForceGraphicalForAlternatives, "1", "0"))%>;

    var dlg_editor_main;
    var dlg_edit_stat;

    var curStatDataImg;
    var copyFromNodeID = -1;
    var copyFromIsEvent = false;
    var copyFromNodeIsTerminal = true;

    function onSetViewMode(value) {
        if (MTMode !== value*1) {
            displayLoadingPanel(true);
            MTMode = value*1;
            sendCommand("action=" + ACTION_VIEW + "&value=" + value, true);        
        }
    }

    function getNodeById(node_id, is_ns) {
        var nodes_len = nodes.length;
        for (var i = 0; i < nodes_len; i++) {
            if (nodes[i][COL_ID] == node_id && ((typeof is_ns == "undefined" && !nodes[i][COL_IS_ALT_WITH_NO_SOURCE]) || (typeof is_ns != "undefined" && nodes[i][COL_IS_ALT_WITH_NO_SOURCE] == is_ns))) {
                return nodes[i];
            }
        }
        return null;
    }

    function getScaleById(id) {
        for (var i = 0; i < mscales.length; i++) {
            if (mscales[i][IDX_GUID] == id) return mscales[i];
        }
        return null;
    }

    function getMeasurementTypeName(id) {
        var retVal = "";
        switch (id) {
            case 0:
                retVal = "Pairwise Comparisons";
                break;
            case 1:
                retVal = "Rating Scale";
                break;
            case 2:
            case 4:
            case 7:
                retVal = "Utility Curve";
                break;
            case 5:
                retVal = "Direct";
                break;
            case 6:
                retVal = "Step Function";
                break;
            case 8:
                retVal = "Pairwise of Outcomes";
                break;
            case 10:
                retVal = "Pairwise with Known Likelihoods";
                break;
            default:
                retVal = "Undefined";
                break;
        }
        return retVal;
    }

    function getTotalStepsCount() {
        var steps_count = 0;
        var countedIds = [];
        for (var i = 0; i < nodes.length; i++) {
            if ((MTMode == mmAll || (MTMode == mmAlt && nodes[i][COL_IS_TERMINAL] == 1) || (MTMode == mmObj && nodes[i][COL_IS_TERMINAL] == 0)) && (nodes[i][COL_ID] !== noSourcesID) && countedIds.indexOf(nodes[i][COL_ID]) == -1) {
                steps_count += nodes[i][COL_JUDG_COUNT];
                countedIds.push(nodes[i][COL_ID]);
            }
        }
        return steps_count;
    }

    function getResetButton(sFuncName, node_id, pid, value, is_ns) {
        return "&nbsp;<a href='' onclick='" + sFuncName + "(" + node_id + "," + pid + "," + value + "," + is_ns + "); return false;'  title='Change to default value'><i class='fas fa-undo'></i></a>";
    }

    /* Data Table */
    function isBlank(data) {
        return data[COL_ID] == noSourcesID || (!(data[COL_IS_TERMINAL] && MTMode !== mmObj || !data[COL_IS_TERMINAL] && MTMode !== mmAlt));
    }
    function initTable() {
        var storageKey = "MM_DG_<% = PRJ.ID %>_<% = CInt(PM.ActiveHierarchy) %>" + column_ver;
        var hasState = typeof localStorage.getItem(storageKey) !== "undefined" && localStorage.getItem(storageKey) != null;

        var table_main = (($("#tableContent").data("dxDataGrid"))) ? $("#tableContent").dxDataGrid("instance") : null;
        if (table_main !== null) { table_main.dispose(); }

        var total_steps_count = getTotalStepsCount();
        //$("#lblTotalSteps").html("Total Evaluation Steps: " + total_steps_count);
        $("#lblTotalSteps").html("Total Judgments: " + total_steps_count); // renamed per Zyza's request - case 21358

        var columns = [];

        var DefaultMeasurementTypeAltString = "<%=DefaultMeasurementTypeAltString(MeasurementMethodsPageTypes.mmAll)%>";
        if (MTMode == mmObj) DefaultMeasurementTypeAltString = "<%=DefaultMeasurementTypeAltString(MeasurementMethodsPageTypes.mmObj)%>";
        if (MTMode == mmAlt) DefaultMeasurementTypeAltString = "<%=DefaultMeasurementTypeAltString(MeasurementMethodsPageTypes.mmAlt)%>";

        var DefaultNumberOfComparisonsString = "<%=DefaultNumberOfComparisonsString(MeasurementMethodsPageTypes.mmAll)%>";
        if (MTMode == mmObj) DefaultNumberOfComparisonsString = "<%=DefaultNumberOfComparisonsString(MeasurementMethodsPageTypes.mmObj)%>";
        if (MTMode == mmAlt) DefaultNumberOfComparisonsString = "<%=DefaultNumberOfComparisonsString(MeasurementMethodsPageTypes.mmAlt)%>";

        var DefaultDisplayString = "<%=DefaultDisplayString(MeasurementMethodsPageTypes.mmAll)%>";
        if (MTMode == mmObj) DefaultDisplayString = "<%=DefaultDisplayString(MeasurementMethodsPageTypes.mmObj)%>";
        if (MTMode == mmAlt) DefaultDisplayString = "<%=DefaultDisplayString(MeasurementMethodsPageTypes.mmAlt)%>";


        var DefaultPairwiseTypeString = "<%=DefaultPairwiseTypeString(MeasurementMethodsPageTypes.mmAll)%>";
        if (MTMode == mmObj) DefaultPairwiseTypeString = "<%=DefaultPairwiseTypeString(MeasurementMethodsPageTypes.mmObj)%>";
        if (MTMode == mmAlt) DefaultPairwiseTypeString = "<%=DefaultPairwiseTypeString(MeasurementMethodsPageTypes.mmAlt)%>";

        var DefaultForceGraphicalString = "<%=DefaultForceGraphicalString(MeasurementMethodsPageTypes.mmAll)%>";
        if (MTMode == mmObj) DefaultForceGraphicalString = "<%=DefaultForceGraphicalString(MeasurementMethodsPageTypes.mmObj)%>";
        if (MTMode == mmAlt) DefaultForceGraphicalString = "<%=DefaultForceGraphicalString(MeasurementMethodsPageTypes.mmAlt)%>";

        //init columns headers                
        columns.push({ "caption": MTMode == mmAlt ? "<%=ResString(If(PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact, "columnHeaderObjectivesImpact", "columnHeaderObjectives"))%>" : MTMode == mmObj ? "<%=ParseString(If(PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact, "Measure %%Objective(i)%% Importance With Respect To", If(PM.IsRiskProject, "Measure %%Objectives(l)%% With Respect To", "Measure %%Objectives%% With Respect To")))%>" : "<%=ParseString(If(PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact, "Measure %%Objective(i)%% Importance/%%Alternative%% Consequences With Respect To", If(PM.IsRiskProject, "Measure %%Objectives%%/%%Alternatives%% With Respect To", "Measure %%Objectives%%/%%Alternatives%% With Respect To")))%>",
            "alignment": "left", "dataType": "string", "allowSorting": true, "allowSearch": true, "dataField" : COL_NAME, "fixed" : true, "minWidth" : 200,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                // nodes names 
                var cb_copy = "<input class='checkbox' type='checkbox' " + (data[COL_ID] !== noSourcesID ? "data-has-cb='" + data[COL_ID] + "' " : "") + "data-term='" + data[COL_IS_TERMINAL] + "' data-is-ns='" + data[COL_IS_ALT_WITH_NO_SOURCE] + "' style='display: none;' >";
                var $div = cellElement.prop("id", "lblNodeName" + data[COL_ID]).prop("title", data[COL_NAME]).addClass(data[COL_RISK_NODE_TYPE] == 1 ? "categorical" : "");
                $div.append(cb_copy);
                $div.append(data[COL_NAME]).appendTo(cellElement); 
                // connecting lines
                //if (data[COL_LINES].length > 0) {
                //    for (var i = 0; i < data[COL_LINES].length; i++) {
                //        var img = "vert-line-32.png";
                //        switch(data[COL_LINES][i]+"") {
                //            case "_":
                //                img = "vert-blank-32.png";
                //                break;
                //            case "|":
                //                img = "vert-line-32.png";
                //                break;
                //            case "L":
                //                img = "vert-corner-32.png";
                //                break;
                //            case "+":
                //                img = "vert-conn-32.png";
                //                break;
                //        }
                //        $div.parent().css({"padding" : "0px"}).prepend("<img align='left' src='../../Images/connection_lines/" + img + "' alt='' >");
                //    }
                //}
            }
        });
        columns.push({ "caption": "Measurement Type" + DefaultMeasurementTypeAltString, "alignment": "left", "dataType": "string", "allowSorting": true, "dataField" : COL_MT, "width" : 170,
            headerCellTemplate : function (columnHeader, headerInfo) {                    
                return columnHeader.append("Measurement Type" + DefaultMeasurementTypeAltString);
            },
            //cellTemplate: function (cellElement, cellInfo) {
            //    //var data = cellInfo.data;
            //    //$("<span></span>").html(data[IDX_WG_USER_EMAIL]).css(data[IDX_WG_USER_DISABLED] ? { "color" : "#a0a0a0" } : {}).appendTo(cellElement); 
            //}
        });
        columns.push({ "caption": "Measurement Scale", "alignment": "center", "dataType": "string", "allowSorting": true, "dataField" : COL_MS_GUID, "width" : 170, "visible" : MTMode != mmObj || isRisk,            
            //cellTemplate: function (cellElement, cellInfo) {
            //    //var data = cellInfo.data;
            //    //$("<span></span>").html(data[IDX_WG_USER_EMAIL]).css(data[IDX_WG_USER_DISABLED] ? { "color" : "#a0a0a0" } : {}).appendTo(cellElement); 
            //}
        });
        columns.push({ "caption": "Action", "dataType": "string", "dataField" : COL_ACTION, "width" : 170,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                var scaleExists = getScaleById(data[COL_MS_GUID]) !== null;

                // action
                var btnCopy = data[COL_MT] >= 0 && (data[COL_MS_GUID] == "" || scaleExists) && !isReadOnly ? "<button type='button' class='button' onclick='startCopyMS(" + data[COL_ID] + "," + data[COL_IS_ALT_WITH_NO_SOURCE] + "); return false;' title='Copy'><i class='fas fa-copy'></i>&nbsp;Copy</button>&nbsp;" : "";
                var btnM = "";//"<a href='' title='Max rather than sum in synthesis' onclick='showNodeMenu(this, \"m\"," + data[COL_ID] + "); return false;'><i class='fab fa-medium-m' " + (data[COL_NODE_SYNTH_TYPE] == 1 ? "" : " style='filter:alpha(opacity=50); opacity:0.5;' ") + "></i></a>&nbsp;";
                <%--var btnS = "<img align='middle' style='cursor:hand;margin:0px 2px; width:16px;' src='<%=ImagePath()%>button_s" + (data[COL_IS_STRUCTURAL_ADJUST] == 1 ? "" : "_") + ".png'  alt='' title='Structural Adjust' onclick='showNodeMenu(this, \"s\"," + data[COL_ID] + ");' >";
                var btnB = "<img align='middle' style='cursor:hand;margin:0px 2px; width:16px;' src='<%=ImagePath()%>button_b" + (data[COL_NODE_HAS_STAT_DATA] == 1 ? "" : "_") + ".png'  alt='' title='Bayesian Updating Data' onclick='showNodeMenu(this, \"b\"," + data[COL_ID] + ");' >";--%>
                var btn_null = "<span style='display:inline-block;margin:0px 2px; width:16px;'></span>";                
                var btnEdit = data[COL_MT] >= 0 && data[COL_MS_GUID] !== "" && scaleExists && !isReadOnly ? "<a href='' title='Edit' onclick='selectedTypeID=" + data[COL_MT] + "; selectedScaleID=\"" + data[COL_MS_GUID] + "\"; onManageScalesDlg(false); return false;'><i class='fas fa-edit'></i></a>&nbsp;" : "";
                var btnPreview = data[COL_PIPE_STEP] > 0 ? "<a href='' title='Preview' onclick='onPreviewPipeStep(\"" + data[COL_NODE_GUID] + "\"," + data[COL_PIPE_STEP] + "," + data[COL_IS_TERMINAL] + "); return false;'><i class='fas fa-eye'></i></a>&nbsp;" : ""; // opacity:0.4;filter:alpha(opacity=40);                                
                $("<span></span>").html(isBlank(data) ? "" : "<nobr>&nbsp;&nbsp;" + (data[COL_NODE_SYNTH_TYPE] != -1 ? btnM : btn_null) + "&nbsp;" + (data[COL_MT] == mtPairwise || data[COL_MT] == mtDirect ? (btnCopy + btnPreview) : (btnCopy + btnPreview + btnEdit)) + "</nobr>").appendTo(cellElement); 
            }
        });
        columns.push({ "caption": "# Of Elements In Cluster", "alignment": "center", "allowSorting": true, "dataField" : COL_EL_COUNT, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                var span = $("<span></span>");                
                span.html(isBlank(data) ? "" : data[COL_EL_COUNT]).appendTo(cellElement); 
            }
        });
        columns.push({ "caption": "# Of Judgments In Cluster <br>Total: " + total_steps_count, "alignment": "center", "allowSorting": true, "dataField" : COL_JUDG_COUNT, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            headerCellTemplate : function (columnHeader, headerInfo) {                    
                return columnHeader.append("# Of Judgments In Cluster <br>Total: " + total_steps_count);
            },
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                var span = $("<span></span>");
                
                span.html(isBlank(data) ? "" : data[COL_JUDG_COUNT_STRING]).appendTo(cellElement); 

                //todo
                // # of judgments in cluster (tooltip)
                if (data[COL_JUDG_COUNT_HINT] != "" && !isBlank(data)) {
                    span.prop("title", data[COL_JUDG_COUNT_HINT]);
                }
            }
        });        
        columns.push({ "caption": "# Of Comparisons <br>" + DefaultNumberOfComparisonsString, "alignment": "center", "allowSorting": true, "dataField" : COL_NUM_COMPARISONS, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            headerCellTemplate : function (columnHeader, headerInfo) {                    
                return columnHeader.append("# Of Comparisons <br>" + DefaultNumberOfComparisonsString);
            },
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                if (!(data[COL_MT] == mtPairwise || data[COL_MT] == mtPWAnalogous || data[COL_MT] == mtPWOutcomes)) return;
                // # of Comparisons
                isDefVal = ((data[COL_IS_TERMINAL] || data[COL_IS_ALT_WITH_NO_SOURCE]) && data[COL_NUM_COMPARISONS] == DefaultNumberComparisonsAlt) || (!data[COL_IS_TERMINAL] && !data[COL_IS_ALT_WITH_NO_SOURCE] && data[COL_NUM_COMPARISONS] == DefaultNumberComparisonsObj);
                var cb = "<select style='width:" + (isDefVal ? "150px" : "135px") + ";margin:3px;' onchange='onNumComparisonsChange(" + data[COL_ID] + "," + ((data[COL_PID])?data[COL_PID]:"-1") + ", this.value," + data[COL_IS_ALT_WITH_NO_SOURCE] + ")' " + (isReadOnly ? " disabled='disabled' " : "") + ">";
                cb += "<option value='0' " + (data[COL_NUM_COMPARISONS] == 0 ? " selected='selected' " : "") +">All pairs (maximum accuracy)(all diagonals)</option>";
                cb += "<option value='2' " + (data[COL_NUM_COMPARISONS] == 2 ? " selected='selected' " : "") +">Two diagonals (first and second diagonals)</option>";
                cb += "<option value='1' " + (data[COL_NUM_COMPARISONS] == 1 ? " selected='selected' " : "") +">One diagonal (least time)(first diagonal)</option>";
                cb += "</select>";
                var btn = isDefVal ? "" : getResetButton("onNumComparisonsChange", data[COL_ID], data[COL_PID], "\"\"", data[COL_IS_ALT_WITH_NO_SOURCE]);
                $("<span></span>").html(isBlank(data) ? "" : "<nobr>" + cb + btn + "</nobr>").appendTo(cellElement); 
            }
        });
        columns.push({ "caption": "Display <br>" + DefaultDisplayString, "alignment": "center", "allowSorting": true, "dataField" : COL_DISPLAY_OPTION, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            headerCellTemplate : function (columnHeader, headerInfo) {                    
                return columnHeader.append("Display <br>" + DefaultDisplayString);
            },
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                if (!(data[COL_MT] == mtPairwise || data[COL_MT] == mtPWAnalogous || data[COL_MT] == mtPWOutcomes)) return;
                // Display
                isDefVal = ((data[COL_IS_TERMINAL] || data[COL_IS_ALT_WITH_NO_SOURCE]) && data[COL_DISPLAY_OPTION] == DefaultDisplayAlt) || (!data[COL_IS_TERMINAL] && !data[COL_IS_ALT_WITH_NO_SOURCE] && data[COL_DISPLAY_OPTION] == DefaultDisplayObj);
                var cb = "<select style='width:" + (isDefVal ? "100px" : "75px") + ";margin:3px;' onchange='onDisplayOptionChange(" + data[COL_ID] + "," + ((data[COL_PID])?data[COL_PID]:"-1") + ", this.value," + data[COL_IS_ALT_WITH_NO_SOURCE] + ");' " + (isReadOnly ? " disabled='disabled' " : "") + ">";
                cb += "<option value='1' " + (data[COL_DISPLAY_OPTION] == 1 ? " selected='selected' " : "") +">One pair</option>";
                cb += "<option value='0' " + (data[COL_DISPLAY_OPTION] == 0 ? " selected='selected' " : "") +">All pairs</option>";                    
                cb += "</select>";
                var btn = isDefVal ? "" : getResetButton("onDisplayOptionChange", data[COL_ID], data[COL_PID], "\"\"", data[COL_IS_ALT_WITH_NO_SOURCE]);
                $("<span></span>").html(isBlank(data) ? "" : "<nobr>" + cb + btn + "</nobr>").appendTo(cellElement);
            }
        });
        columns.push({ "caption": "Pairwise Type <br>" + DefaultPairwiseTypeString, "alignment": "center", "allowSorting": true, "dataField" : COL_PAIRWISE_TYPE, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            headerCellTemplate : function (columnHeader, headerInfo) {                    
                return columnHeader.append("Pairwise Type <br>" + DefaultPairwiseTypeString);
            },
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                if (!(data[COL_MT] == mtPairwise || data[COL_MT] == mtPWAnalogous || data[COL_MT] == mtPWOutcomes)) return;
                // Pairwise type
                isDefVal = data[COL_FORCE_GRAPHICAL_ENABLED] && (((data[COL_IS_TERMINAL] || data[COL_IS_ALT_WITH_NO_SOURCE]) && data[COL_PAIRWISE_TYPE] == DefaultPairwiseTypeAlt) || (!data[COL_IS_TERMINAL] && !data[COL_IS_ALT_WITH_NO_SOURCE] && data[COL_PAIRWISE_TYPE] == DefaultPairwiseTypeObj));
                var cb = "<select style='width:" + (isDefVal ? "100px" : "75px") + ";margin:3px;' onchange='onPairwiseTypeChange(" + data[COL_ID] + "," + ((data[COL_PID])?data[COL_PID]:"-1") + ", this.value," + data[COL_IS_ALT_WITH_NO_SOURCE] + ");' " + (isReadOnly ? " disabled='disabled' " : "") + ">";                    
                cb += "<option value='2' " + (data[COL_PAIRWISE_TYPE] != 1 ? " selected='selected' " : "") +">Graphical/numerical</option>";                    
                cb += "<option value='1' " + (data[COL_PAIRWISE_TYPE] == 1 ? " selected='selected' " : "") +">Verbal</option>";
                cb += "</select>";
                var btn = isDefVal ? "" : getResetButton("onPairwiseTypeChange", data[COL_ID], data[COL_PID], "\"\"", data[COL_IS_ALT_WITH_NO_SOURCE]);
                $("<span></span>").html(isBlank(data) ? "" : "<nobr>" + cb + btn + "</nobr>").appendTo(cellElement);
            }
        });
        columns.push({ "caption": "Force Graphical <br>" + DefaultForceGraphicalString, "alignment": "center", "allowSorting": true, "dataField" : COL_FORCE_GRAPHICAL, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            headerCellTemplate : function (columnHeader, headerInfo) {
                return columnHeader.append("Force Graphical <br>" + DefaultForceGraphicalString);
            },
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                if (!(data[COL_MT] == mtPairwise || data[COL_MT] == mtPWAnalogous || data[COL_MT] == mtPWOutcomes)) return;
                // Force graphical
                isDefVal = ((data[COL_IS_TERMINAL] || data[COL_IS_ALT_WITH_NO_SOURCE]) && data[COL_FORCE_GRAPHICAL] == DefaultForceGraphicalAlt) || (!data[COL_IS_TERMINAL] && !data[COL_IS_ALT_WITH_NO_SOURCE] && data[COL_FORCE_GRAPHICAL] == DefaultForceGraphicalObj);
                var cb = "<input type='checkbox' class='checkbox' style='margin:3px; vertical-align:middle;' onclick='onForceGraphicalChange(" + data[COL_ID] + "," + ((data[COL_PID])?data[COL_PID]:"-1") + ", this.checked," + data[COL_IS_ALT_WITH_NO_SOURCE] + ")' " + (data[COL_FORCE_GRAPHICAL] == 1 && data[COL_FORCE_GRAPHICAL_ENABLED] ? " checked='checked' " : " ") + (isReadOnly || !data[COL_FORCE_GRAPHICAL_ENABLED] ? " disabled='disabled' " : "") + " >";
                var btn = isDefVal ? "" : getResetButton("onForceGraphicalChange", data[COL_ID], data[COL_PID], "\"\"", data[COL_IS_ALT_WITH_NO_SOURCE]);
                $("<span></span>").html(isBlank(data) ? "" : data[COL_PAIRWISE_TYPE] == 1 && (data[COL_EL_COUNT] >= 2) ? "<nobr>" + cb + btn + "</nobr>": "").appendTo(cellElement);
            }
        });
        <% If OPT_ALLOW_STRUCTURAL_ADJUST Then %>
        columns.push({ "caption": "SA", "alignment": "center", "allowSorting": true, "dataField" : "boolean", "dataField" : COL_IS_STRUCTURAL_ADJUST, "visible" : isAdvancedMode, "cssClass" : "on_advanced",
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                var cb = "";
                if (data[COL_IS_STRUCTURAL_ADJUST] !== -1) {
                    // structural adjust checkbox is visible only if node has grand children
                    cb = "<input type='checkbox' class='checkbox' style='margin:3px; vertical-align:middle;' onclick='sendCommand(\"action=" + ACTION_SET_NODE_PROPERTY + "&node_id=" + data[COL_ID] + "&pid=" + data[COL_PID] + "&prop=s&val=\"+this.checked, false);' " + (data[COL_IS_STRUCTURAL_ADJUST] == 1 ?  " checked='checked'" : "") + (isReadOnly ? " disabled='disabled' " : "") + " >";                    
                }
                $("<span></span>").html(isBlank(data) ? "" : cb).appendTo(cellElement);
            }
        });
        <% End If %>
        <% If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then %>
        //if (MTMode !== mmAlt) {
        columns.push({ "caption": "Category", "alignment": "center", "allowSorting": true, "width": 50, "dataField" : "boolean", "dataField" : COL_IS_STRUCTURAL_ADJUST,
            cellTemplate: function (cellElement, cellInfo) {
                var data = cellInfo.data;
                var cb = "";
                if (!data[COL_IS_ALT_WITH_NO_SOURCE]) {
                    // Category                    
                    cb = "<input type='checkbox' class='checkbox' style='margin:3px; vertical-align:middle;' onclick='sendCommand(\"action=" + ACTION_SET_NODE_PROPERTY + "&node_id=" + data[COL_ID] + "&pid=" + data[COL_PID] + "&prop=c&val=\"+this.checked, false);' " + (data[COL_RISK_NODE_TYPE] == 1 ?  " checked=\"checked\"" : "") + (isReadOnly ? " disabled=\"disabled\" " : "") + " >";                    
                }
                $("<span></span>").html(isBlank(data) ? "" : cb).appendTo(cellElement);
            }
        });
        //}
        <% End If %>

        var dataset = [];
        for (var i = 0; i < nodes.length; i++) {
            if (MTMode == mmObj && (nodes[i][COL_IS_ALT_WITH_NO_SOURCE] || nodes[i][COL_ID] == noSourcesID)) continue;
            dataset.push(nodes[i]);
        }

        //if (isRisk) $("#tableContent").removeClass("dx-treelist-withlines");

        table_main = $('#tableContent').dxTreeList( {
            autoExpandAll: true,
            dataSource: dataset,
            //dataStructure: "plain",
            columns: columns,
            columnFixing: {
                enabled: true
            },
            hoverStateEnabled: true,
            focusedRowEnabled: false,
            keyExpr: "id",
            parentIdExpr: "pid",
            rootValue: -1,
            columns: columns,
            searchPanel: {
                visible: true,
                text: "",
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            columnHidingEnabled: false,
            allowColumnResizing: true,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            allowColumnReordering: false,
            columnFixing: {
                enabled: true
            },            
            columnChooser: {                
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: false
            },
            editing: {
                mode: "cell",
                allowUpdating: false //!isReadOnly
            },
            onCellPrepared: getDxTreeListNodeConnectingLinesOnCellPrepared,
            pager: {
                allowedPageSizes: [10, 15, 20, 50, 100, 200, 500],
                showInfo: true,
                showNavigationButtons: true,
                showPageSizeSelector: true,
                visible: false
            },
            paging: {
                enabled: false
            },
            rowAlternationEnabled: true,
            scrolling: {
                showScrollbar: "always",
                mode: "standard"
            },
            showColumnLines: true,
            showRowLines: false,
            showBorders: true,
            loadPanel: main_loadPanel_options,
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: storageKey
            },
            scrolling: {
                mode: "standard"
            },
            sorting: {
                mode: "none" //"multiple"
            },
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items; 
                toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: '<h6 style="padding-top: 10px;">Measurement Methods</h6>' });
            },
            onRowPrepared: function (e) {
                if (e.rowType !== "data") return;
                var row = e.rowElement[0];
                var data = e.data;
                var isDefVal = true;                
                // measurement type
                if (data[COL_MT] >= 0 && (data[COL_IS_TERMINAL] && MTMode !== mmObj || !data[COL_IS_TERMINAL] && MTMode !== mmAlt)) {                    
                    //var mt_name = getMeasurementTypeName(data[COL_MT]*1);
                    var opts = "";
                    <%If Not PRJ.IsRisk Then%>
                    if (data[COL_IS_TERMINAL] == 1) { // measurement types for alternatives
                        opts += "<option value='" + mtPairwise + "' " + (data[COL_MT] == mtPairwise ? " selected='selected' " : "") + ">Pairwise Comparisons</option>";
                        opts += "<option value='" + mtRatings + "' " + (data[COL_MT] == mtRatings ? " selected='selected' " : "") + ">Rating Scale</option>";
                        opts += "<option value='" + mtUtilityCurve + "' " + (data[COL_MT] == mtUtilityCurve ? " selected='selected' " : "") + ">Utility Curve</option>";
                        opts += "<option value='" + mtDirect + "' " + (data[COL_MT] == mtDirect ? " selected='selected' " : "") + ">Direct</option>";
                        opts += "<option value='" + mtStep + "' " + (data[COL_MT] == mtStep ? " selected='selected' " : "") + ">Step Function</option>";
                        isDefVal = data[COL_MT] == DefaultMeasurementTypeAlt;
                    } else { // measurement types for objectives
                        opts += "<option value='" + mtPairwise + "' " + (data[COL_MT] == mtPairwise ? " selected='selected' " : "") + ">Pairwise Comparisons</option>";
                        opts += "<option value='" + mtDirect + "' " + (data[COL_MT] == mtDirect ? " selected='selected' " : "") + ">Direct</option>";
                    }
                    <%End If%>
                    <%If PRJ.IsRisk Then%>
                    <%If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
                    if (!data[COL_IS_ALT_WITH_NO_SOURCE]) {
                        opts += "<option value='" + mtPWAnalogous + "' " + (data[COL_MT] == mtPWAnalogous ? " selected='selected' " : "") + ">Pairwise with Given <%=ParseString("%%Likelihood%%")%></option>";
                    }
                    opts += "<option value='" + mtDirect + "' " + (data[COL_MT] == mtDirect ? " selected='selected' " : "") + ">Direct</option>";
                    opts += "<option value='" + mtRatings + "' " + (data[COL_MT] == mtRatings ? " selected='selected' " : "") + ">Rating Scale</option>";
                    opts += "<option value='" + mtStep + "' " + (data[COL_MT] == mtStep ? " selected='selected' " : "") + ">Step Function</option>";
                    opts += "<option value='" + mtUtilityCurve + "' " + (data[COL_MT] == mtUtilityCurve ? " selected='selected' " : "") + ">Utility Curve</option>";
                    if (!data[COL_IS_ALT_WITH_NO_SOURCE]) {
                        opts += "<option value='" + mtPairwise + "' " + (data[COL_MT] == mtPairwise ? " selected='selected' " : "") + ">Pairwise Comparisons</option>";
                    }
                    opts += "<option value='" + mtPWOutcomes + "' " + (data[COL_MT] == mtPWOutcomes ? " selected='selected' " : "") + ">Pairwise of Probabilities</option>";
                    isDefVal = data[COL_MT] == DefaultMeasurementTypeAlt;
                    <%End If%>
                    <%If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact Then%>
                    if (data[COL_IS_TERMINAL] == 1) { // measurement types for alternatives                        
                        opts += "<option value='" + mtDirect + "' " + (data[COL_MT] == mtDirect ? " selected='selected' " : "") + ">Direct</option>";
                        opts += "<option value='" + mtRatings + "' " + (data[COL_MT] == mtRatings ? " selected='selected' " : "") + ">Rating Scale</option>";
                        opts += "<option value='" + mtStep + "' " + (data[COL_MT] == mtStep ? " selected='selected' " : "") + ">Step Function</option>";
                        opts += "<option value='" + mtUtilityCurve + "' " + (data[COL_MT] == mtUtilityCurve ? " selected='selected' " : "") + ">Utility Curve</option>";
                        opts += "<option value='" + mtPairwise + "' " + (data[COL_MT] == mtPairwise ? " selected='selected' " : "") + ">Pairwise Comparisons</option>";
                        opts += "<option value='" + mtPWAnalogous + "' " + (data[COL_MT] == mtPWAnalogous ? " selected='selected' " : "") + ">Pairwise with Given <%=ParseString("%%Impact%%")%></option>";
                        isDefVal = data[COL_MT] == DefaultMeasurementTypeAlt;
                    } else { // measurement types for objectives
                        opts += "<option value='" + mtPairwise + "' " + (data[COL_MT] == mtPairwise ? " selected='selected' " : "") + ">Pairwise Comparisons</option>";
                        opts += "<option value='" + mtDirect + "' " + (data[COL_MT] == mtDirect ? " selected='selected' " : "") + ">Direct</option>";
                    }
                    <%End If%>
                    <%End If%>
                    var cb = "<select style='width:" + (isDefVal ? "150px" : "135px") + ";margin:0px 0px 3px 0px;' onchange='onMeasureTypeChange(" + data[COL_ID] + "," + ((data[COL_PID])?data[COL_PID]:"-1") + ", this.value, " + data[COL_IS_ALT_WITH_NO_SOURCE] + ", this, " + data[COL_MT] + ");' " + (isReadOnly ? " disabled='disabled' " : "") + ">" + opts + "</select>";
                    var btn = isDefVal ? "" : getResetButton("onMeasureTypeChange", data[COL_ID], data[COL_PID], DefaultMeasurementTypeAlt, data[COL_IS_ALT_WITH_NO_SOURCE]);
                    $('td:eq(1)', row).html(isBlank(data) ? "" : "<nobr>" + cb + btn + "</nobr>");
                } else { $('td:eq(1)', row).html(""); }

                // measurement scale
                //if ((MTMode !== mmObj || isRisk) && data[COL_RISK_NODE_TYPE] !== 1) {
                if (data[COL_IS_TERMINAL] && MTMode !== mmObj || !data[COL_IS_TERMINAL] && MTMode !== mmAlt) {
                    if  (data[COL_MS_GUID] != "") {
                        var cb_title;
                        var is_red_sf_selected = false;
                        var opts = "";
                        var hasValue = false;
                        for (var i = 0; i < mscales.length; i++) {
                            if (mscales[i][IDX_MEASURE_TYPE] == data[COL_MT]) {
                                // mark scales dropdown red if step function is "undefined"
                                var opt_color = "color:black;";
                                if (data[COL_MT] == mtStep) {
                                    if (!(mscales[i][IDX_INTENSITIES]) || (mscales[i][IDX_INTENSITIES].length == 0) || (mscales[i][IDX_INTENSITIES].length == 1 && mscales[i][IDX_INTENSITIES][0][2] == 0 && mscales[i][IDX_INTENSITIES][0][4] == 1)) {
                                        opt_color = 'color:red;';
                                    }
                                }

                                if (opt_color != "color:black;" && mscales[i][IDX_GUID] == data[COL_MS_GUID]) {
                                    is_red_sf_selected = true;
                                    cb_title = "<%=ResString("msgStepIntervalsShouldBeSpecified")%>";
                                }
                                if (mscales[i][IDX_GUID] == data[COL_MS_GUID]) hasValue = true;
                                opts += "<option value='" + mscales[i][IDX_GUID] + "' " + (mscales[i][IDX_GUID] == data[COL_MS_GUID] ? " selected='selected' " : "") + " style='" + opt_color + "'>" + mscales[i][IDX_SCALE_NAME] + "</option>";
                            }
                        }

                        // add "Create New..." command
                        opt_color = "color:black;";
                        opts += "<option value='' style='" + opt_color + "' disabled='disabled'>---------------------------------------</option>";
                        opts += "<option value='-1' style='" + opt_color + "'><%=ResString("optCreateNew")%></option>";

                        if (!hasValue) {
                            opts = "<option disabled selected value> -- select an option -- </option>" + opts;
                        }

                        var cb = "<select style='width:150px;margin:0px 0px 3px 0px;" + (is_red_sf_selected ? "color: red;" : "") + "' onchange='onMeasureScaleChange(" + data[COL_ID] + "," + ((data[COL_PID])?data[COL_PID]:"-1") + ", this.value, " + data[COL_MT] + ", " + data[COL_IS_ALT_WITH_NO_SOURCE] + ");' " + (isReadOnly ? " disabled='disabled' " : "") + " " + ((cb_title) ? " title='" + cb_title + "' " : "") +">" + opts + "</select>";                    
                        $('td:eq(2)', row).html(isBlank(data) ? "" : cb);
                    } else {
                        $('td:eq(2)', row).empty();
                    }

                    if (data[COL_MT] == mtPWAnalogous) {
                        var hasVs = data[COL_KNOWN_VALUE_STRING] !== "";
                        var is_ns = data[COL_IS_ALT_WITH_NO_SOURCE];
                        var sV = hasVs ? data[COL_KNOWN_VALUE_STRING] : "<% = ResString("lblEditGivenLikelihoods") %>";
                        $('td:eq(2)', row).html("<span style='" + (hasVs ? "" : "color: red;") + " cursor: pointer;' onclick='onEditGivenLikelihoodsClick(" + data[COL_ID] + "," + is_ns + ");'>" + sV + "</span>").css("padding-left", "4px");
                    }

                } else {
                    $('td:eq(2)', row).empty();
                }

                // highlight "copy From" node
                row.setAttribute("data-nid", data[COL_ID]);
            },
            onContentReady: function (e) {
                $(".dx-treelist-header-panel").css("border-left", "2px solid #ccc");
            }
        });
        
        resizePage();
    }    
    /* end DataTable */

    function showNodeMenu(img, op, node_id) {
        switch (op+"") {
            case "m":
                toggleNodeProp(img, node_id, COL_NODE_SYNTH_TYPE, op);
                break;
            case "s":
                toggleNodeProp(img, node_id, COL_IS_STRUCTURAL_ADJUST, op);
                break;
            case "c":
                toggleNodeProp(img, node_id, COL_RISK_NODE_TYPE, op);                
                break;
            case "b":
                //TODO: show statistical data dialog
                onEditStatDataDlg(img, node_id);
                break;
        }                 
    }

    var tmp_node_id = 0;
    var tmp_node_ns = false;

    function onEditGivenLikelihoodsClick(parent_node_id, is_ns) {
        tmp_node_id = parent_node_id;
        tmp_node_ns = is_ns;
        callAjax("action=get_known_likelihoods&parent_node_id=" + parent_node_id, onGetKnownLikelihoodsCompleted, undefined, false);
    }

    var popupClosed = false;

    function onGetKnownLikelihoodsCompleted(data) {
        popupClosed = false;
        if ((data)) {
            var rd = eval(data);
            if ((rd)) {
                
                $("#popupContainer").dxPopup({
                    contentTemplate: function() {
                        return $("<div id='divLikelihoodGrid'></div><div style='text-align: center;'><div id='btnOKDlg' style='margin-top:1em;'></div></div>");
                    },
                    width: 525,
                    height: 425,
                    showTitle: true,
                    title: "<%=If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ResString("lblKnownLikelihoodsHeader"), ResString("lblKnownImpactsHeader"))%>" + " \"" + getNodeById(tmp_node_id, tmp_node_ns)[COL_NAME] + "\"",
                    dragEnabled: true,
                    shading: false,
                    closeOnOutsideClick: true,
                    resizeEnabled: false,
                    showCloseButton: true,
                    onHiding: function (e) {
                        var table_likelihoods = (($("#divLikelihoodGrid").data("dxDataGrid"))) ? $("#divLikelihoodGrid").dxDataGrid("instance") : null;
                        if (table_likelihoods !== null && table_likelihoods.hasEditData()) table_likelihoods.saveEditData();                        
                    }
                });
                $("#popupContainer").dxPopup("show");
                $("#btnOKDlg").dxButton({
                    text: "<% =JS_SafeString(ResString("btnOK")) %>",
                    icon: "",
                    type: "normal",
                    width: 100,
                    onClick: function (e) {
                        popupClosed = true;
                        closePopup();
                    }
                });
                $("#divLikelihoodGrid").dxDataGrid({
                    dataSource: rd[1],
                    keyExpr: "ID",
                    height: "300px",
                    columns: [
                        {caption : "Name", "dataField": "name", "encodeHtml" : false }, 
                        {caption : "Value", "dataField": "value", "dataType": "number", "alignment" : "right", cellTemplate : cellTemplate = function(element, info) {            
                            if (info.text*1 >= 0) {
                                var sVal = "";
                                element.append(info.text);
                            } else {
                                element.append("");
                            }
                        }
                        }
                    ],
                    editing: {
                        mode: "cell", //"row", //"batch",
                        useIcons: true,
                        allowUpdating: !isReadOnly
                    },
                    onRowUpdated: function (e) {
                        callAjax("action=set_known_likelihood&parent_node_id=" + tmp_node_id + "&node_id=" + e.data.ID + "&value=" + e.data.value, onSetKnownLikelihoodCompleted, undefined, false);
                        closePopup();
                    }
                });

            }
        }
    }

    function onSetKnownLikelihoodCompleted(data) {
        if (!popupClosed) onGetKnownLikelihoodsCompleted(data);
        if ((data)) {
            var rd = eval(data);
            if ((rd)) {
                refreshNodeData(rd[2]);
            }
        }
    }

    function refreshNodeData(node) {
        // update selected node only
        for (var i = 0; i < nodes.length; i++) {
            if ((nodes[i][COL_ID] == node[COL_ID]) && (nodes[i][COL_IS_ALT_WITH_NO_SOURCE] == node[COL_IS_ALT_WITH_NO_SOURCE])) {
                var pid = nodes[i][COL_PID];
                //nodes[i] = node.slice(0); // clone the node
                nodes[i] = JSON.parse(JSON.stringify(node));
                nodes[i][COL_PID] = pid;
            }
        }
        initTable();
    }

    var inputWidth = 100;
    var defInputPeriod = "<input type='text' class='input_period' style='width:" + inputWidth + "px; margin:2px;' >";
    var defInputData = "<input type='text' class='input_data' style='width:" + inputWidth + "px; margin:2px; text-align:right;' >";

    function onEditStatDataDlg(img, node_id) {
        cancelled = false;
        
        curStatDataImg = img;       
        
        var node = getNodeById(node_id);
        var _title = "Edit Statistical Data for \"" + node[COL_NAME] + "\"";

        dlg_edit_stat = $("#divEditStatData").dialog({
            autoOpen: false,
            modal: true,
            width: 800,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: _title,
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"Save", click: function() { dlg_edit_stat.dialog( "close" ); }},
                      { text:"Cancel", click: function() { cancelled = true; dlg_edit_stat.dialog( "close" ); }}],
            open: function() {
                $("body").css("overflow", "hidden");                
                $("#lblStatDataTitle").html("\"" + node[COL_NAME] + "\"");

                $("#divPeriodsHeaders").html(defInputPeriod);
                $("#divPeriodsData").html(defInputData);

                getStatData(node_id);
            },
            close: function() {
                if (!cancelled) {
                    saveStatData(node_id);
                }
                $("body").css("overflow", "auto");
            }
        });
        dlg_edit_stat.dialog("open");
    }

    function getStatData(node_id) {
        sendCommand("action=" + ACTION_GET_STAT_DATA + "&node_id=" + node_id, true);
    }

    function initNodeStatDataUI(data) {
        if (data.length > 0) {
            var sPeriods = "";
            var sData = "";
            
            for (var i = 0; i < data.length; i++) {
                sPeriods += "<input type='text' class='input_period' style='width:" + inputWidth + "px; margin:2px;' value='" + data[i][0] + "' >";
                sData += "<input type='text' class='input_data' style='width:" + inputWidth + "px; margin:2px; text-align:right;' value='" + data[i][1] + "' >";
            }

            $("#divPeriodsHeaders").html(sPeriods);
            $("#divPeriodsData").html(sData);
        }
    }

    function addStatPeriod() {
        $("#divPeriodsHeaders").html($("#divPeriodsHeaders").html() + defInputPeriod);
        $("#divPeriodsData").html($("#divPeriodsData").html() + defInputData);
    }

    function saveStatData(node_id) {
        var data = "";
        var titles = [];
        var values = [];
        $("input.input_period").each(function() {
            titles.push(this.value.trim());
        });
        $("input.input_data").each(function() {
            values.push(this.value.trim());
        });
        var paramIndex = 0;
        for (var i = 0; i < titles.length; i++) {
            if (titles[i] != "") {
                data += "&p" + paramIndex + "=" + encodeURIComponent(titles[i]);
                data += "&v" + paramIndex + "=" + encodeURIComponent(values[i]);
                paramIndex += 1;
            }
        }
        sendCommand("action=" + ACTION_SAVE_STAT_DATA + "&node_id=" + node_id + "&count=" + paramIndex + data, true);
    }

    function toggleNodeProp(img, node_id, col, op) {
        var nodes_len = nodes.length;
        for (var i = 0; i < nodes_len; i++) {
            if (nodes[i][COL_ID] == node_id) {
                var val = nodes[i][col];
                var toggle_val = (val == 1 ? 0: 1);
                nodes[i][col] = toggle_val;
                img.src = "../../Images/ms/button_" + op + (val == 1 ? "_" : "") + ".png";
                if (op == "c") {
                    $("#lblNodeName" + node_id).toggleClass("categorical");
                }
                sendCommand("action=" + ACTION_SET_NODE_PROPERTY + "&node_id=" + node_id + "&prop=" + op + "&val=" + toggle_val, false);
                i = nodes_len;
            }
        }       
    }

    function startCopyMS(node_id, is_ns) {
        var node = getNodeById(node_id, is_ns);
        copyFromNodeID = node_id;
        copyFromNodeIsTerminal = node[COL_IS_TERMINAL];
        copyFromIsEvent = node[COL_IS_ALT_WITH_NO_SOURCE];
        $("#divCopyToTooltip").show();
        $("table tr[data-nid]").css({"background-color" : "#ffffff"});
        $("input[data-has-cb]").prop("checked", false).hide();        
        $("table tr[data-nid='" + node_id + "']").css({"background-color" : "#d0d0d0"});        
        $("input[data-has-cb][data-has-cb!='" + node_id + "'][data-term='" + node[COL_IS_TERMINAL] + "'][data-is-ns='" + node[COL_IS_ALT_WITH_NO_SOURCE] + "']").show();
        $("#lblCopyToHint").html(node[COL_IS_TERMINAL] == 1 ? "<%=ResString("msgCopyMeasurementTypes")%>" : "<%=ResString("msgCopyMeasurementTypesObjectives")%>");
        resizePage();
    }

    function selectAllToPasteMS(chk) {
        $("input[data-has-cb!='" + copyFromNodeID+ "'][data-term='" + copyFromNodeIsTerminal + "'][data-is-ns='" + copyFromIsEvent + "']").prop("checked", chk);
    }

    function cancelCopyMS() {
        $("table tr[data-nid]").css({"background-color" : "#ffffff"});
        $("input[data-has-cb]").prop("checked", false).hide();
        $("#divCopyToTooltip").hide();
        resizePage();
    }

    function proceedCopyMS() {
        var checked_list = "";
        $("input[data-has-cb]:checked").each(function () {
            checked_list += (checked_list == "" ? "" : ",") + this.getAttribute("data-has-cb");
        });
        if ((checked_list != "")) sendCommand("action=copy_mt_to&from=" + copyFromNodeID + "&is_ns=" + copyFromIsEvent + "&to=" + checked_list, true);
        cancelCopyMS();
    }

    /* Operations */
    var show_pw_lkhd_confirm = <% = JS_SafeString(GetCookie("COOKIE_SHOW_PW_LKHD_CONFIRM", "true"))%>;

    function onMeasureTypeChange(node_id, pid, mt, is_event_with_no_source, sender, orig_mt) {
        if (mt + "" == "11") {
            notImplemented();
            if ((sender) && (orig_mt)) sender.value = orig_mt;
            return false;
        }

        <%If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
        if (mt == mtPairwise && show_pw_lkhd_confirm) {
            var dlg = dxDialog("<span><%=SafeFormString(ResString("msgLikelihoodPairwiseWarning"))%></span></br></br><nobr><input type='checkbox' class='input checkbox' id='cbDontShowAgain' /><label for='cbDontShowAgain'><%=SafeFormString(ResString("msgDoNotShowAgain"))%></label>", "var newVal = !$('#cbDontShowAgain').prop('checked'); if (newVal != show_pw_lkhd_confirm) { show_pw_lkhd_confirm = newVal; document.cookie = 'COOKIE_SHOW_PW_LKHD_CONFIRM=' + newVal.toString(); }", "", "<%=ResString("msgWarning")%>");
            //dlg.dxPopup("instance").option("width", "50%");
        }
        <%End If%>

        sendCommand("action=" + ACTION_MEASURE_TYPE + "&node=" + node_id + "&pid=" + pid + "&mt=" + mt + "&is_ns=" + is_event_with_no_source, true);
    }

    var cur_node_id = -1;
    var cur_node_pid = -1;
    var cur_node_is_ns = -1;

    function onMeasureScaleChange(node_id, pid, ms, mt, is_event_with_no_source) {
        if (ms == "-1") { // option "Create New..."
            cur_node_id = node_id;
            cur_node_pid = pid;
            cur_node_is_ns = is_event_with_no_source;
            selectedTypeID = mt;
            selectedScaleID = "";
            onManageScalesDlg(false);
            onCreateNewScale();
        } else {
            sendCommand("action=" + ACTION_MEASURE_SCALE + "&node=" + node_id + "&pid=" + pid + "&ms=" + ms + "&is_ns=" + is_event_with_no_source, true);
        }
    }

    function onNumComparisonsChange(node_id, pid, value, is_event_with_no_source) {
        sendCommand("action=" + ACTION_NUM_COMPARISONS + "&node=" + node_id + "&pid=" + pid + "&value=" + value + "&is_ns=" + is_event_with_no_source, true);
    }

    function onPairwiseTypeChange(node_id, pid, value, is_event_with_no_source) {
        sendCommand("action=" + ACTION_PAIRWISE_TYPE + "&node=" + node_id + "&pid=" + pid + "&value=" + value + "&is_ns=" + is_event_with_no_source, true);
    }

    function onDisplayOptionChange(node_id, pid, value, is_event_with_no_source) {
        sendCommand("action=" + ACTION_DISPLAY_OPTION + "&node=" + node_id + "&pid=" + pid + "&value=" + value + "&is_ns=" + is_event_with_no_source, true);
    }

    function onForceGraphicalChange(node_id, pid, value, is_event_with_no_source) {
        sendCommand("action=" + ACTION_FORCE_GRAPHICAL + "&node=" + node_id + "&pid=" + pid + "&value=" + value + "&is_ns=" + is_event_with_no_source, true);
    }

    function onPreviewPipeStep(node_id, pipe_step, is_terminal_node) {
        var URL = "<% =JS_SafeString(PageURL(_PGID_EVALUATION, If(App.isEvalURL_EvalSite, "mode", "action") + "=getstep", App.isEvalURL_EvalSite)) %>&mt_type=" + (is_terminal_node ? "1" : "0") + "&node_id=" + node_id;
        //showLoadingPanel();
        //document.location.href = URL;
        openProjectURLWhenGecko(URL);
        return false;
    }

<%--    function onPreviewPipeStep(node_id, pipe_step, is_terminal_node) {
        var sHTMLParams = "&action=getstep&mt_type=" + (is_terminal_node ? "1" : "0") + "&node_id=" + node_id;
        NavigateToPage(<%=_PGID_EVALUATION%>, sHTMLParams);
    }

    function NavigateToPage(pgid, params) {     
        if ((window.opener) && (typeof window.opener.onOpenPage)!="undefined") { window.opener.onOpenPage(pgid, params); return true; } 
        if ((window.parent) && (typeof window.parent.onOpenPage)!="undefined") { window.parent.onOpenPage(pgid, params); return true; } 
        return false;
    }--%>

    <%--function onSetMTforAllClick(mt) {
        dxConfirm("<%=JS_SafeString(ResString("msgAreYouSure"))%>", 'sendCommand("action=' + ACTION_SET_MEASURE_TYPE_FOR_ALL + '&mt=' + mt + '", true);');
    }--%>
    /* end Operations */

    /* Manage Scales */

    var selectedTypeID = 1;

    function cbMeasurementMethodChange(value) {
        if (selectedTypeID * 1 != value * 1) {
            selectedTypeID = value * 1;
            selectedScaleID = "";
            initSelectedTypeScales();
        }
    }

    function cbMeasurementScaleChange(value) {
        if (selectedScaleID + "" != value + "") {
            selectedScaleID = value + ""; 
            showScaleData(selectedScaleID);
        }
    }

    var selectedScaleID = "";

    function scalesCount(mt) {
        var retVal = 0;
        for (var i = 0; i < mscales.length; i++) {
            if (mscales[i][IDX_MEASURE_TYPE] == selectedTypeID) {
                retVal += 1;
            }
        }
        return retVal;
    }

    function initSelectedTypeScales() {
        // fill in measurement scales list 
        var cb = document.getElementById("cbMeasurementScale");
        while (cb.firstChild) {
            cb.removeChild(cb.firstChild);
        }

        for (var i = 0; i < mscales.length; i++) {
            if (mscales[i][IDX_MEASURE_TYPE] == selectedTypeID) {
                var v = mscales[i];
                if (selectedScaleID == "") selectedScaleID = v[IDX_GUID];
                var opt = createOption(v[IDX_GUID], v[IDX_SCALE_NAME]);
                if (selectedScaleID == v[IDX_GUID]) opt.selected = true;
                cb.appendChild(opt);                
            }
        }

        var scale = getScaleById(selectedScaleID);         
        if (selectedScaleID != "" && (scale)) {
            showScaleData(selectedScaleID);

            // disable scales editor
            //editorEnabled(false);        

            if (scale[IDX_MEASURE_TYPE] == mtRatings || scale[IDX_MEASURE_TYPE] == mtStep || scale[IDX_MEASURE_TYPE] == mtPWOutcomes) { $(".assess_button").show(); } else { $(".assess_button").hide(); }
        }

        updateScaleToolbar();
    }

    function updateScaleToolbar() {
        $("#btnDeleteScale").prop("disabled", document.getElementById("cbMeasurementScale").options.length < 2 || document.getElementById("cbScaleIsDefault").checked);
    }

    function onManageScalesDlg(canUserSwitch) {
        cancelled = false;

        // disable elements if user can't switch MT or MS
        $("#cbMeasurementMethod").prop("disabled", !canUserSwitch);
        $("#cbMeasurementScale").prop("disabled", !canUserSwitch);
        $("#btnCreateScale").prop("disabled", !canUserSwitch);
        $("#btnCloneScale").prop("disabled", !canUserSwitch);
        $("#btnDeleteScale").prop("disabled", !canUserSwitch);

        var _title = "Manage Scales";
        var scale = getScaleById(selectedScaleID);

        dlg_editor_main = $("#divManageScales").dialog({
            autoOpen: true,
            modal: true,
            width: 850,
            height: 540,
            dialogClass: "no-close",
            closeOnEscape: true,
            bgiframe: true,
            title: _title,
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [{ text:"Copy to clipboard", "class": 'copy_button', click: function() { copyScaleToClipboard(selectedScaleID); }},
                      { text:"Paste from clipboard", "class": 'paste_button editor_element', "enabled" : false, click: function() { pasteScaleFromClipboard(); }},
                      { text:"<%=If(PM.IsRiskProject, If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("Assess %%Likelihoods%%"), ParseString("Assess %%Impact%%")), "Assess Priorities")%>", "class": 'assess_button', click: function() { assessPriorities(selectedScaleID); }},
                      //{ text:"Save", click: function() { dlg_editor_main.dialog( "close" ); }},
                      { text:"Close", click: function() { dlg_editor_main.dialog( "close" ); }}],
            open: function() {
                $("body").css("overflow", "hidden");
                
                initSelectedTypeScales();                
            },
            close: function() {
                $("body").css("overflow", "auto");
                initTable();
            }
        });
        dlg_editor_main.dialog("open");
        $('.copy_button').css('margin', '0px 4px');
        $('.paste_button').css('margin', '0px 4px');
        $('.assess_button').css('margin', '0px 84px');

        if (!canUserSwitch) $("#btnDeleteScale").prop("disabled", true);
    }

    function createOption(guid, name) {
        var opt = document.createElement("option");
        opt.value= guid;
        opt.innerHTML = name;
        return opt;
    }

    function showScaleData(scale_id) {
        var scale = getScaleById(scale_id);
        if ((scale)) {
            LoadInfodoc(scale);

            $("#cbMeasurementMethod").val(scale[IDX_MEASURE_TYPE]);

            if (selectedTypeID == mtRatings || selectedTypeID == mtPWOutcomes) { $("#trRatings").show(); } else { $("#trRatings").hide(); }
            if (selectedTypeID == mtStep) { $("#trStep").show(); } else { $("#trStep").hide(); }
            if (selectedTypeID == mtUtilityCurve) { $("#trPlot").show(); } else { $("#trPlot").hide(); }

            var tb = document.getElementById("lblScaleName");
            tb.innerHTML = scale[IDX_SCALE_NAME];
            var lblPriority = "<%=If(Not PM.IsRiskProject, "Priority", If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%Likelihood%%"), ParseString("%%Impact%%"))) %>";

            switch (scale[IDX_MEASURE_TYPE]*1) {
                case mtRatings:
                case mtPWOutcomes:
                    var hdr = "<tr><th>Intensity Name</th><th>" + lblPriority + "</th><th>Description</th><th>Actions</th></tr>";                                 
                    var cnt = "";
                    $("#tblScaleContent").html("");
                    for (var i = 0; i < scale[IDX_INTENSITIES].length; i++) {
                        var v = scale[IDX_INTENSITIES][i];
                        var sBar = htmlGraphBarWithValue(v[INT_VALUE], 1, (v[INT_VALUE]).toFixed(5), "<%=ImagePath()%>", 130, 10, "#cccccc");
                        cnt += "<tr class='" + (i % 2 == 0 ? "grid_row" : "grid_row_alt") + "'><td>" + v[INT_NAME] + "</td><td style='text-align: center;'>" + sBar + "</td><td>" + v[INT_COMMENT] + "</td><td style='text-align: center;'><a href='' onclick='addIntensity(\"" + v[INT_ID] + "\",\"" + v[INT_NAME] + "\",\"" + v[INT_VALUE] + "\",\"" + v[INT_COMMENT] + "\"); return false;' title='Edit'><i class='fas fa-pencil-alt ec-icon'></i></a>&nbsp;<a href='' onclick='delIntensity(\"" + v[INT_ID] + "\"); return false;' title='Delete'><i class='fas fa-trash-alt ec-icon'></i></a></td></tr>";
                    }
                    $("#tblScaleContent").html(hdr + cnt);

                    $("#cbScaleIsDefault").prop("checked", scale[IDX_IS_DEFAULT] != 0);
                    $("#cbScaleHidePriorities").prop("checked", scale[IDX_USE_DIRECT_RATINGS] == 0);
                    break;
                case mtUtilityCurve:
                    $("#tbLow").val(scale[IDX_INTENSITIES][0]);
                    $("#tbHigh").val(scale[IDX_INTENSITIES][1]);
                    $("#tbCurvature").val(scale[IDX_INTENSITIES][2]);
                    $("#tbCurvature").prop("disabled", scale[IDX_INTENSITIES][4]); //disabled if Linear
                    $("#lblCurvature").toggleClass("toolbar-label-disabled", scale[IDX_INTENSITIES][4]);
                    var rbInc = $('input:radio[name=rbIncreasing]');
                    rbInc.filter('[value=' + (scale[IDX_INTENSITIES][3] ? 0 : 1) + ']').prop('checked', true);
                    var rbLin = $('input:radio[name=rbLinear]');
                    rbLin.filter('[value=' + (scale[IDX_INTENSITIES][4] ? 0 : 1) + ']').prop('checked', true);
                    buildUtilityCurve(scale[IDX_INTENSITIES]);
                    break;
                case mtStep:
                    $("#cbSFLinear").prop("checked", scale[IDX_USE_DIRECT_RATINGS]);
                    buildStepFunction(scale);
                    var hdr = "<tr><th>Name</th><th>Lower bound data</th><th>" + lblPriority + "</th><th>Actions</th></tr>";                                 
                    var cnt = "";
                    $("#tblSteps").html("");
                    scale[IDX_INTENSITIES].sort(sortByLow);
                    for (var i = 0; i < scale[IDX_INTENSITIES].length; i++) {
                        var v = scale[IDX_INTENSITIES][i];
                        var sBar = htmlGraphBarWithValue(v[4], 1, (v[4]).toFixed(5), "<%=ImagePath()%>", 130, 10, "#cccccc");
                        cnt += "<tr><td>" + v[1] + "</td><td style='text-align: right;'>" + v[2] + "</td><td style='text-align: center;'>" + sBar + "</td><td style='text-align: center;'><a href='' onclick='addInterval(\"" + v[5] + "\",\"" + v[1] + "\",\"" + v[2] + "\",\"" + v[4] + "\"); return false;' title='Edit'><i class='fas fa-pencil-alt'></i></a>&nbsp;<a href='' onclick='delInterval(\"" + v[5] + "\"); return false;' title='Delete'><i class='fas fa-trash-alt'></i></a></td></tr>";
                    }
                    $("#tblSteps").html(hdr + cnt);

                    $("#cbScaleIsDefault").prop("checked", scale[IDX_IS_DEFAULT] != 0);
                    $("#cbScaleHidePriorities").prop("checked", scale[IDX_USE_DIRECT_RATINGS] == 0);
                    break;
            }
        }
        updateScaleToolbar();
    }

    /* Regular Utility Curve / Step Function */
    var last_src = "";
    
    function sortByLow(a, b){
        return ((a[2] > b[2]) ? -1 : ((a[2] < b[2]) ? 1 : 0));    
    }

    function buildStepFunction(scale) {
        var vals = "";
        var N = scale[IDX_INTENSITIES].length;
        scale[IDX_INTENSITIES].sort(sortByLow);
        for (var i = N - 1; i >= 0 ; i--) {
            var v = scale[IDX_INTENSITIES][i];
            var idx = N - i - 1;
            var H = i > 0 ? scale[IDX_INTENSITIES][i-1][2].toFixed(5) : "<%=Integer.MaxValue%>"; // low of prev step
            vals += "&l" + idx + "=" + v[2].toFixed(5) + "&h" + idx + "=" + H + "&v" + idx + "=" + v[4].toFixed(5);
        }
        var src = "<%=_URL_EVALUATE%>" + "UC_Chart.aspx?type=sf&name=Step%20Function&linear=" + (scale[IDX_USE_DIRECT_RATINGS] ? 1 : 0) + "&cnt=" + scale[IDX_INTENSITIES].length + vals + "&rnd=" + Math.round(10000 * Math.random());
        if (src != last_src) { last_src = src; setTimeout('document.getElementById("frmSF").src="' + src +'";', 30); }
    }

    function buildUtilityCurve(params) {
        var src = "<%=_URL_EVALUATE%>" + "UC_Chart.aspx?type=uc&name=Utility%20Curve&low=" + params[0] + "&high=" + params[1] + "&curv=" + params[2] + "&decr=" + (!params[3]) + "&linear=" + params[4] + "&rnd=" + Math.round(10000 * Math.random());
        if (src != last_src) { last_src = src; setTimeout('document.getElementById("frmUC").src="' + src +'";', 30); }
    }
    /* end Regular Utility Curve / Step Function */

    var curIntID = "";

    function addIntensity(id, name, value, comment) {
        curIntID = id;
        var editor = "<table class='text cell-border hover'><tr><th>Intensity Name</th><th>Value</th><th>Description</th></tr>";
        editor += "<tr id='trEditor'><td><input type='text' id='tbIntencityName' value='" + (id == "" ? "Enter intensity name here" : name) + "' class='editor_element' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' ></td>";
        editor += "<td><input type='text' id='tbIntencityValue' class='editor_element as_number' value='" + (id == "" ? "0.00000" : value) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' ></td>";
        editor += "<td><input type='text' class='editor_element' id='tbIntencityDescr' value='" + (id == "" ? "" : comment) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' ></td>";        
        editor += "</table>";
        dxDialog(editor, "doEditIntensity();", ";", id == "" ? "Add Intensity" : "Edit Intensity");
        setTimeout(id == '' ? "$('#tbIntencityName').select();" : "$('#tbIntencityValue').select();", 500);
    }

    function doEditIntensity() {
        var sName = $('#tbIntencityName').val();
        var sValue = $('#tbIntencityValue').val();
        var sDescr = $('#tbIntencityDescr').val();
        sendCommand("action=edit_int&id=" + curIntID + "&ms=" + selectedScaleID + "&name=" + encodeURIComponent(sName.trim()) + "&val=" + sValue.trim() + "&descr=" + encodeURIComponent(sDescr.trim()), true);
    }

    function addInterval(id, name, low, value) { //id = comment
        curIntID = id;
        var editor = "<table class='text cell-border hover'><tr><th>Name</th><th>Lower bound data</th><th>Value</th></tr>";
        editor += "<tr id='trEditor'><td><input type='text' id='tbIntencityName' value='" + (id == "" ? "Enter step name here" : name) + "' class='editor_element' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' ></td>";
        editor += "<td><input type='text' id='tbLowValue' class='editor_element as_number' value='" + (id == "" ? "0.00000" : low) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' ></td>";
        editor += "<td><input type='text' id='tbIntencityValue' class='editor_element as_number' value='" + (id == "" ? "0.00000" : value) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' ></td>";
        editor += "</table>";
        dxDialog(editor, "doEditInterval();", ";", id == "" ? "Add Step" : "Edit Step");
        setTimeout(id == '' ? "$('#tbIntencityName').select();" : "$('#tbLowValue').select();", 500);
    }

    function doEditInterval() {
        var sName  = $('#tbIntencityName').val();
        var sLow   = $('#tbLowValue').val();
        var sValue = $('#tbIntencityValue').val();
        sendCommand("action=edit_sf_int&id=" + curIntID + "&ms=" + selectedScaleID + "&name=" + encodeURIComponent(sName.trim()) + "&val=" + sValue.trim() + "&low=" + sLow.trim(), true);
    }

    function dialogHandleKey() {
        //var e  = (window.event) ? window.event : "undefined";
        //if ((e)) {
        //    if (e.keyCode == KEYCODE_ENTER) $("#jDialog_btnOK").last().click();
        //    if (e.keyCode == KEYCODE_ESCAPE) $("#jDialog_btnCancel").last().click();
        //}
    }

    function delIntensity(id) {
        sendCommand("action=del_int&ms=" + selectedScaleID + "&id=" + id, true);
    }

    function delInterval(id) {
        sendCommand("action=del_sf_int&ms=" + selectedScaleID + "&id=" + id, true);
    }

    function onCreateNewScale() {
        var suggestedName = "Scale for " + (cur_node_id >= 0 ? getNodeById(cur_node_id, cur_node_is_ns)[COL_NAME] : "...");
        var editor = "<input type='text' class='editor_element' id='tbNewScaleName' style='width: 300px;' value='" + suggestedName + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' >";
        dxDialog(editor, "doCreateNewScale();", ";", "Create New ...");
        $("#tbNewScaleName").select();
    }

    function doCreateNewScale() {
        sendCommand("action=new_scale&mt=" + selectedTypeID + "&name=" + encodeURIComponent($("#tbNewScaleName").val()), true);
    }

    function editScaleName() {
        var s = getScaleById(selectedScaleID);
        var editor = "<input type='text' style='width: 100%;' class='editor_element' id='tbEditScaleName' value='" + s[IDX_SCALE_NAME] + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeyup='dialogHandleKey();' >";
        dxDialog(editor, "doEditScaleName();", ";", "Edit Name ...");        
    }

    function doEditScaleName() {
        sendCommand("action=scale_name&mt=" + selectedTypeID + "&ms=" + selectedScaleID + "&name=" + encodeURIComponent($("#tbEditScaleName").val()), true);
    }
    
    function onCloneScale() {
        sendCommand("action=clone_scale&mt=" + selectedTypeID + "&ms=" + selectedScaleID, true);
    }

    function onDeleteSelectedScale() {
        dxDialog(replaceString("{0}", getScaleById(selectedScaleID)[IDX_SCALE_NAME], "<%=JS_SafeString(ResString("msgSureDeleteCommon"))%>"), "doDeleteSelectedScale();", ";", "<%=ResString("titleConfirmation") %>");
    }

    function doDeleteSelectedScale() {
        sendCommand("action=delete_scale&mt=" + selectedTypeID + "&ms=" + selectedScaleID, true);
    }
    
    function copyScaleToClipboard(scale_id) {
        switch (selectedTypeID) {
            case mtRatings:
            case mtPWOutcomes:
                copyDataToClipboard(GetRSText(getScaleById(scale_id)));       
                break;
            case mtStep:
                copyDataToClipboard(GetSFText(getScaleById(scale_id)));       
                break;
            case mtUtilityCurve:
                copyDataToClipboard(GetUCText(getScaleById(scale_id)));       
                break;
        }
    }

    function setScaleDefault(chk) {
        sendCommand("action=scale_default&mt=" + selectedTypeID + "&ms=" + selectedScaleID + "&chk=" + chk, true);
    }

    function setScaleHidePriorities(chk) {
        if (chk) dxDialog("<div style='max-width: 500px;'><%=JS_SafeString(ResString("msgRatingsDirectValues"))%></div>", "", false, "<%=ResString("lblInformation") %>");
        sendCommand("action=scale_hide&mt=" + selectedTypeID + "&ms=" + selectedScaleID + "&chk=" + chk, true);
    }

    function setSFLinear(chk) {
        sendCommand("action=scale_sf_linear&mt=" + selectedTypeID + "&ms=" + selectedScaleID + "&chk=" + chk, true);
    }

    function onUCParamChange(param, value) {
        if (param == "lin") {
            $("#tbCurvature").prop("disabled", value); //disabled if Linear
            if (value === 1) $("#tbCurvature").val(0);
        }
        $("#lblCurvature").toggleClass("toolbar-label-disabled", value);
        sendCommand("action=uc_param&param=" + param + "&mt=" + selectedTypeID + "&ms=" + selectedScaleID + "&value=" + value, true);
    }

    function pasteScaleFromClipboard() {
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteData(data);
        } else {
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteData);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        var pasteDialog = DevExpress.ui.dialog.custom({
                            title: resString("titlePasteFromClipboard"),
                            message: "<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='38' rows='6'></textarea></pre>",
                            buttons: [{ text: "<% =JS_SafeString(ResString("btnPaste"))%>", onClick : function () { return true; }}, {text: "<% =JS_SafeString(ResString("btnCancel")) %>", onClick : function () { return false; }}]
                        });
                        pasteDialog.show().done(function(dialogResult){
                            if (dialogResult) commitPasteFF();
                        });
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }            
        }
    }

    function pasteData(data) {
        sendCommand("action=paste_scale&mt=" + selectedTypeID + "&ms=" + selectedScaleID + "&data=" + encodeURIComponent(data), true);
    }

    /* Edit Infodocs */
    function LoadInfodoc(scale) {
        var src = "";

        if ((scale)) {
            src = replaceString('%%%%', <%=Cint(reObjectType.MeasureScale) %>, '<% =PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid={1}", "%%%%", "' + scale[IDX_GUID] + '")) %><% =GetTempThemeURI(True) %>&r=' + Math.round(10000*Math.random()));
        }

        var b = document.getElementById("btnEditInfodoc");
        if ((b)) b.disabled = !((scale));

        var f = document.getElementById("frmInfodoc");
        if ((f)) {
            if (src=="") {
                f.src = "";
            } else {
                if ((f.contentWindow)) f.contentWindow.document.body.innerHTML = "<div style='width:99%; height:99%; background: url(<%=ImagePath %>devex_loading.gif) no-repeat center center;'>&nbsp</div>";
                setTimeout('document.getElementById("frmInfodoc").src="' + src +'";', 30);
            }
        }
    }

    function EditInfodoc() {
        var scale = getScaleById(selectedScaleID);
        if ((scale)) {
            <%--CreatePopup('<% =PageURL(_PGID_RICHEDITOR) %>?type=' + <% =Cint(reObjectType.MeasureScale) %> + '&field=infodoc&guid=' + scale[IDX_GUID] + '&<% =_PARAM_TEMP_THEME+"="+_THEME_SL %>', 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);--%>
            OpenRichEditor('?type=' + <% =Cint(reObjectType.MeasureScale) %> + '&field=infodoc&guid=' + scale[IDX_GUID]);
        }
    }

    function onRichEditorRefresh(empty, infodoc, callback_param) {        
        window.focus();
        var f = document.getElementById("frmInfodoc");
        if ((f)) {
            LoadInfodoc(getScaleById(selectedScaleID));
        }
    }              
    /* end Edit Infodocs*/

    /* Assess Priorities */
    function assessPriorities(scale_id) {
        sendCommand("action=assess_priorities_options&mt=" + selectedTypeID + "&ms=" + selectedScaleID, true);
    }

    var optionPWType = 2;
    var optionNumOfJudgments = 2;
    var optionForceGraphical = 0;
    var optionShowInconsistency = true;
    var optionShowMultiPairwise = true;
    var optionZeros = "";
    var optionsAll = "";

    function onAssessPrioritiesOptions() {
        optionNumOfJudgments = $('input[name=grpPairs]:checked').val() * 1;
        optionPWType = $('input[name=grpPW]:checked').val() * 1;
        if ((document.getElementById("optEvalPWForceGraphical"))) optionForceGraphical = document.getElementById("optEvalPWForceGraphical").checked ? 1 : 0;
        optionShowInconsistency = document.getElementById("optDispInconsist").checked;
        optionShowMultiPairwise = document.getElementById("optDispMultiPairwise").checked;

        numDiag = optionNumOfJudgments;
        grpPW = optionPWType;
        dispInc = optionShowInconsistency;
        dispMultiPW = optionShowMultiPairwise;

        sessionStorage.setItem("AssessPrioritiesNumDiag<%=App.ProjectID%>", optionNumOfJudgments.toString());
        sessionStorage.setItem("AssessPrioritiesGrpPW<%=App.ProjectID%>", optionPWType.toString());
        sessionStorage.setItem("AssessPrioritiesDispIncons<%=App.ProjectID%>", optionShowInconsistency.toString());
        sessionStorage.setItem("AssessPrioritiesDispMultiPW<%=App.ProjectID%>", optionShowMultiPairwise.toString());    

        sendCommand("action=assess_priorities_zeros&mt=" + selectedTypeID + "&ms=" + selectedScaleID, true);
    }

    function onAssessPrioritiesZeros() {
        optionZeros = "";    
        $('.cb_zeros:checkbox:checked').each(function() {
            optionZeros += (optionZeros == "" ? "" : ",") + this.getAttribute("data-id");
        });
        optionsAll = "NumberOfJudgments="+optionNumOfJudgments+"&ForceGraphical="+optionForceGraphical+"&Type="+optionPWType+"&ShowInconsistency="+(optionShowInconsistency?"Yes":"No")+"&ShowMultiPairwise="+(optionShowMultiPairwise?"Yes":"No")
        
        <%If PM.IsRiskProject And PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
        if (selectedTypeID == mtRatings || selectedTypeID == mtPWOutcomes || selectedTypeID == mtStep) {
            var sCertain = "<%=ResString("sCertainIntensityName")%>".toLowerCase();
            var tContainsCertain = false;
            var scale = getScaleById(selectedScaleID);

            for (var i = 0; i < scale[IDX_INTENSITIES].length; i++) {
                if (scale[IDX_INTENSITIES][i][INT_NAME].trim().toLowerCase() == sCertain) {
                    tContainsCertain = true;
                }
            }
            
            if (!tContainsCertain) {
                dxConfirm(replaceString("{0}", "<%=ResString("sCertainIntensityName")%>", "<%=JS_SafeString(ResString("msgAddCertainIntensity"))%>"), addCertain, proceedAssessPriorities);
            } else {
                proceedAssessPriorities();
            }
        } else {
            proceedAssessPriorities();
        }
        <%Else%>
        proceedAssessPriorities();
        <%End If%>
    }

    function addCertain() {
        sendCommand("action=assess_priorities_certain&mt=" + selectedTypeID + "&ms=" + selectedScaleID, true);
    }

    var frmAssess = null;
    function proceedAssessPriorities() {
        var w = Math.round($(window).width()-96);
        var h = Math.round($(window).height()-96);
        var conf_options = {
            "title": "Assess priorities",
            "messageHtml": "<iframe id='frmAssess' frameborder='0' style='border:0px; width:" + w + "px;height:" + h + "px;'></iframe>", 
        };
        frmAssess = DevExpress.ui.dialog.custom(conf_options);
        frmAssess.show();
        var frm = $("#frmAssess");
        if ((frm) && (frm.length)) {
            frm.parent().parent().css("padding", "10px").next().hide();
            initFrameLoader(document.getElementById("frmAssess"));
            frm.parent().parent().parent().find(".dx-toolbar-after").append("<a href='' class='' style='margin-top:3px' onclick='ConfrimCancelIntensities(); return false;'><i class='dx-icon-close'></i></a>");
        }
        var params = "?intensities=true&rid=" + selectedScaleID + "&step=1&temptheme=sl&rnd=" + Math.round(10000*Math.random()) + "&" + optionsAll + (optionZeros != "" ? "&exclude=" + optionZeros : "");
        setTimeout('document.getElementById("frmAssess").src="<% =JS_SafeString(PageURL(_PGID_EVALUATION)) %>' + params +'";', 50);
        setTimeout('resizeFrame();', 100);
<%--        var params = "?intensities=true&rid=" + selectedScaleID + "&step=1&temptheme=sl&rnd=" + Math.round(10000*Math.random()) + "&" + optionsAll + (optionZeros != "" ? "&exclude=" + optionZeros : "");
        var wnd = CreatePopup('<% =PageURL(_PGID_EVALUATION) %>' + params, 'Evaluate', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,resizable=yes,scrollbars=yes');
        if ((wnd)) wnd.focus();--%>
    }

    function ConfrimCancelIntensities()
    {
        var result = DevExpress.ui.dialog.confirm("<% =JS_SafeString(ResString("msgCancelIntensities")) %>", resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                onEndEvaluateIntensities(false);
            }
        });
        return false;
    }

    function onEndEvaluateIntensities(do_refresh) {
        document.getElementById("frmAssess").src="<% = _URL_ROOT %>dummy.htm";
        //$("#frmAssess").html();
        if ((frmAssess)) frmAssess.hide(); else _ajax_ok_code = "reloadPage();";
        frmAssess = null;
        if ((do_refresh)) sendCommand("action=refresh_scale&mt=" + selectedTypeID + "&ms=" + selectedScaleID, true);
    }

    /* end Assess Priorities */

    /* Scales Clipboard Data */
    var sItemsSeparator = "\t";
    var sCaretReturn = "\r\n";

    var templScaleName = "SCALE NAME:";

    var templUCIncreasing = "Increasing:";
    var templUCLinear = "Linear:";
    var templUCCurvature = "Curvature:";
    var templUCLow = "Low:";
    var templUCHigh = "High:";

    var templSFLinear = "Piecewise Linear:";

    var colName = "Name";
    var colValue = "Value";
    var colDescr = "Description";
    var colLow = "Low";
    
    function GetRSText(scale) {
        var sText = "";
        sText += templScaleName + sItemsSeparator + scale[IDX_SCALE_NAME] + sCaretReturn;
        sText += colName + sItemsSeparator + colValue + sItemsSeparator + colDescr + sCaretReturn;

        for (var i = 0; i < scale[IDX_INTENSITIES].length; i++) {
            //sText += String.Format("{0}{1}{2}{3}{4}{5}", IIf(MeasureTypeID = ECMeasureType.mtPWOutcomes, " ", item.Name), sItemsSeparator, item.Value.ToString("F7"), sItemsSeparator, item.Description, sCaretReturn)
            var v = scale[IDX_INTENSITIES][i];
            sText += (scale[IDX_MEASURE_TYPE] == mtPWOutcomes, " ", v[INT_NAME]) + sItemsSeparator + v[INT_VALUE] + sItemsSeparator + v[INT_COMMENT] + sCaretReturn;
        }

        return sText;
    }

    function GetSFText(scale) {
        var sText = "";
        sText += templScaleName + sItemsSeparator + scale[IDX_SCALE_NAME] + sCaretReturn;
        sText += templSFLinear + sItemsSeparator + (scale[IDX_USE_DIRECT_RATINGS] ? "Yes" : "No") + sCaretReturn;
        sText += colName + sItemsSeparator + colLow + sItemsSeparator + colValue + sCaretReturn;

        for (var i = 0; i < scale[IDX_INTENSITIES].length; i++) {            
            var v = scale[IDX_INTENSITIES][i];
            //sText += v[INT_NAME] + sItemsSeparator + v[2] + sItemsSeparator + v[3] + sItemsSeparator + v[4].toFixed(5) + sCaretReturn;
            sText += v[INT_NAME] + sItemsSeparator + v[2] + sItemsSeparator + v[4].toFixed(5) + sCaretReturn;
        }

        return sText;
    }

    function GetUCText(scale) {
        var sText = "";
        sText += templScaleName + sItemsSeparator + scale[IDX_SCALE_NAME] + sCaretReturn;
        var v = scale[IDX_INTENSITIES];

        sText += templUCIncreasing + sItemsSeparator + (v[3] ? "Yes" : "No") + sCaretReturn;
        sText += templUCCurvature + sItemsSeparator + v[2].toFixed(5) + sCaretReturn;
        sText += templUCLinear + sItemsSeparator + (v[4] ? "Yes" : "No") + sCaretReturn;
        sText += templUCLow + sItemsSeparator + v[0].toFixed(5) + sCaretReturn;
        sText += templUCHigh + sItemsSeparator + v[1].toFixed(5) + sCaretReturn;

        return sText;
    }
    /* end Scales Clipboard Data */

    function commitPasteFF() {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(pasteBox.value);
        }
    }   
    /* end Copy to Clipboard */

    var numDiag = sessionStorage.getItem("AssessPrioritiesNumDiag<%=App.ProjectID%>");
    numDiag = typeof numDiag == "undefined" || numDiag == null || numDiag == "" ? 2 : numDiag * 1;
    
    var grpPW = sessionStorage.getItem("AssessPrioritiesGrpPW<%=App.ProjectID%>");
    grpPW = typeof grpPW == "undefined" || grpPW == null || grpPW == "" ? 1 : grpPW * 1;

    var dispInc = sessionStorage.getItem("AssessPrioritiesDispIncons<%=App.ProjectID%>");
    dispInc = typeof dispInc == "undefined" || dispInc == null || dispInc == "" ? true : dispInc == "true";

    var dispMultiPW = sessionStorage.getItem("AssessPrioritiesDispMultiPW<%=App.ProjectID%>");
    dispMultiPW = typeof dispMultiPW == "undefined" || dispMultiPW == null || dispMultiPW == "" ? true : dispMultiPW == "true";

    /* jQuery Ajax */
    function syncReceived(params) {
        if ((params)) { 
            var received_data = eval(params);
            
            if (received_data[0] == "err_msg") showError(received_data[1]);

            if (received_data[0] == ACTION_MEASURE_TYPE || received_data[0] == ACTION_MEASURE_SCALE || received_data[0] == ACTION_NUM_COMPARISONS || received_data[0] == ACTION_PAIRWISE_TYPE || received_data[0] == ACTION_DISPLAY_OPTION || received_data[0] == ACTION_FORCE_GRAPHICAL || received_data[0] == ACTION_SET_NODE_PROPERTY) {
                refreshNodeData(received_data[1]);
            }                    
        
            if (received_data[0] == ACTION_SET_MEASURE_TYPE_FOR_ALL || received_data[0] == "copy_mt_to") {
                // update all nodes
                nodes = received_data[1];
                initTable();
            }

            if (received_data[0] == ACTION_VIEW) initTable();

            if (received_data[0] == ACTION_GET_STAT_DATA) {
                initNodeStatDataUI(received_data[1]);
            }

            if (received_data[0] == ACTION_SAVE_STAT_DATA) {
                if ((curStatDataImg)) {
                    //set icon highligted or inactive
                    curStatDataImg.src = "../../Images/ms/button_b" + (received_data[1].length == 0 ? "_" : "") + ".png";
                }
            }
            
            if (received_data[0] == "edit_int" || received_data[0] == "edit_sf_int" || received_data[0] == "del_int" || received_data[0] == "del_sf_int" || received_data[0] == "scale_default"  || received_data[0] == "scale_hide" || received_data[0] == "scale_name" || received_data[0] == "refresh_scale" || received_data[0] == "scale_sf_linear" || received_data[0] == "uc_param" || received_data[0] == "paste_scale" || received_data[0] == "assess_priorities_certain") {
                if (received_data.length > 1 && received_data[1].length > 0) {
                    var ms = received_data[1];
                    for (var i = 0; i < mscales.length; i++) {
                        if (mscales[i][IDX_MEASURE_TYPE] == ms[IDX_MEASURE_TYPE] && mscales[i][IDX_MEASURE_TYPE_NAME] == ms[IDX_MEASURE_TYPE_NAME]) mscales[i][IDX_IS_DEFAULT] = 0;
                        if (mscales[i][IDX_GUID] == selectedScaleID) {
                            mscales[i] = ms;
                        }
                    }
                    initSelectedTypeScales();
                    showScaleData(selectedScaleID);
                }
                if (received_data[0] == "assess_priorities_certain") {               
                    proceedAssessPriorities();
                }
            }

            if (received_data[0] == "delete_scale") {
                mscales = received_data[1];
                if (received_data.length > 2) selectedScaleID = received_data[2];
                initSelectedTypeScales();
                showScaleData(selectedScaleID);                
            }

            if (received_data[0] == "new_scale" || received_data[0] == "clone_scale") {
                mscales.push(received_data[1]);
                if (received_data.length > 2) selectedScaleID = received_data[2];
                initSelectedTypeScales();
                showScaleData(selectedScaleID);                
            }

            if (received_data[0] == "new_scale" && cur_node_id >= 0) {
                var node = getNodeById(cur_node_id, cur_node_is_ns);
                node[COL_MS_GUID] = received_data[2];
                onMeasureScaleChange(cur_node_id, cur_node_pid, node[COL_MS_GUID], node[COL_MT], node[COL_IS_ALT_WITH_NO_SOURCE]);
                cur_node_id = -1;
            }

            if (received_data[0] == "assess_priorities_options") {                
                dxDialog(received_data[1], "onAssessPrioritiesOptions();", ";", "Options", "Proceed");

                $('input[name=grpPairs][value=' + optionNumOfJudgments + ']').prop('checked', true);
                $('input[name=grpPW][value=' + optionPWType + ']').prop('checked', true);;
                document.getElementById("optDispInconsist").checked = optionShowInconsistency;
                document.getElementById("optDispMultiPairwise").checked = optionShowMultiPairwise;
            }

            if (received_data[0] == "assess_priorities_zeros") {                
                dxDialog(received_data[1], "onAssessPrioritiesZeros();", ";", "Confirmation");
            }            
        }

        if ($("#divCopyToTooltip").is(":visible")) {
            $("#divCopyToTooltip").hide();        
            resizePage();
        }
        displayLoadingPanel(false);
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function sendCommand(params, showPleaseWait) {       
        if (showPleaseWait) displayLoadingPanel(true);

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function showError(msg) {
        DevExpress.ui.notify(msg, "error");
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(grid_id, parent_id) {
        var margin = 8;
        $("#" + grid_id).height(0).width(0);
        var td = $("#" + parent_id);
        var w = $("#" + grid_id).width(Math.round(td.innerWidth())-margin).width();
        var h = $("#" + grid_id).height(Math.round(td.innerHeight())).height();
        if ((grid_w_old!=w || grid_h_old!=h)) {
            grid_w_old = w;
            grid_h_old = h;
        };
    }

    function checkResize(w_o, h_o) {
        var w = $(window).width();
        var h = $(window).height();
        if (!w || !h || !w_o || !h_o || (w == w_o && h == h_o)) {
            $("#tableContent").height(0).width(0);
            resizeGrid("tableContent", "divContent");
        }
    }

    function resizePage(force_redraw) {
        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        
        var infopanelsHeight = 0;
        $("#divContent").hide();
        $(".infopanel,#divToolbar,#divFooter").each(function () { infopanelsHeight += $(this).is(":visible") ? $(this).outerHeight() : 0; });
        $("#divContent").height($("#divMeasurementMethodsMain").height() - infopanelsHeight - 10).show();

        checkResize(force_redraw ? 0 : w, force_redraw ? 0 : h);
    }

    function resizeFrame() {
        var frm = $("#frmAssess");
        if ((frm) && (frm.length)) {
            var w = $(window).width()-96;
            var h = $(window).height()-96;
            frm.width(w).height(h);
            frm.parent().parent().parent().css("transform", "translate(20px, 20px) scale(1)");
        }
    }

    function resizeDatatable() {
        
    }

    function onSwitchAdvancedMode() {
        initTable();
    }

    function initToolbar() {
        var tabItems = [
            { title: "<%= If(PM.IsRiskProject, If(PM.ActiveHierarchy = ECHierarchyID.hidImpact, ResString("mnuMethodsAltsImpact"), ResString("mnuMethodsAltsLikelihood")), ResString("mnuMethodsAlts")) %>", key: mmAlt, icon: "icon ec-alts" },
            { title: "<%= If(PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidImpact, ResString("mnuMethodsObjsImpact"), ResString("mnuMethodsObjs")) %>", key: mmObj, icon: "icon ec-hierarchy" },
            { title: "<%= ResString("mnuMethodsAll")%>",  key: mmAll, icon: "fas fa-map-signs" }
        ];

        $("#divTabs").dxTabPanel({
            items: tabItems,
            selectedIndex: tabItems.indexOf(tabItems.filter(function (item, index, arr) { return item.key == MTMode; })[0]),
            onSelectionChanged: function(e) {
                if (e.addedItems.length > 0) {
                    onSetViewMode(e.addedItems[0].key);
                }
            }    
        });

        $("#btnManageScales").dxButton({
            text: "Manage Scales",
            icon: "fas fa-tasks",
            type: "default",
            showText: true,
            onClick: function (e) {
                onManageScalesDlg(true);
            }
        });
    }

    function onSetPage(pgid) {
        var curPgID = pgid % <%=_PGID_MAX_MOD%>;
        var oldMTMode = MTMode;
        switch (curPgID) {
            case <% =CInt(_PGID_STRUCTURE_MEASUREMENT_METHODS)%>:
                pgID = pgid;
                MTMode = mmAll;
                break;
            case <% =CInt(_PGID_STRUCTURE_MEASUREMENT_METHODS_OBJS)%>:
                pgID = pgid;
                MTMode = mmObj;
                break;
            case <% =CInt(_PGID_STRUCTURE_MEASUREMENT_METHODS_ALTS)%>:
                pgID = pgid;
                MTMode = mmAlt;
                break;
        }
        if (oldMTMode != MTMode) {
            onSetViewMode(MTMode);
            return true;
        }
        return false;
    }

    $(document).ready(function () {
        toggleScrollMain();
        initTable();
        initToolbar();
    });

    resize_custom  = resizePage;

</script>

<div class="whole" id='divMeasurementMethodsMain'>
    <div id='divToolbar' style="white-space: nowrap; padding-top: 3px;">
        <div id="divTabs" class="ec_tabs ec_tabs_nocontent" style="display: inline-block;"></div>
        <div id="btnManageScales" style="display: inline-block; margin: 5px 8px 0px 5px; float: right;"></div>
    </div>
    <div id="divContent" style="text-align: center; height: calc(100% - 100px); width: 100%;">
        <div id='tableContent' class="dx-treelist-withlines dx-treelist-compact" style="width: 100%;"></div>
    </div>
    <div id="divFooter" style="text-align: left; padding: 4px;">
        <span id="lblTotalSteps" class="text"></span>
    </div>
    <div id="divCopyToTooltip" class="infopanel ra_hidden" style="margin: 2px 1ex 1ex 0px !important;">
        <span id="lblCopyToHint" class="text" style="color: darkblue; float: left; margin: 12px;"><%=ResString("msgCopyMeasurementTypes")%></span>
        <div style='white-space:nowrap; text-align:center; display:inline-block; margin:12px;' class='text'>Select: <a href='' onclick='selectAllToPasteMS(true); return false;' class='actions'><%=ResString("lblAll")%></a> | <a href='' onclick='selectAllToPasteMS(false); return false;' class='actions'><%=ResString("lblSelectNone")%></a></div>
        <input type="button" value="Proceed" style="float: right; margin: 5px; width: 100px;" onclick="proceedCopyMS();" />
        <input type="button" value="Cancel" style="float: right; margin: 5px; width: 80px;" onclick="cancelCopyMS();" />
    </div>            
</div>

<div id='divManageScales' style='display:none;'>
<table border='0' cellspacing='0' cellpadding='0' class='text whole' style='margin-right:5px;'>   
    <tr><td valign='top' style='padding:6px 0px 6px 0px; text-align: right;'>Measurement&nbsp;Method:&nbsp;</td>
        <td>
            <select style='width: 250px; height: 22px;' id='cbMeasurementMethod' class="disabled_for_editor" onchange="cbMeasurementMethodChange(this.value);">
                <option value="<%=CInt(ECMeasureType.mtRatings)%>">Rating Scale</option>
                <option value="<%=CInt(ECMeasureType.mtRegularUtilityCurve)%>">Utility Curve</option>
                <option value="<%=CInt(ECMeasureType.mtStep)%>">Step Function</option>
                <% If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then %>
                <option value="<%=CInt(ECMeasureType.mtPWOutcomes)%>">Pairwise Of Probabilities</option>
                <% End If %>
            </select>
        </td>
        <td align="left"><label for="cbScaleIsDefault" id="lblScaleDefault" class="editor_element"><input type="radio" class="editor_element" id="cbScaleIsDefault" onclick="setScaleDefault(this.checked);" />Use as default</label></td>       
    </tr>
    <tr><td valign='top' style='padding:6px 0px 6px 0px; text-align: right;'>Measurement&nbsp;Scale:&nbsp;</td>
        <td>
            <select style='width: 250px; height: 22px;' id='cbMeasurementScale' class="disabled_for_editor" onchange="cbMeasurementScaleChange(this.value);">
            </select>
        </td>
        <td align="left"><nobr>
                <%--<input type='button' class='button disabled_for_editor' value='Edit' id="btnEditScale" style='width:55px;' onclick='onEditScale();' />
                <input type='button' class='button editor_element' value='Save' id="btnSaveScale" style='width:55px;' onclick='onSaveScale();' />
                <input type='button' class='button editor_element' value='Cancel' id="btnCancelEdit" style='width:55px;' disabled="disabled" onclick='onCancelEditScale();' />--%>                
                <input type='button' class='button disabled_for_editor' id='btnCreateScale' value='Create New' onclick='onCreateNewScale();' />
                <input type='button' class='button disabled_for_editor' id='btnCloneScale' value='Clone' onclick='onCloneScale();' />
                <input type='button' class='button disabled_for_editor' id='btnDeleteScale' value='Delete' onclick='onDeleteSelectedScale();' />
            </nobr>
        </td>
    </tr>
    <tr><td valign='top' style='padding:6px 0px 6px 0px; text-align: right;'>Scale&nbsp;Name:&nbsp;</td>
        <td colspan="2">
            <%--<input type="text" id="lblScaleName" class="editor_element" style="width:537px; margin-top:3px; margin-bottom:3px;" />--%>
            <span id="lblScaleName" style="width:537px; margin-top:3px; margin-bottom:3px; font-weight: bold;"></span>
            <a href='' onclick='editScaleName(); return false;'><i class="fas fa-pencil-alt"></i></a>
        </td>        
    </tr>
    <tr><td valign='top' style='padding:6px 0px 6px 0px; text-align: right;'>Description:&nbsp;</td>
        <td colspan="2">
            <iframe src='' id='frmInfodoc' style='width: 450px; height: 60px; border:1px solid #e0e0e0; background:#ffffff;' frameborder='0' onload='if (typeof(InfodocFrameLoaded)!="undefined") InfodocFrameLoaded(this);'></iframe>
            <input type='button' id='btnEditInfodoc' class='button' style='float:right; width:12em' value='<% =ResString("btnEditInfodoc") %>' disabled='disabled' onclick='EditInfodoc();'/>
        </td>
    </tr>
    <tr id="trRatings" style="display: none;">
        <td class="text" valign='top' style='padding:6px 0px 6px 0px;' colspan="3">            
            <label id="lblHidePriorities" class="editor_element"><input type="checkbox" class="checkbox editor_element" id="cbScaleHidePriorities" onclick="setScaleHidePriorities(this.checked);" />Hide <%=If(Not PM.IsRiskProject, "priorities", If(PM.ActiveHierarchy = ECHierarchyID.hidLikelihood, ParseString("%%likelihoods%%"), ParseString("%%impacts%%"))) %> and direct entry during evaluation</label>
            <div style="overflow: auto; height: 230px; width: 96%;">
                <table id="tblScaleContent" class="lec-table table-with-row-lines" cellpadding="2" cellspacing="1" style="max-height: 230px; width: 100%;"></table>
            </div>
            <input type="button" class="button small" style="width: 130px;" onclick='addIntensity("","","","","");' value="Add Intensity" />
        </td>
    </tr>
    <tr id="trStep" style="display: none;">
        <td class="text" valign='top' style='padding:6px 0px 0px 0px;' colspan="2">
            <label id="lblSFLinear" class="editor_element"><input type="checkbox" class="checkbox editor_element" id="cbSFLinear" checked="checked" onclick="setSFLinear(this.checked);" />Piecewise Linear</label>
            <div style="overflow: auto; height: 230px; width: 96%; vertical-align: top;">
                <table id="tblSteps" class="text cell-border hover" cellpadding="2" cellspacing="1" style="max-height: 230px; width: 100%;"></table>
            </div>
            <input type="button" class="button small" style="width: 130px;" onclick='addInterval("","","","","");' value="Add Step" />
        </td>
        <td class="text" valign='top' style='padding:6px 0px 0px 0px;'>
            <iframe src='' id='frmSF' style='width: 410px; height: 270px; border:1px solid #e0e0e0; background:#ffffff;' frameborder='0'></iframe>
        </td>
    </tr>
    <tr id="trPlot" style="display: none;">
        <td valign='middle' colspan="3" style='padding:6px 0px 0px 0px; text-align:center;'>
            <table id="ctrlUtilityCurve" border='0' cellspacing='0' cellpadding='0' class='text whole' style='margin:5px;'>
                <tr>
                    <td style="vertical-align: top;">
                        <nobr><span style='display: inline-block; width: 70px; text-align: right; vertical-align: middle;'>Low:&nbsp;</span><input type="text"  class="text"  value="0" id="tbLow" style="vertical-align: top; text-align: right; width: 90px; margin-bottom: 4px;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onchange="onUCParamChange('low', this.value);" /></nobr><br/>
                        <nobr><span style='display: inline-block; width: 70px; text-align: right; vertical-align: middle;'>High:&nbsp;</span><input type="text"  class="text"  value="0" id="tbHigh" style="vertical-align: top; text-align: right; width: 90px;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onchange="onUCParamChange('high', this.value);" /></nobr>
                        <br/>
                        <br/>
                        <nobr><label style='display: inline-block; width: 130px; text-align: left;'><input type="radio" class="radio" value="0" name="rbIncreasing" onclick="onUCParamChange('inc', this.checked);" />Increasing</label>
                        <label style='display: inline-block; width: 100px; text-align: left;'><input type="radio" class="radio" value="0" name="rbLinear" onclick="onUCParamChange('lin', this.checked);" />Linear</label>
                        </nobr>
                        <br/>
                        <label style='display: inline-block; width: 130px; text-align: left;'><input type="radio" class="radio" value="1" name="rbIncreasing" onclick="onUCParamChange('inc', !this.checked);" />Decreasing</label>
                        <label style='display: inline-block; width: 100px; text-align: left;'><input type="radio" class="radio" value="1" name="rbLinear" onclick="onUCParamChange('lin', !this.checked);" />Nonlinear</label>
                        </nobr>
                        <br/>
                        <br/>
                        <label style="white-space: nowrap;"><span id="lblCurvature" style='display: inline-block; width: 70px; text-align: right; vertical-align: middle; color: black;'>Curvature:&nbsp;</spa><input type="text" class="text" style="text-align: right; width: 90px;" value="0" id="tbCurvature" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onchange="onUCParamChange('curv', this.value);" /></label>
                        <br/>
                    </td>
                    <td style="text-align: center; vertical-align: top;">
                        <iframe src='' id='frmUC' style='width: 410px; height: 270px; border:1px solid #e0e0e0; background:#ffffff;' frameborder='0'></iframe>
                    </td>
                </tr>                
            </table>
        </td>
    </tr>
</table>
</div>

<div id='divEditStatData' style='display:none;'>
    <h5 id="lblStatDataTitle"></h5>
    <nobr><span style="display:inline-block;text-align:right;width:100px;">Periods:</span><div id="divPeriodsHeaders" style="display:inline-block;"></div></nobr><br/>
    <nobr><span style="display:inline-block;text-align:right;width:100px;">Data:</span><div id="divPeriodsData" style="display:inline-block;"></div></nobr></br>
    <input type="button" value="Add Period" onclick="addStatPeriod();"/>
</div>

</asp:Content>