<%@ Page Language="VB" Inherits="RiskSelectTreatmentsMapPage" title="Define Treatments" Codebehind="RiskTreatmentsMap.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
<script type="text/javascript" src="/Scripts/jszip.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    var isReadOnly = <% = Bool2JS(App.IsActiveProjectReadOnly) %>;
    
    var cur_cmd = "";
    var multiselect = '<% =IIf(MultiselectEnabled, "1", "0") %>' == "1";
    var showDescriptions = <% = Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs) %>;
    var selectedUserID = <% = SelectedUserID %>;
    var columns_ver = "_20200107";

    var dlg_treatment_editor = null;
    var new_cat_name = ""; //new category name
    var rename_cat_id = "";

    var control_data = <% = GetControlsData() %>;   //control_data[] - treatments and applications
    var obj_columns = <% = GetObjectivesColumns() %>;
    var scale_data = <% = GetScaleData() %>;        //scale_data[]   - measurement scales
    var cat_data = <% = GetCategoriesData() %>;     //cat_data[]     - controls categories
    var event_data = <% = GetEventsData() %>;       //event_data[]   - events (alternatives)
    
    var event_id = "<% = SelectedEventID.ToString %>";
    var events_ids = []; // multiselect

    var ControlType = parseInt(<% = ControlType() %>); //cause = 0, vulnerability = 3, consequence = 4
    
    var ctCause = 0;
    var ctVulnerability = 3;
    var ctConsequence = 4;

    var PageType    = <% = PageType() %>;    //mapping, measure, effectiveness
    
    var ptMap = 0;    
    var ptEffect = 1;
    var ptMeasure = 2;

    var by_treatment = <% = Bool2JS(IsByTreatmentPage) %>;
    var treatment_id = "<% = SelectedTreatmentID.ToString %>";

    // measurement types
    var mtPairwise = <%=CInt(ECMeasureType.mtPairwise)%>;
    var mtDirect = <%=CInt(ECMeasureType.mtDirect)%>;
    var mtRatings = <%=CInt(ECMeasureType.mtRatings)%>;
    var mtUtilityCurve = <%=CInt(ECMeasureType.mtRegularUtilityCurve)%>;
    var mtStep = <%=CInt(ECMeasureType.mtStep)%>;
    var mtPWOutcomes = <%=CInt(ECMeasureType.mtPWOutcomes)%>; // Pairwise of probabilities
    var mtPWAnalogous = <%=CInt(ECMeasureType.mtPWAnalogous)%>; // Pairwise with Given Likelihood

    var COMBINED_USER_ID = <% = COMBINED_USER_ID %>;
    var DEFAULT_MT = mtDirect;   
    var DECIMALS  =  2;
    var PARAM_DELIMITER = ";";

    var lblNoApplications = "<% = ResString("lblNoApplications") %>";
    var lblApplyControls  = "<% = ResString("lblApplyControls") %>";
    
    var lblDirect  = "Direct";
    var lblRatings = "Ratings";
    var lblSF      = "Step Function";
    var lblUC      = "Utility Curve";
    var lblPW     = "Pairwise";
    var lblPOP     = "Pairwise Of Probabilities";    

    function InitEvents() {
        // init combobox
        if ($("#cbEvents").data("dxSelectBox")) $("#cbEvents").dxSelectBox("instance").dispose();

        $("#cbEvents").dxSelectBox({
            dataSource: by_treatment ? control_data : event_data,
            displayExpr: "name",
            valueExpr: "id",
            width: "auto",
            searchEnabled: true,
            disabled: by_treatment ? control_data.length == 0 : event_data.length == 0,
            value: by_treatment ? treatment_id : event_id,
            onValueChanged: function (e) {                    
                if (!by_treatment) { event_id = e.value; eventChanged(event_id); } else { treatment_id = e.value; treatmentChanged(treatment_id); }
                getTitle();     
            }
        });
    }

    var controls_data_len = 0;

    function initColumns(data, columns) {
        controls_data_len = by_treatment ? event_data.length : control_data.length;
        for (var i = 0; i < data.length; i++) {
            var clmn = { "caption" : data[i].name, "id" : data[i].id, "alignment" : "left", "allowSorting" : true, "allowSearch" : false, "allowEditing" : false /*PageType !== ptMap && data[i].editable*/, "allowHiding" : false, "dataField": data[i].dataField, "columns" : [] };
            if (data[i].dataField == "infodoc") { clmn.encodeHtml = false; clmn.visible = showDescriptions; }
            if (data[i].dataField == "index") { 
                clmn.alignment = "right"; 
                clmn.cellTemplate = function(element, info) {
                    element.append(padWithZeros(info.value, controls_data_len));
                }
            }
            if (data[i].dataField !== "name" && data[i].dataField !== "infodoc" && data[i].dataField !== "index") {
                if (data[i].editable) {
                    clmn.cellTemplate = function (element, info) {            
                        if (info.value == -1) {
                            element.css({"background-color" : "#d6d6d6", "cursor" : "not-allowed"});
                        } else {
                            switch (PageType) {
                                case ptMap:
                                    element.append(info.value ? "<i class='ec-icon fas fa-check'></i>" : "<i class='ec-icon fas fa-check color_transparent'></i>");
                                    element.addClass("cell_clickable").css({"vertical-align" : "middle", "text-align" : "center", "cursor" : "pointer"});
                                    break;
                                case ptEffect:
                                    if (selectedUserID == -1) {
                                        element.append("<input type='number' value='" + (info.value > 0 ? info.value : 0) + "' onchange='setControlEffectiveness(this, this.value, \""+info.row.key.id+"\", \""+info.column.id+"\", "+info.value+")'>");
                                    } else {
                                        element.append("<span>" + (info.value > 0 ? info.value : 0) + "</span>");
                                    }
                                    break;
                                case ptMeasure:
                                    var tCell = $("<div>");
                                    tCell.html(getMTSelect(info.value, info.row.key.id, info.column.id) +  "&nbsp;" + getMSSelect(info.value, info.data[info.column.dataField == "no_source" ? "no_source_ms" : "ms" + info.column.dataField.substr(1)], info.row.key.id, info.column.id));
                                    //if (checked != "") {
                                    //    //tCell.style.verticalAlign = "top";
                                    //} else {                                    
                                    //    // approach 1 - the "No Applications" text is always visible 
                                    //    // tCell.innerHTML = (disabled != "" ? "&nbsp;" : "<div class='text'><center>" + lblNoApplications + "<br ><a href='' onclick='navigateToApplyControls(); return false;'>" + lblApplyControls + "</a></center><div>");

                                    //    // approach 2 - the "No Applications" text is visible on mouse over only                                    
                                    //    if (disabled == "") {                                        
                                    //        //tCell.style.padding = "0px";
                                    //        //tCell.style.background = BKG_DEFAULT_HOVER;
                                    //        tCellhtml("<div class='text' id='divNoApps_cell_" + i +"_" + j + "' style='visibility:hidden;'><center>" + lblNoApplications + "<br ><a href='' onclick='navigateToApplyControls(); return false;'>" + lblApplyControls + "</a></center></div>");
                                    //        tCell.on("mouseover", function() { document.getElementById("divNoApps_"+this.getAttribute("id")).style.visibility = "visible"; });
                                    //        tCell.on("mouseout", function() { document.getElementById("divNoApps_"+this.getAttribute("id")).style.visibility = "hidden"; });
                                    //    } else {
                                    //        tCell.innerHTML = "&nbsp;";
                                    //    }                                    
                                    //}
                                    element.append(tCell);
                                    break;
                            }
                        }
                    };
                    //clmn.encodeHtml = true;
                    clmn.customizeText = function(cellInfo) {
                        return cellInfo.value == -1 || !cellInfo.value ? "" : "✔";
                    };
                } else {
                    clmn.cellTemplate = function (element, info) {
                        element.css({"background-color" : "#d6d6d6", "cursor" : "not-allowed"});
                    }
                }
            }
            columns.push(clmn);
            if (data[i].children.length > 0) {
                //for (j = 0; j < data[i].children.length; j ++)  {
                initColumns(data[i].children, clmn.columns);
                //}
            }        
        }
    }
        
    function initGrid() {
        var columns = [];
        initColumns(obj_columns, columns);
        
        columns[0].fixed = true;
        columns[1].fixed = true;

        $("#gridControls").dxDataGrid( {
            allowColumnResizing: true,
            dataSource: by_treatment ? event_data : control_data,
            columns: columns,
            columnAutoWidth: true,
            columnResizingMode: 'widget',
            width: "100%",
            searchPanel: {
                visible: false,
                width: 240,
                placeholder: "<%=ResString("btnDoSearch")%>..."
            },
            columnChooser: {
                mode: "select",
                enabled: false
            },
            grouping: {
                autoExpandAll: true,
            },
            groupPanel: {
                visible: false
            },
            rowAlternationEnabled: false,
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            hoverStateEnabled: false,
            <%If App.isExportAvailable Then%>
            "export": {
                enabled: true
            },
            <%End If%>
            editing: {
                mode: "cell",
                useIcons: true,
                allowUpdating: !isReadOnly
            },
            onCellClick: function (e) {
                if (PageType == ptMap) {
                    if ((e.column) && (e.column.index > (showDescriptions ? 1 : 0)) && (e.row) && e.row.rowType == "data") {
                        var obj_id = getObjIdFromColumnDataField($("#gridControls").dxDataGrid("instance").option("columns"), e.column.dataField);
                        if (getObjColumnById(obj_columns, obj_id).editable && e.row.data[e.column.dataField] !== -1) {
                            activateControlClick(!e.value, e.row.data.id, obj_id);
                            e.value  = !e.value;
                            e.component.cellValue(e.row.rowIndex, e.column.dataField, e.value);
                        }
                    }
                }
            },
            onContentReady: function(e) {                
                resizePage();
            },
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items;
                if (ControlType !== ctCause) {
                    var template = '<div ID="RadPaneEvents" class="text" style="text-align: center; display: inline-block;">';
                    template += '<label class="text" style="font-weight: bold; display: inline-block; margin-top: 4px; vertical-align: top;"><%If Not IsByTreatmentPage Then %><%=ResString("lblSelectEvent")%><%Else%><%=ResString("lblSelectControl")%><%End If%>:&nbsp;</label>';
                    template += '<div id="cbEvents" style="display: inline-block; min-width: 500px;"></div>';
                    template += '<%--<%If IsByTreatmentPage Then %><div id="lblControlInfodoc" style="display: inline-block; background-color: #fafafa; border: 1px solid #f0f0f0; padding: 3px; margin:3px; max-width: 1000px; vertical-align: top; text-align: left;"></div><%End If%>--%>';
                    template += '<%If IsByTreatmentPage Then %><i class="fas fa-info-circle ec-icon" style="cursor: default; vertical-align: top; margin: 3px;" id="lblControlInfodoc"></i><%End If%>';
                    template += '</div>';

                    toolbarItems.splice(0, 0, { location: 'center', locateInMenu: 'never', template: template });
                    
                    setTimeout("InitEvents(); showControlInfodoc();", 100);
                }
            },
            showRowLines: true,
            stateStoring: {
                enabled: true,
                type: "sessionStorage",
                storageKey: "controls_Tbl_PRJ__ID_<% = App.ProjectID %>_PGID_<% = CurrentPageID %>" + columns_ver
            },
            noDataText: "<% = GetEmptyMessage()%>",
            wordWrapEnabled: false
        });
    }

    function getObjIdFromColumnDataField(columns, dataField) {
        var retVal = "";
        if (typeof columns !== "undefined") {
            for (var i = 0; i < columns.length; i++) {
                if (columns[i].dataField == dataField) {
                    retVal = columns[i].id;
                    break;
                } else {
                    if ((columns[i].columns) && columns[i].columns.length) {
                        retVal = getObjIdFromColumnDataField(columns[i].columns, dataField);
                        if (retVal !== "") break;
                    }
                }
            }
        }
        return retVal;
    }

    function getObjColumnById(columns, id) {
        var retVal = "";
        if (typeof columns !== "undefined") {
            for (var i = 0; i < columns.length; i++) {
                if (columns[i].id == id) {
                    retVal = columns[i];
                    break;
                } else {
                    if ((columns[i].children) && columns[i].children.length) {
                        retVal = getObjColumnById(columns[i].children, id);
                        if (retVal !== "") break;
                    }
                }
            }
        }
        return retVal;
    }
    
    function getMTSelect(mt, control_id, obj_id) {
        var int_mt = parseInt(mt);
        var select = "<select style='width:8em;' size='1' onchange='setControlMT(this.value, \"" + control_id + "\", \"" + obj_id + "\");'>";
        select += "<option value='" + mtDirect  + "' "+ (int_mt == mtDirect  ? "selected" : "") +">" + lblDirect  + "</option>";
        select += "<option value='" + mtRatings + "' "+ (int_mt == mtRatings ? "selected" : "") +">" + lblRatings + "</option>";
        select += "<option value='" + mtStep    + "' "+ (int_mt == mtStep    ? "selected" : "") +">" + lblSF + "</option>";
        select += "<option value='" + mtUtilityCurve   + "' "+ (int_mt == mtUtilityCurve   ? "selected" : "") +">" + lblUC + "</option>";
        //select += "<option value='" + mtPairwise   + "' "+ (int_mt == mtPairwise   ? "selected" : "") +">" + lblPW + "</option>";
        //select += "<option value='" + mtPWOutcomes   + "' "+ (int_mt == mtPWOutcomes   ? "selected" : "") +">" + lblPOP + "</option>";
        select += "</select>";
        return select;
    }

    function getMSSelect(mt, ms_id, control_id, obj_id) {
        var int_mt = parseInt(mt);
        if (int_mt !== mtDirect && int_mt !== mtPairwise) {
            var select = "<select class='ms-select' style='width:8em; margin-top:3px;' size='1' onchange='setControlMS(this.value, \"" + control_id + "\", \"" + obj_id + "\");'>";
            var scale_data_len = scale_data.length;
            for (var i = 0; i < scale_data_len; i++) {
                if (scale_data[i].type == int_mt) {
                    select += "<option value='" + scale_data[i].id + "' "+ (ms_id == scale_data[i].id ? "selected" : "") +">" + scale_data[i].name + "</option>";
                }       
            }
            select += "</select>";
            return select;
        }
        return "&nbsp;"
    }

    function getEventByID(alt_id) {
        for (var i = 0; i < event_data.length; i++) {
            if (event_data[i].id == alt_id) return event_data[i];
        }
        return undefined;
    }

    function eventChanged(eid) {
        sendCommand("action=event_changed&eid=" + eid);
    }

    function treatmentChanged(id) {
        sendCommand("action=treatment_changed&id=" + id);        
    }

    function showControlInfodoc() {
        <%If IsByTreatmentPage Then %>
        var lbl = document.getElementById("lblControlInfodoc");
        if ((lbl)) {
            var ctrl = getControlByID(treatment_id);
            if ((ctrl)) {
                //lbl.innerHTML = ShortString(ctrl.infodoc, 700, false);
                //lbl.innerHTML = parseDescription(ctrl.infodoc, ctrl.id);
                lbl.title = ctrl.infodoc;    // due to see the short version with "view more" link
                //lbl.title = (ctrl.infodoc.indexOf("%%viewmore%%")>0 ? "<% = JS_SafeString(ResString("lblClick4ViewMore")) %>" : "");
            }
        }
        $("#lblControlInfodoc").toggle(showDescriptions && (lbl) && lbl.title !== "");
        <%End If%>
    }

    function navigateToApplyControls() {
        var pgid = 0;
        switch (ControlType) {
            case ctCause:
                pgid = 70055;
                break;
            case ctVulnerability:
                pgid = 70056;
                break;
            case ctConsequence:
                pgid = 70057;
                break;
        }
        navOpenPage(pgid, true);
    }
    
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

    function getCtrlDescr(sDescr) {
        ctrl_descr = sDescr;
    }

    function getCtrlCost(sCost) {
        if (validFloat(sCost)) { ctrl_cost = sCost } else {ctrl_cost = "0" }
        dxDialogBtnDisable(true, (!validFloat(ctrl_cost)) || (ctrl_name.trim() == ""));
    }

    function getCtrlCategories() {        
        ctrl_cat = "";
        $('input:checkbox.cb_cat:checked').each(function() { ctrl_cat += (ctrl_cat == ""?"" : PARAM_DELIMITER) + this.value });
    }

    var cancelled = false;

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
            createCategoryElement(lb, i, cat_data[i].id, cat_data[i].name, categories);
        }               

        cancelled = false;

        dlg_treatment_editor = $("#editorForm").dialog({
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
                  dlg_treatment_editor.dialog( "close" );
                }},
                Cancel: function() {
                  cancelled = true;
                  dlg_treatment_editor.dialog( "close" );
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

    function addControlClick() {
        resetVars();
        initEditorForm("<% = ParseString("Add a %%control%%") %>", "","","0.00", "addcontrol", "");
        dlg_treatment_editor.dialog("open");
        dxDialogBtnDisable(true, false);
        if ((theForm.tbControlName)) theForm.tbControlName.focus();
    }

    function pasteControlsClick() {
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
        if ((pasteBox)) {
            pasteData(pasteBox.value);
        }
    }

    function pasteData(data) {
        if (data != undefined && data != "") {
            sendCommand("action=paste_controls&data="+encodeURIComponent(data));
        } else { dxDialog("<% = ResString("msgUnableToPaste") %>", "", undefined, "<%=ResString("lblError") %>"); }
    }


    function getControlByID(control_id) {
        var controls_len = control_data.length;
        for (var i = 0; i < controls_len; i++) {
            if (control_data[i].id == control_id) return control_data[i];
        }
        return undefined;
    }

    function getControlApplication(control, obj_id, ev_id) {
        var appl = null;
        var apps = control.apps;
        var apps_len = apps.length;
        for (var j = 0; j < apps_len; j++){
            if (apps[j].obj_id == obj_id) {
                if ((ControlType == ctCause) || (apps[j].alt_id == ev_id)) {
                    appl = apps[j];
                    j = apps_len;
                }
            }
        }
        return appl;
    }

    function activateControlClick(checked, control_id, obj_id) {
        if (!by_treatment) {
            sendCommand("action=activate&checked=" + checked + "&control_id=" + control_id + "&obj_id=" + obj_id + "&event_id=" + get_event_id(), false);
        } else {
            sendCommand("action=activate&checked=" + checked + "&control_id=" + treatment_id + "&obj_id=" + obj_id + "&event_id=" + control_id, false);
        }        
    }

    function validFloat(s) {
        return !isNaN(parseFloat(replaceString(",", ".", s + "")));
    }

    function str2double(s) {
        return parseFloat(replaceString(",", ".", s + ""));
    }

    function get_event_id() {
        if ((PageType != ptMap) || (!multiselect) || (ControlType == ctCause)) {
            events_ids = [event_id];
            return !by_treatment ? event_id : treatment_id;
        } else {
            getSelectedEvents();
            var events_ids_len = events_ids.length;
            var retVal = "";
            for (var i = 0; i < events_ids_len; i++) {
                retVal += (retVal == "" ? "" : PARAM_DELIMITER) + events_ids[i];
            }
            return retVal;
        }
    }

    function setControlEffectiveness(tb, value, control_id, obj_id, orig_value) {
        var f = str2double(value);
        if (validFloat(value) && (f >= 0) && (f <=1 ) && (value != orig_value)) {            
            var e_id = event_id;
            if (by_treatment) {
                e_id = control_id;
                control_id = treatment_id;
            }            
            sendCommand("action=effectiveness&value=" + value.trim() + "&control_id=" + control_id + "&obj_id=" + obj_id + "&event_id=" + e_id, false);
        } else { tb.value = orig_value; }
    }

    function effectivenessKeyDown(event, tb, value, control_id, obj_id, orig_value)
    {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            if (code == 13) setControlEffectiveness(tb, value, control_id, obj_id, orig_value);
            if (code == 27) tb.value = orig_value;
       }
    }

    //set treatment measurement type
    function setControlMT(mt, control_id, obj_id) {
        var e_id = event_id;
        if (by_treatment) {
            e_id = control_id;
            control_id = treatment_id;
        }
        sendCommand("action=mt&mt=" + mt + "&control_id=" + control_id + "&obj_id=" + obj_id + "&event_id=" + e_id, false);
        //var ctrl = getControlByID(control_id);
        //if ((ctrl)) {
        //    var appl = getControlApplication(ctrl, obj_id, e_id);
        //    if ((appl)) {
        //        appl.mt = mt;
        //        if (mt != mtDirect && mt !== mtPairwise) {
        //            var scale_data_len = scale_data.length;
        //            for (var i = 0; i < scale_data_len; i++) {
        //                if (scale_data.type == mt) {
        //                    appl.ms = scale_data.id;
        //                    i = scale_data_len;
        //                }
        //            }
        //        }
        //        initGrid();
        //    }
        //}
    }

    //set treatment measurement scale
    function setControlMS(ms_id, control_id, obj_id) {
        var e_id = event_id;
        if (by_treatment) {
            e_id = control_id;
            control_id = treatment_id;
        }
        sendCommand("action=ms&ms=" + ms_id + "&control_id=" + control_id + "&obj_id=" + obj_id + "&event_id=" + e_id, false);
        //var ctrl = getControlByID(control_id);
        //if ((ctrl)) {
        //    var appl = getControlApplication(ctrl, obj_id, e_id);
        //    if ((appl)) appl.ms = ms_id;
        //}
    }

    /* jQuery Ajax */
    function sendCommand(params, show_please_wait) {       
        cur_cmd = params;
        if (typeof show_please_wait == "undefined" || show_please_wait) showLoadingPanel();

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);

        return false;
    }

    function syncReceived(params) {
        if (cur_cmd.indexOf("action=refresh") == 0 || cur_cmd.indexOf("action=mt") == 0) {
            if (by_treatment) {
                event_data = eval(params);
            } else {
                control_data = eval(params);
            }
            //if (by_treatment) {
            //    InitEvents();
            //}
            initGrid();
        }
        if (cur_cmd.indexOf("action=event_changed") == 0) {
            var rd = eval(params);
            obj_columns = rd[0];
            control_data = rd[1];
            initGrid();
        }
        if (cur_cmd.indexOf("action=treatment_changed") == 0) {
            var rd = eval(params);
            obj_columns = rd[0];
            event_data = rd[1];
            initGrid();
            //showControlInfodoc();
        }
        if (cur_cmd.indexOf("action=show_descriptions") == 0) {            
        }
        if (cur_cmd.indexOf("action=addcontrol") == 0) {
            control_data = eval(params);
            //if (by_treatment) {
            //    InitEvents();
            //}
            initGrid();
            if (by_treatment) {
                $("#cbEvents").dxSelectBox("instance").option("value", control_data[control_data.length - 1].id);
                //treatment_id = control_data[control_data.length - 1].id; 
                //treatmentChanged(treatment_id);
                //getTitle();
            }
        }
        if (cur_cmd.indexOf("action=paste_controls") == 0) {
            var res = eval(params); // [new controls],[all categories]
            for (var i = 0; i < res[0].length; i++) {
                control_data.push(res[0][i]);
            }            
            cat_data = res[1];
            //if (by_treatment) {
            //    InitEvents();
            //}
            initGrid();
        }
        if (cur_cmd.indexOf("action=deletecontrol") == 0) {
            control_data = eval(params);
            //if (by_treatment) {
            //    InitEvents();
            //}
            initGrid();
        }
        if (((cur_cmd.indexOf("action=editcontrol") == 0)) && (params != "")) {
            var edited_control_data = eval(params);
            if ((edited_control_data) && (edited_control_data.length == 2) && (edited_control_data[0] == "effectiveness") && (edited_control_data[1] == "-1")) {
                initGrid();
            } else {
                var control_data_len = control_data.length;
                for (var i=0; i < control_data_len; i++) {
                    if ((control_data[i]) && (control_data[i].id == edited_control_data.id)) {
                        control_data[i] = edited_control_data;
                        i = control_data_len;
                        if (!(cur_cmd.indexOf("action=effectiveness") == 0)) initGrid();
                    }
                }       
            }            
        }
        //if (cur_cmd.indexOf("action=activate") == 0) {
        //    control_data = eval(params);
        //    initGrid();
        //}
        if ((cur_cmd.indexOf("action=activate_row") == 0) || (cur_cmd.indexOf("action=activate_col") == 0) || (cur_cmd.indexOf("action=selected_user_id") == 0)) {
            //alert(params);
            control_data = eval(params);
            initGrid();
        }
        if (cur_cmd.indexOf("action=add_category") == 0) {
            cat_data.push([params, new_cat_name]);
            
            var lb = document.getElementById("lbCategories");
            createCategoryElement(lb, cat_data.length - 1, params, new_cat_name, ctrl_cat);
            lb.scrollTop = lb.lastChild.offsetTop;
        }

        hideLoadingPanel();
    }

    function syncError() {
        hideLoadingPanel();
        dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error");
    }

    var is_context_menu_open = false;

    function showMenu(event, control_id, control_name) {        
        is_context_menu_open = false;
        var ctrl = getControlByID(control_id);
        if ((ctrl)) {   
            $("div.context-menu").hide().remove();
            var sMenu = "<div class='context-menu'>";
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameClick(\"" + control_id + "\"); return false;' style='text-align:left;'><div><nobr><i class='fas fa-pencil-alt'></i>&nbsp;<% = ResString("lblCMenuEditCtrlProps") %></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuEditInfodoc(\"" + control_id + "\"); return false;' style='text-align:left;'><div><nobr><i class='fas fa-info-circle'></i>&nbsp;<% = ResString("lblCMenuEditCtrlInfodoc") %></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='confirmDeleteControl(\"" + control_id + "\"); return false;' style='text-align:left;'><div><nobr><i class='fas fa-trash'></i>&nbsp;<% = ResString("lblCMenuRemoveCtrl") %></nobr></div></a>";
            sMenu += "</div>";                
            var x = event.clientX;
            var y = event.clientY;
            var s = $(sMenu).appendTo("#divTreatmentsMain").css({top: (y - 90) + "px", left: x + "px"});                        
            $("div.context-menu").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }
    }

    function canCloseMenu() {
        is_context_menu_open = true;
    }

    function onContextMenuEditInfodoc(id) {
        <% = PopupRichEditor(reObjectType.Control , "guid=' + id + '") %>;
    }

    function onRichEditorRefresh(empty, infodoc, callback) { 
        sendCommand("action=refresh");
        window.focus();
    }

    function showInfodoc(id) {
        var url = "<% =PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&guid="" + id + ""&r={1}", CInt(reObjectType.Control), GetRandomString(10, True, False)))%>";
        CreatePopup(url, '', 'menubar=no,maximize=no,titlebar=yes,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=800,height=560');
        return false;
    }

    function parseDescription(sDesc, id) {
        if (sDesc.indexOf("%%viewmore%%")>0) {
            sDesc = "<div onclick='showInfodoc(\"" + id + "\"); return false;' style='cursor:pointer'>" +  replaceString("%%viewmore%%", "<i class='fas fa-search'></i>", sDesc) + "</div>";
        }
        return sDesc;
    }

    function onContextMenuRenameClick(control_id) {
        resetVars();
        $("div.context-menu").hide(200); is_context_menu_open = false;
        var control_data_len = control_data.length;
        ctrl_id = control_id;
        for (var i = 0; i < control_data_len; i++) {
            if ((control_data[i]) && (control_data[i].id == control_id)) {
                var cost = (control_data[i].cost == "", "0.00", control_data[i].cost);
                initEditorForm("<% = ParseString("%%Control%% properties") %>", control_data[i].name, control_data[i].infodoc, cost, "editcontrol", control_data[i].cats);
                dlg_treatment_editor.dialog("open");
                dxDialogBtnDisable(true, false);
                if ((theForm.tbControlName)) theForm.tbControlName.focus();
                i = control_data_len;
            }
        }        
    }

    function onContextMenuRemoveClick(control_id) {
        resetVars();
        $("div.context-menu").hide(200); is_context_menu_open = false;
        sendCommand("action=deletecontrol&control_id=" + control_id);
    }

    function confirmDeleteControl(control_id) {
        $("div.context-menu").hide(); is_context_menu_open = false;
        dxConfirm("Are you sure you want to delete this <%=ParseString("%%control%%") %>?", "onContextMenuRemoveClick('" + control_id + "');", ";", "Confirmation");
    }

    /* Region "context menu for categories" */    
    function getCategoryByID(cat_id) {
        var cat_data_len = cat_data.length;
        for (var i = 0; i < cat_data_len; i++) {
            if (cat_data[i].id == cat_id) return cat_data[i];
        }
    }

    function getCategoryByName(cat_name) {
        cat_name = cat_name.trim();
        var cat_data_len = cat_data.length;
        for (var i = 0; i < cat_data_len; i++) {
            if (cat_data[i].name == cat_name) return cat_data[i];
        }
        return null;
    }

    function showMenuCat(event, id, name) {        
        is_context_menu_open = false;
        var cat = getCategoryByID(id);
        if ((cat)) {               
            $("div.context-menu").hide().remove();
            var sMenu = "<div class='context-menu'>";
               sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameCategoryClick(\"" + id + "\"); return false;'><div><nobr><i class='fas fa-pencil-alt'></i>&nbsp;Edit&nbsp;&quot;" + name + "&quot;</nobr></div></a>";
               sMenu += "<a href='' class='context-menu-item' onclick='confrimDeleteCategory(\"" + id + "\"); return false;'><div><nobr>&nbsp;<i class='fas fa-trash'></i>&nbsp;&nbsp;Delete&nbsp;&quot;" + name + "&quot;</nobr></div></a>";
               sMenu += "</div>";                
            var x = event.clientX;
            var y = event.clientY;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});
            $("div.context-menu").fadeIn(500);
            setTimeout('canCloseMenu()', 200);
        }
    }

    function onContextMenuRenameCategoryClick(id) {
        $("div.context-menu").hide(200); is_context_menu_open = false;
        rename_cat_id = id;
        var cat = getCategoryByID(id);        
        if ((cat)) {
            new_cat_name = cat.name;
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
        if ((cat) && (cat.name != newName) && (newName.length > 0)) {
            sendCommand("action=rename_category&cat_id=" + rename_cat_id + "&cat_name=" + encodeURIComponent(newName));
            new_cat_name = ""
            cat.name = newName;
            
            var lb = document.getElementById("lbCategories");        
            var lb_children_count = lb.childNodes.length;
            for (var j=0; j<lb_children_count; j++) {
                if (lb.childNodes[j].getAttribute("id") == rename_cat_id) {
                   var idx = lb.childNodes[j].getAttribute("i");
                   var lbl = document.getElementById('lbl_cat_'+idx);
                   if ((lbl)) { lbl.innerHTML = newName }
                   j = lb_children_count;
                }
            }
        }
    }

    function onRenameEmptyName() {
        dxDialog("Error: Empty category name!", "onContextMenuRenameCategoryClick(rename_cat_id);", undefined, "Error");
    }

    function onContextMenuRemoveCategoryClick(id) {
        $("div.context-menu").hide(200); is_context_menu_open = false;

        var cat_data_len = cat_data.length;
        for (var i = 0; i < cat_data_len; i++) {
            if ((cat_data[i]) && (cat_data[i].id == id)) {
                cat_data.splice(i, 1);
                
                var lb = document.getElementById("lbCategories");        
                var lb_children_count = lb.childNodes.length;
                for (var j=0; j<lb_children_count; j++) {
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
        $("div.context-menu").hide(); is_context_menu_open = false;
        dxConfirm("Are you sure you want to delete this category?", "onContextMenuRemoveCategoryClick('" + id + "');", ";", "Confirmation");
    }
    /* End Region "context menu for categories" */

    function rowHeaderCbClick(control_id, checked) {
        sendCommand("action=activate_row&row_element_id=" + control_id + "&dropdown_element_id=" + get_event_id() + "&checked=" + (checked ? "1" : "0"), false);
    }

    function colHeaderCbClick(obj_id, checked) {
        sendCommand("action=activate_col&obj_id=" + obj_id + "&dropdown_element_id=" + get_event_id() + "&checked=" + (checked ? "1" : "0"), false);
    }

    function SelectAllAlts(selAll) {
        if (selAll) {
            $('input:checkbox.event_cb').prop('checked', true);
        } else {
            $('input:checkbox.event_cb').prop('checked', false);
        }
        initGrid();
    }

    function getSelectedEvents() {
        events_ids = $('input:checkbox.event_cb:checked').map(function () { return this.value; }).get();        
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
        
        opt.style.padding = "0px 0px 0px 0px";
        opt.style.margin  = "0px 0px 0px 0px";

        opt.value = cat_id;
        var chk = (categories.indexOf(cat_id) >= 0 ? " checked " : " ");
        
        opt.innerHTML = "<table cellpadding='0' cellspacing='0' class='text' style='width:100%;'><tr><td align='left'><label><input type='checkbox' id='cb_cat_" + i + "' class='cb_cat' id='cb_cat_" + i + "' " + chk + " value='" + cat_id + "' onclick='getCtrlCategories();' ><span id='lbl_cat_" + i + "'>" + name + "</span></label></td><td align='right'><i class='fas fa-ellipsis-h' align='right' style='visibility:hidden;vertical-align:top;cursor:pointer;height:16px;' id='mnu_cat_img_" + i + "' onclick='showMenuCat(event, \"" + cat_id + "\", \"" + decodeURIComponent(cat_name) + "\");'></i></td></tr></table>";
        opt.onmouseover = function() { document.getElementById("mnu_cat_img_"+this.getAttribute("i")).style.visibility = "visible"; };
        opt.onmouseout  = function() { document.getElementById("mnu_cat_img_"+this.getAttribute("i")).style.visibility = "hidden"; };
        lb.appendChild(opt);
        lb.disabled = false;
    }

    function getTitle() {
        var retVal = "";        
        var elemName = "";

        if (ControlType != ctCause) {
            if (by_treatment) {
                var ctrl = getControlByID(treatment_id);
                if ((ctrl)) elemName = "&quot;" + htmlEscape(ctrl.name) + "&quot;";
            } else {
                var e = getEventByID(event_id);
                if ((e)) elemName = "&quot;" + htmlEscape(e.name) + "&quot;";
            }            
        }
        
        switch (PageType) {
            case ptMap:                
                switch (ControlType) {
                    case ctCause:
                            retVal = "<%=ParseString("%%Controls%% for %%Objective(l)%% %%Likelihoods%%")%>";
                        break;
                    case ctVulnerability:
                        if (by_treatment) {
                                retVal = "<%=ParseString("%%Control%%  ")%>" + elemName + "<%=ParseString(" for %%vulnerabilities%% of %%events%% to %%objectives(l)%%")%>";
                        } else {
                                retVal = "<%=ParseString("%%Controls%% for %%Vulnerabilities%% of %%event%% ")%>" + elemName + "<%=ParseString(" to %%objectives(l)%%")%>";
                        }
                        break;
                    case ctConsequence:
                        if (by_treatment) {
                                retVal = "<%=ParseString("%%Control%%  ")%>" + elemName + "<%=ParseString(" to mitigate consequences of %%events%% to %%objectives(i)%% ")%>";
                        } else {
                                retVal = "<%=ParseString("%%Controls%% to mitigate consequences of %%event%% ")%>" + elemName + "<%=ParseString(" to %%objectives(i)%%")%>";
                        }
                        break;
                }
                break;
            case ptMeasure:
                switch (ControlType) {
                    case ctCause:
                            retVal = "<%=ParseString("Measurement Methods for %%Controls%% for %%Objectives(l)%%")%>";
                        break;
                    case ctVulnerability:                        
                        retVal = "<%=ParseString("Measurement Methods for %%Controls%% for %%Vulnerabilities%%")%>";
                        break;
                    case ctConsequence:
                        retVal = "<%=ParseString("Measurement Methods for %%Controls%% for Consequences")%>";
                        break;
                }                                
                break;
            case ptEffect:
                switch (ControlType) {
                    case ctCause:
                            retVal = "<%=ParseString("Effectiveness of %%Objective(l)%% %%Controls%%")%>";
                        break;
                    case ctVulnerability:                        
                        retVal = "<%=ParseString("Effectiveness of %%Vulnerabilities%% %%Controls%%")%>";
                        break;
                    case ctConsequence:
                        retVal = "<%=ParseString("Effectiveness of Consequence %%Controls%%")%>";
                        break;
                }                                
                break;                
        }

        $("#lblTitle").html(retVal);
    }

</script>

<div id='divTreatmentsMain' style="width:100%; height:100%; overflow:hidden;">
<%-- Toolbar --%>
<div id='divToolbar' class='dxToolbar title-content'></div>
<%-- Title --%>
<div id='divTitle' class="title-content" style='height:auto; width:100%; vertical-align:top; overflow:hidden; padding:1ex;'><h4 id="lblTitle" style='padding-bottom:0px;'></h4><%--<div id='tbNumApplications' style='display:none;'></div>--%></div>

<%-- Page content --%>
<div id="RadSplitterMain" class="whole" style="text-align: left;"> 

<!-- Mapping grid -->
<div id="RadPaneGrid" class="whole text" style="display: block; padding-top:3px;">
    <div id='gridControls'></div>
</div>

</div>

</div>

<div id='editorForm' style='display:none;'>
<table border='0' cellspacing='0' cellpadding='0' class='text' style='margin-right:5px;'>
<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("%%Control%% Name:") %>&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' style='width:280px;' id='tbControlName' value='' onfocus='getCtrlName(this.value);' onkeyup='getCtrlName(this.value);' onblur='getCtrlName(this.value);' /></td></tr>
<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("Cost:") %>&nbsp;&nbsp;</nobr></td><td><input type='text' class='input' style='width:100px;margin-top:3px;text-align:right;' id='tbControlCost' value='' onfocus='getCtrlCost(this.value);' onkeyup='getCtrlCost(this.value);' onblur='getCtrlCost(this.value);' /></td></tr>
<tr><td colspan='2'><hr style='margin:6px 0px 6px 0px; height:1px; border:none; color:#999999;background-color:#999999;' /></td></tr>
<tr><td valign='top' style='padding:6px 0px 0px 0px'><nobr><% = ParseString("Category:") %>&nbsp;&nbsp;</nobr></td><td>
    <ul id='lbCategories' disabled="disabled" style='height:14ex; width:280px; overflow-x: hidden; overflow-y:auto; border:1px solid #999999; text-align:left; margin:0px; padding:0px;'></ul>
</td></tr>
<tr><td>&nbsp;</td><td><input type='text' class='input' id='tbNewCategory' style='width:254px; margin:2px 0px;' /><input type='button' class='button' style='width:24px; margin:2px;' value='+' onclick='AddCategory(); return false;' /></td></tr>
</table>
</div>

<div id="tooltip" style="display: none; text-align: left;">
    <p style="text-align: left;"><%=ResString("msgPasteControlsHint")%></p>
</div>

<script language="javascript" type="text/javascript">                     

    var toolbarItems = [
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxSelectBox',
        visible: PageType == ptEffect,
        disabled: false,
        options: {
            showText: true,
            valueExpr: "key",
            displayExpr: "text",
            value: <% = SelectedUserID %>,
            elementAttr: { id: 'cbUsers' },
            items: [<% = GetParticipants() %>],
            onValueChanged: function (e) {
                selectedUserID = e.value;
                sendCommand("action=selected_user_id&value=" + e.value); 
            }
        }
    }, {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        disabled: <%=Bool2JS(Not CanUserAddControls)%>,                
        options: {
            showText: true,
            icon: "fas fa-plus-square",
            hint: "<%=ResString("btnAddControl")%>",
            text: "<%=ResString("btnAddControl")%>",
            elementAttr: {id: "btnAddControl"},
            onClick: function (e) {
                addControlClick();
            }
        }
    }, {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        disabled: <%=Bool2JS(Not CanUserAddControls)%>,
        options: {
            icon: "fas fa-paste",
            hint: "<% = ResString("btnPasteControlsFromClipboard") %>",
            text: "<% = ResString("btnPasteControlsFromClipboard") %>",
            showText: true,
            elementAttr: {id: "btnPasteControls"},
            onClick: function (e) {
                pasteControlsClick();
            }
        }
    }, {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxCheckBox',
        visible: true,
        options: {
            text: "<%=ParseString("Descriptions")%>",
            showText: true,
            hint: "<%=ParseString("Descriptions")%>",
            disabled: false,
            value: <% = Bool2JS(PM.Parameters.Riskion_Control_ShowInfodocs)%>,
            elementAttr: { id: 'cbControlsDescriptions' },
            onValueChanged: function (e) {
                if (showDescriptions !== e.value) {
                    showDescriptions = e.value; 
                    $("#lblControlInfodoc").toggle(showDescriptions);
                    $("#gridControls").dxDataGrid("columnOption", "infodoc", "visible", showDescriptions);
                    sendCommand("action=show_descriptions&chk=" + showDescriptions); 
                }
            }
        }
    }
    <%--<asp:CheckBox ID="cbMultiselect" runat="server" Enabled="False" AutoPostBack="True" Text="" />--%>
    ];

    function initToolbar() {
        $("#divToolbar").dxToolbar({
            items: toolbarItems,
            disabled: false
        });
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(id, parent_id, offset) {
        $("#" + id).height(0).width(100);
        var td = $("#" + parent_id);
        var w = $("#" + id).width(Math.round(td.innerWidth())).width();
        var h = $("#" + id).height(Math.round(td.innerHeight()) - 20).height();
        if ((grid_w_old != w || grid_h_old != h)) {
            grid_w_old = w;
            grid_h_old = h;
        };
    }

    function checkResize(id, w_o, h_o) {
        var w = $(window).width();
        var h = $(window).height();
        if (!w || !h || !w_o || !h_o || (w == w_o && h == h_o)) {
            resizeGrid(id, "RadPaneGrid", 0);
        }
    }

    function resizePage(force_redraw) {
        var d = document.getElementById("divTreatmentsMain");

        var s = document.getElementById("RadSplitterMain");
        var p = document.getElementById("RadPaneGrid");        

        //var e = document.getElementById("RadPaneEvents");
        var eh = 0;
        //if ((e)) eh = e.clientHeight > 10 ? e.clientHeight : 10;

        if ((s) && (d) && (p)) {

            var title_content_height = 0;
            $(".title-content").each(function () {
                title_content_height += $(this).is(":visible") ? $(this).height() : 0;
            });

            p.style.display = "none";
            s.style.width = "100px";
            s.style.height = "100px";
            s.style.width = d.clientWidth + "px";
            s.style.height =(d.clientHeight - title_content_height - 12) + "px";
            p.style.height= (s.clientHeight) + "px";
            p.style.display = "inline-block";
        }

        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        setTimeout("checkResize('gridControls'," + (force_redraw ? 0 : w) + "," + (force_redraw ? 0 : h) +");", 50);
    }

    resize_custom = resizePage;

    document.onclick = function () { if (is_context_menu_open == true) { $("div.context-menu").hide(200); is_context_menu_open = false; } }

    $(document).ready(function () {
        toggleScrollMain();

        //$("#RadPaneEvents").toggle(ControlType !== ctCause);
        
        initToolbar();
        initGrid();  

        $("#tooltip").dxTooltip({
            target: "#btnPasteControls",
            showEvent: "dxhoverstart",
            hideEvent: "dxhoverend"
        });
        
        //showControlInfodoc();

        getTitle();

        if (PageType == ptEffect) {
            setTimeout("var res = $('input.input'); if (res.length > 0) res[0].focus();", 1000);
        }
    });

</script>
</asp:Content>