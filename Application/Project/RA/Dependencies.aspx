<%@ Page Language="VB" Inherits="RADependenciesPage" title="Dependencies" Codebehind="Dependencies.aspx.vb" %>
<%@ Import Namespace="Canvas.RAGlobalSettings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script language="javascript" type="text/javascript">

    tooltip_delay_hide = 15000;

    var all_alts_list = <% = LoadAlternativesData() %>;
    var deps_list     = <% = LoadDependenciesData() %>;
    var tp_deps_list  = <% = LoadTPDependenciesData() %>;

    var isReadOnly = <%=Bool2JS(App.IsActiveProjectReadOnly)%>;
    var isTimePeriodsVisible = <%=Bool2JS(isTimePeriodsVisible)%>;
    var isTimePeriodsLagsVisible = <%=Bool2JS(isTimePeriodsLagsVisible)%>;
    
    var dtClearCell = -1;
    var dtClearAll  = -1000;
 
    // Dependency Types
    var dtDependsOn = <%=CInt(RADependencyType.dtDependsOn)%>;         // D / S
    var dtMutuallyDependent = <%=CInt(RADependencyType.dtMutuallyDependent)%>; // M
    var dtMutuallyExclusive = <%=CInt(RADependencyType.dtMutuallyExclusive)%>; // X

    // Time Periods Dependency Types
    var dtConcurrent = <%=CInt(RADependencyType.dtConcurrent)%>; // D / C
    var dtSuccessive = <%=CInt(RADependencyType.dtSuccessive)%>; // S
    var dtLag        = <%=CInt(RADependencyType.dtLag)%>;        // +/- int value

    var UNDEFINED_LAG = <%=Integer.MinValue%>;
    var curDependencyType = <%=CInt(DropdownSelectedDependency)%>;

    var view = <%=If(RA.Scenarios.GlobalSettings.DependenciesView, 1, 0)%>;
    var table = null;

    var HIGHLIGHT_COLOR   = "#ffff99";
    var HIGHLIGHT_DIAGONAL_CELL_COLOR = "#c5cad1"; //"#778899";
    var CELL_BACKGROUND   = "#ffffff"; //"#f0f5fa";
    var HDR_BACKGROUND   = "#ffffff"; //"#f0f0f0";
    var GROUP_COLOR = "#3366cc"; // $mariner
    var ROW_HEADERS_WIDTH  = 200;
    var ROW_HEADERS_HEIGHT = 100;
    var DATA_CELL_WIDTH    =  45;
    var DATA_CELL_HEIGHT   =  32;
    var GRID_FONT_SIZE     =  11;

    // alternative data
    var COL_ID   = 0;
    var COL_NAME = 1;
    var IS_GROUP = 2;

    // dependency data
    var COL_ALT1   = 0;
    var COL_ALT2   = 1;
    var COL_DEP    = 2;
    var COL_EXTRA  = 3; // [Lag, Condition, Upper Bound]

    var is_context_menu_open = false;

    // lag conditions
    var lcEqual = 0;
    var lcLessOrEqual = 1;
    var lcGreaterOrEqual = 2;
    var lcRange = 3;

    function InitPage() {               
        $("#divDataGrid").hide();
        $("#divListView").hide();

        var has_data = true;

        if (!(all_alts_list) || (all_alts_list.length == 0)) {
            has_data = false;
            $("#divDataGrid").html("<h6 style='margin:8em 2em'><%=GetEmptyMessage()%></h6>").css("border", "0px solid #e0e0e0");
            $("#divDataGrid").show();
        }
        
        $("#cbView").dxSwitch("instance").option("value", view == 1);
        
        // hide the Dependecies dropdown in List View
        var dropDownButton = $("#btnDependency");
        if (view == 0) { dropDownButton.prop("disabled", false); } else { dropDownButton.prop("disabled", true); }
        
        if (has_data) {
            switch (view) {
                case 0: //table view                                        
                    //$("#tableDataGrid").empty();
                    $("#divDataGrid").show();                    
                    setTimeout("InitTableView('tableDataGrid');", 100);
                    break;
                case 1: //list view
                    var grid = document.getElementById("tableListView");
                    $("#tableListView").empty();
                    $("#divListView").show();
                    if (!isTimePeriodsVisible) {
                        InitListView(grid, dtDependsOn);
                    } else {
                        InitListView(grid, dtConcurrent);
                        InitListView(grid, dtSuccessive);                        
                    }
                    if (isTimePeriodsLagsVisible) {    
                        InitListView(grid, dtLag);
                    }
                    InitListView(grid, dtMutuallyDependent);
                    InitListView(grid, dtMutuallyExclusive);
                    break;
            }
        }
        onPageResize(); // refresh table size and scroll
        //setTimeout("InitToolBar();",100);
    }

    function getAltByID(altID) {
        if ((all_alts_list)) {
            var all_alts_list_len = all_alts_list.length;
            for (var i = 0; i < all_alts_list_len; i++) {
                var alt = all_alts_list[i];
                if (alt[COL_ID] == altID) return alt
            }
        }
        return null
    }
    
    function InitTableView(gridName) {
        var columns = [];
        var dataset = [];        
            
        //init columns headers        
        //dummy top left cell
        columns.push({ "title" : "<span class='hdr_cell' style='font-size:"+GRID_FONT_SIZE+"px; font-weight:bold; display: inline-block; word-wrap:normal; overflow:hidden;'><% = ResString(If(App.ActiveProject.ProjectManager.IsRiskProject, "lblControls", "tblAlternativeName")) %></span>", "width" : ROW_HEADERS_WIDTH + "px" });
        var all_alts_list_len = all_alts_list.length;

        for (var i=0; i<all_alts_list_len; i++) {                
            var alt = all_alts_list[i];
            columns.push({ "title" : "<span class='hdr_cell no-tooltip " + (alt[IS_GROUP] ? "is-group" : "") + "' id='clmn_hdr_" + alt[COL_ID] + "' style='font-size:" + GRID_FONT_SIZE + "px; font-weight:bold; display: inline-block; word-wrap:normal; overflow:hidden; width:" + (DATA_CELL_WIDTH + 10) + "px;' title='" + htmlEscape(alt[COL_NAME]) + "'>" + ShortString(alt[COL_NAME], 65) + "</span>", "width" : DATA_CELL_WIDTH + "px", "class" : "td_center no-tooltip" });
        }

        var deps_list_len = deps_list.length;

        //init rows (headers and content cells)
        for (var i = 0; i < all_alts_list_len; i++) {               
            var alt = all_alts_list[i];
            var row = [];
            row.push("<span id='row_hdr_" + alt[COL_ID] + "' class='hdr_cell  " + (alt[IS_GROUP] ? "is-group" : "") + " row_hdr_" + alt[COL_ID] + " no-tooltip' style='font-size:" + GRID_FONT_SIZE + "px; display: inline-block; font-weight:bold; word-wrap:normal; overflow:hidden; width:" + ROW_HEADERS_WIDTH  + "px;' title='" + htmlEscape(alt[COL_NAME]) + "'>" + ShortString(alt[COL_NAME], 65) + "</span>");
            for (var j = 0; j < all_alts_list.length; j++) {
                var alt2 = all_alts_list[j];
                var tCell = "";
                if (i == j || (alt[IS_GROUP] && alt2[IS_GROUP])) {
                    // diagonal cells                    
                    row.push("<span class='diag_cell'><div style='height:" + DATA_CELL_HEIGHT + "px;'>&nbsp;</div></span>");
                } else {
                    tCell = setCellInnerHTML(alt[COL_ID], alt2[COL_ID]);
                    row.push(tCell);
                }
                
            }
            dataset.push(row);
        }

        table = $('#' + gridName).DataTable( {
            data: dataset,
            columns: columns,
            destroy: true,
            paging:    false,
            ordering:  false,
            //fixedColumns: {
            //    leftColumns: 1
            //},
            //scrollY: "1px",
            //"sScrollX": "100%",
            //"sScrollXInner": "100%",
            //"bScrollCollapse": false,
            scrollY: 245,
            scrollX: true,
            stateSave: false,
            searching: false,
            info:      false,
            "language" : {"emptyTable" : "<h6 style='margin:2em 10em'><nobr><% =GetEmptyMessage()%></nobr></h6>"}
        });         

        //setTimeout('resizeDatatable(); initFixedColumn();', 100);        
        //setTimeout("onPageResize(); setTimeout('initFixedColumn();', 1000)", 100);
        //setTimeout("onPageResize();", 100);

        //dataTablesDisableAlternateRowColor(table);
        
        $('#'   + gridName).css({ backgroundColor : CELL_BACKGROUND });
        $(".diag_cell").parent().css({ backgroundColor : HIGHLIGHT_DIAGONAL_CELL_COLOR });
        $('table.dataTable thead th').css({ backgroundColor : HDR_BACKGROUND, 'border': '1px solid #dddddd', padding : '2px 2px' });
        $(".dep_cell").parent().css({ backgroundColor : CELL_BACKGROUND });
        $(".hdr_cell").parent().css({ backgroundColor : HDR_BACKGROUND });
        $(".is-group").css({ color : GROUP_COLOR });
        //$('.dataTable thead th').css({ border : '0.5px solid #dddddd;' });        

        $("th").css({ fontSize : "" + GRID_FONT_SIZE + "px" });
        //$("table.dataTable thead th, table.dataTable thead td").css({ padding:"3px 3px;" });
        //$("table.dataTables tbody tr").css({ minHeight: DATA_CELL_HEIGHT + "px;" });

        $(".dep_cell").parent().css({ cursor : "pointer" }).on("click", function () { onCellClick(this) });
        
        //dataTablesDisableAlternateRowColor(table);

        //// highlight column and ro headers on mouseover
        //$(".dep_cell").parent().on("mouseover", function() { this.style.backgroundColor = HIGHLIGHT_COLOR; var r = document.getElementById("row_hdr_"+this.childNodes[0].getAttribute("i")); $(".row_hdr_"+this.childNodes[0].getAttribute("i")).parent().css({backgroundColor:HIGHLIGHT_COLOR}); if ((r)) r.parentElement.style.backgroundColor = HIGHLIGHT_COLOR; var p = document.getElementById("clmn_hdr_"+this.childNodes[0].getAttribute("j")); if ((p)) p.parentElement.style.backgroundColor = HIGHLIGHT_COLOR; if ((r) && (p)) this.title = r.parentElement[text] + " "+"<%=ResString("lblVs") %>"+" " + p.parentElement[text] });
        $(".dep_cell").parent().on("mouseover", function() { 
            this.style.backgroundColor = HIGHLIGHT_COLOR; 
            var i = this.childNodes[0].getAttribute("i");
            var j = this.childNodes[0].getAttribute("j");
            var r = document.getElementById("row_hdr_" + i); 
            $(".row_hdr_" + i).parent().css({backgroundColor:HIGHLIGHT_COLOR}); 
            if ((r)) r.parentElement.style.backgroundColor = HIGHLIGHT_COLOR; 
            var p = document.getElementById("clmn_hdr_" + j); 
            if ((p)) p.parentElement.style.backgroundColor = HIGHLIGHT_COLOR; 
            if ((r) && (p)) {
                this.title = r.parentElement[text] + "\r\n" + p.parentElement[text];
                var dt = [getDependency(i, j), getTPDependency(i, j)]; 
                for (var k = 0; k < dt.length; k++) { 
                    if (dt[k] >= 0) {
                        var depname = getDependencyName(dt[k]); 
                        if (dt[k] == dtDependsOn)  {
                            this.title = r.parentElement[text] + " " + depname + " " + p.parentElement[text];
                        }
                        if (dt[k] == dtMutuallyDependent || dt[k] == dtMutuallyExclusive) {
                            this.title = r.parentElement[text] + " <%=ResString("lblAnd") %> " + p.parentElement[text] + " <%=ResString("lblAre")%> " + depname;
                        }
                        if (dt[k] == dtConcurrent || dt[k] == dtSuccessive)  {
                            this.title = r.parentElement[text] + " " + depname + " " + p.parentElement[text];
                            //if (this.title == "") {
                            //    this.title = r.parentElement[text] + " and " + p.parentElement[text] + " are " + depname;
                            //} else {
                            //    this.title += ", " + depname;
                            //}
                        }
                    }
                }
            }
        }); 
        $(".dep_cell").parent().on("mouseout",  function() { this.style.backgroundColor = CELL_BACKGROUND; $(".row_hdr_"+this.childNodes[0].getAttribute("i")).parent().css({backgroundColor:HDR_BACKGROUND}); var p = document.getElementById("clmn_hdr_"+this.childNodes[0].getAttribute("j")); if ((p)) p.parentElement.style.backgroundColor = HDR_BACKGROUND; });

        $(".dep_cell").parent().unbind("contextmenu").bind("contextmenu", function(event) {
            event.preventDefault();                
            $("div.context-menu").hide().remove();                
            var sMenu = "<div class='context-menu' style='display: none;'>";
            markCellSelected(this);
            var i = this.childNodes[0].getAttribute("i");
            var j = this.childNodes[0].getAttribute("j");
            var dtype = getDependency(i, j);
            var dl = getLagDependency(i, j);
            var lag   = (dtype == dtMutuallyExclusive || dl.length == 0 ? UNDEFINED_LAG : dl[0]);
            var alt = getAltByID(i);
            var alt2 = getAltByID(j);
            var altRow = ShortString(alt[COL_NAME], 35);
            var altCol = ShortString(alt2[COL_NAME], 35);
            if (!isTimePeriodsVisible) {
                sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuClick(\""+i+"\",\""+j+"\", " + dtConcurrent + "); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/d-20.png' alt='D'>" + altRow + "<b> <%=ResString("lblDependsOn") %> </b>" + altCol + "</nobr></div></a>";
            } else {
                sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuClick(\""+i+"\",\""+j+"\", " + dtConcurrent + "); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/dc-20.png' alt='D (<%=ResString("lblRADependencyConcurrent")%>)'>" + altRow + "<b> <%=ResString("lblDependsOn") %> </b>" + altCol + ", <b><%=ResString("lblRADependencyConcurrent")%></b></nobr></div></a>";
                sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuClick(\""+i+"\",\""+j+"\", " + dtSuccessive + "); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/ds-20.png' alt='D (<%=ResString("lblRADependencySuccessive")%>)'>" + altRow + "<b> <%=ResString("lblDependsOn") %> </b>" + altCol + ", <b><%=ResString("lblRADependencySuccessive")%></b></nobr></div></a>";
            }
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuClick(\""+i+"\",\""+j+"\", " + dtMutuallyDependent + "); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/m-20.png' alt='M'>" + altRow + "<b> <%=ResString("lblAnd") %> </b>" + altCol + "<b> <%=ResString("lblAre") %> <%=ResString("lblMutuallyDependent") %></b></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuClick(\""+i+"\",\""+j+"\", " + dtMutuallyExclusive + "); return false;'><div><nobr><img class='context-menu-glyph' src='../../Images/ra/x-20.png' alt='X'>" + altRow + "<b> <%=ResString("lblAnd") %> </b>" + altCol + "<b> <%=ResString("lblAre") %> <%=ResString("lblMutuallyExclusive") %></b></nobr></div></a>";
            if (isTimePeriodsLagsVisible) {
                sMenu += "<input type='button' class='button' value='Set Lag' style='width:100px; margin-left:40px;' onclick='setLag(\"" + i + "\", \"" + j + "\");' ><input type='button' class='button' value='Clear/Reset Lag' style='width:100px; margin-left:5px;' onclick='resetLag(\"" + i + "\", \"" + j + "\");' " + (lag == UNDEFINED_LAG ? "disabled='disabled'" : "") + " >";
            }
            sMenu += "<a href='' class='context-menu-item' onclick='onContextMenuClick(\""+i+"\",\""+j+"\"," + dtClearCell + "); return false;'><div><nobr><i class='fas fa-broom context-menu-glyph'></i><%=ResString("btnClear") %></nobr></div></a>";
            sMenu += "</div>";                
            var x = event.pageX;
            var s = $(sMenu).appendTo("body").css({top: event.pageY + "px", left: x + "px"});
            if ((s)) { var w = s.width();var pw = $("#tblMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

            showMenu("div.context-menu");
        });
        
        onPageResize();
    }


    //function initFixedColumn() {
    //    new $.fn.dataTable.FixedColumns(table, {
    //        "iLeftColumns": 1,
    //        "sHeightMatch" : "auto",
    //        "iRightColumns": 0
    //    } );
    //    
    //    //setTimeout("$('div.DTFC_LeftBodyLiner').each(function (index, obj) {$(obj).height($(obj).height()-20); });", 300);
    //}

    function showListViewItemMenu(event, img, i, j, dtype, row_id) {
        is_context_menu_open = false;
        $(".context-menu").hide().remove();
        var sMenu = "<div class='context-menu' style='display: none;'>";
        sMenu += "<a href='' class='context-menu-item' onclick='editDependency(\"" + i + "\",\"" + j + "\", " + dtype + "," + row_id + "); return false;'><div><nobr><i class='fas fa-pencil-alt'></i>&nbsp;&nbsp;&nbsp;<%=ResString("btnEdit") %>&nbsp;</nobr></div></a>";
        sMenu += "<a href='' class='context-menu-item' onclick='confirmDeleteDependency(\"" + i + "\",\"" + j + "\", " + dtype + "); return false;'><div><nobr><i class='fas fa-eraser'></i>&nbsp;&nbsp;&nbsp;<%=ResString("btnRemove") %>&nbsp;</nobr></div></a>";
        sMenu += "</div>";                
        if ((img)) {
            var rect = img.getBoundingClientRect();
            var x = rect.left + $(window).scrollLeft() + 2;
            var y = rect.top  + $(window).scrollTop()  + 12;
            var s = $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});                        
            if ((s)) { var w = s.width();var pw = $("#tblMain").width(); if ((pw) && (x+w+16>pw) && (x-w-6>0)) s.css({left: (x-w-6) + "px"}); }

            showMenu("div.context-menu");
        }
    }

    function showMenu(mnu_id) {
        $("body").css("overflow", "hidden");

        $(mnu_id).show();        
        setTimeout("$(document).unbind('click').bind('click', function () { closeMenu(); }); is_context_menu_open = true;", 200);
    }

    function closeMenu() {
        if (is_context_menu_open == true) { 
            $(".context-menu").hide(); 
            is_context_menu_open = false; 
        }
        $("body").css("overflow", "auto");
    }

    function InitListView(grid, dep_type) {        
        if ((grid)) {
            grid.style.backgroundColor = "#fff";
            var trHeader = grid.insertRow(grid.rows.length);
            var tHeaderCell = trHeader.insertCell(0); // header
            tHeaderCell.style.border = "0px solid #cccccc";
            //tHeaderCell.style.fontWeight = "bold";
            var action_link = "";
            if (dep_type == dtConcurrent) {
                action_link = "&nbsp;&nbsp;&nbsp;<a href='' class='actions no-tooltip' onclick='onConvertAllDependencies(dtConcurrent, dtSuccessive); return false;' title='Make all of the concurrent dependencies non-concurrent'><i class='fas fa-arrow-down' style='vertical-align: middle;'></i></a>";
            }
            if (dep_type == dtSuccessive) {
                action_link = "&nbsp;&nbsp;&nbsp;<a href='' class='actions no-tooltip' onclick='onConvertAllDependencies(dtSuccessive, dtConcurrent); return false;' title='Make all of the non-concurrent dependencies concurrent'><i class='fas fa-arrow-up' style='vertical-align: middle;'></i></a>";
            }
            tHeaderCell.innerHTML = "<br><h6>" + getDependencyName(dep_type) + action_link + "</h6>";
            if (deps_list) {
                var deps_list_len = deps_list.length;                
                for (var i = 0; i < deps_list_len; i++) {
                    var d = deps_list[i][COL_DEP];
                    var a1 = deps_list[i][COL_ALT1];
                    var a2 = deps_list[i][COL_ALT2];
                    var canShowRow = d == dep_type || (d == dtDependsOn && isTimePeriodsVisible);
                    if (canShowRow && d == dtDependsOn && isTimePeriodsVisible) {
                        var dtp = getTPDependency(a1, a2);
                        canShowRow = (dep_type == dtConcurrent && dtp == dtConcurrent) || (dep_type == dtSuccessive && dtp == dtSuccessive) || (dep_type == dtLag && dtp == dtLag);
                    }

                    if (canShowRow && d == dtDependsOn && dtp == dtLag && !isTimePeriodsLagsVisible) {
                        canShowRow = false;                        
                    }
                    
                    //if (canShowRow && !(dep_type == dtLag && (deps_list[i][COL_EXTRA].length == 0 || deps_list[i][COL_EXTRA][0] == UNDEFINED_LAG))) {
                    if (canShowRow) {
                        var alt1 = getAltByID(a1);
                        var alt2 = getAltByID(a2);                        
                        if ((alt1) && (alt2)) {                            
                            var row_id = grid.rows.length;
                            var tr = grid.insertRow(row_id);
                            tr.setAttribute("data-i", row_id);
                            //tr.onmouseover = function() { var name = "btnDelete_"+this.getAttribute("i"); var btn = document.getElementById(name); if ((btn)) btn.style.visibility = 'visible'; };
                            //tr.onmouseout  = function() { var name = "btnDelete_"+this.getAttribute("i"); var btn = document.getElementById(name); if ((btn)) btn.style.visibility = 'hidden'; };
                            var tCell0 = tr.insertCell(0); //Alt 1
                            tCell0.style.border = "0px solid #cccccc";
                            var btnDel = "&nbsp;<a href='' class='btn_menu_rule' style='cursor:context-menu; padding-left:3px;' onclick='showListViewItemMenu(event, this, \"" + a1 + "\", \"" + a2 + "\"," + dep_type + "," + row_id + "); return false;' oncontextmenu='showListViewItemMenu(event, this, \"" + a1 + "\", \"" + a2 + "\"," + dep_type + "," + row_id + "); return false;' title=''><i class='fas fa-caret-square-down'></i></a>";
                            var alt1name = "&nbsp;<b " + (alt1[IS_GROUP] ? "style='color: " + GROUP_COLOR + "'" : "") + ">&quot;" + ShortString(alt1[COL_NAME], 65) + "&quot;</b>";
                            var alt2name = "&nbsp;<b " + (alt2[IS_GROUP] ? "style='color: " + GROUP_COLOR + "'" : "") + ">&quot;" + ShortString(alt2[COL_NAME], 65) + "&quot;</b>";
                            var depname  = "&nbsp;" + getDependencyName(dep_type);
                            switch (dep_type) {
                                case dtDependsOn:
                                case dtConcurrent:
                                case dtSuccessive:
                                    tCell0.innerHTML = alt1name + depname + alt2name + btnDel;
                                    break;
                                case dtLag:
                                    var dl = getLagDependency(a1, a2);
                                    var lag_val = dl[0];
                                    var lag_cond = dl[1];
                                    var lag_ubound = dl[2];
                                    switch (lag_cond) {
                                        case lcEqual:
                                            if (lag_val == 0) {
                                                tCell0.innerHTML = alt1name + "&nbsp;and&nbsp;" + alt2name + "&nbsp;are&nbsp;Dependent&nbsp;with&nbsp;0&nbsp;lag" + btnDel;
                                            } else {
                                                tCell0.innerHTML = alt1name + "&nbsp;starts&nbsp;<b>exactly&nbsp;" + lag_val + "</b>&nbsp;periods&nbsp;after&nbsp;" + alt2name + "&nbsp;starts" + btnDel;
                                            }
                                            break;
                                        case lcLessOrEqual:                                            
                                            tCell0.innerHTML = alt1name + "&nbsp;starts&nbsp;<b>at&nbsp;most&nbsp;" + lag_val + "</b>&nbsp;periods&nbsp;after&nbsp;" + alt2name + "&nbsp;starts" + btnDel;
                                            break;
                                        case lcGreaterOrEqual:
                                            tCell0.innerHTML = alt1name + "&nbsp;starts&nbsp;<b>at&nbsp;least&nbsp;" + lag_val + "</b>&nbsp;periods&nbsp;after&nbsp;" + alt2name + "&nbsp;starts" + btnDel;
                                            break;
                                        case lcRange:
                                            tCell0.innerHTML = alt1name + "&nbsp;starts&nbsp;<b>at&nbsp;least&nbsp;" + lag_val + "&nbsp;and&nbsp;at&nbsp;most&nbsp;" + lag_ubound + "</b>&nbsp;periods&nbsp;after&nbsp;" + alt2name + "&nbsp;starts" + btnDel;
                                            break;
                                    }
                                    //tCell0.innerHTML = alt1name + "&nbsp;starts&nbsp;<b>" + s + "</b>&nbsp;periods after&nbsp;" + alt2name + "&nbsp;starts" + btnDel;
                                    break;
                                default:
                                    tCell0.innerHTML = alt1name + "&nbsp;<%=ResString("lblAnd") %>" + alt2name + "&nbsp;<%=ResString("lblAre") %>" + depname + btnDel;
                                    break;
                            }
                        }
                    }
                }
            }
            var footer_index = grid.rows.length;
            var trFooter = grid.insertRow(footer_index);
            var tFooterCell = trFooter.insertCell(0); // footer
            tFooterCell.style.border = "0px solid #cccccc";
            addButtonUI(footer_index, dep_type);
        }
    }
    
    function addButtonUI(rowIndex, dep_type) {
        $('#current_cell').hide(0);

        var grid = document.getElementById("tableListView");
        if ((grid) && (rowIndex < grid.rows.length)) {
            var cell = grid.rows[rowIndex].cells[0];
            var btnAdd   = "&nbsp;<a href='' class='dt_"+dep_type+" btn_add_rule' onclick='addDependencyUI("+rowIndex+","+dep_type+", false,\"\",\"\"); return false;' title='<%=ResString("btnAdd") %>'><i class='fas fa-plus' style='color: #4b8930;'></i></a>"
            cell.innerHTML = btnAdd;
        }
        
        $(".btn_add_rule").show(); //prop("disabled", false);
        $(".btn_menu_rule").show(); //prop("disabled", false);

        $('#current_cell').fadeIn(500);
    }

    function addDependencyUI(rowIndex, dep_type, is_editing, alt1id, alt2id) {
        $(".btn_add_rule").hide(); //prop("disabled", true);
        $(".btn_menu_rule").hide(); //prop("disabled", true);

        if ($("#cbAlt1").data("dxSelectBox")) $("#cbAlt1").dxSelectBox("instance").dispose();
        if ($("#cbAlt2").data("dxSelectBox")) $("#cbAlt2").dxSelectBox("instance").dispose();

        var grid = document.getElementById("tableListView");
        if ((grid) && (rowIndex < grid.rows.length)) {
            var cell = grid.rows[rowIndex].cells[0];
            cell.setAttribute('id', 'current_cell');
            $('#current_cell').hide();
            var btnApply = "&nbsp;<button type='button' class='button btn_apply_rule' onclick='applyDependency(" + dep_type + ", \"" + alt1id + "\",\"" + alt2id + "\"); return false;' title='<%=ResString("btnApply") %>' style='width: 45px;'><i class='fas fa-check' style='color: #4b8930;'></i></button>";
            var btnCancel= "&nbsp;<button type='button' class='button btn_cancel_rule' onclick='" + (is_editing ? "InitPage();" : "addButtonUI("+rowIndex+","+ dep_type+");") + " return false;' title='<%=ResString("btnCancel") %>' style='width: 45px;'><i class='fas fa-ban' style='color: #c41919;'></i></button>";
            // fill in the first combobox
            var cb_width = (dep_type == dtLag ? 100 : 230);
            var select1 = "&nbsp;<div style='width:" + cb_width + "px; display: inline-block; vertical-align: middle;' id='cbAlt1'></div>";
            var select1_options = [{ "value" : "", "text" : "" }];
            select1_SelectedValue = is_editing ? alt1id : "";
            var all_alts_list_len = all_alts_list.length;
            for (var i = 0; i < all_alts_list_len; i++) { 
                var alt = all_alts_list[i];
                select1_options.push({ "value" : alt[COL_ID], "text" : (alt[IS_GROUP] ? "[" : "") + alt[COL_NAME] + (alt[IS_GROUP] ? "]" : ""), "color" : alt[IS_GROUP] ? GROUP_COLOR : "undefined" });
            }
            // fill in the second combobox
            var select2 = "&nbsp;<div style='width:" + cb_width + "px; display: inline-block; vertical-align: middle;' id='cbAlt2'></div>";
            var depname = "&nbsp;" + getDependencyName(dep_type);

            // "lag" UI
            var uiLag = "";
            if (dep_type == dtLag) {
                //uiLag = "&nbsp;<input type='number' min='0' style='width:80px; text-align:right;' disabled='disabled' id='tbLag' value='0' >";
                var s = "<select id='cbCond' style='width:70px; margin-left:5px; margin-right:5px;' onchange='cbCondChange(this.value);'><option value='0'>=</option><option value='1'>&lt;=</option><option value='2'>&gt;=</option><option value='3'>Interval</option></select>";
                s += "<span id='spanLeft' style='display:none;'>[&nbsp;</span>";
                s += "<input id='tbLag'       type='number' min='0' style='text-align:right;width:70px;' value='0' >";        
                s += "<span id='spanMiddle' style='display:none;'>&nbsp;:&nbsp;</span>";
                s += "<input id='tbLagUbound' type='number' min='0' style='text-align:right;width:70px;display:none;' value='0' >";
                s += "<span id='spanRight' style='display:none;'>&nbsp;]</span>";
                uiLag = s;
            }
            
            // fill in the line
            switch (dep_type) {
                case dtDependsOn:
                case dtConcurrent:
                case dtSuccessive:
                    cell.innerHTML = select1 + depname  + select2 + btnApply + btnCancel;
                    break;
                case dtLag:
                    cell.innerHTML = select1 + "&nbsp;starts&nbsp;" + uiLag + "&nbsp;periods after&nbsp;"+ select2 + "&nbsp;starts.&nbsp;" + btnApply + btnCancel;
                    break;
                default:
                    cell.innerHTML = select1 + "&nbsp;<%=ResString("lblAnd") %>" + select2 + "&nbsp;<%=ResString("lblAre") %>" + depname + btnApply + btnCancel;
                    break;
            };

            $("button.btn_apply_rule").prop('disabled', true);
            $('#current_cell').fadeIn(500);

            $("#cbAlt1").dxSelectBox({ 
                width: cb_width,
                //dataSource: select1_options,
                dataSource: { 
                    paginate: true,
                    store: {
                        type: "array",
                        data: select1_options,
                        key: "value"
                    }
                },
                disabled: isReadOnly,
                displayExpr: "text",
                deferRendering: true,
                searchEnabled: true,
                value: select1_SelectedValue,
                valueExpr: "value",
                onValueChanged: function (e) {
                    onSelectAlt1(e.value, dep_type, "");
                },
                wrapItemText: false
            });
            
            $("#cbAlt2").dxSelectBox({ 
                width: cb_width,
                disabled: true,
                dataSource: { 
                    paginate: true,
                    store: {
                        type: "array",
                        data: [],
                        key: "value"
                    }
                },
                displayExpr: "text",
                deferRendering: true,
                searchEnabled: true,
                valueExpr: "value",
                onValueChanged: function (e) {
                    onSelectAlt2(e.value);
                },
                wrapItemText: false
            });


            if (is_editing) {
                onSelectAlt1(alt1id, dep_type, alt2id);
                $("#cbAlt2").dxSelectBox("instance").option("disabled", false);
                
                if (dep_type == dtLag) {
                    var dl = getLagDependency(alt1id, alt2id);
                    document.getElementById("tbLag").value = (dl) && dl.length == 0 ? 0 : dl[0];
                    document.getElementById("cbCond").value = (dl) && dl.length == 0 ? 0 : dl[1];
                    cbCondChange(document.getElementById("cbCond").value);
                    document.getElementById("tbLagUbound").value = (dl) && dl.length == 0 ? 0 : dl[2];
                } else {
                    $("button.btn_apply_rule").prop('disabled', true);
                }
            }
        }
    }

    function applyDependency(dep_type, alt1id, alt2id) {
        var i = $("#cbAlt1").dxSelectBox("instance").option("value");
        var j = $("#cbAlt2").dxSelectBox("instance").option("value");
        
        var lag_value = (dep_type == dtLag ? parseInt(document.getElementById("tbLag").value) : UNDEFINED_LAG);
        var condition = (dep_type == dtLag ? parseInt(document.getElementById("cbCond").value) : UNDEFINED_LAG);
        var ubound    = (dep_type == dtLag ? parseInt(document.getElementById("tbLagUbound").value) : UNDEFINED_LAG);

        deleteDependency(i, j);

        switch (dep_type*1) {
            case dtDependsOn:
            case dtConcurrent:
                deps_list.push([i, j, dtDependsOn, []]);
                tp_deps_list.push([i, j, dtConcurrent, []]);
                break;
            case dtSuccessive:
                deps_list.push([i, j, dtDependsOn, []]);
                tp_deps_list.push([i, j, dtSuccessive, []]);
                break;
            case dtLag:
                deps_list.push([i, j, dtDependsOn, []]);
                tp_deps_list.push([i, j, dep_type, [lag_value, condition, ubound]]);
                break;
            default:
                deps_list.push([i, j, dep_type, []]);
                break;
        }        
        if (dep_type == dtDependsOn) dep_type = dtConcurrent;
        sendCommand("action=set&i=" + i + "&j=" + j + "&type=" + dep_type + "&value=" + lag_value + "&condition=" + condition + "&ubound=" + ubound + "&ex_alt1_id=" + alt1id + "&ex_alt2_id=" + alt2id);
    }

    function onSelectAlt1(alt1_id, dep_type, alt2_id) {       
        var valid_alt_id = (alt1_id) && alt1_id !== null && alt1_id !== "";
        $("#cbAlt2").dxSelectBox("instance").option('disabled', !valid_alt_id);
        $("button.btn_apply_rule").prop('disabled', !valid_alt_id);
        $("#cbCond").prop('disabled', !valid_alt_id);
        $("#tbLag").prop('disabled', !valid_alt_id);
        $("#tbLagUbound").prop('disabled', !valid_alt_id);

        if (valid_alt_id) { 
            var alternative1 = getAltByID(alt1_id);
            var cbAlt2 = $("#cbAlt2").dxSelectBox("instance");
            if ((cbAlt2) && (deps_list)) {
                var select2_options = [];                
                var all_alts_list_len = all_alts_list.length;
                for (var i = 0; i < all_alts_list_len; i++) {
                    var alt = all_alts_list[i];
                    if (alt1_id != alt[COL_ID] && (!alternative1[IS_GROUP] || !alt[IS_GROUP])) {
                        var has_no_dependency = true;                    
                        
                        if (dep_type == dtLag) {
                            var dl = getLagDependency(alt1_id, alt[COL_ID]);
                            if (dl.length > 0) has_no_dependency = false;
                        } else {
                            var d = getDependency(alt1_id, alt[COL_ID]);
                            if (d >= 0) has_no_dependency = false;
                        }
                        
                        if (has_no_dependency || alt[COL_ID] == alt2_id) {
                            select2_options.push({ "value" : alt[COL_ID], "text" : (alt[IS_GROUP] ? "[" : "") + alt[COL_NAME] + (alt[IS_GROUP] ? "]" : ""), "color" : alt[IS_GROUP] ? GROUP_COLOR : "undefined" });
                        }
                    }
                }

                cbAlt2.option("dataSource.store.data", select2_options);                

                if (select2_options.length == 0) {
                    cbAlt2.option("disabled", true);
                    $("button.btn_apply_rule").prop('disabled', true);
                } else {
                    cbAlt2.option("value", alt2_id);
                }
            }
        }
    }

    function onSelectAlt2(id) {
        $("button.btn_apply_rule").prop('disabled', id == "");
        $("#cbCond").prop('disabled', false);
        $("#tbLag").prop('disabled', false);
        $("#tbLagUbound").prop('disabled', false);
    }

    function confirmDeleteDependency(i, j, dtype) {
        dxConfirm(resString("msgSureRemoveDependency"), "deleteDependency(\"" + i + "\",\"" + j + "\"); sendCommand(\"action=del&i=" + i + "&j=" + j + "&type=-1\");");
    }

    function editDependency(i, j, dtype, row_id) {        
        $("tr[data-i='" + row_id + "']>td").html("");
        addDependencyUI(row_id, dtype, true, i, j);
    }

    function markCellSelected(cell) {
        $(".dep_cell").parent().css({"margin" : "0px", "border" : ""});
        cell.style.margin = "2px";
        cell.style.border = "2px solid " + COLOR_SELECTION_BORDER;
    }

    function onCellClick(sender) {
        markCellSelected(sender);
        var i = sender.childNodes[0].getAttribute("i");
        var j = sender.childNodes[0].getAttribute("j");
        if (!is_context_menu_open) {           
            setCellDependency(i, j, curDependencyType);           
        };
        is_context_menu_open = false;
    }

    function onDependency_Menu() {        
        is_context_menu_open = false;        
        var sender = document.getElementById("btnDependency");
        if ((sender) && !isReadOnly) {
            var rect = sender.getBoundingClientRect();
            var x = rect.left   + $(window).scrollLeft() + 0;
            var y = rect.bottom + $(window).scrollTop()  + 1;

            $("div.context-menu").hide().remove();                
            var sMenu = "<div class='context-menu' style='display: none;'>";
            
            if (!isTimePeriodsVisible) {
                sMenu += "<a href='' class='context-menu-item' data-value='3' onclick='onClickToolbar(3); return false;' title='<%=ResString("lblDependsOnHint")%>'><div><nobr><img src='../../Images/ra/d-20.png' width='20' height='20' style='vertical-align: middle;' >&nbsp;<%=ResString("lblDependsOn")%></nobr></div></a>";
            } else {
                sMenu += "<a href='' class='context-menu-item' data-value='3' onclick='onClickToolbar(3); return false;' title='<%=ResString("lblDependsOnHint")%>. Can be cuncurrent'><div><nobr><img src='../../Images/ra/dc-20.png' width='20' height='20' style='vertical-align: middle;' >&nbsp;<%=ResString("lblDependsOn") + " (" + ResString("lblRADependencyConcurrent") + ")"%></nobr></div></a>";
                sMenu += "<a href='' class='context-menu-item' data-value='4' onclick='onClickToolbar(4); return false;' title='<%=ResString("lblDependsOnHint")%>. Non-concurrent'><div><nobr><img src='../../Images/ra/ds-20.png' width='20' height='20' style='vertical-align: middle;' >&nbsp;<%=ResString("lblDependsOn") + " (" + ResString("lblRADependencySuccessive") + ")"%></nobr></div></a>";
            }
            sMenu += "<a href='' class='context-menu-item' data-value='1' onclick='onClickToolbar(1); return false;' title='<%=ResString("lblMutuallyDependentHint")%>'><div><nobr><img src='../../Images/ra/m-20.png' width='20' height='20' style='vertical-align: middle;' >&nbsp;<%=ResString("lblMutuallyDependent")%></nobr></div></a>";
            sMenu += "<a href='' class='context-menu-item' data-value='2' onclick='onClickToolbar(2); return false;' title='<%=ResString("lblMutuallyExclusiveHint")%>'><div><nobr><img src='../../Images/ra/x-20.png' width='20' height='20' style='vertical-align: middle;' >&nbsp;<%=ResString("lblMutuallyExclusive")%></nobr></div></a>";
            sMenu += "<hr color='#c0c0c0' style='margin: 3px;'></hr>";
            sMenu += "<a href='' class='context-menu-item' data-value='-1' onclick='onClickToolbar(-1); return false;' title='<%=ResString("btnClearCell")%>'><div><nobr><i class='fas fa-eraser' style='vertical-align: middle;'></i>&nbsp;<%=ResString("btnClearCell")%></nobr></div></a>";
            sMenu += "</div>";                

            $(sMenu).appendTo("body").css({top: y + "px", left: x + "px"});
            
            showMenu("div.context-menu");
        }
    }

    function updateToolbarDropDown() {
        var dropDownButton = document.getElementById("btnDependency");
        var button = $(".context-menu-item[data-value='" + (curDependencyType == dtDependsOn ? dtConcurrent : curDependencyType) + "']");
        if ((dropDownButton) && (button) && button.length > 0) {
            dropDownButton.innerHTML = "";
            button.children().first().clone().appendTo($(dropDownButton));
            dropDownButton.title = htmlEscape(button.prop("title"));
            //var img = document.getElementById("imgDependency");
            //if ((img)) dropDownButton.set_imageUrl(button.get_imageUrl());
        }
    }

    function onContextMenuClick(i,j, dType) {
        curDependencyType = dType*1;
        onDependency_Menu();
        is_context_menu_open = true;
        updateToolbarDropDown();
        closeMenu();
        $(".context-menu").hide();
        setCellDependency(i, j, dType);
    }

    function hasDependency(dep_type) {
        var deps_list_len = deps_list.length;
        for (var k = 0; k < deps_list.length; k++) {   
            var d = deps_list[k][COL_DEP];
            var a1 = deps_list[k][COL_ALT1];
            var a2 = deps_list[k][COL_ALT2];
            if (d == dep_type) {
                if ((d == dtConcurrent || d == dtSuccessive) && getDependency(a1,a2) == dtDependsOn) {
                    return true;
                }
                if (d == dtDependsOn && getTPDependency(a1,a2) != -1) {
                    return true;
                }
            }
        }
        return false;
    }

    function getDependency(i, j) {
        var deps_list_len = deps_list.length;        
        for (var k = 0; k < deps_list.length; k++) {   
            var d = deps_list[k][COL_DEP];
            var a1 = deps_list[k][COL_ALT1];
            var a2 = deps_list[k][COL_ALT2];
            if ((a1 == i && a2 == j) || ((d == dtMutuallyDependent || d == dtMutuallyExclusive) && a1 == j && a2 == i)) {
                return d;
            }                
        }
        return -1;
    }

    function getTPDependency(i, j) {
        var tp_deps_list_len = tp_deps_list.length;        
        for (var k = 0; k < tp_deps_list.length; k++) {
            var d = tp_deps_list[k][COL_DEP];
            var a1 = tp_deps_list[k][COL_ALT1];
            var a2 = tp_deps_list[k][COL_ALT2];
            if (a1 == i && a2 == j) {
                return d;
            }
        }
        return -2;
    }

    function getLagDependency(i, j) {
        var tp_deps_list_len = tp_deps_list.length;        
        for (var k = 0; k < tp_deps_list.length; k++) {
            var d = tp_deps_list[k][COL_DEP];
            var a1 = tp_deps_list[k][COL_ALT1];
            var a2 = tp_deps_list[k][COL_ALT2];
            if (d == dtLag && a1 == i && a2 == j) return tp_deps_list[k][COL_EXTRA];
        }
        return [];
    }

    function deleteDependency(i, j) {
        deleteDep(deps_list, i, j);
        deleteDep(tp_deps_list, i, j);        
    }

    function deleteDep(lst, i, j) {
        var lst_len = lst.length;
        var k = 0;
        while (k < lst.length) {
            var a1 = lst[k][COL_ALT1];
            var a2 = lst[k][COL_ALT2];
            if ((a1 == i && a2 == j) || (a1 == j && a2 == i)) {
                lst.splice(k, 1);
            } else {
                k++;
            }
        }
    }

    function setLag(i, j) {
        var dl = getLagDependency(i, j);
        var currentLag = dl.length == 0 ? UNDEFINED_LAG : dl[0];
        var sValue = (currentLag == UNDEFINED_LAG ? "" : currentLag + "");
        
        var currentCond = dl.length == 0 ? 0 : dl[1];
        
        var currentUbound = dl.length == 0 ? UNDEFINED_LAG : dl[2];
        var sUbound = (currentUbound == UNDEFINED_LAG ? "" : currentUbound + "");
        
        var s = "<span style='margin-left:15px;'><% = JS_SafeString(ResString("lblRADependencyLag")) %>:&nbsp;</span>";
        s += "<select id='cbCond' style='width:70px; margin-left:5px; margin-right:5px;' onchange='cbCondChange(this.value);'><option value='0'>=</option><option value='1'>&lt;=</option><option value='2'>&gt;=</option><option value='3'>Interval</option></select>";
        s += "<span id='spanLeft' style='display:none;'>[&nbsp;</span>";
        s += "<input id='tbLag'       type='number' min='0' style='text-align:right;width:70px;' value='" + sValue + "'>";        
        s += "<span id='spanMiddle' style='display:none;'>&nbsp;:&nbsp;</span>";
        s += "<input id='tbLagUbound' type='number' min='0' style='text-align:right;width:70px;display:none;' value='0'>";
        s += "<span id='spanRight' style='display:none;'>&nbsp;]</span>";

        var myDialog = DevExpress.ui.dialog.custom({
                title: "<% = JS_SafeString(ResString("titleEditLag")) %>",
                messageHtml: s,
                buttons: [{ text: resString("btnOK"), onClick : function () { return true; }}, {text: resString("btnCancel"), onClick : function () { return false; }}]
            });
            myDialog.show().done(function(dialogResult){
                if (dialogResult) {
                    onSaveLag(i, j , parseInt($('#tbLag').val()), parseInt($('#cbCond').val()), parseInt($('#tbLagUbound').val()));
                }
            });


        $("#cbCond").val(currentCond);
    }

    function cbCondChange(value) {
        if (value == lcRange) {            
            $("#tbLagUbound").show();
            $("#spanLeft").show();
            $("#spanMiddle").show();
            $("#spanRight").show();
        } else {
            $("#tbLagUbound").hide();
            $("#spanLeft").hide();
            $("#spanMiddle").hide();
            $("#spanRight").hide();
        }
    }

    function onSaveLag(i, j, value, condition, ubound) {        
        if (!isNaN(value) && (condition != lcRange || !isNaN(ubound))) {
            var d = getLagDependency(i, j);
        
            var currentLag = UNDEFINED_LAG;
            var curCondition = 0;
            var curUbound = 0;

            if (d.length > 0) {
                currentLag = d[0];
                curCondition = d[1];
                curUbound = d[2];
            }

            if (currentLag != value || curCondition != condition || curUbound != ubound) {
                deleteDependency(i, j);            
                deps_list.push([i, j, dtDependsOn, []]);
                tp_deps_list.push([i, j, dtLag, [value, condition, ubound]]);
                document.getElementById("cell_" + i +"_" + j).parentElement.innerHTML = setCellInnerHTML(i, j);
                document.getElementById("cell_" + j +"_" + i).parentElement.innerHTML = setCellInnerHTML(j, i);
                sendCommand("action=set&type=" + dtLag + "&i=" + i + "&j=" + j + "&value=" + value + "&condition=" + condition + "&ubound=" + ubound);
            }
        }
    }

    function resetLag(i, j) {
        setCellDependency(i, j, dtClearCell);
    }

    function setCellDependency(i, j, dType) {
        var dep_char = "";
        
        // if clear cell or cell already contains the same dependency
        deleteDependency(i, j);

        if (dType == dtClearCell) {
            sendCommand("action=set&i=" + i + "&j=" + j + "&type=" + dtClearCell);
        } else {                
            switch (dType*1) {                
                case dtMutuallyDependent:
                case dtMutuallyExclusive:
                    deps_list.push([i, j, dType*1, []]);
                    break;
                case dtDependsOn:
                case dtConcurrent:
                    deps_list.push([i, j, dtDependsOn, []]);
                    tp_deps_list.push([i, j, dtConcurrent, []]);
                    break;
                case dtSuccessive:
                    deps_list.push([i, j, dtDependsOn, []]);
                    tp_deps_list.push([i, j, dtSuccessive, []]);
                    break;
            }
            sendCommand("action=set&i=" + i + "&j=" + j + "&type=" + dType);
        }
        document.getElementById("cell_" + i +"_" + j).parentElement.innerHTML = setCellInnerHTML(i, j);
        document.getElementById("cell_" + j +"_" + i).parentElement.innerHTML = setCellInnerHTML(j, i);
    }

    function setCellInnerHTML(i, j) {
        var dt = getDependency(i, j);
        var dt_tp = getTPDependency(i, j);
        var dl = (isTimePeriodsLagsVisible ? getLagDependency(i, j) : []);        
        var lag = "";

        var dep_char = "";
        if (dt >= 0) dep_char = getDependencyChar(dt);
        if (isTimePeriodsVisible && dt_tp >= 0 && dt_tp != dtLag) dep_char += (dep_char == "" ? "" : " (") + getDependencyChar(dt_tp) + (dep_char == "" ? "" : ")");
        if (dt_tp == dtLag && dl.length > 0 && dl[0] != UNDEFINED_LAG) {
            if (!isTimePeriodsLagsVisible) {
                dep_char = "";
                lag == UNDEFINED_LAG;
                deleteDependency(i, j);
            } else {    
                lag = dl[0];
                switch (dl[1]*1) {
                    case lcEqual:
                        lag = "= " + lag;
                        break;
                    case lcGreaterOrEqual:
                        lag = ">= " + lag;
                        break;
                    case lcLessOrEqual:
                        lag = "<= " + lag;
                        break;
                    case lcRange:
                        lag = "[" + lag + " : " + dl[2] + "]";
                        break;
                }
            }
        }

        return "<span class='dep_cell' id='cell_" + i + "_" + j + "' i='" + i + "' j='" + j + "' style='font-weight:bold;'>" + (dep_char == "" ? "&nbsp;" : dep_char) + "</span>" + (lag == UNDEFINED_LAG ? "" : (dep_char == "" ? "" : "&nbsp;") + "<sup style='color:#009900;font-weight:bold;'>" + lag + "</sup>");
    }

    function onConvertAllDependencies(from_dt, to_dt) {
        sendCommand("action=convert_all&from=" + from_dt + "&to=" + to_dt);
    }

    function getDependencyChar(dType) {
       var curDependencyChar = "";       
       switch (dType) {
           case  dtDependsOn:
               curDependencyChar = "D";
               break;
           case dtMutuallyDependent:
               curDependencyChar = "M";
               break;
           case dtMutuallyExclusive:
               curDependencyChar = "X";
               break;
           case dtConcurrent:
               if (isTimePeriodsVisible) {
                   curDependencyChar = "<%=ResString("lblRADependencyConcurrent")%>";
               } else {
                   curDependencyChar = "D";
               }
               break;
           case dtSuccessive:               
               if (isTimePeriodsVisible) {
                   curDependencyChar = "<%=ResString("lblRADependencySuccessive")%>";
               } else {
                   curDependencyChar = "D";
               }
               break;
           case dtLag:
               curDependencyChar = "";
               break;
       }        
       return curDependencyChar;
    }

    function getDependencyName(dType) {
        if (dType == dtDependsOn) return "<%=ResString("lblDependsOn") %>";
        if (dType == dtMutuallyDependent) return "<%=ResString("lblMutuallyDependent") %>";
        if (dType == dtMutuallyExclusive) return "<%=ResString("lblMutuallyExclusive") %>";
        if (dType == dtConcurrent) return "<%=String.Format("{0} ({1})",ResString("lblDependsOn"),ResString("lblRADependencyConcurrent")) %>";
        if (dType == dtSuccessive) return "<%=String.Format("{0} ({1})",ResString("lblDependsOn"),ResString("lblRADependencySuccessive")) %>";
        if (dType == dtLag) return "<%=ResString("lblRADependencyLag") %>";
    }

    function syncReceived(params) {
        if ((params)) {
            var received_data = eval(params);
            if (received_data[0] == "clear") {    
               InitPage();            
            }
            if (received_data[0] == "tableview") {    
                view = 0;
                InitPage();            
            }
            if (received_data[0] == "listview") {    
                view = 1;
                InitPage();            
            }
            if (received_data[0] == "del") {
                if (view == 1) InitPage(); //rebuild page only for list view
            }
            if (received_data[0] == "set" || received_data[0] == "convert_all") {
                if (view == 1) { //rebuild page only for list view
                    deps_list = received_data[1];
                    tp_deps_list = received_data[2];
                    InitPage(); 
                }
            }
        }
        displayLoadingPanel(false);
    }

    function syncError() {
        displayLoadingPanel(false);
        DevExpress.ui.notify(resString("ErrorMsg_ServiceOperation"), "error");
    }

    function onClickToolbar(btn_id) {
        if (btn_id + "" == "-1000") { // clear all dependencies
            ConfirmClearAll();
        } else { // set dependency
            curDependencyType = btn_id * 1;
            updateToolbarDropDown();
            sendCommand('action=dd&val=' + curDependencyType);
        }            
        closeMenu();
    }

    function onPageResize() {
        $("button.button").css({ "max-height": "28px", "min-height": "28px", "width" : "auto" });

        //$("#divListView").height(10);
        //$("#divDataGrid").height(10);

        //$("#divListView").height($("#tdGrid").height() - 5);

        //$("#divDataGrid").height($("#tdGrid").height());
        $("#divDataGrid").width(10).width($("#tdGrid").width());
                
        if ((table)) {
            dataTablesResize("tableDataGrid", "divDataGrid", 0) 
        }
    }

    function ConfirmClearAll() {
        dxConfirm(resString("msgSureRemoveAllDependencies"), "ClearAll();");
    }

    function ClearAll() {
        deps_list = [];
        tp_deps_list = [];
        sendCommand("action=clear");
    }

    function onSetScenario(id) {
        displayLoadingPanel(true);
        document.location.href='<% =PageURL(CurrentPageID) %>?action=scenario<% =GetTempThemeURI(true) %>&sid='+ id;
        return false;
    }

    function onSetView(view) { //0 - table view, 1 - list view
        <% If Not isGridAllowed Then %>if (view == 0) {
            DevExpress.ui.notify("<% =JS_SafeString(ResString("msgRANoGridDependencies")) %>", "warning"); 
            $("#cbView").dxSwitch("instance").option("value", view == 1);
            return false;
        }<% End If %>
        sendCommand("action=" + (view == 0 ? "tableview" : "listview"));
        return true;
    }

    /* jQuery Ajax */
    function sendCommand(params) {
        cmd = params;
        displayLoadingPanel(true);
        
        _ajax_ok_code = syncReceived;
        _ajax_error_code = syncError;
        _ajaxSend(params);
    }

    $(document).ready(function () {
        $("#divMainContent").css("overflow", "hidden");
        $(".mainContentContainer").css({ "overflow" : "hidden"});
        setTimeout('onDependency_Menu(); updateToolbarDropDown(); closeMenu(); $(".context-menu").hide(); InitPage();', 50);

        $("#cbView").dxSwitch({
            disabled: <% = Bool2JS(Not isGridAllowed) %>,
            //switchedOnText: "<% = ResString("lblListView") %>",
            //switchedOffText: "<% = ResString("lblGridView") %>",
            value: <% = Bool2JS(RA.Scenarios.GlobalSettings.DependenciesView OrElse Not isGridAllowed)%>,
            onValueChanged: function(data) {
                onSetView(data.value ? 1 : 0);
            }
        });
    });

    resize_custom = onPageResize;
    
</script>

<table id='tblMain' class="whole" border='0' cellspacing="0" cellpadding="0">
    <tr>
        <td>
            <div id='divToolbar' class='text' style="margin: 0px; padding: 2px;">
                <table class="text">
                    <tr valign="middle">
                        <td class="ec_toolbar_td_separator text" valign="middle">
                            <span class='toolbar-label'>&nbsp;<%=ResString("lblScenario") + ":"%>&nbsp;</span>
                            <% = GetScenarios() %>
                        </td>
                        <td class="ec_toolbar_td_separator text" valign="middle" <%=If(Not isGridAllowed, " style='display: none;' ","") %>>                       
                            <% = ResString("lblListView") %>: <div id="cbView"></div>                            
                        </td>
                        <%If Not App.IsActiveProjectReadOnly Then%>
                        <td class="ec_toolbar_td_separator text" valign="middle" <%=If(Not isGridAllowed, " style='display: none;' ","") %>>
                            <button id="btnDependency" onclick="onDependency_Menu(); return false;" class='button' type="button" <%=If(RA.Scenarios.GlobalSettings.DependenciesView, " disabled='disabled' ", "")%> title="<%=ResString("lblDependsOnHint")%>"><img id="imgDependency" src="../../Images/ra/d-20.png" width="16" height="16" />&nbsp;<%=ResString("lblDependsOn")%>&nbsp;&#x25BE;</button>
                        </td>
                        <td class="ec_toolbar_td_separator text" valign="middle">
                            <button type='button' class='button' onclick='onClickToolbar(-1000); return false;'><i class="fas fa-eraser"></i>&nbsp;<%=ResString("btnClearAll")%></button>
                        </td>
                        <%End If%>
                    </tr>
                </table>
            </div>
        </td>
    </tr>
    <tr>
        <td>
            <h5 style='padding: 10px 0px 4px 0px;' title="<% =SafeFormString(RA.Scenarios.ActiveScenario.Description) %>"><%=ResString("lblDependenciesTitle") %>&nbsp;&quot;<%=SafeFormString(ShortString(RA.Scenarios.ActiveScenario.Name, 45, True))%>&quot;</h5>
        </td>
    </tr>    
    <tr style='height:100%;'>
      <td class='text whole' valign='top' align='center' id='tdGrid'>
        <div id="divDataGrid" class='whole' style='overflow:auto; padding:1px;' onscroll='$("div.context-menu").hide(200); is_context_menu_open = false;'>
            <table id='tableDataGrid' class='text cell-border hover order-column' style="table-layout:fixed; border-collapse:collapse; border-bottom: 1px solid #e0e0e0;"></table>
        </div>
        <div id="divListView" class='whole' style='overflow:auto; width:850px; padding:1px; border:1px solid #e0e0e0; text-align:center;'>
            <center><table id='tableListView' class='text' border='0' cellpadding='2'></table></center>
        </div>
      </td>
    </tr>
</table>
</asp:Content>