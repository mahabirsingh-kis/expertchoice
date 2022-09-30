// Common functions, especially for use with mpDesktop.master and mpEmpty.master | (C) AD/DA v.220513

var main_loadPanel;
var isLoadingPanel = false;
var _loadPanelPersist = false;
var resize_custom = null;
var resize_ignore = false;
var keyup_custom = null;
var logout_before = null;
var last_cmenu_opened = null;
var shortcuts = [];
var rich_editor_url = "";
var infodoc_view_url = "";
var last_focus = null;
//var _body_overflow = "";
var please_wait = "Please wait";
var error500_redirect = false;
var logout_confirmation = 'Do you want to logout?';
var maxUploadSize = 31457280;   // 30Mb;
var show_draft = false;

var isAdvancedMode = false;
var ignoreAdvancedSwitch = false;
var reloadOnAdvanced = false;
// confirmSwitchAdvancedMode -- string/function that returns a string with confirmation _before_ switch to Advanced mode
// onSwitchAdvancedMode -- function after switch option
var isFullscreenMode = false;
var is_popup = false;

var tooltip_delay_show = 500;
var tooltip_delay_hide = 3500;
var tooltip_fade_show = 0;
var tooltip_fade_hide = 250;
var tooltip_ignore_classes = /dx-treelist-*|dx-datagrid-*|dx-header-*|no-tooltip/i;
var tooltip_ignore_ids = /TinyMCEEditor/i;
var tooltip_ignore_tags = /select/i;
var tooltip_obj = null;

var _res_None = 0;
var _res_Success = 1;
var _res_Error = 2;
var _res_Warning = 3;

var _img_loading_global = "/Images/preload.gif";
var _img_loader_preloader = new Image(); _img_loader_preloader.src = _img_loading_global;

var _API_URL = "/api/";
var _method_POST = "POST";
var _method_GET = "GET";
var _method_PUT = "PUT";
var _method_DELETE = "DELETE";
var _ajax_timeout = 5 * 60 * 1000;  // ms
var _ajax_inprogress = false;
var _ajax_error_ignore = false;

var ping_interval = 10 * 1000;      // ms
var ping_inprogress = false;
var ping_active = false;
var ping_timer = null;
var ping_hash = "";
var ping_params = null; // can be string or function
var ping_on_reply = null;
var ping_missed_counter = 0;
var ping_missed_max = 3;

var jDialog_opened = false;

var isRiskion = false;
var curPrjID = -1;
var curPrjHID = 0;
var can_create = false;
var url_on_open = "";
var url_on_open_blank = false;
var open_tt_popup = false;

var curPrjData = {};
var pgID = -1;
var curSID = "";
var is_mobile = false;
var sess_useridx = -1;
var sess_username = "";
var sess_useremail = "";
var sess_user_anon = true;

var help_uri = "";
var ko_widget2 = true;
var _ko16_p = _ko16_p || [];
var _ko19 = _ko19 || {};
var ko_init = true;
var ko_script_added = false;
var ko_script_loaded = false;
var ko_loaded_callback = null;
var ko_opened = false;
var ko_prjid = "";
var ko_base_url = "";
var help_load_attemps = 3;

var dl_token = "";
var dl_timer;

var _url_tt_status = "/Project/TeamTime/Status.aspx";
var _url_tt_pipe = "/Project/TeamTime/Default.aspx";
var _url_structuring = "/Project/TeamTime/Structuring.aspx";
var _url_download = "/Project/Details.aspx?action=download";

var _url_scripts = "/Scripts/";

var wnd_gecko = null;


var main_loadPanel_options = {
    shadingColor: "rgba(235,235,235,0.9)",  // devextreme def "rgba(240,240,240,0.8)", // css x- .dx-overlay-shader
    //        position: { of: "body" },
    visible: false,
    showIndicator: true,
    showPane: true,
    shading: true,
    //delay: 1000,
    //    message: resString("lblPleaseWait"),  ' due to late binding
    indicatorSrc: _img_loading_global,
    minHeight: 170,
    minWidth: 270,
    closeOnOutsideClick: false
};


// ============ String funcs =========

String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};


function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
}

String.prototype.replaceAll = function (find, replace) {
    var target = this;
    return target.replace(new RegExp(escapeRegExp(find), 'g'), replace);
}

//String.prototype.replaceAll = function (search, replacement) {
//    var target = this;
//    return target.replace(new RegExp(search, 'g'), replacement);
//}

function replaceString(pat, newpat, str) {
    var repl = 0;
    str += "";
    while (repl < 100 && str.indexOf(pat) >= 0) { str = str.replace(pat, newpat); repl++; }
    return str;
}

function capitalizeString(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

function getRandomString() {
    return (0 | Math.random() * 1000000000).toString(36);
}

function getRandomHex() {
    return (0 | Math.random() * 1000000000).toString(16);
}

function parseTemplates(res) {
    if (res.indexOf("%%") >= 0 && (typeof _templates != "undefined")) {
        var res_lc = res.toLowerCase();
        var keys = Object.keys(_templates);
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            var tpl = "%%" + key + "%%";
            if (res_lc.indexOf(tpl) >= 0) {
                res = replaceString("%%" + capitalizeString(key) + "%%", capitalizeString(_templates[key]), res);
                res = replaceString(tpl, _templates[key], res);
            }
        }
    }
    return res;
}

function resString(name) {
    var res = "";
    if (typeof _resources != "undefined") {
        var res = _resources[name.toLowerCase()];
        if (typeof res != "undefined") {
            res = parseTemplates(res);
        } else {
            res = "(!) Resource with name '" + name + "' not found!";
        }
    } else {
        res = "(!) No resources loaded!";
    }
    return res;
}

function getPeriodString(dt, dt_now) {
    if ((dt)) {
        if (typeof dt_now == "undefined" || dt_now == null) dt_now = new Date();
        var diff = Math.round((dt_now - dt) / 1000);
        if (diff < 60) {
            return resString("lblPeriodMoment")
        } else {
            if (diff < 3600) {
                return resString("lblPeriodMinutes").f(Math.ceil(diff / 60));
            } else {
                if (diff < 24 * 3600) {
                    return resString("lblPeriodHours").f(Math.ceil(diff / 3600));
                } else {
                    if (diff < 60 * 24 * 3600) {
                        return resString("lblPeriodDays").f(Math.ceil(diff / 86400));
                    } else {
                        return dt.toISOString().substring(0, 10); //toLocaleDateString();
                    }
                }
            }
        }
    }
    return "";
}

// ====== Inits =======

function initStringsFromResourcses() {
    please_wait = resString("lblPleaseWait");
    logout_confirmation = resString("msgConfirmLogOut");
    main_loadPanel_options.message = please_wait;
    if (typeof _rep_name != "undefined") {
        var _rep_prefix = "titleReport";
        _rep_name[_rep_type_doc_id] = _rep_name[_rep_type_doc] = resString(_rep_prefix + _rep_type_doc);
        _rep_name[_rep_type_xls_id] = _rep_name[_rep_type_xls] = resString(_rep_prefix + _rep_type_xls);
        _rep_name[_rep_type_ppt_id] = _rep_name[_rep_type_ppt] = resString(_rep_prefix + _rep_type_ppt);
        _rep_name[_rep_type_dash_id] = _rep_name[_rep_type_dash] = resString(_rep_prefix + _rep_type_dash);
        _rep_names[_rep_type_doc_id] = _rep_names[_rep_type_doc] = resString(_rep_prefix + _rep_type_doc + "s");
        _rep_names[_rep_type_xls_id] = _rep_names[_rep_type_xls] = resString(_rep_prefix + _rep_type_xls + "s");
        _rep_names[_rep_type_ppt_id] = _rep_names[_rep_type_ppt] = resString(_rep_prefix + _rep_type_ppt + "s");
        _rep_names[_rep_type_dash_id] = _rep_names[_rep_type_dash] = resString(_rep_prefix + _rep_type_dash + "s");
        for (var i = 0; i < _rep_dash_allowed.length; i++) {
            var n = _rep_dash_allowed[i];
            if (typeof _rep_item_name[n] == "undefined") _rep_item_name[n] = resString("lblReportItem" + n);
        }
    }
}

function initLoadingPanel() {
    initStringsFromResourcses();
    if ($(".loadpanel").length) {
        $(".loadpanel").dxLoadPanel(main_loadPanel_options);
        if ($(".loadpanel").data("dxLoadPanel")) main_loadPanel = $(".loadpanel").dxLoadPanel("instance");
    }
    initDXDefaults();
}

function initTooltips() {
    initTooltipItems("[title]");
}

function hideTooltips() {
    if ((tooltip_obj) && (tooltip_obj != null)) tooltip_obj.hide();
}

function initTooltipItems(tooltip_items) {  // tooltip_items could be list of objects or just a "a", "#id", etc.
    $(document).tooltip({
        items: tooltip_items,
        show: {
            delay: tooltip_delay_show,
            effect: "fade",
            duration: tooltip_fade_show,
        },
        hide: {
            delay: 0,
            effect: "fade",
            duration: tooltip_fade_hide,
        },
        position: { my: "left+4 top+16", at: "left bottom", collision: "flipfit" },
        tooltipClass: "custom_tooltip",
        track: true,
        content: function () {
            var element = $(this);
            if ((element) && (element.length)) {
                var txt = element.attr("title");
                var restricted_class = (typeof element.attr('class') != "undefined" && typeof tooltip_ignore_classes != "undefined" && element.attr('class').match(tooltip_ignore_classes));
                var restricted_id = (typeof element.attr('id') != "undefined" && typeof tooltip_ignore_ids != "undefined" && element.attr('id').match(tooltip_ignore_ids));
                var restricted_tag = (typeof element[0].tagName != "undefined" && typeof tooltip_ignore_tags != "undefined" && element[0].tagName.match(tooltip_ignore_tags));
                if ((restricted_class) || (restricted_id) || (restricted_tag)) {
                    txt = "";
                }
                return txt;
            }
        },
        open: function (event, ui) {
            if ((ui) && (ui.tooltip)) {
                tooltip_obj = ui.tooltip;
                if (tooltip_delay_hide) ui.tooltip.delay(tooltip_delay_hide).hide("fade", tooltip_fade_hide);
            }
        }
    });
}

function initLoadingPanelImage() {
    $(".dx-loadindicator-wrapper").css("background-image", $(".dx-loadindicator-wrapper").css("background-image"));
}

function compareLangs(a, b) {
    if (a.title < b.title)
        return -1;
    if (a.title > b.title)
        return 1;
    return 0;
}

function initLanguages(div_id, target_id, lang_list, lang_active) {
    if (typeof (lang_list) != "undefined" && lang_list.length > 0 && typeof lang_active != "undefined" && lang_active != "") {
        $("#" + div_id).dxTooltip({
            target: $("#" + target_id),
            showEvent: 'dxclick',
            //hideEvent: 'dxclick',
            //position: { my: 'center bottom', at: 'center top', collision: 'fit flip' },
            closeOnBackButton: true,
            closeOnOutsideClick: true,
            //animation: {
            //    show: { type: "fade", from: 0, to: 1 },
            //    //show: { type: "slide", from: { opacity: 1, left: -100 }, to: { left: 0 } },
            //    hide: { type: "fade", from: 1, to: 0 }
            //},
            contentTemplate: function (contentElement) {
                var sLangs = "";
                lang_list.sort(compareLangs);
                for (var i = 0; i < lang_list.length; i++) {
                    var l = lang_list[i];
                    var n = (l.code == lang_active ? "<b>" + l.title + "</b>" : "<a href='?language=" + encodeURIComponent(l.code) + "' class='actions dashed' onclick=\"switchLanguage(\'" + encodeURIComponent(l.code) + "\'); return false;\">" + l.title + "</a>");
                    sLangs += "<div class='lang'><nobr>&#149;&nbsp;" + n + "</nobr></div>";
                }
                contentElement.append(
                    $("<p style='text-align:left' id='pLangs'></p>").html(sLangs)
                )
            }
        });
    } else {
        $("#" + target_id).hide();
    }
}

function urlWithParams(params, url) {
    if (typeof url == "undefined") url = document.location.href;
    if (typeof params != "undefined") url += (url.indexOf("?") >= 0 ? "&" : "?") + params;
    return url;
}

function switchLanguage(lng) {
    //loadURL("?language=" + lng);
    callAPI("account/?action=language", {"value": lng}, reloadPage, true);
    return false;
}

function initShortcuts(div_id, target_id) {
    if (typeof shortcuts != "undefined" && shortcuts.length > 0) {
        shortcuts = shortcuts.sort(function (a, b) { return a.code.localeCompare(b.code); });
        $("#" + div_id).dxTooltip({
            target: "#" + target_id,
            showEvent: 'dxclick',
            //hideEvent: 'dxclick',
            //position: 'top',
            closeOnBackButton: true,
            closeOnOutsideClick: true,
            animation: {
                show: { type: "fade", from: 0, to: 1 },
                //show: { type: "slide", from: { opacity: 1, left: -100 }, to: { left: 0 } },
                hide: { type: "fade", from: 1, to: 0 }
            },
            contentTemplate: function (contentElement) {
                var sLst = "";
                for (var i = 0; i < shortcuts.length; i++) {
                    var n = shortcuts[i].code;
                    n = "<div class='key'>" + n.split("+").join("</div><div class='key'>") + "</div>";
                    var pgid = shortcuts[i].pgid;
                    var js = shortcuts[i].js;
                    var t = shortcuts[i].title;
                    if (typeof pgid != "undefined" && (pgid > 0)) t = "<a href='" + (typeof shortcuts[i].url != "undefined" && shortcuts[i].url != "" ? shortcuts[i].url : "") + "' onclick='onOpenPage(" + pgid + "); return false;' class='actions aslink'>" + t + "</a>";
                    if (typeof js != "undefined" && (js != "")) t = "<a href='' onclick='" + js + "; return false;' class='actions aslink'>" + t + "</a>";
                    if (shortcuts[i].code == "-" && shortcuts[i].title == "-") {
                        sLst += "<hr>";
                    } else {
                        sLst += "<div class='shortcut'><nobr>" + n + " &ndash; " + t + "</nobr></div>";
                    }
                }
                contentElement.append(
                    $("<p style='text-align:left' id='pLangs'></p>").html(sLst)
                )
            }
        });
    } else {
        $("#" + target_id).hide();
    }
}

