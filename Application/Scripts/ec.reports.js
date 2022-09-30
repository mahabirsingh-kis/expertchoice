// Main library for Reports/Dashboards; | (C) AD v.211130

const _API_REPORTS = "pm/report/";

const _rep_type_doc = "Document";
const _rep_type_xls = "Spreadsheet";
const _rep_type_ppt = "Presentation";
const _rep_type_dash = "Dashboard";

const _rep_type_doc_id = 1;
const _rep_type_xls_id = 2;
const _rep_type_ppt_id = 4;
const _rep_type_dash_id = 8;

const _sess_dash_alts_flt = "DashIsAltsFiltered";

//var _rep_types_allowed = [_rep_type_doc, _rep_type_xls, _rep_type_ppt, _rep_type_dash];
var _rep_types_allowed = [_rep_type_dash];

const _rep_icon_doc = "far fa-file-word";
const _rep_icon_xls = "far fa-file-excel";
const _rep_icon_ppt = "far fa-file-powerpoint";
const _rep_icon_dash = "fas fa-tachometer-alt"; //"fa fa-border-all";

var _rep_icons = {};
_rep_icons[_rep_type_doc_id] = _rep_icons[_rep_type_doc] = _rep_icon_doc;
_rep_icons[_rep_type_xls_id] = _rep_icons[_rep_type_xls] = _rep_icon_xls;
_rep_icons[_rep_type_ppt_id] = _rep_icons[_rep_type_ppt] = _rep_icon_ppt;
_rep_icons[_rep_type_dash_id] = _rep_icons[_rep_type_dash] = _rep_icon_dash;

var _rep_types = {};
_rep_types[_rep_type_doc_id] = _rep_types[_rep_type_doc] = _rep_type_doc;
_rep_types[_rep_type_xls_id] = _rep_types[_rep_type_xls] = _rep_type_xls;
_rep_types[_rep_type_ppt_id] = _rep_types[_rep_type_ppt] = _rep_type_ppt;
_rep_types[_rep_type_dash_id] = _rep_types[_rep_type_dash] = _rep_type_dash;

// must be filled after loading the resources:
var _rep_name = {}
var _rep_names = {};
var _rep_item_name = {}

const _rep_fmt_docx = "docx";
const _rep_fmt_pdf = "pdf";
const _rep_fmt_rtf = "rtf";
const _rep_fmt_xslx = "xslx";
const _rep_fmt_pptx = "pptx";

var rep_sess_report_id = "CurReportID";
var rep_sess_dashboard_id = "CurDashboardID";
var rep_sess_dashboard_last_type = "CurDashboardLastType";

var prj_reports_list = {};

var _url_report_generator = "/Project/Report/default.aspx";
var _url_dashboards = "/Project/Analysis/Dashboard.aspx";

var curReportID = getCurReportID();
var is_new_report = false;
var _new_report_type = (_rep_types_allowed.length ? _rep_types_allowed[0] : _rep_type_doc);
    
const _rep_item_ModelDescription = "ModelDescription";
const _rep_item_Objectives = "Objectives";
const _rep_item_ObjsGrid = "ObjsGrid";
const _rep_item_Alternatives = "Alternatives";
const _rep_item_AltsGrid = "AltsGrid";
const _rep_item_Participants = "Participants";
const _rep_item_EvalProgress = "EvalProgress";
const _rep_item_DataGrid = "DataGrid";
const _rep_item_AlternativesChart = "AlternativesChart";
const _rep_item_ObjectivesChart = "ObjectivesChart";
const _rep_item_DSA = "DSA";
const _rep_item_PSA = "PSA";
const _rep_item_GSA = "GSA";
const _rep_item_Analysis2D = "Analysis2D";
const _rep_item_HTH = "HTH";
const _rep_item_ASA = "ASA";
const _rep_item_ProsAndCons = "ProsAndCons";
const _rep_item_PortfolioView = "PortfolioGrid";
const _rep_item_Counter = "Counter";
const _rep_item_Infodoc = "Infodoc";
const _rep_item_Page = "Page";
const _rep_item_PgBreak = "PageBreak";
const _rep_item_Space = "Space";
const _rep_item_New = "NewItem";

var _rep_dash_allowed = []; // will be filled on run the script
var _rep_dash_itemslist = {
                           "General": [_rep_item_New, _rep_item_Space, _rep_item_Infodoc],
                           "Define Model": [_rep_item_ModelDescription, _rep_item_Objectives, _rep_item_Alternatives, _rep_item_ProsAndCons, _rep_item_Participants],
                           "Overall results": [_rep_item_AlternativesChart, _rep_item_ObjectivesChart, _rep_item_AltsGrid, _rep_item_ObjsGrid],
                           "Sensitivities": [_rep_item_DSA, _rep_item_GSA, _rep_item_PSA, _rep_item_Analysis2D, _rep_item_HTH, _rep_item_ASA],
                           "Allocate": [_rep_item_PortfolioView],
                          };

var _rep_item_icon = [];
_rep_item_icon[_rep_item_Counter] = "fa fa-calculator";
_rep_item_icon[_rep_item_Infodoc] = "far fa-file-alt";
_rep_item_icon[_rep_item_Page] = "fa fa-atlas";
_rep_item_icon[_rep_item_Space] = "far fa-square";
_rep_item_icon[_rep_item_New] = "fas fa-plus-square";
_rep_item_icon[_rep_item_Alternatives] = "icon ec-alts";
_rep_item_icon[_rep_item_Objectives] = "icon ec-hierarchy";
_rep_item_icon[_rep_item_ProsAndCons] = "fas fa-chalkboard-teacher";
_rep_item_icon[_rep_item_Participants] = "fa fa-users";

var _rep_item_page = [];
_rep_item_page[_rep_item_ModelDescription] = 20010;   _rep_item_page[_rep_item_Objectives] = 20101;
_rep_item_page[_rep_item_Alternatives] = 20102; _rep_item_page[_rep_item_EvalProgress] = 20063;
_rep_item_page[_rep_item_DataGrid] = 50108; _rep_item_page[_rep_item_AlternativesChart] = 60221;
_rep_item_page[_rep_item_ObjectivesChart] = 60222;
_rep_item_page[_rep_item_AltsGrid] = 60210; _rep_item_page[_rep_item_ObjsGrid] = 60211;
_rep_item_page[_rep_item_PortfolioView] = 77100;
_rep_item_page[_rep_item_DSA] = 60202;
_rep_item_page[_rep_item_PSA] = 60203; _rep_item_page[_rep_item_GSA] = 60204;
_rep_item_page[_rep_item_Analysis2D] = 60207; _rep_item_page[_rep_item_ASA] = 60214;
_rep_item_page[_rep_item_HTH] = 60215; _rep_item_page[_rep_item_Participants] = 20020;
_rep_item_page[_rep_item_ProsAndCons] = 20450;

var dashboardID = getCurDashboardID();
var dashboardsList = [];

const _dash_last_remember = false;
var _dash_last_type = (_dash_last_remember ? localStorage.getItem(rep_sess_dashboard_last_type) : _rep_item_New);

var _dash_def_cols = 6;
//var _dash_def_adaptiveAspect = false;
var _dash_def_showbrd = true;
var _dash_def_loadpages = false;
var _dash_def_bgcolor = "#ffffff";
//var _dash_def_cellsPerRow = [6, 8];
var _dash_def_aspect = 0.6667;
//var _dash_def_cellsAspect = [{ "text": "Square cells", "value": false }, { "text": "Screen proportions", "value": true }];

