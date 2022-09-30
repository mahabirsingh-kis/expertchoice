<%@ Page Language="VB" Inherits="DiagramViewPage" Title="Diagram View" CodeBehind="DiagramView.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <script type="text/javascript" src="/Scripts/connectingLines.js"></script>
    <style>

        .ui-resizable-e:hover {
        	border-radius: 10px;
        	background-color: #4f4f4f;
            opacity: 0.7;
            width: 16px;
	        right: -8px;
        }

        #mindmap {
            white-space: nowrap;
        }

        #mindmap ul {
        	display: inline-block;
        	vertical-align: middle;            
        	list-style: none;
            text-align: left;
            margin: 0px;
        }
               
        #mindmap li {
        	display: block;
            padding: 5px;
        }

        div.mindmap-node {
        	display: inline-block;
            border: 1px solid #707070;
            padding: 9px 8px;
            background-color: #e0e0e0;
            cursor: pointer;
            min-width: 50px;
            min-height: 14px;
            max-width: 500px;
            white-space: normal;
            text-align: center;
            text-overflow: ellipsis;
            word-wrap: break-word;
            vertical-align: middle;
            -webkit-border-radius: 10px;
            -moz-border-radius: 10px;
            -o-border-radius: 10px;
            -ms-border-radius: 10px;
            -khtml-border-radius: 10px;
            border-radius: 10px;
            box-shadow: rgba(7, 7, 7, 0.4) 2px 2px 2px 0px;
        }

        div.node-font {
            color: #4f4f4f;
            font-weight: 700;
            font-size: 13px;
        }

        div.mindmap-root-node {
            color: white;
            font-size: 14px;
            background-color: #6e675d;
        }

        .node-path {
            background-color: #f0f0f0;
            text-align: left;
            font-weight: normal;
            padding: 2px;
            margin: 2px 2px 10px 2px;
        }

    </style>
    <script language="javascript" type="text/javascript">
        var isReadOnly = <% = Bool2JS(App.IsActiveProjectStructureReadOnly OrElse PRJ.ProjectStatus = ecProjectStatus.psMasterProject) %>;
        var allowItemResizing = true; 

        var tree_nodes_data = <% = Api.get_hierarchy_nodes_data(If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, PM.ActiveObjectives, PM.ActiveAlternatives)) %>;
        var tree_nodes_data_contributions = [];

        var selectedNodeID = "<% = If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, SelectedNodeID.ToString, SelectedEventID.ToString) %>";
        var CanShowUpperLevelNodes = <% = Bool2JS(CanShowUpperLevelNodes) %>;
        
        var root_node_data = <% = GetSelectedEventData() %>;
        var child_nodes_data = <% = GetNodesData() %>;

        var tSplitterSize = sessionStorage.getItem("SynthesisSplitterSize<%=App.ProjectID%>");
        var treeSplitterSize = typeof tSplitterSize != "undefined" && tSplitterSize != null && tSplitterSize != "" ? tSplitterSize*1 : 200;
        
        /* Ajax */
        function sendCommand(params) {
            showLoadingPanel();
            _ajax_ok_code = syncReceived;
            _ajax_error_code = syncError;
            _ajaxSend(params);
        }

        function syncReceived(params) {
            if ((params)) {
                var rd = JSON.parse(params);
                //var rd = eval(params);

                if (rd[0] == "show_upper_level_nodes") {    
                    $(".node-path").toggle(CanShowUpperLevelNodes);
                    updateLines();
                }
            
                if (rd[0] == "get_contributed_nodes") {
                    tree_nodes_data_contributions = rd[1];
                    initTree("divDialogTree", tree_nodes_data_contributions, true);
                }

                if (rd[0] == "selected_node" || rd[0] == "remove_contribution") {    
                    root_node_data = rd[1];
                    selectedNodeID = root_node_data.id;
                    child_nodes_data = rd[2];
                    $("#lblPageTitle").html(rd[4]); // title
                    initMindMap();
                }

                if (rd[0] == "add_events" || rd[0] == "add_objectives") {    
                    root_node_data = rd[1];
                    selectedNodeID = root_node_data.id;
                    child_nodes_data = rd[2];
                    tree_nodes_data = rd[3];
                    initTree("divTree", tree_nodes_data, true);
                    $("#lblPageTitle").html(rd[4]); // title
                    initMindMap();
                }
            }
            hideLoadingPanel();
        }

        function syncError() {
            hideLoadingPanel();
            dxDialog("<% =ResString("ErrorMsg_ServiceOperation") %>", ";", undefined, "Error");
        }
        /* end Ajax */

        /* Actions */    
        var popupAddEvent = null, 
            popupAddEventOptions = {
                animation: null,
                width: 750,
                height: 300,
                contentTemplate: function() {
                    return  $("<div></div>").append(
                        $("<div class='note'><% = ParseString("Add new %%events%% here (press carriage return to add second and third):") %></div>"),
                        $("<div id='tbEventNames' style='display: block; margin: 0px 2px 2px 2px;'></div>"),
                        $("<br>"),
                        $("<center>").append($("<div id='btnAddEventOK' style='margin: 2px;'></div>"), $("<div id='btnAddEventCancel' style='margin: 2px;'></div>"))
                    );
                },
                showTitle: true,
                onShown: function (){
                    dxTextAreaFocus("tbEventNames");
                },
                title: "<%=ParseString("Add %%Alternatives%%")%>",
                dragEnabled: true,
                closeOnOutsideClick: false
            };
        
        function addEvent() {
            if (popupAddEvent) $(".popupAddEvent").remove();
            var $popupAddEventContainer = $("<div></div>").addClass("popupAddEvent").appendTo($("#popupAddEvent"));
            popupAddEvent = $popupAddEventContainer.dxPopup(popupAddEventOptions).dxPopup("instance");
            popupAddEvent.show();
            
            $("#btnAddEventOK").dxButton({
                text: "<% = ResString("btnOK") %>",
                icon: "fas fa-check",
                width: 100,
                onClick: function() {
                    var tb = $("#tbEventNames").dxTextArea("instance");
                    if ((tb) && tb.option("value").trim() != "") {
                        sendCommand("action=add_events&names=" + tb.option("value"));
                    }
                    popupAddEvent.hide();
                }
            });

            $("#btnAddEventCancel").dxButton({
                text: "<% = ResString("btnCancel") %>",
                icon: "fas fa-ban",
                width: 100,
                onClick: function() {
                    popupAddEvent.hide();
                }
            });

            $("#tbEventNames").dxTextArea({ 
                width: "95%",
                height: 180,
                value: ""
            });
        }

        var popupAddObj = null, 
            popupAddObjOptions = {
                animation: null,
                width: function() { return Math.round($(window).width() * 0.8); },
                height: function() { return Math.round($(window).height() * 0.8); },
                contentTemplate: function() {
                    return  $("<div></div>").append(
                        $("<div class='note' id='lblHeader0'><% = ParseString("Add new %%Objectives%% here (press carriage return to add second and third):") %></div>"),
                        $("<div id='tbObjNames' style='display: block; margin: 0px 2px 2px 2px;'></div>"),
                        $("<div class='note' id='lblHeader1'><% = ParseString("or select existing %%objectives%% below:") %></div>"),
                        $("<div id='divDialogTree' style='margin: 0px 2px 2px 2px;'></div>"),
                        $("<br>"),
                        $("<center>").append($("<div id='btnAddObjOK' style='margin: 2px;'></div>"), $("<div id='btnAddObjCancel' style='margin: 2px;'></div>"))
                    );
                },
                showTitle: true,
                onShown: function (){
                    dxTextAreaFocus("tbObjNames");
                    sendCommand("action=get_contributed_nodes");
                },
                title: "<%=ParseString("Add %%Objectives%%")%>",
                dragEnabled: true,
                closeOnOutsideClick: false
            };
        
        function addObjective() {
            if (popupAddObj) $(".popupAddObj").remove();
            var $popupAddObjContainer = $("<div></div>").addClass("popupAddObj").appendTo($("#popupAddObj"));
            popupAddObj = $popupAddObjContainer.dxPopup(popupAddObjOptions).dxPopup("instance");
            popupAddObj.show();

            var eventName = "";
            for (var i=0; i < tree_nodes_data.length; i++) {
                if (tree_nodes_data[i].id == selectedNodeID) {
                    eventName = tree_nodes_data[i].name;
                }
            }
            
            $("#lblHeader0").html(replaceString("%%event-name%%", eventName, "<% = ParseString("Add new %%Objectives%% for <b style='font-size: larger;'>%%event-name%%</b> here (press carriage return to add second and third):") %>"));
            $("#lblHeader1").html(replaceString("%%event-name%%", eventName, "<% = ParseString("or select existing %%Objectives%% for <b style='font-size: larger;'>%%event-name%%</b> below:") %>"));

            $("#btnAddObjOK").dxButton({
                text: "<% = ResString("btnOK") %>",
                icon: "fas fa-check",
                width: 100,
                onClick: function() {
                    var checked_nodes = "";
                    var selectedRowKeys =  $("#divDialogTree").dxTreeList("instance").option("selectedRowKeys");
                    selectedRowKeys.forEach(function (item, i, arr) { checked_nodes += (checked_nodes === "" ? "" : ",") + item; });

                    var tb = $("#tbObjNames").dxTextArea("instance");
                    sendCommand("action=add_objectives&names=" + tb.option("value").trim() + "&contributed_nodes=" + checked_nodes);
                    popupAddObj.hide();
                }
            });

            $("#btnAddObjCancel").dxButton({
                text: "<% = ResString("btnCancel") %>",
                icon: "fas fa-ban",
                width: 100,
                onClick: function() {
                    popupAddObj.hide();
                }
            });

            $("#tbObjNames").dxTextArea({ 
                width: "95%",
                height: function() { return Math.round($(window).height() * 0.15); },
                value: ""
            });
        }

    //function addObjective() {
    //            Ok: function() {
    //                
    //                dlgAddObj.dialog( "close" );
    //            },
    //}

        /* Tree View */
        function initTree(tree_id, datasource, isModal) {
            var tree = (($("#" + tree_id).data("dxTreeList"))) ? $("#" + tree_id).dxTreeList("instance") : null;
            if (tree !== null) tree.dispose();

            var isDialog = tree_id === "divDialogTree";

            var selectedRowKeys = [];
            datasource.forEach(function (item, i, arr) { if (item.checked) selectedRowKeys.push(item.id); });

            var columns = [];

            //init columns headers                
            //columns.push({ "caption" : "Infodoc", "alignment" : "center", "allowSorting" : false, "allowSearch" : false,  "allowResizing" : true, "fixed" : false, "fixedPosition" : "left", "allowHiding": false, "dataField" : "infodoc" });
            columns.push({ "caption" : "<% = If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, ParseString(If(PM.ActiveHierarchy = EChierarchyID.hidLikelihood, "%%Objectives(l)%%", "%%Objectives(i)%%")), ParseString("%%Alternatives%%")) %>", "alignment" : "left", "allowSorting" : false, "allowSearch" : false,  "allowResizing" : true, "allowHiding": false, "dataField" : "name" });
           
            $("#" + tree_id).removeClass("dx-treelist-withlines").addClass("dx-treelist-withlines").dxTreeList({
                autoExpandAll: true,
                allowColumnResizing: true,
                columnAutoWidth: true,
                height: isModal ? function() { return Math.round($(window).height() * 0.4); } : "100%",
                width: "95%",
                columns: columns,
                columnResizingMode: "widget",
                dataSource: datasource,
                dataStructure: "plain",
                keyExpr: "id",
                parentIdExpr: "pid",
                rootValue: "",
                focusedRowEnabled: !isDialog,
                focusedRowKey: selectedNodeID,
                hoverStateEnabled: true,
                onFocusedRowChanging: function (e) {                
                    if (!isDialog && (e.rows[e.newRowIndex])) {
                        <% If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then %>
                        var id = e.rows[e.newRowIndex].key;
                        var node = e.rows[e.newRowIndex].data;
                        if (!node.isterminal) {
                            e.cancel = true;
                            return false;
                        }
                        <% End If %>
                    }
                },
                onFocusedRowChanged: function (e) {
                    if (!isDialog && (e.row)) { 
                        var id = e.row.key;
                        if (selectedNodeID != id) {
                            selectedNodeID = id;
                            sendCommand("action=selected_node&id=" + id);
                        }
                    }
                },
                onDataErrorOccurred: null,
                repaintChangesOnly: true,
                selectedRowKeys: selectedRowKeys,
                selection: { 
                    allowSelectAll: false,
                    mode: isDialog ? "multiple" : "none",
                },
                onCellPrepared: function(e) {
                    <% If CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then %>
                    if (e.rowType === "data") {
                        if (e.data.iscategory) {
                            e.cellElement.addClass("categorical");
                        }
                        if (!e.data.isterminal) {
                            e.cellElement.css("color", "#999");                    
                        }                        
                    }
                    <% Else %>
                    if (e.rowType === "data") {
                        if (!e.data.isterminal) {
                            e.cellElement.find('.dx-select-checkbox').hide(); //.dxCheckBox("instance");
                        }
                    }
                    <% End If %>
                    getDxTreeListNodeConnectingLinesOnCellPrepared(e);
                },
                showBorders: false,
                showColumnHeaders: !isModal,
                showColumnLines: true,
                showRowLines: false,
                sorting: { mode: "single" },
                stateStoring: { enabled: false },
                visible: true,
                wordWrapEnabled: false
            });
        }

        function initMindMap() {
            $("#mindmap").empty();
            
            window.scrollTo(0, 0);

            var ul = document.createElement("ul");
            ul.setAttribute("ul-data-id", "root-ul");
            ul.style.display = "block !important";
            ul.style.textAlign = "center";
            ul.style.paddingTop = "20px";

            root_node_data.nodes = child_nodes_data;
            var rootDiv = drawMindmapNode(root_node_data, ul, "");

            var rootLabel = document.createElement("div");
            rootLabel.innerText = "<% = If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, ParseString(If(PM.ActiveHierarchy = EChierarchyID.hidLikelihood, "%%Objective(l)%%", "%%Objective(i)%%")), ParseString("%%Alternative%%")) %>";
            if (<% = Bool2JS(PM.ActiveHierarchy = EChierarchyID.hidLikelihood) %> && root_node_data.is_ns) rootLabel.innerText += " with no specific <% = ParseString("%%objective(l)%%") %>";
            rootLabel.style.position = "absolute";
            rootLabel.style.top = "-22px";
            rootLabel.style.color = "#777";
            rootLabel.style.whiteSpace = "nowrap";
            rootLabel.style.left = "50%";
            rootLabel.style.transform = "translateX(-50%)";

            $(rootDiv).prepend(rootLabel);

            $("#mindmap").append(ul);           
            
            resizePage();

            setTimeout(function () { updateLines(); }, 100);
        }

        function updateLines() {
            $("#mindmap").scrollTop(0);
            $("#divConnectingLines").empty();
            $("div.mindmap-node").connectingLines({ container: "#mindmap" });
        }

        function drawMindmapNode(node, ul, pid) {
            var li = document.createElement("li");              
            var div = document.createElement("div");

            if (node.path !== "") {
                var path = document.createElement("div");
                path.className = "node-path";
                path.innerText = node.path; 
                path.style.display = CanShowUpperLevelNodes ? "block" : "none";
                div.appendChild(path);
            }

            var span = document.createElement("span");
            span.innerText = node.name; 
            div.id = "ID_" + node.id.split('-').join("");
            div.style.textAlign = "center";
            div.style.cursor = "default";
            div.appendChild(span);
                       
            div.className = "ID_" + pid + " mindmap-node node-font " + (pid == "" ? " mindmap-root-node " : "");
            div.setAttribute("data-id", node.id);
            div.setAttribute("data-pid", node.pid);

            if (allowItemResizing) {
                $(div).resizable({
                    handles: "e",
                    cancel: ".mindmap-placeholder-node",
                    resize: function( event, ui ) { updateLines(); },
                    stop: function (event, ui) {
                        node.w = Math.round(ui.size.width);
                        div.style.width = node.w + "px";
                    }
                });
            }

            li.appendChild(div);
            

            if ((node.nodes) && node.nodes.length) {
                // create a child UL element
                var child_ul = document.createElement("ul");
                child_ul.setAttribute("ul-data-id", node.id);                
                li.appendChild(child_ul);

                //child_ul.style.display = node.open ? "inline-block" : "none";                
                node.nodes.forEach(function (childNode) {
                    drawMindmapNode(childNode, child_ul, node.id.split('-').join(""));
                });

                if (pid === "") {
                    var rootLabel = document.createElement("div");
                    rootLabel.style.color = "#777";
                    rootLabel.style.fontSize = "14px";
                    rootLabel.style.fontWeight = "bold";
                    rootLabel.style.position = "absolute";
                    rootLabel.style.top = "8px";
                    rootLabel.innerText = "<% = If(CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK, ParseString("%%Alternatives%%"), ParseString(If(PM.ActiveHierarchy = EChierarchyID.hidLikelihood, "%%Objectives(l)%%", "%%Objectives(i)%%"))) %>";                    

                    $(child_ul).find('li:first').prepend(rootLabel);
                }
            }

            // add to the bottom of the list
            ul.appendChild(li);

            return div;
        }

        var toolbarItems = [{
            location: 'before',
            widget: 'dxButton',
            locateInMenu: 'auto',
            disabled: isReadOnly,
            options: {
                icon: "plus",
                text: "New <% = ParseString("%%Alternative%%") %>",
                    onClick: function() {
                        addEvent();
                    }
                }
        }, <% If Not CurrentPageID = _PGID_MAP_DIAGRAM_VIEW_BY_OBJ_RISK Then %>{
                location: 'before',
                widget: 'dxButton',
                locateInMenu: 'auto',
                disabled: isReadOnly,
                options: {
                    icon: "edit",
                    text: "New/Change <% = ParseString("%%Objective%%") %>",
                    onClick: function() {
                        addObjective();
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                visible: true,
                disabled: isReadOnly,
                options: {
                    text: "Show full paths",
                    showText: true,
                    value: CanShowUpperLevelNodes,
                    onValueChanged: function (e) {
                        CanShowUpperLevelNodes = this.checked; 
                        sendCommand("action=show_upper_level_nodes&chk=" + (this.checked));
                    }
                }
            } <% End If %>
        ];        

        function initSplitters() {
            $(".split_left").resizable({
                autoHide: false,
                handles: 'e',
                resize: function(e, ui) {
                    var parent = ui.element.parent();
                    var divTwo = ui.element.next();
                    divTwo.width(10);
                    var remainingSpace = parent.width() - ui.element.outerWidth();
                    var divTwoWidth = remainingSpace; // - (divTwo.outerWidth() - divTwo.width());
                    if (divTwoWidth > 20) divTwo.width(divTwoWidth);
                },
                stop: function(e, ui) {
                    treeSplitterSize = ui.element.outerWidth();
                    //sendCommand("action=v_splitter_size&value=" + treeSplitterSize, false);
                    sessionStorage.setItem("SynthesisSplitterSize<%=App.ProjectID%>", treeSplitterSize);
                    resizePage();
                }
            });
            $(".ui-resizable-e").addClass("splitter_v");
        }
        
        function resizePage(force_redraw) {
            var headerHeight = 0;
            $(".dv-header").each(function (index, item) { headerHeight += $(item).outerHeight(); });

            $(".dv-content").hide();
            $("#trContent").height($("#divDiagramViewMain").height() - headerHeight);
            $(".dv-content").show();

            $("#divTree").hide();

            var contentHeight = $("#trContent").height();
            $(".dv-content").height(contentHeight);
            $("#mindmap").height(contentHeight - 40);
            
            $("#divTree").show();

            $("#mindmap").hide().width($("#divDiagramViewMain").innerWidth() - $("#divTreeContent").outerWidth()).show();

            updateLines();
        }

        resize_custom = resizePage;

        $(document).ready( function () {
            toggleScrollMain();

            $("#toolbar").dxToolbar({
                items: toolbarItems
            });

            initSplitters();
            initTree("divTree", tree_nodes_data, false);

            $("#divTreeContent").width(treeSplitterSize);

            initMindMap();
        });    
    
    </script>

    <div id="divDiagramViewMain" class="whole">
        <div id="toolbar" class="dxToolbar dv-header" style="margin-bottom: 0px;"></div>
        <div id="trContent" style="width: 100%; height: 100px;">
            <div id="divTreeContent" class="split_left splitter_left dv-content" style="float: left; border-spacing: 0px; width: 200px;">
                <div id='divTree' style='margin-right: 8px;'></div>
            </div>
            <div id='divContent' class="dv-content" style='overflow: hidden; vertical-align: top; height: 100%; text-align: center;'>
               <h5 id="lblPageTitle" style="padding-top: 10px; height: 30px;"><% = GetTitle() %></h5>
               <div id='mindmap' style='overflow: auto; vertical-align: top; height: calc(100% - 30px); position: relative; text-align: center;'></div>
            </div>
        </div>
    </div>

    <div id="popupAddEvent" class="cs-popup"></div>
    <div id="popupAddObj" class="cs-popup"></div>

</asp:Content>