function onShowIFrame(url, title, w, h, position, btn, onLoadFrame) {
    w = w || Math.round($(window).width() - 64);
    h = h || Math.round($(window).height() - 64);
    var buttons = [];
    if (typeof btn != "undefined" && h > 200) {
        if (typeof btn == "string") {
            buttons = [{
                widget: 'dxButton',
                options: {
                    text: btn,
                    onClick: function () {
                        closePopup();
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'center'
            }];
        } else {
            buttons = btn;
        }
    }

    var popup = $("#popupContainer").dxPopup({
        width: w,
        height: h,
        contentTemplate: function () {
            var frame = $("<iframe src='' allowfullscreen='allowfullscreen' mozallowfullscreen='mozallowfullscreen' msallowfullscreen='msallowfullscreen' oallowfullscreen='oallowfullscreen' webkitallowfullscreen='webkitallowfullscreen' frameborder='0' id='frmInnerFrame' style='border:0px; padding:0px;' class='whole'></iframe>");
            if (typeof onLoadFrame == "function") frame.on("load", function () {
                var iframeDoc = frame.contents();
                if ((iframeDoc) && (iframeDoc[0]) && (iframeDoc[0].location) && (iframeDoc[0].location.href != "about:blank")) {
                    frame.on("load", function () { } );
                    onLoadFrame(frame);
                }
            });
            return frame;
        },
        animation: {
            show: { "duration": 0 },
            hide: { "duration": 0 }
        },
        showTitle: true,
        title: title,
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: false,
        resizeEnabled: true,
        position: position || {},
        showCloseButton: true,
        toolbarItems: buttons,
        onShowing: function (e) {
            $("#frmInnerFrame").parent().css({ "padding": "8px" });
        },
        onHidden: function (e) {
            $("#frmInnerFrame").attr("src", "about:blank");
            $("#frmInnerFrame").parent().css({ "padding": "" });
            if (('speechSynthesis' in window)) {
                if ((window.speechSynthesis.speaking)) {
                    qh_t2s_play = false;
                    window.speechSynthesis.cancel();
                }
            }
            $("body").removeClass("quickhelp");
        }
    });
    $("#popupContainer").dxPopup("show");
    setTimeout(function () {
        var frm = document.getElementById("frmInnerFrame");
        if ((frm)) {
            initFrameLoader(frm);
            frm.src = url;
        }
    }, 30);
    return popup;
}

var qh_content = "";
var qh_t2s_play = false;
var qh_t2s_in_pause = false;
function showQuickHelp(params, is_auto, can_edit, onLoadQH) {
    var c = closePopup;
    if (window.parent && typeof window.parent.closePopup == "function" && window.parent.document != document) {
        c = window.parent.closePopup;
    }
    var cookie_id = "QHAutoShow" + curPrjID;
    var qh_hide = document.cookie.indexOf(cookie_id + "=0") >= 0;
    var do_t2s = (typeof ts2s_avail != "undefined" && (ts2s_avail));
    btn = [{
        widget: 'dxCheckBox',
        options: {
            text: "Don't show again",   // will be assigned resString("lblQHAutoShow") later,
            value: qh_hide,
            //visible: ((can_edit) || is_auto),
            onValueChanged: function (e) {
                document.cookie = cookie_id + "=" + (e.value ? 0 : 1) + ";path=/;";
            },
            onContentReady: function (e) {
                $(e.element).find(".dx-checkbox-text").html(resString("lblQHAutoShow"));
            }
        },
        toolbar: "bottom",
        location: 'before'
    }, {
        widget: 'dxButton',
        options: {
            //text: "Play",
            hint: "Start/pause text to speech",
            icon: "fas fa-play",
            disabled: true,
            visible: (t2s) && (do_t2s),
            elementAttr: { "id": "btnQHPlayTS", "class": "dx-btn-no-minwidth"},
            onClick: function (e) {
                if (('speechSynthesis' in window) && (t2s)) {
                    if ((window.speechSynthesis.speaking) && !qh_t2s_in_pause) {
                        if (qh_t2s_play) {
                            qh_t2s_in_pause = true;
                            window.speechSynthesis.pause();
                        } else {
                            qh_t2s_play = false;
                            window.speechSynthesis.cancel();
                        }
                    } else {
                        qh_t2s_play = true;
                        if (qh_t2s_in_pause) {
                            window.speechSynthesis.resume(t2s);
                            qh_t2s_in_pause = false;
                        } else {
                            qh_t2s_in_pause = false;
                            t2s.text = qh_content;
                            window.speechSynthesis.speak(t2s);
                        }
                    }
                    var qh_btn = $("#btnQHPlayTS");
                    if ((qh_btn) && (qh_btn.data("dxButton")) && (qh_t2s_play)) {
                        qh_btn.dxButton("option", { "icon": (qh_t2s_in_pause ? "fas fa-play" : "fas fa-pause")});
                    }
                }
                return false;
            }
        },
        toolbar: "bottom",
        location: 'center'
        }, {
            widget: 'dxButton',
            options: {
                //text: "Stop",
                hint: "Stop text to speech",
                icon: "fas fa-stop",
                disabled: true,
                visible: (t2s) && (do_t2s),
                elementAttr: { "id": "btnQHStopTS", "class": "dx-btn-no-minwidth" },
                onClick: function (e) {
                    if (('speechSynthesis' in window) && (t2s)) {
                        qh_t2s_play = false;
                        if ((window.speechSynthesis.speaking)) {
                            window.speechSynthesis.cancel();
                        }
                    }
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'center'
    }, {
        widget: 'dxButton',
        options: {
            text: resString("btnEdit"),
            visible: ((can_edit) && typeof editQuickHelp == "function"),
            onClick: function () {
                c();
                editQuickHelp(params);
                return false;
            }
        },
        toolbar: "bottom",
        location: 'after'
    }, {
        widget: 'dxButton',
        options: {
            text: resString("btnClose"),
            onClick: function () {
                c();
                return false;
            }
        },
        toolbar: "bottom",
        location: 'after'
    }];

    if (window.parent && typeof window.parent.showQuickHelp == "function" && window.parent.document != document) {
        return window.parent.showQuickHelp(params, is_auto, can_edit);
    }
    //var pos = {
    //    'at': 'bottom', // right
    //    'my': 'bottom', // right
    //    'collision': 'flip flip',
    //    'offset': '-24 -24',
    //};
    //var h = Math.round($(window).height() * 0.6 - 32);
    //if (h < 250) h = 250;
    //h = dlgMaxHeight(h);
    //var w = Math.round($(window).width() * 0.75 - 40);
    //if (w < 320) w = 320;
    //w = dlgMaxWidth(w);
    var pos = {};
    var w = Math.round(dlgMaxWidth(1920) * 0.80);
    var h = Math.round(dlgMaxHeight(1440) * 0.95);
    var dlg = onShowIFrame(infodoc_view_url + '?type=11&autoshow=0' + params + '&r=' + Math.round(100000 * Math.random()), resString("lblQuickHelp"), w, h, pos, btn, (do_t2s && typeof onLoadQH == "function" ? onLoadQH : null));
    if ((dlg) && (dlg.data("dxPopup"))) {
        //dlg.dxPopup("option", { shading: false });
        $("body").addClass("quickhelp");
    }
}

function onLoadQH(f) {
    qh_content = ((f) && (f[0]) && (f[0].contentWindow) && (f[0].contentWindow.document) ? html2text(f[0].contentWindow.document.body.innerHTML) : "");
    var btn = $("#btnQHPlayTS");
    if ((btn) && (btn.data("dxButton"))) btn.dxButton("option", { disabled: (qh_content == "") });
    if (('speechSynthesis' in window)) {
        if ((window.speechSynthesis.speaking)) {
            window.speechSynthesis.cancel();
        }
    }
}

function initFrameLoader(f, _img_preload, _img_size) {
    if ((f)) {
        var d = null;
        if (typeof f.contentWindow != "undefined") d = f.contentWindow.document;
        if (typeof f.document != "undefined") d = f.document;
        if ((d) && typeof (d.body) != "undefined" && (d.body)) d.body.innerHTML = "<div style='width:99%; height:99%; opacity:0.5; background: url(" + (typeof _img_preload == "undefined" || _img_preload == "" ? _img_loading_global : _img_preload) + ") no-repeat center center; background-size: " + (typeof _img_size == "undefined" || !(_img_size) ? "64px" : _img_size) + ";'>&nbsp</div>";
    }
}

function initPasswordField(obj, options, isDXStyle, strengthBarID, on_change) {
    if ((obj)) {
        var _options = {
            pattern: "Abcd$123",
            acceptRate: 0.8,
            allowEmpty: true,
            isMasked: true,
            showToggle: true,
            showGenerate: true,
            showWarn: false,
            showTip: true,
            strengthCheckTimeout: 500,
            //validationCallback: function (inputEl, validateResult) {
            //}, 
            blackList: "000000|111111|11111111|112233|121212|123123|123456|1234567|12345678|123456789|131313|232323|654321|666666|696969|777777|7777777|888888|88888888|87654321|8675309|987654|nnnnnn|nop123|nopqrs|noteglh|npprff|npprff14|npgvba|nyoreg|nyoregb|nyrkvf|nyrwnaqen|nyrwnaqeb|nznaqn|nzngrhe|nzrevpn|naqern|naqerj|natryn|natryf|navzny|nagubal|ncbyyb|nccyrf|nefrany|neguhe|nfqstu|nfuyrl|nffubyr|nhthfg|nhfgva|onqobl|onvyrl|onanan|onearl|onfronyy|ongzna|orngevm|ornire|ornivf|ovtpbpx|ovtqnqql|ovtqvpx|ovtqbt|ovtgvgf|oveqvr|ovgpurf|ovgrzr|oynmre|oybaqr|oybaqrf|oybjwbo|oybjzr|obaq007|obavgn|obaavr|obbobb|obbtre|obbzre|obfgba|oenaqba|oenaql|oenirf|oenmvy|oebapb|oebapbf|ohyyqbt|ohfgre|ohggre|ohggurnq|pnyiva|pnzneb|pnzreba|pnanqn|pncgnva|pneybf|pnegre|pnfcre|puneyrf|puneyvr|purrfr|puryfrn|purfgre|puvpntb|puvpxra|pbpnpbyn|pbssrr|pbyyrtr|pbzcnd|pbzchgre|pbafhzre|pbbxvr|pbbcre|pbeirggr|pbjobl|pbjoblf|pelfgny|phzzvat|phzfubg|qnxbgn|qnyynf|qnavry|qnavryyr|qroovr|qraavf|qvnoyb|qvnzbaq|qbpgbe|qbttvr|qbycuva|qbycuvaf|qbanyq|qentba|qernzf|qevire|rntyr1|rntyrf|rqjneq|rvafgrva|rebgvp|rfgeryyn|rkgerzr|snypba|sraqre|sreenev|sveroveq|svfuvat|sybevqn|sybjre|sylref|sbbgonyy|sberire|serqql|serrqbz|shpxrq|shpxre|shpxvat|shpxzr|shpxlbh|tnaqnys|tngrjnl|tngbef|trzvav|trbetr|tvnagf|tvatre|tvmzbqb|tbyqra|tbysre|tbeqba|tertbel|thvgne|thaare|unzzre|unaanu|uneqpber|uneyrl|urngure|uryczr|uragnv|ubpxrl|ubbgref|ubearl|ubgqbt|uhagre|uhagvat|vprzna|vybirlbh|vagrearg|vjnagh|wnpxvr|wnpxfba|wnthne|wnfzvar|wnfcre|wraavsre|wrerzl|wrffvpn|wbuaal|wbuafba|wbeqna|wbfrcu|wbfuhn|whavbe|whfgva|xvyyre|xavtug|ynqvrf|ynxref|ynhera|yrngure|yrtraq|yrgzrva|yvggyr|ybaqba|ybiref|znqqbt|znqvfba|znttvr|zntahz|znevar|znevcbfn|zneyobeb|znegva|zneiva|znfgre|zngevk|znggurj|znirevpx|znkjryy|zryvffn|zrzore|zreprqrf|zreyva|zvpunry|zvpuryyr|zvpxrl|zvqavtug|zvyyre|zvfgerff|zbavpn|zbaxrl|zbafgre|zbetna|zbgure|zbhagnva|zhssva|zhecul|zhfgnat|anxrq|anfpne|anguna|anhtugl|app1701|arjlbex|avpubynf|avpbyr|avccyr|avccyrf|byvire|benatr|cnpxref|cnagure|cnagvrf|cnexre|cnffjbeq|cnffjbeq1|cnffjbeq12|cnffjbeq123|cngevpx|crnpurf|crnahg|crccre|cunagbz|cubravk|cynlre|cyrnfr|cbbxvr|cbefpur|cevapr|cevaprff|cevingr|checyr|chffvrf|dnmjfk|djregl|djreglhv|enoovg|enpury|enpvat|envqref|envaobj|enatre|enatref|erorppn|erqfxvaf|erqfbk|erqjvatf|evpuneq|eboreg|eboregb|ebpxrg|ebfrohq|ehaare|ehfu2112|ehffvn|fnznagun|fnzzl|fnzfba|fnaqen|fnghea|fpbbol|fpbbgre|fpbecvb|fpbecvba|fronfgvna|frperg|frkfrk|funqbj|funaaba|funirq|fvreen|fvyire|fxvccl|fynlre|fzbxrl|fabbcl|fbppre|fbcuvr|fcnaxl|fcnexl|fcvqre|fdhveg|fevavinf|fgnegerx|fgnejnef|fgrryref|fgrira|fgvpxl|fghcvq|fhpprff|fhpxvg|fhzzre|fhafuvar|fhcrezna|fhesre|fjvzzvat|flqarl|grdhvreb|gnlybe|graavf|grerfn|grfgre|grfgvat|gurzna|gubznf|guhaqre|guk1138|gvssnal|gvtref|gvttre|gbzpng|gbctha|gblbgn|genivf|gebhoyr|gehfgab1|ghpxre|ghegyr|gjvggre|havgrq|intvan|ivpgbe|ivpgbevn|ivxvat|ibbqbb|iblntre|jnygre|jneevbe|jrypbzr|jungrire|jvyyvnz|jvyyvr|jvyfba|jvaare|jvafgba|jvagre|jvmneq|knivre|kkkkkk|kkkkkkkk|lnznun|lnaxrr|lnaxrrf|lryybj|mkpioa|mkpioaz|mmmmmm|password|1234|pussy|12345|dragon|qwerty|mustang|letmein|baseball|master|michael|football|shadow|monkey|abc123|pass|fuckme|6969|jordan|harley|ranger|iwantu|jennifer|hunter|fuck|2000|test|batman|trustno1|thomas|tigger|robert|access|love|buster|soccer|hockey|killer|george|sexy|andrew|charlie|superman|asshole|fuckyou|dallas|jessica|panties|pepper|1111|austin|william|daniel|golfer|summer|heather|hammer|yankees|joshua|maggie|biteme|enter|ashley|thunder|cowboy|silver|richard|fucker|orange|merlin|michelle|corvette|bigdog|cheese|matthew|patrick|martin|freedom|ginger|blowjob|nicole|sparky|yellow|camaro|secret|dick|falcon|taylor|bitch|hello|scooter|please|porsche|guitar|chelsea|black|diamond|nascar|jackson|cameron|computer|amanda|wizard|xxxxxxxx|money|phoenix|mickey|bailey|knight|iceman|tigers|purple|andrea|horny|dakota|aaaaaa|player|sunshine|morgan|starwars|boomer|cowboys|edward|charles|girls|booboo|coffee|xxxxxx|bulldog|ncc1701|rabbit|peanut|john|johnny|gandalf|spanky|winter|brandy|compaq|carlos|tennis|james|mike|brandon|fender|anthony|blowme|ferrari|cookie|chicken|maverick|chicago|joseph|diablo|sexsex|hardcore|willie|welcome|chris|panther|yamaha|justin|banana|driver|marine|angels|fishing|david|maddog|hooters|wilson|butthead|dennis|fucking|captain|bigdick|chester|smokey|xavier|steven|viking|snoopy|blue|eagles|winner|samantha|house|miller|flower|jack|firebird|butter|united|turtle|steelers|tiffany|zxcvbn|tomcat|golf|bond007|bear|tiger|doctor|gateway|gators|angel|junior|thx1138|porno|badboy|debbie|spider|melissa|booger|1212|flyers|fish|porn|matrix|teens|scooby|jason|walter|cumshot|boston|braves|yankee|lover|barney|victor|tucker|princess|mercedes|5150|doggie|zzzzzz|gunner|horney|bubba|2112|fred|johnson|xxxxx|tits|member|boobs|donald|bigdaddy|bronco|penis|voyager|rangers|birdie|trouble|white|topgun|bigtits|bitches|green|super|qazwsx|magic|lakers|rachel|slayer|scott|2222|asdf|video|london|7777|marlboro|srinivas|internet|action|carter|jasper|monster|teresa|jeremy|bill|crystal|peter|pussies|cock|beer|rocket|theman|oliver|prince|beach|amateur|muffin|redsox|star|testing|shannon|murphy|frank|hannah|dave|eagle1|11111|mother|nathan|raiders|steve|forever|angela|viper|ou812|jake|lovers|suckit|gregory|buddy|whatever|young|nicholas|lucky|helpme|jackie|monica|midnight|college|baby|cunt|brian|mark|startrek|sierra|leather|4444|beavis|bigcock|happy|sophie|ladies|naughty|giants|booty|blonde|fucked|golden|fire|sandra|pookie|packers|einstein|dolphins|chevy|winston|warrior|sammy|slut|zxcvbnm|nipples|power|victoria|asdfgh|vagina|toyota|travis|hotdog|paris|rock|xxxx|extreme|redskins|erotic|dirty|ford|freddy|arsenal|access14|wolf|nipple|iloveyou|alex|florida|eric|legend|movie|success|rosebud|jaguar|great|cool|cooper|1313|scorpio|mountain|madison|brazil|lauren|japan|naked|squirt|stars|apple|alexis|aaaa|bonnie|peaches|jasmine|kevin|matt|qwertyui|danielle|beaver|4321|4128|runner|swimming|dolphin|gordon|casper|stupid|shit|saturn|gemini|apples|august|3333|canada|blazer|cumming|hunting|kitty|rainbow|arthur|cream|calvin|shaved|surfer|samson|kelly|paul|mine|king|racing|5555|eagle|hentai|newyork|little|redwings|smith|sticky|cocacola|animal|broncos|private|skippy|marvin|blondes|enjoy|girl|apollo|parker|qwert|time|sydney|women|voodoo|magnum|juice|abgrtyu|dreams|maxwell|music|rush2112|russia|scorpion|rebecca|tester|mistress|phantom|billy|6666|albert|abcdef|abcdefgh|password1|password12|password123|mypass|mypassword|admin|administrator".split("|"), // http://www.skullsecurity.org/wiki/index.php/Passwords
            locale: "en",
            //warnMsgClassName: "pwd_warn",
            //errorWrapClassName: "pwd_error",
            //events: {
            //focused: function (obj) {
            //    }
            //},
            //blured: function (obj) {
            //    }
            //},
            //changed: function (e) {
            //    if ((e) && typeof e.value != "undefined") {
            //    }
            //}
            //},
            //nonMatchField: "inpEmail",
            length: {
                //    min: null,
                max: 48
            },
        };

        function pswUpdateStrengthBar(value, strength) {
            if (typeof strengthBarID != "undefined" && strengthBarID != "" && typeof strength != "undefined") {
                var is_blank = (value == "");
                var colors = ["#dd0000", "#ff7d00", "#ffb70a", "#f1e72f", "#cef238", "#33cc00"];
                var idx = Math.floor((strength + 1) / 2 * colors.length);     // [-1..1]
                if (idx < 1) idx = 1;
                $("#" + strengthBarID).css("opacity", (is_blank ? 0 : 1)).css("background-color", colors[idx - 1]);
            }
        }

        if (typeof options.events == "undefined") options["events"] = {}

        if (isDXStyle) {
            if (typeof options.events["focused"] != "function") options.events["focused"] = function (obj) {
                if ((obj)) {
                    var e = $(obj).closest(".dx-editor-outlined");
                    if ((e.length)) e.addClass("dx-state-focused");
                }
            }
            if (typeof options.events["blured"] != "function") options.events["blured"] = function (obj) {
                if ((obj)) {
                    var e = $(obj).closest(".dx-editor-outlined");
                    if ((e.length)) e.removeClass("dx-state-focused");
                }
            }
        }

        if (typeof strengthBarID != "undefined" && strengthBarID != "") {
            if (typeof options.events["changed"] != "function") options.events["changed"] = function (e) {
                if ((e) && typeof e.value != "undefined") {
                    if (typeof strengthBarID != "undefined" && strengthBarID != "" && typeof e.strength != "undefined" && typeof e.strength.strength_unnorm != "undefined") {
                        pswUpdateStrengthBar(e.value, e.strength.strength_unnorm);
                    }
                    if (typeof on_change == "function") on_change(e);
                }
            }
        }

        if ((options) && typeof options != "undefined") {
            for (let id in options) {
                if (typeof options[id] != "undefined" && options[id] != null) _options[id] = options[id];
            }
        }

        if (typeof options.allowEmpty != "undefined" && (options.allowEmpty)) {
            if (_options.blackList != "") _options.blackList = [];
            if (_options.pattern != "") _options.pattern = "";
            _options.showWarn = false;
        }

        return obj.passField(_options);
    }
}

// ======== Loading panels ==========

function showLoadingPanel(msg, persist) {
    if ((window.parent) && (typeof window.parent.showLoadingPanel == "function") && (window.parent.document != document)) {
        window.parent.showLoadingPanel(msg, persist);
    } else {
        if ((main_loadPanel)) {
            //last_focus = $(":focus");
            //_body_overflow = null;
            //if (!($(".loadpanel").is(":visible"))) {
            //    _body_overflow = $("body").css("overflow");
            //    $("body").css("overflow", "hidden");
            //}
            main_loadPanel.show();
            main_loadPanel_options.message = ((typeof msg != "undefined" && msg != "") ? msg : please_wait);
            main_loadPanel.option("message", main_loadPanel_options.message);
            initLoadingPanelImage();
            setTimeout(initLoadingPanelImage, 10);
            //if (theForm) theForm.disabled = true;
            if (persist === true) _loadPanelPersist = true;
            isLoadingPanel = true;
        }
    }
}

function hideLoadingPanel() {
    if ((window.parent) && (typeof window.parent.hideLoadingPanel == "function") && (window.parent.document != document)) {
        window.parent.hideLoadingPanel();
    } else {
        if ((main_loadPanel) && !_loadPanelPersist) {
            //if (theForm) theForm.disabled = false;
            main_loadPanel.hide();
            //if (_body_overflow != null) $("body").css("overflow", _body_overflow);
            //if ((last_focus) && (last_focus.length) && !(last_focus.is(':disabled'))) last_focus.focus();
            isLoadingPanel = false;
        }
    }
}

function displayLoadingPanel(visible) {
    if ((visible)) showLoadingPanel(); else hideLoadingPanel();
}

function showBackgroundMessage(msg, icon, do_spin) {
    var txt = "<nobr><i class='" + (typeof icon == "undefined" || icon == "" ? "fas fa-sync" : icon) + (do_spin === false || do_spin === 0 ? "" : " fa-spin") + "' style='line-height:18px'></i>" + (msg == "" ? "" : "<span class='" + ($("body").hasClass("nc") ? "nowide900" : "nowide700") + "' style='padding-left:1ex'>" + msg + "</span>") + "</nobr>";
    $("#divBackgroundProgress").html(txt).fadeIn(200);
}

function hideBackgroundMessage() {
    $("#divBackgroundProgress").fadeOut(400);
}

// ==== Page preloader ====

function onPageLoaded() {
    //$(".page-mainwrapper").hide();
    //return false;
    var preload = $(".page-preloader");
    if ((preload) && (preload.length)) {
        preload.animate({ opacity: "0" }, 500, function () { preload.html("").hide(); });
        $(".page-mainwrapper").animate({ opacity: "1" }, 500)
    }
}

// ============ AJAX / API =================

function showPageContentAsFrame(html) {
    var begin = (html + "").substr(0, 5000).toLowerCase();
    if ((error500_redirect) && (begin.indexOf("error 500") > 0 || begin.indexOf("<html ") >= 0)) {
        setTimeout(function () { loadURL('/Error.aspx'); }, 250);
        return "";
    }
    if ((html + "").substr(0, 10000).toLowerCase().indexOf("app_offline.htm") > 0) {
        document.open();
        document.write(html);
        document.close();
        return false;
    }
    var r = html;
    var idx = r.indexOf("<!-- main content -->");
    if (idx > 0) r = r.substr(idx);
    idx = r.indexOf("<!-- /main content -->");
    if (idx > 0) r = r.substr(0, idx - 1);
    setTimeout(function () {
        $("#btnBack").hide();
    }, 200);
    return "<div class='error-popup whole'><!--iframe class='whole' id='frmRTE'-->" + r + "<!--/iframe--></div>";
}

function _call_success(response, func) {
    var done = false;
    if ((typeof response != "undefined" && typeof response.value != "undefined") || (typeof response != "undefined" && (response + "").substr(0, 1000).toLowerCase().indexOf("<html") < 0)) {
        if (typeof func == "function" && (func)) func((typeof response.value != "undefined") ? response.value : response);
        done = true;
    }
    if (!done) {
        var show_msg = "";
        if ((response + "").substr(0, 10000).toLowerCase().indexOf("app_offline.htm") > 0) {
            document.open();
            document.write(response);
            document.close();
            return false;
        }
        if ((response + "").substr(0, 1000).toLowerCase().indexOf("error 500") > 0) {
            show_msg = showPageContentAsFrame(response);
        } else {
            show_msg = resString("msgWrongServerReply");
        }
        if (show_msg != "") _call_message(show_msg);
    }
    return done;
}

function _call_error(response, textStatus, errorThrown) {
    if ((_ajax_error_ignore)) return false;

    var msg = '';
    var parse_details = true;
    var icons = {
        "400": "far fa-question-circle",
        "401": "fa fa-user-lock",
        "403": "fa fa-user-lock",
        "405": "far fa-window-close",
        "406": "far fa-window-close",
        "408": "fa fa-hourglass-end",
        "414": "fa fa-ruler-horizontal",
        "415": "fa fa-photo-video",
        "429": "fa fa-traffic-light",
        "500": "fa fa-cogs",
        "501": "far fa-meh",
        "502": "fa fa-network-wired",
        "503": "fa fa-plug",
        "504": "fa fa-hourglass-end",

    };

    var icon = "";
    if (typeof response.status != "undefined") {
        if (typeof icons[response.status + ""] != "undefined") icon = icons[response.status + ""];
        switch (response.status) {
            case 0: msg = resString("erAjaxNotConnected"); break;
            case 400: msg = resString("errAjax400"); break; // bad request
            case 401: msg = resString("errAjax401"); parse_details = false; break; // unauthorized
            case 403: msg = resString("errAjax403"); parse_details = false; break; // forbidden access
            case 404: msg = resString("errAjax404"); parse_details = false; break; // page not found
            case 405: msg = resString("errAjax405"); break; // method not allowed
            case 406: msg = resString("errAjax406"); break; // not acceptable
            case 408: msg = resString("errAjax408"); parse_details = false; break; // request timeout
            case 414: msg = resString("errAjax414"); break; // URI too long
            case 415: msg = resString("errAjax415"); break; // unsupported media type
            case 429: msg = resString("errAjax429"); break; // too many requests
            case 500: msg = resString("errAjax500"); break; // internal server error
            case 501: msg = resString("errAjax501"); break; // not implemented
            case 502: msg = resString("errAjax502"); break; // bad gateway
            case 503: msg = resString("errAjax503"); parse_details = false; break; // service unavailable
            case 504: msg = resString("errAjax504"); break; // gateway timeout
            default: if (response.status > 200) msg = "Unknown error " + response.status; break;
        }
    }

    if (msg == "" && typeof textStatus != "undefined") {
        switch (textStatus) {
            case "timeout": msg = resString("errAjaxTimeout"); break;
                //case "error": msg = resString("errAjaxError"); break;
            case "abort": msg = resString("errAjaxAbort"); break;
            case "parsererror": msg = resString("errAjaxParse"); break;
        }
    }

    if (typeof errorThrown != "undefined" && errorThrown != "") msg = "<b>" + msg + "</b>" + (msg == "" ? "" : "<br>[") + errorThrown + (msg == "" ? "" : "]");

    var details = (parse_details && typeof response.responseText != "undefined" ? response.responseText : "");

    if (msg != "") {
        var title = msg;
        var has_html = false;
        var orig_page = "";
        if (details == "") {
            var idx = title.toLowerCase().indexOf("error");
            var msg = (idx >= 0 && idx < 10 ? title : "<b>Error</b>: " + title);
            if (icon != "") msg = "<table border=0><tr valign=middle><td style='padding-right:1em'><i class='" + icon + " fa-2x gray'></i></td><td>" + msg + "</td></tr></table>";
        } else {
            has_html = (details.substr(0, 1000).toLowerCase().indexOf("<html") >= 0);
            if (has_html) {
                msg = showPageContentAsFrame("<h5 class='error'>" + title + "</h5><hr>" + details);
            } else {
                msg = "<div class='error-popup whole'><h5 class='error'>" + title + "</h5><hr>" + replaceString("\n", "<br>", details) + "</div>";
            }
        }
        _call_message(msg);
        showBackgroundMessage(title, "fas fa-exclamation-triangle", false);
        setTimeout(function () {
            var o = $("#divBackgroundProgress").find("i");
            if ((o) && (o.length > 0) && o.attr("class").indexOf("warning") > 0) hideBackgroundMessage();
        }, 3000);
    }
}

function _call_message(msg) {
    closePopup();
    //var w = Math.round($(window).width() - 96);
    //var h = Math.round($(window).height() - 128);
    var container = $("#popupContainer");
    if ((container) && (container.length)) {
        container.dxPopup({
            minWidth: 200,
            maxWidth: dlgMaxWidth(1000),
            width: "auto",
            minHeight: 50,
            maxHeight: dlgMaxHeight(800),
            height: "auto",
            contentTemplate: function () {
                return $("<div>" + msg + "</div>");
            },
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    //icon: "far fa-times-circle", 
                    text: resString("btnContinue"),
                    onClick: function () {
                        closePopup();
                        reloadPage();
                        return false;
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }],
            showTitle: true,
            title: "Request error",
            dragEnabled: true,
            shading: true,
            closeOnOutsideClick: false,
            resizeEnabled: true,
            //position: { my: 'center', at: 'center', of: window },
            showCloseButton: false,
        });
        container.dxPopup("show");
    } else {
        alert(stripHTMLTags(msg));
    }
    //window.focus();
    //var a = DevExpress.ui.dialog.alert(msg, "AJAX request error");
    //a.done(function () {
    //    hideLoadingPanel();
    //    return true;
    //});
    $(".dx-dialog-wrapper").css("z-index", "99999");
}

// api_name: pass path to section without the root (/api/), you can add method as "?action=AAAA"
// data: raw data to post, will be enocded as json string
function callAPI(api_name, data, func, silent, please_wait_delay) {
    return callAjax("params=" + replaceString("&", "%26", JSON.stringify(data)), func, _method_POST, silent, _API_URL + api_name + (api_name.indexOf("?") < 0 && api_name[api_name.length - 1] != "/" ? "/" : ""), undefined, please_wait_delay);   // add trailing slash in case of plain path to WebAPI section
}

function callAjax(params, func, method, silent, url, is_async, please_wait_delay) {
    _ajax_inprogress = true;
    var current_ajax_call_completed = false;
    var url = (typeof url != "undefined" && url != "" && url != null && (url) ? url : null);
    var isFormData = (params instanceof FormData);
    return $.ajax({
        async: (typeof is_async == "undefined" || is_async == true),
        type: (typeof method == "undefined" || isFormData ? _method_POST : method),
        url: url,
        data: ((url == null || url.indexOf(_API_URL) < 0) && !(isFormData) ? "ajax=yes&" + params : params), // for WebAPI or FormData no need to pass ajax=yes
        dataType: (url == null || url.indexOf(_API_URL) < 0 ? null : "json"),
        xhrFields: { withCredentials: true },
        processData: !isFormData,
        contentType: (isFormData ? false : 'application/x-www-form-urlencoded; charset=UTF-8'), // 'multipart/form-data' for forms
        cache: false,
        timeout: _ajax_timeout,
        beforeSend: function (xhr) {
            if (typeof silent == "undefined" || silent == undefined || silent == null || silent === false || silent === 0) {
                if (typeof please_wait_delay != "undefined" && please_wait_delay > 0) {
                    setTimeout(function () {
                        if (!current_ajax_call_completed) showLoadingPanel();
                    }, please_wait_delay * 1);
                } else {
                    showLoadingPanel();
                }
            } else {
                if (silent !== true && silent !== 1) showBackgroundMessage(typeof silent == "string" ? silent : "");
            }
        },
        success: function (response) {
            current_ajax_call_completed = true;
            hideLoadingPanel();
            if (silent !== true && silent !== 1) showBackgroundMessage("", "fas hourglass-start fa-pulse", true);
            _call_success(response, func);
        },
        error: function (response, textStatus, errorThrown) {
            current_ajax_call_completed = true;
            hideLoadingPanel();
            _call_error(response, textStatus, errorThrown);
        },
        complete: function (response, textStatus) {
            current_ajax_call_completed = true;
            hideBackgroundMessage();
            _ajax_inprogress = false;
            startPing();
        },
    });
}

function uploadBinaryData(data, filename, mime, on_upload, silent, dest) {
    var form = new FormData();
    var do_submit = true;
    try {
        form.append("action", "upload");
        if (data.length > 11) {
            if (data.substr(0, 5) == "data:") {
                var idx = data.indexOf("base64,");
                if (idx > 0) {
                    form.append("base64", "yes");
                }
            }
        }
        //form.append("ajax", "yes");
        if (mime != "" && typeof mime != "undefined") form.append("mime", mime);
        if (dest != "" && typeof dest != "undefined") form.append("dest", dest);
        form.append("files" + (is_ie ? "\"; filename=\"" + encodeURI(filename) : ""), data, filename);
    }
    catch (e) {
        do_submit = false;
        _loadPanelPersist = false;
        showErrorMessage("<div class='error'>Unable to submit data to web server.</div><br>" + e.message, true);
    }
    if ((do_submit)) callAjax(form, on_upload, _method_POST, silent, _API_URL + "service/");
    return do_submit;
}

function parseReply(data) {
    return (data != "" && typeof data != "undefined" && data[0] != "<" && (JSON && JSON.parse(data) || $.parseJSON(data)));
}

function isValidReply(data) {
    return (typeof data == "object" && typeof data.Result != "undefined");
}

// ========= Misc for popups and clipboard ===========

function showMessagePopup(msg, title, max_width, btn_caption) {

    onResizePopup = function (e) {
        var dp = $("#divPopupScroll");
        if ((dp) && (dp.length) && (dp.parent().height())) dp.height(dp.parent().height());
    }

    closePopup();
    var popup = $("#popupContainer").dxPopup({
        minWidth: 200,
        maxWidth: dlgMaxWidth(max_width || 1200),
        width: "auto",
        minHeight: 50,
        maxHeight: dlgMaxHeight(900),
        height: "auto",
        contentTemplate: function () {
            return $("<div style='height:100%; overflow:auto;' id='divPopupScroll'>" + msg + "</div>");
        },
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                //icon: "far fa-times-circle", 
                text: btn_caption || resString("btnOK"),
                onClick: function () {
                    closePopup();
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }],
        onShown: onResizePopup,
        onResizeEnd: onResizePopup,
        showTitle: (title != ""),
        title: title,
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: true,
        resizeEnabled: true,
        //position: { my: 'center', at: 'center', of: window },
        showCloseButton: true,
    });
    $("#popupContainer").dxPopup("show");
    $(".dx-dialog-wrapper").css("z-index", "99999");
    return popup;
}

function CreatePopup(url, window_name, options, ignore_blocker) {
    var w = window.open(url, window_name, options);
    if (!(w) && (ignore_blocker != true)) {
        window.focus();
        showErrorMessage(resString("msgPopupBlockerInfo"), true);
    }
    return w;
}

function copyDataToClipboard(data, msg, showDialogAsWorkaround) {
    if (typeof showDialogAsWorkaround == "undefined") showDialogAsWorkaround = true;
    if (typeof msg == "undefined" || msg == "") msg = resString("msgDataCopied");
    if (msg.substr(0, 3) == "(!)") msg = "";
    var notify = function (msg) {
        if (typeof DevExpress != "undefined") {
            DevExpress.ui.notify(msg, 'info', 3000);
        } else {
            jDialog(msg, false, ";", "", "Copy to clipboard");
        }
    }
    if (window.clipboardData) {
        if (window.clipboardData.setData('Text', data)) {
            if (msg != "") notify(msg, 'info', 3000);
        } else {
            if (showDialogAsWorkaround) showCopyDialog(data);
            return false;
            //notify(resString("msgUnableToCopy"), 'error', 3000);
        }
    } else {
        if (is_firefox && false) { // try to do same for Chrome and FF
            if (showDialogAsWorkaround) showCopyDialog(data);
            return false;
            //notify(resString("titleNonIECopy2"), 'warning', 10000);
        } else {
            var success = chromeCopyToClipboard(data);
            if (success) {
                if (msg != "") notify(msg, 'info', 3000);
            } else {
                if (showDialogAsWorkaround) showCopyDialog(data);
                return false;
                //notify(resString("msgUnableToCopy"), 'error', 3000);
            };
        }
    }
    return true;
}

/* Copy to Clipboard in Chrome (works only when called from user-initiated code - click handler, and in the latest Crome version) */
function chromeCopyToClipboard(text) {

    //var textarea = document.createElement('textarea');
    //textarea.textContent = text;
    //document.body.appendChild(textarea);
    //var selection = document.getSelection();
    //var range = document.createRange();
    ////range.selectNodeContents(textarea);
    //range.selectNode(textarea);
    //selection.removeAllRanges();
    //selection.addRange(range);
    //var success = document.execCommand('copy');
    //selection.removeAllRanges();
    //document.body.removeChild(textarea);

    // AD: try this approach since the prev version adding a new line before the real text and links are not valid
    document.oncopy = function (event) {
        event.clipboardData.setData("text/plain", text);
        event.preventDefault();
    };
    var success = document.execCommand("copy", false, null);

    return success;
}

function showCopyDialog(text) {
    dxDialog(resString("titleNonIECopy2") + ":<p><textarea cols=65 rows=3 id='txtCopyClipbord' readonly='readonly' onblur='this.select();'>" + htmlEscape(text) + "</textarea></p>", "", null, resString('titleCopyToClipboard'));
    $("#txtCopyClipbord").val(text).focus();
    setTimeout(function () { $("#txtCopyClipbord").focus().select(); }, 300);
    setTimeout(function () { $("#txtCopyClipbord").focus().select(); }, 500);
    setTimeout(function () { $("#txtCopyClipbord").focus().select(); }, 800);
    setTimeout(function () { $("#txtCopyClipbord").focus().select(); }, 999);
}

function dlgMaxWidth(def) {
    if (def < 100 || typeof def == "undefined") def = 650;
    if ($(window).width() - 48 < def) def = $(window).width() - 48;
    return def;
}

function dlgMaxHeight(def) {
    if (def < 100 || typeof def == "undefined") def = 450;
    if ($(window).height() - 48 < def) def = $(window).height() - 48;
    return def;
}

// =========== Dialogs, menus, etc ==============

function confirmLogout() {
    var result = DevExpress.ui.dialog.confirm(logout_confirmation, resString("titleConfirmation"));
    result.done(function (dialogResult) {
        if (dialogResult) {
            if (typeof (logout_before) == "function") logout_before(doLogout); else doLogout();
        }
    });
    return false;
}

function doLogout() {
    loadURL('/?action=logout');
    return false;
}

function onShowSubmenu(obj, id, items_lst, target_id) {
    if (obj.html() == "") obj.dxContextMenu({
        onItemClick: function (e) {
            last_cmenu_opened = "";
            if (typeof (e.itemData.onItemClick) == "function") e.itemData.onItemClick(e);
        },
        onHidden: function (e) {
            last_cmenu_opened = "";
        },
        onShowing: function (e) {
            if (last_cmenu_opened == "") e.cancel = true;
            $(".custom_tooltip").tooltip().hide();
            setTimeout(hideTooltips, 200);
        }
    });
    var cm = obj.dxContextMenu("instance");
    if (id == last_cmenu_opened) {
        //$("body").css("overflow", "");
        cm.hide();
        last_cmenu_opened = "";
    } else {
        last_cmenu_opened = id;
        cm.beginUpdate();
        cm.option("items", items_lst);
        cm.option("position", (target_id == "" ? { my: 'top left', at: 'top left' } : { of: "#" + target_id, at: 'left bottom' }));
        cm.endUpdate();
        //$("body").css("overflow", "hidden");
        cm.show();
    }
}

function templateButtonSubmenu(title, icon) {
    return $((icon == "" ? "" : "<i class='dx-icon " + icon + "' style='padding-right:10px'></i>") + (title == "" ? "" : "<span class='dx-button-text'>" + title + "</span>") + "<i class='dx-icon dx-icon-spindown dx-icon-right'></i>"); //</div>
}

function notImplemented() {
    DevExpress.ui.notify('Sorry, not implemented yet', 'warning');
    return false;
}

function OpenSendMail(params) {
    var w = CreatePopup('../SendMail.aspx?' + params, 'SenMail', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=yes,resizable=yes,width=600,height=500', true);
    if ((w)) setTimeout(function () { if ((w)) w.focus(); }, 1000);
}

function OpenRichEditor(cmd) {
    //OpenRichEditorPopup(cmd);
    if (window.parent && typeof window.parent.OpenRichEditor == "function" && window.parent.document != document) {
        return window.parent.OpenRichEditor(cmd);
    }
    return OpenRichEditorDialog(cmd);
}

function OpenRichEditorDialog(cmd) {
    var w = Math.round($(window).width() - 32);
    var h = Math.round($(window).height() - 48);
    $("#popupContainer").dxPopup({
        width: w,
        height: h,
        contentTemplate: function () {
            return $("<iframe src=''  allowfullscreen='allowfullscreen' mozallowfullscreen='mozallowfullscreen' msallowfullscreen='msallowfullscreen' oallowfullscreen='oallowfullscreen' webkitallowfullscreen='webkitallowfullscreen' frameborder='0' scrolling='no' id='frmRichEditor' style='border:0px;' class='whole'></iframe>");
        },
        animation: {
            show: { "duration": 0 },
            hide: { "duration": 0 }
        },
        showTitle: true,
        title: resString("btnEditInfodoc"),
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: false,
        resizeEnabled: true,
        //position: { my: 'center', at: 'center', of: window },
        showCloseButton: false,
    });
    $("#popupContainer").dxPopup("show");
    setTimeout(function () {
        var frm = document.getElementById("frmRichEditor");
        if ((frm)) {
            initFrameLoader(frm);
            frm.src = rich_editor_url + cmd;
        }
    }, 30);
}

function onCloseRichEditor() {
    closePopup();
}

function OpenRichEditorPopup(cmd) {
    var dlg_w = dlgMaxWidth(1200);
    var dlg_h = dlgMaxHeight(850);
    var w = CreatePopup(rich_editor_url + cmd, 'RichEditor', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=' + dlg_w + ',height=' + dlg_h, true);
    if ((w)) setTimeout(function () { if ((w)) w.focus(); }, 1000);
}

function OpenInfodoc(cmd) {
    CreatePopup(infodoc_view_url + cmd, '', 'menubar=no,maximize=no,titlebar=no,status=yes,location=no,toolbar=no,channelmode=no,scrollbars=no,resizable=yes,width=840,height=500', true);
}

function loadURL(url, msg) {
    cancelPing(true);
    try {
        showLoadingPanel(msg, true);
        window.location.href = url;
    }
    catch (e) {
        hideLoadingPanel();
    }
}

function reloadPage(delay) {
    if (typeof delay == "undefined" || delay < 1) {
        showLoadingPanel("", true);
        document.location.reload(true);
    } else {
        setTimeout(function () {
            showLoadingPanel("", true);
            document.location.reload(true);
        }, delay * 1000);
    }
}

function createProgress(p, width, fill_class, fill_full_class) {
    if ((p) && (p.length)) {
        var max = p.data("max") * 1;
        var val = p.data("value") * 1;
        p.dxProgressBar({
            "min": 0,
            "max": max,
            "value": val,
            "width": (typeof width == "undefined" ? 150 : width),
            showStatus: false,
        });
        p.find(".dx-progressbar-range").addClass((val <= 0) ? "progress_empty" : (typeof fill_full_class != "undefined" && val >= max ? fill_full_class : (typeof fill_class == "undefined" ? "progress_blue" : fill_class)));
    }
}

function onFormNarrow(form_selector, width_min) {
    var w = ($("#divMainContent").length ? $("#divMainContent").width() : $(window).width());
    var narrow = (w < (width_min < 400 ? 600 : width_min));
    var h_fields = $(form_selector).find(".dx-label-h-align");
    var v_fields = $(form_selector).find(".dx-label-v-align");
    if (narrow && h_fields.length > 0) {
        h_fields.removeClass("dx-label-h-align").addClass("dx-label-v-align");
    }
    if (!narrow && v_fields.length > 0) {
        v_fields.removeClass("dx-label-v-align").addClass("dx-label-h-align");
    }
}

function centerPopup(id) {
    var p = $(typeof id == "undefined" || id == "" ? "#popupContainer" : id);
    if ((p) && (p.length) && p.data("dxPopup")) p.dxPopup("option", "position", { my: 'center', at: 'center', of: window });
}

function closePopup(popup_id) {
    popup_id = popup_id || "#popupContainer";
    if ($(popup_id).length && $(popup_id).data()) {
        $(popup_id).dxPopup();
        if ($(popup_id).dxPopup("option", "visible")) {
            $(popup_id).dxPopup("hide");
            $(popup_id).dxPopup("dispose");
        }
    }
}

function getProjectLinkDialog(prjid, Name, AccessCode, MeetingID, CS_MeetingID, isOnline, root_url, extra_title, at_root_url, show_both) {
    var w = dlgMaxWidth(700);
    var e = w - 290;
    if (e < 150) e = 150;
    var is_pm = can_create;
    if ((curPrjData) && typeof curPrjData.CanModify != "undefined") is_pm = curPrjData.CanModify;
    var show_impact = ((show_both == true) && isRiskion && prjid == curPrjID && typeof curPrjData.AccessCodeImpact != "undefined");
    if (show_impact) {
        AccessCode = curPrjData.AccessCode;
        MeetingID = curPrjData.MeetingID;
        CS_MeetingID = curPrjData.CS_MeetingID;
    }
    $("#popupContainer").dxPopup({
        width: w,
        height: "auto",
        title: resString("lblGetLink") + (typeof extra_title != "undefined" && extra_title != "" ? extra_title : ""),
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                //icon: "far fa-times-circle", 
                text: resString("btnClose"),
                elementAttr: { "class": "button_esc" },
                onClick: function () {
                    closePopup();
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }],
        contentTemplate: function (contentElement) {
            var c = "";
            if (is_pm) c += "<div style='text-align:center'>" + resString("lblProjectLinks") + " '<span style='font-weight:600'>" + ShortString(Name, 75, false) + "</span>':</div>\n";
            c += "<table border=0 style='text-align:center; margin:1ex auto;'>";
            if (show_impact) c += "<tr><td colspan=2><h6 style='margin-top:1ex'><h6>" + parseTemplates("%%Likelihood%%") + ":</h6></td></tr>\n";
            if (AccessCode != "") {
                c += "<tr valign=middle><td class='text' align=right>" + resString("mnuModelLink") + ":</td><td><div id='inpLinkUrl0'></div></td><td><div id='btnCopyLink0'></div></td></tr>\n";
                if ((is_pm)) c += "<tr valign=middle><td class='text' align=right>" + resString("lblAnytimeLink") + ":</td><td><div id='inpLinkUrl1'></div></td><td><div id='btnCopyLink1'></div></td></tr>\n";
            }
            if (MeetingID != "" && (is_pm)) c += "<tr valign=middle><td class='text' align=right>" + resString("mnuTeamTimeEvaluation") + ":</td><td><div id='inpLinkUrl2'></div></td><td><div id='btnCopyLink2'></div></td></tr>\n";
            if (CS_MeetingID != "" && (is_pm)) c += "<tr valign=middle><td class='text' align=right>" + resString("mnuStructuring") + ":</td><td><div id='inpLinkUrl3'></div></td><td><div id='btnCopyLink3'></div></td></tr>";

            if (show_impact) {
                c += "<tr><td colspan=2><h6 style='margin-top:1ex'>" + parseTemplates("%%Impact%%") + ":</h6></td></tr>\n";
                if (AccessCode != curPrjData.AccessCodeImpact) {
                    c += "<tr valign=middle><td class='text' align=right>" + resString("mnuModelLink") + ":</td><td><div id='inpLinkUrl0i'></div></td><td><div id='btnCopyLink0i'></div></td></tr>\n";
                    if ((is_pm)) c += "<tr valign=middle><td class='text' align=right>" + resString("lblAnytimeLink") + ":</td><td><div id='inpLinkUrl1i'></div></td><td><div id='btnCopyLink1i'></div></td></tr>\n";
                }
                if (MeetingID != curPrjData.MeetingIDImpact && (is_pm)) c += "<tr valign=middle><td class='text' align=right>" + resString("mnuTeamTimeEvaluation") + ":</td><td><div id='inpLinkUrl2i'></div></td><td><div id='btnCopyLink2i'></div></td></tr>\n";
                if (CS_MeetingID != curPrjData.CS_MeetingIDImpact && (is_pm)) c += "<tr valign=middle><td class='text' align=right>" + resString("mnuStructuring") + ":</td><td><div id='inpLinkUrl3i'></div></td><td><div id='btnCopyLink3i'></div></td></tr>";
            }

            c += "</table>";
            contentElement.append($(c));
        }
    });
    $("#popupContainer").dxPopup("show");
    if (AccessCode != "") {
        $("#inpLinkUrl0").dxTextBox({ value: (typeof at_root_url != "undefined" && at_root_url != "" ? at_root_url : root_url) + "/?passcode=" + AccessCode, width: e });
        $("#btnCopyLink0").dxButton({
            icon: "far fa-clipboard",
            text: (w > 450 ? resString("btnCopy") : ""),
            onClick: function () {
                checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl0').find('input').val());
                return false;
            }
        });
        setTimeout(function () { $('#inpLinkUrl0').find('input').select().focus(); }, 500);
        if (is_pm) {
            $("#inpLinkUrl1").dxTextBox({ value: (typeof at_root_url != "undefined" && at_root_url != "" ? at_root_url : root_url) + "/?passcode=" + AccessCode + "&pipe=yes", width: e });
            $("#btnCopyLink1").dxButton({
                icon: "far fa-clipboard",
                text: (w > 450 ? resString("btnCopy") : ""),
                onClick: function () {
                    checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl1').find('input').val());
                    return false;
                }
            });
        }
    }
    if (MeetingID != "" && (is_pm)) {
        $("#inpLinkUrl2").dxTextBox({ value: root_url + "/?meeting_id=" + MeetingID, width: e });
        $("#btnCopyLink2").dxButton({
            icon: "far fa-clipboard",
            text: (w > 450 ? resString("btnCopy") : ""),
            onClick: function () {
                checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl2').find('input').val());
                return false;
            }
        });
    }
    if (CS_MeetingID != "" && is_pm) {
        $("#inpLinkUrl3").dxTextBox({ value: root_url + "/?meeting_id=" + CS_MeetingID, width: e });
        $("#btnCopyLink3").dxButton({
            icon: "far fa-clipboard",
            text: (w > 450 ? resString("btnCopy") : ""),
            onClick: function () {
                checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl3').find('input').val());
                return false;
            }
        });
    }
    if (show_impact) {
        if (curPrjData.AccessCodeImpact != "") {
            $("#inpLinkUrl0i").dxTextBox({ value: (typeof at_root_url != "undefined" && at_root_url != "" ? at_root_url : root_url) + "/?passcode=" + curPrjData.AccessCodeImpact, width: e });
            $("#btnCopyLink0i").dxButton({
                icon: "far fa-clipboard",
                text: (w > 450 ? resString("btnCopy") : ""),
                onClick: function () {
                    checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl0i').find('input').val());
                    return false;
                }
            });
            if (is_pm) {
                $("#inpLinkUrl1i").dxTextBox({ value: (typeof at_root_url != "undefined" && at_root_url != "" ? at_root_url : root_url) + "/?passcode=" + curPrjData.AccessCodeImpact + "&pipe=yes", width: e });
                $("#btnCopyLink1i").dxButton({
                    icon: "far fa-clipboard",
                    text: (w > 450 ? resString("btnCopy") : ""),
                    onClick: function () {
                        checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl1i').find('input').val());
                        return false;
                    }
                });
            }
        }
        if (curPrjData.MeetingIDImpact != "" && (is_pm)) {
            $("#inpLinkUrl2i").dxTextBox({ value: root_url + "/?meeting_id=" + curPrjData.MeetingIDImpact, width: e });
            $("#btnCopyLink2i").dxButton({
                icon: "far fa-clipboard",
                text: (w > 450 ? resString("btnCopy") : ""),
                onClick: function () {
                    checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl2i').find('input').val());
                    return false;
                }
            });
        }
        if (curPrjData.CS_MeetingIDImpact != "" && is_pm) {
            $("#inpLinkUrl3i").dxTextBox({ value: root_url + "/?meeting_id=" + curPrjData.CS_MeetingIDImpact, width: e });
            $("#btnCopyLink3i").dxButton({
                icon: "far fa-clipboard",
                text: (w > 450 ? resString("btnCopy") : ""),
                onClick: function () {
                    checkProjectOnlineAndCopyLink(prjid, isOnline, $('#inpLinkUrl3i').find('input').val());
                    return false;
                }
            });
        }
    }

    centerPopup();
}

function checkProjectOnlineAndCopyLink(prjid, isOnline, text) {
    closePopup();
    var copied = copyDataToClipboard(text, "", false);
    var notify = function () {
        DevExpress.ui.notify(resString("msgDataCopied"), 'info', 3000);
    }
    if ((typeof isOnline == "boolean" && (isOnline)) || (typeof isOnline.isOnline != "undefined" && (isOnline.isOnline))) {
        if (!copied) copyDataToClipboard(text);
    } else {
        askSetProjectOnlineAndAction(prjid, function (prjid, is_online) {
            //if (!copied) copyDataToClipboard(text);
            if (typeof isOnline.isOnline != "undefined") isOnline.isOnline = is_online;
            var cb = $("#cbOnline" + prjid);
            if ((cb) && (cb.length)) cb.prop("checked", true);
            notify();
        }, function (prjid, is_online) {
            //if (!copied) copyDataToClipboard(text);
            notify();
        }, function (prjid, is_online) {
            //if (!copied) copyDataToClipboard(text);
            notify();
        });
    }
}

function showResMessageCallFunc(res, is_error, func) {
    if (res.Message != "") {
        var a = showResMessage(res, is_error);
        if (typeof func == "function") a.done(function () { func(res); });
    } else {
        if (typeof func == "function") func(res);
    }
}

function showResMessage(res, is_error) {
    return showErrorMessage(typeof res != "undefined" && typeof res.Message != "undefined" && res.Message != "" ? res.Message : resString("msgCantPerformAction"), is_error);
}

function showErrorMessage(msg, is_error) {
    _loadPanelPersist = false;
    if ((window.parent) && typeof window.parent._loadPanelPersist != "undefined" && (window.parent.document != document)) window.parent._loadPanelPersist = false;
    hideLoadingPanel();
    if ((window.parent) && typeof window.parent.showErrorMessage == "function" && (window.parent.document != document)) {
        return window.parent.showErrorMessage(msg, is_error);
    } else {
        return DevExpress.ui.dialog.alert("<div style='max-width:700px;'>" + msg + "</div>", resString((typeof is_error == "undefined" || (is_error)) ? "titleError" : "titleInformation"));
    }
}

function toggleScrollMain() {
    $("#MainContent").toggleClass("no_scroll");
    $(".mainContentContainer").toggleClass("no_scroll");
}

function pswShowPlain(obj, plain) {
    if ((obj) && (obj.length)) {
        obj.prop("type", (plain) ? "text" : "password");
    }
}

// ==== ping ====

function startPing() {
    if ((ping_active) && !(ping_inprogress)) {
        cancelPing(false);
        ping_timer = setTimeout(function () { doPing(); }, ping_interval);
    }
}

function cancelPing(do_stop) {
    if ((ping_timer)) window.clearTimeout(ping_timer);
    if (do_stop === true) ping_active = false;
}

function doPing() {
    if (!_ajax_inprogress) {
        ping_inprogress = true;
        var data = { "hash": ping_hash, "pgid": pgID };
        callAPI("service/?action=session_state" + (typeof ping_params == "undefined" || ping_params == null || ping_params == "" ? "" : "&" + (typeof ping_params == "function" ? ping_params() : ping_params)), data, onPing, "");
        showBackgroundMessage("Ping...", "fas fa-satellite-dish", false);
    }
}

function onPing(res) {
    hideBackgroundMessage();
    ping_inprogress = false;

    if ((ping_active) && typeof res == "object" && typeof res.hash == "string") {
        ping_missed_counter = 0;
        if (typeof res.hash != "undefined") {

            if (ping_hash != res.hash) {
                ping_hash = res.hash;

                if (typeof res.cmd != "undefined") {
                    switch (res.cmd) {
                        case "session_terminate":
                            var a = DevExpress.ui.dialog.alert(resString("msgPswHasBeenChanged"), resString("titleSession"));
                            a.done(function () {
                                doLogout();
                                return true;
                            });
                            return true;
                            break;
                    }
                }

                var msg = "";
                var session_changed = (typeof res.usid != "undefined" && res.usid != "" && curSID != "" && res.usid != curSID);
                var user_changed = (typeof res.user != "undefined" && res.user != null && typeof res.user.Email != "undefined" && res.user.Email != sess_useremail);
                var wkg_changed = (typeof res.wkg != "undefined" && res.wkg != null && res.wkg.id != "" && res.wkg.id != wkg_id);
                var prj_changed = (typeof res.project != "undefined" && res.project != null && res.project.ID != "" && res.project.ID != curPrjID);

                var msg_reload = true;
                //if (msg == "" && user_changed && res.user.Email == "") msg = "Looks like your session has been lost (log out perfomed/timeout occured).";
                //if (msg == "" && (session_changed || user_changed)) msg = "Looks like your session (" + (user_changed ? "User account" : "SessionID") + ") has been changed.";
                if (msg == "" && (session_changed || user_changed)) {
                    msg = "You are not logged in. Please log in and try again";
                    msg_reload = false;
                }
                if (msg == "" && wkg_changed && (res.wkg)) msg = "Changing your current workgroup to '" + res.wkg.name + "'.";
                if (msg == "" && prj_changed && (res.project)) msg = (res.project.ID <= 0 && curPrjID > 0 ? "Closing your active model." : "Changing your active model to '" + ShortString(res.project.Name, 35, true) + "'.");

                if (msg == "" && typeof res.msg != "undefined") msg = res.msg;

                if (msg != "") {
                    cancelPing(true);
                    showBackgroundMessage("", "fa fa-exchange-alt", false);
                    //window.focus();
                    var a = DevExpress.ui.dialog.alert(msg + (msg_reload ? "<br>Page will reload automatically." : ""), "Changes detected");
                    a.done(function () {
                        reloadPage();
                        return true;
                    });
                }

                if (typeof res.project != "undefined" && typeof curPrjData != "undefined") {
                    curPrjData = res.project;
                    pushProjectData(curPrjData);
                }
            }
        }
        if (typeof ping_on_reply == "function") ping_on_reply(res);
        return true;

    } else {

        ping_missed_counter++;
        if (ping_missed_counter > ping_missed_max) {
            cancelPing(true);
            showBackgroundMessage("No response from the web-server", "far fa-times-circle", false);
            //window.focus();
            var a = DevExpress.ui.dialog.alert("Can't get the valid response for the web server.<br>Please check your connection and reload the page.", "Connection issue");
            a.done(function () {
                reloadPage();
                return true;
            });
        }

        setTimeout(function () { showBackgroundMessage("Invalid response from the server", "fa fa-exclamation-triangle", false); }, 200);
        setTimeout(function () {
            var o = $("#divBackgroundProgress").find("i");
            if ((o) && (o.length) && o.attr("class").indexOf("exclamation-triangle") > 0) hideBackgroundMessage();
        }, 3000);

    }
    return false;
}

// ===== Projects =======

function getItemByID(lst, id) {
    if (typeof lst != "undefined" && (lst.length) && typeof lst[0].ID != "undefined") {
        for (var i = 0; i < lst.length; i++) {
            if (lst[i].ID == id) return lst[i];
        }
    }
    return null;
}

function isProjectsList() {
    return (typeof PrjDataSource != "undefined" && typeof projects != "undefined");
}

function pushProjectData(prj) {
    if (typeof PrjDataSource != "undefined" && (prj) && typeof (prj.ID != " undefined")) {
        PrjDataSource.push([{ type: "update", data: prj, key: prj.ID }]);
    } else {
        updateActiveProjectUIAttributes(prj);
    }
    if ((prj) && typeof prj != "undefined" && typeof prj.ID != "undefined" && curPrjID == prj.ID) {
        if (typeof curPrjData != "undefined") curPrjData = prj;
        if (typeof updateProjectActions == "function") updateProjectActions();
        if (typeof onChangeActiveProject == "function") onChangeActiveProject(prj);
    }
}

function updateActiveProjectUIAttributes(prj) {
    if ((prj) && typeof prj.ID != "undefined" && prj.ID == curPrjID && typeof prj.Name != "undefined") {
        $("#CurPrjName").attr("title", htmlEscape(prj.Name));   //.text(prj.Name);
        if (typeof prj.Starred != "undefined") {
            if (prj.Starred) {
                $("#curPrjStarred").addClass("fas  project-starred").removeClass("far");
            } else {
                $("#curPrjStarred").removeClass("fas  project-starred").addClass("far");
            }
        }
    }
}

function openProject(id, extra) {
    var args = (typeof extra == "undefined" ? {} : extra);
    args["id"] = id;
    if (curPrjID == id && url_on_open != "") {
        openProjectURL();
    } else {
        cancelPing(true);
        callAPI("project/?action=open", args, onOpenProject);
    }
}

function onOpenProject(res) {
    var err = "";
    hideLoadingPanel();
    if (isValidReply(res)) {
        closePopup();
        ping_active = true;
        startPing();
        if (res.Result == _res_Success) {
            var projectID_old = curPrjID;
            if (url_on_open == "" && res.URL != "") url_on_open = res.URL;
            if (res.Data == "confirm_online") {
                var result = DevExpress.ui.dialog.confirm("<div style='max-width:700px;'>" + res.Message + "</div>", resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        openProject(res.ObjectID, { "online": "ignore" });
                    }
                });
                return false;
            }
            if (res.ObjectID > 0) curPrjID = res.ObjectID;
            if (res.Data == "upgrade") {
                showResMessageCallFunc(res, false, projectUpgrade);
                return false;
            }
            if (res.Data == "teamtime") {
                var result = DevExpress.ui.dialog.confirm(resString("confOpenTeamTime"), resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        showTTWindow(curPrjID);
                        url_on_open = _url_tt_status;
                    }
                    openProjectURL();
                });
                return false;
            }
            if (res.Data == "join_teamtime") {
                if (open_tt_popup) showTTWindow(curPrjID);
                openProjectURL();
                open_tt_popup = false;
                return false;
            }
            open_tt_popup = false;
            if (res.Data == "confirm_teamtime") {
                curPrjID = projectID_old;
                if (url_on_open != "" && url_on_open.indexOf("project=") < 0) url_on_open += (url_on_open.indexOf("?") > 0 ? "&" : "?") + "project=" + res.ObjectID;
                var result = DevExpress.ui.dialog.confirm("<div style='max-width:700px;'>" + res.Message + "</div>", resString("titleConfirmation"));
                result.done(function (dialogResult) {
                    if (dialogResult) {
                        openProjectURL();
                    } else {
                        url_on_open = "";
                        url_on_open_blank = false;
                    }
                });
                return false;
            }
            if (res.Data == "antigua") {
                dxConfirm(resString("confOpenAntigua"),
                    function () {
                        url_on_open = _url_structuring;
                        openProjectURL();
                    },
                    function () {
                        openProjectURL();
                    }
                );
                return false;
            }
            if ((isRiskion) && (res.Data == "hid")) {
                moveProjectHID(res);
                return false;
            }
            // keep it last one:
            var is_replace = (typeof res.Data != "undefined" && res.Data != null && res.Data.length > 0 && res.Data[0] == "replace" && typeof res.Tag != "undefined");
            var func = (is_replace ? replaceProject : openProjectURL);
            if (typeof res.Tag == "object" && res.Tag != null && res.Tag !="" && typeof res.Tag.ID != "undefined" && typeof res.Tag.Name != "undefined") {
                var p = getItemByID(projects, res.Tag.ID);
                if ((p)) {
                    pushProjectData(res.Tag);
                } else {
                    projects.push(res.Tag);
                    PrjDataSource.push([{ type: "insert", data: res.Tag, key: res.Tag.ID }]);
                }
                DevExpress.ui.notify(replaceString("{0}", res.Tag.Name, resString("msgWTDecisionCopied")));
                func = null;
            }
            showResMessageCallFunc(res, false, func);
            return true;
        } else {
            if (res.Tag == "wrongmaster") {
                if (typeof restoreMaster == "function") res.Message += "<br>" + resString("lblRestoreMasterProjects");
            }
        }
    }
    showResMessage(res, true);
    return false;
}

