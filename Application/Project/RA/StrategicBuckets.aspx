<%@ Page Language="VB" Inherits="RAStrategicBucketsPage" title="StrategicBuckets" Codebehind="StrategicBuckets.aspx.vb" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI"   Assembly="Telerik.Web.UI" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<asp:ScriptManager ID="ScriptManagerMain" runat="server" AsyncPostBackTimeout="300" LoadScriptsBeforeUI="true"></asp:ScriptManager>
<telerik:RadScriptBlock runat="server" ID="ScriptBlock">
<script language="javascript" type="text/javascript">

    <% =GetAttributesData() %> // var attr_data = [];

    var PgID  = <%=CurrentPageID%>;
    var _PGID_STRUCTURE_ALTERNATIVES = <%=_PGID_STRUCTURE_ALTERNATIVES%>;
    var _PGID_RA_STRATEGIC_BUCKETS = <%=_PGID_RA_STRATEGIC_BUCKETS%>;

    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";

    var AlternativesCount = <% =AlternativesCount %>;
    var SelectedAltID = "<%=SelectedAltID %>";

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
    
    var mh = 500;  // maxHeight for dialogs;
    var is_ajax = 0;

    //var COL_ID = 0 - use inices insted of IDs
    var COL_NAME = 1;
    var COL_FUNDED = 2;
    var COL_TOTAL = 3;
    var COL_COST = 4;
    var COL_ATRIBUTES_START_INDEX = 5;

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
    
    /* Toolbar */
    function UpdateButtons() {   
        // update other elements state
        var isDisabledCalc = (AlternativesCount == 0);
        
        cb = eval("theForm.cbNormalize");
        if (cb) cb.disabled = isDisabledCalc;

        cb = eval("theForm.cbMode");
        if (cb) cb.disabled = isDisabledCalc;
               
        cb = eval("theForm.cbDecimals");
        if (cb) cb.disabled = isDisabledCalc;
    }

    /* Add-remove-rename columns */
    function EditAttribute(index) {
        var n = document.getElementById("tbName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgEmptyCCName") %>", "setTimeout(\"document.getElementById('tbName').focus();\", 150);", "undefined", "<%=ResString("lblError") %>");
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
        var n = document.getElementById("tbName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgCreateEmptyCCName") %>", "setTimeout(\"document.getElementById('tbName').focus();\", 150);", "undefined", "<%=ResString("lblError") %>");
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
        dxConfirm("<%=ResString("msgSureDeleteCC") %>", "sendCommand(\"action=del_column&clmn=" + idx + "\");");
    }

    /* Add-remove-rename items */
    function EditAttributeValue(index) {
        SelectedItemIndex = index;
        var n = document.getElementById("tbCatName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgEmptyCategoryName") %>", "setTimeout(\"document.getElementById('tbCatName').focus();\", 150);", "undefined", "<%=ResString("lblError") %>");
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
        var n = document.getElementById("tbCatName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                dxDialog("<%=ResString("msgCreateEmptyCCName") %>", "setTimeout(\"document.getElementById('tbCatName').focus();\", 150);", "undefined", "<%=ResString("lblError") %>");
            } else {
                sendCommand('action=add_item&name=' + encodeURIComponent(n.value)+'&clmn=' + SelectedAttrIndex);
            }
        }
    }

    function DeleteAttributeValue(index) {
        SelectedItemIndex = index;
        dxConfirm("<%=ResString("msgSureDeleteCategory") %>", "sendCommand(\"action=del_item&item=" + index + "&clmn=" + SelectedAttrIndex + "\");");
    }

    /* Callback */
    function sendCommand(cmd, plsWait) {
        //store scroll position
        var divPos  = document.getElementById("<% =scrollPos.ClientID %>");
        var divGrid = document.getElementById("<% =divGrid.ClientID %>");
        if ((divPos) && (divGrid)) divPos[text] = divGrid.scrollTop + "," + divGrid.scrollLeft;

        if ((on_hit_enter_tmp != "showValuesEditorOnly") || ((cmd.indexOf('action=add_item') != 0) && (cmd.indexOf('action=del_item') != 0) && (cmd.indexOf('action=rename_item') != 0))) {
            on_hit_enter_tmp = "";
        }

        var am = $find("<%=RadAjaxManagerMain.ClientID%>");
        if ((am)) { if (typeof plsWait === "undefined" || plsWait) displayLoadingPanel(true); am.ajaxRequest(cmd); is_ajax = 1; return true; }
        return false;
    }

    function onRequestError(sender, e) {
        is_ajax = 0;
        displayLoadingPanel(false);
        DevExpress.ui.notify("<%=ResString("lblCallbackError") %>\n<%=ResString("lblStatusCode") %>: " + e.Status + "\n<%=ResString("lblResponseText") %>: " + e.ResponseText, "error");
    }

    function onResponseEnd(sender, eventArgs) {
        is_ajax = 0;        

        var action = "";
        var clmn   = -1;
        var item   = -1;
        var name   = "";
        var def_val= "";
        var type   = avtEnumeration;

        //alert(eventArgs.get_eventArgument());

        var res = eventArgs.get_eventArgument().split('&');
        for (var i=0; i<res.length; i++) {
            var s = res[i].split('=');
            if (s[0] == "action") action = s[1];
            if (s[0] == "name")   name   = decodeURIComponent(s[1]);
            if (s[0] == "clmn")   clmn   = parseInt(s[1]);
            if (s[0] == "item")   item   = parseInt(s[1]);
            if (s[0] == "type")   type   = parseInt(s[1]);
            if (s[0] == "def_val")def_val= decodeURIComponent(s[1]);
        }

        var reopen_editor = false;
        var reopen_cat_editor = false;

        switch (action) {
            
            // COLUMNS
            case "add_column":
                SelectedAttrIndex = attr_data.length;
                var items = [];
                
                var new_clmn = [SelectedAttrIndex, name, (type == avtEnumeration || type == avtEnumerationMulti ? [] : def_val), type];
                attr_data.push(new_clmn);
                InitPage();
                reopen_editor = true;
                if (type == avtEnumeration || type == avtEnumerationMulti) reopen_cat_editor = true;
                break;
            case "del_column":                
                attr_data.splice(SelectedAttrIndex, 1);                                                
                InitPage();
                reopen_editor = true;
                break;
            case "rename_column":
                attr_data[SelectedAttrIndex][IDX_ATTR_NAME] = name;
                InitPage();          
                reopen_editor = true;      
                break;
            // ITEMS
            case "add_item":
                if ((SelectedAttrIndex >= 0) && (SelectedAttrIndex < attr_data.length)) {
                    var items = attr_data[SelectedAttrIndex][IDX_ATTR_ITEMS];
                    var new_item = [items.length, name];
                    items.push(new_item);
                    //SelectedItemIndex = items.length - 1;
                    InitPage();
                }
                reopen_editor = true;
                reopen_cat_editor = true;                
                break;
            case "del_item":                
                if ((SelectedAttrIndex >= 0) && (SelectedAttrIndex < attr_data.length)) {
                    var items = attr_data[SelectedAttrIndex][IDX_ATTR_ITEMS];
                    if ((SelectedItemIndex >= 0) && (SelectedItemIndex < items.length)) {                    
                        items.splice(SelectedItemIndex, 1);
                        //if (SelectedItemIndex >= items.length) SelectedItemIndex = items.length - 1;
                        //if (SelectedItemIndex < 0) SelectedItemIndex = 0;
                        InitPage();
                    }                
                }
                reopen_editor = true;
                reopen_cat_editor = true;
                break;
            case "rename_item":
                if ((SelectedAttrIndex >= 0) && (SelectedAttrIndex < attr_data.length)) {
                    var items = attr_data[SelectedAttrIndex][IDX_ATTR_ITEMS];
                    if ((SelectedItemIndex >= 0) && (SelectedItemIndex < items.length)) {
                        items[SelectedItemIndex][IDX_ATTR_NAME] = name;
                        InitPage();
                    }                
                }
                reopen_editor = true;
                reopen_cat_editor = true;
                break;
            case "paste_attribute_data":
                var attr = GetSelectedAttr();
                if ((attr) && (attr[IDX_ATTR_TYPE] == avtEnumeration || attr[IDX_ATTR_TYPE] == avtEnumerationMulti)) {
                    var d = document.getElementById("<% =divSrvData.ClientID %>");
                    if ((d)) {
                        attr[IDX_ATTR_ITEMS] = eval(d[text]);
                        InitPage();
                    }
                }
                break;
            case "paste_default_column":
                break;
            //case "copy_data_to_clipboard":
            //    var d = document.getElementById("<% =divSrvData.ClientID %>");
            //    if ((d)) {
            //        if (window.clipboardData) {
            //            window.clipboardData.setData('Text', eval(d[text])[0]);
            //            dxDialog("Data copied to your clipboard.", false, "", "undefined", "Information", 350, -1);
            //        }                   
            //    }
            //    break;

            case "set_attr_value":
                break;

            case "sort":
                break;

            case "set_default_value":
                initAttributes();
        }        
        
        resizeGrid();
        onSearch($("#tbSearch").val().trim());
        displayLoadingPanel(false);

        //restore scroll position
        var divPos  = document.getElementById("<% =scrollPos.ClientID %>");
        var divGrid = document.getElementById("<% =divGrid.ClientID %>");
        if ((divPos) && (divGrid) && (divPos[text])) {            
            var p = divPos[text].split(",");
            divGrid.scrollTop  = p[0] | 0;
            divGrid.scrollLeft = p[1] | 0;
        }        

        if (reopen_editor && (on_hit_enter_tmp != "showValuesEditorOnly")) {            
            if ((dlg_attributes)) dlg_attributes.dialog("close");
            initAttributes();
            initAttributesDlg();
            if ((on_hit_enter_tmp == "showValuesEditor")) {
                onEditAttributeValues(SelectedAttrIndex, GetSelectedAttr()[IDX_ATTR_TYPE]);
            }
        }
        
        if (reopen_cat_editor) {
            if ((dlg_attributes_cat)) dlg_attributes_cat.dialog("close");
            initAttributesValues();
            initAttributesValuesDlg();
        }

        InitRowSelection();
    }

    /* Init Page */
    function InitPage() {        
        UpdateButtons();
    }

    function cbNormalizeChange(value) {
        sendCommand("action=normalize&value=" + value);
    }

    function cbModeChange(value) {
        sendCommand("action=synthesis&value=" + value);
    }
    
    function cbDecimalsChange(value) {
        sendCommand("action=decimals&value=" + value);
    }

    function cbCategoryChange(altIndex, attrIndex, value) { // edit single caegorical attribute from drop-down
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&enum_idx=" + value, false);
        //sendAjax("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&enum_idx=" + value, false);
    }

    function onEditMultiCategoricalAttributeValue(altIndex, attrIndex) { // edit multi caegorical attribute in a popup window
        SelectedAttrIndex = attrIndex;
        sendAjax("action=get_multi_cat_values&alt_idx=" + altIndex + "&attr_idx=" + attrIndex, true);
    }

    function initMultiCatDialog(data, altIndex) {
        cancelled = false;
        dlg_multi_cat = $("#divSelectMultiCat").dialog({
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
                  Ok: { id: "jDialog_btnOK", text: "OK", click: function() {
                      dlg_multi_cat.dialog( "close" );
                  }},
                  Cancel: function() {
                      cancelled = true;
                      dlg_multi_cat.dialog( "close" );
                  }
              },
              open: function() {
                    $("body").css("overflow", "hidden");

                    // init values
                    var txt = "";
                    for (var i = 0; i < data.length; i++) {
                        txt += "<label for='cbMC" + i + "'><input type='checkbox' class='cb_multi_cat' data-id='" + data[i][1] + "' " + (data[i][0] ? " checked='checked' " : "") + " >" + data[i][2] + "</label><br>";
                    }
                    $("#pValuesList").html(txt);
              },
              close: function() {
                    $("body").css("overflow", "auto");
                    var multi_attr_values = "";
                    $("input.cb_multi_cat").each(function(index, val) {
                        if (val.checked) {
                            var id = val.getAttribute("data-id");
                            multi_attr_values += (multi_attr_values == "" ? "" : ";") + id;
                        }
                    });
                    sendCommand("action=set_multi_cat_values&lst=" + encodeURIComponent(multi_attr_values) + "&clmn=" + SelectedAttrIndex + "&alt_idx=" + altIndex, true);

                    $("#divSelectMultiCat").dialog("destroy");
                    dlg_multi_cat = null;
              },
              resize: function( event, ui ) { }
        });
    }

    function txtAttrValueChange(altIndex, attrIndex, value) {
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&value=" + encodeURIComponent(value));
    }

    function boolAttrValueChange(altIndex, attrIndex, value) {
        sendCommand("action=set_attr_value&alt_idx=" + altIndex + "&attr_idx=" + attrIndex + "&value=" + value);
    }
    
    function onClickToolbar(sender, args) {
        var button = args.get_item();
        if ((button)) {
            var btn_id = button.get_value();
            if ((btn_id) && (btn_id == "btn_edit_attributes")) {
                initAttributes();
                initAttributesDlg();
            }
        }
    }

    function onSetScenario(id) {
        is_ajax = 1;
        displayLoadingPanel(true);
        document.location.href = '<% =PageURL(CurrentPageID) %>?action=scenario<% =GetTempThemeURI(true) %>&sid=' + id;
        return false;
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

                var sHidden = (PgID == _PGID_STRUCTURE_ALTERNATIVES || attr_type == avtEnumeration || attr_type == avtEnumerationMulti ? "" : " style='display:none;' ");
                
                sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "' " + sHidden + ">";
                sRow += "<td " + sHidden + " align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td " + sHidden + " id='tdName" + i + "''>" + n + "</td>";                
                sRow += "<td " + sHidden + " id='tdEditAction" + i + "' align='right'><a href='' onclick='onEditAttribute(" + i + "); return false;'><img src='../../images/ra/edit_small.gif' width='16' height='16' border='0'></a></td>";
                sRow += "<td " + sHidden + " id='tdType" + i + "' align='center'>" + getAttrTypeName(v[IDX_ATTR_TYPE]) + "</td>";
                sRow += "<td " + sHidden + " id='tdValues" + i + "'>" + vals + "</td>";
                sRow += "<td " + sHidden + " id='tdEditValues" + i + "' align='right'><a href='' onclick='onEditAttributeValues(" + i + ","+v[IDX_ATTR_TYPE]+"); return false;'><img src='../../images/ra/edit_small.gif' width='16' height='16' border='0'></a></td>";
                sRow += "<td " + sHidden + " id='tdActions" + i + "' align='center'><a href='' onclick='DeleteAttribute(" + i + "); return false;'><img src='../../images/ra/recycle.gif' width='16' height='16' border='0' alt='Delete'></a></td>";
                sRow +="</tr>";
                t.append(sRow);
            }

            sRow = "<tr class='text grid_footer' id='trNew'>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width: 100%; vertical-align: middle;' id='tbName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'></td>";
            sRow += "<td><select class='select' style='width:100%' id='tbType' onchange='cbNewAttrTypeChanged(this.value)'>" + getAttrTypeOptions() + "</select></td>";
            //sRow += "<td>&nbsp;</td>"; // edit name icon
            sRow += "<td colspan='2'><nobr>&nbsp;<span id='lblDefaultValue' style='display:none'><%=ResString("lblDefaultAttrValue")%>:&nbsp;</span><input type='text' id='tbDefaultTextValue' style='display:none; width:130px;' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'><select id='tbDefaultBoolValue' class='select' style='display:none;width:80px;'><option value='1'><%=ResString("lblYes")%></option><option value='0'><%=ResString("lblNo") %></option></select></nobr></td>"; // values
            //sRow += "<td>&nbsp;</td>"; // edit values icon
            sRow += "<td align='center'><a href='' onclick='AddAttribute(); return false;'><img src='../../images/ra/add-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("titleAddCC")) %>'></a></td></tr>";
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
        <%If CurrentPageID <> _PGID_RA_STRATEGIC_BUCKETS Then%>        
        retVal += "<option value='" + avtEnumerationMulti + "'><%=ResString("optAttrTypeMultiEnum")%></option>";
        retVal += "<option value='-1' disabled='disabled'><%=ResString("optSeparatorLine")%></option>";
        retVal += "<option value='" + avtString + "'><%=ResString("optAttrTypeString")%></option>";
        retVal += "<option value='" + avtLong + "'><%=ResString("optAttrTypeInteger")%></option>";
        retVal += "<option value='" + avtDouble + "'><%=ResString("optAttrTypeDouble")%></option>";
        retVal += "<option value='" + avtBoolean + "'><%=ResString("optAttrTypeBoolean")%></option>";
        <%End If%>
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
                  if (checkUnsavedData(document.getElementById("tbName"), "dlg_attributes.dialog('close')")) dlg_attributes.dialog( "close" );
                }
              },
              open: function() {
                $("body").css("overflow", "hidden");
                on_hit_enter = "AddAttribute();";
                document.getElementById("tbName").focus();
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
                    sRow += "<td id='tdCatActions" + i + "' align='center'><a href='' onclick='onEditAttributeValue(" + i + "); return false;'><img src='../../images/ra/edit_small.gif' width='16' height='16' border='0'></a><a href='' onclick='DeleteAttributeValue(" + i + "); return false;'><img src='../../images/ra/recycle.gif' width='16' height='16' border='0' alt='Delete'></a></td>";
                    sRow +="</tr>";
                    t.append(sRow);
                }
            }

            sRow = "<tr class='text grid_footer' id='trCatNew'>";
            //sRow += "<td align='center' style='width:20px'>&nbsp;</td>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width:100%' id='tbCatName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'></td>";
            sRow += "<td align='center'><a href='' onclick='AddAttributeValue(); return false;'><img src='../../images/ra/add-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("titleAddCategory")) %>'></a></td></tr>";
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
                    var attr = GetSelectedAttr();
                    if ((attr)) {
                        var v = attr[IDX_ATTR_ITEMS];
                        for (var i=0; i<v.length; i++) {
                            v[i][IDX_ATTR_ID] = i; 
                        };
                    }
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
            dxConfirm("<% = JS_SafeString(ResString("msgUnsavedData")) %>", on_agree + ";");
            return false;
        }
        return true;
    }

    function onEditAttribute(index, skip_check) {
        SelectedAttrIndex = index;
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbName"), "onEditAttribute(" + index + ", true)")) return false;
        initAttributes();
        $("#tdName" + index).html("<input type='text' class='input' style='width:" + $("#tdName" + index).width()+ "' id='tbName' value='" + replaceString("'", "&#39;", attr_data[index][IDX_ATTR_NAME]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdEditAction" + index).html("<a href='' onclick='EditAttribute(" + index + "); return false;'><img src='../../images/ra/apply-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbName\").focus(); return false;'><img src='../../images/ra/cancel-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></a>");
        $("#trNew").html("").hide();
        setTimeout("document.getElementById('tbName').focus();", 50);
        on_hit_enter = "EditAttribute(" + index + ");";
    }

    function onEditAttributeValue(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbCatName"), "onEditAttributeValue(" + index + ", true)")) return false;
        initAttributesValues();
        $("#tdCatName" + index).html("<input type='text' class='input' style='width:" + $("#tdCatName" + index).width()+ "' id='tbCatName' value='" + replaceString("'", "&#39;", attr_data[SelectedAttrIndex][IDX_ATTR_ITEMS][index][IDX_ATTR_NAME]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdCatActions" + index).html("<a href='' onclick='EditAttributeValue(" + index + "); return false;'><img src='../../images/ra/apply-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbCatName\").focus(); return false;'><img src='../../images/ra/cancel-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></a>");
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
            //if (!(skip_check) && !checkUnsavedData(document.getElementById("tbName"), "onEditAttribute(" + index + ", true)")) return false;
            if (attr_type == avtBoolean) {
                $("#tdValues"   + index).html("<select id='cbDefValue' class='select' style='width:80px;'><option value='1' " + (attr[IDX_ATTR_DEF_VALUE] == "1" ? " selected='selected' " : " ") + "><%=ResString("lblYes") %></option><option value='0' " + (attr[IDX_ATTR_DEF_VALUE] == "0" ? " selected='selected' " : " ") + "><%=ResString("lblNo") %></option></select>");
            } else {
                $("#tdValues"   + index).html("<input type='text' class='input' style='width:" + $("#tdValues" + index).width()+ "' id='tbDefValue' value='" + htmlEscape(attr[IDX_ATTR_DEF_VALUE]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
            }
            $("#tdEditValues" + index).html("<a href='' onclick='EditDefaultValue(" + index + "); return false;'><img src='../../images/ra/apply-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("btnSaveChanges")) %>'></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbName\").focus(); return false;'><img src='../../images/ra/cancel-16.png' width='16' height='16' border='0' alt='<% = JS_SafeString(ResString("btnCancelChanges")) %>'></a>");
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
            initAttributesValues();
            sendCommand("action=set_default_value&def_val=" + (checked ? "1" : "0") + "&clmn=" + SelectedAttrIndex + "&item_index=" + item_index);
        }
    }

    var is_context_menu_open = false;

    function showMenu(event, uid, hasCopyButton, hasPastButton, isAttribute) {                       
        SelectedAttrIndex = uid;
        var attr_type = avtString;
        var attr = GetSelectedAttr();
        if ((attr)) attr_type = attr[IDX_ATTR_TYPE];

        is_context_menu_open = false;                
        $("#contextmenuheader").hide().remove();
        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
           if (hasCopyButton) sMenu += "<a href='' class='context-menu-item' onclick='doCopyToClipboardValues("+'"'+ uid +'"'+"); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../images/ra/copy2-16.png' alt='' >&nbsp;&nbsp;&nbsp;<%=ResString("titleCopyToClipboard")%>&nbsp;</nobr></div></a>";
           if (hasPastButton) sMenu += "<a href='' class='context-menu-item' onclick='doPasteAttributeValues("+'"'+ uid +'"'+"); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../images/ra/paste-16.png' alt='' >&nbsp;&nbsp;&nbsp;<%=ResString("titlePasteFromClipboard")%>&nbsp;</nobr></div></a>";
           if (isAttribute)   sMenu += "<hr style='height:1px;width:140;margin:0px 10px;color:#ccc;'><a href='' class='context-menu-item' onclick='onEditAttributeValues("+'"'+ uid +'"'+"," + attr_type + "); on_hit_enter_tmp = "+'"showValuesEditorOnly"'+"; return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='../../images/ra/edit_small.gif' alt='' >&nbsp;&nbsp;&nbsp;<%=ResString("btnRAEditCategories")%>&nbsp;</nobr></div></a>";
           sMenu += "</div>";                
        var img = document.getElementById("mnu_img_" + uid);
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

    function canCloseMenu() {
        is_context_menu_open = true;
        $(document).unbind("click").bind("click", function () { if (is_context_menu_open == true) { $("#contextmenuheader").hide(200); is_context_menu_open = false; } });        
    }    

    function doPasteAttributeValues(attr_idx) {
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
            var guids = "";
            $("tr.data_row").each(function (index, row) {
                var guid = row.getAttribute("guid");
                if ((guid)) guids += (guid == "" ? "" : ",") + guid;
            });
            switch (attr_idx+"") {
                case "name":
                case "cost":
                    sendCommand("action=paste_default_column&column="+attr_idx+"&data="+encodeURIComponent(data)+"&guids="+guids);
                    break;
                default:
                SelectedAttrIndex = attr_idx;
                sendCommand("action=paste_attribute_data&attr_idx="+attr_idx+"&data="+encodeURIComponent(data)+"&guids="+guids);
            }
        } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
    }

    function doCopyToClipboardValues(unique_id) {
        var res = "";
        var grid = document.getElementById("<%=GridAlternatives.ClientID %>");
        var row_count = grid.rows.length;        
        var cell_index = -1;
        switch (unique_id+"") {
            case "name":
                cell_index = COL_NAME;
                break;
            case "total":
                cell_index = COL_TOTAL;
                break;
            case "cost":
                cell_index = COL_COST;
                break;
            default:
                if (PgID == _PGID_STRUCTURE_ALTERNATIVES) {
                    cell_index = COL_ATRIBUTES_START_INDEX + unique_id*1;
                } else {
                    cell_index = COL_ATRIBUTES_START_INDEX;
                    for (var i = 0; i < attr_data.length && i < unique_id*1; i++) {
                        if (attr_data[i][IDX_ATTR_TYPE] == avtEnumeration || attr_data[i][IDX_ATTR_TYPE] == avtEnumeration) {
                            cell_index += 1;
                        }
                    }   
                }
                break;
        }
        if ((cell_index >= 0) && (cell_index < grid.rows[0].cells.length)) {
            for (var i=1;i<row_count;i++) { //skip header row           
                var cell = grid.rows[i].cells[cell_index];
                var value= cell.getAttribute('clip_data').toString();
                res += (res==""?"":"\r\n") + value;
            }
        }
        copyDataToClipboard(res);
    }

    /* Copy to Clipboard - Cross-Browser */
    <%--function copyDataToClipboard(data) {
        if (window.clipboardData) {
            if (window.clipboardData.setData('Text', data)) {
                DevExpress.ui.notify("<%=ResString("msgDataCopied") %>", "success");
            } else {
                DevExpress.ui.notify("<%=ResString("msgUnableToCopy") %>", "error");
            }
        } else {            
            if (is_firefox) {                
                dxDialog("<%=ResString("titleNonIECopy") %>:<pre><textarea id='copyBox' cols='48' rows='6'>" + data + "</textarea></pre>", ";", ";", "<%=ResString("titleCopyToClipboard") %>", "<%=ResString("btnCopy") %>", "<%=ResString("btnCancel") %>");
                $("#copyBox").select();
            } else {
                var success = chromeCopyToClipboard(data);
                DevExpress.ui.notify(success?"<%=ResString("msgDataCopied") %>":"<%=ResString("msgUnableToCopy") %>", success?"success":"error");
            }
        }
    }--%>

    function commitPasteChrome(attr_idx) {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(attr_idx, pasteBox.value);
        }
    }

    function resizeGrid() {
        <%--$("#<%=GridAlternatives.ClientID %>").height(10).width(10); //.height($("#tdGridAlts").innerHeight()).width($("#tdGridAlts").width());--%>
    }

    function InitRowSelection() {
        $('#<%=GridAlternatives.ClientID%> tr[guid]').click(function() {
            //$('#<%=GridAlternatives.ClientID%> tr').css({ "background-color": "White", "color": "Black" });
            //$(this).css({ "background-color": "Black", "color": "White" });
            if ($(this).prop("selected") == 0) {
                //$('#<%=GridAlternatives.ClientID%> tr[guid]').prop("selected","0").css({ "font-weight": "normal", "color" : "black" });
                //$(this).prop("selected","1").css({ "font-weight" : "bold", "color" : "black" });
                $('#<%=GridAlternatives.ClientID%> tr[guid]').prop("selected","0").css({ "color" : "black" });
                $(this).prop("selected","1").css({ "color" : "#0000ff" });
                //sendAjax("action=selected_alt_changed&alt_guid="+$(this).prop("guid"));
                sendAjax("action=selected_alt_changed&alt_guid="+$(this)[0].getAttribute("guid").toString(), false);
            }
        });

//        $('#<%=GridAlternatives.ClientID%> tr[guid]').mouseover(function() {
//            $(this).css({ cursor: "default" });
//        });

        InitSelectedAlt();
    }

    function InitSelectedAlt() {
        var d = document.getElementById("<% =divSelectedAltID.ClientID %>");
        if ((d)) {
            SelectedAltID = d[text] + "";
            //$('#<%=GridAlternatives.ClientID%> tr[guid]').prop("selected","0").css({ "font-weight": "normal", "color" : "black" });
            //$('#<%=GridAlternatives.ClientID%> tr[guid='+SelectedAltID+']').prop("selected","1").css({ "font-weight" : "bold", "color" : "black" });
            $('#<%=GridAlternatives.ClientID%> tr[guid]').prop("selected","0").css({ "color" : "black" });
            $('#<%=GridAlternatives.ClientID%> tr[guid='+SelectedAltID+']').prop("selected","1").css({ "color" : "#0000ff" });
        }
    }

    function InitDragDrop() {
        $(function () {
            $(".drag_drop_grid").sortable({
                items: 'tr:not(tr:last-child)',
                cursor: 'crosshair',
                connectWith: '.drag_drop_grid',
                axis: 'y',
                start: function( event, ui ) { drag_index = ui.item.index(); },
                update: function( event, ui ) { onDragIndex(ui.item.index()); }
            });
        });
    }

    var drag_index = -1;
    var attr_order = "";
    var attr_values_order = "";

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

            drag_index = -1;
        }
    }

    function DoSort(idx, dir) {
        sendCommand("action=sort&idx=" + idx + "&dir=" + dir);
    }
    
    var search_old = "";
    function onSearch(value) {
        value = value.trim().toLowerCase();
        if (search_old!="") {
            $('.select option:Contains("' + search_old + '")').css("color", "");
        }
        if (value == "") {
            $("tr.grid_row, tr.grid_row_alt").show().each(function (index, row) {
                var attr = row.cells[COL_NAME].getAttribute("clip_data");
                if ((attr)) row.cells[COL_NAME].innerHTML = attr;
            });
        } else {
            $("tr.grid_row, tr.grid_row_alt").each(function (index, row) { 
                var anyCellContains = false;
                var cells_len = row.cells.length;                
                var r = new RegExp(value, "gi");
                for (var i=0; i<cells_len; i++) {
                    var attr = row.cells[i].getAttribute("clip_data");
                    if (i==COL_NAME && (attr)) row.cells[i].innerHTML = attr;
                    if ((attr) && (attr.toLowerCase().indexOf(value) >= 0)) {
                        if (i==COL_NAME) row.cells[i].innerHTML = row.cells[i].innerHTML.replace(r, "<span style='font-weight:bold; color:#0066cc;'>$&</span>");
                        anyCellContains = true;
                        i = cells_len;
                    }     
                }
                //row.style.visibility = (anyCellContains ? "visible" : "hidden");
                row.style.display = (anyCellContains ? "" : "none");
            });
            $('.select option:Contains("' + value + '")').css("color", "#0066cc");
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
        setTimeout("$('#tbSearch').focus();", 200);
    }
    
    /* jQuery Ajax */
    function syncReceived(params) {
        var msg = "";
        if ((params)) {            
            var received_data = eval(params);
            if ((received_data)) {
                if (received_data[0] == "get_multi_cat_values") {
                    var data = received_data[1];
                    var altIndex = received_data[2];
                    initMultiCatDialog(data, altIndex);
                }              
            }
        }
        
        displayLoadingPanel(false);
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function sendAjax(params, please_wait) {
        cmd = params;

        if (typeof please_wait == "undefined" || please_wait) displayLoadingPanel(true);

        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }
    /* end jQuery Ajax */

    function Hotkeys(event) {
       if (!document.getElementById) return;
       if (window.event) event = window.event;
       if (event) {
            var code = (event.keyCode ? event.keyCode : event.which ? event.which : null);
            if ((code == ENTERKEY) || (code == TABKEY)) {
                if (on_hit_enter != "" && (!(dlg_attributes_cat) || !dlg_attributes_cat.dialog("isOpen"))) {
                    eval(on_hit_enter);
                    if (code == TABKEY) on_hit_enter_tmp = "showValuesEditor";
                }
                if (on_hit_enter_cat != "") {                    
                    eval(on_hit_enter_cat);
                    on_hit_enter_cat = "";
                }
                return false;
            }
        }
    }

    resize_custom  = resizeGrid;
    //document.onkeypress = Hotkeys; - keypress doesn't catch the TAB key
    //document.addEventListener("keydown", Hotkeys, false);
    document.onkeydown = Hotkeys;
    $(document).ready( function () { InitDragDrop(); InitRowSelection(); InitSearch(); resizeGrid(); ResetRadToolbar(); setTimeout("var tb = $('input.attrinput:first').focus();",500); });

</script>
</telerik:RadScriptBlock>

<telerik:RadAjaxManager ID="RadAjaxManagerMain" runat="server" ClientEvents-OnRequestError="onRequestError" ClientEvents-OnResponseEnd="onResponseEnd" EnableAJAX="true" EnableHistory="false" EnablePageHeadUpdate="false">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="divGrid">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="divGrid"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<div id='scrollPos' runat="server" style='display:none'></div>

<table border='0' cellspacing='0' cellpadding='0' class='whole' id='divMain'>
<tr style='height:1em'>
    <td id='tdToolbar' style="height:24px;" class="text">
    <telerik:RadToolBar ID="RadToolBarMain" runat="server" CssClass="dxToolbar" Skin="Default" Width="100%" AutoPostBack="false" EnableViewState="false" OnClientButtonClicked="onClickToolbar">
    <Items>        
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lblOptScenario">
            <ItemTemplate><span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbOptScenario" Enabled="false">
            <ItemTemplate><% =GetScenarios%></ItemTemplate>
        </telerik:RadToolBarButton>

        <telerik:RadToolBarButton ID='btnAddAlternative' runat="server" EnableViewState="false" AllowSelfUnCheck="True" ImageUrl="~/App_Themes/EC09/images/add_alt_20.png" CheckOnClick="False" Checked="False" Text="add_alternative" Value="add_alternatives" Enabled="false" Visible="false"/>
        <telerik:RadToolBarButton ID='btnDeleteNode' runat="server" EnableViewState="false" AllowSelfUnCheck="True" ImageUrl="~/App_Themes/EC09/images/del_20.png" CheckOnClick="False" Checked="False" Text="Delete" Value="delete_node" Enabled="false" Visible="false"/>

        <telerik:RadToolBarButton runat="server" EnableViewState="false" Text="Edit Attributes" ImageUrl="~/App_Themes/EC09/images/config-20.png" Value="btn_edit_attributes" OnLoad="OnLoadEditButton"/>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" >
            <ItemTemplate>&nbsp;|&nbsp;</ItemTemplate>
        </telerik:RadToolBarButton>

        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_totals" Visible="false">
            <ItemTemplate><span class='toolbar-label'><%=ResString("lblTotals") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbNormalizedMnuBtn" Enabled="false" Visible="false">
            <ItemTemplate>
                <select name='cbNormalize' style='width:17ex; margin-right:3px; margin-top:3px;' onchange='cbNormalizeChange(this.value);'>
                      <option value='0'<% =iif(NormalizeMode=0, " selected", "") %>><%=ResString("optUnnormalize")%></option>
                      <option value='1'<% =iif(NormalizeMode=1, " selected", "") %>><%=ResString("optPercentOfMax")%></option>
                      <option value='2'<% =iif(NormalizeMode=2, " selected", "") %>><%=ResString("optNormPriority")%></option>
                      <option value='3'<% =iif(NormalizeMode=3, " selected", "") %>><%=ResString("optMultOfMin")%></option>
                </select>        
            </ItemTemplate>
        </telerik:RadToolBarButton>
        
        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_mode" Visible="false">
            <ItemTemplate><span class='toolbar-label'><%=ResString("lblMode") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbModeMnuBtn" Enabled="false" Visible="false">
            <ItemTemplate>
                <select name='cbMode' style='width:17ex; margin-right:3px; margin-top:3px;' onchange='cbModeChange(this.value);'>
                      <option value='0'<% = iif(SynthesisMode = 0, " selected", "") %>><%=ResString("optIdeal")%></option>
                      <option value='1'<% = iif(SynthesisMode = 1, " selected", "") %>><%=ResString("optDistributive")%></option>
                </select>
            </ItemTemplate>
        </telerik:RadToolBarButton>        

        <telerik:RadToolBarButton runat="server" EnableViewState="false" Value="lbl_decimals">
            <ItemTemplate><span class='toolbar-label'><%=ResString("lblDecimals") + ":"%>&nbsp;</span></ItemTemplate>
        </telerik:RadToolBarButton>
        <telerik:RadToolBarButton runat="server" EnableViewState="false" CheckOnClick="false" Value="cbDecimalsMnuBtn" Enabled="false">
            <ItemTemplate>
                <select class='select' name='cbDecimals' style='width:7ex; margin-right:3px; margin-top:3px;' onchange='cbDecimalsChange(this.value);'>
                      <option value='0'<% =iif(PM.Parameters.DecimalDigits=0, " selected", "") %>>0</option>
                      <option value='1'<% =iif(PM.Parameters.DecimalDigits=1, " selected", "") %>>1</option>
                      <option value='2'<% =iif(PM.Parameters.DecimalDigits=2, " selected", "") %>>2</option>              
                      <option value='3'<% =iif(PM.Parameters.DecimalDigits=3, " selected", "") %>>3</option>
                      <option value='4'<% =iif(PM.Parameters.DecimalDigits=4, " selected", "") %>>4</option>
                      <option value='5'<% =iif(PM.Parameters.DecimalDigits=5, " selected", "") %>>5</option>
                </select>        
            </ItemTemplate>
        </telerik:RadToolBarButton>
                                       
    </Items>
</telerik:RadToolBar>
</td>
</tr>

<tr valign='top' >
<td style='height:40px'>
<telerik:RadCodeBlock runat="server" ID="RadCodeBlockTitle">
    <h5 style='padding:1ex 1ex 0.5ex 1ex'><%=ResString(CStr(IIf(CurrentPageID = _PGID_RA_STRATEGIC_BUCKETS, "titleStrategicBuckets", "titleAlternatives")))%>&nbsp;&quot;<% = ShortString(App.ActiveProject.ProjectName, 45)%>&quot;</h5>
    <h6 style='padding-left:15em'><span id="divSearchBox" class="text" style='float:right; width:18em; text-align:right; padding-bottom:4px;'><nobr><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearch" type="text" style="width:120px;" value="" onkeyup="onSearch(this.value); return false;" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' /></label></nobr></span>(&quot;<%=SafeFormString(ShortString(RA.Scenarios.ActiveScenario.Name, 45, True))%>&quot;)</h6>
    <% =IIf(AlternativesCount > 0 , SolverStateHTML(RA.Solver), "")%>
</telerik:RadCodeBlock>
</td></tr>

<tr valign='top'>
<td id='tdGridAlts' align='center' valign='top'>    
    <div id="divGrid" runat="server" style="overflow:auto; text-align:center; margin-top:0px;">                
        <div id="divSrvData" runat="server" style="display:none;"></div>
        <div id="divSelectedAltID" runat="server" style="display:none;"></div>        
        <asp:GridView EnableViewState="false" AutoGenerateColumns="true" AllowSorting="false" AllowPaging="false" ID="GridAlternatives" runat="server" BorderWidth="0" BorderColor="#e0e0e0" CellSpacing="1" CellPadding="0" TabIndex="1" HorizontalAlign="Center" ShowFooter="false" Width="99%" Height="100%">
            <RowStyle VerticalAlign="Middle" CssClass="text grid_row data_row" Height="2em" Wrap="false"/>
            <AlternatingRowStyle VerticalAlign="Middle" CssClass="text grid_row_alt data_row" Height="2em" Wrap="false"/>
            <HeaderStyle CssClass="text grid_header actions" Wrap="false"/>
            <EmptyDataRowStyle CssClass="text grid_row" />
            <EmptyDataTemplate><h6 style='margin:8em'><nobr><%=ResString("msgRiskResultsNoData")%></nobr></h6></EmptyDataTemplate>
        </asp:GridView>                 
    </div>
</td></tr>
</table>

<div id="divAttributes" style="display:none; text-align:center">
<telerik:RadCodeBlock runat="server" ID="RadCodeBlockDivAttributes">
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
</table></div></telerik:RadCodeBlock>
</div>

<div id="divAttributesValues" style="display:none; text-align:center">
<telerik:RadCodeBlock runat="server" ID="RadCodeBlockDivAttributesValues">
<%--<h6><%=JS_SafeString(ResString("lblRACategories"))%>:</h6>--%>
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
</table></div></telerik:RadCodeBlock>
</div>

<div id="divSelectMultiCat" style="display:none; text-align:center">
  <div style="overflow:auto; text-align:left; padding:5px;" id="pValuesList">
  </div>
</div>

</asp:Content>