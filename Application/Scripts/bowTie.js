/* EC Bow-Tie control by DA, this control also requires misc.js */

// selected_alt_data
// likelihood_data
// impact_data
// control_data

/* Resources */
var CanUserSelectBoxes = false;
var selectedBoxGuidID = "";
var LegendA = "L"; // L - Likelihood of Source 
var LegendB = "V"; // V - Vulnerability 
var LegendC = "C"; // C - Consequence of Event on Objective
var LegendD = "P"; // P - Priority of Objective
var lblEmptyMessage = "No data to display";
var lblNoSource = "No specific source";
var SourceGoalGuid = "";
var lblControlActive = "active";
var lblControlDeactivate = "Deactivate";
var lblControlActivate = "Activate";
var lblControlEdit = "Edit";
var lblControlInfodoc = "Edit Description";
var lblControlEval = "Evaluate Effectiveness";
var lblControlDelete = "Delete";
var lblConfirmation = "Confirmation";
var lblSureRemoveContribution = "Are you sure you want to remove the contribution?";
var lblBEvent = "Event";
var ImagesPath = "";
var lblControl = "Control";
var lblControls = "Controls";
var _PGURL_RICHEDITOR = '20199';
var _TEMP_THEME = 'temptheme=sl';
var showProducts = true;
/* end Resources */

var mh = 500;  // maxHeight for dialogs;

var NODE_ID = 0;
var NODE_TITLE = 1;
var NODE_L = 2;
var NODE_V = 3;
var NODE_LV = 4;
var NODE_PATH = 5;
var NODE_TYPE = 6; // Category = 1; Uncertainty = 0
var NODE_IS_SOURCE_EVENT = 7;
var NODE_HAS_INFODOC = 8;
var NODE_CUR_CONTROLS = 9;
var NODE_IDX_COORDS = 10;

var ALT_ID = 0;
var ALT_TITLE = 1;
var ALT_L = 2; //likelihood
var ALT_I = 3; //impact
var ALT_R = 4;  //risk
var ALT_COLOR = 5;  // ellipse fill color
var ALT_HAS_CONTRIBUTIONS_L = 6;  // if 0 then is event with no sources
var ALT_HAS_CONTRIBUTIONS_I = 7;  // Impact
var ALT_HAS_INFODOC = 8;
var ALT_ATTR_VIEW_DATA = 9;
var ALT_ATTR_FULL_DATA = 10;
//var ALT_I_DOLL = 11;
//var ALT_R_DOLL = 12;

//var ATTR_GUID = 0;
//var ATTR_NAME = 1;
//var ATTR_VALS = 2;

var IDX_ATTR_ID     = 0;
var IDX_ATTR_NAME   = 1;    
var IDX_ATTR_ITEMS  = 2;
var IDX_ATTR_TYPE   = 3;

var IDX_ATTR_ITEM_ID = 0;
var IDX_ATTR_ITEM_NAME = 1;
var IDX_ATTR_ITEM_DEFAULT = 2;
var IDX_ATTR_ITEM_CHECKED = 3;

/* controls types */
var ctCause = 0;
var ctVulnerability = 3;
var ctConsequence = 4;

var ntUncertainty = 0; // RiskNodeType = Uncertainty
var ntCategory = 1; // RiskNodeType = Category

/* ids for Controls */
var IDX_GUID = 0;
var IDX_NAME = 1;
var IDX_PARENT_ID = 2;
var IDX_APPLICATIONS = 2; //Treatments applications array
var IDX_SCALE_TYPE = 2;   //Measurement scale type (for "scale_data" array)
var IDX_LEVEL = 3;
var IDX_COST = 3;         // Cost of Control
var IDX_DESCRIPTION = 4;         // Control Description
var IDX_TERMINAL_NODES_COUNT = 4;
var IDX_CONTRIBUTED_ALTS = 5;
var IDX_CATEGORIES = 5;
var IDX_CONTROL_TYPE = 6;
var IDX_CONTROL_ENABLED = 7;

/* ids for Controls assignments */
var IDX_APPL_GUID = 0;
var IDX_APPL_COMMENT = 1;
var IDX_APPL_VALUE = 2;
var IDX_APPL_OBJ_GUID = 3;
var IDX_APPL_EVENT_GUID = 4;
var IDX_APPL_MEASURETYPE = 5;
var IDX_APPL_SCALE_GUID = 6;
var IDX_APPL_IS_ACTIVE = 7; // "1", "0"
var IDX_APPL_CTRL_INDEX = 8; // "1", "0"

var highlightedControlIndex = -1;

// bow-tie settings
var BT_IsDiagramView = false; // Event shape = rectangle, no priorities, no controls
var BT_ActiveHID = 0;

var CanShowAllPriorities = false;
var CanShowUpperLevelNodes = false;
var CanShowBowTieBackground = false;
var CanShowBowTieAttributes = false;
//var CanUserEditControls = false;
var CanUserRemoveContribution = true;
var CanUserEvaluateControls = false;
var CanDrawNoSourcesNode = true;
var ShowTreatmentIndices = false;
var ShowTreatmentNames = false;
var ShowAllNodesExpanded = false;

var DefaultRectangleHeight = 60;
var DefaultRectangleWidth = 234;
var DefaultRectangleMargin = 10;
var DefaultRectangleMarginWithNodePath = 18;
var DefaultEllipseSize = 75;
var DefaultCenterY = 70;
var CanShowRiskColorCoding = true;
var ControlRowHeight = 20;

var STROKE_COLOR = "#aaa";
var STROKE_THICKNESS = "2";    // selected box border thickness
var SELECTED_STROKE_COLOR     = "#326cd1";  //"#5d8bdb";  // selected box border color
var SELECTED_STROKE_THICKNESS = "3";    // selected box border thickness

var FONT_COLOR = "#222"; // "#fff";

var LIKELIHOOD_COLOR_0 = "#d1e6b5"; // likelihood light gradient part
var LIKELIHOOD_COLOR_1 = "#8abf45"; // likelihood dark gradient part
var IMPACT_COLOR_0 = "#9cd2ff"; // impact light gradient part
var IMPACT_COLOR_1 = "#038cfc"; // impact dark gradient part
var NO_SOURCES_COLOR_0 = "#f5f5f5"; // no sources light gradient part
var NO_SOURCES_COLOR_1 = "#dddddd"; // no sources dark gradient part
var SOURCE_EVENT_COLOR_0 = "#fff7d9"; // source event (edge) light gradient color
var SOURCE_EVENT_COLOR_1 = "#ffd642"; // source event (edge) dark gradient color

var EVENT_COLOR = "#eedfee";
var LIKELIHOOD_COLOR; // = "#6f9c35";
var IMPACT_COLOR;     // = "#025599";
var NO_SOURCES_COLOR; // = "#ddd";
var SOURCE_EVENT_COLOR;

var BACK_LIKELIHOOD_COLOR = "#d3e6ba";
var BACK_IMPACT_COLOR = "#d1eaff";

var HIGHLIGHT_COLOR_ACTIVE = "#faf74d";
//var HIGHLIGHT_COLOR_INACTIVE = "#fffacd";

var BOWTIE_FONT = "12px arial";
var EVENT_FONT = "12px arial";
var NO_SOURCES_FONT = "12px arial";


var CenterX = 300;
var CenterY = DefaultCenterY + DefaultEllipseSize - (CanShowUpperLevelNodes ? 30 : 25);

var canvasID = "";
var tbl_data = [];
var activateClick;
var right_click_ctrl_id = "";
var right_click_node_id = "";
var right_click_appl_id = "";

