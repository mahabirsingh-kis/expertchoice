<%@ Page Language="VB" Inherits="ProjectLookupPage" Codebehind="ProjectLookup.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script type="text/javascript" src="../Scripts/jquery.highlight.js"></script>
<script type="text/javascript">
<!--

    var tblProjectsID = "projects";
    var table = null;
    var search_old = "";
    var data_text = "";

    var max_prj = <% =PRJ_MAX_COUNT %>;

    var is_risk = <% =iif(CanSeeRisk, 1, 0) %>;
    var opt_grouping = false;
    var opt_pages = false;
    var opt_details = false;
        
    var dataSet = [];
    var data_size = -1;

    var idx_prj_id = 0;
    var idx_prj_name = 1;
    var idx_comment = 2;
    var idx_passcode = 3;
    var idx_passcode2 = 4;
    var idx_prj_enabled = 5;
    var idx_wkg_name = 6;
    var idx_wkg_enabled = 7;
    var idx_prj_status = 8;
    var idx_prj_online = 9;
    var idx_created = 10;
    var idx_visited = 11;
    var idx_modified = 12;
    var idx_locked = 13;
    var idx_tt = 14;
    var idx_online = 15;

    var col_expand = 0;
    var col_name = 1;
//    var col_comment = 2;
    var col_wkg = 2;
    var col_passcode = 3;
    var col_passcode2 = 4;
    var col_status = 5;
    var col_online = 6;
    var col_created = 7;
    var col_visited = 8;
    var col_modified = 9;

    function PleaseWait(show) {
        displayLoadingPanel(show);
        //if (show) {
        //    $("#divLoading").show();
        //    $("#divMessage").hide();
        //    $("#divTable").hide();
        //} else {
        //    $("#divLoading").hide();
        //}
    }

    function sendAJAX(params) {
        PleaseWait(true);
        _ajax_ok_code = 'receiveAJAX(data);' + _ajax_ok_code;
        _ajaxSend(params);
    }

    function receiveAJAX(data) {
        PleaseWait(false);
    }

    function getSearch() {
        return $('#txtSearch').val();
    }

    function getProjects() {
        search_old = getSearch();
        data_text = search_old;
        _ajax_ok_code = "parseData(data);";
        sendAJAX("action=load&text=" + encodeURIComponent(search_old));
    }

    function highlightSearch() {
        var s = getSearch();
        if (s!="" && (table)) {
            var body = $(table.table().body());
            body.unhighlight();
            body.highlight(s);
        }
    }

    function parseData(data) {
        if ((typeof data!="undefined")) {
            try {
                dataSet = eval(data);
                initTable();
//                table.search(s).draw();
            }
            catch (e) {
            }
        }    
    }

    function initTable() {
        if ((table)) table.destroy();
        table = null;

        if (!(dataSet) || !(dataSet.length)) {
            $("#divTable").hide();
            $("#divMessage").show().html((getSearch()!="") ? "<% = JS_SafeString(ResString("msgNoMatches")) %>" : "<% =JS_SafeString(ResString("msgStartTyping")) %>");
            return false;
        } else {
            //if (dataSet.length>=max_prj)  $("#divMessage").show().html("<span class='top_warning_nofloat' style='font-size:10pt; color: #993333; margin-top:2ex;'><% = JS_SafeString(ResString("lblPrjLookupAlot")) %></span>"); else $("#divMessage").hide;
            $("#divMessage").hide();
            $("#divTable").show();
        }

        table = $('#' + tblProjectsID).DataTable( {
            "data": dataSet,
            "columns": [
                {
                    "orderable": false,
                    "data": null,
                    "width": 8,
                    "defaultContent": '',
                    "title": "&nbsp;",
                    "searchable": false 
                }, 
                { title: "<% =JS_SafeString(ResString("lblProjectName")) %>", "data": idx_prj_name, "searchable": true, "class": "td_left" },
//                { title: "<% =JS_SafeString(ResString("tblComment"))%>", "data": idx_comment, "searchable": true, "class": "small" },
                { title: "<% =JS_SafeString(ResString("lblWorkgroup")) %>", "data": idx_wkg_name, "searchable": false  },
                { title: "<% =JS_SafeString(ResString(CStr(IIf(App.isRiskEnabled, "lblPasscodeLikelihood", "lblPasscode")))) %>", "data": idx_passcode, "width": 120, "searchable": true, "class": "td_center" },
                { title: "<% =JS_SafeString(ResString("lblPasscodeImpact")) %>", "data": idx_passcode2, "width": 120, "searchable": true, "class": "td_center", "visible": <% = IIf(App.isRiskEnabled, "true", "false") %> },
                { title: "<% =JS_SafeString(ResString("lblProjectStatus")) %>", "data": idx_prj_status, "width": 120, "searchable": false, "class": "td_center"},
                { title: "<% =JS_SafeString(ResString("lblOnline")) %>", "data": idx_prj_online, "width": 60, "searchable": false, "class": "td_center" },
                { title: "<% =JS_SafeString(ResString("lblCreated")) %>", "data": idx_created, "width": 90, "searchable": false, "class": "td_center small" },
                { title: "<% =JS_SafeString(ResString("lblVisited")) %>", "data": idx_visited, "width": 90, "searchable": false, "class": "td_center small" },
                { title: "<% =JS_SafeString(ResString("lblModified")) %>", "data": idx_modified, "width": 90, "searchable": false, "class": "td_center small" }
            ],
            "paging": opt_pages,
            "ordering": true,
	        /*"scrollY": "500px",
	        "scrollCollapse": true,*/
	        "stateSave": false,
            "searching": true,
            "info":     false,
	        "createdRow": function ( row, data, index ) {
                //$('td', row).eq(col_comment).html("<span class='small'>" + data[idx_comment] + "</span>");
	            //$('td', row).eq(col_passcode2).html(data[idx_passcode]==data[idx_passcode2] ? "&nbsp;" : data[idx_passcode2]);
	            if ((data[idx_prj_enabled]) && (data[idx_wkg_enabled])) {
	                var sExtra = "";
	                if (data[idx_locked]!="") sExtra += "<span style='margin:2px; padding:1px 2px; font-size:7pt; background:#ffcccc; border:1px solid #cccccc;cursor:help;' class='small' title=\"" + replaceString('"', "&quot;", data[idx_locked]) + "\">Locked</span>";
	                if (data[idx_online]>0) sExtra += "<span style='margin:2px; padding:1px 2px; font-size:7pt; background:#ccffbb; border:1px solid #cccccc;' class='small'><nobr>Online: " + data[idx_online] + "</nobr></span>";
	                if (data[idx_tt]>0) sExtra += "<span style='margin:2px; padding:1px 2px; font-size:7pt; background:#cceeff; border:1px solid #cccccc;' class='small' title='" + <% If App.isRiskEnabled Then %>(data[idx_tt]==2 ? "Impact TT session" : "Likelihood TT session") + <% End If%> "'>TeamTime</span>";
                    $('td', row).eq(col_name).html((sExtra == "" ? "" : "<span style='float:right'>" + sExtra + "</span>") + "<a href='' onclick='_openProject(\"" + data[idx_prj_id] + "\"); return false;' class='actions'>" + data[idx_prj_name] + "</a>" + ((data[idx_comment]!='') ? '&nbsp;<img src="<% =ImagePath %>info12.png" width="12" height="12" border="0" title="' + replaceString('"', '&quot;', data[idx_comment]) + '">' : ''));
	            } else {
	                $('td', row).eq(col_name).addClass("gray");
	            };
	            $('td', row).eq(col_created-1+is_risk).html("<span class='small'>" + data[idx_created] + "</span>");
	            $('td', row).eq(col_visited-1+is_risk).html("<span class='small'>" + data[idx_visited] + "</span>");
	            $('td', row).eq(col_modified-1+is_risk).html("<span class='small'>" + data[idx_modified] + "</span>");

	        },
            "columnDefs": (opt_grouping ? [
                { "visible": false, "targets": col_wkg  }
            ] : []),
            "order": (opt_grouping ? [[col_wkg, 'asc'],[col_visited, "desc"]] : [[col_visited, "desc"]]),
            "drawCallback": function ( settings ) {
                if (opt_grouping) {
                    var api = this.api();
                    drawGroupRow(api);
                }
            }

        });

        if (opt_grouping) {
            dataTablesGrouping(table, tblProjectsID, col_wkg);
        }

        var hs = $("#" + tblProjectsID + "_filter");
        if ((hs)) hs.children().hide();
        setTimeout("onResize(); highlightSearch();", 10);
        setTimeout("onResize(); ", 150);
    };


    function drawGroupRow(api) {
        var rows = api.rows( {page:'current'} ).nodes();
        var last=null;
 
        api.column(col_wkg, {page:'current'} ).data().each( function ( group, i ) {
            if ( last !== group ) {
                $(rows).eq(i).before(
                    '<tr class="group"><td colspan="' + (is_risk ? 9 : 8) +'"><%  =JS_SafeString(ResString("lblWorkgroup")) %>: "<b>'+group+'</b>":</td></tr>'
                );
                last = group;
            }
        } );
    }

    function onSearch(txt) {
        if (getSearch() == txt && txt != search_old) {
            if (!(table) || !(dataSet) || !(dataSet.length) || (dataSet.length>=max_prj) || (txt.indexOf(data_text)!=0)) {
                getProjects(txt);
            } else {
                table.search(txt).draw();
                search_old = txt;
            }
        }
    }

    function onOption() {
        initTable();
    }

    function _openProject(prj_id) {
        _ajax_ok_code = "_onOpenProject(data);";
        sendAJAX("action=open&id=" + prj_id);
    }

    function _onOpenProject(data) {
        if (typeof (data)!="undefined" && data!='') { 
            PleaseWait(true); 
            if ((window.parent)) window.parent.document.location.href=data; else document.location.href=data; 
        } else { 
            getProjects(); 
            dxDialog("<% = JS_SafeString(ResString("errCantFindProject")) %>", true, ";"); 
        };
    }

    function initSearch() {
        $('#txtSearch').on('keyup', function () {
            setTimeout("onSearch('" + replaceString("'", "\'", this.value) + "');", 300);
        });
        $("#txtSearch").unbind("mouseup").bind("mouseup", function(e){
            var $input = $(this);
            var oldValue = $input.val();
            if (oldValue == "") return;
            setTimeout(function(){ if ($input.val() == "") onSearch(""); }, 100);
        });
        setTimeout("$('#txtSearch').focus()", 200);
    }

    function onResize() {
        $("#divTable").height(100).height($("#tdMain").height()-120);
//	    if ((table)) dataTablesResize(tblProjectsID, "tdMain");
    }

    $(document).ready(function() {
        initSearch();
        getProjects("");
        theForm.cbGrouping.checked = opt_grouping;
        theForm.cbPages.checked = opt_pages;
        resize_custom = onResize;
    });

