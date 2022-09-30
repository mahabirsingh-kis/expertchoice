<%@ Page Language="vb" CodeBehind="Landing.aspx.vb" Inherits="ComparionLandingPage" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="PageContent" runat="server">
<script type="text/javascript">

    var init = true;
    var counterIDs = [];

    function isOneSection(pgid) {
        var oneSection = true;
        var pg = pageByID(nav_json, pgid);
        if ((pg) && typeof pg.items != "undefined") {
            if (pg.items.length > 1) {
                for (var i=0; i<pg.items.length; i++) {
                    var page = pg.items[i];
                    if (page.items) {
                        oneSection = false;
                        break;
                    }
                }
            }
        }
        return oneSection;
    }

    function onResize(force, w, h) {
        //$("#divContent").height(h-16);
        //$("#divContent").width(w-18);
    }

<%--    function onSwitchAdvancedMode() {
        //$("#trOption").toggle(isAdvancedMode);
        $("#trOption").toggle(<% If Not App.HasActiveProject Then %>false<% Else %>isAdvancedMode<% End If %>);
    }--%>
    
    function loadLandingInfo() {
        callAPI("project/?action=landing_info", {"ids": counterIDs.join(",")}, onLoadLandingInfo, "Collect data...");
    }

    function loadEvalProgress() {
        callAPI("project/?action=eval_progress", {}, onLoadEvalProgress, "Collect data...");
    }

    function onLoadLandingInfo(res) {
        if (isValidReply(res) && res.Result == _res_Success && typeof res.Data != "undefined") {
            for (var key in res.Data) {
                var vp = res.Data[key];
                drawPlaceHolderInfo(key, vp);
            }
        }
    }

    function onLoadEvalProgress(res) {
        if (isValidReply(res) && res.Result == _res_Success && typeof res.Data != "undefined") {
            $("#evalProgressBar").width(res.Data + "%");
            $("#evalProgressVal").html(res.Data + "%");
            $("#evalProgressBar").parent().removeClass("preloader_dots");
        }
    }

    function drawPlaceHolderInfo(key, val) {
        var placeHolder = $("#ph" + key);
        if ((placeHolder)) {
            switch (key) {
                //case "goal":
                //    //placeHolder.append($("<div><i class='fas fa-bullseye'></i>" + val + "</div>"));
                //    break;
                case "<% = _PGID_ANTIGUA_MEETING %>":
                case "<% = _PGID_ANTIGUA_MEETING_LIKELIHOOD%>":
                case "<% = _PGID_ANTIGUA_MEETING_IMPACT%>":
                    if (val == "has_structuring_data") {
                        placeHolder.append($("<div class='count extra-icon' title='<% =JS_SafeString(ResString("lblHasStructuringData")) %>'><i class='fas fa-check'></i></div>"));
                    }
                    break;
                default:
                    placeHolder.append($("<div class='count" + (val > 0 ? " count-orange" : "") + "'>" + val + "</div>"));
            }
        }
    }

    function initPageLinkLanding(element, pgid, onclick) {
        if ((element)) {
            var lnk = '';
            var apage = pageByID(nav_json, pgid);
            if (apage) {
                var icon = "";
                if (apage.icon != "") icon = '<i class="lp-link-icon ' + apage.icon + '"></i>';
                //lnk = (apage.title == "" ? apage.text : apage.title);
                lnk = apage.text;
                if (typeof apage.disabled != "undefined" || !(apage.disabled)) {
                    lnk = '<a ' + (typeof onclick != "undefined" && onclick!="" ? 'href="" onclick="' + onclick + '" style="cursor:default; text-overflow: ellipsis; color: gray"' : 'href="' + (apage.url == "" ? "" : apage.url) + '" onclick="onOpenPage(' + (apage.pgID > 0 ? apage.pgID : apage.pguid) + '); return false;"') + ' title="' + htmlEscape(apage.title == "" || apage.text == apage.title ? "" : apage.title) + '">' + lnk + '</a>';
                }
                var extra = "";
                if (typeof apage.extra != "undefined" && apage.extra.indexOf("new") >= 0) {
                    if (apage.pgID) {
                        var c = localStorage.getItem("new" + apage.pgID);
                        if (typeof c == "undefined" || c == null || c == "") {
                            extra += "<span class='menu_new' title='" + resString("lblNewFeature") + "'><i class='fas fa-certificate' style='margin-top:3px'></i><span>N</span></span>";
                        }
                    }
                }
                //lnk = "<table style='width:100%;' border=0 cellspacing=0 cellpadding=1><tr valign=middle>" + icon + "<td width='95%'><div class='lp-link-label'>" + lnk + "</div></td><td align=right><div id='ph"+apage.uid+"'></div></td></tr><table>";
                lnk = "<div class='lp-link-label'>" + icon + lnk + extra + "<div id='ph" + apage.uid + "' style='float:right'></div></div>";
                element.html(lnk);
            }
        }
    }

    function generateLandingPageLinks(pageID) {
        var pg = pageByID(nav_json, pageID);
        var showTitle = false;
        var hasItems = false;
        if ((pg) && typeof pg.items != "undefined") {
            for (var i=0; i<pg.items.length; i++) {
                var page = pg.items[i];
                if (page.uid !== "20063" && !page.items) {
                //if (page.uid !== "20063" && !page.items && !page.disabled) {
                    showTitle = true;
                    break;
                }
            }
            if (showTitle) {
                $("#menus").append('<div id="linksgroup'+pageID+'" class="lp-nocolumn-wrap"></div>')
                var titleItem = '<div class="box box-header box-header-blue lp-box-header">';
                //titleItem += '<h3>' + pg.text + '</h3>';
                var extra = '';
                // add extra for "Define Model"
                if (pageID == 13446493) extra = '<%= If(App.HasActiveProject, JS_SafeString(String.Format(" for '{0}'", ShortString(App.ActiveProject.HierarchyObjectives.Nodes(0).NodeName, 65))), "") %>';
                titleItem += '<h3>' + pg.text + extra + '</h3>';
                //var hint = "Description";
                //titleItem +='<i class="fas fa-question-circle qhint" title="' + hint + '"></i></div>';
                titleItem += '</div>';
                $("#linksgroup"+pageID).append($(titleItem));
                //var goalItem = '<div id="phgoal" class="lp-info-line" style="font-weight:bold"></div>';
                //$("#linksgroup"+pageID).append($(goalItem));
                $("#linksgroup"+pageID).append($('<div id="linksbox'+pg.pgID+'" class="lp-links-box-2"></div>'))
            }
            for (var i=0; i<pg.items.length; i++) {
                var page = pg.items[i];
                if (!page.items) {
                    //if (!page.disabled) {

                        var onclick = "";
                        var show_warning = false;
                        if (typeof page.extra != "undefined" && (typeof page.items == "undefined" || !page.items.length)) {
                            if (page.extra.indexOf(menu_option_alts) >= 0 && typeof has_alts != "undefined") {
                                page.disabled = !(has_alts);
                                if (!(has_alts)) {
                                    show_warning = true;
                                    onclick = "msgNoAlts(); return false;";
                                }
                            }
                            if (page.extra.indexOf(menu_option_noAIP) >= 0 && typeof use_aip != "undefined") {
                                page.disabled = (use_aip);
                                if ((use_aip)) {
                                    show_warning = true;
                                    onclick = "msgUseAIP(); return false;";
                                }
                            }
                        }

                        $("#linksbox"+pg.pgID).append($('<div class="lp-icon-links" id="nav' + page.pgID + '"></div>'));
                        if (page.disabled && typeof page.extra != "undefined" && page.extra.indexOf("hide")>=0) {
                            // just ignore it if "hide" specified and page is disabled
                        } else {
                            initPageLinkLanding($("#nav"+page.pgID), page.pgID, onclick);
                            if (show_warning) {
                                $("#ph" + page.pgID).html("<div class='count' style='background:#cc0000;'><a href='' onclick='" + onclick +"' style='color:white'><b>?</b></a></div>");
                            }
                            counterIDs.push(page.pgID);
                            hasItems = true;
                        }
                    //}
                } else {
                    if (generateLandingPageLinks(page.pgID)) hasItems = true;
                }
            }
            if (!hasItems && showTitle) {
                $("#linksgroup"+pageID).remove();
            }
        }
        return hasItems;
    }

    function initLandingPageContent() {
        if ($("#evalProgressVal").length) { loadEvalProgress() };
        if ($("#evalProgressTitle")) { 
            var apage = pageByID(nav_json, "20063");
            var pageUrl = "";
            var icon = "";
            if (apage) { 
                pageUrl = apage.url;
                icon = '<i class="' + apage.icon + '"></i>';
            };
            $("#evalProgressTitle").html('<a href="'+ pageUrl +'">' + icon + ' <% =JS_SafeString(ResString("lblOverallEvalProgress")) %></a>') 
        };
        if ($("#menus").length) $("#menus").addClass(isOneSection(pgID) ? "lp-links-box-one-column" : "lp-links-box");
        return generateLandingPageLinks(pgID);
    }

    function showTitle(pgid) {
        var pg = pageByID(nav_json, pgid);
        if ((pg) && (pg.title != "" || pg.text !="")) {
            var name = (pg.title != "" ? pg.title : pg.text);
            if (name != "") document.title = replaceString("$$$$", name, "<% =GetPageTitle("$$$$") %>");
        }
    }

    resize_custom = onResize;

    $(document).ready(function () {
        //onSwitchAdvancedMode();
        <%--<% If Not isSLTheme Then %>if (typeof toggleSideBarMenu == "function") toggleSideBarMenu(true, 0, false);<% End if %>--%>
       <%-- var menu = $("#menus");
        var hasItems = false;
        if ((menu) && (menu.length)) hasItems = initLandingPageContent();
        if (hasItems) {
            loadLandingInfo();
            document.cookie = "landing_<% =NavigationPageID %>_<% =App.ProjectID  %>=1;";
            if ($("#cbShowLanding").length) {
                $("#cbShowLanding").dxCheckBox({
                    value: <% =Bool2JS(App.isAuthorized AndAlso Not ShowLandingPages(App, App.ActiveUser.UserID)) %>,
                    text: "<% = JS_SafeString(String.Format(ResString("optLandingPage"), PageMenuItem(_PGID_ACCOUNT_EDIT))) %>",
                    onValueChanged: function(e) {
                        callAPI("account/?action=option", {"name": "ShowLanding", "value": !(e.value)}, hideLoadingPanel);
                    }
                });
            }
        } else {
            loadURL("<% =PageURL(_PGID_PROJECTSLIST) %>");
        }
        init = false;--%>
    });

