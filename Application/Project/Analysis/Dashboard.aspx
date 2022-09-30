<%@ Page Language="VB" Inherits="DashboardPage" title="Dashboard" Codebehind="Dashboard.aspx.vb" %>
<asp:Content ContentPlaceHolderID="head_JSFiles" runat="server">
<script type="text/javascript" src="/Scripts/drawMisc.js"></script>
<script type="text/javascript" src="/Scripts/canvg.min.js"></script>
<script type="text/javascript" src="/Scripts/ec.charts.js"></script>
<script type="text/javascript" src="/scripts/ec.sa.js"></script>
<script type="text/javascript" src="/scripts/ec.dashboard.grid.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
<script type="text/javascript">

    loadSVGLib();

    // The list of specific function where XXX is ItemType:
    // getXXX(item) - return html/object for place the widget;
    // getXXXName(item) - get the item DOM element name (ID)
    // getXXXElement(item) - get the item DOM element itself like $("#" + getXXName(item));
    // resizeXXX(item) - resize the element
    // resetXXX(item) - on reset/dispose widget (if required)
    // toolbarXXX(item) - get list of widget toolbar items
    // updateXXOption - on setting a widget option (can be reset/init cotrol/toolbar, etc);

    const _sess_dash_viewmode = "DashViewMode";
    const noborders_class = "dashbrd_noborders";
    const cmAIJ = <%=CInt(CombinedCalculationsMode.cmAIJ)%>;
    const cmAIP = <%=CInt(CombinedCalculationsMode.cmAIPTotals)%>;

    dashboardID = <% =CheckVar("dashboard_id", -1) %>;
    if (dashboardID<0) dashboardID = getCurDashboardID();
    var dashboardID_uri = "<% =CheckVar("dashboard_id", "") %>";
    var dashboard_pg_title = document.title;

    _dash_viewmode = localStorage.getItem(_sess_dash_viewmode) * 1;                    // 0 - regular, 1 - fit to width
    _dash_viewmode_allowed = true;
    var viewOnly = <% =Bool2JS(ViewOnly) %>;

    var _dash_cellMinWidth = 150;
    var _dash_cellWidth = 200;              // will be recalculated on render/resize
    var _dash_cellHeight = 200;             // will be recalculated on render/resize
    var _dash_cellsPerRow = 6;              // will be recalculated on render/resize
    var _dash_cellsPerCol = 4;              // will be recalculated on render/resize
    var _dash_cellMargin = 16;

    var _dash_def_item_new = {
        "ID":  9999999,
        "Index": 1,
        "ItemType": _rep_item_New,
        "ItemOptions":  {"cols": -1, "rows": 2, "showTitle": true}
    };

    var _dash_placeholder_when_empty = false;

    var dashboardTiles = null;
    var dashboardTiles_updating = false;
    var _dash_ElementID = "#divDashboard";
    var _dash_PreviewID = "#divDashboardPreview";

    var dash_samples = <% = If(ViewOnly, "[]", ReportWebAPI.ReportsListJSON(GetReportSamples(), ecReportCategory.Dashboard, True)) %>;

    var data_charts = {"data": null, "loading": false, wrt: "<% =PM.Parameters.Synthesis_WRTNodeID %>"*1 };
    var data_sa = {"data": null, "loading": false};
    var data_ra = {"data": null, "loading": false};
    var data_users = {"data": null, "loading": false};
    var users_grp_total_cnt = <% =PM.UsersList.Count + PM.CombinedGroups.GroupsList.Count %>;

            // Normalization Modes
    var ntNormalizedForAll = <%=LocalNormalizationType.ntNormalizedForAll %>;
    var ntNormalizedMul100 = <%=LocalNormalizationType.ntNormalizedMul100 %>;
    var ntNormalizedSum100 = <%=LocalNormalizationType.ntNormalizedSum100 %>;
    var ntUnnormalized     = <%=LocalNormalizationType.ntUnnormalized     %>;

    //var decimals_options = [0,1,2,3,4,5]; // # #.#, #.##, #.###, #.####, #.#####
    const decimals_options = [{"text": "0", value: 0}, {"text": "0.0", value: 1}, {"text": "0.00", value: 2}, {"text": "0.000", value: 3}, {"text": "0.0000", value: 4}, {"text": "0.00000", value: 5}]; // # #.#, #.##, #.###, #.####, #.#####
    const synth_options = [{"ID": <%=CInt(ECSynthesisMode.smIdeal)%>, "Text": "Ideal"}, {"ID": <%=CInt(ECSynthesisMode.smDistributive)%>, "Text": "Distributive"}];
    const combined_options = [{"ID": cmAIJ, "Text": "AIJ"},  {"ID": cmAIP, "Text": "AIP"}];
    var option_decimals = <% = CheckVar("decimals", PM.Parameters.DecimalDigits) %>;
    var option_useCIS = <%= Checkvar("usecis", CInt(App.ActiveProject.ProjectManager.PipeParameters.UseCISForIndividuals)) %>;
    var option_combinedMode = <%= CheckVar("combinedmode", CInt(App.ActiveProject.ProjectManager.PipeParameters.CombinedMode)) %>;
    var option_synthMode = <%= CheckVar("synthmode", CInt(App.ActiveProject.ProjectManager.CalculationsManager.SynthesisMode)) %>;
    var option_alts_filtered = (localStorage.getItem(_sess_dash_alts_flt) == "true");
    var option_alts_filter = "<% =AlternativesFilterValue %>";
    var option_normalization = <% =CInt(App.ActiveProject.ProjectManager.Parameters.Normalization)%>;
    var option_sa_sorting = <% =CInt(App.ActiveProject.ProjectManager.Parameters.SensitivitySorting)%>;
    var option_ra_scenario_id = <% =CInt(App.ActiveProject.ProjectManager.Parameters.RAActiveScenarioID)%>;
    var option_ra_scenarios = <% = RAWebAPI.getScenariosAsJSON(App.ActiveProject.ProjectManager.ResourceAligner) %>;
    var alt_filter_modes = {
        "-1": "Show all alternatives",
        "5": "Show top 5 Alternatives",
        "10": "Show top 10 Alternatives",
        "25": "Show top 25 Alternatives",
        "-2": "Show top X",
        "-105": "Show bottom 5 Alternatives",
        "-110": "Show bottom 10 Alternatives",
        "-125": "Show bottom 25 Alternatives",
        "-4": "Show funded in current scenario",
        "-3": "Select/deselect alternatives",
        "-5": "Filter by alternative attribute",
    };
    var norm_options = [{"ID": ntUnnormalized, "Text": "Unnormalized", disabled: (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%>)}, {"ID": ntNormalizedForAll, "Text": "Normalized"}, {"ID": ntNormalizedMul100, "Text": "% of Maximum"}];

    var _opt_load_native_pages = false;
    var _opt_show_global_option = false;
    var _opt_wrt_sync_objectives_panel = false;

    var _is_printing = false;

    var lblAlternatives = "<%=ParseString("%%Alternatives%%")%>";
    var lblObjectives = "<%=ParseString("%%Objectives%%")%>";

    // ========================= General methods and functions ======================================

    function hasData() {
        return ((data_charts) && typeof data_charts.data != "undefined" && data_charts.data != null && typeof data_charts.data.objs != "undefined");
    }

    function hasRAData() {
        return ((data_ra) && typeof data_ra.data != "undefined" && data_ra.data != null);
    }

    function hasUsersData() {
        return ((data_users) && typeof data_users.data != "undefined" && data_users.data != null);
    }

    function resetData() {
        data_charts["data"] = null;
        data_charts["loading"] = false;
        data_sa["data"] = null;
        data_sa["loading"] = false;
        //data_ra["data"] = null;
        //data_ra["loading"] = false;
    }

    function wrtNodeID() {
        return (typeof data_charts.wrt == "undefined" ?  (hasData() ? data_charts.data.objs[0].id : -1) : data_charts.wrt);
    }

    function updateURL(onload) {
        url = "<% = JS_SafeString(PageURL(CurrentPageID)) %>";
        var title = dashboard_pg_title;
        if (dashboardID>0) { 
            url += "?dashboard_id=" + dashboardID;
            var d = currentDashboard();
            if ((d)) title = ShortString(d.Name, 30, false) + " – " + title;
        }
        document.title = title;
        if ((history.replaceState)) {
            history.replaceState({}, document.title, url);  //pushState
        }
    }

    function setDashboardID(id, do_update) {
        if (id<=0 || typeof dashboardsList[id] == "undefined") { 
            id = (Object.keys(dashboardsList).length>0 ? Object.keys(dashboardsList)[0]*1 : -1);
        }

        if (dashboardID != id) {
            setCurDashboardID(id*1);
            updateURL(false);
            if (do_update) {
            } else {
                var cb = $("#cbDashboardID");
                if ((cb) && (cb.length) && (cb.data("dxSelectBox"))) cb.dxSelectBox("option", "value", dashboardID);
            }
            initToolbar();
            if (id>0) {
                var d = currentDashboard();
                _dash_cellsPerRow = 1*((d) ? d.Options.cellsPerRow : _dash_def_cols);
            }
            showDashboard();
            if ($(_dash_ElementID).data("dxScrollView")) $(_dash_ElementID).dxScrollView("instance").scrollTo(0);
            resizeAll(true);
        }
    }

    function onLoadDashboards() {
        var id = dashboardID;
        dashboardID = -2;   // for init even when no dashboards;
        setDashboardID(id, true);
    }

    function reloadAll() {
        showLoadingPanel();
        setTimeout(function () {
            var id = dashboardID;
            dashboardID = -1;
            setDashboardID(id, true);
        }, 30);
    }

    function reloadAllWithData(only_with_prty) {
        if (hasData()) {
            showLoadingPanel();
            resetData();
            data_ra["data"] = null;
            data_ra["loading"] = false;
            data_users["data"] = null;
            data_users["loading"] = false;
            resetChartWidgets(only_with_prty);
            resetSAWidgets();
            //resetGridWidgets();
            var items = dashboardItems();
            chartLoadData(items);
            SALoadData(items);
        }
    }

    function getItemName(item) {
        var func_name = "get" + item.ItemType + "Name";
        if (typeof window[func_name] == "function") {
            return window[func_name](item);
        }
        return "";
    }

    function getElement(item) {
        return $("#" + getItemName(item));
    }

    function resetElement(item) {
        var res = null;
        var has_tb = isItemMenuOpened(item.ID);
        var el = getElement(item);
        if ((el) && (el.length))
        {
            var func_reset = "reset" + item.ItemType;
            if (typeof window[func_reset] == "function") {
                res = window[func_reset](item);
                el.addClass("preloader_dots").html("").removeClass("widget-created");
            }
        }
        //var elem = null;
        //if (typeof item.ID != "undefined") elem = $("#tileItem" + item.ID);
        //if ((elem) && (elem.length)) {
        //    var el = getItemElement(item);
        //    elem.html(el);
        //    setTimeout(function () { 
        //        initItemElement(elem, item);
        //        if (has_tb) onItemMenu(item.ID);
        //    }, 100);
        //}
        return res;
    }

    function isAIPmode() {
        return (option_combinedMode == cmAIP);
    }

    function isTerminalNode(oid) {
        if (hasData() && oid>=0) {
            var node = getObjectiveByID(oid);
            return ((node) && (node.isTerminal));
        }
        return false;
    }

    function isItemEnabled(item) {
        var res = true;
        if ((item) && typeof item.ItemType != "undefined") {
            if (typeof item.Disabled != "undefined" && (item.Disabled)) res= false;
            if ((res) && (isAIPmode() || isTerminalNode(wrtNodeID()))) {
                switch (item.ItemType) {
                    case _rep_item_DSA:
                    case _rep_item_PSA:
                    case _rep_item_GSA:
                    case _rep_item_Analysis2D:
                    case _rep_item_HTH:
                    case _rep_item_ASA:
                    case _rep_item_ObjectivesChart:
                    case _rep_item_ObjsGrid:
                        res = false;
                }
            }
        }
        return res;
    }

    function resetElementsByType(type) {
        var items = dashboardItemsByType(type);
        for (var i=0; i<items.length; i++) {
            resetElement(items[i]);
        }
    }

    function resetAll() {
        var items = dashboardItems();
        for (var i=0; i<items.length; i++) {
            resetElement(items[i]);
        }
    }

    function updateItemOption(item, prop_name, options) {
        if ((item) && typeof item.ID != "undefined") {
            var func_reset = "update" + item.ItemType + "Option";
            if (typeof window[func_reset] == "function") {
                return window[func_reset](item, prop_name, options);
            }
        }
        return null
    }

    function getItemNormalization(item, val) {
        var res = val;
        if (typeof item != "undefined" && typeof item.ItemType != "undefined") {
            switch (item.ItemType) {
                case _rep_item_DSA:
                case _rep_item_PSA:
                case _rep_item_GSA:
                case _rep_item_Analysis2D:
                case _rep_item_HTH:
                case _rep_item_ASA:
                    switch (val) {
                        case ntUnnormalized:
                            res = "unnormalized"
                            break;
                        case ntNormalizedMul100:
                            res ="normMax100";
                            break;
                        default:
                            res = "normAll";
                    }
                    break;
                case _rep_item_AlternativesChart:
                    switch (val) {
                        case ntUnnormalized:
                            res = "none"
                            break;
                        case ntNormalizedMul100:
                            res ="norm100";
                            break;
                        default:
                            res = "normAll";
                    }
                    break;
                case _rep_item_AltsGrid:
                    break;
            }
        }
        return res;
    }

    function setItemNormalization(item, val, refresh) {
        if (typeof item != "undefined" && typeof item.ItemType != "undefined") {
            var opt_name = "normalizationMode";
            if (opt_name!="") {
                var norm_val = getItemNormalization(item, val);
                item.ContentOptions[opt_name] = norm_val;
                if (refresh) {
                    updateItemOption(item, opt_name, item.ContentOptions[opt_name]);
                    <% If Not ViewOnly Then %>updateDashboardItem(item, function () {});<% End If %>
                }
            }
        }
    }

    function setPipeOption(is_pipeparams, name, value, on_saved) {
        <%--<% If Not ViewOnly Then %>callAPI("pm/params/?action=" + (is_pipeparams ? "set_pipe_option" : "set_param_option"), {"name": name, "value": value}, on_saved, typeof on_saved != "function");<% Else %>if (typeof on_saved == "function") on_saved();<% End if %>--%>
        callAPI("pm/params/?action=" + (is_pipeparams ? "set_pipe_option" : "set_param_option"), {"name": name, "value": value}, on_saved, typeof on_saved != "function");
    }

<% If Not ViewOnly Then %>
    // =============================== Dashboard actions ===========================================

    function onEditDashboard(id, changed_options) {
        setToolbarStatus(true);
        if (typeof id != "undefined") {
            if (id == dashboardID) {
                showDashboardTitle();
                initToolbar();
                if (changed_options) updateTileItems();
            } else {
                setDashboardID(id, true);
            }
        }
    }

    function onCloneDashboard(id) {
        setToolbarStatus(true);
        if (typeof id != "undefined") {
            if (id != dashboardID) {
                setDashboardID(id, true);
            }
        }
    }

    function onDeleteDashboard(id) {
        setToolbarStatus(true);
        setDashboardID(-1, true);
    }

    function uploadDashboard() {
        uploadDialog("Upload dashboard", true, _API_URL + _API_REPORTS + "?<% =_PARAM_ACTION %>=upload&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>", onUploaded, function () { setToolbarStatus(false); }, false, [".json", ".txt", ".dash", ".rep"], null);
    }

    function onUploaded(res) {
        setToolbarStatus(true);
        if (isValidReply(res)) {
            if (res.Result == _res_Success && typeof res.Data != "undefined" && res.Data != null && typeof res.ObjectID != "undefined" && res.ObjectID>0) {
                dashboardID = res.ObjectID;
                onDashboardsLoaded(res, onLoadDashboards);
            } else {
                showResMessage(res, true);
            }
        }
    }

    function addFromSamples() {
        if (typeof dash_samples != "undefined" && dash_samples != null) {

            var samples = scanReports(dash_samples, _rep_type_dash);
            if ((samples) && (samples.length)) {

                var sample_id = -1;

                var dlg = $("#divSamples");
                if ((dlg) && (dlg.length)) {
                    if (dlg.data("dxPopup") && dlg.is(":visible")) {
                        dlg.dxPopup("hide");
                        dlg.hide();
                    } else {

                        var max_h = dlgMaxHeight(1200) - 60;
                        var h = (samples.length*96 + 120);
                        if (h>max_h) h = max_h;
                        var w = dlgMaxWidth(600);

                        var div_lst = $("<div id='divSamplesList' class='dashbrdLayout' style='height:calc(100% - 24px);'></div>");

                        div_lst.dxTileView({
                            items: samples,
                            activeStateEnabled: false,
                            baseItemWidth: w-32,
                            baseItemHeight: 94,
                            itemMargin: 3,
                            //height: dlgMaxHeight(425)-210,
                            direction: "vertical",
                            showScrollbar: true,
                            itemTemplate: function (itemData, itemIndex, element) {
                                var lst = "";
                                var c = 0;
                                var c_max = 7;
                                for (let id in itemData.Items) {
                                    lst += (lst=="" ? "" : ", ") + itemData.Items[id].Name;
                                    c++;
                                    if (c>c_max && Object.keys(itemData.Items).length>c_max+1) {
                                        lst+=", and " + (Object.keys(itemData.Items).length-c_max) + " panels more";
                                        break;
                                    }
                                }
                                var tpl = $("<div id='divSamplePreview" + itemData.ID + "' style='border:0px solid #f0f0f0; width:86px; height:86px; float:left; margin:4px 1ex 4px 4px;'></div><h6 style='text-align:left'>" + itemData.Name + "</h6>" + (typeof itemData.Comment != "undefined" && itemData.Comment!="" ? "<div style='width:98%; text-overflow: ellipsis; white-space: normal;'>" + itemData.Comment + "</div>" : "") + (typeof itemData.Items != "undefined" ? "<div class='text small' style='margin-top:4px; width:99%; white-space: normal; text-oveflow:ellipsis;'>" + lst + "</div>" : ""));
                                element.append(tpl);
                                element.dblclick(onAddSample);
                            },
                            onItemClick: function (e) {
                                if (typeof e.itemData.ID != "undefined") {
                                    sample_id = e.itemData.ID;
                                    var btn = $("#btnAddDash");
                                    if ((btn) && (btn.data("dxButton"))) btn.dxButton("option", {disabled: sample_id<0});
                                    btn = $("#btnAdd2CurDash");
                                    if ((btn) && (btn.data("dxButton"))) btn.dxButton("option", {disabled: sample_id<0});
                                }
                                e.event.preventDefault();
                                e.event.stopImmediatePropagation();
                                // please note: this event called even if you click over the any element inside the cell!
                            },
                        });

                        function onAddSample() {
                            if (sample_id>0) {
                                dlg.dxPopup("hide");
                                dlg.hide();
                                showLoadingPanel();
                                callAPI(_API_REPORTS + "?<% =_PARAM_ACTION %>=add_sample&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>", {id: sample_id}, onUploaded);
                            }
                        };

                        dlg.show();
                        dlg.dxPopup({
                            contentTemplate: function(elem) {
                                //$("<div style='margin-bottom:6px; margin-left:1ex;'><i class='as_icon fa fa-database'></i> <b>You can select the dashboard for import into this model</b>:</div>").appendTo(elem);
                                div_lst.appendTo(elem);
                            },
                            minHeight: 120,
                            minWidth: 450,
                            height: h,
                            width: w,
                            maxHeight: max_h,
                            maxWidth: dlgMaxWidth(1200),
                            showTitle: true,
                            title: "Samples list",
                            dragEnabled: true,
                            shading: true,
                            closeOnOutsideClick: true,
                            resizeEnabled: true,
                            showCloseButton: true,
                            toolbarItems: [{
                                widget: 'dxButton',
                                options: {
                                    text: "Create new dashboard",
                                    "type": "default",
                                    elementAttr: { "class": "button_enter", "id": "btnAddDash"},
                                    disabled: true,
                                    onClick: function () {
                                        onAddSample();
                                        return false;
                                    }
                                },
                                toolbar: "bottom",
                                location: 'after'
                            }, {
                                widget: 'dxButton',
                                options: {
                                    text: "Add to current dashboard",
                                    elementAttr: { "id": "btnAdd2CurDash"},
                                    disabled: true,
                                    visible: (dashboardID>0 && dashboardsArray().length),
                                    onClick: function () {
                                        if (sample_id>0) {
                                            dlg.dxPopup("hide");
                                            dlg.hide();
                                            showLoadingPanel();
                                            callAPI(_API_REPORTS + "?<% =_PARAM_ACTION %>=add_sample&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>&dest=" + dashboardID, {id: sample_id}, onUploaded);
                                        }
                                        return false;
                                    }
                                },
                                toolbar: "bottom",
                                location: 'after'
                            }, {
                                widget: 'dxButton',
                                options: {
                                    text: resString("btnCancel"),
                                    elementAttr: { "class": "button_cancel"},
                                    onClick: function () {
                                        dlg.dxPopup("hide");
                                        dlg.hide();
                                        return false;
                                    }
                                },
                                toolbar: "bottom",
                                location: 'after'
                            }],
                            onResizeEnd: function (e) {
                                var w = e.component.option("width");
                                var mw = e.component.option("minWidth");
                                if (typeof mw != "undefined" && mw>100 && w<mw) w = mw;
                                div_lst.dxTileView({baseItemWidth: w-32});
                            }
                        });
                        dlg.dxPopup("show");
                        setTimeout(function () { initSamplePreviewItems(samples); }, 100);
                    }
                }
            } else {
                showErrorMessage("No samples found.", true);
            }
        }
    }

    function initSamplePreviewItems(samples) {
        var grid_opt = {
            mode: "view",
            cellMargin: 1,
            margin: 2,
            width: 84,
            height: 84,
            itemColor: "#a5d490",
            backgroundColor: "#ffffff",
            cellColor: "#e0e0e0",
            showSizes: false,
        };
        for (var i=0; i<samples.length; i++) {
            var d = samples[i];
            var p = $("#divSamplePreview" + d.ID);
            if ((p) && (p.length)) {
                var Items = samples[i].Items;
                var panels = [];
                for (let id in Items) {
                    var opt = Items[id].ItemOptions;
                    panels.push([opt.cols, opt.rows]);
                }
                var rows = Math.ceil(_dash_def_aspect * d.Options.cellsPerRow);
                if (rows > d.Options.cellsPerRow) rows = d.Options.cellsPerRow;
                grid_opt.items = panels;
                grid_opt.cols = d.Options.cellsPerRow;
                grid_opt.rows = d.Options.cellsPerRow;
                grid_opt.visibleRows = rows;
                p.ecDashboardGrid(grid_opt);
            }
        }
    }
<% End If %>
    // =========================== Dashboard items actions ==========================================
<% If Not ViewOnly Then %>
    function onEditDashboardItem(id) {
        updateTileItems();
        var d = currentDashboard();
        if ((d) && Object.keys(d.Items).length == 0) resizeAll(false);
        if ((d) && (typeof d.Items[id] != "undefined") && $(_dash_ElementID).data("dxScrollView")) {
            setTimeout(function () {
                $(_dash_ElementID).dxScrollView("instance").scrollToElement($("#itemCell" + id));
            }, 100);
        }
        if ((d) && (typeof d.Items[id] != "undefined")) {
            var icon = $("#dashComment" + id);
            var icon_exp = $(_dash_PreviewID).find("#dashComment" + id);
            if (!(icon_exp) || !icon_exp.length) icon_exp =null;
            if ((icon) && (icon.length)) {
                if (typeof d.Items[id].Comment != "undefined" && d.Items[id].Comment != "") {
                    icon.removeClass("gray").addClass("blue");
                    if (icon_exp) icon_exp.removeClass("gray").addClass("blue");
                } else {
                    icon.removeClass("blue").addClass("gray");
                    if (icon_exp) icon_exp.removeClass("blue").addClass("gray");
                }
                icon.attr("title", replaceString("\n", "<br>", htmlEscape(d.Items[id].Comment)));
                if (icon_exp) icon_exp.attr("title", icon.attr("title"));
            }
            var item = d.Items[id];
            switch (item.ItemType) {
                case _rep_item_DSA:
                case _rep_item_PSA:
                case _rep_item_GSA:
                case _rep_item_Analysis2D:
                case _rep_item_HTH:
                case _rep_item_AlternativesChart:
                case _rep_item_AltsGrid:
                    setItemNormalization(item, option_normalization, true);
                    break;
                case _rep_item_ASA:
                    if (typeof data_sa.norm == "undefined" || data_sa.norm != option_normalization) {
                        resetElement(item);
                        SALoadData();
                    } else {
                        setItemNormalization(item, option_normalization, true);
                    }
                case _rep_item_PortfolioView:
                    if (!hasRAData()) {
                        resetElement(item);
                        raLoadData(currentDashboard().Items);
                    }
            }
        }
    }

    function onDeleteDashboardItem(id) {
        var isExpanded = $("#itemCell" + id).hasClass("dashbrdExpanded");
        if (isExpanded) {
            onItemToggle(id);
        }
        updateTileItems();
    }

    function updateDashboardItem(item, callback) {
        doDashboardItemEdit("edit_item", item, callback, "Save data...");
    }
