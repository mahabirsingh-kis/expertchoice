<%@ Page Language="VB" Inherits="HierarchyPage" title="Hierarchy Designer" Codebehind="Hierarchy.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
<script language="javascript" type="text/javascript" src="/Scripts/jszip.min.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/download.js"></script>
<!--[if lt IE 9]><script language="javascript" type="text/javascript" src="/Scripts/excanvas.min.js"></script><script language="javascript" type="text/javascript">var is_excanvas = true;</script><![endif]-->
<script language="javascript" type="text/javascript" src="/Scripts/jquery.ztree.core.min.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/jquery.ztree.excheck.min.js"></script>
<script language="javascript" type="text/javascript" src="/Scripts/jquery.ztree.exedit.min.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<style type="text/css">
.dx-datagrid-rowsview .dx-data-row .dx-cell-modified .dx-highlight-outline::after {  
    border-color: transparent;  
}
#tableAlts .dx-datagrid-focus-overlay {
    border: 0px;
}
</style>
<script language="javascript" type="text/javascript">

    var isReadOnly = <%=Bool2JS(App.IsActiveProjectStructureReadOnly OrElse PRJ.ProjectStatus = ecProjectStatus.psMasterProject)%>;
    var isRisk = <%=Bool2JS(PRJ.IsRisk)%>;
    var pagination = <%=PM.Parameters.Riskion_Control_Pagination%>;
    var is_multiselect = true; //<%=Bool2JS(PM.Parameters.Hierarchy_MultiselectEnabled)%>;
    var is_autoredraw = false;
    var infodoc_splitter_size = <%=PM.Parameters.Hierarchy_VerticalSplitterSize%>;
    var curPgID = <%=CurrentPageID%>;
    var _PGID_STRUCTURE_ALTERNATIVES = <%=_PGID_STRUCTURE_ALTERNATIVES%>;

    var CurrencyThousandSeparator = "<% = UserLocale.NumberFormat.CurrencyGroupSeparator %>";
    
    var OPT_BUS_FOR_FULLY_CONTRIBUTED_LEVELS = true;   // show a 'bus'-style connection lines for the full-contributed levels
    var OPT_LINE_STYLE_DIRECT                = false;  // use only direct connection lines
    var OPT_VERT_LINE_PART_HEIGHT_FULL_CONTR = 20;     // height of the vertical part of the connection line for the full contributed level
    var OPT_VERT_LINE_PART_HEIGHT            = 10;     // height of the vertical part of the connection line  

    var alts_data = <%=GetAlternativesData()%>;   

    var nodes_data = <% = GetNodesData() %>;
    var nodesets_list = null;

    var IDX_ID      = 0;
    var IDX_GUID    = 1;
    var IDX_NAME    = 2;
    var IDX_PARENT_GUIDS = 3;
    var IDX_LEVEL   = 4;
    var IDX_COORDS  = 5;     // coordinates and measures of the rectangle (.top, .left, .right, .bottom, .width, .height)
    var IDX_INFODOC = 6;
    var IDX_IS_ALT  = 7;     // 0 or 1
    var IDX_IS_TERMINAL = 8; // 0 or 1
    var IDX_IS_CATEGORY = 9; // 0 or 1
    var IDX_IS_EXPANDED = 10;// 0 or 1
    var IDX_IS_ENABLED  = 11;// 0 or 1
    var IDX_LOCAL  = 12;
    var IDX_GLOBAL  = 13;
    var IDX_PRIORITIES_STRING = 14;
    var IDX_ATTRIBUTES_START  = 15;

    var cmd = "";    
    
    var users_data = [<% = If(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES, "", GetUsersData()) %>];   // participants 
    var activeUserID = <%=UsersList.Where(Function (usr) usr.UserEmail.ToLower = App.ActiveUser.UserEmail.ToLower)(0).UserID%>;
    function users_count() { return users_data.length; }

    var IDX_USER_ID = 0;
    var IDX_USER_EMAIL = 1;
    var IDX_USER_NAME = 2;
    var IDX_USER_AHP_USER_ID = 3;

    var MinCanvasWidth  = 350;
    var MinCanvasHeight = 150;

    var DefaultRectangleHeight = <%=clsProjectParametersWithDefaults.DEFAULT_HIERARCHY_RECTANGLE_HEIGHT%>;
    var DefaultRectangleMinWidth = <%=clsProjectParametersWithDefaults.DEFAULT_HIERARCHY_RECTANGLE_MIN_WIDTH%>;
    var DefaultRectangleMaxWidth = <%=clsProjectParametersWithDefaults.DEFAULT_HIERARCHY_RECTANGLE_MAX_WIDTH%>;

    var RectangleHeight = <%=PM.Parameters.Hierarchy_RectangleHeight%>;
    var RectangleMinWidth = <%=PM.Parameters.Hierarchy_RectangleMinWidth%>;
    var RectangleMaxWidth = <%=PM.Parameters.Hierarchy_RectangleMaxWidth%>;
    
    var DefaultRectangleVerticalMargin = 40;
    var DefaultRectangleHorizontalMargin = 8;

    var STROKE_COLOR     = "#6e6e6e"; // box border color
    var STROKE_THICKNESS = "2";       // box border thickness
    var ARROW_COLOR      = "#8c8c8c"         //"#bfbfbf";  arrow (connecting line) color
    var ARROW_THICKNESS  = "1";              // arrow (connecting line) thickness
    
    var SELECTED_STROKE_COLOR     = "#326cd1";  //"#5d8bdb";  // selected box border color
    var SELECTED_STROKE_THICKNESS = 4;    // selected box border thickness
    var SELECTED_ARROW_COLOR      = "#8c8c8c"
    var SELECTED_ARROW_THICKNESS  = 2;    // selected arrow (connecting line) thickness
       
    var FONT_COLOR      = "#000000";
    var FONT_CATEGORY_COLOR = "#0000ff"; //"#0000ff";

    //var OBJ_COLOR       = "#00ebc4";
    //var OBJ_MULTI_PARENTS_COLOR = "#c8d0ec";

    var OBJ_BRUSH_SINGLE_0 = "#dbefff"; // light gradient color
    var OBJ_BRUSH_SINGLE_1 = "#4c94e0"; // dark  gradient color
    
    var OBJ_BRUSH_MULTI_0  = "#fdfdfc"; // light gradient color       
    var OBJ_BRUSH_MULTI_1  = "#d7e7f7"; // dark  gradient color

    //var ALT_BRUSH_0 = "#ffebee"; // light gradient color       
    //var ALT_BRUSH_1 = "#fadad0"; // dark  gradient color

    var ALT_BRUSH_0 = "#fdfdfc"; // light gradient color       
    var ALT_BRUSH_1 = "#ddffbb"; // dark  gradient color

    var ALT_LINE_BRUSH          = "#74d145"; // connecting line color
    var ALT_LINE_SELECTED_BRUSH = "#555555"; //"#478f24"; // selected connecting line color

    var DRAG_COLOR        = "#eee"; // draggable item fill color
    var DRAG_STROKE_COLOR = "#bbb"; // draggable item border color

    var DROP_COLOR        = "#48d1cc"; // drop target item fill color
    var DROP_STROKE_COLOR = "#d8bfd8"; // drop target item border color
    
    var DISABLED_STROKE_COLOR = "#ccc"; // disabled item border color
    var DISABLED_FILL_COLOR = "#f5f5f5"; // disabled item border color

    var OBJ_BRUSH_SINGLE_PARENT;
    var OBJ_BRUSH_MULTI_PARENT;
    var ALT_BRUSH;

    var DEFAULT_FONT = "12px arial";
    var DEFAULT_FONT_BOLD = "bold 12px arial";
    var DEFAULT_FONT_ITALIC = "italic 12px arial";

    var selectedNodeID = curPgID == _PGID_STRUCTURE_ALTERNATIVES ? (alts_data.length > 0 ? alts_data[0]["guid"] : "") : (nodes_data.length > 0 ? nodes_data[0][IDX_GUID] : "");   
    var mouseOverNode = [];
    var selInfodocIDs;

    var mh = 500;  // maxHeight for dialogs;
    
    var dlg_new_node = null;
    
    var node_name    = "";
    var node_parent_nodes = "";
    var tooltip_node = null;

    var cancelled = false;
    var is_context_menu_open = false;
    var loading = 0;
    var is_complete_hierarchy  = false; // true if any of the hierarchy nodes has multiple parents
    var is_show_alternatives = <%=Bool2JS(ShowAlternatives)%>;
    var is_show_alternatives_priorities = <%=Bool2JS(PM.Parameters.Hierarchy_ShowAlternativesPriorities)%>;
    var objectiveIndexVisible = <% = Bool2JS(PM.Parameters.ObjectiveIndexIsVisible) %>;

    var ActiveView = <%=CInt(ActiveView)%>;
    var hvGraph    = <%=CInt(HierarchyViews.hvGraph)%>;
    var hvTree     = <%=CInt(HierarchyViews.hvTree)%>;
    <%--var hvAlts     = <%=CInt(HierarchyViews.hvAlts)%>;--%>

    var zTreeObj;
    var tree_nodes_data = <% = GetTreeNodesData()%>;

    function synchTreeExpanded(nodes) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i = 0; i < nodes_len; i++) {
                var curState = localStorage.getItem("zTreeNode_<%=App.ProjectID%>_" + nodes[i].id);
                if (curState == "closed") {
                    nodes[i].open = false;
                }
                if (i < nodes_len) synchTreeExpanded(nodes[i].children);
            }
        }
    }

    var display_priorities_mode = <%=PM.Parameters.Structure_DisplayPrioritiesMode%>;

    var PARAM_DELIMITER = ";";

    // actions
    var ACTION_ADD_LEVEL_BELOW   = "<%=ACTION_ADD_LEVEL_BELOW%>";
    var ACTION_ADD_SAME_LEVEL    = "<%=ACTION_ADD_SAME_LEVEL%>";
    var ACTION_ADD_FROM_DATASET  = "<%=ACTION_ADD_FROM_DATASET%>"; //SL16152
    var ACTION_ADD_FROM_DATASET_ALTS  = "<%=ACTION_ADD_FROM_DATASET_ALTS%>"; //SL16152
    var ACTION_EDIT_NODE         = "<%=ACTION_EDIT_NODE%>";
    var ACTION_DELETE_NODE       = "<%=ACTION_DELETE_NODE%>";
    var ACTION_CONVERT_HIERARCHY = "<%=ACTION_CONVERT_HIERARCHY%>";
    var ACTION_SHOW_ALTERNATIVES = "<%=ACTION_SHOW_ALTERNATIVES%>";
    var ACTION_ADD_ALTERNATIVES  = "<%=ACTION_ADD_ALTERNATIVES%>";
    var ACTION_MOVE_NODE         = "<%=ACTION_MOVE_NODE%>";
    var ACTION_ENABLE_NODE       = "<%=ACTION_ENABLE_NODE%>";
    var ACTION_SWITCH_VIEW       = "<%=ACTION_SWITCH_VIEW%>";
    var ACTION_MOVE_TREE_VIEW_NODE = "<%=ACTION_MOVE_TREE_VIEW_NODE%>";
    var ACTION_EXPORT_OBJECTIVES = "<%=ACTION_EXPORT_OBJECTIVES%>";
    var ACTION_EXPORT_ALTERNATIVES = "<%=ACTION_EXPORT_ALTERNATIVES%>";
    var ACTION_EXPORT_MODEL      = "<%=ACTION_EXPORT_MODEL%>";
    var ACTION_EXPORT_MODEL_FILE = "<%=ACTION_EXPORT_MODEL_FILE%>";
    var ACTION_DELETE_JUDGMENTS_FOR_NODE = "<%=ACTION_DELETE_JUDGMENTS_FOR_NODE%>";
    var ACTION_COPY_JUDGMENTS_TO_LOCATION = "<%=ACTION_COPY_JUDGMENTS_TO_LOCATION%>";
    var ACTION_PASTE_JUDGMENTS_FROM_LOCATION = "<%=ACTION_PASTE_JUDGMENTS_FROM_LOCATION%>";
    var ACTION_DISPLAY_PRIORITIES = "<%=ACTION_DISPLAY_PRIORITIES%>";
    var ACTION_SORT_CLUSTERS = "<%=ACTION_SORT_CLUSTERS%>";
    var ACTION_EDIT_ATTRIBUTES = "<%=ACTION_EDIT_ATTRIBUTES%>";

    var levelNodeWidth  = []; // width of the box for each of the levels
    var levelNodesCount = []; // number of nodes on each level
    var levelConnectedWithAllParents  = []; // are all nodes of this level connected with all parent nodes
    var levelConnectedWithAllChildren = []; // are all nodes of this level connected with all children
   
    var canvas; // the canvas object where we can draw figures
    var ctx;    // drawing context

    var is_drag = false;   
    var drag_source = null;
    var drop_target = null;

    var start_drag  = {x:0, y:0}; // {x, y}
    var cur_drag    = {x:0, y:0}; // {x, y}
    var menu_pos    = {x:0, y:0}; // right-click menu position

    var canCopyJudgments = true;
    var canPasteJudgments = <%=Bool2JS(Not JudgmentsSourceNodeID.Equals(Guid.Empty))%>;

    var dlg_copy_judgments = null;
    var dlg_paste_judgments = null;

    var XCoordDrag = 0;
    var YCoordDrag = 0;

    function initCanvas() {
        canvas = document.getElementById("canvasHierarchy");        
        if ((typeof G_vmlCanvasManager != 'undefined') && (G_vmlCanvasManager)) G_vmlCanvasManager.initElement(canvas);
        if (!canvas.getContext) return false;
        ctx = canvas.getContext("2d");       
        if (!ctx.setLineDash) {
            ctx.setLineDash = function () {};
        };	

        
        // add mouse events listeners
        if (canvas.addEventListener) {                        
            canvas.addEventListener("mousedown", doMouseDown, false);
            if (!isReadOnly) {
                canvas.addEventListener("mouseup", doMouseUp, false);
                canvas.addEventListener("mousemove", doMouseMove, false);
                canvas.addEventListener("dblclick", doDblClick, false);      
                canvas.addEventListener("contextmenu", doContextMenu, false);      
            }
        } else {
            canvas.attachEvent("onclick", doMouseDown);
            if (!isReadOnly) {
                //canvas.attachEvent("onmouseup", doMouseUp);
                //canvas.attachEvent("onmousedown", doMouseDown);
                //canvas.attachEvent("onmousemove", doMouseMove);
                canvas.attachEvent("ondblclick", doDblClick);
                canvas.attachEvent("oncontextmenu", doContextMenu);
            }
        }

        // gradient fill for boxes
        OBJ_BRUSH_SINGLE_PARENT = ctx.createLinearGradient(0, 0, 0, RectangleHeight);
        OBJ_BRUSH_SINGLE_PARENT.addColorStop(0, OBJ_BRUSH_SINGLE_0);        
        OBJ_BRUSH_SINGLE_PARENT.addColorStop(1, OBJ_BRUSH_SINGLE_1);

        OBJ_BRUSH_MULTI_PARENT = ctx.createLinearGradient(0, 0, 0, RectangleHeight);
        OBJ_BRUSH_MULTI_PARENT.addColorStop(0, OBJ_BRUSH_MULTI_0);
        OBJ_BRUSH_MULTI_PARENT.addColorStop(1, OBJ_BRUSH_MULTI_1);

        ALT_BRUSH = ctx.createLinearGradient(0, 0, 0, RectangleHeight);
        ALT_BRUSH.addColorStop(0, ALT_BRUSH_0);
        ALT_BRUSH.addColorStop(1, ALT_BRUSH_1);
    }

    function getIsCompleteHierarchy() {
        is_complete_hierarchy = false;
        var nodes_data_len = nodes_data.length;
        for (var i = 0; i < nodes_data_len; i++) {
            if (nodes_data[i][IDX_IS_ALT] == 0 && (nodes_data[i][IDX_PARENT_GUIDS]) && nodes_data[i][IDX_PARENT_GUIDS].length > 1) { is_complete_hierarchy = true; return }
        }
    }    
    
    function drawHierarchy() {
    
        loading+=1;
        if (loading<3 && !(ctx) || !(ctx.fillStyle)) { setTimeout(function () { drawHierarchy(); }, 250); return false; }
          		
        // fill the canvas with white color - works best in IE and FF
        ctx.fillStyle = "#ffffff";
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        //var contentHeight = $("#tdGridCanvas").height();
        var contentWidth = $("#tdGridCanvas").width();       
        canvas.width  = (contentWidth > MinCanvasWidth  ? contentWidth  : MinCanvasWidth);

        // this should be the regular way to clear the canvas, but it produces blinking in IE (with excanvas);
        // ctx.clearRect(0, 0, canvas.width, canvas.height);

        ctx.lineWidth = STROKE_THICKNESS;
        ctx.font = DEFAULT_FONT;        
       
        is_complete_hierarchy = false;

        levelNodesCount = [];
        levelNodeWidth  = [];
        levelConnectedWithAllParents  = [];
        levelConnectedWithAllChildren = [];

        var nodes_data_len = nodes_data.length;
        for (var i = 0; i < nodes_data_len; i++) {
            
            var curLevel = nodes_data[i][IDX_LEVEL] + 1;
            
            if (levelNodesCount.length < curLevel) {
                while (levelNodesCount.length < curLevel) {
                    levelNodesCount.push(0);
                }
            }
            
            if (levelNodeWidth.length < curLevel) {
                while (levelNodeWidth.length < curLevel) {
                    levelNodeWidth.push(0);
                }
            }

            nodes_data[i][IDX_COORDS] = {};
            nodes_data[i][IDX_COORDS].isVisible = is_show_alternatives || !nodes_data[i][IDX_IS_ALT]; //true;
            if (nodes_data[i][IDX_IS_ALT] != 1) nodes_data[i][IDX_COORDS].isVisible = !anyParentCollapsed(nodes_data[i]);
            if (!nodes_data[i][IDX_COORDS].isVisible) levelNodesCount[curLevel - 1] -= 1;
            
            levelNodesCount[curLevel - 1] += 1;
            if (nodes_data[i][IDX_IS_ALT] == 0 && (nodes_data[i][IDX_PARENT_GUIDS]) && nodes_data[i][IDX_PARENT_GUIDS].length > 1) is_complete_hierarchy = true;
        }

        for (var i = 0; i < levelNodesCount.length; i++) {
            levelConnectedWithAllParents.push(true);
            levelConnectedWithAllChildren.push(true);
        }

        var maxLevels = levelNodesCount.length;
        var covObjsCount = getTerminalNodes().length;
        
        for (var i = 0; i < nodes_data_len; i++) {
            var node  = nodes_data[i];            
            var level = node[IDX_LEVEL];
                        
            if (level > 0) {
                if ((node[IDX_IS_ALT] == 0 && node[IDX_PARENT_GUIDS].length != levelNodesCount[level-1]) || (node[IDX_IS_ALT] == 1 && node[IDX_PARENT_GUIDS].length != covObjsCount)) {
                    levelConnectedWithAllParents [level] = false;
                    levelConnectedWithAllChildren[level-1] = false;        
                }
            }            
        }
       
        for (var i = 0; i < maxLevels; i++) {
            var lw = RectangleMaxWidth;
            var margins = levelNodesCount[i] * DefaultRectangleHorizontalMargin;
            if (levelNodesCount[i] * RectangleMaxWidth + margins > canvas.width) {
                var new_lw = float2int((canvas.width - margins) / levelNodesCount[i]);
                if (new_lw >= RectangleMinWidth) {
                    lw = new_lw;
                } else {
                    lw = RectangleMinWidth;
                    canvas.width = levelNodesCount[i] * lw + margins;
                }                
            }
            levelNodeWidth[i] = lw;
        }
        
        var tooltipOffset = 40;
        var height = maxLevels * RectangleHeight + maxLevels * DefaultRectangleVerticalMargin + tooltipOffset;
        
        //var contentHeight = $("#tdGridCanvas").height();
        canvas.height = (height > MinCanvasHeight ?  height : MinCanvasHeight);

        initGraph();
        drawConnectingLines();
        tooltip_node = null;
        drawBoxes();
        drawTooltip();
        //updateToolbar();
        updateLegend();
    }

    function handleMouseClick(event, isContextMenu) {
        var oldSelectedNodeID = selectedNodeID;
        var pos = getCursorCoordinates(event);
        var forceDraw = false;

        var nodes_data_len = nodes_data.length;        
        for (var i=0; i<nodes_data_len; i++) {    
            var v = nodes_data[i][IDX_COORDS];
            if (((pos) && (v) && (pos.x > v.left) && (pos.x < v.right) && (pos.y > v.top) && (pos.y < v.bottom)) || ((pos.x > v.left + v.width / 2 - 10) && (pos.x < v.right - v.width / 2 + 10) && (pos.y > v.bottom) && (pos.y < v.bottom + 15))) {
                setSelectedNode(nodes_data[i][IDX_GUID]);                

                // show the context (right-click) menu
                if (isContextMenu) {                    
                    showMenu(menu_pos.x, menu_pos.y)
                } else {
                    //if ((pos.x > v.right - 16) && (pos.x < v.right) && (pos.y > v.bottom - 16) && (pos.y < v.bottom)) {                        
                    if (!is_complete_hierarchy && (pos.x > v.left + v.width / 2 - 10) && (pos.x < v.right - v.width / 2 + 10) && (pos.y > v.bottom) && (pos.y < v.bottom + 15)) {
                        nodes_data[i][IDX_IS_EXPANDED] = nodes_data[i][IDX_IS_EXPANDED] == 0 ? 1 : 0;
                        localStorage.setItem("zTreeNode_<%=App.ProjectID%>_" + nodes_data[i][IDX_GUID], nodes_data[i][IDX_IS_EXPANDED] == 1 ? "open" : "closed");
                        forceDraw = true;
                    } else {
                        if (canDragNode(nodes_data[i])) {
                            is_drag = true;
                            drag_source = nodes_data[i];
                            drop_target = null;
                            start_drag.x = cur_drag.x =pos.x;
                            start_drag.y = cur_drag.y =pos.y;
                        }
                    }
                }

                i = nodes_data_len; // exit for
            }
        }

        if (forceDraw || oldSelectedNodeID != selectedNodeID) {
            drawHierarchy();
        }
        
    }

    var nodeMenuItems = [
        { text: 'Move', action: "move" },
        { text: 'Copy', action: "copy" }
    ];

    function doMouseUp(event) {
        if (is_drag && (drag_source) && (drop_target)) {
            sourceid = drag_source[IDX_GUID];
            targetid = drop_target[IDX_GUID];
            //var cMenu = $("#node-menu").dxContextMenu("instance");
            //cMenu.option("target", "#canvasHierarchy");
            //cMenu.show();            
            dxDialog("<div id='cbAction'></div><br><div id='cbCopyJudgments'></div>", "proceedMoveNode();", ";", "Confirmation");
            $("#cbAction").dxRadioGroup({
                dataSource: [ "Move", "Copy" ],
                value: "Move",
                searchEnabled: false
            });
            $("#cbCopyJudgments").dxCheckBox({
                text: resString("confDuplicateJudgments"),
                value: true
            });
            $(".dx-texteditor-input")[0].select();
        }
        is_drag = false;
        drop_target = null;
        drag_source = null;
        cur_drag.x = cur_drag.y = start_drag.x = start_drag.y = 0;
        drawHierarchy();
    }

    function proceedMoveNode() {
        var node_action = $("#cbAction").dxRadioGroup("instance").option("value");
        var copy_judgments = $("#cbCopyJudgments").dxCheckBox("instance").option("value");        
        sendCommand("action=" + ACTION_MOVE_NODE + "&source=" + sourceid + "&target=" + targetid + "&node_action=" + node_action + "&copy_judgments=" + copy_judgments);
    }

    function doMouseDown(event) {        
        start_drag = getCursorCoordinates(event);
        handleMouseClick(event, false);
    }

    function doMouseMove(event) {
        if (is_drag) {
            cur_drag = getCursorCoordinates(event);
            drawHierarchy();
        } else {
            cur_drag.x = cur_drag.y = start_drag.x = start_drag.y = 0;
            getMouseOverNode(event);
        }
    }

    function doDblClick(event) {
        resetPageSelection();
        initNewNodeDlg(ACTION_EDIT_NODE);
    }

    function getCursorCoordinates(event) {
        var point = {x:0, y:0};
        if (window.event) event = window.event;
        if (event) {            
            point.x = $("#canvasHierarchy").offset().left - $(window).scrollLeft();
            point.y = $("#canvasHierarchy").offset().top  - $(window).scrollTop();;            
            
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

    function getMouseOverNode(event) {       
        var node = [];
        var pos = getCursorCoordinates(event);

        var nodes_data_len = nodes_data.length;        
        for (var i=0; i<nodes_data_len; i++) {    
            var v = nodes_data[i][IDX_COORDS];
            if ((pos) && (v) && (pos.x > v.left) && (pos.x < v.right) && (pos.y > v.top) && (pos.y < v.bottom)) {
                node = nodes_data[i];
                i = nodes_data_len; // exit for
            }
        }

        if (node.length != 0 && (mouseOverNode.length == 0 || (node[IDX_GUID] != mouseOverNode[IDX_GUID]))) {
            mouseOverNode = node;
            drawHierarchy();
        }
    }

    function doContextMenu(event) {        
        handleMouseClick(event, true);
        if ((event.preventDefault)) event.preventDefault(); // for FF
        return false; // to prevent IE browser's right-click menu
    }

    function isGoal(node) {
        return (node[IDX_IS_ALT] == 0) && (!(node[IDX_PARENT_GUIDS]) || (node[IDX_PARENT_GUIDS].length == 0));
    }

    function canDragNode(node) {
        if (isReadOnly) return false;
        var retVal = !isGoal(node);
        if (retVal && (node[IDX_IS_ALT] == 0)) {
            var nodes_data_len = nodes_data.length;            
            // create a list of all nodes in cluster
            var cluster = [];
            cluster.push(node);
            var cluster_ids = [];
            cluster_ids.push(node[IDX_GUID]);
            var parent_ids = [];
            var j = 0;
            while (j < cluster.length) {
                for (var i = 0; i < nodes_data_len; i++) {
                    if ((nodes_data[i][IDX_IS_ALT] == 0) && ($.inArray(nodes_data[i], cluster) == -1) && ($.inArray(cluster[j][IDX_GUID], nodes_data[i][IDX_PARENT_GUIDS]) >= 0)) {
                        cluster.push(nodes_data[i]);
                        cluster_ids.push(nodes_data[i][IDX_GUID]);
                        for (var k = 0; k < nodes_data[i][IDX_PARENT_GUIDS].length; k++) {
                            var pid = nodes_data[i][IDX_PARENT_GUIDS][k];
                            if ($.inArray(pid, parent_ids) == -1) {
                                parent_ids.push(pid);
                            }
                        }
                    }
                }
                j += 1;
            }
            // check if any node in cluster (except the self) has a parent node outside of cluster
            var cluster_len = cluster_ids.length;
            var parent_len  = parent_ids.length;
            for (var i = 0; i < parent_len; i++) {
                if (retVal && ($.inArray(parent_ids[i], cluster_ids) == -1)) {
                     retVal = false;
                     i = parent_len;            // exit for
                }
            }
        }
        return retVal;
    }

    function canDropToNode(target) {
        var retVal = false;
        //if ((drag_source) && !retVal && (target[IDX_IS_ALT] == 0) && (target[IDX_GUID] != drag_source[IDX_GUID]) && ($.inArray(target[IDX_GUID], drag_source[IDX_PARENT_GUIDS]) == -1)) {
        if ((drag_source) && !retVal && (target[IDX_IS_ALT] == 0) && (target[IDX_GUID] != drag_source[IDX_GUID])) {
                var nodes_data_len = nodes_data.length;            
                // create a list of all nodes in cluster
                var cluster = [];
                cluster.push(drag_source);
                var cluster_ids = [];
                cluster_ids.push(drag_source[IDX_GUID]);
                var parent_ids = [];
                var j = 0;
                while (j < cluster.length) {
                    for (var i = 0; i < nodes_data_len; i++) {
                        if ((nodes_data[i][IDX_IS_ALT] == 0) && ($.inArray(nodes_data[i], cluster) == -1) && ($.inArray(cluster[j][IDX_GUID], nodes_data[i][IDX_PARENT_GUIDS]) >= 0)) {
                            cluster.push(nodes_data[i]);
                            cluster_ids.push(nodes_data[i][IDX_GUID]);
                        }
                    }
                    j += 1;
                }
                // check if trying to drop to any child of current cluster
                retVal = $.inArray(target[IDX_GUID], cluster_ids) == -1;
        }
        //$('#tbNodeTitle').html('target id: ' + target[IDX_GUID] + '<br>' + cluster_ids);
        return retVal;
    }

    function showMenu(x, y) {        
        if (isReadOnly) return;
        is_context_menu_open = false;
        $("#contextmenuheader").hide().remove();
        
        var selNode = getSelectedNode();

        var sMenu = "<div id='contextmenuheader' class='context-menu'>";
        if (selNode[IDX_IS_ALT] == 0) {
            <% If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then %>
            if (selNode[IDX_IS_CATEGORY] !== 1) {
                sMenu += "<a href='' class='context-menu-item' onclick='changeRiskNodeType(<% = CInt(RiskNodeType.ntCategory) %>); return false;'><div style='height:18px;'><nobr><i class='fas fa-circle'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;Set as a Category&nbsp;</span></nobr></div></a>";
            } else {
                sMenu += "<a href='' class='context-menu-item' onclick='changeRiskNodeType(<% = CInt(RiskNodeType.ntUncertainty) %>); return false;'><div style='height:18px;'><nobr><i class='far fa-circle'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;Set as an Uncertainty&nbsp;</span></nobr></div></a>";
            }
            <% End If %>
            sMenu += "<a href='' class='context-menu-item' onclick='initNewNodeDlg(ACTION_ADD_LEVEL_BELOW); return false;'><div style='height:18px;'><nobr><i class='fas fa-plus'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("cmnuObjectivesAddBelow")%>&nbsp;</span></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='if (!this.disabled) initNewNodeDlg(ACTION_ADD_SAME_LEVEL); return false;' " + (isGoal(selNode) ? " disabled='disabled' style='pointer-events: none;opacity:0.4;filter:alpha(opacity=40);'" : "") + "><div style='height:18px;'><nobr><i class='fas fa-plus-circle'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("cmnuObjectivesAddSibling")%>&nbsp;</span></nobr></div></a>";
        }
        sMenu += "<a href='' class='context-menu-item' onclick='getNodeSets(); return false;'><div style='height:18px;'><nobr><i class='fas fa-plus'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("cmnuObjectivesAddFromDataset")%>&nbsp;</span></nobr></div></a>"; //SL16152
        sMenu += "<a href='' class='context-menu-item' onclick='initNewNodeDlg(ACTION_EDIT_NODE); return false;'><div style='height:18px;'><nobr><i class='fas fa-pencil-alt'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnEdit")%>&nbsp;</span></nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='EditInfodoc(); return false;'><div style='height:18px;'><nobr><i class='fas fa-info-circle'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnEditInfodoc")%>&nbsp;</span></nobr></div></a>";
        if (!isGoal(selNode)) {
            <%--sMenu += "<a href='' class='context-menu-item' onclick='toggleSelectedNodeEnabledState(); return false;'><div style='height:18px;'><nobr>" + (selNode[IDX_IS_ENABLED] == 1 ? "<i class='far fa-square'></i>" : "<i class='far fa-check-square'></i>") + "<span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;" + (selNode[IDX_IS_ENABLED] == 1 ? "<%=ParseString("Disable")%>" : "<%=ParseString("Enable")%>") + "&nbsp;</span></nobr></div></a>";--%>
            sMenu += "<a href='' class='context-menu-item' onclick='deleteSelectedNode(); return false;'><div style='height:18px;'><nobr><i class='fas fa-trash-alt'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnDelete")%>&nbsp;</span></nobr></div></a>";
        }
        sMenu += "<a href='' class='context-menu-item' onclick='copyJudgmentsToLocation(); return false;'><div style='height:18px;'><nobr><i class='far fa-copy'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnHierarchyCopyJudgmentsToClipboard")%>&nbsp;</span></nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='pasteJudgmentsFromLocation(); return false;' " + (!canPasteJudgments ? " disabled='disabled' style='pointer-events: none;opacity:0.4;filter:alpha(opacity=40);'" : "") + "><div style='height:18px;'><nobr><i class='fas fa-paste'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnHierarchyPasteJudgmentsFromClipboard")%>&nbsp;</span></nobr></div></a>";
        if (selNode[IDX_IS_ALT] == 0) {
            <%--sMenu += "<a href='' class='context-menu-item' onclick='deleteJudgmentsForSelectedNode(); return false;' disabled='disabled' style='pointer-events:none; opacity:0.4; filter:alpha(opacity=40);'><div style='height:18px;'><nobr><img align='left' width='18' height='18' style='vertical-align:middle;' src='<%=ImagePath()%>del_judg_20.png' alt='' ><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnDeleteJudgmentsForNode")%>&nbsp;</span></nobr></div></a>";--%>
            sMenu += "<a href='' class='context-menu-item' onclick='deleteJudgmentsForSelectedNode(0, 0); return false;'><div style='height:18px;'><nobr><i class='fas fa-eraser'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnDeleteJudgmentsForNode")%>&nbsp;</span></nobr></div></a>";
            if (selNode[IDX_IS_TERMINAL] == 0) {
                sMenu += "<a href='' class='context-menu-item' onclick='deleteJudgmentsForSelectedNode(0, 1); return false;'><div style='height:18px;'><nobr><i class='fas fa-eraser'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnDeleteJudgmentsForPlex")%>&nbsp;</span></nobr></div></a>";
            }
        } else {
            sMenu += "<a href='' class='context-menu-item' onclick='deleteJudgmentsForSelectedNode(1, 0); return false;'><div style='height:18px;'><nobr><i class='fas fa-eraser'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;<%=ResString("btnDeleteJudgmentsForAlt")%>&nbsp;</span></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='duplicateSelectedAlternative(); return false;'><div style='height:18px;'><nobr><i class='fas fa-clone'></i><span style='display:inline-block; padding-top:3px;'>&nbsp;&nbsp;&nbsp;Duplicate&nbsp;</span></nobr></div></a>";
        }
        sMenu += "</div>";                
        
        var containerName = "tdGridCanvas";
        if (ActiveView == hvTree) containerName = "divHierarchyTree";
        var offset = $("#" + containerName).offset();
        y -= offset.top;
        x -= offset.left;
        var s = $(sMenu).appendTo("#" + containerName).css({position: "absolute", top: y + "px", left: x + "px"});
        if ((s)) { 
            var w = s.width();
            var pw = $("#divContentPane").innerWidth(); 
            if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"});
            var h = s.height();
            var ph = $("#divContentPane").innerHeight(); 
            if ((ph) && (y+h>ph) && (ph-h-2 > 0)) s.css({top: (ph-h-2) + "px"});
        }
        
        $("#contextmenuheader").fadeIn(500);
        setTimeout(function () { canCloseMenu(); }, 200);
    }

    function canCloseMenu() {
        is_context_menu_open = true;
        $(document).unbind("click").bind("click", function () { if (is_context_menu_open == true) { $("#contextmenuheader").hide(200); is_context_menu_open = false; } });        
    }    

    function getAlternativeById(guid) {        
        var alts_data_len = alts_data.length;
        for (var i = 0; i < alts_data_len; i++) {
            if (alts_data[i]["guid"] == guid) {                
                return alts_data[i];
            }
        }
        return null;
    }

    function getNodeByGUID(id) {
        var nodes_data_len = nodes_data.length;
        for (var i = 0; i < nodes_data_len; i++) {
            if (nodes_data[i][IDX_GUID] == id) {                
                return nodes_data[i];
            }
        }
        return null;
    }

    //function setSelectedAlternative(guid) {
    //    var selNode = getSelectedNode();
    //    var is_alt  = (selNode) && (selNode[IDX_IS_ALT] == 1);
    //    var table_main = (($("#tableAlts").data("dxDataGrid"))) ? $("#tableAlts").dxDataGrid("instance") : null;
    //    if (is_alt) {
    //        setSelectedNode(selectedNodeID);
    //        if (table_main !== null) {
    //            table_main.selectRows([selectedNodeID], false); 
    //        }
    //    } else {
    //        if (table_main !== null && alts_data.length > 0 && table_main.option("selectedRowKeys").length == 0) { 
    //            selectedNodeID = alts_data[0]["guid"];
    //            table_main.selectRowsByIndexes([0]); 
    //            table_main.option("focusedRowIndex", 0);
    //            setSelectedNode(selectedNodeID);
    //        }
    //    }
    //}

    function setSelectedNode(id, treeNode) {
        selectedNodeID = id;
        var selNode = getSelectedNode();
        var is_alt  = (selNode) && (selNode[IDX_IS_ALT] == 1);
        var is_terminal = (selNode) && (selNode[IDX_IS_TERMINAL] == 1);

        $("#tbNodeTitle").html(htmlEscape(selNode[IDX_NAME]));
        LoadInfodoc(selNode[IDX_ID], selNode[IDX_GUID], selNode[IDX_IS_ALT] == 1);

        <%--var toolbar = $find("<%= RadToolBarMain.ClientID %>");--%>
        if (!$("#toolbar").data("dxToolbar")) initToolbar();
        var toolbar = $("#toolbar").dxToolbar("instance");
        var button_edit = $("#btn_" + ACTION_EDIT_NODE).dxButton("instance");
        var button_delete = $("#btn_" + ACTION_DELETE_NODE).dxButton("instance");
        //var button_sub_node = $("#btn_" + ACTION_ADD_LEVEL_BELOW).dxButton("instance");
        //var button_add_same_level = $("#btn_" + ACTION_ADD_SAME_LEVEL).dxButton("instance");        

        //can't delete Goal node - disable the toolbar button
        if ((selNode)) {            
            if ((button_edit)) button_edit.option("disabled", false);
            button_delete.option("disabled", true);
            //button_sub_node.option("disabled", false);
            //button_add_same_level.option("disabled", true);
            if (is_alt) {
                //button_sub_node.option("disabled", true);
                button_delete.option("disabled", false);
            } else {
                if (!isGoal(selNode) && typeof treeNode == "undefined") {
                    //button_add_same_level.option("disabled", false);
                    button_delete.option("disabled", false);
                }
            }            
        } else {
            if ((button_edit)) button_edit.option("disabled", true);
            button_delete.option("disabled", true);
            //button_sub_node.option("disabled", true);
            //button_add_same_level.option("disabled", true);
        }

        if (is_multiselect) {
            // ?
        }

        $("#btn_sort").dxDropDownButton("instance").option("disabled", isReadOnly || !(curPgID == _PGID_STRUCTURE_ALTERNATIVES && alts_data.length || (curPgID !== _PGID_STRUCTURE_ALTERNATIVES && (!(selNode) || !is_terminal))));
    }

    function alternativesCount() {
        var retVal = 0;
        var nodes_data_len = nodes_data.length;
        for (var i = 0; i < nodes_data_len; i++) {
            if (nodes_data[i][IDX_IS_ALT] != 0) retVal += 1;
        }
        return retVal;
    }

    function updateToolbar() {
        if (!$("#toolbar").data("dxToolbar")) return false;
        var toolbar = $("#toolbar").dxToolbar("instance");
        var selNode = getSelectedNode();
        var is_alt  = (selNode) && (selNode[IDX_IS_ALT] == 1);
        var is_terminal = (selNode) && (selNode[IDX_IS_TERMINAL] == 1);

        toolbar.beginUpdate();

        var button_convert = $("#btn_" + ACTION_CONVERT_HIERARCHY).dxButton("instance");
        var button_view    = $("#btn_" + ACTION_SWITCH_VIEW).dxSelectBox("instance");
        var button_show_alts= $("#btn_" + ACTION_SHOW_ALTERNATIVES).dxCheckBox("instance");
        //var lbl_show_alts = document.getElementById("lbl_show_alts");
        //var button_add_alts = $("#btn_" + ACTION_ADD_ALTERNATIVES).dxButton("instance");
        var button_edit_attrs = $("#btn_" + ACTION_EDIT_ATTRIBUTES).dxButton("instance");
        var button_edit = $("#btn_" + ACTION_EDIT_NODE).dxButton("instance");
        var button_delete = $("#btn_" + ACTION_DELETE_NODE).dxButton("instance");
        var button_delete_alt = $("#btn_delete_alt").dxButton("instance");
        var button_edit_alt_attributes = $("#btn_edit_attributes").dxButton("instance");
        var button_duplicate_alt = $("#btn_duplicate_alt").dxButton("instance");
        var btn_expand_all = $("#btn_expand_all").dxButton("instance");
        var btn_collapse_all = $("#btn_collapse_all").dxButton("instance");
        var btn_auto_redraw = $("#btn_auto_redraw").dxCheckBox("instance");
        var btn_multiselect = $("#btn_multiselect").dxCheckBox("instance");
        var btn_export = $("#btn_export").dxDropDownButton("instance");
        var repaintDropdown = false;

        var btn_priorities = $("#btn_priorities").dxSelectBox("instance");
        var lbl_show_priorities = document.getElementById("lbl_show_priorities");
        
        var btn_update = $("#btn_update").dxButton("instance");
        var btn_settings = $("#btn_settings").dxButton("instance");

        if ((button_convert)) button_convert.option("visible", false);
        if ((button_show_alts)) button_show_alts.option("visible", false);
        //if ((lbl_show_alts)) lbl_show_alts.style.display = "none";

        if ((button_convert)) button_convert.option("disabled", !is_complete_hierarchy);
        if ((button_view))    button_view.option("disabled", is_complete_hierarchy);

        if ((button_show_alts)) {
            button_show_alts.option("disabled", false);
            if (ActiveView == hvTree || alternativesCount() == 0) button_show_alts.option("disabled", true);
        }

        if ((btn_export) && btn_export.option("items")[3].visible !== (curPgID !== _PGID_STRUCTURE_ALTERNATIVES && ActiveView == hvGraph)) {
            btn_export.option("items")[3].visible = curPgID !== _PGID_STRUCTURE_ALTERNATIVES && ActiveView == hvGraph;
            repaintDropdown = true;
        }
        
        if ((ActiveView == hvGraph) && (button_view) && button_view.option("value")) button_view.option("value", false);
        
        if ((btn_settings)) btn_settings.option("visible", false);
        if ((button_delete)) button_delete.option("visible", false);
        if ((button_delete_alt)) button_delete_alt.option("visible", false);
        if ((button_duplicate_alt)) button_duplicate_alt.option("visible", false);
        if ((btn_auto_redraw)) btn_auto_redraw.option("visible", false);
        if ((btn_multiselect)) btn_multiselect.option("visible", false);
        //$("#lbl_auto_redraw").hide();
        //$("#lbl_multiselect").hide();

        if (curPgID == _PGID_STRUCTURE_ALTERNATIVES) { 
            if ((button_edit_attrs)) button_edit_attrs.option("visible", !isReadOnly);
            if ((button_convert)) button_convert.option("visible", false);
            if ((button_edit)) button_edit.option("visible", false);
            if ((btn_expand_all)) btn_expand_all.option("visible", false);
            if ((btn_collapse_all)) btn_collapse_all.option("visible", false);
            if ((btn_priorities)) btn_priorities.option("visible", false);
            if ((lbl_show_priorities)) lbl_show_priorities.style.display = "none";
            if ((btn_update)) btn_update.option("visible", false);
            if ((button_delete_alt)) button_delete_alt.option("visible", !isReadOnly);
            if ((button_duplicate_alt)) button_duplicate_alt.option("visible", !isReadOnly);
            //if ((btn_multiselect)) btn_multiselect.option("visible", !isReadOnly);
            <%--var btnEditInfodoc = document.getElementById("btnEditInfodoc");
            if ((btnEditInfodoc)) { btnEditInfodoc.disabled = (isReadOnly && <%=Bool2JS(Not PRJ.ProjectStatus = ecProjectStatus.psMasterProject)%>) || !(getSelectedNode()); }--%>
            if ((button_edit_alt_attributes)) { button_edit_alt_attributes.option("disabled", isReadOnly); }
            ///$("#lbl_multiselect").show();
        } else {
            if ((button_edit_attrs)) button_edit_attrs.option("visible", false);
            if ((button_edit)) button_edit.option("visible", !isReadOnly);
            if ((btn_expand_all)) btn_expand_all.option("visible", true);
            if ((btn_collapse_all)) btn_collapse_all.option("visible", true);
            if ((btn_priorities)) btn_priorities.option("visible", true);
            if ((lbl_show_priorities)) lbl_show_priorities.style.display = "";
            if ((btn_update)) btn_update.option("visible", true);
            if ((button_delete)) button_delete.option("visible", !isReadOnly);
            if (ActiveView == hvGraph) {
                if ((button_convert)) button_convert.option("visible", !isReadOnly);
                if ((button_show_alts)) button_show_alts.option("visible", true);
                //if ((lbl_show_alts)) lbl_show_alts.style.display = "";
                if ((btn_settings)) btn_settings.option("visible", true);
            } else {
                //if ((btn_multiselect)) btn_multiselect.option("visible", !isReadOnly);
                if ((btn_auto_redraw)) btn_auto_redraw.option("visible", true);
                //$("#lbl_auto_redraw").show();
                //$("#lbl_multiselect").show();
            }
        }

        $("#btn_sort").dxDropDownButton("instance").option("disabled", isReadOnly || !(curPgID == _PGID_STRUCTURE_ALTERNATIVES && alts_data.length || (curPgID !== _PGID_STRUCTURE_ALTERNATIVES && (!(selNode) || !is_terminal))));

        toolbar.endUpdate();

        if (repaintDropdown) {
            //btn_export.repaint();
            btn_export.getDataSource().reload();
        }
    }

    function updateLegend() {           
        var o = document.getElementById("legendObjsText");
        if ((o)) o.innerHTML = is_complete_hierarchy ? "Single parent node" : "<%=ResString("tblObjective")%>";
        var m = document.getElementById("legendMulti");
        if ((m)) m.style.display = (is_complete_hierarchy ? "inline" : "none");
        var a = document.getElementById("legendAlts");
        if ((a)) a.style.display = (is_show_alternatives ? "inline" : "none");
        var d = document.getElementById("divLegend");
        if ((d)) d.style.display = curPgID != _PGID_STRUCTURE_ALTERNATIVES && (ActiveView == hvGraph && (is_complete_hierarchy || is_show_alternatives)) ? "" : "none";
        
    }

    var MainSelectedNode = null;

    // calculate boxes coordinates and connecting lines
    function initGraph() {
        var levelsXCoord = [];

        var XCoord = 0;
        var YCoord = 0;
        XCoordDrag = 0;
        YCoordDrag = 0;
        drop_target = null;
        
        MainSelectedNode = getNodeByGUID(selectedNodeID);

        while (levelsXCoord.length < levelNodesCount.length) {
            levelsXCoord.push(0);
        }

        var nodes_data_len = nodes_data.length;

        for (var i = 0; i < nodes_data_len; i++) {
            if (nodes_data[i][IDX_COORDS].isVisible) {
                var isSelected = selectedNodeID == nodes_data[i][IDX_GUID];

                // X Coordinate
                var curLevel = nodes_data[i][IDX_LEVEL];
                var curWidth = levelNodeWidth[curLevel];

                if (levelsXCoord[curLevel] == 0) {
                    levelsXCoord[curLevel] = float2int(((canvas.width - curWidth * levelNodesCount[curLevel] - DefaultRectangleHorizontalMargin * levelNodesCount[curLevel]) / 2) + 4);
                } else {
                    levelsXCoord[curLevel] += curWidth + DefaultRectangleHorizontalMargin;
                }

                // Y Coordinate
                XCoord = levelsXCoord[curLevel];

                // Y Coordinate
                YCoord = curLevel * RectangleHeight + curLevel * DefaultRectangleVerticalMargin + 5;

                if (is_drag && isSelected) {
                    XCoordDrag = XCoord + cur_drag.x - start_drag.x;
                    YCoordDrag = YCoord + cur_drag.y - start_drag.y; // allow to drag vertically                
                }

                if (is_drag && !isSelected && (nodes_data[i][IDX_IS_ALT] == 0) && (nodes_data[i] != drag_source) && (XCoord<cur_drag.x) && (XCoord+curWidth>cur_drag.x) && (YCoord<cur_drag.y) && (YCoord+RectangleHeight>cur_drag.y) && canDropToNode(nodes_data[i])) {
                    drop_target = nodes_data[i];
                }

                // Auto-size the width
                var width = RectangleMaxWidth;
                //var hasParents = (nodes_data[i][IDX_PARENT_GUIDS].length == 1) && (nodes_data[i][IDX_PARENT_GUIDS][0] != nodes_data[0][IDX_GUID]); // node has more than 1 parent
                //if (!is_complete_hierarchy) hasParents = nodes_data[i][IDX_PARENT_GUIDS].length > 0; // if regular hierarchy - only Goal is higlighted with blue color
                var hasParents = (nodes_data[i][IDX_PARENT_GUIDS].length > 1);

                nodes_data[i][IDX_COORDS] = { top: YCoord, left: XCoord, bottom: YCoord + RectangleHeight, right: XCoord + curWidth, width: curWidth, height: RectangleHeight, hasParents: hasParents, isSelected: isSelected };                
                nodes_data[i][IDX_IS_EXPANDED] = localStorage.getItem("zTreeNode_<%=App.ProjectID%>_" + nodes_data[i][IDX_GUID]) !== "closed";
            }
        }        
    }

    function drawBoxes() {
        ctx.font = DEFAULT_FONT;
        var nodes_data_len = nodes_data.length;
        var selected_node;

        for (var i = 0; i < nodes_data_len; i++) {
            if (is_show_alternatives || nodes_data[i][IDX_IS_ALT] != 1) {
                var isSelected = selectedNodeID == nodes_data[i][IDX_GUID];
                if (isSelected) {
                    selected_node = nodes_data[i];
                } else {
                    drawBox(nodes_data[i]);
                }
            }
        }

        if ((selected_node)) {
            drawBox(selected_node, false); // draw the selected node last so that it would be on top of other nodes when dragging. Optimization - draw selected node in a separate canvas
            if (is_drag) drawBox(selected_node, true); //draw the draggable box (semi-transparent)
        }
    }

    function anyParentCollapsed(node) {
        if (is_complete_hierarchy) return false;

        var nodes_data_len = nodes_data.length;            
        var tmp = node;
        var isCollapsed = false;
        
        while ((!isCollapsed && !isGoal(tmp)) && (tmp[IDX_PARENT_GUIDS].length > 0)) {
            for (var i = 0; i < nodes_data_len; i++) {
                if (tmp[IDX_PARENT_GUIDS].length > 0 && nodes_data[i][IDX_GUID] == tmp[IDX_PARENT_GUIDS][0]) {
                    if (nodes_data[i][IDX_IS_EXPANDED] == 0) isCollapsed = true;
                    tmp = nodes_data[i];
                }
            }
        }

        return isCollapsed;
    }

    // draw the filled box (rectangle)
    function drawBox(node, isDraggable) {                           
        if (anyParentCollapsed(node)) return false;

        var v = node[IDX_COORDS];
        var is_alt = node[IDX_IS_ALT] == 1;

        var l = (!isDraggable ? v.left : XCoordDrag);
        var t = (!isDraggable ? v.top  : YCoordDrag);        

        // gradient fill for boxes
        if (isDraggable) {
            ctx.fillStyle = DRAG_COLOR;
        } else {
            if (typeof is_excanvas != 'undefined') { // IE           
                ctx.fillStyle = (is_alt ? ALT_BRUSH : (v.hasParents ? OBJ_BRUSH_SINGLE_PARENT : OBJ_BRUSH_MULTI_PARENT));
            } else { // Firefox or Chrome
                if (is_alt) {
                    ALT_BRUSH = ctx.createLinearGradient(l, t, l, t + v.height);
                    ALT_BRUSH.addColorStop(0, ALT_BRUSH_0);
                    ALT_BRUSH.addColorStop(1, ALT_BRUSH_1);
                    ctx.fillStyle = ALT_BRUSH;
                } else {
                    if (v.hasParents) {
                        OBJ_BRUSH_SINGLE_PARENT = ctx.createLinearGradient(l, t, l, t + v.height);
                        OBJ_BRUSH_SINGLE_PARENT.addColorStop(0, OBJ_BRUSH_SINGLE_0);
                        OBJ_BRUSH_SINGLE_PARENT.addColorStop(1, OBJ_BRUSH_SINGLE_1);
                        ctx.fillStyle = OBJ_BRUSH_SINGLE_PARENT;
                    } else {
                        OBJ_BRUSH_MULTI_PARENT = ctx.createLinearGradient(l, t, l, t + v.height);
                        OBJ_BRUSH_MULTI_PARENT.addColorStop(0, OBJ_BRUSH_MULTI_0);
                        OBJ_BRUSH_MULTI_PARENT.addColorStop(1, OBJ_BRUSH_MULTI_1);
                        ctx.fillStyle = OBJ_BRUSH_MULTI_PARENT;
                    }
                }
                // if drawing the drop target
                if ((drop_target) && drop_target[IDX_GUID] == node[IDX_GUID]) {
                    ctx.fillStyle = DROP_COLOR;
                }
                // if node is not enabled
                if (node[IDX_IS_ENABLED] != 1) {
                    ctx.fillStyle = DISABLED_FILL_COLOR;
                }
            }
        }
        
        if (!isDraggable) ctx.fillRect(l, t, v.width, v.height);              
        ctx.lineWidth = (v.isSelected ? SELECTED_STROKE_THICKNESS : STROKE_THICKNESS);
        
        if (node[IDX_IS_CATEGORY] == 1) ctx.setLineDash([6,1]);
        if (isDraggable) {
            ctx.strokeStyle = DRAG_STROKE_COLOR;
        } else {
            if ((drop_target) && drop_target[IDX_GUID] == node[IDX_GUID]) {
                ctx.strokeStyle = DROP_STROKE_COLOR;
            } else {
                ctx.strokeStyle = (v.isSelected ? (is_alt ? ALT_LINE_SELECTED_BRUSH : SELECTED_STROKE_COLOR) : STROKE_COLOR);        
            }
        }

        if (node[IDX_IS_ENABLED] != 1) {
            ctx.strokeStyle = DISABLED_STROKE_COLOR;
        }

        ctx.strokeRect(l, t, v.width, v.height);
        ctx.setLineDash([]);

        ctx.fillStyle = node[IDX_IS_ENABLED] == 1 ? (node[IDX_IS_CATEGORY] == 1 ? FONT_CATEGORY_COLOR : FONT_COLOR) : DISABLED_STROKE_COLOR;
        //wrapText(ctx, ShortString(node[IDX_NAME], Math.round(v.width / 2.5), false), v.left + 5, v.top + 17, v.width - 10, 16, true);
        if (!isDraggable) {
            //v.is_trimmed = rectText(ctx, node[IDX_NAME], node[IDX_IS_CATEGORY] == 1 ? DEFAULT_FONT_ITALIC : DEFAULT_FONT, l + 2, t, v.width, v.height, getTextHeight(node[IDX_IS_CATEGORY] == 1 ? DEFAULT_FONT_ITALIC : DEFAULT_FONT), 3);
            v.is_trimmed = rectText(ctx, (objectiveIndexVisible && node[IDX_ID] > 0 ? "[" + node[IDX_ID] + "] ": "") + node[IDX_NAME], DEFAULT_FONT, l + 2, t, v.width, v.height, getTextHeight(DEFAULT_FONT), 3);

            // show tooltip if necessary
            if ((display_priorities_mode > 0 || v.is_trimmed) && mouseOverNode.length != 0 && mouseOverNode[IDX_GUID] == node[IDX_GUID]) {
                tooltip_node = node;
            }

            // show expand/collapse controls
            //if (!is_alt && !is_complete_hierarchy && node[IDX_IS_TERMINAL] == 0 && mouseOverNode.length != 0 && mouseOverNode[IDX_GUID] == node[IDX_GUID]) {                
            if (!is_alt && !is_complete_hierarchy && node[IDX_IS_TERMINAL] == 0) {
                ctx.beginPath();
                ctx.strokeStyle = '#6e6e6e';
                ctx.fillStyle = '#eee';
                ctx.lineWidth = 1;                
                // approach with +/- in circle
                ctx.arc(l + v.width / 2, t + v.height + 10, 5, 0, 2 * Math.PI);
                ctx.fill();
                ctx.moveTo(l + v.width / 2 - 3, t + v.height + 10);
                ctx.lineTo(l + v.width /2 + 3, t + v.height + 10);
                if (!node[IDX_IS_EXPANDED]) {
                    ctx.moveTo(l + v.width / 2, t + v.height + 2);
                    ctx.lineTo(l + v.width /2, t + v.height + 5);
                    ctx.moveTo(l + v.width / 2, t + v.height + 7);
                    ctx.lineTo(l + v.width /2, t + v.height + 13);
                }

                // approach with </> signs
                //if (node[IDX_IS_EXPANDED] == 1) {
                //    ctx.moveTo(l + v.width - 14, t + v.height - 3);
                //    ctx.lineTo(l + v.width - 8, t + v.height - 8);
                //    ctx.lineTo(l + v.width - 2, t + v.height - 3);
                //} else {
                //    ctx.moveTo(l + v.width - 14, t + v.height - 8);
                //    ctx.lineTo(l + v.width - 8, t + v.height - 3);
                //    ctx.lineTo(l + v.width - 2, t + v.height - 8);
                //}
                ctx.stroke();
                
                ctx.closePath();
            }
        }        
    }

    // draw connecting lines between boxes
    function drawConnectingLines() {
        //ctx.lineWidth   = ARROW_THICKNESS;
        var selected_node;

        var nodes_data_len = nodes_data.length;
        for (var i = 1; i < nodes_data_len; i++) {
            if (is_show_alternatives || nodes_data[i][IDX_IS_ALT] != 1) {
                var isSelected = selectedNodeID == nodes_data[i][IDX_GUID];

                if (isSelected) {
                    selected_node = nodes_data[i];
                } else {
                    drawLinesToParent(nodes_data[i]);
                }            
            }
        }

        if ((selected_node)) drawLinesToParent(selected_node);
    }

    function drawLinesToParent(targetNode) {
        if (anyParentCollapsed(targetNode)) return false;        

        var t = targetNode[IDX_COORDS];
        var t_selected = targetNode[IDX_GUID] == selectedNodeID;

        var curLineWidth = (t_selected ? SELECTED_ARROW_THICKNESS : ARROW_THICKNESS);
        var curStrokeStyle = (targetNode[IDX_IS_ALT] == 0 ? (t_selected ? SELECTED_STROKE_COLOR : ARROW_COLOR) : (t_selected ? ALT_LINE_SELECTED_BRUSH : ALT_LINE_BRUSH));

        ctx.lineWidth = curLineWidth;
        ctx.strokeStyle = curStrokeStyle;
        
        var parent_nodes_count = targetNode[IDX_PARENT_GUIDS].length;
        var yCoordBus = 0.0;

        if (!OPT_LINE_STYLE_DIRECT && parent_nodes_count > 0) {
            if (typeof t.targetX == 'undefined' || t_selected) {
                var x = t.left + t.width / 2;
                var y0 = t.top; //  - 2;
                var y1 = y0 - (levelConnectedWithAllParents[targetNode[IDX_LEVEL]] ? OPT_VERT_LINE_PART_HEIGHT_FULL_CONTR : OPT_VERT_LINE_PART_HEIGHT);
                yCoordBus = y1;

                t.targetX = x;
                t.targetY = y1;

                ctx.beginPath();
                ctx.moveTo(x, y0);
                ctx.lineTo(x, y1);
                ctx.stroke();
                ctx.closePath();
            }
        }
        
        for (var j = 0; j < parent_nodes_count; j++) {
            var parentNode = getNodeByGUID(targetNode[IDX_PARENT_GUIDS][j]); // parent (up-level) node
            if ((parentNode)) {
                var p = parentNode[IDX_COORDS];
                var p_selected = parentNode[IDX_GUID] == selectedNodeID;

                //if (p_selected && targetNode[IDX_IS_ALT] == 1) {
                //    ctx.lineWidth = SELECTED_ARROW_THICKNESS;
                //    ctx.strokeStyle = ALT_LINE_SELECTED_BRUSH;
                //} else {
                //    ctx.lineWidth = curLineWidth;
                //    ctx.strokeStyle = curStrokeStyle;
                //}

                if (!OPT_LINE_STYLE_DIRECT) {
                    if (typeof p.sourceX == 'undefined' || t_selected) {
                        var x = p.left + p.width / 2;
                        var y0 = p.top + p.height;
                        //var y1 = y0 + (levelConnectedWithAllChildren[parentNode[IDX_LEVEL]] ? OPT_VERT_LINE_PART_HEIGHT_FULL_CONTR : OPT_VERT_LINE_PART_HEIGHT);
                        var y1 = (levelConnectedWithAllChildren[parentNode[IDX_LEVEL]] ? yCoordBus : y0 + OPT_VERT_LINE_PART_HEIGHT);
                        p.sourceX = x;
                        p.sourceY = y1;

                        ctx.beginPath();
                        ctx.moveTo(x, y0);
                        ctx.lineTo(x, y1);
                        ctx.stroke();
                        ctx.closePath();
                    }
                }

                ctx.beginPath();
                if (!OPT_LINE_STYLE_DIRECT) {
                    ctx.moveTo(p.sourceX, p.sourceY);
                    ctx.lineTo(t.targetX, t.targetY);
                } else {
                    ctx.moveTo(p.left + p.width / 2, p.bottom + (p_selected ? SELECTED_STROKE_THICKNESS : 2));
                    ctx.lineTo(t.left + t.width / 2, t.top - (t_selected ? SELECTED_STROKE_THICKNESS : 2));
                }
                ctx.stroke();
                ctx.closePath();
            }
        }
    }

    function drawTooltip() {
        if ((tooltip_node)) {
            var showPriorities = display_priorities_mode > 0;

            var v = tooltip_node[IDX_COORDS];
            var w = (v.width > 200 ? v.width + 20 : 200);
            ctx.fillStyle = FONT_COLOR;
            var n = wrapText(ctx, tooltip_node[IDX_NAME], v.left + 6, v.top + v.height + 12, w - 12, 14, false);                
            ctx.beginPath();
            ctx.lineWidth = 1;
            ctx.strokeStyle = '#777';
            //ctx.strokeRect(v.left, v.top + v.height + 8, w, 14 * n + 8);
            ctx.fillStyle = '#ffffdd';
            //ctx.fillRect(v.left, v.top + v.height + 8, w, 14 * n + 8);            
            ctx.moveTo(v.left, v.top + v.height + 8);
            ctx.lineTo(v.left + 15, v.top + v.height + 8);
            ctx.lineTo(v.left + 22, v.top + v.height + 3);
            ctx.lineTo(v.left + 29, v.top + v.height + 8);
            ctx.lineTo(v.left + w, v.top + v.height + 8);
            ctx.lineTo(v.left + w, v.top + v.height + 8 + 14 * (n  + (showPriorities ? 1 : 0)) + 8);
            ctx.lineTo(v.left, v.top + v.height + 8 + 14 * (n  + (showPriorities ? 1 : 0)) + 8);
            ctx.lineTo(v.left, v.top + v.height + 8);
            ctx.stroke();
            ctx.fill();
            ctx.closePath();
            ctx.fillStyle = FONT_COLOR;
            var n = wrapText(ctx, tooltip_node[IDX_NAME], v.left + 6, v.top + v.height + 12, w - 12, 14, false);

            if (showPriorities) {
                var p = tooltip_node[IDX_PRIORITIES_STRING]; //"[L:xxxxx, G:xxxxx]";
                ctx.fillText(p, v.left + ((w - canvasTextWidth(ctx, p)) / 2), v.top + v.height + (n + 1) * 14);
            }
        }
    }

    function getSelectedNode() {
        var node = getNodeByGUID(selectedNodeID);
        if ((node)) return node;
        return getAlternativeById(selectedNodeID);
    }

    function getLevelNodes(level) {
        var nodes_data_len = nodes_data.length;
        var res = [];
        for (var i = 0; i < nodes_data_len; i++) {
            if (nodes_data[i][IDX_LEVEL] == level) res.push(nodes_data[i]);
        }
        return res;
    }

    function getTerminalNodes() { // get all covering objectives
        var nodes_data_len = nodes_data.length;
        var res = [];
        for (var i = 0; i < nodes_data_len; i++) {
            if (nodes_data[i][IDX_IS_ALT] == 0 && nodes_data[i][IDX_IS_TERMINAL] == 1) res.push(nodes_data[i]);
        }
        return res;
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm(resString("msgUnsavedData"), on_agree + ";");
            return false;
        }
        return true;
    }

    function getNodeName(sName) {
        node_name = sName;        
        dxDialogBtnDisable(true, (node_name.trim()==""));
    }

    function getCheckedParentNodesGuids() {        
        node_parent_nodes = "";
        $('input:checkbox.cb_p_node:checked').each(function() { node_parent_nodes += (node_parent_nodes == "" ? "" : PARAM_DELIMITER) + this.getAttribute("id") });
    }

    function getNodeSets() {
        if (nodesets_list == null) {
            callAPI("workgroup/nodeset/?<% = _PARAM_ACTION %>=list", {"hid": <% = GetHID() %>}, onGetNodeSets);
        } else {
            <% If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then %>
                initNewNodeDlg(ACTION_ADD_FROM_DATASET_ALTS);
            <% Else %>
                initNewNodeDlg(ACTION_ADD_FROM_DATASET);
            <% End If%>
        }
    }

    function onGetNodeSets(res) {
        if (isValidReply(res)) {
            if (res.Result == _res_Success) {
                nodesets_list = res.Data;
            <% If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then %>
                initNewNodeDlg(ACTION_ADD_FROM_DATASET_ALTS);
            <% Else %>
                initNewNodeDlg(ACTION_ADD_FROM_DATASET);
            <% End If%>
                return true;
            }
        }
        showResMessage(res, true);
        return false;
    }

    var dlgAction = "";
    var dlg_init = true;

    function initNewNodeDlg(action) {        
        var selNode = getSelectedNode();
        var is_alt = curPgID == _PGID_STRUCTURE_ALTERNATIVES || ((selNode) && selNode[IDX_IS_ALT] == 1);
        var parentNodes = [];
        
        dlgAction = action;
        dlg_init = true; 

        switch (action) {
            case ACTION_ADD_LEVEL_BELOW :
            case ACTION_ADD_FROM_DATASET :
                parentNodes = getLevelNodes(selNode[IDX_LEVEL]);
                break;
            case ACTION_ADD_SAME_LEVEL :
                parentNodes = getLevelNodes(selNode[IDX_LEVEL] - 1);
                break;
            case ACTION_EDIT_NODE :
                parentNodes = (!is_alt ? getLevelNodes(selNode[IDX_LEVEL] - 1) : getTerminalNodes());
                break;
            case ACTION_ADD_ALTERNATIVES :
            case "add_alternatives_below" :
            case ACTION_ADD_FROM_DATASET_ALTS:
                <% If App.isRiskEnabled AndAlso App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood %>
                parentNodes = "";
                <% Else %>
                parentNodes = getTerminalNodes();
                <% End If%>
                break;
        }

        var lb = document.getElementById("lbParentNodes");
        lb.disabled = !is_alt;

        while (lb.firstChild) {
            lb.removeChild(lb.firstChild);
        }
                                
        var parent_nodes_len = parentNodes.length;
        for (var i=0; i<parent_nodes_len; i++) {            
            var opt = document.createElement("div");

            var p_node_name = ShortString(parentNodes[i][IDX_NAME], (parent_nodes_len>5 ? 37 : 40));
            
            //            opt.style.padding = "0px 0px 0px 0px";
            //            opt.style.margin  = "0px 0px 0px 0px";

            //            opt.value = parentNodes[i][IDX_ID];
            var chk = "";
            switch (action) {
                case ACTION_ADD_LEVEL_BELOW :
                case ACTION_ADD_FROM_DATASET :
                    chk = (parentNodes[i][IDX_GUID] == selectedNodeID ? chk = " checked" : " ");
                    break;
                case ACTION_ADD_SAME_LEVEL :
                    chk = (i == 0 ? chk = " checked" : " ");
                    break;
                case ACTION_EDIT_NODE :
                    chk = ($.inArray(parentNodes[i][IDX_GUID], selNode[IDX_PARENT_GUIDS]) >= 0 ? " checked " : " ");                  
                    break;
                case ACTION_ADD_ALTERNATIVES :
                case "add_alternatives_below" :
                case ACTION_ADD_FROM_DATASET_ALTS:
                    chk = " checked";
                    break;
            }

            // opt.innerHTML = "<table cellpadding='0' cellspacing='0' class='text' style='width:100%;'><tr><td align='left'><label><input type='checkbox' id='" + parentNodes[i][IDX_GUID] + "' class='cb_p_node'" + chk + " value='" + opt.value + "' onclick='verifyCheckboxes(this);' onchange='verifyCheckboxes(this);' onkeydown='verifyCheckboxes(this);'><span id='lbl_p_node_" + i + "'>" + p_node_name + "</span></label></td></tr></table>";
            opt.innerHTML = "<label><input type='checkbox' id='" + parentNodes[i][IDX_GUID] + "' class='cb_p_node'" + chk + " value='" + opt.value + "' onclick='verifyCheckboxes(this);' onchange='verifyCheckboxes(this);' onkeydown='verifyCheckboxes(this);' >" + p_node_name + "</label>";
            lb.appendChild(opt);
            lb.disabled = false;
        }     

        var sDisplay = (parent_nodes_len > 1 ? "inline-block" : "none");
        document.getElementById("divSelParents").style.display  = sDisplay;

        lb.style.border = (parent_nodes_len > 5 ? "1px solid #999999" : "");
        lb.style.height = (parent_nodes_len > 5 ? "120px" : "");
        lb.style.marginLeft = (parent_nodes_len > 5 ? "0px" : "-4px");

        // disable the checkbox if it is the only option
        if ($('input:checkbox.cb_p_node').length == 1) {         // total checkboxes count
            $('input:checkbox.cb_p_node').prop("disabled", "disabled");
        }

        on_hit_enter = "addNode("+action+");";
                document.getElementById("tbNodeName").value = "";
                document.getElementById("tbNodeName").rows = (action == ACTION_EDIT_NODE ? 5 : 15);
                document.getElementById("tbNodeName").style.height = (action == ACTION_EDIT_NODE ? 50 : 150) + "px";

                <%If App.isRiskEnabled AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
                $("#tdNodeType").hide();
                <%End If%>

                if (!is_alt && (action !== ACTION_ADD_ALTERNATIVES) && (action !== "add_alternatives_below")) {
                    $("#lblNodeName").html(action == ACTION_EDIT_NODE ? "<nobr><% = ResString("tblRAObjectiveName")%>:&nbsp;&nbsp;</nobr>" : "<nobr><% = ResString("tblRAObjectiveName")%>(s)<sup>*</sup>:&nbsp;&nbsp;</nobr>");
                    $("#lblParentNames").html("<nobr><% = ResString("lblParentObjectives")%>:&nbsp;&nbsp;</nobr>");
                    <%If App.isRiskEnabled AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
                    if (action == ACTION_EDIT_NODE) $("#tdNodeType").show();
                    $("#cbCategory").prop("checked", "");                    
                    <%End If%>
                } else {
                    $("#lblNodeName").html(action == ACTION_EDIT_NODE ? "<nobr><% = ResString("tblAlternativeName")%>:&nbsp;&nbsp;</nobr>" : "<nobr><% = ResString("tblAlternativeName")%>(s)<sup>*</sup>:&nbsp;&nbsp;</nobr>");
                    $("#lblParentNames").html("Contributions:&nbsp;");
                }
                if (action == ACTION_ADD_FROM_DATASET || action == ACTION_ADD_FROM_DATASET_ALTS){
                    $("#trSingleEdit").hide();
                    $("#trMultiEdit").hide();
                    $("#trAddFromDataset").show();

                    var tbl = (($("#divTreeNodes").data("dxTreeList"))) ? $("#divTreeNodes").dxTreeList("instance") : null;
                    if (tbl !== null) { tbl.dispose(); }

                    var w = 640;//dlgMaxWidth(950) - 200;
                    var h = 300;//dlgMaxHeight(300);

                    $("#divTreeNodes").dxTreeList({
                        autoExpandAll: true,
                        columns: [{
                            dataField: "Name",
                            caption: "<%If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then %><%= ResString("titleAlternativesEditor") %><%Else %><%= ResString("mnuHierarchyEditor") %><%End If %>",
                            width: Math.round(w*0.4)
                        }, {
                            dataField: "Infodoc",
                            caption: "Description",
                            cellTemplate: function(element, info) {
                                var result = $("<div title='" + htmlEscape(info.data.Infodoc) + "'>").text(info.data.Infodoc);
                                return result;
                            },
                            width: Math.round(w*0.59)
                        }],
                        dataSource: [],
                        height: function() { return Math.round($(window).height() * 0.8) - 90; },
                        keyExpr: "ID",
                        parentIdExpr: "ParentNodeID",
                        rootValue: -1,  
                        searchPanel: {
                            highlightCaseSensitive: false,
                            highlightSearchText: true,
                            placeholder: "Search...",
                            searchVisibleColumnsOnly: false,
                            visible: true,
                            width: 160
                        },
                        selection: {
                            mode: "multiple",
                            recursive: false
                        },
                        //onSelectionChanged: function (e) {
                        //    if (e.component._skipChildrenProcessing) return;
                        //    var currentSelectedRowKeys = e.currentSelectedRowKeys;
                        //    var currentDeselectedRowKeys = e.currentDeselectedRowKeys;
                        //    if (currentSelectedRowKeys.length >= 1 || currentDeselectedRowKeys.length >= 1) {
                        //        var isSelect;
                        //        var currentArray;
                        //        if(currentSelectedRowKeys.length){
                        //            currentArray = currentSelectedRowKeys;
                        //            isSelect = true;
                        //        } else {
                        //            currentArray = currentDeselectedRowKeys;
                        //        }
                        //        currentArray.forEach(function(value, i, array) {
                        //            var key = array[i];
                        //            changeNodeSelection(e.component, key, isSelect);
                        //        });
                        //    }
                        //},
                        onToolbarPreparing: function(e) {
                            var toolbarItems = e.toolbarOptions.items;
                            toolbarItems.splice(1, 0, {
                                text: 'Please select items to add:',
                                locateInMenu: 'never',
                                location: 'before'
                            });
                            toolbarItems.splice(0, 0, {
                                widget: 'dxButton', 
                                options: { 
                                    icon: 'fa fa-download', 
                                    text: "<%=JS_SafeString(ResString("btnDownload")) %>", 
                                    elementAttr: {"id": "btnDownloadNodeSet"},
                                    visible: true,
                                    disabled: false,
                                    onClick: function() {
                                        //showLoadingPanel();
                                        var obj = $("#btnDownloadNodeSet").prop("data-obj");
                                        if ((obj)) document.location.href= document.location.href + "&<% = _PARAM_ACTION %>=download_nodeset&id=" + encodeURIComponent(obj.Name);
                                    } 
                                },
                                locateInMenu: "auto",
                                showText: "always",
                                location: 'after'
                            });
                          },
                          headerFilter: {
                              allowSearch: true,
                              visible: true
                          },
                          allowColumnResizing: true,
                          showRowLines: true,
                          showBorders: false,
                          wordWrapEnabled: false,
                          width: function() { return Math.round($(window).width() * 0.8) - 310; },
                          hoverStateEnabled: true,
                      });

                    var lst = $("#divListSet").dxList({
                        dataSource: nodesets_list,
                        editEnabled: false,
                        showSelectionControls: false,
                        selectionMode: "single",
                        selectedItemKeys: null,
                        selectedItem: null,
                        keyExpr: "Name",
                        onSelectionChanged: function(e) {
                            if ((e.addedItems) && (e.addedItems.length)) {
                                var tree = $("#divTreeNodes").dxTreeList("instance");
                                tree.option("autoExpandAll", false);
                                tree.option("dataSource", e.addedItems[0].Nodes);
                                tree.clearSelection();
                                tree.option("autoExpandAll", true);
                                $("#btnDownloadNodeSet").prop("data-obj", e.addedItems[0]).dxButton("instance").option("disabled", !(e.addedItems[0].Nodes.length));
                            }
                        },
                        itemTemplate: function(data, index) {
                            var result = $("<div>").text(data.Name);
                            return result;
                        },
                        width: 250,
                    }).dxList("instance");

                    if (nodesets_list!=null && nodesets_list.length>0) {
                        lst.selectItem(nodesets_list[0]);
                    }
                    $("#divTreeNodes").find(".dx-toolbar").css("background", "transparent !important");
                    setTimeout(function() {
                        $("#divTreeNodes").find(".dx-item.dx-toolbar-item.dx-toolbar-label").css("max-width", "");
                    }, 550);
                      
                } else {
                    if (action == ACTION_EDIT_NODE) {
                        $("#trSingleEdit").show();
                        $("#trMultiEdit").hide();
                        $("#trAddFromDataset").hide();
                    } else {
                        $("#trSingleEdit").hide();
                        $("#trMultiEdit").show();
                        $("#trAddFromDataset").hide();
                        //$("#hintLabel").hide();

                        var tbl = (($("#divNodesGrid").data("dxDataGrid"))) ? $("#divNodesGrid").dxDataGrid("instance") : null;
                        if (tbl !== null) { tbl.dispose(); }

                        var ds = [{"name" : "", "is_cat" : false, "event_type" : 0, "description" : ""}];

                        var columns = [];
                        <%--columns.push({ "caption" : action == ACTION_ADD_ALTERNATIVES || action == "add_alternatives_below" ? "<% = ResString("tblAlternativeName")%>" : "<% = ResString("tblRAObjectiveName")%>", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "name", "allowEditing": true, "showEditorAlways" : true, "encodeHtml" : false});--%>
                        columns.push({ "caption" : action == ACTION_ADD_ALTERNATIVES || action == "add_alternatives_below" ? "<% = ResString("lblAlternatives")%>" : "<% = ResString("columnHeaderObjectives")%>", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "name", "allowEditing": true, "showEditorAlways" : true, "encodeHtml" : false});
                        <% If PM.IsRiskProject AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then %>
                        if (action == ACTION_ADD_LEVEL_BELOW || action == ACTION_ADD_SAME_LEVEL) columns.push({ "caption" : "Categorical", "alignment" : "center", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "is_cat", "dataType": "boolean", "allowEditing": true, "showEditorAlways" : true, "encodeHtml" : false});
                        <% End If %>
                        <% If PM.IsRiskProject AndAlso PRJ.isMixedModel OrElse PRJ.isMyRiskRewardModel Then %>
                        if (action == ACTION_ADD_ALTERNATIVES) {
                            columns.push({ "caption" : "<% = ResString("colEventType") %>", "alignment" : "center", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "event_type", "allowEditing": true, "showEditorAlways" : true, "encodeHtml" : false, width: 100});
                            columns[columns.length - 1].customizeText = function(cellInfo) {
                                return cellInfo.value == 0 ? "<% = ParseString("%%Risk%%") %>" : "<% = ParseString("%%Opportunity%%") %>";
                            };
                            columns[columns.length - 1].editCellTemplate = function (cellElement, cellInfo) {
                                $("<div></div>").dxSelectBox({
                                    displayExpr: "text",
                                    valueExpr: "key",
                                    items: [{"key" : 0, "text" : "<% = ParseString("%%Risk%%") %>"}, {"key" : 1, "text" : "<% = ParseString("%%Opportunity%%") %>"}],
                                    value: cellInfo.value * 1,
                                    onValueChanged: function(e) {
                                        if (cellInfo.data[cellInfo.column.dataField] != e.value) {
                                            cellInfo.component.option("value", e.value);
                                            cellInfo.data[cellInfo.column.dataField] = e.value;
                                            cellInfo.setValue(e.value);
                                        }
                                    }
                                }).appendTo(cellElement);                    
                            };
                        }
                        <% End If %>
                        columns.push({ "caption" : "Description", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "visible" : true, "dataField" : "description", "allowEditing": true, "showEditorAlways" : true, "encodeHtml" : false});

                          $("#divNodesGrid").dxDataGrid({
                              dataSource: ds,    
                              width: "98%",
                              height: "auto",
                              minHeight: 250,
                              columns: columns,
                              columnHidingEnabled: false,
                              allowColumnResizing: true,
                              cacheEnabled: true,
                              remoteOperations: false,
                              columnAutoWidth: true,
                              columnChooser: {
                                  mode: "select",
                                  enabled: false
                              },            
                              columnFixing: {
                                  enabled: false
                              },
                              columnMinWidth: 100,
                              paging: {
                                  enabled: false
                              },
                              searchPanel: {
                                  visible: false,
                                  width: 240,
                                  placeholder: resString("btnDoSearch") + "..."
                              },
                              sorting: {
                                  mode: "none"
                              },
                              editing: {
                                  mode: "cell",
                                  useIcons: true,
                                  refreshMode: "repaint",
                                  allowAdding: false,
                                  allowUpdating: true,
                                  allowDeleting: function (e) {
                                      return true;
                                  }
                              },
                              //onEditorPreparing: function (e) {
                              //    if (e.dataField === "description") e.editorName = "dxTextArea";
                              //},
                              onRowUpdated: function (e) {

                              },
                              
                              showBorders: false,
                              "export": {
                                  enabled: false
                              },
                              //keyExpr: "guid",
                              selection: {
                                  mode: "none", //is_multiselect ? "multiple" : "single",
                                  allowSelectAll: true,
                                  showCheckBoxesMode: "always"
                              },
                              onSelectionChanged: function (e) {
                
                              },
                              onContentReady: function (e) {
                                  if (dlg_init)
                                  setTimeout(function () { 
                                      var dg = $("#divNodesGrid").dxDataGrid("instance");
                                      dg.focus(); 
                                      dg.selectRows([0], false);
                                      var cell = dg.getCellElement(0, "name");
                                      dg.focus(cell);
                                      dg.editCell(0, "name"); 
                                  }, 500); // dxPopup animation is 400 ms
                                  dlg_init = false;
                              },
                              onCellClick: function (e) {
                
                              },
                              stateStoring: {
                                  enabled: false,
                                  type: "localStorage",
                                  storageKey: "NodesGrid_DxDatagrid_<%=App.ProjectID%>"
                              },
                              focusStateEnabled: true,
                              hoverStateEnabled: false,
                              noDataText: "<% =GetEmptyMessage()%>",
                              onRowPrepared: function (e) {
                                  if (e.rowType != "data") return;
                                  var row = e.rowElement;
                                  var data = e.data;                
                
                              },
                              loadPanel: {
                                  enabled: false,
                              },
                              onToolbarPreparing: onCreateGridToolbar,
                              onKeyDown: function (e) {
                                  if (e.event.originalEvent.keyCode == KEYCODE_TAB && !e.event.originalEvent.shiftKey && e.component.option("focusedColumnIndex") == 1) {
                                      setTimeout(function () {
                                          var r = e.component.option("focusedRowIndex");
                                          var c = e.component.option("focusedColumnIndex");
                                          if (e.component.hasEditData()) e.component.saveEditData();
                                          var ds = e.component.getDataSource();
                                          var ds_items = ds.items();
                                          if (c == 1 && r == ds_items.length - 1) {
                                              var data = ds_items;
                                              data.push({"name" : "", "is_cat" : false, "event_type" : 0, "description" : ""});
                                              e.component.option("dataSource", data);
                                              e.component.refresh(true);
                                              e.component.focus();
                                              setTimeout(function () { 
                                                  var cell = e.component.getCellElement(ds_items.length - 1, "name");
                                                  e.component.focus(cell);
                                                  e.component.editCell(ds_items.length - 1, "name"); 
                                              }, 50);
                                          }
                                      }, 50);
                                  }
                                  if (e.event.originalEvent.keyCode == KEYCODE_ENTER && !e.event.originalEvent.shiftKey) {
                                      setTimeout(function () {
                                         if (e.component.hasEditData()) e.component.saveEditData();
                                         var r = e.component.option("focusedRowIndex");
                                         var c = e.component.option("focusedColumnIndex");
                                         var ds = e.component.getDataSource();
                                         var ds_items = ds.items();
                                         if (r == ds_items.length - 1) {
                                             var data = ds_items;
                                             data.push({"name" : "", "is_cat" : false, "event_type" : 0, "description" : ""});
                                             e.component.option("dataSource", data);
                                             e.component.refresh(true);
                                         }
                                         setTimeout(function () { 
                                             var cell = e.component.getCellElement(r + 1, "name");
                                             e.component.focus(cell);
                                             e.component.editCell(r + 1, "name"); 
                                         }, 50);
                                      }, 50);
                                  }
                              }
                          });
                      }
                  }
                //document.getElementById("hintLabel").style.display = (action == ACTION_EDIT_NODE ? "none" : "");                
                document.getElementById("lblParentNames").style.display = "";
                if (action == ACTION_EDIT_NODE) {
                    var selNode = getSelectedNode();
                    document.getElementById("tbNodeName").value = selNode[IDX_NAME];                    
                    if (selNode[IDX_PARENT_GUIDS].length == 0 && !is_alt) { // Goal node
                        //document.getElementById("lbParentNodes").disabled = true;                                                
                        document.getElementById("lblParentNames").style.display = "none";
                    }
                    <%If App.isRiskEnabled AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
                    $("#cbCategory").prop("checked", selNode[IDX_IS_CATEGORY] == 1);
                    <%End If%>
                    
                    document.getElementById("tbNodeName").focus();
                }

                
                <%If Not IsCompleteHierarchyAllowed OrElse PM.IsRiskProject Then%>
                // hide parent selection UI in Beta
                var el;
                el = document.getElementById("lblParentNames"); if ((el)) el.style.display = "none";
                el = document.getElementById("lbParentNodes"); if ((el)) el.style.display = "none";
                el = document.getElementById("divSelParents"); if ((el)) el.style.display = "none";
                <%End If%>


        cancelled = false;    
        
        var title = action == ACTION_EDIT_NODE ? "Edit" : (action == ACTION_ADD_SAME_LEVEL ? "Add (Same Level)" : "Add (Level Below)");
        if (action == ACTION_ADD_FROM_DATASET || action == ACTION_ADD_FROM_DATASET_ALTS) title = "<% = ResString(If(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES, "cmnuAlternativesAddFromDataset", "cmnuObjectivesAddFromDataset")) %>";
        if (action == ACTION_ADD_ALTERNATIVES) title = "<% = ResString("btnAddAlternatives") %>";
        if (action == "add_alternatives_below") title = "<% = ResString("btnAddAlternativesBelow") %>";

        $("#divNewNode").dxPopup({
            width: action == ACTION_EDIT_NODE ? "auto" : function() { return Math.round($(window).width() * 0.8); },
            height: action == ACTION_EDIT_NODE ? "auto" : function() { return Math.round($(window).height() * 0.8); },
            visible: true,
            title: title,
            //onShowing: function() {
            //
            //},
            //onHiding: function() {
            //if ((!cancelled) && (node_name.trim() != "" || action != ACTION_EDIT_NODE)) {
            //    addNode(action); 
            //}
            //}
        });

        if ($("#btnAddNodeSave").data("dxButton")) $("#btnAddNodeSave").dxButton("instance").dispose();
        $("#btnAddNodeSave").dxButton({
            text: "<%=ResString("btnSave")%>",
            icon: "fas fa-check",
            width: 100,
            onClick: function() {
                on_hit_enter = "";
                if (action != ACTION_EDIT_NODE || document.getElementById("tbNodeName").value.trim() != "") { 
                    cancelled = false; 
                    addNode(action);
                    $("#divNewNode").dxPopup("hide");
                }
            }
        });
        
        if ($("#btnAddNodeCancel").data("dxButton")) $("#btnAddNodeCancel").dxButton("instance").dispose();
        $("#btnAddNodeCancel").dxButton({
            text: "<%=ResString("btnCancel")%>",
            icon: "fas fa-ban",
            width: 100,
            onClick: function() {
                cancelled = true;
                $("#divNewNode").dxPopup("hide");
            }
        });
    }


    function changeRiskNodeType(type) {
        var selNode = getSelectedNode();
        sendCommand('action=set_node_type&id=' + selNode[IDX_GUID] + '&is_cat=' + (type == <% = CInt(RiskNodeType.ntCategory) %>));
    }

    function onCreateGridToolbar(e) {
        var toolbarItemsGrid = e.toolbarOptions.items;

        toolbarItemsGrid.push({
            widget: 'dxButton', 
            options: { 
                icon: "fas fa-plus", 
                text: "Add a row", 
                hint: "Add a row", 
                onClick: function() { 
                    e.component.deselectAll();
                    if (e.component.hasEditData()) e.component.saveEditData();
                    var ds = e.component.getDataSource();
                    //e.component.beginUpdate();
                    var data = ds.items();
                    data.push({"name" : "", "is_cat" : false, "event_type" : 0, "description" : ""});
                    e.component.option("dataSource", data);
                    e.component.refresh(true);
                    //e.component.endUpdate();
                    setTimeout(function () { e.component.editCell(data.length - 1, "name"); }, 200);
                } 
            },
            locateInMenu: "auto",
            showText: "inMenu",
            location: 'after'
        });

        toolbarItemsGrid.push({
            widget: 'dxButton', 
            options: { 
                icon: "fas fa-paste", 
                text: resString("titlePasteFromClipboard"), 
                hint: resString("titlePasteFromClipboard"), 
                disabled: false,
                onClick: function() { 
                    var r = e.component.option("focusedRowIndex");
                    var c = e.component.option("focusedColumnIndex");
                    var ds = e.component.getDataSource();
                    pasteRows();
                } 
            },
            locateInMenu: "auto",
            showText: "inMenu",
            location: 'after'
        });
    }

    function pasteRows() {
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
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='70' rows='20'></textarea></pre>", "commitPasteFF();", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }
        }
    }

    function commitPasteFF() {
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteData(pasteBox.value);
        }
    }

    function pasteData(data) {
        var tbl = $("#divNodesGrid").dxDataGrid("instance");
        tbl.deselectAll();
        var ds = tbl.getDataSource();
        var items = ds.items();
        var tabChar = String.fromCharCode(9);

        var added_rows = 0;
        if ((data) && data != "") {
            var rows = data.split(String.fromCharCode(10));
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var rowPrefix = "";
                while (row.length > 0 && row[0] == tabChar) { rowPrefix += " "; row = row.substr(1); } // replace tabs at the beginning of the string with spaces
                row = rowPrefix + row;
                if ((row) && row != "") {
                    var cells = row.split(tabChar);
                    
                    var name = "";
                    var info = "";
                    
                    if (cells.length > 0) name = cells[0];
                    if (cells.length > 1) info = cells[1].trim();

                    if (name.trim() != "") {
                        while (items.length > 0 && items[items.length - 1]["name"].trim() == "" && items[items.length - 1]["description"].trim() == "" ) { items.splice(-1, 1);}
                        items.push( { "name" : name, "is_cat" : false, "description" : info, "event_type" : 0 } );
                        added_rows += 1;
                    }
                }
            }
            if (added_rows > 0) {
                tbl.option("dataSource", items);
                tbl.refresh(true);
            }
        } else { 
            DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
        }
    }

    function verifyCheckboxes(clicked_cb) { // at least one checkbox should be checked
        if (dlgAction != ACTION_ADD_ALTERNATIVES && dlgAction != "add_alternatives_below" && getSelectedNode()[IDX_IS_ALT] == 0 && !clicked_cb.checked) {
            if ($('input:checkbox.cb_p_node:checked').length == 0) { // checked checkboxes count
                clicked_cb.checked = true
            }
        }
    }

    function selectAllParents(is_checked) {  // check all checkboxes
        $('input:checkbox.cb_p_node').prop("checked", (is_checked ? "checked" : ""));
        if (dlgAction != ACTION_ADD_ALTERNATIVES && dlgAction != "add_alternatives_below" && !is_checked && (getSelectedNode()[IDX_IS_ALT] == 0)) $('input:checkbox.cb_p_node:first').prop("checked", "checked");
    }

    function addNode(action) {
        if ($("#divNodesGrid").data("dxDataGrid")) $("#divNodesGrid").dxDataGrid("instance").saveEditData();
        getCheckedParentNodesGuids();
        var id = "";
        if (action == ACTION_EDIT_NODE) {
            id = selectedNodeID;
            node_name = $("#tbNodeName").val();            
        }
        var upperNodeID = "";
        if (action == ACTION_ADD_SAME_LEVEL) {
            var selNode = getSelectedNode();
            if ((selNode)) {
                upperNodeID = "&upper_node_id=" + selNode[IDX_GUID];
                node_parent_nodes = selNode[IDX_PARENT_GUIDS][0];
            }
        }
        if (action == "add_alternatives_below") {
            var table_main = (($("#tableAlts").data("dxDataGrid"))) ? $("#tableAlts").dxDataGrid("instance") : null;
            if (table_main !== null && alts_data.length > 0 && table_main.option("focusedRowIndex") >= 0) {                 
                upperNodeID = "&upper_node_id=" + table_main.option("focusedRowKey");
            }            
        }
        var is_alt = curPgID == _PGID_STRUCTURE_ALTERNATIVES || ((getSelectedNode()) && getSelectedNode()[IDX_IS_ALT] == 1);
        if (action == ACTION_ADD_ALTERNATIVES) {
            is_show_alternatives = true; 
            if (ActiveView == hvGraph) {
                var toolbar = $("#toolbar").dxToolbar("instance");
                var button_show_alts = $("#btn_" + ACTION_SHOW_ALTERNATIVES).dxCheckBox("instance");
                if ((button_show_alts) && !(button_show_alts.option("value"))) {
                    is_manual = true;
                    button_show_alts.option("value", true);
                    setTimeout(function () { is_manual = false; }, 100);
                }
            }
        }

        if (action != ACTION_EDIT_NODE && action !== ACTION_ADD_FROM_DATASET && action !== ACTION_ADD_FROM_DATASET_ALTS) {
            node_name = "";
            if ($("#divNodesGrid").data("dxDataGrid")) {
                $("#divNodesGrid").blur();
                var tbl = $("#divNodesGrid").dxDataGrid("instance");
                tbl.deselectAll();
                if (tbl.hasEditData()) tbl.saveEditData();
                var ds = tbl.getDataSource();
                node_name = "";
                var ds_items = ds.items();
                for (var i = 0; i < ds_items.length; i++) {
                    if (ds_items[i]["name"] !== "") {
                        node_name += (node_name == "" ? "" : "\n") + ds_items[i]["name"] + "--extra--" + ds_items[i]["is_cat"] + "--extra--" + ds_items[i]["event_type"] + ((ds_items[i]["description"]) && ds_items[i]["description"] !== "" ? "\t" + ds_items[i]["description"] : "");
                    }
                }
            }
        } else {
            if (action == ACTION_ADD_FROM_DATASET || action == ACTION_ADD_FROM_DATASET_ALTS) {
                var tbl = (($("#divTreeNodes").data("dxTreeList"))) ? $("#divTreeNodes").dxTreeList("instance") : null;
                if (tbl !== null) {
                    if (tbl.hasEditData()) tbl.saveEditData();
                    var selRowKeys = tbl.getSelectedRowKeys("all");
                    var data = tbl.option("dataSource");
                    var node_name = "";
                    for(var i = 0;i < selRowKeys.length;i++){
                        var levelSpaces = "";
                        var node = data[selRowKeys[i]];
                        for (var s = 0; s < node.Level; s++) {
                            levelSpaces += " ";
                        }
                        node_name += (node_name == "" ? "" : "\n") + levelSpaces + node.Name + ((node.Infodoc) && node.Infodoc != "" ? "\t" + node.Infodoc : "");
                    }
                    if (action == ACTION_ADD_FROM_DATASET) {
                        action = ACTION_ADD_LEVEL_BELOW;
                    } else {
                        action = ACTION_ADD_ALTERNATIVES;
                    };
                    
                }
            }
        }

        var is_cat = "0";
        <%If App.isRiskEnabled AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
        is_cat = ($('#cbCategory').is(':checked') ? "1" : "0");
        <%End If%>


        sendCommand('action='+action+'&id='+id+'&parent_ids='+node_parent_nodes+'&is_alt='+is_alt+'&name='+encodeURIComponent(node_name)+'&is_cat='+is_cat + upperNodeID);
    }

    //function toggleSelectedNodeEnabledState() {
    //    var selNode = getSelectedNode();
    //    if (!isGoal(selNode)) { //can't disable Goal node
    //        selNode[IDX_IS_ENABLED] = !selNode[IDX_IS_ENABLED];
    //        sendCommand("action=" + ACTION_ENABLE_NODE + "&id=" + selNode[IDX_GUID] + "&is_alt=" + selNode[IDX_IS_ALT] + "&chk=" + selNode[IDX_IS_ENABLED]);
    //    }
    //}
    
    function deleteSelectedNode() {
        if (!is_multiselect || (ActiveView == hvGraph && curPgID !== _PGID_STRUCTURE_ALTERNATIVES)) {
            var selNode = getSelectedNode();
            if (!isGoal(selNode)) { //can't delete Goal node
                dxConfirm(resString("msgConfirmDeleteNode"), "sendCommand(\"action=" + ACTION_DELETE_NODE + "&id=" + selNode[IDX_GUID] + "&is_alt=" + selNode[IDX_IS_ALT] + "\");");
            }
        } else {
            var ids = checked_nodes;
            if (checked_nodes == "") {
                var selNode = getSelectedNode();
                ids = selNode[IDX_GUID];
            }
            dxConfirm(resString("msgConfirmDeleteNodes"), "sendCommand(\"action=" + ACTION_DELETE_NODE + "&ids=" + ids + "\");");
        }
    }

    function getSelectedAltsIDs() {
        if (curPgID == _PGID_STRUCTURE_ALTERNATIVES) {
            var tbl = $("#tableAlts").dxDataGrid("instance");
            var keys = tbl.option("selectedRowKeys");
            var retVal = "";
            for (var i = 0; i < keys.length; i++) {
                if (retVal != "") retVal += ",";
                retVal += keys[i];
            }        
            return retVal;
        } else {
            var selNode = getSelectedNode();
            if ((selNode)) return selNode[IDX_GUID];
        }
        return "";
    }

    function deleteSelectedAlts() {
        var selAlts = getSelectedAltsIDs();
        if (selAlts.length > 0) dxConfirm(resString("msgConfirmDeleteNodes"), "sendCommand(\"action=delete_alts&ids=" + selAlts + "\");");
    }

    function deleteJudgmentsForSelectedNode(isAlt, entirePlex) {
        var selNode = getSelectedNode();
        dxConfirm(resString("msgConfirmDeleteJudgmentsForNode"), "sendCommand(\"action=" + ACTION_DELETE_JUDGMENTS_FOR_NODE + "&id=" + selNode[IDX_GUID] + "&is_plex=" + entirePlex + "&is_alt=" + selNode[IDX_IS_ALT] + "\");");
    }

    function duplicateSelectedAlternative() {
        var selAlts = getSelectedAltsIDs();
        dxConfirm(resString("msgDuplicateJudgmentsForAllUsers"), "sendCommand(\"action=duplicate_alt&ids=" + selAlts + "&judgments=true\");", "sendCommand(\"action=duplicate_alt&ids=" + selAlts + "&judgments=false\");");
    }

    function initSearchLeft() {
        $("#tbSearchLeft").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
                    
            if (oldValue == "") return;

            setTimeout(function(){ if ($input.val() == "") onSearchLeft(""); }, 100);
        });
    }
    function initSearchRight() {
        $("#tbSearchRight").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
                    
            if (oldValue == "") return;

            setTimeout(function(){ if ($input.val() == "") onSearchRight(""); }, 100);
        });
    }

    function onSearchLeft(value) {
        value = value.toLowerCase();
        if (value == "") {
            $(".user_rb_lbl").show();
        } else {
            $(".user_rb_lbl").each(function (index, lbl) { 
                var attr = lbl.getAttribute("search");
                lbl.style.display = (attr) && (attr.indexOf(value) >= 0) ? "" : "none";
            });
        }
    }

    function onSearchRight(value) {
        value = value.toLowerCase();
        if (value == "") {
            $(".user_cb_lbl").show();
        } else {
            $(".user_cb_lbl").each(function (index, lbl) { 
                var attr = lbl.getAttribute("search");
                lbl.style.display = (attr) && (attr.indexOf(value) >= 0) ? "" : "none";
            });
        }
    }

    var copy_from;
    var copy_from_is_alt = false;

    function copyJudgmentsToLocation() {
        cancelled = false;
        dlg_copy_judgments = $("#divCopyJudgments").dialog({
            autoOpen: true,
            width: 450,
            height: "auto",
            maxHeight: mh,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: false,
            bgiframe: true,
            title: "Copy Judgments...",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [
                { id : "btnCopyJudgmentsOK", text : "OK", click: function() { dlg_copy_judgments.dialog( "close" ); }},
                { text : "Cancel", click: function() { cancelled = true; dlg_copy_judgments.dialog( "close" ); }}],
            open: function() { 
                var users_data_len = users_count();
                // init UI features
                initSearchLeft();
               
                // fill in users UI (left and right panes)
                var divLeftFrom = document.getElementById("divLeftFrom");
                if ((divLeftFrom)) {
                    divLeftFrom.innerHTML = "";
                    var sRadios = "";
                    for (var i = 0; i < users_data_len; i++) {
                        var u = users_data[i];
                        var uname = htmlEscape(u[IDX_USER_NAME]);
                        sRadios += "<div class='divRadio'><label class='user_rb_lbl' search='" + uname.toLowerCase() + "'><input type='radio' class='user_radio_btn' id='inputRadio" + i + "' idx='" + i + "' name='inputRadio' value='" + u[IDX_USER_AHP_USER_ID] + "' " + (i == activeUserID ? " checked='checked' ": "") + ">" + uname + "</label></div>"; 
                    }
                    divLeftFrom.innerHTML = sRadios;
                }               
                $(".ui-widget-content").css({border:0});
                $(".ui-dialog .ui-dialog-buttonpane").css({marginTop:0});
            },
            close: function() {
                // sendCommand
                if (!cancelled) {
                    var selNode = getSelectedNode();
                    copy_from = $('input.user_radio_btn:checked').val();
                    copy_from_is_alt = selNode[IDX_IS_ALT] == 1;
                    canPasteJudgments = true;
                    sendCommand("action=" + ACTION_COPY_JUDGMENTS_TO_LOCATION + "&from=" + copy_from + "&id=" + selNode[IDX_GUID] + "&is_alt=" + selNode[IDX_IS_ALT]);
                }
                dlg_copy_judgments = null;                  
            }
        });
        $.ui.dialog.prototype._focusTabbable = $.noop; // force the dialog not to auto-focus elements because it causes Copy function not working
    }

    function pasteJudgmentsFromLocation() {        
        cancelled = false;
        dlg_paste_judgments = $("#divPasteJudgments").dialog({
            autoOpen: true,
            width: 450,
            height: "auto",
            maxHeight: mh,
            modal: true,
            dialogClass: "no-close",
            closeOnEscape: false,
            bgiframe: true,
            title: "Paste Judgments...",
            position: { my: "center", at: "center", of: $("body"), within: $("body") },
            buttons: [
                { id : "btnPasteJudgmentsOK", text : "OK", click: function() { dlg_paste_judgments.dialog( "close" ); }},
                { text : "Cancel", click: function() { cancelled = true; dlg_paste_judgments.dialog( "close" ); }}],
            open: function() { 
                var users_data_len = users_count();
                // init UI features
                initSearchRight();

                var divSA = document.getElementById("divSelectAll");
                if ((divSA)) {
                    divSA.style.display = users_data_len > 1 ? "" : "none";
                }

                $("#btnPasteJudgmentsOK").prop("disabled", true);
                var selNode = getSelectedNode(); //'AS/11642i===
                var is_alt = selNode[IDX_IS_ALT] == 1; 
                if (is_alt) {                
                    $("#cbPWOnly").hide(); 
                    $("#lblPWOnly").hide();
                }
                else {
                    $("#cbPWOnly").show(); 
                    $("#lblPWOnly").show(); 
                }//'AS/11642i==

                // fill in users UI (left and right panes)
                var divLeftTo = document.getElementById("divLeftTo");
                if ((divLeftTo)) {
                    divLeftTo.innerHTML = "";
                    var sCBs = "";
                    for (var i = 0; i < users_data_len; i++) {
                        var u = users_data[i];
                        var uname = htmlEscape(u[IDX_USER_NAME]);
                        //sCBs += "<div class='divCheckbox' idx='" + i + "' " + (i == 0 ? " style='color:#ccc;' ": "") + "><label class='user_cb_lbl' search='" + uname.toLowerCase() + "'><input type='checkbox' class='user_cb_btn' id='inputCB" + i + "' idx='" + i + "' value='" + u[IDX_USER_AHP_USER_ID] + "' " + (copy_from == u[IDX_USER_AHP_USER_ID] ? " disabled='disabled' ": "") + " onclick='onUserCBClick();' >" + uname + "</label></div>"; 
                        sCBs += "<div class='divCheckbox' idx='" + i + "' " + (i == 0 ? " style='color:#ccc;' ": "") + "><label class='user_cb_lbl' search='" + uname.toLowerCase() + "'><input type='checkbox' class='user_cb_btn' id='inputCB" + i + "' idx='" + i + "' value='" + u[IDX_USER_AHP_USER_ID] + "' onclick='onUserCBClick();' >" + uname + "</label></div>"; 
                    }
                    divLeftTo.innerHTML = sCBs;
                }               

                // fill in alternatives UI (left and right panes)
                var divRightTo = document.getElementById("divRightTo");
                if ((divRightTo)) {
                    divRightTo.innerHTML = "";
                    var sCBs = "";
                    var selNodeGuid = selNode[IDX_GUID];
                    if (copy_from_is_alt) {
                        for (var i = 0; i < alts_data.length; i++) {
                            var u = alts_data[i];
                            var uname = htmlEscape(u["name"]);
                            sCBs += "<div class='divAltCheckbox' idx='" + i + "'><label class='alt_cb_lbl'><input type='checkbox' class='alt_cb_btn' id='inputAltCB" + i + "' idx='" + i + "' value='" + u["guid"] + "' onclick='onUserCBClick();' " + (u["guid"] == selNodeGuid ? "checked='checked'" : "") + " >" + uname + "</label></div>"; 
                        }
                    } else {
                        for (var i = 0; i < nodes_data.length; i++) {                            
                            var u = nodes_data[i];
                            var uname = htmlEscape(u[IDX_NAME]);
                            sCBs += "<div class='divAltCheckbox' idx='" + i + "' style='margin-left:" + (u[IDX_LEVEL] * 15) + "px;'><label class='alt_cb_lbl'><input type='checkbox' class='alt_cb_btn' id='inputAltCB" + i + "' idx='" + i + "' value='" + u[IDX_GUID] + "' onclick='onUserCBClick();' " + (u[IDX_GUID] == selNodeGuid ? "checked='checked'" : "") + " >" + uname + "</label></div>"; 
                        }
                    }
                    divRightTo.innerHTML = sCBs;
                }               
                $(".ui-widget-content").css({border:0});
                $(".ui-dialog .ui-dialog-buttonpane").css({marginTop:0});
            },
            close: function() {
                // sendCommand
                if (!cancelled) {
                    var uids = "";
                    $('input.user_cb_btn:checked:enabled').each(function(e) {
                        uids += (uids == "" ? "" : ",") + this.value;
                    });
                    var aids = "";
                    $('input.alt_cb_btn:checked:enabled').each(function(e) {
                        aids += (aids == "" ? "" : ",") + this.value;
                    });
                    if (uids != "" && aids != "") {
                        var selNode = getSelectedNode();
                        canPasteJudgments = false;
                        sendCommand("action=" + ACTION_PASTE_JUDGMENTS_FROM_LOCATION + "&to=" + uids + "&to_alts=" + aids + "&id=" + selNode[IDX_GUID] + "&is_alt=" + selNode[IDX_IS_ALT] + "&mode=" + $('input[name=rbMode]:checked').val() +"&pw_only=" + ($("#cbPWOnly").is(':checked') ? "1" : "0"));
                    }
                }
                dlg_paste_judgments = null;                  
            }
        });
        //if ($("#divCopyJudgments").height()>mh-150) $("#divCopyJudgments").height(mh-150);
        $.ui.dialog.prototype._focusTabbable = $.noop; // force the dialog not to auto-focus elements because it causes Copy function not working
    }

    function onUserCBClick() {// received_data
        var any_checked = $("input.user_cb_btn:checked").length > 0;
        var any_alt_checked = $("input.alt_cb_btn:checked").length > 0;
        $("#btnPasteJudgmentsOK").prop("disabled", !any_checked || !any_alt_checked);
    }

    function selectAllUsersInDlg(chk) {
        $("input.user_cb_btn:enabled").prop("checked", chk);
        onUserCBClick();
    }

    function selectAllAltsInDlg(chk) {
        $("input.alt_cb_btn:enabled").prop("checked", chk);
        onUserCBClick();
    }    

    //function foreachNodes(nodes, func) {
    //    for (var i = 0; i < nodes.length; i++) {
    //        func(nodes[i]);
    //        foreachNodes(nodes[i].children, func);
    //    }
    //}

    //function changeNodeSelection(component, key, isSelect) {
    //    var rowIndex = component.getRowIndexByKey(key);
    //    var row = component.getVisibleRows()[rowIndex];
    //    var childrenKeys = [];
    //    if (!row) return;
    //    foreachNodes(row.node.children, function (node) {
    //        childrenKeys.push(node.key);
    //    });
    //    (isSelect ? component.selectRows(childrenKeys, true) : component.deselectRows(childrenKeys)).done(function () {
    //        var node = row.node;
    //        var parentRowIndexes = [];
    //        while (node.parent) {
    //            node = node.parent;
    //            rowIndex = component.getRowIndexByKey(node.key);
    //            row = component.getVisibleRows()[rowIndex];
    //            if (row) {
    //                parentRowIndexes.push(rowIndex);
    //            }
    //        }
    //        if (parentRowIndexes.length) {
    //            component.repaintRows(parentRowIndexes);
    //        }
    //    });
    //}
    
    /* jQuery Ajax */
    function syncReceived(params) {
        if ((params)) {
            var rd = eval(params); // received_data
            if (rd[0] == ACTION_ADD_LEVEL_BELOW || rd[0] == ACTION_ADD_SAME_LEVEL || rd[0] == ACTION_EDIT_NODE || rd[0] == "set_node_type" || rd[0] == ACTION_DELETE_NODE || rd[0] == "delete_alts" || rd[0] == "duplicate_alt" || rd[0] == ACTION_SHOW_ALTERNATIVES || rd[0] == ACTION_ADD_ALTERNATIVES || rd[0] == "add_alternatives_below" || rd[0] == ACTION_MOVE_NODE || rd[0] == ACTION_MOVE_TREE_VIEW_NODE || rd[0].indexOf("sort_") == 0 || rd[0].indexOf("priorities_") == 0) {
            
                nodes_data = rd[1];
                tree_nodes_data = rd[2];
                alts_data = rd[3];

                menu_setOption(menu_option_alts, alts_data.length > 0);

                <%--<%If App.isRiskEnabled AndAlso App.ActiveProject.ProjectManager.ActiveHierarchy = ECHierarchyID.hidLikelihood %>
                if (rd[0] == ACTION_ADD_LEVEL_BELOW || rd[0] == ACTION_ADD_SAME_LEVEL || rd[0] == ACTION_DELETE_NODE || rd[0] == ACTION_CONVERT_HIERARCHY) {
                    var obj_count = 0;
                    for (var i = 0; i < nodes_data.length; i++) {
                        obj_count += (nodes_data[i][IDX_IS_ALT] == 1 ? 0: 1);
                    }
                    disable_contributions_ui_sl((obj_count > 1 ? 1 : 0));
                }
                <%End If%>--%>

                if (curPgID == _PGID_STRUCTURE_ALTERNATIVES)  { 
                    refreshGrid(); 
                } else {
                    if (ActiveView == hvGraph) { drawHierarchy(); }
                    if (ActiveView == hvTree) {
                        initTree(); 
                        //if (rd[0] !== ACTION_EDIT_NODE)  { 
                        //} else {
                        //    var node = zTreeGetNodeById(selectedNodeID);
                        //    if ((node)) {
                        //        node.name = getSelectedCVTreeNode().name;
                        //        zTreeObj.updateNode(node);
                        //    }
                        //}
                    }
                }

                if (rd[0] == ACTION_DELETE_NODE) setSelectedNode(nodes_data[0][IDX_GUID]);
                if (rd[0] == "delete_alts" && alts_data.length > 0) setSelectedNode(alts_data[0]["guid"]);
            }

            if (rd[0] == "refresh_full" || rd[0] == "sort_name_asc" || rd[0] == "sort_name_desc" || rd[0] == "sort_priority_asc" || rd[0] == "sort_priority_desc" || rd[0] == "obj_id_mode") {
                nodes_data = rd[1];
                tree_nodes_data = rd[2];
                alts_data = rd[3];

                if (curPgID == _PGID_STRUCTURE_ALTERNATIVES)  { 
                    //var grid = $('#tableAlts').dxDataGrid('instance'); 
                    //grid.state({});
                    //grid.dispose(); 
                    initAltsDatagrid();  
                    setTimeout(function() {
                        var grid = $("#tableAlts").dxDataGrid("instance");
                        grid.clearSorting();
                    }, 600);
                } else {
                    if (ActiveView == hvGraph) { drawHierarchy(); }
                    if (ActiveView == hvTree)  { initTree(); }
                }
            }

            if (rd[0] == "reorder_alts") {
                alts_data = rd[2];
                refreshGrid();
            }

            if (rd[0] == "event_id_mode") {
                alts_data = rd[1];
                alts_columns = rd[2];
                initAltsDatagrid();
            }
            
            if (rd[0] == ACTION_PASTE_JUDGMENTS_FROM_LOCATION) {
                var msg = rd[1];
                DevExpress.ui.notify(msg, "info");
            }

            if (rd[0] == ACTION_ENABLE_NODE) { // refresh            
                nodes_data = rd[1];
                tree_nodes_data = rd[2];
                alts_data = rd[3];

                if (ActiveView == hvGraph) { drawHierarchy(); }
                if (ActiveView == hvTree)  { initTree(); }
            }

            if (rd[0] == ACTION_EXPORT_OBJECTIVES || rd[0] == ACTION_EXPORT_ALTERNATIVES || rd[0] == ACTION_EXPORT_MODEL) {
                copyDataToClipboard(rd[1]);
            }

            if (rd[0] == ACTION_EXPORT_MODEL_FILE) {
                download(rd[1], "<%=SafeFileName(App.ActiveProject.ProjectName)%>.txt", "text/plain");
            }

            if (rd[0] == ACTION_CONVERT_HIERARCHY) {
                if (rd[1] == "") {
                    reloadPage(1); // reload the page with 1 second delay
                } else {
                    DevExpress.ui.notify(rd[1], "error");
                }
            }
        }        

        if (curPgID != _PGID_STRUCTURE_ALTERNATIVES && ActiveView == hvTree) {
            getIsCompleteHierarchy();
            if (is_complete_hierarchy) {
                ActiveView = hvGraph;
                initPage();
            }
        }

        if (rd[0] == "add_column" || rd[0] == "del_column" || rd[0] == "rename_column" || rd[0] == "set_default_value" || rd[0] == "attributes_reorder" || rd[0] == "add_item" || rd[0] == "del_item" || rd[0] == "add_item_and_assign" || rd[0] == "rename_item" || rd[0] == "paste_attribute_data") {
            alts_data = rd[1];
            alts_columns = rd[2];
            attr_data = rd[3];
            refreshGrid();
            if (rd[0] !== "paste_attribute_data" && rd[0] !== "add_item_and_assign") manageAttributes();
        }

        if (rd[0] == "set_color") {            
            var alt = getAlternativeById(rd[1]);
            if ((alt)) alt.color = rd[2];
            refreshGrid();
        }

        if (rd[0] == "add_column") {
            var v = attr_data[attr_data.length - 1];
            if (v[IDX_ATTR_TYPE] == avtEnumeration || v[IDX_ATTR_TYPE] == avtEnumerationMulti) {
                onEditAttributeValues(attr_data.length - 1, v[IDX_ATTR_TYPE]);
            } else {
                initAttributes();
            }
        }

        if (rd[0] == "set_default_value" || rd[0] == "add_item" || rd[0] == "del_item" || rd[0] == "rename_item") {
            var v = attr_data[SelectedAttrIndex];
            if (v[IDX_ATTR_TYPE] == avtEnumeration || v[IDX_ATTR_TYPE] == avtEnumerationMulti) {
                onEditAttributeValues(SelectedAttrIndex, v[IDX_ATTR_TYPE]);
            } else {
                initAttributes();
            }
        }

        updateToolbar();
        displayLoadingPanel(false);
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function sendCommand(params, please_wait) {
        cmd = params;
        if (typeof please_wait == "undefined" || please_wait) displayLoadingPanel(true);
        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    function refreshGrid() {
        if ($("#tableAlts").data("dxDataGrid")) {
            var dg = $("#tableAlts").dxDataGrid("instance");
            if ((dg)) {
                dg.beginUpdate();                
                dg.option("dataSource", alts_data.length > 0 ? alts_data : null);
                dg.endUpdate();
            }
        }
    }

    function LoadInfodoc(nodeid, nodeguid, is_alt) {
        var src = "";
        var ids = "";

        src = replaceString('%%%%', (is_alt ? <% =Cint(reObjectType.Alternative) %> : <% =Cint(reObjectType.Node) %>), '<% =PageURL(_PGID_EVALUATE_INFODOC, String.Format("type={0}&{1}={2}&guid={3}", "%%%%", _PARAM_ID, "' + nodeid + '", "' + nodeguid + '")) %><% =GetTempThemeURI(True) %>&r=' + Math.round(10000*Math.random()));
        ids = nodeguid;

        selInfodocIDs = ids;

        var f = document.getElementById("frmInfodoc");
        if ((f)) {
            if (src=="") {
                f.src = "";
            } else {
                if ((f.contentWindow) && (f.contentWindow.document) && (f.contentWindow.document.body)) f.contentWindow.document.body.innerHTML = "<div style='width:99%; height:99%; background: url(<%=ImagePath %>devex_loading.gif) no-repeat center center;'>&nbsp</div>";
                setTimeout(function () { document.getElementById("frmInfodoc").src = src; }, 30);
            }
        }
    }

    function EditInfodoc() {
        var ids = selInfodocIDs;
        if ((ids) && (ids!="")) {
            var node = getNodeByGUID(ids);
            if ((node)) {
                <%--CreatePopup('<% =PageURL(_PGID_RICHEDITOR) %>?type=' + ((node[IDX_IS_ALT] == 1) ? <% =Cint(reObjectType.Alternative) %> : <% =Cint(reObjectType.Node) %>) + '&field=infodoc&id=' + node[IDX_ID] + '&guid=' + node[IDX_GUID] + '&<% =_PARAM_TEMP_THEME+"="+_THEME_SL %>', 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);--%>
                OpenRichEditor('?project=<%=PRJ.ID%>&type=' + ((node[IDX_IS_ALT] == 1) ? <% =Cint(reObjectType.Alternative) %> : <% =Cint(reObjectType.Node) %>) + '&field=infodoc&id=' + node[IDX_ID] + '&guid=' + node[IDX_GUID]);
            }
        }
    }

    function onRichEditorRefresh(empty, infodoc, callback_param)
    {        
        window.focus();
        var f = document.getElementById("frmInfodoc");
        if ((f)) {
            var node = getNodeByGUID(selInfodocIDs);
            if ((node)) {
                node[IDX_INFODOC] = !(empty == 1 || empty == true);
                LoadInfodoc(node[IDX_ID], node[IDX_GUID], node[IDX_IS_ALT] == 1);
            }
            var alt = getAlternativeById(selInfodocIDs);
            if ((alt)) {
                alt[IDX_ALT_INFODOC] = !(empty == 1 || empty == true) ? "<% = ResString("lblYes")%>" : "";
                LoadInfodoc(alt["iid"], alt["guid"], true);
            }
            $("#imgTreeInfodoc" + replaceString("-", "", selInfodocIDs)).removeClass("toolbar-icon").removeClass("toolbar-icon-disabled").addClass(empty == 1 || empty == true ? "toolbar-icon-disabled" : "toolbar-icon");
        }
    }              

    function onSetView(value) {
        ActiveView = value;
        sendCommand('action=' + ACTION_SWITCH_VIEW + '&value=' + ActiveView);
        initPage();
    }

    function onClickToolbar(btn_id, value) {
        switch (btn_id + "") {
            case ACTION_ADD_LEVEL_BELOW:
            case ACTION_ADD_SAME_LEVEL:
            case ACTION_ADD_ALTERNATIVES:
            case "add_alternatives_below":
            case ACTION_EDIT_NODE:                    
                initNewNodeDlg(btn_id+"");
                break;
            case ACTION_ADD_FROM_DATASET: //SL16152
                getNodeSets();
                break;
            case ACTION_DELETE_NODE:
                deleteSelectedNode();
                break;
            case "delete_alt":
                deleteSelectedAlts();
                break;
            case "duplicate_alt":
                duplicateSelectedAlternative();
                break;
            case ACTION_CONVERT_HIERARCHY:
                //var convertFunc = "sendCommand(\"action=" + ACTION_CONVERT_HIERARCHY + "\");"
                
                var convertFunc = "slConvertHierarchy(document.getElementById('tbConvertedProjectName').value.trim(), (document.getElementById('cbCopyJudgments').checked));";
                dxDialog("<span class='note'><%=ResString("msgSureConvertHierarchyToRegular") %></span><br><br><%=ResString("lblProjectName")%>:&nbsp;<input type='text' class='input text' id='tbConvertedProjectName' style='width:80%;' value='<%=JS_SafeString(App.ActiveProject.ProjectName)%>'><br><label><input type='checkbox' id='cbCopyJudgments' value='1' checked><% = JS_SafeString(ResString("lblCHCopyJudgments")) %></label>", convertFunc, ";", "<%=ResString("titleConfirmation") %>");
                document.getElementById("tbConvertedProjectName").value += " (<% = JS_SafeString(ResString("lblIncompleteHierarchy")) %>)";
                break;
            case ACTION_SHOW_ALTERNATIVES:
                is_show_alternatives = !is_show_alternatives;
                callShowAlternatives();
                break;
                //case ACTION_SWITCH_VIEW:
                //    ActiveView = (button.get_checked() ? 1 : 0);
                //    sendCommand('action=' + ACTION_SWITCH_VIEW + '&value=' + ActiveView);
                //    initPage();
                //    break;
            case ACTION_EXPORT_OBJECTIVES:
            case ACTION_EXPORT_ALTERNATIVES:
            case ACTION_EXPORT_MODEL:
            case ACTION_EXPORT_MODEL_FILE:
                sendCommand('action=' + btn_id);
                break;
            case "sort_name_asc":
            case "sort_name_desc":
            case "sort_priority_asc":
            case "sort_priority_desc":
                var selNode = getSelectedNode();
                var is_alt  = curPgID == _PGID_STRUCTURE_ALTERNATIVES || ((selNode) && (selNode[IDX_IS_ALT] == 1));
                var is_terminal = curPgID != _PGID_STRUCTURE_ALTERNATIVES && (selNode) && (selNode[IDX_IS_TERMINAL] == 1);
                var parent_guid = curPgID == _PGID_STRUCTURE_ALTERNATIVES ? "" : selNode[IDX_GUID];
                dxConfirm(curPgID == _PGID_STRUCTURE_ALTERNATIVES ? resString("msgConfirmSortAlternatives") : resString("msgConfirmSortNodesBelow") + " \"" + selNode[IDX_NAME] + "\"?", "sendCommand(\"action=" + btn_id + "&id=" + parent_guid + "&is_alt=" + is_alt + "\");");
                break;
            case ACTION_EDIT_ATTRIBUTES:
                manageAttributes();
                break;
            case "expand_all":
                expandAllNodes(true);
                if (ActiveView == hvTree) {
                    zTreeObj.expandAll(true);
                }
                if (ActiveView == hvGraph) { 
                    drawHierarchy();
                }
                break;
            case "collapse_all":
                expandAllNodes(false);
                if (ActiveView == hvTree) {
                    zTreeObj.expandAll(false);
                    zTreeObj.expandNode(zTreeObj.getNodes()[0], true, false);
                }
                if (ActiveView == hvGraph) { 
                    drawHierarchy();
                }
                break;
            case "multiselect_switch":
                is_multiselect = !is_multiselect;
                zTreeSetting.check.enable = is_multiselect;
                sendCommand('action=multiselect_switch&val=' + is_multiselect, false);
                initPage();
                break;
            case "auto_redraw":
                is_autoredraw = !is_autoredraw;
                if (is_autoredraw) {
                    var nodes = zTreeObj.getNodes();
                    zTreeObj.expandAll(false);
                    zTreeObj.expandNode(nodes[0], true, false);
                } else {
                    zTreeObj.expandAll(true);
                }
                break;
        }
    }

    function slConvertHierarchy(newProjectName, fCopyJudgments) {
        sendCommand('action=' + ACTION_CONVERT_HIERARCHY + '&prj_name=' + newProjectName + '&judgments' + ((fCopyJudgments) ? 1 : 0));
    }

    //function resetGridState() {
    //    sessionStorage.removeItem("StructureAlts_Datagrid");
    //    $("#tableAlts").dxDataGrid("instance").state({});
    //}

    function expandAllNodes(chk) {
        var nodes_data_len = nodes_data.length;
        for (var i = 0; i < nodes_data_len; i++) {
            nodes_data[i][IDX_IS_EXPANDED] = chk ? 1 : 0;
            localStorage.setItem("zTreeNode_<%=App.ProjectID%>_" + nodes_data[i][IDX_GUID], nodes_data[i][IDX_IS_EXPANDED] == 1 ? "open" : "closed");
        }
        if (!chk) {
            nodes_data[0][IDX_IS_EXPANDED] = 1;
            localStorage.setItem("zTreeNode_<%=App.ProjectID%>_" + nodes_data[0][IDX_GUID], nodes_data[0][IDX_IS_EXPANDED] == 1 ? "open" : "closed");
        }
    }

    function onChangeSettings(value) {
        var setting = "";
        switch (value * 1) {
            case 0:
                setting = "priorities_none";
                display_priorities_mode = 0;
                sendCommand('action=' + setting);
                break;
            case 1:
                setting = "priorities_local";
                display_priorities_mode = 1;
                sendCommand('action=' + setting);
                break;
            case 2:
                setting = "priorities_global";
                display_priorities_mode = 2;
                sendCommand('action=' + setting);
                break;
            case 3: 
                setting = "priorities_both";
                display_priorities_mode = 3;
                sendCommand('action=' + setting);
                break;
            case -1: 
                setting = "priorities_update";
                sendCommand('action=' + setting);
                break;
        }
    }

    /* Settings Dialog */
    var popupSettings = null, 
        popupOptions = {
            animation: null,
            width: 610,
            height: "auto",
            contentTemplate: function() {
                return  $("<div class='dx-fieldset'></div>").append(
                    <%--$("<div class='dx-fieldset-header'><%=ParseString("%%Alternatives%% Display")%></div>"),--%>
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Rectangle Height [<span id='tbHValue'>" + RectangleHeight + "</span>]</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='tbRectHeight' style='width:350px;'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div class='dx-field-label'>Rectangle Width [<span id='tbWValue0'>" + RectangleMinWidth + "</span> .. <span id='tbWValue1'>" + RectangleMaxWidth + "</span>]</div>"),
                            $("<div class='dx-field-value'></div>").append(
                                $("<div id='tbRectWidth' style='width:350px;'></div>"))),
                        $("<div class='dx-field'></div>").append(
                            $("<div id='btnRectReset'></div>")
                        ),
                    $("<br>"),
                    $("<center>").append(
                        $("<div id='btnSettingsClose' style='margin: 5px; '></div>"),
                        $("<div id='btnSettingsCancel' style='margin: 5px; '></div>")
                    )
                );
            },
            showTitle: true,
            title: "Preferences",
            dragEnabled: true,
            closeOnOutsideClick: false
        };

    function settingsDialog() {
        if (popupSettings) $(".popupSettings").remove();
        var $popupContainer = $("<div></div>").addClass("popupSettings").appendTo($("#popupSettings"));
        popupSettings = $popupContainer.dxPopup(popupOptions).dxPopup("instance");
        popupSettings.show();
        
        $("#btnSettingsClose").dxButton({
            text: "Save",
            icon: "fas fa-check",
            onClick: function() {
                popupSettings.hide();
                saveSettings();
            }
        });
        $("#btnSettingsCancel").dxButton({
            text: "Cancel",
            icon: "fas fa-ban",
            onClick: function() {
                popupSettings.hide();
            }
        });

        $("#tbRectHeight").dxSlider({
            height: 50,
            keyStep: 1,
            label: { visible : true },
            max: 200,
            min: 20,
            onValueChanged: function (e) { document.getElementById("tbHValue").innerHTML = e.value; },
            showRange: true,
            step: 1,
            tooltip: {},
            value: RectangleHeight,
            visible: true,
            width: 350
        });
        $("#tbRectWidth").dxRangeSlider({
            height: 50,
            keyStep: 1,
            label: { visible : true },
            max: 500,
            min: 20,
            onValueChanged: function (e) { 
                document.getElementById("tbWValue0").innerHTML = e.start; 
                document.getElementById("tbWValue1").innerHTML = e.end;
            },
            showRange: true,
            step: 1,
            showRange: true,
            tooltip: {},
            start: RectangleMinWidth,
            end: RectangleMaxWidth,
            visible: true,
            width: 350
        });
        $("#btnRectReset").dxButton({
            text: "Reset To Defaults",
            onClick: function (e) { 
                $("#tbRectHeight").dxSlider("instance").option("value", DefaultRectangleHeight);
                $("#tbRectWidth").dxRangeSlider("instance").option("start", DefaultRectangleMinWidth);
                $("#tbRectWidth").dxRangeSlider("instance").option("end", DefaultRectangleMaxWidth);
            }
        });
    }

    function saveSettings() {
        RectangleHeight = Math.round($("#tbRectHeight").dxSlider("instance").option("value") * 1);
        RectangleMinWidth = Math.round($("#tbRectWidth").dxRangeSlider("instance").option("start") * 1);
        RectangleMaxWidth = Math.round($("#tbRectWidth").dxRangeSlider("instance").option("end") * 1);
        sendCommand("action=save_settings&rect_height=" + RectangleHeight + "&min_rect_width=" + RectangleMinWidth + "&max_rect_width=" + RectangleMaxWidth);
        if (ActiveView == hvGraph) { drawHierarchy(); }
    }
    /* end Settings Dialog */

    function callShowAlternatives() {
        sendCommand('action=' + ACTION_SHOW_ALTERNATIVES + '&value=' + is_show_alternatives, true);
    }

    /* Tree View */
    function onTreeNodeClick(event, treeId, treeNode){
        if ((treeNode)) {
            setSelectedNode(treeNode.id, treeNode);
            if (is_autoredraw && (treeNode.children) && treeNode.children.length) {
                zTreeObj.expandAll(false);
                tmpNode = treeNode;
                setTimeout(function () { zTreeObj.expandNode(tmpNode, true, false); }, 100);
            }
            //setNodeName(treeNode.title);
            //sendCommand("action=selected_node&id=" + treeNode.id);
            //alert(treeNode.tId + ", " + treeNode.name);
        }
    }

    var tmpNode;
    
    function onTreeNodeDblClick(event, treeId, treeNode){
        if (isReadOnly) return;
        if ((treeNode)) {
            setSelectedNode(treeNode.id, treeNode);
            var nodes = zTreeObj.getNodes();                
            setSelectedTreeNode(nodes);
            initNewNodeDlg(ACTION_EDIT_NODE);
        }
    }
    
    function onTreeNodeRightClick(event, treeId, treeNode){
        if (isReadOnly) return;
        if ((treeNode)) {
            setSelectedNode(treeNode.id, treeNode);
            var nodes = zTreeObj.getNodes();                
            setSelectedTreeNode(nodes);
            menu_pos.x = menu_pos.y = 0;            
            if ((event.pageX)) {       // FF
                menu_pos.x = event.pageX;
                menu_pos.y = event.pageY;
            } else {
                if ((event.clientX)) { // IE
                    menu_pos.x = event.clientX - 3; 
                    menu_pos.y = event.clientY - 43;
                }
            }
            showMenu(menu_pos.x, menu_pos.y);
        }
    }

    function onBeforeTreeNodeClick(event, treeId, treeNode){
        if ((treeNode.disabled)) return false;
    }

    function beforeDrag(treeId, treeNodes) {
        if (isReadOnly) return false;

        for (var i=0,l=treeNodes.length; i<l; i++) {
            if (treeNodes[i].drag === false) {
                return false;
            }
        }
        return true;
    }
    function beforeDrop(treeId, treeNodes, targetNode, moveType) {        
        if (isReadOnly) return false;

        if ((targetNode) && (treeNodes.length > 0)) {
            if (((targetNode.id == "<%=PM.Hierarchy(PM.ActiveHierarchy).Nodes(0).NodeGuidID.ToString%>") && (moveType == "prev" || moveType == "next")) || (targetNode.id == treeNodes[0].id)) {
                return false;
            }
        }
        return targetNode ? targetNode.drop !== false : true;
    } 

    var i_move_type = 2;
    var sourceid = "";
    var targetid = "";    

    function onTreeNodeDrop(event, treeId, treeNodes, targetNode, moveType, isCopy) {
        if (isReadOnly) return;

        if ((moveType) && (treeNodes) && (treeNodes.length == 1)) {
            i_move_type = (moveType == "prev" ? 1 : (moveType == "inner" ? 2 : 4));
            sourceid = treeNodes[0].id;
            targetid = targetNode ? targetNode.id : "";
            //$("#tree-node-menu").dxContextMenu("instance").show();            
            dxDialog(/*resString("lblSelectAction") +*/"<div style='min-width: 350px; min-height: 90px;'><div id='cbAction' style='margin: 5px;'></div><br><div id='cbCopyJudgments' style='margin: 5px;'></div></div>", "proceedMoveTreeNode();", "sendCommand('action=refresh_full');", "Confirmation");
            $("#cbAction").dxRadioGroup({
                dataSource: [ "Move", "Copy" ],
                value: "Move",
                searchEnabled: false,
                onValueChanged: function (e) {
                    if ($("#cbCopyJudgments").data("dxCheckBox")) {
                        var cb = $("#cbCopyJudgments").dxCheckBox("instance");
                        cb.option("visible", e.value == "Copy");
                    }
                }
            });
            $("#cbCopyJudgments").dxCheckBox({
                text: resString("lblDuplicateJudgments"),
                visible: false,
                value: true
            });
            $(".dx-texteditor-input")[0].select();
        }
    }

    function proceedMoveTreeNode() {
        var node_action = $("#cbAction").dxRadioGroup("instance").option("value");
        var copy_judgments = $("#cbCopyJudgments").dxCheckBox("instance").option("value");
        sendCommand("action=" + ACTION_MOVE_TREE_VIEW_NODE + "&source=" + sourceid + "&target=" + targetid + "&move_type=" + i_move_type + "&node_action=" + node_action + "&copy_judgments=" + copy_judgments);
    }
   
    var zTreeSetting = {
        callback: {
            beforeClick: onBeforeTreeNodeClick,
            onClick: onTreeNodeClick,            
            onDblClick: onTreeNodeDblClick,
            onRightClick: onTreeNodeRightClick,
            beforeDrag: beforeDrag,
            beforeDrop: beforeDrop,
            onDrop: onTreeNodeDrop,
            onExpand: function (event, treeId, treeNode) { 
                localStorage.setItem("zTreeNode_<%=App.ProjectID%>_" + treeNode.id, "open"); 
            },
            onCollapse: function (event, treeId, treeNode) { 
                localStorage.setItem("zTreeNode_<%=App.ProjectID%>_" + treeNode.id, "closed"); 
            },
            onCheck: onCheck
        },
        view: {
            showIcon: false,
            showTitle: true,
            fontCss: getFont,
            nameIsHTML: true,
            dblClickExpand: false,
            selectedMulti: false,
            expandSpeed: ""
        }, 
        edit: { 
            enable: true,
            showRemoveBtn: false,
            showRenameBtn: false,
            drag: { isCopy : true, isMove : false, autoExpandTrigger: true } },
        check: {
            enable: is_multiselect,
            chkStyle: "checkbox",
            chkboxType: { "Y": "s", "N": "s" } }
    };
   
    function getFont(treeId, node) {
        return node.font ? node.font : {};
    } 

    function initTree() {
        if ((zTreeObj)) $.fn.zTree.destroy("ulHierarchyTree");
        synchTreeExpanded(tree_nodes_data);
        zTreeObj = $.fn.zTree.init($("#ulHierarchyTree"), zTreeSetting, tree_nodes_data);
        $('.ztree_tnn').css({ 'color':'black', 'font-weight':'normal' }); 
        //$('.ztree_cat').css({ 'color':'blue', 'border': '2px dashed grey', 'padding':'0px 5px' });
        //$('.ztree_cat').css({ 'color':'blue', 'font-style':'italic', 'padding':'0px 5px' });
        $('.ztree_node_disabled').css({ 'color':'#ccc', 'font-weight':'normal' });
        $('.znode_span').each(function () { this.parentNode.parentNode.title = this.innerHTML });
        var nodes = zTreeObj.getNodes();
        setSelectedTreeNode(nodes);        
    }

    function setSelectedTreeNode(nodes) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i=0; i<nodes_len; i++) {
                if (nodes[i].id == selectedNodeID) {
                    zTreeObj.selectNode(nodes[i], false, true);
                    setSelectedNode(nodes[i].id);
                    i = nodes.len;
                } else {
                    setSelectedTreeNode(nodes[i].children);
                }
            }
        }
    }

    function resizeHierarchyTree() {
        if(ActiveView==hvTree){
            $("#ulHierarchyTree").height(100);
            $("#divHierarchyTree").height(100); //.width(100);
            var h = $("#tdContent").innerHeight();
            if (h > 100) {
                $("#divHierarchyTree").height(h); //.width($("#tdContent").width())
                $("#ulHierarchyTree").height(h);
            }
        }
    }

    var checked_nodes = "";

    function onCheck(event, treeId, treeNode) {        
        var nodes = zTreeObj.getNodes();                
        checked_nodes = "";
        getCheckedCVTreeNode(nodes);
        
        <%--var toolbar = $find("<%= RadToolBarMain.ClientID %>");--%>
        var toolbar = $("#toolbar").dxToolbar("instance");
        var button_delete = $("#btn_" + ACTION_DELETE_NODE).dxButton("instance");
        button_delete.option("disabled", true);
        if (checked_nodes.length > 0 && !(checked_nodes.length == nodes_data[0][IDX_GUID].length && checked_nodes.indexOf(nodes_data[0][IDX_GUID]) == 0)) { button_delete.option("disabled", false); }
        //sendCommand("action=" + ACTION_CHECKED_NODES_CV + "&chk_nodes_ids=" + checked_nodes, true);
    }    

    function getCheckedCVTreeNode(nodes) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i = 0; i < nodes_len; i++) {
                if (nodes[i].checked) checked_nodes += (checked_nodes == "" ? "" : ",") + nodes[i].id;
                getCheckedCVTreeNode(nodes[i].children);
            }
        }
    }

    var res_node;

    function zTreeGetNodeById(id) {        
        var nodes = zTreeObj.getNodes();                
        res_node = null;
        _getTreeNodeById(nodes, id);
        return res_node;
    }    

    function _getTreeNodeById(nodes, id) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i = 0; i < nodes_len; i++) {
                if (nodes[i].id == id) {
                    res_node = nodes[i];
                    i = nodes_len;
                }
                if (i < nodes_len) _getTreeNodeById(nodes[i].children, id);
            }
        }
    }

    var tree_node;
    function getSelectedCVTreeNode() {
        tree_node = null;
        _getSelectedCVTreeNode(tree_nodes_data);
        return tree_node;
    }

    function _getSelectedCVTreeNode(nodes) {
        if ((nodes) && nodes.length > 0) {
            var nodes_len = nodes.length;
            for (var i = 0; i < nodes_len; i++) {
                if (nodes[i].id == selectedNodeID) {
                    tree_node = nodes[i];
                    i = nodes_len;
                }
                if (i < nodes_len) _getSelectedCVTreeNode(nodes[i].children);
            }
        }        
    }
    /* end Tree View */

    /* Alternatves Datatable */    
    var IDX_ALT_ID = "id";
    var IDX_ALT_GUID = "guid";
    var IDX_ALT_DISPLAY_ID = "idx";
    var IDX_ALT_NAME = "name";
    var IDX_ALT_INFODOC = "infodoc";
    var IDX_ALT_PRIORITY = "priority";
    var IDX_ALT_DISABLED = "disabled";

    var alts_columns = <% = GetAlternativesColumns() %>;

    var COL_COLUMN_ID = 0;
    var COL_COLUMN_NAME = 1;
    var COL_COLUMN_VISIBILITY = 2;
    var COL_COLUMN_ALIGNMENT = 3;
    var COL_COLUMN_FIELD = 4;
    var COL_COLUMN_SORTABLE = 5;
    var COL_COLUMN_EDITABLE = 6;
    var COL_COLUMN_ATTR_TYPE = 7;
    var COL_COLUMN_ATTR_INDEX = 8;

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
        
        var dg = $("#tableAlts").dxDataGrid("instance");
        //dg.focus(cellInfo.rowIndex, cellInfo.column.dataField);
        dg.closeEditCell();

        //var alt = getAlternativeById(cellInfo.data["guid"]);
        //if ((alt)) {
        //    alt[cellInfo.column.dataField] = value;
            //sendCommand("action=set_attribute_value&guid=" + cellInfo.data["guid"] + "&" + cellInfo.column.dataField + "=" + e.value, true);
            e.component.option("validationStatus", "pending");
            callAPI("pm/hierarchy/?action=set_attribute_value", {"guid" : cellInfo.data["guid"], "dataField" : cellInfo.column.dataField, "value" : value}, function (data) {
                if (!data.success) {
                    //alt[cellInfo.column.dataField] = oldAttrValue;
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

    function initAltsDatagrid() {
        var table_main = (($("#tableAlts").data("dxDataGrid"))) ? $("#tableAlts").dxDataGrid("instance") : null;
        if (table_main !== null) { table_main.dispose(); }

        //init columns headers                
        var columns = [];
        for (var i = 0; i < alts_columns.length; i++) {
            var column_visibility = alts_columns[i][COL_COLUMN_VISIBILITY];
            var showInColumnChooser = true;
            if (isRisk) {
                if (alts_columns[i][COL_COLUMN_FIELD] == "cost") { column_visibility = false; showInColumnChooser = false; }
                if (alts_columns[i][COL_COLUMN_FIELD] == "risk") { column_visibility = false; showInColumnChooser = false; }
            } else {
                if (alts_columns[i][COL_COLUMN_FIELD] == "event_type") { column_visibility = false; showInColumnChooser = false; }
            }
            var allowSorting = true;
            columns.push({ "caption" : alts_columns[i][COL_COLUMN_NAME], "alignment" : alts_columns[i][COL_COLUMN_ALIGNMENT], "allowSorting" : alts_columns[i][COL_COLUMN_SORTABLE] && allowSorting, "allowSearch" : i == 2, "visible" : column_visibility, "dataField" : alts_columns[i][COL_COLUMN_FIELD], "allowEditing": alts_columns[i][COL_COLUMN_EDITABLE], "attrIndex": alts_columns[i][COL_COLUMN_ATTR_INDEX], "encodeHtml" : false, "showInColumnChooser" : showInColumnChooser });

            if (alts_columns[i][COL_COLUMN_FIELD] == "risk") {
                columns[columns.length - 1].dataType = "number";
                columns[columns.length - 1].editorOptions = { "min": 0, "max": 1 };
                columns[columns.length - 1].headerCellTemplate = function (columnHeader, headerInfo) {
                    columnHeader.append($("<div>").text(headerInfo.column.caption).prop("class", "dx-datagrid-text-content align-center").prop("title", "<% = ResString("lblProbFailure")%>"));
                }
            }

            if (alts_columns[i][COL_COLUMN_FIELD] == "name") {
                columns[columns.length - 1].customizeText = function(cellInfo) {
                    return htmlEscape(cellInfo.value);
                };
                columns[columns.length - 1].showEditorAlways = false;
            }
            if (isRisk && alts_columns[i][COL_COLUMN_FIELD] == "event_type") {
                columns[columns.length - 1].showEditorAlways = true;
                columns[columns.length - 1].customizeText = function(cellInfo) {
                    return cellInfo.value == 0 ? "<% = ParseString("%%Risk%%") %>" : "<% = ParseString("%%Opportunity%%") %>";
                };
                columns[columns.length - 1].editCellTemplate = function (cellElement, cellInfo) {
                    $("<div></div>").dxSelectBox({
                        displayExpr: "text",
                        valueExpr: "key",
                        items: [{"key" : 0, "text" : "<% = ParseString("%%Risk%%") %>"}, {"key" : 1, "text" : "<% = ParseString("%%Opportunity%%") %>"}],
                        value: cellInfo.value * 1,
                        onValueChanged: function(e) {
                            if (cellInfo.data[cellInfo.column.dataField] != e.value) {
                                cellInfo.component.option("value", e.value);
                                cellInfo.data[cellInfo.column.dataField] = e.value;
                                cellInfo.setValue(e.value);
                            }
                        }
                    }).appendTo(cellElement);                    
                };
            }
            
            if (alts_columns[i][COL_COLUMN_FIELD] == "color") {
                columns[columns.length - 1].editCellTemplate = function (element, info) {
                    var cb = document.createElement("div");
                    $(cb).dxColorBox({
                        value: info.data.color,
                        onValueChanged: function (e) {
                            if (info.data.color !== e.value) {
                                sendCommand("action=set_color&value=" + e.value + "&guid=" + info.data.guid);
                            }
                        }
                    });
                    element.append($(cb));
                }
            }

            if (alts_columns[i][COL_COLUMN_ATTR_TYPE] >= 0 || alts_columns[i][COL_COLUMN_FIELD] == "name") {
                columns[columns.length - 1].headerCellTemplate = function (columnHeader, headerInfo) {
                    columnHeader.append($("<span>").text(headerInfo.column.caption));
                    columnHeader.parent().children(":first").prepend(
                        $("<i class='fas fa-bars ec-icon'></i>").addClass("attr-header").css("margin-right", "4px").on("click", function (event) { 
                            var cmnu = $("#attr-context-menu").dxContextMenu("instance");
                            cmnu.option('target', $(this));
                            cmnu.option("attrIndex", headerInfo.column.dataField == "name" ? "name" : headerInfo.column.attrIndex);
                            cmnu.option("items[1].visible", headerInfo.column.dataField !== "name");
                            cmnu.show();
                            if ((event.preventDefault)) event.preventDefault(); 
                            return false; 
                        })
                    );                    
                    //columnHeader.width("100%");
                }
            }
            if (alts_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtString%>) {                
                columns[columns.length - 1].cellTemplate = function (cellElement, cellInfo) {
                    $("<div></div>").dxTextBox({
                        width: "100%",                        
                        value: cellInfo.value,
                        onValueChanged: function(e) {
                            setAttributeValueSilent(cellInfo, e);
                        }
                    }).appendTo(cellElement);                    
                }
            }
            if (alts_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtLong%> || alts_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtDouble%>) {                
                columns[columns.length - 1].cellTemplate = function (cellElement, cellInfo) {
                    $("<div></div>").dxNumberBox({
                        width: "100%",                        
                        value: cellInfo.value,
                        onValueChanged: function(e) {
                            setAttributeValueSilent(cellInfo, e);
                        }
                    }).appendTo(cellElement);                    
                }
            }
            if (alts_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtBoolean%>) {
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
                            setAttributeValueSilent(cellInfo, e)
                        }
                    }).appendTo(cellElement);                    
                }
            }
                if (alts_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtEnumeration%>) {
                    //var hfds = [];
                    //for (var k = 0; k < attr_data.length; k++) {
                    //    var v = attr_data[k];
                    //    if (v[IDX_ATTR_ID] == alts_columns[i][COL_COLUMN_ATTR_INDEX]) {
                    //        attr_id = v[IDX_ATTR_ID];
                    //        for (var t = 0; t < v[IDX_ATTR_ITEMS].length; t++) {
                    //            hfds.push({value : v[IDX_ATTR_ITEMS][t][0], text : v[IDX_ATTR_ITEMS][t][1]});
                    //        }
                    //        k = attr_data.length;
                    //    }
                    //}    
                    //columns[columns.length - 1].headerFilter = { dataSource : hfds };
                    //columns[columns.length - 1].allowHeaderFiltering = false;
                    columns[columns.length - 1].allowEditing = true;
                columns[columns.length - 1].editCellTemplate = function (cellElement, cellInfo) {
                    var sOptions = new DevExpress.data.DataSource({ store: [] }); //[];
                    var selectedOption = null;
                    var attr_id = "";
                    var alt_id = cellInfo.data["guid"];
                    for (var k = 0; k < attr_data.length; k++) {
                        var v = attr_data[k];
                        if (v[IDX_ATTR_ID] == cellInfo.column.attrIndex) {
                            attr_id = v[IDX_ATTR_ID];
                            for (var t = 0; t < v[IDX_ATTR_ITEMS].length; t++) {
                                sOptions.store().insert({ "guid" : v[IDX_ATTR_ITEMS][t][0], "text" : v[IDX_ATTR_ITEMS][t][1] });
                                if (v[IDX_ATTR_ITEMS][t][0] == cellInfo.data["vguid" + k]) selectedOption = cellInfo.data["vguid" + k];
                            }
                            k = attr_data.length;
                        }
                    }

                    $("<div></div>").dxSelectBox({
                        acceptCustomValue: true,
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
                      //      onCustomItemCreating: function (e) {
                      //
                      //  },
                        onValueChanged: function(e) {
                            if (typeof e.value == "undefined") {
                                var newValue = e.component.option("selectedItem");
                                var optionExists = false;
                                var nvl = newValue.toLowerCase();
                                for (var i = 0; i < sOptions.items().length; i++) {
                                    if (nvl == sOptions.items()[i].text.toLowerCase()) optionExists = true;
                                }
                                if ((newValue) && newValue !== "" && !optionExists) {
                                    dxConfirm("Do you want to add a new category \""+newValue+"\" and associate it with this <%=ResString("lblAlternative").ToLower%>", "sendCommand(\"action=add_item_and_assign&name=" + encodeURIComponent(newValue) + "&clmn=" + attr_id + "&alt_id=" + alt_id + "\");", function () { e.value = e.previousValue });
                                }
                            } else {                           
                                for (var t = 0; t < sOptions.items().length; t++) { if (sOptions.items()[t].guid == e.value) e.text = sOptions.items()[t].text; }
                                setAttributeValueSilent(cellInfo, e);
                            }
                        }
                     }).appendTo(cellElement);                    
                 }
            }
        if (alts_columns[i][COL_COLUMN_ATTR_TYPE] == <% = AttributeValueTypes.avtEnumerationMulti%>) {
            //columns[columns.length - 1].customizeText = function(cellInfo) {
            //    for (var k = 0; k < attr_data.length; k++) {
            //        var v = attr_data[k];
            //        if (v[IDX_ATTR_ID] == cellInfo.column.attrIndex) {
            //            attr_id = v[IDX_ATTR_ID];
            //            var s = "";
            //            for (var t = 0; t < v[IDX_ATTR_ITEMS].length; t++) {
            //                if ((cellInfo.data["vguid" + k]) && (cellInfo.data["vguid" + k] !== "") && cellInfo.data["vguid" + k].indexOf(v[IDX_ATTR_ITEMS][t][0]) != -1) s += (s == "" ? "" : ",") + v[IDX_ATTR_ITEMS][t][1];
            //            }
            //            k = attr_data.length;
            //            return s;
            //        }
            //    }    
            //    return info.value;
            //}
            columns[columns.length - 1].cellTemplate = function (cellElement, cellInfo) {

                //var multi_items = new DevExpress.data.DataSource({ store: [] });
                var multi_items = [];
                var sel_items = [];

                for (var k = 0; k < attr_data.length; k++) {
                    var v = attr_data[k];
                    if (v[IDX_ATTR_ID] == cellInfo.column.attrIndex) {
                        for (var t = 0; t < v[IDX_ATTR_ITEMS].length; t++) {
                            var item = {"key" : v[IDX_ATTR_ITEMS][t][0], "chk" : (cellInfo.data["vguid" + k]) && (cellInfo.data["vguid" + k] !== "") && cellInfo.data["vguid" + k].indexOf(v[IDX_ATTR_ITEMS][t][0]) != -1, "text" : v[IDX_ATTR_ITEMS][t][1]};
                            //multi_items.store().insert(item);
                            multi_items.push(item);
                            
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
                            setAttributeValueSilent(cellInfo, e, true);
                        //}
                    },
                    onCustomItemCreating: function(e){
                        e.customItem = { "key" : "new_item_guid", "chk" : true, "text" : "" };
                        //multi_items.store().insert(e.customItem);
                        //multi_items.reload();
                        multi_items.push(e.customItem);
                    }
                }).appendTo(cellElement);
                }    
            }
        }

        columns[0].allowHeaderFiltering = false; // index column
        columns[1].allowHeaderFiltering = false; // infodoc column
        columns[3].allowHeaderFiltering = false; // disabled column

        columns[0].allowFiltering = false; // index column
        columns[1].allowFiltering = false; // infodoc column
        columns[3].allowFiltering = false; // disabled column

        columns[0]["width"] = "60px"; // index column width
        columns[1]["width"] = "55px"; // infodoc column width
        //columns[2]["width"] = "250px"; // name column width
        columns[3]["width"] = "100px"; // disabled column width
        <%--columns[4]["width"] = "110px"; // priority column width
        columns[4]["customizeText"] = function(cellInfo) {
            return (cellInfo.value == "<%=UNDEFINED_INTEGER_VALUE.ToString%>" ? "N/A" : cellInfo.value + "%");           
        }--%>
        <%--columns[4].customizeText = function(cellInfo) {
            return (cellInfo.valueText == "<%=UNDEFINED_INTEGER_VALUE.ToString%>" ? "" : cellInfo.value);
        }
        columns[5].customizeText = function(cellInfo) {
            return (cellInfo.valueText == "<%=UNDEFINED_INTEGER_VALUE.ToString%>" ? "" : cellInfo.value);
        }--%>

        columns[1].cellTemplate = function(element, info) {
            var alt = getAlternativeById(info.data[IDX_ALT_GUID]);
            element.append('<i class="fas fa-info-circle ' + (alt[IDX_ALT_INFODOC] ? "toolbar-icon" : "toolbar-icon-disabled") + '" style="' + (isReadOnly ? "" : "cursor: pointer; ") + '" id="imgTreeInfodoc' + replaceString("-", "", info.data[IDX_ALT_GUID]) + '" onclick="setTimeout(function(){selInfodocIDs = \'' + info.data[IDX_ALT_GUID] + '\'; EditInfodoc();}, 50); return false;"></i>');
        };

        store = new DevExpress.data.ArrayStore({
            key: 'guid',
            data: alts_data
        });

        $("#tableAlts").dxDataGrid({
            dataSource: { store : store },
            width: "100%",
            columns: columns,
            columnHidingEnabled: false,
            allowColumnResizing: true,
            allowColumnReordering: false,
            columnAutoWidth: true,
            columnChooser: {
                height: function() { return Math.round($(window).height() * 0.8); },
                mode: "select",
                enabled: true
            },            
            columnFixing: {
                enabled: true
            },
            columnResizingMode: "widget",
            columnMinWidth: 10,            
            editing: {
                mode: "cell", //"row", //"batch",
                useIcons: true,
                allowUpdating: !isReadOnly,
                //startEditAction: "dblClick"
            },
            filterPanel: {
                visible: false
            },
            filterRow: {
                visible: false
            },
            headerFilter: {
                allowSearch: true,
                visible: true
            },
            searchPanel: {
                visible: true,
                width: 240,
                placeholder: resString("btnDoSearch") + "..."
            },
            onCellPrepared: function(e) {
                if(e.rowType === "data" && e.column.dataField === "color") {
                    e.cellElement.css("color", e.data.color);
                }
            },
            onContentReady: function (e) {
                //initDragging(e.element);
                //setSelectedAlternative(selectedNodeID);
                $("#btnResetView").dxButton("instance").option("disabled", !e.component.getDataSource() || e.component.getDataSource().sort() == null);

                e.component.option("rowDragging.allowReordering", !isDataGridSorted());
                if (e.component.option("focusedRowIndex") === -1) e.component.option("focusedRowIndex", 0);
            },
            onRowPrepared: function (e) {
                if (e.rowType != 'data') return;
                var row = e.rowElement;
                var data = e.data;
                row.addClass('myRow').data('keyValue', e.key);
            },
            onRowUpdating: function (e) {
                e.newData.key = e.key;                
                var tmpName;
                if (typeof e.newData.name !== "undefined") { 
                    tmpName = e.newData.name;
                    e.newData.name = e.newData.name;
                }
                // save changes
                callAPI("pm/hierarchy/?action=update_alternative", e.newData, function (data) {                    
                    if (!data.success) {
                        e.cancel = true;
                    }
                    DevExpress.ui.notify(data.success ? "Changes Saved" : "Error", data.success ? "success" : "error");
                    e.component.option("validationStatus", data.success ? "valid" : "invalid");
                }, true);
                if (typeof tmpName !== "undefined") e.newData.name = tmpName;
            },
            onToolbarPreparing: function (e) {
                var toolbarItems = e.toolbarOptions.items;
                toolbarItems.splice(0, 0, {
                    widget: 'dxButton', 
                    options: { 
                        icon: 'fas fa-sync-alt', 
                        elementAttr: { "id" : "btnResetView" },
                        hint: "Reset View",
                        showText: false,
                        visible: curPgID == _PGID_STRUCTURE_ALTERNATIVES,
                        disabled: false,
                        onClick: function() {
                            var grid = $("#tableAlts").dxDataGrid("instance");
                            grid.clearSorting();
                        } 
                    },
                    locateInMenu: "auto",
                    showText: "always",
                    location: 'after'
                });
            },
            "export": {
                enabled: true
            },
            pager: {
                allowedPageSizes: <% = _OPT_PAGINATION_PAGE_SIZES %>,
                showPageSizeSelector: true,
                showNavigationButtons: true,
                visible: alts_data.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            paging: {
                enabled: alts_data.length > <% = _OPT_PAGINATION_LIMIT %>
            },
            rowAlternationEnabled: true,
            rowDragging: {
                allowDropInsideItem: false,
                allowReordering: true,
                dropFeedbackMode: "indicate",
                onReorder: function (e) {
                    sendCommand("action=reorder_alts&from_index=" + e.fromIndex + "&to_index=" + e.toIndex, true);
                }
            },
            showColumnLines: true,
            showBorders: false,
            showRowLines: false,
            keyExpr: "guid",
            selection: {
                mode: is_multiselect ? "multiple" : "single",
                allowSelectAll: true,
                showCheckBoxesMode: "always"
            },
            onFocusedRowChanged: function (e) {
                var btnEditInfodoc = document.getElementById("btnEditInfodoc");
                if ((e.row) && (e.row.data)) { 
                    var alt = getAlternativeById(e.row.data.guid);
                    $("#tbNodeTitle").html(htmlEscape(alt["name"]));
                    LoadInfodoc(alt["iid"], alt["guid"], true); 
                    if ((btnEditInfodoc)) { btnEditInfodoc.disabled = isReadOnly && <% = Bool2JS(Not PRJ.ProjectStatus = ecProjectStatus.psMasterProject) %>; }
                } else {
                    $("#tbNodeTitle").html("");
                    LoadInfodoc("", "", true); 
                    if ((btnEditInfodoc)) { btnEditInfodoc.disabled = true; }
                }
            },
            onSelectionChanged: function (e) {
                updateToolbar();
                //if (!is_multiselect && e.selectedRowKeys.length > 0) { 
                //if (e.selectedRowKeys.length > 0) { 
                //    selectedNodeID = e.selectedRowKeys[0];
                //    var alt = getAlternativeById(selectedNodeID);
                //    $("#tbNodeTitle").html(htmlEscape(alt["name"]));
                //    LoadInfodoc(alt["iid"], alt["guid"], true); 
                //}
                //if (e.selectedRowKeys.length == 0) { 
                //    $("#tbNodeTitle").html("");
                //    LoadInfodoc("", "", true); 
                //}
                var button_delete_alt = $("#btn_delete_alt").dxButton("instance");
                button_delete_alt.option("disabled", isReadOnly || e.selectedRowKeys.length == 0);
                var button_duplicate_alt = $("#btn_duplicate_alt").dxButton("instance");
                button_duplicate_alt.option("disabled", isReadOnly || e.selectedRowKeys.length == 0);
            },
            //onCellClick: function (e) {
                //e.component.selectRows([e.key], false);
                //if (!is_multiselect && selectedNodeID != e.key) {
                //    selectedNodeID = e.key;
                //    setSelectedAlternative(e.key);
                //}
            //},
            focusedRowIndex: -1,
            stateStoring: {
                enabled: true,
                type: "localStorage",
                storageKey: "StructureAlts_Datagrid_<% = App.ProjectID %>_210920"
            },
            sorting: {
                mode: "multiple",
                showSortIndexes: true
            },
            hoverStateEnabled: true,
            //focusStateEnabled: true,
            focusedRowEnabled: true,
            noDataText: "<% = GetEmptyMessage() %>",
            loadPanel: {
                enabled: false,
            },
            //loadPanel: main_loadPanel_options,
        });           
        
        resizePage();        
    }

    function isDataGridSorted() {
        var dg = $("#tableAlts").dxDataGrid("instance");
        var columns = dg.option("columns");
        var retVal = false;
        columns.forEach(function (item, index) {
            if (dg.columnOption(index, 'sortIndex') >= 0) retVal = true;
        });
        return retVal;
    }

    var attribute_index;

    function doPasteAttributeValues(attr_idx) {
        attribute_index = attr_idx;
        var data = "";
        if (window.clipboardData) {
            data = window.clipboardData.getData('Text'); 
            pasteDataAttrValues(data);
        } else {
            if (navigator.clipboard) {                
                try {
                    navigator.clipboard.readText().then(pasteDataAttrValues);
                } catch (error) {
                    if (error.name == "TypeError") { //FF
                        dxDialog("<%=ResString("titleNonIEPaste") %>:<pre><textarea id='pasteBox' cols='70' rows='20'></textarea></pre>", "commitPasteFFAttrValues(" + attr_idx + ");", ";", "<%=ResString("titlePasteFromClipboard") %>", "<%=ResString("btnPaste") %>", "<%=ResString("btnCancel") %>");
                        setTimeout(function () { $("#pasteBox").focus(); }, 500);
                    }
                }
            } else {
                DevExpress.ui.notify(resString("msgUnableToPaste"), "error");
            }
        }
    }

    function commitPasteFFAttrValues(attr_idx) {
        attribute_index = attr_idx;
        var pasteBox = document.getElementById("pasteBox");
        if ((pasteBox)) {
            pasteDataAttrValues(pasteBox.value);
        }
    }

    function pasteDataAttrValues(data) {
        var attr_idx = attribute_index;
        if (typeof data != "undefined" && data != "") {
            switch (attr_idx+"") {
                case "name":
                case "cost":
                    sendCommand("action=paste_default_column&column="+attr_idx+"&data="+encodeURIComponent(data));
                    break;
                default:
                sendCommand("action=paste_attribute_data&attr_idx="+attr_idx+"&data="+encodeURIComponent(data));
            }
        } else { DevExpress.ui.notify("<%=ResString("msgUnableToPaste") %>", "error"); }
    }

    function doCopyToClipboardValues(attr_idx) {
        var res = "";
        var alts_data_len = alts_data.length;        
        for (var i = 0; i < alts_data_len; i++) {
            res += (res==""?"":"\r\n") + (attr_idx == "name" ? alts_data[i]["name"] : alts_data[i]["v" + attr_idx]);
        }
        copyDataToClipboard(res);
    }

    var grid_w_old = 0;
    var grid_h_old = 0;

    function resizeGrid(id) {
        //return false;
        var d = ($("body").hasClass("fullscreen") ? 20 : 0);
        $("#" + id).height(300).width(400);
        var td = $("#divAltsView");
        var w = $("#" + id).width(Math.round(td[0].clientWidth-d)).width();
        var h = $("#" + id).height(Math.round(td[0].clientHeight-5)).height();
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
            resizeGrid(id);
        }
    }

    function resizeDatatable(force_redraw) {
        var table_main = (($("#tableAlts").data("dxDataGrid"))) ? $("#tableAlts").dxDataGrid("instance") : null;
        if ((table_main) &&table_main !== null) {
            table_main.updateDimensions();
        }
        var w = $(window).innerWidth();
        var h = $(window).innerHeight();
        if (force_redraw) {
            grid_w_old = 0;
            grid_h_old = 0;
        }
        checkResize("tableAlts", force_redraw ? 0 : w, force_redraw ? 0 : h);
    }
    /* end Alternatives Datatable */

    // Alternative Attributes
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

    function manageAttributes() {
        initAttributes();
        initAttributesDlg();
    }

    function initAttributes() {
        var t = $("#tblAttributes tbody");
        
        if ((t)) {
            t.html("");        
            for (var i = 0; i < attr_data.length; i++) {
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
                            var isDefault = v[IDX_ATTR_ITEMS][j][IDX_ATTR_ITEM_DEFAULT] == 1;
                            vals += (vals == "" ? "" : ", ") + (isDefault ? "<b>" : "") + htmlEscape(v[IDX_ATTR_ITEMS][j][IDX_ATTR_NAME]) + (isDefault ? "</b>" : "");
                        };
                        break;
                    //case avtEnumerationMulti:
                    //   vals = 'not implemented';
                    //   break;
                }

                var sHidden = ""; //(attr_type == avtEnumeration || attr_type == avtEnumerationMulti ? "" : " style='display:none;' ");
                
                sRow = "<tr class='text grid_row' " + sHidden + " style='min-height: 28px;'>";
                sRow += "<td " + sHidden + " align='center' style='width:20px'>&nbsp;</td>";
                sRow += "<td " + sHidden + " id='tdName" + i + "'' style='cursor: pointer;' ondblclick='onEditAttribute(" + i + "); return false;'>" + n + "</td>";                
                sRow += "<td " + sHidden + " id='tdEditAction" + i + "' align='right'><a href='' style='cursor: pointer;' onclick='onEditAttribute(" + i + "); return false;'><i class='fas fa-pencil-alt ec-icon'></i></a></td>";
                sRow += "<td " + sHidden + " id='tdType" + i + "' align='center'>" + getAttrTypeName(v[IDX_ATTR_TYPE]) + "</td>";
                sRow += "<td " + sHidden + " id='tdValues" + i + "' style='cursor: pointer;' ondblclick='onEditAttributeValues(" + i + ","+v[IDX_ATTR_TYPE]+"); return false;'>" + vals + "</td>";
                sRow += "<td " + sHidden + " id='tdEditValues" + i + "' align='right'><a href='' style='cursor: pointer;' onclick='onEditAttributeValues(" + i + ","+v[IDX_ATTR_TYPE]+"); return false;'><i class='fas fa-pencil-alt ec-icon'></i></a></td>";
                sRow += "<td " + sHidden + " id='tdActions" + i + "' align='center'><a href='' style='cursor: pointer;' onclick='DeleteAttribute(" + i + "); return false;'><i class='fas fa-trash-alt ec-icon'></i></a></td>";
                sRow +="</tr>";
                t.append(sRow);
            }

            sRow = "<tr class='text grid_footer' id='trNew'>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width:100%; height: 18px;' id='tbAttrName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='if (event.keyCode == ENTERKEY) AddAttribute();'></td>";
            sRow += "<td><select class='select' style='width:120px;' id='tbType' onchange='cbNewAttrTypeChanged(this.value)' onkeydown='if (event.keyCode == ENTERKEY) AddAttribute();'>" + getAttrTypeOptions() + "</select></td>";
            //sRow += "<td>&nbsp;</td>"; // edit name icon
            sRow += "<td colspan='2'><nobr>&nbsp;<span id='lblDefaultValue' style='display:none;'><%=ResString("lblDefaultAttrValue")%>:&nbsp;</span><input type='text' id='tbDefaultTextValue' style='display:none; width:130px; height:18px;' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='if (event.keyCode == ENTERKEY) AddAttribute();'><select id='tbDefaultBoolValue' class='select' style='display:none;width:80px;' onkeydown='if (event.keyCode == ENTERKEY) AddAttribute();'><option value='1'><%=ResString("lblYes")%></option><option value='0'><%=ResString("lblNo") %></option></select></nobr></td>"; // values
            //sRow += "<td>&nbsp;</td>"; // edit values icon
            sRow += "<td align='center'><a href='' onclick='AddAttribute(); return false;' title='<% = JS_SafeString(ResString("titleAddCC")) %>'><i class='fas fa-plus-square ec-icon'></i></a></td></tr>";
            t.append(sRow);
            //if ((dlg_attributes) && dlg_attributes.dialog("isOpen")) on_hit_enter = "AddAttribute();";
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
        <% If IsMultiCategoricalAttributesAllowed Then %>
        retVal += "<option value='" + avtEnumerationMulti + "'><%=ResString("optAttrTypeMultiEnum")%></option>";
        <% End If %>
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
                    sendCommand("action=attributes_reorder&lst=" + encodeURIComponent(attr_order), false);
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
        setTimeout(function () { $("#pAttributes").scrollTop(10000); }, 30);
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
                    var isDefault = v[IDX_ATTR_ITEM_DEFAULT] == 1;
                    var isChecked = (isDefault ? "checked='checked'" : "");
                    sRow = "<tr class='text grid_row'>";
                    sRow += "<td align='center' style='width:20px'>&nbsp;</td>";
                    sRow += "<td id='tdCatName" + i + "''>" + (isDefault ? "<b>" : "") + n + (isDefault ? "</b>" : "") + "</td>";
                    sRow += "<td align='center' id='tdCatIsDefault" + i + "''><input type='checkbox' class='checkbox' onclick='onEnumDefaultClick("+i+",this.checked);' onchange='onEnumDefaultClick("+i+",this.checked);' onkeydown='onEnumDefaultClick("+i+",this.checked);' "+isChecked+"></td>";
                    sRow += "<td id='tdCatActions" + i + "' align='center'><a href='' onclick='onEditAttributeValue(" + i + "); return false;'><i class='fas fa-pencil-alt ec-icon'></i></a>&nbsp;<a href='' onclick='DeleteAttributeValue(" + i + "); return false;'><i class='fas fa-trash-alt ec-icon'></i></a></td>";
                    sRow +="</tr>";
                    t.append(sRow);
                }
            }

            sRow = "<tr class='text grid_footer' id='trCatNew'>";
            //sRow += "<td align='center' style='width:20px'>&nbsp;</td>";
            sRow += "<td colspan='3'><input type='text' class='input' style='width:100%; height:18px;' id='tbCatName' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' onkeydown='if (event.keyCode == ENTERKEY) AddAttributeValue();'></td>";
            sRow += "<td align='center'><a href='' onclick='AddAttributeValue(); return false;' title='<% = JS_SafeString(ResString("titleAddCategory")) %>'><i class='fas fa-plus-square ec-icon'></i></a></td></tr>";
            t.append(sRow);

            //if ((dlg_attributes_cat) && dlg_attributes_cat.dialog("isOpen")) on_hit_enter_cat = "AddAttributeValue();";
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
                setTimeout('document.getElementById("tbCatName").focus();', 60);
              },
              close: function() {
                $("body").css("overflow", "auto");
                initAttributesValues();
                $("#tblAttributesValues tbody").html("");
                on_hit_enter_cat = "";
                $("#divAttributesValues").dialog("destroy");
                if (attr_values_order!="") {
                    sendCommand("action=enum_items_reorder&lst=" + encodeURIComponent(attr_values_order) + "&clmn=" + SelectedAttrIndex, false);
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
        setTimeout(function () { $("#pAttributesValues").scrollTop(10000); }, 30);
        setTimeout('document.getElementById("tbCatName").focus();', 100);
    }

    function checkUnsavedData(e, on_agree) {
        if ((e) && (e.value!="")) {
            dxConfirm(resString("msgUnsavedData"), on_agree + ";");
            return false;
        }
        return true;
    }

    function onEditAttribute(index, skip_check) {
        SelectedAttrIndex = index;
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbAttrName"), "onEditAttribute(" + index + ", true)")) return false;
        initAttributes();
        $("#tdName" + index).html("<input type='text' class='input' style='width:" + $("#tdName" + index).width()+ "' id='tbAttrName' value='" + replaceString("'", "&#39;", attr_data[index][IDX_ATTR_NAME]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdEditAction" + index).html("<a href='' style='cursor: pointer;' onclick='EditAttribute(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save'></i></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbAttrName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban'></i></a>");
        //$("#trNew").html("").hide();
        setTimeout(function () { document.getElementById('tbAttrName').focus(); }, 50);
        on_hit_enter = "EditAttribute(" + index + ");";
    }

    function onEditAttributeValue(index, skip_check) {
        if (!(skip_check) && !checkUnsavedData(document.getElementById("tbCatName"), "onEditAttributeValue(" + index + ", true)")) return false;
        initAttributesValues();
        $("#tdCatName" + index).html("<input type='text' class='input' style='height:18px; width:" + $("#tdCatName" + index).width()+ "' id='tbCatName' value='" + replaceString("'", "&#39;", attr_data[SelectedAttrIndex][IDX_ATTR_ITEMS][index][IDX_ATTR_NAME]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
        $("#tdCatActions" + index).html("<nobr><a href='' style='cursor: pointer;' onclick='EditAttributeValue(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save ec-icon'></i></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbCatName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban ec-icon'></i></a></nobr>");
        $("#trCatNew").html("").hide();
        setTimeout(function () { document.getElementById('tbCatName').focus(); }, 70);
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
                $("#tdValues"   + index).html("<input type='text' class='input' style='height:18px; width:" + $("#tdValues" + index).width()+ "' id='tbDefValue' value='" + htmlEscape(attr[IDX_ATTR_DEF_VALUE]) + "' autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false'>");
            }
            $("#tdEditValues" + index).html("<nobr><a href='' style='cursor: pointer;' onclick='EditDefaultValue(" + index + "); return false;' title='<% = JS_SafeString(ResString("btnSaveChanges")) %>'><i class='fas fa-save ec-icon'></i></a>&nbsp;<a href='' onclick='initAttributes(); document.getElementById(\"tbAttrName\").focus(); return false;' title='<% = JS_SafeString(ResString("btnCancelChanges")) %>'><i class='fas fa-ban ec-icon'></i></a></nobr>");
            //$("#trNew").html("").hide();
            //if (attr_type == avtBoolean) { setTimeout(function () { document.getElementById('cbDefValue').focus(); }, 50) } else { setTimeout(function () { document.getElementById('tbDefValue').focus(); }, 50) };
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
                sendCommand("action=set_default_value&def_val=" + encodeURIComponent(def_val) + "&clmn=" + index, false);
            }
        }
    }

    function onEnumDefaultClick(item_index, checked) {
        var attr = GetSelectedAttr();
        if ((attr)) {
            if (attr[IDX_ATTR_TYPE] == avtEnumeration) {
                for (var i = 0; i < attr[IDX_ATTR_ITEMS].length; i++) {
                    attr[IDX_ATTR_ITEMS][i][IDX_ATTR_ITEM_DEFAULT] = ((i == item_index) && checked ? 1 : 0);
                }
            }
            if (attr[IDX_ATTR_TYPE] == avtEnumerationMulti) {
                attr[IDX_ATTR_ITEMS][item_index][IDX_ATTR_ITEM_DEFAULT] = checked ? 1 : 0;
            }
            initAttributesValues();
            sendCommand("action=set_default_value&def_val=" + (checked ? 1 : 0) + "&clmn=" + SelectedAttrIndex + "&item_index=" + item_index, false);
        }
    }

    /* Add-remove-rename columns */
    function EditAttribute(index) {
        var n = document.getElementById("tbAttrName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                DevExpress.ui.notify(resString("msgEmptyCCName"), "error");
            } else {
                if ((index >= 0) && (index < attr_data.length)) {
                    // idx = attr_data[index][IDX_ATTR_ID];
                    attr_data[index][IDX_ATTR_NAME] = htmlEscape(n.value);
                    sendCommand("action=rename_column&name=" + n.value + "&clmn=" + index, false);
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
                DevExpress.ui.notify(resString("msgCreateEmptyCCName"), "error");
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
                    sendCommand('action=add_column&name='+encodeURIComponent(n.value)+'&type='+t.value+'&def_val='+encodeURIComponent(def_val), false);
                }
            }
        }
    }

    function DeleteAttribute(idx) {
        SelectedAttrIndex = idx;
        dxConfirm(resString("msgSureDeleteCC"), "sendCommand(\"action=del_column&clmn=" + idx + "\");", false);
    }

    /* Toolbar */
    var is_manual = false;
    var toolbarItemsMain = [<%If CurrentPageID <> _PGID_STRUCTURE_ALTERNATIVES Then%>
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxSelectBox',
        visible: true,
        options: {
            searchEnabled: false,
            valueExpr: "ID",
            value: <%=ActiveView%>,
            displayExpr: "Text",
            disabled: false,
            elementAttr: {id: 'btn_switch_view'},
            //onValueChanged: function (data) { 
            //    onSetView(data.value);
            //},
            onSelectionChanged: function (e) { 
                if (e.selectedItem.ID * 1 !== ActiveView) {
                    onSetView(e.selectedItem.ID * 1);
                }
            },
            <%--items: [{"ID": <%=CInt(HierarchyViews.hvGraph)%>, "Text": "Hierarchy View"}, {"ID": <%=CInt(HierarchyViews.hvTree)%>, "Text": "Tree View"}, {"ID": <%=CInt(HierarchyViews.hvAlts)%>, "Text": "<%=ParseString("%%Alternatives%%")%> View - IN PROGRESS", "disabled": true}]--%>
            <%--items: [{"ID": <%=CInt(HierarchyViews.hvGraph)%>, "Text": "Hierarchy View"}, {"ID": <%=CInt(HierarchyViews.hvTree)%>, "Text": "Tree View"}, {"ID": <%=CInt(HierarchyViews.hvAlts)%>, "Text": "<%=ParseString("%%Alternatives%%")%>", "disabled": false}]--%>
            items: [{"ID": <%=CInt(HierarchyViews.hvTree)%>, "Text": "Tree View"}, {"ID": <%=CInt(HierarchyViews.hvGraph)%>, "Text": "Hierarchy View"}]
        }
    }, 
    {
        //beginGroup: true, //separator
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        //"~/App_Themes/EC09/images/plus-20.png"
        options: {
            icon: "", //"far fa-plus-square",
            text: "Expand All",
            hint: "Expand All",
            disabled: false,
            elementAttr: {id: 'btn_expand_all'},
            onClick: function () {
                onClickToolbar("expand_all");
           }
        }
    },
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        //"~/App_Themes/EC09/images/minus-20.png"
        options: {
            icon: "", //"far fa-minus-square",
            text: "Collapse All",
            hint: "Collapse All",
            disabled: false,
            elementAttr: {id: 'btn_collapse_all'},
            onClick: function () {
                onClickToolbar("collapse_all");
            }
        }
    },          
    {
        location: 'after',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        options: {
            icon: "fas fa-cog",
            text: "Preferences",
            //hint: "Preferences",
            disabled: false,
            elementAttr: {id: 'btn_settings'},
            onClick: function () {
                settingsDialog();
            }
        }
    }, {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxCheckBox',
        visible: true,
        options: {
            text: "Auto-Redraw",
            showText: true,
            width:100,
            value: is_autoredraw,
            disabled: false,
            elementAttr: {id: 'btn_auto_redraw'},
            onValueChanged: function (e) {
                if (!is_manual) onClickToolbar("auto_redraw");
            }
        }
    },<%End If%>
    {
        location: 'before',
        locateInMenu: 'never',
        widget: 'dxCheckBox',
        visible: false,
        options: {
            text: "Multiselect",
            showText: true,
            //width:100,
            value: is_multiselect,
            disabled: isReadOnly,
            elementAttr: {id: 'btn_multiselect'},
            onValueChanged: function (e) {
                if (!is_manual) onClickToolbar("multiselect_switch");
            }
        }
    },
    { // RadToolBarDropDown ImageUrl c_plus_16.png  Text=Add
        //beginGroup: true, //separator
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxDropDownButton',
        options: {
            elementAttr: { id: 'btn_add', "class": "dx-button-default-dd" },
            type: "default",
            stylingMode: "contained",
            //width: "79px",
            disabled: isReadOnly,
            text: "Add", 
            icon: "fas fa-plus-circle",
            displayExpr: "text",
            useSelectMode: false,
            wrapItemText: false,
            dropDownOptions: {
                width: dlgMaxWidth(370),
            },
            items: [],
            onButtonClick: function (e) {
                var items = [{   //"~/App_Themes/EC09/images/add_below_20.png"
                    text: "<%=ResString("cmnuObjectivesAddBelow")%>",
                    visible: curPgID != _PGID_STRUCTURE_ALTERNATIVES,
                    disabled: isReadOnly || curPgID == _PGID_STRUCTURE_ALTERNATIVES || (!(getSelectedNode()[IDX_IS_ALT] == 0)),
                    onClick: function () {
                        onClickToolbar("add_level_below");
                    }
                }, {   //"~/App_Themes/EC09/images/add_level_20.png"
                    text: "<%=ResString("cmnuObjectivesAddSibling")%>",
                    visible: curPgID != _PGID_STRUCTURE_ALTERNATIVES,
                    disabled: isReadOnly || curPgID == _PGID_STRUCTURE_ALTERNATIVES || (!(getSelectedNode()[IDX_IS_ALT] == 0 && !isGoal(getSelectedNode()))),
                    onClick: function () {
                        onClickToolbar("add_node");
                    }
                }, {
                    text: "<%=ResString("btnAddAlternativesOrPaste")%>",
                    //visible: ActiveView == hvGraph,
                    visible: curPgID == _PGID_STRUCTURE_ALTERNATIVES,
                    disabled: isReadOnly,
                    onClick: function () {
                        onClickToolbar("add_alternatives");
                    }
                }, {
                    text: "<%=ResString("btnAddAlternativesBelow")%>",
                    visible: curPgID == _PGID_STRUCTURE_ALTERNATIVES,
                    disabled: isReadOnly || curPgID !== _PGID_STRUCTURE_ALTERNATIVES || !$("#tableAlts").data("dxDataGrid") || $("#tableAlts").dxDataGrid("instance").option("focusedRowIndex") < 0,
                    onClick: function () {
                        onClickToolbar("add_alternatives_below");
                    }
                }, {   //SL16152
                    text: "<%=ResString(If(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES, "cmnuAlternativesAddFromDataset", "cmnuObjectivesAddFromDataset"))%>",
                    visible: true,  //curPgID != _PGID_STRUCTURE_ALTERNATIVES,
                    disabled: isReadOnly || (curPgID !== _PGID_STRUCTURE_ALTERNATIVES && !(getSelectedNode())),
                    onClick: function () {
                        onClickToolbar(ACTION_ADD_FROM_DATASET);
                    }
                }];
                e.component.option('items', items);
            }
        }
    },<%If CurrentPageID <> _PGID_STRUCTURE_ALTERNATIVES Then%>
    {
        //beginGroup: true, //separator
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        //"~/App_Themes/EC09/images/edit_20.png"
        options: {
            icon: "edit",
            text: "<%=ResString("btnEdit")%>",
            disabled: isReadOnly,
            elementAttr: {id: 'btn_edit_node'},
            onClick: function () {
                onClickToolbar("edit_node");
            }
        }
    },<%End If%>
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        //"~/App_Themes/EC09/images/del_20.png"
        options: {
            icon: "trash",
            text: "<%=ResString("btnDelete")%>",
            disabled: true,
            elementAttr: {id: 'btn_delete_node'},
            onClick: function () {
                onClickToolbar("delete_node");
            }
        }
    },<%If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then%>
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: !isReadOnly,
        //"~/App_Themes/EC09/images/del_20.png"
        options: {
            icon: "trash",
            text: "<%=ResString("btnDelete")%>",
            disabled: true,
            elementAttr: {id: 'btn_delete_alt'},
            onClick: function () {
                onClickToolbar("delete_alt");
            }
        }
    },
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: true,
        options: {
            icon: "fas fa-clone",
            text: "Duplicate",
            disabled: true,
            elementAttr: {id: 'btn_duplicate_alt'},
            onClick: function () {
                onClickToolbar("duplicate_alt");
            }
        }
    },{
        //beginGroup: true, //separator
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxButton',
        visible: !isReadOnly,
        //"~/App_Themes/EC09/images/config-20.png"
        options: {
            icon: "far fa-list-alt",
            text: "Attributes",
            disabled: isReadOnly || <%=Bool2JS(App.IsActiveProjectStructureReadOnly)%>,
            elementAttr: {id: 'btn_edit_attributes'},
            onClick: function () {
                onClickToolbar("edit_attributes");
            }
        }
    }, <% End If %><% If PM.IsRiskProject OrElse CurrentPageID = _PGID_STRUCTURE_HIERARCHY Then %>
    {
        location: 'before', locateInMenu: 'never',  template: '<span>ID:</span>' 
    }, {
        location: 'before',
        locateInMenu: 'never',
        widget: 'dxSelectBox',
        visible: !isReadOnly,
        options: { 
            //width: "100%",
            dataSource: [
                //{ "id" : -1, "text" : "None" }, 
                { "id" : -1, "text" : "None", "visible": curPgID !== _PGID_STRUCTURE_ALTERNATIVES }, 
                { "id" : <% = CInt(IDColumnModes.UniqueID) %>, "text" : "<%=ResString("optUniqueID")%>", "visible": curPgID == _PGID_STRUCTURE_ALTERNATIVES }, 
                { "id" : <% = CInt(IDColumnModes.IndexID) %>, "text" : "<%=ResString("optIndexID")%>" },
                { "id" : <% = CInt(IDColumnModes.Rank) %>, "text" : "<%=ResString("optRank")%>", "visible": curPgID == _PGID_STRUCTURE_ALTERNATIVES }
            ],
            disabled: false,
            displayExpr: "text",
            searchEnabled: false,
            <%--value: <% = CInt(If(PM.Parameters.NodeIndexIsVisible, PM.Parameters.NodeVisibleIndexMode, -1)) %>,--%>
            value: <% = If(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES, CInt(PM.Parameters.NodeVisibleIndexMode), If(PM.Parameters.ObjectiveIndexIsVisible, 1, -1)) %>,
            valueExpr: "id",
            onValueChanged: function (e) {
                if (curPgID === _PGID_STRUCTURE_ALTERNATIVES) {
                    sendCommand("action=event_id_mode&value=" + e.value);
                } else {
                    objectiveIndexVisible = e.value === 1;
                    sendCommand("action=obj_id_mode&value=" + objectiveIndexVisible);
                }
            }
        }
    },<%End If%><%If CurrentPageID <> _PGID_STRUCTURE_ALTERNATIVES Then%>
    {
        location: 'before',
        locateInMenu: 'never',
        widget: 'dxCheckBox',
        visible: true,
        //"~/App_Themes/EC09/images/ch2alts_20.png"
        options: {
            text: "<%=ResString("btnShowAlternatives")%>",
            showText: true,
            width: 140,
            value: <%=Bool2JS(ShowAlternatives)%>,
            disabled: false,
            elementAttr: {id: 'btn_show_alternatives'},
            onValueChanged: function (e) {
                //checked = e.value;
                if (!is_manual) onClickToolbar("show_alternatives");
            }
        }
    },<%End If%>
    <%If CurrentPageID <> _PGID_STRUCTURE_ALTERNATIVES Then%>
    {
        location: 'before',
        locateInMenu: 'never',
        widget: 'dxButton',
        visible: !isReadOnly && <% = Bool2JS(IsCompleteHierarchyAllowed) %>,
        //"~/App_Themes/EC09/images/ch2plain2_20.png"
        options: {
            icon: "fas fa-sitemap",
            text: "<%=ResString("btnConvertCompleteHierarchyToRegular")%>",
            disabled: isReadOnly,
            elementAttr: {id: 'btn_convert_to_non_complete_hierarchy'},
            onClick: function () {
                onClickToolbar("convert_to_non_complete_hierarchy");
            }
        }
    },<%End If%><%If Not PRJ.IsRisk AndAlso CurrentPageID <> _PGID_STRUCTURE_ALTERNATIVES Then%>
    //{
    //    location: 'before',
    //    locateInMenu: 'never',
    //    template: function(){
    //        return $("<span id='lbl_show_priorities'>Priorities:</span>");
    //    }
    //},
    {
        searchEnabled: false,
        location: 'before',
        locateInMenu: 'always',
        widget: 'dxSelectBox',
        visible: true,
        //"~/App_Themes/EC09/images/assembly-20.png"
        options: {
            icon: "fas fa-calculator",
            valueExpr: "ID",
            value: <%=PM.Parameters.Structure_DisplayPrioritiesMode%>,
            displayExpr: "Text",
            disabled: false,
            text: "Priorities",
            showText: true,
            elementAttr: {id: 'btn_priorities'},
            onValueChanged: function (data) { 
                onChangeSettings(data.value);
            },
            items: [{"ID": 0, "Text": "<%=ResString("optNone")%>"}, {"ID": 1, "Text": "<% =ResString("optLocal")%>"}, {"ID": 2, "Text": "<% =ResString("optGlobal")%>"}, {"ID": 3, "Text": "<% =ResString("optBothLocalAndGlobal")%>"}] //, {"ID": -1, "Text": "Update"}]
        }
    },
    {
        location: 'before',
        locateInMenu: 'always',
        widget: 'dxButton',
        visible: true,
        showText: "inMenu",
        //"~/App_Themes/EC09/images/reload-20.png"
        options: {
            icon: "refresh",
            text:  "Update",
            hint: "Update",
            disabled: false,
            elementAttr: {id: 'btn_update'},
            onClick: function () {
                onChangeSettings(-1);
            }
        }
    },<%End If%>
    <%--{
        location: 'before',
        locateInMenu: 'never',
        visible: <%=Bool2JS(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES)%>,
        template: function(){
            return $("<span>Priorities:</span>");
        }
    },
    {
        location: 'before',
        locateInMenu: 'never',
        widget: 'dxCheckBox',
        visible: <%=Bool2JS(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES)%>,
        options: {
            text: "Priorities:",
            value: <%=Bool2JS(PM.Parameters.Hierarchy_ShowAlternativesPriorities)%>,
            disabled: false,
            elementAttr: {id: 'btn_show_alternatives_priorities'},
            onValueChanged: function (e) {
                if (!is_manual) onClickToolbar("show_alternatives_priorities", e.value);
            }
        }
    },--%>
    {
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxDropDownButton',
        options: {
            width: "80px",
            elementAttr: { id: 'btn_sort' },
            text: "Sort",   
            icon: "fas fa-sort",
            displayExpr: "text",
            disabled: false,
            useSelectMode: false,
            wrapItemText: false,
            dropDownOptions: {
                width: 200,
            },
            items: [],
            onButtonClick: function (e) {
                var dis = isReadOnly || curPgID != _PGID_STRUCTURE_ALTERNATIVES && !canSortCurrentCluster();
                var items = [{
                    icon: "fas fa-sort-alpha-up",
                    text: "By Name Ascending",
                    disabled: dis,
                    onClick: function () {
                        onClickToolbar('sort_name_asc');
                    }
                }, {
                    icon: "fas fa-sort-alpha-down",
                    text: "By Name Descending",
                    disabled: dis,
                    onClick: function () {
                        onClickToolbar('sort_name_desc');
                    }
                }, {
                    icon: "fas fa-sort-amount-up",
                    text: "By Priority Ascending",
                    disabled: dis,
                    visible: !isRisk,
                    onClick: function () {
                        onClickToolbar('sort_priority_asc');
                    }
                }, {
                    icon: "fas fa-sort-amount-down",
                    text: "By Priority Descending",
                    disabled: dis,
                    visible: !isRisk,
                    onClick: function () {
                        onClickToolbar('sort_priority_desc');
                    }
                }];
                e.component.option('items', items);
            }
        }
    },
    {
        //"~/App_Themes/EC09/images/export-excel-20.png"
        location: 'before',
        locateInMenu: 'auto',
        widget: 'dxDropDownButton',
        visible: curPgID !== _PGID_STRUCTURE_ALTERNATIVES,
        options: {
            width: "94px",
            elementAttr: { id: 'btn_export' },
            text: "Export",  
            icon: "fas fa-save",
            displayExpr: "text",
            useSelectMode: false,
            wrapItemText: false,
            dropDownOptions: {
                width: 250,
            },
            //onOptionChanged: function(e) {
            //    if (e.name === "opened" && e.value) {
            //        e.component.option("items")[3].visible = curPgID !== _PGID_STRUCTURE_ALTERNATIVES && ActiveView == hvGraph;
            //        e.component.repaint();
            //    }
            //},
            items: [{
                icon: "far fa-clipboard",
                visible: curPgID !== _PGID_STRUCTURE_ALTERNATIVES,
                text: "Export Model Structure to Clipboard",
                onClick: function () {
                    onClickToolbar(ACTION_EXPORT_MODEL);
                }
            },
            {
                icon: "fas fa-file-alt",
                visible: curPgID !== _PGID_STRUCTURE_ALTERNATIVES,
                text: "Export Model Structure to TXT File",
                onClick: function () {
                    onClickToolbar(ACTION_EXPORT_MODEL_FILE);
                }
            },
            {
                icon: "fas fa-clipboard-list",
                text: "<%=ResString("btnExportObjectivesToClipboard")%>",
                visible: curPgID != _PGID_STRUCTURE_ALTERNATIVES,
                onClick: function () {
                    onClickToolbar("export_objs_to_clipboard");
                }
            },
            {
                icon: "fas fa-download",
                text: "Save As Image",
                visible: curPgID != _PGID_STRUCTURE_ALTERNATIVES && ActiveView == hvGraph,
                onClick: function () {
                    download(document.getElementById("canvasHierarchy").toDataURL(), "<% = SafeFileName(App.ActiveProject.ProjectName) %>.png", "image/png");
                }
            },
            <%--{
                icon: "fas fa-clipboard-list",
                text: "<%=ResString("btnExportAlternativesToClipboard")%>",
                visible: curPgID == _PGID_STRUCTURE_ALTERNATIVES,
                onClick: function () {
                    onClickToolbar("export_alts_to_clipboard");
                }
            }--%>],
        }
    }];


    function canSortCurrentCluster() {
        var selNode = getSelectedNode();
        var is_alt  = (selNode) && (selNode[IDX_IS_ALT] == 1);
        var is_terminal = (selNode) && (selNode[IDX_IS_TERMINAL] == 1);

        return (selNode) && !is_alt && !is_terminal;
    }

    function initToolbar() {
        $("#toolbar").dxToolbar({
            items: toolbarItemsMain
        });
    }
    /* end Toolbar*/

    /* Splitter */
    function initSplitters() {
        $(".split_left").resizable({
            autoHide: false,
            handles: 'e',
            resize: function(e, ui) {
                resize_custom = function (force_redraw) { };
                //$("#divContentPane").hide();
                $("#divInfodocPane").hide();
                var parent = ui.element.parent();
                var divOne = ui.element;
                var divTwo = ui.element.next();
                divTwo.width(20);
                var remainingSpace = parent.innerWidth() - divOne.outerWidth();
                var divTwoWidth = remainingSpace - 28;
                if (divTwoWidth > 20) { divTwo.width(divTwoWidth); } else { divTwo.width(20); }                
            },
            stop: function(e, ui) {
                infodoc_splitter_size = ui.element.next().outerWidth()+17;
                sendCommand("action=v_splitter_size&value=" + infodoc_splitter_size, false);
                //$("#divContentPane").show();
                $("#divInfodocPane").show();
                setTimeout(function () { resizePage(); resize_custom = function (force_redraw) { resizePage(); } }, 500);                
            }
        });
        $(".ui-resizable-e").addClass("splitter_v");//css({ right: "0px", "border-right" : "2px dashed #dedede" });//.html("|");
    }
    /* end  Splitter */

    /* Add-remove-rename items */
    function EditAttributeValue(index) {
        SelectedItemIndex = index;
        var n = document.getElementById("tbCatName");
        if ((n)) {
            n.value = n.value.trim();
            if (n.value=='') {
                DevExpress.ui.notify(resString("msgEmptyCategoryName"), "error");
            } else {
                var attr = GetSelectedAttr();
                if ((index >= 0) && (index < attr[IDX_ATTR_ITEMS].length)) {
                    var idx = attr[IDX_ATTR_ITEMS][index][IDX_ATTR_ID];
                    attr[IDX_ATTR_ITEMS][index][IDX_ATTR_NAME] = htmlEscape(n.value);
                    sendCommand("action=rename_item&name=" + n.value + "&item=" + index + '&clmn=' + SelectedAttrIndex, false);
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
                DevExpress.ui.notify(resString("msgCreateEmptyCCName"), "error");
            } else {
                sendCommand('action=add_item&name=' + encodeURIComponent(n.value)+'&clmn=' + SelectedAttrIndex, false);
            }
        }
    }

    function DeleteAttributeValue(index) {
        SelectedItemIndex = index;
        dxConfirm(resString("msgSureDeleteCategory"), "sendCommand(\"action=del_item&item=" + index + "&clmn=" + SelectedAttrIndex + "\");", false);
    }

    var attr_order = "";
    var attr_values_order = "";
    // end Alternative Attributes

    function initContextMenu() {
            $("#attr-context-menu").dxContextMenu({
                dataSource: [
                    { id: "copy", text: '<% = ResString("titleCopyToClipboard") %>', icon: "fas fa-copy" },
                    { id: "paste", text: '<% = ResString("titlePasteFromClipboard") %>', icon: "fas fa-paste", visible: !isReadOnly }
                ],
                width: "auto",
                target: "#dummy",
                onItemClick: function(e){
                    if (!e.itemData.items) {
                        if (e.itemData.id === "copy") {
                            $("#tableAlts").dxDataGrid("instance").clearSorting();
                            doCopyToClipboardValues(e.component.option("attrIndex"));
                        }
                        if (e.itemData.id === "paste") {
                            $("#tableAlts").dxDataGrid("instance").clearSorting();
                            doPasteAttributeValues(e.component.option("attrIndex"));
                        }
                    }
                }
            });
        }

    function initPage() {
        var divGraph = document.getElementById("tdGridCanvas");
        var divTree = document.getElementById("divHierarchyTree");
        var divAlts = document.getElementById("divAltsView");

        displayLoadingPanel(true);

        if ((divGraph) && (divTree) && divAlts){            
            if (curPgID != _PGID_STRUCTURE_ALTERNATIVES){
                if (ActiveView == hvGraph) {    
                    //$("#divInfodocPane").show();
                    divTree.style.display = "none";
                    divAlts.style.display = "none";
                    divGraph.style.display = "";
                    initCanvas();
                    setSelectedNode(selectedNodeID)
                    setTimeout(function () { drawHierarchy(); }, 100);
                    setTimeout(function () { resizePage(); }, 1000);
                    setTimeout(function () { resizePage(); }, 2000);
                }
                if (ActiveView == hvTree){   
                    //$("#divInfodocPane").show();
                    divGraph.style.display = "none";
                    divAlts.style.display = "none";
                    divTree.style.display = "";
                    initTree();
                }
            }
            if (curPgID == _PGID_STRUCTURE_ALTERNATIVES){
                //if (is_multiselect) { $("#divInfodocPane").hide(); } else { $("#divInfodocPane").show(); }
                divGraph.style.display = "none";
                divTree.style.display = "none";
                divAlts.style.display = "";
                initAltsDatagrid();                
            }
            //setTimeout(function () { resizePage(); displayLoadingPanel(false); }, 750);
        }

        updateLegend();
        updateToolbar();
    }

    function centerView() {
        var parent = document.getElementById("divCanvas");
        var content = document.getElementById("canvasHierarchy");
        if ((parent) && (content) && parent.scrollWidth > 0 && content.clientWidth > 0) {
            parent.scrollLeft = parent.scrollWidth / 2 - $("#divCanvas").width() / 2;
        }
    }

    function resizePage() {
        if (infodoc_splitter_size < 150) infodoc_splitter_size = 150;
        $("#divInfodocPane").width(infodoc_splitter_size).height(10);
        $("#tableInfodocPane").height(10);
        $("#divCanvas").height(10).width(10);
        $("#divContentPane").width(100);
        $("#divContentPane").width($("#divContentPane").parent().innerWidth() - $("#divInfodocPane").outerWidth() - 16);
        var h = $("#tdGridCanvas").height();
        var w = $("#tdGridCanvas").width();
        $("#divCanvas").height(h).width(w);
        if (curPgID == _PGID_STRUCTURE_ALTERNATIVES)  { 
            resizeDatatable(); 
        } else {
            if (ActiveView == hvGraph) { drawHierarchy(); }
            if (ActiveView == hvTree)  { resizeHierarchyTree(); }
        }
        h = $("#tdGridCanvas").height();
        w = $("#tdGridCanvas").width();
        $("#divInfodocPane").height(h);
        $("#tableInfodocPane").height(h);
    }

    <%--function onCheckDontShow() {
        var dontShow = document.getElementById("cbNoAsk").value;
        if (dontShow) {
            sendCommand("<% = _PARAM_ACTION %>=dont_show");
        }
    }--%>

    function onEditGoalName() { //A1566
        $("#popupContainer1").dxPopup({
            contentTemplate: function() {
                return $("<div class='dx-fieldset'><div class='dx-fieldset-header'></div><div class='dx-field'><div class='dx-field-label'>Goal name</div><div class='dx-field-value'><div id='tbGoalName'></div></div></div><div class='dx-field'><div class='dx-field-label'>Goal description</div><div class='dx-field-value'><div id='tbGoalInfodoc'></div></div></div></div><div style='text-align: center;'><div id='btnOKDlg' style='margin-top:1em;'></div><div id='btnCancelDlg' style='margin-top:1em; margin-left: 4px;'></div></div>");
            },
            width: 425,
            height: 225,
            showTitle: true,
            title: "Edit Goal",
            dragEnabled: true,
            shading: false,
            closeOnOutsideClick: true,
            resizeEnabled: false,
            showCloseButton: true
        });
        $("#popupContainer1").dxPopup("show");
        $("#tbGoalName").dxTextBox({
            value: "Goal",
            width: 240,
            placeholder: "Type goal name here..."
        });
        $("#tbGoalInfodoc").dxTextArea({
            value: "",
            width: 240,
            height: 60,
            placeholder: "Type goal description here..."
        });
        $("#btnOKDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnOK")) %>",
            icon: "",
            type: "normal",
            width: 100,
            onClick: function (e) {
                onSaveGoalName();
                closePopup("#popupContainer1");
            }
        });
        $("#btnCancelDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnCancel")) %>",
            icon: "",
            type: "normal",
            width: 100,
            onClick: function (e) {
                closePopup("#popupContainer1");
            }
        });
        setTimeout(function () { 
            var tb = $("#tbGoalName").dxTextBox('instance');
            tb.focus(); 
            tb.element().find("input").select();
        }, 800);
    }

    function onSaveGoalName() { //A1566
        var sValue = $("#tbGoalName").dxTextBox("instance").option("value");
        var sInfodoc = $("#tbGoalInfodoc").dxTextArea("instance").option("value");
        if ((sValue) && sValue.trim() != "") sendCommand("<% = _PARAM_ACTION %>=edit_goal_name&nodeid=<%=PM.Hierarchy(PM.ActiveHierarchy).Nodes(0).NodeGuidID.ToString%>&value=" + encodeURIComponent(sValue.trim()) + "&infodoc=" + encodeURIComponent(sInfodoc.trim()));
    }
    
    <%--function onSaveGoalNameCompleted() { //A1566
        dxConfirm("<%=ResString("confEditGoalInfodoc")%>", "onEditNodeInfodoc(<%=PRJ.ID%>, <%=PM.Hierarchy(PM.ActiveHierarchy).Nodes(0).NodeID%>, '<%=PM.Hierarchy(PM.ActiveHierarchy).Nodes(0).NodeGuidID.ToString%>');", ";");
    }--%>

    <%--function onEditNodeInfodoc(prjId, nodeId, nodeGuid) { //A1566
        OpenRichEditor('?project=' + prjId + '&type=' + <% =Cint(reObjectType.Node) %> + '&field=infodoc&id=' + nodeId + '&guid=' + nodeGuid);
    }--%>

    function InfodocFrameLoaded(frm) {
        if ((frm) && (frm.style)) { frm.style.backgroundImage='none'; };
    }

    function popupAddModelData() {
        $("#popupContainer").dxPopup({
            contentTemplate: function() {
                return $("<div style='text-align:center; line-height:38px;'><div id='btnAddObjs'></div><div id='btnAddAlts' style='margin:1ex 0px'></div><div id='btnAddFromDS'></div><div id='btnCancelDlg' style='margin-top:1em'></div></div>");
            },
            width: 425,
            height: 200,
            showTitle: true,
            title: curPgID == _PGID_STRUCTURE_ALTERNATIVES ? "<%=ResString("btnAddAlternatives")%>" : "<%=ResString("btnAddObjectives")%>",
            dragEnabled: true,
            shading: false,
            closeOnOutsideClick: true,
            resizeEnabled: false,
            showCloseButton: true
        });
        $("#popupContainer").dxPopup("show");
        if (curPgID != _PGID_STRUCTURE_ALTERNATIVES) {
            $("#btnAddObjs").dxButton({
                text: "<% =JS_SafeString(ResString("cmnuObjectives")) %>",
                icon: "icon ec-hierarchy",
                type: "default",
                width: 400,
                onClick: function (e) {
                    closePopup();
                    onClickToolbar("add_level_below");
                }
            }).after("<br>");
        }
        if (curPgID == _PGID_STRUCTURE_ALTERNATIVES) {
            $("#btnAddAlts").dxButton({
                text: "<% =JS_SafeString(ResString("btnAddAlternativesOrPaste")) %>",
                icon: "icon ec-alts",
                type: curPgID == _PGID_STRUCTURE_ALTERNATIVES ? "default" : "normal",
                width: 400,
                onClick: function (e) {
                    closePopup();
                    onClickToolbar("add_alternatives");
                }
            }).after("<br>");    
        }
        $("#btnAddFromDS").dxButton({
            text: "<%=ResString(If(CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES, "cmnuAlternativesAddFromDataset", "cmnuObjectivesAddFromDataset"))%>",
            icon: "fas fa-database",
            type: "normal",
            width: 400,
            onClick: function (e) {
                closePopup();
                onClickToolbar(ACTION_ADD_FROM_DATASET);
            }
        }).after("<br>");    
        $("#btnCancelDlg").dxButton({
            text: "<% =JS_SafeString(ResString("btnCancel")) %>",
            icon: "",
            type: "normal",
            width: 100,
            onClick: function (e) {
                closePopup();
            }
        });        
    }

    function onGetReport(addReportItem) {
        if (typeof addReportItem == "function") {
            addReportItem({"name": "<% =JS_SafeString(PageTitle(CurrentPageID)) %>", "type": (pgID == <% =_PGID_STRUCTURE_HIERARCHY %> ? "<% =JS_SafeString(ecReportItemType.Objectives.ToString) %>" : "<% =JS_SafeString(ecReportItemType.Alternatives.ToString) %>"), "edit": document.location.href});
        }
    }

    resize_custom = function (force_redraw) { resizePage(); };
    toggleScrollMain();
    
    $(document).ready(function () {
        FONT_CATEGORY_COLOR = $("#divCategorical").css("color");

        initPage();
        centerView();        

        if (!isReadOnly && ((curPgID == _PGID_STRUCTURE_ALTERNATIVES && alts_data.length == 0) || (curPgID != _PGID_STRUCTURE_ALTERNATIVES && nodes_data.filter(function (item) { return item[IDX_IS_ALT] == 0; } ).length == 1))) {
            <% if (Session(_SESS_OPEN_DLG) IsNot Nothing AndAlso CStr(Session(_SESS_OPEN_DLG)).ToLower = "nodesets") Then %><% Session.Remove(_SESS_OPEN_DLG) %>showLoadingPanel(); setTimeout(function () { onClickToolbar(ACTION_ADD_FROM_DATASET); }, 200);<% Else %>popupAddModelData();<% End if %>
        }

        initToolbar(); 
        updateToolbar();
        updateLegend();
        initSplitters();
        initContextMenu();
        <%If CurrentPageID <> _PGID_STRUCTURE_ALTERNATIVES AndAlso PRJ.ProjectStatus = ecProjectStatus.psActive AndAlso PM.Parameters.Hierarchy_WasShownToPM <> "" AndAlso Not Str2Bool(PM.Parameters.Hierarchy_WasShownToPM) AndAlso PM.Hierarchy(PM.ActiveHierarchy).Nodes(0).NodeName.ToLower = "goal".ToLower Then %>
        dxConfirm("<%=ResString("confEditGoalName")%><br><br><br>", "onEditGoalName();", ";");
        sendCommand("action=hierarchy_shown", false);
        <%End If%>

        if (isReadOnly) DevExpress.ui.notify(resString("lblProjectReadOnly"), "warning");

        //setTimeout('menu_setOption(menu_option_alts, false);',  3000);
        //$("#node-menu").dxContextMenu({
        //    dataSource: nodeMenuItems,
        //    width: 120,
        //    target: "#canvasHierarchy",
        //    onItemClick: function(e){
        //        if (!e.itemData.items) {
        //            if (e.itemData.action == "copy") {
        //                dxConfirm(resString("confDuplicateJudgments"), "proceedMoveNode('copy', true);", "proceedMoveNode('copy', false);");
        //            } else {
        //                proceedMoveNode("move", true);
        //            }
        //        }
        //    },
        //    onHidden: function (e) {
        //        //e.component.option("target", "node-menu-target");
        //    },
        //    onShowing: function (e) {
        //        if (!is_drag) e.component.hide(); // e.cancel = true;
        //    }
        //});
        
        //$("#tree-node-menu").dxContextMenu({
        //    dataSource: nodeMenuItems,
        //    width: 120,
        //    target: "#canvasHierarchy",
        //    onItemClick: function(e){
        //        if (!e.itemData.items) {
        //            if (e.itemData.action == "copy") {
        //                dxConfirm(resString("confDuplicateJudgments"), "proceedMoveTreeNode('copy', true);", "proceedMoveTreeNode('copy', false);");
        //            } else {
        //                proceedMoveTreeNode("move", true);
        //            }
        //        }
        //    },
        //    onHidden: function (e) {
        //        //e.component.option("target", "node-menu-target");
        //    }
        //});

        applyStyleDxButtonEC();

        hideLoadingPanel();
    });
