<%@ Page Language="VB" Inherits="WorkgroupsListPage" title="Workgroups List" Codebehind="WkgList.aspx.vb" %>
<%@ Register Assembly="DevExpress.Web.v9.1" Namespace="DevExpress.Web.ASPxCallback" TagPrefix="dxcb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<h4 style='margin-top:1em; padding-bottom:0px;'><% =PageTitle(CurrentPageID)%></h4>
<p align='center'><table border='0' cellspacing='0' cellpadding='0' width="98%">
<tr class='text' valign='bottom'>
    <td align="left"><input type="button" id="btnRefresh" style='width:9em' class="button" onclick="doRefresh(); return false;" value="Refresh"/>&nbsp;<% if App.CanUserDoAction(ecActionType.at_slCreateWorkgroup, App.ActiveUserWorkgroup, App.ActiveWorkgroup) Then %><input type='button' id="btnNewWorkgroup" style='width:9em' class="button" onclick="document.location.href='<% =PageURL(_PGID_ADMIN_WORKGROUP_CREATE, GetTempThemeURI(True))  %>'; return false" <% =IIf(CanCreateWorkgroup(), "", "disabled") %> value="<% =PageMenuItem(_PGID_ADMIN_WORKGROUP_CREATE) %>"/>&nbsp;<% End If %><input type="button" id="btnExport" style='width:9em' class="button" onclick="DoExport(); return false;" value="Export to Excel"/>&nbsp;<asp:button runat="server" ID="btnExtractParams" CssClass="button" Text="Extract Licenses" Visible="false" Width="9em"/></td>
    <td style="white-space:nowrap" align="right">
        &nbsp;Filter by expiration: <select id='dbFilter' onchange='ExpireFilter(this.value);'><option value='0' style='background:#f0f0f0'>No filter</option><option value='-12'>Last year</option><option value='-6'>-6 months</option><option value='-3'>-3 months</option><option value='-2'>-2 months</option><option value='-1'>-1 month</option><option value='1'>This month</option><option value='2'>In 2 months</option><option value='3'>In 3 months</option><option value='6'>In 6 months</option><option value='12'>Next year</option></select>&nbsp;
        <span id='optShowAll'>&nbsp;<input type='checkbox' name='cbShowAll' value='1' id='cbShowAll' onclick='ShowAll(this.checked);'/><label id='lblShowAll' for='cbShowAll'>Show all</label>&nbsp;</span>
        &nbsp;<input type='checkbox' name='cbShowExpired' value='1' id='cbShowExpired' onclick='CreateTable();' checked/><label for='cbShowExpired'><% =ResString("lblShowExpiredWkg")%>/non-valid</label>&nbsp;
        &nbsp;Search:&nbsp;<input type='text' name='search' class='input' style='width:12em' onkeyup='CreateTable();' value='<% =Checkvar("search", "") %>' />
    </td>