</script>
<%--<table class="whole" cellspacing="0" cellpadding="0">
<tr><td valign="top" style="padding-top:1em;">--%><div id="divContent" class="whole" style="overflow:auto;"><!-- Landing content begin -->

<%--<% If CurrentPageID = _PGID_LANDING_COMMON OrElse CurrentPageID = _PGID_LANDING_RISK_IDENTIFY Then %>
<div class="lp-page" style="margin-top:1em;">
    <div id="menus"></div>
</div>
<% End if%>


<% If CurrentPageID = _PGID_LANDING_STRUCTURE Then %>
<div class="lp-page">
    <div class="row box definition">
        <div class="lp-title"><i class="dx-icon icon ec-hierarchy hover"></i>&nbsp;<% =ResString("titleDefineModel") %></div>
        <p><% = ResString("msgLandingStructure") %></p>
    </div>
    <div id="menus"></div>
</div>
<% End if%>


<% If CurrentPageID = _PGID_LANDING_MEASURE Then %>
<div class="lp-page">
    <div class="row box definition">
        <div class="lp-title"><i class="dx-icon fa fa-keyboard hover"></i>&nbsp;<% = ResString("titleCollectInput") %></div>
        <p><% = ResString("msgLandingMeasure") %></p>
    </div>
    <div id="menus">
        <div class="box box-light-gray evaluation-progress">
            <h3 id="evalProgressTitle" class="title"></h3>
            <div class="progress-detail buffer-bottom">
                <table>
                    <tr>
                        <td style="width:100%;">
                            <div class="progress preloader_dots">
                                <div id="evalProgressBar" class="progress-bar" style="width:0%;"></div>
                            </div>
                        </td>
                        <td>
                            <div id="evalProgressVal" class="percent"><img src="../App_Themes/EC2018/images/loading.gif" /></div>
                        </td>
                    </tr>
                </table>            
            </div>
            <div id="navProgress" style="padding-left:1ex;"><a href="<% =SafeFormString(PageURL(_PGID_MEASURE_EVAL_PROGRESS)) %>"><i class="fas fa-chevron-circle-right"></i>&nbsp;<% =ResString("lblViewEvalProgress") %></a></div>
        </div>
    </div>