function drawBowTie(canvas_id) {                    
    canvasID = canvas_id;
    tbl_data = [];
    var canvas = document.getElementById(canvas_id);
    if ((typeof G_vmlCanvasManager != 'undefined') && (G_vmlCanvasManager)) G_vmlCanvasManager.initElement(canvas);
    if (!canvas.getContext) return false;
    var ctx = canvas.getContext("2d");

    // add mouse events listeners
    if (canvas.addEventListener) {
        canvas.addEventListener("mouseup", doMouseUp, false);
        canvas.addEventListener("mousedown", doMouseDown, false);
        canvas.addEventListener("mousemove", doMouseMove, false);
    } else {
        canvas.attachEvent("onclick", doMouseDown);
    }

    // clear controls div
    var d = $get("divControls");
    if ((d)) d.innerHTML = "";
    
    ctx.lineWidth = STROKE_THICKNESS;
    ctx.font = BOWTIE_FONT;
        
    CenterX = (canvas.width) / 2;
    if (BT_IsDiagramView) CenterX = canvas.width - DefaultRectangleWidth / 2;
    if (BT_IsDiagramView && (likelihood_data.length == 0)) CenterX = DefaultRectangleWidth / 2;

    var max = (likelihood_data.length > impact_data.length ? likelihood_data.length : impact_data.length);
    var boxHeight = ShowAllNodesExpanded ? DefaultRectangleHeight + 3 * ControlRowHeight : DefaultRectangleHeight;
    var height = max * boxHeight + max * (CanShowUpperLevelNodes ? DefaultRectangleMarginWithNodePath : DefaultRectangleMargin) + (CanShowUpperLevelNodes ? 6 : 0);
    var sel_node = getSelectedBoxNode(selectedBoxGuidID);
    if (sel_node) height += 3 * ControlRowHeight;

    canvas.height = (height > 220 ? height : 220);

    // init gradients (for IE with compatibility mode)
    LIKELIHOOD_COLOR = createSimpleGradient(ctx, LIKELIHOOD_COLOR_0, LIKELIHOOD_COLOR_1, 0, 0, 0, DefaultRectangleHeight);
    IMPACT_COLOR = createSimpleGradient(ctx, IMPACT_COLOR_0, IMPACT_COLOR_1, 0, 0, 0, DefaultRectangleHeight);
    NO_SOURCES_COLOR = createSimpleGradient(ctx, NO_SOURCES_COLOR_0, NO_SOURCES_COLOR_1, 0, 0, 0, DefaultRectangleHeight);
    SOURCE_EVENT_COLOR = createSimpleGradient(ctx, SOURCE_EVENT_COLOR_0, SOURCE_EVENT_COLOR_1, 0, 0, 0, DefaultRectangleHeight);

    //fill the background white so that it wouldn't be black when saved as image
    ctx.fillStyle = "#fff";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    //var has_data_to_display = !((selected_alt_data.length == 0) || ((likelihood_data.length == 0) && (impact_data.length == 0)));
    var has_data_to_display = selected_alt_data.length > 0;
        
    if (has_data_to_display) {
        if (CanShowBowTieBackground) {
            ctx.fillStyle = BACK_LIKELIHOOD_COLOR;
            ctx.beginPath();
            ctx.moveTo(0, 0);
            ctx.lineTo(DefaultRectangleWidth * 1.8 + 10, 0);
            ctx.lineTo(CenterX, CenterY);
            ctx.lineTo(DefaultRectangleWidth * 1.8 + 10, canvas.height);
            ctx.lineTo(0, canvas.height);
            ctx.lineTo(0, 0);
            ctx.fill();

            ctx.fillStyle = BACK_IMPACT_COLOR;
            ctx.beginPath();
            ctx.moveTo(canvas.width, 0);
            ctx.lineTo(canvas.width - DefaultRectangleWidth * 1.8 - 10, 0);
            ctx.lineTo(CenterX, CenterY);
            ctx.lineTo(canvas.width - DefaultRectangleWidth * 1.8 - 10, canvas.height);
            ctx.lineTo(canvas.width, canvas.height);
            ctx.lineTo(canvas.width, 0);           
            ctx.fill();
        }

        var Offset = (CanShowUpperLevelNodes ? DefaultRectangleMarginWithNodePath : 5);
        //if (!BT_IsDiagramView || BT_ActiveHID == 0)
            drawDiagram(canvas, ctx, likelihood_data, true, Offset);
        //if (!BT_IsDiagramView || BT_ActiveHID == 2) 
            drawDiagram(canvas, ctx, impact_data, false, Offset);
        
        var radius = DefaultEllipseSize; // Arc radius                        
        var startAngle = 0; // Starting point on circle
        var endAngle = 2 * Math.PI; // End point on circle
        
        ctx.fillStyle = (CanShowRiskColorCoding && (selected_alt_data.length > 0) ? createSimpleGradient(ctx, colorBrightness(selected_alt_data[ALT_COLOR], 80), selected_alt_data[ALT_COLOR], CenterX, CenterY-radius, CenterX, CenterY+radius): "#fff");
        ctx.beginPath();
        if (BT_IsDiagramView) {
            ctx.fillRect(CenterX - DefaultRectangleWidth/2, CenterY - DefaultRectangleHeight, DefaultRectangleWidth, DefaultRectangleHeight*2);
            ctx.strokeRect(CenterX - DefaultRectangleWidth/2, CenterY - DefaultRectangleHeight, DefaultRectangleWidth, DefaultRectangleHeight*2);
        } else {
            ctx.arc(CenterX, CenterY - (CanShowUpperLevelNodes ? 0 : 5), radius, startAngle, endAngle, false);
            ctx.fill();
            ctx.stroke();
        }

        //var maxWidth = DefaultEllipseSize * 1.4;        
        var maxWidth = DefaultEllipseSize * 1.6;
        var maxHeight = DefaultEllipseSize * 1.6;
        var lineHeight = 18;
        ctx.fillStyle = "#000";
        ctx.font = EVENT_FONT;
        //wrapText(ctx, ShortString((selected_alt_data.length > 0 ? selected_alt_data[ALT_TITLE] : ""), 60, false), CenterX - DefaultEllipseSize /1.5 - 2, CenterY - DefaultEllipseSize /2 + 15, maxWidth, lineHeight, true);
        var eLeft = CenterX - DefaultEllipseSize * 1.6 / 2 + 2;
        var eTop = CenterY - DefaultEllipseSize * 1.6 / 2 - 5;
        rectText(ctx, (selected_alt_data.length > 0 ? selected_alt_data[ALT_TITLE] : ""), ctx.fillStyle, eLeft, eTop, maxWidth, maxHeight, getTextHeight(BOWTIE_FONT), 2);
        if (selected_alt_data.length > 0) {
            rectText(ctx, lblBEvent, ctx.fillStyle, eLeft - 2, eTop - 90, maxWidth, maxHeight, getTextHeight(BOWTIE_FONT), 2);
        }        

        addTooltipDiv(selected_alt_data.length > 0 ? selected_alt_data[ALT_TITLE] : "", eLeft, eTop, maxWidth, maxHeight);
        if (selected_alt_data.length > 0) {
            addInfodocIcon(selected_alt_data[ALT_ID], selected_alt_data[ALT_HAS_INFODOC] == 1, (BT_IsDiagramView ? CenterX + DefaultRectangleWidth / 2 - 20 : CenterX - 8), (BT_IsDiagramView ? CenterY - DefaultRectangleWidth / 4 : CenterY - DefaultEllipseSize + 3), 'OpenRichEditor("?type=2&field=infodoc&guid=' + selected_alt_data[ALT_ID] + '&callback=' + selected_alt_data[ALT_ID] + '");');
            if (CanShowBowTieAttributes) {
                addAttributesDiv(selected_alt_data[ALT_ID], selected_alt_data[ALT_ATTR_VIEW_DATA], CenterX - DefaultEllipseSize, CenterY + DefaultEllipseSize - 3, DefaultEllipseSize * 2, 60);
            }
        }
    } else {
        // no data to display
        ctx.font = "bold 12px arial";
        ctx.fillStyle = "#00f";
        wrapText(ctx, lblEmptyMessage, CenterX - 135, CenterY - 10, 240, 30, true);
    }
        
    var lnk = document.getElementById("downloadLnk"); if ((lnk)) lnk.onclick = saveAsImage;
}

function addTooltipDiv(title, tLeft, tTop, tWidth, tHeight) {
    var d = $get("divControls");
    if ((d)) {
        var tooltip_div = document.createElement("div");
        tooltip_div.title = title;
        tooltip_div.style.position = "absolute";
        tooltip_div.style.overflowY = "hidden";
        tooltip_div.style.overflowX = "hidden";
        tooltip_div.style.left = tLeft + "px";
        tooltip_div.style.top = tTop + "px";
        tooltip_div.style.width = tWidth + "px";
        tooltip_div.style.height = tHeight + "px";
        d.appendChild(tooltip_div);
        return tooltip_div;
    }
    return null;
}

function addInfodocIcon(guid, has_infodoc, tLeft, tTop, on_click) {
    var d = $get("divControls");
    if ((d)) {
        var a_infodoc = document.createElement("a");
        a_infodoc.class = "actions infodoc_icon";
        a_infodoc.title = "Add/Edit Information Document";
        a_infodoc.style.position = "absolute";
        a_infodoc.style.overflowY = "hidden";
        a_infodoc.style.overflowX = "hidden";
        a_infodoc.style.left = tLeft + "px";
        a_infodoc.style.top = tTop + "px";
        a_infodoc.style.width = "15px";
        a_infodoc.style.height = "15px";
        //a_infodoc.cursor = "hand";
        //a_infodoc.onclick = on_click;
        a_infodoc.innerHTML = "<img guid='" + guid + "' src='" + ImagesPath + (has_infodoc ? "info15.png" : "info15_dis.png") + "' alt='' style='width:15px;height:15px;cursor:hand;' onclick='" + on_click + "'/>";
        d.appendChild(a_infodoc);
    }
}

function addAttributesDiv(guid, data, tLeft, tTop, tWidth, tHeight) {
    var d = $get("divControls");
    if ((d) && (data) && data.length > 0) {
        var div_attr = document.createElement("div");
        div_attr.class = "text small";
        div_attr.style.position = "absolute";
        div_attr.style.left = tLeft + "px";
        div_attr.style.top = tTop + "px";
        div_attr.style.width = tWidth + "px";
        div_attr.style.height = tHeight + "px";
        div_attr.style.padding = "2px 2px";
        div_attr.style.textAlign = "left";
        div_attr.style.overflowY = "auto";
        div_attr.style.overflowX = "hidden";
        div_attr.style.backgroundColor = "yellow";
        for (var i = 0; i < data.length; i++) {
            var span_attr = document.createElement("span");                                    
            span_attr.style.overflowY = "hidden";
            span_attr.style.overflowX = "hidden";
            span_attr.width = tWidth - 35 + "px";
            span_attr.title = data[i][IDX_ATTR_NAME] + " : " + data[i][IDX_ATTR_ITEMS];
            span_attr.innerHTML = data[i][IDX_ATTR_NAME] + " : " + data[i][IDX_ATTR_ITEMS];
            div_attr.appendChild(span_attr);
            var img_attr = document.createElement("img");
            img_attr.src = ImagesPath + "menu_dots.png";
            img_attr.alt = "";
            img_attr.style.width = "12px";
            img_attr.style.height = "12px";
            img_attr.style.cursor = "hand";
            img_attr.setAttribute("onclick", "showEventAttributeMenu('" + guid + "','" + data[i][IDX_ATTR_ID] + "');");
            div_attr.appendChild(img_attr);
            div_attr.appendChild(document.createElement("br"));
        }
        d.appendChild(div_attr);
    }
}

function showEventAttributeMenu(event_guid, attr_guid) {
    is_context_menu_open = false;
    $("div.context-menu").hide().remove();
    var sMenu = "<div class='context-menu'>";
    sMenu += "<a href='' class='context-menu-item' title='' onclick='editEventAttribute(" + '"' + event_guid + '"' + "," + '"' + attr_guid + '"' + "); return false;' style='text-align:left;'><div><nobr><img align='left' style='vertical-align:middle;' src='" + ImagesPath + "edit_small.gif' alt='' width='16' height='16' border='0' />&nbsp;&nbsp;&nbsp;Edit&nbsp;Categories&nbsp;</nobr></div></a>";
    sMenu += "</div>";
    var x = event.clientX;
    var y = event.clientY + $(window).scrollTop();
    var s = $(sMenu).appendTo("#divMain").css({ top: y + "px", left: x + "px" });
    $("div.context-menu").fadeIn(500);
    setTimeout('canCloseMenu()', 200);
}

// Edit Categories
var SelectedAttrID = "";
var SelectedAttr = "";
var dlg_attributes_cat = null;
var attr_values_order = "";

function editEventAttribute(event_guid, attr_guid) {
    //alert(event_guid + " : " + attr_guid);
    SelectedAttrID = event_guid;
    var attr = [];
    for (var i = 0; i < selected_alt_data[ALT_ATTR_FULL_DATA].length; i++) {
        if (attr_guid == selected_alt_data[ALT_ATTR_FULL_DATA][i][IDX_ATTR_ID]) {
            attr = selected_alt_data[ALT_ATTR_FULL_DATA][i];
        }
    }
    SelectedAttr = attr;
    var attr_type = attr[IDX_ATTR_TYPE];

    if (attr_type == avtEnumeration || attr_type == avtEnumerationMulti) {
        InitAttributesValues();
        InitAttributesValuesDlg();
        dlg_attributes_cat.dialog("open");
    }
}

function InitAttributesValues() {
    var t = $("#tblAttributesValues tbody");
    if ((t)) {
        t.html("");        
            
        var attr = SelectedAttr;
        if ((attr)) {            
            for (i = 0; i < attr[IDX_ATTR_ITEMS].length; i++) {
                var v = attr[IDX_ATTR_ITEMS][i]
                var n = htmlEscape(v[IDX_ATTR_NAME]);
                var isDefault = v[IDX_ATTR_ITEM_DEFAULT] == 1;
                var isChecked = v[IDX_ATTR_ITEM_CHECKED] == 1;
                sRow = "<tr class='text " + ((i&1) ? "grid_row" : "grid_row_alt") + "'>";
                //sRow += "<td align='center' style='width:20px'><span class='drag_vert'>&nbsp;&nbsp;</span></td>";
                sRow += "<td align='center' style='width:0px;'><span>&nbsp;&nbsp;</span></td>";
                sRow += "<td id='tdCatName" + i + "''>" + n + "</td>";
                sRow += "<td align='center' id='tdCatIsChecked" + i + "''><input type='checkbox' class='checkbox' onclick='onEnumCheckedClick(" + i + ",this.checked);' " + (isChecked ? "checked='checked'" : "") + "></td>";
                sRow += "<td align='center' id='tdCatIsDefault" + i + "''><input type='checkbox' class='checkbox' onclick='onEnumDefaultClick(" + i + ",this.checked);' " + (isDefault ? "checked='checked'" : "") + " " + (attr[IDX_ATTR_TYPE] == avtEnumerationMulti ? " disabled='disabled' " : "") + "></td>";
                sRow += "<td id='tdCatActions" + i + "' align='center'><a href='' onclick='onEditAttributeValue(" + i + ",\"" + n + "\"); return false;'><img src='" + ImagesPath + "edit_small.gif' width='16' height='16' border='0'></a><a href='' onclick='onDeleteAttributeValue(" + i + ",\"" + n + "\"); return false;'><img src='" + ImagesPath + "recycle.gif' width=16 height=16 border=0 alt='Delete'></a></td>";
                sRow +="</tr>";
                t.append(sRow);
            }
        }

        sRow = "<tr class='text grid_footer' id='trCatNew'>";
        //sRow += "<td align='center' style='width:20px'>&nbsp;</td>";
        sRow += "<td colspan='3'><input type='text' class='input' style='width:100%' id='tbCatName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'></td>";
        sRow += "<td align='center'><a href='' onclick='onAddAttributeValue(); return false;'><img src='" + ImagesPath + "add-16.png' width=16 height=16 border=0 alt='Add Category'></a></td></tr>";
        t.append(sRow);
        if ((dlg_attributes_cat) && dlg_attributes_cat.dialog("isOpen")) on_hit_enter_cat = "onAddAttributeValue();";
    }
}