</script>

<div id="divCategorical" class="categorical" style="display: none;"></div>

<table border='0' cellspacing="0" class="whole" id='tableHierarchyViewMain'>
    <tr>
        <td valign="top">
            <div id="toolbar" class="dxToolbar"></div>
        </td>
    </tr>
    <tr style='height:100%'>
        <td valign="top">
            <div class="whole">
                <div id="divContentPane" class="split_left splitter_left">
                    <table cellpadding="0" cellspacing="0" border="0" class="whole">
                        <tr><td id="tdContent" class="whole" valign="top">
                            <div id='tdGridCanvas' class='whole' style="position: relative;">
                                <div id='divCanvas' style='overflow:auto; margin: 0px auto; text-align:center; position:relative;'>
                                    <canvas id="canvasHierarchy" width="920" height="220"></canvas>                    
                                </div>                
                            </div>
                            <div id="divHierarchyTree" class="text whole" style="overflow:hidden; display:none;">
                                <ul id="ulHierarchyTree" class="ztree" style="padding: 0px; overflow-x:auto; overflow-y:scroll;"></ul>
                            </div>
                            <div id="divAltsView" class="text whole" style="overflow:hidden; display:none;">  
                                <%--<table id='tableContent' class='text cell-border hover order-column' style="table-layout:fixed;"></table>--%>
                                <div id='tableAlts' style="width: 100%;"></div>
                            </div>
                            </td>
                        </tr>
                        <tr><td>
                            <div id="divLegend" style="margin-top:4px; text-align:center; -moz-user-select: none; -webkit-user-select: none; -ms-user-select:none; user-select:none; -o-user-select: none;" unselectable="on"><nobr>
                                <div id="legendObjs"  style='display:inline;'><img src='<% =ImagePath %>legend-10.png' width='10' height='10' title='' border='0' style='background:#d7e7f7;'>&nbsp;<small><span id="legendObjsText">Single parent node</span></small>&nbsp; &nbsp;</div>
                                <div id="legendMulti" style='display:inline;'><img src='<% =ImagePath %>legend-10.png' width='10' height='10' title='' border='0' style='background:#4c94e0;'>&nbsp;<small>Multiple parent nodes</small>&nbsp; &nbsp;</div>
                                <div id="legendAlts"  style='display:inline;'><img src='<% =ImagePath %>legend-10.png' width='10' height='10' title='' border='0' style='background:#ddffbb;'>&nbsp;<small><%=ResString("lblAlternative")%></small></div>
                            </nobr></div>        
                            </td>
                        </tr>                
                    </table>
                </div>
            
                <!-- Split bar here -->

                <div id="divInfodocPane" style="width: <%=PM.Parameters.Hierarchy_VerticalSplitterSize%>px; height: 85%; display: inline-block; overflow: hidden; vertical-align: top;">
                    <table id="tableInfodocPane" border='0' cellspacing='0' cellpadding='0' class='whole'>
                        <tr style="height:40px">
                            <td class="text" valign="middle" style="background:#fafafa; padding: 2px;"><strong><p align="left" id='tbNodeTitle'></p></strong></td>
                        </tr>
                        <tr style='height: 100%;'>
                            <td style='padding: 8px 0px; height: 100%;' valign="top"><iframe src='' id='frmInfodoc' style='width:100%; height:100%; border-top:1px solid #e0e0e0; border-bottom:0px solid #e0e0e0; border-left:0px; border-right:0px; background:#ffffff;' frameborder='0' onload='if (typeof(InfodocFrameLoaded)!="undefined") InfodocFrameLoaded(this);'></iframe></td>
                        </tr>
                        <tr style='height:32px;'>
                            <td align='right' valign='bottom' style='padding:1ex 0px;'><input type='button' id='btnEditInfodoc' class='button' style='width:10em' value='<% =ResString("btnEditInfodoc") %>' onclick='EditInfodoc();'/></td>
                        </tr>
                    </table>
                </div>

            </div>
        
        </td>
    </tr>