const _def_userID = -1;
const _def_userIDs = [-1];

var _dash_Layouts = [
    {
        "rows": 6,
        "cols": 6,
        //"1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 6, "rows": 3, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 6, "rows": 3, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 6, "rows": 3, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } }
    //}, {
    //    "rows": 6,
    //    "cols": 6,
    //    "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
    //    "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
    //    "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
    //    "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
    //    "5": { "ID": 5, "Index": 5, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
    //    "6": { "ID": 6, "Index": 6, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "5": { "ID": 5, "Index": 5, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 3, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 3, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 3, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 3, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 2, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "2": { "ID": 3, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "3": { "ID": 1, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "4": { "ID": 2, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
        "5": { "ID": 3, "Index": 5, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 3, "showTitle": true } },
    }, {
        "rows": 6,
        "cols": 6,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 3, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 1, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 2, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
        "5": { "ID": 5, "Index": 5, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 3, "showTitle": true } },
    }, {
        "rows": 6,
        "cols": 8,
        //"1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 8, "rows": 4, "showTitle": true } },
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 8, "rows": 4, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 8, "rows": 4, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } }
    //}, {
    //    "rows": 6,
    //    "cols": 8,
    //    "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
    //    "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 4, "showTitle": true } },
    //    "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 4, "showTitle": true } },
    //    "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 4, "showTitle": true } },
    //    "5": { "ID": 5, "Index": 5, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } },
    //    "6": { "ID": 6, "Index": 6, "ItemType": _rep_item_New, "ItemOptions": { "cols": 2, "rows": 4, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 2, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 2, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 8, "rows": 2, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 8, "rows": 4, "showTitle": true } }
    }, {
        "rows": 6,
        "cols": 8,
        "1": { "ID": 1, "Index": 1, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 2, "showTitle": true } },
        "2": { "ID": 2, "Index": 2, "ItemType": _rep_item_New, "ItemOptions": { "cols": 5, "rows": 4, "showTitle": true } },
        "3": { "ID": 3, "Index": 3, "ItemType": _rep_item_New, "ItemOptions": { "cols": 3, "rows": 2, "showTitle": true } },
        "4": { "ID": 4, "Index": 4, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } },
        "5": { "ID": 5, "Index": 5, "ItemType": _rep_item_New, "ItemOptions": { "cols": 4, "rows": 4, "showTitle": true } }
    }];

var _dash_Layout_idx = 0;

// =================================== Manage reports on master page ====================================

function getCurReportID() {
    return localStorage.getItem(rep_sess_report_id) * 1;
}

function setCurReportID(id) {
    curReportID = id;
    localStorage.setItem(rep_sess_report_id, id);
}

function getCurDashboardID() {
    return localStorage.getItem(rep_sess_dashboard_id) * 1;
}

function setCurDashboardID(id) {
    dashboardID = id;
    localStorage.setItem(rep_sess_dashboard_id, id);
}

function isDashboard(rep_type) {
    return (rep_type == _rep_type_dash || rep_type.toLowerCase() == _rep_type_dash.toLowerCase() || rep_type == _rep_type_dash_id);
}

function isReport(rep_type) {
    return !isDashboard(rep_type);
}

function isSensitivity(itemType) {
    switch (itemType) {
        case _rep_item_DSA:
        case _rep_item_PSA:
        case _rep_item_GSA:
        case _rep_item_Analysis2D:
        case _rep_item_HTH:
        case _rep_item_ASA:
            return true;
    }
    return false;
}

function isChart(itemType) {
    switch (itemType) {
        case _rep_item_AlternativesChart:
        case _rep_item_ObjectivesChart:
            return true;
    }
    return false;
}

function isGrid(itemType) {
    switch (itemType) {
        case _rep_item_AltsGrid:
        case _rep_item_ObjsGrid:
        case _rep_item_PortfolioView:
            return true;
    }
    return false;
}

function addPage2Report() {
    if (typeof onGetReport == "function") onGetReport(onGetReportParams);
}

function getCurPageTitle() {
    var title = "";
    if ((nav_json)) {
        var pg = pageByID(nav_json, pgID);
        if ((pg)) title = pg.title;
    }
    if (title == "") title = document.title;
    return title;
}

function prepareURLWithParams(lnk) {
    var url = document.createElement('a');
    url.href = lnk;
    //var param_url = params2list(url.search);
    //var param_page = params2list(document.location.search);
    //$.each(param_page, function (idx, val) {
    //    if (typeof param_url[idx] != "undefined") {
    //        param_url[idx] = null;
    //    }
    //});
    //$.each(param_url, function (idx, val) {
    //    if (param_url[idx] != null) param_page[idx] = val;
    //});
    //var params = list2params(param_page);
    //if (params != "") params = "?" + params;
    //var lnk = url.pathname + params + url.hash;
    //return lnk;
    return url.pathname + url.search + url.hash;
}

function onGetReportParams(data) {
    if (typeof data != "undefined" && typeof data["type"] != "undefined") {
        data["edit"] = (typeof data["edit"] != "undefined" ? prepareURLWithParams(data["edit"]) : "");
        data["export"] = (typeof data["export"] != "undefined" ? prepareURLWithParams(data["export"]) : "");
        var title = (typeof data["name"] != "undefined" ? data["name"] : "");
        if (title == "") title = getCurPageTitle();
        data["name"] = replaceString("\r", "", replaceString("\n", " ", title));
        showDestinationReport(data);
    }
}

function scanReports(lst, type) {
    var res = [];
    for (let id in lst) {
        var rep = lst[id];
        if (rep.ReportType == type) res.push(rep);
    }
    return res;
}

function onChangeReportDest(show_lst) {
    var _type = getDialogReportType();
    var btn = $("#btnAddReport");
    if ((btn) && (btn.length)) {
        if (show_lst < 0) show_lst = (btn.dxButton("option", "icon").indexOf("plus") < 0);
        btn.dxButton("option", "icon", (show_lst ? "fas fa-plus" : "fas fa-list"));
        btn.dxButton("option", "hint", (show_lst ? resString("btnAddToNew") : resString("lblSeeReportsList")) + " " + _rep_name[_type]);
        btn.dxButton("instance", "blur");
    } else {
        show_lst = false;
    }
    $("#divReportsList").toggle((show_lst));
    $("#tblNewReport").toggle(!(show_lst));
    $("#divSelReportPromt").html(((show_lst) ? resString("msgTo") + " " + _rep_name[_type] : resString("lblAddToReport")) + ":");
    if (!(show_lst)) setTimeout(function () {
        $("#inpNewReport").find("input").select().focus();
    }, 150);
    is_new_report = !(show_lst);
}

function getDialogReportType() {
    var new_t = $("#divNewReportType");
    if ((new_t) && new_t.length && new_t.data("dxButtonGroup")) return new_t.dxButtonGroup("option", "selectedItemKeys")[0]; else return _new_report_type;
}

