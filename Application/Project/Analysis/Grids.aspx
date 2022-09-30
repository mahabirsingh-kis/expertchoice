<%@ Page Language="VB" Inherits="GridsPage" title="Grids" Codebehind="Grids.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script language="javascript" type="text/javascript">

    var tabID = <% =If(NavigationPageID = _PGID_ANALYSIS_GRIDS_ALTS OrElse CurrentPageID = _PGID_ANALYSIS_GRIDS_ALTS, 0, 1) %>; <%--<% App.ActiveProject.ProjectManager.Parameters.Synthesis_GridsTab %>--%>
    var showAlts = tabID == 0;
    var tabs_loaded = false;

    <%--function onSetPage(pgid) {
        var tab = -1;
        switch (pgid % _pgid_max_mod) {
            case <% =_PGID_ANALYSIS_GRIDS_ALTS %>:
                tab = 0;
                break;
            case <% =_PGID_ANALYSIS_GRIDS_OBJS %>:
                tab = 1;
                break
        }
        if (tab>=0) {
            setActiveTab(tab);
            return true;
        }
        return false;
    }--%>

    function onSetAIP() {
        $("#tabs").dxTabPanel("instance").option("items")[1].disabled = (use_aip);
        $("#tabs").dxTabPanel("instance").repaint();
    }
    
    function loadTab(value) {
        showAlts = value == 0;
        var frm = document.getElementById("frmContentAlternatives");
        var src = "";
        if ((frm)) {
            switch (value*1){
                case 0 :
                    src = "<% = PageURL(_PGID_ANALYSIS_OVERALL_ALTS) %>&temptheme=sl";
                    break;
                case 1:
                    src = "<% = PageURL(_PGID_ANALYSIS_OVERALL_OBJS) %>&temptheme=sl";
                    break;
            }
            if (src != "") {
                setTabsState(true);
                initFrameLoader(frm);
                setTimeout(function () { 
                    showLoadingPanel();
                    document.getElementById("frmContentAlternatives").src = src;
                }, 30);
            }
        }
    }

    function setActiveTab(value) {
        showLoadingPanel();
        loadTab(value);
        $("#tabs").dxTabPanel('instance').option("selectedIndex", value);
        callAPI("pm/dashboard/?<% =_PARAM_ACTION %>=set_active_grids_tab", { "name" : "value", "value" : value }, onSetActiveTab);
    };

    function onSetActiveTab(data) {
        hideLoadingPanel();
    }

    function onSwitchAdvancedMode(adv) {
        var f = document.getElementById("frmContentAlternatives");
        if ((f) && typeof f.contentWindow.onSwitchAdvancedMode == "function") {
            f.contentWindow.isAdvancedMode = adv;
            f.contentWindow.onSwitchAdvancedMode(adv);
        }
    }

    function onResize() {
        var f = $("#frmContentAlternatives");
        if ((f) && f.length) {
            f.width(300).height(200);
            f.height(f.parent().height() - 51).width($("#tabs").width() - 2);
        }
    }

    function setTabsState(disabled) {
        if ((tabs_loaded)) $("#tabs").dxTabPanel("instance").option("disabled", (disabled));
        //displayLoadingPanel(!disabled);
        hideLoadingPanel();
    }

    function getWidgetParams() {
        var f = document.getElementById("frmContentAlternatives");
        if ((f) && typeof f.contentWindow.getWidgetParams == "function") {
            return f.contentWindow.getWidgetParams();
        }
        return {};
    }

    function onGetDirectLink() {
        return list2params(getWidgetParams());
    };

    function onGetReportLink() {
        var url = document.location.href;
        var pg = pageByID(pgID);
        if ((pg) && typeof pg.url != "undefined" && pg.url != "") url == pg.url;
        return url + "&<% =_PARAM_ACTION %>=upload&" + onGetDirectLink();
    }

    function onGetReportTitle() {
        var f = document.getElementById("frmContentAlternatives");
        if ((f) && typeof f.contentWindow.onGetReportTitle == "function") {
            return f.contentWindow.onGetReportTitle();
        }
        return "";
    }

    function onGetReport(addReportItem) {
        var t = _rep_item_ObjsGrid;
        if (typeof addReportItem == "function") {
            switch (pgID) {
                case <% = _PGID_ANALYSIS_GRIDS_ALTS %>:
                    t = _rep_item_AltsGrid;
                    break;
                case <% = _PGID_ANALYSIS_CHARTS_OBJS %>:
                    t = _rep_item_ObjsGrid;
                    break;
            }
            addReportItem({"name": onGetReportTitle(), "type": t, "edit": document.location.href, "export": "", "ContentOptions": getWidgetParams()});
        }
    }

    resize_custom = onResize;

    $(document).ready(function () {
        $("#tabs").dxTabPanel({
            items: [{id:0, title:"<% =JS_SafeString(ParseString("%%Alternatives%%")) %>", icon:"icon ec-alts"},
                    {id:1, title:"<% =JS_SafeString(ParseString("%%Objectives%%")) %>", icon:"icon ec-hierarchy", disabled: (use_aip)}],
            selectedIndex: (showAlts ? 0 : 1),
            disabled: true,
            onSelectionChanged: function(e) {
                if (e.addedItems.length > 0) {
                    var newValue = e.addedItems[0].id;
                    var pg = -1;
                    if (newValue == 0 && !showAlts) pg = <% =_PGID_ANALYSIS_GRIDS_ALTS %>; 
                    if (newValue == 1 && showAlts) pg = <% =_PGID_ANALYSIS_GRIDS_OBJS %>; 
                    if (typeof navOpenPage == "function" && (pg>0) && (pg!=pgID)) {
                        navOpenPage(pg, true);
                    } else {
                        if (newValue == 0 && !showAlts || newValue == 1 && showAlts) {
                            setActiveTab(newValue);
                        }
                    }
                }
            }
        });
        onResize();
        loadTab(tabID);
        tabs_loaded = true;
    });

</script>

<%--<div style="right:0; top:0; position: absolute; padding-top:10px; padding-right:12px; text-align:right;"><div id="btnShowToolbar"></div></div>--%>
<div id="tabs" class="ec_tabs ec_tabs_nocontent" style="margin-top: 8px; width: 99%; display: none;"></div>
<iframe class="whole" style="border: 1px solid #cccccc; margin-top:-1px;" id="frmContentAlternatives" frameborder='0' scrolling='no' onload="setTabsState(false);"></iframe>

</asp:Content>