function closeProject(id) {
    cancelPing(true);
    callAPI("project/?action=close", { "id": (id * 1) }, onCloseProject);
    if ((wnd_gecko)) wnd_gecko.close();
    showLoadingPanel(resString("lblClosingProject"));
}

function onCloseProject(res) {
    if (isValidReply(res) && res.Result == _res_Success) {
        showLoadingPanel(resString("lblPageReloading"), true);
        if (!(isProjectsList()) && typeof _pgid_projectslist != "undefined" && typeof navOpenPage == "function") {
            navOpenPage(_pgid_projectslist);
        } else {
            document.location.reload(true);
        }
    }
}

function projectUpgrade(res) {
    showLoadingPanel();
    setTimeout(function () { callAPI("project/?action=upgrade", null, onProjectUpgrade); }, 350);
    setTimeout(function () { showLoadingPanel(resString("msgUpdatingDecision")); }, 500);
    
}

function onProjectUpgrade(res) {
    var err = resString("msgCantUpgradeProject");
    hideLoadingPanel();
    if (isValidReply(res)) {
        if (res.Result == _res_Success) {
            if (url_on_open != "") {
                openProjectURL();
            } else {
                if (res.URL != "") loadURL(res.URL); else reloadPage();
            }
            return true;
        } else {
            if (res.Message != "") err = res.Message;
        }
    }
    showErrorMessage(err, true);
}