</tr>
<tr valign="top" align="center"><td class="text" colspan="3" id="tdList" style="padding-top:1ex"><div style='margin:6em' class='gray'><b><% =ResString("lblPleaseWait")%></b></div></td></tr>
</table></p>
<script type="text/javascript">
    <!--

    var Workgroups = [<% =GetWorkgroupsList %>];

    var lbl_desc        = "&nbsp;&#9660;";
    var lbl_asc         = "&nbsp;&#9650;";

    var expire_filter = 0;

    var sort_idx    = 1;
    var page_size   = 20;
    var page        = 0;
    var page_max_show = 20;

    var idx_id      = 0;
    var idx_name    = 1;
    var idx_created = 2;
    var idx_visited = 3;
    var idx_expires = 4;
    var idx_is_exp  = 5;
    var idx_projects= 6;
    var idx_usrmail = 7;
    var idx_usrname = 8;
    var idx_has_psw = 9;
    var idx_online  = 10;
    var idx_users   = 11;
    var idx_status  = 12;
    var idx_cmd     = 13;
    var idx_risk    = 14;
    var idx_lic_valid = 15;
    var idx_add_prj  = 16;
    var idx_opp_id   = 17;
    var idx_exp_val  = 18;
    var idx_instance = 19;
    var idx_see_lic  = 20;
    var idx_see_stat = 21;

    var wkg_active  = <% =App.ActiveWorkgroup.ID %>;
    var line_prev   = -1;

    var sl_theme    = '<% =GetTempThemeURI(true) %>';

    var dt_max = new Date();
    var dt_soon = new Date();
    dt_max.setFullYear(dt_max.getFullYear()+10);
    dt_soon.setMonth(dt_soon.getMonth()+3);
    
    function ConfirmDelete()
    {
        return (confirm('<% =JS_SafeString(ResString("msgConfirmDeleteWorkgroup")) %>'));
    }


    function doRefresh()
    {
        loadURL("?action=refresh");
        return false;
    }
    
    function doExport()
    {
        var lst = "";
        var wkg_lst = PrepareList(Workgroups);
        if (wkg_lst.length==Workgroups.length) lst = "all"; else for (var i=0; i<wkg_lst.length; i++) lst += (lst=="" ? "" : "|") + wkg_lst[i][idx_id];
        if (lst=="") dxDialog("Nothing to export", true, null); else CreatePopup('<% =PageURL(CurrentPageID, _PARAM_ACTION + "=export&ids=") %>' + lst, 'ExportWkg');
        return false;
    }
    

    function PrepareList(lst)
    {
        var res = [];
        var exp = theForm.cbShowExpired;
        if ((exp) && !(exp.checked))
        {
            for (var i=0; i<lst.length; i++)
                if (!(lst[i][idx_is_exp]) &&  lst[i][idx_instance]) res.push(lst[i]);
        }
        else
        {
          res = lst;
        }

        var srch = theForm.search;
        if ((srch) && (srch.value!="" || expire_filter!=0))
        {
            var s = eval("/" + srch.value + "/i");
            res2 = [];
            var a = new Date();
            var b = new Date();
            if (expire_filter<0) a.setMonth(a.getMonth()+expire_filter);
            if (expire_filter>0) b.setMonth(b.getMonth()+expire_filter);

            for (i=0; i<res.length; i++)
            {
               var do_add = 0;
               if (srch.value!="")
               {
                    var idx = (res[i][idx_name]).search(s);
                    if (idx<0) idx = (res[i][idx_usrmail]).search(s);
                    if (idx<0) idx = (res[i][idx_usrname]).search(s);
                    if (idx>=0) do_add = 1;
               }
               if (expire_filter!=0 && res[i][idx_expires]*1>=a && res[i][idx_expires]*1<=b) do_add = 1;
               if (do_add) res2.push(res[i]);
            }
            res=res2;
        }    

        return res;
    }

    function ExpireFilter(flt)
    {
        expire_filter = flt*1;
        if (flt==0 && (theForm.search)) theForm.search.value='';
        if (flt<0 && (theForm.cbShowExpired)) theForm.cbShowExpired.checked=1;
        CreateTable();
    }

    function ShowAll(all)
    {
        page = 0;
        CreateTable();
    }

    function SetPage(pg)
    {
        if ((theForm.cbShowAll)) theForm.cbShowAll.checked=0;
        page = pg;
        CreateTable();
    }

    function GetDataByID(id)
    {
        for (var i=0; i<Workgroups.length; i++)
            if (Workgroups[i][idx_id]==id) return Workgroups[i];
        return null;    
    }

    function SetRowFocus(id, f)
    {
        var tr = $get("tr" + id);
        if ((tr))
        {
            for (var i=0; i<tr.cells.length; i++) tr.cells(i).style.backgroundColor = (f) ? "#e0e0ff" : "";
        }    
    }

    function SwitchLine(id, f)
    {
        if ((f) && (line_prev>=0)) { var idx = line_prev;  SwitchLine(line_prev, 0); if (id==idx) return; }
        var tr = $get("tr" + id);
        if ((tr))
        {
            if ((f))
            {
                var tr_new = tr.parentNode.insertRow(tr.rowIndex+1);
                tr_new.setAttribute("id" , -id);
                tr_new.className = "text gray";
                var td = tr_new.insertCell(0);
                td.colSpan=9;
                td.bgColor="#f0f0ff";
                td.style.padding = "2px 1ex 6px 1em;";
                line_prev = id;
                var d = GetDataByID(id);
                if ((d))
                {
                    var valid = (d[idx_lic_valid] && d[idx_instance]);
                    var can_edit = (d[idx_cmd]&2) ;
                    var can_del = (d[idx_cmd]&1 && d[idx_status]<=0);
                    var can_tpl = (d[idx_status]<=0 && d[idx_add_prj] && valid);
                    <%--var sOpen = "<a href='<% =PageURL(_PGID_WORKGROUP_SELECT, _PARAM_ACTION + "=select&" + _PARAM_ID) %>=" + id + sl_theme + "' class='actions'>Open</a>";--%>
                    var sLicense = "<% =JS_SafeString(PageMenuItem(_PGID_ADMIN_LICENSE))%>";
                    if (d[idx_see_lic]) sLicense = "<a href='<% =PageURL(_PGID_ADMIN_LICENSE, _PARAM_ID) %>=" + id + sl_theme + "' class='actions'>" + sLicense + "</a>";
                    var sSamples = "<% =JS_SafeString(PageMenuItem(_PGID_ADMIN_WORKGROUP_SAMPLES))%>";
                    var sTemplates = "<% =JS_SafeString(PageMenuItem(_PGID_ADMIN_WORKGROUP_TEMPLATES))%>";
                    if (can_tpl)
                    {
                        sSamples = "<a href='<% =PageURL(_PGID_ADMIN_WORKGROUP_TEMPLATES, "type=samples&" + _PARAM_ID) %>=" + id + sl_theme + "' class='actions'>" + sSamples + "</a>";
                        sTemplates = "<a href='<% =PageURL(_PGID_ADMIN_WORKGROUP_TEMPLATES, _PARAM_ID) %>=" + id + sl_theme + "' class='actions'>" + sTemplates + "</a>";
                    }
                    var sDelete = "<% =JS_SafeString(PageMenuItem(_PGID_ADMIN_WORKGROUP_DELETE))%>";
                    if (can_del) sDelete = "<a href='<% =PageURL(_PGID_ADMIN_WORKGROUP_DELETE, _PARAM_ID) %>=" + id + sl_theme + "' class='actions' onclick='return ConfirmDelete()'>" + sDelete + "</a>";
                    var sEdit = "<% =JS_SafeString(PageMenuItem(_PGID_ADMIN_WORKGROUP_EDIT))%>";
                    if (can_edit) sEdit = "<a href='<% =PageURL(_PGID_ADMIN_WORKGROUP_EDIT, _PARAM_ID) %>=" + id + sl_theme + "' class='actions'>" + sEdit + "</a>";
                    var sLogs = "<% =JS_SafeString(PageMenuItem(_PGID_ADMIN_STATISTIC))%>";
                    if (d[idx_see_stat] && valid) sLogs = "<a href='<% =PageURL(_PGID_ADMIN_STATISTIC)%>?wkg=" + id + sl_theme + "' class='actions'>" + sLogs + "</a>";
                    var sExtra = "";
                    if (d[idx_opp_id]!="") sExtra = "<span class='small' style='color:#333333; padding-top:2px; padding-right:1em; float:right'><nobr><% =JS_SafeString(ResString("lblOpportunityID")) %>:&nbsp;" + d[idx_opp_id] + "</nobr></span>";
                    td.innerHTML = sExtra + "» " + sLicense + " | " + sSamples + " | " + sTemplates + " | " + sEdit+ " | " + sLogs + " | " + sDelete;
                }    
            }
            else
            {
                tr.parentNode.deleteRow(tr.rowIndex+1);
                line_prev = -1;
            }
        }
    }


    function GetDate(dt, is_expires)
    {
        if (is_expires && dt>dt_max) return "<% = JS_SafeString(REsString("lblNever")) %>";    // more than 10 years from today;
        if (dt<=1000) return "&nbsp;";
        var dd = dt.getDate();  
        if (dd<10) dd='0'+dd; 
        var mm = dt.getMonth() + 1;
        if (mm<10) mm= '0'+mm; 
        var yy = dt.getFullYear();// % 100;  
        //if (yy<10) yy= '0'+yy; 
//        return mm + "-" + dd + "-" + yy;
        return yy + "-" + mm + "-" + dd;
    }

    function CreateSortColumn(id, text)
    {
        var act = (Math.abs(sort_idx)==id);
        return "<a href='' class='actions' onclick='return DoSort(" + (act ? -sort_idx : id*(id=idx_name ? 1 : -1)) + ");'>" + text + (act ? (sort_idx <0 ? lbl_desc : lbl_asc) : "") + "</a>";
    }

    function DoSort(srt)
    {
        sort_idx = srt;
        Workgroups = Workgroups.sort(SortList);
        CreateTable();
        return false;
    }

    function SortList(a, b)
    {
        var res = 0;
        var idx = Math.abs(sort_idx);
        var x = a[idx];
        var y = b[idx];
        if (idx==idx_name) { x=x.toLowerCase(); y=y.toLowerCase(); }
        res = ((x==y) ? 0 : ((x<y) ? -1 : 1));
        if (sort_idx<0) res=-res;
        return res;
    }

    function ShowSearch(val)
    {
        var srch = theForm.search;
        if ((srch) && (srch.value!=""))
        {
            var s = eval("/" + srch.value + "/i");
            var l = srch.value.length;
            var idx = val.search(s);
            if (idx>=0) val = val.substr(0, idx) + "<span style='background:#fff066;padding:0px 1px;'>" + val.substr(idx, l) + "</span>" + val.substr(idx+l);
        }
        return val;
    }

    function CreateTable()
    {
        var td = $get("tdList");
        if ((td))
        {
            var lst = PrepareList(Workgroups);
            if (lst.length==0)
            {
                td.innerHTML = "<h6 style='margin:4em'>List is empty</h6>";
            }
            else
            {
//                theForm.disabled = 1;
                td.innerHTML = "<table border=0 cellspacing=1 cellpadding=2 id='tblList'><tbody><tr class='tbl_hdr text'><td style='width:35%'>" + CreateSortColumn(idx_name, "Workgroup name") + "</td><td>" + CreateSortColumn(idx_projects, "Projects") + "</td><td>" + CreateSortColumn(idx_users, "Users") + "</td><td>" + CreateSortColumn(idx_online, "Online") + "</td><td>" + CreateSortColumn(idx_created, "Created") + "</td><td>" + CreateSortColumn(idx_visited, "Last Visited") + "</td><td>" + CreateSortColumn(idx_expires, "Expires") + "</td><td>" + CreateSortColumn(idx_status, "Status") + "</td><td>" + CreateSortColumn(idx_usrmail, "Managed by") + "</td><!--td>Cmd</td--></tbody></table>";
                var tbl = $get("tblList");
                var s = ((theForm.search) && theForm.search.value)+"";
                if ((tbl))
                {
                    var i_s = 0;
                    var i_e = lst.length;
                    var sPagesList = "";

                    if ((theForm.cbShowAll) && !(theForm.cbShowAll.checked) && i_e>page_size*2) 
                    {
                        if (page<0) page = 0;
                        var page_max = Math.ceil(lst.length/page_size);
                        if (page>=page_max) page = page_max-1;
                        i_s = page * page_size;
                        i_e = i_s + page_size;
                        if (i_e > lst.length) i_e = lst.length;
                        sPagesList = "";
                        for (var i=0; i<Math.ceil(lst.length/page_size); i++)
                            if (page_max>page_max_show)
                            {
                                sPagesList += "<option value='" + i + "'" + (i==page ? " selected" : "") + ">" + (i+1) + "</option>";
                            }
                            else
                            {
                                sPagesList += (i ? "&nbsp;<span class='gray'>|</span>&nbsp;" : "") + (page==i ? "<b>" + (i+1) + "</b>" : "<a href='' onclick='SetPage(" + i + "); return false' class='actions'>" + (i+1) + "</a>");
                            }
                        if (page_max>page_max_show) sPagesList = "<select name='pages' onchange='SetPage(this.value);'>" + sPagesList + "</select>";
                        if (page>0) sPagesList = "<a href='' onclick='SetPage(" + (page-1) + "); return false' class='actions'>&laquo;&laquo;</a>&nbsp;" + (page_max>page_max_show ? "" : "<span class='gray'>|</span>&nbsp;") + sPagesList;
                        if (page<page_max-1) sPagesList += (page_max>page_max_show ? "" : "&nbsp;<span class='gray'>|</span>") + "&nbsp;<a href='' onclick='SetPage(" + (page+1) + "); return false' class='actions'>&raquo;&raquo;</a>";
                        sPagesList = "Page: " + sPagesList;

                    }
                    for (var i=i_s; i<i_e; i++)
                    {
                        var w = lst[i];
                        
                        var r = tbl.rows.length;
                        tbl.insertRow(r);
                        tbl.rows[r].className = "text aslink " + ((r&1) ? "grid_row" : "grid_row_alt") + (w[idx_id]==wkg_active ? " grid_row_sel" : "");
                        tbl.rows[r].setAttribute("valign" , "middle");
                        tbl.rows[r].setAttribute("id" , "tr" + w[idx_id]);
                        tbl.rows[r].setAttribute("onmouseover" , "SetRowFocus(" + w[idx_id] + ",1);");
                        tbl.rows[r].setAttribute("onmouseout" , "SetRowFocus(" + w[idx_id] + ",0);");
                        tbl.rows[r].setAttribute("onclick" , "SwitchLine(" + w[idx_id] + ",1);");
                        
                        var n = ShowSearch(w[idx_name]);
                        if (w[idx_projects]>0 && w[idx_visited]>1000 && !w[idx_is_exp] && w[idx_instance]) n="<b>"+n+"</b>";
                        var img = "<img src='" + (!w[idx_lic_valid] ? "<%=ImagePath%>discard_14.png" : "/images/" + (w[idx_status]==<% =ecWorkgroupStatus.wsSystem %> ? "wkg_system.png" : (w[idx_name]=="<% = JS_SafeString(_DB_DEFAULT_STARTUPWORKGROUP_NAME) %>" ? "wkg_startup.png" : (w[idx_risk] ? "wkg_riskion.png" : "wkg_comparion.png")))) + "' width=16 height=16 title='' border=0 style='margin-right:5px'>";
                                                
                        var idx=0;
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "left");
                        tbl.rows[r].cells[idx].innerHTML = img + n;

                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "right");
                        tbl.rows[r].cells[idx].innerHTML = 1*w[idx_projects];
                        
                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "right");
                        tbl.rows[r].cells[idx].innerHTML = 1*w[idx_users];
                        
                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "right");
                        tbl.rows[r].cells[idx].innerHTML = w[idx_online]*1;
                        
                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "center");
                        tbl.rows[r].cells[idx].innerHTML = "<nobr>" + GetDate(new Date(1*w[idx_created]), 0) +"</nobr>";

                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "center");
                        tbl.rows[r].cells[idx].innerHTML = "<nobr>" + GetDate(new Date(1*w[idx_visited]), 0) +"</nobr>";

                        var now = new Date();
                        var dt_expires = new Date(1*w[idx_expires]);

                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "center");
                        tbl.rows[r].cells[idx].innerHTML = "<nobr>" + w[idx_exp_val] +"</nobr>";
                        if (w[idx_is_exp] || !w[idx_instance])
                        { 
                            tbl.rows[r].className += ((!w[idx_lic_valid]) ? " error_dark" : " error");
                        } 
                        else
                        { 
                            if (w[idx_projects]==0) tbl.rows[r].className +=" gray"; 
                            if (!w[idx_is_exp] && w[idx_expires]>0 && w[idx_expires]<dt_soon) tbl.rows[r].cells[idx].innerHTML = "<i>" + tbl.rows[r].cells[idx].innerHTML +"</i>";
                        }
                        
                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "center");
                        tbl.rows[r].cells[idx].innerHTML = (w[idx_status] == <% =ecWorkgroupStatus.wsSystem %> ? "System" : (!w[idx_lic_valid] ? "No license" : (w[idx_is_exp] ? "Expired" : (!w[idx_instance] ? "Wrong InstanceID" : (w[idx_status]<0 ? "Disabled" : (w[idx_name] == '<% = JS_SafeString(_DB_DEFAULT_STARTUPWORKGROUP_NAME) %>' ? "Startup" : ((!w[idx_is_exp] && w[idx_expires]>0 && w[idx_expires]<dt_soon) ? "<i>Will&nbsp;expire</i>" : "Enabled")))))));
                        if (w[idx_status]<0) tbl.rows[r].className +=" gray"; 

                        idx+=1
                        tbl.rows[r].insertCell(idx);
                        tbl.rows[r].cells[idx].setAttribute("align" , "left");
                        tbl.rows[r].cells[idx].innerHTML = "&nbsp;" +  (w[idx_usrmail] == "" ? "" : "<a href='mailto:" + w[idx_usrmail] + "'>" + ShowSearch(w[idx_usrname] == "" ? w[idx_usrmail] : w[idx_usrname]) + "</a>" + (w[idx_has_psw] ? "" : "*"));
                        
                    }
                    td.innerHTML += "<div class='text' style='margin-top:1ex; padding:1px 1ex; border:1px solid #d0d0d0; text-align:center'><span class='small gray' style='float:right;'>* User has blank password</span><span style='text-align:left;width:17em;float:left;' class='small'>Show: " + (i_s+1) + "-" + i_e + " of " + lst.length +  (lst.length ==  Workgroups.length ? "" : " (total: "  + Workgroups.length + ")") + "</span>&nbsp;" + sPagesList + "&nbsp;</div>";
                }    