function onEnumDefaultClick(item_index, checked) {
    var attr = SelectedAttr;
    if ((attr) && (attr[IDX_ATTR_TYPE] == avtEnumeration || attr[IDX_ATTR_TYPE] == avtEnumerationMulti)) {
        for (var i = 0; i < attr[IDX_ATTR_ITEMS].length; i++) {
            attr[IDX_ATTR_ITEMS][i][IDX_ATTR_ITEM_DEFAULT] = ((i == item_index) && checked ? "1" : "0");
        }
        InitAttributesValues();
        sendCommand("action=set_default_value&chk=" + (checked ? "1" : "0") + "&attr=" + attr[IDX_ATTR_ID] + "&item_index=" + item_index);
    }
}

function onEnumCheckedClick(item_index, checked) {
    var attr = SelectedAttr;
    if ((attr) && (attr[IDX_ATTR_TYPE] == avtEnumeration)) {
        for (var i = 0; i < attr[IDX_ATTR_ITEMS].length; i++) {
            attr[IDX_ATTR_ITEMS][i][IDX_ATTR_ITEM_CHECKED] = ((i == item_index) && checked ? "1" : "0");
        }
        InitAttributesValues();
    }
    sendAjax("action=set_checked_value&chk=" + (checked ? "1" : "0") + "&attr=" + attr[IDX_ATTR_ID] + "&item_index=" + item_index);
}

var old_enum_value = "";
var old_enum_index = -1;
var new_cat_name = "";

function onEditAttributeValue(item_index, value) {
    old_enum_value = value;
    old_enum_index = item_index;
    var sCatNameEditor = "<input type='text' class='input' style='width:300px;' id='tbCategoryName' value='" + old_enum_value + "' onfocus='getEnumName(this.value);' onkeyup='getEnumName(this.value);' onblur='getEnumName(this.value);'>";
    dxDialog(sCatNameEditor, "renameEnumItem(new_cat_name.trim());", ";", "Edit category name");
    if ((theForm.tbCategoryName)) theForm.tbCategoryName.focus();
}        

function getEnumName(sName) {
    new_cat_name = sName.trim();
    dxDialogBtnDisable(true, (new_cat_name == ""));
}

function renameEnumItem(newName){        
    var attr = SelectedAttr;
    if ((attr) && (attr[IDX_ATTR_TYPE] == avtEnumeration) && old_enum_index < attr[IDX_ATTR_ITEMS].length) {
        attr[IDX_ATTR_ITEMS][old_enum_index][IDX_ATTR_ITEM_NAME] = newName;
        $("#tdCatName" + old_enum_index).html(newName);
    }
    InitAttributesValues();
    sendAjax("action=edit_enum_attr_value&attr=" + attr[IDX_ATTR_ID] + "&value=" + newName + "&item_index=" + old_enum_index);
}

function onAddAttributeValue(item_index) {
    var value = ($("#tbCatName").val() + "").trim();
    var attr = SelectedAttr;
    attr[IDX_ATTR_ITEMS].push(['guid', value, 0, 0]);
    InitAttributesValues();
    sendAjax("action=add_enum_attr_value&attr=" + attr[IDX_ATTR_ID] + "&value=" + value);
}

function onDeleteAttributeValue(item_index, item_name) {
    var cat_item_id = item_index;
    dxConfirm("Are you sure you want to delete '" + item_name + "'?", "DeleteAttributeValue(" + item_index + ");", ";");
}

function DeleteAttributeValue(item_index) {
    var attr = SelectedAttr;
    if ((attr) && (attr[IDX_ATTR_TYPE] == avtEnumeration || attr[IDX_ATTR_TYPE] == avtEnumerationMulti) && (item_index < attr[IDX_ATTR_ITEMS].length)) {
        attr[IDX_ATTR_ITEMS].splice(item_index, 1);
        InitAttributesValues();
        sendAjax("action=delete_enum_attr_value&attr=" + attr[IDX_ATTR_ID] + "&item_index=" + item_index);
    }
}

function getSelectedBoxNode(nodeID) {
    for (var i = 0; i < likelihood_data.length; i++) {
        if (likelihood_data[i][NODE_ID] == nodeID) return likelihood_data[i];
    }
    for (var i = 0; i < impact_data.length; i++) {
        if (impact_data[i][NODE_ID] == nodeID) return impact_data[i];
    }
    return null;
}

function refresh_bowtie() {
    drawBowTie(canvasID);
}

function InitAttributesValuesDlg() {
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
        title: "Edit Categories",
        position: { my: "center", at: "center", of: $("body"), within: $("body") },
        buttons: {
            Close: function() {
                if (checkUnsavedData($get("tbCatName"), "dlg_attributes_cat.dialog('close'); refresh_bowtie();")) { dlg_attributes_cat.dialog("close"); refresh_bowtie(); }
            }
    },
    open: function() {
        $("body").css("overflow", "hidden");
        on_hit_enter_cat = "onAddAttributeValue();";
        $get("tbCatName").focus();
    },
    close: function() {
        $("body").css("overflow", "auto");
        InitAttributesValues();
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
        dxConfirm("You have unsaved data. Do you really want to continue?", on_agree + ";", ";", "Confirmation");
        return false;
    }
    return true;
}
// End Edit Categories


function onContextMenuViewObjectivesClick(event_id, event_name) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    showContributedNodes(event_id, event_name);
}

//function getVisibleControlsForNode(node_id) {
//    var ret_val = 0;
//    var control_data_len = CanUserEditControls && (control_data) ? control_data.length : 0;
//    for (var k = 0; k < control_data_len; k++) {
//        var cType = control_data[k][IDX_CONTROL_TYPE];
//        var app_len = control_data[k][IDX_APPLICATIONS].length;
//        for (var p = 0; p < app_len; p++) {
//            var appl = control_data[k][IDX_APPLICATIONS][p];
//            var app_obj_id = control_data[k][IDX_APPLICATIONS][p][IDX_APPL_OBJ_GUID];
//            var app_ev_id = control_data[k][IDX_APPLICATIONS][p][IDX_APPL_EVENT_GUID];
//            if (node_id == app_obj_id) {
//                ret_val += 1;
//            }
//        }
//    }
//}

var clickHandler = function () {
    var i = this.getAttribute("i") * 1;
    var ctrl_type = this.getAttribute("ctrl_type") * 1;
    if (ctrl_type == ctCause) {
        if (likelihood_data.length > 0 && likelihood_data[i].length > NODE_IDX_COORDS) {
            likelihood_data[i][NODE_IDX_COORDS].expanded = true;
            if (CanUserSelectBoxes) selectedBoxGuidID = likelihood_data[i][NODE_ID];
            drawBowTie(canvasID);
        }
    } else {
        if (impact_data.length > 0 && impact_data[i].length > NODE_IDX_COORDS) {
            impact_data[i][NODE_IDX_COORDS].expanded = true;
            if (CanUserSelectBoxes) selectedBoxGuidID = impact_data[i][NODE_ID];
            drawBowTie(canvasID);
        }
    }
}