<% End If %>
    function applyAltsFiltering(flt_mode) {
        var items = dashboardItems();
        for (var k=0; k<items.length; k++) {
            var item = items[k];
            if (typeof item.ItemType != "undefined") {
                var obj = getElement(item);
                if ((obj) && (obj.length)) {
                    switch (item.ItemType) {
                        case _rep_item_DSA:
                        case _rep_item_PSA:
                        case _rep_item_GSA:
                        case _rep_item_Analysis2D:
                        case _rep_item_HTH:
                            if (obj.hasClass("widget-created")) {
                                var ds = obj.sa("option", "alts");
                                for (var i = 0; i < ds.length; i++) {
                                    ds[i].visible = (flt_mode == -1 ? 1 : ds[i].filter);
                                };
                                obj.sa("option", "alts", ds);
                            }
                            updateSensitivityOption(item, "altFilterValue", flt_mode);
                            break;                        
                        case _rep_item_ASA:
                            if (obj.hasClass("widget-created")) {
                                var ds = obj.sa("option", "ASAdata");
                                for (var i = 0; i < ds.alternatives.length; i++) {
                                    ds.alternatives[i].visible = (flt_mode == -1 ? 1 : ds.alternatives[i].filter);
                                };
                                obj.sa("option", "altFilterValue", flt_mode);
                            }
                            updateSensitivityOption(item, "ASAdata", ds);
                            break;
                        case _rep_item_AlternativesChart:
                            if (obj.hasClass("widget-created")) {
                                var ds = obj.ecChart("option", "dataSource");
                                for (var i = 0; i < ds.alts.length; i++) {
                                    ds.alts[i].visible = (flt_mode == -1 ? 1 : ds.alts[i].filter);
                                };
                                obj.ecChart("option", "dataSource", ds).ecChart("redraw");
                            }
                            break;
                        case _rep_item_Alternatives:
                            //if (obj.data("dxList")) {
                            //    var ds = obj.dxList("option", "dataSource");
                            //    for (var i = 0; i < ds.length; i++) {
                            //        ds[i].visible = (flt_mode == -1 ? 1 : ds[i].filter);
                            //    };
                            //    obj.dxList({"dataSource" : ds});
                            //}
                            //break;
                        case _rep_item_AltsGrid:
                            if (option_alts_filtered && obj.data("dxDataGrid")) obj.dxDataGrid("instance").filter(['filter', '=', 1]); else obj.dxDataGrid("instance").clearFilter();
                            break;
                    }
                }
            }
        }
    }

    // ============================ Dashboard items objects/rendering =================================

    function isPageAvailable(pgid) {
        if (typeof nav_json != "undefined") {
            var pg = pageByID(nav_json, pgid);
            if ((pg) && (typeof pg.url != "undefined" && pg.url != "") && (typeof pg.disabled == "undefined" || !(pg.disabled))) return true;
        }
        return false;s
    }

    function getItemContent(item) {
        var content = "";
        if ((item) && typeof item.ItemType != "undefined" && item.ItemType!="") {
            var func_name = "get" + item.ItemType;
            if (typeof window[func_name] == "function") {
                content = window[func_name](item);
            }
            if (_opt_load_native_pages && content == "" && typeof item.EditURL != "undefined" && item.EditURL !="") {  // load as native page when no routine for render
                content = getPage(item);
            }
        }
        return content;
    }

    function getItemElement(item) {
        if ((item)) {
            var id = item.ID;
            var type = (typeof item.ItemType != "undefined" ? item.ItemType : "");
            var d = currentDashboard();
            var canDrag = ((d) && typeof d.Items != "undefined" && Object.keys(d.Items).length>1) && !viewOnly;
            var itm_class = "dashbrdItem";
            var is_new = (type == _rep_item_Space || type == _rep_item_New);
            var is_enabled = isItemEnabled(item);
            var itm_st = "";
            if (is_enabled && typeof item.ItemOptions.bgColor != "undefined" && item.ItemOptions.bgColor != "") itm_st += "background:" + item.ItemOptions.bgColor + ";";
            var itm_idx = (typeof item.Index != "undefined" ? "<div class='dashbrdIndex' title='Index'>" + item.Index + "</div>" : "");
            var itm_mnu = "";
            if (type != _rep_item_New) itm_mnu += "<div class='" + (type == _rep_item_Space ? "dashbrdWindow" : "dashbrdMenu") + "' id='dashbrdMenu" + id + "' title='Toggle panel menu'><a href='' onclick='onItemMenuToggle(" + id + ", true); return false;'><i class='fas fa-cog'></i></a></div>";
            var itm_content = "";
            if (is_enabled) {
                itm_content = getItemContent(item);
                if (itm_content == "") {
                    var msg = "";
                    if (typeof item.ItemType == "undefined" || (_rep_dash_allowed.indexOf(item.ItemType)<0)) {
                        msg = "<span title='" + item.ItemType + "'><% =JS_SafeString(ResString("lblDashboardNotSupported")) %></span>";
                        itm_class += " disabled";
                    }
                    itm_content = getNoContent(msg);
                 }

            } else {
                itm_content  = "<i class='far fa-times-circle fa-2x'></i><div style='margin-top:1ex; color:#000;' title='Not applicable'>Not applicable<br>" + (isAIPmode() ? "when AIP is enabled" : "when WRT node <nobr>is covering objective</nobr>") + "</div>";
                itm_class += " disabled";
            }
            var show_title = (typeof item.ItemOptions.showTitle == "undefined" || item.ItemOptions.showTitle);
            if (is_new) {
                show_title = false;
                if (type != _rep_item_Space) {
                    if (canDrag || (!viewOnly && !_dash_placeholder_when_empty)) itm_mnu = "<div class='dashbrdWindow' style='margin:6px'><a href='' onclick='deleteDashboardItem(" + id + ", onDeleteDashboardItem); return false;'><i class='fa fa-trash-alt' title='Delete'></i></a></div>" + itm_mnu;
                } else {
                    //setTimeout(function () {
                    //    onItemMenuToggle(id, true);
                    //}, 30);
                }
            } else {
                itm_mnu = "<div class='dashbrdWindow'><a href='' onclick='onItemToggle(" + id + "); return false;'><i class='fa fa-expand' id='iconExpand" + id + "' title='Expand'></i></a></div>" + itm_mnu;
            }
            var itm_hdr = "";
            if (show_title) {
                var desc = "";
                if (!is_new) {
                    desc = "<i class='fas fa-info-circle dashbrd_comment_icon " + (typeof item.Comment != "undefined" && item.Comment != "" ? "blue" : "gray") + "' id='dashComment" + id + "' title='" + replaceString("\n", "<br>", htmlEscape(htmlEscape(item.Comment))) + "'></i>";
                }
                itm_hdr = "<tr><td id='itemHeader" + id + "' valign=middle style='height:1em'><div class='dashbrdTitle" + (canDrag ? " dashbrdDraggable" : "") + "' id='itemTitle" + id + "'>" + desc + "<span title='" +  htmlEscape(item.Name) + "'>" + htmlEscape(item.Name) + "</span></div></td></tr>";
            } else {
                if (type!=_rep_item_New) itm_content = "<div class='dashbrdDraggable dashbrdEmptyHdr'><i class='fa fa-arrows-alt'></i></div>" + itm_content;
            }
            var el = $("<div class='" + itm_class + "' style='" + itm_st + "' id='itemCell" + id + "'>" + itm_idx + itm_mnu + "<table border=0 cellspacing=0 cellpadding=0 class='whole'>" + itm_hdr + "<tr id='trDashbrdToolbar" + id + "' style='height:1px'><td><div id='dashbrdToolbar" + id + "' data-item-id='" + id + "' style='display:none; border-bottom:1px solid #e0e0e0; padding:"+ (type == _rep_item_Space ? "2px 24px 2px 2px" : (is_new ? "2px" : (show_title ? "2px 4px" : "2px 48px 2px 4px"))) + "; margin:" + (show_title ? "0px -4px 2px -4px" : "-4px -4px 2px -4px") + "; width: calc(100% + 6px); z-index:49;'></div></td></tr><tr><td valign='middle' align='center'><div class='dashbrdScrollable'><table border=0 cellspacing=0 cellpadding=0 class='whole'><tr><td align=center valign=middle id='itemContent" + id + "'class='dashbrdContent' data-item-id='" + id + "'>" + itm_content + "</td></tr></table></div></td></tr></table></div>");
            return el;
        } else 
        {
            return "";
        }
    }

    function prepareTileItems(items) {
        var d = currentDashboard();
        _opt_load_native_pages = ((d) && typeof d.Options != "undefined" && typeof d.Options.loadNativePages != "undefined" ? ((d.Options.loadNativePages)) : _dash_def_loadpg);
        if (!(items) || !items.length) { 
            if (_dash_placeholder_when_empty) items.push(_dash_def_item_new);
        }
        var viewmode = _dash_viewmode;
        if (!_dash_viewmode_allowed) viewmode = 1;
        lst = dashboardsItemsSort(items);
        var cols = _dash_cellsPerRow;    // 1*((d) ? d.Options.cellsPerRow : _dash_def_cols);
        var rows = _dash_cellsPerCol;
        var items = [];
        for (var i = 0; i < lst.length; i++) {
            var el = lst[i];
            var item = { "data": el };
            if (typeof el.Name != "undefined") item.text = el.Name;
            if (typeof el.ItemOptions != "undefined") {
                if (typeof el.ItemOptions.cols != "undefined") item.widthRatio = (viewmode ? cols : 1*(el.ItemOptions.cols < 1 ? cols : (el.ItemOptions.cols > cols ? cols : el.ItemOptions.cols)));
                if (typeof el.ItemOptions.rows != "undefined") item.heightRatio = (viewmode ? rows : 1*(el.ItemOptions.rows < 1 ? 1 : el.ItemOptions.rows));
            } else {
                item.widthRatio = 1;
                item.heightRatio = 1;
            }
            var type = (typeof item.ItemType != "undefined" ? item.ItemType : "");
            <% If ViewOnly Then %>if (type != _dash_def_item_new) <% End If %>items.push(item);
        }
        return items;
    }

    function showDashboardTitle() {
        var data = currentDashboard();
        $(".dashboardComment").hide();
        if ((data)) {
            $(".dashboardTitle").show();
            //var title = "<i class='fas fa-info-circle dashbrd_comment_icon " + (typeof data.Comment != "undefined" && data.Comment != "" ? "blue" : "gray") + "' id='dashboardComment' title='" + htmlEscape(data.Comment) + "'></i><a href='' onclick='editDashboard(dashboardID, onEditDashboard); return false;'>" + htmlEscape(data.Name) + "</a>";
            var title = <% If Not ViewOnly Then %>"<a href='' onclick='editDashboard(dashboardID, onEditDashboard); return false;'>" + htmlEscape(data.Name) + "</a>";<% Else %>htmlEscape(data.Name);<% End if %>
            $("#dashboardTitle").html(title);
            if (typeof data.Comment != "undefined" && data.Comment != "") {
                $(".dashboardComment").show().html(replaceString("\n", "<br>", htmlEscape(data.Comment)));
            };
            if (typeof data.Options != "undefined") {
                if (typeof data.Options.showBorders == "undefined" || data.Options.showBorders) {
                    $("#divMainContent").removeClass(noborders_class);
                    $(".ecDashboard").removeClass(noborders_class);
                } else {
                    $("#divMainContent").addClass(noborders_class);
                    $(".ecDashboard").addClass(noborders_class);
                }
            }
        } else {
            $(".dashboardTitle").hide();
        }
    }

    function updateTileItems(items) {
        var lst = (items) ? items : dashboardItems();
        if (lst.length>1 && (dashboardTiles)) {
            showLoadingPanel();
            dashboardTiles.option("items", prepareTileItems(lst));
        } else {
            showDashboard();
        }
        initToolbar();
    }

    function initItemElement(element, data) {
        if ((element)) {
            element.find(".dashbrdScrollable").dxScrollView({
                //showScrollbar: "always",
                direction: "both",
            });
            element.find(".dashbrdTitle").on("dxcontextmenu", function (event) {
                if (typeof data != "undefined") onItemMenu(data.ID);
                event.preventDefault();
                return false;
            })<% If Not ViewOnly Then %>.on("dblclick", function (event) {
                if (typeof data != "undefined") editDashboardItem(data.ID, onEditDashboardItem);
            });<% End If %>
        }
    }

    var drag_over = null;
    function showDashboard() {
        showLoadingPanel();
        showDashboardTitle();
        var data = currentDashboard();
        if ((data)) {
            var isCreating = true;
            var items = dashboardItems();
            items = prepareTileItems(items);
            $("#divNoDashboards").hide();
            if (items.length) {
                $("#divNoDashboardItems").hide();
                var offset = 0;
                if ($(_dash_ElementID).data("dxScrollView")) offset = $(_dash_ElementID).dxScrollView("instance").scrollHeight();
                dashboardTiles = $(_dash_ElementID).show().dxTileView({
                    items: items,
                    activeStateEnabled: false,
                    baseItemWidth: _dash_cellWidth,
                    baseItemHeight: _dash_cellHeight,
                    itemMargin: _dash_cellMargin,
                    direction: "vertical",
                    showScrollbar: true,
                    //itemHoldTimeout: 0,
                    //noDataText: "<div style='margin-top:calc(10%)'>No panels in this dashboard.</div><div style='margin-top:calc(5%)' id='btnAddNewItem'><a href='' onclick='addDashboardItem(-1, onEditDashboardItem); return false;' class='dashed actions'>Add a new panel</a></div>",
                    itemTemplate: function (itemData, itemIndex, itemElement) {
                        itemElement.append(getItemElement(itemData.data));
                        itemElement.prop("id", "tileItem" + itemData.data.ID);
                        itemElement.data("item", itemData.data);
                        if (typeof itemData.data != "undefined" && typeof itemData.data.ItemOptions != "undefined" && typeof itemData.data.ItemType != "undefined") {
                            if (itemData.data.ItemType == _rep_item_Space) itemElement.parent().addClass("empty");
                            if (itemData.data.ItemType == _rep_item_New) itemElement.parent().addClass("new_item");
                        }
                    },
                    onItemClick: function (e) {
                        e.event.preventDefault();
                        e.event.stopImmediatePropagation();
                        // please note: this event called even if you click over the any element inside the cell!
                    },
                    onItemHold: function (e) {
                        if ((e)) {
                            //e.preventDefault(); 
                            //e.event.preventDefault(); 
                            e.event.cancel = 1;
                        }
                    },
                    //onItemContextMenu: function (e) {
                    //    onItemMenu(e.itemData.data.ID);
                    //},
                    onItemRendered: function (e) { 
                        initItemElement(e.itemElement, e.itemData.data);
                        <% If Not ViewOnly Then %>var top = 0;
                        var btm = 9999;
                        e.itemElement.dxDraggable({
                            data: e.itemData,
                            boundary: ".ecDashboard",   //$("#divDashboard").find(".dx-scrollable-content"),
                            handle: ".dashbrdDraggable",
                            group: "dashboard",
                            //autoScroll: true,
                            //scrollSensitivity: 160,
                            //scrollSpeed: 30,
                            onDragStart: function (e) {
                                if (e.itemElement.hasClass(".noDragging")) {
                                    e.cancel = true;
                                } else {
                                    drag_over = null;
                                    e.itemElement.addClass("drag_src");
                                    $(".ecDashboard").addClass("dragging");
                                    top = $(".ecDashboard").offset().top + 32;
                                    btm = top + $(".ecDashboard").height() - _dash_cellHeight;
                                    scroll_offset = 0;
                                    can_scroll = true;
                                }
                            },
                            onDragMove: function (e) {
                                if (e.event.pageY<top && scroll_offset==0) {
                                    can_scroll = true;
                                    scroll_offset = -scroll_dist; 
                                    scrollDashboards(); 
                                } else {
                                    if (e.event.pageY>btm && scroll_offset==0) { 
                                        can_scroll = true;
                                        scroll_offset = scroll_dist; 
                                        scrollDashboards(); 
                                    } else {
                                        scroll_offset = 0;
                                    }
                                }
                                var dest = e.toComponent.element();//.find(".dashbrdItem");
                                if ((dest) && (dest.length)) {
                                    if (drag_over != dest) {
                                        if ((drag_over)) drag_over.removeClass("drag_dest");
                                        if ((e.fromComponent != e.toComponent)) {
                                            drag_over = dest;
                                            drag_over.addClass("drag_dest");
                                        } else {
                                            drag_over = null;
                                        }
                                        if (typeof e.fromData != "undefined" && typeof e.toData != "undefined" && typeof e.toData.data != "undefined" && typeof e.toData.data.Index != "undefined") {
                                            e.itemElement.find(".dashbrdIndex").html(e.fromData.data.Index + ((drag_over) ? " &rarr; " + e.toData.data.Index : ""));
                                        }
                                    }
                                }
                            },
                            onDragEnd: function (e) {
                                drag_dest = null;
                                $(_dash_ElementID).removeClass("dragging");
                                e.itemElement.removeClass("drag_src")
                                var moved = false;
                                if (typeof e.fromData != "undefined" && typeof e.toData != "undefined" && typeof e.toData.data != "undefined" && typeof e.toData.data.Index != "undefined") {
                                    var idx_src = e.fromData.data.Index;
                                    var idx_dst = e.toData.data.Index;
                                    if (idx_src != idx_dst) {
                                        isCreating = true;
                                        var items = dashboardUpdateItemIndex(idx_src, idx_dst, true);
                                        updateTileItems(items);
                                        moved = true;
                                    }
                                }
                                if (!moved) e.cancel = true;
                            },
                        });<% End If %>
                        if (!isCreating && !dashboardTiles_updating) resizeItem(e.itemData.data);
                    },
                    onContentReady: function (e) {
                        hideLoadingPanel();
                        if (!isCreating && !dashboardTiles_updating) resizeAll(false);
                        isCreating = false;
                    },
                    //onOptionChanged: function (e) {
                    //    switch (e.name.toLowerCase()) {
                    //        case "items":
                    //        case "baseitemheight":
                    //            //case "baseitemwidth":
                    //            if (!isCreating && !dashboardTiles_updating) {
                    //                resizeAll(false);
                    //            }
                    //            break;
                    //    }
                    //},
                }).dxTileView("instance");
                if ($(_dash_ElementID).data("dxScrollView")) {
                    $(_dash_ElementID).dxScrollView("instance").option({
                        "scrollByContent": false,
                        "showScrollbar": "always",
                    });
                    if (offset>0) $(_dash_ElementID).dxScrollView("instance").scrollTo(offset);
                }
                $(_dash_ElementID).on("dxclick", function (e) { e.preventDefault(); });
                $(_dash_ElementID).on("dxmousedown", function (e) { e.preventDefault(); });
            } else {
                $(_dash_ElementID).hide();
                $("#divNoDashboardItems").show();
                hideLoadingPanel();
            }
        } else {
            $(_dash_ElementID).hide();
            $("#divNoDashboards").show();
            $("#divNoDashboardItems").hide();
            hideLoadingPanel();
            resizeAll(false);
        }
    }
<% If Not ViewOnly Then %>
    function onItemQuickEdit(id, is_new) {
        if (is_new) {
            addDashboardItem(id, onEditDashboardItem);
        } else {
            editDashboardItem(id, onEditDashboardItem);
        }
    }