</div>
<% End if%>


<% If CurrentPageID = _PGID_LANDING_RESULTS Then %>
<div class="lp-page">
    <div class="row box definition">
        <div class="lp-title"><i class="dx-icon fa fa-chart-bar hover"></i>&nbsp;<% = ResString("mnuSeeResults") %></div>
        <p><% = ResString("msgLandingResults") %></p>
    </div>
    <div id="menus">
    <div class="box box-light-gray evaluation-progress">
        <h3 id="evalProgressTitle" class="title"></h3>
        <div class="progress-detail">
            <div class="progress">
                <div id="evalProgressBar" class="progress-bar" style="width: 0%"></div>
            </div>
            <div id="evalProgressVal" class="percent"><img src="../App_Themes/EC2018/images/loading.gif" /></div>
        </div>
        <div id="navProgress" style="padding-top:1ex;"><a href="<% =SafeFormString(PageURL(_PGID_MEASURE_EVAL_PROGRESS)) %>"><i class="fas fa-chevron-circle-right"></i>&nbsp;<% =ResString("lblViewEvalProgress") %></a></div>
    </div>
    </div>
</div>
<% End If%>


<% If CurrentPageID = _PGID_LANDING_ALLOCATE Then %>
<div class="lp-page">
    <div class="row box definition">
        <div class="lp-title"><i class="dx-icon fa fa-balance-scale hover"></i>&nbsp;<% = ResString("mnuAllocate") %></div>
        <p><% = ResString("msgLandingAllocate") %></p>
    </div>
    <div id="menus"></div>
</div>
<% End If%>


<% If CurrentPageID = _PGID_LANDING_REPORTS Then %>
<div class="lp-page">
    <div class="row box definition">
        <div class="lp-title"><i class="dx-icon fa fa-print hover"></i>&nbsp;<% = ResString("mnuViewReports") %></div>
        <p><% = ResString("msgLandingReports") %></p>
    </div>
    <div id="menus"></div>
</div>
<% End If%>--%>

<!-- Landing content end --></div><%--</td></tr>
<tr id="trOption" class="on_advanced" style="height:1em; color:#feffb4"><td class="text" style="padding:1ex 1ex; text-align:left; border-top:1px solid #f0f0f0;"><div id="cbShowLanding"></div></td></tr>
</table>--%>
</asp:Content>