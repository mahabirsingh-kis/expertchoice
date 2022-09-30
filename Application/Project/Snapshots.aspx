<%@ Page Language="VB" Inherits="Snapshots_Timeline" MasterPageFile="~/mpEmpty.master" Codebehind="Snapshots.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<%--<link href="<%=String.Format("{0}{1}/", _URL_THEMES, _THEME_EC2018)%>dx.common.css" rel="stylesheet" type="text/css" />
<link href="<%=String.Format("{0}{1}/", _URL_THEMES, _THEME_EC2018)%>dx.light-compact.custom.css" rel="stylesheet" type="text/css" />
<link href="<%=String.Format("{0}{1}/", _URL_THEMES, _THEME_EC2018)%>x-dx.light-compact.fixes.css" rel="stylesheet" type="text/css" />--%>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables.extra.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/datatables_only.min.js"></script>
<script type="text/javascript" src="<% =_URL_ROOT %>Scripts/dataTables.fixedColumns.min.js"></script>
<%--<script type="text/javascript" src="/Scripts/dx.web.js"></script>--%>
    <script type="text/javascript" >

        var snapshots = [];
        var msg = "";
        var is_chk = false;
        var last_md5 = "";
        var last_chk = -1;
        var shift_pressed = false;

        var refresh_timeout = 5000;

        //var table = "undefined";

        var div_id = "#divSnapshots";
        var tbl_id = "#tableSnapshots";
        var td_id = "#tdSnapshots";

        var idx_id = 0;
        var idx_dt = 1;
        var idx_name = 2;
        var idx_type = 3;
        var idx_md5 = 4;
        var idx_idx = 5;
        var idx_parent = 6;
        var idx_details = 7;
        var idx_smd5 = 8;
        var idx_ssize = 9;
        var idx_wmd5 = 10;
        var idx_wsize = 11;
        var idx_old = 12;

        var grid_view = <% =iif(GetCookie(_COOKIE_SNAPSHOTS_MODE, "")<>"0", "true", "false") %>;

        var show_mode = 0;  // 0 - all; 1 -- auto only, 2 -- manual only;
        var edit_comments = <% =CStr(IIf(App.CanvasMasterDBVersion < "0.99992", "false", "true"))%>;
        var show_manual_comment = true;