<% End If %>
    // =============================== Dashboard toolbar =========================================

    function initToolbar() {
        var dash_lst = dashboardsArray();
        var has_dashboards = (dash_lst.length);

        var items = [{
            location: "before",
            locateInMenu: "never",
            showText: "always",
            cssClass: "nowide1000",
            text: "Dashboard:",
            visible: false,
        }, {
            location: "before",
            locateInMenu: "auto",
            showText: "always",
            widget: "dxSelectBox",
            disabled: !has_dashboards,
            options: {
                items: dash_lst,
                value: dashboardID,
                elementAttr: { id: 'cbDashboardID' },
                displayExpr: "text",
                valueExpr: "id",
                onValueChanged: function (e) {
                    if (dashboardID != e.value * 1) {
                        setDashboardID(e.value * 1, false);
                    }
                }
            }
        }<% If Not ViewOnly Then %>, {
            location: 'before',
            locateInMenu: 'auto',
            showText: "always",
            widget: 'dxDropDownButton',
            options: {
                text: "Create",
                hint: "",
                icon: 'fas fa-plus',
                splitButton: false,
                useSelectMode: false,
                showArrowIcon: true,
                dropDownOptions: {
                    width: 220
                },
                keyExpr: "id",
                displayExpr: "text",
                items: [{
                    "id": "new",
                    "text": "<% =JS_SafeString(ResString("btnDashboardAdd")) %>",
                    "icon": "fas fa-plus-circle",
                }, {
                    "id" : "sample",
                    "text": "<% =JS_SafeString(ResString("btnDashboardFromSample")) %>",
                    "icon": "fas fa-file-invoice",
                    disabled: (dash_samples == null || !(Object.keys(dash_samples).length)),
                }, {
                    "id": "upload",
                    "text": "<% =JS_SafeString(ResString("btnDashboardUpload")) %>",
                    "icon": "fas fa-upload",
                }],
                onItemClick: function(e) {
                    switch (e.itemData.id) {
                        case "new":
                            addDashboard(onEditDashboard);
                            break;
                        case "sample":
                            addFromSamples();
                            break;
                        case "upload":
                            uploadDashboard();
                            break;
                    };
                },
            }
        //}, {
        //    location: 'before',
        //    locateInMenu: 'never',
        //    showText: "never",
        //    text: "",
        }<% End If %>];

<% If Not ViewOnly Then %>if (dashboardID>=0) {
            items.push({
                location: 'before',
                locateInMenu: 'auto',
                showText: "inMenu",
                widget: 'dxButton',
                options: {
                    text: "Properties",
                    hint: "Edit dashboard properties",
                    icon: "fas fa-pencil-alt",
                    onClick: function () {
                        editDashboard(dashboardID, onEditDashboard);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'auto',
                showText: "inMenu",
                widget: 'dxButton',
                options: {
                    text: "Clone",
                    hint: "Clone current dashboard",
                    icon: "far fa-clone",
                    onClick: function () {
                        cloneDashboard(dashboardID, onCloneDashboard);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'auto',
                showText: "inMenu",
                widget: 'dxButton',
                options: {
                    text: "Delete",
                    hint: "Delete current dashboard",
                    icon: "far fa-trash-alt",
                    onClick: function () {
                        deleteDashboard(dashboardID, onDeleteDashboard);
                    }
                }
            }, {
                location: 'before',
                locateInMenu: 'never',
                showText: "never",
                text: "",
            });
        }
<% End If %>
        var lst_items = [];
        if (has_dashboards) {
            var all_items = dashboardItems();
            for (var i=0; i<all_items.length; i++) {
                var it = all_items[i];
                lst_items.push({"id": it.ID, "index": it.Index, "text": "#" + it.Index + " " + (it.ItemType == _rep_item_New ? _rep_item_name[it.ItemType] : it.Name), "icon": (typeof _rep_item_icon[it.ItemType] == "undefined" ? "far fa-square" : _rep_item_icon[it.ItemType]), "type": it.ItemType});
            }
            lst_items.sort( function(a,b) {
                return a.index-b.index;
            });
        }
        
        items.push(<% If Not ViewOnly Then %>{
            location: 'before',
            locateInMenu: 'never',
            showText: "always",
            widget: 'dxButton',
            visible: has_dashboards,
            options: {
                //type: "default",
                //stylingMode: "contained",
                text: "New panel",
                hint: "Add panel to dashboard",
                icon: "fas fa-plus-square",
                elementAttr: { id: "btnAddItem", "class": "dx-button-default-dd" },
                onClick: function () {
                    addDashboardItem(-1, onEditDashboardItem);
                }
            }
        }, <% End If %>{
            location: 'before',
            locateInMenu: 'auto',
            showText: "inMenu",
            widget: 'dxDropDownButton',
            visible: has_dashboards,
            disabled: !(lst_items.length),
            options: {
                text: "Panels…",
                hint: "List of panel with ability to quick jump",
                icon: "fas fa-map-signs",
                elementAttr: { id: "btnJump2Item", "class": "dxDropDownButtonSplit" },
                splitButton: true,
                useSelectMode: false,
                showArrowIcon: true,
                dropDownOptions: {
                    width: 240
                },
                keyExpr: "id",
                displayExpr: "text",
                items: lst_items,
                onItemClick: function(e) {
                    if (typeof e.itemData.id != "undefined") {
                        jumpTo(e.itemData.id);
                    }
                },
                onButtonClick: function (e) {
                    toggleJumpPopup(lst_items);
                }
            }
        }, {
            location: 'before',
            locateInMenu: 'auto',
            showText: "inMenu",
            visible: has_dashboards,
            widget: 'dxButtonGroup',
            disabled: !_dash_viewmode_allowed || !(lst_items.length),
            options: {
                keyExpr: "ID",
                selectedItemKeys: [_dash_viewmode_allowed ? _dash_viewmode : 1],
                displayExpr: "text",
                focusStateEnabled: false,
                elementAttr: {"id": "btnViewMode"},
                onItemClick: function (e) {
                    var sel = e.component.option("selectedItemKeys");
                    _dash_viewmode = ((sel.length>0 && sel[0]!=0) ? 1 : 0);
                    localStorage.setItem(_sess_dash_viewmode, _dash_viewmode);
                    updateTileItems();
                },
                selectionMode: "single",
                items: [{
                    "ID": 0, 
                    //"text": "Regular",
                    "hint": "Regular view",
                    "icon": "fas fa-border-all"
                }, {
                    "ID": 1, 
                    //"text": "Flow",
                    "hint": "Flow view (Fit to page width)",
                    "icon": "far fa-square"
                }]
            }
        <% If ShowDraftPages AndAlso Not ViewOnly Then %>}, {
            location: 'after',
            locateInMenu: 'auto',
            showText: "inMenu",
            widget: 'dxButton',
            visible: has_dashboards,
            options: {
                icon: 'fas fa-code',
                text: "Dashboard JSON",
                hint: "Dashboard JSON data",
                onClick: function () {
                    var code = JSON.stringify(currentDashboard(), null, '  ');
                    code = code.replace(/\"(\w)+\"\:/g, "<span style='color:navy'>$&</span>");
                    showMessagePopup("<pre id='divJSON' style='word-wrap: break-word; font-size:0.78rem; min-width:250px; height:100%;'><b>dashboard</b>: " + code + "</pre>", "Dashboard JSON data (debug, no items)", 900);
                }
            }<% End If%>
        }, {
            location: 'after',
            locateInMenu: 'auto',
            showText: "inMenu",
            widget: 'dxButton',
            visible: has_dashboards && (typeof lst_items != "undefined" && lst_items.length),
            options: {
                text: "Print",
                icon:"fas fa-print",
                onClick: function () {
                    showLoadingPanel();
                    setTimeout(printDashboard, 100);
                }
            }
        }<%--<% If Not ViewOnly Then %>, {
            location: 'after',
            locateInMenu: 'auto',
            showText: "inMenu",
            //widget: 'dxButton',
            visible: has_dashboards,
            widget: 'dxDropDownButton',
            options: {
                text: "Definition",
                hint: "Download Dashboard Definition(s)",
                icon: 'fas fa-download',
                splitButton: false,
                useSelectMode: false,
                showArrowIcon: true,
                dropDownOptions: {
                    width: 260
                },
                keyExpr: "id",
                displayExpr: "text",
                items: [{
                    "id": "current",
                    "text": "Download current dashboard definition",
                    "icon": "fas fa-download",
                }, {
                    "id": "all",
                    "text": "Download all dashboard definitions",
                    "icon": "fas fa-download",
                }, {
                    "id": "upload",
                    "text": "<% =JS_SafeString(ResString("btnDashboardUpload")) %>",
                    "icon": "fas fa-upload",
                }],
                onItemClick: function(e) {
                    switch (e.itemData.id) {
                        case "upload":
                            uploadDashboard();
                            break;
                        case "all":
                            doDownload(_API_URL + _API_REPORTS + "?<% =_PARAM_ACTION %>=download&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>");
                            break;
                        default:
                            doDownload(_API_URL + _API_REPORTS + "?<% =_PARAM_ACTION %>=download&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>&id=" + dashboardID);
                            break;
                    };
                }
            }
        }<% End If %>--%>
        <% If Not ViewOnly Then %>, {
            location: 'after',
            locateInMenu: 'auto',
            showText: "always",
            widget: 'dxButton',
            visible: has_dashboards,
            options: {
                text: "Download current dashboard definition",
                icon: "fas fa-download",
                onClick: function () {
                    doDownload(_API_URL + _API_REPORTS + "?<% =_PARAM_ACTION %>=download&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>&id=" + dashboardID);
                }
            }
        }, {
            location: 'after',
            locateInMenu: 'auto',
            showText: "always",
            widget: 'dxButton',
            visible: has_dashboards,
            options: {
                text: "Download all dashboard definitions",
                icon: "fas fa-download",
                onClick: function () {
                    doDownload(_API_URL + _API_REPORTS + "?<% =_PARAM_ACTION %>=download&category=<% =JS_SafeString(ecReportCategory.Dashboard) %>");
                }
            }
        },  {
            location: 'after',
            locateInMenu: 'auto',
            showText: "always",
            widget: 'dxButton',
            visible: false,
            options: {
                text: "<% =JS_SafeString(ResString("btnDashboardUpload")) %>",
                icon: "fas fa-upload",
                onClick: function () {
                    uploadDashboard();
                }
            }
        }<% End If %>);

        items.push({
            location: 'after',
            locateInMenu: 'auto',
            showText: "inMenu",
            widget: 'dxButton',
            options: {
                text: "Refresh",
                hint: "Refresh data",
                icon: 'fas fa-sync-alt',
                elementAttr: { id: 'btnReloadDashboards' },
                onClick: function () {
                    data_charts.data = null;
                    data_sa.data = null;
                    loadDashboards(onLoadDashboards);
                }
            }
        });

        getToolbarCommonItems(items);

        var jlst = $("#divJumpToList");
        if ((jlst) && (jlst.length) && jlst.is(":visible") && jlst.data("dxList")) {
            jlst.dxList("instance").option({items: lst_items});
            initItemsListPopup(jlst, lst_items)
        }

        $("#toolbar").show().dxToolbar({
            "items": items,
            //disabled: (typeof _ajax_inprogress != "undefined" && (_ajax_inprogress))
        });
    }

    function setToolbarStatus(enabled) {
        var tb =  $("#toolbar");
        if ((enabled)) {
            if ((data_sa.loading) || (data_charts.loading) || (data_ra.loading) || (data_users.loading)) enabled = false;
        }
        if ((tb) && (tb.data("dxToolbar"))) {
            tb.dxToolbar("option", "disabled", !enabled);
        }
    }

    function getToolbarCommonItems(toolbar_items) {
        var _types = {};

        var items = dashboardItems();
        for (var i=0; i<items.length; i++) {
            var item= items[i];
            if (typeof item.ItemType != "undefined") _types[item.ItemType] = true;
        }

        var hasCharts = _types[_rep_item_AlternativesChart] === true || _types[_rep_item_ObjectivesChart] === true;
        var hasSA = _types[_rep_item_DSA] === true || _types[_rep_item_PSA] === true || _types[_rep_item_GSA] === true || _types[_rep_item_Analysis2D] === true || _types[_rep_item_HTH] === true || _types[_rep_item_ASA] === true;
        var hasGrids = _types[_rep_item_ObjsGrid] === true || _types[_rep_item_AltsGrid] === true;  // || _types[_rep_item_Alternatives] === true;
        var hasRA = _types[_rep_item_PortfolioView] === true;

        var has_data = hasData();

        if (hasRA) {
            toolbar_items.push({
                location: "before",
                locateInMenu: "auto",
                showText: "always",
                widget: "dxSelectBox",
                //disabled: false,
                options: {
                    items: option_ra_scenarios,
                    value: option_ra_scenario_id,
                    elementAttr: { id: 'cbRAScenarioID'},
                    //width: 75,
                    hint: "Portfolio View Active Scenario",
                    displayExpr: "Name",
                    valueExpr: "ID",
                    onValueChanged: function (e) {
                        if (option_ra_scenario_id != e.value * 1) {
                            callAPI("pm/ra/?action=set_active_scenario", {id: e.value*1}, onSetRAScenario);
                        }
                    }
                }
            });
        }

        if (hasSA || hasCharts || hasGrids) {
            //toolbar_items.push({
            //    location: 'before',
            //    locateInMenu: 'never',
            //    showText: "never",
            //    cssClass: "nowide1200",
            //    disabled: true,
            //    width: 150,
            //    text: "Data settings:",
            //});

            toolbar_items.push({
                location: 'before',
                locateInMenu: 'auto',
                showText: "always",
                widget: 'dxDropDownBox',
                options: {
                    "hint": "WRT node",
                    elementAttr: { id: 'WRTNodeID' },
                    value: "",
                    valueExpr: "id",
                    displayExpr: "name",
                    dataSource: [],
                    placeholder: "WRT Node...",
                    showClearButton: false,
                    dropDownOptions: {
                        minWidth: dlgMaxWidth(350),
                        maxWidth: dlgMaxWidth(800),
                    },
                    contentTemplate: function(e) {
                        var value = e.component.option("value"),
                            $treeView = $("<div></div>").dxTreeView({
                                dataSource: e.component.getDataSource(),
                                dataStructure: "plain",
                                keyExpr: "id",
                                parentIdExpr: "pid",
                                rootValue: -1,
                                selectionMode: "single",
                                displayExpr: "name",
                                expandAllEnabled: true,
                                focusStateEnabled: false,
                                selectByClick: false,
                                elementAttr: { id: 'tvWRTNodeID', "class": "dx-treeview-compact" },
                                onContentReady: function(args){
                                    updateToolbarWRT(value);
                                },
                                selectNodesRecursive: false,
                                onItemClick: function(args){
                                    var data = args.itemData;
                                    if ((data) && typeof data.id != "undefined") {
                                        if (!tvwrt_manual) {
                                            setWRTNodeID(data.id);
                                            setTimeout( function () { e.component.close(); }, 100);
                                        }
                                    }
                                },
                            });
            
                        treeView = $treeView.dxTreeView("instance");
                        treeView.expandAll();
           
                        return $treeView;
                    },
                    onContentReady: function(e){
                        setTimeout(function() {
                            updateToolbarWRT(wrtNodeID());
                        }, 30);
                    },
                    onOpened: function (e) {
                        updateTreeViewWRT(e.component.option("value"));
                    }
                }
            });

            toolbar_items.push({
                location: "before",
                locateInMenu: "auto",
                showText: "always",
                widget: "dxSelectBox",
                disabled: !has_data,
                options: {
                    items: decimals_options,
                    value: option_decimals,
                    elementAttr: { id: 'cbDecimals'},
                    width: 75,
                    hint: "<% =ResString("lblDecimals") %> ",
                    displayExpr: "text",
                    valueExpr: "value",
                    onValueChanged: function (e) {
                        if (option_decimals != e.value * 1) {
                            option_decimals = e.value * 1;
                            setPipeOption(false, "DecimalDigits", option_decimals, null);
                            var items = dashboardItems();
                            for (var i=0; i<items.length; i++) {
                                var item = items[i];
                                switch (item.ItemType) {
                                    case _rep_item_DSA:
                                    case _rep_item_GSA:
                                    case _rep_item_PSA:
                                    case _rep_item_ASA:
                                    case _rep_item_HTH:
                                    case _rep_item_Analysis2D:
                                        updateSensitivityOption(item, "valDigits", option_decimals);
                                        break;
                                    case _rep_item_AlternativesChart:
                                    case _rep_item_ObjectivesChart:
                                        updateChartOption(item, "decimals", option_decimals);
                                        break;
                                    case _rep_item_ObjsGrid:
                                    case _rep_item_AltsGrid:
                                    case _rep_item_PortfolioView:
                                        resetElement(item);
                                        resizeItem(item);
                                        break;
                                }
                            }
                        }
                    }
                }
            });

            toolbar_items.push({
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxCheckBox',
                disabled: !has_data,
                visible: _opt_show_global_option,
                options: {
                    text: "CIS",
                    showText: true,
                    hint: "Use Combined Input Source mode",
                    value: option_useCIS,
                    elementAttr: {id: 'cbUseCIS'},
                    onValueChanged: function (e) {
                        if (option_useCIS != e.value) {
                            option_useCIS = e.value;
                            setPipeOption(true, "UseCISForIndividuals", option_useCIS, function() {
                                reloadAllWithData(true);
                            });
                        }
                    }
                }
            });

            <%If Not PM.IsRiskProject Then%>
            toolbar_items.push({
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                disabled: !has_data,
                visible: _opt_show_global_option,
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: option_synthMode,
                    width: 100,
                    displayExpr: "Text",
                    elementAttr: {id: 'selSynthesisMode'},
                    hint: "Synthesis Mode",
                    items: synth_options,
                    onSelectionChanged: function (e) { 
                        if (option_synthMode != e.selectedItem.ID) {
                            option_synthMode = e.selectedItem.ID;
                            if (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%> && option_normalization == ntUnnormalized) option_normalization = ntNormalizedForAll;
                            norm_options[0].disabled = (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%>);
                            setPipeOption(true, 'SynthesisMode', option_synthMode, function() {
                                reloadAllWithData(true);
                                initToolbar();
                            });
                        };
                    }
                }
            });
            toolbar_items.push({   
                location: 'before',
                locateInMenu: 'auto',
                widget: 'dxSelectBox',
                showText: true,
                //text: "Combined Mode",
                disabled: !has_data,
                visible: _opt_show_global_option,
                options: {
                    searchEnabled: false,
                    valueExpr: "ID",
                    value: option_combinedMode,
                    width: 60,
                    displayExpr: "Text",
                    elementAttr: {id: 'selAIJAIPMode', class:"on_advanced"},
                    hint: "Combined mode",
                    items: combined_options,
                    onSelectionChanged: function (e) { 
                        if (option_combinedMode != e.selectedItem.ID) {
                            option_combinedMode = e.selectedItem.ID;
                            menu_setOption(menu_option_noAIP, option_combinedMode == cmAIP);
                            setPipeOption(true, 'CombinedMode', option_combinedMode, function() {
                                reloadAll();
                            });
                        };
                    },
                }
            });<% End If%>

            toolbar_items.push({
                location: "before",
                locateInMenu: "auto",
                showText: "always",
                widget: "dxSelectBox",
                disabled: !has_data,
                options: {
                    items: norm_options,
                    value: option_normalization,
                    elementAttr: { id: 'cbNormalization'},
                    //width: 75,
                    hint: "Normalization",
                    displayExpr: "Text",
                    valueExpr: "ID",
                    onValueChanged: function (e) {
                        option_normalization = e.value;
                        setPipeOption(false, "Normalization", option_normalization, null);
                        var items = dashboardItems();
                        var sa_reload = false;
                        for (var i=0; i<items.length; i++) {
                            var item = items[i];
                            setItemNormalization(item, option_normalization, true);
                            if (typeof item.ItemType != "undefined" && isSensitivity(item.ItemType)) {
                                sa_reload = true;
                                resetElement(item);
                            }
                        }
                        if (sa_reload) SALoadData();
                    }
                }
            });

            var flt = getAltsFilteredCount(data_charts);
            toolbar_items.push({
                location: 'before',
                locateInMenu: 'auto',
                showText: "always",
                widget: 'dxButtonGroup',
                visible: (option_alts_filter != -1),
                disabled: (!hasData || flt.total<=0 || flt.total == flt.selected),
                options: {
                    keyExpr: "ID",
                    selectedItemKeys: [option_alts_filtered ? 1 : 0],
                    displayExpr: "text",
                    focusStateEnabled: false,
                    elementAttr: {"id": "btnAltsFilter"},
                    onItemClick: function (e) {
                        option_alts_filtered = (e.component.option("selectedItemKeys").indexOf(1)>=0);
                        localStorage.setItem(_sess_dash_alts_flt, option_alts_filtered);
                        applyAltsFiltering(option_alts_filtered ? option_alts_filter : -1);
                    },
                    selectionMode: "multiple",
                    items: [{
                        "ID": 1, 
                        "text": flt.title,
                        "hint": flt.hint,
                        "icon": "fas fa-filter"
                    }]
                }
            });

        }

        if (hasSA) {
            var userID = -1; 
            var allUsersAndGroups = (typeof data_charts.data != "undefined" && data_charts.data != null && typeof data_charts.data.users != "undefined" ? JSON.parse(JSON.stringify(data_charts.data.users)) : []);
            if (allUsersAndGroups.length && typeof data_sa.data != "undefined" && data_sa.data != null && typeof data_sa.data.saUserID != "undefined") userID = data_sa.data.saUserID;
            var curUser = (allUsersAndGroups.length ? getItemFromList(data_charts.data.users, userID) : null);
<% If Not ViewOnly Then %>
            toolbar_items.push({
                location: 'before',
                locateInMenu: 'auto',
                showText: "always",
                widget: 'dxButton',
                disabled: viewOnly || !has_data || !(allUsersAndGroups.length),
                options: {
                    elementAttr: {id: 'btnSAUser', "data-uid": userID},
                    "text": "User/Group", //((curUser) ? curUser.name : "Loading...")
                    "hint": "Sensitivity Analysis user/group" + ((curUser) ? ": " + curUser.name : ""),
                    "icon": "fas " + (!has_data || !(allUsersAndGroups.length) ? "fa-user-clock" : (userID <0 ? "fa-user-friends" : "fa-user-check")),
                    onClick: function (e) {
                        var allUsersAndGroups = JSON.parse(JSON.stringify(data_charts.data.users));
                        dlgSelectUsers(allUsersAndGroups, (typeof data_sa.saUserID == "undefined" ? (!(data_sa.data) || typeof data_sa.data.saUserID == "undefined" ? -1 : data_sa.data.saUserID) : data_sa.saUserID), false, function (selID) {
                            //item.ContentOptions["userID"] = JSON.stringify(selID);
                            var old = (typeof data_sa.saUserID == "undefined" ? (!(data_sa.data) || typeof data_sa.data.saUserID == "undefined" ? -1 : data_sa.data.saUserID) : data_sa.saUserID);
                            if (old != selID) {
                                data_sa.saUserID = selID;
                                updateDashboardItem(item, function (data) {
                                    refreshItemToolbar();
                                    resetSAWidgets();
                                    SALoadData(currentDashboard().Items);
                                });
                                e.component.option({
                                    disabled: true,
                                    icon: "fas fa-spinner fa-pulse"
                                });
                            }
                        });
                    },
                }
            });
<% End If %>
            toolbar_items.push({
                location: 'before',
                locateInMenu: 'auto',
                showText: "always",
                widget: 'dxButton',
                disabled: !has_data,
                options: {
                    onClick: function (e) {
                        $(".DSACanvas.widget-created").sa("resetSA");
                    },
                    "text": "Reset SA",
                    "hint": "Reset sensitivities",
                    "icon": "dx-icon fas fa-undo"
                }
            });
        }

    }

    function getAltsFilteredCount(data) {
        var sel = -1;
        var cnt = -1;
        var h = "Toggle alternatives filtering";
        var t = "...";
        if (typeof data.data != "undefined" && data.data != null && typeof data.data.alts != "undefined") {
            cnt = data.data.alts.length - 1;
            sel = 0;
            for (var i=1; i<data.data.alts.length; i++) {
                if (typeof data.data.alts[i].visible != "undefined" && (data.data.alts[i].visible)) sel += 1;
            }
            t = sel + "/" + cnt;
        }
        if (typeof alt_filter_modes[option_alts_filter]!= "undefined") h += " (" + alt_filter_modes[option_alts_filter] + ")";
        return {"total": cnt, "selected": sel, "title": t, "hint": h};
    }

    var tvwrt_manual = false;

    function updateToolbarWRT(id) {
        var div = $("#WRTNodeID");
        if ((div) && div.data("dxDropDownBox")) {
            var has_data = hasData();
            var _m = tvwrt_manual;
            tvwrt_manual = true;
            var dd = div.dxDropDownBox("instance");
            if (!dd.option("dataSource").length || dd.option("value") != id) {
                dd.option({
                    disabled: !has_data,
                    dataSource: (has_data ? data_charts.data.objs : []),
                });
            }
            if (id !== "" && id>=0 && has_data) {
                dd.option({value: id});
                updateTreeViewWRT(id);
            }
            if (!_m) tvwrt_manual = false;
        }
    }

    function updateTreeViewWRT(id) {
        var div = $("#tvWRTNodeID");
        if ((div) && div.data("dxTreeView")) {
            var has_data = hasData();
            var tv = div.dxTreeView("instance");
            var _m = tvwrt_manual;
            tvwrt_manual = true;
            div.find(".dx-treeview-node").removeClass("dx-state-focused").blur();
            var currentNode = div.find("[data-item-id=" + id + "]");
            if ((currentNode) && (currentNode.length)) currentNode.focus().addClass("dx-state-focused");
            if (!_m) tvwrt_manual = false;
        }
    }

    function setWRTNodeID(node_id) {
        if (wrtNodeID() != node_id) {
            var _m = tvwrt_manual;
            tvwrt_manual = true;

            data_charts.wrt = node_id;
            updateToolbarWRT(node_id);

            if (_opt_wrt_sync_objectives_panel) {
                var items = dashboardItemsByType(_rep_item_Objectives);
                for (var i=0; i<items.length; i++) {
                    var div = getElement(items[i]);
                    if ((div) && (div.data("dxTreeList"))) {
                        div.dxTreeList("option", {"focusedRowKey": node_id});
                    }
                }
            }

            var _types = [];
            var items_all = dashboardItems();
            for (var i=0; i<items_all.length; i++) {
                var item= items_all[i];
                if (typeof item.ItemType != "undefined") _types[item.ItemType] = true;
            }

            var hasCharts = _types[_rep_item_AlternativesChart] === true || _types[_rep_item_ObjectivesChart] === true;
            var hasSA = _types[_rep_item_DSA] === true || _types[_rep_item_PSA] === true || _types[_rep_item_GSA] === true || _types[_rep_item_Analysis2D] === true || _types[_rep_item_HTH] === true || _types[_rep_item_ASA] === true;
            var hasGrids = _types[_rep_item_ObjsGrid] === true || _types[_rep_item_AltsGrid] === true;
            
            if (hasCharts || hasSA || hasGrids) {
                showLoadingPanel();
                resetChartWidgets(true);
                resetSAWidgets();
                resetData();
                var items = dashboardItems();
                chartLoadData(items);
                SALoadData(items);
            }
            
            if (!_m) tvwrt_manual = false;
        }
    }

    // ================================== Item toolbars ==========================================

    function refreshItemToolbar() {
        var opened = $('[id^="dashbrdToolbar"]:visible');
        for (var i=0; i<opened.length; i++) {
            var t_id = $(opened[i]).data("item-id")*1;
            if (t_id>0) onItemMenu(t_id);
        }
    }

    function onItemMenuToggle(id) {
        var tb = $("#dashbrdToolbar" + id);
        if ((tb) && (tb.length))
        {
            if (tb.is(":visible")) onItemMenuHide(id); else onItemMenu(id);
        }
    }

    function onItemMenuHide(id) {
        var tb = $("#dashbrdToolbar" + id);
        if ((tb) && (tb.length) && tb.is(":visible")) {
            var isExpanded = $("#itemCell" + id).hasClass("dashbrdExpanded");
            if (true || !isExpanded) {
                tb.hide();
                var tile = $("#tileItem" + id).parent();
                if ((tile) && (tile.length)) resizeItemTile(dashboardItems(), tile);
            }
        }
    }

    function isItemMenuOpened(id) {
        return $("#dashbrdToolbar" + id).is(":visible");
    }

    function onItemMenu(id) {
        var opened = $('[id^="dashbrdToolbar"]:visible');
        for (var i=0; i<opened.length; i++) {
            var t_id = $(opened[i]).data("item-id")
            if (id != t_id) onItemMenuHide(t_id);
        }
        var d = currentDashboard();
        var item = ((d) ? d.Items[id] : null);
        var is_new = (typeof item == "undefined" || typeof item.ItemType == "undefined" || item.ItemType == _rep_item_New);
        var tb = $("#dashbrdToolbar" + id);
        if ((tb) && (tb.length)) {

            var isExpanded = $("#itemCell" + id).hasClass("dashbrdExpanded");
            showLabel = (isExpanded ? "always" : "inMenu");

            var items = [];

            if (!(is_new) && (item) && typeof item.ItemType != "undefined" && item.ItemType != "" && isItemEnabled(item)) {
                var func_name = "toolbar" + item.ItemType;
                if (typeof window[func_name] == "function") {
                    var tbfunc = window[func_name](item);
                    if ((tbfunc) && typeof tbfunc != "undefined") {
                        for (var i=0; i<tbfunc.length; i++) {
                            if (typeof tbfunc[i].location == "undefined") tbfunc[i].location = "before";
                            items.push(tbfunc[i]);
                        }
                    }
                }
            }

            items.push(<% If Not ViewOnly Then %>{
                    location: (is_new ? 'center' : 'after'),
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    showText: (is_new ? 'always' : showLabel),
                    options: {
                        text: (is_new ? "Place panel" : "Properties"),
                        icon: (is_new ? "fas fa-magic" : "far fa-edit"),
                        hint: (is_new ? "Place new panel" : "Edit panel properties"),
                        onClick: function () {
                            if (is_new) {
                                addDashboardItem(id, onEditDashboardItem);
                            } else {
                                editDashboardItem(id, onEditDashboardItem);
                            }
                        }
                    }
                }, {
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    showText: showLabel,
                    options: {
                        text: "Clone",
                        hint: "Clone panel",
                        icon: "far fa-clone",
                        visible: !is_new,
                        onClick: function () {
                            cloneDashboardItem(id, onEditDashboardItem);
                        }
                    }
                }, {
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    showText: showLabel,
                    options: {
                        text: "Delete",
                        hint: "Delete panel",
                        icon: "far fa-trash-alt",
                        visible: !is_new,   // (!is_new || (typeof d.Items != "undefined" && (Object.keys(d.Items).length>0))),
                        onClick: function () {
                            deleteDashboardItem(id, onDeleteDashboardItem);
                        }
                    }
                },<% End If %> {
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    showText: showLabel,
                    options: {
                        text: "Open Page",
                        hint: "Open original page",
                        icon: "fas fa-external-link-alt",
                        disabled: !isPageAvailable(item.PageID),
                        visible: (!is_new && (d) && (typeof item != "undefined") && (typeof item.EditURL != "undefined") && item.EditURL!="" && item.EditURL!="/"),
                        onClick: function () {
                            loadURL(item.EditURL);
                        }
                    }
                });

            <% If ShowDraftPages() AndAlso Not ViewOnly Then %>if (!is_new) {
                items.push({
                    location: 'after',
                    locateInMenu: 'auto',
                    widget: 'dxButton',
                    showText: showLabel,
                    visible: true,
                    options: {
                        text: "Panel data",
                        icon: 'fas fa-code',
                        hint: "Panel JSON data",
                        //elementAttr: { id: 'btn' },
                        onClick: function () {
                            var code = JSON.stringify(item, null, '  ')
                            code = code.replace(/\"(\w)+\"\:/g, "<span style='color:navy'>$&</span>");
                            showMessagePopup("<pre style='word-wrap: break-word; font-size:0.78rem;'><b>item</b>: " + code + "</pre>", "Panel JSON data (debug)", 900);
                        }
                    }
                });
            }<% End If %>

            tb.show().css({"width" : (isExpanded ? "100%" : "width: calc(100% + 8px)")});
            tb.dxToolbar({
                "items": items
            });

            var jlst = $("#divJumpToList");
            if ((jlst) && (jlst.length) && jlst.is(":visible") && jlst.data("dxList")) {
                jlst.dxList("instance").option({selectedItemKeys: id});
            }

            if (!isExpanded) {
                var tile = $("#tileItem" + id).parent();
                if ((tile) && (tile.length)) resizeItemTile(dashboardItems(), tile);
            }
        }
    }
    
    function onItemToggle(id) {
        var el = $("#itemCell" + id);
        var preview = $(_dash_PreviewID);
        var p = $("#tileItem" + id);
        if ((el) && (el.length) && (preview) && (preview.length) && (p) && (p.length)) {
            var expanded = false;
            el.toggleClass("dashbrdExpanded");
            var icon = $("#iconExpand" + id);
            if ((preview).is(":visible")) {
                el.detach().appendTo(p);
                preview.hide().data("item", "");
                icon.addClass("fa-expand").removeClass("fa-compress").prop("title", "Expand"); // -arrows-alt
                el.find(".dashbrdDraggable").removeClass("noDragging");
                onItemMenuHide(id);
                $("#dashbrdMenu" + id).css("opacity", 1);
                expanded = false;
            } else {
                //el.css('background-color', '');
                el.detach().appendTo(preview);
                preview.show().data("item", id);
                icon.removeClass("fa-expand").addClass("fa-compress").prop("title", "Collapse");
                el.find(".dashbrdTitle").width("");
                onItemMenu(id);
                $("#dashbrdMenu" + id).css("opacity", 0);
                expanded = true;
            }

            var scr = el.find(".dashbrdScrollable");
            if ((scr) && (scr.length)) {
                var scr_content = scr.find(".dx-scrollview-content");
                if ((scr) && (scr_content) && (scr_content.length)) {
                    var p = scr[0].parentNode;
                    scr.hide();
                    var w = p.clientWidth - (expanded ? 24 : 0);
                    var h = p.clientHeight - (expanded ? 24 : 16);
                    scr_content.height(h).width(w);
                    scr.show();
                    var item = dashboardItemByID(id);
                    if ((item)) resizeItem(item, w, h);
                }
            }
        }
    }

    function toolbarButton(item, prop_name, prop_title, prop_hint, icon, onClick, isAdvanced) {
        var option =  {
            widget: 'dxButton',
            locateInMenu: 'auto',
            visible: isAdvanced && isAdvancedMode || !isAdvanced,
            options: {
                keyExpr: "ID",
                displayExpr: "text",
                elementAttr: (isAdvanced ? {class:"on_advanced"} : {}),
                onClick: function (e) {
                    if (typeof onClick == "function") onClick(e, item);
                },
                "text": prop_title,
                "hint": (prop_hint || prop_title),
                "icon": icon
            }
        }
        return option;
    }

    function toolbarToggleButton(item, prop_name, prop_title, prop_hint, icon, defOptions, isInvert, isAdvanced) {
        var opt_value = getItemOptionValue(item, prop_name, defOptions);
        var option =  {
            widget: 'dxButtonGroup',
            locateInMenu: 'auto',
            visible: isAdvanced && isAdvancedMode || !isAdvanced,
            options: {
                keyExpr: "ID",
                selectedItemKeys: (opt_value && !isInvert || !opt_value && isInvert ? [prop_name] : []),
                displayExpr: "text",
                focusStateEnabled: false,
                elementAttr: (isAdvanced ? {class:"on_advanced"} : {}),
                onItemClick: function (e) {
                    var sel = e.component.option("selectedItemKeys");
                    item.ContentOptions[prop_name] = (sel.length>0 && !isInvert || sel.length == 0 && isInvert);
                    updateItemOption(item, prop_name, item.ContentOptions[prop_name]);
                    <% If Not ViewOnly Then %>updateDashboardItem(item, function () { });<% End If %>
                },
                selectionMode: "multiple",
                items: [{
                    "ID": prop_name, 
                    "text": prop_title,
                    "hint": (prop_hint || prop_title),
                    "icon": icon
                }]
            }
        }
        return option;
    }

    // =====================================  Dialogs / popups =================================================

    function jumpTo(id) {
        var el = $("#itemCell" + id);
        if ((el) && (el.length)) {
            onItemMenu(id);
            $(_dash_ElementID).dxScrollView("instance").scrollToElement(el);
            el.addClass("blink_opacity_twice");
            setTimeout(function () {
                el.removeClass("blink_opacity_twice");
            }, 2500);
        }
    }

    function toggleJumpPopup(lst_items) {
        var dlg = $("#divJumpPopup");
        if ((dlg) && (dlg.length)) {
            if (dlg.data("dxPopup") && dlg.is(":visible")) {
                dlg.dxPopup("hide");
                dlg.hide();
            } else {
                var div_lst = $("<div id='divJumpToList'></div>");
                div_lst.dxList({
                    keyExpr: "id",
                    displayExpr: "text",
                    items: lst_items,
                    <% If Not ViewOnly Then %>itemDragging: {
                        allowReordering: true,
                        dragDirection: "vertical",
                        moveItemOnDrop: true,
                        onReorder: function (e) {
                            if ((e) &&  (e.fromIndex != e.toIndex)) {
                                isCreating = true;
                                showLoadingPanel();
                                setTimeout(function () {
                                    var items = dashboardUpdateItemIndex(e.fromIndex+1, e.toIndex+1, true);
                                    updateTileItems(items);
                                }, 30);
                            }
                        }
                    },<% End If %>
                    onItemClick: function(e) {
                        if (typeof e.itemData.id != "undefined") {
                            jumpTo(e.itemData.id);
                        }
                    },
                });

                initItemsListPopup(div_lst, lst_items);
               
                dlg.show();
                dlg.dxPopup({
                    contentTemplate: function() {
                        return div_lst;
                    },
                    minHeight: 80,
                    minWidth: 100,
                    height: lst_items.length*28 + 36,
                    width: dlgMaxWidth(220),
                    maxHeight: dlgMaxHeight(600),
                    maxWidth: dlgMaxWidth(600),
                    showTitle: true,
                    title: "Panels list",
                    dragEnabled: !viewOnly,
                    shading: false,
                    closeOnOutsideClick: false,
                    resizeEnabled: true,
                    showCloseButton: true,
                    position: { 
                        my: 'right top', 
                        at: 'right top', 
                        of: $("#tblDashboardsMain"), 
                        offset: { x: -24, y: 36 } }
                });
                dlg.find(".dx-popup-content").css("padding", "4px");
                dlg.dxPopup("show");
            }
        }
    }

    function initItemsListPopup(div_lst, lst_items) {
        <% If Not ViewOnly Then %>div_lst.find(".dx-list-item-after-bag").each(function (idx, el) {
            if ((el) && typeof lst_items[idx]!="undefined") {
                var item = lst_items[idx];
                var is_new = (typeof item == "undefined" || typeof item.type == "undefined" || item.type == _rep_item_New);
                var act = $("<div class='dx-list-item-after-bag' style='width:0px;'><div class='dashbrd_item_actions' style='display:none'><a href='' class='as_icon' onclick='onItemQuickEdit(" + item.id + "," + (is_new) + "); return false;' style='margin-right:4px' title='Edit'><i class='" +  (is_new ? "fas fa-magic" : "far fa-edit") + "'></i></a><a href='' onclick='deleteDashboardItem(" + item.id +", onDeleteDashboardItem); return false;' class='as_icon' title='Delete'><i class='far fa-trash-alt'></i></a></div></div>");
                act.insertBefore($(el));
                $(el).parent().on("mouseover", function (e) {
                    if ((e)) {
                        var td = $(e.currentTarget).find(".dashbrd_item_actions");
                        if ((td) && (td.length)) td.show().parent().width(28);
                    }
                }).on("mouseleave", function (e) {
                    if ((e)) {
                        var td = $(e.currentTarget).find(".dashbrd_item_actions");
                        if ((td) && (td.length)) td.hide().parent().width(0);
                    }
                });
            }
        });<% End If %>
    }
<% If Not ViewOnly Then %>
    function dlgSelectUsers(userslist, selectedIDs, multi_allowed, on_save) {
        var dataGrid = $("<div id='dlgSelUsers' class='whole'></div>");
        var dg = dataGrid.dxDataGrid({
            dataSource: userslist,
            columns: [{dataField: "id", width: 32, visible: false},
                      {dataField: "name"},
                      {dataField: "email"},
                      {dataField: "hasdata", caption: "Has Data", width: 80},
                      {dataField: "group", caption: "Group", width: 80}
            ],
            hoverStateEnabled: true, 
            width: "100%",
            height: "100%",
            filterRow: { visible: true },
            scrolling: { mode: "infinite" },
            keyExpr: "id",
            focusedRowEnabled: !multi_allowed,
            selection: { 
                mode: (multi_allowed ? "multiple" : "single"),
                showCheckBoxesMode: "always",
            },
            onFocusedRowChanging: function (e) {
                if (!multi_allowed && typeof userslist[e.newRowIndex] != "undefined") {
                    if (!userslist[e.newRowIndex].hasdata) e.cancel = true;
                }
            },
            onRowPrepared: function (e) {
                if (typeof userslist[e.rowIndex] != "undefined") {
                    if (!userslist[e.rowIndex].hasdata) e.rowElement.addClass("gray");
                }
            },
            onCellDblClick : function (e) {
                if (!multi_allowed && (e.data)) onSaveUser();
            },
            loadPanel: main_loadPanel_options,
        }).dxDataGrid("instance");

        function onSaveUser() {
            if ((dataGrid) && (dataGrid.data("dxDataGrid"))) {
                var selIDs = [];
                if (multi_allowed) {
                    selIDs = dataGrid.dxDataGrid("instance").getSelectedRowKeys();
                } else {
                    selIDs = dataGrid.dxDataGrid("instance").option("focusedRowKey");
                }
                if (typeof on_save == "function") on_save(selIDs);
            }
            closePopup();
        }

        if (multi_allowed) {
            dg.option("selectedRowKeys", selectedIDs);
        } else {
            dg.option("selectedRowKeys", [selectedIDs]);
            dg.option("focusedRowKey", selectedIDs);
        }

        $("#popupContainer").dxPopup({
            contentTemplate: function() {
                return dataGrid;
            },
            animation: {
                show: { "duration": 0 },
                hide: { "duration": 0 }
            },
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    elementAttr: ({"id": "btnSelUserGroup"}),
                    text: resString("btnOK"),
                    onClick: function () {
                        onSaveUser();
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }, {
                widget: 'dxButton',
                options: {
                    text: resString("btnCancel"),
                    onClick: function () {
                        closePopup();
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }],
            height: dlgMaxHeight(850)-60,
            width: dlgMaxWidth(1200),
            showTitle: true,
            title: "Select Participants and Groups",
            dragEnabled: true,
            shading: true,
            closeOnOutsideClick: false,
            resizeEnabled: true,
            showCloseButton: false
        });
        $("#popupContainer").dxPopup("show");
        dataGrid.focus();
    }
<% End If %>
    // =================================== Methods for resize / adjust ================================

    // Call for item; w and h are optional;
    function resizeItem(item, w, h) {
        if (!$(".ecDashboard").hasClass("resizing")) {
            if ((item) && typeof item.ItemType != "undefined" && item.ItemType != "") {
                var func_name = "resize" + item.ItemType;
                if (typeof window[func_name] == "function") {
                    if (typeof w == "undefined" || w < 1) w = 0;
                    if (typeof h == "undefined" || h < 1) h = 0;
                    if (w<1 || h<1) {
                        var el = $get("itemContent" + item.ID);
                        if ((el)) {
                            if (w<1) w = Math.ceil(el.clientWidth);
                            if (h<1) h = Math.ceil(el.clientHeight);
                        }
                    }
                    if (w>0 && h>0) window[func_name](item, w, h);
                }
            }
        }
    }

    //function resizeByType(type, type_id) {
    function resizeByType(type, type_id) {
        //var items = dashboardItemsByType(type, type_id);
        var items = dashboardItemsByType(type);
        if ((items)) {
            for (i = 0; i < items.length; i++) {
                resizeItem(items[i]);
            }
        }
    }

    function resizeAll(force) {
        if (typeof resize_custom_ == "function") resize_custom_(force);
    }

    function onResize(force, w, h) {
        if (!$(".ecDashboard").hasClass("resizing") && !(_is_printing)) {
            var w_ = w + 6;
            var h_ = h - $("#tdHeader").height() - _dash_cellMargin;

            var d = currentDashboard();
            _dash_cellsPerRow = 1*((d) ? d.Options.cellsPerRow : _dash_def_cols);
            var _perRow = _dash_cellsPerRow;

            var force_redraw = false;

            var viewmode = _dash_viewmode;
            var allowed = true;
            _dash_cellWidth = Math.floor((w - 24) / _dash_cellsPerRow) - _dash_cellMargin;
            if (_dash_cellWidth<_dash_cellMinWidth) {
                // --- auto-adjust grid cells ---
                //_dash_cellsPerRow = Math.floor((w - 16) / (_dash_cellMinWidth + _dash_cellMargin));
                //_dash_cellWidth = Math.floor((w - 16) / _dash_cellsPerRow) - _dash_cellMargin;

                allowed = false;
                viewmode = 1;
            }
            if (_dash_viewmode_allowed != allowed || _dash_viewmode != viewmode) {
                _dash_viewmode_allowed = allowed;
                force_redraw = true;
                initToolbar();
            }
            if ((typeof isFullscreenMode == "undefined" || !isFullscreenMode)) {
                $("#toolbar").show();
            } else {
                $("#toolbar").hide();
            }
           
            _dash_def_aspect =  (w_>0 ? (h_/w_) : 1);
            var rows = Math.ceil(_dash_def_aspect * _dash_cellsPerRow);
            _dash_cellHeight =  Math.floor((h_ - _dash_cellMargin) / rows) - _dash_cellMargin;
            _dash_cellsPerCol = Math.floor((h_ - _dash_cellMargin)/(_dash_cellHeight+_dash_cellMargin));

            if ((w>30) && (h>30) && (dashboardTiles) && ($(_dash_ElementID).data("dxTileView"))) {

                dashboardTiles_updating = true;
                dashboardTiles.beginUpdate();
                dashboardTiles.option("baseItemWidth", _dash_cellWidth);
                dashboardTiles.option("baseItemHeight", _dash_cellHeight);
                dashboardTiles.option("width", w_);
                dashboardTiles.option("height", h_);
                if ((d) && (force_redraw || _perRow != _dash_cellsPerRow)) { 
                    dashboardTiles.option("items", prepareTileItems(dashboardItems()));
                    setTimeout(function() {
                        $(_dash_ElementID).find(".dx-tile-content").resizable(_dash_viewmode_allowed ? "enable" : "disable");
                    }, 100);
                }
                dashboardTiles.endUpdate();

                var items = dashboardItems();
<% If Not ViewOnly Then %>
                if (items.length>0 && _dash_viewmode == 0) {
                    var gr = [Math.floor((w_ - _dash_cellMargin)/_dash_cellsPerRow-1), Math.floor((h_ - _dash_cellMargin)/_dash_cellsPerCol)];
                    var d_height = -1;
                    var main_height = 0;
                    var main_bottom = 0;
                    $(_dash_ElementID).find(".dx-tile-content").resizable({
                        animate: false,
                        autoHide: true,
                        ghost: false,
                        //helper: "dashbrd_resizing",
                        grid: gr,
                        //minWidth: Math.round(_dash_cellWidth - _dash_cellMargin - 1),
                        //minHeight: Math.round(_dash_cellHeight - _dash_cellMargin - 1),
                        maxWidth: Math.round(w_),
                        maxHeight: Math.round(_dash_cellsPerRow * _dash_cellHeight),
                        start: function(event, ui) {
                            resize_ignore = true;
                            $(".ecDashboard").addClass("resizing");

                            var t = $(".ecDashboard").find(".dx-scrollable-container").first().scrollTop();
                            main_height = $(".ecDashboard").find(".dx-scrollable-content").first().height();
                            if (main_height) {
                                $(".ecDashboard").find(".dx-scrollable-content").first().height(main_height + _dash_cellsPerCol*_dash_cellHeight - ui.originalSize.height);
                                $(".ecDashboard").find(".dx-scrollable-container").first().scrollTop(t);
                            }

                            //d_height = $(_dash_ElementID).height();
                            //if (d_height>50) $(_dash_ElementID).height(d_height + 5*_dash_cellHeight).css("overflow-y", "hidden");

                            ui.element.addClass("dashbrd_resizing").find(".dashbrdItem").hide();
                            ui.helper.addClass("dashbrd_resizing");
                        },
                        resize: function(event, ui) {
                            if (ui.originalSize.height != ui.size.height) {
                                var p = ui.element.parent().position().top + ui.size.height;
                                if ((p > (main_height+2)) && (p < (main_height + _dash_cellHeight + _dash_cellMargin)) && (ui.size.height < (_dash_cellsPerRow*_dash_cellHeight - _dash_cellMargin))) {
                                    var t = $(".ecDashboard").find(".dx-scrollable-container").first().scrollTop();
                                    $(".ecDashboard").find(".dx-scrollable-container").first().scrollTop(_dash_cellHeight + t);
                                    ui.size.height += _dash_cellHeight;
                                }
                            }
                        },
                        stop: function(event, ui) {
                            var item = ui.element.data("item");
                            if ((item)) {
                                ui.element.removeClass("dashbrd_resizing").find(".dashbrdItem").show();
                                $(".ecDashboard").removeClass("resizing");
                                if (main_height) {
                                    $(".ecDashboard").find(".dx-scrollable-content").first().height("");
                                }
                                //if (d_height>50) $(_dash_ElementID).height(d_height).css("overflow-y", "");
                                if (Math.abs(ui.size.width - ui.originalSize.width)>4 || Math.abs(ui.size.height - ui.originalSize.height)>4) {
                                    showLoadingPanel();
                                    item.ItemOptions["cols"] = Math.round(ui.size.width/_dash_cellWidth);
                                    item.ItemOptions["rows"] = Math.round(ui.size.height/_dash_cellHeight);
                                    setTimeout(function () {
                                        updateDashboardItem(item, function (id) { 
                                            showLoadingPanel();
                                            setTimeout(function () { onEditDashboardItem(id) }, 30);
                                        });
                                    }, 30);
                                }
                                resize_ignore = false;
                            }
                        }
                    });
                }
<% End If %>
                $(".dx-scrollview-content.dx-tileview-wrapper").height("");

                var tiles = $(".dx-item.dx-tile");
                for (var i=0; i<tiles.length; i++) {
                    var tile = $(tiles[i]);
                    resizeItemTile(items, tile);
                }
                dashboardTiles_updating = false;
                return true;
            }
        }
        return false;
    }

    function resizeItemTile(items, tile) {
        if (!$(".ecDashboard").hasClass("resizing")) {
            var content = tile.find(".dx-tile-content");
            if ((content) && (content.length)) content.hide();
            var w = tile.width();
            var h = tile.height();
            if ((content) && (content.length)) content.show();
                
            var title = tile.find(".dashbrdTitle");
            if ((title) && (title.length)) title.width(w-48);

            var scr = tile.find(".dashbrdScrollable");
            if ((scr) && (scr.length)) {
                var p = scr[0].parentNode;
                var scr_content = scr.find(".dx-scrollview-content");
                // TODO: redo this approach
                if ((scr_content) && (scr_content.length)) {
                    scr.hide();
                    var w_ = p.clientWidth;
                    var h_ = p.clientHeight;
                    scr_content.height(h_).width(w_);
                    scr.show();
                    var el = scr.find(".dashbrdContent");
                    if ((el) && (el.length)) {
                        var id = el.data("item-id");
                        if (typeof id != "undefined" && id!="") {
                            var item = getItemByID(items, id*1);
                            if ((item)) {
                                resizeItem(item, w_, h_);
                            }
                        }
                    }
                }
            }
        }
    }

    // ===================================== Items (widget) actions ==============================================

    function initWidgetOptions(widget, options) {
        if ((widget) && (options)) {
            for (var id in options) {
                //if (typeof widget[id] != "undefined") {
                widget[id] = (typeof options[id] == "string" && options[id].length>2 && (options[id][0] == "[" || options[id][0] == "{")) ? JSON.parse(options[id]) : options[id];
                //}
            }
        }
        return widget;
    }

    function getItemOptionValue(item, opt_name, def_vals) {
        var opt_val = (typeof item.ContentOptions[opt_name] != "undefined" ? item.ContentOptions[opt_name] : def_vals[opt_name]); 
        return opt_val;
    }

    function toolbarCheckBox(item, prop_name, prop_text, prop_hint, defOptions, isAdvanced) {
        var opt_value = getItemOptionValue(item, prop_name, defOptions);
        var option = {
            location: 'before',
            locateInMenu: 'auto',
            widget: 'dxCheckBox',
            visible: isAdvanced && isAdvancedMode || !isAdvanced,
            options: {
                text: prop_text,
                showText: true,
                hint: prop_hint,
                disabled: false,
                value: opt_value,
                elementAttr: (isAdvanced ? {class:"on_advanced"} : {}),
                onValueChanged: function (e) {
                    var opt_valueOld = getItemOptionValue(item, prop_name, defOptions);
                    if (opt_valueOld != e.value) {
                        item.ContentOptions[prop_name] = e.value;
                        <% If Not ViewOnly Then %>updateDashboardItem(item, function () {
                            updateItemOption(item, prop_name, item.ContentOptions[prop_name]);                        
                        });<% Else %>updateItemOption(item, prop_name, item.ContentOptions[prop_name]);<% End If %>
                    }
                }
            }
        };
        return option;
    }

    function toolbarSelectBox(item, prop_name, prop_hint, selectOptions, defOptions, isAdvanced, onChangeOption) {
        var opt_value = getItemOptionValue(item, prop_name, defOptions);
        if (item.ItemType == _rep_item_ObjectivesChart && opt_value == "altColumns") {
            opt_value = "objColumns";
        };
        var option = {   location: 'before',
            locateInMenu: 'auto',
            widget: 'dxSelectBox',
            showText: true,
            visible: isAdvanced && isAdvancedMode || !isAdvanced,
            options: {
                searchEnabled: false,
                valueExpr: "ID",
                value: opt_value,
                hint: prop_hint,
                //width: 120,
                displayExpr: "Text",
                elementAttr: (isAdvanced ? {class:"on_advanced"} : {}),
                onSelectionChanged: function (e) { 
                    var opt_valueOld = getItemOptionValue(item, prop_name, defOptions);
                    if (opt_valueOld != e.selectedItem.ID) {
                        item.ContentOptions[prop_name] = e.selectedItem.ID;
                        updateItemOption(item, prop_name, item.ContentOptions[prop_name]);
                        <% If Not ViewOnly Then %>updateDashboardItem(item, function () {
                            if (typeof onChangeOption == "function") onChangeOption(item, prop_name, item.ContentOptions[prop_name]);
                        });<% End If %>
                        //if (typeof onChangeOption == "function") setTimeout( function() {
                        //    onChangeOption(item, prop_name, item.ContentOptions[prop_name]);
                        //}, 100);
                    }
                },
                items: selectOptions
            }
        };
        return option;
    }

    // ==================== Charts ==============================

    function chartLoadData(items) {
        if (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%> && option_normalization == ntUnnormalized) option_normalization = ntNormalizedForAll;
        norm_options[0].disabled = (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%>);
        if (typeof data_charts.loading == "undefined" || !(data_charts.loading)) {
            var itemsList = "";
            var ids = [];
            for (let id in items) {
                var item = items[id];
                itemsList += (itemsList == "" ? "" : ",") + item.ItemType;
                if (typeof item.ContentOptions != "undefined") {
                    if (typeof item.ContentOptions.userIDs != "undefined") {
                        var userIDs = JSON.parse( item.ContentOptions.userIDs );
                        for (var i = 0; i < userIDs.length; i++) {
                            var uid = userIDs[i];
                            if (ids.indexOf(uid) < 0) ids.push(uid);
                        };
                    };
                };
            };
            var usersList = "";
            for (var i = 0; i < ids.length; i++) {
                usersList += (usersList == "" ? "" : ",") + ids[i];
            };
            data_charts["loading"] = true;
            var params = {"wrt": wrtNodeID()};
            params["items"] = itemsList;
            params["users"] = usersList;
            callAPI("pm/dashboard/?action=synthesize", params, chartDataLoaded);
            setToolbarStatus(false);
        }
    }

    function chartDataLoaded(data) {
        data_charts["loading"] = false;
        setToolbarStatus(true);
        if (typeof data == "object") {
            data_charts["data"] = data;
            if (typeof data.wrt != "undefined") data_charts.wrt = data.wrt;

            var items = dashboardItems();
            for (var i=0; i<items.length; i++) {
                var item = items[i];
                setItemNormalization(item, option_normalization, false);
            }

            var btnAltsFlt = $("#btnAltsFilter");
            if ((btnAltsFlt) && btnAltsFlt.data("dxButtonGroup") && typeof data_charts.data!= "undefined" && data_charts.data != null && typeof data_charts.data.alts != "undefined") {
                var flt = getAltsFilteredCount(data_charts);
                btnAltsFlt.dxButtonGroup({
                    disabled: (flt.total<=0 || flt.total == flt.selected),
                    visible: (option_alts_filter!=-1),
                    items: [{
                        "ID": "1", 
                        "text": flt.title,
                        "hint": flt.hint,
                        "icon": "fas fa-filter"
                    }]
                });
            }

            resizeChartWidgets();
            initToolbar();
            refreshItemToolbar();
        }
    }

    function getChartName(item) {
        return ((item) && typeof item.ID != "undefined") ? "chart" + item.ID : "";
    }

    function getChart(item) {
        if ((item) && typeof item.ID != "undefined") {
            if (!hasData()) chartLoadData(currentDashboard().Items);
            return "<div id='" + getChartName(item) + "' class='whole preloader_dots'></div>";
        } else {
            return "";
        }
    }

    function resizeChart(item, w, h) {
        if ((item) && hasData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "") {
                    div.removeClass("preloader_dots");
                    var options = {};
                    if (typeof item.ContentOptions != "undefined") options = initWidgetOptions(options, item.ContentOptions);
                    options["chartType"] = options["chartType"] || ecChartDefOptions["chartType"];
                    item.ContentOptions["chartType"] = options["chartType"];
                    if (typeof item.ItemType != "undefined") {
                        switch (item.ItemType) {
                            case _rep_item_AlternativesChart:
                                if (typeof options.chartType =="undefined" || options.chartType.indexOf("alt")<0) options.chartType = "altColumns";
                                break;
                            case _rep_item_ObjectivesChart:
                                if (typeof options.chartType =="undefined" || options.chartType.indexOf("obj")<0) options.chartType = "objColumns";
                                break;
                        }
                    };
                    options["selectedNodeID"] = wrtNodeID();
                    options["decimals"] = option_decimals;
                    options["backgroundColor"] = (typeof item.ItemOptions.bgColor != "undefined" && item.ItemOptions.bgColor != "" ? item.ItemOptions.bgColor : _dash_def_bgcolor);
                    var ds = JSON.parse(JSON.stringify(data_charts.data));
                    for (var i = 0; i < ds.alts.length; i++) {
                        ds.alts[i].filter = ds.alts[i].visible;
                        if (!option_alts_filtered) ds.alts[i].visible = 1;
                    };
                    if (options.chartType.indexOf("Columns") > 0) {
                        options["autoFontSize"] = options["autoFontSize"] || ecChartDefOptions["autoFontSize"]; 
                    };
                    options["adjustFontSize"] = 0;
                    options["dataSource"] = ds;
                    options["height"] = h-4;
                    options["width"] = w-4;
                    options["allowDrillDown"] = true;
                    div.ecChart(options);
                } else {
                    if (div.hasClass("widget-created")) div.ecChart("resize", w-4, h-4);
                }
            }
        }
    }

    function setChartFontSize(item, fs) {
        item.ContentOptions["fontSize"] = fs;
        updateItemOption(item, "fontSize", item.ContentOptions["fontSize"]);
        <% If Not ViewOnly Then %>updateDashboardItem(item, function () {});<% End If %>        
    };

    function adjustFont(item, af) {
        var prop_name = "adjustFontSize";
        var caf = $("#chart" + item.ID).ecChart("option", prop_name);
        if (af == 0) caf = 0;
        item.ContentOptions[prop_name] = caf + af;
        updateItemOption(item, prop_name, item.ContentOptions[prop_name]);
        <% If Not ViewOnly Then %>updateDashboardItem(item, function () {});<% End If %>
    };

    function adjustRotation(item, ar) {
        var prop_name = "rotateAngle";
        var cra = $("#chart" + item.ID).ecChart("option", prop_name);
        if (isNaN(cra)) cra = 0;
        item.ContentOptions[prop_name] = cra + ar;
        updateItemOption(item, prop_name, item.ContentOptions[prop_name]);
        <% If Not ViewOnly Then %>updateDashboardItem(item, function () {});<% End If %>
    };

    function adjustZoom(item, az) {
        var prop_name = "zoomRatio";
        var cz = $("#chart" + item.ID).ecChart("option", prop_name) * 1;
        if (isNaN(cz)) cz = 1;
        item.ContentOptions[prop_name] = cz + az;
        updateItemOption(item, prop_name, item.ContentOptions[prop_name]);
        <% If Not ViewOnly Then %>updateDashboardItem(item, function () {});<% End If %>
    };

    function toolbarChart(item) {
        var toolbarItems = [];
        if (item.ItemType == _rep_item_AlternativesChart || item.ItemType == _rep_item_ObjectivesChart) { 
            if (hasData() && typeof data_charts.data.users!="undefined")
            {
                <% If Not ViewOnly Then %>var userIDs = []; 
                if (typeof item.ContentOptions.userIDs != "undefined" && item.ContentOptions.userIDs.length) userIDs = JSON.parse(item.ContentOptions.userIDs);
                if (userIDs.length==0) userIDs = [<% =COMBINED_USER_ID %>];
                toolbarItems.push(toolbarButton(item, "users", userIDs.length + "/" + users_grp_total_cnt, "Select Participants and Groups", "dx-icon fas fa-users", function (e) {
                    var allUsersAndGroups = JSON.parse(JSON.stringify(data_charts.data.users));
                    dlgSelectUsers(allUsersAndGroups, userIDs, true, function (selIDs) {
                        item.ContentOptions["userIDs"] = JSON.stringify(selIDs);
                        item.ContentOptions["chartsPerPage"] = selIDs.length;
                        var has_new = false;
                        for (var i=0; i<selIDs.length; i++) {
                            var uid = selIDs[i];
                            var has_data = false;
                            for (p=0; p<data_charts.data.priorities.length; p++) {
                                if (data_charts.data.priorities[p].uid == uid) {
                                    has_data = true;
                                    break;
                                }
                            }
                            if (!has_data) {
                                has_new = true;
                                break;
                            }
                        }
                        var do_update = true;
                        var chart = getElement(item);
                        if ((chart) && (chart.length) && !has_new && chart.html()!="" && (chart.hasClass("widget-created"))) {
                            chart.ecChart("option", "userIDs", selIDs).ecChart("option", "chartsPerPage", selIDs.length).ecChart("redraw");
                            do_update = false;
                        }
                        updateDashboardItem(item, function (data) {
                            refreshItemToolbar();
                            if (do_update) {
                                resetElement(item);
                                chartLoadData(currentDashboard().Items);
                            }
                        });
                    });
                }));<% End if %>
                var selectChartType = (item.ItemType == _rep_item_ObjectivesChart ?(isAdvancedMode ? [{"ID": "objSunburst", "Text": "Hierarchical pie", "hint": "Hierarchical pie", "icon":"icon ec-chart-sunburst"}, 
                            {"ID": "objPie", "Text": "Pie", "hint": "Pie", "icon":"icon ec-chart-pie"},
                            {"ID": "objDoughnut", "Text": "Donut", "hint": "Donut", "icon":"icon ec-chart-donut"},
                            {"ID": "objColumns", "Text": "Columns", "hint": "Columns", "icon":"icon ec-chart-columns"},
                            {"ID": "objStacked", "Text": "Stacked bars", "hint": "Stacked bars", "icon":"icon ec-chart-stacked"}] : 
                            []) :
                            (isAdvancedMode ? [{"ID": "altPie", "Text": "Pie", "hint": "Pie", "icon":"icon ec-chart-pie"},
                            {"ID": "altDoughnut", "Text": "Donut", "hint": "Donut", "icon":"icon ec-chart-donut"},
                            {"ID": "altColumns", "Text": "Columns", "hint": "Columns", "icon":"icon ec-chart-columns"},
                            {"ID": "altStacked", "Text": "Stacked bars", "hint": "Stacked bars", "icon":"icon ec-chart-stacked"}] : 
                            [])) ;
                //{"ID": "objSunburst", "Text": "Hierarchical pie", "hint": "Hierarchical pie", "icon":"icon ec-chart-sunburst"}, 
                //{"ID": "objColumns", "Text": "Columns", "hint": "Columns", "icon":"icon ec-chart-columns"}
                if (selectChartType.length > 0) toolbarItems.push(toolbarSelectBox(item, "chartType", "Chart Type", selectChartType, ecChartDefOptions, false, refreshItemToolbar));
                toolbarItems.push(toolbarToggleButton(item, "showLegend", "Legend", "Toggle chart legend", "icon ec-chart-legend", ecChartDefOptions, false, false));         
                var cv = getItemOptionValue(item, "chartType", ecChartDefOptions);
                var afs = getItemOptionValue(item, "adjustFontSize", ecChartDefOptions);
                if (cv.indexOf("Sunburst") >= 0) {
                    toolbarItems.push(
                        {   location: 'before',
                            locateInMenu: 'auto',
                            widget: 'dxButton',
                            showText: "inMenu",
                            options: {
                                elementAttr: {class:"on_advanced"},
                                text: "Adjust Rotation CCW",
                                icon: "fas fa-angle-left",
                                hint: "Adjust Rotation CCW",
                                onClick: function (e) { 
                                    adjustRotation(item, -5); 
                                }
                            }
                        }
                    );
                };
                if (cv.indexOf("Stacked") < 0) toolbarItems.push(toolbarToggleButton(item, "isRotated", "Rotate", "Rotate chart", "dx-icon icon ec-chart-rotate-cw", ecChartDefOptions, false, false));
                if (cv.indexOf("Sunburst") >= 0) {
                    toolbarItems.push(
                        {   location: 'before',
                            locateInMenu: 'auto',
                            widget: 'dxButton',
                            showText: "inMenu",
                            options: {
                                text: "Adjust Rotation CW",
                                icon: "fas fa-angle-right",
                                hint: "Adjust Rotation CW",
                                onClick: function (e) { 
                                    adjustRotation(item, 5); 
                                }
                            }
                        }
                    );
                    toolbarItems.push(
                    {   location: 'before',
                        locateInMenu: 'auto',
                        widget: 'dxButton',
                        showText: "inMenu",
                        options: {
                            text: "Zoom In",
                            icon: "fas fa-search-plus",
                            hint: "Zoom In",
                            onClick: function (e) { 
                                adjustZoom(item, 0.1); 
                            }
                        }
                    }
                    );
                    toolbarItems.push(
                        {   location: 'before',
                            locateInMenu: 'auto',
                            widget: 'dxButton',
                            showText: "inMenu",
                            options: {
                                text: "Zoom Out",
                                icon: "fas fa-search-minus",
                                hint: "Zoom Out",
                                onClick: function (e) { 
                                    adjustZoom(item, -0.1); 
                                }
                            }
                        }
                        );

                };
                toolbarItems.push({
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxSelectBox',
                    visible: true,
                    options: {
                        showText: true,
                        hint: "Font Size",
                        focusStateEnabled:false,
                        valueExpr: "ID",
                        displayExpr: "Text",
                        value: (item.ContentOptions["fontSize"] || ecChartDefOptions["fontSize"]),
                        width: 100,
                        disabled: item.ContentOptions["chartType"].indexOf("Columns") > 0 && ((item.ContentOptions["autoFontSize"])),
                        elementAttr: {id: 'selFontSize' + item.ID},
                        onSelectionChanged: function (e) { 
                            var selFontSize = e.selectedItem.ID;
                            if (item.ContentOptions["fontSize"] !== selFontSize) {
                                setChartFontSize(item, selFontSize);
                            }
                        },
                        items:  [{"ID": 8, "Text": "8"},
                                {"ID": 10, "Text": "10"},
                                {"ID": 12, "Text": "12"},
                                {"ID": 14, "Text": "14 - Default"},
                                {"ID": 16, "Text": "16"},
                                {"ID": 18, "Text": "18"},
                                {"ID": 20, "Text": "20"}]
                    }
                });
                toolbarItems.push({
                    location: 'before',
                    locateInMenu: 'auto',
                    widget: 'dxCheckBox',
                    visible: item.ContentOptions["chartType"].indexOf("Columns") > 0,
                    options: {
                        text: "Auto font size",
                        hint: "Auto font size",
                        showText: true,
                        elementAttr: {id: 'cbAutoFontSize' + item.ID},
                        value: (item.ContentOptions["autoFontSize"] || ecChartDefOptions["autoFontSize"]),
                        onValueChanged: function (e) {
                            if (item.ContentOptions["autoFontSize"] !== e.value) {
                                item.ContentOptions["autoFontSize"] = e.value;
                                updateItemOption(item, "autoFontSize", e.value);
                                $('#selFontSize' + item.ID).dxSelectBox("option", "disabled", e.value);
                                <% If Not ViewOnly Then %>updateDashboardItem(item, function () {});<% End If %>
                            }
                        
                        }
                    }
                });
            //    toolbarItems.push(
            //        {   location: 'before',
            //            locateInMenu: 'auto',
            //            widget: 'dxButton',
            //            showText: "inMenu",
            //            visible: true,
            //            options: {
            //                elementAttr: {id: 'btnDecFont' + item.ID},
            //                text: "Decrease font size",
            //                icon: "icon ec-font-dec",
            //                hint: "Decrease size",
            //                onClick: function (e) { 
            //                    adjustFont(item, -2);
            //                    $("#btnResetFont" + item.ID).dxButton("option", "disabled", item.ContentOptions.adjustFontSize == 0);
            //                }
            //            }
            //        });
            //    toolbarItems.push(
            //    {   location: 'before',
            //        locateInMenu: 'auto',
            //        widget: 'dxButton',
            //        showText: "inMenu",
            //        visible: true,
            //        options: {
            //            elementAttr: {id: 'btnIncFont' + item.ID},
            //            text: "Increase size",
            //            icon: "icon ec-font-inc",
            //            hint: "Increase font size",
            //            onClick: function (e) { 
            //                adjustFont(item, 2); 
            //                $("#btnResetFont" + item.ID).dxButton("option", "disabled", item.ContentOptions.adjustFontSize == 0);
            //            }
            //        }
            //    });
            //    toolbarItems.push(
            //{   location: 'before',
            //    locateInMenu: 'auto',
            //    widget: 'dxButton',
            //    showText: "inMenu",
            //    visible: true,
            //    options: {
            //        elementAttr: {id: 'btnResetFont' + item.ID},
            //        disabled: afs == 0,
            //        icon: "fas fa-undo-alt",
            //        text: "Reset font",
            //        hint: "Reset font size",
            //        onClick: function (e) { 
            //            adjustFont(item, 0); 
            //            $("#btnResetFont" + item.ID).dxButton("option", "disabled", true);
            //        }
            //    }
            //});
                if (cv.indexOf("Columns") >= 0) toolbarItems.push(toolbarToggleButton(item, "showComponents", "Components", "Show Components", "dx-icon icon ec-chart-components", ecChartDefOptions, false, false)); 
                toolbarItems.push(toolbarToggleButton(item, "singleRow", "Grid View", "Grid View / Single Row View", "dx-icon icon ec-multi-row", ecChartDefOptions, true, true));
                var selectOptions = [{"ID": "none", "Text": "Sort None", "icon":"icon ec-sort-none"}, 
                                    {"ID": "name", "Text": "Sort by Name", "icon":"icon ec-sort-az"}, 
                                    {"ID": "value", "Text": "Sort by Priority", "icon":"icon ec-sort-val"}];
                toolbarItems.push(toolbarSelectBox(item, "sortBy", "Sorting method", selectOptions, ecChartDefOptions, false));
<%--                if (item.ItemType == _rep_item_AlternativesChart) {
                    //var selectOptions = [{"ID": "none", "Text": "Unnormalized"}, {"ID": "normAll", "Text": "Normalized for All"}, {"ID": "normSelected", "Text": "Normalized for Selected"}, {"ID": "norm100", "Text": "% of Maximum"} ];
                    var selectOptions = [{"ID": "none", "Text": "Unnormalized", disabled: (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%>)}, {"ID": "normAll", "Text": "Normalized for All"}, {"ID": "norm100", "Text": "% of Maximum"} ];
                    toolbarItems.push(toolbarSelectBox(item, "normalizationMode", "Normalization", selectOptions, ecChartDefOptions, false));
                };--%>
                toolbarItems.push(toolbarToggleButton(item, "showLabels", "Labels", "Show Labels", "icon ec-chart-labels", ecChartDefOptions, false, true))
                if (item.ItemType == _rep_item_ObjectivesChart) {
                    toolbarItems.push(toolbarCheckBox(item, "showLocal", "Show Local", "Show Local Priorities", ecChartDefOptions, false));
                }
            }
        }
        return toolbarItems;
    }

    function resetChart(item) {
        var el = getElement(item);
        if ((el) && (el.length) && (el.html()!="") && (el.hasClass("widget-created"))) {
            el.ecChart("instance").destroy();
        }
    }

    function updateChartOption(item, opt_name, opt_value) {
        if ((item) && typeof item.ID != "undefined") {
            var div = getElement(item);
            if ((div) && (div.length) && (div.hasClass("widget-created"))) {
                div.ecChart("option", opt_name, opt_value).ecChart("redraw");
            }
        }
    }

    function resizeChartWidgets() {
        resizeByType(_rep_item_AlternativesChart);
        resizeByType(_rep_item_ObjectivesChart);
        resizeByType(_rep_item_Objectives);
        resizeByType(_rep_item_ObjsGrid);
        resizeByType(_rep_item_Alternatives);
        resizeByType(_rep_item_AltsGrid);
        resizeByType(_rep_item_ProsAndCons);
        resizeByType(_rep_item_Participants);
    }

    function resetChartWidgets(only_with_prty) {
        if (typeof only_with_prty == "undefined" || !(only_with_prty)) {
            resetElementsByType(_rep_item_ProsAndCons);
            resetElementsByType(_rep_item_Participants);
            resetElementsByType(_rep_item_Objectives);
        }
        resetElementsByType(_rep_item_Alternatives);
        resetElementsByType(_rep_item_AltsGrid);
        resetElementsByType(_rep_item_AlternativesChart);
        resetElementsByType(_rep_item_ObjectivesChart);
        resetElementsByType(_rep_item_ObjsGrid);
    }

    getAlternativesChart = getObjectivesChart = getChart;
    getAlternativesChartName = getObjectivesChartName = getChartName;
    resetAlternativesChart = resetObjectivesChart = resetChart;
    resizeAlternativesChart = resizeObjectivesChart = resizeChart;
    toolbarAlternativesChart = toolbarObjectivesChart = toolbarChart;
    updateAlternativesChartOption = updateObjectivesChartOption = updateChartOption;

    //// ================================== Alternatives ========================

    //function getAlternativesName(item) {
    //    return ((item) && typeof item.ID != "undefined") ? "alts" + item.ID : "";
    //}

    //function getAlternatives(item) {
    //    if ((item) && typeof item.ID != "undefined") {
    //        if (!hasData()) chartLoadData(currentDashboard().Items);
    //        return "<div id='" + getAlternativesName(item) + "' class='whole preloader_dots dx-list-compact' style='text-align:left'></div>";
    //    } else {
    //        return "";
    //    }
    //}

    //function resizeAlternatives(item, w, h) {
    //    if ((item) && hasData()) {
    //        var div = getElement(item);
    //        if ((div) && (div.length)) {
    //            if (div.html() == "") {
    //                div.removeClass("preloader_dots");
    //                var ds = JSON.parse(JSON.stringify(data_charts.data.alts));
    //                ds.shift();
    //                for (var i = 0; i < ds.length; i++) {
    //                    ds[i].filter = ds[i].visible;
    //                    if (!option_alts_filtered) ds[i].visible = 1;
    //                };
    //                div.dxList({
    //                    dataSource: ds,
    //                    displayExpr: "name",
    //                    keyExpr: "id",
    //                    selectionMode: "none",
    //                    focusStateEnabled: false,
    //                    width: w,
    //                    height: h,
    //                    pageLoadMode: 'scrollBottom',
    //                });
    //            }
    //            div.dxList("beginUpdate");
    //            div.dxList("option", "width", w);
    //            div.dxList("option", "height", h);
    //            div.dxList("endUpdate");
    //        }
    //    }
    //}

    //function resetAlternatives(item)
    //{
    //    var div = getElement(item);
    //    if ((div) && div.data("dxList")) {
    //        div.dxList("dispose");
    //    };
    //}

    // =================================== Objectives ==========================

    function getObjectivesName(item) {
        return ((item) && typeof item.ID != "undefined") ? "objs" + item.ID : "";
    }

    function getObjectives(item) {
        if ((item) && typeof item.ID != "undefined") {
            if (!hasData()) chartLoadData(currentDashboard().Items);
            return "<div id='" + getObjectivesName(item) + "' class='whole preloader_dots dx-treelist-withlines dx-treelist-compact'></div>";
        } else {
            return "";
        }
    }

    var getNodeByLevel = function(node, level) {  
        if(!node.parent) {  
            return;  
        }  
  
        if (node.parent.level === level || node.parent.level === undefined) { // Remove the "|| node.parent.level === undefined" part after release  
            return node.parent;  
        } else {  
            return getNodeByLevel(node.parent, level);  
        }  
    } 

    function resizeObjectives(item, w, h) {
        if ((item) && hasData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "") {
                    div.removeClass("preloader_dots");
                    var objectives = JSON.parse(JSON.stringify(data_charts.data.objs));
                    var cols = [{caption: "Nodes", dataField: "name"}];
                    div.dxTreeList({
                        autoExpandAll: true,
                        dataSource: objectives,
                        dataStructure: "plain",
                        focusedRowEnabled: true,
                        focusedRowKey: wrtNodeID(),
                        hoverStateEnabled: true,
                        keyExpr: "id",
                        parentIdExpr: "pid",
                        rootValue: "-1",
                        columns: cols,
                        showColumnHeaders: false,
                        selection: {
                            mode: "none",
                        },
                        width: w,
                        height: h,
<%--                        <% If Not ViewOnly Then %>onRowClick: function(e) {
                            if (_opt_wrt_sync_objectives_panel) {
                                if (typeof e.key != "undefined" && e.component.option("focusedRowEnabled") && !tvwrt_manual) {
                                    setWRTNodeID(e.key);
                                }
                            }
                        },<% End If %>--%>
                        onCellPrepared: getDxTreeListNodeConnectingLinesOnCellPrepared,
                        onRowCollapsed: function(e) {
                            e.component.closeEditCell();
                        },
                        onRowExpanded: function(e) {
                            e.component.closeEditCell();
                        },
                        editing: {
                            mode: 'cell', 
                            allowUpdating: !viewOnly,
                            allowAdding: false,
                            allowDeleting: false,
                            selectTextOnEditStart: true,
                            //startEditAction: "dblClick",
                            useIcons: true
                        },
                        //onRowDblClick: function(e) {
                        //    if (e.column.dataField == 'name') {
                        //        if (e.rowType == "data") {
                        //            e.component.editCell(e.rowIndex, e.column.dataField);
                        //        }
                        //    }
                        //},
                        onRowUpdating: function (e) {
                            callAPI("pm/hierarchy/?action=update_node_name", {"id": e.key, "name" : e.newData.name, "hid": <% =PM.ActiveHierarchy %>}, function (data) {
                                if (isValidReply(data)) {
                                    if (data.Result != _res_Success) {
                                        showResMessage(data, true);
                                        e.cancel = true;
                                    } else {
                                        //e.component.option("focusedRowKey", -1);
                                        e.component.option("focusedRowKey", e.key);
                                    }
                                }
                            }, true);
                        },
                        loadPanel: main_loadPanel_options,
                    });
                }
                div.dxTreeList("beginUpdate");
                div.dxTreeList("option", "width", w);
                div.dxTreeList("option", "height", h);
                div.dxTreeList("endUpdate");
            }
        }
    }

    function resetObjectives(item)
    {
        var div = getElement(item);
        if ((div) && div.data("dxTreeList")) {
            div.dxTreeList("dispose");
        };
    }

    
    // ================================== Participants ========================

    function usersLoadData(items, solve) {
        if (typeof data_users.loading == "undefined" || !(data_users.loading)) {
            data_users["loading"] = true;
            var params = {"evalProgress":true};
            callAPI("pm/user/?action=list", params, usersDataLoaded);
            setToolbarStatus(false);
        }
    }

    function usersDataLoaded(res) {
        if (isValidReply(res)) {
            if (res.Result == _res_Success && typeof res.Data != "undefined" && res.Data != null) {
                data_users["loading"] = false;
                setToolbarStatus(true);
                data_users["data"] = res.Data;
                resizeByType(_rep_item_Participants);
                initToolbar();
                refreshItemToolbar();
            } else {
                showResMessage(res, true);
            }
        }
    }

    function getParticipantsName(item) {
        return ((item) && typeof item.ID != "undefined") ? "users" + item.ID : "";
    }

    function getParticipants(item) {
        if ((item)) {
            if (!hasData()) chartLoadData(currentDashboard().Items);
            return "<div id='" + getParticipantsName(item) + "' class='whole preloader_dots' style='text-align:left'></div>";
        } else {
            return "";
        }
    }

    function showDateTime(element, info) {
        if (typeof info.value != "undefined" && info.value != null) {
            element.html(new Date(info.value).toLocaleString("en-US"));
        }
    };

    function resizeParticipants(item, w, h) {
        if ((item) && hasUsersData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "") {
                    div.removeClass("preloader_dots");
                    //var ds = JSON.parse(JSON.stringify(data_users.data["users"]));
                    var avtString       = 0;
                    var avtBoolean      = 1;
                    var avtLong         = 2;
                    var avtDouble       = 3;
                    var avtEnumeration  = 4;
                    var avtEnumerationMulti = 5;
                    
                    var eval_columns = [];
                    eval_columns.push({"caption": "<%=ResString("tblUserHasData")%>", "alignment": "center", "width": 85, "allowSorting": true, "allowSearch": false, "dataField" : "hasData"});
                    eval_columns.push({"caption": "<%=ResString("tblEvaluationProgress")%>", "alignment": "left", "width": 160, "dataType": "string", "dataField" : "perc",  calculateCellValue: function(row) {
                        var eval = data_users.data["evalProgress"][row.ID];
                        return ((eval) ? eval.perc.toFixed(2) + "% (" + eval.made + "/" + eval.total + ")" : "");
                    }});
                    eval_columns.push({"dataField" : "madeCount", "width": 90, "visible": false});
                    eval_columns.push({"dataField" : "totalCount", "width": 90, "visible": false});
                    eval_columns.push({"caption" : "<%=ResString("tblLastJudgmentTime")%>", "alignment" : "left", "width" : "160px", "allowSorting" : true, "allowSearch" : false, "dataField" : "lastJudgment", "dataType": "date", cellTemplate: showDateTime});

                    var columns = [ {dataField: "Email", sortIndex: 0, sortOrder: 'asc', 'fixed': true, 'minWidth': 180},
                                    {dataField: "Name", caption: "Participant Name", 'minWidth': 120},
                                    {"caption": "<%=ResString("tblUserWeight")%>", "alignment": "center", "width": 90, "format": "percent", "dataType": "number",  "allowSorting": true, "allowSearch": false, "dataField" : "priority"},
                                    {"caption" : "Evaluation Status", "columns": eval_columns}];

                    // init column headers for attributes
                    var attr_columns = [];
                    for (var i = 0; i < data_users.data["attributes"].length; i++) {
                        var a = data_users.data["attributes"][i];
                        if (!a.isDefault) {
                            var s_align = "center";
                            var s_data_type = "string";
                            switch (a.valType) {
                                case avtString:
                                    s_align = "left";
                                    s_data_type = "string";
                                    break;
                                case avtDouble:
                                case avtLong:
                                    s_align = "right";
                                    s_data_type = "number";
                                    break;
                                case avtBoolean:
                                    s_align = "center";
                                    s_data_type = "boolean";
                                    break;
                            }
                            attr_columns.push({ "caption": htmlEscape(a.name), "alignment": s_align, "dataType": s_data_type, "dataField": "v" + i, "minWidth": 90 });
                            for (var j = 0; j < data_users.data["users"].length; j++) {
                                usr = data_users.data["users"][j];
                                usr["v" + i] = usr.attrValues[a.guid];
                            }
                        }
                    };
                    if (attr_columns.length > 0) {
                        columns.push({"caption" : "Attributes", "columns": attr_columns, "allowResizing" : false});
                    };

                    var groups_columns = [];
                    for (var gid in data_users.data["groups"]) {
                        if (gid != 0) {
                            var gName = data_users.data["groups"][gid];
                            groups_columns.push({ "caption": htmlEscape(gName), "alignment": "center", "dataType": "boolean", "dataField": "g" + gid, "minWidth": 90 });
                            for (var i = 0; i < data_users.data["users"].length; i++) {
                                usr = data_users.data["users"][i];
                                usr["g" + gid] = usr.groupIDs.indexOf(gid * 1) >= 0;
                            }
                        }
                    };
                    if (groups_columns.length > 0) {
                        columns.push({"caption" : "Groups", "columns": groups_columns, "allowResizing" : false});
                    }
                    var evalProgress = data_users.data["evalProgress"];
                    for (var j = 0; j < data_users.data["users"].length; j++) {
                        usr = data_users.data["users"][j];
                        var uEval = evalProgress[usr.ID];
                        usr["lastJudgment"] = uEval.lastJudgment;
                        usr["perc"] = uEval.perc;
                        usr["madeCount"] = uEval.made;
                        usr["totalCount"] = uEval.total;
                    }

                    div.dxDataGrid({
                        dataSource: data_users.data["users"], //ds,
                        paging: {
                            enabled:false,
                        },
                        stateStoring: {
                            enabled: true,
                            type: "localStorage",
                            storageKey: "dashbrdUsers" + dashboardID + "_" + item.ID
                        },
                        columnChooser: {
                            enabled: true,
                            mode: "select", //"dragAndDrop"
                            title: resString("lblColumnChooser"),
                            //height: 400,
                            //width: 250,
                            emptyPanelText: resString("lblColumnChooserPlace")
                        },
                        stateStoring: {
                            enabled: true,
                            type: "custom",
                            customLoad: function () {
                                return (typeof item.ContentOptions != "undefined" && typeof item.ContentOptions.widgetState != "undefined" ? JSON.parse(item.ContentOptions.widgetState) : {});
                            },
                            customSave: function (state) {
                                if (div.prop("loading") != "1") {
                                    item.ContentOptions["widgetState"] = JSON.stringify(state);
                                    <% If Not ViewOnly Then %>updateDashboardItem(item, function (data) {});<% End If %>
                                }
                            }
                        },
                        onContentReady: function (e) {
                            e.element.find(".dx-toolbar").hide();
                            div.prop("loading", "1");
                            setTimeout(function () { div.prop("loading", ""); }, 1500);
                        },
                        keyExpr: "ID",
                        selection: {
                            mode: "single"
                        },
                        hoverStateEnabled: true,
                        columns: columns,
                        width: w,
                        height: h,
                        loadPanel: main_loadPanel_options,
                    });
                }
                div.dxDataGrid("beginUpdate");
                div.dxDataGrid("option", "width", w);
                div.dxDataGrid("option", "height", h);
                div.dxDataGrid("endUpdate");
            }
        } else {
            usersLoadData();
        }
    }

    function toolbarParticipants(item) {
        var toolbarItems = [];
        toolbarItems.push(toolbarButton(item, "columns", "Columns…", "Select columns", "dx-icon dx-icon-column-chooser", function (e) {
            var div = getElement(item);
            if ((div) && (div.data("dxDataGrid"))) {
                var dg = div.dxDataGrid("instance");
                dg.showColumnChooser();
            }
        }));
        toolbarItems.push(toolbarButton(item, "reset_state", "", "Reset grid settings", "fa fa-eraser", function (e) {
            var div = getElement(item);
            if ((div) && (div.data("dxDataGrid"))) {
                var dg = div.dxDataGrid("instance");
                dg.state({});
                resetElement(item);
                resizeItem(item);
            }
        }));
        return toolbarItems
    }

    function resetParticipants(item) {
        var grid = getElement(item);
        if ((grid) && grid.data("dxDataGrid")) {
            grid.dxDataGrid("dispose");
        };
    }

    // ================================ Pros & Cons ============================


    function getProsAndConsName(item) {
        return ((item) && typeof item.ID != "undefined") ? "pac" + item.ID : "";
    }

    function getProsAndCons(item) {
        if ((item) && typeof item.ID != "undefined") {
            if (!hasData()) chartLoadData(currentDashboard().Items);
            return "<div id='" + getProsAndConsName(item) + "' class='whole preloader_dots dx-list-compact' style='text-align:left'></div>";
        } else {
            return "";
        }
    }

    function resizeProsAndCons(item, w, h) {
        if ((item) && hasData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "") {
                    div.removeClass("preloader_dots");
                    var ds = [];
                    var idx = 1;
                    for (var i = 0; i < data_charts.data.prosandcons.length; i++) {
                        var pnc = data_charts.data.prosandcons[i];
                        var alt = getItemFromList(data_charts.data.alts, pnc.id);
                        var pcLength = Math.max(pnc.pros.length, pnc.cons.length);
                        for (var j = 0; j < pcLength; j++) {
                            var pacItem = {idx: idx, name: alt.name, pros: "", cons: ""};
                            pacItem.pros = (j < pnc.pros.length ? pnc.pros[j] : "");
                            pacItem.cons = (j < pnc.cons.length ? pnc.cons[j] : "");
                            ds.push(pacItem);
                            idx += 1;
                        };
                    };
                    div.dxDataGrid({
                        width: w,
                        height: h,
                        paging: {
                            enabled:false,
                        },
                        onContentReady: function(e) {  
                            e.element.find(".dx-datagrid-text-content").removeClass("dx-text-content-alignment-left");  
                        },
                        dataSource: ds,
                        columns: [{dataField: "name", caption: "Alternative", groupIndex: 0},
                                  {dataField: "pros", width: "50%",
                                  headerCellTemplate: function (header, info) {
                                      $('<div>')
                                          .html(info.column.caption)
                                          .css('text-align', 'center')
                                          .appendTo(header);
                                  }
                                  },
                                  {dataField: "cons", width: "50%",
                                  headerCellTemplate: function (header, info) {
                                      $('<div>')
                                          .html(info.column.caption)
                                          .css('text-align', 'center')
                                          .appendTo(header);
                                  }
                                  }],
                        loadPanel: main_loadPanel_options,
                    });
                } else {
                    div.dxDataGrid("beginUpdate");
                    div.dxDataGrid("option", "width", w);
                    div.dxDataGrid("option", "height", h);
                    div.dxDataGrid("endUpdate");
                };
            }
        }
    }

    function resetProsAndCons(item) {
        var grid = getElement(item);
        if ((grid) && grid.data("dxDataGrid")) {
            grid.dxDataGrid("dispose");
        };
    }

    // ================================== Alternatives Grid ========================

    const ecGridsDefOptions = {
        normalizationMode: ntUnnormalized,
        userIDs: [-1],
        wrtNodeID: -1,
    };

    function getItemFromList(list, id) {
        if (typeof list != "undefined" && (list.length)) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].id == id) return list[i];
            }
        }
        return null;
    }

    function getAltPrtsFromList(list, aid) {
        if (typeof list != "undefined" && (list.length)) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].aid == aid) return list[i];
            }
        }
        return null;
    }

    function getObjPrtsFromList(list, oid) {
        if (typeof list != "undefined" && (list.length)) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].oid == oid) return list[i];
            }
        }
        return null;
    }

    function getAltsGridName(item) {
        return ((item) && typeof item.ID != "undefined") ? "altsGrid" + item.ID : "";
    }


    function getAltsGrid(item) {
        if ((item)) {
            if (!hasData()) chartLoadData(currentDashboard().Items);
            return "<div id='" + getAltsGridName(item) + "' class='whole preloader_dots' style='text-align:left'></div>";
        } else {
            return "";
        }
    }

    function resizeAltsGrid(item, w, h) {
        if ((item) && hasData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "") {
                    var is_list = (item.ItemType == _rep_item_Alternatives);
                    div.removeClass("preloader_dots");
                    div.addClass("dx-datagrid-compact");
                    var ds = JSON.parse(JSON.stringify(data_charts.data.alts));
                    ds.shift();
                    for (var i = 0; i < ds.length; i++) {
                        ds[i].filter = ds[i].visible;
                        if (!option_alts_filtered) ds[i].visible = 1;
                    };
                    var userIDs = (typeof item.ContentOptions.userIDs !== "undefined" ?  JSON.parse(item.ContentOptions.userIDs) : [-1]);
                    var cols = [];
                    cols.push({dataField: "index",
                        caption: "Index",
                        width: 40,
                        fixed: true,
                        visible: false,
                    });
                    cols.push({dataField: "name",
                               caption: lblAlternatives,
                               cellTemplate: function (container, options) {
                                    $("<table title='" + options.value + "'><tr><td><div style='background:" + options.data.color + "; margin:2px; width:14px; height:14px;'></div></td><td>" + options.value + "</td></tr></table>").appendTo(container);
                               },
                               minWidth: 140,
                               fixed: true,
                               visible: true,
                    });
                    var colsPrty = [];
                    var colsAttr = [];
                    var prts = data_charts.data.priorities;
                    for (var j = 0; j < prts.length; j++) {
                        var p = prts[j];
                        if (userIDs.indexOf(p.uid) >= 0) {
                            var maxAltVal = 0;
                            for (var i = 0; i < ds.length; i++) {
                                var prty = getAltPrtsFromList(p.alts, ds[i].id);
                                if (prty !== null) {
                                    if (maxAltVal < prty.uprty) maxAltVal = prty.uprty;
                                };
                            };
                            if (maxAltVal == 0) maxAltVal = 1;
                            for (var i = 0; i < ds.length; i++) {
                                var prty = getAltPrtsFromList(p.alts, ds[i].id);
                                if (prty !== null) {
                                    var pval = prty.uprty;
                                    if (item.ContentOptions.normalizationMode == ntNormalizedForAll || item.ContentOptions.normalizationMode == ntNormalizedSum100) pval = prty.prty;
                                    if (item.ContentOptions.normalizationMode == ntNormalizedMul100) pval = prty.uprty / maxAltVal;
                                    ds[i]["u"+ p.uid] = pval;
                                };
                            }
                            var usr = getItemFromList(data_charts.data.users, p.uid);
                            colsPrty.push({dataField: "u" + usr.id, 
                                caption: usr.name,
                                format: {
                                    type: "percent",
                                    precision: option_decimals
                                },
                                width: 86
                            });
                        };
                    };

                    var attr_has_vis = false;
                    if (ds.length) {
                        for (var j = 0; j < ds[0].attr.length; j++) {
                            var attr = ds[0].attr[j];
                            for (var i = 0; i < ds.length; i++) {
                                var attrVal = ds[i].attr[j].val;
                                if (prty !== null) {
                                    ds[i][attr.guid] = attrVal;
                                };
                            };
                            var vis = true;
                            switch (attr.guid) {
                                case "<% =JS_SafeString(ATTRIBUTE_COST_ID.ToString) %>":
                                case "<% =JS_SafeString(ATTRIBUTE_CONTROL_COST_ID.ToString) %>":
                                case "<% =JS_SafeString(ATTRIBUTE_RISK_ID.ToString) %>":
                                    vis = false;
                            }
                            colsAttr.push({dataField: attr.guid, 
                                caption: attr.name,
                                width: 110,
                                visible: vis
                            });
                            if (vis) attr_has_vis = true;
                        }
                    }

                    if (!is_list) if (colsPrty.length > 0) cols.push({ caption: "Priorities", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "columns" : colsPrty, "visible": true });
                    if (colsAttr.length > 0) cols.push({ caption: "Attributes", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, "columns" : colsAttr, "visible": attr_has_vis && is_list});

                    div.dxDataGrid({
                        dataSource: ds,
                        //allowColumnResizing: true,
                        //columnResizingMode: "widget",
                        columnMinWidth: 80,
                        columnAutoWidth: true,
                        paging: {
                            enabled:false,
                        },
                        columnHidingEnabled: false,
                        columnChooser: {
                            enabled: true,
                            mode: "select",
                            title: resString("lblColumnChooser"),
                            emptyPanelText: resString("lblColumnChooserPlace")
                        },
                        keyExpr: "idx",
                        selection: {
                            mode: "single"
                        },
                        stateStoring: {
                            enabled: true,
                            type: "custom",
                            customLoad: function () {
                                return (typeof item.ContentOptions != "undefined" && typeof item.ContentOptions.widgetState != "undefined" ? JSON.parse(item.ContentOptions.widgetState) : {});
                            },
                            customSave: function (state) {
                                if (div.prop("loading") != "1") {
                                    item.ContentOptions["widgetState"] = JSON.stringify(state);
                                    <% If Not ViewOnly Then %>updateDashboardItem(item, function (data) {});<% End If %>
                                }
                            }
                        },
                        onContentReady: function (e) {
                            e.element.find(".dx-toolbar").hide();
                            if (option_alts_filtered) e.component.filter(['filter', '=', 1]);
                            div.prop("loading", "1");
                            setTimeout(function () { div.prop("loading", ""); }, 1500);
                        },
                        hoverStateEnabled: true,
                        columns: cols,
                        width: w,
                        height: h,
                        loadPanel: main_loadPanel_options,
                    });

                }
                div.dxDataGrid("beginUpdate");
                div.dxDataGrid("option", "width", w);
                div.dxDataGrid("option", "height", h);
                div.dxDataGrid("endUpdate");
            }
        }

    }

    function toolbarGrids(item) {
        var toolbarItems = [];
        if (hasData())
        {
            <% If Not ViewOnly Then %>if (item.ItemType != _rep_item_Alternatives) {
                var userIDs = []; 
                if (typeof item.ContentOptions.userIDs != "undefined" && item.ContentOptions.userIDs.length) userIDs = JSON.parse(item.ContentOptions.userIDs);
                if (userIDs.length==0) userIDs = [<% =COMBINED_USER_ID %>];
                toolbarItems.push(toolbarButton(item, "users", userIDs.length + "/" + users_grp_total_cnt, "Select Participants and Groups", "dx-icon fas fa-users", function (e) {
                    var allUsersAndGroups = JSON.parse(JSON.stringify(data_charts.data.users));
                    dlgSelectUsers(allUsersAndGroups, userIDs, true, function (selIDs) {
                        item.ContentOptions["userIDs"] = JSON.stringify(selIDs);
                        var has_new = false;
                        for (var i=0; i<selIDs.length; i++) {
                            var uid = selIDs[i];
                            var has_data = false;
                            for (p=0; p<data_charts.data.priorities.length; p++) {
                                if (data_charts.data.priorities[p].uid == uid) {
                                    has_data = true;
                                    break;
                                }
                            }
                            if (!has_data) {
                                has_new = true;
                                break;
                            }
                        }
                        var do_update = true;
                        var grid = getItemElement(item);
                        //if ((chart) && (chart.length) && !has_new && chart.html()!="") {
                        //    chart.ecChart("option", "userIDs", selIDs).ecChart("option", "chartsPerPage", selIDs.length).ecChart("redraw");
                        //    do_update = false;
                        //}
                        showLoadingPanel();
                        updateDashboardItem(item, function (data) {
                            refreshItemToolbar();
                            resetElement(item);
                            grid.addClass("preloader_dots").html("");
                            if (has_new) {
                                chartLoadData(currentDashboard().Items);
                            } else {
                                resizeItem(item);
                            }
                        });
                    });
                }))
            }<% End If %>
        };

        switch (item.ItemType) {
            case _rep_item_AltsGrid:
            case _rep_item_Alternatives:
<%--            var selectOptions = [{"ID": ntUnnormalized, "Text": "Unnormalized", disabled: (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%>)}, {"ID": ntNormalizedForAll, "Text": "Normalized"}, {"ID": ntNormalizedMul100, "Text": "% of Maximum"}];
                toolbarItems.push(toolbarSelectBox(item, "normalizationMode", "Normalization", selectOptions, ecGridsDefOptions, false));--%>
                toolbarItems.push(toolbarButton(item, "columns", "Columns…", "Select columns", "dx-icon dx-icon-column-chooser", function (e) {
                    var div = getElement(item);
                    if ((div) && (div.data("dxDataGrid"))) {
                        var dg = div.dxDataGrid("instance");
                        //if ($(".dx-datagrid-column-chooser").is(":visible")) dg.hideColumnChooser(); else dg.showColumnChooser();
                        dg.showColumnChooser();
                    }
                }));
                toolbarItems.push(toolbarButton(item, "reset_state", "", "Reset grid settings", "fa fa-eraser", function (e) {
                    var div = getElement(item);
                    if ((div) && (div.data("dxDataGrid"))) {
                        var dg = div.dxDataGrid("instance");
                        dg.state({});
                        resetElement(item);
                        resizeItem(item);
                    }
                }));
                break;

            case _rep_item_ObjsGrid:
                    toolbarItems.push(toolbarCheckBox(item, "showLocal", "Show Local", "Show Local Priorities", {showLocal: false}, false));
                    toolbarItems.push(toolbarButton(item, "columns", "Columns…", "Select columns", "dx-icon dx-icon-column-chooser", function (e) {
                        var div = getElement(item);
                        if ((div) && (div.data("dxTreeList"))) {
                            var dg = div.dxTreeList("instance");
                            dg.showColumnChooser();
                        }
                    }));
                    toolbarItems.push(toolbarButton(item, "reset_state", "", "Reset grid settings", "fa fa-eraser", function (e) {
                        var div = getElement(item);
                        if ((div) && (div.data("dxDataGrid"))) {
                            var dg = div.dxDataGrid("instance");
                            dg.state({});
                            resetElement(item);
                            resizeItem(item);
                        }
                    }));
                    break;
        }
        return toolbarItems
    }

    function updateAltsGridOption(item, prop_name, options) {
        resetElement(item);
        resizeItem(item);
        refreshItemToolbar();
    }

    function resetAltsGrid(item) {
        var grid = getElement(item);
        if ((grid) && grid.data("dxDataGrid")) {
            grid.dxDataGrid("dispose");
        };
    }

    toolbarAlternatives = toolbarAltsGrid = toolbarObjsGrid = toolbarGrids;
    getAlternativesName = getAltsGridName;
    getAlternatives = getAltsGrid;
    resizeAlternatives = resizeAltsGrid;
    updateAlternativesOption = updateAltsGridOption;
    resetAlternatives = resetAltsGrid;

    // ================================== Portfolio View =========================

    function raLoadData(items, solve) {
        if (typeof data_ra.loading == "undefined" || !(data_ra.loading)) {
            var itemsList = "";
            data_ra["loading"] = true;
            var params = {};
            if (typeof solve != "undefined") params["solve"] = solve;
            params["items"] = itemsList;
            callAPI("pm/ra/?action=get_portfolio_grid", params, raDataLoaded);
            setToolbarStatus(false);
        }
    }

    function raDataLoaded(res) {
        if (isValidReply(res)) {
            if (res.Result == _res_Success && typeof res.Data != "undefined" && res.Data != null) {
                data_ra["loading"] = false;
                setToolbarStatus(true);
                //data_ra["data"] = parseReply(res.Data);
                data_ra["data"] = res.Data;
                if (typeof res.ObjectID != "undefined" && res.ObjectID>=0) option_ra_scenario_id = res.ObjectID;
                resizeByType(_rep_item_PortfolioView);
                initToolbar();
                refreshItemToolbar();
            } else {
                showResMessage(res, true);
            }
        }
    }

    function toolbarPortfolioGrid(item) {
        var toolbarItems = [];
        toolbarItems.push(toolbarButton(item, "ra_solve", "Solve", "Resolve", "fas fa-calculator", function (e) {
            resetElementsByType(_rep_item_PortfolioView);
            data_ra.data = null;
            var items = dashboardItems();
            raLoadData(items, true);
        }));
        toolbarItems.push(toolbarButton(item, "columns", "Columns…", "Select columns", "dx-icon dx-icon-column-chooser", function (e) {
            var div = getElement(item);
            if ((div) && (div.data("dxDataGrid"))) {
                var dg = div.dxDataGrid("instance");
                dg.showColumnChooser();
            }
        }));
        toolbarItems.push(toolbarButton(item, "reset_state", "", "Reset grid settings", "fa fa-eraser", function (e) {
            var div = getElement(item);
            if ((div) && (div.data("dxDataGrid"))) {
                var dg = div.dxDataGrid("instance");
                dg.state({});
                item.ContentOptions["widgetState"] = "{}";
                <% If Not ViewOnly Then %>updateDashboardItem(item, function (data) {});<% End If %>
                localStorage.removeItem("porfolioSettings" + item.ID);
                resetElement(item);
                resizeItem(item);
            }
        }));
        return toolbarItems
    }

    function getPortfolioGridName(item) {
        return "portfolioGrid" + item.ID;
    }

    function getPortfolioGrid(item) {
        if ((item)) {
            if (!hasRAData()) raLoadData(currentDashboard().Items);
            return "<div id='" + getPortfolioGridName(item) + "' class='whole preloader_dots' style='text-align:left'></div>";
        } else {
            return "";
        }
    }

    function resetPortfolioGrid(item) {
        var grid = getElement(item);
        if ((grid) && grid.data("dxDataGrid")) {
            grid.dxDataGrid("dispose");
        };
    }

    function resizePortfolioGrid(item, w, h) {
        if ((item) && hasRAData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "" && data_ra.data !== null && typeof data_ra.data !== "undefined") {
                    div.removeClass("preloader_dots");
                    div.addClass("dx-datagrid-compact");
                    var ds = data_ra.data.Alternatives;
                    //ds["ConstrName"] = 
                    //{{""id"":{0},""name"":""{1}"",""funded"":{2},""benefit"":{3},""cost"":{4},""must"":{5},""mustnot"":{6}}}
                    var cols = [];
                    cols.push({dataField: "SortOrder",
                        caption: "ID",
                        width: 30,
                        fixed: true,
                        visible: true,
                    });
                    cols.push({dataField: "Name",
                               caption: "Alternative Name",
                               minWidth: 140,
                               fixed: true,
                               visible: true,
                    });
                    cols.push({dataField: "Funded",
                        caption: "Funded",
                        alignment: "center",
                        customizeText: function(cellInfo) {
                            var displayVal = "";
                            if ((cellInfo.value * 1) == 1) {displayVal = "YES"};
                            if ((cellInfo.value * 1) < 1 && (cellInfo.value * 1) > 0) {displayVal = (cellInfo.value * 100).toFixed(option_decimals) + " %"};
                            return displayVal;
                        },
                        //cellTemplate: function (container, options) {
                        //    var displayVal = "";
                        //    if ((options.value * 1) == 1) {displayVal = "YES"};
                        //    if ((options.value * 1) < 1 && (options.value * 1) > 0) {displayVal = (options.value * 100) + " %"};
                        //    $(displayVal+"").appendTo(container);
                        //},
                        visible: true,
                    });
                    cols.push({dataField: "Benefit",
                        caption: "Benefit",
                        visible: true,
                        format: {
                            type: "fixedPoint",
                            precision: option_decimals + 2,
                        },
                    });
                    cols.push({dataField: "EBenefit",
                        caption: "E.Benefit",
                        visible: false,
                        format: {
                            type: "fixedPoint",
                            precision: option_decimals + 2,
                        },
                    });
                    cols.push({dataField: "Risk",
                        caption: "Risk",
                        visible: true,
                        format: {
                            type: "fixedPoint",
                            precision: option_decimals + 2,
                        },
                    });
                    cols.push({dataField: "RiskOriginal",
                        caption: "P.Failure",
                        visible: true,
                        format: {
                            type: "fixedPoint",
                            precision: option_decimals + 2,
                        },
                    });
                    cols.push({dataField: "Cost",
                        caption: "Cost",
                        format: "currency",
                        visible: true,
                    });
                    cols.push({dataField: "Must",
                        caption: "Must",
                        visible: true,
                    });
                    cols.push({dataField: "MustNot",
                        caption: "Must Not",
                        visible: true,
                    });

                    var cc = data_ra.data.constraints;
                    for (var i = 0; i < cc.length; i++) {
                        var constr = cc[i];
                        var ccName = "cc" + constr.id;
                        cols.push({dataField: ccName,
                            caption: constr.name,
                            visible: true,
                            width: 50,
                        });
                        for (let aid in constr.values) {
                            for (var j = 0; j < ds.length; j++) {
                                if (ds[j]["ID"] == aid) {
                                    ds[j][ccName] = constr.values[aid];
                                    break;
                                } 
                            }
                        }
                    };

                    div.dxDataGrid({
                        dataSource: ds,
                        //allowColumnResizing: true,
                        //columnResizingMode: "widget",
                        columnMinWidth: 80,
                        columnAutoWidth: true,
                        paging: {
                            enabled:false,
                        },
                        columnHidingEnabled: false,
                        columnChooser: {
                            enabled: true,
                            mode: "select",
                            title: resString("lblColumnChooser"),
                            emptyPanelText: resString("lblColumnChooserPlace")
                        },
                        keyExpr: "ID",
                        selection: {
                            mode: "none"
                        },
                        stateStoring: {
                            enabled: true,
                            type: "custom",
                            storageKey: "porfolioSettings" + item.ID,
                            customLoad: function () {
                                return (typeof item.ContentOptions != "undefined" && typeof item.ContentOptions.widgetState != "undefined" ? JSON.parse(item.ContentOptions.widgetState) : {});
                            },
                            customSave: function (state) {
                                if (div.prop("loading") != "1") {
                                    item.ContentOptions["widgetState"] = JSON.stringify(state);
                                    <% If Not ViewOnly Then %>updateDashboardItem(item, function (data) {});<% End If %>
                                }
                            }
                        },
                        onContentReady: function (e) {
                            e.element.find(".dx-toolbar").hide();
                            //if (option_alts_filtered) e.component.filter(['filter', '=', 1]);
                            div.prop("loading", "1");
                            setTimeout(function () { div.prop("loading", ""); }, 1500);
                        },
                        onRowPrepared: function (e) {
                            if (typeof e.data != "undefined" && typeof e.data.Funded != "undefined") {
                                if (e.data.Funded > 0) {
                                    e.rowElement.addClass("grid_row_funded");
                                }
                            }
                            
                        },
                        summary: {
                            totalItems: [
                                {
                                    name: "fundedTitle",
                                    showInColumn: "Name",
                                    displayFormat: "Funded:",
                                    summaryType: "custom",
                                }, 
                                {
                                    name: "totalTitle",
                                    showInColumn: "Name",
                                    displayFormat: "Total:",
                                    summaryType: "custom",
                                }, 
                                {
                                    name: "fundedCount",
                                    showInColumn: "Funded",
                                    displayFormat: "{0}",
                                    summaryType: "custom",
                                },   
                                {
                                    name: "totalCount",
                                    showInColumn: "Funded",
                                    displayFormat: "{0}",
                                    valueFormat: "decimal",
                                    summaryType: "count",
                                    column: "Funded",
                                },   
                             {
                                name: "fundedBenefit",
                                showInColumn: "Benefit",
                                displayFormat: "{0}",
                                valueFormat: {
                                    type: "fixedPoint",
                                    precision: option_decimals + 2,
                                },
                                summaryType: "custom",
                            },
                            {
                                name: "totalBenefit",
                                showInColumn: "Benefit",
                                displayFormat: "{0}",
                                valueFormat: {
                                    type: "fixedPoint",
                                    precision: option_decimals + 2,
                                },
                                summaryType: "sum",
                                column: "Benefit",
                            },
                            {
                                name: "fundedeBenefit",
                                showInColumn: "EBenefit",
                                displayFormat: "{0}",
                                valueFormat: {
                                    type: "fixedPoint",
                                    precision: option_decimals + 2,
                                },
                                summaryType: "custom",
                            },
                            {
                                name: "totaleBenefit",
                                showInColumn: "EBenefit",
                                displayFormat: "{0}",
                                valueFormat: {
                                    type: "fixedPoint",
                                    precision: option_decimals + 2,
                                },
                                summaryType: "sum",
                                column: "EBenefit",
                            },
                            {
                                name: "fundedCost",
                                showInColumn: "Cost",
                                displayFormat: "{0}",
                                valueFormat: "currency",
                                summaryType: "custom",
                                //column: "cost",
                            },
                            {
                                name: "totalCost",
                                showInColumn: "Cost",
                                displayFormat: "{0}",
                                valueFormat: "currency",
                                summaryType: "sum",
                                column: "Cost",
                            },
                            ],
                            calculateCustomSummary: function (options) {
                                if (options.name === "fundedCount") {
                                    if (options.summaryProcess === "start") {
                                        options.totalValue = 0;
                                    }
                                    if (options.summaryProcess === "calculate") {
                                        if (options.value.Funded > 0) {
                                            options.totalValue = options.totalValue + 1;
                                        }
                                    }
                                };
                                if (options.name === "fundedBenefit") {
                                    if (options.summaryProcess === "start") {
                                        options.totalValue = 0;
                                    }
                                    if (options.summaryProcess === "calculate") {
                                        if (options.value.Funded > 0) {
                                            options.totalValue = options.totalValue + options.value.Benefit * options.value.Funded;
                                        }
                                    }
                                };
                                if (options.name === "fundedeBenefit") {
                                    if (options.summaryProcess === "start") {
                                        options.totalValue = 0;
                                    }
                                    if (options.summaryProcess === "calculate") {
                                        if (options.value.Funded > 0) {
                                            options.totalValue = options.totalValue + options.value.EBenefit * options.value.Funded;
                                        }
                                    }
                                };
                                if (options.name === "fundedCost") {
                                    if (options.summaryProcess === "start") {
                                        options.totalValue = 0;
                                    }
                                    if (options.summaryProcess === "calculate") {
                                        if (options.value.Funded > 0) {
                                            options.totalValue = options.totalValue + options.value.Cost * options.value.Funded;
                                        }
                                    }
                                }
                            }
                        },
                        hoverStateEnabled: true,
                        columns: cols,
                        wordWrapEnabled: true,
                        width: w,
                        height: h,
                        loadPanel: main_loadPanel_options,
                    });

                }
                div.dxDataGrid("beginUpdate");
                div.dxDataGrid("option", "width", w);
                div.dxDataGrid("option", "height", h);
                div.dxDataGrid("endUpdate");
            }
        }

    }

    function onSetRAScenario(res) {
        if (isValidReply(res)) {
            if (res.Result == _res_Success) {
                if (res.ObjectID>=0) {
                    option_ra_scenario_id = res.ObjectID;
                    resetElementsByType(_rep_item_PortfolioView);
                    data_ra.data = null;
                    var items = dashboardItems();
                    raLoadData(items);
                }
            } else {
                showResMessage(res, true);
            }
        }
    }

    // ================================== Objectives Grid ========================
    var IDX_ID = 0;
    var IDX_NODE_ID    = 0;
    var IDX_NODE_GUID  = 1;    
    var IDX_NODE_NAME  = 2;
    var IDX_NODE_PARENT_GUIDS  = 3;
    var IDX_NODE_PARENT_ID  = 4;
    var IDX_NODE_IS_TERMINAL = 5;
    var IDX_NODE_LEVEL = 6;
    var IDX_NODE_COLOR = 7;
    var IDX_NODE_IS_CATEGORY = 8;
    var lblNA = "<%=ResString("lblNA")%>";

    function getObjectiveByID(oid) {
        var result = null;
        var objs = data_charts.data.objs;
        for (var i = 0; i < objs.length; i++) {
            var obj = objs[i];
            if (obj.id == oid) {
                result = obj;
                break;
            };
        };
        return result;
    }

    function getObjsGridName(item) {
        return ((item) && typeof item.ID != "undefined") ? "objsGrid" + item.ID : "";
    }


    function getObjsGrid(item) {
        if ((item)) {
            if (!hasData()) chartLoadData(currentDashboard().Items);
            return "<div id='" + getObjsGridName(item) + "' class='whole preloader_dots dashboard_objs_grid' style='text-align:left'></div>";
        } else {
            return "";
        }
    }

    function resizeObjsGrid(item, w, h) {
        if ((item) && hasData()) {
            var div = getElement(item);
            if ((div) && (div.length)) {
                if (div.html() == "") {
                    div.removeClass("preloader_dots");
                    div.addClass("dx-treelist-withlines");
                    div.addClass("dx-treelist-compact");
                    var dsObjs = JSON.parse(JSON.stringify(data_charts.data.objs));
                    var ds = [];
                    for (var i = 0; i < dsObjs.length; i++) {
                        if (dsObjs[i].isChildNode) ds.push(dsObjs[i]);
                    };
                    var userIDs = (typeof item.ContentOptions.userIDs !== "undefined" ?  JSON.parse(item.ContentOptions.userIDs) : [-1]);

                    var cols = [];
                    cols.push({dataField: "name",
                        caption: "Objectives",
                        cellTemplate: function (container, options) {
                            $("<table title='" + options.value + "'><tr><td><div style='background:" + options.data.color + "; margin:2px; width:14px; height:14px;'></div></td><td>" + options.value + "</td></tr></table>").appendTo(container);
                        },
                        minWidth: 140,
                        fixed: true,
                    });

                    var colsPrty = [];
                    var colsAttr = [];
                    var prts = data_charts.data.priorities;
                    for (var j = 0; j < prts.length; j++) {
                        var p = prts[j];
                        if (userIDs.indexOf(p.uid) >= 0) {                 
                            for (var i = 0; i < ds.length; i++) {
                                var prty = getObjPrtsFromList(p.objs, ds[i].id);
                                if (prty !== null) {
                                    if (typeof item.ContentOptions.showLocal !== "undefined" && item.ContentOptions.showLocal) {
                                        ds[i]["u"+ p.uid] = prty.lprty;
                                    } else {
                                        ds[i]["u"+ p.uid] = (typeof item.ContentOptions.normalizationMode !== "undefined" && item.ContentOptions.normalizationMode == ntUnnormalized ? prty.uprty : prty.prty);
                                    }
                                };
                            };
                            var usr = getItemFromList(data_charts.data.users, p.uid);
                            colsPrty.push({dataField: "u" + usr.id, 
                                caption: usr.name,
                                format: {
                                    type: "percent",
                                    precision: option_decimals
                                },
                                width: 86
                            });
                        };
                    };

                    if (colsPrty.length > 0) cols.push({ caption: "Priorities", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, columns : colsPrty });
                    if (colsAttr.length > 0) cols.push({ caption: "Attributes", "alignment" : "left", "allowSorting" : false, "allowSearch" : false, columns : colsAttr });

                    div.dxTreeList({
                        autoExpandAll: true,
                        dataSource: ds,
                        dataStructure: "plain",
                        focusedRowEnabled: true,
                        focusedRowKey: wrtNodeID(),
                        hoverStateEnabled: true,
                        keyExpr: "id",
                        parentIdExpr: "pid",
                        rootValue: ds[0].pid,
                        columns: cols,
                        showColumnHeaders: true,
                        selection: {
                            mode: "none",
                        },
                        width: w,
                        height: h,
                        columnChooser: {
                            enabled: true,
                            mode: "select", //"dragAndDrop"
                            title: resString("lblColumnChooser"),
                            emptyPanelText: resString("lblColumnChooserPlace")
                        },
                        stateStoring: {
                            enabled: true,
                            type: "custom",
                            customLoad: function () {
                                return (typeof item.ContentOptions != "undefined" && typeof item.ContentOptions.widgetState != "undefined" ? JSON.parse(item.ContentOptions.widgetState) : {});
                            },
                            customSave: function (state) {
                                if (div.prop("loading") != "1") {
                                    item.ContentOptions["widgetState"] = JSON.stringify(state);
                                    <% If Not ViewOnly Then %>updateDashboardItem(item, function (data) {});<% End If %>
                                }
                            }
                        },
                        onContentReady: function (e) {
                            e.element.find(".dx-toolbar").hide();
                            div.prop("loading", "1");
                            setTimeout(function () { div.prop("loading", ""); }, 1500);
                        },
                        onCellPrepared: function(e) {  
                            if(e.rowType === "data" && e.columnIndex === 0) {  
                                var currentNode = e.row.node,  
                                    $emptySpaceElements = e.cellElement.find(".dx-treelist-empty-space"),  
                                    children = currentNode.parent.children,  
                                    isLasChildren = children[children.length - 1].key === currentNode.key;  
  
                                for(var i = 0; i < $emptySpaceElements.length; i++) {  
                                    var node = getNodeByLevel(currentNode, i-1);  
  
                                    if(node && node.children.length > 1 && currentNode.parent.key !== node.children[node.children.length - 1].key) {  
                                        $emptySpaceElements.eq(i).addClass("dx-line");  
                                    }  
  
                                    if(i === ($emptySpaceElements.length - 1)) {  
                                        $emptySpaceElements  
                                            .eq(i)  
                                            .addClass("dx-line")  
                                            .addClass("dx-line-middle")  
                                            .toggleClass("dx-line-last", isLasChildren);  
                                    }  
                                }  
                            }  
                        },
                        loadPanel: main_loadPanel_options,
                    });
                }
                div.dxTreeList("beginUpdate");
                div.dxTreeList("option", "width", w);
                div.dxTreeList("option", "height", h);
                div.dxTreeList("endUpdate");
            };
        }
    }

    function updateObjsGridOption(item, prop_name, options) {
        resetElement(item);
        resizeItem(item);
        refreshItemToolbar();
    }

    function resetObjsGrid(item) {
        var grid = getElement(item);
        if ((grid) && grid.data("dxTreeList")) {
            grid.dxTreeList("dispose");
        };
    }

    function resetGridWidgets() {
        resetElementsByType(_rep_item_ObjsGrid);
        resetElementsByType(_rep_item_AltsGrid);
        resetElementsByType(_rep_item_PortfolioView);
    }

    // ===================================  SA  ================================

    function SALoadData(item) {
        if (typeof data_sa.loading == "undefined" || !(data_sa.loading)) {
            data_sa["loading"] = true;
            var params = {"wrt": wrtNodeID()};
            if (typeof data_sa.saUserID != "undefined") params["saUserID"] = data_sa.saUserID;
            if (dashboardItemsByType(_rep_item_ASA).length>0) params["asa"] = true;
            if (dashboardItemsByType(_rep_item_Analysis2D).length>0) params["view"] = "2D";
            if (dashboardItemsByType(_rep_item_HTH).length>0) params["view"] = "HTH";
            callAPI("pm/dashboard/?action=<%=DashboardWebAPI.ACTION_DSA_INIT_DATA%>", params, SADataLoaded);
            setToolbarStatus(false);
        }
    }

    function SADataLoaded(data) {
        data_sa["loading"] = false;
        setToolbarStatus(true);
        if (typeof data == "object") {
            var old_wrt = wrtNodeID();
            data_sa["data"] = data;
            data_sa["norm"] = option_normalization;
            if (old_wrt>=0) data_sa["wrt"] = old_wrt;

            var items = dashboardItems();
            for (var i=0; i<items.length; i++) {
                var item = items[i];
                setItemNormalization(item, option_normalization, false);
            }

            updateSAWidgets();
            refreshItemToolbar();

            var btnSAUser = $("#btnSAUser");
            if ((btnSAUser) && btnSAUser.data("dxButton") && typeof data_sa.data!= "undefined" && data_sa.data != null && typeof data_sa.data.saUserID != "undefined" && typeof data_charts.data != "undefined" && data_charts.data != null && typeof data_charts.data.users != "undefined") {
                btnSAUser.dxButton({
                    disabled: false,
                });
                var usr = (data_charts.data.users.length ? getItemFromList(data_charts.data.users, data_sa.data.saUserID) : null);
                btnSAUser.dxButton({
                    "hint": "Sensitivity Analysis user/group" + ((usr) ? ": " + usr.name : ""),
                    "icon": "fas " + (data_sa.data.saUserID <0 ? "fa-user-friends" : "fa-user-check"),
                });
            }

            if (!hasData()) chartLoadData(dashboardItems());
        }
    }

    function getSensitivityName(item) {
        return ((item) && typeof item.ID != "undefined") ? "sa" + item.ID : "";
    }

    function getSensitivity(item) {
        if ((item) && typeof item.ID != "undefined") {
            if (typeof data_sa.data == "undefined" || data_sa.data == null || !(data_sa.data) || (typeof data_sa.data.ASAdata == "undefined" && item.ItemType == "ASA")) {
                SALoadData(item);
            }
            return "<canvas id='" + getSensitivityName(item) + "' class='whole preloader_dots DSACanvas'></canvas>";
        } else {
            return "";
        }
    }

    function resizeSensitivity(item, w, h) {
        if ((item) && typeof data_sa.data !== "undefined" && data_sa.data != null) {
            if (typeof data_sa.data.ASAdata !== "undefined" && item.ItemType == "ASA" || item.ItemType !== "ASA") {
                var div = getElement(item);
                if ((div) && (div.length)) {
                    if (div.hasClass("preloader_dots")) {
                        div.removeClass("preloader_dots");
                        div.width  = w;
                        div.height = h;
                        //sa_userid = data_sa.data.saUserID;
                        var options = {};
                        if (typeof item.ItemType != "undefined") {
                            switch (item.ItemType) {
                                case _rep_item_DSA:
                                case _rep_item_GSA:
                                case _rep_item_PSA:
                                case _rep_item_ASA:
                                case _rep_item_HTH:
                                    options["viewMode"] = item.ItemType;
                                    break;
                                case _rep_item_Analysis2D:
                                    options["viewMode"] = "2D";
                                    break;
                            }
                        }
                        options["titleAlternatives"] = lblAlternatives;
                        options["titleObjectives"] = lblObjectives;
                        options["isMultiView"] = true;
                        if (typeof item.ContentOptions != "undefined") options = initWidgetOptions(options, item.ContentOptions);
                        options["objs"] = data_sa.data.objs; //JSON.parse(JSON.stringify(data_sa.data.objs));
                        for (var i = 0; i < data_sa.data.alts.length; i++) {
                            var alt = data_sa.data.alts[i];
                            if (typeof alt.filter == "undefined") {
                                alt.filter = alt.visible;
                                if (!option_alts_filtered) alt.visible = 1;
                            }
                        }; 
                        options["alts"] = data_sa.data.alts; //JSON.parse(JSON.stringify(data_sa.data.alts));
                        if (item.ItemType == _rep_item_ASA && typeof data_sa.data.ASAdata != "undefined") {
                            for (var i = 0; i < data_sa.data.ASAdata.alternatives.length; i++) {
                                var alt = data_sa.data.ASAdata.alternatives[i];
                                if (typeof alt.filter == "undefined") {
                                    alt.filter = alt.visible;
                                    if (!option_alts_filtered) alt.visible = 1;
                                }
                            }; 
                            options["ASAdata"] = data_sa.data.ASAdata; //JSON.parse(JSON.stringify(data_sa.data.ASAdata));
                        };
                        options["altFilterValue"] = (option_alts_filtered ? option_alts_filter : -1);
                        options["DSAComponentsData"] = data_sa.data.comps; //JSON.parse(JSON.stringify(data_sa.data.comps));
                        options["userID"] = data_sa.data.saUserID;
                        options["userName"] = data_sa.data.saUserName;
                        options["onMouseUpEvent"] = onSAMouseUp;
                        if (option_sa_sorting >=64) options["ASASortBy"] = option_sa_sorting-64; else options["SAAltsSortBy"] = option_sa_sorting;
                        options["onSortingEvent"] = function(sortMode, sortBy) {
                            onSASorting(item, sortMode, sortBy);
                        };
                        options["onSelectedObjectiveIndexChangedEvent"] = function(IndexX, IndexY) {
                            item.ContentOptions["SelectedObjectiveIndex"] = IndexX;
                            item.ContentOptions["SelectedObjectiveIndexY"] = IndexY;
                            updateDashboardItem(item);
                        };
                        options["onHTHAltIndexChangedEvent"] = function(Index1, Index2) {
                            item.ContentOptions["HTHAlt1Index"] = Index1;
                            item.ContentOptions["HTHAlt2Index"] = Index2;
                            updateDashboardItem(item);
                        };
                        options["backgroundColor"] = (typeof item.ItemOptions.bgColor != "undefined" && item.ItemOptions.bgColor != "" ? item.ItemOptions.bgColor : _dash_def_bgcolor);
                        options["valDigits"] = option_decimals;
                        options["normalizationMode"] = getItemNormalization(item, option_normalization);
                        div.sa(options);
                    }
                    if (div.hasClass("widget-created")) div.sa("resizeCanvas", w, h);
                }
            }
        }
    }

    function toolbarSensitivity(item) {
        var toolbarItems = [];
        if ((item) && typeof data_sa.data !== "undefined" && data_sa.data != null) {
            //toolbarItems.push(toolbarButton(item, "reset", "Reset", "Reset Values", "dx-icon fas fa-undo", function (e) {
            //    var div = getElement(item);
            //    div.sa("resetSA");
            //}));
            //var selectOptions = [{"ID": "unnormalized", "Text": "Unnormalized"}, {"ID": "normAll", "Text": "Normalized for All"}, {"ID": "normSelected", "Text": "Normalized for Selected"}, {"ID": "normMax100", "Text": "% of Maximum"} ];
<%--            var selectOptions = [{"ID": "unnormalized", "Text": "Unnormalized", disabled: (option_synthMode == <%=CInt(ECSynthesisMode.smDistributive)%>)}, {"ID": "normAll", "Text": "Normalized for All"}, {"ID": "normMax100", "Text": "% of Maximum"} ];
            toolbarItems.push(toolbarSelectBox(item, "normalizationMode", "Normalization", selectOptions, ecSensitivityDefOptions, false));--%>
            var selectOptions = [{"ID": 0, "Text": "Sort None", "icon":"icon ec-sort-none"}, 
                    {"ID": 1, "Text": "Sort by Priority", "icon":"icon ec-sort-val"},
                    {"ID": 2, "Text": "Sort by Name", "icon":"icon ec-sort-az"}];
            if (item.ItemType == _rep_item_ASA)toolbarItems.push(toolbarSelectBox(item, "ASASortBy", "Objectives sorting", selectOptions, ecSensitivityDefOptions, false));
            if (item.ItemType == _rep_item_DSA) {
                toolbarItems.push(toolbarCheckBox(item, "showComponents", "Show Components", "Show Components", ecSensitivityDefOptions, false));
                toolbarItems.push(toolbarCheckBox(item, "showMarkers", "Show Markers", "<%=ResString("titleShowMarkers")%>", ecSensitivityDefOptions, true));
                toolbarItems.push(toolbarCheckBox(item, "DSAActiveSorting", "Active Sorting", "Active Sorting", ecSensitivityDefOptions, true));
            };
            if (item.ItemType == _rep_item_PSA) {
                toolbarItems.push(toolbarCheckBox(item, "PSAShowLines", "Show Lines", "Show Lines", ecSensitivityDefOptions, false));
                toolbarItems.push(toolbarCheckBox(item, "PSALineup", "Line up", "Align labels to values", ecSensitivityDefOptions, false));
                toolbarItems.push(toolbarCheckBox(item, "showRadar", "Radar View", "Radar View", ecSensitivityDefOptions, true));
            };
            toolbarItems.push(toolbarButton(item, "reset", "Reset", "Reset Values", "dx-icon fas fa-undo", function (e) {
                $(".DSACanvas.widget-created").sa("resetSA");
            }));
        };
        return toolbarItems;
    }

    function onSASorting(item, sortMode, sortBy) {
        if ((item)) {
            option_sa_sorting = (sortMode == 1 ?  sortBy : 64 + sortBy);
            <% If Not ViewOnly Then %>setPipeOption(false, "SensitivitySorting", option_sa_sorting, null);<% End if %>
        }
    }

    function onSAMouseUp(values, objids, element) {
        callAPI("pm/dashboard/?action=<%=SynthesisPage.ACTION_DSA_UPDATE_VALUES%>", {
            "values": values, 
            "objIds": objids, 
            "canvasid": element.options.canvas.id, 
            "sauserid": data_sa.data.saUserID,
            "iswrtgoal": true,
            "wrt": wrtNodeID()
        }, onSAMouseUpFinished, true)
    }

    function onSAMouseUpFinished(received_data) {
        var canvasName = received_data[1];
        $("#" + canvasName).sa("GradientMinValues", received_data[2]);
        updateSAWidgets();
    }

    function updateSensitivityOption(item, opt_name, opt_value) {
        var div = getElement(item);
        if ((div) && (div.length) && div.hasClass("widget-created")) {
            div.sa("option", opt_name, opt_value)
            if (item.ItemType == _rep_item_ASA) {
                div.sa("refreshASA");
            } else {
                div.sa("redrawSA");
            };
        }
    }

    function updateSAWidgets() {
        resizeByType(_rep_item_DSA);
        resizeByType(_rep_item_PSA);
        resizeByType(_rep_item_GSA);
        resizeByType(_rep_item_ASA);
        resizeByType(_rep_item_HTH);
        resizeByType(_rep_item_Analysis2D);
    }

    function resetSAWidgets() {
        resetElementsByType(_rep_item_DSA);
        resetElementsByType(_rep_item_PSA);
        resetElementsByType(_rep_item_GSA);
        resetElementsByType(_rep_item_ASA);
        resetElementsByType(_rep_item_HTH);
        resetElementsByType(_rep_item_Analysis2D);
    }

    function resetSA(item) {
        if ((item)) {
            var div = getElement(item);
            if ((div) && (div.length) && div.hasClass("widget-created")) div.sa("destroy");
            div.parent().html(getSensitivity(item));
        }
    }

    getDSA = getPSA = getGSA = getASA = getHTH = getAnalysis2D = getSensitivity;
    getDSAName = getPSAName = getGSAName = getASAName = getHTHName = getAnalysis2DName = getSensitivityName;
    resizeDSA =  resizePSA = resizeGSA = resizeASA = resizeHTH = resizeAnalysis2D = resizeSensitivity;
    toolbarDSA = toolbarPSA = toolbarGSA = toolbarASA = toolbarHTH = toolbarAnalysis2D = toolbarSensitivity;
    updateDSAOption = updatePSAOption = updateGSAOption = updateASAOption = updateHTHOption = updateAnalysis2DOption = updateSensitivityOption;
    resetDSA =  resetPSA = resetGSA = resetASA = resetHTH = resetAnalysis2D = resetSA;

    // ========================== Frames and Images ===========================

    function getFrameName(item) {
        return ((item) && typeof item.ID != "undefined") ? "dashbrdFrame" + item.ID : "";
    }

    function getFrame(item, url) {
        if ((item)) {
            var id = getFrameName(item);
            setTimeout(function () { 
                var frm = document.getElementById(id);
                if ((frm)) {
                    initFrameLoader(frm);
                    frm.src = url;
                }
            }, 30);
            return "<iframe id='" + id + "' frameborder='0' scrolling='auto' style='border:0px solid red;' class='whole'></iframe>";
        } else {
            return "";
        }
    }

    function resizeFrame(item, w, h) {
        //if ((item) && typeof (item.ID)!="undefined") {
        //    var frm = $("#" + getFrameName(item));
        //    if ((frm) && (frm.length)) {
        //        frm.width(w).height(h);
        //    }
        //}
    }

    function updateFrame(item) {
        if ((item)) {
            var frm = document.getElementById(getFrameName(item));
            if ((frm)) {
                initFrameLoader(frm);
                setTimeout(function () {
                    frm.contentWindow.location.reload();
                }, 100);
            }
        }
    }

    function getModelDescription(item) {
        return getFrame(item, "<% =JS_SafeString(PageURL(_PGID_EVALUATE_INFODOC, "type=" + CInt(reObjectType.Description).ToString)) %>&id=" + item.ID + "&r=" + getRandomString());
    }

    function getInfodoc(item) {
        return getFrame(item, "<% =JS_SafeString(PageURL(_PGID_EVALUATE_INFODOC, "type=" + CInt(reObjectType.DashboardInfodoc).ToString)) %>&dash=" + dashboardID + "&id=" + item.ID + "&r=" + getRandomString());
    }

    function toolbarInfodoc(item) {
        var toolbarItems = [];
        if ((item)) {
            if (!viewOnly) {
                toolbarItems.push(toolbarButton(item, "edit", "Edit...", "Edit document", "fas fa-pencil-alt", function (e) {
                    OpenRichEditorDialog("<%=JS_SafeString(String.Format("?project=&type=", App.ProjectID))%>" + (item.ItemType == _rep_item_ModelDescription ? "<% =CInt(reObjectType.Description) %>&callback=description" : "<% =CInt(reObjectType.DashboardInfodoc) %>&dash=" + dashboardID + "&callback=infodoc-"+ item.ID) + "&id=" + item.ID);
                }));
            }
            var r = toolbarButton(item, "reload", "", "Reload document", "fas fa-sync-alt", function (e) {
                updateFrame(item);
            });
            r["location"] = 'after';
            toolbarItems.push(r);
        };
        return toolbarItems;
    }

    function getPage(item) {
        return getFrame(item, urlWithParams("temptheme=sl", item.EditURL));
    }

    function getImage(item) {
        if ((item) && typeof item.ItemOptions.URL != "undefined" && item.ItemOptions.URL != "") {
            var id = item.ID;
            var url = item.ItemOptions.URL;
            return "<img src='" + url + "' class='dashbrdImage' id='dashbrdImage" + id + "' title='" + (typeof item.Name =="undefined" ? "" : item.Name) + "'>";
        } else {
            return "";
        }
    }

    function onRichEditorRefresh(empty, infodoc, callback_param) {
        if ((callback_param) && (callback_param.length)) {
            if (callback_param[0] == "description") { 
                var items = dashboardItemsByType(_rep_item_ModelDescription);
                for (var i=0; i<items.length; i++) {
                    updateFrame(items[i]);
                }
            }
            if (callback_param[0].substr(0,7) == "infodoc") {
                var id = callback_param[0].substr(8) * 1;
                var d = currentDashboard();
                if (typeof d.Items[id] != "undefined") {
                    updateFrame(d.Items[id]);
                }
            }
        }
    }

    getModelDescriptionName = getInfodocName = getPageName = getFrameName;
    resizeModelDescription = resizeInfodoc = resizePage = resizeFrame;
    updateModelDescription = updateInfodoc = updatePage = updateFrame;
    resetModelDescription = resetInfodoc = resetPage = updateFrame;
    toolbarModelDescription = toolbarInfodoc;

    // =========================== Elements ==============================