function setProjectOnline(prjid, is_online, func_success, func_unsuccess) {
    var args = { "id": prjid, "online": (is_online * 1 ? 1 : 0) };
    callAPI("project/?action=set_online", args, function (data) {
        if (typeof curPrjData != "undefined" && prjid == curPrjID && isValidReply(data) && (data.Result == _res_Success)) {
            curPrjData.isOnline = ((is_online) ? true : false);
            curPrjData.ComplexStatus = getComplexStatus(curPrjData);
            pushProjectData(curPrjData);
        }
        if (onSetProjectOnline(data, prjid, is_online)) {
            if (typeof func_success == "function") func_success(data, prjid, is_online);
        } else {
            if (typeof func_unsuccess == "function") func_unsuccess(data, prjid, !is_online);
        }
    });
}

function showErrorsFromTag(res) {
    if (isProjectsList() && res.Result != _res_Success && typeof res.Tag != "undefined") {
        lst = "";
        for (var i = 0; i < res.Tag.length; i++) {
            var p = getItemByID(projects, res.Tag[i].Key)
            if ((p)) {
                lst += "<li><b>" + ShortString(p.Name, 35, false) + "</b>: " + res.Tag[i].Value + "</li>";
            }
        }
        if (lst != "") lst = "<ul type=square>" + lst + "</ul>";
        if (lst == "" && res.Tag.length > 0) lst = res.Tag[0].Value;
        if (lst != "") return showErrorMessage(lst, true);
    }
    return showErrorMessage((typeof res.Message != "undefined" && res.Message != "" ? res.Message : "Unable to perform this action."), true);
}