</table>

<div id="divNewNode" style="display:none; text-align:center; padding-bottom: 35px;">
<%--<h6>Add Node:</h6>--%>
<div style="overflow:auto;" class="whole">
    <div id="trMultiEdit"><div id="divNodesGrid" style="min-height: 200px; min-width: 550px;"></div></div>
    <table border='0' cellspacing='2' cellpadding='2' class='text'>
        <tr id="trSingleEdit" style="display: none;"><td valign='top' align='right'><span id='lblNodeName' style='text-align:right; margin-top:4px;'></span></td><td align="left" style="padding-bottom:4px"><textarea rows='15' cols='100' style='height: 150px; border: 1px solid #999999;' id='tbNodeName' onfocus='getNodeName(this.value);' onkeyup='getNodeName(this.value);' onblur='getNodeName(this.value);'></textarea></td></tr>
        <tr id="trAddFromDataset" style="display: none;"><td valign='top' align='right' colspan="2">
            <table>
                <tr><td style="vertical-align:top;width:150px; padding-right: 1em;">
                    <h5><%If CurrentPageID = _PGID_STRUCTURE_ALTERNATIVES Then %><%=ResString("lblAlternativeSets") %><%Else %><%= ResString("lblObjectiveSets") %><%End If %></h5>
                    <div id="divListSet"></div>
                </td><td>
                    <div id="divTreeNodes" style="min-height: 200px; min-width: 200px; overflow: auto"></div>
                </td></tr>
            </table>
        </td></tr>
        <%If App.isRiskEnabled AndAlso PM.ActiveHierarchy = ECHierarchyID.hidLikelihood Then%>
        <tr><td id="tdNodeType" valign='top' colspan="2"><span id='lblNodeType' style='text-align:right; margin-top:4px;'><nobr style="display: none;"><%=ResString("lblObjectiveType")%>:&nbsp;&nbsp;</nobr></span><label><input id="cbCategory" type="checkbox" style="margin-left:0px;" />Categorical</label></td></tr>
        <%End If%>
        <%--<tr><td valign='top' style='padding:6px 0px 0px 0px'></nobr><% = ParseString("Description:") %>&nbsp;&nbsp;</nobr></td><td><textarea rows='4' cols='40' style='width:270px;margin-top:2px;' id='tbNodeDescr' onfocus='getNodeDescr(this.value);' onkeyup='getNodeDescr(this.value);' onblur='getNodeDescr(this.value);'></textarea></td></tr>--%>
        <%--<tr><td colspan='2'><hr /></tr></td>--%>
        <tr style='margin-top:10px;'><td valign='top' colspan="2"><span id='lblParentNames' style='text-align:right; margin-top:4px;'><nobr>Parent Node(s):&nbsp;&nbsp;</nobr></span>
            <div id='lbParentNodes' style='width:100%; overflow-x: hidden; overflow-y:auto; border:0px solid #999999; text-align:left;'></div>
            <div id='divSelParents' class='text small gray' style='margin-top:1ex'><a href='' class='actions' onclick='selectAllParents(true); return false;'><%=ResString("lblSelectAll")%></a>&nbsp;|&nbsp;<a href='' class='actions' onclick='selectAllParents(false); return false;'><%=ResString("lblSelectNone")%></a></div>
        </td></tr>        
        <%--<tr id='hintLabel'><td colspan='2' class='text small'><sup><b>*</b></sup>&nbsp;Press carriage return to add second and third</td></tr>--%>
    </table>