<%--    function getCounter(item) {
        var content = "";
        case <% =_PGID_STRUCTURE_ALTERNATIVES %>:
                content += getCounter("<% =PRJ.HierarchyAlternatives.Nodes.Count %>", "", "alternatives");
        case <% =_PGID_STRUCTURE_OBJECTIVES %>:
                content += getCounter("<% =PRJ.HierarchyObjectives.Nodes.Count %>", "objectives");
        case <% =_PGID_PROJECT_EVAL_PROGRESS %>:
                content += getCounter("146%", "Overall evaluation progress");
        case <% =_PGID_PROJECT_USERS  %>:
                content += getCounter("<% =PM.UsersList.Count %>", "Users: ", "(with data: <% = PM.StorageManager.Reader.DataExistsForUsersHashset().Count %>)");
        return (typeof prefix != "undefined" && prefix !="" ? "<div class='dashbrdCounterPrefix'>" + prefix + "</div>" : "") + "<div class='dashbrdCounter'>" + text + "</div>" +  (typeof postfix != "undefined" && postfix !="" ? "<div class='dashbrdCounterPostfix'>" + postfix + "</div>" : "");
    }--%>

    function getSpaceName(item) {
        return ((item) && typeof item.ID != "undefined") ? "space" + item.ID : "";    
    }

    function getSpace(item) {
        return "<div style='text-align:center; color: #d0d0d0; font-weight: bold; font-size:1.3rem' id='" + getSpaceName(item) +"'></div>";
    }

    function getNewItem(item) {
        return <% If Not ViewOnly Then %>"<div style='text-align:center; font-weight: bold; font-size:1.3rem'><div id='divNewItem" + item.ID + "'><a href='' onclick='addDashboardItem(" + item.ID + ", onEditDashboardItem); return false;'><i class='" + _rep_item_icon[_rep_item_New] + " fa-4x' title='Add new item'></i></a></div></div>";<% Else %>"";<%End If %>
    }

    function getNoContent(msg) {
        if (typeof msg == "undefined" || msg == "") msg = "<% =JS_SafeString(ResString("lblDashboardNoContent")) %>";
        return "<div style='text-align:center; color: #000033; opacity:0.15; font-size:1.33rem'>" + msg + "</div>";
    }

    // =========================== Inits =====================================

    function initIcons() {
        if ((nav_json) && (nav_json.length)) {
            for (var i = 0; i < _rep_dash_allowed.length; i++) {
                var n = _rep_dash_allowed[i];
                if (typeof _rep_item_icon[n] == "undefined" && typeof _rep_item_page[n] != "undefined") {
                    var pgid = _rep_item_page[n];
                    var pg = pageByID(nav_json, pgid);
                    if ((pg) && typeof pg.icon != "undefined" && pg.icon != "") _rep_item_icon[n] = pg.icon;
                }
            }
        }
    }

    function onKeyDn(e) {
        if (!e.ctrlKey && !e.altKey && !e.shiftKey && e.keyCode==27 && $(_dash_PreviewID).is(":visible")) {
            var id = $(_dash_PreviewID).data("item");
            if (typeof id!="undefined" && id!="") onItemToggle(id*1);
        }
    }

    function initButtons() {
        <% If Not ViewOnly Then %>$("#btnS_AddDashboard").dxButton({
            text: "<% =JS_SafeString(ResString("btnDashboardAdd")) %>",
            icon: "fas fa-plus-circle",
            elementAttr: { "class": "button_enter" },
            type: "success",
            width: "250px",
            height: "28px",
            onClick: function (e) {
                addDashboard(onEditDashboard);
                return false;
            }
        }).focus();
        $("#btnS_AddSample").dxButton({
            text: "<% =JS_SafeString(ResString("btnDashboardFromSample")) %>",
            icon: "fas fa-file-invoice",
            type: "normal",
            width: 250,
            height: 28,
            disabled: (dash_samples == null || !(Object.keys(dash_samples).length)),
            onClick: function (e) {
                addFromSamples();
            }
        });
        $("#btnS_UploadDashboard").dxButton({
            text: "<% =JS_SafeString(ResString("btnDashboardUpload")) %>",
            icon: "fas fa-upload",
            type: "normal",
            width: 250,
            height: 28,
            onClick: function (e) {
                uploadDashboard();
            }
        });
        $("#btnS_AddItem").dxButton({
            text: "Place new panel",
            icon: "fas fa-magic",
            type: "default",
            elementAttr: { "class": "button_enter" },
            width: 250,
            height: 32,
            onClick: function () {
                addDashboardItem(-1, onEditDashboardItem);
            }
        }).focus();<% End if %>
    }

    function printDashboard()
    {
        var newph = $("<div id='printPreview' class='ecDashboard'></div>");
        newph.appendTo($("#theForm"));

        var old_mode = _dash_viewmode;
        if (_dash_viewmode!=1) {
            _dash_viewmode = 1;
            updateTileItems();
            _dash_viewmode = old_mode;
            initToolbar();
        }
        _is_printing = true;
        setTimeout(function() {
            var dash = $(_dash_ElementID).find(".dx-tileview-wrapper");
            var dash_parent = dash.parent();
            dash.find(".dx-tile").css({top: "12px", left: "12px", position: "relative"});
            dash.detach().appendTo(newph);
            $("body").css({overflow: "auto"});
            $(".page-mainwrapper").hide();

            hideLoadingPanel();
            window.print();

            showLoadingPanel();
            setTimeout(function() {
                //$(".dx-tile").css({"opacity": 1});
                $(".page-mainwrapper").show();
                $("body").css({overflow: "hidden"});
                dash.detach().appendTo(dash_parent);
                $(_dash_ElementID).dxTileView("dispose");
                $(_dash_ElementID).html("");
                $("#printPreview").remove();
                _is_printing = false;
                showDashboard();
                resizeAll(true);
            }, 30);

        }, 1500);
    }

    onSwitchAdvancedMode = function () { 
        initButtons();
        refreshItemToolbar();
    };

    var scroll_offset = 0;
    var can_scroll = true;
    const scroll_dist = 12;
    const scroll_speed = 100;
    
    function scrollDashboards() {
        if ((can_scroll) && scroll_offset !=0 && $(".ecDashboard").hasClass("dragging")) {
            var div = $(_dash_ElementID);
            if (div.data("dxScrollView")) {
                var scr = div.dxScrollView("instance");
                var old = scr.scrollOffset().top;
                scr.scrollBy({top: scroll_offset, left:0 });
                if (old == scr.scrollOffset().top) {
                    can_scroll = false;
                } else {
                    setTimeout(scrollDashboards, scroll_speed);
                }
            }
        }
    }

    toggleScrollMain();
    resize_custom = onResize;
    keyup_custom = onKeyDn;

    $(document).ready(function () {
        initButtons();
        loadDashboards(onLoadDashboards);
        initIcons();
        <% If ViewOnly Then %>DevExpress.ui.notify("<% = JS_SafeString(ResString("lblProjectReadOnly")) %>", "info");<% End If %>
    });