function drawDiagram(canvas, ctx, nodes_data, IsLeft, Offset) {       
    var XCoord = GetXCoord(canvas, IsLeft);

    ctx.strokeStyle = STROKE_COLOR;
    ctx.font = BOWTIE_FONT;
       
    var nodes_data_len = nodes_data.length;        
    var isNoSources = CanDrawNoSourcesNode && IsLeft && ((nodes_data_len == 0) || (selected_alt_data[ALT_HAS_CONTRIBUTIONS_L] == 0));
    
    if (isNoSources) {
        nodes_data_len = 1;
        ctx.font = NO_SOURCES_FONT;
    }
        
    var yLabelOffset = (UseControlsReductions ? 34 : 42);
    var yExpandedOffset = 0.0;

    for (var i = 0; i < nodes_data_len; i++) {    
        var n = nodes_data[i];
        yExpandedOffset = 0.0;

        // gradient fill for boxes
        if (typeof is_excanvas != 'undefined') { // IE
            if ((n) && n[NODE_IS_SOURCE_EVENT] > 0) {
                ctx.fillStyle = SOURCE_EVENT_COLOR;
            } else {
                ctx.fillStyle = (IsLeft ? (isNoSources ? NO_SOURCES_COLOR : LIKELIHOOD_COLOR) : IMPACT_COLOR);
            }
        } else { // Firefox or Chrome
            if ((n) && n[NODE_IS_SOURCE_EVENT] > 0) {
                IMPACT_COLOR = createSimpleGradient(ctx, SOURCE_EVENT_COLOR_0, SOURCE_EVENT_COLOR_1, XCoord, Offset, XCoord, Offset + DefaultRectangleHeight);
                ctx.fillStyle = SOURCE_EVENT_COLOR;
            } else {
                if (!IsLeft) {
                    IMPACT_COLOR = createSimpleGradient(ctx, IMPACT_COLOR_0, IMPACT_COLOR_1, XCoord, Offset, XCoord, Offset + DefaultRectangleHeight);
                    ctx.fillStyle = IMPACT_COLOR;
                } else {
                    if (isNoSources) {
                        NO_SOURCES_COLOR = createSimpleGradient(ctx, NO_SOURCES_COLOR_0, NO_SOURCES_COLOR_1, XCoord, Offset, XCoord, Offset + DefaultRectangleHeight);
                        ctx.fillStyle = NO_SOURCES_COLOR;
                    } else {
                        LIKELIHOOD_COLOR = createSimpleGradient(ctx, LIKELIHOOD_COLOR_0, LIKELIHOOD_COLOR_1, XCoord, Offset, XCoord, Offset + DefaultRectangleHeight);
                        ctx.fillStyle = LIKELIHOOD_COLOR;
                    }
                }
            }
        }
               
        //var tmpFill = ctx.fillStyle;

        //ctrl = New ctrlObjectiveRectangle(ViewModel, CStr(IIf(isEventWithNoSpecificCause, ViewModel.ParseResource(Infrastructure.My.Resources.Strings.lblNoSpecificCause), node.Title)), CInt(IIf(IsLeft, 0, 1)), DefaultRectangleHeight, DefaultRectangleWidth, CType(IIf(IsLeft, ViewModelBase.LikelihoodSolidColorBrush, ViewModelBase.ImpactSolidColorBrush), Brush), CanUserEditControls, CanUserSwitchToThreatView, CanShowAllPriorities, node.PriorityL, node.PriorityLV, node.PriorityV, CStr(IIf(CanShowUpperLevelNodes, node.NodePath, "")), isEventWithNoSpecificCause, NodeType = ECTypesRiskNodeType.ntCategory)
        //var is_expanded = isNoSources ? false : n[NODE_ID] == selectedBoxGuidID;
        //var is_expanded = isNoSources ? false : ShowAllNodesExpanded;
        var is_expanded = ShowAllNodesExpanded;

        var coords = { left: XCoord, top: Offset, width: DefaultRectangleWidth, height: DefaultRectangleHeight, expanded : is_expanded };
        // selected color and thickness
        if (is_expanded) {
            yExpandedOffset = 3 * ControlRowHeight;
            coords.height += yExpandedOffset;
        }
        if (!isNoSources) {
            if (n.length < NODE_IDX_COORDS + 1) { n.push(coords); } else { n[NODE_IDX_COORDS] = coords; }
        }

        ctx.fillRect(coords.left, coords.top, coords.width, coords.height);

        //if (is_expanded) {
        //    ctx.strokeStyle = SELECTED_STROKE_COLOR;
        //    ctx.lineWidth = SELECTED_STROKE_THICKNESS;
        //}

        ctx.strokeRect(coords.left, coords.top, coords.width, coords.height);

        // restore default color and thickness
        //if (is_expanded) {
        //    ctx.strokeStyle = STROKE_COLOR;
        //}
        ctx.lineWidth = STROKE_THICKNESS;


        //ctx.fillStyle = (!isNoSources ? FONT_COLOR : "#00f" );
        ctx.fillStyle = (!isNoSources ? (n[NODE_TYPE] != 1 ? FONT_COLOR : "#00f") : "#000" ); // category should be blue, no sources - black            
        var title = !isNoSources ? n[NODE_TITLE] : lblNoSource;
        if (BT_IsDiagramView) {
            rectText(ctx, title, BOWTIE_FONT, XCoord, Offset, DefaultRectangleWidth, DefaultRectangleHeight, getTextHeight(BOWTIE_FONT), 2);
        } else {
            ctx.fillText(ShortString(title, 32, false), XCoord + 5, Offset + 17);
        }

        var tt_div = addTooltipDiv(title, XCoord, Offset, DefaultRectangleWidth, coords.height / 2);
        tt_div.setAttribute("i", i);
        tt_div.setAttribute("ctrl_type", IsLeft ? ctCause : ctConsequence);
        tt_div.onclick = clickHandler;

        //rectText(ctx, ShortString((!isNoSources ? nodes_data[i][NODE_TITLE] : "< %=ResString("lblNoSpecificCause")%>"), 32, false), ctx.fillStyle, XCoord+3, Offset, DefaultRectangleWidth, coords.height/(CanUserEditControls ? 3 : 2), getTextHeight(BOWTIE_FONT), 2, false);
        if (!isNoSources && (nodes_data[i][NODE_TYPE] == 1)) ctx.fillStyle = FONT_COLOR;
        if (CanShowAllPriorities && !isNoSources) {
            if (IsLeft) {
                if (nodes_data[i][NODE_TYPE] != 1) {
                    ctx.fillText("[" + LegendA + ":" + nodes_data[i][NODE_L] + "]", XCoord + 5, Offset + yLabelOffset);
                };
                var lineV = "[" + LegendB + ":" + nodes_data[i][NODE_V] + "]";
                ctx.fillText(lineV, XCoord + DefaultRectangleWidth - canvasTextWidth(ctx, lineV) - 2, Offset + yLabelOffset);
            } else {
                ctx.fillText("[" + LegendC + ":" + nodes_data[i][NODE_V] + "]", XCoord + 5, Offset + yLabelOffset);
                var lineP = "[" + LegendD + ":" + nodes_data[i][NODE_L] + "]";
                ctx.fillText(lineP, XCoord + DefaultRectangleWidth - canvasTextWidth(ctx, lineP) - 2, Offset + yLabelOffset);
            }
            if (showProducts) {
                ctx.fillStyle = (IsLeft ? LIKELIHOOD_COLOR_1 : IMPACT_COLOR_1);
                if (IsLeft) {
                    ctx.fillStyle = "green";
                    ctx.fillText(LegendA + "*" + LegendB + ": " + nodes_data[i][NODE_LV], XCoord + DefaultRectangleWidth + 20, Offset + 26);
                } else {
                    ctx.fillStyle = "#5200cc";
                    //ctx.fillText(LegendC + "*" + LegendD + ": " + (!ShowDollarValue ? nodes_data[i][NODE_LV] : showCost(str2double(nodes_data[i][NODE_LV], true))), XCoord - DefaultRectangleWidth * 0.5 + 16, Offset + 26);
                    ctx.fillText(LegendC + "*" + LegendD + ": " + nodes_data[i][NODE_LV], XCoord - DefaultRectangleWidth * 0.5 + 16, Offset + 26);
                }
            }
        }
        if (CanShowUpperLevelNodes && !isNoSources) {
            ctx.fillStyle = "#000";                
            ctx.fillText(ShortString(nodes_data[i][NODE_PATH], 60, false), XCoord + 2, BT_IsDiagramView ? Offset + 3 : Offset - 3);
        }
        //ctx.fillStyle = tmpFill;

        var lineX1 = (IsLeft ? XCoord + DefaultRectangleWidth * 1.5 : XCoord - DefaultRectangleWidth * 0.5)
        var lineY1 = Offset + (coords.height / 2);
            
        ctx.beginPath();
        ctx.moveTo(CenterX, CenterY);
        ctx.lineTo(lineX1, lineY1);
        ctx.lineTo((IsLeft ? XCoord + DefaultRectangleWidth : XCoord), lineY1);                             
        ctx.stroke();
        ctx.closePath();

        /* draw the controls */
        if ((UseControlsReductions && (control_data)) || CanUserRemoveContribution) {
            var d = $get("divControls");
            if ((d)) {
                var ctrls_div = document.createElement("div");
                //ctrls_div.style.border = "1px solid #cccccc";
                //ctrls_div.style.backgroundColor = "#fff";
                ctrls_div.style.position = "absolute";
                ctrls_div.style.overflowY = "auto";
                ctrls_div.style.overflowX = "hidden";
                ctrls_div.style.textAlign = "left";
                ctrls_div.style.height = (is_expanded ? 3 * ControlRowHeight : 21) + "px";
                ctrls_div.style.padding = "0px 2px";
                ctrls_div.style.width = (IsLeft ? DefaultRectangleWidth - 26 : 40 + DefaultRectangleWidth / 2) + "px";
                ctrls_div.style.top = (is_expanded ? coords.top + 40 : Offset + coords.height - 18 - 4) + "px";
                ctrls_div.style.left = (IsLeft ? XCoord + 3 : XCoord - 130) + "px";
                ctrls_div.onclick = clickHandler;
                divMenuHandler(ctrls_div, i, (IsLeft ? ctCause : ctConsequence));
                var ctrls_menu_div = document.createElement("div");
                ctrls_menu_div.style.position = "absolute";
                ctrls_menu_div.style.height = coords.height - 22 + "px";
                ctrls_menu_div.style.width = DefaultRectangleWidth + "px";
                ctrls_menu_div.style.top = Offset + 22 + "px";
                ctrls_menu_div.style.left = (IsLeft ? XCoord + 2 : XCoord - DefaultRectangleWidth + 24) + "px";
                ctrls_menu_div.onclick = clickHandler;
                divMenuHandler(ctrls_menu_div, i, (IsLeft ? ctCause : ctConsequence));

                var to_event_div = null;
                var to_event_controls_len = 0;
                var controls_len = 0;
                if (IsLeft && UseControlsReductions) {
                    to_event_div = document.createElement("div");
                    //to_event_div.style.border = "1px solid #cccccc";
                    //to_event_div.style.backgroundColor = "#fff";
                    to_event_div.style.position = "absolute";
                    to_event_div.style.overflowY = "auto";
                    to_event_div.style.overflowX = "hidden";
                    to_event_div.style.textAlign = "left";
                    to_event_div.style.height = (is_expanded ? 3 * ControlRowHeight : 21) + "px";
                    to_event_div.style.padding = "0px 2px";
                    to_event_div.style.width = 40 + DefaultRectangleWidth / 2 + "px";
                    to_event_div.style.top = (is_expanded ? coords.top + 40 : Offset + coords.height * 0.5 + 4) + "px";
                    to_event_div.style.left = XCoord + DefaultRectangleWidth + 2 + "px";
                    divMenuHandler(to_event_div, i, ctVulnerability);
                    var to_event_menu_div = document.createElement("div");
                    to_event_menu_div.style.position = "absolute";
                    to_event_menu_div.style.height = 24 + "px";
                    to_event_menu_div.style.width = DefaultRectangleWidth / 2 + 22 + "px";
                    to_event_menu_div.style.top = Offset + coords.height * 0.5 + 2 + "px";
                    to_event_menu_div.style.left = 20 + XCoord + DefaultRectangleWidth + "px";
                    divMenuHandler(to_event_menu_div, i, ctVulnerability);
                }
                    
                var hint = "";
                var num_active_controls = 0;
                var num_total_controls  = 0;
                var t_data = [];

                var to_event_hint = "";
                var to_event_num_active_controls = 0;
                var to_event_num_total_controls  = 0;
                var to_event_data = [];

                var control_data_len = UseControlsReductions && (control_data) ? control_data.length : 0;
                for (var k = 0; k < control_data_len; k++) {
                    var cType = control_data[k][IDX_CONTROL_TYPE];
                    if ((IsLeft && ((cType == ctCause) || (cType == ctVulnerability))) || (!IsLeft && (cType == ctConsequence))){
                        var app_len = control_data[k][IDX_APPLICATIONS].length;
                        for (var p = 0; p < app_len; p++) {
                            var appl = control_data[k][IDX_APPLICATIONS][p];
                            var app_obj_id = control_data[k][IDX_APPLICATIONS][p][IDX_APPL_OBJ_GUID];
                            var app_ev_id  = control_data[k][IDX_APPLICATIONS][p][IDX_APPL_EVENT_GUID];
                            var node_id = (!isNoSources ? nodes_data[i][IDX_GUID] : SourceGoalGuid);
                            //if ((node_id == app_obj_id) && (((cType == ctCause) && !isNoSources) || (selected_alt_data[ALT_ID] == app_ev_id))){
                            if ((node_id == app_obj_id) && (cType == ctCause || selected_alt_data[ALT_ID] == app_ev_id)) {
                                var bkg_color_original = appl[IDX_APPL_IS_ACTIVE] == 0 || control_data[k][IDX_CONTROL_ENABLED] == 0 ? "#ddd" : "#fff";
                                var ctrl = "";
                                var ctrlIndex = "";
                                if (ShowTreatmentIndices && UseControlsReductions) ctrlIndex = "<div class='small' style='display: inline-block; font-size: 8pt; border:0.5px solid #ccc; vertical-align: top; height:15px; padding:1px 3px; margin:0px; background: transparent; " + (appl[IDX_APPL_IS_ACTIVE] == 0 ? "text-decoration:line-through;" : "") + "'>" + appl[IDX_APPL_CTRL_INDEX] + "</div>";
                                var ctrlInput = ctrlIndex + (UseControlsReductions ? "<input type='text' class='" + (control_data[k][IDX_CONTROL_ENABLED] == 0 ? "" : "input_cmenu") + "' readonly='true' ondblclick='activateControl(this, " + appl[IDX_APPL_IS_ACTIVE] + "," + '"' + control_data[k][IDX_GUID] + '"' + ");' style='border:0.5px solid #ccc; background: transparent; width:48px; padding:1px; margin:0px; height:18px;" + (appl[IDX_APPL_IS_ACTIVE] == 0 ? "text-decoration:line-through;" : "") + "' value='" + appl[IDX_APPL_VALUE] + "' ctrl_id='" + '"' + control_data[k][IDX_GUID] + '"' + "' node_id='" + node_id + "' app_id='" + '"' + appl[IDX_APPL_GUID] + '"' + "' app_active='" + appl[IDX_APPL_IS_ACTIVE] + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' />" : "");
                                var ctrlName = "<span type='text' class='text-overflow' style='color:#00677e;' title='" + control_data[k][IDX_NAME] + "'>[" + control_data[k][IDX_NAME] + "]</span>";
                                if (!ShowTreatmentNames) {
                                    ctrl = ctrlInput;
                                } else {
                                    ctrl = ctrlName;
                                }
                                if (is_expanded) {
                                    ctrl = "<nobr>" + ctrlInput + (ShowTreatmentNames ? ctrlName : "") + "</nobr><br/>";
                                }
                                ctrl = "<div style='display: inline-block; margin: 0px 0px 2px 0px; background-color:" + bkg_color_original + ";' data-bkg-color='" + bkg_color_original + "' onmouseenter='if (highlightedControlIndex != " + appl[IDX_APPL_CTRL_INDEX] + ") {highlightedControlIndex=" + appl[IDX_APPL_CTRL_INDEX] + "; highlightControl(true, " + appl[IDX_APPL_IS_ACTIVE] + ");}' onmouseleave='if (highlightedControlIndex != -1) {highlightedControlIndex=-1; highlightControl(false, " + appl[IDX_APPL_IS_ACTIVE] + ");}' class='control_div control_div_id_" + appl[IDX_APPL_CTRL_INDEX] + "' title='" + control_data[k][IDX_NAME] + " : " + appl[IDX_APPL_VALUE] + "'>" + ctrl + "</div>";
                                if (cType != ctVulnerability) {
                                    ctrls_div.innerHTML += ctrl + (is_expanded ? "" : "&nbsp;");
                                    hint += (hint == "" ? "" : "\n") + control_data[k][IDX_NAME] + ": " + appl[IDX_APPL_VALUE] + (appl[IDX_APPL_IS_ACTIVE] == 0 ? "" : " (" + lblControlActive + ")");
                                    t_data.push([control_data[k][IDX_NAME],appl[IDX_APPL_VALUE],(appl[IDX_APPL_IS_ACTIVE] == 0 ? "" : "YES")]);
                                    num_active_controls += (appl[IDX_APPL_IS_ACTIVE] == 0 ? 0 : 1);
                                    num_total_controls  += 1;
                                    controls_len += 1;
                                } else {
                                    to_event_div.innerHTML += ctrl + "&nbsp;";
                                    to_event_hint += (to_event_hint == "" ? "" : "\n") + control_data[k][IDX_NAME] + ": " + appl[IDX_APPL_VALUE] + (appl[IDX_APPL_IS_ACTIVE] == 0 ? "" : " (" + lblControlActive + ")");
                                    to_event_data.push([control_data[k][IDX_NAME], appl[IDX_APPL_VALUE], (appl[IDX_APPL_IS_ACTIVE] == 0 ? "" : "YES")]);
                                    to_event_num_active_controls += (appl[IDX_APPL_IS_ACTIVE] == 0 ? 0 : 1);
                                    to_event_num_total_controls += 1;
                                    to_event_controls_len += 1;
                                }
                            }
                        }                                
                    } 
                }

                if (!IsLeft || !isNoSources) arrangeDivs(d, controls_len, i, ctrls_menu_div, ctrls_div, (IsLeft ? ctCause : ctConsequence), num_total_controls, num_active_controls, hint, t_data, is_expanded, isNoSources);
                if (IsLeft && UseControlsReductions) arrangeDivs(d, to_event_controls_len, i, to_event_menu_div, to_event_div, ctVulnerability, to_event_num_total_controls, to_event_num_active_controls, to_event_hint, to_event_data, is_expanded, isNoSources);

                if (CanUserRemoveContribution && !isNoSources) {
                    var div_remove_contribution = document.createElement("div");
                    div_remove_contribution.style.position = "absolute";
                    //div_remove_contribution.style.height = coords.height;
                    //div_remove_contribution.style.width = DefaultRectangleWidth / 2;
                    //div_remove_contribution.style.top = Offset;
                    //div_remove_contribution.style.left = (IsLeft ? XCoord + DefaultRectangleWidth : XCoord - DefaultRectangleWidth / 2);
                    div_remove_contribution.style.height = coords.height / 2 + 6 + "px";
                    div_remove_contribution.style.width = 20 + "px";
                    div_remove_contribution.style.top = Offset + "px";
                    //div_remove_contribution.style.background = "#ddd";
                    div_remove_contribution.style.left = (IsLeft ? XCoord + DefaultRectangleWidth : XCoord - 20) + "px";
                    d.appendChild(div_remove_contribution);
                    divMenuHandler(div_remove_contribution, "c_" + i, IsLeft ? ctCause : ctConsequence);

                    // adding image with X
                    var img_details = document.createElement("img");
                    img_details.src = ImagesPath + "delete_tiny.gif";
                    img_details.style.cursor = "hand";
                    img_details.style.visibility = "hidden";
                    img_details.style.position = "absolute";
                    img_details.style.top = coords.height / 2 - 5 + "px";
                    img_details.style.left = (IsLeft ? 5 : 5) + "px";
                    img_details.border = 0;
                    img_details.setAttribute("id", "mnu_node_c_" + i + "_" + (IsLeft ? ctCause : ctConsequence));
                    img_details.setAttribute("obj_index", i);
                    img_details.setAttribute("ctrl_type", IsLeft ? ctCause : ctConsequence);
                    img_details.title = "Remove Contribution";
                    img_details.onclick = onRemoveContributionIconClick;
                    div_remove_contribution.appendChild(img_details);
                }

            }
        }
        /* --controls */

        if (nodes_data.length > 0) addInfodocIcon(nodes_data[i][NODE_ID], nodes_data[i][NODE_HAS_INFODOC] == 1, XCoord + DefaultRectangleWidth - 17, Offset + 2, 'OpenRichEditor("?type=1&field=infodoc&guid=' + nodes_data[i][NODE_ID] + '&callback=' + nodes_data[i][NODE_ID] + '");');

        Offset += coords.height + (CanShowUpperLevelNodes ? DefaultRectangleMarginWithNodePath : DefaultRectangleMargin);            
    }

    $('input.input_cmenu').unbind("contextmenu").bind('contextmenu', function(e){
        e.preventDefault();
        activateClick = this.ondblclick;
        var app_active = this.getAttribute("app_active")*1;            
        right_click_ctrl_id = this.getAttribute("ctrl_id");
        right_click_node_id = this.getAttribute("node_id");
        right_click_appl_id = this.getAttribute("app_id");
        showCMenu(e);
        $("#mnuControlActivateContent").html((app_active == 1 ? lblControlDeactivate : lblControlActivate));
        setTimeout("fadeCMenuHandler();", 500);
    }).attr('autocomplete','off').attr('autocorrect','off').attr('autocapitalize','off');
    $('input.input_cmenu').unbind("click").bind('click', function(e){            
        activateClick = this.ondblclick;
        var app_active = this.getAttribute("app_active")*1;            
        right_click_ctrl_id = this.getAttribute("ctrl_id");
        right_click_node_id = this.getAttribute("node_id");
        right_click_appl_id = this.getAttribute("app_id");
        var offset = $(this).offset();
        if ((e.pageX - offset.left >= 32) && (e.pageY - offset.top > 1)) {                
            showCMenu(e);
            setTimeout("fadeCMenuHandler();", 500);                    
        }
        $("#mnuControlActivateContent").html((app_active == 1 ? lblControlDeactivate : lblControlActivate));
    }).attr('autocomplete','off').attr('autocorrect','off').attr('autocapitalize','off');
}

function OpenRichEditor(cmd) {    
    CreatePopup(_PGURL_RICHEDITOR + cmd + "&" + _TEMP_THEME, 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);
}

function onRichEditorRefresh(empty, infodoc, callback_param) {
    window.focus();
    if (infodoc) {        
        var node;
        var guid = callback_param[0] + "";
        if (selected_alt_data[IDX_GUID] == guid) {
            node = selected_alt_data;
        } else {
            var likelihood_data_len = likelihood_data.length;
            for (var i = 0; i < likelihood_data_len; i++) {
                if (likelihood_data[i][NODE_ID] == guid) {
                    node = likelihood_data[i];
                    i = likelihood_data_len;
                }
            }
            var impact_data_len = impact_data.length;
            for (var i = 0; i < impact_data_len; i++) {
                if (impact_data[i][NODE_ID] == guid) {
                    node = impact_data[i];
                    i = impact_data_len;
                }
            }
        }
        if ((node)) {
            node[NODE_HAS_INFODOC] = (empty != "1" ? 1 : 0);
            $("img[guid='" + guid + "']").attr("src", ImagesPath + (empty != "1" ? "info15.png" : "info15_dis.png"));
        }        
    }
}

function showCMenu(e) { //activate/deactivate control menu        
    is_context_menu_open = false;
    $("div.context-menu").hide().remove();

    var sMenu = "<div class='text context-menu' id='divCMenu' style='display:none'>";
    sMenu += "<a href='' id='mnuControlActivate' class='context-menu-item' onclick='activateClick(); return false;'><div style='text-align:left;' id='mnuControlActivateContent'>" + lblControlActivate + "</div></a>";
    sMenu += "<a href='' id='mnuControlEdit' class='context-menu-item' onclick='editControlClick(); return false;'><div style='text-align:left;' id='mnuControlEditContent'>" + lblControlEdit + "</div></a>";
    if (typeof (onEditControlInfodoc) == "function") sMenu += "<a href='' id='mnuControlInfodoc' class='context-menu-item' onclick='editControlInfodoc(); return false;'><div style='text-align:left;' id='mnuControlEditInfodco'>" + lblControlInfodoc + "</div></a>";
    sMenu += "<a href='' id='mnuControlEval' class='context-menu-item' onclick='evalControlClick(); return false;'><div style='text-align:left;' id='mnuControlEval'>" + lblControlEval + "</div></a>";
    sMenu += "<a href='' id='mnuControlDelete' class='context-menu-item' onclick='deleteControlClick(); return false;'><div style='text-align:left;' id='mnuControlDeleteContent'>" + lblControlDelete + "</div></a>";
    sMenu += "</div>";

    $(sMenu).appendTo("body").css({top: (e.pageY+1) + "px", left: (e.pageX+1) + "px"}).fadeIn(200);
    setTimeout('canCloseMenu()', 200);
}

function fadeCMenuHandler() {                
    $(document).bind("click", function(event) { $("div.context-menu").hide(200); is_context_menu_open = false; $(document).unbind("click"); });
}

function divMenuHandler(d, i, ControlType) {
    d.setAttribute("i", i);
    d.setAttribute("ctrl_type", ControlType);
    d.setAttribute("mnu_id", "mnu_node_"+i+"_"+ControlType);        
    d.onmouseover = function() { $get(this.getAttribute("mnu_id")).style.visibility = "visible"; };
    d.onmouseout  = function() { $get(this.getAttribute("mnu_id")).style.visibility = "hidden";  };
}

function arrangeDivs(d, controls_len, i, menu_div, ctrl_div, ControlType, num_total_controls, num_active_controls, hint, t_data, is_expanded, isNoSources) {
    // add div for the menu 
    d.appendChild(menu_div);
    if (controls_len > 0) { 
        // add div with controls
        d.appendChild(ctrl_div);
    }
    // adding image with total controls tooltip
    var img_details = document.createElement("img");
    img_details.src = ImagesPath + "menu_dots_14.png";
    //img_details.width = 12;
    //img_details.height = 12;
    img_details.style.cursor = "pointer";
    img_details.style.visibility = "hidden";
    img_details.style.position = "absolute";
    img_details.style.zIndex = "999";
    img_details.style.top = (ControlType == ctVulnerability ? 5 : (is_expanded ? 40 - 22 : DefaultRectangleHeight - 22 - 20)) + "px";
    img_details.style.left = (ControlType == ctVulnerability ? (controls_len > 0 ? DefaultRectangleWidth / 2 + 3 : 20) : DefaultRectangleWidth - 19) + "px";
    img_details.border = 0;
    img_details.setAttribute("id", "mnu_node_" + i + "_" + ControlType);
    img_details.setAttribute("obj_index", i);
    img_details.setAttribute("ctrl_type", ControlType);
    img_details.setAttribute("controls_len", controls_len);
    img_details.title = hint == "" ? "" : num_active_controls +" out of " + num_total_controls + " " + lblControls + " are active\n\n" + hint;
    img_details.onclick = onShowMenuClick; //function() { showMenu(event, this.getAttribute("id"), this.getAttribute("ctrl_type"), this.getAttribute("controls_len")) };
    $(img_details).unbind("contextmenu").bind('contextmenu', function (e) { onShowMenuClick(e, isNoSources); });
    menu_div.appendChild(img_details);
    tbl_data.push(["mnu_node_" + i + "_" + ControlType, t_data]);
}

function GetXCoord(canvas, isLeft) {
    var XCoord = (isLeft ? 10 : canvas.width - DefaultRectangleWidth - 30);
    if (XCoord < 0) XCoord = 10;
    if (XCoord > canvas.width - DefaultRectangleWidth - 30) XCoord = canvas.width - DefaultRectangleWidth - 30;
    return XCoord
}

function activateControl(tb, cur_is_active, ctrl_id) {
    sendCommand("activate_control=" + (cur_is_active == 0 ? 1 : 0) + "&ctrl_id=" + ctrl_id);
}

var ctrl_id  = "";
var ctrl_name  = "";
//var ctrl_descr = "";
var ctrl_cost  = "";
var ctrl_cat   = "";

function resetVars() {
    ctrl_id  = "";
    ctrl_name  = "";
    //ctrl_descr = "";
    ctrl_cost  = "";
    ctrl_cat   = "";
}

function getCtrlName(sName) {
    ctrl_name = sName;        
    dxDialogBtnDisable(true, (!validFloat(ctrl_cost)) || (ctrl_name.trim() == ""));
}

//function getCtrlDescr(sDescr) {
//    ctrl_descr = sDescr;
//}

function getCtrlCost(sCost) {
    if (validFloat(sCost)) { ctrl_cost = sCost } else {ctrl_cost = "0" };
    dxDialogBtnDisable(true, (!validFloat(ctrl_cost)) || (ctrl_name.trim() == ""));
}

function getCtrlCategories() {        
    ctrl_cat = "";
    $('input:checkbox.cb_cat:checked').each(function() { ctrl_cat += (ctrl_cat == ""?"" : PARAM_DELIMITER) + this.value });
}

function AddCategory() {
    var tb = $get("tbNewCategory");
    if ((tb) && tb.value.trim() != "" && getCategoryByName(tb.value.trim()) == null) {
        new_cat_name = tb.value.trim();
        tb.value = "";
        sendAjax("action=add_category&name=" + encodeURIComponent(new_cat_name));
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
        
    opt.innerHTML = "<table cellpadding='0' cellspacing='0' class='text' style='width:100%;'><tr><td align='left'><label><input type='checkbox' id='cb_cat_" + i + "' class='cb_cat' id='cb_cat_" + i + "' " + chk + " value='" + cat_id + "' onclick='getCtrlCategories();' /><span id='lbl_cat_" + i + "'>" + name + "</span></label></td><td align='right'><img align='right' style='visibility:hidden;vertical-align:top;cursor:pointer;height:16px;' id='mnu_cat_img_" + i + "' src='" + ImagesPath + "menu_dots.png' alt='' onclick='showMenuCat(event, " + '"' + cat_id + '"' + ", " + '"' + decodeURIComponent(cat_name) + '"' + ");' /></td></tr></table>";
    opt.onmouseover = function() { $get("mnu_cat_img_"+this.getAttribute("i")).style.visibility = "visible"; };
    opt.onmouseout  = function() { $get("mnu_cat_img_"+this.getAttribute("i")).style.visibility = "hidden"; };
    lb.appendChild(opt);
    lb.disabled = false;
}

var cancelled = false;

function initEditorForm(_title, name, descr, cost, action, categories, ctrl_type, objGUID) {
    ctrl_name  = name;
    //ctrl_descr = descr;
    ctrl_cost  = cost;
    ctrl_cat   = categories;

    $get("tbControlName").value  = name;
    //$get("tbControlDescr").value = descr;
    $get("tbControlCost").value  = cost;

    var lb = $get("lbCategories");
        
    while (lb.firstChild) {
        lb.removeChild(lb.firstChild);
    }
                                
    var cat_data_len = cat_data.length;
    for (var i=0; i<cat_data_len; i++) {
        createCategoryElement(lb, i, cat_data[i][IDX_GUID], cat_data[i][IDX_NAME], categories);
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
            $get("tbControlName").focus();
        },
        close: function() {
            $("body").css("overflow", "auto");                
            if ((!cancelled) && (ctrl_name.trim() != "")) {
                //sendCommand('action=' + action + '&control_id=' + ctrl_id + '&name=' + encodeURIComponent(ctrl_name) + '&descr=' + encodeURIComponent(ctrl_descr) + '&cost=' + ctrl_cost + '&cat=' + ctrl_cat + '&ctype=' + ctrl_type + '&obj_id=' + objGUID);
                sendCommand('action=' + action + '&control_id=' + ctrl_id + '&name=' + encodeURIComponent(ctrl_name) + '&cost=' + ctrl_cost + '&cat=' + ctrl_cat + '&ctype=' + ctrl_type + '&obj_id=' + objGUID);
            }
        }
    });
}