//        var type_colors = ["#dadada", "#d0d0f0", "#d0f0cc"];
//        var type_names = ["<% =JS_SafeString(ResString("lblSnapshotAuto")) %>", "<% =JS_SafeString(ResString("lblSnapshotManual")) %>", "<% = JS_SafeString(ResString("lblSnapshotRP")) %>"];

        // Simple version:
        var type_colors = ["#b0b0b0", "#80e0b0", "#b0b0b0"];
        var type_names = ["<% =JS_SafeString(ResString("lblSnapshotAuto")) %>", "<% =JS_SafeString(ResString("lblSnapshotManual")) %>", "<% = JS_SafeString(ResString("lblSnapshotAuto")) %>"];

        var filter_names = ["<% =JS_SafeString(ResString("lblSnapshotFltAll"))%>", "<% =JS_SafeString(ResString("lblSnapshotFltAuto"))%>", "<% =JS_SafeString(ResString("lblSnapshotFltManual"))%>"];

        function ShowPreload() {
            showLoadingPanel();
            //if ((window.parent) && typeof window.parent.showLoadingPanel == "function") window.parent.showLoadingPanel(); else showLoadingPanel();
        }

        function HidePreload() {
            hideLoadingPanel();
            //if ((window.parent) && typeof window.parent.hideLoadingPanel == "function") window.parent.hideLoadingPanel(); else hideLoadingPanel();
        }

        function initTable(lst, chk) {
            var cols_lst = [{ title: "<% = JS_SafeString(ResString("lblSnapshotAction"))%><br><img src='<% =ImagePath %>blank.gif' width=150 height=1 title='' border=0>"}];

            var dataSet = [];
            var rows = [];
            var cols = ["~title~"];
            var snaps = [];

            for (i=0; i<lst.length;i++) {
                var s = lst[i];

                var n = s[idx_name];
                var dt = s[idx_dt];

                var r = rows.indexOf(n);
                if (r<0) r = rows.push(n)-1;

                var c = cols.indexOf(dt);
                if (c<0) {
                    c = cols.push(dt)-1;
                    cols_lst.push({title: "<span style='font-family:Tahoma, Verdana; font-size:9px;'>" + replaceString(" ", "<br>",dt) + "</span>"});
                }
                snaps.push([r, c, i]);
            }

            for (var i=0; i<rows.length; i++) {
                dataSet[i] = new Array(c);
                dataSet[i][0] = htmlEscape(rows[i]);
                for (var j=1; j<cols.length; j++) dataSet[i][j]="&nbsp;";
            }

            for (var i=0; i<snaps.length; i++) {
                var s = snaps[i];
                var idx = s[2];
                var snap = lst[idx];
                var cmnt = "";
                if (snap[idx_details]!="") {
                    if (show_manual_comment && snap[idx_type]==<% =CInt(ecSnapShotType.Manual)%>) {
                        cmnt = "<div class='text small gray'>" + snap[idx_details] + "</div>";
                    } else {
                    cmnt = "&nbsp;<i class='far fa-comment' title=\"" + replaceString('"', "&#39;", snap[idx_details]) + "\" style='color:dodgerblue; cursor:default;'></i>";
                    }
                }
                if (snap[idx_parent]!="") cmnt += "<div class='small gray'>Restored from&nbsp;#" + snap[idx_parent] + "</div>";
                //var t = "<a href='' onclick='onRestore(" + snap[idx_id] + "); return false;' style='color:" + (snap[idx_type] == <% =CInt(ecSnapShotType.Manual)%> ? "green" : "#336699") + "'>#<b>" + (snap[idx_idx]) + "</b></a><br><div class='gray' style='font-size:9px; font-family: Tahoma; margin-top:3px'>" + snap[idx_md5] + "</div>";
                var t = "<a href='' onclick='onRestore(" + snap[idx_id] + "); return false;' style='color:" + ((snap[idx_old]) ? "#669999" : (snap[idx_parent]!="" ? "#0000ff" : (snap[idx_type] == <% =CInt(ecSnapShotType.Manual)%> ? "green" : "#336699"))) + "' title=\"" + getSnapshotInfo(snap) + "\">#<b>" + (snap[idx_idx]) + "</b></a>" + cmnt + ((chk) ? "" : "<div class='text small' style='height:10px; margin-top:3px;'><span id='divNum" + idx + "'>&nbsp;</span><span id='divEdit" + idx + "' style='display:none; color:#707070;'>" + (edit_comments ? "<i class='fas fa-pencil-alt' style='cursor:pointer; margin-right:6px;' title='Edit comment' onclick='onEditComment(" + snap[idx_id] + ");'></i>" : "") + "<i class='fas fa-trash-alt' style='cursor:pointer;' title='Delete snapshot' onclick='onDeleteSnapshot(" + snap[idx_id] + ");'></i></span></div>");
                if (chk) t = "<input type='checkbox' id='chk" + i + "' value='" + snap[idx_id] + "' class='checkbox' onclick='setTimeout(\"onCheck(" + i +");\", 25); checkSelected();'>&nbsp;" + t;
                dataSet[s[0]][s[1]] = "<div style='text-align:center;' onmouseover='rowHover(1, " + idx + ");' onmouseout='rowHover(0, " + idx + ");'>" + t + "</div>";
            }

            $(div_id).html("<table id=tableSnapshots border=0 class='cell-border hover compact' width='100%'></table>");

            var table = $(tbl_id).DataTable({
                retrieve: true,
                data: dataSet,
                columns: cols_lst,
                paging:    false,
                ordering:  false,
                scrollY: "300px",
                scrollX: "400px",
                stateSave: false,
                searching: false,
                processing: true,
                info:      false
            });
            
            dataTablesInit(table);

            $(div_id).hide(); 
            setTimeout('$(div_id).show(); onResize(); initFixedColumn(); ', 100);
            //setTimeout('onResize();', 350);
        }

        function initFixedColumn() {
            new $.fn.dataTable.FixedColumns($(tbl_id).dataTable(), {
                "iLeftColumns": 1,
                "sHeightMatch" : "auto",
                "iRightColumns": 0
            } );
        }

        function onResize() {
            var margin = 2;
            
            $(div_id).height(10);
            $(div_id).height($(td_id).innerHeight() - margin);
            $(div_id).width(10);
            $(div_id).width($(td_id).innerWidth() - margin);
            
            if ((grid_view)) {
                $("div.dataTables_scrollBody").height(1).height($(td_id).height() - $("div.dataTables_scrollHead").innerHeight() - margin);
                $("div.dataTables_wrapper").height(1).height($(td_id).innerHeight() - margin);
                //$("div.dataTables_scrollBody").width(1).width($(td_id).width() - margin);
                //$("div.dataTables_wrapper").width(1).width($(td_id).width() - margin);
            } else {
                $("#divContent").width(1).height(1).width($(td_id).innerWidth() - 2*margin).height($(td_id).innerHeight() - 2*margin);
                //dataTablesResize("tableSnapshots", "divSnapshots", margin);
                //dataTablesRedrawRows($(tbl_id).DataTable(), snapshots);
            }
        }

        function getSnapshots() {
            ShowPreload();
            msg = "";
            snapshots = [];
            _ajax_ok_code = "onParseSnapshots(data);"
            _ajaxSend("action=getdata");
        }

        function onCheckDataUpdates() {
            if (!is_chk && !_ajax_active) {
                _ajax_ok_code = "if (data!='') onParseSnapshots(data);"
                _ajaxSend("action=checkdata&md5=" + last_md5);
            }
            checkDataUpdates();
        }

        function checkDataUpdates() {
            setTimeout("onCheckDataUpdates();", refresh_timeout);
        }

        function showMsg(msg)
        {
            $(div_id).html("<p align='center'><b>" + msg + "</b></p>");
        }

        function onParseSnapshots(data) {
            var res = false;
            try { 
                var d = eval(data);
                if (typeof d!="undefined" && d.length == 3) {
                    msg = d[0];
                    snapshots = d[1];
                    last_md5 = d[2];
                    showSnapshots(false);
                    res = (d[1].length>0);
                }
            }
            catch (e) {
                showMsg("<span class='error' title='" + replaceString("'", "&#39;", e.message) + "'>Unable to parse or render data</span>");
            }
            HidePreload();
            return res;
        }

        function onRefreshParent() {
            w = window.opener;
            if (!w) w = window.parent;
            if ((w) && typeof w.onRefreshSnapshots == "function") w.onRefreshSnapshots();
        }

        function getSnapshotInfo(snap) {
            var n = "";
            if ((snap)){
                n = type_names[snap[idx_type]] + " snapshot #" + snap[idx_idx] + " (" + snap[idx_md5] + ")\n";
                if (snap[idx_old]) n += "* Snapshot has been uploaded/copied\n";
                n += "Action: '" + snap[idx_name] + "'\n";
                if (snap[idx_details]!="") n += "Details: '" + snap[idx_details] + "'\n";
                n += "Saved: " + snap[idx_dt] + "\n";
                if (snap[idx_parent]!="") n += "Restored from snapshot #" + snap[idx_parent] + "\n";
                n += "Snapshot project data:\n  * md5: " + snap[idx_smd5] + "\n  * size: " + snap[idx_ssize] + " bytes\n";
                n += "Snapshot users data:\n  * md5: " + snap[idx_wmd5] + "\n  * size: " + snap[idx_wsize] + " bytes";
            }
           return replaceString("\"", "&quot;", n);
        }

        function rowHover(vis, idx) {
            if ((vis)) {
                $("#divNum" + idx).hide();
                $("#divEdit" + idx).show();
            } else {
                $("#divEdit" + idx).hide();
                $("#divNum" + idx).show();
            }
        }

        function showSnapshots(chk) {
            ShowPreload();
            $(div_id).html("");
            if (msg != "") showMsg(msg);

            var tsize = 0;
            var rest = -1;
            if (snapshots.length > 0) {
                for (var i=0; i<snapshots.length; i++) {
                    tsize += snapshots[i][idx_ssize] + snapshots[i][idx_wsize];
                    if (rest<0 && snapshots[i][idx_parent]>0) rest = i;
                }
            }

            $("#divFilter").html("<span title='Snapshots total size: " + tsize + " bytes'>(" + filter_names[show_mode] + ")</span>");

            if (chk) { $("#divButtonsAll").hide(); $("#divButtonsDelete").show(); checkSelected(); } else { $("#divButtonsAll").show(); $("#divButtonsDelete").hide(); }
            if (snapshots.length > 0) {

                var lst = "";

                var rest_snap = null;
                var snaps = [];
                for (var i = 0; i < snapshots.length; i++) {
                    var s = snapshots[i];
                    if (rest>=0 && s[idx_idx]==snapshots[rest][idx_parent]) rest_snap = s;
                    if (show_mode==0 || (show_mode==1 && (s[idx_type]==<% =CInt(ecSnapShotType.Auto) %> || s[idx_type]==<% =CInt(ecSnapShotType.RestorePoint)%>)) || (show_mode==2 && s[idx_type]==<% =CInt(ecSnapShotType.Manual)%>)) {
                        snaps[snaps.length] = s;
                    }
                }

                if (snaps.length==0) {
                    showMsg("<% = JS_SafeString(ResString("msgSnapshotFltNoItems")) %><p align=center class='small'>[ <a href='' class='actions' onclick='show_mode=0; showSnapshots(); return false'><% = JS_SafeString(ResString("lblSnapshotFltReset")) %></a> ]</p>"); 
                } else {

                    if (grid_view) {

                        // show table/grid
                        initTable(snaps, chk);

                    } else {

                        // show simple list
                        for (var i = 0; i < snaps.length; i++) {
                            var s = snaps[i];
                            var t = type_names[s[idx_type]];
                            if (s[idx_parent]!="") t = "R";
                            var t_ = "";
                            if (t != "") t = "<div style='text-align:center; background:" + (s[idx_parent]!="" ? "#b0bae0" :  type_colors[s[idx_type]]) + "; color:#ffffff; border:1px solid #909090; text-align:center; vertical-align:middle; margin-right:4px; width:11px; height:11px; overflow:hide; font:9px Tahoma, Verdana; float:left' title='" + t + "'><b>" + t[0] + "</b></div>";
                            if (chk) t_ = "<input type='checkbox' id='chk" + i + "' value='" + s[idx_id] + "' class='checkbox' onclick='setTimeout(\"onCheck(" + i +");\", 25); checkSelected();'>&nbsp;";
                            //lst += "<tr><td class='text' style='background:" + ((i & 1) == 0 ? "#f5f5f5" : "#fbfbfb") + "' onmouseover='rowHover(1, " + i + ");' onmouseout='rowHover(0, " + i + ");'><span class='small' style='float:right; color:#d0d0d0; font:8pt Monospace'>" + s[idx_md5] + "</span>" + t + "<span style='font:8pt Veradana, Tahoma'>" + s[idx_dt] + "</span><div style='padding:2px 0px 0px " + (t_ == "" ? "18" : "14") + "px;'>" + t_ + "<span class='text small gray' style='float:right; color:#e0e0e0;'><span id='divNum" + i + "' style='width:40px;'>&nbsp;&nbsp;#" + (s[idx_idx]) + "</span><span id='divEdit" + i + "' style='display:none;'><img src='<% =ImagePath %>edit_tiny.gif' style='cursor:hand;' width=10 height=10 title='Edit comment' onclick='onEditComment(" + i + ");'/><img src='<% =ImagePath %>delete_tiny.gif' style='cursor:hand; margin-left:6px;' width=10 height=10 title='Delete snapshot' onclick='onDeleteSnapshot(" + s[idx_id] + ");'/></span></span><b><a href='' onclick='onRestore(" + s[idx_id] + "); return false' class='actions'" + (s[idx_type]==1 ? " style='color:green' title='Permanent snapshot'" : "") + ">" + s[idx_name] + "</a></b></div></td></tr>";
                            lst += "<tr><td class='text' style='background:" + ((i & 1) == 0 ? "#f5f5f5" : "#fbfbfb") + "' onmouseover='rowHover(1, " + i + ");' onmouseout='rowHover(0, " + i + ");'><span class='small gray' style='float:right;'>#<b>" + s[idx_idx] + "</b></span>" + t + "<span style='font:8pt Veradana, Tahoma'>" + s[idx_dt] + "</span><div style='padding:2px 0px 0px " + (t_ == "" ? "18" : "14") + "px;'>" + t_ + "<span class='text small gray' style='float:right; color:#707070;'><span id='divNum" + i + "' style='width:40px;'><nobr>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</nobr></span><span id='divEdit" + i + "' style='display:none;'>" + (edit_comments ? "<i class='fas fa-pencil-alt' style='cursor:pointer;' title='Edit comment' onclick='onEditComment(" + s[idx_id] + ");'></i>" : "") + "<i class='fas fa-trash-alt' style='cursor:pointer; margin-left:6px;' title='Delete snapshot' onclick='onDeleteSnapshot(" + s[idx_id] + ");'></i></span></span><b><a href='' onclick='onRestore(" + s[idx_id] + "); return false' class='actions'" + ((s[idx_old]) ? "#669999" : (s[idx_type]==1 ? " style='color:green'" : (s[idx_parent]!="" ? " style='color:#0000ff;'" : ""))) + " title=\"" + getSnapshotInfo(s) + "\">" + s[idx_name] + "</a></b>" + (s[idx_details]=="" ? "" : "<div class='small gray'>" + s[idx_details] + "</div>") + "</div></td></tr>";
                        }

                        lst = "<table width=100% border=0 cellspacing=1 cellpadding=3>" + lst + "</table>";
                        $(div_id).html("<div id='divContent' class='text' style='width:" + ($(td_id).width()-12) +"px; height:" + ($(td_id).height()-12) + "px; border:1px solid #e0e0e0; overflow:auto; padding:4px;'>" + $(div_id).html() + lst + "</div>");
                    }
                }

            }
            $("#divLastRestore").html((rest>=0) ? "<div class='text' style='font-weight:normal; margin-top:4px'><nobr><i class='far fa-clock' title='(i)'></i> Last restored as #<a href='' class='dashed' style='cursor:help' onclick='return false;' title=\"" + getSnapshotInfo(snapshots[rest]) + "\">" + snapshots[rest][idx_idx] + "</a> <span class='small gray'>(" + snapshots[rest][idx_dt] + ")</span></nobr>" + ((rest_snap) ? " <nobr>from #<a href='' class='dashed' style='cursor:help' onclick='return false;' title=\"" + getSnapshotInfo(rest_snap) + "\">" + rest_snap[idx_idx] + "</a> <span class='small gray'>(" + rest_snap[idx_dt] + ")</span></nobr>" : "") + "</div>" : "");
            $("#btnClean").prop('disabled', (snapshots.length == 0));
            $("#btnFilter").prop('disabled', (snapshots.length == 0));
            is_chk = chk;
            HidePreload();
        }

        function onCheck(idx) {
            if (last_chk>=0 && shift_pressed) {
                var a = last_chk;
                var b = idx;
                if (last_chk>idx) { a = idx; b = last_chk; }
                for (i=a; i<=b; i++) {
                    var c = eval("theForm.chk" + i);
                    if ((c)) c.checked = true;
                }
            }
            last_chk = idx;
        }

        function showFilters() {
            var flt = "<div style='padding:1ex 2em'><% = JS_SafeString(ResString("lblSnapshotFltSelect"))%><div style='padding:1ex 1em'>";
            for (var i=0; i<filter_names.length; i++) {
                flt+="<label><input type='radio' name='rbFilter' value='" + i + "'" + (i==show_mode ? " checked" : "") + " onclick='show_mode=this.value; showSnapshots(false);'>" + filter_names[i] + "</label><br>";
            }
            flt+="</div></div>";
            <%--jDialog_show_icon = false;
            dxDialog(flt, false, "showSnapshots(false);", "show_mode = " + show_mode + "; showSnapshots(false);", "<% = JS_SafeString(ResString("lblSnapshotFltTitle"))%>", 250);
            setTimeout("jDialog_show_icon = true;", 30);--%>
            dxDialog(flt, "showSnapshots(false);", "show_mode = " + show_mode + "; showSnapshots(false);", "<% = JS_SafeString(ResString("lblSnapshotFltTitle"))%>");
        }

        function onRestore(id) {
            dxDialog("<% = JS_SafeString(ResString("confRestoreFromSnapshot")) %>" + "<div style='padding-top:1ex;'><label><input type='checkbox' class='checkbox' id='cbCreateSnapshot' tabIndex='998' value='' onclick='onSaveSnapshotClick(this.checked);'><% = JS_SafeString(ResString("lblSnapshotCreateOnRestore"))%></label><div id='divSnapshotName' style='display:none; padding-left:44px;'><nobr><% = JS_SafeString(ResString("lblSnapshotSaveAs"))%>: <input type='input' id='editName' style='width:230px;' tabIndex='999'></nobr></div>", "doRestore(" + id + ", '&save=' + ($('#cbCreateSnapshot').prop('checked')) + '&name=' + encodeURIComponent($('#editName').val()));", ";", "<% = JS_SafeString(ResString("titleConfirmation")) %>", "<% = JS_SafeString(ResString("btnProceed")) %>", "<% = JS_SafeString(ResString("btnCancel")) %>");
        }

        function onSaveSnapshotClick(chk) {
            if ((chk)) {
                $("#divSnapshotName").show();
                setTimeout('$("#editName").focus();', 100);
            } else {
                $("#divSnapshotName").hide();
            }
        }

        function doRestore(id, params) {
            ShowPreload();
            _ajax_ok_code = "onProjectRestored(data);"
            _ajaxSend("action=restore&id=" + id + params);
        }

        function onProjectRestored(data) {
            if (onParseSnapshots(data)) {
                var msg = eval(data)[0];
                if ( msg == '') { 
                    doReload(); 
                } else {
                    var err = (msg.indexOf("error")>0);
                    dxDialog(msg, ";", "undefined","<% =JS_SafeString(ResString("titleInformation"))%>");
                }
            }
        }

        var snap_comment = "";
        function onCreateNew() {
            //jDialog_show_icon = false;
            snap_comment = "";
            dxDialog("<nobr><b><% =JS_SafeString(ResString("lblCreateSnapshotComment"))%></b><br><input type='text' id='tbComment' value='' style='width:98%;' maxlength='240' onkeyup ='snap_comment = this.value;'>", "doCreateNew();", ";", "<% =JS_SafeString(ResString("lblCreateSnapshot"))%>", "<% =JS_SafeString(ResString("btnCreate")) %>", "<% =JS_SafeString(ResString("btnCancel")) %>");
            setTimeout(function () { $("#tbComment").focus(); }, 200);
        }

        function onEditComment(id) {
            var s = null;
            for (var i = 0; i < snapshots.length; i++) {
                var snap = snapshots[i];
                if (snap[idx_id] == id) {
                    s = snap;
                    break;
                }
            }
            if ((s)) {
                snap_comment = replaceString("'", "&#39;", s[idx_details]);
                dxDialog("<nobr><b><% =JS_SafeString(ResString("lblEditSnapshotComment"))%></b><br><input type='text' id='tbComment' value='" + snap_comment + "' style='width:98%; margin-top:4px;' maxlength='240' onkeyup ='snap_comment = this.value; checkComment();'>", "doEditComment(" + s[idx_id] + ");", ";", "<% =JS_SafeString(ResString("hintEdit"))%>", "<% =JS_SafeString(ResString("btnSave"))%>", "<% =JS_SafeString(ResString("btnCancel")) %>");
                setTimeout(function () { $("#tbComment").focus(); }, 200);
            }
        }

        function checkComment(val) {
            dxDialogBtnDisable(true, (snap_comment == ""));
        }

        function doCreateNew() {
            ShowPreload();
            _ajax_ok_code = "onParseSnapshots(data);";
            _ajaxSend("action=create&text=" + encodeURIComponent(snap_comment));
        }

        function doEditComment(id) {
            ShowPreload();
            _ajax_ok_code = "onParseSnapshots(data);";
            _ajaxSend("action=edit&id=" + encodeURIComponent(id) + "&text=" + encodeURIComponent(snap_comment));
        }

        function onCleanupSnapshots() {
            dxConfirm("<% = JS_SafeString(ResString("confDeleteAllSnapshots"))%>", "doCleanupSnapshots();");
        }
        function doCleanupSnapshots() {
            ShowPreload();
            _ajax_ok_code = "onParseSnapshots(data);"
            _ajaxSend("action=clear");
        }

        function onCleanupAuto() {
            dxConfirm("<% = JS_SafeString(ResString("confDeleteAllAutoSnapshots"))%>", "doCleanupAuto();");
        }

        function doCleanupAuto() {
            var ids = "";
            for (var i=0; i<snapshots.length; i++)
            {
                var snap = snapshots[i];
                if (snap[idx_type] != <% =CInt(ecSnapShotType.Manual)%>) ids += (ids=="" ? "" : ",") + snap[idx_id];
            }
            doDeleteSnapshots(ids);
        }

        function onDeleteSnapshots() {
            var s = getSelected();
            var ids = "";
            for (var i = 0; i < s.length; i++) {
                ids += (ids == "" ? "" : ",") + s[i];
            }
            if (ids!="") {
                dxConfirm("<% = JS_SafeString(ResString("confDeletSelSnapshots"))%>", "doDeleteSnapshots('" + ids + "');");
            }
        }

        function doDeleteSnapshots(ids) {
            ShowPreload();
            _ajax_ok_code = "onParseSnapshots(data); onRefreshParent();"
            _ajaxSend("action=delete&lst=" + ids);
        }

        function onDeleteSnapshot(id) {
            dxConfirm("<% = JS_SafeString(ResString("confDeletSnapshot"))%>", "doDeleteSnapshot(" + id + ");");
        }

        function doDeleteSnapshot(id) {
            ShowPreload();
            _ajax_ok_code = "onParseSnapshots(data);"
            _ajaxSend("action=delete&lst=" + id);
        }

        function selectItems(chk) {
            for (var i = 0; i < snapshots.length; i++) {
                var c = eval("theForm.chk" + i);
                if ((c)) {
                    if (chk == 0) c.checked = false;
                    if (chk == 1) c.checked = true;
                    if (chk == -1) c.checked = !(c.checked);
                }
            }
            checkSelected();
        }

        function getSelected() {
            var lst = [];
            for (var i = 0; i < snapshots.length; i++) {
                var c = eval("theForm.chk" + i);
                if ((c) && (c.checked)) lst.push(c.value);
            }
            return lst;
        }

        function checkSelected() {
            $("#btnDeleteSel").prop("disabled", (getSelected().length == 0)) + ";path=/;expires=Thu, 31-Dec-2037 23:59:59 GMT;";
        }

        function switchViewMode() {
            grid_view = !grid_view;
            document.cookie = "<% =_COOKIE_SNAPSHOTS_MODE%>=" + ((grid_view) ? "1" : "0");
            ShowPreload();
            showSnapshots(false);
            HidePreload();
        }

        function doReload() {
//            alert("<% =JS_SafeString(ResString("msgProjectRestoredFromSnapshot"))%>");
            if ((window.opener) && (typeof window.opener.reloadPage) == "function") {
                window.opener.reloadPage(false);
                this.close();
                return true;
            }
            if ((window.parent) && (typeof window.parent.reloadPage) == "function") {
                window.parent.reloadPage(false);
                this.close();
                return false;
            }
            document.location.reload();
            return false;
        }

        function expandWindow() {
            var t = window.screenTop-16;
            var l = window.screenLeft;
            if (t<10) t = 10;
            if (l<10) l = 10;
            var w = window.innerWidth;
            var h = window.innerHeight;
            if (w<400) w = 600;
            if (h<100) h = 400;
            <%--window.open("<% = PageURL(_PGID_PROJECT_SNAPSHOTS, "prj_id=" + App.ProjectID.ToString)%>", "snapshots", "menubar=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=" + w + ",height=" + h + ",left=" + l + ",top=" + t);--%>
            if ((window.opener) && (typeof window.opener.switchSnapshots) == "function") {
                window.opener.switchSnapshots(w,h,l,t,<% =App.ProjectID%>);
                return true;
            }
            if ((window.parent) && (typeof window.parent.switchSnapshots) == "function") {
                window.parent.switchSnapshots(w,h,l,t,<% =App.ProjectID%>);
                return false;
            }
        }

        function closeWindow() {
            if ((window.opener) && (typeof window.opener.closeSnapshots) == "function") {
                window.opener.closeSnapshots();
                return true;
            }
            if ((window.parent) && (typeof window.parent.closeSnapshots) == "function") {
                window.parent.closeSnapshots();
                return false;
            }
        }

        $(document).ready(function () { getSnapshots(); checkDataUpdates(); $("body").css("overflow", "hidden"); });
        $(document).click(function(e) { shift_pressed = (e.shiftKey); });

        resize_custom = onResize;

    </script>
    <table border="0" cellspacing="0" cellpadding="0" class="whole">
        <tr style="height:40px; margin-bottom:4px;"><td><% If isSLTheme Then %><a href="" onclick="closeWindow(); return false;"><img src='<% =ImagePath %>close_13.gif' width=13 height=13 border="0" title="<% = SafeFormString(ResString("btnCloseWindow"))%>" style="float:right; margin-left:6px;"/></a><a href="" onclick="expandWindow(); return false;"><img src='<% =ImagePath %>expand.gif' width=11 height=11 border="0" title="<% = SafeFormString(ResString("lblSnapshotsNewWindow"))%>" style="float:right;"/></a><% End if %>
            <h6>&nbsp;<% =GetTitle()%> <nobr><span id="divFilter"></span></nobr><span id="divLastRestore"></span></h6>
            <div style="text-align:center; padding-bottom:4px;"><nobr>
                <div id="divButtonsAll">
                    <button id="btnCreate" onclick="onCreateNew(); return false" class='button' style="width:120px; height:24px" <% If Not App.isSnapshotsAvailable Then%>disabled<% End If%>><i class="fas fa-plus-circle"></i> <% =ResString("btnSnapshotCreate") %></button> 
                    <button id="btnFilter" onclick="showFilters(); return false;" class='button' style="width:120px; height:24px"><i class="fas fa-filter"></i> <% =ResString("btnSnapshotsFilter") %></button> 
                    <button id="btnClean" onclick="showSnapshots(true); return false;" class='button' style="width:120px; height:24px" disabled><i class="fas fa-minus-circle"></i> <% =ResString("btnSnapshotsErase") %></button> 
                    <button id="btnViewMode" onclick="switchViewMode(); return false;" class='button' style="width:120px; height:24px" <% If Not App.isSnapshotsAvailable Then%>disabled<% End If%>><i class="fas fa-table"></i> <% =ResString("btnSnapshotViewMode") %></button> 
                    <button id="btnRefresh" onclick="getSnapshots(); return false;" class='button' style="width:120px; height:24px" <% If Not App.isSnapshotsAvailable Then%>disabled<% End If%>><i class="fas fa-sync-alt"></i> <% =ResString("btnRefresh") %></button> 
                </div>
                <div id="divButtonsDelete" style="display:none">
                    <button id="btnDeleteAll" onclick="onCleanupSnapshots(); return false" class='button' style="width:120px; height:24px"><%--<img src="<% =ImagePath %>remove-16.png" width=16 height=16 style="vertical-align:middle" /> --%><% =ResString("btnSnapshotsDeleteAll")%></button> 
                    <button id="btnDeleteAllAuto" onclick="onCleanupAuto(); return false" class='button' style="width:120px; height:24px"><%--<img src="<% =ImagePath %>remove-16.png" width=16 height=16 style="vertical-align:middle" /> --%><% =ResString("btnSnapshotsDeleteAllAuto")%></button> 
                    <button id="btnDeleteSel" onclick="onDeleteSnapshots(); return false" class='button' style="width:120px; height:24px" disabled><%--<img src="<% =ImagePath %>remove-16.png" width=16 height=16 style="vertical-align:middle" /> --%><% =ResString("btnSnapshotsDeleteSel")%></button> 
                    <button id="btnCancel" onclick="showSnapshots(false); return false;" class='button' style="width:120px; height:24px"><%--<img src="<% =ImagePath %>cancel-16.png" width=16 height=16 style="vertical-align:middle" /> --%><% =ResString("btnSnapshotsCancel")%></button> 
                    <div style="margin-top:4px; text-align:center" class="text small gray"><a href="" onclick="selectItems(1); return false;" class="actions dashed"><% = ResString("lblSelectAll")%></a> | <a href="" onclick="selectItems(0); return false;" class="actions dashed"><% = ResString("lblSelectNone")%></a> | <a href="" onclick="selectItems(-1); return false;" class="actions dashed"><% = ResString("lblSelectInverse")%></a></div>
                </div>
            </nobr></div>
        </td></tr>
        <tr><td id='tdSnapshots' class="text"><div id='divSnapshots' style='text-align:left'></div></td></tr>
        <% if App.CanvasMasterDBversion<"0.99992" Then %><tr><td style="height:1ex" class="text"><div class="warning" style="text-align:center; font-size:11px; padding:2px 1ex;margin-top:4px;"><i class="fas fa-info-circle" title="(!)"></i> You need to upgrade your Master Database for use all features.</div></td></tr><% End if %>
    </table>
</asp:Content>