</div>
    <div style="height: 18px;"></div>
    <div style="position: absolute; bottom: 5px; left: 50%; margin-left: -100px; width: 208px;">
        <div id="btnAddNodeSave" style="width: 100px;"></div>
        <div id="btnAddNodeCancel" style="width: 100px;"></div>
    </div>
</div>

<div id="divAttributes" style="display:none; text-align:center">
<h6><%=JS_SafeString(ResString("lblRAColumns"))%>:</h6>
<div style="overflow:auto;" id="pAttributes"><table id="tblAttributes" border='0' cellspacing='0' cellpadding='5' style='width:100%;' class='drag_drop_grid'>
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
<div style="overflow:auto;" id="pAttributesValues"><table id="tblAttributesValues" border='0' cellspacing='0' cellpadding='5' style='width:96%;' class='grid drag_drop_grid'>
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

<div id="divCopyJudgments" style="display:none; text-align:center">
    <table border="0" class="whole text">
        <tr>
            <td align="left" style="width:100%;"><h5 style='text-align:left;'>Copy FROM:</h5></td>        
        </tr>
        <tr style="overflow:hidden;">
            <td align="left" valign="top" style="width:100%;">
                <div class="text" style="margin:0px 21px 2px 0px; text-align:right;"><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearchLeft" type="text" style="width:100px;" value="" onkeyup="onSearchLeft(this.value)" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' /></label></div>
                <div id="divLeftFrom" style="text-align:left; padding:5px; width:90%; height:180px; background-color:white; overflow:auto;"></div>
            </td>
        </tr>
    </table>