function initSelectControlsForm(_title, action, ctrl_type, objGUID) {
    cancelled = false;

    var labels = "";

    // generate list of applications
    var control_data_len = control_data.length;
    for (var k = 0; k < control_data_len; k++) {
        var cType = control_data[k][IDX_CONTROL_TYPE];
        if (cType*1 == ctrl_type*1) {
            var app_len = control_data[k][IDX_APPLICATIONS].length;
            var checked = false;
            for (var p = 0; p < app_len; p++) {
                var appl = control_data[k][IDX_APPLICATIONS][p];
                var app_obj_id = control_data[k][IDX_APPLICATIONS][p][IDX_APPL_OBJ_GUID];
                var app_ev_id  = control_data[k][IDX_APPLICATIONS][p][IDX_APPL_EVENT_GUID];
                    
                if ((objGUID == app_obj_id) && (ctrl_type*1 == ctCause || (selected_alt_data[ALT_ID] == app_ev_id))) {
                    checked = true;
                }
            }
            labels += "<label><input type='checkbox' class='select_control_cb' value='" + control_data[k][IDX_GUID] + "' " + (checked ? " checked='checked' " : " ") + " onclick='dxDialogBtnDisable(true, false);' onchange='dxDialogBtnDisable(true, false);' />" + control_data[k][IDX_NAME] + "</label><br/>";
        }
    }    

    $("#divSelectControls").html(labels);

    $("#selectControlsForm").dxPopup({
        width: "80%",
        height: "80%",
        visible: true,
        title: _title,
        onHiding: function() {
            if (!cancelled) {
                var ctrl_ids = "";
                var cb_arr = $("input:checkbox.select_control_cb");
                $.each(cb_arr, function(index, val) { var cid = val.value+""; if (val.checked) { ctrl_ids += (ctrl_ids == "" ? "" : ",") + cid; } });
                sendCommand('action='+action+'&control_ids='+ctrl_ids+'&ctype='+ctrl_type+'&obj_id='+objGUID);
            }
        }
    });
}