function onGetAndReload(res) {
    if (isValidReply(res)) {
        if (res.Result == _res_Success) {
            reloadPage();
            return true;
        } else {
            var a = showErrorsFromTag(res);
            if ((a)) {
                a.done(function () { reloadPage(); });
            };
            if (res.Message != "") err = res.Message;
        }
        showResMessageCallFunc(res, true, function () { reloadPage(); });
    }
}

function onSetProjectOnline(res, prjid, is_online) {
    var err = "";
    hideLoadingPanel();
    if (isValidReply(res)) {
        if (isProjectsList() && typeof prjid != "undefined" && typeof is_online != "undefined") {    // update projects list datasource
            var p = getItemByID(projects, prjid)
            if ((p)) {
                p.isOnline = ((res.Result == _res_Success) ? is_online : !is_online);
                p.ComplexStatus = getComplexStatus(p);
                pushProjectData(p);
                $("#cbOnline" + prjid).prop("checked", p.isOnline);
            }
        }
        if (res.Result == _res_Success) {
            if (res.Message != "") showRes(res, false);
            return true;
        } else {
            if (showErrorsFromTag(res)) return false;
            if (res.Message != "") err = res.Message;
        }
    }
    showErrorMessage(err, true);
    return false;
}

function moveProjectHID(res) {
    var can_close = false;
    if (typeof res != "undefined") {
        var sList = "<div style='padding-left:2em'><input type='radio' name='dest' value='0' id='hl'><label for='hl' class='text'>" + resString("tpl_risk_likelihood_name") + "</label><br>";
        sList += "<input type='radio' name='dest' value='2' id='hi' checked='1'><label for='hi' class='text'>" + resString("tpl_risk_impact_name") + " (" + resString("lblDefault") + ")</label><br>";
        sList += "<input type='radio' name='dest' value='-1' id='hb'><label for='hb' class='text'>" + resString("lbl_rvBoth") + "</label></div>";
        $("#popupContainer").dxPopup({
            minWidth: 200,
            maxWidth: dlgMaxWidth(600),
            //width: "auto",
            minHeight: 150,
            maxHeight: dlgMaxHeight(600),
            //height: "auto",
            contentTemplate: function () {
                return $("<div style='padding:1ex 1em;'>" + (typeof res.Message == "undefined" || res.Message == "" ? resString("msgUploadedComparionModel2Riskion") : res.Message) + sList + "</div>");
            },
            showTitle: true,
            title: resString("titleConfirmation"),
            dragEnabled: true,
            shading: true,
            closeOnOutsideClick: false,
            resizeEnabled: true,
            showCloseButton: false,
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: resString("btnContinue"),
                    onClick: function (e) {
                        can_close = true;
                        var id = $("[name^=dest]:checked").val();
                        if (id != 2) {
                            doMoveProjectHID(res.ObjectID, id);
                        } else {
                            curPrjID = -1;
                            openProject(res.ObjectID);
                        }
                        closePopup();
                    }
                },
                toolbar: "bottom",
                location: 'after'
            }],
            onHiding: function (e) {
                if (!can_close) e.cancel = true;
            }
        });
        $("#popupContainer").dxPopup("show");
    } else {
        openProjectURL();
    }
}