function showDestinationReport(data) {
    var max_w = dlgMaxWidth(500);

    if (curReportID * 1 <= 0) curReportID = -1;
    var has_dest = false;
    var def_id = -1;

    var reportsList = [];
    var def_names = [];
    var fmt_lst = [];
    for (var i = 0; i < _rep_types_allowed.length; i++) {
        var t = _rep_types_allowed[i];
        var items = scanReports(prj_reports_list, t);
        if ((items) && (items.length)) {
            reportsList.push({ "key": _rep_types[t], "items": items });
            if (typeof items[curReportID] != "undefined") has_dest = true;
            if (def_id < 0) def_id = items[0].ID;
        }
        def_names[t] = _rep_name[t] + " " + (items.length + 1);
        fmt_lst.push({ "hint": _rep_name[t], "icon": _rep_icons[t], "value": t });
    }

    var has_reports = (reportsList.length > 0);
    if (!has_dest) {
        if (def_id > 0) curReportID = def_id; else curReportID = -1;
        setCurReportID(curReportID);
    }

    frm = "<div style='margin-bottom:6px'><b>" + resString("lblReportItemName") + ":</b><div id='inpItemName'></div></div><b><div id='divSelReportPromt'>" + resString("msgTo") + ":</div></b><table border=0 cellspacing=0 cellpadding=0 width='100%'><tr><td><table border=0 cellspacing=0 cellpadding=0 width='100%' id='tblNewReport' style='display:none'><tr><td><div id='inpNewReport'></td><td style='padding-left:4px; width:84px;'><div id='divNewReportType'></div></td></tr></table></div><div id='divReportsList'></div></td>" + (has_reports ? "<td style='padding-left:8px; width:30px;' align='right'><div id='btnAddReport'></div></td>" : "") + "</tr></table>";

    var dlg = $("#popupContainer").dxPopup({
        width: max_w,
        height: "auto",
        title: resString("lblAddPageToReport"),
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                text: resString("btnAdd"),
                type: "default",
                elementAttr: { "class": "button_enter" },
                onClick: function () {
                    var rep_id = curReportID;
                    if (rep_id < 0 || is_new_report) {
                        data["report_type"] = getDialogReportType() ;
                        data["report_id"] = -1;
                        data["report_name"] = $("#inpNewReport").dxTextBox("option", "value");
                    } else {
                        data["report_id"] = curReportID;
                    }
                    data["pgid"] = pgID;
                    callAPI(_API_REPORTS + "?action=add_item", data, onAddPageReport);
                    closePopup();
                }
            },
            toolbar: "bottom",
            location: 'after'
        }, {
            widget: 'dxButton',
            options: {
                text: resString("btnCancel"),
                elementAttr: { "class": "button_esc" },
                onClick: function () { closePopup(); return false; }
            },
            toolbar: "bottom",
            location: 'after'
        }],
        contentTemplate: function () {
            return $(frm);
        }
    }).dxPopup("instance");
    dlg.show();

    if (has_reports) {

        $("#btnAddReport").dxButton({
            "icon": "fas fa-plus",
            onClick: function (e) {
                onChangeReportDest(-1);
            }
        });

        $("#divReportsList").dxSelectBox({
            dataSource: reportsList,
            valueExpr: "ID",
            grouped: true,
            displayExpr: "Name",
            value: curReportID,
            groupTemplate: function (data) {
                return $("<div style='background:#f5faff; color: #333355; padding:2px 0px;'><i class='" + _rep_icons[data.key] + "'></i> " + _rep_names[data.key] + ":</div>");
            },
            onValueChanged: function (e) {
                curReportID = e.value;
            }
        });
    }

    $("#inpItemName").dxTextBox({
        value: (typeof data["name"] == "undefined" || data["name"] == "" ? (typeof data["type"] == "undefined" || data["type"] == "" ? getCurPageTitle() : data["type"]) : data["name"]),
        onValueChanged: function (e) {
            data["name"] = e.value;
        }
    });
    setTimeout(function () {
        $("#inpItemName").find("input").select().focus();
    }, 250);

    $("#inpNewReport").dxTextBox({
        value: def_names[(_rep_types_allowed.length ? _rep_types_allowed[0] : _rep_type_doc)]
    });

    if ((fmt_lst.length > 1)) {
        $("#divNewReportType").dxButtonGroup({
            items: fmt_lst,
            selectionMode: "single",
            keyExpr: "value",
            selectedItemKeys: fmt_lst[0].value,
            elementAttr: { class: 'blue_selected' },
            onItemClick: function (e) {
                $("#inpNewReport").dxTextBox("option", "value", def_names[e.itemData.value]);
                $("#inpNewReport").find("input").select().focus();
            }
        });
        $("#divSelReportPromt").html(resString("msgTo") + " " + _rep_name[getDialogReportType()]) + ":";
    } else {
        $("#divNewReportType").parent().hide();
    }

    onChangeReportDest(has_reports);
    centerPopup();
}

    function onAddPageReport(res) {
        if (isValidReply(res) && res.Result == _res_Success) {
            if ((prj_reports_list) && typeof res.Data != "undefined" && typeof res.Tag != "undefined" && res.ObjectID > 0) {
                curReportID = res.ObjectID;
                setCurReportID(curReportID);
                var reps = parseReply(res.Tag);
                if ((reps)) prj_reports_list = reps;
                var rep = prj_reports_list[curReportID];
                var is_dash = ((rep) && isDashboard(rep.ReportType));
                var result = DevExpress.ui.dialog.confirm(resString(is_dash ? "msgPageAddedToDashboard" : "msgPageAddedToReport") + " " + resString(is_dash ? "confOpenDashboard" : "confOpenReport"), resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        loadURL(urlWithParams((is_dash ? "dashboard_id" : "report_id") + "=" + res.ObjectID, (is_dash ? _url_dashboards : _url_report_generator)));
                    }
                });
            } else {
                DevExpress.ui.notify(resString("msgPageAddedToReport"), 'info', 3000);
            }
        } else {
            showErrorMessage(err, true);
        }
    }



    // ======================================= Reports =======================================================




    // ======================================= Dashboards =======================================================

    function currentDashboard() {
        return (typeof dashboardsList[dashboardID] == "undefined" ? null : dashboardsList[dashboardID]);
    }

    function dashboardItems() {
        var d = currentDashboard();
        if ((d) && (typeof d.Items != "undefined")) {
            var lst = [];
            for (let id in d.Items) {
                lst.push(d.Items[id]);
            }
            return lst;
        } else {
            return [];
        }
    }

    function dashboardItemsByType(type) {
        var d = currentDashboard();
        if ((d) && (typeof d.Items != "undefined")) {
            var lst = [];
            for (let id in d.Items) {
                var item = d.Items[id];
                if (typeof item.ItemType != "undefined" && (item.ItemType == type)) {
                    lst.push(item);
                }
            }
            return lst;
        } else {
            return [];
        }
    }

    function dashboardItemByID(id) {
        var items = dashboardItems();
        if ((items)) {
            for (var i = 0; i < items.length; i++) {
                if (items[i].ID == id) return items[i];
            }
        }
        return null;
    }

    function dashboardsArray() {
        var lst = [];
        for (let id in dashboardsList) {
            var d = dashboardsList[id];
            lst.push({ "id": d.ID, "text": d.Name });
        }
        return lst;
    }

    function dashboardsItemsSort(items) {
        items.sort(function (a, b) {
            return (a.Index - b.Index);
        });
        return items;
    }

    function dashboardUpdateItemIndex(old_idx, new_idx, save_data) {
        var d = currentDashboard();
        if ((d) && old_idx != new_idx) {
            var diff = (new_idx - old_idx);
            var orders = [];
            for (let id in d.Items) {
                if (typeof d.Items[id].Index != "undefined") {
                    var idx = d.Items[id].Index;
                    if (idx == old_idx) d.Items[id].Index = new_idx;
                    if (diff > 0) {
                        if (idx > old_idx && idx <= new_idx) d.Items[id].Index -= 1;
                    }
                    if (diff < 0) {
                        if (idx >= new_idx && idx < old_idx) d.Items[id].Index += 1;
                    }
                }
                orders.push({ "ID": d.Items[id].ID, "Index": 1 * d.Items[id].Index });
            }

            // Now be sure that Indexes are started from 1, no duplicate or missing values
            orders = dashboardsItemsSort(orders);   // sort by Index
            for (var i = 0; i < orders.length; i++) {
                var o = orders[i];
                d.Items[o.ID].Index = (i + 1);          // find related element (ID) and update his Index
            }

            if ((save_data)) callAPI(_API_REPORTS + "?action=item_index", { "report_id": dashboardID, "idx_old": old_idx, "idx_new": new_idx }, function (res) { }, true);
        }
        return dashboardItems();
    }

    function addDashboard(callback) {
        dashboardProperties("add_report", "Add New Dashboard", "Add", {
            "ID": -1,
            "Name": _rep_name[_rep_type_dash_id] + " " + (Object.keys(dashboardsList).length + 1),
            "Comment": "",
        }, callback);
    }

    function editDashboard(id, callback) {
        var d = currentDashboard();
        if ((d)) {
            dashboardProperties("edit_report", "Edit Dashboard Properties", "Save", {
                "ID": d.ID,
                "Name": d.Name,
                "Comment": d.Comment,
                "Options": d.Options
            }, callback);
        }
    }

    function deleteDashboard(id, callback) {
        var d = currentDashboard();
        if ((d)) {
            var result = DevExpress.ui.dialog.confirm("Do you want to delete dashboard '" + d.Name + "'?", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    doDeleteDashboard(id, callback);
                }
            });
        }
    }

    function cloneDashboard(id, callback) {
        var d = currentDashboard();
        if ((d)) {
            dashboardProperties("clone_report", "Clone Dashboard", "Clone", {
                "ID": d.ID,
                "Name": "Copy of " + d.Name,
                "Comment": d.Comment,
                "Options": d.Options
            }, callback);
        }
    }

    function _dashboardMoveValue(from, to, name) {
        if (typeof from[name] != "undefined") {
            to[name] = from[name];
            delete from[name];
        }
    }

    function dashboardProperties(action, title, btn_title, options, callback) {
        showLoadingPanel();
        var form_data = { "ID": options.ID, "Name": options.Name, "Comment": options.Comment };
        if (typeof options.Options != "undefined") {
            form_data["cellsPerRow"] = options.Options.cellsPerRow;
            //form_data["adaptiveAspect"] = options.Options.adaptiveAspect;
            form_data["showBorders"] = !options.Options.showBorders;
            form_data["loadNativePages"] = options.Options.loadNativePages;
        }
        if (typeof form_data["cellsPerRow"] == "undefined") form_data["cellsPerRow"] = _dash_def_cols; else form_data["cellsPerRow"] *= 1;
        //if (typeof form_data["adaptiveAspect"] == "undefined") form_data["adaptiveAspect"] = false;
        if (typeof form_data["showBorders"] == "undefined") form_data["showBorders"] = !_dash_def_showbrd;
        if (typeof form_data["loadNativePages"] == "undefined") form_data["loadNativePages"] = _dash_def_loadpages;

        var add_new = (action == "add_report");

        function onSaveDashProps() {
            var form = $("#edit_frm").dxForm("instance").option("formData");
            var name = form.Name;
            if (typeof name == "undefined" || name == "") {
                DevExpress.ui.notify(resString("msgEmptyTitle"), "error");
                setTimeout(function () {
                    $("#edit_frm").dxForm("instance").getEditor("Name").focus();
                }, 500);
            } else {
                form_data.Name = name;
                form_data.Comment = form.Comment;
                if (typeof form_data.Options == "undefined") form_data["Options"] = {};
                _dashboardMoveValue(form, form_data.Options, "cellsPerRow");
                //_dashboardMoveValue(form, form_data.Options, "adaptiveAspect");
                _dashboardMoveValue(form, form_data.Options, "showBorders"); form_data.Options["showBorders"] = !form_data.Options["showBorders"];
                _dashboardMoveValue(form, form_data.Options, "loadNativePages");
                if (add_new) {
                    if ((_dash_Layouts) && (typeof _dash_Layouts[_dash_Layout_idx] != "undefined")) form_data["Layout"] = _dash_Layouts[_dash_Layout_idx];
                }
                doDashboardEdit(action, form_data, callback);
                closePopup();
            }
            return false;
        };

        $("#popupContainer").dxPopup({
            width: dlgMaxWidth(550),
            height: "auto",
            title: title,
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: (typeof btn_title == "undefined" || btn_title == "" ? resString("btnOK") : btn_title),
                    elementAttr: { "class": "button_enter", "id": "btnAddDashbrd" },
                    onClick: onSaveDashProps,
                },
                toolbar: "bottom",
                location: 'after'
            }, {
                widget: 'dxButton',
                options: {
                    text: resString("btnCancel"),
                    elementAttr: { "class": "button_esc" },
                    onClick: function () { closePopup(); return false; }
                },
                toolbar: "bottom",
                location: 'after'
            }],
            contentTemplate: function () {
                return $("<div id='edit_frm'></div>");
            }
        });
        $("#popupContainer").dxPopup("show");

        var frm = $("#edit_frm").dxForm({
            formData: form_data,
            items: [{
                //itemType: "group",
                //items: [{
                    dataField: "Name",
                    editorType: "dxTextBox",
                    isRequired: true,
                    label: { text: resString("lblTitle") + ":" }
                }, {
                    dataField: "Comment",
                    editorType: "dxTextArea",
                    label: { text: resString("lblNote") + ":" },
                    editorOptions: {}
                //}]
                //}, {
                    //itemType: "group",
                    //colCount: 5,
                    //items: [{
                    //    itemType: "group",
                    //    colSpan: 3,
                    //    items: [{
                    //        dataField: "cellsPerRow",
                    //        editorType: "dxRadioGroup",
                    //        label: { text: "Cells per row:" },
                    //        editorOptions: {
                    //            //width: 100,
                    //            items: _dash_def_cellsPerRow,
                    //            layout: "vertical",
                    //            onValueChanged: function (e) {
                    //                //dashboardUpdateCellsPreview(e.value, form_data.adaptiveAspect, form_data.showBorders, []);
                    //                dashboardUpdateCellsPreview(e.value, form_data.showBorders, []);
                    //                if (form_data.ID <= 0) dashboardPreviewShift(0);
                    //            }
                    //        }
                    //    }, {
                    //    dataField: "adaptiveAspect",
                    //    editorType: "dxRadioGroup",
                    //    label: { text: "Cells aspect:" },
                    //    editorOptions: {
                    //        displayExpr: "text",
                    //        valueExpr: "value",
                    //        items: _dash_def_cellsAspect,
                    //        layout: "vertical",
                    //        onValueChanged: function (e) {
                    //            dashboardUpdateCellsPreview(form_data.cellsPerRow, e.value, form_data.showBorders, []);
                    //        }
                    //    }
                }, {
                    dataField: "showBorders",
                    editorType: "dxCheckBox",
                    visible: !add_new,
                    label: {
                        text: " ",
                        visible: true,
                    },
                    editorOptions: {
                        text: resString("lblDasbrdShowBorders"),
                        onValueChanged: function (e) {
                            //dashboardUpdateCellsPreview(form_data.cellsPerRow, form_data.adaptiveAspect, e.value, []);
                            dashboardUpdateCellsPreview(form_data.cellsPerRow, !(e.value), []);
                        }
                    }
                    //}]
                }, {
                    dataField: "cellsPerRow",
                    //editorType: "dxCheckBox",
                    visible: add_new,
                    label: {
                        //alignment: "right",
                        text: resString("lblDasbrdShowLayout") + ":",
                        //visible: true
                    },
                    template: $("<div id='divLayouts' class='dashbrdLayout'></div>")
                //}] 
            }, {
                dataField: "loadNativePages",
                editorType: "dxCheckBox",
                visible: false,
                label: {
                    text: " ",
                    visible: true,
                },
                editorOptions: {
                    text: "Load native pages when no custom content (slow)",
                }
            }],
            readOnly: false,
            showColonAfterLabel: false,
            labelLocation: "left"
        }).dxForm("instance");

        //dashboardUpdateCellsPreview(form_data.cellsPerRow, form_data.adaptiveAspect, form_data.showBorders);
        if (add_new) {

            $("#divLayouts").dxTileView({
                items: _dash_Layouts,
                activeStateEnabled: false,
                baseItemWidth: 104,
                baseItemHeight: 104,
                itemMargin: 3,
                height: dlgMaxHeight(425)-210,
                direction: "vertical",
                showScrollbar: true,
                //itemHoldTimeout: 0,
                itemTemplate: function (itemData, itemIndex, itemElement) {
                    var l = $("<div id='layout" + itemIndex + "'></div>");
                    itemElement.append(l);
                    l.on("dblclick", function (e) {
                        var btn = $("#btnAddDashbrd");
                        if ((btn) && (btn.data("dxButton"))) {
                            onSaveDashProps();
                        }
                    })
                },
                onItemRendered: function (e) {
                    setTimeout(function () {
                        var l = e.itemElement;
                        var d = e.itemData;
                        var items = dashboardGetPreviewItems(_dash_Layouts[e.itemIndex]);
                        dashboardUpdateCellsPreview("layout" + e.itemIndex, d.cols, form_data.showBorders, items);
                    }, 30);
                },
                onItemClick: function (e) {
                    if (typeof e.itemData.cols != "undefined")  form_data["cellsPerRow"] = e.itemData.cols;
                    dashboardSetLayout(e.itemIndex);
                    e.event.preventDefault();
                    e.event.stopImmediatePropagation();
                    // please note: this event called even if you click over the any element inside the cell!
                },
            });

            //dashboardUpdateCellsPreview(_dash_Layouts[0].cols, form_data.showBorders);
            //dashboardPreviewShift(0);
        }
        onFormNarrow("#edit_frm", 600);
        centerPopup();
        hideLoadingPanel();

        setTimeout(function () {
            frm.getEditor("Name").focus();
            var inp = frm.getEditor("Name").element().find("input");
            if ((inp) && (inp.length)) inp.select();
            if (add_new) dashboardSetLayout(_dash_Layout_idx);
        }, 350);
    }

    //function dashboardPreviewLayout(can_list) {
    //    return "<table border=1 cellspacing=0 cellpadding=0><tr valign=middle align=center>" + (can_list ? "<td><i class='far ec-icon dx-icon fa-arrow-alt-circle-left disabled' style='margin-top:1em;font-size:16px;' onclick='dashboardPreviewShift(-1);' id='dashLayoutPrev'></i></td>" : "") + "<td><div style='margin-top:6px; padding:2px; background:#e0e0e0; width:104px; border-radius: 3px; text-align:center;'><div class='text small'>Layout preview</div><div id='divDashboardCellsPreview'></div></div></td>" + (can_list ? "<td><i class='far ec-icon dx-icon fa-arrow-alt-circle-right disabled' style='margin-top:1em;font-size:16px;' onclick='dashboardPreviewShift(+1);' id='dashLayoutNext'></i></td>" : "") + "</tr></table>";