//function filterControlsCustomSelect(chk) {
//    $("input:checkbox.select_control_cb").prop('checked', chk*1 == 1);
//    dxDialogBtnDisable(true, false);
//}

function selectControlsDialog(ControlType, objGUID) {
    initSelectControlsForm("Select " + lblControls, "select_controls_bt", ControlType, objGUID);
    dxDialogBtnDisable(true, true);
}

function addControlClick(ControlType, objGUID) {
    resetVars();
    initEditorForm("Add a " + lblControl, "","","0.00", "addcontrol", "", ControlType, objGUID);
    dlg_treatment_editor.dialog("open");
    dxDialogBtnDisable(true, true);
    if ((theForm.tbControlName)) theForm.tbControlName.focus();
}

function editControlInfodoc() {
    resetVars();
    $("div.context-menu").hide(200); is_context_menu_open = false;
    var control_data_len = control_data.length;
    var control_id = right_click_ctrl_id.substring(1); // remove first " (quotation mark)
    control_id = control_id.substring(0, control_id.length - 1); // remove last " (quotation mark)
    ctrl_id = control_id;
    onEditControlInfodoc(ctrl_id);
}
    
function editControlClick() {
    resetVars();
    $("div.context-menu").hide(200); is_context_menu_open = false;
    var control_data_len = control_data.length;        
    var control_id = right_click_ctrl_id.substring(1); // remove first " (quotation mark)
    control_id = control_id.substring(0,control_id.length-1); // remove last " (quotation mark)
    ctrl_id = control_id;
    for (var i=0; i<control_data_len; i++) {
        if ((control_data[i]) && (control_data[i][IDX_GUID] == control_id)) {                
            var cost = (control_data[i][IDX_COST] == "", "0.00", control_data[i][IDX_COST]);                
            initEditorForm(lblControl + " properties", control_data[i][IDX_NAME], control_data[i][IDX_DESCRIPTION], cost, "editcontrol", control_data[i][IDX_CATEGORIES]);
            dlg_treatment_editor.dialog("open");
            dxDialogBtnDisable(true, true);
            if ((theForm.tbControlName)) theForm.tbControlName.focus();
            i = control_data_len;
        }
    }
}

function evalControlClick() {
    resetVars();
    $("div.context-menu").hide(200); is_context_menu_open = false;
    var control_data_len = control_data.length;
    var control_id = right_click_ctrl_id.substring(1); // remove first " (quotation mark)
    control_id = control_id.substring(0, control_id.length - 1); // remove last " (quotation mark)
    ctrl_id = control_id;
    evaluateControlEffectiveness(control_id, selected_alt_data[ALT_ID], right_click_node_id, right_click_appl_id);
}

function deleteControlClick() {
    resetVars();
    var control_id = right_click_ctrl_id;
    dxConfirm("Are you sure you want to delete this " + lblControl + "?", "onContextMenuRemoveClick(" + control_id + ");", ";");
}

function onContextMenuRemoveClick(control_id) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    sendCommand("action=deletecontrol&control_id=" + control_id);
}