function replaceProject(res) {
    if (typeof res.Tag == "undefined" || typeof projects == "undefined") {
        openProjectURL();
        return false;
    }
    var data = res.Tag;
    data.push(-1);
    var sList = "";
    var cnt = 0;
    for (var i = 0; i < data.length; i++) {
        var dt = "";
        var name = "";
        if (data[i] * 1 > 0) {
            var p = getItemByID(projects, data[i]);
            if ((p)) {
                if (p.AccessCode) dt = "<nobr>" + resString("lblPasscode") + ": " + p.AccessCode + "</nobr>";
                if (p.CreationDate != "") dt += (dt == "" ? "" : ", ") + "<nobr>" + resString("lblCreated") + ": " + p.CreationDate + "</nobr>";
                if (p.ModifyDate != "") dt += (dt == "" ? "" : ", ") + "<nobr>" + resString("lblModified") + ": " + p.ModifyDate + "</nobr>";
                if (dt != "") dt = "<div style='margin-left:18px;' class='text small gray'>(" + dt + ")</div>";
                name = resString("lblOverwrite") + " &laquo;<b>" + p.Name + "</b>&raquo;";
                cnt += 1;
            }
        } else {
            name = "<span style='color:green'>" + resString("lblReplaceModelKeepAsNew") + " &laquo;<b>" + res.Data[1] + "</b>&raquo;</span>";
        }
        sList += "<div style='margin:2px 0px'><nobr><input type='radio' name='replace_id' value='" + data[i] + "' id='replace_id_" + i + "'" + (sList == "" ? " checked" : "") + "><label for='replace_id_" + i + "' class='text'>" + name + "</label></nobr>" + dt + "</div>";
    }
    if (cnt > 0 && sList != "") {
        sList = "<div style='max-width:600px; padding:1ex 1em;'>" + replaceString("{0}", res.Data[1], resString("lblReplaceModelTitle")) + "<div style='margin-top:1em; max-height:15em; overflow:auto'>" + sList + "</div></div>";
        var conf_options = {
            "title": resString("titleConfirmation"),
            "messageHtml": sList,
            "buttons": [{
                text: resString("btnContinue"),
                onClick: function () {
                    var id = $("[id^=replace_id_]:checked").val();
                    if (id > 0) doReplaceProject(res.ObjectID, id);
                    //if (id == -1) openProjectURL();   // for call for upgrade if required
                    if (id == -1) {
                        curPrjID = -1;
                        openProject(res.ObjectID);
                    }
                }
            }, {
                text: resString("btnCancel"),
                onClick: function () {
                    url_on_open = "";
                    url_on_open_blank = false;
                    deleteProjects([res.ObjectID], true);
                }
            }]
        };
        var a = DevExpress.ui.dialog.custom(conf_options);
        a.show();
        setTimeout(function () {
            $('#replace_id_0').focus();
        }, 100);
    } else {
        openProjectURL();
    }
}

function doReplaceProject(new_id, old_id) {
    var args = { "src_id": new_id, "dest_id": old_id };
    //callAPI("project/?action=replace", args, openProjectURL);   // for call for upgrade if required
    callAPI("project/?action=replace", args, function (res) {
        curPrjID = -1;
        openProject(new_id);
    });
    showLoadingPanel(resString("lblReplaceModel"));
}

function doMoveProjectHID(prj_id, hid) {
    var args = { "prj_id": prj_id, "dest_id": hid * 1 };
    callAPI("project/?action=project_hid_move", args, function (res) {
        curPrjID = -1;
        openProject(prj_id);
    });
    showLoadingPanel(resString("lblUpdateModel"));
}

function deleteProjects(ids, status) {
    if ((ids)) {
        var args = { "ids": ids.join() };
        callAPI("project/?action=" + ((status) ? "delete" : "undelete"), args, ((ids.indexOf(curPrjID) >= 0) ? onGetAndReload : function (data) {
            onIterateProjectsList(data, status, function (prj) {
                prj.isMarkedAsDeleted = status;
                pushProjectData(prj);
            });
            if (typeof scanProjectsList == "function") scanProjectsList();
        }));
    }
}

function onIterateProjectsList(res, value, func) {
    if (isValidReply(res)) {
        if (res.Result == _res_Success) {
            if (typeof res.Data != "undefined") {
                if (isProjectsList()) {
                    if (hasGrid()) grid.clearSelection();
                    if (typeof projects != "undefined") {
                        for (var i = 0; i < res.Data.length; i++) {
                            var p = getItemByID(projects, res.Data[i]);
                            if ((p)) {
                                if (typeof func == "function") func(p, value);
                            }
                        }
                    } else {
                        reloadProjectsList();
                    }
                }
            }
        } else {
            if (showErrorsFromTag(res)) return false;
            if (res.Message != "") err = res.Message;
        }
    } else {
        showResMessage(res, true);
    }
}

//function onGetReplyAndReload(res) {
//    if (isValidReply(res) && res.Result == _res_Success) {
//        reloadPage();
//    } else { 
//        showResError();
//    }
//}


function wipeoutProjects(ids, callback_func) {
    if ((ids)) {
        callAPI("project/?action=wipeout", { "ids": ids.join() }, function (data) {
            onIterateProjectsList(data, true, function (prj) {
                if (typeof PrjDataSource != "undefined" && (prj)) {
                    PrjDataSource.push([{ type: "remove", key: prj.ID }]);
                }
            });
            if (typeof callback_func == "function") setTimeout(function () {
                callback_func(data);
            }, 100);
        });
    }
}

function askSetProjectOnlineAndAction(prjid, on_agree, on_disagree, on_cantupdate) {
    var dlg = DevExpress.ui.dialog.custom({
        title: resString("titleConfirmation"),
        messageHtml: "<div style='max-width:600px'>" + replaceString("\n", "<br>", resString("msgSetProjectStatusOnline")) + "</div>",
        buttons: [{
            text: resString("btnSetPrjOnline"),
            //elementAttr: { "class": "button_enter" },
            onClick: function (e) {
                if (typeof on_agree == "function") {
                    setProjectOnline(prjid, 1, function (data, prjid, is_online) {
                        on_agree(prjid, is_online);
                    }, function (data, prjid, is_online) {
                        if (typeof on_cantupdate == "function") on_cantupdate(prjid, is_online);
                    });
                }
            }
        }, {
            text: resString("btnLeavePrjOffline"),
            elementAttr: { "class": "button_esc" },
            onClick: function (e) {
                if (typeof on_disagree == "function") {
                    on_disagree(prjid, false);
                }
            }
        }]
    });
    dlg.show();
}

