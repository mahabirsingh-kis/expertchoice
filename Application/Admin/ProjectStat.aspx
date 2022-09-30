<%@ Page Language="VB" Inherits="ProjectStatisticPage" Codebehind="ProjectStat.aspx.vb" %>
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

    var is_risk = <% =Bool2Num(App.isRiskEnabled) %>;
    var opt_pages = false;
    var opt_details = false;
        
    var dataSet = [];

    var idx_prj_id = 0;
    var idx_prj_name = 1;
    var idx_comment = 2;
    var idx_passcode = 3;
    var idx_passcode2 = 4;
    var idx_prj_enabled = 5;
    var idx_prj_status = 6;
    var idx_prj_online = 7;
    var idx_created = 8;
    var idx_visited = 9;
    var idx_modified = 10;
    var idx_locked = 11;
    var idx_tt = 12;
    var idx_online = 13;
    var idx_size_streams = 14;
    var idx_size_snapshots = 15;
    var idx_size_total = 16;
    var idx_cnt_m = 17;
    var idx_cnt_a = 18;
    var idx_cnt_total = 19;

    var offs = is_risk-1;

    var col_expand = 0;
    var col_name = 1;
    var col_passcode = 2;
    var col_passcode2 = 3;
    var col_size_stream = 4 + offs;
    var col_size_snapshots = 5 + offs;
    var col_size_total = 6 + offs;
    var col_cnt_m = 7 + offs;
    var col_cnt_a = 8 + offs;
    var col_cnt_total = 9 + offs;
    var col_status = 10 + offs;
    var col_created = 11 + offs;
    var col_visited = 12 + offs;
    var col_modified = 13 + offs;

    function PleaseWait(show) {
        if (show) {
            $("#divLoading").show();
            $("#divMessage").hide();
            $("#divTable").hide();
        } else {
            $("#divLoading").hide();
        }
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
                { title: "<% =JS_SafeString(ResString("lblProjectName")) %>" , "data": idx_prj_name, "searchable": true, "class": "td_left" },
                { title: "<% =JS_SafeString(ResString(CStr(IIf(App.isRiskEnabled, "lblPasscodeLikelihood", "lblPasscode")))) %>", "data": idx_passcode, "width": 110, "searchable": true, "class": "td_center" },
                { title: "<% =JS_SafeString(ResString("lblPasscodeImpact")) %>", "data": idx_passcode2, "width": 110, "searchable": true, "class": "td_center", "visible": <% = IIf(App.isRiskEnabled, "true", "false") %> },
                { title: "<% =JS_SafeString(ResString("lblProjectSizeStreams")) %>", "data": idx_size_streams, "width": 90, "searchable": false, "class": "td_right"},
                { title: "<% =JS_SafeString(ResString("lblProjectSizeSnapshots")) %>", "data": idx_size_snapshots, "width": 90, "searchable": false, "class": "td_right"},
                { title: "<% =JS_SafeString(ResString("lblProjectSizeTotal"))%>", "data": idx_size_total, "width": 90, "searchable": false, "class": "td_right"},
                { title: "<% =JS_SafeString(ResString("lblSnapshotsCntM"))%>", "data": idx_cnt_m, "width": 90, "searchable": false, "class": "td_right"},
                { title: "<% =JS_SafeString(ResString("lblSnapshotsCntA")) %>", "data": idx_cnt_a, "width": 90, "searchable": false, "class": "td_right"},
                { title: "<% =JS_SafeString(ResString("lblSnapshotsCntTotal")) %>", "data": idx_cnt_total, "width": 90, "searchable": false, "class": "td_right"},
                { title: "<% =JS_SafeString(ResString("lblProjectStatus")) %>", "data": idx_prj_status, "width": 90, "searchable": false, "class": "td_center small"},
                { title: "<% =JS_SafeString(ResString("lblCreated")) %>", "data": idx_created, "width": 90, "searchable": false, "class": "td_center small" },
                { title: "<% =JS_SafeString(ResString("lblVisited")) %>", "data": idx_visited, "width": 90, "searchable": false, "class": "td_center small" },
                { title: "<% =JS_SafeString(ResString("lblModified")) %>", "data": idx_modified, "width": 90, "searchable": false, "class": "td_center small" }
            ],
            "paging": opt_pages,
            "ordering": true,
	        //"scrollY": "100%",
	        //"scrollCollapse": true,
	        "stateSave": false,
            "searching": true,
            "info":     false,
	        "createdRow": function ( row, data, index ) {
                //$('td', row).eq(col_comment).html("<span class='small'>" + data[idx_comment] + "</span>");
	            //$('td', row).eq(col_passcode2).html(data[idx_passcode]==data[idx_passcode2] ? "&nbsp;" : data[idx_passcode2]);
	            if ((data[idx_prj_enabled])) {
	                var sExtra = "";
	                if (data[idx_locked]!="") sExtra += "<span style='margin:2px; padding:1px 2px; font-size:7pt; background:#ffcccc; border:1px solid #cccccc;cursor:help;' class='small' title=\"" + replaceString('"', "&quot;", data[idx_locked]) + "\">Locked</span>";
	                if (data[idx_online]>0) sExtra += "<span style='margin:2px; padding:1px 2px; font-size:7pt; background:#ccffbb; border:1px solid #cccccc;' class='small'><nobr>Online: " + data[idx_online] + "</nobr></span>";
	                if (data[idx_tt]>0) sExtra += "<span style='margin:2px; padding:1px 2px; font-size:7pt; background:#cceeff; border:1px solid #cccccc;' class='small' title='" + <% If App.isRiskEnabled Then %>(data[idx_tt]==2 ? "Impact TT session" : "Likelihood TT session") + <% End If%> "'>TeamTime</span>";
                    $('td', row).eq(col_name).html((sExtra == "" ? "" : "<span style='float:right'>" + sExtra + "</span>") + "<a href='' onclick='openProject(\"" + data[idx_prj_id] + "\"); return false;' class='actions'>" + data[idx_prj_name] + "</a>" + ((data[idx_comment]!='') ? '&nbsp;<i class="fas fa-info-circle fa-lg ec-info" title="' + replaceString('"', '&quot;', data[idx_comment]) + '"></i>' : ''));
	                $('td', row).eq(col_cnt_total).html("<a href='' onclick='openSnapshots(\"" + data[idx_prj_id] + "\"); return false;' class='actions'>" + data[idx_cnt_total] + "</a>");
	            } else {
	                $('td', row).eq(col_name).addClass("gray");
	            };
	            $('td', row).eq(col_size_stream).html("<span class=''>" + getSize(data[idx_size_streams]) + "</span>");
	            $('td', row).eq(col_size_snapshots).html("<span class=''>" + getSize(data[idx_size_snapshots]) + "</span>");
	            $('td', row).eq(col_size_total).html("<span class=''>" + getSize(data[idx_size_total]) + "</span>");
	            $('td', row).eq(col_status).html("<span class='small'>" + data[idx_prj_status] + "</span>");
	            $('td', row).eq(col_created).html("<span class='small'>" + data[idx_created] + "</span>");
	            $('td', row).eq(col_visited).html("<span class='small'>" + data[idx_visited] + "</span>");
	            $('td', row).eq(col_modified).html("<span class='small'>" + data[idx_modified] + "</span>");

	        },
            "order": ([[col_visited+1, "desc"]])
        });

        var hs = $("#" + tblProjectsID + "_filter");
        if ((hs)) hs.children().hide();
        setTimeout("onResize(); highlightSearch();", 10);
        setTimeout("onResize(); ", 150);
    };

    function getSize(b) {
        var s = Math.round(b/(b>1024000 ? 1024 : 1)/102.4);
        var l = "Kb";
        if (b>1024000) l = "Mb";
        s = s/10 + "";
        if (s.indexOf(".")<0) s += ".0";
        return s + l;
    }

    function onSearch(txt) {
        if (getSearch() == txt && txt != search_old) {
            if (!(table) || !(dataSet) || !(dataSet.length) || (txt.indexOf(data_text)!=0)) {
                getProjects(txt);
            } else {
                table.search(txt).draw();
                setTimeout("onResize(); highlightSearch();", 10);
                search_old = txt;
            }
        }
    }

    function onOption() {
        initTable();
    }

    function openProject(prj_id) {
        _ajax_ok_code = "PleaseWait(true); if (typeof (data)!='undefined' && data!='') { if ((window.parent)) window.parent.document.location.href=data; else document.location.href=data; } else { initTable(); alert('(!) Unable to open the specified project'); }";
        sendAJAX("action=open&id=" + prj_id);
    }

    function openSnapshots(prj_id) {
        if (typeof onShowSnapshots == "function") {
            onShowSnapshots(prj_id);
        } else {
            window.open("<% = PageURL(_PGID_PROJECT_SNAPSHOTS, "prj_id=")%>" + prj_id, "snapshots", "menubar=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=620,height=500");
        }
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
        var h = 0;
        //var p = $("#projects_paginate");
        //if ((p) && (p.length)) h = p.height();alert(h);
        $("#divTable").height(100).width(500).height($("#tdMain").height()-$("#tdHeader").height() - 8 - h).width($("#tdMain").width()-8);
        //if ((table)) dataTablesResize(tblProjectsID, "divTable");
    }

    function onRefreshSnapshots() {
        getProjects("");
    }

    $(document).ready(function() {
        initSearch();
        getProjects("");
        //theForm.cbGrouping.checked = opt_grouping;
        theForm.cbPages.checked = opt_pages;
        resize_custom = onResize;
    });

//-->
</script>
<div class="table">
    <div class="tr">
        <div class="td">
            <h4 style="margin-top:6px;"><% =GetPageTitle%></h4>
        </div>
    </div>
     <div class="tr" style="height:2em;">
        <div class="td text">
            <div style="border:1px solid #cccccc; background:#f0f0f0; padding:4px 12px; text-align:center;">&nbsp;
                <% =ResString("lblProjectSearch") %> <input type='text' id='txtSearch' value='' style='width:20em'/>
                <span style="margin-top:4px; text-align:center; text-wrap:none;">&nbsp; </span>
                <label><input type='checkbox' name='cbPages' value='1' onclick='opt_pages = this.checked; onOption();' /><% =ResString("lblShowByPages") %></label> 
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
                <div id="divLoading" style="background:url(<% =ImagePath %>process.gif) center center no-repeat; position:relative; width:100%; height:100%; display:none;"></div>
                <div id="divTable" style="width:100%; height:99%; display:none; text-align:center; overflow:auto;"><table id="projects" class="display compact" width="100%"></table></div>
            </div>
        </div>
    </div>
</div>
</asp:Content>