//-->
</script>
<div class="table">
        <div class="tr">
            <div class="td">
                <h4 class="td page-title"><% =GetPageTitle%></h4>
            </div>
        </div>
        <div class="tr" style="height:2em;">
            <div class="td text">
                <div style="border:1px solid #cccccc; background:#f0f0f0; padding:4px 12px; text-align:center;">&nbsp;
                    <% =ResString("lblProjectSearch") %> <input type='text' id='txtSearch' value='' style='width:20em'/>
                    <span style="margin-top:4px; text-align:center; text-wrap:none;">&nbsp; <nobr>
                    <label><input type='checkbox' name='cbGrouping' value='1' onclick='opt_grouping = this.checked; onOption();' /><% =ResString("lblGroupByWkg") %></label> &nbsp;
                    <label><input type='checkbox' name='cbPages' value='1' onclick='opt_pages = this.checked; onOption();' /><% =ResString("lblShowByPages") %></label> &nbsp;
                    </nobr>&nbsp;</span>
                </div>
            </div>
        </div>
        <div class="tr">
            <div class="td text">
                <div id="divMessage" style="text-align:center; font-weight:bold; margin-top:10px; padding:2px 2em;"><% =ResString("lblPleaseWait")%></div>
            </div>
        </div>
        <div class="tr" id="trGrid">
            <div class="td tdCentered" style="height:100%; width:100%; text-align:center;" id="tdGrid">
                <div id="grid" class="whole">
                    <%--<div id="divLoading" style="background:url(<% =ImagePath %>process.gif) center center no-repeat; position:relative; width:100%; height:100%; display:none;"></div>--%>
                    <div id="divTable">
                        <table id="projects" class="display compact" width="100%"></table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