function openProjectURL() {
    if (url_on_open == "") {
        reloadProjectsList();
    } else {
        cancelPing(true);
        //var u_ = url_on_open.toLowerCase();
        //if ((url_on_open_blank) || (u_.indexOf("servicepage.aspx")>=0 && u_.indexOf("eval_"))) {
        //    wnd_gecko = CreatePopup(url_on_open, "gecko", ",", true);
        //    if ((wnd_gecko)) setTimeout(function () { wnd_gecko.focus(); }, 1000);
        //    if ($.isEmptyObject(curPrjData) || typeof curPrjData.ID == "undefined" || curPrjData.ID != curPrjID) reloadPage(0); else reloadProjectsList();
        //} else {
        //    loadURL(url_on_open, resString("lblOpeningModel"));
        //}
        openProjectURLWhenGecko(url_on_open);
    }
    url_on_open = "";
    //url_on_open_blank = false;
}

function openProjectURLWhenGecko(url) {
    var u_ = url.toLowerCase();
    if ((url_on_open_blank) || (u_.indexOf("servicepage.aspx") >= 0 && u_.indexOf("eval_"))) {
        wnd_gecko = CreatePopup(url, "gecko", ",", true);
        if ((wnd_gecko)) setTimeout(function () { wnd_gecko.focus(); }, 1000);
        if ($.isEmptyObject(curPrjData) || typeof curPrjData.ID == "undefined" || curPrjData.ID != curPrjID) reloadPage(0); else reloadProjectsList();
    } else {
        loadURL(url, resString("lblOpeningModel"));
    }
    url_on_open_blank = false;
}

function showTTWindow(prj_id) {
    tt_wnd = CreatePopup(_url_tt_pipe + "?temptheme=tt&project=" + prj_id, "TeamTime", "");
    if ((tt_wnd)) setTimeout(function () { tt_wnd.focus(); }, 1000);
}

function reloadProjectsList() {
    if (isProjectsList() && hasGrid()) {
        $("#grid").dxDataGrid("refresh");
    }
}

function getComplexStatus(prj) {
    if ((prj)) {
        return prj.LockStatus + '-' + prj.TeamTimeStatus + '-' + prj.OnlineUsers + '-' + (prj.isOnline ? 1 : 0);
    } else {
        return "";
    }
}

function setLockProject(id, lock) {
    callAPI("project/?action=set_lock", { "id": (id * 1), "lock": (lock) }, function (data) {
        onSetLockProject(data, lock);
    });
}

function onSetLockProject(res, lock) {
    if (isValidReply(res) && res.Result == _res_Success) {
        if (typeof projects != "undefined") {
            var p = getItemByID(projects, res.ObjectID)
            if ((p)) {
                p.LockStatus = (lock);
                if (res.Data != "undefined" && res.Data != null && typeof res.LockStatus != "undefined") p = res.Data;
                if (lock == 1) {
                    if (typeof sess_useremail != "undefined") p.LockedByUser = sess_useremail;
                    if (typeof sess_useridx != "undefined") p.LockedUserID = sess_useridx;
                }
                p.ComplexStatus = getComplexStatus(p);
                pushProjectData(p);
            }
        }
        if (res.ObjectID == curPrjID) {
            curPrjData.LockStatus = lock;
            if (lock == 1) {
                if (typeof sess_useremail != "undefined") curPrjData.LockedByUser = sess_useremail;
                if (typeof sess_useridx != "undefined") curPrjData.LockedByUser = sess_useridx;
            }
            curPrjData.ComplexStatus = getComplexStatus(curPrjData);
            pushProjectData(curPrjData);
        }
    } else {
        showResMessage(res, true);
    }
}

function onWhatsNew(full) {
    callAPI("service/?action=whatsnew", { "full": full }, function (res) {
        var do_next = (!full && typeof show_popups != "undefined" && (show_popups));
        if (isValidReply(res) && res.Result == _res_Success) {
            if (typeof res.Data != "undefined") {
                if (res.Data == "" && full) res.Data = "<p align=center>Nothing happened :)</p>";
                if (res.Data != "") {
                    showWhatsNew(res.Data, full);
                    do_next = false;
                }
            }
        }
        if (do_next) showPopupsOnLoad(3);
        return null;
    }, !(full));
}

function showWhatsNew(msg, full) {
    msg = "<div id='divPopupMsg' style='height:100%; overflow:auto;'>" + msg + "</div>";
    $("#popupContainer").dxPopup({
        minWidth: 300,
        width: (full ? "inherit" : "auto"),
        maxWidth: dlgMaxWidth(650),
        minHeight: 120,
        height: (full ? "inherit" : "auto"),
        maxHeight: dlgMaxHeight(600),
        contentTemplate: function () {
            return $(msg);
        },
        showTitle: true,
        title: "Recent changes",
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: true,
        resizeEnabled: true,
        showCloseButton: true,
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                text: resString("btnOK"),
                elementAttr: { "class": "button_esc button_enter" },
                onClick: function (e) {
                    closePopup();
                    if (!full && typeof show_popups != "undefined" && (show_popups)) showPopupsOnLoad(3);
                }
            },
            toolbar: "bottom",
            location: 'after'
        }]
    });

    $("#popupContainer").dxPopup("show");

    var targetlinks = $("#divPopupMsg").find('a[target="releasenotes"]').on("click", function (e) {
        var lnk = $(this).prop("href");
        if (typeof lnk != "undefined" && lnk != "" && lnk != "#") {
            CreatePopup(lnk, "releasenotes", "location=no,scrollbars=yes,resizable=yes", true);
            if ((e)) e.preventDefault();
        }
    });
}

function onKnownIssues(show_option) {
    callAPI("service/?action=knownissues", {}, function (res) {
        if (isValidReply(res) && res.Result == _res_Success) {
            if (typeof res.Data != "undefined") {
                if (res.Data == "") res.Data = "<h6 style='margin:auto auto'>No known issues</h6>";
                if (res.Data != "") {
                    //var msg = "<div style='min-width:220px; min-height: 80px; max-width:" + (dlgMaxWidth(650) - 30) + "px; max-height:" + (dlgMaxHeight(650) - 48) + "px; overflow:auto;'>" + res.Data + "</div>";
                    //return DevExpress.ui.dialog.alert(msg, resString("titleKnownIssues"));
                    showKnownIssues(res.Data, (show_option));
                }
            }
        }
        return null;
    }, (show_option));
    return false;
}

function showKnownIssues(msg, show_option) {
    msg = "<div style='overflow:auto; height:calc(100%);'>" + msg + "</div>";
    //msg = "<div style='overflow:auto; height:calc(100% - 42px);'>" + msg + "</div>";
    //msg += "<div style='margin-top:3ex; padding:4px; background:#f5faff;'><nobr><div id='cbShowKnownIssues'></div></nobr></div>";

    $("#popupContainer").dxPopup({
        minWidth: 300,
        //width: "auto",
        maxWidth: dlgMaxWidth(650),
        minHeight: 200,
        //height: "auto",
        maxHeight: dlgMaxHeight(550),
        contentTemplate: function () {
            return $(msg);
        },
        showTitle: true,
        title: resString("titleKnownIssues"),
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: true,
        resizeEnabled: true,
        showCloseButton: true,
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                text: resString("btnOK"),
                elementAttr: { "class": "button_enter button_esc" },
                onClick: function (e) {
                    closePopup();
                    if (show_option && typeof show_popups != "undefined" && (show_popups)) showPopupsOnLoad(4);
                }
            },
            toolbar: "bottom",
            location: 'after'
        }]
    });

    $("#popupContainer").dxPopup("show");
    //if ($("#cbShowKnownIssues").length) {
    //    var title = replaceString("{0}", resString("mnuKnownIssues"), resString("lblDontShowKnownIssues"));
    //    $("#cbShowKnownIssues").dxCheckBox({
    //        value: !show_issues,
    //        text: title,
    //        onValueChanged: function (e) {
    //            callAPI("account/?action=option", { "name": "show_issues", "value": !(e.value) }, hideLoadingPanel, resString("msgSaving"));
    //            show_issues = !(e.value);
    //        }
    //    });
    //    $("#cbShowKnownIssues").find(".dx-checkbox-text").html(title);
    //}
    //document.cookie = "show_issues=0;";
}


function onShowReleaseNotes() {
    var on_close = function (remember) {
        closePopup();
        if ((remember)) {
            var rn = resString("ReleaseNotesCookie");
            document.cookie = "ReleaseNotes=" + (rn == "" ? "1" : rn) + ";path=/;expires=Mon, 31-Dec-2029 23:59:59 GMT;";
            document.cookie = "ReleaseRemind=;";
        } else {
            document.cookie = "ReleaseNotes=;path=/;expires=Mon, 31-Dec-2029 23:59:59 GMT;";
            document.cookie = "ReleaseRemind=1;";
        }
        if (typeof show_popups != "undefined" && (show_popups)) showPopupsOnLoad(6);
    };

    $("#popupContainer").dxPopup({
        minWidth: 300,
        width: "auto",
        maxWidth: dlgMaxWidth(650),
        //minHeight: 200,
        height: "auto",
        //maxHeight: dlgMaxHeight(550),
        contentTemplate: function () {
            return resString("msgOpenReleaseNotes");
        },
        showTitle: true,
        title: resString("title_ReleaseNotes"),
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: false,
        resizeEnabled: true,
        showCloseButton: false,
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                text: resString("btnYes"),
                onClick: function (e) {
                    showReleaseNotes(false);
                    on_close(true);
                }
            },
            toolbar: "bottom",
            location: 'after'
        }, {
            widget: 'dxButton',
            options: {
                text: resString("btnNo"),
                onClick: function (e) {
                    on_close(true);
                }
            },
            toolbar: "bottom",
            location: 'after'
        }, {
            widget: 'dxButton',
            options: {
                text: resString("btnRemindMe"),
                onClick: function (e) {
                    on_close(false);
                }
            },
            toolbar: "bottom",
            location: 'after'
        }]
    });

    $("#popupContainer").dxPopup("show");
}

function showReleaseNotes(show_list) {
    var f = resString((show_list) ? (isRiskion ? "ReleaseNotesListRisk" : "ReleaseNotesList") : (isRiskion ? "ReleaseNotesFileRisk" : "ReleaseNotesFile"));
    if (f != "") {
        var rn = resString("ReleaseNotesCookie");
        document.cookie = "ReleaseNotes=" + (rn == "" ? "1" : rn) + ";path=/;expires=Mon, 31-Dec-2029 23:59:59 GMT;";
        var w = CreatePopup(f, "releasenotes", "location=no,scrollbars=yes,resizable=yes", true);
        if ((w)) setTimeout(function () { w.focus(); }, 500);
    }
    return false;
}

var reminder = null;
function onPswRemind() {
    reminder = $("#popupContainer").dxPopup({
        width: 360,
        height: "auto",
        title: resString("lblPswReminder"),
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                //icon: "far fa-clipboard", 
                elementAttr: { "id": "btnPswContinue", "class": "button_enter" },
                width: 70,
                text: resString("btnSend"),
                onClick: doSendPswReminder
            },
            toolbar: "bottom",
            location: 'after'
        }, {
            widget: 'dxButton',
            options: {
                //icon: "far fa-times-circle", 
                width: 70,
                text: resString("btnClose"),
                elementAttr: { "class": "button_esc" },
                onClick: function () {
                    closePopup();
                    reminder = null;
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }],
        contentTemplate: function () {
            return $("<div id='divPswReminder'><p align='justify' class='text'>" + resString("msgPasswordRestore") + "</p><div style='text-align:center'><span style='line-height:2.2; vertical-align:top;'>" + resString("lblEmail") + ": </span><div id='inpPREmail' style='display:inline-block; width:260px;'></div></div></div>");
        }
    }).dxPopup("instance");
    $("#popupContainer").dxPopup("show");
    var val = "";
    if ((theForm.Email)) val = theForm.Email.value;
    var inp = $("#PageContent_tbEmailR");
    if ((inp) && (inp.length)) val = inp.val();
    $("#inpPREmail").dxTextBox({ value: val });
    setTimeout("$('#inpPREmail').find('input').select().focus();", 500);
}

function doSendPswReminder() {
    var e = $('#inpPREmail').find('input').val();
    if (e.trim() == "") {
        DevExpress.ui.notify(resString("msgEmptyEmail"), "error");
        setTimeout("$('#inpPREmail').find('input').focus();", 500);
    } else {
        sendPswReminder(e);
    }
    return false;
}

function sendPswReminder(email) {
    callAPI("account/?action=passwordremind", { "email": email }, onSendPswReminder);
}

function onSendPswReminder(res) {
    hideLoadingPanel();
    if (isValidReply(res)) {
        if (res.Result == _res_Success) {
            closePopup();
            reminder = null;
            DevExpress.ui.dialog.alert(res.Message == "" ? resString("msgReminderOK") : res.Message, resString("titleInformation"));
        } else {
            showErrorMessage((res.Message == "" ? err : res.Message), true);
        }
    } else {
        showErrorMessage("Unable to perform this request", true);
    }
    return false;
}