</script>
<table border="0" cellpadding="0" cellspacing="0" class="whole" id="tblDashboardsMain">
    <tr style="height:40px;">
        <td colspan="1" id="tdHeader" style="padding-bottom:0px"><div id="toolbar" class="dxToolbar shadow" style="display:none;"></div><table border="0" cellpadding="0" cellspacing="0" style="margin:0px auto;">
            <tr valign="middle" align="center">
                <td><div class="dashboardTitle" id="dashboardTitle"></div></td>
            </tr></table><div class="dashboardComment" style="display:none;"></div></td>
    </tr>
    <tr valign="top" style="height:99%">
        <td id="tdDashboard" align="center" valign="middle" class="ecDashboard"><div id="divDashboard"></div><div id="divDashboardPreview" class="expanded dx-tile" style="display:none;"></div><div class='tdRoundBox' style='display:none; min-height:2em;' id='divNoDashboards'><h6 style='margin:1em'><nobr>No dashboards in this model</nobr></h6><% If Not ViewOnly Then %><div id='btnS_AddDashboard'></div><div id='btnS_AddSample' style='margin:1em 0px; display:block;'></div><div id='btnS_UploadDashboard'></div><% End If %></div><div class='tdRoundBox' style='display:none; min-height: 2em;' id='divNoDashboardItems'><h6 style='margin:1em'><nobr>No panels in this dashboard</nobr></h6><% If Not ViewOnly Then %><div id='btnS_AddItem'></div><% End If %></div></td>
    </tr>
</table><div id="divJumpPopup" style="display:none;"></div><% If Not ViewOnly Then %><div id="divSamples" style="display:none;"></div><% End If %>
</asp:Content>