//}

    function dashboardSetLayout(idx) {
        _dash_Layout_idx = idx;
        $(".dashbrdLayout").find(".dx-state-active").removeClass("dx-state-active");
        $("#layout" + idx).parent().parent().addClass("dx-state-active");
    }

    function dashboardGetPreviewItems(items) {
        var res = [];
        for (key in items) {
            var item = items[key];
            if (typeof item.ItemOptions != "undefined") res.push([item.ItemOptions.cols, item.ItemOptions.rows]);
        }
        return res;
    }

    //function dashboardPreviewShift(offset) {
    //    var obj = $("#divDashboardCellsPreview");
    //    if ((obj) && (obj.length) && obj.hasClass("widget-created")) {
    //        var cols = obj.ecDashboardGrid("option", "cols");
    //        var idx = (_dash_Layout_idx + offset);
    //        if (idx >= 0 && idx < _dash_Layouts.length) {
    //            _dash_Layout_idx = idx;
    //            var items = dashboardGetPreviewItems(_dash_Layouts[idx]);
    //            obj.ecDashboardGrid("option", {
    //                "rows": _dash_Layouts[idx].rows,
    //                "cols": _dash_Layouts[idx].cols,
    //                "items": items
    //            }).ecDashboardGrid("redraw");
    //            $("#dashLayoutPrev").toggleClass("disabled", (idx == 0));
    //            $("#dashLayoutNext").toggleClass("disabled", (idx == (_dash_Layouts.length - 1)));
    //        }
    //    }
    //}

    //function dashboardUpdateCellsPreview(cells, aspect, borders, items, mode) {
    function dashboardUpdateCellsPreview(id, cells, borders, items, on_click) {
        var obj = $("#" + id);
        if ((obj) && (obj.length)) {
            var rows = Math.ceil(_dash_def_aspect * cells);
            if (rows > cells) rows = cells;
            //if (cells < 6 && rows < 4) rows = 4;
            //if (cells > 7 && rows < 5) rows = 5;
            //var h = Math.round(_dash_def_aspect * 100);
            //var ch = Math.floor(h / rows);
            //h = cells * ch + 4;
            var is_edit = (typeof on_click == "function");
            var opt = {
                mode: (is_edit ? "edit" : "view"),
                cellMargin: 2,
                margin: 2,
                width: 100,
                //height: (aspect ? 52 : 100),
                height: 100,
                cols: cells,
                rows: cells,
                items: items,
                visibleRows: rows,
                selectedCols: (is_edit ? items[0] : -1),
                selectedRows: (is_edit ? items[1] : -1),
                selectedColor: "#fff980",
                itemColor: "#a5d490",
                backgroundColor: "#ffffff",
                cellColor: "#e0e0e0",   // (borders) ? "#666666" : "#e0e0e0",
                onCellClick: (is_edit ? on_click : null),
            };
            obj.toggleClass("clickable", (is_edit));
            var is_new = !(obj.hasClass("widget-created"));
            if (is_new) {
                setTimeout(function () { obj.ecDashboardGrid(opt); }, 100);
            } else {
                setTimeout(function () { obj.ecDashboardGrid("beginUpdate").ecDashboardGrid("option", opt).ecDashboardGrid("endUpdate"); }, 100);
            }
        }
    }

    function addDashboardItem(id, callback) {
        var d = currentDashboard();
        if ((d)) {
            var action = "add_item";
            var data = {
                "Index": -1,
                "ID": -1,
                "Name": "", //"Item " + (Object.keys(d.Items).length + 1),
                "Comment": "",
                "ItemType": _rep_item_New,
                "ItemOptions": {},
            };
            if (typeof d.Items[id] != "undefined") {
                var item = d.Items[id];
                if (typeof item.ID != "undefined") data.ID = item.ID;
                if (typeof item.Index != "undefined") data.Index = item.Index;
                if (typeof item.Name != "undefined") data.Name = item.Name;
                if (typeof item.Comment != "undefined") data.Comment = item.Comment;
                if (typeof item.ItemOptions != "undefined") data.ItemOptions = item.ItemOptions;
                if (data.ID>0) action = "edit_item";
            }
            dashboardItemProperties(action, "Add dashboard panel", "Add", data, callback);
        }
    }

    function editDashboardItem(id, callback) {
        var d = currentDashboard();
        if ((d) && typeof d.Items[id] != "undefined") {
            var item = d.Items[id];
            dashboardItemProperties("edit_item", "Edit panel properties", "Save", {
                "Index": item.Index,
                "ID": item.ID,
                "Name": item.Name,
                "Comment": item.Comment,
                "ItemType": item.ItemType,
                "ItemOptions": item.ItemOptions,
            }, callback);
        }
    }

    function deleteDashboardItem(id, callback) {
        var d = currentDashboard();
        if ((d) && typeof d.Items[id] != "undefined") {
            var result = DevExpress.ui.dialog.confirm("Do you want to delete the panel '" + d.Items[id].Name + "'?", resString("titleConfirmation"));
            result.done(function (dialogResult) {
                if (dialogResult) {
                    doDeleteDashboardItem(id, callback);
                }
            });
        }
    }

    function cloneDashboardItem(id, callback) {
        var d = currentDashboard();
        if ((d) && typeof d.Items[id] != "undefined") {
            var item = d.Items[id];
            dashboardItemProperties("clone_item", "Clone panel", "Clone", {
                "Index": -1,
                "ID": item.ID,
                "Name": "Copy of " + item.Name,
                "Comment": item.Comment,
                "ItemType": item.ItemType,
                "ItemOptions": item.ItemOptions,
            }, callback);
        }
    }

    function dashboardItemProperties(action, title, btn_title, options, callback) {
        var form_data = options;
        form_data["ItemOptions"] = (typeof options.ItemOptions != "undefined" ? JSON.parse(JSON.stringify(options.ItemOptions)) : {});
        if (typeof form_data.showTitle == "undefined") form_data["showTitle"] = (typeof options.ItemOptions == "undefined" ? true : options.ItemOptions.showTitle || true);
        if (typeof form_data.cols == "undefined") form_data["cols"] = (typeof options.ItemOptions == "undefined" ? 1 : options.ItemOptions.cols || -1);
        if (typeof form_data.rows == "undefined") form_data["rows"] = (typeof options.ItemOptions == "undefined" ? 1 : options.ItemOptions.rows || 2);
        if (typeof form_data.bgColor == "undefined") form_data["bgColor"] = (typeof options.ItemOptions == "undefined" || typeof options.ItemOptions.bgColor == "undefined" || options.ItemOptions.bgColor == "" ? _dash_def_bgcolor : options.ItemOptions.bgColor);

        var is_new = (form_data.ID <= 0 || form_data.ItemType == _rep_item_New);
        var name_update = (typeof form_data.Name == "undefined" || form_data.Name == "" || form_data.ItemType == _rep_item_New);
        if (typeof form_data.ItemType == "undefined" || form_data.ItemType == _rep_item_New) form_data["ItemType"] = _dash_last_type;
        if (name_update && typeof _rep_item_name[form_data.ItemType] != "undefined") form_data.Name = _rep_item_name[form_data.ItemType];
        var select_type = true;
        if (action == "clone_item") {
            name_update = true;
            select_type = false;
        }
        var old_bg = form_data["bgColor"];

        $("#popupContainer").dxPopup({
            width: dlgMaxWidth(750),
            height: "auto",
            title: title,
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: (typeof btn_title == "undefined" || btn_title == "" ? resString("btnOK") : btn_title),
                    elementAttr: { "class": "button_enter" },
                    disabled: false, 
                    onClick: function () {
                        var form = $("#edit_frm").dxForm("instance").option("formData");
                        var name = form.Name;
                        if (typeof name == "undefined" || name == "") {
                            DevExpress.ui.notify(resString("msgEmptyTitle"), "error");
                            setTimeout(function () {
                                $("#edit_frm").dxForm("instance").getEditor("Name").focus();
                            }, 500);
                        } else {
                            form_data.Name = name;
                            form_data.Comment = form.Comment;
                               if (typeof form_data.ItemOptions == "undefined") form_data["Options"] = {};
                            //_dashboardMoveValue(form, form_data.ItemOptions, "ItemType");
                            _dashboardMoveValue(form, form_data.ItemOptions, "showTitle");
                            _dashboardMoveValue(form, form_data.ItemOptions, "cols");
                            _dashboardMoveValue(form, form_data.ItemOptions, "rows");
                            _dashboardMoveValue(form, form_data.ItemOptions, "bgColor");
                            if (_dash_last_remember) _dash_last_type = form.ItemType;
                            localStorage.setItem(rep_sess_dashboard_last_type, _dash_last_type);
                            doDashboardItemEdit(action, form_data, function (data) {
                                if (is_new && dashboardItems().length <= 1 && typeof resize_custom_ == "function") setTimeout(resize_custom_, 200);
                                if (typeof callback == "function") callback(data);
                            });
                            closePopup();
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
                    elementAttr: { "class": "button_esc" },
                    onClick: function () {
                        if (old_bg != form_data["bgColor"]) dashboardApplyBGcolor(form_data.ID, old_bg, form_data.ItemType);
                        closePopup();
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }],
            contentTemplate: function () {
                return $("<div id='edit_frm'></div>");
            }
        });
        $("#popupContainer").dxPopup("show");

        //var w_sizes = [-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        //var h_sizes = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

        var types = [];
        var sel_grp = 0;
        var sel_idx = 0;
        for (let g in _rep_dash_itemslist) {
            var lst = _rep_dash_itemslist[g];
            var grp = [];
            for (var i = 0; i < lst.length; i++) {
                var n = lst[i];
                grp.push({ "value": n, "name": _rep_item_name[n], "icon": (typeof _rep_item_icon[n] == "undefined" ? "" : _rep_item_icon[n]) });
                if (n == form_data.ItemType) {
                    sel_grp = types.length;
                    sel_idx = grp.length-1;
                }
            }
            types.push({ "key": g, items: grp });
        }

        var indexes = [];
        var d = currentDashboard();
        if ((d)) {
            var idx_max = Object.keys(d.Items).length + (action != "edit_item" || form_data.Index<1 ? 1 : 0);
            for (var i = 1; i <= idx_max ; i++) {
                indexes.push(i);
            }
        }
        if (form_data.Index < 1) form_data.Index = indexes.length;

        var frm = $("#edit_frm").dxForm({
            formData: form_data,
            //colCount: 2,
            minColWidth: 300,
            //colCount: "auto",
            colCountByScreen: {
                lg: 2,
                md: 2,
                sm: 2,
                xs: 1
            },
            labelLocation: "top",
            items: [{
                itemType: "group",
                items: [{
                    dataField: "Index",
                    editorType: "dxSelectBox",
                    visible: false,
                    label: { text: "Index:" },
                    editorOptions: {
                        width: 125,
                        items: indexes,
                    }
                }, {
                    dataField: "ItemType",
                    editorType: "dxList",
                    isRequired: true,
                    label: { text: "Panel type:" },
                    editorOptions: {
                        disabled: (!(select_type)),
                        items: types,
                        displayExpr: "name",
                        valueExpr: "value",
                        keyExpr: "value",
                        grouped: true,
                        height: (dlgMaxHeight(600) - 295),
                        elementAttr: {
                            "class": "dx-texteditor dx-editor-outlined"
                        },
                        selectionMode: "single",
                        selectedItemKeys: [form_data.ItemType],
                        hoverStateEnabled: true,
                        focusStateEnabled: true,
                        onSelectionChanged: function (e) {
                            var frm = $("#edit_frm").dxForm("instance");
                            var old_value = (e.removedItems.length ? e.removedItems[0].value : "");
                            var new_data = (e.addedItems.length ? e.addedItems[0].value : "");
                            frm.updateData("ItemType", new_data);
                            if (name_update || _rep_item_name[old_value] == form_data["Name"]) {
                                frm.updateData("Name", _rep_item_name[new_data]);
                                frm.getEditor("Name").focus();
                                var inp = frm.getEditor("Name").element().find("input");
                                if ((inp) && (inp.length)) inp.select();
                            }
                        }
                    }
                }]
            }, {
                itemType: "group",
                items: [{
                    dataField: "Name",
                    isRequired: true,
                    editorType: "dxTextBox",
                    label: { text: resString("lblTitle") + ":" },
                }, {
                    dataField: "Comment",
                    editorType: "dxTextArea",
                    label: { text: resString("lblNote") + ":" },
                    editorOptions: {
                        height: 80,
                    }
                }, {
                    dataField: "showTitle",
                    editorType: "dxCheckBox",
                    label: {
                        text: " ",
                        visible: true,
                    },
                    editorOptions: {
                        text: "Show title/note",
                    }
                }, {
                    dataField: "cols",
                    visible: !(form_data.ID > 0 && form_data.ItemType == _rep_item_New),    //(!is_new || ((d) && (Object.keys(d.Items).length == 0))),
                    label: {
                        text: "Panel size:",
                    },
                    template: "<div id='divDashboardCellsPreview'></div>"
                    //}, {
                    //    dataField: "cols",
                    //    editorType: "dxSelectBox",
                    //    label: { text: "Columns count:" },
                    //    editorOptions: {
                    //        width: 125,
                    //        items: w_sizes,
                    //        displayExpr: function (val) {
                    //            return (val < 1 ? "The whole row" : val);
                    //        }
                    //    }
                    //}, {
                    //    dataField: "rows",
                    //    editorType: "dxSelectBox",
                    //    label: { text: "Rows count:" },
                    //    editorOptions: {
                    //        width: 125,
                    //        items: h_sizes
                    //    }
                }, {
                    dataField: "bgColor",
                    editorType: "dxColorBox",
                    label: { text: "Background:" },
                    visible: true,
                    editorOptions: {
                        width: 125,
                        onValueChanged: function (e) {
                            if (action == "edit_item") dashboardApplyBGcolor(form_data.ID, e.value, form_data.ItemType);
                            form_data.bgColor = e.value;
                        }
                    }
                }]
            }],
            onContentReady: function (e) {
                var cols = ((d) ? d.Options.cellsPerRow : 6);
                dashboardUpdateCellsPreview("divDashboardCellsPreview", cols, true, [(form_data.cols < 1 ? cols : form_data.cols), form_data.rows], function (w, h) {
                    form_data.cols = w;
                    form_data.rows = h;
                });
            },
            readOnly: false,
            showColonAfterLabel: false,
        }).dxForm("instance");

        //onFormNarrow("#edit_frm", 600);
        var cols = ((d) ? d.Options.cellsPerRow : 6);
        dashboardUpdateCellsPreview("divDashboardCellsPreview", cols, true, [(form_data.cols<1 ? cols : form_data.cols), form_data.rows], function (w, h) {
            form_data.cols = w;
            form_data.rows = h;
        });
        centerPopup();

        setTimeout(function () {
            frm.getEditor("ItemType").scrollToItem({ group: sel_grp, item: sel_idx });
            frm.getEditor("Name").focus();
            var inp = frm.getEditor("Name").element().find("input");
            if ((inp) && (inp.length)) inp.select();
        }, 350);
    }

    function dashboardApplyBGcolor(id, color, itemType) {
        if (typeof color == "undefined" || color == null || color == "") color = _dash_def_bgcolor;
        var cell = $("#itemCell" + id);
        if ((cell) && (cell.length)) {
            cell.css("backgroundColor", color);
            switch (itemType) {
                case _rep_item_AlternativesChart:
                case _rep_item_ObjectivesChart:
                    var c = cell.find("#chart" + id);
                    if ((c) && (c.length) && c.html()!="") {
                        c.ecChart("option", "backgroundColor", color);
                        c.ecChart("redraw");
                    }
                    break;
                case _rep_item_DSA:
                case _rep_item_PSA:
                case _rep_item_GSA:
                case _rep_item_ASA:
                case _rep_item_HTH:
                case _rep_item_Analysis2D:
                    var c = cell.find("#sa" + id);
                    if ((c) && (c.length) && c.html() != "") {
                        c.sa("option", "backgroundColor", color);
                        c.sa("redraw");
                    }
                    break;
            }
        }
        return color;
    }

    function dashboardsCheckDefaults() {
        for (let id in dashboardsList) {
            var d = dashboardsList[id];
            if (typeof d.Options == "undefined") d["Options"] = {};
            if (typeof d.Options.cellsPerRow == "undefined" || d.Options.cellsPerRow < 1) d.Options.cellsPerRow = _dash_def_cols;
            if (typeof d.Options.showBorders == "undefined") d.Options.showBorders = _dash_def_showbrd;
            if (typeof d.Options.loadNativePages == "undefined") d.Options.loadNativePages = _dash_def_loadpages;
            for (let itm in d.Items) {
                var item = d.Items[itm];
                if (typeof item.ItemOptions != "undefined" && typeof item.ItemType != "undefined") {
                    var def_w = -1
                    var def_h = 3;
                    var def_bg = "";
                    var def_t = true;
                    switch (item.ItemType) {
                        case _rep_item_Alternatives:
                        case _rep_item_Objectives:
                        case _rep_item_Participants:
                            //def_w = 2;
                            def_h = 3;
                            break;
                        case _rep_item_AlternativesChart:
                        case _rep_item_ObjectivesChart:
                            //def_w = 2;
                            def_h = 3;
                            if (typeof item.ContentOptions.userIDs == "undefined" || item.ContentOptions.userIDs.length == 0) item.ContentOptions.userIDs = JSON.stringify(_def_userIDs);
                            break;
                        case _rep_item_AltsGrid:
                        case _rep_item_ObjsGrid:
                        case _rep_item_PortfolioView:
                            //def_w = -1;
                            def_h = 3;
                            break; case _rep_item_DataGrid:
                            //def_w = -1;
                            def_h = 3;
                            break;
                        case _rep_item_EvalProgress:
                            //def_w = 2;
                            def_h = 3;
                            break;
                        case _rep_item_ModelDescription:
                            //def_w = 1;
                            def_h = 2;
                            //def_t = false;
                            break;
                        case _rep_item_DSA:
                        case _rep_item_PSA:
                        case _rep_item_GSA:
                        case _rep_item_Analysis2D:
                        case _rep_item_ASA:
                        case _rep_item_HTH:
                            //def_w = 2;
                            def_h = 3;
                            break;
                        case _rep_item_Counter:
                            def_w = 1;
                            def_h = 1;
                            break;
                        case _rep_item_Page:
                            //def_w = -1;
                            def_h = 3;
                            break;
                        case _rep_item_New:
                            //def_w = -1;
                            def_h = 3;
                            break;
                        case _rep_item_Space:
                            def_w = 1;
                            def_h = 1;
                            break;
                        case _rep_item_Infodoc:
                            def_w = 2;
                            def_h = 2;
                            break;
                    }
                    if (typeof item.ItemOptions.cols == "undefined" || item.ItemOptions.cols === "") item.ItemOptions.cols = def_w;
                    if (typeof item.ItemOptions.rows == "undefined" || item.ItemOptions.rows === "") item.ItemOptions.rows = def_h;
                    if (typeof item.ItemOptions.showTitle == "undefined" || item.ItemOptions.showTitle === "") item.ItemOptions.showTitle = def_t;
                    if ((typeof item.ItemOptions.bgColor == "undefined" || item.ItemOptions.bgColor === "") && def_bg != "") item.ItemOptions.bgColor = def_bg;

                    // fix pageIDs
                    if (typeof item.PageID != "undefined") {
                        switch (item.PageID) {
                            case 60223:
                                item.PageID = 60210;
                            case 60224:
                                item.PageID = 60211;
                        }
                    }

                }
            }
        }
    }

    function loadDashboards(callback) {
        showLoadingPanel();
        callAPI(_API_REPORTS + "?action=list", { "category": _rep_type_dash }, function (res) {
            onDashboardsLoaded(res, callback);
        });
    }

    function onDashboardsLoaded(res, callback) {
        var loaded = false;
        if (isValidReply(res) && res.Result == _res_Success) {
            var data = res.Data;
            if (typeof data != "undefined" && data != "") {
                var dash = parseReply(data);
                if ((dash)) {
                    dashboardsList = dash;
                    dashboardsCheckDefaults();
                    if (typeof callback == "function") callback();
                    loaded = true;
                }
            }
            if (typeof res.Message != "undefined" && res.Message != "") showResMessage(res, false);
        }
        hideLoadingPanel();
        if (!loaded) showErrorMessage("Unable to load dashboards list", true);
    }

    function doDashboardEdit(action, options, callback) {
        if (typeof options["Type"] == "undefined") options["Type"] = _rep_type_dash;
        var items = options.Items;
        if (typeof options.Items != "undefined") delete options.Items;
        callAPI(_API_REPORTS + "?action=" + action, options, function (data) {
            onDashboardEdited(data, action, callback);
        });
        options.Items = items;
    }

    function onDashboardEdited(res, action, callback) {
        if (isValidReply(res) && res.Result == _res_Success) {
            if ((dashboardsList) && res.ObjectID > 0 && typeof res.Data != "undefined") {
                var id = res.ObjectID;
                try {
                    var dash = parseReply(res.Data);
                    if ((dash)) {
                        showLoadingPanel();
                        var d = currentDashboard();
                        var changed = false;
                        var cells = 1*((d) && typeof d.Options != "undefined" && typeof d.Options.cellsPerRow != "undefined" ? d.Options.cellsPerRow : _dash_def_cols);
                        //var aspect = ((d) && typeof d.Options != "undefined" && typeof d.Options.adaptiveAspect != "undefined" ? d.Options.adaptiveAspect : _dash_def_adaptiveAspect);
                        var load = ((d) && typeof d.Options != "undefined" && typeof d.Options.loadNativePages != "undefined" ? d.Options.loadNativePages : _dash_def_loadpages);
                        if (typeof dash.Options != "undefined") {
                            //if ((typeof dash.Options.cellsPerRow != "undefined" && cells != dash.Options.cellsPerRow * 1) || (typeof dash.Options.adaptiveAspect != "undefined" && aspect != dash.Options.adaptiveAspect) || (typeof dash.Options.loadNativePages != "undefined" && load != dash.Options.loadNativePages)) {
                            if ((typeof dash.Options.cellsPerRow != "undefined" && cells != dash.Options.cellsPerRow * 1) || (typeof dash.Options.loadNativePages != "undefined" && load != dash.Options.loadNativePages)) {
                                changed = true;
                            }
                        }
                        dashboardsList[id] = dash;
                        dashboardsCheckDefaults();
                        hideLoadingPanel();
                        if (typeof callback == "function") callback(id, changed);
                    }
                }
                catch (e) {
                    DevExpress.ui.notify(e.message, 'error', 3000);
                    loadDashboards(callback);
                }
            }
        } else {
            showResMessage(res, true);
        }
    }

    function doDeleteDashboard(id, callback) {
        callAPI(_API_REPORTS + "?action=delete_report", { "id": id }, function (res) {
            onDashboardDeleted(res, callback);
        });
    }

    function onDashboardDeleted(res, callback) {
        if (isValidReply(res) && res.Result == _res_Success) {
            if ((dashboardsList) && res.ObjectID > 0) {
                delete dashboardsList[res.ObjectID];
                if (typeof callback == "function") callback(res.ObjectID);
            }
        } else {
            showResMessage(res, true);
        }
    }

    function doDashboardItemEdit(action, options, callback, silent) {
        var items = options.Items;
        if (typeof options["report_id"] == "undefined") options["report_id"] = dashboardID;
        if (typeof options.Items != "undefined") delete options.Items;
        callAPI(_API_REPORTS + "?action=" + action, options, function (data) {
            onDashboardItemEdited(data, action, callback)
        }, silent);
        if (typeof items != "undefind") options.Items = items;
    }

    function onDashboardItemEdited(res, action, callback) {
        if (isValidReply(res) && res.Result == _res_Success) {
            var d = currentDashboard();
            if ((d) && res.ObjectID > 0 && typeof res.Data != "undefined") {
                try {
                    var item = parseReply(res.Data);
                    if ((item)) {
                        var id = item.ID;
                        var old_idx = item.Index;
                        var new_idx = item.Index;
                        if (typeof d.Items[id] == "undefined") {
                            old_idx = Object.keys(d.Items).length + 1;
                        } else {
                            old_idx = d.Items[id].Index;
                        }
                        item.Index = old_idx;
                        d.Items[id] = item;
                        if (old_idx != new_idx) dashboardUpdateItemIndex(old_idx, new_idx, false);
                        dashboardsCheckDefaults();
                        if (typeof callback == "function") callback(id);
                    }
                }
                catch (e) {
                    DevExpress.ui.notify(e.message, 'error', 3000);
                    loadDashboards(callback);
                }
            }
        } else {
            showResMessage(res, true);
        }
    }

    function doDeleteDashboardItem(id, callback) {
        callAPI(_API_REPORTS + "?action=delete_item", { "report_id": dashboardID, "id": id }, function (res) {
            onDashboardItemDeleted(res, callback);
        });
    }
 
    function onDashboardItemDeleted(res, callback) {
        if (isValidReply(res) && res.Result == _res_Success) {
            var d = currentDashboard();
            if ((d) && res.ObjectID > 0) {
                dashboardUpdateItemIndex(d.Items[res.ObjectID].Index, Object.keys(d.Items).length + 1, false);
                delete d.Items[res.ObjectID];
                if (typeof callback == "function") callback(res.ObjectID);
            }
        } else {
            showResMessage(res, true);
        }
    }


    // =============================== some inits ====================================

    if (!(_rep_dash_allowed) || !(_rep_dash_allowed.length)) {
        for (let g in _rep_dash_itemslist) {
            for (var i = 0; i < _rep_dash_itemslist[g].length; i++)
                _rep_dash_allowed.push(_rep_dash_itemslist[g][i]);
        }
        if (_rep_dash_allowed.indexOf(_dash_last_type) < 0) _dash_last_type = _rep_dash_allowed[0];
    }