</div>

<div id="divPasteJudgments" style="display:none; text-align:center">
    <table border="0" class="whole text">
        <tr>
            <td colspan="2" align="left" style="width:100%;"><h5 style='text-align:left;'>Copy TO:</h5></td>
        </tr>
        <tr style="overflow:hidden;">
            <td align="left" valign="top" style="width:50%;">
                <div class="text" style="margin:0px 21px 2px 0px;"><label style='cursor:default;'><%=ResString("btnDoSearch")%>:&nbsp;<input id="tbSearchRight" type="text" style="width:100px;" value="" onkeyup="onSearchRight(this.value)" autocomplete='off' autocorrect='off' autocapitalize='off' spellcheck='false' /></label></div>
                <div id="divLeftTo" style="text-align:left;padding:5px; width:90%; height:165px; background-color:white; overflow:auto;"></div>
                <div id="divSelectAll" style='white-space:nowrap; text-align:center; display:block; margin:2px;' class='text'><a href='' onclick='selectAllUsersInDlg(true); return false;' class='actions'><%=ResString("lblAll")%></a> | <a href='' onclick='selectAllUsersInDlg(false); return false;' class='actions'><%=ResString("lblSelectNone")%></a></div>
            </td>
            <td align="left" valign="top" style="width:50%;">
                <div style="padding: 1ex 0px;">
                    <nobr><% = ResString("titleAlternativesEditor") %>:</nobr><br />
                    <div id="divRightTo" style="text-align:left;padding:5px; width:90%; height:165px; background-color:white; overflow:auto;"></div>                    
                </div>

                <div id="divSelectAllAlts" style='white-space:nowrap; text-align:center; display:block; margin:2px;' class='text'><a href='' onclick='selectAllAltsInDlg(true); return false;' class='actions'><%=ResString("lblAll")%></a> | <a href='' onclick='selectAllAltsInDlg(false); return false;' class='actions'><%=ResString("lblSelectNone")%></a></div>            
            </td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <center>
                    <fieldset class="legend" style="text-align: left; vertical-align: top;" id="pnlCopyJudgmentsOptions">
                        <legend class="text legend_title">&nbsp;Settings&nbsp;</legend>
                        <input type='radio' class='radio' name='rbMode' checked='checked' id='rbMode0' value="0" /><label for='rbMode0'>Fully rewrite (replace)</label><br/>
                        <input type='radio' class='radio' name='rbMode' id='rbMode1' value="1" /><label for='rbMode1'>Update existing judgments and copy where participant doesn't have judgments</label><br/>
                        <input type='radio' class='radio' name='rbMode' id='rbMode2' value="2" /><label for='rbMode2'>Copy only where participant doesn't already have judgments</label><br/><br/>
                        <input type='checkbox' class='checkbox' id='cbPWOnly' /><label id="lblPWOnly" for='cbPWOnly'>Pairwise judgments only</label>
                    </fieldset>
                </center>
            </td>
        </tr>     
    </table>
</div>

<div id="popupSettings" class="cs-popup"></div>
<div id="attr-context-menu"></div>
<%--<div id="node-menu"></div>
<div id="tree-node-menu"></div>--%>

</asp:Content>