//                theForm.disabled=0;
            }    
        }
    }

    function Init()
    {
        var showall = theForm.cbShowAll;
        var lbl_showall = $get("lblShowAll");
        if ((showall) && (lbl_showall) && Workgroups.length<page_size*2) { showall.checked=1; showall.style.display='none'; lbl_showall.style.display=showall.style.display; }
//        if ((showall) && (lbl_showall) && Workgroups.length<page_size*2) { showall.checked=1; showall.disabled=1; lbl_showall.disabled=1; }
//        if ((showall) && (lbl_showall) && Workgroups.length>15*page_size) { showall.checked=0; showall.disabled=1; lbl_showall.disabled=1; }
        if (Workgroups.length>200) $("#optShowAll").hide();
        if ((theForm.dbFilter)) ExpireFilter(theForm.dbFilter.value); else CreateTable();
    }

    function ReloadWorkgroups()
    {
        if ((window.parent) && (typeof (window.parent.ReloadWorkgroupsList) == "function")) {
            window.parent.ReloadWorkgroupsList();
        }
    }

    $(document).ready(function () {
        <% If App.SystemWorkgroup IsNot Nothing AndAlso (App.SystemWorkgroup.License Is Nothing OrElse Not App.SystemWorkgroup.License.isValidLicense) Then %>
        var result = DevExpress.ui.dialog.confirm("The License for System Workgroup is invalid or missing. Would you like to change it?", resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                loadURL("<% =JS_SafeString(PageURL(_PGID_ADMIN_WORKGROUP_EDIT, _PARAM_ID + "=" + App.SystemWorkgroup.ID.ToString)) %>");
            }
        });<% Else %>setTimeout('theForm.search.focus();', 300);<% End if %>
        <% If App.SystemWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup IsNot Nothing AndAlso App.ActiveWorkgroup.Status = ecWorkgroupStatus.wsSystem AndAlso App.SystemWorkgroup.License IsNot Nothing AndAlso App.SystemWorkgroup.License.isValidLicense AndAlso App.SystemWorkgroup.License.GetParameterValueByID(ecLicenseParameter.MaxWorkgroupsTotal) = 0 Then %>
        var result = DevExpress.ui.dialog.confirm("Do you want to add a new workgroup?", resString("titleConfirmation"));
        result.done(function (dialogResult) {
            if (dialogResult) {
                loadURL("<% =JS_SafeString(PageURL(_PGID_ADMIN_WORKGROUP_CREATE)) %>");
            }
        });<% End if %>
    });

//    Init();  

// -->
</script>
</asp:Content>