/* Region "context menu for categories" */    
function getCategoryByID(cat_id) {
    var cat_data_len = cat_data.length;
    for (var i=0; i<cat_data_len; i++) {
        if (cat_data[i][IDX_GUID] == cat_id) return cat_data[i];
    }
}

function getCategoryByName(cat_name) {
    cat_name = cat_name.trim();
    var cat_data_len = cat_data.length;
    for (var i=0; i<cat_data_len; i++) {
        if (cat_data[i][IDX_NAME] == cat_name) return cat_data[i];
    }
    return null;
}

function showMenuCat(event, id, name) {        
    is_context_menu_open = false;
    var cat = getCategoryByID(id);
    if ((cat)) {               
        $("div.context-menu").hide().remove();
        var sMenu = "<div class='context-menu'>";
        sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuRenameCategoryClick(" +'"' + id +'"' + "); return false;'><div><nobr><img align='left' style='vertical-align:middle;' src='" + ImagesPath + "edit_small.gif' alt='' />&nbsp;Edit&nbsp;&quot;" + name + "&quot;</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='confrimDeleteCategory(" +'"' + id +'"' + "); return false;'><div><nobr>&nbsp;<img align='left' style='vertical-align:middle;' src='" + ImagesPath + "delete_tiny.gif' alt='' />&nbsp;&nbsp;Remove&nbsp;&quot;" + name + "&quot;</nobr></div></a>";
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
        new_cat_name = cat[IDX_NAME];
        var sCatNameEditor = "<input type='text' class='input' style='width:300px;' id='tbCategoryName' value='" + htmlEscape(new_cat_name) + "' onfocus='getCatName(this.value);' onkeyup='getCatName(this.value);' onblur='getCatName(this.value);'>";
        dxDialog(sCatNameEditor, "if (new_cat_name.trim().length>0) {renameCategory(new_cat_name.trim());} else {onRenameEmptyName();}", ";", "Edit category name");
        if ((theForm.tbCategoryName)) theForm.tbCategoryName.focus();
    }        
}

function getCatName(sName) {
    new_cat_name = sName;
    dxDialogBtnDisable(true, (sName.trim() == ""));
}

function renameCategory(newName){        
    var cat = getCategoryByID(rename_cat_id);
    if ((cat) && (cat[IDX_NAME] != newName) && (newName.length > 0)) {
        sendAjax("action=rename_category&cat_id=" + rename_cat_id + "&cat_name=" + encodeURIComponent(newName));
        new_cat_name = ""
        cat[IDX_NAME] = newName;
            
        var lb = $get("lbCategories");        
        var lb_children_count = lb.childNodes.length;
        for (var j=0; j<lb_children_count; j++) {
            if (lb.childNodes[j].getAttribute("id") == rename_cat_id) {
                var idx = lb.childNodes[j].getAttribute("i");
                var lbl = $get('lbl_cat_'+idx);
                if ((lbl)) { lbl.innerHTML = newName };
                j = lb_children_count;
            }
        }
    }
}

function onRenameEmptyName() {
    dxDialog("Error: Empty category name!", "onContextMenuRenameCategoryClick(rename_cat_id);", "undefined", "Error");
}

function onContextMenuRemoveCategoryClick(id) {
    $("div.context-menu").hide(200); is_context_menu_open = false;

    var cat_data_len = cat_data.length;
    for (var i=0; i<cat_data_len; i++) {
        if ((cat_data[i]) && (cat_data[i][IDX_GUID] == id)) {
            cat_data.splice(i, 1);
                
            var lb = $get("lbCategories");        
            var lb_children_count = lb.childNodes.length;
            for (var j=0; j<lb_children_count; j++) {
                if (lb.childNodes[j].getAttribute("id") == id) {
                    lb.removeChild(lb.childNodes[j]);
                    j = lb_children_count;
                }
            }

            sendAjax("action=delete_category&cat_id=" + id);
            i = cat_data_len;
        }
    }
}

function confrimDeleteCategory(id) {
    $("div.context-menu").hide(); is_context_menu_open = false;
    dxConfirm("Are you sure you want to delete this category?", "onContextMenuRemoveCategoryClick('" + id + "');", ";");
}
/* End Region "context menu for categories" */


/* Bow-Tie context menu */
function onShowMenuClick(event, isNoSources) {
    event = event || window.event;
    if ((this.getAttribute)) showMenu(event, this.getAttribute("id"), this.getAttribute("ctrl_type"), this.getAttribute("controls_len"), this.getAttribute("obj_index"), isNoSources);
    if ((event.target.getAttribute)) { event.preventDefault(); showMenu(event, event.target.getAttribute("id"), event.target.getAttribute("ctrl_type"), event.target.getAttribute("controls_len"), event.target.getAttribute("obj_index"), isNoSources) };
}

var is_context_menu_open = false;

function getObjGUIDFromIndex(obj_index, hid) {
    return (hid*1 == 2 ? (impact_data.length > obj_index ? impact_data[obj_index][IDX_GUID] : impact_goal_guid) : ( likelihood_data.length > obj_index ? likelihood_data[obj_index][IDX_GUID] : likelihood_goal_guid));
}

function showMenu(event, unique_id, ControlType, controls_len, obj_index, isNoSources) {
    is_context_menu_open = false;
    $("div.context-menu").hide().remove();

    var objGUID = (ControlType == ctConsequence ? (impact_data.length > obj_index ? impact_data[obj_index][IDX_GUID] : impact_goal_guid) : ( likelihood_data.length > obj_index ? likelihood_data[obj_index][IDX_GUID] : likelihood_goal_guid));
               
    var sMenu = "<div class='context-menu'>";
    if (UseControlsReductions) {
        if (controls_len > 0) {
            sMenu += "<nobr><a href='' class='context-menu-item' onclick='onContextMenuApplicationsClick(" + '"' + unique_id + '"' + "); return false;'><div style='padding:2px; height:20px;'><img align='left' width='16' height='16' style='margin-top:2px;' src='" + ImagesPath + "search-16.png' alt='' /><span style='display:inline-block; margin-top:2px;'>&nbsp;&nbsp;View&nbsp;" + lblControls + "&nbsp;&nbsp;&nbsp;</span></div></a></nobr>";
            sMenu += "<nobr><a href='' class='context-menu-item' onclick='onContextMenuExpandedApplicationViewClick(" + '"' + objGUID + '",' + ControlType + "); return false;'><div style='padding:2px; height:20px;'><img align='left' width='16' height='16' style='margin-top:2px;' src='" + ImagesPath + "expanded_treatment.png' alt='' /><span style='display:inline-block; margin-top:2px;'>&nbsp;&nbsp;Expanded&nbsp;View&nbsp;&nbsp;&nbsp;</span></div></a></nobr>";
        }
        sMenu += "<nobr><a href='' class='context-menu-item' onclick='onContextMenuAddControlClick(" + ControlType + "," + '"' + objGUID + '"' + "); return false;'><div style='padding:2px; height:20px;'><img align='left' width='16' height='16' style='margin-top:2px;' src='" + ImagesPath + "add-16.png' alt='' /><span style='display:inline-block; margin-top:2px;'>&nbsp;&nbsp;New&nbsp;" + lblControl + "&nbsp;&nbsp;&nbsp;</span></div></a></nobr>";
        sMenu += "<nobr><a href='' class='context-menu-item' onclick='onContextMenuEditControlAssignmentClick(" + ControlType + "," + '"' + objGUID + '"' + "); return false;'><div style='padding:2px; height:20px;'><img align='left' width='16' height='16' style='margin-top:2px;' src='" + ImagesPath + "apply-16.png' alt='' /><span style='display:inline-block; margin-top:2px;'>&nbsp;&nbsp;Select&nbsp;" + lblControls + "&nbsp;&nbsp;&nbsp;</span></div></a></nobr>";
        sMenu += "<nobr><a href='' class='context-menu-item' onclick='onContextMenuEditControlsClick(" + ControlType + "); return false;'><div style='padding:2px; height:20px;'><img align='left' width='16' height='16' style='margin-top:2px;' src='" + ImagesPath + "edit_small_go.png' alt='' /><span style='display:inline-block; margin-top:2px;'>&nbsp;&nbsp;Edit&nbsp;" + lblControls + "&nbsp;&nbsp;&nbsp;</span></div></a></nobr>";
    }

    if (CanUserRemoveContribution && !isNoSources) {
        sMenu += "<nobr><a href='' class='context-menu-item' onclick='onRemoveContributionClick(" + '"' + objGUID + '"' + "," + (ControlType == ctConsequence ? 2 : 0) + "); return false;'><div style='padding:2px; height:20px;'><img align='left' width='16' height='16' style='margin-top:2px;' src='" + ImagesPath + "remove-16.png' alt='' /><span style='display:inline-block; margin-top:2px;'>&nbsp;&nbsp;Remove&nbsp;Contribution&nbsp;&nbsp;&nbsp;</span></div></a></nobr>";
    }
    sMenu += "</div>";                
    var x = event.clientX;
    var y = event.clientY;
    var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});
    var w = $("div.context-menu").width();
    var pw = $("#BowTieViewChart").width(); 
    if ((pw) && (x+w+10>pw) && (x-w-10>0)) $("div.context-menu").css({left: (x-w-10) + "px"});

    $("div.context-menu").fadeIn(500);
    setTimeout('canCloseMenu()', 200);
}

function canCloseMenu() {
    is_context_menu_open = true;
}

function onContextMenuApplicationsClick(unique_id) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    if ((tbl_data)) {
        var tbl_data_len = tbl_data.length;
        for (var i=0; i<tbl_data_len; i++) {
            if (tbl_data[i][0] == unique_id) {
                ShowApplicationsDialog(tbl_data[i][1]);
                i = tbl_data_len;
            }
        }                   
    }
}

function onContextMenuExpandedApplicationViewClick(objGUID, ControlType) {
    $("div.context-menu").hide(200); is_context_menu_open = false;        
    sendAjax("action=" + ACTION_GET_TREATMENTS + "&node=" + objGUID + "&ctrl_type=" + ControlType);
}