var do_upload = 0;
function uploadDialog(title, isEnabled, uploadURL, onUploaded, onUploadStarted, autoOpenBrowse, allowed_ext, on_close) {
    if (typeof allowed_ext == "undefined" || allowed_ext == null) allowed_ext = [".ahps", ".ahp", ".ahpz", ".zip", ".rar", ".txt"];

    var hint = allowed_ext.join(", *");
    if (hint != "") hint = "*" + hint;
    var size = Math.round(maxUploadSize / 1024 / 102.4) / 10;
    hint = replaceString("{0}", hint, replaceString("{1}", size, resString("msgUploadCriteria")));

    $("#popupContainer").dxPopup({
        minHeight: 200,
        minWidth: 300,
        maxWidth: 500,
        height: "auto",
        title: title,
        //animation: {
        //    show: { "duration": 0 },
        //    hide: { "duration": 0 }
        //},
        toolbarItems: [{
            widget: 'dxButton',
            options: {
                elementAttr: { "id": "btnDoUpload", "class": "button_enter" },
                disabled: true,
                text: resString("btnUpload"),
                onClick: function (e) {
                    e.component.option("disabled", true);
                    //var b = $(".dx-fileuploader-content").children(".dx-fileuploader-upload-button:last");
                    var b = $(".dx-fileuploader-upload-button:first");
                    if ((b) && (b.length)) b.click();
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }, {
            widget: 'dxButton',
            options: {
                elementAttr: { "class": "button_esc" },
                text: resString("btnClose"),
                onClick: function () {
                    closePopup();
                    if (typeof on_close == "function") on_close();
                    return false;
                }
            },
            toolbar: "bottom",
            location: 'after'
        }],
        contentTemplate: function () {
            //return $("<form id='frmUpload' method='post' enctype='multipart/form-data'><div class='option_title'>" + resString("lblProjectName") + ":</nobr><div id='inpName'></div></div></div><div id='divUploader'>Uploader</div></form>");
            return $("<div id='divUploader' style='padding:4px 8px'></div><div class='text small' style='margin-top:1em'>" + hint + "</div>");
        }
    });
    $("#popupContainer").dxPopup("show");

    $("#divUploader").dxFileUploader({
        accept: "*",
        height: 80,
        maxHeight: 250,
        allowCanceling: true,
        name: "file",
        labelText: " " + resString("lblUploadDropFile"),
        selectButtonText: resString("btnUploadSelectFile"),
        readyToUploadMessage: resString("lblUploadReady"),
        uploadedMessage: resString("lblUploadSuccess"),
        uploadFailedMessage: resString("lblUploadFailed"),
        uploadButtonText: resString("btnUploadStart"),
        showFileList: true,
        disabled: !isEnabled,
        multiple: false,
        uploadMode: "useButtons",
        //width: "100%",
        uploadUrl: uploadURL,
        allowedFileExtensions: allowed_ext,
        maxFileSize: maxUploadSize,
        invalidMaxFileSizeMessage: resString("msgUploadFileLarge"),
        invalidFileExtensionMessage: resString("msgUploadFileWrongExt"),
        onValueChanged: function (e) {
            //$("#popupContainer").dxPopup("hide");
            $(".dx-fileuploader-upload-button").hide();
            var files = e.value.length;
            do_upload = files;
            //var total = e.component._totalSize;
            //$(".dx-fileuploader-input-wrapper").children(".dx-fileuploader-button").dxButton("option", "disabled", (files>=3));

            var has_upload = ($(".dx-fileuploader-upload-button").length > 1);
            $("#btnDoUpload").dxButton("option", "disabled", !has_upload);

            //var files = e.value;
            //if(files.length > 0) {
            //    $("#selected-files .selected-item").remove();
            //    $.each(files, function(i, file) {
            //        var $selectedItem = $("<div></div>").addClass("selected-item");
            //        $selectedItem.append(
            //            $("<span></span>").html("Name: " + file.name + "<br/>"),
            //            $("<span></span>").html("Size " + file.size + " bytes" + "<br/>"),
            //            $("<span></span>").html("Type " + file.type + "<br/>"),
            //            $("<span></span>").html("Last Modified Date: " + file.lastModifiedDate)
            //        );
            //        $selectedItem.appendTo($("#selected-files"));
            //    });
            //    $("#selected-files").show();
            //}

            //var url = e.component.option("uploadUrl");
            //url = updateQueryStringParameter(url, "id", employee.id);
            //e.component.option("uploadUrl", url);

            //$("#popupContainer").dxPopup("show");
        },
        onUploadStarted: function (e) {
            cancelPing(true);
            $("#btnDoUpload").dxButton("option", "disabled", true);
            if (typeof onUploadStarted == "function") onUploadStarted(e);
        },
        onProgress: function (e) {
            if (e.bytesLoaded >= e.bytesTotal * 0.9) {
                $("#btnCloseUpload").dxButton("option", "disabled", true);
                if (e.bytesLoaded >= e.bytesTotal * 0.97) {
                    $("#popupContainer").dxPopup("hide");
                }
                //showLoadingPanel(replaceString("%name%", ShortString(e.file.name, 35, false), resString("lblFileUploading"));
                showLoadingPanel(resString("lblFileUploading"));
                ping_active = false;
            }
        },
        onUploaded: function (e) {
            hideLoadingPanel();
            if (do_upload) do_upload--;
            if (!do_upload) closePopup();
            if ((e.request)) {
                if (e.request.readyState == 4) {
                    if (_call_success(e.request.response)) {
                        var data = parseReply(e.request.response);
                        if ($("#divUploader").data("dxFileUploader")) $("#divUploader").dxFileUploader("option", "showFileList", false);
                        //var do_ping = true;
                        if (typeof onUploaded == "function") {
                            onUploaded(data, e);
                        }
                        //if (do_ping) startPing();
                    }
                }
            }
        },
        onUploadError: function (e) {
            if (do_upload) do_upload--;
            hideLoadingPanel();
            _call_message("<div style='max-height:500px; max-width:700px; overflow:auto'><h5 class='error'><b>" + resString("lblError") + "</b>: " + e.request.statusText + "</h5><p><b>Details</b>: " + e.request.responseText + "</p></div>");
        }
    });
    centerPopup();

    //$("#inpName").dxTextBox({ value: "" });
    if (autoOpenBrowse === true) {
        setTimeout(function () {
            var btn = $(".dx-fileuploader-button");
            if ((btn) && (btn.length)) btn.dxButton("instance").element().click();
        }, 100);
    }
    //setTimeout("$('#inpName').find('input').select().focus();", 500);
}

function onDownloadComplete() {
    window.clearInterval(dl_timer);
    document.cookie = "dl_token='';";
    theForm.disabled = 0;
    hideLoadingPanel();
}

function doDownload(url, onDownloaded, pgID) {
    if (url != "") {
        showLoadingPanel(resString("msgPrepareDownload"));
        dl_token = curPrjID + "-" + new Date().getMilliseconds();
        document.location.href = url + "&t=" + dl_token;
        dl_timer = window.setInterval(function () {
            if (document.cookie.indexOf("dl_token=" + dl_token) > 0) {
                onDownloadComplete();
                if (typeof onDownloaded == "function") onDownloaded();
            }
        }, 350);
        if (typeof pgID != "undefined") {
            var pg = pageByID(nav_json, pgID);
            if ((pg) && typeof pg.extra != "undefined" && pg.extra.indexOf("new") >= 0) localStorage.setItem("new" + pgID, pgID);
        }
    }
    return false;
}

function downloadCombinedReport() {
    return doDownload('/api/pm/dashboard/?action=download_gembox_xlsx', null, 50130);
}

function downloadActiveModel() {
    if (curPrjID>0) {
        var result = DevExpress.ui.dialog.confirm(resString("confSaveSnapshots"), resString("titleConfirmation"));
        result.done(function (dialogResult) {
            doDownload(_url_download + "&id=" + curPrjID + "&type=ahps&snapshots=" + ((dialogResult) ? "yes" : "no"), function () {  });
        });
    }
    return false;
}


// ================= Help / KO ===============

function storeHelpPanelState() {
    if (!ko_widget2) {
        try {
            var w = $('#_ko16-widget-wrapper');
            if ((w) && (w.length)) {
                var ko_opened = (w.hasClass('_ko16-widget-open'));
            }
        }
        catch (e) {}
    }
    localStorage.setItem("ko_state", ((ko_opened) ? "true" : " false"));
}

function loadKOScript() {
    if (ko_script_added) return false;
    $("#help_icon").addClass("gray");
            
    var ko = document.createElement('script');
    ko.type = 'text/javascript';
    ko.async = true;

    if (ko_widget2) {
        _ko19.__pc = ko_prjid;
        _ko19.base_url = ko_base_url;
        ko.src = _ko19.base_url + 'widget/load';
    } else {
        _ko16_p.push(['_setProject', ko_prjid])
        _ko16_p.push(['_setCustomVar', 'cust_email', sess_useremail]);
        _ko16_p.push(['_setCustomVar', 'cust_name', sess_username]);
        _ko16_p.push(['_setCustomVar', 'reader_groups', (isRiskion ? 'riskion' : 'comparion')]);
        ko.src = ko_base_url + 'javascript/ko-index?__pc=' + ko_prjid;
    }
    document.head.appendChild(ko);
    if (ko_widget2) {
        _ko19.onOpen = function () {
            ko_opened = true;
            updateHelpPage(help_uri);
        }
        _ko19.onClose = function () {
            ko_opened = false;
        }
        _ko19.onLoad = function() {
            //_ko19.open();
            _ko19.loadKnowledge();
            _ko19.updateContact({name: sess_username, email: sess_useremail });
        }
        _ko19.onTabChange = function (data) {
            //if (data.oldTab == "recommended" && data.newTab == "landing" && help_uri !="") {
            //    _ko19.updateRecommended(help_uri);
            //}
            return false; 
        }
    } else {
    }
    ko.addEventListener('load', function(){
        initKOHelp(0);
        ko_script_loaded = true;
    });
    ko_script_added = true;
}

var initKOHelp = function(attempts) {
    setTimeout(function(){
        if ((!ko_widget2 && typeof __ko16 !== 'undefined') || (ko_widget2 && typeof _ko19 !== 'undefined')) {
            setTimeout(function () {
                $("#help_icon").removeClass("gray");
                if (ko_widget2) {
                    $("._ko19-widget-button").css("opacity", "0").css("right", "-100px").css("bottom", "-100px");
                } else {
                    $("#ko-btn-cntr").css("opacity", "0");
                }
                setTimeout(function () {
                    $("#ko-btn-cntr").hide();
                    var b = document.getElementById("_ko16_widget_close_cntr");
                    if ((b)) {
                        b.onclick = function () {
                            setTimeout(function () { 
                                if (!ko_init) storeHelpPanelState(); 
                            }, 500);
                        }
                    }
                    if (typeof ko_loaded_callback != "function") {
                        var o = localStorage.getItem("ko_state");
                        if ((o == "true")) onShowHelp(help_uri);
                    }
                    setTimeout( function () { 
                        ko_init = false; 
                        if (typeof ko_loaded_callback == "function") {
                            ko_loaded_callback();
                        }
                    }, 500);
                }, 550);
            }, 50);
        } else if(attempts <= help_load_attemps) {
            attempts++;
            initKOHelp(attempts);
        }
    }, 100);
}



// =========== RA/Risk Scenarios ==============

function activeScenarioUI(div, scenario_id, scenario_name, scenario_description) {
    $("#" + div).html("<u>" + scenario_name + "</u>").css("cursor", "pointer");
    $("#" + div).on("click", function () {
        selectScenarioUI(scenario_id);
    });
}

function setActiveScenario() {
    var items = $("#divScenariosList").dxList("instance").option("selectedItems");
    if (items.length > 0) {
        var sid = items[0].id;
        if (sid !== active_scenario_id) {
            showLoadingPanel();
            callAPI("pm/scenario/?action=set_active&id=" + sid, null, onSetActiveScenario);
        }
    }
}

function onSetActiveScenario() {
    reloadPage();
}

var active_scenario_id = 0;

function selectScenarioUI(scenario_id) {
    active_scenario_id = scenario_id;
    callAPI("pm/scenario/?action=list", null, onGetScenariosList);
}

function onGetScenariosList(res) {
    if ((res)) {
        var datasource = res;
        var h_max = $(window).height() - 140;
        var h = (datasource.length ? datasource.length : 1) * 28 + 150;
        if (h > h_max) h = h_max;
        var popupOptions = {
            width: ($(window).width() > 500 ? 450 : $(window).width() - 48),
            height: h,
            contentTemplate: function () {
                return $("<div id='divScenariosList'></div>");
            },
            toolbarItems: [{
                widget: 'dxButton',
                options: {
                    text: resString("btnAdd"),
                    onClick: function (e) {
                        addNewScenario();
                    }
                },
                toolbar: "bottom",
                location: 'center'
            }, {
                widget: 'dxButton',
                options: {
                    text: resString("btnDelete"),
                    disabled: true,
                    elementAttr: { id: 'btn_delete_scenario' },
                    onClick: function (e) {
                        var list = $("#divScenariosList").dxList("instance");
                        var selIDs = list.option("selectedItems");
                        if (selIDs.length > 0 && selIDs[0].id !== active_scenario_id && datasource.length > 1 && selIDs[0].id != 0) {
                            var sc_name = selIDs[0].name;
                            dxConfirm(resString("msgSureDeleteCommon2") + "\"" + sc_name + "\"?", function () {
                                callAPI("pm/scenario/?action=delete&id=" + selIDs[0].id, null, onGetScenariosList);
                                closePopup();
                            });
                        }
                    }
                },
                toolbar: "bottom",
                location: 'center'
            }, {
                widget: 'dxButton',
                options: {
                    text: resString("btnSelect"),
                    onClick: function (e) {
                        setActiveScenario();
                        closePopup();
                    }
                },
                toolbar: "bottom",
                location: 'center'
            },
            {
                widget: 'dxButton',
                options: {
                    text: resString("btnClose"),
                    onClick: function (e) {
                        closePopup();
                    }
                },
                toolbar: "bottom",
                location: 'center'
            }],
            showTitle: true,
            title: "Scenarios",
            dragEnabled: true,
            shading: true,
            closeOnOutsideClick: true,
            resizeEnabled: true,
            showCloseButton: true
        };

        $("#popupContainer").dxPopup(popupOptions).dxPopup("show");

        $("#divScenariosList").dxList({
            height: "100%",
            dataSource: datasource,
            searchEnabled: (datasource.length),
            pageLoadMode: "scrollBottom",
            keyExpr: "id",
            searchExpr: "name",
            selectionMode: "single",
            selectedItemKeys: [active_scenario_id],
            onSelectionChanged: function (e) {
                var ids = e.component.option("selectedItemKeys");
                $("#btn_delete_scenario").dxButton("instance").option("disabled", ids.length = 0 || ids.indexOf(active_scenario_id) > -1 || ids.indexOf(0) > -1 || datasource.length <= 1);
            },
            hoverStateEnabled: true,
            focusStateEnabled: true,
            itemTemplate: function (itemData, itemIndex, element) {
                var lnk = "<span>" + htmlEscape(itemData.name) + "</span>";
                element.append(
                    $('<i class="fas fa-scroll" style="margin-right: 1ex;"></i>'), $(lnk)
                );
            }
        });
    } else {
        DevExpress.ui.notify("Unable to load the list of scenarios", "error");
    }
}

function addNewScenario() {
    $("#popupContainer1").dxPopup({
        contentTemplate: function () {
            return $("<div id='tbNewScenarioName'></div><br><div id='tbNewScenarioDesc'></div>");
        },
        width: 500,
        height: 300,
        showTitle: true,
        title: "Create a new scenario",
        dragEnabled: true,
        shading: true,
        closeOnOutsideClick: true,
        resizeEnabled: false,
        showCloseButton: true,
        toolbarItems: [
            {
                toolbar: 'bottom',
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: true,
                options: {
                    text: resString("btnOK"),
                    icon: "fas fa-save",
                    elementAttr: { id: 'btn_add_scenario_ok' },
                    disabled: true,
                    onClick: function () {
                        var name = $("#tbNewScenarioName").dxTextBox("instance").option("value").trim();
                        var desc = $("#tbNewScenarioDesc").dxTextArea("instance").option("value").trim();
                        callAPI("pm/scenario/?action=add&name=" + name + "&desc=" + desc, null, onGetScenariosList);
                        closePopup();
                        closePopup("#popupContainer1");
                    }
                }
            }, {
                toolbar: 'bottom',
                location: 'after',
                locateInMenu: 'auto',
                widget: 'dxButton',
                visible: true,
                options: {
                    text: resString("btnCancel"),
                    icon: "fas fa-ban",
                    disabled: false,
                    onClick: function () {
                        closePopup("#popupContainer1");
                    }
                }
            }
        ]
    });

    $("#popupContainer1").dxPopup("show");

    $("#tbNewScenarioName").dxTextBox({
        placeholder: "Type a scenario name here...",
        onValueChanged: function (e) {
            var btn = $("#btn_add_scenario_ok").dxButton("instance");
            if ((btn)) btn.option("disabled", e.value.trim() == "");
        },
        onKeyDown: function (e) {
            setTimeout(function () {
                var btn = $("#btn_add_scenario_ok").dxButton("instance");
                if ((btn)) btn.option("disabled", $("#tbNewScenarioName").dxTextBox("instance").option("text").trim() == "");
            }, 100);
        }
    });
    $("#tbNewScenarioDesc").dxTextArea({
        placeholder: "Type a scenario description here...",
        height: 150
    });
}

//$(window).on('load', function () {
function initDXDefaults() {

    if (typeof DevExpress != "undefined") {

        // override dx default options:

        DevExpress.ui.dxDataGrid.defaultOptions({
            //device: { deviceType: "desktop" },
            options: {
                scrolling: {
                    useNative: true,
                },
                columnResizingMode: "widget",
            }
        });

        DevExpress.ui.dxList.defaultOptions({
            options: {
                useNativeScrolling: true,
            }
        });

        DevExpress.ui.dxScrollView.defaultOptions({
            options: {
                useNative: true,
            }
        });

        DevExpress.ui.dxTreeList.defaultOptions({
            options: {
                scrolling: {
                    useNative: true,
                },
            }
        });
    }
}
//});

$(window).on('beforeunload', function () {
    _ajax_error_ignore = true;
    cancelPing(true);
});