function drawExpadedTreatments(data) {
    if ((data)) {
        var arr = eval(data);
        var node_name = arr[0];
        var node_type = arr[1]*1;
        var ctrls = arr[2];
        var event_name = arr[3];

        /* Init Expanded View canvas */
        var canvas = document.getElementById("canvasExpandedTreatmentView");
        if ((typeof G_vmlCanvasManager != 'undefined') && (G_vmlCanvasManager)) G_vmlCanvasManager.initElement(canvas);
        if (!canvas.getContext) return false;
        ctx = canvas.getContext("2d");        

        // fill the canvas with white color - works best in IE and FF
        ctx.fillStyle = "#eeeeee";
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // draw the graph
        var x = 20;
        var y = 20;
        var boxOffset = 10;
            
        // objective and event
        var boxWidth  = 150;            
        var boxHeight = 120;

        // treatment
        var tboxWidth  = 120;
        var tboxHeight = 40;

        var TREATMENT_COLOR = "#ffc30f";

        canvas.width = ((ctrls) ? ctrls.length * (boxOffset + tboxWidth) : 0) + ((event_name) && event_name != "" ? 2 : 1) * (x + boxWidth) + boxOffset;
        canvas.height = boxHeight + y * 2;

        /* Draw lines */
        var one_thickness = 16;
        var cur_thickness = one_thickness;
        var ix = x + boxWidth;
        var iy = y + boxHeight / 3;
        ctx.strokeStyle = "#999";
        ctx.fillStyle = "#999";
        ctx.strokeRect(ix, iy, ctrls.length * (tboxWidth + boxOffset) + boxOffset, one_thickness + 2);
        iy += 1;
        for (var i = 0; i < ctrls.length; i++) {                
                
            var end_x;
                
            // first (100%) bar
            if (i == 0) {
                var end_x = (tboxWidth / 2) + boxOffset;
                ctx.fillRect(ix, iy, end_x, one_thickness);
                ix += end_x;
            }

            // connecting line
            ctx.beginPath();
            ctx.moveTo(ix, iy + one_thickness + 1);
            ctx.lineTo(ix, y + boxHeight - tboxHeight);
            ctx.stroke();
            ctx.closePath();
                

            // treatment effectiveness bar
            cur_thickness = cur_thickness - cur_thickness * ctrls[i][2] / 100;
            var end_x = (i == ctrls.length-1 ? (tboxWidth / 2) + boxOffset : tboxWidth + boxOffset);
            //alert(ix + " : " + end_x);
            ctx.fillRect(ix, iy + (one_thickness - cur_thickness) / 2, end_x, cur_thickness);
            ix += end_x;
        }

        /* Draw boxes */
        /* objective */
        ctx.fillStyle = (node_type == -1 ? NO_SOURCES_COLOR : (node_type == 0 ? LIKELIHOOD_COLOR : IMPACT_COLOR));
        ctx.fillRect(x, y, boxWidth, boxHeight);
        ctx.strokeRect(x, y, boxWidth, boxHeight);
        ctx.fillStyle = FONT_COLOR;
        var title = node_type == -1 ? lblNoSource : node_name;
        rectText(ctx, title, BOWTIE_FONT, x, y, boxWidth, boxHeight, getTextHeight(BOWTIE_FONT), 3);
        x += boxWidth;

        addTooltipDiv(title, x, y, boxWidth, boxHeight);
            
        /* treatments */
        for (var i = 0; i < ctrls.length; i++) {
            x += boxOffset;
            ctx.fillStyle = TREATMENT_COLOR;
            ctx.fillRect(x, y + boxHeight - tboxHeight, tboxWidth, tboxHeight);
            ctx.strokeRect(x, y + boxHeight - tboxHeight, tboxWidth, tboxHeight);
            ctx.fillStyle = FONT_COLOR;
            rectText(ctx, ctrls[i][1], BOWTIE_FONT, x, y + boxHeight - tboxHeight, tboxWidth, tboxHeight, getTextHeight(BOWTIE_FONT), 3);
            ctx.fillText("-" + ctrls[i][2] + "%", x + tboxWidth/2 + 5, y + boxHeight - tboxHeight - 17);
            x += tboxWidth;
        }

        /* event */
        if ((event_name) && event_name != "") {
            x += boxOffset;
            ctx.fillStyle = EVENT_COLOR;
            ctx.fillRect(x, y, boxWidth, boxHeight);
            ctx.strokeRect(x, y, boxWidth, boxHeight);
            ctx.fillStyle = FONT_COLOR;
            rectText(ctx, event_name, BOWTIE_FONT, x, y, boxWidth, boxHeight, getTextHeight(BOWTIE_FONT), 3);
        }            
    }
}

function showExpadedTreatments(data) {
    drawExpadedTreatments(data);

    /* Init and Show the dialog window */
    dlg_expanded = $("#divExpandedTreatmentView").dialog({
        autoOpen: false,
        width: dlg_width,
        height: "auto",
        modal: true,
        closeOnEscape: true,
        dialogClass: "no-close",
        bgiframe: true,
        title: "Expanded View",
        position: { my: "center", at: "center", of: $("body"), within: $("body") },
        buttons: [{ text:"Close", click: function() { dlg_expanded.dialog( "close" ); }}],
        //resizeStop: function() { drawExpadedTreatments(data); },
        open:  function() { },
        close: function() { }
    });

    dlg_expanded.dialog("open");
}

function onContextMenuAddControlClick(ControlType, objGUID) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    addControlClick(ControlType, objGUID)
}

function onContextMenuEditControlAssignmentClick(ControlType, objGUID) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    selectControlsDialog(ControlType, objGUID)
}

function onContextMenuEditControlsClick(ControlType) {
    $("div.context-menu").hide(200); is_context_menu_open = false;
    navigateToApplyControls(ControlType);
}

function onRemoveContributionClick(objGUID, hid) {
    dxConfirm(lblSureRemoveContribution, "sendCommand('action=remove_contribution&node_id=" + objGUID + "&hid=" + hid + "');", "undefined");
}

function onRemoveContributionIconClick(event) {
    event = event || window.event;
    if ((this.getAttribute)) { var hid = this.getAttribute("ctrl_type") * 1 == ctCause ? 0 : 2; onRemoveContributionClick(getObjGUIDFromIndex(this.getAttribute("obj_index"), hid), hid) };
    //if ((event.target.getAttribute)) { event.preventDefault(); var hid = event.target.getAttribute("ctrl_type") * 1 == ctCause ? 0 : 2; onRemoveContributionClick(getObjGUIDFromIndex(event.target.getAttribute("obj_index"), hid), hid) };
}


function navigateToApplyControls(ControlType) {
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
    
function InitApplicationsDlg(data) {
    dlg_applications = $("#divApplications").dialog({
        autoOpen: false,
        width: 600,
        height: 430,
        modal: true,
        dialogClass: "no-close",
        closeOnEscape: true,
        bgiframe: true,
        title: lblControls + " Applications",
        position: { my: "center", at: "center", of: $("body"), within: $("body") },
        buttons: {
            Close: function() {
                dlg_applications.dialog( "close" );
            }
        },
        open: function() {
            InitApplications(data);
        },
        close: function() {
            $("#tblApplications").empty();
        }
    });
}

function InitApplications(data) {
    //        var lbl = $get("lblHeader");
    //        if ((lbl)) {
    //            lbl.innerHTML = '<h6>"' + data[2] + '" is applied to:</h6>';
    //        }
                        
    var grid = $get("tblApplications");
    $("#tblApplications").empty();
    if ((grid)) {
        //init columns headers
        var header = grid.createTHead();
        var hRow = header.insertRow(0);
        hRow.className = "text grid_header";
        var th0 = lblControl;
        var th1 = "Effectiveness";
        var th2 = "Is Active";

        var dc = hRow.insertCell(0); 
        dc.innerHTML = "<b>" + th0 + "</b>";
        dc = hRow.insertCell(1); 
        dc.innerHTML = "<b>" + th1 + "</b>";
        dc = hRow.insertCell(2); 
        dc.innerHTML = "<b>" + th2 + "</b>";
            
        for (var i=0; i<data.length; i++) {
            var tr = grid.insertRow(i + 1);
            tr.className = "text";

            //var c1 = tr.insertCell(0);
            //c1.innerText = data[i][0];
            //var c2 = tr.insertCell(1);
            //c2.innerText = data[i][1] + "";
            //c2.style.textAlign = "center";
            //var c3 = tr.insertCell(2);
            //c3.innerText = data[i][2] + "";
            //c3.style.textAlign = "center";
                
            //for Firefox:
            var c1 = document.createElement("td");
            c1.innerHTML = data[i][0];
            tr.appendChild(c1);
            var c2 = document.createElement("td");
            c2.innerHTML = data[i][1];
            c2.style.textAlign = "center";
            tr.appendChild(c2); 
            var c3 = document.createElement("td");
            c3.innerHTML = data[i][2];
            c3.style.textAlign = "center";
            tr.appendChild(c3); 
        }
    }
}

function ShowApplicationsDialog(data) {
    InitApplicationsDlg(data);
    dlg_applications.dialog("open");
}

var start_drag = { x: 0, y: 0 }; // {x, y}
var cur_drag = { x: 0, y: 0 }; // {x, y}
var menu_pos = { x: 0, y: 0 }; // right-click menu position

var XCoordDrag = 0;
var YCoordDrag = 0;

function doMouseMove(event) {
    
}

function doMouseUp(event) {
    drawBowTie(canvasID);
}

function doMouseDown(event) {
    //start_drag = getCursorCoordinates(event);
    handleMouseClick(event, false);
}

function getCursorCoordinates(event) {
    var point = { x: 0, y: 0 };
    if (window.event) event = window.event;
    if (event) {
        point.x = $("#" + canvasID).offset().left - $(window).scrollLeft();
        point.y = $("#" + canvasID).offset().top - $(window).scrollTop();;

        menu_pos.x = menu_pos.y = 0;

        if ((event.pageX)) {       // FF
            point.x = event.pageX - point.x;
            point.y = event.pageY - point.y;
            menu_pos.x = event.pageX;
            menu_pos.y = event.pageY;
        } else {
            if ((event.clientX)) { // IE
                point.x = event.clientX - point.x;
                point.y = event.clientY - point.y;
                menu_pos.x = event.clientX - 3;
                menu_pos.y = event.clientY - 43;
            }
        }
    }
    return point
}

function canDragNode(node) {
    return false;
}

function highlightControl(is_selected, is_active) {
    $("div.control_div").each(function () {
        this.style.backgroundColor = this.getAttribute("data-bkg-color");
    });
    if (is_selected) {
        $("div.control_div_id_" + highlightedControlIndex).css({ "background-color": HIGHLIGHT_COLOR_ACTIVE });
    }       
}

function handleMouseClick(event, isContextMenu) {
    var oldSelectedNodeID = selectedBoxGuidID;
    var pos = getCursorCoordinates(event);
    var forceDraw = false;

    if (!isContextMenu) selectedBoxGuidID = ""; // if left-clicked then collapse the expanded box

    var fn_check = function (nodes_data) {
        var nodes_data_len = nodes_data.length;
        for (var i = 0; i < nodes_data_len; i++) {
            var v = nodes_data[i][NODE_IDX_COORDS];
            if (((v)) && (((pos) && (v) && (pos.x > v.left) && (pos.x < v.right) && (pos.y > v.top) && (pos.y < v.bottom)) || ((pos.x > v.left + v.width / 2 - 10) && (pos.x < v.right - v.width / 2 + 10) && (pos.y > v.bottom) && (pos.y < v.bottom + 15)))) {
                for (var j = 0; j < nodes_data_len; j++) {
                    nodes_data[j][NODE_IDX_COORDS].expanded = false;
                }
                v.expanded = true;
                if (CanUserSelectBoxes) selectedBoxGuidID = nodes_data[i][NODE_ID];

                var isNoSources = nodes_data.length == 0;

                // show the context (right-click) menu
                if (isContextMenu) {
                    showMenu(menu_pos.x, menu_pos.y, isNoSources)
                } else {
                    //if ((pos.x > v.right - 16) && (pos.x < v.right) && (pos.y > v.bottom - 16) && (pos.y < v.bottom)) {                        
                    if ((pos.x > v.left + v.width / 2 - 10) && (pos.x < v.right - v.width / 2 + 10) && (pos.y > v.bottom) && (pos.y < v.bottom + 15)) {
                        v.expanded = !v.expanded;
                        forceDraw = true;
                    } else {
                        if (canDragNode(nodes_data[i])) {
                            is_drag = true;
                            drag_source = nodes_data[i];
                            drop_target = null;
                            start_drag.x = cur_drag.x = pos.x;
                            start_drag.y = cur_drag.y = pos.y;
                        }
                    }
                }

                i = nodes_data_len; // exit for
            }
        }
    }

    fn_check(likelihood_data);
    fn_check(impact_data);

    if (forceDraw || oldSelectedNodeID != selectedBoxGuidID) {
        drawBowTie(canvasID);
    }

}